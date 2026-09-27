using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 自由曲线（Freehand）：按下拖拽自动追加点，松开完成。开放形状。
    /// 参考 Halcon XLD 轮廓、OpenCV 轮廓。
    /// </summary>
    [Serializable]
    public class HTableFreehand : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Freehand;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("自由曲线");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => int.MaxValue; // 不固定，松开完成

        /// <summary>完成所需最少点数</summary>
        public const int MinPoints = 2;

        /// <summary>折线总长度（图像像素）</summary>
        public double Length
        {
            get
            {
                if (Points.Count < 2) return 0;
                double total = 0;
                for (int i = 1; i < Points.Count; i++)
                    total += HTableGeom.Distance(Points[i - 1], Points[i]);
                return total;
            }
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count < 2) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF[] sf = new PointF[pts.Count];
                for (int i = 0; i < pts.Count; i++) sf[i] = imageToScreen(pts[i]);
                g.DrawLines(pen, sf);
                for (int i = 0; i < Points.Count; i++)
                    DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (Points.Count < 2) return false;
            return HTableGeom.DistancePointToPolyline(p, Points, false) <= tol;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect() => HTableGeom.PolygonBoundingRect(Points);
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Points.Count >= MinPoints && Length > 1;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableFreehand(); CopyBaseTo(c); return c; }
    }
}
