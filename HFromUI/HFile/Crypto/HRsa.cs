using System;
using System.Security.Cryptography;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// RSA 非对称加解密 + 数字签名（.NET Framework 内置 RSA，无第三方 DLL）。
    /// 公钥加密、私钥解密；私钥签名、公钥验签。加密解密/签名验签成对，同居于本文件。
    /// 密钥以 XML 字符串导入导出（<see cref="ExportPublicKey"/> 可公开分发，<see cref="ExportPrivateKey"/> 必须保密）。
    /// 注意：RSA 只能加密很短的数据（2048 位密钥 + PKCS#1 v1.5 最多 245 字节），
    /// 加密大文件请用“AES 加密数据 + RSA 加密 AES 密钥”的混合方案。
    /// </summary>
    public sealed class HRsa : IDisposable
    {
        /// <summary>内部 RSA 算法对象。</summary>
        private readonly RSA rsa;

        /// <summary>是否持有私钥（仅公钥时为 false，不能解密/签名）。</summary>
        private readonly bool hasPrivateKey;

        /// <summary>
        /// 生成新的 RSA 密钥对。
        /// </summary>
        /// <param name="keySize">密钥位数，默认 2048（推荐），可选 1024/2048/4096 等。</param>
        public HRsa(int keySize = 2048)
        {
            rsa = RSA.Create(keySize);
            hasPrivateKey = true;
        }

        /// <summary>内部构造：从已有 RSA 对象包装。</summary>
        private HRsa(RSA rsa, bool hasPrivateKey)
        {
            this.rsa = rsa;
            this.hasPrivateKey = hasPrivateKey;
        }

        /// <summary>是否持有私钥。</summary>
        public bool HasPrivateKey
        {
            get { return hasPrivateKey; }
        }

        /// <summary>密钥位数。</summary>
        public int KeySize
        {
            get { return rsa.KeySize; }
        }

        /// <summary>
        /// 从 XML 密钥创建实例：可传入完整私钥 XML（含公钥）或仅公钥 XML。
        /// </summary>
        /// <param name="xml">ToXmlString(true/false) 格式的密钥 XML。</param>
        /// <returns>HRsa 实例。</returns>
        public static HRsa FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            RSA rsa = RSA.Create();
            rsa.FromXmlString(xml);

            // XML 中含 D 节点即私钥
            bool privateKey = xml.IndexOf("<D>", StringComparison.OrdinalIgnoreCase) >= 0;
            return new HRsa(rsa, privateKey);
        }

        /// <summary>导出公钥 XML（可公开分发）。</summary>
        /// <returns>公钥 XML 字符串。</returns>
        public string ExportPublicKey()
        {
            return rsa.ToXmlString(false);
        }

        /// <summary>导出私钥 XML（包含公钥，必须严格保密）。</summary>
        /// <returns>私钥 XML 字符串。</returns>
        public string ExportPrivateKey()
        {
            if (!hasPrivateKey)
            {
                throw new CryptographicException("当前实例仅持有公钥，无法导出私钥。");
            }

            return rsa.ToXmlString(true);
        }

        /// <summary>
        /// 用公钥加密字节数据（PKCS#1 v1.5 填充）。
        /// </summary>
        /// <param name="data">明文，长度不得超过 密钥字节数 - 11。</param>
        /// <returns>密文字节。</returns>
        public byte[] Encrypt(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        /// <summary>
        /// 用私钥解密字节数据。
        /// </summary>
        /// <param name="cipher"><see cref="Encrypt"/> 产生的密文。</param>
        /// <returns>明文字节。</returns>
        public byte[] Decrypt(byte[] cipher)
        {
            if (!hasPrivateKey)
            {
                throw new CryptographicException("当前实例仅持有公钥，无法解密。");
            }

            if (cipher == null)
            {
                throw new ArgumentNullException("cipher");
            }

            return rsa.Decrypt(cipher, RSAEncryptionPadding.Pkcs1);
        }

        /// <summary>用公钥加密字符串，输出 Base64 文本。</summary>
        /// <param name="plainText">明文（长度受限，见类注释）。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>Base64 密文。</returns>
        public string EncryptString(string plainText, Encoding encoding = null)
        {
            if (plainText == null)
            {
                throw new ArgumentNullException("plainText");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Convert.ToBase64String(Encrypt(encoding.GetBytes(plainText)));
        }

        /// <summary>用私钥解密字符串。</summary>
        /// <param name="cipherBase64">Base64 密文。</param>
        /// <param name="encoding">明文编码，默认 UTF-8。</param>
        /// <returns>明文字符串。</returns>
        public string DecryptString(string cipherBase64, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return encoding.GetString(Decrypt(Convert.FromBase64String(cipherBase64)));
        }

        /// <summary>
        /// 用私钥对数据签名（SHA-256 摘要 + PKCS#1 v1.5 签名填充）。
        /// </summary>
        /// <param name="data">待签名数据。</param>
        /// <returns>签名字节。</returns>
        public byte[] Sign(byte[] data)
        {
            if (!hasPrivateKey)
            {
                throw new CryptographicException("当前实例仅持有公钥，无法签名。");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 用公钥验证签名。
        /// </summary>
        /// <param name="data">原始数据。</param>
        /// <param name="signature"><see cref="Sign"/> 产生的签名。</param>
        /// <returns>签名有效返回 true；数据或签名被篡改返回 false。</returns>
        public bool Verify(byte[] data, byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }

            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>用私钥对字符串签名，输出 Base64 文本。</summary>
        /// <param name="text">待签名文本。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>Base64 签名。</returns>
        public string SignString(string text, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Convert.ToBase64String(Sign(encoding.GetBytes(text)));
        }

        /// <summary>用公钥验证文本的 Base64 签名。</summary>
        /// <param name="text">原始文本。</param>
        /// <param name="signatureBase64">Base64 签名。</param>
        /// <param name="encoding">文本编码，默认 UTF-8。</param>
        /// <returns>签名是否有效。</returns>
        public bool VerifyString(string text, string signatureBase64, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Verify(encoding.GetBytes(text), Convert.FromBase64String(signatureBase64));
        }

        /// <summary>释放内部 RSA 资源。</summary>
        public void Dispose()
        {
            rsa.Dispose();
        }
    }
}
