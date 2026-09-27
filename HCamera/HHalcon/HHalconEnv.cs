using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 运行环境静态工具：许可证/版本检测、系统参数（set_system/get_system）读写、
    /// 临时目录设置、采集接口枚举。使用 HHalcon 系列其它类之前可先调用 <see cref="Check"/> 确认
    /// 运行机已正确安装 HALCON 且 License 可用。
    /// </summary>
    public static class HHalconEnv
    {
        #region ==================== 版本与许可 ====================

        /// <summary>当前加载的 halcondotnet 程序集版本号（如 21.11.0.0）；加载失败返回空串。</summary>
        public static string AssemblyVersion
        {
            get
            {
                try { return typeof(HOperatorSet).Assembly.GetName().Version.ToString(); }
                catch { return string.Empty; }
            }
        }

        /// <summary>
        /// HALCON 引擎版本（get_system('version')）；未安装或 License 异常时返回空串。
        /// </summary>
        public static string EngineVersion
        {
            get
            {
                try
                {
                    HTuple v;
                    HOperatorSet.GetSystem("version", out v);
                    return v.S ?? string.Empty;
                }
                catch { return string.Empty; }
            }
        }

        /// <summary>
        /// 检测 HALCON 是否可用：执行一次 get_system 调用，能正常返回即视为安装/授权正常。
        /// </summary>
        /// <param name="error">输出失败原因（成功为空）。</param>
        /// <returns>true=可用；false=未安装、未授权或运行库缺失。</returns>
        public static bool Check(out string error)
        {
            try
            {
                HTuple v;
                HOperatorSet.GetSystem("version", out v);
                error = string.Empty;
                return true;
            }
            catch (HalconException hex)
            {
                error = hex.GetErrorMessage();
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>检测 HALCON 是否可用（不关心具体错误信息）。</summary>
        public static bool IsAvailable()
        {
            string err;
            return Check(out err);
        }

        #endregion

        #region ==================== 系统参数 ====================

        /// <summary>
        /// 读取 HALCON 系统参数（get_system），如 width、height、operating_system、clip_region、
        /// int_zooming、border_shape_models、parallelize_operators 等；失败返回空元组。
        /// </summary>
        /// <param name="name">系统参数名。</param>
        /// <returns>参数值（字符串/数值视参数而定）。</returns>
        public static HTuple GetSystem(string name)
        {
            HTuple v;
            HOperatorSet.GetSystem(name ?? string.Empty, out v);
            return v;
        }

        /// <summary>
        /// 设置 HALCON 系统参数（set_system），如 ('clip_region','true')、('int_zooming','false')、
        /// ('tsp_thread_num', 4)、('border_shape_models','false')。
        /// </summary>
        /// <param name="name">系统参数名。</param>
        /// <param name="value">参数值。</param>
        /// <returns>true=成功；false=失败（参数名/取值非法）。</returns>
        public static bool SetSystem(string name, object value)
        {
            try
            {
                HOperatorSet.SetSystem(name ?? string.Empty, ToTuple(value));
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>设置 HALCON 临时文件目录（set_system('temp_dir', ...)），返回是否成功。</summary>
        /// <param name="dir">目录路径（不存在时不会自动创建，需调用方保证）。</param>
        public static bool SetTempDir(string dir)
        {
            return SetSystem("temp_dir", dir ?? string.Empty);
        }

        /// <summary>设置算子并行线程数（set_system('tsp_thread_num', n)），返回是否成功。</summary>
        /// <param name="threadNum">线程数（1~CPU 逻辑核数）。</param>
        public static bool SetThreadNum(int threadNum)
        {
            return SetSystem("tsp_thread_num", threadNum);
        }

        #endregion

        #region ==================== 图像采集接口 ====================

        /// <summary>
        /// 枚举本机 HALCON 可用的图像采集接口名（info_framegrabber 的 'general'/'info_boards' 不适用，
        /// 这里用 query='defaults' 配合各接口名探测）。常用值：GigEVision2、USB3Vision、GenICamTL、DirectShow、File 等。
        /// 返回的名称可直接传给 <see cref="HHalconCamera.Open"/> 的 interfaceName。
        /// </summary>
        /// <param name="candidate">候选接口名列表（为 null 时使用内置常见接口逐个探测）。</param>
        /// <returns>当前 HALCON 已注册、可实例化的接口名数组。</returns>
        public static string[] QueryFramegrabberInterfaces(string[] candidate = null)
        {
            string[] probes = candidate ?? new[]
            {
                "GigEVision2", "GigEVision", "USB3Vision", "USB3", "GenICamTL", "GenICam",
                "DirectShow", "MediaFoundation", "File", "SaperaLT", "MIL", "pylon", "GigEVisionStar"
            };
            var list = new System.Collections.Generic.List<string>();
            foreach (string name in probes)
            {
                try
                {
                    HTuple info, values;
                    // 对每个接口请求设备列表查询；接口不存在时会抛 HalconException
                    HOperatorSet.InfoFramegrabber(name, "device", out info, out values);
                    if (!list.Contains(name)) list.Add(name);
                }
                catch
                {
                    // 该采集接口未安装/无 License，跳过
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 查询指定采集接口的某项信息（info_framegrabber），query 可取
        /// 'bits_per_channel'、'color_space'、'defaults'、'device'、'external_trigger'、
        /// 'field'、'generic'、'horizontal_resolution'、'image_width' 等。
        /// </summary>
        /// <param name="interfaceName">采集接口名，如 GigEVision2。</param>
        /// <param name="query">查询项。</param>
        /// <returns>取值数组；失败返回空数组。</returns>
        public static string[] InfoFramegrabber(string interfaceName, string query)
        {
            try
            {
                HTuple info, values;
                HOperatorSet.InfoFramegrabber(interfaceName ?? string.Empty, query ?? string.Empty, out info, out values);
                return values == null ? new string[0] : values.SArr;
            }
            catch
            {
                return new string[0];
            }
        }

        #endregion

        #region ==================== 杂项 ====================

        /// <summary>把常用标量装箱为 HTuple（数值/布尔/字符串/数组），null 转为空元组。</summary>
        internal static HTuple ToTuple(object value)
        {
            if (value == null) return new HTuple();
            Type t = value.GetType();
            if (t == typeof(HTuple)) return (HTuple)value;
            if (t == typeof(string)) return new HTuple((string)value);
            if (t == typeof(double[])) return new HTuple((double[])value);
            if (t == typeof(int[]))
            {
                int[] a = (int[])value;
                double[] d = new double[a.Length];
                for (int i = 0; i < a.Length; i++) d[i] = a[i];
                return new HTuple(d);
            }
            if (t == typeof(string[])) return new HTuple((string[])value);
            if (t == typeof(bool)) return new HTuple(((bool)value) ? 1 : 0);
            return new HTuple(Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
        }

        #endregion
    }
}
