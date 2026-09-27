using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】异步流帮助类，涉及两个接口（<see cref="IAsyncEnumerable{T}"/> /
    /// <see cref="IAsyncEnumerator{T}"/>）与 <see cref="IAsyncDisposable"/>：
    /// - IAsyncEnumerable&lt;T&gt;：可异步枚举的序列，只有一个方法
    ///   IAsyncEnumerator&lt;T&gt; GetAsyncEnumerator(CancellationToken)；
    /// - IAsyncEnumerator&lt;T&gt;：枚举器，三个成员——
    ///   ValueTask&lt;bool&gt; MoveNextAsync()（推进到下一条，false=序列结束）、
    ///   T Current（当前元素）、ValueTask DisposeAsync()（释放，如取消网络分页请求）；
    /// - 扩展方法 WithCancellation(CancellationToken)（TaskAsyncEnumerableExtensions 提供）把令牌传入枚举过程，
    ///   同命名空间还提供 ConfigureAwait(bool) 扩展控制枚举续体上下文。
    /// 对比 Task&lt;IEnumerable&gt;：那是“整批就绪后一次返回”；IAsyncEnumerable 是“来一条给一条”的流式序列。
    ///
    /// 【是否跨进程】否（跨进程流需 gRPC/SignalR 等传输承载，本接口本身只是进程内枚举契约）。
    ///
    /// 【典型适用场景】分页 API/数据库游标逐页拉取、日志/消息实时消费、大文件逐行读取、
    /// 服务端逐条产出（数据不必全部进内存）；需要中途取消就传 CancellationToken。
    ///
    /// 【使用步骤（net48 手写，不依赖 C# 8 语法）】
    /// 1) 序列类实现 IAsyncEnumerable&lt;T&gt;，每次 GetAsyncEnumerator 返回一个【新的】枚举器；
    /// 2) 枚举器实现 IAsyncEnumerator&lt;T&gt;：Current 保存当前值；MoveNextAsync 内做异步等待、
    ///    检查取消令牌、推进下标并返回是否还有数据；DisposeAsync 做清理并返回 default(ValueTask)；
    /// 3) 消费方手动循环：GetAsyncEnumerator → try/finally(await DisposeAsync) →
    ///    while (await MoveNextAsync()) 处理 Current；
    /// 4) 需要取消用 source.WithCancellation(token)（令牌经 GetAsyncEnumerator 参数进入枚举器）。
    ///
    /// 【注意事项与坑】
    /// 1) GetAsyncEnumerator 每次必须返回全新枚举器（不能把 this 当枚举器反复返回同一状态实例），
    ///    否则同一个序列无法被枚举第二次；
    /// 2) MoveNextAsync 返回 false 后 Current 无意义；枚举器用完必须 DisposeAsync（用 try/finally）；
    /// 3) 取消的标准行为：在 MoveNextAsync 中抛 OperationCanceledException，而非返回 false；
    /// 4) 枚举过程中不要缓存整条序列，否则失去“流式”意义；
    /// 5) await foreach 消费、async yield return 生产、[EnumeratorCancellation] 特性需要 C# 8.0 编译器，
    ///    本工程固定 C# 7.3，仅在注释中给出（见示例 XML）。
    ///
    /// 【版本可用性】接口在 net48 经 Microsoft.Bcl.AsyncInterfaces NuGet 包提供（本工程已引用，
    /// 同时带来 WithCancellation/ConfigureAwait 扩展与 IAsyncDisposable）；.NET Standard 2.1 /
    /// .NET Core 3.0+ / .NET 5+ 内置。await foreach / yield return 异步迭代器是 C# 8.0 语言特性，
    /// net48 即使装了包也必须把 LangVersion 升到 8 才能用——本类不使用该语法。
    /// </summary>
    ///
    /// <example>
    /// 【C# 8.0 消费写法（net48 需额外 &lt;LangVersion&gt;8&lt;/LangVersion&gt;，本工程未开启，仅注释演示）】
    /// <code>
    /// await foreach (int item in range.WithCancellation(token))
    /// {
    ///     Console.WriteLine(item);
    /// }
    /// </code>
    /// 【C# 8.0 生产端：异步迭代器 + [EnumeratorCancellation]，让 WithCancellation 传入的令牌
    /// 能在【迭代器体内部】被观察到；using System.Threading.Tasks 之外还需
    /// using System.Runtime.CompilerServices】
    /// <code>
    /// public async IAsyncEnumerable&lt;int&gt; ProduceAsync(
    ///     [EnumeratorCancellation] CancellationToken token = default)
    /// {
    ///     for (int i = 0; i &lt; 10; i++)
    ///     {
    ///         await Task.Delay(100, token);
    ///         yield return i;   // 流式产出：来一条、枚举方就能先消费一条
    ///     }
    /// }
    /// // await using (C# 8) 等价于本文件 ConsumeAsync 里的 try/finally + DisposeAsync：
    /// // await using (IAsyncEnumerator&lt;int&gt; e = src.GetAsyncEnumerator(token)) { ... }
    /// </code>
    /// </example>
    public static class HIAsyncEnumerableHelp
    {
        /// <summary>
        /// 手写的异步序列：产生 0..count-1 的整数流，每条之间有极短异步等待（模拟逐条就绪）。
        /// 序列对象只负责“生产枚举器”，枚举状态放在独立的 <see cref="AsyncRangeEnumerator"/> 中，
        /// 因此本序列可被反复枚举。
        /// </summary>
        public sealed class AsyncRange : IAsyncEnumerable<int>
        {
            private readonly int count;

            /// <summary>
            /// 构造异步序列。
            /// </summary>
            /// <param name="count">要产生的元素个数（0..count-1）；负数按 0 处理。</param>
            public AsyncRange(int count)
            {
                this.count = count < 0 ? 0 : count;
            }

            /// <summary>
            /// 返回一个全新的异步枚举器。
            /// </summary>
            /// <param name="cancellationToken">取消令牌；消费方 WithCancellation 或直接传参进入此处。</param>
            /// <returns>从头开始的新枚举器。</returns>
            public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
            {
                // 关键：每次返回新实例，保证多次枚举互不干扰
                return new AsyncRangeEnumerator(count, cancellationToken);
            }
        }

        /// <summary>
        /// <see cref="AsyncRange"/> 的枚举器：保存下标等全部枚举状态；一个实例只服务一次枚举。
        /// </summary>
        public sealed class AsyncRangeEnumerator : IAsyncEnumerator<int>
        {
            private readonly int count;
            private readonly CancellationToken token;
            private int current = -1;

            /// <summary>
            /// 构造枚举器。
            /// </summary>
            /// <param name="count">元素总数。</param>
            /// <param name="token">枚举过程的取消令牌。</param>
            public AsyncRangeEnumerator(int count, CancellationToken token)
            {
                this.count = count;
                this.token = token;
            }

            /// <summary>
            /// 获取当前元素（仅在 MoveNextAsync 返回 true 之后有效）。
            /// </summary>
            public int Current
            {
                get { return current; }
            }

            /// <summary>
            /// 推进到下一元素。
            /// </summary>
            /// <returns>还有元素返回 true；已到序列末尾返回 false。</returns>
            /// <exception cref="OperationCanceledException">枚举被取消时抛出（取消与“正常结束”区分开）。</exception>
            public async ValueTask<bool> MoveNextAsync()
            {
                // 先检查取消：取消是“抛异常”，不是返回 false
                token.ThrowIfCancellationRequested();

                await Task.Delay(1, token).ConfigureAwait(false);   // 模拟逐条异步就绪
                if (current + 1 >= count)
                {
                    return false;                                   // 序列结束
                }

                current++;
                return true;
            }

            /// <summary>
            /// 释放枚举器（本示例无托管资源需清理，返回已完成的 ValueTask）。
            /// </summary>
            /// <returns>表示释放完成的 ValueTask。</returns>
            public ValueTask DisposeAsync()
            {
                // 真实场景此处可取消进行中的分页请求、关闭网络流等
                return default(ValueTask);
            }
        }

        /// <summary>
        /// 示例1：net48 下不使用 await foreach 的等价消费循环（await 一个 ValueTask&lt;bool&gt; 即可）。
        /// </summary>
        /// <param name="count">枚举元素个数（自测传小值）。</param>
        /// <returns>枚举完成的 Task。</returns>
        public static async Task ConsumeAsync(int count)
        {
            AsyncRange source = new AsyncRange(count);
            IAsyncEnumerator<int> enumerator = source.GetAsyncEnumerator();
            try
            {
                // 逐条处理：不必等所有数据就绪，来一条就能先消费一条
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    int item = enumerator.Current;
                    GC.KeepAlive(item);
                }
            }
            finally
            {
                // 对应 C# 8 的 await using：无论正常结束/异常/取消都必须释放枚举器
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 示例2：通过 WithCancellation 扩展把取消令牌送入枚举（令牌经 GetAsyncEnumerator 到达枚举器）。
        /// </summary>
        /// <param name="count">元素个数。</param>
        /// <param name="token">取消令牌；在序列中途取消会使 MoveNextAsync 抛 OperationCanceledException。</param>
        /// <returns>枚举完成的 Task。</returns>
        /// <exception cref="OperationCanceledException">token 被取消时抛出。</exception>
        public static async Task ConsumeWithCancellationAsync(int count, CancellationToken token)
        {
            AsyncRange source = new AsyncRange(count);

            // C# 7.3 手写循环里，直接把令牌传给 GetAsyncEnumerator(token) 即可，
            // 语义与 C# 8 的 “await foreach (x in source.WithCancellation(token))” 完全等价；
            // WithCancellation 只是把令牌存进 ConfiguredCancelableAsyncEnumerable 这个 struct 的语法糖，
            // 其枚举器按鸭子类型返回 ConfigureAwait 包装的 awaitable，不直接实现 IAsyncEnumerator 接口。
            IAsyncEnumerator<int> enumerator = source.GetAsyncEnumerator(token);
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    GC.KeepAlive(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 示例3：把异步流“物化”为内存列表（仅适用于元素总量可控的场景，否则失去流式优势）。
        /// </summary>
        /// <param name="source">异步序列；不能为 null。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>按枚举顺序填充的 List&lt;int&gt;。</returns>
        /// <exception cref="ArgumentNullException">source 为 null 时抛出。</exception>
        /// <exception cref="OperationCanceledException">token 被取消时抛出。</exception>
        public static async Task<List<int>> ToListAsync(IAsyncEnumerable<int> source, CancellationToken token = default(CancellationToken))
        {
            if (source == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("源不能为空"));
            }

            List<int> result = new List<int>();
            IAsyncEnumerator<int> enumerator = source.GetAsyncEnumerator(token);
            try
            {
                while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    result.Add(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            return result;
        }
    }
}
