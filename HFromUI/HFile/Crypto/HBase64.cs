using System;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// Base64 编解码（可逆编码，非加密算法）。常用于把二进制密文/密钥转为可打印文本存储或传输。
    /// </summary>
    public static class HBase64
    {
        /// <summary>字节数组编码为 Base64 文本。</summary>
        /// <param name="data">字节数据。</param>
        /// <returns>Base64 文本。</returns>
        public static string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Convert.ToBase64String(data);
        }

        /// <summary>字符串先按编码转字节再 Base64 编码。</summary>
        /// <param name="text">文本。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>Base64 文本。</returns>
        public static string EncodeString(string text, Encoding encoding = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Convert.ToBase64String(encoding.GetBytes(text));
        }

        /// <summary>Base64 文本解码为字节数组。</summary>
        /// <param name="base64">Base64 文本。</param>
        /// <returns>字节数据。</returns>
        /// <exception cref="FormatException">文本不是合法 Base64 时抛出。</exception>
        public static byte[] Decode(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw new ArgumentNullException("base64");
            }

            return Convert.FromBase64String(base64);
        }

        /// <summary>Base64 文本解码为字符串。</summary>
        /// <param name="base64">Base64 文本。</param>
        /// <param name="encoding">目标文本编码，默认 UTF-8。</param>
        /// <returns>解码后的文本。</returns>
        public static string DecodeString(string base64, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return encoding.GetString(Decode(base64));
        }
    }
}
