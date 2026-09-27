using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// 【是什么】COM 套间（Apartment）模型帮助类：说明 <see cref="ApartmentState"/> 枚举
    /// （<see cref="ApartmentState.STA"/> 单线程套间、<see cref="ApartmentState.MTA"/> 多线程套间、
    /// <see cref="ApartmentState.Unknown"/> 尚未设置），以及如何在显式 Thread 上用
    /// <see cref="Thread.SetApartmentState"/>/<see cref="Thread.TrySetApartmentState"/> 在 Start 前配置套间、
    /// Start 后用 <see cref="Thread.GetApartmentState"/> 只读查询。套间是 COM 为对象并发访问定义的线程约定：
    /// STA 把 COM 对象串行化给唯一属主线程并靠 Windows 消息泵封送调用，MTA 允许 COM 对象被多线程并发访问。
    ///
    /// 【是否跨进程】否。套间是单进程内线程与 COM 运行时之间的约定；跨进程 COM 连接由封送/代理另外处理。
    ///
    /// 【典型适用场景】
    /// 1) UI 主线程：Main 入口标 [STAThread]，因为 OLE 拖放、剪贴板、OpenFileDialog 等文件对话框、
    ///    WebBrowser、部分 ActiveX/COM 组件明确要求运行在 STA 线程；
    /// 2) 新建专用线程承载老式 COM/ActiveX 组件时，Start 前设置 STA 并在线程内跑消息循环；
    /// 3) 调用标记为 MTA 的 COM 组件或做并发 COM 互操作时，使用 MTA（线程池默认即 MTA）。
    ///
    /// 【使用步骤】
    /// 1) new Thread 后、Start 之前调用 SetApartmentState（失败抛异常）或 TrySetApartmentState（返回 bool）；
    /// 2) STA 线程内部必须运行消息泵（WinForms 用 Application.Run，WPF 用 Dispatcher 框架），
    ///    否则其他套间对该线程 COM 对象的封送调用会永久排队、死等；
    /// 3) Start 之后只能 GetApartmentState 查询；老的 Thread.ApartmentState 属性已 Obsolete，不要使用；
    /// 4) 线程池线程与 Task 线程默认 MTA 且不能更改；需要 STA 只能自建 Thread。
    ///
    /// 【注意事项与坑】
    /// 1) SetApartmentState/TrySetApartmentState 只能在线程 Start 之前调用：对已启动（哪怕已结束）的线程
    ///    设置会抛 <see cref="ThreadStateException"/>（net48 实测；套间已初始化的情形文档另记载
    ///    <see cref="InvalidOperationException"/>），TrySet 版本此时同样抛异常而非返回 false；
    /// 2) 套间状态只能设置一次，重复设置也会抛 <see cref="InvalidOperationException"/>；
    /// 3) “设成 STA”不等于“UI 线程”：没有消息泵的 STA 线程收到跨套间 COM 调用会死等；
    /// 4) [STAThread] 只影响进程主线程的默认套间，不会改变线程池线程（仍是 MTA）；
    /// 5) GetApartmentState 在尚未设置的线程上返回 <see cref="ApartmentState.Unknown"/>。
    ///
    /// 【版本可用性】net48 全量可用（mscorlib：枚举、Get/Set/TrySetApartmentState）；
    /// .NET Core/.NET 5+ 同样保留这些 API；但注意 .NET Core 上非 Windows 平台没有 COM，套间设置无实际效果。
    /// </summary>
    /// <example>
    /// UI 主线程的标准写法与自建 STA 线程：
    /// <code>
    /// [STAThread]   // 剪贴板、OLE 拖放、OpenFileDialog 等要求主线程为 STA
    /// private static void Main()
    /// {
    ///     Application.EnableVisualStyles();
    ///     Application.Run(new MainForm());   // STA 必须有消息泵
    /// }
    ///
    /// Thread sta = new Thread(delegate()
    /// {
    ///     // 在该线程内使用要求 STA 的 COM 组件，并运行消息循环
    ///     Application.Run();
    /// }) { IsBackground = true };
    /// sta.TrySetApartmentState(ApartmentState.STA);   // 必须在 Start 之前
    /// sta.Start();
    /// </code>
    /// </example>
    public static class HApartmentStateHelp
    {
        /// <summary>
        /// 真实演示“Start 之前配置 STA”：新建工作线程，调用
        /// <see cref="Thread.TrySetApartmentState"/> 设置 <see cref="ApartmentState.STA"/>（返回 true），
        /// Start 后线程内用 <see cref="Thread.GetApartmentState"/> 回读自身套间状态。
        /// 静态方法用局部数组承载跨线程结果；全部等待带超时，2 秒内结束。
        /// </summary>
        /// <returns>
        /// TrySetApartmentState 返回 true、线程在 2000ms 内结束、且线程内回读到的套间为
        /// <see cref="ApartmentState.STA"/> 时返回 true；否则 false。
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// 套间已初始化时可能抛出（本方法只设置一次且在 Start 前，不会触发）。
        /// </exception>
        /// <exception cref="ThreadStateException">
        /// 若在 Start 之后调用 TrySetApartmentState，net48 运行时抛出（本方法在 Start 前调用，不会触发）。
        /// </exception>
        public static bool ConfigureStaBeforeStart()
        {
            ApartmentState[] result = new ApartmentState[1];   // 局部数组承载子线程回读结果

            Thread staThread = new Thread((ThreadStart)delegate
            {
                // 线程内只读查询当前套间；本演示不跑消息泵，故不做任何真实 COM 调用
                result[0] = Thread.CurrentThread.GetApartmentState();
            }) { IsBackground = true, Name = "HApartmentStateHelp.STA" };

            bool setOk = staThread.TrySetApartmentState(ApartmentState.STA);  // 必须在 Start 之前
            staThread.Start();

            bool joined = staThread.Join(2000);   // 带超时回收
            return setOk && joined && result[0] == ApartmentState.STA;
        }

        /// <summary>
        /// 真实演示“线程池线程默认 MTA”：向 <see cref="ThreadPool"/> 排入一个工作项，
        /// 在线程池线程内用 <see cref="Thread.GetApartmentState"/> 回读套间状态并用
        /// <see cref="ManualResetEventSlim"/> 回报。线程池/Task 线程一律是 MTA 且无法更改。
        /// </summary>
        /// <returns>工作项在 2000ms 内完成、且线程池线程套间为 <see cref="ApartmentState.MTA"/> 时返回 true。</returns>
        public static bool PoolThreadIsMtaDemo()
        {
            using (ManualResetEventSlim done = new ManualResetEventSlim(false))
            {
                ApartmentState[] state = new ApartmentState[1];   // 跨线程承载读到的套间状态

                ThreadPool.QueueUserWorkItem(delegate (object unused)
                {
                    // 线程池线程始终是 MTA，即使进程主线程标了 [STAThread]
                    state[0] = Thread.CurrentThread.GetApartmentState();
                    done.Set();
                }, null);

                bool signaled = done.Wait(2000);
                return signaled && state[0] == ApartmentState.MTA;
            }
        }

        /// <summary>
        /// 真实演示“Start 之后拒绝设置套间”：先启动一个短暂休眠的工作线程并 Join 等其结束，
        /// 再对这个已经启动过（当前已结束）的线程调用 <see cref="Thread.TrySetApartmentState"/>。
        /// net48 实测运行时抛 <see cref="ThreadStateException"/>（“线程对当前操作无效”）；
        /// 套间已初始化等情形文档另记载 <see cref="InvalidOperationException"/>，两种都视为符合契约。
        /// </summary>
        /// <returns>线程在 2000ms 内结束，且事后设置套间确实抛出 <see cref="ThreadStateException"/>
        /// 或 <see cref="InvalidOperationException"/> 时返回 true。</returns>
        public static bool RejectSetAfterStartDemo()
        {
            Thread worker = new Thread((ThreadStart)delegate
            {
                Thread.Sleep(200);    // 短暂存活，制造“已 Start”的事实
            }) { IsBackground = true, Name = "HApartmentStateHelp.Reject" };

            worker.Start();
            bool joined = worker.Join(2000);
            if (!joined)
            {
                return false;         // 回收超时，后续演示无意义
            }

            try
            {
                // 线程一旦 Start（即使已结束），套间状态即锁定：
                // net48 实测抛 ThreadStateException；套间已初始化的情形抛 InvalidOperationException
                worker.TrySetApartmentState(ApartmentState.STA);
                return false;         // 未抛异常与运行时契约不符
            }
            catch (InvalidOperationException)
            {
                // 符合契约：套间已初始化，不允许再设置
                return true;
            }
            catch (ThreadStateException)
            {
                // 符合契约（net48 实测路径）：线程状态不允许 Start 后再设置套间
                return true;
            }
        }
    }
}
