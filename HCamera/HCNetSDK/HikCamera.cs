using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HFromUI.HCamera.HCNetSDK
{
    using HFromUI.HLangage;
    /// <summary>
    /// 海康威视网络摄像机完整管理类（基于 HCNetSDK.dll）。
    /// 功能包括：SDK初始化/清理、用户注册/注销、实时预览、抓图、云台控制、
    /// 参数配置（曝光、增益、图像、编码等）、语音对讲、报警布防/撤防、
    /// 录像下载、设备重启、时间设置、设备信息获取等。
    /// 注意：所有 P/Invoke 声明和结构体需根据官方 HCNetSDK.h 进行调整。
    /// </summary>
    public class HikCamera : IDisposable
    {
        #region ==================== 内部字段 ====================

        /// <summary>_sdkInitRefCount 字段。</summary>
        private static int _sdkInitRefCount = 0;
        /// <summary>_sdkInitLock 字段。</summary>
        private static readonly object _sdkInitLock = new object();

        /// <summary>_userId 字段。</summary>
        private int _userId = -1;            // 用户ID，登录成功后返回
        /// <summary>_realPlayHandle 字段。</summary>
        private int _realPlayHandle = -1;    // 预览句柄
        /// <summary>_alarmHandle 字段。</summary>
        private int _alarmHandle = -1;       // 报警布防句柄
        /// <summary>_talkHandle 字段。</summary>
        private int _talkHandle = -1;        // 语音对讲句柄
        private bool _messageCallbackRegistered; // 报警消息回调是否已注册

        // 回调委托保持引用
        private RealDataCallBack _realDataCallback;
        private AlarmCallBack _alarmCallback;
        private ExceptionCallBack _exceptionCallback;

        // 设备信息
        public NET_DVR_DEVICEINFO_V30 DeviceInfo { get; private set; }
        /// <summary>IP 成员。</summary>
        public string IP { get; private set; }
        /// <summary>Port 成员。</summary>
        public ushort Port { get; private set; }
        /// <summary>UserName 成员。</summary>
        public string UserName { get; private set; }
        /// <summary>Password 成员。</summary>
        public string Password { get; private set; }

        /// <summary>IsLoggedIn 成员。</summary>
        public bool IsLoggedIn => _userId >= 0;
        /// <summary>IsPreviewing 成员。</summary>
        public bool IsPreviewing => _realPlayHandle >= 0;
        /// <summary>LastError 成员。</summary>
        public int LastError { get; private set; }

        // 事件
        public event EventHandler<AlarmEventArgs> AlarmReceived;
        public event EventHandler Disconnected;
        // 原始帧数据事件（用于自定义处理）
        public event Action<IntPtr, int, int, int, int> RawFrameReceived; // dataPtr, size, width, height, frameType

        private readonly SynchronizationContext _syncContext;

        #endregion

        #region ==================== 构造函数与析构 ====================

        public HikCamera()
        {
            _realDataCallback = OnRealDataCallback;
            _alarmCallback = OnAlarmCallback;
            _exceptionCallback = OnExceptionCallback;
            _syncContext = SynchronizationContext.Current;
        }

        ~HikCamera()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            StopRealPlay();
            StopTalk();
            CloseAlarm();
            Logout();
            ReleaseSDK();
        }

        #endregion

        #region ==================== SDK 初始化与清理 ====================

        /// <summary>AcquireSDK 方法。</summary>
        private static bool AcquireSDK()
        {
            lock (_sdkInitLock)
            {
                if (_sdkInitRefCount == 0)
                {
                    if (!NET_DVR_Init())
                        return false;
                    // 可选：设置日志
                    // NET_DVR_SetLogToFile(3, "C:\\HikLog\\");
                }
                _sdkInitRefCount++;
                return true;
            }
        }

        /// <summary>ReleaseSDK 方法。</summary>
        private static void ReleaseSDK()
        {
            lock (_sdkInitLock)
            {
                if (_sdkInitRefCount > 0)
                {
                    _sdkInitRefCount--;
                    if (_sdkInitRefCount == 0)
                    {
                        NET_DVR_Cleanup();
                    }
                }
            }
        }

        #endregion

        #region ==================== 登录与注销 ====================

        /// <summary>
        /// 登录设备
        /// </summary>
        public bool Login(string ip, ushort port, string userName, string password)
        {
            if (!AcquireSDK())
                throw new InvalidOperationException(HTranslation.GetContent("SDK初始化失败"));
            if (IsLoggedIn) Logout();

            IP = ip;
            Port = port;
            UserName = userName;
            Password = password;

            NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
            _userId = NET_DVR_Login_V30(ip, port, userName, password, ref deviceInfo);
            if (_userId < 0)
            {
                LastError = NET_DVR_GetLastError();
                return false;
            }

            DeviceInfo = deviceInfo;
            // 设置异常回调（断线等）
            NET_DVR_SetExceptionCallBack_V30(0, IntPtr.Zero, _exceptionCallback, IntPtr.Zero);
            return true;
        }

        /// <summary>
        /// 注销设备
        /// </summary>
        public void Logout()
        {
            StopRealPlay();
            StopTalk();
            CloseAlarm();
            if (_userId >= 0)
            {
                NET_DVR_Logout(_userId);
                _userId = -1;
            }
        }

        #endregion

        #region ==================== 实时预览 ====================

        /// <summary>
        /// 开始实时预览（直接渲染到窗口句柄）
        /// </summary>
        /// <param name="hWnd">显示窗口句柄</param>
        /// <param name="channel">通道号，从0开始</param>
        /// <param name="streamType">码流类型：0-主码流，1-子码流</param>
        /// <returns>是否成功</returns>
        public bool StartRealPlay(IntPtr hWnd, int channel = 0, int streamType = 0)
        {
            if (!IsLoggedIn) return false;
            StopRealPlay();

            NET_DVR_PREVIEWINFO previewInfo = new NET_DVR_PREVIEWINFO
            {
                hPlayWnd = hWnd,
                lChannel = channel,
                dwStreamType = (uint)streamType, // 修正：显式转换 int 到 uint
                dwLinkMode = 0, // TCP方式
                bBlocked = 1,   // 阻塞取流
                dwDisplayBufNum = 1
            };

            _realPlayHandle = NET_DVR_RealPlay_V40(_userId, ref previewInfo, null, IntPtr.Zero);
            if (_realPlayHandle < 0)
            {
                LastError = NET_DVR_GetLastError();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 开始实时预览（回调方式，获取原始码流）
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="streamType">码流类型：0-主码流，1-子码流</param>
        /// <returns>是否成功</returns>
        public bool StartRawDataPreview(int channel = 0, int streamType = 0)
        {
            if (!IsLoggedIn) return false;
            StopRealPlay();

            NET_DVR_PREVIEWINFO previewInfo = new NET_DVR_PREVIEWINFO
            {
                hPlayWnd = IntPtr.Zero, // 不绑定窗口
                lChannel = channel,
                dwStreamType = (uint)streamType, // 修正
                dwLinkMode = 0,
                bBlocked = 0, // 非阻塞
                dwDisplayBufNum = 1
            };

            _realPlayHandle = NET_DVR_RealPlay_V40(_userId, ref previewInfo, _realDataCallback, IntPtr.Zero);
            if (_realPlayHandle < 0)
            {
                LastError = NET_DVR_GetLastError();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 停止实时预览
        /// </summary>
        public void StopRealPlay()
        {
            if (_realPlayHandle >= 0)
            {
                NET_DVR_StopRealPlay(_realPlayHandle);
                _realPlayHandle = -1;
            }
        }

        // 实时数据回调（原始码流）
        private void OnRealDataCallback(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            if (pBuffer == IntPtr.Zero || dwBufSize == 0) return;
            // dwDataType 可能为 NET_DVR_SYSHEAD（1）或 NET_DVR_STREAMDATA（2）
            // 此处简化：假定为流数据，具体需要解析
            RawFrameReceived?.Invoke(pBuffer, (int)dwBufSize, 0, 0, (int)dwDataType);
        }

        #endregion

        #region ==================== 抓图 ====================

        /// <summary>
        /// 抓图（保存为 JPEG 文件）
        /// </summary>
        /// <param name="savePath">保存路径（.jpg）</param>
        /// <param name="channel">通道号，默认 0</param>
        /// <param name="quality">保留参数（SDK 中 wPicQuality 取值 0-2，此处仅兼容旧调用）</param>
        public bool CapturePicture(string savePath, int channel = 0, int quality = 80)
        {
            if (!IsLoggedIn) return false;
            NET_DVR_JPEGPARA jpegPara = new NET_DVR_JPEGPARA
            {
                wPicSize = 0, // 0-原分辨率
                wPicQuality = (ushort)quality
            };
            bool result = NET_DVR_CaptureJPEGPicture(_userId, channel, ref jpegPara, savePath);
            if (!result) LastError = NET_DVR_GetLastError();
            return result;
        }

        #endregion

        #region ==================== 云台控制 ====================

        /// <summary>
        /// 云台控制（开始动作）
        /// </summary>
        /// <param name="command">控制命令，如 "UP", "DOWN", "LEFT", "RIGHT", "ZOOM_IN" 等</param>
        /// <param name="speed">速度 1-7</param>
        /// <param name="channel">通道号</param>
        public bool PTZControlStart(string command, int speed = 5, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            uint cmd = GetPtzCommand(command);
            if (cmd == 0) return false;
            return NET_DVR_PTZControlWithSpeed(_userId, channel, cmd, 1, (uint)speed);
        }

        /// <summary>
        /// 云台控制（停止动作）
        /// </summary>
        public bool PTZControlStop(string command, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            uint cmd = GetPtzCommand(command);
            if (cmd == 0) return false;
            return NET_DVR_PTZControlWithSpeed(_userId, channel, cmd, 0, 5);
        }

        /// <summary>获取 ptzCommand。</summary>
        private uint GetPtzCommand(string command)
        {
            switch (command.ToUpper())
            {
                case "UP": return 21;      // TILT_UP
                case "DOWN": return 22;    // TILT_DOWN
                case "LEFT": return 23;    // PAN_LEFT
                case "RIGHT": return 24;   // PAN_RIGHT
                case "ZOOM_IN": return 11; // ZOOM_IN
                case "ZOOM_OUT": return 12;// ZOOM_OUT
                default: return 0;
            }
        }

        #endregion

        #region ==================== 参数配置 ====================

        /// <summary>
        /// 设置曝光参数（需根据实际结构体）
        /// </summary>
        public bool SetExposureConfig(NET_DVR_EXPOSURE_CFG config)
        {
            return SetDeviceConfig(NET_DVR_SET_EXPOSURE_CFG, config);
        }

        /// <summary>
        /// 设置增益参数（简化示例）
        /// </summary>
        public bool SetGainConfig(NET_DVR_GAIN_CFG config)
        {
            return SetDeviceConfig(NET_DVR_SET_GAIN_CFG, config);
        }

        // 通用设置配置方法（私有）
        private bool SetDeviceConfig<T>(int command, T config) where T : struct
        {
            if (!IsLoggedIn) return false;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr pConfig = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(config, pConfig, false);
                bool result = NET_DVR_SetDVRConfig(_userId, command, 0, pConfig, size);
                if (!result) LastError = NET_DVR_GetLastError();
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(pConfig);
            }
        }

        #endregion

        #region ==================== 语音对讲 ====================

        /// <summary>
        /// 开始语音对讲（仅发送方向；如需接收设备音频，可传入 VoiceDataCallBack）
        /// </summary>
        /// <param name="voiceComMode">对讲模式：0-对讲，1-广播</param>
        /// <param name="voiceDataCallback">设备端音频数据回调，不需要可传 null</param>
        public bool StartTalk(uint voiceComMode = 0, VoiceDataCallBack voiceDataCallback = null)
        {
            if (!IsLoggedIn) return false;
            StopTalk();
            // bNeedCBNoEncData=1：回调中返回未加密数据
            _talkHandle = NET_DVR_StartVoiceCom_V30(_userId, voiceComMode, 1, voiceDataCallback, IntPtr.Zero);
            if (_talkHandle < 0)
            {
                LastError = NET_DVR_GetLastError();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 停止语音对讲
        /// </summary>
        public bool StopTalk()
        {
            if (_talkHandle >= 0)
            {
                bool result = NET_DVR_StopVoiceCom(_talkHandle);
                _talkHandle = -1;
                return result;
            }
            return false;
        }

        /// <summary>
        /// 发送语音数据
        /// </summary>
        public bool SendTalkData(byte[] data)
        {
            if (_talkHandle < 0 || data == null || data.Length == 0) return false;
            IntPtr pBuffer = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, pBuffer, data.Length);
                return NET_DVR_VoiceComSendData(_talkHandle, pBuffer, (uint)data.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        #endregion

        #region ==================== 报警布防 ====================

        /// <summary>
        /// 布防报警（内部会先注册报警消息回调，原版遗漏了这一步，AlarmReceived 永远不会触发）
        /// </summary>
        public bool SetupAlarm()
        {
            if (!IsLoggedIn) return false;

            // 必须先注册报警消息回调，布防后设备上报的报警才能进入 OnAlarmCallback
            if (!_messageCallbackRegistered)
            {
                if (!NET_DVR_SetDVRMessageCallBack_V50(0, _alarmCallback, IntPtr.Zero))
                {
                    LastError = NET_DVR_GetLastError();
                    return false;
                }
                _messageCallbackRegistered = true;
            }

            NET_DVR_SETUPALARM_PARAM alarmParam = new NET_DVR_SETUPALARM_PARAM
            {
                dwSize = (uint)Marshal.SizeOf(typeof(NET_DVR_SETUPALARM_PARAM)),
                byLevel = 1,
                byAlarmInfoType = 1
            };
            _alarmHandle = NET_DVR_SetupAlarmChan_V41(_userId, ref alarmParam);
            if (_alarmHandle < 0)
            {
                LastError = NET_DVR_GetLastError();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 撤防报警
        /// </summary>
        public void CloseAlarm()
        {
            if (_alarmHandle >= 0)
            {
                NET_DVR_CloseAlarmChan_V30(_alarmHandle);
                _alarmHandle = -1;
            }
        }

        /// <summary>响应 AlarmCallback 事件。</summary>
        private void OnAlarmCallback(int lCommand, ref NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            // 解析报警信息，触发事件
            byte[] data = new byte[dwBufLen];
            Marshal.Copy(pAlarmInfo, data, 0, (int)dwBufLen);
            _syncContext?.Post(_ => AlarmReceived?.Invoke(this, new AlarmEventArgs(data)), null);
        }

        /// <summary>响应 ExceptionCallback 事件。</summary>
        private void OnExceptionCallback(uint dwType, int lUserID, int lHandle, IntPtr pUser)
        {
            _syncContext?.Post(_ => Disconnected?.Invoke(this, EventArgs.Empty), null);
        }

        #endregion

        #region ==================== 录像下载 ====================

        /// <summary>
        /// 按时间下载录像
        /// </summary>
        public int DownloadRecord(int channel, DateTime startTime, DateTime endTime, string savePath)
        {
            if (!IsLoggedIn) return -1;

            NET_DVR_PLAYCOND playCond = new NET_DVR_PLAYCOND
            {
                dwChannel = channel,
                struStartTime = new NET_DVR_TIME
                {
                    dwYear = startTime.Year,
                    dwMonth = startTime.Month,
                    dwDay = startTime.Day,
                    dwHour = startTime.Hour,
                    dwMinute = startTime.Minute,
                    dwSecond = startTime.Second
                },
                struStopTime = new NET_DVR_TIME
                {
                    dwYear = endTime.Year,
                    dwMonth = endTime.Month,
                    dwDay = endTime.Day,
                    dwHour = endTime.Hour,
                    dwMinute = endTime.Minute,
                    dwSecond = endTime.Second
                }
            };

            int downloadHandle = NET_DVR_GetFileByTime(_userId, ref playCond, savePath);
            if (downloadHandle < 0)
                LastError = NET_DVR_GetLastError();
            return downloadHandle;
        }

        /// <summary>StopDownload 方法。</summary>
        public bool StopDownload(int downloadHandle)
        {
            return NET_DVR_StopGetFile(downloadHandle);
        }

        /// <summary>获取 downloadProgress。</summary>
        public int GetDownloadProgress(int downloadHandle)
        {
            return NET_DVR_GetDownloadPos(downloadHandle);
        }

        #endregion

        #region ==================== 设备控制 ====================

        /// <summary>RebootDevice 方法。</summary>
        public bool RebootDevice()
        {
            if (!IsLoggedIn) return false;
            return NET_DVR_RebootDVR(_userId);
        }

        /// <summary>设置 deviceTime。</summary>
        public bool SetDeviceTime(DateTime time)
        {
            NET_DVR_TIME dvrTime = new NET_DVR_TIME
            {
                dwYear = time.Year,
                dwMonth = time.Month,
                dwDay = time.Day,
                dwHour = time.Hour,
                dwMinute = time.Minute,
                dwSecond = time.Second
            };
            // 使用通用配置方法设置时间
            return SetDeviceConfig(NET_DVR_SET_TIMECFG, dvrTime);
        }

        /// <summary>获取 deviceTime。</summary>
        public bool GetDeviceTime(out DateTime time)
        {
            time = DateTime.MinValue;
            if (!IsLoggedIn) return false;
            NET_DVR_TIME dvrTime = new NET_DVR_TIME();
            int size = Marshal.SizeOf(typeof(NET_DVR_TIME));
            IntPtr pBuffer = Marshal.AllocHGlobal(size);
            int bytesReturned = 0;
            try
            {
                bool result = NET_DVR_GetDVRConfig(_userId, NET_DVR_GET_TIMECFG, 0, pBuffer, size, out bytesReturned);
                if (result && bytesReturned > 0)
                {
                    dvrTime = (NET_DVR_TIME)Marshal.PtrToStructure(pBuffer, typeof(NET_DVR_TIME));
                    time = new DateTime(dvrTime.dwYear, dvrTime.dwMonth, dvrTime.dwDay, dvrTime.dwHour, dvrTime.dwMinute, dvrTime.dwSecond);
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        #endregion

        #region ==================== P/Invoke 声明 ====================

        private const int NET_DVR_SET_EXPOSURE_CFG = 1000; // 示例值，实际请查头文件
        private const int NET_DVR_SET_GAIN_CFG = 1001;     // 示例值
        private const int NET_DVR_SET_TIMECFG = 118;
        private const int NET_DVR_GET_TIMECFG = 118;

        // 初始化与清理
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_Init", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_Init();

        // SDK 中 NET_DVR_Cleanup 返回值为 void，不能声明为 bool
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_Cleanup", CallingConvention = CallingConvention.Cdecl)]
        private static extern void NET_DVR_Cleanup();

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_GetLastError", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_GetLastError();

        // 登录
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_Login_V30", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_Login_V30(string sDVRIP, ushort wDVRPort, string sUserName, string sPassword, ref NET_DVR_DEVICEINFO_V30 lpDeviceInfo);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_Logout", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_Logout(int iUserID);

        // 预览
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_RealPlay_V40", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_RealPlay_V40(int iUserID, ref NET_DVR_PREVIEWINFO pPreviewInfo, RealDataCallBack cbRealData, IntPtr pUser);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_StopRealPlay", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_StopRealPlay(int iRealHandle);

        // 抓图
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_CaptureJPEGPicture", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_CaptureJPEGPicture(int iUserID, int iChannel, ref NET_DVR_JPEGPARA lpJpegPara, string sPicFileName);

        // 云台
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_PTZControlWithSpeed", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_PTZControlWithSpeed(int iUserID, int iChannel, uint dwPTZCommand, uint dwStop, uint dwSpeed);

        // 配置
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_SetDVRConfig", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_SetDVRConfig(int iUserID, int dwCommand, int iChannel, IntPtr lpInBuffer, int dwInBufferSize);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_GetDVRConfig", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_GetDVRConfig(int iUserID, int dwCommand, int iChannel, IntPtr lpOutBuffer, int dwOutBufferSize, out int lpBytesReturned);

        // 对讲（V30 接口；旧版 NET_DVR_StartVoiceCom 签名为 (userID, hPlayWnd, cb, pUser)，
        // 原代码 (int, string, IntPtr) 与任何版本都不匹配，调用会导致封送错误）
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_StartVoiceCom_V30", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_StartVoiceCom_V30(int iUserID, uint dwVoiceComMode, int bNeedCBNoEncData, VoiceDataCallBack cbVoiceDataCallBack, IntPtr pUser);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_StopVoiceCom", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_StopVoiceCom(int iVoiceHandle);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_VoiceComSendData", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_VoiceComSendData(int iVoiceHandle, IntPtr pData, uint dwDataSize);

        // 报警
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_SetupAlarmChan_V41", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_SetupAlarmChan_V41(int iUserID, ref NET_DVR_SETUPALARM_PARAM lpSetupParam);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_CloseAlarmChan_V30", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_CloseAlarmChan_V30(int iAlarmHandle);

        // 注册报警消息回调（布防前必须调用，否则布防后报警无回调）
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_SetDVRMessageCallBack_V50", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_SetDVRMessageCallBack_V50(int nIndex, AlarmCallBack fMessageCallBack, IntPtr pUser);

        // 下载
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_GetFileByTime", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_GetFileByTime(int iUserID, ref NET_DVR_PLAYCOND lpPlayCond, string sSavedFileName);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_StopGetFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_StopGetFile(int iHandle);

        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_GetDownloadPos", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NET_DVR_GetDownloadPos(int iHandle);

        // 设备控制
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_RebootDVR", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_RebootDVR(int iUserID);

        // 异常回调
        [DllImport("HCNetSDK.dll", EntryPoint = "NET_DVR_SetExceptionCallBack_V30", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NET_DVR_SetExceptionCallBack_V30(int iMessage, IntPtr hWnd, ExceptionCallBack fExceptionCallBack, IntPtr pUser);

        // 回调委托
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RealDataCallBack(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AlarmCallBack(int lCommand, ref NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ExceptionCallBack(uint dwType, int lUserID, int lHandle, IntPtr pUser);

        // 语音数据回调（对讲时设备端音频数据，本类仅发送数据，不处理接收）
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VoiceDataCallBack(int lVoiceComHandle, IntPtr pRecvDataBuffer, uint dwBufSize, byte byAudioFlag, IntPtr pUser);

        #endregion

        #region ==================== 结构体和枚举 ====================

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_DEVICEINFO_V30
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] sSerialNumber;
            public byte byAlarmInPortNum;
            public byte byAlarmOutPortNum;
            public byte byDiskNum;
            public byte byDVRType;
            public byte byChanNum;
            public byte byStartChan;
            public byte byAudioChanNum;
            public byte byIPChanNum;
            public byte byZeroChanNum;
            public byte byMainProto;
            public byte bySubProto;
            public byte bySupport;
            public byte bySupport1;
            public byte bySupport2;
            public ushort wDevType;
            public byte bySupport3;
            public byte byMultiStreamProto;
            public byte byStartDChan;
            public byte byStartDTalkChan;
            public byte byHighDChanNum;
            public byte bySupport4;
            public byte byLanguageType;
            public byte byVoiceInChanNum;
            public byte byStartVoiceInChanNo;
            public byte bySupport5;
            public byte bySupport6;
            public byte byMirrorChanNum;
            public ushort wStartMirrorChanNo;
            public byte bySupport7;
            public byte byRes2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_PREVIEWINFO
        {
            public int lChannel;
            public uint dwStreamType;
            public uint dwLinkMode;
            public IntPtr hPlayWnd;
            public int bBlocked;            // BOOL
            public int bPassbackRecord;     // BOOL
            public byte byPreviewMode;
            // STREAM_ID_LEN = 32，原版声明为单个 byte 会导致后续字段整体错位
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] byStreamID;
            public byte byProtoType;
            public byte byRes1;
            public byte byVideoCodingType;
            public uint dwDisplayBufNum;
            // 原版声明为单个 byte，实际为 byRes2[20]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] byRes2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_JPEGPARA
        {
            public ushort wPicSize;
            public ushort wPicQuality;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_TIME
        {
            public int dwYear;
            public int dwMonth;
            public int dwDay;
            public int dwHour;
            public int dwMinute;
            public int dwSecond;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_PLAYCOND
        {
            public int dwChannel;
            public NET_DVR_TIME struStartTime;
            public NET_DVR_TIME struStopTime;
            public byte byDrawFrame;
            public byte byStreamType;
            // 原版为单个 byte，实际为 byStreamID[STREAM_ID_LEN=32]，否则 dwFileType 错位
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] byStreamID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] byRes;
            public uint dwFileType;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_SETUPALARM_PARAM
        {
            public uint dwSize;
            public byte byLevel;
            public byte byAlarmInfoType;
            public byte byRetAlarmTypeV40;
            public byte byRetDevInfoVersion;
            public byte byRetVQDAlarmType;
            // 原版为单个 byte，实际为 byRes[6]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] byRes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_ALARMER
        {
            public byte byUserIDValid;
            public byte bySerialValid;
            public byte byVersionValid;
            public byte byDeviceNameValid;
            public byte byMacAddrValid;
            public byte byLinkPortValid;
            public byte byDeviceIPValid;
            public byte bySocketIPValid;
            public int lUserID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] sSerialNumber;
            public uint dwVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string sDeviceName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] byMacAddr;
            public ushort wLinkPort;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string sDeviceIP;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string sSocketIP;
            public byte byIpProtocol;
            public byte byRes2;
        }

        // 曝光配置结构体（示例，需根据 SDK 头文件定义）
        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_EXPOSURE_CFG
        {
            public byte byExposureMode;
            public byte byAutoApertureLevel;
            public byte byRes1;
            public byte byRes2;
            public uint dwVideoExposureSet;
            public uint dwExposureUserSet;
            public uint dwRes;
        }

        // 增益配置结构体（示例）
        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DVR_GAIN_CFG
        {
            public byte byGainMode;
            public byte byRes1;
            public ushort wGain;
        }

        public class AlarmEventArgs : EventArgs
        {
            /// <summary>RawData 成员。</summary>
            public byte[] RawData { get; }
            public AlarmEventArgs(byte[] data)
            {
                RawData = data;
            }
        }

        #endregion
    }

}
