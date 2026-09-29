using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HFromUI.HEnum;
using HFromUI.HSocket.DeviceID;

namespace HFromUI.HSocket.HMail
{
    /// <summary>
    /// POP3 收信客户端（以 HMailAddress 作为连接地址）：手写 POP3 对话，
    /// 支持 995 隐式 SSL 与 110 明文，登录后可统计邮件、按序号收取（含附件自动落盘）、
    /// 删除邮件。邮件内容用 HMimeCodec 解析为 HMailMessage。
    /// 服务器原始错误保留在 StrError（如 -ERR 登录失败）。
    /// </summary>
    public class HMailReceiver : IDisposable
    {
        private const string Crlf = "\r\n";

        private TcpClient mClient;
        private Stream mStream;

        /// <summary>邮件服务器连接地址（POP3 部分生效）。</summary>
        public HMailAddress hMailAddress { set; get; }

        /// <summary>连接/读写超时（毫秒），默认 15 秒。</summary>
        public int Timeout { set; get; } = 15000;

        /// <summary>报错提示（含服务器原始应答）。</summary>
        public string StrError { get; private set; }

        /// <summary>邮箱邮件数（Connect 登录后由 STAT 获取）。</summary>
        public int MessageCount { get; private set; }

        /// <summary>邮箱占用字节数（STAT 获取）。</summary>
        public long MailboxSize { get; private set; }

        /// <summary>是否已登录 POP3 服务器。</summary>
        public bool IsConnected { get; private set; }

        /// <summary>收到一封邮件（ReceiveAll 过程中触发，后台线程）。</summary>
        public event Action<HMailReceiver, HMailMessage> MessageReceived;

        /// <summary>以 HMailAddress 构造。</summary>
        /// <param name="address">邮件服务器地址</param>
        public HMailReceiver(HMailAddress address)
        {
            hMailAddress = address ?? new HMailAddress();
            hMailAddress.SocketType = HSocketType.POP3_Client;
        }

        /// <summary>
        /// 连接并登录 POP3 服务器，登录后自动 STAT 获取邮件数。
        /// </summary>
        /// <returns>是否登录成功（失败原因见 StrError）</returns>
        public bool Connect()
        {
            try
            {
                int port = hMailAddress.Pop3Port <= 0 ? 110 : hMailAddress.Pop3Port;
                mClient = new TcpClient();
                var connectTask = mClient.ConnectAsync(hMailAddress.Pop3Host, port);
                if (!connectTask.Wait(Timeout))
                {
                    StrError = "Connect POP3 timeout " + hMailAddress.Pop3Host + ":" + port;
                    return false;
                }
                mClient.ReceiveTimeout = Timeout;
                mClient.SendTimeout = Timeout;
                NetworkStream network = mClient.GetStream();
                if (hMailAddress.Pop3Ssl)
                {
                    var ssl = new SslStream(network, false, ValidateCertificate, null);
                    ssl.AuthenticateAsClient(hMailAddress.Pop3Host);
                    mStream = ssl;
                }
                else
                {
                    mStream = network;
                }

                ReadSingleLine(); // +OK greeting
                Command("USER", hMailAddress.UserName);
                Command("PASS", hMailAddress.Password);

                string stat = Command("STAT", null);
                string[] parts = stat.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int count;
                long size;
                if (parts.Length >= 3 && int.TryParse(parts[1], out count) && long.TryParse(parts[2], out size))
                {
                    MessageCount = count;
                    MailboxSize = size;
                }
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                Close();
                return false;
            }
        }

        /// <summary>关闭连接并发送 QUIT（QUIT 时服务器才会真正执行 DELE 删除）。</summary>
        public void Close()
        {
            try
            {
                if (mStream != null && IsConnected)
                {
                    WriteRaw("QUIT");
                    ReadSingleLine();
                }
            }
            catch
            {
                // 关闭阶段忽略
            }
            finally
            {
                IsConnected = false;
                try { mStream?.Close(); } catch { /* 忽略 */ }
                try { mClient?.Close(); } catch { /* 忽略 */ }
                mStream = null;
                mClient = null;
            }
        }

