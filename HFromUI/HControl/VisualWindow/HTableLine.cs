using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 直线 ROI：按下拖拽（或两点单击）确定起止点。图像像素坐标。
    /// 可用于拟合直线、边缘检测、测距、测角等找特征处理。
    /// </summary>
    [Serializable]
    public class HTableLine : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Line;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("直线");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 2;

        /// <summary>起点（图像像素坐标）</summary>
        public HPoint Start => Points.Count > 0 ? Points[0] : null;

        /// <summary>终点（图像像素坐标）</summary>
        public HPoint End => Points.Count > 1 ? Points[1] : null;

        /// <summary>线段长度（像素）</summary>
        public double Length
        {
            get
            {
                if (Start == null || End == null) return 0;
                return HTableGeom.Distance(Start, End);
            }
        }

        /// <summary>线段方向角（度，图像坐标系 Y 向下：0=向右，90=向下）</summary>
        public double AngleDeg
        {
            get
            {
                if (Start == null || End == null) return 0;
                return HTableGeom.NormalizeAngle360(
                    Math.Atan2((double)(End.Y.Value - Start.Y.Value), (double)(End.X.Value - Start.X.Value)) * 180.0 / Math.PI);
            }
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
                PointF a = imageToScreen(pts[0]);
                PointF b = imageToScreen(pts[1]);
                g.DrawLine(pen, a, b);
                // 端点
                DrawHandle(g, a, !IsBuilding, pen.Color);
                DrawHandle(g, b, !IsBuilding, pen.Color);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Start == null || End == null) return false;
            return HTableGeom.DistancePointToSegment(imagePoint, Start, End) <= toleranceImagePx;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            return Start != null && End != null && Length > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTableLine c = new HTableLine();
            CopyBaseTo(c);
            return c;
        }
    }
}
