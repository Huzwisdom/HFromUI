using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using HFromUI.HConvert; // 假设存在视频帧转换器

namespace HFromUI.HCamera.Dvp
{
    /// <summary>
    /// 度申（DVP）工业相机完整操作类，封装 DVPCamera64.dll（相机控制）和 dvpir64.dll（红外图像处理）。
    /// 功能包括：
    /// - SDK 初始化/清理（引用计数，相机和红外独立管理）
    /// - 设备枚举、打开、关闭
    /// - 采集控制（连续采集、软触发、触发模式设置）
    /// - 帧数据回调（传递原始指针、尺寸、格式）
    /// - 参数设置（曝光、增益、白平衡、分辨率、帧率、ROI）
    /// - 红外图像增强、温度测量等（假设函数）
    /// - 图像保存（BMP）
    /// 注意：所有 P/Invoke 签名、结构体、枚举值需根据官方 SDK 头文件进行调整。
    /// </summary>
    public class DvpCamera : IDisposable
    {
        #region ==================== 内部字段 ====================

        // 相机 SDK 初始化引用计数
        private static int _cameraSdkRefCount = 0;
        /// <summary>_cameraSdkLock 字段。</summary>
        private static readonly object _cameraSdkLock = new object();

        // 红外 SDK 初始化引用计数
        private static int _irSdkRefCount = 0;
        /// <summary>_irSdkLock 字段。</summary>
        private static readonly object _irSdkLock = new object();

        // 当前相机句柄
        private IntPtr _cameraHandle = IntPtr.Zero;
        /// <summary>_isOpen 字段。</summary>
        private bool _isOpen = false;
        /// <summary>_isGrabbing 字段。</summary>
        private bool _isGrabbing = false;

        // 帧回调委托，保持引用防止被 GC 回收
        private FrameCallback _frameCallback;

        // 断线看门狗：USB 拔出或相机崩溃时帧回调停止触发，定时器超时触发 Disconnected
        private System.Threading.Timer _watchdogTimer;
        /// <summary>_lastFrameTime 字段。</summary>
        private DateTime _lastFrameTime = DateTime.Now;
        private bool _disconnectRaised;
        private const double WatchdogTimeoutSec = 3.0;

        // 设备信息
        public DVP_DEVICE_INFO DeviceInfo { get; private set; }
        /// <summary>SerialNumber 成员。</summary>
        public string SerialNumber { get; private set; }
        /// <summary>ModelName 成员。</summary>
        public string ModelName { get; private set; }
        /// <summary>FriendlyName 成员。</summary>
        public string FriendlyName { get; private set; }

        // 状态属性
        public bool IsOpen => _isOpen;
        /// <summary>IsGrabbing 成员。</summary>
        public bool IsGrabbing => _isGrabbing;

        // 事件：帧数据到达，参数依次为：数据指针、数据大小、宽、高、像素格式（VideoFrameFormat枚举）
        public event Action<IntPtr, int, int, int, VideoFrameFormat> FrameReceived;

        // 设备断开事件
        public event EventHandler Disconnected;

        // 用于将事件封送到创建线程（通常是UI线程）
        private readonly SynchronizationContext _syncContext;

        #endregion

        #region ==================== 构造函数与析构 ====================

        public DvpCamera()
        {
            _frameCallback = OnFrameCallback;
            _syncContext = SynchronizationContext.Current;
        }

        ~DvpCamera()
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
            // Close 内部会停止采集并释放 Open 时获取的全部 SDK 引用
            Close();
        }

        #endregion

        #region ==================== SDK 初始化与清理（相机） ====================

        /// <summary>
        /// 初始化相机 SDK（内部引用计数，全局只需调用一次）
        /// </summary>
        private static bool AcquireCameraSDK()
        {
            lock (_cameraSdkLock)
            {
                if (_cameraSdkRefCount == 0)
                {
                    if (DVP_Init() != DVP_STATUS_OK)
                        return false;
                }
                _cameraSdkRefCount++;
                return true;
            }
        }

