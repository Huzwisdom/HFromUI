using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.IO;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 磁盘与存储管理工具类（集成多语言翻译）。
    /// 提供磁盘分区信息、格式化、磁盘清理、配额管理、碎片整理等功能。
    /// 底层基于 WMI (Win32_LogicalDisk, Win32_DiskPartition 等) 及命令行工具。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HDiskManager
    {

        #region 信息类定义

        /// <summary>逻辑磁盘分区信息。</summary>
        public class LogicalDiskInfo
        {
            /// <summary>盘符（如 "C:"）</summary>
            public string DeviceID { get; set; }
            /// <summary>文件系统（如 "NTFS"）</summary>
            public string FileSystem { get; set; }
            /// <summary>卷标</summary>
            public string VolumeName { get; set; }
            /// <summary>卷序列号</summary>
            public string VolumeSerialNumber { get; set; }
            /// <summary>总大小（字节）</summary>
            public ulong Size { get; set; }
            /// <summary>可用空间（字节）</summary>
            public ulong FreeSpace { get; set; }
            /// <summary>驱动器类型描述（已翻译）</summary>
            public string DriveType { get; set; }
        }

        /// <summary>物理磁盘分区信息。</summary>
        public class DiskPartitionInfo
        {
            /// <summary>分区名称（如 "Disk #0, Partition #0"）</summary>
            public string Name { get; set; }
            /// <summary>描述</summary>
            public string Description { get; set; }
            /// <summary>大小（字节）</summary>
            public ulong Size { get; set; }
            /// <summary>是否启动分区</summary>
            public bool BootPartition { get; set; }
            /// <summary>分区类型</summary>
            public string Type { get; set; }
        }

        /// <summary>磁盘配额信息。</summary>
        public class DiskQuotaInfo
        {
            /// <summary>用户名</summary>
            public string User { get; set; }
            /// <summary>已用空间（字节）</summary>
            public ulong UsedSpace { get; set; }
            /// <summary>配额限制（字节）</summary>
            public ulong Limit { get; set; }
            /// <summary>警告级别（字节）</summary>
            public ulong WarningLimit { get; set; }
        }

        #endregion

        #region 磁盘分区信息

        /// <summary>获取所有逻辑驱动器（分区）信息。</summary>
        /// <returns>逻辑磁盘信息列表。</returns>
        public static List<LogicalDiskInfo> GetLogicalDisks()
        {
            var list = new List<LogicalDiskInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType = 3"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var disk = new LogicalDiskInfo
                        {
                            DeviceID = obj["DeviceID"]?.ToString() ?? "",
                            FileSystem = obj["FileSystem"]?.ToString() ?? "",
                            VolumeName = obj["VolumeName"]?.ToString() ?? "",
                            VolumeSerialNumber = obj["VolumeSerialNumber"]?.ToString() ?? "",
                            Size = Convert.ToUInt64(obj["Size"] ?? 0),
                            FreeSpace = Convert.ToUInt64(obj["FreeSpace"] ?? 0)
                        };

                        uint driveType = Convert.ToUInt32(obj["DriveType"] ?? 3);
                        disk.DriveType = TranslateDriveType(driveType);
                        list.Add(disk);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取物理磁盘分区信息。</summary>
        /// <returns>分区信息列表。</returns>
        public static List<DiskPartitionInfo> GetDiskPartitions()
        {
            var list = new List<DiskPartitionInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var part = new DiskPartitionInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            Description = obj["Description"]?.ToString() ?? "",
                            Size = Convert.ToUInt64(obj["Size"] ?? 0),
                            BootPartition = obj["BootPartition"] != null && Convert.ToBoolean(obj["BootPartition"]),
                            Type = obj["Type"]?.ToString() ?? ""
                        };
                        list.Add(part);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>TranslateDriveType 方法。</summary>
        private static string TranslateDriveType(uint type)
        {
            switch (type)
            {
                case 0: return HTranslation.GetContent("未知");
                case 1: return HTranslation.GetContent("无根目录");
                case 2: return HTranslation.GetContent("可移动磁盘");
                case 3: return HTranslation.GetContent("本地磁盘");
                case 4: return HTranslation.GetContent("网络磁盘");
                case 5: return HTranslation.GetContent("光盘");
                case 6: return HTranslation.GetContent("RAM 磁盘");
                default: return HTranslation.GetContent("未知");
            }
        }

        #endregion

        #region 磁盘格式化（危险操作）

        /// <summary>格式化指定驱动器（使用 format.com，需要管理员权限）。</summary>
        /// <param name="driveLetter">盘符，如 "D:"</param>
        /// <param name="fileSystem">文件系统，如 "NTFS"，空则保持默认。</param>
        /// <param name="quickFormat">是否快速格式化。</param>
        /// <returns>是否成功启动格式化进程。</returns>
        public static bool FormatDrive(string driveLetter, string fileSystem = "", bool quickFormat = true)
        {
            try
            {
                string args = driveLetter + " ";
                if (quickFormat) args += "/Q ";
                if (!string.IsNullOrEmpty(fileSystem)) args += "/FS:" + fileSystem + " ";
                args += "/Y"; // 自动确认
                var psi = new ProcessStartInfo("format.com", args)
                {
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };
                Process.Start(psi);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 磁盘清理

        /// <summary>启动磁盘清理工具（针对指定驱动器）。</summary>
        /// <param name="driveLetter">盘符，如 "C:"</param>
        /// <returns>是否成功启动。</returns>
        public static bool StartDiskCleanup(string driveLetter)
        {
            try
            {
                // cleanmgr /d 驱动器
                Process.Start("cleanmgr.exe", "/d " + driveLetter);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 磁盘配额管理

        /// <summary>获取指定驱动器的磁盘配额信息（需要管理员权限）。</summary>
        /// <param name="driveLetter">盘符，如 "C:"</param>
        /// <returns>配额条目列表。</returns>
        public static List<DiskQuotaInfo> GetDiskQuotas(string driveLetter)
        {
            var list = new List<DiskQuotaInfo>();
            try
            {
                // Win32_DiskQuota 类通常需要管理员权限
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskQuota WHERE QuotaVolume = 'Win32_LogicalDisk.DeviceID=\"" + driveLetter.Replace(":", "") + ":\"'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        list.Add(new DiskQuotaInfo
                        {
                            User = obj["User"]?.ToString() ?? "",
                            UsedSpace = Convert.ToUInt64(obj["DiskUsed"] ?? 0), // 已用空间（Win32_DiskQuota.DiskUsed，单位字节）；此前复制粘贴误读为 Limit
                            Limit = Convert.ToUInt64(obj["Limit"] ?? 0),
                            WarningLimit = Convert.ToUInt64(obj["WarningLimit"] ?? 0)
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>启用指定驱动器的 NTFS 配额管理。</summary>
        /// <param name="driveLetter">盘符，如 "C:"</param>
        /// <returns>是否成功。</returns>
        public static bool EnableDiskQuota(string driveLetter)
        {
            try
            {
                return RunCmdCommand("fsutil quota enforce " + driveLetter + " 1");
            }
            catch { return false; }
        }

        /// <summary>禁用指定驱动器的 NTFS 配额管理。</summary>
        /// <param name="driveLetter">盘符。</param>
        /// <returns>是否成功。</returns>
        public static bool DisableDiskQuota(string driveLetter)
        {
            try
            {
                return RunCmdCommand("fsutil quota enforce " + driveLetter + " 0");
            }
            catch { return false; }
        }

        /// <summary>RunCmdCommand 方法。</summary>
        private static bool RunCmdCommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c " + arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return false;
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 磁盘碎片整理

        /// <summary>对指定驱动器进行碎片整理（使用 defrag.exe）。</summary>
        /// <param name="driveLetter">盘符，如 "C:"</param>
        /// <returns>是否成功启动。</returns>
        public static bool DefragmentDrive(string driveLetter)
        {
            try
            {
                Process.Start("defrag.exe", driveLetter + " /U /V");
                return true;
            }
            catch { return false; }
        }

        /// <summary>分析指定驱动器的碎片情况。</summary>
        /// <param name="driveLetter">盘符。</param>
        /// <returns>是否成功启动分析。</returns>
        public static bool AnalyzeDrive(string driveLetter)
        {
            try
            {
                Process.Start("defrag.exe", driveLetter + " /A");
                return true;
            }
            catch { return false; }
        }

        /// <summary>通过 WMI 对指定卷进行碎片整理（Win32_Volume.Defrag）。</summary>
        /// <param name="driveLetter">盘符，如 "C:"</param>
        /// <returns>是否成功。</returns>
        public static bool DefragmentVolumeWmi(string driveLetter)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Volume WHERE DriveLetter = '" + driveLetter + "'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        obj.InvokeMethod("Defrag", null);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion
    }
}
