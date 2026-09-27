using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// Windows 事件日志管理工具类（集成多语言翻译，已优化导出/清除）。
    /// 提供应用程序、安全、系统等日志的读取（支持过滤和条数限制）、日志导出、日志清除等功能。
    /// 底层使用 System.Diagnostics.EventLog 和 wevtutil 命令，无需额外依赖。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HEventLog
    {

        #region 信息类定义

        /// <summary>事件日志条目信息。</summary>
        public class EventLogEntryInfo
        {
            /// <summary>日志名称（如 Application、System）</summary>
            public string LogName { get; set; }
            /// <summary>事件 ID</summary>
            public int EventID { get; set; }
            /// <summary>事件类型（错误/警告/信息等，已翻译）</summary>
            public string EntryType { get; set; }
            /// <summary>来源</summary>
            public string Source { get; set; }
            /// <summary>事件描述</summary>
            public string Message { get; set; }
            /// <summary>记录时间（本地时间）</summary>
            public DateTime TimeGenerated { get; set; }
            /// <summary>计算机名称</summary>
            public string MachineName { get; set; }
            /// <summary>用户名称（若可用）</summary>
            public string UserName { get; set; }
        }

        #endregion

        #region 日志读取

        /// <summary>读取指定事件日志的最近若干条记录。</summary>
        /// <param name="logName">日志名称，如 "Application"、"System"、"Security"（需管理员权限）</param>
        /// <param name="maxEntries">最大返回条数，-1 表示所有条目（慎用）</param>
        /// <returns>事件日志条目列表，按时间降序排列（最新在前）。</returns>
        public static List<EventLogEntryInfo> ReadEventLog(string logName, int maxEntries = 100)
        {
            var list = new List<EventLogEntryInfo>();
            try
            {
                // using 保证异常路径下也释放事件日志句柄
                using (EventLog log = new EventLog(logName))
                {
                    int count = 0;
                    // Entries 集合索引从 0 开始（最旧），我们倒序读取最新的
                    for (int i = log.Entries.Count - 1; i >= 0; i--)
                    {
                        EventLogEntry entry = log.Entries[i];
                        try
                        {
                            list.Add(new EventLogEntryInfo
                            {
                                LogName = logName,
                                EventID = entry.EventID,
                                EntryType = TranslateEntryType(entry.EntryType),
                                Source = entry.Source,
                                Message = entry.Message,
                                TimeGenerated = entry.TimeGenerated,
                                MachineName = entry.MachineName,
                                UserName = entry.UserName ?? ""
                            });
                            count++;
                            if (maxEntries > 0 && count >= maxEntries)
                                break;
                        }
                        catch { /* 跳过个别读取失败的条目 */ }
                    }
                }
            }
            catch { /* 无权限或日志不存在时返回空列表 */ }
            return list;
        }

        /// <summary>读取指定事件日志中满足类型过滤的最近若干条记录。</summary>
        /// <param name="logName">日志名称。</param>
        /// <param name="entryType">事件类型过滤："Error"、"Warning"、"Information" 等；null 表示不过滤。</param>
        /// <param name="maxEntries">最大返回条数。</param>
        /// <returns>事件日志条目列表。</returns>
        public static List<EventLogEntryInfo> ReadEventLogWithFilter(string logName, string entryType, int maxEntries = 100)
        {
            var list = new List<EventLogEntryInfo>();
            try
            {
                using (EventLog log = new EventLog(logName))
                {
                    int count = 0;
                    for (int i = log.Entries.Count - 1; i >= 0; i--)
                    {
                        EventLogEntry entry = log.Entries[i];
                        try
                        {
                            if (!string.IsNullOrEmpty(entryType) &&
                                !entry.EntryType.ToString().Equals(entryType, StringComparison.OrdinalIgnoreCase))
                                continue;

                            list.Add(new EventLogEntryInfo
                            {
                                LogName = logName,
                                EventID = entry.EventID,
                                EntryType = TranslateEntryType(entry.EntryType),
                                Source = entry.Source,
                                Message = entry.Message,
                                TimeGenerated = entry.TimeGenerated,
                                MachineName = entry.MachineName,
                                UserName = entry.UserName ?? ""
                            });
                            count++;
                            if (maxEntries > 0 && count >= maxEntries)
                                break;
                        }
                        catch { }
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取系统中所有事件日志名称（如 Application, System, Security 等）。</summary>
        public static List<string> GetEventLogNames()
        {
            var names = new List<string>();
            EventLog[] logs = null;
            try
            {
                logs = EventLog.GetEventLogs();
                foreach (EventLog log in logs)
                {
                    using (log)
                    {
                        names.Add(log.Log);
                    }
                }
            }
            catch
            {
                // 异常路径下把已拿到的日志句柄全部释放，避免泄漏
                if (logs != null)
                    foreach (EventLog log in logs) { try { log.Dispose(); } catch { } }
            }
            return names;
        }

        #endregion

        #region 日志导出（使用 wevtutil 避免 API 参数混乱）

        /// <summary>将指定事件日志导出为 evtx 文件。</summary>
        /// <param name="logName">日志名称，如 "Application"。</param>
        /// <param name="exportFilePath">导出文件完整路径（扩展名建议 .evtx）。</param>
        /// <returns>是否导出成功。</returns>
        public static bool ExportEventLog(string logName, string exportFilePath)
        {
            try
            {
                // 确保目录存在
                string dir = Path.GetDirectoryName(exportFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // 使用 wevtutil epl 命令导出
                string args = string.Format("epl \"{0}\" \"{1}\"", logName, exportFilePath);
                return RunWevtUtil(args);
            }
            catch { return false; }
        }

        #endregion

        #region 日志清除

        /// <summary>清除指定事件日志的所有条目（需要管理员权限）。</summary>
        /// <param name="logName">日志名称。</param>
        /// <returns>是否清除成功。</returns>
        public static bool ClearEventLog(string logName)
        {
            try
            {
                // 使用 wevtutil cl 命令清除日志
                return RunWevtUtil("cl \"" + logName + "\"");
            }
            catch { return false; }
        }

        /// <summary>清除指定事件日志并备份到文件。</summary>
        /// <param name="logName">日志名称。</param>
        /// <param name="backupFilePath">备份文件路径。</param>
        /// <returns>是否成功。</returns>
        public static bool ClearEventLogWithBackup(string logName, string backupFilePath)
        {
            if (ExportEventLog(logName, backupFilePath))
                return ClearEventLog(logName);
            return false;
        }

        #endregion

        #region 工具方法

        /// <summary>执行 wevtutil 命令并返回是否成功。</summary>
        private static bool RunWevtUtil(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("wevtutil.exe", arguments)
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

        /// <summary>翻译事件类型为可读文本。</summary>
        private static string TranslateEntryType(EventLogEntryType type)
        {
            switch (type)
            {
                case EventLogEntryType.Error: return HTranslation.GetContent("错误");
                case EventLogEntryType.Warning: return HTranslation.GetContent("警告");
                case EventLogEntryType.Information: return HTranslation.GetContent("信息");
                case EventLogEntryType.SuccessAudit: return HTranslation.GetContent("成功审核");
                case EventLogEntryType.FailureAudit: return HTranslation.GetContent("失败审核");
                default: return HTranslation.GetContent("未知");
            }
        }

        #endregion
    }
}
