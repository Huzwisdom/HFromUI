using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using HFromUI.HEnum;
using HFromUI.HSocket.DeviceID;

namespace HFromUI.HSocket.HNamedPipe
{
    /// <summary>
    /// 命名管道服务端 / 监听方（Windows 内核原生 IPC）：以内核对象的全局唯一管道名
    /// （\\.\pipe\名称）对外提供通道，使用 PipeDirection.InOut 全双工实例，支持同一管道名
    /// 挂接多个客户端（最多 MaxServerInstances 个并发实例）。
    /// <para>每个客户端会话拥有独立的后台读线程与写锁：服务端可在接收任意客户端数据的同时，
    /// 向任意客户端主动推送，收发互不阻塞。事件均在后台线程触发，更新 UI 需 Invoke。</para>
    /// </summary>
    public class SocketNamedPipeServer
    {
        /// <summary>单个客户端连接的工作会话。</summary>
        private sealed class PipeSession
        {
            public string Id;
            public NamedPipeServerStream Stream;
            public Thread Reader;
            public readonly object WriteLock = new object();
            public readonly HPipeRecvBuffer Recv = new HPipeRecvBuffer();
            public volatile bool Alive;
        }

        private readonly Dictionary<string, PipeSession> mSessions =
            new Dictionary<string, PipeSession>(StringComparer.Ordinal);
        private readonly object mLock = new object();
        private Thread mAcceptThread;
        private volatile bool mRunning;
        private NamedPipeServerStream mPendingServer;

        /// <summary>命名管道监听地址（管道名/最大实例/缓冲区）。</summary>
        public HNamedPipeAddress hNamedPipeAddress { set; get; }

        /// <summary>文本编码格式。</summary>
        public Encoding Encoding = Encoding.UTF8;

        /// <summary>写超时（毫秒），默认 -1 无限等待；超时按会话故障处理（自动断开该客户端）。</summary>
        public int Timeout = -1;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否处于监听状态。</summary>
        public bool IsRunning
        {
            get { return mRunning; }
        }

        /// <summary>当前已连接的客户端数量。</summary>
        public int ClientCount
        {
            get
            {
                lock (mLock)
                {
                    return mSessions.Count;
                }
            }
        }

        /// <summary>客户端接入：(服务端, 客户端会话ID)。</summary>
        public event Action<SocketNamedPipeServer, string> StationConnected;

        /// <summary>客户端断开（含异常掉线与主动移除）：(服务端, 客户端会话ID)。</summary>
        public event Action<SocketNamedPipeServer, string> StationDisconnected;

        /// <summary>收到某客户端数据：(服务端, 客户端会话ID, 数据)。</summary>
        public event Action<SocketNamedPipeServer, string, byte[]> DataReceived;

        /// <summary>会话异常：(服务端, 客户端会话ID, 错误信息)。</summary>
        public event Action<SocketNamedPipeServer, string, string> StationError;

        /// <summary>以管道短名构造（需再调 Start 开始监听）。</summary>
        /// <param name="pipeName">管道短名或 Global\ 前缀名</param>
        public SocketNamedPipeServer(string pipeName)
            : this(new HNamedPipeAddress { PipeName = pipeName })
        {
        }

        /// <summary>以 HNamedPipeAddress 构造。</summary>
        /// <param name="address">命名管道监听地址</param>
        public SocketNamedPipeServer(HNamedPipeAddress address)
        {
            hNamedPipeAddress = address ?? new HNamedPipeAddress();
            hNamedPipeAddress.SocketType = HSocketType.NamedPipe_Server;
        }

        /// <summary>
        /// 开始监听（后台线程等待客户端连接，接入后自动挂接读线程并继续等待下一个实例）。
        /// 重复调用幂等。
        /// </summary>
        /// <returns>是否成功启动监听（失败原因见 StrError）</returns>
        public bool Start()
        {
            if (mRunning)
            {
                return true;
            }
            string name = hNamedPipeAddress.GetShortName();
            if (string.IsNullOrEmpty(name))
            {
                StrError = "PipeName is empty";
                return false;
            }
            mRunning = true;
            mAcceptThread = new Thread(AcceptLoop)
            {
                IsBackground = true,
                Name = "HNamedPipeServer-Accept"
            };
            mAcceptThread.Start();
            return true;
        }

        /// <summary>停止监听并断开全部客户端（幂等）。</summary>
        public void Stop()
        {
            Close();
        }

