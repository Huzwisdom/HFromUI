using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HAdler32"/> 校验类自测：RFC 1950 标准向量、空数据、与 CRC-32 区分、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、跨缓冲大文件流式一致性。
    /// 调用方式：<c>HAdler32Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HAdler32Verify
    {
        /// <summary>运行 Adler-32 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HAdler32", v =>
            {
                // RFC 1950 / zlib 标准向量
                v.Ok("Adler-32(123456789) 标准向量 0x091E01DE",
                    HAdler32.Compute(Encoding.ASCII.GetBytes("123456789")) == 0x091E01DE);
                v.Ok("Adler-32 空数据为 0x00000001（A=1,B=0）",
                    HAdler32.Compute(new byte[0]) == 0x00000001);

                // 与 CRC-32 是不同算法
                byte[] vector = Encoding.ASCII.GetBytes("123456789");
                v.Ok("Adler-32 与 CRC-32 结果不同",
                    HAdler32.Compute(vector) != HCrc32.Compute(vector));

                // 手工小向量：单字节 0xFF → A=256,B=256 → 0x01000100
                v.Ok("Adler-32 单字节 0xFF 手工向量 0x01000100",
                    HAdler32.Compute(new byte[] { 0xFF }) == 0x01000100);

                // 确定性与中文
                string zh = "zlib 流与 PNG 数据块的配套校验和";
                uint c1 = HAdler32.Compute(zh);
                v.Ok("Adler-32 同输入结果确定", c1 == HAdler32.Compute(zh));
                v.Ok("Adler-32 不同输入结果不同", HAdler32.Compute(zh + "x") != c1);
                v.Ok("Adler-32 中文与 UTF8 字节计算一致",
                    c1 == HAdler32.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("Adler-32 数值 Verify 通过", HAdler32.Verify(data, c1));
                v.Ok("Adler-32 错误期望值 Verify 失败", !HAdler32.Verify(data, c1 ^ 0xFFFFFFFF));
                v.Ok("Adler-32 Hex Verify 小写通过", HAdler32.Verify(data, HAdler32.ComputeHex(data)));
                v.Ok("Adler-32 Hex Verify 大写通过", HAdler32.Verify(data, HAdler32.ComputeHex(data).ToUpperInvariant()));
                v.Ok("Adler-32 非法 Hex Verify 返回 false", !HAdler32.Verify(data, "zzzzzzzz"));

                // GetBytes 大端
                byte[] bytes = HAdler32.GetBytes(c1);
                v.Ok("Adler-32 GetBytes 大端 4 字节",
                    bytes.Length == 4
                    && bytes[0] == (byte)(c1 >> 24) && bytes[1] == (byte)(c1 >> 16)
                    && bytes[2] == (byte)(c1 >> 8) && bytes[3] == (byte)(c1 & 0xFF));

                // 文件（数据量远超 MaxBlock 分块上限，验证分块取模正确）
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "adler.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("Adler-32 文件校验与内存一致", HAdler32.ComputeFile(file) == c1);
                    v.Ok("Adler-32 文件 VerifyFile 通过", HAdler32.VerifyFile(file, c1));

                    byte[] big = HCryptoCore.RandomBytes(120 * 1024 + 31);
                    uint bigValue = HAdler32.Compute(big);
                    string bigFile = Path.Combine(dir, "adler_big.bin");
                    File.WriteAllBytes(bigFile, big);
                    v.Ok("Adler-32 大文件（跨分块/缓冲）与内存一致", HAdler32.ComputeFile(bigFile) == bigValue);

                    byte[] tampered = (byte[])big.Clone();
                    tampered[tampered.Length / 2] ^= 0x7F;
                    string bad = Path.Combine(dir, "adler_bad.bin");
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("Adler-32 篡改后 VerifyFile 失败", !HAdler32.VerifyFile(bad, bigValue));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
