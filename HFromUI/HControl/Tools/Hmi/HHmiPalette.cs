using System;
using System.Drawing;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>
    /// HMI 仪表/信号灯族控件的统一配色方案：表盘面、外圈金属边、刻度与文字、
    /// 主题强调色（弧段/填充）、指针色、报警色与半透明辉光。
    /// Classic 为工业经典白盘红针，1..12 与电机/电池/阀门共用 <see cref="HToolTheme"/> 色序。
    /// </summary>
    public struct HHmiScheme
    {
        /// <summary>表盘/灯面主底色。</summary>
        public Color Face;
        /// <summary>表盘边缘渐暗色（径向渐变外圈）。</summary>
        public Color FaceDark;
        /// <summary>外圈金属环亮面。</summary>
        public Color Bezel;
        /// <summary>外圈金属环暗面。</summary>
        public Color BezelDark;
        /// <summary>刻度线颜色。</summary>
        public Color Scale;
        /// <summary>刻度数字/正文颜色。</summary>
        public Color Text;
        /// <summary>主题强调色（彩色弧段、液柱、发光）。</summary>
        public Color Accent;
        /// <summary>强调色加深（指针、暗面）。</summary>
        public Color AccentDark;
        /// <summary>指针色。</summary>
        public Color Needle;
        /// <summary>报警区段/报警灯色。</summary>
        public Color Alarm;
        /// <summary>半透明辉光色。</summary>
        public Color Glow;
    }

    /// <summary>
    /// 13 种色调的仪表方案表。外形由各控件的 Style 枚举决定，色调只替换颜色体系，
    /// 同一 Style 在不同 Theme 下明暗结构保持一致。
    /// </summary>
    public static class HHmiPalettes
    {
        /// <summary>工业经典：白盘、石墨外圈、蓝弧、红针、橙红报警。</summary>
        public static readonly HHmiScheme Classic = new HHmiScheme
        {
            Face = Color.FromArgb(250, 251, 253),
            FaceDark = Color.FromArgb(218, 223, 230),
            Bezel = Color.FromArgb(78, 84, 92),
            BezelDark = Color.FromArgb(32, 36, 42),
            Scale = Color.FromArgb(66, 73, 82),
            Text = Color.FromArgb(28, 32, 38),
            Accent = Color.FromArgb(46, 134, 224),
            AccentDark = Color.FromArgb(24, 88, 168),
            Needle = Color.FromArgb(222, 62, 52),
            Alarm = Color.FromArgb(238, 74, 46),
            Glow = Color.FromArgb(70, 46, 134, 224)
        };

        /// <summary>按 <see cref="HToolTheme"/> 取方案；Classic 返回经典工业配色。</summary>
        public static HHmiScheme Get(HToolTheme theme)
        {
            if (theme == HToolTheme.Classic) return Classic;
            HToolPalette p = HToolPalettes.Get((int)theme);
            return new HHmiScheme
            {
                Face = Color.FromArgb(250, 251, 253),
                FaceDark = HToolPalettes.Mix(p.Main, Color.White, 0.80f),
                Bezel = HToolPalettes.Shade(p.Main, 0.42f),
                BezelDark = HToolPalettes.Shade(p.Main, 0.66f),
                Scale = HToolPalettes.Shade(p.Main, 0.58f),
                Text = Color.FromArgb(28, 32, 38),
                Accent = p.Main,
                AccentDark = p.Dark,
                Needle = HToolPalettes.Shade(p.Main, 0.28f),
                Alarm = Color.FromArgb(238, 74, 46),
                Glow = Color.FromArgb(70, p.Main.R, p.Main.G, p.Main.B)
            };
        }

        /// <summary>深盘方案（夜视/液晶/赛博等深色外形共用）：按主题强调色换发光体系。</summary>
        public static HHmiScheme Dark(HToolTheme theme)
        {
            Color acc = theme == HToolTheme.Classic ? Color.FromArgb(64, 224, 196) : HToolPalettes.Get((int)theme).Main;
            Color dark = HToolPalettes.Shade(acc, 0.35f);
            return new HHmiScheme
            {
                Face = Color.FromArgb(24, 28, 34),
                FaceDark = Color.FromArgb(12, 14, 18),
                Bezel = Color.FromArgb(58, 64, 72),
                BezelDark = Color.FromArgb(14, 17, 22),
                Scale = Color.FromArgb(150, 158, 168),
                Text = Color.FromArgb(228, 234, 242),
                Accent = acc,
                AccentDark = dark,
                Needle = acc,
                Alarm = Color.FromArgb(255, 96, 64),
                Glow = Color.FromArgb(90, acc.R, acc.G, acc.B)
            };
        }

        /// <summary>两色按 t（0=base，1=other）线性混合（转发 HToolPalettes.Mix）。</summary>
        public static Color Mix(Color a, Color b, float t) => HToolPalettes.Mix(a, b, t);
        /// <summary>提亮（与白色混合）。</summary>
        public static Color Tint(Color c, float t) => HToolPalettes.Tint(c, t);
        /// <summary>加深（与黑色混合）。</summary>
        public static Color Shade(Color c, float t) => HToolPalettes.Shade(c, t);
    }
}
