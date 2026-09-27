using HalconDotNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 图像基础工具（静态方法集）：图像读写、通道分解/合成/灰度化、
    /// 像素缩放/反相/差分、几何变换（旋转/镜像/缩放/裁剪/仿射）、尺寸查询，
    /// 以及 HImage(HObject) 与 System.Drawing.Bitmap 的高速互转（byte 灰度 / 24 位彩色，指针直拷）。
    /// </summary>
    public static class HHalconImage
    {
        #region ==================== 读写 ====================

        /// <summary>从文件读图像（read_image），支持 bmp/jpg/png/tiff 等 HALCON 全部图像格式；失败返回 null。</summary>
        /// <param name="fileName">图像文件完整路径。</param>
        public static HObject Read(string fileName)
        {
            try
            {
                HObject img;
                HOperatorSet.ReadImage(out img, fileName);
                return img;
            }
            catch (HalconException)
            {
                return null;
            }
        }

        /// <summary>
        /// 写图像到文件（write_image）。
        /// </summary>
        /// <param name="image">待写图像。</param>
        /// <param name="fileName">目标路径（扩展名决定容器，HALCON 部分格式按 format 参数决定）。</param>
        /// <param name="format">HALCON 图像格式："bmp"、"jpeg"、"png"、"tiff" 等。</param>
        /// <param name="fillColor">区域外/透明部分填充灰度值（一般传 0）。</param>
        public static bool Write(HObject image, string fileName, string format = "png", int fillColor = 0)
        {
            if (image == null) return false;
            try
            {
                HOperatorSet.WriteImage(image, format ?? "png", fillColor, fileName);
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        #endregion

        #region ==================== 尺寸与通道 ====================

        /// <summary>获取图像宽高（get_image_size）。</summary>
        public static bool GetSize(HObject image, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (image == null) return false;
            try
            {
                HTuple w, h;
                HOperatorSet.GetImageSize(image, out w, out h);
                width = w.I;
                height = h.I;
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>获取通道数（count_channels），灰度=1，RGB=3；失败返回 0。</summary>
        public static int CountChannels(HObject image)
        {
            if (image == null) return 0;
            try
            {
                HTuple c;
                HOperatorSet.CountChannels(image, out c);
                return c.I;
            }
            catch (HalconException)
            {
                return 0;
            }
        }

        /// <summary>获取像素类型（get_image_type），如 byte/uint2/real；失败返回空串。</summary>
        public static string GetType(HObject image)
        {
            if (image == null) return string.Empty;
            try
            {
                HTuple t;
                HOperatorSet.GetImageType(image, out t);
                return t.S ?? string.Empty;
            }
            catch (HalconException)
            {
                return string.Empty;
            }
        }

        /// <summary>把 3 通道图像分解为 R/G/B 三个单通道图像（decompose3）；成功后三个对象由调用方释放。</summary>
        public static bool Decompose3(HObject rgb, out HObject r, out HObject g, out HObject b)
        {
            r = g = b = null;
            if (rgb == null) return false;
            try
            {
                HOperatorSet.Decompose3(rgb, out r, out g, out b);
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>把 R/G/B 三个单通道图像合成彩色图像（compose3）。</summary>
        public static HObject Compose3(HObject r, HObject g, HObject b)
        {
            try
            {
                HObject rgb;
                HOperatorSet.Compose3(r, g, b, out rgb);
                return rgb;
            }
            catch (HalconException)
            {
                return null;
            }
        }

        /// <summary>彩色/单通道图像统一转 byte 灰度（RGB 走 rgb3_to_gray 权重公式，单通道原样拷贝）。</summary>
        public static HObject ToGray(HObject image)
        {
            if (image == null) return null;
            try
            {
                HObject gray;
                if (CountChannels(image) >= 3)
                {
                    HObject r, g, b;
                    HOperatorSet.Decompose3(image, out r, out g, out b);
                    HOperatorSet.Rgb3ToGray(r, g, b, out gray);
                    r.Dispose();
                    g.Dispose();
                    b.Dispose();
                }
                else
                {
                    HOperatorSet.Rgb1ToGray(image, out gray);
                }
                return gray;
            }
            catch (HalconException)
            {
                return null;
            }
        }

        #endregion

        #region ==================== 灰度运算 ====================

        /// <summary>线性缩放灰度（scale_image）：g' = g * factor + offset（自动钳位到 0~255）。</summary>
        public static HObject Scale(HObject image, double factor, double offset)
        {
            try
            {
                HObject o;
                HOperatorSet.ScaleImage(image, out o, factor, offset);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>灰度反相（invert_image）：g' = 255 - g（byte 图）。</summary>
        public static HObject Invert(HObject image)
        {
            try
            {
                HObject o;
                HOperatorSet.InvertImage(image, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>两幅图灰度相减（sub_image）：(g1 - g2) * mult + add。</summary>
        public static HObject Sub(HObject image1, HObject image2, double mult = 1.0, double add = 0.0)
        {
            try
            {
                HObject o;
                HOperatorSet.SubImage(image1, image2, out o, mult, add);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>两幅图绝对差（abs_diff_image）：|g1 - g2| * mult，常用于帧差检测。</summary>
        public static HObject AbsDiff(HObject image1, HObject image2, double mult = 1.0)
        {
            try
            {
                HObject o;
                HOperatorSet.AbsDiffImage(image1, image2, out o, mult);
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 几何变换 ====================

        /// <summary>
        /// 绕图像中心旋转（rotate_image，扩展画布以容纳整图）。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="phi">旋转角（弧度，正=逆时针）。</param>
        /// <param name="interpolation">插值："constant"（快）/"nearest neighbor"/"bilinear"。</param>
        public static HObject Rotate(HObject image, double phi, string interpolation = "constant")
        {
            try
            {
                HObject o;
                HOperatorSet.RotateImage(image, out o, phi, interpolation ?? "constant");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 镜像翻转（mirror_image）。
        /// </summary>
        /// <param name="mode">"row"=上下翻转，"column"=左右翻转，"diagonal"=对角翻转（转置）。</param>
        public static HObject Mirror(HObject image, string mode = "column")
        {
            try
            {
                HObject o;
                HOperatorSet.MirrorImage(image, out o, mode ?? "column");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>缩放到指定尺寸（zoom_image_size）。</summary>
        /// <param name="width">目标宽。</param>
        /// <param name="height">目标高。</param>
        /// <param name="interpolation">"constant"/"nearest neighbor"/"bilinear"。</param>
        public static HObject Zoom(HObject image, int width, int height, string interpolation = "bilinear")
        {
            try
            {
                HObject o;
                HOperatorSet.ZoomImageSize(image, out o, width, height, interpolation ?? "bilinear");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>从图像中裁剪矩形区域（crop_part），输出尺寸固定为 width×height，越界部分填充。</summary>
        /// <param name="row">裁剪区起始行。</param>
        /// <param name="column">裁剪区起始列。</param>
        /// <param name="width">裁剪宽。</param>
        /// <param name="height">裁剪高。</param>
        public static HObject Crop(HObject image, double row, double column, double width, double height)
        {
            try
            {
                HObject o;
                HOperatorSet.CropPart(image, out o, row, column, width, height);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>用两个角点裁剪轴对齐矩形（crop_rectangle1），输出紧贴矩形内容。</summary>
        public static HObject CropRectangle1(HObject image, double row1, double col1, double row2, double col2)
        {
            try
            {
                HObject o;
                HOperatorSet.CropRectangle1(image, out o, row1, col1, row2, col2);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>按 2D 齐次变换矩阵做仿射变换（affine_trans_image）。</summary>
        /// <param name="image">输入图像。</param>
        /// <param name="homMat2D">齐次变换矩阵（hom_mat2d_* 系列构造）。</param>
        /// <param name="interpolation">插值方式。</param>
        /// <param name="adaptImageSize">"true"=输出尺寸随变换扩展，"false"=保持原尺寸。</param>
        public static HObject AffineTrans(HObject image, HTuple homMat2D,
            string interpolation = "constant", string adaptImageSize = "true")
        {
            try
            {
                HObject o;
                HOperatorSet.AffineTransImage(image, out o, homMat2D, interpolation ?? "constant",
                    adaptImageSize ?? "true");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>单位 2D 齐次矩阵（hom_mat2d_identity）。</summary>
        public static HTuple HomMat2DIdentity()
        {
            HTuple m;
            HOperatorSet.HomMat2dIdentity(out m);
            return m;
        }

        #endregion

        #region ==================== Bitmap 互转 ====================

        /// <summary>
        /// HALCON 图像转 System.Drawing.Bitmap：
        /// 单通道 byte 转 8bpp 灰度位图（带灰度调色板）；3 通道 byte 转 24bpp 位图（R/G/B 行拷贝）。
        /// 其它位深/通道数返回 null。返回的 Bitmap 由调用方 Dispose。
        /// </summary>
        /// <param name="image">HALCON 图像。</param>
        public static Bitmap ToBitmap(HObject image)
        {
            if (image == null) return null;
            try
            {
                int ch = CountChannels(image);
                HTuple w, h;
                HOperatorSet.GetImageSize(image, out w, out h);
                int width = w.I, height = h.I;

                if (ch == 1)
                {
                    HTuple ptr, type;
                    HOperatorSet.GetImagePointer1(image, out ptr, out type, out w, out h);
                    if (type.S != "byte") return null;
                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                    ColorPalette pal = bmp.Palette;
                    for (int i = 0; i < 256; i++) pal.Entries[i] = Color.FromArgb(i, i, i);
                    bmp.Palette = pal;
                    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                    int stride = bd.Stride;
                    byte[] buf = new byte[stride * height];
                    Marshal.Copy(ptr.IP, buf, 0, width * height);
                    if (stride == width)
                    {
                        Marshal.Copy(buf, 0, bd.Scan0, width * height);
                    }
                    else
                    {
                        // HALCON 数据紧密排列，位图按 stride 逐行拷贝
                        for (int r = 0; r < height; r++)
                            Marshal.Copy(buf, r * width, IntPtr.Add(bd.Scan0, r * stride), width);
                    }
                    bmp.UnlockBits(bd);
                    return bmp;
                }

                if (ch == 3)
                {
                    HTuple pR, pG, pB, type;
                    HOperatorSet.GetImagePointer3(image, out pR, out pG, out pB, out type, out w, out h);
                    if (type.S != "byte") return null;
                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height),
                        ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    int stride = bd.Stride;
                    byte[] rr = new byte[width * height];
                    byte[] gg = new byte[width * height];
                    byte[] bb = new byte[width * height];
                    Marshal.Copy(pR.IP, rr, 0, rr.Length);
                    Marshal.Copy(pG.IP, gg, 0, gg.Length);
                    Marshal.Copy(pB.IP, bb, 0, bb.Length);
                    byte[] line = new byte[stride];
                    for (int row = 0; row < height; row++)
                    {
                        int off = row * width;
                        for (int col = 0; col < width; col++)
                        {
                            line[col * 3] = bb[off + col];     // B
                            line[col * 3 + 1] = gg[off + col]; // G
                            line[col * 3 + 2] = rr[off + col]; // R
                        }
                        Marshal.Copy(line, 0, IntPtr.Add(bd.Scan0, row * stride), stride);
                    }
                    bmp.UnlockBits(bd);
                    return bmp;
                }
                return null;
            }
            catch (HalconException)
            {
                return null;
            }
        }

        /// <summary>
        /// System.Drawing.Bitmap 转 HALCON 图像：
        /// 8bpp/16bpp/32bpp 灰度类位图 → "byte" 单通道；24bpp/32bpp 彩色 → "byte" 三通道（rgb）。
        /// 返回的 HObject 由调用方 Dispose。
        /// </summary>
        /// <param name="bmp">源位图。</param>
        public static HObject FromBitmap(Bitmap bmp)
        {
            if (bmp == null) return null;
            try
            {
                int width = bmp.Width;
                int height = bmp.Height;
                Rectangle rect = new Rectangle(0, 0, width, height);
                bool color = bmp.PixelFormat == PixelFormat.Format24bppRgb ||
                             bmp.PixelFormat == PixelFormat.Format32bppRgb ||
                             bmp.PixelFormat == PixelFormat.Format32bppArgb;

                BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly,
                    color ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed);
                try
                {
                    int stride = bd.Stride;
                    int len = width * height;
                    IntPtr p = bd.Scan0;

                    if (!color)
                    {
                        // 非紧密排列时逐行收拢
                        byte[] gray = new byte[len];
                        if (stride == width)
                        {
                            Marshal.Copy(p, gray, 0, len);
                        }
                        else
                        {
                            for (int r = 0; r < height; r++)
                                Marshal.Copy(IntPtr.Add(p, r * stride), gray, r * width, width);
                        }
                        GCHandle handle = GCHandle.Alloc(gray, GCHandleType.Pinned);
                        try
                        {
                            HObject img;
                            HOperatorSet.GenImage1(out img, "byte", width, height, handle.AddrOfPinnedObject());
                            return img;
                        }
                        finally { handle.Free(); }
                    }
                    else
                    {
                        // 24bpp：内存即 BGR 交错排列，gen_image_interleaved 直接生成 rgb 图
                        HObject img;
                        if (stride == width * 3)
                        {
                            HOperatorSet.GenImageInterleaved(out img, p, "bgr",
                                width, height, 0, "byte", width, height, 0, 0, 8, 0);
                        }
                        else
                        {
                            // stride 含尾部填充：逐行收拢成紧密 BGR
                            byte[] bgr = new byte[width * height * 3];
                            byte[] line = new byte[stride];
                            for (int r = 0; r < height; r++)
                            {
                                Marshal.Copy(IntPtr.Add(p, r * stride), line, 0, stride);
                                Buffer.BlockCopy(line, 0, bgr, r * width * 3, width * 3);
                            }
                            GCHandle handle = GCHandle.Alloc(bgr, GCHandleType.Pinned);
                            try
                            {
                                HOperatorSet.GenImageInterleaved(out img, handle.AddrOfPinnedObject(), "bgr",
                                    width, height, 0, "byte", width, height, 0, 0, 8, 0);
                            }
                            finally { handle.Free(); }
                        }
                        return img;
                    }
                }
                finally
                {
                    bmp.UnlockBits(bd);
                }
            }
            catch (HalconException)
            {
                return null;
            }
        }

        #endregion
    }
}
