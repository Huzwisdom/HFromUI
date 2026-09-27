using System.Drawing;

namespace HFromUI.HControl.Tools.Button
{
    /// <summary>按钮着色方式：实心 / 描边 / 链接 / 立体 / 纵向渐变。</summary>
    internal enum HPushButtonMode
    {
        Fill = 0,
        Outline = 1,
        Link = 2,
        ThreeD = 3,
        Gradient = 4
    }

    /// <summary>一帧三色：底、边、字。</summary>
    internal struct HPushButtonPalette
    {
        public Color Back;
        public Color Border;
        public Color Fore;
    }

    /// <summary>
    /// 122 种预设风格配色表：30 个语义色 × 4 种着色方式，只定义一套基准色，
    /// 悬停/按下/禁用三态自动推导。
    /// </summary>
    internal static class HPushButtonScheme
    {
        // 30 个语义色（顺序与 HPushButtonStyle 各分段内的色名一一对应，0=主色蓝）
        private static readonly Color[] BaseColors =
        {
            Color.FromArgb(59, 130, 246),  // 0 Blue 主色蓝
            Color.FromArgb(107, 114, 128), // 1 Gray 中性灰
            Color.FromArgb(22, 163, 74),   // 2 Green 成功绿
            Color.FromArgb(220, 38, 38),   // 3 Red 危险红
            Color.FromArgb(245, 158, 11),  // 4 Amber 警告琥珀
            Color.FromArgb(6, 182, 212),   // 5 Cyan 信息青
            Color.FromArgb(243, 244, 246), // 6 Light 浅灰白
            Color.FromArgb(31, 41, 55),    // 7 Dark 深灰黑
            Color.FromArgb(79, 70, 229),   // 8 Indigo 靛蓝
            Color.FromArgb(147, 51, 234),  // 9 Purple 紫色
            Color.FromArgb(236, 72, 153),  // 10 Pink 粉玫
            Color.FromArgb(244, 63, 94),   // 11 Rose 玫瑰红
            Color.FromArgb(249, 115, 22),  // 12 Orange 橙色
            Color.FromArgb(13, 148, 136),  // 13 Teal 蓝绿
            Color.FromArgb(5, 150, 105),   // 14 Emerald 翡翠绿
            Color.FromArgb(132, 204, 22),  // 15 Lime 青柠
            Color.FromArgb(234, 179, 8),   // 16 Yellow 明黄
            Color.FromArgb(14, 165, 233),  // 17 Sky 天青
            Color.FromArgb(124, 58, 237),  // 18 Violet 紫罗兰
            Color.FromArgb(217, 70, 239),  // 19 Fuchsia 品红
            Color.FromArgb(146, 64, 14),   // 20 Brown 棕色
            Color.FromArgb(71, 85, 105),   // 21 Slate 板岩
            Color.FromArgb(202, 138, 4),   // 22 Gold 金黄
            Color.FromArgb(190, 18, 60),   // 23 Crimson 深红
            Color.FromArgb(16, 185, 129),  // 24 Mint 薄荷
            Color.FromArgb(2, 132, 199),   // 25 Ocean 海蓝
            Color.FromArgb(192, 38, 211),  // 26 Magenta 洋红
            Color.FromArgb(77, 124, 15),   // 27 Olive 橄榄
            Color.FromArgb(127, 29, 29),   // 28 Maroon 栗色
            Color.FromArgb(51, 65, 85)     // 29 Charcoal 炭灰
        };

        /// <summary>取风格的基准语义色（开关/勾选/单选的强调色同源）。</summary>
        public static Color BaseColor(HPushButtonStyle style)
        {
            int idx = HueIndex(style);
            return idx >= 0 ? BaseColors[idx] : BaseColors[0];
        }

        /// <summary>风格对应的绘制方式。</summary>
        public static HPushButtonMode Mode(HPushButtonStyle style)
        {
            int v = (int)style;
            if (style == HPushButtonStyle.Link) return HPushButtonMode.Link;
            if (v >= 400 && v <= 429) return HPushButtonMode.Gradient;
            if (v >= 300 && v <= 329) return HPushButtonMode.ThreeD;
            if (v >= 200 && v <= 229) return HPushButtonMode.Outline;
            return HPushButtonMode.Fill;
        }

