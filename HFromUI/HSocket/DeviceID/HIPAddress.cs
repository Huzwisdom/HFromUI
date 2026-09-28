using HFromUI.HBase;
using HFromUI.HEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HSocket.DeviceID
{
    public class HIPAddress: HDataBase
    {
        public string IP { set; get; }
        public int Port { set; get; }
        public string HostName { set; get; }
        public string StationName { set; get; }
        public int StationID { set; get; }
        public HSocketType SocketType { set; get; }

       

    }
}
