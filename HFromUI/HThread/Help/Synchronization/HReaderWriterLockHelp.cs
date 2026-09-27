using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】<see cref="ReaderWriterLock"/> 帮助类：.NET Framework 1.1 时代引入的“老式”读写锁
    /// （纯托管实现）。允许多个线程同时持有读锁（共享），写锁则排他（同一时刻仅一个写者，且持写锁时
    /// 不允许任何读者）。典型成员：AcquireReaderLock(int)/AcquireWriterLock(int) 加锁、
    /// ReleaseReaderLock/ReleaseWriterLock 解锁、UpgradeToWriterLock(out LockCookie) 读锁升级为写锁、
    /// DowngradeFromWriterLock(ref LockCookie) 再降回读锁、ReleaseLock 释放锁计数后用 LockCookie 恢复、
    /// IsReaderLockHeld/IsWriterLockHeld 判断当前线程持锁状态、WriterSeqNum 读取当前写序号
    /// （配合 AnyWritersSince 可判断自上次获取序号后是否有写者来过，常用于读缓存失效判断）。
    ///
    /// 【是否跨进程】否。纯进程内托管锁，不能命名、不能跨进程。
    ///
    /// 【典型适用场景】【仅维护老代码】读多写少、且历史代码已经在使用 ReaderWriterLock 的场合，
    /// 用于阅读和安全修改既有逻辑；新写的读写锁代码请一律使用
    /// <see cref="HReaderWriterLockSlimHelp"/>（<see cref="ReaderWriterLockSlim"/>）。
    ///
    /// 【使用步骤】（维护老代码时的标准配对）
    /// 1) new ReaderWriterLock()；
    /// 2) 读路径：AcquireReaderLock(毫秒超时) → 读数据 → finally 中 ReleaseReaderLock；
    /// 3) 写路径：AcquireWriterLock(毫秒超时) → 写数据 → finally 中 ReleaseWriterLock；
    /// 4) 持读锁途中发现必须写：LockCookie cookie; UpgradeToWriterLock(超时, out cookie) → 写 →
    ///    finally 中 DowngradeFromWriterLock(ref cookie) 降回读锁，最后仍要 ReleaseReaderLock；
    /// 5) 所有锁都支持递归重入，但必须按相同次数/正确顺序释放。
    ///
    /// 【注意事项与坑】
    /// 1) 【历史包袱重，新代码禁用】该类型存在写者饥饿（writer starvation）问题：
    ///    读者源源不断时写者可能长期拿不到锁；递归锁语义混乱（锁计数升级/恢复容易错配）；
    ///    性能也比后出的 Slim 版本差（内部甚至借助内核事件）。微软官方明确建议新代码改用
    ///    <see cref="ReaderWriterLockSlim"/>（见 <see cref="HReaderWriterLockSlimHelp"/>）；
    /// 2) 【超时抛 ApplicationException】这是老 API 的非惯例设计：AcquireReaderLock/AcquireWriterLock/
    ///    UpgradeToWriterLock 超时不返回 bool，而是抛 <see cref="ApplicationException"/>，catch 时按此处理；
    /// 3) 支持锁递归：同线程可重复获取读锁/写锁（写锁可再次获取读锁），但释放次数必须匹配，
    ///    跨线程释放或未持锁释放会抛 <see cref="ApplicationException"/>/<see cref="SynchronizationLockException"/>；
    /// 4) DowngradeFromWriterLock 的参数是 ref LockCookie，必须传入对应 UpgradeToWriterLock 给出的 Cookie，
    ///    Cookie 是结构体，不要跨线程共享；
    /// 5) 写者锁内异常不会自动释放锁，务必 try/finally；
    /// 6) ReaderWriterLock 不实现 IDisposable，它没有内核句柄所有权语义，无需 using。
    ///
    /// 【版本可用性】<see cref="ReaderWriterLock"/> 自 .NET Framework 1.1 起提供，net48 内置；
    /// .NET Core/.NET 5+ 中仍保留该类型（兼容老代码），但同样不推荐使用。
    /// 替代类型 <see cref="ReaderWriterLockSlim"/> 自 .NET Framework 3.5 起提供。
    /// </summary>
    /// <example>
    /// 示例1：标准多读单写配对。
    /// <code>
    /// ReaderWriterLock rwLock = new ReaderWriterLock();
    /// double cache = 0;
    ///
    /// // 读路径：多个读者可并发
    /// rwLock.AcquireReaderLock(1000);
    /// try
    /// {
    ///     double snapshot = cache;
    /// }
    /// finally
    /// {
    ///     rwLock.ReleaseReaderLock();
    /// }
    ///
    /// // 写路径：排他
    /// rwLock.AcquireWriterLock(1000);
    /// try
    /// {
    ///     cache = cache + 1.0;
    /// }
    /// finally
    /// {
    ///     rwLock.ReleaseWriterLock();
    /// }
    /// </code>
    /// 示例2：用 LockCookie 在读锁内升级为写锁，再降级回读锁。
    /// <code>
    /// rwLock.AcquireReaderLock(1000);
    /// try
    /// {
    ///     LockCookie cookie = rwLock.UpgradeToWriterLock(1000);
    ///     try
    ///     {
    ///         // 此时当前线程独占写锁
    ///     }
    ///     finally
    ///     {
    ///         rwLock.DowngradeFromWriterLock(ref cookie);
    ///     }
    ///     // 此时重新持有读锁
    /// }
    /// finally
    /// {
    ///     rwLock.ReleaseReaderLock();
    /// }
    /// </code>
    /// </example>
    public static class HReaderWriterLockHelp
    {
        /// <summary>
        /// 示例1：多读单写自包含演示。锁对象在方法内部 new，先启动 3 个后台读者线程并发持有读锁
        /// （读者之间不互斥，各自留驻 20ms 以体现可并发），全部释放后当前线程再获取写锁修改数据。
        /// 所有获取锁调用均带 1000ms 超时（老 API 超时抛 <see cref="ApplicationException"/>，
        /// 工作线程内捕获后直接结束，主线程路径按正常逻辑执行），线程 Join 带超时，2 秒内必然结束。
        /// </summary>
        /// <returns>写者自增完成后的共享数据值，正常情况下固定返回 1。</returns>
        /// <exception cref="ApplicationException">当前线程获取写锁超时（正常时序下不会发生）。</exception>
        public static int MultipleReadersOneWriter()
        {
            const int readerCount = 3;                // 固定 3 个读者，方法完全自包含

            ReaderWriterLock rwLock = new ReaderWriterLock();
            int sharedData = 0;                      // 被读写锁保护的共享数据
            int[] snapshots = new int[readerCount];  // 各读者读到的快照
            Thread[] readers = new Thread[readerCount];

            for (int i = 0; i < readerCount; i++)
            {
                int slot = i;                        // 闭包捕获需要每份循环独立副本
                readers[i] = new Thread((ThreadStart)delegate
                {
                    try
                    {
                        rwLock.AcquireReaderLock(1000);   // 多个读者可同时进入
                        try
                        {
                            snapshots[slot] = sharedData;
                            Thread.Sleep(20);              // 持读锁短暂留驻，验证读者之间不互斥
                        }
                        finally
                        {
                            rwLock.ReleaseReaderLock();   // 与 AcquireReaderLock 严格配对
                        }
                    }
                    catch (ApplicationException)
                    {
                        // 老惯例：获取读锁超时抛 ApplicationException（本示例不会超时，仅示意 catch 写法）
                    }
                }) { IsBackground = true };
                readers[i].Start();
            }

            for (int i = 0; i < readers.Length; i++)
            {
                readers[i].Join(2000);              // 带超时回收全部读者线程
            }

            // 所有读者结束后获取写锁：写锁排他，与任何读/写都互斥
            rwLock.AcquireWriterLock(1000);
            try
            {
                sharedData++;                        // 临界区写操作
            }
            finally
            {
                rwLock.ReleaseWriterLock();         // 与 AcquireWriterLock 严格配对
            }

            return sharedData;
        }

        /// <summary>
        /// 示例2：读锁升级为写锁、再降级回读锁的完整 LockCookie 生命周期。
        /// 流程：AcquireReaderLock → UpgradeToWriterLock(out cookie) → 写 →
        /// DowngradeFromWriterLock(ref cookie) → ReleaseReaderLock。
        /// 降级后当前线程重新持有读锁；最终释放后不持有任何锁。
        /// </summary>
        /// <returns>升级期间写入成功（数据为 1）、最终读锁/写锁均已正确释放时返回 true。</returns>
        /// <exception cref="ApplicationException">获取读锁、升级写锁超时（正常时序下不会发生）。</exception>
        public static bool UpgradeWithLockCookie()
        {
            ReaderWriterLock rwLock = new ReaderWriterLock();
            int sharedData = 0;

            rwLock.AcquireReaderLock(1000);          // 1) 先以读者身份进入
            try
            {
                // 2) 持读锁升级：out 给出 Cookie，升级期间当前线程独占写锁
                LockCookie cookie = rwLock.UpgradeToWriterLock(1000);
                try
                {
                    sharedData++;                    // 3) 排他写操作
                }
                finally
                {
                    // 4) 凭 Cookie 降级回读锁：ref 传回填好的 Cookie
                    rwLock.DowngradeFromWriterLock(ref cookie);
                }

                // 降级后这里又持有读锁，可继续一致性读取
                int snapshotAfterDowngrade = sharedData;

                // 读锁最终在最外层 finally 中释放
                if (snapshotAfterDowngrade != 1)
                {
                    return false;
                }
            }
            finally
            {
                rwLock.ReleaseReaderLock();          // 5) 配对释放最初的读锁
            }

            // 释放后当前线程不应再持有任何锁
            return !rwLock.IsReaderLockHeld && !rwLock.IsWriterLockHeld;
        }
    }
}
