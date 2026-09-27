using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 电源管理扩展工具类（集成多语言翻译，包含防休眠功能）。
    /// 提供电源计划详情查询与设置、电池信息、唤醒定时器列表、防止系统休眠/恢复休眠等功能。
    /// 底层通过 powercfg 命令、WMI 和 Win32 API 实现，异常安全。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HPowerManagement
    {
        #region 翻译辅助


        /// <summary>
        /// 控制台程序按系统 OEM 代码页输出（中文系统为 GBK/936）。
        /// 修复：重定向输出默认按 UTF-8 解码，中文系统下 powercfg 的中文标签全部乱码，导致正则匹配失败。
        /// </summary>
        private static readonly Encoding OemEncoding = GetOemEncoding();

        /// <summary>获取 oemEncoding。</summary>
        private static Encoding GetOemEncoding()
        {
            try { return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage); }
            catch { return Encoding.Default; }
        }

        /// <summary>
        /// 读取重定向命令的全部输出（按字节读取后自适应解码）。
        /// 修复：powercfg 在控制台为 UTF-8(65001) 时输出 UTF-8、否则按 OEM(GBK) 输出，
        /// 固定用单一编码解码会导致「电源方案 GUID」「电源设置索引」等中文标签乱码、正则匹配失败。
        /// </summary>
        private static string ReadAllOutput(Process p)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                p.StandardOutput.BaseStream.CopyTo(ms);
                byte[] bytes = ms.ToArray();
                string utf8 = Encoding.UTF8.GetString(bytes);
                if (utf8.IndexOf('\uFFFD') >= 0)
                {
                    try { return OemEncoding.GetString(bytes).Replace("\uFEFF", ""); }
                    catch { }
                }
                return utf8.Replace("\uFEFF", "");
            }
        }

        #endregion

        #region 信息类定义

        /// <summary>当前活动的电源计划简要信息。</summary>
        public class ActivePowerScheme
        {
            /// <summary>计划 GUID</summary>
            public string SchemeGuid { get; set; }
            /// <summary>计划名称（如“平衡”）</summary>
            public string Name { get; set; }
        }

        /// <summary>电池信息。</summary>
        public class BatteryInfo
        {
            /// <summary>电池名称</summary>
            public string Name { get; set; }
            /// <summary>预计剩余电量百分比</summary>
            public ushort EstimatedChargeRemaining { get; set; }
            /// <summary>电池状态（充电/放电/已充满等，已翻译）</summary>
            public string Status { get; set; }
            /// <summary>预计剩余运行时间（分钟）</summary>
            public uint EstimatedRunTime { get; set; }
        }

        /// <summary>唤醒定时器信息。</summary>
        public class WakeTimerInfo
        {
            /// <summary>定时器名称</summary>
            public string Name { get; set; }
            /// <summary>状态（已启用/已禁用）</summary>
            public string Status { get; set; }
            /// <summary>下次唤醒时间</summary>
            public string NextWakeTime { get; set; }
            /// <summary>拥有者</summary>
            public string Owner { get; set; }
        }

        /// <summary>电源计划详情（具体设置）。</summary>
        public class PowerSettingDetail
        {
            /// <summary>设置名称（如“显示器超时”）（已翻译）</summary>
            public string SettingName { get; set; }
            /// <summary>接通电源时的值（秒或分钟）</summary>
            public int AcValue { get; set; }
            /// <summary>使用电池时的值（秒或分钟）</summary>
            public int DcValue { get; set; }
            /// <summary>值的单位描述（已翻译）</summary>
            public string Unit { get; set; }
        }

        #endregion

        #region 电源计划基础

        /// <summary>获取当前活动的电源计划名称和 GUID。</summary>
        /// <returns>活动计划信息，失败返回 null。</returns>
        public static ActivePowerScheme GetActivePowerScheme()
        {
            try
            {
                string output = RunPowerCfg("/getactivescheme");
                if (string.IsNullOrEmpty(output)) return null;

                // 修复：中文系统 powercfg 输出「电源方案 GUID: ... (平衡)」，需同时兼容中英文标签
                var match = Regex.Match(output, @"^(?:Power Scheme GUID|电源方案 GUID):\s*([a-fA-F0-9\-]+)\s*[（(](.+?)[)）]", RegexOptions.Multiline);
                if (match.Success)
                {
                    return new ActivePowerScheme
                    {
                        SchemeGuid = match.Groups[1].Value.Trim(),
                        Name = match.Groups[2].Value.Trim()
                    };
                }
            }
            catch { }
            return null;
        }

        /// <summary>获取所有电源计划列表。</summary>
        /// <returns>计划 GUID 和名称的列表。</returns>
        public static List<ActivePowerScheme> GetPowerSchemes()
        {
            var list = new List<ActivePowerScheme>();
            try
            {
                string output = RunPowerCfg("/list");
                foreach (Match m in Regex.Matches(output, @"([a-fA-F0-9\-]+)\s+\((.+)\)"))
                {
                    list.Add(new ActivePowerScheme
                    {
                        SchemeGuid = m.Groups[1].Value.Trim(),
                        Name = m.Groups[2].Value.Trim()
                    });
                }
            }
            catch { }
            return list;
        }

        /// <summary>激活指定的电源计划。</summary>
        /// <param name="schemeGuid">电源计划 GUID。</param>
        /// <returns>是否成功。</returns>
        public static bool SetActivePowerScheme(string schemeGuid)
        {
            return RunPowerCfgSilent("/setactive " + schemeGuid);
        }

        #endregion

        #region 常用电源设置获取

        /// <summary>获取显示器超时设置（秒）。</summary>
        public static PowerSettingDetail GetMonitorTimeout()
        {
            var detail = new PowerSettingDetail { SettingName = HTranslation.GetContent("显示器超时"), Unit = HTranslation.GetContent("秒") };
            string output = RunPowerCfg("/query SCHEME_CURRENT SUB_VIDEO 3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e");
            detail.AcValue = ParsePowerSettingValue(output, "AC Power Setting Index", "当前交流电源设置索引");
            detail.DcValue = ParsePowerSettingValue(output, "DC Power Setting Index", "当前直流电源设置索引");
            return detail;
        }

        /// <summary>获取硬盘超时设置（秒）。</summary>
        public static PowerSettingDetail GetDiskTimeout()
        {
            var detail = new PowerSettingDetail { SettingName = HTranslation.GetContent("硬盘超时"), Unit = HTranslation.GetContent("秒") };
            string output = RunPowerCfg("/query SCHEME_CURRENT SUB_DISK 6738e2c4-e8a5-4a42-b16a-e040e769756e");
            detail.AcValue = ParsePowerSettingValue(output, "AC Power Setting Index", "当前交流电源设置索引");
            detail.DcValue = ParsePowerSettingValue(output, "DC Power Setting Index", "当前直流电源设置索引");
            return detail;
        }

        /// <summary>获取睡眠超时设置（秒）。</summary>
        public static PowerSettingDetail GetSleepTimeout()
        {
            var detail = new PowerSettingDetail { SettingName = HTranslation.GetContent("睡眠超时"), Unit = HTranslation.GetContent("秒") };
            string output = RunPowerCfg("/query SCHEME_CURRENT SUB_SLEEP 29f6c1db-86da-48c5-9fdb-f2b67b1f44da");
            detail.AcValue = ParsePowerSettingValue(output, "AC Power Setting Index", "当前交流电源设置索引");
            detail.DcValue = ParsePowerSettingValue(output, "DC Power Setting Index", "当前直流电源设置索引");
            return detail;
        }

        /// <summary>获取休眠超时设置（秒）。</summary>
        public static PowerSettingDetail GetHibernateTimeout()
        {
            var detail = new PowerSettingDetail { SettingName = HTranslation.GetContent("休眠超时"), Unit = HTranslation.GetContent("秒") };
            string output = RunPowerCfg("/query SCHEME_CURRENT SUB_SLEEP 9d7815a6-7ee4-497e-8888-515a05f02364");
            detail.AcValue = ParsePowerSettingValue(output, "AC Power Setting Index", "当前交流电源设置索引");
            detail.DcValue = ParsePowerSettingValue(output, "DC Power Setting Index", "当前直流电源设置索引");
            return detail;
        }

        /// <summary>获取常用电源设置详情列表。</summary>
        public static List<PowerSettingDetail> GetCommonPowerSettings()
        {
            var list = new List<PowerSettingDetail>();
            try
            {
                list.Add(GetMonitorTimeout());
                list.Add(GetDiskTimeout());
                list.Add(GetSleepTimeout());
                list.Add(GetHibernateTimeout());
            }
            catch { }
            return list;
        }

        /// <summary>ParsePowerSettingValue 方法。</summary>
        private static int ParsePowerSettingValue(string output, string key, string cnKey)
        {
            try
            {
                // 修复：中文系统 powercfg 输出「当前交流/直流电源设置索引: 0x...」，需同时兼容中英文标签
                string pattern = @"(?:" + Regex.Escape(key) + "|" + Regex.Escape(cnKey) + @"):\s*(0x[0-9a-fA-F]+|(\d+))";
                var match = Regex.Match(output, pattern);
                if (match.Success)
                {
                    string val = match.Groups[1].Value;
                    if (val.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt32(val, 16);
                    else
                        return int.Parse(val);
                }
            }
            catch { }
            return -1;
        }

        #endregion

        #region 设置常用电源选项

        /// <summary>设置显示器超时（秒）。</summary>
        /// <param name="acSeconds">接通电源时的超时秒数。</param>
        /// <param name="dcSeconds">使用电池时的超时秒数。</param>
        /// <returns>是否成功。</returns>
        public static bool SetMonitorTimeout(int acSeconds, int dcSeconds)
        {
            bool ok = true;
            if (acSeconds >= 0) ok &= RunPowerCfgSilent("/setacvalueindex SCHEME_CURRENT SUB_VIDEO 3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e " + acSeconds);
            if (dcSeconds >= 0) ok &= RunPowerCfgSilent("/setdcvalueindex SCHEME_CURRENT SUB_VIDEO 3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e " + dcSeconds);
            if (ok) RunPowerCfgSilent("/setactive SCHEME_CURRENT");
            return ok;
        }

        /// <summary>设置硬盘超时（秒）。</summary>
        public static bool SetDiskTimeout(int acSeconds, int dcSeconds)
        {
            bool ok = true;
            if (acSeconds >= 0) ok &= RunPowerCfgSilent("/setacvalueindex SCHEME_CURRENT SUB_DISK 6738e2c4-e8a5-4a42-b16a-e040e769756e " + acSeconds);
            if (dcSeconds >= 0) ok &= RunPowerCfgSilent("/setdcvalueindex SCHEME_CURRENT SUB_DISK 6738e2c4-e8a5-4a42-b16a-e040e769756e " + dcSeconds);
            if (ok) RunPowerCfgSilent("/setactive SCHEME_CURRENT");
            return ok;
        }

        /// <summary>设置睡眠超时（秒）。</summary>
        public static bool SetSleepTimeout(int acSeconds, int dcSeconds)
        {
            bool ok = true;
            if (acSeconds >= 0) ok &= RunPowerCfgSilent("/setacvalueindex SCHEME_CURRENT SUB_SLEEP 29f6c1db-86da-48c5-9fdb-f2b67b1f44da " + acSeconds);
            if (dcSeconds >= 0) ok &= RunPowerCfgSilent("/setdcvalueindex SCHEME_CURRENT SUB_SLEEP 29f6c1db-86da-48c5-9fdb-f2b67b1f44da " + dcSeconds);
            if (ok) RunPowerCfgSilent("/setactive SCHEME_CURRENT");
            return ok;
        }

        /// <summary>设置休眠超时（秒）。</summary>
        public static bool SetHibernateTimeout(int acSeconds, int dcSeconds)
        {
            bool ok = true;
            if (acSeconds >= 0) ok &= RunPowerCfgSilent("/setacvalueindex SCHEME_CURRENT SUB_SLEEP 9d7815a6-7ee4-497e-8888-515a05f02364 " + acSeconds);
            if (dcSeconds >= 0) ok &= RunPowerCfgSilent("/setdcvalueindex SCHEME_CURRENT SUB_SLEEP 9d7815a6-7ee4-497e-8888-515a05f02364 " + dcSeconds);
            if (ok) RunPowerCfgSilent("/setactive SCHEME_CURRENT");
            return ok;
        }

        #endregion

        #region 电池信息

        /// <summary>获取笔记本电池信息。</summary>
        /// <returns>电池信息列表。</returns>
        public static List<BatteryInfo> GetBatteryInfo()
        {
            var list = new List<BatteryInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var battery = new BatteryInfo
                        {
                            Name = obj["Name"]?.ToString() ?? "",
                            EstimatedChargeRemaining = Convert.ToUInt16(obj["EstimatedChargeRemaining"] ?? 0),
                            EstimatedRunTime = Convert.ToUInt32(obj["EstimatedRunTime"] ?? 0)
                        };
                        // 注意：必须用 BatteryStatus（uint16 数值码 1~11），不能用 CIM 的 Status 字符串属性
                        // （Status 返回的是 "OK"/"Degraded" 等字符串，与下方 1~8 数字分支完全对不上）
                        string rawStatus = obj["BatteryStatus"]?.ToString() ?? "";
                        battery.Status = TranslateBatteryStatus(rawStatus);
                        list.Add(battery);
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>TranslateBatteryStatus 方法。</summary>
        private static string TranslateBatteryStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return HTranslation.GetContent("未知");
            switch (status)
            {
                case "1": return HTranslation.GetContent("正在放电");
                case "2": return HTranslation.GetContent("交流电源");
                case "3": return HTranslation.GetContent("已充满");
                case "4": return HTranslation.GetContent("低");
                case "5": return HTranslation.GetContent("临界");
                case "6": return HTranslation.GetContent("正在充电");
                case "7": return HTranslation.GetContent("充电中");
                case "8": return HTranslation.GetContent("高");
                case "9": return HTranslation.GetContent("充电中(低电量)");
                case "10": return HTranslation.GetContent("充电中(临界电量)");
                case "11": return HTranslation.GetContent("未定义");
                default: return status;
            }
        }

        #endregion

        #region 唤醒定时器

        /// <summary>获取系统已启用的唤醒定时器列表。</summary>
        /// <returns>定时器信息列表。</returns>
        public static List<WakeTimerInfo> GetWakeTimers()
        {
            var list = new List<WakeTimerInfo>();
            try
            {
                string output = RunPowerCfg("-waketimers");
                if (string.IsNullOrEmpty(output)) return list;
                var timerBlocks = output.Split(new[] { "Timer:" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string block in timerBlocks)
                {
                    if (string.IsNullOrWhiteSpace(block)) continue;
                    var timer = new WakeTimerInfo();
                    timer.Name = ExtractValue(block, "Name:");
                    timer.Status = ExtractValue(block, "Status:");
                    timer.NextWakeTime = ExtractValue(block, "Next Wake:");
                    timer.Owner = ExtractValue(block, "Owner:");
                    if (!string.IsNullOrEmpty(timer.Name))
                        list.Add(timer);
                }
            }
            catch { }
            return list;
        }

        /// <summary>ExtractValue 方法。</summary>
        private static string ExtractValue(string block, string key)
        {
            var match = Regex.Match(block, Regex.Escape(key) + @"\s*(.+)");
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        #endregion

        #region 防止休眠（系统将持续运行）

        /// <summary>阻止系统自动休眠或关闭显示器。</summary>
        /// <param name="keepScreenOn">是否同时保持显示器常亮。默认为 true。</param>
        public static void PreventSleep(bool keepScreenOn = true)
        {
            EXECUTION_STATE flags = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;
            if (keepScreenOn)
                flags |= EXECUTION_STATE.ES_DISPLAY_REQUIRED;
            SetThreadExecutionState(flags);
        }

        /// <summary>恢复正常的电源管理（允许系统休眠和关闭显示器）。</summary>
        public static void AllowSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_CONTINUOUS = 0x80000000,
        }

        [DllImport("kernel32.dll")]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        #endregion

        #region 辅助命令执行

        /// <summary>RunPowerCfg 方法。</summary>
        private static string RunPowerCfg(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powercfg.exe", arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    // 修复：powercfg 按系统 OEM 代码页输出（中文系统 GBK），需正确解码中文标签
                    StandardOutputEncoding = OemEncoding
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return null;
                    string output = ReadAllOutput(p);
                    p.WaitForExit();
                    return output;
                }
            }
            catch { return null; }
        }

        /// <summary>RunPowerCfgSilent 方法。</summary>
        private static bool RunPowerCfgSilent(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powercfg.exe", arguments)
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
    }
}
