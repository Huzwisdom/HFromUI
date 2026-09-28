using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HFromUI.HSocket.HTcpServer
{
    using HFromUI.HEnum;
    using HFromUI.HLangage;
    using HFromUI.HSocket.DeviceID;

    /// <summary>
    /// TCP 服务端：本地端口监听、自动接受多客户端接入，每个连接独立后台线程接收，
    /// 接口风格与 SocketTcpClient 保持一致（hIPAddress / Encoding / Timeout / DataLength / StrError）。
    /// 事件在后台工作线程触发，更新 UI 需自行 Invoke；停止监听不影响已打开客户端的关闭流程。
    /// </summary>
    public class SocketTcpServer
    {
        private TcpListener mListener;
        private Thread mAcceptThread;
        private volatile bool mRunning;
        private readonly Dictionary<string, TcpServerClient> mClients = new Dictionary<string, TcpServerClient>();
        private readonly object mClientLock = new object();

        /// <summary>监听地址：IP 留空或 0.0.0.0 表示监听全部网卡，Port 为监听端口。</summary>
        public HIPAddress hIPAddress { set; get; }

        /// <summary>发送超时时间（毫秒），-1 不限制。接收不设超时以保持长连接阻塞读取。</summary>
        public int Timeout = -1;

        /// <summary>单块收发缓冲长度（字节），-1 使用默认 8192。</summary>
        public int DataLength = -1;

        /// <summary>文本编码格式。</summary>
        public Encoding Encoding = Encoding.UTF8;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否正在监听。</summary>
        public bool IsRunning
        {
            get { return mRunning; }
        }

        /// <summary>实际监听端口（Port 传 0 由系统分配时，Start 成功后取此值）。</summary>
        public int LocalPort
        {
            get
            {
                return (mListener?.LocalEndpoint as IPEndPoint)?.Port ?? hIPAddress.Port;
            }
        }

        /// <summary>当前在线客户端数量。</summary>
        public int ClientCount
        {
            get
            {
                lock (mClientLock)
                {
                    return mClients.Count;
                }
            }
        }

        /// <summary>客户端接入（已开始接收）。</summary>
        public event Action<SocketTcpServer, TcpServerClient> ClientConnected;

        /// <summary>客户端断开（接收结束或连接关闭）。</summary>
        public event Action<SocketTcpServer, TcpServerClient> ClientDisconnected;

        /// <summary>收到客户端数据（data 为本帧字节副本，可直接保留）。</summary>
        public event Action<SocketTcpServer, TcpServerClient, byte[]> DataReceived;

        /// <summary>服务端异常（监听失败、接收异常等）。</summary>
        public event Action<SocketTcpServer, string> ServerError;

        /// <summary>以监听端口构造（监听全部网卡）。</summary>
        /// <param name="nPort">监听端口</param>
        public SocketTcpServer(int nPort) : this(new HIPAddress { Port = nPort })
        {
        }

        /// <summary>以监听 IP 与端口构造。</summary>
        /// <param name="strIp">监听 IP，空或 0.0.0.0 表示全部网卡</param>
        /// <param name="nPort">监听端口</param>
        public SocketTcpServer(string strIp, int nPort) : this(new HIPAddress { IP = strIp, Port = nPort })
        {
        }

        /// <summary>以 HIPAddress 构造。</summary>
        /// <param name="hIP">监听地址（IP/Port）</param>
        public SocketTcpServer(HIPAddress hIP)
        {
            hIPAddress = hIP ?? new HIPAddress();
            hIPAddress.SocketType = HSocketType.TCP_Server;
        }

        /// <summary>以 HIPAddress 与发送超时构造。</summary>
        /// <param name="hIP">监听地址（IP/Port）</param>
        /// <param name="timeout">发送超时（毫秒）</param>
        public SocketTcpServer(HIPAddress hIP, int timeout) : this(hIP)
        {
            Timeout = timeout;
        }

        /// <summary>
        /// 开始监听并接受客户端接入；重复调用幂等。
        /// </summary>
        /// <returns>是否启动成功（失败原因见 StrError / ServerError 事件）</returns>
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
                if (DataLength > 0)
                {
                    mListener.Server.SendBufferSize = DataLength;
                    mListener.Server.ReceiveBufferSize = DataLength;
                }
                mListener.Start();
                mRunning = true;
                mAcceptThread = new Thread(AcceptLoop)
                {
                    IsBackground = true,
                    Name = "HTcpServer-Accept"
                };
                mAcceptThread.Start();
                return true;
            }
            catch (SocketException ex)
            {
                // 10048：端口已被占用/重复启动；交由调用方改端口或先停旧实例，不在此杀进程
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

        /// <summary>停止监听并关闭全部客户端连接（幂等）。</summary>
        public void Stop()
        {
            Close();
        }

        /// <summary>停止监听并关闭全部客户端连接（同 Stop，命名与 SocketTcpClient 对齐）。</summary>
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
                // 停止监听时 Accept 抛出属正常流程
            }
            List<TcpServerClient> snapshot;
            lock (mClientLock)
            {
                snapshot = new List<TcpServerClient>(mClients.Values);
                mClients.Clear();
            }
            foreach (TcpServerClient client in snapshot)
            {
                client.Close();
            }
        }

        /// <summary>获取当前在线客户端快照。</summary>
        /// <returns>客户端会话列表（副本，遍历安全）</returns>
        public List<TcpServerClient> GetClients()
        {
            lock (mClientLock)
            {
                return new List<TcpServerClient>(mClients.Values);
            }
        }

        /// <summary>按连接 Id（远端“IP:端口”）查找客户端。</summary>
        /// <param name="clientId">连接 Id</param>
        /// <returns>客户端会话；不在线返回 null</returns>
        public TcpServerClient GetClient(string clientId)
        {
            lock (mClientLock)
            {
                TcpServerClient client;
                return mClients.TryGetValue(clientId ?? string.Empty, out client) ? client : null;
            }
        }

        /// <summary>
        /// 向指定客户端发送字节数据。
        /// </summary>
        /// <param name="client">客户端会话</param>
        /// <param name="bytes">数据内容</param>
        /// <returns>实际写入字节数；未连接返回 -101，异常返回 -200</returns>
        public int Write(TcpServerClient client, byte[] bytes)
        {
            if (client == null)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            return client.Write(bytes);
        }

        /// <summary>
        /// 向指定客户端发送文本（使用服务端 Encoding 编码）。
        /// </summary>
        /// <param name="client">客户端会话</param>
        /// <param name="strCommand">文本内容</param>
        /// <returns>实际写入字节数；未连接返回 -101，异常返回 -200</returns>
        public int Write(TcpServerClient client, string strCommand)
        {
            if (client == null)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -102;
            }
            int result = client.Write(Encoding.GetBytes(strCommand));
            if (result < 0)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
            }
            return result;
        }

        /// <summary>按连接 Id 发送字节数据。</summary>
        /// <param name="clientId">连接 Id（远端“IP:端口”）</param>
        /// <param name="bytes">数据内容</param>
        /// <returns>实际写入字节数；客户端不存在返回 -101</returns>
        public int Write(string clientId, byte[] bytes)
        {
            return Write(GetClient(clientId), bytes);
        }

        /// <summary>按连接 Id 发送文本。</summary>
        /// <param name="clientId">连接 Id（远端“IP:端口”）</param>
        /// <param name="strCommand">文本内容</param>
        /// <returns>实际写入字节数；客户端不存在返回 -101</returns>
        public int Write(string clientId, string strCommand)
        {
            return Write(GetClient(clientId), strCommand);
        }

        /// <summary>
        /// 向全部在线客户端广播字节数据。
        /// </summary>
        /// <param name="bytes">数据内容</param>
        /// <returns>发送成功的客户端数量</returns>
        public int Broadcast(byte[] bytes)
        {
            int ok = 0;
            foreach (TcpServerClient client in GetClients())
            {
                if (client.Write(bytes) > 0)
                {
                    ok++;
                }
            }
            return ok;
        }

        /// <summary>
        /// 向全部在线客户端广播文本。
        /// </summary>
        /// <param name="strCommand">文本内容</param>
        /// <returns>发送成功的客户端数量</returns>
        public int Broadcast(string strCommand)
        {
            return Broadcast(Encoding.GetBytes(strCommand));
        }

        /// <summary>主动断开指定客户端。</summary>
        /// <param name="client">客户端会话</param>
        public void DisconnectClient(TcpServerClient client)
        {
            if (client != null)
            {
                client.Close();
                RemoveClient(client);
            }
        }

        /// <summary>
        /// 清空指定客户端的接收缓存（丢弃流中残留字节，返回丢弃数量）。
        /// 警告：接收线程会持续读取该连接，请勿在连接存活（尤其交互）期间调用，
        /// 并发读同一 NetworkStream 可能导致接收线程异常；建议在确认无交互的维护场景使用。
        /// </summary>
        /// <param name="client">客户端会话</param>
        /// <returns>丢弃字节数；客户端不存在返回 0</returns>
        public int ClearCache(TcpServerClient client)
        {
            return client == null ? 0 : client.ClearCache();
        }

        /// <summary>按连接 Id 清空该客户端接收缓存。</summary>
        /// <param name="clientId">连接 Id（远端“IP:端口”）</param>
        /// <returns>丢弃字节数；客户端不存在返回 0</returns>
        public int ClearCache(string clientId)
        {
            return ClearCache(GetClient(clientId));
        }

        /// <summary>清空全部在线客户端接收缓存。</summary>
        /// <returns>累计丢弃字节数</returns>
        public int ClearCacheAll()
        {
            int total = 0;
            foreach (TcpServerClient client in GetClients())
            {
                total += client.ClearCache();
            }
            return total;
        }

        /// <summary>接受连接循环：每个连接登记后启动独立接收线程。</summary>
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
                    // Stop 触发的 SocketException：运行标志已清则正常退出
                    if (!mRunning)
                    {
                        break;
                    }
                    continue;
                }
                try
                {
                    if (Timeout > 0)
                    {
                        tcp.SendTimeout = Timeout;
                    }
                    if (DataLength > 0)
                    {
                        tcp.SendBufferSize = DataLength;
                        tcp.ReceiveBufferSize = DataLength;
                    }
                    var client = new TcpServerClient(tcp)
                    {
                        Encoding = Encoding
                    };
                    AddClient(client);
                    ClientConnected?.Invoke(this, client);
                    var recvThread = new Thread(() => ReceiveLoop(client))
                    {
                        IsBackground = true,
                        Name = "HTcpServer-Receive"
                    };
                    recvThread.Start();
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
                        // 接入失败时关闭资源即可
                    }
                }
            }
        }

        /// <summary>单客户端接收循环：读到 0 字节或异常即视为断开。</summary>
        private void ReceiveLoop(TcpServerClient client)
        {
            int bufferSize = DataLength > 0 ? DataLength : 8192;
            var buffer = new byte[bufferSize];
            while (mRunning && client.IsConnected)
            {
                int bytesReceived;
                try
                {
                    bytesReceived = client.Stream.Read(buffer, 0, buffer.Length);
                }
                catch
                {
                    break;
                }
                if (bytesReceived <= 0)
                {
                    break;
                }
                var data = new byte[bytesReceived];
                Buffer.BlockCopy(buffer, 0, data, 0, bytesReceived);
                DataReceived?.Invoke(this, client, data);
            }
            RemoveClient(client);
            client.Close();
            ClientDisconnected?.Invoke(this, client);
        }

        /// <summary>登记新客户端；同一远端标识重复接入时保留最新连接、关闭旧连接。</summary>
        private void AddClient(TcpServerClient client)
        {
            TcpServerClient old;
            lock (mClientLock)
            {
                mClients.TryGetValue(client.Id, out old);
                mClients[client.Id] = client;
            }
            old?.Close();
        }

        /// <summary>移除客户端；仅当字典里仍是该会话时移除，避免顶号场景误删新连接。</summary>
        private void RemoveClient(TcpServerClient client)
        {
            lock (mClientLock)
            {
                TcpServerClient current;
                if (mClients.TryGetValue(client.Id, out current) && ReferenceEquals(current, client))
                {
                    mClients.Remove(client.Id);
                }
            }
        }
    }
}
