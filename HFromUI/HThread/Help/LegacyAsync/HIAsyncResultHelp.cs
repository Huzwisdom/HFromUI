using System;
using System.Threading;

namespace HFromUI.HThread.Help.LegacyAsync
{
    using HFromUI.HLangage;
    /// <summary>
    /// IAsyncResult 帮助类：APM 异步操作的状态凭证接口及手写实现骨架。
    /// 【是什么】System.IAsyncResult 是 BeginXxx 返回、EndXxx 消费的接口，四个成员：
    /// 1) IsCompleted（bool）：操作是否已结束（成功/失败/取消都算完成），轮询模式用；
    /// 2) AsyncWaitHandle（WaitHandle）：完成时被 Set 的内核等待句柄，可 WaitOne/WaitAll 阻塞或多对象等待，
    ///    通常惰性创建（没人等就不分配内核句柄）；
    /// 3) AsyncState（object）：BeginXxx 最后一个 state 参数原样带回，用于关联请求上下文；
    /// 4) CompletedSynchronously（bool）：优化提示——true 表示操作在 Begin 调用内已同步完成，
    ///    此时 AsyncCallback 可能被【内联】调用（还在 Begin 的调用栈上），消费方须避免重入假设。
    /// 实现方契约：完成时必须先发布结果/异常并保证 IsCompleted 可见，再 Set 等待句柄、最后调 AsyncCallback；
    /// EndXxx 必须且只能被调用一次（重复调用抛 InvalidOperationException），负责传播结果或重新抛出后台异常。
    /// 【是否跨进程】否。接口只承载进程内异步状态；异步 IO 场景下的跨设备等待由内核 IOCP 完成，与该接口无关。
    /// 【典型适用场景】手写老式 Begin/End API、把自定义组件适配成 APM、阅读框架源码（Task 本身也实现了 IAsyncResult）；
    /// 新代码直接用 Task，无需自己实现。
    /// 【使用步骤】
    /// 1) BeginXxx 中创建 IAsyncResult 实现（捕获 callback/state），后台线程执行工作；
    /// 2) 工作结束发布 result/error → 置 IsCompleted → AsyncWaitHandle.Set() → 调 AsyncCallback；
    /// 3) EndXxx 校验参数归属与"只调一次"，未完成则阻塞 AsyncWaitHandle，之后抛异常或返回结果；
    /// 4) 释放实现时关闭惰性创建的 WaitHandle。
    /// 【注意事项与坑】
    /// - 完成信号顺序：必须先写结果/异常（加内存屏障/锁/Volatile），再 Set 句柄/回调，否则等待方可能读到半成品；
    /// - AsyncCallback 内通常立即 EndXxx，因此回调可能在后台线程或 Begin 调用栈上执行，回调代码要两种都兼容；
    /// - AsyncWaitHandle 用惰性初始化时，竞态下"完成先于创建句柄"也要保证句柄创建后立刻处于 Set 态；
    /// - EndXxx 传错对象（别的操作的 IAsyncResult）必须抛 ArgumentException；
    /// - 异常重抛建议用 ExceptionDispatchInfo 保留原始堆栈（net45+），直接 throw ex 会截断堆栈。
    /// 【版本可用性】net48 全量可用；.NET Core/.NET 5+ 接口保留；委托 BeginInvoke 在 .NET Core 移除，
    /// 但接口与框架自带 APM（Stream/Socket 等）仍在。
    /// </summary>
    public static class HIAsyncResultHelp
    {
        /// <summary>
        /// 示例1：解读一次 APM 调用返回的 IAsyncResult 四成员含义。
        /// </summary>
        /// <param name="ar">BeginXxx 返回的凭证</param>
        /// <param name="expectedState">期望在 AsyncState 中看到的对象</param>
        /// <returns>四成员状态描述字符串</returns>
        /// <exception cref="ArgumentNullException">ar 为 null</exception>
        public static string Describe(IAsyncResult ar, object expectedState)
        {
            if (ar == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("异步结果不能为空"));
            }
            bool done = ar.IsCompleted;                       // true=操作已结束（成功/失败/取消都算）
            WaitHandle handle = ar.AsyncWaitHandle;           // 可 WaitOne/WaitAll 阻塞等待（惰性创建）
            object state = ar.AsyncState;                     // 即 BeginXxx 最后一个参数，原样带回
            bool sync = ar.CompletedSynchronously;            // true=Begin 内同步做完，回调可能内联
            return string.Format(HTranslation.GetContent("IsCompleted={0} AsyncState匹配={1} CompletedSynchronously={2} WaitHandle={3}"),
                                 done, state == expectedState, sync,
                                 handle == null ? "(null)" : handle.GetType().Name);
        }

        /// <summary>
        /// 示例2：简易手写 IAsyncResult（非泛型基础骨架）：状态 + 惰性句柄 + AsyncCallback 触发。
        /// </summary>
        public sealed class SimpleAsyncResult : IAsyncResult
        {
            private readonly ManualResetEvent waitHandle;
            private readonly AsyncCallback callback;
            private int completed;
            private int endCalled;

