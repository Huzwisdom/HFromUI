using System;
using System.Collections.Generic;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】ReaderWriterLockSlim（轻量读写锁）帮助类：区分"读锁/写锁/可升级读锁"三种进入模式。
    /// 读锁共享——多个读线程可同时持有；写锁独占——持写锁时其他读、写全部等待；
    /// 可升级读锁同一时刻只允许一个线程持有，线程可在其内部再进入写锁完成"先查后改"。
    /// 【是否跨进程】否。纯进程内托管锁（用于替代老旧的 ReaderWriterLock，性能与递归语义更好）。
    /// 【典型适用场景】读多写少的共享结构：缓存、配置字典、路由表等。相比所有操作一律互斥的 lock，
    /// 读路径可真正并发。
    /// 【使用步骤】
    /// <list type="number">
    /// <item>构造 <c>new ReaderWriterLockSlim()</c>（默认 <see cref="LockRecursionPolicy.NoRecursion"/>）
    /// 或 <c>new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)</c>。</item>
    /// <item>读：EnterReadLock / TryEnterReadLock(int|TimeSpan)，finally 中 ExitReadLock。</item>
    /// <item>写：EnterWriteLock / TryEnterWriteLock(int|TimeSpan)，finally 中 ExitWriteLock。</item>
    /// <item>先查后改：EnterUpgradeableReadLock，查后确需修改再 EnterWriteLock，退出顺序相反，
    /// finally 中先 ExitWriteLock 再 ExitUpgradeableReadLock。</item>
    /// <item>实例用完 Dispose。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>默认 NoRecursion 策略下同模式重入会抛 <see cref="LockRecursionException"/>；
    /// "可升级读 → 写"是唯一定义好的升级路径，其他模式组合升级都会抛异常。</item>
    /// <item>SupportsRecursion 允许重入但有性能损耗、且更容易写出死锁，非必要不使用。</item>
    /// <item>Enter 与 Exit 必须严格配对（含异常路径），少 Exit 一次会导致锁永远被持有；
    /// 在未持对应锁时调用 Exit 抛 <see cref="SynchronizationLockException"/>。</item>
    /// <item>写饥饿：持续不断的读可能使写线程长时间等待，可用 WaitingWriteCount 监控，
    /// 或给 TryEnter* 统一加超时。</item>
    /// <item>不要在锁内执行长时间 I/O 或未知回调；该锁非线程亲和但进入/退出须在同一线程配对。</item>
    /// </list>
    /// 【版本可用性】ReaderWriterLockSlim 自 .NET Framework 3.5（实际 3.5/4.0 系列广泛使用）；
    /// 本类用到的全部成员在 4.8 可用。CurrentReadCount 在持写锁时为 -1。
    /// </summary>
    /// <example>
    /// <code>
    /// private readonly ReaderWriterLockSlim rw = new ReaderWriterLockSlim();
    /// public T Read()
    /// {
    ///     rw.EnterReadLock();
    ///     try { return data; }
    ///     finally { rw.ExitReadLock(); }
    /// }
    /// </code>
    /// </example>
    public static class HReaderWriterLockSlimHelp
    {
        /// <summary>示例1：读多写少的线程安全缓存（嵌套类：读共享、写独占、可升级读做 GetOrAdd）。</summary>
        /// <typeparam name="T">缓存值类型。</typeparam>
        public class ReadManyCache<T> : IDisposable
        {
            private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
            private readonly Dictionary<string, T> data = new Dictionary<string, T>();
            private bool disposed;

            /// <summary>读：多线程可同时持有读锁并发查询。</summary>
            /// <param name="key">缓存键。</param>
            /// <param name="value">输出：找到的值；未找到为类型默认值。</param>
            /// <returns>找到返回 true；否则 false。</returns>
            /// <exception cref="ObjectDisposedException">缓存已释放。</exception>
            public bool TryGet(string key, out T value)
            {
                rwLock.EnterReadLock();
                try
                {
                    return data.TryGetValue(key, out value);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }

            /// <summary>写：独占进入，其他读、写线程全部等待。</summary>
            /// <param name="key">缓存键。</param>
            /// <param name="value">缓存值。</param>
            /// <exception cref="ObjectDisposedException">缓存已释放。</exception>
            public void Set(string key, T value)
            {
                rwLock.EnterWriteLock();
                try
                {
                    data[key] = value;
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }

            /// <summary>
            /// 升级锁写法：先以可升级读模式"检查不存在则添加"。可升级读持有时其他读线程仍可并发，
            /// 且同一时刻只有一个可升级读，升级到写锁期间不会被其他写插队；比"退出读锁再抢写锁"更安全。
            /// </summary>
            /// <param name="key">缓存键。</param>
            /// <param name="factory">未命中时创建值的工厂（仅在升级到写锁后调用）。</param>
            /// <returns>已有或新创建的值。</returns>
            /// <exception cref="ArgumentNullException"><paramref name="factory"/> 为 null。</exception>
            public T GetOrAdd(string key, Func<T> factory)
            {
                if (factory == null)
                {
                    throw new ArgumentNullException(nameof(factory));
                }

                rwLock.EnterUpgradeableReadLock();
                try
                {
                    T value;
                    if (data.TryGetValue(key, out value))
                    {
                        return value;                       // 只读，无需升级
                    }
                    rwLock.EnterWriteLock();               // 升级为写锁（NoRecursion 下唯一允许的升级路径）
                    try
                    {
                        // 双重检查：进入写锁前不可能有其他写插入（可升级读全局唯一），此处检查为标准防御写法
                        if (!data.TryGetValue(key, out value))
                        {
                            value = factory();
                            data[key] = value;
                        }
                        return value;
                    }
                    finally
                    {
                        rwLock.ExitWriteLock();            // 先退写锁
                    }
                }
                finally
                {
                    rwLock.ExitUpgradeableReadLock();      // 再退可升级读锁
                }
            }

            /// <summary>释放内部读写锁。</summary>
            /// <param name="disposing">true 表示显式 Dispose；该类无终结器，仅托管资源。</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        rwLock.Dispose();
                    }
                    disposed = true;
                }
            }

            /// <summary>释放实例占用的资源；释放后不得再使用。</summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>锁状态快照（诊断/监控用，值为采样瞬间的状态，不保证随后仍成立）。</summary>
        public sealed class LockStats
        {
            /// <summary>当前持有读锁的线程数；当前线程持写锁时为 -1。</summary>
            public int CurrentReadCount { get; set; }

            /// <summary>等待进入读锁的线程数。</summary>
            public int WaitingReadCount { get; set; }

            /// <summary>等待进入写锁的线程数。</summary>
            public int WaitingWriteCount { get; set; }

            /// <summary>等待进入可升级读锁的线程数。</summary>
            public int WaitingUpgradeCount { get; set; }

            /// <summary>当前线程是否持有读锁。</summary>
            public bool IsReadLockHeld { get; set; }

            /// <summary>当前线程是否持有写锁。</summary>
            public bool IsWriteLockHeld { get; set; }

            /// <summary>当前线程是否持有可升级读锁。</summary>
            public bool IsUpgradeableReadLockHeld { get; set; }

            /// <summary>当前线程递归进入读锁的次数（NoRecursion 策略下只可能为 0 或 1）。</summary>
            public int RecursiveReadCount { get; set; }
        }

        /// <summary>示例2：读取诊断计数（CurrentReadCount / WaitingReadCount / WaitingWriteCount / WaitingUpgradeCount 及各 IsXxxHeld）。</summary>
        /// <param name="rwLock">读写锁实例。</param>
        /// <returns>采样时刻的锁状态快照。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rwLock"/> 为 null。</exception>
        public static LockStats GetStatistics(ReaderWriterLockSlim rwLock)
        {
            if (rwLock == null)
            {
                throw new ArgumentNullException(nameof(rwLock));
            }

            return new LockStats
            {
                CurrentReadCount = rwLock.CurrentReadCount,
                WaitingReadCount = rwLock.WaitingReadCount,
                WaitingWriteCount = rwLock.WaitingWriteCount,
                WaitingUpgradeCount = rwLock.WaitingUpgradeCount,
                IsReadLockHeld = rwLock.IsReadLockHeld,
                IsWriteLockHeld = rwLock.IsWriteLockHeld,
                IsUpgradeableReadLockHeld = rwLock.IsUpgradeableReadLockHeld,
                RecursiveReadCount = rwLock.RecursiveReadCount
            };
        }

        /// <summary>示例3：带超时的读锁尝试（拿不到返回 false，防止写线程长期饥饿/死等）。</summary>
        /// <param name="rwLock">读写锁实例。</param>
        /// <param name="timeoutMs">等待毫秒数；0 不等待。</param>
        /// <param name="readWork">读临界区工作。</param>
        /// <returns>true 执行成功；false 超时未进入读锁。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rwLock"/> 或 <paramref name="readWork"/> 为 null。</exception>
        public static bool TryReadExample(ReaderWriterLockSlim rwLock, int timeoutMs, Action readWork)
        {
            if (rwLock == null)
            {
                throw new ArgumentNullException(nameof(rwLock));
            }
            if (readWork == null)
            {
                throw new ArgumentNullException(nameof(readWork));
            }

            if (!rwLock.TryEnterReadLock(timeoutMs))
            {
                return false;
            }
            try
            {
                readWork();
            }
            finally
            {
                rwLock.ExitReadLock();
            }
            return true;
        }

        /// <summary>示例4：带超时的写锁尝试（TryEnterWriteLock）。</summary>
        /// <param name="rwLock">读写锁实例。</param>
        /// <param name="timeoutMs">等待毫秒数；0 不等待。</param>
        /// <param name="writeWork">写临界区工作。</param>
        /// <returns>true 执行成功；false 超时未进入写锁。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rwLock"/> 或 <paramref name="writeWork"/> 为 null。</exception>
        public static bool TryWriteExample(ReaderWriterLockSlim rwLock, int timeoutMs, Action writeWork)
        {
            if (rwLock == null)
            {
                throw new ArgumentNullException(nameof(rwLock));
            }
            if (writeWork == null)
            {
                throw new ArgumentNullException(nameof(writeWork));
            }

            if (!rwLock.TryEnterWriteLock(timeoutMs))
            {
                return false;
            }
            try
            {
                writeWork();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
            return true;
        }

        /// <summary>
        /// 示例5：带超时的"可升级读 + 按需升级写"守卫（TryEnterUpgradeableReadLock / TryEnterWriteLock）。
        /// 用法：<c>using (var g = new ConditionalUpdateGuard(rw, 1000)) { if (NeedChange() &amp;&amp; g.UpgradeToWrite(1000)) { ... } }</c>
        /// </summary>
        public sealed class ConditionalUpdateGuard : IDisposable
        {
            private readonly ReaderWriterLockSlim rwLock;
            private bool enteredUpgradeable;
            private bool upgraded;

            /// <summary>尝试在超时内进入可升级读锁。</summary>
            /// <param name="rwLock">读写锁实例。</param>
            /// <param name="timeoutMs">等待毫秒数。</param>
            /// <exception cref="ArgumentNullException"><paramref name="rwLock"/> 为 null。</exception>
            /// <exception cref="TimeoutException">超时未进入可升级读锁。</exception>
            public ConditionalUpdateGuard(ReaderWriterLockSlim rwLock, int timeoutMs)
            {
                if (rwLock == null)
                {
                    throw new ArgumentNullException(nameof(rwLock));
                }

                if (!rwLock.TryEnterUpgradeableReadLock(timeoutMs))
                {
                    throw new TimeoutException(HTranslation.GetContent("在指定超时内未能进入可升级读锁。"));
                }
                enteredUpgradeable = true;
                this.rwLock = rwLock;
            }

            /// <summary>在可升级读锁内部按需升级为写锁（幂等：重复调用直接返回 true）。</summary>
            /// <param name="timeoutMs">等待毫秒数。</param>
            /// <returns>true 已持有写锁；false 超时升级失败（可继续以只读方式使用或放弃）。</returns>
            public bool UpgradeToWrite(int timeoutMs)
            {
                if (upgraded)
                {
                    return true;
                }
                if (!rwLock.TryEnterWriteLock(timeoutMs))
                {
                    return false;
                }
                upgraded = true;
                return true;
            }

            /// <summary>按与进入相反的顺序释放：先写锁（若升级过），再可升级读锁。</summary>
            public void Dispose()
            {
                if (upgraded)
                {
                    rwLock.ExitWriteLock();
                    upgraded = false;
                }
                if (enteredUpgradeable)
                {
                    rwLock.ExitUpgradeableReadLock();
                    enteredUpgradeable = false;
                }
            }
        }

        /// <summary>
        /// 示例6：递归策略演示——<see cref="LockRecursionPolicy.SupportsRecursion"/> 允许同一线程重入读锁，
        /// 重入几次就要 Exit 几次。默认 NoRecursion 下同样代码会抛 <see cref="LockRecursionException"/>，
        /// 非必要不要使用递归策略（性能更差、易死锁）。
        /// </summary>
        /// <returns>重入期间的 RecursiveReadCount（本示例进入两层读锁，返回 2）。</returns>
        public static int RecursionPolicyExample()
        {
            using (ReaderWriterLockSlim recursiveLock =
                new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion))
            {
                recursiveLock.EnterReadLock();
                try
                {
                    recursiveLock.EnterReadLock();        // 仅 SupportsRecursion 策略允许
                    try
                    {
                        return recursiveLock.RecursiveReadCount;
                    }
                    finally
                    {
                        recursiveLock.ExitReadLock();     // 退一层
                    }
                }
                finally
                {
                    recursiveLock.ExitReadLock();         // 再退一层，严格配平
                }
            }
        }
    }
}
