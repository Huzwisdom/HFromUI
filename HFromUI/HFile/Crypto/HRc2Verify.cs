namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HRc2"/> 算法自测验证类。
    /// 覆盖字符串/字节/文件加解密往返、随机盐 IV 唯一性、错误密码、密文篡改、垃圾数据等场景。
    /// 调用方式：<c>HRc2Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HRc2Verify
    {
        /// <summary>运行 RC2 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HRc2", v =>
            {
                HCryptoVerifier.SymmetricSuite(
                    v,
                    "RC2",
                    (t, p) => HRc2.Encrypt(t, p),
                    (c, p) => HRc2.Decrypt(c, p),
                    (string c, string p, out string o) => HRc2.TryDecrypt(c, p, out o),
                    HRc2.Encrypt,
                    HRc2.Decrypt,
                    HRc2.EncryptFile,
                    HRc2.DecryptFile);
            });
        }
    }
}
