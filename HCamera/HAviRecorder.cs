using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace HFromUI.HCamera
{
    using HFromUI.HLangage;
    /// <summary>
    /// AVI 视频录像机（基于 Windows 自带 avifil32.dll / Video for Windows，无第三方依赖）。
    /// 用法：SubmitFrame(Bitmap) 随时投递图像（内部只保留最新帧），后台线程按固定 FPS 写入 AVI；
    /// 投递频率高于录像帧率时自动丢帧，低于时自动重复上一帧（保证录像时长与真实时间一致）。
    /// 编码：默认 MJPG 压缩（Windows 自带 mjpg 压缩器，体积小），压缩器不可用时自动退化为无压缩 RGB24。
    /// 所有 P/Invoke 结构体按 x86/x64 兼容方式声明。
    /// </summary>
    public class HAviRecorder : IDisposable
    {
        #region ==================== 属性与字段 ====================

        /// <summary>录像文件完整路径（.avi）</summary>
        public string FilePath { get; private set; }
        /// <summary>视频宽度（像素）</summary>
        public int Width { get; private set; }
        /// <summary>视频高度（像素）</summary>
        public int Height { get; private set; }
        /// <summary>录像帧率（帧/秒）</summary>
        public int Fps { get; private set; }
        /// <summary>是否正在录像</summary>
        public bool IsRecording { get; private set; }
        /// <summary>已写入帧数</summary>
        public long FramesWritten { get; private set; }
        /// <summary>实际使用的编码说明（MJPG / 无压缩）</summary>
        public string CodecName { get; private set; }
        /// <summary>最近一次错误信息</summary>
        public string Error { get; private set; } = string.Empty;

        /// <summary>_aviFile 字段。</summary>
        private IntPtr _aviFile = IntPtr.Zero;
        /// <summary>_stream 字段。</summary>
        private IntPtr _stream = IntPtr.Zero;       // 压缩流（或原始流）
        /// <summary>_rawStream 字段。</summary>
        private IntPtr _rawStream = IntPtr.Zero;    // 未压缩源流（使用压缩器时）
        /// <summary>_vfwInited 字段。</summary>
        private bool _vfwInited = false;

        private Thread _writerThread;
        private volatile bool _running = false;
        /// <summary>_frameReady 字段。</summary>
        private readonly ManualResetEvent _frameReady = new ManualResetEvent(false);

        // 最新帧的像素缓冲（24bpp，按 DIB 底向上行序存储）
        private byte[] _buffer;
        // 写帧专用缓冲：写线程在锁内复制一份后锁外编码，避免与 SubmitFrame 并发导致花屏
        private byte[] _writeBuf;
        private int _stride;
        /// <summary>_bufLock 字段。</summary>
        private readonly object _bufLock = new object();
        /// <summary>_hasFrame 字段。</summary>
        private bool _hasFrame = false;
        /// <summary>_compressed 字段。</summary>
        private bool _compressed = false;

        // 复用的 24bpp 工作位图（SubmitFrame 时把任意位图统一画进来）
        private Bitmap _workBmp;
        private Graphics _workG;

        private const uint OF_WRITE = 0x1;
        private const uint OF_CREATE = 0x1000;
        private const uint AVICOMPRESSF_KEYFRAMES = 0x1;
        // mmioFOURCC('v','i','d','s') / ('v','i','d','c')
        private static uint FCC(string s) => (uint)(s[0] | s[1] << 8 | s[2] << 16 | s[3] << 24);

        #endregion

        #region ==================== 构造 / 开始 / 停止 ====================

        /// <summary>
        /// 创建并开始录像。
        /// </summary>
        /// <param name="filePath">avi 文件路径（目录自动创建）</param>
        /// <param name="width">视频宽</param>
        /// <param name="height">视频高</param>
        /// <param name="fps">录像帧率</param>
        /// <param name="fourcc">压缩器四字符码，默认 "MJPG"；传 "NONE" 表示无压缩</param>
        /// <param name="quality">压缩质量 0-100（MJPG），默认 75</param>
        public HAviRecorder(string filePath, int width, int height, int fps = 15, string fourcc = "MJPG", int quality = 75)
        {
            FilePath = filePath;
            Width = width;
            Height = height;
            Fps = fps < 1 ? 15 : fps;

            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _stride = ((width * 3 + 3) / 4) * 4; // 4 字节对齐
            _buffer = new byte[_stride * height];
            _writeBuf = new byte[_stride * height];
            _workBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _workG = Graphics.FromImage(_workBmp);

            try
            {
                AVIFileInit();
                _vfwInited = true;

                int hr = AVIFileOpenW(out _aviFile, filePath, OF_WRITE | OF_CREATE, IntPtr.Zero);
                if (hr != 0) throw new Exception(HTranslation.GetContent("AVIFileOpen 失败，错误码=") + hr);

                AVISTREAMINFO si = new AVISTREAMINFO
                {
                    fccType = FCC("vids"),
                    fccHandler = 0,
                    dwScale = 1,
                    dwRate = (uint)Fps,
                    dwStart = 0,
                    dwLength = 0,
                    dwSuggestedBufferSize = (uint)(_stride * height),
                    dwQuality = (uint)(quality * 100),
                    rcRight = width,
                    rcBottom = height,
                    szName = Path.GetFileNameWithoutExtension(filePath)
                };

                hr = AVIFileCreateStream(_aviFile, out _rawStream, ref si);
                if (hr != 0) throw new Exception(HTranslation.GetContent("AVIFileCreateStream 失败，错误码=") + hr);

                BITMAPINFOHEADER bmi = MakeBmi(width, height, _stride, 0, 24);

                bool useCompressor = !string.IsNullOrEmpty(fourcc) && fourcc.ToUpperInvariant() != "NONE";
                bool compressed = false;
                if (useCompressor)
                {
                    AVICOMPRESSOPTIONS opts = new AVICOMPRESSOPTIONS
                    {
                        fccType = FCC("vidc"),
                        fccHandler = FCC(fourcc),
                        dwKeyFrameEvery = 30,
                        dwQuality = (uint)(quality * 100),
                        dwBytesPerSecond = 0,
                        dwFlags = AVICOMPRESSF_KEYFRAMES,
                        lpFormat = IntPtr.Zero, cbFormat = 0,
                        lpParms = IntPtr.Zero, cbParms = 0,
                        dwInterleaveEvery = 0
                    };
                    hr = AVIMakeCompressedStream(out _stream, _rawStream, ref opts, IntPtr.Zero);
                    compressed = hr == 0;
                    if (compressed)
                    {
                        CodecName = fourcc.ToUpperInvariant();
                        _compressed = true;
                    }
                }

                if (!compressed)
                {
                    // 退化：直接写未压缩流
                    _stream = _rawStream;
                    CodecName = HTranslation.GetContent("无压缩RGB24");
                }

                // 设置输入帧格式（24bpp RGB）
                GCHandle h = GCHandle.Alloc(bmi, GCHandleType.Pinned);
                try
                {
                    hr = AVIStreamSetFormat(_stream, 0, h.AddrOfPinnedObject(), Marshal.SizeOf(typeof(BITMAPINFOHEADER)));
                    if (hr != 0) throw new Exception(HTranslation.GetContent("AVIStreamSetFormat 失败，错误码=") + hr);
                }
                finally { h.Free(); }

                _running = true;
                _writerThread = new Thread(WriterLoop) { IsBackground = true, Name = "HAviRecorder-" + Path.GetFileName(filePath) };
                _writerThread.Start();
                IsRecording = true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                CleanupAvi();
                throw;
            }
        }

        /// <summary>MakeBmi 方法。</summary>
        private static BITMAPINFOHEADER MakeBmi(int w, int h, int stride, uint compression, ushort bitCount)
        {
            return new BITMAPINFOHEADER
            {
                biSize = 40,
                biWidth = w,
                biHeight = h,
                biPlanes = 1,
                biBitCount = bitCount,
                biCompression = compression,
                biSizeImage = (uint)(stride * h),
                biXPelsPerMeter = 0,
                biYPelsPerMeter = 0,
                biClrUsed = 0,
                biClrImportant = 0
            };
        }

        /// <summary>
        /// 投递一帧图像（任意尺寸/格式；内部只保留最新帧，线程安全）。
        /// 建议在相机图像更新时调用，频率可高于录像帧率。
        /// </summary>
        public void SubmitFrame(Bitmap frame)
        {
            if (!_running || frame == null) return;
            try
            {
                lock (_bufLock)
                {
                    if (frame.Width != Width || frame.Height != Height)
                    {
                        // 尺寸不一致：缩放到录像尺寸
                        _workG.DrawImage(frame, 0, 0, Width, Height);
                    }
                    else if (frame.PixelFormat != PixelFormat.Format24bppRgb)
                    {
                        _workG.DrawImage(frame, 0, 0, Width, Height);
                    }
                    else
                    {
                        // 直接锁定位图复制；GDI+ 位图为顶向下行序，AVI DIB（正 biHeight）为底向上，
                        // 故源第 y 行写入缓冲的 (Height-1-y) 行，保证回放画面不上下颠倒
                        BitmapData bd = frame.LockBits(new Rectangle(0, 0, Width, Height),
                            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        try
                        {
                            int srcStride = bd.Stride;
                            IntPtr src = bd.Scan0;
                            for (int y = 0; y < Height; y++)
                            {
                                Marshal.Copy(IntPtr.Add(src, y * srcStride), _buffer, (Height - 1 - y) * _stride, Width * 3);
                            }
                        }
                        finally { frame.UnlockBits(bd); }
                        _hasFrame = true;
                        _frameReady.Set();
                        return;
                    }

                    // 经由 _workBmp（24bpp）复制，同样按底向上行序入缓冲
                    BitmapData wd = _workBmp.LockBits(new Rectangle(0, 0, Width, Height),
                        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    try
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            Marshal.Copy(IntPtr.Add(wd.Scan0, y * wd.Stride), _buffer, (Height - 1 - y) * _stride, Width * 3);
                        }
                    }
                    finally { _workBmp.UnlockBits(wd); }
                    _hasFrame = true;
                    _frameReady.Set();
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }

        /// <summary>停止录像并关闭文件（可重复调用）。</summary>
        public void Stop()
        {
            if (!_running && !IsRecording) return;
            _running = false;
            _frameReady.Set();
            try
            {
                if (_writerThread != null && _writerThread.IsAlive)
                    _writerThread.Join(3000);
            }
            catch { }
            CleanupAvi();
        }

        public void Dispose()
        {
            Stop();
            lock (_bufLock)
            {
                _workG?.Dispose();
                _workBmp?.Dispose();
                _workG = null;
                _workBmp = null;
            }
            _frameReady?.Dispose();
        }

        #endregion

        #region ==================== 写帧线程 ====================

        /// <summary>WriterLoop 方法。</summary>
        private void WriterLoop()
        {
            int interval = 1000 / Fps;
            long sample = 0;
            while (_running)
            {
                // 等待第一帧；之后按固定节拍写（无新帧则重复上一帧，保证时间轴连续）
                if (!_hasFrame)
                {
                    _frameReady.WaitOne(interval);
                    if (!_running) break;
                    if (!_hasFrame) continue;
                }
                else
                {
                    Thread.Sleep(interval);
                }

                // 锁内把最新帧复制到写缓冲，锁外再编码/写盘：
                // AVIStreamWrite（尤其 MJPG 压缩）耗时较长，若与 SubmitFrame 共用同一数组会读到半新半旧数据导致花屏
                lock (_bufLock)
                {
                    if (!_hasFrame) continue;
                    Buffer.BlockCopy(_buffer, 0, _writeBuf, 0, _buffer.Length);
                }

                try
                {
                    GCHandle h = GCHandle.Alloc(_writeBuf, GCHandleType.Pinned);
                    try
                    {
                        int hr = AVIStreamWrite(_stream, (int)sample, 1, h.AddrOfPinnedObject(),
                            (int)(_stride * Height), 0, IntPtr.Zero, IntPtr.Zero);
                        if (hr != 0)
                        {
                            Error = HTranslation.GetContent("AVIStreamWrite 失败，错误码=") + hr;
                        }
                        else
                        {
                            sample++;
                            FramesWritten = sample;
                        }
                    }
                    finally { h.Free(); }
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
            }
        }

        /// <summary>CleanupAvi 方法。</summary>
        private void CleanupAvi()
        {
            IsRecording = false;
            try { if (_stream != IntPtr.Zero) { AVIStreamRelease(_stream); } } catch { }
            try
            {
                // 仅压缩模式下源流与压缩流是两个指针，需分别释放；
                // 无压缩模式 _stream 与 _rawStream 是同一指针，绝不能重复 Release
                if (_compressed && _rawStream != IntPtr.Zero)
                {
                    AVIStreamRelease(_rawStream);
                }
            }
            catch { }
            _stream = IntPtr.Zero;
            _rawStream = IntPtr.Zero;
            _compressed = false;
            try { if (_aviFile != IntPtr.Zero) { AVIFileRelease(_aviFile); _aviFile = IntPtr.Zero; } } catch { }
            try { if (_vfwInited) { AVIFileExit(); _vfwInited = false; } } catch { }
        }

        #endregion

        #region ==================== VFW P/Invoke 与结构体 ====================

        [DllImport("avifil32.dll")] private static extern void AVIFileInit();
        [DllImport("avifil32.dll")] private static extern void AVIFileExit();
        [DllImport("avifil32.dll", CharSet = CharSet.Unicode)]
        private static extern int AVIFileOpenW(out IntPtr pAviFile, string szFile, uint uMode, IntPtr pclsidHandler);
        [DllImport("avifil32.dll")]
        private static extern int AVIFileCreateStream(IntPtr pAviFile, out IntPtr pAviStream, ref AVISTREAMINFO psi);
        [DllImport("avifil32.dll")]
        private static extern int AVIMakeCompressedStream(out IntPtr ppsCompressed, IntPtr psSourceStream,
            ref AVICOMPRESSOPTIONS lpOptions, IntPtr pclsidHandler);
        [DllImport("avifil32.dll")]
        private static extern int AVIStreamSetFormat(IntPtr paviStream, int lPos, IntPtr lpFormat, int cbFormat);
        [DllImport("avifil32.dll")]
        private static extern int AVIStreamWrite(IntPtr paviStream, int lStart, int lSamples,
            IntPtr lpBuffer, int cbBuffer, int dwFlags, IntPtr dummy1, IntPtr dummy2);
        [DllImport("avifil32.dll")]
        private static extern int AVIStreamRelease(IntPtr paviStream);
        [DllImport("avifil32.dll")]
        private static extern int AVIFileRelease(IntPtr pfile);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct AVISTREAMINFO
        {
            public uint fccType;
            public uint fccHandler;
            public uint dwFlags;
            public uint dwCaps;
            public ushort wPriority;
            public ushort wLanguage;
            public uint dwScale;
            public uint dwRate;
            public uint dwStart;
            public uint dwLength;
            public uint dwInitialFrames;
            public uint dwSuggestedBufferSize;
            public uint dwQuality;
            public uint dwSampleSize;
            public int rcLeft;
            public int rcTop;
            public int rcRight;
            public int rcBottom;
            public uint dwEditCount;
            public uint dwFormatChangeCount;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AVICOMPRESSOPTIONS
        {
            public uint fccType;
            public uint fccHandler;
            public uint dwKeyFrameEvery;
            public uint dwQuality;
            public uint dwBytesPerSecond;
            public uint dwFlags;
            public IntPtr lpFormat;
            public uint cbFormat;
            public IntPtr lpParms;
            public uint cbParms;
            public uint dwInterleaveEvery;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        #endregion
    }
}
