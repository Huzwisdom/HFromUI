using System;
using System.Threading;

namespace HFromUI.HThread.Help.Diagnostics
{
    using HFromUI.HLangage;
    /// <summary>
    /// 线程诊断与线程池配置帮助类：当前线程全景信息、线程池容量查询/设置、线程优先级、线程主动等待四件套。
    /// 【是什么】聚合三类基础诊断能力：
    /// 1) Thread.CurrentThread 的全部常用只读信息（身份/状态/优先级/单元模型/区域性/安全主体）；
    /// 2) ThreadPool 静态容量 API：GetAvailableThreads/GetMaxThreads/SetMaxThreads/GetMinThreads/SetMinThreads；
    /// 3) 线程让出与等待：Thread.Sleep(0)/Sleep(1)/Sleep(ms)/Sleep(TimeSpan)、Thread.Yield、Thread.SpinWait、Thread.Join 三重载。
    /// 【是否跨进程】否。全部信息仅描述本进程的托管线程与本进程的 CLR 线程池。
    /// 【典型适用场景】日志埋点（哪个线程/是否线程池/什么优先级触发的回调）、启动期线程池容量诊断、
    /// 高并发突发时调大 MinThreads 缓解爬坡延迟、教学/排查时理解 Sleep/Yield/SpinWait 的差异、Join 带超时收尾。
    /// 【使用步骤】
    /// 1) 用 ThreadSnapshot.CaptureCurrent() 或 DescribeCurrentThread() 抓当前线程快照；
    /// 2) 用 ThreadPoolSnapshot.Capture() 一次性查看可用/上限/下限/核数；
    /// 3) 确需调整时先 Get 原值，SetMinThreads/SetMaxThreads 必须判断返回值（false=被拒绝，不抛异常）；
    /// 4) 线程创建后立即设 ThreadPriority；等待退出一律用带超时 Join。
    /// 【注意事项与坑】
    /// - ThreadPriority 五级（Lowest/BelowNormal/Normal【默认】/AboveNormal/Highest）只是相对调度倾向，
    ///   不是实时保证、不决定正确性；实际优先级还要乘上进程优先级类；高优先级线程长期忙会饿死低优先级线程；
    /// - ThreadState 是位标志组合（如 Background|WaitSleepJoin），不要用 == 判等，用 HasFlag/按位与；
    /// - ApartmentState 用 GetApartmentState() 读取（ApartmentState 属性已标注 Obsolete）；
    /// - SetMaxThreads 返回 false 的条件（官方文档）：小于处理器核数 Environment.ProcessorCount、或小于当前 MinThreads、
    ///   或宿主（IIS/SQL Server 等）禁止修改；上限实际还可能被截到 short.MaxValue；
    /// - SetMinThreads 返回 false 的条件：负数或大于当前 MaxThreads；默认最小值约等于处理器核数，
    ///   线程数超过最小值后，新线程注入受爬坡算法限制（约每秒注入 1 个工作线程，文档旧版明确，版本间略有差异），
    ///   突发短任务会因此排队等待——这正是适度调大 MinThreads 的唯一正当理由；过大最小值会增加上下文切换/内存/GC 开销；
    /// - Sleep(0)：让出剩余时间片但只让给【同优先级】就绪线程，没有则立即返回，不适合"等等再说"；
    /// - Sleep(1)：真正挂起约 1ms（受 ~15ms 系统时钟分辨率影响常常更久），其他优先级/进程线程可运行；
    /// - Thread.Yield()：把时间片让给【同一处理器】上任意就绪线程（含低优先级），没有就绪线程立即返回 false，
    ///   比 Sleep(1) 便宜（不切到等待队列、不耗时 ~15ms）；
    /// - Thread.SpinWait(n)：不放弃时间片、CPU 空转 n 个迭代，仅用于极短（几十纳秒级）等待，长等用 Sleep/锁；
    /// - Join() 无超时重载可能无限等待，库/自测代码只用 Join(int)/Join(TimeSpan)；
    /// - CurrentCulture/CurrentUICulture 在线程池线程上可能不是你以为的值（不会自动从 UI 线程流转）。
    /// 【版本可用性】net48 全部可用：Thread 上述属性/GetApartmentState、ThreadPool 五件套、Sleep/Yield/SpinWait/Join；
    /// .NET Core/.NET 5+ 同名 API 基本保留；Thread.Abort/Suspend/Resume 已被现代框架移除，本类不涉及。
    /// </summary>
    public static class HThreadDiagnosticsHelp
    {
        /// <summary>
        /// 示例1：当前线程简述（日志埋点一行流；完整字段用 ThreadSnapshot.CaptureCurrent）。
        /// </summary>
        /// <returns>含 Id/Name/IsBackground/IsThreadPoolThread/Priority 的字符串</returns>
        public static string DescribeCurrentThread()
        {
            Thread t = Thread.CurrentThread;
            return string.Format("Id={0} Name={1} IsBackground={2} IsThreadPoolThread={3} Priority={4}",
                                 t.ManagedThreadId, t.Name, t.IsBackground, t.IsThreadPoolThread, t.Priority);
        }

