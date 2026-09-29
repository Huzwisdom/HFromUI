using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;
using HFromUI.HEnum;
using HFromUI.HSocket.DeviceID;

namespace HFromUI.HSocket.HNamedPipe
{
    /// <summary>
    /// 命名管道客户端 / 主动连接方（Windows 内核原生 IPC）：以 HNamedPipeAddress 中的
    /// 全局唯一管道名连接服务端，使用 PipeDirection.InOut 全双工通道，读与写各自独立，
    /// 可在接收数据的同时持续发送。
    /// <para>后台读线程持续排空管道并触发 DataReceived；Write 经写锁串行化并支持写超时；
    /// 主动 Read 从接收队列取数（支持读超时）。对端关闭时自动置为断开并触发 Disconnected。</para>
    /// </summary>
    public class SocketNamedPipeClient
    {
        private readonly object mSendLock = new object();
        private NamedPipeClientStream mPipe;
        private Thread mReader;
        private volatile bool mAlive;
        private bool mConnected;
        private readonly HPipeRecvBuffer mRecv = new HPipeRecvBuffer();

        /// <summary>命名管道连接地址（管道名/计算机名/超时/缓冲区）。</summary>
        public HNamedPipeAddress hNamedPipeAddress { set; get; }

        /// <summary>文本编码格式。</summary>
        public Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// 写超时与主动 Read 等待超时（毫秒），默认 -1 无限等待；
        /// 写超时后按管道故障处理（自动断开）。
        /// </summary>
        public int Timeout = -1;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否已连接。</summary>
        public bool IsConnected
        {
            get { return mConnected && mPipe != null && mPipe.IsConnected; }
        }

        /// <summary>收到对端数据（后台读线程触发，更新 UI 需 Invoke）。</summary>
        public event Action<SocketNamedPipeClient, byte[]> DataReceived;

        /// <summary>通讯异常（写超时/IO 异常等）。</summary>
        public event Action<SocketNamedPipeClient, Exception> ErrorOccurred;

        /// <summary>连接断开（对端关闭或本地 Close 不触发；仅对端异常断开触发）。</summary>
        public event Action<SocketNamedPipeClient> Disconnected;

        /// <summary>以管道短名构造（连接本机 \\.\pipe\pipeName）。</summary>
        /// <param name="pipeName">管道短名或 Global\ 前缀名</param>
        public SocketNamedPipeClient(string pipeName)
            : this(new HNamedPipeAddress { PipeName = pipeName })
        {
        }

        /// <summary>以 HNamedPipeAddress 构造。</summary>
        /// <param name="address">命名管道连接地址</param>
        public SocketNamedPipeClient(HNamedPipeAddress address)
        {
            hNamedPipeAddress = address ?? new HNamedPipeAddress();
            hNamedPipeAddress.SocketType = HSocketType.NamedPipe_Client;
        }

        /// <summary>
        /// 连接服务端并启动后台读线程；重复调用幂等。
        /// </summary>
        /// <returns>是否连接成功（失败原因见 StrError）</returns>
        public bool Connect()
        {
            if (IsConnected)
            {
                return true;
            }
            try
            {
                string name = hNamedPipeAddress.GetShortName();
                if (string.IsNullOrEmpty(name))
                {
                    StrError = "PipeName is empty";
                    return false;
                }
                int bufferSize = hNamedPipeAddress.BufferSize <= 0 ? 64 * 1024 : hNamedPipeAddress.BufferSize;
                // 必须用 Asynchronous（重叠 IO）句柄：阻塞句柄下同一句柄并发读与写会死锁，无法全双工
                var pipe = new NamedPipeClientStream(
                    string.IsNullOrWhiteSpace(hNamedPipeAddress.ServerName) ? "." : hNamedPipeAddress.ServerName,
                    name, PipeDirection.InOut, PipeOptions.Asynchronous,
                    TokenImpersonationLevel.None, HandleInheritability.None);
                int timeout = hNamedPipeAddress.ConnectTimeout;
                if (timeout > 0)
                {
                    pipe.Connect(timeout);
                }
                else
                {
                    pipe.Connect();
                }
                pipe.ReadMode = PipeTransmissionMode.Byte;
                mPipe = pipe;
                mAlive = true;
                mConnected = true;
                mRecv.Clear();
                mReader = new Thread(ReadLoop) { IsBackground = true, Name = "HNamedPipeClient-Reader" };
                mReader.Start();
                return true;
            }
            catch (Exception ex)
            {
                StrError = "Pipe Connect Error:" + ex.Message;
                mAlive = false;
                mConnected = false;
                return false;
            }
        }

