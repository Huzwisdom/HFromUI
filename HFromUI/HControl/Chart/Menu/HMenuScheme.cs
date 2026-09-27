using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart.Menu
{
    using HFromUI.HLangage;
    /// <summary>菜单配色风格（12 种，只决定颜色，与 HMenuSkin 几何样式自由组合）。</summary>
    public enum HMenuScheme
    {
        /// <summary>素雅白（默认，浅灰蓝悬停）。</summary>
        Light,
        /// <summary>深邃黑（深色仪表）。</summary>
        Dark,
        /// <summary>科技蓝（深蓝底亮蓝悬停）。</summary>
        Blue,
        /// <summary>魅紫（暗紫底）。</summary>
        Violet,
        /// <summary>夕橙暖白。</summary>
        Sunset,
        /// <summary>护眼米黄。</summary>
        Sand,
        /// <summary>矩阵荧光（纯黑荧光绿）。</summary>
        Matrix,
        /// <summary>米纸墨蓝。</summary>
        Paper,
        /// <summary>晴海冰蓝。</summary>
        Aqua,
        /// <summary>草绿清新。</summary>
        Grass,
        /// <summary>沙漠暖棕。</summary>
        Desert,
        /// <summary>暗夜绯红。</summary>
        Crimson
    }

    /// <summary>
    /// 菜单配色板：渲染器所需的全部颜色。
    /// 浅底风格与深底风格各字段语义相同，图标墨色/强调色随板切换，深底下 glyph 自动变浅色。
    /// </summary>
    public sealed class HMenuPalette
    {
        /// <summary>显示名称（中文）。</summary>
        public string Name;
        /// <summary>下拉菜单整体底色。</summary>
        public Color Back;
        /// <summary>左侧图标栏底色。</summary>
        public Color Margin;
        /// <summary>菜单外边框。</summary>
        public Color Border;
        /// <summary>菜单项正文色。</summary>
        public Color Text;
        /// <summary>禁用项文字色。</summary>
        public Color TextDisabled;
        /// <summary>悬停块起始色（渐变上/纯色块）。</summary>
        public Color HoverStart;
        /// <summary>悬停块结束色（渐变下；纯色块时两色相同）。</summary>
        public Color HoverEnd;
        /// <summary>勾选项常驻底色。</summary>
        public Color Checked;
        /// <summary>强调色（勾选符/箭头/左竖条/描边悬停等）。</summary>
        public Color Accent;
        /// <summary>分隔线色。</summary>
        public Color Separator;
        /// <summary>图标墨色（线条主色）。</summary>
        public Color GlyphInk;
        /// <summary>图标强调色。</summary>
        public Color GlyphAccent;
        /// <summary>实底悬停块（磁贴样式）上的文字色。</summary>
        public Color HoverText;
        /// <summary>拟物/玻璃顶高光色。</summary>
        public Color TopLight;
        /// <summary>拟物/玻璃底暗影色。</summary>
        public Color EdgeDark;
    }

    /// <summary>配色板提供者：按枚举返回 12 种预置颜色方案。</summary>
    public static class HMenuPalettes
    {
        /// <summary>缓存已构造的配色板。</summary>
        private static readonly Dictionary<HMenuScheme, HMenuPalette> _map =
            new Dictionary<HMenuScheme, HMenuPalette>();

        /// <summary>获取指定风格的配色板。</summary>
        public static HMenuPalette Get(HMenuScheme scheme)
        {
            if (!_map.TryGetValue(scheme, out var p))
            {
                p = Build(scheme);
                _map[scheme] = p;
            }
            return p;
        }

        /// <summary>构造一块配色板（统一白色悬停文字，仅磁贴等实底悬停使用）。</summary>
        private static HMenuPalette Mk(string name, int back, int margin, int border, int text, int disabled,
            int hover, int hoverEnd, int checkBg, int accent, int separ, int ink, int topLight, int edgeDark)
        {
            return new HMenuPalette
            {
                Name = name,
                Back = Op(back),
                Margin = Op(margin),
                Border = Op(border),
                Text = Op(text),
                TextDisabled = Op(disabled),
                HoverStart = Op(hover),
                HoverEnd = Op(hoverEnd),
                Checked = Op(checkBg),
                Accent = Op(accent),
                Separator = Op(separ),
                GlyphInk = Op(ink),
                GlyphAccent = Op(accent),
                HoverText = Color.White,
                TopLight = Op(topLight),
                EdgeDark = Op(edgeDark)
            };
        }

        /// <summary>由 0xRRGGBB 构造不透明色（Color.FromArgb(int) 会把高位 0 当作 alpha=0 全透明，必须补满 255）。</summary>
        private static Color Op(int rgb) => Color.FromArgb(255, Color.FromArgb(rgb));

        /// <summary>按风格枚举给出具体 ARGB（均为不透明色，深/浅底均可清晰阅读）。</summary>
        private static HMenuPalette Build(HMenuScheme s)
        {
            switch (s)
            {
                case HMenuScheme.Light:
                    return Mk(HTranslation.GetContent("素雅白"), 0xFFFFFF, 0xF8F9FB, 0xCDD0D6, 0x343A44, 0xA0A4AC,
                        0xE8EEF7, 0xE8EEF7, 0xF0F4FA, 0x406EBE, 0xE1E4E8, 0x48505C, 0xFFFFFF, 0xC3C7CF);
                case HMenuScheme.Dark:
                    return Mk(HTranslation.GetContent("深邃黑"), 0x282A2F, 0x222428, 0x14161A, 0xE4E6EA, 0x787C84,
                        0x3A404C, 0x3A404C, 0x343944, 0x609BEB, 0x40444C, 0xCED2DA, 0x464A54, 0x121418);
                case HMenuScheme.Blue:
                    return Mk(HTranslation.GetContent("科技蓝"), 0x17305C, 0x122850, 0x0C1E40, 0xFFFFFF, 0x8CA0BE,
                        0x2E66B2, 0x24549C, 0x285AA0, 0x5AB4FF, 0x466496, 0xE1EEFA, 0x5082C8, 0x0A1C3C);
                case HMenuScheme.Violet:
                    return Mk(HTranslation.GetContent("魅紫"), 0x302648, 0x281F3E, 0x1A142C, 0xEEE8F8, 0x968CAF,
                        0x5C448C, 0x4E387A, 0x503A80, 0xBA86FC, 0x52466E, 0xE2D8F6, 0x6A5098, 0x140E24);
                case HMenuScheme.Sunset:
                    return Mk(HTranslation.GetContent("夕橙暖白"), 0xFFF8F0, 0xFCF0E2, 0xE4C8AF, 0x783712, 0xBEA58C,
                        0xFAE4CD, 0xF7D8BA, 0xF8ECDC, 0xD6702C, 0xEEDAC4, 0x965528, 0xFFFFFF, 0xD8B492);
                case HMenuScheme.Sand:
                    return Mk(HTranslation.GetContent("护眼米黄"), 0xFAF4E4, 0xF4EBD6, 0xD6C8A8, 0x544830, 0xAFA587,
                        0xEEE4C4, 0xEAE0BC, 0xF2EACE, 0xA88440, 0xE2D6B9, 0x6E5F42, 0xFFFCF2, 0xCBBB94);
                case HMenuScheme.Matrix:
                    return Mk(HTranslation.GetContent("矩阵荧光"), 0x040805, 0x080E09, 0x193720, 0x78F096, 0x3C6E46,
                        0x0C2414, 0x0C2414, 0x0E2C18, 0x3CE66E, 0x18321E, 0x6EE68C, 0x1E4628, 0x020604);
                case HMenuScheme.Paper:
                    return Mk(HTranslation.GetContent("米纸墨蓝"), 0xFDF8EB, 0xF7EFDC, 0xD6CAAF, 0x2A3A60, 0x96968C,
                        0xE8EEF6, 0xE8EEF6, 0xEEF2F8, 0x3E64AA, 0xE0D5BE, 0x465578, 0xFFFCF2, 0xC8BC9E);
                case HMenuScheme.Aqua:
                    return Mk(HTranslation.GetContent("晴海冰蓝"), 0xEEF7FC, 0xE4F0F8, 0xBAD4E4, 0x143E5C, 0x82A0B2,
                        0xD2E8F6, 0xD2E8F6, 0xDCEEF8, 0x2680BA, 0xC8DEEC, 0x325F7D, 0xFFFFFF, 0xA8C8DC);
                case HMenuScheme.Grass:
                    return Mk(HTranslation.GetContent("草绿清新"), 0xECF7EC, 0xE1F2E1, 0xB4D6B4, 0x264E34, 0x82A587,
                        0xD2ECD4, 0xD2ECD4, 0xDCF0DE, 0x3A8E56, 0xC6E0C8, 0x40694A, 0xFFFFFF, 0x9CC8A0);
                case HMenuScheme.Desert:
                    return Mk(HTranslation.GetContent("沙漠暖棕"), 0xF6E8CD, 0xEEDBB8, 0xCEB28A, 0x603A18, 0xA88E69,
                        0xE8D2AC, 0xE8D2AC, 0xEEDAB8, 0xB0682C, 0xDAC39E, 0x784E28, 0xFBF2E0, 0xC0A074);
                case HMenuScheme.Crimson:
                    return Mk(HTranslation.GetContent("暗夜绯红"), 0x22161A, 0x1C1216, 0x481E26, 0xF0DEE2, 0x8A6970,
                        0x602430, 0x501E28, 0x54202A, 0xEB5A6E, 0x4E2C34, 0xE4C8D0, 0x72303C, 0x100A0E);
            }
            return Get(HMenuScheme.Light);
        }
    }
}
