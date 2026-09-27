using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableAngle : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Angle;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("角度测量");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 3;
        /// <summary>Vertex 成员。</summary>
        public HPoint Vertex => Points.Count > 0 ? Points[0] : null;
        /// <summary>P1 成员。</summary>
        public HPoint P1 => Points.Count >= 2 ? Points[1] : null;
        /// <summary>P2 成员。</summary>
        public HPoint P2 => Points.Count >= 3 ? Points[2] : null;

        public double InnerAngleDeg
        {
            get
            {
                if (Vertex == null || P1 == null || P2 == null) return 0;
                double a1 = Math.Atan2((double)(P1.Y.Value - Vertex.Y.Value), (double)(P1.X.Value - Vertex.X.Value));
                double a2 = Math.Atan2((double)(P2.Y.Value - Vertex.Y.Value), (double)(P2.X.Value - Vertex.X.Value));
                double d = Math.Abs(a2 - a1) * 180.0 / Math.PI;
                if (d > 180) d = 360 - d;
                return d;
            }
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (Vertex == null || P1 == null || P2 == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF v = imageToScreen(Vertex);
                PointF p1 = imageToScreen(P1);
                PointF p2 = imageToScreen(P2);
                g.DrawLine(pen, v, p1);
                g.DrawLine(pen, v, p2);
                float arcR = (float)(Math.Min(HTableGeom.Distance(Vertex, P1), HTableGeom.Distance(Vertex, P2)) * 0.3);
                if (arcR > 1)
                {
                    double ang1 = Math.Atan2((double)(P1.Y.Value - Vertex.Y.Value), (double)(P1.X.Value - Vertex.X.Value)) * 180.0 / Math.PI;
                    double ang2 = Math.Atan2((double)(P2.Y.Value - Vertex.Y.Value), (double)(P2.X.Value - Vertex.X.Value)) * 180.0 / Math.PI;
                    double sweep = ang2 - ang1;
                    if (sweep > 180) sweep -= 360;
                    if (sweep < -180) sweep += 360;
                    RectangleF arcRect = new RectangleF(v.X - arcR, v.Y - arcR, arcR * 2, arcR * 2);
                    g.DrawArc(pen, arcRect, (float)ang1, (float)sweep);
                }
                DrawHandle(g, v, true, pen.Color);
                DrawHandle(g, p1, !IsBuilding, pen.Color);
                DrawHandle(g, p2, !IsBuilding, pen.Color);
            }
            if (!IsBuilding && Vertex != null)
            {
                System.Drawing.Font font = SystemFonts.DefaultFont;
                PointF v = imageToScreen(Vertex);
                string label = string.Format("{0:F1}°", InnerAngleDeg);
                SizeF sz = g.MeasureString(label, font);
                using (Brush bg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    g.FillRectangle(bg, v.X + 6, v.Y - sz.Height - 4, sz.Width + 4, sz.Height + 4);
                using (Brush b = new SolidBrush(Color))
                    g.DrawString(label, font, b, v.X + 8, v.Y - sz.Height - 2);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (Vertex == null || P1 == null || P2 == null) return false;
            double d1 = HTableGeom.DistancePointToSegment(p, Vertex, P1);
            double d2 = HTableGeom.DistancePointToSegment(p, Vertex, P2);
            return Math.Min(d1, d2) <= tol;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect() =>
            HTableGeom.PolygonBoundingRect(new[] { Vertex ?? new HPoint(0, 0), P1 ?? new HPoint(0, 0), P2 ?? new HPoint(0, 0) });
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Vertex != null && P1 != null && P2 != null;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableAngle(); CopyBaseTo(c); return c; }
    }
}
