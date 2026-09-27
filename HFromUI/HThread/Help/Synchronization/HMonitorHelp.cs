using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】Monitor（监视器）帮助类：基于引用对象的进程内排他锁（互斥锁），也是 C#
    /// <c>lock</c> 关键字背后的实现。<c>lock (syncRoot) { ... }</c> 等价于
    /// <c>Monitor.Enter</c> + try/finally + <c>Monitor.Exit</c> 的可靠写法。Monitor 还是 .NET
    /// 中最轻量的"条件变量"：<see cref="Monitor.Wait(object)"/> 释放锁并等待通知，
    /// <see cref="Monitor.Pulse(object)"/> / <see cref="Monitor.PulseAll(object)"/> 唤醒等待线程。
    /// 【是否跨进程】否。锁状态存在于 CLR 托管对象头（同步块索引）中，仅同一进程内可见；
    /// 跨进程互斥请用 <see cref="Mutex"/>。
    /// 【典型适用场景】
    /// <list type="bullet">
    /// <item>保护进程内共享状态（计数器、集合、非线程安全对象），同一时刻只允许一个线程进入临界区。</item>
    /// <item>需要"尝试加锁 + 毫秒超时 + 失败降级"（<see cref="Monitor.TryEnter(object,int)"/>）。</item>
    /// <item>生产者/消费者式的条件等待（Wait/Pulse/PulseAll），更现代的替代是
    /// <c>Monitor.Pulse</c> 封装或 <c>System.Threading.Monitor</c> 之外的 <c>SemaphoreSlim</c>、
    /// <c>ManualResetEventSlim</c>、通道等，但 Wait/Pulse 不分配内核句柄。</item>
    /// </list>
    /// 【使用步骤】
    /// <list type="number">
    /// <item>声明 <c>private readonly object syncRoot = new object();</c> 作为专用锁对象。</item>
    /// <item>进入临界区：日常直接 <c>lock</c>；需要超时用 <c>Monitor.TryEnter</c>；
    /// 需要可靠异常语义用 <c>Monitor.Enter(obj, ref lockTaken)</c>（.NET Framework 4.0+）。</item>
    /// <item>在 finally 中配对调用 <see cref="Monitor.Exit(object)"/>，Enter 与 Exit 必须同线程、同对象、次数相同。</item>
    /// <item>条件等待：<c>lock</c> 内用 <c>while (!条件) Monitor.Wait(...) ;</c>（while 不是 if，防伪唤醒/条件被抢）。</item>
    /// <item>通知：<c>lock</c> 内先改条件再 <c>Monitor.Pulse</c>（唤醒一个）或 <c>PulseAll</c>（唤醒全部）。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>锁对象必须是 private readonly 的引用类型，禁止锁 <c>this</c>、<c>typeof(T)</c>、字符串（字符串留用，
    /// 可能与外部代码同一把锁）和值类型（值类型会被装箱，每次锁的都是新对象，完全失效）。</item>
    /// <item>Pulse 是"边沿信号"不计数：Pulse 时没有等待者，信号直接丢失。所以必须"先判条件再等待、
    /// 持锁改条件再 Pulse"，等待方一律 while 循环重判条件。</item>
    /// <item>Wait 会原子地"释放锁并阻塞"，被唤醒后必须重新拿到锁才返回；可能存在伪唤醒，绝不能用 if 判一次条件。</item>
    /// <item>避免在持锁区域内调用未知虚方法/委托、阻塞 I/O，防止死锁与锁护送（lock convoy）。</item>
    /// <item>锁是可重入的：同一线程可重复 Enter，但必须相同次数 Exit。</item>
    /// </list>
    /// 【版本可用性】Monitor 自 .NET Framework 1.1 起提供；Enter/Exit、TryEnter 各超时重载、Wait 各重载、
    /// Pulse/PulseAll 在 2.0 即齐全；<c>Enter(obj, ref lockTaken)</c> 与带 lockTaken 的 TryEnter 重载为
    /// 4.0 新增；<see cref="Monitor.IsEntered(object)"/> 为 4.5 新增。本类全部示例均基于 .NET Framework 4.8。
    /// </summary>
    /// <example>
    /// <code>
    /// private readonly object syncRoot = new object();
    ///
    /// public void SafeUpdate()
    /// {
    ///     lock (syncRoot)            // 等价 Monitor.Enter + try/finally + Monitor.Exit
    ///     {
    ///         // 临界区：同一时刻只有一个线程能进入
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class HMonitorHelp
    {
        /// <summary>示例1：标准 lock 临界区（最常用，等价 Monitor.Enter/Exit 的 try-finally 可靠写法）。</summary>
        /// <remarks>本示例新建局部锁对象，立即加锁立即释放，可安全重复调用，不会阻塞。</remarks>
        public static void LockExample()
        {
            object syncRoot = new object();   // 专用锁对象：private readonly object 的方法内演示版
            int counter = 0;
            lock (syncRoot)
            {
                counter++;                    // 临界区：多线程串行执行
            }
        }

        /// <summary>
        /// 示例2：.NET Framework 4.0 起的可靠 Enter 模式 —— <c>Enter(obj, ref lockTaken)</c>。
        /// 即使 Enter 与临界区之间发生异步异常（如 ThreadAbortException），也能凭 lockTaken
        /// 判断锁是否真的拿到，避免"没拿到锁却 Exit"抛 <see cref="SynchronizationLockException"/>。
        /// </summary>
        /// <param name="syncRoot">锁对象，应为 private readonly 的引用类型实例，不能为 null。</param>
        /// <param name="criticalWork">临界区工作；在持锁状态下同步执行。</param>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 或 <paramref name="criticalWork"/> 为 null。</exception>
        public static void EnterReliableExample(object syncRoot, Action criticalWork)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            if (criticalWork == null)
            {
                throw new ArgumentNullException(nameof(criticalWork));
            }

            bool lockTaken = false;           // 必须先初始化为 false
            try
            {
                Monitor.Enter(syncRoot, ref lockTaken);  // 只有真正拿到锁时 lockTaken 才会被置为 true
                criticalWork();
            }
            finally
            {
                if (lockTaken)                 // 没拿到锁绝不 Exit
                {
                    Monitor.Exit(syncRoot);
                }
            }
        }

        /// <summary>示例3：<see cref="Monitor.TryEnter(object)"/> 立即尝试一次（等价超时 0），拿不到锁立刻返回 false，不阻塞。</summary>
        /// <param name="syncRoot">锁对象，不能为 null。</param>
        /// <param name="criticalWork">拿到锁后执行的临界区工作。</param>
        /// <returns>true 表示成功加锁并执行完毕；false 表示锁正被占用，本次直接放弃。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 或 <paramref name="criticalWork"/> 为 null。</exception>
        public static bool TryEnterImmediateExample(object syncRoot, Action criticalWork)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            if (criticalWork == null)
            {
                throw new ArgumentNullException(nameof(criticalWork));
            }

            if (!Monitor.TryEnter(syncRoot))   // 无参重载：立即返回，不等待
            {
                return false;                  // 可走降级逻辑：稍后重试、排队、报错
            }
            try
            {
                criticalWork();
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
            return true;
        }

        /// <summary>示例4：<see cref="Monitor.TryEnter(object,int)"/> 带毫秒超时尝试加锁，超时未拿到不傻等，避免死等/死锁。</summary>
        /// <param name="syncRoot">锁对象，不能为 null。</param>
        /// <param name="timeoutMs">等待毫秒数；0 表示不等待，<see cref="Timeout.Infinite"/> (-1) 表示无限等待（示例不建议）。</param>
        /// <param name="criticalWork">拿到锁后执行的临界区工作。</param>
        /// <returns>true 表示在超时内拿到锁并执行完毕；false 表示超时放弃。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 或 <paramref name="criticalWork"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 不是非负数或 -1。</exception>
        public static bool TryEnterExample(object syncRoot, int timeoutMs, Action criticalWork)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            if (criticalWork == null)
            {
                throw new ArgumentNullException(nameof(criticalWork));
            }

            if (!Monitor.TryEnter(syncRoot, timeoutMs))
            {
                return false;                  // 超时未拿到锁，放弃或走降级逻辑
            }
            try
            {
                criticalWork();
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
            return true;
        }

        /// <summary>示例5：<see cref="Monitor.TryEnter(object,TimeSpan)"/> 用 <see cref="TimeSpan"/> 表达超时的重载，语义与毫秒版一致。</summary>
        /// <param name="syncRoot">锁对象，不能为 null。</param>
        /// <param name="timeout">超时时长；必须介于零与 <see cref="int.MaxValue"/> 毫秒之间，或表示无限等待的 -1 毫秒。</param>
        /// <param name="criticalWork">拿到锁后执行的临界区工作。</param>
        /// <returns>true 表示在超时内拿到锁并执行完毕；false 表示超时放弃。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 或 <paramref name="criticalWork"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 超出合法范围。</exception>
        public static bool TryEnterTimeSpanExample(object syncRoot, TimeSpan timeout, Action criticalWork)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            if (criticalWork == null)
            {
                throw new ArgumentNullException(nameof(criticalWork));
            }

            if (!Monitor.TryEnter(syncRoot, timeout))
            {
                return false;
            }
            try
            {
                criticalWork();
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
            return true;
        }

        /// <summary>条件标志容器：lambda/匿名方法中不能捕获 ref 变量，用对象字段承载被等待的共享条件。</summary>
        public sealed class ConditionFlag
        {
            /// <summary>条件是否已满足（所有读写均应在持有外层锁时进行）。</summary>
            public bool Ready;
        }

        /// <summary>
        /// 示例6：Monitor.Wait/Pulse 线程条件通知：消费者在 lock 内 while 判条件并等待，生产者改变条件后唤醒。
        /// 即使 Pulse 先于消费者进入等待发出，消费者随后进入时也会因 while 重判 <see cref="ConditionFlag.Ready"/>
        /// 已为 true 而不会挂死（这就是必须用 while 而非 if 的原因）。
        /// </summary>
        /// <param name="syncRoot">等待与通知双方共用的锁对象。</param>
        /// <param name="flag">被等待的共享条件。</param>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 或 <paramref name="flag"/> 为 null。</exception>
        public static void WaitPulseExample(object syncRoot, ConditionFlag flag)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }

            // 等待方
            ThreadPool.QueueUserWorkItem(delegate
            {
                lock (syncRoot)
                {
                    while (!flag.Ready)
                    {
                        Monitor.Wait(syncRoot);   // 原子地"释放锁并等待"，被 Pulse 唤醒后重新拿到锁才返回
                    }
                }
            });

            // 通知方
            lock (syncRoot)
            {
                flag.Ready = true;                // 必须在持锁状态下修改条件
                Monitor.Pulse(syncRoot);          // 唤醒一个等待线程；广播全部等待者用 PulseAll
            }
        }

        /// <summary>
        /// 示例7：带毫秒超时的条件等待（<see cref="Monitor.Wait(object,int)"/>），返回 false 表示超时，
        /// 绝不无限期死等。注意：调用本方法前当前线程必须已持有 <paramref name="syncRoot"/>。
        /// </summary>
        /// <param name="syncRoot">已被当前线程持有的锁对象。</param>
        /// <param name="flag">被等待的共享条件。</param>
        /// <param name="timeoutMs">单次等待毫秒数（超时返回 false）。</param>
        /// <returns>true 表示条件已满足；false 表示超时仍未满足。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 或 <paramref name="flag"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 非法。</exception>
        /// <exception cref="SynchronizationLockException">当前线程未持有 <paramref name="syncRoot"/>。</exception>
        public static bool WaitTimeoutExample(object syncRoot, ConditionFlag flag, int timeoutMs)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            if (flag == null)
            {
                throw new ArgumentNullException(nameof(flag));
            }
            if (!Monitor.IsEntered(syncRoot))
            {
                throw new SynchronizationLockException(HTranslation.GetContent("调用 Monitor.Wait 前当前线程必须先 lock/Enter 同一对象。"));
            }

            while (!flag.Ready)
            {
                // Wait 超时返回 false；被唤醒返回 true（可能伪唤醒，所以外层必须 while 重判条件）
                if (!Monitor.Wait(syncRoot, timeoutMs))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 示例8：<see cref="Monitor.PulseAll(object)"/> 广播唤醒 —— 多个等待者同时等待同一条件时，
        /// 用 PulseAll 一次性唤醒全部线程（它们仍需排队逐个重新获取锁）。本示例自包含、可安全自测：
        /// 每个等待者都带毫秒超时，主线程 Join 也带超时，任何时序下都不会死锁。
        /// </summary>
        /// <param name="waiterCount">等待线程数量，应为非负数。</param>
        /// <param name="waitTimeoutMs">等待者单次 Wait 的超时毫秒数。</param>
        /// <param name="joinTimeoutMs">主线程等待全部工作线程结束的 Join 超时毫秒数。</param>
        /// <returns>被成功唤醒并通过条件检查的等待者数量。</returns>
        /// <exception cref="ArgumentOutOfRangeException">参数为负数。</exception>
        public static int PulseAllWaitersExample(int waiterCount, int waitTimeoutMs, int joinTimeoutMs)
        {
            if (waiterCount < 0 || waitTimeoutMs < 0 || joinTimeoutMs < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            object syncRoot = new object();
            ConditionFlag flag = new ConditionFlag();
            int wokeCount = 0;
            Thread[] waiters = new Thread[waiterCount];

            for (int i = 0; i < waiterCount; i++)
            {
                Thread waiter = new Thread(delegate ()
                {
                    lock (syncRoot)
                    {
                        while (!flag.Ready)
                        {
                            if (!Monitor.Wait(syncRoot, waitTimeoutMs))
                            {
                                return;             // 超时放弃，绝不死等
                            }
                        }
                        wokeCount++;                // 已在锁内，自增无需 Interlocked
                    }
                });
                waiter.IsBackground = true;
                waiters[i] = waiter;
                waiter.Start();
            }

            Thread.Sleep(10);                       // 演示用途：尽量让等待者先进入等待队列
            lock (syncRoot)
            {
                flag.Ready = true;                  // 先改条件
                Monitor.PulseAll(syncRoot);         // 再广播：唤醒等待队列中的全部线程
            }

            for (int i = 0; i < waiters.Length; i++)
            {
                waiters[i].Join(joinTimeoutMs);     // 带超时收尾，避免调用方被无限挂住
            }
            return wokeCount;
        }

        /// <summary>
        /// 示例9：<see cref="Monitor.IsEntered(object)"/>（.NET Framework 4.5+）判断当前线程是否持有指定锁。
        /// 仅适合断言/诊断（如 Debug.Assert），不要据此做加锁决策——返回后锁状态可能立刻变化。
        /// </summary>
        /// <param name="syncRoot">锁对象，不能为 null。</param>
        /// <returns>当前线程持有该锁返回 true；否则 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="syncRoot"/> 为 null。</exception>
        public static bool IsLockHeld(object syncRoot)
        {
            if (syncRoot == null)
            {
                throw new ArgumentNullException(nameof(syncRoot));
            }
            return Monitor.IsEntered(syncRoot);
        }
    }
}
