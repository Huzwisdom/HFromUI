using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// Fletcher-16 校验和（John Gould Fletcher 提出）：把数据按字节送入两个累加器，
    /// sum1 累加每个字节、sum2 累加每一步的 sum1，均对 255 取模，结果 = sum2 占高字节、sum1 占低字节。
    /// 检错能力强于单字节累加和（可检出接近全部 8 位突发错误），成本略高于简单加法和，
    /// 常见于 TCP 头校验思想原型、嵌入式遥测帧、某些硬盘 SMART/固件协议。
    /// 与 CRC 不同，它不含多项式除法；与 <see cref="HAdler32"/> 同族但模数为 255、宽度 16。
    /// 标准校验值：Fletcher16(ASCII "123456789") = 0x1EDE；空数据 = 0x0000。
    /// 纯 .NET 手写实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HFletcher16
    {
        /// <summary>取模值（255 不是质数，是 Fletcher 算法规定）。</summary>
        private const int Modulo = 255;

        /// <summary>
        /// 两次取模之间最多累加的字节数。按 4095 分块保证 32 位有符号累加器不溢出。
        /// </summary>
        private const int MaxBlock = 4095;

        /// <summary>计算字节数组的 Fletcher-16。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>2 字节校验值（sum2 高字节，sum1 低字节）。</returns>
        public static ushort Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            int merged = Update(0, 0, data, 0, data.Length);
            return (ushort)merged;
        }

        /// <summary>计算字符串的 Fletcher-16。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>2 字节校验值。</returns>
        public static ushort Compute(string text, Encoding encoding = null)
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

        /// <summary>流式计算文件的 Fletcher-16，不全量载入内存，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>2 字节校验值。</returns>
        public static ushort ComputeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[HCryptoCore.BufferSize];
                int s1 = 0;
                int s2 = 0;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    int merged = Update(s1, s2, buffer, 0, read);
                    s1 = merged & 0xFFFF;
                    s2 = (merged >> 16) & 0xFFFF;
                }

                return (ushort)((s2 << 8) | s1);
            }
        }

        /// <summary>
        /// 把 Fletcher-16 校验值转为大端字节表示（sum2 在前，2 字节），便于追加到数据帧尾部。
        /// 字节列表组装统一使用 <see cref="HBytes"/>。
        /// </summary>
        /// <param name="value">校验值。</param>
        /// <returns>2 字节大端数组。</returns>
        public static byte[] GetBytes(ushort value)
        {
            var hb = new HBytes();
            hb.Add(value);
            return hb.Bytes.ToArray();
        }

        /// <summary>计算字节数组的 Fletcher-16，输出 4 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>4 位小写十六进制（如 "1ede"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 Fletcher-16，输出 4 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>4 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 Fletcher-16，输出 4 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>4 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 Fletcher-16 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, ushort expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 Fletcher-16 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, ushort expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 Fletcher-16 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的校验值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, ushort expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 Fletcher-16 是否等于期望的十六进制（大小写均可）。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expectedHex">4 位十六进制期望值。</param>
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
                return expected.Length == 2
                    && Compute(data) == (ushort)((expected[0] << 8) | expected[1]);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 在已有 sum1/sum2 基础上继续累加一段数据（文件流式分段复用），
        /// 按 MaxBlock 分块取模，保证 32 位有符号累加不溢出。
        /// </summary>
        /// <param name="s1">当前第一累加器。</param>
        /// <param name="s2">当前第二累加器。</param>
        /// <param name="data">数据缓冲。</param>
        /// <param name="offset">起始偏移。</param>
        /// <param name="count">本次处理字节数。</param>
        /// <returns>合并值（sum2 在高 16 位，sum1 在低 16 位）。</returns>
        private static int Update(int s1, int s2, byte[] data, int offset, int count)
        {
            int done = 0;
            while (done < count)
            {
                int chunk = Math.Min(MaxBlock, count - done);
                for (int i = 0; i < chunk; i++)
                {
                    s1 += data[offset + done + i];
                    s2 += s1;
                }

                s1 %= Modulo;
                s2 %= Modulo;
                done += chunk;
            }

            return (s2 << 8) | s1;
        }
    }
}
