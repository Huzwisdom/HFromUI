using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】[MethodImpl(MethodImplOptions.Synchronized)] 特性式同步帮助类：
    /// 把 <see cref="MethodImplOptions.Synchronized"/> 标记在方法上，CLR 会把整个方法体包进
    /// “lock + try/finally 释放”的形式，是比 C# lock 语句更早（也更粗粒度）的声明式同步写法。
    /// 锁定对象由方法种类决定：实例方法锁【当前实例 this】；静态方法锁【该类型的 RuntimeTypeHandle
    /// 对应的类型对象】（即等价于 lock(typeof(T))）。
    ///
    /// 【是否跨进程】否。本质仍是进程内 Monitor 锁（对象头同步块），与 lock 语句同一机制，不能跨进程。
    ///
    /// 【典型适用场景】【仅维护老代码】阅读/修改早期遗留代码中带该特性的方法时查阅。
    /// 新代码不要用它做同步：实例方法锁 this、静态方法锁类型对象，锁对象对外可见，无法阻止外部代码
    /// 锁同一对象，容易被外部代码“误占同一把锁”造成死锁，且方法内哪段代码在锁里也不直观。
    ///
    /// 【使用步骤】（老代码中常见形态）
    /// 1) using System.Runtime.CompilerServices;
    /// 2) 实例方法：[MethodImpl(MethodImplOptions.Synchronized)] 标在 public 实例方法上，等价锁 this；
    /// 3) 静态方法：同样标记，等价锁 typeof(当前类)；
    /// 4) 维护时应优先重构为 private readonly object syncRoot + lock(syncRoot)（见 <see cref="HLockHelp"/>）。
    ///
    /// 【注意事项与坑】
    /// 1) 实例方法锁 this、静态方法锁类型对象——外部任何拿到该实例/Type 的代码都能锁同一把锁，
    ///    死锁风险不可控；这是不推荐使用的根本原因；
    /// 2) 整个方法体都在锁内：方法中哪怕只有一行需要保护，其余耗时代码（I/O、虚方法调用、事件回调）
    ///    也会一直持锁，锁护送严重；
    /// 3) 与 lock 语句一样：锁可重入、异常时 finally 会释放、方法内不能 await（且该方法也不能轻易改 async）；
    /// 4) 该特性只是 MethodImplOptions 的一个位标志，可用反射
    ///    MethodBase.GetMethodImplementationFlags() 读取到（见本类 <see cref="GetMethodIsSynchronized"/>）；
    /// 5) 不要把它理解成“方法级原子性/事务”，它只保证同一锁对象上的线程互斥。
    ///
    /// 【MethodImplOptions 其他成员】（注释列出，便于顺带查阅）
    /// NoInlining：禁止 JIT 内联该方法（常用于做方法级拦截/堆栈边界）；
    /// NoOptimization：禁止 JIT 优化（调试/ Profiler 场景）；
    /// PreserveSig：保留非托管 HRESULT 签名，不做 HRESULT→异常转换（P/Invoke 互操作）；
    /// InternalCall：内部调用，方法由 CLR 内部实现；
    /// ForwardRef：方法已前向声明、实现在别处（少见）；
    /// Managed/ILCode/NativeCode：方法实现形态标志；
    /// AggressiveInlining：强烈建议内联，.NET Framework 4.5 起可用（net48 可用）；
    /// AggressiveOptimization：禁用部分提升调试体验的优化以换取峰值性能，仅 .NET Core 3.0/.NET 5+，
    /// net48【没有】该成员。
    ///
    /// 【版本可用性】<see cref="MethodImplOptions.Synchronized"/> 自 .NET Framework 1.1 起提供，
    /// net48 内置（mscorlib）；.NET Core/.NET 5+ 保留，但官方文档长期标注“不推荐用于新代码”。
    /// </summary>
    /// <example>
    /// 实例方法锁 this、静态方法锁类型对象的写法，以及推荐的重构方向。
    /// <code>
    /// using System.Runtime.CompilerServices;
    ///
    /// public sealed class LegacyCounter
    /// {
    ///     private int count;
    ///
    ///     // 等价于整个方法体 lock (this)
    ///     [MethodImpl(MethodImplOptions.Synchronized)]
    ///     public int Increment()
    ///     {
    ///         return ++count;
    ///     }
    ///
    ///     private static int globalCount;
    ///
    ///     // 静态方法等价于整个方法体 lock (typeof(LegacyCounter))
    ///     [MethodImpl(MethodImplOptions.Synchronized)]
    ///     public static int IncrementGlobal()
    ///     {
    ///         return ++globalCount;
    ///     }
    ///
    ///     // 推荐的新写法：专用私有锁
    ///     private readonly object syncRoot = new object();
    ///
    ///     public int SafeIncrement()
    ///     {
    ///         lock (syncRoot)
    ///         {
    ///             return ++count;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class HMethodImplSynchronizedHelp
    {
        /// <summary>实例计数器：<see cref="IncrementSynchronized"/> 持锁保护的共享状态。</summary>
        private int synchronizedCount;

        /// <summary>静态计数器：<see cref="StaticSynchronizedDemo"/> 持锁保护的共享状态。</summary>
        private static int staticSynchronizedCount;

        /// <summary>
        /// 示例1：带 <see cref="MethodImplOptions.Synchronized"/> 的实例方法。
        /// CLR 在进入方法时锁定当前实例 this，方法返回（含异常路径）时释放，等价于整个方法体 lock(this)。
        /// </summary>
        /// <returns>自增后的实例计数器值（同一实例上从 1 开始单调递增）。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int IncrementSynchronized()
        {
            // 整个方法体隐式持有 this 上的 Monitor 锁
            return ++synchronizedCount;
        }

        /// <summary>
        /// 示例2：带 <see cref="MethodImplOptions.Synchronized"/> 的静态方法。
        /// 静态方法锁的是当前类型对象（等价 lock(typeof(HMethodImplSynchronizedHelp))），
        /// 全进程同一类型的所有调用方共用这一把锁。
        /// </summary>
        /// <returns>自增后的静态计数器值（从 1 开始单调递增）。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static int StaticSynchronizedDemo()
        {
            // 整个方法体隐式持有类型对象上的 Monitor 锁
            return ++staticSynchronizedCount;
        }

        /// <summary>
        /// 示例3：用反射验证方法是否带 Synchronized 实现标志。读取本类
        /// <see cref="IncrementSynchronized"/> 的 <see cref="MethodBase.GetMethodImplementationFlags"/>，
        /// 按位与 <see cref="MethodImplAttributes.Synchronized"/> 判断该位是否置位。
        /// </summary>
        /// <returns>目标方法的实现标志中包含 Synchronized 位时返回 true；否则返回 false。</returns>
        /// <exception cref="Exception">本方法不主动引发异常（目标方法名固定存在）。</exception>
        public bool GetMethodIsSynchronized()
        {
            // 反射拿到本类实例方法 IncrementSynchronized 的方法实现标志位
            MethodInfo target = typeof(HMethodImplSynchronizedHelp).GetMethod("IncrementSynchronized");
            MethodImplAttributes attributes = target.GetMethodImplementationFlags();

            // MethodImplAttributes 是位枚举，按位与判断 Synchronized 标志
            return (attributes & MethodImplAttributes.Synchronized) == MethodImplAttributes.Synchronized;
        }
    }
}
