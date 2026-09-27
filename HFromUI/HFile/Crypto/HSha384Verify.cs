using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HSha384"/> 单向散列自测验证类。
    /// 覆盖 FIPS 180 标准向量、确定性、中文摘要校验、错误文本负例、文件摘要与内存摘要一致性。
    /// 调用方式：<c>HSha384Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HSha384Verify
    {
        /// <summary>运行 SHA-384 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HSha384", v =>
            {
                // FIPS 180 标准测试向量 SHA384("abc")
                v.Ok("SHA384(abc) 标准向量",
                    HSha384.Hash("abc") == "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7");

                // 摘要长度（384 位 = 96 hex）
                v.Ok("SHA384 摘要长度为 96 hex", HSha384.Hash("abc").Length == 96);

                // 确定性
                string zh = "中文摘要测试 abc";
                string d1 = HSha384.Hash(zh);
                v.Ok("SHA384 同输入摘要确定", d1 == HSha384.Hash(zh));
                v.Ok("SHA384 不同输入摘要不同", HSha384.Hash(zh + "x") != d1);

                // Verify 正负例
                v.Ok("SHA384 正确文本 Verify 通过", HSha384.Verify(zh, d1));
                v.Ok("SHA384 错误文本 Verify 失败", !HSha384.Verify("别的文本", d1));
                v.Ok("SHA384 大写摘要兼容", HSha384.Verify(zh, d1.ToUpperInvariant()));

                // 字节 API + 文件 API 一致
                byte[] data = Encoding.UTF8.GetBytes(zh);
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "sha384.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("SHA384 文件摘要与字节摘要一致",
                        HSha384.HashFile(file) == HHex.Encode(HSha384.Hash(data)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
