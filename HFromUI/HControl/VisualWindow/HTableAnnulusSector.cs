using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableAnnulusSector : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.AnnulusSector;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("圆环扇形");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 5;
        /// <summary>Center 成员。</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;
        /// <summary>ROuter 成员。</summary>
        public double ROuter => Points.Count >= 2 ? HTableGeom.Distance(Points[0], Points[1]) : 0;
        /// <summary>RInner 成员。</summary>
        public double RInner => Points.Count >= 3 ? HTableGeom.Distance(Points[0], Points[2]) : 0;
        /// <summary>StartAngleDeg 成员。</summary>
        public double StartAngleDeg { get; set; }
        /// <summary>SweepAngleDeg 成员。</summary>
        public double SweepAngleDeg { get; set; } = 45;
        /// <summary>Area 成员。</summary>
        public double Area => HTableGeom.AnnulusSectorArea(ROuter, RInner, Math.Abs(SweepAngleDeg));

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 2 || Center == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF c = imageToScreen(pts[0]);
                // 屏幕半径用变换后的点计算，任意缩放下面与锚点重合
                PointF e1 = imageToScreen(pts[1]);
                float rO = (float)Math.Sqrt((e1.X - c.X) * (e1.X - c.X) + (e1.Y - c.Y) * (e1.Y - c.Y));
                float rI = 0f;
                if (pts.Count >= 3)
                {
                    PointF e2 = imageToScreen(pts[2]);
                    rI = (float)Math.Sqrt((e2.X - c.X) * (e2.X - c.X) + (e2.Y - c.Y) * (e2.Y - c.Y));
                }
                if (rO <= 0) return;
                // 角度：已完成用缓存；绘制中 P3=起始角方向，末尾预览点=结束角方向，橡皮筋跟手
                double startDeg = StartAngleDeg, sweepDeg = SweepAngleDeg;
                if (pts.Count >= 4)
                {
                    double a3 = Math.Atan2((double)(pts[3].Y.Value - pts[0].Y.Value), (double)(pts[3].X.Value - pts[0].X.Value)) * 180.0 / Math.PI;
                    HPoint endP = pts[pts.Count - 1];
                    double a4 = Math.Atan2((double)(endP.Y.Value - pts[0].Y.Value), (double)(endP.X.Value - pts[0].X.Value)) * 180.0 / Math.PI;
                    startDeg = HTableGeom.NormalizeAngle360(a3);
                    sweepDeg = HTableGeom.NormalizeAngle360(a4 - a3);
                }
                RectangleF outer = new RectangleF(c.X - rO, c.Y - rO, rO * 2, rO * 2);
                g.DrawPie(pen, outer, (float)startDeg, (float)sweepDeg);
                if (rI > 0 && rI < rO)
                {
                    RectangleF inner = new RectangleF(c.X - rI, c.Y - rI, rI * 2, rI * 2);
                    g.DrawPie(pen, inner, (float)startDeg, (float)sweepDeg);
                }
                DrawCross(g, c, 5, pen);
            }
        }

        /// <summary>几何变化后重算角度：P3 方向=起始角，P4 方向=结束角</summary>
        protected override void OnGeometryChanged()
        {
            base.OnGeometryChanged();
            if (Center == null || Points.Count < 5) return;
            double a3 = Math.Atan2((double)(Points[3].Y.Value - Center.Y.Value), (double)(Points[3].X.Value - Center.X.Value)) * 180.0 / Math.PI;
            double a4 = Math.Atan2((double)(Points[4].Y.Value - Center.Y.Value), (double)(Points[4].X.Value - Center.X.Value)) * 180.0 / Math.PI;
            StartAngleDeg = HTableGeom.NormalizeAngle360(a3);
            SweepAngleDeg = HTableGeom.NormalizeAngle360(a4 - a3);
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            double d = HTableGeom.Distance(p, Center);
            if (d > ROuter + tol) return false;
            if (RInner > 0 && d < RInner - tol) return false;
            return HTableGeom.PointInSector(p, Center, ROuter, StartAngleDeg, SweepAngleDeg);
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Center == null || ROuter <= 0) return RectangleF.Empty;
            float r = (float)ROuter;
            return new RectangleF((float)Center.X.Value - r, (float)Center.Y.Value - r, r * 2, r * 2);
        }
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Center != null && ROuter > RInner + 1.0 && RInner >= 0;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableAnnulusSector(); CopyBaseTo(c); c.StartAngleDeg = StartAngleDeg; c.SweepAngleDeg = SweepAngleDeg; return c; }
    }
}
