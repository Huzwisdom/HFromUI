using System;
using System.IO;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// CRC-8/MAXIM（又称 Dallas/Maxim 1-Wire CRC）循环冗余校验：宽度 8、多项式 0x31、初值 0x00、
    /// 输入输出均反射（反射多项式常量 0x8C）、结果异或值 0x00。
    /// 用于 DS18B20 等 Dallas/Maxim 单总线温度传感器 ROM 校验、iButton 器件地址校验等。
    /// 注意与 <see cref="HCrc8"/>（多项式 0x07、不反射）不是同一个算法，结果不可互换。
    /// 标准校验值：CRC8/MAXIM(ASCII "123456789") = 0xA1；空数据 = 0x00。
    /// 纯 .NET 手写表驱动实现，不依赖任何第三方 DLL。
    /// </summary>
    public static class HCrc8Maxim
    {
        /// <summary>反射后的生成多项式常量（0x31 的位序反转）。</summary>
        private const byte ReflectedPolynomial = 0x8C;

        /// <summary>寄存器初值。</summary>
        private const byte InitialValue = 0x00;

        /// <summary>256 项查表，类加载时构造一次后全程复用。</summary>
        private static readonly byte[] table = BuildTable();

        /// <summary>计算字节数组的 CRC-8/MAXIM。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>1 字节 CRC 校验值。</returns>
        public static byte Compute(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            byte crc = InitialValue;
            for (int i = 0; i < data.Length; i++)
            {
                // 反射位序：CRC 低 8 位与输入字节异或后查表
                crc = table[crc ^ data[i]];
            }

            return crc;
        }

        /// <summary>计算字符串的 CRC-8/MAXIM。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>1 字节 CRC 校验值。</returns>
        public static byte Compute(string text, Encoding encoding = null)
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

        /// <summary>流式计算文件的 CRC-8/MAXIM，不全量载入内存，支持超大文件。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>1 字节 CRC 校验值。</returns>
        public static byte ComputeFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[HCryptoCore.BufferSize];
                byte crc = InitialValue;
                int read;
                while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (int i = 0; i < read; i++)
                    {
                        crc = table[crc ^ buffer[i]];
                    }
                }

                return crc;
            }
        }

        /// <summary>
        /// 把 CRC-8/MAXIM 校验值转为大端字节表示（1 字节），便于追加到数据帧尾部。
        /// 字节列表组装统一使用 <see cref="HBytes"/>。
        /// </summary>
        /// <param name="value">CRC 校验值。</param>
        /// <returns>1 字节数组。</returns>
        public static byte[] GetBytes(byte value)
        {
            var hb = new HBytes();
            hb.Add(value);
            return hb.Bytes.ToArray();
        }

        /// <summary>计算字节数组的 CRC-8/MAXIM，输出 2 位小写十六进制。</summary>
        /// <param name="data">原始数据。</param>
        /// <returns>2 位小写十六进制（如 "a1"）。</returns>
        public static string ComputeHex(byte[] data)
        {
            return HHex.Encode(GetBytes(Compute(data)));
        }

        /// <summary>计算字符串的 CRC-8/MAXIM，输出 2 位小写十六进制。</summary>
        /// <param name="text">原文。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>2 位小写十六进制。</returns>
        public static string ComputeHex(string text, Encoding encoding = null)
        {
            return HHex.Encode(GetBytes(Compute(text, encoding)));
        }

        /// <summary>流式计算文件的 CRC-8/MAXIM，输出 2 位小写十六进制。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>2 位小写十六进制。</returns>
        public static string ComputeFileHex(string filePath)
        {
            return HHex.Encode(GetBytes(ComputeFile(filePath)));
        }

        /// <summary>校验字节数组的 CRC-8/MAXIM 是否等于期望值。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(byte[] data, byte expected)
        {
            return Compute(data) == expected;
        }

        /// <summary>校验字符串的 CRC-8/MAXIM 是否等于期望值。</summary>
        /// <param name="text">原文。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>一致返回 true。</returns>
        public static bool Verify(string text, byte expected, Encoding encoding = null)
        {
            return Compute(text, encoding) == expected;
        }

        /// <summary>校验文件的 CRC-8/MAXIM 是否等于期望值。</summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="expected">期望的 CRC 数值。</param>
        /// <returns>一致返回 true。</returns>
        public static bool VerifyFile(string filePath, byte expected)
        {
            return ComputeFile(filePath) == expected;
        }

        /// <summary>校验字节数组的 CRC-8/MAXIM 是否等于期望的十六进制（大小写均可）。</summary>
        /// <param name="data">原始数据。</param>
        /// <param name="expectedHex">2 位十六进制期望值。</param>
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
                return expected.Length == 1 && Compute(data) == expected[0];
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>按“反射位序”构造 256 项 CRC-8/MAXIM 查表。</summary>
        /// <returns>长度 256 的查表。</returns>
        private static byte[] BuildTable()
        {
            var result = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                byte crc = (byte)i;
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 0x01) != 0)
                    {
                        crc = (byte)((crc >> 1) ^ ReflectedPolynomial);
                    }
                    else
                    {
                        crc = (byte)(crc >> 1);
                    }
                }

                result[i] = crc;
            }

            return result;
        }
    }
}
