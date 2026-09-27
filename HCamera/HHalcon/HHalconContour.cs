using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    #region ==================== 拟合结果结构 ====================

    /// <summary>XLD 直线拟合结果（fit_line_contour_xld）。</summary>
    public struct HXldLine
    {
        /// <summary>起点行。</summary>
        public double RowBegin;
        /// <summary>起点列。</summary>
        public double ColBegin;
        /// <summary>终点行。</summary>
        public double RowEnd;
        /// <summary>终点列。</summary>
        public double ColEnd;
        /// <summary>直线方程法向量行分量。</summary>
        public double Nr;
        /// <summary>直线方程法向量列分量。</summary>
        public double Nc;
        /// <summary>直线到原点距离。</summary>
        public double Dist;
    }

    /// <summary>XLD 圆拟合结果（fit_circle_contour_xld）。</summary>
    public struct HXldCircle
    {
        /// <summary>圆心行。</summary>
        public double Row;
        /// <summary>圆心列。</summary>
        public double Column;
        /// <summary>半径。</summary>
        public double Radius;
        /// <summary>起始角（弧度，圆弧）。</summary>
        public double StartPhi;
        /// <summary>终止角（弧度，圆弧）。</summary>
        public double EndPhi;
        /// <summary>绕向：0=顺时针正，1=逆时针正。</summary>
        public double PointOrder;
    }

    /// <summary>XLD 椭圆拟合结果（fit_ellipse_contour_xld）。</summary>
    public struct HXldEllipse
    {
        /// <summary>中心行。</summary>
        public double Row;
        /// <summary>中心列。</summary>
        public double Column;
        /// <summary>长轴方向角（弧度）。</summary>
        public double Phi;
        /// <summary>长半轴。</summary>
        public double Radius1;
        /// <summary>短半轴。</summary>
        public double Radius2;
        /// <summary>起始角。</summary>
        public double StartPhi;
        /// <summary>终止角。</summary>
        public double EndPhi;
        /// <summary>绕向。</summary>
        public double PointOrder;
    }

    /// <summary>XLD 旋转矩形拟合结果（fit_rectangle2_contour_xld）。</summary>
    public struct HXldRectangle2
    {
        /// <summary>中心行。</summary>
        public double Row;
        /// <summary>中心列。</summary>
        public double Column;
        /// <summary>长边方向角（弧度）。</summary>
        public double Phi;
        /// <summary>半长。</summary>
        public double Length1;
        /// <summary>半宽。</summary>
        public double Length2;
        /// <summary>绕向。</summary>
        public double PointOrder;
    }

    #endregion

    /// <summary>
    /// XLD 亚像素轮廓分析工具（静态方法集）：
    /// edges_sub_pix 亚像素边缘（Canny/Deriche/Lanser/Shen 等滤波，定位精度可达 1/50 像素）、
    /// lines_gauss Steger 线提取（亮线/暗线中心及线宽）、轮廓筛选/分割/共线合并、
    /// 直线/圆/椭圆/旋转矩形最小二乘（含 Huber/Tukey 鲁棒）拟合、轮廓长度/面积中心/坐标点读取，
    /// 以及按点列生成多边形 XLD。输出轮廓 HObject 由调用方 Dispose。
    /// </summary>
    public static class HHalconContour
    {
        #region ==================== 亚像素边缘/线提取 ====================

        /// <summary>
        /// 亚像素边缘提取（edges_sub_pix）。
        /// </summary>
        /// <param name="image">输入图像（建议灰度）。</param>
        /// <param name="filter">
        /// 滤波器："canny"（经典 Canny）、"deriche1"/"deriche2"（Deriche 递归，默认）、
        /// "lanser1"/"lanser2"、"shen"、"mshen"。
        /// </param>
        /// <param name="alpha">滤波平滑系数（越小越平滑，Deriche 常用 1.0，Canny 常用 1.0~2.0）。</param>
        /// <param name="low">滞后阈值低门限。</param>
        /// <param name="high">滞后阈值高门限。</param>
        /// <returns>XLD 轮廓集；失败 null。</returns>
        public static HObject EdgesSubPix(HObject image,
            string filter = "deriche1", double alpha = 1.0, double low = 20, double high = 40)
        {
            try
            {
                HObject edges;
                HOperatorSet.EdgesSubPix(image, out edges, filter ?? "deriche1", alpha, low, high);
                return edges;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// Steger 亚像素线提取（lines_gauss）：提取曲线状亮线/暗线的中心线，精度优于普通边缘检测。
        /// </summary>
        /// <param name="image">输入图像。</param>
        /// <param name="sigma">高斯平滑 sigma（常用 1.5/2.5/3.5，线越粗越大）。</param>
        /// <param name="low">滞后阈值低门限。</param>
        /// <param name="high">滞后阈值高门限。</param>
        /// <param name="lightDark">"light"（默认）提取亮线，"dark" 提取暗线，"all" 两者都提。</param>
        /// <param name="extractWidth">"true"=同时提取线宽，"false"=只提中心线。</param>
        /// <param name="lineModel">"none"（默认）/"bar-shaped"/"gaussian"（配合线宽）。</param>
        /// <param name="completeJunctions">"true"=补全交叉点连接，"false"=不补。</param>
        public static HObject LinesGauss(HObject image, double sigma = 1.5,
            double low = 3, double high = 8, string lightDark = "light",
            string extractWidth = "true", string lineModel = "bar-shaped", string completeJunctions = "true")
        {
            try
            {
                HObject lines;
                HOperatorSet.LinesGauss(image, out lines, sigma, low, high,
                    lightDark ?? "light", extractWidth ?? "true",
                    lineModel ?? "bar-shaped", completeJunctions ?? "true");
                return lines;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 筛选/分割/合并 ====================

        /// <summary>
        /// 按单特征筛选轮廓（select_contours_xld）：
        /// 常用特征 "contour_length"（长度，min1/max1 有效，min2/max2 传 0/0）、
        /// "direction"（角度范围）、"curvature"（曲率，两组范围）、"maximum_extent"。
        /// </summary>
        public static HObject SelectContours(HObject contours, string feature,
            double min1, double max1, double min2 = 0, double max2 = 0)
        {
            try
            {
                HObject sel;
                HOperatorSet.SelectContoursXld(contours, out sel, feature ?? "contour_length",
                    min1, max1, min2, max2);
                return sel;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 按通用形状特征筛选 XLD（select_shape_xld），如 "area"/"circularity"/"compactness"。
        /// </summary>
        public static HObject SelectShape(HObject contours, string feature,
            string operation = "and", double min = 0, double max = 1e9)
        {
            try
            {
                HObject sel;
                HOperatorSet.SelectShapeXld(contours, out sel, feature ?? "length", operation ?? "and", min, max);
                return sel;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 把轮廓在折点处分段（segment_contours_xld）：直线段/圆弧/椭圆弧。
        /// </summary>
        /// <param name="mode">"lines"（全直线段）/"lines_circles"（直线+圆）/"lines_ellipses"。</param>
        /// <param name="smoothCont">分割前平滑点数（&gt;=1，奇数，常用 5）。</param>
        /// <param name="maxLineDist1">折线首段近似阈值（像素）。</param>
        /// <param name="maxLineDist2">细分阈值（像素）。</param>
        public static HObject SegmentContours(HObject contours, string mode = "lines_circles",
            int smoothCont = 5, double maxLineDist1 = 5.0, double maxLineDist2 = 2.0)
        {
            try
            {
                HObject split;
                HOperatorSet.SegmentContoursXld(contours, out split, mode ?? "lines_circles",
                    smoothCont, maxLineDist1, maxLineDist2);
                return split;
            }
            catch (HalconException) { return null; }
        }

        /// <summary>
        /// 合并近似共线的相邻轮廓段（union_collinear_contours_xld）：修复边缘断裂。
        /// </summary>
        /// <param name="maxDistAbs">端点绝对距离上限（像素）。</param>
        /// <param name="maxDistRel">相对长度比例上限。</param>
        /// <param name="maxShift">侧向错位上限（像素）。</param>
        /// <param name="maxAngle">方向夹角上限（弧度）。</param>
        /// <param name="mode">"attr_keep"（保留属性，默认）/"default"。</param>
        public static HObject UnionCollinear(HObject contours,
            double maxDistAbs = 10, double maxDistRel = 0.5, double maxShift = 5,
            double maxAngle = 0.1, string mode = "attr_keep")
        {
            try
            {
                HObject u;
                HOperatorSet.UnionCollinearContoursXld(contours, out u,
                    maxDistAbs, maxDistRel, maxShift, maxAngle, mode ?? "attr_keep");
                return u;
            }
            catch (HalconException) { return null; }
        }

        #endregion

        #region ==================== 几何拟合 ====================

        /// <summary>
        /// 直线拟合（fit_line_contour_xld）：对轮廓集每条轮廓给出一组直线参数。
        /// </summary>
        /// <param name="contours">XLD 轮廓集（通常先分段/筛选）。</param>
        /// <param name="algorithm">"tukey"（鲁棒，默认）/"huber"/"drop"/"gauss"。</param>
        /// <param name="maxNumPoints">最多参与拟合点数（-1=全部）。</param>
        /// <param name="clippingEndPoints">裁掉的端点个数。</param>
        /// <param name="iterations">鲁棒迭代次数（gauss 忽略）。</param>
        /// <param name="clippingFactor">鲁棒剪裁因子（Tukey 常用 2.0~3.0）。</param>
        public static HXldLine[] FitLines(HObject contours, string algorithm = "tukey",
            int maxNumPoints = -1, int clippingEndPoints = 0, int iterations = 5, double clippingFactor = 2.0)
        {
            try
            {
                HTuple rb, cb, re, ce, nr, nc, dist;
                HOperatorSet.FitLineContourXld(contours, algorithm ?? "tukey", maxNumPoints,
                    clippingEndPoints, iterations, clippingFactor,
                    out rb, out cb, out re, out ce, out nr, out nc, out dist);
                double[] a = rb.DArr, b = cb.DArr, c = re.DArr, d = ce.DArr,
                    e = nr.DArr, f = nc.DArr, g = dist.DArr;
                int n = a.Length;
                HXldLine[] res = new HXldLine[n];
                for (int i = 0; i < n; i++)
                    res[i] = new HXldLine
                    {
                        RowBegin = a[i], ColBegin = b[i], RowEnd = c[i], ColEnd = d[i],
                        Nr = e[i], Nc = f[i], Dist = g[i]
                    };
                return res;
            }
            catch (HalconException) { return new HXldLine[0]; }
        }

        /// <summary>圆拟合（fit_circle_contour_xld），对每条轮廓返回一个圆结果。</summary>
        /// <param name="algorithm">"ahuber"/"atukey"（鲁棒）/"algebraic"/"ahuber"（默认 atukey）/"geohuber"/"geotukey"。</param>
        /// <param name="maxClosureDist">首尾封闭判定距离（默认 0，&gt;0 时按闭圆拟合）。</param>
        public static HXldCircle[] FitCircles(HObject contours, string algorithm = "atukey",
            int maxNumPoints = -1, double maxClosureDist = 0, int clippingEndPoints = 0,
            int iterations = 3, double clippingFactor = 2.0)
        {
            try
            {
                HTuple r, c, rad, sp, ep, po;
                HOperatorSet.FitCircleContourXld(contours, algorithm ?? "atukey", maxNumPoints,
                    maxClosureDist, clippingEndPoints, iterations, clippingFactor,
                    out r, out c, out rad, out sp, out ep, out po);
                double[] ra = r.DArr, ca = c.DArr, rd = rad.DArr,
                    s = sp.DArr, e2 = ep.DArr, p = po.DArr;
                int n = ra.Length;
                HXldCircle[] res = new HXldCircle[n];
                for (int i = 0; i < n; i++)
                    res[i] = new HXldCircle
                    {
                        Row = ra[i], Column = ca[i], Radius = rd[i],
                        StartPhi = s[i], EndPhi = e2[i], PointOrder = p[i]
                    };
                return res;
            }
            catch (HalconException) { return new HXldCircle[0]; }
        }

        /// <summary>椭圆拟合（fit_ellipse_contour_xld）。</summary>
        /// <param name="vossTabSize">Voss 查表点数（仅 'voss' 类算法用，默认 150）。</param>
        public static HXldEllipse[] FitEllipses(HObject contours, string algorithm = "fitzgibbon",
            int maxNumPoints = -1, double maxClosureDist = 0, int clippingEndPoints = 0,
            int vossTabSize = 150, int iterations = 3, double clippingFactor = 2.0)
        {
            try
            {
                HTuple r, c, phi, r1, r2, sp, ep, po;
                HOperatorSet.FitEllipseContourXld(contours, algorithm ?? "fitzgibbon", maxNumPoints,
                    maxClosureDist, clippingEndPoints, vossTabSize, iterations, clippingFactor,
                    out r, out c, out phi, out r1, out r2, out sp, out ep, out po);
                double[] ra = r.DArr, ca = c.DArr, pa = phi.DArr, u = r1.DArr, v = r2.DArr,
                    s = sp.DArr, e2 = ep.DArr, p = po.DArr;
                int n = ra.Length;
                HXldEllipse[] res = new HXldEllipse[n];
                for (int i = 0; i < n; i++)
                    res[i] = new HXldEllipse
                    {
                        Row = ra[i], Column = ca[i], Phi = pa[i], Radius1 = u[i], Radius2 = v[i],
                        StartPhi = s[i], EndPhi = e2[i], PointOrder = p[i]
                    };
                return res;
            }
            catch (HalconException) { return new HXldEllipse[0]; }
        }

        /// <summary>旋转矩形拟合（fit_rectangle2_contour_xld）：对矩形外轮廓给中心/角度/半长宽。</summary>
        public static HXldRectangle2[] FitRectangle2s(HObject contours, string algorithm = "tukey",
            int maxNumPoints = -1, double maxClosureDist = 0, int clippingEndPoints = 0,
            int iterations = 3, double clippingFactor = 2.0)
        {
            try
            {
                HTuple r, c, phi, l1, l2, po;
                HOperatorSet.FitRectangle2ContourXld(contours, algorithm ?? "tukey", maxNumPoints,
                    maxClosureDist, clippingEndPoints, iterations, clippingFactor,
                    out r, out c, out phi, out l1, out l2, out po);
                double[] ra = r.DArr, ca = c.DArr, pa = phi.DArr, u = l1.DArr, v = l2.DArr, p = po.DArr;
                int n = ra.Length;
                HXldRectangle2[] res = new HXldRectangle2[n];
                for (int i = 0; i < n; i++)
                    res[i] = new HXldRectangle2
                    {
                        Row = ra[i], Column = ca[i], Phi = pa[i],
                        Length1 = u[i], Length2 = v[i], PointOrder = p[i]
                    };
                return res;
            }
            catch (HalconException) { return new HXldRectangle2[0]; }
        }

        #endregion

        #region ==================== 度量与构造 ====================

        /// <summary>计算轮廓总长度（length_xld，单位像素）。</summary>
        public static double Length(HObject contour)
        {
            try
            {
                HTuple l;
                HOperatorSet.LengthXld(contour, out l);
                return l.D;
            }
            catch (HalconException) { return 0; }
        }

        /// <summary>计算闭合轮廓面积与中心（area_center_xld）；非闭合轮廓 area 为 0。</summary>
        public static bool AreaCenter(HObject contour,
            out double area, out double row, out double column, out string pointOrder)
        {
            area = 0;
            row = 0;
            column = 0;
            pointOrder = string.Empty;
            try
            {
                HTuple a, r, c, po;
                HOperatorSet.AreaCenterXld(contour, out a, out r, out c, out po);
                area = a.D;
                row = r.D;
                column = c.D;
                pointOrder = po.S ?? string.Empty;
                return true;
            }
            catch (HalconException) { return false; }
        }

        /// <summary>读取轮廓全部亚像素点坐标（get_contour_xld），失败返回 false。</summary>
        public static bool GetPoints(HObject contour, out double[] rows, out double[] cols)
        {
            rows = cols = new double[0];
            try
            {
                HTuple r, c;
                HOperatorSet.GetContourXld(contour, out r, out c);
                rows = r.DArr;
                cols = c.DArr;
                return true;
            }
            catch (HalconException) { return false; }
        }

        /// <summary>按亚像素点列生成多边形 XLD 轮廓（gen_contour_polygon_xld）。</summary>
        public static HObject FromPolygon(double[] rows, double[] cols)
        {
            if (rows == null || cols == null || rows.Length != cols.Length || rows.Length < 2) return null;
            try
            {
                HObject contour;
                HOperatorSet.GenContourPolygonXld(out contour, new HTuple(rows), new HTuple(cols));
                return contour;
            }
            catch (HalconException) { return null; }
        }

        #endregion
    }
}
