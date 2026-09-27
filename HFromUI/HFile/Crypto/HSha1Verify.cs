using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HSha1"/> 单向散列自测验证类。
    /// 覆盖 FIPS 180 标准向量、确定性、中文摘要校验、错误文本负例、文件摘要与内存摘要一致性。
    /// 调用方式：<c>HSha1Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HSha1Verify
    {
        /// <summary>运行 SHA-1 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HSha1", v =>
            {
                // FIPS 180 标准测试向量 SHA1("abc")
                v.Ok("SHA1(abc) 标准向量",
                    HSha1.Hash("abc") == "a9993e364706816aba3e25717850c26c9cd0d89d");

                // 摘要长度（160 位 = 40 hex）
                v.Ok("SHA1 摘要长度为 40 hex", HSha1.Hash("abc").Length == 40);

                // 确定性
                string zh = "中文摘要测试 abc";
                string d1 = HSha1.Hash(zh);
                v.Ok("SHA1 同输入摘要确定", d1 == HSha1.Hash(zh));
                v.Ok("SHA1 不同输入摘要不同", HSha1.Hash(zh + "x") != d1);

                // Verify 正负例
                v.Ok("SHA1 正确文本 Verify 通过", HSha1.Verify(zh, d1));
                v.Ok("SHA1 错误文本 Verify 失败", !HSha1.Verify("别的文本", d1));
                v.Ok("SHA1 大写摘要兼容", HSha1.Verify(zh, d1.ToUpperInvariant()));

                // 字节 API + 文件 API 一致
                byte[] data = Encoding.UTF8.GetBytes(zh);
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "sha1.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("SHA1 文件摘要与字节摘要一致",
                        HSha1.HashFile(file) == HHex.Encode(HSha1.Hash(data)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
