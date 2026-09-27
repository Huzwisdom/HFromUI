using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// Parallel 帮助类：高层数据/任务并行。
    /// 【是什么】System.Threading.Tasks.Parallel 提供静态的 For / ForEach / Invoke：内部基于 Task 与
    /// 线程池把循环迭代或多个动作自动分区并行执行，调用线程本身也参与计算，并阻塞到全部完成。
    /// 【是否跨进程】否。所有迭代仍在本进程的线程池线程上执行。
    /// 【典型适用场景】
    /// 1) CPU 密集型、大批量数据的统一变换/计算（图片像素、数值数组、日志解析）；
    /// 2) 一批互相独立的动作同时执行（Parallel.Invoke）；
    /// 3) 需要限制最大并行度、可取消、可提前 Break/Stop 的批处理。
    /// 【使用步骤】
    /// 1) 确认迭代之间相互独立（无共享可变状态，或自行加锁/用本地聚合）；
    /// 2) 构造 ParallelOptions：MaxDegreeOfParallelism、CancellationToken、TaskScheduler；
    /// 3) 调 Parallel.For / ForEach / Invoke，循环体签名可带 ParallelLoopState；
    /// 4) 用 state.Stop()（尽快停）或 state.Break()（不再启动新迭代）提前结束；
    /// 5) 用返回的 ParallelLoopResult 检查 IsCompleted / LowestBreakIteration。
    /// 【注意事项与坑】
    /// - 调用线程阻塞到全部完成，不要在 UI 线程直接调用（async 化用 Task.Run 包一层）；
    /// - Stop 与 Break 区别：Stop 立即不再启动任何迭代（已启动的尽力跑完）；
    ///   Break 表示"不再执行序号大于当前的迭代"，LowestBreakIteration 记录最小 Break 序号；
    /// - ShouldExitCurrentIteration 在 Stop/Break/异常/取消后变 true，长迭代体应主动检查；
    /// - 任何迭代抛异常都会在调用点重新抛 AggregateException（InnerExceptions 含全部异常）；
    /// - 迭代极轻量时并行分区/合并开销可能比串行还慢；
    /// - 需要线程级累加时用 localInit/localFinally 重载，不要每个元素都锁。
    /// 【版本可用性】net48 全量可用：Parallel.For/ForEach/Invoke 全部重载、ParallelOptions、
    /// ParallelLoopState、ParallelLoopResult、Partitioner 静态分区（System.Collections.Concurrent）。
    /// </summary>
    public static class HParallelHelp
    {
        /// <summary>示例1：并行 for 循环（等价 for 但分区并行）</summary>
        /// <param name="fromInclusive">起始索引（含）</param>
        /// <param name="toExclusive">结束索引（不含）</param>
        /// <param name="body">每次迭代执行的动作</param>
        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            Parallel.For(fromInclusive, toExclusive, body);
        }

        /// <summary>示例2：并行遍历集合</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">要遍历的集合</param>
        /// <param name="body">每个元素执行的动作</param>
        public static void ForEach<T>(IEnumerable<T> source, Action<T> body)
        {
            Parallel.ForEach(source, body);
        }

        /// <summary>示例3：并行执行多个动作（全部完成才返回）</summary>
        /// <param name="actions">要并行执行的动作</param>
        public static void Invoke(params Action[] actions)
        {
            Parallel.Invoke(actions);
        }

        /// <summary>示例4：限制最大并行度 + 可取消（如最多 4 路并发）</summary>
        /// <param name="fromInclusive">起始索引（含）</param>
        /// <param name="toExclusive">结束索引（不含）</param>
        /// <param name="maxDegreeOfParallelism">最大并发任务数，-1 表示不限</param>
        /// <param name="token">取消令牌</param>
        /// <param name="body">每次迭代执行的动作</param>
        public static void ForWithOptions(int fromInclusive, int toExclusive, int maxDegreeOfParallelism,
                                          CancellationToken token, Action<int> body)
        {
            ParallelOptions options = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = token
            };
            Parallel.For(fromInclusive, toExclusive, options, body);
        }

        /// <summary>示例5：带 ParallelLoopState 的并行 for：Break/Stop 由传入的循环体决定</summary>
        /// <param name="count">迭代总数（0..count-1）</param>
        /// <param name="body">循环体，可通过 state 提前结束</param>
        public static void ForBreakable(int count, Action<int, ParallelLoopState> body)
        {
            Parallel.For(0, count, delegate (int i, ParallelLoopState state) { body(i, state); });
        }

        /// <summary>示例6：带状态的 For 并返回 ParallelLoopResult（IsCompleted/LowestBreakIteration）</summary>
        /// <param name="fromInclusive">起始索引（含）</param>
        /// <param name="toExclusive">结束索引（不含）</param>
        /// <param name="body">带循环状态的循环体</param>
        /// <returns>并行循环结果；IsCompleted=false 时查看 LowestBreakIteration</returns>
        public static ParallelLoopResult ForResult(int fromInclusive, int toExclusive,
                                                   Action<int, ParallelLoopState> body)
        {
            return Parallel.For(fromInclusive, toExclusive,
                delegate (int i, ParallelLoopState state) { body(i, state); });
        }

        /// <summary>示例7：Stop 用法：找到第一个匹配项后尽快停止全部迭代</summary>
        /// <param name="count">迭代总数</param>
        /// <param name="match">判定谓词</param>
        /// <returns>任一匹配迭代写入的索引；无匹配返回 -1（多迭代并发时返回其中一个匹配索引）</returns>
        public static int StopAtFirst(int count, Func<int, bool> match)
        {
            int found = -1;
            Parallel.For(0, count, delegate (int i, ParallelLoopState state)
            {
                if (state.ShouldExitCurrentIteration)
                {
                    return;                        // 已有迭代 Stop，长任务体应尽快自行退出
                }
                if (match(i))
                {
                    found = i;
                    state.Stop();                 // 尽快停止：不再启动任何新迭代
                }
            });
            return found;
        }

        /// <summary>示例8：Break 用法：到达指定序号后不再执行更大序号的迭代，并返回结果对象</summary>
        /// <param name="count">迭代总数</param>
        /// <param name="breakAt">在该序号处调用 Break</param>
        /// <returns>ParallelLoopResult，LowestBreakIteration 记录最小的 Break 序号</returns>
        public static ParallelLoopResult BreakExample(int count, int breakAt)
        {
            return Parallel.For(0, count, delegate (int i, ParallelLoopState state)
            {
                Thread.SpinWait(10);              // 模拟极短工作
                if (i == breakAt)
                {
                    state.Break();                // 不阻止更小序号补跑，只截断更大序号
                }
            });
        }

        /// <summary>示例9：ForEach 带 ParallelLoopState：遍历中可 Stop/Break</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">要遍历的集合</param>
        /// <param name="body">带循环状态的元素处理动作</param>
        public static void ForEachWithState<T>(IEnumerable<T> source, Action<T, ParallelLoopState> body)
        {
            Parallel.ForEach(source, delegate (T item, ParallelLoopState state) { body(item, state); });
        }

        /// <summary>示例10：ForEach 带长索引（第三个参数即元素在数据源中的 0 基序号）</summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="source">要遍历的集合</param>
        /// <param name="body">(元素, 循环状态, 序号) 处理动作</param>
        public static void ForEachIndexed<T>(IEnumerable<T> source, Action<T, ParallelLoopState, long> body)
        {
            Parallel.ForEach(source, delegate (T item, ParallelLoopState state, long index)
            {
                body(item, state, index);
            });
        }

        /// <summary>示例11：本地聚合重载（localInit/body/localFinally）：每分区各自累加，最后合并，避免每元素加锁</summary>
        /// <param name="fromInclusive">起始索引（含）</param>
        /// <param name="toExclusive">结束索引（不含）</param>
        /// <param name="map">把索引映射为 long 值的函数</param>
        /// <returns>所有迭代映射值之和</returns>
        public static long ForWithLocalSum(int fromInclusive, int toExclusive, Func<int, long> map)
        {
            long total = 0;
            Parallel.For(fromInclusive, toExclusive,
                delegate () { return 0L; },                       // 每个分区线程的本地初值
                delegate (int i, ParallelLoopState state, long localSum)
                {
                    return localSum + map(i);                     // 分区内无锁累加
                },
                delegate (long localSum)
                {
                    Interlocked.Add(ref total, localSum);         // 各分区结束时一次性汇总
                });
            return total;
        }

        /// <summary>示例12：Parallel.Invoke 带 ParallelOptions（限并发/取消）</summary>
        /// <param name="options">并行选项</param>
        /// <param name="actions">要并行执行的动作</param>
        public static void InvokeWithOptions(ParallelOptions options, params Action[] actions)
        {
            Parallel.Invoke(options, actions);
        }

        /// <summary>示例13：取消示例：另一线程在指定毫秒后取消，Parallel.For 抛 OperationCanceledException</summary>
        /// <param name="cancelAfterMs">多少毫秒后取消</param>
        /// <returns>true=如期捕获到取消异常；false=循环在取消前就跑完</returns>
        public static bool CancelExample(int cancelAfterMs)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                Thread.Sleep(cancelAfterMs);    // 定时器到点取消
                cts.Cancel();
            });
            ParallelOptions options = new ParallelOptions { CancellationToken = cts.Token };
            try
            {
                Parallel.For(0, 100000, options, delegate (int i)
                {
                    options.CancellationToken.ThrowIfCancellationRequested();  // 检查点
                    Thread.Sleep(5);                 // 模拟工作，保证循环能跑到取消
                });
                return false;
            }
            catch (OperationCanceledException)
            {
                return true;                        // 取消会以该异常在调用点重现
            }
        }

        /// <summary>示例14：异常收集：迭代抛异常时 Parallel 在调用点抛 AggregateException，这里拍平为内部异常列表</summary>
        /// <param name="actions">可能各自抛异常的动作</param>
        /// <returns>所有迭代异常的列表；全部成功时为空表</returns>
        public static List<Exception> CollectExceptions(params Action[] actions)
        {
            try
            {
                Parallel.Invoke(actions);
            }
            catch (AggregateException aggregate)
            {
                return new List<Exception>(aggregate.InnerExceptions);
            }
            return new List<Exception>();
        }
    }
}