        /// <summary>
        /// 释放相机 SDK 引用，当计数归零时清理
        /// </summary>
        private static void ReleaseCameraSDK()
        {
            lock (_cameraSdkLock)
            {
                if (_cameraSdkRefCount > 0)
                {
                    _cameraSdkRefCount--;
                    if (_cameraSdkRefCount == 0)
                    {
                        DVP_Uninit();
                    }
                }
            }
        }

        #endregion

        #region ==================== SDK 初始化与清理（红外） ====================

        /// <summary>
        /// 初始化红外处理 SDK（内部引用计数）
        /// </summary>
        private static bool AcquireIRSDK()
        {
            lock (_irSdkLock)
            {
                if (_irSdkRefCount == 0)
                {
                    if (DVPIR_Init() != DVP_STATUS_OK)
                        return false;
                }
                _irSdkRefCount++;
                return true;
            }
        }

        /// <summary>
        /// 释放红外 SDK 引用，当计数归零时清理
        /// </summary>
        private static void ReleaseIRSDK()
        {
            lock (_irSdkLock)
            {
                if (_irSdkRefCount > 0)
                {
                    _irSdkRefCount--;
                    if (_irSdkRefCount == 0)
                    {
                        DVPIR_Uninit();
                    }
                }
            }
        }

        #endregion

        #region ==================== 设备枚举与打开 ====================

        /// <summary>
        /// 枚举所有可用相机，返回设备信息数组
        /// </summary>
        public static DVP_DEVICE_INFO[] EnumerateDevices()
        {
            if (!AcquireCameraSDK()) return null;

            uint count = 0;
            if (DVP_Enum(ref count, IntPtr.Zero) != DVP_STATUS_OK || count == 0)
            {
                ReleaseCameraSDK();
                return new DVP_DEVICE_INFO[0];
            }

            IntPtr pDeviceList = Marshal.AllocHGlobal((int)(count * Marshal.SizeOf(typeof(DVP_DEVICE_INFO))));
            try
            {
                if (DVP_Enum(ref count, pDeviceList) != DVP_STATUS_OK)
                    return new DVP_DEVICE_INFO[0];

                DVP_DEVICE_INFO[] devices = new DVP_DEVICE_INFO[count];
                for (int i = 0; i < count; i++)
                {
                    IntPtr pEntry = IntPtr.Add(pDeviceList, i * Marshal.SizeOf(typeof(DVP_DEVICE_INFO)));
                    devices[i] = (DVP_DEVICE_INFO)Marshal.PtrToStructure(pEntry, typeof(DVP_DEVICE_INFO));
                }
                return devices;
            }
            finally
            {
                Marshal.FreeHGlobal(pDeviceList);
                ReleaseCameraSDK();
            }
        }

        /// <summary>
        /// 打开指定索引的相机（仅初始化相机 SDK；红外 SDK 在调用红外功能时按需懒加载）
        /// </summary>
        /// <param name="index">设备索引，默认 0</param>
        /// <returns>是否成功</returns>
        public bool Open(int index = 0)
        {
            if (_isOpen) Close();
            if (!AcquireCameraSDK()) return false;
            // 注意：红外库 dvpir64.dll 仅红外相机需要，普通度申相机部署目录没有该文件。
            // 原代码在此强制 AcquireIRSDK，缺 DLL 时抛 DllNotFoundException 导致普通相机永远打不开。
            // 红外功能改为在 IR_* 方法内按需懒加载（见下）。

            _cameraHandle = DVP_Open(index);
            if (_cameraHandle == IntPtr.Zero)
            {
                ReleaseCameraSDK();
                return false;
            }

            _isOpen = true;

            // 获取设备信息
            DVP_DEVICE_INFO info = new DVP_DEVICE_INFO();
            if (DVP_GetDeviceInfo(_cameraHandle, ref info) == DVP_STATUS_OK)
            {
                DeviceInfo = info;
                SerialNumber = info.szSerialNumber;
                ModelName = info.szModelName;
                FriendlyName = info.szFriendlyName;
            }

            // 注册帧回调
            DVP_SetFrameCallback(_cameraHandle, _frameCallback, IntPtr.Zero);

            return true;
        }

