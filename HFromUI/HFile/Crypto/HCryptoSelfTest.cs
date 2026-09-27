using System;
using System.Collections.Generic;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// 加解密全套算法自测总入口：顺序运行本文件夹下全部 19 个算法验证类，
    /// 并额外验证“不同算法的密文包不能互相解密”（魔术头隔离）。
    /// 用法：<c>HCryptoSelfTest.Run()</c>；全部通过返回 true，详细明细输出到控制台。
    /// 各算法也可单独调用对应的 <c>XxxVerify.Run()</c>。
    /// </summary>
    public static class HCryptoSelfTest
    {
        /// <summary>运行全部算法验证与跨算法隔离验证。</summary>
        /// <returns>所有算法、所有验证项全部通过返回 true；任一失败返回 false。</returns>
        public static bool Run()
        {
            Console.WriteLine("############################################");
            Console.WriteLine("# HFromUI.HFile.Crypto 加解密全套自测开始 #");
            Console.WriteLine("############################################");

            // 算法名 → 该算法验证入口（每一个对应一个独立的 *Verify.cs 文件）
            var cases = new List<KeyValuePair<string, Func<bool>>>
            {
                new KeyValuePair<string, Func<bool>>("HAes", HAesVerify.Run),
                new KeyValuePair<string, Func<bool>>("HDES", HDESVerify.Run),
                new KeyValuePair<string, Func<bool>>("HTripleDES", HTripleDESVerify.Run),
                new KeyValuePair<string, Func<bool>>("HRc2", HRc2Verify.Run),
                new KeyValuePair<string, Func<bool>>("HXor", HXorVerify.Run),
                new KeyValuePair<string, Func<bool>>("HRsa", HRsaVerify.Run),
                new KeyValuePair<string, Func<bool>>("HMd5", HMd5Verify.Run),
                new KeyValuePair<string, Func<bool>>("HSha1", HSha1Verify.Run),
                new KeyValuePair<string, Func<bool>>("HSha256", HSha256Verify.Run),
                new KeyValuePair<string, Func<bool>>("HSha384", HSha384Verify.Run),
                new KeyValuePair<string, Func<bool>>("HSha512", HSha512Verify.Run),
                new KeyValuePair<string, Func<bool>>("HHmacMd5", HHmacMd5Verify.Run),
                new KeyValuePair<string, Func<bool>>("HHmacSha1", HHmacSha1Verify.Run),
                new KeyValuePair<string, Func<bool>>("HHmacSha256", HHmacSha256Verify.Run),
                new KeyValuePair<string, Func<bool>>("HHmacSha384", HHmacSha384Verify.Run),
                new KeyValuePair<string, Func<bool>>("HHmacSha512", HHmacSha512Verify.Run),
                new KeyValuePair<string, Func<bool>>("HBase64", HBase64Verify.Run),
                new KeyValuePair<string, Func<bool>>("HHex", HHexVerify.Run),
                new KeyValuePair<string, Func<bool>>("HCryptoRandom", HCryptoRandomVerify.Run)
            };

            int passed = 0;
            int failed = 0;
            var failedNames = new List<string>();

            foreach (KeyValuePair<string, Func<bool>> item in cases)
            {
                bool ok;
                try
                {
                    ok = item.Value();
                }
                catch (Exception ex)
                {
                    ok = false;
                    Console.WriteLine("[ERROR] " + item.Key + " 验证入口抛出未预期异常：" + ex);
                }

                if (ok)
                {
                    passed++;
                }
                else
                {
                    failed++;
                    failedNames.Add(item.Key);
                }
            }

            // 跨算法魔术头隔离：A 算法的密文交给 B 算法必须解密失败，防止误当同格式处理
            bool cross = CrossAlgorithmCheck();

            Console.WriteLine();
            Console.WriteLine("############################################");
            Console.WriteLine("# 全套结果：算法组通过 " + passed + " / 失败 " + failed + "；跨算法隔离 " + (cross ? "通过" : "失败"));
            if (failedNames.Count > 0)
            {
                Console.WriteLine("# 失败算法组：" + string.Join(", ", failedNames.ToArray()));
            }

            Console.WriteLine("# 总体结论：" + (failed == 0 && cross ? "全部通过" : "存在失败"));
            Console.WriteLine("############################################");
            return failed == 0 && cross;
        }

        /// <summary>
        /// 跨算法隔离验证：每种对称算法只能解自己的密文包，
        /// 拿到其他算法的密文（或 XOR 包、垃圾数据）必须安全失败。
        /// </summary>
        /// <returns>隔离全部成立返回 true。</returns>
        private static bool CrossAlgorithmCheck()
        {
            Console.WriteLine();
            Console.WriteLine("==== 跨算法魔术头隔离 ====");
            bool all = true;
            const string password = "cross-check-pwd";
            string plain = "跨算法隔离测试文本 abc";

            string aesCipher = HAes.Encrypt(plain, password);
            string desCipher = HDES.Encrypt(plain, password);
            string tdesCipher = HTripleDES.Encrypt(plain, password);
            string rc2Cipher = HRc2.Encrypt(plain, password);
            string xorCipher = HXor.Encrypt(plain, "xor-key");
            string garbage = Convert.ToBase64String(Encoding.UTF8.GetBytes("这不是任何密文包"));

            // AES 的密文交给其它三个对称算法
            all &= ExpectFail("AES密文->DES 拒绝", () => HDES.TryDecrypt(aesCipher, password, out string s1));
            all &= ExpectFail("AES密文->3DES 拒绝", () => HTripleDES.TryDecrypt(aesCipher, password, out string s2));
            all &= ExpectFail("AES密文->RC2 拒绝", () => HRc2.TryDecrypt(aesCipher, password, out string s3));
            all &= ExpectFail("AES密文->AES错密码 拒绝", () => HAes.TryDecrypt(aesCipher, "bad", out string s4));

            // DES/3DES/RC2 的密文交给 AES
            all &= ExpectFail("DES密文->AES 拒绝", () => HAes.TryDecrypt(desCipher, password, out string s5));
            all &= ExpectFail("3DES密文->AES 拒绝", () => HAes.TryDecrypt(tdesCipher, password, out string s6));
            all &= ExpectFail("RC2密文->AES 拒绝", () => HAes.TryDecrypt(rc2Cipher, password, out string s7));

            // XOR 包交给 AES；AES 包交给 XOR
            all &= ExpectFail("XOR密文->AES 拒绝", () => HAes.TryDecrypt(xorCipher, password, out string s8));
            all &= ExpectFail("AES密文->XOR 拒绝", () =>
            {
                try
                {
                    HXor.Decrypt(aesCipher, password);
                    return true;
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    return false;
                }
            });

            // 垃圾数据所有算法拒绝
            all &= ExpectFail("垃圾数据->AES 拒绝", () => HAes.TryDecrypt(garbage, password, out string s9));
            all &= ExpectFail("垃圾数据->DES 拒绝", () => HDES.TryDecrypt(garbage, password, out string s10));
            all &= ExpectFail("垃圾数据->XOR 拒绝", () => HXor.TryDecrypt(garbage, "xor-key", out string s11));

            Console.WriteLine("---- 跨算法隔离小计：" + (all ? "全部通过" : "存在失败") + " ----");
            return all;
        }

        /// <summary>执行一次“应当解密失败”的检查并打印结果。</summary>
        /// <param name="name">检查项名称。</param>
        /// <param name="shouldFail">返回 true 表示错误地解密成功（失败项），false 表示正确拒绝。</param>
        /// <returns>被正确拒绝返回 true。</returns>
        private static bool ExpectFail(string name, Func<bool> shouldFail)
        {
            bool accepted;
            try
            {
                accepted = shouldFail();
            }
            catch (Exception)
            {
                // 抛出拒绝异常同样视为正确拒绝
                accepted = false;
            }

            bool result = !accepted;
            Console.WriteLine((result ? "[PASS] " : "[FAIL] ") + name);
            return result;
        }
    }
}
