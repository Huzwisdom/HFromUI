using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】Semaphore（信号量）帮助类：基于操作系统内核对象的计数等待句柄，内部维护一个计数，
    /// WaitOne 成功则计数 -1，计数为 0 时阻塞；<see cref="Semaphore.Release()"/> 使计数 +1 并唤醒等待者。
    /// 初始计数=初始通行证数，最大计数=上限。计数为 1 时退化为跨进程互斥（但与 Mutex 不同，
    /// Semaphore 不记录线程身份，一个线程可以 WaitOne、另一个线程 Release）。
    /// 【是否跨进程】是。命名信号量（构造时传 name）可被同机多个进程共享，合计限流；
    /// <c>Global\</c>/<c>Local\</c> 前缀规则与 Mutex 相同。未命名信号量仅进程内可见。
    /// 【典型适用场景】跨进程并发数限制（共享资源池、许可数量）；进程内异步限流请优先用
    /// <see cref="SemaphoreSlim"/>（纯托管、支持 WaitAsync、开销更小）。
    /// 【使用步骤】
    /// <list type="number">
    /// <item>构造 <c>new Semaphore(initialCount, maximumCount[, name, out createdNew])</c>。</item>
    /// <item>进入前 <c>WaitOne()</c>/<c>WaitOne(int)</c>/<c>WaitOne(TimeSpan)</c>。</item>
    /// <item>finally 中 <c>Release()</c> 或 <c>Release(n)</c> 归还名额。</item>
    /// <item>跨进程打开已有命名信号量：OpenExisting / TryOpenExisting（4.5+）。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>Release 后计数不得超过 maximumCount，否则抛 <see cref="SemaphoreFullException"/>；
    /// 名额"借还"必须配平，异常路径也要在 finally 归还。</item>
    /// <item>不绑定线程身份：可以 A 线程 WaitOne、B 线程 Release（与 Mutex 的关键区别）。</item>
    /// <item>WaitOne 超时返回 false 表示没拿到名额，绝不能 Release。</item>
    /// <item>构造时 initialCount 必须在 0..maximumCount 之间，maximumCount 必须为正，否则 ArgumentOutOfRangeException。</item>
    /// <item>信号量在等待队列满等场景是 FIFO 公平释放（Windows 内核），但不应在业务层依赖该顺序。</item>
    /// </list>
    /// 【版本可用性】Semaphore 自 .NET Framework 2.0；OpenExisting 为 2.0 起；
    /// TryOpenExisting 为 4.5 新增；GetAccessControl/SetAccessControl 与 SemaphoreRights 重载在
    /// 2.0+ 可用；带 NamedWaitHandleOptions 的重载属于更高版本 .NET，4.8 不可用。
    /// </summary>
    /// <example>
    /// <code>
    /// using (Semaphore pool = new Semaphore(3, 3))   // 同时只放 3 个线程
    /// {
    ///     pool.WaitOne();
    ///     try { /* 访问最多 3 路并发的资源 */ }
    ///     finally { pool.Release(); }
    /// }
    /// </code>
    /// </example>
    public static class HSemaphoreHelp
    {
        /// <summary>示例1：进程内限流，最多 maxCount 个线程同时执行 work（Task.WaitAll 保证结束后再释放信号量）。</summary>
        /// <param name="maxCount">最大并发数（初始名额=最大名额），必须大于 0。</param>
        /// <param name="work">受限流保护的工作。</param>
        /// <param name="runTimes">总任务数，必须非负。</param>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCount"/> 或 <paramref name="runTimes"/> 非法。</exception>
        public static void LimitConcurrency(int maxCount, Action work, int runTimes)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }
            if (maxCount <= 0 || runTimes < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            using (Semaphore semaphore = new Semaphore(maxCount, maxCount))
            {
                Task[] tasks = new Task[runTimes];
                for (int i = 0; i < runTimes; i++)
                {
                    tasks[i] = Task.Factory.StartNew(delegate
                    {
                        semaphore.WaitOne();          // 计数 -1，为 0 时阻塞等待
                        try
                        {
                            work();
                        }
                        finally
                        {
                            semaphore.Release();      // 计数 +1，唤醒一个等待者
                        }
                    });
                }
                Task.WaitAll(tasks);                  // 等所有任务结束，再离开 using 释放句柄
            }
        }

        /// <summary>示例2：跨进程限流（命名信号量，多个进程合计不超过 maxCount 并发）。</summary>
        /// <param name="semaphoreName">命名信号量名称（不存在则按 maxCount 初始名额创建）。</param>
        /// <param name="maxCount">最大并发数，必须大于 0。</param>
        /// <param name="timeoutMs">等待毫秒数。</param>
        /// <param name="work">受限流保护的工作。</param>
        /// <returns>true 表示拿到名额并执行完毕；false 表示超时未拿到。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException">数值参数非法。</exception>
        public static bool CrossProcessLimit(string semaphoreName, int maxCount, int timeoutMs, Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }
            if (maxCount <= 0 || timeoutMs < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            using (Semaphore semaphore = new Semaphore(maxCount, maxCount, semaphoreName))
            {
                if (!semaphore.WaitOne(timeoutMs))
                {
                    return false;
                }
                try
                {
                    work();
                }
                finally
                {
                    semaphore.Release();
                }
            }
            return true;
        }

        /// <summary>示例3：一次归还多个名额 <see cref="Semaphore.Release(int)"/>（返回释放前的计数）。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="count">归还名额数，必须为正数。</param>
        /// <returns>释放前的剩余计数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于 1。</exception>
        /// <exception cref="SemaphoreFullException">归还后计数超过最大计数（借还不配平）。</exception>
        public static int ReleaseMany(Semaphore semaphore, int count)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            return semaphore.Release(count);
        }

        /// <summary>示例4：显式创建命名信号量并通过 createdNew 区分"新建"与"打开已有"。</summary>
        /// <param name="name">信号量名称。</param>
        /// <param name="initialCount">初始名额（0..maxCount）。</param>
        /// <param name="maxCount">最大名额，必须大于 0。</param>
        /// <param name="createdNew">输出：true 表示内核对象由本次调用新建；false 表示同名对象已存在（沿用其当前计数）。</param>
        /// <returns>命名信号量实例（调用方负责 Dispose）。</returns>
        /// <exception cref="ArgumentOutOfRangeException">计数参数非法。</exception>
        public static Semaphore CreateNamed(string name, int initialCount, int maxCount, out bool createdNew)
        {
            return new Semaphore(initialCount, maxCount, name, out createdNew);
        }

        /// <summary>示例5：<see cref="Semaphore.TryOpenExisting"/>（4.5+）以 Try 模式打开已存在的命名信号量。</summary>
        /// <param name="name">信号量名称。</param>
        /// <param name="semaphore">输出：打开成功的实例（调用方负责 Dispose），失败为 null。</param>
        /// <returns>true 打开成功；false 同名对象不存在（无权限仍抛 <see cref="UnauthorizedAccessException"/>）。</returns>
        public static bool TryOpenExistingExample(string name, out Semaphore semaphore)
        {
            return Semaphore.TryOpenExisting(name, out semaphore);
        }

        /// <summary>示例6：<see cref="WaitHandle.WaitOne(TimeSpan)"/> 超时重载的用法。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="timeout">超时时长。</param>
        /// <param name="work">拿到名额后执行的工作。</param>
        /// <returns>true 执行成功；false 超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 或 <paramref name="work"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 非法。</exception>
        public static bool WaitOneTimeSpanExample(Semaphore semaphore, TimeSpan timeout, Action work)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            if (!semaphore.WaitOne(timeout))
            {
                return false;
            }
            try
            {
                work();
            }
            finally
            {
                semaphore.Release();
            }
            return true;
        }
    }
}
