using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 高级系统设置工具类（终极全功能版，集成多语言翻译）。
    /// 提供视觉效果、启动和故障恢复、虚拟内存、环境变量、计算机名称、系统描述、
    /// BCD 启动选项等高级功能的读取与设置。
    /// 底层基于注册表、WMI、Win32 API 及 bcdedit 命令，异常安全。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HAdvancedSystemSettings
    {

        #region 信息类定义

        /// <summary>启动和故障恢复设置。</summary>
        public class StartupRecoverySettings
        {
            /// <summary>系统失败时自动重新启动</summary>
            public bool AutoReboot { get; set; }
            /// <summary>写入调试信息类型：0=无，1=完全内存转储，2=核心内存转储，3=小内存转储，7=自动内存转储</summary>
            public int CrashDumpType { get; set; }
            /// <summary>调试信息文件路径（如 "%SystemRoot%\MEMORY.DMP"）</summary>
            public string DumpFile { get; set; }
            /// <summary>将事件写入系统日志</summary>
            public bool WriteToSystemLog { get; set; }
            /// <summary>覆盖任何现有文件</summary>
            public bool OverwriteExistingDump { get; set; }
        }

        /// <summary>分页文件信息。</summary>
        public class PageFileInfo
        {
            /// <summary>分页文件路径（如 "C:\pagefile.sys"）</summary>
            public string Path { get; set; }
            /// <summary>初始大小（MB）</summary>
            public long InitialSizeMB { get; set; }
            /// <summary>最大大小（MB）</summary>
            public long MaximumSizeMB { get; set; }
        }

        #endregion

        #region 视觉效果（性能选项）

        /// <summary>将系统视觉性能设置为“最佳性能”（关闭所有视觉效果）。</summary>
        /// <returns>是否成功写入注册表。</returns>
        public static bool SetForBestPerformance()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", true))
                {
                    if (key == null) return false;
                    key.SetValue("VisualFXSetting", 2, RegistryValueKind.DWord);
                    key.SetValue("Themes", 0, RegistryValueKind.DWord);
                    key.SetValue("TaskbarAnimations", 0, RegistryValueKind.DWord);
                    key.SetValue("ListviewShadow", 0, RegistryValueKind.DWord);
                    key.SetValue("FontSmoothing", 0, RegistryValueKind.DWord);
                    key.SetValue("DropShadow", 0, RegistryValueKind.DWord);
                    key.SetValue("WindowAnimation", 0, RegistryValueKind.DWord);
                    key.SetValue("MenuAnimation", 0, RegistryValueKind.DWord);
                    key.SetValue("ComboBoxAnimation", 0, RegistryValueKind.DWord);
                    key.SetValue("CursorShadow", 0, RegistryValueKind.DWord);
                    key.SetValue("ListBoxSmoothScrolling", 0, RegistryValueKind.DWord);
                }
                // 修复：SPI_SETUIEFFECTS 的 pvParam 为 BOOL，FALSE 关闭全部 UI 效果；
                // 必须带 SPIF_UPDATEINIFILE 才能持久化到用户配置（原 0x001F 并非视觉效果动作值，调用无效）
                SystemParametersInfo(SPI_SETUIEFFECTS, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                return true;
            }
            catch { return false; }
        }

        /// <summary>将系统视觉性能设置为“最佳外观”（启用所有视觉效果）。</summary>
        /// <returns>是否成功写入注册表。</returns>
        public static bool SetForBestAppearance()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", true))
                {
                    if (key == null) return false;
                    key.SetValue("VisualFXSetting", 1, RegistryValueKind.DWord);
                    key.SetValue("Themes", 1, RegistryValueKind.DWord);
                    key.SetValue("TaskbarAnimations", 1, RegistryValueKind.DWord);
                    key.SetValue("ListviewShadow", 1, RegistryValueKind.DWord);
                    key.SetValue("FontSmoothing", 1, RegistryValueKind.DWord);
                    key.SetValue("DropShadow", 1, RegistryValueKind.DWord);
                    key.SetValue("WindowAnimation", 1, RegistryValueKind.DWord);
                    key.SetValue("MenuAnimation", 1, RegistryValueKind.DWord);
                    key.SetValue("ComboBoxAnimation", 1, RegistryValueKind.DWord);
                    key.SetValue("CursorShadow", 1, RegistryValueKind.DWord);
                    key.SetValue("ListBoxSmoothScrolling", 1, RegistryValueKind.DWord);
                }
                // 修复：pvParam 传 TRUE 启用全部 UI 效果，并带 SPIF_UPDATEINIFILE 持久化
                SystemParametersInfo(SPI_SETUIEFFECTS, 0, (IntPtr)1, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 启动和故障恢复

        /// <summary>获取当前启动和故障恢复设置。</summary>
        /// <returns>设置对象，失败返回默认值。</returns>
        public static StartupRecoverySettings GetStartupRecoverySettings()
        {
            var settings = new StartupRecoverySettings();
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\CrashControl"))
                {
                    if (key != null)
                    {
                        settings.AutoReboot = (int)key.GetValue("AutoReboot", 1) == 1;
                        settings.CrashDumpType = (int)key.GetValue("CrashDumpEnabled", 1);
                        settings.DumpFile = key.GetValue("DumpFile", @"%SystemRoot%\MEMORY.DMP").ToString();
                        settings.WriteToSystemLog = (int)key.GetValue("LogEvent", 1) == 1;
                        settings.OverwriteExistingDump = (int)key.GetValue("Overwrite", 1) == 1;
                    }
                }
            }
            catch { }
            return settings;
        }

        /// <summary>设置启动和故障恢复选项。</summary>
        /// <param name="settings">要应用的设置。</param>
        /// <returns>是否成功写入注册表。</returns>
        public static bool SetStartupRecoverySettings(StartupRecoverySettings settings)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\CrashControl", true))
                {
                    if (key == null) return false;
                    key.SetValue("AutoReboot", settings.AutoReboot ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("CrashDumpEnabled", settings.CrashDumpType, RegistryValueKind.DWord);
                    key.SetValue("DumpFile", settings.DumpFile, RegistryValueKind.String);
                    key.SetValue("LogEvent", settings.WriteToSystemLog ? 1 : 0, RegistryValueKind.DWord);
                    key.SetValue("Overwrite", settings.OverwriteExistingDump ? 1 : 0, RegistryValueKind.DWord);
                    return true;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 虚拟内存

        /// <summary>获取当前所有分页文件的路径和大小。</summary>
        /// <returns>分页文件信息列表。</returns>
        public static List<PageFileInfo> GetPageFiles()
        {
            var list = new List<PageFileInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileSetting"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // 修复：ManagementObject 持有 COM 资源，必须释放，否则反复调用会泄漏
                        using (obj)
                        {
                            list.Add(new PageFileInfo
                            {
                                Path = obj["Caption"]?.ToString() ?? obj["Name"]?.ToString() ?? "",
                                InitialSizeMB = Convert.ToInt64(obj["InitialSize"] ?? 0),
                                MaximumSizeMB = Convert.ToInt64(obj["MaximumSize"] ?? 0)
                            });
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>设置指定驱动器的分页文件大小（需要管理员权限）。</summary>
        /// <param name="drive">驱动器号，如 "C:"。</param>
        /// <param name="initialSizeMB">初始大小（MB），0 表示系统管理。</param>
        /// <param name="maximumSizeMB">最大大小（MB），0 表示系统管理。</param>
        /// <returns>是否成功。</returns>
        public static bool SetPagingFile(string drive, long initialSizeMB, long maximumSizeMB)
        {
            try
            {
                using (var mc = new ManagementClass("Win32_PageFileSetting"))
                {
                    foreach (ManagementObject obj in mc.GetInstances())
                    {
                        // 修复：释放 ManagementObject 持有的 COM 资源（return 路径也会经 using 释放）
                        using (obj)
                        {
                            string path = obj["Name"]?.ToString();
                            if (path != null && path.StartsWith(drive, StringComparison.OrdinalIgnoreCase))
                            {
                                obj["InitialSize"] = initialSizeMB;
                                obj["MaximumSize"] = maximumSizeMB;
                                obj.Put();
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }
            catch { return false; }
        }

        #endregion

        #region 环境变量

        /// <summary>获取指定环境变量的值。</summary>
        /// <param name="name">变量名。</param>
        /// <param name="target">目标级别（Machine/User/Process），默认为 Machine。</param>
        /// <returns>变量值，若不存在则返回 null。</returns>
        public static string GetEnvironmentVariable(string name, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            return Environment.GetEnvironmentVariable(name, target);
        }

        /// <summary>设置环境变量（若已存在则覆盖）。</summary>
        /// <param name="name">变量名。</param>
        /// <param name="value">变量值。</param>
        /// <param name="target">目标级别。</param>
        public static void SetEnvironmentVariable(string name, string value, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            try { Environment.SetEnvironmentVariable(name, value, target); }
            catch { }
        }

        /// <summary>删除指定环境变量。</summary>
        /// <param name="name">变量名。</param>
        /// <param name="target">目标级别。</param>
        public static void DeleteEnvironmentVariable(string name, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
        {
            try { Environment.SetEnvironmentVariable(name, null, target); }
            catch { }
        }

        /// <summary>列出指定目标下的所有环境变量。</summary>
        /// <param name="target">目标（Machine/User/Process）。</param>
        /// <returns>环境变量键值对字典。</returns>
        public static Dictionary<string, string> GetAllEnvironmentVariables(EnvironmentVariableTarget target)
        {
            var dict = new Dictionary<string, string>();
            try
            {
                foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables(target))
                {
                    dict[entry.Key.ToString()] = entry.Value?.ToString() ?? "";
                }
            }
            catch { }
            return dict;
        }

        #endregion

        #region 计算机名称

        /// <summary>获取计算机名称。</summary>
        public static string GetComputerName()
        {
            return Environment.MachineName;
        }

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

        #endregion

        #region 系统描述（Windows 描述）

        /// <summary>获取系统描述（控制面板\系统中的计算机描述）。</summary>
        /// <returns>系统描述字符串，失败返回 null。</returns>
        public static string GetSystemDescription()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\lanmanserver\Parameters"))
                {
                    return key?.GetValue("srvcomment")?.ToString();
                }
            }
            catch { return null; }
        }

        /// <summary>设置系统描述。</summary>
        /// <param name="description">描述文字，若为空则删除描述。</param>
        /// <returns>是否成功。</returns>
        public static bool SetSystemDescription(string description)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\lanmanserver\Parameters", true))
                {
                    if (key == null) return false;
                    if (string.IsNullOrEmpty(description))
                        key.DeleteValue("srvcomment", false);
                    else
                        key.SetValue("srvcomment", description, RegistryValueKind.String);
                    return true;
                }
            }
            catch { return false; }
        }

        #endregion

        #region BCD 启动选项

        /// <summary>获取系统默认启动项。</summary>
        /// <returns>默认启动项的标识符，如 "{current}"，失败返回 null。</returns>
        public static string GetDefaultBootEntry()
        {
            string output = RunBcdEdit("/enum {bootmgr}");
            return output != null ? ExtractValue(output, "default") : null;
        }

        /// <summary>设置系统默认启动项。</summary>
        /// <param name="identifier">启动项标识符，如 "{current}"。</param>
        /// <returns>是否成功。</returns>
        public static bool SetDefaultBootEntry(string identifier)
        {
            return RunBcdEditSilent("/default " + identifier);
        }

        /// <summary>获取引导菜单超时时间（秒）。</summary>
        /// <returns>超时秒数，失败返回 -1。</returns>
        public static int GetBootTimeout()
        {
            string output = RunBcdEdit("/enum {bootmgr}");
            string timeout = ExtractValue(output, "timeout");
            if (int.TryParse(timeout, out int t))
                return t;
            return -1;
        }

        /// <summary>设置引导菜单超时时间（秒）。</summary>
        /// <param name="seconds">超时秒数。</param>
        /// <returns>是否成功。</returns>
        public static bool SetBootTimeout(int seconds)
        {
            return RunBcdEditSilent("/timeout " + seconds);
        }

        /// <summary>RunBcdEdit 方法。</summary>
        private static string RunBcdEdit(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("bcdedit.exe", arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return null;
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output;
                }
            }
            catch { return null; }
        }

        /// <summary>RunBcdEditSilent 方法。</summary>
        private static bool RunBcdEditSilent(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("bcdedit.exe", arguments)
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

        /// <summary>ExtractValue 方法。</summary>
        private static string ExtractValue(string output, string key)
        {
            if (string.IsNullOrEmpty(output)) return null;
            var match = System.Text.RegularExpressions.Regex.Match(output, key + @"\s+(.+)");
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        #endregion

        #region Win32 API 常量及声明

        // 修复：0x001F 不是视觉效果动作值（实为已废弃的 SPI_SETFASTTASKSWITCH）；
        // 正确的 UI 效果总开关为 SPI_SETUIEFFECTS(0x003F)，pvParam 为 BOOL
        private const uint SPI_SETUIEFFECTS = 0x003F;
        private const uint SPIF_UPDATEINIFILE = 0x0001; // 将设置写入用户配置文件（持久化）
        private const uint SPIF_SENDCHANGE = 0x0002;    // 广播 WM_SETTINGCHANGE 通知应用刷新

        private enum ComputerNameFormat { ComputerNamePhysicalDnsHostname = 5 }

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetComputerNameEx(ComputerNameFormat NameType, string lpBuffer);

        #endregion
    }
}
