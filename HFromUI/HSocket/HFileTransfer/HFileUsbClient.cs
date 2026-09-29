using System;
using System.IO;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HFileTransfer
{
    using HFromUI.HEnum;
    using HFromUI.HSocket.DeviceID;
    using HFromUI.HSocket.HSerialCommon;

    /// <summary>
    /// USB CDC（虚拟串口）大文件传输客户端 / 主动方：以 HUsbAddress 作为连接地址，
    /// 可直接指定 COM 口或按 VID/PID 自动发现设备，底层在 CDC 串口流上跑统一分帧协议，
    /// 支持发送、接收与断点续传。默认分块 1KB（低速 CDC 逐块应答），同一时刻只允许一个传输任务。
    /// <para>仅适用于 USB CDC/USB 转串口设备。</para>
    /// </summary>
    public class HFileUsbClient : HFileTransferBase, IDisposable
    {
        private SerialPortSession mSession;
        private HFileTransferContext mContext;

        /// <summary>USB 设备连接地址（端口/VID/PID/串口参数）。</summary>
        public HUsbAddress hUsbAddress { set; get; }

        /// <summary>是否已连接（CDC 串口是否打开）。</summary>
        public bool IsConnected
        {
            get { return mSession != null && mSession.IsOpen; }
        }

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>以 HUsbAddress 构造。</summary>
        /// <param name="address">USB CDC 设备地址</param>
        public HFileUsbClient(HUsbAddress address)
        {
            hUsbAddress = address ?? new HUsbAddress();
            hUsbAddress.SocketType = HSocketType.Usb_Client;
            ChunkSize = SerialChunkSize;
        }

        /// <summary>以虚拟串口号与波特率构造。</summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        /// <param name="baudRate">波特率</param>
        public HFileUsbClient(string portName, int baudRate = 115200)
            : this(new HUsbAddress { PortName = portName, BaudRate = baudRate })
        {
        }

        /// <summary>
        /// 打开 USB CDC 设备；PortName 为空时按 Vid/Pid 自动发现 COM 口。重复调用幂等。
        /// </summary>
        /// <returns>是否连接成功（失败原因见 StrError）</returns>
        public bool Connect()
        {
            if (mSession != null && mSession.IsOpen)
            {
                return true;
            }
            hUsbAddress.SocketType = HSocketType.Usb_Client;
            HSerialPortAddress serial = hUsbAddress.ToSerialPortAddress();
            if (serial == null)
            {
                StrError = "No USB CDC serial port found (PortName/VID/PID unmatched)";
                return false;
            }
            mSession = new SerialPortSession(serial)
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

        /// <summary>关闭设备连接（幂等）。</summary>
        public void Close()
        {
            Cancel();
            if (mSession != null)
            {
                mSession.Close();
                mSession = null;
            }
        }

        /// <summary>清空 CDC 接收缓存（返回丢弃字节数；未连接返回 0）。</summary>
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
                hUsbAddress, mContext);
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
                hUsbAddress, mContext);
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

        /// <summary>释放设备资源。</summary>
        public void Dispose()
        {
            Close();
        }
    }
}
