using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// 点/线几何运算静态工具（HALCON 几何算子的标量便捷封装）：
    /// 点点距离 distance_pp、点线距离 distance_pl、点到线段距离 distance_ps、
    /// 点到直线垂足 projection_pl、两直线交点 intersection_lines、两直线夹角 angle_ll。
    /// 所有角度单位均为弧度，距离单位与输入坐标一致（通常像素）。
    /// </summary>
    public static class HHalconGeometry
    {
        #region ==================== 距离 ====================

        /// <summary>两点距离（distance_pp）。</summary>
        public static double DistancePoints(double row1, double col1, double row2, double col2)
        {
            try
            {
                HTuple d;
                HOperatorSet.DistancePp(row1, col1, row2, col2, out d);
                return d.D;
            }
            catch (HalconException)
            {
                return 0;
            }
        }

        /// <summary>
        /// 点到<b>无限长直线</b>的垂直距离（distance_pl），直线由两点 (row1,col1)-(row2,col2) 确定。
        /// </summary>
        public static double DistancePointLine(double row, double col,
            double row1, double col1, double row2, double col2)
        {
            try
            {
                HTuple d;
                HOperatorSet.DistancePl(row, col, row1, col1, row2, col2, out d);
                return d.D;
            }
            catch (HalconException)
            {
                return 0;
            }
        }

        /// <summary>
        /// 点到<b>线段</b>的距离（distance_ps）：垂足在线段内时为垂距，否则为到较近端点距离。
        /// </summary>
        /// <param name="distanceMin">到线段的最近距离。</param>
        /// <param name="distanceMax">到线段两端点的较远距离。</param>
        public static bool DistancePointSegment(double row, double col,
            double row1, double col1, double row2, double col2,
            out double distanceMin, out double distanceMax)
        {
            distanceMin = distanceMax = 0;
            try
            {
                HTuple dmin, dmax;
                HOperatorSet.DistancePs(row, col, row1, col1, row2, col2, out dmin, out dmax);
                distanceMin = dmin.D;
                distanceMax = dmax.D;
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        #endregion

        #region ==================== 投影/交点/夹角 ====================

        /// <summary>
        /// 点到无限长直线的垂足（projection_pl）。
        /// </summary>
        public static bool ProjectToLine(double row, double col,
            double row1, double col1, double row2, double col2,
            out double rowProj, out double colProj)
        {
            rowProj = colProj = 0;
            try
            {
                HTuple rp, cp;
                HOperatorSet.ProjectionPl(row, col, row1, col1, row2, col2, out rp, out cp);
                rowProj = rp.D;
                colProj = cp.D;
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>
        /// 两条无限长直线的交点（intersection_lines）：两条线各由两个点确定。
        /// </summary>
        /// <param name="isOverlapping">输出 true=两线重合（交点不唯一）。</param>
        public static bool IntersectLines(
            double rowA1, double colA1, double rowA2, double colA2,
            double rowB1, double colB1, double rowB2, double colB2,
            out double row, out double col, out bool isOverlapping)
        {
            row = col = 0;
            isOverlapping = false;
            try
            {
                HTuple r, c, over;
                HOperatorSet.IntersectionLines(
                    rowA1, colA1, rowA2, colA2,
                    rowB1, colB1, rowB2, colB2,
                    out r, out c, out over);
                row = r.D;
                col = c.D;
                isOverlapping = over.I != 0;
                return true;
            }
            catch (HalconException)
            {
                return false;
            }
        }

        /// <summary>
        /// 两条直线的夹角（angle_ll，弧度，0~π/2 取锐角可自行 Abs/取补）：
        /// 返回两线方向的有向夹角，范围 (-π/2, π/2]；平行返回 0。
        /// </summary>
        public static double AngleBetweenLines(
            double rowA1, double colA1, double rowA2, double colA2,
            double rowB1, double colB1, double rowB2, double colB2)
        {
            try
            {
                HTuple a;
                HOperatorSet.AngleLl(
                    rowA1, colA1, rowA2, colA2,
                    rowB1, colB1, rowB2, colB2, out a);
                return a.D;
            }
            catch (HalconException)
            {
                return 0;
            }
        }

        #endregion

        #region ==================== 纯数学辅助 ====================

        /// <summary>两点中点。</summary>
        public static void MidPoint(double row1, double col1, double row2, double col2,
            out double row, out double col)
        {
            row = (row1 + row2) * 0.5;
            col = (col1 + col2) * 0.5;
        }

        /// <summary>由两点确定的直线方向角（atan2，弧度）。</summary>
        public static double LineAngle(double row1, double col1, double row2, double col2)
        {
            return Math.Atan2(row2 - row1, col2 - col1);
        }

        /// <summary>角度归一化到 (-π, π]。</summary>
        public static double NormalizeAngle(double angle)
        {
            double a = angle % (Math.PI * 2);
            if (a <= -Math.PI) a += Math.PI * 2;
            if (a > Math.PI) a -= Math.PI * 2;
            return a;
        }

        #endregion
    }
}
