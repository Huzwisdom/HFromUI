using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using HFromUI;

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全网最全面计算机硬件/系统信息工具类（.NET 4.8 / C# 7.3）
    /// 涵盖 CPU、内存、磁盘、网络、系统日志、Windows更新、进程、服务等 100+ 项数据。
    /// 所有面向用户的文本均通过 <see cref="HTranslation.GetContent"/> 多语言翻译。
    /// </summary>
    public class HComputer
    {
        #region 翻译
        #endregion

        #region WMI 枚举辅助
        /// <summary>枚举 WMI 查询结果，并自动释放结果集合与每个 ManagementObject，避免 WMI 句柄泄漏。</summary>
        private static IEnumerable<ManagementObject> Enumerate(ManagementObjectSearcher searcher)
        {
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject mo in results)
                {
                    using (mo)
                    {
                        yield return mo;
                    }
                }
            }
        }
        #endregion

        #region 【1】CPU
        public class CpuInfo
        {
            public string DeviceID, Name, Manufacturer;
            public int MaxClockSpeed, CurrentClockSpeed, NumberOfCores, NumberOfLogicalProcessors, L2CacheSize, L3CacheSize;
            public string Architecture, Status;
        }
        /// <summary>获取 cpuInfo。</summary>
        public static List<CpuInfo> GetCpuInfo()
        {
            var list = new List<CpuInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    foreach (ManagementObject o in Enumerate(searcher))
                        list.Add(new CpuInfo
                        {
                            DeviceID = o["DeviceID"]?.ToString(),
                            Name = o["Name"]?.ToString(),
                            Manufacturer = o["Manufacturer"]?.ToString(),
                            MaxClockSpeed = Convert.ToInt32(o["MaxClockSpeed"] ?? 0),
                            CurrentClockSpeed = Convert.ToInt32(o["CurrentClockSpeed"] ?? 0),
                            NumberOfCores = Convert.ToInt32(o["NumberOfCores"] ?? 0),
                            NumberOfLogicalProcessors = Convert.ToInt32(o["NumberOfLogicalProcessors"] ?? 0),
                            L2CacheSize = Convert.ToInt32(o["L2CacheSize"] ?? 0),
                            L3CacheSize = Convert.ToInt32(o["L3CacheSize"] ?? 0),
                            Architecture = ArchString(o["Architecture"]),
                            Status = o["Status"]?.ToString() ?? HTranslation.GetContent("正常")
                        });
            }
            catch { }
            return list;
        }
        /// <summary>获取 cpuUsagePercent。</summary>
        public static float GetCpuUsagePercent()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToSingle(o["PercentProcessorTime"]);
            }
            catch { }
            return -1f;
        }
        /// <summary>获取 cpuUsagePerCore。</summary>
        public static List<float> GetCpuUsagePerCore()
        {
            var list = new List<float>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name <> '_Total'"))
                    foreach (ManagementObject o in Enumerate(s)) list.Add(Convert.ToSingle(o["PercentProcessorTime"]));
            }
            catch { }
            return list;
        }
        /// <summary>ArchString 方法。</summary>
        private static string ArchString(object a)
        {
            if (a == null) return HTranslation.GetContent("未知");
            switch (Convert.ToInt16(a))
            {
                case 0: return HTranslation.GetContent("x86");
                case 1: return HTranslation.GetContent("MIPS");
                case 5: return HTranslation.GetContent("ARM");
                case 9: return HTranslation.GetContent("x64");
                default: return HTranslation.GetContent("未知");
            }
        }
        #endregion

        #region 【2】内存
        public class PhysicalMemoryInfo
        {
            public string BankLabel, Manufacturer, PartNumber, SerialNumber, MemoryType, FormFactor;
            public long Capacity;
            public int Speed;
        }
        /// <summary>获取 physicalMemoryInfo。</summary>
        public static List<PhysicalMemoryInfo> GetPhysicalMemoryInfo()
        {
            var list = new List<PhysicalMemoryInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new PhysicalMemoryInfo
                        {
                            BankLabel = o["BankLabel"]?.ToString(),
                            Capacity = Convert.ToInt64(o["Capacity"] ?? 0),
                            Speed = Convert.ToInt32(o["Speed"] ?? 0),
                            Manufacturer = o["Manufacturer"]?.ToString(),
                            PartNumber = o["PartNumber"]?.ToString(),
                            SerialNumber = o["SerialNumber"]?.ToString(),
                            MemoryType = MemType(ReadMemType(o)),
                            FormFactor = FormFactor(Convert.ToInt32(o["FormFactor"] ?? 0))
                        });
            }
            catch { }
            return list;
        }
        /// <summary>获取 totalMemoryBytes。</summary>
        public static long GetTotalMemoryBytes()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToInt64(o["TotalVisibleMemorySize"]) * 1024;
            }
            catch { }
            return -1;
        }
        /// <summary>获取 availableMemoryBytes。</summary>
        public static long GetAvailableMemoryBytes()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToInt64(o["FreePhysicalMemory"]) * 1024;
            }
            catch { }
            return -1;
        }
        /// <summary>获取 memoryUsagePercent。</summary>
        public static float GetMemoryUsagePercent()
        {
            long t = GetTotalMemoryBytes(), a = GetAvailableMemoryBytes();
            if (t <= 0 || a < 0) return -1f;
            return (1f - (float)a / t) * 100f;
        }
        /// <summary>FormatBytes 方法。</summary>
        public static string FormatBytes(long b)
        {
            if (b < 0) return HTranslation.GetContent("未知");
            if (b < 1024) return $"{b} B";
            double kb = b / 1024.0;
            if (kb < 1024) return $"{kb:F1} KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return $"{mb:F2} MB";
            double gb = mb / 1024.0;
            if (gb < 1024) return $"{gb:F2} GB";
            return $"{gb / 1024.0:F2} TB";
        }
        /// <summary>MemType 方法。</summary>
        private static string MemType(int t)
        {
            switch (t) { case 20: return "DDR"; case 21: return "DDR2"; case 24: return "DDR3"; case 26: return "DDR4"; case 34: return "DDR5"; }
            return HTranslation.GetContent("未知");
        }
        /// <summary>读取内存类型；MemoryType 在 DDR4/DDR5 平台常返回 0，此时回退到 SMBIOSMemoryType（两者 DDR 世代码值一致）。</summary>
        private static int ReadMemType(ManagementObject o)
        {
            int t = Convert.ToInt32(o["MemoryType"] ?? 0);
            if (t == 0 && o["SMBIOSMemoryType"] != null) t = Convert.ToInt32(o["SMBIOSMemoryType"]);
            return t;
        }
        /// <summary>FormFactor 方法。</summary>
        private static string FormFactor(int f)
        {
            // 修正 Win32_PhysicalMemory.FormFactor 码表：8=DIMM, 9=TSOP, 11=RIMM, 12=SODIMM（原 9/12 标签为复制粘贴错误）
            switch (f) { case 8: return "DIMM"; case 9: return "TSOP"; case 11: return "RIMM"; case 12: return "SODIMM"; }
            return HTranslation.GetContent("未知");
        }
        /// <summary>获取 totalVirtualMemoryBytes。</summary>
        public static long GetTotalVirtualMemoryBytes()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT TotalVirtualMemorySize FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToInt64(o["TotalVirtualMemorySize"]) * 1024;
            }
            catch { }
            return -1;
        }
        /// <summary>获取 availableVirtualMemoryBytes。</summary>
        public static long GetAvailableVirtualMemoryBytes()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT FreeVirtualMemory FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToInt64(o["FreeVirtualMemory"]) * 1024;
            }
            catch { }
            return -1;
        }
        /// <summary>获取 totalPageFileSizeMB。</summary>
        public static long GetTotalPageFileSizeMB()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT TotalPageFileSize FROM Win32_PageFileUsage"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToInt64(o["TotalPageFileSize"]);
            }
            catch { }
            return -1;
        }
        #endregion

        #region 【3】磁盘
        public class LogicalDiskInfo
        {
            public string Name, FileSystem, VolumeName, DriveType, VolumeSerialNumber;
            public long TotalSize, FreeSpace;
        }
        public class PhysicalDiskInfo
        {
            public string Model, InterfaceType, MediaType, Status, SerialNumber;
            public long Size;
        }
        public class PartitionInfo
        {
            public string Name, BootPartition;
            public long Size;
        }
        /// <summary>获取 logicalDiskInfo。</summary>
        public static List<LogicalDiskInfo> GetLogicalDiskInfo()
        {
            var list = new List<LogicalDiskInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3 OR DriveType=2"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new LogicalDiskInfo
                        {
                            Name = o["DeviceID"]?.ToString(),
                            FileSystem = o["FileSystem"]?.ToString(),
                            TotalSize = Convert.ToInt64(o["Size"] ?? 0),
                            FreeSpace = Convert.ToInt64(o["FreeSpace"] ?? 0),
                            VolumeName = o["VolumeName"]?.ToString(),
                            DriveType = DriveTypeStr(Convert.ToInt32(o["DriveType"])),
                            VolumeSerialNumber = o["VolumeSerialNumber"]?.ToString()
                        });
            }
            catch { }
            return list;
        }
        /// <summary>获取 physicalDiskInfo。</summary>
        public static List<PhysicalDiskInfo> GetPhysicalDiskInfo()
        {
            var list = new List<PhysicalDiskInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new PhysicalDiskInfo
                        {
                            Model = o["Model"]?.ToString(),
                            Size = Convert.ToInt64(o["Size"] ?? 0),
                            InterfaceType = o["InterfaceType"]?.ToString(),
                            MediaType = o["MediaType"]?.ToString(),
                            Status = o["Status"]?.ToString(),
                            SerialNumber = o["SerialNumber"]?.ToString()?.Trim()
                        });
            }
            catch { }
            return list;
        }
        /// <summary>获取 partitionInfo。</summary>
        public static List<PartitionInfo> GetPartitionInfo()
        {
            var list = new List<PartitionInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new PartitionInfo
                        {
                            Name = o["Name"]?.ToString(),
                            Size = Convert.ToInt64(o["Size"] ?? 0),
                            BootPartition = Convert.ToBoolean(o["BootPartition"] ?? false) ? HTranslation.GetContent("是") : HTranslation.GetContent("否")
                        });
            }
            catch { }
            return list;
        }
        /// <summary>获取 diskSmartStatus。</summary>
        public static string GetDiskSmartStatus()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Status FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'"))
                    foreach (ManagementObject o in Enumerate(s)) return o["Status"]?.ToString() ?? HTranslation.GetContent("未知");
            }
            catch { }
            return HTranslation.GetContent("未知");
        }
        /// <summary>DriveTypeStr 方法。</summary>
        private static string DriveTypeStr(int t)
        {
            switch (t) { case 3: return HTranslation.GetContent("本地磁盘"); case 2: return HTranslation.GetContent("可移动磁盘"); case 5: return HTranslation.GetContent("光盘"); case 4: return HTranslation.GetContent("网络磁盘"); }
            return HTranslation.GetContent("未知");
        }
        #endregion

        #region 【4】网络
        public class NetworkAdapterInfo
        {
            public string Name, Description, MACAddress, AdapterType, ConnectionStatus, Manufacturer;
            public long Speed;
            public string[] IPAddresses, SubnetMasks, Gateways, DnsServers;
            public bool DhcpEnabled;
            public string DhcpServer;
        }
        /// <summary>获取 macAddresses。</summary>
        public static List<string> GetMacAddresses()
        {
            var macs = new List<string>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback || ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;
                string mac = ni.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(mac))
                {
                    string f = Regex.Replace(mac, ".{2}", "$0:").TrimEnd(':');
                    if (!macs.Contains(f)) macs.Add(f);
                }
            }
            return macs;
        }
        /// <summary>获取 networkInterfaceCount。</summary>
        public static int GetNetworkInterfaceCount() => GetMacAddresses().Count;
        /// <summary>获取 networkAdapterDetails。</summary>
        public static List<NetworkAdapterInfo> GetNetworkAdapterDetails()
        {
            var list = new List<NetworkAdapterInfo>();
            try
            {
                using (var cfg = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True"))
                    foreach (ManagementObject c in Enumerate(cfg))
                    {
                        string mac = c["MACAddress"]?.ToString();
                        var info = new NetworkAdapterInfo
                        {
                            MACAddress = mac,
                            IPAddresses = (c["IPAddress"] as string[]) ?? new string[0],
                            SubnetMasks = (c["IPSubnet"] as string[]) ?? new string[0],
                            Gateways = (c["DefaultIPGateway"] as string[]) ?? new string[0],
                            DnsServers = (c["DNSServerSearchOrder"] as string[]) ?? new string[0],
                            DhcpEnabled = Convert.ToBoolean(c["DHCPEnabled"]),
                            DhcpServer = c["DHCPServer"]?.ToString()
                        };
                        using (var ad = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapter WHERE MACAddress='{mac}'"))
                            foreach (ManagementObject a in Enumerate(ad))
                            {
                                info.Name = a["Name"]?.ToString();
                                info.Description = a["Description"]?.ToString();
                                info.AdapterType = a["AdapterType"]?.ToString();
                                info.Speed = Convert.ToInt64(a["Speed"] ?? 0);
                                info.Manufacturer = a["Manufacturer"]?.ToString();
                                int st = Convert.ToInt32(a["NetConnectionStatus"] ?? 0);
                                info.ConnectionStatus = (st == 2) ? HTranslation.GetContent("已连接") : HTranslation.GetContent("未连接");
                                break;
                            }
                        list.Add(info);
                    }
            }
            catch { }
            return list;
        }
        /// <summary>获取 tcpConnectionsCount。</summary>
        public static int GetTcpConnectionsCount()
        {
            try { using (var p = new PerformanceCounter("TCPv4", "Connections Established")) return (int)p.NextValue(); }
            catch { return -1; }
        }
        public class TcpUdpEndPoint
        {
            public string Protocol;
            public string LocalAddress;
            public int LocalPort;
            public string RemoteAddress;
            public int RemotePort;
            public string State;
        }
        /// <summary>获取 networkConnections。</summary>
        public static List<TcpUdpEndPoint> GetNetworkConnections()
        {
            var list = new List<TcpUdpEndPoint>();
            try
            {
                IPGlobalProperties ip = IPGlobalProperties.GetIPGlobalProperties();
                // TCP
                foreach (TcpConnectionInformation tcp in ip.GetActiveTcpConnections())
                {
                    list.Add(new TcpUdpEndPoint
                    {
                        Protocol = "TCP",
                        LocalAddress = tcp.LocalEndPoint.Address.ToString(),
                        LocalPort = tcp.LocalEndPoint.Port,
                        RemoteAddress = tcp.RemoteEndPoint.Address.ToString(),
                        RemotePort = tcp.RemoteEndPoint.Port,
                        State = tcp.State.ToString()
                    });
                }
                // TCP listeners
                foreach (IPEndPoint ep in ip.GetActiveTcpListeners())
                {
                    list.Add(new TcpUdpEndPoint
                    {
                        Protocol = "TCP",
                        LocalAddress = ep.Address.ToString(),
                        LocalPort = ep.Port,
                        RemoteAddress = "*",
                        RemotePort = 0,
                        State = HTranslation.GetContent("监听")
                    });
                }
                // UDP listeners
                foreach (IPEndPoint ep in ip.GetActiveUdpListeners())
                {
                    list.Add(new TcpUdpEndPoint
                    {
                        Protocol = "UDP",
                        LocalAddress = ep.Address.ToString(),
                        LocalPort = ep.Port,
                        RemoteAddress = "*",
                        RemotePort = 0,
                        State = HTranslation.GetContent("监听")
                    });
                }
            }
            catch { }
            return list;
        }
        /// <summary>获取 networkShares。</summary>
        public static List<string> GetNetworkShares()
        {
            var shares = new List<string>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_Share"))
                    foreach (ManagementObject o in Enumerate(s)) shares.Add($"{o["Name"]} ({o["Path"]})");
            }
            catch { }
            return shares;
        }
        #endregion

        #region 【5】显卡/声卡/BIOS/主板
        public class VideoControllerInfo
        {
            public string Name, DriverVersion, VideoProcessor;
            public long AdapterRAM;
            public int RefreshRate;
        }
        public class SoundDeviceInfo { public string Name, Manufacturer, Status; }
        public class BiosInfo { public string Manufacturer, Name, Version, SerialNumber, ReleaseDate; }
        public class MotherboardInfo { public string Manufacturer, Product, SerialNumber, Version; }

        /// <summary>获取 videoControllerInfo。</summary>
        public static List<VideoControllerInfo> GetVideoControllerInfo()
        {
            var list = new List<VideoControllerInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new VideoControllerInfo
                        {
                            Name = o["Name"]?.ToString(),
                            AdapterRAM = Convert.ToInt64(o["AdapterRAM"] ?? 0),
                            DriverVersion = o["DriverVersion"]?.ToString(),
                            VideoProcessor = o["VideoProcessor"]?.ToString(),
                            RefreshRate = Convert.ToInt32(o["CurrentRefreshRate"] ?? 0)
                        });
            }
            catch { }
            return list;
        }
        /// <summary>获取 soundDeviceInfo。</summary>
        public static List<SoundDeviceInfo> GetSoundDeviceInfo()
        {
            var list = new List<SoundDeviceInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new SoundDeviceInfo { Name = o["Name"]?.ToString(), Manufacturer = o["Manufacturer"]?.ToString(), Status = o["Status"]?.ToString() });
            }
            catch { }
            return list;
        }
        /// <summary>获取 biosInfo。</summary>
        public static BiosInfo GetBiosInfo()
        {
            var b = new BiosInfo();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                    foreach (ManagementObject o in Enumerate(s))
                    {
                        b.Manufacturer = o["Manufacturer"]?.ToString();
                        b.Name = o["Name"]?.ToString();
                        b.Version = o["Version"]?.ToString();
                        b.SerialNumber = o["SerialNumber"]?.ToString();
                        b.ReleaseDate = o["ReleaseDate"]?.ToString();
                    }
            }
            catch { }
            return b;
        }
        /// <summary>获取 motherboardInfo。</summary>
        public static MotherboardInfo GetMotherboardInfo()
        {
            var m = new MotherboardInfo();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                    foreach (ManagementObject o in Enumerate(s))
                    {
                        m.Manufacturer = o["Manufacturer"]?.ToString();
                        m.Product = o["Product"]?.ToString();
                        m.SerialNumber = o["SerialNumber"]?.ToString();
                        m.Version = o["Version"]?.ToString();
                    }
            }
            catch { }
            return m;
        }
        #endregion

        #region 【6】系统常规
        /// <summary>ComputerName 成员。</summary>
        public static string ComputerName => Environment.MachineName;
        /// <summary>UserName 成员。</summary>
        public static string UserName => Environment.UserName;
        /// <summary>OsVersion 方法。</summary>
        public static string OsVersion()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return o["Caption"]?.ToString();
            }
            catch { }
            return "";
        }
        /// <summary>OsArchitecture 方法。</summary>
        public static string OsArchitecture()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT OSArchitecture FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return o["OSArchitecture"]?.ToString();
            }
            catch { }
            return "";
        }
        /// <summary>BootTime 方法。</summary>
        public static DateTime BootTime()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return ManagementDateTimeConverter.ToDateTime(o["LastBootUpTime"].ToString());
            }
            catch { }
            return DateTime.MinValue;
        }
        /// <summary>UpTime 成员。</summary>
        public static TimeSpan UpTime => DateTime.UtcNow - BootTime().ToUniversalTime();
        /// <summary>TimeZoneName 成员。</summary>
        public static string TimeZoneName => TimeZoneInfo.Local.DisplayName;
        /// <summary>SystemLanguage 成员。</summary>
        public static string SystemLanguage => CultureInfo.InstalledUICulture.DisplayName;
        /// <summary>SystemDirectory 成员。</summary>
        public static string SystemDirectory => Environment.SystemDirectory;
        /// <summary>Is64Bit 成员。</summary>
        public static bool Is64Bit => Environment.Is64BitOperatingSystem;
        /// <summary>Domain 方法。</summary>
        public static string Domain()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Domain FROM Win32_ComputerSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return o["Domain"]?.ToString();
            }
            catch { }
            return "";
        }
        /// <summary>判断是否 DomainController。</summary>
        public static bool IsDomainController()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT DomainRole FROM Win32_ComputerSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return Convert.ToInt32(o["DomainRole"]) >= 4;
            }
            catch { }
            return false;
        }
        /// <summary>Manufacturer 方法。</summary>
        public static string Manufacturer()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Manufacturer FROM Win32_ComputerSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return o["Manufacturer"]?.ToString();
            }
            catch { }
            return "";
        }
        /// <summary>Model 方法。</summary>
        public static string Model()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return o["Model"]?.ToString();
            }
            catch { }
            return "";
        }
        /// <summary>WindowsProductId 方法。</summary>
        public static string WindowsProductId()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s)) return o["SerialNumber"]?.ToString();
            }
            catch { }
            return "";
        }
        /// <summary>ComputerUuid 方法。</summary>
        public static string ComputerUuid()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct"))
                    foreach (ManagementObject o in Enumerate(s)) return o["UUID"]?.ToString();
            }
            catch { }
            return "";
        }
        #endregion

        #region 【7】进程/服务/环境变量
        public class ProcInfo
        {
            public int Id; public string Name; public long WorkingSet, PrivateMemory; public int Threads; public double CpuTimeMs;
        }
        public class SvcInfo { public string Name, DisplayName, Status, StartMode, StartName, PathName, Description; }
        /// <summary>获取 processList。</summary>
        public static List<ProcInfo> GetProcessList()
        {
            var list = new List<ProcInfo>();
            try
            {
                foreach (Process p in Process.GetProcesses())
                    using (p) // 释放 Process 对象，避免进程/线程句柄泄漏
                    {
                        try { list.Add(new ProcInfo { Id = p.Id, Name = p.ProcessName, WorkingSet = p.WorkingSet64, PrivateMemory = p.PrivateMemorySize64, Threads = p.Threads.Count, CpuTimeMs = p.TotalProcessorTime.TotalMilliseconds }); } catch { }
                    }
            }
            catch { }
            return list;
        }
        /// <summary>获取 serviceList。</summary>
        public static List<SvcInfo> GetServiceList()
        {
            var list = new List<SvcInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new SvcInfo
                        {
                            Name = o["Name"]?.ToString(),
                            DisplayName = o["DisplayName"]?.ToString(),
                            Status = o["State"]?.ToString(),
                            StartMode = o["StartMode"]?.ToString(),
                            StartName = o["StartName"]?.ToString(),
                            PathName = o["PathName"]?.ToString(),
                            Description = o["Description"]?.ToString()
                        });
            }
            catch { }
            return list;
        }
        public static Dictionary<string, string> GetMachineEnvVars()
        {
            var d = new Dictionary<string, string>();
            try { foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine)) d[de.Key.ToString()] = de.Value.ToString(); } catch { }
            return d;
        }
        public static Dictionary<string, string> GetUserEnvVars()
        {
            var d = new Dictionary<string, string>();
            try { foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User)) d[de.Key.ToString()] = de.Value.ToString(); } catch { }
            return d;
        }
        #endregion

        #region 【8】打印机/USB/键盘/鼠标/电池
        /// <summary>获取 printers。</summary>
        public static List<string> GetPrinters()
        {
            var l = new List<string>();
            try { using (var s = new ManagementObjectSearcher("SELECT Name FROM Win32_Printer")) foreach (ManagementObject o in Enumerate(s)) l.Add(o["Name"]?.ToString()); } catch { }
            return l;
        }
        /// <summary>获取 usbDevices。</summary>
        public static List<string> GetUsbDevices()
        {
            var l = new List<string>();
            try { using (var s = new ManagementObjectSearcher("SELECT Name FROM Win32_PnPEntity WHERE PNPClass='USB'")) foreach (ManagementObject o in Enumerate(s)) l.Add(o["Name"]?.ToString()); } catch { }
            return l;
        }
        /// <summary>获取 keyboardList。</summary>
        public static List<string> GetKeyboardList()
        {
            var l = new List<string>();
            try { using (var s = new ManagementObjectSearcher("SELECT Name FROM Win32_Keyboard")) foreach (ManagementObject o in Enumerate(s)) l.Add(o["Name"]?.ToString()); } catch { }
            return l;
        }
        /// <summary>获取 pointingDeviceList。</summary>
        public static List<string> GetPointingDeviceList()
        {
            var l = new List<string>();
            try { using (var s = new ManagementObjectSearcher("SELECT Name FROM Win32_PointingDevice")) foreach (ManagementObject o in Enumerate(s)) l.Add(o["Name"]?.ToString()); } catch { }
            return l;
        }
        public class BatteryInfo { public int ChargePercent, RemainingMinutes; public string Status; }
        /// <summary>获取 batteryStatus。</summary>
        public static BatteryInfo GetBatteryStatus()
        {
            var b = new BatteryInfo { ChargePercent = -1, RemainingMinutes = -1, Status = HTranslation.GetContent("未知") };
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                    foreach (ManagementObject o in Enumerate(s))
                    {
                        b.ChargePercent = Convert.ToInt32(o["EstimatedChargeRemaining"] ?? 0);
                        // EstimatedRunTime 接通电源时常返回 0xFFFFFFFF，直接 ToInt32 会溢出抛异常
                        object rt = o["EstimatedRunTime"];
                        if (rt != null)
                        {
                            long rtMin = Convert.ToInt64(rt);
                            b.RemainingMinutes = rtMin > int.MaxValue ? -1 : (int)rtMin;
                        }
                        // 电池充放电状态应取 BatteryStatus 数值码；Status 字符串只有 OK/Error 之类，不表示充放电状态
                        b.Status = MapBatteryStatus(o["BatteryStatus"] != null ? Convert.ToInt32(o["BatteryStatus"]) : 0);
                    }
            }
            catch { }
            return b;
        }
        /// <summary>Win32_Battery.BatteryStatus 数值码映射为中文状态（1=放电,2=接通交流电源,3=已充满…）。</summary>
        private static string MapBatteryStatus(int code)
        {
            switch (code)
            {
                case 1: return HTranslation.GetContent("放电中");
                case 2: return HTranslation.GetContent("已接通交流电源");
                case 3: return HTranslation.GetContent("已充满");
                case 4: return HTranslation.GetContent("电量低");
                case 5: return HTranslation.GetContent("电量严重不足");
                case 6: return HTranslation.GetContent("充电中");
                case 7: return HTranslation.GetContent("充电中(高电量)");
                case 8: return HTranslation.GetContent("充电中(低电量)");
                case 9: return HTranslation.GetContent("充电中(电量严重不足)");
                case 10: return HTranslation.GetContent("未定义");
                case 11: return HTranslation.GetContent("部分充电");
                default: return HTranslation.GetContent("未知");
            }
        }
        #endregion

        #region 【9】性能计数器
        /// <summary>转换为 talHandles。</summary>
        public static long TotalHandles() => GetPerfCounter("Process", "Handle Count", "_Total");
        /// <summary>转换为 talThreads。</summary>
        public static long TotalThreads() => GetPerfCounter("Process", "Thread Count", "_Total");
        // System 是单实例计数器类别，实例名必须传空字符串；传 "*" 会抛异常导致恒返回 -1
        public static float CtxSwitchesSec() => GetPerfCounterF("System", "Context Switches/sec", "");
        /// <summary>DiskReadBytesSec 方法。</summary>
        public static float DiskReadBytesSec() => GetPerfCounterF("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
        /// <summary>DiskWriteBytesSec 方法。</summary>
        public static float DiskWriteBytesSec() => GetPerfCounterF("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
        // Network Interface 是多实例类别且不支持 "*" 通配实例名，需逐实例创建计数器再汇总
        public static float NetworkSentBytesSec() => GetNetworkCounterBytes("Bytes Sent/sec");
        /// <summary>NetworkReceivedBytesSec 方法。</summary>
        public static float NetworkReceivedBytesSec() => GetNetworkCounterBytes("Bytes Received/sec");

        /// <summary>获取 perfCounter。</summary>
        private static long GetPerfCounter(string cat, string cnt, string instance)
        {
            try { using (var p = new PerformanceCounter(cat, cnt, instance)) return (long)p.NextValue(); } catch { }
            return -1;
        }
        /// <summary>获取 perfCounterF。</summary>
        private static float GetPerfCounterF(string cat, string cnt, string instance = "*")
        {
            try { using (var p = new PerformanceCounter(cat, cnt, instance)) return p.NextValue(); } catch { }
            return -1f;
        }
        /// <summary>汇总所有网卡实例的网络字节速率（PerformanceCounter 不支持 "*" 通配实例名）。</summary>
        private static float GetNetworkCounterBytes(string counter)
        {
            float total = 0f;
            bool any = false;
            try
            {
                var category = new PerformanceCounterCategory("Network Interface");
                foreach (string instance in category.GetInstanceNames())
                {
                    try { using (var p = new PerformanceCounter("Network Interface", counter, instance)) { total += p.NextValue(); any = true; } }
                    catch { }
                }
            }
            catch { }
            return any ? total : -1f;
        }
        #endregion

        #region 【10】监视器
        public class MonitorInfo { public string Name; public int Width, Height; }
        /// <summary>获取 monitorInfo。</summary>
        public static List<MonitorInfo> GetMonitorInfo()
        {
            var l = new List<MonitorInfo>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
                    foreach (ManagementObject o in Enumerate(s))
                        l.Add(new MonitorInfo { Name = o["Name"]?.ToString(), Width = Convert.ToInt32(o["ScreenWidth"] ?? 0), Height = Convert.ToInt32(o["ScreenHeight"] ?? 0) });
            }
            catch { }
            return l;
        }
        /// <summary>ScreenResolutionAndRefresh 方法。</summary>
        public static string ScreenResolutionAndRefresh()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate FROM Win32_VideoController"))
                    foreach (ManagementObject o in Enumerate(s))
                        return $"{o["CurrentHorizontalResolution"]}x{o["CurrentVerticalResolution"]} @{o["CurrentRefreshRate"]}Hz";
            }
            catch { }
            return HTranslation.GetContent("未知");
        }
        #endregion

        #region 【11】事件日志（Application/System/Security）
        public class EventLogEntryItem
        {
            public string LogName, Level, Source, Message;
            public DateTime Time;
        }
        /// <param name="logName">"Application","System","Security"</param>
        /// <param name="maxEntries">最大条数</param>
        /// <param name="entryType">"Error","Warning","Information" 或 null 表示全部</param>
        public static List<EventLogEntryItem> GetEventLogs(string logName, int maxEntries = 20, string entryType = null)
        {
            var list = new List<EventLogEntryItem>();
            try
            {
                using (EventLog log = new EventLog(logName)) // 释放 EventLog 组件，避免事件日志句柄泄漏
                {
                    foreach (EventLogEntry e in log.Entries)
                    {
                        if (!string.IsNullOrEmpty(entryType) && !e.EntryType.ToString().Equals(entryType, StringComparison.OrdinalIgnoreCase)) continue;
                        list.Add(new EventLogEntryItem { LogName = logName, Level = e.EntryType.ToString(), Source = e.Source, Message = e.Message, Time = e.TimeGenerated });
                        if (list.Count >= maxEntries) break;
                    }
                }
            }
            catch { }
            return list;
        }
        #endregion

        #region 【12】防火墙状态
        /// <summary>FirewallStatus 方法。</summary>
        public static string FirewallStatus()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT State FROM Win32_Service WHERE Name='MpsSvc'"))
                    foreach (ManagementObject o in Enumerate(s)) return o["State"]?.ToString() == "Running" ? HTranslation.GetContent("已启用") : HTranslation.GetContent("已禁用");
            }
            catch { }
            return HTranslation.GetContent("未知");
        }
        #endregion

        #region 【13】.NET 版本
        /// <summary>获取 dotNetVersions。</summary>
        public static List<string> GetDotNetVersions()
        {
            var v = new List<string>();
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
                {
                    int release = Convert.ToInt32(key?.GetValue("Release") ?? 0);
                    if (release >= 528040) v.Add("4.8+");
                    else if (release >= 461808) v.Add("4.7.2");
                    else if (release >= 461308) v.Add("4.7.1");
                    else if (release >= 460798) v.Add("4.7");
                    else v.Add("4.x (Release=" + release + ")");
                }
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP"))
                {
                    foreach (string sub in key?.GetSubKeyNames() ?? new string[0])
                    {
                        using (var k = key.OpenSubKey(sub))
                            if (k?.GetValue("Install")?.ToString() == "1") v.Add(sub);
                    }
                }
            }
            catch { }
            return v.Distinct().ToList();
        }
        #endregion

        #region 【14】已安装程序
        /// <summary>获取 installedPrograms。</summary>
        public static List<string> GetInstalledPrograms()
        {
            var progs = new List<string>();
            try
            {
                foreach (string root in new[] { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall" })
                    using (var key = Registry.LocalMachine.OpenSubKey(root))
                    {
                        if (key == null) continue;
                        foreach (string sub in key.GetSubKeyNames())
                            using (var subKey = key.OpenSubKey(sub))
                            {
                                string name = subKey?.GetValue("DisplayName")?.ToString();
                                if (!string.IsNullOrEmpty(name)) progs.Add(name);
                            }
                    }
            }
            catch { }
            return progs;
        }
        #endregion

        #region 【15】系统还原点
        /// <summary>获取 restorePoints。</summary>
        public static List<string> GetRestorePoints()
        {
            var r = new List<string>();
            try { using (var s = new ManagementObjectSearcher("SELECT Description, CreationTime FROM SystemRestore")) foreach (ManagementObject o in Enumerate(s)) r.Add($"{o["Description"]} ({o["CreationTime"]})"); } catch { }
            return r;
        }
        #endregion

        #region 【16】Windows 更新（已安装补丁）
        /// <summary>获取 windowsUpdates。</summary>
        public static List<string> GetWindowsUpdates()
        {
            var updates = new List<string>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT HotFixID, Description, InstalledOn FROM Win32_QuickFixEngineering"))
                    foreach (ManagementObject o in Enumerate(s))
                        updates.Add($"{o["HotFixID"]} : {o["Description"]} ({o["InstalledOn"]})");
            }
            catch { }
            return updates;
        }
        #endregion

        #region 【17】计划任务
        /// <summary>获取 scheduledTasks。</summary>
        public static List<string> GetScheduledTasks()
        {
            var tasks = new List<string>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Name, Status FROM Win32_ScheduledJob"))
                    foreach (ManagementObject o in Enumerate(s)) tasks.Add($"{o["Name"]} ({o["Status"]})");
            }
            catch { }
            return tasks;
        }
        #endregion

        #region 【18】启动项（注册表常用位置）
        /// <summary>获取 startupItems。</summary>
        public static List<string> GetStartupItems()
        {
            var items = new List<string>();
            string[] keys = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\RunOnce"
            };
            foreach (string keyPath in keys)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key == null) continue;
                        foreach (string valueName in key.GetValueNames())
                            items.Add($"{keyPath}: {valueName} = {key.GetValue(valueName)}");
                    }
                }
                catch { }
            }
            // 当前用户 Run
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
                    foreach (string v in key.GetValueNames()) items.Add($"HKCU\\Run: {v} = {key.GetValue(v)}");
            }
            catch { }
            return items;
        }
        #endregion

        #region 【19】设备管理器（所有 PnP 实体）
        public class PnpDevice
        {
            public string Name, Status, Class, Manufacturer;
        }
        /// <summary>获取 allPnPDevices。</summary>
        public static List<PnpDevice> GetAllPnPDevices()
        {
            var list = new List<PnpDevice>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Name, Status, PNPClass, Manufacturer FROM Win32_PnPEntity"))
                    foreach (ManagementObject o in Enumerate(s))
                        list.Add(new PnpDevice { Name = o["Name"]?.ToString(), Status = o["Status"]?.ToString(), Class = o["PNPClass"]?.ToString(), Manufacturer = o["Manufacturer"]?.ToString() });
            }
            catch { }
            return list;
        }
        #endregion

        #region 【20】SMART 健康与温度
        /// <summary>获取 diskHealth。</summary>
        public static string GetDiskHealth()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT Status FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'"))
                    foreach (ManagementObject o in Enumerate(s)) return o["Status"]?.ToString() ?? HTranslation.GetContent("未知");
            }
            catch { }
            return HTranslation.GetContent("未知");
        }
        #endregion

        #region 【21】电源计划
        /// <summary>获取 activePowerPlan。</summary>
        public static string GetActivePowerPlan()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT InstanceID, ElementName FROM Win32_PowerPlan WHERE IsActive = True"))
                    foreach (ManagementObject o in Enumerate(s)) return o["ElementName"]?.ToString();
            }
            catch { }
            return HTranslation.GetContent("未知");
        }
        #endregion

        #region 【22】屏幕详细信息
        /// <summary>获取 screenInfo。</summary>
        public static string GetScreenInfo()
        {
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate, CurrentBitsPerPixel FROM Win32_VideoController"))
                    foreach (ManagementObject o in Enumerate(s))
                        return $"{o["CurrentHorizontalResolution"]}x{o["CurrentVerticalResolution"]} {o["CurrentBitsPerPixel"]}bit @{o["CurrentRefreshRate"]}Hz";
            }
            catch { }
            return HTranslation.GetContent("未知");
        }
        #endregion

        #region 【23】Hyper‑V 状态
        /// <summary>判断是否 HyperVEnabled。</summary>
        public static bool IsHyperVEnabled()
        {
            try
            {
                // 修正：功能记录存在不代表已启用，必须判断 InstallState（1=已启用, 2=已禁用, 3=不存在）
                using (var s = new ManagementObjectSearcher("SELECT InstallState FROM Win32_OptionalFeature WHERE Name='Microsoft-Hyper-V-All'"))
                    foreach (ManagementObject o in Enumerate(s))
                        return Convert.ToInt32(o["InstallState"] ?? 0) == 1;
            }
            catch { }
            return false;
        }
        #endregion

        #region 【24】终端服务（远程桌面）
        /// <summary>判断是否 RemoteDesktopEnabled。</summary>
        public static bool IsRemoteDesktopEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server"))
                    return Convert.ToInt32(key?.GetValue("fDenyTSConnections") ?? 1) == 0;
            }
            catch { }
            return false;
        }
        #endregion

        #region 【25】时区偏移
        /// <summary>TimeZoneOffsetHours 方法。</summary>
        public static double TimeZoneOffsetHours() => TimeZoneInfo.Local.BaseUtcOffset.TotalHours;
        #endregion

        #region 【26】语言包
        /// <summary>获取 installedLanguages。</summary>
        public static List<string> GetInstalledLanguages()
        {
            var langs = new List<string>();
            try
            {
                // 修正：Nls\Language 下只有注册表值（InstallLanguage 等）、没有子键，原实现永远返回空；
                // 已安装 MUI 语言应读取 Win32_OperatingSystem.MUILanguages 字符串数组
                using (var s = new ManagementObjectSearcher("SELECT MUILanguages FROM Win32_OperatingSystem"))
                    foreach (ManagementObject o in Enumerate(s))
                    {
                        string[] arr = o["MUILanguages"] as string[];
                        if (arr != null) langs.AddRange(arr);
                    }
            }
            catch { }
            return langs;
        }
        #endregion

        #region 综合报告
        /// <summary>FullReport 方法。</summary>
        public static string FullReport()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"======== {HTranslation.GetContent("系统基本信息")} ========");
            sb.AppendLine($"{HTranslation.GetContent("名称")}: {ComputerName}  {HTranslation.GetContent("用户")}: {UserName}");
            sb.AppendLine($"{HTranslation.GetContent("OS")}: {OsVersion()} ({OsArchitecture()})  {HTranslation.GetContent("语言")}: {SystemLanguage}  {HTranslation.GetContent("时区")}: {TimeZoneName} ({TimeZoneOffsetHours():F1}h)");
            sb.AppendLine($"{HTranslation.GetContent("制造商")}: {Manufacturer()}  {HTranslation.GetContent("型号")}: {Model()}");
            sb.AppendLine($"{HTranslation.GetContent("域")}: {Domain()}  {HTranslation.GetContent("域控")}: {(IsDomainController() ? HTranslation.GetContent("是") : HTranslation.GetContent("否"))}");
            sb.AppendLine($"{HTranslation.GetContent("产品ID")}: {WindowsProductId()}  {HTranslation.GetContent("UUID")}: {ComputerUuid()}");
            sb.AppendLine($"{HTranslation.GetContent("启动时间")}: {BootTime():yyyy-MM-dd HH:mm:ss}  {HTranslation.GetContent("运行时间")}: {UpTime}");
            sb.AppendLine($"{HTranslation.GetContent("远程桌面")}: {(IsRemoteDesktopEnabled() ? HTranslation.GetContent("已启用") : HTranslation.GetContent("已禁用"))}  Hyper-V: {(IsHyperVEnabled() ? HTranslation.GetContent("已启用") : HTranslation.GetContent("已禁用"))}");
            sb.AppendLine();

            sb.AppendLine($"======== CPU ========");
            foreach (var c in GetCpuInfo())
                sb.AppendLine($"{c.Name} ({c.NumberOfCores}C/{c.NumberOfLogicalProcessors}T) {c.CurrentClockSpeed}MHz {c.Architecture}");
            sb.AppendLine($"{HTranslation.GetContent("使用率")}: {GetCpuUsagePercent():F1}%");
            sb.AppendLine();

            sb.AppendLine($"======== {HTranslation.GetContent("内存")} ========");
            sb.AppendLine($"{HTranslation.GetContent("物理")}: {FormatBytes(GetAvailableMemoryBytes())}/{FormatBytes(GetTotalMemoryBytes())} ({GetMemoryUsagePercent():F1}%)");
            sb.AppendLine($"{HTranslation.GetContent("虚拟")}: {FormatBytes(GetAvailableVirtualMemoryBytes())}/{FormatBytes(GetTotalVirtualMemoryBytes())}");
            sb.AppendLine();

            sb.AppendLine($"======== {HTranslation.GetContent("磁盘")} ========");
            foreach (var d in GetLogicalDiskInfo())
                sb.AppendLine($"{d.Name} {d.VolumeName} {d.FileSystem} {FormatBytes(d.FreeSpace)}/{FormatBytes(d.TotalSize)} ({d.DriveType})");
            sb.AppendLine();

            sb.AppendLine($"======== {HTranslation.GetContent("网络")} ========");
            foreach (var n in GetNetworkAdapterDetails())
                sb.AppendLine($"{n.Name}: {n.MACAddress} {n.ConnectionStatus} IP={string.Join(",", n.IPAddresses)}");
            sb.AppendLine($"{HTranslation.GetContent("TCP连接数")}: {GetTcpConnectionsCount()}  {HTranslation.GetContent("共享文件夹")}: {string.Join("; ", GetNetworkShares())}");
            sb.AppendLine();

            sb.AppendLine($"======== {HTranslation.GetContent("显卡/BIOS/电源")} ========");
            foreach (var g in GetVideoControllerInfo()) sb.AppendLine($"{g.Name} {FormatBytes(g.AdapterRAM)}");
            var bios = GetBiosInfo(); sb.AppendLine($"BIOS: {bios.Manufacturer} {bios.Version}");
            var bat = GetBatteryStatus(); sb.AppendLine(bat.ChargePercent >= 0 ? $"{HTranslation.GetContent("电池")}: {bat.ChargePercent}% {bat.Status}" : $"{HTranslation.GetContent("电池")}: {HTranslation.GetContent("无")}");
            sb.AppendLine($"{HTranslation.GetContent("屏幕")}: {GetScreenInfo()}");
            sb.AppendLine();

            sb.AppendLine($"======== {HTranslation.GetContent("安全与更新")} ========");
            sb.AppendLine($"{HTranslation.GetContent("防火墙")}: {FirewallStatus()}  .NET: {string.Join(",", GetDotNetVersions())}");
            sb.AppendLine($"{HTranslation.GetContent("补丁数")}: {GetWindowsUpdates().Count}");
            sb.AppendLine($"{HTranslation.GetContent("系统日志(错误,最近5条)")}:");
            foreach (var e in GetEventLogs("System", 5, "Error")) sb.AppendLine($"  [{e.Time}] {e.Source}: {e.Message}");
            sb.AppendLine();

            sb.AppendLine($"======== {HTranslation.GetContent("进程/服务")} ========");
            sb.AppendLine($"{HTranslation.GetContent("进程数")}: {GetProcessList().Count}  {HTranslation.GetContent("服务数")}: {GetServiceList().Count}");
            sb.AppendLine($"{HTranslation.GetContent("句柄")}: {TotalHandles()}  {HTranslation.GetContent("线程")}: {TotalThreads()}");

            return sb.ToString();
        }
        #endregion
    }
}
