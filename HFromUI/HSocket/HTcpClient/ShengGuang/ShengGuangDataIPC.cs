using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HSocket.HTcpClient.ShengGuang
{
    using HFromUI.HConvert;
    using HFromUI.HFile;
    using HFromUI.HLangage;
    using HFromUI.HSocket.DeviceID;
    using HFromUI.HSocket.HNamedPipe;
    using System.Threading;

    public class ShengGuangDataIPC
    {

        public int SaveDay { set; get; }
        public HNamedPipeAddress hAddress { set; get; } = new HNamedPipeAddress();

        public string Error { private set; get; }

        public int HeartbeatSpan { set; get; } = 1;
        private SocketNamedPipeClient socketTcpClient;
        private bool IsRun = false;
        private Task task;
        private List<ShengGuangDataRun> sendList=new List<ShengGuangDataRun>();
        private readonly string path= HFromUI.HData.HAppData.AppPath + @"\ShengGuangData.txt";
        private List<ShengGuangDataRun> failureList = new List<ShengGuangDataRun>();

        public string PipeName
        {
            set
            {
                hAddress.PipeName = value;
            }
            get
            { 
            return hAddress.PipeName;
            }
        }


        public void Add(string data)
        {
            ShengGuangDataRun shengGuangDataRun = new ShengGuangDataRun();
            shengGuangDataRun.Data = data;
            shengGuangDataRun.DateTime = DateTime.Now;
            sendList.Add(shengGuangDataRun);
        }
        public void Run()
        {
            if (task!=null)
            {
                return;
            }
            DateTime dateTime = DateTime.Now;
            IsRun = true;

            task = Task.Run(() => {
                while (IsRun)
                {
                    Thread.Sleep(20);
                    if (sendList.Count>0)
                    {
                        bool isok = false;
                        ShengGuangDataRun buffer = sendList[0];

                        if (string.IsNullOrWhiteSpace(buffer.Data))
                        {
                            if (Send(HJsonFile.Serialize(buffer.DeviceStatus, false), buffer.MessageType))
                            {
                                isok = true;
                            }
                        }
                        else
                        {
                            if (Send(buffer.Data,buffer.MessageType))
                            {
                                isok = true;
                            }
                        }
                        if (isok)
                        {
                            sendList.RemoveAt(0);
                        }
                        else
                        {
                            failureList.Add(buffer);
                            sendList.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (failureList.Count > 0)
                        {
                            bool isok = false;
                            ShengGuangDataRun buffer = failureList[0];

                            if (string.IsNullOrWhiteSpace(buffer.Data))
                            {
                                if (Send(HJsonFile.Serialize(buffer.DeviceStatus, false), buffer.MessageType))
                                {
                                    isok = true;
                                }
                            }
                            else
                            {
                                if (Send(buffer.Data, buffer.MessageType))
                                {
                                    isok = true;
                                }
                            }
                            if (isok)
                            {
                                failureList.RemoveAt(0);
                            }
                            else
                            {
                                failureList.RemoveAt(0);
                                failureList.Add(buffer);
                            }
                        }

                    }
                    if (HeartbeatSpan>0)
                    {
                        if ((DateTime.Now - dateTime).TotalSeconds >= HeartbeatSpan)
                        {
                            dateTime = DateTime.Now;
                            SendHeartbeat();
                        }
                    }
                }
            });
        }

        public void Stop()
        {
            IsRun = false;
            if (task!=null)
            {
                task.Wait();
            }
            if (sendList.Count>0)
            {
                failureList.AddRange(sendList);
             
            }
            if (failureList.Count>0)
            {
                HTxtFile hTxtFile = new HTxtFile(path);
                hTxtFile.WriteAllText(HJsonFile.Serialize(failureList, false));
            }
            if (task != null)
            {
                task.Dispose();
                task = null;
            }
        }
        public void Load()
        {
            HTxtFile hTxtFile = new HTxtFile(path);
            if (hTxtFile.IsDocuments())
            {
                failureList = HJsonFile.Deserialize<List<ShengGuangDataRun>>(hTxtFile.ReadAllText());
                try
                {
                    hTxtFile.DeleteText();
                }
                catch { }

                if (SaveDay>0)
                {
                    if (failureList.Count > 0)
                    {
                        List<ShengGuangDataRun> DataRuns = new List<ShengGuangDataRun>();
                        foreach (var item in failureList)
                        {
                            if ((DateTime.Now- item.DateTime).TotalDays<SaveDay)
                            {
                                DataRuns.Add(item);
                            }
                        }
                        failureList.Clear();
                        failureList = DataRuns;
                    }
                }

            }
        }

        private bool Heartbeat = false;
        public bool SendHeartbeat()
        {
            Heartbeat = !Heartbeat;
            return Send(Heartbeat?"1":"0", MessageType.M0x0001);
        }
        public bool Send(string str, MessageType message )
        {
            int num = 0;
          
            if (hAddress == null)
            {
                Error = HTranslation.GetContent("IP地址没有设置");
                return false;
            }
            if (socketTcpClient == null)
            {
                socketTcpClient = new SocketNamedPipeClient(hAddress);
                socketTcpClient.Timeout = 2000;
            }
        SendNum:
            if (!socketTcpClient.IsConnected)
            {
                socketTcpClient.Connect();
            }
            bool isOK = false;
            HBytes hBytes = new HBytes();
            hBytes.Clear();
            hBytes.Add((byte)0x5A);
            hBytes.Add((byte)0xA5);
            hBytes.Add((byte)0x3C);
            hBytes.Add((byte)0xC3);

            HBytes strBytes = new HBytes();
            strBytes.Clear();
            strBytes.Add((short)message, 3);
            strBytes.Add(str, Encoding.UTF8);
            strBytes.AddXorChecksum(false);

            hBytes.Add(strBytes.Count,3);
            hBytes.Add(strBytes);

            if (socketTcpClient == null)
            {
                return isOK;
            }
            isOK =   socketTcpClient.Write(hBytes.Bytes.ToArray())>=0;
            num++;
            if (!isOK)
            {
                if (num < 6)
                {
                    goto SendNum;
                }
                Error = socketTcpClient.StrError;
            }
            return isOK;
        }
    }
}
