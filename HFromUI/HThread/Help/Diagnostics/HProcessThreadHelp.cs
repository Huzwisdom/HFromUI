using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace HFromUI.HThread.Help.Diagnostics
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】操作系统线程（非托管 OS 层）只读诊断帮助类：包装 <see cref="System.Diagnostics.ProcessThread"/>
    /// 与 <see cref="Process.Threads"/>（<see cref="ProcessThreadCollection"/>）。
    /// ProcessThread 描述的是操作系统调度的内核线程，不是托管 <see cref="System.Threading.Thread"/>，
    /// 两者之间没有稳定映射关系。常用成员逐个说明：
    /// 1) Id：OS 线程 ID（内核 TID），与托管 Thread.ManagedThreadId 完全是两套编号、无对应关系；
    /// 2) StartAddress：线程起始函数的本机内存地址（IntPtr），只是一个地址，拿不到托管方法名；
    /// 3) ThreadState：<see cref="System.Diagnostics.ThreadState"/> 枚举（Initialized/Ready/Running/
    ///    Standby/Terminated/Wait/Transition/Unknown）；WaitReason 仅在 ThreadState==Wait 时有意义，
    ///    其他状态下读取 WaitReason 会抛 <see cref="InvalidOperationException"/>；
    /// 4) WaitReason：内核等待原因枚举（EventPair/Executive/LpcReceive/PageIn/UserRequest/VirtualMemory 等）；
    /// 5) PriorityLevel：<see cref="ProcessPriorityLevel"/> 的线程版本 <see cref="ThreadPriorityLevel"/>，
    ///    “可读写”——读取不需要权限，但修改可能需要提升权限，且可能影响系统稳定性（生产慎用，需管理员）；
    /// 6) StartPriority：线程启动时的操作系统优先级（只读留存值）；
    /// 7) IdealProcessor / ProcessorAffinity：理想处理器与处理器亲和性“位掩码”，用 <see cref="IntPtr"/>
    ///    表示（第 n 位置 1 表示可在第 n 个逻辑核上运行）；32 位进程里是 32 位、64 位进程里是 64 位，
    ///    一律用 ToInt64()/X16 展示避免截断；注意 net48 下这两个属性公开层面“只能写不能读”
    ///    （CanRead=False），读取需另走 kernel32 GetProcessAffinityMask；
    /// 8) TotalProcessorTime / UserProcessorTime / PrivilegedProcessorTime：累计 CPU 时间
    ///    （总时间=用户态时间+内核态时间，TimeSpan），可做两次采样算差值得到线程实时 CPU 占用；
    /// 9) Process.Threads 是 <see cref="ProcessThreadCollection"/>：某一时刻线程列表的快照式集合，
    ///    集合中的线程随时可能退出，访问个别属性可能抛 Win32Exception，需防御。
    ///
    /// 【是否跨进程】API 本身可读“任意有权限的进程”（GetProcessById 等拿到的 Process 的 Threads），
    /// 但本类出于安全自测只读取 Process.GetCurrentProcess() 当前进程，绝不做任何写操作。
    ///
    /// 【典型适用场景】
    /// 1) 诊断高 CPU：统计当前进程线程数、枚举各线程 CPU 时间与等待原因；
    /// 2) 记录崩溃/卡顿现场：线程总数、样例线程 TID 与 WaitReason；
    /// 3) 只读核对处理器亲和性掩码（ProcessorAffinity），排查是否被作业对象/启动配置限制了可用核心；
    /// 4) 教学：理解 OS 线程与托管线程、ThreadState 与托管 ThreadState 的区别。
    ///
    /// 【使用步骤】
    /// 1) using (Process p = Process.GetCurrentProcess()) { p.Refresh(); … }，先 Refresh 保证值是新的；
    /// 2) 遍历 p.Threads：读 Id、ThreadState、TotalProcessorTime 等做统计；
    /// 3) 读 WaitReason 前先判 ThreadState==Wait，或 try/catch InvalidOperationException；
    /// 4) 读 ProcessorAffinity 用 IntPtr.ToInt64() 转 16 进制字符串，全程只读不写；
    /// 5) 需算实时 CPU 占用时，间隔固定时间采两次 TotalProcessorTime.Ticks 求差。
    ///
    /// 【注意事项与坑】
    /// 1) ProcessThread “不提供”安全的 Abort/暂停手段（Suspend/Resume 早已被标记弃用且可能死锁/崩溃，
    ///    托管 Thread.Abort 在现代 .NET 也已移除）；托管程序要让线程协作式停止，请用
    ///    <see cref="HFromUI.HThread.Help.Signaling.HCancellationTokenHelp"/>（CancellationToken）；
    /// 2) 修改 PriorityLevel/ProcessorAffinity 可能需要管理员权限，可能影响系统稳定性、拖垮整机调度，
    ///    本类全部只读，不包装任何写方法；
    /// 3) Id 是 OS 线程 ID，切勿拿它和托管线程对话（无法从 TID 反查托管 Thread）；
    /// 4) WaitReason 只在 Wait 状态有意义，Running/Ready 等状态读取直接抛 InvalidOperationException；
    /// 5) 【net48 实证大坑】ProcessThread.ProcessorAffinity 在 .NET Framework 上是“只写属性”
    ///    （反射实测 CanRead=False/CanWrite=True，连非公开 get 访问器都没有，IdealProcessor 同样只写），
    ///    即能 set 不能 get，直接写 thread.ProcessorAffinity.ToInt64() 编译就报 CS0154；
    ///    读取只能 P/Invoke kernel32 GetProcessAffinityMask（.NET Core/.NET 5+ 才补上 get 访问器）；
    ///    ProcessorAffinity 是位掩码而非核编号：掩码 0x5=二进制 101=可用第 0、2 号核；
    ///    32 位进程只有低 32 位；64 核以上机器上，32 位（WOW64）进程只能表达 32 位掩码
    ///    且可能受 WOW64 处理器数限制（传统上被限制在单个 64 核节点内），
    ///    需要观察/设置超过 64 核亲和性的生产程序应编译为 64 位或直接用原生线程亲和性 API；
    /// 6) 权限不足（如读受保护进程）访问属性会抛 Win32Exception/InvalidOperationException，本类读亲和性
    ///    时统一兜底返回 "access denied" 字符串；
    /// 7) StartAddress 是本机地址，ASLR 下每次运行都可能不同，不要据此做身份判断。
    ///
    /// 【版本可用性】net48 全部可用（ProcessThread 自 .NET 1.1）；.NET Core/.NET 5+ 在 Windows 上保留，
    /// 部分成员在非 Windows 平台抛 PlatformNotSupportedException。
    /// </summary>
    /// <example>
    /// 读取当前进程 OS 线程快照与亲和性掩码（全程只读、毫秒级完成、不弹窗）：
    /// <code>
    /// using HFromUI.HThread.Help.Diagnostics;
    ///
    /// // 一次性快照：线程总数、累计 CPU 时间滴答、样例线程 TID 与等待原因
    /// HProcessThreadHelp.OsThreadSnapshot snapshot = HProcessThreadHelp.SnapshotCurrentProcess();
    /// Console.WriteLine(snapshot.ToString());
    ///
    /// // 只读亲和性位掩码，如 0x000000000000000F 表示低 4 个逻辑核可用；无权限时返回 access denied
    /// string affinity = HProcessThreadHelp.ReadAffinityInfo();
    /// Console.WriteLine("ProcessorAffinity=" + affinity);
    ///
    /// // 协作式停止的正解是 CancellationToken，而不是去强杀 OS 线程：
    /// // 用 HCancellationTokenHelp 创建令牌，工作线程轮询 IsCancellationRequested 自行退出。
    /// </code>
    /// </example>
    public static class HProcessThreadHelp
    {
        /// <summary>
        /// 对当前进程的 OS 线程做一次只读快照：统计线程总数、累加所有线程的
        /// <see cref="ProcessThread.TotalProcessorTime"/> 滴答数，并记录第一条线程的 OS 线程 Id
        /// 与 WaitReason 名称。全程只读：不改优先级、不改亲和性、不暂停/终止任何线程。
        /// </summary>
        /// <returns>填充完毕的 <see cref="OsThreadSnapshot"/>；取不到样例线程时各样例字段为 0/空串。</returns>
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// 读取当前进程信息被系统拒绝（正常情况下当前进程始终可读，理论保底声明）。
        /// </exception>
        public static OsThreadSnapshot SnapshotCurrentProcess()
        {
            int threadCount = 0;
            long totalTimeTicks = 0L;
            int sampleThreadId = 0;
            string sampleWaitReason = string.Empty;

            // using 块保证 Process 句柄及时释放；Get 之后先 Refresh，让 Threads/CPU 时间取到最新值
            using (Process current = Process.GetCurrentProcess())
            {
                current.Refresh();

                // Threads 是某一时刻的快照集合；遍历期间线程退出由运行时保证枚举仍安全
                ProcessThreadCollection threads = current.Threads;
                foreach (ProcessThread thread in threads)
                {
                    // 统计线程总数
                    threadCount++;

                    // 累加内核态+用户态的总 CPU 时间（Ticks：100 纳秒单位）
                    totalTimeTicks += thread.TotalProcessorTime.Ticks;

                    if (threadCount == 1)
                    {
                        // 取第一条线程做样例：记录 OS 线程 Id（注意非托管 ManagedThreadId）
                        sampleThreadId = thread.Id;
                        try
                        {
                            // WaitReason 仅在 ThreadState==Wait 时有意义；
                            // Running/Ready 等状态读取会抛 InvalidOperationException，需兜底
                            sampleWaitReason = thread.WaitReason.ToString();
                        }
                        catch (InvalidOperationException)
                        {
                            // 非等待状态：改记状态名并明确标注 WaitReason 无意义，不让快照整体失败
                            sampleWaitReason = thread.ThreadState.ToString() + HTranslation.GetContent("(非等待状态,WaitReason无意义)");
                        }
                    }
                }
            }

            // 通过嵌套类内部工厂构造，保证快照字段一次性固定、外部不可改
            return OsThreadSnapshot.Build(threadCount, totalTimeTicks, sampleThreadId, sampleWaitReason);
        }

        /// <summary>
        /// 只读获取当前进程处理器亲和性位掩码，并转为 16 进制字符串
        /// （形如 "0x000000000000000F"，第 n 位置 1 表示允许在第 n 个逻辑核运行）。
        /// 先取 <see cref="Process.Threads"/> 的第一条 <see cref="ProcessThread"/>（定位 OS 线程集合），
        /// 但由于 net48 下 ProcessThread.ProcessorAffinity 只有 set 访问器（只写属性，直接读取编译报 CS0154），
        /// 真正取值改走只读 P/Invoke kernel32 GetProcessAffinityMask：未给单线程单设亲和性时，
        /// 进程掩码即各线程允许运行的 CPU 集合。掩码统一按 ToUInt64/X16 输出，32/64 位进程都不截断；只读不写。
        /// </summary>
        /// <returns>形如 "0x" + 16 位十六进制的掩码字符串；线程集合为空返回 "no threads"；
        /// 因权限不足等任何原因读取失败时返回 "access denied"。本方法吞掉全部异常，不向调用方引发。</returns>
        public static string ReadAffinityInfo()
        {
            try
            {
                using (Process current = Process.GetCurrentProcess())
                {
                    current.Refresh();

                    // 取集合第一条 ProcessThread 作为代表（Id 是 OS 线程 TID）
                    ProcessThreadCollection threads = current.Threads;
                    if (threads.Count == 0)
                    {
                        // 理论上当前进程至少有一个线程；防御空集合，返回固定可读文本
                        return "no threads";
                    }

                    ProcessThread first = threads[0];
                    int firstThreadId = first.Id;   // 触达第一个 OS 线程；TID 仅用于表明取样位置
                    if (firstThreadId < 0)
                    {
                        // TID 恒为正数；此分支纯为防御，正常不会进入
                        return "access denied";
                    }

                    // net48 的 first.ProcessorAffinity 只能写不能读；用 kernel32 只读 API 取进程级掩码
                    UIntPtr processMask;
                    UIntPtr systemMask;
                    if (!GetProcessAffinityMask(current.Handle, out processMask, out systemMask))
                    {
                        // API 返回 false（如句柄权限不足）：按约定返回固定字符串，不尝试抛 Win32Exception
                        return "access denied";
                    }

                    // 统一按 64 位无符号转十六进制；WOW64(32 位进程) 下高位自然为 0
                    long maskValue = unchecked((long)processMask.ToUInt64());
                    return "0x" + maskValue.ToString("X16", CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                // 权限不足（Win32Exception）、WOW64/平台限制等：按约定统一返回固定字符串，不向调用方抛
                return "access denied";
            }
        }

        // kernel32 只读查询：取进程亲和性掩码与系统亲和性掩码（均为原生位掩码；
        // 第 n 位置 1 表示允许/存在第 n 个逻辑核）。纯查询，不改变任何调度状态，失败返回 false。
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetProcessAffinityMask(IntPtr hProcess,
                                                          out UIntPtr lpProcessAffinityMask,
                                                          out UIntPtr lpSystemAffinityMask);

        /// <summary>
        /// 当前进程 OS 线程的只读快照：线程总数、CPU 时间合计、样例线程身份与等待原因。
        /// 字段经 <see cref="Build"/> 一次性填充后对外只读，线程安全地作为数据载体传递。
        /// </summary>
        public sealed class OsThreadSnapshot
        {
            /// <summary>仅供外层 <see cref="OsThreadSnapshot.Build"/> 构造，外部不能绕过工厂创建空快照。</summary>
            /// <param name="threadCount">线程总数。</param>
            /// <param name="totalTimeTicks">全部线程 TotalProcessorTime 的 Ticks 之和。</param>
            /// <param name="sampleThreadId">第一条线程的 OS 线程 Id。</param>
            /// <param name="sampleWaitReason">第一条线程的 WaitReason 名称（非等待状态为说明文本）。</param>
            private OsThreadSnapshot(int threadCount, long totalTimeTicks,
                                    int sampleThreadId, string sampleWaitReason)
            {
                ThreadCount = threadCount;
                TotalTimeTicks = totalTimeTicks;
                SampleThreadId = sampleThreadId;
                SampleWaitReason = sampleWaitReason;
            }

            /// <summary>快照时刻当前进程的 OS 线程总数（ProcessThreadCollection.Count 累计值）。</summary>
            /// <returns>线程数量。</returns>
            public int ThreadCount { get; private set; }

            /// <summary>全部线程 <see cref="ProcessThread.TotalProcessorTime"/> 的 Ticks 累加值
            /// （1 Tick = 100 纳秒；用户态与内核态之和）。</summary>
            /// <returns>CPU 时间合计滴答数。</returns>
            public long TotalTimeTicks { get; private set; }

            /// <summary>样例（第一条）线程的 OS 线程 Id；与托管 ManagedThreadId 无对应关系。</summary>
            /// <returns>OS 线程 ID（TID）。</returns>
            public int SampleThreadId { get; private set; }

            /// <summary>样例线程的 WaitReason 名称；非等待状态下为“状态名(非等待状态,WaitReason无意义)”。</summary>
            /// <returns>等待原因枚举名或兜底说明文本。</returns>
            public string SampleWaitReason { get; private set; }

            /// <summary>
            /// 嵌套类内部工厂：由外层 <see cref="SnapshotCurrentProcess"/> 调用，一次性固定全部字段。
            /// </summary>
            /// <param name="threadCount">线程总数。</param>
            /// <param name="totalTimeTicks">CPU 时间合计滴答数。</param>
            /// <param name="sampleThreadId">样例线程 OS Id。</param>
            /// <param name="sampleWaitReason">样例线程等待原因文本。</param>
            /// <returns>填充完毕、不可再改的快照实例。</returns>
            internal static OsThreadSnapshot Build(int threadCount, long totalTimeTicks,
                                                   int sampleThreadId, string sampleWaitReason)
            {
                return new OsThreadSnapshot(threadCount, totalTimeTicks, sampleThreadId, sampleWaitReason);
            }

            /// <summary>
            /// 多行可读表示（日志/自测断言用）。
            /// </summary>
            /// <returns>含线程数、CPU 时间 Ticks、样例 TID 与等待原因的格式化文本。</returns>
            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "OsThreadCount={0}, TotalProcessorTimeTicks={1}, SampleThreadId={2}, SampleWaitReason={3}",
                    ThreadCount, TotalTimeTicks, SampleThreadId, SampleWaitReason ?? string.Empty);
            }
        }
    }
}