            /// <summary>
            /// 构造一个不带完成回调的凭证。
            /// </summary>
            /// <param name="state">自定义状态（AsyncState）</param>
            /// <param name="completedSynchronously">是否同步完成（true 时句柄创建即为有信号）</param>
            public SimpleAsyncResult(object state, bool completedSynchronously)
                : this(state, completedSynchronously, null)
            {
            }

            /// <summary>
            /// 构造一个带 AsyncCallback 的凭证。
            /// </summary>
            /// <param name="state">自定义状态（AsyncState）</param>
            /// <param name="completedSynchronously">是否同步完成</param>
            /// <param name="callback">完成时调用的回调；null 表示纯轮询/阻塞使用</param>
            public SimpleAsyncResult(object state, bool completedSynchronously, AsyncCallback callback)
            {
                AsyncState = state;
                CompletedSynchronously = completedSynchronously;
                this.callback = callback;
                // 同步完成：句柄直接创建为有信号；异步完成：无信号，等 SetComplete
                waitHandle = new ManualResetEvent(completedSynchronously);
                if (completedSynchronously)
                {
                    Volatile.Write(ref completed, 1);
                }
            }

            /// <summary>BeginXxx 传入的自定义状态对象</summary>
            /// <returns>AsyncState</returns>
            public object AsyncState { get; private set; }

            /// <summary>完成时被 Set 的内核等待句柄</summary>
            /// <returns>ManualResetEvent</returns>
            public WaitHandle AsyncWaitHandle
            {
                get { return waitHandle; }
            }

            /// <summary>是否在 Begin 调用内同步完成</summary>
            /// <returns>true=同步完成（回调可能内联）</returns>
            public bool CompletedSynchronously { get; private set; }

            /// <summary>操作是否已结束</summary>
            /// <returns>true=已完成</returns>
            public bool IsCompleted
            {
                // Volatile 读保证后台线程的完成写对等待方可见
                get { return Volatile.Read(ref completed) != 0; }
            }

            /// <summary>
            /// EndXxx 进入时调用的"只调一次"门闩。
            /// </summary>
            /// <returns>true=本次是第一次 End；false=重复 End，应抛 InvalidOperationException</returns>
            public bool TryBeginEnd()
            {
                return Interlocked.Exchange(ref endCalled, 1) == 0;
            }

            /// <summary>
            /// 工作线程完成后调用：发布完成状态 → Set 句柄 → 触发 AsyncCallback（顺序不可颠倒）。
            /// </summary>
            public void SetComplete()
            {
                // 幂等：保证只完成一次
                if (Interlocked.Exchange(ref completed, 1) == 1)
                {
                    return;
                }
                waitHandle.Set();                  // 先放行阻塞等待者
                if (callback != null)
                {
                    callback(this);                // 再发回调（回调内通常立即 EndXxx）
                }
            }
        }

