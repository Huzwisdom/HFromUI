using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HFromUI.HConvert;

namespace HFromUI.HFile.Crypto
{
    /// <summary>对称算法“字符串加密”委托。</summary>
    /// <param name="plainText">明文。</param>
    /// <param name="password">密码。</param>
    /// <returns>Base64 密文。</returns>
    internal delegate string StringEncrypt(string plainText, string password);

    /// <summary>对称算法“字符串解密”委托。</summary>
    /// <param name="cipherBase64">Base64 密文。</param>
    /// <param name="password">密码。</param>
    /// <returns>明文。</returns>
    internal delegate string StringDecrypt(string cipherBase64, string password);

    /// <summary>对称算法“尝试字符串解密”委托。</summary>
    /// <param name="cipherBase64">Base64 密文。</param>
    /// <param name="password">密码。</param>
    /// <param name="plainText">成功时输出明文，失败时为 null。</param>
    /// <returns>是否成功。</returns>
    internal delegate bool StringTryDecrypt(string cipherBase64, string password, out string plainText);

    /// <summary>对称算法“字节加密”委托。</summary>
    /// <param name="data">明文字节。</param>
    /// <param name="password">密码。</param>
    /// <returns>密文包。</returns>
    internal delegate byte[] BytesEncrypt(byte[] data, string password);

    /// <summary>对称算法“字节解密”委托。</summary>
    /// <param name="cipher">密文包。</param>
    /// <param name="password">密码。</param>
    /// <returns>明文字节。</returns>
    internal delegate byte[] BytesDecrypt(byte[] cipher, string password);

    /// <summary>对称算法“文件加密”委托。</summary>
    /// <param name="sourceFilePath">源文件。</param>
    /// <param name="targetFilePath">目标文件。</param>
    /// <param name="password">密码。</param>
    internal delegate void FileEncrypt(string sourceFilePath, string targetFilePath, string password);

    /// <summary>对称算法“文件解密”委托。</summary>
    /// <param name="sourceFilePath">源文件。</param>
    /// <param name="targetFilePath">目标文件。</param>
    /// <param name="password">密码。</param>
    internal delegate void FileDecrypt(string sourceFilePath, string targetFilePath, string password);

    /// <summary>
    /// 加解密自测基础设施：统一统计/打印每个验证项结果，并提供对称算法通用验证套件、
    /// 临时目录管理、字节比较等公共能力。仅供本文件夹下各 *Verify 类调用。
    /// </summary>
    internal sealed class HCryptoVerifier
    {
        /// <summary>本组验证名称。</summary>
        private readonly string title;

        /// <summary>通过项数。</summary>
        private int passed;

        /// <summary>失败项数。</summary>
        private int failed;

        /// <summary>构造。</summary>
        /// <param name="title">本组验证名称。</param>
        private HCryptoVerifier(string title)
        {
            this.title = title;
        }

        /// <summary>
        /// 执行一组验证：打印标题、调用检查逻辑、汇总通过/失败数量。
        /// 检查过程中抛出的未预期异常会记为一项失败而不会中断其他算法的验证。
        /// </summary>
        /// <param name="title">验证组名称（如 HAes）。</param>
        /// <param name="check">具体检查逻辑，通过 Ok 记录每一项。</param>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run(string title, Action<HCryptoVerifier> check)
        {
            var v = new HCryptoVerifier(title);
            Console.WriteLine();
            Console.WriteLine("==== " + title + " ====");
            try
            {
                check(v);
            }
            catch (Exception ex)
            {
                v.Ok(title + " 验证过程未抛出未预期异常（实际抛出：" + ex.GetType().Name + " " + ex.Message + "）", false);
            }

            Console.WriteLine("---- " + title + " 小计：通过 " + v.passed + " / 失败 " + v.failed + " ----");
            return v.failed == 0;
        }

        /// <summary>记录一个验证项的结果并打印。</summary>
        /// <param name="name">验证项名称。</param>
        /// <param name="result">是否通过。</param>
        public void Ok(string name, bool result)
        {
            if (result)
            {
                passed++;
                Console.WriteLine("[PASS] " + name);
            }
            else
            {
                failed++;
                Console.WriteLine("[FAIL] " + name);
            }
        }

        /// <summary>创建本次验证专用的临时目录。</summary>
        /// <returns>临时目录绝对路径。</returns>
        public static string CreateTempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "hcrypto_verify_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        /// <summary>删除临时目录（忽略清理失败）。</summary>
        /// <param name="dir">临时目录路径。</param>
        public static void CleanupTempDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch
            {
                // 临时文件清理失败不影响验证结论
            }
        }

