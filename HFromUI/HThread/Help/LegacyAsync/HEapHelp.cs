using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace HFromUI.HThread.Help.LegacyAsync
{
    using HFromUI.HLangage;
    /// <summary>
    /// EAP（Event-based Asynchronous Pattern，基于事件的异步模式）帮助类（.NET 2.0 时代模式）。
    /// 【是什么】EAP 用"XxxAsync 启动方法 + XxxCompleted 完成事件 + ProgressChanged 进度事件 + CancelAsync 取消"
    /// 表达异步：组件在发起线程上捕获同步上下文（内部靠 AsyncOperationManager/AsyncOperation），
    /// 后台线程完成/报进度时自动 Post 回发起线程——在 UI 线程发起则回调直接在 UI 线程执行，可直接更新控件。
    /// 完成事件参数派生于 AsyncCompletedEventArgs，四个关键成员：Error（后台异常，不崩进程）、
    /// Cancelled（是否被取消）、UserState（启动时传入的任务标识）、protected RaiseExceptionIfNecessary()
    /// （派生类的 Result 属性访问前调用：出错抛原异常、取消抛 InvalidOperationException）；
    /// 进度参数 ProgressChangedEventArgs 提供 ProgressPercentage(0-100) 与 UserState。
    /// 代表实现：BackgroundWorker（DoWork/ProgressChanged/RunWorkerCompleted，EAP 教科书）、
    /// WebClient（DownloadStringAsync/DownloadStringCompleted/DownloadProgressChanged/CancelAsync/IsBusy）。
    /// 【是否跨进程】否；WebClient 等组件访问网络是跨机器通信，但模式本身只是进程内的事件封送。
    /// 【典型适用场景】阅读/维护老式 EAP 组件；给库写"UI 线程自动回调"的异步组件（今天更推荐 Task+IProgress&lt;T&gt;）。
    /// 【使用步骤】
    /// 1) 实例化组件，订阅 XxxCompleted（取 Result/判断 Error/Cancelled）与 ProgressChanged；
    /// 2) 调 XxxAsync(参数, userState) 启动（可带任务 Id）；3) 需要时 CancelAsync()（协作式，仅置取消请求）；
    /// 4) 自定义组件按本类 SimulatedEapWorker 骨架：AsyncOperationManager.CreateOperation 捕获上下文，
    ///    后台线程用 AsyncOperation.Post 报进度、PostOperationCompleted 发完成事件。
    /// 【注意事项与坑】
    /// - 完成事件里必须先看 e.Cancelled/e.Error，最后才访问 Result（Result 内部 RaiseExceptionIfNecessary）；
    /// - CancelAsync 只是"请求"，工作体必须协作检查并退出；组件通常用 IsBusy 拒绝重复启动；
    /// - 后台异常经 e.Error 带出，不会终止进程；
    /// - WebClient 在 net48 已被标记 Obsolete（推荐 HttpClient），且真实下载会访问网络——本帮助类不写可编译的联网代码，
    ///   WebClient 写法仅放在 &lt;example&gt; 注释中演示；可编译代码用自造事件 SimulatedEapWorker 演示 EAP 三要素；
    /// - AsyncOperation.Post 在没有同步上下文的线程（控制台/线程池）上会投递到线程池，而非"回原线程"，属正常；
    /// - 现代替代：await 承担完成通知，IProgress&lt;T&gt;/Progress&lt;T&gt; 承担进度——Progress&lt;T&gt; 构造时捕获
    ///   当前 SynchronizationContext，Report 自动 Post 回该上下文（UI 线程）。
    /// 【版本可用性】net48：AsyncOperationManager/AsyncOperation、AsyncCompletedEventArgs、ProgressChangedEventArgs、
    /// BackgroundWorker、WebClient、Progress&lt;T&gt; 全部可用；.NET Core/.NET 5+ 保留 BackgroundWorker（WinForms 程序集）
    /// 与 Progress&lt;T&gt;，WebClient 仍存在但不推荐。
    /// </summary>
    ///
    /// <example>
    /// 【WebClient EAP 标准写法——仅文档示例，本工程不实际访问 URL】
    /// <code>
    /// WebClient client = new WebClient();
    /// client.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs e)
    /// {
    ///     // ProgressPercentage 0-100；在发起线程（UI线程）触发
    ///     progressBar1.Value = e.ProgressPercentage;
    /// };
    /// client.DownloadStringCompleted += delegate(object sender, DownloadStringCompletedEventArgs e)
    /// {
    ///     if (e.Cancelled) { /* 用户取消*/ return; }
    ///     if (e.Error != null) { ShowError(e.Error); return; }   // 后台异常经事件带出
    ///     string html = e.Result;                                // 成功：UI线程可直接赋值控件
    /// };
    /// client.DownloadStringAsync(new Uri("https://example.com"), "任务Id-001");  // userState 原样进 e.UserState
    /// // client.IsBusy 为 true 期间不应再次启动；client.CancelAsync() 协作取消
    /// </code>
    /// </example>
    public static class HEapHelp
    {
        /// <summary>
        /// 示例1：现代替代——通过 IProgress&lt;T&gt; 上报一次进度（固定 50%）。
        /// </summary>
        /// <param name="progress">进度接收器；null 时本方法不做任何事</param>
        public static void ModernProgress(IProgress<int> progress)
        {
            if (progress != null)
            {
                progress.Report(50);   // 构造时捕获同步上下文，Report自动回UI线程
            }
        }

        /// <summary>
        /// 示例2：现代替代——上报任意进度值。
        /// </summary>
        /// <param name="progress">进度接收器；null 时忽略</param>
        /// <param name="percent">进度百分比（约定 0-100）</param>
        public static void Report(IProgress<int> progress, int percent)
        {
            if (progress != null)
            {
                progress.Report(percent);
            }
        }

        /// <summary>
        /// 示例3：创建 Progress&lt;int&gt;：构造瞬间捕获当前 SynchronizationContext，
        /// 之后任意线程 Report 都会 Post 回构造它的上下文（UI 线程上构造即回 UI 线程）。
        /// </summary>
        /// <param name="handler">每个进度值的处理（在捕获的上下文上执行）</param>
        /// <returns>可传给后台方法的 IProgress&lt;int&gt;</returns>
        /// <exception cref="ArgumentNullException">handler 为 null</exception>
        public static IProgress<int> CreateProgress(Action<int> handler)
        {
            // 必须在 UI 线程调用构造，才能把上下文捕获为 UI 上下文
            return new Progress<int>(handler);
        }

        /// <summary>
        /// 示例4：创建并装配一个标准 BackgroundWorker（EAP 代表）：开启进度上报与取消支持、挂好三类事件，
        /// 但【不调用 RunWorkerAsync】——只演示形状，创建/订阅不触发任何后台执行、不依赖消息循环。
        /// </summary>
        /// <param name="work">DoWork 工作体（后台线程执行；可用 sender 调 ReportProgress、检查 CancellationPending）</param>
        /// <param name="onProgress">进度回调（发起线程执行）</param>
        /// <param name="onCompleted">完成回调（成功/失败/取消三态都走这里，先查 Cancelled/Error 再取 Result）</param>
        /// <returns>装配完毕、尚未启动的 BackgroundWorker（调用方 RunWorkerAsync 启动）</returns>
        /// <exception cref="ArgumentNullException">work/onProgress/onCompleted 为 null</exception>
        public static BackgroundWorker CreateBackgroundWorkerShape(
            Action<BackgroundWorker, DoWorkEventArgs> work,
            Action<ProgressChangedEventArgs> onProgress,
            Action<RunWorkerCompletedEventArgs> onCompleted)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;          // 允许 ReportProgress
            worker.WorkerSupportsCancellation = true;     // 允许 CancelAsync（工作体需查 CancellationPending）
            worker.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                work((BackgroundWorker)sender, e);        // 后台线程：sender 即 worker 自身
            };
            worker.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                onProgress(e);                            // 自动封送回发起线程
            };
            worker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                onCompleted(e);                           // e.Error/e.Cancelled/e.Result 三态
            };
            return worker;                                // 不 RunWorkerAsync：安全可自测
        }

        /// <summary>
        /// EAP 三要素的统一形状接口（自定义 EAP 组件时遵循：Async 后缀启动方法 + Completed 事件 + CancelAsync）。
        /// </summary>
        /// <typeparam name="TResult">异步操作结果类型</typeparam>
        public interface IAsyncPattern<TResult>
        {
            /// <summary>
            /// 是否有进行中的操作（进行中再次启动通常抛 InvalidOperationException）。
            /// </summary>
            /// <returns>true=忙碌中</returns>
            bool IsBusy { get; }

            /// <summary>
            /// 启动异步操作。
            /// </summary>
            /// <param name="argument">业务入参</param>
            /// <param name="taskId">任务标识（原样进完成事件参数的 UserState）</param>
            void DoWorkAsync(object argument, object taskId);

            /// <summary>
            /// 完成事件：参数携带 Result/Error/Cancelled/UserState。
            /// </summary>
            event EventHandler<CompletedArgs<TResult>> DoWorkCompleted;

            /// <summary>
            /// 请求取消（协作式：仅发信号，工作体自行响应）。
            /// </summary>
            void CancelAsync();
        }

        /// <summary>
        /// EAP 完成事件参数标准形状（对应 AsyncCompletedEventArgs，演示派生类如何加 Result）。
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        public class CompletedArgs<TResult> : AsyncCompletedEventArgs
        {
            private readonly TResult result;

            /// <summary>
            /// 构造完成参数。
            /// </summary>
            /// <param name="result">成功结果（失败/取消时传默认值）</param>
            /// <param name="error">后台异常；无则 null</param>
            /// <param name="cancelled">是否被取消</param>
            /// <param name="state">任务标识（对应 UserState）</param>
            public CompletedArgs(TResult result, Exception error, bool cancelled, object state)
                : base(error, cancelled, state)
            {
                this.result = result;
            }

            /// <summary>
            /// 操作结果；出错或被取消时访问会抛异常（基类 RaiseExceptionIfNecessary 的 EAP 标准约定）。
            /// </summary>
            /// <returns>成功结果</returns>
            /// <exception cref="InvalidOperationException">操作被取消时访问</exception>
            /// <exception cref="Exception">后台发生的原异常被重新抛出</exception>
            public TResult Result
            {
                get
                {
                    RaiseExceptionIfNecessary();   // 出错抛原异常，取消抛 InvalidOperationException
                    return result;
                }
            }
        }

        /// <summary>
        /// 自造 EAP 组件（不联网、可编译、可自测）：用 AsyncOperationManager 完整演示
        /// "XxxAsync + XxxCompleted + ProgressChanged + CancelAsync + IsBusy" 四件套与上下文封送。
        /// </summary>
        /// <typeparam name="TResult">工作结果类型</typeparam>
        public sealed class SimulatedEapWorker<TResult> : IAsyncPattern<TResult>, IDisposable
        {
            // 工作体委托：入参=业务参数、进度上报委托(0-100)、取消令牌；返回结果
            private readonly Func<object, Action<int>, CancellationToken, TResult> worker;
            private readonly object cancelGate = new object();
            private CancellationTokenSource cts;
            private int busy;

            /// <summary>
            /// 用可注入的工作体构造 EAP 组件（不做任何真实 IO/网络）。
            /// </summary>
            /// <param name="worker">工作体：object=DoWorkAsync 的 argument，Action&lt;int&gt;=进度上报，
            /// CancellationToken=取消令牌；检测到取消应抛 OperationCanceledException</param>
            /// <exception cref="ArgumentNullException">worker 为 null</exception>
            public SimulatedEapWorker(Func<object, Action<int>, CancellationToken, TResult> worker)
            {
                if (worker == null)
                {
                    throw new ArgumentNullException(HTranslation.GetContent("工作线程不能为空"));
                }
                this.worker = worker;
            }

            /// <summary>
            /// 是否有进行中的操作。
            /// </summary>
            /// <returns>true=已有操作在跑</returns>
            public bool IsBusy
            {
                // 原子读取忙标志
                get { return Interlocked.CompareExchange(ref busy, 0, 0) == 1; }
            }

            /// <summary>
            /// 进度事件（参数 ProgressPercentage 0-100；自动封送回发起线程）。
            /// </summary>
            public event ProgressChangedEventHandler ProgressChanged;

            /// <summary>
            /// 完成事件（成功/失败/取消均只触发一次）。
            /// </summary>
            public event EventHandler<CompletedArgs<TResult>> DoWorkCompleted;

            /// <summary>
            /// 启动异步操作：后台线程执行工作体，进度/完成事件经 AsyncOperation 封送回发起线程。
            /// </summary>
            /// <param name="argument">业务入参（原样给工作体）</param>
            /// <param name="taskId">任务标识（进 ProgressChanged/Completed 的 UserState）</param>
            /// <exception cref="InvalidOperationException">上一个操作尚未完成</exception>
            public void DoWorkAsync(object argument, object taskId)
            {
                // EAP 约定：同一组件一个 taskId 同时只允许一个进行中的操作
                if (Interlocked.Exchange(ref busy, 1) == 1)
                {
                    throw new InvalidOperationException(HTranslation.GetContent("该 EAP 组件已有进行中的操作，请等待完成后再启动"));
                }
                CancellationTokenSource source = new CancellationTokenSource();
                lock (cancelGate)
                {
                    cts = source;                       // 供 CancelAsync 使用
                }
                // 关键：在发起线程创建 AsyncOperation，捕获其 SynchronizationContext（UI 线程即捕获 UI 上下文）
                AsyncOperation operation = AsyncOperationManager.CreateOperation(taskId);
                ThreadPool.QueueUserWorkItem(delegate (object unused)
                {
                    Exception error = null;
                    TResult result = default(TResult);
                    bool cancelled = false;
                    try
                    {
                        result = worker(argument, delegate (int percent)
                        {
                            // Post：异步封送进度事件回发起线程（不等执行完）
                            operation.Post(delegate (object s)
                            {
                                OnProgressChanged(new ProgressChangedEventArgs(percent, taskId));
                            }, null);
                        }, source.Token);
                        cancelled = source.IsCancellationRequested;
                    }
                    catch (OperationCanceledException)
                    {
                        cancelled = true;               // 协作取消的标准出口
                    }
                    catch (Exception ex)
                    {
                        error = ex;                     // 异常不吞也不崩进程，交给完成事件参数
                    }
                    finally
                    {
                        CompletedArgs<TResult> args = new CompletedArgs<TResult>(result, error, cancelled, taskId);
                        // PostOperationCompleted：封送完成事件并终结 AsyncOperation（一生只能调一次）
                        operation.PostOperationCompleted(delegate (object s)
                        {
                            OnDoWorkCompleted(args);
                        }, null);
                        Interlocked.Exchange(ref busy, 0);
                        // 注意：此处不能 Dispose(source)——完成后用户仍可能调用 CancelAsync，
                        // 令牌源统一在 Dispose() 中释放
                    }
                });
            }

            /// <summary>
            /// 请求取消（协作式：只 Cancel 令牌，不等待工作体结束）。
            /// </summary>
            public void CancelAsync()
            {
                CancellationTokenSource current = null;
                lock (cancelGate)
                {
                    current = cts;
                }
                if (current != null)
                {
                    current.Cancel();                   // 工作体是否响应取决于其是否检查 token
                }
            }

            /// <summary>
            /// 同步执行路径（自测/演示专用）：不投递线程池，在当前线程直接跑工作体并引发进度/完成事件，
            /// 对应 EAP 中"同步完成（CompletedSynchronously）"分支；结果确定、不依赖消息循环。
            /// </summary>
            /// <param name="argument">业务入参</param>
            /// <returns>成功时返回工作体结果；失败/取消时返回类型默认值（详情见完成事件参数）</returns>
            public TResult RunSynchronouslyForDemo(object argument)
            {
                Exception error = null;
                TResult result = default(TResult);
                bool cancelled = false;
                try
                {
                    result = worker(argument, delegate (int percent)
                    {
                        // 同步路径：事件在当前线程内联引发
                        OnProgressChanged(new ProgressChangedEventArgs(percent, null));
                    }, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                CompletedArgs<TResult> args = new CompletedArgs<TResult>(result, error, cancelled, null);
                OnDoWorkCompleted(args);
                return error == null && !cancelled ? result : default(TResult);
            }

            /// <summary>
            /// 释放内部取消令牌源。
            /// </summary>
            public void Dispose()
            {
                CancellationTokenSource current = null;
                lock (cancelGate)
                {
                    current = cts;
                    cts = null;
                }
                if (current != null)
                {
                    current.Dispose();
                }
            }

            private void OnProgressChanged(ProgressChangedEventArgs e)
            {
                // 事件为 null 时无订阅者，直接跳过
                ProgressChangedEventHandler handler = ProgressChanged;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            private void OnDoWorkCompleted(CompletedArgs<TResult> e)
            {
                EventHandler<CompletedArgs<TResult>> handler = DoWorkCompleted;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        /// <summary>
        /// 示例5：用自造事件在当前线程确定性地演示 EAP 三要素（启动→两次 ProgressChanged→Completed），
        /// 不访问网络、不等待线程池，可安全自测。
        /// </summary>
        /// <returns>按发生顺序拼接的事件日志，形如 "进度=50|进度=100|完成 Result=结果(X)"</returns>
        public static string SimulateThreeElementsInline()
        {
            List<string> log = new List<string>();
            SimulatedEapWorker<string> worker = new SimulatedEapWorker<string>(
                delegate (object argument, Action<int> report, CancellationToken token)
                {
                    // 注入的"假下载"工作体：只报进度、拼结果，完全不联网
                    report(50);
                    report(100);
                    return HTranslation.GetContent("结果(") + argument + ")";
                });
            worker.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                log.Add(HTranslation.GetContent("进度=") + e.ProgressPercentage);   // 要素①：ProgressChanged
            };
            worker.DoWorkCompleted += delegate (object sender, CompletedArgs<string> e)
            {
                // 要素②：XxxCompleted；要素③：三态判断（此处演示成功分支）
                string body = e.Cancelled
                    ? HTranslation.GetContent("已取消")
                    : (e.Error != null ? HTranslation.GetContent("失败:") + e.Error.GetType().Name : "Result=" + e.Result);
                log.Add(HTranslation.GetContent("完成 ") + body);
            };
            string value = worker.RunSynchronouslyForDemo("X");  // 要素④：XxxAsync 的同步等价启动
            worker.Dispose();
            return string.Join("|", log.ToArray()) + " => " + value;
        }
    }
}
