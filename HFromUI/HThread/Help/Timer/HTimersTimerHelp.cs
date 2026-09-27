using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using TimersTimer = System.Timers.Timer;

namespace HFromUI.HThread.Help.Timer
{
    using HFromUI.HLangage;
    /// <summary>
    /// System.Timers.Timer 帮助类：基于事件的组件式"服务器定时器"。
    /// 【是什么】System.Timers.Timer（System.dll）继承 System.ComponentModel.Component，是对
    /// System.Threading.Timer 的组件化包装：以 Elapsed 事件替代回调委托，提供 Interval/AutoReset/Enabled
    /// 等属性，可拖入设计器、可放入 IContainer 随宿主一起释放。net48 只有两个构造：Timer()（Interval 默认 100ms）
    /// 与 Timer(double interval)；Timer(TimeSpan) 构造是高版本 API，net48【不可用】（见类末"注释-only"清单）。
    /// 【是否跨进程】否。SynchronizingObject 为 null 时 Elapsed 在 CLR 线程池线程触发；
    /// 绑定到实现了 ISynchronizeInvoke 的 UI 对象（WinForms 的 Form/Control）后，Elapsed 被封送回该对象所在线程，
    /// 回调中可直接更新控件（本质是 ISynchronizeInvoke.BeginInvoke/Invoke 投递消息）。
    /// 【典型适用场景】服务端/组件中的周期批处理、需要设计器支持的定时任务、希望一行绑定就把回调切回 WinForms UI 线程；
    /// WPF 请直接用 DispatcherTimer（Timers.Timer 的 SynchronizingObject 不认识 DispatcherObject）。
    /// 【使用步骤】
    /// 1) new TimersTimer(intervalMs) 或 new TimersTimer() 后设 Interval（单位毫秒，double）；
    /// 2) AutoReset=true（默认）周期触发；false 只触发一次（触发后 Enabled 自动变 false）；
    /// 3) 订阅 Elapsed；Start() 或 Enabled=true 启动；Stop() 或 Enabled=false 停止；
    /// 4) 回 UI 线程：timer.SynchronizingObject = 某 Form/Control；
    /// 5) 设计器/批量化初始化用 BeginInit/EndInit 包住属性设置；结束用 Close()（等价 Dispose）。
    /// 【注意事项与坑】
    /// - Elapsed 仍可能并发重入：底层是线程池定时器，上一拍没跑完下一拍到点会在另一线程触发；必须自行防重入；
    /// - Stop/Close 返回后仍可能有一次在途 Elapsed 正在执行；回调内引用的资源要考虑"释放后仍回调一次"；
    /// - 同步（非 async void）Elapsed 处理程序中抛出的异常会被 Timer 捕获并【吞掉】（文档明确：未来版本可能改变），
    ///   不会崩进程但也无人知晓——必须自己 try/catch；async void 处理器中的异常不被吞，会直接导致进程崩溃；
    /// - Interval 必须 &gt;0，否则 ArgumentException；运行中改 Interval 会按新间隔重新计时；
    /// - Enabled=true 与 Start() 完全等价；AutoReset=false 触发一次后 Enabled 自动复位 false；
    /// - 分辨率同系统时钟（约 15ms，GetTickCount 同源，不受 timeBeginPeriod 影响）；
    /// - ElapsedEventArgs.SignalTime 是"理论触发时刻"，不等于回调实际开始执行的时刻；
    /// - 绑定 SynchronizingObject 后若 UI 线程阻塞/句柄未创建/窗体已释放，封送可能失败或延迟。
    /// 【版本可用性】net48 可用：Timer()/Timer(double)、Interval、AutoReset、Enabled、SynchronizingObject、
    /// Start、Stop、Close、BeginInit/EndInit、Elapsed。高版本才有（net48 仅注释说明，勿在本工程调用）：
    /// Timer(TimeSpan) 构造（.NET Core 2.0+/ .NET Standard 2.1）、Enabled 与 Active 命名演进等。
    /// </summary>
    /// <example>
    /// 绑定 UI 线程触发（WinForms）：
    /// <code>
    /// System.Timers.Timer t = new System.Timers.Timer(1000);
    /// t.AutoReset = true;
    /// t.SynchronizingObject = this;          // this 是 Form/Control：Elapsed 自动回 UI 线程
    /// t.Elapsed += delegate(object sender, ElapsedEventArgs e)
    /// {
    ///     label1.Text = e.SignalTime.ToString("HH:mm:ss");   // 无需 Invoke
    /// };
    /// t.Start();
    /// // ……
    /// t.Stop();
    /// t.Close();                             // 等价 Dispose
    /// </code>
    /// </example>
    public static class HTimersTimerHelp
    {
        /// <summary>
        /// 示例1：创建周期定时器并订阅 Elapsed（int/double 毫秒间隔），内部置 AutoReset=true 并启动。
        /// 只创建并返回，不等待事件触发；调用方负责 Close/Dispose。
        /// </summary>
        /// <param name="intervalMs">间隔毫秒数（double）；必须 &gt;0，否则 ArgumentException</param>
        /// <param name="onTick">Elapsed 事件处理程序（默认在线程池线程触发，可能重入）</param>
        /// <returns>已启动的 System.Timers.Timer</returns>
        /// <exception cref="ArgumentException">intervalMs 小于等于 0</exception>
        /// <exception cref="ArgumentNullException">onTick 为 null（事件订阅处抛出）</exception>
        public static TimersTimer CreateTimer(double intervalMs, ElapsedEventHandler onTick)
        {
            TimersTimer timer = new TimersTimer(intervalMs);   // Timer(double) 构造
            timer.AutoReset = true;        // true=周期；false=只响一次
            timer.Elapsed += onTick;       // 订阅 Elapsed
            timer.Enabled = true;          // 等价 Start()
            return timer;
        }

