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
    /// 月亮动画外形：22 种月亮造型——弯月、满月、连续月相、星月、月食、血月、
    /// 陨石坑、弦月、凸月、新月、云遮月、丰收月、蓝月、海上月、月出山头、
    /// 八月相盘、光晕月、全食环、笑脸月等。Running 时月相/光晕连续变化。
    /// </summary>
    public enum HMoonStyle
    {
        Crescent = 0,      // 弯月
        FullMoon = 1,      // 满月
        PhaseCycle = 2,    // 连续月相
        MoonStar = 3,      // 星月
        Eclipse = 4,       // 月偏食
        BloodMoon = 5,     // 血月
        MoonCrater = 6,    // 陨石坑满月
        HalfMoon = 7,      // 上弦月
        Gibbous = 8,       // 盈凸月
        NewMoon = 9,       // 新月（暗盘）
        MoonCloud = 10,    // 云遮月
        HarvestMoon = 11,  // 丰收橙月
        BlueMoon = 12,     // 蓝月
        MoonSea = 13,      // 海上生明月
        MoonMountains = 14,// 月出山头
        MoonLamp = 15,     // 弯月装饰灯
        CrescentRain = 16, // 弯月星雨
        PhaseDots = 17,    // 八月相排列
        MoonGlow = 18,     // 光晕满月
        EclipseTotal = 19, // 月全食
        MoonSmile = 20,    // 笑脸月
        Ornament = 21      // 装饰弯月
    }
    /// <summary>
    /// 月亮动画控件（继承 HToolAnimBase）：22 种月亮/月相造型，Running 时月相连续变化、
    /// 光晕呼吸；Classic 为月光银白色。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("月亮动画控件：22 种月亮造型，运行时月相连续变化")]
    public class HMoon : HToolAnimBase
    {
        private HMoonStyle _style = HMoonStyle.Crescent;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.SkyBlue);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CMoon = Color.FromArgb(244, 240, 218);
        private static readonly Color CMoonLt = Color.FromArgb(255, 253, 240);
        private static readonly Color CMoonDk = Color.FromArgb(206, 200, 172);
        private static readonly Color CCrater = Color.FromArgb(186, 180, 154);
        private static readonly Color CBlood = Color.FromArgb(176, 52, 38);
        private static readonly Color CBlue = Color.FromArgb(150, 190, 230);
        private static readonly Color CStar = Color.FromArgb(255, 246, 190);
        /// <summary>月亮造型样式。</summary>
        [HCategoryLanguage("月亮"), HDisplayNameLanguage("月亮造型样式"), HDescriptionLanguage("月亮造型样式"), Browsable(true)]
        [DefaultValue(HMoonStyle.Crescent)]
        public HMoonStyle MoonStyle
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
                case HMoonStyle.FullMoon: DrawFull(g, a, CMoon, 3); break;
                case HMoonStyle.PhaseCycle: DrawPhase(g, a, Running ? Phase / (float)(Math.PI * 2) : 0.62f); break;
                case HMoonStyle.MoonStar: DrawCrescent(g, a, 0.14f); DrawStars(g, a, 12); break;
                case HMoonStyle.Eclipse: DrawEclipse(g, a); break;
                case HMoonStyle.BloodMoon: DrawFull(g, a, CBlood, 2); break;
                case HMoonStyle.MoonCrater: DrawFull(g, a, CMoon, 9); break;
                case HMoonStyle.HalfMoon: DrawPhase(g, a, 0.25f); break;
                case HMoonStyle.Gibbous: DrawPhase(g, a, 0.38f); break;
                case HMoonStyle.NewMoon: DrawNewMoon(g, a); break;
                case HMoonStyle.MoonCloud: DrawMoonCloud(g, a); break;
                case HMoonStyle.HarvestMoon: DrawHarvest(g, a, Color.FromArgb(240, 178, 80)); break;
                case HMoonStyle.BlueMoon: DrawFull(g, a, CBlue, 3); break;
                case HMoonStyle.MoonSea: DrawMoonSea(g, a); break;
                case HMoonStyle.MoonMountains: DrawMountains(g, a); break;
                case HMoonStyle.MoonLamp: DrawMoonLamp(g, a); break;
                case HMoonStyle.CrescentRain: DrawCrescent(g, a, 0.12f); DrawStarRain(g, a); break;
                case HMoonStyle.PhaseDots: DrawPhaseDots(g, a); break;
                case HMoonStyle.MoonGlow: DrawGlow(g, a); break;
                case HMoonStyle.EclipseTotal: DrawTotal(g, a); break;
                case HMoonStyle.MoonSmile: DrawSmile(g, a); break;
                case HMoonStyle.Ornament: DrawOrnament(g, a); break;
                default: DrawCrescent(g, a, 0.16f); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // 满月（带坑）
        // ----------------------------------------------------------------
        private void DrawFull(Graphics g, RectangleF a, Color color, int craters)
        {
            float r = Math.Min(a.Width, a.Height) * 0.4f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            var disc = new RectangleF(cx - r, cy - r, r * 2, r * 2);
            using (var mb = new LinearGradientBrush(disc,
                HToolPalettes.Tint(color, 0.3f), HToolPalettes.Shade(color, 0.18f), 90f))
                g.FillEllipse(mb, disc);
            DrawCraters(g, cx, cy, r, craters);
            using (var rim = new Pen(HToolPalettes.Shade(color, 0.3f), Math.Max(1f, r * 0.03f)))
                g.DrawEllipse(rim, disc);
        }
        private void DrawCraters(Graphics g, float cx, float cy, float r, int n)
        {
            if (r < 10f || n <= 0) return;
            var rnd = new Random(7);
            using (var cb = new SolidBrush(CCrater))
                for (int i = 0; i < n; i++)
                {
                    float ang = (float)(rnd.NextDouble() * Math.PI * 2);
                    float d = (float)rnd.NextDouble() * r * 0.7f;
                    float cr = r * (0.05f + (float)rnd.NextDouble() * 0.1f);
                    g.FillEllipse(cb, cx + (float)Math.Cos(ang) * d - cr,
                        cy + (float)Math.Sin(ang) * d - cr, cr * 2, cr * 2);
                }
        }
        // ----------------------------------------------------------------
        // 月相：k=0 新月，0.25 上弦，0.5 满月，0.75 下弦
        // ----------------------------------------------------------------
        private void DrawPhase(Graphics g, RectangleF a, float k)
        {
            k = ((k % 1f) + 1f) % 1f;
            float r = Math.Min(a.Width, a.Height) * 0.4f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float t = (float)Math.Cos(k * Math.PI * 2);
            float erx = Math.Max(0.001f, Math.Abs(t) * r);
            using (var path = new GraphicsPath())
            {
                if (k <= 0.5f)
                {
                    // 亮右侧：外弧顶→底经右
                    path.AddArc(cx - r, cy - r, r * 2, r * 2, 270f, 180f);
                    if (t >= 0)
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 90f, 180f);
                    else
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 90f, -180f);
                }
                else
                {
                    // 亮左侧：外弧底→顶经左
                    path.AddArc(cx - r, cy - r, r * 2, r * 2, 90f, 180f);
                    if (t < 0)
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 270f, 180f);
                    else
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 270f, -180f);
                }
                path.CloseFigure();
                var box = new RectangleF(cx - r, cy - r, r * 2, r * 2);
                using (var mb = new LinearGradientBrush(box, CMoonLt, CMoonDk, 90f))
                    g.FillPath(mb, path);
                using (var ep = new Pen(HToolPalettes.Shade(CMoonDk, 0.2f), Math.Max(1f, r * 0.02f)))
                    g.DrawPath(ep, path);
            }
            // 暗面微光
            using (var shade = new SolidBrush(Color.FromArgb(26, 90, 100, 130)))
                g.FillEllipse(shade, cx - r, cy - r, r * 2, r * 2);
            // 再画一次亮面盖回
            using (var path = new GraphicsPath())
            {
                if (k <= 0.5f)
                {
                    path.AddArc(cx - r, cy - r, r * 2, r * 2, 270f, 180f);
                    if (t >= 0)
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 90f, 180f);
                    else
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 90f, -180f);
                }
                else
                {
                    path.AddArc(cx - r, cy - r, r * 2, r * 2, 90f, 180f);
                    if (t < 0)
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 270f, 180f);
                    else
                        path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 270f, -180f);
                }
                path.CloseFigure();
                using (var mb = new LinearGradientBrush(
                    new RectangleF(cx - r, cy - r, r * 2, r * 2), CMoonLt, CMoonDk, 90f))
                    g.FillPath(mb, path);
            }
        }
        // ----------------------------------------------------------------
        // 弯月：外右半弧 + 内凹弧首尾相接显式围合（不依赖双椭圆 even-odd 挖空）
        // ----------------------------------------------------------------
        private void DrawCrescent(Graphics g, RectangleF a, float thickness)
        {
            float r = Math.Min(a.Width, a.Height) * 0.38f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float rx = Math.Max(r * 0.12f, r * (1f - thickness * 2.2f));
            var box = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            using (var path = new GraphicsPath())
            {
                // 外弧：自圆顶经东侧到底
                path.AddArc(cx - r, cy - r, r * 2f, r * 2f, 270f, 180f);
                // 内弧：自底经西侧回到顶（凸向右侧），与外弧合成右侧新月
                path.AddArc(cx - rx, cy - r, rx * 2f, r * 2f, 90f, -180f);
                path.CloseFigure();
                using (var mb = new LinearGradientBrush(box, CMoonLt, CMoonDk, 90f))
                    g.FillPath(mb, path);
                using (var ep = new Pen(HToolPalettes.Shade(CMoonDk, 0.25f), Math.Max(1f, r * 0.02f)))
                    g.DrawPath(ep, path);
            }
        }
        // 星星点缀（透明底，淡黄星）
        private void DrawStars(Graphics g, RectangleF a, int n)
        {
            var rnd = new Random(13);
            for (int i = 0; i < n; i++)
            {
                float x = a.Left + a.Width * (rnd.Next(6, 94) / 100f);
                float y = a.Top + a.Height * (rnd.Next(6, 90) / 100f);
                int al = Running ? (int)(110 + 130 * Math.Abs(Math.Sin(Phase * 2 + i))) : 200;
                using (var sb = new SolidBrush(Color.FromArgb(al, CStar)))
                    g.FillEllipse(sb, x - 1.6f, y - 1.6f, 3.2f, 3.2f);
            }
        }
        private void DrawStarRain(Graphics g, RectangleF a)
        {
            var rnd = new Random(8);
            for (int i = 0; i < 10; i++)
            {
                float t = Running ? (Phase * 0.14f + i * 0.1f) % 1f : i * 0.1f;
                float x = a.Left + a.Width * (rnd.Next(5, 95) / 100f);
                int al = (int)(200 * Math.Sin(t * Math.PI));
                using (var sb = new SolidBrush(Color.FromArgb(Math.Max(0, al), CStar)))
                    g.FillEllipse(sb, x - 1.4f, a.Top + t * a.Height - 1.4f, 2.8f, 2.8f);
            }
        }
        // ----------------------------------------------------------------
        // 月偏食
        // ----------------------------------------------------------------
        private void DrawEclipse(Graphics g, RectangleF a)
        {
            DrawFull(g, a, CMoon, 2);
            float r = Math.Min(a.Width, a.Height) * 0.4f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float off = Running ? r * (0.4f + 0.5f * (float)Math.Sin(Phase)) : r * 0.55f;
            using (var sh = new SolidBrush(Color.FromArgb(225, 26, 22, 34)))
                g.FillEllipse(sh, cx - r + off, cy - r * 1.05f, r * 2, r * 2.1f);
        }
        // ----------------------------------------------------------------
        // 新月（暗盘 + 细环）
        // ----------------------------------------------------------------
        private void DrawNewMoon(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.38f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var sh = new SolidBrush(Color.FromArgb(40, 60, 70, 90)))
                g.FillEllipse(sh, cx - r, cy - r, r * 2, r * 2);
            using (var rim = new Pen(Color.FromArgb(120, 180, 190, 210), Math.Max(1f, r * 0.03f)))
                g.DrawEllipse(rim, cx - r, cy - r, r * 2, r * 2);
            DrawStars(g, a, 10);
        }
        // 云遮月
        private void DrawMoonCloud(Graphics g, RectangleF a)
        {
            DrawFull(g, a, CMoon, 2);
            float s = Math.Min(a.Width, a.Height);
            float drift = Running ? (float)Math.Sin(Phase) * s * 0.04f : 0f;
            using (var cb = new SolidBrush(Color.FromArgb(235, 190, 196, 210)))
            {
                float cl = a.Left + a.Width * 0.28f + drift;
                float cbY = a.Bottom - s * 0.34f;
                g.FillEllipse(cb, cl, cbY, s * 0.26f, s * 0.22f);
                g.FillEllipse(cb, cl + s * 0.16f, cbY - s * 0.1f, s * 0.3f, s * 0.26f);
                g.FillEllipse(cb, cl + s * 0.38f, cbY - s * 0.02f, s * 0.24f, s * 0.2f);
                g.FillRectangle(cb, cl, cbY + s * 0.08f, s * 0.62f, s * 0.1f);
            }
        }
        // 丰收月（大圆月 + 地平线）
        private void DrawHarvest(Graphics g, RectangleF a, Color color)
        {
            float r = Math.Min(a.Width, a.Height) * 0.38f;
            float cx = a.Left + a.Width / 2f;
            float cy = a.Bottom - a.Height * 0.24f - r * 0.3f;
            var disc = new RectangleF(cx - r, cy - r, r * 2, r * 2);
            using (var mb = new LinearGradientBrush(disc, HToolPalettes.Tint(color, 0.35f),
                HToolPalettes.Shade(color, 0.2f), 90f))
                g.FillEllipse(mb, disc);
            using (var ground = new SolidBrush(Color.FromArgb(70, 52, 40)))
                g.FillRectangle(ground, a.Left, a.Bottom - a.Height * 0.24f, a.Width, a.Height * 0.24f);
        }
        // 海上月 + 倒影
        private void DrawMoonSea(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.26f;
            float cx = a.Left + a.Width / 2f;
            float seaY = a.Top + a.Height * 0.56f;
            float cy = seaY - r * 0.7f;
            using (var mb = new SolidBrush(CMoonLt))
                g.FillEllipse(mb, cx - r, cy - r, r * 2, r * 2);
            using (var sea = new LinearGradientBrush(
                new RectangleF(a.Left, seaY, a.Width, a.Bottom - seaY),
                Color.FromArgb(30, 46, 86), Color.FromArgb(14, 22, 46), 90f))
                g.FillRectangle(sea, a.Left, seaY, a.Width, a.Bottom - seaY);
            using (var refl = new Pen(Color.FromArgb(160, 230, 226, 190), Math.Max(1f, r * 0.08f)))
                for (int i = 0; i < 4; i++)
                {
                    float w = r * (1.2f - i * 0.2f);
                    float y = seaY + r * 0.3f + i * r * 0.42f;
                    float wob = Running ? (float)Math.Sin(Phase * 2 + i) * r * 0.08f : 0f;
                    g.DrawLine(refl, cx - w / 2 + wob, y, cx + w / 2 + wob, y);
                }
        }
        // 月出山头
        private void DrawMountains(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.26f;
            float cx = a.Left + a.Width * 0.62f, cy = a.Top + a.Height * 0.32f;
            using (var mb = new SolidBrush(CMoon))
                g.FillEllipse(mb, cx - r, cy - r, r * 2, r * 2);
            using (var m1 = new SolidBrush(Color.FromArgb(50, 56, 78)))
                g.FillPolygon(m1, new[]
                {
                    new PointF(a.Left, a.Bottom),
                    new PointF(a.Left + a.Width * 0.34f, a.Top + a.Height * 0.42f),
                    new PointF(a.Left + a.Width * 0.58f, a.Bottom)
                });
            using (var m2 = new SolidBrush(Color.FromArgb(34, 38, 56)))
                g.FillPolygon(m2, new[]
                {
                    new PointF(a.Left + a.Width * 0.3f, a.Bottom),
                    new PointF(a.Right - a.Width * 0.1f, a.Top + a.Height * 0.5f),
                    new PointF(a.Right, a.Top + a.Height * 0.58f),
                    new PointF(a.Right, a.Bottom)
                });
        }
        // 弯月装饰灯（挂绳 + 小星坠）
        private void DrawMoonLamp(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f;
            using (var cord = new Pen(Color.FromArgb(150, 150, 155), Math.Max(1f, a.Width * 0.015f)))
                g.DrawLine(cord, cx, a.Top, cx, a.Top + a.Height * 0.14f);
            DrawCrescent(g, new RectangleF(a.X, a.Y + a.Height * 0.08f, a.Width, a.Height * 0.84f), 0.16f);
            using (var sb = new SolidBrush(CStar))
                g.FillEllipse(sb, cx - 2f, a.Bottom - a.Height * 0.12f, 4f, 4f);
        }
        // 八月相排列
        private void DrawPhaseDots(Graphics g, RectangleF a)
        {
            int n = 8;
            bool vert = a.Height > a.Width * 1.6f;
            float d = vert
                ? Math.Min(a.Width * 0.7f, a.Height / (n + 1.4f))
                : Math.Min(a.Height * 0.62f, a.Width / (n + 1.2f));
            for (int i = 0; i < n; i++)
            {
                float k = i / 8f + 0.02f;
                float x, y;
                if (vert)
                {
                    x = a.Left + a.Width / 2f;
                    y = a.Top + d * 0.7f + i * (a.Height - d * 1.4f) / (n - 1);
                }
                else
                {
                    x = a.Left + d * 0.7f + i * (a.Width - d * 1.4f) / (n - 1);
                    y = a.Top + a.Height / 2f;
                }
                var cell = new RectangleF(x - d / 2f, y - d / 2f, d, d);
                DrawMiniPhase(g, cell, k);
            }
        }
        private void DrawMiniPhase(Graphics g, RectangleF cell, float k)
        {
            float r = Math.Min(cell.Width, cell.Height) * 0.44f;
            float cx = cell.Left + cell.Width / 2f, cy = cell.Top + cell.Height / 2f;
            float t = (float)Math.Cos(k * Math.PI * 2);
            float erx = Math.Max(0.001f, Math.Abs(t) * r);
            using (var shade = new SolidBrush(Color.FromArgb(30, 90, 100, 130)))
                g.FillEllipse(shade, cx - r, cy - r, r * 2, r * 2);
            using (var path = new GraphicsPath())
            {
                if (k <= 0.5f)
                {
                    path.AddArc(cx - r, cy - r, r * 2, r * 2, 270f, 180f);
                    path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 90f, t >= 0 ? 180f : -180f);
                }
                else
                {
                    path.AddArc(cx - r, cy - r, r * 2, r * 2, 90f, 180f);
                    path.AddArc(cx - erx, cy - r, erx * 2, r * 2, 270f, t < 0 ? 180f : -180f);
                }
                path.CloseFigure();
                using (var mb = new SolidBrush(CMoon))
                    g.FillPath(mb, path);
            }
        }
        // 光晕满月
        private void DrawGlow(Graphics g, RectangleF a)
        {
            float r0 = Math.Min(a.Width, a.Height) * 0.4f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float breath = Running ? 1f + 0.08f * (float)Math.Sin(Phase * 1.5) : 1f;
            for (int k = 4; k >= 1; k--)
                using (var halo = new SolidBrush(Color.FromArgb(12, 255, 250, 220)))
                    g.FillEllipse(halo, cx - r0 * k * 0.32f * breath, cy - r0 * k * 0.32f * breath,
                        r0 * k * 0.64f * breath, r0 * k * 0.64f * breath);
            DrawFull(g, a, CMoonLt, 2);
        }
        // 月全食（黑月 + 红光环）
        private void DrawTotal(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.38f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var ring = new SolidBrush(Color.FromArgb(120, CBlood)))
                g.FillEllipse(ring, cx - r * 1.22f, cy - r * 1.22f, r * 2.44f, r * 2.44f);
            using (var disc = new SolidBrush(Color.FromArgb(235, 38, 26, 30)))
                g.FillEllipse(disc, cx - r, cy - r, r * 2, r * 2);
            using (var rim = new Pen(Color.FromArgb(200, 200, 80, 50), r * 0.08f))
                g.DrawEllipse(rim, cx - r * 0.98f, cy - r * 0.98f, r * 1.96f, r * 1.96f);
        }
        // 笑脸月
        private void DrawSmile(Graphics g, RectangleF a)
        {
            DrawFull(g, a, CMoonLt, 1);
            float r = Math.Min(a.Width, a.Height) * 0.4f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var face = new Pen(Color.FromArgb(90, 80, 50), Math.Max(1.5f, r * 0.07f)))
            {
                g.DrawArc(face, cx - r * 0.42f, cy - r * 0.28f, r * 0.22f, r * 0.22f, 200f, 140f);
                g.DrawArc(face, cx + r * 0.2f, cy - r * 0.28f, r * 0.22f, r * 0.22f, 200f, 140f);
                g.DrawArc(face, cx - r * 0.4f, cy - r * 0.1f, r * 0.8f, r * 0.6f, 20f, 140f);
            }
            // 眨眼（Running 时右眼眨）
            if (Running && Math.Sin(Phase * 2) > 0.9)
                using (var wipe = new SolidBrush(CMoonLt))
                    g.FillEllipse(wipe, cx + r * 0.2f, cy - r * 0.3f, r * 0.22f, r * 0.24f);
        }
        // 装饰胖弯月（内挂小星）
        private void DrawOrnament(Graphics g, RectangleF a)
        {
            DrawCrescent(g, a, 0.24f);
            float r = Math.Min(a.Width, a.Height) * 0.38f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var sb = new SolidBrush(CStar))
            {
                g.FillEllipse(sb, cx - r * 0.38f, cy - r * 0.28f, r * 0.13f, r * 0.13f);
                g.FillEllipse(sb, cx - r * 0.18f, cy + r * 0.12f, r * 0.1f, r * 0.1f);
                g.FillEllipse(sb, cx - r * 0.42f, cy + r * 0.3f, r * 0.08f, r * 0.08f);
            }
        }
    }
}