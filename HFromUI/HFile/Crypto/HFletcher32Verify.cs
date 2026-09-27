using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HFletcher32"/> 校验类自测：标准向量（含奇数字节补 0x00 规则）、空数据、
    /// 手工大端组字小向量、奇偶长度文件跨缓冲组字正确性、中文 UTF-8 一致性、数值/Hex Verify。
    /// 调用方式：<c>HFletcher32Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HFletcher32Verify
    {
        /// <summary>运行 Fletcher-32 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HFletcher32", v =>
            {
                // 标准向量：9 个字节为奇数，尾部补 0x00 组字
                v.Ok("Fletcher-32(123456789) 标准向量 0x09DF09D5",
                    HFletcher32.Compute(Encoding.ASCII.GetBytes("123456789")) == 0x09DF09D5);
                v.Ok("Fletcher-32 空数据为 0x00000000",
                    HFletcher32.Compute(new byte[0]) == 0x00000000);

                // 手工大端组字小向量
                // 00 01 → 字 0x0001：s1=1, s2=1 → 0x00010001
                v.Ok("Fletcher-32 字节 00 01 手工向量 0x00010001",
                    HFletcher32.Compute(new byte[] { 0x00, 0x01 }) == 0x00010001);
                // 单个奇数字节 0x01 → 补 0x00 组字 0x0100：s1=256, s2=256 → 0x01000100
                v.Ok("Fletcher-32 单字节 0x01 补零组字 0x01000100",
                    HFletcher32.Compute(new byte[] { 0x01 }) == 0x01000100);

                // 补零规则：奇数长度数组末尾隐式补 0x00，应与显式补零的偶数长度数组完全一致
                v.Ok("Fletcher-32 abc 与显式 abc00 补零一致",
                    HFletcher32.Compute(new byte[] { 0x61, 0x62, 0x63 })
                    == HFletcher32.Compute(new byte[] { 0x61, 0x62, 0x63, 0x00 }));

                // 确定性与中文
                string zh = "存储页 16 位字 Fletcher-32 校验";
                uint c1 = HFletcher32.Compute(zh);
                v.Ok("Fletcher-32 同输入结果确定", c1 == HFletcher32.Compute(zh));
                v.Ok("Fletcher-32 不同输入结果不同", HFletcher32.Compute(zh + "x") != c1);
                v.Ok("Fletcher-32 中文与 UTF8 字节计算一致",
                    c1 == HFletcher32.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("Fletcher-32 数值 Verify 通过", HFletcher32.Verify(data, c1));
                v.Ok("Fletcher-32 错误期望值 Verify 失败", !HFletcher32.Verify(data, c1 ^ 0xFFFFFFFF));
                v.Ok("Fletcher-32 Hex Verify 小写通过", HFletcher32.Verify(data, HFletcher32.ComputeHex(data)));
                v.Ok("Fletcher-32 Hex Verify 大写通过", HFletcher32.Verify(data, HFletcher32.ComputeHex(data).ToUpperInvariant()));
                v.Ok("Fletcher-32 非法 Hex Verify 返回 false", !HFletcher32.Verify(data, "zzzzzzzz"));

                // GetBytes 大端
                byte[] bytes = HFletcher32.GetBytes(c1);
                v.Ok("Fletcher-32 GetBytes 大端 4 字节",
                    bytes.Length == 4
                    && bytes[0] == (byte)(c1 >> 24) && bytes[1] == (byte)(c1 >> 16)
                    && bytes[2] == (byte)(c1 >> 8) && bytes[3] == (byte)(c1 & 0xFF));

                // 文件：偶数长度与奇数长度各跑一次，验证跨缓冲的组字 carry 正确
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "f32.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("Fletcher-32 文件校验与内存一致", HFletcher32.ComputeFile(file) == c1);
                    v.Ok("Fletcher-32 文件 VerifyFile 通过", HFletcher32.VerifyFile(file, c1));

                    byte[] even = HCryptoCore.RandomBytes(200 * 1024);
                    byte[] odd = HCryptoCore.RandomBytes(200 * 1024 + 1);
                    string evenFile = Path.Combine(dir, "f32_even.bin");
                    string oddFile = Path.Combine(dir, "f32_odd.bin");
                    File.WriteAllBytes(evenFile, even);
                    File.WriteAllBytes(oddFile, odd);
                    v.Ok("Fletcher-32 偶数长度大文件与内存一致",
                        HFletcher32.ComputeFile(evenFile) == HFletcher32.Compute(even));
                    v.Ok("Fletcher-32 奇数长度大文件与内存一致（跨缓冲补零）",
                        HFletcher32.ComputeFile(oddFile) == HFletcher32.Compute(odd));

                    byte[] tampered = (byte[])odd.Clone();
                    tampered[81920] ^= 0x12; // 恰好在流式缓冲边界附近
                    string bad = Path.Combine(dir, "f32_bad.bin");
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("Fletcher-32 边界附近篡改后 VerifyFile 失败",
                        !HFletcher32.VerifyFile(bad, HFletcher32.Compute(odd)));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
