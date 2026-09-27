using System;
using System.Collections.Generic;
using System.Management;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 设备管理器与即插即用管理工具类（集成多语言翻译）。
    /// 提供设备列表、启用/禁用设备、硬件 ID 解析、驱动程序信息获取等功能。
    /// 底层基于 WMI (Win32_PnPEntity, Win32_PnPSignedDriver) 实现。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HDeviceManager
    {

        #region 信息类定义

        /// <summary>设备信息（来自 Win32_PnPEntity）。</summary>
        public class DeviceInfo
        {
            /// <summary>设备名称</summary>
            public string Name { get; set; }
            /// <summary>设备描述</summary>
            public string Description { get; set; }
            /// <summary>即插即用设备 ID (PNPDeviceID)</summary>
            public string PNPDeviceID { get; set; }
            /// <summary>设备状态（如 "OK", "Error" 等，已翻译）</summary>
            public string Status { get; set; }
            /// <summary>硬件 ID 列表（可能多个）</summary>
            public string[] HardwareIDs { get; set; }
            /// <summary>兼容 ID 列表</summary>
            public string[] CompatibleIDs { get; set; }
            /// <summary>设备类 GUID（如 "{4d36e972-e325-11ce-bfc1-08002be10318}" 代表网络适配器）</summary>
            public string ClassGuid { get; set; }
            /// <summary>设备类名称（如 "网络适配器"）</summary>
            public string ClassName { get; set; }
            /// <summary>制造商</summary>
            public string Manufacturer { get; set; }
            /// <summary>服务名称（驱动程序服务名）</summary>
            public string Service { get; set; }
        }

        /// <summary>驱动程序信息（来自 Win32_PnPSignedDriver）。</summary>
        public class DriverInfo
        {
            /// <summary>设备名称</summary>
            public string DeviceName { get; set; }
            /// <summary>驱动版本</summary>
            public string DriverVersion { get; set; }
            /// <summary>驱动日期</summary>
            public string DriverDate { get; set; }
            /// <summary>驱动提供商</summary>
            public string Manufacturer { get; set; }
            /// <summary>设备 ID (PNPDeviceID)</summary>
            public string PNPDeviceID { get; set; }
            /// <summary>驱动文件路径</summary>
            public string DriverPath { get; set; }
            /// <summary>是否已签名</summary>
            public bool IsSigned { get; set; }
        }

        #endregion

        #region 设备列表

        /// <summary>获取所有即插即用设备列表（包含状态、硬件ID等）。</summary>
        /// <returns>设备信息列表。</returns>
        public static List<DeviceInfo> GetAllDevices()
        {
            var list = new List<DeviceInfo>();
            try
            {
                // 一次性查询所有设备类，构建 ClassGuid -> 类名 映射；
                // 原实现在设备循环内逐个查询 Win32_PnPClass（N+1 查询，数百台设备即数百次 WMI 调用，极慢）
                var classGuidToName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    using (var classSearcher = new ManagementObjectSearcher("SELECT ClassGuid, Name FROM Win32_PnPClass"))
                    {
                        foreach (ManagementObject cls in classSearcher.Get())
                        {
                            string guid = cls["ClassGuid"]?.ToString();
                            string clsName = cls["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(guid) && !classGuidToName.ContainsKey(guid))
                            {
                                classGuidToName[guid] = clsName ?? "";
                            }
                        }
                    }
                }
                catch { }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var device = new DeviceInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            Description = obj["Description"]?.ToString() ?? "",
                            PNPDeviceID = obj["PNPDeviceID"]?.ToString() ?? "",
                            Status = obj["Status"] != null ? HTranslation.GetContent(obj["Status"].ToString()) : HTranslation.GetContent("未知"),
                            ClassGuid = obj["ClassGuid"]?.ToString() ?? "",
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            Service = obj["Service"]?.ToString() ?? ""
                        };

                        // 获取硬件 ID
                        if (obj["HardwareID"] != null)
                            device.HardwareIDs = (string[])obj["HardwareID"];
                        else
                            device.HardwareIDs = new string[0];

                        // 获取兼容 ID
                        if (obj["CompatibleID"] != null)
                            device.CompatibleIDs = (string[])obj["CompatibleID"];
                        else
                            device.CompatibleIDs = new string[0];

                        // 设备类名：优先直接读 PNPClass 属性，没有则查一次性映射表
                        device.ClassName = obj["PNPClass"]?.ToString() ?? "";
                        if (string.IsNullOrEmpty(device.ClassName) &&
                            !string.IsNullOrEmpty(device.ClassGuid) &&
                            classGuidToName.ContainsKey(device.ClassGuid))
                        {
                            device.ClassName = classGuidToName[device.ClassGuid];
                        }
                        list.Add(device);
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 启用/禁用设备

        /// <summary>启用指定设备。</summary>
        /// <param name="pnpDeviceId">设备的 PNPDeviceID。</param>
        /// <returns>是否成功。</returns>
        public static bool EnableDevice(string pnpDeviceId)
        {
            return InvokeDeviceMethod(pnpDeviceId, "Enable");
        }

        /// <summary>禁用指定设备。</summary>
        /// <param name="pnpDeviceId">设备的 PNPDeviceID。</param>
        /// <returns>是否成功。</returns>
        public static bool DisableDevice(string pnpDeviceId)
        {
            return InvokeDeviceMethod(pnpDeviceId, "Disable");
        }

        /// <summary>调用 Win32_PnPEntity 的 Enable/Disable 方法。</summary>
        private static bool InvokeDeviceMethod(string pnpDeviceId, string methodName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID='" + pnpDeviceId + "'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // Win32_PnPEntity.Enable/Disable 返回 ReturnValue：0=成功，非 0 为错误码
                        // 原实现忽略返回值，设备实际禁用/启用失败（如权限不足）时仍报告 true
                        ManagementBaseObject outParams = (ManagementBaseObject)obj.InvokeMethod(methodName, (object[])null);
                        using (outParams)
                        {
                            if (outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion

        #region 硬件 ID 解析

        /// <summary>从 PNPDeviceID 中解析出硬件 ID（第一个）。</summary>
        /// <param name="pnpDeviceId">完整的 PNPDeviceID 字符串。</param>
        /// <returns>硬件 ID，若解析失败返回空字符串。</returns>
        public static string ParseHardwareId(string pnpDeviceId)
        {
            if (string.IsNullOrEmpty(pnpDeviceId)) return "";
            // 格式通常为：PCI\VEN_8086&DEV_1000&... 或 USB\VID_...
            // 返回 "PCI\VEN_8086&DEV_1000" 这样的第一级硬件 ID
            int ampIndex = pnpDeviceId.IndexOf('&');
            if (ampIndex > 0)
                return pnpDeviceId.Substring(0, ampIndex);
            return pnpDeviceId;
        }

        /// <summary>从 PNPDeviceID 中解析出兼容 ID（提取厂商和产品部分）。</summary>
        /// <param name="pnpDeviceId">完整的 PNPDeviceID。</param>
        /// <returns>兼容 ID 列表（如 VEN_8086, DEV_1000）。</returns>
        public static List<string> ParseCompatibleIds(string pnpDeviceId)
        {
            var ids = new List<string>();
            if (string.IsNullOrEmpty(pnpDeviceId)) return ids;

            // 提取 VEN_ 和 DEV_ 等
            var parts = pnpDeviceId.Split('&');
            foreach (string part in parts)
            {
                if (part.StartsWith("VEN_", StringComparison.OrdinalIgnoreCase) ||
                    part.StartsWith("DEV_", StringComparison.OrdinalIgnoreCase))
                {
                    ids.Add(part);
                }
            }
            return ids;
        }

        #endregion

        #region 驱动程序信息

        /// <summary>获取所有已签名驱动程序的详细信息。</summary>
        /// <returns>驱动程序信息列表。</returns>
        public static List<DriverInfo> GetDriverInfo()
        {
            var list = new List<DriverInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var driver = new DriverInfo
                        {
                            DeviceName = obj["DeviceName"]?.ToString() ?? "",
                            DriverVersion = obj["DriverVersion"]?.ToString() ?? "",
                            DriverDate = obj["DriverDate"]?.ToString() ?? "",
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            PNPDeviceID = obj["PNPDeviceID"]?.ToString() ?? "",
                            DriverPath = obj["DriverPath"]?.ToString() ?? "",
                            IsSigned = obj["IsSigned"] != null && Convert.ToBoolean(obj["IsSigned"])
                        };
                        list.Add(driver);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>根据设备 PNPDeviceID 获取其驱动程序信息。</summary>
        /// <param name="pnpDeviceId">PNPDeviceID。</param>
        /// <returns>驱动程序信息，若未找到返回 null。</returns>
        public static DriverInfo GetDriverInfoByDeviceId(string pnpDeviceId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver WHERE PNPDeviceID='" + pnpDeviceId + "'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return new DriverInfo
                        {
                            DeviceName = obj["DeviceName"]?.ToString() ?? "",
                            DriverVersion = obj["DriverVersion"]?.ToString() ?? "",
                            DriverDate = obj["DriverDate"]?.ToString() ?? "",
                            Manufacturer = obj["Manufacturer"]?.ToString() ?? "",
                            PNPDeviceID = obj["PNPDeviceID"]?.ToString() ?? "",
                            DriverPath = obj["DriverPath"]?.ToString() ?? "",
                            IsSigned = obj["IsSigned"] != null && Convert.ToBoolean(obj["IsSigned"])
                        };
                    }
                }
            }
            catch { }
            return null;
        }

        #endregion
    }
}
