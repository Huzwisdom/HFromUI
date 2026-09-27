using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// TaskScheduler 帮助类：控制 Task 实际被调度到哪里执行。
    /// 【是什么】System.Threading.Tasks.TaskScheduler 是任务调度抽象：Task.Factory/Task.Run 创建的任务
    /// 最终都要交给某个 TaskScheduler 排队执行。内置两个：Default（线程池调度器）和
    /// 通过 FromCurrentSynchronizationContext 得到的"当前同步上下文调度器"（UI 线程）。
    /// 【是否跨进程】否。调度器只决定任务在本进程的哪个/哪些线程上跑。
    /// 【典型适用场景】
    /// 1) 后台任务完成后更新 WinForms/WPF 控件：延续指定 UI 调度器；
    /// 2) 自定义派生调度器：单线程串行执行、限量并发、专属线程亲和（如游戏逻辑线程）；
    /// 3) 全局观察"没人 await 的故障任务"：订阅 UnobservedTaskException；
    /// 4) 诊断：Id / MaximumConcurrencyLevel。
    /// 【使用步骤】
    /// 1) 在 UI 线程上 FromCurrentSynchronizationContext 捕获 UI 调度器并存为字段；
    /// 2) ContinueWith(continuation, uiScheduler) 把延续送回 UI 线程；
    /// 3) 自定义调度器：派生 TaskScheduler，重写 QueueTask/TryExecuteTaskInline/GetScheduledTasks，
    ///    在自己的线程上调 TryExecuteTask 真正执行任务；
    /// 4) 应用启动时订阅 TaskScheduler.UnobservedTaskException 做兜底日志。
    /// 【注意事项与坑】
    /// - FromCurrentSynchronizationContext 必须在 UI 线程调用；线程池线程上 SynchronizationContext.Current 为 null，
    ///   会抛 InvalidOperationException；
    /// - 静态事件 UnobservedTaskException 会让委托长期存活，用完记得 -= 注销；
    /// - 自定义调度器必须保证任务最终被执行，否则任务永远卡住；内联执行判断线程身份；
    /// - 一般业务代码不直接操作调度器，UI 封送也可用 async/await + ConfigureAwait(true) 代替。
    /// 【版本可用性】net48 全量可用：Default、Current、FromCurrentSynchronizationContext、
    /// Id、MaximumConcurrencyLevel、UnobservedTaskException 事件、三个 protected 抽象成员。
    /// </summary>
    /// <example>
    /// 后台任务完成后回到 UI 线程更新控件：
    /// <code>
    /// TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();   // 在 UI 线程捕获
    /// Task.Run(delegate { /* 后台取数据 */ })
    ///     .ContinueWith(delegate { /* 此处已在 UI 线程，可直接赋值控件 */ }, ui);
    /// </code>
    /// </example>
    public static class HTaskSchedulerHelp
    {
        /// <summary>示例1：默认调度器（线程池），Task.Run 内部就是它</summary>
        public static TaskScheduler Default
        {
            get { return TaskScheduler.Default; }
        }

        /// <summary>示例2：当前调度器（TaskScheduler.Current）：在任务内返回该任务所用调度器，任务外等同于 Default</summary>
        public static TaskScheduler Current
        {
            get { return TaskScheduler.Current; }
        }

        /// <summary>示例3：获取 UI 线程调度器（必须在 UI 线程调用，常存为字段供后台延续使用）</summary>
        /// <returns>绑定当前 SynchronizationContext 的调度器</returns>
        /// <exception cref="InvalidOperationException">当前线程没有 SynchronizationContext（如线程池线程）时抛出</exception>
        public static TaskScheduler CaptureUiScheduler()
        {
            // WinForms 消息循环/WPF Dispatcher 环境下才能取到，否则抛 InvalidOperationException
            return TaskScheduler.FromCurrentSynchronizationContext();
        }

        /// <summary>示例4：安全版捕获：没有同步上下文（非 UI 线程）时返回 false 而不是抛异常</summary>
        /// <param name="scheduler">输出：捕获成功的 UI 调度器；失败为 null</param>
        /// <returns>true=捕获成功；false=当前线程无 SynchronizationContext</returns>
        public static bool TryCaptureUiScheduler(out TaskScheduler scheduler)
        {
            if (SynchronizationContext.Current == null)
            {
                scheduler = null;
                return false;
            }
            scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return true;
        }

        /// <summary>示例5：后台任务完成后用 UI 调度器执行延续更新控件</summary>
        /// <param name="backgroundTask">后台任务</param>
        /// <param name="updateUi">更新 UI 的动作（将在 UI 线程执行）</param>
        /// <param name="uiScheduler">UI 线程调度器</param>
        /// <returns>延续任务</returns>
        public static Task ContinueOnUi(Task backgroundTask, Action updateUi, TaskScheduler uiScheduler)
        {
            return backgroundTask.ContinueWith(delegate { updateUi(); }, uiScheduler);
        }

        /// <summary>示例6：读取调度器诊断信息：Id 与 MaximumConcurrencyLevel（int.MaxValue 表示不限制）</summary>
        /// <param name="scheduler">要描述的调度器</param>
        /// <returns>诊断信息字符串</returns>
        public static string Describe(TaskScheduler scheduler)
        {
            return "Id=" + scheduler.Id + ", MaximumConcurrencyLevel=" + scheduler.MaximumConcurrencyLevel;
        }

        /// <summary>当前挂载的 UnobservedTaskException 处理器（用于注销）</summary>
        private static EventHandler<UnobservedTaskExceptionEventArgs> unobservedHandler;

        /// <summary>示例7：订阅未被观察的任务异常（任务既没被 await/Wait 也没人读 Exception，GC 终结时触发）</summary>
        /// <param name="onError">收到未观察异常时的回调，参数是 AggregateException</param>
        /// <remarks>静态事件会持有委托，应用关闭前应调 DetachUnobservedException 注销。
        /// 该事件在终结器线程触发，回调内不要直接碰 UI。</remarks>
        public static void CatchUnobservedException(Action<AggregateException> onError)
        {
            DetachUnobservedException();
            unobservedHandler = delegate (object sender, UnobservedTaskExceptionEventArgs e)
            {
                e.SetObserved();                  // 标记已观察：net45+ 默认不会因此终止进程
                onError(e.Exception);
            };
            TaskScheduler.UnobservedTaskException += unobservedHandler;
        }

        /// <summary>示例8：注销 UnobservedTaskException 处理器，避免静态事件泄漏</summary>
        public static void DetachUnobservedException()
        {
            if (unobservedHandler != null)
            {
                TaskScheduler.UnobservedTaskException -= unobservedHandler;
                unobservedHandler = null;
            }
        }

        /// <summary>示例9：把一个动作排到单线程调度器上执行并等待完成（毫秒超时），用完即释放调度线程</summary>
        /// <param name="work">要串行执行的工作</param>
        /// <param name="timeoutMs">等待超时毫秒</param>
        /// <returns>true=工作在超时内完成；false=超时</returns>
        public static bool RunOnSingleThread(Action work, int timeoutMs)
        {
            using (SingleThreadScheduler scheduler = new SingleThreadScheduler("HThreadHelp.SingleThread"))
            {
                Task task = new Task(work);
                task.Start(scheduler);
                return task.Wait(timeoutMs);
            }
        }

        /// <summary>
        /// 自定义调度器：把所有任务串行排队到一个专用后台线程执行（可运行的简化版单线程调度器）。
        /// 派生子类必须重写 QueueTask（入队）、TryExecuteTaskInline（内联）、GetScheduledTasks（诊断），
        /// 并在自有线程上用 TryExecuteTask 真正执行任务。
        /// </summary>
        public sealed class SingleThreadScheduler : TaskScheduler, IDisposable
        {
            private readonly Queue<Task> queue = new Queue<Task>();   // 单队列保证 FIFO 串行
            private readonly Thread workerThread;
            private volatile bool disposed;

            /// <summary>创建调度器并立即启动专用后台工作线程</summary>
            /// <param name="name">工作线程名，便于调试辨认</param>
            public SingleThreadScheduler(string name = "HTaskSchedulerHelp.SingleThread")
            {
                workerThread = new Thread(RunLoop) { IsBackground = true, Name = name };
                workerThread.Start();
            }

            /// <summary>专用工作线程的托管线程 Id</summary>
            public int WorkerThreadId
            {
                get { return workerThread.ManagedThreadId; }
            }

            /// <summary>调度器把任务入队并唤醒工作线程</summary>
            /// <param name="task">要排队的任务</param>
            protected override void QueueTask(Task task)
            {
                lock (queue)
                {
                    queue.Enqueue(task);
                    Monitor.Pulse(queue);         // 唤醒可能正在 Wait 的工作线程
                }
            }

            /// <summary>尝试在调用线程内联执行任务：只有调用方正是本调度器的工作线程时才内联</summary>
            /// <param name="task">请求内联的任务</param>
            /// <param name="taskWasPreviouslyQueued">该任务之前是否已入队</param>
            /// <returns>true=已内联执行；false=拒绝内联，仍走队列</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (Thread.CurrentThread.ManagedThreadId != workerThread.ManagedThreadId)
                {
                    return false;                  // 其他线程不内联，保证始终串行
                }
                return TryExecuteTask(task);      // 基类方法负责真正执行并防重复执行
            }

            /// <summary>返回已排队任务快照（调试器/诊断用）</summary>
            /// <returns>队列中的任务数组</returns>
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                lock (queue)
                {
                    return queue.ToArray();
                }
            }

            /// <summary>工作线程主循环：无任务时休眠，被唤醒后取队首任务执行</summary>
            private void RunLoop()
            {
                while (true)
                {
                    Task task = null;
                    lock (queue)
                    {
                        while (queue.Count == 0)
                        {
                            if (disposed)
                            {
                                return;          // 释放后排空退出
                            }
                            Monitor.Wait(queue);  // 释放锁并休眠，Pulse 唤醒后重新拿锁
                        }
                        task = queue.Dequeue();
                    }
                    TryExecuteTask(task);         // 在锁外执行，避免任务体反向入队时死锁
                }
            }

            /// <summary>停止调度器：唤醒工作线程退出，带 1 秒 Join 超时防止自测卡死；未执行任务不再执行</summary>
            public void Dispose()
            {
                disposed = true;
                lock (queue)
                {
                    Monitor.PulseAll(queue);
                }
                workerThread.Join(1000);
            }
        }
    }
}
