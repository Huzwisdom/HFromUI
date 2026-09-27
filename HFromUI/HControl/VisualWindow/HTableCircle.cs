using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 圆 ROI：按下定圆心、拖拽定半径（P0=圆心，P1=圆周上一点）。图像像素坐标。
    /// 适用于圆孔、圆定位、圆拟合等找特征处理。
    /// </summary>
    [Serializable]
    public class HTableCircle : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Circle;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("圆");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 2;

        /// <summary>圆心（图像像素坐标）</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;

        /// <summary>半径（像素）</summary>
        public double Radius
        {
            get
            {
                if (Center == null || Points.Count < 2) return 0;
                return HTableGeom.Distance(Points[0], Points[1]);
            }
        }

        /// <summary>点是否在圆内（含边界）</summary>
        public bool Contains(HPoint imagePoint)
        {
            if (Center == null) return false;
            return HTableGeom.Distance(imagePoint, Center) <= Radius;
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 1) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF c = imageToScreen(pts[0]);
                float rScreen;
                if (pts.Count >= 2)
                {
                    PointF e = imageToScreen(pts[pts.Count - 1]);
                    rScreen = (float)Math.Sqrt((e.X - c.X) * (e.X - c.X) + (e.Y - c.Y) * (e.Y - c.Y));
                }
                else rScreen = 1f;

                if (pts.Count >= 2)
                    g.DrawEllipse(pen, c.X - rScreen, c.Y - rScreen, rScreen * 2, rScreen * 2);

                // 圆心标记
                DrawCross(g, c, 5, pen);
                DrawHandle(g, c, true, pen.Color);
                if (pts.Count >= 2)
                {
                    PointF edge = imageToScreen(pts[pts.Count - 1]);
                    g.DrawLine(pen, c, edge); // 半径线
                    DrawHandle(g, edge, !IsBuilding, pen.Color);
                }
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Center == null || Radius <= 0) return false;
            // 圆内或贴近圆周均算命中
            double d = HTableGeom.Distance(imagePoint, Center);
            return d <= Radius + toleranceImagePx;
        }

        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Center == null || Radius <= 0) return RectangleF.Empty;
            float r = (float)Radius;
            float cx = (float)Center.X.Value, cy = (float)Center.Y.Value;
            return new RectangleF(cx - r, cy - r, r * 2, r * 2);
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            return Center != null && Radius > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTableCircle c = new HTableCircle();
            CopyBaseTo(c);
            return c;
        }
    }
}
