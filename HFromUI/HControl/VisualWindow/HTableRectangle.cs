using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 轴对齐矩形 ROI：按下拖拽对角点。图像像素坐标。
    /// 最常用的找特征区域：模板匹配、Blob 分析、灰度统计等。
    /// </summary>
    [Serializable]
    public class HTableRectangle : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Rectangle;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("矩形");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 2;

        /// <summary>归一化后的矩形（图像像素坐标，X/Y 为左上角）</summary>
        public RectangleF Rect
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

        /// <summary>矩形中心（图像像素坐标）</summary>
        public HPoint Center
        {
            get
            {
                RectangleF r = Rect;
                return r.IsEmpty ? null : new HPoint(r.X + r.Width / 2.0, r.Y + r.Height / 2.0);
            }
        }

        /// <summary>左上角（图像像素坐标）</summary>
        public HPoint TopLeft => Points.Count >= 2 ? new HPoint(Rect.X, Rect.Y) : null;

        /// <summary>宽（像素）</summary>
        public double Width => Rect.Width;

        /// <summary>高（像素）</summary>
        public double Height => Rect.Height;

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
                PointF b = imageToScreen(pts[1]);
                float x = Math.Min(a.X, b.X), y = Math.Min(a.Y, b.Y);
                float w = Math.Abs(a.X - b.X), h = Math.Abs(a.Y - b.Y);
                g.DrawRectangle(pen, x, y, w, h);
                DrawHandle(g, a, !IsBuilding, pen.Color);
                DrawHandle(g, b, !IsBuilding, pen.Color);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Points.Count < 2) return false;
            RectangleF r = Rect;
            // 矩形内或贴近边框均算命中（方便细线选择）
            float px = (float)imagePoint.X.Value, py = (float)imagePoint.Y.Value;
            bool inside = px >= r.X - toleranceImagePx && px <= r.Right + toleranceImagePx
                       && py >= r.Y - toleranceImagePx && py <= r.Bottom + toleranceImagePx;
            return inside;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            RectangleF r = Rect;
            return r.Width > 1.0 && r.Height > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTableRectangle c = new HTableRectangle();
            CopyBaseTo(c);
            return c;
        }
    }
}
