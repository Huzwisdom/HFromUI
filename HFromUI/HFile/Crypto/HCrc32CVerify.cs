using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc32C"/> 校验类自测：标准向量、空数据、与经典 CRC-32 区分、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、跨缓冲大文件流式一致性。
    /// 调用方式：<c>HCrc32CVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc32CVerify
    {
        /// <summary>运行 CRC-32C（Castagnoli）全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc32C", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-32C(123456789) 标准向量 0xE3069283",
                    HCrc32C.Compute(Encoding.ASCII.GetBytes("123456789")) == 0xE3069283);
                v.Ok("CRC-32C 空数据为 0x00000000",
                    HCrc32C.Compute(new byte[0]) == 0x00000000);

                // 与经典 CRC-32 多项式不同
                byte[] vector = Encoding.ASCII.GetBytes("123456789");
                v.Ok("CRC-32C 与 CRC-32 结果不同",
                    HCrc32C.Compute(vector) != HCrc32.Compute(vector));

                // 确定性与中文
                string zh = "iSCSI/ext4/Btrfs 使用的 Castagnoli 校验";
                uint c1 = HCrc32C.Compute(zh);
                v.Ok("CRC-32C 同输入结果确定", c1 == HCrc32C.Compute(zh));
                v.Ok("CRC-32C 不同输入结果不同", HCrc32C.Compute(zh + "x") != c1);
                v.Ok("CRC-32C 中文与 UTF8 字节计算一致",
                    c1 == HCrc32C.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-32C 数值 Verify 通过", HCrc32C.Verify(data, c1));
                v.Ok("CRC-32C 错误期望值 Verify 失败", !HCrc32C.Verify(data, c1 ^ 0xFFFFFFFF));
                v.Ok("CRC-32C Hex Verify 小写通过", HCrc32C.Verify(data, HCrc32C.ComputeHex(data)));
                v.Ok("CRC-32C Hex Verify 大写通过", HCrc32C.Verify(data, HCrc32C.ComputeHex(data).ToUpperInvariant()));
                v.Ok("CRC-32C 非法 Hex Verify 返回 false", !HCrc32C.Verify(data, "zzzzzzzz"));

                // GetBytes 大端
                byte[] bytes = HCrc32C.GetBytes(c1);
                v.Ok("CRC-32C GetBytes 大端 4 字节",
                    bytes.Length == 4
                    && bytes[0] == (byte)(c1 >> 24) && bytes[1] == (byte)(c1 >> 16)
                    && bytes[2] == (byte)(c1 >> 8) && bytes[3] == (byte)(c1 & 0xFF));

                // 文件（跨缓冲）
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "crc32c.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-32C 文件校验与内存一致", HCrc32C.ComputeFile(file) == c1);
                    v.Ok("CRC-32C 文件 VerifyFile 通过", HCrc32C.VerifyFile(file, c1));

                    byte[] big = HCryptoCore.RandomBytes(200 * 1024 + 77);
                    uint bigCrc = HCrc32C.Compute(big);
                    string bigFile = Path.Combine(dir, "crc32c_big.bin");
                    File.WriteAllBytes(bigFile, big);
                    v.Ok("CRC-32C 大文件（跨缓冲）与内存一致", HCrc32C.ComputeFile(bigFile) == bigCrc);

                    byte[] tampered = (byte[])big.Clone();
                    tampered[0] ^= 0x80;
                    string bad = Path.Combine(dir, "crc32c_bad.bin");
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-32C 篡改后 VerifyFile 失败", !HCrc32C.VerifyFile(bad, bigCrc));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
