using HFromUI.HBase;
using HFromUI.HEnum;

namespace HFromUI.HSocket.DeviceID
{
    /// <summary>
    /// 邮件服务器连接地址（DeviceID 体系）：SMTP 发信 + POP3 收信合一。
    /// <para>发信：SmtpHost/SmtpPort，SmtpSsl=true 走隐式 SSL（465），
    /// SmtpStartTls=true 走明文连接后升级 TLS（587/25），两者皆否则明文。</para>
    /// <para>收信：Pop3Host/Pop3Port，Pop3Ssl=true 走隐式 SSL（995），否则明文 110。</para>
    /// </summary>
    public class HMailAddress : HDataBase
    {
        /// <summary>SMTP 发信服务器主机，如 smtp.qq.com。</summary>
        public string SmtpHost { set; get; }

        /// <summary>SMTP 端口：465 隐式 SSL / 587 STARTTLS / 25 明文，默认 25。</summary>
        public int SmtpPort { set; get; } = 25;

        /// <summary>SMTP 是否隐式 SSL 直连（端口 465）。</summary>
        public bool SmtpSsl { set; get; }

        /// <summary>SMTP 是否在明文连接后发送 STARTTLS 升级（端口 587）。</summary>
        public bool SmtpStartTls { set; get; }

        /// <summary>POP3 收信服务器主机，如 pop.qq.com。</summary>
        public string Pop3Host { set; get; }

        /// <summary>POP3 端口：995 隐式 SSL / 110 明文，默认 110。</summary>
        public int Pop3Port { set; get; } = 110;

        /// <summary>POP3 是否隐式 SSL 直连（端口 995）。</summary>
        public bool Pop3Ssl { set; get; }

        /// <summary>登录账号（通常为完整邮箱地址）。</summary>
        public string UserName { set; get; }

        /// <summary>登录密码或授权码（多数服务商需用客户端授权码而非登录密码）。</summary>
        public string Password { set; get; }

        /// <summary>发件人邮箱地址（为空时回退 UserName）。</summary>
        public string FromAddress { set; get; }

        /// <summary>发件人显示名（收件人看到的友好名称）。</summary>
        public string DisplayName { set; get; }

        /// <summary>是否需要 AUTH 登录（默认 true；仅内网匿名中继可关闭）。</summary>
        public bool RequiresAuth { set; get; } = true;

        /// <summary>通讯类型（SMTP_Client 或 POP3_Client，按用途赋值）。</summary>
        public HSocketType SocketType { set; get; } = HSocketType.SMTP_Client;
    }
}
