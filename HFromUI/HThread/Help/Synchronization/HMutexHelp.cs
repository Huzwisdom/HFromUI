using System;
using System.Threading;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】Mutex（互斥体）帮助类：基于操作系统内核等待句柄（<see cref="WaitHandle"/>）的排他锁。
    /// 分为未命名的"本地互斥体"（仅进程内）与命名的"系统互斥体"（操作系统全局可见）。
    /// 【是否跨进程】是。命名 Mutex 由操作系统内核对象支撑，可被同一机器上的多个进程打开共享；
    /// 名称前缀 <c>Global\</c> 表示跨终端服务会话全局可见，<c>Local\</c>（默认）表示仅当前会话。
    /// 未命名 Mutex 不跨进程，此时进程内互斥应优先用 <c>lock</c>（Monitor 是混合锁，开销小得多）。
    /// 【典型适用场景】
    /// <list type="bullet">
    /// <item>单实例程序检测：<c>new Mutex(true, 名称, out createdNew)</c>，createdNew=false 说明已有实例。</item>
    /// <item>跨进程/跨会话的临界区保护（共享文件、共享硬件、与非托管程序互斥）。</item>
    /// </list>
    /// 【使用步骤】
    /// <list type="number">
    /// <item>构造：未命名用 <c>new Mutex()</c>；命名用 <c>new Mutex(false/true, name, out createdNew)</c>。</item>
    /// <item>请求所有权：<c>WaitOne()</c> / <c>WaitOne(int 毫秒)</c> / <c>WaitOne(TimeSpan)</c>（均带超时版本）。</item>
    /// <item>访问受保护资源。</item>
    /// <item>在 finally 中调用 <see cref="Mutex.ReleaseMutex"/>，并用 using 保证 <see cref="Mutex.Dispose"/>。</item>
    /// <item>打开他人创建的命名互斥体：<see cref="Mutex.OpenExisting"/> 或
    /// <see cref="Mutex.TryOpenExisting"/>（后者 .NET Framework 4.5+）。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>Mutex 强制线程标识：哪个线程 WaitOne 拿到，就必须由同一个线程 ReleaseMutex，
    /// 否则抛 <see cref="ApplicationException"/>；可重入，但 WaitOne 几次就要 ReleaseMutex 几次。</item>
    /// <item>持锁线程异常终止（如被强制 Kill）会"遗弃"互斥体，下一个获取者收到
    /// <see cref="AbandonedMutexException"/>——这通常意味着受保护数据可能已损坏，需校验一致性。</item>
    /// <item>WaitOne 超时返回 false 时从未获得所有权，绝不能调用 ReleaseMutex。</item>
    /// <item>反斜杠 <c>\</c> 是名称保留字符，除 <c>Global\</c>/<c>Local\</c> 前缀外不要在名称中使用。</item>
    /// <item>命名内核对象默认不限制访问用户，不受信任用户环境下需配合 MutexSecurity 做访问控制。</item>
    /// <item>每次进入内核等待都有用户态/内核态切换开销，进程内短临界区不要用 Mutex。</item>
    /// </list>
    /// 【版本可用性】Mutex 自 .NET Framework 1.1（2.0 完善）；OpenExisting 为 2.0 起；
    /// TryOpenExisting 为 4.5 新增；带 MutexSecurity 的构造/访问控制重载在 2.0+ 可用；
    /// 带 NamedWaitHandleOptions 的新重载属于更高版本 .NET，4.8 不可用。
    /// </summary>
    /// <example>
    /// <code>
    /// bool createdNew;
    /// using (Mutex mutex = new Mutex(true, @"Global\MyApp", out createdNew))
    /// {
    ///     if (!createdNew) { /* 已有实例运行，退出 */ return; }
    ///     try { /* 主程序 */ }
    ///     finally { mutex.ReleaseMutex(); }   // 必须同线程释放
    /// }
    /// </code>
    /// </example>
    public static class HMutexHelp
    {
        /// <summary>示例1：单实例程序检测——命名 Mutex 跨进程互斥，createdNew=false 说明已有实例在运行。</summary>
        /// <param name="mutexName">互斥体名称；建议包含公司/产品名等唯一片段，可用 <c>Global\</c>/<c>Local\</c> 前缀。</param>
        /// <param name="mutex">输出：新建/打开的命名互斥体；调用方负责在程序退出时 ReleaseMutex 并 Dispose。</param>
        /// <returns>true 表示本进程是首个实例（已获得初始所有权）；false 表示同名实例已存在。</returns>
        /// <exception cref="UnauthorizedAccessException">命名互斥体存在但当前进程无访问权限。</exception>
        /// <exception cref="WaitHandleCannotBeOpenedException">名称无效或与其他类型内核对象同名。</exception>
        public static bool TrySingleInstance(string mutexName, out Mutex mutex)
        {
            bool createdNew;
            mutex = new Mutex(true, mutexName, out createdNew);  // initiallyOwned=true：新创建即归本线程所有
            return createdNew;
        }

        /// <summary>
        /// 示例2：跨进程临界区：进入保护区用带毫秒超时的 WaitOne，离开在 finally 中 ReleaseMutex；
        /// using 保证内核句柄释放。超时返回 false 时不获得所有权，因此不调用 ReleaseMutex。
        /// </summary>
        /// <param name="mutexName">命名互斥体名称（不存在会自动创建）。</param>
        /// <param name="timeoutMs">等待毫秒数；0 不等待，-1 无限等待（示例不建议）。</param>
        /// <param name="work">受互斥体保护的工作。</param>
        /// <returns>true 表示成功进入并执行完毕；false 表示超时未获得互斥体。</returns>
        /// <exception cref="AbandonedMutexException">上一线程持锁终止；异常被本方法向上抛出，提示数据可能不一致。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        public static bool CrossProcessSection(string mutexName, int timeoutMs, Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            using (Mutex mutex = new Mutex(false, mutexName))   // initiallyOwned=false：创建了也不先占有
            {
                if (!mutex.WaitOne(timeoutMs))
                {
                    return false;                                 // 其他进程占用且超时：未获所有权，禁止 ReleaseMutex
                }
                try
                {
                    work();
                }
                finally
                {
                    mutex.ReleaseMutex();                         // 必须由执行 WaitOne 的同一线程调用
                }
            }
            return true;
        }

        /// <summary>示例3：全局单实例模板（Program.Main 中调用）；返回 null 表示已有实例，调用方应退出。</summary>
        /// <param name="appName">应用唯一名（不含前缀）。</param>
        /// <returns>持有初始所有权的全局 Mutex；已有实例时返回 null。调用方负责在程序退出时释放并 Dispose。</returns>
        public static Mutex CreateGlobalSingleInstance(string appName)
        {
            // Global\ 前缀表示跨终端服务会话（多用户/服务）全局唯一；默认 Local\ 仅当前会话
            bool createdNew;
            Mutex mutex = new Mutex(true, @"Global\" + appName, out createdNew);
            return createdNew ? mutex : null;
        }

        /// <summary>示例4：未命名（本地）互斥体 —— 仅进程内可见。演示 <c>new Mutex()</c> 与无参 WaitOne 的标准配对。</summary>
        /// <param name="timeoutMs">等待毫秒数。</param>
        /// <param name="work">受保护的工作。</param>
        /// <returns>true 表示执行成功；false 表示超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        public static bool UnnamedMutexExample(int timeoutMs, Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            using (Mutex mutex = new Mutex())          // 未命名：纯进程内核对象
            {
                if (!mutex.WaitOne(timeoutMs))
                {
                    return false;
                }
                try
                {
                    work();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return true;
        }

        /// <summary>示例5：<see cref="WaitHandle.WaitOne(TimeSpan)"/> 以 <see cref="TimeSpan"/> 表达超时的重载。</summary>
        /// <param name="mutexName">命名互斥体名称。</param>
        /// <param name="timeout">超时时长。</param>
        /// <param name="work">受保护的工作。</param>
        /// <returns>true 表示执行成功；false 表示超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 超出合法范围。</exception>
        public static bool WaitOneTimeSpanExample(string mutexName, TimeSpan timeout, Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            using (Mutex mutex = new Mutex(false, mutexName))
            {
                if (!mutex.WaitOne(timeout))
                {
                    return false;
                }
                try
                {
                    work();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return true;
        }

        /// <summary>
        /// 示例6：<see cref="Mutex.OpenExisting"/> 打开其他进程创建的命名互斥体；不存在时抛
        /// <see cref="WaitHandleCannotBeOpenedException"/>，无权限时抛 <see cref="UnauthorizedAccessException"/>。
        /// </summary>
        /// <param name="mutexName">已存在的命名互斥体名称。</param>
        /// <returns>打开的互斥体；不存在或无权限时返回 null（调用方负责 Dispose 返回的对象）。</returns>
        public static Mutex OpenExistingExample(string mutexName)
        {
            try
            {
                return Mutex.OpenExisting(mutexName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                return null;                     // 同名互斥体不存在
            }
            catch (UnauthorizedAccessException)
            {
                return null;                     // 存在但当前进程无访问权限
            }
        }

        /// <summary>
        /// 示例7：<see cref="Mutex.TryOpenExisting"/>（.NET Framework 4.5+）以 Try 模式打开，
        /// 不存在直接返回 false，避免异常控制流。
        /// </summary>
        /// <param name="mutexName">命名互斥体名称。</param>
        /// <param name="mutex">输出：打开成功时为互斥体实例（调用方负责 Dispose）；失败为 null。</param>
        /// <returns>true 表示打开成功；false 表示不存在（无权限仍会抛 <see cref="UnauthorizedAccessException"/>）。</returns>
        public static bool TryOpenExistingExample(string mutexName, out Mutex mutex)
        {
            return Mutex.TryOpenExisting(mutexName, out mutex);
        }

        /// <summary>
        /// 示例8：正确处理 <see cref="AbandonedMutexException"/>。持锁线程异常终止后，下一个等待者会捕获该异常，
        /// 且互斥体所有权已授予当前线程——仍须在 finally 中 ReleaseMutex，但应先校验受保护资源的一致性。
        /// 本方法只演示处理骨架，不会真正制造遗弃。
        /// </summary>
        /// <param name="mutex">已构造的互斥体。</param>
        /// <param name="timeoutMs">等待毫秒数。</param>
        /// <param name="work">获得互斥体后的工作（可在其中校验数据一致性）。</param>
        /// <returns>true 表示正常获得并执行；false 表示超时放弃。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="mutex"/> 或 <paramref name="work"/> 为 null。</exception>
        public static bool WaitWithAbandonedHandling(Mutex mutex, int timeoutMs, Action work)
        {
            if (mutex == null)
            {
                throw new ArgumentNullException(nameof(mutex));
            }
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            bool acquired = false;
            bool abandoned = false;
            try
            {
                acquired = mutex.WaitOne(timeoutMs);
            }
            catch (AbandonedMutexException)
            {
                // 上一线程持锁终止：所有权已转交给本线程，先标记再校验数据
                acquired = true;
                abandoned = true;
            }

            if (!acquired)
            {
                return false;                    // 超时：未获所有权，不能 ReleaseMutex
            }

            try
            {
                if (abandoned)
                {
                    // 此处应检查共享资源是否处于一致状态，再决定继续或修复
                }
                work();
            }
            finally
            {
                mutex.ReleaseMutex();            // 即使是遗弃接手，也要正常释放
            }
            return true;
        }
    }
}
