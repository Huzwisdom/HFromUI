using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// Task / Task&lt;TResult&gt; 帮助类：基于线程池的异步任务抽象。
    /// 【是什么】System.Threading.Tasks.Task 表示一个"将来完成的工作单元"：Task 无返回值，
    /// Task&lt;TResult&gt; 有返回值。默认任务排队到 CLR 线程池执行，在上层提供返回值、异常聚合
    /// （AggregateException）、取消（CancellationToken）、延续（ContinueWith）、组合（WhenAll/WhenAny）
    /// 等能力；async/await 也建立在 Task 之上。
    /// 【是否跨进程】否。Task 是进程内的调度单元，跨进程通信请用 Process + 管道/Socket。
    /// 【典型适用场景】
    /// 1) 把后台工作丢给线程池并在之后取结果/延续（Task.Run、StartNew）；
    /// 2) 一组并发任务的"全部完成/任一完成"组合（WhenAll/WhenAny/WaitAll）；
    /// 3) 制造已完成任务（FromResult/FromException/FromCanceled/CompletedTask）做缓存或测试；
    /// 4) 定时延后（Delay）、超时控制（WhenAny + Delay）；
    /// 5) 长阻塞专用线程（StartNew + LongRunning）；async/await 异步链。
    /// 【使用步骤】
    /// 1) Task.Run / Task.Factory.StartNew 创建并启动任务；或 new Task(...) 后手动 Start；
    /// 2) 需要结果用 Task&lt;T&gt;.Result（阻塞）或 await；需要等待用 Wait/WaitAll（可带毫秒超时）；
    /// 3) 用 ContinueWith 指定完成后的延续，用 TaskContinuationOptions 筛选完成/出错/取消分支；
    /// 4) 用 Status/IsCompleted/IsFaulted/IsCanceled/Exception 观察任务状态；
    /// 5) 任务内异常在 Wait/Result/await 时以 AggregateException（await 时为首个内部异常）抛出。
    /// 【注意事项与坑】
    /// - 不要在 UI 线程上随意 .Result/.Wait()：任务若需要回到 UI 线程延续会死锁；
    /// - 从不 await/Wait 且从不读 Exception 的故障任务，在 GC 时触发 TaskScheduler.UnobservedTaskException；
    /// - Task.Run 默认会 DenyChildAttach，AttachedToParent 子任务只在 StartNew 场景有效；
    /// - 超时等待不会替你取消任务，取消要配合 CancellationTokenSource.CancelAfter/CTS；
    /// - Task.Status 是枚举（Created/WaitingForActivation/Running/RanToCompletion/Faulted/Canceled...）。
    /// 【版本可用性】net48 全量可用：Task(4.0)、Run/Delay/WhenAll/WhenAny/ContinueWith(4.5)、
    /// CompletedTask/FromException/FromCanceled/FromResult(FromResult 4.0，其余 4.6)、
    /// Wait/WaitAll/WaitAny、CreationOptions、AsyncState、Status、IsFaulted、IsCanceled、Exception。
    /// IAsyncEnumerable 风格的 TaskAsyncEnumerable 等属于更高版本，net48 不涉及。
    /// </summary>
    public static class HTaskHelp
    {
        /// <summary>示例1：启动无返回值任务（默认排队到线程池，等价 Task.Factory.StartNew 的常用形态）</summary>
        /// <param name="work">任务执行体</param>
        /// <returns>代表该工作的 Task</returns>
        public static Task Run(Action work)
        {
            return Task.Run(work);
        }

        /// <summary>示例2：启动带返回值任务，用 Result 阻塞取结果；任务内异常包成 AggregateException 抛出</summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="work">产生结果的工作</param>
        /// <returns>任务结果</returns>
        public static T RunAndGet<T>(Func<T> work)
        {
            return Task.Run(work).Result;
        }

        /// <summary>示例3：可取消任务；外部 cts.Cancel() 后任务在检查点抛 OperationCanceledException 退出</summary>
        /// <param name="work">接收取消令牌的工作</param>
        /// <param name="token">取消令牌</param>
        /// <returns>代表该工作的 Task</returns>
        public static Task RunWithCancel(Action<CancellationToken> work, CancellationToken token)
        {
            return Task.Run(delegate () { work(token); }, token);
        }

        /// <summary>示例4：完成后执行延续（不阻塞，前一任务结束自动跑 continuation）</summary>
        /// <param name="continuation">延续动作，参数是前一个任务</param>
        /// <param name="antecedent">前导任务；为 null 时用 Task.CompletedTask 立即延续</param>
        /// <returns>延续任务</returns>
        public static Task Continue(Action<Task> continuation, Task antecedent = null)
        {
            return (antecedent ?? Task.CompletedTask).ContinueWith(continuation);
        }

        /// <summary>示例5：按延续选项接延续（OnlyOnRanToCompletion/OnlyOnFaulted/OnlyOnCanceled 等分支）</summary>
        /// <param name="antecedent">前导任务</param>
        /// <param name="continuation">延续动作</param>
        /// <param name="options">延续触发条件，如 TaskContinuationOptions.OnlyOnFaulted</param>
        /// <returns>延续任务</returns>
        public static Task ContinueWithOptions(Task antecedent, Action<Task> continuation,
                                              TaskContinuationOptions options)
        {
            return antecedent.ContinueWith(continuation, options);
        }

        /// <summary>示例6：等待全部任务完成（Task.WhenAll）</summary>
        /// <param name="tasks">要组合的任务</param>
        /// <returns>全部完成才完成的 Task；任一出错则该 Task 为 Faulted</returns>
        public static Task WhenAll(params Task[] tasks)
        {
            return Task.WhenAll(tasks);
        }

        /// <summary>示例7：等待任一任务完成（Task.WhenAny），常用于超时/竞速</summary>
        /// <param name="tasks">要竞速的任务</param>
        /// <returns>最先完成的那个任务</returns>
        public static Task<Task> WhenAny(params Task[] tasks)
        {
            return Task.WhenAny(tasks);
        }

        /// <summary>示例8：带超时等待：超时返回 false，但不强制结束任务（取消需配合 CancellationToken）</summary>
        /// <param name="task">要等待的任务</param>
        /// <param name="timeoutMs">超时毫秒</param>
        /// <returns>true=任务在超时内完成；false=超时</returns>
        public static bool Wait(Task task, int timeoutMs)
        {
            return task.Wait(timeoutMs);
        }

        /// <summary>示例9：Task.Delay + WhenAny 实现"任务或超时先到"的超时控制（超时不取消原任务）</summary>
        /// <param name="task">要等待的任务</param>
        /// <param name="timeoutMs">超时毫秒</param>
        /// <returns>true=任务先完成；false=超时先到</returns>
        public static bool WaitWithTimeout(Task task, int timeoutMs)
        {
            Task waiter = Task.WhenAny(task, Task.Delay(timeoutMs));  // 谁先完成返回谁
            waiter.Wait(timeoutMs + 100);                            // 多给100ms，理论上 WhenAny 一定能完成
            return task.IsCompleted;
        }

        /// <summary>示例10：Task.Delay：异步延后，不阻塞线程（时间到了任务自动完成）</summary>
        /// <param name="millisecondsDelay">延后毫秒数</param>
        /// <param name="token">可选取消令牌</param>
        /// <returns>延时完成的 Task</returns>
        public static Task Delay(int millisecondsDelay, CancellationToken token = default(CancellationToken))
        {
            return Task.Delay(millisecondsDelay, token);
        }

        /// <summary>示例11：等待一组任务全部完成，带总超时（Task.WaitAll 重载）</summary>
        /// <param name="tasks">要等待的任务集合</param>
        /// <param name="timeoutMs">超时毫秒</param>
        /// <returns>true=全部完成；false=超时</returns>
        public static bool WaitAllWithTimeout(IEnumerable<Task> tasks, int timeoutMs)
        {
            List<Task> list = new List<Task>(tasks);
            return Task.WaitAll(list.ToArray(), timeoutMs);
        }

        /// <summary>示例12：Task.FromResult：直接造一个已成功完成、带结果的任务（缓存命中/同步实现异步接口常用）</summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="value">结果值</param>
        /// <returns>RanToCompletion 状态的 Task&lt;T&gt;</returns>
        public static Task<T> FromResult<T>(T value)
        {
            return Task.FromResult(value);
        }

        /// <summary>示例13：Task.FromException：造一个已出错的任务（同步实现异步方法时回报错误）</summary>
        /// <param name="exception">要携带的异常</param>
        /// <returns>Faulted 状态的 Task</returns>
        public static Task FromFaulted(Exception exception)
        {
            return Task.FromException(exception);
        }

        /// <summary>示例14：Task.FromException 泛型版：造一个已出错的 Task&lt;T&gt;</summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="exception">要携带的异常</param>
        /// <returns>Faulted 状态的 Task&lt;T&gt;</returns>
        public static Task<T> FromFaulted<T>(Exception exception)
        {
            return Task.FromException<T>(exception);
        }

        /// <summary>示例15：Task.FromCanceled：造一个已取消的 Task&lt;T&gt;（令牌必须已取消，否则抛 ArgumentException）</summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="token">已处于取消状态的令牌</param>
        /// <returns>Canceled 状态的 Task&lt;T&gt;</returns>
        /// <exception cref="ArgumentException">token 尚未取消时抛出</exception>
        public static Task<T> FromCanceled<T>(CancellationToken token)
        {
            return Task.FromCanceled<T>(token);
        }

        /// <summary>示例16：Task.CompletedTask：表示"已完成、无返回值"的单例任务</summary>
        public static Task CompletedTask
        {
            get { return Task.CompletedTask; }
        }

        /// <summary>示例17：new Task 后手动 Start，并携带状态对象与创建选项（Task.CreationOptions/AsyncState 可回看）</summary>
        /// <param name="work">任务执行体</param>
        /// <param name="state">挂到 Task.AsyncState 的状态对象</param>
        /// <param name="options">创建选项，如 LongRunning/PreferFairness/AttachedToParent</param>
        /// <returns>已经 Start 的 Task</returns>
        public static Task CreateAndStart(Action work, object state, TaskCreationOptions options)
        {
            Task task = new Task(delegate (object s) { work(); }, state, options);
            task.Start();                  // 与 Task.Run 的区别：创建与启动分离，可先存后发
            return task;
        }

        /// <summary>示例18：观察任务状态：Status/IsCompleted/IsFaulted/IsCanceled/CreationOptions/AsyncState/Exception</summary>
        /// <param name="task">要描述的任务</param>
        /// <returns>各状态属性拼成的可读字符串</returns>
        public static string DescribeStatus(Task task)
        {
            string text = "Status=" + task.Status
                + ", IsCompleted=" + task.IsCompleted
                + ", IsFaulted=" + task.IsFaulted
                + ", IsCanceled=" + task.IsCanceled
                + ", CreationOptions=" + task.CreationOptions
                + ", AsyncState=" + (task.AsyncState == null ? "null" : task.AsyncState.ToString());
            if (task.Exception != null)
            {
                // AggregateException 可能包多层，Flatten 后取根因
                text += ", BaseException=" + task.Exception.Flatten().GetBaseException().GetType().Name;
            }
            return text;
        }

        /// <summary>示例19：观察并处理故障任务：Flatten 后逐个 Handle（标记已处理），避免 UnobservedTaskException</summary>
        /// <param name="task">可能出错的任务</param>
        /// <param name="onError">对每个内部异常的处理回调（如记日志）</param>
        /// <returns>用于观察的延续任务</returns>
        public static Task ObserveAggregateException(Task task, Action<Exception> onError)
        {
            return task.ContinueWith(delegate (Task t)
            {
                t.Exception.Flatten().Handle(delegate (Exception ex)
                {
                    onError(ex);
                    return true;           // true=该异常已处理；返回 false 会重新抛出聚合异常
                });
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>示例20：长阻塞专用线程（不占线程池）：TaskCreationOptions.LongRunning，内部新建独立线程</summary>
        /// <param name="work">会长时间阻塞的工作</param>
        /// <returns>按 LongRunning 选项启动的 Task</returns>
        public static Task RunLongRunning(Action work)
        {
            return Task.Factory.StartNew(work, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 示例21：TaskExtensions.Unwrap 把 Task&lt;Task&gt;/Task&lt;Task&lt;T&gt;&gt; 嵌套任务展平。
        /// 外层任务代表“外层工作”，它的结果【本身又是一个任务】；直接拿到 Task&lt;Task&lt;T&gt;&gt;
        /// 时 await 一次只能得到内层 Task&lt;T&gt;（还得再等一次），Unwrap 返回一个代理 Task：
        /// 内外两层都完成后它才完成，并直接承载内层结果（内层出错则代理任务携带内层异常）。
        /// 本示例内层用 <see cref="Task.FromResult{TResult}(TResult)"/> 制造已完成任务，
        /// 避免线程池调度不确定性，快速同步就绪、无 UI、不阻塞。
        /// </summary>
        /// <returns>展平后的 Task&lt;int&gt;；调用方可直接 await/等待它拿到内层结果 7，无需二次等待。</returns>
        /// <exception cref="T:System.AggregateException">
        /// 仅当内层任务出错时，等待/访问返回的代理 Task 才会观察到异常（await 时解包为原始异常）；
        /// 本示例内层恒为 Task.FromResult(7)，不会抛出。
        /// </exception>
        public static Task<int> UnwrapNestedTaskExample()
        {
            // 注意：Task.Run 对“返回 Task 的委托”会优先选用自动解包重载 Run<TResult>(Func<Task<TResult>>)，
            // 直接得到 Task<int>；这里显式给出泛型实参 Task<int>，强制走 Run<TResult>(Func<TResult>)，
            // 才能得到真正的“嵌套任务” Task<Task<int>>（用 Task.Factory.StartNew 则天然是嵌套形态）
            Task<Task<int>> nested = Task.Run<Task<int>>(delegate
            {
                // 内层直接返回“已完成且结果为 7”的任务，保证同步快速完成、无调度不确定性
                return Task.FromResult(7);
            });
            Task<int> flat = nested.Unwrap();   // using System.Threading.Tasks; TaskExtensions.Unwrap 展平嵌套
            return flat;   // 调用方可直接 await/等待展平后的内层任务，拿到 int 结果
        }
    }
}
