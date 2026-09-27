using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="TaskCompletionSource{TResult}"/> 帮助类：不自己执行任何代码、
    /// 只用来【手动控制一个 Task 的完成时机与结果】的原语。关联的 Task 从 <see cref="TaskCompletionSource{TResult}.Task"/>
    /// 取出交给消费方 await；生产方在任意线程上调用：
    /// SetResult / SetException / SetCanceled（只能调一次，重复抛 InvalidOperationException），
    /// 或 TrySetResult / TrySetException / TrySetCanceled（返回 bool，重复调用安全，仅第一次生效）。
    /// 构造可传 <see cref="TaskCreationOptions"/>；最常用的是
    /// <see cref="TaskCreationOptions.RunContinuationsAsynchronously"/>（.NET 4.6+，net48 可用）：
    /// 完成调用会把续体【异步投递】而非在调用 SetResult 的线程上内联执行，防止回调重入/持锁回调/栈加深。
    ///
    /// 【是否跨进程】否。
    ///
    /// 【典型适用场景】
    /// 1) 把回调/事件式老 API（EAP、定时回调、一次性通知、Native Overlapped 回调）桥接成可 await 的 Task；
    /// 2) 手写异步原语：异步锁、异步信号、限流器、带超时/可取消的操作；
    /// 3) 测试中制造各种状态的 Task（已成功/已失败/已取消/延迟完成）。
    ///
    /// 【使用步骤】
    /// 1) 创建 TCS（库代码建议带 RunContinuationsAsynchronously）；
    /// 2) 把 tcs.Task 返回给调用方；
    /// 3) 在成功回调里 TrySetResult；失败回调里 TrySetException(单个 Exception)；中止时 TrySetCanceled；
    /// 4) 一次性事件用 TrySetXxx 防止重复回调导致异常；
    /// 5) 需要超时/外部取消时用 CreateLinkedTokenSource + Register 桥接，并注销回调、Dispose CTS。
    ///
    /// 【注意事项与坑】
    /// 1) SetException 传【业务异常本身】（如 new HttpRequestException(...)）即可，
    ///    不要手动包 AggregateException——await 会解包，多包一层只会让调用方多剥一层；
    ///    需要挂多个异常才用 TrySetException(IEnumerable&lt;Exception&gt;)；
    /// 2) SetXxx 在 Task 已完成后调用会抛 InvalidOperationException；不确定回调是否多次触发时一律 TrySetXxx；
    /// 3) 默认选项下，SetResult 时续体【可能在调用线程上同步内联执行】（await 续体与加了
    ///    ExecuteSynchronously 的 ContinueWith 会内联；普通 ContinueWith 在 net48 实测常被异步排队，
    ///    总之不能依赖“一定/一定不”内联）：内联续体若回头抢同一把锁/再次调用触发方，
    ///    会产生重入死锁或超长调用栈；库代码用 RunContinuationsAsynchronously 获得“绝不内联”的保证；
    /// 4) TrySetCanceled() 产生的被取消 Task 的 Task.CancellationToken 不匹配任何已知令牌；
    ///    需要 await 方正确识别“由哪枚令牌取消”时用 TrySetCanceled(CancellationToken)（.NET 4.6+）；
    /// 5) TCS 自身不占用线程、无需 Dispose；但为它创建的 CTS、注册的 CancellationTokenRegistration 要清理；
    /// 6) 不要用 TaskCompletionSource（无泛型）——.NET Framework 上只有 TaskCompletionSource&lt;TResult&gt;，
    ///    无返回值场景用 TaskCompletionSource&lt;bool&gt;/&lt;object&gt; 或 TaskCompletionSource&lt;Tuple&gt;。
    ///
    /// 【版本可用性】TCS、Set/TrySet 六个基础方法为 .NET Framework 4.0+；
    /// SetException(IEnumerable)、TrySetCanceled(CancellationToken)、RunContinuationsAsynchronously 为 4.6+；
    /// TrySetFromTask / 非泛型 TaskCompletionSource 【net48 不提供】（仅 .NET 5+/部分 Core 版本），
    /// 需要“任务完成后把结果搬给 TCS”请自己 await 后 TrySet（见类内注释示例）。
    /// </summary>
    ///
    /// <example>
    /// TrySetFromTask 的 net48 手工等价（注释-only，因为 net48 没有该方法）：
    /// <code>
    /// // async Task TransferAsync(TaskCompletionSource&lt;int&gt; tcs, Task&lt;int&gt; source)
    /// // {
    /// //     try { tcs.TrySetResult(await source.ConfigureAwait(false)); }
    /// //     catch (OperationCanceledException) { tcs.TrySetCanceled(); }
    /// //     catch (Exception ex) { tcs.TrySetException(ex); }
    /// // }
    /// </code>
    /// </example>
    public static class HTaskCompletionSourceHelp
    {
        /// <summary>
        /// 示例1：把一次性事件回调桥接成 Task（事件触发即完成）。
        /// </summary>
        /// <param name="registerOnce">注册逻辑：参数为“结果回调”，实现方在事件发生时调用它。</param>
        /// <returns>事件触发时完成、携带回调结果的 Task。</returns>
        /// <exception cref="ArgumentNullException">registerOnce 为 null 时抛出。</exception>
        public static Task<string> EventToTask(Action<Action<string>> registerOnce)
        {
            if (registerOnce == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("注册回调不能为空"));
            }

            // 异步续体选项：回调线程 SetResult 时不会内联执行 await 方续体，避免回调链重入
            TaskCompletionSource<string> tcs = CreateWithAsyncContinuations<string>();
            registerOnce(delegate (string result)
            {
                tcs.TrySetResult(result);      // 回调在任意线程触发都安全；重复触发只有第一次生效
            });
            return tcs.Task;
        }

        /// <summary>
        /// 示例2：把成功/失败两类回调桥接成 Task；失败侧传【单个业务异常】，不要包 AggregateException。
        /// </summary>
        /// <param name="registerOnce">注册逻辑：第1参=成功回调，第2参=失败回调(Exception)。</param>
        /// <returns>成功携带字符串、失败携带原始异常的 Task。</returns>
        /// <exception cref="ArgumentNullException">registerOnce 为 null 时抛出。</exception>
        public static Task<string> EventWithError(Action<Action<string>, Action<Exception>> registerOnce)
        {
            if (registerOnce == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("注册回调不能为空"));
            }

            TaskCompletionSource<string> tcs = CreateWithAsyncContinuations<string>();
            registerOnce(
                delegate (string result) { tcs.TrySetResult(result); },
                delegate (Exception ex)
                {
                    // 直接传业务异常本身；await 时调用方 catch 到的就是它
                    tcs.TrySetException(ex);
                });
            return tcs.Task;
        }

        /// <summary>
        /// 示例3：给外部 TCS 加超时/外部取消：任一触发即把关联 Task 置为 Canceled，避免永久挂起。
        /// </summary>
        /// <param name="tcs">要保护的 TCS；不能为 null。</param>
        /// <param name="token">外部取消令牌。</param>
        /// <param name="timeoutMs">超时毫秒（自测传小值）。</param>
        /// <returns>tcs.Task（完成后会清理内部链接 CTS）。</returns>
        /// <exception cref="ArgumentNullException">tcs 为 null 时抛出。</exception>
        public static Task<string> WithTimeout(TaskCompletionSource<string> tcs, CancellationToken token, int timeoutMs)
        {
            if (tcs == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务完成源不能为空"));
            }

            // linkedCTS：外部令牌取消 或 自身超时，都会取消它
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            CancellationTokenRegistration registration =
                cts.Token.Register(delegate { tcs.TrySetCanceled(); });
            cts.CancelAfter(timeoutMs);

            // 任务终态后注销回调并释放链接 CTS，避免句柄泄漏
            tcs.Task.ContinueWith(delegate
            {
                registration.Dispose();
                cts.Dispose();
            }, TaskScheduler.Default);

            return tcs.Task;
        }

        /// <summary>
        /// 示例4：让关联任务“同步成功完成”（缓存/测试常用）。
        /// </summary>
        /// <param name="value">完成结果。</param>
        /// <returns>状态为 RanToCompletion 的 Task&lt;int&gt;。</returns>
        public static Task<int> CompletedWith(int value)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            tcs.SetResult(value);              // SetResult 后 Task.Status 立即为 RanToCompletion
            return tcs.Task;
        }

        /// <summary>
        /// 示例5：手动取消（外部中止事件 → Task 变为 Canceled）。TrySet 版本可重复调用。
        /// </summary>
        /// <typeparam name="T">TCS 结果类型。</typeparam>
        /// <param name="tcs">目标 TCS；不能为 null。</param>
        /// <returns>本次调用是否成功把 Task 置为 Canceled（首次为 true，已终态为 false）。</returns>
        /// <exception cref="ArgumentNullException">tcs 为 null 时抛出。</exception>
        public static bool MarkCanceled<T>(TaskCompletionSource<T> tcs)
        {
            if (tcs == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务完成源不能为空"));
            }

            return tcs.TrySetCanceled();
        }

        /// <summary>
        /// 创建带 <see cref="TaskCreationOptions.RunContinuationsAsynchronously"/> 选项的 TCS。
        /// 库代码首选：Set/TrySet 完成时续体一律被异步调度，不会在完成调用的线程/调用栈内联执行，
        /// 从而避免“持锁线程执行回调 → 回调再抢锁”的重入死锁和栈过深。
        /// </summary>
        /// <typeparam name="T">关联 Task 的结果类型。</typeparam>
        /// <returns>配置好异步续体选项的 TCS（.NET 4.6+，net48 可用）。</returns>
        public static TaskCompletionSource<T> CreateWithAsyncContinuations<T>()
        {
            return new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// 示例6：以单个业务异常使任务失败。强调：传 Exception 本身，不要包 AggregateException。
        /// </summary>
        /// <typeparam name="T">TCS 结果类型。</typeparam>
        /// <param name="tcs">目标 TCS；不能为 null。</param>
        /// <param name="error">业务异常；不能为 null。</param>
        /// <returns>本次是否成功置为 Faulted。</returns>
        /// <exception cref="ArgumentNullException">tcs 或 error 为 null 时抛出。</exception>
        public static bool TryMarkFaulted<T>(TaskCompletionSource<T> tcs, Exception error)
        {
            if (tcs == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务完成源不能为空"));
            }

            if (error == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("异常对象不能为空"));
            }

            // await tcs.Task 时调用方 catch 到的就是这个原始异常
            return tcs.TrySetException(error);
        }

        /// <summary>
        /// 示例7：一次挂接多个异常（Task.Exception.InnerExceptions 会包含全部）。
        /// </summary>
        /// <typeparam name="T">TCS 结果类型。</typeparam>
        /// <param name="tcs">目标 TCS；不能为 null。</param>
        /// <param name="errors">异常集合；不能为 null/空。</param>
        /// <returns>本次是否成功置为 Faulted。</returns>
        /// <exception cref="ArgumentNullException">参数为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">errors 为空集合时抛出。</exception>
        public static bool TryMarkFaulted<T>(TaskCompletionSource<T> tcs, IEnumerable<Exception> errors)
        {
            if (tcs == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务完成源不能为空"));
            }

            if (errors == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("异常集合不能为空"));
            }

            List<Exception> list = new List<Exception>(errors);
            if (list.Count == 0)
            {
                throw new ArgumentException(HTranslation.GetContent("异常集合不能为空"), "errors");
            }

            // await 仍只抛第一个；需要看到全部得读 tcs.Task.Exception.InnerExceptions
            return tcs.TrySetException(list);
        }

        /// <summary>
        /// 示例8：带具体令牌的取消（.NET 4.6+）。相比无参 TrySetCanceled()，关联 Task 记录的
        /// CancellationToken 就是该令牌，await 方捕获的 OperationCanceledException.CancellationToken
        /// 能与之匹配，从而被正确识别为“正常取消”而非故障。
        /// </summary>
        /// <typeparam name="T">TCS 结果类型。</typeparam>
        /// <param name="tcs">目标 TCS；不能为 null。</param>
        /// <param name="cancellationToken">导致取消的令牌（通常已处于取消状态）。</param>
        /// <returns>本次是否成功置为 Canceled。</returns>
        /// <exception cref="ArgumentNullException">tcs 为 null 时抛出。</exception>
        public static bool TryMarkCanceled<T>(TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
        {
            if (tcs == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务完成源不能为空"));
            }

            return tcs.TrySetCanceled(cancellationToken);
        }

        /// <summary>
        /// 示例9：把“一次性定时回调”桥接成可 await Task（自测友好：不依赖 UI/网络，小延时秒回）。
        /// 用 CTS.CancelAfter + Register 模拟某个老 API 在完成时回调一次。
        /// </summary>
        /// <param name="result">回调触发时要交付的结果。</param>
        /// <param name="delayMs">回调延时毫秒（自测传小值，如 10）。</param>
        /// <returns>回调触发后得到 result。</returns>
        /// <exception cref="ArgumentOutOfRangeException">delayMs 为负数时由 CancelAfter 抛出。</exception>
        public static async Task<int> SimulateOneShotCallbackAsync(int result, int delayMs)
        {
            TaskCompletionSource<int> tcs = CreateWithAsyncContinuations<int>();
            CancellationTokenSource cts = new CancellationTokenSource();

            // 令牌取消（= 老 API 的“完成回调”）时把结果交给 TCS
            CancellationTokenRegistration registration =
                cts.Token.Register(delegate { tcs.TrySetResult(result); });
            cts.CancelAfter(delayMs);

            try
            {
                // 注册完成后立即 await；回调在定时器线程触发后此 await 结束
                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                registration.Dispose();
                cts.Dispose();
            }
        }

        /// <summary>
        /// 查询关联 Task 的当前状态（RanToCompletion/Faulted/Canceled/WaitingForActivation 等）。
        /// </summary>
        /// <typeparam name="T">Task 结果类型。</typeparam>
        /// <param name="task">被查询的任务；不能为 null。</param>
        /// <returns><see cref="TaskStatus"/> 枚举值。</returns>
        /// <exception cref="ArgumentNullException">task 为 null 时抛出。</exception>
        public static TaskStatus GetStatus<T>(Task<T> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务不能为空"));
            }

            // TCS 创建的任务初始为 WaitingForActivation（不能手动 Start）
            return task.Status;
        }

        /// <summary>
        /// 示例10：续体内联执行演示——用 <see cref="TaskContinuationOptions.ExecuteSynchronously"/>
        /// 显式要求续体在 SetResult 的调用栈上同步执行，返回 SetResult 返回前续体已执行的次数（恒为 1）。
        /// 这是“回调重入/锁重入”风险的确定性复现：默认 TCS 的普通 ContinueWith 是否内联在 net48 上
        /// 并不保证（受完成线程与调度器影响），因此需要确定内联语义时使用 ExecuteSynchronously；
        /// 对应的“绝不内联”保证见 <see cref="AsyncContinuationDepthRightAfterSetAsync"/>。
        /// </summary>
        /// <returns>SetResult 返回时已内联执行的续体计数（1）。</returns>
        public static Task<int> InlineContinuationDepthAsync()
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            int executed = 0;
            // 单独丢弃符 _ 表示刻意不等待续体任务（避免 CS4014）；
            // ExecuteSynchronously：前置任务完成时优先在完成它的线程上内联执行本续体
            _ = tcs.Task.ContinueWith(delegate { executed++; },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            TaskCompletionSource<int> done = new TaskCompletionSource<int>();

            // 用原始线程池工作项完成（而非 Task.Run），排除 DenyChildAttach 等外层任务因素
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                tcs.SetResult(0);   // 续体在本调用栈内被同步执行完
                done.TrySetResult(executed);
            });

            return done.Task;      // 恒为 1
        }

        /// <summary>
        /// 示例11：<see cref="TaskCreationOptions.RunContinuationsAsynchronously"/> 选项——
        /// SetResult 返回瞬间续体【尚未执行】，计数为 0；续体保证被排队异步执行。
        /// 与 <see cref="InlineContinuationDepthAsync"/> 对照，这正是防止“回调线程上重入执行续体”的开关。
        /// </summary>
        /// <returns>SetResult 返回瞬间的续体执行计数（0）。</returns>
        public static Task<int> AsyncContinuationDepthRightAfterSetAsync()
        {
            TaskCompletionSource<int> tcs = CreateWithAsyncContinuations<int>();
            int executed = 0;
            // 即使加了 ExecuteSynchronously，TCS 的 RunContinuationsAsynchronously 也会强制异步排队
            _ = tcs.Task.ContinueWith(delegate { executed++; },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            // 计数在完成动作所在的同一线程上立刻读取：续体只被排队、尚未执行，必为 0
            return Task.Run(delegate
            {
                tcs.SetResult(0);
                return executed;
            });
        }
    }
}
