using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// ThreadPool 帮助类：CLR 线程池，复用线程。
    /// 【是什么】System.Threading.ThreadPool 是进程内唯一的、由 CLR 统一管理的后台线程池：
    /// 工作项排入全局/本地队列，由少量线程取走执行，跑完归还复用，避免频繁创建/销毁线程的开销。
    /// Task / Parallel / PLINQ / Timer 回调 / 异步 IO 完成端口底层默认都走线程池。
    /// 【是否跨进程】否。线程池是每个进程（每个 CLR）各自独立的一组线程。
    /// 【典型适用场景】
    /// 1) 大量、短小、突发的后台工作（日志落盘、心跳、触发后不管的计算）；
    /// 2) 不需要返回值/延续/取消的简单回调（QueueUserWorkItem）；
    /// 3) 异步等待内核句柄（RegisterWaitForSingleObject）；
    /// 4) IO 完成端口回调（UnsafeQueueNativeOverlapped，底层库使用）。
    /// 需要返回值、异常聚合、取消、延续时直接用 Task（见 HTaskHelp）。
    /// 【使用步骤】
    /// 1) 调用 ThreadPool.QueueUserWorkItem(WaitCallback) 或带 state 的重载入队；
    /// 2) 回调签名是 void WaitCallback(object state)，在线程池线程上被调用；
    /// 3) 需要调线程池规模时用 GetMinThreads/SetMinThreads/GetMaxThreads/SetMaxThreads；
    /// 4) 用 GetAvailableThreads 观察当前空闲工作线程数；
    /// 5) UnsafeQueueUserWorkItem 用于不流转 ExecutionContext 的高性能场景。
    /// 【注意事项与坑】
    /// - 线程池线程全部是后台线程，不能命名，不能精确控制其生命周期；
    /// - 不要在线程池线程里做长阻塞（长时间 Sleep/等待锁/同步 IO）：会占用池线程，
    ///   线程爬坡有延迟，突发时可能饿死其他工作项；长阻塞用 LongRunning 任务或独立 Thread；
    /// - 工作线程与 IOCP（异步 IO）线程是两条独立的队列，配置时两个数字都要给；
    /// - SetMaxThreads 不能小于 CPU 核数、不能小于 MinThreads，否则返回 false（参数不会被修改）；
    /// - 回调内未捕获异常会终止进程，回调体必须自行 try/catch；
    /// - Unsafe 版不流转执行上下文，回调里拿不到当前身份/上下文，安全敏感场景慎用。
    /// 【版本可用性】net48 全量可用：QueueUserWorkItem(2 个重载)、UnsafeQueueUserWorkItem(WaitCallback 重载)、
    /// GetMaxThreads/SetMaxThreads、GetMinThreads/SetMinThreads、GetAvailableThreads、
    /// RegisterWaitForSingleObject(多个重载)、UnsafeQueueNativeOverlapped。
    /// 接收 IThreadPoolWorkItem 的泛型 UnsafeQueueUserWorkItem&lt;T&gt;(T, bool) 是 .NET Core 3.0+ 才有
    /// （见 HIThreadPoolWorkItemHelp），net48 不可用。
    /// </summary>
    public static class HThreadPoolHelp
    {
        /// <summary>示例1：把一个无参工作项排入线程池队列</summary>
        /// <param name="work">在线程池线程上执行的工作</param>
        public static void QueueWork(Action work)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state) { work(); });
        }

        /// <summary>示例2：带状态对象排入队列（避免闭包捕获带来的分配，state 可为 null）</summary>
        /// <param name="work">接收状态对象的回调</param>
        /// <param name="state">透传给回调的数据</param>
        public static void QueueWork(Action<object> work, object state = null)
        {
            ThreadPool.QueueUserWorkItem(delegate (object s) { work(s); }, state);
        }

        /// <summary>示例3：调整线程池最小线程数（突发大量任务时避免爬坡延迟，一般应用无需调整）</summary>
        /// <param name="workerThreads">最小工作线程数</param>
        /// <param name="completionPortThreads">最小异步 IO(IOCP)线程数</param>
        /// <returns>true=设置成功；false=参数非法（小于 CPU 核数等），设置被忽略</returns>
        public static bool SetMinThreads(int workerThreads, int completionPortThreads)
        {
            return ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
        }

        /// <summary>示例4：调整线程池最大线程数；过大反而增加上下文切换，通常保持默认</summary>
        /// <param name="workerThreads">最大工作线程数</param>
        /// <param name="completionPortThreads">最大异步 IO(IOCP)线程数</param>
        /// <returns>true=设置成功；false=数字小于 MinThreads 或 CPU 核数，设置被忽略</returns>
        public static bool SetMaxThreads(int workerThreads, int completionPortThreads)
        {
            return ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);
        }

        /// <summary>示例5：查询线程池当前配置：最小/最大线程数、可用（空闲）线程数</summary>
        /// <param name="minWorker">输出：最小工作线程数</param>
        /// <param name="minIocp">输出：最小 IOCP 线程数</param>
        /// <param name="maxWorker">输出：最大工作线程数</param>
        /// <param name="maxIocp">输出：最大 IOCP 线程数</param>
        /// <param name="availableWorker">输出：当前可用工作线程数</param>
        /// <param name="availableIocp">输出：当前可用 IOCP 线程数</param>
        public static void GetSetup(out int minWorker, out int minIocp, out int maxWorker, out int maxIocp,
                                   out int availableWorker, out int availableIocp)
        {
            ThreadPool.GetMinThreads(out minWorker, out minIocp);
            ThreadPool.GetMaxThreads(out maxWorker, out maxIocp);
            ThreadPool.GetAvailableThreads(out availableWorker, out availableIocp);
        }

        /// <summary>示例6：仅查询最大线程数（GetMaxThreads）</summary>
        /// <param name="workerThreads">输出：最大工作线程数</param>
        /// <param name="completionPortThreads">输出：最大 IOCP 线程数</param>
        public static void GetMaxThreads(out int workerThreads, out int completionPortThreads)
        {
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
        }

        /// <summary>示例7：仅查询当前可用线程数（GetAvailableThreads = 最大值 - 正在使用的数量）</summary>
        /// <param name="workerThreads">输出：可用工作线程数</param>
        /// <param name="completionPortThreads">输出：可用 IOCP 线程数</param>
        public static void GetAvailableThreads(out int workerThreads, out int completionPortThreads)
        {
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
        }

        /// <summary>示例8：UnsafeQueueUserWorkItem：不流转 ExecutionContext，少做一次上下文捕获，性能略高</summary>
        /// <param name="work">在线程池线程上执行的工作</param>
        /// <remarks>与 QueueUserWorkItem 的区别：不传播安全上下文/逻辑调用上下文，
        /// 回调里 Thread.CurrentPrincipal、AsyncLocal 等不会跟随；仅在确定不需要这些上下文时使用。</remarks>
        public static void UnsafeEnqueue(Action work)
        {
            // net48 的签名是 UnsafeQueueUserWorkItem(WaitCallback, object)；泛型 IThreadPoolWorkItem 重载 net48 没有
            ThreadPool.UnsafeQueueUserWorkItem(delegate (object state)
            {
                try
                {
                    work();
                }
                catch
                {
                    // 线程池回调异常必须自己吞掉或记录，否则可能终止进程
                }
            }, null);
        }

        /// <summary>示例9：RegisterWaitForSingleObject：让线程池等待内核句柄，信号到来（或超时）时在池线程回调</summary>
        /// <param name="handle">要等待的 WaitHandle（ManualResetEvent/AutoResetEvent/Mutex 等）</param>
        /// <param name="onSignaled">回调：true=句柄收到信号；false=等待超时</param>
        /// <param name="state">透传给回调的状态，可为 null</param>
        /// <param name="timeoutMs">超时毫秒；Timeout.Infinite 表示不超时只等信号</param>
        /// <param name="executeOnce">true=只回调一次后自动注销；false=反复等待（轮询式回调）</param>
        /// <returns>RegisteredWaitHandle 句柄，用完应调 Unregister 注销</returns>
        public static RegisteredWaitHandle RegisterWait(WaitHandle handle, Action<bool> onSignaled,
                                                        object state, int timeoutMs, bool executeOnce)
        {
            return ThreadPool.RegisterWaitForSingleObject(handle,
                delegate (object s, bool timedOut)
                {
                    onSignaled(timedOut);       // timedOut=false 表示句柄被置位
                }, state, timeoutMs, executeOnce);
        }

        /// <summary>示例10：RegisterWaitForSingleObject 自测：立即置位事件，回调必然很快执行，用毫秒超时兜底</summary>
        /// <returns>true=回调在 1 秒内被信号触发；false=未触发（异常情况）</returns>
        public static bool WaitForSignalExample()
        {
            ManualResetEvent signal = new ManualResetEvent(false);
            ManualResetEventSlim callbackDone = new ManualResetEventSlim(false);
            RegisteredWaitHandle registered = RegisterWait(signal,
                delegate (bool timedOut)
                {
                    if (!timedOut)
                    {
                        callbackDone.Set();     // 收到信号，通知主线程
                    }
                }, null, 500, true);           // 500ms 超时兜底，只执行一次
            signal.Set();                       // 立即置位，回调在线程池线程触发
            bool result = callbackDone.Wait(1000);
            registered.Unregister(null);        // 注销等待，避免泄漏
            signal.Dispose();
            callbackDone.Dispose();
            return result;
        }

        /// <summary>示例11：注销 RegisterWaitForSingleObject 返回的等待句柄</summary>
        /// <param name="registeredWaitHandle">注册时返回的句柄</param>
        /// <param name="waitForCallbacks">注销时若有回调正在执行，等它结束后置位的通知句柄；可为 null</param>
        /// <returns>true=注销成功</returns>
        public static bool UnregisterWait(RegisteredWaitHandle registeredWaitHandle, WaitHandle waitForCallbacks = null)
        {
            return registeredWaitHandle.Unregister(waitForCallbacks);
        }
    }
}
