using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HFromUI.HSocket.HTcpClient.ShengGuang.ShengGuangData;

namespace HFromUI.HSocket.HTcpClient.ShengGuang
{
    public class ShengGuangDataRun
    {
        public DateTime DateTime { set; get; }

        public string Data { set; get; }

        public DeviceStatus DeviceStatus { set; get; }

        public MessageType MessageType { set; get; } = MessageType.M0x0005;
    }
}
