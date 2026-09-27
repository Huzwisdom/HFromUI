using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Vessel
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 混合搅拌设备样式：Classic 为立式搅拌仓，其余 21 种覆盖螺带/桨叶/犁刀/V 型/双锥混合机、
    /// 滚筒与盘式搅拌机、双卧轴、反应釜、锥形螺杆、静态混合器等。
    /// </summary>
    public enum HMixSiloStyle
    {
        Classic = 0,        // 立式搅拌仓
        RibbonBlender = 1,  // 卧式螺带混合机
        PaddleMixer = 2,    // 双轴桨叶混合机
        Plowshare = 3,      // 犁刀混合机
        VBlender = 4,       // V 型混合机
        DoubleCone = 5,     // 双锥混合机
        DrumMixer = 6,      // 滚筒混料机
        PanMixer = 7,       // 立轴行星盘式搅拌机
        TwinShaft = 8,      // 双卧轴混凝土搅拌机
        VerticalRibbon = 9, // 立式螺带混合机
        Turbine = 10,       // 涡轮搅拌反应釜
        Anchor = 11,        // 锚式搅拌釜
        MagneticStirrer = 12,// 磁力搅拌器
        HighShear = 13,     // 高剪切乳化罐
        NautaScrew = 14,    // 锥形螺杆混合机
        StaticMixer = 15,   // 管道静态混合器
        ContinuousMixer = 16,// 连续式搅拌机
        SlurryTank = 17,    // 泥浆搅拌罐
        Jacketed = 18,      // 夹套反应釜
        BottomEntry = 19,   // 底入式搅拌罐
        SideEntry = 20,     // 侧入式搅拌罐
        MortarMixer = 21    // 灰浆倾翻搅拌机
    }
    /// <summary>
    /// 混合搅拌仓控件（继承 HToolAnimBase）：22 种混合设备外形、13 种色调，
    /// Running 时桨叶转动；Level 为仓内料位（0..100）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("混合搅拌设备控件：22 种混合外形、13 种色调，运行时桨叶转动")]
    public class HMixSilo : HToolAnimBase
    {
        private double _level = 55;
        private HMixSiloStyle _style = HMixSiloStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HMixSilo() { Size = new Size(124, 196); }
        /// <summary>仓内料位百分比（0..100）。</summary>
        [HCategoryLanguage("混合料仓"), HDisplayNameLanguage("仓内料位百分比"), HDescriptionLanguage("仓内料位百分比，0..100"), Browsable(true)]
        [DefaultValue(55.0)]
        public double Level
        {
            get => _level;
            set { _level = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }
        /// <summary>混合搅拌设备外形样式。</summary>
        [HCategoryLanguage("混合料仓"), HDisplayNameLanguage("混合搅拌设备外形样式"), HDescriptionLanguage("混合搅拌设备外形样式"), Browsable(true)]
        [DefaultValue(HMixSiloStyle.Classic)]
        public HMixSiloStyle MixSiloStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HMixSiloStyle.RibbonBlender: DrawRibbonH(g, a, pal, false); break;
                case HMixSiloStyle.ContinuousMixer: DrawRibbonH(g, a, pal, true); break;
                case HMixSiloStyle.PaddleMixer: DrawTwinPaddle(g, a, pal, true); break;
                case HMixSiloStyle.TwinShaft: DrawTwinPaddle(g, a, pal, false); break;
                case HMixSiloStyle.Plowshare: DrawPlow(g, a, pal); break;
                case HMixSiloStyle.VBlender: DrawV(g, a, pal); break;
                case HMixSiloStyle.DoubleCone: DrawDoubleCone(g, a, pal); break;
                case HMixSiloStyle.DrumMixer: DrawDrum(g, a, pal); break;
                case HMixSiloStyle.PanMixer: DrawPan(g, a, pal); break;
                case HMixSiloStyle.VerticalRibbon: DrawRibbonV(g, a, pal); break;
                case HMixSiloStyle.Turbine: DrawReactor(g, a, pal, 0); break;
                case HMixSiloStyle.Anchor: DrawReactor(g, a, pal, 1); break;
                case HMixSiloStyle.Jacketed: DrawReactor(g, a, pal, 2); break;
                case HMixSiloStyle.MagneticStirrer: DrawMagnetic(g, a, pal); break;
                case HMixSiloStyle.HighShear: DrawHighShear(g, a, pal); break;
                case HMixSiloStyle.NautaScrew: DrawNauta(g, a, pal); break;
                case HMixSiloStyle.StaticMixer: DrawStatic(g, a, pal); break;
                case HMixSiloStyle.SlurryTank: DrawTank(g, a, pal, 0); break;
                case HMixSiloStyle.BottomEntry: DrawTank(g, a, pal, 1); break;
                case HMixSiloStyle.SideEntry: DrawTank(g, a, pal, 2); break;
                case HMixSiloStyle.MortarMixer: DrawMortar(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：立式搅拌仓
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.66f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + a.Height * 0.16f, cylBot = a.Top + a.Height * 0.62f;
            float neckW = w * 0.2f, neckY = a.Top + a.Height * 0.82f;
            using (var leg = new Pen(pal.MetalDark, 4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(leg, lx + 2f, cylBot, lx - 5f, a.Bottom - 4f);
                g.DrawLine(leg, rx - 2f, cylBot, rx + 5f, a.Bottom - 4f);
            }
            var shell = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckW / 2f, neckY),
                new PointF(cx - neckW / 2f, neckY), new PointF(lx, cylBot)
            };
            using (var body = new GraphicsPath())
            {
                body.AddPolygon(shell);
                body.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(lx, a.Top, w, neckY - a.Top),
                    HToolPalettes.Tint(pal.Main, 0.35f), HToolPalettes.Shade(pal.Main, 0.2f), 0f))
                    g.FillPath(b, body);
                var oldClip = g.Clip;
                g.SetClip(body);
                float pct = (float)(_level / 100.0);
                float fillTop = neckY - pct * (neckY - top);
                var fr = new RectangleF(lx - 2f, fillTop, w + 4f, neckY - fillTop + 2f);
                using (var mb = new LinearGradientBrush(fr, Color.FromArgb(224, 222, 214),
                    Color.FromArgb(188, 186, 176), 90f))
                    g.FillRectangle(mb, fr);
                g.DrawLine(new Pen(Color.FromArgb(150, 148, 138), 1.2f), lx - 2f, fillTop, rx + 2f, fillTop);
                float scale = Math.Abs((float)Math.Cos(SpinAngle * Math.PI / 180.0));
                using (var shaft = new Pen(Color.FromArgb(96, 100, 106), 3.4f))
                    g.DrawLine(shaft, cx, top - 4f, cx, neckY - 2f);
                using (var paddle = new Pen(Color.FromArgb(74, 78, 84), 4.4f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    float[] ys = { cylBot - w * 0.12f, cylBot + (neckY - cylBot) * 0.45f };
                    foreach (float py in ys)
                    {
                        float pw = (w * 0.34f) * (0.35f + 0.65f * scale);
                        g.DrawLine(paddle, cx - pw, py, cx + pw, py);
                    }
                }
                g.Clip = oldClip;
                using (var sheen = new LinearGradientBrush(new RectangleF(lx, a.Top, w, neckY - a.Top),
                    Color.FromArgb(46, Color.White), Color.FromArgb(0, Color.White), 90f))
                    g.FillPath(sheen, body);
                g.DrawPath(new Pen(pal.Edge, 1.6f), body);
            }
            using (var lid = new SolidBrush(pal.MetalDark))
                g.FillEllipse(lid, lx - 2f, top - 5f, w + 4f, 10f);
            var gb = new RectangleF(cx - w * 0.18f, top - a.Height * 0.13f, w * 0.36f, a.Height * 0.08f);
            g.Box(gb, 2f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.2f);
            var mot = new RectangleF(cx - w * 0.3f, top - a.Height * 0.2f, w * 0.6f, a.Height * 0.075f);
            using (var gp = HBarBase.RoundPath(mot, mot.Height / 2f))
            using (var b = HBarBase.CylinderH(mot, pal.MetalDark, pal.Metal))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.2f), gp);
            }
            if (Running)
                g.Ell(cx, mot.Bottom + 4f, 3f, 3f, pal.Accent);
            using (var spout = new SolidBrush(pal.Metal))
                g.FillRectangle(spout, cx - neckW / 2f, neckY, neckW, a.Height * 0.08f);
        }
        /// <summary>在槽体内裁剪填充料浆（灰白色）。</summary>
        private void FillPaste(Graphics g, RectangleF cavity, float top, float bottom, float pct)
        {
            float fillTop = bottom - pct * (bottom - top);
            var fr = new RectangleF(cavity.X, fillTop, cavity.Width, bottom - fillTop);
            using (var mb = new LinearGradientBrush(fr, Color.FromArgb(224, 222, 214),
                Color.FromArgb(188, 186, 176), 90f))
                g.FillRectangle(mb, fr);
            g.Line(Color.FromArgb(150, 148, 138), 1.2f, cavity.X, fillTop, cavity.Right, fillTop);
        }
        // ----------------------------------------------------------------
        // 新式混合设备
        // ----------------------------------------------------------------
        private void DrawRibbonH(Graphics g, RectangleF a, HToolPalette pal, bool cont)
        {
            // 卧式螺带混合机 U 槽；cont 为连续式（两端进出料口）
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.78f, x1 = cx - w / 2f, x2 = cx + w / 2f;
            float top = a.Top + a.Height * 0.34f, r = a.Height * 0.22f;
            float bot = top + r * 2f;
            // 支腿
            g.Line(pal.MetalDark, 4f, x1 + 10f, bot, x1 + 6f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, x2 - 10f, bot, x2 - 6f, a.Bottom - 5f);
            // U 槽
            var trough = new RectangleF(x1, top, w, r * 2f);
            var old = g.Clip;
            using (var path = new GraphicsPath())
            {
                path.AddArc(x1, top, r * 2f, r * 2f, 90f, 180f);
                path.AddLine(x2, top, x1, top);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(trough, HToolPalettes.Tint(pal.Main, 0.3f),
                    HToolPalettes.Shade(pal.Main, 0.22f), 0f))
                    g.FillPath(b, path);
                g.SetClip(path);
                FillPaste(g, new RectangleF(x1, top, w, r * 2f), top, bot, (float)(_level / 100.0) * 0.8f);
                // 螺带（螺旋带状叶片）
                float move = Running ? Phase / (float)Math.PI * 0.5f * 30f : 0f;
                using (var ribbon = new Pen(HToolPalettes.Shade(pal.Metal, 0.15f), 3.4f))
                    for (float x = x1 - 30f + move % 30f; x < x2; x += 15f)
                    {
                        g.DrawLine(ribbon, x, top + 2f, x + 15f, bot - 2f);
                        g.DrawLine(ribbon, x, bot - 2f, x + 15f, top + 2f);
                    }
                // 中心轴
                g.Line(pal.MetalDark, 3.4f, x1 + 4f, top + r, x2 - 4f, top + r);
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), path);
            }
            // 端盖 + 驱动
            g.Box(new RectangleF(x2 - 2f, top + r - 10f, 8f, 20f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Motor(x2 + 8f, a.Right - 2f, top + r - 2f, a.Height * 0.07f, pal);
            if (cont)
            {
                // 上进料口 + 下出料口
                g.Box(new RectangleF(x1 + w * 0.28f, a.Top + 8f, 12f, top - a.Top - 8f), 1.5f,
                    pal.MetalDark, pal.Edge, 1.1f);
                var hopper = new[]
                {
                    new PointF(x1 + w * 0.24f, a.Top + 6f), new PointF(x1 + w * 0.36f, a.Top + 6f),
                    new PointF(x1 + w * 0.34f, a.Top + 14f), new PointF(x1 + w * 0.26f, a.Top + 14f)
                };
                g.Poly(hopper, pal.Metal, pal.Edge, 1.1f);
                g.Box(new RectangleF(x1 + w * 0.6f, bot, 10f, 10f), 1.5f, pal.MetalDark, pal.Edge, 1.1f);
            }
            else
            {
                // 上盖 + 检修口
                g.Line(pal.MetalDark, 3.4f, x1, top, x2, top);
                g.Box(new RectangleF(cx - 10f, top - 8f, 20f, 8f), 2f, pal.MetalDark, pal.Edge, 1f);
            }
        }
        private void DrawTwinPaddle(Graphics g, RectangleF a, HToolPalette pal, bool overlap)
        {
            // 双轴桨叶（W 槽）/ 双卧轴（长槽两轴交错）
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.8f, x1 = cx - w / 2f, x2 = cx + w / 2f;
            float top = a.Top + a.Height * 0.32f, r = a.Height * 0.2f, cy1 = top + r * 0.85f,
                cy2 = top + r * 1.15f;
            // W/长槽
            var old = g.Clip;
            using (var path = new GraphicsPath())
            {
                path.AddLine(x1, top, x2, top);
                path.AddLine(x2, top, x2, cy1);
                path.AddArc(cx - r * 1.4f, top + 2f, r * 2.8f, r * 2f, 0f, -180f);
                path.AddLine(x1, cy1, x1, top);
                path.CloseFigure();
                using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.32f)))
                    g.FillPath(b, path);
                g.SetClip(path);
                FillPaste(g, new RectangleF(x1, top, w, r * 2f), top, cy1 + r,
                    (float)(_level / 100.0) * 0.7f);
                // 两根轴 + 交错桨叶
                foreach (float sy in new[] { cy1, cy2 })
                {
                    g.Line(pal.MetalDark, 2.8f, x1 + 6f, sy, x2 - 6f, sy);
                    float move = Running ? Phase / (float)Math.PI * 0.5f * 40f : 0f, span = x2 - x1 - 12f;
                    using (var pad = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.2f)))
                        for (int i = 0; i < 6; i++)
                        {
                            float x = x1 + 8f + ((i * 26f - move) % span + span) % span;
                            var st = g.Save();
                            g.TranslateTransform(x, sy);
                            g.RotateTransform(overlap ? 35f : 50f);
                            g.FillRectangle(pad, -2.5f, -r * 0.7f, 5f, r * 1.4f);
                            g.Restore(st);
                        }
                }
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), path);
            }
            g.Line(pal.MetalDark, 3.4f, x1, top, x2, top);
            g.Line(pal.MetalDark, 4f, x1 + 8f, top + r * 2f + 4f, x1 + 4f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, x2 - 8f, top + r * 2f + 4f, x2 - 4f, a.Bottom - 5f);
            // 双轴电机端
            g.Motor(x2 + 4f, a.Right - 2f, cy1, a.Height * 0.05f, pal);
            g.Motor(x2 + 4f, a.Right - 2f, cy2, a.Height * 0.05f, pal);
        }
        private void DrawPlow(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 犁刀混合机：卧式圆筒 + 主轴 + 犁头 + 侧飞刀
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.76f, x1 = cx - w / 2f, x2 = cx + w / 2f;
            float cy = a.Top + a.Height * 0.46f, r = a.Height * 0.24f;
            var body = new RectangleF(x1, cy - r, w, r * 2f);
            var old = g.Clip;
            using (var gp = HBarBase.RoundPath(body, r))
            {
                using (var b = HBarBase.CylinderH(body, HToolPalettes.Shade(pal.Main, 0.25f),
                    HToolPalettes.Tint(pal.Main, 0.2f)))
                    g.FillPath(b, gp);
                g.SetClip(gp);
                FillPaste(g, body, cy - r, cy + r, (float)(_level / 100.0) * 0.7f);
                // 主轴
                g.Line(pal.MetalDark, 3.4f, x1 + 4f, cy, x2 - 4f, cy);
                // 犁头（斜置三角板，随轴公转——用相位上下分布）
                for (int i = 0; i < 4; i++)
                {
                    float x = x1 + w * (0.2f + i * 0.2f);
                    float ang = Running ? SpinAngle + i * 90f : i * 90f;
                    double rad = ang * Math.PI / 180.0;
                    float py = cy + (float)Math.Sin(rad) * r * 0.7f;
                    float scl = Math.Abs((float)Math.Cos(rad));
                    g.Poly(new[]
                    {
                        new PointF(x - 3f, cy), new PointF(x + 7f, py - 6f * scl - 2f),
                        new PointF(x + 7f, py + 6f * scl + 2f)
                    }, HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1f);
                }
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), gp);
            }
            // 侧飞刀小电机
            g.Motor(a.Left + 2f, x1 - 4f, cy - r * 1.2f, a.Height * 0.045f, pal);
            g.Motor(a.Left + 2f, x1 - 4f, cy + r * 1.2f - a.Height * 0.09f, a.Height * 0.045f, pal);
            g.Motor(x2 + 4f, a.Right - 2f, cy, a.Height * 0.06f, pal);
        }
        private void DrawV(Graphics g, RectangleF a, HToolPalette pal)
        {
            // V 型混合机：两段圆筒对顶成 V + 两端封盖 + 横置转轴（整体绕水平轴转动）
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.2f, bot = a.Bottom - a.Height * 0.16f;
            float neckY = bot - 2f;
            var st = g.Save();
            g.TranslateTransform(cx, (top + bot) / 2f);
            g.RotateTransform(Running ? SpinAngle * 0.6f : 0f);
            var left = new[]
            {
                new PointF(-a.Width * 0.4f, -a.Height * 0.22f), new PointF(-a.Width * 0.12f, -a.Height * 0.08f),
                new PointF(0f, a.Height * 0.26f), new PointF(-a.Width * 0.28f, a.Height * 0.12f)
            };
            var right = new[]
            {
                new PointF(a.Width * 0.12f, -a.Height * 0.08f), new PointF(a.Width * 0.4f, -a.Height * 0.22f),
                new PointF(a.Width * 0.28f, a.Height * 0.12f), new PointF(0f, a.Height * 0.26f)
            };
            foreach (var pts in new[] { left, right })
            {
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(pts);
                    path.CloseFigure();
                    using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.3f)))
                        g.FillPath(b, path);
                    var old = g.Clip;
                    g.SetClip(path);
                    // 料（聚集底部）
                    float pct = (float)(_level / 100.0) * 0.4f;
                    using (var mb = new SolidBrush(Color.FromArgb(214, 212, 204)))
                        g.FillRectangle(mb, -a.Width / 2f, a.Height * 0.26f - pct * a.Height * 0.5f,
                            a.Width, pct * a.Height * 0.5f);
                    g.Clip = old;
                    g.DrawPath(new Pen(pal.Edge, 1.5f), path);
                }
            }
            g.Restore(st);
            // 两端转轴 + 支架
            g.Ell(cx - a.Width * 0.4f, (top + bot) / 2f - 4f, 4f, 4f, pal.MetalDark);
            g.Ell(cx + a.Width * 0.4f, (top + bot) / 2f - 4f, 4f, 4f, pal.MetalDark);
            g.Line(pal.MetalDark, 4f, cx - a.Width * 0.4f, (top + bot) / 2f,
                cx - a.Width * 0.42f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, cx + a.Width * 0.4f, (top + bot) / 2f,
                cx + a.Width * 0.42f, a.Bottom - 5f);
            // 卸料阀
            g.Box(new RectangleF(cx - 6f, neckY, 12f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
        }
        private void DrawDoubleCone(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 双锥混合机：上下对锥 + 短圆筒段 + 水平转轴
            float cx = a.Left + a.Width / 2f;
            float midY = a.Top + a.Height * 0.5f;
            float w = a.Width * 0.56f, band = a.Height * 0.12f;
            var st = g.Save();
            g.TranslateTransform(cx, midY);
            g.RotateTransform(Running ? SpinAngle * 0.6f : 0f);
            var pts = new[]
            {
                new PointF(0f, -a.Height * 0.3f), new PointF(w / 2f, -band),
                new PointF(w / 2f, band), new PointF(0f, a.Height * 0.3f),
                new PointF(-w / 2f, band), new PointF(-w / 2f, -band)
            };
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(pts);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(-w / 2f, -a.Height * 0.3f, w, a.Height * 0.6f),
                    HToolPalettes.Tint(pal.Main, 0.35f), HToolPalettes.Shade(pal.Main, 0.2f), 0f))
                    g.FillPath(b, path);
                var old = g.Clip;
                g.SetClip(path);
                float pct = (float)(_level / 100.0) * 0.4f;
                using (var mb = new SolidBrush(Color.FromArgb(214, 212, 204)))
                    g.FillRectangle(mb, -w / 2f, a.Height * 0.3f - pct * a.Height * 0.55f,
                        w, pct * a.Height * 0.55f);
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), path);
            }
            // 圆筒段环筋
            g.Line(HToolPalettes.Shade(pal.Main, 0.3f), 1.4f, -w / 2f, -band, w / 2f, -band);
            g.Line(HToolPalettes.Shade(pal.Main, 0.3f), 1.4f, -w / 2f, band, w / 2f, band);
            g.Restore(st);
            // 轴 + 支架
            g.Line(pal.MetalDark, 4f, cx - w / 2f, midY, cx - w / 2f - 8f, midY);
            g.Line(pal.MetalDark, 4f, cx + w / 2f, midY, cx + w / 2f + 8f, midY);
            g.Line(pal.MetalDark, 4f, cx - w / 2f - 6f, midY, cx - w / 2f - 6f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, cx + w / 2f + 6f, midY, cx + w / 2f + 6f, a.Bottom - 5f);
            // 驱动
            g.Motor(a.Left + 2f, cx - w / 2f - 10f, midY - 10f, 8f, pal);
        }
        private void DrawDrum(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 滚筒混料机：倾斜旋转鼓 + 托轮 + 机架
            var st = g.Save();
            g.TranslateTransform(a.Left + a.Width / 2f, a.Top + a.Height * 0.46f);
            g.RotateTransform(-14f);
            float w = a.Width * 0.7f, r = a.Height * 0.22f;
            var body = new RectangleF(-w / 2f, -r, w, r * 2f);
            using (var b = HBarBase.CylinderH(body, HToolPalettes.Shade(pal.Main, 0.25f),
                HToolPalettes.Tint(pal.Main, 0.2f)))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.5f), body.X, body.Y, body.Width, body.Height);
            // 滚圈
            using (var ring = new Pen(pal.MetalDark, 3f))
            {
                g.DrawLine(ring, -w / 2f + 8f, -r, -w / 2f + 8f, r);
                g.DrawLine(ring, w / 2f - 8f, -r, w / 2f - 8f, r);
            }
            // 内部扬料板（随转）
            var stt = g.Save();
            g.TranslateTransform(0f, 0f);
            using (var lifter = new Pen(HToolPalettes.Shade(pal.Main, 0.35f), 2.4f))
                for (int i = 0; i < 6; i++)
                {
                    double an = (Running ? SpinAngle : 30f) * Math.PI / 180.0 + i * Math.PI / 3.0;
                    g.DrawLine(lifter, (float)Math.Cos(an) * w * 0.32f, (float)Math.Sin(an) * r * 0.6f,
                        (float)Math.Cos(an) * w * 0.42f, (float)Math.Sin(an) * r * 0.9f);
                }
            g.Restore(stt);
            // 料
            using (var mb = new SolidBrush(Color.FromArgb(206, 204, 196)))
                g.FillEllipse(mb, -w * 0.3f, r * 0.15f, w * 0.5f, r * 0.7f);
            g.Restore(st);
            // 托轮 + 机架
            float cy = a.Top + a.Height * 0.46f;
            foreach (float k in new[] { -0.22f, 0.22f })
            {
                float rx2 = a.Left + a.Width * (0.5f + k);
                g.Ell(rx2, cy + a.Height * 0.22f, 7f, 7f, pal.MetalDark);
                g.Line(pal.MetalDark, 3.4f, rx2, cy + a.Height * 0.22f + 7f, rx2, a.Bottom - 5f);
            }
            g.Motor(a.Left + a.Width * 0.6f, a.Right - 4f, a.Bottom - 18f, 8f, pal);
        }
        private void DrawPan(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 立轴行星盘式搅拌机：圆盘锅 + 行星搅拌臂（自转+公转）
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.56f;
            float r = a.Width * 0.36f;
            // 锅
            g.Ell(cx, cy, r, r * 0.62f, HToolPalettes.Tint(pal.Metal, 0.15f));
            g.DrawEllipse(new Pen(pal.Edge, 1.6f), cx - r, cy - r * 0.62f, r * 2f, r * 1.24f);
            // 料
            float pct = (float)(_level / 100.0);
            using (var mb = new SolidBrush(Color.FromArgb(210, 208, 200)))
                g.FillEllipse(mb, cx - r * 0.86f, cy - r * 0.4f - (1f - pct) * r * 0.2f,
                    r * 1.72f, r * 0.86f);
            // 搅拌臂（公转）
            float orb = Running ? SpinAngle : 0f;
            for (int arm = 0; arm < 2; arm++)
            {
                double an = (orb + arm * 180f) * Math.PI / 180.0;
                float ax = cx + (float)Math.Cos(an) * r * 0.5f;
                float ay = cy + (float)Math.Sin(an) * r * 0.28f;
                g.Line(pal.Edge, 3f, cx, cy - r * 0.5f, ax, ay - 4f);
                // 自转桨叶（小星）
                g.FanWheel(ax, ay, r * 0.16f, 4, Running ? SpinAngle * 2.4f : 0f, Running, pal);
            }
            // 立轴 + 减速电机 + 立柱
            g.Line(pal.MetalDark, 4f, cx, a.Top + 6f, cx, cy - r * 0.5f);
            g.Motor(cx - 16f, cx + 16f, a.Top + 14f, 9f, pal);
            g.Line(pal.MetalDark, 4.4f, a.Left + a.Width * 0.16f, a.Top + 10f,
                a.Left + a.Width * 0.16f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4.4f, a.Left + a.Width * 0.16f, a.Bottom - 5f,
                cx - r, a.Bottom - 5f);
        }
        private void DrawRibbonV(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 立式螺带：锥底筒 + 内外双层螺带（螺旋线）绕轴
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.6f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + a.Height * 0.2f, cylBot = a.Top + a.Height * 0.55f;
            float neckY = a.Bottom - 16f, neckHalf = w * 0.1f;
            g.Line(pal.MetalDark, 4f, lx, cylBot, lx - 4f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, rx, cylBot, rx + 4f, a.Bottom - 5f);
            var shell = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            using (var body = new GraphicsPath())
            {
                body.AddPolygon(shell);
                body.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(lx, top, w, neckY - top),
                    HToolPalettes.Tint(pal.Main, 0.35f), HToolPalettes.Shade(pal.Main, 0.2f), 0f))
                    g.FillPath(b, body);
                var old = g.Clip;
                g.SetClip(body);
                FillPaste(g, new RectangleF(lx, top, w, neckY - top), top, neckY,
                    (float)(_level / 100.0));
                // 螺旋带（两条正弦包络线）
                float ph = Running ? Phase : 0.4f;
                using (var ribbon = new Pen(HToolPalettes.Shade(pal.Metal, 0.12f), 3f))
                {
                    var pts1 = new PointF[40];
                    var pts2 = new PointF[40];
                    for (int i = 0; i < 40; i++)
                    {
                        float t = i / 39f;
                        float y = top + 4f + t * (neckY - top - 8f);
                        float rr = (w / 2f - 5f) * (1f - t * 0.72f);
                        pts1[i] = new PointF(cx + (float)Math.Sin(t * 14f - ph * 4f) * rr, y);
                        pts2[i] = new PointF(cx - (float)Math.Sin(t * 14f - ph * 4f) * rr * 0.7f, y);
                    }
                    g.DrawLines(ribbon, pts1);
                    g.DrawLines(ribbon, pts2);
                }
                g.Line(pal.MetalDark, 3f, cx, top - 2f, cx, neckY);
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), body);
            }
            // 顶驱
            g.Motor(cx - 18f, cx + 18f, top - 16f, 9f, pal);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 9f), 1.5f, pal.Metal, pal.Edge, 1.1f);
        }
        /// <summary>反应釜系列：0 涡轮、1 锚式、2 夹套。</summary>
        private void DrawReactor(Graphics g, RectangleF a, HToolPalette pal, int kind)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.62f, lx = cx - w / 2f, rx = cx + w / 2f;
            float r = w / 2f;
            // 矮单元中为上封头与顶驱预留空间，避免几何越界
            float top = Math.Max(a.Top + a.Height * 0.22f, a.Top + r * 0.6f + 6f);
            float bot = a.Bottom - a.Height * 0.16f;
            // 支耳
            foreach (float bx in new[] { lx, rx })
                g.Poly(new[]
                {
                    new PointF(bx - 5f, bot - a.Height * 0.12f), new PointF(bx + 9f, bot - a.Height * 0.12f),
                    new PointF(bx + 7f, bot - a.Height * 0.12f + 9f), new PointF(bx - 7f, bot - a.Height * 0.12f + 9f)
                }, pal.MetalDark, pal.Edge, 1.1f);
            // 釜体（圆柱 + 椭圆下封头）
            var old = g.Clip;
            using (var body = new GraphicsPath())
            {
                body.AddRectangle(new RectangleF(lx, top, w, bot - top - r * 0.5f));
                body.AddArc(lx, bot - r, w, r, 0f, 180f);
                body.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(lx, top, w, bot - top),
                    HToolPalettes.Tint(pal.Main, 0.38f), HToolPalettes.Shade(pal.Main, 0.18f), 0f))
                    g.FillPath(b, body);
                g.SetClip(body);
                // 夹套层
                if (kind == 2)
                {
                    using (var jb = new SolidBrush(Color.FromArgb(70, HToolPalettes.Tint(pal.Main, 0.4f))))
                    {
                        g.FillRectangle(jb, lx, top, 6f, bot - top);
                        g.FillRectangle(jb, rx - 6f, top, 6f, bot - top);
                    }
                    g.Line(pal.Edge, 1.4f, lx + 7f, top + 6f, lx + 7f, bot - 10f);
                    g.Line(pal.Edge, 1.4f, rx - 7f, top + 6f, rx - 7f, bot - 10f);
                    // 夹套接口
                    g.Box(new RectangleF(lx - 6f, top + 10f, 6f, 8f), 1.5f, pal.MetalDark, pal.Edge, 1f);
                    g.Box(new RectangleF(rx, bot - 30f, 6f, 8f), 1.5f, pal.MetalDark, pal.Edge, 1f);
                }
                FillPaste(g, new RectangleF(lx, top, w, bot - top), top + 4f, bot - 6f,
                    (float)(_level / 100.0) * 0.8f);
                // 搅拌轴
                g.Line(Color.FromArgb(96, 100, 106), 3.2f, cx, a.Top + 8f, cx, bot - 8f);
                var stt = g.Save();
                g.TranslateTransform(cx, 0f);
                float shaftY = top + (bot - top) * 0.55f;
                if (kind == 1)
                {
                    // 锚式
                    g.TranslateTransform(0f, shaftY);
                    g.RotateTransform(Running ? SpinAngle * 0.5f : 0f);
                    float aw = w * 0.34f, ah = (bot - top) * 0.34f;
                    using (var anc = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 4f)
                    { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLine(anc, 0f, -ah, -aw, ah * 0.6f);
                        g.DrawLine(anc, -aw, ah * 0.6f, aw, ah * 0.6f);
                        g.DrawLine(anc, aw, ah * 0.6f, 0f, -ah);
                    }
                }
                else
                {
                    // 涡轮（圆盘 + 六叶片）
                    g.TranslateTransform(0f, shaftY);
                    g.RotateTransform(Running ? SpinAngle : 0f);
                    using (var bl = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.3f)))
                        for (int i = 0; i < 6; i++)
                        {
                            g.RotateTransform(60f);
                            g.FillRectangle(bl, 0f, -3f, w * 0.3f, 6f);
                        }
                    g.Ell(0f, 0f, 5f, 5f, pal.MetalDark);
                }
                g.Restore(stt);
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), body);
            }
            // 椭圆上封头
            using (var hd = new SolidBrush(Color.FromArgb(140, pal.Metal)))
                g.FillPie(hd, lx, top - r * 0.6f, w, r * 0.6f, 180f, 180f);
            g.DrawArc(new Pen(pal.Edge, 1.6f), lx, top - r * 0.6f, w, r * 0.6f, 180f, 180f);
            // 人孔 + 接管
            g.Ell(lx + w * 0.28f, top - r * 0.25f, 5f, 3.4f, pal.MetalDark);
            g.Box(new RectangleF(cx - 5f, a.Top + 4f, 10f, top - r * 0.6f - a.Top + 2f), 1.5f,
                pal.MetalDark, pal.Edge, 1f);
            // 顶驱电机 + 减速器
            g.Box(new RectangleF(cx - 12f, top - 26f, 24f, 12f), 2f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.2f);
            g.Motor(cx - 20f, cx + 20f, top - 34f, 8f, pal);
        }
        private void DrawMagnetic(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 磁力搅拌器：方机座 + 烧杯（料液 + 搅拌子）+ 旋钮
            float cx = a.Left + a.Width / 2f;
            float baseY = a.Bottom - 22f;
            g.Box(new RectangleF(a.Left + a.Width * 0.14f, baseY, a.Width * 0.72f, 18f), 4f,
                HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.3f);
            g.Ell(a.Left + a.Width * 0.26f, baseY + 9f, 4f, 4f, pal.Accent);
            g.Ell(a.Right - a.Width * 0.26f, baseY + 9f, 4f, 4f, pal.Metal);
            // 烧杯
            float bx1 = cx - a.Width * 0.26f, bx2 = cx + a.Width * 0.26f;
            float bTop = a.Top + a.Height * 0.22f, bBot = baseY - 1f;
            var cup = new[]
            {
                new PointF(bx1, bTop), new PointF(bx2, bTop),
                new PointF(bx2 - 4f, bBot), new PointF(bx1 + 4f, bBot)
            };
            using (var glass = new SolidBrush(Color.FromArgb(60, 200, 220, 235)))
                g.FillPolygon(glass, cup);
            g.DrawPolygon(new Pen(pal.Edge, 1.5f), cup);
            var old = g.Clip;
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(cup);
                path.CloseFigure();
                g.SetClip(path);
                float pct = (float)(_level / 100.0);
                using (var liq = new SolidBrush(Color.FromArgb(140, HToolPalettes.Tint(pal.Main, 0.25f))))
                    g.FillRectangle(liq, bx1 - 2f, bBot - (bBot - bTop) * pct * 0.8f,
                        bx2 - bx1 + 4f, (bBot - bTop) * pct * 0.8f);
                // 搅拌子（旋转椭圆）
                var st = g.Save();
                g.TranslateTransform(cx, bBot - 10f);
                g.RotateTransform(Running ? SpinAngle * 2f : 0f);
                g.FillR(new RectangleF(-10f, -2.6f, 20f, 5.2f), 2.6f, Color.FromArgb(240, 240, 244));
                g.Restore(st);
                g.Clip = old;
            }
            // 刻度
            using (var tick = new Pen(pal.Edge, 1f))
                for (int i = 1; i < 5; i++)
                    g.DrawLine(tick, bx2 - 4f, bTop + i * (bBot - bTop) / 5f,
                        bx2 + 2f, bTop + i * (bBot - bTop) / 5f);
        }
        private void DrawHighShear(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 高剪切乳化罐：矮罐 + 底部分散头（定转子齿）+ 顶置电机
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.64f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + a.Height * 0.24f, bot = a.Bottom - 16f;
            g.Line(pal.MetalDark, 4f, lx, bot - 6f, lx - 5f, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, rx, bot - 6f, rx + 5f, a.Bottom - 5f);
            var body = new RectangleF(lx, top, w, bot - top);
            using (var b = new LinearGradientBrush(body, HToolPalettes.Tint(pal.Main, 0.38f),
                HToolPalettes.Shade(pal.Main, 0.18f), 0f))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.6f), body.X, body.Y, body.Width, body.Height);
            FillPaste(g, body, top, bot - 4f, (float)(_level / 100.0) * 0.82f);
            // 长轴 + 底部转子头
            float headY = bot - 16f;
            g.Line(Color.FromArgb(96, 100, 106), 3.2f, cx, a.Top + 8f, cx, headY);
            var st = g.Save();
            g.TranslateTransform(cx, headY);
            g.RotateTransform(Running ? SpinAngle * 2f : 0f);
            // 定子（外圈槽）+ 转子（内齿）
            g.DrawEllipse(new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 2.4f), -12f, -12f, 24f, 24f);
            using (var tooth = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.35f)))
                for (int i = 0; i < 8; i++)
                {
                    g.RotateTransform(45f);
                    g.FillRectangle(tooth, 2f, -2f, 9f, 4f);
                }
            g.Restore(st);
            // 顶置高转速电机
            g.Motor(cx - 18f, cx + 18f, a.Top + 16f, 9f, pal);
            g.Box(new RectangleF(cx - 9f, a.Top + 24f, 18f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 夹套接口（冷却）
            g.Box(new RectangleF(lx - 6f, top + 14f, 6f, 7f), 1.5f, pal.MetalDark, pal.Edge, 1f);
        }
        private void DrawNauta(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 锥形螺杆混合机：锥筒 + 公转臂 + 沿壁自转螺杆
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.66f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + a.Height * 0.2f, neckY = a.Bottom - 14f;
            var shell = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(cx + 5f, neckY), new PointF(cx - 5f, neckY)
            };
            using (var body = new GraphicsPath())
            {
                body.AddPolygon(shell);
                body.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(lx, top, w, neckY - top),
                    HToolPalettes.Tint(pal.Main, 0.36f), HToolPalettes.Shade(pal.Main, 0.2f), 0f))
                    g.FillPath(b, body);
                var old = g.Clip;
                g.SetClip(body);
                FillPaste(g, new RectangleF(lx, top, w, neckY - top), top, neckY,
                    (float)(_level / 100.0) * 0.85f);
                // 公转臂（从顶中心扫到壁）+ 螺杆贴壁
                float orb = Running ? Phase * 0.6f : 0.6f;
                float sw = (float)Math.Cos(orb) * w * 0.36f;
                g.Line(HToolPalettes.Shade(pal.Main, 0.3f), 3f, cx, top + 2f, cx + sw, top + 22f);
                // 螺杆（沿锥壁斜线 + 螺纹）
                float sx = cx + sw * 0.9f;
                g.Line(pal.MetalDark, 3.4f, sx, top + 18f, cx + sw * 0.12f + 2f, neckY - 4f);
                using (var flight = new Pen(HToolPalettes.Shade(pal.Metal, 0.1f), 2.2f))
                    for (int i = 0; i < 9; i++)
                    {
                        float t = i / 8f;
                        float px = sx + (cx + sw * 0.12f + 2f - sx) * t;
                        float py = top + 18f + (neckY - 22f - top) * t;
                        g.DrawLine(flight, px - 5f, py, px + 5f, py + 5f);
                    }
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.6f), body);
            }
            // 顶盖 + 摆线电机 + 弯臂驱动
            using (var lid = new SolidBrush(pal.MetalDark))
                g.FillEllipse(lid, lx - 2f, top - 5f, w + 4f, 10f);
            g.Motor(cx - 14f, cx + 14f, top - 20f, 8f, pal);
            g.Box(new RectangleF(cx - 5f, neckY, 10f, 8f), 1.5f, pal.MetalDark, pal.Edge, 1.1f);
        }
        private void DrawStatic(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 管道静态混合器：横管 + 内部交叉混合元件 + 法兰
            float cy = a.Top + a.Height * 0.5f;
            float r = a.Height * 0.18f, x1 = a.Left + a.Width * 0.12f, x2 = a.Right - a.Width * 0.12f;
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            g.FillCylH(body, HToolPalettes.Shade(pal.Metal, 0.3f), Color.White);
            var old = g.Clip;
            using (var path = HBarBase.RoundPath(body, r))
            {
                g.SetClip(path);
                // 交叉元件（左右旋叶片）
                int n = 7;
                for (int i = 0; i < n; i++)
                {
                    float bx = x1 + (x2 - x1) * i / n;
                    using (var el = new Pen(i % 2 == 0 ? HToolPalettes.Shade(pal.Main, 0.25f)
                        : HToolPalettes.Tint(pal.Main, 0.15f), 3.4f))
                    {
                        g.DrawLine(el, bx, cy - r + 2f, bx + (x2 - x1) / n, cy + r - 2f);
                        g.DrawLine(el, bx, cy + r - 2f, bx + (x2 - x1) / n, cy - r + 2f);
                    }
                }
                // 料流（半透明料带）
                float pct = (float)(_level / 100.0);
                using (var mb = new SolidBrush(Color.FromArgb((int)(120 * pct), 214, 212, 204)))
                    g.FillRectangle(mb, x1, cy - r * 0.4f, x2 - x1, r * 0.8f * pct + 2f);
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.5f), path);
            }
            // 法兰
            g.Flange(new RectangleF(x1 - 8f, cy - r - 3f, 8f, r * 2f + 6f), pal);
            g.Flange(new RectangleF(x2, cy - r - 3f, 8f, r * 2f + 6f), pal);
            // 流向箭头
            float flow = Running ? Phase / (float)Math.PI * 0.5f * 24f : 0f;
            using (var ar = new Pen(Color.FromArgb(180, pal.Accent), 2f))
                for (int i = 0; i < 3; i++)
                {
                    float x = x1 + 8f + ((i * 40f + flow) % (x2 - x1 - 16f));
                    g.DrawLine(ar, x, cy - 14f, x + 10f, cy - 14f);
                    g.DrawLine(ar, x + 10f, cy - 14f, x + 5f, cy - 18f);
                    g.DrawLine(ar, x + 10f, cy - 14f, x + 5f, cy - 10f);
                }
        }
        /// <summary>罐体系列：0 泥浆（人孔顶搅）、1 底入式、2 侧入式。</summary>
        private void DrawTank(Graphics g, RectangleF a, HToolPalette pal, int entry)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.66f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + a.Height * 0.22f, bot = a.Bottom - 14f;
            g.Line(pal.MetalDark, 4f, lx + 4f, bot, lx, a.Bottom - 5f);
            g.Line(pal.MetalDark, 4f, rx - 4f, bot, rx, a.Bottom - 5f);
            var body = new RectangleF(lx, top, w, bot - top);
            using (var b = new LinearGradientBrush(body, HToolPalettes.Tint(pal.Main, 0.36f),
                HToolPalettes.Shade(pal.Main, 0.18f), 0f))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.6f), body.X, body.Y, body.Width, body.Height);
            using (var lid = new SolidBrush(pal.MetalDark))
                g.FillEllipse(lid, lx - 2f, top - 5f, w + 4f, 10f);
            var old = g.Clip;
            g.SetClip(body);
            FillPaste(g, body, top, bot - 4f, (float)(_level / 100.0) * 0.85f);
            if (entry == 2)
            {
                // 侧入式：侧壁斜插轴 + 螺旋桨
                float sx = lx, sy = bot - a.Height * 0.16f;
                g.Line(Color.FromArgb(96, 100, 106), 3.4f, sx - 14f, sy + 10f, sx + w * 0.3f, sy - 6f);
                var st = g.Save();
                g.TranslateTransform(sx + w * 0.3f, sy - 6f);
                g.RotateTransform(-24f);
                g.FanWheel(0f, 0f, a.Height * 0.08f, 3, Running ? SpinAngle : 0f, Running, pal);
                g.Restore(st);
                g.Motor(a.Left + 1f, sx - 12f, sy + 14f, a.Height * 0.05f, pal);
            }
            else
            {
                // 顶入 / 底入轴
                if (entry == 0)
                {
                    g.Line(Color.FromArgb(96, 100, 106), 3.2f, cx, a.Top + 8f, cx, bot - 10f);
                    var st = g.Save();
                    g.TranslateTransform(cx, bot - a.Height * 0.18f);
                    g.RotateTransform(Running ? SpinAngle : 0f);
                    using (var pad = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 4f))
                        for (int i = 0; i < 3; i++)
                        {
                            g.RotateTransform(120f);
                            g.DrawLine(pad, 0f, 0f, w * 0.3f, 0f);
                        }
                    g.Restore(st);
                    g.Motor(cx - 14f, cx + 14f, top - 20f, 8f, pal);
                    // 人孔
                    g.Ell(lx + w * 0.26f, top - 2f, 5f, 3.4f, pal.MetalDark);
                }
                else
                {
                    // 底入式：轴从罐底伸入 + 密封座
                    g.Line(Color.FromArgb(96, 100, 106), 3.2f, cx, bot + 6f, cx, top + a.Height * 0.14f);
                    var st = g.Save();
                    g.TranslateTransform(cx, top + a.Height * 0.22f);
                    g.RotateTransform(Running ? SpinAngle : 0f);
                    using (var pad = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.3f)))
                        for (int i = 0; i < 6; i++)
                        {
                            g.RotateTransform(60f);
                            g.FillRectangle(pad, 0f, -3f, w * 0.28f, 6f);
                        }
                    g.Restore(st);
                    g.Box(new RectangleF(cx - 9f, bot - 2f, 18f, 10f), 2f, pal.MetalDark, pal.Edge, 1.2f);
                    g.Motor(cx - 14f, cx + 14f, bot + 18f, 8f, pal);
                }
            }
            g.Clip = old;
        }
        private void DrawMortar(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 灰浆倾翻搅拌机：可倾搅拌桶 + 侧置电机 + 机架轮
            float cx = a.Left + a.Width / 2f;
            float pivY = a.Top + a.Height * 0.46f;
            var st = g.Save();
            g.TranslateTransform(cx, pivY);
            g.RotateTransform(Running ? 0f : -10f);
            // 桶（上宽下窄锥桶）
            float w = a.Width * 0.56f, top = -a.Height * 0.32f, h = a.Height * 0.56f;
            var bucket = new[]
            {
                new PointF(-w / 2f, top), new PointF(w / 2f, top),
                new PointF(w * 0.3f, top + h), new PointF(-w * 0.3f, top + h)
            };
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(bucket);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(-w / 2f, top, w, h),
                    HToolPalettes.Tint(pal.Main, 0.3f), HToolPalettes.Shade(pal.Main, 0.22f), 0f))
                    g.FillPath(b, path);
                var old = g.Clip;
                g.SetClip(path);
                float pct = (float)(_level / 100.0);
                using (var mb = new SolidBrush(Color.FromArgb(120, 110, 96)))
                    g.FillRectangle(mb, -w / 2f, top + h - h * pct * 0.7f, w, h * pct * 0.7f);
                // 桶内桨叶（竖轴）
                g.Line(pal.MetalDark, 3f, 0f, top, 0f, top + h - 6f);
                var stt = g.Save();
                g.TranslateTransform(0f, top + h - 14f);
                g.RotateTransform(Running ? SpinAngle : 0f);
                using (var pad = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 3.4f))
                    for (int i = 0; i < 2; i++)
                    {
                        g.RotateTransform(180f);
                        g.DrawLine(pad, 0f, 0f, w * 0.26f, 0f);
                    }
                g.Restore(stt);
                g.Clip = old;
                g.DrawPath(new Pen(pal.Edge, 1.5f), path);
            }
            g.Restore(st);
            // 枢轴 + 倾翻手柄
            g.Ell(cx, pivY, 4f, 4f, pal.MetalDark);
            g.Line(pal.MetalDark, 3f, cx + a.Width * 0.26f, pivY, a.Right - 4f, pivY - 20f);
            // 电机（桶下侧，经齿轮驱动）
            g.Motor(a.Left + a.Width * 0.12f, a.Left + a.Width * 0.42f, pivY + a.Height * 0.16f,
                a.Height * 0.06f, pal);
            // 机架 + 轮
            g.Line(pal.MetalDark, 4f, cx - a.Width * 0.26f, pivY + 4f,
                a.Left + a.Width * 0.2f, a.Bottom - 10f);
            g.Line(pal.MetalDark, 4f, cx + a.Width * 0.26f, pivY + 4f,
                a.Right - a.Width * 0.2f, a.Bottom - 10f);
            g.Ell(a.Left + a.Width * 0.24f, a.Bottom - 12f, 7f, 7f, Color.FromArgb(60, 64, 70));
            g.Ell(a.Right - a.Width * 0.24f, a.Bottom - 12f, 7f, 7f, Color.FromArgb(60, 64, 70));
        }
    }
}