using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="IValueTaskSource"/> / <see cref="IValueTaskSource{TResult}"/> 帮助类：
    /// ValueTask 的“可复用后端”。普通 Task 每次异步操作都 new 一个对象；而
    /// <c>new ValueTask&lt;T&gt;(source, token)</c> 构造出的 ValueTask 只是一个包着
    /// “源对象 + 版本号 token”的轻量句柄，真正的完成状态、结果、异常、续体回调全部由 source 承载。
    /// 源对象用完一次后 Reset 即可服务下一次操作——配合对象池，高频异步热路径可做到零堆分配
    /// （Socket 收发、Channel 读写、IAsyncEnumerator.MoveNextAsync 都靠这套机制）。
    /// 泛型接口 <see cref="IValueTaskSource{TResult}"/> 三个方法（非泛型 <see cref="IValueTaskSource"/>
    /// 只有 GetResult 返回 void）：
    /// ① GetStatus(short token) → <see cref="ValueTaskSourceStatus"/>：Pending(0) 进行中 /
    ///    Succeeded(1) 成功 / Faulted(2) 出错 / Canceled(3) 已取消，四态互斥；
    /// ② OnCompleted(Action&lt;object&gt; continuation, object state, short token,
    ///    <see cref="ValueTaskSourceOnCompletedFlags"/> flags)：注册“操作完成时要执行的续体”；
    ///    若注册时已完成，实现必须立即（或按 RunContinuationsAsynchronously 安排）运行续体；
    ///    flags：None(0) 无要求、FlowExecutionContext(2) 需流动 ExecutionContext、
    ///    UseSchedulingContext(1) 需捕获并使用当时的 SynchronizationContext/TaskScheduler；
    /// ③ GetResult(short token)：操作完成后由 await 底层调用——成功返回 T 结果，
    ///    出错则重新抛出 SetException 存入的异常（取消异常会被规范化为 TaskCanceledException），
    ///    未完成就调用属于使用错误。
    /// token（版本号）：每次 Reset 后 Version 加 1，ValueTask 把构造时的 token 带在身上，
    /// 源据此识别“旧 ValueTask 不许再碰新一轮状态”——这是“只能 await 一次”的底层保障。
    ///
    /// 【是否跨进程】否。纯进程内的异步状态机承载结构。
    ///
    /// 【典型适用场景】
    /// 1) 自定义高性能异步原语：一次只允许一个挂起操作、操作完成后源可 Reset 复用的场景
    ///    （连接上的下一次读、队列的下一次 Dequeue）；
    /// 2) 对象池化异步操作对象：从池里租 ManualResetValueTaskSourceCore 包装的源，
    ///    返回 new ValueTask&lt;T&gt;(source, version)，await 消费完 Reset 还池，零分配；
    /// 3) 理解 IAsyncEnumerator / Socket / Channel 返回的 ValueTask 为何不能 await 两次。
    ///    普通业务方法不要用它——默认仍返回 Task，ValueTask+自定义源只属于热路径优化。
    ///
    /// 【使用步骤（正确姿势：包 Core，不要从零手写接口）】
    /// 1) 自己的源类实现 IValueTaskSource&lt;T&gt;，内部持有一个
    ///    <see cref="ManualResetValueTaskSourceCore{TResult}"/> 字段（struct，无额外堆分配）；
    /// 2) 构造时把 core.RunContinuationsAsynchronously = true（续体扔到线程池异步执行，
    ///    避免在锁内/栈很深的回调里同步重入——绝大多数情况都应打开）；
    /// 3) 三方法直接转发：GetStatus/GetResult/OnCompleted 全部交给 core 对应方法，token 原样透传；
    /// 4) 对外暴露 SetResult(T)/SetException(Exception)/Reset()，内部转发 core；
    ///    core.SetException 即“让这一轮进入 Faulted 状态”（SetFaulted 语义）；
    /// 5) 发令方操作完成时 SetResult 或 SetException；消费方 new ValueTask&lt;T&gt;(source, source.Version)
    ///    后正常 await；
    /// 6) 只有等 ValueTask【被完整消费完】之后才允许 Reset；Reset 后 Version 递增，旧 ValueTask 全部作废；
    /// 7) 池化：Reset 后把源还池，下一轮 SetResult 前重新出租，配合 Version 做新旧代隔离。
    ///
    /// 【注意事项与坑】
    /// 1) 一个 ValueTask 只能 await 一次、不能并发 await、await 后不能再取 Result——旧 token 已失效；
    /// 2) 必须“先完整消费、再 Reset”：GetResult 还没被调用就 Reset，await 方拿到的行为未定义；
    /// 3) 不要自己手写三接口方法（版本号、ExecutionContext 流动、同步完成时续体语义都极易写错）；
    ///    永远转发 <see cref="ManualResetValueTaskSourceCore{TResult}"/>，它已处理全部细节；
    /// 4) RunContinuationsAsynchronously=false 时 SetResult 会在调用栈上【同步】执行续体，
    ///    持锁发令会导致续体在锁内重入——除非刻意为之，否则保持 true；
    /// 5) 一个源同一时刻只代表一个操作；想支持并发多操作必须每操作租一个源（池化，而非单实例排队）；
    /// 6) SetException 后 GetResult 才抛异常：SetException 本身不抛，异常在 await 端重现。
    ///
    /// 【版本可用性】<see cref="IValueTaskSource"/>、<see cref="IValueTaskSource{TResult}"/>、
    /// <see cref="ValueTaskSourceStatus"/>、<see cref="ValueTaskSourceOnCompletedFlags"/>
    /// 在 net48 由已引用的 System.Threading.Tasks.Extensions（4.6.3）提供；
    /// <see cref="ManualResetValueTaskSourceCore{TResult}"/> 在 net48 由已引用的
    /// Microsoft.Bcl.AsyncInterfaces（10.0.9）提供；.NET Core 2.1+ / .NET 5+ 全部内置于运行时。
    /// using System.Threading.Tasks.Sources; 即可，本工程 C# 7.3 下类型真实可编译。
    /// </summary>
    /// <example>
    /// 最小可用的可复用源（三方法全部转发 Core）与一次完整的“出租→发令→消费→Reset”循环：
    /// <code>
    /// using System;
    /// using System.Threading.Tasks;
    /// using System.Threading.Tasks.Sources;
    ///
    /// public sealed class ManualValueTaskSource&lt;T&gt; : IValueTaskSource&lt;T&gt;
    /// {
    ///     // struct 核心：状态、结果、异常、续体、版本号全归它管
    ///     private ManualResetValueTaskSourceCore&lt;T&gt; core =
    ///         new ManualResetValueTaskSourceCore&lt;T&gt; { RunContinuationsAsynchronously = true };
    ///
    ///     public short Version { get { return core.Version; } }
    ///
    ///     public ValueTaskSourceStatus GetStatus(short token)
    ///     {
    ///         return core.GetStatus(token);          // 转发：Pending/Succeeded/Faulted/Canceled
    ///     }
    ///
    ///     public void OnCompleted(Action&lt;object&gt; continuation, object state,
    ///                             short token, ValueTaskSourceOnCompletedFlags flags)
    ///     {
    ///         core.OnCompleted(continuation, state, token, flags);   // 转发：续体注册/上下文流动
    ///     }
    ///
    ///     public T GetResult(short token)
    ///     {
    ///         return core.GetResult(token);          // 转发：成功给结果，失败重抛异常
    ///     }
    ///
    ///     public void SetResult(T result) { core.SetResult(result); }
    ///     public void SetException(Exception error) { core.SetException(error); }
    ///     public void Reset() { core.Reset(); }       // 消费完才能调；Version 随之 +1
    /// }
    ///
    /// // 消费一轮（C# 7.3 手写 await）：
    /// ManualValueTaskSource&lt;int&gt; source = new ManualValueTaskSource&lt;int&gt;();
    /// source.SetResult(42);                                          // 发令：这一轮成功完成
    /// ValueTask&lt;int&gt; vt = new ValueTask&lt;int&gt;(source, source.Version);
    /// int answer = await vt.ConfigureAwait(false);                   // 只能 await 这一次
    /// source.Reset();                                                // 消费完毕：重置版本，准备还池复用
    /// </code>
    /// </example>
    public static class HIValueTaskSourceHelp
    {
        /// <summary>
        /// 示例1（自包含）：消费“已成功完成”的源。创建 <see cref="ManualValueTaskSource{T}"/>，
        /// SetResult(42) 使其进入 Succeeded 状态，再以当前 Version 为 token 构造 ValueTask&lt;int&gt;
        /// 并 await，验证 GetResult 路径能取回 42。
        /// </summary>
        /// <returns>await 得到的结果，恒为 42。</returns>
        public static async Task<int> ConsumeSucceededSourceAsync()
        {
            ManualValueTaskSource<int> source = new ManualValueTaskSource<int>();
            source.SetResult(42);                                    // 发令：本轮成功，结果 42

            // 用“源 + 当前版本号”构造轻量 ValueTask 句柄；Version 不匹配的旧句柄会被 core 拒绝
            ValueTask<int> valueTask = new ValueTask<int>(source, source.Version);

            int result = await valueTask.ConfigureAwait(false);      // 底层走 GetStatus→OnCompleted→GetResult
            return result;                                           // 42
        }

        /// <summary>
        /// 示例2（自包含）：消费“已失败”的源。SetException 装入
        /// <see cref="InvalidOperationException"/>，await 时 GetResult 重新抛出该异常，
        /// 用 try/catch 捕获并返回异常类型名，验证 Faulted 路径与异常原样重现。
        /// </summary>
        /// <returns>捕获到的异常类型名，恒为 "InvalidOperationException"。</returns>
        public static async Task<string> ConsumeFaultedSourceAsync()
        {
            ManualValueTaskSource<int> source = new ManualValueTaskSource<int>();

            // SetException 本身不抛异常：只让这一轮进入 Faulted 状态（即 SetFaulted 语义）
            source.SetException(new InvalidOperationException(HTranslation.GetContent("演示异常")));
            ValueTask<int> valueTask = new ValueTask<int>(source, source.Version);

            try
            {
                await valueTask.ConfigureAwait(false);               // await 端由 GetResult 重抛异常
                return string.Empty;                                 // 理论不可达
            }
            catch (Exception ex)
            {
                // 非取消类异常原样重现；这里应得到 InvalidOperationException
                return ex.GetType().Name;
            }
        }

        /// <summary>
        /// 示例3（自包含）：Reset 池化复用与版本号递增演示。第一轮 SetResult(42) 构造 ValueTask
        /// 并完整 await 消费；消费完才 Reset（Version +1），第二轮 SetResult(100) 用新版本号构造
        /// 新 ValueTask 再 await，验证同一源对象可跨轮次零分配复用，且新旧 token 互不干扰。
        /// </summary>
        /// <returns>第一轮得 42、第二轮得 100、且第二轮 Version 恰比第一轮大 1 时返回 true。</returns>
        public static async Task<bool> ResetAndReuseVersionDemoAsync()
        {
            ManualValueTaskSource<int> source = new ManualValueTaskSource<int>();

            // —— 第一轮：出租源、发令、构造句柄 ——
            source.SetResult(42);
            short version1 = source.Version;
            int first = await new ValueTask<int>(source, version1).ConfigureAwait(false);

            // —— 必须在第一轮 ValueTask 完整消费完之后才能 Reset ——
            source.Reset();                                           // 内部状态清空，Version 递增
            short version2 = source.Version;

            // —— 第二轮：同一源对象重新发令，新句柄必须携带新 token ——
            source.SetResult(100);
            int second = await new ValueTask<int>(source, version2).ConfigureAwait(false);

            // 复用验证：两轮结果都正确，且版本号严格 +1（旧 ValueTask 持有的 token 随之失效）
            return first == 42 && second == 100 && version2 == (short)(version1 + 1);
        }

        /// <summary>
        /// 教学用可复用 ValueTask 源：实现 <see cref="IValueTaskSource{TResult}"/>，
        /// 三方法全部转发内部 <see cref="ManualResetValueTaskSourceCore{TResult}"/>，
        /// 不自己处理版本号、上下文流动与续体重入。对外仅暴露发令与重置方法。
        /// </summary>
        /// <typeparam name="T">异步操作携带的结果类型。</typeparam>
        public sealed class ManualValueTaskSource<T> : IValueTaskSource<T>
        {
            // struct 核心承载全部复杂逻辑；true=续体一律异步执行，避免同步重入发令方调用栈
            private ManualResetValueTaskSourceCore<T> core =
                new ManualResetValueTaskSourceCore<T> { RunContinuationsAsynchronously = true };

            /// <summary>
            /// 当前版本号（转发 core.Version）。初始为某正值，每次 <see cref="Reset"/> 后加 1；
            /// 构造 ValueTask 时必须带上当前值，过期 token 的调用会被 core 拒绝。
            /// </summary>
            public short Version
            {
                get { return core.Version; }
            }

            /// <summary>
            /// 查询操作状态（转发 core.GetStatus）。
            /// </summary>
            /// <param name="token">ValueTask 携带的版本号，必须与当前版本一致。</param>
            /// <returns>Pending / Succeeded / Faulted / Canceled 之一。</returns>
            public ValueTaskSourceStatus GetStatus(short token)
            {
                return core.GetStatus(token);
            }

            /// <summary>
            /// 注册完成续体（转发 core.OnCompleted）。注册时若已完成，core 负责按
            /// <see cref="ManualResetValueTaskSourceCore{T}.RunContinuationsAsynchronously"/>
            /// 设置立即安排或同步执行续体。
            /// </summary>
            /// <param name="continuation">操作完成时要回调的委托。</param>
            /// <param name="state">透传给委托的状态对象。</param>
            /// <param name="token">版本号。</param>
            /// <param name="flags">上下文流动/调度上下文捕获要求。</param>
            public void OnCompleted(Action<object> continuation, object state,
                                    short token, ValueTaskSourceOnCompletedFlags flags)
            {
                core.OnCompleted(continuation, state, token, flags);
            }

            /// <summary>
            /// 取最终结果（转发 core.GetResult）：成功返回结果；失败重抛 SetException 存入的异常；
            /// 取消被规范化为 TaskCanceledException；未完成或 token 过期时抛异常。
            /// </summary>
            /// <param name="token">版本号。</param>
            /// <returns>本轮操作的结果。</returns>
            public T GetResult(short token)
            {
                return core.GetResult(token);
            }

            /// <summary>让本轮成功完成并携带指定结果（转发 core.SetResult）。</summary>
            /// <param name="result">要交给 await 方的结果。</param>
            public void SetResult(T result)
            {
                core.SetResult(result);
            }

            /// <summary>让本轮以异常失败（转发 core.SetException，即 Faulted/SetFaulted 语义）。</summary>
            /// <param name="error">await 方将在 GetResult 时重新收到的异常，不能为 null。</param>
            /// <exception cref="ArgumentNullException"><paramref name="error"/> 为 null。</exception>
            public void SetException(Exception error)
            {
                if (error == null)
                {
                    throw new ArgumentNullException(HTranslation.GetContent("异常对象不能为空"));
                }
                core.SetException(error);
            }

            /// <summary>
            /// 重置源以服务下一轮操作（转发 core.Reset）：清空状态/结果/异常并使 Version 加 1。
            /// 必须在上一轮 ValueTask 被完整 await（GetResult 已发生）之后才允许调用。
            /// </summary>
            public void Reset()
            {
                core.Reset();
            }
        }
    }
}
