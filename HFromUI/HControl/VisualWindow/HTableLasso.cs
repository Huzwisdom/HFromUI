using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 套索（Lasso）：按下拖拽任意形状，松开自动闭合起点到终点。
    /// 闭合多边形 ROI，不规则区域找特征。
    /// </summary>
    [Serializable]
    public class HTableLasso : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Lasso;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("套索");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => int.MaxValue;

        public const int MinPoints = 3;

        /// <summary>闭合多边形面积（像素²）</summary>
        public double Area => HTableGeom.PolygonArea(Points);

        /// <summary>Centroid 成员。</summary>
        public HPoint Centroid => HTableGeom.PolygonCentroid(Points);

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
                    {
                        g.DrawLines(pen, pts.ConvertAll(new System.Converter<HPoint, PointF>(imageToScreen)).ToArray());
                    }
                }
                return;
            }
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF[] sf = new PointF[pts.Count];
                for (int i = 0; i < pts.Count; i++) sf[i] = imageToScreen(pts[i]);
                if (pts.Count >= 3)
                    g.DrawPolygon(pen, sf);
                for (int i = 0; i < Points.Count; i++)
                    DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (Points.Count < 3) return false;
            return HTableGeom.PointInPolygon(p, Points)
                || HTableGeom.DistancePointToPolyline(p, Points, true) <= tol;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect() => HTableGeom.PolygonBoundingRect(Points);
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Points.Count >= MinPoints && Area > 1;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableLasso(); CopyBaseTo(c); return c; }
    }
}
