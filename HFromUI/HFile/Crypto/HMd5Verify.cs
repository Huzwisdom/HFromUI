using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HMd5"/> 单向散列自测验证类。
    /// 覆盖 RFC 1321 标准向量、确定性、中文摘要校验、错误文本负例、文件摘要与内存摘要一致性。
    /// 调用方式：<c>HMd5Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HMd5Verify
    {
        /// <summary>运行 MD5 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HMd5", v =>
            {
                // RFC 1321 标准测试向量 MD5("abc")
                v.Ok("MD5(abc) 标准向量",
                    HMd5.Hash("abc") == "900150983cd24fb0d6963f7d28e17f72");

                // 摘要长度（128 位 = 32 hex）
                v.Ok("MD5 摘要长度为 32 hex", HMd5.Hash("abc").Length == 32);

                // 确定性：同输入两次相同；不同输入不同
                string zh = "中文摘要测试 abc";
                string d1 = HMd5.Hash(zh);
                string d2 = HMd5.Hash(zh);
                v.Ok("MD5 同输入摘要确定", d1 == d2);
                v.Ok("MD5 不同输入摘要不同", HMd5.Hash(zh + "x") != d1);

                // Verify 正负例
                v.Ok("MD5 正确文本 Verify 通过", HMd5.Verify(zh, d1));
                v.Ok("MD5 错误文本 Verify 失败", !HMd5.Verify("别的文本", d1));

                // 大写摘要也可校验
                v.Ok("MD5 大写摘要兼容", HMd5.Verify(zh, d1.ToUpperInvariant()));

                // 字节 API + 文件 API 一致
                byte[] data = Encoding.UTF8.GetBytes(zh);
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "md5.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("MD5 文件摘要与字节摘要一致",
                        HMd5.HashFile(file) == HHex.Encode(HMd5.Hash(data)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
