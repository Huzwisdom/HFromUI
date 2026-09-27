namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HAes"/> 算法自测验证类。
    /// 覆盖字符串/字节/文件加解密往返、随机盐 IV 唯一性、错误密码、密文篡改、垃圾数据等场景。
    /// 调用方式：<c>HAesVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HAesVerify
    {
        /// <summary>运行 AES 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HAes", v =>
            {
                HCryptoVerifier.SymmetricSuite(
                    v,
                    "AES",
                    (t, p) => HAes.Encrypt(t, p),
                    (c, p) => HAes.Decrypt(c, p),
                    (string c, string p, out string o) => HAes.TryDecrypt(c, p, out o),
                    HAes.Encrypt,
                    HAes.Decrypt,
                    HAes.EncryptFile,
                    HAes.DecryptFile);
            });
        }
    }
}
