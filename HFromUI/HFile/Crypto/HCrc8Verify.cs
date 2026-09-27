using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc8"/> 校验类自测：CRC 目录标准向量 "123456789"、空数据、确定性、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、文件流式与内存结果一致性。
    /// 调用方式：<c>HCrc8Verify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc8Verify
    {
        /// <summary>运行 CRC-8/SMBUS 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc8", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-8(123456789) 标准向量 0xF4",
                    HCrc8.Compute(Encoding.ASCII.GetBytes("123456789")) == 0xF4);
                v.Ok("CRC-8 空数据为初值 0x00",
                    HCrc8.Compute(new byte[0]) == 0x00);

                // 确定性与区分度
                string zh = "帧校验测试：温度 26.5℃";
                byte c1 = HCrc8.Compute(zh);
                v.Ok("CRC-8 同输入结果确定", c1 == HCrc8.Compute(zh));
                v.Ok("CRC-8 不同输入结果不同", HCrc8.Compute(zh + "!") != c1);

                // 字符串与字节 API 一致
                v.Ok("CRC-8 中文与 UTF8 字节计算一致",
                    c1 == HCrc8.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify 数值 / Hex / 负例
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-8 数值 Verify 通过", HCrc8.Verify(data, c1));
                v.Ok("CRC-8 错误期望值 Verify 失败", !HCrc8.Verify(data, (byte)(c1 ^ 0xFF)));
                v.Ok("CRC-8 Hex Verify 小写通过", HCrc8.Verify(data, HHex.Encode(HCrc8.GetBytes(c1))));
                v.Ok("CRC-8 Hex Verify 大写通过", HCrc8.Verify(data, HHex.Encode(HCrc8.GetBytes(c1)).ToUpperInvariant()));
                v.Ok("CRC-8 非法 Hex Verify 返回 false", !HCrc8.Verify(data, "zz"));

                // GetBytes 大端表示（8 位为 1 字节）
                byte[] bytes = HCrc8.GetBytes(c1);
                v.Ok("CRC-8 GetBytes 长度为 1", bytes.Length == 1 && bytes[0] == c1);

                // 文件与内存一致 + 篡改拒绝
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "crc8.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-8 文件校验与内存一致", HCrc8.ComputeFile(file) == c1);
                    v.Ok("CRC-8 文件 Hex 与数值一致", HCrc8.ComputeFileHex(file) == HHex.Encode(bytes));
                    v.Ok("CRC-8 文件 VerifyFile 通过", HCrc8.VerifyFile(file, c1));

                    string bad = Path.Combine(dir, "crc8_bad.bin");
                    byte[] tampered = (byte[])data.Clone();
                    tampered[0] ^= 0xFF;
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-8 篡改后 VerifyFile 失败", !HCrc8.VerifyFile(bad, c1));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
