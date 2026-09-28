using System;
using System.Text;

namespace HFromUI.HSocket.HSerialClient
{
    using HFromUI.HEnum;
    using HFromUI.HLangage;
    using HFromUI.HSocket.DeviceID;
    using HFromUI.HSocket.HSerialCommon;

    /// <summary>
    /// 串口客户端 / 主动方：以 HSerialPortAddress 作为连接地址打开串口，
    /// 接口与 SocketTcpClient 对齐（Connect / Close / Write / Read / ReadStreamDataClear /
    /// StrError / IsConnected / Encoding），另提供数据到达事件用于异步轮询场景。
    /// </summary>
    public class SocketSerialClient
    {
        private SerialPortSession mSession;

        /// <summary>串口连接地址（端口名/波特率/数据位/校验/流控/超时）。</summary>
        public HSerialPortAddress hSerialPortAddress { set; get; }

        /// <summary>文本编码格式。</summary>
        public Encoding Encoding = Encoding.UTF8;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否已连接（串口是否打开）。</summary>
        public bool IsConnected
        {
            get { return mSession != null && mSession.IsOpen; }
        }

        /// <summary>收到串口数据（后台线程触发，更新 UI 需 Invoke）。</summary>
        public event Action<SocketSerialClient, byte[]> DataReceived;

        /// <summary>串口错误（帧错/奇偶校验错等）。</summary>
        public event Action<SocketSerialClient, Exception> ErrorOccurred;

        /// <summary>以串口号与波特率构造（8 数据位/1 停止位/无校验）。</summary>
        /// <param name="portName">串口号，如 COM3</param>
        /// <param name="baudRate">波特率，如 9600、115200</param>
        public SocketSerialClient(string portName, int baudRate)
            : this(new HSerialPortAddress { PortName = portName, BaudRate = baudRate })
        {
        }

        /// <summary>以 HSerialPortAddress 连接地址构造。</summary>
        /// <param name="address">串口通讯参数</param>
        public SocketSerialClient(HSerialPortAddress address)
        {
            hSerialPortAddress = address ?? new HSerialPortAddress();
            hSerialPortAddress.SocketType = HSocketType.SerialPort_Client;
        }

        /// <summary>
        /// 打开串口连接；重复调用幂等。
        /// </summary>
        /// <returns>是否连接成功（失败原因见 StrError）</returns>
        public bool Connect()
        {
            if (mSession != null && mSession.IsOpen)
            {
                return true;
            }
            hSerialPortAddress.SocketType = HSocketType.SerialPort_Client;
            mSession = new SerialPortSession(hSerialPortAddress)
            {
                Encoding = Encoding
            };
            mSession.DataReceived += OnSessionData;
            mSession.ErrorOccurred += OnSessionError;
            if (!mSession.Open())
            {
                StrError = mSession.StrError;
                return false;
            }
            return true;
        }

        /// <summary>关闭串口连接（幂等）。</summary>
        public void Close()
        {
            if (mSession != null)
            {
                mSession.DataReceived -= OnSessionData;
                mSession.ErrorOccurred -= OnSessionError;
                mSession.Close();
                mSession = null;
            }
        }

        /// <summary>
        /// 发送文本指令。
        /// </summary>
        /// <param name="strCommand">指令内容</param>
        /// <returns>实际写入字节数；未连接返回 -102，异常返回 -200</returns>
        public int Write(string strCommand)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -102;
            }
            int result = mSession.Write(strCommand);
            if (result < 0)
            {
                StrError = mSession.StrError;
            }
            return result;
        }

        /// <summary>
        /// 发送 HEX / 字节指令。
        /// </summary>
        /// <param name="bytes">指令字节</param>
        /// <returns>实际写入字节数；未连接返回 -101，异常返回 -200</returns>
        public int Write(byte[] bytes)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = mSession.Write(bytes);
            if (result < 0)
            {
                StrError = mSession.StrError;
            }
            return result;
        }

        /// <summary>
        /// 立即读取接收缓冲中的全部文本（不阻塞，轮询用法）。
        /// </summary>
        /// <param name="strData">读到的文本</param>
        /// <returns>读取字节数；未连接返回 -101，异常返回 -200</returns>
        public int Read(ref string strData)
        {
            if (!IsConnected)
            {
                strData = string.Empty;
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = mSession.ReadExisting(out strData);
            if (result < 0)
            {
                StrError = mSession.StrError;
            }
            return result;
        }

        /// <summary>
        /// 阻塞读取字节到调用方缓冲区（遵守地址中的 ReadTimeout）。
        /// </summary>
        /// <param name="bytes">调用方缓冲区，返回实际读到的前 N 个字节</param>
        /// <returns>实际读取字节数；未连接返回 -101，超时返回 -9，异常返回 -200</returns>
        public int Read(ref byte[] bytes)
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = mSession.Read(ref bytes);
            if (result < 0 && result != -9)
            {
                StrError = mSession.StrError;
            }
            return result;
        }

        /// <summary>
        /// 清空接收缓存并丢弃现有数据（ReadStreamDataClear 的统一命名版本）。
        /// </summary>
        /// <returns>丢弃字节数；未连接返回 -101，异常返回 -200</returns>
        public int ClearCache()
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = mSession.ClearCache();
            if (result < 0)
            {
                StrError = mSession.StrError;
            }
            return result;
        }

        /// <summary>
        /// 清空并丢弃接收缓冲中现有数据（对齐 SocketTcpClient.ReadStreamDataClear）。
        /// </summary>
        /// <returns>丢弃字节数；未连接返回 -101，异常返回 -200</returns>
        public int ReadStreamDataClear()
        {
            if (!IsConnected)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = mSession.ClearInBuffer();
            if (result < 0)
            {
                StrError = mSession.StrError;
            }
            return result;
        }

        /// <summary>会话数据透传为客户端事件。</summary>
        private void OnSessionData(SerialPortSession session, byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }

        /// <summary>会话错误透传为客户端事件。</summary>
        private void OnSessionError(SerialPortSession session, Exception ex)
        {
            StrError = session.StrError;
            ErrorOccurred?.Invoke(this, ex);
        }
    }
}
