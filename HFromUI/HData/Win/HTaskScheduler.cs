using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// Windows 计划任务管理工具类（集成多语言翻译）。
    /// 提供任务的列出、创建、删除、启用、禁用、运行，以及状态查询（上次运行结果、下次运行时间）等功能。
    /// 底层通过 schtasks.exe 命令实现，无需额外依赖。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HTaskScheduler
    {

        #region 命令输出编码

        /// <summary>
        /// schtasks/netsh 等控制台程序按系统 OEM 代码页输出（中文系统为 GBK/936）。
        /// 修复：进程重定向默认按 UTF-8 解码，中文系统下中文列名会变成乱码，
        /// 导致 BuildColumnMap 按中英文列名匹配全部失败、任务列表恒为空。
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
        /// 修复：netsh/schtasks 等命令在控制台为 UTF-8(65001) 时输出 UTF-8、否则按 OEM(GBK) 输出，
        /// 固定用单一编码解码会导致中文标签乱码、按列名/标签匹配全部失败。
        /// </summary>
        private static string ReadAllOutput(Process p)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                p.StandardOutput.BaseStream.CopyTo(ms);
                byte[] bytes = ms.ToArray();
                string utf8 = Encoding.UTF8.GetString(bytes);
                // UTF-8 解码出现替换字符，说明实际是本地 OEM(GBK) 编码
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

        /// <summary>计划任务简要信息。</summary>
        public class TaskInfo
        {
            /// <summary>任务名称（路径）</summary>
            public string Name { get; set; }
            /// <summary>任务状态：就绪、正在运行、已禁用等（已翻译）</summary>
            public string Status { get; set; }
            /// <summary>下次运行时间（字符串），若无则显示“无”</summary>
            public string NextRunTime { get; set; }
            /// <summary>上次运行结果（已翻译，如“操作成功完成”）</summary>
            public string LastResult { get; set; }
        }

        /// <summary>任务详细信息。</summary>
        public class TaskDetail
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>Description 成员。</summary>
            public string Description { get; set; }
            /// <summary>任务计划服务状态（已翻译）</summary>
            public string Status { get; set; }
            /// <summary>NextRunTime 成员。</summary>
            public string NextRunTime { get; set; }
            /// <summary>LastRunTime 成员。</summary>
            public string LastRunTime { get; set; }
            /// <summary>LastResult 成员。</summary>
            public string LastResult { get; set; }
            /// <summary>创建者</summary>
            public string Author { get; set; }
            /// <summary>要运行的程序</summary>
            public string ApplicationName { get; set; }
            /// <summary>参数</summary>
            public string Arguments { get; set; }
            /// <summary>工作目录</summary>
            public string WorkingDirectory { get; set; }
            /// <summary>触发器摘要</summary>
            public string Triggers { get; set; }
        }

        #endregion

        #region 任务列表

        /// <summary>获取所有计划任务的简要信息列表（包括系统任务）。</summary>
        /// <returns>任务信息列表。</returns>
        public static List<TaskInfo> GetAllTasks()
        {
            var list = new List<TaskInfo>();
            try
            {
                // 使用 /FO CSV 格式输出，便于解析
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = "/query /FO CSV /V",
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
                    if (string.IsNullOrEmpty(output)) return list;

                    string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length < 2) return list;

                    // 按标题行动态定位列（兼容 Win7 首列 TaskName 与 Win10/11 首列 HostName、中英文系统）
                    Dictionary<string, int> columnMap = BuildColumnMap(lines[0]);

                    // 跳过标题行，解析数据行
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        try
                        {
                            string[] fields = ParseCsvLine(line);
                            // 按列名取值（带中英文别名）
                            string name = GetFieldByNames(fields, columnMap, "TaskName", "任务名");
                            if (string.IsNullOrEmpty(name)) continue;
                            TaskInfo info = new TaskInfo
                            {
                                Name = name,
                                NextRunTime = NaToNone(GetFieldByNames(fields, columnMap, "Next Run Time", "下次运行时间")),
                                // 别名补充：中文版 schtasks 的 CSV 标题把 Status 列本地化为「模式」（实测列内值为就绪/正在运行/已禁用）
                                Status = GetFieldByNames(fields, columnMap, "Status", "状态", "模式") ?? "",
                                LastResult = GetFieldByNames(fields, columnMap, "Last Result", "上次结果") ?? ""
                            };
                            // 翻译状态
                            info.Status = TranslateTaskStatus(info.Status);
                            info.LastResult = TranslateTaskResult(info.LastResult);
                            list.Add(info);
                        }
                        catch { /* 跳过解析失败的行 */ }
                    }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 任务创建

        /// <summary>创建一个简单的计划任务（立即运行一次）。</summary>
        /// <param name="taskName">任务名称（唯一标识）。</param>
        /// <param name="programPath">要运行的程序完整路径。</param>
        /// <param name="arguments">命令行参数。</param>
        /// <returns>是否创建成功。</returns>
        public static bool CreateTask(string taskName, string programPath, string arguments = "")
        {
            return ExecuteSchTasks(string.Format(
                "/create /tn \"{0}\" /tr \"{1}\" {2} /sc once /st 00:00 /f",
                taskName, programPath, string.IsNullOrEmpty(arguments) ? "" : "/arg \"" + arguments + "\""));
        }

        /// <summary>创建一个每日定时执行的计划任务。</summary>
        /// <param name="taskName">任务名称。</param>
        /// <param name="programPath">程序路径。</param>
        /// <param name="arguments">参数。</param>
        /// <param name="startTime">开始时间（HH:mm 格式）。</param>
        /// <returns>是否创建成功。</returns>
        public static bool CreateDailyTask(string taskName, string programPath, string arguments, string startTime)
        {
            return ExecuteSchTasks(string.Format(
                "/create /tn \"{0}\" /tr \"{1}\" {2} /sc daily /st {3} /f",
                taskName, programPath,
                string.IsNullOrEmpty(arguments) ? "" : "/arg \"" + arguments + "\"",
                startTime));
        }

        /// <summary>创建一个在系统启动时运行的任务。</summary>
        public static bool CreateStartupTask(string taskName, string programPath, string arguments = "")
        {
            return ExecuteSchTasks(string.Format(
                "/create /tn \"{0}\" /tr \"{1}\" {2} /sc onstart /f",
                taskName, programPath,
                string.IsNullOrEmpty(arguments) ? "" : "/arg \"" + arguments + "\""));
        }

        #endregion

        #region 任务删除

        /// <summary>删除指定名称的计划任务。</summary>
        /// <param name="taskName">任务名称。</param>
        /// <returns>是否成功。</returns>
        public static bool DeleteTask(string taskName)
        {
            return ExecuteSchTasks("/delete /tn \"" + taskName + "\" /f");
        }

        #endregion

        #region 任务控制

        /// <summary>立即运行一次指定任务。</summary>
        public static bool RunTask(string taskName)
        {
            return ExecuteSchTasks("/run /tn \"" + taskName + "\"");
        }

        /// <summary>结束正在运行的任务。</summary>
        public static bool EndTask(string taskName)
        {
            return ExecuteSchTasks("/end /tn \"" + taskName + "\"");
        }

        /// <summary>启用指定任务。</summary>
        public static bool EnableTask(string taskName)
        {
            return ExecuteSchTasks("/change /tn \"" + taskName + "\" /enable");
        }

        /// <summary>禁用指定任务。</summary>
        public static bool DisableTask(string taskName)
        {
            return ExecuteSchTasks("/change /tn \"" + taskName + "\" /disable");
        }

        #endregion

        #region 任务状态查询

        /// <summary>获取指定任务的详细信息（含上次运行结果、下次运行时间等）。</summary>
        /// <param name="taskName">任务名称。</param>
        /// <returns>任务详细信息，若任务不存在返回 null。</returns>
        public static TaskDetail GetTaskDetail(string taskName)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = "/query /tn \"" + taskName + "\" /FO CSV /V",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = OemEncoding
                };
                using (Process p = Process.Start(psi))
                {
                    if (p == null) return null;
                    string output = ReadAllOutput(p);
                    p.WaitForExit();
                    if (string.IsNullOrEmpty(output)) return null;

                    string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length < 2) return null;
                    string dataLine = lines[1]; // 第二行是数据行
                    string[] fields = ParseCsvLine(dataLine);

                    // 按标题行动态定位列（修复原硬编码索引整行错位：
                    // 原实现把 Logon Mode 当 Last Result、Author 当 ApplicationName 等）
                    Dictionary<string, int> columnMap = BuildColumnMap(lines[0]);

                    TaskDetail detail = new TaskDetail();
                    detail.Name = GetFieldByNames(fields, columnMap, "TaskName", "任务名") ?? "";
                    detail.NextRunTime = NaToNone(GetFieldByNames(fields, columnMap, "Next Run Time", "下次运行时间"));
                    detail.Status = TranslateTaskStatus(GetFieldByNames(fields, columnMap, "Status", "状态", "模式"));
                    detail.LastResult = TranslateTaskResult(GetFieldByNames(fields, columnMap, "Last Result", "上次结果"));
                    detail.LastRunTime = NaToNone(GetFieldByNames(fields, columnMap, "Last Run Time", "上次运行时间"));
                    detail.Author = GetFieldByNames(fields, columnMap, "Author", "Creator", "创建者") ?? "";
                    detail.ApplicationName = GetFieldByNames(fields, columnMap, "Task To Run", "要运行的任务") ?? "";
                    detail.WorkingDirectory = GetFieldByNames(fields, columnMap, "Start In", "起始于") ?? "";
                    // CSV 中没有独立的参数字位列（参数包含在 Task To Run 里）
                    detail.Arguments = "";
                    detail.Triggers = GetFieldByNames(fields, columnMap, "Schedule Type", "Scheduled Type", "计划类型") ?? "";
                    detail.Description = GetFieldByNames(fields, columnMap, "Comment", "注释") ?? "";

                    return detail;
                }
            }
            catch { return null; }
        }

        #endregion

        #region 辅助方法

        /// <summary>执行 schtasks 命令。</summary>
        /// <param name="arguments">命令参数（不包含 schtasks.exe 本身）</param>
        /// <returns>执行是否成功（退出码为0）。</returns>
        private static bool ExecuteSchTasks(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe", arguments)
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

        /// <summary>简单解析 CSV 行，处理引号内的逗号。</summary>
        private static string[] ParseCsvLine(string line)
        {
            // 实现简单的 CSV 解析，假设字段由逗号分隔，字段可能被双引号包裹
            var fields = new List<string>();
            bool inQuotes = false;
            int start = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"') inQuotes = !inQuotes;
                else if (line[i] == ',' && !inQuotes)
                {
                    fields.Add(line.Substring(start, i - start));
                    start = i + 1;
                }
            }
            fields.Add(line.Substring(start));
            return fields.ToArray();
        }

        /// <summary>
        /// 根据 CSV 标题行构建「列名 -> 列索引」映射（列名忽略大小写与首尾空白/引号）。
        /// 背景：Win7 的 schtasks 输出首列是 TaskName，Win10/11 首列是 HostName，
        /// 硬编码列索引会导致整行字段错位（如把 Logon Mode 当成 Last Result）。
        /// </summary>
        private static Dictionary<string, int> BuildColumnMap(string headerLine)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            string[] headers = ParseCsvLine(headerLine);
            for (int i = 0; i < headers.Length; i++)
            {
                string name = headers[i].Trim('"').Trim();
                if (name.Length > 0 && !map.ContainsKey(name))
                {
                    map[name] = i;
                }
            }
            return map;
        }

        /// <summary>
        /// 按列的候选名称（中英文别名）从数据行取值；找不到返回 null。
        /// </summary>
        private static string GetFieldByNames(string[] fields, Dictionary<string, int> map, params string[] aliases)
        {
            if (map == null) return null;
            foreach (string alias in aliases)
            {
                int idx;
                if (map.TryGetValue(alias, out idx) && idx >= 0 && idx < fields.Length)
                {
                    return fields[idx].Trim('"').Trim();
                }
            }
            return null;
        }

        /// <summary>"N/A" 统一转为「无」，其余原样返回。</summary>
        private static string NaToNone(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return HTranslation.GetContent("无");
            return value;
        }

        /// <summary>TranslateTaskStatus 方法。</summary>
        private static string TranslateTaskStatus(string rawStatus)
        {
            if (string.IsNullOrEmpty(rawStatus)) return HTranslation.GetContent("未知");
            // schtasks 输出状态可能包含中文，直接返回翻译映射
            switch (rawStatus.ToLowerInvariant())
            {
                case "ready": return HTranslation.GetContent("就绪");
                case "running": return HTranslation.GetContent("正在运行");
                case "disabled": return HTranslation.GetContent("已禁用");
                default: return rawStatus;
            }
        }

        /// <summary>TranslateTaskResult 方法。</summary>
        private static string TranslateTaskResult(string result)
        {
            if (string.IsNullOrEmpty(result)) return HTranslation.GetContent("未知");
            // 结果为完成代码，可能是十六进制或数字
            // 常见：0x0 表示成功，其他表示错误
            if (result.Equals("0x0", StringComparison.OrdinalIgnoreCase) || result == "0")
                return HTranslation.GetContent("操作成功完成");
            if (result.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                return HTranslation.GetContent("无");
            return HTranslation.GetContent("错误") + " (" + result + ")";
        }

        #endregion
    }
}
