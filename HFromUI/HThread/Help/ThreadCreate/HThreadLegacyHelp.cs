using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// 【是什么】Thread 遗留/危险 API 集中帮助类：汇总 .NET Framework 4.8 中仍然可用、但已被标记为
    /// 过时或语义危险的老 Thread API——<see cref="Thread.Abort()"/>、Thread.Suspend/Thread.Resume、
    /// <see cref="Thread.Interrupt()"/>、<see cref="Thread.BeginCriticalRegion()"/>/
    /// <see cref="Thread.EndCriticalRegion()"/>、Thread.VolatileRead/Thread.VolatileWrite，
    /// 并提供少量“受控、可回收、绝不波及调用线程”的真实可运行演示，帮助理解这些 API 的准确语义。
    /// 新代码一律使用协作式取消（CancellationToken）与 <see cref="System.Threading.Volatile"/>。
    ///
    /// 【是否跨进程】否。全部是进程内对“另一个托管线程”的控制或对当前线程的内存/宿主通知，
    /// 不涉及任何跨进程、跨机器能力。
    ///
    /// 【典型适用场景】
    /// 1) 阅读/维护 .NET Framework 老代码时，准确理解 Abort/Suspend/Interrupt 的行为与风险；
    /// 2) 把老代码迁移到协作式取消前，先在受控环境复现遗留 API 的现象；
    /// 3) 理解 CLR 宿主临界区通知（BeginCriticalRegion/EndCriticalRegion）的历史作用；
    /// 4) 识别老内存 API（Thread.VolatileRead/VolatileWrite）并替换为 System.Threading.Volatile
    ///    （见 <see cref="HVolatileHelp"/>）。
    ///
    /// 【使用步骤】
    /// 1) 停止线程：不要用 Abort/Suspend，改为 CancellationToken 或 volatile 停止标志，线程自行退出；
    /// 2) 唤醒阻塞线程：优先 CancellationToken 的可取消等待重载；确需理解老机制时看 Interrupt 演示；
    /// 3) 临界区通知：普通自托管程序无需调用，仅 SQL CLR 之类寄宿宿主关心；
    /// 4) 内存读写：删除 VolatileRead/VolatileWrite，统一改用 Volatile.Read/Volatile.Write。
    ///
    /// 【注意事项与坑】
    /// 1) Thread.Abort()：在目标线程上抛出 <see cref="ThreadAbortException"/>。即使线程 catch 住该异常，
    ///    CLR 也会在 catch 块末尾“自动重新抛出”，只有在线程内调用 <see cref="Thread.ResetAbort()"/> 才能取消中止；
    ///    finally 块仍保证执行，但异常可能在 lock/Monitor.Enter 与 using(Dispose) 中途炸开，导致锁状态被破坏、
    ///    资源半释放。纯托管无限循环通常可被及时打断；线程若正处于非托管代码/P/Invoke 中，要等其返回托管代码后
    ///    才会注入异常，不保证及时。绝不要对当前线程（或反射测试线程）调用 Abort；
    /// 2) Thread.Suspend/Thread.Resume 在 net48 已标 [Obsolete]：被 Suspend 的线程可能正持有锁/Monitor，
    ///    其他线程再去获取同一把锁即死锁，且挂起点不可控。本类不提供任何可执行的 Suspend/Resume 演示；
    /// 3) Thread.Interrupt()：唤醒正处于 WaitSleepJoin（Sleep/Wait/Join）的线程并抛
    ///    <see cref="ThreadInterruptedException"/>；目标线程此刻不在等待，则异常被“记账”，等它下次进入等待时才抛。
    ///    异常点明确、目标可自行 catch 清理，比 Abort 安全，但仍要求目标配合，首选 CancellationToken；
    /// 4) BeginCriticalRegion/EndCriticalRegion：通知宿主当前线程进入/离开临界区。区域内发生未处理异常时，
    ///    宿主可能选择卸载 AppDomain 而不是终止整个进程。只有寄宿在 SQL Server 这类自定义宿主中才有实际意义；
    ///    普通自托管宿主（控制台/WinForms/IIS 普通站点）观察不到任何区别；
    /// 5) Thread.VolatileRead/VolatileWrite 是 .NET 1.x 时代的弱类型内存语义 API（大量重载、按类型区分），
    ///    容易用错重载，应统一改用 System.Threading.Volatile 类（见 <see cref="HVolatileHelp"/>）。
    ///
    /// 【版本可用性】以上 API 在 net48（mscorlib）均存在；.NET Core/.NET 5+ 中：
    /// Thread.Abort() 已移除（运行时抛 <see cref="PlatformNotSupportedException"/>）；
    /// Suspend/Resume 已移除；VolatileRead/VolatileWrite 的部分重载被移除；
    /// Interrupt 与 BeginCriticalRegion/EndCriticalRegion 保留，但后者的宿主策略模型在 .NET Core 中基本失去意义。
    /// </summary>
    /// <example>
    /// 推荐做法——协作式取消替代 Abort（强杀）：
    /// <code>
    /// CancellationTokenSource cts = new CancellationTokenSource();
    /// Thread worker = new Thread((ThreadStart)delegate
    /// {
    ///     while (true)
    ///     {
    ///         if (cts.IsCancellationRequested)
    ///         {
    ///             break;                       // 线程在安全点自行退出，锁/using 状态完好
    ///         }
    ///         Thread.Sleep(20);
    ///     }
    /// }) { IsBackground = true };
    /// worker.Start();
    /// cts.Cancel();                            // 发取消信号而不是 Abort
    /// worker.Join(2000);                       // 带超时回收
    /// </code>
    /// </example>
    /// <example>
    /// Suspend/Resume 反面教材（仅说明，禁止照抄；net48 编译即 Obsolete 警告，.NET Core 已移除）：
    /// <code>
    /// // worker.Suspend();   // 线程可能正持有 lock，其他线程立刻死锁，且挂起点不可控
    /// // worker.Resume();    // 即使恢复，也无法保证锁/资源状态一致
    /// // 正确替代：协作式停止标志 + CancellationToken
    /// </code>
    /// </example>
    public static class HThreadLegacyHelp
    {
        /// <summary>
        /// 受控演示 Thread.Abort() 的真实语义：新建一个“纯托管”工作线程在 Sleep 循环中空转，
        /// Start 并确认其运行后，仅对该专用工作线程调用 Abort；工作线程 catch
        /// <see cref="ThreadAbortException"/> 后 CLR 仍在 catch 末尾自动重新抛出，线程随之结束，
        /// 最后用带超时的 Join 回收并核对 IsAlive。
        /// 这是受控演示：绝不 Abort 当前线程/调用线程，工作线程不持有任何锁、不使用 using 资源，
        /// 生产代码停止线程必须改用 CancellationToken。
        /// </summary>
        /// <returns>工作线程在 2000ms 内被中止回收（Join 成功且 IsAlive 为 false）返回 true；否则 false。</returns>
        /// <exception cref="PlatformNotSupportedException">
        /// 仅当本方法在 .NET Core/.NET 5+ 上运行时，Abort 会由运行时抛出；net48 不会。
        /// </exception>
        public static bool AbortWorkerSafelyDemo()
        {
            Thread worker = new Thread((ThreadStart)delegate
            {
                try
                {
                    while (true)
                    {
                        // 纯托管代码：Abort 可在任意托管执行点及时注入 ThreadAbortException
                        Thread.Sleep(50);
                    }
                }
                catch (ThreadAbortException)
                {
                    // 关键语义：即使 catch 住，CLR 也会在 catch 末尾自动重新抛出；
                    // 只有调用 Thread.ResetAbort() 才能取消中止。此处刻意不取消，让线程受控结束。
                    // finally 块仍会执行（本演示没有 lock/using，故不存在状态损坏风险）。
                }
            }) { IsBackground = true, Name = "HThreadLegacyHelp.AbortDemo" };

            worker.Start();
            Thread.Sleep(50);           // 确保工作线程已进入托管循环
            worker.Abort();             // 仅中止这个专用演示线程，绝不 Abort 当前线程/测试线程

            bool joined = worker.Join(2000);   // 带超时回收，防止异常环境下挂死
            return joined && !worker.IsAlive;  // 纯托管循环中 Abort 应及时生效
        }

        /// <summary>
        /// 受控演示 <see cref="Thread.Interrupt()"/>：工作线程在
        /// <see cref="Thread.Sleep(int)"/>(<see cref="Timeout.Infinite"/>) 中处于 WaitSleepJoin，
        /// 主线程对其调用 Interrupt 后，工作线程收到 <see cref="ThreadInterruptedException"/>，
        /// 自行 catch 并通过 <see cref="ManualResetEventSlim"/> 回报后正常退出。
        /// 相比 Abort：Interrupt 的异常只在明确的等待点抛出，不会在普通托管指令中间炸开，
        /// 但仍要求目标线程配合 catch；新代码应优先使用可取消等待（CancellationToken）。
        /// </summary>
        /// <returns>工作线程在 2000ms 内确认收到中断信号且线程被 Join 回收时返回 true；否则 false。</returns>
        public static bool InterruptSleepingWorkerDemo()
        {
            using (ManualResetEventSlim done = new ManualResetEventSlim(false))
            {
                Thread worker = new Thread((ThreadStart)delegate
                {
                    try
                    {
                        // 无限等待：线程处于 WaitSleepJoin，可被外部 Interrupt 唤醒
                        Thread.Sleep(Timeout.Infinite);
                    }
                    catch (ThreadInterruptedException)
                    {
                        // 被主线程协作唤醒：异常点明确，可安全清理后正常退出
                        done.Set();
                    }
                }) { IsBackground = true, Name = "HThreadLegacyHelp.InterruptDemo" };

                worker.Start();
                Thread.Sleep(50);           // 确保工作线程已进入 WaitSleepJoin
                worker.Interrupt();         // 注入 ThreadInterruptedException（记账机制保证下次等待也会抛）

                bool signaled = done.Wait(2000);  // 等待工作线程确认收到中断
                bool joined = worker.Join(2000);  // 带超时回收
                return signaled && joined;
            }
        }

        /// <summary>
        /// 受控演示 <see cref="Thread.BeginCriticalRegion()"/> 与
        /// <see cref="Thread.EndCriticalRegion()"/> 的配对调用：在当前线程上立即进入并离开临界区。
        /// 注意其实际效果只对“自定义 CLR 宿主”有意义（临界区内未处理异常时宿主可能只卸载 AppDomain
        /// 而非终止整个进程）；在普通控制台/WinForms 等自托管宿主中调用与否观察不到任何区别，
        /// 因此本方法仅验证配对调用本身不抛异常。
        /// </summary>
        /// <returns>配对调用正常完成始终返回 true。</returns>
        public static bool CriticalRegionPairDemo()
        {
            // 静态方法，作用于“当前线程”：通知宿主进入临界区（中止代价更大）
            Thread.BeginCriticalRegion();
            try
            {
                // 临界区代码：宿主被告知此处线程异常终止的影响面更大
            }
            finally
            {
                // 用 finally 保证严格配对，避免提前 return/异常时漏调
                Thread.EndCriticalRegion();  // 静态方法，通知宿主离开临界区；自托管宿主中无可见效果
            }
            return true;
        }
    }
}
