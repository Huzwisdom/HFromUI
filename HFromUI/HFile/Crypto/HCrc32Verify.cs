using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc32"/> 校验类自测：CRC 目录标准向量、空数据、英文常见向量、
    /// 与 CRC-32C 多项式区分、中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、
    /// 文件流式（含跨缓冲大文件）与内存一致性、篡改拒绝。
    /// 调用方式：<c>HCrc32Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc32Verify
    {
        /// <summary>运行 CRC-32/ISO-HDLC 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc32", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-32(123456789) 标准向量 0xCBF43926",
                    HCrc32.Compute(Encoding.ASCII.GetBytes("123456789")) == 0xCBF43926);
                v.Ok("CRC-32 空数据为 0x00000000",
                    HCrc32.Compute(new byte[0]) == 0x00000000);

                // 广泛引用的英文句子向量
                v.Ok("CRC-32(The quick brown fox...) 向量 0x414FA339",
                    HCrc32.Compute("The quick brown fox jumps over the lazy dog", Encoding.ASCII) == 0x414FA339);

                // 与 CRC-32C 多项式不同
                byte[] vector = Encoding.ASCII.GetBytes("123456789");
                v.Ok("CRC-32 与 CRC-32C 结果不同",
                    HCrc32.Compute(vector) != HCrc32C.Compute(vector));

                // 确定性与中文
                string zh = "ZIP/PNG/zlib 常用 CRC-32 完整性校验";
                uint c1 = HCrc32.Compute(zh);
                v.Ok("CRC-32 同输入结果确定", c1 == HCrc32.Compute(zh));
                v.Ok("CRC-32 不同输入结果不同", HCrc32.Compute(zh + "x") != c1);
                v.Ok("CRC-32 中文与 UTF8 字节计算一致",
                    c1 == HCrc32.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-32 数值 Verify 通过", HCrc32.Verify(data, c1));
                v.Ok("CRC-32 错误期望值 Verify 失败", !HCrc32.Verify(data, c1 ^ 0xFFFFFFFF));
                v.Ok("CRC-32 Hex Verify 小写通过", HCrc32.Verify(data, HCrc32.ComputeHex(data)));
                v.Ok("CRC-32 Hex Verify 大写通过", HCrc32.Verify(data, HCrc32.ComputeHex(data).ToUpperInvariant()));
                v.Ok("CRC-32 非法 Hex Verify 返回 false", !HCrc32.Verify(data, "zzzzzzzz"));
                v.Ok("CRC-32 长度不符 Hex 返回 false", !HCrc32.Verify(data, "0102"));

                // GetBytes 大端
                byte[] bytes = HCrc32.GetBytes(c1);
                v.Ok("CRC-32 GetBytes 大端 4 字节",
                    bytes.Length == 4
                    && bytes[0] == (byte)(c1 >> 24) && bytes[1] == (byte)(c1 >> 16)
                    && bytes[2] == (byte)(c1 >> 8) && bytes[3] == (byte)(c1 & 0xFF));

                // 文件：小文件 + 跨越 80KB 流式缓冲边界的大文件
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "crc32.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-32 文件校验与内存一致", HCrc32.ComputeFile(file) == c1);
                    v.Ok("CRC-32 文件 Hex 长度 8", HCrc32.ComputeFileHex(file).Length == 8);
                    v.Ok("CRC-32 文件 VerifyFile 通过", HCrc32.VerifyFile(file, c1));

                    byte[] big = HCryptoCore.RandomBytes(200 * 1024 + 57);
                    uint bigCrc = HCrc32.Compute(big);
                    string bigFile = Path.Combine(dir, "crc32_big.bin");
                    File.WriteAllBytes(bigFile, big);
                    v.Ok("CRC-32 大文件（跨缓冲）与内存一致", HCrc32.ComputeFile(bigFile) == bigCrc);

                    byte[] tampered = (byte[])big.Clone();
                    tampered[tampered.Length - 1] ^= 0x01;
                    string bad = Path.Combine(dir, "crc32_bad.bin");
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-32 大文件末字节篡改后 VerifyFile 失败", !HCrc32.VerifyFile(bad, bigCrc));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
