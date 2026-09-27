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
    /// 星空动画外形：22 种夜空造型——满天繁星、流星、北斗/猎户星座、银河、弯月星空、
    /// 星雨、星爆、萤火虫、星轨、星云、北极星、螺旋星系、彗星等。Running 时闪烁/流星运行。
    /// </summary>
    public enum HStarSkyStyle
    {
        Classic = 0,        // 满天繁星
        ShootingStars = 1,  // 多流星
        BigDipper = 2,      // 北斗七星
        Orion = 3,          // 猎户座
        Galaxy = 4,         // 银河
        MoonStars = 5,      // 弯月星空
        StarFall = 6,       // 星雨
        SparkBurst = 7,     // 星爆
        TwinkleRing = 8,    // 环形闪烁
        SnowNight = 9,      // 雪夜星闪
        Fireflies = 10,     // 萤火虫
        Constellation = 11, // 连线星座
        StarTrail = 12,     // 同心圆星轨
        MeteorShower = 13,  // 流星雨
        Nebula = 14,        // 彩色星云
        PoleStar = 15,      // 北极星
        StarWave = 16,      // 星浪
        SpiralGalaxy = 17,  // 螺旋星系
        NightSky = 18,      // 夜空远山
        StarChain = 19,     // 轨道星链
        Comet = 20,         // 彗星
        CrossStars = 21     // 十字闪光群星
    }
    /// <summary>
    /// 满天星动画控件（继承 HToolAnimBase）：22 种夜空/星空造型，
    /// Running 时繁星闪烁、流星划落、星轨旋转；Classic 为深蓝夜空。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("满天星动画控件：22 种星空造型，运行时闪烁流星")]
    public class HStarSky : HToolAnimBase
    {
        private HStarSkyStyle _style = HStarSkyStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CSkyTop = Color.FromArgb(18, 28, 66);
        private static readonly Color CSkyBottom = Color.FromArgb(6, 10, 28);
        /// <summary>星空造型样式。</summary>
        [HCategoryLanguage("星空"), HDisplayNameLanguage("星空造型样式"), HDescriptionLanguage("星空造型样式"), Browsable(true)]
        [DefaultValue(HStarSkyStyle.Classic)]
        public HStarSkyStyle StarSkyStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            if (a.Width <= 4f || a.Height <= 4f) { PaintPlacedText(g); return; }
            DrawSky(g, a);
            switch (_style)
            {
                case HStarSkyStyle.ShootingStars: DrawField(g, a, 24); DrawMeteor(g, a, 2, false); break;
                case HStarSkyStyle.BigDipper: DrawBigDipper(g, a); break;
                case HStarSkyStyle.Orion: DrawOrion(g, a); break;
                case HStarSkyStyle.Galaxy: DrawGalaxy(g, a); break;
                case HStarSkyStyle.MoonStars: DrawMoonStars(g, a); break;
                case HStarSkyStyle.StarFall: DrawStarFall(g, a); break;
                case HStarSkyStyle.SparkBurst: DrawSparkBurst(g, a); break;
                case HStarSkyStyle.TwinkleRing: DrawTwinkleRing(g, a); break;
                case HStarSkyStyle.SnowNight: DrawSnowNight(g, a); break;
                case HStarSkyStyle.Fireflies: DrawFireflies(g, a); break;
                case HStarSkyStyle.Constellation: DrawRandomConstellation(g, a); break;
                case HStarSkyStyle.StarTrail: DrawStarTrail(g, a); break;
                case HStarSkyStyle.MeteorShower: DrawField(g, a, 18); DrawMeteor(g, a, 5, true); break;
                case HStarSkyStyle.Nebula: DrawNebula(g, a); break;
                case HStarSkyStyle.PoleStar: DrawPoleStar(g, a); break;
                case HStarSkyStyle.StarWave: DrawStarWave(g, a); break;
                case HStarSkyStyle.SpiralGalaxy: DrawSpiral(g, a); break;
                case HStarSkyStyle.NightSky: DrawNightSky(g, a); break;
                case HStarSkyStyle.StarChain: DrawField(g, a, 14); DrawStarChain(g, a); break;
                case HStarSkyStyle.Comet: DrawField(g, a, 16); DrawComet(g, a); break;
                case HStarSkyStyle.CrossStars: DrawCrossStars(g, a); break;
                default: DrawField(g, a, 46); break;
            }
            PaintPlacedText(g);
        }
        // 夜空天幕
        private void DrawSky(Graphics g, RectangleF a)
        {
            using (var sky = new LinearGradientBrush(a, CSkyTop, CSkyBottom, 90f))
            using (var path = HBarBase.RoundPath(a, Math.Min(10f, Math.Min(a.Width, a.Height) * 0.08f)))
                g.FillPath(sky, path);
        }
        // 星点（dot=true 圆点；否则十字芒星）
        private void StarDot(Graphics g, float x, float y, float r, int alpha)
        {
            if (r < 0.6f || alpha <= 0) return;
            using (var sb = new SolidBrush(Color.FromArgb(Math.Min(255, alpha), 255, 252, 230)))
                g.FillEllipse(sb, x - r, y - r, r * 2f, r * 2f);
        }
        private void CrossStar(Graphics g, float x, float y, float r, int alpha)
        {
            if (r < 1f || alpha <= 0) return;
            using (var sb = new SolidBrush(Color.FromArgb(Math.Min(255, alpha), 255, 252, 230)))
                g.FillPolygon(sb, new[]
                {
                    new PointF(x, y - r), new PointF(x + r * 0.22f, y - r * 0.22f),
                    new PointF(x + r, y), new PointF(x + r * 0.22f, y + r * 0.22f),
                    new PointF(x, y + r), new PointF(x - r * 0.22f, y + r * 0.22f),
                    new PointF(x - r, y), new PointF(x - r * 0.22f, y - r * 0.22f)
                });
        }
        // 随机星野（固定种子，闪烁随 Phase）
        private void DrawField(Graphics g, RectangleF a, int count)
        {
            var rnd = new Random(42);
            for (int i = 0; i < count; i++)
            {
                float fx = rnd.Next(4, 96) / 100f;
                float fy = rnd.Next(4, 92) / 100f;
                float rr = a.Width * (0.004f + rnd.Next(0, 12) / 1000f);
                int baseA = 90 + rnd.Next(0, 120);
                int alpha = Running
                    ? (int)(baseA * (0.45 + 0.55 * Math.Abs(Math.Sin(Phase * 1.3 + i * 1.7))))
                    : baseA;
                if (i % 7 == 0)
                    CrossStar(g, a.Left + a.Width * fx, a.Top + a.Height * fy, rr * 2.4f, alpha);
                else
                    StarDot(g, a.Left + a.Width * fx, a.Top + a.Height * fy, rr * 1.4f, alpha);
            }
        }
        // 流星：n 颗，沿对角周期划落
        private void DrawMeteor(Graphics g, RectangleF a, int n, bool shower)
        {
            for (int i = 0; i < n; i++)
            {
                float seed = i * 0.37f + (shower ? 0.1f : 0f);
                float t = Running ? (Phase * 0.16f + seed) % 1f : (i * 0.25f + 0.2f) % 1f;
                float sx = a.Left + a.Width * (0.1f + (i * 0.29f) % 0.7f) + t * a.Width * 0.5f;
                float sy = a.Top + a.Height * 0.05f + t * a.Height * 0.8f;
                float len = Math.Min(a.Width, a.Height) * (shower ? 0.22f : 0.18f);
                int fade = (int)(220 * Math.Sin(t * Math.PI));
                using (var tail = new Pen(Color.FromArgb(Math.Max(0, fade), 255, 250, 220),
                    Math.Max(1f, a.Width * 0.012f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLine(tail, sx, sy, sx - len * 0.7f, sy - len * 0.5f);
                StarDot(g, sx, sy, a.Width * 0.016f, fade);
            }
        }
        // 北斗七星
        private void DrawBigDipper(Graphics g, RectangleF a)
        {
            var pts = new PointF[7];
            float[] px = { 0.16f, 0.3f, 0.44f, 0.58f, 0.72f, 0.78f, 0.74f };
            float[] py = { 0.66f, 0.58f, 0.54f, 0.5f, 0.42f, 0.28f, 0.14f };
            for (int i = 0; i < 7; i++)
                pts[i] = new PointF(a.Left + a.Width * px[i], a.Top + a.Height * py[i]);
            using (var line = new Pen(Color.FromArgb(120, 200, 220, 255), Math.Max(1f, a.Width * 0.01f)))
                g.DrawLines(line, pts);
            for (int i = 0; i < 7; i++)
            {
                int al = Running ? (int)(160 + 80 * Math.Abs(Math.Sin(Phase * 2 + i))) : 220;
                StarDot(g, pts[i].X, pts[i].Y, a.Width * (i == 6 ? 0.022f : 0.016f), al);
            }
        }
        // 猎户座
        private void DrawOrion(Graphics g, RectangleF a)
        {
            var stars = new (float x, float y, float r)[]
            {
                (0.5f, 0.14f, 0.02f), (0.28f, 0.24f, 0.015f), (0.72f, 0.24f, 0.015f),
                (0.42f, 0.4f, 0.012f), (0.58f, 0.4f, 0.012f),
                (0.46f, 0.52f, 0.013f), (0.54f, 0.52f, 0.013f), (0.5f, 0.52f, 0.013f),
                (0.34f, 0.78f, 0.016f), (0.66f, 0.78f, 0.016f)
            };
            using (var line = new Pen(Color.FromArgb(100, 200, 220, 255), Math.Max(1f, a.Width * 0.008f)))
            {
                g.DrawLine(line, X(0.28f), Y(0.24f), X(0.42f), Y(0.4f));
                g.DrawLine(line, X(0.72f), Y(0.24f), X(0.58f), Y(0.4f));
                g.DrawLine(line, X(0.42f), Y(0.4f), X(0.34f), Y(0.78f));
                g.DrawLine(line, X(0.58f), Y(0.4f), X(0.66f), Y(0.78f));
                g.DrawLine(line, X(0.46f), Y(0.52f), X(0.54f), Y(0.52f));
            }
            foreach (var s in stars)
                StarDot(g, X(s.x), Y(s.y), a.Width * s.r, Running ? 200 : 230);
            float X(float f) => a.Left + a.Width * f;
            float Y(float f) => a.Top + a.Height * f;
        }
        // 银河斜带
        private void DrawGalaxy(Graphics g, RectangleF a)
        {
            var st = g.Save();
            g.TranslateTransform(a.Left + a.Width / 2f, a.Top + a.Height / 2f);
            g.RotateTransform(-24f);
            using (var band = new SolidBrush(Color.FromArgb(46, 190, 190, 230)))
                g.FillEllipse(band, -a.Width * 0.72f, -a.Height * 0.16f, a.Width * 1.44f, a.Height * 0.32f);
            using (var core = new SolidBrush(Color.FromArgb(70, 240, 235, 255)))
                g.FillEllipse(core, -a.Width * 0.12f, -a.Height * 0.1f, a.Width * 0.24f, a.Height * 0.2f);
            g.Restore(st);
            var rnd = new Random(7);
            for (int i = 0; i < 60; i++)
            {
                double ang = -24 * Math.PI / 180;
                float u = (rnd.Next(0, 100) - 50) / 50f * a.Width * 0.66f;
                float v = (rnd.Next(0, 100) - 50) / 50f * a.Height * 0.13f;
                float x = a.Left + a.Width / 2f + u * (float)Math.Cos(ang) - v * (float)Math.Sin(ang);
                float y = a.Top + a.Height / 2f + u * (float)Math.Sin(ang) + v * (float)Math.Cos(ang);
                int al = Running ? (int)(120 + 100 * Math.Abs(Math.Sin(Phase + i))) : 180;
                StarDot(g, x, y, a.Width * 0.006f, al);
            }
        }
        // 弯月 + 星
        private void DrawMoonStars(Graphics g, RectangleF a)
        {
            DrawField(g, a, 20);
            float mr = Math.Min(a.Width, a.Height) * 0.2f;
            float cx = a.Right - a.Width * 0.22f, cy = a.Top + a.Height * 0.26f;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - mr, cy - mr, mr * 2, mr * 2);
                path.AddEllipse(cx - mr * 0.4f, cy - mr * 1.12f, mr * 2.1f, mr * 2.1f);
                using (var mb = new SolidBrush(Color.FromArgb(250, 246, 214)))
                    g.FillPath(mb, path);
            }
        }
        // 星雨（多列下滑）
        private void DrawStarFall(Graphics g, RectangleF a)
        {
            var rnd = new Random(3);
            for (int i = 0; i < 26; i++)
            {
                float fx = rnd.Next(4, 96) / 100f;
                float t = Running ? (Phase * 0.22f + i * 0.07f) % 1f : (i * 0.07f) % 1f;
                float y = a.Top + t * a.Height;
                float sway = (float)Math.Sin(t * 6 + i) * a.Width * 0.012f;
                int al = (int)(220 * Math.Sin(t * Math.PI));
                StarDot(g, a.Left + a.Width * fx + sway, y, a.Width * 0.01f, al);
            }
        }
        // 星爆
        private void DrawSparkBurst(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            for (int i = 0; i < 14; i++)
            {
                double ang = i * Math.PI * 2 / 14;
                float t = Running ? 0.5f + 0.5f * (float)Math.Sin(Phase * 2 + i * 0.7) : 0.7f;
                float d = Math.Min(a.Width, a.Height) * 0.34f * t;
                CrossStar(g, cx + (float)Math.Cos(ang) * d, cy + (float)Math.Sin(ang) * d,
                    a.Width * 0.014f, 160 + (int)(90 * t));
            }
            CrossStar(g, cx, cy, a.Width * 0.06f, 255);
        }
        // 环形闪烁
        private void DrawTwinkleRing(Graphics g, RectangleF a)
        {
            DrawField(g, a, 14);
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float rad = Math.Min(a.Width, a.Height) * 0.34f;
            for (int i = 0; i < 12; i++)
            {
                double ang = i * Math.PI * 2 / 12;
                int al = Running
                    ? (int)(70 + 180 * Math.Abs(Math.Sin(Phase * 2.2 + i * 0.9)))
                    : 180;
                CrossStar(g, cx + (float)Math.Cos(ang) * rad, cy + (float)Math.Sin(ang) * rad,
                    a.Width * 0.014f, al);
            }
        }
        // 雪夜（大十字星 + 雪点）
        private void DrawSnowNight(Graphics g, RectangleF a)
        {
            DrawField(g, a, 30);
            using (var snow = new SolidBrush(Color.White))
            {
                var rnd = new Random(11);
                for (int i = 0; i < 14; i++)
                {
                    float t = Running ? (Phase * 0.12f + i * 0.08f) % 1f : i * 0.08f;
                    float x = a.Left + a.Width * (rnd.Next(5, 95) / 100f)
                        + (float)Math.Sin(t * 8 + i) * a.Width * 0.02f;
                    g.FillEllipse(snow, x, a.Top + t * a.Height, a.Width * 0.012f, a.Width * 0.012f);
                }
            }
        }
        // 萤火虫
        private void DrawFireflies(Graphics g, RectangleF a)
        {
            var rnd = new Random(21);
            for (int i = 0; i < 12; i++)
            {
                float bx = a.Left + a.Width * (0.12f + rnd.Next(0, 76) / 100f);
                float by = a.Top + a.Height * (0.2f + rnd.Next(0, 70) / 100f);
                float t = Running ? Phase * 0.5f + i : i;
                float x = bx + (float)Math.Sin(t * 1.3) * a.Width * 0.06f;
                float y = by + (float)Math.Cos(t * 0.9) * a.Height * 0.05f;
                int al = Running ? (int)(120 + 120 * Math.Abs(Math.Sin(Phase * 3 + i * 2))) : 160;
                using (var glow = new SolidBrush(Color.FromArgb(al / 3, 200, 255, 120)))
                    g.FillEllipse(glow, x - a.Width * 0.03f, y - a.Width * 0.03f, a.Width * 0.06f, a.Width * 0.06f);
                using (var dot = new SolidBrush(Color.FromArgb(Math.Min(255, al), 230, 255, 140)))
                    g.FillEllipse(dot, x - a.Width * 0.008f, y - a.Width * 0.008f,
                        a.Width * 0.016f, a.Width * 0.016f);
            }
        }
        // 随机星座
        private void DrawRandomConstellation(Graphics g, RectangleF a)
        {
            var rnd = new Random(33);
            int n = 9;
            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
                pts[i] = new PointF(a.Left + a.Width * (0.12f + rnd.Next(0, 76) / 100f),
                    a.Top + a.Height * (0.12f + rnd.Next(0, 76) / 100f));
            using (var line = new Pen(Color.FromArgb(110, 200, 220, 255), Math.Max(1f, a.Width * 0.008f)))
                for (int i = 1; i < n; i++)
                    if (rnd.Next(0, 2) > 0) g.DrawLine(line, pts[i - 1], pts[i]);
            foreach (var pt in pts)
                StarDot(g, pt.X, pt.Y, a.Width * 0.014f, Running ? 210 : 230);
            DrawField(g, a, 12);
        }
        // 星轨
        private void DrawStarTrail(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var ring = new Pen(Color.FromArgb(60, 200, 220, 255), 1f))
                for (int r = 1; r <= 3; r++)
                {
                    float d = Math.Min(a.Width, a.Height) * 0.24f * r;
                    g.DrawEllipse(ring, cx - d, cy - d * 0.55f, d * 2, d * 1.1f);
                }
            for (int i = 0; i < 6; i++)
            {
                float ang = Running ? SpinAngle * (float)Math.PI / 180f + i * 1.1f : i;
                float rad = Math.Min(a.Width, a.Height) * 0.28f * (1 + (i % 3) * 0.5f);
                CrossStar(g, cx + (float)Math.Cos(ang) * rad, cy + (float)Math.Sin(ang) * rad * 0.55f,
                    a.Width * 0.012f, 230);
            }
        }
        // 星云
        private void DrawNebula(Graphics g, RectangleF a)
        {
            Color[] clouds = { Color.FromArgb(70, 140, 90, 220), Color.FromArgb(60, 60, 140, 220),
                Color.FromArgb(60, 220, 100, 160) };
            var rnd = new Random(17);
            for (int k = 0; k < 3; k++)
            {
                float cx = a.Left + a.Width * (0.3f + k * 0.2f);
                float cy = a.Top + a.Height * (0.4f + (k % 2) * 0.16f);
                float rw = a.Width * 0.34f, rh = a.Height * 0.3f;
                using (var cb = new SolidBrush(clouds[k]))
                    g.FillEllipse(cb, cx - rw / 2, cy - rh / 2, rw, rh);
            }
            for (int i = 0; i < 40; i++)
            {
                float x = a.Left + a.Width * (rnd.Next(8, 92) / 100f);
                float y = a.Top + a.Height * (rnd.Next(10, 90) / 100f);
                int al = Running ? (int)(100 + 120 * Math.Abs(Math.Sin(Phase + i * 1.3))) : 180;
                StarDot(g, x, y, a.Width * 0.007f, al);
            }
        }
        // 北极星
        private void DrawPoleStar(Graphics g, RectangleF a)
        {
            DrawField(g, a, 22);
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var ring = new Pen(Color.FromArgb(100, 200, 220, 255), 1.4f))
                g.DrawEllipse(ring, cx - a.Width * 0.16f, cy - a.Width * 0.16f,
                    a.Width * 0.32f, a.Width * 0.32f);
            float tw = Running ? 1f + 0.15f * (float)Math.Sin(Phase * 2) : 1f;
            CrossStar(g, cx, cy, a.Width * 0.1f * tw, 255);
        }
        // 星浪
        private void DrawStarWave(Graphics g, RectangleF a)
        {
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 9; col++)
                {
                    float wave = (float)Math.Sin(col * 0.8 + row * 1.4 + (Running ? Phase : 0));
                    float x = a.Left + a.Width * (0.07f + col * 0.107f);
                    float y = a.Top + a.Height * (0.2f + row * 0.2f) + wave * a.Height * 0.05f;
                    int al = (int)(120 + 120 * (wave * 0.5f + 0.5f));
                    if ((row + col) % 3 == 0) CrossStar(g, x, y, a.Width * 0.012f, al);
                    else StarDot(g, x, y, a.Width * 0.008f, al);
                }
        }
        // 螺旋星系
        private void DrawSpiral(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float maxR = Math.Min(a.Width, a.Height) * 0.42f;
            var rnd = new Random(5);
            for (int arm = 0; arm < 2; arm++)
                for (int i = 0; i < 46; i++)
                {
                    float t = i / 45f;
                    double ang = arm * Math.PI + t * 4.2 + (rnd.Next(-1, 1) * 0.12);
                    float rad = maxR * t;
                    float x = cx + (float)Math.Cos(ang) * rad;
                    float y = cy + (float)Math.Sin(ang) * rad * 0.72f;
                    int al = (int)(120 + 120 * (1 - t));
                    StarDot(g, x, y, a.Width * (0.004f + (1 - t) * 0.008f),
                        Running ? (int)(al * (0.6 + 0.4 * Math.Abs(Math.Sin(Phase + i)))) : al);
                }
            using (var core = new SolidBrush(Color.FromArgb(220, 250, 244, 214)))
                g.FillEllipse(core, cx - maxR * 0.16f, cy - maxR * 0.12f, maxR * 0.32f, maxR * 0.24f);
        }
        // 夜空远山
        private void DrawNightSky(Graphics g, RectangleF a)
        {
            DrawField(g, a, 34);
            using (var m1 = new SolidBrush(Color.FromArgb(20, 28, 52)))
                g.FillPolygon(m1, new[]
                {
                    new PointF(a.Left, a.Bottom - a.Height * 0.22f),
                    new PointF(a.Left + a.Width * 0.24f, a.Top + a.Height * 0.42f),
                    new PointF(a.Left + a.Width * 0.46f, a.Bottom - a.Height * 0.22f),
                    new PointF(a.Left + a.Width * 0.66f, a.Top + a.Height * 0.5f),
                    new PointF(a.Right, a.Bottom - a.Height * 0.16f),
                    new PointF(a.Right, a.Bottom), new PointF(a.Left, a.Bottom)
                });
            using (var m2 = new SolidBrush(Color.FromArgb(10, 14, 30)))
                g.FillPolygon(m2, new[]
                {
                    new PointF(a.Left, a.Bottom - a.Height * 0.1f),
                    new PointF(a.Left + a.Width * 0.3f, a.Top + a.Height * 0.66f),
                    new PointF(a.Left + a.Width * 0.58f, a.Bottom - a.Height * 0.12f),
                    new PointF(a.Right, a.Top + a.Height * 0.72f),
                    new PointF(a.Right, a.Bottom), new PointF(a.Left, a.Bottom)
                });
        }
        // 轨道星链
        private void DrawStarChain(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.3f;
            using (var path = new Pen(Color.FromArgb(50, 200, 220, 255), 1f))
                g.DrawBezier(path, a.Left, cy + a.Height * 0.16f,
                    a.Left + a.Width * 0.3f, cy - a.Height * 0.12f,
                    a.Left + a.Width * 0.7f, cy - a.Height * 0.12f,
                    a.Right, cy + a.Height * 0.16f);
            for (int i = 0; i < 8; i++)
            {
                float t = Running ? (i / 8f + Phase * 0.08f) % 1f : i / 8f;
                float x = a.Left + t * a.Width;
                float y = cy + a.Height * 0.16f * (1 - 4f * (t - 0.5f) * (t - 0.5f)) - a.Height * 0.06f;
                StarDot(g, x, y, a.Width * 0.011f, 230);
            }
        }
        // 彗星
        private void DrawComet(Graphics g, RectangleF a)
        {
            float t = Running ? (Phase * 0.2f + 0.2f) % 1f : 0.55f;
            float cx = a.Left + a.Width * (0.12f + t * 0.7f);
            float cy = a.Top + a.Height * (0.14f + t * 0.62f);
            float len = Math.Min(a.Width, a.Height) * 0.42f;
            for (int k = 6; k >= 1; k--)
                using (var tail = new SolidBrush(Color.FromArgb(10 + k * 8, 180, 220, 255)))
                    g.FillEllipse(tail, cx - len * k / 6f * 0.86f - a.Width * 0.02f,
                        cy - len * k / 6f * 0.7f - a.Width * 0.02f,
                        a.Width * 0.04f + len * k / 6f * 0.3f, a.Width * 0.04f + len * k / 6f * 0.24f);
            StarDot(g, cx, cy, a.Width * 0.03f, 255);
        }
        // 十字闪光群星
        private void DrawCrossStars(Graphics g, RectangleF a)
        {
            var rnd = new Random(55);
            for (int i = 0; i < 12; i++)
            {
                float x = a.Left + a.Width * (rnd.Next(6, 94) / 100f);
                float y = a.Top + a.Height * (rnd.Next(6, 92) / 100f);
                int al = Running ? (int)(100 + 150 * Math.Abs(Math.Sin(Phase * 2 + i * 1.6))) : 220;
                CrossStar(g, x, y, a.Width * (0.012f + rnd.Next(0, 14) / 1000f), al);
            }
        }
    }
}