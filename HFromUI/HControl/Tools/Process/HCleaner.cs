using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Process
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 清洗设备样式：Classic 为立式喷淋清洗机，其余 21 种覆盖超声/兆声、湿法多槽、
    /// 旋转喷淋/甩干、滚刷、等离子、紫外臭氧、CO2/干冰/激光/蒸汽清洗、溶剂浸泡、
    /// 溢流级联、IPA 干燥、真空、CMP、石英管与机械手自动线等。
    /// </summary>
    public enum HCleanerStyle
    {
        Classic = 0,      // 立式喷淋清洗机
        Ultrasonic = 1,   // 超声波清洗机
        Megasonic = 2,    // 兆声波清洗机
        WetBank = 3,      // RCA 湿法多槽
        Spray = 4,        // 旋转喷淋腔
        Brush = 5,        // 滚刷清洗机
        Plasma = 6,       // 等离子清洗
        UV = 7,           // 紫外臭氧清洗
        CO2Snow = 8,      // CO2 雪清洗
        Laser = 9,        // 激光清洗
        Steam = 10,       // 蒸汽清洗
        HighPressure = 11,// 高压水清洗
        Solvent = 12,     // 溶剂浸泡槽
        Cascade = 13,     // 多级溢流槽
        IPADry = 14,      // IPA 蒸气干燥塔
        SpinDryer = 15,   // 离心甩干机
        Vacuum = 16,      // 真空清洗腔
        CMP = 17,         // 化学机械清洗
        Scrubber = 18,    // 晶圆刷洗机
        QuartzTube = 19,  // 石英管清洗
        DryIce = 20,      // 干冰清洗机
        Robotic = 21      // 机械手自动线
    }
    /// <summary>
    /// 清洗机控件（继承 HToolAnimBase）：22 种清洗工艺设备外形、13 种色调，
    /// Running 时喷淋、气泡、辉光、旋转臂等按工艺产生动画。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("清洗设备控件：22 种清洗工艺外形、13 种色调，运行时显示喷淋/气泡/辉光动画")]
    public class HCleaner : HToolAnimBase
    {
        private HCleanerStyle _style = HCleanerStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HCleaner() { Size = new Size(130, 150); }
        /// <summary>清洗设备外形样式。</summary>
        [HCategoryLanguage("清洗机"), HDisplayNameLanguage("清洗设备外形样式"), HDescriptionLanguage("清洗设备外形样式"), Browsable(true)]
        [DefaultValue(HCleanerStyle.Classic)]
        public HCleanerStyle CleanerStyle
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
                case HCleanerStyle.Ultrasonic: DrawUltrasonic(g, a, pal, false); break;
                case HCleanerStyle.Megasonic: DrawUltrasonic(g, a, pal, true); break;
                case HCleanerStyle.WetBank: DrawWetBank(g, a, pal); break;
                case HCleanerStyle.Spray: DrawSprayChamber(g, a, pal); break;
                case HCleanerStyle.Brush: DrawBrush(g, a, pal); break;
                case HCleanerStyle.Plasma: DrawPlasma(g, a, pal); break;
                case HCleanerStyle.UV: DrawUV(g, a, pal); break;
                case HCleanerStyle.CO2Snow: DrawJetGun(g, a, pal, 0); break;
                case HCleanerStyle.DryIce: DrawJetGun(g, a, pal, 1); break;
                case HCleanerStyle.Steam: DrawJetGun(g, a, pal, 2); break;
                case HCleanerStyle.Laser: DrawLaser(g, a, pal); break;
                case HCleanerStyle.HighPressure: DrawHighPressure(g, a, pal); break;
                case HCleanerStyle.Solvent: DrawSolvent(g, a, pal); break;
                case HCleanerStyle.Cascade: DrawCascade(g, a, pal); break;
                case HCleanerStyle.IPADry: DrawIPA(g, a, pal); break;
                case HCleanerStyle.SpinDryer: DrawSpinDryer(g, a, pal); break;
                case HCleanerStyle.Vacuum: DrawVacuum(g, a, pal); break;
                case HCleanerStyle.CMP: DrawCMP(g, a, pal); break;
                case HCleanerStyle.Scrubber: DrawScrubber(g, a, pal); break;
                case HCleanerStyle.QuartzTube: DrawQuartzTube(g, a, pal); break;
                case HCleanerStyle.Robotic: DrawRobotic(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>槽体内清洗液（带液面线）。</summary>
        private void Liquid(Graphics g, RectangleF r, float ratio, Color c)
        {
            if (ratio <= 0f) return;
            var lr = new RectangleF(r.X, r.Bottom - r.Height * ratio, r.Width, r.Height * ratio);
            using (var b = new SolidBrush(Color.FromArgb(150, c)))
                g.FillRectangle(b, lr);
            g.Line(Color.FromArgb(180, c), 1.2f, lr.X, lr.Y, lr.Right, lr.Y);
        }
        /// <summary>上升气泡。</summary>
        private void Bubbles(Graphics g, RectangleF r, int n)
        {
            if (!Running) return;
            float move = Phase / (float)Math.PI * 0.5f;
            using (var bub = new SolidBrush(Color.FromArgb(150, 225, 240, 250)))
                for (int i = 0; i < n; i++)
                {
                    float t = (move + i * 0.17f) % 1f;
                    float bx = r.X + r.Width * (0.12f + 0.76f * ((i * 37) % 100) / 100f);
                    float by = r.Bottom - 4f - t * (r.Height - 8f);
                    float d = 2.2f + (i % 3);
                    g.FillEllipse(bub, bx, by, d, d);
                }
        }
        // ----------------------------------------------------------------
        // Classic：立式喷淋清洗机
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.74f, x0 = a.Left + (a.Width - w) / 2f;
            float top = a.Top + a.Height * 0.08f, bot = a.Bottom - a.Height * 0.08f;
            var body = new RectangleF(x0, top, w, bot - top);
            using (var gp = HBarBase.RoundPath(body, 8f))
            using (var b = new LinearGradientBrush(body, HToolPalettes.Tint(pal.Main, 0.22f),
                HToolPalettes.Shade(pal.Main, 0.25f), 0f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.6f), gp);
            }
            using (var foot = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(foot, x0 + 6f, bot, w - 12f, 6f);
                g.FillRectangle(foot, x0 + 8f, bot + 6f, 6f, 5f);
                g.FillRectangle(foot, x0 + w - 14f, bot + 6f, 6f, 5f);
            }
            var door = RectangleF.Inflate(body, -w * 0.12f, -(bot - top) * 0.14f);
            door.Height -= (bot - top) * 0.12f;
            using (var glass = new LinearGradientBrush(door, Color.FromArgb(150, 206, 232),
                Color.FromArgb(120, 90, 130, 168), 90f))
                g.FillRectangle(glass, door);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), door.X, door.Y, door.Width, door.Height);
            g.Line(pal.Metal, 4f, door.Left + 4, door.Y + 2, door.Right - 4, door.Y + 2);
            int nozzles = 4;
            using (var nozzle = new SolidBrush(pal.MetalDark))
                for (int i = 0; i < nozzles; i++)
                {
                    float nx = door.Left + door.Width * (i + 0.5f) / nozzles;
                    g.FillRectangle(nozzle, nx - 2.5f, door.Y + 3f, 5f, 6f);
                }
            g.FillRectangle(new SolidBrush(pal.Metal), door.Right - 7f, door.Y + door.Height * 0.32f, 4f, door.Height * 0.22f);
            // 料篮
            float basketY = door.Bottom - door.Height * 0.3f;
            var basket = new RectangleF(door.Left + 8f, basketY, door.Width - 16f, door.Height * 0.24f);
            using (var wire = new Pen(Color.FromArgb(200, pal.Metal), 1.2f))
            {
                g.DrawRectangle(wire, basket.X, basket.Y, basket.Width, basket.Height);
                for (int i = 1; i < 4; i++)
                    g.DrawLine(wire, basket.X + basket.Width * i / 4f, basket.Y,
                        basket.X + basket.Width * i / 4f, basket.Bottom);
                g.DrawLine(wire, basket.X, basket.Y + basket.Height / 2f,
                    basket.Right, basket.Y + basket.Height / 2f);
            }
            // 喷淋水雾
            if (Running)
                using (var spray = new Pen(Color.FromArgb(180, Color.White), 1.6f))
                    for (int i = 0; i < nozzles; i++)
                    {
                        float nx = door.Left + door.Width * (i + 0.5f) / nozzles;
                        for (int k = 0; k < 3; k++)
                        {
                            float t = (Phase / (float)Math.PI * 0.5f + k / 3f + i * 0.07f) % 1f;
                            float sy = door.Y + 11f + t * (basketY - door.Y - 12f);
                            spray.Color = Color.FromArgb((int)(190 * (1f - t)), 210, 236, 250);
                            g.DrawLine(spray, nx - 3f + k * 3f, sy, nx - 3f + k * 3f, sy + 5f);
                        }
                    }
            Bubbles(g, new RectangleF(door.X, door.Y + door.Height * 0.4f, door.Width, door.Height * 0.5f), 6);
            g.Lamp(x0 + w - 10f, top + 10f, Running);
        }
        // ----------------------------------------------------------------
        // Ultrasonic / Megasonic：敞口液槽 + 换能器
        // ----------------------------------------------------------------
        private void DrawUltrasonic(Graphics g, RectangleF a, HToolPalette pal, bool mega)
        {
            var tank = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.26f,
                a.Width * 0.76f, a.Height * 0.56f);
            // 槽体
            g.Box(tank, 3f, HToolPalettes.Tint(pal.Main, 0.24f), pal.Edge, 1.5f);
            Liquid(g, tank, 0.68f, Color.FromArgb(96, 170, 210));
            // 换能器（底部圆盘阵列 / 兆声棒）
            if (mega)
            {
                g.Line(pal.MetalDark, 3f, tank.Left + 8f, tank.Y + 6f, tank.Right - 8f, tank.Y + 6f);
                g.Box(new RectangleF(tank.Right - 14f, tank.Top - 8f, 10f, 8f), 2f,
                    pal.MetalDark, pal.Edge, 1f);
            }
            else
                using (var td = new SolidBrush(pal.MetalDark))
                    for (int i = 0; i < 4; i++)
                        g.FillEllipse(td, tank.X + tank.Width * (0.16f + 0.22f * i) - 5f,
                            tank.Bottom - 9f, 10f, 7f);
            // 料篮（沉浸）
            var basket = new RectangleF(tank.X + 8f, tank.Bottom - tank.Height * 0.42f,
                tank.Width - 16f, tank.Height * 0.3f);
            using (var wire = new Pen(Color.FromArgb(200, pal.Metal), 1.1f))
            {
                g.DrawRectangle(wire, basket.X, basket.Y, basket.Width, basket.Height);
                for (int i = 1; i < 4; i++)
                    g.DrawLine(wire, basket.X + basket.Width * i / 4f, basket.Y,
                        basket.X + basket.Width * i / 4f, basket.Bottom);
            }
            Bubbles(g, new RectangleF(tank.X, tank.Y + 8f, tank.Width, tank.Height * 0.6f), mega ? 5 : 9);
            // 超声波纹
            if (Running)
                using (var arc = new Pen(Color.FromArgb(150, pal.Accent), 1.5f))
                    for (int k = 0; k < 3; k++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + k / 3f) % 1f;
                        arc.Color = Color.FromArgb((int)(160 * (1f - t)), pal.Accent);
                        float rr = 8f + t * 18f;
                        g.DrawArc(arc, tank.Left + tank.Width * 0.3f - rr, tank.Bottom - rr - 4f,
                            rr * 2f, rr * 2f, 200f, 140f);
                    }
            // 控制盒
            g.Box(new RectangleF(tank.Right - 24f, tank.Top - 16f, 22f, 12f), 2f,
                HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.1f);
            g.Lamp(tank.Right - 13f, tank.Top - 10f, Running);
        }
        // ----------------------------------------------------------------
        // WetBank：湿法多槽 + 排风罩
        // ----------------------------------------------------------------
        private void DrawWetBank(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 排风罩
            g.Poly(new[]
            {
                new PointF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.12f),
                new PointF(a.Right - a.Width * 0.08f, a.Top + a.Height * 0.12f),
                new PointF(a.Right - a.Width * 0.2f, a.Top + a.Height * 0.02f),
                new PointF(a.Left + a.Width * 0.2f, a.Top + a.Height * 0.02f)
            }, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.3f);
            // 三个药槽
            int n = 3;
            float tw = a.Width * 0.26f, gap = a.Width * 0.04f;
            float total = n * tw + (n - 1) * gap;
            float x0 = a.Left + (a.Width - total) / 2f;
            var liquidCols = new[] { Color.FromArgb(120, 200, 120), Color.FromArgb(120, 180, 220), Color.FromArgb(150, 150, 160) };
            for (int i = 0; i < n; i++)
            {
                var tank = new RectangleF(x0 + i * (tw + gap), a.Top + a.Height * 0.34f,
                    tw, a.Height * 0.46f);
                g.Box(tank, 2f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.3f);
                Liquid(g, tank, 0.6f, liquidCols[i]);
                Bubbles(g, new RectangleF(tank.X, tank.Y + 10f, tank.Width, tank.Height * 0.5f), 3);
            }
            // 管路
            using (var pipe = new Pen(pal.MetalDark, 2.4f))
                g.DrawLine(pipe, x0, a.Top + a.Height * 0.26f, x0 + total, a.Top + a.Height * 0.26f);
            // 底座
            g.Box(new RectangleF(x0 - 6f, a.Bottom - a.Height * 0.14f, total + 12f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
        }
        // ----------------------------------------------------------------
        // Spray：旋转喷淋腔
        // ----------------------------------------------------------------
        private void DrawSprayChamber(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.46f;
            float r = Math.Min(a.Width, a.Height) * 0.34f;
            // 圆筒腔
            var body = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            g.Box(body, 8f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.5f);
            // 晶圆（立）
            g.Box(new RectangleF(cx - 4f, cy - r * 0.7f, 8f, r * 1.4f), 2f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // 中心旋转喷淋臂
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 30f);
            using (var arm = new Pen(pal.MetalDark, 3f))
            {
                g.DrawLine(arm, -r * 0.7f, 0, r * 0.7f, 0);
                g.DrawLine(arm, -r * 0.35f, -r * 0.6f, -r * 0.35f, r * 0.6f);
            }
            g.Restore(st);
            g.Ell(cx, cy, 4f, 4f, pal.Metal);
            // 甩出的水滴
            if (Running)
                using (var drop = new SolidBrush(Color.FromArgb(170, 214, 236, 250)))
                    for (int i = 0; i < 8; i++)
                    {
                        double ang = i * Math.PI / 4.0 + Phase * 0.8;
                        float rr = r * (0.78f + 0.1f * (float)Math.Sin(Phase * 2f + i));
                        g.FillEllipse(drop, cx + (float)Math.Cos(ang) * rr - 1.6f,
                            cy + (float)Math.Sin(ang) * rr - 1.6f, 3.2f, 3.2f);
                    }
            g.Lamp(cx + r - 10f, cy - r + 8f, Running);
        }
        // ----------------------------------------------------------------
        // Brush：滚刷清洗（滚刷 + 喷淋 + 输送）
        // ----------------------------------------------------------------
        private void DrawBrush(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.44f;
            float rw = a.Width * 0.36f, rh = a.Height * 0.2f;
            // 滚刷圆柱
            var roller = new RectangleF(cx - rw, cy - rh, rw * 2f, rh * 2f);
            g.FillCylH(roller, HToolPalettes.Shade(pal.Main, 0.25f), HToolPalettes.Tint(pal.Main, 0.25f));
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), roller.X, roller.Y, roller.Width, roller.Height);
            // 刷毛（旋转放射）
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 20f);
            using (var br = new Pen(Color.FromArgb(90, 86, 80), 1.4f))
                for (int i = 0; i < 12; i++)
                {
                    g.RotateTransform(30f);
                    g.DrawLine(br, rw * 0.9f, 0, rw * 1.14f, 0);
                }
            g.Restore(st);
            // 输送带上的晶圆
            float beltY = cy + rh + a.Height * 0.14f;
            g.Box(new RectangleF(a.Left + a.Width * 0.12f, beltY, a.Width * 0.76f, 7f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            g.Box(new RectangleF(cx - 16f, beltY - 6f, 32f, 5f), 1f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // 喷淋管
            g.Line(pal.Metal, 3.2f, cx - rw * 0.8f, cy - rh - 10f, cx + rw * 0.8f, cy - rh - 10f);
            if (Running)
                using (var sp = new Pen(Color.FromArgb(170, 214, 236, 250), 1.5f))
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.2f) % 1f;
                        float x = cx - rw * 0.7f + i * rw * 0.35f;
                        sp.Color = Color.FromArgb((int)(180 * (1f - t)), 214, 236, 250);
                        g.DrawLine(sp, x, cy - rh - 8f, x, cy - rh - 8f + t * 14f);
                    }
        }
        // ----------------------------------------------------------------
        // Plasma：真空等离子腔（辉光）
        // ----------------------------------------------------------------
        private void DrawPlasma(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.16f, bot = a.Bottom - a.Height * 0.12f;
            float w = a.Width * 0.56f;
            // 钟罩腔
            var body = new RectangleF(cx - w / 2f, top + 12f, w, bot - top - 12f);
            using (var gp = new GraphicsPath())
            {
                gp.AddArc(cx - w / 2f, top, w, 26f, 180f, 180f);
                gp.AddRectangle(body);
                gp.CloseFigure();
                using (var b = new LinearGradientBrush(body, Color.FromArgb(60, 64, 78),
                    Color.FromArgb(36, 40, 52), 0f))
                    g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.5f), gp);
            }
            // 电极 + 晶圆
            g.Line(pal.Metal, 3f, cx - w * 0.32f, top + 22f, cx + w * 0.32f, top + 22f);
            g.Box(new RectangleF(cx - 18f, top + 26f, 36f, 5f), 1f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // 紫色辉光
            if (Running)
            {
                int glow = (int)(90 + 60 * Math.Abs(Math.Sin(Phase * 2.4f)));
                using (var pl = new SolidBrush(Color.FromArgb(glow / 2, 168, 96, 230)))
                    g.FillEllipse(pl, cx - w * 0.36f, top + 30f, w * 0.72f, a.Height * 0.34f);
                using (var bolt = new Pen(Color.FromArgb(200, 200, 140, 255), 1.3f))
                    for (int i = 0; i < 3; i++)
                    {
                        float x = cx + (i - 1) * 12f;
                        g.DrawLine(bolt, x, top + 24f, x + (i % 2 == 0 ? 4f : -4f), top + 44f);
                    }
            }
            // 真空管路 + 压力表
            g.Line(pal.MetalDark, 2.6f, cx + w / 2f, top + 14f, a.Right - 6f, top + 6f);
            g.Ell(cx - w / 2f - 8f, top + 16f, 5f, 5f, Color.White);
            g.DrawEllipse(new Pen(pal.Edge, 1f), cx - w / 2f - 13f, top + 11f, 10f, 10f);
        }
        // ----------------------------------------------------------------
        // UV：紫外臭氧清洗（紫光灯管）
        // ----------------------------------------------------------------
        private void DrawUV(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.68f;
            var body = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + a.Height * 0.16f,
                w, a.Height * 0.62f);
            g.Box(body, 5f, HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.5f);
            // 抽屉式托盘（拉出一点）
            var tray = new RectangleF(body.X + 6f, body.Bottom - 16f, body.Width - 12f, 12f);
            g.Box(tray, 2f, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(tray.X + 8f, tray.Y - 5f, 26f, 5f), 1f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // UV 灯管
            float ly = body.Y + body.Height * 0.3f;
            if (Running)
                using (var halo = new SolidBrush(Color.FromArgb(70, 150, 110, 255)))
                    g.FillEllipse(halo, body.X + 6f, ly - 12f, body.Width - 12f, 24f);
            using (var tube = new Pen(Running ? Color.FromArgb(220, 180, 130, 255) : Color.FromArgb(120, 110, 150), 5f))
            {
                g.DrawLine(tube, body.X + 12f, ly, body.Right - 12f, ly);
                g.DrawLine(tube, body.X + 12f, ly + 16f, body.Right - 12f, ly + 16f);
            }
            g.Lamp(body.Right - 12f, body.Y + 8f, Running);
        }
        // ----------------------------------------------------------------
        // JetGun：喷枪式清洗（0 CO2 雪、1 干冰颗粒、2 蒸汽）
        // ----------------------------------------------------------------
        private void DrawJetGun(Graphics g, RectangleF a, HToolPalette pal, int kind)
        {
            float hx = a.Left + a.Width * 0.34f, hy = a.Top + a.Height * 0.3f;
            // 枪体
            g.Box(new RectangleF(hx - 26f, hy - 9f, 44f, 18f), 6f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            g.Poly(new[]
            {
                new PointF(hx - 14f, hy + 8f), new PointF(hx + 2f, hy + 8f),
                new PointF(hx - 2f, hy + 30f), new PointF(hx - 16f, hy + 30f)
            }, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            // 喷嘴
            float nx = hx + 18f, ny = hy;
            g.Poly(new[]
            {
                new PointF(nx, ny - 6f), new PointF(nx + 20f, ny - 2.5f),
                new PointF(nx + 20f, ny + 2.5f), new PointF(nx, ny + 6f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 软管
            using (var hose = new Pen(Color.FromArgb(58, 60, 66), 4f))
                g.DrawBezier(hose, hx - 26f, hy, hx - 42f, hy - 14f,
                    a.Left + 10f, hy + 10f, a.Left + 10f, a.Bottom - 8f);
            // 工件
            g.Box(new RectangleF(a.Right - a.Width * 0.3f, a.Bottom - a.Height * 0.3f,
                a.Width * 0.24f, a.Height * 0.18f), 2f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.2f);
            // 喷射介质
            if (Running)
            {
                float move = Phase / (float)Math.PI * 0.5f;
                for (int i = 0; i < 9; i++)
                {
                    float t = (move + i * 0.11f) % 1f;
                    float px = nx + 20f + t * (a.Right - a.Width * 0.3f - nx - 20f);
                    float spread = t * t * 12f;
                    float py = ny + (i % 3 - 1) * spread;
                    if (kind == 2)
                        // 蒸汽：淡白渐扩云
                        using (var cl = new SolidBrush(Color.FromArgb((int)(120 * (1f - t)), 240, 240, 244)))
                            g.FillEllipse(cl, px - 3f - t * 3f, py - 2f - t * 2f, 6f + t * 6f, 4f + t * 4f);
                    else
                        // CO2 雪 / 干冰颗粒
                        using (var pt = new SolidBrush(kind == 0
                            ? Color.FromArgb(220, 240, 250) : Color.FromArgb(210, 214, 222)))
                        {
                            float d = kind == 0 ? 2.6f : 3.6f;
                            g.FillEllipse(pt, px - d / 2f, py - d / 2f, d, d);
                        }
                }
            }
        }
        // ----------------------------------------------------------------
        // Laser：激光清洗头
        // ----------------------------------------------------------------
        private void DrawLaser(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 主机
            g.Box(new RectangleF(a.Left + 8f, a.Bottom - a.Height * 0.42f, a.Width * 0.32f, a.Height * 0.34f),
                4f, HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.3f);
            // 光纤 + 枪头
            using (var fiber = new Pen(Color.FromArgb(58, 60, 66), 3f))
                g.DrawBezier(fiber, a.Left + 8f + a.Width * 0.32f, a.Bottom - a.Height * 0.36f,
                    a.Left + a.Width * 0.6f, a.Bottom - a.Height * 0.5f,
                    a.Left + a.Width * 0.5f, a.Top + a.Height * 0.26f,
                    a.Left + a.Width * 0.62f, a.Top + a.Height * 0.34f);
            float hx = a.Left + a.Width * 0.62f, hy = a.Top + a.Height * 0.34f;
            g.Box(new RectangleF(hx - 8f, hy - 6f, 30f, 12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.2f);
            // 工件
            g.Box(new RectangleF(a.Right - a.Width * 0.36f, a.Bottom - a.Height * 0.26f,
                a.Width * 0.3f, a.Height * 0.14f), 2f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.2f);
            // 红色激光束（扫描折线）
            if (Running)
            {
                float tx = a.Right - a.Width * 0.36f + a.Width * 0.15f * (float)Math.Sin(Phase * 2f);
                using (var beam = new Pen(Color.FromArgb(220, 255, 60, 40), 2.2f))
                    g.DrawLine(beam, hx + 22f, hy + 2f, tx, a.Bottom - a.Height * 0.26f);
                using (var gl = new SolidBrush(Color.FromArgb(80, 255, 80, 40)))
                    g.FillEllipse(gl, tx - 8f, a.Bottom - a.Height * 0.28f, 16f, 8f);
            }
            g.Lamp(a.Left + 14f, a.Bottom - a.Height * 0.4f + 6f, Running);
        }
        // ----------------------------------------------------------------
        // HighPressure：高压清洗机组 + 喷枪
        // ----------------------------------------------------------------
        private void DrawHighPressure(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 机组（带轮）
            var body = new RectangleF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.36f,
                a.Width * 0.44f, a.Height * 0.42f);
            g.Box(body, 5f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.4f);
            foreach (float wx in new[] { body.X + 10f, body.Right - 10f })
                g.Ell(wx, body.Bottom + 8f, 6f, 6f, Color.FromArgb(48, 50, 56));
            g.Line(pal.MetalDark, 2.6f, body.X + 6f, body.Y, body.X - 4f, body.Y - 16f);
            g.Line(pal.MetalDark, 3.4f, body.X - 4f, body.Y - 16f, body.X + 14f, body.Y - 16f);
            // 高压管
            using (var hose = new Pen(Color.FromArgb(58, 60, 66), 3.4f))
                g.DrawBezier(hose, body.Right, body.Y + 10f,
                    a.Left + a.Width * 0.7f, body.Y - 18f,
                    a.Left + a.Width * 0.66f, a.Top + a.Height * 0.3f,
                    a.Left + a.Width * 0.7f, a.Top + a.Height * 0.36f);
            // 喷枪 + 水柱
            float nx = a.Left + a.Width * 0.7f, ny = a.Top + a.Height * 0.36f;
            g.Line(pal.MetalDark, 4f, nx - 12f, ny + 4f, nx + 8f, ny);
            if (Running)
                using (var jet = new Pen(Color.FromArgb(180, 200, 228, 246), 2.4f))
                    for (int i = -1; i <= 1; i++)
                        g.DrawLine(jet, nx + 8f, ny + i * 2f,
                            a.Right - 6f, ny + 14f + i * 7f);
            g.Lamp(body.X + 8f, body.Y + 8f, Running);
        }
        // ----------------------------------------------------------------
        // Solvent：溶剂浸泡槽（冷凝盘管 + 盖）
        // ----------------------------------------------------------------
        private void DrawSolvent(Graphics g, RectangleF a, HToolPalette pal)
        {
            var tank = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.24f,
                a.Width * 0.72f, a.Height * 0.56f);
            g.Box(tank, 3f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.5f);
            Liquid(g, tank, 0.62f, Color.FromArgb(150, 170, 190));
            // 浸泡的网篮
            using (var wire = new Pen(Color.FromArgb(200, pal.Metal), 1.1f))
            {
                g.DrawRectangle(wire, tank.X + 8f, tank.Bottom - tank.Height * 0.4f,
                    tank.Width - 16f, tank.Height * 0.3f);
                g.DrawLine(wire, tank.X + tank.Width / 2f, tank.Bottom - tank.Height * 0.4f,
                    tank.X + tank.Width / 2f, tank.Bottom - tank.Height * 0.1f);
            }
            // 盖（半开）
            g.Poly(new[]
            {
                new PointF(tank.X, tank.Y), new PointF(tank.Right, tank.Y),
                new PointF(tank.Right - 6f, tank.Y - 12f), new PointF(tank.X + 6f, tank.Y - 12f)
            }, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.2f);
            // 冷凝盘管
            using (var coil = new Pen(Color.FromArgb(170, 200, 224), 2f))
                for (int i = 0; i < 5; i++)
                    g.DrawArc(coil, tank.X + 8f + i * (tank.Width - 16f) / 5f,
                        tank.Y - 22f, 16f, 16f, 0f, 180f);
            // 溶剂蒸气
            if (Running)
                using (var vap = new Pen(Color.FromArgb(120, 220, 224, 230), 1.3f))
                    for (int i = 0; i < 4; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.25f) % 1f;
                        float x = tank.X + tank.Width * (0.2f + 0.2f * i);
                        g.DrawLine(vap, x, tank.Y + 6f - t * 18f,
                            x + (float)Math.Sin(t * 6f) * 4f, tank.Y - 2f - t * 18f);
                    }
        }
        // ----------------------------------------------------------------
        // Cascade：多级溢流阶梯槽
        // ----------------------------------------------------------------
        private void DrawCascade(Graphics g, RectangleF a, HToolPalette pal)
        {
            int n = 3;
            float tw = a.Width * 0.26f;
            float baseY = a.Bottom - a.Height * 0.18f;
            for (int i = 0; i < n; i++)
            {
                float th = a.Height * (0.24f + 0.1f * i);
                var tank = new RectangleF(a.Left + a.Width * 0.08f + i * (tw + a.Width * 0.02f),
                    baseY - th, tw, th);
                g.Box(tank, 2f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.3f);
                Liquid(g, tank, 0.72f, Color.FromArgb(120, 180, 220));
                // 溢流瀑布到下一级
                if (i > 0 && Running)
                    using (var wf = new Pen(Color.FromArgb(160, 200, 228, 246), 2f))
                        g.DrawLine(wf, tank.X, tank.Y, tank.X, tank.Y + 10f);
            }
            Bubbles(g, new RectangleF(a.Left + a.Width * 0.1f, a.Top + a.Height * 0.3f,
                a.Width * 0.8f, a.Height * 0.4f), 6);
            // 进水管
            g.Line(pal.MetalDark, 2.6f, a.Left + 10f, a.Top + a.Height * 0.16f,
                a.Left + 10f, a.Top + a.Height * 0.32f);
        }
        // ----------------------------------------------------------------
        // IPADry：IPA 蒸气干燥塔（晶圆上升）
        // ----------------------------------------------------------------
        private void DrawIPA(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.44f;
            var body = new RectangleF(cx - w / 2f, a.Top + a.Height * 0.14f, w, a.Height * 0.68f);
            g.Box(body, 6f, HToolPalettes.Tint(pal.Main, 0.24f), pal.Edge, 1.5f);
            // 底部加热液
            Liquid(g, new RectangleF(body.X, body.Y, body.Width, body.Height), 0.24f,
                Color.FromArgb(170, 200, 180));
            // IPA 蒸气（上升波纹）
            if (Running)
                using (var vap = new Pen(Color.FromArgb(130, 225, 220, 240), 1.3f))
                    for (int i = 0; i < 6; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i / 6f) % 1f;
                        float x = body.X + body.Width * (0.2f + 0.6f * ((i * 37) % 100) / 100f);
                        float y = body.Bottom - 10f - t * (body.Height - 16f);
                        g.DrawLine(vap, x, y, x + (float)Math.Sin(t * 8f) * 3f, y - 7f);
                    }
            // 晶圆（缓慢提升）
            float lift = Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) : 0.5f;
            float wy = body.Bottom - 10f - lift * (body.Height * 0.55f);
            g.Box(new RectangleF(cx - 4f, wy, 8f, body.Height * 0.42f), 2f,
                Color.FromArgb(180, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // 顶吊臂
            g.Line(pal.MetalDark, 2.6f, cx, body.Top - 6f, cx, wy);
        }
        // ----------------------------------------------------------------
        // SpinDryer：离心甩干机
        // ----------------------------------------------------------------
        private void DrawSpinDryer(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.48f;
            float r = Math.Min(a.Width, a.Height) * 0.34f;
            // 圆筒 + 开盖
            var body = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            g.Box(body, 8f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.5f);
            g.Poly(new[]
            {
                new PointF(cx - r, cy - r), new PointF(cx + r, cy - r),
                new PointF(cx + r - 6f, cy - r - 14f), new PointF(cx - r + 6f, cy - r - 14f)
            }, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.2f);
            // 转鼓（栅条）+ 晶圆
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle * 1.5f : 0f);
            using (var bar = new Pen(pal.MetalDark, 1.4f))
                for (int i = 0; i < 8; i++)
                {
                    g.RotateTransform(45f);
                    g.DrawLine(bar, r * 0.45f, -r * 0.6f, r * 0.45f, r * 0.6f);
                }
            g.Restore(st);
            g.Box(new RectangleF(cx - 3f, cy - r * 0.55f, 6f, r * 1.1f), 2f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // 甩出水滴
            if (Running)
                using (var drop = new SolidBrush(Color.FromArgb(160, 214, 236, 250)))
                    for (int i = 0; i < 8; i++)
                    {
                        double ang = i * Math.PI / 4.0 + Phase;
                        float rr = r * (0.86f + 0.08f * (float)Math.Sin(Phase * 3f + i));
                        g.FillEllipse(drop, cx + (float)Math.Cos(ang) * rr - 1.6f,
                            cy + (float)Math.Sin(ang) * rr - 1.6f, 3.2f, 3.2f);
                    }
        }
        // ----------------------------------------------------------------
        // Vacuum：真空清洗腔（圆形观察门）
        // ----------------------------------------------------------------
        private void DrawVacuum(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            var body = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.14f,
                a.Width * 0.72f, a.Height * 0.68f);
            g.Box(body, 6f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.6f);
            // 圆观察门
            float dr = Math.Min(body.Width, body.Height) * 0.32f;
            g.Ell(cx, body.Top + body.Height * 0.42f, dr, dr, Color.FromArgb(50, 60, 76));
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - dr, body.Top + body.Height * 0.42f - dr, dr * 2f, dr * 2f);
            // 门铰链 + 把手
            g.Line(pal.MetalDark, 3f, body.Right - 6f, body.Top + body.Height * 0.2f,
                body.Right - 6f, body.Top + body.Height * 0.64f);
            g.Ell(cx + dr * 0.7f, body.Top + body.Height * 0.42f, 3f, 3f, pal.Metal);
            // 真空表 + 管路
            g.Ell(body.X + 12f, body.Top + 10f, 6f, 6f, Color.White);
            g.DrawEllipse(new Pen(pal.Edge, 1f), body.X + 6f, body.Top + 4f, 12f, 12f);
            g.Line(pal.MetalDark, 2.6f, body.Right, body.Top + 16f, a.Right - 6f, body.Top + 8f);
            // 辉光（运行）
            if (Running)
                using (var glow = new SolidBrush(Color.FromArgb(60, 140, 120, 220)))
                    g.FillEllipse(glow, cx - dr * 0.8f, body.Top + body.Height * 0.42f - dr * 0.8f,
                        dr * 1.6f, dr * 1.6f);
        }
        // ----------------------------------------------------------------
        // CMP：化学机械抛光（抛光盘 + 摆臂头 + 浆料）
        // ----------------------------------------------------------------
        private void DrawCMP(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Bottom - a.Height * 0.28f;
            float pr = a.Width * 0.32f;
            // 抛光盘（旋转）
            g.Ell(cx, cy, pr, pr * 0.32f, HToolPalettes.Shade(pal.Main, 0.15f));
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - pr, cy - pr * 0.32f, pr * 2f, pr * 0.64f);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 0f);
            using (var txt = new Pen(Color.FromArgb(120, pal.Accent), 1f))
                for (int i = 0; i < 4; i++)
                {
                    g.RotateTransform(90f);
                    g.DrawLine(txt, pr * 0.2f, 0, pr * 0.8f, 0);
                }
            g.Restore(st);
            // 摆臂 + 抛光头（带晶圆）
            float pivotX = a.Right - a.Width * 0.14f, pivotY = a.Top + a.Height * 0.2f;
            g.Ell(pivotX, pivotY, 4f, 4f, pal.MetalDark);
            float ang = Running ? -0.5f + (float)Math.Sin(Phase) * 0.25f : -0.4f;
            float hx = pivotX + (float)Math.Cos(ang) * a.Width * 0.34f;
            float hy = pivotY - (float)Math.Sin(ang) * a.Width * 0.34f;
            using (var arm = new Pen(pal.MetalDark, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(arm, pivotX, pivotY, hx, hy);
            g.Ell(hx, hy, 9f, 9f, HToolPalettes.Shade(pal.Main, 0.1f));
            g.Ell(hx, hy, 6f, 6f, Color.FromArgb(170, 178, 224));
            // 浆料滴
            if (Running)
                using (var sl = new SolidBrush(Color.FromArgb(180, 200, 220)))
                    for (int i = 0; i < 4; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.25f) % 1f;
                        g.FillEllipse(sl, pivotX - 14f + t * 8f, pivotY + 6f + t * 22f, 3f, 4f);
                    }
        }
        // ----------------------------------------------------------------
        // Scrubber：晶圆刷洗机（双辊 + 晶圆通过 + 冲洗）
        // ----------------------------------------------------------------
        private void DrawScrubber(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.46f;
            // 上下滚刷
            foreach (float dy in new[] { -a.Height * 0.12f, a.Height * 0.12f })
            {
                var roller = new RectangleF(a.Left + a.Width * 0.2f, cy + dy - 8f,
                    a.Width * 0.6f, 16f);
                g.FillCylH(roller, HToolPalettes.Shade(pal.Main, 0.25f), HToolPalettes.Tint(pal.Main, 0.25f));
                g.DrawRectangle(new Pen(pal.Edge, 1.1f), roller.X, roller.Y, roller.Width, roller.Height);
            }
            // 通过的晶圆（水平移动）
            float move = Running ? Phase / (float)Math.PI * 0.5f * a.Width * 0.2f : 0f;
            float wx = a.Left + a.Width * 0.34f + ((move % (a.Width * 0.3f) + a.Width * 0.3f) % (a.Width * 0.3f)) - a.Width * 0.05f;
            g.Box(new RectangleF(wx, cy - 5f, a.Width * 0.24f, 10f), 2f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
            // 两侧喷淋
            foreach (float sx in new[] { a.Left + a.Width * 0.16f, a.Right - a.Width * 0.16f })
            {
                g.Line(pal.Metal, 3f, sx, cy - a.Height * 0.2f, sx, cy + a.Height * 0.2f);
                if (Running)
                    using (var sp = new Pen(Color.FromArgb(160, 214, 236, 250), 1.4f))
                        g.DrawLine(sp, sx, cy - 8f, sx + (sx < a.Left + a.Width / 2f ? 8f : -8f), cy + 8f);
            }
        }
        // ----------------------------------------------------------------
        // QuartzTube：卧式石英管 + 晶圆舟
        // ----------------------------------------------------------------
        private void DrawQuartzTube(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f;
            float x1 = a.Left + a.Width * 0.12f, x2 = a.Right - a.Width * 0.12f;
            float r = a.Height * 0.22f;
            // 石英管（半透明 + 椭圆封端）
            using (var tube = new SolidBrush(Color.FromArgb(60, 210, 220, 230)))
                g.FillRectangle(tube, x1, cy - r, x2 - x1, r * 2f);
            g.DrawRectangle(new Pen(Color.FromArgb(150, 142, 128), 1.3f), x1, cy - r, x2 - x1, r * 2f);
            g.DrawEllipse(new Pen(Color.FromArgb(150, 142, 128), 1.3f), x2 - 2f, cy - r, 12f, r * 2f);
            // 管内晶圆（侧视）
            for (int i = 0; i < 7; i++)
            {
                float x = x1 + 14f + i * (x2 - x1 - 28f) / 6f;
                using (var wb = new SolidBrush(Color.FromArgb(150, 170, 200, 230)))
                {
                    g.FillRectangle(wb, x - 2f, cy - r * 0.7f, 4f, r * 1.4f);
                    g.FillEllipse(wb, x - 2f, cy - r * 0.82f, 4f, r * 0.26f);
                }
            }
            // 气管 + 支架
            g.Line(pal.MetalDark, 2.6f, x1, cy, a.Left + 6f, cy - 12f);
            g.Line(pal.MetalDark, 3f, x1 + 14f, cy + r, x1 + 14f, a.Bottom - 8f);
            g.Line(pal.MetalDark, 3f, x2 - 14f, cy + r, x2 - 14f, a.Bottom - 8f);
            // 加热辉光
            if (Running)
                using (var glow = new Pen(Color.FromArgb(120, 255, 140, 60), 1.4f)
                { DashStyle = DashStyle.Dash })
                    g.DrawRectangle(glow, x1 + 4f, cy - r + 3f, x2 - x1 - 8f, r * 2f - 6f);
        }
        // ----------------------------------------------------------------
        // Robotic：机械手自动清洗线
        // ----------------------------------------------------------------
        private void DrawRobotic(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 一排槽位
            int n = 3;
            float tw = a.Width * 0.22f, gap = a.Width * 0.03f;
            float total = n * tw + (n - 1) * gap;
            float x0 = a.Left + (a.Width - total) / 2f;
            for (int i = 0; i < n; i++)
            {
                var tank = new RectangleF(x0 + i * (tw + gap), a.Top + a.Height * 0.5f,
                    tw, a.Height * 0.32f);
                g.Box(tank, 2f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.2f);
                Liquid(g, tank, 0.55f, Color.FromArgb(120, 180, 220));
            }
            // 顶部导轨 + 横移机械手
            g.Line(pal.MetalDark, 4f, a.Left + a.Width * 0.08f, a.Top + a.Height * 0.16f,
                a.Right - a.Width * 0.08f, a.Top + a.Height * 0.16f);
            float move = Running ? Phase / (float)Math.PI * 0.5f : 0.2f;
            float rx = a.Left + a.Width * (0.16f + 0.68f * (move % 1f));
            float ry = a.Top + a.Height * 0.16f;
            g.Box(new RectangleF(rx - 8f, ry - 5f, 16f, 10f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            float armDrop = a.Height * (0.14f + 0.1f * (float)Math.Abs(Math.Sin(Phase)));
            g.Line(pal.MetalDark, 3f, rx, ry + 5f, rx, ry + armDrop);
            // 夹爪上的晶圆
            g.Box(new RectangleF(rx - 5f, ry + armDrop, 10f, a.Height * 0.14f), 1f,
                Color.FromArgb(170, 178, 224), Color.FromArgb(90, 80, 130), 1f);
        }
    }
}