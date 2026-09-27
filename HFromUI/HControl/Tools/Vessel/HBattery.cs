using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HMath;
namespace HFromUI.HControl.Tools.Vessel
{
    using HFromUI.HControl.Base;
    /// <summary>电池样式：Classic 与旧版 HFrom.HBattery 完全一致，其余 21 种为新式样。</summary>
    public enum HBatteryStyle
    {
        Classic = 0,        // 经典圆柱电池（旧版原样）
        Cylinder3D = 1,     // 3D 圆柱胶囊
        Rounded = 2,        // 圆角矩形
        Segments = 3,       // 分段块
        Thin = 4,           // 超薄条
        Bold = 5,           // 粗笨方块
        Double = 6,         // 双芯堆叠
        Ring = 7,           // 环形电量
        Bolt = 8,           // 闪电图标
        Outline = 9,        // 大百分比描边
        Scale = 10,         // 带刻度尺
        Gloss = 11,         // 光泽高光
        Terminal = 12,      // 方头接线柱
        Brick = 13,         // 厚边框砖
        Slab = 14,          // 无头平板
        Cells = 15,         // 两节五号
        Stripes = 16,       // 斜纹填充
        Gradient = 17,      // 色调渐变填充
        Glow = 18,          // 辉光边
        Watch = 19,         // 手表圆环
        Icon = 20,          // 小图标
        Stacked = 21        // 三格储备
    }
    /// <summary>色调：Classic 为旧版青灰壳配色，1..12 取现代色调表。</summary>
    public enum HBatteryTheme
    {
        Classic = 0, 天蓝 = 1, 海蓝 = 2, 翠绿 = 3, 墨青 = 4, 琥珀 = 5, 橙 = 6,
        玫瑰 = 7, 红 = 8, 紫 = 9, 品红 = 10, 咖啡 = 11, 石墨 = 12
    }
    /// <summary>
    /// 电池/电量控件（继承 HLabelBase → HBarBase）：22 种样式、13 种色调、横/纵方向，
    /// 五段状态色（高→低），Classic 样式 + Classic 色调与旧版 HFrom.HBattery 外形颜色一致。
    /// </summary>
    [HDescriptionLanguage("电池控件：22 种样式、13 种色调，支持横/纵方向、五段状态色与百分比文本")]
    public class HBattery : HBarBase
    {
        private HBatteryStyle _style = HBatteryStyle.Classic;
        private HBatteryTheme _theme = HBatteryTheme.Classic;
        // 五段状态色及分割点（默认值与旧版 HFrom.HBattery 一致）
        private Color _color1 = Color.Green;
        private Color _color2 = Color.LimeGreen;
        private Color _color3 = Color.Orange;
        private Color _color4 = Color.Tomato;
        private Color _color5 = Color.Red;
        private float _s1 = 0.85f, _s2 = 0.60f, _s3 = 0.40f, _s4 = 0.15f;
        private StringFormat _sf;
        private static readonly Color ClsShell = Color.FromArgb(142, 196, 216);
        public HBattery()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Value = 0.0;
            ShowValueText = false;
            Orientation = HBarOrientation.Vertical;
            Size = new Size(93, 171);
            _sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _sf?.Dispose();
            base.Dispose(disposing);
        }
        /// <summary>电池样式。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("电池样式"), HDescriptionLanguage("电池外形样式，Classic 与旧版完全一致"), Browsable(true)]
        [DefaultValue(HBatteryStyle.Classic)]
        public HBatteryStyle BatteryStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("电池壳色调"), HDescriptionLanguage("电池壳色调，Classic 为旧版青灰壳"), Browsable(true)]
        [DefaultValue(HBatteryTheme.Classic)]
        public HBatteryTheme BatteryTheme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>第一区间颜色（最高电量）。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("颜色 1"), HDescriptionLanguage("第一区间（最高电量）颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "Green")]
        public Color Color1 { get => _color1; set { _color1 = value; Invalidate(); } }
        /// <summary>第二区间颜色。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("颜色 2"), HDescriptionLanguage("第二区间颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "LimeGreen")]
        public Color Color2 { get => _color2; set { _color2 = value; Invalidate(); } }
        /// <summary>第三区间颜色。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("颜色 3"), HDescriptionLanguage("第三区间颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "Orange")]
        public Color Color3 { get => _color3; set { _color3 = value; Invalidate(); } }
        /// <summary>第四区间颜色。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("颜色 4"), HDescriptionLanguage("第四区间颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "Tomato")]
        public Color Color4 { get => _color4; set { _color4 = value; Invalidate(); } }
        /// <summary>第五区间颜色（最低电量）。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("颜色 5"), HDescriptionLanguage("第五区间（最低电量）颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "Red")]
        public Color Color5 { get => _color5; set { _color5 = value; Invalidate(); }
        }
        /// <summary>一/二区区间分割点（百分比 0..1）。</summary>
        [HCategoryLanguage("电池"), HDisplayNameLanguage("第一分割点"), HDescriptionLanguage("第一/二区间分割点（0..1）"), Browsable(true)]
        [DefaultValue(0.85f)] public float Separatrix1 { get => _s1; set { _s1 = value; Invalidate(); } }
        /// <summary>二/三区区间分割点。</summary>
        [HCategoryLanguage("电池"), HDescriptionLanguage("第二/三区间分割点（0..1）")]
        [DefaultValue(0.60f)] public float Separatrix2 { get => _s2; set { _s2 = value; Invalidate(); } }
        /// <summary>三/四区区间分割点。</summary>
        [HCategoryLanguage("电池"), HDescriptionLanguage("第三/四区间分割点（0..1）")]
        [DefaultValue(0.40f)] public float Separatrix3 { get => _s3; set { _s3 = value; Invalidate(); } }
        /// <summary>四/五区区间分割点。</summary>
        [HCategoryLanguage("电池"), HDescriptionLanguage("第四/五区间分割点（0..1）")]
        [DefaultValue(0.15f)] public float Separatrix4 { get => _s4; set { _s4 = value; Invalidate(); } }
        private HToolPalette Palette => _theme == HBatteryTheme.Classic
            ? new HToolPalette
            {
                Main = ClsShell, Light = HToolPalettes.Tint(ClsShell, 0.5f), Lighter = Color.WhiteSmoke,
                Dark = HToolPalettes.Shade(ClsShell, 0.3f), Edge = ClsShell,
                Glass = Color.FromArgb(247, 252, 253), GlassEdge = ClsShell,
                Metal = Color.FromArgb(247, 252, 253), MetalDark = ClsShell, Accent = Color.DodgerBlue
            }
            : HToolPalettes.Get((int)_theme);
        private Color Status => StatusColor(Percent, _color1, _color2, _color3, _color4, _color5,
            _s1, _s2, _s3, _s4);
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (Width < 8 || Height < 8) return;
            if (_style == HBatteryStyle.Classic)
            {
                PaintClassic(g, Width, Height);
                return;
            }
            if (_style == HBatteryStyle.Ring || _style == HBatteryStyle.Watch)
            {
                DrawRing(g, _style == HBatteryStyle.Watch);
                DrawValueText(g, ClientRectangle);
                return;
            }
            // 纵向：逆时针 90 度旋转后按横向绘制——极耳朝上、电量自下而上填充
            if (Orientation == HBarOrientation.Vertical)
            {
                g.TranslateTransform(0f, Height);
                g.RotateTransform(-90f);
                DrawModern(g, new RectangleF(0f, 0f, Height, Width));
                g.ResetTransform();
            }
            else
            {
                DrawModern(g, new RectangleF(0f, 0f, Width, Height));
            }
            if (ShowValueText)
                DrawValueText(g, ClientRectangle);
        }
        private void DrawValueText(Graphics g, Rectangle rect)
        {
            if (!ShowValueText) return;
            TextRenderer.DrawText(g, ((int)(Percent * 100)).ToString() + " %", Font, rect,
                ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
        // ------------------------------------------------------------------
        // Classic：旧版 HFrom.HBattery 原样移植（圆柱、五段色、凸点、旋转）
        // ------------------------------------------------------------------
        private void PaintClassic(Graphics g, int width, int height)
        {
            RectangleF textRect;
            if (Orientation == HBarOrientation.Vertical)
            {
                // 立式：竖矩形内直接绘制（与旧版默认外观一致，93x171）
                textRect = new RectangleF(0f, height * 0.1f, width, height * 0.9f);
                PaintClassicMain(g, width, height);
            }
            else
            {
                // 卧式：旋转 90 度后按立式几何绘制（旧版 DirectionStyleH.Vertical 同一变换）
                textRect = new RectangleF(0f, 0f, width * 0.9f, height);
                g.TranslateTransform(width, 0f);
                g.RotateTransform(90f);
                PaintClassicMain(g, height, width);
                g.ResetTransform();
            }
            if (ShowValueText)
                using (var b = new SolidBrush(ForeColor))
                    g.DrawString(string.Format("{0} %", Value), Font, b, textRect, _sf);
        }
        private void PaintClassicMain(Graphics g, float width, float height)
        {
            g.TranslateTransform(width / 2f, 0f);
            // Classic 色调沿用旧版青灰壳；其他色调壳身取现代调色板（电量五段状态色不变）
            Color shell = _theme == HBatteryTheme.Classic
                ? ClsShell
                : HToolPalettes.Get((int)_theme).Edge;
            var cb = new ColorBlend
            {
                Positions = new[] { 0f, 0.75f, 1f },
                Colors = new[] { shell, Color.WhiteSmoke, shell }
            };
            LinearGradientBrush lgb = new LinearGradientBrush(
                new PointF(-width / 2f, 0), new PointF(width / 2f, 0),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            using (var backPen = new Pen(shell))
            using (var backBrush = new SolidBrush(shell))
            {
                g.FillEllipse(lgb, -width * 0.5f - 1f, height * 0.93f, width + 1f, height * 0.06f);
                g.DrawEllipse(backPen, -width * 0.5f - 1f, height * 0.93f, width + 1f, height * 0.06f);
                g.FillRectangle(lgb, -width * 0.5f - 1f, height * 0.1f, width + 1f, height * 0.86f);
                g.FillEllipse(backBrush, -width * 0.5f - 1f, height * 0.07f, width + 1f, height * 0.06f);
                using (var lgb2 = new LinearGradientBrush(new PointF(-width * 0.15f, 0), new PointF(width * 0.15f, 0),
                           Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
                {
                    lgb2.InterpolationColors = cb;
                    g.FillEllipse(lgb2, -width * 0.15f, height * 0.085f, width * 0.3f, height * 0.02f);
                    g.FillRectangle(lgb2, -width * 0.15f, height * 0.03f, width * 0.3f, height * 0.065f);
                    g.FillEllipse(backBrush, -width * 0.15f, height * 0.02f, width * 0.3f, height * 0.02f);
                }
                Color vc = Status;
                cb.Colors = new[] { vc, Color.WhiteSmoke, vc };
                float num = height * 0.86f - Percent * height * 0.86f + height * 0.1f;
                if (num < height * 0.1f) num = height * 0.1f;
                if (num > height * 0.96f) num = height * 0.96f;
                lgb.InterpolationColors = cb;
                g.FillEllipse(lgb, -width * 0.5f - 1f, height * 0.93f, width + 1f, height * 0.06f);
                using (var pen = new Pen(vc))
                    g.DrawEllipse(pen, -width * 0.5f - 1f, height * 0.93f, width + 1f, height * 0.06f);
                g.FillRectangle(lgb, -width * 0.5f - 1f, num, width + 1f, height * 0.86f - (num - height * 0.1f));
                using (var bb = new SolidBrush(vc))
                    g.FillEllipse(bb, -width * 0.5f - 1f, num - height * 0.03f, width + 1f, height * 0.06f);
            }
            lgb.Dispose();
            g.TranslateTransform(-width / 2f, 0f);
        }
        // ------------------------------------------------------------------
        // 现代样式（横向画布 c：c.X..c.Right）
        // ------------------------------------------------------------------
        private void DrawModern(Graphics g, RectangleF c)
        {
            var pal = Palette;
            switch (_style)
            {
                case HBatteryStyle.Cylinder3D: DrawCylinder3D(g, c, pal); break;
                case HBatteryStyle.Rounded: DrawRounded(g, c, pal, 0.22f, true); break;
                case HBatteryStyle.Segments: DrawSegments(g, c, pal); break;
                case HBatteryStyle.Thin: DrawRounded(g, c, pal, 0.5f, true, 0.55f); break;
                case HBatteryStyle.Bold: DrawRounded(g, c, pal, 0.12f, true, 0.92f); break;
                case HBatteryStyle.Double: DrawDouble(g, c, pal); break;
                case HBatteryStyle.Bolt: DrawRounded(g, c, pal, 0.22f, true); DrawBolt(g, c, Color.White); break;
                case HBatteryStyle.Outline: DrawOutline(g, c, pal); break;
                case HBatteryStyle.Scale: DrawScale(g, c, pal); break;
                case HBatteryStyle.Gloss: DrawRounded(g, c, pal, 0.22f, true); DrawGloss(g, c); break;
                case HBatteryStyle.Terminal: DrawTerminal(g, c, pal); break;
                case HBatteryStyle.Brick: DrawRounded(g, c, pal, 0.08f, true, 0.86f, 2.4f); break;
                case HBatteryStyle.Slab: DrawRounded(g, c, pal, 0.18f, false); break;
                case HBatteryStyle.Cells: DrawCells(g, c, pal); break;
                case HBatteryStyle.Stripes: DrawRounded(g, c, pal, 0.22f, true); DrawStripes(g, c); break;
                case HBatteryStyle.Gradient: DrawGradient(g, c, pal); break;
                case HBatteryStyle.Glow: DrawGlow(g, c, pal); break;
                case HBatteryStyle.Icon: DrawRounded(g, c, pal, 0.28f, true, 0.78f); break;
                case HBatteryStyle.Stacked: DrawStacked(g, c, pal); break;
            }
        }
        /// <summary>通用电池主体矩形（右侧留出极耳）。</summary>
        private static RectangleF ShellRect(RectangleF c, float heightRatio, out float nubW, float topPad = 0f)
        {
            nubW = Math.Max(3f, c.Height * 0.16f);
            float h = c.Height * heightRatio;
            float y = c.Y + (c.Height - h) / 2f + topPad;
            return new RectangleF(c.X + 1.5f, y, c.Width - nubW - 3f, h);
        }
        private void DrawRounded(Graphics g, RectangleF c, HToolPalette pal, float radiusRatio, bool nub,
            float heightRatio = 0.72f, float edgeW = 1.5f)
        {
            float nubW;
            var r = ShellRect(c, heightRatio, out nubW);
            float rad = Math.Min(r.Height * radiusRatio, r.Width * 0.2f);
            var nubR = nub
                ? new RectangleF(r.Right + 1f, r.Y + r.Height * 0.32f, nubW - 1f, r.Height * 0.36f)
                : RectangleF.Empty;
            // 外壳与极耳路径各只构造一次：先填充后描边，避免重复创建泄漏 GDI 路径
            using (GraphicsPath sp = RoundPath(r, rad))
            using (GraphicsPath np = nub ? RoundPath(nubR, 1.5f) : null)
            using (var sb = CylinderH(r, pal.GlassEdge, Color.WhiteSmoke))
            using (var ep = new Pen(pal.Dark, edgeW))
            {
                g.FillPath(sb, sp);
                g.DrawPath(ep, sp);
                if (nub)
                {
                    using (var nb = new SolidBrush(pal.GlassEdge))
                        g.FillPath(nb, np);
                    g.DrawPath(ep, np);
                }
                // 内槽
                var inner = RectangleF.Inflate(r, -r.Height * 0.16f, -r.Height * 0.18f);
                using (GraphicsPath ip = RoundPath(inner, inner.Height / 2f))
                using (var ib = new SolidBrush(Color.FromArgb(214, 218, 224)))
                    g.FillPath(ib, ip);
                // 电量填充
                float p = Percent;
                if (p > 0f)
                {
                    var fill = inner;
                    fill.Width *= p;
                    float fr = p >= 0.999f ? inner.Height / 2f : Math.Min(fill.Height / 2f, fill.Width);
                    using (GraphicsPath fp = RoundPath(fill, fr))
                    using (var fb = new SolidBrush(Status))
                        g.FillPath(fb, fp);
                }
            }
        }
        private void DrawCylinder3D(Graphics g, RectangleF c, HToolPalette pal)
        {
            float nubW;
            var r = ShellRect(c, 0.78f, out nubW);
            float cap = r.Height;
            // 壳体：右封头 + 底母线 + 左封头（标准横向胶囊）
            using (var shell = new GraphicsPath())
            {
                shell.StartFigure();
                shell.AddArc(r.Right - cap, r.Y, cap, cap, 270, 180);
                shell.AddLine(r.Right - cap / 2f, r.Bottom, r.X + cap / 2f, r.Bottom);
                shell.AddArc(r.X, r.Y, cap, cap, 90, 180);
                shell.CloseFigure();
                using (var sb = CylinderH(r, pal.GlassEdge, Color.WhiteSmoke))
                    g.FillPath(sb, shell);
                using (var ep = new Pen(pal.Dark, 1.5f))
                    g.DrawPath(ep, shell);
            }
            // 极耳
            using (var nb = new SolidBrush(pal.Dark))
                g.FillRectangle(nb, r.Right + 1f, r.Y + r.Height * 0.36f, nubW - 1f, r.Height * 0.28f);
            // 液位（圆柱内腔）
            var inner = new RectangleF(r.X + cap * 0.4f, r.Y + r.Height * 0.16f,
                r.Width - cap * 0.9f, r.Height * 0.68f);
            float p = Percent;
            var fill = inner;
            fill.X = inner.X;
            fill.Width *= p;
            if (p > 0f)
                using (var fb = new SolidBrush(Status))
                    g.FillRectangle(fb, fill);
        }
        private void DrawSegments(Graphics g, RectangleF c, HToolPalette pal)
        {
            float nubW;
            var r = ShellRect(c, 0.72f, out nubW);
            using (var ep = new Pen(pal.Dark, 1.5f))
            {
                g.DrawRectangle(ep, r.X, r.Y, r.Width, r.Height);
                using (var nb = new SolidBrush(pal.Dark))
                    g.FillRectangle(nb, r.Right + 1f, r.Y + r.Height * 0.34f, nubW - 1f, r.Height * 0.32f);
            }
            const int n = 8;
            float gap = r.Height * 0.18f;
            float bw = (r.Width - gap * (n + 1)) / n;
            int lit = (int)Math.Ceiling(Percent * n);
            for (int i = 0; i < n; i++)
            {
                var br = new RectangleF(r.X + gap + i * (bw + gap), r.Y + gap, bw, r.Height - gap * 2);
                Color cc = i < lit ? Status : Color.FromArgb(205, 210, 216);
                using (var bb = new SolidBrush(cc))
                using (GraphicsPath bp = RoundPath(br, bw * 0.25f))
                    g.FillPath(bb, bp);
            }
        }
        private void DrawDouble(Graphics g, RectangleF c, HToolPalette pal)
        {
            float nubW;
            var r = ShellRect(c, 0.92f, out nubW);
            float h2 = (r.Height - 6f) / 2f;
            for (int i = 0; i < 2; i++)
            {
                var rr = new RectangleF(r.X, r.Y + i * (h2 + 6f), r.Width - nubW, h2);
                using (GraphicsPath sp = RoundPath(rr, h2 / 2f))
                using (var sb = CylinderH(rr, pal.GlassEdge, Color.WhiteSmoke))
                {
                    g.FillPath(sb, sp);
                    var fill = rr;
                    fill.Inflate(-2f, -2f);
                    fill.Width = fill.Width * Percent * (i == 0 ? 1f : 0.7f); // 下芯作储备显示
                    if (fill.Width > 1f)
                        using (GraphicsPath fp = RoundPath(fill, Math.Min(fill.Height / 2f, fill.Width / 2f)))
                        using (var fb = new SolidBrush(i == 0 ? Status : HToolPalettes.Tint(Status, 0.35f)))
                            g.FillPath(fb, fp);
                    using (var ep = new Pen(pal.Dark, 1.2f))
                        g.DrawPath(ep, sp);
                }
            }
            using (var nb = new SolidBrush(pal.Dark))
                g.FillRectangle(nb, r.Right + 1f, r.Y + r.Height * 0.34f, nubW - 1f, r.Height * 0.32f);
        }
        private void DrawOutline(Graphics g, RectangleF c, HToolPalette pal)
        {
            DrawRounded(g, c, pal, 0.22f, true);
            string s = ((int)(Percent * 100)).ToString();
            using (var f = new Font(Font.FontFamily, c.Height * 0.34f, FontStyle.Bold, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, s, f,
                    new Rectangle(0, 0, (int)(c.Width - c.Height * 0.3f), (int)c.Height),
                    Color.FromArgb(90, pal.Dark), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void DrawScale(Graphics g, RectangleF c, HToolPalette pal)
        {
            DrawRounded(g, c, pal, 0.22f, true, 0.60f);
            float nubW;
            var r = ShellRect(c, 0.60f, out nubW);
            using (var tp = new Pen(pal.Dark, 1f))
            using (var tb = new SolidBrush(pal.Dark))
                for (int i = 0; i <= 10; i++)
                {
                    float x = r.X + r.Width * i / 10f;
                    float len = i % 5 == 0 ? r.Height * 0.22f : r.Height * 0.12f;
                    g.DrawLine(tp, x, r.Bottom + 1f, x, r.Bottom + 1f + len);
                }
        }
        private void DrawGloss(Graphics g, RectangleF c)
        {
            float nubW;
            var r = ShellRect(c, 0.72f, out nubW);
            var shine = new RectangleF(r.X + r.Width * 0.05f, r.Y + r.Height * 0.10f,
                r.Width * 0.84f, r.Height * 0.30f);
            using (GraphicsPath gp = RoundPath(shine, r.Height * 0.12f))
            using (var sb = new SolidBrush(Color.FromArgb(70, Color.White)))
                g.FillPath(sb, gp);
        }
        private void DrawTerminal(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.X + c.Height * 0.18f, c.Y + c.Height * 0.14f,
                c.Width - c.Height * 0.36f, c.Height * 0.72f);
            using (var sb = CylinderH(r, pal.GlassEdge, Color.WhiteSmoke))
                g.FillRectangle(sb, r);
            using (var ep = new Pen(pal.Dark, 1.6f))
                g.DrawRectangle(ep, r.X, r.Y, r.Width, r.Height);
            // 左右接线柱
            foreach (float sx in new[] { r.X, r.Right })
            {
                var cap = new RectangleF(sx - c.Height * 0.09f, r.Y + r.Height * 0.30f,
                    c.Height * 0.18f, c.Height * 0.40f);
                using (var mb = new SolidBrush(pal.Metal))
                    g.FillEllipse(mb, cap);
                using (var cp = new Pen(pal.Dark, 1.4f))
                    g.DrawEllipse(cp, cap);
                using (var db = new SolidBrush(pal.Dark))
                    g.FillEllipse(db, sx - c.Height * 0.03f, r.Y + r.Height * 0.45f,
                        c.Height * 0.06f, c.Height * 0.10f);
            }
            var inner = RectangleF.Inflate(r, -r.Height * 0.12f, -r.Height * 0.22f);
            if (Percent > 0f)
            {
                var fill = inner;
                fill.Width *= Percent;
                using (var fb = new SolidBrush(Status))
                    g.FillRectangle(fb, fill);
            }
        }
        private void DrawCells(Graphics g, RectangleF c, HToolPalette pal)
        {
            // 两节五号电池横排
            float gap = c.Height * 0.10f;
            float cellW = (c.Width - gap) / 2f;
            for (int i = 0; i < 2; i++)
            {
                var r = new RectangleF(c.X + i * (cellW + gap), c.Y + c.Height * 0.16f,
                    cellW - gap, c.Height * 0.68f);
                float cap = r.Height;
                using (var path = new GraphicsPath())
                {
                    path.StartFigure();
                    path.AddArc(r.Right - cap, r.Y, cap, cap, 270, 180);
                    path.AddLine(r.Right - cap / 2f, r.Bottom, r.X + cap / 2f, r.Bottom);
                    path.AddArc(r.X, r.Y, cap, cap, 90, 180);
                    path.CloseFigure();
                    using (var sb = CylinderH(r, pal.GlassEdge, Color.WhiteSmoke))
                        g.FillPath(sb, path);
                    using (var ep = new Pen(pal.Dark, 1.2f))
                        g.DrawPath(ep, path);
                }
                using (var nb = new SolidBrush(pal.Dark))
                    g.FillRectangle(nb, r.Right - cap * 0.2f, r.Y + r.Height * 0.36f,
                        cap * 0.5f, r.Height * 0.28f);
                float p = Math.Max(0f, Math.Min(1f, Percent * 2 - i));
                var fill = new RectangleF(r.X + cap * 0.5f, r.Y + r.Height * 0.22f,
                    (r.Width - cap) * p, r.Height * 0.56f);
                if (p > 0f)
                    using (var fb = new SolidBrush(Status))
                        g.FillRectangle(fb, fill);
            }
        }
        private void DrawStripes(Graphics g, RectangleF c)
        {
            float nubW;
            var r = ShellRect(c, 0.72f, out nubW);
            var inner = RectangleF.Inflate(r, -r.Height * 0.16f, -r.Height * 0.18f);
            using (var sp = new Pen(Color.FromArgb(110, Color.White), Math.Max(2f, inner.Height / 8f)))
            {
                for (float x = inner.Left - inner.Height; x < inner.Right; x += inner.Height / 1.6f)
                    g.DrawLine(sp, x, inner.Bottom, x + inner.Height, inner.Top);
            }
        }
        private void DrawGradient(Graphics g, RectangleF c, HToolPalette pal)
        {
            float nubW;
            var r = ShellRect(c, 0.72f, out nubW);
            using (GraphicsPath sp = RoundPath(r, r.Height * 0.22f))
            using (var sb = CylinderH(r, pal.GlassEdge, Color.WhiteSmoke))
                g.FillPath(sb, sp);
            using (var nb = new SolidBrush(pal.Dark))
                g.FillRectangle(nb, r.Right + 1f, r.Y + r.Height * 0.34f, nubW - 1f, r.Height * 0.32f);
            var inner = RectangleF.Inflate(r, -r.Height * 0.16f, -r.Height * 0.18f);
            using (GraphicsPath ip = RoundPath(inner, inner.Height / 2f))
            using (var ib = new SolidBrush(Color.FromArgb(214, 218, 224)))
                g.FillPath(ib, ip);
            if (Percent > 0f)
            {
                var fill = inner;
                fill.Width *= Percent;
                using (var clip = new Region(RoundPath(inner, inner.Height / 2f)))
                using (var lb = new LinearGradientBrush(inner, pal.Light, pal.Dark, 0f))
                {
                    var old = g.Clip;
                    g.SetClip(clip, CombineMode.Replace);
                    g.FillRectangle(lb, inner.X, inner.Y, inner.Width * Percent, inner.Height);
                    g.Clip = old;
                    old.Dispose();
                }
            }
            using (var ep = new Pen(pal.Dark, 1.5f))
            using (GraphicsPath epp = RoundPath(r, r.Height * 0.22f))
                g.DrawPath(ep, epp);
        }
        private void DrawGlow(Graphics g, RectangleF c, HToolPalette pal)
        {
            DrawRounded(g, c, pal, 0.22f, true);
            float nubW;
            var r = ShellRect(c, 0.72f, out nubW);
            Color glow = Status;
            using (var gp = new Pen(Color.FromArgb(70, glow), 5f))
            using (GraphicsPath gpp = RoundPath(r, r.Height * 0.22f))
                g.DrawPath(gp, gpp);
        }
        private void DrawStacked(Graphics g, RectangleF c, HToolPalette pal)
        {
            float nubW;
            var r = ShellRect(c, 0.82f, out nubW);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawRectangle(ep, r.X, r.Y, r.Width, r.Height);
            using (var nb = new SolidBrush(pal.Dark))
                g.FillRectangle(nb, r.Right + 1f, r.Y + r.Height * 0.34f, nubW - 1f, r.Height * 0.32f);
            const int n = 3;
            float gap = r.Height * 0.10f;
            float bh = (r.Height - gap * (n + 1)) / n;
            int lit = (int)Math.Ceiling(Percent * n);
            for (int i = 0; i < n; i++)
            {
                var br = new RectangleF(r.X + gap, r.Bottom - gap - bh - i * (bh + gap),
                    r.Width - gap * 2, bh);
                Color cc = i < lit ? HToolPalettes.Tint(Status, i * 0.18f) : Color.FromArgb(205, 210, 216);
                using (var bb = new SolidBrush(cc))
                    g.FillRectangle(bb, br);
            }
        }
        /// <summary>环形电量（Ring）/ 手表环（Watch）。</summary>
        private void DrawRing(Graphics g, bool watch)
        {
            var pal = Palette;
            float cx = Width / 2f, cy = Height / 2f;
            float d = Math.Min(Width, Height) - (watch ? 6f : 10f);
            var arc = new RectangleF(cx - d / 2f, cy - d / 2f, d, d);
            float thick = watch ? d * 0.10f : d * 0.14f;
            using (var track = new Pen(Color.FromArgb(214, 218, 224), thick)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(track, arc, -90f, 360f);
            if (Percent > 0f)
                using (var value = new Pen(Status, thick) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(value, arc, -90f, 360f * Percent);
            if (watch)
            {
                // 表冠
                using (var b = new SolidBrush(pal.MetalDark))
                    g.FillRectangle(b, cx - 2f, arc.Y - 5f, 4f, 7f);
                // 表盘内圈
                using (var ep = new Pen(pal.MetalDark, 1.2f))
                    g.DrawEllipse(ep, arc.X + thick * 1.4f, arc.Y + thick * 1.4f,
                        d - thick * 2.8f, d - thick * 2.8f);
            }
            else
            {
                // 电池小图标
                float iw = d * 0.34f, ih = d * 0.18f;
                var rr = new RectangleF(cx - iw / 2f, cy - ih / 2f, iw, ih);
                using (var fb = new SolidBrush(Status))
                    g.FillRectangle(fb, rr);
                using (var ep = new Pen(pal.Dark, 1.4f))
                {
                    g.DrawRectangle(ep, rr.X, rr.Y, rr.Width, rr.Height);
                    g.DrawLine(ep, rr.Right, cy - ih * 0.28f, rr.Right + 3f, cy - ih * 0.28f);
                    g.DrawLine(ep, rr.Right, cy + ih * 0.28f, rr.Right + 3f, cy + ih * 0.28f);
                }
            }
        }
        /// <summary>白色闪电图标。</summary>
        private void DrawBolt(Graphics g, RectangleF c, Color color)
        {
            float nubW;
            var r = ShellRect(c, 0.72f, out nubW);
            float bx = r.X + r.Width * 0.42f, bw = r.Width * 0.16f;
            var pts = new[]
            {
                new PointF(bx + bw * 0.75f, r.Y + r.Height * 0.12f),
                new PointF(bx, r.Y + r.Height * 0.55f),
                new PointF(bx + bw * 0.55f, r.Y + r.Height * 0.55f),
                new PointF(bx + bw * 0.25f, r.Y + r.Height * 0.88f),
                new PointF(bx + bw, r.Y + r.Height * 0.45f),
                new PointF(bx + bw * 0.45f, r.Y + r.Height * 0.45f)
            };
            using (var b = new SolidBrush(color))
                g.FillPolygon(b, pts);
        }
    }
}