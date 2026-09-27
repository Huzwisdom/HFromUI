using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableCaliper : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Caliper;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("卡尺");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 3;
        /// <summary>Start 成员。</summary>
        public HPoint Start => Points.Count > 0 ? Points[0] : null;
        /// <summary>End 成员。</summary>
        public HPoint End => Points.Count >= 2 ? Points[1] : null;
        /// <summary>DirPoint 成员。</summary>
        public HPoint DirPoint => Points.Count >= 3 ? Points[2] : null;

        public double MeasurementDistance
        {
            get
            {
                if (Start == null || DirPoint == null) return 0;
                HPoint perp = PerpDirection;
                return Math.Abs((double)(DirPoint.X.Value - Start.X.Value) * (double)perp.X.Value + (double)(DirPoint.Y.Value - Start.Y.Value) * (double)perp.Y.Value);
            }
        }
        public double AngleDeg
        {
            get { if (Start == null || End == null) return 0; return HTableGeom.NormalizeAngle180(Math.Atan2((double)(End.Y.Value - Start.Y.Value), (double)(End.X.Value - Start.X.Value)) * 180.0 / Math.PI); }
        }

        public HPoint PerpDirection
        {
            get
            {
                if (Start == null || End == null) return new HPoint(0, -1);
                double dx = (double)(End.X.Value - Start.X.Value), dy = (double)(End.Y.Value - Start.Y.Value);
                double len = Math.Sqrt(dx * dx + dy * dy);
                if (len < 1e-9) return new HPoint(0, -1);
                return new HPoint(-dy / len, dx / len);
            }
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (Start == null || End == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF s = imageToScreen(Start);
                PointF e = imageToScreen(End);
                g.DrawLine(pen, s, e);
                DrawHandle(g, s, !IsBuilding, pen.Color);
                DrawHandle(g, e, !IsBuilding, pen.Color);
                if (DirPoint != null)
                {
                    HPoint perp = PerpDirection;
                    double sign = HTableGeom.Dot(new HPoint((double)(DirPoint.X.Value - Start.X.Value), (double)(DirPoint.Y.Value - Start.Y.Value)), perp);
                    if (sign < 0) { perp = new HPoint(-perp.X.Value, -perp.Y.Value); }
                    HPoint proj = new HPoint(Start.X.Value + perp.X.Value * MeasurementDistance, Start.Y.Value + perp.Y.Value * MeasurementDistance);
                    PointF pf = imageToScreen(proj);
                    PointF dp = imageToScreen(DirPoint);
                    using (Pen aux = new Pen(Color.FromArgb(120, pen.Color), 1f) { DashStyle = DashStyle.Dot })
                        g.DrawLine(aux, pf, dp);
                    DrawHandle(g, dp, !IsBuilding, pen.Color);
                    System.Drawing.Font font = SystemFonts.DefaultFont;
                    string label = string.Format("{0:F2}px @ {1:F1}°", MeasurementDistance, AngleDeg);
                    SizeF sz = g.MeasureString(label, font);
                    using (Brush bg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                        g.FillRectangle(bg, dp.X + 6, dp.Y - sz.Height - 4, sz.Width + 4, sz.Height + 4);
                    using (Brush b = new SolidBrush(Color))
                        g.DrawString(label, font, b, dp.X + 8, dp.Y - sz.Height - 2);
                }
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (Start == null || End == null) return false;
            return HTableGeom.DistancePointToSegment(p, Start, End) <= tol;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect() =>
            HTableGeom.PolygonBoundingRect(new[] { Start ?? new HPoint(0, 0), End ?? new HPoint(0, 0), DirPoint ?? new HPoint(0, 0) });
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Start != null && End != null;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableCaliper(); CopyBaseTo(c); return c; }
    }
}
