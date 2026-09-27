using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using HFromUI.HCamera.Dvp;
using HFromUI.HConvert;
using HFromUI.HControl;
namespace HFromUI.HCamera
{
    using HFromUI.HLangage;
    using HFromUI.HControl.VisualWindow;
    using HFromUI.HCamera.DaHuaNetSDK;
    /// <summary>相机品牌类型</summary>
    public enum HCameraKind
    {
        /// <summary>未配置</summary>
        None = 0,
        /// <summary>大华网络相机（dhnetsdk + dhplay 解码）</summary>
        Dahua = 1,
        /// <summary>度申工业相机（DVPCamera64 USB/GigE）</summary>
        Dvp = 2
    }
    /// <summary>相机连接参数（可序列化保存到 HAppData）</summary>
    public class HCameraConnParams
    {
        /// <summary>相机品牌</summary>
        public HCameraKind Kind = HCameraKind.None;
        /// <summary>通道标题（用于显示与录像目录名）</summary>
        public string Title = HTranslation.GetContent("通道");
        // ---- 大华网络相机 ----
        public string DahuaIP = "192.168.1.108";
        /// <summary>DahuaPort 成员。</summary>
        public ushort DahuaPort = 37777;
        /// <summary>DahuaUser 成员。</summary>
        public string DahuaUser = "admin";
        /// <summary>DahuaPassword 成员。</summary>
        public string DahuaPassword = "";
        /// <summary>DahuaChannel 成员。</summary>
        public int DahuaChannel = 0;
        // ---- 度申工业相机 ----
        public int DvpIndex = 0;
        // ---- 图像参数 ----
        /// <summary>曝光时间（微秒）</summary>
        public double ExposureUs = 5000;
        /// <summary>增益（0-100）</summary>
        public double Gain = 50;
    }
    /// <summary>
    /// 统一相机通道：封装大华/度申两类相机的“连接 → 取流 → 显示(HVisualWindow) → 录像(HAviRecorder)”完整链路。
    /// 线程模型（参考实际项目经验，避免出图滞后/卡顿）：
    /// 1) SDK 回调线程只做一件事：Marshal.Copy 复制最新原始帧（最新帧语义，不排队）；
    /// 2) 通道定时器按固定节拍（~25fps）取最新帧转换为 Bitmap，交给 HVisualWindow 显示与录像机；
    /// 3) 录像机内部单线程按录像 FPS 写 AVI，投递快于录像帧率自动丢帧、慢则重复上一帧。
    /// </summary>
    public class HCameraChannel : IDisposable
    {
        #region ==================== 对外属性/事件 ====================
        /// <summary>通道标题</summary>
        public string Title { get; private set; } = HTranslation.GetContent("通道");
        /// <summary>显示窗口（由界面赋值）</summary>
        public HVisualWindow Window;
        /// <summary>是否已连接并出流</summary>
        public bool IsConnected { get; private set; }
        /// <summary>是否正在录像</summary>
        public bool IsRecording { get; private set; }
        /// <summary>实测显示帧率</summary>
        public int Fps { get; private set; }
        /// <summary>当前分辨率文本</summary>
        public string Resolution { get; private set; } = "";
        /// <summary>相机品牌</summary>
        public HCameraKind Kind { get; private set; } = HCameraKind.None;
        /// <summary>最近错误</summary>
        public string Error { get; private set; } = "";
        /// <summary>录像分段时长（分钟），到时自动滚动新文件，默认 30 分钟</summary>
        public int RecordSegmentMinutes = 30;
        /// <summary>当前录像文件路径（无录像为空）</summary>
        public string CurrentRecordFile { get; private set; } = "";
        /// <summary>本次录像已录制时长（跨分段累计；未录像为 Zero）</summary>
        public TimeSpan RecordElapsed
        {
            get { return IsRecording ? DateTime.Now - _recordStartTime : TimeSpan.Zero; }
        }
        /// <summary>状态消息事件（SDK 线程/线程池触发，订阅者需自行切 UI 线程）</summary>
        public event EventHandler<string> Status;
        /// <summary>设备掉线事件（网络中断/相机被拔出；SDK 线程或线程池触发，订阅者需自行切 UI 线程）。
        /// 触发时通道已自动停止录像并清理，IsConnected=false，可据此做自动重连。</summary>
        public event EventHandler ConnectionLost;
        #endregion
        #region ==================== 内部字段 ====================
        private DahuaCamera _dh;
        private DvpCamera _dvp;
        private HCameraConnParams _params;
        // 最新原始帧（SDK 回调写、定时线程读）
        private byte[] _raw;
        private int _rawSize, _rawW, _rawH;
        private VideoFrameFormat _rawFmt;
        /// <summary>_rawLock 字段。</summary>
        private readonly object _rawLock = new object();
        private Timer _timer;
        private const int TickMs = 40; // 显示节拍 ~25fps
        private Bitmap _bmp;          // 复用显示位图（24bpp，仅定时线程写）
        /// <summary>_bmpLock 字段。</summary>
        private readonly object _bmpLock = new object(); // 保护 _bmp 的转换/投递/抓拍复制
        private int _uiPending;       // UI 刷新去重标志（0=可投递，1=已有刷新在队列）
        // 无帧看门狗：连接后超过阈值未收到任何帧（网线断/USB 拔出/SDK 回调丢失），判定设备掉线
        private DateTime _lastFrameTime = DateTime.Now;
        private bool _lostRaised;
        /// <summary>_lostLock 字段。</summary>
        private readonly object _lostLock = new object();
        private const double NoFrameTimeoutSec = 10.0;
        // 帧率统计
        private int _frames;
        /// <summary>_fpsTime 字段。</summary>
        private DateTime _fpsTime = DateTime.Now;
        // 录像
        private HAviRecorder _rec;
        /// <summary>_recLock 字段。</summary>
        private readonly object _recLock = new object(); // 保护录像启停与 _rec 生命周期（防与定时线程竞态产生孤儿录像机）
        private string _recDir;
        /// <summary>_recFps 字段。</summary>
        private int _recFps = 15;
        /// <summary>_recFourcc 字段。</summary>
        private string _recFourcc = "MJPG";
        private DateTime _segStart;
        private DateTime _recordStartTime; // 本次录像起点（跨分段累计时长用）
        #endregion
        #region ==================== 设备枚举 ====================
        /// <summary>枚举度申相机列表（返回 "索引: 友好名 (序列号)" 数组）；失败返回空数组。</summary>
        public static string[] ListDvpDevices()
        {
            try
            {
                DvpCamera.DVP_DEVICE_INFO[] infos = DvpCamera.EnumerateDevices();
                if (infos == null || infos.Length == 0) return new string[0];
                return infos.Select((i, idx) =>
                {
                    string name = !string.IsNullOrWhiteSpace(i.szFriendlyName) ? i.szFriendlyName
                        : (!string.IsNullOrWhiteSpace(i.szModelName) ? i.szModelName : HTranslation.GetContent("度申相机"));
                    string sn = i.szSerialNumber ?? "";
                    return idx + ": " + name + (string.IsNullOrWhiteSpace(sn) ? "" : " (" + sn.Trim() + ")");
                }).ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
        #endregion
        #region ==================== 连接 / 断开 ====================
        /// <summary>按参数连接相机并开始取流。</summary>
        public bool Connect(HCameraConnParams p)
        {
            Disconnect();
            _params = p ?? new HCameraConnParams();
            Title = string.IsNullOrWhiteSpace(_params.Title) ? HTranslation.GetContent("通道") : _params.Title;
            Kind = _params.Kind;
            Error = "";
            try
            {
                if (_params.Kind == HCameraKind.Dahua)
                {
                    _dh = new DahuaCamera();
                    _dh.RawFrameReceived += OnRawFrame;
                    _dh.Disconnected += OnCameraDisconnected;
                    if (!_dh.Login(_params.DahuaIP, _params.DahuaPort, _params.DahuaUser, _params.DahuaPassword))
                    {
                        Error = HTranslation.GetContent("大华登录失败，错误码=") + _dh.LastError + HTranslation.GetContent("（请检查 IP/端口/账号密码/网络）");
                        CleanupCamera();
                        return false;
                    }
                    // 原始帧预览：压缩码流经 dhplay.dll/play.dll 解码为 YV12 帧
                    if (!_dh.StartRawDataPreview(_params.DahuaChannel))
                    {
                        Error = HTranslation.GetContent("大华取流失败，错误码=") + _dh.LastError +
                                HTranslation.GetContent("。请确认 dhnetsdk.dll、dhplay.dll（或 play.dll）已放到 exe 同目录，且通道号正确");
                        CleanupCamera();
                        return false;
                    }
                }
                else if (_params.Kind == HCameraKind.Dvp)
                {
                    _dvp = new DvpCamera();
                    _dvp.FrameReceived += OnRawFrame;
                    _dvp.Disconnected += OnCameraDisconnected;
                    if (!_dvp.Open(_params.DvpIndex))
                    {
                        Error = HTranslation.GetContent("度申相机打开失败（索引=") + _params.DvpIndex +
                                HTranslation.GetContent("），请检查 USB/网线连接、驱动与 DVPCamera64.dll");
                        CleanupCamera();
                        return false;
                    }
                    _dvp.SetTriggerMode(0); // 0=连续采集
                    if (!_dvp.StartGrab())
                    {
                        Error = HTranslation.GetContent("度申相机开始采集失败");
                        CleanupCamera();
                        return false;
                    }
                }
                else
                {
                    Error = HTranslation.GetContent("未选择相机类型");
                    return false;
                }
                // 应用曝光/增益（失败不影响出流）
                SetExposure(_params.ExposureUs);
                SetGain(_params.Gain);
                IsConnected = true;
                _fpsTime = DateTime.Now;
                _lastFrameTime = DateTime.Now;
                _lostRaised = false;
                _frames = 0;
                _timer = new Timer(OnTick, null, 100, TickMs);
                RaiseStatus(Title + HTranslation.GetContent("：已连接") + (_params.Kind == HCameraKind.Dahua ? HTranslation.GetContent("（大华）") : HTranslation.GetContent("（度申）")));
                return true;
            }
            catch (DllNotFoundException ex)
            {
                // 厂商 SDK DLL 未部署：给出明确的中文部署提示（现场排查最常见问题）
                Error = HTranslation.GetContent("缺少相机 SDK 动态库（") + ex.Message + HTranslation.GetContent("）。请把厂商 DLL 放到 exe 同目录：") +
                        HTranslation.GetContent("大华需 dhnetsdk.dll + dhplay.dll（或 play.dll）；度申需 DVPCamera64.dll");
                CleanupCamera();
                IsConnected = false;
                return false;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                CleanupCamera();
                IsConnected = false;
                return false;
            }
        }
        /// <summary>断开相机（自动停止录像）。</summary>
        public void Disconnect()
        {
            IsConnected = false; // 先置位，防止 SDK 清理中回调 OnCameraDisconnected 误报掉线
            StopRecording();
            try { _timer?.Dispose(); } catch { }
            _timer = null;
            CleanupCamera();
            lock (_rawLock) { _rawSize = 0; }
        }
        /// <summary>SDK 掉线回调 / 无帧看门狗（SDK 线程或定时线程触发）：停录像、清理资源、通知上层重连。</summary>
        private void OnCameraDisconnected(object sender, EventArgs e)
        {
            bool wasRecording = false;
            lock (_lostLock)
            {
                if (!IsConnected) return; // 主动断开/重复回调忽略（SDK 回调与看门狗可能同时到达）
                IsConnected = false;
                _lostRaised = true;
                wasRecording = IsRecording;
                try { StopRecording(); } catch { }
                try { _timer?.Dispose(); } catch { }
                _timer = null;
                CleanupCamera();
                lock (_rawLock) { _rawSize = 0; }
            }
            RaiseStatus(Title + HTranslation.GetContent("：设备连接断开！") + (wasRecording ? HTranslation.GetContent("（录像已停止）") : ""));
            try { ConnectionLost?.Invoke(this, EventArgs.Empty); } catch { }
        }
        /// <summary>CleanupCamera 方法。</summary>
        private void CleanupCamera()
        {
            try { _dh?.Dispose(); } catch { }
            try { _dvp?.Dispose(); } catch { }
            _dh = null;
            _dvp = null;
        }
        public void Dispose()
        {
            Disconnect();
            lock (_bmpLock)
            {
                try { _bmp?.Dispose(); } catch { }
                _bmp = null;
            }
        }
        #endregion
        #region ==================== 曝光 / 增益 ====================
        /// <summary>设置曝光时间（微秒）；未连接时只记录参数，连接后自动应用。</summary>
        public bool SetExposure(double exposureUs)
        {
            if (_params != null) _params.ExposureUs = exposureUs;
            try
            {
                if (_dh != null && _dh.IsLoggedIn)
                    return _dh.SetExposureTime((uint)Math.Max(0, exposureUs));
                if (_dvp != null && _dvp.IsOpen)
                    return _dvp.SetExposure(exposureUs);
            }
            catch (Exception ex) { Error = ex.Message; }
            return false;
        }
        /// <summary>设置增益（0-100）；未连接时只记录参数，连接后自动应用。</summary>
        public bool SetGain(double gain)
        {
            if (_params != null) _params.Gain = gain;
            try
            {
                if (_dh != null && _dh.IsLoggedIn)
                    return _dh.SetGainValue((uint)Math.Max(0, gain));
                if (_dvp != null && _dvp.IsOpen)
                    return _dvp.SetGain(gain);
            }
            catch (Exception ex) { Error = ex.Message; }
            return false;
        }
        #endregion
        #region ==================== 帧链路：SDK 回调 → 定时转换 → 显示/录像 ====================
        // SDK 回调线程：只复制最新帧
        private void OnRawFrame(IntPtr dataPtr, int dataSize, int width, int height, VideoFrameFormat format)
        {
            if (dataPtr == IntPtr.Zero || dataSize <= 0 || width <= 0 || height <= 0) return;
            if (dataSize < FrameByteSize(width, height, format)) return;
            _lastFrameTime = DateTime.Now; // 看门狗喂狗
            lock (_rawLock)
            {
                if (_raw == null || _raw.Length < dataSize) _raw = new byte[dataSize * 2];
                Marshal.Copy(dataPtr, _raw, 0, dataSize);
                _rawSize = dataSize;
                _rawW = width;
                _rawH = height;
                _rawFmt = format;
            }
        }
        // 定时线程：看门狗 → 取最新帧 → Bitmap → 显示 + 录像
        private void OnTick(object state)
        {
            // 无帧看门狗：定时器不依赖相机回调，网线断开/USB 拔出/SDK 回调丢失都能发现
            if (IsConnected && !_lostRaised &&
                (DateTime.Now - _lastFrameTime).TotalSeconds > NoFrameTimeoutSec)
            {
                OnCameraDisconnected(this, EventArgs.Empty);
                return;
            }
            try
            {
                byte[] data;
                int size, w, h;
                VideoFrameFormat fmt;
                lock (_rawLock)
                {
                    if (_raw == null || _rawSize == 0) return;
                    data = _raw; size = _rawSize; w = _rawW; h = _rawH; fmt = _rawFmt;
                }
                if (size < FrameByteSize(w, h, fmt)) return;
                lock (_bmpLock)
                {
                    if (_bmp == null || _bmp.Width != w || _bmp.Height != h)
                    {
                        try { _bmp?.Dispose(); } catch { }
                        _bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                        Resolution = w + "x" + h;
                    }
                    GCHandle pin = GCHandle.Alloc(data, GCHandleType.Pinned);
                    try
                    {
                        try
                        {
                            // 快速路径：YV12 直接写入复用位图（零分配）
                            HFrameToBitmapConverter.ConvertToBitmap(pin.AddrOfPinnedObject(), w, h, fmt, _bmp);
                        }
                        catch (NotSupportedException)
                        {
                            // 其它像素格式（BGR/Gray/Bayer 等）走全量转换后贴入复用位图
                            using (Bitmap tmp = HFrameToBitmapConverter.Convert(pin.AddrOfPinnedObject(), w, h, fmt))
                            using (Graphics g = Graphics.FromImage(_bmp))
                            {
                                g.DrawImage(tmp, 0, 0, w, h);
                            }
                        }
                    }
                    finally { pin.Free(); }
                    // 显示：只投递最新帧（Interlocked 去重，UI 繁忙时中间帧自动丢弃）。
                    // UI 线程在同一把 _bmpLock 内把 _bmp 画入窗口自持位图——
                    // 绝不直接把 _bmp 交给 UI（定时线程写像素与 UI 绘制并发会引发 GDI+ 异常/花屏）
                    if (Window != null && Window.IsHandleCreated &&
                        System.Threading.Interlocked.Exchange(ref _uiPending, 1) == 0)
                    {
                        try
                        {
                            Window.BeginInvoke(new Action(() =>
                            {
                                System.Threading.Interlocked.Exchange(ref _uiPending, 0);
                                try
                                {
                                    Bitmap wb = Window.Image;
                                    if (wb == null || wb.Width != w || wb.Height != h)
                                    {
                                        wb = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                                        Window.Image = wb; // 旧位图由 HVisualWindow.Image 释放
                                    }
                                    lock (_bmpLock)
                                    {
                                        using (Graphics g = Graphics.FromImage(wb))
                                        {
                                            g.DrawImage(_bmp, 0, 0, w, h);
                                        }
                                    }
                                }
                                catch { }
                            }));
                        }
                        catch { }
                    }
                    // 帧率统计
                    _frames++;
                    DateTime now = DateTime.Now;
                    double secs = (now - _fpsTime).TotalSeconds;
                    if (secs >= 1.0)
                    {
                        Fps = (int)(_frames / secs);
                        _frames = 0;
                        _fpsTime = now;
                    }
                    // 录像：首帧到达后建文件；分段到时或分辨率变化（缩放会变形）滚动新文件。
                    // 加锁并二次确认状态：防止与 StopRecording/Disconnect 竞态，
                    // 产生“孤儿录像机”（写线程永不退出、AVI 不封口导致文件损坏+句柄泄漏）
                    lock (_recLock)
                    {
                        if (IsRecording && IsConnected)
                        {
                            if (_rec == null) StartRecorderFile(w, h);
                            else if ((now - _segStart).TotalMinutes >= RecordSegmentMinutes
                                     || _rec.Width != w || _rec.Height != h)
                            {
                                StopRecorderFile();
                                StartRecorderFile(w, h);
                            }
                            try { _rec?.SubmitFrame(_bmp); } catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
        /// <summary>FrameByteSize 方法。</summary>
        private static int FrameByteSize(int width, int height, VideoFrameFormat format)
        {
            switch (format)
            {
                case VideoFrameFormat.BGR24:
                case VideoFrameFormat.RGB24:
                    return width * height * 3;
                case VideoFrameFormat.YUYV:
                case VideoFrameFormat.UYVY:
                case VideoFrameFormat.YVYU:
                case VideoFrameFormat.VYUY:
                    return width * height * 2;
                case VideoFrameFormat.Gray8:
                    return width * height;
                default: // YV12/I420/NV12/NV21 等 4:2:0
                    return width * height * 3 / 2;
            }
        }
        #endregion
        #region ==================== 录像 ====================
        /// <summary>
        /// 开始录像（首帧到达后自动创建 avi 文件；文件按 通道名/日期/时间.avi 组织，自动分段）。
        /// </summary>
        /// <param name="dir">录像根目录</param>
        /// <param name="fps">录像帧率</param>
        /// <param name="fourcc">编码："MJPG"（默认压缩）或 "NONE"（无压缩）</param>
        public bool StartRecording(string dir, int fps, string fourcc = "MJPG")
        {
            if (!IsConnected)
            {
                Error = HTranslation.GetContent("请先连接相机再录像");
                return false;
            }
            lock (_recLock)
            {
                _recDir = dir;
                _recFps = fps < 1 ? 15 : fps;
                _recFourcc = string.IsNullOrEmpty(fourcc) ? "MJPG" : fourcc;
                IsRecording = true;
                _recordStartTime = DateTime.Now;
            }
            RaiseStatus(Title + HTranslation.GetContent("：开始录像（等待首帧）..."));
            return true;
        }
        /// <summary>
        /// 抓拍当前画面为 JPG：保存到 rootDir\Snapshots\通道名\日期\时间.jpg。
        /// 线程安全（复制当前显示位图）；成功返回 true 并输出完整路径。
        /// </summary>
        public bool Snapshot(string rootDir, out string path)
        {
            path = "";
            if (!IsConnected) { Error = HTranslation.GetContent("相机未连接，无画面可抓拍"); return false; }
            Bitmap copy;
            lock (_bmpLock)
            {
                if (_bmp == null) { Error = HTranslation.GetContent("尚无画面帧，稍后再试"); return false; }
                try { copy = new Bitmap(_bmp); } // 复制一份，避免与取帧线程冲突
                catch (Exception ex) { Error = HTranslation.GetContent("抓拍失败：") + ex.Message; return false; }
            }
            try
            {
                string folder = Path.Combine(rootDir, "Snapshots", SafeFileName(Title), DateTime.Now.ToString("yyyyMMdd"));
                Directory.CreateDirectory(folder);
                path = Path.Combine(folder, DateTime.Now.ToString("HHmmssfff") + ".jpg");
                SaveJpeg(copy, path, 92L);
                RaiseStatus(Title + HTranslation.GetContent("：抓拍已保存 ") + Path.GetFileName(path));
                return true;
            }
            catch (Exception ex)
            {
                Error = HTranslation.GetContent("抓拍保存失败：") + ex.Message;
                path = "";
                return false;
            }
            finally { try { copy.Dispose(); } catch { } }
        }
        /// <summary>SaveJpeg 方法。</summary>
        private static void SaveJpeg(Bitmap bmp, string path, long quality)
        {
            ImageCodecInfo enc = ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
            if (enc == null) { bmp.Save(path, ImageFormat.Jpeg); return; }
            using (EncoderParameters ps = new EncoderParameters(1))
            {
                ps.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                bmp.Save(path, enc, ps);
            }
        }
        /// <summary>停止录像并关闭当前分段文件。</summary>
        public void StopRecording()
        {
            lock (_recLock)
            {
                IsRecording = false;
                StopRecorderFile();
            }
        }
        /// <summary>StartRecorderFile 方法。</summary>
        private void StartRecorderFile(int w, int h)
        {
            try
            {
                string folder = Path.Combine(_recDir, SafeFileName(Title));
                string path = Path.Combine(folder,
                    DateTime.Now.ToString("yyyyMMdd"),
                    DateTime.Now.ToString("HHmmss") + ".avi");
                _rec = new HAviRecorder(path, w, h, _recFps, _recFourcc);
                _segStart = DateTime.Now;
                CurrentRecordFile = path;
                RaiseStatus(Title + HTranslation.GetContent("：录像中 ") + Path.GetFileName(path) + " [" + _rec.CodecName + " " + w + "x" + h + "@" + _recFps + "fps]");
            }
            catch (Exception ex)
            {
                Error = HTranslation.GetContent("录像启动失败：") + ex.Message;
                RaiseStatus(Title + "：" + Error);
                IsRecording = false;
                _rec = null;
                CurrentRecordFile = "";
            }
        }
        /// <summary>StopRecorderFile 方法。</summary>
        private void StopRecorderFile()
        {
            if (_rec != null)
            {
                long frames = _rec.FramesWritten;
                string file = CurrentRecordFile;
                try { _rec.Stop(); _rec.Dispose(); } catch { }
                _rec = null;
                CurrentRecordFile = "";
                if (frames > 0) RaiseStatus(Title + HTranslation.GetContent("：录像已保存 ") + Path.GetFileName(file) + "（" + frames + HTranslation.GetContent(" 帧）"));
            }
        }
        /// <summary>SafeFileName 方法。</summary>
        private static string SafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Channel";
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
        #endregion
        /// <summary>RaiseStatus 方法。</summary>
        private void RaiseStatus(string msg)
        {
            try { Status?.Invoke(this, msg); } catch { }
        }
    }
}
