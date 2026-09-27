using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】C# <c>lock</c> 关键字帮助类：<c>lock</c> 不是独立的同步原语，它只是编译器提供的语法糖，
    /// 展开后等价于 <see cref="Monitor.Enter(object, ref bool)"/> + try/finally + <see cref="Monitor.Exit(object)"/>
    /// 的可靠写法，锁状态记录在托管对象头的同步块中（详见 <see cref="HMonitorHelp"/>）。
    /// 典型写法 <c>lock (syncRoot) { 临界区 }</c> 保证：同一时刻只有一个线程进入临界区；
    /// 临界区抛异常时 finally 仍会释放锁；锁是可重入的（同一线程可重复进入，Exit 次数匹配即可）。
    ///
    /// 【是否跨进程】否。<c>lock</c>/Monitor 完全是 CLR 进程内机制，锁对象不能跨 AppDomain/进程共享；
    /// 需要跨进程互斥请使用命名 <see cref="Mutex"/>（见 HMutexHelp）。
    ///
    /// 【典型适用场景】
    /// 1) 保护进程内共享字段/集合/非线程安全对象（计数器、缓存、字典等），做短小的读写临界区；
    /// 2) 需要可重入互斥、且等待时间极短的代码路径；
    /// 3) 配合 Monitor.Wait/Pulse 做条件变量（条件等待本身见 HMonitorHelp，本类只讲 lock 语法）。
    ///
    /// 【使用步骤】
    /// 1) 在类型中声明专用锁对象：<c>private readonly object syncRoot = new object();</c>；
    /// 2) 用 <c>lock (syncRoot) { ... }</c> 包裹所有访问共享状态的代码路径，读和写都要进同一把锁；
    /// 3) 需要“尝试加锁 + 超时 + 失败降级”时不要用 lock 语句，直接用
    ///    <see cref="Monitor.TryEnter(object, int, ref bool)"/>（见本类 <see cref="TryExecuteWithTimeout"/>）；
    /// 4) 保持临界区尽量短：不要在锁内做阻塞 I/O、调用未知虚方法/委托、遍历耗时大数据。
    ///
    /// 【注意事项与坑】
    /// 1) 锁对象必须是 private readonly object（私有、只读、引用类型、专用）。绝不要：
    ///    lock(this)——外部对象也能拿你的 this 当锁，造成死锁；
    ///    lock(typeof(T))——Type 对象跨 AppDomain/被所有人共享，相当于公开锁；
    ///    lock("字符串")——CLR 字符串留用（interning），两处相同字面量可能是同一把锁；
    ///    lock(值类型)——值类型每次装箱成新对象，锁了等于没锁；
    /// 2) 锁内禁止 await：C# 编译器直接报错（Monitor 必须由 Enter 的同一线程 Exit，
    ///    而 await 恢复后可能跑在另一个线程池线程上）。需要异步互斥请用 SemaphoreSlim.WaitAsync；
    /// 3) 锁内禁止 Thread.Abort 式编程：线程在持锁区被强制中止可能造成锁护送/状态破坏，
    ///    .NET Core/.NET 5+ 中 Thread.Abort 已不支持（抛 PlatformNotSupportedException）；
    /// 4) 保持锁内代码短：持锁期间其他线程全部排队，临界区越长争用越严重；
    /// 5) lock 不支持超时；要超时/探测必须手写 Monitor.TryEnter，并像示例一样用 lockTaken + finally。
    ///
    /// 【版本可用性】<c>lock</c> 语句自 C# 1.0 起提供；编译器自 C# 4.0 起展开为带 lockTaken 的可靠
    /// Enter 重载（.NET Framework 4.0+，net48 即为该形态）。.NET 9 新增专用值类型锁
    /// System.Threading.Lock（提供 Enter/TryEnter/EnterScope，lock 语句可直接锁它，线程争用下性能更好），
    /// net48【不可用】，仅在下方 example 第 3 段给出示例供升级参考。
    /// </summary>
    /// <example>
    /// 示例1：最常见的 lock 用法。
    /// <code>
    /// private readonly object syncRoot = new object();
    /// private int counter;
    ///
    /// public int Next()
    /// {
    ///     lock (syncRoot)
    ///     {
    ///         return ++counter;   // 同一时刻只有一个线程能自增
    ///     }
    /// }
    /// </code>
    /// 示例2：lock 语句的编译器等价展开（手写时照此模板，注意 lockTaken 必须先置 false）。
    /// <code>
    /// bool lockTaken = false;
    /// try
    /// {
    ///     Monitor.Enter(syncRoot, ref lockTaken);
    ///     // 临界区
    /// }
    /// finally
    /// {
    ///     if (lockTaken)
    ///     {
    ///         Monitor.Exit(syncRoot);   // 只有真正拿到锁才 Exit
    ///     }
    /// }
    /// </code>
    /// 示例3：仅 .NET 9+ 可用的 System.Threading.Lock（net48 不可用，仅供参考）。
    /// <code>
    /// // .NET 9+：专用锁类型，lock 语句可以直接锁它
    /// System.Threading.Lock spinLock = new System.Threading.Lock();
    ///
    /// lock (spinLock)
    /// {
    ///     // 临界区
    /// }
    ///
    /// // EnterScope 返回一个令牌，using 块结束自动释放
    /// using (spinLock.EnterScope())
    /// {
    ///     // 临界区
    /// }
    ///
    /// // 非阻塞尝试
    /// if (spinLock.TryEnter())
    /// {
    ///     try
    ///     {
    ///         // 临界区
    ///     }
    ///     finally
    ///     {
    ///         spinLock.Exit();
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class HLockHelp
    {
        /// <summary>专用锁对象：必须 private readonly，且不要把它暴露给类外部。</summary>
        private static readonly object syncRoot = new object();

        /// <summary>被 <see cref="syncRoot"/> 保护的共享计数器，仅允许在 lock 内读写。</summary>
        private static int counter;

        /// <summary>双检锁演示用的延迟资源；volatile 保证锁外读取能看到已完整构造的对象。</summary>
        private static volatile object lazyResource;

        /// <summary>
        /// 示例1：最直接的 lock 自增。每次调用在 lock 内对共享计数器自增并返回自增后的值，
        /// 多线程并发调用也不会出现丢失更新/重复值。
        /// </summary>
        /// <returns>自增后的计数器值（从 1 开始的单调递增值，静态计数器多次调用会持续累加）。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        public static int SafeIncrement()
        {
            // lock 保证自增“读取-修改-写回”三步整体串行，等价于 Interlocked.Increment 的演示版
            lock (syncRoot)
            {
                return ++counter;
            }
        }

        /// <summary>
        /// 示例2：多线程并发版本的 lock 自增演示。方法入口先把计数器归零（自包含、不依赖上次调用状态），
        /// 启动 <paramref name="threadCount"/> 个后台线程，每个线程在同一把 lock 内自增一次，
        /// 最后等待所有线程结束并返回计数器终值。若无丢失更新，返回值应等于实际启动的线程数。
        /// </summary>
        /// <param name="threadCount">并发自增线程数；为避免误传超大值，内部上限截断为 64。</param>
        /// <returns>所有线程自增完成后的计数器值；线程未能在 2000ms 内全部结束时返回当时的部分结果。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="threadCount"/> 为负数。</exception>
        public static int IncrementInParallel(int threadCount)
        {
            if (threadCount < 0)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("线程数量不能小于 0"));
            }

            // 超大入参截断，防止反射/误调用时创建海量线程
            int workers = threadCount > 64 ? 64 : threadCount;

            lock (syncRoot)
            {
                counter = 0;                 // 入口归零，保证本方法自包含、可重复调用
            }

            Thread[] threads = new Thread[workers];
            for (int i = 0; i < workers; i++)
            {
                threads[i] = new Thread((ThreadStart)delegate
                {
                    lock (syncRoot)
                    {
                        counter++;           // 所有线程抢同一把锁，自增不会丢失
                    }
                }) { IsBackground = true };
                threads[i].Start();
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join(2000);       // 带超时回收，绝不无限等待
            }

            lock (syncRoot)
            {
                return counter;
            }
        }

        /// <summary>
        /// 示例3：带毫秒超时的尝试加锁（lock 语句本身不支持超时，必须手写
        /// <see cref="Monitor.TryEnter(object, int, ref bool)"/>）。在超时内拿到锁则执行
        /// <c>counter++</c> 并返回 true；超时未拿到锁立即返回 false，绝不死等。
        /// </summary>
        /// <param name="timeoutMs">
        /// 等待毫秒数：0 表示仅探测（锁空闲才进入），<see cref="Timeout.Infinite"/> (-1) 表示无限等待，
        /// 其余非 -1 的负数非法。
        /// </param>
        /// <returns>在超时内拿到锁并完成自增返回 true；超时放弃返回 false。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 是除 -1 外的负数。</exception>
        public static bool TryExecuteWithTimeout(int timeoutMs)
        {
            if (timeoutMs < -1)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("超时时间不能小于 -1 毫秒"));
            }

            bool lockTaken = false;           // 必须先初始化为 false
            try
            {
                // 带 lockTaken 的重载：只有真正拿到锁，lockTaken 才会被置为 true
                Monitor.TryEnter(syncRoot, timeoutMs, ref lockTaken);
                if (lockTaken)
                {
                    counter++;                // 拿到锁才执行临界区工作
                }
                return lockTaken;             // false 表示超时，调用方可走降级逻辑
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);   // 仅在确实持锁时释放，避免 SynchronizationLockException
                }
            }
        }

        /// <summary>
        /// 示例4：双检锁定（double-checked locking）懒初始化模式。锁外先读一次共享字段，
        /// 为 null 才加锁；进入锁后【必须再判一次】，防止多个线程同时通过第一层检查导致重复创建。
        /// 生产代码中等价需求优先用 <c>Lazy&lt;T&gt;</c>，本方法仅演示经典手写模式。
        /// </summary>
        /// <returns>
        /// 已初始化资源的标识字符串（类型名 + GetHashCode）；多次调用返回同一对象的同一标识。
        /// </returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        public static string DoubleCheckLockExample()
        {
            // 第一层检查（锁外）：资源已初始化时绝大多数调用直接走这里，避免每次加锁
            object resource = lazyResource;
            if (resource == null)
            {
                lock (syncRoot)
                {
                    // 第二层检查：拿到锁后必须重判，期间资源可能已被其他线程初始化
                    resource = lazyResource;
                    if (resource == null)
                    {
                        resource = new object();   // 先在局部变量中完整构造
                        lazyResource = resource;   // 再发布；字段声明为 volatile，锁外读取安全
                    }
                }
            }
            return resource.GetType().Name + "_" + resource.GetHashCode();
        }
    }
}
