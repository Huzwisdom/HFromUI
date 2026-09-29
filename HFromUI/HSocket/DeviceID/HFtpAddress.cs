using HFromUI.HBase;
using HFromUI.HEnum;

namespace HFromUI.HSocket.DeviceID
{
    /// <summary>
    /// FTP/FTPS 服务器连接地址（DeviceID 体系）。
    /// 用于 HFtpClient：主机 + 端口 + 账号 + 远程根目录，FTPS 显式 TLS 由 EnableSsl 控制。
    /// </summary>
    public class HFtpAddress : HDataBase
    {
        /// <summary>服务器主机名或 IP，如 ftp.example.com / 192.168.1.10。</summary>
        public string Host { set; get; }

        /// <summary>FTP 端口，默认 21（FTPS 显式 TLS 通常仍是 21/990）。</summary>
        public int Port { set; get; } = 21;

        /// <summary>登录用户名（匿名登录用 anonymous）。</summary>
        public string UserName { set; get; }

        /// <summary>登录密码（匿名可填邮箱或空）。</summary>
        public string Password { set; get; }

        /// <summary>是否启用 FTPS（FtpWebRequest.EnableSsl，AUTH TLS 加密控制/数据连接）。</summary>
        public bool EnableSsl { set; get; }

        /// <summary>是否被动模式（PASV，客户端侧通常必须 true）。</summary>
        public bool UsePassive { set; get; } = true;

        /// <summary>远程根目录，如 /upload（相对路径基于登录后的当前工作目录）。</summary>
        public string RemoteBaseDir { set; get; } = "/";

        /// <summary>通讯类型（固定为 FTP_Client）。</summary>
        public HSocketType SocketType { set; get; } = HSocketType.FTP_Client;
    }
}
