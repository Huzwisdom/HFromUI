using HFromUI.HMath;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HColor
{
    public class HBorder
    {
        /// <summary>Up 成员。</summary>
        public HDouble Up { get; set; }
        /// <summary>Down 成员。</summary>
        public HDouble Down { get; set; }
        /// <summary>左边。</summary>
        public HDouble Left { get; set; }
        /// <summary>Rigth 成员。</summary>
        public HDouble Rigth { get; set; }

        /// <summary>屏幕起点位置。</summary>
        public HPoint Location => new HPoint(Left, Up);

        /// <summary>宽度。</summary>
        public HDouble Width { get; set; }
        /// <summary>高度。</summary>
        public HDouble Height { get; set; }
        /// <summary>Radius 成员。</summary>
        public HDouble Radius { get; set; }
        /// <summary>线宽。</summary>
        public HDouble LineWidth { get; set; }
        /// <summary>文本。</summary>
        public string Text { get; set; }
        /// <summary>字体。</summary>
        public Font Font { get; set; }
        /// <summary>TextAlign 成员。</summary>
        public ContentAlignment TextAlign { get; set; }
        /// <summary>线颜色。</summary>
        public Color LineColor { get; set; }

        // 新增：文字颜色
        public Color TextColor { get; set; } = Color.Black;

        /// <summary>
        /// 获取边框的外轮廓路径（已存在，未修改）
        /// </summary>
        public GraphicsPath GetGraphicsPath()
        {
            GraphicsPath path = new GraphicsPath();
            float x = Left.ToSingle();
            float y = Up.ToSingle();
            float w = (Width - Left - Rigth).ToSingle();
            float h = (Height - Up - Down).ToSingle();
            float r = Radius.ToSingle();

            if (r > w / 2) r = w / 2;
            if (r > h / 2) r = h / 2;

            path.AddArc(x, y, r * 2, r * 2, 180, 90);
            path.AddLine(x + r, y, x + w - r, y);
            path.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
            path.AddLine(x + w, y + r, x + w, y + h - r);
            path.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
            path.AddLine(x + w - r, y + h, x + r, y + h);
            path.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
            path.AddLine(x, y + h - r, x, y + r);
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// 获取文字绘制区域（边框内缩一个线条宽度）
        /// </summary>
        public RectangleF GetTextRectangle()
        {
            float lw = LineWidth.ToSingle();
            return new RectangleF(
                Left.ToSingle() + lw,
                Up.ToSingle() + lw,
                (Width - Left - Rigth).ToSingle() - 2 * lw,
                (Height - Up - Down).ToSingle() - 2 * lw);
        }

        /// <summary>
        /// 绘制边框和文字
        /// </summary>
        public void Draw(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. 画边框
            using (var path = GetGraphicsPath())
            using (var pen = new Pen(LineColor, LineWidth.ToSingle()))
            {
                g.DrawPath(pen, path);
            }

            // 2. 画文字
            if (!string.IsNullOrEmpty(Text) && Font != null)
            {
                RectangleF rect = GetTextRectangle();
                using (var brush = new SolidBrush(TextColor))
                using (var format = new StringFormat())
                {
                    // 对齐转换
                    switch (TextAlign)
                    {
                        case ContentAlignment.TopLeft:
                            format.Alignment = StringAlignment.Near;
                            format.LineAlignment = StringAlignment.Near;
                            break;
                        case ContentAlignment.TopCenter:
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Near;
                            break;
                        case ContentAlignment.TopRight:
                            format.Alignment = StringAlignment.Far;
                            format.LineAlignment = StringAlignment.Near;
                            break;
                        case ContentAlignment.MiddleLeft:
                            format.Alignment = StringAlignment.Near;
                            format.LineAlignment = StringAlignment.Center;
                            break;
                        case ContentAlignment.MiddleCenter:
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Center;
                            break;
                        case ContentAlignment.MiddleRight:
                            format.Alignment = StringAlignment.Far;
                            format.LineAlignment = StringAlignment.Center;
                            break;
                        case ContentAlignment.BottomLeft:
                            format.Alignment = StringAlignment.Near;
                            format.LineAlignment = StringAlignment.Far;
                            break;
                        case ContentAlignment.BottomCenter:
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Far;
                            break;
                        case ContentAlignment.BottomRight:
                            format.Alignment = StringAlignment.Far;
                            format.LineAlignment = StringAlignment.Far;
                            break;
                        default:
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Center;
                            break;
                    }
                    g.DrawString(Text, Font, brush, rect, format);
                }
            }
        }
    }
}
