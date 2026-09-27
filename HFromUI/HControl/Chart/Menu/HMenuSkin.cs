namespace HFromUI.HControl.Chart.Menu
{
    using HFromUI.HLangage;
    /// <summary>
    /// 菜单几何样式（20 种，只决定形状/结构/画法，颜色由 HMenuScheme 提供，二者自由组合）。
    /// </summary>
    public enum HMenuSkin
    {
        /// <summary>标准：直角浅灰悬停 + 细边框（Win 原生感）。</summary>
        Standard,
        /// <summary>圆角卡片：浅蓝圆角悬停块（默认外观）。</summary>
        Rounded,
        /// <summary>扁平：直角纯色淡块。</summary>
        Flat,
        /// <summary>渐变：圆角线性渐变悬停。</summary>
        Gradient,
        /// <summary>左强调条：悬停左侧竖色条 + 淡底。</summary>
        LeftBar,
        /// <summary>胶囊：全圆角药丸悬停。</summary>
        Pill,
        /// <summary>描边卡片：悬停只画强调色描边。</summary>
        Outline,
        /// <summary>磁贴：大间距实底色块 + 白字，项加高。</summary>
        Tile,
        /// <summary>极简：无图标栏无边框，悬停下划线。</summary>
        Minimal,
        /// <summary>下划线：通栏底部强调条。</summary>
        Underline,
        /// <summary>分段：卡片间距 + 圆角分段块。</summary>
        Segmented,
        /// <summary>标题栏：顶部强调色横带 + 强调色图标栏。</summary>
        HeaderBar,
        /// <summary>Win11：大圆角云母白 + 柔灰悬停。</summary>
        Win11,
        /// <summary>Aero 玻璃：白色纵向高光渐变。</summary>
        Aero,
        /// <summary>霓虹：近黑底 + 高饱和发光描边悬停。</summary>
        Neon,
        /// <summary>像素复古：直角硬边 + 上下斜面。</summary>
        Retro,
        /// <summary>拟物：渐变凸起悬停 + 高光/暗边。</summary>
        Skeuomorph,
        /// <summary>新拟态：同色柔光凹凸。</summary>
        Neumorph,
        /// <summary>macOS 悬浮：小圆角卡片 + 半透明灰悬停。</summary>
        MacFloat,
        /// <summary>命令面板：窄内缩圆角悬停、无图标栏、紧凑。</summary>
        Command
    }

    /// <summary>悬停块画法。</summary>
    public enum HMenuHover
    {
        /// <summary>纯色块。</summary>
        Solid,
        /// <summary>纵向渐变块。</summary>
        Gradient,
        /// <summary>左侧强调竖条 + 淡底。</summary>
        LeftBar,
        /// <summary>透明底 + 强调色描边。</summary>
        Outline,
        /// <summary>全圆角药丸。</summary>
        Pill,
        /// <summary>底部下划线（极简）。</summary>
        Underline,
        /// <summary>强调色实底 + 白字。</summary>
        Tile,
        /// <summary>顶亮底暗斜面凸起。</summary>
        Bevel,
        /// <summary>同色柔光（亮上沿 + 暗下沿）。</summary>
        Neumorph,
        /// <summary>无悬停绘制。</summary>
        None
    }

    /// <summary>下拉菜单整体底画法。</summary>
    public enum HMenuDrop
    {
        /// <summary>纯平底色。</summary>
        Solid,
        /// <summary>纵向渐变底色。</summary>
        Gradient,
        /// <summary>玻璃高光（白顶渐亮）。</summary>
        Glass,
        /// <summary>新拟态柔光底。</summary>
        Neumorph,
        /// <summary>近黑底（霓虹）。</summary>
        Glow,
        /// <summary>顶部强调色标题横带。</summary>
        HeaderBar
    }

    /// <summary>勾选记号画法。</summary>
    public enum HMenuCheck
    {
        /// <summary>右侧 ✓。</summary>
        Check,
        /// <summary>右侧实心圆点。</summary>
        Dot,
        /// <summary>不画记号。</summary>
        None
    }

    /// <summary>分隔线画法。</summary>
    public enum HMenuSep
    {
        /// <summary>普通单线。</summary>
        Line,
        /// <summary>左右内缩线。</summary>
        Inset,
        /// <summary>立体渐变线（高光+暗影）。</summary>
        Gradient,
        /// <summary>不画分隔线。</summary>
        None
    }

    /// <summary>
    /// 菜单几何样式参数：渲染器据这些参数决定圆角、悬停/底色画法、勾选记号、
    /// 边框、图标栏、卡片间距、项高等，实现 20 种外观差异。
    /// </summary>
    public sealed class HMenuSkinStyle
    {
        /// <summary>显示名称（中文）。</summary>
        public string Name;
        /// <summary>悬停块/菜单圆角半径。</summary>
        public int Radius;
        /// <summary>悬停块画法。</summary>
        public HMenuHover Hover;
        /// <summary>菜单整体底色画法。</summary>
        public HMenuDrop Drop;
        /// <summary>勾选记号画法。</summary>
        public HMenuCheck Check;
        /// <summary>分隔线画法。</summary>
        public HMenuSep Separator;
        /// <summary>是否绘制外边框。</summary>
        public bool Border = true;
        /// <summary>项是否带卡片式外间距（菜单更通透）。</summary>
        public bool CardMargin;
        /// <summary>是否显示左侧图标栏。</summary>
        public bool ShowImageMargin = true;
        /// <summary>悬停块是否左右大内缩（命令面板）。</summary>
        public bool NarrowHover;
        /// <summary>菜单项是否加高（磁贴）。</summary>
        public bool TallItems;
    }

    /// <summary>20 种几何样式预设提供者。</summary>
    public static class HMenuSkins
    {
        /// <summary>按枚举返回对应几何样式参数。</summary>
        public static HMenuSkinStyle Get(HMenuSkin skin)
        {
            switch (skin)
            {
                case HMenuSkin.Standard:
                    return S(HTranslation.GetContent("标准"), 0, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line);
                case HMenuSkin.Rounded:
                    return S(HTranslation.GetContent("圆角卡片"), 5, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Flat:
                    return S(HTranslation.GetContent("扁平"), 0, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line);
                case HMenuSkin.Gradient:
                    return S(HTranslation.GetContent("渐变"), 3, HMenuHover.Gradient, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line);
                case HMenuSkin.LeftBar:
                    return S(HTranslation.GetContent("左强调条"), 3, HMenuHover.LeftBar, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Pill:
                    return S(HTranslation.GetContent("胶囊"), 9, HMenuHover.Pill, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Outline:
                    return S(HTranslation.GetContent("描边卡片"), 5, HMenuHover.Outline, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Tile:
                    return S(HTranslation.GetContent("磁贴"), 2, HMenuHover.Tile, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Inset,
                        card: true, tall: true);
                case HMenuSkin.Minimal:
                    return S(HTranslation.GetContent("极简"), 0, HMenuHover.Underline, HMenuDrop.Solid, HMenuCheck.Dot, HMenuSep.Inset,
                        border: false, imageMargin: false);
                case HMenuSkin.Underline:
                    return S(HTranslation.GetContent("下划线"), 0, HMenuHover.Underline, HMenuDrop.Solid, HMenuCheck.Dot, HMenuSep.Line);
                case HMenuSkin.Segmented:
                    return S(HTranslation.GetContent("分段"), 4, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Inset, card: true);
                case HMenuSkin.HeaderBar:
                    return S(HTranslation.GetContent("标题栏"), 3, HMenuHover.Solid, HMenuDrop.HeaderBar, HMenuCheck.Check, HMenuSep.Line);
                case HMenuSkin.Win11:
                    return S("Win11", 8, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Aero:
                    return S(HTranslation.GetContent("Aero 玻璃"), 6, HMenuHover.Gradient, HMenuDrop.Glass, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Neon:
                    return S(HTranslation.GetContent("霓虹"), 4, HMenuHover.Outline, HMenuDrop.Glow, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Retro:
                    return S(HTranslation.GetContent("像素复古"), 0, HMenuHover.Bevel, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Gradient);
                case HMenuSkin.Skeuomorph:
                    return S(HTranslation.GetContent("拟物凸起"), 4, HMenuHover.Bevel, HMenuDrop.Gradient, HMenuCheck.Check, HMenuSep.Gradient);
                case HMenuSkin.Neumorph:
                    return S(HTranslation.GetContent("新拟态"), 8, HMenuHover.Neumorph, HMenuDrop.Neumorph, HMenuCheck.Check, HMenuSep.None, card: true);
                case HMenuSkin.MacFloat:
                    return S(HTranslation.GetContent("macOS 悬浮"), 6, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
                case HMenuSkin.Command:
                    return S(HTranslation.GetContent("命令面板"), 3, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Dot, HMenuSep.Inset,
                        imageMargin: false, narrow: true);
            }
            return S(HTranslation.GetContent("圆角卡片"), 5, HMenuHover.Solid, HMenuDrop.Solid, HMenuCheck.Check, HMenuSep.Line, card: true);
        }

        /// <summary>样式参数构造简写（默认：有边框、有图标栏、无卡片间距、标准项高）。</summary>
        private static HMenuSkinStyle S(string name, int radius, HMenuHover hover, HMenuDrop drop,
            HMenuCheck check, HMenuSep sep, bool border = true, bool card = false,
            bool imageMargin = true, bool narrow = false, bool tall = false)
        {
            return new HMenuSkinStyle
            {
                Name = name,
                Radius = radius,
                Hover = hover,
                Drop = drop,
                Check = check,
                Separator = sep,
                Border = border,
                CardMargin = card,
                ShowImageMargin = imageMargin,
                NarrowHover = narrow,
                TallItems = tall
            };
        }
    }
}