        /// <summary>
        /// 示例2：查询线程池容量：工作线程/IO线程各自的当前可用数与上限。
        /// </summary>
        /// <param name="workerAvailable">输出：当前可用工作线程数（上限减去已在用）</param>
        /// <param name="ioAvailable">输出：当前可用异步 IO 线程数</param>
        /// <param name="workerMax">输出：工作线程上限</param>
        /// <param name="ioMax">输出：异步 IO 线程上限</param>
        public static void GetThreadPoolInfo(out int workerAvailable, out int ioAvailable,
                                            out int workerMax, out int ioMax)
        {
            // 注意：两次静态调用之间数字可能变化，仅作瞬时快照参考
            ThreadPool.GetAvailableThreads(out workerAvailable, out ioAvailable);
            ThreadPool.GetMaxThreads(out workerMax, out ioMax);
        }

        /// <summary>
        /// 示例3：设置最小线程数——突发短时任务借此避免"超过最小值后约每秒注入 1 个线程"的爬坡延迟；
        /// 返回 false 表示被拒绝（负数或大于 MaxThreads），不抛异常。
        /// </summary>
        /// <param name="minWorker">最小工作线程数（经验值：核数的 1~几倍，按压测结果定，忌拍脑袋调大）</param>
        /// <param name="minIo">最小异步 IO 线程数</param>
        /// <returns>true=设置成功；false=被拒绝（参数非法或宿主限制）</returns>
        public static bool SetMinThreads(int minWorker, int minIo)
        {
            // 值不要照抄核数之外的大值，过多常驻线程反而增加调度开销；返回false表示被拒绝
            return ThreadPool.SetMinThreads(minWorker, minIo);
        }

        /// <summary>
        /// 示例4：设置最大线程数（不建议常规调用；只在压测证明默认上限不合理时使用）。
        /// </summary>
        /// <param name="maxWorker">最大工作线程数；不能小于核数、不能小于当前最小工作线程数</param>
        /// <param name="maxIo">最大异步 IO 线程数；同样不能小于核数/对应最小值</param>
        /// <returns>true=成功；false=被拒绝（小于核数、小于 MinThreads、超过实现上限或宿主/IIS 禁止修改）</returns>
        public static bool SetMaxThreads(int maxWorker, int maxIo)
        {
            // 官方坑：小于 Environment.ProcessorCount 或当前 MinThreads 一律 false；宿主托管时也可能 false
            return ThreadPool.SetMaxThreads(maxWorker, maxIo);
        }

        /// <summary>
        /// 示例5：查询当前最小线程数（SetMinThreads 之后核对）。
        /// </summary>
        /// <param name="minWorker">输出：最小工作线程数</param>
        /// <param name="minIo">输出：最小异步 IO 线程数</param>
        public static void GetMinThreads(out int minWorker, out int minIo)
        {
            ThreadPool.GetMinThreads(out minWorker, out minIo);
        }

        /// <summary>
        /// 示例6：创建线程时设置优先级（必须在 Start 之前设置；优先级是"倾向性"不是实时保证）。
        /// </summary>
        /// <param name="start">线程执行体</param>
        /// <param name="priority">五级优先级之一：Lowest/BelowNormal/Normal/AboveNormal/Highest</param>
        /// <returns>尚未 Start 的 Thread（由调用方 Start）</returns>
        /// <exception cref="ArgumentNullException">start 为 null</exception>
        public static Thread CreateWithPriority(ThreadStart start, ThreadPriority priority)
        {
            Thread thread = new Thread(start);
            thread.Priority = priority;   // Lowest/BelowNormal/Normal/AboveNormal/Highest
            return thread;
        }

