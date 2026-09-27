using System;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// 密码学安全随机数生成器（只能“生成”，不可逆）。
    /// 底层使用 RNGCryptoServiceProvider，适合生成盐、IV、密钥、一次性验证码/随机密码；
    /// 不要用 System.Random（可预测）处理安全数据。字节累积统一使用 <see cref="HBytes"/>。
    /// </summary>
    public static class HCryptoRandom
    {
        /// <summary>随机密码默认字母表（去除易混淆的 0/O、1/l/I）。</summary>
        private const string DefaultAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

        /// <summary>
        /// 生成指定数量的密码学随机字节。
        /// </summary>
        /// <param name="count">字节数，必须大于 0。</param>
        /// <returns>随机字节数组。</returns>
        public static byte[] Bytes(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count", "数量必须大于 0。");
            }

            return HCryptoCore.RandomBytes(count);
        }

        /// <summary>
        /// 生成一个非负随机整数 [0, int.MaxValue)。
        /// </summary>
        public static int Int()
        {
            byte[] data = HCryptoCore.RandomBytes(4);

            // 清零最高位保证非负
            data[0] &= 0x7F;
            return (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
        }

        /// <summary>
        /// 生成 [minValue, maxValue) 区间的随机整数。
        /// </summary>
        /// <param name="minValue">下界（含）。</param>
        /// <param name="maxValue">上界（不含）。</param>
        /// <returns>区间内随机整数。</returns>
        public static int Int(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
            {
                throw new ArgumentException("上界必须大于下界。");
            }

            uint range = (uint)(maxValue - minValue);
            return minValue + (int)(UInt() % range);
        }

        /// <summary>
        /// 生成指定长度的随机字符串（可用于随机密码/验证码）。
        /// </summary>
        /// <param name="length">字符个数。</param>
        /// <param name="alphabet">候选字符集，默认使用去除易混淆字符的 57 字符集。</param>
        /// <returns>随机字符串。</returns>
        public static string String(int length, string alphabet = DefaultAlphabet)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length", "长度必须大于 0。");
            }

            if (string.IsNullOrEmpty(alphabet))
            {
                throw new ArgumentNullException("alphabet");
            }

            // 用 HBytes 累积随机字符（每个字符按 UTF-8 字节加入）
            var result = new HBytes();
            for (int i = 0; i < length; i++)
            {
                int index = Int(0, alphabet.Length);
                result.Add(new[] { (byte)alphabet[index] });
            }

            return System.Text.Encoding.ASCII.GetString(result.Bytes.ToArray());
        }

        /// <summary>
        /// 生成指定字节数的随机十六进制字符串（长度为字节数的 2 倍）。
        /// </summary>
        /// <param name="byteCount">字节数。</param>
        /// <param name="upperCase">是否大写，默认小写。</param>
        /// <returns>随机十六进制文本。</returns>
        public static string Hex(int byteCount, bool upperCase = false)
        {
            return HHex.Encode(Bytes(byteCount), upperCase);
        }

        /// <summary>生成 0 ~ uint.MaxValue 的无符号随机整数。</summary>
        private static uint UInt()
        {
            byte[] data = HCryptoCore.RandomBytes(4);
            return ((uint)data[0] << 24) | ((uint)data[1] << 16) | ((uint)data[2] << 8) | data[3];
        }
    }
}
