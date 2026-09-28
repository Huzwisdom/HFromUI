using HFromUI.HBase;
using HFromUI.HEnum;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HSocket.DeviceID
{
    public enum HBaudRate
    {
        B300 = 300,
        B600 = 600,
        B1200 = 1200,
        B2400 = 2400,
        B4800 = 4800,
        B9600 = 9600,
        B19200 = 19200,
        B38400 = 38400,
        B57600 = 57600,
        B115200 = 115200,
        B230400 = 230400,
        B460800 = 460800,
        B921600 = 921600
    }
    /// <summary>
    /// 串口通讯参数类
    /// </summary>
    public class HSerialPortAddress : HDataBase
    {
        /// <summary>
        /// 串口名，例如 "COM1"
        /// </summary>
        public string PortName { set; get; }

        /// <summary>
        /// 波特率，例如 9600、115200
        /// </summary>
        public int BaudRate { set; get; }

        /// <summary>
        /// 数据位（5~8）
        /// </summary>
        public int DataBits { set; get; } = 8;

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { set; get; } = StopBits.One;

        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Parity { set; get; } = Parity.None;

        /// <summary>
        /// 握手/流控方式
        /// </summary>
        public Handshake Handshake { set; get; } = Handshake.None;

        /// <summary>
        /// 读取超时时间（毫秒）
        /// </summary>
        public int ReadTimeout { set; get; }

        /// <summary>
        /// 写入超时时间（毫秒）
        /// </summary>
        public int WriteTimeout { set; get; }

        /// <summary>
        /// 站点名称（可选，用于多设备标识）
        /// </summary>
        public string StationName { set; get; }

        /// <summary>
        /// 站点 ID（可选，用于多设备标识）
        /// </summary>
        public int StationID { set; get; }

        public HSocketType SocketType { set; get; }
    }
}