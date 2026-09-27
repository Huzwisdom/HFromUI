using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Vessel
{
    using HFromUI.HLangage;
    using HFromUI.HControl.Base;
    /// <summary>瓶/罐样式：Classic 与旧版 HFrom.HBottle 完全一致，其余 21 种为新瓶型。</summary>
    public enum HBottleStyle
    {
        Classic = 0,      // 经典三角烧瓶（旧版原样）
        WineBottle = 1,   // 红酒瓶
        RoundFlask = 2,   // 圆底烧瓶
        Erlenmeyer = 3,   // 锥形瓶
        Reagent = 4,      // 试剂瓶
        Jar = 5,          // 广口罐
        Can = 6,          // 易拉罐
        Barrel = 7,       // 木桶
        TestTube = 8,     // 试管
        Beaker = 9,       // 烧杯
        Perfume = 10,     // 香水瓶
        GasCylinder = 11, // 钢瓶气瓶
        Amphora = 12,     // 双耳陶罐
        Vase = 13,        // 花瓶
        Carton = 14,      // 屋顶纸盒
        Tank = 15,        // 卧式储罐
        Bucket = 16,      // 提桶
        Dropper = 17,     // 滴瓶
        Potion = 18,      // 魔药瓶
        Pouch = 19,       // 自立袋
        Growler = 20,     // 啤酒扎壶
        Keg = 21          // 小酒桶
    }
    /// <summary>色调：Classic 为旧版黄液+青玻璃配色，1..12 取现代色调表。</summary>
    public enum HBottleTheme
    {
        Classic = 0, 天蓝 = 1, 海蓝 = 2, 翠绿 = 3, 墨青 = 4, 琥珀 = 5, 橙 = 6,
        玫瑰 = 7, 红 = 8, 紫 = 9, 品红 = 10, 咖啡 = 11, 石墨 = 12
    }
    /// <summary>
    /// 瓶子/罐子控件（继承 HLabelBase → HBarBase）：支持液位值、开关状态、22 种瓶型、13 种色调。
    /// Classic 样式 + Classic 色调与旧版 HFrom.HBottle 的形状和颜色完全一致。
    /// </summary>
    [HDescriptionLanguage("瓶/罐控件：22 种瓶型、13 种色调，支持液位显示与标题文本")]
    public class HBottle : HBarBase
    {
        private HBottleStyle _style = HBottleStyle.Classic;
        private HBottleTheme _theme = HBottleTheme.Classic;
        private bool _isOpen;
        private string _bottleTag = "";
        private string _headTag = HTranslation.GetContent("原料1");
        private float _dockHeight = 30f;
        private StringFormat _sf;
        // 经典样式六色（与旧版 HFrom.HBottle 逐色一致）
        private static readonly Color ClsForeTop = Color.FromArgb(243, 245, 139);
        private static readonly Color ClsForeEdge = Color.FromArgb(194, 190, 77);
        private static readonly Color ClsForeCenter = Color.FromArgb(226, 221, 98);
        private static readonly Color ClsBackTop = Color.FromArgb(151, 232, 244);
        private static readonly Color ClsBackEdge = Color.FromArgb(142, 196, 216);
        private static readonly Color ClsBackCenter = Color.FromArgb(240, 240, 240);
        public HBottle()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Value = 50.0;
            Size = new Size(150, 150);
            _sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        }
        /// <summary>瓶型样式。</summary>
        [HCategoryLanguage("瓶罐"), HDisplayNameLanguage("瓶型样式"), HDescriptionLanguage("瓶/罐外形样式，Classic 与旧版完全一致"), Browsable(true)]
        [DefaultValue(HBottleStyle.Classic)]
        public HBottleStyle BottleStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("瓶罐"), HDisplayNameLanguage("整体色调"), HDescriptionLanguage("整体色调，Classic 为旧版黄液青玻璃配色"), Browsable(true)]
        [DefaultValue(HBottleTheme.Classic)]
        public HBottleTheme BottleTheme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>是否开盖/开口。</summary>
        [HCategoryLanguage("瓶罐"), HDisplayNameLanguage("打开"), HDescriptionLanguage("瓶子是否处于开盖/开口状态"), Browsable(true)]
        [DefaultValue(false)]
        public bool IsOpen
        {
            get => _isOpen;
            set { _isOpen = value; Invalidate(); }
        }
        /// <summary>瓶身标签文本。</summary>
        [HCategoryLanguage("瓶罐"), HDisplayNameLanguage("瓶罐标签"), HDescriptionLanguage("绘制在瓶身上的标签信息"), Browsable(true)]
        [DefaultValue("")]
        public string BottleTag
        {
            get => _bottleTag;
            set { _bottleTag = value ?? string.Empty; Invalidate(); }
        }
        /// <summary>顶部标题文本。</summary>
        [HCategoryLanguage("瓶罐"), HDisplayNameLanguage("封头标签"), HDescriptionLanguage("绘制在瓶子顶部的标题信息"), Browsable(true)]
        [DefaultValue("原料1")]
        public string HeadTag
        {
            get => _headTag;
            set { _headTag = value ?? string.Empty; Invalidate(); }
        }
        /// <summary>底座高度（像素；小于 1 视为按控件高度的比例），仅 Classic 样式使用。</summary>
        [HCategoryLanguage("瓶罐"), HDisplayNameLanguage("底座高度"), HDescriptionLanguage("Classic 样式底座高度，小于 1 按高度比例"), Browsable(true)]
        [DefaultValue(30f)]
        public float DockHeight
        {
            get => _dockHeight;
            set { _dockHeight = value; Invalidate(); }
        }
        private HToolPalette Palette => _theme == HBottleTheme.Classic
            ? new HToolPalette
            {
                Main = ClsForeCenter, Light = ClsForeTop, Lighter = HToolPalettes.Tint(ClsForeTop, 0.6f),
                Dark = HToolPalettes.Shade(ClsForeCenter, 0.25f), Edge = ClsForeEdge,
                Glass = ClsBackTop, GlassEdge = ClsBackEdge, Metal = Color.DimGray, MetalDark = Color.Gray,
                Accent = ClsBackCenter
            }
            : HToolPalettes.Get((int)_theme);
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (Width < 8 || Height < 8) return;
            if (_style == HBottleStyle.Classic) PaintClassic(g, Width, Height);
            else PaintModern(g);
        }
        // ------------------------------------------------------------------
        // Classic：旧版 HFrom.HBottle.PaintControlsH 原样移植
        // ------------------------------------------------------------------
        private void PaintClassic(Graphics g, int width, int height)
        {
            if (width < 15 || height < 15) return;
            // Classic 色调沿用旧版黄液青玻璃；其他色调把同一几何映射到现代调色板
            bool themed = _theme != HBottleTheme.Classic;
            HToolPalette pal = themed ? HToolPalettes.Get((int)_theme) : default(HToolPalette);
            Color backEdge = themed ? pal.GlassEdge : ClsBackEdge;
            Color backCenter = themed ? pal.Lighter : ClsBackCenter;
            Color backTop = themed ? Color.White : ClsBackTop;
            Color foreEdge = themed ? pal.Dark : ClsForeEdge;
            Color foreCenter = themed ? pal.Main : ClsForeCenter;
            Color foreTop = themed ? pal.Light : ClsForeTop;
            int num = (_dockHeight < 1f) ? (int)(_dockHeight * height) : (int)_dockHeight;
            int num2 = 20;
            float x = width / 2f;
            float num3 = (height - num) - (height - num - num2) * (float)Value / 100f;
            int num4 = width / 50 + 3;
            if (string.IsNullOrEmpty(_headTag)) num2 = num4;
            var gp = new GraphicsPath();
            gp.AddPolygon(new[]
            {
                new PointF(0f, num2), new PointF(0f, height - num), new PointF(x, height - 1),
                new PointF(width - 1, height - num), new PointF(width - 1, num2), new PointF(0f, num2)
            });
            var lgb = new LinearGradientBrush(new Point(0, num2), new Point(width - 1, num2), backEdge, backCenter);
            var cb = new ColorBlend { Positions = new[] { 0f, 0.5f, 1f }, Colors = new[] { backEdge, backCenter, backEdge } };
            lgb.InterpolationColors = cb;
            g.FillPath(lgb, gp);
            using (var b = new SolidBrush(backTop))
                g.FillEllipse(b, 1, num2 - num4, width - 2, num4 * 2);
            gp.Reset();
            gp.AddPolygon(new[]
            {
                new PointF(0f, num3), new PointF(0f, height - num), new PointF(x, height - 1),
                new PointF(width - 1, height - num), new PointF(width - 1, num3), new PointF(0f, num3)
            });
            cb.Colors = new[] { foreEdge, foreCenter, foreEdge };
            lgb.InterpolationColors = cb;
            g.FillPath(lgb, gp);
            using (var b = new SolidBrush(foreTop))
                g.FillEllipse(b, 1f, num3 - num4, width - 2, num4 * 2);
            gp.Reset();
            gp.AddPolygon(new[]
            {
                new PointF(0f, height - num), new PointF(x, height - 1), new PointF(width - 1, height - num)
            });
            gp.AddArc(0, height - num - num4, width, num4 * 2, 0f, 180f);
            using (var b = new SolidBrush(foreEdge))
                g.FillPath(b, gp);
            gp.Reset();
            gp.AddLines(new[]
            {
                new PointF(width / 4f, height - num / 2f), new PointF(width / 4f, height - 1),
                new PointF(width * 3f / 4f, height - 1), new PointF(width * 3f / 4f, height - num / 2f)
            });
            gp.AddArc(width / 4f, height - num / 2f - num4 - 1, width / 2f, num4 * 2, 0f, 180f);
            g.FillPath(Brushes.DimGray, gp);
            lgb.Dispose();
            gp.Dispose();
            using (var b = new SolidBrush(ForeColor))
            {
                if (!string.IsNullOrEmpty(_bottleTag))
                    g.DrawString(_bottleTag, Font, b, new Rectangle(-10, 26, width + 20, 20), _sf);
                if (!string.IsNullOrEmpty(_headTag))
                    g.DrawString(_headTag, Font, b, new Rectangle(-10, 0, width + 20, 20), _sf);
            }
        }
        // ------------------------------------------------------------------
        // 现代瓶型：统一“玻璃体 + 液位 + 描边 + 瓶盖附件 + 文本”管线
        // ------------------------------------------------------------------
        private void PaintModern(Graphics g)
        {
            var pal = Palette;
            float headH = string.IsNullOrEmpty(_headTag) ? 2f : 18f;
            var r = new RectangleF(2f, headH, Width - 4f, Height - headH - 2f);
            using (GraphicsPath body = BuildBody(_style, r))
            {
                body.FillMode = FillMode.Winding;
                var oldClip = g.Clip;
                g.SetClip(new Region(body), CombineMode.Replace);
                // 玻璃罐体
                using (var glass = CylinderV(r, pal.Lighter, Color.White, pal.GlassEdge))
                    g.FillPath(glass, body);
                // 液位（瓶体轮廓裁剪区内直接从底部填充，圆底同样覆盖）
                float p = Percent;
                if (p > 0f)
                {
                    float ly = r.Bottom - r.Height * p;
                    var lr = new RectangleF(r.X - 2f, ly, r.Width + 4f, r.Bottom - ly + 2f);
                    using (var lb = CylinderV(lr, pal.Light, pal.Main, pal.Dark))
                        g.FillRectangle(lb, r.X - 2f, ly - 1f, r.Width + 4f, r.Bottom - ly + 3f);
                    if (p < 0.995f)
                        using (var hp = new Pen(Color.FromArgb(170, Color.White), 1.6f))
                            g.DrawLine(hp, r.X + 2f, ly + 0.8f, r.Right - 2f, ly + 0.8f);
                }
                g.Clip = oldClip;
                // 玻璃高光线
                using (var hp = new Pen(Color.FromArgb(90, Color.White), Math.Max(1.5f, r.Width * 0.045f)))
                    g.DrawLine(hp, r.X + r.Width * 0.16f, r.Y + r.Height * 0.12f, r.X + r.Width * 0.16f, r.Bottom - r.Height * 0.10f);
                // 轮廓
                using (var ep = new Pen(pal.GlassEdge, 1.6f))
                    g.DrawPath(ep, body);
            }
            DrawAccessory(g, _style, r, pal);
            DrawMouthIfOpen(g, _style, r, pal);
            using (var b = new SolidBrush(ForeColor))
            {
                if (!string.IsNullOrEmpty(_headTag))
                    g.DrawString(_headTag, Font, b, new RectangleF(-10f, 0f, Width + 20f, headH), _sf);
                if (!string.IsNullOrEmpty(_bottleTag))
                    g.DrawString(_bottleTag, Font, b,
                        new RectangleF(r.X, r.Y + r.Height * 0.32f, r.Width, r.Height * 0.3f), _sf);
            }
        }
        /// <summary>按样式构造瓶体外形路径（含瓶颈，不含瓶盖装饰）。</summary>
        private static GraphicsPath BuildBody(HBottleStyle s, RectangleF r)
        {
            float w = r.Width, h = r.Height, cx = r.X + w / 2f;
            var p = new GraphicsPath();
            float rr;
            switch (s)
            {
                case HBottleStyle.WineBottle:
                    rr = w * 0.06f;
                    p.StartFigure();
                    p.AddLine(cx - w * 0.07f, r.Y, cx + w * 0.07f, r.Y);
                    p.AddLine(cx + w * 0.07f, r.Y, cx + w * 0.07f, r.Y + h * 0.30f);
                    p.AddBezier(cx + w * 0.07f, r.Y + h * 0.30f, cx + w * 0.30f, r.Y + h * 0.34f,
                        r.X + w * 0.06f, r.Y + h * 0.46f, r.X + rr, r.Y + h * 0.52f);
                    p.AddLine(r.X + rr, r.Y + h * 0.52f, r.X + rr, r.Bottom - rr);
                    p.AddArc(r.X, r.Bottom - rr * 2, rr * 2, rr * 2, 0, 90);
                    p.AddLine(r.X, r.Bottom, r.Right, r.Bottom);
                    p.AddArc(r.Right - rr * 2, r.Bottom - rr * 2, rr * 2, rr * 2, 90, 90);
                    p.AddLine(r.Right - rr, r.Bottom - rr, r.Right - rr, r.Y + h * 0.52f);
                    p.AddBezier(r.Right - rr, r.Y + h * 0.52f, r.Right - w * 0.06f, r.Y + h * 0.46f,
                        cx - w * 0.30f, r.Y + h * 0.34f, cx - w * 0.07f, r.Y + h * 0.30f);
                    p.AddLine(cx - w * 0.07f, r.Y + h * 0.30f, cx - w * 0.07f, r.Y);
                    p.CloseFigure();
                    break;
                case HBottleStyle.RoundFlask:
                case HBottleStyle.Potion:
                {
                    float nr = w * 0.09f;
                    float rad = w * 0.36f;
                    float cy = r.Bottom - rad;
                    p.AddLine(cx - nr, r.Y, cx + nr, r.Y);
                    p.AddLine(cx + nr, r.Y, cx + nr, cy - rad * 0.35f);
                    p.AddArc(cx - rad, cy - rad, rad * 2, rad * 2, 20, 140);
                    p.AddLine(cx - nr, cy - rad * 0.35f, cx - nr, r.Y);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Erlenmeyer:
                {
                    float nr = w * 0.09f;
                    rr = w * 0.05f;
                    p.AddLine(cx - nr, r.Y, cx + nr, r.Y);
                    p.AddLine(cx + nr, r.Y, cx + nr, r.Y + h * 0.18f);
                    p.AddLine(cx + nr, r.Y + h * 0.18f, r.Right - rr, r.Bottom - rr);
                    p.AddArc(r.Right - rr * 2, r.Bottom - rr * 2, rr * 2, rr * 2, 0, 90);
                    p.AddArc(r.X, r.Bottom - rr * 2, rr * 2, rr * 2, 90, 90);
                    p.AddLine(r.X + rr, r.Bottom - rr, cx - nr, r.Y + h * 0.18f);
                    p.AddLine(cx - nr, r.Y + h * 0.18f, cx - nr, r.Y);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Reagent:
                    rr = w * 0.06f;
                    float nw = w * 0.24f;
                    p.AddLine(cx - nw / 2, r.Y, cx + nw / 2, r.Y);
                    p.AddLine(cx + nw / 2, r.Y, cx + nw / 2, r.Y + h * 0.14f);
                    p.AddLine(cx + nw / 2, r.Y + h * 0.14f, r.Right - rr, r.Y + h * 0.22f);
                    p.AddLine(r.Right - rr, r.Y + h * 0.22f, r.Right - rr, r.Bottom - rr);
                    p.AddArc(r.Right - rr * 2, r.Bottom - rr * 2, rr * 2, rr * 2, 0, 90);
                    p.AddArc(r.X, r.Bottom - rr * 2, rr * 2, rr * 2, 90, 90);
                    p.AddLine(r.X + rr, r.Bottom - rr, r.X + rr, r.Y + h * 0.22f);
                    p.AddLine(cx - nw / 2, r.Y + h * 0.14f, cx - nw / 2, r.Y);
                    p.CloseFigure();
                    break;
                case HBottleStyle.Jar:
                case HBottleStyle.Dropper:
                    rr = w * (s == HBottleStyle.Jar ? 0.12f : 0.18f);
                    float top = s == HBottleStyle.Jar ? r.Y + h * 0.08f : r.Y + h * 0.34f;
                    float x0 = s == HBottleStyle.Jar ? r.X : cx - w * 0.22f;
                    float x1 = s == HBottleStyle.Jar ? r.Right : cx + w * 0.22f;
                    p.StartFigure();
                    p.AddArc(x0, top, rr * 2, rr * 2, 180, 90);
                    p.AddArc(x1 - rr * 2, top, rr * 2, rr * 2, 270, 90);
                    p.AddArc(x1 - rr * 2, r.Bottom - rr * 2, rr * 2, rr * 2, 0, 90);
                    p.AddArc(x0, r.Bottom - rr * 2, rr * 2, rr * 2, 90, 90);
                    p.CloseFigure();
                    if (s == HBottleStyle.Dropper)
                        p.AddRectangle(new RectangleF(cx - w * 0.05f, r.Y, w * 0.10f, h * 0.36f));
                    break;
                case HBottleStyle.Can:
                    p.StartFigure();
                    rr = w * 0.12f;
                    p.AddArc(r.X, r.Y, rr * 2, rr * 2, 180, 90);
                    p.AddArc(r.Right - rr * 2, r.Y, rr * 2, rr * 2, 270, 90);
                    p.AddArc(r.Right - rr * 2, r.Bottom - rr * 2, rr * 2, rr * 2, 0, 90);
                    p.AddArc(r.X, r.Bottom - rr * 2, rr * 2, rr * 2, 90, 90);
                    p.CloseFigure();
                    break;
                case HBottleStyle.Barrel:
                case HBottleStyle.Keg:
                {
                    float bulge = s == HBottleStyle.Keg ? w * 0.04f : w * 0.08f;
                    float ey = h * (s == HBottleStyle.Keg ? 0.16f : 0.10f);
                    float y0 = r.Y + ey, y1 = r.Bottom - ey;
                    p.StartFigure();
                    p.AddBezier(r.X + w * 0.12f, y0, r.X - bulge, y0 + (y1 - y0) * 0.3f,
                        r.X - bulge, y1 - (y1 - y0) * 0.3f, r.X + w * 0.12f, y1);
                    p.AddBezier(r.Right - w * 0.12f, y1, r.Right + bulge, y1 - (y1 - y0) * 0.3f,
                        r.Right + bulge, y0 + (y1 - y0) * 0.3f, r.Right - w * 0.12f, y0);
                    p.AddArc(r.X + w * 0.12f, y0 - ey, w * 0.76f, ey * 2, 0, -180);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.TestTube:
                {
                    float tw = w * 0.28f;
                    float tr = tw / 2f;
                    p.StartFigure();
                    p.AddLine(cx - tr, r.Y, cx - tr, r.Bottom - tr);
                    p.AddArc(cx - tr, r.Bottom - tr * 2, tr * 2, tr * 2, 180, -180);   // 圆底下鼓
                    p.AddLine(cx + tr, r.Bottom - tr, cx + tr, r.Y);
                    p.AddLine(cx + tr, r.Y, cx - tr, r.Y);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Beaker:
                {
                    float bw = w * 0.66f, tw2 = w * 0.78f;
                    float by = r.Y + h * 0.10f;
                    rr = w * 0.04f;
                    p.StartFigure();
                    p.AddLine(cx - tw2 / 2, r.Y, cx + tw2 / 2, r.Y);
                    p.AddLine(cx + tw2 / 2, r.Y, cx + bw / 2, by);
                    p.AddLine(cx + bw / 2, by, cx + bw / 2, r.Bottom - rr);
                    p.AddArc(cx - bw / 2, r.Bottom - rr * 2, bw, rr * 2, 0, 180);
                    p.AddLine(cx - bw / 2, by, cx - tw2 / 2, r.Y);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Perfume:
                {
                    float nw2 = w * 0.12f;
                    p.AddRectangle(new RectangleF(cx - nw2 / 2, r.Y, nw2, h * 0.16f));
                    p.StartFigure();
                    var br = new RectangleF(cx - w * 0.30f, r.Y + h * 0.16f, w * 0.60f, h * 0.78f);
                    p.AddArc(br.X, br.Y, w * 0.16f, w * 0.16f, 180, 90);
                    p.AddArc(br.Right - w * 0.16f, br.Y, w * 0.16f, w * 0.16f, 270, 90);
                    p.AddArc(br.Right - w * 0.16f, br.Bottom - w * 0.16f, w * 0.16f, w * 0.16f, 0, 90);
                    p.AddArc(br.X, br.Bottom - w * 0.16f, w * 0.16f, w * 0.16f, 90, 90);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.GasCylinder:
                {
                    // 瓶身为高胶囊体，瓶颈/阀门在 DrawAccessory 中另画
                    float cw = w * 0.62f;
                    float x = cx - cw / 2f;
                    float cylTop = r.Y + h * 0.12f;
                    rr = cw / 2f;
                    p.StartFigure();
                    p.AddArc(x, cylTop, cw, cw, 180, 180);                 // 顶封头上拱
                    p.AddLine(x, cylTop + rr, x, r.Bottom - rr);
                    p.AddArc(x, r.Bottom - cw, cw, cw, 180, -180);         // 底封头下鼓
                    p.AddLine(x + cw, r.Bottom - rr, x + cw, cylTop + rr);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Amphora:
                case HBottleStyle.Vase:
                {
                    float nw2 = w * (s == HBottleStyle.Amphora ? 0.18f : 0.30f);
                    float bulge2 = w * 0.44f;
                    p.StartFigure();
                    p.AddLine(cx - nw2 / 2, r.Y, cx + nw2 / 2, r.Y);
                    p.AddBezier(cx + nw2 / 2, r.Y, cx + w * 0.28f, r.Y + h * 0.16f,
                        cx + bulge2, r.Y + h * 0.42f, cx + bulge2, r.Y + h * 0.60f);
                    p.AddBezier(cx + bulge2, r.Y + h * 0.60f, cx + bulge2, r.Y + h * 0.80f,
                        cx + w * 0.20f, r.Bottom - h * 0.06f, cx + w * 0.14f, r.Bottom);
                    p.AddLine(cx + w * 0.14f, r.Bottom, cx - w * 0.14f, r.Bottom);
                    p.AddBezier(cx - w * 0.20f, r.Bottom - h * 0.06f, cx - bulge2, r.Y + h * 0.80f,
                        cx - bulge2, r.Y + h * 0.60f, cx - bulge2, r.Y + h * 0.42f);
                    p.AddBezier(cx - bulge2, r.Y + h * 0.42f, cx - w * 0.28f, r.Y + h * 0.16f,
                        cx - nw2 / 2, r.Y, cx - nw2 / 2, r.Y);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Carton:
                    p.AddPolygon(new[]
                    {
                        new PointF(r.X, r.Y + h * 0.14f), new PointF(cx, r.Y), new PointF(r.Right, r.Y + h * 0.14f),
                        new PointF(r.Right, r.Bottom), new PointF(r.X, r.Bottom)
                    });
                    break;
                case HBottleStyle.Tank:
                {
                    float th = h * 0.50f;
                    float ty = r.Y + h * 0.22f;
                    p.StartFigure();
                    p.AddArc(r.X, ty, th, th, 270, -180);                 // 左封头（向左鼓）
                    p.AddLine(r.X, ty + th, r.Right - th / 2f, ty + th);  // 底母线
                    p.AddArc(r.Right - th, ty, th, th, 90, -180);        // 右封头（向右鼓）
                    p.CloseFigure();                                     // 顶母线闭合
                    break;
                }
                case HBottleStyle.Bucket:
                {
                    rr = w * 0.05f;
                    p.StartFigure();
                    p.AddLine(r.X + w * 0.04f, r.Y + h * 0.08f, r.Right - w * 0.04f, r.Y + h * 0.08f);
                    p.AddLine(r.Right - w * 0.04f, r.Y + h * 0.08f, r.Right - w * 0.12f, r.Bottom - rr);
                    p.AddArc(r.X + w * 0.12f - rr, r.Bottom - rr * 2, (w * 0.76f), rr * 2, 0, 180);
                    p.CloseFigure();
                    break;
                }
                case HBottleStyle.Pouch:
                    p.AddPolygon(new[]
                    {
                        new PointF(cx - w * 0.34f, r.Y + h * 0.12f),
                        new PointF(cx + w * 0.34f, r.Y + h * 0.12f),
                        new PointF(cx + w * 0.44f, r.Y + h * 0.22f),
                        new PointF(cx + w * 0.38f, r.Bottom - h * 0.06f),
                        new PointF(cx + w * 0.20f, r.Bottom),
                        new PointF(cx - w * 0.20f, r.Bottom),
                        new PointF(cx - w * 0.38f, r.Bottom - h * 0.06f),
                        new PointF(cx - w * 0.44f, r.Y + h * 0.22f)
                    });
                    break;
                case HBottleStyle.Growler:
                {
                    rr = w * 0.07f;
                    float nw2 = w * 0.30f;
                    p.StartFigure();
                    p.AddLine(cx - nw2 / 2, r.Y, cx + nw2 / 2, r.Y);
                    p.AddLine(cx + nw2 / 2, r.Y, cx + nw2 / 2, r.Y + h * 0.12f);
                    p.AddLine(cx + nw2 / 2, r.Y + h * 0.12f, r.Right - rr, r.Y + h * 0.20f);
                    p.AddLine(r.Right - rr, r.Y + h * 0.20f, r.Right - rr, r.Bottom - rr);
                    p.AddArc(r.Right - rr * 2, r.Bottom - rr * 2, rr * 2, rr * 2, 0, 90);
                    p.AddArc(r.X, r.Bottom - rr * 2, rr * 2, rr * 2, 90, 90);
                    p.AddLine(r.X + rr, r.Y + h * 0.12f, cx - nw2 / 2, r.Y);
                    p.CloseFigure();
                    break;
                }
                default:
                    p.AddRectangle(r);
                    break;
            }
            return p;
        }
        /// <summary>瓶盖、阀门、把手等附件（在瓶体之外绘制）。</summary>
        private void DrawAccessory(Graphics g, HBottleStyle s, RectangleF r, HToolPalette pal)
        {
            float w = r.Width, h = r.Height, cx = r.X + w / 2f;
            using (var metal = new SolidBrush(pal.MetalDark))
            using (var metalL = new SolidBrush(pal.Metal))
            using (var darkB = new SolidBrush(pal.Dark))
            using (var mainB = new SolidBrush(pal.Main))
            using (var edgeP = new Pen(pal.Edge, 1.4f))
            using (var metalP = new Pen(pal.MetalDark, 1.6f))
            {
                switch (s)
                {
                    case HBottleStyle.WineBottle:
                        if (!_isOpen) // 锡箔封套 + 软木塞
                        {
                            g.FillRectangle(darkB, cx - w * 0.09f, r.Y - 1, w * 0.18f, h * 0.16f);
                            g.FillRectangle(metalL, cx - w * 0.07f, r.Y - h * 0.03f, w * 0.14f, h * 0.03f);
                        }
                        break;
                    case HBottleStyle.RoundFlask:
                    case HBottleStyle.Potion:
                        if (!_isOpen)
                            g.FillRectangle(metal, cx - w * 0.10f, r.Y - h * 0.04f, w * 0.20f, h * 0.06f);
                        break;
                    case HBottleStyle.Reagent:
                        if (!_isOpen)
                            g.FillRectangle(metalL, cx - w * 0.14f, r.Y - h * 0.04f, w * 0.28f, h * 0.06f);
                        break;
                    case HBottleStyle.Jar:
                        if (!_isOpen)
                        {
                            var lr = new RectangleF(r.X + w * 0.02f, r.Y + h * 0.01f, w * 0.96f, h * 0.09f);
                            using (GraphicsPath lp = RoundPath(lr, w * 0.05f))
                            using (var lb = CylinderV(lr, pal.Metal, pal.Metal, pal.MetalDark))
                            {
                                g.FillPath(lb, lp);
                                g.DrawPath(metalP, lp);
                            }
                        }
                        break;
                    case HBottleStyle.Can:
                        g.DrawLine(edgeP, r.X + w * 0.06f, r.Y + h * 0.10f, r.Right - w * 0.06f, r.Y + h * 0.10f);
                        if (!_isOpen)
                        {
                            g.FillEllipse(metalL, cx - w * 0.10f, r.Y + h * 0.02f, w * 0.20f, h * 0.05f);
                            using (var tp = new Pen(pal.MetalDark, 1.2f))
                                g.DrawEllipse(tp, cx - w * 0.10f, r.Y + h * 0.02f, w * 0.20f, h * 0.05f);
                        }
                        else g.FillEllipse(darkB, cx - w * 0.07f, r.Y + h * 0.03f, w * 0.14f, h * 0.04f);
                        break;
                    case HBottleStyle.Barrel:
                    case HBottleStyle.Keg:
                        foreach (float fy in s == HBottleStyle.Keg
                            ? new[] { 0.24f, 0.80f } : new[] { 0.18f, 0.50f, 0.82f })
                        {
                            RectangleF br = BarrelBandRect(r, fy, s == HBottleStyle.Keg ? w * 0.04f : w * 0.08f);
                            g.FillRectangle(metal, br);
                        }
                        if (s == HBottleStyle.Keg)
                            g.FillEllipse(darkB, cx - w * 0.05f, r.Bottom - h * 0.30f, w * 0.10f, h * 0.05f);
                        break;
                    case HBottleStyle.TestTube:
                        g.DrawLine(metalP, cx - w * 0.17f, r.Y, cx + w * 0.17f, r.Y);
                        break;
                    case HBottleStyle.Beaker:
                        if (!_isOpen)
                            g.FillPolygon(metalL, new[]
                            {
                                new PointF(cx + w * 0.36f, r.Y), new PointF(cx + w * 0.46f, r.Y + h * 0.02f),
                                new PointF(cx + w * 0.36f, r.Y + h * 0.04f)
                            });
                        break;
                    case HBottleStyle.Perfume:
                        if (!_isOpen)
                        {
                            g.FillRectangle(darkB, cx - w * 0.10f, r.Y, w * 0.20f, h * 0.08f);
                            g.FillRectangle(mainB, cx + w * 0.02f, r.Y + h * 0.02f, w * 0.16f, h * 0.03f);
                        }
                        break;
                    case HBottleStyle.GasCylinder:
                        // 瓶颈、手轮与阀门（位于瓶顶上方区域内）
                        g.FillRectangle(metal, cx - w * 0.06f, r.Y, w * 0.12f, h * 0.06f);
                        g.FillEllipse(darkB, cx - w * 0.11f, r.Y + h * 0.005f, w * 0.22f, h * 0.07f);
                        g.FillRectangle(metalL, cx - w * 0.04f, r.Y + h * 0.07f, w * 0.08f, h * 0.04f);
                        break;
                    case HBottleStyle.Amphora:
                        using (var hp = new Pen(pal.MetalDark, Math.Max(2f, w * 0.05f)))
                        {
                            g.DrawArc(hp, r.X + w * 0.02f, r.Y + h * 0.10f, w * 0.26f, h * 0.30f, 300, 150);
                            g.DrawArc(hp, r.Right - w * 0.28f, r.Y + h * 0.10f, w * 0.26f, h * 0.30f, 90, 150);
                        }
                        break;
                    case HBottleStyle.Vase:
                        g.DrawLine(metalP, cx - w * 0.18f, r.Y, cx + w * 0.18f, r.Y);
                        break;
                    case HBottleStyle.Carton:
                        g.DrawLine(edgeP, cx, r.Y, cx, r.Bottom);
                        g.DrawLine(edgeP, r.X, r.Y + h * 0.14f, r.Right, r.Y + h * 0.14f);
                        g.FillPolygon(metalL, new[]
                        {
                            new PointF(cx, r.Y), new PointF(r.Right, r.Y + h * 0.14f),
                            new PointF(cx, r.Y + h * 0.14f)
                        });
                        break;
                    case HBottleStyle.Tank:
                    {
                        // 支脚贴紧罐体下母线（罐体位于 0.22h..0.72h）
                        float tankBottom = r.Y + h * 0.72f;
                        g.FillRectangle(metal, r.X + w * 0.14f, tankBottom - 2f, w * 0.10f, r.Bottom - tankBottom + 2f);
                        g.FillRectangle(metal, r.Right - w * 0.24f, tankBottom - 2f, w * 0.10f, r.Bottom - tankBottom + 2f);
                        g.FillEllipse(darkB, cx - w * 0.08f, r.Y + h * 0.16f, w * 0.16f, h * 0.08f);
                        break;
                    }
                    case HBottleStyle.Bucket:
                        using (var hp = new Pen(pal.MetalDark, Math.Max(1.5f, w * 0.04f)))
                            g.DrawArc(hp, r.X + w * 0.10f, r.Y - h * 0.02f, w * 0.80f, h * 0.16f, 200, 140);
                        break;
                    case HBottleStyle.Dropper:
                        if (!_isOpen)
                            g.FillEllipse(darkB, cx - w * 0.12f, r.Y, w * 0.24f, h * 0.18f);
                        break;
                    case HBottleStyle.Pouch:
                        g.FillRectangle(metal, r.X + w * 0.08f, r.Y + h * 0.06f, w * 0.84f, h * 0.06f);
                        g.FillPolygon(darkB, new[]
                        {
                            new PointF(cx + w * 0.28f, r.Y + h * 0.06f),
                            new PointF(cx + w * 0.36f, r.Y),
                            new PointF(cx + w * 0.30f, r.Y + h * 0.08f)
                        });
                        break;
                    case HBottleStyle.Growler:
                        if (!_isOpen)
                            g.FillRectangle(metalL, cx - w * 0.17f, r.Y - h * 0.04f, w * 0.34f, h * 0.07f);
                        using (var hp = new Pen(pal.MetalDark, Math.Max(1.5f, w * 0.045f)))
                            g.DrawArc(hp, r.Right - w * 0.34f, r.Y + h * 0.10f, w * 0.30f, h * 0.40f, 300, 160);
                        break;
                }
            }
        }
        /// <summary>木桶/小酒桶箍条矩形（随鼓腹外扩）。</summary>
        private static RectangleF BarrelBandRect(RectangleF r, float fy, float bulge)
        {
            float w = r.Width, h = r.Height;
            float t = (fy - 0.18f) / 0.64f;
            t = Math.Max(0f, Math.Min(1f, t));
            float extra = bulge * (float)Math.Sin(t * Math.PI);
            return new RectangleF(r.X - extra, r.Y + h * fy - h * 0.018f,
                w + extra * 2f, h * 0.036f);
        }
        /// <summary>开口瓶型：未加盖时在瓶口画深色口沿。</summary>
        private void DrawMouthIfOpen(Graphics g, HBottleStyle s, RectangleF r, HToolPalette pal)
        {
            if (!_isOpen) return;
            float w = r.Width, h = r.Height, cx = r.X + w / 2f;
            using (var mb = new SolidBrush(pal.Edge))
            {
                switch (s)
                {
                    case HBottleStyle.WineBottle:
                        g.FillRectangle(mb, cx - w * 0.07f, r.Y, w * 0.14f, h * 0.025f);
                        break;
                    case HBottleStyle.Erlenmeyer:
                        g.FillRectangle(mb, cx - w * 0.09f, r.Y, w * 0.18f, h * 0.025f);
                        break;
                    case HBottleStyle.Reagent:
                        g.FillRectangle(mb, cx - w * 0.12f, r.Y, w * 0.24f, h * 0.025f);
                        break;
                    case HBottleStyle.Jar:
                        g.FillEllipse(mb, r.X + w * 0.08f, r.Y + h * 0.05f, w * 0.84f, h * 0.05f);
                        break;
                }
            }
        }
    }
}