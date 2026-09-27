using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】volatile 帮助类：涵盖 C# <c>volatile</c> 关键字与 <see cref="Volatile"/> 静态类两套机制，
    /// 解决多线程内存"可见性 + 顺序"问题：禁止 JIT/CPU 把对该字段的访问缓存进寄存器或与相邻内存操作重排序。
    /// volatile 读具有"获取语义（Acquire）"，volatile 写具有"释放语义（Release）"，都是单向栅栏。
    /// 【是否跨进程】否。仅约束进程内线程对托管内存的观察顺序。
    /// 【典型适用场景】简单状态标志（bool 停止位、枚举状态）、不可变对象的安全发布、无锁算法中的
    /// 单次读/写；不配合任何锁即可保证"一个线程写、其他线程及时看到"。
    /// 【使用步骤】
    /// <list type="number">
    /// <item>字段可声明 volatile 时：<c>private volatile bool running;</c>，正常读写即可。</item>
    /// <item>不能声明时（long/double、数组元素、被闭包捕获、泛型 T）：用 <c>Volatile.Read(ref x)</c>
    /// 与 <c>Volatile.Write(ref x, v)</c> 显式包裹每一次访问（两边都要包，只包一侧无效）。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>volatile 只保证单次读或写的可见性/顺序，不保证"读-改-写"原子：
    /// <c>volatileInt++</c> 仍是读、加、写三步，计数必须用 <see cref="Interlocked"/>。</item>
    /// <item>volatile 不提供互斥，不能保护多字段/多语句的复合不变量，那种场景用 lock。</item>
    /// <item>C# volatile 关键字【可声明的类型】：sbyte、byte、short、ushort、int、uint、char、float、bool；
    /// 以上整数类型为基础类型的 enum；所有引用类型（class/接口/委托/string/数组，含 null 赋值）；
    /// IntPtr、UIntPtr；unsafe 上下文中的指针类型。
    /// 【不能声明】long、ulong、double、decimal 与所有 struct（8 字节及以上类型关键字不支持，
    /// 需要可见性时改用 <c>Volatile.Read/Write</c>，需要原子算术时用 Interlocked）。</item>
    /// <item><see cref="Volatile"/> 静态类（.NET Framework 4.5+）的 Read/Write 重载覆盖更广：
    /// bool/byte/sbyte/short/ushort/int/uint/long/ulong/IntPtr/UIntPtr/float/double、object
    /// 与泛型 T（class），即 long/double 也能获得易变访问。</item>
    /// </list>
    /// 【版本可用性】volatile 关键字随 C# 1.0；<see cref="Volatile"/> 类自 .NET Framework 4.5。
    /// 更高版本 API（仅注释说明，4.8 不可用）：<c>Volatile.ReadBarrier()</c>/<c>Volatile.WriteBarrier()</c>
    /// 为 .NET 10+ 新增的纯获取/释放栅栏方法。
    /// </summary>
    public static class HVolatileHelp
    {
        /// <summary>示例1：标准停止标志：工作线程轮询 volatile bool，另一线程置 false 后下一轮检查即可见即停。</summary>
        public class Worker
        {
            private volatile bool running = true;   // 保证工作线程读到最新值，不被 JIT 优化成寄存器死循环
            private Thread thread;

            /// <summary>启动后台工作线程并将运行标志置为 true。</summary>
            public void Start()
            {
                running = true;
                thread = new Thread(delegate ()
                {
                    while (running)
                    {
                        // 业务循环；注意循环体不应长时间阻塞，否则无法及时响应停止
                    }
                })
                { IsBackground = true };
                thread.Start();
            }

            /// <summary>请求停止：写 volatile 字段，工作线程在下一轮条件判断时立即看到。</summary>
            /// <param name="joinTimeoutMs">等待工作线程退出的 Join 毫秒数。</param>
            /// <returns>true 表示线程已退出；false 表示超时仍在运行。</returns>
            public bool Stop(int joinTimeoutMs)
            {
                running = false;                     // volatile 写：释放语义，此前的普通写一并对其他线程可见
                if (thread != null)
                {
                    return thread.Join(joinTimeoutMs);
                }
                return true;
            }
        }

        /// <summary>示例2：volatile 不能用于复合操作：自增不是原子的，计数器必须用 Interlocked。</summary>
        /// <param name="counter">计数器变量（普通或 volatile 字段均适用本结论）。</param>
        public static void CorrectCounter(ref int counter)
        {
            // 错误写法（counter 是普通/volatile 字段都不行）：counter++;
            Interlocked.Increment(ref counter);
        }

        /// <summary>示例3：字段无法声明 volatile 时（闭包捕获、数组元素等），用 <see cref="Volatile.Read(ref bool)"/> 显式易变读。</summary>
        /// <param name="flag">标志变量。</param>
        /// <returns>读到的最新值（获取语义）。</returns>
        public static bool ReadFlag(ref bool flag)
        {
            return Volatile.Read(ref flag);
        }

        /// <summary>示例4：<see cref="Volatile.Write(ref bool,bool)"/> 显式易变写（释放语义，写前的操作不会重排到写之后）。</summary>
        /// <param name="flag">标志变量。</param>
        /// <param name="value">要写入的值。</param>
        public static void WriteFlag(ref bool flag, bool value)
        {
            Volatile.Write(ref flag, value);
        }

        /// <summary>
        /// 示例5：关键字不支持的 long 也能做易变读（<see cref="Volatile.Read(ref long)"/>，4.5+）。
        /// 注意：这里只保证"可见性 + 顺序"；需要原子 +1 请用 <see cref="Interlocked"/>。
        /// </summary>
        /// <param name="location">64 位变量。</param>
        /// <returns>具有获取语义的读取值。</returns>
        public static long ReadInt64(ref long location)
        {
            return Volatile.Read(ref location);
        }

        /// <summary>示例6：long 易变写（<see cref="Volatile.Write(ref long,long)"/>）。</summary>
        /// <param name="location">64 位变量。</param>
        /// <param name="value">要写入的值。</param>
        public static void WriteInt64(ref long location, long value)
        {
            Volatile.Write(ref location, value);
        }

        /// <summary>示例7：double 易变读（double 不能声明 volatile 关键字，但 Volatile 类有重载）。</summary>
        /// <param name="location">浮点变量。</param>
        /// <returns>具有获取语义的读取值。</returns>
        public static double ReadDouble(ref double location)
        {
            return Volatile.Read(ref location);
        }

        /// <summary>示例8：double 易变写。</summary>
        /// <param name="location">浮点变量。</param>
        /// <param name="value">要写入的值。</param>
        public static void WriteDouble(ref double location, double value)
        {
            Volatile.Write(ref location, value);
        }

        /// <summary>
        /// 示例9：泛型易变读引用类型（<c>Volatile.Read&lt;T&gt;</c>，T 必须是 class），
        /// 用于读取其他线程发布的不可变对象。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="location">对象引用槽。</param>
        /// <returns>发布的引用（获取语义，保证看到发布方在写引用前完成的全部字段初始化）。</returns>
        public static T ReadPublishedReference<T>(ref T location) where T : class
        {
            return Volatile.Read(ref location);
        }

        /// <summary>
        /// 示例10：泛型易变写发布不可变对象（先构造完毕，再用一条 Volatile.Write 发布引用，
        /// 读取方通过 <see cref="ReadPublishedReference{T}"/> 看到完整对象）。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="location">对象引用槽。</param>
        /// <param name="value">构造完成、准备发布的对象。</param>
        public static void PublishReference<T>(ref T location, T value) where T : class
        {
            Volatile.Write(ref location, value);
        }
    }
}
