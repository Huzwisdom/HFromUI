using System;
using System.IO;
using System.Security.Cryptography;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// 加解密内部公共工具：密码学随机数、PBKDF2 密钥派生、自描述数据包头读写、
    /// 对称算法通用加解密流程、流拷贝等。仅供本文件夹下各算法类调用，不对外公开。
    /// 所有字节列表拼接统一使用 <see cref="HBytes"/>。
    /// </summary>
    internal static class HCryptoCore
    {
        /// <summary>PBKDF2(Rfc2898) 密钥派生迭代次数，次数越高抗暴力破解越强。</summary>
        public const int Iterations = 10000;

        /// <summary>流式加解密时的缓冲区大小（80KB），兼顾内存占用与大文件性能。</summary>
        public const int BufferSize = 81920;

        /// <summary>
        /// 生成密码学安全的随机字节（基于 RNGCryptoServiceProvider，不可预测，用于盐、IV、密钥）。
        /// </summary>
        /// <param name="length">需要的字节数。</param>
        /// <returns>随机字节数组。</returns>
        public static byte[] RandomBytes(int length)
        {
            var data = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }

            return data;
        }

        /// <summary>
        /// 使用 PBKDF2-SHA1（Rfc2898DeriveBytes）从“密码 + 盐”派生指定长度的密钥。
        /// 相同密码与盐会得到相同密钥；盐每次随机即可保证相同密码产生不同加密结果。
        /// </summary>
        /// <param name="password">用户密码。</param>
        /// <param name="salt">随机盐。</param>
        /// <param name="keySize">目标密钥长度（字节）。</param>
        /// <returns>派生出的密钥字节。</returns>
        public static byte[] DeriveKey(string password, byte[] salt, int keySize)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                return derive.GetBytes(keySize);
            }
        }

        /// <summary>
        /// 对称加密通用流程：生成随机盐/IV → PBKDF2 派生密钥 → 写出自描述包头 → CBC/PKCS7 加密。
        /// 输出包结构：算法魔术头(4B) + 盐 + IV + 密文。
        /// </summary>
        /// <param name="algorithm">已配置好密钥长度的对称算法实例（方法内会设置 CBC/PKCS7）。</param>
        /// <param name="plainBytes">明文字节。</param>
        /// <param name="password">密码。</param>
        /// <param name="keySize">密钥长度（字节）。</param>
        /// <param name="blockSize">分组/IV/盐长度（字节）。</param>
        /// <param name="magic">算法魔术头，例如 HAES/HDES，解密时用于校验与区分。</param>
        /// <returns>自描述密文包。</returns>
        public static byte[] Encrypt(SymmetricAlgorithm algorithm, byte[] plainBytes, string password, int keySize, int blockSize, byte[] magic)
        {
            byte[] salt = RandomBytes(blockSize);
            byte[] iv = RandomBytes(blockSize);

            using (algorithm)
            {
                algorithm.Mode = CipherMode.CBC;
                algorithm.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform encryptor = algorithm.CreateEncryptor(DeriveKey(password, salt, keySize), iv))
                using (var ms = new MemoryStream())
                {
                    WriteHeader(ms, magic, salt, iv);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 对称解密通用流程：校验魔术头 → 读出盐/IV → 派生密钥 → 解密并去除 PKCS7 填充。
        /// </summary>
        /// <param name="algorithm">与加密时相同类型的对称算法实例。</param>
        /// <param name="cipherBytes">自描述密文包。</param>
        /// <param name="password">密码。</param>
        /// <param name="keySize">密钥长度（字节）。</param>
        /// <param name="blockSize">分组/IV/盐长度（字节）。</param>
        /// <param name="magic">期望的算法魔术头。</param>
        /// <returns>明文字节。</returns>
        /// <exception cref="CryptographicException">密码错误、魔术头不符或数据损坏时抛出。</exception>
        public static byte[] Decrypt(SymmetricAlgorithm algorithm, byte[] cipherBytes, string password, int keySize, int blockSize, byte[] magic)
        {
            using (var input = new MemoryStream(cipherBytes))
            {
                byte[][] header = ReadHeader(input, magic, blockSize);
                byte[] salt = header[0];
                byte[] iv = header[1];

                using (algorithm)
                {
                    algorithm.Mode = CipherMode.CBC;
                    algorithm.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = algorithm.CreateDecryptor(DeriveKey(password, salt, keySize), iv))
                    using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                    using (var output = new MemoryStream())
                    {
                        CopyStream(cs, output);
                        return output.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 文件流式加密：包头 + 密文直接写入目标文件，不全量载入内存，可加密超大文件。
        /// </summary>
        public static void EncryptFile(SymmetricAlgorithm algorithm, string sourceFilePath, string targetFilePath, string password, int keySize, int blockSize, byte[] magic)
        {
            byte[] salt = RandomBytes(blockSize);
            byte[] iv = RandomBytes(blockSize);

            using (algorithm)
            using (FileStream input = File.OpenRead(sourceFilePath))
            using (FileStream output = File.Create(targetFilePath))
            {
                algorithm.Mode = CipherMode.CBC;
                algorithm.Padding = PaddingMode.PKCS7;
                WriteHeader(output, magic, salt, iv);
                using (ICryptoTransform encryptor = algorithm.CreateEncryptor(DeriveKey(password, salt, keySize), iv))
                using (var cs = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                {
                    CopyStream(input, cs);
                    cs.FlushFinalBlock();
                }
            }
        }

        /// <summary>
        /// 文件流式解密：校验包头后把明文写入目标文件。
        /// </summary>
        public static void DecryptFile(SymmetricAlgorithm algorithm, string sourceFilePath, string targetFilePath, string password, int keySize, int blockSize, byte[] magic)
        {
            using (FileStream input = File.OpenRead(sourceFilePath))
            {
                byte[][] header = ReadHeader(input, magic, blockSize);
                byte[] salt = header[0];
                byte[] iv = header[1];

                using (algorithm)
                using (FileStream output = File.Create(targetFilePath))
                {
                    algorithm.Mode = CipherMode.CBC;
                    algorithm.Padding = PaddingMode.PKCS7;
                    using (ICryptoTransform decryptor = algorithm.CreateDecryptor(DeriveKey(password, salt, keySize), iv))
                    using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                    {
                        CopyStream(cs, output);
                    }
                }
            }
        }

        /// <summary>
        /// 把 4 字节 ASCII 魔术头转为字节数组（如 "HAES"）。
        /// </summary>
        public static byte[] Magic(string text)
        {
            return new[] { (byte)text[0], (byte)text[1], (byte)text[2], (byte)text[3] };
        }

        /// <summary>写出包头：魔术头 + 盐 + IV（用 HBytes 组织内容）。</summary>
        private static void WriteHeader(Stream stream, byte[] magic, byte[] salt, byte[] iv)
        {
            var header = new HBytes();
            header.Add(magic);
            header.Add(salt);
            header.Add(iv);
            byte[] data = header.Bytes.ToArray();
            stream.Write(data, 0, data.Length);
        }

        /// <summary>读取并校验包头，返回 [盐, IV]。</summary>
        private static byte[][] ReadHeader(Stream stream, byte[] expectedMagic, int blockSize)
        {
            byte[] magic = ReadExact(stream, 4);
            for (int i = 0; i < 4; i++)
            {
                if (magic[i] != expectedMagic[i])
                {
                    throw new CryptographicException("无效的密文格式：算法魔术头不匹配。");
                }
            }

            return new[]
            {
                ReadExact(stream, blockSize),
                ReadExact(stream, blockSize)
            };
        }

        /// <summary>从流中恰好读取 length 字节，不足说明数据损坏。</summary>
        public static byte[] ReadExact(Stream stream, int length)
        {
            var buffer = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(buffer, offset, length - offset);
                if (read <= 0)
                {
                    throw new CryptographicException("密文已损坏：包头或数据不完整。");
                }

                offset += read;
            }

            return buffer;
        }

        /// <summary>带缓冲的流拷贝（不依赖 Stream.CopyTo，兼容全部目标框架）。</summary>
        public static void CopyStream(Stream source, Stream target)
        {
            var buffer = new byte[BufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                target.Write(buffer, 0, read);
            }
        }

        /// <summary>把多个字节数组按顺序拼接为一个数组（统一用 HBytes 完成列表添加）。</summary>
        public static byte[] Combine(params byte[][] arrays)
        {
            var all = new HBytes();
            foreach (byte[] array in arrays)
            {
                all.Add(array);
            }

            return all.Bytes.ToArray();
        }

        /// <summary>
        /// 恒定时间字符串比较：无论匹配与否，比较耗时基本一致，避免摘要校准时的时序侧信道泄露。
        /// </summary>
        public static bool FixedTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }
    }
}