        /// <summary>关闭连接（幂等；主动关闭不触发 Disconnected 事件）。</summary>
        public void Close()
        {
            bool wasConnected = mConnected;
            mAlive = false;
            mConnected = false;
            NamedPipeClientStream pipe = mPipe;
            mPipe = null;
            if (pipe != null)
            {
                try { pipe.Close(); } catch { /* 关闭阶段忽略 */ }
            }
            mRecv.WakeAll();
            Thread reader = mReader;
            if (reader != null && wasConnected && !ReferenceEquals(Thread.CurrentThread, reader))
            {
                reader.Join(1000);
            }
            mReader = null;
        }

        /// <summary>发送文本指令。</summary>
        /// <param name="strCommand">指令内容</param>
        /// <returns>写入字节数；未连接 -102，异常 -200</returns>
        public int Write(string strCommand)
        {
            return Write(Encoding.GetBytes(strCommand ?? string.Empty));
        }

        /// <summary>发送字节数据。</summary>
        /// <param name="bytes">数据字节</param>
        /// <returns>写入字节数；未连接 -101，写超时/异常 -200</returns>
        public int Write(byte[] bytes)
        {
            if (bytes == null)
            {
                StrError = "Data is null";
                return -101;
            }
            if (!IsConnected)
            {
                StrError = "Not connected";
                return -101;
            }
            Exception error;
            int result = HPipeIo.TimedWrite(mPipe, mSendLock, bytes, 0, bytes.Length, Timeout, out error);
            if (result < 0)
            {
                StrError = error == null ? "Write timeout" : error.Message;
                OnPeerClosed(error ?? new IOException("Pipe write failed"), true);
            }
            return result;
        }

        /// <summary>
        /// 主动读取一块文本（从接收队列取下一块，受 Timeout 控制）。
        /// </summary>
        /// <param name="strData">读到的文本</param>
        /// <returns>读取字节数；未连接 -101，超时 -9，断开 -102，异常 -200</returns>
        public int Read(ref string strData)
        {
            byte[] chunk;
            int result = TakeChunk(out chunk);
            if (result <= 0)
            {
                strData = string.Empty;
                return result;
            }
            strData = Encoding.GetString(chunk).TrimEnd('\0');
            return result;
        }

        /// <summary>
        /// 主动读取一块字节到调用方缓冲区（超长部分保留在队列头部供下次读取）。
        /// </summary>
        /// <param name="bytes">调用方缓冲区，返回实际内容</param>
        /// <returns>实际拷贝字节数；未连接 -101，超时 -9，断开 -102，异常 -200</returns>
        public int Read(ref byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                bytes = new byte[0];
                StrError = "Buffer is null or empty";
                return -101;
            }
            byte[] chunk;
            int result = TakeChunk(out chunk);
            if (result <= 0)
            {
                return result;
            }
            int count = Math.Min(chunk.Length, bytes.Length);
            Array.Copy(chunk, 0, bytes, 0, count);
            return count;
        }

        /// <summary>
        /// 清空接收缓存：丢弃接收队列中的全部数据块（管道内核缓冲由后台读线程持续排空，
        /// 清队列即丢弃全部待取数据），返回丢弃字节数。
        /// </summary>
        public int ClearCache()
        {
            return mRecv.Clear();
        }

