using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HFromUI.HSocket.HTcpServer
{
    /// <summary>
    /// TCP 服务端上的单个客户端会话：每个接入连接对应一个实例，
    /// 持有该连接的网络流与发送锁，业务侧可通过 Id/IP/Port 区分客户端。
    /// </summary>
    public class TcpServerClient
    {
        private TcpClient mClient;
        private NetworkStream mStream;
        private readonly object mSendLock = new object();

        /// <summary>由 SocketTcpServer 在 Accept 后构造。</summary>
        internal TcpServerClient(TcpClient client)
        {
            mClient = client;
            mStream = client.GetStream();
            var ep = client.Client.RemoteEndPoint as IPEndPoint;
            Id = ep == null ? string.Empty : ep.ToString();
            IP = ep == null ? string.Empty : ep.Address.ToString();
            Port = ep == null ? 0 : ep.Port;
        }

        /// <summary>连接唯一标识（远端“IP:端口”）。</summary>
        public string Id { get; }

        /// <summary>远端 IP。</summary>
        public string IP { get; }

        /// <summary>远端端口。</summary>
        public int Port { get; }

        /// <summary>用户自定义挂载对象（可放站点信息/协议状态机）。</summary>
        public object Tag { set; get; }

        /// <summary>本会话文本发送编码（由 SocketTcpServer 接入时统一赋值）。</summary>
        public Encoding Encoding { internal set; get; } = Encoding.UTF8;

        /// <summary>是否仍在连接。</summary>
        public bool IsConnected
        {
            get { return mClient != null && mClient.Connected; }
        }

        /// <summary>接收线程使用的网络流（仅服务端内部访问）。</summary>
        internal NetworkStream Stream
        {
            get { return mStream; }
        }

        /// <summary>
        /// 发送字节数据（线程安全）。
        /// </summary>
        /// <param name="bytes">数据内容</param>
        /// <returns>实际写入字节数；未连接返回 -101，异常返回 -200</returns>
        internal int Write(byte[] bytes)
        {
            lock (mSendLock)
            {
                if (!IsConnected)
                {
                    return -101;
                }
                try
                {
                    mStream.Write(bytes, 0, bytes.Length);
                    mStream.Flush();
                    return bytes.Length;
                }
                catch
                {
                    return -200;
                }
            }
        }

        /// <summary>
        /// 发送文本（使用本会话 Encoding 编码）。
        /// </summary>
        /// <param name="strData">文本内容</param>
        /// <returns>实际写入字节数；未连接返回 -101，异常返回 -200</returns>
        public int Write(string strData)
        {
            return Write(Encoding.GetBytes(strData));
        }

        /// <summary>
        /// 清空本连接接收缓存（丢弃流中全部残留字节，尽力操作不抛异常）。
        /// </summary>
        /// <returns>累计丢弃字节数</returns>
        internal int ClearCache()
        {
            int total = 0;
            lock (mSendLock)
            {
                if (mStream == null)
                {
                    return 0;
                }
                byte[] buffer = new byte[8192];
                int oldTimeout = mClient.ReceiveTimeout;
                try
                {
                    mClient.ReceiveTimeout = 20;
                    while (mStream.DataAvailable)
                    {
                        int read = mStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                        {
                            break;
                        }
                        total += read;
                    }
                }
                catch
                {
                    // 超时即代表缓冲已空
                }
                finally
                {
                    mClient.ReceiveTimeout = oldTimeout;
                }
            }
            return total;
        }

        /// <summary>关闭本连接（幂等，可重复调用）。</summary>
        public void Close()
        {
            lock (mSendLock)
            {
                if (mStream != null)
                {
                    mStream.Close();
                    mStream.Dispose();
                    mStream = null;
                }
                if (mClient != null)
                {
                    mClient.Close();
                    mClient = null;
                }
            }
        }
    }
}
