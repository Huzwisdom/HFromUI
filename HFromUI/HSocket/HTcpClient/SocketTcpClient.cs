using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HFromUI.HSocket.HTcpClient
{
    using HFromUI.HEnum;
    using HFromUI.HLangage;
    using HFromUI.HSocket.DeviceID;

    public  class SocketTcpClient
    {
        private readonly object mSendLock = new object();
        private readonly object mRecvLock = new object();
        private TcpClient mClient;
        private NetworkStream mStream;
        private bool isConnected = false;

        /// <summary>
        /// 报错提示
        /// </summary>
        public string StrError { get; private set; }
        /// <summary>
        ///是否连接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (mClient == null)
                {
                    return false;
                }
                return isConnected;
            }
        }

        public HIPAddress hIPAddress { set; get; }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout = -1;
        public int DataLength = -1;
        /// <summary>
        /// 编码格式
        /// </summary>
        public Encoding Encoding = Encoding.UTF8;

        public SocketTcpClient(string strIp, int nPort)
        {
            if (hIPAddress==null)
            {
                hIPAddress = new HIPAddress();
                hIPAddress.SocketType= HSocketType.TCP_Client;
            }
            hIPAddress.Port = nPort > 0 ? nPort : hIPAddress.Port;
            hIPAddress.IP = !string.IsNullOrEmpty(strIp) ? strIp : hIPAddress.IP;
        }
        public SocketTcpClient(string strIp, int nPort,int timeout)
        {
            if (hIPAddress == null)
            {
                hIPAddress = new HIPAddress();
                hIPAddress.SocketType = HSocketType.TCP_Client;
            }
            hIPAddress.Port = nPort >  0 ? nPort : hIPAddress.Port;
            hIPAddress.IP = !string.IsNullOrEmpty(strIp) ? strIp : hIPAddress.IP;
            Timeout = timeout;
        }

        public SocketTcpClient(HIPAddress hIP)
        {
            hIPAddress=hIP;
            hIPAddress.SocketType = HSocketType.TCP_Client;
        }
        public SocketTcpClient(HIPAddress hIP, int timeout)
        {
            hIPAddress = hIP; Timeout = timeout;
            hIPAddress.SocketType = HSocketType.TCP_Client;
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                mClient = new TcpClient();
                if (Timeout > 0)
                {
                    mClient.SendTimeout = Timeout;
                    mClient.ReceiveTimeout = Timeout;
                }
                if (DataLength > 0)
                {
                    mClient.ReceiveBufferSize = DataLength;
                    mClient.SendBufferSize = DataLength;
                }
                mClient.Connect(hIPAddress.IP, hIPAddress.Port);
                mStream = mClient.GetStream();
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                mClient = null;
                mStream = null;
                StrError = "IP Error:" + ex.Message;
            }
            isConnected = false;
            return false;
        }
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            Thread.Sleep(100);
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

        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="strCommand">指令内容</param>
        /// <returns></returns>
        public int Write(string strCommand)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -102;
            }
            try
            {
                lock (mSendLock)
                {
                    byte[] bytes = Encoding.GetBytes(strCommand);
                    mStream.Write(bytes, 0, bytes.Length);
                    mStream.Flush();
                    return bytes.Length;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException&&(ex.InnerException as SocketException).ErrorCode != 10060)
                {
                    return -9;
                }
                isConnected = false;
                StrError = ex.Message;
            }
            return -200;
        }

        /// <summary>
        /// 发送HEX指令
        /// </summary>
        /// <param name="bytes">HEX指令</param>
        /// <returns></returns>
        public int Write(byte[] bytes)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            lock (mSendLock)
            {
                try
                {
                    mStream.Write(bytes, 0, bytes.Length);
                    mStream.Flush();
                    return bytes.Length;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is SocketException && (ex.InnerException as SocketException).ErrorCode != 10060)
                    {
                        return -9;
                    }
                    isConnected = false;
                    StrError = ex.Message;
                }
                return -200;
            }
        }
        public int Read(ref string strData)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            lock (mRecvLock)
            {
                try
                {
                    byte[] receive = new byte[mClient.ReceiveBufferSize];
                    int bytesReceived = mStream.Read(receive, 0, receive.Length);
                    mStream.Flush();
                    if (bytesReceived <= 0)
                    {
                        isConnected = false;
                        StrError = HTranslation.GetContent(@"连接中断!(Connection break!)");
                        return -102;
                    }
                    strData = Encoding.GetString(receive);
                    strData = strData.TrimEnd('\0');
                    return bytesReceived;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is SocketException && (ex.InnerException as SocketException).ErrorCode == 10060)
                    {
                        return -9;
                    }
                    isConnected = false;
                    StrError = ex.Message;
                }
                return -200;
            }
        }

        public int Read(ref byte[] bytes)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            try
            {
                lock (mRecvLock)
                {
                    int bytesReceived = mStream.Read(bytes, 0, bytes.Length);
                    mStream.Flush();
                    if (bytesReceived <= 0)
                    {
                        if (mClient.ReceiveTimeout <= 0)
                        {
                            isConnected = false;
                            StrError = HTranslation.GetContent(@"连接中断!(Connection break!)");
                        }
                        return -102;
                    }
                    return bytesReceived;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException && (ex.InnerException as SocketException).ErrorCode == 10060)
                {
                    return -9;
                }
                isConnected = false;
                StrError = ex.Message;
            }
            return -200;
        }

        public int ReadStreamDataClear()
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            try
            {
                lock (mRecvLock)
                {
                    if (mStream.DataAvailable)
                    {
                        byte[] receive = new byte[mClient.ReceiveBufferSize];
                        int bytesReceived = mStream.Read(receive, 0, receive.Length);
                        mStream.Flush();
                        if (bytesReceived <= 0)
                        {
                            if (mClient.ReceiveTimeout <= 0)
                            {
                                isConnected = false;
                                StrError = HTranslation.GetContent(@"连接中断!(Connection break!)");
                            }
                            return -102;
                        }
                        return bytesReceived;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException && (ex.InnerException as SocketException).ErrorCode == 10060)
                {
                    return -9;
                }
                isConnected = false;
                StrError = ex.Message;
            }
            return -200;
        }

    }
}
