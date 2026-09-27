using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HXor"/> 异或加解密自测验证类。
    /// 覆盖字符串往返、异或自逆特性、手工已知向量、错误密钥长度、垃圾数据、文件流式往返。
    /// 调用方式：<c>HXorVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HXorVerify
    {
        /// <summary>运行 XOR 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HXor", v =>
            {
                const string key = "Xor密钥#1";
                string plain = "异或自逆测试：abc 中文 123 !@#";

                // 1. 字符串往返
                string cipher = HXor.Encrypt(plain, key);
                v.Ok("XOR 字符串加解密往返", HXor.Decrypt(cipher, key) == plain);

                // 2. XOR 自逆：加密与解密是同一个异或运算，裸数据连做两次必然还原。
                //    取出密文包正文，用 HBytes 重新包一个 HXOR 包头后再解密，即等价于“对正文再异或一次”。
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] plainBytes = Encoding.UTF8.GetBytes(plain);
                byte[] once = HXor.Encrypt(plainBytes, keyBytes);
                byte[] onceBody = new byte[once.Length - 8];
                Buffer.BlockCopy(once, 8, onceBody, 0, onceBody.Length);
                var repacket = new HBytes();
                repacket.Add(HCryptoCore.Magic("HXOR"));
                repacket.Add(keyBytes.Length);
                repacket.Add(onceBody);
                byte[] selfInverse = HXor.Decrypt(repacket.Bytes.ToArray(), keyBytes);
                v.Ok("XOR 自逆（裸异或连做两次还原）", HCryptoVerifier.BytesEqual(selfInverse, plainBytes));

                // 3. 手工已知向量：单字节密钥 0xAA，明文 0x00/0xFF/0x5A
                byte[] vectorKey = new byte[] { 0xAA };
                byte[] vectorPlain = new byte[] { 0x00, 0xFF, 0x5A };
                byte[] vectorExpected = new byte[] { 0xAA, 0x55, 0xF0 };
                // 去掉 HXOR 包头（4 魔术 + 4 密钥长度）后与手工结果比较
                byte[] vectorCipher = HXor.Encrypt(vectorPlain, vectorKey);
                byte[] vectorBody = new byte[vectorExpected.Length];
                Buffer.BlockCopy(vectorCipher, 8, vectorBody, 0, vectorBody.Length);
                v.Ok("XOR 手工已知向量（key=0xAA）", HCryptoVerifier.BytesEqual(vectorBody, vectorExpected));

                // 4. 多字节循环密钥往返
                byte[] multiKey = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89 };
                byte[] multiBack = HXor.Decrypt(HXor.Encrypt(plainBytes, multiKey), multiKey);
                v.Ok("XOR 多字节循环密钥往返", HCryptoVerifier.BytesEqual(multiBack, plainBytes));

                // 5. 密钥长度不一致必须失败
                bool lengthThrew = false;
                try
                {
                    HXor.Decrypt(once, new byte[] { 0x01 });
                }
                catch (CryptographicException)
                {
                    lengthThrew = true;
                }

                v.Ok("XOR 密钥长度不一致抛异常", lengthThrew);

                // 6. 垃圾数据
                string garbageOut;
                v.Ok("XOR 垃圾数据 TryDecrypt 失败",
                    !HXor.TryDecrypt(Convert.ToBase64String(new byte[] { 1, 2, 3 }), key, out garbageOut));

                // 7. 空串往返
                v.Ok("XOR 空字符串往返", HXor.Decrypt(HXor.Encrypt(string.Empty, key), key) == string.Empty);

                // 8. 文件流式往返
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    byte[] fileData = HCryptoCore.RandomBytes(40 * 1024 + 77);
                    string src = Path.Combine(dir, "plain.bin");
                    string enc = Path.Combine(dir, "plain.xor");
                    string dec = Path.Combine(dir, "dec.bin");
                    File.WriteAllBytes(src, fileData);

                    HXor.EncryptFile(src, enc, keyBytes);
                    HXor.DecryptFile(enc, dec, keyBytes);
                    v.Ok("XOR 文件流式往返字节一致", HCryptoVerifier.BytesEqual(File.ReadAllBytes(dec), fileData));
                    v.Ok("XOR 密文文件不同于明文", !HCryptoVerifier.BytesEqual(File.ReadAllBytes(enc), fileData));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
