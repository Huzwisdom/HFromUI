using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 风格公共配色：橙色为强调色（Codrut Fluent Design 同款调），
    /// 另有范围滑块蓝、六色状态色、明暗两套面板底色与彩虹渐变色组。
    /// </summary>
    public static class HFluentPalette
    {
        /// <summary>强调橙（按钮/开关/滑块选中态）。</summary>
        public static readonly Color Accent = Color.FromArgb(249, 115, 22);

        /// <summary>范围滑块蓝。</summary>
        public static readonly Color Blue = Color.FromArgb(43, 107, 233);

        /// <summary>彩虹按钮紫。</summary>
        public static readonly Color Purple = Color.FromArgb(147, 51, 234);

        /// <summary>渐变起点粉。</summary>
        public static readonly Color Pink = Color.FromArgb(236, 72, 153);

        /// <summary>未选中轨道灰。</summary>
        public static readonly Color Track = Color.FromArgb(229, 231, 235);

        /// <summary>数值气泡近黑。</summary>
        public static readonly Color Bubble = Color.FromArgb(17, 24, 39);

        /// <summary>浅底弱化文字灰。</summary>
        public static readonly Color MutedText = Color.FromArgb(107, 114, 128);

        /// <summary>暗色主题窗体底。</summary>
        public static readonly Color DarkPanel = Color.FromArgb(31, 31, 31);

        /// <summary>暗色主题卡片底。</summary>
        public static readonly Color DarkCard = Color.FromArgb(45, 45, 45);

        /// <summary>暗色主题描边。</summary>
        public static readonly Color DarkBorder = Color.FromArgb(64, 64, 64);

        /// <summary>成功绿。</summary>
        public static readonly Color Success = Color.FromArgb(34, 197, 94);

        /// <summary>危险红。</summary>
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);

        /// <summary>信息蓝。</summary>
        public static readonly Color Info = Color.FromArgb(59, 130, 246);

        /// <summary>警告黄。</summary>
        public static readonly Color Warning = Color.FromArgb(234, 179, 8);

        /// <summary>疑问灰。</summary>
        public static readonly Color Question = Color.FromArgb(107, 114, 128);

        /// <summary>彩虹按钮横向渐变色组（红→黄→绿→蓝→紫→粉）。</summary>
        public static readonly Color[] Rainbow =
        {
            Color.FromArgb(239, 68, 68),
            Color.FromArgb(245, 158, 11),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(59, 130, 246),
            Color.FromArgb(139, 92, 246),
            Color.FromArgb(236, 72, 153)
        };

        /// <summary>两色线性渐变画刷（粉→紫，斜 45°）。</summary>
        public static LinearGradientBrush PinkPurple(RectangleF r)
        {
            return new LinearGradientBrush(r, Pink, Purple, 45f);
        }

        /// <summary>多色横向渐变画刷（彩虹）。</summary>
        public static LinearGradientBrush RainbowBrush(RectangleF r)
        {
            var brush = new LinearGradientBrush(r, Rainbow[0], Rainbow[Rainbow.Length - 1], 0f);
            brush.InterpolationColors = new ColorBlend
            {
                Positions = new[] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f },
                Colors = Rainbow
            };
            return brush;
        }
    }
}
