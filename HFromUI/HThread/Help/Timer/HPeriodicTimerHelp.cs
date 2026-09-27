using System;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HThread.Help.Timer
{
    using HFromUI.HLangage;
    /// <summary>
    /// PeriodicTimer 帮助类：async/await 原生的异步周期定时器（高版本 API，net48 仅文档说明 + Task.Delay 等价实现）。
    /// 【是什么】System.Threading.PeriodicTimer 是 .NET 6 引入的 sealed 类（System.Runtime.dll，System.Threading 命名空间），
    /// 实现 IDisposable；构造时给定周期，消费端在循环里 await WaitForNextTickAsync 等待下一拍。
    /// 它不是回调模型而是"异步等待"模型：没有回调、没有 Elapsed 事件、不排队到线程池，
    /// 等待期间不占用线程，到点后接续执行 await 之后的代码。
    /// 【是否跨进程】否。进程内的托管计时器，内部基于系统定时器。
    /// 【典型适用场景】.NET 6+ 的 async 周期循环：后台轮询、定期刷新、心跳上报；需要 CancellationToken 优雅取消的循环。
    /// 【使用步骤（.NET 6+）】
    /// 1) using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
    /// 2) while (await timer.WaitForNextTickAsync(ct)) { await DoWorkAsync(); }
    /// 3) 返回 false 表示定时器已被 Dispose，循环自然退出；释放用 Dispose()。
    /// 【注意事项与坑（对照微软文档）】
    /// - 天然不重入：必须等上一轮循环体（含 DoWorkAsync）执行完，才会再次进入 WaitForNextTickAsync，
    ///   因此同一循环体永远不会并发——这是它相对 Threading.Timer/Timers.Timer 的核心优势；
    /// - 单消费者：任一时刻只允许一个 WaitForNextTickAsync 在飞，两个循环同时 await 同一实例会抛异常/行为未定义；
    /// - 节拍合并：行为类似自动重置事件，两次 await 之间若错过多个周期，只算作一拍（不会补拍）；
    /// - 取消行为：CancellationToken 取消只影响【当前这一次等待】，定时器本身继续运行；
    ///   await 一个因取消而结束的 WaitForNextTickAsync 会抛 OperationCanceledException（取消时 ValueTask 进入取消态），
    ///   调用前 token 已取消则 ValueTask 直接以"已取消"状态创建；Dispose 与在飞等待可并发，Dispose 会让等待返回 false
    ///   并丢弃尚未消费的节拍；
    /// - 周期从"每次等待开始"计时，循环体耗时会推迟下一拍（固定延迟语义；若要固定时刻对齐需自行补偿）。
    /// 【版本可用性】
    /// - .NET 6+：PeriodicTimer(TimeSpan period) 构造、Period 属性（.NET 6 只读）、
    ///   ValueTask&lt;bool&gt; WaitForNextTickAsync(CancellationToken cancellationToken = default)、Dispose()；
    /// - .NET 7+：Period 增加 set 访问器，可在运行中调整周期；
    /// - .NET 8+：新增 PeriodicTimer(TimeSpan period, TimeProvider timeProvider) 构造，可用 TimeProvider 做虚拟时间测试；
    /// - net48：【不可编译】本类型不存在；本类保持为可编译的静态帮助类，下面给出 Task.Delay 循环等价实现，
    ///   语义对应"固定延迟 + 取消 + 不重入"。
    /// </summary>
    ///
    /// <example>
    /// 【.NET 6+ 原生写法（本工程 net48 不可编译，仅作文档示例）】
    /// <code>
    /// using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1)))
    /// {
    ///     while (await timer.WaitForNextTickAsync(cts.Token))
    ///     {
    ///         await DoWorkAsync();      // 天然不重入：等这里执行完才会等下一拍
    ///     }
    /// }
    /// // 退出循环的两种路径：
    /// // 1) timer.Dispose() → WaitForNextTickAsync 返回 false
    /// // 2) cts.Cancel()   → await 抛 OperationCanceledException（仅当前等待被取消，定时器仍存活）
    /// </code>
    /// </example>
    public static class HPeriodicTimerHelp
    {
        // ===== 以下为高版本 PeriodicTimer API 的注释-only 清单（net48 无此类型，严禁取消注释） =====
        //
        // 【.NET 6+】sealed class PeriodicTimer : IDisposable
        //   构造： PeriodicTimer(TimeSpan period)
        //   属性： TimeSpan Period { get; }                 // .NET 6 只读；.NET 7 起可 set
        //   方法： ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
        //          返回 true=到点；false=定时器已 Dispose；取消→await 抛 OperationCanceledException
        //   方法： void Dispose()                           // 停止计时并释放资源，可与在飞等待并发
        // 【.NET 8+】构造： PeriodicTimer(TimeSpan period, TimeProvider timeProvider)
        //
        // ============================================================================================

        /// <summary>
        /// net48 等价实现：Task.Delay 循环模拟 PeriodicTimer（int 毫秒版）。
        /// 语义：先等 intervalMs，再执行 tickAsync，循环往复；天然不重入（await 串行）；
        /// token 取消时 Task.Delay 抛 OperationCanceledException，向调用方传播（与 PeriodicTimer 取消行为一致）。
        /// </summary>
        /// <param name="intervalMs">每拍间隔（毫秒）；建议 &gt;0，0 会造成紧密循环</param>
        /// <param name="tickAsync">每拍工作；上一轮完成后才开始下一轮等待，不会并发</param>
        /// <param name="token">取消令牌；取消后当前 Task.Delay 抛 OperationCanceledException</param>
        /// <returns>表示循环生命周期的 Task（正常仅因取消而结束）</returns>
        /// <exception cref="ArgumentNullException">tickAsync 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">intervalMs 为负数（Task.Delay 限制，-1 之外均非法）</exception>
        /// <exception cref="OperationCanceledException">token 被取消</exception>
        public static async Task RunPeriodicAsync(
            int intervalMs, Func<Task> tickAsync, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // 先等一拍：取消时这里抛 OCE，直接结束循环
                await Task.Delay(intervalMs, token).ConfigureAwait(false);
                await tickAsync().ConfigureAwait(false);   // 上一轮完成后才开始下一轮等待，天然不重入
            }
        }

        /// <summary>
        /// net48 等价实现：Task.Delay 循环（TimeSpan 版），对应 new PeriodicTimer(period) 的传参习惯。
        /// </summary>
        /// <param name="period">每拍间隔；须为非负 TimeSpan</param>
        /// <param name="tickAsync">每拍工作（串行不重入）</param>
        /// <param name="token">取消令牌</param>
        /// <returns>表示循环生命周期的 Task</returns>
        /// <exception cref="ArgumentNullException">tickAsync 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">period 为负（Task.Delay 限制）</exception>
        /// <exception cref="OperationCanceledException">token 被取消</exception>
        public static async Task RunPeriodicAsync(
            TimeSpan period, Func<Task> tickAsync, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // TimeSpan 版延迟，便于与高版本 PeriodicTimer(TimeSpan) 代码对照迁移
                await Task.Delay(period, token).ConfigureAwait(false);
                await tickAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// net48 等价实现：固定时刻对齐版（简单漂移补偿）。
        /// PeriodicTimer 的节拍按等待起点计时，本方法在每拍结束后计算"距下一对齐点还剩多久"，
        /// 用剩余时间做 Delay，尽量消除工作耗时造成的漂移；工作超时（耗完一整拍）则立即进入下一拍。
        /// </summary>
        /// <param name="intervalMs">对齐周期（毫秒）；必须 &gt;0</param>
        /// <param name="tickAsync">每拍工作</param>
        /// <param name="token">取消令牌</param>
        /// <returns>表示循环生命周期的 Task</returns>
        /// <exception cref="ArgumentNullException">tickAsync 为 null</exception>
        /// <exception cref="ArgumentOutOfRangeException">intervalMs 小于等于 0</exception>
        /// <exception cref="OperationCanceledException">token 被取消</exception>
        public static async Task RunAlignedPeriodicAsync(
            int intervalMs, Func<Task> tickAsync, CancellationToken token)
        {
            if (intervalMs <= 0)
            {
                // 对齐模式必须有正周期，否则"下一对齐点"无意义
                throw new ArgumentOutOfRangeException(HTranslation.GetContent("间隔时间必须大于 0 毫秒"));
            }
            // 记录第一个对齐基准（UTC 滴答数，避免夏令时/系统时间回拨影响用 Stopwatch 更严谨，此处演示取 UtcNow）
            DateTime next = DateTime.UtcNow;
            while (!token.IsCancellationRequested)
            {
                next = next.AddMilliseconds(intervalMs);   // 推进到下一对齐点
                await tickAsync().ConfigureAwait(false);  // 先干活
                TimeSpan wait = next - DateTime.UtcNow;    // 距对齐点剩余时间
                int waitMs = wait.TotalMilliseconds > 0d ? (int)Math.Ceiling(wait.TotalMilliseconds) : 0;
                await Task.Delay(waitMs, token).ConfigureAwait(false);  // 超时则 waitMs=0，立即下一拍
            }
        }
    }
}
