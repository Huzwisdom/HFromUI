using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HHmacSha1"/> 带密钥散列自测验证类。
    /// 覆盖 RFC 2202 标准向量、密钥敏感性、数据敏感性、Verify 正负例、文件 HMAC 与内存一致。
    /// 调用方式：<c>HHmacSha1Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HHmacSha1Verify
    {
        /// <summary>运行 HMAC-SHA1 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HHmacSha1", v =>
            {
                // RFC 2202 测试用例 1：key = 0x0b × 20，data = "Hi There"
                byte[] key = new byte[20];
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = 0x0B;
                }

                byte[] data = Encoding.UTF8.GetBytes("Hi There");
                v.Ok("HMAC-SHA1 RFC2202 标准向量",
                    HHex.Encode(HHmacSha1.Hash(data, key)) == "b617318655057264e28bc0b6fb378c8ef146be00");

                // 摘要长度（SHA1 = 20 字节 = 40 hex）
                v.Ok("HMAC-SHA1 摘要长度为 40 hex", HHex.Encode(HHmacSha1.Hash(data, key)).Length == 40);

                // 字符串 API + Verify
                string text = "接口签名测试 abc";
                string secret = "secret-key-2026";
                string mac = HHmacSha1.Hash(text, secret);
                v.Ok("HMAC-SHA1 正确密钥 Verify 通过", HHmacSha1.Verify(text, secret, mac));
                v.Ok("HMAC-SHA1 错误密钥 Verify 失败", !HHmacSha1.Verify(text, "other-key", mac));
                v.Ok("HMAC-SHA1 数据被改 Verify 失败", !HHmacSha1.Verify(text + "x", secret, mac));
                v.Ok("HMAC-SHA1 密钥敏感", HHmacSha1.Hash(text, "key1") != HHmacSha1.Hash(text, "key2"));

                // 文件 API
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "hmacsha1.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("HMAC-SHA1 文件与字节结果一致",
                        HHmacSha1.HashFile(file, key) == HHex.Encode(HHmacSha1.Hash(data, key)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
