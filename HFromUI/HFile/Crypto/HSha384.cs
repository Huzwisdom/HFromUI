using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// SHA-384 单向散列（384 位摘要，SHA-2 家族）。只能计算摘要，不可解密还原。
    /// 安全强度高于 SHA-256，摘要更长，适用于对安全余量要求更高的完整性校验与签名场景。
    /// </summary>
    public static class HSha384
    {
        /// <summary>计算字符串的 SHA-384 摘要，输出小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>96 位小写十六进制摘要。</returns>
        public static string Hash(string text, Encoding encoding = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return HHex.Encode(Hash(encoding.GetBytes(text)));
        }

        /// <summary>计算字节数组的 SHA-384 摘要。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>48 字节摘要。</returns>
        public static byte[] Hash(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            using (SHA384 sha = SHA384.Create())
            {
                return sha.ComputeHash(data);
            }
        }

        /// <summary>流式计算文件的 SHA-384 摘要，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>96 位小写十六进制摘要。</returns>
        public static string HashFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (FileStream fs = File.OpenRead(filePath))
            using (SHA384 sha = SHA384.Create())
            {
                return HHex.Encode(sha.ComputeHash(fs));
            }
        }

        /// <summary>恒定时间校验字符串摘要是否匹配。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expectedHex">期望的十六进制摘要。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>匹配返回 true。</returns>
        public static bool Verify(string text, string expectedHex, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(expectedHex))
            {
                return false;
            }

            return HCryptoCore.FixedTimeEquals(Hash(text, encoding), expectedHex.ToLowerInvariant());
        }

        /// <summary>恒定时间校验字节数组摘要是否匹配。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expectedHex">期望的十六进制摘要。</param>
        /// <returns>匹配返回 true。</returns>
        public static bool Verify(byte[] data, string expectedHex)
        {
            if (string.IsNullOrEmpty(expectedHex))
            {
                return false;
            }

            return HCryptoCore.FixedTimeEquals(HHex.Encode(Hash(data)), expectedHex.ToLowerInvariant());
        }
    }
}