        /// <summary>
        /// 示例7：把 ThreadPriority 五级翻译为中文说明（日志/UI 展示用）。
        /// </summary>
        /// <param name="priority">线程优先级</param>
        /// <returns>中文含义说明字符串</returns>
        public static string DescribePriority(ThreadPriority priority)
        {
            // 五级相对优先级（同进程内相对彼此），Normal 为默认
            switch (priority)
            {
                case ThreadPriority.Lowest:
                    return HTranslation.GetContent("Lowest=最低：可被任何更高优先级线程抢占");
                case ThreadPriority.BelowNormal:
                    return HTranslation.GetContent("BelowNormal=低于正常：后台非关键任务");
                case ThreadPriority.Normal:
                    return HTranslation.GetContent("Normal=正常（默认）：绝大多数线程");
                case ThreadPriority.AboveNormal:
                    return HTranslation.GetContent("AboveNormal=高于正常：响应敏感任务");
                case ThreadPriority.Highest:
                    return HTranslation.GetContent("Highest=最高：慎用，长期忙会饿死其他线程");
                default:
                    return priority.ToString();
            }
        }

        /// <summary>
        /// 示例8：Sleep(int) 挂起当前线程指定毫秒；0=只让出给同优先级就绪线程；-1(Timeout.Infinite)=无限睡眠。
        /// </summary>
        /// <param name="milliseconds">挂起毫秒数；0=仅让出；-1=无限（不建议）</param>
        /// <exception cref="ArgumentOutOfRangeException">milliseconds 为小于 -1 的负数</exception>
        public static void SleepMs(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// 示例9：Sleep(TimeSpan) 重载（内部仍按毫秒取整，精度受系统时钟约 15ms 限制）。
        /// </summary>
        /// <param name="timeout">挂起时长；负值（非 InfiniteTimeSpan）会抛异常</param>
        public static void SleepTimeSpan(TimeSpan timeout)
        {
            Thread.Sleep(timeout);
        }

        /// <summary>
        /// 示例10：Sleep(0)：立即让出当前时间片；仅当存在【同优先级】就绪线程时才真正切换，否则立即继续执行。
        /// 适合自旋微调，不适合等待低优先级线程完成工作。
        /// </summary>
        public static void SleepZero()
        {
            // 不经过约 15ms 的等待粒度，但也可能"让不出去"立刻返回
            Thread.Sleep(0);
        }

        /// <summary>
        /// 示例11：Sleep(1)：真正挂起约 1ms（实际常接近系统时钟粒度 ~15ms），任何优先级/进程的就绪线程都可被调度。
        /// </summary>
        public static void SleepOneMs()
        {
            // 最便宜的"真等待"：当前线程进入等待队列，给其他线程让路
            Thread.Sleep(1);
        }

        /// <summary>
        /// 示例12：Yield 让出一个时间片给同处理器上任意就绪线程（没有可运行线程时立即返回 false，比 Sleep(1) 便宜）。
        /// </summary>
        /// <returns>true=确实切换给了其他线程；false=没有就绪线程，当前线程继续</returns>
        public static bool Yield()
        {
            return Thread.Yield();        // 返回false表示没有其他线程可切换
        }

        /// <summary>
        /// 示例13：SpinWait 不放弃时间片忙等若干迭代（仅用于极短等待；详见 HSpinWaitHelp）。
        /// </summary>
        /// <param name="iterations">自旋迭代数（几十~几千，不要传大值空烧 CPU）</param>
        public static void SpinFew(int iterations)
        {
            Thread.SpinWait(iterations);
        }

        /// <summary>
        /// 示例14：Join(int) 等待目标线程结束（带超时；false=超时未结束，不抛异常）。
        /// </summary>
        /// <param name="thread">要等待的线程</param>
        /// <param name="timeoutMs">超时毫秒；0=探测；-1=无限等待（等同无参 Join，不建议）</param>
        /// <returns>true=线程已结束；false=超时</returns>
        /// <exception cref="ArgumentNullException">thread 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">timeoutMs 为小于 -1 的负数</exception>
        public static bool WaitFor(Thread thread, int timeoutMs)
        {
            // Join 重载一：int 毫秒超时
            return thread.Join(timeoutMs);
        }

        /// <summary>
        /// 示例15：Join(TimeSpan) 重载（语义同 int 版，按毫秒取整）。
        /// </summary>
        /// <param name="thread">要等待的线程</param>
        /// <param name="timeout">超时时长</param>
        /// <returns>true=线程已结束；false=超时</returns>
        /// <exception cref="ArgumentNullException">thread 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">timeout 超出合法范围</exception>
        public static bool WaitFor(Thread thread, TimeSpan timeout)
        {
            // Join 重载二：TimeSpan 超时
            return thread.Join(timeout);
        }
        // Join 重载三为无参 thread.Join()：无限等待直到线程结束，库代码不包装、不推荐，仅在此说明。

        /// <summary>
        /// 示例16：四种主动等待方式差异速查（教学/排查用文本）。
        /// </summary>
        /// <returns>SpinWait / Yield / Sleep(0) / Sleep(1) 的对比说明</returns>
        public static string ExplainWaitStrategies()
        {
            return
                HTranslation.GetContent("SpinWait(n)：不释放时间片，CPU 空转，适合纳秒/微秒级极短等待；") +
                HTranslation.GetContent("Yield()：让出时间片给同处理器任意就绪线程，无目标则立即返回 false；") +
                HTranslation.GetContent("Sleep(0)：让出时间片但只给同优先级就绪线程，常立即返回；") +
                HTranslation.GetContent("Sleep(1)：真正挂起（实际约15ms粒度），任何线程可被调度。");
        }

        /// <summary>
        /// 当前线程信息快照：一次性抓取 Thread.CurrentThread 全部常用属性（值类型式快照，抓完即固定）。
        /// </summary>
        public sealed class ThreadSnapshot
        {
            private ThreadSnapshot()
            {
                // 仅允许通过 CaptureCurrent 构造，保证字段一致性
            }

            /// <summary>托管线程 Id（进程内唯一，注意非 OS 线程 Id）</summary>
            /// <returns>Thread.ManagedThreadId</returns>
            public int ManagedThreadId { get; private set; }

            /// <summary>线程名（只能赋值一次；未命名为 null）</summary>
            /// <returns>Thread.Name</returns>
            public string Name { get; private set; }

            /// <summary>是否已启动且尚未终止</summary>
            /// <returns>Thread.IsAlive</returns>
            public bool IsAlive { get; private set; }

            /// <summary>是否后台线程（后台线程不阻止进程退出；线程池线程恒为 true）</summary>
            /// <returns>Thread.IsBackground</returns>
            public bool IsBackground { get; private set; }

            /// <summary>是否属于托管线程池</summary>
            /// <returns>Thread.IsThreadPoolThread</returns>
            public bool IsThreadPoolThread { get; private set; }

            /// <summary>线程状态位标志（可能为组合值，勿用 == 比较）</summary>
            /// <returns>Thread.ThreadState</returns>
            public ThreadState ThreadState { get; private set; }

            /// <summary>五级调度优先级</summary>
            /// <returns>Thread.Priority</returns>
            public ThreadPriority Priority { get; private set; }

            /// <summary>COM 单元状态（STA/MTA/Unknown）</summary>
            /// <returns>GetApartmentState() 结果</returns>
            public ApartmentState ApartmentState { get; private set; }

            /// <summary>当前区域性名（如 zh-CN）</summary>
            /// <returns>CurrentCulture.Name</returns>
            public string CurrentCultureName { get; private set; }

            /// <summary>当前 UI 区域性名（资源查找用）</summary>
            /// <returns>CurrentUICulture.Name</returns>
            public string CurrentUiCultureName { get; private set; }

            /// <summary>当前安全主体的身份名（未配置时为 null）</summary>
            /// <returns>CurrentPrincipal.Identity.Name 或 null</returns>
            public string PrincipalName { get; private set; }

            /// <summary>
            /// 抓取当前线程快照。
            /// </summary>
            /// <returns>填充完毕的 ThreadSnapshot</returns>
            public static ThreadSnapshot CaptureCurrent()
            {
                Thread t = Thread.CurrentThread;
                ThreadSnapshot snapshot = new ThreadSnapshot
                {
                    ManagedThreadId = t.ManagedThreadId,
                    Name = t.Name,
                    IsAlive = t.IsAlive,
                    IsBackground = t.IsBackground,
                    IsThreadPoolThread = t.IsThreadPoolThread,
                    ThreadState = t.ThreadState,
                    Priority = t.Priority,
                    ApartmentState = t.GetApartmentState(),   // 不用已 Obsolete 的 ApartmentState 属性
                    CurrentCultureName = t.CurrentCulture.Name,
                    CurrentUiCultureName = t.CurrentUICulture.Name
                };
                // CurrentPrincipal 与其 Identity 都可能为 null，逐层防御
                if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
                {
                    snapshot.PrincipalName = Thread.CurrentPrincipal.Identity.Name;
                }
                return snapshot;
            }

            /// <summary>
            /// 多行可读表示（日志用）。
            /// </summary>
            /// <returns>全部字段的格式化文本</returns>
            public override string ToString()
            {
                return string.Format(
                    "ManagedThreadId={0}, Name={1}, IsAlive={2}, IsBackground={3}, IsThreadPoolThread={4}, " +
                    "ThreadState={5}, Priority={6}, ApartmentState={7}, CurrentCulture={8}, CurrentUICulture={9}, Principal={10}",
                    ManagedThreadId, Name, IsAlive, IsBackground, IsThreadPoolThread,
                    ThreadState, Priority, ApartmentState, CurrentCultureName, CurrentUiCultureName,
                    PrincipalName ?? "(none)");
            }
        }

        /// <summary>
        /// 线程池容量快照：一次调用内取齐可用/上限/下限/核数。
        /// </summary>
        public sealed class ThreadPoolSnapshot
        {
            private ThreadPoolSnapshot()
            {
            }

            /// <summary>可用工作线程数（上限减去已在用）</summary>
            /// <returns>GetAvailableThreads 的工作线程值</returns>
            public int WorkerAvailable { get; private set; }

            /// <summary>可用异步 IO 线程数</summary>
            /// <returns>GetAvailableThreads 的 IO 线程值</returns>
            public int IoAvailable { get; private set; }

            /// <summary>最小工作线程数</summary>
            /// <returns>GetMinThreads 工作线程值</returns>
            public int WorkerMin { get; private set; }

            /// <summary>最小异步 IO 线程数</summary>
            /// <returns>GetMinThreads IO 值</returns>
            public int IoMin { get; private set; }

            /// <summary>工作线程上限</summary>
            /// <returns>GetMaxThreads 工作线程值</returns>
            public int WorkerMax { get; private set; }

            /// <summary>异步 IO 线程上限</summary>
            /// <returns>GetMaxThreads IO 值</returns>
            public int IoMax { get; private set; }

            /// <summary>逻辑处理器核数（SetMaxThreads 不可低于此值）</summary>
            /// <returns>Environment.ProcessorCount</returns>
            public int ProcessorCount { get; private set; }

            /// <summary>
            /// 抓取线程池容量快照（三组 out API 各调一次，属近似同一时刻）。
            /// </summary>
            /// <returns>填充完毕的 ThreadPoolSnapshot</returns>
            public static ThreadPoolSnapshot Capture()
            {
                int workerAvailable, ioAvailable, workerMin, ioMin, workerMax, ioMax;
                ThreadPool.GetAvailableThreads(out workerAvailable, out ioAvailable);
                ThreadPool.GetMinThreads(out workerMin, out ioMin);
                ThreadPool.GetMaxThreads(out workerMax, out ioMax);
                return new ThreadPoolSnapshot
                {
                    WorkerAvailable = workerAvailable,
                    IoAvailable = ioAvailable,
                    WorkerMin = workerMin,
                    IoMin = ioMin,
                    WorkerMax = workerMax,
                    IoMax = ioMax,
                    ProcessorCount = Environment.ProcessorCount
                };
            }

            /// <summary>
            /// 可读表示。
            /// </summary>
            /// <returns>工作线程与 IO 线程的 下限/可用/上限 及核数</returns>
            public override string ToString()
            {
                return string.Format(
                    HTranslation.GetContent("CPU核数={0}; 工作线程 最小={1} 可用={2} 上限={3}; IO线程 最小={4} 可用={5} 上限={6}"),
                    ProcessorCount, WorkerMin, WorkerAvailable, WorkerMax,
                    IoMin, IoAvailable, IoMax);
            }
        }
    }
}
