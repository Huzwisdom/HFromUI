using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// CRC-16/MODBUS 循环冗余校验：宽度 16、多项式 0x8005（反射计算常量 0xA001）、初值 0xFFFF、
    /// 输入输出均反射、结果异或值 0x0000。
    /// 用于标准 Modbus RTU 串行链路（RS-485/RS-232）报文帧尾校验，是工业现场最常见的 CRC-16 变种。
    /// Modbus RTU 线上发送时低字节在前、高字节在后；本类 <see cref="GetBytes"/> 输出大端，
    /// 若需线上小端字节序请自行交换两字节。
    /// 标准校验值：CRC16/MODBUS(ASCII "123456789") = 0x4B37；空数据 = 0xFFFF。
    /// 纯 .NET 手写表驱动实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HCrc16Modbus
    {
        /// <summary>反射后的生成多项式常量（0x8005 的 16 位位序反转）。</summary>
        private const ushort ReflectedPolynomial = 0xA001;

        /// <summary>寄存器初值。</summary>
        private const ushort InitialValue = 0xFFFF;

        /// <summary>256 项查表，类加载时构造一次后全程复用。</summary>
        private static readonly ushort[] table = BuildTable();

        /// <summary>计算字节数组的 CRC-16/MODBUS。</summary>
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
                // 反射位序：CRC 低字节与输入字节异或后查表
                crc = (ushort)((crc >> 8) ^ table[(crc ^ data[i]) & 0xFF]);
            }

            return crc;
        }

        /// <summary>计算字符串的 CRC-16/MODBUS。</summary>
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

        /// <summary>流式计算文件的 CRC-16/MODBUS，不全量载入内存，支持超大文件。</summary>
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
                        crc = (ushort)((crc >> 8) ^ table[(crc ^ buffer[i]) & 0xFF]);
                    }
                }

                return crc;
            }
        }

        /// <summary>
        /// 把 CRC-16 校验值转为大端字节表示（高字节在前，2 字节）。
        /// 注意 Modbus RTU 线上为小端（低字节先发送），组帧时需交换两字节。
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

        /// <summary>计算字节数组的 CRC-16/MODBUS，输出 4 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>4 位小写十六进制（如 "4b37"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 CRC-16/MODBUS，输出 4 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>4 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 CRC-16/MODBUS，输出 4 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>4 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 CRC-16/MODBUS 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, ushort expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 CRC-16/MODBUS 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, ushort expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 CRC-16/MODBUS 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, ushort expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 CRC-16/MODBUS 是否等于期望的十六进制（大小写均可）。</summary>
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

        /// <summary>按“反射位序”构造 256 项 CRC-16 查表。</summary>
        /// <returns>长度 256 的查表。</returns>
        private static ushort[] BuildTable()
        {
            var result = new ushort[256];
            for (int i = 0; i < 256; i++)
            {
                ushort crc = (ushort)i;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ ReflectedPolynomial);
                    }
                    else
                    {
                        crc = (ushort)(crc >> 1);
                    }
                }

                result[i] = crc;
            }

            return result;
        }
    }
}
