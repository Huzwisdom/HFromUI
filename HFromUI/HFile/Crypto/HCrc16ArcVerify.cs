using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc16Arc"/> 校验类自测：标准向量、空数据、与 MODBUS 初值区分、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、文件流式与内存一致性。
    /// 调用方式：<c>HCrc16ArcVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc16ArcVerify
    {
        /// <summary>运行 CRC-16/ARC 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc16Arc", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-16/ARC(123456789) 标准向量 0xBB3D",
                    HCrc16Arc.Compute(Encoding.ASCII.GetBytes("123456789")) == 0xBB3D);
                v.Ok("CRC-16/ARC 空数据为初值 0x0000",
                    HCrc16Arc.Compute(new byte[0]) == 0x0000);

                // 与 MODBUS 同多项式、不同初值
                byte[] vector = Encoding.ASCII.GetBytes("123456789");
                v.Ok("CRC-16/ARC 与 CRC-16/MODBUS 结果不同",
                    HCrc16Arc.Compute(vector) != HCrc16Modbus.Compute(vector));

                // 确定性与中文
                string zh = "LHA/ARC 归档帧校验数据";
                ushort c1 = HCrc16Arc.Compute(zh);
                v.Ok("CRC-16/ARC 同输入结果确定", c1 == HCrc16Arc.Compute(zh));
                v.Ok("CRC-16/ARC 不同输入结果不同", HCrc16Arc.Compute(zh + "x") != c1);
                v.Ok("CRC-16/ARC 中文与 UTF8 字节计算一致",
                    c1 == HCrc16Arc.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-16/ARC 数值 Verify 通过", HCrc16Arc.Verify(data, c1));
                v.Ok("CRC-16/ARC 错误期望值 Verify 失败", !HCrc16Arc.Verify(data, (ushort)(c1 ^ 0xFFFF)));
                v.Ok("CRC-16/ARC Hex Verify 小写通过", HCrc16Arc.Verify(data, HCrc16Arc.ComputeHex(data)));
                v.Ok("CRC-16/ARC Hex Verify 大写通过", HCrc16Arc.Verify(data, HCrc16Arc.ComputeHex(data).ToUpperInvariant()));
                v.Ok("CRC-16/ARC 非法 Hex Verify 返回 false", !HCrc16Arc.Verify(data, "zzzz"));

                // GetBytes 大端
                byte[] bytes = HCrc16Arc.GetBytes(c1);
                v.Ok("CRC-16/ARC GetBytes 大端 2 字节",
                    bytes.Length == 2 && bytes[0] == (c1 >> 8) && bytes[1] == (c1 & 0xFF));

                // 文件
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "arc.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-16/ARC 文件校验与内存一致", HCrc16Arc.ComputeFile(file) == c1);
                    v.Ok("CRC-16/ARC 文件 VerifyFile 通过", HCrc16Arc.VerifyFile(file, c1));

                    string bad = Path.Combine(dir, "arc_bad.bin");
                    byte[] tampered = (byte[])data.Clone();
                    tampered[1] ^= 0x80;
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-16/ARC 篡改后 VerifyFile 失败", !HCrc16Arc.VerifyFile(bad, c1));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
