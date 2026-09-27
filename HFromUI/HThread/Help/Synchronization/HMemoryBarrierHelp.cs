using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】内存屏障（Memory Barrier / Fence）帮助类：多核 CPU 的存储缓冲、乱序执行以及 JIT 优化
    /// 都可能重排内存读写，屏障用于显式约束"屏障前后内存操作的可见顺序"。常见三类：
    /// <list type="bullet">
    /// <item>全屏障（Full Fence）：屏障前的读、写都不能与屏障后的读、写互相越过；
    /// <see cref="Thread.MemoryBarrier"/> 与 <see cref="Interlocked.MemoryBarrier"/> 都是全屏障。</item>
    /// <item>获取栅栏（Acquire Fence）：位于"读"之后，屏障之后的读写不能被重排到该读之前；
    /// <see cref="Volatile.Read"/>、volatile 字段读、lock 进入均具有获取语义。</item>
    /// <item>释放栅栏（Release Fence）：位于"写"之前，屏障之前的读写不能被重排到该写之后；
    /// <see cref="Volatile.Write"/>、volatile 字段写、lock 退出（Monitor.Exit）均具有释放语义。</item>
    /// </list>
    /// 【是否跨进程】否。约束的是本进程内处理器/JIT 对内存操作的排序。
    /// 【典型适用场景】无锁数据结构中的安全发布（先写数据再写"已发布"标志）、无锁单例、
    /// 标志位驱动的停止/就绪协议。获取-释放配对可以保证"发布方在标志置位前的所有写，
    /// 在读取方看到标志后必然可见"。
    /// 【使用步骤】
    /// <list type="number">
    /// <item>发布方：把全部数据写完，最后执行一次释放栅栏（Volatile.Write 标志）。</item>
    /// <item>读取方：先执行一次获取栅栏（Volatile.Read 标志），读到已发布后再读数据。</item>
    /// <item>需要双向都不重排时用全屏障；业务代码优先选择 lock/Interlocked/volatile，屏障只用于无锁底层。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>屏障约束的是"可见性顺序"而非"强制刷新时机"：读写双方必须使用匹配的获取/释放模式。</item>
    /// <item>Interlocked 的所有原子方法、lock（Monitor.Enter/Exit）内部都已包含屏障，不要重复添加。</item>
    /// <item>在 x86/x64 上普通写即具释放语义、普通读即具获取语义（硬件强排序），但在 ARM 等弱排序
    /// 架构上不是，代码不能依赖特定 CPU 的宽松行为。</item>
    /// <item>手写屏障极容易出错且难以复现调试，除非编写无锁基础结构，否则不要使用。</item>
    /// </list>
    /// 【版本可用性】<see cref="Thread.MemoryBarrier"/> 自 .NET Framework 2.0；
    /// <see cref="Volatile.Read/Write"/> 与 <see cref="Interlocked.MemoryBarrier"/> 为 4.5 新增。
    /// 更高版本 API（仅注释说明，4.8 不可用、本类不调用）：
    /// <c>Interlocked.MemoryBarrierProcessWide()</c> 为 .NET 5+ 新增，作用范围是整个进程的所有处理器
    /// （比线程级全屏障重得多，Windows 上通过 FlushProcessWriteBuffers 实现）。
    /// </summary>
    public static class HMemoryBarrierHelp
    {
        /// <summary>示例1：全屏障 —— 保证屏障前的所有读写先于屏障后的读写对其他线程可见。</summary>
        /// <param name="a">先写入的变量。</param>
        /// <param name="b">在屏障之后写入的变量。</param>
        public static void FullBarrier(ref int a, ref int b)
        {
            a = 1;
            Thread.MemoryBarrier();                 // 全栅栏：a=1 一定先于 b=2 被其他核观察到
            b = 2;
        }

        /// <summary>
        /// 示例2：获取/释放单向栅栏配对（最常用的无锁发布写法）。同方法内演示发布方与读取方两侧逻辑，
        /// 实际使用时它们分属两个线程。
        /// </summary>
        /// <param name="data">被发布的数据。</param>
        /// <param name="published">发布标志。</param>
        public static void PublishAndRead(ref int data, ref bool published)
        {
            // 发布方：先写数据，再做释放栅栏写标志，此前的数据写不会被重排到标志写之后
            data = 42;
            Volatile.Write(ref published, true);

            // 读取方：先做获取栅栏读标志，看到 true 之后再读数据，后续读不会被重排到标志读之前
            if (Volatile.Read(ref published))
            {
                int got = data;                     // 必然读到发布方写入的 42
                GC.KeepAlive(got);
            }
        }

        /// <summary>
        /// 示例3：无锁单例 —— <see cref="Interlocked.CompareExchange"/> 自带全屏障，发布后其他线程
        /// 读到的就是完整构造的对象；<see cref="Volatile.Read"/> 做无锁快路径读取。
        /// </summary>
        public sealed class LazyOnce
        {
            private static object instance;

            private LazyOnce()
            {
            }

            /// <summary>获取单例（多线程安全，无锁快路径 + CAS 慢路径）。</summary>
            /// <returns>唯一实例。</returns>
            public static object Get()
            {
                object current = Volatile.Read(ref instance);   // 获取语义的快路径读
                if (current == null)
                {
                    object created = new object();
                    // CompareExchange 自带全屏障：只有一个线程发布成功
                    if (Interlocked.CompareExchange(ref instance, created, null) == null)
                    {
                        current = created;
                    }
                    else
                    {
                        current = Volatile.Read(ref instance);   // 输家读取赢家发布的实例
                    }
                }
                return current;
            }
        }

        /// <summary>
        /// 示例4：<see cref="Interlocked.MemoryBarrier"/>（.NET Framework 4.5+）与
        /// <see cref="Thread.MemoryBarrier"/> 语义相同，都是全屏障；写法选择只看代码可读性。
        /// </summary>
        public static void InterlockedBarrierExample()
        {
            int x = 0;
            Interlocked.MemoryBarrier();           // 等价 Thread.MemoryBarrier()
            int y = x;
            GC.KeepAlive(y);
        }

        /// <summary>
        /// 示例5：显式的"获取-释放"两阶段协议：发布方用 <see cref="Volatile.Write(ref int,int)"/>
        /// 结束临界操作（释放栅栏），读取方用 <see cref="Volatile.Read(ref int)"/> 开始（获取栅栏）。
        /// </summary>
        /// <param name="data">负载数据。</param>
        /// <param name="ready">就绪标志：0 未就绪，1 已就绪。</param>
        /// <returns>读取方在看到就绪后读到的数据；未就绪时为 -1。</returns>
        public static int AcquireReleaseProtocol(ref int data, ref int ready)
        {
            // —— 发布方（通常在另一线程）——
            data = 100;
            Volatile.Write(ref ready, 1);          // 释放栅栏：data=100 不会重排到 ready=1 之后

            // —— 读取方 ——
            if (Volatile.Read(ref ready) == 1)     // 获取栅栏：之后的读不会重排到此读之前
            {
                return data;                       // 保证看到 100
            }
            return -1;
        }

        /// <summary>
        /// 示例6：经典双重检查锁定（Double-Check Locking）。lock 自带获取/释放屏障，
        /// 但快路径的无锁读仍需 volatile（或 Volatile.Read），这是 DCL 正确的必要条件。
        /// </summary>
        /// <typeparam name="T">单例类型（引用类型，须有无参构造或由工厂创建）。</typeparam>
        public sealed class DoubleCheckedSingleton<T> where T : class, new()
        {
            private static volatile T instance;    // volatile：禁止 DCL 快路径读到"半构造"对象
            private static readonly object syncRoot = new object();

            private DoubleCheckedSingleton()
            {
            }

            /// <summary>获取单例：无锁快路径 + 锁内二次检查。</summary>
            /// <returns>唯一实例。</returns>
            public static T Instance
            {
                get
                {
                    if (instance == null)          // 第一次检查：无锁，靠 volatile 读保证可见性
                    {
                        lock (syncRoot)
                        {
                            if (instance == null)  // 第二次检查：持锁后确认，防止多线程重复创建
                            {
                                instance = new T();
                            }
                        }
                    }
                    return instance;
                }
            }
        }
    }
}
