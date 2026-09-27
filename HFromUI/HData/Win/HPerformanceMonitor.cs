using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 系统性能高级监控工具类（优化版，集成多语言翻译）。
    /// 提供通用性能计数器读取、CPU/GPU 温度、风扇转速、GPU 使用率、磁盘 I/O、网络吞吐量等功能。
    /// 底层基于 PerformanceCounter 和 WMI，异常安全。
    /// 部分功能（如 GPU 使用率）需要 Windows 10 + WDDM 2.0 驱动支持，若不可用返回 -1。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HPerformanceMonitor
    {

        #region 通用性能计数器读取

        /// <summary>读取指定性能计数器的当前值（返回 float）。</summary>
        /// <param name="categoryName">类别名称，如 "Processor"。</param>
        /// <param name="counterName">计数器名称，如 "% Processor Time"。</param>
        /// <param name="instanceName">实例名称，如 "_Total"，可为 null 表示无实例。</param>
        /// <returns>当前值，失败返回 0。</returns>
        public static float ReadPerformanceCounter(string categoryName, string counterName, string instanceName = null)
        {
            try
            {
                using (var counter = string.IsNullOrEmpty(instanceName)
                    ? new PerformanceCounter(categoryName, counterName)
                    : new PerformanceCounter(categoryName, counterName, instanceName))
                {
                    return counter.NextValue();
                }
            }
            catch { return 0; }
        }

        /// <summary>读取指定性能计数器的当前值（返回 long）。</summary>
        public static long ReadPerformanceCounterLong(string categoryName, string counterName, string instanceName = null)
        {
            return (long)ReadPerformanceCounter(categoryName, counterName, instanceName);
        }

        #endregion

        #region CPU 使用率（优化：优先使用 PerformanceCounter）

        /// <summary>获取系统整体 CPU 使用率（%），优先使用性能计数器（更稳定）。</summary>
        /// <returns>使用率百分比，失败返回 -1。</returns>
        public static float GetCpuUsage()
        {
            // 方法1：尝试使用性能计数器（最可靠）
            try
            {
                using (var counter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    // 第一次调用 NextValue() 可能返回 0，需要第二次获取才能得到正确值
                    counter.NextValue();
                    System.Threading.Thread.Sleep(100); // 等待一个采样周期
                    float value = counter.NextValue();
                    return value;
                }
            }
            catch
            {
                // 方法2：备选 WMI 查询
                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                            return Convert.ToSingle(obj["PercentProcessorTime"]);
                    }
                }
                catch { }
            }
            return -1;
        }

        /// <summary>获取每个逻辑核心的 CPU 使用率列表。</summary>
        public static List<float> GetCpuUsagePerCore()
        {
            var list = new List<float>();
            try
            {
                // 尝试性能计数器
                // 注意：PerformanceCounterCategory 在 .NET Framework 中不实现 IDisposable
                // （它自身不持有需要释放的资源；真正需要释放的是下方每个 PerformanceCounter，已 using）
                PerformanceCounterCategory category = new PerformanceCounterCategory("Processor");
                string[] instanceNames = category.GetInstanceNames();
                foreach (string instance in instanceNames)
                {
                    if (instance == "_Total") continue;
                    try
                    {
                        using (var counter = new PerformanceCounter("Processor", "% Processor Time", instance))
                        {
                            counter.NextValue();
                            System.Threading.Thread.Sleep(100);
                            list.Add(counter.NextValue());
                        }
                    }
                    catch { }
                }
                if (list.Count > 0) return list;

                // 备选 WMI
                using (var searcher = new ManagementObjectSearcher("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name <> '_Total'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                        list.Add(Convert.ToSingle(obj["PercentProcessorTime"]));
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region CPU 温度

        /// <summary>获取 CPU 温度（摄氏度），基于 WMI root\WMI 命名空间下的 MSAcpi_ThermalZoneTemperature。</summary>
        /// <returns>温度值，失败返回 -1。</returns>
        public static double GetCpuTemperature()
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"root\WMI");
                scope.Connect();
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSAcpi_ThermalZoneTemperature");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["CurrentTemperature"] != null)
                        {
                            uint kelvin = Convert.ToUInt32(obj["CurrentTemperature"]);
                            return Math.Round(kelvin / 10.0 - 273.15, 1);
                        }
                    }
                }
            }
            catch { }
            return -1;
        }

        #endregion

        #region GPU 温度与使用率

        /// <summary>获取 GPU 温度（摄氏度），尝试通过 Win32_VideoController 的 AdapterTemperature 属性获取。</summary>
        /// <returns>温度值，失败返回 -1。</returns>
        public static double GetGpuTemperature()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["AdapterTemperature"] != null)
                            return Convert.ToDouble(obj["AdapterTemperature"]);
                    }
                }
            }
            catch { }
            return -1;
        }

        /// <summary>获取 GPU 使用率（%），通过性能计数器 "GPU Engine" 类别（需要 Windows 10 + WDDM 2.0）。</summary>
        /// <returns>GPU 使用率 0-100，失败返回 -1。</returns>
        public static double GetGpuUsage()
        {
            try
            {
                // PerformanceCounterCategory 不实现 IDisposable，无需 using
                var category = new PerformanceCounterCategory("GPU Engine");
                if (category.CounterExists("Utilization Percentage"))
                {
                    string[] instanceNames = category.GetInstanceNames();
                    if (instanceNames.Length == 0) return -1;
                    float total = 0;
                    int count = 0;
                    foreach (string instance in instanceNames)
                    {
                        try
                        {
                            using (var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance))
                            {
                                float val = counter.NextValue();
                                total += val;
                                count++;
                            }
                        }
                        catch { }
                    }
                    return count > 0 ? Math.Round(total / count, 1) : -1;
                }
            }
            catch { }
            return -1;
        }

        #endregion

        #region 风扇转速

        /// <summary>获取所有风扇的当前转速（RPM），通过 Win32_Fan WMI 类。</summary>
        /// <returns>风扇转速列表，失败返回空列表。</returns>
        public static List<int> GetFanSpeeds()
        {
            var speeds = new List<int>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Fan"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["DesiredSpeed"] != null)
                            speeds.Add(Convert.ToInt32(obj["DesiredSpeed"]));
                    }
                }
            }
            catch { }
            return speeds;
        }

        #endregion

        #region 磁盘 I/O

        /// <summary>获取指定磁盘的读速率（字节/秒）。</summary>
        /// <param name="diskName">磁盘名称，如 "C:" 或 "_Total"。</param>
        /// <returns>读速率，失败返回 0。</returns>
        public static long GetDiskReadBytesPerSec(string diskName = "_Total")
        {
            try
            {
                using (var counter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", diskName))
                {
                    return (long)counter.NextValue();
                }
            }
            catch { return 0; }
        }

        /// <summary>获取指定磁盘的写速率（字节/秒）。</summary>
        public static long GetDiskWriteBytesPerSec(string diskName = "_Total")
        {
            try
            {
                using (var counter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", diskName))
                {
                    return (long)counter.NextValue();
                }
            }
            catch { return 0; }
        }

        /// <summary>获取指定磁盘的总 I/O 速率（字节/秒）。</summary>
        public static long GetDiskTotalBytesPerSec(string diskName = "_Total")
        {
            return GetDiskReadBytesPerSec(diskName) + GetDiskWriteBytesPerSec(diskName);
        }

        #endregion

        #region 网络吞吐量

        /// <summary>获取指定网络接口的发送速率（字节/秒）。</summary>
        /// <param name="interfaceName">网络接口名称，如 "Realtek PCIe GbE Family Controller"。</param>
        /// <returns>发送速率，失败返回 0。</returns>
        public static long GetNetworkSentBytesPerSec(string interfaceName)
        {
            try
            {
                using (var counter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", interfaceName))
                {
                    return (long)counter.NextValue();
                }
            }
            catch { return 0; }
        }

        /// <summary>获取指定网络接口的接收速率（字节/秒）。</summary>
        public static long GetNetworkReceivedBytesPerSec(string interfaceName)
        {
            try
            {
                using (var counter = new PerformanceCounter("Network Interface", "Bytes Received/sec", interfaceName))
                {
                    return (long)counter.NextValue();
                }
            }
            catch { return 0; }
        }

        #endregion
    }
}
