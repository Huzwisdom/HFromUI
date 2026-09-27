using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using HFromUI.HConvert; // 引入 VideoFrameFormat 枚举（用于原始帧回调）

namespace HFromUI.HCamera.DaHuaNetSDK
{
    using HFromUI.HLangage;
    /// <summary>
    /// 大华网络摄像机完整管理类（优化版，基于大华设备网络SDK V3.x）。
    /// 功能全面，涵盖：SDK初始化/清理（引用计数）、设备登录/登出、实时预览（三种方式）、
    /// 本地录像、抓图（文件/内存）、云台控制（基本/扩展/预置位）、参数配置（曝光、增益、
    /// 图像、白平衡、编码、网络、OSD等）、音频对讲、报警订阅、录像下载、设备重启/时间设置、
    /// 设备信息获取、透明通道（串口透传）以及原始帧回调。
    /// 所有 P/Invoke 声明均带有完整 EntryPoint 和 CallingConvention。
    /// 注意：结构体和函数签名可能与具体 SDK 版本略有差异，请以官方头文件（dhnetsdk.h）为准调整。
    /// </summary>
    public class DahuaCamera : IDisposable
    {
        #region ==================== 内部字段 ====================

        // SDK 全局初始化引用计数（静态，多个实例共享）
        private static int _sdkInitRefCount = 0;
        /// <summary>_sdkInitLock 字段。</summary>
        private static readonly object _sdkInitLock = new object();

        // 断线/报警回调是 SDK 全局回调（CLIENT_Init / CLIENT_SetDVRMessCallBack 均为全局设置），
        // 必须使用静态委托保持引用，并按登录句柄路由到对应实例
        private static readonly DisConnectCallBack s_globalDisConnectCallback = OnGlobalDisConnect;
        /// <summary>s_globalAlarmCallback 字段。</summary>
        private static readonly AlarmMessageCallBack s_globalAlarmCallback = OnGlobalAlarmMessage;
        /// <summary>s_instances 字段。</summary>
        private static readonly List<DahuaCamera> s_instances = new List<DahuaCamera>();
        /// <summary>s_instancesLock 字段。</summary>
        private static readonly object s_instancesLock = new object();

        // 当前实例登录句柄
        private IntPtr _loginId = IntPtr.Zero;
        // 实时预览句柄（CLIENT_RealPlay/Ex 返回）
        private IntPtr _realPlayHandle = IntPtr.Zero;
        // 播放解码库端口与状态（dhplay.dll / play.dll）。
        // 注意：实时流回调输出的是 H.264/H.265 压缩码流，必须经播放库解码后才是 YV12 原始帧
        private int _playPort = -1;
        /// <summary>_decoderOpened 字段。</summary>
        private bool _decoderOpened = false;
        // 语音对讲句柄
        private IntPtr _talkHandle = IntPtr.Zero;

        // 回调委托保持引用，防止被 GC 回收
        private RealDataCallBackEx _realDataCallbackEx; // 实时流回调（fRealDataCallBackEx，6 参数）
        private DecFrameCallBack _decFrameCallback;      // 播放库解码后帧回调（fDecCBFun）
        private fTransComCallBack _transComCallback;     // 透明通道回调

        // 设备信息
        public NET_DEVICEINFO DeviceInfo { get; private set; }
        /// <summary>IP 成员。</summary>
        public string IP { get; private set; }
        /// <summary>Port 成员。</summary>
        public ushort Port { get; private set; }
        /// <summary>UserName 成员。</summary>
        public string UserName { get; private set; }
        /// <summary>Password 成员。</summary>
        public string Password { get; private set; }

        /// <summary>IsLoggedIn 成员。</summary>
        public bool IsLoggedIn => _loginId != IntPtr.Zero;
        /// <summary>IsPreviewing 成员。</summary>
        public bool IsPreviewing => _realPlayHandle != IntPtr.Zero;
        /// <summary>LastError 成员。</summary>
        public int LastError { get; private set; }

        // 事件
        public event EventHandler<AlarmEventArgs> AlarmReceived;
        public event EventHandler Disconnected;
        // 原始帧数据事件（用于自定义显示/处理）
        public event Action<IntPtr, int, int, int, VideoFrameFormat> RawFrameReceived;
        // 透明通道数据接收事件
        public event EventHandler<TransComEventArgs> TransComDataReceived;

        // 同步上下文用于在 UI 线程触发事件
        private readonly SynchronizationContext _syncContext;

        #endregion

        #region ==================== 构造函数与析构 ====================

        public DahuaCamera()
        {
            _realDataCallbackEx = OnRealDataCallbackEx;
            _decFrameCallback = OnDecodedFrame;
            _transComCallback = OnTransComCallback;
            _syncContext = SynchronizationContext.Current;

            // 注册到全局实例表，供 SDK 全局回调按登录句柄路由
            lock (s_instancesLock)
                s_instances.Add(this);
        }

        ~DahuaCamera()
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
            // 从全局实例表移除
            lock (s_instancesLock)
                s_instances.Remove(this);

            // 先关闭透明通道（如果打开）
            CloseTransCom(0); // 假设通道0，实际可根据需要调整
            StopRealPlay();
            StopTalk();
            Logout();
            ReleaseSDK();
        }

        #endregion

        #region ==================== SDK 初始化与清理（引用计数） ====================

        /// <summary>
        /// 初始化 SDK（内部使用引用计数，多个实例可安全调用）
        /// </summary>
        private static bool AcquireSDK()
        {
            lock (_sdkInitLock)
            {
                if (_sdkInitRefCount == 0)
                {
                    // CLIENT_Init 必须传入断线回调；原代码传 IntPtr.Zero，断线事件永远不会触发
                    if (!CLIENT_Init(s_globalDisConnectCallback, IntPtr.Zero))
                        return false;
                    // 报警消息回调为全局回调，需在订阅报警（CLIENT_StartListenEx）之前设置
                    CLIENT_SetDVRMessCallBack(s_globalAlarmCallback, IntPtr.Zero);
                }
                _sdkInitRefCount++;
                return true;
            }
        }

        /// <summary>
        /// 释放 SDK 引用，当计数归零时执行清理
        /// </summary>
        private static void ReleaseSDK()
        {
            lock (_sdkInitLock)
            {
                if (_sdkInitRefCount > 0)
                {
                    _sdkInitRefCount--;
                    if (_sdkInitRefCount == 0)
                    {
                        CLIENT_Cleanup();
                    }
                }
            }
        }

        #endregion

        #region ==================== 登录与注销 ====================

