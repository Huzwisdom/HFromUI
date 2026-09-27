using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Synchronization
{
    /// <summary>
    /// 【是什么】SemaphoreSlim（轻量信号量）帮助类：Semaphore 的纯托管混合实现。先在用户态自旋/等待，
    /// 必要时才惰性分配内核等待句柄，不强制线程身份。最大特点是提供 <c>WaitAsync</c> 系列异步重载，
    /// 可在 async/await 链路中不阻塞线程地限流。
    /// 【是否跨进程】否。纯进程内构造，不能命名、不能跨进程共享；跨进程请用 <see cref="Semaphore"/>。
    /// 【典型适用场景】
    /// <list type="bullet">
    /// <item>异步任务并发度限制（同时最多 N 个 HTTP 请求、数据库连接、文件句柄），是标准做法。</item>
    /// <item>进程内同步限流，且希望避免内核信号量开销。</item>
    /// </list>
    /// 【使用步骤】
    /// <list type="number">
    /// <item>构造 <c>new SemaphoreSlim(initialCount)</c> 或 <c>new SemaphoreSlim(initialCount, maxCount)</c>。</item>
    /// <item>同步：Wait()/Wait(int)/Wait(TimeSpan)/Wait(CancellationToken)；
    /// 异步：WaitAsync()/WaitAsync(int)/WaitAsync(TimeSpan)/WaitAsync(CancellationToken)/WaitAsync(int, CancellationToken)。</item>
    /// <item>finally 中 Release() 或 Release(n)。</item>
    /// <item>用完 Dispose；若使用过 <see cref="SemaphoreSlim.AvailableWaitHandle"/>，该句柄随实例一起释放。</item>
    /// </list>
    /// 【注意事项与坑】
    /// <list type="bullet">
    /// <item>异步等待返回时不保证在原线程，Release 可由任意线程调用（无线程身份约束）。</item>
    /// <item>Release 后计数超过 maxCount 抛 <see cref="SemaphoreFullException"/>。</item>
    /// <item>Dispose 后再 Wait/Release 抛 <see cref="ObjectDisposedException"/>；
    /// 有等待者时 Dispose 的行为未定义，务必先保证无人使用。</item>
    /// <item><see cref="SemaphoreSlim.AvailableWaitHandle"/> 首次访问才创建内核句柄，只在需要把它放入
    /// WaitHandle.WaitAny/WaitAll 数组时才用，纯 WaitAsync 场景不要碰（白付内核句柄开销）。</item>
    /// <item>Wait 系列支持 <see cref="CancellationToken"/>，取消时抛 <see cref="OperationCanceledException"/>。</item>
    /// <item>SemaphoreSlim 不保证等待者的 FIFO 公平性，不要依赖唤醒顺序。</item>
    /// </list>
    /// 【版本可用性】SemaphoreSlim 自 .NET Framework 4.0；WaitAsync 全部重载为 4.5 新增；
    /// AvailableWaitHandle、CurrentCount、Release(int) 在 4.0 即有。
    /// </summary>
    public static class HSemaphoreSlimHelp
    {
        /// <summary>示例1：同步限流（新建信号量名额充足，本示例立即获得，不会阻塞）。</summary>
        /// <param name="maxCount">最大并发数，必须大于 0。</param>
        /// <param name="work">受限流保护的工作。</param>
        /// <exception cref="ArgumentNullException"><paramref name="work"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCount"/> 小于 1。</exception>
        public static void LimitSync(int maxCount, Action work)
        {
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }
            if (maxCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            }

            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxCount, maxCount))
            {
                semaphore.Wait();
                try
                {
                    work();
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        /// <summary>示例2：异步限流（async/await 中不占线程），可通过取消令牌取消。</summary>
        /// <param name="semaphore">信号量实例（调用方负责生命周期与 Dispose）。</param>
        /// <param name="work">返回 Task 的异步工作。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>表示限流执行过程的 Task。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 或 <paramref name="work"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException">等待期间 <paramref name="token"/> 被取消。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="semaphore"/> 已释放。</exception>
        public static async Task LimitAsync(SemaphoreSlim semaphore, Func<Task> work, CancellationToken token = default)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            await semaphore.WaitAsync(token);       // 拿不到通行证时异步等待，不占线程
            try
            {
                await work();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>示例3：给一批异步任务统一限并发（如同时最多 4 个 HTTP 请求）。</summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="source">待处理元素序列。</param>
        /// <param name="maxConcurrency">最大并发数，必须大于 0。</param>
        /// <param name="body">处理单个元素的异步委托。</param>
        /// <returns>全部元素处理完成的 Task。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> 或 <paramref name="body"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxConcurrency"/> 小于 1。</exception>
        public static async Task ForEachLimitedAsync<T>(IEnumerable<T> source, int maxConcurrency, Func<T, Task> body)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }
            if (maxConcurrency <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxConcurrency));
            }

            using (SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency))
            {
                async Task ProcessItem(T item)
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await body(item);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
                await Task.WhenAll(source.Select(ProcessItem));
            }
        }

        /// <summary>示例4：读取 <see cref="SemaphoreSlim.CurrentCount"/>（当前剩余通行证数，监控/调试用）。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <returns>当前可立即授予的通行证数量。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 为 null。</exception>
        public static int CurrentCount(SemaphoreSlim semaphore)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            return semaphore.CurrentCount;
        }

        /// <summary>示例5：同步 Wait 的毫秒超时重载——拿不到名额不阻塞，返回 false 走降级路径。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="timeoutMs">等待毫秒数；0 不等待。</param>
        /// <param name="work">拿到名额后的工作。</param>
        /// <returns>true 执行成功；false 超时未拿到（不会 Release）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 或 <paramref name="work"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeoutMs"/> 非法（-1 以外的负数）。</exception>
        public static bool WaitTimeoutExample(SemaphoreSlim semaphore, int timeoutMs, Action work)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            if (work == null)
            {
                throw new ArgumentNullException(nameof(work));
            }

            if (!semaphore.Wait(timeoutMs))
            {
                return false;
            }
            try
            {
                work();
            }
            finally
            {
                semaphore.Release();
            }
            return true;
        }

        /// <summary>示例6：异步 WaitAsync 的毫秒超时重载，超时返回 false，绝不无限挂起。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="timeoutMs">等待毫秒数；0 不等待。</param>
        /// <returns>true 表示拿到名额（调用方随后负责 Release）；false 表示超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 为 null。</exception>
        public static async Task<bool> WaitAsyncTimeoutExample(SemaphoreSlim semaphore, int timeoutMs)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            return await semaphore.WaitAsync(timeoutMs);
        }

        /// <summary>示例7：可取消的异步等待（WaitAsync(int, CancellationToken) 重载组合）。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="timeoutMs">等待毫秒数。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>true 拿到名额；false 超时。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 为 null。</exception>
        /// <exception cref="OperationCanceledException">等待被取消。</exception>
        public static async Task<bool> WaitAsyncCancellableExample(SemaphoreSlim semaphore, int timeoutMs, CancellationToken token)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            return await semaphore.WaitAsync(timeoutMs, token);
        }

        /// <summary>示例8：一次归还多个名额 <see cref="SemaphoreSlim.Release(int)"/>。</summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="releaseCount">归还数量，必须为正数。</param>
        /// <returns>释放前的剩余计数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="semaphore"/> 为 null。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="releaseCount"/> 小于 1。</exception>
        /// <exception cref="SemaphoreFullException">归还后超过 maxCount。</exception>
        public static int ReleaseMany(SemaphoreSlim semaphore, int releaseCount)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }
            return semaphore.Release(releaseCount);
        }

        /// <summary>
        /// 示例9：<see cref="SemaphoreSlim.AvailableWaitHandle"/> 惰性创建的内核句柄，可放进
        /// WaitHandle.WaitAny/WaitAll 与其他句柄一起等。本示例用带超时的 WaitOne 观察，可安全自测。
        /// 注意：句柄生命周期归 SemaphoreSlim 所有，不要单独 Close/Dispose 它。
        /// </summary>
        /// <param name="semaphore">信号量实例。</param>
        /// <param name="timeoutMs">等待毫秒数。</param>
        /// <returns>true 表示在超时内有名额可用；false 表示超时。</returns>
        /// <exception cref="ArgumentNullException"><parameter name="semaphore"/> 为 null。</exception>
        public static bool AvailableWaitHandleExample(SemaphoreSlim semaphore, int timeoutMs)
        {
            if (semaphore == null)
            {
                throw new ArgumentNullException(nameof(semaphore));
            }

            WaitHandle handle = semaphore.AvailableWaitHandle;   // 首次访问才分配内核句柄
            return handle.WaitOne(timeoutMs);
        }
    }
}
