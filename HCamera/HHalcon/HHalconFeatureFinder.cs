using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>特征点（角点/鞍点）坐标。</summary>
    public struct HFeaturePoint
    {
        /// <summary>行坐标（亚像素）。</summary>
        public double Row;
        /// <summary>列坐标（亚像素）。</summary>
        public double Column;
    }

    /// <summary>Hough 直线的极坐标参数：Row*sin(Angle)+Col*cos(Angle)=Dist。</summary>
    public struct HLinePolar
    {
        /// <summary>直线法向角（弧度）。</summary>
        public double Angle;
        /// <summary>原点到直线的法向距离（像素）。</summary>
        public double Dist;
    }

    /// <summary>Hough 圆参数。</summary>
    public struct HCircleGeom
    {
        /// <summary>圆心行。</summary>
        public double Row;
        /// <summary>圆心列。</summary>
        public double Column;
        /// <summary>半径（像素）。</summary>
        public double Radius;
    }

    /// <summary>
    /// 特征几何检测（找点 / 找线 / 找圆）静态封装，覆盖 HALCON 三类经典检测算子：
    /// 角点 points_harris（Harris）、points_foerstner（Förstner 角点+区域点）、
    /// 亚像素鞍点 saddle_points_sub_pix（Steger 鞍点/线交点）；
    /// 直线 hough_lines（极坐标直线，抗断裂/遮挡）；圆 hough_circles（区域 Hough 投票，
    /// 再用 fit_circle_contour_xld 拟合圆心半径，适合缺损圆弧）。
    /// 需要从 XLD 轮廓拟合线/圆/椭圆时见 <see cref="HHalconContour"/>；
    /// 点线距离/交点等几何运算见 <see cref="HHalconGeometry"/>。
    /// </summary>
    public static class HHalconFeatureFinder
    {
        #region ==================== 找点 ====================

        /// <summary>
        /// Harris 角点检测（points_harris）。
        /// </summary>
        /// <param name="image">输入灰度图。</param>
        /// <param name="sigmaGrad">梯度平滑系数（常用 0.7~1.5）。</param>
        /// <param name="sigmaSmooth">响应平滑系数（常用 2~4）。</param>
        /// <param name="alpha">Harris 自由参数（常用 0.04~0.08，越小点越多）。</param>
        /// <param name="threshold">响应阈值（越大点越少越可靠，经验值随图调整，如 1000）。</param>
        public static HFeaturePoint[] FindHarrisPoints(HObject image,
            double sigmaGrad = 1.0, double sigmaSmooth = 3.0,
            double alpha = 0.08, double threshold = 1000)
        {
            if (image == null) return new HFeaturePoint[0];
            try
            {
                HTuple rows, cols;
                HOperatorSet.PointsHarris(image, sigmaGrad, sigmaSmooth, alpha, threshold,
                    out rows, out cols);
                return ToPoints(rows.DArr, cols.DArr);
            }
            catch (HalconException)
            {
                return new HFeaturePoint[0];
            }
        }

        /// <summary>
        /// Förstner 特征点检测（points_foerstner），分别输出“角点（junction）”与
        /// “区域中心类点（area）”两组亚像素点及各自协方差。
        /// </summary>
        /// <param name="image">输入灰度图。</param>
        /// <param name="junctionPoints">输出角点（交点、直角顶点等）。</param>
        /// <param name="areaPoints">输出区域类点（圆斑中心、质心类特征）。</param>
        /// <param name="sigmaGrad">梯度平滑（默认 1）。</param>
        /// <param name="sigmaInt">积分窗平滑（默认 3.2）。</param>
        /// <param name="sigmaPoints">亚像素定位平滑（默认 2.4）。</param>
        /// <param name="threshInhom">非均匀性阈值（0~1，默认 0.5，越大越宽松）。</param>
        /// <param name="threshShape">形状阈值（0~1，默认 0.3）。</param>
        /// <returns>true=执行成功。</returns>
        public static bool FindFoerstnerPoints(HObject image,
            out HFeaturePoint[] junctionPoints, out HFeaturePoint[] areaPoints,
            double sigmaGrad = 1.0, double sigmaInt = 3.2, double sigmaPoints = 2.4,
            double threshInhom = 0.5, double threshShape = 0.3)
        {
            junctionPoints = new HFeaturePoint[0];
            areaPoints = new HFeaturePoint[0];
            if (image == null) return false;
            try
            {
                HTuple rj, cj, corrj, corcj, coccj, ra, ca, corra, corca, cocca;
                HOperatorSet.PointsFoerstner(image, sigmaGrad, sigmaInt, sigmaPoints,
                    threshInhom, threshShape, "gaussian", "true",
                    out rj, out cj, out corrj, out corcj, out coccj,
                    out ra, out ca, out corra, out corca, out cocca);
                junctionPoints = ToPoints(rj.DArr, cj.DArr);
                areaPoints = ToPoints(ra.DArr, ca.DArr);
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>
        /// 亚像素鞍点检测（saddle_points_sub_pix）：提取线条交叉/弯曲处的鞍点，
        /// 是 Steger 线提取体系的配套算子，适合网格、栅线交点（标定靶、织物经纬交点）。
        /// </summary>
        /// <param name="image">输入灰度图。</param>
        /// <param name="filter">滤波器："nms"（非极大抑制，默认）/"facet"。</param>
        /// <param name="sigma">平滑系数（常用 1.5，对应线宽）。</param>
        /// <param name="threshold">鞍点响应阈值（越大越严格，常用 5~20）。</param>
        public static HFeaturePoint[] FindSaddlePoints(HObject image,
            string filter = "nms", double sigma = 1.5, double threshold = 5.0)
        {
            if (image == null) return new HFeaturePoint[0];
            try
            {
                HTuple rows, cols;
                HOperatorSet.SaddlePointsSubPix(image, filter ?? "nms", sigma, threshold,
                    out rows, out cols);
                return ToPoints(rows.DArr, cols.DArr);
            }
            catch (HalconException)
            {
                return new HFeaturePoint[0];
            }
        }

        /// <summary>把点坐标数组对转为 <see cref="HFeaturePoint"/> 数组。</summary>
        private static HFeaturePoint[] ToPoints(double[] rows, double[] cols)
        {
            int n = Math.Min(rows.Length, cols.Length);
            HFeaturePoint[] pts = new HFeaturePoint[n];
            for (int i = 0; i < n; i++)
                pts[i] = new HFeaturePoint { Row = rows[i], Column = cols[i] };
            return pts;
        }

        /// <summary>
        /// 为点集生成十字标记 XLD（gen_cross_contour_xld），用于窗口叠加显示；调用方 Dispose。
        /// </summary>
        public static HObject GenCrosses(HFeaturePoint[] points, double size = 8, double angle = 0.7853982)
        {
            if (points == null || points.Length == 0) return null;
            try
            {
                double[] r = new double[points.Length];
                double[] c = new double[points.Length];
                for (int i = 0; i < points.Length; i++) { r[i] = points[i].Row; c[i] = points[i].Column; }
                HObject crosses;
                HOperatorSet.GenCrossContourXld(out crosses, new HTuple(r), new HTuple(c),
                    size, angle);
                return crosses;
            }
            catch (HalconException)
            {
                return null;
            }
        }

        #endregion

        #region ==================== 找线（Hough） ====================

        /// <summary>
        /// Hough 直线检测（hough_lines）：输入二值边缘区域，输出极坐标直线参数。
        /// 对遮挡、断裂、噪声鲁棒；边缘区域可用 threshold + edges_image 等得到。
        /// </summary>
        /// <param name="edgeRegion">二值边缘区域。</param>
        /// <param name="angleResolution">角度分辨率（弧度，常用 0.01，约 0.5°）。</param>
        /// <param name="threshold">投票阈值（像素数，越大直线越长越显著，常用 50~200）。</param>
        /// <param name="angleGap">同一线允许的角度间隙（弧度，常用 5°~10°）。</param>
        /// <param name="distGap">同一线允许的法向距离间隙（像素，常用 5~15）。</param>
        public static HLinePolar[] FindHoughLines(HObject edgeRegion,
            double angleResolution = 0.01, double threshold = 100,
            double angleGap = 0.1, double distGap = 10)
        {
            if (edgeRegion == null) return new HLinePolar[0];
            try
            {
                HTuple angles, dists;
                HOperatorSet.HoughLines(edgeRegion, angleResolution, threshold,
                    angleGap, distGap, out angles, out dists);
                double[] aa = angles.DArr, da = dists.DArr;
                HLinePolar[] lines = new HLinePolar[aa.Length];
                for (int i = 0; i < aa.Length; i++)
                    lines[i] = new HLinePolar
                    {
                        Angle = aa[i],
                        Dist = i < da.Length ? da[i] : 0
                    };
                return lines;
            }
            catch (HalconException)
            {
                return new HLinePolar[0];
            }
        }

        /// <summary>
        /// 把 Hough 极坐标直线裁剪到给定图像范围内，返回两端点（用于显示或测距）。
        /// 直线方程：Row*sin(Angle)+Col*cos(Angle)=Dist；裁剪矩形 [0,height)×[0,width)。
        /// 完全不相交时返回 false。
        /// </summary>
        public static bool PolarToEndpoints(HLinePolar line, int width, int height,
            out double rowBegin, out double colBegin, out double rowEnd, out double colEnd)
        {
            rowBegin = colBegin = rowEnd = colEnd = 0;
            // 法向单位向量 n=(sin,cos)，方向向量 d=(-cos,sin)；垂足 p0=dist*n
            double sn = Math.Sin(line.Angle), cs = Math.Cos(line.Angle);
            double r0 = line.Dist * sn, c0 = line.Dist * cs;
            double dr = -cs, dc = sn;
            // 求 p = p0 + t*d 与四条边界的交点参数 t
            double[] hits = new double[4];
            int cnt = 0;
            // Row = 0 / height
            if (Math.Abs(dr) > 1e-12)
            {
                double t1 = (0 - r0) / dr, t2 = (height - 1 - r0) / dr;
                double cc1 = c0 + t1 * dc, cc2 = c0 + t2 * dc;
                if (cc1 >= 0 && cc1 <= width - 1) hits[cnt++] = t1;
                if (cc2 >= 0 && cc2 <= width - 1) hits[cnt++] = t2;
            }
            // Col = 0 / width
            if (Math.Abs(dc) > 1e-12)
            {
                double t1 = (0 - c0) / dc, t2 = (width - 1 - c0) / dc;
                double rr1 = r0 + t1 * dr, rr2 = r0 + t2 * dr;
                if (rr1 >= 0 && rr1 <= height - 1) hits[cnt++] = t1;
                if (rr2 >= 0 && rr2 <= height - 1) hits[cnt++] = t2;
            }
            if (cnt < 2) return false;
            double tmin = double.MaxValue, tmax = double.MinValue;
            for (int i = 0; i < cnt; i++)
            {
                if (hits[i] < tmin) tmin = hits[i];
                if (hits[i] > tmax) tmax = hits[i];
            }
            rowBegin = r0 + tmin * dr;
            colBegin = c0 + tmin * dc;
            rowEnd = r0 + tmax * dr;
            colEnd = c0 + tmax * dc;
            return true;
        }

        #endregion

        #region ==================== 找圆（Hough + 拟合） ====================

        /// <summary>
        /// Hough 圆检测（hough_circles + 连通 + fit_circle_contour_xld）：
        /// 输入二值边缘区域，输出每个检出圆的圆心与半径。对部分缺损圆弧仍可检出。
        /// </summary>
        /// <param name="edgeRegion">二值边缘区域。</param>
        /// <param name="radiusMin">最小半径（像素）。</param>
        /// <param name="radiusMax">最大半径（像素）。</param>
        /// <param name="percent">一个圆至少被边缘覆盖的百分比（0~100，默认 50）。</param>
        /// <param name="mode">"classic"（经典二值边缘，默认）/"gradient"（用梯度方向，更快更抗噪）。</param>
        public static HCircleGeom[] FindHoughCircles(HObject edgeRegion,
            double radiusMin, double radiusMax, double percent = 50, string mode = "classic")
        {
            if (edgeRegion == null) return new HCircleGeom[0];
            HObject circlesRegion = null;
            HObject conn = null;
            HObject contours = null;
            try
            {
                HOperatorSet.HoughCircles(edgeRegion, out circlesRegion,
                    new HTuple(new double[] { radiusMin, radiusMax }),
                    percent, mode ?? "classic");
                HOperatorSet.Connection(circlesRegion, out conn);
                HOperatorSet.GenContourRegionXld(conn, out contours, "border");

                HTuple rows, cols, radii, sp, ep, order;
                HOperatorSet.FitCircleContourXld(contours, "ahuber", 0, 3.28, 0, 3, 2,
                    out rows, out cols, out radii, out sp, out ep, out order);
                double[] ra = rows.DArr, ca = cols.DArr, ka = radii.DArr;
                HCircleGeom[] res = new HCircleGeom[ra.Length];
                for (int i = 0; i < ra.Length; i++)
                    res[i] = new HCircleGeom
                    {
                        Row = ra[i],
                        Column = i < ca.Length ? ca[i] : 0,
                        Radius = i < ka.Length ? ka[i] : 0
                    };
                return res;
            }
            catch (HalconException)
            {
                return new HCircleGeom[0];
            }
            finally
            {
                if (circlesRegion != null) try { circlesRegion.Dispose(); } catch { }
                if (conn != null) try { conn.Dispose(); } catch { }
                if (contours != null) try { contours.Dispose(); } catch { }
            }
        }

        #endregion
    }
}
