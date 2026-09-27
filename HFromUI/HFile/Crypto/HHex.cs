using System;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// 十六进制编解码（可逆编码，非加密算法）。常用于把摘要、密文字节展示为文本。
    /// 字节列表拼接统一使用 <see cref="HBytes"/>。
    /// </summary>
    public static class HHex
    {
        /// <summary>
        /// 字节数组转十六进制字符串。
        /// </summary>
        /// <param name="data">字节数据。</param>
        /// <param name="upperCase">true 输出大写；默认 false 输出小写。</param>
        /// <returns>十六进制文本，长度恒为字节数的 2 倍。</returns>
        public static string Encode(byte[] data, bool upperCase = false)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            string format = upperCase ? "X2" : "x2";
            var sb = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
            {
                sb.Append(b.ToString(format));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 十六进制字符串还原为字节数组。
        /// </summary>
        /// <param name="hex">长度必须为偶数的十六进制文本（大小写均可）。</param>
        /// <returns>字节数组。</returns>
        /// <exception cref="FormatException">长度为奇数或含非法字符时抛出。</exception>
        public static byte[] Decode(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                throw new ArgumentNullException("hex");
            }

            if (hex.Length % 2 != 0)
            {
                throw new FormatException("十六进制字符串长度必须为偶数。");
            }

            // 边解析边用 HBytes 累积字节
            var result = new HBytes();
            for (int i = 0; i < hex.Length; i += 2)
            {
                byte high = ParseHexChar(hex[i]);
                byte low = ParseHexChar(hex[i + 1]);
                result.Add((byte)((high << 4) | low));
            }

            return result.Bytes.ToArray();
        }

        /// <summary>解析单个十六进制字符为 0~15。</summary>
        private static byte ParseHexChar(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }

            if (c >= 'a' && c <= 'f')
            {
                return (byte)(c - 'a' + 10);
            }

            if (c >= 'A' && c <= 'F')
            {
                return (byte)(c - 'A' + 10);
            }

            throw new FormatException("非法的十六进制字符：" + c);
        }
    }
}
