using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HThread.Help.Diagnostics
{
    /// <summary>
    /// 【是什么】多线程/异步程序“全局未处理异常兜底”帮助类：把 .NET 提供的五类最后防线事件一次性讲清，
    /// 并提供可反复挂/摘（幂等、可精确还原）的成对方法。处理器内部只做 <see cref="Interlocked"/> 计数，
    /// 不弹窗、不写文件、绝不抛异常。五类事件按“离异常抛出点由近到远”分别为：
    /// 1) <see cref="AppDomain.FirstChanceException"/>：CLR 每次“抛出异常的瞬间”触发，发生在任何 catch
    ///    之前（连最终被正常吞掉的异常也算），参数 FirstChanceExceptionEventArgs 只有 Exception；
    ///    只能观察/计数，不能阻止异常继续传播，处理体内若再抛异常会造成递归通知甚至进程崩溃，因此绝不能抛；
    /// 2) <see cref="AppDomain.UnhandledException"/>：前台或后台线程上“最终无人捕获”的异常冒泡到顶层时触发，
    ///    参数 <see cref="UnhandledExceptionEventArgs"/> 有 ExceptionObject（装箱的 object，需自行转型）与
    ///    IsTerminating；该事件仅用于记录遗言/落盘日志，不能阻止崩溃——.NET Framework 2.0 起默认策略就是
    ///    终止进程（.NET 1.x 时代可以吞掉继续跑，已废弃）；
    /// 3) <see cref="TaskScheduler.UnobservedTaskException"/>：一个 Task 抛了异常却从未被 await/Wait/Result
    ///    观察，等到它被 GC 终结时触发；参数 <see cref="UnobservedTaskExceptionEventArgs"/> 有
    ///    Exception（<see cref="AggregateException"/>）与 SetObserved()；调用 SetObserved() 可阻止默认的
    ///    进程终止升级策略；注意 .NET Framework 4.5 起默认已不因此终止进程（事件仍会触发），如需在测试环境
    ///    强制其直接崩溃，在 app.config 加 &lt;runtime&gt;&lt;ThrowUnobservedTaskExceptions enabled="1"/&gt;/&lt;runtime&gt;；
    /// 4) <see cref="System.Windows.Forms.Application.ThreadException"/>：仅 WinForms UI 线程消息泵内
    ///    （按钮事件、Paint、Timer 回调等）未处理的异常会走这里；配合
    ///    <see cref="Application.SetUnhandledExceptionMode"/> 传
    ///    <see cref="UnhandledExceptionMode.CatchException"/> 才会路由到该事件；它只管 UI 线程，
    ///    工作线程/线程池线程的未捕获异常仍走 AppDomain.UnhandledException；
    /// 5) WPF 用 <see cref="System.Windows.Threading.Dispatcher.CurrentDispatcher"/> 的 UnhandledException
    ///    事件（类型 System.Windows.Threading.DispatcherUnhandledExceptionEventArgs，位于 WindowsBase 程序集），
    ///    仅 WPF Dispatcher 线程；把 e.Handled 置 true 可吞掉异常让应用继续（谨慎使用）。
    ///    注意 CurrentDispatcher 在当前线程没有 Dispatcher 时会“新建”一个，所以本类不在无窗口自测里碰它，
    ///    完整写法只放在 &lt;example&gt; 注释中。
    ///
    /// 【是否跨进程】否。全部是本进程（本 AppDomain/本消息泵）内的事件，与其他进程无关。
    ///
    /// 【典型适用场景】
    /// 1) 程序上线前统一兜底：异常计数、写临终日志、上报崩溃现场（UnhandledException 里只做极简、不会再失败的事）；
    /// 2) 自测/教学：FirstChanceException 计数观察“程序实际抛了多少次异常（含被 catch 的）”；
    /// 3) 排查“消失的 Task 异常”：观察 UnobservedTaskException 是否被触发，暴露遗漏 await 的火并忘任务；
    /// 4) WinForms 程序防止 UI 回调里一个小异常直接闪退：ThreadException 记录后让消息泵继续。
    ///
    /// 【使用步骤】Program.cs 推荐接线顺序（顺序很重要）：
    /// 1) 最先挂事件：AttachGlobalHandlers() 与 AttachWinFormsThreadException()——必须在创建第一个窗口、
    ///    启动业务线程之前，否则启动期的异常兜不住；
    /// 2) 再调用 Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
    ///    ——必须在第一个窗口句柄创建之前，之后调用可能抛 InvalidOperationException；
    /// 3) 最后才 Application.Run(主窗口)；
    /// 4) 退出时按相反顺序 Detach 还原（成对调用，测试程序尤其需要）；
    /// 5) Task 异常的正解不是等 UnobservedTaskException，而是在生产代码源头观察：
    ///    task.ContinueWith(日志处理, TaskContinuationOptions.OnlyOnFaulted) 或直接 await。
    ///
    /// 【注意事项与坑】
    /// 1) FirstChanceException 频率极高（程序里每个 try 内被正常处理的异常都会触发），回调体必须极轻量、
    ///    绝不加锁做 IO、绝不抛异常；本类只做 <see cref="Interlocked.Increment"/>；
    /// 2) UnhandledException 里无法取消崩溃：IsTerminating=true 时进程必死，回调应假设“随时被杀”，
    ///    只写最少的日志，不要弹窗等用户点确定（没人点就卡死）；
    /// 3) 事件挂摘必须用“同一个委托实例”：本类把委托保存为静态字段，Detach 时精确 -= 该字段，
    ///    避免匿名方法临时新建导致摘不掉；Attach/Detach 均幂等，重复 Attach 不会叠加处理器；
    /// 4) ThreadException 只覆盖 WinForms 消息泵内的异常；非 UI 线程异常、消息泵外的异常都不归它管；
    /// 5) WPF 与 WinForms 不要混用：WPF Dispatcher 与 WinForms Application 各管各的 UI 线程；
    /// 6) UnobservedTaskException 触发时机依赖 GC，时间不确定，不要把业务正确性寄托在它上面；
    ///    调 SetObserved() 等于声明“我认了”，会吞掉未观察异常的默认升级；
    /// 7) 配置 ThrowUnobservedTaskExceptions enabled=1 仅用于测试环境暴露问题，生产慎用。
    ///
    /// 【版本可用性】net48 全部可用：AppDomain 两事件（2.0+）、TaskScheduler.UnobservedTaskException
    /// （4.0+，4.5 改默认策略）、Application.ThreadException（1.1+）、WPF Dispatcher 事件（3.0+，WindowsBase）。
    /// </summary>
    /// <example>
    /// WinForms 程序 Program.cs 的推荐接线（先挂事件、再设异常模式、最后 Run；退出成对摘除）：
    /// <code>
    /// using System;
    /// using System.Threading.Tasks;
    /// using System.Windows.Forms;
    /// using HFromUI.HThread.Help.Diagnostics;
    ///
    /// internal static class Program
    /// {
    ///     [STAThread]
    ///     private static void Main()
    ///     {
    ///         // 第1步：在创建任何窗口、启动任何业务线程之前，先挂好全部兜底事件
    ///         HThreadExceptionHelp.AttachGlobalHandlers();
    ///         HThreadExceptionHelp.AttachWinFormsThreadException();
    ///
    ///         // 第2步：设置 WinForms 未处理异常路由模式（必须在第一个窗口句柄创建之前调用）
    ///         Application.EnableVisualStyles();
    ///         Application.SetCompatibleTextRenderingDefault(false);
    ///         Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    ///
    ///         // 第3步：最后启动消息泵；此后 UI 回调里的未处理异常才会进入 ThreadException
    ///         try
    ///         {
    ///             Application.Run(new FormMain());
    ///         }
    ///         finally
    ///         {
    ///             // 退出时按相反顺序成对摘除，还原全局事件状态
    ///             HThreadExceptionHelp.DetachWinFormsThreadException();
    ///             HThreadExceptionHelp.DetachGlobalHandlers();
    ///         }
    ///     }
    /// }
    ///
    /// // Task 异常的生产正解：在源头用 OnlyOnFaulted 观察，不要等 UnobservedTaskException：
    /// Task work = Task.Run(delegate { DoBackgroundWork(); });
    /// work.ContinueWith(delegate(Task t)
    /// {
    ///     LogError("后台任务失败", t.Exception);   // t.Exception 是 AggregateException
    /// }, TaskContinuationOptions.OnlyOnFaulted);
    ///
    /// // 若要在测试环境让“未观察的 Task 异常”直接崩溃，app.config 内容（XML 需转义）：
    /// // &lt;configuration&gt;
    /// //   &lt;runtime&gt;
    /// //     &lt;ThrowUnobservedTaskExceptions enabled="1"/&gt;
    /// //   &lt;/runtime&gt;
    /// // &lt;/configuration&gt;
    /// </code>
    /// </example>
    /// <example>
    /// WPF Dispatcher 未处理异常的完整写法（仅注释示例，不在无窗口自测中执行——
    /// Dispatcher.CurrentDispatcher 在当前线程没有 Dispatcher 时会新建一个）：
    /// <code>
    /// using System;
    /// using System.Windows;
    /// using System.Windows.Threading;
    ///
    /// public partial class App : Application
    /// {
    ///     protected override void OnStartup(StartupEventArgs e)
    ///     {
    ///         // WPF UI 线程（Dispatcher 线程）消息循环内未处理异常的最后防线
    ///         Dispatcher.CurrentDispatcher.UnhandledException +=
    ///             delegate(object sender, DispatcherUnhandledExceptionEventArgs args)
    ///             {
    ///                 // 只做记录等极轻量工作；绝不能在此再抛异常
    ///                 LogError("WPF Dispatcher 未处理异常", args.Exception);
    ///
    ///                 // Handled=true 表示“已处理”，吞掉异常让应用继续运行；
    ///                 // 状态可能已损坏，是否吞掉要按场景权衡，崩溃重启有时更安全
    ///                 args.Handled = true;
    ///             };
    ///
    ///         base.OnStartup(e);
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class HThreadExceptionHelp
    {
        /// <summary>挂/摘全局处理器与计数读取时共用的锁对象（操作极短，不做任何 IO）。</summary>
        private static readonly object SyncRoot = new object();

        // 以下委托必须保存为字段：Detach 时要用“同一个委托实例”精确 -=，否则匿名临时委托摘不掉
        private static UnhandledExceptionEventHandler _unhandledHandler;
        private static EventHandler<FirstChanceExceptionEventArgs> _firstChanceHandler;
        private static EventHandler<UnobservedTaskExceptionEventArgs> _unobservedTaskHandler;
        private static ThreadExceptionEventHandler _winFormsThreadHandler;

        // 各事件分别计数（处理器体内唯一允许做的事）；默认 0，GetObservedCount 不重置
        private static int _unhandledCount;
        private static int _firstChanceCount;
        private static int _unobservedTaskCount;
        private static int _winFormsThreadCount;

        // 两组事件各自的“已挂接”标志，保证 Attach/Detach 幂等
        private static bool _globalAttached;
        private static bool _winFormsAttached;

        /// <summary>
        /// 挂接三个进程级兜底事件：<see cref="AppDomain.UnhandledException"/>、
        /// <see cref="AppDomain.FirstChanceException"/>、<see cref="TaskScheduler.UnobservedTaskException"/>。
        /// 幂等：重复调用不会叠加处理器，已处于挂接状态时直接返回 false。
        /// </summary>
        /// <returns>true=本次调用实际完成了挂接；false=此前已挂接，本次为重复调用未做任何改动。</returns>
        public static bool AttachGlobalHandlers()
        {
            lock (SyncRoot)
            {
                if (_globalAttached)
                {
                    // 幂等：已挂过时什么都不做，避免处理器叠加导致一次异常多次计数
                    return false;
                }

                // 先把委托存入字段，再 += 字段（Detach 时才能精确 -= 同一个实例）
                _unhandledHandler = new UnhandledExceptionEventHandler(OnUnhandledException);
                _firstChanceHandler = new EventHandler<FirstChanceExceptionEventArgs>(OnFirstChanceException);
                _unobservedTaskHandler = new EventHandler<UnobservedTaskExceptionEventArgs>(OnUnobservedTaskException);

                AppDomain.CurrentDomain.UnhandledException += _unhandledHandler;
                AppDomain.CurrentDomain.FirstChanceException += _firstChanceHandler;
                TaskScheduler.UnobservedTaskException += _unobservedTaskHandler;

                _globalAttached = true;
                return true;
            }
        }

        /// <summary>
        /// 摘除 <see cref="AttachGlobalHandlers"/> 挂接的三个事件，并清空保存的委托字段，精确还原全局状态。
        /// 幂等：未挂接或已摘除时直接返回 false，绝不抛异常。
        /// </summary>
        /// <returns>true=本次调用实际完成了摘除；false=当前本就未挂接，调用无副作用。</returns>
        public static bool DetachGlobalHandlers()
        {
            lock (SyncRoot)
            {
                if (!_globalAttached)
                {
                    // 幂等：没挂过时直接返回，不触碰任何全局事件
                    return false;
                }

                // 用挂接时保存的同一委托实例精确 -=；-= 不存在的委托在 CLR 中也是安全无操作
                AppDomain.CurrentDomain.UnhandledException -= _unhandledHandler;
                AppDomain.CurrentDomain.FirstChanceException -= _firstChanceHandler;
                TaskScheduler.UnobservedTaskException -= _unobservedTaskHandler;

                // 清空字段与标志，保证之后可再次干净地 Attach
                _unhandledHandler = null;
                _firstChanceHandler = null;
                _unobservedTaskHandler = null;
                _globalAttached = false;
                return true;
            }
        }

        /// <summary>
        /// 挂接 WinForms UI 线程消息泵的 <see cref="System.Windows.Forms.Application.ThreadException"/> 事件。
        /// 只挂事件本身，不调用 <see cref="Application.SetUnhandledExceptionMode"/>
        /// （异常模式属于全局策略，应由 Program.cs 按推荐顺序显式设置）。幂等，重复调用不叠加。
        /// </summary>
        /// <returns>true=本次实际完成挂接；false=已挂接，重复调用无副作用。</returns>
        public static bool AttachWinFormsThreadException()
        {
            lock (SyncRoot)
            {
                if (_winFormsAttached)
                {
                    // 幂等：已挂过，避免叠加
                    return false;
                }

                // 静态事件在消息泵启动前挂接也是允许的；先存字段再 +=
                _winFormsThreadHandler = new ThreadExceptionEventHandler(OnWinFormsThreadException);
                Application.ThreadException += _winFormsThreadHandler;
                _winFormsAttached = true;
                return true;
            }
        }

        /// <summary>
        /// 摘除 <see cref="AttachWinFormsThreadException"/> 挂接的 WinForms 事件并清空委托字段。
        /// 幂等：未挂接时返回 false，不抛异常。
        /// </summary>
        /// <returns>true=本次实际完成摘除；false=当前本就未挂接。</returns>
        public static bool DetachWinFormsThreadException()
        {
            lock (SyncRoot)
            {
                if (!_winFormsAttached)
                {
                    // 幂等：无挂接记录时直接返回
                    return false;
                }

                // 同一委托实例精确 -=，保证全局事件完全还原
                Application.ThreadException -= _winFormsThreadHandler;
                _winFormsThreadHandler = null;
                _winFormsAttached = false;
                return true;
            }
        }

        /// <summary>
        /// 读取自本类挂接以来四类兜底事件处理器被触发的总次数（Unhandled + FirstChance
        /// + UnobservedTask + WinFormsThread）。只观察、不重置计数；为简单起见返回总计数，
        /// 不区分事件类型（FirstChance 通常占绝大多数）。
        /// </summary>
        /// <returns>四类事件累计触发次数之和（单调递增，调用本方法不会清零）。</returns>
        public static int GetObservedCount()
        {
            // 易失读取各计数后求和；计数本身由各处理器用 Interlocked 自增，保证不丢次数
            return Volatile.Read(ref _unhandledCount)
                 + Volatile.Read(ref _firstChanceCount)
                 + Volatile.Read(ref _unobservedTaskCount)
                 + Volatile.Read(ref _winFormsThreadCount);
        }

        /// <summary>
        /// AppDomain.UnhandledException 处理器：线程上最终无人捕获的异常冒泡到顶层时触发。
        /// 仅计数；这里无法阻止进程终止，也绝不弹窗或抛异常。
        /// </summary>
        /// <param name="sender">事件源（AppDomain）。</param>
        /// <param name="e">含 ExceptionObject（装箱异常）与 IsTerminating（是否即将终止进程）。</param>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 处理器唯一职责：计数。生产实现可在此做极简日志；禁止弹窗（崩溃时可能无人点击）
            Interlocked.Increment(ref _unhandledCount);
        }

        /// <summary>
        /// AppDomain.FirstChanceException 处理器：CLR 抛出异常的瞬间、任何 catch 之前触发。
        /// 频率极高，只能观察/计数；绝不能吞异常，处理体内绝不能再抛（否则递归通知）。
        /// </summary>
        /// <param name="sender">事件源（AppDomain）。</param>
        /// <param name="e">含刚被抛出的 Exception（含最终会被正常 catch 的异常）。</param>
        private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            // 只做一次原子自增：无锁、无 IO、无分支，保证高频调用下足够轻量且绝不抛异常
            Interlocked.Increment(ref _firstChanceCount);
        }

        /// <summary>
        /// TaskScheduler.UnobservedTaskException 处理器：未 await/Wait 的 Task 异常在 GC 终结时触发。
        /// 仅计数；生产代码如需吞掉默认升级，可在此调用 e.SetObserved()（本兜底处理器按约定只计数）。
        /// </summary>
        /// <param name="sender">事件源（TaskScheduler）。</param>
        /// <param name="e">含 Exception（AggregateException）；调 SetObserved() 可标记已观察。</param>
        private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // 计数即可；刻意不调 e.SetObserved()——是否“认下”未观察异常交给业务层决定
            Interlocked.Increment(ref _unobservedTaskCount);
        }

        /// <summary>
        /// WinForms Application.ThreadException 处理器：仅 UI 线程消息泵内的未处理异常会进入。
        /// 仅计数、绝不弹窗（弹窗本身走消息泵，可能造成递归）。
        /// </summary>
        /// <param name="sender">事件源（Application）。</param>
        /// <param name="e">含 UI 回调中未处理的 Exception。</param>
        private static void OnWinFormsThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // 生产实现可在此记录日志后让消息泵继续；自测处理器只计数，不做任何可能再抛的事
            Interlocked.Increment(ref _winFormsThreadCount);
        }
    }
}