        /// <summary>停止监听并断开全部客户端（同 Stop）。</summary>
        public void Close()
        {
            mRunning = false;
            NamedPipeServerStream pending = mPendingServer;
            mPendingServer = null;
            if (pending != null)
            {
                try { pending.Close(); } catch { /* 解除 Accept 阻塞 */ }
            }
            List<PipeSession> snapshot;
            lock (mLock)
            {
                snapshot = new List<PipeSession>(mSessions.Values);
                mSessions.Clear();
            }
            foreach (PipeSession session in snapshot)
            {
                ShutdownSession(session, false);
            }
        }

        /// <summary>
        /// 断开并移除指定客户端会话。
        /// </summary>
        /// <param name="clientId">客户端会话 ID（StationConnected 事件中获得）</param>
        public void RemoveStation(string clientId)
        {
            PipeSession session;
            lock (mLock)
            {
                if (!mSessions.TryGetValue(clientId ?? string.Empty, out session))
                {
                    return;
                }
                mSessions.Remove(clientId ?? string.Empty);
            }
            ShutdownSession(session, true);
        }

        /// <summary>指定客户端是否在线。</summary>
        public bool IsStationOpen(string clientId)
        {
            lock (mLock)
            {
                return mSessions.ContainsKey(clientId ?? string.Empty);
            }
        }

        /// <summary>获取全部在线客户端会话 ID 副本。</summary>
        public List<string> GetStations()
        {
            lock (mLock)
            {
                return new List<string>(mSessions.Keys);
            }
        }

        /// <summary>向指定客户端发送字节。</summary>
        /// <param name="clientId">客户端会话 ID</param>
        /// <param name="bytes">数据</param>
        /// <returns>写入字节数；客户端不在线 -101，异常 -200</returns>
        public int Write(string clientId, byte[] bytes)
        {
            PipeSession session = GetSession(clientId);
            if (session == null)
            {
                StrError = "Station not connected: " + clientId;
                return -101;
            }
            Exception error;
            int result = HPipeIo.TimedWrite(session.Stream, session.WriteLock,
                bytes, 0, bytes.Length, Timeout, out error);
            if (result < 0)
            {
                StrError = error == null ? "Write timeout" : error.Message;
                HandleSessionClosed(session, error ?? new IOException("Pipe write failed"), true);
            }
            return result;
        }

        /// <summary>向指定客户端发送文本。</summary>
        public int Write(string clientId, string strCommand)
        {
            return Write(clientId, Encoding.GetBytes(strCommand ?? string.Empty));
        }

        /// <summary>向全部在线客户端发送字节，返回至少写入成功的客户端数。</summary>
        public int WriteAll(byte[] bytes)
        {
            int success = 0;
            foreach (string clientId in GetStations())
            {
                if (Write(clientId, bytes) >= 0)
                {
                    success++;
                }
            }
            return success;
        }

        /// <summary>向全部在线客户端发送文本。</summary>
        public int WriteAll(string strCommand)
        {
            return WriteAll(Encoding.GetBytes(strCommand ?? string.Empty));
        }

        /// <summary>主动读取指定客户端的一块文本（受 Timeout 控制）。</summary>
        public int Read(string clientId, ref string strData)
        {
            PipeSession session = GetSession(clientId);
            if (session == null)
            {
                strData = string.Empty;
                StrError = "Station not connected: " + clientId;
                return -101;
            }
            byte[] chunk;
            if (!session.Recv.TryTake(Timeout, out chunk))
            {
                return -9;
            }
            strData = Encoding.GetString(chunk).TrimEnd('\0');
            return chunk.Length;
        }

        /// <summary>清空指定客户端的接收队列，返回丢弃字节数；客户端不在线返回 0。</summary>
        public int ClearCache(string clientId)
        {
            PipeSession session = GetSession(clientId);
            return session == null ? 0 : session.Recv.Clear();
        }

        /// <summary>清空全部客户端接收队列，返回累计丢弃字节数。</summary>
        public int ClearCacheAll()
        {
            int total = 0;
            foreach (string clientId in GetStations())
            {
                PipeSession session = GetSession(clientId);
                if (session != null)
                {
                    total += session.Recv.Clear();
                }
            }
            return total;
        }

