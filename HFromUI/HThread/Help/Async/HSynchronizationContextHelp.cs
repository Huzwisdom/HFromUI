using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="SynchronizationContext"/> 帮助类：同步上下文是“把工作单元投递到某个确定执行位置”
    /// 的抽象基类。核心成员：
    /// - Current（静态）：当前线程安装的上下文，没有则为 null；SetSynchronizationContext 设置/还原；
    /// - Post(SendOrPostCallback, state)：【异步】投递，调用方不等待（UI 上下文即塞消息队列）；
    /// - Send(...)：【同步】投递，阻塞调用线程直到目标位置执行完；
    /// - OperationStarted/OperationCompleted：成对通知“一个异步操作开始/结束”（宿主计数用，必须配对）；
    /// - CreateCopy：跨线程传播时复制上下文；可重写 Wait 拦截 WaitAll/WaitAny（少用）。
    /// await 捕获原理：await 挂起前读取 SynchronizationContext.Current 并存入状态机；任务完成后，
    /// 若 ConfigureAwait(true) 则把续体 Post 回该上下文执行；上下文为 null 时续体直接走线程池。
    ///
    /// 【是否跨进程】否（上下文只在本进程内调度线程/消息循环）。
    ///
    /// 【各框架实现（记住差异就理解了 await 行为）】
    /// - WinForms：System.Windows.Forms.WindowsFormsSynchronizationContext，Post=封送到控件消息循环
    ///   （Control.BeginInvoke 语义），只在 UI 线程执行；创建首个控件时自动安装到该线程；
    /// - WPF：System.Windows.Threading.DispatcherSynchronizationContext，Post=Dispatcher.BeginInvoke；
    /// - ASP.NET 经典（System.Web）：AspNetSynchronizationContext，续体回到原请求上下文（锁请求）；
    /// - 控制台/Windows 服务/ASP.NET Core：Current 默认 null → 续体由 ThreadPool 接管。
    ///
    /// 【典型适用场景】
    /// 1) 后台线程算出结果后回 UI 线程更新控件（Post）；
    /// 2) 非 UI 库需要在指定上下文执行回调时，由上层传入捕获的上下文；
    /// 3) 自定义宿主/测试：安装自己的 SynchronizationContext 决定 await 续体落点。
    ///
    /// 【使用步骤】
    /// 1) 在目标线程（如 UI 线程）捕获 SynchronizationContext.Current 存字段；
    /// 2) 后台线程用 context.Post 投递更新（不阻塞）；必须同步拿结果才用 Send（慎用）；
    /// 3) 自定义上下文继承 SynchronizationContext 并 override Post/Send/CreateCopy；
    /// 4) 临时 SetSynchronizationContext 后务必在 finally 还原原上下文。
    ///
    /// 【注意事项与坑】
    /// 1) Send 在“UI 线程正等待后台线程、后台线程又 Send 回 UI”时会死锁；优先 Post；
    /// 2) Current 是按线程/ExecutionContext 保存的：线程池线程上通常是 null，不要假设一定能拿到；
    /// 3) SetSynchronizationContext 只影响当前线程；它内部借助 ExecutionContext，异步流动有专门规则，
    ///    不要靠它传业务数据（业务数据用 AsyncLocal，见 HAsyncLocalHelp）；
    /// 4) OperationStarted/OperationCompleted 必须严格成对（finally），否则宿主操作计数泄漏；
    /// 5) 不要 new 一个基类 SynchronizationContext 安装后期待 Post 回同一线程——基类 Post 走线程池。
    ///
    /// 【版本可用性】SynchronizationContext（Post/Send/Current/SetSynchronizationContext/
    /// OperationStarted/OperationCompleted/CreateCopy/Wait）.NET Framework 2.0+ 即有；
    /// await 对它的捕获行为随 4.5 的 async/await 生效，net48 全部可用。
    /// </summary>
    ///
    /// <example>
    /// 后台线程安全更新 WinForms 控件：
    /// <code>
    /// SynchronizationContext ui = SynchronizationContext.Current;   // 在 UI 线程先捕获
    /// await Task.Run(() => Compute());
    /// HSynchronizationContextHelp.PostToUi(ui, () => label1.Text = "完成");
    /// </code>
    /// </example>
    public static class HSynchronizationContextHelp
    {
        /// <summary>
        /// 获取当前线程的同步上下文；控制台/线程池线程通常为 null。
        /// </summary>
        public static SynchronizationContext Current
        {
            get { return SynchronizationContext.Current; }
        }

        /// <summary>
        /// 示例1：捕获当前 UI 上下文（必须在 UI 线程上调用并存字段，不能事后在后台线程取）。
        /// </summary>
        /// <returns>当前同步上下文；没有安装时为 null。</returns>
        public static SynchronizationContext Capture()
        {
            return SynchronizationContext.Current;
        }

        /// <summary>
        /// 诊断辅助：返回当前上下文类型名；为 null 时提示“线程池/控制台（无上下文）”。
        /// </summary>
        /// <returns>上下文类型名或无上下文说明。</returns>
        public static string DescribeCurrent()
        {
            SynchronizationContext current = SynchronizationContext.Current;
            return current == null ? HTranslation.GetContent("(null) 线程池/控制台/ASP.NET Core：无 SynchronizationContext")
                                   : current.GetType().FullName;
        }

        /// <summary>
        /// 示例2：后台线程更新 UI——Post 异步投递到 UI 消息队列（不阻塞后台线程）。
        /// 传入 null 时（控制台/线程池无上下文）直接在当前线程执行。
        /// </summary>
        /// <param name="uiContext">预先在 UI 线程捕获的上下文；可为 null。</param>
        /// <param name="updateUi">更新 UI 的委托；不能为 null。</param>
        /// <exception cref="ArgumentNullException">updateUi 为 null 时抛出。</exception>
        public static void PostToUi(SynchronizationContext uiContext, Action updateUi)
        {
            if (updateUi == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("更新界面委托不能为空"));
            }

            if (uiContext != null)
            {
                // SendOrPostCallback 签名为 void(object)，用闭包把无参 Action 包进去
                uiContext.Post(delegate (object state) { updateUi(); }, null);
            }
            else
            {
                updateUi();    // 无 UI 上下文（控制台/线程池）直接执行
            }
        }

        /// <summary>
        /// 示例3：Send 同步投递——阻塞调用线程直到目标上下文执行完毕。
        /// 小心反方向依赖：UI 线程若正等待本线程会死锁；优先用 <see cref="PostToUi"/>。
        /// </summary>
        /// <param name="uiContext">目标上下文；不能为 null。</param>
        /// <param name="work">要在目标上下文执行的工作；不能为 null。</param>
        /// <exception cref="ArgumentNullException">参数为 null 时抛出。</exception>
        public static void SendToUi(SynchronizationContext uiContext, Action work)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("界面同步上下文不能为空"));
            }

            if (work == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("工作委托不能为空"));
            }

            // Send 内部会阻塞直到回调执行完成；回调内异常会直接传回调用线程
            uiContext.Send(delegate (object state) { work(); }, null);
        }

        /// <summary>
        /// 示例4：把 Post 包成可 await 的 Task（回调在目标上下文执行；异常经 TCS 正常传播）。
        /// </summary>
        /// <param name="context">目标上下文；null 表示直接在当前线程执行。</param>
        /// <param name="work">要执行的工作；不能为 null。</param>
        /// <returns>工作完成的 Task；工作抛异常时 Task 为 Faulted。</returns>
        /// <exception cref="ArgumentNullException">work 为 null 时抛出。</exception>
        public static Task PostAsync(SynchronizationContext context, Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("工作委托不能为空"));
            }

            // 异步续体选项，避免在 UI 线程 Post 回调里内联执行 await 方续体
            TaskCompletionSource<object> tcs =
                new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (context == null)
            {
                try
                {
                    work();
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
            else
            {
                context.Post(delegate (object state)
                {
                    try
                    {
                        work();
                        tcs.TrySetResult(null);
                    }
                    catch (Exception ex)
                    {
                        // 回投线程的异常经 TCS 传给 await 方，不会丢失在消息循环里
                        tcs.TrySetException(ex);
                    }
                }, null);
            }

            return tcs.Task;
        }

        /// <summary>
        /// 示例5：后台工作完成后安全回到 UI 线程的完整模式（Task.Run 计算 + Post 回 UI 呈现）。
        /// </summary>
        /// <param name="uiContext">UI 线程上下文；null 时更新动作直接在后台线程执行。</param>
        /// <param name="backgroundWork">线程池上执行的计算；不能为 null。</param>
        /// <param name="updateUi">在目标上下文执行的呈现动作；不能为 null。</param>
        /// <returns>整个流程的 Task。</returns>
        /// <exception cref="ArgumentNullException">参数为 null 时抛出。</exception>
        public static Task RunBackgroundThenUiAsync(SynchronizationContext uiContext,
                                                   Func<string> backgroundWork, Action<string> updateUi)
        {
            if (backgroundWork == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("后台工作委托不能为空"));
            }

            if (updateUi == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("更新界面委托不能为空"));
            }

            return Task.Run(delegate
            {
                string result = backgroundWork();           // 线程池线程做重计算
                PostToUi(uiContext, delegate { updateUi(result); });
            });
        }

        /// <summary>
        /// 示例6：成对通知宿主“异步操作开始/结束”（OperationStarted/OperationCompleted 必须严格配对）。
        /// 自定义宿主据此维护未完成操作计数；普通业务代码很少直接用。
        /// </summary>
        /// <param name="context">目标上下文；null 时不发通知直接执行。</param>
        /// <param name="operation">被计数的操作；不能为 null。</param>
        /// <exception cref="ArgumentNullException">operation 为 null 时抛出。</exception>
        public static void NotifyOperation(SynchronizationContext context, Action operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("操作委托不能为空"));
            }

            if (context == null)
            {
                operation();
                return;
            }

            context.OperationStarted();
            try
            {
                operation();
            }
            finally
            {
                // 即使操作抛异常也必须配对通知，否则宿主计数永远不平
                context.OperationCompleted();
            }
        }

        /// <summary>
        /// 示例7：临时安装上下文（测试/自定义宿主；await 回线程原理演示），finally 中还原原上下文。
        /// </summary>
        /// <param name="context">要临时安装的上下文；可为 null（显式清空）。</param>
        /// <param name="body">安装期间执行的委托；不能为 null。</param>
        /// <exception cref="ArgumentNullException">body 为 null 时抛出。</exception>
        public static void InstallTemporarily(SynchronizationContext context, Action body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("方法体委托不能为空"));
            }

            SynchronizationContext previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                body();   // 此期间 await 捕获到的就是刚安装的 context
            }
            finally
            {
                // 不还原会“污染”线程池线程的下一次租借
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        /// <summary>
        /// 示例8：最简自定义上下文——Post 始终把工作丢给线程池（行为接近“无上下文”），
        /// Send 通过 Post + 等待实现。用于理解上下文契约；生产中 WinForms/WPF 用各自内置实现。
        /// </summary>
        public sealed class ThreadPoolSynchronizationContext : SynchronizationContext
        {
            /// <summary>
            /// 异步投递：立即返回，回调在线程池线程执行。
            /// </summary>
            /// <param name="d">要执行的回调；不能为 null。</param>
            /// <param name="state">透传给回调的状态对象。</param>
            public override void Post(SendOrPostCallback d, object state)
            {
                // 基类默认实现也是 QueueUserWorkItem；此处显式重写以示契约
                ThreadPool.QueueUserWorkItem(delegate (object s) { d(s); }, state);
            }

            /// <summary>
            /// 同步投递：异步投递后阻塞等待完成，并用 ExceptionDispatchInfo 原样重抛回调异常。
            /// </summary>
            /// <param name="d">要执行的回调；不能为 null。</param>
            /// <param name="state">透传给回调的状态对象。</param>
            public override void Send(SendOrPostCallback d, object state)
            {
                if (d == null)
                {
                    throw new ArgumentNullException(HTranslation.GetContent("委托不能为空"));
                }

                Exception error = null;
                using (ManualResetEventSlim done = new ManualResetEventSlim(false))
                {
                    // 复用本类 Post：排队到线程池；本线程阻塞等信号
                    Post(delegate (object s)
                    {
                        try
                        {
                            d(s);
                        }
                        catch (Exception ex)
                        {
                            error = ex;   // 跨线程不能直接抛，先捕获
                        }
                        finally
                        {
                            done.Set();
                        }
                    }, state);
                    done.Wait();
                }

                if (error != null)
                {
                    // ExceptionDispatchInfo 保留原始堆栈/StackTrace（.NET 4.5+）
                    ExceptionDispatchInfo.Capture(error).Throw();
                }
            }

            /// <summary>
            /// 创建副本（ExecutionContext 跨线程传播时调用）。
            /// </summary>
            /// <returns>同类型的新实例。</returns>
            public override SynchronizationContext CreateCopy()
            {
                // 本上下文无每实例状态，直接新建即可；有状态时必须复制状态
                return new ThreadPoolSynchronizationContext();
            }
        }
    }
}
