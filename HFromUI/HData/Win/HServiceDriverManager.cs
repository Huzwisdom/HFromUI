using System;
using System.Collections.Generic;
using System.Management;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 服务与驱动管理工具类（集成多语言翻译）。
    /// 提供服务列表查询、启动/停止/暂停/继续/重启服务、修改启动类型，
    /// 以及已加载驱动程序的查询。
    /// 所有 WMI 调用均异常安全，描述性文本已翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HServiceDriverManager
    {

        #region 信息类定义

        /// <summary>Windows 服务信息。</summary>
        public class ServiceInfo
        {
            /// <summary>服务名称（内部名称）</summary>
            public string Name { get; set; }
            /// <summary>显示名称</summary>
            public string DisplayName { get; set; }
            /// <summary>当前状态（已翻译）</summary>
            public string State { get; set; }
            /// <summary>启动类型（已翻译，如“自动”、“手动”、“禁用”）</summary>
            public string StartMode { get; set; }
            /// <summary>执行文件路径</summary>
            public string PathName { get; set; }
            /// <summary>服务描述</summary>
            public string Description { get; set; }
        }

        /// <summary>系统驱动程序信息。</summary>
        public class DriverInfo
        {
            /// <summary>驱动名称</summary>
            public string Name { get; set; }
            /// <summary>显示名称</summary>
            public string DisplayName { get; set; }
            /// <summary>当前状态（已翻译）</summary>
            public string State { get; set; }
            /// <summary>启动类型（已翻译）</summary>
            public string StartMode { get; set; }
            /// <summary>驱动文件路径</summary>
            public string PathName { get; set; }
            /// <summary>描述</summary>
            public string Description { get; set; }
            /// <summary>状态码（如“OK”）</summary>
            public string Status { get; set; }
        }

        #endregion

        #region 服务列表

        /// <summary>获取所有服务的详细信息。</summary>
        /// <returns>服务信息列表。</returns>
        public static List<ServiceInfo> GetServices()
        {
            var list = new List<ServiceInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var service = new ServiceInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            DisplayName = obj["DisplayName"]?.ToString() ?? "",
                            PathName = obj["PathName"]?.ToString() ?? "",
                            Description = obj["Description"]?.ToString() ?? ""
                        };

                        // 状态
                        if (obj["State"] != null)
                        {
                            string raw = obj["State"].ToString();
                            service.State = TranslateServiceState(raw);
                        }
                        else service.State = HTranslation.GetContent("未知");

                        // 启动类型
                        if (obj["StartMode"] != null)
                        {
                            string raw = obj["StartMode"].ToString();
                            service.StartMode = TranslateStartMode(raw);
                        }
                        else service.StartMode = HTranslation.GetContent("未知");

                        list.Add(service);
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 服务控制

        /// <summary>启动指定服务。</summary>
        /// <param name="serviceName">服务名称（内部名称）。</param>
        /// <returns>是否成功。</returns>
        public static bool StartService(string serviceName)
        {
            return InvokeServiceMethod(serviceName, "StartService");
        }

        /// <summary>停止指定服务。</summary>
        /// <param name="serviceName">服务名称。</param>
        /// <returns>是否成功。</returns>
        public static bool StopService(string serviceName)
        {
            return InvokeServiceMethod(serviceName, "StopService");
        }

        /// <summary>暂停指定服务。</summary>
        /// <param name="serviceName">服务名称。</param>
        /// <returns>是否成功。</returns>
        public static bool PauseService(string serviceName)
        {
            return InvokeServiceMethod(serviceName, "PauseService");
        }

        /// <summary>继续（恢复）指定服务。</summary>
        /// <param name="serviceName">服务名称。</param>
        /// <returns>是否成功。</returns>
        public static bool ResumeService(string serviceName)
        {
            return InvokeServiceMethod(serviceName, "ResumeService");
        }

        /// <summary>重启指定服务（先停止，再启动）。</summary>
        /// <param name="serviceName">服务名称。</param>
        /// <returns>是否成功。</returns>
        public static bool RestartService(string serviceName)
        {
            // 重启服务：先尝试停止，再启动
            bool stopped = StopService(serviceName);
            // 等待一下再启动（简单处理）
            System.Threading.Thread.Sleep(1000);
            if (stopped)
                return StartService(serviceName);
            // 如果停止失败，可能服务已经停止，直接启动
            return StartService(serviceName);
        }

        /// <summary>修改指定服务的启动类型。</summary>
        /// <param name="serviceName">服务名称。</param>
        /// <param name="startMode">新的启动类型："Auto"（自动）、"Manual"（手动）、"Disabled"（禁用）。</param>
        /// <returns>是否成功。</returns>
        public static bool SetServiceStartMode(string serviceName, string startMode)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service WHERE Name='" + serviceName + "'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // WMI 方法返回 ReturnValue：0=成功，非 0 为错误码（如 2=拒绝访问）
                        // 原实现完全忽略返回值，权限不足/参数非法时仍报告 true
                        using (ManagementBaseObject inParams = obj.GetMethodParameters("ChangeStartMode"))
                        {
                            inParams["StartMode"] = startMode;
                            using (ManagementBaseObject outParams = obj.InvokeMethod("ChangeStartMode", inParams, null))
                            {
                                if (outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                                {
                                    return true;
                                }
                            }
                        }
                        return false;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>调用 WMI 服务对象的方法。</summary>
        /// <param name="serviceName">服务名称。</param>
        /// <param name="methodName">方法名（StartService, StopService 等）。</param>
        /// <returns>是否成功。</returns>
        private static bool InvokeServiceMethod(string serviceName, string methodName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service WHERE Name='" + serviceName + "'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // 直接调用方法，不需要参数；必须检查 ReturnValue：
                        // Win32_Service 方法 0=成功，非 0 为错误（1=不受支持,2=拒绝访问,5=服务已停止等）
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

        #region 驱动程序列表

        /// <summary>获取所有已加载的系统驱动程序信息。</summary>
        /// <returns>驱动程序信息列表。</returns>
        public static List<DriverInfo> GetDrivers()
        {
            var list = new List<DriverInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemDriver"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var driver = new DriverInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            DisplayName = obj["DisplayName"]?.ToString() ?? "",
                            PathName = obj["PathName"]?.ToString() ?? "",
                            Description = obj["Description"]?.ToString() ?? "",
                            Status = obj["Status"]?.ToString() ?? ""
                        };

                        if (obj["State"] != null)
                        {
                            string raw = obj["State"].ToString();
                            driver.State = TranslateDriverState(raw);
                        }
                        else driver.State = HTranslation.GetContent("未知");

                        if (obj["StartMode"] != null)
                        {
                            string raw = obj["StartMode"].ToString();
                            driver.StartMode = TranslateStartMode(raw);
                        }
                        else driver.StartMode = HTranslation.GetContent("未知");

                        list.Add(driver);
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 状态翻译

        /// <summary>翻译服务状态。</summary>
        private static string TranslateServiceState(string state)
        {
            switch (state)
            {
                case "Stopped": return HTranslation.GetContent("已停止");
                case "Start Pending": return HTranslation.GetContent("启动挂起");
                case "Stop Pending": return HTranslation.GetContent("停止挂起");
                case "Running": return HTranslation.GetContent("正在运行");
                case "Continue Pending": return HTranslation.GetContent("继续挂起");
                case "Pause Pending": return HTranslation.GetContent("暂停挂起");
                case "Paused": return HTranslation.GetContent("已暂停");
                default: return HTranslation.GetContent("未知") + " (" + state + ")";
            }
        }

        /// <summary>翻译驱动状态（与 Win32_SystemDriver 的状态值）。</summary>
        private static string TranslateDriverState(string state)
        {
            switch (state)
            {
                case "Stopped": return HTranslation.GetContent("已停止");
                case "Start Pending": return HTranslation.GetContent("启动挂起");
                case "Stop Pending": return HTranslation.GetContent("停止挂起");
                case "Running": return HTranslation.GetContent("正在运行");
                case "Continue Pending": return HTranslation.GetContent("继续挂起");
                case "Pause Pending": return HTranslation.GetContent("暂停挂起");
                case "Paused": return HTranslation.GetContent("已暂停");
                default: return HTranslation.GetContent("未知") + " (" + state + ")";
            }
        }

        /// <summary>翻译启动类型。</summary>
        private static string TranslateStartMode(string mode)
        {
            switch (mode)
            {
                case "Auto": return HTranslation.GetContent("自动");
                case "Manual": return HTranslation.GetContent("手动");
                case "Disabled": return HTranslation.GetContent("禁用");
                case "Boot": return HTranslation.GetContent("引导");
                case "System": return HTranslation.GetContent("系统");
                default: return HTranslation.GetContent("未知") + " (" + mode + ")";
            }
        }

        #endregion
    }
}
