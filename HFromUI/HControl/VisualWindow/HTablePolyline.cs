using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 折线 ROI（开放）：逐点单击，双击完成，右键/Esc 取消。图像像素坐标。
    /// 适用于沿边缘路径、轨迹线等找特征处理。
    /// </summary>
    [Serializable]
    public class HTablePolyline : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Polyline;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("折线");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => int.MaxValue; // 不固定，双击完成

        /// <summary>完成所需最少点数</summary>
        public const int MinPoints = 2;

        /// <summary>折线总长度（像素）</summary>
        public double Length
        {
            get
            {
                double sum = 0;
                for (int i = 1; i < Points.Count; i++)
                    sum += HTableGeom.Distance(Points[i - 1], Points[i]);
                return sum;
            }
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
                    g.DrawLines(pen, sf);
                }
                int anchorCount = Points.Count;
                for (int i = 0; i < anchorCount; i++)
                    DrawHandle(g, imageToScreen(Points[i]), !IsBuilding, pen.Color);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Points.Count < 2)
            {
                return Points.Count == 1 && Points[0].DistanceTo(imagePoint) <= toleranceImagePx;
            }
            return HTableGeom.DistancePointToPolyline(imagePoint, Points, false) <= toleranceImagePx;
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            return Points.Count >= MinPoints && Length > 1.0;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTablePolyline c = new HTablePolyline();
            CopyBaseTo(c);
            return c;
        }
    }
}
