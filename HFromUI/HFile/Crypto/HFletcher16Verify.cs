using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HFletcher16"/> 校验类自测：标准向量、空数据、手工累加小向量、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、跨分块大文件流式一致性。
    /// 调用方式：<c>HFletcher16Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HFletcher16Verify
    {
        /// <summary>运行 Fletcher-16 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HFletcher16", v =>
            {
                // 标准向量
                v.Ok("Fletcher-16(123456789) 标准向量 0x1EDE",
                    HFletcher16.Compute(Encoding.ASCII.GetBytes("123456789")) == 0x1EDE);
                v.Ok("Fletcher-16 空数据为 0x0000",
                    HFletcher16.Compute(new byte[0]) == 0x0000);

                // 手工累加小向量：单字节 0x01 → s1=1, s2=1 → 0x0101
                v.Ok("Fletcher-16 单字节 0x01 手工向量 0x0101",
                    HFletcher16.Compute(new byte[] { 0x01 }) == 0x0101);
                // 两字节 0x01,0x02 → s1=3, s2=4 → 0x0403
                v.Ok("Fletcher-16 字节 01 02 手工向量 0x0403",
                    HFletcher16.Compute(new byte[] { 0x01, 0x02 }) == 0x0403);

                // 确定性与中文
                string zh = "遥测帧 Fletcher 双累加器校验";
                ushort c1 = HFletcher16.Compute(zh);
                v.Ok("Fletcher-16 同输入结果确定", c1 == HFletcher16.Compute(zh));
                v.Ok("Fletcher-16 不同输入结果不同", HFletcher16.Compute(zh + "x") != c1);
                v.Ok("Fletcher-16 中文与 UTF8 字节计算一致",
                    c1 == HFletcher16.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("Fletcher-16 数值 Verify 通过", HFletcher16.Verify(data, c1));
                v.Ok("Fletcher-16 错误期望值 Verify 失败", !HFletcher16.Verify(data, (ushort)(c1 ^ 0xFFFF)));
                v.Ok("Fletcher-16 Hex Verify 小写通过", HFletcher16.Verify(data, HFletcher16.ComputeHex(data)));
                v.Ok("Fletcher-16 Hex Verify 大写通过", HFletcher16.Verify(data, HFletcher16.ComputeHex(data).ToUpperInvariant()));
                v.Ok("Fletcher-16 非法 Hex Verify 返回 false", !HFletcher16.Verify(data, "zzzz"));

                // GetBytes 大端
                byte[] bytes = HFletcher16.GetBytes(c1);
                v.Ok("Fletcher-16 GetBytes 大端 2 字节",
                    bytes.Length == 2 && bytes[0] == (c1 >> 8) && bytes[1] == (c1 & 0xFF));

                // 文件（数据量超过 4095 分块上限，验证跨块取模正确）
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "f16.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("Fletcher-16 文件校验与内存一致", HFletcher16.ComputeFile(file) == c1);
                    v.Ok("Fletcher-16 文件 VerifyFile 通过", HFletcher16.VerifyFile(file, c1));

                    byte[] big = HCryptoCore.RandomBytes(30 * 1024 + 13);
                    ushort bigValue = HFletcher16.Compute(big);
                    string bigFile = Path.Combine(dir, "f16_big.bin");
                    File.WriteAllBytes(bigFile, big);
                    v.Ok("Fletcher-16 大文件（跨分块/缓冲）与内存一致", HFletcher16.ComputeFile(bigFile) == bigValue);

                    byte[] tampered = (byte[])big.Clone();
                    tampered[0] ^= 0xFF;
                    string bad = Path.Combine(dir, "f16_bad.bin");
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("Fletcher-16 篡改后 VerifyFile 失败", !HFletcher16.VerifyFile(bad, bigValue));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
