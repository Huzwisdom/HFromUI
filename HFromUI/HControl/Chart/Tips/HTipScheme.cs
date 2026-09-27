using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart.Tips
{
    using HFromUI.HLangage;
    /// <summary>悬停气泡配色风格（12 种：自动反色 + 11 种固定配色，只决定颜色，与 HTipSkin 气泡外形自由组合）。</summary>
    public enum HTipSchemeKind
    {
        /// <summary>自动反色（默认，与 HChart 提取前像素一致：浅底主题深泡冷蓝边，深底主题浅泡冷蓝灰边）。</summary>
        Auto = 0,
        /// <summary>深海蓝（深底亮字冷蓝描边）。</summary>
        DeepSea,
        /// <summary>暗夜墨（近黑底中性灰边）。</summary>
        Night,
        /// <summary>米纸暖白（纸张感浅底棕边）。</summary>
        Paper,
        /// <summary>森林绿（深绿底嫩绿字）。</summary>
        Forest,
        /// <summary>暮光橙（暖白底橙边）。</summary>
        Sunset,
        /// <summary>葡萄紫（暗紫底浅紫字）。</summary>
        Grape,
        /// <summary>樱粉（暖白粉底玫红边）。</summary>
        Cherry,
        /// <summary>鎏金暗棕（深棕底金字）。</summary>
        Gold,
        /// <summary>极简黑白（近白底纯黑边）。</summary>
        Mono,
        /// <summary>晴海冰蓝（浅蓝底深蓝边）。</summary>
        Aqua,
        /// <summary>街机翠绿（游戏风深绿底荧光描边，参考绿色标题牌）。</summary>
        Arcade
    }

    /// <summary>气泡配色板：背景/边框/正文/强调/装饰色 + 深底标记（系列色自适应用）。</summary>
    public struct HTipColors
    {
        /// <summary>气泡填充色（已含 alpha）。</summary>
        public Color Bg;
        /// <summary>气泡外边框色。</summary>
        public Color Border;
        /// <summary>正文字颜色。</summary>
        public Color Text;
        /// <summary>首行/X 值强调文字色。</summary>
        public Color TagText;
        /// <summary>装饰绘图（HTipArt）颜色。</summary>
        public Color Accent;
        /// <summary>true=深底色气泡（系列图例色需调亮）；false=浅底色气泡（系列色需压暗）。</summary>
        public bool DarkPopup;
    }

    /// <summary>气泡配色提供者：按枚举返回 12 种预置配色；Auto 随图表主题深浅取反色（复刻提取前 BuildTipScheme）。</summary>
    public static class HTipPalettes
    {
        /// <summary>固定配色缓存。</summary>
        private static readonly Dictionary<HTipSchemeKind, HTipColors> _map =
            new Dictionary<HTipSchemeKind, HTipColors>();

        /// <summary>取指定配色板；Auto 需要当前图表主题深浅（themeDark=true 表示图表为深色主题）。</summary>
        public static HTipColors Get(HTipSchemeKind kind, bool themeDark)
        {
            if (kind == HTipSchemeKind.Auto) return BuildAuto(themeDark);
            if (_map.TryGetValue(kind, out var c)) return c;
            c = BuildFixed(kind);
            _map[kind] = c;
            return c;
        }

        /// <summary>配色显示名（中文，菜单用）。</summary>
        public static string Name(HTipSchemeKind kind)
        {
            switch (kind)
            {
                case HTipSchemeKind.Auto: return HTranslation.GetContent("自动反色（默认）");
                case HTipSchemeKind.DeepSea: return HTranslation.GetContent("深海蓝");
                case HTipSchemeKind.Night: return HTranslation.GetContent("暗夜墨");
                case HTipSchemeKind.Paper: return HTranslation.GetContent("米纸暖白");
                case HTipSchemeKind.Forest: return HTranslation.GetContent("森林绿");
                case HTipSchemeKind.Sunset: return HTranslation.GetContent("暮光橙");
                case HTipSchemeKind.Grape: return HTranslation.GetContent("葡萄紫");
                case HTipSchemeKind.Cherry: return HTranslation.GetContent("樱粉");
                case HTipSchemeKind.Gold: return HTranslation.GetContent("鎏金暗棕");
                case HTipSchemeKind.Mono: return HTranslation.GetContent("极简黑白");
                case HTipSchemeKind.Aqua: return HTranslation.GetContent("晴海冰蓝");
                case HTipSchemeKind.Arcade: return HTranslation.GetContent("街机翠绿");
            }
            return kind.ToString();
        }

        /// <summary>自动反色：与 HChart.BuildTipScheme 完全一致的冷蓝微调反色板。</summary>
        private static HTipColors BuildAuto(bool themeDark)
        {
            if (!themeDark)
            {
                // 浅底主题：深气泡 + 冷蓝边框/强调
                return new HTipColors
                {
                    DarkPopup = true,
                    Bg = Color.FromArgb(230, 26, 30, 38),
                    Border = Color.FromArgb(170, 200, 222, 245),
                    Text = Color.FromArgb(246, 247, 249),
                    TagText = Color.FromArgb(135, 198, 255),
                    Accent = Color.FromArgb(135, 198, 255),
                };
            }
            // 深底主题：浅气泡 + 冷蓝灰边框/强调
            return new HTipColors
            {
                DarkPopup = false,
                Bg = Color.FromArgb(230, 255, 253, 247),
                Border = Color.FromArgb(220, 150, 172, 205),
                Text = Color.FromArgb(35, 35, 40),
                TagText = Color.FromArgb(38, 92, 175),
                Accent = Color.FromArgb(38, 92, 175),
            };
        }

        /// <summary>11 种固定配色（不随主题变；DarkPopup 决定系列图例色压暗/调亮）。</summary>
        private static HTipColors BuildFixed(HTipSchemeKind kind)
        {
            switch (kind)
            {
                case HTipSchemeKind.DeepSea:
                    return Mk(true, C(235, 12, 38, 66), C(170, 90, 180, 230), C(235, 245, 255), C(120, 205, 255), C(90, 170, 240));
                case HTipSchemeKind.Night:
                    return Mk(true, C(235, 22, 22, 26), C(200, 95, 95, 105), C(230, 230, 235), C(175, 175, 195), C(130, 130, 150));
                case HTipSchemeKind.Paper:
                    return Mk(false, C(245, 253, 248, 235), C(220, 150, 120, 80), C(60, 50, 40), C(150, 90, 30), C(200, 150, 90));
                case HTipSchemeKind.Forest:
                    return Mk(true, C(235, 20, 58, 32), C(180, 120, 200, 130), C(235, 250, 238), C(140, 230, 160), C(70, 170, 90));
                case HTipSchemeKind.Sunset:
                    return Mk(false, C(240, 255, 240, 224), C(220, 230, 120, 50), C(80, 45, 25), C(210, 90, 30), C(235, 140, 60));
                case HTipSchemeKind.Grape:
                    return Mk(true, C(235, 52, 34, 74), C(180, 190, 140, 230), C(245, 240, 250), C(200, 160, 255), C(140, 90, 220));
                case HTipSchemeKind.Cherry:
                    return Mk(false, C(240, 255, 238, 245), C(220, 230, 120, 170), C(90, 40, 60), C(210, 70, 130), C(235, 130, 180));
                case HTipSchemeKind.Gold:
                    return Mk(true, C(238, 38, 32, 18), C(200, 220, 175, 90), C(245, 235, 210), C(255, 210, 120), C(230, 180, 80));
                case HTipSchemeKind.Mono:
                    return Mk(false, C(240, 250, 250, 250), C(255, 40, 40, 40), C(25, 25, 25), C(0, 0, 0), C(90, 90, 90));
                case HTipSchemeKind.Aqua:
                    return Mk(false, C(240, 232, 246, 252), C(220, 70, 140, 190), C(20, 55, 80), C(20, 110, 170), C(70, 160, 220));
                case HTipSchemeKind.Arcade:
                    return Mk(true, C(240, 0, 168, 40), C(235, 214, 255, 214), C(245, 255, 240), C(200, 255, 170), C(120, 255, 80));
            }
            return BuildAuto(false);
        }

        /// <summary>构造不透明色（字段简写）。</summary>
        private static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);

        /// <summary>构造带透明度颜色（字段简写）。</summary>
        private static Color C(int a, int r, int g, int b) => Color.FromArgb(a, r, g, b);

        /// <summary>构造配色板（字段简写）。</summary>
        private static HTipColors Mk(bool dark, Color bg, Color border, Color text, Color tag, Color accent)
            => new HTipColors { DarkPopup = dark, Bg = bg, Border = border, Text = text, TagText = tag, Accent = accent };
    }
}