        /// <summary>
        /// 示例3：EndXxx 实现方的标准校验：类型归属 + 只调一次 + 未完成则阻塞。
        /// </summary>
        /// <param name="ar">传给 EndXxx 的凭证</param>
        /// <exception cref="ArgumentNullException">ar 为 null</exception>
        /// <exception cref="ArgumentException">ar 不是本操作的 SimpleAsyncResult</exception>
        /// <exception cref="InvalidOperationException">End 被重复调用</exception>
        public static void EndCore(IAsyncResult ar)
        {
            if (ar == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("异步结果不能为空"));
            }
            SimpleAsyncResult mine = ar as SimpleAsyncResult;
            if (mine == null)
            {
                throw new ArgumentException(HTranslation.GetContent("IAsyncResult 不属于当前操作"));
            }
            if (!mine.TryBeginEnd())
            {
                // EndXxx 只调一次原则：重复调用直接拒绝
                throw new InvalidOperationException(HTranslation.GetContent("EndXxx 只能调用一次"));
            }
            if (!mine.IsCompleted)
            {
                mine.AsyncWaitHandle.WaitOne();    // 未完成则阻塞等（生产代码建议带超时）
            }
            // 真实实现在此重新抛出工作异常 / 返回结果（参考 FullAsyncResult<T>.End）
        }

        /// <summary>
        /// 示例4：完整手写 IAsyncResult 泛型骨架：结果/异常存储、惰性 WaitHandle、AsyncCallback 触发、
        /// End 只调一次校验、Begin 的同步/异步两种启动路径，是自定义 BeginXxx/EndXxx 的可直接套用模板。
        /// </summary>
        /// <typeparam name="TResult">异步结果类型</typeparam>
        public sealed class FullAsyncResult<TResult> : IAsyncResult, IDisposable
        {
            private readonly AsyncCallback callback;   // Begin 传入的完成回调
            private readonly object syncRoot = new object();
            private ManualResetEvent waitHandle;       // 惰性创建：没人阻塞就不分配内核句柄
            private int completed;                     // 0=未完成 1=已完成
            private int endCalled;                     // End 一次门闩
            private TResult result;
            private Exception error;

            private FullAsyncResult(object asyncState, AsyncCallback callback, bool completedSynchronously)
            {
                AsyncState = asyncState;
                this.callback = callback;
                CompletedSynchronously = completedSynchronously;
            }

            /// <summary>BeginXxx 传入的自定义状态</summary>
            /// <returns>AsyncState</returns>
            public object AsyncState { get; private set; }

            /// <summary>是否同步完成</summary>
            /// <returns>true=在 Begin 调用栈内完成</returns>
            public bool CompletedSynchronously { get; private set; }

            /// <summary>是否已完成（成功/失败/取消均算）</summary>
            /// <returns>true=已完成</returns>
            public bool IsCompleted
            {
                get { return Volatile.Read(ref completed) != 0; }
            }

            /// <summary>
            /// 惰性创建的完成等待句柄；竞态下若操作已先完成，创建后立即置为有信号。
            /// </summary>
            /// <returns>可阻塞等待的 ManualResetEvent</returns>
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    // 双检锁：保证全实例只创建一个句柄
                    if (waitHandle == null)
                    {
                        lock (syncRoot)
                        {
                            if (waitHandle == null)
                            {
                                ManualResetEvent handle = new ManualResetEvent(false);
                                if (IsCompleted)
                                {
                                    handle.Set();     // 完成先于句柄创建：补信号
                                }
                                waitHandle = handle;
                            }
                        }
                    }
                    return waitHandle;
                }
            }

            /// <summary>
            /// Begin 语义入口：创建凭证并启动工作。
            /// </summary>
            /// <param name="callback">完成回调（可为 null）</param>
            /// <param name="state">自定义状态</param>
            /// <param name="work">实际工作（返回 TResult；抛异常即异步失败）</param>
            /// <param name="runSynchronously">true=在当前线程内联执行（CompletedSynchronously=true）；
            /// false=排队到线程池</param>
            /// <returns>已启动的凭证</returns>
            /// <exception cref="ArgumentNullException">work 为 null</exception>
            public static FullAsyncResult<TResult> Start(
                AsyncCallback callback, object state, Func<TResult> work, bool runSynchronously)
            {
                if (work == null)
                {
                    throw new ArgumentNullException(HTranslation.GetContent("工作委托不能为空"));
                }
                FullAsyncResult<TResult> ar = new FullAsyncResult<TResult>(state, callback, runSynchronously);
                if (runSynchronously)
                {
                    ar.Execute(work);                  // 内联执行：回调也会内联触发（APM 允许）
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(delegate (object unused)
                    {
                        ar.Execute(work);              // 异步路径：完成后回调在线程池线程触发
                    });
                }
                return ar;
            }

            private void Execute(Func<TResult> work)
            {
                try
                {
                    result = work();                   // 存结果，暂不发布
                }
                catch (Exception ex)
                {
                    error = ex;                        // 存异常，由 End 重抛
                }
                Complete();
            }

            private void Complete()
            {
                // 完成只发布一次
                if (Interlocked.Exchange(ref completed, 1) == 1)
                {
                    return;
                }
                lock (syncRoot)
                {
                    // 先 Set 句柄（结果/异常字段在此之前已写完，配合 AsyncWaitHandle 锁建立可见性）
                    if (waitHandle != null)
                    {
                        waitHandle.Set();
                    }
                }
                if (callback != null)
                {
                    callback(this);                    // 最后通知：回调内即可 End
                }
            }

            /// <summary>
            /// EndXxx 语义：校验只调一次 → 等待完成 → 重抛异常或返回结果。
            /// </summary>
            /// <returns>工作结果</returns>
            /// <exception cref="InvalidOperationException">End 被重复调用</exception>
            public TResult End()
            {
                // End 只调一次原则
                if (Interlocked.Exchange(ref endCalled, 1) == 1)
                {
                    throw new InvalidOperationException(HTranslation.GetContent("EndXxx 只能调用一次"));
                }
                if (!IsCompleted)
                {
                    AsyncWaitHandle.WaitOne();         // 未完成则阻塞（库代码建议带超时重载）
                }
                if (error != null)
                {
                    // 保留原始堆栈重抛（net45+；比裸 throw error 更利于排错）
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
                }
                return result;
            }

            /// <summary>
            /// 释放惰性句柄（End 之后由调用方释放）。
            /// </summary>
            public void Dispose()
            {
                lock (syncRoot)
                {
                    if (waitHandle != null)
                    {
                        waitHandle.Dispose();
                        waitHandle = null;
                    }
                }
            }
        }

        /// <summary>
        /// 示例5：完整骨架的确定性自测：同步启动 → End 取结果 → Dispose，不等待线程池、不依赖消息循环。
        /// </summary>
        /// <returns>结果/同步标志/状态的描述字符串</returns>
        public static string FullPatternDemo()
        {
            // 同步路径：工作在 Start 内联完成，End 立即拿到结果
            FullAsyncResult<int> ar = FullAsyncResult<int>.Start(
                null, "state-1", delegate { return 42; }, true);
            int value = ar.End();                      // 第一次 End：成功
            ar.Dispose();
            return string.Format("Result={0}, CompletedSynchronously={1}, AsyncState={2}",
                                 value, ar.CompletedSynchronously, ar.AsyncState);
        }
    }
}
