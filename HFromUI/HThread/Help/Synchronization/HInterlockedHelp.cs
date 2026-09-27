using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】Interlocked（互锁原子操作）帮助类：以单条 CPU 指令（如 lock xadd、cmpxchg）完成对单个
    /// 变量的"读-改-写"或 CAS（Compare-And-Swap），全程不可分割，并附带完整内存屏障语义。
    /// 【是否跨进程】否。操作的是进程内的托管内存（共享内存场景另当别论）。
    /// 【典型适用场景】无锁计数器、序列号生成、状态标志翻转、引用的原子发布、无锁数据结构的 CAS 重试循环。
    /// 多变量一致性事务仍需 lock/Monitor；<c>i++</c>/<c>++i</c> 在多线程下不是原子的（读、加、写三步），
    /// 必须用 <see cref="Interlocked.Increment(ref int)"/>。
    /// 【使用步骤】
    /// <list type="number">
    /// <item>计数：Increment/Decrement/Add（int 与 long 均支持，返回操作后的新值）。</item>
    /// <item>覆盖：Exchange（返回旧值）。</item>
    /// <item>条件更新：CompareExchange（仅当前值等于比较值才写入，返回原值），失败则在循环中重读重试。</item>
    /// <item>32 位平台读取 64 位 long：<see cref="Interlocked.Read(ref long)"/> 保证原子读到完整 64 位。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>原子性仅限"单个变量的单次操作"：先 CompareExchange 判断、再修改其他字段的组合不是事务，
    /// 复杂一致性用 lock。</item>
    /// <item>泛型 Exchange&lt;T&gt;/CompareExchange&lt;T&gt; 约束为 <c>where T : class</c>（引用类型），
    /// 不能用于 struct；object 重载用于非泛型场景。</item>
    /// <item>float/double/IntPtr 也有 Exchange/CompareExchange 重载；但没有针对 decimal 的原子算术。</item>
    /// <item>Interlocked 的每个方法都隐式包含全屏障（获取+释放语义），不要在其外再"补"屏障。</item>
    /// </list>
    /// 【版本可用性】核心 Increment/Decrement/Add/Exchange/CompareExchange/Read 自 .NET Framework 1.1/2.0；
    /// <see cref="Interlocked.MemoryBarrier"/> 为 4.5 新增（与 <see cref="Thread.MemoryBarrier"/> 等价）。
    /// 更高版本 API（仅在注释中说明，.NET Framework 4.8 不可用、本类不调用）：
    /// <list type="bullet">
    /// <item><c>Interlocked.MemoryBarrierProcessWide()</c>：.NET 5+ 新增的进程级全屏障
    ///（Windows 上对应 FlushProcessWriteBuffers，强制该进程所有处理器的存储缓冲刷新）。</item>
    /// <item><c>Interlocked.And</c>/<c>Interlocked.Or</c>（int/uint/long/ulong 原子与/或）：.NET 5+ 新增。</item>
    /// </list>
    /// </summary>
    public static class HInterlockedHelp
    {
        /// <summary>示例1：线程安全 int 计数器（自增/自减/加任意增量/原子读）。</summary>
        public class AtomicCounter
        {
            private int value;

            /// <summary>原子 +1。</summary>
            /// <returns>自增后的新值。</returns>
            public int Increment()
            {
                return Interlocked.Increment(ref value);      // 原子 +1，返回新值
            }

            /// <summary>原子 -1。</summary>
            /// <returns>自减后的新值。</returns>
            public int Decrement()
            {
                return Interlocked.Decrement(ref value);     // 原子 -1
            }

            /// <summary>原子加上指定增量（可为负数）。</summary>
            /// <param name="delta">增量。</param>
            /// <returns>相加后的新值。</returns>
            public int Add(int delta)
            {
                return Interlocked.Add(ref value, delta);    // 原子加 delta
            }

            /// <summary>当前值（原子读；32 位 int 普通读本身原子，这里示范统一写法）。</summary>
            /// <returns>当前计数值。</returns>
            public int Value
            {
                get { return Interlocked.CompareExchange(ref value, 0, 0); }
            }
        }

        /// <summary>示例2：64 位原子计数器。32 位平台上普通 long 读/写可能被撕裂成两次 32 位访问，必须走 Interlocked。</summary>
        public class AtomicInt64Counter
        {
            private long value;

            /// <summary>原子 +1。</summary>
            /// <returns>自增后的新值。</returns>
            public long Increment()
            {
                return Interlocked.Increment(ref value);
            }

            /// <summary>原子 -1。</summary>
            /// <returns>自减后的新值。</returns>
            public long Decrement()
            {
                return Interlocked.Decrement(ref value);
            }

            /// <summary>原子加上指定增量。</summary>
            /// <param name="delta">增量。</param>
            /// <returns>相加后的新值。</returns>
            public long Add(long delta)
            {
                return Interlocked.Add(ref value, delta);
            }

            /// <summary>原子读取 64 位值（<see cref="Interlocked.Read(ref long)"/>）。</summary>
            /// <returns>完整、不被撕裂的 64 位值。</returns>
            public long Value
            {
                get { return Interlocked.Read(ref value); }
            }
        }

        /// <summary>示例3：int 原子交换：无条件设置为新值并返回旧值（状态翻转常用）。</summary>
        /// <param name="location">目标变量。</param>
        /// <param name="newValue">要写入的新值。</param>
        /// <returns>交换前的旧值。</returns>
        public static int Exchange(ref int location, int newValue)
        {
            return Interlocked.Exchange(ref location, newValue);
        }

        /// <summary>示例4：long 原子交换。</summary>
        /// <param name="location">目标变量。</param>
        /// <param name="newValue">要写入的新值。</param>
        /// <returns>交换前的旧值。</returns>
        public static long ExchangeInt64(ref long location, long newValue)
        {
            return Interlocked.Exchange(ref location, newValue);
        }

        /// <summary>示例5：double 原子交换（浮点也有 Exchange 重载，用于原子发布浮点状态）。</summary>
        /// <param name="location">目标变量。</param>
        /// <param name="newValue">要写入的新值。</param>
        /// <returns>交换前的旧值。</returns>
        public static double ExchangeDouble(ref double location, double newValue)
        {
            return Interlocked.Exchange(ref location, newValue);
        }

        /// <summary>示例6：引用（object）原子交换：发布/替换对象并取回旧引用。</summary>
        /// <param name="location">目标引用。</param>
        /// <param name="newValue">要发布的新对象。</param>
        /// <returns>交换前的旧引用（可能为 null）。</returns>
        public static object ExchangeObject(ref object location, object newValue)
        {
            return Interlocked.Exchange(ref location, newValue);
        }

        /// <summary>
        /// 示例7：泛型原子交换（T 必须是引用类型）。比 object 重载少一次类型转换，适合强类型状态机。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="location">目标引用。</param>
        /// <param name="newValue">要发布的新对象。</param>
        /// <returns>交换前的旧引用（可能为 null）。</returns>
        public static T ExchangeGeneric<T>(ref T location, T newValue) where T : class
        {
            return Interlocked.Exchange(ref location, newValue);
        }

        /// <summary>示例8：CAS 比较交换（int）：仅当当前值==comparand 时才替换为 newValue，返回原值。</summary>
        /// <param name="location">目标变量。</param>
        /// <param name="newValue">条件满足时写入的新值。</param>
        /// <param name="comparand">比较值（期望值）。</param>
        /// <returns>true 表示比较成功并已写入；false 表示当前值不等于比较值，未写入。</returns>
        public static bool CompareExchangeOnce(ref int location, int newValue, int comparand)
        {
            return Interlocked.CompareExchange(ref location, newValue, comparand) == comparand;
        }

        /// <summary>示例9：只执行一次的初始化守卫（0=未执行，1=已执行）。</summary>
        /// <param name="state">状态变量，初始应为 0。</param>
        /// <returns>true 表示本次调用赢得 CAS、负责执行初始化；false 表示已被其他线程抢先。</returns>
        public static bool TryStartOnce(ref int state)
        {
            return Interlocked.CompareExchange(ref state, 1, 0) == 0;
        }

        /// <summary>示例10：long 比较交换（无锁更新 64 位状态）。</summary>
        /// <param name="location">目标变量。</param>
        /// <param name="newValue">条件满足时写入的新值。</param>
        /// <param name="comparand">比较值。</param>
        /// <returns>交换前的原值；等于 comparand 即表示更新成功。</returns>
        public static long CompareExchangeInt64(ref long location, long newValue, long comparand)
        {
            return Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        /// <summary>示例11：double 比较交换（无锁更新浮点量，如统计指标）。</summary>
        /// <param name="location">目标变量。</param>
        /// <param name="newValue">条件满足时写入的新值。</param>
        /// <param name="comparand">比较值。</param>
        /// <returns>交换前的原值。</returns>
        public static double CompareExchangeDouble(ref double location, double newValue, double comparand)
        {
            return Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        /// <summary>示例12：object 比较交换：仅当引用仍是 comparand 时才发布新对象。</summary>
        /// <param name="location">目标引用。</param>
        /// <param name="newValue">要发布的新对象。</param>
        /// <param name="comparand">期望的当前引用（常为 null 表示"尚未初始化"）。</param>
        /// <returns>交换前的旧引用；等于 comparand 表示发布成功。</returns>
        public static object CompareExchangeObject(ref object location, object newValue, object comparand)
        {
            return Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        /// <summary>
        /// 示例13：泛型 CAS（T 为引用类型）——单例/不可变状态原子发布的标准原语。
        /// 调用方通常以 while 重试：读取当前值 → 计算新值 → CAS，失败则重读。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="location">目标引用。</param>
        /// <param name="newValue">条件满足时发布的新对象。</param>
        /// <param name="comparand">期望的当前引用。</param>
        /// <returns>交换前的旧引用；等于 comparand 表示发布成功。</returns>
        public static T CompareExchangeGeneric<T>(ref T location, T newValue, T comparand) where T : class
        {
            return Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        /// <summary>示例14：<see cref="Interlocked.Read(ref long)"/> 原子读取 64 位值（32 位平台防撕裂）。</summary>
        /// <param name="location">64 位变量。</param>
        /// <returns>完整的 64 位当前值。</returns>
        public static long ReadInt64(ref long location)
        {
            return Interlocked.Read(ref location);
        }

        /// <summary>
        /// 示例15：<see cref="Interlocked.MemoryBarrier"/>（.NET Framework 4.5+）显式全屏障，
        /// 与 <see cref="Thread.MemoryBarrier"/> 等价。一般无需直接调用——所有 Interlocked 方法自带屏障。
        /// </summary>
        public static void MemoryBarrierExample()
        {
            Interlocked.MemoryBarrier();              // 屏障前的读写不会与屏障后的读写互相重排
        }
    }
}
