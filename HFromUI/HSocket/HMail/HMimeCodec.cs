using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HFromUI.HSocket.HMail
{
    /// <summary>
    /// 精简 MIME 编解码工具（SMTP 发送构信 / POP3 收信解析共用）。
    /// 支持：RFC2047 头编码（=?charset?B/Q?..?=）、base64/quoted-printable/7bit 传输编码、
    /// multipart/mixed|alternative 递归解析、text/plain 与 text/html 正文、附件提取。
    /// </summary>
    internal static class HMimeCodec
    {
        private const string Crlf = "\r\n";

        // ==================== 发送构信 ====================

        /// <summary>将邮件内容构建为可直接投递的 RFC822/MIME 字节（UTF-8）。</summary>
        public static byte[] BuildMime(string from, IEnumerable<string> to, IEnumerable<string> cc,
            string subject, string body, bool isHtml, IEnumerable<string> attachmentPaths)
        {
            var sb = new StringBuilder();
            sb.Append("From: ").Append(EncodeAddress(from)).Append(Crlf);
            AppendAddressHeader(sb, "To", to);
            AppendAddressHeader(sb, "Cc", cc);
            sb.Append("Subject: ").Append(EncodeHeader(subject)).Append(Crlf);
            sb.Append("Date: ").Append(DateTime.UtcNow.ToString("r")).Append(Crlf);
            sb.Append("MIME-Version: 1.0").Append(Crlf);

            bool hasAttachment = attachmentPaths != null && HasAny(attachmentPaths);
            string boundary = "HMAIL-" + Guid.NewGuid().ToString("N");
            if (!hasAttachment)
            {
                string kind = isHtml ? "text/html" : "text/plain";
                sb.Append("Content-Type: " + kind + "; charset=utf-8").Append(Crlf);
                sb.Append("Content-Transfer-Encoding: base64").Append(Crlf);
                sb.Append(Crlf);
                sb.Append(ToBase64Lines(Encoding.UTF8.GetBytes(body ?? string.Empty)));
            }
            else
            {
                sb.Append("Content-Type: multipart/mixed; boundary=\"" + boundary + "\"").Append(Crlf);
                sb.Append(Crlf);
                sb.Append("This is a multi-part message in MIME format.").Append(Crlf);

                sb.Append("--" + boundary).Append(Crlf);
                string kind = isHtml ? "text/html" : "text/plain";
                sb.Append("Content-Type: " + kind + "; charset=utf-8").Append(Crlf);
                sb.Append("Content-Transfer-Encoding: base64").Append(Crlf);
                sb.Append(Crlf);
                sb.Append(ToBase64Lines(Encoding.UTF8.GetBytes(body ?? string.Empty)));
                sb.Append(Crlf);

                foreach (string path in attachmentPaths)
                {
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        continue;
                    }
                    string fileName = Path.GetFileName(path);
                    sb.Append("--" + boundary).Append(Crlf);
                    sb.Append("Content-Type: application/octet-stream; name=\"" + EncodeHeader(fileName) + "\"").Append(Crlf);
                    sb.Append("Content-Transfer-Encoding: base64").Append(Crlf);
                    sb.Append("Content-Disposition: attachment; filename=\"" + EncodeHeader(fileName) + "\"").Append(Crlf);
                    sb.Append(Crlf);
                    sb.Append(ToBase64Lines(File.ReadAllBytes(path)));
                    sb.Append(Crlf);
                }
                sb.Append("--" + boundary + "--").Append(Crlf);
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>邮箱地址直接放头（地址本身为 ASCII；显示名部分做 RFC2047 编码）。</summary>
        private static string EncodeAddress(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return string.Empty;
            }
            int lt = addr.IndexOf('<');
            if (lt > 0 && addr.TrimEnd().EndsWith(">"))
            {
                string display = addr.Substring(0, lt).Trim().Trim('"');
                string rest = addr.Substring(lt);
                return "\"" + EncodeHeader(display) + "\" " + rest;
            }
            return addr;
        }

        private static void AppendAddressHeader(StringBuilder sb, string name, IEnumerable<string> addrs)
        {
            if (addrs == null)
            {
                return;
            }
            string joined = string.Join(", ", ToList(addrs));
            if (!string.IsNullOrEmpty(joined))
            {
                sb.Append(name + ": ").Append(joined).Append(Crlf);
            }
        }

        private static bool HasAny(IEnumerable<string> items)
        {
            using (IEnumerator<string> e = items.GetEnumerator())
            {
                return e.MoveNext();
            }
        }

        private static List<string> ToList(IEnumerable<string> items)
        {
            var list = new List<string>();
            foreach (string item in items)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    list.Add(item.Trim());
                }
            }
            return list;
        }

        /// <summary>含非 ASCII 的头文本编码为 =?utf-8?B?..?=（每 75 字符一段，用空格连接）。</summary>
        public static string EncodeHeader(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            if (IsAscii(text))
            {
                return text;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            string encoded = Convert.ToBase64String(bytes);
            var parts = new List<string>();
            const int chunk = 57; // base64(57)=76，留足 =?utf-8?B? 前后缀
            for (int i = 0; i < encoded.Length; i += chunk)
            {
                parts.Add("=?utf-8?B?" + encoded.Substring(i, Math.Min(chunk, encoded.Length - i)) + "?=");
            }
            return string.Join("\r\n ", parts);
        }

        private static bool IsAscii(string text)
        {
            foreach (char c in text)
            {
                if (c > 127)
                {
                    return false;
                }
            }
            return true;
        }

        private static string ToBase64Lines(byte[] bytes)
        {
            string encoded = Convert.ToBase64String(bytes);
            var sb = new StringBuilder();
            for (int i = 0; i < encoded.Length; i += 76)
            {
                sb.Append(encoded.Substring(i, Math.Min(76, encoded.Length - i))).Append(Crlf);
            }
            return sb.ToString();
        }

        // ==================== 收信解析 ====================

        /// <summary>解析一封原始 RFC822 邮件（RETR 得到的字节）。</summary>
        public static HMailMessage Parse(byte[] raw)
        {
            var message = new HMailMessage();
            var root = new Part();
            ParseEntity(raw, root);
            message.From = root.From;
            message.To = root.To;
            message.Cc = root.Cc;
            message.Subject = root.Subject;
            FillMessage(root, message);
            return message;
        }

        /// <summary>MIME 实体（叶子为正文/附件，multipart 含子实体）。</summary>
        private class Part
        {
            public string From;
            public string To;
            public string Cc;
            public string Subject;
            public string MediaType = "text/plain";
            public string Charset = "utf-8";
            public string ContentName;
            public string Disposition;
            public string ContentId;
            public string ContentTypeRaw;
            public string TransferEncoding = "7bit";
            public byte[] RawBody = new byte[0];
            public readonly List<Part> Children = new List<Part>();
        }

        /// <summary>填充邮件对象：递归收集正文与附件。</summary>
        private static void FillMessage(Part part, HMailMessage message)
        {
            if (part.Children.Count > 0)
            {
                foreach (Part child in part.Children)
                {
                    FillMessage(child, message);
                }
                return;
            }
            bool isText = part.MediaType.IndexOf("text", StringComparison.OrdinalIgnoreCase) >= 0;
            bool named = !string.IsNullOrEmpty(part.ContentName);
            bool isAttachment = string.Equals(part.Disposition, "attachment", StringComparison.OrdinalIgnoreCase)
                || string.Equals(part.Disposition, "inline", StringComparison.OrdinalIgnoreCase)
                || (named && !isText);
            if (isAttachment && named)
            {
                byte[] data = DecodeBody(part.RawBody, part.TransferEncoding);
                message.Attachments.Add(new HMailAttachment
                {
                    FileName = DecodeHeader(part.ContentName) ?? ("attachment_" + (message.Attachments.Count + 1)),
                    Bytes = data,
                    Size = data.Length
                });
                return;
            }
            if (!isText)
            {
                // 无名的非文本叶子（少见），按附件兜底保存
                byte[] data = DecodeBody(part.RawBody, part.TransferEncoding);
                message.Attachments.Add(new HMailAttachment
                {
                    FileName = "attachment_" + (message.Attachments.Count + 1) + ".bin",
                    Bytes = data,
                    Size = data.Length
                });
                return;
            }
            string text = EncodingFromName(part.Charset).GetString(DecodeBody(part.RawBody, part.TransferEncoding));
            text = NormalizeLineEnding(text);
            if (part.MediaType.IndexOf("html", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (message.HtmlBody == null)
                {
                    message.HtmlBody = text;
                }
            }
            else if (message.TextBody == null)
            {
                message.TextBody = text;
            }
        }

        /// <summary>递归解析实体：拆头/体，multipart 按 boundary 切子实体。</summary>
        private static void ParseEntity(byte[] raw, Part part)
        {
            int split = IndexOfBytes(raw, Encoding.ASCII.GetBytes("\r\n\r\n"), 0);
            if (split < 0)
            {
                part.RawBody = raw;
                return;
            }
            string headerText = Encoding.ASCII.GetString(raw, 0, split).Replace("\r\n ", " ").Replace("\r\n\t", " ");
            byte[] body = new byte[raw.Length - split - 4];
            Buffer.BlockCopy(raw, split + 4, body, 0, body.Length);

            foreach (string line in headerText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = line.IndexOf(':');
                if (colon <= 0)
                {
                    continue;
                }
                string key = line.Substring(0, colon).Trim().ToLowerInvariant();
                string value = line.Substring(colon + 1).Trim();
                ApplyHeader(part, key, value);
            }

            if (part.MediaType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
            {
                string boundary = ExtractParameter(part.ContentTypeRaw, "boundary");
                if (!string.IsNullOrEmpty(boundary))
                {
                    SplitMultipart(body, boundary, part);
                }
            }
            else
            {
                part.RawBody = body;
            }
        }

        /// <summary>临时保存原始 Content-Type 头以便提取 boundary。</summary>
        private static void ApplyHeader(Part part, string key, string value)
        {
            switch (key)
            {
                case "from":
                    part.From = DecodeHeader(value);
                    break;
                case "to":
                    part.To = DecodeHeader(value);
                    break;
                case "cc":
                    part.Cc = DecodeHeader(value);
                    break;
                case "subject":
                    part.Subject = DecodeHeader(value);
                    break;
                case "content-type":
                    part.ContentTypeRaw = value;
                    part.MediaType = value.Split(';')[0].Trim();
                    string charset = ExtractParameter(value, "charset");
                    if (!string.IsNullOrEmpty(charset))
                    {
                        part.Charset = charset.Trim('"');
                    }
                    string name = ExtractParameter(value, "name");
                    if (!string.IsNullOrEmpty(name))
                    {
                        part.ContentName = name;
                    }
                    break;
                case "content-transfer-encoding":
                    part.TransferEncoding = value.Trim();
                    break;
                case "content-disposition":
                    string disp = value.Split(';')[0].Trim();
                    part.Disposition = disp;
                    string fileName = ExtractParameter(value, "filename");
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        part.ContentName = fileName;
                    }
                    break;
                case "content-id":
                    part.ContentId = value.Trim('>', '<');
                    break;
            }
        }

        /// <summary>按 --boundary 切割 multipart 体并递归解析。</summary>
        private static void SplitMultipart(byte[] body, string boundary, Part parent)
        {
            byte[] delimiter = Encoding.ASCII.GetBytes("\r\n--" + boundary);
            var sections = new List<byte[]>();
            int pos = IndexOfBytes(body, Encoding.ASCII.GetBytes("--" + boundary), 0);
            if (pos < 0)
            {
                return;
            }
            pos += boundary.Length + 2;
            while (true)
            {
                int next = IndexOfBytes(body, delimiter, pos);
                if (next < 0)
                {
                    break;
                }
                int start = pos;
                if (body[start] == '\r')
                {
                    start += 2;
                }
                else if (body[start] == '\n')
                {
                    start += 1;
                }
                int len = next - start;
                if (len > 0)
                {
                    var section = new byte[len];
                    Buffer.BlockCopy(body, start, section, 0, len);
                    sections.Add(section);
                }
                pos = next + delimiter.Length;
                if (pos + 1 < body.Length && body[pos] == '-' && body[pos + 1] == '-')
                {
                    break;
                }
            }
            foreach (byte[] section in sections)
            {
                var child = new Part();
                ParseEntity(section, child);
                parent.Children.Add(child);
            }
        }

        /// <summary>从头参数值中提取指定参数（charset/boundary/name/filename），值可能带引号或 RFC2047 编码。</summary>
        private static string ExtractParameter(string headerValue, string name)
        {
            if (string.IsNullOrEmpty(headerValue))
            {
                return null;
            }
            Match match = Regex.Match(headerValue, name + @"\s*=\s*(""?)([^"";]+)\1",
                RegexOptions.IgnoreCase);
            return match.Success ? DecodeHeader(match.Groups[2].Value.Trim()) : null;
        }

        /// <summary>按传输编码还原字节。</summary>
        public static byte[] DecodeBody(byte[] raw, string transferEncoding)
        {
            if (transferEncoding == null)
            {
                return raw;
            }
            string enc = transferEncoding.Trim().ToLowerInvariant();
            if (enc == "base64")
            {
                string text = Encoding.ASCII.GetString(raw).Replace("\r", "").Replace("\n", "").Replace(" ", "");
                try
                {
                    return Convert.FromBase64String(text);
                }
                catch
                {
                    return raw;
                }
            }
            if (enc == "quoted-printable")
            {
                return DecodeQuotedPrintable(raw);
            }
            return raw; // 7bit / 8bit / binary
        }

        /// <summary>quoted-printable 解码。</summary>
        private static byte[] DecodeQuotedPrintable(byte[] raw)
        {
            string text = Encoding.ASCII.GetString(raw);
            text = text.Replace("=\r\n", "").Replace("=\n", "");
            var output = new List<byte>(raw.Length);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '=' && i + 2 < text.Length && IsHex(text[i + 1]) && IsHex(text[i + 2]))
                {
                    output.Add(Convert.ToByte(text.Substring(i + 1, 2), 16));
                    i += 2;
                }
                else
                {
                    output.Add((byte)text[i]);
                }
            }
            return output.ToArray();
        }

        private static bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }

        /// <summary>解码 RFC2047 头（含混合的普通文本与多个编码词）。</summary>
        public static string DecodeHeader(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf("=?") < 0)
            {
                return text;
            }
            var sb = new StringBuilder();
            int index = 0;
            MatchCollection matches = Regex.Matches(text, @"=\?([^?]+)\?([BbQq])\?([^?]*)\?=");
            foreach (Match match in matches)
            {
                if (match.Index > index)
                {
                    sb.Append(text.Substring(index, match.Index - index));
                }
                string charset = match.Groups[1].Value;
                bool isBase64 = match.Groups[2].Value.Equals("B", StringComparison.OrdinalIgnoreCase);
                string data = match.Groups[3].Value;
                byte[] bytes;
                if (isBase64)
                {
                    bytes = Convert.FromBase64String(data);
                }
                else
                {
                    bytes = DecodeQEncoding(data);
                }
                sb.Append(EncodingFromName(charset).GetString(bytes));
                index = match.Index + match.Length;
            }
            if (index < text.Length)
            {
                sb.Append(text.Substring(index));
            }
            return sb.ToString().Replace("\r\n", "").Replace("\t", " ").Trim();
        }

        /// <summary>Q 编码（头中的 =XX 与 _ 表示空格）。</summary>
        private static byte[] DecodeQEncoding(string data)
        {
            var bytes = new List<byte>(data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '_')
                {
                    bytes.Add((byte)' ');
                }
                else if (data[i] == '=' && i + 2 < data.Length)
                {
                    bytes.Add(Convert.ToByte(data.Substring(i + 1, 2), 16));
                    i += 2;
                }
                else
                {
                    bytes.Add((byte)data[i]);
                }
            }
            return bytes.ToArray();
        }

        private static Encoding EncodingFromName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Encoding.UTF8;
            }
            try
            {
                return Encoding.GetEncoding(name.Trim().Trim('"'));
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        private static string NormalizeLineEnding(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n").TrimEnd('\r', '\n');
        }

        /// <summary>在字节数组中查找子序列（模拟内存流 IndexOf）。</summary>
        private static int IndexOfBytes(byte[] haystack, byte[] needle, int start)
        {
            if (needle.Length == 0 || haystack.Length < needle.Length)
            {
                return -1;
            }
            for (int i = start; i <= haystack.Length - needle.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
