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
    /// 搅拌器具样式：Classic 为带柄头玻璃搅拌棒，其余 21 种覆盖磁力搅拌子、桨式/锚式/
    /// 涡轮/螺旋搅拌、顶置搅拌器、涡旋仪、加热台、均质机、螺带机、摇床、研钵等。
    /// </summary>
    public enum HStirRodStyle
    {
        Classic = 0,        // 带柄头玻璃搅拌棒
        MagneticBar = 1,    // 磁力搅拌子（杯中）
        Paddle = 2,         // 桨式搅拌
        Anchor = 3,         // 锚式搅拌
        Turbine = 4,        // 涡轮圆盘搅拌
        Helix = 5,          // 螺旋带搅拌
        StraightRod = 6,    // 直玻璃棒
        Spatula = 7,        // 药勺
        OverheadMixer = 8,  // 顶置机械搅拌器
        Vortex = 9,         // 涡旋振荡器
        Hotplate = 10,      // 磁力加热台
        Homogenizer = 11,   // 均质乳化机
        RibbonBlender = 12, // 立式螺带混合机
        DrumRoller = 13,    // 滚瓶机
        Shaker = 14,        // 往复摇床
        Propeller = 15,     // 三叶推进桨
        Retriever = 16,     // 磁子取出棒
        Mortar = 17,        // 研钵与研杵
        Whisk = 18,         // 丝笼搅棒
        StaticMixer = 19,   // 静态混合管
        AirStone = 20,      // 曝气石
        Centrifuge = 21     // 离心机
    }
    /// <summary>
    /// 搅拌棒控件（继承 HToolAnimBase）：22 种搅拌混合器具外形、13 种色调，
    /// Running 时棒身摆动、桨叶旋转、旋弧扩散或气泡/颗粒运动。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("搅拌器具控件：22 种搅拌外形、13 种色调，运行时摆动旋转并显示搅拌旋弧")]
    public class HStirRod : HToolAnimBase
    {
        private HStirRodStyle _style = HStirRodStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HStirRod()
        {
            Size = new Size(72, 170);
        }
        /// <summary>搅拌器具外形样式。</summary>
        [HCategoryLanguage("搅拌棒"), HDisplayNameLanguage("搅拌器具外形样式"), HDescriptionLanguage("搅拌器具外形样式"), Browsable(true)]
        [DefaultValue(HStirRodStyle.Classic)]
        public HStirRodStyle StirRodStyle
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
                case HStirRodStyle.MagneticBar: DrawMagnetic(g, a, pal); break;
                case HStirRodStyle.Paddle: DrawPaddle(g, a, pal); break;
                case HStirRodStyle.Anchor: DrawAnchor(g, a, pal); break;
                case HStirRodStyle.Turbine: DrawTurbine(g, a, pal); break;
                case HStirRodStyle.Helix: DrawHelix(g, a, pal, false); break;
                case HStirRodStyle.StraightRod: DrawStraight(g, a, pal); break;
                case HStirRodStyle.Spatula: DrawSpatula(g, a, pal); break;
                case HStirRodStyle.OverheadMixer: DrawOverhead(g, a, pal); break;
                case HStirRodStyle.Vortex: DrawVortex(g, a, pal); break;
                case HStirRodStyle.Hotplate: DrawHotplate(g, a, pal); break;
                case HStirRodStyle.Homogenizer: DrawHomogenizer(g, a, pal); break;
                case HStirRodStyle.RibbonBlender: DrawHelix(g, a, pal, true); break;
                case HStirRodStyle.DrumRoller: DrawRoller(g, a, pal); break;
                case HStirRodStyle.Shaker: DrawShaker(g, a, pal); break;
                case HStirRodStyle.Propeller: DrawPropeller(g, a, pal); break;
                case HStirRodStyle.Retriever: DrawRetriever(g, a, pal); break;
                case HStirRodStyle.Mortar: DrawMortar(g, a, pal); break;
                case HStirRodStyle.Whisk: DrawWhisk(g, a, pal); break;
                case HStirRodStyle.StaticMixer: DrawStatic(g, a, pal); break;
                case HStirRodStyle.AirStone: DrawAirStone(g, a, pal); break;
                case HStirRodStyle.Centrifuge: DrawCentrifuge(g, a, pal); break;
                default: DrawClassic(g, a); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：带柄头玻璃搅拌棒（已验收绘法，原样保留）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width;
            float glyphH = a.Height;
            float tipX = Width / 2f, tipY = glyphH * 0.9f;
            float len = glyphH * 0.72f;
            float swing = Running ? (float)Math.Sin(Phase * 2.2) * 13f : -6f;
            // 搅拌旋弧（棒尖）
            if (Running)
            {
                using (var swirl = new Pen(Color.FromArgb(150, 110, 180, 220), 2f))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float rr = 8f + i * 7f + (float)Math.Sin(Phase + i) * 2f;
                        g.DrawArc(swirl, tipX - rr, tipY - rr * 0.5f, rr * 2f, rr, 10f, 150f);
                    }
                }
            }
            // 棒身绕棒尖摆动
            var st = g.Save();
            g.TranslateTransform(tipX, tipY);
            g.RotateTransform(swing);
            // 玻璃棒（圆角线 + 高光）
            using (var rod = new Pen(Color.FromArgb(200, 190, 215, 230), 5f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(rod, 0f, 0f, 0f, -len);
            using (var hl = new Pen(Color.FromArgb(200, Color.White), 1.6f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(hl, -1.4f, -5f, -1.4f, -len + 5f);
            // 柄头（扁圆握柄）
            using (var knob = new SolidBrush(Color.FromArgb(210, 200, 222, 236)))
                g.FillEllipse(knob, -6f, -len - 9f, 12f, 10f);
            g.DrawEllipse(new Pen(Color.FromArgb(140, 150, 165), 1.2f), -6f, -len - 9f, 12f, 10f);
            // 棒尖液滴
            using (var drop = new SolidBrush(Color.FromArgb(170, 110, 180, 220)))
                g.FillEllipse(drop, -2.6f, 1f, 5.2f, 6.2f);
            g.Restore(st);
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // 1 磁力搅拌：烧杯 + 杯底旋转搅拌子
        // ----------------------------------------------------------------
        private void DrawMagnetic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var cup = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.08f,
                a.Width * 0.72f, a.Height * 0.82f);
            Cup(g, cup, 0.5f, pal);
            float cy = cup.Bottom - 12f;
            if (Running) Blur(g, cup.Left + cup.Width / 2f, cy, a.Width * 0.24f, 7f, pal.Accent);
            Capsule(g, cup.Left + cup.Width / 2f, cy, a.Width * 0.4f, 4f,
                Running ? SpinAngle : 0f, Color.FromArgb(225, pal.Metal), pal.Edge);
        }
        // ----------------------------------------------------------------
        // 2 桨式搅拌：顶驱 + 直轴 + 平桨（侧视桨长随转角变化）
        // ----------------------------------------------------------------
        private void DrawPaddle(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float hy = a.Top + a.Height * 0.78f;
            DriveBox(g, new RectangleF(cx - 7f, a.Top + 2f, 14f, 10f), pal);
            Shaft(g, cx, a.Top + 12f, hy, pal);
            float bl = a.Width * 0.31f * (Running ? 0.3f + 0.7f * CosSpin() : 1f);
            g.Box(new RectangleF(cx - bl, hy - 4f, bl * 2f, 8f), 3f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.2f);
            if (Running) Swirl(g, cx, hy + 9f, 15f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 3 锚式搅拌：杯内 U 形锚框绕底往复摆动
        // ----------------------------------------------------------------
        private void DrawAnchor(Graphics g, RectangleF a, HToolPalette pal)
        {
            var cup = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.1f,
                a.Width * 0.76f, a.Height * 0.8f);
            Cup(g, cup, 0.42f, pal);
            float cx = cup.Left + cup.Width / 2f, pivot = cup.Bottom - 12f;
            float legX = a.Width * 0.23f, upH = a.Height * 0.3f;
            DriveBox(g, new RectangleF(cx - 7f, a.Top + 2f, 14f, 10f), pal);
            Shaft(g, cx, a.Top + 12f, pivot, pal);
            var st = g.Save();
            g.TranslateTransform(cx, pivot);
            g.RotateTransform(Running ? (float)Math.Sin(Phase * 1.6) * 8f : 0f);
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.18f), 4f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(p, 0, 0, -legX, 0);
                g.DrawLine(p, -legX, 0, -legX, -upH);
                g.DrawLine(p, 0, 0, legX, 0);
                g.DrawLine(p, legX, 0, legX, -upH);
            }
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 4 涡轮圆盘搅拌：多层叶片投影 + 旋转虚影
        // ----------------------------------------------------------------
        private void DrawTurbine(Graphics g, RectangleF a, HToolPalette pal)
        {
            var cup = new RectangleF(a.Left + a.Width * 0.13f, a.Top + a.Height * 0.09f,
                a.Width * 0.74f, a.Height * 0.81f);
            Cup(g, cup, 0.55f, pal);
            float cx = cup.Left + cup.Width / 2f, dy = cup.Bottom - 24f;
            DriveBox(g, new RectangleF(cx - 7f, a.Top + 2f, 14f, 10f), pal);
            Shaft(g, cx, a.Top + 12f, dy, pal);
            if (Running) Blur(g, cx, dy, a.Width * 0.3f, 9f, pal.Accent);
            // 圆盘 + 6 片叶片侧视投影
            float proj = Running ? Math.Abs(CosSpin()) : 1f;
            for (int i = 0; i < 3; i++)
            {
                float bl = a.Width * 0.28f * (0.25f + 0.75f * proj) * (1f - i * 0.18f);
                using (var p = new Pen(Color.FromArgb(220 - i * 50, HToolPalettes.Shade(pal.Main, 0.1f + i * 0.12f)),
                    4f - i * 0.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLine(p, cx - bl, dy - i * 2.5f, cx + bl, dy - i * 2.5f);
            }
            g.Ell(cx, dy, 3.5f, 3.5f, pal.Edge);
        }
        // ----------------------------------------------------------------
        // 5/12 螺旋带：中心轴 + 双条相位螺旋带（ribbon 时外加槽体）
        // ----------------------------------------------------------------
        private void DrawHelix(Graphics g, RectangleF a, HToolPalette pal, bool ribbon)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + (ribbon ? a.Height * 0.16f : a.Height * 0.12f);
            float bot = a.Bottom - a.Height * 0.08f, amp = a.Width * (ribbon ? 0.24f : 0.18f);
            if (ribbon)
            {
                // 立式 U 槽
                var trough = new RectangleF(cx - a.Width * 0.34f, top - a.Height * 0.05f,
                    a.Width * 0.68f, (bot - top) + a.Height * 0.08f);
                using (var tp = HBarBase.RoundPath(trough, 8f))
                {
                    using (var b = new SolidBrush(Color.FromArgb(70, pal.Metal)))
                        g.FillPath(b, tp);
                    using (var p = new Pen(pal.Edge, 1.5f))
                        g.DrawPath(p, tp);
                }
                DriveBox(g, new RectangleF(cx - 9f, a.Top + 2f, 18f, 12f), pal);
            }
            else
            {
                DriveBox(g, new RectangleF(cx - 7f, a.Top + 2f, 14f, 10f), pal);
            }
            g.Line(pal.MetalDark, 2.4f, cx, top, cx, bot);
            // 双螺旋带
            var s1 = new PointF[25];
            var s2 = new PointF[25];
            for (int k = 0; k < 25; k++)
            {
                float y = top + (bot - top) * k / 24f;
                float x1 = cx + (float)Math.Sin(k * 0.62 + (Running ? Phase * 2.2 : 0)) * amp;
                float x2 = cx + (float)Math.Sin(k * 0.62 + (Running ? Phase * 2.2 : 0) + Math.PI) * amp;
                s1[k] = new PointF(x1, y);
                s2[k] = new PointF(x2, y);
            }
            using (var p1 = new Pen(HToolPalettes.Shade(pal.Main, 0.15f), ribbon ? 4f : 2.8f))
                g.DrawLines(p1, s1);
            using (var p2 = new Pen(pal.Accent, ribbon ? 3f : 2f))
                g.DrawLines(p2, s2);
        }
        // ----------------------------------------------------------------
        // 6 直玻璃棒（无柄头）
        // ----------------------------------------------------------------
        private void DrawStraight(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.06f, tip = a.Top + a.Height * 0.88f;
            float len = tip - top, swing = Running ? (float)Math.Sin(Phase * 2.2) * 10f : -4f;
            var st = g.Save();
            g.TranslateTransform(cx, tip);
            g.RotateTransform(swing);
            using (var rod = new Pen(Color.FromArgb(200, pal.GlassEdge), 5f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(rod, 0f, 0f, 0f, -len);
            using (var hl = new Pen(Color.FromArgb(190, Color.White), 1.5f))
                g.DrawLine(hl, -1.4f, -5f, -1.4f, -len + 5f);
            g.Restore(st);
            if (Running) Swirl(g, cx, tip, 14f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 7 药勺：金属勺柄 + 扁平勺头，运行时抖动落粉
        // ----------------------------------------------------------------
        private void DrawSpatula(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float neck = a.Top + a.Height * 0.66f, sy = a.Top + a.Height * 0.82f;
            var st = g.Save();
            g.TranslateTransform(cx, sy);
            g.RotateTransform(Running ? (float)Math.Sin(Phase * 3f) * 3f : -5f);
            // 勺柄
            using (var p = new Pen(pal.MetalDark, 3.4f) { StartCap = LineCap.Round })
                g.DrawLine(p, 0, -a.Height * 0.66f, 0, -a.Height * 0.16f);
            // 勺头（扁椭圆）
            using (var b = new SolidBrush(pal.Metal))
                g.FillEllipse(b, -a.Width * 0.22f, -a.Height * 0.13f, a.Width * 0.44f, a.Height * 0.1f);
            using (var p = new Pen(pal.Edge, 1.3f))
                g.DrawEllipse(p, -a.Width * 0.22f, -a.Height * 0.13f, a.Width * 0.44f, a.Height * 0.1f);
            g.Restore(st);
            // 粉末
            if (Running)
                for (int i = 0; i < 4; i++)
                {
                    float t = (Phase * 0.35f + i * 0.23f) % 1f;
                    g.Ell(cx + (float)Math.Sin(t * 9f + i) * 5f - 1.5f, neck + t * a.Height * 0.16f,
                        2.4f, 2.4f, Color.FromArgb((int)(180 * (1f - t * 0.5f)), pal.Light));
                }
        }
        // ----------------------------------------------------------------
        // 8 顶置机械搅拌器：电机头 + 支架 + 杯内桨叶
        // ----------------------------------------------------------------
        private void DrawOverhead(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            // 支架 + 底座
            g.Line(pal.Edge, 3f, a.Right - a.Width * 0.12f, a.Top + a.Height * 0.18f,
                a.Right - a.Width * 0.12f, a.Bottom - a.Height * 0.06f);
            g.Box(new RectangleF(a.Right - a.Width * 0.26f, a.Bottom - a.Height * 0.08f,
                a.Width * 0.28f, 7f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 电机头 + 夹座
            var motor = new RectangleF(cx - a.Width * 0.3f, a.Top + 2f, a.Width * 0.56f, a.Height * 0.12f);
            DriveBox(g, motor, pal);
            g.Box(new RectangleF(a.Right - a.Width * 0.2f, a.Top + a.Height * 0.05f,
                a.Width * 0.16f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            var cup = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.42f,
                a.Width * 0.72f, a.Height * 0.42f);
            Cup(g, cup, 0.5f, pal);
            float hy = cup.Bottom - 14f;
            Shaft(g, cx, motor.Bottom, hy, pal);
            float bl = a.Width * 0.28f * (Running ? 0.3f + 0.7f * CosSpin() : 1f);
            g.Box(new RectangleF(cx - bl, hy - 3.5f, bl * 2f, 7f), 3f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.1f);
            if (Running) Swirl(g, cx, hy + 8f, 12f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 9 涡旋振荡器：基座 + 橡胶杯座 + 倾斜试管涡旋
        // ----------------------------------------------------------------
        private void DrawVortex(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseY = a.Top + a.Height * 0.6f;
            // 基座
            g.Box(new RectangleF(a.Left + a.Width * 0.08f, baseY,
                a.Width * 0.84f, a.Height * 0.3f), 6f,
                HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.4f);
            g.Lamp(a.Right - a.Width * 0.16f, baseY + a.Height * 0.2f, Running);
            // 橡胶杯座
            g.Ell(cx, baseY - 4f, a.Width * 0.2f, 8f, pal.Dark);
            g.Box(new RectangleF(cx - a.Width * 0.16f, baseY - 10f, a.Width * 0.32f, 10f), 3f,
                pal.Dark, pal.Edge, 1.2f);
            // 涡旋线
            if (Running)
                for (int i = 0; i < 2; i++)
                    using (var sp = new Pen(Color.FromArgb(150 - i * 40, pal.Accent), 1.5f))
                    {
                        float rr = 5f + i * 5f + (float)Math.Sin(Phase * 3f + i) * 1.5f;
                        g.DrawArc(sp, cx - rr, baseY - 18f - i * 3f, rr * 2f, rr * 1.4f, 200f, 140f);
                    }
            // 倾斜试管（运行时微颤）
            float ang = (Running ? 18f + (float)Math.Sin(Phase * 4f) * 2.5f : 16f);
            float tw = a.Width * 0.16f, th = a.Height * 0.4f;
            var st = g.Save();
            g.TranslateTransform(cx, baseY - 8f);
            g.RotateTransform(ang);
            using (var tp = RoundTube(-tw / 2f, -th, tw, 0f))
            {
                var old = g.Clip;
                g.SetClip(tp);
                using (var lb = new SolidBrush(Color.FromArgb(120, pal.Main)))
                    g.FillRectangle(lb, -tw / 2f, -th * 0.35f, tw, th * 0.35f);
                g.Clip = old;
                using (var b = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                    g.FillPath(b, tp);
                using (var p = new Pen(pal.GlassEdge, 1.4f))
                    g.DrawPath(p, tp);
            }
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 10 磁力加热台：面板 + 双旋钮 + 烧杯搅拌子
        // ----------------------------------------------------------------
        private void DrawHotplate(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.1f;
            var cup = new RectangleF(a.Left + a.Width * 0.16f, top,
                a.Width * 0.68f, a.Height * 0.56f);
            Cup(g, cup, 0.5f, pal);
            Capsule(g, cup.Left + cup.Width / 2f, cup.Bottom - 10f, a.Width * 0.34f, 3.5f,
                Running ? SpinAngle : 0f, Color.FromArgb(225, pal.Metal), pal.Edge);
            // 面板
            float py = a.Top + a.Height * 0.68f;
            g.Box(new RectangleF(a.Left + a.Width * 0.06f, py,
                a.Width * 0.88f, a.Height * 0.22f), 5f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.4f);
            // 加热盘
            g.Ell(cx, py + 2f, a.Width * 0.3f, 6f, Color.FromArgb(90, pal.Edge));
            // 旋钮 + 灯
            g.Ell(a.Left + a.Width * 0.28f, py + a.Height * 0.14f, 5f, 5f, pal.MetalDark);
            g.Ell(a.Right - a.Width * 0.28f, py + a.Height * 0.14f, 5f, 5f, pal.MetalDark);
            g.Lamp(cx, py + a.Height * 0.15f, Running);
        }
        // ----------------------------------------------------------------
        // 11 均质乳化机：手持电机 + 长轴 + 开槽定子头浸入杯中
        // ----------------------------------------------------------------
        private void DrawHomogenizer(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            // 电机身
            g.Box(new RectangleF(cx - a.Width * 0.18f, a.Top + 2f,
                a.Width * 0.36f, a.Height * 0.18f), 5f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.4f);
            g.Box(new RectangleF(cx - a.Width * 0.06f, a.Top + a.Height * 0.06f,
                a.Width * 0.12f, 7f), 2f, pal.MetalDark, pal.Edge, 1f);
            // 杯
            var cup = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.5f,
                a.Width * 0.76f, a.Height * 0.38f);
            Cup(g, cup, 0.6f, pal);
            // 轴 + 定子头
            float hy = cup.Bottom - 12f;
            g.Line(pal.MetalDark, 3f, cx, a.Top + a.Height * 0.2f, cx, hy - 8f);
            g.Box(new RectangleF(cx - a.Width * 0.09f, hy - 8f, a.Width * 0.18f, 15f), 3f,
                pal.Metal, pal.Edge, 1.2f);
            for (int i = 0; i < 3; i++)
                g.Line(pal.Edge, 1.2f, cx - a.Width * 0.07f, hy - 3f + i * 4f,
                    cx + a.Width * 0.07f, hy - 3f + i * 4f);
            if (Running)
            {
                Blur(g, cx, hy, a.Width * 0.16f, 7f, pal.Accent);
                Swirl(g, cx, hy + 9f, 10f, pal.Accent);
            }
        }
        // ----------------------------------------------------------------
        // 13 滚瓶机：双滚轮 + 瓶身自转
        // ----------------------------------------------------------------
        private void DrawRoller(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseY = a.Top + a.Height * 0.72f;
            g.Box(new RectangleF(a.Left + a.Width * 0.06f, baseY,
                a.Width * 0.88f, a.Height * 0.2f), 5f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.3f);
            // 双滚轮
            foreach (float rx in new[] { -0.2f, 0.2f })
            {
                float px = cx + a.Width * rx, py = baseY - 4f, rr = 9f;
                g.Ell(px, py, rr, rr, pal.Metal);
                using (var p = new Pen(pal.Edge, 1.3f))
                    g.DrawEllipse(p, px - rr, py - rr, rr * 2f, rr * 2f);
                var st = g.Save();
                g.TranslateTransform(px, py);
                g.RotateTransform(SpinAngle);
                g.Line(pal.Edge, 1.6f, -rr * 0.6f, 0, rr * 0.6f, 0);
                g.Restore(st);
            }
            g.Lamp(a.Right - a.Width * 0.14f, baseY + a.Height * 0.12f, Running);
            // 直立样品瓶（自转标识弧）
            var vial = new RectangleF(cx - a.Width * 0.13f, a.Top + a.Height * 0.28f,
                a.Width * 0.26f, a.Height * 0.4f);
            g.Box(vial, 4f, Color.FromArgb(60, pal.Glass), pal.GlassEdge, 1.3f);
            g.Box(new RectangleF(vial.X - 2f, vial.Y - 9f, vial.Width + 4f, 10f), 3f,
                pal.Dark, pal.Edge, 1.2f);
            if (Running)
                using (var ar = new Pen(Color.FromArgb(170, pal.Accent), 1.6f))
                    g.DrawArc(ar, vial.X - 4f, vial.Y + 4f, vial.Width + 8f, vial.Height - 8f,
                        300f, 100f);
        }
        // ----------------------------------------------------------------
        // 14 往复摇床：平台载锥形瓶水平往复
        // ----------------------------------------------------------------
        private void DrawShaker(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseY = a.Top + a.Height * 0.72f;
            // 基座 + 偏心轮
            g.Box(new RectangleF(a.Left + a.Width * 0.08f, baseY,
                a.Width * 0.84f, a.Height * 0.2f), 5f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.3f);
            float ox = Running ? cx + (float)Math.Sin(Phase * 2f) * 4f : cx;
            g.Ell(ox, baseY + a.Height * 0.1f, 5f, 5f, pal.MetalDark);
            g.Lamp(a.Right - a.Width * 0.14f, baseY + a.Height * 0.13f, Running);
            // 往复平台 + 锥形瓶
            float dx = Running ? (float)Math.Sin(Phase * 2f) * 4f : 0f;
            var st = g.Save();
            g.TranslateTransform(dx, 0f);
            float py = baseY - 6f;
            g.Box(new RectangleF(cx - a.Width * 0.3f, py, a.Width * 0.6f, 7f), 2.5f,
                pal.MetalDark, pal.Edge, 1.2f);
            float fTop = a.Top + a.Height * 0.26f, fBot = py - 2f;
            var fpts = new[]
            {
                new PointF(cx - 4f, fTop), new PointF(cx + 4f, fTop),
                new PointF(cx + 4f, fTop + 10f),
                new PointF(cx + a.Width * 0.2f, fBot), new PointF(cx - a.Width * 0.2f, fBot),
                new PointF(cx - 4f, fTop + 10f)
            };
            using (var fp = new GraphicsPath())
            {
                fp.AddPolygon(fpts);
                fp.CloseFigure();
                var old = g.Clip;
                g.SetClip(fp);
                using (var lb = new SolidBrush(Color.FromArgb(120, pal.Main)))
                    g.FillRectangle(lb, cx - a.Width * 0.2f, fBot - a.Height * 0.14f,
                        a.Width * 0.4f, a.Height * 0.14f);
                g.Clip = old;
                using (var b = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                    g.FillPath(b, fp);
                using (var p = new Pen(pal.GlassEdge, 1.4f))
                    g.DrawPath(p, fp);
            }
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 15 三叶推进桨：长轴 + 旋转桨叶投影
        // ----------------------------------------------------------------
        private void DrawPropeller(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float hy = a.Top + a.Height * 0.82f;
            DriveBox(g, new RectangleF(cx - 7f, a.Top + 2f, 14f, 10f), pal);
            Shaft(g, cx, a.Top + 12f, hy, pal);
            // 杯壁暗示
            using (var w = new Pen(Color.FromArgb(70, pal.GlassEdge), 1.3f))
            {
                g.DrawLine(w, a.Left + a.Width * 0.16f, a.Top + a.Height * 0.3f,
                    a.Left + a.Width * 0.16f, a.Bottom - a.Height * 0.08f);
                g.DrawLine(w, a.Right - a.Width * 0.16f, a.Top + a.Height * 0.3f,
                    a.Right - a.Width * 0.16f, a.Bottom - a.Height * 0.08f);
            }
            if (Running) Blur(g, cx, hy, a.Width * 0.32f, 9f, pal.Accent);
            float proj = Running ? Math.Abs(CosSpin()) : 1f;
            float bl = a.Width * 0.3f * (0.25f + 0.75f * proj);
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.15f), 4.4f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, cx - bl, hy, cx + bl, hy);
            // 叶端折角（螺距）
            g.Line(HToolPalettes.Shade(pal.Main, 0.15f), 2.6f, cx - bl, hy, cx - bl + 4f, hy - 5f);
            g.Line(HToolPalettes.Shade(pal.Main, 0.15f), 2.6f, cx + bl, hy, cx + bl - 4f, hy - 5f);
            g.Ell(cx, hy, 3.2f, 3.2f, pal.Edge);
            if (Running) Swirl(g, cx, hy + 10f, 14f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 16 磁子取出棒：长棒端磁头 + 吸附搅拌子，运行时整体上下提动
        // ----------------------------------------------------------------
        private void DrawRetriever(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float bob = Running ? (float)Math.Sin(Phase * 2f) * a.Height * 0.03f : 0f;
            float gripY = a.Top + a.Height * 0.06f;
            float headY = a.Top + a.Height * 0.66f;
            var st = g.Save();
            g.TranslateTransform(0f, bob);
            // 握柄
            g.Box(new RectangleF(cx - 5f, gripY, 10f, a.Height * 0.12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.2f);
            // 长棒
            g.Line(pal.MetalDark, 3f, cx, gripY + a.Height * 0.12f, cx, headY - 6f);
            // 磁头（两极分块）
            var head = new RectangleF(cx - a.Width * 0.1f, headY - 6f, a.Width * 0.2f, 11f);
            g.Box(head, 3f, pal.Metal, pal.Edge, 1.2f);
            g.Line(pal.Edge, 1f, cx, head.Y + 1.5f, cx, head.Bottom - 1.5f);
            // 吸附在磁头下的搅拌子
            Capsule(g, cx, head.Bottom + 5f, a.Width * 0.34f, 3f, 0f,
                Color.FromArgb(225, HToolPalettes.Shade(pal.Main, 0.05f)), pal.Edge);
            g.Restore(st);
            // 磁力弧线
            if (Running)
                using (var fm = new Pen(Color.FromArgb(150, pal.Accent), 1.4f))
                    for (int i = 0; i < 2; i++)
                    {
                        float rr = a.Width * (0.16f + i * 0.08f);
                        g.DrawArc(fm, cx - rr, head.Bottom - rr * 0.2f + bob, rr * 2f, rr, 30f, 120f);
                    }
        }
        // ----------------------------------------------------------------
        // 17 研钵与研杵：厚壁碗 + 研杵往复研磨
        // ----------------------------------------------------------------
        private void DrawMortar(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float r = a.Width * 0.32f, rimY = a.Top + a.Height * 0.62f;
            // 钵体（厚壁 ∪）
            var bowl = new RectangleF(cx - r, rimY - r, r * 2f, r * 2f);
            using (var b = new SolidBrush(Color.FromArgb(220, HToolPalettes.Tint(pal.Main, 0.3f))))
                g.FillPie(b, bowl.X, bowl.Y, bowl.Width, bowl.Height, 0f, 180f);
            using (var p = new Pen(pal.Edge, 1.8f))
            {
                g.DrawArc(p, bowl, 0f, 180f);
                g.DrawLine(p, cx - r, rimY, cx + r, rimY);
            }
            g.Ell(cx, rimY + r - 2f, r * 0.4f, 4f, HToolPalettes.Shade(pal.Main, 0.15f));
            // 药粉
            for (int i = -2; i <= 2; i++)
                g.Ell(cx + i * 7f, rimY + r * 0.55f + Math.Abs(i) * 1.2f, 2.2f, 2.2f, pal.Light);
            // 研杵（绕碗心摆动）
            var st = g.Save();
            g.TranslateTransform(cx + a.Width * 0.06f, rimY + r * 0.4f);
            g.RotateTransform(Running ? (float)Math.Sin(Phase * 1.8) * 14f : -10f);
            using (var p = new Pen(pal.MetalDark, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 0, 0, 0, -a.Height * 0.5f);
            g.Ell(0, -a.Height * 0.52f, 5f, 5f, pal.MetalDark);
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 18 丝笼搅棒：握柄 + 三圈丝笼，整体轻摆
        // ----------------------------------------------------------------
        private void DrawWhisk(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float neck = a.Top + a.Height * 0.3f, tip = a.Top + a.Height * 0.9f;
            var st = g.Save();
            g.TranslateTransform(cx, tip);
            g.RotateTransform(Running ? (float)Math.Sin(Phase * 2f) * 7f : -3f);
            // 三圈丝环（顶端汇聚于握柄，底端汇聚于棒尖）
            for (int i = 1; i <= 3; i++)
            {
                float rx = i * 4.6f;
                using (var w = new Pen(Color.FromArgb(200, pal.Metal), 1.4f))
                    g.DrawEllipse(w, -rx, -a.Height * 0.58f, rx * 2f, a.Height * 0.56f);
            }
            // 握柄（压住丝环顶端）
            using (var p = new Pen(pal.Dark, 6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 0, -a.Height * 0.34f, 0, -a.Height * 0.82f);
            g.Restore(st);
            if (Running) Swirl(g, cx, tip, 13f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 19 静态混合管：透明管内交错折流板，运行时颗粒沿蛇形通道下行
        // ----------------------------------------------------------------
        private void DrawStatic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float tw = a.Width * 0.34f;
            var tube = new RectangleF(cx - tw / 2f, a.Top + a.Height * 0.1f, tw, a.Height * 0.76f);
            // 上下接口
            g.Box(new RectangleF(cx - tw * 0.3f, tube.Top - 7f, tw * 0.6f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            g.Box(new RectangleF(cx - tw * 0.3f, tube.Bottom - 1f, tw * 0.6f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            using (var gp = HBarBase.RoundPath(tube, 4f))
            {
                using (var b = new SolidBrush(Color.FromArgb(22, 255, 255, 255)))
                    g.FillPath(b, gp);
                var old = g.Clip;
                g.SetClip(gp);
                // 交错折流板
                int n = 6;
                float bay = (tube.Height - 8f) / n;
                using (var bp = new Pen(HToolPalettes.Shade(pal.Main, 0.05f), 3.2f)
                { StartCap = LineCap.Round })
                    for (int i = 0; i < n; i++)
                    {
                        float dir = (i % 2 == 0) ? 1f : -1f;
                        float y1 = tube.Top + 4f + i * bay;
                        g.DrawLine(bp, cx - dir * tw * 0.42f, y1,
                            cx + dir * tw * 0.42f, y1 + bay * 0.82f);
                    }
                // 下行颗粒
                if (Running)
                    using (var bb = new SolidBrush(Color.FromArgb(200, pal.Accent)))
                        for (int i = 0; i < n; i++)
                        {
                            float seg = (Phase * 0.22f + i * 1f / n) % 1f * n;
                            int k = (int)seg;
                            float u = seg - k;
                            float dir = (k % 2 == 0) ? 1f : -1f;
                            float y1 = tube.Top + 4f + k * bay;
                            float px = cx - dir * tw * 0.42f + dir * tw * 0.84f * u;
                            g.FillEllipse(bb, px - 1.8f, y1 + bay * 0.82f * u - 1.8f, 3.6f, 3.6f);
                        }
                g.Clip = old;
                using (var p = new Pen(pal.GlassEdge, 1.5f))
                    g.DrawPath(p, gp);
            }
        }
        // ----------------------------------------------------------------
        // 20 曝气石：导气管 + 沉底曝气石 + 上升气泡
        // ----------------------------------------------------------------
        private void DrawAirStone(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            var cup = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.24f,
                a.Width * 0.76f, a.Height * 0.62f);
            Cup(g, cup, 0.75f, pal);
            float stoneY = cup.Bottom - 10f;
            // 曝气石 + 导气管（弯到液面外）
            g.Box(new RectangleF(cx - a.Width * 0.09f, stoneY - 4f, a.Width * 0.18f, 9f), 3f,
                pal.MetalDark, pal.Edge, 1.2f);
            using (var t = new Pen(pal.Edge, 2.4f) { StartCap = LineCap.Round })
                g.DrawBezier(t, cx, stoneY - 2f, cx - a.Width * 0.22f, cup.Top - a.Height * 0.1f,
                    a.Left + a.Width * 0.24f, cup.Top - a.Height * 0.04f,
                    a.Left + a.Width * 0.24f, cup.Top - a.Height * 0.18f);
            // 气泡
            if (Running)
                for (int i = 0; i < 6; i++)
                {
                    float t0 = (Phase * 0.4f + i * 0.16f) % 1f;
                    float y = stoneY - 4f - t0 * (stoneY - cup.Top - 6f);
                    float rr = 1.6f + t0 * 2.6f;
                    float xx = cx + (float)Math.Sin(t0 * 12f + i * 2f) * 4f;
                    using (var p = new Pen(Color.FromArgb((int)(170 * (1f - t0 * 0.3f)), pal.Accent), 1.2f))
                        g.DrawEllipse(p, xx - rr, y - rr, rr * 2f, rr * 2f);
                }
        }
        // ----------------------------------------------------------------
        // 21 离心机：机箱 + 开盖 + 视窗内转子与离心管
        // ----------------------------------------------------------------
        private void DrawCentrifuge(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float bodyY = a.Top + a.Height * 0.26f;
            // 开盖（斜线）
            using (var lid = new Pen(pal.Edge, 3f) { StartCap = LineCap.Round })
                g.DrawLine(lid, cx - a.Width * 0.3f, bodyY, cx - a.Width * 0.06f, a.Top + 4f);
            // 箱体
            g.Box(new RectangleF(a.Left + a.Width * 0.06f, bodyY,
                a.Width * 0.88f, a.Height * 0.52f), 6f,
                HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.4f);
            // 视窗
            float wy = bodyY + a.Height * 0.06f;
            var win = new RectangleF(cx - a.Width * 0.28f, wy, a.Width * 0.56f, a.Height * 0.34f);
            g.Box(win, 5f, Color.FromArgb(255, 30, 36, 44), pal.Edge, 1.3f);
            float rcx = win.Left + win.Width / 2f, rcy = win.Top + win.Height / 2f, rr = win.Height * 0.4f;
            g.Ell(rcx, rcy, rr, rr, Color.FromArgb(255, 70, 78, 88));
            // 两支对置离心管
            float ang = Running ? SpinAngle : 0f;
            for (int k = 0; k < 2; k++)
            {
                double aa = (ang + k * 180f) * Math.PI / 180.0;
                float px = rcx + (float)Math.Cos(aa) * rr;
                float py = rcy + (float)Math.Sin(aa) * rr;
                Capsule(g, px, py, 13f, 3.2f, ang + k * 180f,
                    Color.FromArgb(220, pal.Glass), pal.GlassEdge);
            }
            g.Ell(rcx, rcy, 3f, 3f, pal.MetalDark);
            // 面板
            g.Lamp(a.Right - a.Width * 0.16f, bodyY + a.Height * 0.44f, Running);
            g.Ell(a.Left + a.Width * 0.16f, bodyY + a.Height * 0.43f, 4f, 4f, pal.MetalDark);
        }
        // ----------------------------------------------------------------
        // 共享原语
        // ----------------------------------------------------------------
        /// <summary>简单玻璃杯：圆角杯身 + 口沿，level 为固定液位比例。</summary>
        private void Cup(Graphics g, RectangleF r, float level, HToolPalette pal)
        {
            float rad = Math.Min(5f, r.Width * 0.14f);
            using (var gp = HBarBase.RoundPath(r, rad))
            {
                var old = g.Clip;
                g.SetClip(gp);
                float fy = r.Bottom - 3f - level * (r.Height - 8f);
                var lr = new RectangleF(r.X, fy, r.Width, r.Bottom - fy);
                using (var lb = new LinearGradientBrush(lr,
                    Color.FromArgb(70, HToolPalettes.Tint(pal.Main, 0.5f)),
                    Color.FromArgb(130, pal.Main), 90f))
                    g.FillRectangle(lb, lr);
                g.Clip = old;
                using (var b = new SolidBrush(Color.FromArgb(24, 255, 255, 255)))
                    g.FillPath(b, gp);
                using (var p = new Pen(pal.GlassEdge, 1.5f))
                    g.DrawPath(p, gp);
            }
            g.Line(pal.GlassEdge, 1.4f, r.X - 2f, r.Y, r.Right + 2f, r.Y);
        }
        /// <summary>搅拌旋弧：两层随相位轻颤的弧线。</summary>
        private void Swirl(Graphics g, float cx, float cy, float scale, Color c)
        {
            using (var p = new Pen(Color.FromArgb(150, c), 1.8f))
                for (int i = 0; i < 2; i++)
                {
                    float rr = scale * (0.55f + i * 0.45f) + (float)Math.Sin(Phase + i) * 1.6f;
                    g.DrawArc(p, cx - rr, cy - rr * 0.5f, rr * 2f, rr, 10f, 150f);
                }
        }
        /// <summary>旋转虚影椭圆。</summary>
        private static void Blur(Graphics g, float cx, float cy, float rx, float ry, Color c)
        {
            using (var p = new Pen(Color.FromArgb(46, c), 1.6f))
                g.DrawEllipse(p, cx - rx, cy - ry, rx * 2f, ry * 2f);
        }
        /// <summary>胶囊形搅拌子/离心管，angle 为旋转角（度）。</summary>
        private static void Capsule(Graphics g, float cx, float cy, float len, float rh,
            float angle, Color fill, Color edge)
        {
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            var r = new RectangleF(-len / 2f, -rh, len, rh * 2f);
            using (var gp = HBarBase.RoundPath(r, rh))
            {
                using (var b = new SolidBrush(fill))
                    g.FillPath(b, gp);
                using (var p = new Pen(edge, 1.2f))
                    g.DrawPath(p, gp);
                using (var ring = new Pen(edge, 1f))
                    g.DrawEllipse(ring, -rh * 0.7f, -rh * 0.55f, rh * 1.4f, rh * 1.1f);
            }
            g.Restore(st);
        }
        /// <summary>驱动小盒：盒体 + 散热筋 + 运行灯。</summary>
        private void DriveBox(Graphics g, RectangleF r, HToolPalette pal)
        {
            g.Box(r, 2.5f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.3f);
            using (var fin = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.1f))
                for (int i = 1; i < 4; i++)
                    g.DrawLine(fin, r.X + r.Width * i / 4f, r.Y + 2f,
                        r.X + r.Width * i / 4f, r.Y + r.Height - 2f);
            g.Lamp(r.Right - 4f, r.Bottom - 4f, Running);
        }
        /// <summary>搅拌轴。</summary>
        private void Shaft(Graphics g, float cx, float y1, float y2, HToolPalette pal)
            => g.Line(pal.MetalDark, 3f, cx, y1, cx, y2);
        /// <summary>当前 SpinAngle 的余弦绝对值（桨叶侧视投影）。</summary>
        private float CosSpin()
            => Math.Abs((float)Math.Cos(SpinAngle * Math.PI / 180.0));
        /// <summary>圆底小管轮廓（局部坐标）。</summary>
        private static GraphicsPath RoundTube(float l, float top, float w, float straightBot)
        {
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddLine(l, top, l + w, top);
            p.AddLine(l + w, top, l + w, straightBot);
            p.AddArc(l, straightBot - w / 2f, w, w, 0f, 180f);
            p.AddLine(l, straightBot, l, top);
            p.CloseFigure();
            return p;
        }
    }
}