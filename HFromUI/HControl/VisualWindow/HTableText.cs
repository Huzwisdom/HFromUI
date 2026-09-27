using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    [Serializable]
    public class HTableText : HTableShape
    {
        /// <summary>ShapeType 成员。</summary>
        public override HTableTool ShapeType => HTableTool.Text;
        /// <summary>TypeName 成员。</summary>
        public override string TypeName => HTranslation.GetContent("文字标注");
        /// <summary>RequiredPoints 成员。</summary>
        public override int RequiredPoints => 1;
        /// <summary>文本。</summary>
        public string Text { get; set; } = "Label";
        /// <summary>DrawLeader 成员。</summary>
        public bool DrawLeader { get; set; } = false;
        /// <summary>LeaderAnchor 成员。</summary>
        public HPoint LeaderAnchor { get; set; }
        /// <summary>Anchor 成员。</summary>
        public HPoint Anchor => Points.Count > 0 ? Points[0] : null;

        /// <summary>绘制。</summary>
        public override void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale)
        {
            if (Anchor == null || string.IsNullOrEmpty(Text)) return;
            PointF sp = imageToScreen(Anchor);
            System.Drawing.Font font = SystemFonts.DefaultFont;
            SizeF sz = g.MeasureString(Text, font);
            RectangleF bg = new RectangleF(sp.X + 4, sp.Y - sz.Height - 4, sz.Width + 8, sz.Height + 4);
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                g.FillRectangle(bgBrush, bg);
            using (Brush fgBrush = new SolidBrush(Color))
                g.DrawString(Text, font, fgBrush, sp.X + 4, sp.Y - sz.Height - 2);
            if (DrawLeader)
            {
                using (Pen pen = CreatePen(selected, scale))
                {
                    PointF lead = LeaderAnchor != null ? imageToScreen(LeaderAnchor) : sp;
                    g.DrawLine(pen, new PointF(sp.X + 4, sp.Y), lead);
                }
            }
            DrawHandle(g, sp, true, Color);
        }
        /// <summary>命中测试。</summary>
        public override bool HitTest(HPoint p, double tol) => Anchor != null && HTableGeom.Distance(p, Anchor) <= tol + 20;
        /// <summary>获取 boundingRect。</summary>
        public override RectangleF GetBoundingRect()
        {
            if (Anchor == null) return RectangleF.Empty;
            return new RectangleF((float)Anchor.X.Value - 5, (float)Anchor.Y.Value - 20, 100, 25);
        }
        /// <summary>判断是否 Valid。</summary>
        public override bool IsValid() => Anchor != null && !string.IsNullOrEmpty(Text);
        /// <summary>CloneShape 方法。</summary>
        public override HTableShape CloneShape() { var c = new HTableText(); CopyBaseTo(c); c.Text = Text; c.DrawLeader = DrawLeader; c.LeaderAnchor = LeaderAnchor?.Clone(); return c; }
    }
}
