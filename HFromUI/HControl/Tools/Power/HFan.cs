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
    /// 风扇样式：Classic 为方形壁式轴流风扇，其余 21 种覆盖落地扇、台扇、吊扇、
    /// 牛角扇、管道扇、屋顶风帽、散热扇、射流风机等常用外形。
    /// </summary>
    public enum HFanStyle
    {
        Classic = 0,        // 方形壁式轴流风扇
        Pedestal = 1,       // 落地扇
        Desk = 2,           // 台扇
        Ceiling = 3,        // 吊扇
        RoundVent = 4,      // 圆形排气扇
        Horn = 5,           // 牛角工业扇
        Shutter = 6,        // 百叶排气扇
        Duct = 7,           // 圆筒管道扇
        ExProof = 8,        // 防爆网罩扇
        CoolingPad = 9,     // 湿帘风机
        RoofCap = 10,       // 屋顶风帽扇
        TurbineVent = 11,   // 涡轮屋顶风帽
        MobileDrum = 12,    // 移动式圆筒扇
        TwinCooler = 13,    // 双联冷却扇
        PCCooler = 14,      // 机箱散热扇
        Turbo = 15,         // 涡轮风扇
        CarClip = 16,       // 车载夹扇
        Floor = 17,         // 趴地扇
        PoultryHorn = 18,   // 畜舍喇叭扇
        Jet = 19,           // 射流风机
        Handheld = 20,      // 手持小风扇
        Oscillating = 21    // 壁式摇头扇
    }
    /// <summary>
    /// 风扇控件（继承 HToolAnimBase）：22 种风扇外形、13 种色调，
    /// Running 时扇叶旋转、送风侧有气流波纹；支持文本标签。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("风扇控件：22 种风扇外形、13 种色调，运行时扇叶旋转并有送风波纹")]
    public class HFan : HToolAnimBase
    {
        private HFanStyle _style = HFanStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HFan() { Size = new Size(150, 140); }
        /// <summary>风扇外形样式。</summary>
        [HCategoryLanguage("风扇"), HDisplayNameLanguage("风扇外形样式"), HDescriptionLanguage("风扇外形样式"), Browsable(true)]
        [DefaultValue(HFanStyle.Classic)]
        public HFanStyle FanStyle
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
                case HFanStyle.Pedestal: DrawStand(g, a, pal, false); break;
                case HFanStyle.Desk: DrawStand(g, a, pal, true); break;
                case HFanStyle.Ceiling: DrawCeiling(g, a, pal); break;
                case HFanStyle.RoundVent: DrawRoundVent(g, a, pal, false); break;
                case HFanStyle.Horn: DrawHorn(g, a, pal); break;
                case HFanStyle.Shutter: DrawShutter(g, a, pal); break;
                case HFanStyle.Duct: DrawDuct(g, a, pal, false); break;
                case HFanStyle.ExProof: DrawRoundVent(g, a, pal, true); break;
                case HFanStyle.CoolingPad: DrawPad(g, a, pal); break;
                case HFanStyle.RoofCap: DrawRoofCap(g, a, pal); break;
                case HFanStyle.TurbineVent: DrawTurbine(g, a, pal); break;
                case HFanStyle.MobileDrum: DrawDuct(g, a, pal, true); break;
                case HFanStyle.TwinCooler: DrawTwin(g, a, pal); break;
                case HFanStyle.PCCooler: DrawPCCooler(g, a, pal); break;
                case HFanStyle.Turbo: DrawTurbo(g, a, pal); break;
                case HFanStyle.CarClip: DrawCarClip(g, a, pal); break;
                case HFanStyle.Floor: DrawFloor(g, a, pal); break;
                case HFanStyle.PoultryHorn: DrawPoultry(g, a, pal); break;
                case HFanStyle.Jet: DrawJet(g, a, pal); break;
                case HFanStyle.Handheld: DrawHandheld(g, a, pal); break;
                case HFanStyle.Oscillating: DrawOscillating(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：方形壁式轴流风扇（外框 + 格栅 + 五叶 + 送风波纹）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var frame = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.08f,
                Math.Min(a.Width, a.Height) * 0.72f, Math.Min(a.Width, a.Height) * 0.72f);
            g.Box(frame, 8f, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.6f);
            float cx = frame.Left + frame.Width / 2f, cy = frame.Top + frame.Height / 2f, r = frame.Width * 0.42f;
            DrawHead(g, cx, cy, r, pal, true);
            // 四角固定点
            foreach (var pt in new[] { new PointF(frame.Left + 7, frame.Top + 7), new PointF(frame.Right - 7, frame.Top + 7),
                new PointF(frame.Left + 7, frame.Bottom - 7), new PointF(frame.Right - 7, frame.Bottom - 7) })
                g.Bolt(pt.X, pt.Y, 2.2f, pal.Edge);
            DrawWaves(g, frame.Right + 4f, cy, r * 0.8f, pal);
        }
        // ----------------------------------------------------------------
        // 风扇头（圆环 + 扇叶 + 格栅 + 轴帽）
        // ----------------------------------------------------------------
        private void DrawHead(Graphics g, float cx, float cy, float r, HToolPalette pal, bool cage)
        {
            using (var b = new SolidBrush(Color.FromArgb(245, 246, 248)))
                g.FillEllipse(b, cx - r, cy - r, r * 2f, r * 2f);
            DrawBlades(g, cx, cy, r * 0.86f, 5, SpinAngle, Running);
            using (var ring = new Pen(Color.FromArgb(150, 154, 160), 1.1f))
            {
                g.DrawEllipse(ring, cx - r, cy - r, r * 2f, r * 2f);
                if (cage)
                {
                    g.DrawEllipse(ring, cx - r * 0.72f, cy - r * 0.72f, r * 1.44f, r * 1.44f);
                    g.DrawEllipse(ring, cx - r * 0.46f, cy - r * 0.46f, r * 0.92f, r * 0.92f);
                    for (int i = 0; i < 8; i++)
                    {
                        double an = i * Math.PI / 4.0;
                        g.DrawLine(ring, cx + (float)Math.Cos(an) * r * 0.18f, cy + (float)Math.Sin(an) * r * 0.18f,
                            cx + (float)Math.Cos(an) * r * 0.98f, cy + (float)Math.Sin(an) * r * 0.98f);
                    }
                }
            }
            g.Ell(cx, cy, r * 0.13f, r * 0.13f, pal.Accent);
        }
        private void DrawBlades(Graphics g, float cx, float cy, float r, int n, float angle, bool run)
        {
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            using (var blade = new SolidBrush(run ? Color.FromArgb(90, 96, 104) : Color.FromArgb(160, 164, 170)))
            {
                float ri = r * 0.22f;
                for (int i = 0; i < n; i++)
                {
                    double a0 = i * Math.PI * 2.0 / n;
                    using (var bp = new GraphicsPath())
                    {
                        bp.AddBezier((float)Math.Cos(a0) * ri, (float)Math.Sin(a0) * ri,
                            (float)Math.Cos(a0 + 0.25) * r * 0.5f, (float)Math.Sin(a0 + 0.25) * r * 0.5f,
                            (float)Math.Cos(a0 + 0.5) * r * 0.92f, (float)Math.Sin(a0 + 0.5) * r * 0.92f,
                            (float)Math.Cos(a0 + 0.72) * r * 0.62f, (float)Math.Sin(a0 + 0.72) * r * 0.62f);
                        bp.AddBezier((float)Math.Cos(a0 + 0.72) * r * 0.62f, (float)Math.Sin(a0 + 0.72) * r * 0.62f,
                            (float)Math.Cos(a0 + 0.4) * r * 0.4f, (float)Math.Sin(a0 + 0.4) * r * 0.4f,
                            (float)Math.Cos(a0 + 0.18) * ri, (float)Math.Sin(a0 + 0.18) * ri,
                            (float)Math.Cos(a0) * ri, (float)Math.Sin(a0) * ri);
                        bp.CloseFigure();
                        g.FillPath(blade, bp);
                    }
                }
            }
            g.Restore(st);
        }
        private void DrawWaves(Graphics g, float x, float cy, float r, HToolPalette pal)
        {
            if (!Running) return;
            using (var p = new Pen(pal.Accent, 1.8f))
            {
                for (int i = 0; i < 3; i++)
                {
                    float t = (Phase / (float)Math.PI * 0.5f + i / 3f) % 1f;
                    p.Color = Color.FromArgb(Math.Max(50, (int)(220 * (1f - t))), pal.Accent);
                    float wx = x + t * r * 0.7f;
                    g.DrawArc(p, wx, cy - r * 0.42f, 10f, r * 0.4f, -50f, 100f);
                    g.DrawArc(p, wx, cy + r * 0.02f, 10f, r * 0.4f, -50f, 100f);
                }
            }
        }
        // ----------------------------------------------------------------
        // 新式风扇
        // ----------------------------------------------------------------
        private void DrawStand(Graphics g, RectangleF a, HToolPalette pal, bool desk)
        {
            float cx = a.Left + a.Width * 0.52f;
            float hr = Math.Min(a.Width, a.Height) * (desk ? 0.26f : 0.24f);
            float hy = a.Top + (desk ? a.Height * 0.3f : a.Height * 0.34f);
            DrawHead(g, cx, hy, hr, pal, true);
            // 摇头电机盒
            g.Box(new RectangleF(cx - hr * 0.22f, hy - hr * 0.28f, hr * 0.44f, hr * 0.56f),
                3f, pal.MetalDark, pal.Edge, 1.1f);
            float poleBottom = desk ? a.Bottom - a.Height * 0.16f : a.Bottom - a.Height * 0.1f;
            g.Line(pal.MetalDark, 4f, cx, hy + hr * 0.5f, cx, poleBottom);
            // 升降锁扣
            g.Box(new RectangleF(cx - 5f, hy + hr + (poleBottom - hy - hr) * 0.55f, 10f, 7f),
                2f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1f);
            // 底座
            float by = desk ? a.Bottom - a.Height * 0.12f : a.Bottom - a.Height * 0.08f;
            g.Ell(cx, by, hr * 0.78f, hr * 0.18f, pal.MetalDark);
            if (!desk)
                g.Line(pal.MetalDark, 3f, cx, poleBottom, cx - hr * 0.5f, by - 2f);
            DrawWaves(g, cx + hr + 2f, hy, hr, pal);
        }
        private void DrawCeiling(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            // 吊杆 + 顶座
            g.Line(pal.MetalDark, 4f, cx, a.Top + 2f, cx, a.Top + a.Height * 0.32f);
            g.Box(new RectangleF(cx - 12f, a.Top, 24f, 7f), 2f, pal.MetalDark, pal.Edge, 1f);
            float cy = a.Top + a.Height * 0.42f, r = a.Width * 0.4f;
            // 三叶（无罩）
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle * 0.6f : 0f);
            using (var blade = new SolidBrush(Running ? Color.FromArgb(110, 116, 124) : HToolPalettes.Tint(pal.Main, 0.35f)))
            {
                for (int i = 0; i < 3; i++)
                {
                    g.RotateTransform(120f);
                    g.FillR(new RectangleF(r * 0.14f, -r * 0.13f, r * 0.82f, r * 0.26f), r * 0.12f,
                        Color.FromArgb(blade.Color.A, blade.Color.R, blade.Color.G, blade.Color.B));
                }
            }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.16f, r * 0.16f, pal.MetalDark);
            // 灯下吊球
            g.Ell(cx, cy + r * 0.28f, r * 0.09f, r * 0.09f, Color.FromArgb(235, 220, 170));
        }
        private void DrawRoundVent(Graphics g, RectangleF a, HToolPalette pal, bool heavy)
        {
            float cx = a.Left + a.Width * 0.46f, cy = a.Top + a.Height * 0.5f;
            float r = Math.Min(a.Width, a.Height) * 0.36f;
            using (var b = new SolidBrush(heavy ? HToolPalettes.Shade(pal.Main, 0.3f) : Color.FromArgb(244, 246, 248)))
                g.FillEllipse(b, cx - r, cy - r, r * 2f, r * 2f);
            DrawBlades(g, cx, cy, r * 0.78f, 6, SpinAngle, Running);
            using (var pen = new Pen(pal.Edge, heavy ? 2.6f : 1.6f))
            {
                g.DrawEllipse(pen, cx - r, cy - r, r * 2f, r * 2f);
                g.DrawEllipse(pen, cx - r * 0.55f, cy - r * 0.55f, r * 1.1f, r * 1.1f);
            }
            g.Ell(cx, cy, r * 0.14f, r * 0.14f, pal.Accent);
            for (int i = 0; i < 6; i++)
            {
                double an = i * Math.PI / 3.0;
                g.Bolt(cx + (float)Math.Cos(an) * r * 0.9f, cy + (float)Math.Sin(an) * r * 0.9f,
                    heavy ? 2.6f : 2f, pal.Edge);
            }
            if (heavy)
            {
                var tag = new RectangleF(cx - 10f, cy + r * 0.35f, 20f, 10f);
                g.FillR(tag, 2f, Color.FromArgb(200, 40, 40));
                HBarBase.DrawText(g, "Ex", new Font("微软雅黑", 6.5f, FontStyle.Bold), Color.White, tag,
                    ContentAlignment.MiddleCenter);
            }
            DrawWaves(g, cx + r + 2f, cy, r, pal);
        }
        private void DrawHorn(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.42f, cy = a.Top + a.Height * 0.36f;
            float r = Math.Min(a.Width, a.Height) * 0.27f;
            // 大网环 + 三片长叶
            using (var ring = new Pen(pal.MetalDark, 3.4f))
                g.DrawEllipse(ring, cx - r, cy - r, r * 2f, r * 2f);
            DrawBlades(g, cx, cy, r * 0.92f, 3, SpinAngle * 0.8f, Running);
            g.Ell(cx, cy, r * 0.15f, r * 0.15f, pal.Accent);
            // 密网罩
            using (var cage = new Pen(Color.FromArgb(160, 164, 170), 0.8f))
                for (int i = 1; i <= 3; i++)
                    g.DrawEllipse(cage, cx - r * i / 4f, cy - r * i / 4f, r * i / 2f, r * i / 2f);
            // 后电机 + 立杆 + 十字底座
            g.Motor(cx - r * 0.3f, cx + r * 0.3f, cy + r * 0.15f, r * 0.24f, pal);
            g.Line(pal.MetalDark, 5f, cx, cy + r, cx, a.Bottom - 10f);
            g.Line(pal.MetalDark, 5f, cx - r * 0.7f, a.Bottom - 10f, cx + r * 0.7f, a.Bottom - 10f);
            DrawWaves(g, cx + r + 2f, cy, r, pal);
        }
        private void DrawShutter(Graphics g, RectangleF a, HToolPalette pal)
        {
            var frame = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.1f,
                Math.Min(a.Width, a.Height) * 0.74f, Math.Min(a.Width, a.Height) * 0.74f);
            g.Box(frame, 4f, pal.MetalDark, pal.Edge, 1.8f);
            float cx = frame.Left + frame.Width / 2f, cy = frame.Top + frame.Height / 2f;
            DrawBlades(g, cx, cy, frame.Width * 0.32f, 5, SpinAngle, Running);
            g.Ell(cx, cy, frame.Width * 0.06f, frame.Width * 0.06f, pal.Accent);
            // 前面百叶（随运行微张）
            int n = 6;
            float gap = frame.Height / (n + 1);
            using (var lou = new Pen(HToolPalettes.Tint(pal.Main, 0.3f), 3.4f))
                for (int i = 1; i <= n; i++)
                {
                    float y = frame.Top + gap * i;
                    float tilt = Running ? 5f : 1.5f;
                    g.DrawLine(lou, frame.Left + 5, y, frame.Right - 5, y - tilt);
                }
        }
        private void DrawDuct(Graphics g, RectangleF a, HToolPalette pal, bool mobile)
        {
            float cy = a.Top + a.Height * (mobile ? 0.42f : 0.5f), x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.08f;
            float rh = a.Height * (mobile ? 0.28f : 0.3f);
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.FillCylH(tube, HToolPalettes.Shade(pal.Main, 0.3f), HToolPalettes.Tint(pal.Main, 0.25f));
            g.DrawR(tube, 5f, pal.Edge, 1.5f);
            DrawBlades(g, x1 + rh + 2f, cy, rh * 0.85f, 5, SpinAngle, Running);
            g.Ell(x1 + rh + 2f, cy, rh * 0.14f, rh * 0.14f, pal.Accent);
            g.Flange(new RectangleF(x1 - 7f, cy - rh - 2f, 9f, rh * 2f + 4f), pal);
            if (mobile)
            {
                float wy = a.Bottom - 10f;
                g.Ell(x1 + 18f, wy, 10f, 10f, Color.FromArgb(60, 64, 70));
                g.Ell(x2 - 18f, wy, 10f, 10f, Color.FromArgb(60, 64, 70));
                g.Line(pal.MetalDark, 4f, x1 + 18f, cy + rh, x1 + 18f, wy);
                g.Line(pal.MetalDark, 4f, x2 - 18f, cy + rh, x2 - 18f, wy);
                g.Line(pal.Edge, 4f, x2 - 6f, cy + rh * 0.6f, x2 + 10f, cy - rh * 0.9f);
            }
            else
            {
                g.Line(pal.MetalDark, 4f, x1 + 14f, cy + rh, x1 + 14f, a.Bottom - 4f);
                g.Line(pal.MetalDark, 4f, x2 - 14f, cy + rh, x2 - 14f, a.Bottom - 4f);
            }
            DrawWaves(g, x2 + 2f, cy, rh, pal);
        }
        private void DrawPad(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.1f, -a.Height * 0.12f);
            g.Box(box, 4f, HToolPalettes.Tint(pal.Main, 0.25f), pal.Edge, 1.6f);
            // 湿帘纸（蜂窝斜纹）
            var pad = RectangleF.Inflate(box, -8f, -8f);
            g.FillR(pad, 2f, Color.FromArgb(120, 150, 110));
            using (var h = new Pen(Color.FromArgb(80, 110, 70), 1.2f))
                for (float x = pad.Left; x < pad.Right; x += 6f)
                {
                    g.DrawLine(h, x, pad.Top, x + 8f, pad.Bottom);
                    g.DrawLine(h, x + 8f, pad.Top, x, pad.Bottom);
                }
            // 顶部布水管 + 下部水槽
            g.Line(Color.FromArgb(60, 90, 130), 3f, box.Left + 4f, box.Top + 3f, box.Right - 4f, box.Top + 3f);
            g.Box(new RectangleF(box.Left, box.Bottom - 6f, box.Width, 6f), 1f, Color.FromArgb(70, 100, 140), pal.Edge, 1f);
            // 侧面小风机圆
            float fx = box.Right + a.Width * 0.06f, fr = a.Height * 0.13f;
            if (fx + fr < a.Right)
            {
                DrawBlades(g, fx, box.Top + box.Height * 0.6f, fr, 5, SpinAngle, Running);
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), fx - fr, box.Top + box.Height * 0.6f - fr, fr * 2f, fr * 2f);
            }
        }
        private void DrawRoofCap(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, baseY = a.Bottom - 4f, w = a.Width * 0.36f;
            g.Box(new RectangleF(cx - w / 2f, baseY - a.Height * 0.26f, w, a.Height * 0.26f),
                3f, pal.Metal, pal.Edge, 1.3f);
            float cy = baseY - a.Height * 0.42f, r = w * 0.62f;
            DrawHead(g, cx, cy, r, pal, false);
            // 伞帽
            g.Poly(new[]
            {
                new PointF(cx - r * 1.25f, cy - r * 0.35f),
                new PointF(cx, cy - r * 1.25f),
                new PointF(cx + r * 1.25f, cy - r * 0.35f)
            }, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.5f);
            g.Flange(new RectangleF(cx - w * 0.8f, baseY - 6f, w * 1.6f, 6f), pal);
        }
        private void DrawTurbine(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, baseY = a.Bottom - 4f, w = a.Width * 0.3f;
            g.Box(new RectangleF(cx - w / 2f, baseY - a.Height * 0.22f, w, a.Height * 0.22f),
                3f, pal.Metal, pal.Edge, 1.2f);
            float cy = baseY - a.Height * 0.44f, r = w * 0.72f;
            // 涡轮球（多叶片南瓜瓣）
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var ball = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.45f)))
                g.FillEllipse(ball, -r, -r, r * 2f, r * 2f);
            using (var vein = new Pen(HToolPalettes.Shade(pal.Main, 0.25f), 1.3f))
                for (int i = 0; i < 12; i++)
                {
                    g.RotateTransform(30f);
                    g.DrawArc(vein, -r * 0.25f, -r, r * 0.5f, r * 2f, -60f, 120f);
                }
            g.Restore(st);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r, cy - r, r * 2f, r * 2f);
            g.Flange(new RectangleF(cx - w * 0.8f, baseY - 6f, w * 1.6f, 6f), pal);
        }
        private void DrawTwin(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f, r = a.Height * 0.22f;
            foreach (float k in new[] { 0.32f, 0.68f })
            {
                float cx = a.Left + a.Width * k;
                DrawHead(g, cx, cy, r, pal, true);
                g.Line(pal.MetalDark, 4f, cx, cy + r, cx, a.Bottom - 8f);
                DrawWaves(g, cx + r + 2f, cy, r * 0.8f, pal);
            }
            g.Line(pal.MetalDark, 5f, a.Left + a.Width * 0.2f, a.Bottom - 8f, a.Right - a.Width * 0.2f, a.Bottom - 8f);
        }
        private void DrawPCCooler(Graphics g, RectangleF a, HToolPalette pal)
        {
            float s = Math.Min(a.Width, a.Height) * 0.72f;
            var frame = new RectangleF((a.Width - s) / 2f + a.Left, a.Top + a.Height * 0.1f, s, s);
            g.Box(frame, 6f, Color.FromArgb(40, 44, 50), pal.Edge, 1.4f);
            float cx = frame.Left + s / 2f, cy = frame.Top + s / 2f;
            DrawBlades(g, cx, cy, s * 0.36f, 7, SpinAngle, Running);
            using (var ring = new Pen(Color.FromArgb(90, 96, 104), 1.1f))
            {
                g.DrawEllipse(ring, cx - s * 0.36f, cy - s * 0.36f, s * 0.72f, s * 0.72f);
                g.DrawEllipse(ring, cx - s * 0.24f, cy - s * 0.24f, s * 0.48f, s * 0.48f);
            }
            g.Ell(cx, cy, s * 0.07f, s * 0.07f, pal.Accent);
            // 四角螺丝孔
            foreach (var pt in new[] { new PointF(frame.Left + 7, frame.Top + 7), new PointF(frame.Right - 7, frame.Top + 7),
                new PointF(frame.Left + 7, frame.Bottom - 7), new PointF(frame.Right - 7, frame.Bottom - 7) })
                g.Bolt(pt.X, pt.Y, 2.4f, Color.FromArgb(120, 124, 130));
            if (Running)
                HBarBase.DrawText(g, "PWM", new Font("Consolas", 6.5f), Color.FromArgb(90, 200, 120),
                    new RectangleF(frame.Left + 3f, frame.Bottom - 13f, 30f, 10f), ContentAlignment.MiddleLeft);
        }
        private void DrawTurbo(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.46f;
            float r = Math.Min(a.Width, a.Height) * 0.32f;
            // 无叶涡轮：密离心叶片 + 蜗壳圈
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.3f)))
                g.FillEllipse(b, cx - r, cy - r, r * 2f, r * 2f);
            DrawBlades(g, cx, cy, r * 0.7f, 12, SpinAngle * 1.4f, Running);
            using (var pen = new Pen(pal.Edge, 1.6f))
            {
                g.DrawEllipse(pen, cx - r, cy - r, r * 2f, r * 2f);
                g.DrawEllipse(pen, cx - r * 0.45f, cy - r * 0.45f, r * 0.9f, r * 0.9f);
            }
            g.Ell(cx, cy, r * 0.12f, r * 0.12f, pal.Accent);
            // 风管出口
            g.Poly(new[]
            {
                new PointF(cx + r * 0.7f, cy - r * 0.5f), new PointF(a.Right - 6f, cy - r * 0.7f),
                new PointF(a.Right - 6f, cy - r * 0.2f), new PointF(cx + r * 0.7f, cy - r * 0.1f)
            }, pal.Metal, pal.Edge, 1.3f);
        }
        private void DrawCarClip(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.36f, r = a.Height * 0.22f;
            DrawHead(g, cx, cy, r, pal, true);
            // 鹅颈管
            using (var p = new Pen(pal.MetalDark, 3.4f))
                g.DrawArc(p, cx - r * 0.9f, cy + r * 0.5f, r * 1.4f, r * 1.1f, 200f, 140f);
            // 夹子
            g.Poly(new[]
            {
                new PointF(cx - r * 0.5f, a.Bottom - 6f), new PointF(cx + r * 0.5f, a.Bottom - 6f),
                new PointF(cx + r * 0.2f, a.Bottom - a.Height * 0.22f),
                new PointF(cx - r * 0.2f, a.Bottom - a.Height * 0.22f)
            }, pal.MetalDark, pal.Edge, 1.3f);
            DrawWaves(g, cx + r + 2f, cy, r * 0.8f, pal);
        }
        private void DrawFloor(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.56f;
            float r = Math.Min(a.Width, a.Height) * 0.34f;
            // 前倾圆筒 + 粗防倾架
            DrawHead(g, cx, cy, r, pal, true);
            g.Motor(cx - r * 0.3f, cx + r * 0.3f, cy + r * 0.2f, r * 0.22f, pal);
            using (var f = new Pen(pal.MetalDark, 4.4f))
            {
                g.DrawLine(f, cx - r * 0.7f, cy + r * 0.7f, cx - r * 1.15f, a.Bottom - 6f);
                g.DrawLine(f, cx + r * 0.7f, cy + r * 0.7f, cx + r * 1.15f, a.Bottom - 6f);
                g.DrawLine(f, cx - r * 1.15f, a.Bottom - 6f, cx + r * 1.15f, a.Bottom - 6f);
            }
            DrawWaves(g, cx + r * 0.6f, cy - r * 0.6f, r, pal);
        }
        private void DrawPoultry(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.42f, cy = a.Top + a.Height * 0.42f;
            float r = a.Height * 0.26f;
            DrawHead(g, cx, cy, r, pal, true);
            // 喇叭导风筒（向前扩张）
            g.Poly(new[]
            {
                new PointF(cx + r * 0.7f, cy - r * 0.7f), new PointF(a.Right - 4f, a.Top + a.Height * 0.12f),
                new PointF(a.Right - 4f, a.Bottom - a.Height * 0.12f), new PointF(cx + r * 0.7f, cy + r * 0.7f)
            }, HToolPalettes.Tint(pal.Main, 0.35f), pal.Edge, 1.5f);
            g.Motor(cx - r * 0.3f, cx + r * 0.3f, cy + r * 0.15f, r * 0.2f, pal);
            g.Line(pal.MetalDark, 4f, cx - r * 0.5f, cy + r, cx - r * 0.5f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 4f, cx + r * 0.5f, cy + r, cx + r * 0.5f, a.Bottom - 4f);
            DrawWaves(g, a.Right - a.Width * 0.02f, cy, r, pal);
        }
        private void DrawJet(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = a.Left + a.Width * 0.05f, x2 = a.Right - a.Width * 0.05f;
            float rh = a.Height * 0.26f;
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            g.FillCylH(tube, HToolPalettes.Shade(pal.Main, 0.35f), HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawR(tube, rh, pal.Edge, 1.6f);
            // 两端喷嘴收口
            g.Poly(new[]
            {
                new PointF(x1, cy - rh), new PointF(x1 - a.Width * 0.03f, cy - rh * 0.55f),
                new PointF(x1 - a.Width * 0.03f, cy + rh * 0.55f), new PointF(x1, cy + rh)
            }, pal.MetalDark, pal.Edge, 1.3f);
            DrawBlades(g, x1 + (x2 - x1) * 0.4f, cy, rh * 0.78f, 6, SpinAngle, Running);
            g.Ell(x1 + (x2 - x1) * 0.4f, cy, rh * 0.13f, rh * 0.13f, pal.Accent);
            // 吊耳
            g.Line(pal.MetalDark, 4f, x1 + (x2 - x1) * 0.25f, cy - rh, x1 + (x2 - x1) * 0.25f, a.Top + 4f);
            g.Line(pal.MetalDark, 4f, x1 + (x2 - x1) * 0.75f, cy - rh, x1 + (x2 - x1) * 0.75f, a.Top + 4f);
            DrawWaves(g, x2 + 2f, cy, rh * 1.2f, pal);
        }
        private void DrawHandheld(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.28f, r = a.Height * 0.2f;
            DrawHead(g, cx, cy, r, pal, true);
            // 手柄
            g.Box(new RectangleF(cx - r * 0.26f, cy + r * 0.7f, r * 0.52f, a.Height * 0.34f),
                r * 0.2f, pal.MetalDark, pal.Edge, 1.2f);
            // 开关 + USB
            g.Box(new RectangleF(cx - r * 0.3f, cy + r + a.Height * 0.16f, r * 0.6f, 6f), 2f, pal.Accent, pal.Edge, 0.8f);
            HBarBase.DrawText(g, "USB", new Font("Consolas", 6f), Color.White,
                new RectangleF(cx - r, a.Bottom - 14f, r * 2f, 10f), ContentAlignment.MiddleCenter);
            DrawWaves(g, cx + r + 2f, cy, r * 0.8f, pal);
        }
        private void DrawOscillating(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 墙面摇臂 + 圆头
            g.Line(pal.MetalDark, 4f, a.Left + a.Width * 0.14f, a.Top + a.Height * 0.5f,
                a.Left + a.Width * 0.14f, a.Top + a.Height * 0.78f);
            g.Box(new RectangleF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.76f, a.Width * 0.12f, 7f),
                2f, pal.MetalDark, pal.Edge, 1f);
            float pivot = a.Left + a.Width * 0.34f, py = a.Top + a.Height * 0.5f;
            g.Line(pal.MetalDark, 3.4f, a.Left + a.Width * 0.14f, py, pivot, py);
            float cx = a.Left + a.Width * 0.6f, cy = a.Top + a.Height * 0.42f, r = a.Height * 0.26f;
            g.Line(pal.MetalDark, 3.4f, pivot, py, cx - r * 0.2f, cy);
            DrawHead(g, cx, cy, r, pal, true);
            // 摇头范围弧
            using (var arc = new Pen(Color.FromArgb(180, 184, 190), 1.2f) { DashStyle = DashStyle.Dash })
                g.DrawArc(arc, pivot - 4f, py - 4f, 8f, 8f, -30f, 60f);
            DrawWaves(g, cx + r + 2f, cy, r, pal);
        }
    }
}