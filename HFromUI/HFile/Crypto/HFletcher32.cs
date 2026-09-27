using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// Fletcher-32 校验和：把数据按“大端 16 位字”送入两个 16 位累加器，
    /// sum1 累加每个字、sum2 累加每一步的 sum1，均对 65535 取模，结果 = sum2 占高 16 位、sum1 占低 16 位。
    /// 数据长度为奇数个字节时，末尾补一个 0x00 再组字（与 Wikipedia/常见实现一致）。
    /// 检错能力强于 16 位 Fletcher，常用于嵌入式大容量存储页校验、部分航天/遥测协议。
    /// 标准校验值：Fletcher32(ASCII "123456789") = 0x09DF09D5（奇数尾部按 0x00 补）；空数据 = 0x00000000。
    /// 纯 .NET 手写实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HFletcher32
    {
        /// <summary>取模值（65535 = 2^16-1，Fletcher 算法规定）。</summary>
        private const uint Modulo = 65535;

        /// <summary>
        /// 两次取模之间最多累加的 16 位字数。按 360 字分块保证 32 位无符号累加器不溢出
        /// （经典 Fletcher 参考实现采用同一上限）。
        /// </summary>
        private const int MaxWordsPerBlock = 360;

        /// <summary>计算字节数组的 Fletcher-32。</summary>
        /// <param name="data">原始数据（奇数长度时末尾按 0x00 补字节组字）。</param>
        /// <returns>4 字节校验值。</returns>
        public static uint Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            uint s1 = 0;
            uint s2 = 0;
            bool hasCarry = false;
            byte carry = 0;
            int blockWords = 0;
            Accumulate(ref s1, ref s2, data, 0, data.Length, ref hasCarry, ref carry, ref blockWords);

            // 奇数尾字节：补 0x00 组成最后一个大端字
            if (hasCarry)
            {
                AddWord(ref s1, ref s2, (ushort)(carry << 8), ref blockWords);
            }

            s1 %= Modulo;
            s2 %= Modulo;
            return (s2 << 16) | s1;
        }

        /// <summary>计算字符串的 Fletcher-32。</summary>
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

        /// <summary>流式计算文件的 Fletcher-32，不全量载入内存，支持超大文件。</summary>
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
                uint s1 = 0;
                uint s2 = 0;
                bool hasCarry = false;
                byte carry = 0;
                int blockWords = 0;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Accumulate(ref s1, ref s2, buffer, 0, read, ref hasCarry, ref carry, ref blockWords);
                }

                // 跨缓冲遗留的奇数尾字节：补 0x00 组成最后一个大端字
                if (hasCarry)
                {
                    AddWord(ref s1, ref s2, (ushort)(carry << 8), ref blockWords);
                }

                s1 %= Modulo;
                s2 %= Modulo;
                return (s2 << 16) | s1;
            }
        }

        /// <summary>
        /// 把 Fletcher-32 校验值转为大端字节表示（最高字节在前，4 字节）。
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

        /// <summary>计算字节数组的 Fletcher-32，输出 8 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>8 位小写十六进制（如 "09df09d5"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 Fletcher-32，输出 8 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>8 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 Fletcher-32，输出 8 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>8 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 Fletcher-32 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, uint expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 Fletcher-32 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, uint expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 Fletcher-32 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, uint expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 Fletcher-32 是否等于期望的十六进制（大小写均可）。</summary>
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
        /// 累加一段字节（内存整段与文件流式分段共用）：跨段保留奇数字节状态与分块计数，
        /// 每 MaxWordsPerBlock 个字取模一次，保证 32 位累加不溢出。
        /// </summary>
        private static void Accumulate(ref uint s1, ref uint s2, byte[] data, int offset, int count,
            ref bool hasCarry, ref byte carry, ref int blockWords)
        {
            for (int i = 0; i < count; i++)
            {
                byte b = data[offset + i];
                if (!hasCarry)
                {
                    // 先存高字节，等下一字节组成大端字
                    carry = b;
                    hasCarry = true;
                }
                else
                {
                    ushort word = (ushort)((carry << 8) | b);
                    AddWord(ref s1, ref s2, word, ref blockWords);
                    hasCarry = false;
                }
            }
        }

        /// <summary>加入一个 16 位字并按块取模。</summary>
        private static void AddWord(ref uint s1, ref uint s2, ushort word, ref int blockWords)
        {
            s1 += word;
            s2 += s1;
            blockWords++;
            if (blockWords >= MaxWordsPerBlock)
            {
                s1 %= Modulo;
                s2 %= Modulo;
                blockWords = 0;
            }
        }
    }
}
