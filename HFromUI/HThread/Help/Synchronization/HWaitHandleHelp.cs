using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】<see cref="WaitHandle"/> 帮助类：等待句柄抽象基类（抽象类，实现 <see cref="IDisposable"/>），
    /// 是 <see cref="Mutex"/>、<see cref="Semaphore"/>、<see cref="EventWaitHandle"/>
    /// （<see cref="AutoResetEvent"/>/<see cref="ManualResetEvent"/>）等【内核同步对象】的共同托管包装。
    /// WaitHandle 既可以单独等待（<see cref="WaitHandle.WaitOne(int)"/>），也可以批量等待
    /// （静态 <see cref="WaitHandle.WaitAny(WaitHandle[], int)"/> / <see cref="WaitHandle.WaitAll(WaitHandle[], int)"/>，
    /// 最多 64 个句柄），还能“通知一个、等待另一个”的原子交接
    /// （<see cref="WaitHandle.SignalAndWait(WaitHandle, WaitHandle, int, bool)"/>）。
    ///
    /// 【是否跨进程】可以。派生类中带名字（named）的内核对象可跨进程/跨会话共享；
    /// 即使是匿名句柄，等待本身也经由操作系统内核（与 Monitor 等纯托管原语不同，开销更大）。
    ///
    /// 【典型适用场景】
    /// 1) 等待一个内核事件/互斥/信号量被置位（WaitOne 各超时重载）；
    /// 2) 一批后台任务中“只等最先完成的一个”（WaitAny，返回最先收到信号的句柄索引）；
    /// 3) 等待一批任务“全部完成”（WaitAll）；
    /// 4) 两个线程做原子交接（SignalAndWait：Set 一个句柄并同时等待另一个）。
    ///
    /// 【使用步骤】
    /// 1) 创建具体派生类实例（AutoResetEvent/ManualResetEvent/Mutex/Semaphore 等）；
    /// 2) 调用 WaitOne/WaitAny/WaitAll/SignalAndWait，一律带毫秒或 <see cref="TimeSpan"/> 超时，
    ///    常量 <see cref="Timeout.Infinite"/> 的值为 -1，代表无限等待（自测代码不要用）；
    /// 3) WaitAny 超时时返回 <see cref="WaitHandle.WaitTimeout"/>（值为 258），WaitOne/WaitAll/SignalAndWait
    ///    超时返回 false——它们不会抛异常（注意：System.Threading.WaitTimeoutException 是 .NET Framework
    ///    1.0/1.1 的历史类型，2.0 起已移除，net48 中不存在，代码中不要引用）；
    /// 4) 用完用 using/Dispose 释放内核句柄；也可调用 <see cref="WaitHandle.Close"/>（与 Dispose 等价）；
    /// 5) 需要与原生句柄互操作时通过 <see cref="WaitHandle.SafeWaitHandle"/> 属性传/换句柄
    ///    （旧的 Handle 属性已过时，不要使用）。
    ///
    /// 【注意事项与坑】
    /// 1) 【STA 线程不支持 WaitAll】：在单线程单元（STA，如 WinForms/WPF UI 线程、标记 [STAThread] 的主线程）
    ///    上调用 WaitAll 会抛 <see cref="NotSupportedException"/>；WaitOne/WaitAny 不受影响。
    ///    必须在 STA 环境用 WaitAll 时，把调用放到一个 MTA 后台线程上（本类示例即如此处理）；
    /// 2) WaitAll/WaitAny 数组元素不能为 null、不能重复，数量上限 64，否则抛异常；
    /// 3) 被放弃的 Mutex（持锁线程未 ReleaseMutex 即结束）会让等待方收到
    ///    <see cref="AbandonedMutexException"/>，通常意味着前一线程把共享状态改坏了；
    /// 4) 不要去找/捕获 System.Threading.WaitTimeoutException：它是 .NET Framework 1.0/1.1 的历史异常
    ///    类型，2.0 改版等待 API（用返回值表达超时）后已从框架移除，net48 与 .NET Core/.NET 5+ 均不存在；
    ///    本时代码判断超时只看返回值（false 或索引 258），真正可能遇到的异常是
    ///    <see cref="AbandonedMutexException"/>、<see cref="NotSupportedException"/>（STA 调 WaitAll）等；
    /// 5) SignalAndWait 在 Windows 上最多保证 64 个句柄场景正确，且跨上下文重载有 exitContext 参数，
    ///    非托管宿主/远程处理场景一般传 false；
    /// 6) 句柄是内核资源：丢失 Dispose 会依赖终结器回收，高频率创建时可能耗尽句柄；
    /// 7) 纯进程内、短等待优先用 ManualResetEventSlim/SemaphoreSlim/Monitor，内核句柄切换开销大。
    ///
    /// 【版本可用性】WaitHandle 自 .NET Framework 1.1 起提供；WaitAny/WaitAll/SignalAndWait 各超时重载、
    /// SafeWaitHandle 在 2.0/3.5 即齐全；.NET Core/.NET 5+ 均可用。本类示例基于 .NET Framework 4.8。
    /// </summary>
    /// <example>
    /// 示例1：WaitOne 带超时等待事件。
    /// <code>
    /// using (AutoResetEvent done = new AutoResetEvent(false))
    /// {
    ///     ThreadPool.QueueUserWorkItem(delegate(object state)
    ///     {
    ///         // 干活...
    ///         done.Set();
    ///     });
    ///
    ///     if (done.WaitOne(3000))
    ///     {
    ///         // 3000ms 内收到信号
    ///     }
    ///     else
    ///     {
    ///         // 超时兜底
    ///     }
    /// }
    /// </code>
    /// 示例2：WaitAny 等最先完成的任务。
    /// <code>
    /// WaitHandle[] tasks = new WaitHandle[]
    /// {
    ///     new ManualResetEvent(false),
    ///     new ManualResetEvent(true)
    /// };
    /// int finishedIndex = WaitHandle.WaitAny(tasks, 5000);
    /// if (finishedIndex == WaitHandle.WaitTimeout)
    /// {
    ///     // 5000ms 内一个都没完成（WaitTimeout 的值为 258）
    /// }
    /// foreach (WaitHandle h in tasks)
    /// {
    ///     h.Close();
    /// }
    /// </code>
    /// </example>
    public static class HWaitHandleHelp
    {
        /// <summary>
        /// 示例1：<see cref="WaitHandle.WaitAny(WaitHandle[], int)"/> 等待最先收到信号的句柄。
        /// 构造两个 <see cref="AutoResetEvent"/>，第一个构造时即为终止状态（true），第二个为非终止（false），
        /// WaitAny 必然立即返回第一个句柄的索引 0。两个句柄用 using 释放，不泄露内核资源。
        /// </summary>
        /// <returns>最先收到信号的句柄索引，本示例固定返回 0。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        public static int WaitFirstSignaled()
        {
            using (AutoResetEvent first = new AutoResetEvent(true))    // 初始即终止
            using (AutoResetEvent second = new AutoResetEvent(false))
            {
                WaitHandle[] handles = new WaitHandle[] { first, second };

                // 等待任意一个信号；1000ms 超时兜底，超时才会返回 WaitTimeout(258)
                int index = WaitHandle.WaitAny(handles, 1000);
                return index;                  // first 已终止，立即返回 0
            }
        }

        /// <summary>
        /// 示例2：<see cref="WaitHandle.WaitAll(WaitHandle[], int)"/> 等待全部句柄收到信号。
        /// 两个 <see cref="AutoResetEvent"/> 初始均为终止状态，WaitAll 立即返回 true。
        /// 若当前线程是 STA（如 UI 线程），WaitAll 会抛 <see cref="NotSupportedException"/>，
        /// 本方法自动切换到 MTA 工作线程执行，保证任何调用环境下都可运行。
        /// </summary>
        /// <returns>1000ms 内所有句柄均收到信号返回 true，本示例固定返回 true。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        public static bool WaitAllAlreadySignaled()
        {
            using (AutoResetEvent first = new AutoResetEvent(true))
            using (AutoResetEvent second = new AutoResetEvent(true))
            {
                WaitHandle[] handles = new WaitHandle[] { first, second };

                // 两个句柄都已终止：所有对象同时满足，返回 true
                return WaitAllOnMtaThread(handles, 1000);
            }
        }

        /// <summary>
        /// 示例3：WaitAll 超时返回 false。两个句柄初始均为非终止状态且全程无人 Set，
        /// WaitAll 等待 100ms 后超时返回 false（不抛异常）。同样自动规避 STA 限制。
        /// </summary>
        /// <returns>超时未全部收到信号，固定返回 false。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        public static bool WaitAllTimeoutReturnsFalse()
        {
            using (AutoResetEvent first = new AutoResetEvent(false))
            using (AutoResetEvent second = new AutoResetEvent(false))
            {
                WaitHandle[] handles = new WaitHandle[] { first, second };

                // 没有任何线程 Set 这两个句柄：100ms 后超时，返回 false 而不是抛异常
                return WaitAllOnMtaThread(handles, 100);
            }
        }

        /// <summary>
        /// 示例4：<see cref="WaitHandle.SignalAndWait(WaitHandle, WaitHandle, int, bool)"/> 原子交接。
        /// 主线程原子地“置位第一个句柄并等待第二个句柄”；工作线程等到第一个句柄后置位第二个句柄放行主线程。
        /// 工作线程为后台线程并用 Join(2000) 保底回收，任何时序下本方法都会在 2 秒内结束。
        /// </summary>
        /// <returns>交接成功（主线程在 1000ms 内收到第二个句柄信号且工作线程正常结束）返回 true。</returns>
        /// <exception cref="Exception">本方法不主动引发异常。</exception>
        public static bool SignalAndWaitDemo()
        {
            using (ManualResetEvent first = new ManualResetEvent(false))
            using (ManualResetEvent second = new ManualResetEvent(false))
            {
                bool workerFinished = false;

                Thread worker = new Thread((ThreadStart)delegate
                {
                    // 等待主线程在 SignalAndWait 中把 first 置位
                    if (first.WaitOne(1000))
                    {
                        second.Set();          // 置位 second，放行正在等待的主线程
                    }
                    workerFinished = true;
                }) { IsBackground = true };
                worker.Start();

                // 一个内核调用内完成两件事：通知 first + 等待 second，超时 1000ms，非远程宿主传 false
                bool signaled = WaitHandle.SignalAndWait(first, second, 1000, false);

                worker.Join(2000);             // 带超时回收工作线程
                return signaled && workerFinished;
            }
        }

        /// <summary>
        /// 在 MTA 线程上执行 <see cref="WaitHandle.WaitAll(WaitHandle[], int)"/> 的兜底包装：
        /// STA 线程（UI 线程等）调用 WaitAll 会抛 <see cref="NotSupportedException"/>，
        /// 检测到 STA 时把调用搬到显式标记为 MTA 的短生命周期工作线程上执行。
        /// </summary>
        /// <param name="handles">要等待的句柄数组。</param>
        /// <param name="timeoutMs">等待超时毫秒数。</param>
        /// <returns>超时前全部收到信号为 true；超时或工作线程未能在 2000ms 内结束为 false。</returns>
        private static bool WaitAllOnMtaThread(WaitHandle[] handles, int timeoutMs)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                // 当前本来就是 MTA/未知单元：直接等待
                return WaitHandle.WaitAll(handles, timeoutMs);
            }

            bool result = false;
            Thread worker = new Thread((ThreadStart)delegate
            {
                // MTA 线程上 WaitAll 受支持
                result = WaitHandle.WaitAll(handles, timeoutMs);
            });
            worker.SetApartmentState(ApartmentState.MTA);
            worker.Start();

            if (!worker.Join(2000))
            {
                // 理论上不会走到（内部等待有超时），保底返回 false 防止调用方挂住
                return false;
            }
            return result;
        }
    }
}
