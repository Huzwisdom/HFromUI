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
    /// 硅料外形：0 单晶硅棒、1 硅圆片为经典外形，其余 20 种覆盖多晶硅铸锭/碎料、
    /// 坩埚熔硅、直拉/区熔炉、线切割、切方/磨圆、晶圆舟/传送盒、外延/SOI/注入片、
    /// 碎片、硅粉、太阳能片/串与探针测试等。
    /// </summary>
    public enum HSiliconStyle
    {
        Crystal = 0,      // 单晶硅棒
        Wafer = 1,        // 硅圆片
        PolyIngot = 2,    // 多晶硅铸锭
        PolyChunk = 3,    // 多晶硅碎料
        Crucible = 4,     // 石英坩埚熔硅
        CZPull = 5,       // 直拉单晶炉
        FZ = 6,           // 区熔单晶
        WaferBoat = 7,    // 石英晶圆舟
        WaferCassette = 8,// 晶圆传送盒
        WaferNotch = 9,   // 立式缺口圆片
        WireSaw = 10,     // 多线切割机
        PolishPad = 11,   // 吸盘抛光片
        Epi = 12,         // 外延片
        SOI = 13,         // SOI 夹层片
        Implant = 14,     // 离子注入片
        Broken = 15,      // 碎片
        Powder = 16,      // 硅粉碎料
        SolarCell = 17,   // 太阳能电池片
        SolarString = 18, // 太阳能电池串
        SquaredIngot = 19,// 切方单晶棒
        GroundIngot = 20, // 磨圆角方棒
        ProbeCard = 21    // 探针测试
    }
    /// <summary>
    /// 硅晶体/硅圆片控件（继承 HToolAnimBase）：22 种硅料与晶圆工艺外形、13 种色调；静态图元。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("硅料控件：22 种硅棒/圆片/工艺设备外形、13 种色调")]
    public class HSilicon : HToolAnimBase
    {
        private HSiliconStyle _style = HSiliconStyle.Crystal;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HSilicon()
        {
            Size = new Size(110, 140);
        }
        /// <summary>硅料外形。</summary>
        [HCategoryLanguage("硅片"), HDisplayNameLanguage("硅料外形样式"), HDescriptionLanguage("硅料外形样式"), Browsable(true)]
        [DefaultValue(HSiliconStyle.Crystal)]
        public HSiliconStyle SiliconStyle
        {
            get => _style;
            set
            {
                _style = value;
                Size = value == HSiliconStyle.Wafer ? new Size(130, 96) : new Size(110, 140);
                Invalidate();
            }
        }
        private HToolPalette Pal => Palette;
        // 单晶硅金属银紫配色
        private static readonly Color SiLight = Color.FromArgb(206, 210, 226);
        private static readonly Color SiMid = Color.FromArgb(158, 164, 188);
        private static readonly Color SiDark = Color.FromArgb(96, 102, 128);
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            var pal = Pal;
            switch (_style)
            {
                case HSiliconStyle.Wafer: DrawWafer(g, a); break;
                case HSiliconStyle.PolyIngot: DrawPolyIngot(g, a); break;
                case HSiliconStyle.PolyChunk: DrawPolyChunk(g, a); break;
                case HSiliconStyle.Crucible: DrawCrucible(g, a); break;
                case HSiliconStyle.CZPull: DrawCZ(g, a, pal); break;
                case HSiliconStyle.FZ: DrawFZ(g, a, pal); break;
                case HSiliconStyle.WaferBoat: DrawBoat(g, a); break;
                case HSiliconStyle.WaferCassette: DrawCassette(g, a, pal); break;
                case HSiliconStyle.WaferNotch: DrawNotch(g, a); break;
                case HSiliconStyle.WireSaw: DrawWireSaw(g, a, pal); break;
                case HSiliconStyle.PolishPad: DrawPolish(g, a); break;
                case HSiliconStyle.Epi: DrawEpi(g, a); break;
                case HSiliconStyle.SOI: DrawSOI(g, a); break;
                case HSiliconStyle.Implant: DrawImplant(g, a); break;
                case HSiliconStyle.Broken: DrawBroken(g, a); break;
                case HSiliconStyle.Powder: DrawPowder(g, a); break;
                case HSiliconStyle.SolarCell: DrawSolarCell(g, a); break;
                case HSiliconStyle.SolarString: DrawSolarString(g, a); break;
                case HSiliconStyle.SquaredIngot: DrawSquared(g, a, false); break;
                case HSiliconStyle.GroundIngot: DrawSquared(g, a, true); break;
                case HSiliconStyle.ProbeCard: DrawProbe(g, a, pal); break;
                default: DrawCrystal(g, a); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>镀膜硅圆盘（带定位边）。</summary>
        private static void WaferDisc(Graphics g, float cx, float cy, float rx, float ry)
        {
            using (var disc = new GraphicsPath())
            {
                disc.AddEllipse(cx - rx, cy - ry, rx * 2f, ry * 2f);
                disc.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(cx - rx, cy - ry, rx * 2f, ry * 2f),
                    Color.FromArgb(190, 178, 224), Color.FromArgb(120, 140, 190), 35f))
                    g.FillPath(b, disc);
            }
            using (var ring = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                for (int i = 1; i <= 3; i++)
                    g.DrawEllipse(ring, cx - rx * i / 4f, cy - ry * i / 4f, rx * i / 2f, ry * i / 2f);
            g.DrawArc(new Pen(Color.FromArgb(90, 80, 130), 1.5f),
                cx - rx, cy - ry, rx * 2f, ry * 2f, 180f, 360f);
            using (var hl = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                g.FillEllipse(hl, cx - rx * 0.6f, cy - ry * 0.7f, rx * 0.7f, ry * 0.5f);
        }
        // ----------------------------------------------------------------
        // Classic：带籽晶颈的单晶硅棒
        // ----------------------------------------------------------------
        private void DrawCrystal(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f;
            float r = Math.Min(a.Width * 0.26f, 34f);
            float top = a.Top + a.Height * 0.14f, neckB = a.Top + a.Height * 0.22f;
            float bot = a.Bottom - a.Height * 0.1f;
            using (var nb = new LinearGradientBrush(new RectangleF(cx - r * 0.22f, a.Top + 2f, r * 0.44f, neckB - a.Top), SiDark, SiMid, 0f))
                g.FillRectangle(nb, cx - r * 0.22f, a.Top + 2f, r * 0.44f, neckB - a.Top - 2f);
            var body = new RectangleF(cx - r, top, r * 2f, bot - top);
            using (var b = new LinearGradientBrush(body, SiDark, Color.White, 0f))
            {
                b.InterpolationColors = new ColorBlend
                {
                    Positions = new[] { 0f, 0.25f, 0.55f, 0.8f, 1f },
                    Colors = new[] { SiDark, SiLight, SiMid, SiLight, SiDark }
                };
                g.FillRectangle(b, body);
            }
            using (var facet = new Pen(Color.FromArgb(60, 255, 255, 255), 1.2f))
            {
                g.DrawLine(facet, cx - r * 0.55f, top + 4, cx - r * 0.55f, bot - 4);
                g.DrawLine(facet, cx + r * 0.5f, top + 4, cx + r * 0.5f, bot - 4);
            }
            g.DrawRectangle(new Pen(SiDark, 1.3f), body.X, body.Y, body.Width, body.Height);
            using (var topB = new SolidBrush(SiLight))
                g.FillEllipse(topB, cx - r, top - r * 0.28f, r * 2f, r * 0.56f);
            g.DrawEllipse(new Pen(SiDark, 1.2f), cx - r, top - r * 0.28f, r * 2f, r * 0.56f);
            using (var bb = new SolidBrush(SiDark))
                g.FillEllipse(bb, cx - r, bot - r * 0.2f, r * 2f, r * 0.4f);
        }
        // ----------------------------------------------------------------
        // Wafer：带定位边与镀膜虹彩的硅圆片
        // ----------------------------------------------------------------
        private void DrawWafer(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.46f;
            float rx = Math.Min(a.Width * 0.42f, 56f), ry = rx * 0.42f;
            WaferDisc(g, cx, cy, rx, ry);
            float flat = rx * 0.34f, fy = cy + ry * 0.72f;
            g.DrawLine(new Pen(Color.FromArgb(90, 80, 130), 1.6f), cx - flat, fy, cx + flat, fy);
            using (var side = new Pen(Color.FromArgb(70, 74, 100), 2f))
                g.DrawArc(side, cx - rx, cy - ry + ry * 0.35f, rx * 2f, ry * 2f, 20f, 140f);
        }
        // ----------------------------------------------------------------
        // PolyIngot：多晶硅方形铸锭（顶面晶粒）
        // ----------------------------------------------------------------
        private void DrawPolyIngot(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f;
            float w = Math.Min(a.Width * 0.56f, 60f);
            float top = a.Top + a.Height * 0.18f, bot = a.Bottom - a.Height * 0.12f;
            var body = new RectangleF(cx - w / 2f, top, w, bot - top);
            using (var b = new LinearGradientBrush(body, Color.FromArgb(150, 156, 172),
                Color.FromArgb(96, 102, 120), 0f))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(SiDark, 1.4f), body.X, body.Y, body.Width, body.Height);
            // 顶面碎晶拼纹
            var topFace = new[]
            {
                new PointF(cx - w / 2f, top), new PointF(cx - w * 0.18f, top - 12f),
                new PointF(cx + w / 2f, top - 12f), new PointF(cx + w / 2f, top)
            };
            g.Poly(topFace, Color.FromArgb(190, 196, 212), SiDark, 1.3f);
            using (var cr = new Pen(Color.FromArgb(120, 126, 146), 1f))
            {
                g.DrawLine(cr, cx - w * 0.1f, top - 10f, cx + w * 0.1f, top);
                g.DrawLine(cr, cx + w * 0.2f, top - 11f, cx + w * 0.05f, top);
            }
            // 正面晶界
            using (var cr = new Pen(Color.FromArgb(70, 255, 255, 255), 1.2f))
            {
                g.DrawLine(cr, cx - w * 0.2f, top + 10f, cx - w * 0.05f, bot - 8f);
                g.DrawLine(cr, cx + w * 0.15f, top + 8f, cx + w * 0.3f, bot - 12f);
            }
        }
        // ----------------------------------------------------------------
        // PolyChunk：多晶硅碎料堆
        // ----------------------------------------------------------------
        private void DrawPolyChunk(Graphics g, RectangleF a)
        {
            var chunks = new[]
            {
                new[] { 0.28f, 0.72f, 0.16f, 0.14f },
                new[] { 0.5f, 0.66f, 0.2f, 0.18f },
                new[] { 0.7f, 0.74f, 0.14f, 0.12f },
                new[] { 0.38f, 0.52f, 0.13f, 0.12f },
                new[] { 0.6f, 0.48f, 0.12f, 0.11f }
            };
            foreach (var c in chunks)
            {
                float cx = a.Left + a.Width * c[0], cy = a.Top + a.Height * c[1];
                float w = a.Width * c[2], h = a.Height * c[3];
                var pts = new[]
                {
                    new PointF(cx - w / 2f, cy), new PointF(cx - w * 0.3f, cy - h / 2f),
                    new PointF(cx + w * 0.2f, cy - h * 0.55f), new PointF(cx + w / 2f, cy + h * 0.1f),
                    new PointF(cx + w * 0.1f, cy + h / 2f), new PointF(cx - w * 0.4f, cy + h * 0.4f)
                };
                g.Poly(pts, Color.FromArgb(168 + (int)(c[0] * 30) % 40, 174, 190), SiDark, 1.1f);
            }
        }
        // ----------------------------------------------------------------
        // Crucible：石英坩埚 + 炽热熔硅
        // ----------------------------------------------------------------
        private void DrawCrucible(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.34f, bot = a.Bottom - a.Height * 0.12f;
            float rw = a.Width * 0.32f;
            // 坩埚（剖面碗）
            var pts = new[]
            {
                new PointF(cx - rw, top), new PointF(cx + rw, top),
                new PointF(cx + rw * 0.62f, bot), new PointF(cx - rw * 0.62f, bot)
            };
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(pts);
                using (var b = new LinearGradientBrush(new RectangleF(cx - rw, top, rw * 2f, bot - top),
                    Color.FromArgb(232, 226, 214), Color.FromArgb(176, 168, 154), 0f))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(Color.FromArgb(120, 110, 96), 1.4f), path);
            }
            // 熔硅液面（橙红发光）
            using (var glow = new SolidBrush(Color.FromArgb(70, 255, 120, 40)))
                g.FillEllipse(glow, cx - rw * 0.9f, top - 8f, rw * 1.8f, 16f);
            g.Ell(cx, top, rw * 0.84f, rw * 0.2f, Color.FromArgb(255, 168, 72));
            g.DrawEllipse(new Pen(Color.FromArgb(200, 110, 40), 1.2f),
                cx - rw * 0.84f, top - rw * 0.2f, rw * 1.68f, rw * 0.4f);
        }
        // ----------------------------------------------------------------
        // CZPull：直拉单晶炉（炉室 + 上升晶棒）
        // ----------------------------------------------------------------
        private void DrawCZ(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.52f;
            float top = a.Top + a.Height * 0.3f, bot = a.Bottom - a.Height * 0.08f;
            // 炉体
            var body = new RectangleF(cx - w / 2f, top, w, bot - top);
            using (var gp = new GraphicsPath())
            {
                gp.AddArc(cx - w / 2f, top - w * 0.18f, w, w * 0.36f, 180f, 180f);
                gp.AddRectangle(body);
                gp.CloseFigure();
                using (var b = new LinearGradientBrush(body, HToolPalettes.Tint(pal.Main, 0.28f),
                    HToolPalettes.Shade(pal.Main, 0.2f), 0f))
                    g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.5f), gp);
            }
            // 观察窗（红光熔池）
            g.Ell(cx, top + (bot - top) * 0.42f, w * 0.22f, w * 0.14f, Color.FromArgb(255, 150, 60));
            g.DrawEllipse(new Pen(pal.Edge, 1.2f),
                cx - w * 0.22f, top + (bot - top) * 0.42f - w * 0.14f, w * 0.44f, w * 0.28f);
            // 上升的晶棒 + 籽晶绳
            float ir = w * 0.12f;
            g.Line(Color.FromArgb(56, 58, 64), 1.6f, cx, a.Top + 4f, cx, a.Top + a.Height * 0.16f);
            var ingot = new RectangleF(cx - ir, a.Top + a.Height * 0.16f, ir * 2f, top - a.Top - a.Height * 0.16f);
            using (var b = new LinearGradientBrush(ingot, SiDark, Color.White, 0f))
                g.FillRectangle(b, ingot);
            g.DrawRectangle(new Pen(SiDark, 1.1f), ingot.X, ingot.Y, ingot.Width, ingot.Height);
            // 支腿
            foreach (float lx in new[] { cx - w * 0.32f, cx + w * 0.32f })
                g.Line(pal.MetalDark, 3f, lx, bot, lx, bot + a.Height * 0.08f);
        }
        // ----------------------------------------------------------------
        // FZ：区熔单晶（射频线圈 + 上下晶棒）
        // ----------------------------------------------------------------
        private void DrawFZ(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float r = a.Width * 0.1f;
            float coilY = a.Top + a.Height * 0.52f;
            // 上下晶棒
            foreach (var bar in new[]
            {
                new RectangleF(cx - r, a.Top + a.Height * 0.08f, r * 2f, coilY - a.Top - a.Height * 0.08f - 6f),
                new RectangleF(cx - r, coilY + 8f, r * 2f, a.Bottom - a.Height * 0.1f - coilY - 8f)
            })
            {
                using (var b = new LinearGradientBrush(bar, SiDark, Color.White, 0f))
                    g.FillRectangle(b, bar);
                g.DrawRectangle(new Pen(SiDark, 1f), bar.X, bar.Y, bar.Width, bar.Height);
            }
            // 熔融区（发光球）
            using (var halo = new SolidBrush(Color.FromArgb(60, 255, 120, 40)))
                g.FillEllipse(halo, cx - r * 2f, coilY - r * 2f, r * 4f, r * 4f);
            g.Ell(cx, coilY, r * 0.8f, r * 0.8f, Color.FromArgb(255, 170, 74));
            // 射频线圈（两侧圆盘）
            g.Ell(cx - r * 1.9f, coilY, r * 0.55f, r * 0.55f, pal.Metal);
            g.Ell(cx + r * 1.9f, coilY, r * 0.55f, r * 0.55f, pal.Metal);
            g.Line(pal.MetalDark, 2f, cx - r * 1.4f, coilY, cx - r * 0.8f, coilY);
            g.Line(pal.MetalDark, 2f, cx + r * 0.8f, coilY, cx + r * 1.4f, coilY);
            // 支架
            g.Line(pal.MetalDark, 2.6f, cx - r * 2.4f, coilY, cx - r * 2.4f, a.Bottom - 6f);
            g.Line(pal.MetalDark, 2.6f, cx + r * 2.4f, coilY, cx + r * 2.4f, a.Bottom - 6f);
        }
        // ----------------------------------------------------------------
        // WaferBoat：石英晶圆舟（立插圆片）
        // ----------------------------------------------------------------
        private void DrawBoat(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f;
            float baseY = a.Bottom - a.Height * 0.16f;
            // 石英槽梁
            g.Box(new RectangleF(a.Left + a.Width * 0.14f, baseY, a.Width * 0.72f, 7f), 2f,
                Color.FromArgb(226, 222, 212), Color.FromArgb(150, 142, 128), 1.1f);
            // 立插圆片（侧视细线 + 顶部弧）
            int n = 7;
            for (int i = 0; i < n; i++)
            {
                float t = i / (n - 1f);
                float x = a.Left + a.Width * (0.22f + 0.56f * t);
                float h = a.Height * (0.34f + 0.1f * (float)Math.Sin(t * Math.PI));
                float top = baseY - h;
                using (var wb = new SolidBrush(Color.FromArgb(170, 168, 206, 230)))
                {
                    g.FillRectangle(wb, x - 2.4f, top + h * 0.16f, 4.8f, h * 0.84f);
                    g.FillEllipse(wb, x - 2.4f, top, 4.8f, h * 0.32f);
                }
                g.DrawArc(new Pen(Color.FromArgb(120, 130, 170), 0.9f),
                    x - 2.4f, top, 4.8f, h * 0.32f, 180f, 180f);
            }
        }
        // ----------------------------------------------------------------
        // WaferCassette：晶圆传送盒
        // ----------------------------------------------------------------
        private void DrawCassette(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.62f, h = a.Height * 0.62f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + a.Height * 0.16f, w, h);
            g.Box(box, 4f, HToolPalettes.Tint(pal.Main, 0.24f), pal.Edge, 1.4f);
            // 开口观察窗
            var win = new RectangleF(box.X + 8f, box.Y + 10f, box.Width - 16f, box.Height - 22f);
            using (var wb = new SolidBrush(Color.FromArgb(70, 90, 110)))
                g.FillRectangle(wb, win);
            // 槽内圆片（侧视弧顶）
            for (int i = 0; i < 6; i++)
            {
                float x = win.X + 6f + i * (win.Width - 12f) / 5f;
                using (var wf = new SolidBrush(Color.FromArgb(150, 170, 200, 230)))
                {
                    g.FillRectangle(wf, x - 2f, win.Y + 12f, 4f, win.Height - 16f);
                    g.FillEllipse(wf, x - 2f, win.Y + 8f, 4f, 9f);
                }
            }
            // 提手
            g.Line(pal.MetalDark, 2.6f, box.X + w * 0.3f, box.Y - 5f, box.X + w * 0.7f, box.Y - 5f);
        }
        // ----------------------------------------------------------------
        // WaferNotch：立式带缺口圆片（侧视）
        // ----------------------------------------------------------------
        private void DrawNotch(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f;
            float r = Math.Min(a.Width * 0.34f, a.Height * 0.36f);
            float cy = a.Top + a.Height * 0.5f;
            using (var disk = new GraphicsPath())
            {
                disk.AddEllipse(cx - r, cy - r, r * 2f, r * 2f);
                disk.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(cx - r, cy - r, r * 2f, r * 2f),
                    Color.FromArgb(196, 184, 228), Color.FromArgb(120, 138, 188), 40f))
                    g.FillPath(b, disk);
                g.DrawPath(new Pen(Color.FromArgb(90, 80, 130), 1.4f), disk);
            }
            // 顶部定位缺口（V 形凹口）
            using (var cut = new Pen(Color.FromArgb(90, 80, 130), 1.2f))
            {
                g.DrawLine(cut, cx - 6f, cy - r + 1f, cx, cy - r - 5f);
                g.DrawLine(cut, cx + 6f, cy - r + 1f, cx, cy - r - 5f);
            }
            // 膜面环
            using (var ring = new Pen(Color.FromArgb(70, 255, 255, 255), 1f))
                for (int i = 1; i <= 3; i++)
                    g.DrawEllipse(ring, cx - r * i / 4f, cy - r * i / 4f, r * i / 2f, r * i / 2f);
        }
        // ----------------------------------------------------------------
        // WireSaw：多线切割机（晶棒 + 钢线网 + 双导轮）
        // ----------------------------------------------------------------
        private void DrawWireSaw(Graphics g, RectangleF a, HToolPalette pal)
        {
            float y1 = a.Top + a.Height * 0.3f, y2 = a.Top + a.Height * 0.7f;
            float x1 = a.Left + a.Width * 0.2f, x2 = a.Right - a.Width * 0.2f;
            // 导轮
            foreach (float wx in new[] { x1, x2 })
            {
                g.Ell(wx, y1, 12f, 12f, pal.Metal);
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), wx - 12f, y1 - 12f, 24f, 24f);
                g.Ell(wx, y1, 3f, 3f, pal.Accent);
                g.Ell(wx, y2, 12f, 12f, pal.Metal);
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), wx - 12f, y2 - 12f, 24f, 24f);
                g.Ell(wx, y2, 3f, 3f, pal.Accent);
            }
            // 排线网
            using (var wire = new Pen(Color.FromArgb(90, 96, 106), 1f))
                for (int i = 0; i < 10; i++)
                {
                    float ox = -6f + i * 1.3f;
                    g.DrawLine(wire, x1 + ox, y1, x2 + ox, y1);
                    g.DrawLine(wire, x1 + ox, y2, x2 + ox, y2);
                }
            // 待切晶棒（横置）
            float bx = a.Left + a.Width / 2f, by = a.Top + a.Height * 0.5f;
            var ingot = new RectangleF(bx - a.Width * 0.26f, by - 9f, a.Width * 0.52f, 18f);
            g.FillCylH(ingot, SiDark, Color.White);
            g.DrawRectangle(new Pen(SiDark, 1.2f), ingot.X, ingot.Y, ingot.Width, ingot.Height);
            // 切削液
            using (var drop = new SolidBrush(Color.FromArgb(160, 200, 224)))
                for (int i = 0; i < 4; i++)
                    g.FillEllipse(drop, bx - 20f + i * 13f, by + 11f, 3f, 5f);
        }
        // ----------------------------------------------------------------
        // PolishPad：真空吸盘上的镜面抛光片
        // ----------------------------------------------------------------
        private void DrawPolish(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.56f;
            float rx = a.Width * 0.34f, ry = rx * 0.3f;
            // 吸盘
            g.Ell(cx, cy, rx + 8f, ry + 8f, Color.FromArgb(86, 92, 104));
            g.DrawEllipse(new Pen(Color.FromArgb(50, 54, 62), 1.3f),
                cx - rx - 8f, cy - ry - 8f, (rx + 8f) * 2f, (ry + 8f) * 2f);
            // 镜面片
            WaferDisc(g, cx, cy - 3f, rx, ry);
            // 抛光盘微孔环
            using (var hole = new Pen(Color.FromArgb(120, 130, 150), 0.8f))
                g.DrawEllipse(hole, cx - rx - 5f, cy - ry - 5f, (rx + 5f) * 2f, (ry + 5f) * 2f);
        }
        // ----------------------------------------------------------------
        // Epi：外延片（膜层虹彩 + 生长气流）
        // ----------------------------------------------------------------
        private void DrawEpi(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.54f;
            float rx = a.Width * 0.34f, ry = rx * 0.32f;
            WaferDisc(g, cx, cy, rx, ry);
            // 外延膜虹彩层
            using (var epi = new Pen(Color.FromArgb(140, 120, 220, 180), 2f))
                g.DrawEllipse(epi, cx - rx * 0.62f, cy - ry * 0.62f, rx * 1.24f, ry * 1.24f);
            // 上方气流线
            using (var flow = new Pen(Color.FromArgb(120, 140, 200), 1.3f))
                for (int i = 0; i < 3; i++)
                {
                    float y = cy - ry - 16f - i * 7f;
                    g.DrawLine(flow, cx - rx * 0.8f, y, cx + rx * 0.8f, y);
                    g.DrawLine(flow, cx + rx * 0.8f, y, cx + rx * 0.66f, y - 3f);
                    g.DrawLine(flow, cx + rx * 0.8f, y, cx + rx * 0.66f, y + 3f);
                }
        }
        // ----------------------------------------------------------------
        // SOI：三层夹层剖面圆片
        // ----------------------------------------------------------------
        private void DrawSOI(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.52f;
            float rx = a.Width * 0.34f, ry = rx * 0.32f;
            WaferDisc(g, cx, cy, rx, ry);
            // 剖面三层线（顶层硅 + 氧化层）
            using (var oxide = new Pen(Color.FromArgb(200, 120, 200, 140), 2.4f))
                g.DrawArc(oxide, cx - rx * 0.7f, cy - ry * 0.7f, rx * 1.4f, ry * 1.4f, 200f, 140f);
            using (var topSi = new Pen(Color.FromArgb(180, 110, 130, 180), 1.4f))
                g.DrawArc(topSi, cx - rx * 0.52f, cy - ry * 0.52f, rx * 1.04f, ry * 1.04f, 200f, 140f);
        }
        // ----------------------------------------------------------------
        // Implant：离子注入（圆片 + 注入束）
        // ----------------------------------------------------------------
        private void DrawImplant(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.58f;
            float rx = a.Width * 0.32f, ry = rx * 0.3f;
            WaferDisc(g, cx, cy, rx, ry);
            // 注入束（扇形虚线）
            using (var beam = new Pen(Color.FromArgb(170, 110, 200, 255), 1.6f) { DashStyle = DashStyle.Dash })
                for (int i = -2; i <= 2; i++)
                {
                    float ex = cx + i * rx * 0.28f;
                    g.DrawLine(beam, cx + i * 3f, a.Top + a.Height * 0.08f, ex, cy - ry * 0.7f);
                }
            // 注入枪
            g.Box(new RectangleF(cx - 12f, a.Top + 6f, 24f, 10f), 2f,
                Color.FromArgb(96, 102, 116), Color.FromArgb(60, 64, 72), 1.1f);
        }
        // ----------------------------------------------------------------
        // Broken：带裂纹的碎圆片
        // ----------------------------------------------------------------
        private void DrawBroken(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.5f;
            float rx = a.Width * 0.34f, ry = rx * 0.32f;
            // 缺角盘（多边形近似）
            var pts = new PointF[14];
            for (int i = 0; i < pts.Length; i++)
            {
                double ang = i * Math.PI * 2.0 / pts.Length;
                float rr = (i >= 9 && i <= 11) ? 0.62f : 1f;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * rx * rr,
                    cy + (float)Math.Sin(ang) * ry * rr);
            }
            g.Poly(pts, Color.FromArgb(168, 160, 206), Color.FromArgb(90, 80, 130), 1.3f);
            // 裂纹
            using (var crack = new Pen(Color.FromArgb(80, 70, 120), 1.2f))
            {
                g.DrawLine(crack, cx - rx * 0.5f, cy - ry * 0.4f, cx, cy);
                g.DrawLine(crack, cx, cy, cx + rx * 0.3f, cy + ry * 0.5f);
                g.DrawLine(crack, cx, cy, cx - rx * 0.2f, cy + ry * 0.6f);
            }
            // 脱落小碎片
            g.Poly(new[]
            {
                new PointF(cx + rx * 0.86f, cy + ry * 0.5f),
                new PointF(cx + rx * 1.14f, cy + ry * 0.72f),
                new PointF(cx + rx * 0.92f, cy + ry * 0.95f)
            }, Color.FromArgb(168, 160, 206), Color.FromArgb(90, 80, 130), 1f);
        }
        // ----------------------------------------------------------------
        // Powder：硅粉碎料（料堆 + 颗粒）
        // ----------------------------------------------------------------
        private void DrawPowder(Graphics g, RectangleF a)
        {
            // 料袋
            float bx = a.Left + a.Width * 0.16f, bw = a.Width * 0.42f;
            float by = a.Top + a.Height * 0.5f, bh = a.Height * 0.34f;
            g.Poly(new[]
            {
                new PointF(bx, by), new PointF(bx + bw, by),
                new PointF(bx + bw * 0.9f, by + bh), new PointF(bx + bw * 0.1f, by + bh)
            }, Color.FromArgb(210, 204, 192), Color.FromArgb(140, 132, 118), 1.3f);
            g.Line(Color.FromArgb(140, 132, 118), 2f, bx + bw * 0.5f, by, bx + bw * 0.5f, by - 10f);
            // 倒出的粉堆
            float px = a.Right - a.Width * 0.22f, ground = a.Bottom - a.Height * 0.1f;
            g.Poly(new[]
            {
                new PointF(px - a.Width * 0.2f, ground), new PointF(px, ground - a.Height * 0.16f),
                new PointF(px + a.Width * 0.2f, ground)
            }, Color.FromArgb(172, 178, 192), SiDark, 1.1f);
            using (var dot = new SolidBrush(Color.FromArgb(120, 126, 144)))
                for (int i = 0; i < 12; i++)
                {
                    float dx = px + (i % 5 - 2) * 7f;
                    float dy = ground - 4f - (i * 13) % (int)(a.Height * 0.13f);
                    g.FillEllipse(dot, dx, dy, 2.4f, 2.4f);
                }
        }
        // ----------------------------------------------------------------
        // SolarCell：蓝膜电池片 + 栅线
        // ----------------------------------------------------------------
        private void DrawSolarCell(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.62f;
            var box = new RectangleF(a.Left + (a.Width - s) / 2f, a.Top + (a.Height - s) / 2f, s, s * 0.62f);
            using (var b = new LinearGradientBrush(box, Color.FromArgb(74, 104, 178),
                Color.FromArgb(36, 60, 120), 45f))
                g.FillRectangle(b, box);
            g.DrawRectangle(new Pen(Color.FromArgb(30, 44, 90), 1.3f), box.X, box.Y, box.Width, box.Height);
            // 主栅 + 细栅
            using (var bus = new Pen(Color.FromArgb(225, 220, 200), 1.8f))
                for (int i = 1; i <= 3; i++)
                    g.DrawLine(bus, box.X + box.Width * i / 4f, box.Y + 2f,
                        box.X + box.Width * i / 4f, box.Bottom - 2f);
            using (var finger = new Pen(Color.FromArgb(180, 210, 225), 0.7f))
                for (int i = 1; i < 10; i++)
                    g.DrawLine(finger, box.X + 2f, box.Y + box.Height * i / 10f,
                        box.Right - 2f, box.Y + box.Height * i / 10f);
        }
        // ----------------------------------------------------------------
        // SolarString：焊带串联的电池串
        // ----------------------------------------------------------------
        private void DrawSolarString(Graphics g, RectangleF a)
        {
            int n = 4;
            float cw = a.Width * 0.16f, gap = a.Width * 0.05f;
            float total = n * cw + (n - 1) * gap;
            float x0 = a.Left + (a.Width - total) / 2f;
            float cy = a.Top + a.Height * 0.52f;
            for (int i = 0; i < n; i++)
            {
                float x = x0 + i * (cw + gap);
                var box = new RectangleF(x, cy - cw * 0.5f, cw, cw);
                using (var b = new LinearGradientBrush(box, Color.FromArgb(74, 104, 178),
                    Color.FromArgb(36, 60, 120), 45f))
                    g.FillRectangle(b, box);
                g.DrawRectangle(new Pen(Color.FromArgb(30, 44, 90), 1f), box.X, box.Y, box.Width, box.Height);
                using (var bus = new Pen(Color.FromArgb(225, 220, 200), 1.2f))
                {
                    g.DrawLine(bus, x + cw * 0.3f, box.Y + 2f, x + cw * 0.3f, box.Bottom - 2f);
                    g.DrawLine(bus, x + cw * 0.7f, box.Y + 2f, x + cw * 0.7f, box.Bottom - 2f);
                }
                // 互联焊带
                if (i < n - 1)
                    g.Line(Color.FromArgb(214, 190, 120), 2.2f, x + cw, cy, x + cw + gap, cy);
            }
        }
        // ----------------------------------------------------------------
        // SquaredIngot / GroundIngot：切方棒（ground 为磨圆角八角截面）
        // ----------------------------------------------------------------
        private void DrawSquared(Graphics g, RectangleF a, bool ground)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.3f;
            float top = a.Top + a.Height * 0.1f, bot = a.Bottom - a.Height * 0.12f;
            PointF[] face;
            if (ground)
            {
                float c = w * 0.28f;
                face = new[]
                {
                    new PointF(cx - w + c, top), new PointF(cx + w - c, top),
                    new PointF(cx + w, top + c), new PointF(cx + w, bot - c),
                    new PointF(cx + w - c, bot), new PointF(cx - w + c, bot),
                    new PointF(cx - w, bot - c), new PointF(cx - w, top + c)
                };
            }
            else
            {
                face = new[]
                {
                    new PointF(cx - w, top), new PointF(cx + w, top),
                    new PointF(cx + w, bot), new PointF(cx - w, bot)
                };
            }
            g.Poly(face, Color.FromArgb(158, 164, 184), SiDark, 1.4f);
            // 顶面
            PointF[] topFace;
            if (ground)
            {
                float c = w * 0.28f;
                topFace = new[]
                {
                    new PointF(cx - w + c, top), new PointF(cx + w - c, top),
                    new PointF(cx + w - c - 4f, top - 8f), new PointF(cx - w + c + 4f, top - 8f)
                };
            }
            else
                topFace = new[]
                {
                    new PointF(cx - w, top), new PointF(cx + w, top),
                    new PointF(cx + w - 4f, top - 10f), new PointF(cx - w + 4f, top - 10f)
                };
            g.Poly(topFace, SiLight, SiDark, 1.2f);
            // 晶面折线
            using (var facet = new Pen(Color.FromArgb(60, 255, 255, 255), 1.1f))
            {
                g.DrawLine(facet, cx - w * 0.5f, top + 6, cx - w * 0.5f, bot - 6);
                g.DrawLine(facet, cx + w * 0.5f, top + 6, cx + w * 0.5f, bot - 6);
            }
        }
        // ----------------------------------------------------------------
        // ProbeCard：探针卡 + 晶圆测试
        // ----------------------------------------------------------------
        private void DrawProbe(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float cy = a.Top + a.Height * 0.6f, rx = a.Width * 0.3f, ry = rx * 0.3f;
            // 晶圆
            WaferDisc(g, cx, cy, rx, ry);
            // 探针卡圆盘（上部，下压）
            float pcR = a.Width * 0.26f, pcY = a.Top + a.Height * 0.3f;
            g.Ell(cx, pcY, pcR, pcR * 0.4f, HToolPalettes.Shade(pal.Main, 0.12f));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - pcR, pcY - pcR * 0.4f, pcR * 2f, pcR * 0.8f);
            // 探针阵列
            using (var probe = new Pen(Color.FromArgb(200, 204, 212), 1.1f))
                for (int i = -3; i <= 3; i++)
                    g.DrawLine(probe, cx + i * 7f, pcY + pcR * 0.32f,
                        cx + i * 5f, cy - ry * 0.7f);
            // 测试光点
            g.Ell(cx, cy - ry * 0.7f, 2.4f, 2.4f, pal.Accent);
        }
    }
}