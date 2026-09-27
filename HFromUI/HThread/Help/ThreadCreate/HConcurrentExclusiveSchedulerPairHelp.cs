using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// ConcurrentExclusiveSchedulerPair 帮助类：并发/独占两类任务调度对（类似读写锁语义）。
    /// 【是什么】System.Threading.Tasks.ConcurrentExclusiveSchedulerPair 提供一对配对的 TaskScheduler：
    /// ConcurrentScheduler（并发调度器）上的任务只要没有独占任务在跑，就可以彼此并发执行；
    /// ExclusiveScheduler（独占调度器）上的任务运行时，既不会有任何并发任务在跑，也不会有其他
    /// 独占任务在跑——语义等价于读写锁中的“写者”，并发任务等价于“读者”。任务用
    /// new TaskFactory(scheduler).StartNew(...) 投递，处理在底层（默认线程池）线程上进行。
    /// 【是否跨进程】否。只是本进程内 Task 的排队/互斥执行策略。
    /// 【典型适用场景】
    /// 1) 一批读操作可并发，偶尔出现的写/重置操作必须独占且必须等所有在跑的读操作收尾；
    /// 2) 需要“优雅排空”：投递独占任务即可保证它执行前同对调度器上没有其他任务；
    /// 3) 关闭阶段：Complete() 声明不再接收新任务，await/Wait Completion 任务即可等待全部排空。
    /// 【使用步骤】
    /// 1) new ConcurrentExclusiveSchedulerPair()，可指定底层 TaskScheduler 与最大并发级别；
    /// 2) 用 new TaskFactory(pair.ConcurrentScheduler) 投递可并发任务；
    /// 3) 用 new TaskFactory(pair.ExclusiveScheduler) 投递必须独占的任务；
    /// 4) 不再投递后调 pair.Complete()，等待 pair.Completion 任务确认全部排空。
    /// 【注意事项与坑】
    /// - 独占任务 FIFO 等待，且会长时间堵住后续所有任务，独占体务必短，避免饥饿；
    /// - Complete() 后再向工厂投递任务会抛 TaskSchedulerException（内部包 InvalidOperationException）；
    /// - Completion 在有任务故障时也会完成（异常转到该任务或 Completion 上观察），等待请带超时；
    /// - 独占保证只针对“同一对调度器”投递的任务，与外部线程池里的其他任务无关；
    /// - 底层默认仍是线程池，峰值并发受线程池线程供给影响，不要假设 N 个并发任务一定同时开跑。
    /// 【版本可用性】.NET Framework 4.5 起；.NET Core/.NET 5+ 均可用，net48 全量可用。
    /// </summary>
    /// <example>
    /// 两个并发任务先跑，独占任务一定等它们全部结束后才执行：
    /// <code>
    /// ConcurrentExclusiveSchedulerPair pair = new ConcurrentExclusiveSchedulerPair();
    /// TaskFactory concurrentFactory = new TaskFactory(pair.ConcurrentScheduler);
    /// TaskFactory exclusiveFactory = new TaskFactory(pair.ExclusiveScheduler);
    /// Task t1 = concurrentFactory.StartNew(delegate { /* 读操作，可与其他并发任务同时跑 */ });
    /// Task t2 = concurrentFactory.StartNew(delegate { /* 读操作 */ });
    /// Task x = exclusiveFactory.StartNew(delegate { /* 写操作：此刻 t1、t2 必然都已结束 */ });
    /// Task.WaitAll(new[] { t1, t2, x }, 2000);
    /// pair.Complete();
    /// pair.Completion.Wait(2000);
    /// </code>
    /// </example>
    public static class HConcurrentExclusiveSchedulerPairHelp
    {
        /// <summary>
        /// 示例1（真实行为验证）：独占任务必须等并发任务全部结束。
        /// 向并发调度器投递 2 个各阻塞 50ms 的任务，再向独占调度器投递 1 个短任务，
        /// 独占任务体内读取“当前并发执行数”，应为 0；同时用 Interlocked 记录并发峰值。
        /// 所有等待带 2000ms 超时，自测不会挂死。
        /// </summary>
        /// <returns>独占任务执行时观察到的并发计数恰好为 0，且调度对在 2000ms 内正常排空时返回 true。</returns>
        public static bool ExclusiveWaitsForConcurrentToFinish()
        {
            ConcurrentExclusiveSchedulerPair pair = new ConcurrentExclusiveSchedulerPair();
            TaskFactory concurrentFactory = new TaskFactory(pair.ConcurrentScheduler);
            TaskFactory exclusiveFactory = new TaskFactory(pair.ExclusiveScheduler);

            int currentConcurrent = 0;      // 此刻在跑的并发任务数
            int peakConcurrent = 0;         // 历史峰值
            int observedByExclusive = -1;   // 独占任务开始时读到的并发数

            // 先投递两个可并发执行的任务
            Task t1 = concurrentFactory.StartNew(delegate
            {
                EnterConcurrency(ref currentConcurrent, ref peakConcurrent);
                try
                {
                    Task.Delay(50).Wait();  // 模拟在途读操作
                }
                finally
                {
                    Interlocked.Decrement(ref currentConcurrent);
                }
            });
            Task t2 = concurrentFactory.StartNew(delegate
            {
                EnterConcurrency(ref currentConcurrent, ref peakConcurrent);
                try
                {
                    Task.Delay(50).Wait();
                }
                finally
                {
                    Interlocked.Decrement(ref currentConcurrent);
                }
            });

            // 稍等确保两个并发任务已经开跑，再投递独占任务（演示它等待在途任务收尾）
            Thread.Sleep(10);
            Task exclusive = exclusiveFactory.StartNew(delegate
            {
                // 独占语义保证：读到的并发计数必须为 0
                observedByExclusive = Volatile.Read(ref currentConcurrent);
                Thread.Sleep(2);
            });

            bool allDone = Task.WaitAll(new[] { t1, t2, exclusive }, 2000);

            pair.Complete();
            bool drained = pair.Completion.Wait(2000);

            // 独占任务确实跑过（标记被改写）、看到 0 个并发任务、全部按时排空
            return allDone && drained && observedByExclusive == 0;
        }

        /// <summary>
        /// 示例2（真实行为验证）：并发调度器上的任务确实彼此并发。
        /// 投递 8 个短任务，用 Interlocked 统计同时在跑的峰值；为避免线程池预热不足影响观察，
        /// 临时把线程池最小工作线程数抬到 8，结束后恢复原值。
        /// </summary>
        /// <returns>观测到的并发峰值，应大于 1（任务确实重叠执行）。</returns>
        public static int PeakConcurrencyDemo()
        {
            int minWorker;
            int minIo;
            ThreadPool.GetMinThreads(out minWorker, out minIo);
            bool bumped = false;
            if (minWorker < 8)
            {
                // 临时抬高下限，保证 8 个短任务有足够线程同时开跑
                bumped = ThreadPool.SetMinThreads(Math.Max(minWorker, 8), minIo);
            }
            try
            {
                ConcurrentExclusiveSchedulerPair pair = new ConcurrentExclusiveSchedulerPair();
                TaskFactory concurrentFactory = new TaskFactory(pair.ConcurrentScheduler);

                int currentConcurrent = 0;
                int peakConcurrent = 0;
                Task[] tasks = new Task[8];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = concurrentFactory.StartNew(delegate
                    {
                        EnterConcurrency(ref currentConcurrent, ref peakConcurrent);
                        try
                        {
                            Task.Delay(10).Wait();  // 重叠窗口，便于出现并发峰值
                        }
                        finally
                        {
                            Interlocked.Decrement(ref currentConcurrent);
                        }
                    });
                }
                Task.WaitAll(tasks, 2000);

                pair.Complete();
                pair.Completion.Wait(2000);
                return Volatile.Read(ref peakConcurrent);   // 期望 >1
            }
            finally
            {
                if (bumped)
                {
                    ThreadPool.SetMinThreads(minWorker, minIo);  // 恢复进程原设置
                }
            }
        }

        /// <summary>
        /// 读取默认配置下并发调度器声明的最大并发级别。默认构造未显式限制时，该值取自底层
        /// TaskScheduler（默认线程池调度器通常返回处理器数），仅表示调度器声称的上限参考。
        /// </summary>
        /// <returns>并发调度器的 MaximumConcurrencyLevel（默认构造下通常等于处理器数）。</returns>
        public static int GetConcurrentSchedulerMaxConcurrencyLevel()
        {
            ConcurrentExclusiveSchedulerPair pair = new ConcurrentExclusiveSchedulerPair();
            try
            {
                // 仅为读取属性，立即关闭并等待排空
                return pair.ConcurrentScheduler.MaximumConcurrencyLevel;
            }
            finally
            {
                pair.Complete();
                pair.Completion.Wait(1000);
            }
        }

        /// <summary>
        /// 进入临界区：当前并发数 +1，并用 CAS 循环更新历史峰值。
        /// </summary>
        /// <param name="current">当前并发计数（按引用原子更新）。</param>
        /// <param name="peak">历史峰值（按引用原子更新）。</param>
        /// <returns>进入后的当前并发数。</returns>
        private static int EnterConcurrency(ref int current, ref int peak)
        {
            int now = Interlocked.Increment(ref current);
            int oldPeak;
            do
            {
                oldPeak = peak;
                if (now <= oldPeak)
                {
                    break;                              // 已有更高峰值，无需更新
                }
            }
            while (Interlocked.CompareExchange(ref peak, now, oldPeak) != oldPeak);
            return now;
        }
    }
}
