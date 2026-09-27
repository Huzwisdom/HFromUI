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
    /// 爱心动画外形：22 种爱心造型——心跳单心、双心、飘心、心电脉冲、破碎心、
    /// 彩虹心、星闪、迸发、心链、波纹、丘比特之箭、心锁、气球、霓虹、火焰心等。
    /// Running 时心跳/漂浮/发光动画运行。
    /// </summary>
    public enum HHeartStyle
    {
        Classic = 0,       // 心跳单心
        Twin = 1,          // 双心
        Floating = 2,      // 飘心上升
        PulseLine = 3,     // 心电图脉冲
        Broken = 4,        // 破碎心
        Rainbow = 5,       // 彩虹心
        Outline = 6,       // 描边心
        Sparkle = 7,       // 星闪心
        Burst = 8,         // 迸发心
        HeartChain = 9,    // 三连心
        PulseRings = 10,   // 心跳波纹
        Cupid = 11,        // 丘比特箭穿心
        HeartLock = 12,    // 心锁
        Balloon = 13,      // 心形气球
        Badge = 14,        // 心形徽章
        Neon = 15,         // 霓虹发光心
        Stack = 16,        // 堆叠心
        HeartDrop = 17,    // 滴落心
        HeartFire = 18,    // 火焰心
        HeartCloud = 19,   // 云朵飘心
        HeartCrown = 20,   // 加冕心
        HeartOrbit = 21    // 环绕心
    }
    /// <summary>
    /// 爱心动画控件（继承 HToolAnimBase）：22 种爱心造型，Running 时心跳缩放、飘心上升、
    /// 波纹扩散等动画运行；Classic 为红色爱心，其余色调随主题。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("爱心动画控件：22 种爱心造型，运行时心跳/漂浮动画")]
    public class HHeart : HToolAnimBase
    {
        private HHeartStyle _style = HHeartStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Rose);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CHeart = Color.FromArgb(232, 68, 107);
        private static readonly Color CHeartLt = Color.FromArgb(255, 128, 158);
        private static readonly Color CHeartDk = Color.FromArgb(168, 30, 66);
        private static readonly Color CFire1 = Color.FromArgb(255, 180, 40);
        private static readonly Color CFire2 = Color.FromArgb(255, 96, 30);
        /// <summary>爱心造型样式。</summary>
        [HCategoryLanguage("心形"), HDisplayNameLanguage("爱心造型样式"), HDescriptionLanguage("爱心造型样式"), Browsable(true)]
        [DefaultValue(HHeartStyle.Classic)]
        public HHeartStyle HeartStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        private Color Main => _style == HHeartStyle.Rainbow ? CHeart : Palette.Main;
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            if (a.Width <= 3f || a.Height <= 3f) { PaintPlacedText(g); return; }
            switch (_style)
            {
                case HHeartStyle.Twin: DrawTwin(g, a); break;
                case HHeartStyle.Floating: DrawFloating(g, a); break;
                case HHeartStyle.PulseLine: DrawPulseLine(g, a); break;
                case HHeartStyle.Broken: DrawBroken(g, a); break;
                case HHeartStyle.Rainbow: DrawRainbow(g, a); break;
                case HHeartStyle.Outline: DrawOutline(g, a); break;
                case HHeartStyle.Sparkle: DrawSparkle(g, a); break;
                case HHeartStyle.Burst: DrawBurst(g, a); break;
                case HHeartStyle.HeartChain: DrawChain(g, a); break;
                case HHeartStyle.PulseRings: DrawPulseRings(g, a); break;
                case HHeartStyle.Cupid: DrawCupid(g, a); break;
                case HHeartStyle.HeartLock: DrawLock(g, a); break;
                case HHeartStyle.Balloon: DrawBalloon(g, a); break;
                case HHeartStyle.Badge: DrawBadge(g, a); break;
                case HHeartStyle.Neon: DrawNeon(g, a); break;
                case HHeartStyle.Stack: DrawStack(g, a); break;
                case HHeartStyle.HeartDrop: DrawDrop(g, a); break;
                case HHeartStyle.HeartFire: DrawFire(g, a); break;
                case HHeartStyle.HeartCloud: DrawCloud(g, a); break;
                case HHeartStyle.HeartCrown: DrawCrown(g, a); break;
                case HHeartStyle.HeartOrbit: DrawOrbit(g, a); break;
                default: DrawSingle(g, a, HeartScale()); break;
            }
            PaintPlacedText(g);
        }
        // 心跳缩放
        private float HeartScale()
        {
            if (!Running) return 1f;
            float beat = (float)Math.Sin(Phase * 2.2);
            return 1f + 0.09f * Math.Max(0f, beat * beat);
        }
        // ----------------------------------------------------------------
        // 心形路径（s 为基准尺寸）
        // ----------------------------------------------------------------
        private static GraphicsPath HeartPathF(float cx, float cy, float s)
        {
            var gp = new GraphicsPath();
            gp.AddBezier(cx, cy + s * 0.46f,
                cx - s * 0.56f, cy + s * 0.08f,
                cx - s * 0.54f, cy - s * 0.44f,
                cx, cy - s * 0.2f);
            gp.AddBezier(cx, cy - s * 0.2f,
                cx + s * 0.54f, cy - s * 0.44f,
                cx + s * 0.56f, cy + s * 0.08f,
                cx, cy + s * 0.46f);
            gp.CloseFigure();
            return gp;
        }
        // 单心（渐变 + 高光）
        private void DrawSingle(Graphics g, RectangleF a, float scale)
        {
            float s = Math.Min(a.Width, a.Height) * 0.82f * scale;
            if (s <= 2f) return;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            DrawHeartAt(g, cx, cy, s, Main, true);
        }
        private void DrawHeartAt(Graphics g, float cx, float cy, float s, Color color, bool highlight)
        {
            if (s <= 1.5f) return;
            using (var path = HeartPathF(cx, cy, s))
            using (var hb = new LinearGradientBrush(
                new RectangleF(cx - s * 0.56f, cy - s * 0.46f, s * 1.12f, s * 0.96f),
                HToolPalettes.Tint(color, 0.35f), HToolPalettes.Shade(color, 0.22f), 90f))
                g.FillPath(hb, path);
            if (highlight && s > 12f)
            {
                using (var path2 = HeartPathF(cx, cy, s))
                using (var hi = new SolidBrush(Color.FromArgb(70, Color.White)))
                {
                    var old = g.Clip;
                    g.SetClip(path2);
                    g.FillEllipse(hi, cx - s * 0.34f, cy - s * 0.36f, s * 0.26f, s * 0.2f);
                    g.Clip = old;
                }
            }
        }
        // ----------------------------------------------------------------
        private void DrawTwin(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.5f * HeartScale();
            DrawHeartAt(g, a.Left + a.Width * 0.42f, a.Top + a.Height * 0.56f, s,
                HToolPalettes.Tint(Main, 0.25f), true);
            DrawHeartAt(g, a.Left + a.Width * 0.62f, a.Top + a.Height * 0.42f, s * 0.82f, Main, true);
        }
        private void DrawFloating(Graphics g, RectangleF a)
        {
            var rnd = new Random(9);
            for (int i = 0; i < 5; i++)
            {
                float px = a.Left + a.Width * (0.16f + rnd.Next(0, 70) / 100f);
                float t = Running ? (Phase * 0.11f + i * 0.2f) % 1f : (i * 0.2f);
                float py = a.Bottom - t * a.Height * 0.92f;
                float s = Math.Min(a.Width, a.Height) * (0.14f + i % 3 * 0.05f);
                int alpha = (int)(120 + 100 * Math.Sin(t * Math.PI));
                var old = g.Save();
                using (var path = HeartPathF(px, py, s))
                using (var hb = new SolidBrush(Color.FromArgb(Math.Max(40, alpha), Main)))
                    g.FillPath(hb, path);
                g.Restore(old);
            }
        }
        private void DrawPulseLine(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.62f;
            float s = Math.Min(a.Width, a.Height) * 0.34f;
            DrawHeartAt(g, a.Left + a.Width * 0.74f, cy - s * 0.1f, s * HeartScale(), Main, false);
            // 心电折线（随 Phase 平移脉冲位置）
            float shift = Running ? (Phase / (float)(Math.PI * 2)) * a.Width * 0.5f : a.Width * 0.2f;
            using (var line = new Pen(HToolPalettes.Tint(Main, 0.2f), Math.Max(1.5f, a.Width * 0.018f)))
            {
                float x0 = a.Left + a.Width * 0.08f;
                float x1 = a.Left + a.Width * 0.62f;
                float pulse = (x0 + shift) % (x1 - x0 + 20f) + x0;
                g.DrawLine(line, x0, cy, pulse - 18f, cy);
                g.DrawLines(line, new[]
                {
                    new PointF(pulse - 18f, cy),
                    new PointF(pulse - 8f, cy),
                    new PointF(pulse - 2f, cy - a.Height * 0.2f),
                    new PointF(pulse + 6f, cy + a.Height * 0.26f),
                    new PointF(pulse + 12f, cy - a.Height * 0.08f),
                    new PointF(pulse + 20f, cy),
                    new PointF(x1, cy)
                });
            }
        }
        private void DrawBroken(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.82f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            DrawHeartAt(g, cx, cy, s, Main, false);
            // 锯齿裂纹（黄色/白色）
            using (var crack = new Pen(Color.FromArgb(245, 240, 220), Math.Max(1.5f, s * 0.03f)))
                g.DrawLines(crack, new[]
                {
                    new PointF(cx - s * 0.06f, cy - s * 0.44f),
                    new PointF(cx + s * 0.08f, cy - s * 0.18f),
                    new PointF(cx - s * 0.1f, cy - s * 0.02f),
                    new PointF(cx + s * 0.12f, cy + s * 0.16f),
                    new PointF(cx - s * 0.02f, cy + s * 0.46f)
                });
        }
        private void DrawRainbow(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.84f * HeartScale();
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            Color[] rainbow =
            {
                Color.FromArgb(238, 70, 70), Color.FromArgb(255, 150, 40), Color.FromArgb(250, 214, 60),
                Color.FromArgb(70, 190, 90), Color.FromArgb(60, 150, 230), Color.FromArgb(90, 100, 220),
                Color.FromArgb(160, 90, 200)
            };
            using (var path = HeartPathF(cx, cy, s))
            {
                var old = g.Clip;
                g.SetClip(path);
                float top = cy - s * 0.46f, bandH = s * 0.98f / rainbow.Length;
                for (int i = 0; i < rainbow.Length; i++)
                    using (var rb = new SolidBrush(rainbow[i]))
                        g.FillRectangle(rb, cx - s * 0.6f, top + i * bandH, s * 1.2f, bandH + 1f);
                g.Clip = old;
                using (var edge = new Pen(HToolPalettes.Shade(CHeartDk, 0.1f), Math.Max(1f, s * 0.02f)))
                    g.DrawPath(edge, path);
            }
        }
        private void DrawOutline(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.8f * HeartScale();
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var path = HeartPathF(cx, cy, s))
            using (var pen = new Pen(Main, Math.Max(2f, s * 0.1f))
            { StartCap = LineCap.Round, LineJoin = LineJoin.Round })
                g.DrawPath(pen, path);
        }
        private void DrawSparkle(Graphics g, RectangleF a)
        {
            DrawSingle(g, a, HeartScale());
            float s = Math.Min(a.Width, a.Height);
            DrawFourStar(g, a.Left + a.Width * 0.2f, a.Top + a.Height * 0.2f, s * 0.14f);
            DrawFourStar(g, a.Right - a.Width * 0.18f, a.Top + a.Height * 0.3f, s * 0.1f);
            DrawFourStar(g, a.Left + a.Width * 0.26f, a.Bottom - a.Height * 0.2f, s * 0.08f);
        }
        private void DrawFourStar(Graphics g, float cx, float cy, float r)
        {
            if (r < 1.5f) return;
            float tw = Running ? 0.7f + 0.3f * (float)Math.Sin(Phase * 2.5 + cx) : 1f;
            using (var sb = new SolidBrush(Color.FromArgb((int)(200 * tw), 255, 235, 130)))
                g.FillPolygon(sb, new[]
                {
                    new PointF(cx, cy - r), new PointF(cx + r * 0.26f, cy - r * 0.26f),
                    new PointF(cx + r, cy), new PointF(cx + r * 0.26f, cy + r * 0.26f),
                    new PointF(cx, cy + r), new PointF(cx - r * 0.26f, cy + r * 0.26f),
                    new PointF(cx - r, cy), new PointF(cx - r * 0.26f, cy - r * 0.26f)
                });
        }
        private void DrawBurst(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float s = Math.Min(a.Width, a.Height);
            for (int i = 0; i < 8; i++)
            {
                double ang = i * Math.PI / 4;
                float dist = Running ? s * (0.28f + 0.1f * (float)Math.Sin(Phase * 2 + i)) : s * 0.32f;
                DrawHeartAt(g, cx + (float)Math.Cos(ang) * dist, cy + (float)Math.Sin(ang) * dist,
                    s * 0.14f, HToolPalettes.Tint(Main, (i % 3) * 0.12f), false);
            }
            DrawHeartAt(g, cx, cy, s * 0.42f * HeartScale(), Main, true);
        }
        private void DrawChain(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Height * 0.5f, a.Width * 0.26f);
            float y = a.Top + a.Height * 0.52f;
            DrawHeartAt(g, a.Left + a.Width * 0.22f, y, s, HToolPalettes.Tint(Main, 0.2f), false);
            DrawHeartAt(g, a.Left + a.Width * 0.5f, y - s * 0.12f, s * 1.15f * HeartScale(), Main, true);
            DrawHeartAt(g, a.Left + a.Width * 0.78f, y, s, HToolPalettes.Shade(Main, 0.15f), false);
        }
        private void DrawPulseRings(Graphics g, RectangleF a)
        {
            DrawSingle(g, a, 1f);
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float s0 = Math.Min(a.Width, a.Height) * 0.5f;
            for (int k = 0; k < 2; k++)
            {
                float t = Running ? (Phase / (float)(Math.PI * 2) + k * 0.5f) % 1f : k * 0.5f;
                using (var rp = new Pen(Color.FromArgb((int)(180 * (1 - t)), Main), Math.Max(1f, s0 * 0.02f)))
                    g.DrawEllipse(rp, cx - s0 * 0.5f - t * s0, cy - s0 * 0.45f - t * s0 * 0.8f,
                        s0 + t * s0 * 2f, s0 * 0.9f + t * s0 * 1.6f);
            }
        }
        private void DrawCupid(Graphics g, RectangleF a)
        {
            DrawSingle(g, a, HeartScale());
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float s = Math.Min(a.Width, a.Height);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(-32f);
            using (var shaft = new Pen(Color.FromArgb(120, 78, 44), Math.Max(1.5f, s * 0.018f)))
                g.DrawLine(shaft, -s * 0.62f, 0, s * 0.62f, 0);
            // 箭头
            using (var head = new SolidBrush(Color.FromArgb(120, 78, 44)))
                g.FillPolygon(head, new[]
                {
                    new PointF(s * 0.62f, 0), new PointF(s * 0.5f, -s * 0.06f),
                    new PointF(s * 0.5f, s * 0.06f)
                });
            // 尾羽
            using (var feather = new Pen(Color.FromArgb(200, 90, 110), Math.Max(1f, s * 0.014f)))
            {
                g.DrawLine(feather, -s * 0.62f, 0, -s * 0.72f, -s * 0.07f);
                g.DrawLine(feather, -s * 0.62f, 0, -s * 0.72f, s * 0.07f);
            }
            g.Restore(st);
        }
        private void DrawLock(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.72f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.56f;
            DrawHeartAt(g, cx, cy, s, Main, true);
            // 锁梁 + 锁孔
            using (var shackle = new Pen(Color.FromArgb(235, 210, 90), Math.Max(2f, s * 0.07f)))
                g.DrawArc(shackle, cx - s * 0.22f, cy - s * 0.42f, s * 0.44f, s * 0.4f, 180f, 180f);
            g.FillEllipse(Brushes.Gold, cx - s * 0.07f, cy - s * 0.02f, s * 0.14f, s * 0.14f);
            g.FillRectangle(Brushes.Gold, cx - s * 0.035f, cy + s * 0.08f, s * 0.07f, s * 0.12f);
        }
        private void DrawBalloon(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.62f * HeartScale();
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.36f;
            DrawHeartAt(g, cx, cy, s, Main, true);
            // 气球线（波浪）
            using (var str = new Pen(Color.FromArgb(150, 150, 155), Math.Max(1f, s * 0.015f)))
                g.DrawBezier(str, cx, cy + s * 0.48f,
                    cx - s * 0.2f, cy + s * 0.62f, cx + s * 0.24f, cy + s * 0.8f,
                    cx, a.Bottom - 2f);
        }
        private void DrawBadge(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float d = Math.Min(a.Width, a.Height) * 0.92f;
            using (var disc = new LinearGradientBrush(
                new RectangleF(cx - d / 2f, cy - d / 2f, d, d),
                HToolPalettes.Tint(Main, 0.15f), HToolPalettes.Shade(Main, 0.35f), 45f))
                g.FillEllipse(disc, cx - d / 2f, cy - d / 2f, d, d);
            using (var rim = new Pen(Color.FromArgb(235, 220, 120), d * 0.05f))
                g.DrawEllipse(rim, cx - d * 0.44f, cy - d * 0.44f, d * 0.88f, d * 0.88f);
            DrawHeartAt(g, cx, cy, d * 0.52f * HeartScale(), Color.White, false);
        }
        private void DrawNeon(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height) * 0.78f * HeartScale();
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            Color neon = Color.FromArgb(255, 90, 150);
            for (int k = 4; k >= 1; k--)
                using (var path = HeartPathF(cx, cy, s))
                using (var glow = new Pen(Color.FromArgb((int)(14 + k * 10), neon), s * (0.04f + k * 0.03f))
                { StartCap = LineCap.Round, LineJoin = LineJoin.Round })
                    g.DrawPath(glow, path);
            using (var path = HeartPathF(cx, cy, s))
            using (var core = new Pen(Color.White, Math.Max(1.5f, s * 0.03f)))
                g.DrawPath(core, path);
        }
        private void DrawStack(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height);
            DrawHeartAt(g, a.Left + a.Width * 0.38f, a.Top + a.Height * 0.62f, s * 0.42f,
                HToolPalettes.Shade(Main, 0.25f), false);
            DrawHeartAt(g, a.Left + a.Width * 0.62f, a.Top + a.Height * 0.5f, s * 0.48f,
                HToolPalettes.Tint(Main, 0.1f), false);
            DrawHeartAt(g, a.Left + a.Width * 0.5f, a.Top + a.Height * 0.36f, s * 0.5f * HeartScale(), Main, true);
        }
        private void DrawDrop(Graphics g, RectangleF a)
        {
            DrawSingle(g, a, HeartScale());
            float cx = a.Left + a.Width / 2f;
            float s = Math.Min(a.Width, a.Height);
            float len = Running ? s * (0.14f + 0.1f * ((Phase * 0.2f) % 1f)) : s * 0.16f;
            using (var drop = new SolidBrush(HToolPalettes.Tint(Main, 0.3f)))
                g.FillEllipse(drop, cx - s * 0.04f, a.Top + a.Height * 0.66f + len, s * 0.08f, s * 0.12f);
        }
        private void DrawFire(Graphics g, RectangleF a)
        {
            DrawSingle(g, a, HeartScale());
            float cx = a.Left + a.Width / 2f;
            float s = Math.Min(a.Width, a.Height);
            float flick = Running ? 1f + 0.12f * (float)Math.Sin(Phase * 3) : 1f;
            using (var f1 = new SolidBrush(Color.FromArgb(210, CFire2)))
                g.FillPolygon(f1, new[]
                {
                    new PointF(cx, a.Top - s * 0.02f * flick),
                    new PointF(cx - s * 0.14f, a.Top + s * 0.22f),
                    new PointF(cx, a.Top + s * 0.12f),
                    new PointF(cx + s * 0.13f, a.Top + s * 0.22f)
                });
            using (var f2 = new SolidBrush(Color.FromArgb(220, CFire1)))
                g.FillPolygon(f2, new[]
                {
                    new PointF(cx, a.Top + s * 0.06f * flick),
                    new PointF(cx - s * 0.08f, a.Top + s * 0.2f),
                    new PointF(cx, a.Top + s * 0.14f),
                    new PointF(cx + s * 0.07f, a.Top + s * 0.2f)
                });
        }
        private void DrawCloud(Graphics g, RectangleF a)
        {
            float s = Math.Min(a.Width, a.Height);
            // 云
            using (var cb = new SolidBrush(Color.FromArgb(225, 232, 240)))
            {
                g.FillEllipse(cb, a.Left + s * 0.12f, a.Bottom - s * 0.42f, s * 0.28f, s * 0.24f);
                g.FillEllipse(cb, a.Left + s * 0.3f, a.Bottom - s * 0.52f, s * 0.34f, s * 0.3f);
                g.FillEllipse(cb, a.Left + s * 0.52f, a.Bottom - s * 0.46f, s * 0.28f, s * 0.26f);
                g.FillRectangle(cb, a.Left + s * 0.14f, a.Bottom - s * 0.3f, s * 0.62f, s * 0.12f);
            }
            for (int i = 0; i < 3; i++)
            {
                float t = Running ? (Phase * 0.1f + i * 0.33f) % 1f : i * 0.33f;
                DrawHeartAt(g, a.Left + a.Width * (0.3f + i * 0.2f),
                    a.Bottom - s * 0.46f - t * s * 0.5f, s * 0.13f, Main, false);
            }
        }
        private void DrawCrown(Graphics g, RectangleF a)
        {
            DrawSingle(g, a, HeartScale());
            float s = Math.Min(a.Width, a.Height);
            for (int i = -1; i <= 1; i++)
                DrawFourStar(g, a.Left + a.Width / 2f + i * s * 0.2f,
                    a.Top + s * 0.06f, s * (0.07f + (i == 0 ? 0.03f : 0f)));
        }
        private void DrawOrbit(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float s = Math.Min(a.Width, a.Height);
            DrawHeartAt(g, cx, cy, s * 0.42f, Main, true);
            using (var orbit = new Pen(Color.FromArgb(90, Main), Math.Max(1f, s * 0.012f)))
                g.DrawEllipse(orbit, cx - s * 0.44f, cy - s * 0.22f, s * 0.88f, s * 0.44f);
            float ang = Running ? Phase : 0f;
            float px = cx + (float)Math.Cos(ang) * s * 0.44f;
            float py = cy + (float)Math.Sin(ang) * s * 0.22f;
            DrawHeartAt(g, px, py, s * 0.15f, HToolPalettes.Tint(Main, 0.3f), false);
        }
    }
}