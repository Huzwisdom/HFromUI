namespace HFromUI.HThread.Help.Signaling
{
    /// <summary>
    /// 【是什么】Channel（<c>System.Threading.Channels.Channel&lt;T&gt;</c>）帮助类：天生异步的
    /// 生产者-消费者有界/无界队列。Channel 把读写两侧显式拆成 <c>ChannelWriter&lt;T&gt;</c> 与
    /// <c>ChannelReader&lt;T&gt;</c>：生产者用 WaitToWriteAsync/WriteAsync（满时异步等待，不阻塞线程），
    /// 消费者用 WaitToReadAsync/ReadAllAsync（空时异步等待），写完调 Writer.Complete，
    /// 消费者 await foreach 在 Complete 且取空后自然结束，并可 await Reader.Completion 等待全部处理完毕。
    ///
    /// 【是否跨进程】否。进程内异步数据结构，不涉及内核命名对象。
    ///
    /// 【典型适用场景】
    /// 1) async/await 流水线（网络收包→解码→落库），需要背压但不能用阻塞调用占住线程池线程；
    /// 2) 单一/固定数量消费者的工作队列（SingleReader=true 可启用单读无锁优化）；
    /// 3) 需要“满了怎么办”策略的有界队列（等待 / 丢新 / 丢旧 / 直接丢弃）。
    /// 与 <see cref="HBlockingCollectionHelp"/> 的取舍：同步线程、老代码用 BlockingCollection；
    /// 全异步管线用 Channel。
    ///
    /// 【使用步骤】
    /// 1) 创建：Channel.CreateBounded&lt;T&gt;(容量或 BoundedChannelOptions) / CreateUnbounded&lt;T&gt;()；
    /// 2) 生产者：await Writer.WaitToWriteAsync(token) 后 TryWrite/WriteAsync，结束 Writer.Complete()；
    /// 3) 消费者：await foreach (var x in Reader.ReadAllAsync(token))，或 WaitToReadAsync + TryRead；
    /// 4) 优雅关停：await Reader.Completion（Complete 后取空即成功完成；Complete(ex) 则带异常完成）。
    ///
    /// 【注意事项与坑】
    /// 1) FullMode 仅对有界通道生效；CreateBounded(1) 容量最小合法值为 1；
    /// 2) DropOldest：满了丢“最老”的队首元素给新元素腾位；DropNewest：满了直接丢“本次新写入”的值；
    ///    DropWrite：TryWrite 直接返回 false（丢弃本次写入，不报错）；Wait：WriteAsync 异步等待空位（默认）；
    /// 3) SingleReader/SingleWriter 只是“优化承诺”：违反约定（多线程同时写）不会抛异常，但数据竞争由你自己负责；
    /// 4) Complete 之后再 WriteAsync 会抛 ChannelClosedException，应先 WaitToWriteAsync 或用 TryWrite/TryComplete；
    /// 5) AllowSynchronousContinuations=true 可能让“唤醒对方”的代码在当前线程上内联执行（重入/栈增长风险），默认 false 更稳；
    /// 6) Channel 不实现 IDisposable，靠 Complete/Completion 表达生命周期。
    ///
    /// 【版本可用性】.NET Framework 4.8 的 BCL 【不内置】 Channel，需 NuGet 安装
    /// “System.Threading.Channels”包（其 netstandard2.0 资产可在 net48 上使用）；
    /// .NET Core 3.0 / .NET 5+ 起为框架内置（System.Threading.Channels 命名空间）。
    /// 因此本帮助类保持为空的可编译静态类，所有真实用法仅以下方 XML 代码示例给出。
    /// </summary>
    ///
    /// <example>
    /// 【前置条件】net48 需先在 NuGet 安装 System.Threading.Channels；.NET Core 3.0+/.NET 5+ 直接可用。
    /// <code>
    /// using System.Threading.Channels;
    /// using System.Threading.Tasks;
    ///
    /// // ============ 1) 创建有界通道：CreateBounded + BoundedChannelOptions ============
    /// // FullMode 四种策略：
    /// //   Wait       满了则 WriteAsync 异步等待空位（默认，严格背压）
    /// //   DropWrite  满了则 TryWrite 直接返回 false（静默丢弃本次新值）
    /// //   DropOldest 满了丢掉队首最老的元素，给新值腾位
    /// //   DropNewest 满了直接丢掉本次写入的最新值
    /// Channel&lt;string&gt; channel = Channel.CreateBounded&lt;string&gt;(
    ///     new BoundedChannelOptions(100)                    // 容量 Capacity = 100（最小为 1）
    ///     {
    ///         FullMode = BoundedChannelFullMode.Wait,       // 背压策略（其余三种见上）
    ///         SingleReader = true,                          // 承诺只有一个消费者读（可做无锁优化）
    ///         SingleWriter = false,                         // 是否只有一个生产者写
    ///         AllowSynchronousContinuations = false,        // 唤醒对端是否允许内联同步执行
    ///     });
    ///
    /// // 只用容量、其余默认的快捷重载：Channel.CreateBounded&lt;string&gt;(100);
    ///
    /// // 无界通道（永不满，WaitToWriteAsync 恒就绪）：
    /// Channel&lt;string&gt; unbounded = Channel.CreateUnbounded&lt;string&gt;(
    ///     new UnboundedChannelOptions
    ///     {
    ///         SingleReader = true,
    ///         SingleWriter = true,
    ///         AllowSynchronousContinuations = false,
    ///     });
    /// // 快捷重载：Channel.CreateUnbounded&lt;string&gt;();
    ///
    /// // ============ 2) 生产者：Writer（WaitToWriteAsync / WriteAsync / TryWrite / Complete） ============
    /// async Task ProduceAsync(System.Threading.CancellationToken cancellationToken)
    /// {
    ///     for (int i = 0; i &lt; 1000; i++)
    ///     {
    ///         // 方式A：先问能不能写（满时异步等待；通道完成后返回 false）
    ///         while (await channel.Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false))
    ///         {
    ///             if (channel.Writer.TryWrite("msg" + i))   // 非阻塞尝试写入；DropWrite 模式满了返回 false
    ///             {
    ///                 break;
    ///             }
    ///         }
    ///
    ///         // 方式B：直接异步写，FullMode.Wait 时自动等空位；通道完成后抛 ChannelClosedException
    ///         await channel.Writer.WriteAsync("msg" + i, cancellationToken).ConfigureAwait(false);
    ///     }
    ///
    ///     channel.Writer.Complete();                        // 声明写完（等价 TryComplete(null)）
    ///     // channel.Writer.Complete(new InvalidOperationException("故障")); // 带异常完成
    /// }
    ///
    /// // ============ 3) 消费者：Reader（WaitToReadAsync / ReadAllAsync / TryRead） ============
    /// async Task ConsumeAsync(System.Threading.CancellationToken cancellationToken)
    /// {
    ///     // 推荐：await foreach 在 Complete 且数据取空后自动结束循环
    ///     await foreach (string msg in
    ///         channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
    ///     {
    ///         Console.WriteLine(msg);
    ///     }
    /// }
    ///
    /// // 手动轮询风格（等价语义）：
    /// async Task ConsumeManuallyAsync(System.Threading.CancellationToken cancellationToken)
    /// {
    ///     while (await channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
    ///     {
    ///         while (channel.Reader.TryRead(out string msg))  // 一次唤醒尽量排空，减少异步往返
    ///         {
    ///             Console.WriteLine(msg);
    ///         }
    ///     }
    ///     // 也可逐个 await channel.Reader.ReadAsync(cancellationToken);
    /// }
    ///
    /// // ============ 4) 优雅关停：await Reader.Completion ============
    /// async Task RunAsync()
    /// {
    ///     Task producer = ProduceAsync(System.Threading.CancellationToken.None);
    ///     Task consumer = ConsumeAsync(System.Threading.CancellationToken.None);
    ///     await producer.ConfigureAwait(false);
    ///     await channel.Reader.Completion.ConfigureAwait(false); // Complete 且取空后成功完成；带异常完成时此处抛出
    /// }
    ///
    /// // ============ 5) 四种满策略对照（同一通道选项，行为差异） ============
    /// // Wait       : 生产慢于消费无碍；生产更快时 WriteAsync 排队等待，内存稳定、不丢数据
    /// // DropWrite  : TryWrite 满了立即 false，调用方自行决定丢弃/记录（指标采样场景常用）
    /// // DropOldest : 保留最新数据（如行情/最新状态推送，老数据已无价值）
    /// // DropNewest : 注意它丢的是“刚到的新值”，适合“只保留历史窗口、新值可弃”的场景
    /// </code>
    /// 选型速查：同步阻塞线程的管线 → BlockingCollection（见 HBlockingCollectionHelp）；
    /// async/await 非阻塞管线 → Channel。
    /// </example>
    public static class HChannelHelp
    {
        // net48 不内置 System.Threading.Channels：
        //   1) 本类刻意保持空实现，不引用任何 Channel 类型，保证在 net48 + C# 7.3 下直接编译；
        //   2) 需要使用时，请通过 NuGet 安装 “System.Threading.Channels” 包（netstandard2.0 资产支持 net48），
        //      或迁移到 .NET Core 3.0 / .NET 5+（框架内置），上方 <example> 代码即可直接使用；
        //   3) 不想引入 NuGet 时，net48 上的同步等价方案是 System.Collections.Concurrent.BlockingCollection，
        //      参见同目录 HBlockingCollectionHelp 的完整示例。
    }
}
