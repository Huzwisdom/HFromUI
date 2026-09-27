using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 轴对齐椭圆 ROI：按下拖拽外接矩形（P0/P1=外接框对角点）。图像像素坐标。
    /// 适用于椭圆孔、类圆目标的找特征处理。
    /// </summary>
    [Serializable]
    public class HTableEllipse : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Ellipse;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("椭圆");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 2;

        /// <summary>外接矩形（图像像素坐标）</summary>
        public RectangleF Bounds
        {
            get
            {
                if (Points.Count < 2) return RectangleF.Empty;
                float x1 = (float)Points[0].X.Value, y1 = (float)Points[0].Y.Value;
                float x2 = (float)Points[1].X.Value, y2 = (float)Points[1].Y.Value;
                return RectangleF.FromLTRB(Math.Min(x1, x2), Math.Min(y1, y2),
                                          Math.Max(x1, x2), Math.Max(y1, y2));
            }
        }

        /// <summary>中心（图像像素坐标）</summary>
        public HPoint Center
        {
            get
            {
                RectangleF r = Bounds;
                return r.IsEmpty ? null : new HPoint(r.X + r.Width / 2.0, r.Y + r.Height / 2.0);
            }
        }

        /// <summary>X 半轴（像素）</summary>
        public double RadiusX => Bounds.Width / 2.0;

        /// <summary>Y 半轴（像素）</summary>
        public double RadiusY => Bounds.Height / 2.0;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 2)
            {
                if (pts.Count == 1) DrawHandle(g, imageToScreen(pts[0]), false, HTableShapeList.BuildingColor);
                return;
            }
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF a = imageToScreen(pts[0]);
                PointF b = imageToScreen(pts[pts.Count - 1]);
                float x = Math.Min(a.X, b.X), y = Math.Min(a.Y, b.Y);
                float w = Math.Abs(a.X - b.X), h = Math.Abs(a.Y - b.Y);
                g.DrawEllipse(pen, x, y, w, h);
                // 中心
                PointF c = new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                DrawCross(g, c, 4, pen);
                DrawHandle(g, a, !IsBuilding, pen.Color);
                DrawHandle(g, b, !IsBuilding, pen.Color);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            HPoint c = Center;
            if (c == null || RadiusX < 1e-6 || RadiusY < 1e-6) return false;
            double dx = ((double)imagePoint.X.Value - (double)c.X.Value) / RadiusX;
            double dy = ((double)imagePoint.Y.Value - (double)c.Y.Value) / RadiusY;
            // 椭圆内（公差按归一化坐标放宽）
            return dx * dx + dy * dy <= 1.0 + toleranceImagePx * 0.05;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            RectangleF r = Bounds;
            return r.Width > 1.0 && r.Height > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTableEllipse c = new HTableEllipse();
            CopyBaseTo(c);
            return c;
        }
    }
}
