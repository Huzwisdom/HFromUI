using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// PLINQ 帮助类：Parallel LINQ，声明式数据并行。
    /// 【是什么】对 IEnumerable 调 AsParallel() 得到 ParallelQuery（ParallelEnumerable），
    /// 之后的 Select/Where/Aggregate 等 LINQ 算子由线程池分区并行执行；查询默认延迟执行，
    /// 在 foreach/ForAll/聚合（ToList/Count/Aggregate 等）时才真正跑起来。
    /// 【是否跨进程】否。PLINQ 在本进程线程池线程上分区计算。
    /// 【典型适用场景】
    /// 1) 对内存中的大数据集做 CPU 密集、单条成本较高的统一变换/过滤；
    /// 2) 需要保序的并行查询（AsOrdered）；
    /// 3) 不要最终合并步骤的直接消费（ForAll）；
    /// 4) map/reduce 式分区聚合（Aggregate 四参数重载）。
    /// 【使用步骤】
    /// 1) source.AsParallel() 进入并行；
    /// 2) 需要保持输入顺序链式 AsOrdered()，不需要时用 AsUnordered() 提速；
    /// 3) 可选 WithDegreeOfParallelism（并行度）、WithCancellation（取消）、
    ///    WithMergeOptions（结果合并方式）、WithExecutionMode（强制并行）；
    /// 4) 写 Select/Where/Aggregate 等算子；
    /// 5) 用 ForAll 直接消费（不合并），或回到 IEnumerable（AsSequential/foreach）拿保序结果。
    /// 【注意事项与坑】
    /// - PLINQ 有分区与合并开销，数据量小或单条很快时反而比普通 LINQ 慢；运行时也可能自己决定回退串行
    ///   （除非 WithExecutionMode(ForceParallelism)）；
    /// - 默认不保序；AsOrdered 保序但有额外开销，后续可用 AsUnordered 解除；
    /// - WithCancellation 取消会抛 OperationCanceledException；
    /// - 合并选项 FullyBuffered 延迟大但吞吐高；NotBuffered 最快见到结果（适合流式）；
    /// - 不要在并行 lambda 里写共享可变状态，需要累加用 Aggregate 的分区合并重载；
    /// - AsSequential() 之后的算子恢复串行执行。
    /// 【版本可用性】net48 全量可用：AsParallel、AsOrdered、AsUnordered、AsSequential、
    /// WithDegreeOfParallelism、WithMergeOptions(ParallelMergeOptions)、WithExecutionMode(ParallelExecutionMode)、
    /// WithCancellation、ForAll、Aggregate、ParallelEnumerable.Range/Repeat、WithPartitionOptions。
    /// </summary>
    public static class HPlinqHelp
    {
        /// <summary>示例1：并行 Select（不保序，性能最好）</summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">结果元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="selector">每条数据的变换函数</param>
        /// <returns>并行查询结果（枚举时才执行）</returns>
        public static IEnumerable<TResult> ParallelSelect<TSource, TResult>(IEnumerable<TSource> source,
                                                                             Func<TSource, TResult> selector)
        {
            return source.AsParallel().Select(selector);
        }

        /// <summary>示例2：并行 Where + AsOrdered 保序（结果顺序与输入一致）</summary>
        /// <typeparam name="TSource">元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="predicate">过滤谓词</param>
        /// <returns>保持输入顺序的并行过滤结果</returns>
        public static IEnumerable<TSource> ParallelWhereOrdered<TSource>(IEnumerable<TSource> source,
                                                                          Func<TSource, bool> predicate)
        {
            return source.AsParallel().AsOrdered().Where(predicate);
        }

        /// <summary>示例3：指定并行度（同时参与计算的最大任务数）</summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">结果元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="degreeOfParallelism">并行分区数</param>
        /// <param name="selector">变换函数</param>
        /// <returns>并行查询结果</returns>
        public static IEnumerable<TResult> WithDegree<TSource, TResult>(IEnumerable<TSource> source,
                                                                         int degreeOfParallelism,
                                                                         Func<TSource, TResult> selector)
        {
            return source.AsParallel()
                         .WithDegreeOfParallelism(degreeOfParallelism)
                         .Select(selector);
        }

        /// <summary>示例4：ForAll 直接在并行分区线程上消费结果（无最后合并步骤，比 foreach 快）</summary>
        /// <typeparam name="TSource">元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="action">对每条结果的消费动作（注意线程安全）</param>
        public static void ForAll<TSource>(IEnumerable<TSource> source, Action<TSource> action)
        {
            source.AsParallel().ForAll(action);
        }

        /// <summary>示例5：可取消的并行查询（token 取消时枚举抛 OperationCanceledException）</summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">结果元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="token">取消令牌</param>
        /// <param name="selector">变换函数</param>
        /// <returns>可取消的并行查询结果</returns>
        public static IEnumerable<TResult> WithCancel<TSource, TResult>(IEnumerable<TSource> source,
                                                                         CancellationToken token,
                                                                         Func<TSource, TResult> selector)
        {
            return source.AsParallel().WithCancellation(token).Select(selector);
        }

        /// <summary>示例6：AsSequential：并行查询后切回串行，后续算子在当前线程逐个执行</summary>
        /// <typeparam name="TSource">元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="predicate">串行执行的过滤谓词</param>
        /// <returns>串行枚举的结果</returns>
        public static IEnumerable<TSource> AsSequential<TSource>(IEnumerable<TSource> source,
                                                                 Func<TSource, bool> predicate)
        {
            // AsParallel 之后又 AsSequential：只让前面的算子并行，Take/OrderBy 等复杂场景常用
            return source.AsParallel().AsSequential().Where(predicate);
        }

        /// <summary>示例7：AsOrdered 后 AsUnordered：先保序进入，某算子之后解除保序以恢复性能</summary>
        /// <typeparam name="TSource">元素类型</typeparam>
        /// <typeparam name="TResult">结果元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="selector">变换函数</param>
        /// <returns>不保证顺序的并行查询结果</returns>
        public static IEnumerable<TResult> UnorderedAfterOrdered<TSource, TResult>(IEnumerable<TSource> source,
                                                                                    Func<TSource, TResult> selector)
        {
            return source.AsParallel().AsOrdered().AsUnordered().Select(selector);
        }

        /// <summary>示例8：WithMergeOptions 控制结果合并缓冲策略（NotBuffered/AutoBuffered/FullyBuffered）</summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">结果元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="mergeOptions">合并选项，流式输出用 NotBuffered</param>
        /// <param name="selector">变换函数</param>
        /// <returns>按指定合并选项产出的查询结果</returns>
        public static IEnumerable<TResult> WithMerge<TSource, TResult>(IEnumerable<TSource> source,
                                                                        ParallelMergeOptions mergeOptions,
                                                                        Func<TSource, TResult> selector)
        {
            return source.AsParallel()
                         .WithMergeOptions(mergeOptions)
                         .Select(selector);
        }

        /// <summary>示例9：WithExecutionMode(ForceParallelism)：即使查询很小也强制并行（默认运行时可能选择串行）</summary>
        /// <typeparam name="TSource">源元素类型</typeparam>
        /// <typeparam name="TResult">结果元素类型</typeparam>
        /// <param name="source">数据源</param>
        /// <param name="selector">变换函数</param>
        /// <returns>强制并行执行的查询结果</returns>
        public static IEnumerable<TResult> ForceParallel<TSource, TResult>(IEnumerable<TSource> source,
                                                                           Func<TSource, TResult> selector)
        {
            return source.AsParallel()
                         .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                         .Select(selector);
        }

        /// <summary>示例10：Aggregate 四参数重载（seed/分区内累加/分区合并/最终投影）：并行求和的 reduce 写法</summary>
        /// <param name="numbers">数据源</param>
        /// <returns>所有元素之和</returns>
        public static long AggregateSum(IEnumerable<int> numbers)
        {
            return numbers.AsParallel().Aggregate(
                0L,
                delegate (long localSum, int item) { return localSum + item; },   // 每个分区内部累加
                delegate (long all, long localSum) { return all + localSum; },    // 合并各分区结果
                delegate (long total) { return total; });                         // 最终结果投影
        }

        /// <summary>示例11：ParallelEnumerable.Range：直接产生并行整数序列，省去对 List 再 AsParallel</summary>
        /// <param name="start">起始值（含）</param>
        /// <param name="count">序列长度</param>
        /// <param name="selector">对每个整数的变换函数</param>
        /// <returns>并行变换结果</returns>
        public static IEnumerable<long> RangeParallel(int start, int count, Func<int, long> selector)
        {
            return ParallelEnumerable.Range(start, count).Select(selector);
        }
    }
}