        /// <summary>
        /// 示例2：用无参构造创建定时器（Interval 默认 100ms），默认不启动，由调用方自行配置后 Start。
        /// </summary>
        /// <returns>Interval=100ms、Enabled=false 的 System.Timers.Timer</returns>
        public static TimersTimer CreateDefault()
        {
            // Timer() 构造：所有属性取默认值（Interval=100, AutoReset=true, Enabled=false）
            return new TimersTimer();
        }

        /// <summary>
        /// 示例3：创建一次性定时器（AutoReset=false）：首次 Elapsed 后自动停止，Enabled 自动复位 false。
        /// </summary>
        /// <param name="intervalMs">触发延迟（毫秒）；必须 &gt;0</param>
        /// <param name="onTick">只接收一次的 Elapsed 处理程序</param>
        /// <returns>已启动的一次性 System.Timers.Timer</returns>
        /// <exception cref="ArgumentException">intervalMs 小于等于 0</exception>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        public static TimersTimer CreateOneShot(double intervalMs, ElapsedEventHandler onTick)
        {
            TimersTimer timer = new TimersTimer(intervalMs);
            timer.AutoReset = false;       // 只触发一次
            timer.Elapsed += onTick;
            timer.Start();                 // 一次性也需要显式启动
            return timer;
        }

        /// <summary>
        /// 示例4：生成防重入的 Elapsed 处理程序（无参工作版）：重入发生时直接跳过这一拍。
        /// </summary>
        /// <param name="work">每拍工作；上一拍未结束时新来的拍次被跳过</param>
        /// <returns>可直接 += 到 Elapsed 的事件处理程序</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        public static ElapsedEventHandler NonReentrantHandler(Action work)
        {
            int running = 0;               // 门闩：0=空闲 1=有一拍在跑
            return delegate (object sender, ElapsedEventArgs e)
            {
                if (Interlocked.Exchange(ref running, 1) == 1)
                {
                    return;                // 上一拍还没跑完，跳过
                }
                try
                {
                    work();                // e.SignalTime 是理论触发时刻
                }
                finally
                {
                    Interlocked.Exchange(ref running, 0);
                }
            };
        }

        /// <summary>
        /// 示例5：生成防重入处理程序（带 ElapsedEventArgs 版），工作体内可读取 SignalTime。
        /// </summary>
        /// <param name="work">每拍工作，入参为 Elapsed 事件参数</param>
        /// <returns>可直接 += 到 Elapsed 的事件处理程序</returns>
        /// <exception cref="ArgumentNullException">work 为 null</exception>
        public static ElapsedEventHandler NonReentrantHandler(Action<ElapsedEventArgs> work)
        {
            int running = 0;
            return delegate (object sender, ElapsedEventArgs e)
            {
                if (Interlocked.Exchange(ref running, 1) == 1)
                {
                    return;                // 跳过重叠拍
                }
                try
                {
                    work(e);               // 可使用 e.SignalTime
                }
                finally
                {
                    Interlocked.Exchange(ref running, 0);
                }
            };
        }

        /// <summary>
        /// 示例6：停止 → 改间隔 → 按要求启停（运行中修改 Interval 会立即按新值重新计时）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="enabled">true=改完后启动；false=保持停止</param>
        /// <param name="intervalMs">新间隔（毫秒）；必须 &gt;0</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ArgumentException">intervalMs 小于等于 0</exception>
        /// <exception cref="ObjectDisposedException">timer 已 Close/Dispose</exception>
        public static void Control(TimersTimer timer, bool enabled, double intervalMs)
        {
            timer.Stop();                  // 先停，避免改配置期间插入一拍
            timer.Interval = intervalMs;
            timer.Enabled = enabled;       // 与 Start/Stop 等价
        }