        /// <summary>
        /// 解析某风格在指定状态下的三色板。
        /// hoverMix：悬停动画 0~1 进度；normal/hoverC/pressC：Custom 风格的用户三色（缺省字段自动推导）。
        /// </summary>
        public static HPushButtonPalette Resolve(HPushButtonStyle style, bool hover, bool pressed, bool disabled,
            bool checkedOn, HPushButtonPalette normal, HPushButtonPalette hoverC, HPushButtonPalette pressC)
        {
            if (disabled)
                return new HPushButtonPalette
                {
                    Back = Color.FromArgb(238, 240, 243),
                    Border = Color.FromArgb(220, 223, 228),
                    Fore = Color.FromArgb(160, 166, 175)
                };

            if (style == HPushButtonStyle.Custom)
                return ResolveCustom(hover, pressed, checkedOn, normal, hoverC, pressC);

            int hue = HueIndex(style);
            Color baseC = BaseColors[hue];
            var mode = Mode(style);
            Color fore = ContrastText(baseC);

            switch (mode)
            {
                case HPushButtonMode.Link:
                    return new HPushButtonPalette
                    {
                        Back = Color.Empty,
                        Border = Color.Empty,
                        Fore = pressed || checkedOn ? Shade(baseC, 0.18f) : hover ? Shade(baseC, 0.08f) : baseC
                    };
                case HPushButtonMode.Outline:
                {
                    Color edge = pressed || checkedOn ? Shade(baseC, 0.15f) : hover ? Shade(baseC, 0.06f) : baseC;
                    int alpha = pressed ? 46 : checkedOn ? 40 : hover ? 26 : 0;
                    return new HPushButtonPalette
                    {
                        Back = alpha > 0 ? Color.FromArgb(alpha, baseC) : Color.Empty,
                        Border = edge,
                        Fore = edge
                    };
                }
                default: // Fill / ThreeD / Gradient 同基准，渐变底色由绘制层再取 GradientEnd
                {
                    Color back = baseC;
                    if (pressed || checkedOn) back = Lum(baseC, -0.12f);
                    else if (hover) back = Lum(baseC, baseC.GetBrightness() > 0.6f ? -0.06f : 0.10f);
                    return new HPushButtonPalette
                    {
                        Back = back,
                        Border = hue == 6 ? Color.FromArgb(209, 213, 219) : Shade(back, 0.10f),
                        Fore = ContrastText(back)
                    };
                }
            }
        }

        /// <summary>渐变风格的下端色（以上端色压暗得到，按下/悬停已体现在上端色中）。</summary>
        public static Color GradientEnd(Color top) => Shade(top, 0.12f);

        /// <summary>Custom 风格：以 Normal 三色为基准，Hover/Pressed 可显式覆盖，缺省自动推导。</summary>
        public static HPushButtonPalette ResolveCustom(bool hover, bool pressed, bool checkedOn,
            HPushButtonPalette n, HPushButtonPalette h, HPushButtonPalette p)
        {
            Color back = n.Back == Color.Empty ? Color.FromArgb(243, 244, 246) : n.Back;
            Color autoHover = Lum(back, back.GetBrightness() > 0.7f ? -0.05f : 0.08f);
            Color autoPress = Lum(back, -0.10f);
            Color foreBase = n.Fore == Color.Empty ? ContrastText(back) : n.Fore;

            if (pressed || checkedOn)
                return Merge(p, autoPress, back, foreBase);
            if (hover)
                return Merge(h, autoHover, back, foreBase);
            return new HPushButtonPalette { Back = back, Border = n.Border == Color.Empty ? back : n.Border, Fore = foreBase };
        }

        /// <summary>显式色板覆盖自动色板，空字段回退到推导值。</summary>
        private static HPushButtonPalette Merge(HPushButtonPalette user, Color autoBack, Color normalBack, Color fore)
        {
            return new HPushButtonPalette
            {
                Back = user.Back == Color.Empty ? autoBack : user.Back,
                Border = user.Border == Color.Empty ? (normalBack == Color.Empty ? autoBack : normalBack) : user.Border,
                Fore = user.Fore == Color.Empty ? fore : user.Fore
            };
        }

        /// <summary>风格 → 语义色下标 0~29（按枚举值所在百段减去段基值）；Custom/Link 回退 0。</summary>
        private static int HueIndex(HPushButtonStyle style)
        {
            int v = (int)style;
            if (v >= 100 && v <= 129) return v - 100;
            if (v >= 200 && v <= 229) return v - 200;
            if (v >= 300 && v <= 329) return v - 300;
            if (v >= 400 && v <= 429) return v - 400;
            return 0;
        }

        /// <summary>按感知亮度取黑/白文字（与 HDTColorSets 同规则）。</summary>
        public static Color ContrastText(Color c)
        {
            int lum = (c.R * 299 + c.G * 587 + c.B * 114) / 1000;
            return lum >= 160 ? Color.FromArgb(30, 30, 30) : Color.White;
        }

        /// <summary>向黑色混合（f：0~1）。</summary>
        public static Color Shade(Color c, float f) => Mix(c, Color.Black, f);

        /// <summary>亮度微调：f 为正向白、负向黑。</summary>
        public static Color Lum(Color c, float f) => f >= 0 ? Mix(c, Color.White, f) : Mix(c, Color.Black, -f);

        /// <summary>两色线性插值。</summary>
        public static Color Mix(Color a, Color b, float t)
        {
            if (t <= 0) return a;
            if (t >= 1) return b;
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
    }
}
