using System;
using System.IO;
using System.Text;

namespace HFromUI.HSocket.HFileTransfer
{
    /// <summary>
    /// 大文件传输二进制分帧协议（同时适用于 TCP 网络流与串口 BaseStream）。
    /// <para>帧头固定 9 字节：Magic[4]='H','F','T','1' + Type[1] + PayloadLength[4](小端)，后接负载。</para>
    /// <para>断点续传：接收方维护 *.hftpart 临时分片文件，并以自身已写长度作为续传偏移，
    /// 发送方按接收方 ACK 的偏移 Seek 后继续发送。</para>
    /// </summary>
    internal static class HFilePacket
    {
        /// <summary>发送请求：负载 = 文件名 UTF8 字节(4 长度前缀) + 文件总长度(8)。</summary>
        public const byte SendReq = 1;

        /// <summary>请求应答：负载 = 续传起始偏移(8) + 是否接受(1)。</summary>
        public const byte SendAck = 2;

        /// <summary>数据块：负载 = 块在整文件中的绝对偏移(8) + 数据(n)。</summary>
        public const byte Data = 3;

        /// <summary>数据块应答：负载 = 接收方当前已收长度/下一期望偏移(8)。</summary>
        public const byte DataAck = 4;

        /// <summary>发送完成通知：无负载。</summary>
        public const byte Finish = 5;

        /// <summary>完成应答：负载 = 是否校验通过(1)。</summary>
        public const byte FinishAck = 6;

        /// <summary>取消传输：无负载（双方收到后保留分片文件以备下次续传）。</summary>
        public const byte Cancel = 7;

        /// <summary>帧头长度（字节）。</summary>
        public const int HeaderSize = 9;

        private static readonly byte[] Magic = { (byte)'H', (byte)'F', (byte)'T', (byte)'1' };

        /// <summary>向流写入一帧（含帧头与负载）。</summary>
        /// <param name="stream">目标流</param>
        /// <param name="type">帧类型</param>
        /// <param name="payload">负载（可为 null）</param>
        public static void Write(Stream stream, byte type, byte[] payload)
        {
            int length = payload == null ? 0 : payload.Length;
            byte[] header = new byte[HeaderSize];
            Buffer.BlockCopy(Magic, 0, header, 0, 4);
            header[4] = type;
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, header, 5, 4);
            stream.Write(header, 0, HeaderSize);
            if (length > 0)
            {
                stream.Write(payload, 0, length);
            }
            stream.Flush();
        }

        /// <summary>构造发送请求负载。</summary>
        public static byte[] BuildSendReq(string fileName, long fileSize)
        {
            byte[] name = Encoding.UTF8.GetBytes(fileName ?? string.Empty);
            byte[] payload = new byte[4 + name.Length + 8];
            Buffer.BlockCopy(BitConverter.GetBytes(name.Length), 0, payload, 0, 4);
            Buffer.BlockCopy(name, 0, payload, 4, name.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(fileSize), 0, payload, 4 + name.Length, 8);
            return payload;
        }

        /// <summary>解析发送请求负载。</summary>
        public static void ParseSendReq(byte[] payload, out string fileName, out long fileSize)
        {
            int nameLen = BitConverter.ToInt32(payload, 0);
            fileName = Encoding.UTF8.GetString(payload, 4, nameLen);
            fileSize = BitConverter.ToInt64(payload, 4 + nameLen);
        }

        /// <summary>构造请求应答负载。</summary>
        public static byte[] BuildSendAck(long offset, bool accept)
        {
            byte[] payload = new byte[9];
            Buffer.BlockCopy(BitConverter.GetBytes(offset), 0, payload, 0, 8);
            payload[8] = accept ? (byte)1 : (byte)0;
            return payload;
        }

        /// <summary>解析请求应答负载。</summary>
        public static void ParseSendAck(byte[] payload, out long offset, out bool accept)
        {
            offset = BitConverter.ToInt64(payload, 0);
            accept = payload[8] != 0;
        }

        /// <summary>构造数据块负载。</summary>
        public static byte[] BuildData(long offset, byte[] chunk, int count)
        {
            byte[] payload = new byte[8 + count];
            Buffer.BlockCopy(BitConverter.GetBytes(offset), 0, payload, 0, 8);
            Buffer.BlockCopy(chunk, 0, payload, 8, count);
            return payload;
        }

        /// <summary>解析数据块负载的数据绝对偏移。</summary>
        public static long ParseDataOffset(byte[] payload)
        {
            return BitConverter.ToInt64(payload, 0);
        }

        /// <summary>构造数据块应答负载。</summary>
        public static byte[] BuildDataAck(long offset)
        {
            return BitConverter.GetBytes(offset);
        }

        /// <summary>解析数据块应答负载。</summary>
        public static long ParseDataAck(byte[] payload)
        {
            return BitConverter.ToInt64(payload, 0);
        }

        /// <summary>构造完成应答负载。</summary>
        public static byte[] BuildFinishAck(bool ok)
        {
            return new[] { ok ? (byte)1 : (byte)0 };
        }

        /// <summary>解析完成应答负载。</summary>
        public static bool ParseFinishAck(byte[] payload)
        {
            return payload.Length >= 1 && payload[0] != 0;
        }

        /// <summary>判断一段帧头是否以协议魔数开头。</summary>
        public static bool IsMagic(byte[] header)
        {
            return header.Length >= 4
                && header[0] == Magic[0]
                && header[1] == Magic[1]
                && header[2] == Magic[2]
                && header[3] == Magic[3];
        }
    }
}
