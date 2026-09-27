namespace HFromUI.HThread.Help.DataflowPipeline
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// TPL Dataflow（System.Threading.Tasks.Dataflow 命名空间）是基于“块（Block）”的进程内消息传递 /
    /// Actor 风格并行流水线库：每个块自带输入队列、调度、并发度与有界容量，块与块用 LinkTo 连成有向图，
    /// 消息自动流转、完成与异常自动传播。常用块：BufferBlock、ActionBlock、TransformBlock、TransformManyBlock、
    /// BroadcastBlock、BatchBlock、WriteOnceBlock（另有 JoinBlock/BatchedJoinBlock 做多输入汇合）。
    /// 文档入口：https://learn.microsoft.com/zh-cn/dotnet/api/system.threading.tasks.dataflow?view=netstandard-2.0</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// 块与队列都在当前进程内存中，不跨进程；跨进程消息请用消息队列/gRPC 等。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) 多阶段生产者-消费者流水线（解析→变换→批量写库），每阶段独立限并发、独立背压；
    /// 2) 需要并行处理但阶段边界清晰的 CPU/异步混合任务（如限并发 URL 抓取，逻辑同形）；
    /// 3) 一对多广播（同一事件通知多个处理器）、奇偶路由、攒批提交；
    /// 4) 比裸 Queue + Monitor + 手写 Task 编排省大量胶水代码，且天然支持 async 委托与取消。</para>
    ///
    /// <para><b>【使用步骤】</b>
    /// 1) 准备 <b>ExecutionDataflowBlockOptions</b>：MaxDegreeOfParallelism（并发度，-1 无上限）、
    ///    BoundedCapacity（输入队列上限，-1 无界；设正数即开启背压）、CancellationToken、SingleProducerConstrained
    ///    （仅一个生产者时可开的性能开关）、MaxMessagesPerTask/TaskScheduler 按需；
    /// 2) 构造各块（TransformBlock/ActionBlock 等），委托支持 async；
    /// 3) LinkTo 连线并传 <b>DataflowLinkOptions { PropagateCompletion = true }</b>，
    ///    需要分流时给每个目标传谓词，最后一条兜底（谓词全 false 会丢消息）；
    /// 4) 生产者用 SendAsync（有界且满时会异步等待）/ Post（立即返回 bool，满则 false）投递；
    /// 5) 投完调源块 Complete()，最后 await 终端块 Completion 即知整条流水线结束/异常。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) <b>Post 不等待</b>：有界块满了 Post 直接返回 false（消息没进去，要自行处理）；要背压等待必须 await SendAsync。
    /// 2) PropagateCompletion 传播的是“完成<b>和异常</b>”：任一块委托抛异常，下游 Completion 会进入 Faulted，
    ///    await Completion 把原异常抛出来——必须 await 并处理，否则异常被静默吞在块内。
    /// 3) <b>谓词过滤要兜底</b>：LinkTo(谓词) 按 Append 顺序匹配，所有谓词都 false 的消息无处可去，会滞留在源块，
    ///    通常在末尾 LinkTo(死信块) 不接谓词；谓词只看消息本身，不要在谓词里做副作用。
    /// 4) BroadcastBlock 只保留“最新一条”：慢消费者会错过中间消息（每个已链接目标至少收到当前最新值），
    ///    它适合“状态广播”不适合“不丢消息的队列”；块构造参数是克隆委托，引用类型建议传深拷贝函数防止共享篡改。
    /// 5) WriteOnceBlock 一生只接收第一条消息，之后所有接收方都拿到同一值（经克隆委托）；再 Post 永远 false。
    /// 6) BatchBlock 攒够 batchSize 输出一个 T[]；不足一批时 Complete 会触发剩余输出；
    ///    非贪婪模式（GroupingDataflowBlockOptions { Greedy = false }）可从多源公平凑批。
    /// 7) TransformBlock 默认<b>按输入顺序输出</b>（即便并发处理，输出仍排队保序；新版本 EnsureOrdered 可关，旧包无此项）；
    ///    MaxDegreeOfParallelism 调的是“处理并发”，不是“输出乱序”。
    /// 8) BoundedCapacity 只限制该块输入队列，不统计正在处理中的消息语义外的下游堆积——容量按每块独立规划。
    /// 9) 不 LinkTo 也能直接从源块 ReceiveAsync/TryReceive 拉消息；用完即弃的短流水线可以只有源没有图。
    /// 10) 属性名是 <b>SingleProducerConstrained</b>（不是 Constrain），仅当确证只有一个线程投递时才设 true，否则数据结构会损坏。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Framework 4.8 <b>不内置</b>，需 NuGet 安装 System.Threading.Tasks.Dataflow（官方包支持 net48）；
    /// .NET Core/.NET 5+ 同样通过该 NuGet 包提供。本类保持空静态类可编译，真实用法全部在下方 example，
    /// 装包后代码可直接复制运行（示例纯内存数据，不碰文件/网络/UI）。</para>
    /// </summary>
    ///
    /// <example>
    /// 【安装 System.Threading.Tasks.Dataflow NuGet 包后可用】
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Threading;
    /// using System.Threading.Tasks;
    /// using System.Threading.Tasks.Dataflow;
    ///
    /// class DataflowDemo
    /// {
    ///     // ---------- 1) 七个常用块速览（行为注释） ----------
    ///     // BufferBlock&lt;T&gt;        ：纯 FIFO 缓冲块，不做变换；可手动 SendAsync/ReceiveAsync 当异步队列用
    ///     // ActionBlock&lt;TIn&gt;      ：终端块，对每条消息执行一个 Action/async 委托，无输出
    ///     // TransformBlock&lt;TIn,TOut&gt;       ：一进一出，Func 委托把 TIn 变 TOut
    ///     // TransformManyBlock&lt;TIn,TOut&gt;   ：一进多出，委托返回 IEnumerable&lt;TOut&gt;，逐个输出（如一行拆多词）
    ///     // BroadcastBlock&lt;T&gt;     ：始终只存最新一条，链接的所有目标各收一份（状态广播，不保证历史不丢）
    ///     // BatchBlock&lt;T&gt;         ：攒够 N 条输出一个 T[]（批量提交）
    ///     // WriteOnceBlock&lt;T&gt;     ：只接受第一条消息并永久保存，之后所有接收者拿到同一值
    ///
    ///     // ---------- 2) 完整流水线：TransformMany(拆词) -> 谓词分流 -> 两个 ActionBlock ----------
    ///     public async Task RunPipelineAsync()
    ///     {
    ///         var cts = new CancellationTokenSource();
    ///
    ///         // 一进多出：一行文本拆成多个单词（纯内存变换）
    ///         var split = new TransformManyBlock&lt;string, string&gt;(
    ///             line => (line ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
    ///             new ExecutionDataflowBlockOptions
    ///             {
    ///                 MaxDegreeOfParallelism = 2,   // 最多 2 条消息并行处理
    ///                 BoundedCapacity = 16,         // 输入队列上限：满则上游 SendAsync 自然等待（背压）
    ///                 CancellationToken = cts.Token,
    ///                 SingleProducerConstrained = true, // 确证只有下面这一个生产者投递，可开的提速开关
    ///             });
    ///
    ///         var even = new ActionBlock&lt;string&gt;(
    ///             word => Console.WriteLine("偶数长度: " + word),
    ///             new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }); // 1 = 严格顺序消费
    ///
    ///         var odd = new ActionBlock&lt;string&gt;(
    ///             word => Console.WriteLine("奇数长度: " + word),
    ///             new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
    ///
    ///         var deadLetter = new ActionBlock&lt;string&gt;(
    ///             word => Console.WriteLine("兜底分支: " + word));
    ///
    ///         // 谓词分流：LinkTo 按链接顺序匹配；PropagateCompletion 让完成与异常向下游传播
    ///         split.LinkTo(even, new DataflowLinkOptions { PropagateCompletion = true },
    ///             word => word.Length % 2 == 0);
    ///         split.LinkTo(odd, new DataflowLinkOptions { PropagateCompletion = true },
    ///             word => word.Length % 2 == 1);
    ///         split.LinkTo(deadLetter, new DataflowLinkOptions { PropagateCompletion = true }); // 兜底，防滞留
    ///
    ///         string[] lines = { "hello dataflow world", "one two three four", "batch broadcast writeonce" };
    ///         foreach (string line in lines)
    ///         {
    ///             await split.SendAsync(line);      // 有界且满时这里自动等待，形成全链路背压
    ///         }
    ///         split.Complete();                     // 声明不再投递
    ///         await Task.WhenAll(even.Completion, odd.Completion, deadLetter.Completion);
    ///         // 任一块内委托抛异常时，上面 await 会把异常重新抛出，必须处理
    ///     }
    ///
    ///     // ---------- 3) BufferBlock：当异步有界队列直接收发 ----------
    ///     public async Task BufferBlockDemoAsync()
    ///     {
    ///         var buffer = new BufferBlock&lt;int&gt;(
    ///             new ExecutionDataflowBlockOptions { BoundedCapacity = 4 });
    ///         await buffer.SendAsync(1);            // 不链接、直接投
    ///         int v = await buffer.ReceiveAsync();  // 无消息时异步等待，区别于 TryReceive 的立即返回 false
    ///         buffer.Complete();
    ///         await buffer.Completion;
    ///     }
    ///
    ///     // ---------- 4) TransformBlock：限并发 + 保序输出 ----------
    ///     public async Task TransformDemoAsync()
    ///     {
    ///         var square = new TransformBlock&lt;int, int&gt;(
    ///             x => x * x,                       // 也可传 async 委托：async x => { await Task.Yield(); return x * x; }
    ///             new ExecutionDataflowBlockOptions
    ///             {
    ///                 MaxDegreeOfParallelism = 4,   // 4 路并发处理
    ///                 BoundedCapacity = 8,          // 背压：上游过快时 SendAsync 等待
    ///             });
    ///         var sink = new ActionBlock&lt;int&gt;(x => Console.WriteLine(x));
    ///         square.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });
    ///         for (int i = 0; i &lt; 20; i++)
    ///         {
    ///             bool ok = square.Post(i);         // Post 立即返回；队列满时为 false，消息需自行补偿
    ///             if (!ok) await square.SendAsync(i); // 简化处理：Post 失败再退化为等待投递
    ///         }
    ///         square.Complete();
    ///         await sink.Completion;               // 输出顺序仍为 0..19：并发处理不改保序语义
    ///     }
    ///
    ///     // ---------- 5) BroadcastBlock：同一条最新消息扇出给多个目标 ----------
    ///     public void BroadcastDemo()
    ///     {
    ///         // 构造参数是“克隆函数”：引用类型建议深拷贝，避免多目标共享同一实例互相改
    ///         var tick = new BroadcastBlock&lt;int&gt;(value => value);
    ///         var a = new ActionBlock&lt;int&gt;(v => Console.WriteLine("A=" + v));
    ///         var b = new ActionBlock&lt;int&gt;(v => Console.WriteLine("B=" + v));
    ///         tick.LinkTo(a, new DataflowLinkOptions { PropagateCompletion = true });
    ///         tick.LinkTo(b, new DataflowLinkOptions { PropagateCompletion = true });
    ///         tick.Post(1); tick.Post(2);          // 慢消费者可能只收到 2：只保证最新值，不保证历史
    ///         tick.Complete();
    ///     }
    ///
    ///     // ---------- 6) BatchBlock：攒批输出 T[] ----------
    ///     public async Task BatchDemoAsync()
    ///     {
    ///         var batch = new BatchBlock&lt;int&gt;(
    ///             batchSize: 3,
    ///             new GroupingDataflowBlockOptions { BoundedCapacity = 10 }); // 攒批块用 Grouping 选项
    ///         var writer = new ActionBlock&lt;int[]&gt;(
    ///             arr => Console.WriteLine("写入一批: " + arr.Length));
    ///         batch.LinkTo(writer, new DataflowLinkOptions { PropagateCompletion = true });
    ///         for (int i = 1; i &lt;= 7; i++) await batch.SendAsync(i); // 两整批(6) + 余 1
    ///         batch.Complete();                    // Complete 时不足 3 条的余数也会作为最后一批输出
    ///         await writer.Completion;
    ///     }
    ///
    ///     // ---------- 7) WriteOnceBlock：一次性赋值，多读取者同值 ----------
    ///     public void WriteOnceDemo()
    ///     {
    ///         var once = new WriteOnceBlock&lt;string&gt;(value => value); // 参数同样是克隆委托
    ///         once.Post("init");                   // 第一条被接受
    ///         bool rejected = once.Post("again");  // false：永远拒绝后续消息
    ///         string v1 = once.Receive();          // init
    ///         string v2 = once.Receive();          // 仍是 init：所有接收者拿到同一值
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class HTplDataflowHelp
    {
        // net48 使用前请先 NuGet 安装 System.Threading.Tasks.Dataflow；
        // 上方 example 已覆盖：BufferBlock / ActionBlock / TransformBlock / TransformManyBlock /
        // BroadcastBlock / BatchBlock / WriteOnceBlock 七个块；
        // ExecutionDataflowBlockOptions（MaxDegreeOfParallelism / BoundedCapacity /
        // CancellationToken / SingleProducerConstrained / MaxMessagesPerTask）；
        // GroupingDataflowBlockOptions（BatchBlock 用，含 Greedy 非贪婪凑批）；
        // DataflowLinkOptions（PropagateCompletion / Append / MaxMessages）；
        // SendAsync / Post / ReceiveAsync / Complete / Completion / LinkTo 谓词分流与兜底。
        // 本类故意保持空实现，以免未安装该 NuGet 包的工程产生编译错误。
    }
}
