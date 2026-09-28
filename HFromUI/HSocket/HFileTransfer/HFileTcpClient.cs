using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HFileTransfer
{
    using HFromUI.HEnum;
    using HFromUI.HSocket.DeviceID;

    /// <summary>
    /// 网口大文件传输客户端：以 HIPAddress 连接远端，支持发送、接收与断点续传。
    /// 协议见 HFilePacket；发送/接收为阻塞接口（建议在 Task 中调用），也提供 Async 版本。
    /// 同一时刻只允许一个传输任务。
    /// </summary>
    public class HFileTcpClient : HFileTransferBase, IDisposable
    {
        private TcpClient mClient;
        private NetworkStream mStream;
        private HFileTransferContext mContext;

        /// <summary>连接地址（IP / Port）。</summary>
        public HIPAddress hIPAddress { set; get; }

        /// <summary>连接/发送超时（毫秒），默认 10 秒。</summary>
        public int ConnectTimeout { set; get; } = 10000;

        /// <summary>是否已连接。</summary>
        public bool IsConnected
        {
            get { return mClient != null && mClient.Connected; }
        }

        /// <summary>以 IP 与端口构造。</summary>
        /// <param name="strIp">远端 IP</param>
        /// <param name="nPort">远端端口</param>
        public HFileTcpClient(string strIp, int nPort)
            : this(new HIPAddress { IP = strIp, Port = nPort })
        {
        }

        /// <summary>以 HIPAddress 构造。</summary>
        /// <param name="hIP">连接地址</param>
        public HFileTcpClient(HIPAddress hIP)
        {
            hIPAddress = hIP ?? new HIPAddress();
            hIPAddress.SocketType = HSocketType.TCP_Client;
            ChunkSize = NetworkChunkSize;
        }

        /// <summary>
        /// 连接远端；重复调用幂等。
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
                hIPAddress.SocketType = HSocketType.TCP_Client;
                mClient = new TcpClient();
                var connectTask = mClient.ConnectAsync(hIPAddress.IP, hIPAddress.Port);
                if (!connectTask.Wait(ConnectTimeout))
                {
                    mClient.Close();
                    mClient = null;
                    StrError = "Connect timeout";
                    return false;
                }
                mClient.NoDelay = true;
                mClient.SendBufferSize = ChunkSize * 2;
                mClient.ReceiveBufferSize = ChunkSize * 2;
                mStream = mClient.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                mClient = null;
                mStream = null;
                StrError = "Connect error:" + ex.Message;
                return false;
            }
        }

        /// <summary>关闭连接（幂等）。</summary>
        public void Close()
        {
            Cancel();
            NetworkStream stream = mStream;
            mStream = null;
            TcpClient client = mClient;
            mClient = null;
            try
            {
                stream?.Close();
            }
            catch
            {
                // 关闭时忽略流异常
            }
            try
            {
                client?.Close();
            }
            catch
            {
                // 关闭时忽略套接字异常
            }
        }

        /// <summary>清空网络流接收缓存（丢弃残留字节，返回丢弃数量）。</summary>
        /// <returns>丢弃字节数；未连接返回 0</returns>
        public int ClearCache()
        {
            return mStream == null ? 0 : ClearStreamCache(mStream);
        }

        /// <summary>取消当前传输（会向对端发送取消帧，分片文件保留以便续传）。</summary>
        public void Cancel()
        {
            HFileTransferContext context = mContext;
            if (context != null)
            {
                context.Cancel = true;
            }
        }

        /// <summary>
        /// 发送文件到远端（自动断点续传）。
        /// </summary>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="remoteName">对端保存文件名（null 使用本地文件名）</param>
        /// <returns>是否发送并被确认完成</returns>
        public bool SendFile(string filePath, string remoteName = null)
        {
            if (!CheckReady())
            {
                return false;
            }
            mContext = new HFileTransferContext();
            bool ok = SendFileCore(mStream, filePath, remoteName, hIPAddress, mContext);
            mContext = null;
            return ok;
        }

        /// <summary>
        /// 等待接收远端文件。
        /// </summary>
        /// <param name="saveDir">本地保存目录（已有同名分片时自动续收）</param>
        /// <returns>最终文件完整路径；失败/超时返回 null（原因见 StrError）</returns>
        public string ReceiveFile(string saveDir)
        {
            if (!CheckReady())
            {
                return null;
            }
            mContext = new HFileTransferContext();
            string path = ReceiveFileCore(mStream, saveDir, hIPAddress, mContext);
            mContext = null;
            return path;
        }

        /// <summary>异步发送文件。</summary>
        public Task<bool> SendFileAsync(string filePath, string remoteName = null)
        {
            return Task.Run(() => SendFile(filePath, remoteName));
        }

        /// <summary>异步接收文件。</summary>
        public Task<string> ReceiveFileAsync(string saveDir)
        {
            return Task.Run(() => ReceiveFile(saveDir));
        }

        /// <summary>传输前检查连接与忙状态。</summary>
        private bool CheckReady()
        {
            if (!IsConnected)
            {
                StrError = "Not connected";
                return false;
            }
            if (IsBusy)
            {
                StrError = "A transfer is already running";
                return false;
            }
            return true;
        }

        /// <summary>释放连接资源。</summary>
        public void Dispose()
        {
            Close();
        }
    }
}
