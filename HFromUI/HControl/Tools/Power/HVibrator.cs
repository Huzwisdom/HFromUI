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
    /// 振动夯实设备样式：Classic 为两端偏心块振动电机，其余 21 种覆盖平板夯、冲击夯、
    /// 振动压路机、振捣棒、振动台/筛/给料机、激振器、仓壁振动器、气动振动器、振动盘等。
    /// </summary>
    public enum HVibratorStyle
    {
        Classic = 0,        // 振动电机
        PlateCompactor = 1, // 平板夯
        Rammer = 2,         // 冲击夯
        RoadRoller = 3,     // 单钢轮振动压路机
        TandemRoller = 4,   // 双钢轮压路机
        Poker = 5,          // 插入式振捣棒
        External = 6,       // 附着式振动器
        VibroTable = 7,     // 振动台
        VibroFeeder = 8,    // 振动给料机
        VibroScreen = 9,    // 振动筛
        Grizzly = 10,       // 棒条筛
        VibroHammer = 11,   // 振动桩锤
        Exciter = 12,       // 箱式激振器
        AirKnocker = 13,    // 仓壁气动敲击器
        BallVibrator = 14,  // 滚珠式气动振动器
        TurbineVibrator = 15,// 涡轮式气动振动器
        PistonVibrator = 16,// 活塞往复振动器
        LinearFeeder = 17,  // 直线振动送料器
        BowlFeeder = 18,    // 振动盘
        ShakerConveyor = 19,// 振动输送机
        ImpactBed = 20,     // 落料缓冲撞击床
        Screed = 21         // 混凝土振动梁
    }
    /// <summary>
    /// 振动设备控件（继承 HToolAnimBase）：22 种振动夯实设备外形、13 种色调，
    /// Running 时偏心块旋转、整机抖动并显示振动波纹。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("振动设备控件：22 种振动夯实外形、13 种色调，运行时抖动并显示振动波纹")]
    public class HVibrator : HToolAnimBase
    {
        private HVibratorStyle _style = HVibratorStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HVibrator() { Size = new Size(170, 110); }
        /// <summary>振动设备外形样式。</summary>
        [HCategoryLanguage("振动器"), HDisplayNameLanguage("振动设备外形样式"), HDescriptionLanguage("振动设备外形样式"), Browsable(true)]
        [DefaultValue(HVibratorStyle.Classic)]
        public HVibratorStyle VibratorStyle
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
                case HVibratorStyle.PlateCompactor: DrawPlate(g, a, pal); break;
                case HVibratorStyle.Rammer: DrawRammer(g, a, pal); break;
                case HVibratorStyle.RoadRoller: DrawRoadRoller(g, a, pal, false); break;
                case HVibratorStyle.TandemRoller: DrawRoadRoller(g, a, pal, true); break;
                case HVibratorStyle.Poker: DrawPoker(g, a, pal); break;
                case HVibratorStyle.External: DrawExternal(g, a, pal); break;
                case HVibratorStyle.VibroTable: DrawTable(g, a, pal); break;
                case HVibratorStyle.VibroFeeder: DrawFeeder(g, a, pal, false); break;
                case HVibratorStyle.ShakerConveyor: DrawFeeder(g, a, pal, true); break;
                case HVibratorStyle.VibroScreen: DrawScreen(g, a, pal, false); break;
                case HVibratorStyle.Grizzly: DrawScreen(g, a, pal, true); break;
                case HVibratorStyle.VibroHammer: DrawHammer(g, a, pal); break;
                case HVibratorStyle.Exciter: DrawExciter(g, a, pal); break;
                case HVibratorStyle.AirKnocker: DrawKnocker(g, a, pal); break;
                case HVibratorStyle.BallVibrator: DrawPneu(g, a, pal, 0); break;
                case HVibratorStyle.TurbineVibrator: DrawPneu(g, a, pal, 1); break;
                case HVibratorStyle.PistonVibrator: DrawPneu(g, a, pal, 2); break;
                case HVibratorStyle.LinearFeeder: DrawLinear(g, a, pal); break;
                case HVibratorStyle.BowlFeeder: DrawBowl(g, a, pal); break;
                case HVibratorStyle.ImpactBed: DrawImpactBed(g, a, pal); break;
                case HVibratorStyle.Screed: DrawScreed(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>运行时高频抖动的画布变换。</summary>
        private GraphicsState Jitter(Graphics g)
        {
            float jx = Running ? (float)Math.Sin(Phase * 6f) * 1.6f : 0f;
            float jy = Running ? (float)Math.Cos(Phase * 5f) * 1.2f : 0f;
            var st = g.Save();
            g.TranslateTransform(jx, jy);
            return st;
        }
        /// <summary>中心两侧的振动波纹弧线。</summary>
        private void DrawWaves(Graphics g, RectangleF a, float cx, float cy)
        {
            if (!Running) return;
            var pal = Palette;
            using (var arc = new Pen(Color.FromArgb(160, pal.Accent), 1.8f))
                for (int k = 0; k < 2; k++)
                {
                    float t = (Phase / (float)Math.PI * 0.5f + k * 0.5f) % 1f;
                    float rr = a.Height * (0.28f + t * 0.2f);
                    arc.Color = Color.FromArgb((int)(170 * (1f - t)), pal.Accent);
                    g.DrawArc(arc, cx - rr, cy - rr, rr * 2f, rr * 2f, 200f, 30f);
                    g.DrawArc(arc, cx - rr, cy - rr, rr * 2f, rr * 2f, -20f, 30f);
                }
        }
        // ----------------------------------------------------------------
        // Classic：振动电机
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            float cy = a.Top + a.Height * 0.5f;
            float bw = a.Width * 0.56f, bh = a.Height * 0.46f;
            float x1 = a.Left + a.Width * 0.22f;
            var body = new RectangleF(x1, cy - bh / 2f, bw, bh);
            using (var baseB = new SolidBrush(pal.MetalDark))
                g.FillRectangle(baseB, x1 - 6f, cy + bh / 2f + 2f, bw + 12f, 6f);
            using (var shaft = new Pen(Color.FromArgb(70, 74, 80), 4f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(shaft, x1 - a.Width * 0.13f, cy, x1 + bw + a.Width * 0.13f, cy);
            DrawWeight(g, x1 - a.Width * 0.13f, cy, a.Height * 0.13f);
            DrawWeight(g, x1 + bw + a.Width * 0.13f, cy, a.Height * 0.13f);
            using (var gp = HBarBase.RoundPath(body, bh * 0.46f))
            using (var b = HBarBase.CylinderH(body, HToolPalettes.Shade(pal.Main, 0.28f),
                HToolPalettes.Tint(pal.Main, 0.2f)))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.5f), gp);
            }
            using (var fin = new Pen(HToolPalettes.Shade(pal.Main, 0.15f), 1.4f))
                for (int i = 1; i < 6; i++)
                    g.DrawLine(fin, x1 + bw * i / 6f, body.Y + 3, x1 + bw * i / 6f, body.Bottom - 3);
            using (var box = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.18f)))
                g.FillRectangle(box, x1 + bw * 0.38f, body.Y - a.Height * 0.09f, bw * 0.24f, a.Height * 0.09f);
            g.Restore(st0);
            DrawWaves(g, a, a.Left + a.Width * 0.5f, cy);
        }
        private void DrawWeight(Graphics g, float cx, float cy, float r)
        {
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var wb = new SolidBrush(Color.FromArgb(96, 100, 106)))
            {
                g.FillPie(wb, -r, -r * 0.7f, r * 2f, r * 1.4f, 0f, 180f);
                g.FillEllipse(Brushes.DimGray, -r * 0.32f, -r * 0.32f, r * 0.64f, r * 0.64f);
            }
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 新式振动设备
        // ----------------------------------------------------------------
        private void DrawPlate(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 底板
            g.Box(new RectangleF(a.Left + a.Width * 0.12f, a.Bottom - 14f, a.Width * 0.76f, 10f),
                3f, pal.MetalDark, pal.Edge, 1.3f);
            // 减振块 + 机架
            float fx = a.Left + a.Width * 0.5f, fy = a.Bottom - 14f;
            g.Line(pal.Edge, 3f, fx - 18f, fy, fx - 8f, fy - 20f);
            g.Line(pal.Edge, 3f, fx + 18f, fy, fx + 8f, fy - 20f);
            // 发动机底板上的振动电机（立置）
            float my = fy - 34f;
            g.Motor(fx - 26f, fx + 26f, my, 13f, pal);
            DrawWeight(g, fx - 28f, my, 9f);
            DrawWeight(g, fx + 28f, my, 9f);
            // 操作扶手
            g.Line(pal.MetalDark, 3.2f, fx + 10f, fy - 20f, a.Right - 8f, a.Top + 10f);
            g.Line(pal.MetalDark, 4f, a.Right - 16f, a.Top + 10f, a.Right - 2f, a.Top + 10f);
            g.Restore(st0);
            DrawWaves(g, a, fx, a.Bottom - 4f);
        }
        private void DrawRammer(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            float cx = a.Left + a.Width * 0.42f;
            // 夯脚板
            g.Box(new RectangleF(cx - 24f, a.Bottom - 10f, 48f, 8f), 3f, pal.MetalDark, pal.Edge, 1.2f);
            // 波纹护套（叠锥）
            float bootBot = a.Bottom - 12f;
            for (int i = 0; i < 4; i++)
            {
                float y = bootBot - 8f - i * 8f, w = 20f - i * 2.5f;
                g.Poly(new[] { new PointF(cx - w, y), new PointF(cx + w, y),
                    new PointF(cx + w - 3f, y - 7f), new PointF(cx - w + 3f, y - 7f) },
                    Color.FromArgb(60, 62, 68), pal.Edge, 1f);
            }
            // 机身上段
            float top = bootBot - 50f;
            g.Motor(cx - 18f, cx + 18f, top, 12f, pal);
            // 油箱 + 扶手
            g.Box(new RectangleF(cx - 12f, top - 22f, 24f, 9f), 2f,
                HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.1f);
            g.Line(pal.MetalDark, 3f, cx - 14f, top - 14f, a.Right - 8f, a.Top + 6f);
            g.Line(pal.MetalDark, 4f, a.Right - 16f, a.Top + 6f, a.Right - 2f, a.Top + 6f);
            g.Restore(st0);
            // 向下冲击波纹
            if (Running)
                using (var arc = new Pen(Color.FromArgb(160, pal.Accent), 1.8f))
                    for (int k = 0; k < 2; k++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + k * 0.5f) % 1f;
                        float w = 12f + t * 20f;
                        arc.Color = Color.FromArgb((int)(170 * (1f - t)), pal.Accent);
                        g.DrawArc(arc, cx - w, a.Bottom - 14f - t * 10f, w * 2f, 18f, 200f, 140f);
                    }
        }
        private void DrawRoadRoller(Graphics g, RectangleF a, HToolPalette pal, bool tandem)
        {
            var st0 = Jitter(g);
            float cy = a.Top + a.Height * 0.62f;
            float r1 = a.Height * 0.3f;
            float x1 = a.Left + a.Width * 0.26f, x2 = a.Right - a.Width * 0.22f;
            // 前钢轮
            DrawDrum(g, x1, cy, r1, pal);
            if (tandem)
            {
                DrawDrum(g, x2, cy, r1 * 0.92f, pal);
            }
            else
            {
                // 后橡胶轮
                g.Ell(x2, cy, r1 * 0.7f, r1 * 0.7f, Color.FromArgb(48, 50, 56));
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), x2 - r1 * 0.7f, cy - r1 * 0.7f, r1 * 1.4f, r1 * 1.4f);
                g.Ell(x2, cy, r1 * 0.28f, r1 * 0.28f, pal.MetalDark);
            }
            // 车架 + 驾驶室
            g.Poly(new[]
            {
                new PointF(x1, cy - r1 * 0.6f), new PointF(x2, cy - r1 * 0.5f),
                new PointF(x2 - 4f, cy - r1 * 1.5f), new PointF(x1 + 10f, cy - r1 * 1.4f)
            }, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            g.Box(new RectangleF(x1 + 12f, a.Top + 6f, a.Width * 0.34f, cy - r1 * 1.4f - a.Top - 8f),
                3f, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.2f);
            // 驾驶室玻璃窗
            using (var glass = new SolidBrush(Color.FromArgb(170, 200, 220)))
                g.FillRectangle(glass, x1 + 17f, a.Top + 11f, a.Width * 0.34f - 10f,
                    cy - r1 * 1.4f - a.Top - 18f);
            g.Restore(st0);
            // 钢轮振动波纹
            DrawWaves(g, a, x1, a.Bottom - 6f);
        }
        private void DrawDrum(Graphics g, float cx, float cy, float r, HToolPalette pal)
        {
            var body = new RectangleF(cx - r * 1.05f, cy - r, r * 2.1f, r * 2f);
            using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), body.X, body.Y, body.Width, body.Height);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var sp = new Pen(pal.MetalDark, 1.6f))
                for (int i = 0; i < 3; i++)
                {
                    g.RotateTransform(60f);
                    g.DrawLine(sp, 0, 0, r * 0.85f, 0);
                }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.22f, r * 0.22f, pal.Accent);
        }
        private void DrawPoker(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 软轴弯管（S 形）
            using (var hose = new Pen(Color.FromArgb(58, 60, 66), 6f))
            {
                g.DrawBezier(hose, a.Left + a.Width * 0.12f, a.Top + 8f,
                    a.Left + a.Width * 0.4f, a.Top - 6f,
                    a.Left + a.Width * 0.3f, a.Top + a.Height * 0.5f,
                    a.Left + a.Width * 0.5f, a.Top + a.Height * 0.62f);
            }
            // 驱动电机（手提）
            g.Motor(a.Left + a.Width * 0.02f, a.Left + a.Width * 0.22f, a.Top + 16f, 11f, pal);
            g.Line(pal.MetalDark, 3f, a.Left + a.Width * 0.06f, a.Top + 5f,
                a.Left + a.Width * 0.18f, a.Top + 5f);
            // 棒头（圆柱尖头）
            float hx = a.Left + a.Width * 0.5f, hy = a.Top + a.Height * 0.62f;
            g.Box(new RectangleF(hx, hy - 6f, a.Width * 0.36f, 12f), 5f,
                HToolPalettes.Shade(pal.Metal, 0.2f), pal.Edge, 1.2f);
            g.Poly(new[]
            {
                new PointF(hx + a.Width * 0.36f, hy - 6f),
                new PointF(a.Right - 4f, hy),
                new PointF(hx + a.Width * 0.36f, hy + 6f)
            }, HToolPalettes.Shade(pal.Metal, 0.2f), pal.Edge, 1.2f);
            g.Restore(st0);
            // 棒端振动环
            if (Running)
                using (var arc = new Pen(Color.FromArgb(160, pal.Accent), 1.6f))
                    for (int k = 0; k < 2; k++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + k * 0.5f) % 1f;
                        arc.Color = Color.FromArgb((int)(170 * (1f - t)), pal.Accent);
                        g.DrawArc(arc, a.Right - 14f - t * 8f, hy - 10f - t * 4f,
                            20f + t * 16f, 20f + t * 8f, 300f, 120f);
                    }
        }
        private void DrawExternal(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 附着在模板（左侧竖板）上的振动器
            g.Box(new RectangleF(a.Left + 4f, a.Top + 6f, 8f, a.Height - 12f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            float cx = a.Left + a.Width * 0.46f, cy = a.Top + a.Height * 0.5f, r = a.Height * 0.26f;
            // 端盖圆
            g.Ell(cx, cy, r, r, HToolPalettes.Shade(pal.Main, 0.15f));
            g.DrawEllipse(new Pen(pal.Edge, 1.5f), cx - r, cy - r, r * 2f, r * 2f);
            // 偏心块
            var stt = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var wb = new SolidBrush(Color.FromArgb(96, 100, 106)))
            {
                g.FillPie(wb, -r * 0.7f, -r * 0.5f, r * 1.4f, r, 0f, 180f);
                g.FillEllipse(Brushes.DimGray, -r * 0.22f, -r * 0.22f, r * 0.44f, r * 0.44f);
            }
            g.Restore(stt);
            // 安装座
            g.Box(new RectangleF(cx - 10f, cy + r - 2f, 20f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Restore(st0);
            // 沿模板传导的波纹
            if (Running)
                using (var arc = new Pen(Color.FromArgb(150, pal.Accent), 1.6f))
                    for (int k = 0; k < 3; k++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + k / 3f) % 1f;
                        arc.Color = Color.FromArgb((int)(160 * (1f - t)), pal.Accent);
                        float y = a.Top + a.Height * (0.2f + t * 0.6f);
                        g.DrawArc(arc, a.Left + 12f, y - 7f, 26f, 14f, -60f, 120f);
                    }
        }
        private void DrawTable(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 台面
            float ty = a.Top + a.Height * 0.42f;
            g.Box(new RectangleF(a.Left + a.Width * 0.1f, ty, a.Width * 0.8f, 12f), 3f,
                HToolPalettes.Tint(pal.Metal, 0.1f), pal.Edge, 1.3f);
            // 台面上的试块
            g.Box(new RectangleF(a.Left + a.Width * 0.24f, ty - 14f, 18f, 14f), 1.5f,
                Color.FromArgb(150, 152, 158), pal.Edge, 1f);
            g.Box(new RectangleF(a.Left + a.Width * 0.56f, ty - 10f, 14f, 10f), 1.5f,
                Color.FromArgb(150, 152, 158), pal.Edge, 1f);
            // 弹簧支腿
            foreach (float lx in new[] { a.Left + a.Width * 0.18f, a.Right - a.Width * 0.18f })
            {
                using (var sp = new Pen(pal.Edge, 1.6f))
                    for (int k = 0; k < 5; k++)
                        g.DrawArc(sp, lx - 5f, ty + 14f + k * 6f, 10f, 7f, 0f, 180f);
                g.Line(pal.MetalDark, 3f, lx, ty + 44f, lx, a.Bottom - 5f);
            }
            // 台底振动电机
            g.Motor(a.Left + a.Width * 0.36f, a.Right - a.Width * 0.36f, ty + 32f, 10f, pal);
            g.Restore(st0);
            DrawWaves(g, a, a.Left + a.Width * 0.5f, a.Bottom - 6f);
        }
        private void DrawFeeder(Graphics g, RectangleF a, HToolPalette pal, bool longTrough)
        {
            var st0 = Jitter(g);
            float cy = a.Top + a.Height * 0.46f;
            float x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.06f;
            float t = a.Height * 0.26f, bb = a.Height * (longTrough ? 0.1f : 0.16f);
            // 槽体（梯形）
            g.Poly(new[]
            {
                new PointF(x1, cy - t / 2f), new PointF(x2, cy - t / 2f),
                new PointF(x2 - bb, cy + t / 2f), new PointF(x1 + bb, cy + t / 2f)
            }, HToolPalettes.Tint(pal.Metal, 0.15f), pal.Edge, 1.4f);
            // 槽内物料（跳跃前移）
            using (var mat = new SolidBrush(Color.FromArgb(149, 112, 72)))
                for (int i = 0; i < 7; i++)
                {
                    float span = x2 - x1 - bb * 2f;
                    float px = x1 + bb + (i * 22f) % span;
                    float hop = Running ? (float)Math.Abs(Math.Sin(Phase * 3f + i)) * 8f : 0f;
                    g.FillEllipse(mat, px, cy - t * 0.35f - hop, 7f, 7f);
                }
            // 吊挂弹簧
            foreach (float hx in new[] { x1 + 16f, x2 - 16f })
            {
                using (var sp = new Pen(pal.Edge, 1.4f))
                    for (int k = 0; k < 3; k++)
                        g.DrawArc(sp, hx - 4f, a.Top + 4f + k * 5f, 8f, 6f, 0f, 180f);
                g.Line(pal.Edge, 2f, hx, a.Top + 19f, hx, cy - t / 2f);
            }
            g.Restore(st0);
        }
        private void DrawScreen(Graphics g, RectangleF a, HToolPalette pal, bool bars)
        {
            var st0 = Jitter(g);
            float sx = a.Left + a.Width * 0.12f, sy = a.Top + a.Height * 0.22f;
            float sw = a.Width * 0.76f, sh = a.Height * 0.4f;
            // 筛箱（倾斜）
            var stt = g.Save();
            g.TranslateTransform(sx + sw / 2f, sy + sh / 2f);
            g.RotateTransform(-8f);
            g.Box(new RectangleF(-sw / 2f, -sh / 2f, sw, sh), 3f,
                HToolPalettes.Tint(pal.Metal, 0.12f), pal.Edge, 1.4f);
            if (bars)
            {
                // 棒条
                using (var bar = new Pen(pal.MetalDark, 2.4f))
                    for (float x = -sw / 2f + 8f; x < sw / 2f - 4f; x += 9f)
                        g.DrawLine(bar, x, -sh / 2f + 4f, x, sh / 2f - 4f);
            }
            else
            {
                // 筛网网格
                using (var mesh = new Pen(pal.MetalDark, 1.1f))
                {
                    for (float x = -sw / 2f + 6f; x < sw / 2f; x += 9f)
                        g.DrawLine(mesh, x, -sh / 2f + 3f, x, sh / 2f - 3f);
                    g.DrawLine(mesh, -sw / 2f + 3f, -2f, sw / 2f - 3f, -2f);
                    g.DrawLine(mesh, -sw / 2f + 3f, sh * 0.22f, sw / 2f - 3f, sh * 0.22f);
                }
            }
            // 料粒
            using (var stone = new SolidBrush(Color.FromArgb(149, 112, 72)))
                for (int i = 0; i < 6; i++)
                    g.FillEllipse(stone, -sw / 2f + 10f + i * 16f, -sh / 2f - 2f - (i % 2) * 3f, 6f, 6f);
            g.Restore(stt);
            // 弹簧支座
            foreach (float lx in new[] { sx + 10f, sx + sw - 10f })
            {
                float top = sy + sh - 2f;
                using (var sp = new Pen(pal.Edge, 1.5f))
                    for (int k = 0; k < 4; k++)
                        g.DrawArc(sp, lx - 5f, top + k * 5f, 10f, 6f, 0f, 180f);
            }
            // 激振电机（筛箱上方）
            float my = sy - 12f;
            g.Motor(sx + sw * 0.3f, sx + sw * 0.62f, my, 9f, pal);
            DrawWeight(g, sx + sw * 0.3f - 4f, my, 7f);
            DrawWeight(g, sx + sw * 0.62f + 4f, my, 7f);
            g.Restore(st0);
        }
        private void DrawHammer(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            float cx = a.Left + a.Width * 0.5f;
            // 吊起的桩锤箱体
            g.Box(new RectangleF(cx - 26f, a.Top + 12f, 52f, a.Height * 0.34f), 3f,
                HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.4f);
            // 偏心块轴
            float cy = a.Top + 12f + a.Height * 0.17f;
            using (var shaft = new Pen(Color.FromArgb(70, 74, 80), 3.6f))
                g.DrawLine(shaft, cx - 30f, cy, cx + 30f, cy);
            DrawWeight(g, cx - 22f, cy, 10f);
            DrawWeight(g, cx + 22f, cy, 10f);
            // 夹头 + 桩
            g.Box(new RectangleF(cx - 14f, a.Top + 12f + a.Height * 0.34f, 28f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            g.Box(new RectangleF(cx - 6f, a.Bottom - 30f, 12f, 30f), 1f,
                Color.FromArgb(168, 128, 78), pal.Edge, 1.2f);
            // 吊缆
            g.Line(pal.Edge, 2f, cx, a.Top + 2f, cx, a.Top + 12f);
            g.Restore(st0);
            DrawWaves(g, a, cx, a.Bottom - 4f);
        }
        private void DrawExciter(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 箱式激振器：方箱体 + 两根偏心轴（齿轮同步）
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.5f;
            float w = a.Width * 0.62f, h = a.Height * 0.52f;
            g.Box(new RectangleF(cx - w / 2f, cy - h / 2f, w, h), 4f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.5f);
            float dy = h * 0.2f, dx = w * 0.18f;
            foreach (int s in new[] { -1, 1 })
            {
                var stt = g.Save();
                g.TranslateTransform(cx + s * dx, cy + (s < 0 ? -dy : dy));
                g.RotateTransform(s < 0 ? SpinAngle : -SpinAngle);
                using (var wb = new SolidBrush(Color.FromArgb(96, 100, 106)))
                {
                    g.FillEllipse(wb, -9f, -9f, 18f, 18f);
                    g.FillPie(Brushes.DimGray, -9f, -9f, 18f, 18f, 0f, 90f);
                }
                g.Restore(stt);
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), cx + s * dx - 9f, cy + (s < 0 ? -dy : dy) - 9f, 18f, 18f);
            }
            // 安装底座
            g.Line(pal.MetalDark, 5f, cx - w / 2f - 4f, a.Bottom - 8f, cx + w / 2f + 4f, a.Bottom - 8f);
            g.Restore(st0);
            DrawWaves(g, a, cx, a.Bottom - 6f);
        }
        private void DrawKnocker(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 仓壁（竖板）
            g.Box(new RectangleF(a.Right - 14f, a.Top + 6f, 10f, a.Height - 12f), 2f,
                pal.MetalDark, pal.Edge, 1.3f);
            // 气缸体
            float cx = a.Left + a.Width * 0.44f, cy = a.Top + a.Height * 0.5f;
            g.Box(new RectangleF(cx - 26f, cy - 14f, 44f, 28f), 5f,
                HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.4f);
            // 冲击活塞（伸出撞向仓壁）
            float ext = Running ? 6f + (float)Math.Abs(Math.Sin(Phase * 3f)) * 8f : 4f;
            g.Box(new RectangleF(cx + 18f, cy - 5f, a.Right - 14f - (cx + 18f) + ext - 2f, 10f), 2f,
                pal.Metal, pal.Edge, 1.1f);
            // 气管
            using (var hose = new Pen(Color.FromArgb(58, 60, 66), 3.4f))
                g.DrawBezier(hose, cx - 26f, cy - 10f, cx - 40f, cy - 26f, a.Left + 6f, cy - 10f,
                    a.Left + 6f, a.Top + 6f);
            g.Restore(st0);
            // 撞击星芒
            if (Running)
            {
                float hit = (float)Math.Abs(Math.Sin(Phase * 3f));
                if (hit > 0.7f)
                    using (var sp = new Pen(Color.FromArgb((int)(200 * hit), pal.Accent), 1.6f))
                        for (int k = 0; k < 6; k++)
                        {
                            double an = k * Math.PI / 3.0;
                            float px = a.Right - 16f, py = cy;
                            g.DrawLine(sp, px + (float)Math.Cos(an) * 6f, py + (float)Math.Sin(an) * 6f,
                                px + (float)Math.Cos(an) * 12f, py + (float)Math.Sin(an) * 12f);
                        }
            }
        }
        /// <summary>气动振动器：0 滚珠、1 涡轮、2 活塞。</summary>
        private void DrawPneu(Graphics g, RectangleF a, HToolPalette pal, int kind)
        {
            var st0 = Jitter(g);
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.5f;
            float r = a.Height * 0.3f;
            // 圆/方壳体
            if (kind == 2)
                g.Box(new RectangleF(cx - r * 1.5f, cy - r, r * 3f, r * 2f), 5f,
                    HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.4f);
            else
            {
                g.Ell(cx, cy, r, r, HToolPalettes.Shade(pal.Main, 0.15f));
                g.DrawEllipse(new Pen(pal.Edge, 1.5f), cx - r, cy - r, r * 2f, r * 2f);
            }
            if (kind == 0)
            {
                // 钢珠沿环道跑
                double an = Running ? Phase * 3f : Math.PI / 4.0;
                float bx = cx + (float)Math.Cos(an) * r * 0.62f;
                float by = cy + (float)Math.Sin(an) * r * 0.62f;
                g.Ell(bx, by, r * 0.18f, r * 0.18f, Color.FromArgb(220, 222, 228));
                using (var track = new Pen(pal.Edge, 1.2f))
                    g.DrawEllipse(track, cx - r * 0.62f, cy - r * 0.62f, r * 1.24f, r * 1.24f);
            }
            else if (kind == 1)
            {
                // 涡轮
                g.FanWheel(cx, cy, r * 0.78f, 8, SpinAngle, Running, pal);
            }
            else
            {
                // 活塞往复
                float px = Running ? (float)Math.Sin(Phase * 3f) * r * 0.9f : 0f;
                g.Box(new RectangleF(cx - 6f + px, cy - 6f, 12f, 12f), 2f,
                    pal.Metal, pal.Edge, 1.1f);
                g.Line(pal.Edge, 2f, cx - r * 1.5f, cy, cx + r * 1.5f, cy);
            }
            // 气口
            g.Line(Color.FromArgb(58, 60, 66), 3.4f, cx - 6f, cy - r, cx - 6f, a.Top + 4f);
            g.Restore(st0);
            DrawWaves(g, a, cx, cy);
        }
        private void DrawLinear(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 直振送料器：板簧斜腿 + 料槽 + 前进小零件
            float ty = a.Top + a.Height * 0.36f;
            g.Box(new RectangleF(a.Left + a.Width * 0.1f, ty, a.Width * 0.8f, 9f), 2f,
                HToolPalettes.Tint(pal.Metal, 0.12f), pal.Edge, 1.2f);
            // 板簧
            foreach (float lx in new[] { a.Left + a.Width * 0.24f, a.Right - a.Width * 0.24f })
                g.Line(pal.Edge, 2.6f, lx - 8f, ty + 9f, lx + 6f, a.Bottom - 12f);
            // 底座 + 电磁铁
            g.Box(new RectangleF(a.Left + a.Width * 0.18f, a.Bottom - 14f, a.Width * 0.64f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            g.Box(new RectangleF(a.Left + a.Width * 0.44f, ty + 12f, a.Width * 0.12f, a.Bottom - 14f - ty - 12f),
                2f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.1f);
            // 小零件前进
            float move = Running ? Phase / (float)Math.PI * 0.5f * a.Width : 0f;
            using (var part = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.2f)))
                for (int i = 0; i < 5; i++)
                {
                    float span = a.Width * 0.72f;
                    float x = a.Left + a.Width * 0.14f + ((i * 34f - move) % span + span) % span;
                    g.FillRectangle(part, x, ty - 6f, 8f, 6f);
                }
            g.Restore(st0);
        }
        private void DrawBowl(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.62f;
            // 减振脚垫
            foreach (float lx in new[] { cx - 30f, cx + 30f })
                g.Box(new RectangleF(lx - 6f, a.Bottom - 8f, 12f, 6f), 2f,
                    Color.FromArgb(60, 62, 68), pal.Edge, 1f);
            // 底座（振动电磁铁）
            g.Box(new RectangleF(cx - 24f, cy + 8f, 48f, a.Bottom - 14f - cy - 8f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            // 螺旋料盘（俯视梯形 + 螺旋线）
            var bowl = new[]
            {
                new PointF(cx - 44f, cy + 8f), new PointF(cx + 44f, cy + 8f),
                new PointF(cx + 30f, cy - 34f), new PointF(cx - 30f, cy - 34f)
            };
            g.Poly(bowl, HToolPalettes.Tint(pal.Metal, 0.18f), pal.Edge, 1.4f);
            float rot = Running ? Phase * 0.6f : 0f;
            using (var sp = new Pen(HToolPalettes.Shade(pal.Metal, 0.25f), 1.6f))
            {
                var pts = new PointF[40];
                for (int i = 0; i < pts.Length; i++)
                {
                    double t = i / 39.0;
                    double an = rot + t * Math.PI * 2.4;
                    float rr = 6f + (float)t * 32f;
                    pts[i] = new PointF(cx + (float)Math.Cos(an) * rr,
                        cy - 12f - (float)Math.Sin(an) * rr * 0.5f);
                }
                g.DrawLines(sp, pts);
            }
            // 出料轨道
            g.Line(pal.MetalDark, 3f, cx + 44f, cy - 24f, a.Right - 4f, cy - 30f);
            g.Restore(st0);
            DrawWaves(g, a, cx, cy);
        }
        private void DrawImpactBed(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            float cy = a.Top + a.Height * 0.5f;
            // 落料斗 + 缓冲床（弹簧 + 托板）
            g.Poly(new[]
            {
                new PointF(a.Left + a.Width * 0.3f, a.Top + 2f),
                new PointF(a.Left + a.Width * 0.62f, a.Top + 2f),
                new PointF(a.Left + a.Width * 0.54f, cy - 16f),
                new PointF(a.Left + a.Width * 0.38f, cy - 16f)
            }, pal.MetalDark, pal.Edge, 1.3f);
            // 下落骨料
            using (var stone = new SolidBrush(Color.FromArgb(149, 112, 72)))
                for (int i = 0; i < 4; i++)
                {
                    float fall = Running ? (Phase * 30f + i * 14f) % 30f : i * 7f;
                    g.FillEllipse(stone, a.Left + a.Width * 0.42f + (i % 2) * 14f,
                        cy - 14f + fall, 7f, 7f);
                }
            // 缓冲托板
            g.Box(new RectangleF(a.Left + a.Width * 0.22f, cy + 4f, a.Width * 0.56f, 8f), 2f,
                pal.Metal, pal.Edge, 1.2f);
            // 弹簧
            foreach (float lx in new[] { a.Left + a.Width * 0.3f, a.Right - a.Width * 0.3f })
            {
                using (var sp = new Pen(pal.Edge, 1.5f))
                    for (int k = 0; k < 4; k++)
                        g.DrawArc(sp, lx - 5f, cy + 14f + k * 5f, 10f, 6f, 0f, 180f);
            }
            g.Line(pal.MetalDark, 4f, a.Left + a.Width * 0.2f, a.Bottom - 6f,
                a.Right - a.Width * 0.2f, a.Bottom - 6f);
            g.Restore(st0);
        }
        private void DrawScreed(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = Jitter(g);
            // 振动梁：长铝合金梁 + 偏心振动单元 + 扶手
            float by = a.Top + a.Height * 0.56f;
            g.Box(new RectangleF(a.Left + a.Width * 0.06f, by, a.Width * 0.88f, 10f), 2f,
                HToolPalettes.Tint(pal.Metal, 0.1f), pal.Edge, 1.3f);
            // 梁上振动电机
            float mx = a.Left + a.Width * 0.4f;
            g.Motor(mx, mx + a.Width * 0.22f, by - 12f, 10f, pal);
            DrawWeight(g, mx - 4f, by - 12f, 7f);
            DrawWeight(g, mx + a.Width * 0.22f + 4f, by - 12f, 7f);
            // 双扶手
            foreach (float hx in new[] { a.Left + a.Width * 0.3f, a.Left + a.Width * 0.62f })
                g.Line(pal.MetalDark, 3f, hx, by, hx + 14f, a.Top + 8f);
            g.Line(pal.MetalDark, 4f, a.Left + a.Width * 0.3f + 14f, a.Top + 8f,
                a.Left + a.Width * 0.62f + 14f, a.Top + 8f);
            g.Restore(st0);
            // 梁底波纹
            if (Running)
                using (var arc = new Pen(Color.FromArgb(150, pal.Accent), 1.5f))
                    for (int k = 0; k < 3; k++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + k / 3f) % 1f;
                        arc.Color = Color.FromArgb((int)(160 * (1f - t)), pal.Accent);
                        float x = a.Left + a.Width * (0.15f + t * 0.7f);
                        g.DrawArc(arc, x - 9f, by + 8f, 18f, 12f, 200f, 140f);
                    }
        }
    }
}