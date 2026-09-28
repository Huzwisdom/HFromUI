using System;
using System.Collections.Generic;
using System.Text;

namespace HFromUI.HSocket.HSerialServer
{
    using HFromUI.HEnum;
    using HFromUI.HLangage;
    using HFromUI.HSocket.DeviceID;
    using HFromUI.HSocket.HSerialCommon;

    /// <summary>
    /// 串口服务端 / 被动方：物理串口是点对点链路，“服务端”按应用层角色定义——
    /// 打开一个或多个串口（站点）被动接收数据并按需应答，接口风格与 SocketTcpServer 对齐。
    /// 每个 HSerialPortAddress 对应一个串口站点（StationName/StationID 可作业务标识），
    /// 事件在后台线程触发，更新 UI 需自行 Invoke。
    /// </summary>
    public class SocketSerialServer
    {
        private readonly List<HSerialPortAddress> mAddresses = new List<HSerialPortAddress>();
        private readonly Dictionary<string, SerialPortSession> mSessions = new Dictionary<string, SerialPortSession>();
        private readonly object mLock = new object();

        /// <summary>文本编码格式（对新打开站点生效）。</summary>
        public Encoding Encoding = Encoding.UTF8;

        /// <summary>报错提示。</summary>
        public string StrError { get; private set; }

        /// <summary>是否至少有一个串口站点处于打开状态。</summary>
        public bool IsRunning
        {
            get { return ClientCount > 0; }
        }

        /// <summary>当前已打开的串口站点数量。</summary>
        public int ClientCount
        {
            get
            {
                lock (mLock)
                {
                    int count = 0;
                    foreach (SerialPortSession session in mSessions.Values)
                    {
                        if (session.IsOpen)
                        {
                            count++;
                        }
                    }
                    return count;
                }
            }
        }

        /// <summary>站点串口打开（接入）。</summary>
        public event Action<SocketSerialServer, HSerialPortAddress> StationConnected;

        /// <summary>站点串口关闭（移除）。</summary>
        public event Action<SocketSerialServer, HSerialPortAddress> StationDisconnected;

        /// <summary>收到某站点数据（data 为本帧字节副本）。</summary>
        public event Action<SocketSerialServer, HSerialPortAddress, byte[]> DataReceived;

        /// <summary>站点异常（打开失败、帧错等）。</summary>
        public event Action<SocketSerialServer, HSerialPortAddress, string> StationError;

        /// <summary>构造空服务端，后续用 Add/Start 添加站点。</summary>
        public SocketSerialServer()
        {
        }

        /// <summary>以单个串口站点构造并登记（调用 Start 后打开）。</summary>
        /// <param name="address">串口通讯参数</param>
        public SocketSerialServer(HSerialPortAddress address)
        {
            Add(address);
        }

        /// <summary>
        /// 登记一个串口站点（不立即打开）；同串口号重复登记将被忽略。
        /// </summary>
        /// <param name="address">串口通讯参数</param>
        public void Add(HSerialPortAddress address)
        {
            if (address == null || string.IsNullOrWhiteSpace(address.PortName))
            {
                return;
            }
            address.SocketType = HSocketType.SerialPort_Server;
            lock (mLock)
            {
                if (FindAddress(address.PortName) == null)
                {
                    mAddresses.Add(address);
                }
            }
        }

        /// <summary>
        /// 打开全部已登记站点。
        /// </summary>
        /// <returns>是否全部打开成功（任一失败原因见 StrError / StationError 事件）</returns>
        public bool Start()
        {
            bool allOk = true;
            List<HSerialPortAddress> snapshot;
            lock (mLock)
            {
                snapshot = new List<HSerialPortAddress>(mAddresses);
            }
            foreach (HSerialPortAddress address in snapshot)
            {
                if (!OpenStation(address))
                {
                    allOk = false;
                }
            }
            return allOk;
        }

        /// <summary>
        /// 登记并立即打开单个站点。
        /// </summary>
        /// <param name="address">串口通讯参数</param>
        /// <returns>是否打开成功</returns>
        public bool Start(HSerialPortAddress address)
        {
            Add(address);
            return OpenStation(address);
        }

        /// <summary>关闭全部站点（保留登记信息，可再次 Start）。</summary>
        public void Stop()
        {
            Close();
        }

        /// <summary>关闭全部站点（同 Stop，命名与 SocketTcpServer 对齐）。</summary>
        public void Close()
        {
            List<SerialPortSession> sessions;
            lock (mLock)
            {
                sessions = new List<SerialPortSession>(mSessions.Values);
                mSessions.Clear();
            }
            foreach (SerialPortSession session in sessions)
            {
                session.DataReceived -= OnSessionData;
                session.ErrorOccurred -= OnSessionError;
                HSerialPortAddress address = session.Address;
                session.Close();
                StationDisconnected?.Invoke(this, address);
            }
        }

        /// <summary>
        /// 关闭并移除指定串口号站点。
        /// </summary>
        /// <param name="portName">串口号，如 COM3</param>
        public void RemoveStation(string portName)
        {
            SerialPortSession session = null;
            HSerialPortAddress address = null;
            lock (mLock)
            {
                if (mSessions.TryGetValue(portName ?? string.Empty, out session))
                {
                    mSessions.Remove(session.Id);
                }
                address = FindAddress(portName);
                if (address != null)
                {
                    mAddresses.Remove(address);
                }
            }
            if (session != null)
            {
                session.DataReceived -= OnSessionData;
                session.ErrorOccurred -= OnSessionError;
                session.Close();
                StationDisconnected?.Invoke(this, session.Address);
            }
        }