        /// <summary>
        /// 示例7：显式 Start（等价 Enabled=true）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ObjectDisposedException">timer 已释放</exception>
        public static void Start(TimersTimer timer)
        {
            timer.Start();
        }

        /// <summary>
        /// 示例8：显式 Stop（等价 Enabled=false）；返回后可能仍有一次在途回调。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ObjectDisposedException">timer 已释放</exception>
        public static void Stop(TimersTimer timer)
        {
            timer.Stop();
        }

        /// <summary>
        /// 示例9：Close() 释放定时器（Component 模式，等价 Dispose）；释放后再访问属性抛 ObjectDisposedException。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static void Close(TimersTimer timer)
        {
            // Close 是组件风格命名，内部即 Dispose
            timer.Close();
        }

        /// <summary>
        /// 示例10：绑定 UI 封送对象：设置后 Elapsed 将通过 ISynchronizeInvoke 封送回该对象所在线程。
        /// WinForms 的 Form/Control 均实现 ISynchronizeInvoke；传 null 可恢复为线程池线程触发。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="synchronizingObject">UI 封送对象（通常是 Form/Control 实例）；null=线程池触发</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        /// <exception cref="ObjectDisposedException">timer 已释放</exception>
        public static void BindToUi(TimersTimer timer, ISynchronizeInvoke synchronizingObject)
        {
            // 内部在每次 Elapsed 时调用其 BeginInvoke/Invoke 投递，故回调能直接操作控件
            timer.SynchronizingObject = synchronizingObject;
        }

        /// <summary>
        /// 示例11：BeginInit/EndInit 成对包裹属性赋值（ISupportInitialize 设计器模式）：
        /// 初始化期间对 Interval/Enabled 等的修改被挂起，EndInit 时一次性生效（Enabled=true 则此刻启动）。
        /// </summary>
        /// <param name="intervalMs">间隔（毫秒）；必须 &gt;0</param>
        /// <param name="autoReset">true=周期；false=一次性</param>
        /// <param name="enabled">EndInit 后是否立即启动</param>
        /// <returns>初始化完成的 System.Timers.Timer</returns>
        /// <exception cref="ArgumentException">intervalMs 小于等于 0（在 EndInit 生效时抛出）</exception>
        public static TimersTimer ConfigureViaInit(double intervalMs, bool autoReset, bool enabled)
        {
            TimersTimer timer = new TimersTimer();
            timer.BeginInit();             // 进入初始化模式：暂存属性、不启动
            timer.AutoReset = autoReset;
            timer.Interval = intervalMs;
            timer.Enabled = enabled;       // 初始化期间置 Enabled 不会立即 Start
            timer.EndInit();               // 提交：enabled=true 时在此处统一启动
            return timer;
        }

        /// <summary>
        /// 示例12：生命周期自测演示：创建 → BeginInit/EndInit 配置 → Start → Stop → Close，
        /// 立即执行完毕，不订阅耗时 Elapsed、不依赖消息循环。
        /// </summary>
        /// <returns>固定的完成描述字符串</returns>
        public static string QuickLifecycleDemo()
        {
            TimersTimer timer = ConfigureViaInit(100d, true, false);
            timer.Start();                 // 等价 Enabled=true
            timer.Stop();                  // 立即停：不保证/不等待 Elapsed 触发
            timer.Close();                 // 释放
            return HTranslation.GetContent("TimersTimer BeginInit/EndInit/Start/Stop/Close 完成");
        }

        /// <summary>
        /// Elapsed 触发快照示例：把一次 Elapsed 的关键信息固化为普通数据对象（可跨线程安全传递/记录日志）。
        /// </summary>
        public sealed class ElapsedSnapshot
        {
            /// <summary>
            /// 从 Elapsed 事件参数构造快照。
            /// </summary>
            /// <param name="e">Elapsed 事件参数</param>
            public ElapsedSnapshot(ElapsedEventArgs e)
            {
                // SignalTime 为本地时间；同时记录 UTC 版本便于跨时区日志对齐
                SignalTime = e.SignalTime;
                SignalTimeUtc = e.SignalTime.ToUniversalTime();
            }

            /// <summary>
            /// 理论触发时刻（本地时间）。
            /// </summary>
            /// <returns>ElapsedEventArgs.SignalTime 原值</returns>
            public DateTime SignalTime { get; private set; }

            /// <summary>
            /// 理论触发时刻（UTC）。
            /// </summary>
            /// <returns>SignalTime 转换后的 UTC 时间</returns>
            public DateTime SignalTimeUtc { get; private set; }
        }
    }
}
