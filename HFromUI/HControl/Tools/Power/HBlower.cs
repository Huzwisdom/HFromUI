using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Power
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 风机样式：Classic 为直联离心风机（蜗壳+电机），其余 21 种覆盖轴流、罗茨、
    /// 屋顶、箱式、防爆、移动等工业常用风机外形。
    /// </summary>
    public enum HBlowerStyle
    {
        Classic = 0,          // 直联离心风机
        Axial = 1,            // 管道轴流风机
        Roots = 2,            // 罗茨鼓风机
        Roof = 3,             // 屋顶风机
        Cabinet = 4,          // 箱式离心风机
        InlineDuct = 5,       // 管道斜流风机
        Ring = 6,             // 高压漩涡风机
        BeltDrive = 7,        // 皮带传动离心风机
        DoubleInlet = 8,      // 双吸离心风机
        Mobile = 9,           // 移动式风机
        BoilerID = 10,        // 锅炉引风机
        MineLocal = 11,       // 矿用局扇
        DustCyclone = 12,     // 除尘旋风风机
        CoolingTower = 13,    // 冷却塔风机
        HighTemp = 14,        // 耐高温保温风机
        ExProof = 15,         // 防爆离心风机
        ForwardCurve = 16,    // 前倾叶轮风机
        BackwardCurve = 17,   // 后倾叶轮风机
        SquareCasing = 18,    // 方箱离心风机
        Turbo = 19,           // 涡轮增压器风机
        VaneAxial = 20,       // 带导叶轴流风机
        MixedFlow = 21        // 混流风机
    }
    /// <summary>
    /// 风机控件（继承 HToolAnimBase）：22 种风机外形、13 种色调，
    /// Running 时叶轮旋转、出风口有气流；支持文本标签。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("风机控件：22 种风机外形、13 种色调，运行时叶轮旋转并有气流动画")]
    public class HBlower : HToolAnimBase
    {
        private HBlowerStyle _style = HBlowerStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HBlower()
        {
            Size = new Size(150, 140);
        }
        /// <summary>风机外形样式。</summary>
        [HCategoryLanguage("风机"), HDisplayNameLanguage("风机外形样式"), HDescriptionLanguage("风机外形样式"), Browsable(true)]
        [DefaultValue(HBlowerStyle.Classic)]
        public HBlowerStyle BlowerStyle
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
                case HBlowerStyle.Axial: DrawAxial(g, a, pal); break;
                case HBlowerStyle.Roots: DrawRoots(g, a, pal); break;
                case HBlowerStyle.Roof: DrawRoof(g, a, pal); break;
                case HBlowerStyle.Cabinet: DrawCabinet(g, a, pal); break;
                case HBlowerStyle.InlineDuct: DrawInline(g, a, pal, false); break;
                case HBlowerStyle.Ring: DrawRing(g, a, pal); break;
                case HBlowerStyle.BeltDrive: DrawBeltDrive(g, a, pal); break;
                case HBlowerStyle.DoubleInlet: DrawDoubleInlet(g, a, pal); break;
                case HBlowerStyle.Mobile: DrawMobile(g, a, pal); break;
                case HBlowerStyle.BoilerID: DrawBoiler(g, a, pal); break;
                case HBlowerStyle.MineLocal: DrawMine(g, a, pal); break;
                case HBlowerStyle.DustCyclone: DrawCyclone(g, a, pal); break;
                case HBlowerStyle.CoolingTower: DrawCooling(g, a, pal); break;
                case HBlowerStyle.HighTemp: DrawHighTemp(g, a, pal); break;
                case HBlowerStyle.ExProof: DrawExProof(g, a, pal); break;
                case HBlowerStyle.ForwardCurve: DrawCasing(g, a, pal, true, false); break;
                case HBlowerStyle.BackwardCurve: DrawCasing(g, a, pal, false, false); break;
                case HBlowerStyle.SquareCasing: DrawCasing(g, a, pal, false, true); break;
                case HBlowerStyle.Turbo: DrawTurbo(g, a, pal); break;
                case HBlowerStyle.VaneAxial: DrawInline(g, a, pal, true); break;
                case HBlowerStyle.MixedFlow: DrawMixed(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：直联离心风机（蜗壳 + 出风筒 + 电机）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float s = Math.Min(a.Width, a.Height);
            float cx = a.Left + a.Width * 0.42f;
            float cy = a.Top + a.Height * 0.62f;
            float R = s * 0.30f;
            using (var foot = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(foot, cx - R * 0.75f, cy + R * 0.92f, R * 0.5f, s * 0.06f);
                g.FillRectangle(foot, cx + R * 0.25f, cy + R * 0.92f, R * 0.5f, s * 0.06f);
            }
            float ductW = R * 0.72f, ductH = R * 0.85f;
            var duct = new RectangleF(cx + R * 0.35f, cy - R - ductH * 0.45f, ductW, ductH);
            using (var gp = HBarBase.RoundPath(duct, 4f))
            using (var b = HBarBase.CylinderH(duct, pal.MetalDark, pal.Metal))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.4f), gp);
            }
            using (var bb = new SolidBrush(Color.FromArgb(45, 50, 56)))
                g.FillRectangle(bb, duct.X + 5, duct.Y + 4, duct.Width - 10, duct.Height - 8);
            var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
            using (var shell = new GraphicsPath())
            {
                shell.AddArc(casing, -60, 210);
                var c2 = RectangleF.Inflate(casing, -R * 0.10f, -R * 0.10f);
                shell.AddArc(c2, 150, -210);
                shell.CloseFigure();
                using (var b = new LinearGradientBrush(casing, pal.Metal, pal.MetalDark, 45f))
                    g.FillPath(b, shell);
                using (var pen = new Pen(pal.Edge, 1.6f))
                    g.DrawPath(pen, shell);
            }
            float rIn = R * 0.72f;
            g.FanWheel(cx, cy, rIn, 8, SpinAngle, Running, pal);
            g.Line(pal.MetalDark, 4f, cx + rIn * 0.9f, cy, cx + rIn * 1.25f, cy);
            var motor = new RectangleF(cx + rIn * 1.2f, cy - R * 0.42f, R * 1.05f, R * 0.84f);
            g.Motor(motor.Left, motor.Right, cy, motor.Height / 2f, pal);
            using (var box = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.15f)))
                g.FillRectangle(box, motor.X + motor.Width * 0.25f, motor.Y - s * 0.05f, motor.Width * 0.3f, s * 0.05f);
            DrawAirUp(g, duct, pal);
        }
        // ----------------------------------------------------------------
        // 新式风机
        // ----------------------------------------------------------------
        private void DrawAxial(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.56f, cx = a.Left + a.Width * 0.5f;
            float rh = a.Height * 0.26f, x1 = a.Left + a.Width * 0.14f, x2 = a.Right - a.Width * 0.14f;
            // 支腿
            g.Line(pal.MetalDark, 4f, x1 + 6f, cy + rh, x1 + 6f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 4f, x2 - 6f, cy + rh, x2 - 6f, a.Bottom - 4f);
            // 圆筒风筒
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.FillCylH(tube, pal.MetalDark, Color.White);
            g.DrawR(tube, 4f, pal.Edge, 1.4f);
            // 筒内风轮
            float fr = rh * 0.8f;
            using (var dark = new SolidBrush(Color.FromArgb(50, 54, 60)))
                g.FillEllipse(dark, cx - fr, cy - fr, fr * 2f, fr * 2f);
            DrawBlades(g, cx, cy, fr, 6, SpinAngle);
            g.Ell(cx, cy, fr * 0.16f, fr * 0.16f, pal.Accent);
            // 接线盒
            g.Box(new RectangleF(cx - 9f, cy - rh - 12f, 18f, 10f), 2f, pal.MetalDark, pal.Edge, 1f);
            DrawAirSide(g, x2 + 2f, cy, rh, pal, 1f);
        }
        private void DrawRoots(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f;
            float x1 = a.Left + a.Width * 0.12f, x2 = a.Left + a.Width * 0.62f;
            float rh = a.Height * 0.24f;
            // 腰形壳体（两圆相交）
            var casing = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.Box(casing, rh, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.6f);
            // 双转子（8 字形）
            float rr = rh * 0.52f, my1 = cy - rh * 0.42f, my2 = cy + rh * 0.42f;
            using (var rot = new SolidBrush(pal.MetalDark))
            {
                g.FillEllipse(rot, casing.Left + casing.Width * 0.3f - rr, my1 - rr, rr * 2f, rr * 2f);
                g.FillEllipse(rot, casing.Left + casing.Width * 0.62f - rr, my2 - rr, rr * 2f, rr * 2f);
            }
            g.FillEllipse(Brushes.White, casing.Left + casing.Width * 0.3f - rr * 0.4f, my1 - rr * 0.4f, rr * 0.8f, rr * 0.8f);
            g.FillEllipse(Brushes.White, casing.Left + casing.Width * 0.62f - rr * 0.4f, my2 - rr * 0.4f, rr * 0.8f, rr * 0.8f);
            // 进排气口
            g.Box(new RectangleF(x1 + 8f, cy - rh - 12f, casing.Width * 0.3f, 12f), 2f, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(x2 - casing.Width * 0.3f - 8f, cy + rh, casing.Width * 0.3f, 12f), 2f, pal.Metal, pal.Edge, 1.1f);
            // 皮带轮 + 电机
            float py = cy;
            g.Ell(x2 + 10f, py, rh * 0.42f, rh * 0.42f, pal.MetalDark);
            g.Line(pal.Edge, 2f, x2 + 10f - rh * 0.3f, py + rh * 0.2f, x2 + 10f + rh * 0.3f, py + rh * 0.2f);
            var mot = new RectangleF(x2 + 22f, cy - rh * 0.42f, a.Right - x2 - 26f, rh * 0.84f);
            g.Motor(mot.Left, mot.Right, cy, mot.Height / 2f, pal);
            g.BasePlate(a.Left + a.Width * 0.5f, a.Width * 0.82f, a.Bottom - 12f, 8f, pal.MetalDark);
        }
        private void DrawRoof(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            float baseY = a.Bottom - 6f, throatW = a.Width * 0.34f;
            // 风筒座
            g.Box(new RectangleF(cx - throatW / 2f, baseY - a.Height * 0.22f, throatW, a.Height * 0.22f),
                3f, pal.Metal, pal.Edge, 1.3f);
            // 蘑菇帽
            float capY = baseY - a.Height * 0.62f;
            g.Poly(new[]
            {
                new PointF(cx - throatW * 0.95f, capY + a.Height * 0.22f),
                new PointF(cx, capY),
                new PointF(cx + throatW * 0.95f, capY + a.Height * 0.22f)
            }, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.5f);
            g.Line(pal.Edge, 3f, cx, capY - 6f, cx, capY);
            // 内部叶轮（从帽下可见）
            float fr = throatW * 0.34f, fy = capY + a.Height * 0.26f;
            DrawBlades(g, cx, fy, fr, 5, SpinAngle);
            g.Ell(cx, fy, fr * 0.16f, fr * 0.16f, pal.Accent);
            // 底座法兰
            g.Box(new RectangleF(cx - throatW * 0.72f, baseY - 6f, throatW * 1.44f, 6f), 1f, pal.MetalDark, pal.Edge, 1f);
        }
        private void DrawCabinet(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.12f, -a.Height * 0.08f);
            g.Box(box, 6f, HToolPalettes.Tint(pal.Main, 0.28f), pal.Edge, 1.6f);
            // 正面大圆窗 + 叶轮
            float cx = box.Left + box.Width * 0.38f, cy = box.Top + box.Height * 0.52f, fr = box.Height * 0.32f;
            using (var d = new SolidBrush(Color.FromArgb(52, 56, 62)))
                g.FillEllipse(d, cx - fr, cy - fr, fr * 2f, fr * 2f);
            DrawBlades(g, cx, cy, fr * 0.92f, 6, SpinAngle);
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - fr, cy - fr, fr * 2f, fr * 2f);
            g.Ell(cx, cy, fr * 0.15f, fr * 0.15f, pal.Accent);
            // 出风格栅
            float gx = box.Right - box.Width * 0.22f;
            using (var lou = new Pen(pal.MetalDark, 2.4f))
                for (int i = 0; i < 5; i++)
                    g.DrawLine(lou, gx - 10f, box.Top + 12f + i * 9f, gx + 10f, box.Top + 12f + i * 9f);
            // 支脚
            g.Line(pal.MetalDark, 4f, box.Left + 10f, box.Bottom, box.Left + 10f, a.Bottom - 3f);
            g.Line(pal.MetalDark, 4f, box.Right - 10f, box.Bottom, box.Right - 10f, a.Bottom - 3f);
        }
        private void DrawInline(Graphics g, RectangleF a, HToolPalette pal, bool vanes)
        {
            float cy = a.Top + a.Height * 0.52f, cx = a.Left + a.Width * 0.5f;
            float rh = a.Height * 0.26f, x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.08f;
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.FillCylH(tube, pal.MetalDark, Color.White);
            g.DrawR(tube, 6f, pal.Edge, 1.4f);
            // 两端法兰
            g.Flange(new RectangleF(x1 - 8f, cy - rh - 3f, 10f, rh * 2f + 6f), pal);
            g.Flange(new RectangleF(x2 - 2f, cy - rh - 3f, 10f, rh * 2f + 6f), pal);
            float fr = rh * 0.78f;
            using (var d = new SolidBrush(Color.FromArgb(50, 54, 60)))
                g.FillEllipse(d, cx - fr, cy - fr, fr * 2f, fr * 2f);
            DrawBlades(g, cx, cy, fr, vanes ? 7 : 5, SpinAngle);
            g.Ell(cx, cy, fr * 0.15f, fr * 0.15f, pal.Accent);
            if (vanes)
                using (var v = new Pen(HToolPalettes.Shade(pal.Metal, 0.3f), 2.2f))
                    for (int i = 0; i < 6; i++)
                    {
                        double an = i * Math.PI / 3.0;
                        g.DrawLine(v, cx + (float)Math.Cos(an) * fr * 0.55f, cy + (float)Math.Sin(an) * fr * 0.55f,
                            cx + (float)Math.Cos(an) * fr * 1.02f, cy + (float)Math.Sin(an) * fr * 1.02f);
                    }
            DrawAirSide(g, x2 + 10f, cy, rh, pal, 1f);
        }
        private void DrawRing(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.46f, cy = a.Top + a.Height * 0.58f, r = a.Height * 0.26f;
            // 环形泵体（甜甜圈）
            g.Ell(cx, cy, r, r, HToolPalettes.Tint(pal.Main, 0.25f));
            g.Ell(cx, cy, r * 0.52f, r * 0.52f, Color.FromArgb(60, 64, 70));
            g.DrawEllipse(new Pen(pal.Edge, 1.5f), cx - r, cy - r, r * 2f, r * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r * 0.52f, cy - r * 0.52f, r * 1.04f, r * 1.04f);
            // 进出气口
            g.Box(new RectangleF(cx - r * 0.35f, cy - r - 14f, r * 0.7f, 14f), 2f, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(cx + r, cy - 8f, 14f, 16f), 2f, pal.Metal, pal.Edge, 1.1f);
            // 顶置电机
            var mot = new RectangleF(cx - r * 1.1f, a.Top + 8f, r * 2.2f, r * 0.72f);
            g.Motor(mot.Left, mot.Right, mot.Top + mot.Height / 2f, mot.Height / 2f, pal);
            g.Line(pal.Edge, 3f, cx, mot.Bottom, cx, cy - r - 14f);
            g.BasePlate(cx, r * 2.6f, cy + r, a.Bottom - cy - r, pal.MetalDark);
            DrawAirSide(g, cx + r + 14f, cy, r * 0.4f, pal, 1f);
        }
        private void DrawBeltDrive(Graphics g, RectangleF a, HToolPalette pal)
        {
            float s = Math.Min(a.Width, a.Height);
            float cx = a.Left + a.Width * 0.36f, cy = a.Top + a.Height * 0.5f, R = s * 0.28f;
            // 蜗壳（同 Classic 轮廓）
            var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
            using (var shell = new GraphicsPath())
            {
                shell.AddArc(casing, -60, 210);
                shell.AddArc(RectangleF.Inflate(casing, -R * 0.1f, -R * 0.1f), 150, -210);
                shell.CloseFigure();
                using (var b = new LinearGradientBrush(casing, pal.Metal, pal.MetalDark, 45f))
                    g.FillPath(b, shell);
                g.DrawPath(new Pen(pal.Edge, 1.6f), shell);
            }
            float rIn = R * 0.72f;
            g.FanWheel(cx, cy, rIn, 8, SpinAngle, Running, pal);
            // 大皮带轮（轴端）
            float pw = R * 0.55f;
            g.Ell(cx - rIn * 0.2f, cy, pw, pw, pal.MetalDark);
            // 电机 + 小轮 + 皮带
            float mx = a.Right - a.Width * 0.16f, mr = R * 0.32f;
            g.Ell(mx, cy, mr, mr, pal.MetalDark);
            using (var belt = new Pen(Color.FromArgb(46, 48, 54), 5f))
            {
                g.DrawLine(belt, cx - rIn * 0.2f, cy - pw * 0.8f, mx, cy - mr * 0.8f);
                g.DrawLine(belt, cx - rIn * 0.2f, cy + pw * 0.8f, mx, cy + mr * 0.8f);
            }
            var mot = new RectangleF(mx - R * 0.6f, cy + mr * 0.4f, R * 1.2f, R * 0.62f);
            g.Motor(mot.Left, mot.Right, mot.Top + mot.Height / 2f, mot.Height / 2f, pal);
            // 滑轨底座
            g.Line(pal.MetalDark, 5f, a.Left + a.Width * 0.1f, a.Bottom - 8f, a.Right - a.Width * 0.08f, a.Bottom - 8f);
        }
        private void DrawDoubleInlet(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.42f, cy = a.Top + a.Height * 0.58f, R = Math.Min(a.Width, a.Height) * 0.26f;
            var casing = new RectangleF(cx - R * 1.25f, cy - R, R * 2.5f, R * 2f);
            g.Box(casing, 8f, pal.Metal, pal.Edge, 1.6f);
            // 双侧进风喇叭口
            g.FillEllipse(Brushes.White, cx - R * 1.5f, cy - R * 0.7f, R * 0.9f, R * 1.4f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - R * 1.5f, cy - R * 0.7f, R * 0.9f, R * 1.4f);
            g.FillEllipse(Brushes.White, cx + R * 0.6f, cy - R * 0.7f, R * 0.9f, R * 1.4f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx + R * 0.6f, cy - R * 0.7f, R * 0.9f, R * 1.4f);
            g.FanWheel(cx, cy, R * 0.68f, 8, SpinAngle, Running, pal);
            // 顶出风筒 + 电机
            var duct = new RectangleF(cx - R * 0.36f, cy - R - R * 0.8f, R * 0.72f, R * 0.8f);
            g.Box(duct, 3f, pal.Metal, pal.Edge, 1.3f);
            var mot = new RectangleF(cx + R * 0.7f, cy - R * 0.35f, R * 1.1f, R * 0.7f);
            g.Motor(mot.Left, mot.Right, cy, mot.Height / 2f, pal);
            DrawAirUp(g, duct, pal);
        }
        private void DrawMobile(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f, x1 = a.Left + a.Width * 0.1f, x2 = a.Right - a.Width * 0.34f;
            float rh = a.Height * 0.24f;
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.FillCylH(tube, pal.MetalDark, Color.White);
            g.DrawR(tube, 5f, pal.Edge, 1.4f);
            float fr = rh * 0.8f, fx = (x1 + x2) / 2f;
            DrawBlades(g, fx, cy, fr, 6, SpinAngle);
            g.Ell(fx, cy, fr * 0.15f, fr * 0.15f, pal.Accent);
            // 后部电机/机架
            g.Motor(x2 - 4f, a.Right - a.Width * 0.06f, cy, rh * 0.42f, pal);
            // 推把
            g.Line(pal.Edge, 4f, a.Right - a.Width * 0.1f, cy + rh, a.Right - a.Width * 0.02f, cy - rh * 1.1f);
            // 轮组
            float wy = a.Bottom - 12f;
            g.Ell(x1 + 16f, wy, 11f, 11f, Color.FromArgb(60, 64, 70));
            g.Ell(x2 - 10f, wy, 11f, 11f, Color.FromArgb(60, 64, 70));
            g.Line(pal.MetalDark, 4f, x1 + 16f, cy + rh, x2 - 10f, wy);
            DrawAirSide(g, x1 - 2f, cy, rh, pal, -1f);
        }
        private void DrawBoiler(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.52f, cy = a.Top + a.Height * 0.56f, R = Math.Min(a.Width, a.Height) * 0.3f;
            // 大型进风箱（矩形）+ 蜗壳
            g.Box(new RectangleF(cx - R * 1.3f, cy - R * 0.9f, R * 1.1f, R * 1.8f), 4f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.5f);
            var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
            using (var shell = new GraphicsPath())
            {
                shell.AddArc(casing, -60, 210);
                shell.AddArc(RectangleF.Inflate(casing, -R * 0.1f, -R * 0.1f), 150, -210);
                shell.CloseFigure();
                using (var b = new LinearGradientBrush(casing, pal.Metal, pal.MetalDark, 45f))
                    g.FillPath(b, shell);
                g.DrawPath(new Pen(pal.Edge, 1.6f), shell);
            }
            g.FanWheel(cx, cy, R * 0.7f, 10, SpinAngle, Running, pal);
            // 顶部保温烟囱接口
            var stack = new RectangleF(cx + R * 0.4f, a.Top + 2f, R * 0.6f, R * 0.8f);
            g.Box(stack, 3f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // 水冷轴承座
            g.Ell(cx - R * 0.7f, cy, R * 0.22f, R * 0.22f, pal.MetalDark);
            g.BasePlate(cx, R * 2.6f, cy + R, a.Bottom - cy - R - 2f, pal.MetalDark);
            DrawAirUp(g, stack, pal);
        }
        private void DrawMine(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = a.Left + a.Width * 0.1f, x2 = a.Right - a.Width * 0.1f;
            float rh = a.Height * 0.3f;
            // 长风筒（压花铁皮圈）
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.FillCylH(tube, HToolPalettes.Shade(pal.Main, 0.25f), HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawR(tube, 4f, pal.Edge, 1.4f);
            using (var ring = new Pen(HToolPalettes.Shade(pal.Main, 0.4f), 2f))
                for (float x = x1 + 14f; x < x2 - 6f; x += 22f)
                    g.DrawLine(ring, x, cy - rh + 2, x, cy + rh - 2);
            float fr = rh * 0.72f;
            DrawBlades(g, x1 + fr + 4f, cy, fr, 6, SpinAngle);
            g.Ell(x1 + fr + 4f, cy, fr * 0.16f, fr * 0.16f, pal.Accent);
            // 三角支架
            g.Line(pal.MetalDark, 4f, x1 + 20f, cy + rh, x1 + 8f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 4f, x1 + 20f, cy + rh, x1 + 34f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 4f, x2 - 20f, cy + rh, x2 - 34f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 4f, x2 - 20f, cy + rh, x2 - 8f, a.Bottom - 4f);
            DrawAirSide(g, x2, cy, rh, pal, 1f);
        }
        private void DrawCyclone(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.58f, w = a.Width * 0.34f, top = a.Top + a.Height * 0.12f;
            float cyTop = top + a.Height * 0.34f, tip = a.Bottom - 8f;
            // 旋风筒（圆柱 + 锥）
            g.Poly(new[]
            {
                new PointF(cx - w / 2f, top), new PointF(cx + w / 2f, top),
                new PointF(cx + w / 2f, cyTop), new PointF(cx + w * 0.12f, tip),
                new PointF(cx - w * 0.12f, tip), new PointF(cx - w / 2f, cyTop)
            }, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.5f);
            // 中心排气筒
            g.Box(new RectangleF(cx - w * 0.14f, top - 16f, w * 0.28f, 18f), 2f, pal.Metal, pal.Edge, 1.1f);
            // 蜗壳进口（左侧）
            float R = a.Height * 0.17f;
            var casing = new RectangleF(a.Left + a.Width * 0.08f, top + 2f, R * 2f, R * 2f);
            g.Box(casing, 6f, pal.Metal, pal.Edge, 1.4f);
            float fc = casing.Left + R;
            g.FanWheel(fc, casing.Top + R, R * 0.72f, 8, SpinAngle, Running, pal);
            // 进口管连旋风筒
            g.Poly(new[]
            {
                new PointF(fc + R * 0.6f, casing.Top + R * 0.7f),
                new PointF(cx - w / 2f, top + 6f),
                new PointF(cx - w / 2f, top + 22f),
                new PointF(fc + R * 0.6f, casing.Top + R * 1.2f)
            }, pal.Metal, pal.Edge, 1.2f);
            // 卸灰阀
            g.Box(new RectangleF(cx - w * 0.16f, tip, w * 0.32f, 8f), 2f, pal.MetalDark, pal.Edge, 1f);
        }
        private void DrawCooling(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, w = a.Width * 0.72f, lx = cx - w / 2f;
            float top = a.Top + a.Height * 0.3f, bot = a.Bottom - 6f;
            // 双曲线塔体（上宽下窄鼓形）
            using (var tower = new GraphicsPath())
            {
                tower.AddBezier(lx, top, lx - w * 0.12f, top + (bot - top) * 0.4f,
                    lx + w * 0.08f, bot, lx + w * 0.16f, bot);
                tower.AddLine(lx + w * 0.16f, bot, lx + w * 0.84f, bot);
                tower.AddBezier(lx + w * 0.84f, bot, lx + w * 0.92f, bot,
                    lx + w * 1.12f, top + (bot - top) * 0.4f, lx + w, top);
                tower.CloseFigure();
                using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.5f)))
                    g.FillPath(b, tower);
                g.DrawPath(new Pen(pal.Edge, 1.5f), tower);
            }
            // 顶部风筒 + 大叶轮
            g.Box(new RectangleF(cx - w * 0.36f, top - a.Height * 0.16f, w * 0.72f, a.Height * 0.14f),
                4f, pal.MetalDark, pal.Edge, 1.3f);
            DrawBlades(g, cx, top - a.Height * 0.09f, w * 0.26f, 6, SpinAngle);
            g.Ell(cx, top - a.Height * 0.09f, w * 0.05f, w * 0.05f, pal.Accent);
            // 百叶
            using (var lou = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 2f))
                for (int i = 0; i < 4; i++)
                    g.DrawLine(lou, lx + w * 0.2f, bot - 14f - i * 10f, lx + w * 0.8f, bot - 14f - i * 10f);
        }
        private void DrawHighTemp(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.42f, cy = a.Top + a.Height * 0.6f, R = Math.Min(a.Width, a.Height) * 0.27f;
            // 保温棉外壳（米黄色厚壳）
            var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
            using (var shell = new GraphicsPath())
            {
                shell.AddArc(casing, -60, 210);
                shell.AddArc(RectangleF.Inflate(casing, -R * 0.1f, -R * 0.1f), 150, -210);
                shell.CloseFigure();
                using (var b = new SolidBrush(Color.FromArgb(214, 196, 158)))
                    g.FillPath(b, shell);
                g.DrawPath(new Pen(Color.FromArgb(150, 120, 80), 2.4f), shell);
            }
            g.FanWheel(cx, cy, R * 0.66f, 8, SpinAngle, Running, pal);
            // 膨胀节（波纹软管）
            float dx = cx + R * 0.5f;
            using (var bel = new Pen(Color.FromArgb(120, 80, 60), 2.4f))
                for (int i = 0; i < 4; i++)
                    g.DrawArc(bel, dx + i * 7f - 4f, cy - R - 10f, 8f, 20f, 0f, 180f);
            // 出风筒
            var duct = new RectangleF(dx + 26f, cy - R - 8f, R * 0.6f, R * 0.8f);
            g.Box(duct, 3f, Color.FromArgb(214, 196, 158), Color.FromArgb(150, 120, 80), 1.4f);
            // 电机 + 联轴器加长套
            g.Line(Color.FromArgb(120, 124, 130), 6f, cx + R * 0.7f, cy, cx + R * 1.1f, cy);
            var mot = new RectangleF(cx + R * 1.1f, cy - R * 0.4f, R * 1.05f, R * 0.8f);
            g.Motor(mot.Left, mot.Right, cy, mot.Height / 2f, pal);
            // 高温红光
            if (Running)
                using (var glow = new SolidBrush(Color.FromArgb((int)(60 + 40 * Math.Abs(Math.Sin(Phase * 2f))), Color.OrangeRed)))
                    g.FillEllipse(glow, duct.X, duct.Y - 3f, duct.Width, duct.Height + 6f);
        }
        private void DrawExProof(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.4f, cy = a.Top + a.Height * 0.6f, R = Math.Min(a.Width, a.Height) * 0.29f;
            var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
            using (var shell = new GraphicsPath())
            {
                shell.AddArc(casing, -60, 210);
                shell.AddArc(RectangleF.Inflate(casing, -R * 0.1f, -R * 0.1f), 150, -210);
                shell.CloseFigure();
                using (var b = new LinearGradientBrush(casing,
                    HToolPalettes.Shade(pal.Main, 0.35f), HToolPalettes.Shade(pal.Main, 0.1f), 45f))
                    g.FillPath(b, shell);
                g.DrawPath(new Pen(pal.Edge, 2.2f), shell);
            }
            g.FanWheel(cx, cy, R * 0.7f, 8, SpinAngle, Running, pal);
            // 厚重法兰螺栓圈
            for (int i = 0; i < 8; i++)
            {
                double an = i * Math.PI / 4.0;
                g.Bolt(cx + (float)Math.Cos(an) * R * 0.92f, cy + (float)Math.Sin(an) * R * 0.92f, 2.4f, pal.Edge);
            }
            var duct = new RectangleF(cx + R * 0.4f, cy - R - R * 0.7f, R * 0.66f, R * 0.7f);
            g.Box(duct, 3f, pal.MetalDark, pal.Edge, 1.6f);
            var mot = new RectangleF(cx + R * 1.15f, cy - R * 0.42f, R, R * 0.84f);
            g.Motor(mot.Left, mot.Right, cy, mot.Height / 2f, pal);
            // EX 标牌
            var tag = new RectangleF(casing.Left + 4f, casing.Bottom - 16f, 24f, 12f);
            g.FillR(tag, 2f, Color.FromArgb(200, 40, 40));
            HBarBase.DrawText(g, "Ex", new Font("微软雅黑", 7f, FontStyle.Bold), Color.White, tag,
                ContentAlignment.MiddleCenter);
            DrawAirUp(g, duct, pal);
        }
        private void DrawCasing(Graphics g, RectangleF a, HToolPalette pal, bool forward, bool square)
        {
            float cx = a.Left + a.Width * 0.44f, cy = a.Top + a.Height * 0.6f, R = Math.Min(a.Width, a.Height) * 0.28f;
            if (square)
            {
                g.Box(new RectangleF(cx - R, cy - R, R * 2f, R * 2f), 4f, pal.Metal, pal.Edge, 1.6f);
            }
            else
            {
                var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
                using (var shell = new GraphicsPath())
                {
                    shell.AddArc(casing, -60, 210);
                    shell.AddArc(RectangleF.Inflate(casing, -R * 0.1f, -R * 0.1f), 150, -210);
                    shell.CloseFigure();
                    using (var b = new LinearGradientBrush(casing, pal.Metal, pal.MetalDark, 45f))
                        g.FillPath(b, shell);
                    g.DrawPath(new Pen(pal.Edge, 1.6f), shell);
                }
            }
            float rIn = R * 0.7f;
            using (var plate = new SolidBrush(Color.FromArgb(238, 240, 242)))
                g.FillEllipse(plate, cx - rIn, cy - rIn, rIn * 2f, rIn * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - rIn, cy - rIn, rIn * 2f, rIn * 2f);
            // 前/后倾叶片（扫掠方向相反）
            float ri = rIn * 0.26f, ro = rIn * 0.9f, dir = forward ? -1f : 1f;
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var blade = new SolidBrush(Running ? Color.FromArgb(74, 80, 88) : Color.FromArgb(150, 154, 160)))
            {
                for (int i = 0; i < (forward ? 12 : 8); i++)
                {
                    double a0 = i * Math.PI * 2.0 / (forward ? 12 : 8);
                    g.FillPolygon(blade, new[]
                    {
                        new PointF((float)Math.Cos(a0 + dir * 0.1) * ri, (float)Math.Sin(a0 + dir * 0.1) * ri),
                        new PointF((float)Math.Cos(a0 + dir * 0.22) * ro, (float)Math.Sin(a0 + dir * 0.22) * ro),
                        new PointF((float)Math.Cos(a0 + dir * 0.55) * ro, (float)Math.Sin(a0 + dir * 0.55) * ro),
                        new PointF((float)Math.Cos(a0 + dir * 0.34) * ri, (float)Math.Sin(a0 + dir * 0.34) * ri)
                    });
                }
            }
            g.Restore(st);
            g.Ell(cx, cy, rIn * 0.18f, rIn * 0.18f, pal.Accent);
            var duct = new RectangleF(cx + R * 0.42f, cy - R - R * 0.72f, R * 0.64f, R * 0.72f);
            g.Box(duct, 3f, pal.Metal, pal.Edge, 1.3f);
            var mot = new RectangleF(cx + R * 1.05f, cy - R * 0.4f, R * 1.1f, R * 0.8f);
            g.Motor(mot.Left, mot.Right, cy, mot.Height / 2f, pal);
            DrawAirUp(g, duct, pal);
        }
        private void DrawTurbo(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.46f, cy = a.Top + a.Height * 0.58f, R = Math.Min(a.Width, a.Height) * 0.24f;
            // 涡轮壳（小蜗牛）
            var casing = new RectangleF(cx - R, cy - R, R * 2f, R * 2f);
            using (var shell = new GraphicsPath())
            {
                shell.AddArc(casing, -40, 240);
                shell.AddArc(RectangleF.Inflate(casing, -R * 0.34f, -R * 0.34f), 200, -240);
                shell.CloseFigure();
                using (var b = new LinearGradientBrush(casing, HToolPalettes.Tint(pal.Main, 0.3f),
                    HToolPalettes.Shade(pal.Main, 0.3f), 45f))
                    g.FillPath(b, shell);
                g.DrawPath(new Pen(pal.Edge, 1.6f), shell);
            }
            g.FanWheel(cx, cy, R * 0.5f, 10, SpinAngle * 1.6f, Running, pal);
            // 压气机壳（另一侧镜像小壳）
            float cx2 = cx - R * 1.7f;
            var c2 = new RectangleF(cx2 - R * 0.8f, cy - R * 0.8f, R * 1.6f, R * 1.6f);
            g.Box(c2, R * 0.5f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.4f);
            g.FanWheel(cx2, cy, R * 0.4f, 8, -SpinAngle, Running, pal);
            // 空气滤芯（圆筒）
            g.FillCylH(new RectangleF(a.Left + 4f, cy - R * 0.34f, R * 0.9f, R * 0.68f),
                HToolPalettes.Shade(pal.Main, 0.3f), HToolPalettes.Tint(pal.Main, 0.3f));
            // 排气管
            g.Line(pal.MetalDark, 4f, cx + R * 0.6f, cy - R * 0.5f, a.Right - 6f, cy - R * 0.5f);
            g.Line(pal.MetalDark, 4f, a.Right - 6f, cy - R * 0.5f, a.Right - 6f, a.Top + 4f);
            DrawAirUp(g, new RectangleF(a.Right - 12f, a.Top - 4f, 10f, R * 0.5f), pal);
        }
        private void DrawMixed(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.54f, cx = a.Left + a.Width * 0.5f;
            float x1 = a.Left + a.Width * 0.1f, x2 = a.Right - a.Width * 0.1f;
            float rh1 = a.Height * 0.3f, rh2 = a.Height * 0.2f;
            // 混流筒（两端喇叭口 + 鼓形中段）
            using (var tube = new GraphicsPath())
            {
                tube.AddLine(x1, cy - rh1, x1 + a.Width * 0.22f, cy - rh2);
                tube.AddLine(x1 + a.Width * 0.22f, cy - rh2, x2 - a.Width * 0.22f, cy - rh2);
                tube.AddLine(x2 - a.Width * 0.22f, cy - rh2, x2, cy - rh1);
                tube.AddLine(x2, cy + rh1, x2 - a.Width * 0.22f, cy + rh2);
                tube.AddLine(x2 - a.Width * 0.22f, cy + rh2, x1 + a.Width * 0.22f, cy + rh2);
                tube.AddLine(x1 + a.Width * 0.22f, cy + rh2, x1, cy + rh1);
                tube.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(x1, 0, x2 - x1, 1), pal.MetalDark, Color.White, 0f))
                    g.FillPath(b, tube);
                g.DrawPath(new Pen(pal.Edge, 1.5f), tube);
            }
            DrawBlades(g, cx, cy, rh2 * 0.9f, 6, SpinAngle);
            g.Ell(cx, cy, rh2 * 0.16f, rh2 * 0.16f, pal.Accent);
            g.Flange(new RectangleF(x1 - 8f, cy - rh1 - 2f, 10f, rh1 * 2f + 4f), pal);
            g.Flange(new RectangleF(x2 - 2f, cy - rh1 - 2f, 10f, rh1 * 2f + 4f), pal);
            g.Line(pal.MetalDark, 4f, cx - 10f, cy + rh2, cx - 10f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 4f, cx + 10f, cy + rh2, cx + 10f, a.Bottom - 4f);
            DrawAirSide(g, x2 + 8f, cy, rh1, pal, 1f);
        }
        // ----------------------------------------------------------------
        // 共享小原语
        // ----------------------------------------------------------------
        private void DrawBlades(Graphics g, float cx, float cy, float r, int n, float angle)
        {
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            using (var blade = new SolidBrush(Running ? Color.FromArgb(82, 88, 96) : Color.FromArgb(150, 154, 160)))
            {
                float ri = r * 0.24f;
                for (int i = 0; i < n; i++)
                {
                    double a0 = i * Math.PI * 2.0 / n;
                    g.FillPolygon(blade, new[]
                    {
                        new PointF((float)Math.Cos(a0) * ri, (float)Math.Sin(a0) * ri),
                        new PointF((float)Math.Cos(a0 + 0.3f) * r, (float)Math.Sin(a0 + 0.3f) * r),
                        new PointF((float)Math.Cos(a0 + 0.62f) * r, (float)Math.Sin(a0 + 0.62f) * r),
                        new PointF((float)Math.Cos(a0 + 0.22f) * ri, (float)Math.Sin(a0 + 0.22f) * ri)
                    });
                }
            }
            g.Restore(st);
        }
        private void DrawAirUp(Graphics g, RectangleF duct, HToolPalette pal)
        {
            if (!Running) return;
            using (var air = new Pen(pal.Accent, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                for (int i = 0; i < 3; i++)
                {
                    float t = (Phase / (float)Math.PI * 0.5f + i / 3f) % 1f;
                    float ay = duct.Y - 6 - t * 20f;
                    air.Color = Color.FromArgb(Math.Max(40, (int)(200 * (1f - t))), pal.Accent);
                    g.DrawLine(air, duct.Left + duct.Width * 0.3f, ay, duct.Left + duct.Width * 0.3f, ay - 7);
                    g.DrawLine(air, duct.Left + duct.Width * 0.7f, ay + 3, duct.Left + duct.Width * 0.7f, ay - 4);
                }
            }
        }
        private void DrawAirSide(Graphics g, float x, float cy, float rh, HToolPalette pal, float dir)
        {
            if (!Running) return;
            using (var air = new Pen(pal.Accent, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                for (int i = 0; i < 3; i++)
                {
                    float t = (Phase / (float)Math.PI * 0.5f + i / 3f) % 1f;
                    float wx = x + dir * (6f + t * 18f);
                    air.Color = Color.FromArgb(Math.Max(40, (int)(220 * (1f - t))), pal.Accent);
                    g.DrawArc(air, wx - 5, cy - rh * 0.45f - 3, 10, rh, -60f, 120f);
                    g.DrawArc(air, wx - 5, cy - 3, 10, rh, -60f, 120f);
                }
            }
        }
    }
}