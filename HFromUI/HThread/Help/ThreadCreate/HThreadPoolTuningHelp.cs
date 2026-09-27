using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// 线程池调优帮助类：读取/调整工作线程与完成端口线程数量，以及突发延迟与预热。
    ///
    /// 【是什么】<see cref="ThreadPool"/> 提供“五件套”查询/调整 API：
    /// GetMinThreads(out worker, out io)/SetMinThreads、GetMaxThreads(out worker, out io)/SetMaxThreads、
    /// GetAvailableThreads(out worker, out io)。最小线程数（MinThreads）是线程池愿意长期维持、
    /// 且新工作到来时【立即】创建到的线程数；最大线程数（MaxThreads）是极端积压下允许增长到的上限；
    /// 可用线程数（AvailableThreads）= 最大值 − 当前正在忙碌的数量。
    /// 两组数字分别针对“工作线程（workerThreads）”与“完成端口线程（completionPortThreads，处理异步 I/O
    /// 完成包）”，它们是两条相互独立的队列与配额，调优时必须成对给出。
    ///
    /// 【是否跨进程】否。线程池配置是进程（CLR 实例）级全局状态：一处修改影响全进程所有组件。
    ///
    /// 【典型适用场景】
    /// 1) 应用启动、可预见一波突发负载前，用 SetMinThreads 预热，避开线程爬坡延迟；
    /// 2) 压测/排障时用 ReadCounts 观察线程池规模与余量，判断瓶颈是否在线程池；
    /// 3) 寄宿环境（IIS/ASP.NET）下核对 MaxThreads 策略是否被宿主限制。
    ///
    /// 【使用步骤】
    /// 1) GetMinThreads 保存原始值；2) SetMinThreads 临时预热；3) 执行业务/压测；
    /// 4) finally 中无条件 SetMinThreads 还原原值（Set 返回 false 也要记录）；
    /// 5) 任何调整前先压测对比，调整后再次压测验证。
    ///
    /// 【注意事项与坑】
    /// 1) 突发延迟（爬坡）：线程池的线程增长走“爬山算法”，工作项积压时大约每 0.5~1 秒才新增一个线程；
    ///    突发几百个会阻塞的任务时，前 MinThreads 个立即有线程，之后每 0.5~1 秒才补一个，
    ///    延迟尖刺往往由此产生——SetMinThreads 预热的正确用途就是“已知突发、提前把线程数顶上去”；
    /// 2) 盲目调高最小线程的代价：每个线程默认预留约 1MB 栈空间（数百线程即数百 MB 虚拟内存），
    ///    线程过多增加上下文切换与调度开销，还会掩盖“线程池线程里做长阻塞/同步 I/O”等实现问题；
    /// 3) 完成端口线程与工作线程不同：完成端口线程只负责异步 I/O 完成回调，数量需求通常很小；
    ///    把大量计算/阻塞任务的瓶颈误判为 IOCP 线程不足而乱调 completionPortThreads 没有意义；
    /// 4) SetMaxThreads 不能小于 CPU 核数、不能小于 MinThreads，否则返回 false 且参数不生效；
    ///    在 IIS/ASP.NET 等宿主中，线程池设置可能被宿主（machine.config/aspnet.config、
    ///    autoConfig/minFreeThreads 等策略）限制或接管，调用可能返回 false 或随后被宿主改写——
    ///    【必须检查返回值】，不要假设设置一定生效；
    /// 5) 推荐流程是“先压测、用数据证明瓶颈在线程池爬坡，再小步调整并回归压测”，默认值对多数应用已足够；
    /// 6) 这些 API 是进程全局状态：库代码不应擅自修改，修改必须 try/finally 还原，否则污染同进程其他组件。
    ///
    /// 【版本可用性】net48 全量可用（mscorlib）。注意 net48 下 GetMinThreads/GetMaxThreads/
    /// GetAvailableThreads 返回 void（结果走 out 参数），SetMinThreads/SetMaxThreads 返回 bool；
    /// .NET Core 对应 Get 方法才返回 bool。
    /// </summary>
    /// <example>
    /// 压测前临时预热 4 个工作线程，结束后在 finally 中无条件还原：
    /// <code>
    /// int origWorker;
    /// int origIo;
    /// ThreadPool.GetMinThreads(out origWorker, out origIo);
    /// try
    /// {
    ///     if (!ThreadPool.SetMinThreads(origWorker + 4, origIo))
    ///     {
    ///         Console.WriteLine("预热失败：参数非法或被宿主限制，保持原值继续");
    ///     }
    ///     RunBurstWorkload();   // 已确认的突发工作负载
    /// }
    /// finally
    /// {
    ///     bool restored = ThreadPool.SetMinThreads(origWorker, origIo);  // 无条件还原并检查返回值
    ///     Console.WriteLine("已还原最小线程数：" + restored);
    /// }
    /// </code>
    /// </example>
    public static class HThreadPoolTuningHelp
    {
        /// <summary>
        /// 线程池计数快照：工作线程与完成端口线程的最小值、最大值、当前可用值，共 6 个数字。
        /// </summary>
        public sealed class ThreadPoolCounts
        {
            /// <summary>最小工作线程数（GetMinThreads 的 workerThreads）。</summary>
            public int WorkerMin { get; set; }

            /// <summary>最小完成端口（异步 I/O）线程数（GetMinThreads 的 completionPortThreads）。</summary>
            public int IoMin { get; set; }

            /// <summary>最大工作线程数（GetMaxThreads 的 workerThreads）。</summary>
            public int WorkerMax { get; set; }

            /// <summary>最大完成端口（异步 I/O）线程数（GetMaxThreads 的 completionPortThreads）。</summary>
            public int IoMax { get; set; }

            /// <summary>当前可用（空闲）工作线程数（GetAvailableThreads 的 workerThreads）。</summary>
            public int WorkerAvailable { get; set; }

            /// <summary>当前可用（空闲）完成端口线程数（GetAvailableThreads 的 completionPortThreads）。</summary>
            public int IoAvailable { get; set; }
        }

        /// <summary>
        /// 读取线程池“三件读取 API”给出的 6 个计数：最小值、最大值、可用值（工作线程/完成端口线程各一组），
        /// 填充到 <see cref="ThreadPoolCounts"/> 返回。纯内存查询，毫秒级、无副作用、不弹 UI、不依赖文件网络。
        /// </summary>
        /// <returns>当前线程池计数快照。</returns>
        public static ThreadPoolCounts ReadCounts()
        {
            ThreadPoolCounts counts = new ThreadPoolCounts();

            // 1) 最小值：工作线程 + 完成端口线程
            ThreadPool.GetMinThreads(out int minWorker, out int minIo);
            counts.WorkerMin = minWorker;
            counts.IoMin = minIo;

            // 2) 最大值：工作线程 + 完成端口线程
            ThreadPool.GetMaxThreads(out int maxWorker, out int maxIo);
            counts.WorkerMax = maxWorker;
            counts.IoMax = maxIo;

            // 3) 当前可用（= 最大值 − 正在忙碌的数量）
            ThreadPool.GetAvailableThreads(out int availableWorker, out int availableIo);
            counts.WorkerAvailable = availableWorker;
            counts.IoAvailable = availableIo;

            return counts;
        }

        /// <summary>
        /// 示例（自包含可执行）：临时把最小工作线程数提高 2 个并验证生效，然后在 finally 中
        /// 无条件把最小线程数还原为原始值。整个过程只做内存级查询/设置，正常毫秒级结束；
        /// 不修改最大线程数、不弹 UI、不依赖文件网络，适合反射自测反复调用。
        /// </summary>
        /// <returns>
        /// 临时设置生效（读回的最小工作线程数等于“原值+2”、最小完成端口线程数不变），
        /// 且 finally 中还原原始值的 SetMinThreads 返回 true，两项同时成立返回 true；否则 false。
        /// </returns>
        public static bool TemporarilyRaiseMinThreadsDemo()
        {
            // 1) 先保存原始值（net48 的 GetMinThreads 返回 void，结果走 out）
            ThreadPool.GetMinThreads(out int origWorker, out int origIo);

            bool temporaryApplied = false;
            bool restored = false;
            try
            {
                // 2) 仅提高工作线程最小值 2 个，完成端口线程保持不动
                if (ThreadPool.SetMinThreads(origWorker + 2, origIo))
                {
                    // 3) 读回验证是否真的生效（宿主限制/参数非法时可能不生效）
                    ThreadPool.GetMinThreads(out int nowWorker, out int nowIo);
                    temporaryApplied = (nowWorker == origWorker + 2) && (nowIo == origIo);
                }
            }
            finally
            {
                // 4) 无条件还原进程全局配置，并检查返回值，避免污染后续其他测试/组件
                restored = ThreadPool.SetMinThreads(origWorker, origIo);
            }

            return temporaryApplied && restored;
        }
    }
}
