using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.ThreadLocal
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="ThreadLocal{T}"/> 帮助类：[ThreadStatic] 的功能加强版（IDisposable 包装对象）。
    /// 核心成员：
    /// - 构造函数 new ThreadLocal&lt;T&gt;(Func&lt;T&gt; valueFactory)：每个线程【第一次访问 Value】时
    ///   由工厂委托自动初始化（根治 [ThreadStatic] 初始化器只执行一次的坑）；
    /// - Value：本线程专属值（读写）；
    /// - IsValueCreated：本线程是否已触发过初始化（只想探测、不想误触发工厂时用）；
    /// - Values：一次性拿到【所有线程】各自那份值（聚合/并行归并用），仅 trackAllValues=true 时可用；
    /// - trackAllValues 构造参数【极易踩坑】：无参构造 ThreadLocal&lt;T&gt;() 默认 true（可用 Values）；
    ///   但【工厂单参构造 ThreadLocal&lt;T&gt;(Func&lt;T&gt;) 默认 false】（不跟踪，访问 Values 立即抛
    ///   InvalidOperationException）——需要 Values 时务必用两参构造显式传 true；
    /// - Dispose：清理内部资源；Dispose 后再访问 Value/Values 抛 ObjectDisposedException。
    ///
    /// 【是否跨进程】否；同 [ThreadStatic]，按托管线程隔离，不随 async/await 流转。
    ///
    /// 【典型适用场景】
    /// 1) 需要工厂自动初始化的每线程状态（计数器、随机数、缓冲区）；
    /// 2) Parallel/PLINQ 的线程本地结果：每个线程各自累加，最后用 Values 汇总（Map-Reduce 风格）；
    /// 3) 需要在运行时枚举/聚合并行各线程结果（[ThreadStatic] 做不到）。
    ///
    /// 【使用步骤】
    /// 1) using/new 创建 ThreadLocal&lt;T&gt;，传工厂委托；
    /// 2) 各线程直接读写 .Value，首次访问自动初始化；
    /// 3) 需要全局聚合时 foreach .Values；
    /// 4) using/Dispose 释放（线程持有的 T 若是 IDisposable 需自行额外清理）。
    ///
    /// 【注意事项与坑】
    /// 1) 线程池复用残留：与 [ThreadStatic] 一样，租借同一线程的新任务会看到旧值，
    ///    ThreadLocal 不会替你重置——任务开始时自行赋初值；
    /// 2) 工厂委托在“每个线程第一次访问 Value”时调用，且仅一次；工厂内再次访问同一 ThreadLocal.Value
    ///    会抛 InvalidOperationException（防止递归初始化）；
    /// 3) Values 是快照式枚举；有线程在并发修改时枚举可能抛异常或拿到不一致结果，聚合时自行同步；
    /// 4) trackAllValues=false 省内存但 Values 立即抛 InvalidOperationException；
    ///    注意工厂单参构造默认就是 false（本类旧版示例曾在此踩坑），要用 Values 请显式 new ThreadLocal&lt;T&gt;(工厂, true)；
    /// 5) ThreadLocal 实现基于 async-aware 的 ExecutionContext 数据槽，性能略低于 [ThreadStatic]
    ///    （但语义上仍【不跨 await 跟随】）；极高频路径用 [ThreadStatic]；
    /// 6) 忘记 Dispose 会泄漏内部的数据槽（长期运行的服务尤其要注意）。
    ///
    /// 【版本可用性】<see cref="ThreadLocal{T}"/> .NET Framework 4.0+ 提供；
    /// trackAllValues 构造重载 4.5+；net48 全部可用。
    /// </summary>
    ///
    /// <example>
    /// 并行累加后聚合：
    /// <code>
    /// using (ThreadLocal&lt;int&gt; local = new ThreadLocal&lt;int&gt;(() =&gt; 0))
    /// {
    ///     Parallel.For(0, 1000, i =&gt; local.Value += i);
    ///     int total = local.Values.Sum();   // 每个线程一份部分和，再汇总
    /// }
    /// </code>
    /// </example>
    public static class HThreadLocalHelp
    {
        /// <summary>
        /// 示例1：工厂初始化的线程本地计数器，每个线程第一次访问自动从 100 开始。
        /// </summary>
        public sealed class PerThreadCounter : IDisposable
        {
            // 工厂委托：每个线程首次读 Value 时调用一次；
            // 显式 trackAllValues:true——工厂单参构造默认 false，那样 SumAllThreads 访问 Values 会抛异常
            private readonly ThreadLocal<int> counter = new ThreadLocal<int>(delegate { return 100; }, true);

            /// <summary>
            /// 取出当前线程的下一个序号（自增）。
            /// </summary>
            /// <returns>自增后的值。</returns>
            /// <exception cref="ObjectDisposedException">对象已 Dispose 后调用时抛出。</exception>
            public int Next()
            {
                // Value 的读改写只在本线程内发生，无需加锁
                counter.Value++;
                return counter.Value;
            }

            /// <summary>
            /// 汇总所有线程的当前值（trackAllValues 默认开启）。
            /// </summary>
            /// <returns>各线程计数之和。</returns>
            public int SumAllThreads()
            {
                return counter.Values.Sum();
            }

            /// <summary>
            /// 释放 ThreadLocal 内部数据槽。
            /// </summary>
            public void Dispose()
            {
                counter.Dispose();
            }
        }

        /// <summary>
        /// 示例2：汇总所有线程的本地值（聚合并行结果）。using 保证数据槽释放。
        /// </summary>
        /// <returns>当前线程这一份值 +1 后，所有线程值的总和。</returns>
        public static int SumAll()
        {
            // 工厂 + true：不写 true 时工厂构造默认不跟踪 Values，访问 local.Values 会抛异常
            using (ThreadLocal<int> local =
                   new ThreadLocal<int>(delegate { return Environment.CurrentManagedThreadId % 7; }, true))
            {
                local.Value = local.Value + 1;
                int sum = 0;
                foreach (int v in local.Values)   // 枚举每个线程各自的一份
                {
                    sum += v;
                }

                return sum;
            }
        }

        /// <summary>
        /// 示例3：线程池线程复用陷阱——租借开始时显式重置。
        /// </summary>
        /// <param name="slot">线程本地槽；不能为 null。</param>
        /// <exception cref="ArgumentNullException">slot 为 null 时抛出。</exception>
        public static void ResetOnRent(ThreadLocal<string> slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("数据槽不能为空"));
            }

            slot.Value = null;       // 不重置可能读到上一个租借此线程的任务残留
        }

        /// <summary>
        /// 示例4：IsValueCreated 判断本线程是否已触发过初始化（探测而不触发工厂）。
        /// </summary>
        /// <param name="slot">线程本地槽；不能为 null。</param>
        /// <returns>当前线程已初始化返回 true。</returns>
        /// <exception cref="ArgumentNullException">slot 为 null 时抛出。</exception>
        public static bool Initialized(ThreadLocal<object> slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("数据槽不能为空"));
            }

            return slot.IsValueCreated;   // 读 Value 会触发工厂，IsValueCreated 不会
        }

        /// <summary>
        /// 示例5：创建【不跟踪所有线程值】的 ThreadLocal（trackAllValues=false）。
        /// 适用：只关心本线程值、线程数可能很多、不需要 Values 聚合的场景，能减少引用持有与内存占用。
        /// </summary>
        /// <typeparam name="T">值类型。</typeparam>
        /// <param name="valueFactory">每线程首次访问时的工厂；不能为 null。</param>
        /// <returns>不跟踪其他线程值的 ThreadLocal 实例（调用方负责 Dispose）。</returns>
        /// <exception cref="ArgumentNullException">valueFactory 为 null 时抛出。</exception>
        public static ThreadLocal<T> CreateUntracked<T>(Func<T> valueFactory)
        {
            if (valueFactory == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("值工厂不能为空"));
            }

            // false：访问 .Values 会抛 InvalidOperationException，仅 .Value 可用
            return new ThreadLocal<T>(valueFactory, false);
        }

        /// <summary>
        /// 示例6：并行聚合自测——多个线程池任务各自在自己的本地槽累加，最后用 Values 汇总。
        /// 不依赖 UI/网络，任务数请传小值以便快速完成。
        /// </summary>
        /// <param name="taskCount">并行任务数（每个任务累加 1）。</param>
        /// <returns>所有线程本地值的总和（不小于 taskCount）。</returns>
        /// <exception cref="ArgumentOutOfRangeException">taskCount 为负数时抛出。</exception>
        public static async Task<int> ParallelAggregateDemoAsync(int taskCount)
        {
            if (taskCount < 0)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("任务数量不能小于 0"));
            }

            using (ThreadLocal<int> local = new ThreadLocal<int>(delegate { return 0; }, true))
            {
                Task[] tasks = new Task[taskCount];
                for (int i = 0; i < taskCount; i++)
                {
                    tasks[i] = Task.Run(delegate { local.Value++; });
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                int sum = 0;
                // Values 枚举的是“每个触碰过该槽的线程”的一份；同一线程执行多个任务只占一份但值已累加
                foreach (int partial in local.Values)
                {
                    sum += partial;
                }

                return sum;   // 恰好等于 taskCount
            }
        }
    }
}
