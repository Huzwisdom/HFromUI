using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// AES-256-CBC 对称加解密（推荐使用的对称算法）。
    /// 加密与解密成对，同居于本文件。仅使用 .NET Framework 内置实现，无第三方 DLL。
    /// 密钥经 PBKDF2(1 万次迭代) 从密码派生；每次加密使用随机盐与随机 IV，
    /// 相同明文与密码每次产生的密文都不同。密文为自描述包：HAES + 盐(16) + IV(16) + 密文。
    /// </summary>
    public static class HAes
    {
        /// <summary>AES-256 密钥长度（字节）。</summary>
        private const int KeySize = 32;

        /// <summary>AES 分组/盐/IV 长度（字节）。</summary>
        private const int BlockSize = 16;

        /// <summary>密文包魔术头。</summary>
        private static readonly byte[] Magic = HCryptoCore.Magic("HAES");

        /// <summary>加密字符串，输出 Base64 文本。</summary>
        /// <param name="plainText">明文。</param>
        /// <param name="password">密码，不可为空。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>Base64 密文（含盐与 IV）。</returns>
        public static string Encrypt(string plainText, string password, Encoding encoding = null)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Convert.ToBase64String(Encrypt(encoding.GetBytes(plainText), password));
        }

        /// <summary>解密字符串。密码错误或数据损坏时抛出 <see cref="CryptographicException"/>。</summary>
        public static string Decrypt(string cipherBase64, string password, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            byte[] plain = Decrypt(Convert.FromBase64String(cipherBase64), password);
            return encoding.GetString(plain);
        }

        /// <summary>尝试解密字符串；密码错误或数据损坏时返回 false 而不抛异常。</summary>
        /// <param name="cipherBase64">Base64 密文。</param>
        /// <param name="password">密码。</param>
        /// <param name="plainText">解密成功时输出明文，失败时为 null。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>是否解密成功。</returns>
        public static bool TryDecrypt(string cipherBase64, string password, out string plainText, Encoding encoding = null)
        {
            try
            {
                plainText = Decrypt(cipherBase64, password, encoding);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is CryptographicException || ex is InvalidDataException)
                {
                    plainText = null;
                    return false;
                }

                throw;
            }
        }

        /// <summary>加密字节数组，返回自描述密文包。</summary>
        /// <param name="data">明文字节。</param>
        /// <param name="password">密码。</param>
        /// <returns>HAES 密文包。</returns>
        public static byte[] Encrypt(byte[] data, string password)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            Aes aes = Aes.Create();
            aes.KeySize = KeySize * 8;
            return HCryptoCore.Encrypt(aes, data, password, KeySize, BlockSize, Magic);
        }

        /// <summary>解密字节数组。</summary>
        /// <param name="cipher">HAES 密文包。</param>
        /// <param name="password">密码。</param>
        /// <returns>明文字节。</returns>
        public static byte[] Decrypt(byte[] cipher, string password)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }

            Aes aes = Aes.Create();
            aes.KeySize = KeySize * 8;
            return HCryptoCore.Decrypt(aes, cipher, password, KeySize, BlockSize, Magic);
        }

        /// <summary>流式加密文件，支持超大文件。</summary>
        /// <param name="sourceFilePath">明文源文件。</param>
        /// <param name="targetFilePath">密文目标文件。</param>
        /// <param name="password">密码。</param>
        public static void EncryptFile(string sourceFilePath, string targetFilePath, string password)
        {
            Aes aes = Aes.Create();
            aes.KeySize = KeySize * 8;
            HCryptoCore.EncryptFile(aes, sourceFilePath, targetFilePath, password, KeySize, BlockSize, Magic);
        }

        /// <summary>流式解密文件。</summary>
        /// <param name="sourceFilePath">密文源文件。</param>
        /// <param name="targetFilePath">明文目标文件。</param>
        /// <param name="password">密码。</param>
        public static void DecryptFile(string sourceFilePath, string targetFilePath, string password)
        {
            Aes aes = Aes.Create();
            aes.KeySize = KeySize * 8;
            HCryptoCore.DecryptFile(aes, sourceFilePath, targetFilePath, password, KeySize, BlockSize, Magic);
        }
    }
}
