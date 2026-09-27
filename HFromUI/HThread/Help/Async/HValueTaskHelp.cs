using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    /// <summary>
    /// 【是什么】<see cref="ValueTask"/> / <see cref="ValueTask{TResult}"/> 帮助类：可携带异步结果的值类型，
    /// 用于在“绝大多数调用同步完成”的热路径上消除 Task 对象的堆分配。它是 struct：
    /// 同步完成时内部只装一个 T 结果（零堆分配）；异步完成时内部包装 Task 或 IValueTaskSource。
    /// 常用成员：Result（仅已成功完成时可取）、IsCompleted / IsCompletedSuccessfully / IsFaulted /
    /// IsCanceled、AsTask() 转普通 Task、Preserve() 生成可被多次安全等待的副本、ConfigureAwait(bool)。
    ///
    /// 【ValueTask 三条铁律（违反属未定义行为，可能得到错误结果或异常）】
    /// 1) 一个 ValueTask 【只能 await 一次】——await 会消费底层 IValueTaskSource 的版本号；
    /// 2) 【不能并发 await】同一 ValueTask 不能同时被多个地方等待；
    /// 3) 调过 AsTask()/GetAwaiter().GetResult() 后【不能再 await 原对象】，也不能几种用法混用；
    ///    需要多次等待、并发等待、组合（WhenAll）时：要么调用 <see cref="ValueTask{TResult}.Preserve"/>
    ///    后只操作其返回值，要么方法直接返回普通 Task。
    ///
    /// 【是否跨进程】否。
    ///
    /// 【典型适用场景】
    /// 1) 读缓存/读池：命中时同步返回、未命中才走真异步（ValueTask 的主场）；
    /// 2) IAsyncEnumerator.MoveNextAsync/DisposeAsync 等接口签名（见 HIAsyncEnumerableHelp）；
    /// 3) 高频调用的异步基元（Socket 收发、Channel 读取）——底层通常配合 IValueTaskSource 复用对象池。
    ///    普通业务方法【不要】为了“时髦”返回 ValueTask：它不可组合、误用代价高，默认仍应返回 Task。
    ///
    /// 【使用步骤】
    /// 1) 同步成功：return new ValueTask&lt;T&gt;(result)（或 C# 7.3 下显式 return default(ValueTask)）；
    /// 2) 同步失败：new ValueTask&lt;T&gt;(Task.FromException(...))（不要在异步方法里直接 throw 破坏契约，
    ///    async ValueTask 方法除外，它由状态机保证）；
    /// 3) 异步路径：new ValueTask&lt;T&gt;(innerTask)；
    /// 4) 消费方只 await 一次；需要先探测同步完成可用 IsCompletedSuccessfully + Result；
    /// 5) 需要多次/并发消费先 Preserve()。
    ///
    /// 【注意事项与坑】
    /// 1) 不要对未完成的 ValueTask 取 Result（阻塞且语义未定义）；先判断 IsCompletedSuccessfully；
    /// 2) Preserve 返回的是“包装后的可复用句柄”，原 ValueTask 仍按铁律不可再用；
    /// 3) 作为字段/参数存储 ValueTask 会发生值拷贝，拷贝出的是同一底层源的两个句柄，依旧不许各 await 一次；
    /// 4) async Task 与 async ValueTask 的状态机成本不同：纯异步路径 ValueTask 反而多一层包装，
    ///    只有“同步完成占多数”时才划算。
    ///
    /// 【版本可用性】net48 通过 NuGet 包 System.Threading.Tasks.Extensions 提供（本工程已 HintPath 引用
    /// 4.6.3，程序集版本 4.2.4.0）：ValueTask/ValueTask&lt;T&gt;、Preserve、AsTask、
    /// IsCompleted/IsCompletedSuccessfully/IsFaulted/IsCanceled、IValueTaskSource 均在该包内可用；
    /// .NET Core 2.0+/Mono/.NET 5+ 内置。System.Threading.Tasks.Sources 命名空间下的
    /// IValueTaskSource / ManualResetValueTaskSourceCore&lt;T&gt; 高级用法见本类注释（见相关方法 XML 注释）。
    /// </summary>
    ///
    /// <example>
    /// 推荐的“同步快路径 + 异步慢路径”写法：
    /// <code>
    /// public ValueTask&lt;int&gt; ReadAsync(CancellationToken token)
    /// {
    ///     int cached;
    ///     if (TryGetCache(out cached))
    ///     {
    ///         return new ValueTask&lt;int&gt;(cached);   // 零分配
    ///     }
    ///     return new ValueTask&lt;int&gt;(LoadFromSourceAsync(token));
    /// }
    /// </code>
    /// </example>
    public static class HValueTaskHelp
    {
        private static int cachedValue;
        private static bool hasCache;

        /// <summary>
        /// 示例1：缓存命中同步完成、未命中才走真异步——ValueTask 的主场。
        /// </summary>
        /// <param name="token">取消令牌，仅异步路径使用。</param>
        /// <returns>命中缓存时同步完成、零分配；否则包装一个真 Task。</returns>
        public static ValueTask<int> GetValueAsync(CancellationToken token)
        {
            if (hasCache)
            {
                // 同步路径：直接用结果构造值类型，无任何堆分配
                return new ValueTask<int>(cachedValue);
            }

            // 异步路径：内部仍走普通 Task，ValueTask 只作包装
            return new ValueTask<int>(LoadFromSourceAsync(token));
        }

        /// <summary>
        /// 示例2：无返回值的轻量异步（非泛型 ValueTask 同样只能 await 一次）。
        /// </summary>
        /// <param name="ready">true 表示已同步就绪，返回已完成实例。</param>
        /// <returns>就绪返回 default(ValueTask)；未就绪返回延时 1ms 的 ValueTask。</returns>
        public static ValueTask SendAsyncIfReady(bool ready)
        {
            if (ready)
            {
                return default(ValueTask);   // 等价“已同步完成的 void 异步操作”
            }

            return new ValueTask(Task.Delay(1));
        }

        /// <summary>
        /// 示例3：需要组合/多次等待时，先 <see cref="ValueTask{TResult}.AsTask"/> 转成普通 Task 再用。
        /// 转换后原 ValueTask 立即作废（铁律3）。
        /// </summary>
        /// <returns>组合结果。</returns>
        public static async Task<int> ConsumeSafelyAsync()
        {
            // AsTask 后只持有 Task，可自由 WhenAll/多次 await；不要再碰原来的 ValueTask
            Task<int> t = GetValueAsync(CancellationToken.None).AsTask();
            Task<int[]> all = Task.WhenAll(t, Task.FromResult(0));
            return (await all.ConfigureAwait(false))[0];
        }

        /// <summary>
        /// 示例4：判断是否【同步成功完成】（IsCompletedSuccessfully）。
        /// 已完成还包含“已失败/已取消”，判断“可安全取 Result”必须用 IsCompletedSuccessfully。
        /// </summary>
        /// <param name="valueTask">被检查的 ValueTask&lt;int&gt;。</param>
        /// <returns>已成功完成（可取 Result）返回 true。</returns>
        public static bool IsCompletedSync(ValueTask<int> valueTask)
        {
            return valueTask.IsCompletedSuccessfully;
        }

        /// <summary>
        /// 示例5：完成状态四态判定，返回可读描述（调试/自测用）。
        /// 四态互斥：成功完成 / 已失败(IsFaulted) / 已取消(IsCanceled) / 进行中(IsCompleted=false)。
        /// </summary>
        /// <param name="valueTask">被检查的 ValueTask&lt;int&gt;。</param>
        /// <returns>Running / Faulted / Canceled / RanToCompletion 之一。</returns>
        public static string DescribeStatus(ValueTask<int> valueTask)
        {
            // 先判成功，再判失败、取消，最后才是进行中
            if (valueTask.IsCompletedSuccessfully)
            {
                return "RanToCompletion";
            }

            if (valueTask.IsFaulted)
            {
                return "Faulted";
            }

            if (valueTask.IsCanceled)
            {
                return "Canceled";
            }

            return "Running";
        }

        /// <summary>
        /// 示例6：非阻塞尝试取结果。只有同步成功完成时才取 Result，绝不阻塞等待。
        /// </summary>
        /// <param name="valueTask">被尝试消费的 ValueTask&lt;int&gt;（仅在成功完成时被消费）。</param>
        /// <param name="result">同步成功时为其结果；否则为 0。</param>
        /// <returns>成功拿到同步结果返回 true。</returns>
        public static bool TryGetResult(ValueTask<int> valueTask, out int result)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                // 此时取 Result 立即返回、不阻塞
                result = valueTask.Result;
                return true;
            }

            result = 0;
            return false;
        }

        /// <summary>
        /// 示例7：<see cref="ValueTask{TResult}.Preserve"/>——需要多次 await 或既要判断状态又要 await 时，
        /// 先 Preserve 得到一个可重复 await 的句柄（本示例 await 两次，合法）。
        /// </summary>
        /// <returns>两次 await 得到的结果之和。</returns>
        public static async Task<int> PreserveAndAwaitTwiceAsync()
        {
            // Preserve 返回包装 ValueTask；之后只操作 preserved，原对象不再使用
            ValueTask<int> preserved = GetValueAsync(CancellationToken.None).Preserve();
            int first = await preserved.ConfigureAwait(false);
            int second = await preserved.ConfigureAwait(false);   // 没有 Preserve 这是非法用法
            return first + second;
        }

        /// <summary>
        /// 示例8：显式同步成功工厂（测试与“缓存已命中”场景用），等价 new ValueTask&lt;int&gt;(result)。
        /// </summary>
        /// <param name="result">要携带的同步结果。</param>
        /// <returns>已成功完成的 ValueTask&lt;int&gt;。</returns>
        public static ValueTask<int> FromCompletedResult(int result)
        {
            return new ValueTask<int>(result);
        }

        /// <summary>
        /// 清空静态缓存（仅供单元测试隔离状态使用）。
        /// </summary>
        public static void ResetCacheForTest()
        {
            hasCache = false;
            cachedValue = 0;
        }

        /// <summary>
        /// 模拟异步数据源：小延时后写缓存并返回。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>加载结果。</returns>
        /// <exception cref="OperationCanceledException">token 取消时由 Task.Delay 抛出。</exception>
        private static async Task<int> LoadFromSourceAsync(CancellationToken token)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            cachedValue = 42;
            hasCache = true;
            return cachedValue;
        }

        // ======================================================================
        // 【注释-only 高级内容】IValueTaskSource / IValueTaskSource<T>（.NET Core 2.1+ 概念，
        // 接口类型在 net48 下由 System.Threading.Tasks.Extensions 包提供，但【手写实现】属于高级用法：
        // 必须自己管理版本号(version token)、完成状态、续体回调，实现错误会产生悬空续体或结果错乱）。
        // 生产中几乎不手写接口，而是派生/包装 ManualResetValueTaskSourceCore<T>（该核心类型在 net48 下
        // 由 Microsoft.Bcl.AsyncInterfaces 包提供；.NET Core 2.1+ 内置）。Socket/Channel 等高性能 API
        // 即靠“池化 IValueTaskSource + ValueTask”实现每次异步零分配。骨架仅供阅读：
        //
        // private sealed class DelayValueTaskSource : IValueTaskSource<int>
        // {
        //     private ManualResetValueTaskSourceCore<int> core =
        //         new ManualResetValueTaskSourceCore<int> { RunContinuationsAsynchronously = true };
        //
        //     public ValueTask<int> Task { get { return new ValueTask<int>(this, core.Version); } }
        //
        //     public int GetResult(short token)
        //     {
        //         return core.GetResult(token);   // 未完成会抛 InvalidOperationException
        //     }
        //
        //     public ValueTaskSourceStatus GetStatus(short token)
        //     {
        //         return core.GetStatus(token);   // Pending/Succeeded/Faulted/Canceled
        //     }
        //
        //     public void OnCompleted(Action<object> continuation, object state,
        //                             short token, ValueTaskSourceOnCompletedFlags flags)
        //     {
        //         core.OnCompleted(continuation, state, token, flags);
        //     }
        //
        //     public void SetResult(int result) { core.SetResult(result); }
        //     public void Reset() { core.Reset(); }   // 归还对象池前必须重置版本号
        // }
        // ======================================================================
    }
}
