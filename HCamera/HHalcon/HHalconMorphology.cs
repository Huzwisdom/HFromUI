using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// 形态学处理工具（静态方法集）：区域二值形态学（圆形/矩形结构元的腐蚀、膨胀、开运算、闭运算，
    /// 边界提取、骨架）与灰度形态学（gray_erosion/dilation_shape，用于亮/暗特征的局部极值提取）。
    /// 输入输出均为 HObject（Region 或 Image），调用方负责 Dispose。
    /// </summary>
    public static class HHalconMorphology
    {
        #region ==================== 二值形态学——圆形结构元 ====================

        /// <summary>圆形腐蚀（erosion_circle）：区域整体内缩 radius 像素，去毛刺/断开细颈。</summary>
        public static HObject ErosionCircle(HObject region, double radius)
        {
            try
            {
                HObject o;
                HOperatorSet.ErosionCircle(region, out o, radius);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>圆形膨胀（dilation_circle）：区域外扩 radius 像素，填小孔/桥接缝隙。</summary>
        public static HObject DilationCircle(HObject region, double radius)
        {
            try
            {
                HObject o;
                HOperatorSet.DilationCircle(region, out o, radius);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>圆形开运算（opening_circle）：先腐蚀后膨胀，去除比 radius 小的亮斑/凸起。</summary>
        public static HObject OpeningCircle(HObject region, double radius)
        {
            try
            {
                HObject o;
                HOperatorSet.OpeningCircle(region, out o, radius);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>圆形闭运算（closing_circle）：先膨胀后腐蚀，填补比 radius 小的孔洞/缺口。</summary>
        public static HObject ClosingCircle(HObject region, double radius)
        {
            try
            {
                HObject o;
                HOperatorSet.ClosingCircle(region, out o, radius);
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 二值形态学——矩形结构元 ====================

        /// <summary>矩形腐蚀（erosion_rectangle1）：width×height 矩形结构元内缩。</summary>
        public static HObject ErosionRectangle1(HObject region, int width, int height)
        {
            try
            {
                HObject o;
                HOperatorSet.ErosionRectangle1(region, out o, width, height);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>矩形膨胀（dilation_rectangle1）：width×height 矩形结构元外扩。</summary>
        public static HObject DilationRectangle1(HObject region, int width, int height)
        {
            try
            {
                HObject o;
                HOperatorSet.DilationRectangle1(region, out o, width, height);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>矩形开运算（opening_rectangle1）：适合去除水平/垂直方向的细小干扰。</summary>
        public static HObject OpeningRectangle1(HObject region, int width, int height)
        {
            try
            {
                HObject o;
                HOperatorSet.OpeningRectangle1(region, out o, width, height);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>矩形闭运算（closing_rectangle1）：填补矩形形态的缝隙/断裂。</summary>
        public static HObject ClosingRectangle1(HObject region, int width, int height)
        {
            try
            {
                HObject o;
                HOperatorSet.ClosingRectangle1(region, out o, width, height);
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 边界与骨架 ====================

        /// <summary>
        /// 提取区域边界（boundary）。
        /// </summary>
        /// <param name="boundaryType">"inner"（默认，内边界一像素宽）/"outer"（外边界）/"inner_filled"。</param>
        public static HObject Boundary(HObject region, string boundaryType = "inner")
        {
            try
            {
                HObject o;
                HOperatorSet.Boundary(region, out o, boundaryType ?? "inner");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>提取区域中轴骨架（skeleton）：输出单像素宽线条区域，适合线状目标尺寸/拓扑分析。</summary>
        public static HObject Skeleton(HObject region)
        {
            try
            {
                HObject o;
                HOperatorSet.Skeleton(region, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 灰度形态学 ====================

        /// <summary>
        /// 灰度腐蚀（gray_erosion_shape）：输出邻域内最暗值，用于提取暗特征、压低亮噪点。
        /// </summary>
        /// <param name="image">灰度图。</param>
        /// <param name="maskShape">结构元形状："rectangle1"/"rectangle2"/"octagon"/"rhombus"/"circle"。</param>
        /// <param name="radius">结构元尺寸（像素，矩形时为外接尺度）。</param>
        public static HObject GrayErosion(HObject image, string maskShape = "rectangle1", double radius = 1.5)
        {
            try
            {
                HObject o;
                HOperatorSet.GrayErosionShape(image, out o, radius, radius, maskShape ?? "rectangle1");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 灰度膨胀（gray_dilation_shape）：输出邻域内最亮值，用于提取亮特征、填补暗断点。
        /// </summary>
        /// <param name="image">灰度图。</param>
        /// <param name="maskShape">结构元形状。</param>
        /// <param name="radius">结构元尺寸。</param>
        public static HObject GrayDilation(HObject image, string maskShape = "rectangle1", double radius = 1.5)
        {
            try
            {
                HObject o;
                HOperatorSet.GrayDilationShape(image, out o, radius, radius, maskShape ?? "rectangle1");
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 形态学梯度：灰度膨胀结果减去灰度腐蚀结果，得到边缘强度图（等效 top-hat 边界提取）。
        /// </summary>
        /// <param name="image">灰度图。</param>
        /// <param name="maskShape">结构元形状。</param>
        /// <param name="radius">结构元尺寸。</param>
        public static HObject GrayGradient(HObject image, string maskShape = "rectangle1", double radius = 1.5)
        {
            HObject dil = GrayDilation(image, maskShape, radius);
            HObject ero = GrayErosion(image, maskShape, radius);
            if (dil == null || ero == null)
            {
                if (dil != null) dil.Dispose();
                if (ero != null) ero.Dispose();
                return null;
            }
            try
            {
                HObject g;
                HOperatorSet.SubImage(dil, ero, out g, 1.0, 0.0);
                return g;
            }
            catch (HalconException) { return null; }
            finally
            {
                dil.Dispose();
                ero.Dispose();
            }
        }

        #endregion
    }
}
