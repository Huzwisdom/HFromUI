using HFromUI.HMath;
using System;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 点 ROI（特征点）：单击放置。图像像素坐标。
    /// 绘制风格参考 HCoordinate/HDrawPoint 与 HDrawPointCross：圆 + 十字。
    /// </summary>
    [Serializable]
    public class HTablePoint : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Point;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("点");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 1;

        /// <summary>点中心（图像像素坐标）</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;

        /// <summary>屏幕上点标记半径（像素，固定屏幕尺寸）</summary>
        public float ScreenRadius { get; set; } = 5f;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (Points.Count == 0) return;
            Color color = selected ? HTableShapeList.SelectedColor : (IsBuilding ? HTableShapeList.BuildingColor : Color);
            PointF c = imageToScreen(Points[0]);
            float r = selected ? ScreenRadius + 1.5f : ScreenRadius;
            using (Pen pen = new Pen(color, selected ? LineWidth + 1.5f : LineWidth))
            using (Brush brush = new SolidBrush(color))
            {
                DrawCross(g, c, r + 3, pen);
                g.FillEllipse(brush, c.X - r * 0.5f, c.Y - r * 0.5f, r, r);
                g.DrawEllipse(pen, c.X - r, c.Y - r, r * 2, r * 2);
            }
        }

        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            if (Points.Count == 0) return false;
            return Points[0].DistanceTo(imagePoint) <= Math.Max(toleranceImagePx, 2.0);
        }

        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid()
        {
            return Points.Count >= 1 && Center != null;
        }

        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape()
        {
            HTablePoint c = new HTablePoint { ScreenRadius = ScreenRadius };
            CopyBaseTo(c);
            return c;
        }
    }
}
