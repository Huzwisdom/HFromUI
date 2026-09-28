using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HFileTransfer
{
    using HFromUI.HEnum;
    using HFromUI.HSocket.DeviceID;

    /// <summary>
    /// 网口大文件传输服务端：以 HIPAddress 本地监听，自动接受多客户端并发传输。
    /// 客户端连入后可直接发送文件（自动落盘并断点续传）；服务端也可通过 PushFile 向
    /// 处于接收等待中的客户端推送文件。事件均在后台线程触发，更新 UI 需 Invoke。
    /// </summary>
    public class HFileTcpServer : HFileTransferBase
    {
        /// <summary>单个客户端连接的工作会话。</summary>
        private sealed class ClientSession
        {
            public TcpClient Tcp;
            public NetworkStream Stream;
            public string Id;
            public readonly Queue<HFileWorkItem> PushQueue = new Queue<HFileWorkItem>();
            public readonly object QueueLock = new object();

            /// <summary>循环级上下文：仅 Stop/顶号时取消（结束工作循环）。</summary>
            public readonly HFileTransferContext Context = new HFileTransferContext();

            /// <summary>当前正在接收的传输上下文（取消只中止本次传输，连接保留）。</summary>
            public volatile HFileTransferContext Receiving;

            /// <summary>当前正在推送发送的传输上下文。</summary>
            public volatile HFileTransferContext Sending;
        }

        private TcpListener mListener;
        private Thread mAcceptThread;
        private volatile bool mRunning;
        private readonly Dictionary<string, ClientSession> mSessions = new Dictionary<string, ClientSession>();
        private readonly object mSessionLock = new object();

        /// <summary>监听地址：IP 空或 0.0.0.0 监听全部网卡，Port 为监听端口。</summary>
        public HIPAddress hIPAddress { set; get; }

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否正在监听。</summary>
        public bool IsRunning
        {
            get { return mRunning; }
        }

        /// <summary>当前在线客户端数量。</summary>
        public int ClientCount
        {
            get
            {
                lock (mSessionLock)
                {
                    return mSessions.Count;
                }
            }
        }

        /// <summary>客户端接入（文件工作循环已启动）。</summary>
        public event Action<HFileTcpServer, string> ClientConnected;

        /// <summary>客户端断开。</summary>
        public event Action<HFileTcpServer, string> ClientDisconnected;

        /// <summary>
        /// 请求保存路径：(客户端 Id, 文件名, 文件长度) → 本地完整保存路径；
        /// 不订阅或返回 null 时拒绝接收。
        /// </summary>
        public event Func<string, string, long, string> FileRequested;

        /// <summary>监听/连接异常。</summary>
        public event Action<HFileTcpServer, string> ServerError;

        /// <summary>以监听端口构造（监听全部网卡）。</summary>
        /// <param name="nPort">监听端口</param>
        public HFileTcpServer(int nPort) : this(new HIPAddress { Port = nPort })
        {
        }

        /// <summary>以监听 IP 与端口构造。</summary>
        /// <param name="strIp">监听 IP，空或 0.0.0.0 为全部网卡</param>
        /// <param name="nPort">监听端口</param>
        public HFileTcpServer(string strIp, int nPort)
            : this(new HIPAddress { IP = strIp, Port = nPort })
        {
        }

        /// <summary>以 HIPAddress 构造。</summary>
        /// <param name="hIP">监听地址</param>
        public HFileTcpServer(HIPAddress hIP)
        {
            hIPAddress = hIP ?? new HIPAddress();
            hIPAddress.SocketType = HSocketType.TCP_Server;
            ChunkSize = NetworkChunkSize;
            SavePathResolver = ResolveSavePath;
        }

        /// <summary>
        /// 开始监听；重复调用幂等。
        /// </summary>
        /// <returns>是否启动成功（如端口被占用返回 false，原因见 StrError）</returns>
        public bool Start()
        {
            if (mRunning)
            {
                return true;
            }
            try
            {
                hIPAddress.SocketType = HSocketType.TCP_Server;
                IPAddress bind = string.IsNullOrWhiteSpace(hIPAddress.IP) || hIPAddress.IP == "0.0.0.0"
                    ? IPAddress.Any
                    : IPAddress.Parse(hIPAddress.IP);
                mListener = new TcpListener(bind, hIPAddress.Port);
                mListener.Start();
                mRunning = true;
                mAcceptThread = new Thread(AcceptLoop)
                {
                    IsBackground = true,
                    Name = "HFileTcpServer-Accept"
                };
                mAcceptThread.Start();
                return true;
            }
            catch (SocketException ex)
            {
                mRunning = false;
                StrError = "Listen Error:" + ex.SocketErrorCode + "/" + ex.Message;
                ServerError?.Invoke(this, StrError);
                return false;
            }
            catch (Exception ex)
            {
                mRunning = false;
                StrError = "Listen Error:" + ex.Message;
                ServerError?.Invoke(this, StrError);
                return false;
            }
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
            TcpListener listener = mListener;
            mListener = null;
            try
            {
                listener?.Stop();
            }
            catch
            {
                // Accept 因停止抛异常属正常流程
            }
            List<ClientSession> snapshot;
            lock (mSessionLock)
            {
                snapshot = new List<ClientSession>(mSessions.Values);
                mSessions.Clear();
            }
            foreach (ClientSession session in snapshot)
            {
                session.Context.Cancel = true;
                try
                {
                    session.Stream?.Close();
                    session.Tcp?.Close();
                }
                catch
                {
                    // 批量关闭时忽略单个连接异常
                }
            }
        }

        /// <summary>获取在线客户端 Id 列表。</summary>
        /// <returns>Id 列表副本</returns>
        public List<string> GetClientIds()
        {
            lock (mSessionLock)
            {
                return new List<string>(mSessions.Keys);
            }
        }

        /// <summary>指定客户端是否在线。</summary>
        /// <param name="clientId">客户端 Id（远端“IP:端口”）</param>
        /// <returns>是否在线</returns>
        public bool IsClientOnline(string clientId)
        {
            lock (mSessionLock)
            {
                return mSessions.ContainsKey(clientId ?? string.Empty);
            }
        }

        /// <summary>
        /// 清空指定客户端接收缓存（丢弃流中残留字节，返回丢弃数量）。
        /// 警告：服务端工作线程会持续读取该连接，请勿在连接存活（尤其传输）期间调用，
        /// 否则两个线程并发读同一流可能导致工作循环异常退出；仅用于连接建立前的维护场景。
        /// </summary>
        /// <param name="clientId">客户端 Id</param>
        /// <returns>丢弃字节数；客户端不存在返回 0</returns>
        public int ClearCache(string clientId)
        {
            ClientSession session = GetSession(clientId);
            return session == null ? 0 : ClearStreamCache(session.Stream);
        }

        /// <summary>清空全部在线客户端的接收缓存。</summary>
        /// <returns>累计丢弃字节数</returns>
        public int ClearCacheAll()
        {
            int total = 0;
            foreach (ClientSession session in GetSessions())
            {
                total += ClearStreamCache(session.Stream);
            }
            return total;
        }

        /// <summary>取消指定客户端正在进行的传输（分片保留以便续传；连接不断开）。</summary>
        /// <param name="clientId">客户端 Id</param>
        public void CancelClient(string clientId)
        {
            ClientSession session = GetSession(clientId);
            if (session != null)
            {
                CancelSessionTransfers(session);
            }
        }

        /// <summary>取消全部客户端正在进行的传输（分片保留以便续传；连接不断开）。</summary>
        public void CancelAll()
        {
            foreach (ClientSession session in GetSessions())
            {
                CancelSessionTransfers(session);
            }
        }

        /// <summary>取消单个会话的在途传输与排队推送（不动循环级上下文）。</summary>
        private static void CancelSessionTransfers(ClientSession session)
        {
            HFileTransferContext receiving = session.Receiving;
            if (receiving != null)
            {
                receiving.Cancel = true;
            }
            HFileTransferContext sending = session.Sending;
            if (sending != null)
            {
                sending.Cancel = true;
            }
            lock (session.QueueLock)
            {
                while (session.PushQueue.Count > 0)
                {
                    HFileWorkItem item = session.PushQueue.Dequeue();
                    item.Context.Cancel = true;
                    item.Tcs.TrySetResult(false);
                }
            }
        }

        /// <summary>
        /// 向指定客户端排队推送一个文件（客户端必须正处于 ReceiveFile 等待中）。
        /// </summary>
        /// <param name="clientId">客户端 Id</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="remoteName">对端保存文件名（null 使用本地文件名）</param>
        /// <returns>发送结果任务；客户端不在线返回失败任务</returns>
        public Task<bool> PushFileAsync(string clientId, string filePath, string remoteName = null)
        {
            ClientSession session = GetSession(clientId);
            if (session == null)
            {
                return Task.FromResult(false);
            }
            var item = new HFileWorkItem { FilePath = filePath, RemoteName = remoteName };
            lock (session.QueueLock)
            {
                session.PushQueue.Enqueue(item);
            }
            return item.Tcs.Task;
        }

        /// <summary>推送文件（阻塞等待发送完成）。</summary>
        /// <param name="clientId">客户端 Id</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="remoteName">对端保存文件名（null 使用本地文件名）</param>
        /// <returns>是否发送并被确认完成</returns>
        public bool PushFile(string clientId, string filePath, string remoteName = null)
        {
            try
            {
                return PushFileAsync(clientId, filePath, remoteName).Result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>保存路径事件适配到基类解析器。</summary>
        private string ResolveSavePath(object tag, string fileName, long fileSize)
        {
            return FileRequested?.Invoke(tag as string, fileName, fileSize);
        }

        /// <summary>接受连接循环。</summary>
        private void AcceptLoop()
        {
            while (mRunning)
            {
                TcpClient tcp;
                try
                {
                    tcp = mListener.AcceptTcpClient();
                }
                catch
                {
                    if (!mRunning)
                    {
                        break;
                    }
                    continue;
                }
                try
                {
                    tcp.NoDelay = true;
                    tcp.SendBufferSize = ChunkSize * 2;
                    tcp.ReceiveBufferSize = ChunkSize * 2;
                    var session = new ClientSession
                    {
                        Tcp = tcp,
                        Stream = tcp.GetStream(),
                        Id = (tcp.Client.RemoteEndPoint as IPEndPoint)?.ToString()
                    };
                    AddSession(session);
                    var worker = new Thread(() => ClientWorker(session))
                    {
                        IsBackground = true,
                        Name = "HFileTcpServer-Worker"
                    };
                    worker.Start();
                    ClientConnected?.Invoke(this, session.Id);
                }
                catch (Exception ex)
                {
                    StrError = "Accept Error:" + ex.Message;
                    ServerError?.Invoke(this, StrError);
                    try
                    {
                        tcp.Close();
                    }
                    catch
                    {
                        // 接入失败关闭资源即可
                    }
                }
            }
        }

        /// <summary>单客户端工作循环（接收文件 / 排队推送复用基类轮询循环）。</summary>
        private void ClientWorker(ClientSession session)
        {
            try
            {
                ServeLoopCore(session.Stream, () =>
                {
                    HFileWorkItem item = Dequeue(session);
                    session.Sending = item == null ? null : item.Context;
                    return item;
                },
                    () => mRunning && session.Tcp.Connected, session.Id, session.Context,
                    ctx => session.Receiving = ctx);
            }
            catch (Exception ex)
            {
                StrError = "Worker Error:" + ex.Message;
                ServerError?.Invoke(this, StrError);
            }
            finally
            {
                RemoveSession(session);
                FailPending(session);
                try
                {
                    session.Stream?.Close();
                    session.Tcp?.Close();
                }
                catch
                {
                    // 断开时忽略资源关闭异常
                }
                ClientDisconnected?.Invoke(this, session.Id);
            }
        }

        /// <summary>取该客户端下一个待推送工作项。</summary>
        private static HFileWorkItem Dequeue(ClientSession session)
        {
            lock (session.QueueLock)
            {
                return session.PushQueue.Count > 0 ? session.PushQueue.Dequeue() : null;
            }
        }

        /// <summary>连接断开时把未完成的推送置为失败。</summary>
        private static void FailPending(ClientSession session)
        {
            lock (session.QueueLock)
            {
                while (session.PushQueue.Count > 0)
                {
                    session.PushQueue.Dequeue().Tcs.TrySetResult(false);
                }
            }
        }

        /// <summary>登记会话；同标识重复接入时顶掉旧连接。</summary>
        private void AddSession(ClientSession session)
        {
            ClientSession old;
            lock (mSessionLock)
            {
                mSessions.TryGetValue(session.Id, out old);
                mSessions[session.Id] = session;
            }
            if (old != null)
            {
                old.Context.Cancel = true;
                try
                {
                    old.Stream?.Close();
                    old.Tcp?.Close();
                }
                catch
                {
                    // 顶号关闭旧连接时忽略异常
                }
            }
        }

        /// <summary>移除会话（仅当仍是同一实例）。</summary>
        private void RemoveSession(ClientSession session)
        {
            lock (mSessionLock)
            {
                ClientSession current;
                if (mSessions.TryGetValue(session.Id, out current) && ReferenceEquals(current, session))
                {
                    mSessions.Remove(session.Id);
                }
            }
        }

        /// <summary>按 Id 取会话。</summary>
        private ClientSession GetSession(string clientId)
        {
            lock (mSessionLock)
            {
                ClientSession session;
                return mSessions.TryGetValue(clientId ?? string.Empty, out session) ? session : null;
            }
        }

        /// <summary>取会话快照。</summary>
        private List<ClientSession> GetSessions()
        {
            lock (mSessionLock)
            {
                return new List<ClientSession>(mSessions.Values);
            }
        }
    }
}
