using System;
using System.Collections.Generic;
using System.Text;
using HFromUI.HEnum;
using HFromUI.HSocket.DeviceID;
using HFromUI.HSocket.HSerialServer;

namespace HFromUI.HSocket.HUsb
{
    /// <summary>
    /// USB CDC（虚拟串口）服务端 / 被动方：管理一个或多个 USB CDC 设备站点，
    /// 底层完全复用 SocketSerialServer（地址经 HUsbAddress.ToSerialPortAddress 转换）。
    /// 事件在后台线程触发，更新 UI 需 Invoke。仅适用于 USB CDC/USB 转串口设备。
    /// </summary>
    public class HUsbServer
    {
        private readonly SocketSerialServer mServer = new SocketSerialServer();
        private readonly Dictionary<string, HUsbAddress> mUsbAddresses =
            new Dictionary<string, HUsbAddress>(StringComparer.OrdinalIgnoreCase);
        private readonly object mLock = new object();

        /// <summary>文本编码（透传内部串口服务端）。</summary>
        public Encoding Encoding
        {
            set { mServer.Encoding = value; }
            get { return mServer.Encoding; }
        }

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否至少有一个设备站点处于打开状态。</summary>
        public bool IsRunning
        {
            get { return mServer.IsRunning; }
        }

        /// <summary>当前已打开的设备站点数量。</summary>
        public int ClientCount
        {
            get { return mServer.ClientCount; }
        }

        /// <summary>设备站点打开（接入）。</summary>
        public event Action<HUsbServer, HUsbAddress> StationConnected;

        /// <summary>设备站点关闭（移除）。</summary>
        public event Action<HUsbServer, HUsbAddress> StationDisconnected;

        /// <summary>收到某站点数据。</summary>
        public event Action<HUsbServer, HUsbAddress, byte[]> DataReceived;

        /// <summary>站点异常（打开失败、帧错等）。</summary>
        public event Action<HUsbServer, HUsbAddress, string> StationError;

        /// <summary>构造空服务端，后续用 Add/Start 添加设备。</summary>
        public HUsbServer()
        {
            HookEvents();
        }

        /// <summary>以单个设备构造并登记（调用 Start 后打开）。</summary>
        public HUsbServer(HUsbAddress address) : this()
        {
            Add(address);
        }

        /// <summary>
        /// 登记一个 USB CDC 设备站点（不立即打开）；PortName 为空时按 VID/PID 发现 COM 口。
        /// 无法解析到串口号的设备会被忽略（原因见 StrError）。
        /// </summary>
        /// <param name="address">USB 设备地址</param>
        public void Add(HUsbAddress address)
        {
            HSerialPortAddress serial = Resolve(address);
            if (serial == null)
            {
                return;
            }
            lock (mLock)
            {
                mUsbAddresses[serial.PortName] = address;
            }
            mServer.Add(serial);
        }

        /// <summary>打开全部已登记设备站点。</summary>
        /// <returns>是否全部打开成功</returns>
        public bool Start()
        {
            return mServer.Start();
        }

        /// <summary>登记并立即打开单个设备站点。</summary>
        public bool Start(HUsbAddress address)
        {
            HSerialPortAddress serial = Resolve(address);
            if (serial == null)
            {
                return false;
            }
            lock (mLock)
            {
                mUsbAddresses[serial.PortName] = address;
            }
            return mServer.Start(serial);
        }

        /// <summary>关闭全部站点（保留登记信息，可再次 Start）。</summary>
        public void Stop()
        {
            mServer.Stop();
        }

        /// <summary>关闭全部站点。</summary>
        public void Close()
        {
            mServer.Close();
        }

        /// <summary>关闭并移除指定串口号设备站点。</summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        public void RemoveStation(string portName)
        {
            mServer.RemoveStation(portName);
            lock (mLock)
            {
                mUsbAddresses.Remove(portName ?? string.Empty);
            }
        }

        /// <summary>指定站点是否已打开。</summary>
        public bool IsStationOpen(string portName)
        {
            return mServer.IsStationOpen(portName);
        }

        /// <summary>向指定站点发送字节。</summary>
        public int Write(string portName, byte[] bytes)
        {
            int result = mServer.Write(portName, bytes);
            SyncError(result);
            return result;
        }

        /// <summary>向指定站点发送文本。</summary>
        public int Write(string portName, string strCommand)
        {
            int result = mServer.Write(portName, strCommand);
            SyncError(result);
            return result;
        }

        /// <summary>向全部已打开站点发送字节。</summary>
        public int WriteAll(byte[] bytes)
        {
            return mServer.WriteAll(bytes);
        }

        /// <summary>向全部已打开站点发送文本。</summary>
        public int WriteAll(string strCommand)
        {
            return mServer.WriteAll(strCommand);
        }

        /// <summary>清空指定站点接收缓存（返回丢弃字节数；站点不存在返回 -101）。</summary>
        public int ClearCache(string portName)
        {
            int result = mServer.ClearCache(portName);
            SyncError(result);
            return result;
        }

        /// <summary>清空全部站点接收缓存。</summary>
        public int ClearCacheAll()
        {
            return mServer.ClearCacheAll();
        }

        /// <summary>获取当前登记的全部 USB 设备地址副本。</summary>
        public List<HUsbAddress> GetStations()
        {
            lock (mLock)
            {
                return new List<HUsbAddress>(mUsbAddresses.Values);
            }
        }

        /// <summary>解析 USB 地址为串口地址并写入错误提示。</summary>
        private HSerialPortAddress Resolve(HUsbAddress address)
        {
            if (address == null)
            {
                StrError = "Address is null";
                return null;
            }
            address.SocketType = HSocketType.Usb_Server;
            HSerialPortAddress serial = address.ToSerialPortAddress();
            if (serial == null)
            {
                StrError = "No USB CDC serial port found (PortName/VID/PID unmatched): "
                    + (address.StationName ?? address.Name ?? string.Empty);
            }
            return serial;
        }

        /// <summary>同步内部错误文本。</summary>
        private void SyncError(int result)
        {
            if (result < 0)
            {
                StrError = mServer.StrError;
            }
        }

        /// <summary>挂接内部串口服务端事件并转换为 USB 地址事件。</summary>
        private void HookEvents()
        {
            mServer.StationConnected += (sender, serial) =>
                StationConnected?.Invoke(this, MapAddress(serial));
            mServer.StationDisconnected += (sender, serial) =>
                StationDisconnected?.Invoke(this, MapAddress(serial));
            mServer.DataReceived += (sender, serial, data) =>
                DataReceived?.Invoke(this, MapAddress(serial), data);
            mServer.StationError += (sender, serial, msg) =>
            {
                StrError = msg;
                StationError?.Invoke(this, MapAddress(serial), msg);
            };
        }

        /// <summary>按串口号回查登记的 USB 地址；找不到时以串口号构造一个临时地址。</summary>
        private HUsbAddress MapAddress(HSerialPortAddress serial)
        {
            if (serial == null)
            {
                return new HUsbAddress();
            }
            lock (mLock)
            {
                HUsbAddress address;
                if (mUsbAddresses.TryGetValue(serial.PortName ?? string.Empty, out address))
                {
                    return address;
                }
            }
            return new HUsbAddress { PortName = serial.PortName, StationName = serial.StationName };
        }
    }
}
