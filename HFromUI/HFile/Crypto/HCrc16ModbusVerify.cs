using System.IO;
using System.Text;

namespace HFromUI.HFile.Crypto
{
    /// <summary>
    /// <see cref="HCrc16Modbus"/> 校验类自测：标准向量、空数据、与 CRC-16/ARC 的初值区分、
    /// 中文 UTF-8 一致性、数值/Hex Verify、GetBytes 大端表示、文件流式与内存一致性。
    /// 调用方式：<c>HCrc16ModbusVerify.Run()</c>，全部通过返回 true，明细输出到控制台。
    /// </summary>
    public static class HCrc16ModbusVerify
    {
        /// <summary>运行 CRC-16/MODBUS 全部自测项。</summary>
        /// <returns>全部通过返回 true。</returns>
        public static bool Run()
        {
            return HCryptoVerifier.Run("HCrc16Modbus", v =>
            {
                // CRC 目录标准向量
                v.Ok("CRC-16/MODBUS(123456789) 标准向量 0x4B37",
                    HCrc16Modbus.Compute(Encoding.ASCII.GetBytes("123456789")) == 0x4B37);
                v.Ok("CRC-16/MODBUS 空数据为初值 0xFFFF",
                    HCrc16Modbus.Compute(new byte[0]) == 0xFFFF);

                // 与 CRC-16/ARC 同多项式但初值不同，结果必须不同
                byte[] vector = Encoding.ASCII.GetBytes("123456789");
                v.Ok("CRC-16/MODBUS 与 CRC-16/ARC 初值不同结果不同",
                    HCrc16Modbus.Compute(vector) != HCrc16Arc.Compute(vector));

                // 寄存器 CRC = 0xCDC5；GetBytes 为大端 {CD,C5}，RTU 线上低字节先发故线序为 C5 CD
                byte[] rtuFrame = new byte[] { 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A };
                ushort rtuCrc = HCrc16Modbus.Compute(rtuFrame);
                v.Ok("CRC-16/MODBUS RTU 典型帧寄存器值 0xCDC5", rtuCrc == 0xCDC5);
                byte[] bigEndian = HCrc16Modbus.GetBytes(rtuCrc);
                v.Ok("CRC-16/MODBUS GetBytes 大端为 CD C5（线序需交换成 C5 CD）",
                    bigEndian[0] == 0xCD && bigEndian[1] == 0xC5);

                // 确定性与中文
                string zh = "Modbus RTU 从站地址 01 功能码 03";
                ushort c1 = HCrc16Modbus.Compute(zh);
                v.Ok("CRC-16/MODBUS 同输入结果确定", c1 == HCrc16Modbus.Compute(zh));
                v.Ok("CRC-16/MODBUS 不同输入结果不同", HCrc16Modbus.Compute(zh + "!") != c1);
                v.Ok("CRC-16/MODBUS 中文与 UTF8 字节计算一致",
                    c1 == HCrc16Modbus.Compute(Encoding.UTF8.GetBytes(zh)));

                // Verify
                byte[] data = Encoding.UTF8.GetBytes(zh);
                v.Ok("CRC-16/MODBUS 数值 Verify 通过", HCrc16Modbus.Verify(data, c1));
                v.Ok("CRC-16/MODBUS 错误期望值 Verify 失败", !HCrc16Modbus.Verify(data, (ushort)(c1 ^ 0xFFFF)));
                v.Ok("CRC-16/MODBUS Hex Verify 小写通过", HCrc16Modbus.Verify(data, HCrc16Modbus.ComputeHex(data)));
                v.Ok("CRC-16/MODBUS Hex Verify 大写通过", HCrc16Modbus.Verify(data, HCrc16Modbus.ComputeHex(data).ToUpperInvariant()));
                v.Ok("CRC-16/MODBUS 非法 Hex Verify 返回 false", !HCrc16Modbus.Verify(data, "zzzz"));

                // GetBytes 大端（RTU 线上为小端，此处只验大端工具方法）
                byte[] bytes = HCrc16Modbus.GetBytes(c1);
                v.Ok("CRC-16/MODBUS GetBytes 大端 2 字节",
                    bytes.Length == 2 && bytes[0] == (c1 >> 8) && bytes[1] == (c1 & 0xFF));

                // 文件
                string dir = HCryptoVerifier.CreateTempDir();
                try
                {
                    string file = Path.Combine(dir, "modbus.bin");
                    File.WriteAllBytes(file, data);
                    v.Ok("CRC-16/MODBUS 文件校验与内存一致", HCrc16Modbus.ComputeFile(file) == c1);
                    v.Ok("CRC-16/MODBUS 文件 VerifyFile 通过", HCrc16Modbus.VerifyFile(file, c1));

                    string bad = Path.Combine(dir, "modbus_bad.bin");
                    byte[] tampered = (byte[])data.Clone();
                    tampered[1] ^= 0x80;
                    File.WriteAllBytes(bad, tampered);
                    v.Ok("CRC-16/MODBUS 篡改后 VerifyFile 失败", !HCrc16Modbus.VerifyFile(bad, c1));
                }
                finally
                {
                    HCryptoVerifier.CleanupTempDir(dir);
                }
            });
        }
    }
}
