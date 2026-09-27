using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadCreate
{
    using HFromUI.HLangage;
    /// <summary>
    /// Lazy 与 LazyInitializer 帮助类：延迟初始化（首次用到时才创建昂贵对象）。
    /// 【是什么】System.Lazy&lt;T&gt; 把“工厂委托 + 值”包装成一个对象，首次读 Value 时才执行工厂；
    /// System.Threading.LazyInitializer 是静态类，用 EnsureInitialized 对普通字段做懒初始化，
    /// 不需要 Lazy&lt;T&gt; 包装对象，少一次堆分配。三种线程安全模式由 LazyThreadSafetyMode 指定：
    /// None（不加任何锁，单线程场景最快）、ExecutionAndPublication（默认：工厂最多执行一次，
    /// 异常会被缓存并对之后每个访问者原样重抛）、PublicationOnly（多线程下可能各跑一次工厂，
    /// 只有一个结果成功发布，工厂抛异常不会被缓存，后续访问会再次尝试）。
    /// 【是否跨进程】否。纯进程内的字段/对象初始化行为。
    /// 【典型适用场景】
    /// 1) 构造成本高但不一定用得到的对象（配置、大缓存、外部连接描述）：用 Lazy&lt;T&gt; 延迟到首次访问；
    /// 2) 多线程共享单例：默认 ExecutionAndPublication 保证工厂只跑一次；
    /// 3) 初始化可能临时失败、希望下次访问自动重试：PublicationOnly；
    /// 4) 对每个实例都省一个包装对象的高性能字段模式：LazyInitializer.EnsureInitialized(ref 字段...)。
    /// 【使用步骤】
    /// 1) new Lazy&lt;T&gt;(工厂委托) 或显式传入 LazyThreadSafetyMode；
    /// 2) 首次读 Value 触发初始化；IsValueCreated 可在不触发初始化的前提下查询是否已创建；
    /// 3) 字段模式：声明 T 字段 + bool 已初始化标记 + object 锁对象三个字段，
    ///    调 LazyInitializer.EnsureInitialized(ref T, ref bool, ref object, Func&lt;T&gt;)；
    /// 4) 需要重试语义时选 PublicationOnly；确定单线程访问时选 None 省锁。
    /// 【注意事项与坑】
    /// - ExecutionAndPublication 下工厂异常会被缓存：之后每次读 Value 都重抛同一个异常，
    ///   “失败一次、永远失败”，需要重试请改用 PublicationOnly；
    /// - PublicationOnly 下工厂可能被多个线程并发执行多次，工厂本身必须无副作用或副作用幂等；
    /// - None 模式在多线程下首次并发访问可能创建多个对象，绝不能用于共享字段；
    /// - LazyInitializer 等价于手写双检锁/Interlocked.CompareExchange 的标准实现，
    ///   锁对象字段为 null 时由它内部按需初始化，不要自己再套一层锁；
    /// - 静态单例在现代 .NET 中也可由 CLR 类型加载保证，但实例字段的延迟初始化仍需上述机制。
    /// 【版本可用性】Lazy&lt;T&gt; 与 LazyInitializer 均为 .NET Framework 4.0 起；net48 全量可用。
    /// </summary>
    /// <example>
    /// 字段模式懒初始化（无 Lazy 包装对象）：
    /// <code>
    /// private Expensive _expensive;                 // 真实值字段
    /// private bool _initialized;                    // 是否已初始化标记
    /// private object _gate;                         // 锁对象（可为 null，内部按需创建）
    /// public Expensive ExpensiveInstance
    /// {
    ///     get
    ///     {
    ///         return LazyInitializer.EnsureInitialized(ref _expensive, ref _initialized,
    ///             ref _gate, delegate { return new Expensive(); });
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class HLazyInitHelp
    {
        /// <summary>
        /// 示例1（真实行为验证）：ExecutionAndPublication 模式下工厂只执行一次。
        /// 8 个并行任务同时读 Value，工厂内用 Interlocked 计数，最终计数必须为 1，值为 42。
        /// </summary>
        /// <returns>FactoryCounter：Value=42，FactoryCalls=1。</returns>
        public static FactoryCounter FactoryRunsExactlyOnce()
        {
            int calls = 0;
            Lazy<int> lazy = new Lazy<int>(delegate
            {
                Interlocked.Increment(ref calls);     // 记录工厂真实执行次数
                return 42;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            Parallel.For(0, 8, delegate (int i)
            {
                int value = lazy.Value;               // 多线程同时首次访问
                if (value != 42)
                {
                    throw new InvalidOperationException(HTranslation.GetContent("懒初始化返回了非预期值 ") + value);
                }
            });

            // 默认模式：工厂只允许完整执行并发布一次
            return new FactoryCounter { Value = lazy.Value, FactoryCalls = calls };
        }

        /// <summary>
        /// 示例2（真实行为验证）：ExecutionAndPublication 会缓存工厂异常。
        /// 工厂总是抛 InvalidOperationException，连续两次读 Value 都应收到异常，且工厂只执行 1 次。
        /// </summary>
        /// <returns>两次访问都收到 InvalidOperationException 且工厂仅执行 1 次时返回 true。</returns>
        public static bool ExecutionAndPublicationCachesException()
        {
            int calls = 0;
            Lazy<int> lazy = new Lazy<int>(delegate
            {
                Interlocked.Increment(ref calls);
                throw new InvalidOperationException(HTranslation.GetContent("工厂总是失败"));
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            bool firstThrew = false;
            bool secondThrew = false;
            try
            {
                int first = lazy.Value;               // 首次访问：执行工厂，异常被缓存
            }
            catch (InvalidOperationException)
            {
                firstThrew = true;
            }
            try
            {
                int second = lazy.Value;              // 再次访问：不重跑工厂，直接重抛缓存异常
            }
            catch (InvalidOperationException)
            {
                secondThrew = true;
            }
            return firstThrew && secondThrew && calls == 1;
        }

        /// <summary>
        /// 示例3（真实行为验证）：PublicationOnly 模式失败后会重试。
        /// 工厂第 1、2 次执行时抛异常（不缓存），第 3 次成功并发布；最多尝试三次访问。
        /// </summary>
        /// <returns>第三次访问得到值 3，且工厂恰好执行 3 次时返回 true。</returns>
        public static bool PublicationOnlyRetriesUntilSuccess()
        {
            int attempts = 0;
            Lazy<int> lazy = new Lazy<int>(delegate
            {
                int n = Interlocked.Increment(ref attempts);
                if (n < 3)
                {
                    // 前两次失败：PublicationOnly 不缓存异常，下次访问会重新执行工厂
                    throw new InvalidOperationException(HTranslation.GetContent("第 ") + n + HTranslation.GetContent(" 次初始化失败"));
                }
                return n;                             // 第三次成功，值 3 被发布
            }, LazyThreadSafetyMode.PublicationOnly);

            for (int visit = 0; visit < 3; visit++)
            {
                try
                {
                    int value = lazy.Value;
                    return value == 3 && attempts == 3;
                }
                catch (InvalidOperationException)
                {
                    // 本次工厂失败，继续下一次访问触发重试
                }
            }
            return false;
        }

        /// <summary>
        /// 示例4（真实行为验证）：LazyInitializer.EnsureInitialized 字段模式。
        /// 三个普通字段（值/已初始化标记/锁对象）连续调用两次，工厂只执行一次，两次都得到 99。
        /// </summary>
        /// <returns>两次调用都返回 99、工厂只执行 1 次且初始化标记为 true 时返回 true。</returns>
        public static bool EnsureInitializedFieldPattern()
        {
            int expensive = 0;                        // 真实值字段
            bool initialized = false;                 // 已初始化标记
            object gate = null;                       // 锁对象，null 时由 EnsureInitialized 内部创建
            int factoryCalls = 0;

            // 第一次：执行工厂并发布
            int first = LazyInitializer.EnsureInitialized(ref expensive, ref initialized, ref gate,
                delegate { factoryCalls++; return 99; });
            // 第二次：直接返回已发布的值，不再执行工厂
            int second = LazyInitializer.EnsureInitialized(ref expensive, ref initialized, ref gate,
                delegate { factoryCalls++; return 99; });

            return first == 99 && second == 99 && factoryCalls == 1 && initialized;
        }

        /// <summary>
        /// 示例5：IsValueCreated 查询语义。None 模式下创建 Lazy 后、读 Value 前 IsValueCreated 为 false，
        /// 读取后变为 true；读取该属性本身不会触发初始化。
        /// </summary>
        /// <returns>读取前 false、读取后 true 且值正确时返回 true。</returns>
        public static bool IsValueCreatedDemo()
        {
            Lazy<string> lazy = new Lazy<string>(delegate
            {
                return "created";
            }, LazyThreadSafetyMode.None);            // 单线程最快模式

            if (lazy.IsValueCreated)
            {
                return false;                         // 尚未读 Value 不应已创建
            }
            string value = lazy.Value;
            return lazy.IsValueCreated && value == "created";
        }

        /// <summary>
        /// 工厂调用计数结果载体：Value 为懒初始化发布的值，FactoryCalls 为工厂实际执行次数。
        /// </summary>
        public sealed class FactoryCounter
        {
            /// <summary>懒初始化最终发布的值。</summary>
            public int Value { get; set; }

            /// <summary>工厂委托实际被执行的次数。</summary>
            public int FactoryCalls { get; set; }
        }
    }
}
