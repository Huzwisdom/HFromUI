using System;
using System.ComponentModel;
using System.Windows.Forms;
using FormsTimer = System.Windows.Forms.Timer;

namespace HFromUI.HThread.Help.Timer
{
    using HFromUI.HLangage;
    /// <summary>
    /// System.Windows.Forms.Timer 帮助类：WinForms 消息循环定时器。
    /// 【是什么】System.Windows.Forms.Timer（System.Windows.Forms.dll，继承 Component）是对 Win32 多媒体/窗口计时器的
    /// 纯 WinForms 封装：Start 后通过 SetTimer 在【创建它的线程】的消息队列投递 WM_TIMER 消息，
    /// 窗体消息泵取出 WM_TIMER 时在该线程（即 UI 线程）调用受保护虚方法 OnTick，OnTick 再引发 Tick 事件。
    /// 构造 2 个：Timer() 与 Timer(IContainer container)（加入容器，随容器一并释放，设计器使用）。
    /// 常用成员：Interval(int 毫秒，默认 100)、Enabled、Start()/Stop()（等价 Enabled=true/false）、
    /// Tick 事件（EventHandler）、protected virtual void OnTick(EventArgs)、Close()。
    /// 【是否跨进程】否；且跨线程能力为零——计时器绑定创建线程的消息队列，只能由该 UI 线程操作控件。
    /// 【典型适用场景】UI 刷新（时钟、闪烁、动画）、轻量轮询、需要"绝不重入且天然在 UI 线程"的短时任务。
    /// 【使用步骤】
    /// 1) new FormsTimer() 或 new FormsTimer(container)；2) 设 Interval(int 毫秒)、订阅 Tick；
    /// 3) Start()；4) 一次性在 Tick 内 Stop；5) 不用时 Stop+Dispose（加入 IContainer 可随宿主释放）。
    /// 【注意事项与坑】
    /// - WM_TIMER：本质是消息不是中断，UI 线程不泵送消息（模态阻塞、长计算、消息泵未运行）时，Tick 延迟甚至被丢弃；
    /// - 精度约 15ms：与系统时钟/GetTickCount 同分辨率，Interval 小于约 15ms 也按 ~15ms 走，不能用于精确计时；
    /// - 绝不重入：WM_TIMER 是低优先级消息且只在 UI 线程串行派发，上一个 Tick 没处理完不会进入下一个；
    ///   这也意味着 Tick 里做长任务会拖慢/合并后续 Tick（消息积压时多个 WM_TIMER 合并为一个）；
    /// - 必须在 UI 线程创建和操作（Enabled/Start/Stop/Dispose）；在非 UI 线程 new 出的计时器收不到消息（无泵）；
    /// - Interval 为 0 时计时器持续以最快速度产生 WM_TIMER（等同紧密循环，禁止）；
    /// - Tick 中抛未处理异常在 WinForms 默认策略下会走 ThreadException/可能导致程序崩溃，务必自行 try/catch；
    /// - 重活请用 System.Threading.Timer/Task 跑后台，再 Control.Invoke/BeginInvoke 回 UI 线程更新控件。
    /// 【版本可用性】.NET Framework 全版本（含 4.8，System.Windows.Forms.dll）；.NET Core 3.0+.NET 5+ WinForms 同样可用。
    /// </summary>
    /// <example>
    /// WinForms 秒表（Tick 内直接更新控件）：
    /// <code>
    /// System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
    /// t.Interval = 1000;                       // 约1秒
    /// t.Tick += delegate(object sender, EventArgs e)
    /// {
    ///     label1.Text = DateTime.Now.ToString("HH:mm:ss");  // UI 线程：无需 Invoke
    /// };
    /// t.Start();
    /// </code>
    /// </example>
    public static class HFormsTimerHelp
    {
        /// <summary>
        /// 示例1：创建并启动窗体定时器，Tick 在 UI 线程触发，回调内可直接操作控件。
        /// 只创建并返回；没有消息泵则不会真正 Tick（不报错）。
        /// </summary>
        /// <param name="intervalMs">间隔（毫秒）；精度约 15ms，0 会导致高频消息风暴</param>
        /// <param name="onTick">Tick 事件处理程序（UI 线程串行执行，不重入）</param>
        /// <returns>已 Start 的 System.Windows.Forms.Timer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        public static FormsTimer Create(int intervalMs, EventHandler onTick)
        {
            FormsTimer timer = new FormsTimer();       // 构造1：无容器
            timer.Interval = intervalMs;
            timer.Tick += delegate (object sender, EventArgs e)
            {
                onTick(sender, e);   // UI线程触发
            };
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例2：构造2：创建并加入 IContainer（如 Form 的 components），容器释放时定时器自动释放（设计器标准写法）。
        /// </summary>
        /// <param name="container">宿主容器（通常是窗体 designer 生成的 components）；可为 null（等价无参构造）</param>
        /// <param name="intervalMs">间隔（毫秒）</param>
        /// <param name="onTick">Tick 处理程序</param>
        /// <returns>已 Start、生命周期托管给 container 的 System.Windows.Forms.Timer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        public static FormsTimer CreateOwnedBy(IContainer container, int intervalMs, EventHandler onTick)
        {
            // 构造2：Timer(IContainer)，内部把自身 Add 进容器
            FormsTimer timer = new FormsTimer(container);
            timer.Interval = intervalMs;
            timer.Tick += onTick;
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例3：一次性计时：首次 Tick 先 Stop 再执行回调。
        /// </summary>
        /// <param name="delayMs">延迟（毫秒）</param>
        /// <param name="onTick">只执行一次的 Tick 处理程序</param>
        /// <returns>已 Start 的一次性 System.Windows.Forms.Timer</returns>
        /// <exception cref="ArgumentNullException">onTick 为 null</exception>
        public static FormsTimer OneShot(int delayMs, EventHandler onTick)
        {
            FormsTimer timer = new FormsTimer();
            timer.Interval = delayMs;
            timer.Tick += delegate (object sender, EventArgs e)
            {
                timer.Stop();          // 自停：只执行一次
                onTick(sender, e);
            };
            timer.Start();
            return timer;
        }

        /// <summary>
        /// 示例4：延迟在 UI 线程执行一次动作，触发后自动 Stop/Dispose（适合延迟打开菜单/延迟聚焦等）。
        /// </summary>
        /// <param name="host">宿主控件（保留参数用于表意：本计时器应在该控件所在 UI 线程创建与调用；方法内不使用）</param>
        /// <param name="delayMs">延迟（毫秒）</param>
        /// <param name="action">触发时执行的动作（UI 线程）</param>
        /// <exception cref="ArgumentNullException">action 为 null</exception>
        public static void DelayOnce(Control host, int delayMs, Action action)
        {
            // host 仅用于约束调用语义：调用方需在 host 所在 UI 线程调用本方法
            FormsTimer timer = new FormsTimer();
            timer.Interval = delayMs;
            timer.Tick += delegate
            {
                timer.Stop();
                timer.Dispose();       // 用完即释放，避免泄漏组件
                action();
            };
            timer.Start();
        }

        /// <summary>
        /// 示例5：启停（Enabled 与 Start/Stop 等价）。
        /// </summary>
        /// <param name="timer">目标定时器</param>
        /// <param name="enabled">true=启动；false=停止</param>
        /// <exception cref="ArgumentNullException">timer 为 null</exception>
        public static void SetEnabled(FormsTimer timer, bool enabled)
        {
            timer.Enabled = enabled;   // 与 Start/Stop 等价
        }

        /// <summary>
        /// 示例6：OnTick 演示派生类：重写 protected virtual OnTick 可在"引发 Tick 事件"前后插入逻辑
        /// （订阅 Tick 事件只能在引发后；重写 OnTick 是更面向对象的扩展点，派生类记得调 base.OnTick）。
        /// </summary>
        public sealed class OnTickDemoTimer : FormsTimer
        {
            private int tickCount;

            /// <summary>
            /// 初始化演示计时器并预置 100ms 间隔。
            /// </summary>
            public OnTickDemoTimer()
            {
                Interval = 100;
            }

            /// <summary>
            /// OnTick 重写：WM_TIMER 被消息泵取出时调用；base.OnTick(e) 负责引发 Tick 事件。
            /// </summary>
            /// <param name="e">事件参数（通常 EventArgs.Empty）</param>
            protected override void OnTick(EventArgs e)
            {
                // Tick 事件引发前的逻辑（仍在 UI 线程，同样不会重入）
                tickCount++;
                base.OnTick(e);        // 必须调用：否则外部订阅的 Tick 不会触发
            }

            /// <summary>
            /// 累计 OnTick 次数。
            /// </summary>
            /// <returns>自创建以来 OnTick 被调用的次数</returns>
            public int TickCount
            {
                get { return tickCount; }
            }
        }

        /// <summary>
        /// 示例7：生命周期自测演示：构造 → Start → Stop → Dispose 立即完成，不等待 WM_TIMER、不依赖消息泵。
        /// </summary>
        /// <returns>固定的完成描述字符串</returns>
        public static string QuickLifecycleDemo()
        {
            FormsTimer timer = new FormsTimer();
            timer.Interval = 100;
            timer.Start();              // Enabled=true：消息泵不存在时只是无消息可取，无副作用
            timer.Stop();
            timer.Dispose();
            return HTranslation.GetContent("FormsTimer Start/Stop/Dispose 完成");
        }
    }
}
