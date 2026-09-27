using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】<see cref="IAsyncDisposable"/> 帮助类：异步释放资源的统一契约，接口只有一个方法
    /// <see cref="IAsyncDisposable.DisposeAsync"/>，返回 <see cref="ValueTask"/>（无结果值）。
    /// 当资源释放过程本身需要异步 I/O（关闭网络连接、Flush 缓冲、异步取消订阅、释放内部同样
    /// 只有异步释放路径的对象）时，同步 <see cref="IDisposable.Dispose"/> 只能阻塞线程硬等，
    /// DisposeAsync 则可以真正异步完成。接口契约：①必须【幂等】——可被多次调用，第二次及以后
    /// 直接返回已完成的 ValueTask，不得重复释放；②除了严重错误（如线程终止、上下文损坏），
    /// 实现中不应抛异常——尤其重复调用绝不能抛 <see cref="ObjectDisposedException"/>；
    /// ③只表示“释放”，不保证等待所有挂起操作完成（那是各资源自己的语义）。
    ///
    /// 【是否跨进程】否。这是进程内资源生命周期契约，与跨进程无关。
    ///
    /// 【典型适用场景】
    /// 1) 持有者内部包含 IAsyncDisposable 子资源（异步流枚举器、Socket/管道、异步文件句柄）；
    /// 2) 关闭时必须先做异步 Flush/取消请求再释放的客户端、通道、会话对象；
    /// 3) 同时实现 IDisposable：同步路径给老代码/using 块用，异步路径给 async 代码用，
    ///    两条路径都必须正确且互相幂等（任一先调，另一个再调都是安全空操作）。
    ///
    /// 【使用步骤（IDisposable + IAsyncDisposable 双实现标准模式）】
    /// 1) public async ValueTask DisposeAsync()：先 await DisposeAsyncCore().ConfigureAwait(false)
    ///    做异步托管资源清理，再调 Dispose(disposing: false) 释放非托管部分，最后 GC.SuppressFinalize(this)；
    /// 2) protected virtual ValueTask DisposeAsyncCore()：子类只重写它来追加自己的异步清理，
    ///    内部用“原子摘走引用 + null 判定”保证只释放一次；
    /// 3) public void Dispose() 调 Dispose(disposing: true) 再 SuppressFinalize；
    /// 4) protected virtual void Dispose(bool disposing)：disposing=true 时同步释放托管资源，
    ///    两条路径通过 disposed 标志/交换引用保证互不重复；
    /// 5) 消费方（本工程 C# 7.3，没有 await using）手写 try/finally：
    ///    await resource.DisposeAsync().ConfigureAwait(false)。
    ///
    /// 【注意事项与坑】
    /// 1) C# 8.0 的 await using 语法与 using 声明（using var x = ...;）在本工程固定的 C# 7.3
    ///    下都不可用；net48 手写 await x.DisposeAsync().ConfigureAwait(false) 与 await using 完全等价；
    /// 2) DisposeAsync 内部 await 之后再访问实例字段要想到对象可能已被并发释放——清理要幂等、原子；
    /// 3) 同一对象 Dispose() 与 DisposeAsync() 都可能被调用：先到的路径真正释放，后到的必须为空操作；
    /// 4) 不要在 DisposeAsync 里用 Result/.Wait() 阻塞等待，那会抵消异步意义，还可能死锁；
    /// 5) DisposeAsync 返回的 ValueTask 同样只能 await 一次；契约要求重复调用返回的是可正常 await
    ///    的已完成 ValueTask，而不是“把上次那个 ValueTask 再 await 一遍”；
    /// 6) 只有同步释放需求时不要硬加 IAsyncDisposable——实现双模式是有维护成本的。
    ///
    /// 【版本可用性】<see cref="IAsyncDisposable"/> 原生属于 C# 8.0 / .NET Standard 2.1 /
    /// .NET Core 3.0+；本工程 net48 通过 NuGet 包 Microsoft.Bcl.AsyncInterfaces（已引用 10.0.9）
    /// 获得该接口，DisposeAsync 返回的 <see cref="ValueTask"/> 由已引用的
    /// System.Threading.Tasks.Extensions（4.6.3）提供，net48 下类型真实可编译、可 await；
    /// 但 await using 属于 C# 8.0 语言特性，本工程 LangVersion 为 7.3，故只手写 try/finally。
    /// </summary>
    /// <example>
    /// 典型异步资源持有者：同时实现 IDisposable 与 IAsyncDisposable 的完整双实现模式
    /// （本示例可直接照抄，已按 C# 7.3 语法书写，泛型与比较符均已 XML 转义）：
    /// <code>
    /// using System;
    /// using System.Threading;
    /// using System.Threading.Tasks;
    ///
    /// public class AsyncHolder : IDisposable, IAsyncDisposable
    /// {
    ///     private ManualResetEventSlim innerResource;   // 模拟需要释放的内部句柄
    ///     private int disposed;
    ///
    ///     public AsyncHolder()
    ///     {
    ///         innerResource = new ManualResetEventSlim(false);
    ///     }
    ///
    ///     // ① 异步释放入口：先异步清理托管资源，再走同步 Dispose 的非托管分支
    ///     public async ValueTask DisposeAsync()
    ///     {
    ///         await DisposeAsyncCore().ConfigureAwait(false);
    ///         Dispose(false);
    ///         GC.SuppressFinalize(this);
    ///     }
    ///
    ///     // ② 子类重写这里追加异步清理；原子摘引用保证只释放一次
    ///     protected virtual async ValueTask DisposeAsyncCore()
    ///     {
    ///         ManualResetEventSlim resource = Interlocked.Exchange(ref innerResource, null);
    ///         if (resource != null)
    ///         {
    ///             await Task.Delay(1).ConfigureAwait(false);   // 真实代码：await 子资源.DisposeAsync()
    ///             resource.Dispose();
    ///         }
    ///     }
    ///
    ///     // ③ 同步释放入口（老代码 using 块走这条）
    ///     public void Dispose()
    ///     {
    ///         Dispose(true);
    ///         GC.SuppressFinalize(this);
    ///     }
    ///
    ///     // ④ 真正的清理逻辑，两条路径共用，幂等
    ///     protected virtual void Dispose(bool disposing)
    ///     {
    ///         if (Interlocked.Exchange(ref disposed, 1) != 0)
    ///         {
    ///             return;
    ///         }
    ///         if (disposing)
    ///         {
    ///             ManualResetEventSlim resource = Interlocked.Exchange(ref innerResource, null);
    ///             if (resource != null)
    ///             {
    ///                 resource.Dispose();
    ///             }
    ///         }
    ///     }
    /// }
    ///
    /// // 消费方（C# 7.3 手写 try/finally，等价 C# 8 的 await using）：
    /// AsyncHolder holder = new AsyncHolder();
    /// try
    /// {
    ///     await Task.Delay(1).ConfigureAwait(false);   // 使用资源
    /// }
    /// finally
    /// {
    ///     await holder.DisposeAsync().ConfigureAwait(false);
    /// }
    /// </code>
    /// </example>
    public static class HIAsyncDisposableHelp
    {
        /// <summary>
        /// 示例1（自包含）：创建 <see cref="AsyncResource"/>，做一点工作，然后异步释放，
        /// 返回内部记录的真实释放次数（正确实现下应为 1）。
        /// </summary>
        /// <returns>DisposeAsync 完成后 <see cref="AsyncResource.DisposeCalled"/> 的值；释放恰好发生一次为 1。</returns>
        public static async Task<int> UseAndDisposeAsync()
        {
            AsyncResource resource = new AsyncResource();
            bool usable = resource.DoWork();          // 释放前使用：句柄可用，返回 true
            Console.WriteLine(HTranslation.GetContent("释放前资源可用：") + usable);

            // C# 7.3 没有 await using：手写 await DisposeAsync（等价 await using 块结束时的回调）
            await resource.DisposeAsync().ConfigureAwait(false);

            return resource.DisposeCalled;            // 验证值：真实释放动作恰好 1 次
        }

        /// <summary>
        /// 示例2（自包含）：幂等性验证。对同一资源连续调用两次 DisposeAsync，
        /// 内部句柄只允许被真实释放一次，第二次调用必须是安全的空操作（不抛异常、不重复计数）。
        /// </summary>
        /// <returns>两次 DisposeAsync 都正常完成且真实释放次数恰好为 1 时返回 true。</returns>
        public static async Task<bool> DoubleDisposeIsIdempotentAsync()
        {
            AsyncResource resource = new AsyncResource();
            resource.DoWork();

            await resource.DisposeAsync().ConfigureAwait(false);
            await resource.DisposeAsync().ConfigureAwait(false);   // 契约：再次调用必须安全且不重复释放

            // 幂等：调用两次，真实释放只发生一次；释放后再使用也不应抛异常（返回 false 表示句柄已不在）
            bool usableAfterDispose = resource.DoWork();
            return resource.DisposeCalled == 1 && !usableAfterDispose;
        }

        /// <summary>
        /// 教学用异步资源：同时实现 <see cref="IDisposable"/> 与 <see cref="IAsyncDisposable"/>
        /// 的标准双实现模式。内部持有一个 <see cref="ManualResetEventSlim"/> 模拟需要释放的句柄，
        /// 用 <see cref="Interlocked"/> 原子摘引用 + disposed 标志保证两条释放路径都只释放一次。
        /// </summary>
        public class AsyncResource : IDisposable, IAsyncDisposable
        {
            // 模拟“非托管/内部句柄”：真实代码里可以是网络连接、异步文件句柄等
            private ManualResetEventSlim innerHandle;

            // 0=未释放，1=已释放；Interlocked.Exchange 保证 Dispose(bool) 幂等
            private int disposed;

            // 真实释放动作发生的次数（教学/自测验证用）：正确实现永远只可能是 0 或 1
            private int disposeCalledCount;

            /// <summary>创建资源：内部初始化一个 Slim 手动事件作为模拟句柄。</summary>
            public AsyncResource()
            {
                innerHandle = new ManualResetEventSlim(false);
            }

            /// <summary>
            /// 真实释放动作发生过几次。无论 Dispose/DisposeAsync 被调多少次，正确实现下只可能为 0 或 1。
            /// </summary>
            public int DisposeCalled
            {
                get { return Volatile.Read(ref disposeCalledCount); }
            }

            /// <summary>
            /// 做一点工作：释放前句柄可用（探测等待立即返回）返回 true；释放后句柄已摘走返回 false，不抛异常。
            /// </summary>
            /// <returns>资源尚未释放返回 true；已释放返回 false。</returns>
            public bool DoWork()
            {
                ManualResetEventSlim handle = Volatile.Read(ref innerHandle);
                if (handle == null)
                {
                    return false;                      // 已释放：幂等降级，而不是抛 ObjectDisposedException
                }
                handle.Wait(0);                        // 句柄可用：零超时探测，做一点工作
                return true;
            }

            /// <summary>
            /// 异步释放（<see cref="IAsyncDisposable.DisposeAsync"/>）：先走异步核心清理，
            /// 再以 disposing=false 调同步 Dispose 释放非托管部分，最后禁止终结化。
            /// </summary>
            /// <returns>表示释放完成的 <see cref="ValueTask"/>；重复调用返回已完成实例，不重复释放。</returns>
            public async ValueTask DisposeAsync()
            {
                // 标准模式：托管异步资源在这里 await 清理（可被子类重写的核心方法）
                await DisposeAsyncCore().ConfigureAwait(false);

                // 异步路径已经释放过托管资源：false 表示只处理非托管部分，避免重复释放
                Dispose(false);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 异步释放核心：子类重写本方法追加自己的异步清理。原子摘走内部句柄，
            /// 保证同步/异步两条路径无论谁先调用，真实清理都只执行一次。
            /// </summary>
            /// <returns>表示异步清理完成的 <see cref="ValueTask"/>。</returns>
            protected virtual async ValueTask DisposeAsyncCore()
            {
                ManualResetEventSlim handle = Interlocked.Exchange(ref innerHandle, null);
                if (handle != null)
                {
                    // 只有第一个调用者能摘到非 null 句柄：这里模拟真实的异步关闭（如 Flush、关连接）
                    await Task.Delay(1).ConfigureAwait(false);
                    handle.Set();
                    handle.Dispose();
                    Interlocked.Increment(ref disposeCalledCount);
                }
            }

            /// <summary>
            /// 同步释放（<see cref="IDisposable.Dispose"/>）：供 using 块等同步调用方使用。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);                         // 同步路径：托管与非托管都在这里释放
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 真正的清理逻辑（标准同步模式）。disposing=true 表示由 Dispose() 调用，可释放托管对象；
            /// false 表示由 DisposeAsync 回调，托管资源已在 DisposeAsyncCore 中释放。
            /// </summary>
            /// <param name="disposing">true=同步 Dispose 路径；false=异步释放后的非托管兜底路径。</param>
            protected virtual void Dispose(bool disposing)
            {
                if (Interlocked.Exchange(ref disposed, 1) != 0)
                {
                    return;                            // 幂等：第二次起直接返回
                }

                if (disposing)
                {
                    // 若 DisposeAsync 已先执行，这里拿到的是 null，不会重复释放
                    ManualResetEventSlim handle = Interlocked.Exchange(ref innerHandle, null);
                    if (handle != null)
                    {
                        handle.Dispose();
                        Interlocked.Increment(ref disposeCalledCount);
                    }
                }
            }
        }
    }
}
