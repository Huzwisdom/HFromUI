using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HFileTransfer
{
    /// <summary>
    /// 文件传输进度事件参数。
    /// </summary>
    public class HFileProgressEventArgs : EventArgs
    {
        /// <summary>远端约定的文件名。</summary>
        public string FileName { set; get; }

        /// <summary>已传输字节数。</summary>
        public long BytesTransferred { set; get; }

        /// <summary>文件总字节数。</summary>
        public long TotalBytes { set; get; }

        /// <summary>完成百分比（0~100）。</summary>
        public int Percent { set; get; }

        /// <summary>发起传输的连接标识（TcpServerClient / 串口号 等）。</summary>
        public object Tag { set; get; }
    }

    /// <summary>
    /// 单次传输的取消上下文（服务端每个连接独立一份，可按连接取消）。
    /// </summary>
    internal sealed class HFileTransferContext
    {
        /// <summary>是否已请求取消本次传输。</summary>
        public volatile bool Cancel;
    }

    /// <summary>
    /// 服务端待发送工作项（推送文件到当前连接）。
    /// </summary>
    internal sealed class HFileWorkItem
    {
        /// <summary>本地待发送文件完整路径。</summary>
        public string FilePath;

        /// <summary>远端保存文件名（null 使用本地文件名）。</summary>
        public string RemoteName;

        /// <summary>本次推送的取消上下文。</summary>
        public HFileTransferContext Context = new HFileTransferContext();

        /// <summary>发送完成后的任务结果源。</summary>
        public TaskCompletionSource<bool> Tcs = new TaskCompletionSource<bool>();
    }

    /// <summary>帧头轮询读取结果。</summary>
    internal enum HHeaderRead
    {
        /// <summary>读到完整帧头。</summary>
        Packet,

        /// <summary>等待期间超时（无数据，正常空转）。</summary>
        Timeout,

        /// <summary>流断开或致命错误。</summary>
        Error
    }

    /// <summary>
    /// 大文件传输基类：在任意可读写字节流上实现分块发送、接收与断点续传，
    /// TCP（NetworkStream）与串口（SerialPort.BaseStream）共用同一套核心逻辑。
    /// 事件回调运行在后台工作线程，更新 UI 需自行 Invoke。
    /// </summary>
    public abstract class HFileTransferBase
    {
        /// <summary>默认网络分块大小（64KB）。</summary>
        public const int NetworkChunkSize = 64 * 1024;

        /// <summary>默认串口分块大小（1KB，适合低速串口逐块应答）。</summary>
        public const int SerialChunkSize = 1024;

        /// <summary>分片文件后缀。</summary>
        public const string PartExt = ".hftpart";

        /// <summary>分块大小（字节）：网口默认 65536，串口默认 1024。</summary>
        public int ChunkSize { set; get; } = NetworkChunkSize;

        /// <summary>读帧/等待应答超时（毫秒），超时触发重传。</summary>
        public int Timeout { set; get; } = 10000;

        /// <summary>单块最大重传次数，超过判为失败。</summary>
        public int MaxRetry { set; get; } = 3;

        /// <summary>是否正在传输。</summary>
        public bool IsBusy { protected set; get; }

        /// <summary>报错提示。</summary>
        public string StrError { get; protected set; }

        /// <summary>
        /// 保存路径决策：(连接标识, 文件名, 文件长度) → 本地完整保存路径；
        /// 返回 null 或空表示拒绝接收。服务端用此事件让业务侧按客户端决定落盘位置。
        /// </summary>
        public Func<object, string, long, string> SavePathResolver { set; get; }

        /// <summary>传输进度变化（后台线程触发）。</summary>
        public event EventHandler<HFileProgressEventArgs> ProgressChanged;

        /// <summary>接收完成：(连接标识, 最终文件路径, 文件名)。</summary>
        public event Action<object, string, string> FileReceived;

        /// <summary>传输失败：(连接标识, 错误信息)。</summary>
        public event Action<object, string> TransferError;

        /// <summary>触发进度事件。</summary>
        protected void RaiseProgress(object tag, string fileName, long transferred, long total)
        {
            int percent = total > 0 ? (int)(transferred * 100 / total) : 0;
            ProgressChanged?.Invoke(this, new HFileProgressEventArgs
            {
                FileName = fileName,
                BytesTransferred = transferred,
                TotalBytes = total,
                Percent = percent,
                Tag = tag
            });
        }

        /// <summary>触发接收完成事件。</summary>
        protected void RaiseReceived(object tag, string filePath, string fileName)
        {
            FileReceived?.Invoke(tag, filePath, fileName);
        }

        /// <summary>触发错误事件并记录错误文本。</summary>
        protected void RaiseError(object tag, string error)
        {
            StrError = error;
            TransferError?.Invoke(tag, error);
        }

        /// <summary>
        /// 在流上发送文件（支持断点续传：先问接收方已收偏移，再从该偏移继续）。
        /// </summary>
        /// <param name="stream">已连接的双向流</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="remoteName">对端保存文件名（null 使用本地文件名）</param>
        /// <param name="tag">连接标识</param>
        /// <param name="context">取消上下文</param>
        /// <returns>是否发送并被对端确认完成</returns>
        private protected bool SendFileCore(Stream stream, string filePath, string remoteName,
            object tag, HFileTransferContext context)
        {
            IsBusy = true;
            try
            {
                return SendFileInternal(stream, filePath, remoteName, tag, context);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>发送主流程。</summary>
        private bool SendFileInternal(Stream stream, string filePath, string remoteName,
            object tag, HFileTransferContext context)
        {
            if (!File.Exists(filePath))
            {
                RaiseError(tag, "Send file not found:" + filePath);
                return false;
            }
            var info = new FileInfo(filePath);
            string name = string.IsNullOrEmpty(remoteName) ? info.Name : remoteName;
            long size = info.Length;
            try
            {
                HFilePacket.Write(stream, HFilePacket.SendReq, HFilePacket.BuildSendReq(name, size));
                byte type;
                byte[] payload;
                if (!ReadPacket(stream, out type, out payload))
                {
                    RaiseError(tag, "Wait SendAck timeout/error");
                    return false;
                }
                if (type == HFilePacket.Cancel)
                {
                    RaiseError(tag, "Remote rejected/cancelled");
                    return false;
                }
                if (type != HFilePacket.SendAck)
                {
                    RaiseError(tag, "Protocol error: expect SendAck, got " + type);
                    return false;
                }
                long offset;
                bool accept;
                HFilePacket.ParseSendAck(payload, out offset, out accept);
                if (!accept)
                {
                    RaiseError(tag, "Remote refused the file");
                    return false;
                }
                if (offset < 0 || offset > size)
                {
                    RaiseError(tag, "Invalid resume offset:" + offset);
                    return false;
                }

                using (FileStream fs = File.OpenRead(filePath))
                {
                    fs.Position = offset;
                    var chunk = new byte[Math.Max(64, ChunkSize)];
                    while (fs.Position < size)
                    {
                        if (context.Cancel)
                        {
                            SafeWrite(stream, HFilePacket.Cancel, null);
                            RaiseError(tag, "Cancelled");
                            return false;
                        }
                        long pos = fs.Position;
                        int want = (int)Math.Min(chunk.Length, size - pos);
                        int count = ReadFill(fs, chunk, want);
                        if (count != want)
                        {
                            RaiseError(tag, "Local file short read at " + pos);
                            return false;
                        }
                        // 下一块位置（读到此处）；若需要重发本块则回退到 pos
                        long next = fs.Position;
                        byte[] dataPacket = HFilePacket.BuildData(pos, chunk, count);
                        bool done = false;
                        for (int retry = 0; retry <= MaxRetry; retry++)
                        {
                            HFilePacket.Write(stream, HFilePacket.Data, dataPacket);
                            if (!ReadPacket(stream, out type, out payload))
                            {
                                // ACK 丢失（对端可能已收到）：重发同一数据块，由对端按偏移去重
                                if (retry == MaxRetry)
                                {
                                    RaiseError(tag, "DataAck timeout after retries at offset " + pos);
                                    return false;
                                }
                                continue;
                            }
                            if (type == HFilePacket.Cancel)
                            {
                                RaiseError(tag, "Remote cancelled");
                                return false;
                            }
                            if (type != HFilePacket.DataAck)
                            {
                                RaiseError(tag, "Protocol error: expect DataAck, got " + type);
                                return false;
                            }
                            long ack = HFilePacket.ParseDataAck(payload);
                            if (ack < pos || ack > size)
                            {
                                RaiseError(tag, "Invalid ack offset:" + ack);
                                return false;
                            }
                            if (ack == pos + count)
                            {
                                // 本块确认
                                fs.Position = next;
                                done = true;
                                break;
                            }
                            // 对端进度落后（重传场景）或超前（分片被补传）：按对端偏移整体重定位，
                            // 当前块作废，外层 while 从 ack 重新读块发送
                            fs.Position = ack;
                            done = true;
                            break;
                        }
                        if (!done)
                        {
                            RaiseError(tag, "Data not acknowledged at offset " + pos);
                            return false;
                        }
                        RaiseProgress(tag, name, fs.Position, size);
                    }

                    HFilePacket.Write(stream, HFilePacket.Finish, null);
                    if (!ReadPacket(stream, out type, out payload))
                    {
                        RaiseError(tag, "Wait FinishAck timeout/error");
                        return false;
                    }
                    if (type != HFilePacket.FinishAck || !HFilePacket.ParseFinishAck(payload))
                    {
                        RaiseError(tag, "Remote verify failed");
                        return false;
                    }
                    RaiseProgress(tag, name, size, size);
                    return true;
                }
            }
            catch (Exception ex)
            {
                RaiseError(tag, "Send error:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 在流上接收文件到目录（自动续传：同名 *.hftpart 存在则从其长度续收）。
        /// </summary>
        /// <param name="stream">已连接的双向流</param>
        /// <param name="saveDir">保存目录（与 SavePathResolver 二选一）</param>
        /// <param name="tag">连接标识</param>
        /// <param name="context">取消上下文</param>
        /// <returns>最终文件完整路径；失败/拒绝返回 null（原因见 StrError）</returns>
        private protected string ReceiveFileCore(Stream stream, string saveDir, object tag,
            HFileTransferContext context)
        {
            byte type;
            byte[] payload;
            if (!ReadPacket(stream, out type, out payload))
            {
                RaiseError(tag, "Wait SendReq timeout/error");
                return null;
            }
            return ReceiveFileCore(stream, saveDir, tag, context, type, payload);
        }

        /// <summary>接收主流程；首帧由服务端轮询循环预读时走此重载。</summary>
        private protected string ReceiveFileCore(Stream stream, string saveDir, object tag,
            HFileTransferContext context, byte firstType, byte[] firstPayload)
        {
            IsBusy = true;
            try
            {
                return ReceiveFileInternal(stream, saveDir, tag, context, firstType, firstPayload);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>接收主流程。</summary>
        private string ReceiveFileInternal(Stream stream, string saveDir, object tag,
            HFileTransferContext context, byte firstType, byte[] firstPayload)
        {
            if (firstType == HFilePacket.Cancel)
            {
                return null;
            }
            if (firstType != HFilePacket.SendReq)
            {
                RaiseError(tag, "Protocol error: expect SendReq, got " + firstType);
                return null;
            }
            string remoteName;
            long size;
            HFilePacket.ParseSendReq(firstPayload, out remoteName, out size);
            string safeName = SanitizeFileName(remoteName);
            string finalPath = SavePathResolver != null
                ? SavePathResolver(tag, safeName, size)
                : (string.IsNullOrEmpty(saveDir) ? null : Path.Combine(saveDir, safeName));
            if (string.IsNullOrWhiteSpace(finalPath))
            {
                HFilePacket.Write(stream, HFilePacket.SendAck, HFilePacket.BuildSendAck(0, false));
                RaiseError(tag, "File rejected:" + safeName);
                return null;
            }
            string partPath = finalPath + PartExt;
            try
            {
                string dir = Path.GetDirectoryName(finalPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                using (FileStream fs = new FileStream(partPath, FileMode.OpenOrCreate,
                    FileAccess.Write, FileShare.Read))
                {
                    long offset = fs.Length;
                    if (offset > size)
                    {
                        // 分片大于本次声明长度（换了同名文件）：作废重收
                        fs.SetLength(0);
                        offset = 0;
                    }
                    // 关键：流初始位置为 0，必须显式定位到分片末尾再追加，否则会从头覆盖
                    fs.Seek(offset, SeekOrigin.Begin);
                    HFilePacket.Write(stream, HFilePacket.SendAck,
                        HFilePacket.BuildSendAck(offset, true));
                    if (offset > 0)
                    {
                        RaiseProgress(tag, safeName, offset, size);
                    }

                    while (true)
                    {
                        if (context.Cancel)
                        {
                            SafeWrite(stream, HFilePacket.Cancel, null);
                            RaiseError(tag, "Cancelled");
                            return null;
                        }
                        byte type;
                        byte[] payload;
                        if (!ReadPacket(stream, out type, out payload))
                        {
                            RaiseError(tag, "Wait data/finish timeout, part kept at " + partPath);
                            return null;
                        }
                        switch (type)
                        {
                            case HFilePacket.Data:
                                long dataOffset = HFilePacket.ParseDataOffset(payload);
                                if (dataOffset == fs.Length && payload.Length > 8)
                                {
                                    fs.Write(payload, 8, payload.Length - 8);
                                    fs.Flush();
                                }
                                // 偏移不匹配（重复块/丢块）一律回 ACK 真实进度，发送方自行重定位
                                HFilePacket.Write(stream, HFilePacket.DataAck,
                                    HFilePacket.BuildDataAck(fs.Length));
                                RaiseProgress(tag, safeName, fs.Length, size);
                                break;

                            case HFilePacket.Finish:
                                if (fs.Length != size)
                                {
                                    HFilePacket.Write(stream, HFilePacket.FinishAck,
                                        HFilePacket.BuildFinishAck(false));
                                    RaiseError(tag, "Size mismatch: got " + fs.Length + ", expect " + size);
                                    return null;
                                }
                                fs.Flush();
                                break;

                            case HFilePacket.Cancel:
                                RaiseError(tag, "Remote cancelled, part kept at " + partPath);
                                return null;

                            default:
                                RaiseError(tag, "Protocol error: unexpected type " + type);
                                HFilePacket.Write(stream, HFilePacket.FinishAck,
                                    HFilePacket.BuildFinishAck(false));
                                return null;
                        }
                        if (type == HFilePacket.Finish)
                        {
                            break;
                        }
                    }
                }
                if (File.Exists(finalPath))
                {
                    File.Delete(finalPath);
                }
                File.Move(partPath, finalPath);
                HFilePacket.Write(stream, HFilePacket.FinishAck, HFilePacket.BuildFinishAck(true));
                RaiseReceived(tag, finalPath, safeName);
                return finalPath;
            }
            catch (Exception ex)
            {
                RaiseError(tag, "Receive error:" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 服务端连接工作循环：先处理推送队列，再以短超时轮询读取对端发送请求；
        /// TCP 服务端的每个客户端连接与串口服务端的每个站点共用本循环。
        /// </summary>
        /// <param name="stream">连接流</param>
        /// <param name="dequeue">取一个待推送工作项（无项返回 null）</param>
        /// <param name="alive">连接是否仍存活</param>
        /// <param name="tag">连接标识</param>
        /// <param name="context">循环级取消上下文（Stop 时取消，停止整个循环）</param>
        /// <param name="transferBound">每次对端发起接收前回调，绑定/解绑本次传输的取消上下文（null 不绑定）</param>
        private protected void ServeLoopCore(Stream stream, Func<HFileWorkItem> dequeue,
            Func<bool> alive, object tag, HFileTransferContext context,
            Action<HFileTransferContext> transferBound = null)
        {
            while (alive() && !context.Cancel)
            {
                HFileWorkItem item = dequeue();
                if (item != null)
                {
                    bool ok = SendFileCore(stream, item.FilePath, item.RemoteName, tag, item.Context);
                    item.Tcs.TrySetResult(ok);
                    continue;
                }
                HHeaderRead hr = ReadHeaderPolled(stream, 50, out byte type, out int length);
                if (hr == HHeaderRead.Timeout)
                {
                    continue;
                }
                if (hr == HHeaderRead.Error)
                {
                    break;
                }
                byte[] payload = length == 0 ? new byte[0] : new byte[length];
                if (length > 0 && !ReadExact(stream, payload, length, Timeout))
                {
                    RaiseError(tag, "Frame payload read error");
                    break;
                }
                if (type == HFilePacket.SendReq)
                {
                    // 每次接收使用独立上下文：取消本次传输不会杀掉整个服务循环，连接可继续续传
                    HFileTransferContext receiveContext = new HFileTransferContext();
                    transferBound?.Invoke(receiveContext);
                    ReceiveFileCore(stream, null, tag, receiveContext, type, payload);
                    transferBound?.Invoke(null);
                }
                // 其它帧类型在服务空闲态无会话上下文，丢弃后继续等待
            }
        }

        /// <summary>
        /// 轮询读取一帧帧头：第一个字节用短超时空转（便于穿插推送），
        /// 一旦有数据，剩余 8 字节用完整传输超时等待。
        /// </summary>
        private protected HHeaderRead ReadHeaderPolled(Stream stream, int pollTimeout,
            out byte type, out int payloadLength)
        {
            type = 0;
            payloadLength = 0;
            byte[] first = new byte[1];
            if (!ReadExact(stream, first, 1, pollTimeout))
            {
                return string.IsNullOrEmpty(StrError)
                    || StrError.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0
                    ? HHeaderRead.Timeout
                    : HHeaderRead.Error;
            }
            byte[] rest = new byte[HFilePacket.HeaderSize - 1];
            if (!ReadExact(stream, rest, rest.Length, Timeout))
            {
                return HHeaderRead.Error;
            }
            byte[] header = new byte[HFilePacket.HeaderSize];
            header[0] = first[0];
            Buffer.BlockCopy(rest, 0, header, 1, rest.Length);
            if (!HFilePacket.IsMagic(header))
            {
                RaiseError(null, "Bad magic, stream desynchronized");
                return HHeaderRead.Error;
            }
            type = header[4];
            payloadLength = BitConverter.ToInt32(header, 5);
            if (payloadLength < 0)
            {
                RaiseError(null, "Bad frame length");
                return HHeaderRead.Error;
            }
            return HHeaderRead.Packet;
        }

        /// <summary>读取完整一帧（帧头 + 负载），使用 <see cref="Timeout"/> 作为读超时。</summary>
        protected bool ReadPacket(Stream stream, out byte type, out byte[] payload)
        {
            payload = null;
            byte[] header = new byte[HFilePacket.HeaderSize];
            if (!ReadExact(stream, header, HFilePacket.HeaderSize, Timeout))
            {
                type = 0;
                return false;
            }
            if (!HFilePacket.IsMagic(header))
            {
                type = 0;
                StrError = "Bad magic, stream desynchronized";
                return false;
            }
            type = header[4];
            int length = BitConverter.ToInt32(header, 5);
            if (length < 0)
            {
                StrError = "Bad frame length";
                return false;
            }
            payload = new byte[length];
            if (length > 0 && !ReadExact(stream, payload, length, Timeout))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 从流中精确读取 count 个字节（处理短读）；超时或断开返回 false 并写 StrError。
        /// </summary>
        protected bool ReadExact(Stream stream, byte[] buffer, int count, int timeout)
        {
            int oldTimeout = stream.CanTimeout ? stream.ReadTimeout : 0;
            try
            {
                if (stream.CanTimeout)
                {
                    stream.ReadTimeout = timeout;
                }
                int total = 0;
                while (total < count)
                {
                    int read = stream.Read(buffer, total, count - total);
                    if (read <= 0)
                    {
                        StrError = "Stream closed";
                        return false;
                    }
                    total += read;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (IsTimeout(ex))
                {
                    StrError = "Read timeout";
                }
                else
                {
                    StrError = ex.Message;
                }
                return false;
            }
            finally
            {
                if (stream.CanTimeout)
                {
                    stream.ReadTimeout = oldTimeout;
                }
            }
        }

        /// <summary>尽力清空流中残留数据（协议同步/传输前清缓存），返回丢弃字节数。</summary>
        protected static int ClearStreamCache(Stream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                return 0;
            }
            int total = 0;
            int oldTimeout = stream.CanTimeout ? stream.ReadTimeout : 0;
            try
            {
                if (stream.CanTimeout)
                {
                    stream.ReadTimeout = 20;
                }
                byte[] buffer = new byte[4096];
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }
                    total += read;
                }
            }
            catch
            {
                // 超时即代表缓冲已空；清缓存为尽力操作，不抛异常
            }
            finally
            {
                if (stream.CanTimeout)
                {
                    stream.ReadTimeout = oldTimeout;
                }
            }
            return total;
        }

        /// <summary>发送帧时吞掉异常（用于取消/失败后的通知帧）。</summary>
        protected static void SafeWrite(Stream stream, byte type, byte[] payload)
        {
            try
            {
                HFilePacket.Write(stream, type, payload);
            }
            catch
            {
                // 连接已断时通知帧发不出去属预期
            }
        }

        /// <summary>从文件流尽量读满 count 字节（仅末尾可能短读）。</summary>
        private static int ReadFill(Stream stream, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = stream.Read(buffer, total, count - total);
                if (read <= 0)
                {
                    break;
                }
                total += read;
            }
            return total;
        }

        /// <summary>文件名净化：只保留文件名部分，非法字符替换为下划线，防路径穿越。</summary>
        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "unnamed_" + DateTime.Now.Ticks;
            }
            string leaf = Path.GetFileName(name.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar));
            if (string.IsNullOrEmpty(leaf))
            {
                leaf = "unnamed_" + DateTime.Now.Ticks;
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                leaf = leaf.Replace(c, '_');
            }
            return leaf;
        }

        /// <summary>判断异常是否为读超时（网络流包 IOException，串口流抛 TimeoutException）。</summary>
        private static bool IsTimeout(Exception ex)
        {
            if (ex is TimeoutException)
            {
                return true;
            }
            var io = ex as IOException;
            if (io != null)
            {
                Exception inner = io.InnerException;
                if (inner is TimeoutException)
                {
                    return true;
                }
                var socket = inner as System.Net.Sockets.SocketException;
                if (socket != null && (socket.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut
                    || socket.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
