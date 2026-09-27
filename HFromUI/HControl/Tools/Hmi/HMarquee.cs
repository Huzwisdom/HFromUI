using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>跑马灯文字外形与动效样式，共 22 种。</summary>
    public enum HMarqueeStyle
    {
        ScrollLeft = 0,     // 经典向左连滚
        ScrollRight = 1,    // 向右连滚
        ScrollUp = 2,       // 向上连滚
        ScrollDown = 3,     // 向下连滚
        PingPongH = 4,      // 水平碰壁往返
        PingPongV = 5,      // 垂直碰壁往返
        FadeEdges = 6,      // 两端渐隐滚动
        NewsTicker = 7,     // 新闻条（底板+分隔符+渐隐）
        Typewriter = 8,     // 打字机
        Blink = 9,          // 闪烁
        WaveChars = 10,     // 逐字波浪
        Neon = 11,          // 霓虹发光
        Lcd = 12,           // 液晶屏文字
        Cyber = 13,         // 赛博波浪
        GradientText = 14,  // 渐变文字
        TextShadow = 15,    // 3D 投影文字
        RoundedPanel = 16,  // 圆角底板
        Pill = 17,          // 胶囊底板
        Bordered = 18,      // 描边框
        VerticalRotated = 19, // 竖排旋转滚动
        TrackDot = 20,      // 前置轨道圆点
        FadeBreath = 21     // 呼吸淡入淡出
    }

    /// <summary>
    /// 滚动文字控件：22 种外形/动效 × 13 种色调，四向连滚、碰壁往返、打字机、
    /// 逐字波浪、闪烁呼吸、霓虹发光、液晶屏、新闻条等；速度与节拍可调。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("跑马灯文字控件：22 种外形动效、13 种色调")]
    public class HMarquee : HLabelBase
    {
        private readonly Timer _timer;
        private HMarqueeStyle _style = HMarqueeStyle.ScrollLeft;
        private HToolTheme _theme = HToolTheme.Classic;
        private float _moveSpeed = 2f;
        private float _x, _y;
        private int _phase;
        private int _dir = 1;
        private bool _inited;

        /// <summary>默认横条尺寸并启动节拍。</summary>
        public HMarquee()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(260, 40);
            _timer = new Timer { Interval = 50 };
            _timer.Tick += Tick;
        }

        /// <summary>外形/动效样式。</summary>
        [HCategoryLanguage("跑马灯"), HDisplayNameLanguage("样式效果"), HDescriptionLanguage("跑马灯文字外形与动效样式，共 22 种"), Browsable(true)]
        [DefaultValue(HMarqueeStyle.ScrollLeft)]
        public HMarqueeStyle MarqueeStyle
        {
            get => _style;
            set { _style = value; _inited = false; Invalidate(); }
        }

        /// <summary>图元色调。</summary>
        [HCategoryLanguage("跑马灯"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>每拍移动像素（0 静止，但波浪/闪烁等特效仍运行）。</summary>
        [HCategoryLanguage("跑马灯"), HDisplayNameLanguage("移动速度"), HDescriptionLanguage("每拍移动像素，0 为静止"), Browsable(true)]
        [DefaultValue(2f)]
        public float MoveSpeed
        {
            get => _moveSpeed;
            set { _moveSpeed = Math.Max(0f, Math.Min(60f, value)); }
        }

        /// <summary>动画节拍间隔（毫秒）。</summary>
        [HCategoryLanguage("跑马灯"), HDisplayNameLanguage("节拍间隔"), HDescriptionLanguage("动画节拍间隔（毫秒）"), Browsable(true)]
        [DefaultValue(50)]
        public int Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = Math.Max(15, value);
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
            _phase++;
            M desc = StyleOf(_style);
            if (!_inited)
            {
                _x = desc.Mode == 1 ? -TextWidth() : (desc.Mode == 4 ? 0f : 0f);
                _y = desc.Mode == 3 ? -TextHeight() : 0f;
                _dir = 1;
                _inited = true;
            }
            float sp = _moveSpeed;
            switch (desc.Mode)
            {
                case 0: _x -= sp; if (_x < -TextWidth() - 24f) _x += TextWidth() + 24f; break;
                case 1: _x += sp; if (_x > Width) _x -= TextWidth() + 24f; break;
                case 2: _y -= sp; if (_y < -TextHeight() - 16f) _y += TextHeight() + 16f; break;
                case 3: _y += sp; if (_y > Height) _y -= TextHeight() + 16f; break;
                case 4:
                    _x += sp * _dir;
                    if (_x >= Math.Max(0f, Width - TextWidth())) { _x = Math.Max(0f, Width - TextWidth()); _dir = -1; }
                    if (_x <= 0f) { _x = 0f; _dir = 1; }
                    break;
                case 5:
                    _y += sp * _dir;
                    if (_y >= Math.Max(0f, Height - TextHeight())) { _y = Math.Max(0f, Height - TextHeight()); _dir = -1; }
                    if (_y <= 0f) { _y = 0f; _dir = 1; }
                    break;
            }
            Invalidate();
        }

        private float TextWidth()
        {
            using (var g = CreateGraphics())
                return g.MeasureString(string.IsNullOrEmpty(Text) ? " " : Text, Font).Width;
        }
        private float TextHeight() => Font.Height + 4f;

        private sealed class M
        {
            public int Mode;
            public int Panel;          // 0 无、1 圆角底、2 胶囊、3 描边
            public bool Dark;
            public bool Glow;
            public bool Wave;
            public bool Gradient;
            public bool Shadow;
            public bool Fade;
            public bool Blink;
            public bool Breath;
            public bool Typewriter;
            public bool Rotated;
            public bool Dots;
            public Color Acc;
            public bool CustomAcc;
        }

        private static M StyleOf(HMarqueeStyle s)
        {
            var m = new M();
            switch (s)
            {
                case HMarqueeStyle.ScrollRight: m.Mode = 1; break;
                case HMarqueeStyle.ScrollUp: m.Mode = 2; break;
                case HMarqueeStyle.ScrollDown: m.Mode = 3; break;
                case HMarqueeStyle.PingPongH: m.Mode = 4; break;
                case HMarqueeStyle.PingPongV: m.Mode = 5; break;
                case HMarqueeStyle.FadeEdges: m.Mode = 0; m.Fade = true; break;
                case HMarqueeStyle.NewsTicker:
                    m.Mode = 0; m.Fade = true; m.Panel = 1; m.Dots = true; break;
                case HMarqueeStyle.Typewriter: m.Mode = 6; m.Typewriter = true; m.Panel = 1; m.Dark = true; break;
                case HMarqueeStyle.Blink: m.Mode = 6; m.Blink = true; break;
                case HMarqueeStyle.WaveChars: m.Mode = 6; m.Wave = true; break;
                case HMarqueeStyle.Neon: m.Mode = 0; m.Dark = true; m.Glow = true; break;
                case HMarqueeStyle.Lcd: m.Mode = 0; m.Panel = 1; m.Dark = true; break;
                case HMarqueeStyle.Cyber:
                    m.Mode = 0; m.Dark = true; m.Glow = true; m.Wave = true;
                    m.Acc = Color.FromArgb(0, 229, 255); m.CustomAcc = true; break;
                case HMarqueeStyle.GradientText: m.Mode = 0; m.Gradient = true; break;
                case HMarqueeStyle.TextShadow: m.Mode = 0; m.Shadow = true; break;
                case HMarqueeStyle.RoundedPanel: m.Mode = 0; m.Panel = 1; break;
                case HMarqueeStyle.Pill: m.Mode = 0; m.Panel = 2; break;
                case HMarqueeStyle.Bordered: m.Mode = 0; m.Panel = 3; break;
                case HMarqueeStyle.VerticalRotated: m.Mode = 0; m.Rotated = true; break;
                case HMarqueeStyle.TrackDot: m.Mode = 0; m.Dots = true; break;
                case HMarqueeStyle.FadeBreath: m.Mode = 6; m.Breath = true; break;
            }
            return m;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            if (string.IsNullOrEmpty(Text)) return;

            M st = StyleOf(_style);
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : Scheme0();
            Color fore = st.CustomAcc ? st.Acc : (st.Dark ? (st.Panel == 1 ? sc.Accent : sc.Text) : ForeColor);
            if (st.Panel == 1 && _style == HMarqueeStyle.Lcd) fore = sc.Accent;

            if (st.Rotated)
            {
                var state = g.Save();
                g.TranslateTransform(Width, 0f);
                g.RotateTransform(90f);
                DrawBody(g, st, sc, fore, Height, Width);
                g.Restore(state);
            }
            else DrawBody(g, st, sc, fore, Width, Height);
        }

        private HHmiScheme Scheme0() => HHmiPalettes.Get(Theme);

        private void DrawBody(Graphics g, M st, HHmiScheme sc, Color fore, float W, float H)
        {
            // 底板
            if (st.Panel > 0)
            {
                var box = new RectangleF(2f, H * 0.16f, W - 4f, H * 0.68f);
                float rad = st.Panel == 2 ? box.Height / 2f : Math.Min(8f, box.Height * 0.3f);
                if (st.Panel == 3)
                {
                    using (var gp = HHmiDraw.RoundBox(box, rad))
                    using (var p = new Pen(sc.Bezel, 1.4f)) g.DrawPath(p, gp);
                }
                else
                {
                    Color pc = st.Dark ? Color.FromArgb(16, 20, 26) : HHmiDraw.HMixer(sc.Accent, Color.White, 0.90f);
                    using (var gp = HHmiDraw.RoundBox(box, rad))
                    {
                        using (var b = new SolidBrush(pc))
                            g.FillPath(b, gp);
                        if (st.Dark)
                            using (var p = new Pen(Color.FromArgb(70, sc.Accent.R, sc.Accent.G, sc.Accent.B), 1.1f))
                                g.DrawPath(p, gp);
                    }
                }
            }

            float alpha = 255f;
            if (st.Blink) alpha = (_phase / 6) % 2 == 0 ? 255 : 30;
            if (st.Breath) alpha = 90 + 165 * (0.5f + 0.5f * (float)Math.Sin(_phase * 0.12));
            int a = (int)Math.Max(0, Math.Min(255, alpha));
            Color fc = Color.FromArgb(a, fore.R, fore.G, fore.B);
            float cy = H / 2f - TextHeight() / 2f + 2f;

            // 打字机
            if (st.Typewriter)
            {
                int n = Text.Length;
                int cyc = (_phase / 3) % (n + 16);
                int shown = Math.Min(n, cyc);
                string s = Text.Substring(0, shown);
                using (var b = new SolidBrush(fc))
                    g.DrawString(s, Font, b, 10f, cy);
                float curX = 10f + g.MeasureString(s, Font).Width;
                if ((_phase / 3) % (n + 16) <= n || ((_phase / 3) % 2 == 0))
                    using (var b = new SolidBrush(fc))
                        g.FillRectangle(b, curX + 1f, cy + 2f, Font.Height * 0.5f, Font.Height - 3f);
                return;
            }

            // 静态逐字波浪
            if (st.Wave && st.Mode == 6)
            {
                DrawWave(g, Text, Font, fc, 8f, cy, st, _phase);
                return;
            }

            // 纯静态（闪烁/呼吸）
            if (st.Mode == 6)
            {
                using (var b = new SolidBrush(fc))
                    g.DrawString(Text, Font, b, 8f, cy);
                return;
            }

            // 滚动系
            bool vertical = st.Mode == 2 || st.Mode == 5;
            float step = (vertical ? TextHeight() : TextWidth()) + 24f;
            using (var brush = MakeBrush(st, fc, W, H, a))
            {
                if (vertical)
                {
                    for (float yy = _y - step; yy < H + step; yy += step)
                        g.DrawString(Text, Font, brush, 8f, yy);
                }
                else
                {
                    for (float xx = _x - step; xx < W + step; xx += step)
                    {
                        if (st.Dots && !st.Wave) g.Ell(xx - 10f, H / 2f, 2.6f, 2.6f, fc);
                        if (st.Wave) DrawWave(g, Text, Font, fc, xx, cy, st, _phase);
                        else
                        {
                            if (st.Shadow)
                                using (var sb = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
                                    g.DrawString(Text, Font, sb, xx + 2f, cy + 2f);
                            if (st.Glow)
                                for (int k = -2; k <= 2; k += 2)
                                    using (var gb = new SolidBrush(Color.FromArgb(55, fc.R, fc.G, fc.B)))
                                        g.DrawString(Text, Font, gb, xx + k * 0.8f, cy + (2 - Math.Abs(k)) * 0.6f);
                            g.DrawString(Text, Font, brush, xx, cy);
                        }
                        if (st.Panel == 1 && !st.Wave)
                            g.DrawString("  ●  ", Font, brush, xx + TextWidth() + 2f, cy);
                    }
                }
            }
        }

        // 逐字波浪绘制（每字独立正弦偏移）
        private static void DrawWave(Graphics g, string s, Font font, Color c,
            float x0, float y0, M st, int phase)
        {
            float x = x0;
            var chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                string ch = chars[i].ToString();
                float w = g.MeasureString(ch, font).Width;
                float dy = (float)Math.Sin(phase * 0.18 + i * 0.55) * font.Height * 0.22f;
                if (st.Glow)
                    for (int k = -2; k <= 2; k += 2)
                        using (var gb = new SolidBrush(Color.FromArgb(60, c.R, c.G, c.B)))
                            g.DrawString(ch, font, gb, x + k, y0 + dy);
                using (var b = new SolidBrush(c))
                    g.DrawString(ch, font, b, x, y0 + dy);
                x += w;
            }
        }

        // 文字画刷：普通/渐隐/渐变/阴影
        private Brush MakeBrush(M st, Color fore, float W, float H, int alpha)
        {
            if (st.Fade)
            {
                var rect = new RectangleF(0, 0, W, H);
                var c1 = Color.FromArgb(0, fore.R, fore.G, fore.B);
                var c2 = Color.FromArgb(alpha, fore.R, fore.G, fore.B);
                var blend = new ColorBlend(4)
                {
                    Colors = new[] { c1, c2, c2, c1 },
                    Positions = new[] { 0f, 0.12f, 0.88f, 1f }
                };
                return new LinearGradientBrush(rect, c2, c1, 0f) { InterpolationColors = blend };
            }
            if (st.Gradient)
                return new LinearGradientBrush(new RectangleF(0, 0, W, H),
                    Color.FromArgb(alpha, 46, 134, 224), Color.FromArgb(alpha, 222, 62, 110), 0f);
            return new SolidBrush(fore);
        }
    }
}
