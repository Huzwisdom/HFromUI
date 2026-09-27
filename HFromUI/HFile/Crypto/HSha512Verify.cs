using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HSha512"/> 单向散列自测验证类。
    /// 覆盖 FIPS 180 标准向量、确定性、中文摘要校验、错误文本负例、文件摘要与内存摘要一致性。
    /// 调用方式：<c>HSha512Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HSha512Verify
    {
        /// <summary>运行 SHA-512 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HSha512", v =>
            {
                // FIPS 180 标准测试向量 SHA512("abc")
                v.Ok("SHA512(abc) 标准向量",
                    HSha512.Hash("abc") == "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f");

                // 摘要长度（512 位 = 128 hex）
                v.Ok("SHA512 摘要长度为 128 hex", HSha512.Hash("abc").Length == 128);

                // 确定性
                string zh = "中文摘要测试 abc";
                string d1 = HSha512.Hash(zh);
                v.Ok("SHA512 同输入摘要确定", d1 == HSha512.Hash(zh));
                v.Ok("SHA512 不同输入摘要不同", HSha512.Hash(zh + "x") != d1);

                // Verify 正负例
                v.Ok("SHA512 正确文本 Verify 通过", HSha512.Verify(zh, d1));
                v.Ok("SHA512 错误文本 Verify 失败", !HSha512.Verify("别的文本", d1));
                v.Ok("SHA512 大写摘要兼容", HSha512.Verify(zh, d1.ToUpperInvariant()));

                // 字节 API + 文件 API 一致
                byte[] data = Encoding.UTF8.GetBytes(zh);
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "sha512.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("SHA512 文件摘要与字节摘要一致",
                        HSha512.HashFile(file) == HHex.Encode(HSha512.Hash(data)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
