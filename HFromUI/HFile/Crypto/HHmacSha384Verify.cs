using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HHmacSha384"/> 带密钥散列自测验证类。
    /// 覆盖 RFC 4231 标准向量、密钥敏感性、数据敏感性、Verify 正负例、文件 HMAC 与内存一致。
    /// 调用方式：<c>HHmacSha384Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HHmacSha384Verify
    {
        /// <summary>运行 HMAC-SHA384 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HHmacSha384", v =>
            {
                // RFC 4231 测试用例 1：key = 0x0b × 20，data = "Hi There"
                byte[] key = new byte[20];
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = 0x0B;
                }

                byte[] data = Encoding.UTF8.GetBytes("Hi There");
                v.Ok("HMAC-SHA384 RFC4231 标准向量",
                    HHex.Encode(HHmacSha384.Hash(data, key)) == "afd03944d84895626b0825f4ab46907f15f9dadbe4101ec682aa034c7cebc59cfaea9ea9076ede7f4af152e8b2fa9cb6");

                // 摘要长度（SHA384 = 48 字节 = 96 hex）
                v.Ok("HMAC-SHA384 摘要长度为 96 hex", HHex.Encode(HHmacSha384.Hash(data, key)).Length == 96);

                // 字符串 API + Verify
                string text = "接口签名测试 abc";
                string secret = "secret-key-2026";
                string mac = HHmacSha384.Hash(text, secret);
                v.Ok("HMAC-SHA384 正确密钥 Verify 通过", HHmacSha384.Verify(text, secret, mac));
                v.Ok("HMAC-SHA384 错误密钥 Verify 失败", !HHmacSha384.Verify(text, "other-key", mac));
                v.Ok("HMAC-SHA384 数据被改 Verify 失败", !HHmacSha384.Verify(text + "x", secret, mac));
                v.Ok("HMAC-SHA384 密钥敏感", HHmacSha384.Hash(text, "key1") != HHmacSha384.Hash(text, "key2"));

                // 文件 API
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "hmacsha384.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("HMAC-SHA384 文件与字节结果一致",
                        HHmacSha384.HashFile(file, key) == HHex.Encode(HHmacSha384.Hash(data, key)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
