using System;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HBase64"/> 编解码自测验证类。
    /// 覆盖 RFC 4648 标准向量、中文与任意二进制往返、非法输入异常、空参异常。
    /// 调用方式：<c>HBase64Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HBase64Verify
    {
        /// <summary>运行 Base64 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HBase64", v =>
            {
                // RFC 4648 §10 标准测试向量
                v.Ok("Base64(\"\") 标准向量", HBase64.EncodeString(string.Empty) == string.Empty);
                v.Ok("Base64(\"f\") 标准向量", HBase64.EncodeString("f") == "Zg==");
                v.Ok("Base64(\"fo\") 标准向量", HBase64.EncodeString("fo") == "Zm8=");
                v.Ok("Base64(\"foo\") 标准向量", HBase64.EncodeString("foo") == "Zm9v");
                v.Ok("Base64(\"foob\") 标准向量", HBase64.EncodeString("foob") == "Zm9vYg==");
                v.Ok("Base64(\"fooba\") 标准向量", HBase64.EncodeString("fooba") == "Zm9vYmE=");
                v.Ok("Base64(\"foobar\") 标准向量", HBase64.EncodeString("foobar") == "Zm9vYmFy");

                // 解码还原
                v.Ok("Base64 解码 foobar", HBase64.DecodeString("Zm9vYmFy") == "foobar");

                // 中文往返
                string zh = "Base64 中文 abc 123 !@#";
                v.Ok("Base64 中文字符串往返", HBase64.DecodeString(HBase64.EncodeString(zh, Encoding.UTF8), Encoding.UTF8) == zh);

                // 任意随机二进制往返（含 0x00/0xFF）
                byte[] data = HCryptoCore.RandomBytes(200);
                v.Ok("Base64 随机字节往返", HCryptoVerifier.BytesEqual(HBase64.Decode(HBase64.Encode(data)), data));

                // 非法 Base64 文本抛 FormatException
                bool formatThrew = false;
                try
                {
                    HBase64.Decode("!!!!不是合法base64====");
                }
                catch (FormatException)
                {
                    formatThrew = true;
                }

                v.Ok("Base64 非法文本抛 FormatException", formatThrew);

                // 空参保护
                bool nullThrew = false;
                try
                {
                    HBase64.Encode(null);
                }
                catch (ArgumentNullException)
                {
                    nullThrew = true;
                }

                v.Ok("Base64 null 参数抛 ArgumentNullException", nullThrew);
            });
        }
    }
}
