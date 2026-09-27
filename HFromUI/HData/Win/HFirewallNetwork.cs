using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 防火墙与网络安全工具类（集成多语言翻译）。
    /// 提供防火墙规则管理、端口监听查询、IP 路由表、DNS 缓存管理及防火墙状态检测。
    /// 底层依赖 netsh、IPGlobalProperties、WMI 和 ipconfig 命令。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HFirewallNetwork
    {

        #region 命令输出编码

        /// <summary>
        /// netsh/ipconfig 按系统 OEM 代码页输出（中文系统为 GBK/936）。
        /// 修复：重定向输出默认按 UTF-8 解码，中文系统下中文标签（如「状态」「记录名称」）乱码导致解析失败。
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
        /// 修复：netsh/ipconfig 等命令在控制台为 UTF-8(65001) 时输出 UTF-8、否则按 OEM(GBK) 输出，
        /// 固定用单一编码解码会导致中文标签（状态/规则名称等）乱码、状态解析恒为「未知」。
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

        /// <summary>防火墙规则信息。</summary>
        public class FirewallRuleInfo
        {
            /// <summary>规则名称</summary>
            public string Name { get; set; }
            /// <summary>方向（入站/出站，已翻译）</summary>
            public string Direction { get; set; }
            /// <summary>操作（允许/阻止，已翻译）</summary>
            public string Action { get; set; }
            /// <summary>是否启用（已翻译）</summary>
            public string Enabled { get; set; }
            /// <summary>协议类型（TCP/UDP/Any）</summary>
            public string Protocol { get; set; }
            /// <summary>本地端口</summary>
            public string LocalPort { get; set; }
            /// <summary>远程地址</summary>
            public string RemoteAddress { get; set; }
            /// <summary>程序路径</summary>
            public string Program { get; set; }
            /// <summary>描述</summary>
            public string Description { get; set; }
        }

        /// <summary>监听端口信息。</summary>
        public class ListeningPortInfo
        {
            /// <summary>协议（TCP/UDP）</summary>
            public string Protocol { get; set; }
            /// <summary>本地 IP 地址</summary>
            public string LocalAddress { get; set; }
            /// <summary>本地端口号</summary>
            public int Port { get; set; }
        }

        /// <summary>IP 路由表条目。</summary>
        public class RouteEntryInfo
        {
            /// <summary>目标网络（如 "0.0.0.0"）</summary>
            public string Destination { get; set; }
            /// <summary>子网掩码</summary>
            public string Mask { get; set; }
            /// <summary>网关</summary>
            public string NextHop { get; set; }
            /// <summary>接口索引</summary>
            public int InterfaceIndex { get; set; }
            /// <summary>跃点数</summary>
            public int Metric { get; set; }
        }

        /// <summary>DNS 缓存条目。</summary>
        public class DnsCacheEntry
        {
            /// <summary>域名</summary>
            public string Name { get; set; }
            /// <summary>IP 地址</summary>
            public string IPAddress { get; set; }
            /// <summary>生存时间（秒）</summary>
            public int TTL { get; set; }
        }

        #endregion

        #region 防火墙状态

        /// <summary>获取防火墙当前配置文件的启用状态。</summary>
        /// <returns>描述性文本（已翻译，如“已启用”）。</returns>
        public static string GetFirewallState()
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", "advfirewall show currentprofile")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = OemEncoding
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return HTranslation.GetContent("未知");
                    // 修复：与规则/DNS 解析一致使用自适应字节解码（本环境 netsh 可能输出 UTF-8 中文，
                    // 固定 GBK 解码会把「状态」乱码成「鐘舵€?」导致状态恒为「未知」）
                    string output = ReadAllOutput(p);
                    p.WaitForExit();
                    foreach (string line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // 修复：show currentprofile 的状态行没有冒号：英文 "State   OFF"、中文 "状态   关闭"
                        // 原实现 IndexOf(':') 返回 -1 后 Substring(0) 取到整行，永远匹配不到 ON/OFF
                        if (!line.TrimStart().StartsWith("State", StringComparison.OrdinalIgnoreCase) &&
                            !line.TrimStart().StartsWith("状态", StringComparison.Ordinal))
                            continue;
                        // 按空白/冒号切分，标签后面的第一个词即为状态值
                        string[] parts = line.Split(new[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) continue;
                        string state = parts[1].Trim();
                        // 修复：兼容中文系统输出（打开/关闭）与英文系统（ON/OFF）
                        if (state.Equals("ON", StringComparison.OrdinalIgnoreCase) || state.Equals("打开") || state.Equals("开启"))
                            return HTranslation.GetContent("已启用");
                        if (state.Equals("OFF", StringComparison.OrdinalIgnoreCase) || state.Equals("关闭"))
                            return HTranslation.GetContent("已禁用");
                        return state;
                    }
                }
            }
            catch { }
            return HTranslation.GetContent("未知");
        }

        #endregion

        #region 防火墙规则管理

        /// <summary>获取所有防火墙规则（入站和出站）。</summary>
        /// <returns>防火墙规则列表。</returns>
        public static List<FirewallRuleInfo> GetAllFirewallRules()
        {
            var rules = new List<FirewallRuleInfo>();
            // 分别获取入站和出站规则
            rules.AddRange(GetFirewallRules("in"));
            rules.AddRange(GetFirewallRules("out"));
            return rules;
        }

        /// <summary>获取指定方向的防火墙规则。</summary>
        /// <param name="direction">"in" 或 "out"</param>
        /// <returns>规则列表。</returns>
        private static List<FirewallRuleInfo> GetFirewallRules(string direction)
        {
            var list = new List<FirewallRuleInfo>();
            try
            {
                string args = "advfirewall firewall show rule name=all dir=" + direction + " verbose";
                var psi = new ProcessStartInfo("netsh", args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = OemEncoding
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return list;
                    string output = ReadAllOutput(p);
                    p.WaitForExit();
                    list = ParseFirewallRules(output, direction);
                }
            }
            catch { }
            return list;
        }

        /// <summary>ParseFirewallRules 方法。</summary>
        private static List<FirewallRuleInfo> ParseFirewallRules(string netshOutput, string direction)
        {
            var rules = new List<FirewallRuleInfo>();
            // 兼容中英文标题（netsh 规则输出通常为英文 "Rule Name:"，个别系统输出中文「规则名称:」）
            string[] blocks = Regex.Split(netshOutput, @"Rule Name:|规则名称\s*:");
            foreach (string block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block)) continue;
                var rule = new FirewallRuleInfo();
                rule.Direction = direction == "in" ? HTranslation.GetContent("入站") : HTranslation.GetContent("出站");

                foreach (string line in block.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("Enabled:", StringComparison.OrdinalIgnoreCase))
                        rule.Enabled = trimmed.Substring(8).Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) ? HTranslation.GetContent("是") : HTranslation.GetContent("否");
                    else if (trimmed.StartsWith("Action:", StringComparison.OrdinalIgnoreCase))
                        rule.Action = trimmed.Substring(7).Trim().Equals("Allow", StringComparison.OrdinalIgnoreCase) ? HTranslation.GetContent("允许") : HTranslation.GetContent("阻止");
                    else if (trimmed.StartsWith("Protocol:", StringComparison.OrdinalIgnoreCase))
                        rule.Protocol = trimmed.Substring(9).Trim();
                    else if (trimmed.StartsWith("LocalPort:", StringComparison.OrdinalIgnoreCase))
                        rule.LocalPort = trimmed.Substring(10).Trim();
                    else if (trimmed.StartsWith("RemoteIP:", StringComparison.OrdinalIgnoreCase))
                        rule.RemoteAddress = trimmed.Substring(9).Trim();
                    else if (trimmed.StartsWith("Program:", StringComparison.OrdinalIgnoreCase))
                        rule.Program = trimmed.Substring(8).Trim();
                    else if (trimmed.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                        rule.Description = trimmed.Substring(12).Trim();
                }
                // 规则名称在 block 的第一行
                string name = block.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                if (!string.IsNullOrEmpty(name)) rule.Name = name;
                rules.Add(rule);
            }
            return rules;
        }

        /// <summary>添加一条简单的防火墙规则。</summary>
        /// <param name="name">规则名称。</param>
        /// <param name="direction">方向，"in" 或 "out"。</param>
        /// <param name="action">操作，"allow" 或 "block"。</param>
        /// <param name="protocol">协议，如 "TCP"、"UDP"。</param>
        /// <param name="localPort">本地端口，如 "80"。</param>
        /// <returns>是否成功。</returns>
        public static bool AddFirewallRule(string name, string direction, string action, string protocol, string localPort)
        {
            string args = $"advfirewall firewall add rule name=\"{name}\" dir={direction} action={action} protocol={protocol} localport={localPort}";
            return RunNetshCommand(args);
        }

        /// <summary>删除指定名称的防火墙规则。</summary>
        /// <param name="name">规则名称。</param>
        /// <returns>是否成功。</returns>
        public static bool DeleteFirewallRule(string name)
        {
            string args = $"advfirewall firewall delete rule name=\"{name}\"";
            return RunNetshCommand(args);
        }

        /// <summary>启用防火墙规则。</summary>
        public static bool EnableFirewallRule(string name)
        {
            return RunNetshCommand($"advfirewall firewall set rule name=\"{name}\" new enable=yes");
        }

        /// <summary>禁用防火墙规则。</summary>
        public static bool DisableFirewallRule(string name)
        {
            return RunNetshCommand($"advfirewall firewall set rule name=\"{name}\" new enable=no");
        }

        /// <summary>RunNetshCommand 方法。</summary>
        private static bool RunNetshCommand(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("netsh", arguments)
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

        #region 端口监听查询

        /// <summary>获取当前所有 TCP/UDP 监听端口。</summary>
        /// <returns>监听端口列表。</returns>
        public static List<ListeningPortInfo> GetListeningPorts()
        {
            var ports = new List<ListeningPortInfo>();
            try
            {
                IPGlobalProperties ip = IPGlobalProperties.GetIPGlobalProperties();
                // TCP 监听
                foreach (IPEndPoint ep in ip.GetActiveTcpListeners())
                {
                    ports.Add(new ListeningPortInfo
                    {
                        Protocol = "TCP",
                        LocalAddress = ep.Address.ToString(),
                        Port = ep.Port
                    });
                }
                // UDP 监听
                foreach (IPEndPoint ep in ip.GetActiveUdpListeners())
                {
                    ports.Add(new ListeningPortInfo
                    {
                        Protocol = "UDP",
                        LocalAddress = ep.Address.ToString(),
                        Port = ep.Port
                    });
                }
            }
            catch { }
            return ports;
        }

        #endregion

        #region IP 路由表

        /// <summary>获取 IPv4 路由表。</summary>
        /// <returns>路由条目列表。</returns>
        public static List<RouteEntryInfo> GetRouteTable()
        {
            var routes = new List<RouteEntryInfo>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_IP4RouteTable"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        routes.Add(new RouteEntryInfo
                        {
                            Destination = obj["Destination"]?.ToString() ?? "",
                            Mask = obj["Mask"]?.ToString() ?? "",
                            NextHop = obj["NextHop"]?.ToString() ?? "",
                            InterfaceIndex = Convert.ToInt32(obj["InterfaceIndex"] ?? 0),
                            Metric = Convert.ToInt32(obj["Metric1"] ?? 0)
                        });
                    }
                }
            }
            catch { }
            return routes;
        }

        #endregion

        #region DNS 缓存管理

        /// <summary>获取当前 DNS 缓存内容。</summary>
        /// <returns>DNS 缓存条目列表。</returns>
        public static List<DnsCacheEntry> GetDnsCache()
        {
            var cache = new List<DnsCacheEntry>();
            try
            {
                var psi = new ProcessStartInfo("ipconfig", "/displaydns")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = OemEncoding
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return cache;
                    string output = ReadAllOutput(p);
                    p.WaitForExit();
                    // 修复：按记录起始标签分块（英文 "Record Name" / 中文 "记录名称"），
                    // 原正则只匹配中文标签且跨行要求严格，英文系统或空缓存时恒为空
                    string[] blocks = Regex.Split(output, @"(?:Record Name|记录名称)");
                    foreach (string block in blocks)
                    {
                        if (string.IsNullOrEmpty(block) || block.IndexOf(':') < 0) continue;
                        // 名称：块内第一个冒号后的内容
                        Match nameM = Regex.Match(block, @"[:：]\s*(\S+)");
                        // TTL：英文 "Time To Live" / 中文 "生存时间"
                        Match ttlM = Regex.Match(block, @"(?:Time To Live|生存时间)[^\d]*(\d+)");
                        if (!nameM.Success || !ttlM.Success || !int.TryParse(ttlM.Groups[1].Value, out int ttl)) continue;
                        // IP：解析记录行（A/AAAA Record / 记录），取第一个能解析为 IP 的值，忽略 CNAME
                        // 修复：ipconfig 的省略号为点空格交替（". . . . :"），原正则 \.+\s*: 只认连续点串，中文系统下恒匹配失败
                        string ip = null;
                        foreach (Match rm in Regex.Matches(block, @"(?:Record|记录)[\s.]*:\s*(\S+)"))
                        {
                            string cand = rm.Groups[1].Value.Trim().TrimEnd('.');
                            if (IPAddress.TryParse(cand, out _)) { ip = cand; break; }
                        }
                        if (!string.IsNullOrEmpty(ip))
                            cache.Add(new DnsCacheEntry { Name = nameM.Groups[1].Value.Trim().TrimEnd('.'), IPAddress = ip, TTL = ttl });
                    }
                }
            }
            catch { }
            return cache;
        }

        /// <summary>清空 DNS 缓存。</summary>
        /// <returns>是否成功。</returns>
        public static bool ClearDnsCache()
        {
            try
            {
                var psi = new ProcessStartInfo("ipconfig", "/flushdns")
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
