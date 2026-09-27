using System;
using System.ComponentModel;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    using HFromUI.HLangage;
    /// <summary>
    /// BackgroundWorker 帮助类：老式异步组件（System.ComponentModel，.NET Framework 内置）。
    /// 【是什么】BackgroundWorker 是一个可在组件/窗体上托管的后台执行器：RunWorkerAsync 在线程池线程
    /// 触发 DoWork；ReportProgress 触发 ProgressChanged；后台结束（正常/取消/异常）触发
    /// RunWorkerCompleted。后两个事件通过捕获的 SynchronizationContext（AsyncOperation）自动送回
    /// RunWorkerAsync 调用时所在的上下文（UI 程序里就是 UI 线程）。
    /// 【是否跨进程】否。DoWork 在本进程线程池线程上执行。
    /// 【典型适用场景】
    /// 1) WinForms/WPF 老式数据加载、文件拷贝、耗时计算，且需要进度条/百分比与取消按钮；
    /// 2) 不想手写 Task + IProgress&lt;T&gt; + UI 封送的旧项目维护。
    /// 新项目建议 Task + IProgress&lt;T&gt; / async-await。
    /// 【使用步骤】
    /// 1) new BackgroundWorker()，按需置 WorkerReportsProgress=true、WorkerSupportsCancellation=true；
    /// 2) 订阅 DoWork（后台）、ProgressChanged（UI）、RunWorkerCompleted（UI）三个事件；
    /// 3) 在 UI 线程调 RunWorkerAsync()（可带参数，DoWork 里用 e.Argument 取）；
    /// 4) DoWork 中检查 worker.CancellationPending、调 worker.ReportProgress(percent, userState)，
    ///    结果赋 e.Result；取消则 e.Cancel=true；
    /// 5) IsBusy 期间不能再次 RunWorkerAsync（抛 InvalidOperationException），用 IsBusy 守门。
    /// 【注意事项与坑】
    /// - ReportProgress 要求 WorkerReportsProgress=true，否则抛 InvalidOperationException；
    /// - CancelAsync 只是置 CancellationPending，不会强杀线程，DoWork 必须自己检查并退出；
    /// - RunWorkerCompleted 里只有 e.Error==null 且 !e.Cancelled 时才能读 e.Result，否则读 Result 抛异常；
    /// - DoWork 内不要碰 UI 控件；事件回调在 UI 线程才能碰控件（依赖 UI 同步上下文）；
    /// - 没有 UI 同步上下文的环境（控制台/单元测试）事件会在线程池线程触发，逻辑仍可运行但不在"UI 线程"。
    /// 【版本可用性】.NET Framework 2.0 起内置（System.dll），net48 全量可用；
    /// .NET Core/.NET 5+ 的桌面 SDK 中通过 System.ComponentModel.EventBasedAsync 提供。
    /// </summary>
    public static class HBackgroundWorkerHelp
    {
        /// <summary>示例1：创建并配置一个带进度、可取消的后台组件（三个事件全部挂好）</summary>
        /// <returns>配置完成但尚未启动的 BackgroundWorker</returns>
        public static BackgroundWorker CreateWorker()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;       // 允许 ReportProgress
            worker.WorkerSupportsCancellation = true;  // 允许 CancelAsync
            worker.DoWork += DoWorkHandler;
            worker.ProgressChanged += ProgressChangedHandler;
            worker.RunWorkerCompleted += CompletedHandler;
            return worker;
        }

        /// <summary>示例2：创建最简后台组件（不报告进度、不支持取消，只挂 DoWork）</summary>
        /// <returns>只执行一次后台工作的 BackgroundWorker</returns>
        public static BackgroundWorker CreateSimpleWorker()
        {
            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            worker.DoWork += DoWorkHandler;
            return worker;
        }

        /// <summary>示例3：后台执行体：耗时循环中检查取消、按百分比报告进度，结果放 e.Result</summary>
        /// <param name="sender">BackgroundWorker 实例</param>
        /// <param name="e">事件参数，Argument 为入参，Result 为出参，Cancel 标记取消</param>
        public static void DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            for (int i = 0; i <= 100; i++)
            {
                if (worker.CancellationPending)       // 协作式取消检查
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(10);
                worker.ReportProgress(i);             // 触发 UI 线程上的 ProgressChanged
            }
            e.Result = HTranslation.GetContent("完成");                        // 结果交给 RunWorkerCompleted
        }

        /// <summary>示例4：带自定义状态对象的后台执行体：ReportProgress 第二重载传 userState（如当前文件名）</summary>
        /// <param name="sender">BackgroundWorker 实例</param>
        /// <param name="e">事件参数</param>
        public static void DoWorkWithStateHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string[] files = new string[] { "a.txt", "b.txt", "c.txt" };
            for (int i = 0; i < files.Length; i++)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                Thread.Sleep(5);
                int percent = (i + 1) * 100 / files.Length;
                worker.ReportProgress(percent, files[i]);   // ProgressChangedEventArgs.UserState 可取到
            }
            e.Result = files.Length;
        }

        /// <summary>进度事件（业务方可订阅；e.ProgressPercentage 为百分比，e.UserState 为可选状态）</summary>
        public static event ProgressChangedEventHandler Progress;

        /// <summary>进度事件处理器：默认转发到静态 Progress 事件</summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">进度事件参数</param>
        public static void ProgressChangedHandler(object sender, ProgressChangedEventArgs e)
        {
            Progress?.Invoke(sender, e);              // e.ProgressPercentage 即百分比
        }

        /// <summary>完成事件（业务方可订阅；先判 Cancelled/Error 再读 Result）</summary>
        public static event RunWorkerCompletedEventHandler Completed;

        /// <summary>完成事件处理器：区分正常完成/取消/异常三种情况后转发到静态 Completed 事件</summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">完成事件参数</param>
        public static void CompletedHandler(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // 用户取消
            }
            else if (e.Error != null)
            {
                // 后台异常在 e.Error；此分支不要读 e.Result（会抛 InvalidOperationException）
            }
            else
            {
                object result = e.Result;             // 只有成功完成时读取才安全
            }
            Completed?.Invoke(sender, e);
        }

        /// <summary>示例5：无参启动（对应 RunWorkerAsync() 重载，e.Argument 为 null）</summary>
        /// <param name="worker">目标组件</param>
        /// <exception cref="InvalidOperationException">worker 正忙（IsBusy=true）时调用抛出</exception>
        public static void Start(BackgroundWorker worker)
        {
            worker.RunWorkerAsync();
        }

        /// <summary>示例6：IsBusy 守门启动：忙碌时跳过而不是抛异常</summary>
        /// <param name="worker">目标组件</param>
        /// <param name="argument">传给 DoWork 的参数（e.Argument），可为 null</param>
        /// <returns>true=已启动；false=组件正忙，本次跳过</returns>
        public static bool RunIfIdle(BackgroundWorker worker, object argument = null)
        {
            if (worker.IsBusy)
            {
                return false;                         // 上一次后台工作尚未结束
            }
            worker.RunWorkerAsync(argument);
            return true;
        }

        /// <summary>示例7：有界等待后台工作结束（自测/控制台无消息循环时用，避免无限等待）</summary>
        /// <param name="worker">目标组件</param>
        /// <param name="timeoutMs">超时毫秒</param>
        /// <returns>true=已空闲；false=超时仍在运行</returns>
        public static bool WaitIdle(BackgroundWorker worker, int timeoutMs)
        {
            int waited = 0;
            while (worker.IsBusy && waited < timeoutMs)
            {
                Thread.Sleep(10);
                waited += 10;
            }
            return !worker.IsBusy;
        }

        /// <summary>示例8：读取组件最常用的开关/状态属性</summary>
        /// <param name="worker">目标组件</param>
        /// <returns>属性拼成的可读字符串</returns>
        public static string Describe(BackgroundWorker worker)
        {
            return "IsBusy=" + worker.IsBusy
                + ", WorkerReportsProgress=" + worker.WorkerReportsProgress
                + ", WorkerSupportsCancellation=" + worker.WorkerSupportsCancellation
                + ", CancellationPending=" + worker.CancellationPending;
        }

        /// <summary>示例9：启动与取消的标准写法（启动后立即请求取消，是否真正退出由 DoWork 循环检查决定）</summary>
        /// <param name="worker">目标组件（需 WorkerSupportsCancellation=true）</param>
        public static void StartAndCancel(BackgroundWorker worker)
        {
            worker.RunWorkerAsync(HTranslation.GetContent("可选参数"));        // 参数在 DoWorkEventArgs.Argument
            // 需要中止时（不强杀，仅置取消请求）：
            worker.CancelAsync();
        }
    }
}
