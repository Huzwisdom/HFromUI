using System;
using System.Text;
using HFromUI.HEnum;
using HFromUI.HSocket.DeviceID;
using HFromUI.HSocket.HSerialClient;

namespace HFromUI.HSocket.HUsb
{
    /// <summary>
    /// USB CDC（虚拟串口）客户端 / 主动方：以 HUsbAddress 作为连接地址，
    /// 可直接指定串口号或按 VID/PID 自动发现，底层通讯完全复用 SocketSerialClient。
    /// 接口与 SocketTcpClient/SocketSerialClient 对齐（Connect/Close/Write/Read/ClearCache/事件）。
    /// <para>注意：仅适用于 USB CDC/USB 转串口设备；HID/WinUSB 设备需厂商专用 SDK。</para>
    /// </summary>
    public class HUsbClient
    {
        private SocketSerialClient mClient;

        /// <summary>USB 设备连接地址（端口/VID/PID/串口参数）。</summary>
        public HUsbAddress hUsbAddress { set; get; }

        /// <summary>文本编码格式（对连接生效）。</summary>
        public Encoding Encoding { set; get; } = Encoding.UTF8;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否已连接（虚拟串口是否打开）。</summary>
        public bool IsConnected
        {
            get { return mClient != null && mClient.IsConnected; }
        }

        /// <summary>收到设备数据（后台线程触发，更新 UI 需 Invoke）。</summary>
        public event Action<HUsbClient, byte[]> DataReceived;

        /// <summary>设备通讯错误（帧错/校验错等）。</summary>
        public event Action<HUsbClient, Exception> ErrorOccurred;

        /// <summary>以 USB 设备地址构造。</summary>
        /// <param name="address">USB CDC 地址（端口或 VID/PID）</param>
        public HUsbClient(HUsbAddress address)
        {
            hUsbAddress = address ?? new HUsbAddress();
            hUsbAddress.SocketType = HSocketType.Usb_Client;
        }

        /// <summary>以虚拟串口号与波特率构造。</summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        /// <param name="baudRate">波特率</param>
        public HUsbClient(string portName, int baudRate = 115200)
            : this(new HUsbAddress { PortName = portName, BaudRate = baudRate })
        {
        }

        /// <summary>
        /// 打开 USB CDC 设备；PortName 为空时按 Vid/Pid 自动发现 COM 口。重复调用幂等。
        /// </summary>
        /// <returns>是否连接成功（失败原因见 StrError）</returns>
        public bool Connect()
        {
            if (mClient != null && mClient.IsConnected)
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
            mClient = new SocketSerialClient(serial) { Encoding = Encoding };
            mClient.DataReceived += OnInnerData;
            mClient.ErrorOccurred += OnInnerError;
            if (!mClient.Connect())
            {
                StrError = mClient.StrError;
                mClient = null;
                return false;
            }
            return true;
        }

        /// <summary>关闭设备连接（幂等）。</summary>
        public void Close()
        {
            if (mClient != null)
            {
                mClient.DataReceived -= OnInnerData;
                mClient.ErrorOccurred -= OnInnerError;
                mClient.Close();
                mClient = null;
            }
        }

        /// <summary>发送文本指令。</summary>
        /// <returns>实际写入字节数；未连接返回负值（见 StrError）</returns>
        public int Write(string strCommand)
        {
            if (!EnsureClient())
            {
                return -102;
            }
            int result = mClient.Write(strCommand);
            SyncError(result);
            return result;
        }

        /// <summary>发送字节指令。</summary>
        /// <returns>实际写入字节数；未连接返回负值（见 StrError）</returns>
        public int Write(byte[] bytes)
        {
            if (!EnsureClient())
            {
                return -101;
            }
            int result = mClient.Write(bytes);
            SyncError(result);
            return result;
        }

        /// <summary>非阻塞读取接收缓冲中的全部文本。</summary>
        public int Read(ref string strData)
        {
            strData = string.Empty;
            if (!EnsureClient())
            {
                return -101;
            }
            int result = mClient.Read(ref strData);
            SyncError(result);
            return result;
        }

        /// <summary>阻塞读取字节（遵守地址中的 ReadTimeout）。</summary>
        public int Read(ref byte[] bytes)
        {
            bytes = new byte[0];
            if (!EnsureClient())
            {
                return -101;
            }
            int result = mClient.Read(ref bytes);
            SyncError(result);
            return result;
        }

        /// <summary>
        /// 清空接收缓存并丢弃现有数据，返回丢弃字节数；未连接返回 -101。
        /// </summary>
        public int ClearCache()
        {
            if (!EnsureClient())
            {
                return -101;
            }
            int result = mClient.ClearCache();
            SyncError(result);
            return result;
        }

        /// <summary>转发内部串口数据事件。</summary>
        private void OnInnerData(SocketSerialClient sender, byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }

        /// <summary>转发内部串口错误事件。</summary>
        private void OnInnerError(SocketSerialClient sender, Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }

        /// <summary>确保已连接（未连接写 StrError）。</summary>
        private bool EnsureClient()
        {
            if (mClient == null || !mClient.IsConnected)
            {
                StrError = "Not connected";
                return false;
            }
            return true;
        }

        /// <summary>同步内部错误文本。</summary>
        private void SyncError(int result)
        {
            if (result < 0 && mClient != null)
            {
                StrError = mClient.StrError;
            }
        }
    }
}
