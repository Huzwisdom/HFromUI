using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HHmacSha512"/> 带密钥散列自测验证类。
    /// 覆盖 RFC 4231 标准向量、密钥敏感性、数据敏感性、Verify 正负例、文件 HMAC 与内存一致。
    /// 调用方式：<c>HHmacSha512Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HHmacSha512Verify
    {
        /// <summary>运行 HMAC-SHA512 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HHmacSha512", v =>
            {
                // RFC 4231 测试用例 1：key = 0x0b × 20，data = "Hi There"
                byte[] key = new byte[20];
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = 0x0B;
                }

                byte[] data = Encoding.UTF8.GetBytes("Hi There");
                v.Ok("HMAC-SHA512 RFC4231 标准向量",
                    HHex.Encode(HHmacSha512.Hash(data, key)) == "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854");

                // 摘要长度（SHA512 = 64 字节 = 128 hex）
                v.Ok("HMAC-SHA512 摘要长度为 128 hex", HHex.Encode(HHmacSha512.Hash(data, key)).Length == 128);

                // 字符串 API + Verify
                string text = "接口签名测试 abc";
                string secret = "secret-key-2026";
                string mac = HHmacSha512.Hash(text, secret);
                v.Ok("HMAC-SHA512 正确密钥 Verify 通过", HHmacSha512.Verify(text, secret, mac));
                v.Ok("HMAC-SHA512 错误密钥 Verify 失败", !HHmacSha512.Verify(text, "other-key", mac));
                v.Ok("HMAC-SHA512 数据被改 Verify 失败", !HHmacSha512.Verify(text + "x", secret, mac));
                v.Ok("HMAC-SHA512 密钥敏感", HHmacSha512.Hash(text, "key1") != HHmacSha512.Hash(text, "key2"));

                // 文件 API
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "hmacsha512.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("HMAC-SHA512 文件与字节结果一致",
                        HHmacSha512.HashFile(file, key) == HHex.Encode(HHmacSha512.Hash(data, key)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
