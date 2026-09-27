using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// RegisteredWaitHandle 帮助类：让线程池替你等待内核句柄，信号或超时发生时在池线程回调。
    ///
    /// 【是什么】<see cref="ThreadPool.RegisterWaitForSingleObject(WaitHandle, WaitOrTimerCallback, object, int, bool)"/>
    /// 把一个 <see cref="WaitHandle"/>（AutoResetEvent/ManualResetEvent/Mutex/Semaphore 等）注册给 CLR 线程池：
    /// 由线程池线程阻塞等待该句柄，一旦句柄被 Set（收到信号）或到达指定超时，就在线程池线程上调用
    /// <see cref="WaitOrTimerCallback"/>（签名 void (object state, bool timedOut)）。
    /// 返回的 <see cref="RegisteredWaitHandle"/> 用 <see cref="RegisteredWaitHandle.Unregister"/> 注销。
    /// 它避免了“每个等待对象占一个线程”的浪费——少量等待者线程即可看护大量句柄。
    ///
    /// 【是否跨进程】否。注册关系、回调都在本进程线程池内；被等待的内核句柄本身可以是跨进程命名对象
    /// （如命名 Mutex/EventWaitHandle），但回调仍在本进程执行。
    ///
    /// 【典型适用场景】
    /// 1) 等待一个内核事件发生，但不想独占一个专职线程去 WaitOne；
    /// 2) 为异步等待加超时（定时轮询）：句柄迟迟不信号时也能周期性收到回调做兜底处理；
    /// 3) 等待跨进程命名 Mutex/Event 的通知，同时需要超时与线程复用；
    /// 4) 旧式 APM/通知模型与 WaitHandle 桥接。
    ///
    /// 【使用步骤】
    /// 1) 准备 <see cref="WaitHandle"/> 与回调 <see cref="WaitOrTimerCallback"/>；
    /// 2) 调 <see cref="ThreadPool.RegisterWaitForSingleObject(WaitHandle, WaitOrTimerCallback, object, int, bool)"/>，
    ///    传入 state、timeoutMs（<see cref="Timeout.Infinite"/>(-1) 表示只等信号不超时）、executeOnlyOnce；
    /// 3) 回调内根据 timedOut 区分原因：true=超时未触发，false=句柄被 Set；
    /// 4) 用完必须对返回的 <see cref="RegisteredWaitHandle"/> 调
    ///    <see cref="RegisteredWaitHandle.Unregister"/>（可传一个通知句柄等待在途回调结束）；
    /// 5) executeOnlyOnce=false 时会反复重新注册直到 Unregister，适合轮询；true 回调一次后自动注销。
    ///
    /// 【注意事项与坑】
    /// 1) 回调运行在线程池线程上：多个等待的回调可能并发，同一回调也可能重入，回调体必须自行 try/catch，
    ///    未捕获异常会终止进程；不要在回调里做长阻塞，避免占满线程池；
    /// 2) timedOut 语义不要搞反：true 表示“超时仍未收到信号”，false 表示“句柄收到信号”；
    /// 3) 注册时如果句柄【已经处于 signaled 状态】，回调会被立即调度（本类 SignalFiresCallbackDemo 即利用此点）；
    /// 4) executeOnlyOnce=false 的重复注册在“回调执行期间”超时计时可能继续，存在重入，
    ///    若回调不是重入安全的，应在回调内自行加锁或改用 true；
    /// 5) 普通 <see cref="ThreadPool.RegisterWaitForSingleObject(WaitHandle, WaitOrTimerCallback, object, int, bool)"/>
    ///    会流转 ExecutionContext；<see cref="ThreadPool.UnsafeRegisterWaitForSingleObject(WaitHandle, WaitOrTimerCallback, object, int, bool)"/>
    ///    不流转（少一次捕获，回调里拿不到 AsyncLocal/安全上下文，安全敏感场景慎用）；
    /// 6) 无论超时还是信号分支，结束后都应 Unregister（executeOnlyOnce=true 自动注销后再调一次也安全），
    ///    否则注册对象与句柄可能无法回收。
    ///
    /// 【版本可用性】net48 全量可用：RegisterWaitForSingleObject 多个重载、
    /// UnsafeRegisterWaitForSingleObject、RegisteredWaitHandle.Unregister 均在 mscorlib。
    /// </summary>
    /// <example>
    /// 注册一个带 100ms 超时的重复等待，收到信号后置一次性注销标志：
    /// <code>
    /// AutoResetEvent handle = new AutoResetEvent(false);
    /// RegisteredWaitHandle registered = ThreadPool.RegisterWaitForSingleObject(
    ///     handle,
    ///     delegate(object state, bool timedOut)
    ///     {
    ///         if (timedOut)
    ///         {
    ///             Console.WriteLine("100ms 内没有信号，执行超时兜底逻辑");
    ///         }
    ///         else
    ///         {
    ///             Console.WriteLine("收到信号");
    ///         }
    ///     },
    ///     null, 100, false);          // false=反复注册，直到 Unregister
    /// // ... 业务结束后必须注销
    /// registered.Unregister(null);
    /// </code>
    /// </example>
    public static class HRegisteredWaitHelp
    {
        /// <summary>
        /// 示例1（自包含可执行）：验证“超时触发回调”。
        /// 注册一个永远不被 Set 的 <see cref="AutoResetEvent"/>，超时设为 50ms，
        /// 回调以 timedOut=true 被调度后置位 done；主线程等待 done（2000ms 兜底），
        /// 然后无条件 <see cref="RegisteredWaitHandle.Unregister"/> 并释放句柄。
        /// </summary>
        /// <returns>回调在 2000ms 内确实以超时原因（timedOut=true）触发返回 true；否则 false。</returns>
        public static bool TimeoutFiresCallbackDemo()
        {
            AutoResetEvent waitEvent = new AutoResetEvent(false);   // 全程不 Set，强制走超时分支
            ManualResetEventSlim done = new ManualResetEventSlim(false);
            bool sawTimeout = false;

            RegisteredWaitHandle registered = ThreadPool.RegisterWaitForSingleObject(
                waitEvent,
                delegate (object state, bool timedOut)
                {
                    // 回调在线程池线程执行；timedOut=true 表示 50ms 超时仍未收到信号
                    if (timedOut)
                    {
                        sawTimeout = true;
                        done.Set();                                // 通知主线程：超时回调已发生
                    }
                },
                null, 50, true);                                   // executeOnlyOnce:true，回调一次即自动注销

            bool firedInTime;
            try
            {
                firedInTime = done.Wait(2000);                     // 2 秒兜底，正常约 50ms 即返回
            }
            finally
            {
                registered.Unregister(null);                       // 无条件注销（双保险，防泄漏）
                done.Dispose();
                waitEvent.Close();
            }

            return firedInTime && sawTimeout;
        }

        /// <summary>
        /// 示例2（自包含可执行）：验证“信号触发回调”。
        /// 直接构造初始状态即 signaled 的 <see cref="AutoResetEvent"/>（注册时句柄已置位会立即调度回调），
        /// executeOnlyOnce=true，回调以 timedOut=false 被调度后置位 done；主线程等待 done（2000ms 兜底），
        /// 然后无条件 <see cref="RegisteredWaitHandle.Unregister"/> 并释放句柄。
        /// </summary>
        /// <returns>回调在 2000ms 内确实以信号原因（timedOut=false）触发返回 true；否则 false。</returns>
        public static bool SignalFiresCallbackDemo()
        {
            // 初始即为 signaled：RegisterWaitForSingleObject 发现句柄已置位，会立即调度回调
            AutoResetEvent waitEvent = new AutoResetEvent(true);
            ManualResetEventSlim done = new ManualResetEventSlim(false);
            bool sawSignal = false;

            RegisteredWaitHandle registered = ThreadPool.RegisterWaitForSingleObject(
                waitEvent,
                delegate (object state, bool timedOut)
                {
                    // timedOut=false 表示句柄被 Set（此处注册时就已经处于 signaled）
                    if (!timedOut)
                    {
                        sawSignal = true;
                        done.Set();                                // 通知主线程：信号回调已发生
                    }
                },
                null, 500, true);                                  // 500ms 仅作超时兜底，实际立即以信号触发

            bool firedInTime;
            try
            {
                firedInTime = done.Wait(2000);                     // 2 秒兜底，正常毫秒级即返回
            }
            finally
            {
                registered.Unregister(null);                       // 无条件注销，避免注册对象泄漏
                done.Dispose();
                waitEvent.Close();
            }

            return firedInTime && sawSignal;
        }
    }
}
