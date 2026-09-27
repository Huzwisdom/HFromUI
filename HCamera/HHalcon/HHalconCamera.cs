using HalconDotNet;
using System;
using System.Runtime.InteropServices;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 图像采集完整封装（open_framegrabber / grab_image / grab_image_async / close_framegrabber）：
    /// 支持 GigE Vision（GigEVision2）、USB3 Vision（USB3Vision）、GenICam GenTL（GenICamTL）、
    /// DirectShow/MediaFoundation 以及 HALCON 自带 File 虚拟采集等全部已安装的 HALCON 采集接口；
    /// 提供同步抓图、异步抓图（maxDelay 超时）、连续采集回调（transfer_end/exception）、
    /// 相机参数读写（曝光/增益/触发模式等）与接口/设备枚举。
    /// </summary>
    public class HHalconCamera : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>采集句柄（open_framegrabber 返回）；未打开为空元组。</summary>
        public HTuple AcqHandle { get; private set; } = new HTuple();

        /// <summary>采集接口名（如 GigEVision2、USB3Vision、GenICamTL）。</summary>
        public string InterfaceName { get; private set; } = string.Empty;

        /// <summary>设备号/设备串（open_framegrabber 的 Device 参数）。</summary>
        public string Device { get; private set; } = string.Empty;

        /// <summary>相机类型（cameraType，如 'pro'、'SENSOR' 等，多数接口用 'default'）。</summary>
        public string CameraType { get; private set; } = "default";

        /// <summary>相机是否已打开。</summary>
        public bool IsOpen
        {
            get
            {
                try { return AcqHandle != null && AcqHandle.Length > 0; }
                catch { return false; }
            }
        }

        /// <summary>连续采集（transfer_end 回调）是否已挂接。</summary>
        public bool IsGrabbing { get; private set; }

        // 必须把回调委托保存为字段，防止 GC 回收导致原生回调崩溃
        private HalconAPI.HFramegrabberCallback _transferEndCallback;
        private HalconAPI.HFramegrabberCallback _exceptionCallback;

        /// <summary>连续采集时一帧传输完成事件（在 HALCON 采集线程触发，处理后请调用 <see cref="GrabImageAsync"/> 取图）。</summary>
        public event EventHandler ImageGrabbed;

        /// <summary>连续采集异常事件，参数为 HALCON 错误描述。</summary>
        public event EventHandler<string> GrabException;

        #endregion

        #region ==================== 接口/设备枚举 ====================

        /// <summary>枚举本机可用采集接口（实际安装了驱动/授权的子集）。</summary>
        public static string[] QueryInterfaces()
        {
            return HHalconEnv.InfoFramegrabber("File", "defaults").Length >= 0
                ? HHalconEnv.QueryFramegrabberInterfaces()
                : new string[0];
        }

        /// <summary>枚举指定接口下的可用设备（info_framegrabber(...,'device')）。</summary>
        /// <param name="interfaceName">采集接口名。</param>
        /// <returns>设备标识数组（序号或序列号，取决于接口），失败为空数组。</returns>
        public static string[] QueryDevices(string interfaceName)
        {
            return HHalconEnv.InfoFramegrabber(interfaceName, "device");
        }

        /// <summary>查询接口支持的参数名列表（info_framegrabber(...,'parameters')）。</summary>
        public static string[] QueryParameters(string interfaceName)
        {
            return HHalconEnv.InfoFramegrabber(interfaceName, "parameters");
        }

        /// <summary>查询接口支持的颜色空间（info_framegrabber(...,'color_space')）。</summary>
        public static string[] QueryColorSpaces(string interfaceName)
        {
            return HHalconEnv.InfoFramegrabber(interfaceName, "color_space");
        }

        #endregion

        #region ==================== 打开/关闭 ====================

        /// <summary>
        /// 全参数打开采集设备（open_framegrabber）。不确定的参数传 "default" 或 0 由 HALCON 取默认值。
        /// </summary>
        /// <param name="interfaceName">采集接口名，如 GigEVision2 / USB3Vision / GenICamTL / DirectShow / File。</param>
        /// <param name="horizontalResolution">水平分辨率，0/default 用默认。</param>
        /// <param name="verticalResolution">垂直分辨率，0/default 用默认。</param>
        /// <param name="imageWidth">图像宽，0/default 用相机当前值。</param>
        /// <param name="imageHeight">图像高，0/default 用相机当前值。</param>
        /// <param name="startRow">感兴趣区起始行，0/default。</param>
        /// <param name="startColumn">感兴趣区起始列，0/default。</param>
        /// <param name="field">场模式：'default'/'pro'/'interlaced' 等。</param>
        /// <param name="bitsPerChannel">每通道位数，0/default。</param>
        /// <param name="colorSpace">颜色空间：'gray'/'rgb'/'yuv' 等，default 用接口默认。</param>
        /// <param name="generic">通用扩展参数（部分接口需要，如 GigE 的 IP 配置串），可空。</param>
        /// <param name="externalTrigger">外部触发：'true'/'false'/'default'。</param>
        /// <param name="cameraType">相机类型子类型，通常 'default'。</param>
        /// <param name="device">设备标识（序号 "0" 或设备序列号/相机名）。</param>
        /// <param name="port">端口（多相机交换板用，通常 0）。</param>
        /// <param name="lineIn">线路输入（通常 0）。</param>
        public bool Open(string interfaceName,
            object horizontalResolution, object verticalResolution,
            object imageWidth, object imageHeight,
            object startRow, object startColumn,
            string field, object bitsPerChannel,
            string colorSpace, string generic,
            string externalTrigger, string cameraType,
            string device, object port, object lineIn)
        {
            Close();
            return SafeRun(() =>
            {
                HTuple handle;
                HOperatorSet.OpenFramegrabber(
                    interfaceName ?? "File",
                    HHalconEnv.ToTuple(horizontalResolution ?? "default"),
                    HHalconEnv.ToTuple(verticalResolution ?? "default"),
                    HHalconEnv.ToTuple(imageWidth ?? "default"),
                    HHalconEnv.ToTuple(imageHeight ?? "default"),
                    HHalconEnv.ToTuple(startRow ?? "default"),
                    HHalconEnv.ToTuple(startColumn ?? "default"),
                    string.IsNullOrEmpty(field) ? "default" : field,
                    HHalconEnv.ToTuple(bitsPerChannel ?? "default"),
                    string.IsNullOrEmpty(colorSpace) ? "default" : colorSpace,
                    string.IsNullOrEmpty(generic) ? new HTuple() : new HTuple(generic),
                    string.IsNullOrEmpty(externalTrigger) ? "default" : externalTrigger,
                    string.IsNullOrEmpty(cameraType) ? "default" : cameraType,
                    string.IsNullOrEmpty(device) ? "default" : device,
                    HHalconEnv.ToTuple(port ?? "default"),
                    HHalconEnv.ToTuple(lineIn ?? "default"),
                    out handle);
                AcqHandle = handle;
                InterfaceName = interfaceName ?? string.Empty;
                Device = device ?? string.Empty;
                CameraType = string.IsNullOrEmpty(cameraType) ? "default" : cameraType;
            });
        }

        /// <summary>
        /// 常用参数快捷打开：接口名 + 设备 + 颜色空间，其余全部取默认值。
        /// 触发模式默认关闭（free-run），需要触发再调 <see cref="SetParam"/> 写 ExposureTime/TriggerMode 等厂商参数。
        /// </summary>
        /// <param name="interfaceName">采集接口名。</param>
        /// <param name="device">设备标识（默认 "0"）。</param>
        /// <param name="colorSpace">颜色空间（默认 gray，彩色相机传 rgb）。</param>
        public bool Open(string interfaceName, string device = "0", string colorSpace = "gray")
        {
            return Open(interfaceName, 0, 0, 0, 0, 0, 0, "default", 0,
                colorSpace, string.Empty, "false", "default", device, 0, 0);
        }

        /// <summary>关闭采集设备（close_framegrabber），并摘除连续采集回调。</summary>
        public void Close()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsGrabbing) StopGrab();
                    if (IsOpen)
                    {
                        try { HOperatorSet.CloseFramegrabber(AcqHandle); } catch { }
                        AcqHandle = new HTuple();
                    }
                }
                catch { }
            }
            InterfaceName = string.Empty;
            Device = string.Empty;
        }

        #endregion

        #region ==================== 同步 / 异步抓图 ====================

        /// <summary>
        /// 同步抓一帧（grab_image）：阻塞直到拿到图像或超时/出错。
        /// </summary>
        /// <param name="image">输出的 HALCON 图像（调用方负责 Dispose）。</param>
        /// <returns>true=成功；false=失败（见 <see cref="Error"/>）。</returns>
        public bool GrabImage(out HObject image)
        {
            image = null;
            if (!IsOpen)
            {
                Error = HTranslation.GetContent("采集设备尚未打开");
                return false;
            }
            HObject img = null;
            bool ok = SafeRun(() =>
            {
                HOperatorSet.GrabImage(out img, AcqHandle);
            });
            image = img;
            return ok;
        }

        /// <summary>
        /// 异步抓图（grab_image_async）：先启动连续入队（<see cref="GrabImageStart"/>）后调用本方法，
        /// 取到的是“不早于调用时刻”的最新帧，延迟显著低于 grab_image。
        /// </summary>
        /// <param name="image">输出图像（调用方负责 Dispose）。</param>
        /// <param name="maxDelay">最大可接受延迟（秒）：-1 表示强制下一帧，0 取最新缓冲帧。</param>
        public bool GrabImageAsync(out HObject image, double maxDelay = -1)
        {
            image = null;
            if (!IsOpen)
            {
                Error = HTranslation.GetContent("采集设备尚未打开");
                return false;
            }
            HObject img = null;
            bool ok = SafeRun(() =>
            {
                HOperatorSet.GrabImageAsync(out img, AcqHandle, maxDelay);
            });
            image = img;
            return ok;
        }

        /// <summary>
        /// 启动相机连续采集入队（grab_image_start），随后用 <see cref="GrabImageAsync"/> 取帧。
        /// </summary>
        /// <param name="maxDelay">入队等待参数（秒），默认 -1。</param>
        public bool GrabImageStart(double maxDelay = -1)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.GrabImageStart(AcqHandle, maxDelay));
        }

        #endregion

        #region ==================== 连续采集回调 ====================

        /// <summary>
        /// 挂接 transfer_end 连续采集回调（set_framegrabber_callback）：相机每传完一帧触发
        /// <see cref="ImageGrabbed"/> 事件；事件处理中调用 <see cref="GrabImageAsync"/> 取最新帧。
        /// 同时挂接 exception 回调把相机异常转发到 <see cref="GrabException"/>。
        /// </summary>
        public bool StartGrab()
        {
            if (!IsOpen || IsGrabbing) return IsOpen && IsGrabbing;
            return SafeRun(() =>
            {
                _transferEndCallback = OnTransferEnd;
                _exceptionCallback = OnException;
                // set_framegrabber_callback 的回调形参为 HTuple 句柄，需把委托封送成原生函数指针传入
                IntPtr fpTransfer = Marshal.GetFunctionPointerForDelegate(_transferEndCallback);
                IntPtr fpException = Marshal.GetFunctionPointerForDelegate(_exceptionCallback);
                HOperatorSet.SetFramegrabberCallback(AcqHandle, "transfer_end", new HTuple(fpTransfer), new HTuple());
                HOperatorSet.SetFramegrabberCallback(AcqHandle, "exception", new HTuple(fpException), new HTuple());
                HOperatorSet.GrabImageStart(AcqHandle, -1);
                IsGrabbing = true;
            });
        }

        /// <summary>摘除采集回调并停止连续采集。</summary>
        public bool StopGrab()
        {
            if (!IsOpen || !IsGrabbing) return true;
            bool ok = SafeRun(() =>
            {
                // 传空回调指针即摘除
                HOperatorSet.SetFramegrabberCallback(AcqHandle, "transfer_end", new HTuple(IntPtr.Zero), new HTuple());
                HOperatorSet.SetFramegrabberCallback(AcqHandle, "exception", new HTuple(IntPtr.Zero), new HTuple());
                IsGrabbing = false;
            });
            _transferEndCallback = null;
            _exceptionCallback = null;
            return ok;
        }

        /// <summary>HALCON transfer_end 原生回调（在采集线程执行）。</summary>
        private int OnTransferEnd(IntPtr handle, IntPtr userContext, IntPtr context)
        {
            EventHandler handler = ImageGrabbed;
            if (handler != null)
            {
                try { handler(this, EventArgs.Empty); } catch { }
            }
            return 0;
        }

        /// <summary>HALCON exception 原生回调（在采集线程执行）。</summary>
        private int OnException(IntPtr handle, IntPtr userContext, IntPtr context)
        {
            EventHandler<string> handler = GrabException;
            if (handler != null)
            {
                try { handler(this, HTranslation.GetContent("采集回调报告异常")); } catch { }
            }
            return 0;
        }

        #endregion

        #region ==================== 相机参数读写 ====================

        /// <summary>
        /// 写相机参数（set_framegrabber_param），参数名为各接口 GenICam 节点名，
        /// 如 "ExposureTime"（微秒）、"Gain"、"TriggerMode"("On"/"Off")、"TriggerSource"、
        /// "AcquisitionFrameRate"、"Width"、"Height"、"PixelFormat"。
        /// </summary>
        /// <param name="param">参数名。</param>
        /// <param name="value">参数值（数值/字符串/HTuple）。</param>
        public bool SetParam(string param, object value)
        {
            if (!IsOpen) return false;
            return SafeRun(() =>
                HOperatorSet.SetFramegrabberParam(AcqHandle, param ?? string.Empty, HHalconEnv.ToTuple(value)));
        }

        /// <summary>
        /// 读相机参数（get_framegrabber_param），如 "ExposureTime"、"Gain"、"DeviceTemperature"、
        /// "Width"、"Height"、"TriggerMode"；失败返回空元组。
        /// </summary>
        /// <param name="param">参数名。</param>
        /// <param name="value">输出参数值。</param>
        public bool GetParam(string param, out HTuple value)
        {
            value = new HTuple();
            if (!IsOpen) return false;
            HTuple v = new HTuple();
            bool ok = SafeRun(() =>
                HOperatorSet.GetFramegrabberParam(AcqHandle, param ?? string.Empty, out v));
            value = v;
            return ok;
        }

        /// <summary>读相机参数并转字符串（失败返回空串）。</summary>
        public string GetParamString(string param)
        {
            HTuple v;
            return GetParam(param, out v) ? ToS(v) : string.Empty;
        }

        /// <summary>读相机参数并转 double（失败返回 0）。</summary>
        public double GetParamDouble(string param)
        {
            HTuple v;
            return GetParam(param, out v) ? ToD(v) : 0.0;
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>关闭相机、摘除回调并释放资源。</summary>
        protected override void DisposeUnmanaged()
        {
            Close();
        }

        #endregion
    }
}
