using System;
using System.Security.Cryptography;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HRsa"/> 非对称加解密与数字签名自测验证类。
    /// 覆盖密钥对生成与 XML 导入导出、公钥加密/私钥解密、密钥分离使用、
    /// 公钥不能解密/签名、SHA-256 签名/验签与篡改检测、1024/2048 位密钥可用性。
    /// 调用方式：<c>HRsaVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HRsaVerify
    {
        /// <summary>运行 RSA 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HRsa", v =>
            {
                // 1. 生成 2048 位密钥对并检查公/私钥 XML 形态
                using (var rsa = new HRsa(2048))
                {
                    string publicXml = rsa.ExportPublicKey();
                    string privateXml = rsa.ExportPrivateKey();
                    v.Ok("RSA 密钥位数为 2048", rsa.KeySize == 2048);
                    v.Ok("RSA 新实例持有私钥", rsa.HasPrivateKey);
                    v.Ok("RSA 公钥 XML 不含私钥节点 <D>", publicXml.IndexOf("<D>", StringComparison.OrdinalIgnoreCase) < 0);
                    v.Ok("RSA 私钥 XML 含私钥节点 <D>", privateXml.IndexOf("<D>", StringComparison.OrdinalIgnoreCase) >= 0);

                    // 2. 同实例公钥加密、私钥解密（短数据，受 RSA 长度限制）
                    string plain = "RSA 非对称加解密 abc 123";
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plain);
                    byte[] cipherBytes = rsa.Encrypt(plainBytes);
                    v.Ok("RSA 字节加解密往返", HCryptoVerifier.BytesEqual(rsa.Decrypt(cipherBytes), plainBytes));

                    // 3. 字符串 API 往返
                    string cipherText = rsa.EncryptString(plain);
                    v.Ok("RSA 字符串加解密往返", rsa.DecryptString(cipherText) == plain);

                    // 4. 密文必须与明文不同
                    v.Ok("RSA 密文不同于明文", !HCryptoVerifier.BytesEqual(cipherBytes, plainBytes));

                    // 5. 签名 + 验签（SHA-256）
                    byte[] signature = rsa.Sign(plainBytes);
                    v.Ok("RSA 正确签名验签通过", rsa.Verify(plainBytes, signature));

                    byte[] tampered = (byte[])plainBytes.Clone();
                    tampered[0] ^= 0x01;
                    v.Ok("RSA 数据被篡改验签失败", !rsa.Verify(tampered, signature));

                    byte[] badSig = (byte[])signature.Clone();
                    badSig[0] ^= 0xFF;
                    v.Ok("RSA 签名被篡改验签失败", !rsa.Verify(plainBytes, badSig));

                    // 6. 字符串签名 API
                    string sigText = rsa.SignString(plain);
                    v.Ok("RSA 字符串签名验签往返", rsa.VerifyString(plain, sigText));
                    v.Ok("RSA 字符串篡改验签失败", !rsa.VerifyString(plain + "x", sigText));

                    // 7. 密钥分离：只拿到公钥的一方负责加密/验签，私钥一方解密/签名
                    using (HRsa publicOnly = HRsa.FromXml(publicXml))
                    {
                        v.Ok("RSA 公钥实例无私钥", !publicOnly.HasPrivateKey);

                        byte[] remoteCipher = publicOnly.Encrypt(plainBytes);
                        v.Ok("RSA 公钥加密→私钥解密", HCryptoVerifier.BytesEqual(rsa.Decrypt(remoteCipher), plainBytes));
                        v.Ok("RSA 公钥验签私钥签名", publicOnly.Verify(plainBytes, signature));

                        bool noDecrypt = false;
                        bool noSign = false;
                        try { publicOnly.Decrypt(remoteCipher); }
                        catch (CryptographicException) { noDecrypt = true; }

                        try { publicOnly.Sign(plainBytes); }
                        catch (CryptographicException) { noSign = true; }

                        v.Ok("RSA 公钥不能解密", noDecrypt);
                        v.Ok("RSA 公钥不能签名", noSign);
                    }

                    // 8. 从私钥 XML 恢复的实例同样可解密/签名
                    using (HRsa restored = HRsa.FromXml(privateXml))
                    {
                        v.Ok("RSA 私钥 XML 重建后可解密", HCryptoVerifier.BytesEqual(restored.Decrypt(cipherBytes), plainBytes));
                        v.Ok("RSA 私钥 XML 重建后可验签自身签名", restored.Verify(plainBytes, signature));
                    }
                }

                // 9. 1024 位密钥也可正常工作
                using (var rsa1024 = new HRsa(1024))
                {
                    byte[] small = Encoding.UTF8.GetBytes("short");
                    v.Ok("RSA-1024 加解密可用",
                        HCryptoVerifier.BytesEqual(rsa1024.Decrypt(rsa1024.Encrypt(small)), small));
                }
            });
        }
    }
}
