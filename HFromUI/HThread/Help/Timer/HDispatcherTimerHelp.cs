using System;
using System.Windows.Threading;

namespace HFromUI.HThread.Help.Timer
{
    /// <summary>
    /// DispatcherTimer 帮助类：WPF 的 Dispatcher（UI 线程）定时器。
    /// 【是什么】System.Windows.Threading.DispatcherTimer（WindowsBase.dll）不使用内核定时器回调，
    /// 而是在 Dispatcher 消息循环的指定 DispatcherPriority 位置排队：Interval 到点后，Tick 作为一个
    /// Dispatcher 作业入队，等 UI 线程处理到该优先级时执行。Tick 始终在 Dispatcher 所属线程（通常即 UI 线程）触发。
    /// 共 4 个构造重载：
    /// DispatcherTimer()（默认优先级 Background，绑定当前 Dispatcher）；
    /// DispatcherTimer(DispatcherPriority priority)；
    /// DispatcherTimer(DispatcherPriority priority, Dispatcher dispatcher)（可指定别的 UI 线程/ Dispatcher）；
    /// DispatcherTimer(TimeSpan interval, DispatcherPriority priority, EventHandler callback, Dispatcher dispatcher)（一次给齐）。
    /// 常用成员：Interval(TimeSpan)、IsEnabled、Start()/Stop()（等价 IsEnabled=true/false）、Tick 事件、
    /// Dispatcher 属性、Tag 属性。
    /// 【是否跨进程】否；跨线程也仅限同一进程内指定目标 Dispatcher。
    /// 【典型适用场景】WPF UI 周期刷新（时钟、动画节拍、轮询后直接改控件）；需要在回调里免 Invoke 操作 UI 的定时任务。
    /// 【使用步骤】
    /// 1) new DispatcherTimer(...)（4 个构造按需选）；2) 设 Interval、订阅 Tick；3) Start()；
    /// 4) 停止用 Stop()；一次性定时器在 Tick 内 Stop。
    /// 【注意事项与坑】
    /// - 不会重入：UI 线程单线程执行 Tick，上一拍没跑完，下一拍只能排队等待；
    /// - 不补拍：UI 线程忙期间错过多个间隔，恢复后只执行一次 Tick（官方说明：过期的 Tick 会被合并/不补发）；
    /// - 优先级语义：DispatcherPriority 数值越大越早执行，常用：Send=9（最高，同步发送）、Normal=8（多数代码默认）、
    ///   DataBind=7、Render=6、Loaded=5、Input=4、Background=3（DispatcherTimer【默认】）、ContextIdle=2、
    ///   ApplicationIdle=1、Inactive=-1、Invalid=0；用 Background/Input 等低优先级可让渲染、输入先行，避免卡 UI；
    /// - Start() 重新从零计时；Stop 后再 Start 间隔重置；运行中改 Interval 也会重新计时；
    /// - Interval 为负会抛 ArgumentOutOfRangeException；Interval=TimeSpan.Zero 表示尽可能频繁（不要这么写）；
    /// - 没有消息循环（非 UI 线程又不 Run）Tick 永远不会执行；在非 UI 线程 new 会绑定该线程的 Dispatcher（自动创建）；
    /// - Tick 中绝不能放长阻塞/重活，否则整个 UI 卡死；重活丢后台线程再 Dispatcher.Invoke 回来。
    /// 【版本可用性】.NET Framework 3.0+（含 net48，WindowsBase.dll）全部 4 构造与成员可用；
    /// .NET Core 3.0/.NET 5+ 的 WPF 同样可用。WinForms 无此类型，请用 System.Windows.Forms.Timer。
    /// </summary>
    /// <example>
    /// WPF 每秒刷新（Tick 内直接碰控件）：
    /// <code>
    /// DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
    /// timer.Interval = TimeSpan.FromSeconds(1);
    /// timer.Tick += delegate(object sender, EventArgs e)
    /// {
    ///     clockText.Text = DateTime.Now.ToString("HH:mm:ss");   // UI 线程：无需 Dispatcher.Invoke
    /// };
    /// timer.Start();
    /// </code>
    /// </example>
    public static class HDispatcherTimerHelp
    {
        /// <summary>
        /// 示例1：默认构造（DispatcherPriority.Background）创建定时器并启动，Tick 里直接更新控件。
        /// 只创建并返回，Tick 是否触发取决于该线程是否运行 Dispatcher 消息循环。
        /// </summary>
        /// <param name="interval">Tick 间隔（TimeSpan）；须为非负</param>
        /// <param name="onTick">Tick 事件处理程序（在 Dispatcher/UI 线程执行）</param>
        /// <returns>已 Start 的 DispatcherTimer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">interval 为负</exception>
        public static DispatcherTimer Create(TimeSpan interval, EventHandler onTick)
        {
            DispatcherTimer timer = new DispatcherTimer();      // 构造1：默认 Background 优先级、当前 Dispatcher
            timer.Interval = interval;
            timer.Tick += delegate (object sender, EventArgs e)
            {
                onTick(sender, e);   // 已在UI线程，无需 Invoke
            };
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例2：构造2 指定 DispatcherPriority（创建后【不自动启动】，由调用方 Start）。
        /// </summary>
        /// <param name="interval">Tick 间隔</param>
        /// <param name="onTick">Tick 处理程序</param>
        /// <returns>已配置但未启动的 DispatcherTimer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">interval 为负</exception>
        public static DispatcherTimer CreateBackground(TimeSpan interval, EventHandler onTick)
        {
            // 构造2：仅指定优先级（Background=3，让渲染/输入优先）
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = interval;
            timer.Tick += onTick;
            return timer;              // 不 Start：留给调用方控制
        }

        /// <summary>
        /// 示例3：构造2 指定任意优先级创建并启动；低优先级避免与渲染/输入争抢 UI 时间。
        /// </summary>
        /// <param name="interval">Tick 间隔</param>
        /// <param name="priority">Dispatcher 队列优先级（如 Background/Normal/Input）</param>
        /// <param name="onTick">Tick 处理程序</param>
        /// <returns>已 Start 的 DispatcherTimer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">interval 为负</exception>
        public static DispatcherTimer CreateWithPriority(TimeSpan interval, DispatcherPriority priority, EventHandler onTick)
        {
            // 构造2：显式优先级
            DispatcherTimer timer = new DispatcherTimer(priority);
            timer.Interval = interval;
            timer.Tick += onTick;
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例4：构造3 显式指定 Dispatcher（可挂到另一个 UI 线程的 Dispatcher 上），创建并启动。
        /// </summary>
        /// <param name="interval">Tick 间隔</param>
        /// <param name="priority">Dispatcher 队列优先级</param>
        /// <param name="dispatcher">目标 Dispatcher（null 等价构造2，这里要求显式传非 null）</param>
        /// <param name="onTick">Tick 处理程序（在 dispatcher 所属线程执行）</param>
        /// <returns>已 Start 的 DispatcherTimer</returns>
        /// <exception cref="ArgumentNullException">dispatcher 或 onTick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">interval 为负</exception>
        public static DispatcherTimer CreateOnDispatcher(
            TimeSpan interval, DispatcherPriority priority, Dispatcher dispatcher, EventHandler onTick)
        {
            // 构造3：优先级 + Dispatcher
            DispatcherTimer timer = new DispatcherTimer(priority, dispatcher);
            timer.Interval = interval;
            timer.Tick += onTick;
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例5：构造4 一次给齐间隔/优先级/Tick 回调/Dispatcher（创建后未启动，按需 Start）。
        /// </summary>
        /// <param name="interval">Tick 间隔</param>
        /// <param name="priority">Dispatcher 队列优先级</param>
        /// <param name="onTick">构造时直接登记的 Tick 处理程序（内部 += 到 Tick）</param>
        /// <param name="dispatcher">目标 Dispatcher</param>
        /// <returns>已配置但未启动的 DispatcherTimer</returns>
        /// <exception cref="ArgumentNullException">dispatcher 或 onTick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">interval 为负</exception>
        public static DispatcherTimer CreateFull(
            TimeSpan interval, DispatcherPriority priority, EventHandler onTick, Dispatcher dispatcher)
        {
            // 构造4：间隔、优先级、回调、Dispatcher 全部在构造时指定
            DispatcherTimer timer = new DispatcherTimer(interval, priority, onTick, dispatcher);
            return timer;              // 该构造不自动 Start
        }

        /// <summary>
        /// 示例6：一次性定时器（Tick 中先 Stop 再执行回调）。
        /// </summary>
        /// <param name="delay">延迟时间</param>
        /// <param name="onTick">只执行一次的 Tick 处理程序</param>
        /// <returns>已 Start 的 DispatcherTimer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">delay 为负</exception>
        public static DispatcherTimer OneShot(TimeSpan delay, EventHandler onTick)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = delay;
            timer.Tick += delegate (object sender, EventArgs e)
            {
                timer.Stop();          // 先停：保证只执行一次
                onTick(sender, e);
            };
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例7：统一启停控制（Start/Stop 等价于 IsEnabled=true/false）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="start">true=启动（重新从零计时）；false=停止</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static void StartStop(DispatcherTimer timer, bool start)
        {
            if (start)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        /// <summary>
        /// 示例8：读取定时器运行状态与归属线程信息（日志/诊断用）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <returns>含 IsEnabled、Interval、Dispatcher 线程 Id、当前线程是否其 UI 线程的描述字符串</returns>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static string Describe(DispatcherTimer timer)
        {
            // CheckAccess=true 表示当前代码正运行在该 Dispatcher 线程上
            bool onDispatcherThread = timer.Dispatcher.CheckAccess();
            int dispatcherThreadId = -1;
            if (onDispatcherThread)
            {
                // 只有在 Dispatcher 线程上才能安全访问其 Thread 属性
                dispatcherThreadId = timer.Dispatcher.Thread.ManagedThreadId;
            }
            return string.Format(
                "IsEnabled={0}, Interval={1}ms, DispatcherThreadId={2}, OnDispatcherThread={3}",
                timer.IsEnabled, timer.Interval.TotalMilliseconds, dispatcherThreadId, onDispatcherThread);
        }

        /// <summary>
        /// 示例9：生命周期自测演示：构造 → Start → Stop，立即返回，不依赖消息循环真正触发 Tick。
        /// 注意：在非 UI 线程调用会为当前线程创建 Dispatcher 对象，但不 Start 消息循环即无任何排队作业被执行，无副作用。
        /// </summary>
        /// <returns>启停后 Describe 的状态字符串（IsEnabled=False）</returns>
        public static string QuickLifecycleDemo()
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += delegate (object sender, EventArgs e) { };   // 空 Tick：即使触发也无操作
            timer.Start();
            timer.Stop();                  // 立即停：不等待 Tick
            return Describe(timer);
        }
    }
}
