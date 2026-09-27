using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>旋转/拨杆开关外形样式，共 22 种。</summary>
    public enum HRotarySwitchStyle
    {
        Classic = 0,        // 经典圆形旋转拨杆（红绿端点）
        ToggleLever = 1,    // 竖直拨杆
        Rocker = 2,         // 船型翘板开关
        Slider = 3,         // 滑动开关
        RotaryKnob = 4,     // 档位旋钮
        Knife = 5,          // 闸刀开关
        Industrial = 6,     // 工业方形带螺栓
        KeySwitch = 7,      // 钥匙开关
        Guarded = 8,        // 防误触护罩
        MetalLever = 9,     // 金属拨杆
        Flat = 10,          // 扁平
        Minimal = 11,       // 极简圆点
        Digital = 12,       // 数显 ON/OFF 面板
        PilotButton = 13,   // 带灯自锁按钮
        CircuitBreaker = 14,// 断路器扳手
        DualThrow = 15,     // 双掷开关
        GlassPanel = 16,    // 玻璃面板
        Cyber = 17,         // 赛博朋克
        NightVision = 18,   // 夜视荧光绿
        Amber = 19,         // 琥珀色
        Vintage = 20,       // 黄铜复古
        HeavyDuty = 21      // 重型防护
    }

    /// <summary>
    /// 开关控件：22 种外形 × 13 种色调，拨杆/滑块/旋钮带平滑动画，
    /// 端点红绿状态指示、可配置状态文本（如 Off;On），点击切换并触发事件。
    /// </summary>
    [DefaultProperty("Checked")]
    [DefaultEvent("CheckedChanged")]
    [HDescriptionLanguage("旋转/拨杆开关控件：22 种外形、13 种色调，带平滑切换动画")]
    public class HRotarySwitch : HLabelBase
    {
        private readonly Timer _timer;
        private HRotarySwitchStyle _style = HRotarySwitchStyle.Classic;
        private HToolTheme _theme = HToolTheme.Classic;
        private bool _checked;
        private float _anim;                  // 0..1 动画进度
        private string _statusTexts = "Off;On";
        private static readonly Color OffColor = Color.FromArgb(196, 64, 58);
        private static readonly Color OnColor = Color.FromArgb(72, 196, 96);

        /// <summary>默认方形。</summary>
        public HRotarySwitch()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(80, 80);
            _timer = new Timer { Interval = 20 };
            _timer.Tick += AnimTick;
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("开关"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("开关外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HRotarySwitchStyle.Classic)]
        public HRotarySwitchStyle SwitchStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>图元色调。</summary>
        [HCategoryLanguage("开关"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>合闸（On）状态。</summary>
        [HCategoryLanguage("开关"), HDisplayNameLanguage("合闸状态"), HDescriptionLanguage("是否处于合闸 On 状态"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                if (IsHandleCreated) _timer.Start();
                else _anim = value ? 1f : 0f;
                CheckedChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>状态文本，分号分隔（如 Off;On）。</summary>
        [HCategoryLanguage("开关"), HDisplayNameLanguage("状态文本"), HDescriptionLanguage("分号分隔的断开/合闸状态文本"), Browsable(true)]
        [DefaultValue("Off;On")]
        public string StatusTexts
        {
            get => _statusTexts;
            set { _statusTexts = value ?? "Off;On"; Invalidate(); }
        }

        /// <summary>状态切换事件。</summary>
        public event EventHandler CheckedChanged;

        /// <summary>切换合闸/断开。</summary>
        public void Toggle() { Checked = !_checked; }

        protected override void OnClick(EventArgs e) { Toggle(); base.OnClick(e); }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Stop(); _timer.Dispose(); }
            base.Dispose(disposing);
        }

        private void AnimTick(object sender, EventArgs e)
        {
            if (IsDisposed) { _timer.Stop(); return; }
            float target = _checked ? 1f : 0f;
            _anim += (target - _anim) * 0.28f;
            if (Math.Abs(target - _anim) < 0.01f) { _anim = target; _timer.Stop(); }
            Invalidate();
        }

        private string[] Labels()
        {
            var p = _statusTexts.Split(';');
            return new[] { p.Length > 0 ? p[0] : "Off", p.Length > 1 ? p[1] : "On" };
        }

        private sealed class S
        {
            public int Plate;         // 0 圆、1 圆角方、2 胶囊
            public int Lever;         // 0 旋转杆、1 竖拨杆、2 翘板、3 滑块、4 旋钮、5 闸刀
            public bool Dark;
            public bool Glow;
            public bool Bolts;
            public bool Guard;
            public bool Flat;
            public bool Metal;
            public bool Digital;
            public Color Acc;
            public bool CustomAcc;
        }

        private static S StyleOf(HRotarySwitchStyle s)
        {
            var d = new S();
            switch (s)
            {
                case HRotarySwitchStyle.ToggleLever: d.Lever = 1; break;
                case HRotarySwitchStyle.Rocker: d.Plate = 2; d.Lever = 2; break;
                case HRotarySwitchStyle.Slider: d.Plate = 2; d.Lever = 3; d.Flat = true; break;
                case HRotarySwitchStyle.RotaryKnob: d.Lever = 4; d.Metal = true; break;
                case HRotarySwitchStyle.Knife: d.Lever = 5; break;
                case HRotarySwitchStyle.Industrial: d.Plate = 1; d.Bolts = true; d.Metal = true; break;
                case HRotarySwitchStyle.KeySwitch: d.Lever = 4; d.Plate = 1; break;
                case HRotarySwitchStyle.Guarded: d.Bolts = true; d.Guard = true; d.Plate = 1; break;
                case HRotarySwitchStyle.MetalLever: d.Metal = true; d.Lever = 1; break;
                case HRotarySwitchStyle.Flat: d.Flat = true; d.Lever = 3; d.Plate = 2; break;
                case HRotarySwitchStyle.Minimal: d.Flat = true; break;
                case HRotarySwitchStyle.Digital: d.Plate = 2; d.Digital = true; d.Dark = true; break;
                case HRotarySwitchStyle.PilotButton: d.Plate = 1; d.Lever = 6; break;
                case HRotarySwitchStyle.CircuitBreaker: d.Plate = 2; d.Lever = 5; break;
                case HRotarySwitchStyle.DualThrow: d.Lever = 0; break;
                case HRotarySwitchStyle.GlassPanel: d.Plate = 1; d.Metal = true; d.Lever = 1; break;
                case HRotarySwitchStyle.Cyber: d.Dark = true; d.Glow = true; d.Plate = 1;
                    d.Acc = Color.FromArgb(0, 229, 255); d.CustomAcc = true; break;
                case HRotarySwitchStyle.NightVision: d.Dark = true; d.Glow = true;
                    d.Acc = Color.FromArgb(96, 255, 150); d.CustomAcc = true; break;
                case HRotarySwitchStyle.Amber: d.Dark = true; d.Glow = true;
                    d.Acc = Color.FromArgb(255, 176, 40); d.CustomAcc = true; break;
                case HRotarySwitchStyle.Vintage: d.Metal = true; d.Bolts = true;
                    d.Acc = Color.FromArgb(186, 132, 60); d.CustomAcc = true; break;
                case HRotarySwitchStyle.HeavyDuty: d.Plate = 1; d.Bolts = true; d.Metal = true;
                    d.Lever = 1; d.Guard = true; break;
            }
            return d;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 14 || Height < 14) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            S st = StyleOf(_style);
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : HHmiPalettes.Get(Theme);
            Color acc = st.CustomAcc ? st.Acc : sc.Accent;
            string[] lb = Labels();
            float t = _anim;
            Color live = Color.FromArgb((int)(OffColor.R + (OnColor.R - OffColor.R) * t),
                (int)(OffColor.G + (OnColor.G - OffColor.G) * t),
                (int)(OffColor.B + (OnColor.B - OffColor.B) * t));

            float cx = Width / 2f, cy = Height / 2f;
            float r = Math.Min(Width, Height) / 2f - 3f;

            // 底板
            RectangleF plate;
            GraphicsPath pp;
            if (st.Plate == 2)
            {
                float pw = Math.Min(Width - 6f, r * 2.1f), ph = r * 1.05f;
                plate = new RectangleF(cx - pw / 2f, cy - ph / 2f, pw, ph);
                pp = HHmiDraw.RoundBox(plate, ph / 2f);
            }
            else if (st.Plate == 1)
            {
                float s2 = r * 1.86f;
                plate = new RectangleF(cx - s2 / 2f, cy - s2 / 2f, s2, s2);
                pp = HHmiDraw.RoundBox(plate, s2 * 0.16f);
            }
            else
            {
                plate = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
                pp = new GraphicsPath();
                pp.AddEllipse(plate);
            }

            using (pp)
            {
                Brush backB;
                if (st.Digital) backB = new SolidBrush(Color.FromArgb(14, 18, 24));
                else if (st.Flat) backB = new SolidBrush(Color.FromArgb(224, 228, 233));
                else if (st.Metal)
                    backB = new LinearGradientBrush(plate,
                        HHmiDraw.HMixer(sc.Bezel, Color.White, 0.35f),
                        HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.30f), 55f);
                else
                    backB = new LinearGradientBrush(plate,
                        st.Dark ? Color.FromArgb(46, 52, 60) : Color.FromArgb(246, 248, 250),
                        st.Dark ? Color.FromArgb(22, 26, 32) : Color.FromArgb(212, 218, 225), 55f);
                using (backB) g.FillPath(backB, pp);
                using (var p = new Pen(st.Flat ? Color.FromArgb(186, 192, 199) : sc.BezelDark, 1.4f))
                    g.DrawPath(p, pp);
            }

            // 螺栓
            if (st.Bolts)
            {
                float br = Math.Max(1.8f, r * 0.05f);
                for (int i = 0; i < 4; i++)
                {
                    var bp = HHmiDraw.Polar(cx, cy, r * 0.80f, i * 90f + 45f);
                    g.Rivet(bp.X, bp.Y, br, sc.Bezel, sc.BezelDark);
                }
            }

            // 护罩
            if (st.Guard)
                using (var p = new Pen(Color.FromArgb(150, sc.Bezel), Math.Max(2f, r * 0.07f)))
                    g.DrawArc(p, cx - r * 0.62f, cy - r * 0.62f, r * 1.24f, r * 1.24f, 200f, 140f);

            // 数显面板
            if (st.Digital)
            {
                float pw = plate.Width * 0.72f, ph = plate.Height * 0.42f;
                var box = new RectangleF(cx - pw / 2f, cy - ph * 0.78f, pw, ph);
                using (var df = HHmiDraw.Pf("Consolas", Math.Max(10.7f, ph * 0.5f), FontStyle.Bold))
                g.Lcd(box, lb[_checked ? 1 : 0].ToUpperInvariant(), df,
                    Color.FromArgb(8, 12, 16), live, Color.FromArgb(80, live));
                using (var tf = HHmiDraw.Pf(Font.FontFamily, 8.7f))
                    g.CenterText(new RectangleF(plate.X, plate.Bottom - plate.Height * 0.30f,
                        plate.Width, plate.Height * 0.22f), Text, tf, sc.Text);
                DrawLedDots(g, cx, cy + plate.Height * 0.28f, r * 0.5f, t, live);
                return;
            }

            DrawLever(g, st, acc, sc, cx, cy, r, t, live);

            // 状态文本
            if (!st.Digital)
            {
                using (var tf = HHmiDraw.Pf(Font.FontFamily, Math.Max(8f, r * 0.16f), FontStyle.Bold))
                {
                    float lw = r * 0.7f;
                    g.CenterText(new RectangleF(cx - r * 0.85f, cy + r * 0.52f, lw, r * 0.24f),
                        lb[0], tf, Color.FromArgb((int)(255 - 120 * t), OffColor));
                    g.CenterText(new RectangleF(cx + r * 0.15f, cy + r * 0.52f, lw, r * 0.24f),
                        lb[1], tf, Color.FromArgb((int)(135 + 120 * t), OnColor));
                }
            }
            if (!string.IsNullOrEmpty(Text) && st.Plate != 2)
                using (var tf = HHmiDraw.Pf(Font.FontFamily, Math.Max(8f, r * 0.16f)))
                    g.CenterText(new RectangleF(0, Height - 13f, Width, 12f), Text, tf, sc.Text);
        }

        // 各类操作件
        private void DrawLever(Graphics g, S st, Color acc, HHmiScheme sc,
            float cx, float cy, float r, float t, Color live)
        {
            float swing = 36f * (t * 2f - 1f);    // -36..+36
            switch (st.Lever)
            {
                case 1: // 竖直拨杆
                {
                    float slot = r * 0.62f, dy = -slot / 2f + slot * t;
                    using (var gp = HHmiDraw.RoundBox(new RectangleF(cx - r * 0.13f, cy - slot / 2f, r * 0.26f, slot), r * 0.13f))
                    using (var b = new SolidBrush(Color.FromArgb(st.Dark ? 40 : 210, 40, 40, 40)))
                        g.FillPath(b, gp);
                    var state = g.Save();
                    g.TranslateTransform(cx, cy + dy);
                    g.RotateTransform(swing * 0.25f);
                    using (var b = new LinearGradientBrush(new RectangleF(-r * 0.07f, -r * 0.42f, r * 0.14f, r * 0.5f),
                        Color.FromArgb(238, 240, 244), sc.BezelDark, 0f))
                        g.FillRectangle(b, -r * 0.07f, -r * 0.42f, r * 0.14f, r * 0.5f);
                    g.FaceDisk(0, -r * 0.42f, r * 0.18f, live, HHmiDraw.HMixer(live, Color.Black, 0.35f));
                    g.Restore(state);
                    g.Hub(cx, cy, r * 0.12f, 2, sc.Bezel, sc.BezelDark);
                    break;
                }
                case 2: // 翘板
                {
                    float pw = r * 1.32f, ph = r * 0.62f;
                    var box = new RectangleF(cx - pw / 2f, cy - ph / 2f, pw, ph);
                    using (var gp = HHmiDraw.RoundBox(box, ph * 0.28f))
                    using (var b = new SolidBrush(HHmiDraw.HMixer(Color.FromArgb(60, 66, 74), live, 0.30f * (t > 0.5f ? 1f : 0f))))
                        g.FillPath(b, gp);
                    var state = g.Save();
                    g.TranslateTransform(cx, cy);
                    g.RotateTransform(-12f + 24f * t);
                    using (var flap = HHmiDraw.RoundBox(new RectangleF(-pw * 0.42f, -ph * 0.30f, pw * 0.84f, ph * 0.60f), ph * 0.22f))
                    using (var b = new LinearGradientBrush(new RectangleF(0, -ph / 2f, 1, ph), Color.White, sc.Bezel, 90f))
                        g.FillPath(b, flap);
                    g.Restore(state);
                    DrawLedDots(g, cx, cy + ph * 0.62f, pw * 0.7f, t, live);
                    break;
                }
                case 3: // 滑块
                {
                    float track = r * 1.5f;
                    using (var gp = HHmiDraw.RoundBox(new RectangleF(cx - track / 2f, cy - r * 0.12f, track, r * 0.24f), r * 0.12f))
                    using (var b = new SolidBrush(Color.FromArgb(60, 64, 70)))
                        g.FillPath(b, gp);
                    float kx = cx - track / 2f + track * t;
                    if (st.Glow) g.Ell(kx, cy, r * 0.26f, r * 0.26f, Color.FromArgb(80, live));
                    g.FaceDisk(kx, cy, r * 0.21f, Color.White, sc.BezelDark);
                    DrawLedDots(g, cx, cy + r * 0.42f, track * 0.8f, t, live);
                    break;
                }
                case 4: // 旋钮
                {
                    g.FaceDisk(cx, cy, r * 0.62f, HHmiDraw.HMixer(sc.Bezel, Color.White, 0.30f), sc.BezelDark);
                    for (int i = 0; i < 24; i++)
                    {
                        float a = -135f + i * 270f / 23f;
                        using (var p = new Pen(Color.FromArgb(120, sc.BezelDark), 1.2f))
                            g.DrawLine(p, HHmiDraw.Polar(cx, cy, r * 0.66f, a),
                                HHmiDraw.Polar(cx, cy, r * 0.74f, a));
                    }
                    var state = g.Save();
                    g.TranslateTransform(cx, cy);
                    g.RotateTransform(-135f + 270f * t);
                    using (var b = new SolidBrush(live))
                        g.FillPolygon(b, new[]
                        {
                            new PointF(0, -r * 0.44f), new PointF(r * 0.07f, r * 0.1f),
                            new PointF(-r * 0.07f, r * 0.1f)
                        });
                    g.Restore(state);
                    g.Hub(cx, cy, r * 0.14f, 1, sc.Bezel, sc.BezelDark);
                    break;
                }
                case 5: // 闸刀/扳手
                {
                    float px = cx - r * 0.42f;
                    g.Hub(px, cy, r * 0.12f, 2, sc.Bezel, sc.BezelDark);
                    var state = g.Save();
                    g.TranslateTransform(px, cy);
                    g.RotateTransform(-38f + 76f * t);
                    using (var b = new SolidBrush(sc.Bezel))
                        g.FillRectangle(b, 0, -r * 0.06f, r * 0.72f, r * 0.12f);
                    using (var b = new SolidBrush(HHmiDraw.HMixer(live, Color.Black, 0.2f)))
                        g.FillEllipse(b, r * 0.60f, -r * 0.13f, r * 0.22f, r * 0.26f);
                    g.Restore(state);
                    g.Hub(cx + r * 0.42f, cy, r * 0.10f, 0, live, sc.BezelDark);
                    break;
                }
                case 6: // 带灯按钮
                {
                    float rr = r * 0.60f * (1f - 0.06f * (float)Math.Sin(_anim * Math.PI));
                    if (st.Glow || _anim > 0.1f)
                        g.Ell(cx, cy, rr * 1.35f, rr * 1.35f, Color.FromArgb(70, live));
                    g.FaceDisk(cx, cy, rr, live, HHmiDraw.HMixer(live, Color.Black, 0.4f));
                    g.Ell(cx - rr * 0.3f, cy - rr * 0.3f, rr * 0.32f, rr * 0.32f,
                        Color.FromArgb(110, 255, 255, 255));
                    break;
                }
                default: // 0 旋转拨杆
                {
                    float rr = r * 0.60f;
                    var p1 = HHmiDraw.Polar(cx, cy, rr, -36f);
                    var p2 = HHmiDraw.Polar(cx, cy, rr, 36f);
                    g.Ell(p1.X, p1.Y, r * 0.10f, r * 0.10f, Color.FromArgb((int)(255 - 130 * t), OffColor));
                    g.Ell(p2.X, p2.Y, r * 0.10f, r * 0.10f, Color.FromArgb((int)(125 + 130 * t), OnColor));
                    if (st.Glow)
                    {
                        var pg = t > 0.5f ? p2 : p1;
                        g.Ell(pg.X, pg.Y, r * 0.20f, r * 0.20f, Color.FromArgb(70, acc.R, acc.G, acc.B));
                    }
                    // 双掷：三组端点
                    if (_style == HRotarySwitchStyle.DualThrow)
                    {
                        var p3 = HHmiDraw.Polar(cx, cy, rr, 180f);
                        g.Ell(p3.X, p3.Y, r * 0.09f, r * 0.09f, Color.Gray);
                    }
                    var state = g.Save();
                    g.TranslateTransform(cx, cy);
                    g.RotateTransform(swing);
                    using (var b = new SolidBrush(st.Metal ? sc.Bezel : Color.FromArgb(48, 54, 62)))
                        g.FillRectangle(b, -r * 0.05f, -r * 0.58f, r * 0.10f, r * 0.62f);
                    g.Restore(state);
                    g.Hub(cx, cy, r * 0.18f, 0,
                        st.Metal ? HHmiDraw.HMixer(sc.Bezel, Color.White, 0.3f) : Color.FromArgb(70, 78, 88),
                        sc.BezelDark);
                    break;
                }
            }
        }

        // 左右两颗状态灯
        private static void DrawLedDots(Graphics g, float cx, float cy, float halfW, float t, Color live)
        {
            g.Ell(cx - halfW * 0.5f, cy, 3.2f, 3.2f, Color.FromArgb((int)(255 - 130 * t), OffColor));
            g.Ell(cx + halfW * 0.5f, cy, 3.2f, 3.2f, Color.FromArgb((int)(125 + 130 * t), OnColor));
        }
    }
}
