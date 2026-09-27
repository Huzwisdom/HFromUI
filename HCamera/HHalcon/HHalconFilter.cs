using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// 图像预处理——滤波、增强、边缘幅度与频域 FFT 工具（静态方法集）：
    /// 均值/高斯/中值滤波、局部对比度增强（emphasize）、直方图均衡、Sobel 边缘幅度、自定义掩膜卷积，
    /// 以及 rft_generic → gen_filter_mask/convol_fft → fft_generic 的完整频域滤波链路（低通/高通）、
    /// Gabor 滤波器生成。全部输出 HObject 由调用方 Dispose。
    /// </summary>
    public static class HHalconFilter
    {
        #region ==================== 平滑滤波 ====================

        /// <summary>均值滤波（mean_image）：maskWidth×maskHeight 矩形邻域均值，降噪同时会模糊边缘。</summary>
        /// <param name="maskWidth">掩膜宽（奇数，如 3/5/9）。</param>
        /// <param name="maskHeight">掩膜高（奇数）。</param>
        public static HObject MeanImage(HObject image, int maskWidth = 3, int maskHeight = 3)
        {
            try
            {
                HObject o;
                HOperatorSet.MeanImage(image, out o, maskWidth, maskHeight);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>高斯滤波（gauss_image）：按高斯权重平滑，边缘保持优于均值滤波。</summary>
        /// <param name="size">掩膜尺寸：3/5/7/9/11（越大越平滑）。</param>
        public static HObject GaussImage(HObject image, int size = 5)
        {
            try
            {
                HObject o;
                HOperatorSet.GaussImage(image, out o, size);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 中值滤波（median_image）：取邻域中值，对椒盐噪声效果好。
        /// </summary>
        /// <param name="maskType">邻域形状："circle"（默认）/"square"。</param>
        /// <param name="radius">邻域半径（1/2/3...）。</param>
        /// <param name="margin">边界处理："mirrored"（默认）/"continued"。</param>
        public static HObject MedianImage(HObject image,
            string maskType = "circle", int radius = 1, string margin = "mirrored")
        {
            try
            {
                HObject o;
                HOperatorSet.MedianImage(image, out o, maskType ?? "circle", radius, margin ?? "mirrored");
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 增强 ====================

        /// <summary>
        /// 局部对比度增强（emphasize）：在 maskWidth×maskHeight 邻域内放大与局部均值的差异，
        /// factor 为增强系数（常用 1.0~3.0），用于弱化阴影、突出暗纹/划痕。
        /// </summary>
        public static HObject Emphasize(HObject image, int maskWidth = 7, int maskHeight = 7, double factor = 1.5)
        {
            try
            {
                HObject o;
                HOperatorSet.Emphasize(image, out o, maskWidth, maskHeight, factor);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>直方图均衡化（equ_histo_image）：自动拉开灰度分布，改善整体偏暗/偏亮。</summary>
        public static HObject EquHisto(HObject image)
        {
            try
            {
                HObject o;
                HOperatorSet.EquHistoImage(image, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 边缘幅度与自定义卷积 ====================

        /// <summary>
        /// Sobel 边缘幅度（sobel_amp）：输出各点边缘强度灰度图，可直接阈值化。
        /// </summary>
        /// <param name="filterType">滤波器形式："sum_abs"（默认，快）/"sum_sqrt"/"thin_max_abs" 等。</param>
        /// <param name="size">掩膜尺寸 3/5/7。</param>
        public static HObject SobelAmp(HObject image, string filterType = "sum_abs", int size = 3)
        {
            try
            {
                HObject o;
                HOperatorSet.SobelAmp(image, out o, filterType ?? "sum_abs", size);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 用自定义数值掩膜做卷积（convol_image），掩膜用 gen_rectangle1/gen_gabor 等生成或自行构造。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="filterMask">HALCON 掩膜图像（与图像同域的灰度图）。</param>
        /// <param name="margin">边界处理："mirrored"（默认）/"continued"。</param>
        public static HObject Convol(HObject image, HObject filterMask, string margin = "mirrored")
        {
            try
            {
                HObject o;
                // 本版 dotnet 封装中卷积掩膜以对象键句柄传入
                HOperatorSet.ConvolImage(image, out o, new HTuple(filterMask.Key), margin ?? "mirrored");
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== FFT 频域 ====================

        /// <summary>
        /// 正/反实数快速傅里叶变换（rft_generic）。
        /// </summary>
        /// <param name="image">输入：正变换给灰度图，反变换给频域复数图。</param>
        /// <param name="toFrequency">true=空域→频域（direction='to_freq'）；false=频域→空域（'from_freq'）。</param>
        /// <param name="width">正变换时的频域宽度优化（一般传图像宽）；反变换传 0。</param>
        public static HObject Rft(HObject image, bool toFrequency, int width = 0)
        {
            try
            {
                HObject o;
                HOperatorSet.RftGeneric(image, out o,
                    toFrequency ? "to_freq" : "from_freq", "none", "complex", toFrequency ? width : 0);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 生成频域滤波掩膜（gen_filter_mask）。
        /// </summary>
        /// <param name="width">图像/频域宽。</param>
        /// <param name="height">图像高。</param>
        /// <param name="maskType">掩膜类型："inner_filled"/"outer_filled"/"inner_normalized"/"outer_normalized"。</param>
        /// <param name="scale">掩膜截止尺寸参数（内/外半径语义，按 maskType 解释）。</param>
        public static HObject GenFilterMask(int width, int height, string maskType = "inner_filled", double scale = 0.1)
        {
            try
            {
                HObject m;
                HOperatorSet.GenFilterMask(out m, maskType ?? "inner_filled", scale, width, height);
                return m;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>频域相乘（convol_fft）：频域图与滤波掩膜逐点相乘，输出滤波后的频域图。</summary>
        /// <param name="imageFft">rft_generic 正变换得到的频域图。</param>
        /// <param name="filterMask">gen_filter_mask/gen_gabor 生成的频域掩膜。</param>
        public static HObject ConvolFft(HObject imageFft, HObject filterMask)
        {
            try
            {
                HObject o;
                HOperatorSet.ConvolFft(imageFft, filterMask, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 一步完成频域滤波：正变换 → 频域相乘 → 反变换，返回空域滤波结果。
        /// 典型用途：inner_filled=低通（去噪/模糊），outer_filled=高通（去背景/提边缘）。
        /// </summary>
        /// <param name="image">输入灰度图。</param>
        /// <param name="maskType">"inner_filled"=低通；"outer_filled"=高通。</param>
        /// <param name="scale">截止半径比例/尺寸参数。</param>
        public static HObject FftFilter(HObject image, string maskType = "inner_filled", double scale = 0.1)
        {
            int w, h;
            if (!HHalconImage.GetSize(image, out w, out h)) return null;
            HObject fft = null;
            HObject mask = null;
            HObject conved = null;
            HObject result = null;
            try
            {
                fft = Rft(image, true, w);
                if (fft == null) return null;
                mask = GenFilterMask(w, h, maskType, scale);
                if (mask == null) return null;
                conved = ConvolFft(fft, mask);
                if (conved == null) return null;
                result = Rft(conved, false, 0);
                return result;
            }
            catch (HalconException)
            {
                if (result != null) result.Dispose();
                return null;
            }
            finally
            {
                if (fft != null) fft.Dispose();
                if (mask != null) mask.Dispose();
                if (conved != null) conved.Dispose();
            }
        }

        /// <summary>
        /// 生成 Gabor 滤波器（gen_gabor）：对特定方向/频率的纹理响应强烈，
        /// 可配合 convol_fft 做方向性纹理/划痕增强。
        /// </summary>
        /// <param name="angle">方向角（弧度）。</param>
        /// <param name="frequency">中心频率。</param>
        /// <param name="bandwidth">频率带宽。</param>
        /// <param name="orientation">方向带宽。</param>
        /// <param name="width">掩膜宽。</param>
        /// <param name="height">掩膜高。</param>
        public static HObject Gabor(double angle, double frequency, double bandwidth,
            double orientation, int width, int height)
        {
            try
            {
                HObject g;
                HOperatorSet.GenGabor(out g, angle, frequency, bandwidth, orientation,
                    "none", "dc_center", width, height);
                return g;
            }
            catch (HalconException) { return null; }
        }

        #endregion
    }
}
