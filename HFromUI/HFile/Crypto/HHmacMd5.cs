using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// HMAC-MD5 带密钥单向散列（基于 MD5 的消息认证码）。只能计算认证码，不可解密还原。
    /// 与普通 <see cref="HMd5"/> 的区别：没有密钥的人无法伪造/校验认证码，可用于接口签名、防篡改。
    /// 注：MD5 抗碰撞较弱，新系统优先使用 <see cref="HHmacSha256"/>。
    /// </summary>
    public static class HHmacMd5
    {
        /// <summary>计算字符串的 HMAC-MD5，输出小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="key">密钥字符串（按 UTF-8 取字节）。</param>
        /// <param name="encoding">原文编码，默认 UTF-8。</param>
        /// <returns>32 位小写十六进制认证码。</returns>
        public static string Hash(string text, string key, Encoding encoding = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return HHex.Encode(Hash(encoding.GetBytes(text), encoding.GetBytes(key)));
        }

        /// <summary>计算字节数组的 HMAC-MD5。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="key">密钥字节，不可为空。</param>
        /// <returns>16 字节认证码。</returns>
        public static byte[] Hash(byte[] data, byte[] key)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            using (var hmac = new HMACMD5(key))
            {
                return hmac.ComputeHash(data);
            }
        }

        /// <summary>流式计算文件的 HMAC-MD5，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="key">密钥字节。</param>
        /// <returns>32 位小写十六进制认证码。</returns>
        public static string HashFile(string filePath, byte[] key)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            using (FileStream fs = File.OpenRead(filePath))
            using (var hmac = new HMACMD5(key))
            {
                return HHex.Encode(hmac.ComputeHash(fs));
            }
        }

        /// <summary>恒定时间校验字符串认证码是否匹配。</summary>
        /// <param name="text">原文。</param>
        /// <param name="key">密钥字符串。</param>
        /// <param name="expectedHex">期望的十六进制认证码。</param>
        /// <param name="encoding">原文编码，默认 UTF-8。</param>
        /// <returns>匹配返回 true。</returns>
        public static bool Verify(string text, string key, string expectedHex, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(expectedHex))
            {
                return false;
            }

            return HCryptoCore.FixedTimeEquals(Hash(text, key, encoding), expectedHex.ToLowerInvariant());
        }

        /// <summary>恒定时间校验字节数组认证码是否匹配。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="key">密钥字节。</param>
        /// <param name="expectedHex">期望的十六进制认证码。</param>
        /// <returns>匹配返回 true。</returns>
        public static bool Verify(byte[] data, byte[] key, string expectedHex)
        {
            if (string.IsNullOrEmpty(expectedHex))
            {
                return false;
            }

            return HCryptoCore.FixedTimeEquals(HHex.Encode(Hash(data, key)), expectedHex.ToLowerInvariant());
        }
    }
}
