using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Nature
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 太阳动画外形：22 种太阳造型——旋转光芒太阳、日出/日落、笑脸太阳、
    /// 光冠、星爆、日环食、太阳能板、热浪、海上日落、云遮日、花瓣太阳、日晷等。
    /// Running 时光芒旋转、热浪/光晕流动。
    /// </summary>
    public enum HSunStyle
    {
        Classic = 0,       // 旋转光芒太阳
        Sunrise = 1,       // 日出
        Sunset = 2,        // 日落
        SmilingSun = 3,    // 笑脸太阳
        RaysOnly = 4,      // 光芒环
        SunGlow = 5,       // 光晕太阳
        SolarPanel = 6,    // 太阳能板与太阳
        HeatWave = 7,      // 热浪太阳
        EclipseSun = 8,    // 日环食
        Corona = 9,        // 日冕
        SunBurst = 10,     // 星爆太阳
        NoonSun = 11,      // 正午烈日
        SunsetSea = 12,    // 海上日落
        SunCloud = 13,     // 云遮日
        DottedRays = 14,   // 点状光芒
        SpiralRays = 15,   // 螺旋光芒
        SunDial = 16,      // 日晷
        SunRing = 17,      // 空心环日
        SunFlower = 18,    // 花瓣太阳
        HorizonSun = 19,   // 地平线横光
        DawnSun = 20,      // 晨光朝阳
        StarSun = 21       // 十字星芒太阳
    }
    /// <summary>
    /// 太阳动画控件（继承 HToolAnimBase）：22 种太阳造型，Running 时光芒旋转、
    /// 热浪流动；日盘为暖色语义色，金属部件随主题。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("太阳动画控件：22 种太阳造型，运行时光芒旋转")]
    public class HSun : HToolAnimBase
    {
        private HSunStyle _style = HSunStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Amber);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CRay = Color.FromArgb(255, 176, 40);
        private static readonly Color CDisc = Color.FromArgb(255, 200, 60);
        private static readonly Color CDiscLt = Color.FromArgb(255, 236, 150);
        private static readonly Color CDiscDk = Color.FromArgb(240, 140, 30);
        /// <summary>太阳造型样式。</summary>
        [HCategoryLanguage("太阳"), HDisplayNameLanguage("太阳造型样式"), HDescriptionLanguage("太阳造型样式"), Browsable(true)]
        [DefaultValue(HSunStyle.Classic)]
        public HSunStyle SunStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            if (a.Width <= 3f || a.Height <= 3f) { PaintPlacedText(g); return; }
            switch (_style)
            {
                case HSunStyle.Sunrise: DrawHalfSun(g, a, false); break;
                case HSunStyle.Sunset: DrawHalfSun(g, a, true); break;
                case HSunStyle.SmilingSun: DrawSmile(g, a); break;
                case HSunStyle.RaysOnly: DrawRays(g, a, 18, 0.32f, 0.44f, false); break;
                case HSunStyle.SunGlow: DrawGlow(g, a); break;
                case HSunStyle.SolarPanel: DrawPanel(g, a); break;
                case HSunStyle.HeatWave: DrawHeat(g, a); break;
                case HSunStyle.EclipseSun: DrawEclipse(g, a); break;
                case HSunStyle.Corona: DrawCorona(g, a); break;
                case HSunStyle.SunBurst: DrawBurst(g, a); break;
                case HSunStyle.NoonSun: DrawNoon(g, a); break;
                case HSunStyle.SunsetSea: DrawSunsetSea(g, a); break;
                case HSunStyle.SunCloud: DrawSunCloud(g, a); break;
                case HSunStyle.DottedRays: DrawDotted(g, a); break;
                case HSunStyle.SpiralRays: DrawSpiral(g, a); break;
                case HSunStyle.SunDial: DrawSundial(g, a); break;
                case HSunStyle.SunRing: DrawRing(g, a); break;
                case HSunStyle.SunFlower: DrawFlowerRays(g, a); break;
                case HSunStyle.HorizonSun: DrawHorizon(g, a); break;
                case HSunStyle.DawnSun: DrawDawn(g, a); break;
                case HSunStyle.StarSun: DrawStar(g, a); break;
                default: DrawClassic(g, a); break;
            }
            PaintPlacedText(g);
        }
        // 标准太阳几何
        private static void Geom(RectangleF a, out float cx, out float cy, out float r)
        {
            cx = a.Left + a.Width / 2f;
            cy = a.Top + a.Height / 2f;
            r = Math.Min(a.Width, a.Height) * 0.26f;
        }
        // 经典：旋转直光芒 + 日盘
        private void DrawClassic(Graphics g, RectangleF a)
        {
            DrawRays(g, a, 12, 0.32f, 0.46f, false);
            DrawDisc(g, a, CDiscLt, CDiscDk);
        }
        private void DrawDisc(Graphics g, RectangleF a, Color lt, Color dk)
        {
            Geom(a, out float cx, out float cy, out float r);
            var disc = new RectangleF(cx - r, cy - r, r * 2, r * 2);
            using (var db = new LinearGradientBrush(disc, lt, dk, 90f))
                g.FillEllipse(db, disc);
            using (var hl = new SolidBrush(Color.FromArgb(90, Color.White)))
                g.FillEllipse(hl, cx - r * 0.5f, cy - r * 0.55f, r * 0.42f, r * 0.32f);
        }
        // 光芒：n 根，r1/r2 为盘心距比例
        private void DrawRays(Graphics g, RectangleF a, int n, float r1, float r2, bool triangles)
        {
            Geom(a, out float cx, out float cy, out float rr);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 20f);
            float baseR = Math.Min(a.Width, a.Height);
            using (var pen = new Pen(CRay, Math.Max(1.5f, baseR * 0.025f))
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(CRay))
            {
                for (int i = 0; i < n; i++)
                {
                    double ang = i * Math.PI * 2 / n;
                    float x1 = (float)Math.Cos(ang) * baseR * r1;
                    float y1 = (float)Math.Sin(ang) * baseR * r1;
                    float x2 = (float)Math.Cos(ang) * baseR * r2;
                    float y2 = (float)Math.Sin(ang) * baseR * r2;
                    if (triangles)
                    {
                        double da = Math.PI / n * 0.32;
                        g.FillPolygon(brush, new[]
                        {
                            new PointF(x1, y1),
                            new PointF((float)Math.Cos(ang - da) * baseR * (r2 - 0.04f),
                                (float)Math.Sin(ang - da) * baseR * (r2 - 0.04f)),
                            new PointF(x2, y2),
                            new PointF((float)Math.Cos(ang + da) * baseR * (r2 - 0.04f),
                                (float)Math.Sin(ang + da) * baseR * (r2 - 0.04f))
                        });
                    }
                    else g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
            g.Restore(st);
        }
        // 半日出（日出/日落）
        private void DrawHalfSun(Graphics g, RectangleF a, bool sunset)
        {
            Color lt = sunset ? Color.FromArgb(255, 150, 80) : CDiscLt;
            Color dk = sunset ? Color.FromArgb(226, 80, 50) : CDiscDk;
            float r = Math.Min(a.Width, a.Height) * 0.34f;
            float cx = a.Left + a.Width / 2f;
            float horizon = a.Top + a.Height * 0.62f;
            float lift = Running ? (0.08f + 0.1f * (float)(Math.Sin(Phase) * 0.5 + 0.5)) : 0.1f;
            float cy = horizon + r - a.Height * lift;
            var disc = new RectangleF(cx - r, cy - r, r * 2, r * 2);
            using (var db = new LinearGradientBrush(disc, lt, dk, 90f))
                g.FillEllipse(db, disc);
            // 光芒（仅地平线以上）
            var old = g.Clip;
            g.SetClip(new RectangleF(a.Left, a.Top, a.Width, horizon - a.Top));
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 10f);
            using (var pen = new Pen(CRay, Math.Max(1.5f, r * 0.08f)))
                for (int i = 0; i < 10; i++)
                {
                    double ang = i * Math.PI * 2 / 10;
                    g.DrawLine(pen, (float)Math.Cos(ang) * r * 1.15f, (float)Math.Sin(ang) * r * 1.15f,
                        (float)Math.Cos(ang) * r * 1.5f, (float)Math.Sin(ang) * r * 1.5f);
                }
            g.Restore(st);
            g.Clip = old;
            // 海面/地面
            using (var sea = new LinearGradientBrush(
                new RectangleF(a.Left, horizon, a.Width, a.Bottom - horizon),
                sunset ? Color.FromArgb(90, 50, 80) : Color.FromArgb(40, 80, 120),
                sunset ? Color.FromArgb(40, 26, 50) : Color.FromArgb(20, 44, 74), 90f))
                g.FillRectangle(sea, a.Left, horizon, a.Width, a.Bottom - horizon);
            using (var line = new Pen(Color.FromArgb(200, 240, 200, 120), 2f))
                g.DrawLine(line, a.Left, horizon, a.Right, horizon);
        }
        // 笑脸太阳
        private void DrawSmile(Graphics g, RectangleF a)
        {
            DrawRays(g, a, 10, 0.34f, 0.46f, false);
            DrawDisc(g, a, CDiscLt, CDiscDk);
            Geom(a, out float cx, out float cy, out float r);
            using (var face = new Pen(Color.FromArgb(150, 90, 50), Math.Max(1.5f, r * 0.07f)))
            {
                g.DrawArc(face, cx - r * 0.46f, cy - r * 0.28f, r * 0.24f, r * 0.24f, 200f, 140f);
                g.DrawArc(face, cx + r * 0.22f, cy - r * 0.28f, r * 0.24f, r * 0.24f, 200f, 140f);
                g.DrawArc(face, cx - r * 0.44f, cy - r * 0.08f, r * 0.88f, r * 0.62f, 25f, 130f);
            }
            using (var blush = new SolidBrush(Color.FromArgb(90, 230, 120, 90)))
            {
                g.FillEllipse(blush, cx - r * 0.5f, cy + r * 0.08f, r * 0.16f, r * 0.1f);
                g.FillEllipse(blush, cx + r * 0.34f, cy + r * 0.08f, r * 0.16f, r * 0.1f);
            }
        }
        // 光晕
        private void DrawGlow(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out float r);
            float breath = Running ? 1f + 0.07f * (float)Math.Sin(Phase * 1.5) : 1f;
            for (int k = 4; k >= 1; k--)
                using (var halo = new SolidBrush(Color.FromArgb(12, 255, 190, 60)))
                    g.FillEllipse(halo, cx - r - k * r * 0.3f * breath, cy - r - k * r * 0.3f * breath,
                        (r + k * r * 0.3f) * 2 * breath, (r + k * r * 0.3f) * 2 * breath);
            DrawDisc(g, a, Color.FromArgb(255, 244, 180), CDisc);
        }
        // 太阳能板 + 小太阳
        private void DrawPanel(Graphics g, RectangleF a)
        {
            var pal = Palette;
            // 小太阳（右上）
            float sr = Math.Min(a.Width, a.Height) * 0.13f;
            float sx = a.Right - a.Width * 0.2f, sy = a.Top + a.Height * 0.22f;
            var st = g.Save();
            g.TranslateTransform(sx, sy);
            g.RotateTransform(Running ? SpinAngle : 0f);
            using (var ray = new Pen(CRay, Math.Max(1f, sr * 0.12f)))
                for (int i = 0; i < 8; i++)
                {
                    double ang = i * Math.PI / 4;
                    g.DrawLine(ray, (float)Math.Cos(ang) * sr * 1.1f, (float)Math.Sin(ang) * sr * 1.1f,
                        (float)Math.Cos(ang) * sr * 1.45f, (float)Math.Sin(ang) * sr * 1.45f);
                }
            g.Restore(st);
            using (var db = new SolidBrush(CDisc))
                g.FillEllipse(db, sx - sr, sy - sr, sr * 2, sr * 2);
            // 倾斜电池板
            st = g.Save();
            g.TranslateTransform(a.Left + a.Width * 0.5f, a.Top + a.Height * 0.62f);
            g.RotateTransform(-12f);
            float pw = a.Width * 0.62f, ph = a.Height * 0.34f;
            var board = new RectangleF(-pw / 2f, -ph / 2f, pw, ph);
            using (var bb = new LinearGradientBrush(board, Color.FromArgb(34, 70, 130), Color.FromArgb(18, 40, 80), 90f))
                g.FillRectangle(bb, board);
            using (var grid = new Pen(Color.FromArgb(120, 150, 200), 1f))
            {
                for (int i = 1; i < 4; i++)
                    g.DrawLine(grid, -pw / 2 + pw * i / 4, -ph / 2, -pw / 2 + pw * i / 4, ph / 2);
                g.DrawLine(grid, -pw / 2, 0, pw / 2, 0);
            }
            g.Restore(st);
            // 支架
            using (var leg = new SolidBrush(pal.MetalDark))
                g.FillRectangle(leg, a.Left + a.Width * 0.47f, a.Top + a.Height * 0.62f,
                    a.Width * 0.06f, a.Height * 0.26f);
        }
        // 热浪
        private void DrawHeat(Graphics g, RectangleF a)
        {
            DrawDisc(g, a, CDiscLt, CDiscDk);
            Geom(a, out float cx, out _, out float r);
            float shift = Running ? Phase * a.Width * 0.05f : 0f;
            using (var wave = new Pen(Color.FromArgb(200, CRay), Math.Max(1.5f, r * 0.07f)))
                for (int i = -1; i <= 1; i++)
                {
                    float x0 = cx + i * r * 0.8f + shift % (r * 1.6f);
                    using (var path = new GraphicsPath())
                    {
                        path.AddBezier(x0 - r * 0.2f, a.Bottom - a.Height * 0.06f,
                            x0 + r * 0.3f, a.Bottom - a.Height * 0.16f,
                            x0 - r * 0.3f, a.Bottom - a.Height * 0.26f,
                            x0 + r * 0.2f, a.Bottom - a.Height * 0.36f);
                        g.DrawPath(wave, path);
                    }
                }
        }
        // 日环食
        private void DrawEclipse(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out float r);
            using (var corona = new SolidBrush(Color.FromArgb(80, 255, 220, 140)))
                g.FillEllipse(corona, cx - r * 1.5f, cy - r * 1.5f, r * 3, r * 3);
            using (var db = new SolidBrush(Color.FromArgb(255, 220, 130)))
                g.FillEllipse(db, cx - r * 1.12f, cy - r * 1.12f, r * 2.24f, r * 2.24f);
            using (var moon = new SolidBrush(Color.FromArgb(255, 22, 24, 32)))
                g.FillEllipse(moon, cx - r * 0.98f, cy - r * 0.98f, r * 1.96f, r * 1.96f);
        }
        // 日冕（长短交替大光冠）
        private void DrawCorona(Graphics g, RectangleF a)
        {
            DrawRays(g, a, 24, 0.3f, 0.48f, false);
            DrawDisc(g, a, Color.FromArgb(255, 240, 200), CDisc);
        }
        // 星爆
        private void DrawBurst(Graphics g, RectangleF a)
        {
            DrawRays(g, a, 16, 0.28f, 0.48f, true);
            DrawDisc(g, a, CDiscLt, CDiscDk);
        }
        // 正午烈日
        private void DrawNoon(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out float r);
            for (int k = 3; k >= 1; k--)
                using (var halo = new SolidBrush(Color.FromArgb(14, 255, 255, 220)))
                    g.FillEllipse(halo, cx - r - k * r * 0.22f, cy - r - k * r * 0.22f,
                        (r + k * r * 0.22f) * 2, (r + k * r * 0.22f) * 2);
            DrawDisc(g, a, Color.White, Color.FromArgb(255, 226, 120));
        }
        // 海上日落 + 倒影
        private void DrawSunsetSea(Graphics g, RectangleF a)
        {
            DrawHalfSun(g, a, true);
            Geom(a, out float cx, out _, out float r);
            float horizon = a.Top + a.Height * 0.62f;
            using (var refl = new Pen(Color.FromArgb(170, 255, 170, 90), Math.Max(1.5f, r * 0.1f)))
                for (int i = 0; i < 4; i++)
                {
                    float w = r * (1.2f - i * 0.22f);
                    float y = horizon + r * 0.25f + i * r * 0.36f;
                    float wob = Running ? (float)Math.Sin(Phase * 2 + i) * r * 0.1f : 0f;
                    g.DrawLine(refl, cx - w / 2 + wob, y, cx + w / 2 + wob, y);
                }
        }
        // 云遮日
        private void DrawSunCloud(Graphics g, RectangleF a)
        {
            DrawDisc(g, a, CDiscLt, CDiscDk);
            float s = Math.Min(a.Width, a.Height);
            float drift = Running ? (float)Math.Sin(Phase) * s * 0.04f : 0f;
            using (var cb = new SolidBrush(Color.FromArgb(235, 225, 228, 236)))
            {
                float cl = a.Left + a.Width * 0.3f + drift;
                float y = a.Bottom - s * 0.38f;
                g.FillEllipse(cb, cl, y, s * 0.28f, s * 0.22f);
                g.FillEllipse(cb, cl + s * 0.18f, y - s * 0.12f, s * 0.32f, s * 0.26f);
                g.FillEllipse(cb, cl + s * 0.42f, y - s * 0.02f, s * 0.24f, s * 0.2f);
                g.FillRectangle(cb, cl, y + s * 0.08f, s * 0.66f, s * 0.1f);
            }
        }
        // 点状光芒
        private void DrawDotted(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out _);
            float baseR = Math.Min(a.Width, a.Height);
            float ang0 = Running ? SpinAngle * (float)Math.PI / 180f : 0f;
            using (var dot = new SolidBrush(CRay))
                for (int ring = 0; ring < 2; ring++)
                    for (int i = 0; i < 12; i++)
                    {
                        double ang = ang0 + i * Math.PI * 2 / 12 + ring * Math.PI / 12;
                        float rad = baseR * (ring == 0 ? 0.38f : 0.46f);
                        float d = baseR * (ring == 0 ? 0.018f : 0.026f);
                        g.FillEllipse(dot, cx + (float)Math.Cos(ang) * rad - d / 2,
                            cy + (float)Math.Sin(ang) * rad - d / 2, d, d);
                    }
            DrawDisc(g, a, CDiscLt, CDiscDk);
        }
        // 螺旋卷芒
        private void DrawSpiral(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out _);
            float baseR = Math.Min(a.Width, a.Height);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 0f);
            using (var pen = new Pen(CRay, Math.Max(1.5f, baseR * 0.02f)))
                for (int i = 0; i < 8; i++)
                {
                    g.RotateTransform(45f);
            g.DrawArc(pen, baseR * 0.26f, -baseR * 0.07f, baseR * 0.2f, baseR * 0.14f, -100f, 200f);
                }
            g.Restore(st);
            DrawDisc(g, a, CDiscLt, CDiscDk);
        }
        // 日晷
        private void DrawSundial(Graphics g, RectangleF a)
        {
            // 底盘
            var plate = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.4f,
                a.Width * 0.76f, a.Height * 0.42f);
            using (var pb = new LinearGradientBrush(plate, Color.FromArgb(232, 220, 190),
                Color.FromArgb(190, 172, 134), 90f))
                g.FillEllipse(pb, plate);
            // 时刻刻线
            float cxp = plate.Left + plate.Width / 2f;
            using (var tick = new Pen(Color.FromArgb(120, 90, 60), 1.2f))
                for (int i = 0; i < 9; i++)
                {
                    float t = i / 8f;
                    g.DrawLine(tick, plate.Left + plate.Width * (0.12f + t * 0.76f),
                        plate.Top + plate.Height * 0.6f,
                        plate.Left + plate.Width * (0.14f + t * 0.72f), plate.Bottom - plate.Height * 0.12f);
                }
            // 晷针影
            float shadowAng = Running ? SpinAngle : 40f;
            using (var sh = new SolidBrush(Color.FromArgb(120, 70, 60, 50)))
                g.FillPolygon(sh, new[]
                {
                    new PointF(cxp, plate.Top + plate.Height * 0.1f),
                    new PointF(cxp + (float)Math.Cos(shadowAng * Math.PI / 180) * plate.Width * 0.3f,
                        plate.Bottom - plate.Height * 0.2f),
                    new PointF(cxp, plate.Top + plate.Height * 0.3f)
                });
            // 小太阳
            float sr = Math.Min(a.Width, a.Height) * 0.15f;
            float sx = a.Left + a.Width * 0.72f, sy = a.Top + a.Height * 0.2f;
            using (var db = new SolidBrush(CDisc))
                g.FillEllipse(db, sx - sr, sy - sr, sr * 2, sr * 2);
        }
        // 空心环日
        private void DrawRing(Graphics g, RectangleF a)
        {
            DrawRays(g, a, 12, 0.34f, 0.46f, false);
            Geom(a, out float cx, out float cy, out float r);
            using (var ring = new Pen(CDisc, r * 0.22f) { StartCap = LineCap.Round })
                g.DrawEllipse(ring, cx - r, cy - r, r * 2, r * 2);
        }
        // 花瓣太阳
        private void DrawFlowerRays(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out _);
            float baseR = Math.Min(a.Width, a.Height);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle * 0.5f : 0f);
            using (var pb = new SolidBrush(Color.FromArgb(255, 210, 90)))
                for (int i = 0; i < 14; i++)
                {
                    g.RotateTransform(360f / 14);
                    g.FillEllipse(pb, baseR * 0.26f, -baseR * 0.05f,
                        baseR * 0.2f, baseR * 0.1f);
                }
            g.Restore(st);
            DrawDisc(g, a, CDiscLt, CDiscDk);
        }
        // 地平线横光
        private void DrawHorizon(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.26f;
            float cx = a.Left + a.Width / 2f;
            float horizon = a.Top + a.Height * 0.6f;
            float cy = horizon + r * 0.7f;
            using (var db = new LinearGradientBrush(
                new RectangleF(cx - r, cy - r, r * 2, r * 2), CDiscLt, CDiscDk, 90f))
                g.FillEllipse(db, cx - r, cy - r, r * 2, r * 2);
            using (var ground = new SolidBrush(Color.FromArgb(40, 44, 60)))
                g.FillRectangle(ground, a.Left, horizon, a.Width, a.Bottom - horizon);
            using (var ray = new Pen(Color.FromArgb(150, 255, 200, 100), Math.Max(1f, r * 0.05f)))
                for (int i = -3; i <= 3; i++)
                    g.DrawLine(ray, cx + i * r * 0.4f, horizon - r * 0.1f,
                        cx + i * r * 0.7f, a.Top + r * 0.3f);
        }
        // 晨光朝阳（柔光 + 地平线）
        private void DrawDawn(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.3f;
            float cx = a.Left + a.Width / 2f;
            float horizon = a.Top + a.Height * 0.66f;
            float cy = horizon + r * 0.8f;
            for (int k = 3; k >= 1; k--)
                using (var halo = new SolidBrush(Color.FromArgb(16, 255, 210, 150)))
                    g.FillEllipse(halo, cx - r - k * r * 0.28f, cy - r - k * r * 0.28f,
                        (r + k * r * 0.28f) * 2, (r + k * r * 0.28f) * 2);
            using (var db = new SolidBrush(Color.FromArgb(255, 232, 170)))
                g.FillEllipse(db, cx - r, cy - r, r * 2, r * 2);
            using (var ground = new LinearGradientBrush(
                new RectangleF(a.Left, horizon, a.Width, a.Bottom - horizon),
                Color.FromArgb(90, 90, 120), Color.FromArgb(50, 50, 80), 90f))
                g.FillRectangle(ground, a.Left, horizon, a.Width, a.Bottom - horizon);
        }
        // 十字星芒太阳
        private void DrawStar(Graphics g, RectangleF a)
        {
            Geom(a, out float cx, out float cy, out float r);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 0f);
            using (var ray = new Pen(Color.FromArgb(255, 220, 90), Math.Max(2f, r * 0.08f))
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 4; i++)
                {
                    g.RotateTransform(90f);
                    g.DrawLine(ray, r * 1.05f, 0, r * 1.7f, 0);
                }
            g.Restore(st);
            DrawDisc(g, a, Color.White, CDisc);
        }
    }
}