using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc16Ccitt"/> 校验类自测：标准向量、空数据、中文 UTF-8 一致性、
    /// 数值/Hex Verify、GetBytes 大端表示、文件流式与内存一致性、篡改拒绝。
    /// 调用方式：<c>HCrc16CcittVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc16CcittVerify
    {
        /// <summary>运行 CRC-16/CCITT-FALSE 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc16Ccitt", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-16/CCITT-FALSE(123456789) 标准向量 0x29B1",
                    HCrc16Ccitt.Compute(Encoding.ASCII.GetBytes("123456789")) == 0x29B1);
                v.Ok("CRC-16/CCITT-FALSE 空数据为初值 0xFFFF",
                    HCrc16Ccitt.Compute(new byte[0]) == 0xFFFF);

                // 确定性与区分度
                string zh = "仪表通信帧 FCS 校验 0x1021";
                ushort c1 = HCrc16Ccitt.Compute(zh);
                v.Ok("CRC-16/CCITT-FALSE 同输入结果确定", c1 == HCrc16Ccitt.Compute(zh));
                v.Ok("CRC-16/CCITT-FALSE 不同输入结果不同", HCrc16Ccitt.Compute(zh + "x") != c1);
                v.Ok("CRC-16/CCITT-FALSE 中文与 UTF8 字节计算一致",
                    c1 == HCrc16Ccitt.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify 数值 / Hex / 负例
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-16/CCITT-FALSE 数值 Verify 通过", HCrc16Ccitt.Verify(data, c1));
                v.Ok("CRC-16/CCITT-FALSE 错误期望值 Verify 失败", !HCrc16Ccitt.Verify(data, (ushort)(c1 ^ 0xFFFF)));
                v.Ok("CRC-16/CCITT-FALSE Hex Verify 小写通过", HCrc16Ccitt.Verify(data, HCrc16Ccitt.ComputeHex(data)));
                v.Ok("CRC-16/CCITT-FALSE Hex Verify 大写通过", HCrc16Ccitt.Verify(data, HCrc16Ccitt.ComputeHex(data).ToUpperInvariant()));
                v.Ok("CRC-16/CCITT-FALSE 非法 Hex Verify 返回 false", !HCrc16Ccitt.Verify(data, "zzzz"));
                v.Ok("CRC-16/CCITT-FALSE 长度不符 Hex 返回 false", !HCrc16Ccitt.Verify(data, "0102"));

                // GetBytes 大端（高字节在前）
                byte[] bytes = HCrc16Ccitt.GetBytes(c1);
                v.Ok("CRC-16/CCITT-FALSE GetBytes 大端 2 字节",
                    bytes.Length == 2 && bytes[0] == (c1 >> 8) && bytes[1] == (c1 & 0xFF));

                // 文件
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "ccitt.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-16/CCITT-FALSE 文件校验与内存一致", HCrc16Ccitt.ComputeFile(file) == c1);
                    v.Ok("CRC-16/CCITT-FALSE 文件 Hex 长度 4", HCrc16Ccitt.ComputeFileHex(file).Length == 4);
                    v.Ok("CRC-16/CCITT-FALSE 文件 VerifyFile 通过", HCrc16Ccitt.VerifyFile(file, c1));

                    string bad = Path.Combine(dir, "ccitt_bad.bin");
                    byte[] tampered = (byte[])data.Clone();
                    tampered[tampered.Length / 2] ^= 0x01;
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-16/CCITT-FALSE 篡改后 VerifyFile 失败", !HCrc16Ccitt.VerifyFile(bad, c1));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
