using System;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// ThreadPool.QueueUserWorkItem 帮助类：最轻量地直接向线程池排队一个工作项。
    /// 【是什么】ThreadPool.QueueUserWorkItem(WaitCallback) 是 .NET 2.0 起最原始的线程池入口：
    /// WaitCallback 签名为 void(object state)，任务被排入线程池全局队列，无任务对象、无返回值、无延续。
    /// 【是否跨进程】否。工作项在本进程线程池线程上执行。
    /// 【典型适用场景】
    /// 1) 不需要返回值/延续的"发射后不管"后台小事（埋点、缓存预热、通知）；
    /// 2) 比 Task 少一层抽象、少一些分配的高频热路径（配合 state 避免闭包）；
    /// 3) 用 RegisterWaitForSingleObject 让线程池代替自己阻塞等待内核句柄。
    /// 【使用步骤】
    /// 1) 写 WaitCallback（object state）回调，内部第一步 try/catch 兜底；
    /// 2) 调 QueueUserWorkItem(callback) 或 QueueUserWorkItem(callback, state)；
    /// 3) 需要高性能且不依赖 ExecutionContext 时用 UnsafeQueueUserWorkItem；
    /// 4) 需要等句柄时用 RegisterWaitForSingleObject，返回的 RegisteredWaitHandle 用完 Unregister。
    /// 【注意事项与坑】
    /// - 回调若抛未处理异常会终止进程（.NET 2.0+ 默认策略），回调内部务必自行 try/catch；
    /// - 不支持取消、无返回值、无异常聚合——需要这些用 Task；
    /// - Unsafe 版不流转 ExecutionContext：拿不到当前安全身份/AsyncLocal，仅在明确不需要时使用；
    /// - RegisterWaitForSingleObject 的回调收到 timedOut=true 表示超时未收到信号，注意分支；
    ///   executeOnce=false 时会反复注册回调，最后一次之后一定要 Unregister。
    /// 【版本可用性】net48 全量可用：QueueUserWorkItem(2 个重载)、
    /// UnsafeQueueUserWorkItem(WaitCallback, object 重载)、RegisterWaitForSingleObject(多个重载)、
    /// UnsafeRegisterWaitForSingleObject、UnsafeQueueNativeOverlapped（IO 线程投递，底层库用）。
    /// 接收 IThreadPoolWorkItem 的泛型 UnsafeQueueUserWorkItem&lt;T&gt;(T, bool) 为 .NET Core 3.0+，net48 没有。
    /// </summary>
    public static class HQueueUserWorkItemHelp
    {
        /// <summary>示例1：最简单的入队（回调签名 WaitCallback，参数 object），内部 try/catch 兜底</summary>
        /// <param name="work">在线程池线程执行的工作</param>
        public static void EnqueueSimple(Action work)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                try
                {
                    work();
                }
                catch
                {
                    // 线程池工作项异常必须自己吞掉或记录，否则可能导致进程崩溃
                }
            });
        }

        /// <summary>示例2：带状态参数入队，避免闭包分配（state 原样透传给回调）</summary>
        /// <param name="state">透传的状态对象</param>
        public static void EnqueueWithState(object state)
        {
            ThreadPool.QueueUserWorkItem(delegate (object s)
            {
                object captured = s;                  // 即传入的 state
            }, state);
        }

        /// <summary>示例3：泛型包装：拿到接近 Task 的调用体验但保持轻量（state 装箱一次）</summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="argument">透传给工作的强类型参数</param>
        /// <param name="work">接收强类型参数的工作</param>
        public static void QueueWork<T>(T argument, Action<T> work)
        {
            ThreadPool.QueueUserWorkItem(delegate (object state)
            {
                try
                {
                    work((T)state);
                }
                catch
                {
                    // 与示例1同理：异常不能逃出线程池回调
                }
            }, argument);
        }

        /// <summary>示例4：UnsafeQueueUserWorkItem：不流转 ExecutionContext，省掉上下文捕获，略快</summary>
        /// <param name="work">在线程池线程执行的工作</param>
        public static void EnqueueUnsafe(Action work)
        {
            ThreadPool.UnsafeQueueUserWorkItem(delegate (object state)
            {
                try
                {
                    work();
                }
                catch
                {
                    // Unsafe 版本同样不能让异常逃逸
                }
            }, null);
        }

        /// <summary>示例5：RegisterWaitForSingleObject：线程池等待一个内核句柄，信号或超时到来时在线程池执行回调</summary>
        /// <param name="handle">要等待的句柄</param>
        /// <param name="onSignaled">回调：true=超时；false=收到信号</param>
        /// <param name="timeoutMs">超时毫秒，Timeout.Infinite 表示只等信号</param>
        /// <returns>注册句柄，用完应 Unregister</returns>
        public static RegisteredWaitHandle WaitThenEnqueue(WaitHandle handle, Action<bool> onSignaled, int timeoutMs)
        {
            return ThreadPool.RegisterWaitForSingleObject(handle,
                delegate (object state, bool timedOut)
                {
                    onSignaled(timedOut);
                }, null, timeoutMs, true);           // true=只执行一次
        }

        /// <summary>示例6：超时分支自测：句柄始终不置位，回调以 timedOut=true 触发，毫秒级完成</summary>
        /// <returns>true=正确收到超时回调；false=异常未收到</returns>
        public static bool TimeoutWaitExample()
        {
            ManualResetEvent neverSignaled = new ManualResetEvent(false);
            ManualResetEventSlim callbackDone = new ManualResetEventSlim(false);
            RegisteredWaitHandle registered = ThreadPool.RegisterWaitForSingleObject(
                neverSignaled,
                delegate (object state, bool timedOut)
                {
                    if (timedOut)
                    {
                        callbackDone.Set();          // 1ms 内必然超时
                    }
                }, null, 1, true);                  // 1ms 超时，只执行一次
            bool result = callbackDone.Wait(1000);
            registered.Unregister(null);
            neverSignaled.Dispose();
            callbackDone.Dispose();
            return result;
        }

        /// <summary>示例7：注销一次注册等待（executeOnce=false 的循环等待尤其需要手动注销）</summary>
        /// <param name="registeredWaitHandle">注册时返回的句柄</param>
        /// <param name="waitForCallbacks">若注销时回调正在执行，等待其完成后置位该句柄；可为 null</param>
        /// <returns>true=注销成功</returns>
        public static bool Unregister(RegisteredWaitHandle registeredWaitHandle, WaitHandle waitForCallbacks = null)
        {
            return registeredWaitHandle.Unregister(waitForCallbacks);
        }
    }
}
