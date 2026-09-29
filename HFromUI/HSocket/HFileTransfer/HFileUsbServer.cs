using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HFileTransfer
{
    using HFromUI.HEnum;
    using HFromUI.HSocket.DeviceID;
    using HFromUI.HSocket.HSerialCommon;

    /// <summary>
    /// USB CDC（虚拟串口）大文件传输服务端 / 被动方：以 HUsbAddress 登记一个或多个
    /// USB CDC 设备站点（可指定 COM 口或按 VID/PID 自动发现），打开后在各 CDC 底层流上运行
    /// 与网口相同的分帧协议（自动落盘/断点续传），也可向处于接收等待的站点推送文件。
    /// 事件均在后台线程触发，更新 UI 需 Invoke。仅适用于 USB CDC/USB 转串口设备。
    /// </summary>
    public class HFileUsbServer : HFileTransferBase
    {
        /// <summary>单个 USB 站点的工作会话。</summary>
        private sealed class StationSession
        {
            public HUsbAddress UsbAddress;
            public HSerialPortAddress SerialAddress;
            public SerialPortSession Port;
            public Thread Worker;
            public readonly Queue<HFileWorkItem> PushQueue = new Queue<HFileWorkItem>();
            public readonly object QueueLock = new object();

            /// <summary>循环级上下文：仅 Stop/RemoveStation 时取消（结束工作循环）。</summary>
            public readonly HFileTransferContext Context = new HFileTransferContext();

            /// <summary>当前正在接收的传输上下文（取消只中止本次传输，站点保留）。</summary>
            public volatile HFileTransferContext Receiving;

            /// <summary>当前正在推送发送的传输上下文。</summary>
            public volatile HFileTransferContext Sending;
        }

        private readonly List<HUsbAddress> mAddresses = new List<HUsbAddress>();
        private readonly Dictionary<string, StationSession> mStations = new Dictionary<string, StationSession>(
            StringComparer.OrdinalIgnoreCase);
        private readonly object mLock = new object();
        private volatile bool mRunning;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否至少有一个站点在运行。</summary>
        public bool IsRunning
        {
            get { return mRunning; }
        }

        /// <summary>当前已打开的站点数量。</summary>
        public int StationCount
        {
            get
            {
                lock (mLock)
                {
                    return mStations.Count;
                }
            }
        }

        /// <summary>设备站点打开（接入）。</summary>
        public event Action<HFileUsbServer, HUsbAddress> StationConnected;

        /// <summary>设备站点关闭。</summary>
        public event Action<HFileUsbServer, HUsbAddress> StationDisconnected;

        /// <summary>
        /// 请求保存路径：(虚拟串口号, 文件名, 文件长度) → 本地完整保存路径；
        /// 不订阅或返回 null 时拒绝接收。
        /// </summary>
        public event Func<string, string, long, string> FileRequested;

        /// <summary>站点异常（打开失败、设备未发现、帧失步等）。</summary>
        public event Action<HFileUsbServer, string, string> StationError;

        /// <summary>构造空服务端，后续用 Add/Start 添加设备。</summary>
        public HFileUsbServer()
        {
            ChunkSize = SerialChunkSize;
            SavePathResolver = ResolveSavePath;
        }

        /// <summary>以单个设备站点构造（调用 Start 后打开）。</summary>
        /// <param name="address">USB CDC 设备地址</param>
        public HFileUsbServer(HUsbAddress address) : this()
        {
            Add(address);
        }

        /// <summary>
        /// 登记一个 USB CDC 站点（不立即打开）；PortName 为空时按 VID/PID 发现 COM 口，
        /// 同串口号重复登记忽略；无法解析到串口号时忽略（原因见 StrError）。
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
                if (FindAddress(serial.PortName) == null)
                {
                    mAddresses.Add(address);
                }
            }
        }

        /// <summary>
        /// 打开全部已登记站点。
        /// </summary>
        /// <returns>是否全部打开成功（失败原因见 StrError / StationError 事件）</returns>
        public bool Start()
        {
            bool allOk = true;
            List<HUsbAddress> snapshot;
            lock (mLock)
            {
                snapshot = new List<HUsbAddress>(mAddresses);
            }
            foreach (HUsbAddress address in snapshot)
            {
                if (!OpenStation(address))
                {
                    allOk = false;
                }
            }
            mRunning = allOk || StationCount > 0;
            return allOk;
        }

        /// <summary>
        /// 登记并立即打开单个设备站点。
        /// </summary>
        /// <param name="address">USB 设备地址</param>
        /// <returns>是否打开成功</returns>
        public bool Start(HUsbAddress address)
        {
            if (Resolve(address) == null)
            {
                return false;
            }
            Add(address);
            bool ok = OpenStation(address);
            mRunning = StationCount > 0;
            return ok;
        }

        /// <summary>关闭全部站点（保留登记信息，可再次 Start）。</summary>
        public void Stop()
        {
            Close();
        }

        /// <summary>关闭全部站点（同 Stop）。</summary>
        public void Close()
        {
            mRunning = false;
            List<StationSession> snapshot;
            lock (mLock)
            {
                snapshot = new List<StationSession>(mStations.Values);
                mStations.Clear();
            }
            foreach (StationSession station in snapshot)
            {
                ShutdownStation(station);
                StationDisconnected?.Invoke(this, station.UsbAddress);
            }
        }

        /// <summary>
        /// 关闭并移除指定站点。
        /// </summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        public void RemoveStation(string portName)
        {
            StationSession station = null;
            HUsbAddress address = null;
            lock (mLock)
            {
                string key = portName ?? string.Empty;
                if (mStations.TryGetValue(key, out station))
                {
                    mStations.Remove(key);
                }
                address = FindAddress(key);
                if (address != null)
                {
                    mAddresses.Remove(address);
                }
            }
            if (station != null)
            {
                ShutdownStation(station);
                StationDisconnected?.Invoke(this, station.UsbAddress);
            }
        }

        /// <summary>指定站点是否已打开。</summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        public bool IsStationOpen(string portName)
        {
            lock (mLock)
            {
                return mStations.ContainsKey(portName ?? string.Empty);
            }
        }

        /// <summary>获取全部已登记 USB 设备地址。</summary>
        /// <returns>地址列表副本</returns>
        public List<HUsbAddress> GetStations()
        {
            lock (mLock)
            {
                return new List<HUsbAddress>(mAddresses);
            }
        }

        /// <summary>
        /// 清空指定站点接收缓存（丢弃残留字节，返回丢弃数量）。
        /// 警告：站点工作线程会持续读取该端口，请勿在站点开启（尤其传输）期间调用，
        /// 建议 Stop 后维护时使用。
        /// </summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        /// <returns>丢弃字节数；站点不在线返回 0</returns>
        public int ClearCache(string portName)
        {
            StationSession station = GetStation(portName);
            Stream stream = station == null ? null : station.Port.BaseStream;
            return stream == null ? 0 : ClearStreamCache(stream);
        }

        /// <summary>清空全部站点接收缓存。</summary>
        public int ClearCacheAll()
        {
            int total = 0;
            foreach (StationSession station in GetSessions())
            {
                Stream stream = station.Port.BaseStream;
                if (stream != null)
                {
                    total += ClearStreamCache(stream);
                }
            }
            return total;
        }

        /// <summary>取消指定站点正在进行的传输（分片保留以便续传；站点不断开）。</summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        public void CancelStation(string portName)
        {
            StationSession station = GetStation(portName);
            if (station != null)
            {
                CancelSessionTransfers(station);
            }
        }

        /// <summary>取消全部站点正在进行的传输（分片保留以便续传；站点不断开）。</summary>
        public void CancelAll()
        {
            foreach (StationSession station in GetSessions())
            {
                CancelSessionTransfers(station);
            }
        }

        /// <summary>取消单个会话的在途传输与排队推送（不动循环级上下文）。</summary>
        private static void CancelSessionTransfers(StationSession station)
        {
            HFileTransferContext receiving = station.Receiving;
            if (receiving != null)
            {
                receiving.Cancel = true;
            }
            HFileTransferContext sending = station.Sending;
            if (sending != null)
            {
                sending.Cancel = true;
            }
            lock (station.QueueLock)
            {
                while (station.PushQueue.Count > 0)
                {
                    HFileWorkItem item = station.PushQueue.Dequeue();
                    item.Context.Cancel = true;
                    item.Tcs.TrySetResult(false);
                }
            }
        }

        /// <summary>
        /// 向指定站点排队推送一个文件（对端必须正处于 ReceiveFile 等待中）。
        /// </summary>
        /// <param name="portName">虚拟串口号，如 COM3</param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="remoteName">对端保存文件名（null 使用本地文件名）</param>
        /// <returns>发送结果任务；站点不在线返回失败任务</returns>
        public Task<bool> PushFileAsync(string portName, string filePath, string remoteName = null)
        {
            StationSession station = GetStation(portName);
            if (station == null)
            {
                return Task.FromResult(false);
            }
            var item = new HFileWorkItem { FilePath = filePath, RemoteName = remoteName };
            lock (station.QueueLock)
            {
                station.PushQueue.Enqueue(item);
            }
            return item.Tcs.Task;
        }

        /// <summary>推送文件（阻塞等待发送完成）。</summary>
        public bool PushFile(string portName, string filePath, string remoteName = null)
        {
            try
            {
                return PushFileAsync(portName, filePath, remoteName).Result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>USB 地址解析为 CDC 串口地址（VID/PID 发现失败记录错误）。</summary>
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

        /// <summary>保存路径事件适配到基类解析器（Tag 为虚拟串口号）。</summary>
        private string ResolveSavePath(object tag, string fileName, long fileSize)
        {
            return FileRequested?.Invoke(tag as string, fileName, fileSize);
        }

        /// <summary>打开单个站点并启动工作线程；已打开直接成功。</summary>
        private bool OpenStation(HUsbAddress address)
        {
            HSerialPortAddress serial = Resolve(address);
            if (serial == null)
            {
                return false;
            }
            lock (mLock)
            {
                StationSession existing;
                if (mStations.TryGetValue(serial.PortName, out existing) && existing.Port.IsOpen)
                {
                    return true;
                }
            }
            var port = new SerialPortSession(serial)
            {
                UseDataEvent = false
            };
            if (!port.Open())
            {
                StrError = port.StrError;
                StationError?.Invoke(this, serial.PortName, StrError);
                return false;
            }
            var station = new StationSession
            {
                UsbAddress = address,
                SerialAddress = serial,
                Port = port
            };
            lock (mLock)
            {
                mStations[serial.PortName] = station;
            }
            station.Worker = new Thread(() => StationWorker(station))
            {
                IsBackground = true,
                Name = "HFileUsbServer-" + serial.PortName
            };
            station.Worker.Start();
            StationConnected?.Invoke(this, address);
            return true;
        }

        /// <summary>单站点工作循环（接收文件 / 排队推送复用基类轮询循环）。</summary>
        private void StationWorker(StationSession station)
        {
            try
            {
                Stream stream = station.Port.BaseStream;
                ServeLoopCore(stream, () =>
                {
                    HFileWorkItem item = Dequeue(station);
                    station.Sending = item == null ? null : item.Context;
                    return item;
                },
                    () => mRunning && station.Port.IsOpen,
                    station.SerialAddress.PortName, station.Context,
                    ctx => station.Receiving = ctx);
            }
            catch (Exception ex)
            {
                StrError = "Worker Error:" + ex.Message;
                StationError?.Invoke(this, station.SerialAddress.PortName, StrError);
            }
        }

        /// <summary>停止单个站点：取消传输、失败待推送、关闭串口。</summary>
        private void ShutdownStation(StationSession station)
        {
            station.Context.Cancel = true;
            lock (station.QueueLock)
            {
                while (station.PushQueue.Count > 0)
                {
                    station.PushQueue.Dequeue().Tcs.TrySetResult(false);
                }
            }
            station.Port.Close();
        }

        /// <summary>取该站点下一个待推送工作项。</summary>
        private static HFileWorkItem Dequeue(StationSession station)
        {
            lock (station.QueueLock)
            {
                return station.PushQueue.Count > 0 ? station.PushQueue.Dequeue() : null;
            }
        }

        /// <summary>按虚拟串口号查登记的 USB 地址。</summary>
        private HUsbAddress FindAddress(string portName)
        {
            foreach (HUsbAddress address in mAddresses)
            {
                if (string.Equals(address.PortName, portName, StringComparison.OrdinalIgnoreCase))
                {
                    return address;
                }
                string resolved = address.ResolvePortName();
                if (string.Equals(resolved, portName, StringComparison.OrdinalIgnoreCase))
                {
                    return address;
                }
            }
            return null;
        }

        /// <summary>按虚拟串口号取站点会话。</summary>
        private StationSession GetStation(string portName)
        {
            lock (mLock)
            {
                StationSession station;
                return mStations.TryGetValue(portName ?? string.Empty, out station) ? station : null;
            }
        }

        /// <summary>取站点会话快照。</summary>
        private List<StationSession> GetSessions()
        {
            lock (mLock)
            {
                return new List<StationSession>(mStations.Values);
            }
        }
    }
}
