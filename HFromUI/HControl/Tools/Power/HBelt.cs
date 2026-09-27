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
    /// 输送设备样式：Classic 为平皮带输送机，其余 21 种覆盖槽形带、挡边带、圆管带、
    /// 转弯/爬坡/伸缩皮带、链板、网带、辊筒、螺旋、刮板、移动升降带、带传动等。
    /// </summary>
    public enum HBeltStyle
    {
        Classic = 0,        // 平皮带输送机
        Trough = 1,         // 槽形托辊皮带
        Cleated = 2,        // 挡边横隔皮带
        Herringbone = 3,    // 人字花纹皮带
        TubeBelt = 4,       // 圆管带式
        Curve = 5,          // 转弯皮带
        Incline = 6,        // 爬坡皮带
        Telescopic = 7,     // 可伸缩皮带
        Apron = 8,          // 链板给料机
        Mesh = 9,           // 网带输送机
        RollerTable = 10,   // 辊筒输送机
        Screw = 11,         // 螺旋输送机
        Vacuum = 12,        // 真空吸风皮带
        Magnetic = 13,      // 磁性皮带
        Sorting = 14,       // 分拣摆轮皮带
        VDrive = 15,        // 窄 V 带传动
        Timing = 16,        // 同步齿形带
        PolyV = 17,         // 多楔带传动
        DriveSection = 18,  // 电机减速器驱动端
        DragFlight = 19,    // 刮板输送机
        SlatChain = 20,     // 塑料链板线
        MobileBelt = 21     // 移动升降皮带
    }
    /// <summary>
    /// 皮带输送控件（继承 HToolAnimBase）：22 种输送设备外形、13 种色调，
    /// Running（运料）时辊轮/纹理转动、骨料移动；ShowMaterial 控制骨料显示。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("输送设备控件：22 种输送外形、13 种色调，运料时皮带转动骨料移动")]
    public class HBelt : HToolAnimBase
    {
        private bool _showMaterial = true;
        private HBeltStyle _style = HBeltStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HBelt() { Size = new Size(190, 104); }
        /// <summary>输送设备外形样式。</summary>
        [HCategoryLanguage("传送带"), HDisplayNameLanguage("输送设备外形样式"), HDescriptionLanguage("输送设备外形样式"), Browsable(true)]
        [DefaultValue(HBeltStyle.Classic)]
        public HBeltStyle BeltStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>是否在皮带上绘制骨料。</summary>
        [HCategoryLanguage("传送带"), HDisplayNameLanguage("是否在皮带上绘制骨料"), HDescriptionLanguage("是否在皮带上绘制骨料"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowMaterial
        {
            get => _showMaterial;
            set { _showMaterial = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HBeltStyle.Trough: DrawTrough(g, a, pal, false); break;
                case HBeltStyle.Cleated: DrawTrough(g, a, pal, true); break;
                case HBeltStyle.Herringbone: DrawHerring(g, a, pal); break;
                case HBeltStyle.TubeBelt: DrawTube(g, a, pal); break;
                case HBeltStyle.Curve: DrawCurve(g, a, pal); break;
                case HBeltStyle.Incline: DrawIncline(g, a, pal, false); break;
                case HBeltStyle.MobileBelt: DrawIncline(g, a, pal, true); break;
                case HBeltStyle.Telescopic: DrawTelescopic(g, a, pal); break;
                case HBeltStyle.Apron: DrawApron(g, a, pal, false); break;
                case HBeltStyle.SlatChain: DrawApron(g, a, pal, true); break;
                case HBeltStyle.Mesh: DrawMesh(g, a, pal); break;
                case HBeltStyle.RollerTable: DrawRollerTable(g, a, pal); break;
                case HBeltStyle.Screw: DrawScrew(g, a, pal); break;
                case HBeltStyle.Vacuum: DrawVacuum(g, a, pal); break;
                case HBeltStyle.Magnetic: DrawMagnetic(g, a, pal); break;
                case HBeltStyle.Sorting: DrawSorting(g, a, pal); break;
                case HBeltStyle.VDrive: DrawBeltDrive(g, a, pal, false, false); break;
                case HBeltStyle.Timing: DrawBeltDrive(g, a, pal, true, false); break;
                case HBeltStyle.PolyV: DrawBeltDrive(g, a, pal, false, true); break;
                case HBeltStyle.DriveSection: DrawDriveSection(g, a, pal); break;
                case HBeltStyle.DragFlight: DrawDrag(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：平皮带
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f;
            float pr = Math.Min(15f, a.Height * 0.2f);
            float lx = a.Left + a.Width * 0.16f, rx = a.Right - a.Width * 0.16f;
            float top = cy - pr, bot = cy + pr;
            using (var frame = new Pen(pal.MetalDark, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(frame, lx, cy, lx, a.Bottom - a.Height * 0.14f);
                g.DrawLine(frame, rx, cy, rx, a.Bottom - a.Height * 0.14f);
                g.DrawLine(frame, lx - pr, a.Bottom - a.Height * 0.14f, rx + pr, a.Bottom - a.Height * 0.14f);
            }
            DrawBeltLoop(g, lx, rx, cy, pr, pal, 24f, false);
            DrawPulley(g, lx, cy, pr * 0.78f, pal);
            DrawPulley(g, rx, cy, pr * 0.78f, pal);
            DrawStones(g, lx, rx, top, pal);
        }
        /// <summary>环形皮带 + 斜纹（spacing 为纹理间距；thick 加厚）。</summary>
        private void DrawBeltLoop(Graphics g, float lx, float rx, float cy, float pr,
            HToolPalette pal, float spacing, bool thick)
        {
            float top = cy - pr, bot = cy + pr;
            using (var belt = new GraphicsPath())
            {
                belt.AddLine(lx, top, rx, top);
                belt.AddArc(rx - pr, cy - pr, pr * 2f, pr * 2f, -90f, 180f);
                belt.AddLine(rx, bot, lx, bot);
                belt.AddArc(lx - pr, cy - pr, pr * 2f, pr * 2f, 90f, 180f);
                belt.CloseFigure();
                using (var rubber = new SolidBrush(Color.FromArgb(62, 64, 70)))
                    g.FillPath(rubber, belt);
                var old = g.Clip;
                g.SetClip(belt);
                float move = Running ? Phase / (float)Math.PI * 0.5f * spacing * 1.6f : 0f;
                using (var stripe = new Pen(Color.FromArgb(96, 98, 104), thick ? 2.2f : 1.6f))
                    for (float x = lx - pr - spacing * 3f + move % spacing; x < rx + pr; x += spacing)
                        g.DrawLine(stripe, x, top - 2f, x + pr, top + pr * 2f);
                g.Clip = old;
                g.DrawPath(new Pen(Color.FromArgb(38, 40, 45), 1.6f), belt);
            }
        }
        private void DrawPulley(Graphics g, float cx, float cy, float r, HToolPalette pal)
        {
            g.Ell(cx, cy, r, r, pal.Metal);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r, cy - r, r * 2f, r * 2f);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var spoke = new Pen(pal.MetalDark, 1.6f))
                for (int i = 0; i < 4; i++)
                {
                    g.RotateTransform(90f);
                    g.DrawLine(spoke, 0, 0, r * 0.92f, 0);
                }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.22f, r * 0.22f, pal.Accent);
        }
        /// <summary>皮带上匀速移动的骨料。</summary>
        private void DrawStones(Graphics g, float lx, float rx, float top, HToolPalette pal)
        {
            if (!_showMaterial) return;
            float span = rx - lx;
            if (span <= 0.5f) return;
            float move = Running ? Phase / (float)Math.PI * 0.5f * span : 0f;
            var rnd = new Random(7);
            using (var stone = new SolidBrush(Color.FromArgb(149, 112, 72)))
            using (var edge = new Pen(Color.FromArgb(110, 80, 48), 1f))
            {
                for (int i = 0; i < 9; i++)
                {
                    float baseX = lx + ((i * 37f) % (int)span);
                    float x = lx + ((baseX - lx - move) % span + span) % span;
                    float d = Math.Max(2f, 7f + (i % 3) * 2.4f);
                    float y = top - d * 0.7f - rnd.Next(0, 3);
                    g.FillEllipse(stone, x - d / 2f, y - d / 2f, d, d);
                    g.DrawEllipse(edge, x - d / 2f, y - d / 2f, d, d);
                }
            }
        }
        // ----------------------------------------------------------------
        // 新式输送机
        // ----------------------------------------------------------------
        private void DrawTrough(Graphics g, RectangleF a, HToolPalette pal, bool cleated)
        {
            float cy = a.Top + a.Height * 0.5f, pr = a.Height * 0.16f;
            float lx = a.Left + a.Width * 0.1f, rx = a.Right - a.Width * 0.1f;
            // 槽形托辊（三段槽）
            using (var idler = new Pen(pal.MetalDark, 3.4f))
                for (float x = lx + 6f; x < rx; x += 22f)
                {
                    g.DrawLine(idler, x - 10f, cy - 8f, x, cy + 3f);
                    g.DrawLine(idler, x + 10f, cy - 8f, x, cy + 3f);
                    g.DrawLine(idler, x - 10f, cy - 8f, x + 10f, cy - 8f);
                }
            // 槽形皮带（U 截面带）
            using (var belt = new GraphicsPath())
            {
                belt.AddLine(lx, cy - 9f, rx, cy - 9f);
                belt.AddArc(rx - pr, cy - pr * 1.2f, pr * 2f, pr * 2.4f, -90f, 180f);
                belt.AddLine(rx, cy + pr * 0.2f, lx, cy + pr * 0.2f);
                belt.AddArc(lx - pr, cy - pr * 1.2f, pr * 2f, pr * 2.4f, 90f, 180f);
                belt.CloseFigure();
                using (var rb = new SolidBrush(Color.FromArgb(54, 56, 62))) g.FillPath(rb, belt);
                g.DrawPath(new Pen(Color.FromArgb(34, 36, 40), 1.5f), belt);
            }
            DrawPulley(g, lx, cy - 3f, pr * 0.7f, pal);
            DrawPulley(g, rx, cy - 3f, pr * 0.7f, pal);
            if (cleated)
            {
                float move = Running ? Phase / (float)Math.PI * 0.5f * (rx - lx) : 0f, span = rx - lx;
                for (int i = 0; i < 10; i++)
                {
                    float x = lx + ((i * 26f - move) % span + span) % span;
                    g.Line(Color.FromArgb(44, 46, 52), 3f, x, cy - 9f, x, cy - 20f);
                }
            }
            // 骨料堆在槽内
            if (_showMaterial)
            {
                float move = Running ? Phase / (float)Math.PI * 0.5f * (rx - lx) : 0f, span = rx - lx;
                using (var stone = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    for (int i = 0; i < 12; i++)
                    {
                        float x = lx + ((i * 31f - move) % span + span) % span;
                        float d = 6f + (i % 3) * 2f;
                        g.FillEllipse(stone, x - d / 2f, cy - 12f - (i % 2) * 5f, d, d);
                    }
            }
            // 支腿
            g.Line(pal.MetalDark, 3f, lx, cy + pr, lx, a.Bottom - 4f);
            g.Line(pal.MetalDark, 3f, rx, cy + pr, rx, a.Bottom - 4f);
        }
        private void DrawHerring(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.44f, pr = a.Height * 0.17f;
            float lx = a.Left + a.Width * 0.12f, rx = a.Right - a.Width * 0.12f;
            DrawBeltLoop(g, lx, rx, cy, pr, pal, 18f, true);
            DrawPulley(g, lx, cy, pr * 0.72f, pal);
            DrawPulley(g, rx, cy, pr * 0.72f, pal);
            // 人字纹
            float move = Running ? Phase / (float)Math.PI * 0.5f * (rx - lx) : 0f, span = rx - lx;
            using (var h = new Pen(Color.FromArgb(120, 122, 128), 1.8f))
                for (int i = 0; i < 12; i++)
                {
                    float x = lx + ((i * 24f - move) % span + span) % span;
                    g.DrawLine(h, x, cy - pr + 3f, x + 8f, cy - pr - 4f);
                    g.DrawLine(h, x + 16f, cy - pr + 3f, x + 8f, cy - pr - 4f);
                }
            DrawStones(g, lx + 4f, rx - 4f, cy - pr, pal);
            g.Line(pal.MetalDark, 3f, lx, cy + pr, lx, a.Bottom - 4f);
            g.Line(pal.MetalDark, 3f, rx, cy + pr, rx, a.Bottom - 4f);
        }
        private void DrawTube(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.48f, r = a.Height * 0.26f;
            float x1 = a.Left + a.Width * 0.12f, x2 = a.Right - a.Width * 0.12f;
            // 圆管段
            g.FillCylH(new RectangleF(x1, cy - r, x2 - x1, r * 2f), Color.FromArgb(44, 46, 52), Color.FromArgb(96, 98, 104));
            g.DrawR(new RectangleF(x1, cy - r, x2 - x1, r * 2f), r * 0.4f, pal.Edge, 1.4f);
            // 多边形托辊组（六边形窗口暗示卷管）
            using (var hex = new Pen(HToolPalettes.Tint(pal.Metal, 0.3f), 1.4f))
                for (float x = x1 + 20f; x < x2 - 10f; x += 34f)
                {
                    var pts = new PointF[6];
                    for (int k = 0; k < 6; k++)
                    {
                        double an = k * Math.PI / 3.0 + Math.PI / 6.0;
                        pts[k] = new PointF(x + (float)Math.Cos(an) * r * 0.5f, cy + (float)Math.Sin(an) * r * 0.5f);
                    }
                    g.DrawPolygon(hex, pts);
                }
            // 两端成型段（喇叭过渡）
            g.Poly(new[] { new PointF(a.Left + 6f, cy - r * 0.5f), new PointF(x1, cy - r),
                new PointF(x1, cy + r), new PointF(a.Left + 6f, cy + r * 0.5f) }, pal.MetalDark, pal.Edge, 1.2f);
            g.Poly(new[] { new PointF(x2, cy - r), new PointF(a.Right - 6f, cy - r * 0.5f),
                new PointF(a.Right - 6f, cy + r * 0.5f), new PointF(x2, cy + r) }, pal.MetalDark, pal.Edge, 1.2f);
            DrawPulley(g, a.Left + a.Width * 0.06f, cy, r * 0.34f, pal);
            DrawPulley(g, a.Right - a.Width * 0.06f, cy, r * 0.34f, pal);
            // 管内骨料（露出在喇叭口）
            if (_showMaterial)
                using (var st = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    for (int i = 0; i < 5; i++)
                        g.FillEllipse(st, x1 + 6f + i * 8f, cy - 4f - (i % 2) * 5f, 6f, 6f);
        }
        private void DrawCurve(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 90° 转弯段：弧心在左下角，皮带由顶端向右转出（270°→360° 扇环）
            float cx = a.Left + a.Width * 0.1f, cy = a.Bottom - a.Height * 0.08f;
            float rOut = Math.Max(2f, Math.Min(a.Width * 0.52f, cy - a.Top - 4f));
            // 窄高单元中带宽按外半径收缩，保证内弧半径非负
            float bw = Math.Min(a.Height * 0.22f, rOut * 0.6f), rIn = rOut - bw;
            using (var path = new GraphicsPath())
            {
                path.AddArc(cx - rOut, cy - rOut, rOut * 2f, rOut * 2f, 270f, 90f);
                path.AddArc(cx - rIn, cy - rIn, rIn * 2f, rIn * 2f, 0f, -90f);
                path.CloseFigure();
                using (var rb = new SolidBrush(Color.FromArgb(62, 64, 70))) g.FillPath(rb, path);
                g.DrawPath(new Pen(Color.FromArgb(38, 40, 45), 1.5f), path);
            }
            // 转弯后水平出料段
            float ex = a.Right - 8f;
            using (var rb2 = new SolidBrush(Color.FromArgb(62, 64, 70)))
                g.FillRectangle(rb2, cx + rOut - 1f, cy - rOut, ex - cx - rOut, rOut - rIn);
            g.Line(Color.FromArgb(38, 40, 45), 1.5f, cx + rOut, cy - rOut, ex, cy - rOut);
            g.Line(Color.FromArgb(38, 40, 45), 1.5f, cx + rOut, cy - rIn, ex, cy - rIn);
            // 锥形辊（径向短线）
            using (var roller = new Pen(pal.MetalDark, 2.6f))
            {
                for (int i = 0; i <= 6; i++)
                {
                    double an = (270 + i * 14f) * Math.PI / 180.0;
                    g.DrawLine(roller, cx + (float)Math.Cos(an) * rIn, cy + (float)Math.Sin(an) * rIn,
                        cx + (float)Math.Cos(an) * rOut, cy + (float)Math.Sin(an) * rOut);
                }
                // 出料段直辊
                for (float x = cx + rOut + 10f; x < ex; x += 16f)
                    g.DrawLine(roller, x, cy - rOut, x, cy - rIn);
            }
            // 骨料沿弧
            if (_showMaterial && Running)
            {
                float move = Phase / (float)Math.PI * 0.5f * 90f;
                using (var st = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    for (int i = 0; i < 6; i++)
                    {
                        float deg = 272f + ((i * 14f - move) % 86f + 86f) % 86f;
                        double an = deg * Math.PI / 180.0;
                        float rr = (rIn + rOut) / 2f;
                        g.FillEllipse(st, cx + (float)Math.Cos(an) * rr - 3.5f,
                            cy + (float)Math.Sin(an) * rr - 3.5f, 7f, 7f);
                    }
            }
        }
        private void DrawIncline(Graphics g, RectangleF a, HToolPalette pal, bool mobile)
        {
            // 斜置皮带（沿倾斜角旋转绘制）
            var st = g.Save();
            g.TranslateTransform(a.Left + a.Width * 0.5f, a.Top + a.Height * 0.5f);
            g.RotateTransform(mobile ? -22f : -16f);
            float w = a.Width * 0.86f, pr = a.Height * 0.13f, lx = -w / 2f, rx = w / 2f;
            DrawBeltLoop(g, lx, rx, 0f, pr, pal, 20f, false);
            DrawPulley(g, lx, 0f, pr * 0.7f, pal);
            DrawPulley(g, rx, 0f, pr * 0.7f, pal);
            DrawStones(g, lx + 2f, rx - 2f, -pr, pal);
            g.Restore(st);
            if (mobile)
            {
                // 行走轮 + 牵引杆
                float wy = a.Bottom - 8f;
                g.Ell(a.Left + a.Width * 0.22f, wy, 10f, 10f, Color.FromArgb(60, 64, 70));
                g.Ell(a.Right - a.Width * 0.22f, wy, 10f, 10f, Color.FromArgb(60, 64, 70));
                g.Line(pal.Edge, 3.4f, a.Left + 6f, wy - 4f, a.Left + a.Width * 0.22f, wy);
            }
            else
            {
                // 固定支腿（高低差）
                g.Line(pal.MetalDark, 4f, a.Left + a.Width * 0.24f, a.Top + a.Height * 0.36f,
                    a.Left + a.Width * 0.2f, a.Bottom - 4f);
                g.Line(pal.MetalDark, 4f, a.Right - a.Width * 0.24f, a.Bottom - a.Height * 0.42f,
                    a.Right - a.Width * 0.2f, a.Bottom - 4f);
            }
        }
        private void DrawTelescopic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f;
            // 三节嵌套
            for (int k = 0; k < 3; k++)
            {
                float inset = k * a.Width * 0.09f, h = a.Height * (0.32f - k * 0.05f);
                var sec = new RectangleF(a.Left + 6f + inset, cy - h / 2f - k * 1.5f,
                    a.Width - 12f - inset * 2f, h);
                g.FillR(sec, h / 2f, Color.FromArgb(62 + k * 14, 64 + k * 14, 70 + k * 14));
                g.DrawR(sec, h / 2f, pal.Edge, 1.2f);
            }
            float pr = a.Height * 0.12f;
            DrawPulley(g, a.Left + 6f, cy, pr * 0.75f, pal);
            DrawPulley(g, a.Right - 6f, cy, pr * 0.75f, pal);
            // 辊子
            using (var rol = new Pen(pal.MetalDark, 2.2f))
                for (float x = a.Left + 24f; x < a.Right - 20f; x += 16f)
                    g.DrawLine(rol, x, cy - pr, x, cy + pr);
            DrawStones(g, a.Left + 12f, a.Right - 12f, cy - pr, pal);
        }
        private void DrawApron(Graphics g, RectangleF a, HToolPalette pal, bool plastic)
        {
            float cy = a.Top + a.Height * 0.5f, pr = a.Height * 0.16f;
            float lx = a.Left + a.Width * 0.1f, rx = a.Right - a.Width * 0.1f;
            // 链板闭环（矩形链节）
            var band = new RectangleF(lx, cy - pr, rx - lx, pr * 2f);
            g.FillR(band, pr, plastic ? Color.FromArgb(90, 120, 150) : Color.FromArgb(86, 88, 94));
            float move = Running ? Phase / (float)Math.PI * 0.5f * (rx - lx) : 0f, span = rx - lx;
            using (var link = new Pen(plastic ? Color.FromArgb(50, 76, 106) : Color.FromArgb(48, 50, 56), 1.5f))
                for (int i = 0; i < 16; i++)
                {
                    float x = lx + ((i * 14f - move) % span + span) % span;
                    g.DrawLine(link, x, cy - pr + 1f, x, cy + pr - 1f);
                }
            DrawPulley(g, lx, cy, pr * 0.8f, pal);
            DrawPulley(g, rx, cy, pr * 0.8f, pal);
            // 链条
            g.Line(Color.FromArgb(40, 42, 48), 2f, lx, cy - pr - 3f, rx, cy - pr - 3f);
            DrawStones(g, lx + 4f, rx - 4f, cy - pr, pal);
            g.Line(pal.MetalDark, 3f, lx, cy + pr, lx, a.Bottom - 4f);
            g.Line(pal.MetalDark, 3f, rx, cy + pr, rx, a.Bottom - 4f);
        }
        private void DrawMesh(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.48f, pr = a.Height * 0.17f;
            float lx = a.Left + a.Width * 0.1f, rx = a.Right - a.Width * 0.1f;
            var band = new RectangleF(lx, cy - pr, rx - lx, pr * 2f);
            g.FillR(band, pr, Color.FromArgb(150, 154, 160));
            // 菱形网孔
            using (var mesh = new Pen(Color.FromArgb(96, 100, 106), 1f))
            {
                for (float x = lx; x < rx; x += 10f)
                    g.DrawLine(mesh, x, cy - pr, x + 10f, cy);
                for (float x = lx; x < rx; x += 10f)
                    g.DrawLine(mesh, x, cy, x + 10f, cy - pr);
                for (float x = lx; x < rx; x += 10f)
                    g.DrawLine(mesh, x, cy, x + 10f, cy + pr);
                for (float x = lx; x < rx; x += 10f)
                    g.DrawLine(mesh, x, cy + pr, x + 10f, cy);
            }
            DrawPulley(g, lx, cy, pr * 0.72f, pal);
            DrawPulley(g, rx, cy, pr * 0.72f, pal);
            DrawStones(g, lx + 6f, rx - 6f, cy - pr, pal);
        }
        private void DrawRollerTable(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.46f, rh = a.Height * 0.15f;
            float x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.08f;
            using (var frame = new Pen(pal.MetalDark, 3f))
            {
                g.DrawLine(frame, x1, cy + rh + 3f, x2, cy + rh + 3f);
                g.DrawLine(frame, x1, cy - rh - 3f, x2, cy - rh - 3f);
            }
            int n = 8;
            float step = (x2 - x1) / n;
            for (int i = 0; i <= n; i++)
            {
                float x = x1 + i * step;
                var st = g.Save();
                g.TranslateTransform(x, cy);
                g.RotateTransform(Running ? SpinAngle * (i % 2 == 0 ? 1 : -1) : 0f);
                using (var b = new SolidBrush(pal.Metal))
                    g.FillRectangle(b, -step * 0.36f, -rh, step * 0.72f, rh * 2f);
                g.DrawRectangle(new Pen(pal.Edge, 1f), -step * 0.36f, -rh, step * 0.72f, rh * 2f);
                using (var hl = new Pen(Color.White, 1.2f))
                    g.DrawLine(hl, -step * 0.3f, -rh + 2f, step * 0.3f, -rh + 2f);
                g.Restore(st);
            }
            DrawStones(g, x1 + 4f, x2 - 4f, cy - rh, pal);
            g.Line(pal.MetalDark, 3f, x1, cy + rh + 3f, x1, a.Bottom - 4f);
            g.Line(pal.MetalDark, 3f, x2, cy + rh + 3f, x2, a.Bottom - 4f);
        }
        private void DrawScrew(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f, rh = a.Height * 0.26f;
            float x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.08f;
            var tube = new RectangleF(x1, cy - rh, x2 - x1, rh * 2f);
            // U 形槽（半筒）
            g.FillCylH(tube, HToolPalettes.Shade(pal.Metal, 0.35f), Color.White);
            g.DrawR(tube, 6f, pal.Edge, 1.4f);
            // 螺旋叶片（沿程 X 纹）
            float move = Running ? Phase / (float)Math.PI * 0.5f * 28f : 0f;
            using (var flight = new Pen(HToolPalettes.Shade(pal.Main, 0.15f), 3f))
                for (float x = x1 - 28f + move % 28f; x < x2; x += 14f)
                {
                    g.DrawLine(flight, x, cy - rh * 0.8f, x + 14f, cy + rh * 0.8f);
                    g.DrawLine(flight, x, cy + rh * 0.8f, x + 14f, cy - rh * 0.8f);
                }
            // 中心轴
            g.Line(pal.MetalDark, 3.4f, x1 + 4f, cy, x2 - 4f, cy);
            // 端盖 + 驱动
            g.Box(new RectangleF(x2 - 2f, cy - rh - 3f, 8f, rh * 2f + 6f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Motor(x2 + 8f, a.Right - 2f, cy - rh * 0.1f, rh * 0.42f, pal);
            // 槽内骨料
            if (_showMaterial)
            {
                float move2 = Running ? Phase / (float)Math.PI * 0.5f * (x2 - x1) : 0f, span = x2 - x1;
                using (var st = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    for (int i = 0; i < 10; i++)
                    {
                        float x = x1 + ((i * 34f - move2) % span + span) % span;
                        float d = 6f + (i % 2) * 2f;
                        g.FillEllipse(st, x, cy + rh * 0.25f - d / 2f, d, d);
                    }
            }
            // 支腿
            g.Line(pal.MetalDark, 3f, x1 + 12f, cy + rh, x1 + 12f, a.Bottom - 4f);
            g.Line(pal.MetalDark, 3f, x2 - 12f, cy + rh, x2 - 12f, a.Bottom - 4f);
        }
        private void DrawVacuum(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f, pr = a.Height * 0.15f;
            float lx = a.Left + a.Width * 0.1f, rx = a.Right - a.Width * 0.1f;
            DrawBeltLoop(g, lx, rx, cy, pr, pal, 22f, false);
            // 吸风孔
            using (var hole = new SolidBrush(Color.FromArgb(30, 32, 36)))
                for (float x = lx + 10f; x < rx - 8f; x += 14f)
                    g.FillEllipse(hole, x - 2f, cy - pr + 3f, 4f, 4f);
            DrawPulley(g, lx, cy, pr * 0.72f, pal);
            DrawPulley(g, rx, cy, pr * 0.72f, pal);
            // 风箱 + 风管
            g.Box(new RectangleF(lx + (rx - lx) * 0.3f, cy + pr + 2f, (rx - lx) * 0.4f, a.Height * 0.16f),
                3f, pal.MetalDark, pal.Edge, 1.2f);
            g.Line(Color.FromArgb(120, 124, 130), 4f, lx + (rx - lx) * 0.45f, a.Bottom - 6f,
                lx + (rx - lx) * 0.45f, a.Bottom - 1f);
            DrawStones(g, lx + 4f, rx - 4f, cy - pr, pal);
        }
        private void DrawMagnetic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f, pr = a.Height * 0.15f;
            float lx = a.Left + a.Width * 0.1f, rx = a.Right - a.Width * 0.1f;
            DrawBeltLoop(g, lx, rx, cy, pr, pal, 22f, false);
            DrawPulley(g, lx, cy, pr * 0.72f, pal);
            // 磁力轮（红蓝半圆）
            float mr = pr * 0.9f;
            g.Ell(rx, cy, mr, mr, Color.FromArgb(200, 70, 60));
            using (var b = new SolidBrush(Color.FromArgb(60, 110, 200)))
                g.FillPie(b, rx - mr, cy - mr, mr * 2f, mr * 2f, 0f, 180f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), rx - mr, cy - mr, mr * 2f, mr * 2f);
            // 铁屑被吸起
            float move = Running ? Phase / (float)Math.PI * 0.5f * (rx - lx) : 0f, span = rx - lx;
            using (var iron = new SolidBrush(Color.FromArgb(70, 74, 80)))
                for (int i = 0; i < 8; i++)
                {
                    float x = lx + ((i * 30f - move) % span + span) % span;
                    g.FillRectangle(iron, x - 1.5f, cy - pr - 3f - (i % 3) * 2f, 3f, 3f);
                }
        }
        private void DrawSorting(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f, pr = a.Height * 0.13f;
            float lx = a.Left + a.Width * 0.08f, rx = a.Right - a.Width * 0.08f;
            DrawBeltLoop(g, lx, rx, cy, pr, pal, 24f, false);
            DrawPulley(g, lx, cy, pr * 0.7f, pal);
            DrawPulley(g, rx, cy, pr * 0.7f, pal);
            // 摆轮分拣台（几个可转向小轮）
            float sx = lx + (rx - lx) * 0.45f;
            for (int k = -1; k <= 1; k++)
            {
                float x = sx + k * 22f;
                var stt = g.Save();
                g.TranslateTransform(x, cy - pr * 0.4f);
                g.RotateTransform(Running ? 28f * (float)Math.Sin(Phase * 2f + k) : 0f);
                g.Ell(0, 0, 8f, 8f, HToolPalettes.Tint(pal.Main, 0.2f));
                g.DrawEllipse(new Pen(pal.Edge, 1.1f), -8f, -8f, 16f, 16f);
                g.Restore(stt);
            }
            // 分拣出口（斜溜槽）
            g.Line(pal.MetalDark, 3.4f, sx + 30f, cy - pr, sx + 52f, cy + pr + 14f);
            DrawStones(g, lx + 4f, sx - 6f, cy - pr, pal);
        }
        private void DrawBeltDrive(Graphics g, RectangleF a, HToolPalette pal, bool timing, bool poly)
        {
            float c1x = a.Left + a.Width * 0.28f, c2x = a.Right - a.Width * 0.24f;
            float cy = a.Top + a.Height * 0.52f, r1 = a.Height * 0.26f, r2 = a.Height * 0.17f;
            // 皮带（开式环绕两轮）
            using (var bp = new GraphicsPath())
            {
                bp.AddLine(c1x - r1, cy - r1 * 0.92f, c2x + r2, cy - r2 * 0.92f);
                bp.AddArc(c2x - r2, cy - r2, r2 * 2f, r2 * 2f, -90f, 180f);
                bp.AddLine(c2x + r2, cy + r2 * 0.92f, c1x - r1, cy + r1 * 0.92f);
                bp.AddArc(c1x - r1, cy - r1, r1 * 2f, r1 * 2f, 90f, 180f);
                bp.CloseFigure();
                using (var rb = new SolidBrush(Color.FromArgb(58, 60, 66))) g.FillPath(rb, bp);
                g.DrawPath(new Pen(Color.FromArgb(34, 36, 40), 1.5f), bp);
            }
            // 齿形/楔纹
            if (timing || poly)
                using (var tooth = new Pen(Color.FromArgb(110, 112, 118), 1.4f))
                    for (float x = c1x + r1; x < c2x - r2; x += (timing ? 7f : 5f))
                    {
                        g.DrawLine(tooth, x, cy - r1 * 0.92f, x, cy - r1 * 0.92f + (timing ? 5f : 3f));
                        if (poly) g.DrawLine(tooth, x + 2.5f, cy - r1 * 0.92f, x + 2.5f, cy - r1 * 0.92f + 2f);
                    }
            // 大/小带轮
            DrawSheave(g, c1x, cy, r1, timing, pal);
            DrawSheave(g, c2x, cy, r2, timing, pal);
            // V 带槽（普通 V 带轮）
            if (!timing && !poly)
                using (var groove = new Pen(Color.FromArgb(120, 124, 130), 1.2f))
                {
                    g.DrawEllipse(groove, c1x - r1 * 0.6f, cy - r1 * 0.6f, r1 * 1.2f, r1 * 1.2f);
                    g.DrawEllipse(groove, c2x - r2 * 0.6f, cy - r2 * 0.6f, r2 * 1.2f, r2 * 1.2f);
                }
            // 中心连线
            g.Line(pal.Edge, 1.4f, c1x, cy + r1 + 4f, c2x, cy + r2 + 4f);
        }
        private void DrawSheave(Graphics g, float cx, float cy, float r, bool tooth, HToolPalette pal)
        {
            g.Ell(cx, cy, r, r, pal.Metal);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r, cy - r, r * 2f, r * 2f);
            if (tooth)
            {
                var stt = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(SpinAngle);
                using (var tp = new Pen(pal.MetalDark, 1.2f))
                    for (int i = 0; i < 16; i++)
                    {
                        double an = i * Math.PI / 8.0;
                        g.DrawLine(tp, (float)Math.Cos(an) * r * 0.86f, (float)Math.Sin(an) * r * 0.86f,
                            (float)Math.Cos(an) * r, (float)Math.Sin(an) * r);
                    }
                g.Restore(stt);
            }
            else
            {
                var stt = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(SpinAngle);
                using (var sp = new Pen(pal.MetalDark, 1.5f))
                    for (int i = 0; i < 4; i++)
                    {
                        g.RotateTransform(90f);
                        g.DrawLine(sp, 0, 0, r * 0.9f, 0);
                    }
                g.Restore(stt);
            }
            g.Ell(cx, cy, r * 0.2f, r * 0.2f, pal.Accent);
        }
        private void DrawDriveSection(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.4f, pr = a.Height * 0.14f;
            float lx = a.Left + a.Width * 0.08f, rx = a.Right - a.Width * 0.36f;
            DrawBeltLoop(g, lx, rx, cy, pr, pal, 22f, false);
            DrawPulley(g, lx, cy, pr * 0.72f, pal);
            DrawPulley(g, rx, cy, pr * 0.72f, pal);
            // 驱动端：减速器方箱 + 电机（同心轴，整体限制在 a 内）
            float gx = rx + 5f;
            g.Box(new RectangleF(gx, cy - pr * 1.25f, pr * 1.7f, pr * 2.5f), 3f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.3f);
            g.Ell(gx + pr * 0.35f, cy, pr * 0.3f, pr * 0.3f, pal.MetalDark);
            float m1x = gx + pr * 1.8f, m2x = a.Right - 4f;
            g.Motor(m1x, m2x, cy, pr * 0.72f, pal);
            // 底座
            g.Line(pal.MetalDark, 5f, gx - 6f, a.Bottom - 8f, m2x, a.Bottom - 8f);
            DrawStones(g, lx + 4f, rx - 4f, cy - pr, pal);
        }
        private void DrawDrag(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 槽体 + 链条 + 刮板
            float cy = a.Top + a.Height * 0.5f;
            float x1 = a.Left + a.Width * 0.08f, x2 = a.Right - a.Width * 0.08f;
            float t = a.Height * 0.3f, b = a.Height * 0.16f;
            g.Poly(new[]
            {
                new PointF(x1, cy - t / 2f), new PointF(x2, cy - t / 2f),
                new PointF(x2 - b, cy + t / 2f), new PointF(x1 + b, cy + t / 2f)
            }, HToolPalettes.Tint(pal.Metal, 0.2f), pal.Edge, 1.5f);
            // 头轮
            DrawPulley(g, x2 - 4f, cy - t * 0.1f, t * 0.22f, pal);
            DrawPulley(g, x1 + 4f, cy - t * 0.1f, t * 0.22f, pal);
            // 刮板（沿槽移动的立板）
            float move = Running ? Phase / (float)Math.PI * 0.5f * (x2 - x1) : 0f, span = x2 - x1;
            using (var flight = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.2f)))
                for (int i = 0; i < 8; i++)
                {
                    float x = x1 + 8f + ((i * 36f - move) % span + span) % span;
                    g.FillRectangle(flight, x, cy - t * 0.42f, 5f, t * 0.72f);
                }
            // 料床
            if (_showMaterial)
                using (var st = new SolidBrush(Color.FromArgb(120, 92, 60)))
                    g.FillRectangle(st, x1 + b, cy + t * 0.05f, x2 - x1 - b * 2f, t * 0.34f);
        }
    }
}