        /// <summary>指定串口站点是否已打开。</summary>
        /// <param name="portName">串口号，如 COM3</param>
        /// <returns>是否打开</returns>
        public bool IsStationOpen(string portName)
        {
            lock (mLock)
            {
                SerialPortSession session;
                return mSessions.TryGetValue(portName ?? string.Empty, out session) && session.IsOpen;
            }
        }

        /// <summary>获取全部已登记站点地址。</summary>
        /// <returns>地址列表副本</returns>
        public List<HSerialPortAddress> GetStations()
        {
            lock (mLock)
            {
                return new List<HSerialPortAddress>(mAddresses);
            }
        }

        /// <summary>
        /// 向指定站点发送字节应答。
        /// </summary>
        /// <param name="portName">串口号，如 COM3</param>
        /// <param name="bytes">数据内容</param>
        /// <returns>实际写入字节数；站点不存在返回 -101，异常返回 -200</returns>
        public int Write(string portName, byte[] bytes)
        {
            SerialPortSession session = GetSession(portName);
            if (session == null)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = session.Write(bytes);
            if (result < 0)
            {
                StrError = session.StrError;
            }
            return result;
        }

        /// <summary>
        /// 向指定站点发送文本应答。
        /// </summary>
        /// <param name="portName">串口号，如 COM3</param>
        /// <param name="strCommand">文本内容</param>
        /// <returns>实际写入字节数；站点不存在返回 -102，异常返回 -200</returns>
        public int Write(string portName, string strCommand)
        {
            SerialPortSession session = GetSession(portName);
            if (session == null)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -102;
            }
            int result = session.Write(Encoding.GetBytes(strCommand));
            if (result < 0)
            {
                StrError = session.StrError;
            }
            return result;
        }

        /// <summary>
        /// 向全部已打开站点发送字节数据。
        /// </summary>
        /// <param name="bytes">数据内容</param>
        /// <returns>发送成功的站点数量</returns>
        public int WriteAll(byte[] bytes)
        {
            int ok = 0;
            foreach (SerialPortSession session in GetSessions())
            {
                if (session.Write(bytes) > 0)
                {
                    ok++;
                }
            }
            return ok;
        }

        /// <summary>
        /// 向全部已打开站点发送文本。
        /// </summary>
        /// <param name="strCommand">文本内容</param>
        /// <returns>发送成功的站点数量</returns>
        public int WriteAll(string strCommand)
        {
            return WriteAll(Encoding.GetBytes(strCommand));
        }

        /// <summary>
        /// 清空指定站点接收缓存（丢弃残留字节，返回丢弃数量）。
        /// </summary>
        /// <param name="portName">串口号，如 COM3</param>
        /// <returns>丢弃字节数；站点不存在返回 -101，异常返回 -200</returns>
        public int ClearCache(string portName)
        {
            SerialPortSession session = GetSession(portName);
            if (session == null)
            {
                StrError = HTranslation.GetContent(@"未连接!(Disconnected!)");
                return -101;
            }
            int result = session.ClearCache();
            if (result < 0)
            {
                StrError = session.StrError;
            }
            return result;
        }

        /// <summary>
        /// 清空全部已打开站点接收缓存。
        /// </summary>
        /// <returns>累计丢弃字节数</returns>
        public int ClearCacheAll()
        {
            int total = 0;
            foreach (SerialPortSession session in GetSessions())
            {
                int result = session.ClearCache();
                if (result > 0)
                {
                    total += result;
                }
            }
            return total;
        }

        /// <summary>打开单个站点并挂接事件；已打开则直接成功。</summary>
        private bool OpenStation(HSerialPortAddress address)
        {
            lock (mLock)
            {
                SerialPortSession existing;
                if (mSessions.TryGetValue(address.PortName, out existing) && existing.IsOpen)
                {
                    return true;
                }
            }
            var session = new SerialPortSession(address)
            {
                Encoding = Encoding
            };
            session.DataReceived += OnSessionData;
            session.ErrorOccurred += OnSessionError;
            if (!session.Open())
            {
                StrError = session.StrError;
                StationError?.Invoke(this, address, StrError);
                session.DataReceived -= OnSessionData;
                session.ErrorOccurred -= OnSessionError;
                return false;
            }
            lock (mLock)
            {
                mSessions[address.PortName] = session;
            }
            StationConnected?.Invoke(this, address);
            return true;
        }

        /// <summary>取已打开会话（不存在返回 null）。</summary>
        private SerialPortSession GetSession(string portName)
        {
            lock (mLock)
            {
                SerialPortSession session;
                return mSessions.TryGetValue(portName ?? string.Empty, out session) ? session : null;
            }
        }

        /// <summary>取已打开会话快照。</summary>
        private List<SerialPortSession> GetSessions()
        {
            lock (mLock)
            {
                return new List<SerialPortSession>(mSessions.Values);
            }
        }

        /// <summary>按串口号查登记地址。</summary>
        private HSerialPortAddress FindAddress(string portName)
        {
            foreach (HSerialPortAddress address in mAddresses)
            {
                if (string.Equals(address.PortName, portName, StringComparison.OrdinalIgnoreCase))
                {
                    return address;
                }
            }
            return null;
        }

        /// <summary>会话数据透传为站点事件。</summary>
        private void OnSessionData(SerialPortSession session, byte[] data)
        {
            DataReceived?.Invoke(this, session.Address, data);
        }

        /// <summary>会话错误透传为站点事件。</summary>
        private void OnSessionError(SerialPortSession session, Exception ex)
        {
            StrError = session.StrError;
            StationError?.Invoke(this, session.Address, StrError);
        }
    }
}
