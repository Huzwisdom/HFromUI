using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableBullseye : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Bullseye;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("双圆靶心");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 3;
        /// <summary>Center 成员。</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;
        /// <summary>ROuter 成员。</summary>
        public double ROuter => Points.Count >= 2 ? HTableGeom.Distance(Points[0], Points[1]) : 0;
        /// <summary>RInner 成员。</summary>
        public double RInner => Points.Count >= 3 ? HTableGeom.Distance(Points[0], Points[2]) : 0;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 1 || Center == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF c = imageToScreen(pts[0]);
                // 屏幕半径用变换后的锚点计算，缩放/拖拽时双圆始终跟锚点重合
                float rO = 0f, rI = 0f;
                if (pts.Count >= 2)
                {
                    PointF e1 = imageToScreen(pts[1]);
                    rO = (float)Math.Sqrt((e1.X - c.X) * (e1.X - c.X) + (e1.Y - c.Y) * (e1.Y - c.Y));
                }
                if (pts.Count >= 3)
                {
                    PointF e2 = imageToScreen(pts[2]);
                    rI = (float)Math.Sqrt((e2.X - c.X) * (e2.X - c.X) + (e2.Y - c.Y) * (e2.Y - c.Y));
                }
                if (rO > 0)
                    g.DrawEllipse(pen, c.X - rO, c.Y - rO, rO * 2, rO * 2);
                if (rI > 0)
                    g.DrawEllipse(pen, c.X - rI, c.Y - rI, rI * 2, rI * 2);
                DrawCross(g, c, 5, pen);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            double d = HTableGeom.Distance(p, Center);
            return d <= ROuter + tol && (RInner <= 0 || d >= RInner - tol);
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Center == null || ROuter <= 0) return RectangleF.Empty;
            float r = (float)ROuter;
            return new RectangleF((float)Center.X.Value - r, (float)Center.Y.Value - r, r * 2, r * 2);
        }
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Center != null && ROuter > 1;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableBullseye(); CopyBaseTo(c); return c; }
    }
}
