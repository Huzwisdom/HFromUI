namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HDES"/> 算法自测验证类。
    /// 覆盖字符串/字节/文件加解密往返、随机盐 IV 唯一性、错误密码、密文篡改、垃圾数据等场景。
    /// 调用方式：<c>HDESVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HDESVerify
    {
        /// <summary>运行 DES 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HDES", v =>
            {
                HCryptoVerifier.SymmetricSuite(
                    v,
                    "DES",
                    (t, p) => HDES.Encrypt(t, p),
                    (c, p) => HDES.Decrypt(c, p),
                    (string c, string p, out string o) => HDES.TryDecrypt(c, p, out o),
                    HDES.Encrypt,
                    HDES.Decrypt,
                    HDES.EncryptFile,
                    HDES.DecryptFile);
            });
        }
    }
}
