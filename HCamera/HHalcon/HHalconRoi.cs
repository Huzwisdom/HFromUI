using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// ROI（感兴趣区域）生成、域裁剪与区域分割工具（静态方法集）：
    /// 矩形/旋转矩形/圆/椭圆/多边形区域生成，reduce_domain / change_domain 裁剪，
    /// 以及固定阈值、自动阈值（Otsu 等）、动态阈值、双色阈值等区域分割入口。
    /// 生成的 HObject 为 Region，调用方负责 Dispose。
    /// </summary>
    public static class HHalconRoi
    {
        #region ==================== 区域生成 ====================

        /// <summary>生成轴对齐矩形区域（gen_rectangle1）。</summary>
        public static HObject Rectangle1(double row1, double col1, double row2, double col2)
        {
            try
            {
                HObject r;
                HOperatorSet.GenRectangle1(out r, row1, col1, row2, col2);
                return r;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>生成任意角度旋转矩形区域（gen_rectangle2）。</summary>
        /// <param name="row">中心行。</param>
        /// <param name="col">中心列。</param>
        /// <param name="phi">长边方向角（弧度）。</param>
        /// <param name="length1">半长（沿 phi 方向）。</param>
        /// <param name="length2">半宽。</param>
        public static HObject Rectangle2(double row, double col, double phi, double length1, double length2)
        {
            try
            {
                HObject r;
                HOperatorSet.GenRectangle2(out r, row, col, phi, length1, length2);
                return r;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>生成圆区域（gen_circle）。</summary>
        public static HObject Circle(double row, double col, double radius)
        {
            try
            {
                HObject r;
                HOperatorSet.GenCircle(out r, row, col, radius);
                return r;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>生成椭圆区域（gen_ellipse）。</summary>
        /// <param name="phi">长轴方向角（弧度）。</param>
        /// <param name="radius1">长半轴。</param>
        /// <param name="radius2">短半轴。</param>
        public static HObject Ellipse(double row, double col, double phi, double radius1, double radius2)
        {
            try
            {
                HObject r;
                HOperatorSet.GenEllipse(out r, row, col, phi, radius1, radius2);
                return r;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>用顶点序列生成实心多边形区域（gen_region_polygon_filled）。</summary>
        /// <param name="rows">各顶点行坐标。</param>
        /// <param name="cols">各顶点列坐标。</param>
        public static HObject PolygonFilled(double[] rows, double[] cols)
        {
            if (rows == null || cols == null || rows.Length != cols.Length || rows.Length < 3) return null;
            try
            {
                HObject r;
                HOperatorSet.GenRegionPolygonFilled(out r, new HTuple(rows), new HTuple(cols));
                return r;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>生成空区域（gen_empty_region），用于结果初始化/占位。</summary>
        public static HObject Empty()
        {
            HObject r;
            HOperatorSet.GenEmptyRegion(out r);
            return r;
        }

        #endregion

        #region ==================== 域裁剪 ====================

        /// <summary>
        /// 把图像处理范围裁剪到区域内部（reduce_domain）：输出图像尺寸不变、仅域内像素参与后续算子，
        /// 是 ROI 处理最常用的入口（模板制作、局部测量等）。
        /// </summary>
        /// <param name="image">原图像。</param>
        /// <param name="region">ROI 区域。</param>
        public static HObject ReduceDomain(HObject image, HObject region)
        {
            try
            {
                HObject o;
                HOperatorSet.ReduceDomain(image, region, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 直接替换图像定义域（change_domain）：与 reduce_domain 不同，不裁剪区域到图像边界，
        /// 区域被原样作为新域使用。
        /// </summary>
        public static HObject ChangeDomain(HObject image, HObject region)
        {
            try
            {
                HObject o;
                HOperatorSet.ChangeDomain(image, region, out o);
                return o;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>提取图像当前定义域为区域（get_domain）。</summary>
        public static HObject GetDomain(HObject image)
        {
            try
            {
                HObject r;
                HOperatorSet.GetDomain(image, out r);
                return r;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 区域分割 ====================

        /// <summary>固定双阈值分割（threshold）：取灰度落在 [minGray,maxGray] 的像素，输出区域。</summary>
        /// <param name="image">输入图像（通常先 ToGray）。</param>
        /// <param name="minGray">灰度下限（含）。</param>
        /// <param name="maxGray">灰度上限（含）。</param>
        public static HObject Threshold(HObject image, double minGray, double maxGray)
        {
            try
            {
                HObject r;
                HOperatorSet.Threshold(image, out r, minGray, maxGray);
                return r;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 自动单阈值分割（binary_threshold）：自动确定阈值并二值化，适合背景均匀的明场/暗场。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="region">输出二值区域。</param>
        /// <param name="method">阈值法："max_separability"（Otsu 类间方差，默认）/"smooth_histo"。</param>
        /// <param name="lightDark">"dark"=提取比背景暗的目标（默认），"light"=提取亮目标。</param>
        /// <param name="usedThreshold">输出实际采用的阈值。</param>
        public static bool BinaryThreshold(HObject image, out HObject region,
            out double usedThreshold, string method = "max_separability", string lightDark = "dark")
        {
            region = null;
            usedThreshold = 0;
            try
            {
                HTuple t;
                HOperatorSet.BinaryThreshold(image, out region, method ?? "max_separability",
                    lightDark ?? "dark", out t);
                usedThreshold = t.D;
                return true;
            }
            catch (HalconException) { return false; }
        }

        /// <summary>
        /// 直方图自动多阈值分割（auto_threshold）：按直方图波谷把图像分成多块，输出多个区域，
        /// 调用方再用 select_shape/connection 筛选。
        /// </summary>
        /// <param name="sigma">直方图高斯平滑 sigma（越大分块越少，常用 2.0）。</param>
        public static HObject AutoThreshold(HObject image, double sigma = 2.0)
        {
            try
            {
                HObject regions;
                HOperatorSet.AutoThreshold(image, out regions, sigma);
                return regions;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 动态阈值分割（dyn_threshold）：原图与均值/高斯背景图逐像素比较，
        /// 提取 'diff' 大于阈值 offset 的亮/暗目标，适合光照不均、表面凸起/脏污检测。
        /// 调用前需自行准备背景图（如 <see cref="HHalconFilter.MeanImage"/>）。
        /// </summary>
        /// <param name="image">原图。</param>
        /// <param name="background">背景（均值）图。</param>
        /// <param name="offset">判定差值阈值（灰度）。</param>
        /// <param name="lightDark">"light"=比背景亮，"dark"=比背景暗，"equal"=接近，"not_equal"=差异大。</param>
        public static HObject DynThreshold(HObject image, HObject background,
            double offset = 5, string lightDark = "light")
        {
            try
            {
                HObject r;
                HOperatorSet.DynThreshold(image, background, out r, offset, lightDark ?? "light");
                return r;
            }
            catch (HalconException) { return null; }
        }

        #endregion
    }
}
