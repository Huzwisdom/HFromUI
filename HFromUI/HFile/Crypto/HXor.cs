using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// XOR 异或加解密（最基础的对称算法：按密钥循环逐字节异或）。
    /// 异或运算自逆——加密与解密是同一个运算，因此本文件同时提供 Encrypt/Decrypt 两个语义化名称。
    /// 安全性很弱（密钥短或明文有规律时容易被统计分析破解），仅适用于简单混淆/教学/私有协议场景；
    /// 需要真正的保密请使用 <see cref="HAes"/>。密文为自描述包：HXOR + 密钥长度(4B) + 密文。
    /// </summary>
    public static class HXor
    {
        /// <summary>密文包魔术头。</summary>
        private static readonly byte[] Magic = HCryptoCore.Magic("HXOR");

        /// <summary>加密字符串，输出 Base64 文本。</summary>
        /// <param name="plainText">明文。</param>
        /// <param name="key">任意非空密钥字符串（按 UTF-8 取字节，循环使用）。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>Base64 密文。</returns>
        public static string Encrypt(string plainText, string key, Encoding encoding = null)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Convert.ToBase64String(Encrypt(encoding.GetBytes(plainText), encoding.GetBytes(key)));
        }

        /// <summary>解密字符串（XOR 自逆，等价于再加密一次）。</summary>
        /// <param name="cipherBase64">Base64 密文。</param>
        /// <param name="key">加密时使用的同一个密钥字符串。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>明文字符串。</returns>
        public static string Decrypt(string cipherBase64, string key, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            byte[] plain = Decrypt(Convert.FromBase64String(cipherBase64), encoding.GetBytes(key));
            return encoding.GetString(plain);
        }

        /// <summary>尝试解密字符串；密文损坏时返回 false。</summary>
        /// <param name="cipherBase64">Base64 密文。</param>
        /// <param name="key">密钥字符串。</param>
        /// <param name="plainText">解密成功时输出明文，失败时为 null。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>是否解密成功。</returns>
        public static bool TryDecrypt(string cipherBase64, string key, out string plainText, Encoding encoding = null)
        {
            try
            {
                plainText = Decrypt(cipherBase64, key, encoding);
                return true;
            }
            catch (FormatException)
            {
                plainText = null;
                return false;
            }
            catch (CryptographicException)
            {
                plainText = null;
                return false;
            }
        }

        /// <summary>
        /// 加密字节数组：data[i] ^= key[i % key.Length]，并加上 HXOR 包头。
        /// </summary>
        /// <param name="data">明文字节。</param>
        /// <param name="key">任意非空密钥字节。</param>
        /// <returns>HXOR 密文包。</returns>
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            byte[] body = XorCore(data, key);

            // 用 HBytes 组装：魔术头 + 密钥长度(4B 大端) + 密文
            var packet = new HBytes();
            packet.Add(Magic);
            packet.Add(key.Length);
            packet.Add(body);
            return packet.Bytes.ToArray();
        }

        /// <summary>
        /// 解密字节数组（XOR 自逆）。会校验魔术头，并检查密钥长度是否与加密时一致。
        /// </summary>
        /// <param name="cipher">HXOR 密文包。</param>
        /// <param name="key">加密时使用的同一个密钥字节。</param>
        /// <returns>明文字节。</returns>
        /// <exception cref="CryptographicException">魔术头不符、密钥长度不符或数据损坏时抛出。</exception>
        public static byte[] Decrypt(byte[] cipher, byte[] key)
        {
            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            if (cipher.Length < Magic.Length + 4)
            {
                throw new CryptographicException("密文已损坏：长度不足。");
            }

            for (int i = 0; i < Magic.Length; i++)
            {
                if (cipher[i] != Magic[i])
                {
                    throw new CryptographicException("无效的密文格式：缺少 HXOR 魔术头。");
                }
            }

            // 包头中的密钥长度（大端 4 字节）
            int savedKeyLength = (cipher[4] << 24) | (cipher[5] << 16) | (cipher[6] << 8) | cipher[7];
            if (savedKeyLength != key.Length)
            {
                throw new CryptographicException("密钥长度与加密时不一致。");
            }

            byte[] body = new byte[cipher.Length - Magic.Length - 4];
            Buffer.BlockCopy(cipher, Magic.Length + 4, body, 0, body.Length);
            return XorCore(body, key);
        }

        /// <summary>流式加密文件。</summary>
        /// <param name="sourceFilePath">明文源文件。</param>
        /// <param name="targetFilePath">密文目标文件。</param>
        /// <param name="key">任意非空密钥字节。</param>
        public static void EncryptFile(string sourceFilePath, string targetFilePath, byte[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            using (FileStream input = File.OpenRead(sourceFilePath))
            using (FileStream output = File.Create(targetFilePath))
            {
                var header = new HBytes();
                header.Add(Magic);
                header.Add(key.Length);
                byte[] headerBytes = header.Bytes.ToArray();
                output.Write(headerBytes, 0, headerBytes.Length);
                TransformStream(input, output, key);
            }
        }

        /// <summary>流式解密文件。</summary>
        /// <param name="sourceFilePath">密文源文件。</param>
        /// <param name="targetFilePath">明文目标文件。</param>
        /// <param name="key">加密时使用的同一个密钥字节。</param>
        public static void DecryptFile(string sourceFilePath, string targetFilePath, byte[] key)
        {
            if (key == null || key.Length == 0)
            {
                throw new ArgumentNullException("key");
            }

            using (FileStream input = File.OpenRead(sourceFilePath))
            {
                byte[] header = HCryptoCore.ReadExact(input, Magic.Length + 4);
                for (int i = 0; i < Magic.Length; i++)
                {
                    if (header[i] != Magic[i])
                    {
                        throw new CryptographicException("无效的密文格式：缺少 HXOR 魔术头。");
                    }
                }

                int savedKeyLength = (header[4] << 24) | (header[5] << 16) | (header[6] << 8) | header[7];
                if (savedKeyLength != key.Length)
                {
                    throw new CryptographicException("密钥长度与加密时不一致。");
                }

                using (FileStream output = File.Create(targetFilePath))
                {
                    TransformStream(input, output, key);
                }
            }
        }

        /// <summary>XOR 核心：逐字节循环异或（加密解密同一逻辑）。</summary>
        private static byte[] XorCore(byte[] data, byte[] key)
        {
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }

            return result;
        }

        /// <summary>流式 XOR：按缓冲区处理，密钥随全局位置循环。</summary>
        private static void TransformStream(Stream input, Stream output, byte[] key)
        {
            var buffer = new byte[HCryptoCore.BufferSize];
            int read;
            long position = 0;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    buffer[i] = (byte)(buffer[i] ^ key[(position + i) % key.Length]);
                }

                output.Write(buffer, 0, read);
                position += read;
            }
        }
    }
}
