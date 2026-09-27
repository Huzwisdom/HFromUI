using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】SpinWait（智能自旋等待）帮助类：<see cref="SpinWait"/> 是值类型，提供"先自旋、后让出"
    /// 的两级等待策略。前若干次 <see cref="SpinWait.SpinOnce"/> 执行 CPU 自旋（SpinWait 内部封装
    /// Thread.SpinWait/Yield），超过阈值后阶梯退让为 <see cref="Thread.Yield"/>、
    /// <c>Thread.Sleep(0)</c>、<c>Thread.Sleep(1)</c>，避免长时间独占核心。<see cref="SpinLock"/>
    /// 内部正是用它实现。
    /// 【是否跨进程】否。纯进程内、线程本地的等待结构。
    /// 【典型适用场景】等待一个预计在数十/数百条指令内就会成立的无锁条件（标志翻转、引用发布、
    /// 无锁队列非空）；实现两阶段等待（先自旋再内核等待）。
    /// 【使用步骤】
    /// <list type="number">
    /// <item><c>SpinWait spinner = new SpinWait();</c></item>
    /// <item><c>while (!condition()) spinner.SpinOnce();</c>，或直接用静态 <c>SpinWait.SpinUntil(...)</c>。</item>
    /// <item>需要上限时用带超时的 SpinUntil 重载（返回 false 表示超时），或自管截止时间。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>只适合"条件极快成立"。条件迟迟不来时自旋阶段仍会烧 CPU，必须配合超时或在上层退化为内核等待。</item>
    /// <item>被等待的条件必须用 <see cref="Volatile"/> 读或 <see cref="Interlocked"/> 发布，
    /// 否则 JIT/CPU 可能把读取缓存到寄存器，自旋永远看不到新值。</item>
    /// <item>SpinWait 实例是可变 struct，不要存进 readonly 字段、不要跨线程共享（设计为等待线程本地使用）。</item>
    /// <item>单线程内重入等待同一资源没有意义；不要在持有任意锁时长时间 SpinOnce。</item>
    /// </list>
    /// 【版本可用性】SpinWait 自 .NET Framework 4.0，4.0 即有 SpinOnce()、静态 SpinUntil 三个重载、
    /// Count、NextSpinWillYield、Reset。注意：实例方法 <c>SpinOnce(int sleep1Threshold)</c>
    /// 仅 .NET Core 3.0/.NET Standard 2.1 及更高版本提供，.NET Framework 4.8 【没有】该重载，
    /// 本帮助类不以任何代码调用它。
    /// </summary>
    public static class HSpinWaitHelp
    {
        /// <summary>示例1：自旋直到条件成立（无锁标志位翻转的标准写法）。</summary>
        /// <param name="condition">条件判定；应通过 Volatile.Read/Interlocked 读取共享状态。</param>
        /// <exception cref="ArgumentNullException"><paramref name="condition"/> 为 null。</exception>
        public static void SpinUntil(Func<bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            SpinWait spin = new SpinWait();
            while (!condition())
            {
                spin.SpinOnce();                    // 内部自动：多次自旋 → Yield → Sleep(1) 阶梯退让
            }
        }

        /// <summary>示例2：手写截止时间的自旋等待（避免无限烧 CPU）。</summary>
        /// <param name="condition">条件判定。</param>
        /// <param name="timeoutMs">超时毫秒数。</param>
        /// <returns>true 条件在超时内成立；false 超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="condition"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为负数。</exception>
        public static bool SpinUntilTimeout(Func<bool> condition, int timeoutMs)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            if (timeoutMs < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMs));
            }

            SpinWait spin = new SpinWait();
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (!condition())
            {
                if (DateTime.UtcNow >= deadline)
                {
                    return false;
                }
                spin.SpinOnce();
            }
            return true;
        }

        /// <summary>示例3：手写自旋锁骨架（展示 <see cref="SpinWait.NextSpinWillYield"/> 的用法）。</summary>
        public sealed class SimpleSpinLock
        {
            private int locked;                     // 0=空闲 1=占用，配合 Interlocked 做 CAS

            /// <summary>获取锁；竞争时自旋 + 阶梯退让，直到 CAS 成功。</summary>
            public void Enter()
            {
                SpinWait spin = new SpinWait();
                while (Interlocked.CompareExchange(ref locked, 1, 0) != 0)
                {
                    if (spin.NextSpinWillYield)
                    {
                        // 已自旋较久，下一轮将让出 CPU；此处可埋点统计锁竞争或切换到内核等待
                    }
                    spin.SpinOnce();
                }
            }

            /// <summary>释放锁（原子写 0，带释放语义）。</summary>
            public void Exit()
            {
                Interlocked.Exchange(ref locked, 0);
            }
        }

        /// <summary>示例4：直接使用框架内置静态 <see cref="SpinWait.SpinUntil(Func{bool})"/>（无超时版）。</summary>
        /// <param name="condition">条件判定。</param>
        /// <exception cref="ArgumentNullException"><paramref name="condition"/> 为 null。</exception>
        public static void SpinUntilBuiltIn(Func<bool> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            SpinWait.SpinUntil(condition);
        }

        /// <summary>示例5：内置 <see cref="SpinWait.SpinUntil(Func{bool},int)"/> 毫秒超时重载。</summary>
        /// <param name="condition">条件判定。</param>
        /// <param name="timeoutMs">超时毫秒数；0 只判断一次，-1 等价无超时（不建议）。</param>
        /// <returns>true 条件成立；false 超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="condition"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 非法。</exception>
        public static bool SpinUntilBuiltInTimeout(Func<bool> condition, int timeoutMs)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            return SpinWait.SpinUntil(condition, timeoutMs);
        }

        /// <summary>示例6：内置 <see cref="SpinWait.SpinUntil(Func{bool},TimeSpan)"/> 超时重载。</summary>
        /// <param name="condition">条件判定。</param>
        /// <param name="timeout">超时时长。</param>
        /// <returns>true 条件成立；false 超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="condition"/> 为 null。</exception>
        public static bool SpinUntilTimeSpan(Func<bool> condition, TimeSpan timeout)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            return SpinWait.SpinUntil(condition, timeout);
        }

        /// <summary>
        /// 示例7：<see cref="SpinWait.Count"/> 与 <see cref="SpinWait.Reset"/> —— Count 记录已自旋次数，
        /// Reset 把内部计数清零（同一实例用于新一轮等待时，让退让节奏从头开始）。
        /// 条件立即满足，方法可安全自测、不空转。
        /// </summary>
        /// <returns>重置前的 SpinOnce 调用次数。</returns>
        public static int CountAndResetExample()
        {
            SpinWait spin = new SpinWait();
            spin.SpinOnce();                        // 计数 1
            spin.SpinOnce();                        // 计数 2
            int count = spin.Count;                 // 读取已自旋次数（诊断/自适应策略用）
            spin.Reset();                           // 清零：下次等待重新从纯自旋开始
            return count;
        }
    }
}
