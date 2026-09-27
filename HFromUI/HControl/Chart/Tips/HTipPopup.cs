using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HFromUI.HControl.Chart.Tips
{
    /// <summary>
    /// 悬停气泡渲染器：由 HChart 在 OnPaint 中直接调用（非独立顶层窗口）。
    /// 负责测量、定位（右下展开，靠右/靠下越界自动翻转，整体夹在客户区内）、外形/配色/角饰绘制与逐行文字。
    /// 默认 Classic 外形 + Auto 配色 + None 装饰时，渲染结果与提取前 HChart.DrawTip 像素一致。
    /// </summary>
    public static class HTipPopup
    {
        /// <summary>行高（px，与提取前一致）。</summary>
        private const float Lh = 16f;
        /// <summary>气泡距鼠标锚点的间距：尽量贴近鼠标热点，又不压住光标箭头。</summary>
        private const float Gap = 6f;
        /// <summary>角饰尺寸。</summary>
        private const float ArtSize = 11.5f;

        /// <summary>取当前气泡配色板（HChart 构造文字行时用：正文色/强调色/系列色自适应）。</summary>
        public static HTipColors Colors(HTipSchemeKind scheme, bool themeDark)
            => HTipPalettes.Get(scheme, themeDark);

        /// <summary>系列图例色按气泡深浅自适应：深底气泡调亮、浅底气泡压暗（与提取前 AdaptForPopup 同参数）。</summary>
        public static Color Adapt(Color c, HTipColors sc)
            => sc.DarkPopup ? Brighten(c, 0.38f) : Darken(c, 0.7f);

        /// <summary>颜色压暗（浅底气泡上，浅色图例文字乘 0.7）。</summary>
        private static Color Darken(Color c, float f)
            => Color.FromArgb(Math.Min(255, (int)(c.R * f)), Math.Min(255, (int)(c.G * f)), Math.Min(255, (int)(c.B * f)));

        /// <summary>颜色调亮（深底气泡上，深色图例文字向白色混合 38%）。</summary>
        private static Color Brighten(Color c, float f)
            => Color.FromArgb((int)(c.R + (255 - c.R) * f), (int)(c.G + (255 - c.G) * f), (int)(c.B + (255 - c.B) * f));

        /// <summary>
        /// 在 HChart 画布上绘制悬停气泡。
        /// </summary>
        /// <param name="g">图表画布（OnPaint 传入）。</param>
        /// <param name="lines">已配色的文字行。</param>
        /// <param name="font">气泡字体（HChart 的 RulerFont：Segoe UI 8.5）。</param>
        /// <param name="anchor">鼠标锚点（客户区坐标）。</param>
        /// <param name="client">图表客户区矩形。</param>
        /// <param name="skin">气泡外形（64 种）。</param>
        /// <param name="scheme">气泡配色（12 种）。</param>
        /// <param name="themeDark">图表当前是否深色主题（Auto 配色随其反色）。</param>
        /// <param name="art">右上角饰（None=不画）。</param>
        /// <param name="offset">气泡相对默认位置的额外偏移（像素，X 向右、Y 向下为正；翻转侧同向平移）。</param>
        public static void Render(Graphics g, IList<HTipLine> lines, Font font, Point anchor, Rectangle client,
            HTipSkinKind skin, HTipSchemeKind scheme, bool themeDark, HTipArt art, PointF offset)
        {
            if (lines == null || lines.Count == 0) return;
            var pc = HTipPalettes.Get(scheme, themeDark);
            var def = HTipSkins.Get(skin);

            // 1) 量正文（TextRenderer，与提取前一致）
            float cw = 0f;
            foreach (var hl in lines)
                cw = Math.Max(cw, TextRenderer.MeasureText(hl.Text, font).Width);
            float ch = lines.Count * Lh;
            bool hasArt = art != HTipArt.None;

            // 2) 量外框（宽/高夹在客户区内，同提取前规则）
            var sz = HTipShape.Measure(def, cw, ch, hasArt);
            float bw = Math.Min(sz.Width, client.Width - 4f);
            float bh = Math.Min(sz.Height, client.Height - 4f);

            // 3) 定位：默认右下展开（含偏移），越界翻转（翻转侧同样叠加偏移），再夹到客户区 2px 内
            float bx = anchor.X + Gap + offset.X;
            float by = anchor.Y + Gap + offset.Y;
            bool flipH = false, flipV = false;
            if (bx + bw > client.Right - 2f) { bx = anchor.X - Gap - bw + offset.X; flipH = true; }   // 右侧不够 → 翻左
            if (by + bh > client.Bottom - 2f) { by = anchor.Y - Gap - bh + offset.Y; flipV = true; }  // 下侧不够 → 翻上
            bx = Math.Max(2f, Math.Min(bx, client.Width - bw - 2f));
            by = Math.Max(2f, Math.Min(by, client.Height - bh - 2f));

            // 4) 画外形（尾巴随翻转）
            var tail = HTipSkins.FlipTail(def.Tail, flipH, flipV);
            var box = new RectangleF(bx, by, bw, bh);
            HTipShape.Paint(g, def, box, tail, pc);

            // 5) 右上角饰（正文右侧已预留 15px）
            var body = HTipShape.BodyRect(def, box, tail);
            if (hasArt)
                HTipArts.Draw(g, new RectangleF(body.Right - ArtSize - 1.5f, body.Top - 0.5f, ArtSize, ArtSize),
                              art, pc.Accent, pc.TagText);

            // 6) 逐行文字
            var ori = HTipShape.TextOrigin(def, cw, ch, body, hasArt);
            for (int i = 0; i < lines.Count; i++)
                TextRenderer.DrawText(g, lines[i].Text, font,
                    new Point((int)ori.X, (int)(ori.Y + i * Lh)),
                    lines[i].Color);
        }
    }
}
