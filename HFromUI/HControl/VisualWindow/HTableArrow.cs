using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableArrow : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Arrow;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("箭头");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 2;
        /// <summary>ArrowHeadSizeScreen 成员。</summary>
        public float ArrowHeadSizeScreen { get; set; } = 10f;
        /// <summary>LabelText 成员。</summary>
        public string LabelText { get; set; }

        /// <summary>Start 成员。</summary>
        public HPoint Start => Points.Count > 0 ? Points[0] : null;
        /// <summary>End 成员。</summary>
        public HPoint End => Points.Count >= 2 ? Points[1] : null;
        /// <summary>Length 成员。</summary>
        public double Length => (Start != null && End != null) ? HTableGeom.Distance(Start, End) : 0;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 2) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF s = imageToScreen(pts[0]);
                PointF e = imageToScreen(pts[pts.Count - 1]);
                g.DrawLine(pen, s, e);
                float head = ArrowHeadSizeScreen;
                float ang = (float)Math.Atan2(e.Y - s.Y, e.X - s.X);
                PointF tip = e;
                PointF left = new PointF(e.X - head * (float)Math.Cos(ang - Math.PI / 6),
                    e.Y - head * (float)Math.Sin(ang - Math.PI / 6));
                PointF right = new PointF(e.X - head * (float)Math.Cos(ang + Math.PI / 6),
                    e.Y - head * (float)Math.Sin(ang + Math.PI / 6));
                PointF[] tri = { tip, left, right };
                using (Brush b = new SolidBrush(pen.Color))
                    g.FillPolygon(b, tri);
                DrawHandle(g, s, !IsBuilding, pen.Color);
                DrawHandle(g, e, !IsBuilding, pen.Color);
            }
            if (!string.IsNullOrEmpty(LabelText) && pts.Count >= 2)
            {
                PointF mid = imageToScreen(new HPoint((pts[0].X.Value + pts[pts.Count - 1].X.Value) / 2,
                    (pts[0].Y.Value + pts[pts.Count - 1].Y.Value) / 2));
                System.Drawing.Font font = SystemFonts.DefaultFont;
                SizeF sz = g.MeasureString(LabelText, font);
                using (Brush bg = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    g.FillRectangle(bg, mid.X + 4, mid.Y - sz.Height - 4, sz.Width + 4, sz.Height + 4);
                using (Brush b = new SolidBrush(Color))
                    g.DrawString(LabelText, font, b, mid.X + 6, mid.Y - sz.Height - 2);
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
            HTableGeom.PolygonBoundingRect(new[] { Start ?? new HPoint(0, 0), End ?? new HPoint(0, 0) });
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Start != null && End != null && Length > 1;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableArrow(); CopyBaseTo(c); c.ArrowHeadSizeScreen = ArrowHeadSizeScreen; c.LabelText = LabelText; return c; }
    }
}
