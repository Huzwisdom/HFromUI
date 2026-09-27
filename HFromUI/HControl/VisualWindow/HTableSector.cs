using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableSector : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Sector;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("扇形");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 4;
        /// <summary>Center 成员。</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;
        /// <summary>Radius 成员。</summary>
        public double Radius => Points.Count >= 2 ? HTableGeom.Distance(Points[0], Points[1]) : 0;
        /// <summary>StartAngleDeg 成员。</summary>
        public double StartAngleDeg { get; set; }
        /// <summary>SweepAngleDeg 成员。</summary>
        public double SweepAngleDeg { get; set; } = 90;
        /// <summary>Area 成员。</summary>
        public double Area => HTableGeom.SectorArea(Radius, Math.Abs(SweepAngleDeg));

        /// <summary>判断包含。</summary>
        public bool Contains(HPoint p) => HTableGeom.PointInSector(p, Center, Radius, StartAngleDeg, SweepAngleDeg);

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 2 || Center == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF c = imageToScreen(pts[0]);
                PointF e1 = imageToScreen(pts[1]);
                // 屏幕半径：用变换后的点计算，保证任意缩放下面与锚点重合
                float rS = (float)Math.Sqrt((e1.X - c.X) * (e1.X - c.X) + (e1.Y - c.Y) * (e1.Y - c.Y));
                if (rS <= 0) return;
                // 角度：已完成图形用缓存值；绘制中用最新锚点/预览点实时算，橡皮筋跟手
                double startDeg = StartAngleDeg, sweepDeg = SweepAngleDeg;
                if (pts.Count >= 3)
                {
                    double a1 = Math.Atan2((double)(pts[1].Y.Value - pts[0].Y.Value), (double)(pts[1].X.Value - pts[0].X.Value)) * 180.0 / Math.PI;
                    HPoint endP = pts[pts.Count - 1];
                    double a2 = Math.Atan2((double)(endP.Y.Value - pts[0].Y.Value), (double)(endP.X.Value - pts[0].X.Value)) * 180.0 / Math.PI;
                    startDeg = HTableGeom.NormalizeAngle360(a1);
                    sweepDeg = HTableGeom.NormalizeAngle360(a2 - a1);
                }
                RectangleF rect = new RectangleF(c.X - rS, c.Y - rS, rS * 2, rS * 2);
                g.DrawPie(pen, rect, (float)startDeg, (float)sweepDeg);
                DrawCross(g, c, 5, pen);
            }
        }

        /// <summary>几何变化（加锚点/拖锚点/移动）后重算起始角与扫掠角：P1 方向=起始角，P2 方向=结束角</summary>
        protected override void OnGeometryChanged()
        {
            base.OnGeometryChanged();
            if (Center == null || Points.Count < 3) return;
            // GDI DrawPie 角度约定：0°=+X 轴、正角顺时针，与图像坐标（Y 向下）下 Atan2 结果一致
            double a1 = Math.Atan2((double)(Points[1].Y.Value - Center.Y.Value), (double)(Points[1].X.Value - Center.X.Value)) * 180.0 / Math.PI;
            double a2 = Math.Atan2((double)(Points[2].Y.Value - Center.Y.Value), (double)(Points[2].X.Value - Center.X.Value)) * 180.0 / Math.PI;
            StartAngleDeg = HTableGeom.NormalizeAngle360(a1);
            SweepAngleDeg = HTableGeom.NormalizeAngle360(a2 - a1);
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            double d = HTableGeom.Distance(p, Center);
            if (d > Radius + tol) return false;
            return Contains(p);
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Center == null || Radius <= 0) return RectangleF.Empty;
            float r = (float)Radius;
            return new RectangleF((float)Center.X.Value - r, (float)Center.Y.Value - r, r * 2, r * 2);
        }
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Center != null && Radius > 1;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableSector(); CopyBaseTo(c); c.StartAngleDeg = StartAngleDeg; c.SweepAngleDeg = SweepAngleDeg; return c; }
    }
}
