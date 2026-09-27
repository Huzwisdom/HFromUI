using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// ParallelOptions / ParallelLoopState / ParallelLoopResult 帮助类：控制并行循环怎么跑、何时停。
    /// 【是什么】
    /// - ParallelOptions：Parallel.For/ForEach/Invoke 的可选项，三个属性：MaxDegreeOfParallelism
    ///   （最大并发任务数，-1 不限、1 等同串行）、CancellationToken（取消令牌）、TaskScheduler
    ///   （指定调度器，默认线程池调度器）；
    /// - ParallelLoopState：循环体第二个参数，用于协作式提前结束：Stop()（尽快停止全部迭代）、
    ///   Break()（不再执行序号比当前大的迭代，低序号迭代允许跑完）、ShouldExitCurrentIteration
    ///   （其他迭代已请求停止/取消/异常，长任务体应主动检查并退出）、IsStopped、IsExceptional、
    ///   LowestBreakIteration；
    /// - ParallelLoopResult：Parallel.For/ForEach 的返回值，IsCompleted 为 false 表示未跑完全部迭代，
    ///   LowestBreakIteration（long?）记录最小的 Break 序号（Stop 时为 null）。
    /// 【是否跨进程】否。只控制本进程线程池上的并行循环。
    /// 【典型适用场景】
    /// 1) 防止并行循环占满 CPU/磁盘：限制 MaxDegreeOfParallelism；
    /// 2) 用户取消或超时：外部 CancellationTokenSource.Cancel()，循环抛 OperationCanceledException；
    /// 3) 找到目标即停：Stop()；要求“前 N 项必须处理完、后面的不要了”：Break()；
    /// 4) 循环后用 ParallelLoopResult 判断是正常跑完还是被 Break/Stop/取消截断。
    /// 【使用步骤】
    /// 1) new ParallelOptions 并设置并发度/令牌/调度器；
    /// 2) 循环体使用带 ParallelLoopState 的重载，按需调 Stop/Break；
    /// 3) 长循环体在开头检查 state.ShouldExitCurrentIteration 或 token.ThrowIfCancellationRequested()；
    /// 4) 接收 ParallelLoopResult，检查 IsCompleted 与 LowestBreakIteration.HasValue/Value。
    /// 【注意事项与坑】
    /// - Break 与 Stop 区别：Break 是“有序截断”——让更低索引的迭代尽快补跑完成，只保证不再启动
    ///   更大索引的迭代；Stop 是“尽快全停”——不再启动任何新迭代，已启动的尽力跑完；
    /// - Break 后 LowestBreakIteration 取所有调用 Break 的迭代中最小的序号；Stop 不设置该值；
    /// - MaxDegreeOfParallelism 为 0 或小于 -1 会抛 ArgumentOutOfRangeException；
    /// - 取消后异常在调用点以 OperationCanceledException 重现（Parallel 包装的 AggregateException
    ///   会被特化为该取消异常）；
    /// - Parallel 循环会阻塞调用线程且调用线程本身也参与计算，不要在 UI 线程直接调用。
    /// 【版本可用性】.NET Framework 4.0 起，net48 全量可用。
    /// </summary>
    /// <example>
    /// 限制最多 2 路并发，并用 Interlocked 统计真实峰值：
    /// <code>
    /// int current = 0;
    /// int peak = 0;
    /// ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
    /// Parallel.For(0, 20, options, delegate(int i)
    /// {
    ///     int now = Interlocked.Increment(ref current);
    ///     Interlocked.Exchange(ref peak, Math.Max(peak, now));
    ///     Thread.Sleep(5);
    ///     Interlocked.Decrement(ref current);
    /// });
    /// </code>
    /// </example>
    public static class HParallelOptionsHelp
    {
        /// <summary>
        /// 示例1（真实行为验证）：MaxDegreeOfParallelism=2 时并行循环的真实并发峰值不超过 2。
        /// 20 个迭代各睡 5ms，用 Interlocked 统计同时在跑的峰值。
        /// </summary>
        /// <returns>并发峰值处于 [1,2] 区间时返回 true。</returns>
        public static bool LimitDegreeToTwo()
        {
            int current = 0;
            int peak = 0;
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 2;            // 最多 2 路并发（含调用线程）

            Parallel.For(0, 20, options, delegate (int i)
            {
                int now = Interlocked.Increment(ref current);
                UpdatePeak(ref peak, now);                 // CAS 更新峰值
                try
                {
                    Thread.Sleep(5);                       // 不能 await，用同步睡眠制造重叠窗口
                }
                finally
                {
                    Interlocked.Decrement(ref current);
                }
            });

            int observed = Volatile.Read(ref peak);
            return observed >= 1 && observed <= 2;
        }

        /// <summary>
        /// 示例2（真实行为验证）：在索引 5 处 Break。
        /// Break 表示不再启动索引大于 5 的迭代，因此循环未“完整”跑完：IsCompleted 为 false，
        /// LowestBreakIteration（long?）有值且等于 5。
        /// </summary>
        /// <returns>IsCompleted 为 false 且 LowestBreakIteration == 5 时返回 true。</returns>
        public static bool BreakFromIndexFive()
        {
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 4;

            ParallelLoopResult result = Parallel.For(0, 20, options,
                delegate (int i, ParallelLoopState state)
                {
                    if (i == 5)
                    {
                        state.Break();                     // 有序截断：低索引允许补跑，高索引不再启动
                    }
                    Thread.Sleep(1);
                });

            // Break 导致未完整完成；注意 LowestBreakIteration 是 long?，必须先判 HasValue
            return !result.IsCompleted
                && result.LowestBreakIteration.HasValue
                && result.LowestBreakIteration.Value == 5L;
        }

        /// <summary>
        /// 示例3（真实行为验证）：Stop 与 Break 的差异。在索引 10 处 Stop：IsCompleted 为 false，
        /// 且 Stop 不记录断点——LowestBreakIteration 为 null。
        /// </summary>
        /// <returns>IsCompleted 为 false 且 LowestBreakIteration 无值时返回 true。</returns>
        public static bool StopAllAsSoonAsPossible()
        {
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 4;

            ParallelLoopResult result = Parallel.For(0, 50, options,
                delegate (int i, ParallelLoopState state)
                {
                    if (i == 10)
                    {
                        state.Stop();                      // 尽快停全部：不再启动任何新迭代
                    }
                    Thread.Sleep(1);
                });

            // Stop 不留断点序号：LowestBreakIteration 保持 null
            return !result.IsCompleted && !result.LowestBreakIteration.HasValue;
        }

        /// <summary>
        /// 示例4（真实行为验证）：通过 CancellationToken 取消并行循环。
        /// 第一个迭代就 Cancel 并睡眠 10ms，循环体设置取消检查点，Parallel 在调用点抛
        /// OperationCanceledException。
        /// </summary>
        /// <returns>如期捕获取消异常返回 "Canceled"；极端情况下循环先跑完则返回 "Completed"。</returns>
        public static string CancelViaTokenDemo()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                ParallelOptions options = new ParallelOptions();
                options.CancellationToken = cts.Token;
                options.MaxDegreeOfParallelism = 4;
                try
                {
                    Parallel.For(0, 1000, options, delegate (int i)
                    {
                        if (i == 0)
                        {
                            cts.Cancel();                 // 第一个迭代即请求取消
                        }
                        Thread.Sleep(10);
                        // 显式检查点：取消后以 OperationCanceledException 退出
                        options.CancellationToken.ThrowIfCancellationRequested();
                    });
                    return "Completed";
                }
                catch (OperationCanceledException)
                {
                    return "Canceled";                    // 取消在调用点重现
                }
            }
        }

        /// <summary>
        /// 用 CAS 循环把峰值更新为 peak 与 now 中的较大者。
        /// </summary>
        /// <param name="peak">历史峰值（按引用原子更新）。</param>
        /// <param name="now">本次观测到的并发数。</param>
        private static void UpdatePeak(ref int peak, int now)
        {
            int oldPeak;
            do
            {
                oldPeak = peak;
                if (now <= oldPeak)
                {
                    return;                               // 无需更新
                }
            }
            while (Interlocked.CompareExchange(ref peak, now, oldPeak) != oldPeak);
        }
    }
}
