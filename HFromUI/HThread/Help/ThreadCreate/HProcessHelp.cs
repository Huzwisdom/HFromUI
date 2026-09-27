using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// Process 帮助类：操作系统进程（独立地址空间，不是线程！）。
    /// 【是什么】System.Diagnostics.Process 用于启动/停止本地系统进程、读取其退出码与输出、
    /// 枚举系统中正在运行的进程并读取模块/内存/性能信息。每个进程有独立地址空间与至少一个线程，
    /// 进程间相互隔离，一个崩溃不影响另一个。
    /// 【是否跨进程】本身就是"跨进程"的入口；本类所有示例都是启动或观察另一个独立进程。
    /// 进程间不能直接共享内存，通信要用管道/Socket/共享内存/文件/命令行参数。
    /// 【典型适用场景】
    /// 1) 调用外部 EXE/控制台命令并取退出码、标准输出/错误；
    /// 2) 隔离不稳定代码、利用多进程突破单进程内存/权限限制；
    /// 3) 监控外部进程退出（EnableRaisingEvents + Exited 事件）；
    /// 4) 枚举/查找本机进程（GetProcessById/GetProcesses/GetProcessesByName/GetCurrentProcess）。
    /// 【使用步骤】
    /// 1) 构造 ProcessStartInfo：FileName、Arguments、UseShellExecute、是否重定向流、CreateNoWindow、工作目录；
    /// 2) new Process { StartInfo = info }（要退出事件先置 EnableRaisingEvents=true 并挂 Exited）；
    /// 3) Start() 启动；必要时 BeginOutputReadLine/BeginErrorReadLine 异步收输出（必须在 Start 之后立刻调）；
    /// 4) WaitForExit(毫秒超时) 等待；超时则 Kill；正常退出读 ExitCode；
    /// 5) 用完 Close()/Dispose 释放进程句柄。
    /// 【注意事项与坑】
    /// - 重定向标准流必须 UseShellExecute=false；
    /// - 同时重定向 stdout 和 stderr 后若同步 ReadToEnd 两个流，子进程写满一个管道缓冲会死锁：
    ///   要么两个流分别在线程/任务上并发读，要么用 BeginOutputReadLine 异步读；
    /// - UseShellExecute=false 时 Verb/ShellExecute 特性不可用，路径/环境变量按 CreateNoWindow 规则走；
    /// - Kill 是强杀（被 kill 进程没有机会清理）；有窗口的程序优先 CloseMainWindow 协商退出；
    /// - Exited 事件在线程池线程触发，且必须 EnableRaisingEvents=true；回调内更新 UI 要自行封送；
    /// - 路径与参数属于外部输入，注意注入风险与引号转义；net48 只有 Arguments 字符串，没有 ArgumentList。
    /// 【版本可用性】net48 全量可用：Start(静态/实例)、StartInfo、Kill()、WaitForExit(2 个重载)、
    /// CloseMainWindow、StandardOutput/StandardError/StandardInput、BeginOutputReadLine/
    /// BeginErrorReadLine/OutputDataReceived/ErrorDataReceived、EnableRaisingEvents/Exited、
    /// GetProcessById/GetProcesses/GetProcessesByName/GetCurrentProcess、ExitCode/HasExited/Id/ProcessName。
    /// net48【没有】：ProcessStartInfo.ArgumentList（.NET Core 2.1+，自动转义参数集合）、
    /// Kill(bool entireProcessTree)（.NET Core 3.0+）、WaitForExitAsync（.NET 5+）。
    /// </summary>
    /// <example>
    /// 自测推荐使用秒退命令，例如 cmd /c echo（本类提供了 QuickExitFileName/QuickExitArguments 常量，
    /// 但所有启动方法都要求调用方显式传入文件名，帮助类自身不会自动启动任何外部进程）：
    /// <code>
    /// int code = HProcessHelp.StartAndWait(
    ///     HProcessHelp.QuickExitFileName, HProcessHelp.QuickExitArguments, 5000);
    /// </code>
    /// </example>
    public static class HProcessHelp
    {
        /// <summary>自测用秒退程序名（不主动启动，仅提供给调用方做安全测试）</summary>
        public const string QuickExitFileName = "cmd.exe";

        /// <summary>自测用秒退参数：打印一行后立刻退出</summary>
        public const string QuickExitArguments = "/c echo HFromUI";

        /// <summary>示例1：启动外部程序（不等待）；是否走 Shell 由 useShellExecute 决定</summary>
        /// <param name="fileName">要启动的程序/文件路径，由调用方提供</param>
        /// <param name="arguments">命令行参数字符串，可为 null</param>
        /// <param name="useShellExecute">true=走系统 Shell（可打开文档/URL、支持 Verb）；false=直接 CreateProcess</param>
        /// <returns>关联新进程的 Process 对象</returns>
        /// <exception cref="Win32Exception">文件不存在或启动失败时抛出</exception>
        public static Process Start(string fileName, string arguments = null, bool useShellExecute = true)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = useShellExecute
            };
            return Process.Start(startInfo);
        }

        /// <summary>示例2：只构造启动信息不启动（net48 用 Arguments 字符串；ArgumentList 是 Core 2.1+ 才有）</summary>
        /// <param name="fileName">程序路径</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="redirectOutput">是否重定向标准输出（自动配套 UseShellExecute=false）</param>
        /// <param name="createNoWindow">是否不创建新窗口（控制台程序常用）</param>
        /// <returns>配置好的 ProcessStartInfo</returns>
        public static ProcessStartInfo BuildStartInfo(string fileName, string arguments,
                                                      bool redirectOutput = false, bool createNoWindow = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = createNoWindow
            };
            if (redirectOutput)
            {
                // 重定向流必须不走系统 Shell（UseShellExecute 默认 true，仅在需要重定向时关掉）
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
            }
            return startInfo;
        }

        /// <summary>示例3：无窗口启动控制台程序（UseShellExecute=false、CreateNoWindow=true），不等待</summary>
        /// <param name="fileName">程序路径，由调用方提供</param>
        /// <param name="arguments">命令行参数，可为 null</param>
        /// <returns>已启动进程的 Process 对象</returns>
        public static Process StartNoWindow(string fileName, string arguments = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,     // 控制台程序直接 CreateProcess
                    CreateNoWindow = true
                }
            };
            process.Start();      // 实例 Start() 返回 bool；Process 对象在 Start 前就已 new 好
            return process;
        }

        /// <summary>示例4：启动并等待退出（可设超时），返回退出码；超时强杀并返回 -1</summary>
        /// <param name="fileName">程序路径，由调用方提供</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="timeoutMs">等待超时毫秒</param>
        /// <returns>进程退出码；超时被杀返回 -1</returns>
        public static int StartAndWait(string fileName, string arguments, int timeoutMs)
        {
            using (Process process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            }))
            {
                if (process.WaitForExit(timeoutMs))
                {
                    return process.ExitCode;      // 正常退出取退出码
                }
                process.Kill();                   // 超时强杀
                process.WaitForExit(1000);        // 给强杀留 1 秒收尾，避免僵尸句柄
                return -1;
            }
        }

        /// <summary>示例5：捕获控制台程序标准输出（UseShellExecute=false、RedirectStandardOutput=true），带超时</summary>
        /// <param name="fileName">程序路径，由调用方提供</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="timeoutMs">等待超时毫秒，默认 5000</param>
        /// <returns>标准输出全文；超时则返回已读到的部分内容</returns>
        public static string StartAndReadOutput(string fileName, string arguments, int timeoutMs = 5000)
        {
            ProcessStartInfo startInfo = BuildStartInfo(fileName, arguments, true, true);
            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();  // 本示例未重定向 stderr，不存在双管道死锁
                if (!process.WaitForExit(timeoutMs))
                {
                    process.Kill();
                    process.WaitForExit(1000);
                }
                return output;
            }
        }

        /// <summary>示例6：同时捕获 stdout/stderr：两个流分别用任务并发读取，规避同步双读的管道死锁</summary>
        /// <param name="fileName">程序路径，由调用方提供</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="timeoutMs">等待超时毫秒</param>
        /// <returns>含退出码、两路输出与超时标志的 ProcessOutput</returns>
        public static ProcessOutput StartAndReadBoth(string fileName, string arguments, int timeoutMs)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,    // 同时重定向两个流
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startInfo))
            {
                // 两个流并发读到 EOF，任何一个写满管道缓冲都不会卡住对方
                Task<string> outTask = Task.Run(delegate () { return process.StandardOutput.ReadToEnd(); });
                Task<string> errTask = Task.Run(delegate () { return process.StandardError.ReadToEnd(); });
                bool exited = process.WaitForExit(timeoutMs);
                if (!exited)
                {
                    process.Kill();              // 超时强杀，杀后管道关闭，读任务自然结束
                    process.WaitForExit(1000);
                }
                outTask.Wait(1000);
                errTask.Wait(1000);
                return new ProcessOutput
                {
                    TimedOut = !exited,
                    ExitCode = exited ? process.ExitCode : -1,
                    StdOut = outTask.IsCompleted ? outTask.Result : string.Empty,
                    StdErr = errTask.IsCompleted ? errTask.Result : string.Empty
                };
            }
        }

        /// <summary>示例7：异步事件式读取输出（OutputDataReceived + BeginOutputReadLine），适合长进程实时输出</summary>
        /// <param name="fileName">程序路径，由调用方提供</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="onOutputLine">每收到一行输出的回调（在线程池线程触发，更新 UI 需自行封送）</param>
        /// <param name="timeoutMs">等待超时毫秒</param>
        /// <returns>true=进程正常退出；false=超时被杀</returns>
        public static bool StartWithOutputEvents(string fileName, string arguments,
                                                 Action<string> onOutputLine, int timeoutMs)
        {
            Process process = new Process
            {
                StartInfo = BuildStartInfo(fileName, arguments, true, true),
                EnableRaisingEvents = true
            };
            DataReceivedEventHandler handler = delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    onOutputLine(e.Data);        // e.Data 为 null 表示流结束
                }
            };
            process.OutputDataReceived += handler;
            process.Start();
            process.BeginOutputReadLine();       // 必须在 Start 之后立即调用
            bool exited = process.WaitForExit(timeoutMs);
            if (!exited)
            {
                process.Kill();
                process.WaitForExit(1000);
            }
            process.CancelOutputRead();
            process.OutputDataReceived -= handler;
            process.Close();
            return exited;
        }

        /// <summary>示例8：订阅 Exited 事件：进程退出时由线程池回调（必须 EnableRaisingEvents=true）</summary>
        /// <param name="fileName">程序路径，由调用方提供</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="onExited">进程退出回调（线程池线程；可在此读 ExitCode）</param>
        /// <param name="timeoutMs">兜底等待超时毫秒（事件外的双保险）</param>
        /// <returns>true=超时时间内退出；false=超时</returns>
        public static bool StartAndObserve(string fileName, string arguments,
                                           Action onExited, int timeoutMs)
        {
            Process process = new Process
            {
                StartInfo = BuildStartInfo(fileName, arguments, false, true),
                EnableRaisingEvents = true       // 不打开这个开关，Exited 永远不会触发
            };
            EventHandler exitedHandler = delegate (object sender, EventArgs e)
            {
                onExited();
            };
            process.Exited += exitedHandler;
            process.Start();
            bool exited = process.WaitForExit(timeoutMs);
            process.Exited -= exitedHandler;     // 解除事件，避免回调持有外部对象
            process.Close();
            return exited;
        }

        /// <summary>示例9：重定向标准输入：向子进程写入数据后必须关闭输入流，等待 EOF 的子进程才会退出</summary>
        /// <param name="fileName">程序路径，由调用方提供（如 sort/find 等读 stdin 的控制台程序）</param>
        /// <param name="arguments">命令行参数</param>
        /// <param name="input">写入子进程标准输入的文本</param>
        /// <param name="timeoutMs">等待超时毫秒</param>
        /// <returns>子进程标准输出全文；超时返回已读到的部分</returns>
        public static string WriteStandardInput(string fileName, string arguments,
                                                string input, int timeoutMs)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startInfo))
            {
                process.StandardInput.Write(input);
                process.StandardInput.Close();   // 关键：不关闭子进程可能一直等输入而不退出
                string output = process.StandardOutput.ReadToEnd();
                if (!process.WaitForExit(timeoutMs))
                {
                    process.Kill();
                    process.WaitForExit(1000);
                }
                return output;
            }
        }

        /// <summary>示例10：协商式关闭：先 CloseMainWindow 发关闭消息（仅 GUI 程序有效），不接受再考虑 Kill</summary>
        /// <param name="process">目标进程</param>
        /// <param name="timeoutMs">协商等待超时毫秒</param>
        /// <returns>true=进程已退出；false=没有主窗口或超时未退出（调用方可接着调 Terminate）</returns>
        public static bool CloseMainWindowGraceful(Process process, int timeoutMs)
        {
            if (process.HasExited)
            {
                return true;
            }
            if (!process.CloseMainWindow())       // 控制台程序/无窗口进程返回 false
            {
                return false;
            }
            return process.WaitForExit(timeoutMs);
        }

        /// <summary>示例11：强杀进程并等待最多 1 秒收尾，随后释放句柄</summary>
        /// <param name="process">目标进程</param>
        public static void Terminate(Process process)
        {
            if (!process.HasExited)
            {
                process.Kill();                   // net48 只有无参 Kill；Kill(bool 杀进程树) 是 Core 3.0+
                process.WaitForExit(1000);
            }
            process.Close();
        }

        /// <summary>示例12：按 PID 查找进程并读取常用只读信息（GetProcessById）</summary>
        /// <param name="processId">操作系统进程 Id</param>
        /// <returns>Id/ProcessName/HasExited 等拼成的描述字符串</returns>
        /// <exception cref="ArgumentException">指定 Id 的进程不存在时抛出</exception>
        public static string DescribeProcess(int processId)
        {
            using (Process process = Process.GetProcessById(processId))
            {
                return "Id=" + process.Id
                    + ", ProcessName=" + process.ProcessName
                    + ", HasExited=" + process.HasExited
                    + ", WorkingSet64=" + process.WorkingSet64;
            }
        }

        /// <summary>示例13：枚举本机全部进程（GetProcesses），返回进程名快照</summary>
        /// <param name="max">最多返回的条数（0 表示不限制）</param>
        /// <returns>进程名数组</returns>
        public static string[] GetProcessNames(int max = 0)
        {
            Process[] all = Process.GetProcesses();
            List<string> names = new List<string>(all.Length);
            try
            {
                for (int i = 0; i < all.Length; i++)
                {
                    if (max > 0 && names.Count >= max)
                    {
                        break;
                    }
                    names.Add(all[i].ProcessName);
                }
            }
            finally
            {
                foreach (Process process in all)
                {
                    process.Dispose();            // Process 持有的内核句柄要及时释放
                }
            }
            return names.ToArray();
        }

        /// <summary>示例14：按名字统计进程数量（GetProcessesByName，不含 .exe 后缀）</summary>
        /// <param name="processName">进程友好名（不带 .exe）</param>
        /// <returns>同名进程数量</returns>
        public static int CountByName(string processName)
        {
            Process[] matches = Process.GetProcessesByName(processName);
            foreach (Process process in matches)
            {
                process.Dispose();
            }
            return matches.Length;
        }

        /// <summary>示例15：当前进程的 Id（GetCurrentProcess）</summary>
        /// <returns>当前进程 Id</returns>
        public static int CurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        /// <summary>示例16：当前进程名（GetCurrentProcess）</summary>
        /// <returns>当前进程友好名（不带 .exe）</returns>
        public static string CurrentProcessName()
        {
            return Process.GetCurrentProcess().ProcessName;
        }

        /// <summary>
        /// 进程输出结果容器：StartAndReadBoth 的返回值。
        /// </summary>
        public sealed class ProcessOutput
        {
            /// <summary>是否因超时被强杀（true=超时，未拿到真实退出码）</summary>
            public bool TimedOut;

            /// <summary>进程退出码；超时时为 -1</summary>
            public int ExitCode;

            /// <summary>标准输出全文</summary>
            public string StdOut;

            /// <summary>标准错误全文</summary>
            public string StdErr;
        }
    }
}