        /// <summary>等待连接循环：每接入一个实例立即挂接会话，并立刻创建下一个等待实例。</summary>
        private void AcceptLoop()
        {
            string name = hNamedPipeAddress.GetShortName();
            int bufferSize = hNamedPipeAddress.BufferSize <= 0 ? 64 * 1024 : hNamedPipeAddress.BufferSize;
            int maxInstances = hNamedPipeAddress.MaxServerInstances <= 0
                ? NamedPipeServerStream.MaxAllowedServerInstances : hNamedPipeAddress.MaxServerInstances;
            while (mRunning)
            {
                NamedPipeServerStream server;
                try
                {
                    // 必须用 Asynchronous（重叠 IO）句柄：阻塞句柄下同一句柄并发读与写会死锁，无法全双工
                    server = new NamedPipeServerStream(name, PipeDirection.InOut, maxInstances,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous, bufferSize, bufferSize);
                    mPendingServer = server;
                    server.WaitForConnection();
                }
                catch (Exception ex)
                {
                    if (mRunning)
                    {
                        StrError = "Wait connection error: " + ex.Message;
                    }
                    break;
                }
                if (!mRunning)
                {
                    try { server.Close(); } catch { /* 忽略 */ }
                    break;
                }
                mPendingServer = null;
                OnConnected(server);
            }
            mRunning = false;
        }

        /// <summary>挂接新接入客户端：建会话、登记、启动后台读线程并通知。</summary>
        private void OnConnected(NamedPipeServerStream server)
        {
            var session = new PipeSession
            {
                Id = Guid.NewGuid().ToString("N"),
                Stream = server,
                Alive = true
            };
            lock (mLock)
            {
                mSessions[session.Id] = session;
            }
            session.Reader = new Thread(() => SessionReadLoop(session))
            {
                IsBackground = true,
                Name = "HNamedPipeServer-Reader-" + session.Id.Substring(0, 8)
            };
            session.Reader.Start();
            try { StationConnected?.Invoke(this, session.Id); } catch { /* 事件异常不影响会话 */ }
        }

        /// <summary>单客户端读循环：全双工的读方向，与向该客户端的写操作互不阻塞。</summary>
        private void SessionReadLoop(PipeSession session)
        {
            int bufferSize = hNamedPipeAddress == null || hNamedPipeAddress.BufferSize <= 0
                ? 64 * 1024 : hNamedPipeAddress.BufferSize;
            try
            {
                while (session.Alive && mRunning)
                {
                    byte[] buffer = new byte[bufferSize];
                    int read = session.Stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }
                    byte[] chunk = new byte[read];
                    Array.Copy(buffer, 0, chunk, 0, read);
                    session.Recv.Enqueue(chunk);
                    DataReceived?.Invoke(this, session.Id, chunk);
                }
                if (session.Alive)
                {
                    // 客户端正常关闭（Read 返回 0）：只通知断开，不算错误
                    HandleSessionClosed(session, null, false);
                }
            }
            catch (Exception ex)
            {
                if (session.Alive)
                {
                    HandleSessionClosed(session, ex, true);
                }
            }
        }

        /// <summary>会话结束处理：摘除会话，按需通知异常与断开（幂等）。</summary>
        /// <param name="session">会话</param>
        /// <param name="ex">异常；正常关闭为 null</param>
        /// <param name="isError">是否异常结束（true 触发 StationError）</param>
        private void HandleSessionClosed(PipeSession session, Exception ex, bool isError)
        {
            bool existed;
            lock (mLock)
            {
                existed = mSessions.Remove(session.Id);
            }
            if (!existed && !session.Alive)
            {
                return;
            }
            session.Alive = false;
            session.Recv.WakeAll();
            if (isError && ex != null)
            {
                StrError = ex.Message;
                try { StationError?.Invoke(this, session.Id, ex.Message); } catch { /* 忽略 */ }
            }
            try { StationDisconnected?.Invoke(this, session.Id); } catch { /* 忽略 */ }
            try { session.Stream.Close(); } catch { /* 忽略 */ }
        }

        /// <summary>关闭单个会话；raiseEvent=false 时为整体 Close，不再逐个发断开事件。</summary>
        private void ShutdownSession(PipeSession session, bool raiseEvent)
        {
            session.Alive = false;
            session.Recv.WakeAll();
            try { session.Stream.Close(); } catch { /* 忽略 */ }
            if (raiseEvent)
            {
                try { StationDisconnected?.Invoke(this, session.Id); } catch { /* 忽略 */ }
            }
        }

        /// <summary>按会话 ID 取会话。</summary>
        private PipeSession GetSession(string clientId)
        {
            lock (mLock)
            {
                PipeSession session;
                return mSessions.TryGetValue(clientId ?? string.Empty, out session) ? session : null;
            }
        }
    }
}
