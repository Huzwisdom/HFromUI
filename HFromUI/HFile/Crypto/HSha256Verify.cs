using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HSha256"/> 单向散列自测验证类。
    /// 覆盖 FIPS 180 标准向量、确定性、中文摘要校验、错误文本负例、文件摘要与内存摘要一致性。
    /// 调用方式：<c>HSha256Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HSha256Verify
    {
        /// <summary>运行 SHA-256 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HSha256", v =>
            {
                // FIPS 180 标准测试向量 SHA256("abc")
                v.Ok("SHA256(abc) 标准向量",
                    HSha256.Hash("abc") == "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");

                // 摘要长度（256 位 = 64 hex）
                v.Ok("SHA256 摘要长度为 64 hex", HSha256.Hash("abc").Length == 64);

                // 确定性
                string zh = "中文摘要测试 abc";
                string d1 = HSha256.Hash(zh);
                v.Ok("SHA256 同输入摘要确定", d1 == HSha256.Hash(zh));
                v.Ok("SHA256 不同输入摘要不同", HSha256.Hash(zh + "x") != d1);

                // Verify 正负例
                v.Ok("SHA256 正确文本 Verify 通过", HSha256.Verify(zh, d1));
                v.Ok("SHA256 错误文本 Verify 失败", !HSha256.Verify("别的文本", d1));
                v.Ok("SHA256 大写摘要兼容", HSha256.Verify(zh, d1.ToUpperInvariant()));

                // 字节 API + 文件 API 一致
                byte[] data = Encoding.UTF8.GetBytes(zh);
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "sha256.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("SHA256 文件摘要与字节摘要一致",
                        HSha256.HashFile(file) == HHex.Encode(HSha256.Hash(data)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
