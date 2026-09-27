using System;
using System.Threading;

namespace HFromUI.HThread.Help.Signaling
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="CountdownEvent"/> 帮助类：倒计数汇合事件（纯托管，内部可惰性创建内核句柄）。
    /// 构造时给定初始计数 N：每个分支任务完成时调用 <see cref="CountdownEvent.Signal()"/> 将计数减 1
    /// （或 Signal(n) 一次减 n）；当 <see cref="CountdownEvent.CurrentCount"/> 归 0 的瞬间事件被触发（门打开），
    /// 所有在 <see cref="CountdownEvent.Wait()"/> 上阻塞的线程立即放行。这就是经典的 fork-join（分叉-汇合）同步原语，
    /// 比手写“N 个事件 + 共享计数器 + 锁”简洁可靠。归 0 后可 <see cref="CountdownEvent.Reset()"/> 复用，
    /// 也可在未归 0 前 <see cref="CountdownEvent.AddCount()"/> 动态增加计数。
    ///
    /// 【是否跨进程】否。纯进程内同步原语；其 <see cref="CountdownEvent.WaitHandle"/> 属性虽返回内核句柄，
    /// 但同样只在本进程内有效，不能命名跨进程。跨进程等待多个事件请用多个命名 <see cref="EventWaitHandle"/>
    /// 配合 <see cref="WaitHandle.WaitAll(WaitHandle[])"/>。
    ///
    /// 【典型适用场景】
    /// 1) fork-join：主线程分出 N 个子任务后，等待它们全部完成再继续；
    /// 2) 分批工作的阶段汇合（Reset 后可重复使用于下一批）；
    /// 3) 任务数运行时才能确定：先给较小初值，子任务动态加入时 AddCount；
    /// 4) 需要用 WaitAny/WaitAll 与其他句柄组合时，取其 WaitHandle（会惰性内核化）。
    /// 与 <see cref="Barrier"/> 区别：CountdownEvent 只汇合“一次”（可 Reset 再来），不要求参与线程身份，
    /// 任意线程都可 Signal；Barrier 面向固定参与者多阶段反复汇合。
    ///
    /// 【使用步骤】
    /// 1) using new CountdownEvent(N)；
    /// 2) 分派 N 个工作线程/任务，各自在 finally 中 Signal()（确保异常时也计数）；
    /// 3) 汇合线程 Wait() / Wait(毫秒, CancellationToken) 等待归 0；
    /// 4) 运行中要追加分支：在归 0 之前 AddCount/AddCount(n)；
    /// 5) 下一轮复用：Reset() / Reset(N) 重新计数；
    /// 6) using 结束 Dispose。
    ///
    /// 【注意事项与坑】
    /// 1) Signal 让计数变成负数会抛 <see cref="InvalidOperationException"/>（信号发多了），Signal(n) 的 n 必须 ≥ 1
    ///    （否则 <see cref="ArgumentOutOfRangeException"/>）；
    /// 2) 计数已经归 0（事件已触发）后再 AddCount 会抛 <see cref="InvalidOperationException"/>——
    ///    动态加分支必须赶在归 0 之前；
    /// 3) 构造 initialCount 允许为 0（创建即归 0、Wait 立即通过），但不能为负；
    /// 4) 子任务里务必 try/finally Signal，否则一个分支抛异常漏发信号，Wait 方会永久等待——
    ///    因此生产代码一律用带超时的 Wait；
    /// 5) Dispose 后不得再访问；访问 WaitHandle 会创建内核句柄，用完随 CountdownEvent 一起释放。
    ///
    /// 【版本可用性】.NET Framework 4.0 起；.NET Core/.NET 5+ 均可用（.NET Compact Framework 不支持）。
    /// </summary>
    /// <example>
    /// 经典 fork-join（3 个分支全部完成后主线程继续）：
    /// <code>
    /// using (CountdownEvent countdown = new CountdownEvent(3))
    /// {
    ///     for (int i = 0; i &lt; 3; i++)
    ///     {
    ///         int index = i;
    ///         ThreadPool.QueueUserWorkItem(delegate
    ///         {
    ///             try { DoWork(index); }
    ///             finally { countdown.Signal(); }   // 完成一个，计数 -1
    ///         });
    ///     }
    ///     countdown.Wait(5000);                     // 归 0 前阻塞，带超时保底
    /// }
    /// </code>
    /// </example>
    public static class HCountdownEventHelp
    {
        /// <summary>
        /// 示例1：N 个工作线程全部完成后主线程再继续（经典 fork-join），使用 5000ms 等待超时保底。
        /// 每个工作线程的 Signal 放在 finally 中，业务回调抛异常也不会漏发信号。
        /// </summary>
        /// <param name="workerCount">分支数（初始计数），不能为负数。</param>
        /// <param name="work">每个分支执行的业务逻辑，参数为分支下标；允许为 null（空工作）。</param>
        /// <returns>所有分支是否在 5000ms 内全部完成并使计数归 0；超时返回 false。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="workerCount"/> 为负数。</exception>
        public static bool WaitAllWorkers(int workerCount, Action<int> work)
        {
            return WaitAllWorkers(workerCount, work, 5000);
        }

        /// <summary>
        /// 示例1 的带超时版本：N 个分支完成后汇合，可由调用方指定超时时间。
        /// </summary>
        /// <param name="workerCount">分支数（初始计数），不能为负数。</param>
        /// <param name="work">每个分支的业务逻辑，参数为分支下标；允许为 null。</param>
        /// <param name="timeoutMs">等待超时（毫秒），建议自测时传小值（如 1000~5000）。</param>
        /// <returns>超时前计数归 0 返回 true；超时仍有未完成分支返回 false。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="workerCount"/> 为负数，或 <paramref name="timeoutMs"/> 为非 -1 的负数。</exception>
        public static bool WaitAllWorkers(int workerCount, Action<int> work, int timeoutMs)
        {
            if (workerCount < 0)
            {
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("工作线程数量不能小于 0"));
            }

            using (CountdownEvent countdown = new CountdownEvent(workerCount))
            {
                for (int i = 0; i < workerCount; i++)
                {
                    int index = i;
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            if (work != null)
                            {
                                work(index);
                            }
                        }
                        finally
                        {
                            countdown.Signal();    // 计数 -1；即使 work 抛异常也必须发信号
                        }
                    });
                }
                return countdown.Wait(timeoutMs);   // 计数归 0 立即放行，否则等到超时
            }
        }

        /// <summary>
        /// 示例2：带超时与取消令牌的等待（<see cref="CountdownEvent.Wait(int, CancellationToken)"/>）。
        /// </summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <param name="timeoutMs">超时毫秒；0 为立即探测，-1 为无限等待。</param>
        /// <param name="token">取消令牌；取消后等待抛出 <see cref="OperationCanceledException"/>。</param>
        /// <returns>超时前归 0 返回 true；超时返回 false。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="token"/> 被取消。</exception>
        /// <exception cref="ObjectDisposedException">事件或令牌相关资源已释放。</exception>
        public static bool WaitAllWorkers(CountdownEvent countdown, int timeoutMs, CancellationToken token)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.Wait(timeoutMs, token);
        }

        /// <summary>
        /// 示例3：计数减 1（<see cref="CountdownEvent.Signal()"/>）。归 0 的瞬间事件触发。
        /// </summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <returns>本次 Signal 后计数是否恰好归 0。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        /// <exception cref="InvalidOperationException">计数已经是 0（信号多发，将变负）。</exception>
        /// <exception cref="ObjectDisposedException">事件已 Dispose。</exception>
        public static bool SignalOne(CountdownEvent countdown)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.Signal();
        }

        /// <summary>
        /// 示例4：一次将计数减 n（<see cref="CountdownEvent.Signal(int)"/>），
        /// 用于一个分支代表 n 个工作单元的场景。
        /// </summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <param name="signalCount">本次递减的数量，必须 ≥ 1，且不能大于当前计数。</param>
        /// <returns>递减后计数是否归 0。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="signalCount"/> 小于 1。</exception>
        /// <exception cref="InvalidOperationException"><paramref name="signalCount"/> 大于当前计数（会变负）。</exception>
        public static bool SignalMany(CountdownEvent countdown, int signalCount)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.Signal(signalCount);
        }

        /// <summary>
        /// 示例5：动态增加等待数（<see cref="CountdownEvent.AddCount()"/> / AddCount(n)）。
        /// 初始任务数未知、运行中追加分支时使用；必须在计数归 0 之前调用。
        /// </summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <param name="count">增加的计数，默认 1；必须 ≥ 1。</param>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> 小于 1。</exception>
        /// <exception cref="InvalidOperationException">事件已经归 0，不允许再增加计数。</exception>
        public static void AddWork(CountdownEvent countdown, int count = 1)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            countdown.AddCount(count);   // 计数 +n，对应还需要额外 n 次 Signal
        }

        /// <summary>
        /// 示例6：归 0 后复用（<see cref="CountdownEvent.Reset()"/> / Reset(n)），
        /// 重新将计数设回初始值或新值，开始下一轮汇合。
        /// </summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <param name="initialCount">新一轮的初始计数；为 -1（默认）表示恢复为构造时的 <see cref="CountdownEvent.InitialCount"/>。</param>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialCount"/> 小于 0（-1 除外）。</exception>
        public static void Reuse(CountdownEvent countdown, int initialCount = -1)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            if (initialCount < 0)
            {
                countdown.Reset();          // 恢复为最初的 InitialCount
            }
            else
            {
                countdown.Reset(initialCount);   // 重新指定新计数
            }
        }

        /// <summary>读取剩余计数（<see cref="CountdownEvent.CurrentCount"/>）：还差多少个 Signal 才汇合。</summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <returns>当前剩余计数；归 0 表示已汇合。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        public static int GetCurrentCount(CountdownEvent countdown)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.CurrentCount;
        }

        /// <summary>读取构造/最近一次 Reset 时设定的初始计数（<see cref="CountdownEvent.InitialCount"/>）。</summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <returns>初始计数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        public static int GetInitialCount(CountdownEvent countdown)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.InitialCount;
        }

        /// <summary>查询是否已经汇合（<see cref="CountdownEvent.IsSet"/>），等价于 CurrentCount == 0。</summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <returns>计数已归 0 返回 true。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        public static bool IsSet(CountdownEvent countdown)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.IsSet;
        }

        /// <summary>
        /// 取出内部内核等待句柄（<see cref="CountdownEvent.WaitHandle"/>），
        /// 仅用于必须与其他 <see cref="WaitHandle"/> 做 WaitAny/WaitAll 组合的老接口；首次访问惰性创建，
        /// 其生命周期归 <paramref name="countdown"/> 所有，Dispose 事件即可，不要单独释放。
        /// </summary>
        /// <param name="countdown">倒计数事件，不能为 null。</param>
        /// <returns>归 0 时变为有信号状态的内核等待句柄。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="countdown"/> 为 null。</exception>
        public static WaitHandle GetWaitHandle(CountdownEvent countdown)
        {
            if (countdown == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("倒计时事件不能为空"));
            }
            return countdown.WaitHandle;
        }

        /// <summary>
        /// 示例7：完全自包含的 fork-join 演示——3 个分支各做极短工作，观察 CurrentCount 从 3 逐减到 0，
        /// 归 0 时 Wait 立即放行。所有等待均带毫秒超时，自测安全。
        /// </summary>
        /// <returns>计数按 3 →（归 0）变化且等待成功时返回 true。</returns>
        public static bool CurrentCountToZeroExample()
        {
            using (CountdownEvent countdown = new CountdownEvent(3))
            {
                for (int i = 0; i < 3; i++)
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        Thread.Sleep(10);          // 模拟极短工作
                        countdown.Signal();        // 计数 -1，到 0 时事件触发
                    });
                }

                bool joined = countdown.Wait(2000);   // CurrentCount 到 0 时立即放行
                return joined && countdown.CurrentCount == 0 && countdown.IsSet;
            }
        }

        /// <summary>
        /// 示例8：演示“信号多发”与“归 0 后加计数”的两个典型异常，便于在自己的代码中规避。
        /// 正常调用方应避免这两种情形；本方法仅用于教学验证，返回是否都按文档抛出了异常。
        /// </summary>
        /// <returns>两种误用都抛出 <see cref="InvalidOperationException"/> 时返回 true。</returns>
        public static bool CommonMisuseThrowsExample()
        {
            bool overSignalThrows = false;
            using (CountdownEvent countdown = new CountdownEvent(1))
            {
                countdown.Signal();                   // 归 0
                try
                {
                    countdown.Signal();               // 再 Signal 会让计数变负
                }
                catch (InvalidOperationException)
                {
                    overSignalThrows = true;
                }
            }

            bool addAfterZeroThrows = false;
            using (CountdownEvent countdown = new CountdownEvent(1))
            {
                countdown.Signal();                   // 归 0，事件已触发
                try
                {
                    countdown.AddCount();             // 归 0 后不能再动态加计数
                }
                catch (InvalidOperationException)
                {
                    addAfterZeroThrows = true;
                }
            }

            return overSignalThrows && addAfterZeroThrows;
        }
    }
}
