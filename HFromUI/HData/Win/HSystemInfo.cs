using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 操作系统信息查询与设置工具类（集成多语言翻译）。
    /// 提供操作系统详细信息、环境变量、.NET版本、激活状态、区域/语言/键盘/货币/日期格式的读取与设置。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HSystemInfo
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

        /// <summary>操作系统详细信息。</summary>
        public class OperatingSystemDetail
        {
            /// <summary>操作系统名称</summary>
            public string Caption { get; set; }
            /// <summary>版本号</summary>
            public string Version { get; set; }
            /// <summary>安装日期（UTC）</summary>
            public DateTime InstallDate { get; set; }
            /// <summary>启动设备</summary>
            public string BootDevice { get; set; }
            /// <summary>产品 ID</summary>
            public string SerialNumber { get; set; }
            /// <summary>系统类型（已翻译）</summary>
            public string SystemType { get; set; }
            /// <summary>操作系统架构</summary>
            public string OSArchitecture { get; set; }
            /// <summary>当前时区显示名</summary>
            public string TimeZone { get; set; }
        }

        /// <summary>.NET Framework 版本信息。</summary>
        public class DotNetVersionInfo
        {
            /// <summary>版本描述</summary>
            public string Version { get; set; }
            /// <summary>Release 值（仅 v4 以上有效）</summary>
            public int Release { get; set; }
        }

        /// <summary>Windows 激活状态。</summary>
        public class WindowsActivationInfo
        {
            /// <summary>产品名称</summary>
            public string Name { get; set; }
            /// <summary>许可证状态码</summary>
            public uint LicenseStatus { get; set; }
            /// <summary>许可证状态描述（已翻译）</summary>
            public string LicenseStatusDescription { get; set; }
            /// <summary>剩余重新授权计数</summary>
            public uint RemainingReArmCount { get; set; }
        }

        /// <summary>区域与语言信息。</summary>
        public class RegionLanguageInfo
        {
            /// <summary>当前区域</summary>
            public string CurrentCulture { get; set; }
            /// <summary>当前 UI 语言</summary>
            public string CurrentUICulture { get; set; }
            /// <summary>键盘布局列表</summary>
            public List<string> KeyboardLayouts { get; set; }
            /// <summary>货币符号</summary>
            public string CurrencySymbol { get; set; }
            /// <summary>小数点符号</summary>
            public string NumberDecimalSeparator { get; set; }
            /// <summary>短日期格式</summary>
            public string ShortDatePattern { get; set; }
            /// <summary>长日期格式</summary>
            public string LongDatePattern { get; set; }
        }

        #endregion

        #region 查询方法

        /// <summary>获取操作系统详细信息。</summary>
        public static OperatingSystemDetail GetOperatingSystemDetail()
        {
            var info = new OperatingSystemDetail();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        info.Caption = obj["Caption"]?.ToString() ?? "";
                        info.Version = obj["Version"]?.ToString() ?? "";
                        info.BootDevice = obj["BootDevice"]?.ToString() ?? "";
                        info.SerialNumber = obj["SerialNumber"]?.ToString() ?? "";
                        info.OSArchitecture = obj["OSArchitecture"]?.ToString() ?? "";

                        if (obj["InstallDate"] != null)
                        {
                            try { info.InstallDate = ManagementDateTimeConverter.ToDateTime(obj["InstallDate"].ToString()); }
                            catch { info.InstallDate = DateTime.MinValue; }
                        }

                        if (obj["ProductType"] != null)
                        {
                            uint pt = Convert.ToUInt32(obj["ProductType"]);
                            switch (pt)
                            {
                                case 1: info.SystemType = HTranslation.GetContent("工作站"); break;
                                case 2: info.SystemType = HTranslation.GetContent("域控制器"); break;
                                case 3: info.SystemType = HTranslation.GetContent("服务器"); break;
                                default: info.SystemType = HTranslation.GetContent("未知"); break;
                            }
                        }
                        else info.SystemType = HTranslation.GetContent("未知");

                        break;
                    }
                }
                info.TimeZone = TimeZoneInfo.Local.DisplayName;
            }
            catch { }
            return info;
        }

        /// <summary>获取机器级环境变量。</summary>
        public static Dictionary<string, string> GetMachineEnvironmentVariables()
        {
            var dict = new Dictionary<string, string>();
            try
            {
                foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine))
                    dict[entry.Key.ToString()] = entry.Value?.ToString() ?? "";
            }
            catch { }
            return dict;
        }

        /// <summary>获取用户级环境变量。</summary>
        public static Dictionary<string, string> GetUserEnvironmentVariables()
        {
            var dict = new Dictionary<string, string>();
            try
            {
                foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
                    dict[entry.Key.ToString()] = entry.Value?.ToString() ?? "";
            }
            catch { }
            return dict;
        }

        /// <summary>获取已安装的 .NET Framework 版本列表。</summary>
        public static List<DotNetVersionInfo> GetInstalledDotNetVersions()
        {
            var list = new List<DotNetVersionInfo>();
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
                {
                    if (key != null)
                    {
                        object releaseObj = key.GetValue("Release");
                        if (releaseObj != null)
                        {
                            int rel = Convert.ToInt32(releaseObj);
                            list.Add(new DotNetVersionInfo { Version = MapReleaseToVersion(rel), Release = rel });
                        }
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP"))
                {
                    if (key != null)
                    {
                        foreach (string subName in key.GetSubKeyNames())
                        {
                            if (subName.StartsWith("v"))
                            {
                                using (var subKey = key.OpenSubKey(subName))
                                {
                                    if (subKey?.GetValue("Install")?.ToString() == "1")
                                    {
                                        string ver = subKey.GetValue("Version")?.ToString() ?? subName;
                                        if (!subName.StartsWith("v4"))
                                            list.Add(new DotNetVersionInfo { Version = ver, Release = 0 });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>MapReleaseToVersion 方法。</summary>
        private static string MapReleaseToVersion(int release)
        {
            if (release >= 528040) return "4.8";
            if (release >= 461808) return "4.7.2";
            if (release >= 461308) return "4.7.1";
            if (release >= 460798) return "4.7";
            if (release >= 394802) return "4.6.2";
            if (release >= 394254) return "4.6.1";
            if (release >= 393295) return "4.6";
            if (release >= 379893) return "4.5.2";
            if (release >= 378675) return "4.5.1";
            if (release >= 378389) return "4.5";
            return HTranslation.GetContent("未知版本") + " (" + release + ")";
        }

        /// <summary>获取 Windows 激活状态。</summary>
        public static WindowsActivationInfo GetWindowsActivationInfo()
        {
            var info = new WindowsActivationInfo();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM SoftwareLicensingProduct WHERE Name LIKE '%Windows%' AND PartialProductKey IS NOT NULL"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        info.Name = obj["Name"]?.ToString() ?? "";
                        if (obj["LicenseStatus"] != null)
                        {
                            info.LicenseStatus = Convert.ToUInt32(obj["LicenseStatus"]);
                            info.LicenseStatusDescription = MapLicenseStatus(info.LicenseStatus);
                        }
                        if (obj["RemainingReArmCount"] != null)
                            info.RemainingReArmCount = Convert.ToUInt32(obj["RemainingReArmCount"]);
                        break;
                    }
                }
            }
            catch { }
            return info;
        }

        /// <summary>MapLicenseStatus 方法。</summary>
        private static string MapLicenseStatus(uint status)
        {
            // SoftwareLicensingProduct.LicenseStatus 官方码表，原 3/4/6 标签错配
            switch (status)
            {
                case 0: return HTranslation.GetContent("未授权");
                case 1: return HTranslation.GetContent("已激活");
                case 2: return HTranslation.GetContent("初始宽限期(OOB)");
                case 3: return HTranslation.GetContent("OOT 宽限期(超出容忍)");
                case 4: return HTranslation.GetContent("非正版宽限期");
                case 5: return HTranslation.GetContent("通知模式");
                case 6: return HTranslation.GetContent("扩展宽限期");
                default: return HTranslation.GetContent("未知") + " (" + status + ")";
            }
        }

        /// <summary>获取当前区域、语言、键盘、货币、日期格式等信息。</summary>
        public static RegionLanguageInfo GetRegionLanguageInfo()
        {
            var info = new RegionLanguageInfo();
            try
            {
                CultureInfo cur = CultureInfo.CurrentCulture;
                CultureInfo ui = CultureInfo.CurrentUICulture;
                info.CurrentCulture = cur.Name + " - " + cur.DisplayName;
                info.CurrentUICulture = ui.Name + " - " + ui.DisplayName;
                info.CurrencySymbol = cur.NumberFormat.CurrencySymbol;
                info.NumberDecimalSeparator = cur.NumberFormat.NumberDecimalSeparator;
                info.ShortDatePattern = cur.DateTimeFormat.ShortDatePattern;
                info.LongDatePattern = cur.DateTimeFormat.LongDatePattern;

                var layouts = new List<string>();
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Keyboard"))
                {
                    foreach (ManagementObject obj in Enumerate(searcher))
                    {
                        string name = obj["Name"]?.ToString() ?? "";
                        string layout = obj["Layout"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(name))
                            layouts.Add(name + (string.IsNullOrEmpty(layout) ? "" : " (" + layout + ")"));
                    }
                }
                info.KeyboardLayouts = layouts;
            }
            catch { }
            return info;
        }

        #endregion

        #region 设置方法

        /// <summary>设置计算机名称（需要管理员权限，重启后生效）。</summary>
        /// <param name="name">新计算机名。</param>
        /// <returns>是否成功。</returns>
        public static bool SetComputerName(string name)
        {
            try
            {
                return SetComputerNameEx(ComputerNameFormat.ComputerNamePhysicalDnsHostname, name);
            }
            catch { return false; }
        }

        /// <summary>设置系统时间（UTC）。</summary>
        /// <param name="time">要设置的时间。</param>
        /// <returns>是否成功。</returns>
        public static bool SetSystemTime(DateTime time)
        {
            try
            {
                SYSTEMTIME st = new SYSTEMTIME
                {
                    wYear = (ushort)time.Year,
                    wMonth = (ushort)time.Month,
                    wDay = (ushort)time.Day,
                    wHour = (ushort)time.Hour,
                    wMinute = (ushort)time.Minute,
                    wSecond = (ushort)time.Second,
                    wMilliseconds = (ushort)time.Millisecond
                };
                return SetSystemTime(ref st);
            }
            catch { return false; }
        }

        /// <summary>设置系统时区（需要管理员权限）。</summary>
        /// <param name="timeZoneId">时区 ID，如 "China Standard Time"。</param>
        /// <returns>是否成功。</returns>
        public static bool SetTimeZone(string timeZoneId)
        {
            try
            {
                // 释放 Process 对象避免句柄泄漏，并等待 tzutil 执行完成再返回
                using (var p = System.Diagnostics.Process.Start("tzutil.exe", "/s \"" + timeZoneId + "\""))
                {
                    if (p != null) p.WaitForExit();
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>设置环境变量。</summary>
        /// <param name="name">变量名。</param>
        /// <param name="value">变量值。</param>
        /// <param name="target">目标级别（Machine/User）。</param>
        public static void SetEnvironmentVariable(string name, string value, EnvironmentVariableTarget target)
        {
            try { Environment.SetEnvironmentVariable(name, value, target); }
            catch { }
        }

        /// <summary>删除环境变量。</summary>
        /// <param name="name">变量名。</param>
        /// <param name="target">目标级别。</param>
        public static void DeleteEnvironmentVariable(string name, EnvironmentVariableTarget target)
        {
            try { Environment.SetEnvironmentVariable(name, null, target); }
            catch { }
        }

        /// <summary>设置当前用户的区域和语言（修改注册表，立即生效）。</summary>
        /// <param name="locale">区域名称，如 "zh-CN"。</param>
        /// <param name="language">语言名称，如 "zh-CN"（保留参数兼容旧签名）。</param>
        /// <returns>是否成功。</returns>
        public static bool SetRegionAndLanguage(string locale, string language)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true))
                {
                    if (key == null) return false;
                    // 修正：Vista 以后区域名称应写入 LocaleName（如 zh-CN）；
                    // 原代码写入的 Locale 需要的是十六进制 LCID（如 00000804），sLanguage 需要三字母缩写（如 CHS），直接写 zh-CN 是无效值
                    key.SetValue("LocaleName", locale);
                }
                // 广播更改通知
                UIntPtr result;
                SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, UIntPtr.Zero, "Intl", SMTO_ABORTIFHUNG, 5000, out result);
                return true;
            }
            catch { return false; }
        }

        /// <summary>加载指定的键盘布局。</summary>
        /// <param name="dllPath">键盘布局标识 KLID（注意：Win32 API LoadKeyboardLayout 接收的是 KLID 字符串，如美式英语 "00000409"，并非 DLL 文件名；参数名保留以兼容旧签名）。</param>
        /// <returns>成功返回布局句柄，失败返回 IntPtr.Zero。</returns>
        public static IntPtr LoadKeyboardLayout(string dllPath)
        {
            try
            {
                return LoadKeyboardLayoutAPI(dllPath, KLF_ACTIVATE);
            }
            catch { return IntPtr.Zero; }
        }

        /// <summary>卸载指定的键盘布局。</summary>
        /// <param name="hkl">布局句柄。</param>
        /// <returns>是否成功。</returns>
        public static bool UnloadKeyboardLayout(IntPtr hkl)
        {
            try
            {
                return UnloadKeyboardLayoutAPI(hkl);
            }
            catch { return false; }
        }

        /// <summary>设置货币符号（当前用户，立即生效）。</summary>
        /// <param name="symbol">新货币符号。</param>
        /// <returns>是否成功。</returns>
        public static bool SetCurrencySymbol(string symbol)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true))
                {
                    if (key == null) return false;
                    key.SetValue("sCurrency", symbol);
                }
                UIntPtr result;
                SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, UIntPtr.Zero, "Intl", SMTO_ABORTIFHUNG, 5000, out result);
                return true;
            }
            catch { return false; }
        }

        /// <summary>设置日期格式（短日期和长日期）。</summary>
        /// <param name="shortPattern">短日期格式，如 "yyyy-MM-dd"。</param>
        /// <param name="longPattern">长日期格式，如 "yyyy'年'M'月'd'日'"。</param>
        /// <returns>是否成功。</returns>
        public static bool SetDatePattern(string shortPattern, string longPattern)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true))
                {
                    if (key == null) return false;
                    key.SetValue("sShortDate", shortPattern);
                    key.SetValue("sLongDate", longPattern);
                }
                UIntPtr result;
                SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_SETTINGCHANGE, UIntPtr.Zero, "Intl", SMTO_ABORTIFHUNG, 5000, out result);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Win32 API 与常量

        private const int HWND_BROADCAST = 0xffff;
        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint KLF_ACTIVATE = 0x00000001;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        private enum ComputerNameFormat { ComputerNamePhysicalDnsHostname = 5 }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetComputerNameEx(ComputerNameFormat NameType, string lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort wYear, wMonth, wDayOfWeek, wDay, wHour, wMinute, wSecond, wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam, uint fuFlags, uint uTimeout, out UIntPtr lpdwResult);

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "LoadKeyboardLayout")]
        private static extern IntPtr LoadKeyboardLayoutAPI(string pwszKLID, uint Flags);

        [DllImport("user32.dll", EntryPoint = "UnloadKeyboardLayout")]
        private static extern bool UnloadKeyboardLayoutAPI(IntPtr hkl);

        #endregion
    }
}
