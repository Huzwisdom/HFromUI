using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTablePointRect : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.PointRect;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("点方框");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 2;

        /// <summary>Center 成员。</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;
        /// <summary>Corner 成员。</summary>
        public HPoint Corner => Points.Count >= 2 ? Points[1] : null;
        /// <summary>宽度。</summary>
        public double Width => Corner != null && Center != null ? Math.Abs((double)Corner.X.Value - (double)Center.X.Value) * 2 : 0;
        /// <summary>高度。</summary>
        public double Height => Corner != null && Center != null ? Math.Abs((double)Corner.Y.Value - (double)Center.Y.Value) * 2 : 0;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (Center == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF c = imageToScreen(Center);
                if (Corner != null)
                {
                    PointF cr = imageToScreen(Corner);
                    float x = Math.Min(c.X, cr.X);
                    float y = Math.Min(c.Y, cr.Y);
                    float w = Math.Abs(c.X - cr.X) * 2;
                    float h = Math.Abs(c.Y - cr.Y) * 2;
                    g.DrawRectangle(pen, x, y, w, h);
                    DrawCross(g, c, 5, pen);
                    DrawHandle(g, cr, !IsBuilding, pen.Color);
                }
                DrawHandle(g, c, true, pen.Color);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (Center == null) return false;
            if (Corner == null) return HTableGeom.Distance(p, Center) <= tol;
            float minX = (float)Math.Min(Center.X.Value, Corner.X.Value), minY = (float)Math.Min(Center.Y.Value, Corner.Y.Value);
            float w = (float)Width, h = (float)Height;
            RectangleF rect = new RectangleF(minX, minY, w, h);
            return rect.Contains((float)p.X.Value, (float)p.Y.Value) || HTableGeom.DistancePointToSegment(p, Center, Corner) <= tol;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Center == null) return RectangleF.Empty;
            float minX = (float)Center.X.Value - 4, minY = (float)Center.Y.Value - 4;
            float w = 8, h = 8;
            if (Corner != null)
            {
                minX = (float)Math.Min(Center.X.Value, Corner.X.Value);
                minY = (float)Math.Min(Center.Y.Value, Corner.Y.Value);
                w = (float)Width; h = (float)Height;
            }
            return new RectangleF(minX, minY, w, h);
        }
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Center != null;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTablePointRect(); CopyBaseTo(c); return c; }
    }
}
