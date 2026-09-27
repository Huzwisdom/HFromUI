using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HFrom;
using HFromUI.HControl;
namespace HFromUI.HCamera
{
    using HFromUI.HLangage;
    using HFromUI.HFrom.HUiKit;
    using HFromUI.HFrom.Panel;
    using HFromUI.HFrom.Text;
    using HFromUI.HControl.Tools.Button;
    using HFromUI.HControl.Tools.Bars;
    /// <summary>
    /// 本地视频播放器控件（纯 WinForms）：优先使用 VLC(libvlc) 引擎，支持 VLC 能播放的
    /// 全部格式（mkv/flv/ts/m2ts/vob/rmvb/3gp/webm/mp4/avi/wmv… 及 RTSP/HTTP 网络流）；
    /// 本机未安装 VLC 时自动降级为 DirectShow（quartz.dll 系统组件，无 WPF/WMP 依赖）。
    /// 视频由渲染器硬件加速直接绘制到画面窗口，支持播放/暂停/停止、进度条拖动定位、时间显示；
    /// 不支持的文件格式提示而不崩溃。与 HAviRecorder/HRecordCleaner 同属录像回放模块。
    /// </summary>
    public class HVideoPlayer : UserControl
    {
        /// <summary>打开文件对话框过滤串（覆盖 VLC 支持的常见视频格式）。</summary>
        public static readonly string VideoFileFilter =
            HTranslation.GetContent("视频文件(*.avi;*.wmv;*.asf;*.mpg;*.mpeg;*.mpe;*.m2v;*.mp4;*.m4v;*.f4v;*.mov;*.mkv;*.flv;*.ts;*.m2ts;*.mts;*.vob;*.dat;*.rm;*.rmvb;*.3gp;*.3g2;*.webm;*.ogv;*.ogm;*.divx;*.dv;*.iso)|") +
            "*.avi;*.wmv;*.asf;*.mpg;*.mpeg;*.mpe;*.m2v;*.mp4;*.m4v;*.f4v;*.mov;*.mkv;*.flv;*.ts;*.m2ts;*.mts;*.vob;*.dat;*.rm;*.rmvb;*.3gp;*.3g2;*.webm;*.ogv;*.ogm;*.divx;*.dv;*.iso|" +
            HTranslation.GetContent("所有文件(*.*)|*.*");
        /// <summary>开始播放（Open 成功并 Run）。</summary>
        public event EventHandler PlaybackStarted;
        /// <summary>停止播放。</summary>
        public event EventHandler PlaybackStopped;
        /// <summary>播放到结尾。</summary>
        public event EventHandler PlaybackEnded;
        /// <summary>打开/播放出错。</summary>
        public event EventHandler PlaybackError;
        private const int WS_CHILD = 0x40000000;
        private const int WS_CLIPSIBLINGS = 0x04000000;
        private const int WS_CLIPCHILDREN = 0x02000000;
        private const int OATRUE = -1;
        private const int OAFALSE = 0;
        // DirectShow FilterState
        private const int State_Stopped = 0;
        private const int State_Paused = 1;
        private const int State_Running = 2;
        private readonly Panel _videoHost;
        private readonly HLabel _lblHint;
        private readonly HLabel _lblFile;
        private readonly HLabel _lblTime;
        private readonly HPushButton _btnPlay;
        private readonly HPushButton _btnStop;
        private readonly HTrackBar _track;
        private readonly System.Windows.Forms.Timer _timer;
        private object _graph;
        private Ds.IMediaControl _mc;
        private Ds.IVideoWindow _vw;
        private Ds.IMediaPosition _mp;
        /// <summary>_fileName 字段。</summary>
        private string _fileName = "";
        private bool _endNotified;
        /// <summary>_lastState 字段。</summary>
        private int _lastState = State_Stopped;
        // ---------- VLC(libvlc) 引擎字段（优先使用，缺失时降级 DirectShow） ----------
        private bool _useVlc;                 // 当前是否走 VLC 引擎
        private IntPtr _vlcPlayer;            // libvlc_media_player_t*
        private IntPtr _vlcMedia;             // libvlc_media_t*
        private long _vlcLengthMs;            // 缓存的总时长（毫秒，-1 未知）
        private volatile bool _vlcEnded;      // 播放到结尾（VLC 回调线程置位）
        private volatile bool _vlcError;      // 播放出错（VLC 回调线程置位）
        private Vlc.VlcEventCallback _vlcCb;  // 事件回调委托（必须保持引用防 GC）
        public HVideoPlayer()
        {
            BackColor = Color.Black;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            // 视频画面宿主（DirectShow 渲染窗口作为它的子窗口）
            _videoHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                TabStop = false
            };
            _videoHost.Resize += (s, e) => PositionVideo();
            _videoHost.DoubleClick += (s, e) => TogglePlay();
            _lblHint = new HLabel
            {
                Dock = DockStyle.Fill,
                Text = HTranslation.GetContent("本地视频播放\r\n右键画面框选择“打开本地视频…”，或在录像回放页选择文件"),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = HUiTheme.TextSub,
                BackColor = Color.Transparent,
                Font = HUiTheme.FontUi
            };
            _videoHost.Controls.Add(_lblHint);
            // ---------- 底部控制条 ----------
            HPanel bar = new HPanel { Dock = DockStyle.Bottom, Height = 46, BackColor = HUiTheme.PanelBg };
            HUiTheme.StylePanel(bar, HUiTheme.PanelBg, 0);
            bar.ShowBorder = false;
            bar.RoundStyle = HRoundStyle.None;
            _lblFile = new HLabel
            {
                Dock = DockStyle.Top,
                Height = 20,
                Text = "",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                ForeColor = HUiTheme.TextSub,
                BackColor = Color.Transparent,
                Font = HUiTheme.FontSmall,
                Padding = new Padding(8, 2, 8, 0)
            };
            HPanel row = new HPanel { Dock = DockStyle.Fill, BackColor = HUiTheme.PanelBg };
            HUiTheme.StylePanel(row, HUiTheme.PanelBg, 0);
            row.ShowBorder = false;
            row.RoundStyle = HRoundStyle.None;
            _btnPlay = new HPushButton { Dock = DockStyle.Left, Width = 42, Glyph = HPushButtonGlyph.Play, ImageSize = 16 };
            HUiTheme.StyleAccent(_btnPlay);
            _btnPlay.Click += (s, e) => TogglePlay();
            _btnStop = new HPushButton { Dock = DockStyle.Left, Width = 42, Left = 44, Glyph = HPushButtonGlyph.Stop, ImageSize = 14 };
            HUiTheme.StyleGhost(_btnStop);
            _btnStop.Click += (s, e) => Stop();
            _lblTime = new HLabel
            {
                Dock = DockStyle.Right,
                Width = 118,
                Text = "00:00 / 00:00",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                ForeColor = HUiTheme.TextSub,
                BackColor = Color.Transparent,
                Font = HUiTheme.FontSmall
            };
            _track = new HTrackBar
            {
                Dock = DockStyle.Fill,
                MinValue = 0,
                MaxValue = 1000,
                Value = 0,
                IsShowTips = false,
                ShowButton = true,
                LineWidth = 4,
                BackColor = HUiTheme.PanelBg
            };
            _track.LineColor = HUiTheme.Border;
            _track.ValueColor = HUiTheme.Accent;
            _track.EllipseColor = HUiTheme.Accent;
            _track.EllipseBorderColor = Color.White;
            _track.ManualValueChanged += (s, e) => SeekToTrack();
            row.Controls.Add(_track);   // Fill 先加
            row.Controls.Add(_lblTime);
            row.Controls.Add(_btnStop);
            row.Controls.Add(_btnPlay);
            bar.Controls.Add(row);       // Fill 先加
            bar.Controls.Add(_lblFile);  // Top 后加
            Controls.Add(_videoHost);
            Controls.Add(bar);
            _timer = new System.Windows.Forms.Timer { Interval = 250 };
            _timer.Tick += (s, e) => TickRefresh();
        }
        /// <summary>当前打开的文件完整路径（无则空串）。</summary>
        public string FileName { get { return _fileName; } }
        /// <summary>是否正在播放。</summary>
        public bool IsPlaying { get { return _lastState == State_Running; } }
        /// <summary>总时长（秒），无文件为 0。</summary>
        public double Duration
        {
            get
            {
                if (_useVlc)
                {
                    try
                    {
                        long ms = _vlcLengthMs > 0 ? _vlcLengthMs : Vlc.MediaPlayerGetLength(_vlcPlayer);
                        return ms > 0 ? ms / 1000.0 : 0;
                    }
                    catch { return 0; }
                }
                if (_mp == null) return 0;
                try { double d; return _mp.get_Duration(out d) >= 0 ? d : 0; }
                catch { return 0; }
            }
        }
        /// <summary>当前播放位置（秒）。</summary>
        public double Position
        {
            get
            {
                if (_useVlc)
                {
                    try
                    {
                        long ms = Vlc.MediaPlayerGetTime(_vlcPlayer);
                        return ms > 0 ? ms / 1000.0 : 0;
                    }
                    catch { return 0; }
                }
                if (_mp == null) return 0;
                try { double p; return _mp.get_CurrentPosition(out p) >= 0 ? p : 0; }
                catch { return 0; }
            }
        }
        /// <summary>打开并立即播放视频文件（或 rtsp/http/rtmp 网络流地址）；失败返回 false（不抛异常）。</summary>
        public bool Open(string path)
        {
            TeardownAll();
            _endNotified = false;
            _vlcEnded = false;
            _vlcError = false;
            _vlcLengthMs = -1;
            _fileName = "";
            _track.Value = 0;
            _lblTime.Text = "00:00 / 00:00";
            bool isNetwork = IsNetworkUrl(path);
            if (string.IsNullOrWhiteSpace(path) || (!isNetwork && !File.Exists(path)))
            {
                ShowHint(HTranslation.GetContent("文件不存在：\n") + path);
                PlaybackError?.Invoke(this, EventArgs.Empty);
                return false;
            }
            // 优先使用 VLC 引擎（全格式 + 网络流）；不可用或失败时降级 DirectShow
            if (Vlc.IsAvailable)
            {
                if (OpenWithVlc(path, isNetwork)) return true;
                TeardownVlc(); // VLC 打开失败，清理后降级
            }
            if (isNetwork)
            {
                // DirectShow 不支持网络流，VLC 又不可用
                ShowHint(HTranslation.GetContent("播放网络流需要安装 VLC 播放器（支持 RTSP/HTTP 等协议）"));
                PlaybackError?.Invoke(this, EventArgs.Empty);
                return false;
            }
            try
            {
                IntPtr hwnd = _videoHost.Handle; // 强制创建句柄
                Type t = Type.GetTypeFromCLSID(Ds.CLSID_FilterGraph);
                _graph = Activator.CreateInstance(t);
                _mc = (Ds.IMediaControl)_graph;
                _vw = (Ds.IVideoWindow)_graph;
                _mp = (Ds.IMediaPosition)_graph;
                int hr = _mc.RenderFile(path);
                if (hr < 0)
                {
                    // DirectShow 不支持该格式（如 MP4/H.264），尝试用系统默认播放器打开
                    TeardownGraph();
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                        _fileName = path;
                        _lblHint.Visible = false;
                        _lblFile.Text = Path.GetFileName(path) + HTranslation.GetContent("（系统播放器）");
                        _btnPlay.Glyph = HPushButtonGlyph.Play;
                        PlaybackStarted?.Invoke(this, EventArgs.Empty);
                        return true;
                    }
                    catch
                    {
                        ShowHint(HTranslation.GetContent("无法播放该文件（格式或解码器不支持）"));
                        PlaybackError?.Invoke(this, EventArgs.Empty);
                        return false;
                    }
                }
                _vw.put_Owner(hwnd);
                _vw.put_WindowStyle(WS_CHILD | WS_CLIPSIBLINGS | WS_CLIPCHILDREN);
                _vw.put_Visible(OATRUE);
                PositionVideo();
                _fileName = path;
                _lblHint.Visible = false;
                _lblFile.Text = Path.GetFileName(path);
                _btnPlay.Glyph = HPushButtonGlyph.Pause;
                hr = _mc.Run();
                if (hr < 0)
                {
                    ShowHint(HTranslation.GetContent("播放启动失败（错误码 0x") + hr.ToString("X8") + "）");
                    TeardownGraph();
                    PlaybackError?.Invoke(this, EventArgs.Empty);
                    return false;
                }
                _lastState = State_Running;
                _timer.Start();
                PlaybackStarted?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                ShowHint(HTranslation.GetContent("播放失败：") + ex.Message);
                TeardownGraph();
                PlaybackError?.Invoke(this, EventArgs.Empty);
                return false;
            }
        }
        /// <summary>播放（若已到结尾则从头开始）。</summary>
        public void Play()
        {
            if (_useVlc)
            {
                if (_vlcPlayer == IntPtr.Zero) return;
                try
                {
                    if (_vlcEnded)
                    {
                        Vlc.MediaPlayerSetTime(_vlcPlayer, 0);
                        _vlcEnded = false;
                    }
                    Vlc.MediaPlayerSetPause(_vlcPlayer, 0);
                    if (Vlc.MediaPlayerIsPlaying(_vlcPlayer) == 0)
                        Vlc.MediaPlayerPlay(_vlcPlayer);
                    _endNotified = false;
                    _vlcError = false;
                    _btnPlay.Glyph = HPushButtonGlyph.Pause;
                    _lastState = State_Running;
                }
                catch { }
                return;
            }
            if (_mc == null) return;
            try
            {
                double dur, pos;
                if (_mp != null && _mp.get_Duration(out dur) >= 0 && _mp.get_CurrentPosition(out pos) >= 0
                    && dur > 0 && pos >= dur - 0.8)
                {
                    _mp.put_CurrentPosition(0);
                }
                _mc.Run();
                _endNotified = false;
                _btnPlay.Glyph = HPushButtonGlyph.Pause;
            }
            catch { }
        }
        /// <summary>暂停。</summary>
        public void Pause()
        {
            if (_useVlc)
            {
                if (_vlcPlayer == IntPtr.Zero) return;
                try { Vlc.MediaPlayerSetPause(_vlcPlayer, 1); _btnPlay.Glyph = HPushButtonGlyph.Play; _lastState = State_Paused; }
                catch { }
                return;
            }
            if (_mc == null) return;
            try { _mc.Pause(); _btnPlay.Glyph = HPushButtonGlyph.Play; }
            catch { }
        }
        /// <summary>播放/暂停切换。</summary>
        public void TogglePlay()
        {
            if (_useVlc)
            {
                if (_vlcPlayer == IntPtr.Zero) return;
                if (IsPlaying) Pause(); else Play();
                return;
            }
            if (_mc == null) return;
            if (IsPlaying) Pause(); else Play();
        }
        /// <summary>停止并回到开头。</summary>
        public void Stop()
        {
            if (_useVlc)
            {
                if (_vlcPlayer == IntPtr.Zero) return;
                try
                {
                    Vlc.MediaPlayerStop(_vlcPlayer);
                    _vlcEnded = false;
                    _track.Value = 0;
                    _lblTime.Text = "00:00 / " + FormatTime(Duration);
                    _btnPlay.Glyph = HPushButtonGlyph.Play;
                    _lastState = State_Stopped;
                    PlaybackStopped?.Invoke(this, EventArgs.Empty);
                }
                catch { }
                return;
            }
            if (_mc == null) return;
            try
            {
                _mc.Stop();
                if (_mp != null) _mp.put_CurrentPosition(0);
                _track.Value = 0;
                _lblTime.Text = "00:00 / " + FormatTime(Duration);
                _btnPlay.Glyph = HPushButtonGlyph.Play;
                _lastState = State_Stopped;
                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }
        /// <summary>SeekToTrack 方法。</summary>
        private void SeekToTrack()
        {
            if (_useVlc)
            {
                if (_vlcPlayer == IntPtr.Zero) return;
                try
                {
                    long dur = Vlc.MediaPlayerGetLength(_vlcPlayer);
                    if (dur <= 0) dur = _vlcLengthMs;
                    if (dur > 0)
                    {
                        long pos = (long)(dur * (_track.Value / 1000.0));
                        Vlc.MediaPlayerSetTime(_vlcPlayer, pos);
                        _vlcEnded = false;
                        _endNotified = false;
                    }
                }
                catch { }
                return;
            }
            if (_mp == null) return;
            try
            {
                double dur;
                if (_mp.get_Duration(out dur) >= 0 && dur > 0)
                {
                    double pos = dur * (_track.Value / 1000.0);
                    _mp.put_CurrentPosition(pos);
                    _endNotified = false;
                }
            }
            catch { }
        }
        /// <summary>TickRefresh 方法。</summary>
        private void TickRefresh()
        {
            if (_useVlc)
            {
                VlcTickRefresh();
                return;
            }
            if (_mc == null || _mp == null) return;
            try
            {
                int state;
                _mc.GetState(100, out state);
                _lastState = state;
                double dur, pos;
                bool hasDur = _mp.get_Duration(out dur) >= 0;
                bool hasPos = _mp.get_CurrentPosition(out pos) >= 0;
                if (hasDur && hasPos && dur > 0)
                {
                    long v = (long)Math.Max(0, Math.Min(1000, pos / dur * 1000.0));
                    if (Math.Abs(v - _track.Value) > 1) _track.Value = v;
                    _lblTime.Text = FormatTime(pos) + " / " + FormatTime(dur);
                }
                if (state == State_Running) _btnPlay.Glyph = HPushButtonGlyph.Pause;
                else if (state == State_Paused) _btnPlay.Glyph = HPushButtonGlyph.Play;
                // 播放到结尾
                if (state == State_Stopped && hasPos && hasDur && dur > 0 && pos >= dur - 1.0)
                {
                    if (!_endNotified)
                    {
                        _endNotified = true;
                        _btnPlay.Glyph = HPushButtonGlyph.Play;
                        PlaybackEnded?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch { }
        }
        // ======================== VLC(libvlc) 引擎实现 ========================
        /// <summary>判断是否为网络流地址（RTSP/HTTP/RTMP/UDP/FTP/MMS 等）。</summary>
        private static bool IsNetworkUrl(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string p = path.TrimStart();
            int sp = p.IndexOf("://", StringComparison.OrdinalIgnoreCase);
            if (sp <= 0 || sp > 8) return false;
            string scheme = p.Substring(0, sp).ToLowerInvariant();
            switch (scheme)
            {
                case "rtsp": case "rtmp": case "rtmpe": case "rtmps":
                case "http": case "https": case "udp": case "tcp":
                case "ftp": case "mms": case "mmsh": case "srt":
                case "rtp": case "vlc": case "file":
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>用 VLC 打开并播放；成功返回 true。</summary>
        private bool OpenWithVlc(string path, bool isNetwork)
        {
            try
            {
                IntPtr instance = Vlc.EnsureInstance();
                if (instance == IntPtr.Zero) return false;
                IntPtr hwnd = _videoHost.Handle; // 强制创建句柄，VLC 画面嵌入此窗口
                // 本地路径用 new_path（UTF-8），网络地址用 new_location
                _vlcMedia = isNetwork
                    ? Vlc.MediaNewLocation(instance, path)
                    : Vlc.MediaNewPath(instance, path);
                if (_vlcMedia == IntPtr.Zero) return false;
                _vlcPlayer = Vlc.MediaPlayerNewFromMedia(_vlcMedia);
                if (_vlcPlayer == IntPtr.Zero) return false;
                // 订阅播放结束/出错事件（回调在 VLC 线程，仅置标志位）
                _vlcCb = new Vlc.VlcEventCallback(OnVlcEvent);
                IntPtr mgr = Vlc.MediaPlayerEventManager(_vlcPlayer);
                if (mgr != IntPtr.Zero)
                {
                    Vlc.EventAttach(mgr, Vlc.EventMediaPlayerEndReached, _vlcCb, IntPtr.Zero);
                    Vlc.EventAttach(mgr, Vlc.EventMediaPlayerEncounteredError, _vlcCb, IntPtr.Zero);
                }
                Vlc.MediaPlayerSetHwnd(_vlcPlayer, hwnd);
                int hr = Vlc.MediaPlayerPlay(_vlcPlayer);
                if (hr != 0) return false;
                _useVlc = true;
                _fileName = path;
                _vlcLengthMs = -1;
                _vlcEnded = false;
                _vlcError = false;
                _lblHint.Visible = false;
                _lblFile.Text = (isNetwork ? path : Path.GetFileName(path)) + "（VLC）";
                _btnPlay.Glyph = HPushButtonGlyph.Pause;
                _lastState = State_Running;
                _timer.Start();
                PlaybackStarted?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>VLC 事件回调（VLC 工作线程调用，只做标志置位）。</summary>
        private void OnVlcEvent(IntPtr pEvent, IntPtr opaque)
        {
            try
            {
                uint type = (uint)Marshal.ReadInt32(pEvent);
                if (type == Vlc.EventMediaPlayerEndReached) _vlcEnded = true;
                else if (type == Vlc.EventMediaPlayerEncounteredError) _vlcError = true;
            }
            catch { }
        }
        /// <summary>VLC 引擎定时刷新：进度条、时间显示、结束/错误通知。</summary>
        private void VlcTickRefresh()
        {
            if (_vlcPlayer == IntPtr.Zero) return;
            try
            {
                // 出错通知
                if (_vlcError)
                {
                    _vlcError = false;
                    _lastState = State_Stopped;
                    _btnPlay.Glyph = HPushButtonGlyph.Play;
                    PlaybackError?.Invoke(this, EventArgs.Empty);
                    return;
                }
                long dur = Vlc.MediaPlayerGetLength(_vlcPlayer);
                if (dur > 0) _vlcLengthMs = dur; else dur = _vlcLengthMs;
                long pos = Vlc.MediaPlayerGetTime(_vlcPlayer);
                if (pos < 0) pos = 0;
                if (dur > 0)
                {
                    long v = (long)Math.Max(0, Math.Min(1000, pos * 1000.0 / dur));
                    if (Math.Abs(v - _track.Value) > 1) _track.Value = v;
                    _lblTime.Text = FormatTime(pos / 1000.0) + " / " + FormatTime(dur / 1000.0);
                }
                bool playing = Vlc.MediaPlayerIsPlaying(_vlcPlayer) != 0;
                _lastState = playing ? State_Running : (_vlcEnded ? State_Stopped : State_Paused);
                _btnPlay.Glyph = playing ? HPushButtonGlyph.Pause : HPushButtonGlyph.Play;
                if (_vlcEnded && !_endNotified)
                {
                    _endNotified = true;
                    _btnPlay.Glyph = HPushButtonGlyph.Play;
                    _lastState = State_Stopped;
                    PlaybackEnded?.Invoke(this, EventArgs.Empty);
                }
            }
            catch { }
        }
        /// <summary>释放 VLC 播放器与媒体对象（libvlc 实例全局共享，不释放）。</summary>
        private void TeardownVlc()
        {
            try { if (_vlcPlayer != IntPtr.Zero) Vlc.MediaPlayerStop(_vlcPlayer); } catch { }
            if (_vlcPlayer != IntPtr.Zero)
            {
                try { Vlc.MediaPlayerRelease(_vlcPlayer); } catch { }
                _vlcPlayer = IntPtr.Zero;
            }
            if (_vlcMedia != IntPtr.Zero)
            {
                try { Vlc.MediaRelease(_vlcMedia); } catch { }
                _vlcMedia = IntPtr.Zero;
            }
            _vlcCb = null;
            _useVlc = false;
            _vlcEnded = false;
            _vlcError = false;
            _vlcLengthMs = -1;
        }
        /// <summary>释放全部播放资源（VLC + DirectShow）。</summary>
        private void TeardownAll()
        {
            TeardownVlc();
            TeardownGraph();
        }
        /// <summary>PositionVideo 方法。</summary>
        private void PositionVideo()
        {
            if (_vw == null || !_videoHost.IsHandleCreated) return;
            try
            {
                _vw.SetWindowPosition(0, 0, _videoHost.ClientSize.Width, _videoHost.ClientSize.Height);
            }
            catch { }
        }
        /// <summary>ShowHint 方法。</summary>
        private void ShowHint(string text)
        {
            _lblHint.Text = text;
            _lblHint.Visible = true;
            _lblFile.Text = "";
        }
        /// <summary>FormatTime 方法。</summary>
        private static string FormatTime(double sec)
        {
            if (sec < 0 || double.IsNaN(sec) || double.IsInfinity(sec)) sec = 0;
            int t = (int)Math.Floor(sec);
            int h = t / 3600;
            int m = (t % 3600) / 60;
            int s = t % 60;
            return h > 0
                ? h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00")
                : m.ToString("00") + ":" + s.ToString("00");
        }
        /// <summary>释放 DirectShow 滤波器图与全部 COM 对象。</summary>
        private void TeardownGraph()
        {
            _timer.Stop();
            try { _mc?.Stop(); } catch { }
            try
            {
                if (_vw != null)
                {
                    _vw.put_Visible(OAFALSE);
                    _vw.put_Owner(IntPtr.Zero);
                }
            }
            catch { }
            if (_mp != null) { try { Marshal.ReleaseComObject(_mp); } catch { } _mp = null; }
            if (_vw != null) { try { Marshal.ReleaseComObject(_vw); } catch { } _vw = null; }
            if (_mc != null) { try { Marshal.ReleaseComObject(_mc); } catch { } _mc = null; }
            if (_graph != null) { try { Marshal.ReleaseComObject(_graph); } catch { } _graph = null; }
            _btnPlay.Glyph = HPushButtonGlyph.Play;
            _lastState = State_Stopped;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TeardownAll();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>DirectShow 手写 COM 互操作（quartz.dll 系统组件，无需任何引用程序集）。</summary>
        internal static class Ds
        {
            /// <summary>CLSID_FilterGraph 成员。</summary>
            public static readonly Guid CLSID_FilterGraph = new Guid("e436ebb3-524f-11ce-9f53-0020af0ba770");
            [ComImport, Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770"),
             InterfaceType(ComInterfaceType.InterfaceIsDual)]
            public interface IMediaControl
            {
                [PreserveSig] int Run();
                [PreserveSig] int Pause();
                [PreserveSig] int Stop();
                [PreserveSig] int GetState([In] int msTimeout, out int pfs);
                [PreserveSig] int RenderFile([In, MarshalAs(UnmanagedType.BStr)] string strFilename);
                [PreserveSig] int AddSourceFilter([In, MarshalAs(UnmanagedType.BStr)] string strFilename,
                    [Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);
                [PreserveSig] int get_FilterCollection([Out, MarshalAs(UnmanagedType.IDispatch)] out object coll);
                [PreserveSig] int get_RegFilterCollection([Out, MarshalAs(UnmanagedType.IDispatch)] out object coll);
                [PreserveSig] int StopWhenReady();
            }
            [ComImport, Guid("56a868b2-0ad4-11ce-b03a-0020af0ba770"),
             InterfaceType(ComInterfaceType.InterfaceIsDual)]
            public interface IMediaPosition
            {
                [PreserveSig] int get_Duration(out double plength);
                [PreserveSig] int put_CurrentPosition(double llTime);
                [PreserveSig] int get_CurrentPosition(out double pllTime);
                [PreserveSig] int put_StopTime(double llTime);
                [PreserveSig] int get_StopTime(out double pllTime);
                [PreserveSig] int put_PrerollTime(double llTime);
                [PreserveSig] int get_PrerollTime(out double pllTime);
                [PreserveSig] int put_Rate(double dRate);
                [PreserveSig] int get_Rate(out double pdRate);
                [PreserveSig] int CanSeekForward(out int pCanSeekForward);
                [PreserveSig] int CanSeekBackward(out int pCanSeekBackward);
            }
            [ComImport, Guid("56a868b0-0ad4-11ce-b03a-0020af0ba770"),
             InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IVideoWindow
            {
                [PreserveSig] int put_Caption([In, MarshalAs(UnmanagedType.BStr)] string strCaption);
                [PreserveSig] int get_Caption([Out, MarshalAs(UnmanagedType.BStr)] out string strCaption);
                [PreserveSig] int put_WindowStyle([In] int WindowStyle);
                [PreserveSig] int get_WindowStyle(out int pWindowStyle);
                [PreserveSig] int put_WindowStyleEx([In] int WindowStyleEx);
                [PreserveSig] int get_WindowStyleEx(out int pWindowStyleEx);
                [PreserveSig] int put_AutoShow([In] int AutoShow);
                [PreserveSig] int get_AutoShow(out int pAutoShow);
                [PreserveSig] int put_WindowState([In] int WindowState);
                [PreserveSig] int get_WindowState(out int pWindowState);
                [PreserveSig] int put_BackgroundPalette([In] int BackgroundPalette);
                [PreserveSig] int get_BackgroundPalette(out int pBackgroundPalette);
                [PreserveSig] int put_Visible([In] int Visible);
                [PreserveSig] int get_Visible(out int pVisible);
                [PreserveSig] int put_Owner([In] IntPtr Owner);
                [PreserveSig] int get_Owner(out IntPtr pOwner);
                [PreserveSig] int put_MessageDrain([In] IntPtr Drain);
                [PreserveSig] int get_MessageDrain(out IntPtr pDrain);
                [PreserveSig] int get_BorderColor(out int pColor);
                [PreserveSig] int put_BorderColor([In] int Color);
                [PreserveSig] int put_FullScreenMode([In] int FullScreenMode);
                [PreserveSig] int get_FullScreenMode(out int pFullScreenMode);
                [PreserveSig] int SetWindowForeground([In] int Focus);
                [PreserveSig] int NotifyOwnerMessage([In] IntPtr hwnd, [In] int uMsg,
                    [In] IntPtr wParam, [In] IntPtr lParam);
                [PreserveSig] int SetWindowPosition([In] int Left, [In] int Top, [In] int Width, [In] int Height);
                [PreserveSig] int GetWindowPosition(out int pLeft, out int pTop, out int pWidth, out int pHeight);
                [PreserveSig] int GetMinIdealImageSize(out int pWidth, out int pHeight);
                [PreserveSig] int GetMaxIdealImageSize(out int pWidth, out int pHeight);
                [PreserveSig] int GetRestorePosition(out int pLeft, out int pTop, out int pWidth, out int pHeight);
                [PreserveSig] int HideCursor([In] int HideCursor);
                [PreserveSig] int IsCursorHidden(out int CursorHidden);
            }
        }
        /// <summary>
        /// VLC(libvlc) 原生互操作：自动探测 VLC 安装目录并预加载 DLL，
        /// 无需 NuGet 包。libvlc 实例进程内全局共享一次，所有播放器复用。
        /// </summary>
        internal static class Vlc
        {
            // libvlc 事件类型（libvlc_events.h）
            public const uint EventMediaPlayerEndReached = 265;
            public const uint EventMediaPlayerEncounteredError = 266;
            public delegate void VlcEventCallback(IntPtr pEvent, IntPtr opaque);
            private const string Lib = "libvlc.dll";
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr libvlc_new(int argc,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_release(IntPtr instance);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            private static extern IntPtr libvlc_media_new_path(IntPtr instance, byte[] pathUtf8);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            private static extern IntPtr libvlc_media_new_location(IntPtr instance, byte[] locationUtf8);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_media_release(IntPtr media);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_media_player_release(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_media_player_set_hwnd(IntPtr player, IntPtr drawable);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern int libvlc_media_player_play(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_media_player_set_pause(IntPtr player, int pause);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_media_player_stop(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern int libvlc_media_player_is_playing(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern long libvlc_media_player_get_length(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern long libvlc_media_player_get_time(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern void libvlc_media_player_set_time(IntPtr player, long time);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr libvlc_media_player_event_manager(IntPtr player);
            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
            private static extern int libvlc_event_attach(IntPtr eventManager, uint eventType,
                VlcEventCallback callback, IntPtr opaque);
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern IntPtr LoadLibrary(string lpFileName);
            private static IntPtr _instance;
            private static bool _tried;
            /// <summary>_lock 字段。</summary>
            private static readonly object _lock = new object();
            /// <summary>VLC 是否可用（已找到并成功加载 libvlc）。</summary>
            public static bool IsAvailable
            {
                get { EnsureInstance(); return _instance != IntPtr.Zero; }
            }
            /// <summary>获取（必要时创建）全局共享的 libvlc 实例；失败返回 Zero。</summary>
            public static IntPtr EnsureInstance()
            {
                lock (_lock)
                {
                    if (_instance != IntPtr.Zero) return _instance;
                    if (_tried) return IntPtr.Zero;
                    _tried = true;
                    try
                    {
                        string dir = FindVlcDir();
                        if (string.IsNullOrEmpty(dir)) return IntPtr.Zero;
                        // 让 libvlc 能找到插件目录与依赖 DLL
                        string plugins = Path.Combine(dir, "plugins");
                        if (Directory.Exists(plugins))
                            Environment.SetEnvironmentVariable("VLC_PLUGIN_PATH", plugins);
                        try { Environment.SetEnvironmentVariable("PATH", dir + ";" + Environment.GetEnvironmentVariable("PATH")); } catch { }
                        // 先按绝对路径加载 core 再加载 libvlc，确保后续 P/Invoke 命中正确位数的 DLL
                        string core = Path.Combine(dir, "libvlccore.dll");
                        string main = Path.Combine(dir, "libvlc.dll");
                        if (!File.Exists(core) || !File.Exists(main)) return IntPtr.Zero;
                        if (LoadLibrary(core) == IntPtr.Zero) return IntPtr.Zero;
                        if (LoadLibrary(main) == IntPtr.Zero) return IntPtr.Zero;
                        // --no-video-title-show：不显示文件名浮层
                        string[] args = new string[] { "-I", "dummy", "--no-video-title-show", "--quiet" };
                        _instance = libvlc_new(args.Length, args);
                        return _instance;
                    }
                    catch
                    {
                        return IntPtr.Zero;
                    }
                }
            }
            /// <summary>探测 VLC 安装目录（注册表 + 常见路径，匹配进程位数）。</summary>
            private static string FindVlcDir()
            {
                // 1) 注册表（同时查 64 位/32 位视图）
                string[] valueNames = new string[] { "InstallDir", "InstallDir64" };
                Microsoft.Win32.RegistryView[] views = new Microsoft.Win32.RegistryView[]
                {
                    Environment.Is64BitProcess ? Microsoft.Win32.RegistryView.Registry64 : Microsoft.Win32.RegistryView.Registry32,
                    Environment.Is64BitProcess ? Microsoft.Win32.RegistryView.Registry32 : Microsoft.Win32.RegistryView.Registry64
                };
                foreach (Microsoft.Win32.RegistryView view in views)
                {
                    try
                    {
                        using (Microsoft.Win32.RegistryKey baseKey =
                            Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, view))
                        using (Microsoft.Win32.RegistryKey k = baseKey.OpenSubKey(@"SOFTWARE\VideoLAN\VLC"))
                        {
                            if (k != null)
                            {
                                foreach (string vn in valueNames)
                                {
                                    string v = k.GetValue(vn) as string;
                                    if (!string.IsNullOrEmpty(v) && File.Exists(Path.Combine(v, "libvlc.dll")))
                                        return v.TrimEnd('\\');
                                }
                            }
                        }
                    }
                    catch { }
                }
                // 2) 常见安装路径
                string[] candidates;
                if (Environment.Is64BitProcess)
                {
                    candidates = new string[]
                    {
                        @"d:\Program Files\VideoLAN\VLC",
                        @"C:\Program Files\VideoLAN\VLC",
                        @"C:\Program Files (x86)\VideoLAN\VLC"
                    };
                }
                else
                {
                    candidates = new string[]
                    {
                        @"C:\Program Files (x86)\VideoLAN\VLC",
                        @"d:\Program Files (x86)\VideoLAN\VLC",
                        @"C:\Program Files\VideoLAN\VLC"
                    };
                }
                foreach (string c in candidates)
                {
                    try { if (File.Exists(Path.Combine(c, "libvlc.dll"))) return c; } catch { }
                }
                return null;
            }
            /// <summary>MediaFromUtf8 方法。</summary>
            private static IntPtr MediaFromUtf8(IntPtr instance, string s, bool location)
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                byte[] z = new byte[bytes.Length + 1];
                Buffer.BlockCopy(bytes, 0, z, 0, bytes.Length);
                return location ? libvlc_media_new_location(instance, z) : libvlc_media_new_path(instance, z);
            }
            /// <summary>MediaNewPath 方法。</summary>
            public static IntPtr MediaNewPath(IntPtr instance, string path) { return MediaFromUtf8(instance, path, false); }
            /// <summary>MediaNewLocation 方法。</summary>
            public static IntPtr MediaNewLocation(IntPtr instance, string url) { return MediaFromUtf8(instance, url, true); }
            /// <summary>MediaRelease 方法。</summary>
            public static void MediaRelease(IntPtr media) { if (media != IntPtr.Zero) libvlc_media_release(media); }
            /// <summary>MediaPlayerNewFromMedia 方法。</summary>
            public static IntPtr MediaPlayerNewFromMedia(IntPtr media) { return libvlc_media_player_new_from_media(media); }
            /// <summary>MediaPlayerRelease 方法。</summary>
            public static void MediaPlayerRelease(IntPtr player) { if (player != IntPtr.Zero) libvlc_media_player_release(player); }
            /// <summary>MediaPlayerSetHwnd 方法。</summary>
            public static void MediaPlayerSetHwnd(IntPtr player, IntPtr hwnd) { libvlc_media_player_set_hwnd(player, hwnd); }
            /// <summary>MediaPlayerPlay 方法。</summary>
            public static int MediaPlayerPlay(IntPtr player) { return libvlc_media_player_play(player); }
            /// <summary>MediaPlayerSetPause 方法。</summary>
            public static void MediaPlayerSetPause(IntPtr player, int pause) { libvlc_media_player_set_pause(player, pause); }
            /// <summary>MediaPlayerStop 方法。</summary>
            public static void MediaPlayerStop(IntPtr player) { libvlc_media_player_stop(player); }
            /// <summary>MediaPlayerIsPlaying 方法。</summary>
            public static int MediaPlayerIsPlaying(IntPtr player) { return libvlc_media_player_is_playing(player); }
            /// <summary>MediaPlayerGetLength 方法。</summary>
            public static long MediaPlayerGetLength(IntPtr player) { return libvlc_media_player_get_length(player); }
            /// <summary>MediaPlayerGetTime 方法。</summary>
            public static long MediaPlayerGetTime(IntPtr player) { return libvlc_media_player_get_time(player); }
            /// <summary>MediaPlayerSetTime 方法。</summary>
            public static void MediaPlayerSetTime(IntPtr player, long ms) { libvlc_media_player_set_time(player, ms); }
            /// <summary>MediaPlayerEventManager 方法。</summary>
            public static IntPtr MediaPlayerEventManager(IntPtr player) { return libvlc_media_player_event_manager(player); }
            /// <summary>EventAttach 方法。</summary>
            public static int EventAttach(IntPtr mgr, uint type, VlcEventCallback cb, IntPtr opaque)
            {
                return libvlc_event_attach(mgr, type, cb, opaque);
            }
        }
    }
}
