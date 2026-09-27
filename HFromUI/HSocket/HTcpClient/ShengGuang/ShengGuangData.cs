using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HTcpClient.ShengGuang
{
    using HFromUI.HConvert;

    public class ShengGuangData
    {
        public enum MessageType
        { 
            M0x0001 = 1, 
            M0x0002 = 2,
            M0x0003 = 3,
            M0x0004 = 4,
            M0x0005 = 5,
            M0x0006 = 6, 
            M0x0007 = 7,
        }
        public SocketTcpClient socketTcpClient;

        private bool Heartbeat = false;
        public bool SendHeartbeat()
        {
            Heartbeat = !Heartbeat;
            return Send(Heartbeat?"1":"0", MessageType.M0x0001);
        }
        public bool Send(string str, MessageType message )
        {
            bool isOK = false;
            HBytes hBytes = new HBytes();

            hBytes.Add((byte)0x5A);
            hBytes.Add((byte)0xA5);
            hBytes.Add((byte)0x3C);
            hBytes.Add((byte)0xC3);

            HBytes strBytes = new HBytes();
            strBytes.Add((short)message, 3);
            strBytes.Add(str, Encoding.UTF8);
            strBytes.AddXorChecksum(false);

            hBytes.Add(strBytes.Count,3);
            hBytes.Add(strBytes);

            if (socketTcpClient == null)
            {
                return isOK;
            }
            isOK =    socketTcpClient.Write(hBytes.Bytes.ToArray())>=0;
            return isOK;
        }
    }
}
