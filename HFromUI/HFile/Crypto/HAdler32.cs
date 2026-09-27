using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// Adler-32 校验和（RFC 1950，zlib/deflate 内置校验）：32 位结果 = B 占高 16 位、A 占低 16 位，
    /// A 从 1 开始累加每个字节，B 累加每一步的 A，二者均对质数 65521 取模。
    /// 计算速度远快于 CRC-32，但检错能力弱于 CRC，仅用于 zlib 流、PNG zlib 块等配套完整性校验，
    /// 不能用于对抗恶意篡改（可轻易构造碰撞，安全场景请用 SHA-256）。
    /// 标准校验值：Adler32(ASCII "123456789") = 0x091E01DE；空数据 = 0x00000001（A=1,B=0）。
    /// 纯 .NET 手写实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HAdler32
    {
        /// <summary>取模质数：小于 2^16 的最大质数 65521。</summary>
        private const uint Modulo = 65521;

        /// <summary>
        /// 两次取模之间最多累加的字节数。按 5552 分块可保证 32 位无符号整数不溢出
        /// （zlib 参考实现采用同一上限）。
        /// </summary>
        private const int MaxBlock = 5552;

        /// <summary>A 累加器初值。</summary>
        private const uint InitialA = 1;

        /// <summary>B 累加器初值。</summary>
        private const uint InitialB = 0;

        /// <summary>计算字节数组的 Adler-32。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>4 字节校验值（B 在高 16 位，A 在低 16 位）。</returns>
        public static uint Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return Update(InitialA, InitialB, data, 0, data.Length);
        }

        /// <summary>计算字符串的 Adler-32。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>4 字节校验值。</returns>
        public static uint Compute(string text, Encoding encoding = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Compute(encoding.GetBytes(text));
        }

        /// <summary>流式计算文件的 Adler-32，不全量载入内存，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>4 字节校验值。</returns>
        public static uint ComputeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[HCryptoCore.BufferSize];
                uint a = InitialA;
                uint b = InitialB;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    uint merged = Update(a, b, buffer, 0, read);
                    a = merged & 0xFFFF;
                    b = (merged >> 16) & 0xFFFF;
                }

                return (b << 16) | a;
            }
        }

        /// <summary>
        /// 把 Adler-32 校验值转为大端字节表示（最高字节在前，4 字节）。
        /// 字节列表组装统一使用 <see cref="HBytes"/>。
        /// </summary>
        /// <param name="value">校验值。</param>
        /// <returns>4 字节大端数组。</returns>
        public static byte[] GetBytes(uint value)
        {
            var hb = new HBytes();
            hb.Add(value);
            return hb.Bytes.ToArray();
        }

        /// <summary>计算字节数组的 Adler-32，输出 8 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>8 位小写十六进制（如 "091e01de"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 Adler-32，输出 8 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>8 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 Adler-32，输出 8 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>8 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 Adler-32 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, uint expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 Adler-32 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, uint expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 Adler-32 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, uint expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 Adler-32 是否等于期望的十六进制（大小写均可）。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expectedHex">8 位十六进制期望值。</param>
        /// <returns>一致返回 true；空值或非法十六进制返回 false。</returns>
        public static bool Verify(byte[] data, string expectedHex)
        {
            if (string.IsNullOrEmpty(expectedHex))
            {
                return false;
            }

            try
            {
                byte[] expected = HHex.Decode(expectedHex);
                if (expected.Length != 4)
                {
                    return false;
                }

                uint value = ((uint)expected[0] << 24) | ((uint)expected[1] << 16)
                    | ((uint)expected[2] << 8) | expected[3];
                return Compute(data) == value;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 在已有 A/B 累加器基础上继续累加一段数据（文件流式分段复用），
        /// 按 MaxBlock 分块取模，保证 32 位累加不溢出。
        /// </summary>
        /// <param name="a">当前 A（低 16 位累加器）。</param>
        /// <param name="b">当前 B（高 16 位累加器）。</param>
        /// <param name="data">数据缓冲。</param>
        /// <param name="offset">起始偏移。</param>
        /// <param name="count">本次处理字节数。</param>
        /// <returns>合并后的校验值（B 在高 16 位，A 在低 16 位）。</returns>
        private static uint Update(uint a, uint b, byte[] data, int offset, int count)
        {
            int done = 0;
            while (done < count)
            {
                int chunk = Math.Min(MaxBlock, count - done);
                for (int i = 0; i < chunk; i++)
                {
                    a += data[offset + done + i];
                    b += a;
                }

                a %= Modulo;
                b %= Modulo;
                done += chunk;
            }

            return (b << 16) | a;
        }
    }
}