        /// <summary>
        /// 登录设备
        /// </summary>
        /// <param name="ip">设备 IP 地址</param>
        /// <param name="port">设备端口（默认 37777）</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>登录是否成功</returns>
        public bool Login(string ip, ushort port, string userName, string password)
        {
            if (!AcquireSDK())
                throw new InvalidOperationException(HTranslation.GetContent("SDK初始化失败"));
            if (IsLoggedIn) Logout();

            IP = ip;
            Port = port;
            UserName = userName;
            Password = password;

            NET_DEVICEINFO deviceInfo = new NET_DEVICEINFO();
            int error = 0;
            _loginId = CLIENT_Login(ip, port, userName, password, ref deviceInfo, ref error);
            if (_loginId == IntPtr.Zero)
            {
                LastError = error;
                return false;
            }

            DeviceInfo = deviceInfo;
            // 断线回调与报警回调均为 SDK 全局回调，已在 AcquireSDK/CLIENT_Init 中统一注册
            return true;
        }

        /// <summary>
        /// 注销设备
        /// </summary>
        public void Logout()
        {
            StopRealPlay();
            StopTalk();
            if (_loginId != IntPtr.Zero)
            {
                CLIENT_Logout(_loginId);
                _loginId = IntPtr.Zero;
            }
        }

        #endregion

        #region ==================== 实时预览 ====================

        /// <summary>
        /// 开始实时预览（方式一：使用 CLIENT_RealPlayEx 直接显示，SDK 内部解码）
        /// </summary>
        /// <param name="hWnd">显示窗口句柄（如 Panel.Handle）</param>
        /// <param name="channel">通道号（从 0 开始）</param>
        /// <param name="streamType">码流类型：0-主码流，1-辅码流</param>
        /// <returns>是否成功</returns>
        public bool StartRealPlay(IntPtr hWnd, int channel = 0, int streamType = 0)
        {
            if (!IsLoggedIn) return false;
            StopRealPlay();

            if (hWnd == IntPtr.Zero) return false;

            _realPlayHandle = CLIENT_RealPlayEx(_loginId, channel, hWnd, streamType);
            return _realPlayHandle != IntPtr.Zero;
        }

        /// <summary>
        /// 开始实时预览（方式二：窗口模式，SDK 内部解码并直接渲染到指定窗口）
        /// </summary>
        /// <param name="hWnd">显示窗口句柄（如 HVisualWindow.Handle）</param>
        /// <param name="channel">通道号</param>
        /// <returns>是否成功</returns>
        public bool StartRealPlayWithPlayer(IntPtr hWnd, int channel = 0)
        {
            // 窗口模式由 SDK 内部解码渲染，无需播放库回调；与 StartRealPlay 等效
            return StartRealPlay(hWnd, channel, 0);
        }

