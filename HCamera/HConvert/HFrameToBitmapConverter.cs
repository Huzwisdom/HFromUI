using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HFromUI.HConvert
{
    using HFromUI.HLangage;
    /// <summary>
    /// 视频帧格式枚举
    /// 涵盖了当前主流工业相机可能输出的各种未压缩像素格式。
    /// 包括大华、海康等品牌的常见输出格式。
    /// </summary>
    public enum VideoFrameFormat
    {
        // ============ YUV 4:2:0 平面格式 ============
        /// <summary>YV12：Y 平面 + V 平面 + U 平面（大华常用）</summary>
        YV12,
        /// <summary>I420：Y 平面 + U 平面 + V 平面（标准 YUV420 平面）</summary>
        I420,

        // ============ YUV 4:2:0 半平面格式 ============
        /// <summary>NV12：Y 平面 + UV 交错平面（海康常用）</summary>
        NV12,
        /// <summary>NV21：Y 平面 + VU 交错平面</summary>
        NV21,

        // ============ YUV 4:2:0 高位深格式 ============
        /// <summary>P010：10-bit 4:2:0 半平面（每个分量占 2 字节，高位有效）</summary>
        P010,
        /// <summary>P016：16-bit 4:2:0 半平面（每个分量占 2 字节）</summary>
        P016,

        // ============ YUV 4:2:2 打包格式 ============
        /// <summary>YUYV (YUY2)：Y0 U0 Y1 V0 Y2 U1 Y3 V1 ...</summary>
        YUYV,
        /// <summary>UYVY：U0 Y0 V0 Y1 U1 Y2 V1 Y3 ...</summary>
        UYVY,
        /// <summary>YVYU：Y0 V0 Y1 U0 Y2 V1 Y3 U1 ...</summary>
        YVYU,
        /// <summary>VYUY：V0 Y0 U0 Y1 V1 Y2 U1 Y3 ...</summary>
        VYUY,

        // ============ YUV 4:2:2 平面格式 ============
        /// <summary>YUV422P：Y 平面 + U 平面 + V 平面（每两个像素共享一对 UV）</summary>
        YUV422P,
        /// <summary>YV16：同 YUV422P（别名）</summary>
        YV16,

        // ============ YUV 4:2:2 半平面格式 ============
        /// <summary>NV16：Y 平面 + UV 交错平面（每两个像素共享一对 UV）</summary>
        NV16,
        /// <summary>NV61：Y 平面 + VU 交错平面</summary>
        NV61,

        // ============ YUV 4:2:2 高位深格式 ============
        /// <summary>Y210：10-bit 打包格式（通常每像素 4 字节，高位填充）</summary>
        Y210,

        // ============ YUV 4:4:4 平面格式 ============
        /// <summary>YUV444P：Y 平面 + U 平面 + V 平面（每像素独立 UV）</summary>
        YUV444P,

        // ============ YUV 4:4:4 半平面格式 ============
        /// <summary>NV24：Y 平面 + UV 交错平面（每像素独立 UV）</summary>
        NV24,
        /// <summary>NV42：Y 平面 + VU 交错平面</summary>
        NV42,

        // ============ YUV 4:1:1 打包格式 ============
        /// <summary>YUV411：每四个像素共享一对 UV（打包格式，常见 UYYVYY 布局）</summary>
        YUV411,

        // ============ RGB/BGR 格式 ============
        /// <summary>RGB24：R G B 顺序，每像素 3 字节</summary>
        RGB24,
        /// <summary>BGR24：B G R 顺序，每像素 3 字节</summary>
        BGR24,
        /// <summary>RGB32：R G B X（填充）每像素 4 字节</summary>
        RGB32,
        /// <summary>BGR32：B G R X（填充）每像素 4 字节</summary>
        BGR32,
        /// <summary>ARGB32：A R G B 每像素 4 字节</summary>
        ARGB32,
        /// <summary>RGBA32：R G B A 每像素 4 字节</summary>
        RGBA32,
        /// <summary>BGRA32：B G R A 每像素 4 字节</summary>
        BGRA32,
        /// <summary>ABGR32：A B G R 每像素 4 字节</summary>
        ABGR32,

        // ============ 16 位 RGB 格式 ============
        /// <summary>RGB565：16 位，R5 G6 B5</summary>
        RGB565,
        /// <summary>BGR565：16 位，B5 G6 R5</summary>
        BGR565,
        /// <summary>RGB555：16 位，X1 R5 G5 B5（高位可忽略）</summary>
        RGB555,
        /// <summary>BGR555：16 位，X1 B5 G5 R5</summary>
        BGR555,
        /// <summary>RGB444：16 位，X4 R4 G4 B4</summary>
        RGB444,
        /// <summary>BGR444：16 位，X4 B4 G4 R4</summary>
        BGR444,

        // ============ 30-bit RGB 格式 ============
        /// <summary>RGB30：每像素 4 字节，每通道 10 位（高位填充）</summary>
        RGB30,
        /// <summary>BGR30：每像素 4 字节，每通道 10 位（高位填充）</summary>
        BGR30,

        // ============ 灰度格式 ============
        /// <summary>Gray8：8 位灰度</summary>
        Gray8,
        /// <summary>Gray16：16 位灰度（仅取高 8 位转换为 8 位）</summary>
        Gray16,

        // ============ Bayer 格式（工业相机常用）============
        /// <summary>BayerRG8：8-bit Bayer，RGGB 排列</summary>
        BayerRG8,
        /// <summary>BayerBG8：8-bit Bayer，BGGR 排列</summary>
        BayerBG8,
        /// <summary>BayerGR8：8-bit Bayer，GRBG 排列</summary>
        BayerGR8,
        /// <summary>BayerGB8：8-bit Bayer，GBRG 排列</summary>
        BayerGB8,

        // ============ 别名（仅供方便使用，实际数值与原始成员相同）============
        /// <summary>Y8：同 Gray8</summary>
        Y8 = Gray8,
        /// <summary>Y16：同 Gray16</summary>
        Y16 = Gray16
    }

    /// <summary>
    /// 视频帧转 Bitmap 转换器
    /// 全面支持主流未压缩像素格式，基于 Windows 原生 System.Drawing 和 unsafe 指针操作实现。
    /// 性能高效，无任何第三方依赖。
    /// </summary>
    public static class HFrameToBitmapConverter
    {
        // 翻译辅助函数，用于将错误提示和格式名称本地化

        /// <summary>
        /// 将原始帧数据转换为 Bitmap
        /// </summary>
        /// <param name="data">指向帧数据的指针（起始地址）</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="format">帧格式</param>
        /// <returns>转换后的 Bitmap 对象（调用者负责释放，除非赋给控件）</returns>
        public static Bitmap Convert(IntPtr data, int width, int height, VideoFrameFormat format)
        {
            if (data == IntPtr.Zero)
                throw new ArgumentException(HTranslation.GetContent("数据指针不能为空"), nameof(data));
            if (width <= 0 || height <= 0)
                throw new ArgumentException(HTranslation.GetContent("宽度和高度必须大于零"));

            switch (format)
            {
                // YUV 4:2:0 平面
                case VideoFrameFormat.YV12: return YV12ToBitmap(data, width, height);
                case VideoFrameFormat.I420: return I420ToBitmap(data, width, height);
                // YUV 4:2:0 半平面
                case VideoFrameFormat.NV12: return NV12ToBitmap(data, width, height);
                case VideoFrameFormat.NV21: return NV21ToBitmap(data, width, height);
                // YUV 4:2:0 高位深半平面
                case VideoFrameFormat.P010: return P010ToBitmap(data, width, height);
                case VideoFrameFormat.P016: return P016ToBitmap(data, width, height);
                // YUV 4:2:2 打包
                case VideoFrameFormat.YUYV: return Yuv422PackedToBitmap(data, width, height, PackedYuvOrder.YUYV);
                case VideoFrameFormat.UYVY: return Yuv422PackedToBitmap(data, width, height, PackedYuvOrder.UYVY);
                case VideoFrameFormat.YVYU: return Yuv422PackedToBitmap(data, width, height, PackedYuvOrder.YVYU);
                case VideoFrameFormat.VYUY: return Yuv422PackedToBitmap(data, width, height, PackedYuvOrder.VYUY);
                // YUV 4:2:2 平面
                case VideoFrameFormat.YUV422P:
                case VideoFrameFormat.YV16: return Yuv422PlanesToBitmap(data, width, height);
                // YUV 4:2:2 半平面
                case VideoFrameFormat.NV16: return Yuv422SemiPlanarToBitmap(data, width, height, true);
                case VideoFrameFormat.NV61: return Yuv422SemiPlanarToBitmap(data, width, height, false);
                // YUV 4:2:2 10-bit
                case VideoFrameFormat.Y210: return Y210ToBitmap(data, width, height);
                // YUV 4:4:4 平面
                case VideoFrameFormat.YUV444P: return Yuv444PlanesToBitmap(data, width, height);
                // YUV 4:4:4 半平面
                case VideoFrameFormat.NV24: return Yuv444SemiPlanarToBitmap(data, width, height, true);
                case VideoFrameFormat.NV42: return Yuv444SemiPlanarToBitmap(data, width, height, false);
                // YUV 4:1:1 打包
                case VideoFrameFormat.YUV411: return Yuv411ToBitmap(data, width, height);
                // RGB/BGR 24/32
                case VideoFrameFormat.RGB24: return RGB24ToBitmap(data, width, height);
                case VideoFrameFormat.BGR24: return BGR24ToBitmap(data, width, height);
                case VideoFrameFormat.RGB32: return RGB32ToBitmap(data, width, height);
                case VideoFrameFormat.BGR32: return BGR32ToBitmap(data, width, height);
                case VideoFrameFormat.ARGB32: return ARGB32ToBitmap(data, width, height);
                case VideoFrameFormat.RGBA32: return RGBA32ToBitmap(data, width, height);
                case VideoFrameFormat.BGRA32: return BGRA32ToBitmap(data, width, height);
                case VideoFrameFormat.ABGR32: return ABGR32ToBitmap(data, width, height);
                // 16 位 RGB
                case VideoFrameFormat.RGB565: return RGB565ToBitmap(data, width, height);
                case VideoFrameFormat.BGR565: return BGR565ToBitmap(data, width, height);
                case VideoFrameFormat.RGB555: return RGB555ToBitmap(data, width, height);
                case VideoFrameFormat.BGR555: return BGR555ToBitmap(data, width, height);
                case VideoFrameFormat.RGB444: return RGB444ToBitmap(data, width, height);
                case VideoFrameFormat.BGR444: return BGR444ToBitmap(data, width, height);
                // 30-bit RGB
                case VideoFrameFormat.RGB30: return RGB30ToBitmap(data, width, height);
                case VideoFrameFormat.BGR30: return BGR30ToBitmap(data, width, height);
                // 灰度（注意：Y8/Y16 为别名，与 Gray8/Gray16 相同，无需重复 case）
                case VideoFrameFormat.Gray8: return Gray8ToBitmap(data, width, height);
                case VideoFrameFormat.Gray16: return Gray16ToBitmap(data, width, height);
                // Bayer
                case VideoFrameFormat.BayerRG8: return BayerToBitmap(data, width, height, BayerPattern.RGGB);
                case VideoFrameFormat.BayerBG8: return BayerToBitmap(data, width, height, BayerPattern.BGGR);
                case VideoFrameFormat.BayerGR8: return BayerToBitmap(data, width, height, BayerPattern.GRBG);
                case VideoFrameFormat.BayerGB8: return BayerToBitmap(data, width, height, BayerPattern.GBRG);
                default:
                    throw new NotSupportedException(HTranslation.GetContent("不支持的视频帧格式") + ": " + format);
            }
        }

        /// <summary>
        /// 获取格式的中文名称（用于 UI 显示）
        /// </summary>
        public static string GetFormatName(VideoFrameFormat format)
        {
            // 使用翻译辅助函数将枚举名称翻译成中文
            return HTranslation.GetContent(format.ToString());
        }

        // ==================== YUV 4:2:0 平面格式 ====================

        /// <summary>YV12 转 Bitmap</summary>
        private static unsafe Bitmap YV12ToBitmap(IntPtr data, int width, int height)
        {
            int ySize = width * height;
            int uvSize = ySize / 4;
            byte* yPlane = (byte*)data;
            byte* vPlane = yPlane + ySize;
            byte* uPlane = vPlane + uvSize;
            return Yuv420PlanesToBitmap(yPlane, uPlane, vPlane, width, height);
        }

        /// <summary>I420 转 Bitmap</summary>
        private static unsafe Bitmap I420ToBitmap(IntPtr data, int width, int height)
        {
            int ySize = width * height;
            int uvSize = ySize / 4;
            byte* yPlane = (byte*)data;
            byte* uPlane = yPlane + ySize;
            byte* vPlane = uPlane + uvSize;
            return Yuv420PlanesToBitmap(yPlane, uPlane, vPlane, width, height);
        }

        /// <summary>
        /// 通用 YUV 4:2:0 平面转 Bitmap（Y、U、V 平面分离）
        /// </summary>
        private static unsafe Bitmap Yuv420PlanesToBitmap(byte* yPlane, byte* uPlane, byte* vPlane, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    int rowY = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int yIndex = rowY + x;
                        int uvIndex = (y / 2) * (width / 2) + (x / 2);
                        int Y = yPlane[yIndex];
                        int U = uPlane[uvIndex];
                        int V = vPlane[uvIndex];
                        ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:2:0 半平面格式 ====================

        /// <summary>NV12 转 Bitmap</summary>
        private static unsafe Bitmap NV12ToBitmap(IntPtr data, int width, int height)
        {
            int ySize = width * height;
            byte* yPlane = (byte*)data;
            byte* uvPlane = yPlane + ySize; // UV 交错，U 在前
            return Yuv420SemiPlanarToBitmap(yPlane, uvPlane, true, width, height);
        }

        /// <summary>NV21 转 Bitmap</summary>
        private static unsafe Bitmap NV21ToBitmap(IntPtr data, int width, int height)
        {
            int ySize = width * height;
            byte* yPlane = (byte*)data;
            byte* vuPlane = yPlane + ySize; // VU 交错，V 在前
            return Yuv420SemiPlanarToBitmap(yPlane, vuPlane, false, width, height);
        }

        /// <summary>
        /// 通用 YUV 4:2:0 半平面转 Bitmap
        /// </summary>
        /// <param name="yPlane">Y 平面指针</param>
        /// <param name="uvPlane">UV 或 VU 交错平面指针</param>
        /// <param name="isUV">true 表示 UV 顺序（U 在前），false 表示 VU 顺序（V 在前）</param>
        private static unsafe Bitmap Yuv420SemiPlanarToBitmap(byte* yPlane, byte* uvPlane, bool isUV, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    int rowY = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int yIndex = rowY + x;
                        int uvIndex = (y / 2) * width + (x / 2) * 2;
                        int Y = yPlane[yIndex];
                        int U, V;
                        if (isUV)
                        {
                            U = uvPlane[uvIndex];
                            V = uvPlane[uvIndex + 1];
                        }
                        else
                        {
                            V = uvPlane[uvIndex];
                            U = uvPlane[uvIndex + 1];
                        }
                        ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:2:0 高位深格式 ====================

        /// <summary>P010（10-bit 4:2:0）转 Bitmap，内部转换为 8-bit 处理</summary>
        private static unsafe Bitmap P010ToBitmap(IntPtr data, int width, int height)
        {
            return ConvertHighBitDepthTo8Bit(data, width, height, true, 10);
        }

        /// <summary>P016（16-bit 4:2:0）转 Bitmap，内部转换为 8-bit 处理</summary>
        private static unsafe Bitmap P016ToBitmap(IntPtr data, int width, int height)
        {
            return ConvertHighBitDepthTo8Bit(data, width, height, true, 16);
        }

        /// <summary>
        /// 将高位深 4:2:0 半平面数据转换为 8-bit 后，再调用 8-bit 半平面转换
        /// </summary>
        private static unsafe Bitmap ConvertHighBitDepthTo8Bit(IntPtr data, int width, int height, bool isUV, int bitDepth)
        {
            int ySize = width * height * 2; // 每个 Y 分量 2 字节
            byte* src = (byte*)data;

            // 分配临时缓冲区存储 8-bit YUV 数据
            byte* y8 = stackalloc byte[width * height];
            byte* uv8 = stackalloc byte[width * height]; // UV 交错，每个像素对两个字节对应一个 UV 对

            // 转换 Y 分量（高位深转 8 位）
            for (int i = 0; i < width * height; i++)
            {
                ushort val = (ushort)(src[i * 2] | (src[i * 2 + 1] << 8));
                y8[i] = (byte)(val >> (bitDepth - 8));
            }

            // 转换 UV 分量（交错，每两个像素共享一对）
            int uvCount = width * height / 2; // UV 对数量
            for (int i = 0; i < uvCount; i++)
            {
                int uvIndex = i * 4; // 每个 UV 对占 4 字节（U 2字节 + V 2字节）
                ushort uVal = (ushort)(src[ySize + uvIndex] | (src[ySize + uvIndex + 1] << 8));
                ushort vVal = (ushort)(src[ySize + uvIndex + 2] | (src[ySize + uvIndex + 3] << 8));
                uv8[i * 2] = (byte)(uVal >> (bitDepth - 8));
                uv8[i * 2 + 1] = (byte)(vVal >> (bitDepth - 8));
            }

            // 调用 8-bit 半平面转换
            return Yuv420SemiPlanarToBitmap(y8, uv8, isUV, width, height);
        }

        // ==================== YUV 4:2:2 打包格式 ====================

        // 内部枚举：定义 4:2:2 打包顺序
        private enum PackedYuvOrder { YUYV, UYVY, YVYU, VYUY }

        /// <summary>
        /// 通用 YUV 4:2:2 打包格式转 Bitmap
        /// </summary>
        private static unsafe Bitmap Yuv422PackedToBitmap(IntPtr data, int width, int height, PackedYuvOrder order)
        {
            byte* src = (byte*)data;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                int bytesPerPixel = 2; // 每个像素平均 2 字节（实际每两个像素 4 字节）
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * bytesPerPixel;
                    for (int x = 0; x < width; x += 2)
                    {
                        int idx = x * 2; // 每个像素对的起始字节索引
                        int Y0, U, Y1, V;
                        // 根据不同的打包顺序提取分量
                        switch (order)
                        {
                            case PackedYuvOrder.YUYV:
                                Y0 = srcRow[idx]; U = srcRow[idx + 1]; Y1 = srcRow[idx + 2]; V = srcRow[idx + 3];
                                break;
                            case PackedYuvOrder.UYVY:
                                U = srcRow[idx]; Y0 = srcRow[idx + 1]; V = srcRow[idx + 2]; Y1 = srcRow[idx + 3];
                                break;
                            case PackedYuvOrder.YVYU:
                                Y0 = srcRow[idx]; V = srcRow[idx + 1]; Y1 = srcRow[idx + 2]; U = srcRow[idx + 3];
                                break;
                            case PackedYuvOrder.VYUY:
                                V = srcRow[idx]; Y0 = srcRow[idx + 1]; U = srcRow[idx + 2]; Y1 = srcRow[idx + 3];
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        // 转换为 RGB 并写入位图
                        ConvertYUVToRGB(Y0, U, V, out int r0, out int g0, out int b0);
                        ConvertYUVToRGB(Y1, U, V, out int r1, out int g1, out int b1);
                        dstRow[x * 3] = (byte)b0;
                        dstRow[x * 3 + 1] = (byte)g0;
                        dstRow[x * 3 + 2] = (byte)r0;
                        dstRow[(x + 1) * 3] = (byte)b1;
                        dstRow[(x + 1) * 3 + 1] = (byte)g1;
                        dstRow[(x + 1) * 3 + 2] = (byte)r1;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:2:2 平面格式 ====================

        /// <summary>YUV422P 转 Bitmap</summary>
        private static unsafe Bitmap Yuv422PlanesToBitmap(IntPtr data, int width, int height)
        {
            int ySize = width * height;
            int uvSize = ySize / 2; // 每两个像素共享一对 UV
            byte* yPlane = (byte*)data;
            byte* uPlane = yPlane + ySize;
            byte* vPlane = uPlane + uvSize;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    int rowY = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int yIndex = rowY + x;
                        int uvIndex = y * (width / 2) + (x / 2);
                        int Y = yPlane[yIndex];
                        int U = uPlane[uvIndex];
                        int V = vPlane[uvIndex];
                        ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:2:2 半平面格式 ====================

        /// <summary>
        /// 通用 YUV 4:2:2 半平面转 Bitmap
        /// </summary>
        private static unsafe Bitmap Yuv422SemiPlanarToBitmap(IntPtr data, int width, int height, bool isUV)
        {
            int ySize = width * height;
            byte* yPlane = (byte*)data;
            byte* uvPlane = yPlane + ySize; // UV 或 VU 交错平面，长度 = width * height

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    int rowY = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int yIndex = rowY + x;
                        int uvIndex = y * width + (x / 2) * 2;
                        int Y = yPlane[yIndex];
                        int U, V;
                        if (isUV)
                        {
                            U = uvPlane[uvIndex];
                            V = uvPlane[uvIndex + 1];
                        }
                        else
                        {
                            V = uvPlane[uvIndex];
                            U = uvPlane[uvIndex + 1];
                        }
                        ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:2:2 10-bit 格式 ====================

        /// <summary>Y210 转 Bitmap（10-bit 打包，每像素 4 字节）</summary>
        private static unsafe Bitmap Y210ToBitmap(IntPtr data, int width, int height)
        {
            // 假设每个像素对（两个像素）占用 4 字节，但实际为 32bit 字，顺序为 Y0(10bit) U(10bit) Y1(10bit) V(10bit)
            byte* src = (byte*)data;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    uint* srcRow = (uint*)(src + y * width * 4); // 每像素 4 字节，但每两个像素实际共享一个 uint
                    for (int x = 0; x < width; x += 2)
                    {
                        uint pixelPair = srcRow[x / 2];
                        // 提取两个像素的 YUV 值（低 10 位有效）
                        int Y0 = (int)(pixelPair & 0x3FF);
                        int U = (int)((pixelPair >> 10) & 0x3FF);
                        int Y1 = (int)((pixelPair >> 20) & 0x3FF);
                        int V = (int)((pixelPair >> 30) & 0x3FF); // 实际可能不足10位，但此处假设
                        // 10-bit 转 8-bit
                        Y0 >>= 2; U >>= 2; Y1 >>= 2; V >>= 2;
                        ConvertYUVToRGB(Y0, U, V, out int r0, out int g0, out int b0);
                        ConvertYUVToRGB(Y1, U, V, out int r1, out int g1, out int b1);
                        dstRow[x * 3] = (byte)b0;
                        dstRow[x * 3 + 1] = (byte)g0;
                        dstRow[x * 3 + 2] = (byte)r0;
                        dstRow[(x + 1) * 3] = (byte)b1;
                        dstRow[(x + 1) * 3 + 1] = (byte)g1;
                        dstRow[(x + 1) * 3 + 2] = (byte)r1;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:4:4 平面格式 ====================

        /// <summary>YUV444P 转 Bitmap</summary>
        private static unsafe Bitmap Yuv444PlanesToBitmap(IntPtr data, int width, int height)
        {
            int planeSize = width * height;
            byte* yPlane = (byte*)data;
            byte* uPlane = yPlane + planeSize;
            byte* vPlane = uPlane + planeSize;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    int rowY = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = rowY + x;
                        int Y = yPlane[idx];
                        int U = uPlane[idx];
                        int V = vPlane[idx];
                        ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:4:4 半平面格式 ====================

        /// <summary>
        /// 通用 YUV 4:4:4 半平面转 Bitmap
        /// </summary>
        private static unsafe Bitmap Yuv444SemiPlanarToBitmap(IntPtr data, int width, int height, bool isUV)
        {
            int ySize = width * height;
            byte* yPlane = (byte*)data;
            byte* uvPlane = yPlane + ySize; // UV 或 VU 交错平面，长度 = width * height * 2

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    int rowY = y * width;
                    for (int x = 0; x < width; x++)
                    {
                        int idx = rowY + x;
                        int uvIndex = idx * 2;
                        int Y = yPlane[idx];
                        int U, V;
                        if (isUV)
                        {
                            U = uvPlane[uvIndex];
                            V = uvPlane[uvIndex + 1];
                        }
                        else
                        {
                            V = uvPlane[uvIndex];
                            U = uvPlane[uvIndex + 1];
                        }
                        ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== YUV 4:1:1 打包格式 ====================

        /// <summary>YUV411 转 Bitmap（常见 UYYVYY 布局）</summary>
        private static unsafe Bitmap Yuv411ToBitmap(IntPtr data, int width, int height)
        {
            // 常见 YUV411 布局：U Y0 Y1 V Y2 Y3（每四个像素共享一对 UV）
            byte* src = (byte*)data;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                int bytesPerGroup = 6; // 每四个像素 6 字节
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * (width / 4) * bytesPerGroup;
                    for (int x = 0; x < width; x += 4)
                    {
                        int groupIndex = (x / 4) * bytesPerGroup;
                        int U = srcRow[groupIndex];
                        int Y0 = srcRow[groupIndex + 1];
                        int Y1 = srcRow[groupIndex + 2];
                        int V = srcRow[groupIndex + 3];
                        int Y2 = srcRow[groupIndex + 4];
                        int Y3 = srcRow[groupIndex + 5];

                        ConvertYUVToRGB(Y0, U, V, out int r0, out int g0, out int b0);
                        ConvertYUVToRGB(Y1, U, V, out int r1, out int g1, out int b1);
                        ConvertYUVToRGB(Y2, U, V, out int r2, out int g2, out int b2);
                        ConvertYUVToRGB(Y3, U, V, out int r3, out int g3, out int b3);

                        dstRow[x * 3] = (byte)b0;
                        dstRow[x * 3 + 1] = (byte)g0;
                        dstRow[x * 3 + 2] = (byte)r0;
                        dstRow[(x + 1) * 3] = (byte)b1;
                        dstRow[(x + 1) * 3 + 1] = (byte)g1;
                        dstRow[(x + 1) * 3 + 2] = (byte)r1;
                        dstRow[(x + 2) * 3] = (byte)b2;
                        dstRow[(x + 2) * 3 + 1] = (byte)g2;
                        dstRow[(x + 2) * 3 + 2] = (byte)r2;
                        dstRow[(x + 3) * 3] = (byte)b3;
                        dstRow[(x + 3) * 3 + 1] = (byte)g3;
                        dstRow[(x + 3) * 3 + 2] = (byte)r3;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== RGB/BGR 格式 ====================

        /// <summary>RGB24 转 Bitmap（转换为 24bppRgb，即 BGR 顺序）</summary>
        private static unsafe Bitmap RGB24ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 3;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 3] = srcRow[x * 3 + 2];     // B
                        dstRow[x * 3 + 1] = srcRow[x * 3 + 1]; // G
                        dstRow[x * 3 + 2] = srcRow[x * 3];     // R
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGR24 转 Bitmap（直接复制，因为 GDI+ 的 24bppRgb 就是 BGR 顺序）</summary>
        private static Bitmap BGR24ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            CopyRows(data, bmpData.Scan0, width, height, 3, bmpData.Stride);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        /// <summary>RGB32（XRGB）转 Bitmap</summary>
        private static unsafe Bitmap RGB32ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 4] = srcRow[x * 4 + 2];     // B
                        dstRow[x * 4 + 1] = srcRow[x * 4 + 1]; // G
                        dstRow[x * 4 + 2] = srcRow[x * 4];     // R
                        dstRow[x * 4 + 3] = 255;               // Alpha
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGR32（XBGR）转 Bitmap</summary>
        private static unsafe Bitmap BGR32ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 4] = srcRow[x * 4];         // B
                        dstRow[x * 4 + 1] = srcRow[x * 4 + 1]; // G
                        dstRow[x * 4 + 2] = srcRow[x * 4 + 2]; // R
                        dstRow[x * 4 + 3] = 255;               // Alpha
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>ARGB32 转 Bitmap</summary>
        private static unsafe Bitmap ARGB32ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 4] = srcRow[x * 4 + 3];     // B
                        dstRow[x * 4 + 1] = srcRow[x * 4 + 2]; // G
                        dstRow[x * 4 + 2] = srcRow[x * 4 + 1]; // R
                        dstRow[x * 4 + 3] = srcRow[x * 4];     // Alpha
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>RGBA32 转 Bitmap</summary>
        private static unsafe Bitmap RGBA32ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 4] = srcRow[x * 4 + 2];     // B
                        dstRow[x * 4 + 1] = srcRow[x * 4 + 1]; // G
                        dstRow[x * 4 + 2] = srcRow[x * 4];     // R
                        dstRow[x * 4 + 3] = srcRow[x * 4 + 3]; // Alpha
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGRA32 转 Bitmap</summary>
        private static unsafe Bitmap BGRA32ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 4] = srcRow[x * 4];         // B
                        dstRow[x * 4 + 1] = srcRow[x * 4 + 1]; // G
                        dstRow[x * 4 + 2] = srcRow[x * 4 + 2]; // R
                        dstRow[x * 4 + 3] = srcRow[x * 4 + 3]; // Alpha
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>ABGR32 转 Bitmap</summary>
        private static unsafe Bitmap ABGR32ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                byte* src = (byte*)data;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width * 4;
                    for (int x = 0; x < width; x++)
                    {
                        dstRow[x * 4] = srcRow[x * 4 + 1];     // B
                        dstRow[x * 4 + 1] = srcRow[x * 4 + 2]; // G
                        dstRow[x * 4 + 2] = srcRow[x * 4 + 3]; // R
                        dstRow[x * 4 + 3] = srcRow[x * 4];     // Alpha
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== 16 位 RGB 格式 ====================

        /// <summary>RGB565 转 Bitmap</summary>
        private static unsafe Bitmap RGB565ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        ushort pixel = srcRow[x];
                        int r = (pixel >> 11) & 0x1F;
                        int g = (pixel >> 5) & 0x3F;
                        int b = pixel & 0x1F;
                        // 扩展到 8 位
                        r = (r << 3) | (r >> 2);
                        g = (g << 2) | (g >> 4);
                        b = (b << 3) | (b >> 2);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGR565 转 Bitmap</summary>
        private static unsafe Bitmap BGR565ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        ushort pixel = srcRow[x];
                        int b = (pixel >> 11) & 0x1F;
                        int g = (pixel >> 5) & 0x3F;
                        int r = pixel & 0x1F;
                        r = (r << 3) | (r >> 2);
                        g = (g << 2) | (g >> 4);
                        b = (b << 3) | (b >> 2);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>RGB555 转 Bitmap</summary>
        private static unsafe Bitmap RGB555ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        ushort pixel = srcRow[x];
                        int r = (pixel >> 10) & 0x1F;
                        int g = (pixel >> 5) & 0x1F;
                        int b = pixel & 0x1F;
                        r = (r << 3) | (r >> 2);
                        g = (g << 3) | (g >> 2);
                        b = (b << 3) | (b >> 2);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGR555 转 Bitmap</summary>
        private static unsafe Bitmap BGR555ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        ushort pixel = srcRow[x];
                        int b = (pixel >> 10) & 0x1F;
                        int g = (pixel >> 5) & 0x1F;
                        int r = pixel & 0x1F;
                        r = (r << 3) | (r >> 2);
                        g = (g << 3) | (g >> 2);
                        b = (b << 3) | (b >> 2);
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>RGB444 转 Bitmap</summary>
        private static unsafe Bitmap RGB444ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        ushort pixel = srcRow[x];
                        int r = (pixel >> 8) & 0x0F;
                        int g = (pixel >> 4) & 0x0F;
                        int b = pixel & 0x0F;
                        r = (r << 4) | r;
                        g = (g << 4) | g;
                        b = (b << 4) | b;
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGR444 转 Bitmap</summary>
        private static unsafe Bitmap BGR444ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        ushort pixel = srcRow[x];
                        int b = (pixel >> 8) & 0x0F;
                        int g = (pixel >> 4) & 0x0F;
                        int r = pixel & 0x0F;
                        r = (r << 4) | r;
                        g = (g << 4) | g;
                        b = (b << 4) | b;
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== 30-bit RGB 格式 ====================

        /// <summary>RGB30 转 Bitmap（每像素 4 字节，每通道 10 位）</summary>
        private static unsafe Bitmap RGB30ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                uint* src = (uint*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    uint* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        uint pixel = srcRow[x];
                        int r = (int)(pixel & 0x3FF);
                        int g = (int)((pixel >> 10) & 0x3FF);
                        int b = (int)((pixel >> 20) & 0x3FF);
                        // 10-bit 转 8-bit
                        r >>= 2; g >>= 2; b >>= 2;
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>BGR30 转 Bitmap</summary>
        private static unsafe Bitmap BGR30ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                uint* src = (uint*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    uint* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        uint pixel = srcRow[x];
                        int b = (int)(pixel & 0x3FF);
                        int g = (int)((pixel >> 10) & 0x3FF);
                        int r = (int)((pixel >> 20) & 0x3FF);
                        r >>= 2; g >>= 2; b >>= 2;
                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== 灰度格式 ====================

        /// <summary>Gray8 转 Bitmap（转换为 24bppRgb）</summary>
        private static unsafe Bitmap Gray8ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* src = (byte*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    byte* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        byte gray = srcRow[x];
                        dstRow[x * 3] = gray;
                        dstRow[x * 3 + 1] = gray;
                        dstRow[x * 3 + 2] = gray;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        /// <summary>Gray16 转 Bitmap（取高 8 位）</summary>
        private static unsafe Bitmap Gray16ToBitmap(IntPtr data, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                ushort* src = (ushort*)data;
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    ushort* srcRow = src + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        byte gray = (byte)(srcRow[x] >> 8);
                        dstRow[x * 3] = gray;
                        dstRow[x * 3 + 1] = gray;
                        dstRow[x * 3 + 2] = gray;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== Bayer 格式 ====================

        // Bayer 模式枚举
        private enum BayerPattern { RGGB, BGGR, GRBG, GBRG }

        /// <summary>
        /// Bayer 格式转 Bitmap（使用简单双线性插值）
        /// </summary>
        private static unsafe Bitmap BayerToBitmap(IntPtr data, int width, int height, BayerPattern pattern)
        {
            byte* src = (byte*)data;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;
                for (int y = 0; y < height; y++)
                {
                    byte* dstRow = dst + y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        bool isRed = false, isGreen = false, isBlue = false;
                        switch (pattern)
                        {
                            case BayerPattern.RGGB:
                                isRed = (x % 2 == 0) && (y % 2 == 0);
                                isBlue = (x % 2 == 1) && (y % 2 == 1);
                                isGreen = !isRed && !isBlue;
                                break;
                            case BayerPattern.BGGR:
                                isBlue = (x % 2 == 0) && (y % 2 == 0);
                                isRed = (x % 2 == 1) && (y % 2 == 1);
                                isGreen = !isRed && !isBlue;
                                break;
                            case BayerPattern.GRBG:
                                isGreen = (x % 2 == 0) && (y % 2 == 0) || (x % 2 == 1) && (y % 2 == 1);
                                isRed = (x % 2 == 1) && (y % 2 == 0);
                                isBlue = (x % 2 == 0) && (y % 2 == 1);
                                break;
                            case BayerPattern.GBRG:
                                isGreen = (x % 2 == 0) && (y % 2 == 0) || (x % 2 == 1) && (y % 2 == 1);
                                isBlue = (x % 2 == 1) && (y % 2 == 0);
                                isRed = (x % 2 == 0) && (y % 2 == 1);
                                break;
                        }

                        int r = 0, g = 0, b = 0;
                        byte center = src[y * width + x];
                        if (isRed)
                        {
                            r = center;
                            int gSum = 0, gCount = 0;
                            if (x > 0) { gSum += src[y * width + x - 1]; gCount++; }
                            if (x < width - 1) { gSum += src[y * width + x + 1]; gCount++; }
                            if (y > 0) { gSum += src[(y - 1) * width + x]; gCount++; }
                            if (y < height - 1) { gSum += src[(y + 1) * width + x]; gCount++; }
                            g = gCount > 0 ? gSum / gCount : 0;
                            int bSum = 0, bCount = 0;
                            if (x > 0 && y > 0) { bSum += src[(y - 1) * width + x - 1]; bCount++; }
                            if (x < width - 1 && y > 0) { bSum += src[(y - 1) * width + x + 1]; bCount++; }
                            if (x > 0 && y < height - 1) { bSum += src[(y + 1) * width + x - 1]; bCount++; }
                            if (x < width - 1 && y < height - 1) { bSum += src[(y + 1) * width + x + 1]; bCount++; }
                            b = bCount > 0 ? bSum / bCount : 0;
                        }
                        else if (isBlue)
                        {
                            b = center;
                            int gSum = 0, gCount = 0;
                            if (x > 0) { gSum += src[y * width + x - 1]; gCount++; }
                            if (x < width - 1) { gSum += src[y * width + x + 1]; gCount++; }
                            if (y > 0) { gSum += src[(y - 1) * width + x]; gCount++; }
                            if (y < height - 1) { gSum += src[(y + 1) * width + x]; gCount++; }
                            g = gCount > 0 ? gSum / gCount : 0;
                            int rSum = 0, rCount = 0;
                            if (x > 0 && y > 0) { rSum += src[(y - 1) * width + x - 1]; rCount++; }
                            if (x < width - 1 && y > 0) { rSum += src[(y - 1) * width + x + 1]; rCount++; }
                            if (x > 0 && y < height - 1) { rSum += src[(y + 1) * width + x - 1]; rCount++; }
                            if (x < width - 1 && y < height - 1) { rSum += src[(y + 1) * width + x + 1]; rCount++; }
                            r = rCount > 0 ? rSum / rCount : 0;
                        }
                        else // Green
                        {
                            g = center;
                            int rSum = 0, rCount = 0, bSum = 0, bCount = 0;
                            if (x > 0) { if (x % 2 == 0) { bSum += src[y * width + x - 1]; bCount++; } else { rSum += src[y * width + x - 1]; rCount++; } }
                            if (x < width - 1) { if (x % 2 == 0) { bSum += src[y * width + x + 1]; bCount++; } else { rSum += src[y * width + x + 1]; rCount++; } }
                            if (y > 0) { if (y % 2 == 0) { bSum += src[(y - 1) * width + x]; bCount++; } else { rSum += src[(y - 1) * width + x]; rCount++; } }
                            if (y < height - 1) { if (y % 2 == 0) { bSum += src[(y + 1) * width + x]; bCount++; } else { rSum += src[(y + 1) * width + x]; rCount++; } }
                            r = rCount > 0 ? rSum / rCount : 0;
                            b = bCount > 0 ? bSum / bCount : 0;
                        }

                        dstRow[x * 3] = (byte)b;
                        dstRow[x * 3 + 1] = (byte)g;
                        dstRow[x * 3 + 2] = (byte)r;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }

        // ==================== 辅助方法 ====================

        /// <summary>
        /// YUV 转 RGB（BT.601 标准）
        /// </summary>
        private static void ConvertYUVToRGB(int y, int u, int v, out int r, out int g, out int b)
        {
            int C = y - 16;
            int D = u - 128;
            int E = v - 128;
            r = (298 * C + 409 * E + 128) >> 8;
            g = (298 * C - 100 * D - 208 * E + 128) >> 8;
            b = (298 * C + 516 * D + 128) >> 8;
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));
        }

        /// <summary>
        /// 逐行内存复制（用于 RGB/BGR 直接复制）
        /// </summary>
        private static void CopyRows(IntPtr src, IntPtr dst, int width, int height, int bytesPerPixel, int dstStride)
        {
            int srcStride = width * bytesPerPixel;
            for (int y = 0; y < height; y++)
            {
                IntPtr srcRow = IntPtr.Add(src, y * srcStride);
                IntPtr dstRow = IntPtr.Add(dst, y * dstStride);
                CopyMemory(dstRow, srcRow, (uint)srcStride);
            }
        }
        /// <summary>
        /// 将 YV12 原始帧直接写入指定的 Bitmap（避免重新分配 Bitmap）。
        /// 注意：Bitmap 必须是 24bppRgb 格式，且尺寸与帧一致。
        /// </summary>
        /// <param name="data">YV12 数据指针</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="bmp">目标 Bitmap（预先创建）</param>
        public static unsafe void ConvertToBitmap(IntPtr data, int width, int height, VideoFrameFormat format, Bitmap bmp)
        {
            if (data == IntPtr.Zero || bmp == null)
                throw new ArgumentNullException();
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb)
                throw new ArgumentException(HTranslation.GetContent("目标 Bitmap 必须为 Format24bppRgb"));
            if (bmp.Width != width || bmp.Height != height)
                throw new ArgumentException(HTranslation.GetContent("Bitmap 尺寸与帧尺寸不一致"));

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                byte* dst = (byte*)bmpData.Scan0;
                int stride = bmpData.Stride;

                switch (format)
                {
                    case VideoFrameFormat.YV12:
                        ConvertYV12ToBitmap(data, width, height, dst, stride);
                        break;
                    // 可在此添加其他格式的支持，例如 NV12、YUYV 等
                    default:
                        throw new NotSupportedException(HTranslation.GetContent($"ConvertToBitmap 暂不支持格式 {format}"));
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }
        }

        /// <summary>ConvertYV12ToBitmap 方法。</summary>
        private static unsafe void ConvertYV12ToBitmap(IntPtr data, int width, int height, byte* dst, int stride)
        {
            int ySize = width * height;
            int uvSize = ySize / 4;
            byte* yPlane = (byte*)data;
            byte* vPlane = yPlane + ySize;
            byte* uPlane = vPlane + uvSize;

            for (int y = 0; y < height; y++)
            {
                byte* dstRow = dst + y * stride;
                int rowY = y * width;
                for (int x = 0; x < width; x++)
                {
                    int yIndex = rowY + x;
                    int uvIndex = (y / 2) * (width / 2) + (x / 2);
                    int Y = yPlane[yIndex];
                    int U = uPlane[uvIndex];
                    int V = vPlane[uvIndex];
                    ConvertYUVToRGB(Y, U, V, out int r, out int g, out int b);
                    dstRow[x * 3] = (byte)b;
                    dstRow[x * 3 + 1] = (byte)g;
                    dstRow[x * 3 + 2] = (byte)r;
                }
            }
        }
        // 内存复制 API
        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint length);
    }

}