        /// <summary>
        /// 收取指定序号邮件（序号从 1 开始），附件保存到指定目录。
        /// </summary>
        /// <param name="sequenceNumber">邮件序号 1~MessageCount</param>
        /// <param name="attachmentDir">附件保存目录（null 则附件仅保留字节不落盘）</param>
        /// <returns>解析后的邮件；失败返回 null（原因见 StrError）</returns>
        public HMailMessage ReceiveMessage(int sequenceNumber, string attachmentDir = null)
        {
            if (!CheckConnected())
            {
                return null;
            }
            try
            {
                WriteRaw("RETR " + sequenceNumber);
                string first = ReadSingleLine();
                if (!IsOk(first))
                {
                    throw new IOException(first);
                }
                byte[] raw = ReadDataBlock();
                HMailMessage message = HMimeCodec.Parse(raw);
                message.SequenceNumber = sequenceNumber;
                SaveAttachments(message, attachmentDir);
                return message;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 收取全部邮件。
        /// </summary>
        /// <param name="attachmentDir">附件保存目录</param>
        /// <param name="deleteAfterReceive">是否收取后标记删除（本次连接 QUIT 后生效）</param>
        /// <returns>成功收取并解析的邮件列表</returns>
        public List<HMailMessage> ReceiveAll(string attachmentDir = null, bool deleteAfterReceive = false)
        {
            var result = new List<HMailMessage>();
            if (!CheckConnected())
            {
                return result;
            }
            for (int i = 1; i <= MessageCount; i++)
            {
                HMailMessage message = ReceiveMessage(i, attachmentDir);
                if (message == null)
                {
                    continue;
                }
                result.Add(message);
                MessageReceived?.Invoke(this, message);
                if (deleteAfterReceive)
                {
                    DeleteMessage(i);
                }
            }
            return result;
        }

        /// <summary>标记删除指定序号邮件（连接正常 QUIT 后生效）。</summary>
        /// <param name="sequenceNumber">邮件序号</param>
        /// <returns>服务器是否接受</returns>
        public bool DeleteMessage(int sequenceNumber)
        {
            if (!CheckConnected())
            {
                return false;
            }
            try
            {
                Command("DELE", sequenceNumber.ToString());
                return true;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>空操作保持连接（并刷新 STAT 计数）。</summary>
        public bool Noop()
        {
            if (!CheckConnected())
            {
                return false;
            }
            try
            {
                Command("NOOP", null);
                string stat = Command("STAT", null);
                string[] parts = stat.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int count;
                if (parts.Length >= 2 && int.TryParse(parts[1], out count))
                {
                    MessageCount = count;
                }
                return true;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
        }

        /// <summary>异步连接。</summary>
        public Task<bool> ConnectAsync()
        {
            return Task.Run(() => Connect());
        }

        /// <summary>异步收取全部邮件。</summary>
        public Task<List<HMailMessage>> ReceiveAllAsync(string attachmentDir = null, bool deleteAfterReceive = false)
        {
            return Task.Run(() => ReceiveAll(attachmentDir, deleteAfterReceive));
        }

        /// <summary>
        /// 清空接收缓存（丢弃流中尚未读取的字节），返回丢弃数量；
        /// 正常请求/响应一一对应时无需调用，协议错位重连前可复位。
        /// </summary>
        public int ClearCache()
        {
            int total = 0;
            try
            {
                mClient.ReceiveTimeout = 50;
                byte[] buffer = new byte[4096];
                while (mClient.Connected)
                {
                    int read = mStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        break;
                    }
                    total += read;
                }
            }
            catch
            {
                // 超时即缓冲已空
            }
            finally
            {
                try
                {
                    if (mClient != null)
                    {
                        mClient.ReceiveTimeout = Timeout;
                    }
                }
                catch
                {
                    // 忽略
                }
            }
            return total;
        }

        /// <summary>附件落盘并回写 SavedPath。</summary>
        private void SaveAttachments(HMailMessage message, string attachmentDir)
        {
            if (message.Attachments.Count == 0 || string.IsNullOrEmpty(attachmentDir))
            {
                return;
            }
            Directory.CreateDirectory(attachmentDir);
            foreach (HMailAttachment att in message.Attachments)
            {
                string safeName = MakeSafeFileName(att.FileName);
                string path = Path.Combine(attachmentDir, safeName);
                int seq = 1;
                while (File.Exists(path))
                {
                    string name = Path.GetFileNameWithoutExtension(safeName);
                    string ext = Path.GetExtension(safeName);
                    path = Path.Combine(attachmentDir, name + "_" + seq + ext);
                    seq++;
                }
                File.WriteAllBytes(path, att.Bytes);
                att.SavedPath = path;
            }
        }

        /// <summary>文件名净化（去路径分隔符，防目录穿越）。</summary>
        private static string MakeSafeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "attachment.bin";
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace('/', '_').Replace('\\', '_');
        }

        /// <summary>发送命令并读取单行 +OK 应答（参数为空时不带参数），返回应答文本。</summary>
        private string Command(string command, string argument)
        {
            WriteRaw(string.IsNullOrEmpty(argument) ? command : command + " " + argument);
            string reply = ReadSingleLine();
            if (!IsOk(reply))
            {
                throw new IOException("POP3 " + command + " failed: " + reply);
            }
            return reply;
        }

        /// <summary>写一行（CRLF）。</summary>
        private void WriteRaw(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text + Crlf);
            mStream.Write(bytes, 0, bytes.Length);
            mStream.Flush();
        }

        /// <summary>逐字节读一行（CRLF），返回去掉行尾的字符串。</summary>
        private string ReadSingleLine()
        {
            var bytes = new List<byte>(256);
            int value;
            while ((value = mStream.ReadByte()) >= 0)
            {
                if (value == '\r')
                {
                    int next = mStream.ReadByte();
                    if (next == '\n')
                    {
                        break;
                    }
                    bytes.Add((byte)'\r');
                    if (next >= 0)
                    {
                        bytes.Add((byte)next);
                    }
                }
                else if (value == '\n')
                {
                    break;
                }
                else
                {
                    bytes.Add((byte)value);
                }
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>读取多行数据块（RETR 响应体），以单独的 . 行结束，处理点字节填充。</summary>
        private byte[] ReadDataBlock()
        {
            using (var ms = new MemoryStream())
            {
                var line = new List<byte>(1024);
                int value;
                bool firstOfLine = true;
                while ((value = mStream.ReadByte()) >= 0)
                {
                    if (firstOfLine && value == '.')
                    {
                        int next = mStream.ReadByte();
                        if (next == '\r')
                        {
                            mStream.ReadByte(); // 吃掉 \n，数据块结束
                            break;
                        }
                        if (next == '\n')
                        {
                            break;
                        }
                        // 点填充：".." 表示行首一个真实点
                        line.Add((byte)'.');
                        if (next >= 0)
                        {
                            line.Add((byte)next);
                        }
                        firstOfLine = false;
                        continue;
                    }
                    if (value == '\n')
                    {
                        ms.Write(line.ToArray(), 0, line.Count);
                        ms.WriteByte((byte)'\r');
                        ms.WriteByte((byte)'\n');
                        line.Clear();
                        firstOfLine = true;
                    }
                    else if (value != '\r')
                    {
                        line.Add((byte)value);
                        firstOfLine = false;
                    }
                }
                if (line.Count > 0)
                {
                    ms.Write(line.ToArray(), 0, line.Count);
                }
                return ms.ToArray();
            }
        }

        /// <summary>POP3 成功应答以 +OK 开头。</summary>
        private static bool IsOk(string reply)
        {
            return reply != null && reply.StartsWith("+OK", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>登录状态检查。</summary>
        private bool CheckConnected()
        {
            if (!IsConnected || mStream == null)
            {
                StrError = "Not connected";
                return false;
            }
            return true;
        }

        /// <summary>证书校验默认全部接受（内网自签名场景）。</summary>
        private static bool ValidateCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>释放连接资源。</summary>
        public void Dispose()
        {
            Close();
        }
    }
}
