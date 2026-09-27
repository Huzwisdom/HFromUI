using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Async
{
    using HFromUI.HLangage;
    /// <summary>
    /// 【是什么】async/await 异步编程帮助类，基于 <see cref="Task"/> / <see cref="Task{TResult}"/>。
    /// async 关键字告诉编译器把方法体编译成“异步状态机”（生成 IAsyncStateMachine 状态机类 +
    /// AsyncTaskMethodBuilder 调度器）：遇到第一个未完成的 await 即【立即把控制权交还调用方】，
    /// 方法被切成多段续体（continuation）；等待期间不占用任何线程（I/O 等待时工作线程归还线程池，
    /// 由内核/IOCP 在完成后再投递续体），await 返回时默认通过当前 <see cref="SynchronizationContext"/>
    /// （或 TaskScheduler）把续体送回原上下文。await 本身【不创建新线程】；只有 Task.Run 才借线程池线程。
    ///
    /// 异步方法三种返回值约定：
    /// 1) Task：不返回值、可 await、可观察异常/取消（99% 的异步方法用它）；
    /// 2) Task&lt;T&gt;：异步返回 T；
    /// 3) void：【仅用于事件处理器】——无法 await、异常直接抛到 SynchronizationContext（可能崩溃进程），
    ///    除事件处理器外一律不要写 async void。
    ///
    /// 【是否跨进程】否。Task/状态机/取消令牌全部在当前进程内流转。
    ///
    /// 【典型适用场景】
    /// 1) I/O 密集：文件、网络、数据库、Web 请求——等待期不占线程；
    /// 2) 延时与计时：<see cref="Task.Delay(int)"/>（对比 Thread.Sleep 会阻塞线程）；
    /// 3) UI 程序：await 后自动回 UI 线程更新控件，界面不卡死；
    /// 4) 并发 I/O：<see cref="Task.WhenAll(Task[])"/> 同时发起多个等待、<see cref="Task.WhenAny(Task[])"/>
    ///    做超时/先到先得；
    /// 5) 可取消长任务：传入 <see cref="CancellationToken"/>（详见 HCancellationTokenHelp）。
    ///
    /// 【使用步骤】
    /// 1) 方法签名加 async，返回 Task/Task&lt;T&gt;，方法名以 Async 结尾；
    /// 2) 内部对异步操作使用 await；类库代码每一跳加 ConfigureAwait(false)（见 HConfigureAwaitHelp）；
    /// 3) 需要取消则增加 CancellationToken 参数并在 await 点透传；
    /// 4) 调用方 await 该方法；UI 事件处理器可直接 async void（仅此一种例外）；
    /// 5) 多个独立 I/O 用 Task.WhenAll，超时用 Task.WhenAny + Task.Delay。
    ///
    /// 【注意事项与坑】
    /// 1) async 方法中抛异常：await 时【解包】抛出原始异常（catch 到的就是原始 Exception，
    ///    不是 AggregateException）；而 Task.Wait()/Task.Result 的同步阻塞会抛
    ///    <see cref="AggregateException"/>（用 Flatten()/Handle() 处理）；
    /// 2) 经典死锁：UI/ASP.NET 经典管线中，UI 线程调用 SomeAsync().Result（或 .Wait()）阻塞 UI 线程，
    ///    异步方法 await 完成后续体要回 UI 上下文 → UI 线程在等结果、续体等 UI 线程 → 死锁。
    ///    避免：一路 async/await 不要中途阻塞；库内一律 ConfigureAwait(false)；详见 HConfigureAwaitHelp；
    /// 3) async void 无法 await、异常无法在调用方捕获，除事件处理器外禁用；
    /// 4) Task.Delay 不保证精确，仅“不早于”指定时间；高精度计时用 Stopwatch；
    /// 5) 未被 await 且未观察异常的 Task：net45 起异常默认在 GC 时不再终止进程，但仍应处理；
    /// 6) CPU 密集才用 Task.Run；I/O 密集用原生 XxxAsync API，不要用 Task.Run 包一个阻塞调用假装异步。
    ///
    /// 【版本可用性】async/await、Task、Task.Delay、Task.Yield、Task.WhenAll/WhenAny 均为
    /// .NET Framework 4.5+（net48 全部可用）；<see cref="Task.FromException(System.Exception)"/>
    /// 与带 <see cref="CancellationToken"/> 的部分重载为 4.6+（net48 可用）。
    /// </summary>
    ///
    /// <example>
    /// UI 按钮点击（async void 唯一合法场景）：
    /// <code>
    /// private async void Button_Click(object sender, EventArgs e)
    /// {
    ///     try
    ///     {
    ///         string text = await HAsyncAwaitHelp.ReadFileAsync(@"D:\a.txt");
    ///         textBox1.Text = text;   // await 后已回到 UI 线程，可直接操作控件
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         textBox1.Text = ex.Message;
    ///     }
    /// }
    /// </code>
    /// 禁止的死锁写法（在 UI 线程上执行时）：
    /// <code>
    /// // string s = SomeAsync().Result;   // 经典 sync-over-async 死锁
    /// // SomeAsync().Wait();              // 同理
    /// </code>
    /// </example>
    public static class HAsyncAwaitHelp
    {
        /// <summary>
        /// 示例1：异步延时（不阻塞线程；对比 <see cref="Thread.Sleep(int)"/> 会占住线程空转）。
        /// </summary>
        /// <param name="milliseconds">延时毫秒数；自测请传小值（如 1~50）。</param>
        /// <returns>表示延时的 Task。</returns>
        /// <exception cref="ArgumentOutOfRangeException">milliseconds 为负数时由 Task.Delay 抛出。</exception>
        public static async Task DelayAsync(int milliseconds)
        {
            // 等待期间当前线程被释放去做别的工作；时间到后续体被重新调度
            await Task.Delay(milliseconds).ConfigureAwait(false);
        }

        /// <summary>
        /// 示例2：带返回值的异步方法 + 协作式取消。
        /// </summary>
        /// <param name="token">取消令牌；取消时 Task.Delay 抛 <see cref="TaskCanceledException"/>。</param>
        /// <returns>正常完成返回 42；被取消则任务以 Canceled 结束并抛 OperationCanceledException。</returns>
        /// <exception cref="OperationCanceledException">token 被取消时抛出（取消的标准表达）。</exception>
        public static async Task<int> DoWorkAsync(CancellationToken token = default(CancellationToken))
        {
            // 取消令牌一路透传给底层等待点；取消是协作式的，不会强杀线程
            await Task.Delay(100, token).ConfigureAwait(false);
            return 42;
        }

        /// <summary>
        /// 示例3：I/O 密集——异步读文件（磁盘等待期间不占线程）。
        /// </summary>
        /// <param name="path">要读取的文本文件完整路径。</param>
        /// <returns>文件全部文本内容。</returns>
        /// <exception cref="FileNotFoundException">路径不存在时由 StreamReader 抛出。</exception>
        /// <exception cref="IOException">读取过程发生 I/O 错误时抛出。</exception>
        public static async Task<string> ReadFileAsync(string path)
        {
            // using 块（C# 7.3 不使用 using 声明写法）；离开作用域自动 Dispose
            using (StreamReader reader = new StreamReader(path))
            {
                // ConfigureAwait(false)：库代码不需要回到原同步上下文，避免上层误用 .Result 时死锁
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 示例4：I/O 密集——HTTP GET【模拟版，不发起真实网络请求】。
        /// 真实项目的等价写法（需自行保证 HttpClient 静态复用，勿每次 new）：
        /// <code>
        /// // private static readonly HttpClient Http = new HttpClient();   // 应静态复用
        /// // string html = await Http.GetStringAsync(url).ConfigureAwait(false);
        /// </code>
        /// </summary>
        /// <param name="url">请求地址（本示例只回显，不实际访问）。</param>
        /// <returns>模拟的响应文本。</returns>
        /// <exception cref="ArgumentException">url 为 null 或空串时抛出。</exception>
        public static Task<string> HttpGetAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(HTranslation.GetContent("url 不能为空"), "url");
            }

            // 用 FromResult 模拟“已就绪的异步结果”：零等待、零网络，自测安全
            return Task.FromResult(HTranslation.GetContent("模拟响应(未访问网络): ") + url);
        }

        /// <summary>
        /// 示例5：并行发起多个异步 I/O 再一起等待【模拟版，不访问网络】。
        /// 真实场景把 HttpGetAsync 换成 HttpClient.GetStringAsync 即可：所有请求同时在途，
        /// 总耗时约等于最慢的一个（而非各请求耗时之和），等待期间不额外占用线程。
        /// </summary>
        /// <param name="urls">地址列表（可为空）。</param>
        /// <returns>所有模拟请求完成的 Task。</returns>
        /// <exception cref="ArgumentNullException">urls 为 null 时抛出。</exception>
        public static async Task WhenAllUrlsAsync(params string[] urls)
        {
            if (urls == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("网址集合不能为空"));
            }

            // Array.ConvertAll 立即“热启动”所有任务（注意：此时它们已并发在途）
            Task<string>[] tasks = Array.ConvertAll(urls, delegate (string u) { return HttpGetAsync(u); });
            // 任一任务失败时 await 抛出第一个异常；其余任务的异常需观察该 Task.Exception 才不丢失
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// 示例6：异步超时——先完成者胜：业务任务先完成返回 true；延时先到返回 false。
        /// 注意：超时返回 false 【不会取消】内部任务，它可能仍在后台运行；需要真正取消请用
        /// CancellationTokenSource.CreateLinkedTokenSource + CancelAfter（见示例8）。
        /// </summary>
        /// <param name="task">要等待的业务任务；不能为 null。</param>
        /// <param name="timeoutMs">超时时长（毫秒），自测传小值。</param>
        /// <returns>业务任务先完成返回 true；超时返回 false。</returns>
        /// <exception cref="ArgumentNullException">task 为 null 时抛出。</exception>
        public static async Task<bool> WaitWithTimeout(Task task, int timeoutMs)
        {
            if (task == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("任务不能为空"));
            }

            // WhenAny：任一任务完成即返回，不聚合结果、不抛业务异常
            Task winner = await Task.WhenAny(task, Task.Delay(timeoutMs)).ConfigureAwait(false);
            return object.ReferenceEquals(winner, task);
        }

        /// <summary>
        /// 示例7：<see cref="Task.Yield"/>——主动让出一次控制权，让当前线程立刻可以去处理别的工作
        /// （UI 线程上可让消息循环先响应重绘/输入），续体随后按当前上下文重新排队执行。
        /// 与 Task.Delay(0) 不同：Delay(0) 同步完成不发生让出；Yield 必定产生一次真正的异步续体调度。
        /// </summary>
        /// <param name="token">取消令牌。</param>
        /// <returns>让出完成后的 Task。</returns>
        /// <exception cref="OperationCanceledException">token 已取消时抛出。</exception>
        public static async Task YieldDemoAsync(CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            // 此处之前的代码与调用方同步执行；Yield 之后的代码一定作为续体异步执行
            await Task.Yield();
            token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// 示例8：真正可取消的超时——用 CreateLinkedTokenSource 把“外部令牌 + 内部超时”组合，
        /// 到点自动取消工作任务，避免任务在后台泄漏运行。
        /// </summary>
        /// <param name="work">接收取消令牌的实际工作；不能为 null。</param>
        /// <param name="timeoutMs">超时时长（毫秒），自测传小值（如 30）。</param>
        /// <returns>工作任务本身。</returns>
        /// <exception cref="ArgumentNullException">work 为 null 时抛出。</exception>
        /// <exception cref="OperationCanceledException">超时或外部取消时由工作任务内部抛出。</exception>
        public static async Task RunWithTimeoutAsync(Func<CancellationToken, Task> work, int timeoutMs)
        {
            if (work == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("工作委托不能为空"));
            }

            // linkedCTS 关联一个内部令牌；任一源取消它就取消——它是独立资源，必须 Dispose
            using (CancellationTokenSource linkedCts = new CancellationTokenSource())
            {
                linkedCts.CancelAfter(timeoutMs);   // 到点自动 Cancel，不依赖定时器回调以外的线程
                await work(linkedCts.Token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 示例9：await 观察异常——await 会【解包】异常，catch 到的是原始异常而非 AggregateException。
        /// </summary>
        /// <returns>被捕获异常的类型名（演示用，自测秒回）。</returns>
        public static async Task<string> ObserveAwaitExceptionAsync()
        {
            // FromException：构造一个“已失败”的任务，不做任何真实等待（net46+，net48 可用）
            Task fail = Task.FromException(new InvalidOperationException(HTranslation.GetContent("演示失败")));
            try
            {
                await fail.ConfigureAwait(false);
                return HTranslation.GetContent("未捕获到异常");
            }
            catch (InvalidOperationException ex)
            {
                // 这里 catch 到的就是原始 InvalidOperationException，无需处理 AggregateException
                return ex.GetType().Name + ": " + ex.Message;
            }
        }

        /// <summary>
        /// 示例10：同步等待与 WhenAll 场景下的 <see cref="AggregateException"/>——
        /// 多个子任务同时失败时，用 AggregateException.Handle 逐个处理（每个都处理才算处理完）。
        /// 本示例只构造已失败任务，不阻塞、不访问网络。
        /// </summary>
        /// <returns>被 Handle 处理掉的内部异常条数。</returns>
        public static async Task<int> ObserveAggregateExceptionAsync()
        {
            Task a = Task.FromException(new InvalidOperationException(HTranslation.GetContent("错误1")));
            Task b = Task.FromException(new ArgumentException(HTranslation.GetContent("错误2")));
            Task all = Task.WhenAll(a, b);

            int handled = 0;
            try
            {
                // 不使用 await（await 只会抛第一个内部异常）；直接读聚合任务的 Exception
                await all.ConfigureAwait(false);
            }
            catch
            {
                // 真正同步阻塞（.Wait()/.Result）或读 Task.Exception 时拿到的是 AggregateException
                AggregateException aggregate = all.Exception.Flatten();
                aggregate.Handle(delegate (Exception ex)
                {
                    // 返回 true 表示“已处理”；返回 false 的异常会重新抛出
                    handled++;
                    return true;
                });
            }

            return handled;
        }

        /// <summary>
        /// 示例11：async void【唯一合法用途——事件处理器】。无法被 await，异常直接投递到同步上下文，
        /// 因此方法内部必须自己 try/catch 兜底，否则未处理异常可能终止进程。
        /// </summary>
        /// <param name="sender">事件发送者（通常为控件本身）。</param>
        /// <param name="e">事件参数。</param>
        public static async void EventHandlerOnlyDemo(object sender, EventArgs e)
        {
            // async void 方法内部务必自行兜底：调用方无法 catch 这里面的异常
            try
            {
                await Task.Delay(1).ConfigureAwait(true);   // 事件处理器需要回 UI 线程，不写 false
                string text = sender == null ? "(null)" : sender.ToString();
                // 实际项目此处可安全操作控件；e 可能为 EventArgs.Empty
                GC.KeepAlive(text);
                GC.KeepAlive(e);
            }
            catch (Exception)
            {
                // 吞掉或记录；绝不能让异常逃逸 async void
            }
        }

        /// <summary>
        /// 示例12：CPU 密集工作应放到线程池（<see cref="Task.Run(Func{Task})"/>），
        /// 调用方 await 时 UI 线程保持畅通。注意：纯 I/O 不要用 Task.Run 包阻塞调用，那只是浪费一个线程。
        /// </summary>
        /// <param name="iterations">计算循环次数（自测传小值）。</param>
        /// <returns>累加结果。</returns>
        public static async Task<long> CpuBoundAsync(int iterations)
        {
            return await Task.Run(delegate
            {
                long sum = 0;
                for (int i = 0; i < iterations; i++)
                {
                    sum += i;   // 模拟 CPU 计算
                }
                return sum;
            }).ConfigureAwait(false);
        }
    }
}
