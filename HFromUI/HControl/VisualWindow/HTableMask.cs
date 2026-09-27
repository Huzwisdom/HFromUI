using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableMask : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Mask;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("画笔遮罩");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => int.MaxValue;
        public const int MinPoints = 3;
        /// <summary>BrushThickness 成员。</summary>
        public double BrushThickness { get; set; } = 0;
        /// <summary>Fill 成员。</summary>
        public bool Fill { get; set; } = true;

        /// <summary>Area 成员。</summary>
        public double Area => HTableGeom.PolygonArea(Points);
        /// <summary>判断包含。</summary>
        public bool Contains(HPoint p) => HTableGeom.PointInPolygon(p, Points);

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 3)
            {
                if (pts.Count >= 2)
                {
                    using (Pen pen = CreatePen(selected, scale))
                        g.DrawLines(pen, pts.ConvertAll(new Converter<HPoint, PointF>(imageToScreen)).ToArray());
                }
                return;
            }
            Converter<HPoint, PointF> conv = new Converter<HPoint, PointF>(imageToScreen);
            PointF[] sf = pts.ConvertAll(conv).ToArray();
            if (Fill)
            {
                using (Brush fillBrush = new SolidBrush(Color.FromArgb(60, Color)))
                    g.FillPolygon(fillBrush, sf, FillMode.Alternate);
            }
            if (BrushThickness > 0 && pts.Count >= 2)
            {
                float thickS = (float)(BrushThickness * scale);
                using (Pen thickPen = new Pen(Color.FromArgb(80, Color), thickS))
                {
                    thickPen.StartCap = LineCap.Round;
                    thickPen.EndCap = LineCap.Round;
                    var drawPts = IsBuilding ? pts : new List<HPoint>(pts) { pts[0] };
                    PointF[] sf2 = drawPts.ConvertAll(conv).ToArray();
                    g.DrawLines(thickPen, sf2);
                }
            }
            using (Pen pen = CreatePen(selected, scale))
                g.DrawPolygon(pen, sf);
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (Points.Count < 3) return false;
            return HTableGeom.PointInPolygon(p, Points)
                || HTableGeom.DistancePointToPolyline(p, Points, true) <= tol + BrushThickness;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect() => HTableGeom.PolygonBoundingRect(Points);
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Points.Count >= MinPoints && Area > 1;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        { var c = new HTableMask(); CopyBaseTo(c); c.BrushThickness = BrushThickness; c.Fill = Fill; return c; }
    }
}
