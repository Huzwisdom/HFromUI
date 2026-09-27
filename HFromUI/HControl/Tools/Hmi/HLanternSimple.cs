using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>简单信号灯外形样式，共 22 种。</summary>
    public enum HLanternSimpleStyle
    {
        Classic = 0,        // 经典玻璃球（径向渐变）
        Flat = 1,           // 扁平实心圆
        Ring = 2,           // 环形灯
        Square = 3,         // 圆角方灯
        LedCluster = 4,     // LED 点阵
        Neon = 5,           // 霓虹光晕
        MetalBezel = 6,     // 金属螺纹圈
        Dot = 7,            // 迷你圆点
        Pill = 8,           // 胶囊灯
        Hexagon = 9,        // 六边形
        Diamond = 10,       // 菱形
        Sunray = 11,        // 光芒灯（光芒旋转）
        Glass = 12,         // 高光玻璃
        InsetDark = 13,     // 深色嵌入
        Cyber = 14,         // 赛博朋克
        NightVision = 15,   // 夜视绿
        Amber = 16,         // 琥珀
        Emerald = 17,       // 翡翠
        RedAlert = 18,      // 报警红
        PurpleHolo = 19,    // 紫色全息
        Pulse = 20,         // 脉冲呼吸灯
        Bracket = 21        // 带安装支架
    }

    /// <summary>
    /// 简单信号灯控件：22 种外形 × 13 种色调，径向玻璃球/点阵/光环/多角形等，
    /// 点亮时带辉光，支持闪烁、呼吸脉冲与旋转光芒动画。
    /// </summary>
    [DefaultProperty("Lit")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("简单信号灯控件：22 种外形、13 种色调，支持闪烁与光芒动画")]
    public class HLanternSimple : HLabelBase
    {
        private readonly Timer _timer;
        private HLanternSimpleStyle _style = HLanternSimpleStyle.Classic;
        private HToolTheme _theme = HToolTheme.Classic;
        private bool _lit = true;
        private bool _blinking;
        private int _blinkInterval = 500;
        private Color _lampColor = Color.Empty;
        private int _tick;
        private bool _flashOn = true;

        /// <summary>默认方形灯。</summary>
        public HLanternSimple()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(44, 44);
            _timer = new Timer { Interval = 50 };
            _timer.Tick += Tick;
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("信号灯外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HLanternSimpleStyle.Classic)]
        public HLanternSimpleStyle LanternStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>图元色调。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>是否点亮。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("点亮"), HDescriptionLanguage("是否点亮发光"), Browsable(true)]
        [DefaultValue(true)]
        public bool Lit
        {
            get => _lit;
            set { _lit = value; Invalidate(); }
        }

        /// <summary>是否闪烁。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("闪烁"), HDescriptionLanguage("点亮状态下是否闪烁"), Browsable(true)]
        [DefaultValue(false)]
        public bool Blinking
        {
            get => _blinking;
            set { _blinking = value; Invalidate(); }
        }

        /// <summary>闪烁间隔（毫秒）。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("闪烁间隔"), HDescriptionLanguage("闪烁间隔（毫秒）"), Browsable(true)]
        [DefaultValue(500)]
        public int BlinkInterval
        {
            get => _blinkInterval;
            set => _blinkInterval = Math.Max(100, value);
        }

        /// <summary>灯光自定义颜色；Empty 使用当前色调强调色。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("灯光颜色"), HDescriptionLanguage("灯光自定义颜色，未设置使用色调强调色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color LampColor
        {
            get => _lampColor;
            set { _lampColor = value; Invalidate(); }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _timer.Start();
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && IsHandleCreated) _timer.Start();
            else _timer.Stop();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Stop(); _timer.Dispose(); }
            base.Dispose(disposing);
        }

        private void Tick(object sender, EventArgs e)
        {
            if (IsDisposed) { _timer.Stop(); return; }
            _tick++;
            int n = Math.Max(2, _blinkInterval / 50);
            bool f = (_tick % (n * 2)) < n;
            if (f != _flashOn) { _flashOn = f; Invalidate(); }
            if (_style == HLanternSimpleStyle.Sunray || _style == HLanternSimpleStyle.Pulse)
                Invalidate();
        }

        private sealed class L
        {
            public int Shape;        // 0 圆、1 方、2 胶囊、3 六边、4 菱形、5 点
            public int Bezel;        // 0 无、1 金属圈、2 螺纹、3 支架
            public bool Glow;
            public bool Cluster;
            public bool Rays;
            public bool Pulse;
            public bool Inset;
            public bool Flat;
            public bool Glass = true;
            public Color Lamp;
            public bool CustomLamp;
        }

        private static L StyleOf(HLanternSimpleStyle s)
        {
            var l = new L();
            switch (s)
            {
                case HLanternSimpleStyle.Flat: l.Flat = true; l.Glass = false; break;
                case HLanternSimpleStyle.Ring: l.Flat = true; l.Bezel = 1; l.Glass = false; break;
                case HLanternSimpleStyle.Square: l.Shape = 1; break;
                case HLanternSimpleStyle.LedCluster: l.Cluster = true; l.Bezel = 1; l.Glass = false; break;
                case HLanternSimpleStyle.Neon: l.Glow = true; l.Glass = false; break;
                case HLanternSimpleStyle.MetalBezel: l.Bezel = 2; break;
                case HLanternSimpleStyle.Dot: l.Shape = 5; l.Glass = false; break;
                case HLanternSimpleStyle.Pill: l.Shape = 2; l.Glass = false; l.Flat = true; break;
                case HLanternSimpleStyle.Hexagon: l.Shape = 3; break;
                case HLanternSimpleStyle.Diamond: l.Shape = 4; break;
                case HLanternSimpleStyle.Sunray: l.Rays = true; l.Glow = true; l.Glass = false; break;
                case HLanternSimpleStyle.Glass: l.Bezel = 1; break;
                case HLanternSimpleStyle.InsetDark: l.Inset = true; l.Bezel = 1; break;
                case HLanternSimpleStyle.Cyber:
                    l.Glow = true; l.Shape = 3; l.Inset = true;
                    l.Lamp = Color.FromArgb(0, 229, 255); l.CustomLamp = true; break;
                case HLanternSimpleStyle.NightVision:
                    l.Glow = true; l.Inset = true;
                    l.Lamp = Color.FromArgb(96, 255, 150); l.CustomLamp = true; break;
                case HLanternSimpleStyle.Amber:
                    l.Glow = true;
                    l.Lamp = Color.FromArgb(255, 176, 40); l.CustomLamp = true; break;
                case HLanternSimpleStyle.Emerald:
                    l.Glow = true;
                    l.Lamp = Color.FromArgb(64, 220, 150); l.CustomLamp = true; break;
                case HLanternSimpleStyle.RedAlert:
                    l.Glow = true;
                    l.Lamp = Color.FromArgb(238, 74, 46); l.CustomLamp = true; break;
                case HLanternSimpleStyle.PurpleHolo:
                    l.Glow = true;
                    l.Lamp = Color.FromArgb(192, 128, 255); l.CustomLamp = true; break;
                case HLanternSimpleStyle.Pulse:
                    l.Pulse = true; l.Glow = true; l.Glass = false; break;
                case HLanternSimpleStyle.Bracket:
                    l.Bezel = 3; break;
            }
            return l;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 8 || Height < 8) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            L st = StyleOf(_style);
            HHmiScheme sc = st.Inset ? HHmiPalettes.Dark(Theme) : HHmiPalettes.Get(Theme);
            bool show = _lit && (!_blinking || _flashOn);
            Color lamp = st.CustomLamp ? st.Lamp :
                (_lampColor != Color.Empty ? _lampColor :
                 (_lit ? sc.Accent : Color.FromArgb(120, 128, 136)));

            float pad = st.Bezel == 3 ? 5f : 2f;
            float cx = Width / 2f, cy = Height / 2f;
            float r = Math.Min(Width, Height) / 2f - pad;
            if (st.Shape == 5) r *= 0.55f;
            float pulseK = st.Pulse && show ? 0.82f + 0.18f * (0.5f + 0.5f * (float)Math.Sin(_tick * 0.16)) : 1f;

            // 支架
            if (st.Bezel == 3)
            {
                using (var p = new Pen(sc.Bezel, 3f))
                {
                    g.DrawLine(p, cx - r * 0.5f, Height - 2f, cx - r * 0.5f, cy + r * 0.8f);
                    g.DrawLine(p, cx + r * 0.5f, Height - 2f, cx + r * 0.5f, cy + r * 0.8f);
                }
            }

            // 辉光
            if (st.Glow && show)
                DrawGlow(g, cx, cy, r * pulseK, lamp, st.Rays);

            GraphicsPath face = FacePath(st.Shape, cx, cy, r);
            using (face)
            {
                // 金属外圈/螺纹
                if (st.Bezel == 1 || st.Bezel == 2)
                {
                    float bw = Math.Max(2.5f, r * 0.16f);
                    using (var ring = new GraphicsPath())
                    {
                        ring.AddPath(face, false);
                        var inner = FacePath(st.Shape, cx, cy, Math.Max(2f, r - bw));
                        ring.AddPath(inner, true);
                        using (var lgb = new LinearGradientBrush(
                            new RectangleF(cx - r, cy - r, r * 2f, r * 2f),
                            HHmiDraw.HMixer(sc.Bezel, Color.White, 0.3f),
                            HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.35f), 50f))
                            g.FillPath(lgb, ring);
                        if (st.Bezel == 2)
                        {
                            using (var hp = new Pen(Color.FromArgb(90, sc.BezelDark), 1f))
                                for (int i = 0; i < 24; i++)
                                {
                                    float a = i * 15f;
                                    g.DrawLine(hp, HHmiDraw.Polar(cx, cy, r - bw * 0.5f, a),
                                        HHmiDraw.Polar(cx, cy, r, a));
                                }
                        }
                    }
                }

                float fr = st.Bezel == 1 || st.Bezel == 2 ? r - Math.Max(2.5f, r * 0.16f) : r;
                GraphicsPath innerFace = FacePath(st.Shape, cx, cy, fr);
                using (innerFace)
                {
                    // 灯面
                    if (show && !st.Flat)
                        DrawRadialLamp(g, innerFace, cx, cy, fr, lamp);
                    else
                        using (var b = new SolidBrush(show ? lamp : Color.FromArgb(96, 102, 110)))
                            g.FillPath(b, innerFace);

                    // LED 点阵
                    if (st.Cluster)
                    {
                        float dr = fr * 0.13f;
                        for (int row = -2; row <= 2; row++)
                            for (int col = -2; col <= 2; col++)
                            {
                                float dx = col * dr * 2.2f, dy = row * dr * 2.2f;
                                if (dx * dx + dy * dy > (fr - dr) * (fr - dr)) continue;
                                Color dc = show ? HHmiDraw.HMixer(lamp, Color.White, 0.45f)
                                    : Color.FromArgb(70, 76, 84);
                                g.Ell(cx + dx, cy + dy, dr * 0.75f, dr * 0.75f, dc);
                            }
                    }

                    // 玻璃高光
                    if (st.Glass && show)
                        using (var b = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                            g.FillEllipse(b, cx - fr * 0.55f, cy - fr * 0.62f, fr * 0.62f, fr * 0.42f);
                    else if (st.Glass && !show)
                        using (var b = new SolidBrush(Color.FromArgb(24, 255, 255, 255)))
                            g.FillEllipse(b, cx - fr * 0.55f, cy - fr * 0.62f, fr * 0.62f, fr * 0.42f);

                    using (var p = new Pen(show ? HHmiDraw.HMixer(lamp, Color.Black, 0.35f)
                        : Color.FromArgb(90, 96, 104), st.Flat ? 1.4f : 1.2f))
                        g.DrawPath(p, innerFace);
                }
            }

            // 文本
            if (!string.IsNullOrEmpty(Text))
                using (var tf = HHmiDraw.Pf(Font.FontFamily, 10f, FontStyle.Bold))
                    g.CenterText(new RectangleF(0, Height - 14f, Width, 13f), Text, tf, ForeColor);
        }

        // 径向玻璃球灯面
        private static void DrawRadialLamp(Graphics g, GraphicsPath gp, float cx, float cy, float r, Color c)
        {
            using (var pgb = new PathGradientBrush(gp))
            {
                pgb.CenterPoint = new PointF(cx - r * 0.22f, cy - r * 0.26f);
                pgb.CenterColor = HHmiDraw.HMixer(c, Color.White, 0.62f);
                pgb.SurroundColors = new[] { HHmiDraw.HMixer(c, Color.Black, 0.18f) };
                g.FillPath(pgb, gp);
            }
        }

        // 辉光与旋转光芒
        private void DrawGlow(Graphics g, float cx, float cy, float r, Color c, bool rays)
        {
            for (int k = 4; k >= 1; k--)
                g.Ell(cx, cy, r + k * r * 0.10f, r + k * r * 0.10f,
                    Color.FromArgb(Math.Max(6, 34 - k * 7), c));
            if (rays)
            {
                float rot = _tick * 6f % 30f;
                using (var p = new Pen(Color.FromArgb(150, c), 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * 30f + rot;
                        g.DrawLine(p, HHmiDraw.Polar(cx, cy, r * 1.12f, a),
                            HHmiDraw.Polar(cx, cy, r * 1.42f, a));
                    }
            }
        }

        private static GraphicsPath FacePath(int shape, float cx, float cy, float r)
        {
            switch (shape)
            {
                case 1:
                    return HHmiDraw.RoundBox(new RectangleF(cx - r, cy - r, r * 2f, r * 2f), r * 0.28f);
                case 2:
                    return HHmiDraw.RoundBox(new RectangleF(cx - r, cy - r * 0.5f, r * 2f, r), r * 0.5f);
                case 3:
                    var hex = new PointF[6];
                    for (int i = 0; i < 6; i++) hex[i] = HHmiDraw.Polar(cx, cy, r, i * 60f - 30f);
                    var gpHex = new GraphicsPath();
                    gpHex.AddPolygon(hex);
                    return gpHex;
                case 4:
                    var gpDia = new GraphicsPath();
                    gpDia.AddPolygon(new[]
                    {
                        new PointF(cx, cy - r), new PointF(cx + r, cy),
                        new PointF(cx, cy + r), new PointF(cx - r, cy)
                    });
                    return gpDia;
                default:
                    var gp = new GraphicsPath();
                    gp.AddEllipse(cx - r, cy - r, r * 2f, r * 2f);
                    return gp;
            }
        }
    }
}