        /// <summary>逐字节比较两个数组是否完全一致。</summary>
        /// <param name="a">数组 a。</param>
        /// <param name="b">数组 b。</param>
        /// <returns>长度与每个字节都相同返回 true。</returns>
        public static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return a == null && b == null;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 对称算法（AES/DES/3DES/RC2）通用验证套件：
        /// 中文往返、空串往返、随机盐导致密文不同、错误密码、密文篡改、垃圾数据、文件流式往返等。
        /// </summary>
        /// <param name="v">验证记录器。</param>
        /// <param name="name">算法名称（用于显示）。</param>
        /// <param name="encryptString">字符串加密方法。</param>
        /// <param name="decryptString">字符串解密方法。</param>
        /// <param name="tryDecryptString">尝试字符串解密方法。</param>
        /// <param name="encryptBytes">字节加密方法。</param>
        /// <param name="decryptBytes">字节解密方法。</param>
        /// <param name="encryptFile">文件加密方法。</param>
        /// <param name="decryptFile">文件解密方法。</param>
        public static void SymmetricSuite(
            HCryptoVerifier v,
            string name,
            StringEncrypt encryptString,
            StringDecrypt decryptString,
            StringTryDecrypt tryDecryptString,
            BytesEncrypt encryptBytes,
            BytesDecrypt decryptBytes,
            FileEncrypt encryptFile,
            FileDecrypt decryptFile)
        {
            const string password = "P@ss验证2026";
            string plain = "加解密自测：中文/English/123 !@#$%^&*()_+-= 末尾";

            // 1. 字符串往返
            string cipher = encryptString(plain, password);
            v.Ok(name + " 中文字符串加解密往返", decryptString(cipher, password) == plain);

            // 2. 空串往返
            string cipherEmpty = encryptString(string.Empty, password);
            v.Ok(name + " 空字符串往返", decryptString(cipherEmpty, password) == string.Empty);

            // 3. 随机盐/IV：同明文同密码两次密文必须不同，且都能解开
            string c1 = encryptString(plain, password);
            string c2 = encryptString(plain, password);
            v.Ok(name + " 随机盐IV使两次密文不同", c1 != c2);
            v.Ok(name + " 两次密文均可解密",
                decryptString(c1, password) == plain && decryptString(c2, password) == plain);

            // 4. 错误密码：TryDecrypt 返回 false
            string wrongPlain;
            v.Ok(name + " 错误密码 TryDecrypt 返回 false",
                !tryDecryptString(c1, "wrong-password", out wrongPlain) && wrongPlain == null);

            // 5. 错误密码：直接解密抛 CryptographicException
            bool threw = false;
            try
            {
                decryptString(c1, "wrong-password");
            }
            catch (CryptographicException)
            {
                threw = true;
            }

            v.Ok(name + " 错误密码直解抛 CryptographicException", threw);

            // 6. 密文被篡改
            byte[] raw = Convert.FromBase64String(c1);
            raw[raw.Length - 1] ^= 0xFF;
            string tampered = Convert.ToBase64String(raw);
            string tamperedPlain;
            v.Ok(name + " 篡改密文解密失败", !tryDecryptString(tampered, password, out tamperedPlain));

            // 7. 垃圾数据/包头缺失
            string garbage = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            string garbagePlain;
            v.Ok(name + " 垃圾数据解密失败", !tryDecryptString(garbage, password, out garbagePlain));

            // 8. 字节数组往返
            byte[] data = Encoding.UTF8.GetBytes(plain);
            v.Ok(name + " 字节数组往返", BytesEqual(decryptBytes(encryptBytes(data, password), password), data));

            // 9. 文件流式往返（约 50KB 随机二进制 + 零碎尾字节，验证不丢字节）
            string dir = CreateTempDir();
            try
            {
                byte[] fileData = HCryptoCore.RandomBytes(50 * 1024 + 123);
                string src = Path.Combine(dir, "plain.bin");
                string enc = Path.Combine(dir, "plain.enc");
                string dec = Path.Combine(dir, "dec.bin");
                File.WriteAllBytes(src, fileData);

                encryptFile(src, enc, password);
                decryptFile(enc, dec, password);
                v.Ok(name + " 文件流式加解密字节一致", BytesEqual(File.ReadAllBytes(dec), fileData));
                v.Ok(name + " 密文文件不同于明文", !BytesEqual(File.ReadAllBytes(enc), fileData));

                bool fileThrew = false;
                try
                {
                    decryptFile(enc, Path.Combine(dir, "bad.bin"), "bad-pwd");
                }
                catch (CryptographicException)
                {
                    fileThrew = true;
                }

                v.Ok(name + " 文件错误密码抛异常", fileThrew);
            }
            finally
            {
                CleanupTempDir(dir);
            }
        }

        /// <summary>把若干随机/指定字节按顺序装入一个数组（用 HBytes 组织验证数据）。</summary>
        /// <param name="arrays">待拼接的字节数组。</param>
        /// <returns>拼接结果。</returns>
        public static byte[] CombineBytes(params byte[][] arrays)
        {
            var all = new HBytes();
            foreach (byte[] array in arrays)
            {
                all.Add(array);
            }

            return all.Bytes.ToArray();
        }
    }
}
