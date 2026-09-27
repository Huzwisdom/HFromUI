using System.Runtime.InteropServices;
using System.Threading;

namespace HFromUI.HThread.Help.ThreadCreate
{
    /// <summary>
    /// IOCP（Windows I/O 完成端口）与线程池异步 I/O 帮助类：仅说明原理与骨架，net48 下不放可执行方法。
    ///
    /// 【是什么】IOCP（I/O Completion Port，I/O 完成端口）是 Windows 内核提供的高性能异步 I/O 机制：
    /// 设备（文件、Socket、命名管道等）以 overlapped（重叠/异步）方式发起读写下落后立即返回，
    /// 内核完成硬件操作后把“完成包”投递到完成端口队列，少量线程循环 GetQueuedCompletionStatus 取包处理，
    /// 用几个线程即可承载成千上万并发 I/O。CLR 线程池与一个完成端口绑定，专门拿出“完成端口线程
    /// （completionPortThreads，与工作线程 workerThreads 是两条独立队列）”消费这些完成包。
    /// net48 下手工接入的关键 API：
    /// - <see cref="ThreadPool.BindHandle(SafeHandle)"/>
    ///   （另有已过时的 IntPtr 重载）：把操作系统句柄绑定到线程池的完成端口；
    /// - System.Threading.Overlapped.Overlapped.Pack(IOCompletionCallback, object)：
    ///   把回调与状态/缓冲区打包并“钉住”内存，返回 NativeOverlapped*，供 ReadFile/WSARecv 等
    ///   Win32 重叠 I/O API 使用；Overlapped.UnsafePack 不流转 ExecutionContext；
    /// - <see cref="ThreadPool.UnsafeQueueNativeOverlapped(NativeOverlapped*)"/>：
    ///   把 NativeOverlapped* 排入线程池的 I/O 路径；
    /// - 完成后在 IOCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped*) 中
    ///   Overlapped.Unpack 解包、Overlapped.Free 释放非托管内存；Win32 GetOverlappedResult 可查询重叠操作结果；
    /// - 文件句柄必须用 FileOptions.Asynchronous（useAsync:true）打开，否则底层会退化成“假异步”
    ///   （在额外线程上同步阻塞），失去 IOCP 的意义。
    ///
    /// 【是否跨进程】否。完成端口是内核对象，但 CLR 线程池的绑定、回调分发都在本进程内。
    ///
    /// 【典型适用场景】
    /// 1) 编写高性能网络/存储基础库（Socket、文件、管道的海量并发异步读写）；
    /// 2) 需要把自定义支持重叠 I/O 的 Win32 句柄接入托管线程池；
    /// 3) 理解 APM/Task 异步 I/O 的底层实现，排查“假异步”、完成端口线程数等问题。
    /// 普通业务开发不会直接使用这些 API（见生产建议）。
    ///
    /// 【使用步骤（手工接入的大致流程，仅理解用）】
    /// 1) 用 FileOptions.Asynchronous 打开文件（或创建 overlapped Socket），拿到 SafeHandle；
    /// 2) ThreadPool.BindHandle(safeHandle) 把句柄绑到线程池完成端口；
    /// 3) new Overlapped(...) 并 Pack(IOCompletionCallback, state)（必要时传入要钉住的缓冲区数组），
    ///    得到 NativeOverlapped*；
    /// 4) 通过 P/Invoke 调 ReadFile/WSARecv，把 NativeOverlapped* 作为 lpOverlapped 传入；
    /// 5) 内核完成后回调在完成端口线程触发：Unpack 取状态、处理数据、Overlapped.Free 释放，
    ///    异常必须在回调内自行吞掉/记录。
    ///
    /// 【注意事项与坑】
    /// 1) Pack 会固定（pin）托管缓冲区，不当复用/忘记 Free 会造成内存固定与非托管内存泄漏；
    /// 2) 回调运行在线程池“完成端口线程”而非工作线程，与 QueueUserWorkItem 不共用爬坡逻辑，
    ///    调线程池规模时 completionPortThreads 与 workerThreads 两个数字要分开看；
    /// 3) Pack 会流转 ExecutionContext（UnsafePack/UnsafeQueueNativeOverlapped 不流转）；
    /// 4) 忘记 FileOptions.Asynchronous 是最常见的坑：FileStream 构造参数 useAsync=false 时，
    ///    BeginRead/异步读会用后台阻塞线程模拟异步，高并发下同样耗线程；
    /// 5) 与 APM 的关系：FileStream/Socket 的 BeginRead/EndRead（APM 模式）内部就是 IOCP 路径
    ///   （句柄以异步方式打开时），手工 Overlapped.Pack 只是把同一套机制裸露出来，二者不要重复绑定。
    ///
    /// 【版本可用性】
    /// - net48 可用：ThreadPool.BindHandle(SafeHandle/IntPtr)、Overlapped.Pack/UnsafePack/Unpack/Free、
    ///   ThreadPool.UnsafeQueueNativeOverlapped、FileOptions.Asynchronous、APM（BeginRead/EndRead）、
    ///   SocketAsyncEventArgs（System.Net.Sockets，.NET Framework 3.5 起即有，用于 Socket 高性能异步）；
    /// - net48【没有】：基于 Span/Memory 的 ValueTask I/O、System.IO.RandomAccess.ReadAsync(
    ///   Memory&lt;byte&gt;, SafeFileHandle, long, CancellationToken)（.NET Core 2.1+/.NET 5+ 提供），
    ///   以及 Socket 的 ReceiveAsync(Span&lt;byte&gt;) 等 Span 重载；net48 上对应需求请用 APM 或
    ///   TaskFactory.FromAsync 包装，不要手写 Overlapped.Pack。
    ///
    /// 【生产建议】业务代码直接用 APM（BeginRead/EndRead，可配合 TaskFactory.FromAsync 转 Task）
    /// 或 FileStream.ReadAsync/Socket Task 扩展方法即可；底层库作者才需要直接操作 Overlapped/IOCP。
    /// </summary>
    ///
    /// <example>
    /// 【以下为原理骨架，需要 /unsafe 编译且省略了 ReadFile 的 P/Invoke，生产请勿照抄，
    /// 直接使用 FileStream.BeginRead/EndRead 或 ReadAsync】
    /// <code>
    /// using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read,
    ///            FileShare.Read, 4096, FileOptions.Asynchronous))   // 必须 Asynchronous
    /// {
    ///     // 1) 把 OS 文件句柄绑定到线程池的 IOCP
    ///     ThreadPool.BindHandle(fs.SafeFileHandle);
    ///
    ///     byte[] buffer = new byte[4096];
    ///     Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, null);
    ///
    ///     // 2) Pack：固定缓冲区并注册完成回调，返回可传给 ReadFile 的 NativeOverlapped*
    ///     NativeOverlapped* native = overlapped.Pack(
    ///         delegate(uint errorCode, uint numBytes, NativeOverlapped* o)
    ///         {
    ///             try
    ///             {
    ///                 // 3) IOCP 完成后，本回调在线程池完成端口线程执行
    ///                 Overlapped.Unpack(o);
    ///                 Console.WriteLine("读到字节数：" + numBytes + "，错误码：" + errorCode);
    ///             }
    ///             finally
    ///             {
    ///                 Overlapped.Free(o);   // 必须释放，否则非托管内存泄漏
    ///             }
    ///         },
    ///         buffer);
    ///
    ///     // 4) 真实代码此处通过 P/Invoke 调 ReadFile(handle, buffer, len, out read, native)；
    ///     //    这里仅示意线程池 I/O 入队入口，正常由设备驱动在操作完成时投递完成包：
    ///     // ThreadPool.UnsafeQueueNativeOverlapped(native);
    /// }
    /// </code>
    /// 等价的生产写法（底层同样走 IOCP，但无需 unsafe、无需手工内存管理）：
    /// <code>
    /// using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read,
    ///            FileShare.Read, 4096, FileOptions.Asynchronous))
    /// {
    ///     byte[] buffer = new byte[4096];
    ///     IAsyncResult ar = fs.BeginRead(buffer, 0, buffer.Length, null, null);
    ///     int read = fs.EndRead(ar);   // APM：内部即 IOCP 完成端口路径
    /// }
    /// </code>
    /// </example>
    public static class HIoCompletionPortHelp
    {
        /// <summary>版本可用性提示：BindHandle/Overlapped.Pack/UnsafeQueueNativeOverlapped 在 net48 可用；
        /// RandomAccess.ReadAsync 与 ValueTask/Memory 版 I/O 仅 .NET Core 2.1+/.NET 5+ 可用。</summary>
        private const string AvailabilityNote =
            "net48: ThreadPool.BindHandle / Overlapped.Pack / UnsafeQueueNativeOverlapped available; "
            + "RandomAccess.ReadAsync and ValueTask I/O are .NET Core 2.1+ / .NET 5+ only.";

        /// <summary>生产建议提示：业务代码直接使用 APM(BeginRead/EndRead) 或 ReadAsync，不要手写 Overlapped.Pack。</summary>
        private const string ProductionAdvice =
            "Prefer APM (BeginRead/EndRead) or ReadAsync/Task-based async I/O in production; "
            + "manual Overlapped.Pack is for low-level library authors only.";

        /// <summary>打开方式提示：文件句柄必须以 FileOptions.Asynchronous（useAsync:true）创建，否则退化成假异步。</summary>
        private const string AsynchronousFlagRequired =
            "FileStream must be opened with FileOptions.Asynchronous, otherwise async I/O degrades to blocking threads.";

        // 本类故意不放任何可执行方法：直接操作 IOCP 需要 unsafe 代码、缓冲区固定与非托管内存管理，
        // 不适合反射自测。可执行的异步 I/O 请使用 FileStream.BeginRead/EndRead（APM）或 ReadAsync，
        // 线程池规模（工作线程/完成端口线程）的读取与调整见 HThreadPoolTuningHelp。
    }
}
