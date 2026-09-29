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
    /// SMTP 发信客户端（以 HMailAddress 作为连接地址）：手写 SMTP 对话，
    /// 同时支持 465 隐式 SSL、587 STARTTLS 与 25 明文，AUTH LOGIN 认证，
    /// 支持 HTML/纯文本正文与多附件（MIME 由 HMimeCodec 构建）。
    /// 同步发送为阻塞调用，另提供 Async 版本；服务器原始应答会保留在 StrError 中便于排查 535 等错误。
    /// </summary>
    public class HMailClient : IDisposable
    {
        private const string Crlf = "\r\n";

        private TcpClient mClient;
        private Stream mStream;
        private StreamReader mReader;

        /// <summary>邮件服务器连接地址（SMTP 部分生效）。</summary>
        public HMailAddress hMailAddress { set; get; }

        /// <summary>连接/读写超时（毫秒），默认 15 秒。</summary>
        public int Timeout { set; get; } = 15000;

        /// <summary>报错提示（含服务器原始应答码与文本）。</summary>
        public string StrError { get; private set; }

        /// <summary>是否已与 SMTP 服务器完成会话建立。</summary>
        public bool IsConnected { get; private set; }

        /// <summary>以 HMailAddress 构造。</summary>
        /// <param name="address">邮件服务器地址</param>
        public HMailClient(HMailAddress address)
        {
            hMailAddress = address ?? new HMailAddress();
            hMailAddress.SocketType = HSocketType.SMTP_Client;
        }

        /// <summary>
        /// 发送邮件（完整地址列表，抄送行写入邮件头，密送仅投递不写头）。
        /// </summary>
        /// <param name="to">收件人地址（多个用逗号/分号分隔）</param>
        /// <param name="subject">主题</param>
        /// <param name="body">正文</param>
        /// <param name="attachments">附件本地文件路径（可空）</param>
        /// <param name="isBodyHtml">正文是否为 HTML</param>
        /// <param name="cc">抄送地址（可空）</param>
        /// <param name="bcc">密送地址（可空）</param>
        /// <returns>是否被服务器接受（250）</returns>
        public bool Send(string to, string subject, string body,
            IEnumerable<string> attachments = null, bool isBodyHtml = false,
            string cc = null, string bcc = null)
        {
            var message = new HMailMessage
            {
                To = to,
                Cc = cc,
                Bcc = bcc,
                Subject = subject,
                IsBodyHtml = isBodyHtml
            };
            if (isBodyHtml)
            {
                message.HtmlBody = body;
            }
            else
            {
                message.TextBody = body;
            }
            return Send(message, attachments);
        }

        /// <summary>
        /// 发送 HMailMessage（附件取 message.Attachments 中的字节或 attachments 路径，二选一或同时使用）。
        /// </summary>
        public bool Send(HMailMessage message, IEnumerable<string> attachmentPaths = null)
        {
            if (message == null)
            {
                StrError = "Message is null";
                return false;
            }
            try
            {
                ConnectSession();

                List<string> toList = SplitAddresses(message.To);
                List<string> ccList = SplitAddresses(message.Cc);
                List<string> bccList = SplitAddresses(message.Bcc);
                if (toList.Count + ccList.Count + bccList.Count == 0)
                {
                    StrError = "No recipient";
                    return false;
                }

                string from = string.IsNullOrWhiteSpace(hMailAddress.FromAddress)
                    ? hMailAddress.UserName
                    : hMailAddress.FromAddress;
                string body = message.IsBodyHtml ? (message.HtmlBody ?? string.Empty)
                    : (message.TextBody ?? string.Empty);

                // 附件：路径文件 + 消息自带字节附件（落临时文件统一构信）
                var paths = new List<string>();
                if (attachmentPaths != null)
                {
                    paths.AddRange(attachmentPaths);
                }
                var tempFiles = new List<string>();
                foreach (HMailAttachment att in message.Attachments)
                {
                    if (att.Bytes == null)
                    {
                        continue;
                    }
                    string temp = Path.Combine(Path.GetTempPath(),
                        "hmail_" + Guid.NewGuid().ToString("N") + "_" + (att.FileName ?? "attachment.bin"));
                    File.WriteAllBytes(temp, att.Bytes);
                    tempFiles.Add(temp);
                    paths.Add(temp);
                }
                try
                {
                    byte[] mime = HMimeCodec.BuildMime(
                        string.IsNullOrWhiteSpace(hMailAddress.DisplayName) ? from
                            : "\"" + hMailAddress.DisplayName + "\" <" + from + ">",
                        toList, ccList, message.Subject, body, message.IsBodyHtml, paths);

                    WriteCommand("MAIL FROM:<" + from + ">", "250");
                    foreach (string recipient in toList)
                    {
                        WriteCommand("RCPT TO:<" + recipient + ">", "250", "251");
                    }
                    foreach (string recipient in ccList)
                    {
                        WriteCommand("RCPT TO:<" + recipient + ">", "250", "251");
                    }
                    foreach (string recipient in bccList)
                    {
                        WriteCommand("RCPT TO:<" + recipient + ">", "250", "251");
                    }
                    WriteCommand("DATA", "354");
                    SendRawData(mime);
                    Expect("250");
                }
                finally
                {
                    foreach (string temp in tempFiles)
                    {
                        try { File.Delete(temp); } catch { /* 临时文件清理尽力而为 */ }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
                return false;
            }
            finally
            {
                QuitAndClose();
            }
        }

        /// <summary>异步发送。</summary>
        public Task<bool> SendAsync(string to, string subject, string body,
            IEnumerable<string> attachments = null, bool isBodyHtml = false,
            string cc = null, string bcc = null)
        {
            return Task.Run(() => Send(to, subject, body, attachments, isBodyHtml, cc, bcc));
        }

        /// <summary>
        /// 清空接收缓存（丢弃服务器尚未读取的应答字节），返回丢弃字节数。
        /// 每封邮件发完会正常 QUIT 关闭连接，通常无需调用；长连接异常重连前可用于复位。
        /// </summary>
        public int ClearCache()
        {
            int count = 0;
            try
            {
                if (mClient != null && mClient.Connected && mClient.Client.Available > 0)
                {
                    byte[] buffer = new byte[mClient.Client.Available];
                    count = mStream == null ? 0 : mStream.Read(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                // 清缓存为尽力操作
            }
            return Math.Max(0, count);
        }

        /// <summary>建立 TCP/(START)TLS 连接并完成 EHLO/AUTH 握手。</summary>
        private void ConnectSession()
        {
            int port = hMailAddress.SmtpPort <= 0 ? 25 : hMailAddress.SmtpPort;
            mClient = new TcpClient();
            var connectTask = mClient.ConnectAsync(hMailAddress.SmtpHost, port);
            if (!connectTask.Wait(Timeout))
            {
                throw new IOException("Connect SMTP timeout " + hMailAddress.SmtpHost + ":" + port);
            }
            mClient.ReceiveTimeout = Timeout;
            mClient.SendTimeout = Timeout;
            NetworkStream network = mClient.GetStream();
            if (hMailAddress.SmtpSsl)
            {
                var ssl = new SslStream(network, false, ValidateCertificate, null);
                ssl.AuthenticateAsClient(hMailAddress.SmtpHost);
                mStream = ssl;
            }
            else
            {
                mStream = network;
            }
            mReader = new StreamReader(mStream, Encoding.ASCII, false, 1024, leaveOpen: true);

            Expect("220");
            WriteRaw("EHLO " + Environment.MachineName);
            string ehlo = ReadReply();
            EnsureCode(ehlo, "250");

            if (!hMailAddress.SmtpSsl && hMailAddress.SmtpStartTls)
            {
                WriteCommand("STARTTLS", "220");
                var ssl = new SslStream(network, false, ValidateCertificate, null);
                ssl.AuthenticateAsClient(hMailAddress.SmtpHost);
                mStream = ssl;
                mReader = new StreamReader(mStream, Encoding.ASCII, false, 1024, leaveOpen: true);
                WriteRaw("EHLO " + Environment.MachineName);
                EnsureCode(ReadReply(), "250");
            }

            if (hMailAddress.RequiresAuth)
            {
                WriteRaw("AUTH LOGIN");
                EnsureCode(ReadReply(), "334");
                WriteRaw(Convert.ToBase64String(Encoding.UTF8.GetBytes(hMailAddress.UserName ?? string.Empty)));
                EnsureCode(ReadReply(), "334");
                WriteRaw(Convert.ToBase64String(Encoding.UTF8.GetBytes(hMailAddress.Password ?? string.Empty)));
                EnsureCode(ReadReply(), "235");
            }
            IsConnected = true;
        }

        /// <summary>发送命令并校验应答码（任一期望码通过），返回完整应答。</summary>
        private string WriteCommand(string command, params string[] expectedCodes)
        {
            WriteRaw(command);
            string reply = ReadReply();
            EnsureCode(reply, expectedCodes);
            return reply;
        }

        /// <summary>仅等待期望应答。</summary>
        private void Expect(params string[] expectedCodes)
        {
            EnsureCode(ReadReply(), expectedCodes);
        }

        /// <summary>写一行命令（CRLF）。</summary>
        private void WriteRaw(string text)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(text + Crlf);
            mStream.Write(bytes, 0, bytes.Length);
            mStream.Flush();
        }

        /// <summary>发送 MIME 数据（做 SMTP 点字节填充）。</summary>
        private void SendRawData(byte[] mime)
        {
            string text = Encoding.UTF8.GetString(mime).Replace("\r\n", "\n").Replace("\r", "\n");
            var output = new StringBuilder();
            foreach (string line in text.Split('\n'))
            {
                if (line.StartsWith("."))
                {
                    output.Append('.');
                }
                output.Append(line).Append(Crlf);
            }
            output.Append("." + Crlf);
            byte[] bytes = Encoding.UTF8.GetBytes(output.ToString());
            mStream.Write(bytes, 0, bytes.Length);
            mStream.Flush();
        }

        /// <summary>读取完整应答（处理多行：续行形如 250-xxx，结束行为 250 空格 text）。</summary>
        private string ReadReply()
        {
            string line;
            string last = null;
            while ((line = mReader.ReadLine()) != null)
            {
                last = line;
                if (line.Length >= 4 && line[3] == ' ')
                {
                    return line;
                }
                // 续行继续读
            }
            return last ?? string.Empty;
        }

        /// <summary>应答码不匹配时抛异常并保留服务器原始文本。</summary>
        private static void EnsureCode(string reply, params string[] expectedCodes)
        {
            string code = reply.Length >= 3 ? reply.Substring(0, 3) : string.Empty;
            foreach (string expected in expectedCodes)
            {
                if (code == expected)
                {
                    return;
                }
            }
            throw new IOException("SMTP error: expect " + string.Join("/", expectedCodes) + ", got " + reply);
        }

        /// <summary>拆分逗号/分号分隔的邮箱地址（去除显示名尖括号外的空白）。</summary>
        private static List<string> SplitAddresses(string text)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return list;
            }
            foreach (string part in text.Split(new[] { ',', ';', '，', '；' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string addr = part.Trim();
                int lt = addr.IndexOf('<');
                if (lt >= 0 && addr.EndsWith(">"))
                {
                    addr = addr.Substring(lt + 1, addr.Length - lt - 2);
                }
                if (!string.IsNullOrWhiteSpace(addr))
                {
                    list.Add(addr);
                }
            }
            return list;
        }

        /// <summary>证书校验：默认全部接受（内网自签名/工业场景），如需严格校验可在此扩展。</summary>
        private static bool ValidateCertificate(object sender, X509Certificate certificate,
            X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>QUIT 礼貌关闭（异常吞掉，避免掩盖主流程结果）。</summary>
        private void QuitAndClose()
        {
            try
            {
                if (mStream != null && IsConnected)
                {
                    WriteRaw("QUIT");
                    ReadReply();
                }
            }
            catch
            {
                // 关闭阶段忽略
            }
            finally
            {
                IsConnected = false;
                try { mReader?.Dispose(); } catch { /* 忽略 */ }
                try { mStream?.Close(); } catch { /* 忽略 */ }
                try { mClient?.Close(); } catch { /* 忽略 */ }
                mStream = null;
                mClient = null;
                mReader = null;
            }
        }

        /// <summary>释放连接资源。</summary>
        public void Dispose()
        {
            QuitAndClose();
        }
    }
}
