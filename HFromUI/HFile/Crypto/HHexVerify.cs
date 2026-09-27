using System;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HHex"/> 十六进制编解码自测验证类。
    /// 覆盖大小写编码、边界字节、中文二进制往返、奇数长度/非法字符/空参异常。
    /// 调用方式：<c>HHexVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HHexVerify
    {
        /// <summary>运行 Hex 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HHex", v =>
            {
                // 边界字节与大小写
                byte[] data = new byte[] { 0x00, 0xFF, 0x10, 0xAB };
                v.Ok("Hex 小写编码", HHex.Encode(data) == "00ff10ab");
                v.Ok("Hex 大写编码", HHex.Encode(data, true) == "00FF10AB");

                // 大小写混排均可解码
                v.Ok("Hex 大小写混排解码", HCryptoVerifier.BytesEqual(HHex.Decode("00fF10Ab"), data));

                // 空数组
                v.Ok("Hex 空数组编码为空串", HHex.Encode(new byte[0]) == string.Empty);

                // 中文 UTF-8 字节往返
                string zh = "十六进制中文测试";
                byte[] zhBytes = Encoding.UTF8.GetBytes(zh);
                v.Ok("Hex 中文二进制往返", HCryptoVerifier.BytesEqual(HHex.Decode(HHex.Encode(zhBytes)), zhBytes));

                // 随机字节往返
                byte[] random = HCryptoCore.RandomBytes(100);
                v.Ok("Hex 随机字节往返", HCryptoVerifier.BytesEqual(HHex.Decode(HHex.Encode(random)), random));

                // 奇数长度必须报错
                bool oddThrew = false;
                try
                {
                    HHex.Decode("abc");
                }
                catch (FormatException)
                {
                    oddThrew = true;
                }

                v.Ok("Hex 奇数长度抛 FormatException", oddThrew);

                // 非法字符必须报错
                bool badCharThrew = false;
                try
                {
                    HHex.Decode("zz");
                }
                catch (FormatException)
                {
                    badCharThrew = true;
                }

                v.Ok("Hex 非法字符抛 FormatException", badCharThrew);

                // 空参保护
                bool nullThrew = false;
                try
                {
                    HHex.Decode(null);
                }
                catch (ArgumentNullException)
                {
                    nullThrew = true;
                }

                v.Ok("Hex null 参数抛 ArgumentNullException", nullThrew);
            });
        }
    }
}
