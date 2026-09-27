using System;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCryptoRandom"/> 密码学随机数自测验证类。
    /// 覆盖字节长度、区间随机（大量采样验证边界）、字符串长度与字符集、
    /// 十六进制随机串可解码、两次生成不重复、非法参数异常。
    /// 调用方式：<c>HCryptoRandomVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCryptoRandomVerify
    {
        /// <summary>运行随机数全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCryptoRandom", v =>
            {
                // 长度正确
                v.Ok("Random Bytes(16) 长度为 16", HCryptoRandom.Bytes(16).Length == 16);
                v.Ok("Random Bytes(1000) 长度为 1000", HCryptoRandom.Bytes(1000).Length == 1000);

                // 两次 32 字节随机不相同（碰撞概率可忽略）
                v.Ok("Random 两次随机字节不同",
                    !HCryptoVerifier.BytesEqual(HCryptoRandom.Bytes(32), HCryptoRandom.Bytes(32)));

                // 非负整数
                bool nonNegative = true;
                for (int i = 0; i < 1000; i++)
                {
                    if (HCryptoRandom.Int() < 0)
                    {
                        nonNegative = false;
                        break;
                    }
                }

                v.Ok("Random Int() 千次采样均非负", nonNegative);

                // 区间随机：采样 2000 次全部落在 [5,10)
                bool inRange = true;
                bool hitLow = false;
                bool hitHigh = false;
                for (int i = 0; i < 2000; i++)
                {
                    int n = HCryptoRandom.Int(5, 10);
                    if (n < 5 || n >= 10)
                    {
                        inRange = false;
                        break;
                    }

                    if (n == 5)
                    {
                        hitLow = true;
                    }

                    if (n == 9)
                    {
                        hitHigh = true;
                    }
                }

                v.Ok("Random Int(5,10) 采样均在区间内", inRange);
                v.Ok("Random Int(5,10) 能取到下界 5", hitLow);
                v.Ok("Random Int(5,10) 能取到上界内侧 9", hitHigh);

                // 非法区间
                bool rangeThrew = false;
                try
                {
                    HCryptoRandom.Int(10, 10);
                }
                catch (ArgumentException)
                {
                    rangeThrew = true;
                }

                v.Ok("Random 非法区间抛 ArgumentException", rangeThrew);

                // 随机字符串：长度、全部字符在默认字母表内、两次不同
                const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
                string s1 = HCryptoRandom.String(24);
                string s2 = HCryptoRandom.String(24);
                bool allInAlphabet = true;
                foreach (char c in s1)
                {
                    if (alphabet.IndexOf(c) < 0)
                    {
                        allInAlphabet = false;
                        break;
                    }
                }

                v.Ok("Random String 长度为 24", s1.Length == 24);
                v.Ok("Random String 字符均在字母表内", allInAlphabet);
                v.Ok("Random String 两次不同", s1 != s2);

                // 十六进制随机串：长度正确且可被 HHex 解码回原字节数
                string hex = HCryptoRandom.Hex(16);
                byte[] back = HHex.Decode(hex);
                v.Ok("Random Hex 长度为 32", hex.Length == 32);
                v.Ok("Random Hex 可解码为 16 字节", back.Length == 16);

                // 用 HBytes 收集若干随机字节做拼接一致性校验
                var collected = new HBytes();
                collected.Add(HCryptoRandom.Bytes(8));
                collected.Add(HCryptoRandom.Bytes(8));
                v.Ok("Random 配合 HBytes 拼接长度为 16", collected.Count == 16);

                // 非法长度
                bool zeroThrew = false;
                try
                {
                    HCryptoRandom.Bytes(0);
                }
                catch (ArgumentOutOfRangeException)
                {
                    zeroThrew = true;
                }

                v.Ok("Random Bytes(0) 抛 ArgumentOutOfRangeException", zeroThrew);
            });
        }
    }
}
