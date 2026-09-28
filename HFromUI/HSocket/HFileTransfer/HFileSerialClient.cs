using System;
using System.IO;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HFileTransfer
{
    using HFromUI.HEnum;
    using HFromUI.HSocket.DeviceID;
    using HFromUI.HSocket.HSerialCommon;

    /// <summary>
    /// 串口大文件传输客户端 / 主动方：以 HSerialPortAddress 作为连接地址打开串口，
    /// 在串口底层流上跑与网口相同的分帧协议，支持发送、接收与断点续传。
    /// 默认分块 1KB（低速串口逐块应答），可按波特率调大。同一时刻只允许一个传输任务。
    /// </summary>
    public class HFileSerialClient : HFileTransferBase, IDisposable
    {
        private SerialPortSession mSession;
        private HFileTransferContext mContext;

        /// <summary>串口连接地址（端口名/波特率/数据位/校验/流控/超时）。</summary>
        public HSerialPortAddress hSerialPortAddress { set; get; }

        /// <summary>是否已连接（串口是否打开）。</summary>
        public bool IsConnected
        {
            get { return mSession != null && mSession.IsOpen; }
        }

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>以串口号与波特率构造（8 数据位/1 停止位/无校验）。</summary>
        /// <param name="portName">串口号，如 COM3</param>
        /// <param name="baudRate">波特率，如 9600、115200</param>
        public HFileSerialClient(string portName, int baudRate)
            : this(new HSerialPortAddress { PortName = portName, BaudRate = baudRate })
        {
        }

        /// <summary>以 HSerialPortAddress 连接地址构造。</summary>
        /// <param name="address">串口通讯参数</param>
        public HFileSerialClient(HSerialPortAddress address)
        {
            hSerialPortAddress = address ?? new HSerialPortAddress();
            hSerialPortAddress.SocketType = HSocketType.SerialPort_Client;
            ChunkSize = SerialChunkSize;
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
                UseDataEvent = false
            };
            if (!mSession.Open())
            {
                StrError = mSession.StrError;
                mSession = null;
                return false;
            }
            return true;
        }

        /// <summary>关闭串口连接（幂等）。</summary>
        public void Close()
        {
            Cancel();
            if (mSession != null)
            {
                mSession.Close();
                mSession = null;
            }
        }

        /// <summary>清空串口接收缓存（返回丢弃字节数）。</summary>
        /// <returns>丢弃字节数；未连接返回 0</returns>
        public int ClearCache()
        {
            Stream stream = mSession == null ? null : mSession.BaseStream;
            return stream == null ? 0 : ClearStreamCache(stream);
        }

        /// <summary>取消当前传输（会向对端发送取消帧，分片保留以便续传）。</summary>
        public void Cancel()
        {
            HFileTransferContext context = mContext;
            if (context != null)
            {
                context.Cancel = true;
            }
        }

        /// <summary>
        /// 发送文件到对端（自动断点续传）。
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
            bool ok = SendFileCore(mSession.BaseStream, filePath, remoteName,
                hSerialPortAddress, mContext);
            mContext = null;
            return ok;
        }

        /// <summary>
        /// 等待接收对端文件。
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
            string path = ReceiveFileCore(mSession.BaseStream, saveDir,
                hSerialPortAddress, mContext);
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

        /// <summary>释放串口资源。</summary>
        public void Dispose()
        {
            Close();
        }
    }
}
