using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using HFromUI; // HTranslation 所在命名空间

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 全面系统硬件信息查询类（终极版，集成多语言翻译，兼容 .NET 4.8 / C# 7.3）。
    /// 提供 CPU、主板、BIOS、内存、硬盘、显卡、声卡、网卡、电池、显示器、
    /// USB设备、打印机、风扇、电压、SMART、系统插槽、温度等超过 20 类硬件信息的获取。
    /// 所有数据均通过 WMI (System.Management) 获取，异常安全。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// </summary>
    public  class HHardwareInfo
    {

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

        #region 信息类定义

        /// <summary>CPU 信息。</summary>
        public class ProcessorInfo
        {
            /// <summary>处理器名称</summary>
            public string Name { get; set; }
            /// <summary>制造商</summary>
            public string Manufacturer { get; set; }
            /// <summary>最大时钟频率 (MHz)</summary>
            public uint MaxClockSpeed { get; set; }
            /// <summary>当前时钟频率 (MHz)</summary>
            public uint CurrentClockSpeed { get; set; }
            /// <summary>物理核心数</summary>
            public uint NumberOfCores { get; set; }
            /// <summary>逻辑处理器数</summary>
            public uint NumberOfLogicalProcessors { get; set; }
            /// <summary>架构 (已翻译)</summary>
            public string Architecture { get; set; }
            /// <summary>L2 缓存大小 (KB)</summary>
            public uint L2CacheSize { get; set; }
            /// <summary>L3 缓存大小 (KB)</summary>
            public uint L3CacheSize { get; set; }
        }

        /// <summary>主板信息。</summary>
        public class BaseBoardInfo
        {
            /// <summary>Manufacturer 成员。</summary>
            public string Manufacturer { get; set; }
            /// <summary>Product 成员。</summary>
            public string Product { get; set; }
            /// <summary>SerialNumber 成员。</summary>
            public string SerialNumber { get; set; }
            /// <summary>Version 成员。</summary>
            public string Version { get; set; }
        }

        /// <summary>BIOS 信息。</summary>
        public class BiosInfo
        {
            /// <summary>Manufacturer 成员。</summary>
            public string Manufacturer { get; set; }
            /// <summary>Version 成员。</summary>
            public string Version { get; set; }
            /// <summary>ReleaseDate 成员。</summary>
            public string ReleaseDate { get; set; }
            /// <summary>SMBIOSBIOSVersion 成员。</summary>
            public string SMBIOSBIOSVersion { get; set; }
            /// <summary>SerialNumber 成员。</summary>
            public string SerialNumber { get; set; }
        }

        /// <summary>内存条信息。</summary>
        public class PhysicalMemoryInfo
        {
            /// <summary>BankLabel 成员。</summary>
            public string BankLabel { get; set; }
            /// <summary>Capacity 成员。</summary>
            public ulong Capacity { get; set; }
            /// <summary>Speed 成员。</summary>
            public uint Speed { get; set; }
            /// <summary>Manufacturer 成员。</summary>
            public string Manufacturer { get; set; }
            /// <summary>SerialNumber 成员。</summary>
            public string SerialNumber { get; set; }
            /// <summary>PartNumber 成员。</summary>
            public string PartNumber { get; set; }
            /// <summary>MemoryType 成员。</summary>
            public string MemoryType { get; set; }
            /// <summary>FormFactor 成员。</summary>
            public string FormFactor { get; set; }
        }

        /// <summary>磁盘驱动器信息。</summary>
        public class DiskDriveInfo
        {
            /// <summary>Model 成员。</summary>
            public string Model { get; set; }
            /// <summary>屏幕尺寸。</summary>
            public ulong Size { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
            /// <summary>InterfaceType 成员。</summary>
            public string InterfaceType { get; set; }
            /// <summary>MediaType 成员。</summary>
            public string MediaType { get; set; }
            /// <summary>SerialNumber 成员。</summary>
            public string SerialNumber { get; set; }
            /// <summary>Temperature 成员。</summary>
            public int Temperature { get; set; }
        }

        /// <summary>显卡信息。</summary>
        public class VideoControllerInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>AdapterRAM 成员。</summary>
            public ulong AdapterRAM { get; set; }
            /// <summary>DriverVersion 成员。</summary>
            public string DriverVersion { get; set; }
            /// <summary>CurrentHorizontalResolution 成员。</summary>
            public uint CurrentHorizontalResolution { get; set; }
            /// <summary>CurrentVerticalResolution 成员。</summary>
            public uint CurrentVerticalResolution { get; set; }
            /// <summary>CurrentRefreshRate 成员。</summary>
            public uint CurrentRefreshRate { get; set; }
            /// <summary>VideoProcessor 成员。</summary>
            public string VideoProcessor { get; set; }
        }

        /// <summary>声卡信息。</summary>
        public class SoundDeviceInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>Manufacturer 成员。</summary>
            public string Manufacturer { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
        }

        /// <summary>网络适配器简要信息。</summary>
        public class NetworkAdapterBriefInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>MACAddress 成员。</summary>
            public string MACAddress { get; set; }
            /// <summary>AdapterType 成员。</summary>
            public string AdapterType { get; set; }
            /// <summary>NetConnectionStatus 成员。</summary>
            public string NetConnectionStatus { get; set; }
        }

        /// <summary>电池信息。</summary>
        public class BatteryInfo
        {
            /// <summary>EstimatedChargeRemaining 成员。</summary>
            public ushort EstimatedChargeRemaining { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
            /// <summary>EstimatedRunTime 成员。</summary>
            public uint EstimatedRunTime { get; set; }
        }

        /// <summary>显示器信息。</summary>
        public class DesktopMonitorInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>ScreenWidth 成员。</summary>
            public uint ScreenWidth { get; set; }
            /// <summary>ScreenHeight 成员。</summary>
            public uint ScreenHeight { get; set; }
        }

        /// <summary>温度信息。</summary>
        public class TemperatureInfo
        {
            /// <summary>DeviceName 成员。</summary>
            public string DeviceName { get; set; }
            /// <summary>Temperature 成员。</summary>
            public double Temperature { get; set; }
        }

        /// <summary>处理器扩展信息。</summary>
        public class ProcessorInfoEx
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>SocketDesignation 成员。</summary>
            public string SocketDesignation { get; set; }
            /// <summary>ProcessorId 成员。</summary>
            public ushort ProcessorId { get; set; }
            /// <summary>Description 成员。</summary>
            public string Description { get; set; }
            /// <summary>IsEnabled 成员。</summary>
            public bool IsEnabled { get; set; }
            /// <summary>Role 成员。</summary>
            public string Role { get; set; }
        }

        /// <summary>内存条扩展信息。</summary>
        public class PhysicalMemoryInfoEx
        {
            /// <summary>BankLabel 成员。</summary>
            public string BankLabel { get; set; }
            /// <summary>Capacity 成员。</summary>
            public ulong Capacity { get; set; }
            /// <summary>Speed 成员。</summary>
            public uint Speed { get; set; }
            /// <summary>Manufacturer 成员。</summary>
            public string Manufacturer { get; set; }
            /// <summary>SerialNumber 成员。</summary>
            public string SerialNumber { get; set; }
            /// <summary>PartNumber 成员。</summary>
            public string PartNumber { get; set; }
            /// <summary>MemoryType 成员。</summary>
            public string MemoryType { get; set; }
            /// <summary>FormFactor 成员。</summary>
            public string FormFactor { get; set; }
            /// <summary>ConfiguredClockSpeed 成员。</summary>
            public uint ConfiguredClockSpeed { get; set; }
            /// <summary>ConfiguredVoltage 成员。</summary>
            public uint ConfiguredVoltage { get; set; }
            /// <summary>DeviceLocator 成员。</summary>
            public string DeviceLocator { get; set; }
        }

        /// <summary>磁盘分区信息。</summary>
        public class DiskPartitionInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>Description 成员。</summary>
            public string Description { get; set; }
            /// <summary>屏幕尺寸。</summary>
            public ulong Size { get; set; }
            /// <summary>Type 成员。</summary>
            public string Type { get; set; }
            /// <summary>BootPartition 成员。</summary>
            public string BootPartition { get; set; }
        }

        /// <summary>风扇信息。</summary>
        public class FanInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>DesiredSpeed 成员。</summary>
            public uint DesiredSpeed { get; set; }
            /// <summary>VariableSpeed 成员。</summary>
            public bool VariableSpeed { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
        }

        /// <summary>电压信息。</summary>
        public class VoltageInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>CurrentVoltage 成员。</summary>
            public uint CurrentVoltage { get; set; }
            /// <summary>MinVoltage 成员。</summary>
            public uint MinVoltage { get; set; }
            /// <summary>MaxVoltage 成员。</summary>
            public uint MaxVoltage { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
        }

        /// <summary>USB 设备信息。</summary>
        public class USBDeviceInfo
        {
            /// <summary>Description 成员。</summary>
            public string Description { get; set; }
            /// <summary>Manufacturer 成员。</summary>
            public string Manufacturer { get; set; }
            /// <summary>Service 成员。</summary>
            public string Service { get; set; }
            /// <summary>PNPDeviceID 成员。</summary>
            public string PNPDeviceID { get; set; }
        }

        /// <summary>打印机信息。</summary>
        public class PrinterInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>DriverName 成员。</summary>
            public string DriverName { get; set; }
            /// <summary>PortName 成员。</summary>
            public string PortName { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
        }

        /// <summary>详细网卡信息（含 IP 配置）。</summary>
        public class NetworkAdapterInfoEx
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>MACAddress 成员。</summary>
            public string MACAddress { get; set; }
            /// <summary>IPAddress 成员。</summary>
            public string IPAddress { get; set; }
            /// <summary>IPSubnet 成员。</summary>
            public string IPSubnet { get; set; }
            /// <summary>DefaultGateway 成员。</summary>
            public string DefaultGateway { get; set; }
            /// <summary>DHCPServer 成员。</summary>
            public string DHCPServer { get; set; }
            /// <summary>DNSHostName 成员。</summary>
            public string DNSHostName { get; set; }
            /// <summary>DHCPEnabled 成员。</summary>
            public bool DHCPEnabled { get; set; }
            /// <summary>AdapterType 成员。</summary>
            public string AdapterType { get; set; }
            /// <summary>Speed 成员。</summary>
            public ulong Speed { get; set; }
            /// <summary>NetConnectionStatus 成员。</summary>
            public string NetConnectionStatus { get; set; }
        }

        /// <summary>电池扩展信息。</summary>
        public class BatteryInfoEx
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>EstimatedChargeRemaining 成员。</summary>
            public ushort EstimatedChargeRemaining { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
            /// <summary>EstimatedRunTime 成员。</summary>
            public uint EstimatedRunTime { get; set; }
            /// <summary>DesignCapacity 成员。</summary>
            public uint DesignCapacity { get; set; }
            /// <summary>FullChargeCapacity 成员。</summary>
            public uint FullChargeCapacity { get; set; }
            /// <summary>Chemistry 成员。</summary>
            public string Chemistry { get; set; }
        }

        /// <summary>系统插槽信息。</summary>
        public class SystemSlotInfo
        {
            /// <summary>SlotDesignation 成员。</summary>
            public string SlotDesignation { get; set; }
            /// <summary>SlotType 成员。</summary>
            public string SlotType { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
            /// <summary>CurrentUsage 成员。</summary>
            public string CurrentUsage { get; set; }
        }

        /// <summary>SMART 属性信息（简化）。</summary>
        public class SMARTAttributeInfo
        {
            /// <summary>AttributeName 成员。</summary>
            public string AttributeName { get; set; }
            /// <summary>值。</summary>
            public int Value { get; set; }
            /// <summary>Worst 成员。</summary>
            public int Worst { get; set; }
            /// <summary>Threshold 成员。</summary>
            public int Threshold { get; set; }
            /// <summary>Status 成员。</summary>
            public string Status { get; set; }
        }

        #endregion

        #region CPU 信息

        /// <summary>获取所有 CPU 的详细信息。</summary>
        public static List<ProcessorInfo> GetProcessorInfo()
        {
            var list = new List<ProcessorInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        var cpu = new ProcessorInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            MaxClockSpeed = Convert.ToUInt32(obj["MaxClockSpeed"] ?? 0),
                            CurrentClockSpeed = Convert.ToUInt32(obj["CurrentClockSpeed"] ?? 0),
                            NumberOfCores = Convert.ToUInt32(obj["NumberOfCores"] ?? 0),
                            NumberOfLogicalProcessors = Convert.ToUInt32(obj["NumberOfLogicalProcessors"] ?? 0),
                            L2CacheSize = Convert.ToUInt32(obj["L2CacheSize"] ?? 0),
                            L3CacheSize = Convert.ToUInt32(obj["L3CacheSize"] ?? 0)
                        };

                        if (obj["Architecture"] != null)
                        {
                            ushort arch = Convert.ToUInt16(obj["Architecture"]);
                            switch (arch)
                            {
                                case 0: cpu.Architecture = HTranslation.GetContent("x86"); break;
                                case 1: cpu.Architecture = HTranslation.GetContent("MIPS"); break;
                                case 2: cpu.Architecture = HTranslation.GetContent("Alpha"); break;
                                case 3: cpu.Architecture = HTranslation.GetContent("PowerPC"); break;
                                case 5: cpu.Architecture = HTranslation.GetContent("ARM"); break;
                                case 6: cpu.Architecture = HTranslation.GetContent("IA64"); break;
                                case 9: cpu.Architecture = HTranslation.GetContent("x64"); break;
                                default: cpu.Architecture = HTranslation.GetContent("未知"); break;
                            }
                        }
                        else cpu.Architecture = HTranslation.GetContent("未知");

                        list.Add(cpu);
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 主板信息

        /// <summary>获取主板信息。</summary>
        public static List<BaseBoardInfo> GetBaseBoardInfo()
        {
            var list = new List<BaseBoardInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new BaseBoardInfo
                        {
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            Product = obj["Product"]?.ToString() ?? "",
                            SerialNumber = obj["SerialNumber"]?.ToString() ?? "",
                            Version = obj["Version"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region BIOS 信息

        /// <summary>获取 BIOS 信息。</summary>
        public static List<BiosInfo> GetBiosInfo()
        {
            var list = new List<BiosInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new BiosInfo
                        {
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            Version = obj["Version"]?.ToString() ?? "",
                            ReleaseDate = obj["ReleaseDate"]?.ToString() ?? "",
                            SMBIOSBIOSVersion = obj["SMBIOSBIOSVersion"]?.ToString() ?? "",
                            SerialNumber = obj["SerialNumber"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 内存条信息

        /// <summary>获取物理内存条信息。</summary>
        public static List<PhysicalMemoryInfo> GetPhysicalMemoryInfo()
        {
            var list = new List<PhysicalMemoryInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        var info = new PhysicalMemoryInfo
                        {
                            BankLabel = obj["BankLabel"]?.ToString() ?? "",
                            Capacity = Convert.ToUInt64(obj["Capacity"] ?? 0),
                            Speed = Convert.ToUInt32(obj["Speed"] ?? 0),
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            SerialNumber = obj["SerialNumber"]?.ToString() ?? "",
                            PartNumber = obj["PartNumber"]?.ToString() ?? ""
                        };
                        info.MemoryType = MapMemoryType(ReadMemType(obj));
                        info.FormFactor = obj["FormFactor"] != null
                            ? MapFormFactor(Convert.ToUInt32(obj["FormFactor"])) : HTranslation.GetContent("未知");
                        list.Add(info);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>MapMemoryType 方法。</summary>
        private static string MapMemoryType(uint type)
        {
            switch (type)
            {
                case 20: return HTranslation.GetContent("DDR");
                case 21: return HTranslation.GetContent("DDR2");
                case 24: return HTranslation.GetContent("DDR3");
                case 26: return HTranslation.GetContent("DDR4");
                case 34: return HTranslation.GetContent("DDR5");
                default: return HTranslation.GetContent("未知") + " (" + type + ")";
            }
        }

        /// <summary>MapFormFactor 方法。</summary>
        private static string MapFormFactor(uint factor)
        {
            switch (factor)
            {
                case 0: return HTranslation.GetContent("未知");
                case 1: return HTranslation.GetContent("其他");
                case 8: return HTranslation.GetContent("DIMM");
                case 9: return HTranslation.GetContent("TSOP");
                case 12: return HTranslation.GetContent("SODIMM");
                case 11: return HTranslation.GetContent("RIMM");
                default: return HTranslation.GetContent("其他") + " (" + factor + ")";
            }
        }

        /// <summary>读取内存类型；MemoryType 在 DDR4/DDR5 平台常返回 0，此时回退到 SMBIOSMemoryType（两者 DDR 世代码值一致）。</summary>
        private static uint ReadMemType(ManagementObject obj)
        {
            uint t = obj["MemoryType"] != null ? Convert.ToUInt32(obj["MemoryType"]) : 0u;
            if (t == 0u && obj["SMBIOSMemoryType"] != null) t = Convert.ToUInt32(obj["SMBIOSMemoryType"]);
            return t;
        }

        /// <summary>WMI 通用 Status 字符串（英文，如 OK/Error/Pred Fail）映射为中文；翻译函数只接受中文词条，不能直接传英文。</summary>
        private static string MapCimStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return HTranslation.GetContent("未知");
            switch (status.Trim())
            {
                case "OK": return HTranslation.GetContent("正常");
                case "Error": return HTranslation.GetContent("错误");
                case "Degraded": return HTranslation.GetContent("降级");
                case "Pred Fail": return HTranslation.GetContent("预测故障");
                case "Starting": return HTranslation.GetContent("启动中");
                case "Stopping": return HTranslation.GetContent("停止中");
                case "Service": return HTranslation.GetContent("维护中");
                case "Stressed": return HTranslation.GetContent("过载");
                case "NonRecover": return HTranslation.GetContent("不可恢复");
                case "No Contact": return HTranslation.GetContent("无连接");
                case "Lost Comm": return HTranslation.GetContent("通信丢失");
                case "Unknown": return HTranslation.GetContent("未知");
                default: return status;
            }
        }

        #endregion

        #region 硬盘信息

        /// <summary>获取所有磁盘驱动器信息，含 SMART 状态和温度。</summary>
        public static List<DiskDriveInfo> GetDiskDriveInfo()
        {
            var list = new List<DiskDriveInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        var disk = new DiskDriveInfo
                        {
                            Model = obj["Model"]?.ToString() ?? "",
                            Size = Convert.ToUInt64(obj["Size"] ?? 0),
                            // WMI 的 Status 是英文枚举（OK/Error/Pred Fail…），不能直接送翻译函数，需显式映射
                            Status = obj["Status"] != null ? MapCimStatus(obj["Status"].ToString()) : HTranslation.GetContent("未知"),
                            InterfaceType = obj["InterfaceType"]?.ToString() ?? "",
                            // MediaType 为英文描述（如 Fixed hard disk media），原样保留，不送翻译
                            MediaType = obj["MediaType"]?.ToString() ?? "",
                            SerialNumber = obj["SerialNumber"]?.ToString()?.Trim() ?? ""
                        };
                        disk.Temperature = GetDiskTemperature(obj["DeviceID"]?.ToString());
                        list.Add(disk);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取 diskTemperature。</summary>
        private static int GetDiskTemperature(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return -1;
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + deviceId + "'} WHERE AssocClass = Win32_DiskDrivePhysicalMedia"))
                {
                    foreach (ManagementObject physicalMedia in Enumerate(searcher))
                    {
                        // Tag 形如 \\.\PHYSICALDRIVE0，WQL 字符串字面量中的反斜杠必须转义为双反斜杠
                        string tag = physicalMedia["Tag"]?.ToString().Replace("\\", "\\\\") ?? "";
                        using (var smartSearcher = new ManagementObjectSearcher(
                            "ASSOCIATORS OF {Win32_PhysicalMedia.Tag='" + tag + "'} WHERE AssocClass = Win32_PhysicalMediaToSmartData"))
                        {
                            foreach (ManagementObject smartData in Enumerate(smartSearcher))
                            {
                                if (smartData["Temperature"] != null)
                                    return Convert.ToInt32(smartData["Temperature"]);
                            }
                        }
                    }
                }
            }
            catch { }
            return -1;
        }

        #endregion

        #region 显卡信息

        /// <summary>获取所有显卡信息。</summary>
        public static List<VideoControllerInfo> GetVideoControllerInfo()
        {
            var list = new List<VideoControllerInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new VideoControllerInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            AdapterRAM = Convert.ToUInt64(obj["AdapterRAM"] ?? 0),
                            DriverVersion = obj["DriverVersion"]?.ToString() ?? "",
                            CurrentHorizontalResolution = Convert.ToUInt32(obj["CurrentHorizontalResolution"] ?? 0),
                            CurrentVerticalResolution = Convert.ToUInt32(obj["CurrentVerticalResolution"] ?? 0),
                            CurrentRefreshRate = Convert.ToUInt32(obj["CurrentRefreshRate"] ?? 0),
                            VideoProcessor = obj["VideoProcessor"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 声卡信息

        /// <summary>获取所有声卡信息。</summary>
        public static List<SoundDeviceInfo> GetSoundDeviceInfo()
        {
            var list = new List<SoundDeviceInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new SoundDeviceInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            Status = obj["Status"] != null ? MapCimStatus(obj["Status"].ToString()) : HTranslation.GetContent("未知")
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 网卡简要信息

        /// <summary>获取所有物理网卡的基本信息。</summary>
        public static List<NetworkAdapterBriefInfo> GetNetworkAdapterBriefInfo()
        {
            var list = new List<NetworkAdapterBriefInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=True"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new NetworkAdapterBriefInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            MACAddress = obj["MACAddress"]?.ToString() ?? "",
                            AdapterType = obj["AdapterType"]?.ToString() ?? "",
                            NetConnectionStatus = ConvertNetStatus(Convert.ToUInt16(obj["NetConnectionStatus"] ?? 0))
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>ConvertNetStatus 方法。</summary>
        private static string ConvertNetStatus(ushort status)
        {
            // Win32_NetworkAdapter.NetConnectionStatus 官方码表，原 4~7 标签含义全部错位
            switch (status)
            {
                case 0: return HTranslation.GetContent("已断开");
                case 1: return HTranslation.GetContent("连接中");
                case 2: return HTranslation.GetContent("已连接");
                case 3: return HTranslation.GetContent("断开中");
                case 4: return HTranslation.GetContent("硬件不存在");
                case 5: return HTranslation.GetContent("硬件已禁用");
                case 6: return HTranslation.GetContent("硬件故障");
                case 7: return HTranslation.GetContent("介质断开");
                case 8: return HTranslation.GetContent("认证中");
                case 9: return HTranslation.GetContent("认证成功");
                case 10: return HTranslation.GetContent("认证失败");
                default: return HTranslation.GetContent("未知") + " (" + status + ")";
            }
        }

        #endregion

        #region 电池信息

        /// <summary>获取电池信息。</summary>
        public static List<BatteryInfo> GetBatteryInfo()
        {
            var list = new List<BatteryInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new BatteryInfo
                        {
                            EstimatedChargeRemaining = Convert.ToUInt16(obj["EstimatedChargeRemaining"] ?? 0),
                            // 电池充放电状态应取 BatteryStatus 数值码；Status 字符串只有 OK/Error 之类
                            Status = MapBatteryStatus(obj["BatteryStatus"] != null ? Convert.ToUInt16(obj["BatteryStatus"]) : (ushort)0),
                            EstimatedRunTime = Convert.ToUInt32(obj["EstimatedRunTime"] ?? 0)
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>Win32_Battery.BatteryStatus 数值码映射为中文状态（1=放电,2=接通交流电源,3=已充满…）。</summary>
        private static string MapBatteryStatus(ushort code)
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

        /// <summary>Win32_Battery.Chemistry 为 uint16 枚举，映射为中文电池类型。</summary>
        private static string MapChemistry(ushort c)
        {
            switch (c)
            {
                case 1: return HTranslation.GetContent("其他");
                case 2: return HTranslation.GetContent("未知");
                case 3: return HTranslation.GetContent("铅酸电池");
                case 4: return HTranslation.GetContent("镍镉电池");
                case 5: return HTranslation.GetContent("镍氢电池");
                case 6: return HTranslation.GetContent("锂离子电池");
                case 7: return HTranslation.GetContent("锌空气电池");
                case 8: return HTranslation.GetContent("锂聚合物电池");
                default: return HTranslation.GetContent("未知") + " (" + c + ")";
            }
        }

        #endregion

        #region 显示器信息

        /// <summary>获取所有显示器信息。</summary>
        public static List<DesktopMonitorInfo> GetDesktopMonitorInfo()
        {
            var list = new List<DesktopMonitorInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new DesktopMonitorInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            ScreenWidth = Convert.ToUInt32(obj["ScreenWidth"] ?? 0),
                            ScreenHeight = Convert.ToUInt32(obj["ScreenHeight"] ?? 0)
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 温度信息

        /// <summary>获取系统各部件的温度（CPU、GPU、主板、硬盘）。</summary>
        public static List<TemperatureInfo> GetTemperatures()
        {
            var temps = new List<TemperatureInfo>();

            // CPU 温度
            double cpuTemp = GetCpuTemperatureFromWmi();
            temps.Add(new TemperatureInfo { DeviceName = HTranslation.GetContent("CPU"), Temperature = cpuTemp });

            // GPU 温度
            double gpuTemp = GetGpuTemperatureFromWmi();
            temps.Add(new TemperatureInfo { DeviceName = HTranslation.GetContent("GPU"), Temperature = gpuTemp });

            // 主板温度
            double mbTemp = GetMotherboardTemperatureFromWmi();
            temps.Add(new TemperatureInfo { DeviceName = HTranslation.GetContent("主板"), Temperature = mbTemp });

            // 硬盘温度
            var disks = GetDiskDriveInfo();
            foreach (var disk in disks)
            {
                temps.Add(new TemperatureInfo
                {
                    DeviceName = HTranslation.GetContent("硬盘") + " (" + disk.Model + ")",
                    Temperature = disk.Temperature
                });
            }

            return temps;
        }

        /// <summary>获取 cpuTemperatureFromWmi。</summary>
        private static double GetCpuTemperatureFromWmi()
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"root\WMI");
                scope.Connect();
                ObjectQuery query = new ObjectQuery("SELECT * FROM MSAcpi_ThermalZoneTemperature");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
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

        /// <summary>获取 gpuTemperatureFromWmi。</summary>
        private static double GetGpuTemperatureFromWmi()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        if (obj["AdapterTemperature"] != null)
                            return Convert.ToDouble(obj["AdapterTemperature"]);
                    }
                }
            }
            catch { }
            return -1;
        }

        /// <summary>获取 motherboardTemperatureFromWmi。</summary>
        private static double GetMotherboardTemperatureFromWmi()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_TemperatureProbe"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        if (obj["CurrentReading"] != null)
                            return Convert.ToUInt32(obj["CurrentReading"]) / 10.0;
                    }
                }
            }
            catch { }
            return -1;
        }

        #endregion

        #region 扩展查询

        /// <summary>获取处理器扩展信息。</summary>
        public static List<ProcessorInfoEx> GetProcessorInfoEx()
        {
            var list = new List<ProcessorInfoEx>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        // 注意：Win32_Processor.ProcessorId 是十六进制字符串（如 BFEBFBFF000906EA），
                        // 直接 Convert.ToUInt16 会抛 FormatException 导致整个方法返回空列表；此处安全解析，失败为 0
                        string pidText = obj["ProcessorId"]?.ToString();
                        ushort pidValue = ushort.TryParse(pidText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort pidParsed) ? pidParsed : (ushort)0;
                        list.Add(new ProcessorInfoEx
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            SocketDesignation = obj["SocketDesignation"]?.ToString() ?? "",
                            ProcessorId = pidValue,
                            Description = obj["Description"]?.ToString() ?? "",
                            IsEnabled = obj["CpuStatus"] != null ? obj["CpuStatus"].ToString() == "1" : true,
                            Role = obj["Role"]?.ToString() ?? HTranslation.GetContent("CPU")
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取内存条扩展信息。</summary>
        public static List<PhysicalMemoryInfoEx> GetPhysicalMemoryInfoEx()
        {
            var list = new List<PhysicalMemoryInfoEx>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        var item = new PhysicalMemoryInfoEx
                        {
                            BankLabel = obj["BankLabel"]?.ToString() ?? "",
                            Capacity = Convert.ToUInt64(obj["Capacity"] ?? 0),
                            Speed = Convert.ToUInt32(obj["Speed"] ?? 0),
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            SerialNumber = obj["SerialNumber"]?.ToString() ?? "",
                            PartNumber = obj["PartNumber"]?.ToString() ?? "",
                            DeviceLocator = obj["DeviceLocator"]?.ToString() ?? "",
                            ConfiguredClockSpeed = obj["ConfiguredClockSpeed"] != null ? Convert.ToUInt32(obj["ConfiguredClockSpeed"]) : 0,
                            ConfiguredVoltage = obj["ConfiguredVoltage"] != null ? Convert.ToUInt32(obj["ConfiguredVoltage"]) : 0
                        };
                        item.MemoryType = MapMemoryType(ReadMemType(obj));
                        item.FormFactor = obj["FormFactor"] != null ? MapFormFactor(Convert.ToUInt32(obj["FormFactor"])) : HTranslation.GetContent("未知");
                        list.Add(item);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取磁盘分区信息。</summary>
        public static List<DiskPartitionInfo> GetDiskPartitions()
        {
            var list = new List<DiskPartitionInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new DiskPartitionInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            Description = obj["Description"]?.ToString() ?? "",
                            Size = Convert.ToUInt64(obj["Size"] ?? 0),
                            Type = obj["Type"]?.ToString() ?? "",
                            BootPartition = obj["BootPartition"] != null && Convert.ToBoolean(obj["BootPartition"]) ? HTranslation.GetContent("是") : HTranslation.GetContent("否")
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取风扇信息。</summary>
        public static List<FanInfo> GetFanSpeeds()
        {
            var list = new List<FanInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Fan"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new FanInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            DesiredSpeed = obj["DesiredSpeed"] != null ? Convert.ToUInt32(obj["DesiredSpeed"]) : 0,
                            VariableSpeed = obj["VariableSpeed"] != null && Convert.ToBoolean(obj["VariableSpeed"]),
                            Status = obj["Status"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取电压信息。</summary>
        public static List<VoltageInfo> GetVoltages()
        {
            var list = new List<VoltageInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Voltage"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new VoltageInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            CurrentVoltage = obj["CurrentVoltage"] != null ? Convert.ToUInt32(obj["CurrentVoltage"]) : 0,
                            MinVoltage = obj["MinVoltage"] != null ? Convert.ToUInt32(obj["MinVoltage"]) : 0,
                            MaxVoltage = obj["MaxVoltage"] != null ? Convert.ToUInt32(obj["MaxVoltage"]) : 0,
                            Status = obj["Status"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取 USB 设备列表。</summary>
        public static List<USBDeviceInfo> GetUSBDevices()
        {
            var list = new List<USBDeviceInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBHub"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new USBDeviceInfo
                        {
                            Description = obj["Description"]?.ToString() ?? "",
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            Service = obj["Service"]?.ToString() ?? "",
                            PNPDeviceID = obj["PNPDeviceID"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取打印机列表。</summary>
        public static List<PrinterInfo> GetPrinters()
        {
            var list = new List<PrinterInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new PrinterInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            DriverName = obj["DriverName"]?.ToString() ?? "",
                            PortName = obj["PortName"]?.ToString() ?? "",
                            Status = obj["Status"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取详细网卡信息（含 IP 配置）。</summary>
        public static List<NetworkAdapterInfoEx> GetNetworkAdaptersEx()
        {
            var list = new List<NetworkAdapterInfoEx>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=True"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        var item = new NetworkAdapterInfoEx
                        {
                            MACAddress = obj["MACAddress"]?.ToString() ?? "",
                            IPAddress = obj["IPAddress"] != null ? string.Join(",", (string[])obj["IPAddress"]) : "",
                            IPSubnet = obj["IPSubnet"] != null ? string.Join(",", (string[])obj["IPSubnet"]) : "",
                            DefaultGateway = obj["DefaultIPGateway"] != null ? string.Join(",", (string[])obj["DefaultIPGateway"]) : "",
                            DHCPServer = obj["DHCPServer"]?.ToString() ?? "",
                            DHCPEnabled = obj["DHCPEnabled"] != null && Convert.ToBoolean(obj["DHCPEnabled"]),
                            DNSHostName = obj["DNSHostName"]?.ToString() ?? ""
                        };
                        using (var aSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE MACAddress='" + item.MACAddress + "'"))
                        {
                            foreach (ManagementObject a in Enumerate(aSearcher))
                            {
                                item.Name = a["Name"]?.ToString() ?? "";
                                item.AdapterType = a["AdapterType"]?.ToString() ?? "";
                                item.Speed = a["Speed"] != null ? Convert.ToUInt64(a["Speed"]) : 0;
                                item.NetConnectionStatus = a["NetConnectionStatus"] != null ? ConvertNetStatus(Convert.ToUInt16(a["NetConnectionStatus"])) : HTranslation.GetContent("未知");
                                break;
                            }
                        }
                        list.Add(item);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取电池扩展信息。</summary>
        public static List<BatteryInfoEx> GetBatteryInfoEx()
        {
            var list = new List<BatteryInfoEx>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new BatteryInfoEx
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            EstimatedChargeRemaining = Convert.ToUInt16(obj["EstimatedChargeRemaining"] ?? 0),
                            // 电池充放电状态应取 BatteryStatus 数值码；Status 字符串只有 OK/Error 之类
                            Status = MapBatteryStatus(obj["BatteryStatus"] != null ? Convert.ToUInt16(obj["BatteryStatus"]) : (ushort)0),
                            EstimatedRunTime = Convert.ToUInt32(obj["EstimatedRunTime"] ?? 0),
                            DesignCapacity = obj["DesignCapacity"] != null ? Convert.ToUInt32(obj["DesignCapacity"]) : 0,
                            FullChargeCapacity = obj["FullChargeCapacity"] != null ? Convert.ToUInt32(obj["FullChargeCapacity"]) : 0,
                            // Chemistry 是 uint16 枚举（1=其他…6=锂离子…8=锂聚合物），直接 ToString 只会得到数字
                            Chemistry = MapChemistry(obj["Chemistry"] != null ? Convert.ToUInt16(obj["Chemistry"]) : (ushort)0)
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取系统插槽信息。</summary>
        public static List<SystemSlotInfo> GetSystemSlots()
        {
            var list = new List<SystemSlotInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemSlot"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        list.Add(new SystemSlotInfo
                        {
                            SlotDesignation = obj["SlotDesignation"]?.ToString() ?? "",
                            SlotType = obj["SlotType"]?.ToString() ?? "",
                            Status = obj["Status"]?.ToString() ?? "",
                            CurrentUsage = obj["CurrentUsage"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取指定磁盘的 SMART 属性（简略）。</summary>
        public static List<SMARTAttributeInfo> GetSMARTDetails(string driveModel)
        {
            var list = new List<SMARTAttributeInfo>();
            // 实际解析复杂，返回占位信息
            list.Add(new SMARTAttributeInfo
            {
                AttributeName = HTranslation.GetContent("未实现详细SMART解析"),
                Value = 0,
                Worst = 0,
                Threshold = 0,
                Status = HTranslation.GetContent("未知")
            });
            return list;
        }

        #endregion
    }
}
