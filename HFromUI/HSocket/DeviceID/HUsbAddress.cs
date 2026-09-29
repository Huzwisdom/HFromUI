using HFromUI.HBase;
using HFromUI.HEnum;
using System.Collections.Generic;
using System.IO.Ports;
using Microsoft.Win32;

namespace HFromUI.HSocket.DeviceID
{
    /// <summary>
    /// USB 设备连接地址（DeviceID 体系）：面向 USB CDC（虚拟串口）设备，
    /// 可直接指定串口号，或仅给 VID/PID 由系统注册表自动发现对应 COM 口。
    /// <para>说明：Windows 下绝大多数 USB 转串口/USB CDC 设备（CH340/FT232/STM32 USB CDC 等）
    /// 均以 COM 口形式枚举，底层通讯与串口完全一致；HID/WinUSB 设备需厂商 SDK，不在此列。</para>
    /// </summary>
    public class HUsbAddress : HDataBase
    {
        /// <summary>虚拟串口号，如 COM3（为空时按 Vid/Pid 自动发现）。</summary>
        public string PortName { set; get; }

        /// <summary>设备厂商标识 VID（16 位，十进制；0 表示不按 VID 过滤）。</summary>
        public int Vid { set; get; }

        /// <summary>设备产品标识 PID（16 位，十进制；0 表示不按 PID 过滤）。</summary>
        public int Pid { set; get; }

        /// <summary>波特率，CDC 虚拟串口默认 115200。</summary>
        public int BaudRate { set; get; } = 115200;

        /// <summary>数据位，默认 8。</summary>
        public int DataBits { set; get; } = 8;

        /// <summary>停止位：0=One,1=OnePointFive,2=Two（默认 One）。</summary>
        public int StopBits { set; get; } = 0;

        /// <summary>校验：0=None,1=Odd,2=Even,3=Mark,4=Space（默认 None）。</summary>
        public int Parity { set; get; } = 0;

        /// <summary>流控：0=None,1=XOnXOff,2=RequestToSend,3=RequestToSendXOnXOff（默认 None）。</summary>
        public int Handshake { set; get; } = 0;

        /// <summary>读超时（毫秒），默认 3000。</summary>
        public int ReadTimeout { set; get; } = 3000;

        /// <summary>写超时（毫秒），默认 3000。</summary>
        public int WriteTimeout { set; get; } = 3000;

        /// <summary>站点名称（多设备时的业务标识）。</summary>
        public string StationName { set; get; }

        /// <summary>站点编号。</summary>
        public int StationID { set; get; }

        /// <summary>通讯类型（Usb_Client / Usb_Server 角色）。</summary>
        public HSocketType SocketType { set; get; } = HSocketType.Usb_Client;

        /// <summary>
        /// 转换为等价的串口地址（USB CDC 底层即串口，文件传输/普通通讯均复用串口实现）。
        /// PortName 为空时尝试按 Vid/Pid 自动发现，失败返回 null（原因见错误返回对象的 Name 字段）。
        /// </summary>
        /// <returns>串口地址；无法确定串口号返回 null</returns>
        public HSerialPortAddress ToSerialPortAddress()
        {
            string portName = ResolvePortName();
            if (string.IsNullOrEmpty(portName))
            {
                return null;
            }
            return new HSerialPortAddress
            {
                PortName = portName,
                BaudRate = BaudRate <= 0 ? 115200 : BaudRate,
                DataBits = DataBits,
                StopBits = MapStopBits(StopBits),
                Parity = (Parity)Parity,
                Handshake = (Handshake)Handshake,
                ReadTimeout = ReadTimeout,
                WriteTimeout = WriteTimeout,
                StationName = StationName,
                StationID = StationID,
                Name = Name,
                GuidCode = GuidCode,
                SocketType = SocketType == HSocketType.Usb_Server
                    ? HSocketType.SerialPort_Server
                    : HSocketType.SerialPort_Client
            };
        }

        /// <summary>解析实际串口号：优先 PortName，否则按 VID/PID 发现第一个匹配设备。</summary>
        public string ResolvePortName()
        {
            if (!string.IsNullOrWhiteSpace(PortName))
            {
                return PortName.Trim();
            }
            List<string> ports = FindPortNames(Vid, Pid);
            return ports.Count > 0 ? ports[0] : null;
        }

        /// <summary>整数停止位映射为串口枚举（0=One,1=OnePointFive,2=Two）。</summary>
        private static System.IO.Ports.StopBits MapStopBits(int value)
        {
            switch (value)
            {
                case 1: return System.IO.Ports.StopBits.OnePointFive;
                case 2: return System.IO.Ports.StopBits.Two;
                default: return System.IO.Ports.StopBits.One;
            }
        }

        /// <summary>
        /// 在注册表中按 VID/PID 查找 USB CDC（虚拟串口）设备对应的 COM 口列表。
        /// </summary>
        /// <param name="vid">厂商 ID（0 表示不过滤）</param>
        /// <param name="pid">产品 ID（0 表示不过滤）</param>
        /// <returns>匹配到的串口号列表（如 COM3、COM12），无匹配返回空列表</returns>
        public static List<string> FindPortNames(int vid, int pid)
        {
            var result = new List<string>();
            try
            {
                using (RegistryKey usbRoot = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))
                {
                    if (usbRoot == null)
                    {
                        return result;
                    }
                    foreach (string deviceKeyName in usbRoot.GetSubKeyNames())
                    {
                        bool vidOk = vid <= 0
                            || deviceKeyName.StartsWith("VID_" + vid.ToString("X4"),
                                System.StringComparison.OrdinalIgnoreCase);
                        bool pidOk = pid <= 0
                            || deviceKeyName.IndexOf("PID_" + pid.ToString("X4"),
                                System.StringComparison.OrdinalIgnoreCase) >= 0;
                        if (!vidOk || !pidOk)
                        {
                            continue;
                        }
                        using (RegistryKey deviceKey = usbRoot.OpenSubKey(deviceKeyName))
                        {
                            if (deviceKey == null)
                            {
                                continue;
                            }
                            foreach (string instanceName in deviceKey.GetSubKeyNames())
                            {
                                using (RegistryKey paramKey = deviceKey.OpenSubKey(instanceName + @"\Device Parameters"))
                                {
                                    object port = paramKey == null ? null : paramKey.GetValue("PortName");
                                    if (port != null)
                                    {
                                        string name = port.ToString();
                                        if (!string.IsNullOrWhiteSpace(name) && !result.Contains(name))
                                        {
                                            result.Add(name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 注册表不可访问（权限/系统差异）时返回已发现结果
            }
            return result;
        }
    }
}