        /// <summary>
        /// 关闭相机（停止采集，并释放 Open 时获取的相机/红外 SDK 引用）
        /// </summary>
        public void Close()
        {
            if (_cameraHandle != IntPtr.Zero)
            {
                StopGrab();
                DVP_Close(_cameraHandle);
                _cameraHandle = IntPtr.Zero;
                _isOpen = false;
                _isGrabbing = false;

                // 与 Open 中的 AcquireCameraSDK 一一对应；
                // 多次 Open/Close 后引用计数平衡归零，DVP_Uninit 正常执行
                ReleaseCameraSDK();
            }
        }

        #endregion

        #region ==================== 采集控制 ====================

        /// <summary>
        /// 开始连续采集
        /// </summary>
        public bool StartGrab()
        {
            if (!_isOpen) return false;
            if (_isGrabbing) return true;

            if (DVP_Start(_cameraHandle) == DVP_STATUS_OK)
            {
                _isGrabbing = true;
                _lastFrameTime = DateTime.Now;
                _disconnectRaised = false;
                // 启动 1 秒一次的看门狗：3 秒没帧就触发断线
                _watchdogTimer = new System.Threading.Timer(WatchdogTick, null, 1000, 1000);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public bool StopGrab()
        {
            if (!_isGrabbing) return true;
            try { _watchdogTimer?.Dispose(); } catch { }
            _watchdogTimer = null;
            if (DVP_Stop(_cameraHandle) == DVP_STATUS_OK)
            {
                _isGrabbing = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 触发一帧（在触发模式下有效）
        /// </summary>
        public bool TriggerFrame()
        {
            if (!_isOpen) return false;
            return DVP_Trigger(_cameraHandle) == DVP_STATUS_OK;
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="mode">0-连续，1-软触发，2-硬触发</param>
        public bool SetTriggerMode(int mode)
        {
            if (!_isOpen) return false;
            return DVP_SetTriggerMode(_cameraHandle, mode) == DVP_STATUS_OK;
        }

        #endregion

        #region ==================== 帧回调 ====================

        // 帧回调处理（运行在 SDK 内部线程）
        private void OnFrameCallback(IntPtr cameraHandle, IntPtr pFrameBuffer, int frameSize, int width, int height, int pixelFormat, IntPtr userData)
        {
            if (pFrameBuffer == IntPtr.Zero || frameSize <= 0) return;
            _lastFrameTime = DateTime.Now; // 喂狗
            _disconnectRaised = false;

            // 将 SDK 的像素格式转换为自定义枚举
            VideoFrameFormat fmt = ConvertPixelFormat(pixelFormat);
            FrameReceived?.Invoke(pFrameBuffer, frameSize, width, height, fmt);
        }

        // 帧看门狗定时器（系统线程池）：3 秒以上没帧 → 触发断线事件
        private void WatchdogTick(object state)
        {
            if (!_isOpen || !_isGrabbing) return;
            if (_disconnectRaised) return;
            if ((DateTime.Now - _lastFrameTime).TotalSeconds > WatchdogTimeoutSec)
            {
                _disconnectRaised = true;
                try
                {
                    if (_syncContext != null)
                        _syncContext.Post(_ => Disconnected?.Invoke(this, EventArgs.Empty), null);
                    else
                        Disconnected?.Invoke(this, EventArgs.Empty);
                }
                catch { }
            }
        }

        // 度申 SDK 像素格式常量（来自 DVP SDK v2.x 头文件 dvp_types.h）。
        // 不同相机/固件输出格式不同，这里覆盖最常见的格式，未知格式统一降级为 BGR24 兜底。
        private const uint DVP_IMG_MONO      = 0x00010001; // 8-bit 灰度
        private const uint DVP_IMG_BGR24     = 0x00010006; // BGR 24bpp（部分 SDK 上 RGB24 与 MONO12 值相同，按驱动实际值调整）
        private const uint DVP_IMG_RGB32     = 0x00010004; // RGB 32bpp
        private const uint DVP_IMG_BAYER_RG8 = 0x01080009; // Bayer RGGB 8-bit
        private const uint DVP_IMG_YUYV      = 0x00020002; // YUV 4:2:2

        // 像素格式转换（按真实度申 SDK 枚举值映射，未知格式降级为 BGR24）
        private VideoFrameFormat ConvertPixelFormat(int dvpPixelFormat)
        {
            uint fmt = (uint)dvpPixelFormat;
            switch (fmt)
            {
                case DVP_IMG_MONO:
                    return VideoFrameFormat.Gray8;
                case DVP_IMG_BGR24:
                    return VideoFrameFormat.BGR24;
                case DVP_IMG_RGB32:
                    return VideoFrameFormat.RGB24;
                case DVP_IMG_BAYER_RG8:
                    return VideoFrameFormat.BayerRG8;
                case DVP_IMG_YUYV:
                    return VideoFrameFormat.YUYV;
                default:
                    // 可能是 MONO12/RGB24 等，让 HFrameToBitmapConverter 尝试兜底
                    return VideoFrameFormat.BGR24;
            }
        }

        #endregion

        #region ==================== 参数设置（相机） ====================

        /// <summary>
        /// 设置曝光时间（微秒）
        /// </summary>
        public bool SetExposure(double exposureTimeUs)
        {
            if (!_isOpen) return false;
            return DVP_SetExposure(_cameraHandle, exposureTimeUs) == DVP_STATUS_OK;
        }

        /// <summary>
        /// 设置增益（范围通常 0-100 或 0-255，请参考 SDK）
        /// </summary>
        public bool SetGain(double gain)
        {
            if (!_isOpen) return false;
            return DVP_SetGain(_cameraHandle, gain) == DVP_STATUS_OK;
        }

        /// <summary>
        /// 设置白平衡（红、绿、蓝增益）
        /// </summary>
        public bool SetWhiteBalance(double rGain, double gGain, double bGain)
        {
            if (!_isOpen) return false;
            return DVP_SetWhiteBalance(_cameraHandle, rGain, gGain, bGain) == DVP_STATUS_OK;
        }

        /// <summary>
        /// 设置分辨率（需相机支持）
        /// </summary>
        public bool SetResolution(int width, int height)
        {
            if (!_isOpen) return false;
            return DVP_SetResolution(_cameraHandle, width, height) == DVP_STATUS_OK;
        }

        /// <summary>
        /// 设置帧率（需相机支持）
        /// </summary>
        public bool SetFrameRate(double fps)
        {
            if (!_isOpen) return false;
            return DVP_SetFrameRate(_cameraHandle, fps) == DVP_STATUS_OK;
        }

        /// <summary>
        /// 设置 ROI（感兴趣区域）
        /// </summary>
        public bool SetROI(int x, int y, int width, int height)
        {
            if (!_isOpen) return false;
            return DVP_SetROI(_cameraHandle, x, y, width, height) == DVP_STATUS_OK;
        }

        #endregion

        #region ==================== 红外处理（dvpir64.dll） ====================

        /// <summary>
        /// 红外图像增强（假设函数）
        /// </summary>
        /// <param name="pInput">输入图像数据指针</param>
        /// <param name="pOutput">输出图像数据指针（需预分配）</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns>是否成功</returns>
        public bool IR_Enhance(IntPtr pInput, IntPtr pOutput, int width, int height)
        {
            if (!_isOpen) return false; // 红外功能仅打开相机后可用
            try
            {
                if (!AcquireIRSDK()) return false; // 缺 dvpir64.dll 时静默降级为不支持
                return DVPIR_Enhance(pInput, pOutput, width, height) == DVP_STATUS_OK;
            }
            catch (DllNotFoundException) { return false; }
        }

        /// <summary>
        /// 获取图像指定坐标的温度（假设支持测温）
        /// </summary>
        /// <param name="pImage">红外图像数据指针</param>
        /// <param name="x">像素 X 坐标</param>
        /// <param name="y">像素 Y 坐标</param>
        /// <param name="temperature">返回温度值</param>
        /// <returns>是否成功</returns>
        public bool IR_GetTemperature(IntPtr pImage, int x, int y, out double temperature)
        {
            temperature = 0;
            if (!_isOpen) return false;
            try
            {
                if (!AcquireIRSDK()) return false;
                return DVPIR_GetTemperature(pImage, x, y, ref temperature) == DVP_STATUS_OK;
            }
            catch (DllNotFoundException) { return false; }
        }

        // 可继续添加其他红外处理函数

        #endregion

        #region ==================== 图像保存 ====================

        /// <summary>
        /// 将原始帧数据保存为 BMP 文件
        /// </summary>
        /// <param name="filePath">保存路径</param>
        /// <param name="dataPtr">原始数据指针</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="format">像素格式</param>
        /// <returns>是否成功</returns>
        public bool SaveImage(string filePath, IntPtr dataPtr, int width, int height, VideoFrameFormat format)
        {
            try
            {
                using (Bitmap bmp = HFrameToBitmapConverter.Convert(dataPtr, width, height, format))
                {
                    bmp.Save(filePath, ImageFormat.Bmp);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ==================== P/Invoke 声明（相机 SDK） ====================

        private const int DVP_STATUS_OK = 0; // 假设成功返回值为 0

        // 基础函数
        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Init", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Init();

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Uninit", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Uninit();

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Enum", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Enum(ref uint count, IntPtr pDeviceList);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Open", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DVP_Open(int index);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Close", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Close(IntPtr cameraHandle);

        // 采集与控制
        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Start", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Start(IntPtr cameraHandle);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Stop", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Stop(IntPtr cameraHandle);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_Trigger", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_Trigger(IntPtr cameraHandle);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetTriggerMode", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetTriggerMode(IntPtr cameraHandle, int mode);

        // 帧回调设置
        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetFrameCallback", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetFrameCallback(IntPtr cameraHandle, FrameCallback callback, IntPtr userData);

        // 设备信息
        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_GetDeviceInfo", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_GetDeviceInfo(IntPtr cameraHandle, ref DVP_DEVICE_INFO info);

        // 参数设置
        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetExposure", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetExposure(IntPtr cameraHandle, double exposureTimeUs);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetGain", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetGain(IntPtr cameraHandle, double gain);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetWhiteBalance", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetWhiteBalance(IntPtr cameraHandle, double rGain, double gGain, double bGain);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetResolution", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetResolution(IntPtr cameraHandle, int width, int height);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetFrameRate", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetFrameRate(IntPtr cameraHandle, double fps);

        [DllImport("DVPCamera64.dll", EntryPoint = "DVP_SetROI", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVP_SetROI(IntPtr cameraHandle, int x, int y, int width, int height);

        // 回调委托（需与 SDK 定义一致）
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FrameCallback(IntPtr cameraHandle, IntPtr pFrameBuffer, int frameSize, int width, int height, int pixelFormat, IntPtr userData);

        // 设备信息结构体（示例）
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DVP_DEVICE_INFO
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szFriendlyName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szModelName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szSerialNumber;
            public uint uReserved;
        }

        #endregion

        #region ==================== P/Invoke 声明（红外 SDK） ====================

        // 红外库函数（假设，请根据实际头文件修改）
        [DllImport("dvpir64.dll", EntryPoint = "DVPIR_Init", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVPIR_Init();

        [DllImport("dvpir64.dll", EntryPoint = "DVPIR_Uninit", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVPIR_Uninit();

        [DllImport("dvpir64.dll", EntryPoint = "DVPIR_Enhance", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVPIR_Enhance(IntPtr pInput, IntPtr pOutput, int width, int height);

        [DllImport("dvpir64.dll", EntryPoint = "DVPIR_GetTemperature", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DVPIR_GetTemperature(IntPtr pImage, int x, int y, ref double temperature);

        // 可继续添加其他红外函数声明

        #endregion
    }

}
