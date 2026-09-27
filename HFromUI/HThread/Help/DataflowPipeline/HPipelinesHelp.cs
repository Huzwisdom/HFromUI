namespace HFromUI.HThread.Help.DataflowPipeline
{
    /// <summary>
    /// <para><b>【是什么】</b>
    /// System.IO.Pipelines 是高性能字节流 I/O 管道库：调度器统一管理<b>可复用缓冲区</b>（Memory&lt;byte&gt; 池化段、
    /// ReadOnlySequence&lt;byte&gt; 跨段视图）、异步就绪通知与背压，把传统 Stream 编程里“自己开 byte[]、
    /// 自己记读到哪、自己拷贝拼接半包”的工作全部接管。核心三类型：Pipe（进程内读写配对）、
    /// PipeReader（读端，可从 Stream/Socket 创建）、PipeWriter（写端，可写回 Stream）。
    /// 文档入口：https://learn.microsoft.com/zh-cn/dotnet/api/system.io.pipelines?view=net-8.0</para>
    ///
    /// <para><b>【是否跨进程：否】</b>
    /// Pipe 本身是进程内读写配对；PipeReader/Writer 只是对 Stream/Socket 的<b>本进程内缓冲包装</b>，
    /// 跨进程通信仍由底层 Stream/Socket 完成。</para>
    ///
    /// <para><b>【典型适用场景】</b>
    /// 1) 自定义 TCP 报文拆包/粘包处理、Redis/HTTP 等文本或二进制协议解析（Kestrel 的基础）；
    /// 2) 大文件/网络流的流式切分与转发，要求最少字节拷贝、最低 GC 压力；
    /// 3) 解码循环中“一次 ReadAsync 可能只来半包、要保留已 examined 位置继续等数据”的场景。</para>
    ///
    /// <para><b>【使用步骤】（读端循环五板斧）</b>
    /// 1) PipeReader.Create(stream)（或 new Pipe() 后取 Reader）；
    /// 2) ReadResult rr = await reader.ReadAsync(token)；
    /// 3) 在 rr.Buffer（ReadOnlySequence&lt;byte&gt;）里找完整帧，用 Slice/PositionOf 解析，不拷贝；
    /// 4) reader.AdvanceTo(consumed, examined)：consumed=已消费完的位置，examined=已检查到的位置（examined 不得落后于 consumed）；
    /// 5) rr.IsCompleted（写端 Complete 且数据取空）时退出，最后 reader.CompleteAsync()。
    /// 写端：writer.GetSpan/GetMemory 拿可写缓冲 → 手工写入 → writer.Advance(bytes) 申报长度 →
    /// await writer.FlushAsync() 推送并接受背压 → writer.CompleteAsync()。</para>
    ///
    /// <para><b>【注意事项与坑】</b>
    /// 1) <b>consumed / examined 语义</b>：consumed 之前的数据可被回收；examined 表示“我已经看到这了，
    ///    没有更多数据别再叫醒我”。半包时 consumed 不动、examined 推到 buffer.End；若 examined 不前进，
    ///    调度器不会再分配新数据唤醒，形成<b>假性死循环/饿死</b>；反过来对未解析数据宣告 consumed 会丢包。
    /// 2) <b>背压原理</b>：写端 FlushAsync 后，若读端未消费字节数超过 PipeOptions.PauseWriterThreshold，
    ///    返回的 ValueTask&lt;FlushResult&gt; 不完成，写端自然 await 暂停；回落到 ResumeWriterThreshold 以下才恢复，
    ///    从而内存占用有硬上限。Stream 适配版的背压阈值在 StreamPipeReaderOptions/StreamPipeWriterOptions 上配。
    /// 3) ReadResult.IsCanceled 表示被 CancelPendingRead 取消（不抛异常地让 ReadAsync 返回）；
    ///    FlushResult.IsCanceled 对应 CancelPendingFlush；IsCompleted 表示对端已 Complete。
    /// 4) 解析必须<b>幂等可重入</b>：一次 ReadAsync 的数据可能被检查多次，不要在解析路径放一次性副作用；
    ///    ReadOnlySequence 可能由多个内存段拼成（IsSingleSegment=false），不要假设一次 ToArray 拿到全部。
    /// 5) SequencePosition 是不透明位置标记（内部对象+整数），用 buffer.Start/End/Slice/PositionOf 获取，
    ///    不要自己 new，也不要跨不同 Pipe/缓冲区复用。
    /// 6) CompleteAsync(Exception) 可传异常把故障通知对端；两端都要 Complete，否则资源可能延迟释放。
    /// 7) 与 TPL Dataflow 分工：Dataflow 传递<b>对象消息</b>流水线；Pipelines 处理<b>裸字节流</b>解析。</para>
    ///
    /// <para><b>【版本可用性】</b>
    /// .NET Core 2.1+ / .NET 5+ 内置；<b>.NET Framework 4.8 不内置，需 NuGet 安装 System.IO.Pipelines</b>
    /// （官方包支持 net48，PipeReader.Create/Create(Stream) 在包版本内可用）。
    /// 本类保持空静态类可编译，真实用法全部见下方 example，装包后即可复制运行。</para>
    /// </summary>
    ///
    /// <example>
    /// 【.NET Core 2.1+，或 net48 安装 System.IO.Pipelines NuGet 包；示例纯内存、无文件/网络/UI】
    /// <code>
    /// using System;
    /// using System.Buffers;
    /// using System.IO;
    /// using System.IO.Pipelines;
    /// using System.Text;
    /// using System.Threading.Tasks;
    ///
    /// class PipelinesDemo
    /// {
    ///     // ============ 1) 进程内 Pipe：自写自读的“按行拆帧”，演示 consumed/examined 与背压 ============
    ///     public async Task InMemoryPipeDemoAsync()
    ///     {
    ///         var pipe = new Pipe(new PipeOptions(
    ///             pauseWriterThreshold: 64,     // 未消费字节达到 64：挂起写端 FlushAsync（背压上限）
    ///             resumeWriterThreshold: 32)); // 回落到 32 以下：恢复写端
    ///
    ///         // 写端任务：往托管缓冲里塞 3 行文本
    ///         Task writerTask = Task.Run(async () =>
    ///         {
    ///             PipeWriter w = pipe.Writer;
    ///             foreach (string line in new[] { "hello", "pipe", "end" })
    ///             {
    ///                 Memory&lt;byte&gt; mem = w.GetMemory();          // 向池申请可写缓冲，不自己 new byte[]
    ///                 int n = Encoding.UTF8.GetBytes(line + "\n", mem.Span); // 就地写入
    ///                 w.Advance(n);                                // 申报实际写入字节数
    ///                 ValueTask&lt;FlushResult&gt; flush = w.FlushAsync();
    ///                 FlushResult fr = await flush;               // 超暂停阈值时此处自动等待（背压）
    ///                 if (fr.IsCompleted) break;                  // 读端已 Complete，停止写
    ///             }
    ///             w.Complete();                                   // 通知 EOF
    ///         });
    ///
    ///         // 读端：标准五板斧循环，按 '\n' 切行
    ///         PipeReader r = pipe.Reader;
    ///         while (true)
    ///         {
    ///             ReadResult result = await r.ReadAsync();
    ///             ReadOnlySequence&lt;byte&gt; buffer = result.Buffer;
    ///             SequencePosition consumed = buffer.Start;
    ///             SequencePosition examined = buffer.End;        // 默认“全检查过”
    ///
    ///             // 在可能跨多段的序列里找分隔符，零拷贝
    ///             SequencePosition? nl = buffer.PositionOf((byte)'\n');
    ///             if (nl.HasValue)
    ///             {
    ///                 ReadOnlySequence&lt;byte&gt; line = buffer.Slice(buffer.Start, nl.Value);
    ///                 string text = Encoding.UTF8.GetString(line.ToArray()); // 仅演示；生产可用不拷贝的 Decoder
    ///                 Console.WriteLine(text);
    ///                 consumed = buffer.GetPosition(1, nl.Value); // 消费位置跨过换行符
    ///                 examined = consumed;                       // 该行已完整处理
    ///             }
    ///             // nl == null（半包）时：consumed=Start（什么都没消费），examined=End（已看到末尾，继续等数据）
    ///
    ///             r.AdvanceTo(consumed, examined);              // 两个位置都必须申报，examined 不得落后
    ///             if (result.IsCompleted) break;                // 写端 Complete 且缓冲取空
    ///         }
    ///         r.Complete();
    ///         await writerTask;
    ///     }
    ///
    ///     // ============ 2) Stream 适配：从任意 Stream 创建读端（Socket/FileStream 同形） ============
    ///     public async Task ReadFromStreamAsync(Stream stream)
    ///     {
    ///         PipeReader reader = PipeReader.Create(stream, new StreamPipeReaderOptions(
    ///             bufferSize: 4096,
    ///             minimumReadSize: 1024));
    ///         try
    ///         {
    ///             while (true)
    ///             {
    ///                 ReadResult result = await reader.ReadAsync();
    ///                 ReadOnlySequence&lt;byte&gt; buffer = result.Buffer;
    ///                 SequencePosition consumed = buffer.Start;
    ///                 SequencePosition examined = buffer.End;
    ///
    ///                 // 多段遍历时标准写法：先处理 First，再用 Slice 逐段推进
    ///                 foreach (ReadOnlyMemory&lt;byte&gt; segment in buffer)
    ///                 {
    ///                     // 对 segment.Span 做协议解析；IsSingleSegment 为 true 时只有一段
    ///                 }
    ///                 reader.AdvanceTo(consumed, examined);
    ///                 if (result.IsCanceled) break;              // 被 CancelPendingRead 取消
    ///                 if (result.IsCompleted) break;             // 对端 EOF
    ///             }
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             await reader.CompleteAsync(ex);               // 故障要带着异常 Complete 通知对端
    ///             return;
    ///         }
    ///         await reader.CompleteAsync();
    ///     }
    ///
    ///     // ============ 3) Stream 适配写端：GetSpan/Advance/FlushAsync 三件套 ============
    ///     public async Task WriteToStreamAsync(Stream stream, byte[] payload)
    ///     {
    ///         PipeWriter writer = PipeWriter.Create(stream, new StreamPipeWriterOptions(
    ///             minimumBufferSize: 4096));
    ///         writer.Write(payload);                             // 等价于 GetSpan+Advance 的封装
    ///         FlushResult result = await writer.FlushAsync();   // 推到底层 Stream；IsCanceled/IsCompleted 判状态
    ///         await writer.CompleteAsync();
    ///     }
    ///
    ///     // ============ 4) ReadOnlySequence&lt;T&gt; 常用只读 API 速查 ============
    ///     public void SequenceApiCheatsheet(ReadOnlySequence&lt;byte&gt; buffer)
    ///     {
    ///         long len = buffer.Length;                         // 跨段总字节数
    ///         bool empty = buffer.IsEmpty;                      // 没有任何字节
    ///         bool one = buffer.IsSingleSegment;                // 是否单段（单段可直接 First.Span 解析）
    ///         ReadOnlyMemory&lt;byte&gt; first = buffer.First;        // 首段
    ///         SequencePosition start = buffer.Start;            // 不透明起点
    ///         SequencePosition end = buffer.End;                // 不透明终点
    ///         SequencePosition? p = buffer.PositionOf((byte)'\r'); // 按元素定位，找不到返回 null
    ///         ReadOnlySequence&lt;byte&gt; head = buffer.Slice(0, 4);    // 按下标切片
    ///         ReadOnlySequence&lt;byte&gt; part = buffer.Slice(start, p ?? end); // 按位置切片
    ///         long offset = buffer.GetPosition(2).GetInteger(); // 位置转整数偏移仅用于调试
    ///     }
    /// }
    /// </code>
    /// 与 TPL Dataflow 区别：Dataflow 处理“对象消息流水线”（见 HTplDataflowHelp）；
    /// Pipelines 处理“裸字节流解析”，两者经常搭配：Pipelines 拆帧反序列化出对象后送 Dataflow 块处理。
    /// </example>
    public static class HPipelinesHelp
    {
        // net48 使用前请先 NuGet 安装 System.IO.Pipelines；
        // 上方 example 已覆盖：Pipe / PipeOptions(PauseWriterThreshold/ResumeWriterThreshold)、
        // PipeReader.Create(Stream, StreamPipeReaderOptions)、PipeWriter.Create(Stream, StreamPipeWriterOptions)、
        // ReadResult(Buffer / IsCompleted / IsCanceled)、AdvanceTo(consumed, examined)、
        // FlushAsync / FlushResult(IsCanceled/IsCompleted) / GetMemory / GetSpan / Advance / CompleteAsync、
        // CancelPendingRead / CancelPendingFlush、SequencePosition、ReadOnlySequence 切片与遍历、背压原理。
        // 本类故意保持空实现，以免未安装该 NuGet 包的工程产生编译错误。
    }
}
