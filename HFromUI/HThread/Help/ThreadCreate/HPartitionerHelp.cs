using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    using HFromUI.HLangage;
    /// <summary>
    /// Partitioner 帮助类：把大批量数据切成分区（chunk）交给 Parallel/PLINQ 并行处理。
    /// 【是什么】System.Collections.Concurrent.Partitioner 是静态工厂类，生产 Partitioner&lt;T&gt; /
    /// OrderablePartitioner&lt;TSource&gt;（可排序、可追踪元素索引的分区器）。它把一个整数区间或一个
    /// 数组/列表切成若干块：Parallel.ForEach 拿到的是“整块”而不是逐个元素，每个分区内部用普通
    /// for/foreach 串行处理，从而在“并行负载均衡”和“每次循环的委托调度开销”之间取得平衡。
    /// 【是否跨进程】否。分区只是进程内的数据切分策略，处理仍在本进程线程池线程上进行。
    /// 【典型适用场景】
    /// 1) Partitioner.Create(fromInclusive, toExclusive[, chunkSize])：把整数区间切成
    ///    Tuple&lt;int,int&gt;（long 重载为 Tuple&lt;long,long&gt;）范围块，配合 Parallel.ForEach 做并行 for，
    ///    循环体极轻量时显著减少委托调用次数；
    /// 2) Partitioner.Create(IList&lt;T&gt;)：对数组/列表做静态分区（支持负载均衡的重载），
    ///    每个工作线程拿走一大块，适合元素处理时间不均的负载均衡场景；
    /// 3) OrderablePartitioner&lt;T&gt;：ForEach 重载可拿到元素在数据源中的 0 基索引（第三个 long 参数），
    ///    用于结果回填到原位置、按序输出等；
    /// 4) EnumerablePartitionerOptions.NoBuffering：去掉缓冲，每个元素被取出后尽快交给一个线程执行，
    ///    粒度最细、最公平、取消最敏感，但委托开销最大；None（默认）会成块领取以提高吞吐。
    /// 【使用步骤】
    /// 1) 评估单次迭代开销：开销极小（如纯整数加法）时用范围分区并加大 chunkSize，减少分区/委托开销；
    /// 2) 调用 Partitioner.Create 得到分区器，传给 Parallel.ForEach；
    /// 3) 范围分区的循环体对 Tuple&lt;int,int&gt;（或 long 版本的 Tuple&lt;long,long&gt;）块用 for 从 Item1 累加到 Item2；
    /// 4) 需要原索引时用带 (元素, ParallelLoopState, long index) 的 ForEach 重载；
    /// 5) 取消敏感/要求公平逐元素调度时使用 EnumerablePartitionerOptions.NoBuffering。
    /// 【注意事项与坑】
    /// - 粒度太细（逐元素、元素处理又极快）时，线程池调度与 ForEach 委托开销可能超过计算本身，
    ///   此时分区（加大块）反而更快；粒度太粗则各线程负载不均、尾部拖尾；
    /// - chunkSize 必须 ≥ 1，fromInclusive 不能大于 toExclusive，否则抛 ArgumentOutOfRangeException；
    /// - 范围分区元素类型：int 边界重载是 Tuple&lt;int,int&gt;，long 边界重载是 Tuple&lt;long,long&gt;，不要混用；
    /// - NoBuffering 取消响应最快，但并行度再高也要逐元素支付委托开销，不要用于超轻量循环；
    /// - 分区只负责“切活”，循环体之间仍需保证相互独立，共享累加请用 Interlocked 或本地聚合。
    /// 【版本可用性】.NET Framework 4.0 起（4.5 起补充 EnumerablePartitionerOptions 与 IList 重载）；
    /// .NET Core/.NET 5+ 均可用，net48 全量可用。
    /// </summary>
    /// <example>
    /// 把 0..99999 按每块 1000 做范围分区，Parallel.ForEach 逐块内部 for 累加：
    /// <code>
    /// OrderablePartitioner&lt;Tuple&lt;int, int&gt;&gt; partitioner = Partitioner.Create(0, 100000, 1000);
    /// long total = 0;
    /// Parallel.ForEach(partitioner, delegate(Tuple&lt;int, int&gt; range, ParallelLoopState state)
    /// {
    ///     long subtotal = 0;                       // 块内串行累加，无锁
    ///     for (int i = range.Item1; i &lt; range.Item2; i++)
    ///     {
    ///         subtotal += i;
    ///     }
    ///     Interlocked.Add(ref total, subtotal);    // 各块结束时一次性合并
    /// });
    /// </code>
    /// </example>
    public static class HPartitionerHelp
    {
        /// <summary>
        /// 创建整数范围分区器（不指定块大小）：系统自行决定分块策略并在多线程间动态负载均衡。
        /// 每个分区元素是一个 [Item1, Item2) 的 int 范围元组。
        /// </summary>
        /// <param name="fromInclusive">起始索引（含）。</param>
        /// <param name="toExclusive">结束索引（不含），必须不小于 <paramref name="fromInclusive"/>。</param>
        /// <returns>可排序的整数范围分区器。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="toExclusive"/> 小于 <paramref name="fromInclusive"/>。
        /// </exception>
        public static OrderablePartitioner<Tuple<int, int>> CreateRange(int fromInclusive, int toExclusive)
        {
            if (fromInclusive > toExclusive)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("结束边界不能小于起始边界"));
            }
            // 不指定 chunkSize：分区大小与负载均衡由系统决定
            return Partitioner.Create(fromInclusive, toExclusive);
        }

        /// <summary>
        /// 创建固定块大小的整数范围分区器：每个分区至多处理 chunkSize 个连续整数。
        /// 循环体极轻量时，增大块可显著减少 ForEach 委托与线程池调度开销。
        /// </summary>
        /// <param name="fromInclusive">起始索引（含）。</param>
        /// <param name="toExclusive">结束索引（不含），必须不小于 <paramref name="fromInclusive"/>。</param>
        /// <param name="chunkSize">每块元素个数，必须 ≥ 1。</param>
        /// <returns>固定块大小的可排序整数范围分区器。</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="toExclusive"/> 小于 <paramref name="fromInclusive"/>，或
        /// <paramref name="chunkSize"/> 小于 1。
        /// </exception>
        public static OrderablePartitioner<Tuple<int, int>> CreateRange(int fromInclusive, int toExclusive, int chunkSize)
        {
            if (fromInclusive > toExclusive)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("结束边界不能小于起始边界"));
            }
            if (chunkSize < 1)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("分块大小不能小于 1"));
            }
            // 固定块：每个 Tuple&lt;int,int&gt; 分区长度不超过 chunkSize
            return Partitioner.Create(fromInclusive, toExclusive, chunkSize);
        }

        /// <summary>
        /// 示例1（真实计算）：范围分区并行求和 0+1+...+99999。
        /// 使用 Partitioner.Create(0, 100000, 1000) 切成每块 1000，Parallel.ForEach 逐块用 for 累加，
        /// 块内无锁、块间用 Interlocked 合并，避免逐元素委托开销。总和用 long 防止 int 溢出。
        /// </summary>
        /// <returns>0..99999 之和，固定为 4999950000。</returns>
        public static long SumWithRangePartitioner()
        {
            // 每块 1000 个连续整数，共约 100 块，粒度适中
            OrderablePartitioner<Tuple<int, int>> partitioner = Partitioner.Create(0, 100000, 1000);
            long total = 0;
            Parallel.ForEach(partitioner, delegate (Tuple<int, int> range, ParallelLoopState loopState)
            {
                long subtotal = 0;                                  // 块内串行累加，无锁
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    subtotal += i;
                }
                Interlocked.Add(ref total, subtotal);               // 整块只做一次原子合并
            });
            return total;                                           // 4999950000
        }

        /// <summary>
        /// 示例2（真实计算）：对 List&lt;int&gt; 用支持负载均衡的分区器并行求和 1+2+...+1000。
        /// Partitioner.Create(list, EnumerablePartitionerOptions.None) 让各工作线程动态领取元素块，
        /// 处理速度不均时不会出现“快线程干完干等、慢线程拖尾”的情况。
        /// </summary>
        /// <returns>1..1000 之和，固定为 500500。</returns>
        public static long SumListWithBalancing()
        {
            List<int> list = new List<int>(1000);
            for (int i = 1; i <= 1000; i++)
            {
                list.Add(i);
            }
            // None=默认成块领取；IList 重载支持按块动态分配，天然负载均衡
            OrderablePartitioner<int> partitioner = Partitioner.Create(list, EnumerablePartitionerOptions.None);
            long total = 0;
            Parallel.ForEach(partitioner, delegate (int item)
            {
                Interlocked.Add(ref total, item);                   // 共享累加用原子操作
            });
            return total;                                           // 500500
        }

        /// <summary>
        /// 示例3：OrderablePartitioner 可追踪索引演示。ForEach 的第三个参数 long index 即元素在
        /// 数据源中的 0 基位置；本方法把每个索引标记到对应槽位，并核对“元素值==索引”，
        /// 验证可排序分区器给出的索引与数据一一对应。
        /// </summary>
        /// <returns>0..29 每个索引都被处理且索引与元素值全部相等时返回 true。</returns>
        public static bool OrderableIndexDemo()
        {
            const int count = 30;
            List<int> list = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }
            OrderablePartitioner<int> partitioner = Partitioner.Create(list, EnumerablePartitionerOptions.None);
            bool[] seenIndex = new bool[count];
            bool mismatch = false;
            Parallel.ForEach(partitioner, delegate (int item, ParallelLoopState state, long index)
            {
                // 第三个参数：元素在原列表中的可追踪索引
                if (index < 0L || index >= (long)count || item != (int)index)
                {
                    mismatch = true;                                // 索引与元素对不上
                    state.Stop();
                    return;
                }
                lock (seenIndex)                                   // 标记该槽位已处理
                {
                    seenIndex[index] = true;
                }
            });
            if (mismatch)
            {
                return false;
            }
            for (int i = 0; i < count; i++)
            {
                if (!seenIndex[i])
                {
                    return false;                                   // 有索引漏处理
                }
            }
            return true;
        }

        /// <summary>
        /// 示例4：EnumerablePartitionerOptions.NoBuffering 演示。无缓冲模式下每个元素一旦可用就尽快
        /// 交给线程执行（不成块囤积），公平性与取消响应最好；这里核对全部元素被处理且求和正确。
        /// </summary>
        /// <returns>0..49 全部处理且总和为 1225 时返回 true。</returns>
        public static bool NoBufferingDemo()
        {
            const int count = 50;
            List<int> list = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(i);
            }
            // NoBuffering：逐元素尽快派发，适合需要公平、取消敏感的场景（委托开销也最大）
            OrderablePartitioner<int> partitioner = Partitioner.Create(list, EnumerablePartitionerOptions.NoBuffering);
            long total = 0;
            int processed = 0;
            Parallel.ForEach(partitioner, delegate (int item, ParallelLoopState state, long index)
            {
                Interlocked.Add(ref total, item);
                Interlocked.Increment(ref processed);
            });
            // 0..49 之和 = 49*50/2 = 1225
            return processed == count && total == 1225L;
        }
    }
}
