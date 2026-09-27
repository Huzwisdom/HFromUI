using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// CRC-16/CCITT-FALSE 循环冗余校验：宽度 16、多项式 0x1021（x^16+x^12+x^5+1）、初值 0xFFFF、
    /// 输入输出均不反射、结果异或值 0x0000。
    /// 广泛用于 XMODEM、蓝牙、射频识别、很多 PLC/仪表通信协议的帧校验（FCS）。
    /// 注意它与 KERMIT（初值 0、反射）、MODBUS（多项式 0xA001、反射）参数不同，结果不可互换。
    /// 标准校验值：CRC16/CCITT-FALSE(ASCII "123456789") = 0x29B1；空数据 = 0xFFFF。
    /// 纯 .NET 手写表驱动实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HCrc16Ccitt
    {
        /// <summary>生成多项式（正常位序表示）。</summary>
        private const ushort Polynomial = 0x1021;

        /// <summary>寄存器初值。</summary>
        private const ushort InitialValue = 0xFFFF;

        /// <summary>256 项查表，类加载时构造一次后全程复用。</summary>
        private static readonly ushort[] table = BuildTable();

        /// <summary>计算字节数组的 CRC-16/CCITT-FALSE。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>2 字节 CRC 校验值。</returns>
        public static ushort Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            ushort crc = InitialValue;
            for (int i = 0; i < data.Length; i++)
            {
                // 正常位序：CRC 高字节与输入字节异或后查表，低字节顺移
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ data[i]) & 0xFF]);
            }

            return crc;
        }

        /// <summary>计算字符串的 CRC-16/CCITT-FALSE。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>2 字节 CRC 校验值。</returns>
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

        /// <summary>流式计算文件的 CRC-16/CCITT-FALSE，不全量载入内存，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>2 字节 CRC 校验值。</returns>
        public static ushort ComputeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[HCryptoCore.BufferSize];
                ushort crc = InitialValue;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ buffer[i]) & 0xFF]);
                    }
                }

                return crc;
            }
        }

        /// <summary>
        /// 把 CRC-16 校验值转为大端字节表示（高字节在前，2 字节），便于追加到数据帧尾部。
        /// 字节列表组装统一使用 <see cref="HBytes"/>。
        /// </summary>
        /// <param name="value">CRC 校验值。</param>
        /// <returns>2 字节大端数组。</returns>
        public static byte[] GetBytes(ushort value)
        {
            var hb = new HBytes();
            hb.Add(value);
            return hb.Bytes.ToArray();
        }

        /// <summary>计算字节数组的 CRC-16/CCITT-FALSE，输出 4 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>4 位小写十六进制（如 "29b1"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 CRC-16/CCITT-FALSE，输出 4 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>4 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 CRC-16/CCITT-FALSE，输出 4 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>4 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 CRC-16/CCITT-FALSE 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, ushort expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 CRC-16/CCITT-FALSE 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, ushort expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 CRC-16/CCITT-FALSE 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, ushort expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 CRC-16/CCITT-FALSE 是否等于期望的十六进制（大小写均可）。</summary>
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

        /// <summary>按“正常位序”构造 256 项 CRC-16 查表。</summary>
        /// <returns>长度 256 的查表。</returns>
        private static ushort[] BuildTable()
        {
            var result = new ushort[256];
            for (int i = 0; i < 256; i++)
            {
                ushort crc = (ushort)(i << 8);
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)((crc << 1) ^ Polynomial);
                    }
                    else
                    {
                        crc = (ushort)(crc << 1);
                    }
                }

                result[i] = crc;
            }

            return result;
        }
    }
}
