using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 旋转矩形 ROI（对应 Halcon rectangle2 / VisionPro 旋转矩形）。
    /// 交互：先按下拖拽一条边（确定中心、角度、长边），松开后移动鼠标单击确定宽度。
    /// 图像像素坐标。适用于有角度的目标找特征（定位、测量、对准）。
    /// </summary>
    [Serializable]
    public class HTableRotatedRectangle : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.RotatedRectangle;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("旋转矩形");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 3;

        /// <summary>边起点 P0</summary>
        public HPoint EdgeStart => Points.Count > 0 ? Points[0] : null;

        /// <summary>边终点 P1</summary>
        public HPoint EdgeEnd => Points.Count > 1 ? Points[1] : null;

        /// <summary>宽度点 P2（到边的垂直距离=半宽）</summary>
        public HPoint WidthPoint => Points.Count > 2 ? Points[2] : null;

        /// <summary>中心（图像像素坐标）</summary>
        public HPoint Center
        {
            get
            {
                if (EdgeStart == null || EdgeEnd == null) return null;
                return new HPoint(((double)EdgeStart.X.Value + (double)EdgeEnd.X.Value) / 2.0,
                                  ((double)EdgeStart.Y.Value + (double)EdgeEnd.Y.Value) / 2.0);
            }
        }

        /// <summary>旋转角（度，图像坐标系 Y 向下：0=水平向右，顺时针为正）</summary>
        public double AngleDeg
        {
            get
            {
                if (EdgeStart == null || EdgeEnd == null) return 0;
                return Math.Atan2((double)(EdgeEnd.Y.Value - EdgeStart.Y.Value),
                                  (double)(EdgeEnd.X.Value - EdgeStart.X.Value)) * 180.0 / Math.PI;
            }
        }

        /// <summary>长边长度（沿边方向，像素）</summary>
        public double Length1
        {
            get
            {
                if (EdgeStart == null || EdgeEnd == null) return 0;
                return HTableGeom.Distance(EdgeStart, EdgeEnd);
            }
        }

        /// <summary>短边长度（宽度，像素，= 2 倍宽度点到边的垂直距离）</summary>
        public double Length2
        {
            get
            {
                if (EdgeStart == null || EdgeEnd == null || WidthPoint == null) return 0;
                return 2.0 * HTableGeom.DistancePointToSegment(WidthPoint, EdgeStart, EdgeEnd);
            }
        }

        /// <summary>长边方向单位向量</summary>
        private HPoint AxisDir()
        {
            HPoint d = EdgeEnd - EdgeStart;
            double len = d.DistanceTo(new HPoint(0, 0)).Value;
            if (len < 1e-9) return new HPoint(1, 0);
            return new HPoint((double)d.X.Value / len, (double)d.Y.Value / len);
        }

        /// <summary>短边方向单位向量（图像坐标 Y 向下，垂直方向取 (-y, x)）</summary>
        private static HPoint NormalDir(HPoint axis)
        {
            return new HPoint(-(double)axis.Y.Value, (double)axis.X.Value);
        }

        /// <summary>
        /// 获取四个角点（图像像素坐标）：以边为中心轴，半宽向宽度点所在侧展开。
        /// </summary>
        /// <param name="widthPointOverride">宽度点覆盖（绘制中传入预览点）；null 用已提交的 P2</param>
        public HPoint[] GetCorners(HPoint widthPointOverride = null)
        {
            if (EdgeStart == null || EdgeEnd == null) return new HPoint[0];
            HPoint c = Center;
            HPoint axis = AxisDir();
            HPoint n = NormalDir(axis);
            double half1 = Length1 / 2.0;

            HPoint widthPt = widthPointOverride ?? WidthPoint;
            double half2 = 0;
            if (widthPt != null)
            {
                half2 = HTableGeom.DistancePointToSegment(widthPt, EdgeStart, EdgeEnd);
                // 宽度点在边的哪一侧，矩形就向哪一侧展开（保证包含宽度点）
                HPoint rel = widthPt - c;
                double side = (double)rel.X.Value * (double)n.X.Value + (double)rel.Y.Value * (double)n.Y.Value;
                if (side < 0) n = new HPoint(-(double)n.X.Value, -(double)n.Y.Value);
            }

            HPoint h1 = new HPoint(axis.X.Value * half1, axis.Y.Value * half1);
            HPoint h2 = new HPoint(n.X.Value * half2, n.Y.Value * half2);
            return new[]
            {
                c - h1 - h2,
                c + h1 - h2,
                c + h1 + h2,
                c - h1 + h2
            };
        }

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
                // 宽度点：完成时用 P2；绘制中（2 锚点+预览）用预览点
                HPoint widthPt = Points.Count >= 3 ? Points[2]
                                : (pts.Count >= 3 ? pts[pts.Count - 1] : null);
                if (widthPt != null)
                {
                    HPoint[] corners = GetCorners(widthPt);
                    if (corners.Length == 4)
                    {
                        PointF[] sf = new PointF[4];
                        for (int i = 0; i < 4; i++) sf[i] = imageToScreen(corners[i]);
                        g.DrawPolygon(pen, sf);
                    }
                    int anchorCount = Math.Min(Points.Count, 3);
                    for (int i = 0; i < anchorCount; i++)
                        DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
                }
                else
                {
                    // 第一步：拖出的边 + 中点垂直方向提示
                    PointF a = imageToScreen(pts[0]);
                    PointF b = imageToScreen(pts[1]);
                    g.DrawLine(pen, a, b);
                    DrawHandle(g, a, true, pen.Color);
                    DrawHandle(g, b, true, pen.Color);
                    PointF mid = new PointF((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                    // 屏幕坐标下与边垂直的方向
                    float dx = b.X - a.X, dy = b.Y - a.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (len > 1e-3f)
                    {
                        float nx = -dy / len, ny = dx / len;
                        g.DrawLine(pen, mid.X - nx * 7, mid.Y - ny * 7, mid.X + nx * 7, mid.Y + ny * 7);
                    }
                }
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Points.Count < 3) return false;
            HPoint c = Center;
            HPoint axis = AxisDir();
            HPoint n = NormalDir(axis);
            HPoint rel = imagePoint - c;
            double along = Math.Abs((double)rel.X.Value * (double)axis.X.Value + (double)rel.Y.Value * (double)axis.Y.Value);
            double perp = Math.Abs((double)rel.X.Value * (double)n.X.Value + (double)rel.Y.Value * (double)n.Y.Value);
            double half1 = Length1 / 2.0;
            double half2 = Length2 / 2.0;
            return along <= half1 + toleranceImagePx && perp <= half2 + toleranceImagePx;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            return Length1 > 1.0 && Length2 > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTableRotatedRectangle c = new HTableRotatedRectangle();
            CopyBaseTo(c);
            return c;
        }
    }
}