        /// <summary>
        /// 启动原始帧预览（方式三：压缩码流经播放库解码为 YV12 帧，触发 RawFrameReceived 事件）。
        /// 流程：CLIENT_RealPlay(无窗口) → CLIENT_SetRealDataCallBackEx 取压缩码流
        /// → PLAY_OpenStream/PLAY_InputData 送入播放库 → PLAY_SetDecCallBackEx 解码回调输出 YV12。
        /// 需要部署播放库 dhplay.dll（新版 SDK）或 play.dll（旧版 SDK）。
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <returns>是否成功</returns>
        public bool StartRawDataPreview(int channel = 0)
        {
            if (!IsLoggedIn) return false;
            StopRealPlay();

            try
            {
                // 1、申请播放库解码端口。解码回调必须在 PLAY_OpenStream 成功之后注册
                // （标准次序：流头 → PLAY_OpenStream → PLAY_SetDecCallBackEx → PLAY_Play，见 OpenDecoder）
                if (!PlayGetFreePort(ref _playPort) || _playPort < 0)
                {
                    LastError = CLIENT_GetLastError();
                    _playPort = -1;
                    return false;
                }
                _decoderOpened = false;

                // 2、启动无窗口实时预览（SDK 通过回调输出压缩码流）
                _realPlayHandle = CLIENT_RealPlay(_loginId, channel, IntPtr.Zero);
                if (_realPlayHandle == IntPtr.Zero)
                {
                    LastError = CLIENT_GetLastError();
                    StopRealPlay();
                    return false;
                }

                // 3、注册实时流回调，dwFlag=0x1F 表示回调全部数据（流头/视频流/音频等）
                if (!CLIENT_SetRealDataCallBackEx(_realPlayHandle, _realDataCallbackEx, IntPtr.Zero, 0x1F))
                {
                    LastError = CLIENT_GetLastError();
                    StopRealPlay();
                    return false;
                }
            }
            catch (DllNotFoundException)
            {
                // 未部署播放库 dhplay.dll / play.dll
                StopRealPlay();
                LastError = -1;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 停止实时预览（同时关闭播放库解码流）
        /// </summary>
        public void StopRealPlay()
        {
            if (_realPlayHandle != IntPtr.Zero)
            {
                CLIENT_StopRealPlay(_realPlayHandle);
                _realPlayHandle = IntPtr.Zero;
            }
            if (_playPort >= 0)
            {
                int port = _playPort;
                _playPort = -1;
                _decoderOpened = false;
                try
                {
                    // 标准销毁次序（与创建逆序）：PLAY_Stop → PLAY_CloseStream → PLAY_ReleasePort。
                    // 原代码缺少 ReleasePort，反复连断会泄漏播放库解码端口，最终取流失败
                    PlayStop(port);
                    PlayCloseStream(port);
                    PlayReleasePort(port);
                }
                catch (DllNotFoundException) { }
            }
        }

        /// <summary>
        /// 实时流回调（fRealDataCallBackEx，SDK 线程触发，共 6 个参数）。
        /// dwDataType：0-流头，1-视频流数据，3-音频数据等（见 dhnetsdk.h）。
        /// 视频数据为 H.264/H.265 压缩码流，必须送入播放库解码，不能直接当作 YUV 帧使用。
        /// </summary>
        private void OnRealDataCallbackEx(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer,
            uint dwBufSize, int lParam, IntPtr dwUser)
        {
            if (pBuffer == IntPtr.Zero || dwBufSize == 0 || _playPort < 0) return;

            try
            {
                if (dwDataType == 0)
                {
                    // 流头：用流头打开播放库解码流（部分设备可能不发送流头）
                    if (!_decoderOpened)
                        _decoderOpened = OpenDecoder(pBuffer, dwBufSize);
                }
                else if (dwDataType == 1)
                {
                    // 视频流数据：若尚未打开解码流（设备未发流头），用空流头打开
                    if (!_decoderOpened)
                        _decoderOpened = OpenDecoder(IntPtr.Zero, 0);
                    if (_decoderOpened)
                        PlayInputData(_playPort, pBuffer, (int)dwBufSize);
                }
            }
            catch
            {
                // SDK 回调线程中不得抛出异常
            }
        }

        /// <summary>
        /// 打开播放库解码流：PLAY_OpenStream（传流头）→ PLAY_SetDecCallBackEx → PLAY_Play(无窗口)
        /// </summary>
        private bool OpenDecoder(IntPtr pHeaderBuf, uint headerSize)
        {
            if (_playPort < 0) return false;
            // nBufPoolSize：码流缓冲池大小，2MB 足够
            if (!PlayOpenStream(_playPort, pHeaderBuf, headerSize, 2u * 1024 * 1024))
                return false;
            if (!PlaySetDecCallBackEx(_playPort, _decFrameCallback, IntPtr.Zero))
            {
                PlayCloseStream(_playPort); // 打开成功但后续失败：先关流，避免端口残留半开状态
                return false;
            }
            // hWnd 传 NULL：仅解码不显示，解码帧通过 OnDecodedFrame 回调输出
            if (!PlayPlay(_playPort, IntPtr.Zero))
            {
                PlayCloseStream(_playPort);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 播放库解码回调（fDecCBFun，播放库线程触发）。
        /// nType==3（T_YV12）时 pBuf 为解码后的 YV12 数据，宽高在 FRAME_INFO 中。
        /// 注意：pBuf 在回调返回后即失效，订阅者必须在事件处理中同步复制数据。
        /// </summary>
        private void OnDecodedFrame(int nPort, IntPtr pBuf, int nSize,
            IntPtr pFrameInfo, IntPtr pReserved1, int nReserved2)
        {
            if (pBuf == IntPtr.Zero || pFrameInfo == IntPtr.Zero || nSize <= 0) return;

            try
            {
                PLAY_FRAME_INFO info = Marshal.PtrToStructure<PLAY_FRAME_INFO>(pFrameInfo);
                if (info.nType != 3) return; // T_YV12 = 3，仅处理 YV12 视频帧
                if (info.nWidth <= 0 || info.nHeight <= 0) return;
                // YV12 数据大小应为 width*height*3/2，校验防止异常数据导致越界
                if (nSize < info.nWidth * info.nHeight * 3 / 2) return;

                RawFrameReceived?.Invoke(pBuf, nSize, info.nWidth, info.nHeight, VideoFrameFormat.YV12);
            }
            catch
            {
                // 播放库回调线程中不得抛出异常
            }
        }

        #endregion

        #region ==================== 本地录像 ====================

        /// <summary>
        /// 开始本地录像（未压缩 AVI 格式）
        /// </summary>
        /// <param name="filePath">录像文件保存路径（.avi）</param>
        /// <returns>是否成功</returns>
        public bool StartRecord(string filePath)
        {
            if (_realPlayHandle == IntPtr.Zero) return false;
            return CLIENT_SaveRealData(_realPlayHandle, filePath);
        }

        /// <summary>
        /// 停止本地录像
        /// </summary>
        public bool StopRecord()
        {
            if (_realPlayHandle == IntPtr.Zero) return false;
            return CLIENT_StopSaveRealData(_realPlayHandle);
        }

        #endregion

        #region ==================== 抓图 ====================

        /// <summary>
        /// SDK 直接抓图（保存为 JPEG 文件）
        /// </summary>
        /// <param name="savePath">保存路径（.jpg）</param>
        /// <param name="quality">图像质量 1-100</param>
        /// <returns>是否成功</returns>
        public bool CapturePicture(string savePath, int quality = 80)
        {
            if (_realPlayHandle == IntPtr.Zero) return false;
            return CLIENT_CapturePicture(_realPlayHandle, savePath, quality);
        }

        /// <summary>
        /// 从预览帧抓图（两种预览模式均可用 SDK 抓图，保存为 JPEG）
        /// </summary>
        public bool CaptureFromPreview(string savePath, int quality = 80)
        {
            return CapturePicture(savePath, quality);
        }

        /// <summary>
        /// 抓图到内存（返回 JPEG 数据）
        /// </summary>
        /// <param name="buffer">用于存储 JPEG 数据的缓冲区</param>
        /// <param name="bufferSize">缓冲区大小</param>
        /// <param name="sizeReturned">实际返回的数据大小</param>
        /// <returns>是否成功</returns>
        public bool CapturePictureToMemory(byte[] buffer, int bufferSize, out int sizeReturned)
        {
            sizeReturned = 0;
            if (_realPlayHandle == IntPtr.Zero) return false;
            IntPtr pBuffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                bool result = CLIENT_CapturePictureToMemory(_realPlayHandle, pBuffer, bufferSize, ref sizeReturned);
                if (result && sizeReturned > 0)
                    Marshal.Copy(pBuffer, buffer, 0, sizeReturned);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        #endregion

        #region ==================== 云台控制 ====================

        /// <summary>
        /// 云台控制（开始动作）
        /// </summary>
        /// <param name="command">控制命令（EM_PTZ_CMD 枚举）</param>
        /// <param name="speed">速度 1-7</param>
        /// <param name="channel">通道号</param>
        /// <returns>是否成功</returns>
        public bool PTZControlStart(EM_PTZ_CMD command, int speed = 5, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_DHPTZControlStart(_loginId, channel, command.ToString(), speed);
        }

        /// <summary>
        /// 云台控制（停止动作）
        /// </summary>
        public bool PTZControlStop(EM_PTZ_CMD command, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_DHPTZControlStop(_loginId, channel, command.ToString());
        }

        /// <summary>
        /// 云台控制一次（自动开始并停止）
        /// </summary>
        public bool PTZControlOnce(EM_PTZ_CMD command, int speed = 5, int durationMs = 500, int channel = 0)
        {
            if (!PTZControlStart(command, speed, channel)) return false;
            Thread.Sleep(durationMs);
            return PTZControlStop(command, channel);
        }

        /// <summary>
        /// 扩展云台控制（绝对移动、变倍等，需传入特定的结构体指针）
        /// </summary>
        public bool PTZControlEx(IntPtr param, int paramSize)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_DHPTZControlEx(_loginId, 0, param, paramSize, 5000);
        }

        /// <summary>
        /// 设置预置位
        /// </summary>
        /// <param name="presetIndex">预置位编号</param>
        /// <param name="channel">通道号</param>
        public bool SetPreset(int presetIndex, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_DHPTZPresetSet(_loginId, channel, presetIndex);
        }

        /// <summary>
        /// 调用预置位
        /// </summary>
        public bool GotoPreset(int presetIndex, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_DHPTZPresetGoto(_loginId, channel, presetIndex);
        }

        /// <summary>
        /// 删除预置位
        /// </summary>
        public bool DeletePreset(int presetIndex, int channel = 0)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_DHPTZPresetDelete(_loginId, channel, presetIndex);
        }

        #endregion

        #region ==================== 参数配置 ====================

        /// <summary>
        /// 设置曝光参数
        /// </summary>
        public bool SetExposureConfig(NET_DEVICE_EXPOSURE_CFG config) => SetConfig(DH_DEV_EXPOSURE_CFG, config);
        /// <summary>
        /// 获取曝光参数
        /// </summary>
        public bool GetExposureConfig(ref NET_DEVICE_EXPOSURE_CFG config) => GetConfig(DH_DEV_EXPOSURE_CFG, ref config);
        /// <summary>
        /// 设置增益参数
        /// </summary>
        public bool SetGainConfig(NET_DEVICE_GAIN_CFG config) => SetConfig(DH_DEV_GAIN_CFG, config);

        /// <summary>
        /// 便捷设置曝光时间（微秒，自动切换为手动曝光模式）。
        /// 对应监控软件中直接输入曝光时间（微秒）的场景。
        /// </summary>
        /// <param name="exposureTimeUs">曝光时间，单位微秒</param>
        public bool SetExposureTime(uint exposureTimeUs)
        {
            NET_DEVICE_EXPOSURE_CFG cfg = new NET_DEVICE_EXPOSURE_CFG
            {
                byExposureMode = 1,        // 1=手动曝光
                byGainMode = 1,            // 1=手动增益（与曝光同在一个配置体）
                dwExposureTime = exposureTimeUs,
                dwMinExposureTime = 100,
                dwMaxExposureTime = 1000000,
                dwMinGain = 0,
                dwMaxGain = 100
            };
            return SetExposureConfig(cfg);
        }

        /// <summary>
        /// 便捷设置增益值（自动切换为手动增益模式）。
        /// </summary>
        /// <param name="gain">增益值（通常 0-100）</param>
        public bool SetGainValue(uint gain)
        {
            NET_DEVICE_GAIN_CFG cfg = new NET_DEVICE_GAIN_CFG
            {
                byGainMode = 1,            // 1=手动增益
                dwGain = gain,
                dwMinGain = 0,
                dwMaxGain = 100
            };
            return SetGainConfig(cfg);
        }

        /// <summary>
        /// 切换为自动曝光 + 自动增益。
        /// </summary>
        public bool SetAutoExposureGain()
        {
            NET_DEVICE_EXPOSURE_CFG cfg = new NET_DEVICE_EXPOSURE_CFG
            {
                byExposureMode = 0,        // 0=自动曝光
                byGainMode = 0             // 0=自动增益
            };
            return SetExposureConfig(cfg);
        }
        /// <summary>
        /// 设置图像参数（亮度、对比度等）
        /// </summary>
        public bool SetImageConfig(NET_DEVICE_IMAGE_CFG config) => SetConfig(DH_DEV_IMAGE_CFG, config);
        /// <summary>
        /// 设置白平衡参数
        /// </summary>
        public bool SetWhiteBalanceConfig(NET_DEVICE_WHITEBALANCE_CFG config) => SetConfig(DH_DEV_WHITEBALANCE_CFG, config);
        /// <summary>
        /// 设置编码参数（码流、分辨率等）
        /// </summary>
        public bool SetEncodeConfig(NET_DEVICE_ENCODE_CFG config) => SetConfig(DH_DEV_ENCODE_CFG, config);
        /// <summary>
        /// 设置网络参数
        /// </summary>
        public bool SetNetworkConfig(NET_DEVICE_NET_CFG config) => SetConfig(DH_DEV_NET_CFG, config);
        /// <summary>
        /// 设置 OSD 参数
        /// </summary>
        public bool SetOSDConfig(NET_DEVICE_OSD_CFG config) => SetConfig(DH_DEV_OSD_CFG, config);

        /// <summary>
        /// 通用设置配置方法（私有）
        /// </summary>
        private bool SetConfig<T>(int command, T config) where T : struct
        {
            if (!IsLoggedIn) return false;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr pBuff = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(config, pBuff, false);
                bool result = CLIENT_SetDevConfig(_loginId, command, pBuff, size, 5000);
                if (!result) LastError = CLIENT_GetLastError();
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(pBuff);
            }
        }

        /// <summary>
        /// 通用获取配置方法（私有）
        /// </summary>
        private bool GetConfig<T>(int command, ref T config) where T : struct
        {
            if (!IsLoggedIn) return false;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr pBuff = Marshal.AllocHGlobal(size);
            int bytesReturned = 0;
            try
            {
                bool result = CLIENT_GetDevConfig(_loginId, command, pBuff, size, ref bytesReturned, 5000);
                if (result && bytesReturned > 0)
                {
                    config = (T)Marshal.PtrToStructure(pBuff, typeof(T));
                }
                if (!result) LastError = CLIENT_GetLastError();
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(pBuff);
            }
        }

        #endregion

        #region ==================== 音频对讲 ====================

        /// <summary>
        /// 开始语音对讲
        /// </summary>
        /// <param name="audioParam">音频参数（采样率、编码格式等）</param>
        /// <returns>是否成功</returns>
        public bool StartTalk(NET_AUDIO_TALK_PARAM audioParam)
        {
            if (!IsLoggedIn) return false;
            _talkHandle = CLIENT_StartTalk(_loginId, ref audioParam, IntPtr.Zero);
            return _talkHandle != IntPtr.Zero;
        }

        /// <summary>
        /// 停止语音对讲
        /// </summary>
        public bool StopTalk()
        {
            if (_talkHandle == IntPtr.Zero) return false;
            bool result = CLIENT_StopTalk(_talkHandle);
            _talkHandle = IntPtr.Zero;
            return result;
        }

        /// <summary>
        /// 发送音频数据（对讲时调用）
        /// </summary>
        /// <param name="data">PCM 音频数据</param>
        /// <returns>是否成功</returns>
        public bool SendTalkData(byte[] data)
        {
            if (_talkHandle == IntPtr.Zero || data == null || data.Length == 0) return false;
            IntPtr pBuffer = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, pBuffer, data.Length);
                return CLIENT_TalkSendData(_talkHandle, pBuffer, data.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        #endregion

        #region ==================== 报警监听 ====================

        /// <summary>
        /// 订阅报警消息（报警回调已在 SDK 初始化时全局注册）
        /// </summary>
        public bool SubscribeAlarm()
        {
            if (!IsLoggedIn) return false;
            // V3 SDK 使用 CLIENT_StartListenEx（旧版 CLIENT_StartListen 签名不同）
            if (!CLIENT_StartListenEx(_loginId))
            {
                LastError = CLIENT_GetLastError();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取消订阅报警
        /// </summary>
        public bool UnsubscribeAlarm()
        {
            if (!IsLoggedIn) return false;
            return CLIENT_StopListen(_loginId);
        }

        // 全局断线回调（SDK 线程）：按登录句柄路由到对应实例
        private static void OnGlobalDisConnect(IntPtr lLoginID, string pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            List<DahuaCamera> snapshot;
            lock (s_instancesLock)
                snapshot = new List<DahuaCamera>(s_instances);

            foreach (DahuaCamera cam in snapshot)
            {
                if (cam._loginId == lLoginID)
                {
                    // 注意：本类可能在线程池线程构造（HCameraChannel 连接在后台线程进行），
                    // 此时 SynchronizationContext.Current 为 null，Post 会被静默跳过导致断线事件丢失。
                    // 无上下文时直接在 SDK 线程触发——订阅方（HCameraChannel）本身按线程安全设计
                    if (cam._syncContext != null)
                        cam._syncContext.Post(_ => cam.Disconnected?.Invoke(cam, EventArgs.Empty), null);
                    else
                        cam.Disconnected?.Invoke(cam, EventArgs.Empty);
                }
            }
        }

        // 全局报警消息回调（SDK 线程）：按登录句柄路由到对应实例
        private static void OnGlobalAlarmMessage(IntPtr lLoginID, IntPtr lCommand, IntPtr lTime,
            string sIp, string sDeviceName, IntPtr pBuf, int dwBufLen, IntPtr dwUser)
        {
            if (pBuf == IntPtr.Zero || dwBufLen <= 0) return;

            byte[] data = new byte[dwBufLen];
            Marshal.Copy(pBuf, data, 0, dwBufLen);

            List<DahuaCamera> snapshot;
            lock (s_instancesLock)
                snapshot = new List<DahuaCamera>(s_instances);

            foreach (DahuaCamera cam in snapshot)
            {
                if (cam._loginId == lLoginID)
                {
                    if (cam._syncContext != null)
                        cam._syncContext.Post(_ => cam.AlarmReceived?.Invoke(cam, new AlarmEventArgs(data)), null);
                    else
                        cam.AlarmReceived?.Invoke(cam, new AlarmEventArgs(data));
                }
            }
        }

        #endregion

        #region ==================== 录像下载 ====================

        /// <summary>
        /// 按时间下载录像文件
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="savePath">保存路径（含文件名）</param>
        /// <returns>下载句柄（用于查询进度或停止下载）</returns>
        public IntPtr DownloadRecord(int channel, DateTime startTime, DateTime endTime, string savePath)
        {
            if (!IsLoggedIn) return IntPtr.Zero;
            NET_TIME start = NET_TIME.FromDateTime(startTime);
            NET_TIME end = NET_TIME.FromDateTime(endTime);
            // nRecordFileType=0 表示全部录像类型；cbTimeDownPos 传 null（如需进度可后续扩展）
            IntPtr handle = CLIENT_DownloadByTime(_loginId, savePath, channel, 0, ref start, ref end, null, IntPtr.Zero);
            if (handle == IntPtr.Zero)
                LastError = CLIENT_GetLastError();
            return handle;
        }

        /// <summary>
        /// 停止录像下载
        /// </summary>
        public bool StopDownload(IntPtr downloadHandle)
        {
            if (downloadHandle == IntPtr.Zero) return false;
            return CLIENT_StopDownload(downloadHandle);
        }

        /// <summary>
        /// 获取下载进度（百分比）
        /// </summary>
        public int GetDownloadPos(IntPtr downloadHandle)
        {
            if (downloadHandle == IntPtr.Zero) return -1;
            return CLIENT_GetDownloadPos(downloadHandle);
        }

        #endregion

        #region ==================== 设备控制 ====================

        /// <summary>
        /// 重启设备
        /// </summary>
        public bool RebootDevice()
        {
            if (!IsLoggedIn) return false;
            return CLIENT_Reboot(_loginId);
        }

        /// <summary>
        /// 设置设备时间
        /// </summary>
        public bool SetDeviceTime(DateTime time)
        {
            if (!IsLoggedIn) return false;
            NET_TIME netTime = NET_TIME.FromDateTime(time);
            return CLIENT_SetDeviceTime(_loginId, ref netTime);
        }

        /// <summary>
        /// 获取设备时间
        /// </summary>
        public bool GetDeviceTime(out DateTime time)
        {
            time = DateTime.MinValue;
            if (!IsLoggedIn) return false;
            NET_TIME netTime = new NET_TIME();
            bool result = CLIENT_GetDeviceTime(_loginId, ref netTime);
            if (result) time = netTime.ToDateTime();
            return result;
        }

        /// <summary>
        /// 获取设备信息（字符串格式）
        /// </summary>
        public string GetDeviceInfo()
        {
            if (!IsLoggedIn) return string.Empty;
            if (DeviceInfo.sSerialNumber != null)
                return $"Device SN: {System.Text.Encoding.ASCII.GetString(DeviceInfo.sSerialNumber).TrimEnd('\0')}";
            return "Device SN: Unknown";
        }

        /// <summary>
        /// 获取最后一次错误码
        /// </summary>
        public int GetLastError()
        {
            return CLIENT_GetLastError();
        }

        #endregion

        #region ==================== 透明通道（串口透传） ====================

        /// <summary>
        /// 打开透明通道（串口透传），并注册回调
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="parity">校验位</param>
        /// <returns>是否成功</returns>
        public bool OpenTransCom(int channel, int baudRate, byte dataBits, byte stopBits, byte parity)
        {
            if (!IsLoggedIn) return false;
            // 构造串口参数结构体（可自行定义，此处简化）
            NET_TRANSCOM_CFG cfg = new NET_TRANSCOM_CFG
            {
                baudRate = baudRate,
                dataBits = dataBits,
                stopBits = stopBits,
                parity = parity
            };
            IntPtr pCfg = Marshal.AllocHGlobal(Marshal.SizeOf(cfg));
            Marshal.StructureToPtr(cfg, pCfg, false);
            bool result = CLIENT_OpenTransCom(_loginId, channel, pCfg);
            Marshal.FreeHGlobal(pCfg);
            if (result)
            {
                // 注册透明通道回调，确保能接收数据
                CLIENT_SetTransComCallBack(_transComCallback, IntPtr.Zero);
            }
            return result;
        }

        /// <summary>
        /// 关闭透明通道
        /// </summary>
        public bool CloseTransCom(int channel)
        {
            if (!IsLoggedIn) return false;
            return CLIENT_CloseTransCom(_loginId, channel);
        }

        /// <summary>
        /// 发送透明通道数据
        /// </summary>
        public bool SendTransComData(int channel, byte[] data)
        {
            if (!IsLoggedIn || data == null || data.Length == 0) return false;
            IntPtr pBuffer = Marshal.AllocHGlobal(data.Length);
            try
            {
                Marshal.Copy(data, 0, pBuffer, data.Length);
                return CLIENT_SendTransComData(_loginId, channel, pBuffer, data.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        // 透明通道回调处理（内部）
        private void OnTransComCallback(IntPtr lLoginID, int nChannel, IntPtr pBuffer, int dwBufSize, IntPtr dwUser)
        {
            byte[] data = new byte[dwBufSize];
            Marshal.Copy(pBuffer, data, 0, dwBufSize);
            _syncContext?.Post(_ => TransComDataReceived?.Invoke(this, new TransComEventArgs(nChannel, data)), null);
        }

        #endregion

        #region ==================== P/Invoke 声明 ====================

        // ==================== dhnetsdk.dll ====================
        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_Init", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_Init(DisConnectCallBack cbDisConnect, IntPtr dwUser);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_Cleanup", CallingConvention = CallingConvention.StdCall)]
        private static extern void CLIENT_Cleanup();

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_Login", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CLIENT_Login(string pchDVRIP, ushort wDVRPort,
            string pchUserName, string pchPassword, ref NET_DEVICEINFO lpDeviceInfo, ref int error);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_Logout", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_Logout(IntPtr lLoginID);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_RealPlay", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CLIENT_RealPlay(IntPtr lLoginID, int nChannelID, IntPtr hWnd);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_RealPlayEx", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CLIENT_RealPlayEx(IntPtr lLoginID, int nChannelID, IntPtr hWnd, int nStreamType);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StopRealPlay", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_StopRealPlay(IntPtr lRealHandle);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SetRealDataCallBackEx", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SetRealDataCallBackEx(IntPtr lRealHandle,
            RealDataCallBackEx cbRealData, IntPtr dwUser, int dwFlag);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_CapturePicture", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_CapturePicture(IntPtr lRealHandle, string pchPicFileName, int nQuality);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_CapturePictureToMemory", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_CapturePictureToMemory(IntPtr lRealHandle,
            IntPtr pBuffer, int nBufSize, ref int nSizeReturned);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DHPTZControlStart", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_DHPTZControlStart(IntPtr lLoginID, int nChannelID,
            string ptzCmd, int nSpeed);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DHPTZControlStop", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_DHPTZControlStop(IntPtr lLoginID, int nChannelID,
            string ptzCmd);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DHPTZControlEx", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_DHPTZControlEx(IntPtr lLoginID, int nChannelID,
            IntPtr pParam, int nParamSize, int waittime);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DHPTZPresetSet", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_DHPTZPresetSet(IntPtr lLoginID, int nChannelID, int nPresetIndex);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DHPTZPresetGoto", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_DHPTZPresetGoto(IntPtr lLoginID, int nChannelID, int nPresetIndex);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DHPTZPresetDelete", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_DHPTZPresetDelete(IntPtr lLoginID, int nChannelID, int nPresetIndex);

        // V3 SDK 订阅报警使用 CLIENT_StartListenEx（旧版 CLIENT_StartListen 参数不同）
        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StartListenEx", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_StartListenEx(IntPtr lLoginID);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StopListen", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_StopListen(IntPtr lLoginID);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SetDVRMessCallBack", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SetDVRMessCallBack(AlarmMessageCallBack cbMessageCallBack, IntPtr dwUser);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_GetDevConfig", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_GetDevConfig(IntPtr lLoginID, int nCommand,
            IntPtr lpParam, int nParamSize, ref int nBytesReturned, int waittime);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SetDevConfig", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SetDevConfig(IntPtr lLoginID, int nCommand,
            IntPtr lpParam, int nParamSize, int waittime);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SaveRealData", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SaveRealData(IntPtr lRealHandle, string pchFileName);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StopSaveRealData", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_StopSaveRealData(IntPtr lRealHandle);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StartTalk", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CLIENT_StartTalk(IntPtr lLoginID, ref NET_AUDIO_TALK_PARAM pAudioParam, IntPtr pUser);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StopTalk", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_StopTalk(IntPtr lTalkHandle);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_TalkSendData", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_TalkSendData(IntPtr lTalkHandle, IntPtr pBuffer, int nBufSize);

