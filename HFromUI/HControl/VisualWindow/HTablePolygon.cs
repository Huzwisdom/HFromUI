using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 多边形 ROI（闭合）：逐点单击，双击闭合，右键/Esc 取消。图像像素坐标。
    /// 适用于任意形状区域的找特征处理（不规则 ROI、Blob 限定区域等）。
    /// </summary>
    [Serializable]
    public class HTablePolygon : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Polygon;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("多边形");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => int.MaxValue; // 不固定，双击完成

        /// <summary>完成所需最少点数</summary>
        public const int MinPoints = 3;

        /// <summary>多边形面积（像素²，鞋带公式）</summary>
        public double Area
        {
            get
            {
                if (Points.Count < 3) return 0;
                double s = 0;
                for (int i = 0; i < Points.Count; i++)
                {
                    HPoint a = Points[i];
                    HPoint b = Points[(i + 1) % Points.Count];
                    s += (double)a.X.Value * (double)b.Y.Value - (double)b.X.Value * (double)a.Y.Value;
                }
                return Math.Abs(s) / 2.0;
            }
        }

        /// <summary>点是否在多边形内部</summary>
        public bool Contains(HPoint imagePoint)
        {
            return HTableGeom.PointInPolygon(imagePoint, Points);
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            List<HPoint> pts = new List<HPoint>(EnumerateDrawPoints());
            if (pts.Count == 0) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                if (pts.Count >= 2)
                {
                    PointF[] sf = new PointF[pts.Count];
                    for (int i = 0; i < pts.Count; i++) sf[i] = imageToScreen(pts[i]);
                    if (pts.Count >= 3 && !IsBuilding)
                        g.DrawPolygon(pen, sf); // 完成：闭合
                    else
                        g.DrawLines(pen, sf);   // 绘制中：开放橡皮筋
                }
                int anchorCount = Points.Count;
                for (int i = 0; i < anchorCount; i++)
                    DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Points.Count < 3)
            {
                if (Points.Count == 1) return Points[0].DistanceTo(imagePoint) <= toleranceImagePx;
                if (Points.Count == 2) return HTableGeom.DistancePointToSegment(imagePoint, Points[0], Points[1]) <= toleranceImagePx;
                return false;
            }
            // 多边形内部或贴近边界均算命中
            return HTableGeom.PointInPolygon(imagePoint, Points)
                || HTableGeom.DistancePointToPolyline(imagePoint, Points, true) <= toleranceImagePx;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            return Points.Count >= MinPoints && Area > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTablePolygon c = new HTablePolygon();
            CopyBaseTo(c);
            return c;
        }
    }
}
