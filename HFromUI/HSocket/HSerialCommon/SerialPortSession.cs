using System;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace HFromUI.HSocket.HSerialCommon
{
    using HFromUI.HSocket.DeviceID;

    /// <summary>
    /// 单个串口的打开/收发/事件会话，供串口客户端与串口服务端共用，避免两套重复实现。
    /// 参数全部来自 HSerialPortAddress（端口名、波特率、数据位、停止位、校验、流控、超时）。
    /// DataReceived 事件在线程池线程触发，更新 UI 需自行 Invoke。
    /// </summary>
    internal sealed class SerialPortSession : IDisposable
    {
        private SerialPort mPort;
        private readonly object mSendLock = new object();

        /// <summary>以串口连接地址构造会话（此时尚未打开）。</summary>
        /// <param name="address">串口通讯参数</param>
        public SerialPortSession(HSerialPortAddress address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        /// <summary>串口通讯参数。</summary>
        public HSerialPortAddress Address { get; }

        /// <summary>会话标识（串口号，如 COM3）。</summary>
        public string Id
        {
            get { return Address.PortName; }
        }

        /// <summary>
        /// 是否启用 DataReceived 事件模式（默认 true，普通收发用）；
        /// 文件传输走 BaseStream 自定义分帧时必须设为 false，否则事件回调会抢先读走字节。
        /// 需在 Open 之前设置。
        /// </summary>
        public bool UseDataEvent { set; get; } = true;

        /// <summary>文本编码格式。</summary>
        public Encoding Encoding { set; get; } = Encoding.UTF8;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>串口是否已打开。</summary>
        public bool IsOpen
        {
            get { return mPort != null && mPort.IsOpen; }
        }

        /// <summary>
        /// 串口底层流（供文件传输等自定义分帧协议直接读写）；
        /// 仅在 UseDataEvent=false 打开时使用，避免与事件接收互相抢数据。
        /// </summary>
        internal Stream BaseStream
        {
            get { return mPort == null ? null : mPort.BaseStream; }
        }

        /// <summary>收到串口数据（data 为本帧字节副本）。</summary>
        public event Action<SerialPortSession, byte[]> DataReceived;

        /// <summary>串口底层错误（帧错/奇偶校验错等）。</summary>
        public event Action<SerialPortSession, Exception> ErrorOccurred;

        /// <summary>
        /// 按地址参数打开串口；重复调用幂等。
        /// </summary>
        /// <returns>是否打开成功（失败原因见 StrError / ErrorOccurred 事件）</returns>
        public bool Open()
        {
            if (IsOpen)
            {
                return true;
            }
            try
            {
                mPort = new SerialPort(
                    Address.PortName,
                    Address.BaudRate,
                    Address.Parity,
                    Address.DataBits,
                    Address.StopBits)
                {
                    Handshake = Address.Handshake
                };
                if (Address.ReadTimeout > 0)
                {
                    mPort.ReadTimeout = Address.ReadTimeout;
                }
                if (Address.WriteTimeout > 0)
                {
                    mPort.WriteTimeout = Address.WriteTimeout;
                }
                if (UseDataEvent)
                {
                    mPort.DataReceived += OnDataReceived;
                    mPort.ErrorReceived += OnErrorReceived;
                }
                mPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                StrError = "Serial Open Error:" + ex.Message;
                ReleasePort();
                ErrorOccurred?.Invoke(this, ex);
                return false;
            }
        }

        /// <summary>关闭串口（幂等）。</summary>
        public void Close()
        {
            ReleasePort();
        }

        /// <summary>
        /// 发送字节数据（线程安全）。
        /// </summary>
        /// <param name="bytes">数据内容</param>
        /// <returns>实际写入字节数；未打开返回 -101，异常返回 -200</returns>
        public int Write(byte[] bytes)
        {
            lock (mSendLock)
            {
                if (!IsOpen)
                {
                    StrError = "Serial Not Open";
                    return -101;
                }
                try
                {
                    mPort.Write(bytes, 0, bytes.Length);
                    return bytes.Length;
                }
                catch (Exception ex)
                {
                    StrError = ex.Message;
                    ErrorOccurred?.Invoke(this, ex);
                    return -200;
                }
            }
        }

        /// <summary>
        /// 发送文本（使用本会话 Encoding 编码）。
        /// </summary>
        /// <param name="strData">文本内容</param>
        /// <returns>实际写入字节数；未打开返回 -101，异常返回 -200</returns>
        public int Write(string strData)
        {
            return Write(Encoding.GetBytes(strData));
        }

        /// <summary>
        /// 阻塞读取字节到调用方缓冲区（遵守地址中的 ReadTimeout）。
        /// </summary>
        /// <param name="bytes">调用方缓冲区，返回实际读到的前 N 个字节</param>
        /// <returns>实际读取字节数；未打开返回 -101，超时返回 -9，异常返回 -200</returns>
        public int Read(ref byte[] bytes)
        {
            if (!IsOpen)
            {
                StrError = "Serial Not Open";
                return -101;
            }
            try
            {
                int bytesReceived = mPort.Read(bytes, 0, bytes.Length);
                return bytesReceived;
            }
            catch (TimeoutException)
            {
                return -9;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                ErrorOccurred?.Invoke(this, ex);
                return -200;
            }
        }

        /// <summary>
        /// 立即读取串口接收缓冲中的全部文本（不阻塞）。
        /// </summary>
        /// <param name="strData">读到的文本</param>
        /// <returns>读取字节数；未打开返回 -101，异常返回 -200</returns>
        public int ReadExisting(out string strData)
        {
            if (!IsOpen)
            {
                strData = string.Empty;
                StrError = "Serial Not Open";
                return -101;
            }
            try
            {
                strData = mPort.ReadExisting();
                return Encoding.GetByteCount(strData);
            }
            catch (Exception ex)
            {
                strData = string.Empty;
                StrError = ex.Message;
                ErrorOccurred?.Invoke(this, ex);
                return -200;
            }
        }

        /// <summary>
        /// 清空接收缓存并丢弃现有数据（ClearInBuffer 的统一命名版本）。
        /// </summary>
        /// <returns>丢弃的字节数；未打开返回 -101，异常返回 -200</returns>
        public int ClearCache()
        {
            return ClearInBuffer();
        }

        /// <summary>
        /// 清空接收缓冲并丢弃现有数据（对齐 TCP 客户端 ReadStreamDataClear 语义）。
        /// </summary>
        /// <returns>丢弃的字节数；未打开返回 -101，异常返回 -200</returns>
        public int ClearInBuffer()
        {
            if (!IsOpen)
            {
                StrError = "Serial Not Open";
                return -101;
            }
            try
            {
                int count = mPort.BytesToRead;
                if (count > 0)
                {
                    byte[] buffer = new byte[count];
                    int read = 0;
                    while (read < count)
                    {
                        int r = mPort.Read(buffer, read, count - read);
                        if (r <= 0)
                        {
                            break;
                        }
                        read += r;
                    }
                    return read;
                }
                return 0;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                ErrorOccurred?.Invoke(this, ex);
                return -200;
            }
        }

        /// <summary>串口数据到达：一次性读完当前缓冲字节并抛出事件。</summary>
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int count = mPort == null ? 0 : mPort.BytesToRead;
                if (count <= 0)
                {
                    return;
                }
                var buffer = new byte[count];
                int read = 0;
                while (read < count)
                {
                    int r = mPort.Read(buffer, read, count - read);
                    if (r <= 0)
                    {
                        break;
                    }
                    read += r;
                }
                if (read <= 0)
                {
                    return;
                }
                var data = new byte[read];
                Buffer.BlockCopy(buffer, 0, data, 0, read);
                DataReceived?.Invoke(this, data);
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                ErrorOccurred?.Invoke(this, ex);
            }
        }

        /// <summary>底层串口错误透传。</summary>
        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            StrError = "Serial Error:" + e.EventType;
            ErrorOccurred?.Invoke(this, new InvalidOperationException(StrError));
        }

        /// <summary>注销事件并释放串口资源。</summary>
        private void ReleasePort()
        {
            if (mPort != null)
            {
                mPort.DataReceived -= OnDataReceived;
                mPort.ErrorReceived -= OnErrorReceived;
                try
                {
                    if (mPort.IsOpen)
                    {
                        mPort.Close();
                    }
                }
                catch
                {
                    // 关闭失败无需再抛：资源即将释放
                }
                mPort.Dispose();
                mPort = null;
            }
        }

        /// <summary>释放资源。</summary>
        public void Dispose()
        {
            ReleasePort();
        }
    }
}