        // SDK 实际签名：时间参数为指针，且需要进度回调和用户数据
        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_DownloadByTime", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CLIENT_DownloadByTime(IntPtr lLoginID, string sSavedFileName, int nChannelId,
            int nRecordFileType, ref NET_TIME pstStartTime, ref NET_TIME pstStopTime,
            TimeDownPosCallBack cbTimeDownPos, IntPtr dwUser);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_StopDownload", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_StopDownload(IntPtr lFileHandle);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_GetDownloadPos", CallingConvention = CallingConvention.StdCall)]
        private static extern int CLIENT_GetDownloadPos(IntPtr lFileHandle);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_Reboot", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_Reboot(IntPtr lLoginID);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SetDeviceTime", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SetDeviceTime(IntPtr lLoginID, ref NET_TIME pDeviceTime);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_GetDeviceTime", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_GetDeviceTime(IntPtr lLoginID, ref NET_TIME pDeviceTime);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_GetLastError", CallingConvention = CallingConvention.StdCall)]
        private static extern int CLIENT_GetLastError();

        // 透明通道相关
        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_OpenTransCom", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_OpenTransCom(IntPtr lLoginID, int nChannel, IntPtr pConfig);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_CloseTransCom", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_CloseTransCom(IntPtr lLoginID, int nChannel);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SendTransComData", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SendTransComData(IntPtr lLoginID, int nChannel, IntPtr pBuffer, int nBufSize);

        [DllImport("dhnetsdk.dll", EntryPoint = "CLIENT_SetTransComCallBack", CallingConvention = CallingConvention.StdCall)]
        private static extern bool CLIENT_SetTransComCallBack(fTransComCallBack cbTransCom, IntPtr dwUser);

        // ==================== 播放解码库 dhplay.dll / play.dll ====================
        // 大华播放库：新版 SDK 文件名为 dhplay.dll，旧版为 play.dll，按程序目录实际存在的文件自动选择。
        // 实时流回调输出的是 H.264/H.265 压缩码流，必须经播放库解码后才能得到 YV12 原始帧。
        private static readonly bool s_useDhPlayLib = DetectPlayLib();

        /// <summary>DetectPlayLib 方法。</summary>
        private static bool DetectPlayLib()
        {
            try
            {
                string dir = HFromUI.HData.HAppData.AppPath;
                if (System.IO.File.Exists(System.IO.Path.Combine(dir, "dhplay.dll"))) return true;
                if (System.IO.File.Exists(System.IO.Path.Combine(dir, "play.dll"))) return false;
            }
            catch { }
            return true; // 缺省按新版 dhplay.dll
        }

        // PLAY_GetFreePort：获取空闲播放通道号
        [DllImport("dhplay.dll", EntryPoint = "PLAY_GetFreePort", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayGetFreePort(ref int nPort);
        [DllImport("play.dll", EntryPoint = "PLAY_GetFreePort", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayGetFreePort(ref int nPort);
        /// <summary>PlayGetFreePort 方法。</summary>
        private static bool PlayGetFreePort(ref int nPort)
            => s_useDhPlayLib ? DH_PlayGetFreePort(ref nPort) : OLD_PlayGetFreePort(ref nPort);

        // PLAY_OpenStream：打开流（pFileHeadBuf 为流头缓冲，可为 NULL；nBufPoolSize 为缓冲池大小）
        [DllImport("dhplay.dll", EntryPoint = "PLAY_OpenStream", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayOpenStream(int nPort, IntPtr pFileHeadBuf, uint nSize, uint nBufPoolSize);
        [DllImport("play.dll", EntryPoint = "PLAY_OpenStream", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayOpenStream(int nPort, IntPtr pFileHeadBuf, uint nSize, uint nBufPoolSize);
        /// <summary>PlayOpenStream 方法。</summary>
        private static bool PlayOpenStream(int nPort, IntPtr pFileHeadBuf, uint nSize, uint nBufPoolSize)
            => s_useDhPlayLib ? DH_PlayOpenStream(nPort, pFileHeadBuf, nSize, nBufPoolSize)
                              : OLD_PlayOpenStream(nPort, pFileHeadBuf, nSize, nBufPoolSize);

        // PLAY_InputData：向播放库送入压缩码流
        [DllImport("dhplay.dll", EntryPoint = "PLAY_InputData", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayInputData(int nPort, IntPtr pBuf, int nSize);
        [DllImport("play.dll", EntryPoint = "PLAY_InputData", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayInputData(int nPort, IntPtr pBuf, int nSize);
        /// <summary>PlayInputData 方法。</summary>
        private static bool PlayInputData(int nPort, IntPtr pBuf, int nSize)
            => s_useDhPlayLib ? DH_PlayInputData(nPort, pBuf, nSize)
                              : OLD_PlayInputData(nPort, pBuf, nSize);

        // PLAY_Play：开始播放（hWnd 为 NULL 时仅解码不显示，解码帧通过解码回调输出）
        [DllImport("dhplay.dll", EntryPoint = "PLAY_Play", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayPlay(int nPort, IntPtr hWnd);
        [DllImport("play.dll", EntryPoint = "PLAY_Play", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayPlay(int nPort, IntPtr hWnd);
        /// <summary>PlayPlay 方法。</summary>
        private static bool PlayPlay(int nPort, IntPtr hWnd)
            => s_useDhPlayLib ? DH_PlayPlay(nPort, hWnd) : OLD_PlayPlay(nPort, hWnd);

        // PLAY_SetDecCallBackEx：注册解码后数据回调（fDecCBFun）
        [DllImport("dhplay.dll", EntryPoint = "PLAY_SetDecCallBackEx", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlaySetDecCallBackEx(int nPort, DecFrameCallBack cbDecCBFun, IntPtr nUser);
        [DllImport("play.dll", EntryPoint = "PLAY_SetDecCallBackEx", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlaySetDecCallBackEx(int nPort, DecFrameCallBack cbDecCBFun, IntPtr nUser);
        /// <summary>PlaySetDecCallBackEx 方法。</summary>
        private static bool PlaySetDecCallBackEx(int nPort, DecFrameCallBack cbDecCBFun, IntPtr nUser)
            => s_useDhPlayLib ? DH_PlaySetDecCallBackEx(nPort, cbDecCBFun, nUser)
                              : OLD_PlaySetDecCallBackEx(nPort, cbDecCBFun, nUser);

        // PLAY_Stop：停止播放
        [DllImport("dhplay.dll", EntryPoint = "PLAY_Stop", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayStop(int nPort);
        [DllImport("play.dll", EntryPoint = "PLAY_Stop", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayStop(int nPort);
        /// <summary>PlayStop 方法。</summary>
        private static bool PlayStop(int nPort)
            => s_useDhPlayLib ? DH_PlayStop(nPort) : OLD_PlayStop(nPort);

        // PLAY_CloseStream：关闭流
        [DllImport("dhplay.dll", EntryPoint = "PLAY_CloseStream", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayCloseStream(int nPort);
        [DllImport("play.dll", EntryPoint = "PLAY_CloseStream", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayCloseStream(int nPort);
        /// <summary>PlayCloseStream 方法。</summary>
        private static bool PlayCloseStream(int nPort)
            => s_useDhPlayLib ? DH_PlayCloseStream(nPort) : OLD_PlayCloseStream(nPort);

        [DllImport("dhplay.dll", EntryPoint = "PLAY_ReleasePort", CallingConvention = CallingConvention.StdCall)]
        private static extern bool DH_PlayReleasePort(int nPort);
        [DllImport("play.dll", EntryPoint = "PLAY_ReleasePort", CallingConvention = CallingConvention.StdCall)]
        private static extern bool OLD_PlayReleasePort(int nPort);
        /// <summary>PlayReleasePort 方法。</summary>
        private static bool PlayReleasePort(int nPort)
            => s_useDhPlayLib ? DH_PlayReleasePort(nPort) : OLD_PlayReleasePort(nPort);

        #endregion

        #region ==================== 委托、结构体、枚举 ====================

        // 实时流回调（fRealDataCallBackEx，共 6 个参数；与 dhnetsdk.h 一致）。
        // 注意：第 5 个参数 lParam 为 LONG(4字节)，第 6 个参数 dwUser 为 LDWORD(指针宽度)；
        // 旧代码只有 5 个参数且 dwUser 声明为 int，参数个数/宽度不匹配，x86 下会导致回调栈失衡。
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RealDataCallBackEx(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer,
            uint dwBufSize, int lParam, IntPtr dwUser);

        // 播放库解码回调（fDecCBFun，dhplay.h）：pBuf 为解码后的 YV12 数据，pFrameInfo 含宽高/帧类型
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void DecFrameCallBack(int nPort, IntPtr pBuf, int nSize,
            IntPtr pFrameInfo, IntPtr pReserved1, int nReserved2);

        // 播放库帧信息（dhplay.h 中 FRAME_INFO）
        [StructLayout(LayoutKind.Sequential)]
        private struct PLAY_FRAME_INFO
        {
            public int nWidth;
            public int nHeight;
            public int nStamp;
            public int nType;        // 帧类型：3 = T_YV12
            public int nFrameRate;
            public uint dwFrameNum;
        }

        // 报警消息回调（fMessCallBack，V3 签名共 8 个参数；
        // 原代码仅 4 个参数且把 lCommand 当作 pBuf 指针使用，报警触发时会导致访问冲突）
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void AlarmMessageCallBack(IntPtr lLoginID, IntPtr lCommand, IntPtr lTime,
            [MarshalAs(UnmanagedType.LPStr)] string sIp,
            [MarshalAs(UnmanagedType.LPStr)] string sDeviceName,
            IntPtr pBuf, int dwBufLen, IntPtr dwUser);

        // 断线回调（fDisConnect，通过 CLIENT_Init 注册）
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DisConnectCallBack(IntPtr lLoginID,
            [MarshalAs(UnmanagedType.LPStr)] string pchDVRIP, int nDVRPort, IntPtr dwUser);

        // 录像下载进度回调（fTimeDownPosCallBack）
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TimeDownPosCallBack(IntPtr lLoginID, IntPtr lFileHandle,
            uint dwTotalSize, uint dwDownLoadSize, IntPtr dwUser);

        // 透明通道回调
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void fTransComCallBack(IntPtr lLoginID, int nChannel, IntPtr pBuffer, int dwBufSize, IntPtr dwUser);

        // 设备信息结构体（与 dhnetsdk.h 中 NET_DEVICEINFO 一致；
        // 原代码字段为虚构布局，虽然总大小接近，但通道数等字段含义全部错位）
        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICEINFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] sSerialNumber;     // 序列号
            public byte byAlarmInPortNum;    // 报警输入口数
            public byte byAlarmOutPortNum;   // 报警输出口数
            public byte byDiskNum;           // 硬盘数
            public byte byDVRType;           // 设备类型
            public byte byChanNum;           // 模拟通道数
            public byte byStartChan;         // 起始模拟通道号
            public byte byAudioChanNum;      // 语音通道数
            public byte byIPChanNum;         // 数字通道数（低 8 位）
            public byte byZeroChanNum;       // 零通道数
            public byte byMainProto;         // 主版本
            public byte bySubProto;          // 次版本
            public byte bySupport;           // 能力位
            public byte bySupport1;
            public byte bySupport2;
            public ushort wDevType;          // 设备型号
            public byte bySupport3;
            public byte byMultiStreamProto;  // 多码流能力
            public byte byStartDChan;        // 起始数字通道号
            public byte byStartDTalkChan;    // 起始数字对讲通道号
            public byte byHighDChanNum;      // 数字通道数高 8 位
            public byte bySupport4;
            public byte byLanguageType;      // 语言
            public byte byVoiceInChanNum;    // 音频输入通道数
            public byte byStartVoiceInChanNo;// 起始音频输入通道号
            public byte bySupport5;
            public byte bySupport6;
            public byte byMirrorChanNum;     // 镜像通道数
            public ushort wStartMirrorChanNo;// 起始镜像通道号
            public byte bySupport7;
            public byte byRes2;
        }

        // 时间结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct NET_TIME
        {
            public int dwYear;
            public int dwMonth;
            public int dwDay;
            public int dwHour;
            public int dwMinute;
            public int dwSecond;

            /// <summary>从 DateTime 创建实例。</summary>
            public static NET_TIME FromDateTime(DateTime dt)
            {
                return new NET_TIME
                {
                    dwYear = dt.Year,
                    dwMonth = dt.Month,
                    dwDay = dt.Day,
                    dwHour = dt.Hour,
                    dwMinute = dt.Minute,
                    dwSecond = dt.Second
                };
            }

            /// <summary>转换为 DateTime。</summary>
            public DateTime ToDateTime()
            {
                return new DateTime(dwYear, dwMonth, dwDay, dwHour, dwMinute, dwSecond);
            }
        }

        // ==================== 配置结构体（示例，请对照 SDK 头文件完善） ====================

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_EXPOSURE_CFG
        {
            public byte byExposureMode;         // 曝光模式：0-自动，1-手动，2-光圈优先，3-快门优先
            public byte byGainMode;             // 增益模式：0-自动，1-手动
            public byte byIrisMode;             // 光圈模式：0-自动，1-手动
            public byte byReserved1;
            public uint dwMinExposureTime;      // 最小曝光时间（微秒）
            public uint dwMaxExposureTime;      // 最大曝光时间（微秒）
            public uint dwExposureTime;         // 当前曝光时间（微秒）
            public uint dwMinGain;              // 最小增益
            public uint dwMaxGain;              // 最大增益
            public uint dwGain;                 // 当前增益
            public byte byExposureMode2;        // 扩展曝光模式（可选）
            public byte byReserved2;
            public ushort wReserved3;
            public uint dwReserved4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_GAIN_CFG
        {
            public byte byGainMode;             // 增益模式：0-自动，1-手动
            public byte byReserved1;
            public ushort wReserved2;
            public uint dwMinGain;              // 最小增益值
            public uint dwMaxGain;              // 最大增益值
            public uint dwGain;                 // 当前增益值
            public byte byGainMode2;            // 扩展增益模式
            public byte byReserved3;
            public ushort wReserved4;
            public uint dwReserved5;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_IMAGE_CFG
        {
            public byte byContrast;             // 对比度 0-100
            public byte byBrightness;           // 亮度 0-100
            public byte bySaturation;           // 饱和度 0-100
            public byte bySharpness;            // 锐度 0-100
            public byte byHue;                  // 色调 0-100
            public byte byReserved1;
            public ushort wReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_WHITEBALANCE_CFG
        {
            public byte byWhiteBalanceMode;      // 白平衡模式：0-自动，1-手动
            public byte byReserved1;
            public ushort wReserved2;
            public uint dwRGain;                 // 红色增益（手动时有效）
            public uint dwBGain;                 // 蓝色增益（手动时有效）
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_ENCODE_CFG
        {
            public byte byCompression;           // 编码类型：0-H.264, 1-MJPEG 等
            public byte byResolution;            // 分辨率枚举
            public byte byBitrateControl;        // 码率控制
            public byte byReserved1;
            public uint dwBitrate;               // 码率
            public uint dwFrameRate;             // 帧率
            public byte byQuality;               // 图像质量
            public byte byReserved2;
            public ushort wReserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_NET_CFG
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] byIP;                  // IP 地址
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] bySubNetMask;          // 子网掩码
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] byGateway;             // 网关
            public ushort wPort;                 // 端口
            public byte byReserved1;
            public byte byReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_DEVICE_OSD_CFG
        {
            public byte byEnable;                // 是否显示 OSD
            public byte byReserved1;
            public ushort wReserved2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szOSDText;             // OSD 文本
            public byte byOSDType;               // OSD 类型
            public byte byReserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NET_AUDIO_TALK_PARAM
        {
            public int nAudioFormat;            // 音频格式（如 PCM）
            public int nSampleRate;             // 采样率
            public int nBitDepth;               // 位深
            public int nChannel;                // 通道数
        }

        // 透明通道串口配置结构体（示例）
        [StructLayout(LayoutKind.Sequential)]
        public struct NET_TRANSCOM_CFG
        {
            public int baudRate;                // 波特率
            public byte dataBits;               // 数据位
            public byte stopBits;               // 停止位
            public byte parity;                 // 校验位
            public byte reserved;
        }

        // 云台命令枚举
        public enum EM_PTZ_CMD
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            ZOOM_IN,
            ZOOM_OUT,
            FOCUS_NEAR,
            FOCUS_FAR,
            APERTURE_OPEN,
            APERTURE_CLOSE,
            AUTO
        }

        // 配置命令常量（请根据 SDK 文档调整，以下为示例值）
        private const int DH_DEV_EXPOSURE_CFG = 0x0010;    // 可能需要修改
        private const int DH_DEV_GAIN_CFG = 0x0012;        // 可能需要修改
        private const int DH_DEV_IMAGE_CFG = 0x0013;
        private const int DH_DEV_WHITEBALANCE_CFG = 0x0014;
        private const int DH_DEV_ENCODE_CFG = 0x0015;
        private const int DH_DEV_NET_CFG = 0x0002;
        private const int DH_DEV_OSD_CFG = 0x000D;

        // 报警事件参数类
        public class AlarmEventArgs : EventArgs
        {
            /// <summary>RawData 成员。</summary>
            public byte[] RawData { get; }
            public AlarmEventArgs(byte[] data)
            {
                RawData = data;
            }
        }

        // 透明通道数据事件参数类
        public class TransComEventArgs : EventArgs
        {
            /// <summary>Channel 成员。</summary>
            public int Channel { get; }
            /// <summary>数据。</summary>
            public byte[] Data { get; }
            public TransComEventArgs(int channel, byte[] data)
            {
                Channel = channel;
                Data = data;
            }
        }

        #endregion
    }

 
}
