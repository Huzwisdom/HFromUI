using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart
{
    /// <summary>
    /// 图表调色板静态工具类。根据 HChartPalette 枚举返回预定义的颜色数组，
    /// Series 在未显式指定 Color 时按索引循环使用这些颜色。
    /// </summary>
    public static class ChartPalettes
    {
        /// <summary>每个调色板的颜色数组（惰性初始化）。</summary>
        private static readonly Dictionary<HChartPalette, Color[]> _palettes = BuildPalettes();

        /// <summary>构建各调色板的颜色表（程序启动时一次性初始化，之后只读）。</summary>
        private static Dictionary<HChartPalette, Color[]> BuildPalettes()
        {
            var d = new Dictionary<HChartPalette, Color[]>
            {
                [HChartPalette.Default] = new[]
                {
                    Color.FromArgb(31, 119, 180),   // Blue
                    Color.FromArgb(255, 127, 14),   // Orange
                    Color.FromArgb(44, 160, 44),    // Green
                    Color.FromArgb(214, 39, 40),    // Red
                    Color.FromArgb(148, 103, 189),  // Purple
                    Color.FromArgb(140, 86, 75),    // Brown
                    Color.FromArgb(227, 119, 194),  // Pink
                    Color.FromArgb(127, 127, 127),  // Gray
                    Color.FromArgb(188, 189, 34),   // Olive
                    Color.FromArgb(23, 190, 207),   // Cyan
                },
                [HChartPalette.Warm] = new[]
                {
                    Color.FromArgb(230, 57, 70),
                    Color.FromArgb(247, 127, 0),
                    Color.FromArgb(242, 190, 69),
                    Color.FromArgb(249, 162, 112),
                    Color.FromArgb(239, 111, 87),
                    Color.FromArgb(243, 204, 46),
                    Color.FromArgb(214, 39, 40),
                    Color.FromArgb(255, 159, 243),
                },
                [HChartPalette.Cool] = new[]
                {
                    Color.FromArgb(31, 119, 180),
                    Color.FromArgb(52, 152, 219),
                    Color.FromArgb(46, 204, 113),
                    Color.FromArgb(26, 188, 156),
                    Color.FromArgb(155, 89, 182),
                    Color.FromArgb(52, 73, 94),
                    Color.FromArgb(142, 168, 210),
                    Color.FromArgb(127, 140, 141),
                },
                [HChartPalette.Ocean] = new[]
                {
                    Color.FromArgb(0, 51, 102),
                    Color.FromArgb(0, 102, 153),
                    Color.FromArgb(0, 153, 204),
                    Color.FromArgb(0, 204, 204),
                    Color.FromArgb(0, 153, 153),
                    Color.FromArgb(153, 204, 204),
                    Color.FromArgb(102, 153, 204),
                    Color.FromArgb(51, 153, 255),
                },
                [HChartPalette.Earth] = new[]
                {
                    Color.FromArgb(139, 90, 43),
                    Color.FromArgb(160, 82, 45),
                    Color.FromArgb(101, 67, 33),
                    Color.FromArgb(85, 107, 47),
                    Color.FromArgb(143, 188, 143),
                    Color.FromArgb(154, 205, 50),
                    Color.FromArgb(34, 139, 34),
                    Color.FromArgb(190, 178, 143),
                },
            };
            return d;
        }

        /// <summary>根据调色板和索引取颜色（索引循环），浅底用。</summary>
        public static Color GetColor(HChartPalette palette, int index) => GetColor(palette, index, false);

        /// <summary>根据调色板和索引取颜色（索引循环）。
        /// dark=true（深底主题）时把过暗/过灰的色提亮加饱和，保证系列色与背景反色调、看得清。</summary>
        public static Color GetColor(HChartPalette palette, int index, bool dark)
        {
            if (palette == HChartPalette.Custom || !_palettes.TryGetValue(palette, out var colors) || colors.Length == 0)
                colors = _palettes[HChartPalette.Default];
            var c = colors[index % colors.Length];
            return dark ? AdaptOnDark(c) : c;
        }

        /// <summary>深色背景上的系列色自适应：亮度不足提到约 0.62，彩色饱和度不足提到 0.6（灰阶不动）。</summary>
        private static Color AdaptOnDark(Color c)
        {
            ToHsl(c, out double h, out double s, out double l);
            if (l < 0.62) l = 0.62;
            if (l > 0.88) l = 0.88;
            if (s < 0.6 && s > 0.0) s = 0.6;
            return FromHsl(h, s, l);
        }

        /// <summary>RGB → HSL（h:0..360，s/l:0..1）。</summary>
        private static void ToHsl(Color c, out double h, out double s, out double l)
        {
            double r = c.R / 255.0, g = c.G / 255.0, b = c.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            l = (max + min) / 2;
            double d = max - min;
            if (d == 0) { h = 0; s = 0; return; }
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;
            h *= 60;
        }

        /// <summary>HSL → RGB（h:0..360，s/l:0..1）。</summary>
        private static Color FromHsl(double h, double s, double l)
        {
            double c2 = (1 - Math.Abs(2 * l - 1)) * s;
            double hp = h / 60.0;
            double x = c2 * (1 - Math.Abs(hp % 2 - 1));
            double r1, g1, b1;
            if (hp < 1) { r1 = c2; g1 = x; b1 = 0; }
            else if (hp < 2) { r1 = x; g1 = c2; b1 = 0; }
            else if (hp < 3) { r1 = 0; g1 = c2; b1 = x; }
            else if (hp < 4) { r1 = 0; g1 = x; b1 = c2; }
            else if (hp < 5) { r1 = x; g1 = 0; b1 = c2; }
            else { r1 = c2; g1 = 0; b1 = x; }
            double m = l - c2 / 2;
            return Color.FromArgb((int)Math.Round((r1 + m) * 255),
                                  (int)Math.Round((g1 + m) * 255),
                                  (int)Math.Round((b1 + m) * 255));
        }

        /// <summary>获取调色板的颜色数量。</summary>
        public static int Count(HChartPalette palette)
        {
            if (!_palettes.TryGetValue(palette, out var c)) c = _palettes[HChartPalette.Default];
            return c.Length;
        }
    }
}
