using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】SpinLock（自旋锁）帮助类：<see cref="SpinLock"/> 是一个值类型（struct）的低级互斥锁。
    /// 竞争时先让 CPU 自旋空转若干轮，期望持锁者很快释放，从而避免内核态等待句柄的上下文切换开销。
    /// 【是否跨进程】否。纯进程内、纯 CPU 指令级的锁。
    /// 【典型适用场景】临界区极短（几条赋值/自增指令）、竞争线程数接近核心数、且持锁期间绝不阻塞
    /// （无 I/O、无锁嵌套、不分配大对象）的热点路径；<see cref="System.Threading.SpinLock"/> 内部
    /// 借助 <see cref="SpinWait"/> 实现。普通业务代码优先用 <c>lock</c>。
    /// 【使用步骤】
    /// <list type="number">
    /// <item>声明字段 <c>private SpinLock spinLock = new SpinLock();</c>（默认开启线程归属跟踪）。</item>
    /// <item><c>bool lockTaken = false;</c> 后 <c>Enter(ref lockTaken)</c> 或三个 TryEnter 重载之一。</item>
    /// <item>finally 中 <c>if (lockTaken) spinLock.Exit();</c>。</item>
    /// </list>
    /// 【注意事项与坑（值类型拷贝是最大的坑）】
    /// <list type="bullet">
    /// <item>SpinLock 是可变 struct：作为字段时【绝对不能加 readonly】——对 readonly 字段调用方法会作用于
    /// 防御性拷贝，每个线程锁的都是自己的副本，锁完全失效。也不要按值传递/装箱，一律 ref 传或用字段。</item>
    /// <item>临界区里绝不能阻塞：I/O、等待其他锁、Sleep 都会让其他核空转烧 CPU。</item>
    /// <item>开启归属跟踪时，同线程重入抛 <see cref="LockRecursionException"/>。</item>
    /// <item><see cref="SpinLock.IsHeldByCurrentThread"/> 仅在开启线程归属跟踪时可用，否则访问抛
    /// <see cref="InvalidOperationException"/>；<see cref="SpinLock.IsHeld"/> 则任何模式都可读。</item>
    /// <item>Exit 后 lockTaken 要复位为 false（在同一 try/finally 作用域内自然满足）。</item>
    /// </list>
    /// 【版本可用性】SpinLock 自 .NET Framework 4.0；本类全部成员 4.8 可用。
    /// </summary>
    public static class HSpinLockHelp
    {
        /// <summary>示例1：自旋锁保护极短临界区（自增计数器）。嵌套类演示字段绝不能加 readonly。</summary>
        public class SpinCounter
        {
            // 注意：SpinLock 是可变结构体，字段不能加 readonly，否则方法调用作用于防御性拷贝、锁会失效
            private SpinLock spinLock = new SpinLock();
            private int count;

            /// <summary>读取当前计数（读也加锁，保证读到一致值）。</summary>
            /// <returns>当前计数值。</summary>
            public int Count
            {
                get
                {
                    bool lockTaken = false;
                    try
                    {
                        spinLock.Enter(ref lockTaken);
                        return count;
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            spinLock.Exit();
                        }
                    }
                }
            }

            /// <summary>原子地自增一次（极短操作，适合自旋）。</summary>
            public void Increment()
            {
                bool lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);
                    count++;                            // 极短操作：适合自旋
                }
                finally
                {
                    if (lockTaken)
                    {
                        spinLock.Exit();
                    }
                }
            }
        }

        /// <summary>
        /// 示例2：毫秒超时尝试加锁（<c>TryEnter(int, ref bool)</c> 返回 void，超时未拿到时 lockTaken 保持 false）。
        /// </summary>
        /// <param name="spinLock">自旋锁（按 ref 传递，禁止拷贝）。</param>
        /// <param name="timeoutMs">等待毫秒数；0 不等待，-1 无限等待（不建议）。</param>
        /// <param name="veryShortWork">极短的临界区工作。</param>
        /// <returns>true 执行成功；false 超时放弃。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="veryShortWork"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 非法。</exception>
        public static bool TryEnterExample(ref SpinLock spinLock, int timeoutMs, Action veryShortWork)
        {
            if (veryShortWork == null)
            {
                throw new ArgumentNullException(nameof(veryShortWork));
            }

            bool lockTaken = false;
            try
            {
                spinLock.TryEnter(timeoutMs, ref lockTaken);   // void 返回：结果看 lockTaken
                if (!lockTaken)
                {
                    return false;
                }
                veryShortWork();
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
            return true;
        }

        /// <summary>示例3：立即尝试一次（<c>TryEnter(ref bool)</c>，等价超时 0），不自旋等待。</summary>
        /// <param name="spinLock">自旋锁（按 ref 传递）。</param>
        /// <param name="veryShortWork">极短的临界区工作。</param>
        /// <returns>true 执行成功；false 锁正被占用。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="veryShortWork"/> 为 null。</exception>
        public static bool TryEnterImmediateExample(ref SpinLock spinLock, Action veryShortWork)
        {
            if (veryShortWork == null)
            {
                throw new ArgumentNullException(nameof(veryShortWork));
            }

            bool lockTaken = false;
            try
            {
                spinLock.TryEnter(ref lockTaken);             // 立即返回，不等待
                if (!lockTaken)
                {
                    return false;
                }
                veryShortWork();
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
            return true;
        }

        /// <summary>示例4：<see cref="TimeSpan"/> 超时重载。</summary>
        /// <param name="spinLock">自旋锁（按 ref 传递）。</param>
        /// <param name="timeout">超时时长。</param>
        /// <param name="veryShortWork">极短的临界区工作。</param>
        /// <returns>true 执行成功；false 超时放弃。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="veryShortWork"/> 为 null。</exception>
        public static bool TryEnterTimeSpanExample(ref SpinLock spinLock, TimeSpan timeout, Action veryShortWork)
        {
            if (veryShortWork == null)
            {
                throw new ArgumentNullException(nameof(veryShortWork));
            }

            bool lockTaken = false;
            try
            {
                spinLock.TryEnter(timeout, ref lockTaken);
                if (!lockTaken)
                {
                    return false;
                }
                veryShortWork();
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit();
                }
            }
            return true;
        }

        /// <summary>
        /// 示例5：<c>Exit(bool useMemoryBarrier)</c>——传 false 表示释放时不生成内存屏障（少一次栅栏开销，
        /// 仅当后续操作自带屏障时使用）；默认无参 Exit() 等价 Exit(true)，绝大多数情况用默认即可。
        /// </summary>
        /// <param name="spinLock">自旋锁（按 ref 传递）。</param>
        /// <param name="veryShortWork">极短的临界区工作。</param>
        /// <exception cref="ArgumentNullException"><paramref name="veryShortWork"/> 为 null。</exception>
        public static void ExitWithMemoryBarrierChoiceExample(ref SpinLock spinLock, Action veryShortWork)
        {
            if (veryShortWork == null)
            {
                throw new ArgumentNullException(nameof(veryShortWork));
            }

            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);
                veryShortWork();
            }
            finally
            {
                if (lockTaken)
                {
                    spinLock.Exit(true);              // true：释放带全栅栏，保证临界区内写入对后续获取者可见
                }
            }
        }

        /// <summary>SpinLock 状态快照。</summary>
        public sealed class SpinLockStatus
        {
            /// <summary>锁是否被任意线程持有（任何跟踪模式下都可读）。</summary>
            public bool IsHeld { get; set; }

            /// <summary>锁是否被当前线程持有（仅开启线程归属跟踪时有意义）。</summary>
            public bool IsHeldByCurrentThread { get; set; }

            /// <summary>是否启用了线程归属跟踪。</summary>
            public bool IsThreadOwnerTrackingEnabled { get; set; }
        }

        /// <summary>
        /// 示例6：读取 IsHeld / IsHeldByCurrentThread / IsThreadOwnerTrackingEnabled。
        /// 归属跟踪关闭时 IsHeldByCurrentThread 会抛异常，此处安全降级为 false（仅用于诊断）。
        /// </summary>
        /// <param name="spinLock">自旋锁（按 ref 传递）。</param>
        /// <returns>采样时刻的锁状态快照。</returns>
        public static SpinLockStatus GetStatus(ref SpinLock spinLock)
        {
            SpinLockStatus status = new SpinLockStatus
            {
                IsHeld = spinLock.IsHeld,
                IsThreadOwnerTrackingEnabled = spinLock.IsThreadOwnerTrackingEnabled
            };

            if (spinLock.IsThreadOwnerTrackingEnabled)
            {
                status.IsHeldByCurrentThread = spinLock.IsHeldByCurrentThread;
            }
            return status;
        }

        /// <summary>示例7：显式构造关闭线程归属跟踪的自旋锁（少一些记录开销，代价是不能查询持有者、重入检测失效）。</summary>
        /// <returns>关闭归属跟踪的 SpinLock（值类型，调用方应存入非 readonly 字段）。</returns>
        public static SpinLock CreateWithoutOwnerTracking()
        {
            // new SpinLock() 默认 enableThreadOwnerTracking=true；传 false 关闭跟踪换取性能
            return new SpinLock(false);
        }
    }
}
