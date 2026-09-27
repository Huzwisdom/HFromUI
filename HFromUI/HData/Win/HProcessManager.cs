using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 进程管理增强工具类（集成多语言翻译）。
    /// 提供进程优先级/Affinity设置（按名称）、终止进程（按名称/PID）、进程模块列表、
    /// 进程命令行参数获取、进程信息枚举等功能。
    /// 底层基于 System.Diagnostics.Process 和 WMI，异常安全。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HProcessManager
    {

        #region 信息类定义

        /// <summary>进程基本信息。</summary>
        public class ProcessInfo
        {
            /// <summary>进程ID</summary>
            public int ProcessId { get; set; }
            /// <summary>进程名称</summary>
            public string ProcessName { get; set; }
            /// <summary>主窗口标题（若有）</summary>
            public string MainWindowTitle { get; set; }
            /// <summary>工作集（内存字节数）</summary>
            public long WorkingSet { get; set; }
            /// <summary>启动时间</summary>
            public DateTime StartTime { get; set; }
            /// <summary>总CPU时间</summary>
            public TimeSpan TotalProcessorTime { get; set; }
            /// <summary>优先级类（已翻译）</summary>
            public string PriorityClass { get; set; }
            /// <summary>线程数</summary>
            public int Threads { get; set; }
            /// <summary>命令行参数（可能为空）</summary>
            public string CommandLine { get; set; }
        }

        #endregion

        #region 枚举进程

        /// <summary>获取系统所有进程的基本信息。</summary>
        /// <returns>进程信息列表。</returns>
        public static List<ProcessInfo> GetAllProcesses()
        {
            var list = new List<ProcessInfo>();
            try
            {
                // 先批量获取命令行（WMI），避免逐个查询性能差
                Dictionary<int, string> commandLines = GetProcessCommandLines();

                Process[] processes = Process.GetProcesses();
                foreach (Process p in processes)
                {
                    try
                    {
                        var info = new ProcessInfo
                        {
                            ProcessId = p.Id,
                            ProcessName = p.ProcessName,
                            MainWindowTitle = p.MainWindowTitle,
                            WorkingSet = p.WorkingSet64,
                            StartTime = p.StartTime,
                            TotalProcessorTime = p.TotalProcessorTime,
                            Threads = p.Threads.Count,
                            PriorityClass = TranslatePriorityClass(p.PriorityClass)
                        };

                        if (commandLines.TryGetValue(p.Id, out string cmd))
                            info.CommandLine = cmd;
                        else
                            info.CommandLine = "";

                        list.Add(info);
                    }
                    catch { /* 跳过无法访问的进程 */ }
                }
            }
            catch { }
            return list;
        }

        #endregion

        #region 获取命令行参数

        /// <summary>获取所有进程的命令行参数字典（进程ID -> 命令行）。</summary>
        /// <returns>字典，进程ID映射到命令行字符串。</returns>
        private static Dictionary<int, string> GetProcessCommandLines()
        {
            var dict = new Dictionary<int, string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessId, CommandLine FROM Win32_Process"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["ProcessId"] != null)
                        {
                            int pid = Convert.ToInt32(obj["ProcessId"]);
                            string cmd = obj["CommandLine"]?.ToString() ?? "";
                            dict[pid] = cmd;
                        }
                    }
                }
            }
            catch { }
            return dict;
        }

        /// <summary>获取指定进程ID的命令行参数。</summary>
        /// <param name="processId">进程ID。</param>
        /// <returns>命令行字符串，若失败返回空字符串。</returns>
        public static string GetProcessCommandLine(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + processId))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        #endregion

        #region 终止进程

        /// <summary>通过进程名称终止所有同名进程。</summary>
        /// <param name="processName">进程名称（不含扩展名，如 "notepad"）。</param>
        /// <returns>终止的进程数量。</returns>
        public static int KillProcessByName(string processName)
        {
            int count = 0;
            Process[] processes = null;
            try
            {
                processes = Process.GetProcessesByName(processName);
                foreach (Process p in processes)
                {
                    try { p.Kill(); count++; } catch { }
                }
            }
            catch { }
            finally
            {
                // Process 对象持有 OS 进程句柄，必须释放，否则句柄泄漏
                if (processes != null)
                    foreach (Process p in processes) { try { p.Dispose(); } catch { } }
            }
            return count;
        }

        /// <summary>通过进程ID终止进程。</summary>
        /// <param name="processId">进程ID。</param>
        /// <returns>是否成功。</returns>
        public static bool KillProcessById(int processId)
        {
            Process p = null;
            try
            {
                p = Process.GetProcessById(processId);
                p.Kill();
                return true;
            }
            catch { return false; }
            finally
            {
                // 释放进程句柄，避免句柄泄漏
                if (p != null) { try { p.Dispose(); } catch { } }
            }
        }

        #endregion

        #region 优先级/Affinity（按名称）

        /// <summary>设置所有同名进程的优先级类。</summary>
        /// <param name="processName">进程名称（如 "notepad"）。</param>
        /// <param name="priorityClass">优先级类（如 "High"、"AboveNormal" 等）。</param>
        /// <returns>成功设置的进程数量。</returns>
        public static int SetPriorityClassByName(string processName, ProcessPriorityClass priorityClass)
        {
            int count = 0;
            Process[] processes = null;
            try
            {
                processes = Process.GetProcessesByName(processName);
                foreach (Process p in processes)
                {
                    try
                    {
                        p.PriorityClass = priorityClass;
                        count++;
                    }
                    catch { }
                }
            }
            catch { }
            finally
            {
                if (processes != null)
                    foreach (Process p in processes) { try { p.Dispose(); } catch { } }
            }
            return count;
        }

        /// <summary>设置所有同名进程的 CPU 亲和性掩码。</summary>
        /// <param name="processName">进程名称。</param>
        /// <param name="affinityMask">亲和性掩码（如 0x0003 表示CPU0和1）。</param>
        /// <returns>成功设置的进程数量。</returns>
        public static int SetProcessorAffinityByName(string processName, IntPtr affinityMask)
        {
            int count = 0;
            Process[] processes = null;
            try
            {
                processes = Process.GetProcessesByName(processName);
                foreach (Process p in processes)
                {
                    try
                    {
                        p.ProcessorAffinity = affinityMask;
                        count++;
                    }
                    catch { }
                }
            }
            catch { }
            finally
            {
                if (processes != null)
                    foreach (Process p in processes) { try { p.Dispose(); } catch { } }
            }
            return count;
        }

        #endregion

        #region 进程模块列表

        /// <summary>列出指定进程加载的所有模块（DLL）。</summary>
        /// <param name="processId">进程ID。</param>
        /// <returns>模块文件路径列表，失败返回空列表。</returns>
        public static List<string> GetProcessModules(int processId)
        {
            var modules = new List<string>();
            Process p = null;
            try
            {
                p = Process.GetProcessById(processId);
                foreach (ProcessModule module in p.Modules)
                {
                    try
                    {
                        modules.Add(module.FileName);
                    }
                    catch { }
                }
            }
            catch { }
            finally
            {
                // 释放进程句柄（Modules 枚举还会打开目标进程句柄，一并释放）
                if (p != null) { try { p.Dispose(); } catch { } }
            }
            return modules;
        }

        /// <summary>列出指定进程名称的所有进程加载的模块。</summary>
        /// <param name="processName">进程名称（如 "explorer"）。</param>
        /// <returns>模块文件路径列表（去重）。</returns>
        public static List<string> GetProcessModulesByName(string processName)
        {
            var modules = new List<string>();
            Process[] processes = null;
            try
            {
                processes = Process.GetProcessesByName(processName);
                foreach (Process p in processes)
                {
                    modules.AddRange(GetProcessModules(p.Id));
                }
            }
            catch { }
            finally
            {
                if (processes != null)
                    foreach (Process p in processes) { try { p.Dispose(); } catch { } }
            }
            return modules.Distinct().ToList();
        }

        #endregion

        #region 工具方法

        /// <summary>翻译优先级类为可读字符串（已翻译）。</summary>
        private static string TranslatePriorityClass(ProcessPriorityClass priority)
        {
            switch (priority)
            {
                case ProcessPriorityClass.RealTime: return HTranslation.GetContent("实时");
                case ProcessPriorityClass.High: return HTranslation.GetContent("高");
                case ProcessPriorityClass.AboveNormal: return HTranslation.GetContent("高于正常");
                case ProcessPriorityClass.Normal: return HTranslation.GetContent("正常");
                case ProcessPriorityClass.BelowNormal: return HTranslation.GetContent("低于正常");
                case ProcessPriorityClass.Idle: return HTranslation.GetContent("空闲");
                default: return HTranslation.GetContent("未知");
            }
        }

        #endregion
    }
}
