using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 三次贝塞尔曲线：4 个控制点。用于流畅曲线、S 轮廓、任意弯曲路径。
    /// </summary>
    [Serializable]
    public class HTableBezier : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Bezier;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("三次贝塞尔");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 4;

        /// <summary>P0 成员。</summary>
        public HPoint P0 => Points.Count > 0 ? Points[0] : null;
        /// <summary>P1 成员。</summary>
        public HPoint P1 => Points.Count >= 2 ? Points[1] : null;
        /// <summary>P2 成员。</summary>
        public HPoint P2 => Points.Count >= 3 ? Points[2] : null;
        /// <summary>P3 成员。</summary>
        public HPoint P3 => Points.Count >= 4 ? Points[3] : null;

        /// <summary>将曲线上 t ∈ [0,1] 的点计算出来</summary>
        public HPoint Evaluate(double t)
        {
            if (P0 == null || P1 == null || P2 == null || P3 == null) return null;
            double mt = 1 - t;
            double x = mt * mt * mt * P0.X.Value + 3 * mt * mt * t * P1.X.Value + 3 * mt * t * t * P2.X.Value + t * t * t * P3.X.Value;
            double y = mt * mt * mt * P0.Y.Value + 3 * mt * mt * t * P1.Y.Value + 3 * mt * t * t * P2.Y.Value + t * t * t * P3.Y.Value;
            return new HPoint(x, y);
        }

        /// <summary>把曲线重采样成折线（等间距 dx 像素）</summary>
        public List<HPoint> Sample(double dx = 1.0)
        {
            var result = new List<HPoint>();
            if (P0 == null || P1 == null || P2 == null || P3 == null) return result;
            result.Add(P0.Clone());
            double step = 0.005;
            double prevX = P0.X.Value, prevY = P0.Y.Value;
            for (double t = step; t <= 1.0001; t += step)
            {
                HPoint np = Evaluate(Math.Min(t, 1.0));
                double d = Math.Sqrt((double)(np.X.Value - prevX) * (double)(np.X.Value - prevX) + (double)(np.Y.Value - prevY) * (double)(np.Y.Value - prevY));
                if (d >= dx)
                {
                    result.Add(np); prevX = np.X.Value; prevY = np.Y.Value;
                }
            }
            if (result.Count == 0 || result[result.Count - 1].DistanceTo(P3) > 1)
                result.Add(P3.Clone());
            return result;
        }

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (P0 == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                if (P3 != null)
                {
                    PointF p0 = imageToScreen(P0);
                    PointF p1 = imageToScreen(P1);
                    PointF p2 = imageToScreen(P2);
                    PointF p3 = imageToScreen(P3);
                    g.DrawBezier(pen, p0, p1, p2, p3);
                    // 画虚线连接线（辅助线）
                    using (Pen aux = new Pen(Color.FromArgb(100, pen.Color), 1f) { DashStyle = DashStyle.Dot })
                    {
                        g.DrawLine(aux, p0, p1);
                        g.DrawLine(aux, p3, p2);
                    }
                }
                for (int i = 0; i < Points.Count; i++)
                    DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol)
        {
            if (P3 == null) return false;
            var pts = Sample(2);
            return HTableGeom.DistancePointToPolyline(p, pts, false) <= tol;
        }
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect() => HTableGeom.PolygonBoundingRect(Points);
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => P3 != null;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableBezier(); CopyBaseTo(c); return c; }
    }
}
