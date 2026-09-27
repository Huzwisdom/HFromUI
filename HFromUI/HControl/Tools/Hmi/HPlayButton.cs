using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>播放按钮外形样式，共 22 种。</summary>
    public enum HPlayButtonStyle
    {
        Classic = 0,        // 经典圆环三角
        SolidCircle = 1,    // 实心圆
        RoundSquare = 2,    // 圆角方块
        Pill = 3,           // 胶囊（带文字）
        Ghost = 4,          // 幽灵描边
        Neon = 5,           // 霓虹发光
        Glass = 6,          // 玻璃质感
        Metal = 7,          // 金属按钮
        Flat = 8,           // 扁平
        Gradient = 9,       // 渐变按钮
        SoftShadow = 10,    // 软阴影
        TwoTone = 11,       // 双色拼接
        DarkPanel = 12,     // 深色面板嵌入钮
        OutlineRing = 13,   // 细环
        MiniGlyph = 14,     // 纯符号无边
        PlayCard = 15,      // 卡片式
        Cyber = 16,         // 赛博朋克
        Amber = 17,         // 琥珀色
        EmeraldGlow = 18,   // 翡翠辉光
        PurpleHolo = 19,    // 紫色全息
        Sunset = 20,        // 夕阳渐变
        Monochrome = 21     // 黑白单色
    }

    /// <summary>
    /// 播放/暂停按钮控件：22 种外形 × 13 种色调，点击切换播放状态，
    /// 支持悬停/按压反馈与自定义文字（胶囊/卡片样式文字排在符号右侧）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("PlayingChanged")]
    [HDescriptionLanguage("播放按钮控件：22 种外形、13 种色调，点击切换播放/暂停")]
    public class HPlayButton : HLabelBase
    {
        private HPlayButtonStyle _style = HPlayButtonStyle.Classic;
        private HToolTheme _theme = HToolTheme.Classic;
        private bool _playing;
        private bool _hover;
        private bool _pressed;

        /// <summary>默认方形按钮尺寸。</summary>
        public HPlayButton()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(60, 60);
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("播放按钮"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("播放按钮外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HPlayButtonStyle.Classic)]
        public HPlayButtonStyle PlayButtonStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>图元色调。</summary>
        [HCategoryLanguage("播放按钮"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调，Classic 为经典蓝边红停绿播"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>是否处于播放（暂停图标显示）状态。</summary>
        [HCategoryLanguage("播放按钮"), HDisplayNameLanguage("播放状态"), HDescriptionLanguage("播放中显示暂停图标"), Browsable(true)]
        [DefaultValue(false)]
        public bool Playing
        {
            get => _playing;
            set
            {
                if (_playing == value) return;
                _playing = value;
                Invalidate();
                PlayingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>播放状态切换事件。</summary>
        public event EventHandler PlayingChanged;

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false; Invalidate(); base.OnMouseUp(e);
        }
        protected override void OnClick(EventArgs e)
        {
            Playing = !_playing;
            base.OnClick(e);
        }

        private sealed class B
        {
            public int Shape;       // 0 圆、1 圆角方、2 胶囊
            public int Fill;        // 0 无填充、1 浅色底、2 主色实底、3 渐变、4 玻璃、5 深色
            public int Glyph;       // 0 实心三角/双杠、1 描边、2 细双杠
            public bool Glow;
            public bool Shadow;
            public bool Card;
            public Color Acc;
            public bool CustomAcc;
            public float RingW = 0.07f;
        }

        private static B StyleOf(HPlayButtonStyle s)
        {
            var b = new B();
            switch (s)
            {
                case HPlayButtonStyle.SolidCircle: b.Fill = 2; b.Glyph = 0; break;
                case HPlayButtonStyle.RoundSquare: b.Shape = 1; b.Fill = 2; break;
                case HPlayButtonStyle.Pill: b.Shape = 2; b.Fill = 2; b.Glyph = 0; break;
                case HPlayButtonStyle.Ghost: b.Fill = 0; b.Glyph = 1; b.RingW = 0.055f; break;
                case HPlayButtonStyle.Neon: b.Fill = 5; b.Glow = true; b.Glyph = 0; break;
                case HPlayButtonStyle.Glass: b.Fill = 4; b.Glyph = 0; break;
                case HPlayButtonStyle.Metal: b.Fill = 3; b.Glyph = 0;
                    b.Acc = Color.FromArgb(120, 128, 138); b.CustomAcc = true; break;
                case HPlayButtonStyle.Flat: b.Shape = 1; b.Fill = 1; b.Glyph = 1; b.RingW = 0.03f; break;
                case HPlayButtonStyle.Gradient: b.Fill = 3; b.Shape = 1; break;
                case HPlayButtonStyle.SoftShadow: b.Fill = 2; b.Shadow = true; b.Shape = 1; break;
                case HPlayButtonStyle.TwoTone: b.Fill = 2; b.Shape = 1; break;
                case HPlayButtonStyle.DarkPanel: b.Fill = 5; b.Shape = 1; b.RingW = 0.05f; break;
                case HPlayButtonStyle.OutlineRing: b.Fill = 0; b.Glyph = 0; b.RingW = 0.035f; break;
                case HPlayButtonStyle.MiniGlyph: b.Fill = -1; b.Glyph = 0; b.RingW = 0f; break;
                case HPlayButtonStyle.PlayCard: b.Shape = 2; b.Fill = 1; b.Card = true; break;
                case HPlayButtonStyle.Cyber: b.Fill = 5; b.Glow = true; b.Shape = 1;
                    b.Acc = Color.FromArgb(0, 229, 255); b.CustomAcc = true; break;
                case HPlayButtonStyle.Amber: b.Fill = 5; b.Glow = true;
                    b.Acc = Color.FromArgb(255, 176, 40); b.CustomAcc = true; break;
                case HPlayButtonStyle.EmeraldGlow: b.Fill = 5; b.Glow = true;
                    b.Acc = Color.FromArgb(64, 220, 150); b.CustomAcc = true; break;
                case HPlayButtonStyle.PurpleHolo: b.Fill = 5; b.Glow = true;
                    b.Acc = Color.FromArgb(192, 128, 255); b.CustomAcc = true; break;
                case HPlayButtonStyle.Sunset: b.Fill = 3; b.Shape = 1;
                    b.Acc = Color.FromArgb(232, 96, 70); b.CustomAcc = true; break;
                case HPlayButtonStyle.Monochrome: b.Fill = 2;
                    b.Acc = Color.FromArgb(40, 42, 46); b.CustomAcc = true; break;
                default: b.Fill = 0; b.Glyph = 0; break;
            }
            return b;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 10 || Height < 10) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            B st = StyleOf(_style);
            HHmiScheme sc = _theme == HToolTheme.Classic
                ? HHmiPalettes.Classic : HHmiPalettes.Get(_theme);
            Color acc = st.CustomAcc ? st.Acc : sc.Accent;

            bool pill = st.Shape == 2 || (!string.IsNullOrEmpty(Text) && st.Card);
            float h = Height;
            RectangleF body;
            if (st.Shape == 2 || (st.Card && !string.IsNullOrEmpty(Text)))
            {
                float bh = h * 0.62f;
                body = new RectangleF(2f, (h - bh) / 2f, Width - 4f, bh);
                if (!string.IsNullOrEmpty(Text)) body.Width = Width - 4f;
            }
            else
            {
                float d2 = Math.Min(Width, Height) - 4f;
                float dd = d2;
                body = new RectangleF((Width - dd) / 2f, (Height - dd) / 2f, dd, dd);
            }
            float scaleOff = _pressed ? 1.6f : 0f;
            body.Inflate(-scaleOff, -scaleOff);

            using (var gp = st.Shape == 0
                ? EllipsePath(body)
                : HHmiDraw.RoundBox(body, st.Shape == 1 ? body.Height * 0.22f : body.Height / 2f))
            {
                // 悬停/按压明暗
                float hi = _pressed ? -0.08f : (_hover ? 0.10f : 0f);
                Color fillC = Color.Empty;
                switch (st.Fill)
                {
                    case 1: fillC = HHmiDraw.HMixer(acc, Color.White, 0.84f); break;
                    case 2: fillC = acc; break;
                    case 3: fillC = acc; break;
                    case 4: fillC = HHmiDraw.HMixer(acc, Color.White, 0.55f); break;
                    case 5: fillC = Color.FromArgb(28, 32, 38); break;
                }
                if (hi != 0f && fillC != Color.Empty)
                    fillC = hi > 0 ? HHmiDraw.HMixer(fillC, Color.White, hi)
                        : HHmiDraw.HMixer(fillC, Color.Black, -hi);

                if (st.Shadow && fillC != Color.Empty)
                    using (var sb = new SolidBrush(Color.FromArgb(46, 0, 0, 0)))
                    {
                        var sh = body; sh.Offset(0, 2.5f);
                        using (var sgp = st.Shape == 0 ? EllipsePath(sh) : HHmiDraw.RoundBox(sh, body.Height * 0.22f))
                            g.FillPath(sb, sgp);
                    }

                if (st.Fill == 3)
                    using (var lgb = new LinearGradientBrush(body,
                        HHmiDraw.HMixer(acc, Color.White, 0.32f),
                        HHmiDraw.HMixer(acc, Color.Black, 0.18f), 60f))
                        g.FillPath(lgb, gp);
                else if (st.Fill == 4)
                {
                    using (var b = new SolidBrush(fillC)) g.FillPath(b, gp);
                    using (var b = new SolidBrush(Color.FromArgb(90, 255, 255, 255)))
                    {
                        var hl = body; hl.Height /= 2f; hl.Inflate(-body.Width * 0.12f, -body.Height * 0.18f);
                        using (var hgp = HHmiDraw.RoundBox(hl, hl.Height / 2f))
                            g.FillPath(b, hgp);
                    }
                }
                else if (fillC != Color.Empty)
                    using (var b = new SolidBrush(fillC)) g.FillPath(b, gp);

                if (st.RingW > 0f)
                {
                    Color ring = st.Fill == 5 || st.Glow ? acc :
                        (st.Fill == 2 || st.Fill == 3 || st.Fill == -1) ? acc : sc.Bezel;
                    if (_theme == HToolTheme.Classic && st.Fill == 0 && !st.Glow) ring = Color.DodgerBlue;
                    using (var p = new Pen(ring, Math.Max(1.2f, body.Height * st.RingW)))
                        g.DrawPath(p, gp);
                }

                if (st.Glow)
                {
                    Color glow = Color.FromArgb(_hover ? 130 : 80, acc.R, acc.G, acc.B);
                    using (var p = new Pen(glow, body.Height * 0.16f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawPath(p, gp);
                    // 辉光画在描边下方会覆盖填充，重画一次填充与边
                    if (fillC != Color.Empty)
                        using (var b = new SolidBrush(fillC)) g.FillPath(b, gp);
                    using (var p = new Pen(acc, Math.Max(1.2f, body.Height * st.RingW)))
                        g.DrawPath(p, gp);
                }
            }

            // 符号
            bool glyphOnAccent = st.Fill == 2 || st.Fill == 3 || st.Fill == 5 || st.Fill == 4;
            Color glyphC = glyphOnAccent ? Color.White :
                (st.Glow || st.Fill == -1 ? acc : (_theme == HToolTheme.Classic ? Color.OrangeRed : acc));
            float cx, cy, glyphR;
            if (st.Shape == 2 || st.Card) { cx = body.X + body.Height / 2f; cy = body.Y + body.Height / 2f; glyphR = body.Height * 0.40f; }
            else { cx = body.X + body.Width / 2f; cy = body.Y + body.Height / 2f; glyphR = body.Width * 0.36f; }
            DrawGlyph(g, cx, cy, glyphR, _playing, st.Glyph, glyphC);

            // 文字（胶囊/卡片在符号右侧）
            if (!string.IsNullOrEmpty(Text))
            {
                RectangleF tb;
                if (st.Shape == 2 || st.Card)
                    tb = new RectangleF(body.X + body.Height, body.Y,
                        body.Right - body.X - body.Height - 6f, body.Height);
                else
                    tb = new RectangleF(0, body.Bottom - 2f, Width, Math.Min(18f, Height - body.Bottom + 2f));
                using (var tf = HHmiDraw.Pf(Font.FontFamily, 10f, FontStyle.Bold))
                using (var b = new SolidBrush(glyphOnAccent ? Color.White : ForeColor))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(Text, tf, b, tb, sf);
            }
        }

        // 播放三角 / 暂停双杠
        private static void DrawGlyph(Graphics g, float cx, float cy, float r, bool playing, int kind, Color c)
        {
            var state = g.Save();
            g.TranslateTransform(cx, cy);
            if (playing)
            {
                float bw = r * 0.34f, gap = r * 0.18f, gh = r * 0.86f;
                var r1 = new RectangleF(-gap - bw, -gh / 2f, bw, gh);
                var r2 = new RectangleF(gap, -gh / 2f, bw, gh);
                float rad = kind == 2 ? 1f : Math.Min(bw / 2f, 3.5f);
                using (var b = new SolidBrush(c))
                {
                    using (var p1 = HHmiDraw.RoundBox(r1, rad)) g.FillPath(b, p1);
                    using (var p2 = HHmiDraw.RoundBox(r2, rad)) g.FillPath(b, p2);
                }
            }
            else
            {
                var tri = new[]
                {
                    new PointF(-r * 0.30f, -r * 0.50f), new PointF(-r * 0.30f, r * 0.50f),
                    new PointF(r * 0.42f, 0f)
                };
                if (kind == 1)
                    using (var p = new Pen(c, Math.Max(1.6f, r * 0.13f)) { LineJoin = LineJoin.Round })
                        g.DrawPolygon(p, tri);
                else
                    using (var b = new SolidBrush(c))
                        g.FillPolygon(b, tri);
            }
            g.Restore(state);
        }

        private static GraphicsPath EllipsePath(RectangleF r)
        {
            var gp = new GraphicsPath();
            gp.AddEllipse(r);
            return gp;
        }
    }
}
