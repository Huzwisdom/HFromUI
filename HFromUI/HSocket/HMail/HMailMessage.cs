using System.Collections.Generic;

namespace HFromUI.HSocket.HMail
{
    /// <summary>
    /// 邮件附件（收信时附件已落盘，同时保留字节便于直接使用）。
    /// </summary>
    public class HMailAttachment
    {
        /// <summary>附件文件名（已解码）。</summary>
        public string FileName { set; get; }

        /// <summary>附件保存到本地的完整路径（未落盘为 null）。</summary>
        public string SavedPath { set; get; }

        /// <summary>附件原始字节。</summary>
        public byte[] Bytes { set; get; }

        /// <summary>附件大小（字节）。</summary>
        public long Size { set; get; }
    }

    /// <summary>
    /// 一封邮件的解析结果 / 待发送内容（SMTP 发送与 POP3 接收共用）。
    /// </summary>
    public class HMailMessage
    {
        /// <summary>发件人显示与地址（如 "张三" &lt;a@b.com&gt;；发送时填地址即可）。</summary>
        public string From { set; get; }

        /// <summary>收件人地址（多个用分号或逗号分隔）。</summary>
        public string To { set; get; }

        /// <summary>抄送地址（多个分隔，可空）。</summary>
        public string Cc { set; get; }

        /// <summary>密送地址（仅发送时使用，不写入邮件头）。</summary>
        public string Bcc { set; get; }

        /// <summary>主题（已解码）。</summary>
        public string Subject { set; get; }

        /// <summary>纯文本正文（无纯文本时为 null）。</summary>
        public string TextBody { set; get; }

        /// <summary>HTML 正文（无 HTML 时为 null）。</summary>
        public string HtmlBody { set; get; }

        /// <summary>附件列表（无附件为空列表）。</summary>
        public List<HMailAttachment> Attachments { set; get; } = new List<HMailAttachment>();

        /// <summary>发送时正文是否按 HTML 处理（true 用 HtmlBody/正文作为 text/html）。</summary>
        public bool IsBodyHtml { set; get; }

        /// <summary>POP3 邮件序号（从 1 开始；仅接收结果有值）。</summary>
        public int SequenceNumber { set; get; }
    }
}
