using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】ConfigureAwait 帮助类：控制 await 等待结束后【续体是否回到原先捕获的同步上下文】。
    /// await 的底层机制：挂起前状态机会调用 SynchronizationContext.Current 抓一份上下文
    /// （UI 线程上是 WinForms/WPF 的 UI 上下文；控制台/线程池通常为 null）；任务完成时：
    /// - ConfigureAwait(true)（与不写等价，默认值）：尝试把续体 Post 回那份上下文。
    ///   UI 程序因此 await 后自动回到 UI 线程；但如果 UI 线程正被阻塞，续体只能排队等；
    /// - ConfigureAwait(false)：不抓/不用上下文，续体直接在线程池线程上跑，
    ///   少一次上下文封送、且不会因为原上下文被占满而死锁。
    /// 注意：ConfigureAwait 只配置【这一跳】；它不影响被调异步方法内部、也不沿调用链向下传播。
    ///
    /// 【是否跨进程】否。
    ///
    /// 【典型适用场景】
    /// 1) 类库（DLL）代码：每一个 await 一律 ConfigureAwait(false)——库不应依赖调用方是什么上下文；
    /// 2) UI 事件处理最外层：不写 ConfigureAwait（=true），await 后直接操作控件；
    /// 3) ASP.NET Core/控制台/线程池：本来就没有同步上下文，写不写行为相同，但库仍应写以便复用。
    ///
    /// 【使用步骤】
    /// 1) 库内每个 await 后链式调用 ConfigureAwait(false)，包括最后一跳和 using 内的等待；
    /// 2) UI 层调用库方法处无需特殊处理，外层不写 ConfigureAwait 即可保证回到 UI 线程；
    /// 3) 需要在 await 后更新控件的语句不要放在 ConfigureAwait(false) 之后（那时可能已在非 UI 线程）。
    ///
    /// 【注意事项与坑】
    /// 1) 经典死锁链条：UI 线程执行 SomeAsync().Result/.Wait() 阻塞 UI 线程；该异步方法内部
    ///   ConfigureAwait(true) 等待回 UI 上下文执行续体 → 两边互等 → 死锁。
    ///   库内全部 ConfigureAwait(false) 可让续体不依赖 UI 线程，从而不死锁；
    /// 2) 只在【第一个】await 写 false 不够：方法里有多个 await 就要每个都写；
    /// 3) ConfigureAwait(false) 不是“在线程池执行整个方法”：方法开头到第一个 await 前的代码
    ///    仍在调用线程同步执行；
    /// 4) 同步阻塞异步方法本身就是反模式，库加 false 是“防御性兜底”，应用层应一路 async/await。
    ///
    /// 【GetAwaiter().GetResult() 与 Task.Result 的区别（同步阻塞，两者都会阻塞、在 UI 上都可能死锁）】
    /// - .Result/.Wait()：异常被包成 AggregateException，catch 后还要 InnerException/Flatten；
    /// - GetAwaiter().GetResult()：同样阻塞，但【直接抛原始异常】（与 await 抛出的一致），
    ///   还能用于 ValueTask、Task.Yield 等没有 Result 属性的可等待对象；
    /// - 二者都不要在 UI 线程/ASP.NET 经典管线里用。控制台/线程池线程上配合
    ///   Task.Run(库方法.ConfigureAwait(false)) 使用相对安全（见示例5）。
    ///
    /// 【版本可用性】<see cref="Task.ConfigureAwait(bool)"/> 与 ConfiguredTaskAwaitable 为
    /// .NET Framework 4.5+（net48 全部可用）。
    /// </summary>
    ///
    /// <example>
    /// 真实网络代码的标准库内写法（本类不主动访问网络，仅注释保留；HttpClient 应静态复用）：
    /// <code>
    /// // private static readonly HttpClient Http = new HttpClient();
    /// // using (HttpResponseMessage response = await Http.GetAsync(url).ConfigureAwait(false))
    /// // {
    /// //     response.EnsureSuccessStatusCode();
    /// //     return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    /// // }
    /// </code>
    /// </example>
    public static class HConfigureAwaitHelp
    {
        /// <summary>
        /// 示例1：类库内部异步方法：每一跳都 ConfigureAwait(false)，续体不抓任何上下文。
        /// 本方法为【模拟实现，不访问网络】；真实 HTTP 写法见类级注释。
        /// </summary>
        /// <param name="url">目标地址（只回显，不实际请求）。</param>
        /// <returns>模拟响应文本。</returns>
        /// <exception cref="ArgumentException">url 为 null/空时抛出。</exception>
        public static async Task<string> ReadUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(HTranslation.GetContent("url 不能为空"), "url");
            }

            // 第一跳 false：完成后续体留在线程池，不回调用方上下文
            string simulated = await SimulatedHttpGetAsync(url).ConfigureAwait(false);
            // 第二跳同样 false：方法内每个 await 都要单独配置，ConfigureAwait 不向下传递
            return await Task.FromResult(simulated).ConfigureAwait(false);
        }

        /// <summary>
        /// 示例2：库代码忽略取消异常的常见写法（同样不回上下文）。
        /// </summary>
        /// <param name="milliseconds">延时毫秒（自测传小值）。</param>
        /// <param name="token">取消令牌。</param>
        /// <returns>正常或被取消都会顺利完成的 Task。</returns>
        public static async Task DelayQuietly(int milliseconds, CancellationToken token)
        {
            try
            {
                await Task.Delay(milliseconds, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 取消属正常退出，库方法选择吞掉而不是上抛
            }
        }

        /// <summary>
        /// 示例3：UI 层正确姿势——库调用 ConfigureAwait(false)，而【事件处理方法本身不写 ConfigureAwait】，
        /// 所以 await 整个库调用之后仍在 UI 线程，可直接更新控件。
        /// </summary>
        /// <param name="libraryCall">返回库数据的异步委托（其内部应已 ConfigureAwait(false)）。</param>
        /// <param name="setText">在 UI 线程上执行的控件更新委托。</param>
        /// <returns>表示完成的 Task。</returns>
        /// <exception cref="ArgumentNullException">参数为 null 时抛出。</exception>
        public static async Task UiEventHandlerAsync(Func<Task<string>> libraryCall, Action<string> setText)
        {
            if (libraryCall == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("库调用委托不能为空"));
            }

            if (setText == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("设置文本委托不能为空"));
            }

            // 外层不写 ConfigureAwait：捕获 UI 上下文，await 之后回到 UI 线程
            string data = await libraryCall().ConfigureAwait(false);  // 被调库内部不抓上下文
            setText(data);                                            // 此处仍在 UI 线程，更新控件安全
        }

        /// <summary>
        /// 示例4：ConfigureAwait(true) 显式表达需要原上下文（与不写等价，仅用于强调意图）。
        /// </summary>
        /// <returns>固定返回 1。</returns>
        public static async Task<int> NeedOriginalContextAsync()
        {
            // 与不写 ConfigureAwait 等价：续体回到 await 前捕获的同步上下文
            return await Task.FromResult(1).ConfigureAwait(true);
        }

        /// <summary>
        /// 示例5：观察续体线程流转。在【没有同步上下文】的线程上 await（ConfigureAwait(false)），
        /// 续体直接由线程池调度，线程 id 可能变化；UI 上下文下若用 true 则前后同为 UI 线程。
        /// </summary>
        /// <returns>形如 "before线程id -> after线程id" 的描述串（仅观测，不断言）。</returns>
        public static async Task<string> ObserveContinuationThreadAsync()
        {
            int before = Environment.CurrentManagedThreadId;
            // 注意：Task.Yield() 返回的 YieldAwaitable 在 net48 没有 ConfigureAwait 扩展（Core 才有）；
            // 在无 SynchronizationContext 的线程上 Yield 本身即把续体投递到线程池，效果等同 false
            await Task.Yield();
            int after = Environment.CurrentManagedThreadId;
            return before + " -> " + after;
        }

        /// <summary>
        /// 示例6：GetAwaiter().GetResult() 与 .Result 的差异演示——前者直接抛原始异常（无 AggregateException
        /// 包装）。注意：它仍是【同步阻塞】，在 UI 线程上同样可能死锁，仅限非 UI 的顶层/Main 中少量使用。
        /// </summary>
        /// <param name="failingTask">已失败或将要失败的任务；不能为 null。</param>
        /// <returns>被阻塞等待时抛出的异常类型名（应为原始异常类型，而非 AggregateException）。</returns>
        /// <exception cref="ArgumentNullException">failingTask 为 null 时抛出。</exception>
        public static string BlockAndGetOriginalException(Task failingTask)
        {
            if (failingTask == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("失败任务不能为空"));
            }

            try
            {
                // 对比：failingTask.Wait() 在这里会 catch 到 AggregateException
                failingTask.GetAwaiter().GetResult();
                return HTranslation.GetContent("(无异常)");
            }
            catch (Exception ex)
            {
                // 直接是 InvalidOperationException 等原始类型，无需 ex.InnerException
                return ex.GetType().Name;
            }
        }

        /// <summary>
        /// 示例7：非 UI 环境（控制台 Main、后台服务）偶尔必须同步等待异步方法时的相对安全模板：
        /// 在线程池线程上运行 + 库内 ConfigureAwait(false) + GetAwaiter().GetResult()。
        /// 【绝不要】在 UI 事件/WinForms/WPF 线程上使用。
        /// </summary>
        /// <param name="asyncWork">异步工作（内部建议 ConfigureAwait(false)）；不能为 null。</param>
        /// <returns>工作结果。</returns>
        /// <exception cref="ArgumentNullException">asyncWork 为 null 时抛出。</exception>
        public static int SafeSyncOverAsyncFromThreadPool(Func<Task<int>> asyncWork)
        {
            if (asyncWork == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("异步工作委托不能为空"));
            }

            // Task.Run 把执行起点搬上没有 SynchronizationContext 的线程池线程，
            // 内部 ConfigureAwait(false) 保证续体不回头；外层再用 GetAwaiter().GetResult() 阻塞
            return Task.Run(delegate
            {
                return asyncWork().ConfigureAwait(false).GetAwaiter().GetResult();
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 模拟 HTTP GET：不访问网络，立即返回已完成任务（供本类示例自测）。
        /// </summary>
        /// <param name="url">要回显的地址。</param>
        /// <returns>携带模拟响应的已完成任务。</returns>
        private static Task<string> SimulatedHttpGetAsync(string url)
        {
            // Task.FromResult 零延时零网络，对应真实代码中的 await Http.GetAsync(url)
            return Task.FromResult(HTranslation.GetContent("模拟响应(未访问网络): ") + url);
        }
    }
}
