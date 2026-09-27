using System;
using System.IO;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HHmacMd5"/> 带密钥散列自测验证类。
    /// 覆盖 RFC 2202 标准向量、密钥敏感性、数据敏感性、Verify 正负例、文件 HMAC 与内存一致。
    /// 调用方式：<c>HHmacMd5Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HHmacMd5Verify
    {
        /// <summary>运行 HMAC-MD5 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HHmacMd5", v =>
            {
                // RFC 2202 测试用例 1：key = 0x0b × 16，data = "Hi There"
                byte[] key = new byte[16];
                for (int i = 0; i < key.Length; i++)
                {
                    key[i] = 0x0B;
                }

                byte[] data = System.Text.Encoding.UTF8.GetBytes("Hi There");
                v.Ok("HMAC-MD5 RFC2202 标准向量",
                    HHex.Encode(HHmacMd5.Hash(data, key)) == "9294727a3638bb1c13f48ef8158bfc9d");

                // 摘要长度（MD5 = 16 字节 = 32 hex）
                v.Ok("HMAC-MD5 摘要长度为 32 hex", HHex.Encode(HHmacMd5.Hash(data, key)).Length == 32);

                // 字符串 API + Verify
                string text = "接口签名测试 abc";
                string secret = "secret-key-2026";
                string mac = HHmacMd5.Hash(text, secret);
                v.Ok("HMAC-MD5 正确密钥 Verify 通过", HHmacMd5.Verify(text, secret, mac));
                v.Ok("HMAC-MD5 错误密钥 Verify 失败", !HHmacMd5.Verify(text, "other-key", mac));
                v.Ok("HMAC-MD5 数据被改 Verify 失败", !HHmacMd5.Verify(text + "x", secret, mac));

                // 密钥不同认证码不同
                v.Ok("HMAC-MD5 密钥敏感", HHmacMd5.Hash(text, "key1") != HHmacMd5.Hash(text, "key2"));

                // 文件 API
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "hmacmd5.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("HMAC-MD5 文件与字节结果一致",
                        HHmacMd5.HashFile(file, key) == HHex.Encode(HHmacMd5.Hash(data, key)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
