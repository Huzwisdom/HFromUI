using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 三点圆弧 ROI：依次单击 起点、弧上一点、终点（参考 HCoordinate/HDraw3PArc）。
    /// 图像像素坐标。适用于圆弧边缘找特征、圆边局部检测等。
    /// </summary>
    [Serializable]
    public class HTableArc3P : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Arc3P;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("三点圆弧");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 3;

        /// <summary>起点 P0</summary>
        public HPoint StartPoint => Points.Count > 0 ? Points[0] : null;

        /// <summary>弧上点 P1</summary>
        public HPoint MidPoint => Points.Count > 1 ? Points[1] : null;

        /// <summary>终点 P2</summary>
        public HPoint EndPoint => Points.Count > 2 ? Points[2] : null;

        /// <summary>圆心（图像像素坐标），三点共线时为 null</summary>
        public HPoint Center { get; private set; }

        /// <summary>半径（像素）</summary>
        public double Radius { get; private set; }

        /// <summary>响应 GeometryChanged 事件。</summary>
        protected override void OnGeometryChanged()
        {
            Center = null;
            Radius = 0;
        }

        /// <summary>
        /// 计算圆弧参数与离散点（图像像素坐标）。
        /// 三点共线或点数不足返回 null。
        /// </summary>
        /// <param name="endPointOverride">终点覆盖（绘制中传入预览点）</param>
        public List<HPoint> GetArcPoints(HPoint midPointOverride = null, HPoint endPointOverride = null, int segments = 64)
        {
            if (StartPoint == null || MidPoint == null) return null;
            HPoint p1 = midPointOverride ?? MidPoint;
            HPoint p2 = endPointOverride ?? EndPoint;
            if (p1 == null || p2 == null) return null;

            if (!HTableGeom.CircleFrom3Points(StartPoint, p1, p2, out HPoint center, out double radius))
                return null;

            Center = center;
            Radius = radius;

            double a0 = Math.Atan2((double)(StartPoint.Y.Value - center.Y.Value), (double)(StartPoint.X.Value - center.X.Value));
            double am = Math.Atan2((double)(p1.Y.Value - center.Y.Value), (double)(p1.X.Value - center.X.Value));
            double a2 = Math.Atan2((double)(p2.Y.Value - center.Y.Value), (double)(p2.X.Value - center.X.Value));

            // 确定扫掠方向：让弧经过中间点
            double sweep = HTableGeom.NormalizeAngle360((a2 - a0) * 180.0 / Math.PI) * Math.PI / 180.0;
            double midDelta = HTableGeom.NormalizeAngle360((am - a0) * 180.0 / Math.PI) * Math.PI / 180.0;
            if (midDelta > sweep) sweep -= 2.0 * Math.PI;

            List<HPoint> pts = new List<HPoint>(segments + 1);
            for (int i = 0; i <= segments; i++)
            {
                double t = (double)i / segments;
                double ang = a0 + sweep * t;
                pts.Add(new HPoint(center.X.Value + radius * Math.Cos(ang), center.Y.Value + radius * Math.Sin(ang)));
            }
            return pts;
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> drawPts = new List<HPoint>(EnumerateDrawPoints());
            if (drawPts.Count < 2)
            {
                if (drawPts.Count == 1) DrawHandle(g, imageToScreen(drawPts[0]), false, HTableShapeList.BuildingColor);
                return;
            }
            using (Pen pen = CreatePen(selected, scale))
            {
                // 绘制中：第 2 点为弧上点预览，第 3 点为终点预览
                HPoint midOverride = null, endOverride = null;
                if (Points.Count < 3 && drawPts.Count >= 3) endOverride = drawPts[drawPts.Count - 1];
                if (Points.Count < 2 && drawPts.Count >= 2) midOverride = drawPts[1];

                List<HPoint> arc = GetArcPoints(midOverride, endOverride);
                if (arc != null && arc.Count >= 2)
                {
                    PointF[] sf = new PointF[arc.Count];
                    for (int i = 0; i < arc.Count; i++) sf[i] = imageToScreen(arc[i]);
                    g.DrawLines(pen, sf);

                    // 圆心虚线十字
                    if (Center != null)
                    {
                        PointF cc = imageToScreen(Center);
                        using (Pen dash = new Pen(pen.Color, 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                            DrawCross(g, cc, 5, dash);
                    }
                }
                int anchorCount = Math.Min(Points.Count, 3);
                for (int i = 0; i < anchorCount; i++)
                    DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            List<HPoint> arc = GetArcPoints();
            if (arc == null) return false;
            double min = double.MaxValue;
            for (int i = 0; i < arc.Count - 1; i++)
                min = Math.Min(min, HTableGeom.DistancePointToSegment(imagePoint, arc[i], arc[i + 1]));
            return min <= toleranceImagePx;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            if (StartPoint == null || MidPoint == null || EndPoint == null) return false;
            return HTableGeom.CircleFrom3Points(StartPoint, MidPoint, EndPoint, out _, out double r) && r > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTableArc3P c = new HTableArc3P();
            CopyBaseTo(c);
            return c;
        }
    }
}
