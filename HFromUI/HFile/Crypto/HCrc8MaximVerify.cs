using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc8Maxim"/> 校验类自测：标准向量、空数据、与 CRC-8/SMBUS 的算法区分、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes、文件流式与内存一致性。
    /// 调用方式：<c>HCrc8MaximVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc8MaximVerify
    {
        /// <summary>运行 CRC-8/MAXIM 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc8Maxim", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-8/MAXIM(123456789) 标准向量 0xA1",
                    HCrc8Maxim.Compute(Encoding.ASCII.GetBytes("123456789")) == 0xA1);
                v.Ok("CRC-8/MAXIM 空数据为初值 0x00",
                    HCrc8Maxim.Compute(new byte[0]) == 0x00);

                // 与 CRC-8/SMBUS（多项式 0x07、不反射）结果必须不同，防止参数写错
                byte[] vector = Encoding.ASCII.GetBytes("123456789");
                v.Ok("CRC-8/MAXIM 与 CRC-8/SMBUS 结果不同",
                    HCrc8Maxim.Compute(vector) != HCrc8.Compute(vector));

                // 确定性与区分度
                string zh = "单总线 DS18B20 ROM 校验";
                byte c1 = HCrc8Maxim.Compute(zh);
                v.Ok("CRC-8/MAXIM 同输入结果确定", c1 == HCrc8Maxim.Compute(zh));
                v.Ok("CRC-8/MAXIM 不同输入结果不同", HCrc8Maxim.Compute(zh + "x") != c1);
                v.Ok("CRC-8/MAXIM 中文与 UTF8 字节计算一致",
                    c1 == HCrc8Maxim.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify 数值 / Hex / 负例
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-8/MAXIM 数值 Verify 通过", HCrc8Maxim.Verify(data, c1));
                v.Ok("CRC-8/MAXIM 错误期望值 Verify 失败", !HCrc8Maxim.Verify(data, (byte)(c1 ^ 0xFF)));
                v.Ok("CRC-8/MAXIM Hex Verify 小写通过", HCrc8Maxim.Verify(data, HCrc8Maxim.ComputeHex(data)));
                v.Ok("CRC-8/MAXIM Hex Verify 大写通过", HCrc8Maxim.Verify(data, HCrc8Maxim.ComputeHex(data).ToUpperInvariant()));
                v.Ok("CRC-8/MAXIM 非法 Hex Verify 返回 false", !HCrc8Maxim.Verify(data, "zz"));

                // GetBytes
                byte[] bytes = HCrc8Maxim.GetBytes(c1);
                v.Ok("CRC-8/MAXIM GetBytes 长度为 1", bytes.Length == 1 && bytes[0] == c1);

                // 文件
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "crc8m.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-8/MAXIM 文件校验与内存一致", HCrc8Maxim.ComputeFile(file) == c1);
                    v.Ok("CRC-8/MAXIM 文件 VerifyFile 通过", HCrc8Maxim.VerifyFile(file, c1));

                    string bad = Path.Combine(dir, "crc8m_bad.bin");
                    byte[] tampered = (byte[])data.Clone();
                    tampered[0] ^= 0xFF;
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-8/MAXIM 篡改后 VerifyFile 失败", !HCrc8Maxim.VerifyFile(bad, c1));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