        /// <summary>后台读循环：全双工的读方向，与写操作完全独立。</summary>
        private void ReadLoop()
        {
            int bufferSize = hNamedPipeAddress == null || hNamedPipeAddress.BufferSize <= 0
                ? 64 * 1024 : hNamedPipeAddress.BufferSize;
            try
            {
                while (mAlive)
                {
                    byte[] buffer = new byte[bufferSize];
                    int read = mPipe.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }
                    byte[] chunk = new byte[read];
                    Array.Copy(buffer, 0, chunk, 0, read);
                    mRecv.Enqueue(chunk);
                    DataReceived?.Invoke(this, chunk);
                }
                if (mAlive)
                {
                    // 对端正常关闭（Read 返回 0）：只通知断开，不算错误
                    OnPeerClosed(null, false);
                }
            }
            catch (Exception ex)
            {
                if (mAlive)
                {
                    OnPeerClosed(ex, true);
                }
            }
        }

        /// <summary>从接收队列取一块（等待受 Timeout 控制）。</summary>
        private int TakeChunk(out byte[] chunk)
        {
            chunk = null;
            if (!mConnected)
            {
                StrError = "Not connected";
                return -101;
            }
            if (!mRecv.TryTake(Timeout, out chunk))
            {
                if (!mAlive)
                {
                    StrError = "Connection break";
                    return -102;
                }
                return -9;
            }
            return chunk.Length;
        }

        /// <summary>对端关闭/管道故障：置断开、唤醒等待者并通知（避免重复通知）。</summary>
        /// <param name="ex">异常信息；正常关闭为 null</param>
        /// <param name="isError">是否为异常断开（true 才触发 ErrorOccurred）</param>
        private void OnPeerClosed(Exception ex, bool isError)
        {
            bool notify = mConnected;
            mAlive = false;
            mConnected = false;
            mRecv.WakeAll();
            if (isError && ex != null)
            {
                StrError = ex.Message;
                if (notify)
                {
                    try { ErrorOccurred?.Invoke(this, ex); } catch { /* 事件异常不影响回收 */ }
                }
            }
            if (notify)
            {
                try { Disconnected?.Invoke(this); } catch { /* 事件异常不影响回收 */ }
            }
        }
    }

    /// <summary>命名管道接收队列：后台读线程入队，主动 Read 出队，支持等待超时与清空。</summary>
    internal sealed class HPipeRecvBuffer
    {
        private readonly Queue<byte[]> mQueue = new Queue<byte[]>();
        private readonly object mGate = new object();
        private readonly AutoResetEvent mArrived = new AutoResetEvent(false);

        /// <summary>入队一块数据并唤醒一个等待者。</summary>
        public void Enqueue(byte[] chunk)
        {
            lock (mGate)
            {
                mQueue.Enqueue(chunk);
            }
            mArrived.Set();
        }

        /// <summary>等待并取出一块；超时返回 false。</summary>
        public bool TryTake(int timeoutMs, out byte[] chunk)
        {
            chunk = null;
            if (!mArrived.WaitOne(timeoutMs < 0 ? -1 : timeoutMs))
            {
                return false;
            }
            lock (mGate)
            {
                if (mQueue.Count == 0)
                {
                    return false;
                }
                chunk = mQueue.Dequeue();
                if (mQueue.Count > 0)
                {
                    mArrived.Set();
                }
                return true;
            }
        }

        /// <summary>清空队列，返回累计丢弃字节数。</summary>
        public int Clear()
        {
            lock (mGate)
            {
                int total = 0;
                foreach (byte[] chunk in mQueue)
                {
                    total += chunk.Length;
                }
                mQueue.Clear();
                return total;
            }
        }

        /// <summary>唤醒全部等待者（关闭/断线时调用）。</summary>
        public void WakeAll()
        {
            mArrived.Set();
        }
    }

    /// <summary>命名管道同步写入工具：写锁串行化 + 异步写实现写超时（PipeStream 不支持 WriteTimeout）。</summary>
    internal static class HPipeIo
    {
        /// <summary>
        /// 带超时写入。
        /// </summary>
        /// <param name="stream">管道流</param>
        /// <param name="writeLock">写锁（同管道多线程写时串行化）</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">起始偏移</param>
        /// <param name="count">字节数</param>
        /// <param name="timeoutMs">超时毫秒，&lt;=0 无限等待</param>
        /// <param name="error">失败时的异常信息</param>
        /// <returns>写入字节数；失败 -200</returns>
        public static int TimedWrite(Stream stream, object writeLock, byte[] buffer,
            int offset, int count, int timeoutMs, out Exception error)
        {
            error = null;
            if (stream == null)
            {
                error = new IOException("Pipe is not open");
                return -200;
            }
            lock (writeLock)
            {
                try
                {
                    IAsyncResult ar = stream.BeginWrite(buffer, offset, count, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(timeoutMs <= 0 ? -1 : timeoutMs))
                    {
                        error = new TimeoutException("Pipe write timeout " + timeoutMs + "ms");
                        TryClose(stream);
                        return -200;
                    }
                    stream.EndWrite(ar);
                    stream.Flush();
                    return count;
                }
                catch (Exception ex)
                {
                    error = ex;
                    return -200;
                }
            }
        }

        /// <summary>写超时后尽力关闭故障管道（解除阻塞中的读）。</summary>
        private static void TryClose(Stream stream)
        {
            try { stream.Close(); } catch { /* 忽略 */ }
        }
    }
}
