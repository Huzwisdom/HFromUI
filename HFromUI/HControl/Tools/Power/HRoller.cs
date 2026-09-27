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
    /// 托辊滚筒样式：Classic 为单根平行承载托辊，其余 21 种覆盖槽形/缓冲/梳形/螺旋托辊、
    /// 传动/改向/张紧/翼轮滚筒、包胶沟槽辊、动力与无动力辊道、输送机截面等。
    /// </summary>
    public enum HRollerStyle
    {
        Classic = 0,        // 平行承载托辊
        TroughSet = 1,      // 槽形三联托辊组
        ImpactIdler = 2,    // 缓冲托辊（橡胶圈）
        ReturnIdler = 3,    // 下平行回程托辊
        Aligning = 4,       // 调心托辊
        Garland = 5,        // 吊挂式三联托辊
        RubberDisc = 6,     // 橡胶盘清扫托辊
        Comb = 7,           // 梳形托辊
        Spiral = 8,         // 螺旋清扫托辊
        DrivePulley = 9,    // 传动滚筒
        BendPulley = 10,    // 改向滚筒
        SnubPulley = 11,    // 增面滚筒
        WingPulley = 12,    // 翼轮滚筒
        TakeUp = 13,        // 张紧滚筒
        LiveRoller = 14,    // 动力辊道
        GravityRoller = 15, // 无动力辊筒
        TaperRoller = 16,   // 锥形转弯辊
        Lagged = 17,        // 包胶滚筒
        DiamondGroove = 18, // 菱形沟槽滚筒
        ConveyorSection = 19,// 输送机截面（上下托辊组）
        ImpactBed = 20,     // 缓冲床
        BeltCleaner = 21    // 清扫器滚筒
    }
    /// <summary>
    /// 滚轮/托辊控件（继承 HToolAnimBase）：22 种托辊滚筒外形、13 种色调，
    /// Running 时辊筒标记线转动。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("托辊滚筒控件：22 种托辊滚筒外形、13 种色调，运行时辊筒转动")]
    public class HRoller : HToolAnimBase
    {
        private HRollerStyle _style = HRollerStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HRoller() { Size = new Size(170, 70); }
        /// <summary>托辊滚筒外形样式。</summary>
        [HCategoryLanguage("辊筒"), HDisplayNameLanguage("托辊滚筒外形样式"), HDescriptionLanguage("托辊滚筒外形样式"), Browsable(true)]
        [DefaultValue(HRollerStyle.Classic)]
        public HRollerStyle RollerStyle
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
                case HRollerStyle.TroughSet: DrawTrough(g, a, pal, false); break;
                case HRollerStyle.ImpactIdler: DrawTrough(g, a, pal, true); break;
                case HRollerStyle.ReturnIdler: DrawReturn(g, a, pal, false); break;
                case HRollerStyle.RubberDisc: DrawReturn(g, a, pal, true); break;
                case HRollerStyle.Aligning: DrawAligning(g, a, pal); break;
                case HRollerStyle.Garland: DrawGarland(g, a, pal); break;
                case HRollerStyle.Comb: DrawComb(g, a, pal); break;
                case HRollerStyle.Spiral: DrawSpiral(g, a, pal); break;
                case HRollerStyle.DrivePulley: DrawPulley(g, a, pal, true, false); break;
                case HRollerStyle.BendPulley:
                case HRollerStyle.SnubPulley:
                case HRollerStyle.TakeUp: DrawPulley(g, a, pal, false, false); break;
                case HRollerStyle.WingPulley: DrawPulley(g, a, pal, false, true); break;
                case HRollerStyle.LiveRoller: DrawRollerLine(g, a, pal, true); break;
                case HRollerStyle.GravityRoller: DrawRollerLine(g, a, pal, false); break;
                case HRollerStyle.TaperRoller: DrawTaper(g, a, pal); break;
                case HRollerStyle.Lagged: DrawLagged(g, a, pal, false); break;
                case HRollerStyle.DiamondGroove: DrawLagged(g, a, pal, true); break;
                case HRollerStyle.ConveyorSection: DrawSection(g, a, pal); break;
                case HRollerStyle.ImpactBed: DrawImpactBed(g, a, pal); break;
                case HRollerStyle.BeltCleaner: DrawCleaner(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：单根平行托辊
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float r = Math.Min(a.Height * 0.32f, 22f);
            float x1 = a.Left + a.Width * 0.16f, x2 = a.Right - a.Width * 0.16f;
            float cy = a.Top + a.Height * 0.46f;
            DrawStand(g, x1 - r * 0.55f, x2 + r * 0.3f, cy, a.Bottom - a.Height * 0.14f, pal);
            DrawCylinder(g, x1, x2, cy, r, pal, 3, false);
            DrawCaps(g, x1, x2, cy, r, pal);
            DrawAxle(g, x1 - r * 0.85f, x2 + r * 0.6f, cy);
        }
        /// <summary>水平圆柱辊身：金属渐变 + 环槽 + 运行高亮标记。</summary>
        private void DrawCylinder(Graphics g, float x1, float x2, float cy, float r,
            HToolPalette pal, int grooves, bool coated)
        {
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            if (coated)
            {
                using (var b = new SolidBrush(Color.FromArgb(46, 48, 54))) g.FillRectangle(b, body);
                using (var hl = new Pen(Color.FromArgb(96, 98, 104), 1.2f))
                    g.DrawLine(hl, body.Left + 2f, cy - r * 0.55f, body.Right - 2f, cy - r * 0.55f);
            }
            else
            {
                using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                    g.FillRectangle(b, body);
            }
            using (var groove = new Pen(HToolPalettes.Shade(pal.Metal, 0.25f), 1f))
                for (int i = 1; i <= grooves; i++)
                    g.DrawLine(groove, x1 + (x2 - x1) * i / (grooves + 1), body.Top + 2,
                        x1 + (x2 - x1) * i / (grooves + 1), body.Bottom - 2);
            float mark = Running ? (float)Math.Sin(Phase * 2f) * r * 0.62f : 0f;
            using (var hl = new Pen(Color.FromArgb(Running ? 180 : 90, pal.Accent), 2.4f))
                g.DrawLine(hl, body.Left + 4, cy + mark, body.Right - 4, cy + mark);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), body.X, body.Y, body.Width, body.Height);
        }
        private void DrawCaps(Graphics g, float x1, float x2, float cy, float r, HToolPalette pal)
        {
            foreach (float ex in new[] { x1, x2 })
                using (var capb = HBarBase.CylinderH(new RectangleF(ex - r * 0.32f, cy - r, r * 0.32f, r * 2f),
                    pal.MetalDark, pal.Metal))
                    g.FillRectangle(capb, ex - r * 0.32f, cy - r, r * 0.32f, r * 2f);
        }
        private void DrawAxle(Graphics g, float x1, float x2, float cy)
        {
            using (var axle = new Pen(Color.FromArgb(70, 74, 80), 3.4f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(axle, x1, cy, x2, cy);
        }
        private void DrawStand(Graphics g, float x1, float x2, float yTop, float yBot, HToolPalette pal)
        {
            using (var stand = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(stand, x1, yTop - 2f, 5f, yBot - yTop + 2f);
                g.FillRectangle(stand, x2, yTop - 2f, 5f, yBot - yTop + 2f);
                g.FillRectangle(stand, x1 - 3f, yBot - 3f, x2 - x1 + 11f, 5f);
            }
        }
        // ----------------------------------------------------------------
        // 新式托辊
        // ----------------------------------------------------------------
        private void DrawTrough(Graphics g, RectangleF a, HToolPalette pal, bool impact)
        {
            // 槽形三联：中辊水平 + 两侧辊外倾 30°
            float cy = a.Top + a.Height * 0.52f;
            float r = a.Height * 0.13f;
            float midL = a.Left + a.Width * 0.34f, midR = a.Right - a.Width * 0.34f;
            float sideW = a.Width * 0.22f;
            // 支架横梁
            g.Line(pal.MetalDark, 4f, a.Left + a.Width * 0.1f, a.Bottom - 6f,
                a.Right - a.Width * 0.1f, a.Bottom - 6f);
            // 中辊
            DrawSmallRoller(g, midL, midR, cy, r, pal, impact);
            // 侧辊（倾斜）
            foreach (int side in new[] { -1, 1 })
            {
                var st = g.Save();
                float sx = side < 0 ? midL : midR;
                g.TranslateTransform(sx, cy);
                g.RotateTransform(side * 28f);
                DrawSmallRoller(g, 0f, side < 0 ? -sideW : sideW, 0f, r, pal, impact);
                g.Restore(st);
            }
            // 中心销轴
            g.Bolt(midL + (midR - midL) / 2f, cy, 2.4f, pal.Edge);
        }
        private void DrawSmallRoller(Graphics g, float x1, float x2, float cy, float r,
            HToolPalette pal, bool impact)
        {
            var body = new RectangleF(Math.Min(x1, x2), cy - r, Math.Abs(x2 - x1), r * 2f);
            using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.1f), body.X, body.Y, body.Width, body.Height);
            if (impact)
                using (var ring = new SolidBrush(Color.FromArgb(60, 62, 68)))
                    for (float x = body.Left + 3f; x < body.Right - 2f; x += 7f)
                        g.FillRectangle(ring, x, body.Top, 3.4f, body.Height);
            float mark = Running ? (float)Math.Sin(Phase * 2f) * r * 0.5f : 0f;
            g.Line(Color.FromArgb(Running ? 180 : 90, pal.Accent), 1.8f, body.Left + 2f, cy + mark,
                body.Right - 2f, cy + mark);
        }
        private void DrawReturn(Graphics g, RectangleF a, HToolPalette pal, bool discs)
        {
            float cy = a.Top + a.Height * 0.5f, r = a.Height * 0.16f;
            float x1 = a.Left + a.Width * 0.14f, x2 = a.Right - a.Width * 0.14f;
            if (discs)
            {
                // 轴 + 间隔橡胶圆盘
                DrawAxle(g, x1 - 8f, x2 + 8f, cy);
                var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
                using (var b = new SolidBrush(Color.FromArgb(200, 202, 208))) g.FillRectangle(b, body);
                using (var rb = new SolidBrush(Color.FromArgb(54, 56, 62)))
                    for (float x = x1 + 4f; x < x2; x += 16f)
                        g.FillEllipse(rb, x, cy - r, 8f, r * 2f);
                g.DrawRectangle(new Pen(pal.Edge, 1.2f), body.X, body.Y, body.Width, body.Height);
            }
            else
            {
                DrawCylinder(g, x1, x2, cy, r, pal, 0, false);
                DrawCaps(g, x1, x2, cy, r, pal);
                DrawAxle(g, x1 - r * 0.7f, x2 + r * 0.5f, cy);
            }
            // 上吊耳
            g.Line(pal.MetalDark, 3.4f, x1 + (x2 - x1) * 0.2f, a.Top + 4f,
                x1 + (x2 - x1) * 0.2f, cy - r);
            g.Line(pal.MetalDark, 3.4f, x1 + (x2 - x1) * 0.8f, a.Top + 4f,
                x1 + (x2 - x1) * 0.8f, cy - r);
        }
        private void DrawAligning(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 调心托辊：槽形辊 + 侧边立辊 + 回转支架
            DrawTrough(g, a, pal, false);
            float r = a.Height * 0.13f;
            float cy = a.Top + a.Height * 0.52f;
            foreach (int side in new[] { -1, 1 })
            {
                float x = side < 0 ? a.Left + a.Width * 0.12f : a.Right - a.Width * 0.12f;
                var body = new RectangleF(x - r * 0.5f, cy - r * 1.7f, r, r * 1.5f);
                using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                    g.FillRectangle(b, body);
                g.DrawRectangle(new Pen(pal.Edge, 1.1f), body.X, body.Y, body.Width, body.Height);
            }
            // 回转弧
            using (var arc = new Pen(pal.MetalDark, 1.4f))
                g.DrawArc(arc, a.Width * 0.3f, a.Bottom - 12f, a.Width * 0.4f, 12f, 20f, 140f);
        }
        private void DrawGarland(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 吊挂三联：上方吊链 + 下垂三辊
            float topY = a.Top + 5f, cy = a.Top + a.Height * 0.58f, r = a.Height * 0.12f;
            float midL = a.Left + a.Width * 0.36f, midR = a.Right - a.Width * 0.36f, sw = a.Width * 0.2f;
            // 吊链（弧线）
            using (var chain = new Pen(pal.Edge, 1.6f))
            {
                g.DrawLine(chain, a.Left + a.Width * 0.16f, topY, a.Left + a.Width * 0.14f, cy - r);
                g.DrawLine(chain, a.Right - a.Width * 0.16f, topY, a.Right - a.Width * 0.14f, cy - r);
                g.DrawLine(chain, a.Left + a.Width * 0.5f, topY, a.Left + a.Width * 0.5f, cy - r * 1.2f);
            }
            DrawSmallRoller(g, midL, midR, cy, r, pal, false);
            foreach (int side in new[] { -1, 1 })
            {
                var st = g.Save();
                float sx = side < 0 ? midL : midR;
                g.TranslateTransform(sx, cy);
                g.RotateTransform(side * 22f);
                DrawSmallRoller(g, 0f, side * sw, 0f, r, pal, false);
                g.Restore(st);
            }
        }
        private void DrawComb(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f, r = a.Height * 0.16f;
            float x1 = a.Left + a.Width * 0.14f, x2 = a.Right - a.Width * 0.14f;
            DrawAxle(g, x1 - 8f, x2 + 8f, cy);
            // 梳齿片（交替上下的圆盘）
            int n = 16;
            for (int i = 0; i < n; i++)
            {
                float x = x1 + (x2 - x1) * i / (n - 1);
                using (var disk = new SolidBrush(i % 2 == 0 ? pal.Metal : HToolPalettes.Tint(pal.Metal, 0.15f)))
                    g.FillEllipse(disk, x - 3.4f, cy - r, 6.8f, r * 2f);
            }
            using (var pe = new Pen(pal.Edge, 1.1f))
                g.DrawEllipse(pe, x1 - 3.4f, cy - r, 6.8f, r * 2f);
            DrawStand(g, x1 - 8f, x2 + 3f, cy, a.Bottom - 5f, pal);
        }
        private void DrawSpiral(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f, r = a.Height * 0.17f;
            float x1 = a.Left + a.Width * 0.12f, x2 = a.Right - a.Width * 0.12f;
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            using (var b = new SolidBrush(Color.FromArgb(206, 208, 214))) g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), body.X, body.Y, body.Width, body.Height);
            // 螺旋叶片（双向 X 纹）
            float move = Running ? Phase / (float)Math.PI * 0.5f * 20f : 0f;
            using (var fl = new Pen(Color.FromArgb(80, 84, 90), 2.2f))
                for (float x = x1 - 20f + move % 20f; x < x2; x += 10f)
                {
                    g.DrawLine(fl, x, cy - r + 2f, x + 10f, cy + r - 2f);
                    g.DrawLine(fl, x, cy + r - 2f, x + 10f, cy - r + 2f);
                }
            DrawAxle(g, x1 - 8f, x2 + 8f, cy);
        }
        private void DrawPulley(Graphics g, RectangleF a, HToolPalette pal, bool drive, bool wing)
        {
            float cy = a.Top + a.Height * 0.5f, r = a.Height * 0.34f;
            float x1 = a.Left + a.Width * 0.22f, x2 = a.Right - a.Width * 0.22f;
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            if (wing)
            {
                // 翼轮：两侧圆盘 + 轴向圆钢
                using (var plate = new SolidBrush(pal.MetalDark))
                {
                    g.FillEllipse(plate, x1 - r * 0.18f, cy - r, r * 0.36f, r * 2f);
                    g.FillEllipse(plate, x2 - r * 0.18f, cy - r, r * 0.36f, r * 2f);
                }
                using (var bar = new Pen(pal.Metal, 4f))
                    for (int k = -2; k <= 2; k++)
                        g.DrawLine(bar, x1, cy + k * r * 0.34f, x2, cy + k * r * 0.34f);
            }
            else
            {
                using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                    g.FillRectangle(b, body);
                // 人字形包胶（传动滚筒）
                if (drive)
                {
                    float move = Running ? Phase / (float)Math.PI * 0.5f * (x2 - x1) : 0f;
                    using (var chevron = new Pen(Color.FromArgb(90, 94, 100), 2f))
                        for (float x = x1 - (x2 - x1) + move % 26f; x < x2; x += 26f)
                        {
                            g.DrawLine(chevron, x, cy - r * 0.8f, x + 13f, cy);
                            g.DrawLine(chevron, x + 26f, cy - r * 0.8f, x + 13f, cy);
                        }
                }
            }
            g.DrawRectangle(new Pen(pal.Edge, 1.4f), body.X, body.Y, body.Width, body.Height);
            // 端盖轮毂
            foreach (float ex in new[] { x1, x2 })
            {
                g.Ell(ex, cy, r * 0.28f, r * 0.28f, pal.Metal);
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), ex - r * 0.28f, cy - r * 0.28f, r * 0.56f, r * 0.56f);
            }
            DrawAxle(g, x1 - r * 0.45f, x2 + r * 0.45f, cy);
            if (drive)
            {
                // 轴端小联轴器标记
                g.Box(new RectangleF(x2 + r * 0.2f, cy - r * 0.22f, r * 0.5f, r * 0.44f), 1.5f,
                    pal.MetalDark, pal.Edge, 1.1f);
            }
        }
        private void DrawRollerLine(Graphics g, RectangleF a, HToolPalette pal, bool live)
        {
            // 一排等径辊筒
            float cy = a.Top + a.Height * 0.48f, r = a.Height * 0.22f;
            float x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.08f;
            int n = 5;
            float step = (x2 - x1) / n;
            g.Line(pal.MetalDark, 4f, x1 - 4f, cy + r + 4f, x2 + 4f, cy + r + 4f);
            for (int i = 0; i < n; i++)
            {
                float rx = x1 + step * (i + 0.5f);
                var body = new RectangleF(rx - step * 0.36f, cy - r, step * 0.72f, r * 2f);
                using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                    g.FillRectangle(b, body);
                g.DrawRectangle(new Pen(pal.Edge, 1.1f), body.X, body.Y, body.Width, body.Height);
                if (live)
                {
                    float mark = Running ? (float)Math.Sin(Phase * 2f + i) * r * 0.5f : 0f;
                    g.Line(Color.FromArgb(Running ? 180 : 90, pal.Accent), 1.8f,
                        body.Left + 2f, cy + mark, body.Right - 2f, cy + mark);
                }
                else
                {
                    // 无动力辊：可见轴承孔
                    g.Ell(rx, cy, 2.6f, 2.6f, pal.Edge);
                }
            }
            if (live)
                // 链条联动
                g.Line(Color.FromArgb(44, 46, 52), 2.2f, x1, cy - r - 2f, x2, cy - r - 2f);
        }
        private void DrawTaper(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 锥形转弯辊（双截锥）
            float cy = a.Top + a.Height * 0.5f;
            float x1 = a.Left + a.Width * 0.12f, x2 = a.Right - a.Width * 0.12f, mid = (x1 + x2) / 2f;
            float rOut = a.Height * 0.3f, rIn = a.Height * 0.1f;
            foreach (int s in new[] { -1, 1 })
            {
                var pts = new[]
                {
                    new PointF(s < 0 ? x1 : mid, cy - rIn),
                    new PointF(s < 0 ? mid : x2, cy - rOut),
                    new PointF(s < 0 ? mid : x2, cy + rOut),
                    new PointF(s < 0 ? x1 : mid, cy + rIn)
                };
                g.Poly(pts, HToolPalettes.Tint(pal.Metal, 0.1f), pal.Edge, 1.3f);
            }
            g.Ell(x1, cy, rIn, rIn, pal.MetalDark);
            g.Ell(x2, cy, rOut, rOut, pal.MetalDark);
            DrawAxle(g, x1 - 6f, x2 + 6f, cy);
            float mark = Running ? (float)Math.Sin(Phase * 2f) * rIn : 0f;
            g.Line(Color.FromArgb(Running ? 180 : 90, pal.Accent), 2f, x1 + 2f, cy + mark,
                x2 - 2f, cy + mark * (rOut / rIn));
        }
        private void DrawLagged(Graphics g, RectangleF a, HToolPalette pal, bool diamond)
        {
            float cy = a.Top + a.Height * 0.5f, r = a.Height * 0.3f;
            float x1 = a.Left + a.Width * 0.16f, x2 = a.Right - a.Width * 0.16f;
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            using (var b = new SolidBrush(Color.FromArgb(44, 46, 52))) g.FillRectangle(b, body);
            if (diamond)
            {
                // 菱形沟槽
                using (var groove = new Pen(Color.FromArgb(96, 98, 104), 1.3f))
                {
                    for (float x = x1; x < x2; x += 12f)
                    {
                        g.DrawLine(groove, x, body.Top + 1f, x + 12f, body.Bottom - 1f);
                        g.DrawLine(groove, x, body.Bottom - 1f, x + 12f, body.Top + 1f);
                    }
                }
            }
            else
            {
                using (var hl = new Pen(Color.FromArgb(110, 112, 118), 1.4f))
                    g.DrawLine(hl, body.Left + 2f, cy - r * 0.6f, body.Right - 2f, cy - r * 0.6f);
            }
            g.DrawRectangle(new Pen(pal.Edge, 1.4f), body.X, body.Y, body.Width, body.Height);
            DrawCaps(g, x1, x2, cy, r, pal);
            DrawAxle(g, x1 - r * 0.5f, x2 + r * 0.3f, cy);
            DrawStand(g, x1 - r * 0.5f, x2 + r * 0.05f, cy, a.Bottom - 5f, pal);
        }
        private void DrawSection(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 输送机截面视图：槽形上托辊 + 平下托辊 + 皮带轮廓 + 纵梁
            float cyU = a.Top + a.Height * 0.3f, cyD = a.Top + a.Height * 0.78f;
            float r = a.Height * 0.08f, w = a.Width * 0.66f, cx = a.Left + a.Width * 0.5f;
            // 纵梁
            g.Line(pal.MetalDark, 4.6f, cx - w / 2f + 6f, cyU - r, cx - w / 2f + 6f, cyD + r);
            g.Line(pal.MetalDark, 4.6f, cx + w / 2f - 6f, cyU - r, cx + w / 2f - 6f, cyD + r);
            // 上槽形皮带 + 三辊
            g.Line(Color.FromArgb(62, 64, 70), 4f, cx - w / 2f, cyU - r * 1.6f, cx, cyU + r * 0.5f);
            g.Line(Color.FromArgb(62, 64, 70), 4f, cx + w / 2f, cyU - r * 1.6f, cx, cyU + r * 0.5f);
            g.Line(Color.FromArgb(62, 64, 70), 4f, cx - w * 0.28f, cyU + r * 0.5f, cx + w * 0.28f, cyU + r * 0.5f);
            // 下平皮带 + 托辊
            g.Line(Color.FromArgb(62, 64, 70), 4f, cx - w * 0.4f, cyD, cx + w * 0.4f, cyD);
            g.Ell(cx - w * 0.4f, cyD, r, r, pal.Metal);
            g.Ell(cx + w * 0.4f, cyD, r, r, pal.Metal);
        }
        private void DrawImpactBed(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 缓冲床：一排弧形缓冲条 + 支撑
            float cy = a.Top + a.Height * 0.42f;
            float x1 = a.Left + a.Width * 0.1f, x2 = a.Right - a.Width * 0.1f;
            using (var frame = new Pen(pal.MetalDark, 4f))
            {
                g.DrawLine(frame, x1, a.Bottom - 6f, x2, a.Bottom - 6f);
                g.DrawLine(frame, x1, cy + a.Height * 0.18f, x2, cy + a.Height * 0.18f);
            }
            int n = 7;
            for (int i = 0; i < n; i++)
            {
                float x = x1 + (x2 - x1) * i / (n - 1);
                float d = Math.Abs(i - (n - 1) / 2f) / (n / 2f);
                float top = cy + d * a.Height * 0.16f;
                var bar = new RectangleF(x - 6f, top, 12f, a.Bottom - 8f - top);
                g.FillR(bar, 3f, Color.FromArgb(58, 60, 66));
                g.DrawR(bar, 3f, pal.Edge, 1f);
            }
            // 上方皮带
            g.Line(Color.FromArgb(72, 74, 80), 4.4f, x1 - 8f, cy - 2f, x2 + 8f, cy - 2f);
        }
        private void DrawCleaner(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 清扫器：滚筒 + 弹簧张紧刮刀臂
            float cy = a.Top + a.Height * 0.42f, r = a.Height * 0.26f;
            float x1 = a.Left + a.Width * 0.24f, x2 = a.Right - a.Width * 0.16f;
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), body.X, body.Y, body.Width, body.Height);
            // 刮板（聚氨酯刀头）
            g.Poly(new[]
            {
                new PointF(x1 - 2f, cy + r * 0.2f), new PointF(x1 - 12f, cy + r + 6f),
                new PointF(x1 - 6f, cy + r + 8f), new PointF(x1 + 2f, cy + r * 0.5f)
            }, Color.FromArgb(180, 130, 60), pal.Edge, 1.2f);
            // 刮刀臂 + 弹簧
            g.Line(pal.MetalDark, 3f, a.Left + 8f, a.Bottom - 8f, x1 - 8f, cy + r + 6f);
            float sx = a.Left + a.Width * 0.16f, sy = a.Top + a.Height * 0.66f;
            using (var sp = new Pen(pal.Edge, 1.4f))
                for (int k = 0; k < 4; k++)
                    g.DrawArc(sp, sx - 4f, sy + k * 5f, 8f, 6f, 0f, 180f);
        }
    }
}