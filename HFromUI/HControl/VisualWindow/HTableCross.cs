using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableCross : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Cross;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("十字准星");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 1;

        /// <summary>十字臂长（图像像素）</summary>
        public double ArmLength { get; set; } = 20;

        /// <summary>Center 成员。</summary>
        public HPoint Center => Points.Count > 0 ? Points[0] : null;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (Center == null) return;
            using (Pen pen = CreatePen(selected, scale))
            {
                PointF c = imageToScreen(Center);
                // 画十字（四条臂）
                g.DrawLine(pen, c.X - (float)(ArmLength * scale), c.Y, c.X + (float)(ArmLength * scale), c.Y);
                g.DrawLine(pen, c.X, c.Y - (float)(ArmLength * scale), c.X, c.Y + (float)(ArmLength * scale));
                // 画中心点圆圈
                float r = 3 * scale;
                if (r < 2) r = 2;
                g.DrawEllipse(pen, c.X - r, c.Y - r, r * 2, r * 2);
                DrawHandle(g, c, true, pen.Color);
            }
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol) => Center != null && HTableGeom.Distance(p, Center) <= tol + ArmLength;
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Center == null) return RectangleF.Empty;
            float r = (float)ArmLength;
            return new RectangleF((float)Center.X.Value - r, (float)Center.Y.Value - r, r * 2, r * 2);
        }
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Center != null;
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableCross(); CopyBaseTo(c); c.ArmLength = ArmLength; return c; }
    }
}
