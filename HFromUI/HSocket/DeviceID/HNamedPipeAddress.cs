using HFromUI.HBase;
using HFromUI.HEnum;

namespace HFromUI.HSocket.DeviceID
{
    /// <summary>
    /// 命名管道连接地址（DeviceID 体系）：命名管道是 Windows 内核提供的原生 IPC 机制，
    /// 由内核维护、以全局唯一名称（\\.\pipe\管道名）标识通信通道，不经过网络栈，
    /// 可在同一台机器的不同进程之间进行全双工（PipeDirection.InOut）双向数据传输。
    /// <para>同会话内进程通信直接用普通管道名；跨 Windows 会话（如服务进程与桌面进程）
    /// 需在管道名前加 "Global\" 前缀，且对端进程需具备相应权限。</para>
    /// </summary>
    public class HNamedPipeAddress : HDataBase
    {
        /// <summary>
        /// 管道名称（全局唯一标识）：可直接写短名（如 MY_PIPE，实际对应 \\.\pipe\MY_PIPE），
        /// 也可写 "Global\MY_PIPE" 跨会话；误写完整 \\.\pipe\ 路径会自动剥离前缀。
        /// </summary>
        public string PipeName { set; get; }

        /// <summary>
        /// 管道所在计算机名（仅客户端使用）："." 为本机（默认），
        /// 远程机器需对端开放 SMB/管道共享与相应权限。
        /// </summary>
        public string ServerName { set; get; } = ".";

        /// <summary>服务端最大并发实例数（同一管道名可同时接入的客户端数），默认 10。</summary>
        public int MaxServerInstances { set; get; } = 10;

        /// <summary>读写缓冲区大小（字节），默认 64KB。</summary>
        public int BufferSize { set; get; } = 64 * 1024;

        /// <summary>客户端连接等待超时（毫秒），默认 5 秒；&lt;=0 表示无限等待。</summary>
        public int ConnectTimeout { set; get; } = 5000;

        /// <summary>站点名称（多客户端时的业务标识）。</summary>
        public string StationName { set; get; }

        /// <summary>站点编号。</summary>
        public int StationID { set; get; }

        /// <summary>通讯类型（NamedPipe_Client / NamedPipe_Server 角色）。</summary>
        public HSocketType SocketType { set; get; } = HSocketType.NamedPipe_Client;

        /// <summary>管道完整内核路径（显示/诊断用），形如 \\.\pipe\MY_PIPE。</summary>
        public string FullPipeName
        {
            get { return @"\\.\pipe\" + GetShortName(); }
        }

        /// <summary>
        /// 获取可直接传给 NamedPipeClientStream/NamedPipeServerStream 构造函数的短名：
        /// 去掉用户可能误写的 \\.\pipe\ 或 \\server\pipe\ 前缀，保留 Global\ / Local\ 前缀。
        /// </summary>
        /// <returns>管道短名；未设置返回空字符串</returns>
        public string GetShortName()
        {
            if (string.IsNullOrWhiteSpace(PipeName))
            {
                return string.Empty;
            }
            string name = PipeName.Trim().Replace('/', '\\').TrimStart('\\');
            string lower = name.ToLowerInvariant();
            if (lower.StartsWith(@".\pipe\", System.StringComparison.Ordinal))
            {
                name = name.Substring(@".\pipe\".Length);
            }
            else
            {
                // \\<server>\pipe\<name> 形式：取 pipe\ 之后的部分
                int pipeIndex = lower.IndexOf(@"\pipe\", System.StringComparison.Ordinal);
                if (pipeIndex > 0)
                {
                    name = name.Substring(pipeIndex + @"\pipe\".Length);
                }
            }
            return name.TrimStart('\\');
        }
    }
}
