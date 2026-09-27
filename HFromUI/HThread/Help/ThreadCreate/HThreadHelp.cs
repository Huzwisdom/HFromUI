using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// Thread 帮助类：显式操作系统线程。
    /// 【是什么】System.Threading.Thread 是对操作系统线程的封装：new Thread(...) 会创建一个独占的、
    /// 不受 CLR 线程池管理的托管线程，Start 后执行 ThreadStart / ParameterizedThreadStart 委托。
    /// 每个 Thread 独占一个 OS 线程，创建/销毁开销大、不自动复用，但可控性最高（可命名、设优先级、
    /// 前后台、单元类型、Join 等待、拥有独立异常出口）。
    /// 【是否跨进程】否。线程是进程内的执行单元，同一进程内的线程共享地址空间、堆、句柄；
    /// 需要进程间隔离请用 Process（见 HProcessHelp）。
    /// 【典型适用场景】
    /// 1) 长生命周期、数量可控的后台服务线程（监听器、消息泵、专用消费线程）；
    /// 2) 需要 ThreadPriority / ApartmentState(STA) / Name / IsBackground 等精细控制的场合；
    /// 3) 会长时间阻塞（Wait/IO/锁）但又不想占住线程池线程的工作（也可用 TaskCreationOptions.LongRunning）；
    /// 4) 需要独立异常出口的关键任务（线程内未处理异常在 .NET 2.0+ 默认会终止进程）。
    /// 短任务、大批量并发应优先用 ThreadPool / Task / Parallel。
    /// 【使用步骤】
    /// 1) new Thread(ThreadStart 委托) 或 new Thread(ParameterizedThreadStart 委托)；
    /// 2) Start 之前设置 IsBackground / Name / Priority / ApartmentState（Start 之后这些属性不能再改）；
    /// 3) 调 Start() 或 Start(parameter) 真正创建并启动 OS 线程；
    /// 4) 用 Join(毫秒超时) 等待结束；用 ThreadState / IsAlive 判断运行状态；
    /// 5) 停止只能"协作式"：业务循环检查 volatile 标志或 CancellationToken，禁止强杀。
    /// 【注意事项与坑】
    /// - Name 只能赋值一次，重复赋值抛 InvalidOperationException；
    /// - ApartmentState 必须在 Start 之前 SetApartmentState，否则抛 InvalidOperationException；
    /// - Thread.Abort 在 net48 可用但已废弃：它向目标线程抛 ThreadAbortException，可能打断 finally/lock
    ///   造成状态损坏；在 .NET Core/.NET 5+ 直接抛 PlatformNotSupportedException，统一改用协作式取消；
    /// - Thread.Suspend/Resume 已废弃，绝不要使用；
    /// - 线程内未捕获异常自 .NET 2.0 起默认终止整个进程，线程体必须自行 try/catch；
    /// - 前台线程（IsBackground=false）会阻止进程退出；线程池线程一律是后台线程；
    /// - Join() 无超时重载可能无限等待，自测/库代码一律用带毫秒超时的 Join。
    /// 【版本可用性】net48 全量可用：Start、Join(3 个重载)、Sleep、Interrupt、Abort(废弃)、
    /// ThreadState、IsAlive、ManagedThreadId、IsThreadPoolThread、CurrentThread、Priority、
    /// GetApartmentState/SetApartmentState/TrySetApartmentState、Yield、SpinWait、MemoryBarrier、
    /// BeginThreadAffinity/EndThreadAffinity、GetDomain 等。
    /// .NET Core/.NET 5+ 不再支持 Abort / Suspend / Resume；本类不调用这些废弃 API，仅作文档说明。
    /// </summary>
    /// <example>
    /// 基本启动与协作式停止：
    /// <code>
    /// bool running = true;
    /// Thread t = new Thread(delegate()
    /// {
    ///     while (running)
    ///     {
    ///         Thread.Sleep(20);   // 业务循环
    ///     }
    /// }) { IsBackground = true, Name = "worker" };
    /// t.Start();
    /// running = false;   // 外部置标志，线程自行退出
    /// t.Join(1000);      // 带超时等待，绝不无限阻塞
    /// </code>
    /// </example>
    public static class HThreadHelp
    {
        /// <summary>示例1：启动无参后台线程（IsBackground=true 时主程序退出不等待该线程）</summary>
        /// <param name="work">线程执行体</param>
        /// <param name="name">线程名，在调试器"线程"窗口可直接辨认；可为 null</param>
        /// <returns>已经调用 Start 的 Thread 实例，调用方可继续 Join 或查状态</returns>
        /// <exception cref="InvalidOperationException">线程重复 Start 时抛出</exception>
        public static Thread StartBackground(Action work, string name = null)
        {
            Thread thread = new Thread(delegate () { work(); })
            {
                IsBackground = true,                 // 后台线程：不阻止进程退出
                Name = name ?? "HThreadHelp",        // 命名后可在调试器线程窗口直接辨认
                Priority = ThreadPriority.Normal
            };
            thread.Start();
            return thread;
        }

        /// <summary>示例2：通用启动：可指定前后台与优先级（属性必须在 Start 之前设置）</summary>
        /// <param name="work">线程执行体</param>
        /// <param name="name">线程名，可为 null</param>
        /// <param name="isBackground">true=后台线程（不阻止进程退出）；false=前台线程</param>
        /// <param name="priority">线程优先级，仅影响 OS 调度倾向，不保证执行顺序</param>
        /// <returns>已经调用 Start 的 Thread 实例</returns>
        public static Thread StartThread(Action work, string name, bool isBackground, ThreadPriority priority)
        {
            Thread thread = new Thread(delegate () { work(); })
            {
                IsBackground = isBackground,
                Name = name,
                Priority = priority
            };
            thread.Start();
            return thread;
        }

        /// <summary>示例3：启动 STA（单线程单元）线程；剪贴板、打开文件对话框、OLE 拖放等 COM 组件要求 STA</summary>
        /// <param name="work">线程执行体（内部通常还要跑消息循环，如 WinForms Application.Run）</param>
        /// <param name="name">线程名，可为 null</param>
        /// <returns>已经调用 Start 的 STA Thread 实例</returns>
        /// <exception cref="InvalidOperationException">Start 之后再设置单元状态时抛出（本方法在 Start 前设置，不会触发）</exception>
        public static Thread StartSta(Action work, string name = null)
        {
            Thread thread = new Thread(delegate () { work(); })
            {
                IsBackground = true,
                Name = name ?? "HThreadHelp.STA"
            };
            thread.SetApartmentState(ApartmentState.STA);   // 必须在 Start 之前设置
            thread.Start();
            return thread;
        }

        /// <summary>示例4：带参数启动线程（参数装箱为 object，即 ParameterizedThreadStart）</summary>
        /// <param name="work">参数化线程入口，收到的参数即 Start 传入的对象</param>
        /// <param name="parameter">传给线程的数据，可为 null</param>
        /// <param name="name">线程名，可为 null</param>
        /// <returns>已经调用 Start 的 Thread 实例</returns>
        public static Thread StartWithParameter(ParameterizedThreadStart work, object parameter, string name = null)
        {
            Thread thread = new Thread(work) { IsBackground = true, Name = name };
            thread.Start(parameter);                        // 参数装箱后在线程内拆箱使用
            return thread;
        }

        /// <summary>示例5：等待线程结束（阻塞调用线程，可设超时）；对应 Thread.Join(int) 重载</summary>
        /// <param name="thread">要等待的线程</param>
        /// <param name="timeoutMs">超时毫秒；默认 Timeout.Infinite 表示无限等待，自测建议显式传小超时</param>
        /// <returns>true=线程已结束；false=等待超时线程仍在运行</returns>
        /// <exception cref="ArgumentNullException">thread 为 null 时抛出</exception>
        public static bool WaitExit(Thread thread, int timeoutMs = Timeout.Infinite)
        {
            return thread.Join(timeoutMs);
        }

        /// <summary>示例6：在总超时预算内依次 Join 多个线程（对应 Thread.Join 各重载的组合用法）</summary>
        /// <param name="threads">要等待的线程集合</param>
        /// <param name="timeoutMs">全部线程的总超时预算（毫秒），不是每个线程的超时</param>
        /// <returns>true=全部线程在预算内结束；false=至少一个未结束</returns>
        public static bool JoinAll(IEnumerable<Thread> threads, int timeoutMs)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();      // 用秒表给所有 Join 共享一个总预算
            foreach (Thread thread in threads)
            {
                int remaining = timeoutMs - (int)stopwatch.ElapsedMilliseconds;
                if (remaining < 0)
                {
                    remaining = 0;
                }
                if (!thread.Join(remaining))                 // 单个超时则整体失败
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>示例7：读取线程最常用的状态成员：Name/ManagedThreadId/ThreadState/IsAlive/IsBackground/IsThreadPoolThread/Priority/ApartmentState</summary>
        /// <param name="thread">要描述的线程</param>
        /// <returns>各状态属性拼成的可读字符串</returns>
        public static string Describe(Thread thread)
        {
            return "Name=" + thread.Name
                + ", ManagedThreadId=" + thread.ManagedThreadId
                + ", ThreadState=" + thread.ThreadState          // Running/Background/WaitSleepJoin 等标志位组合
                + ", IsAlive=" + thread.IsAlive
                + ", IsBackground=" + thread.IsBackground
                + ", IsThreadPoolThread=" + thread.IsThreadPoolThread
                + ", Priority=" + thread.Priority
                + ", ApartmentState=" + thread.GetApartmentState();
        }

        /// <summary>示例8：Interrupt 唤醒处于 WaitSleepJoin（Sleep/Wait/Join）的线程，目标线程会抛 ThreadInterruptedException</summary>
        /// <remarks>Interrupt 只对正在阻塞的线程生效；若线程没在阻塞，异常会被"记下"，等它下次阻塞时再抛。
        /// 它比 Abort 安全（异常点明确），但仍需目标线程配合 catch，优先使用协作式取消。</remarks>
        public static void InterruptSleepingExample()
        {
            Thread worker = new Thread(delegate ()
            {
                try
                {
                    Thread.Sleep(5000);                 // 处于 WaitSleepJoin，可被 Interrupt 打断
                }
                catch (ThreadInterruptedException)
                {
                    // 被外部 Interrupt 唤醒：这里做清理后正常退出
                }
            }) { IsBackground = true, Name = "HThreadHelp.Interrupt" };
            worker.Start();
            worker.Interrupt();                         // 对阻塞中的线程注入 ThreadInterruptedException
            worker.Join(1000);                          // 带超时等待，自测不会卡死
        }

        /// <summary>示例9：线程内异常隔离包装；线程体未捕获异常会终止进程，必须在边界 try/catch</summary>
        /// <param name="work">业务执行体</param>
        /// <param name="onError">捕获到异常时的回调（如记录日志），可为 null</param>
        /// <param name="name">线程名，可为 null</param>
        /// <returns>已经调用 Start 的 Thread 实例</returns>
        public static Thread StartSafe(Action work, Action<Exception> onError, string name = null)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    work();
                }
                catch (Exception ex)
                {
                    // 线程内未处理异常自 .NET 2.0 起默认终止进程，必须在边界吞掉或上报
                    onError?.Invoke(ex);
                }
            }) { IsBackground = true, Name = name };
            thread.Start();
            return thread;
        }

        /// <summary>示例10：让出 CPU 的几种方式对比：Yield / Sleep(0) / Sleep(1) / SpinWait / MemoryBarrier</summary>
        /// <remarks>
        /// Thread.Yield：让出当前时间片给同核上就绪的线程，对方没就绪则立即返回；
        /// Thread.Sleep(0)：让出给同优先级线程（Windows 上可能立即继续）；
        /// Thread.Sleep(1)：至少让出到下次时钟中断（默认约 15ms）；
        /// Thread.SpinWait：用户态自旋几十到几百纳秒，不发生上下文切换，适合极短等待；
        /// Thread.MemoryBarrier：不等待任何线程，只阻止 CPU 指令重排（内存栅栏）。
        /// </remarks>
        public static void YieldSleepSpinExample()
        {
            Thread.Yield();          // 让出时间片
            Thread.Sleep(0);         // 同优先级让步
            Thread.Sleep(1);         // 至少等一个时钟滴答
            Thread.SpinWait(100);    // 用户态自旋 100 次
            Thread.MemoryBarrier();  // 全栅栏：刷新读写顺序
        }

        /// <summary>示例11：协作式停止：用自定义停止标志让线程自行退出（不要用已废弃的 Thread.Abort 强杀）</summary>
        public static void CooperativeStopExample()
        {
            bool running = true;
            Thread thread = new Thread(delegate ()
            {
                while (running)
                {
                    Thread.Sleep(20);    // 业务循环
                }
            }) { IsBackground = true };
            thread.Start();
            running = false;             // 外部置标志，线程下次循环自行退出
            thread.Join(1000);           // 带超时等待，避免无限阻塞
        }
    }
}
