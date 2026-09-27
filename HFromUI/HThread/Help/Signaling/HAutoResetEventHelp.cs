using System;
using System.Threading;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="AutoResetEvent"/> 帮助类。AutoResetEvent 是基于操作系统内核等待句柄
    /// （<see cref="EventWaitHandle"/> 的派生类）的“自动重置事件”，行为像地铁“旋转闸机”：
    /// 初始状态由构造参数决定（true=门已开、false=门关闭）；一次 <see cref="EventWaitHandle.Set"/>
    /// 只放行恰好一个等待线程，放行后闸门立即自动恢复为关闭状态。
    /// 与之相对，<see cref="ManualResetEvent"/> 像“门闩/大门”：Set 后门一直开着，所有等待线程
    /// （含后续到达者）全部放行，直到显式 <see cref="EventWaitHandle.Reset"/> 才关门。
    ///
    /// 【是否跨进程】普通的 <see cref="AutoResetEvent(bool)"/> 构造函数只能创建进程内匿名句柄，不能跨进程；
    /// 需要跨进程时不要用 AutoResetEvent 本身（它没有命名构造函数），应使用
    /// <c>new EventWaitHandle(initialState, EventResetMode.AutoReset, name)</c> 创建“命名自动重置事件”，
    /// 同名对象由操作系统内核在多进程间共享。
    ///
    /// 【典型适用场景】
    /// 1) 一对一的“任务完成/资源就绪”通知：一个线程发信号，另一个线程被唤醒；
    /// 2) 资源池的单令牌发放（每次 Set 只发出一个许可）；
    /// 3) 跨进程的单次通知（用命名 EventWaitHandle + AutoReset 模式）。
    /// 需要一次唤醒全部等待线程（广播/发令枪/全局停止）时请改用 <see cref="ManualResetEvent"/>/
    /// <see cref="ManualResetEventSlim"/>；需要异步等待请用 Task/TaskCompletionSource。
    ///
    /// 【使用步骤】
    /// 1) using 包裹创建 <see cref="AutoResetEvent"/>（false 表示初始无信号）；
    /// 2) 等待方调用 <see cref="WaitHandle.WaitOne()"/> / WaitOne(毫秒) / WaitOne(TimeSpan)；
    /// 3) 发信号方调用 <see cref="EventWaitHandle.Set"/> 放行一个等待者；
    /// 4) 必要时 <see cref="EventWaitHandle.Reset"/> 手动关门（很少用，自动重置一般不需要）；
    /// 5) using 结束自动 Dispose 释放内核句柄。
    ///
    /// 【注意事项与坑】
    /// 1) Set 时若没有等待线程，信号会“留存在闸机里”，下一个 WaitOne 的线程会直接通过并消耗掉它，不会累积——
    ///    多次连续 Set 也最多留存一个信号；
    /// 2) 一次 Set 只能放行一个线程，两个等待线程需要两次 Set，需要广播务必用 ManualResetEvent；
    /// 3) WaitOne(0) 是非阻塞探测：立即返回，且若门是开着会顺手把信号“消耗掉”（门重新关上），不是只读查询；
    /// 4) 内核句柄有分配/上下文切换开销，纯进程内、等待时间极短的高频同步优先考虑
    ///    <see cref="ManualResetEventSlim"/>、Monitor 等轻量方案；
    /// 5) 跨 AppDomain/进程的命名事件注意名称冲突，可用 “Global\” 前缀跨终端会话，并妥善处理权限异常。
    ///
    /// 【版本可用性】.NET Framework 1.1 起提供（WaitOne(TimeSpan) 自 2.0）；.NET Core/.NET 5+ 同样可用。
    /// </summary>
    /// <example>
    /// 最简“生产-消费单次通知”：
    /// <code>
    /// using (AutoResetEvent gate = new AutoResetEvent(false))
    /// {
    ///     ThreadPool.QueueUserWorkItem(delegate
    ///     {
    ///         gate.WaitOne(2000);   // 等待开门，2秒超时保证自测不挂死
    ///         // 收到信号后执行（一次 Set 只放这一个线程，闸门随即自动关闭）
    ///     });
    ///     Thread.Sleep(20);
    ///     gate.Set();               // 开门：放行一个等待者后自动复位
    /// }
    /// </code>
    /// 跨进程命名自动重置事件：
    /// <code>
    /// bool createdNew;
    /// using (EventWaitHandle named = new EventWaitHandle(
    ///     false, EventResetMode.AutoReset, "Local\\MyApp.WorkReady", out createdNew))
    /// {
    ///     named.Set();              // 其他进程中同名对象的 WaitOne 会收到这一次信号
    /// }
    /// </code>
    /// </example>
    public static class HAutoResetEventHelp
    {
        /// <summary>
        /// 示例1：生产者-消费者单次通知。工作线程先在闸机前等待，主线程 Set 放行恰好一个线程。
        /// 全程使用小超时与带超时的 Join，任何一步异常都不会永久挂死调用线程。
        /// </summary>
        /// <returns>工作线程是否在 3000ms 内被放行完成；正常情况始终返回 true。</returns>
        public static bool SignalExample()
        {
            using (AutoResetEvent gate = new AutoResetEvent(false))   // 初始关门
            {
                bool released = false;
                Thread worker = new Thread(delegate ()
                {
                    // 阻塞等待开门；传 2000ms 超时，避免自测时死等
                    if (gate.WaitOne(2000))
                    {
                        released = true;   // 收到信号（一次 Set 只放这一个线程，闸门随即自动关闭）
                    }
                }) { IsBackground = true };
                worker.Start();

                Thread.Sleep(20);          // 确保工作线程已进入等待
                gate.Set();                // 开门：放行一个等待者后自动复位

                bool finished = worker.Join(3000);   // 带超时回收线程，杜绝挂死
                return finished && released;
            }
        }

        /// <summary>
        /// 示例2：带毫秒超时的等待（<see cref="WaitHandle.WaitOne(int)"/>）。
        /// 超时时间内收到信号返回 true 并消耗信号；超时返回 false，适合周期性检查停止标志的场景。
        /// </summary>
        /// <param name="gate">自动重置事件句柄，不能为 null。</param>
        /// <param name="timeoutMs">等待超时（毫秒）；0 表示不等待立即探测，<see cref="Timeout.Infinite"/> 表示无限等待（自测不建议）。</param>
        /// <returns>在超时前收到信号为 true；超时未收到为 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为非 -1 的负数。</exception>
        /// <exception cref="ObjectDisposedException">句柄已 Dispose。</exception>
        public static bool WaitSignal(AutoResetEvent gate, int timeoutMs)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.WaitOne(timeoutMs);
        }

        /// <summary>
        /// 示例3：以 <see cref="TimeSpan"/> 指定超时的等待（<see cref="WaitHandle.WaitOne(TimeSpan)"/>）。
        /// 与毫秒重载语义完全一致，仅超时参数形式不同。
        /// </summary>
        /// <param name="gate">自动重置事件句柄，不能为 null。</param>
        /// <param name="timeout">等待时长；<see cref="TimeSpan.Zero"/> 表示不等待，-1ms 表示无限等待。</param>
        /// <returns>超时前收到信号为 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 超出可接受范围（负值或过大）。</exception>
        /// <exception cref="ObjectDisposedException">句柄已 Dispose。</exception>
        public static bool WaitSignal(AutoResetEvent gate, TimeSpan timeout)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.WaitOne(timeout);
        }

        /// <summary>
        /// 示例4：非阻塞探测（<c>WaitOne(0)</c>）。立即返回闸门当前是否为开状态，
        /// 注意：若返回 true，本次调用已经把那一个信号“消耗”掉，闸门随之关闭，它不是只读查询。
        /// </summary>
        /// <param name="gate">自动重置事件句柄，不能为 null。</param>
        /// <returns>调用瞬间闸门是否有信号（true 同时会消耗该信号）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">句柄已 Dispose。</exception>
        public static bool IsSignaled(AutoResetEvent gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.WaitOne(0);
        }

        /// <summary>
        /// 示例5：<see cref="EventWaitHandle.Reset"/> 的作用——在等待者到达之前手动关门，
        /// 可让此前 Set 留存的信号作废，随后等待者只能等到下一次 Set（本示例中必然超时返回 false）。
        /// </summary>
        /// <returns>等待者在 100ms 内是否收到信号；由于 Set 后立刻 Reset，返回 false。</returns>
        public static bool ResetDiscardsPendingSignalExample()
        {
            using (AutoResetEvent gate = new AutoResetEvent(false))
            {
                gate.Set();      // 没有等待者：信号留存于闸机中
                gate.Reset();    // 手动关门：把留存的信号作废
                return gate.WaitOne(100);   // 100ms 内不会有新信号，返回 false
            }
        }

        /// <summary>
        /// 示例6：演示“旋转闸机”与门闩的核心区别——一次 <see cref="EventWaitHandle.Set"/>
        /// 只放行一个等待线程。两个线程同时等待时，第一次 Set 后仅 1 个被放行；
        /// 随后再 Set 一次以保证两个线程都能干净退出。
        /// </summary>
        /// <returns>第一次 Set 后被放行的线程数，始终为 1（以此证明不是广播）。</returns>
        public static int ReleaseOnlyOneWaiterExample()
        {
            int released = 0;
            using (AutoResetEvent gate = new AutoResetEvent(false))
            {
                ThreadStart waiter = delegate
                {
                    if (gate.WaitOne(2000))
                    {
                        Interlocked.Increment(ref released);   // 被放行后计数
                    }
                };
                Thread t1 = new Thread(waiter) { IsBackground = true };
                Thread t2 = new Thread(waiter) { IsBackground = true };
                t1.Start();
                t2.Start();

                Thread.Sleep(50);    // 确保两个线程都已堵在闸机前
                gate.Set();          // 第一次开门：恰好放行一个
                Thread.Sleep(50);
                int afterFirstSet = Thread.VolatileRead(ref released);   // 此时只可能为 1

                gate.Set();          // 再开一次门，放另一个线程干净退出
                t1.Join(3000);
                t2.Join(3000);
                return afterFirstSet;
            }
        }

        /// <summary>
        /// 示例7：创建“命名自动重置事件”，可被同一台机器上的其他进程/ AppDomain 共享。
        /// <see cref="AutoResetEvent"/> 本身没有命名构造函数，必须用基类 <see cref="EventWaitHandle"/>
        /// 配合 <see cref="EventResetMode.AutoReset"/> 创建。
        /// </summary>
        /// <param name="name">
        /// 内核对象名称；建议加 “Local\”（默认，单会话）或 “Global\”（跨终端会话/服务）前缀；null 为匿名事件。
        /// </param>
        /// <param name="initialState">true 表示创建后即为有信号状态，false 表示关闭。</param>
        /// <param name="createdNew">输出是否由本次调用新建；false 表示同名事件已被其他进程创建。</param>
        /// <returns>命名事件句柄；调用方负责 Dispose（建议 using 包裹）。</returns>
        /// <exception cref="WaitHandleCannotBeOpenedException">在少数受限场景下名称无效时抛出。</exception>
        /// <exception cref="System.IO.IOException">名称无效或无权限（如被其他会话占用 Global 名称）。</exception>
        /// <exception cref="UnauthorizedAccessException">同名对象已存在但访问权限不足。</exception>
        public static EventWaitHandle CreateNamedCrossProcess(string name, bool initialState, out bool createdNew)
        {
            // 同名事件由操作系统内核在多进程间共享；一个进程 Set，其他进程 WaitOne 收到一次信号
            return new EventWaitHandle(initialState, EventResetMode.AutoReset, name, out createdNew);
        }

        /// <summary>
        /// 示例8：在另一进程中打开已经存在的命名自动重置事件（只读获取同一内核对象的引用）。
        /// 打开模式仍需由创建方决定；OpenExisting 不创建新对象。
        /// </summary>
        /// <param name="name">创建时使用的同名内核事件名。</param>
        /// <returns>指向既有命名事件的句柄；调用方负责 Dispose。</returns>
        /// <exception cref="WaitHandleCannotBeOpenedException">指定名称的事件不存在。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> 为 null。</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> 为空字符串。</exception>
        public static EventWaitHandle OpenNamedCrossProcess(string name)
        {
            return EventWaitHandle.OpenExisting(name);
        }
    }
}
