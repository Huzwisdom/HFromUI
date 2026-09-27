using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】手动重置事件帮助类，包含两个类型：
    /// 1) <see cref="ManualResetEvent"/>：操作系统内核等待句柄（<see cref="EventWaitHandle"/> 派生），
    ///    行为像“大门/门闩”——<see cref="EventWaitHandle.Set"/> 开门后门一直保持打开，
    ///    所有正在等待的线程以及之后到达的线程全部直接放行，直到显式 <see cref="EventWaitHandle.Reset"/> 才关门；
    /// 2) <see cref="ManualResetEventSlim"/>：纯托管轻量实现，短等待先自旋再混合等待，
    ///    支持 <see cref="CancellationToken"/>，开销比内核句柄小，进程内场景应优先使用。
    /// 与 <see cref="AutoResetEvent"/>（旋转闸机，一次 Set 只放一个并自动关门）的关键区别就在“手动”二字：
    /// 门是否关闭完全由代码控制，可一次广播唤醒任意多个线程。
    ///
    /// 【是否跨进程】
    /// <see cref="ManualResetEvent"/> 与 <see cref="ManualResetEventSlim"/> 自身均无命名构造函数，不能直接命名跨进程；
    /// 跨进程/跨 AppDomain 需用 <c>new EventWaitHandle(initialState, EventResetMode.ManualReset, name)</c>
    /// 创建“命名手动重置事件”。Slim 版本纯托管，只能在同一进程内使用，不能参与 WaitHandle.WaitAll/WaitAny 组合。
    ///
    /// 【典型适用场景】
    /// 1) 发令枪：N 个工作线程先就绪等待，主线程一次 Set 全部同时开跑；
    /// 2) 全局停止/关闭广播：一个停止标志唤醒所有工作循环退出；
    /// 3) “一次性就绪”门闩：初始化完成前所有访问者等待，完成后永久放行；
    /// 4) 跨进程的全局状态通知（用命名 EventWaitHandle + ManualReset 模式）。
    ///
    /// 【使用步骤】
    /// 1) 进程内优先 new ManualResetEventSlim(false)，using 包裹；
    /// 2) 等待方调用 Wait() / Wait(毫秒) / Wait(TimeSpan) / Wait(CancellationToken)；
    /// 3) 发令方 Set() 开门广播；需要重新关门时 Reset()（会让之后的等待者再次阻塞）；
    /// 4) 通过 IsSet 查询门是否开着；SpinCount 查询构造时设定的自旋次数；
    /// 5) 需要与其他内核句柄组合（WaitAll/WaitAny）时用 Slim 的 WaitHandle 属性（惰性创建）；
    /// 6) using 结束 Dispose（注意：不要自行 Dispose 从 Slim.WaitHandle 取出的句柄，它归 Slim 所有）。
    ///
    /// 【注意事项与坑】
    /// 1) Set 之后到达的等待线程也不会阻塞——门是“持续打开”的，不是只唤醒当前等待者；
    /// 2) 忘记 Reset 是常见 bug：想做“第二轮发令枪”却发现第二轮线程根本不等直接跑过；
    /// 3) Slim 不能命名、不能跨进程、不能用于 <see cref="WaitHandle.WaitAll(WaitHandle[])"/>（STA 线程上 WaitAll 还会抛异常）；
    /// 4) 访问 Slim.WaitHandle 会强制创建内核句柄，失去轻量优势，仅在确实需要时访问；
    /// 5) Slim 构造 spinCount 合法范围为 0 ~ <see cref="short.MaxValue"/>，超界抛 ArgumentOutOfRangeException。
    ///
    /// 【版本可用性】<see cref="ManualResetEvent"/> 自 .NET Framework 1.1；
    /// <see cref="ManualResetEventSlim"/> 自 .NET Framework 4.0。.NET Core/.NET 5+ 均可用。
    /// </summary>
    /// <example>
    /// 发令枪模式（门闩广播）：
    /// <code>
    /// using (ManualResetEventSlim startGun = new ManualResetEventSlim(false))
    /// {
    ///     for (int i = 0; i &lt; 3; i++)
    ///     {
    ///         int index = i;
    ///         new Thread((ThreadStart)delegate
    ///         {
    ///             startGun.Wait(3000);        // 门关着：所有人在此排队
    ///             Console.WriteLine(index + " 开跑");
    ///         }) { IsBackground = true }.Start();
    ///     }
    ///     Thread.Sleep(50);
    ///     startGun.Set();                     // 开门：所有等待者同时放行，门持续保持打开
    /// }
    /// </code>
    /// </example>
    public static class HManualResetEventHelp
    {
        /// <summary>
        /// 示例1：发令枪模式。多个工作线程先在关闭的门闩前等待，主线程一次 Set 全部同时放行。
        /// 所有等待与线程回收均带毫秒超时，自测不会挂死。
        /// </summary>
        /// <param name="workerCount">参与等待的工作线程数，必须为非负数。</param>
        /// <returns>全部工作线程是否在 3000ms 内被放行完成。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="workerCount"/> 为负数。</exception>
        public static bool StartGunExample(int workerCount)
        {
            if (workerCount < 0)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("工作线程数量不能小于 0"));
            }

            using (ManualResetEventSlim startGun = new ManualResetEventSlim(false))
            {
                Thread[] workers = new Thread[workerCount];
                for (int i = 0; i < workerCount; i++)
                {
                    int index = i;
                    workers[i] = new Thread((ThreadStart)delegate
                    {
                        if (startGun.Wait(2000))   // 门闩关闭，全部在此排队；2 秒超时保底
                        {
                            Console.WriteLine(index + HTranslation.GetContent(" 开跑"));
                        }
                    }) { IsBackground = true };
                    workers[i].Start();
                }

                Thread.Sleep(50);          // 等所有线程就绪排队
                startGun.Set();            // 开门：所有等待者同时放行，门保持打开

                foreach (Thread t in workers)
                {
                    if (!t.Join(3000))      // 带超时回收，杜绝挂死
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 示例2：全局停止广播。工作循环平时每 100ms 醒一次，Set 后立即全部退出；
        /// 演示 <see cref="ManualResetEventSlim.IsSet"/> 与带超时的 Wait(int) 配合。
        /// </summary>
        /// <returns>工作线程是否在 2000ms 内观察到停止信号并退出。</returns>
        public static bool StopBroadcastExample()
        {
            ManualResetEventSlim stopEvent = new ManualResetEventSlim(false);
            Thread worker = new Thread((ThreadStart)delegate
            {
                while (!stopEvent.IsSet)                 // IsSet：门是否开着（只读，不消耗信号）
                {
                    if (stopEvent.Wait(100))             // 分段等待：平时 100ms 一轮，停止时立即响应
                    {
                        break;                           // Wait 返回 true 表示门已开
                    }
                }
            }) { IsBackground = true };
            worker.Start();

            Thread.Sleep(50);
            stopEvent.Set();               // 广播停止；门会一直开着，要再次关门需手动 Reset()

            bool finished = worker.Join(2000);
            stopEvent.Dispose();           // 未使用 using，手动释放
            return finished;
        }

        /// <summary>
        /// 示例3：Slim 的可取消/异步等待。<see cref="ManualResetEventSlim"/> 没有内置 WaitAsync，
        /// 用 <see cref="Task.Run(Action, CancellationToken)"/> 包装阻塞等待，得到可 await 的 Task。
        /// </summary>
        /// <param name="gate">手动重置事件（Slim），不能为 null。</param>
        /// <param name="token">可取消等待的令牌。</param>
        /// <returns>在门打开或令牌取消时完成的任务；取消时以 TaskCanceledException 结束。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        public static Task WaitAsync(ManualResetEventSlim gate, CancellationToken token)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return Task.Run(delegate { gate.Wait(token); }, token);
        }

        /// <summary>
        /// 示例4：创建内核版“命名手动重置事件”，供多进程/多 AppDomain 共享。
        /// <see cref="ManualResetEvent"/> 类本身没有命名构造函数，用 <see cref="EventWaitHandle"/>
        /// 配合 <see cref="EventResetMode.ManualReset"/> 实现。
        /// </summary>
        /// <param name="name">内核事件名，建议带 Local\ 或 Global\ 前缀；null 为匿名。</param>
        /// <param name="initialState">true 创建后即为开门状态。</param>
        /// <returns>命名事件句柄；调用方负责 Dispose。</returns>
        /// <exception cref="System.IO.IOException">名称无效或无权限。</exception>
        /// <exception cref="UnauthorizedAccessException">同名对象已存在但权限不足。</exception>
        public static EventWaitHandle CreateCrossProcess(string name, bool initialState)
        {
            // 同名事件被多个进程共享：任何一个进程 Set，其他进程的等待线程全部放行（门闩语义）
            return new EventWaitHandle(initialState, EventResetMode.ManualReset, name);
        }

        /// <summary>
        /// 示例5：打开其他进程已创建的命名手动重置事件。
        /// </summary>
        /// <param name="name">创建时使用的同名内核事件名。</param>
        /// <returns>既有命名事件的句柄；调用方负责 Dispose。</returns>
        /// <exception cref="WaitHandleCannotBeOpenedException">指定名称的事件不存在。</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> 为空字符串。</exception>
        public static EventWaitHandle OpenCrossProcess(string name)
        {
            return EventWaitHandle.OpenExisting(name);
        }

        /// <summary>
        /// 示例6：带毫秒超时的等待（<see cref="ManualResetEventSlim.Wait(int)"/>）。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <param name="timeoutMs">超时毫秒；0 为立即探测，-1 为无限等待。</param>
        /// <returns>超时前门被打开为 true；超时仍关闭为 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 为非 -1 的负数。</exception>
        /// <exception cref="ObjectDisposedException">对象已 Dispose。</exception>
        public static bool WaitWithTimeout(ManualResetEventSlim gate, int timeoutMs)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.Wait(timeoutMs);
        }

        /// <summary>
        /// 示例7：以 <see cref="TimeSpan"/> 超时等待（<see cref="ManualResetEventSlim.Wait(TimeSpan)"/>）。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <param name="timeout">等待时长；<see cref="TimeSpan.Zero"/> 表示不等待。</param>
        /// <returns>超时前门被打开为 true，否则 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 超出合法范围。</exception>
        public static bool WaitWithTimeSpan(ManualResetEventSlim gate, TimeSpan timeout)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.Wait(timeout);
        }

        /// <summary>
        /// 示例8：演示 Set / Reset / IsSet 的完整循环。Set 后 IsSet 为 true 且等待者立即通过；
        /// Reset 后 IsSet 变 false，之后的等待者重新阻塞。
        /// </summary>
        /// <returns>始终返回 true（用 100ms 小超时验证 Reset 后门确实重新关上）。</returns>
        public static bool SetResetCycleExample()
        {
            using (ManualResetEventSlim gate = new ManualResetEventSlim(false))
            {
                gate.Set();
                bool openAfterSet = gate.IsSet;          // true
                bool passWhileOpen = gate.Wait(100);     // 门开着：立即 true（不消耗信号，可持续放行）

                gate.Reset();                            // 手动关门
                bool openAfterReset = gate.IsSet;        // false
                bool blockedAfterReset = gate.Wait(100); // 门关着：100ms 超时返回 false

                return openAfterSet && passWhileOpen && !openAfterReset && !blockedAfterReset;
            }
        }

        /// <summary>
        /// 示例9：按指定自旋次数创建 Slim。短等待场景先自旋若干次再内核等待，可降低上下文切换开销。
        /// </summary>
        /// <param name="initialState">初始是否开门。</param>
        /// <param name="spinCount">自旋次数，合法范围 0 ~ <see cref="short.MaxValue"/>。</param>
        /// <returns>创建好的 Slim 事件；调用方负责 Dispose。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="spinCount"/> 小于 0 或大于 <see cref="short.MaxValue"/>。</exception>
        public static ManualResetEventSlim CreateWithSpinCount(bool initialState, int spinCount)
        {
            return new ManualResetEventSlim(initialState, spinCount);
        }

        /// <summary>
        /// 示例10：读取构造时设定的自旋次数（<see cref="ManualResetEventSlim.SpinCount"/>）。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <returns>自旋等待次数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        public static int GetSpinCount(ManualResetEventSlim gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.SpinCount;
        }

        /// <summary>
        /// 示例11：取出 Slim 背后的内核 <see cref="WaitHandle"/>（<see cref="ManualResetEventSlim.WaitHandle"/>），
        /// 用于必须与其他内核句柄组合（WaitAny/WaitAll/SignalAndWait）的老接口。首次访问时惰性创建。
        /// 注意：该句柄生命周期归 <paramref name="gate"/> 所有，Dispose gate 即可，切勿单独释放它。
        /// </summary>
        /// <param name="gate">Slim 手动事件，不能为 null。</param>
        /// <returns>Slim 对应的内核等待句柄。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gate"/> 为 null。</exception>
        /// <exception cref="ObjectDisposedException">对象已 Dispose。</exception>
        public static WaitHandle GetKernelWaitHandle(ManualResetEventSlim gate)
        {
            if (gate == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("等待句柄不能为空"));
            }
            return gate.WaitHandle;
        }

        /// <summary>
        /// 示例12：直接使用内核版 <see cref="ManualResetEvent"/>（非 Slim），
        /// 演示 Set/Reset/WaitOne 与“门持续打开”语义；该类型可作为普通 <see cref="WaitHandle"/>
        /// 参与 WaitAny/WaitAll 组合，但不支持取消令牌。
        /// </summary>
        /// <returns>验证结果全部符合预期时返回 true。</returns>
        public static bool KernelManualResetExample()
        {
            using (ManualResetEvent gate = new ManualResetEvent(false))
            {
                bool first = gate.WaitOne(100);          // 初始关闭：超时 false
                gate.Set();                              // 开门并持续保持
                bool second = gate.WaitOne(100);         // 立即 true，且信号不被消耗
                bool third = gate.WaitOne(0);            // 再来一个等待者同样立即 true（广播语义）
                gate.Reset();                            // 手动关门
                bool fourth = gate.WaitOne(100);         // 重新阻塞：超时 false
                return !first && second && third && !fourth;
            }
        }
    }
}
