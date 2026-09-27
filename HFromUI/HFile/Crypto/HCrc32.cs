using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// CRC-32 循环冗余校验（CRC-32/ISO-HDLC，又名 CRC-32/ADCCP）：宽度 32、多项式 0x04C11DB7
    /// （反射计算常量 0xEDB88320）、初值 0xFFFFFFFF、输入输出均反射、结果异或 0xFFFFFFFF。
    /// 这是最常见的 CRC-32：ZIP/PKZIP、PNG（IDAT 块）、zlib、gzip、以太网帧 FCS 均用同一参数。
    /// CRC 只能验错不能还原原文；它不是密码学哈希，可被任意构造碰撞，安全场景请用 SHA-256。
    /// 标准校验值：CRC32(ASCII "123456789") = 0xCBF43926；空数据 = 0x00000000。
    /// 纯 .NET 手写表驱动实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HCrc32
    {
        /// <summary>反射后的生成多项式常量（0x04C11DB7 的 32 位位序反转）。</summary>
        private const uint ReflectedPolynomial = 0xEDB88320;

        /// <summary>寄存器初值。</summary>
        private const uint InitialValue = 0xFFFFFFFF;

        /// <summary>结果异或值（输出前再异或一次）。</summary>
        private const uint OutputXor = 0xFFFFFFFF;

        /// <summary>256 项查表，类加载时构造一次后全程复用。</summary>
        private static readonly uint[] table = BuildTable();

        /// <summary>计算字节数组的 CRC-32。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>4 字节 CRC 校验值。</returns>
        public static uint Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            uint crc = InitialValue;
            for (int i = 0; i < data.Length; i++)
            {
                crc = (crc >> 8) ^ table[(crc ^ data[i]) & 0xFF];
            }

            return crc ^ OutputXor;
        }

        /// <summary>计算字符串的 CRC-32。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>4 字节 CRC 校验值。</returns>
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

        /// <summary>流式计算文件的 CRC-32，不全量载入内存，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>4 字节 CRC 校验值。</returns>
        public static uint ComputeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[HCryptoCore.BufferSize];
                uint crc = InitialValue;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        crc = (crc >> 8) ^ table[(crc ^ buffer[i]) & 0xFF];
                    }
                }

                return crc ^ OutputXor;
            }
        }

        /// <summary>
        /// 把 CRC-32 校验值转为大端字节表示（最高字节在前，4 字节），便于追加到数据帧尾部。
        /// 字节列表组装统一使用 <see cref="HBytes"/>。
        /// </summary>
        /// <param name="value">CRC 校验值。</param>
        /// <returns>4 字节大端数组。</returns>
        public static byte[] GetBytes(uint value)
        {
            var hb = new HBytes();
            hb.Add(value);
            return hb.Bytes.ToArray();
        }

        /// <summary>计算字节数组的 CRC-32，输出 8 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>8 位小写十六进制（如 "cbf43926"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 CRC-32，输出 8 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>8 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 CRC-32，输出 8 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>8 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 CRC-32 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, uint expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 CRC-32 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, uint expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 CRC-32 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, uint expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 CRC-32 是否等于期望的十六进制（大小写均可）。</summary>
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

        /// <summary>按“反射位序”构造 256 项 CRC-32 查表。</summary>
        /// <returns>长度 256 的查表。</returns>
        private static uint[] BuildTable()
        {
            var result = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x00000001) != 0)
                    {
                        crc = (crc >> 1) ^ ReflectedPolynomial;
                    }
                    else
                    {
                        crc = crc >> 1;
                    }
                }

                result[i] = crc;
            }

            return result;
        }
    }
}
