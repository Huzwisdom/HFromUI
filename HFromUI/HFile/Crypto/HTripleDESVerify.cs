namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HTripleDES"/> 算法自测验证类。
    /// 覆盖字符串/字节/文件加解密往返、随机盐 IV 唯一性、错误密码、密文篡改、垃圾数据等场景。
    /// 调用方式：<c>HTripleDESVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HTripleDESVerify
    {
        /// <summary>运行 TripleDES 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HTripleDES", v =>
            {
                HCryptoVerifier.SymmetricSuite(
                    v,
                    "3DES",
                    (t, p) => HTripleDES.Encrypt(t, p),
                    (c, p) => HTripleDES.Decrypt(c, p),
                    (string c, string p, out string o) => HTripleDES.TryDecrypt(c, p, out o),
                    HTripleDES.Encrypt,
                    HTripleDES.Decrypt,
                    HTripleDES.EncryptFile,
                    HTripleDES.DecryptFile);
            });
        }
    }
}
