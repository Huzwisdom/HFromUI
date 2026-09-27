using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HHmacSha256"/> 带密钥散列自测验证类。
    /// 覆盖 RFC 4231 标准向量、密钥敏感性、数据敏感性、Verify 正负例、文件 HMAC 与内存一致。
    /// 调用方式：<c>HHmacSha256Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HHmacSha256Verify
    {
        /// <summary>运行 HMAC-SHA256 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HHmacSha256", v =>
            {
                // RFC 4231 测试用例 1：key = 0x0b × 20，data = "Hi There"
                byte[] key = new byte[20];
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = 0x0B;
                }

                byte[] data = Encoding.UTF8.GetBytes("Hi There");
                v.Ok("HMAC-SHA256 RFC4231 标准向量",
                    HHex.Encode(HHmacSha256.Hash(data, key)) == "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7");

                // 摘要长度（SHA256 = 32 字节 = 64 hex）
                v.Ok("HMAC-SHA256 摘要长度为 64 hex", HHex.Encode(HHmacSha256.Hash(data, key)).Length == 64);

                // 字符串 API + Verify
                string text = "接口签名测试 abc";
                string secret = "secret-key-2026";
                string mac = HHmacSha256.Hash(text, secret);
                v.Ok("HMAC-SHA256 正确密钥 Verify 通过", HHmacSha256.Verify(text, secret, mac));
                v.Ok("HMAC-SHA256 错误密钥 Verify 失败", !HHmacSha256.Verify(text, "other-key", mac));
                v.Ok("HMAC-SHA256 数据被改 Verify 失败", !HHmacSha256.Verify(text + "x", secret, mac));
                v.Ok("HMAC-SHA256 密钥敏感", HHmacSha256.Hash(text, "key1") != HHmacSha256.Hash(text, "key2"));

                // 文件 API
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "hmacsha256.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("HMAC-SHA256 文件与字节结果一致",
                        HHmacSha256.HashFile(file, key) == HHex.Encode(HHmacSha256.Hash(data, key)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
