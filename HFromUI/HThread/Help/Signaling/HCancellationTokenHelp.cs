using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】协作式取消帮助类，涉及两个配套类型：
    /// 1) <see cref="CancellationTokenSource"/>（CTS，发令方）：负责 <see cref="CancellationTokenSource.Cancel"/>
    ///    发起取消、<see cref="CancellationTokenSource.CancelAfter(int)"/> 定时取消、构造时直接指定超时，
    ///    并通过 <see cref="CancellationTokenSource.Token"/> 分发只读令牌，用完要 Dispose；
    /// 2) <see cref="CancellationToken"/>（CT，观察方，是 struct）：分发给工作线程/任务，
    ///    用 <see cref="CancellationToken.IsCancellationRequested"/> 轮询、
    ///    <see cref="CancellationToken.ThrowIfCancellationRequested()"/> 在取消点抛
    ///    <see cref="OperationCanceledException"/>、<see cref="CancellationToken.Register(Action)"/> 注册取消回调、
    ///    <see cref="CancellationToken.WaitHandle"/> 对接只认内核句柄的老接口。
    /// 取消是“协作式”的：调用 Cancel 不会强杀线程，任务必须自己检查令牌后主动退出。
    ///
    /// 【是否跨进程】否。令牌与注册回调都在当前进程内；WaitHandle 也是进程内匿名内核句柄。
    ///
    /// 【典型适用场景】
    /// 1) 用户点“停止/取消”按钮，通知后台任务、Task、并行循环尽快退出；
    /// 2) 超时保护：new CancellationTokenSource(ms) 或 CancelAfter(ms)，到点自动取消；
    /// 3) 组合多个取消源：<see cref="CancellationTokenSource.CreateLinkedTokenSource(CancellationToken[])"/>，
    ///    外部令牌与内部超时/内部按钮任一触发即整体取消；
    /// 4) 非 Task 的老阻塞 API：轮询 IsCancellationRequested 或等 WaitHandle。
    ///
    /// 【使用步骤】
    /// 1) using 创建 CTS（可在构造参数或 CancelAfter 中给超时）；
    /// 2) 把 cts.Token 传给工作任务，任务内周期性检查 IsCancellationRequested / ThrowIfCancellationRequested；
    /// 3) 需要取消时调用 Cancel()（可在别处、别的线程调用，线程安全）；
    /// 4) 需要清理动作时 token.Register 注册回调，或释放返回的 CancellationTokenRegistration 提前注销；
    /// 5) 多个取消源组合时创建 linked CTS（它也要单独 Dispose）；
    /// 6) using 结束释放 CTS。
    ///
    /// 【注意事项与坑】
    /// 1) Cancel 后不可恢复：CTS 是一次性的，需要再次取消要新建实例；
    /// 2) Register 的回调默认在“调用 Cancel 的线程”上同步执行，回调里不要做长时间阻塞或重入 Cancel；
    ///    传 useSynchronizationContext:true 且注册时捕获到 SynchronizationContext 时才会 Post 回该上下文；
    /// 3) Dispose CTS 不取消任务，也不会自动注销回调；要先 Cancel 再 Dispose；
    ///    令牌在 CTS Dispose 后仍可安全访问 IsCancellationRequested，但访问 WaitHandle 会抛 ObjectDisposedException；
    /// 4) 链接令牌产生的 linked CTS 必须单独 Dispose；任一源取消会同时取消链接源（反之不会）；
    /// 5) CancellationToken.None（以及 default(CancellationToken)）的 CanBeCanceled 永远为 false，
    ///    是表达“不可取消”的标准空对象，不要传 null 令牌；
    /// 6) Task 体内抛出的 OperationCanceledException 只有其 Token 与传入 Task 的令牌匹配时才被识别为“正常取消”，
    ///    否则被当作故障。
    ///
    /// 【版本可用性】.NET Framework 4.0 起提供 CTS/Token 全部上述成员；
    /// CancellationToken.UnsafeRegister(Action{Object}, Object) 仅 .NET Core 3.0/.NET 5+ 提供，
    /// .NET Framework 4.8 【没有】此方法，只能用 Register（本文件仅以注释示例给出，见下）。
    /// </summary>
    /// <example>
    /// 标准可取消后台循环：
    /// <code>
    /// using (CancellationTokenSource cts = new CancellationTokenSource())
    /// {
    ///     CancellationToken token = cts.Token;
    ///     Task task = Task.Run(delegate
    ///     {
    ///         while (!token.IsCancellationRequested)
    ///         {
    ///             token.ThrowIfCancellationRequested();   // 或在取消点直接抛 OCE
    ///             Thread.Sleep(50);
    ///         }
    ///     }, token);
    ///     cts.CancelAfter(1000);   // 1 秒后自动取消
    ///     task.Wait(2000);
    /// }
    /// </code>
    /// .NET Core 3.0+/.NET 5+ 才有的 UnsafeRegister（net48 不可编译，仅供迁移参考，
    /// 它不通过 ExecutionContext 流动，回调内拿不到 AsyncLocal 值）：
    /// <code>
    /// // CancellationTokenRegistration reg =
    /// //     token.UnsafeRegister(delegate(object state) { /* 清理 */ }, null);
    /// </code>
    /// </example>
    public static class HCancellationTokenHelp
    {
        /// <summary>不可取消的空令牌（<see cref="CancellationToken.None"/>），等价于 default(CancellationToken)，
        /// 其 CanBeCanceled 恒为 false。用于调用方不需要取消能力时，避免传 null。</summary>
        public static CancellationToken NoneToken
        {
            get { return CancellationToken.None; }
        }

        /// <summary>
        /// 示例1：可取消的后台循环：周期检查 <see cref="CancellationToken.IsCancellationRequested"/> 后自行退出。
        /// </summary>
        /// <param name="token">取消令牌；传 <see cref="CancellationToken.None"/> 表示永不取消。</param>
        /// <returns>后台循环任务；取消后任务以 RanToCompletion 状态正常结束（本示例不抛异常）。</returns>
        public static Task RunLoop(CancellationToken token)
        {
            return Task.Run(delegate
            {
                while (!token.IsCancellationRequested)
                {
                    // 业务循环
                    Thread.Sleep(50);
                }
            }, token);
        }

        /// <summary>
        /// 示例2：在取消点直接抛 <see cref="OperationCanceledException"/>（调用方可统一 catch 处理取消）。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <exception cref="OperationCanceledException">令牌已被取消时抛出。</exception>
        public static void WorkWithThrow(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
        }

        /// <summary>查询令牌是否已收到取消请求（<see cref="CancellationToken.IsCancellationRequested"/>）。</summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>已请求取消为 true。</returns>
        public static bool IsCancellationRequested(CancellationToken token)
        {
            return token.IsCancellationRequested;
        }

        /// <summary>
        /// 查询令牌是否“可能被取消”（<see cref="CancellationToken.CanBeCanceled"/>）。
        /// <see cref="CancellationToken.None"/> 恒返回 false；由真实 CTS 分发的令牌返回 true。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>令牌背后存在可触发的 CTS 时为 true。</returns>
        public static bool CanBeCanceled(CancellationToken token)
        {
            return token.CanBeCanceled;
        }

        /// <summary>
        /// 示例3：创建“到点自动取消”的 CTS（构造函数 <c>CancellationTokenSource(int millisecondsDelay)</c>）。
        /// 从构造完成起计时，到时自动 Cancel；也可提前手动 Cancel。
        /// </summary>
        /// <param name="timeoutMs">多少毫秒后自动取消，必须 ≥ 0。</param>
        /// <returns>已设置定时取消的 CTS；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为负数。</exception>
        public static CancellationTokenSource CreateTimeoutSource(int timeoutMs)
        {
            return new CancellationTokenSource(timeoutMs);
        }

        /// <summary>
        /// 示例3 的 TimeSpan 版本：构造函数 <c>CancellationTokenSource(TimeSpan delay)</c>。
        /// </summary>
        /// <param name="delay">自动取消前的延迟，不能为负。</param>
        /// <returns>已设置定时取消的 CTS；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> 为负。</exception>
        public static CancellationTokenSource CreateTimeoutSource(TimeSpan delay)
        {
            return new CancellationTokenSource(delay);
        }

        /// <summary>
        /// 示例4：演示 <see cref="CancellationTokenSource.Cancel()"/> 的即时效果与轮询观察。
        /// 工作线程每 20ms 检查一次令牌，Cancel 后很快自行退出。全程带超时，自测安全。
        /// </summary>
        /// <returns>工作线程是否在 2000ms 内观察到取消并退出。</returns>
        public static bool CancelExample()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                bool exitedCleanly = false;
                Thread worker = new Thread((ThreadStart)delegate
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        Thread.Sleep(20);          // 协作式：自己检查，自己退出
                    }
                    exitedCleanly = true;
                }) { IsBackground = true };
                worker.Start();

                Thread.Sleep(50);
                cts.Cancel();                      // 发起取消（线程安全，任何线程可调）

                bool joined = worker.Join(2000);   // 带超时回收
                return joined && exitedCleanly && cts.IsCancellationRequested;
            }
        }

        /// <summary>
        /// 示例5：带异常聚合选项的 Cancel（<see cref="CancellationTokenSource.Cancel(bool)"/>）。
        /// throwOnFirstException=true 时，注册回调中第一个异常立即抛出（回调可能中断）；
        /// false（默认）时所有回调都执行，异常聚合成 AggregateException 抛出。
        /// </summary>
        /// <param name="throwOnFirstException">是否在回调抛出第一个异常时立即向外抛。</param>
        public static void CancelWithOption(bool throwOnFirstException)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Token.Register(delegate { Console.WriteLine(HTranslation.GetContent("取消回调执行清理")); });
                cts.Cancel(throwOnFirstException);
            }
        }

        /// <summary>
        /// 示例6：创建 CTS 后再安排定时取消（<see cref="CancellationTokenSource.CancelAfter(int)"/>）。
        /// 与构造超时的区别是可以在运行中根据情况随时设定；重复调用会以最后一次计时为准。
        /// </summary>
        /// <param name="timeoutMs">多少毫秒后取消，必须 ≥ 0（0 表示立即取消）。</param>
        /// <returns>带定时取消的 CTS；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为负数。</exception>
        public static CancellationTokenSource CancelAfterExample(int timeoutMs)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(timeoutMs);
            return cts;
        }

        /// <summary>
        /// 示例6 的 <see cref="TimeSpan"/> 版本（<see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/>）。
        /// </summary>
        /// <param name="delay">取消延迟，不能为负。</param>
        /// <returns>带定时取消的 CTS；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><param name="delay"/> 为负。</exception>
        public static CancellationTokenSource CancelAfterExample(TimeSpan delay)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(delay);
            return cts;
        }

        /// <summary>
        /// 示例7：注册取消回调（<see cref="CancellationToken.Register(Action)"/>）。
        /// 若注册时令牌已取消，回调会立即（同步）执行；否则在 Cancel 调用线程上同步执行。
        /// 返回的 registration 可 Dispose 以提前注销。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <param name="cleanup">取消时执行的清理动作，不能为 null。</param>
        /// <returns>回调注册句柄；Dispose 它可提前注销回调。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cleanup"/> 为 null。</exception>
        public static CancellationTokenRegistration OnCancel(CancellationToken token, Action cleanup)
        {
            if (cleanup == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("清理委托不能为空"));
            }
            return token.Register(cleanup);
        }

        /// <summary>
        /// 示例8：带状态对象的回调注册（<see cref="CancellationToken.Register(Action{Object}, Object)"/>），
        /// 避免把状态闭包进回调（减少委托分配）；状态作为参数传给回调。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <param name="cleanup">取消时执行的清理动作，参数为 <paramref name="state"/>。</param>
        /// <param name="state">传给回调的状态对象，可为 null。</param>
        /// <returns>回调注册句柄；Dispose 可提前注销。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cleanup"/> 为 null。</exception>
        public static CancellationTokenRegistration OnCancelWithState(CancellationToken token, Action<object> cleanup, object state)
        {
            if (cleanup == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("清理委托不能为空"));
            }
            return token.Register(cleanup, state);
        }

        /// <summary>
        /// 示例9：可选择同步上下文的回调注册（<see cref="CancellationToken.Register(Action, bool)"/>）。
        /// useSynchronizationContext=true 且注册时当前线程持有 <see cref="System.Threading.SynchronizationContext"/>
        /// （如 UI 线程）时，回调 Post 回该上下文执行；否则在 Cancel 调用线程同步执行。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <param name="cleanup">取消时执行的回调。</param>
        /// <param name="useSynchronizationContext">是否把回调送回注册时捕获的 SynchronizationContext。</param>
        /// <returns>回调注册句柄。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cleanup"/> 为 null。</exception>
        public static CancellationTokenRegistration OnCancel(CancellationToken token, Action cleanup, bool useSynchronizationContext)
        {
            if (cleanup == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("清理委托不能为空"));
            }
            return token.Register(cleanup, useSynchronizationContext);
        }

        /// <summary>
        /// 示例10：通过 <see cref="CancellationToken.WaitHandle"/> 等待取消信号（适合只接受内核句柄的老接口）。
        /// 该句柄在令牌取消时变为有信号；其生命周期由 CTS 管理，不要 Dispose 它。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <param name="timeoutMs">等待超时毫秒；建议自测传小值。</param>
        /// <returns>超时前令牌被取消返回 true；超时返回 false。</returns>
        /// <exception cref="ObjectDisposedException">
        /// 底层 CTS 已 Dispose（WaitHandle 是惰性创建的内核对象，CTS 释放后访问会抛此异常）。
        /// </exception>
        public static bool WaitCanceled(CancellationToken token, int timeoutMs)
        {
            return token.WaitHandle.WaitOne(timeoutMs);
        }

        /// <summary>
        /// 示例11：组合两个令牌（<see cref="CancellationTokenSource.CreateLinkedTokenSource(CancellationToken, CancellationToken)"/>）。
        /// 任一令牌取消，链接源即取消；链接源本身也可独立 Cancel。返回的链接 CTS 必须单独 Dispose。
        /// </summary>
        /// <param name="token1">外部取消令牌 1。</param>
        /// <param name="token2">外部取消令牌 2。</param>
        /// <returns>与两个入参链接的新 CTS；调用方负责 Dispose。</returns>
        public static CancellationTokenSource CreateLinked(CancellationToken token1, CancellationToken token2)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(token1, token2);
        }

        /// <summary>
        /// 示例12：外部令牌或内部超时任一触发都取消（params 数组版 CreateLinkedTokenSource + CancelAfter）。
        /// </summary>
        /// <param name="outer">外部取消令牌（如上层请求的取消）。</param>
        /// <param name="timeoutMs">内部超时毫秒。</param>
        /// <returns>链接 CTS；用完必须 Dispose。</returns>
        public static CancellationTokenSource LinkWithTimeout(CancellationToken outer, int timeoutMs)
        {
            CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(outer);
            linked.CancelAfter(timeoutMs);
            return linked;
        }

        /// <summary>
        /// 示例13：异步等待取消信号——用 <see cref="TaskCompletionSource{TResult}"/> 把回调模型转成 await 模型，
        /// 避免阻塞线程；比直接等 WaitHandle 更适合 async 代码。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>令牌取消时完成的 Task；若注册时令牌已取消则几乎立即完成。</returns>
        public static Task WaitForCancelAsync(CancellationToken token)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            CancellationTokenRegistration registration = token.Register(delegate
            {
                tcs.TrySetResult(true);   // 取消回调里放行等待者
            });

            // 任务完成后注销回调，避免回调持有 Task 导致泄漏
            tcs.Task.ContinueWith(delegate (Task t) { registration.Dispose(); }, TaskScheduler.Default);
            return tcs.Task;
        }

        /// <summary>
        /// 示例14：CTS 标准生命周期模板：创建→分发 Token→工作→（取消）→Dispose。
        /// 工作正常结束时也应 Dispose；Dispose 不会触发取消，仅释放内部内核句柄等资源。
        /// </summary>
        /// <param name="work">使用令牌执行的业务逻辑，不能为 null。</param>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        public static void UseAndDispose(Action<CancellationToken> work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("工作委托不能为空"));
            }

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                work(cts.Token);     // 业务自行决定何时检查/抛出；需要时外部可保存 cts 引用以 Cancel
            }   // 离开 using 自动 Dispose
        }
    }
}
