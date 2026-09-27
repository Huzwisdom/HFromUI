using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Process
{
    using HFromUI.HControl.Base;
    /// <summary>厂房样式：Classic 与旧版 HFrom.HFactory 完全一致，其余 21 种为新式厂房。</summary>
    public enum HFactoryStyle
    {
        Classic = 0,          // 经典等距厂房（旧版原样）
        FlatRoof = 1,         // 平顶厂房
        GableFactory = 2,     // 双坡厂房
        SawTooth = 3,         // 锯齿屋顶厂房
        ChimneyWorks = 4,     // 烟囱厂房
        CoolingTower = 5,     // 冷却塔
        SiloFarm = 6,         // 筒仓群
        SolarFactory = 7,     // 太阳能屋顶厂
        GlassTower = 8,       // 玻璃幕墙高楼
        Warehouse = 9,        // 物流仓库
        Refinery = 10,        // 炼油精馏装置
        Hangar = 11,          // 拱形机库
        DomePlant = 12,       // 穹顶厂房
        OfficeBlock = 13,     // 退台办公楼
        WorkshopSkylight = 14,// 天窗通风车间
        TankFarm = 15,        // 储罐区
        WindmillPlant = 16,   // 风电厂房
        HydroDam = 17,        // 水电站大坝
        NuclearPlant = 18,    // 核电站
        Greenhouse = 19,      // 玻璃温室
        ThermalPlant = 20,    // 火力发电厂
        ContainerYard = 21    // 集装箱堆场
    }
    /// <summary>色调：Classic 为旧版 7 色等距配色，1..12 取现代色调表。</summary>
    public enum HFactoryTheme
    {
        Classic = 0, 天蓝 = 1, 海蓝 = 2, 翠绿 = 3, 墨青 = 4, 琥珀 = 5, 橙 = 6,
        玫瑰 = 7, 红 = 8, 紫 = 9, 品红 = 10, 咖啡 = 11, 石墨 = 12
    }
    /// <summary>
    /// 工厂厂房控件（继承 HLabelBase）：22 种厂房外形、13 种色调、横/纵方向，
    /// Classic 样式 + Classic 色调与旧版 HFrom.HFactory 的形状和颜色完全一致。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("工厂厂房控件：22 种厂房样式、13 种色调，支持颜色自定义")]
    public class HFactory : HLabelBase
    {
        private HFactoryStyle _style = HFactoryStyle.Classic;
        private HFactoryTheme _theme = HFactoryTheme.Classic;
        private HBarOrientation _orientation = HBarOrientation.Horizontal;
        // Classic 七色（与旧版 HFrom.HFactory 逐色一致）
        private Color _color1 = Color.FromArgb(160, 216, 239);
        private Color _color2 = Color.FromArgb(149, 202, 224);
        private Color _color3 = Color.FromArgb(248, 248, 248);
        private Color _color4 = Color.FromArgb(181, 181, 181);
        private Color _color5 = Color.FromArgb(255, 222, 173);
        private Color _color6 = Color.FromArgb(233, 201, 142);
        private Color _color7 = Color.FromArgb(158, 158, 158);
        public HFactory()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(325, 163);
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }
        /// <summary>厂房样式。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("厂房外形样式"), HDescriptionLanguage("厂房外形样式，Classic 与旧版完全一致"), Browsable(true)]
        [DefaultValue(HFactoryStyle.Classic)]
        public HFactoryStyle FactoryStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("厂房色调"), HDescriptionLanguage("厂房色调，Classic 为旧版等距配色"), Browsable(true)]
        [DefaultValue(HFactoryTheme.Classic)]
        public HFactoryTheme FactoryTheme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>厂房方向（纵向旋转 90 度绘制，与旧版一致）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("厂房方向"), HDescriptionLanguage("厂房方向：横向或纵向"), Browsable(true)]
        [DefaultValue(HBarOrientation.Horizontal)]
        public HBarOrientation PipeLineStyle
        {
            get => _orientation;
            set { _orientation = value; Invalidate(); }
        }
        /// <summary>自定义颜色1（Classic 屋顶受光面）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 1"), HDescriptionLanguage("自定义颜色1（Classic 屋顶受光面）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[160, 216, 239]")]
        public Color Color1 { get => _color1; set { _color1 = value; Invalidate(); } }
        /// <summary>自定义颜色2（Classic 屋顶背光面）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 2"), HDescriptionLanguage("自定义颜色2（Classic 屋顶背光面）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[149, 202, 224]")]
        public Color Color2 { get => _color2; set { _color2 = value; Invalidate(); } }
        /// <summary>自定义颜色3（Classic 墙受光面）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 3"), HDescriptionLanguage("自定义颜色3（Classic 墙受光面）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[248, 248, 248]")]
        public Color Color3 { get => _color3; set { _color3 = value; Invalidate(); } }
        /// <summary>自定义颜色4（Classic 墙背光面）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 4"), HDescriptionLanguage("自定义颜色4（Classic 墙背光面）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[181, 181, 181]")]
        public Color Color4 { get => _color4; set { _color4 = value; Invalidate(); } }
        /// <summary>自定义颜色5（Classic 墙体浅黄）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 5"), HDescriptionLanguage("自定义颜色5（Classic 墙体浅黄）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[255, 222, 173]")]
        public Color Color5 { get => _color5; set { _color5 = value; Invalidate(); } }
        /// <summary>自定义颜色6（Classic 墙体深黄）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 6"), HDescriptionLanguage("自定义颜色6（Classic 墙体深黄）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[233, 201, 142]")]
        public Color Color6 { get => _color6; set { _color6 = value; Invalidate(); } }
        /// <summary>自定义颜色7（Classic 大门）。</summary>
        [HCategoryLanguage("工厂"), HDisplayNameLanguage("颜色 7"), HDescriptionLanguage("自定义颜色7（Classic 大门）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[158, 158, 158]")]
        public Color Color7 { get => _color7; set { _color7 = value; Invalidate(); } }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (Width < 8 || Height < 8) return;
            if (_orientation == HBarOrientation.Horizontal)
                PaintAll(g, Width, Height);
            else
            {
                g.TranslateTransform(Width, 0f);
                g.RotateTransform(90f);
                PaintAll(g, Height, Width);
                g.ResetTransform();
            }
        }
        private void PaintAll(Graphics g, float width, float height)
        {
            if (_style == HFactoryStyle.Classic) PaintClassic(g, width, height);
            else PaintModern(g, new RectangleF(0f, 0f, width, height));
        }
        // ------------------------------------------------------------------
        // Classic：旧版 HFrom.HFactory 原样移植（等距屋顶 4 点×3 面/墙体/天窗/门）
        // ------------------------------------------------------------------
        private void PaintClassic(Graphics g, float width, float height)
        {
            // Classic 色调沿用旧版等距七色；其他色调映射到现代调色板（自定义 Color1..7 仅 Classic 色调生效）
            bool themed = _theme != HFactoryTheme.Classic;
            HToolPalette pal = themed ? HToolPalettes.Get((int)_theme) : default(HToolPalette);
            Color c1 = !themed ? _color1 : pal.Light;        // 屋顶受光面
            Color c2 = !themed ? _color2 : pal.Main;         // 屋顶背光面
            Color c3 = !themed ? _color3 : pal.Lighter;      // 墙受光面
            Color c4 = !themed ? _color4 : pal.GlassEdge;    // 墙背光面
            Color c5 = !themed ? _color5 : pal.Lighter;      // 墙正面
            Color c6 = !themed ? _color6 : pal.Light;        // 墙侧面
            Color c7 = !themed ? _color7 : pal.Accent;       // 大门
            using (var b1 = new SolidBrush(c1))
            using (var b2 = new SolidBrush(c2))
            using (var b3 = new SolidBrush(c3))
            using (var b4 = new SolidBrush(c4))
            using (var b5 = new SolidBrush(c5))
            using (var b6 = new SolidBrush(c6))
            using (var b7 = new SolidBrush(c7))
            {
                PointF p1 = new PointF(width * 0.2f, 0f);
                PointF p2 = new PointF(width * 0.4f, 0f);
                PointF p3 = new PointF(width - 1f, height * 0.35f);
                PointF p4 = new PointF(width * 0.8f, height * 0.35f);
                PointF p5 = new PointF(0f, height * 0.25f);
                PointF p6 = new PointF(width * 0.6f, height * 0.6f);
                PointF p7 = new PointF(p5.X, p5.Y + height * 0.03f);
                PointF p8 = new PointF(p6.X, p6.Y + height * 0.03f);
                PointF p9 = new PointF(p3.X, p3.Y + height * 0.03f);
                PointF p10 = new PointF(p4.X, p4.Y + height * 0.03f);
                PointF p11 = new PointF(width * 0.03f, p7.Y);
                PointF p12 = new PointF(p6.X + width * 0.03f, p3.Y);
                PointF p13 = new PointF(p6.X + width * 0.03f, height - 1f);
                PointF p14 = new PointF(width * 0.03f, height * 0.65f);
                PointF p15 = new PointF(width * 0.97f, height * 0.35f);
                PointF p16 = new PointF(width * 0.97f, height * 0.75f);
                g.FillPolygon(b5, new[] { p11, p12, p13, p14 });
                g.FillPolygon(b6, new[] { p12, p13, p16, p15 });
                g.FillPolygon(b2, new[] { p1, p2, p3, p4 });
                g.FillPolygon(b1, new[] { p1, p4, p6, p5 });
                g.FillPolygon(b3, new[] { p5, p6, p8, p7 });
                g.FillPolygon(b4, new[] { p6, p4, p3, p9, p10, p8 });
                PointF[] wins =
                {
                    new PointF(width * 0.12f, height * 0.46f),
                    new PointF(width * 0.12f, height * 0.54f),
                    new PointF(width * 0.22f, height * 0.6f),
                    new PointF(width * 0.22f, height * 0.52f)
                };
                g.FillPolygon(b1, wins);
                AddOffset(wins, width * 0.14f, height * 0.08f);
                g.FillPolygon(b1, wins);
                AddOffset(wins, width * 0.14f, height * 0.08f);
                g.FillPolygon(b1, wins);
                wins = new[]
                {
                    new PointF(width * 0.72f, height * 0.934f),
                    new PointF(width * 0.91f, height * 0.8f),
                    new PointF(width * 0.91f, height * 0.6f),
                    new PointF(width * 0.72f, height * 0.734f)
                };
                g.FillPolygon(b7, wins);
            }
        }
        /// <summary>等距天窗坐标平移（旧版 AddOffect 同名笔误，保持几何一致）。</summary>
        private static void AddOffset(PointF[] points, float x, float y)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] = new PointF(points[i].X + x, points[i].Y + y);
        }
        // ------------------------------------------------------------------
        // 现代样式（横向画布 c）
        // ------------------------------------------------------------------
        private HToolPalette Palette => _theme == HFactoryTheme.Classic
            ? HToolPalettes.Get(1)
            : HToolPalettes.Get((int)_theme);
        private static readonly Color WinColor = Color.FromArgb(74, 108, 132);
        private static readonly Color GroundColor = Color.FromArgb(150, 156, 162);
        private void PaintModern(Graphics g, RectangleF c)
        {
            var pal = Palette;
            // 地面线
            using (var gp = new Pen(GroundColor, Math.Max(1.2f, c.Height * 0.012f)))
                g.DrawLine(gp, c.X + c.Width * 0.02f, c.Bottom - c.Height * 0.045f,
                    c.Right - c.Width * 0.02f, c.Bottom - c.Height * 0.045f);
            switch (_style)
            {
                case HFactoryStyle.FlatRoof: DrawFlatRoof(g, c, pal); break;
                case HFactoryStyle.GableFactory: DrawGable(g, c, pal, false); break;
                case HFactoryStyle.SawTooth: DrawSawTooth(g, c, pal); break;
                case HFactoryStyle.ChimneyWorks: DrawGable(g, c, pal, true); break;
                case HFactoryStyle.CoolingTower: DrawCoolingTower(g, c, c, pal); break;
                case HFactoryStyle.SiloFarm: DrawSiloFarm(g, c, pal); break;
                case HFactoryStyle.SolarFactory: DrawSolar(g, c, pal); break;
                case HFactoryStyle.GlassTower: DrawGlassTower(g, c, pal); break;
                case HFactoryStyle.Warehouse: DrawWarehouse(g, c, pal); break;
                case HFactoryStyle.Refinery: DrawRefinery(g, c, pal); break;
                case HFactoryStyle.Hangar: DrawHangar(g, c, pal); break;
                case HFactoryStyle.DomePlant: DrawDome(g, c, pal); break;
                case HFactoryStyle.OfficeBlock: DrawOffice(g, c, pal); break;
                case HFactoryStyle.WorkshopSkylight: DrawWorkshop(g, c, pal); break;
                case HFactoryStyle.TankFarm: DrawTankFarm(g, c, pal); break;
                case HFactoryStyle.WindmillPlant: DrawWindmill(g, c, pal); break;
                case HFactoryStyle.HydroDam: DrawDam(g, c, pal); break;
                case HFactoryStyle.NuclearPlant: DrawNuclear(g, c, pal); break;
                case HFactoryStyle.Greenhouse: DrawGreenhouse(g, c, pal); break;
                case HFactoryStyle.ThermalPlant: DrawThermal(g, c, pal); break;
                case HFactoryStyle.ContainerYard: DrawContainerYard(g, c, pal); break;
            }
        }
        /// <summary>实心矩形。</summary>
        private static void Rect(Graphics g, float x, float y, float w, float h, Color color)
        {
            using (var b = new SolidBrush(color))
                g.FillRectangle(b, x, y, w, h);
        }
        /// <summary>描边矩形。</summary>
        private static void RectE(Graphics g, float x, float y, float w, float h, Color color, float lw = 1.2f)
        {
            using (var p = new Pen(color, lw))
                g.DrawRectangle(p, x, y, w, h);
        }
        /// <summary>实心多边形。</summary>
        private static void Poly(Graphics g, PointF[] pts, Color color)
        {
            using (var b = new SolidBrush(color))
                g.FillPolygon(b, pts);
        }
        /// <summary>窗格阵列。</summary>
        private static void WinGrid(Graphics g, RectangleF r, int cols, int rows, Color win, Color sep)
        {
            Rect(g, r.X, r.Y, r.Width, r.Height, win);
            using (var p = new Pen(sep, Math.Max(0.8f, r.Height * 0.015f)))
            {
                for (int i = 1; i < cols; i++)
                    g.DrawLine(p, r.X + r.Width * i / cols, r.Y, r.X + r.Width * i / cols, r.Bottom);
                for (int j = 1; j < rows; j++)
                    g.DrawLine(p, r.X, r.Y + r.Height * j / rows, r.Right, r.Y + r.Height * j / rows);
            }
        }
        /// <summary>平顶厂房：女儿墙 + 采光窗 + 大门。</summary>
        private void DrawFlatRoof(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.12f, w = c.Width * 0.76f, y = c.Height * 0.34f, h = c.Height * 0.56f, gy = c.Bottom - c.Height * 0.05f;
            Rect(g, x, y, w, gy - y, pal.Lighter);
            RectE(g, x, y, w, gy - y, pal.Edge);
            Rect(g, x - c.Width * 0.01f, y - c.Height * 0.025f, w + c.Width * 0.02f, c.Height * 0.03f, pal.Dark);
            WinGrid(g, new RectangleF(x + w * 0.06f, y + h * 0.10f, w * 0.88f, h * 0.26f), 6, 2, WinColor, pal.Lighter);
            Rect(g, x + w * 0.40f, y + h * 0.46f, w * 0.20f, h * 0.54f - (gy - y - h), pal.Accent);
            RectE(g, x + w * 0.40f, y + h * 0.46f, w * 0.20f, gy - (y + h * 0.46f), pal.Dark);
        }
        /// <summary>双坡厂房（withChimney=true 时右侧加烟囱冒烟）。</summary>
        private void DrawGable(Graphics g, RectangleF c, HToolPalette pal, bool withChimney)
        {
            float x = c.Width * 0.14f, w = c.Width * 0.60f, baseY = c.Height * 0.44f, gy = c.Bottom - c.Height * 0.05f;
            // 墙体
            Rect(g, x, baseY, w, gy - baseY, pal.Lighter);
            RectE(g, x, baseY, w, gy - baseY, pal.Edge);
            // 三角山墙
            Poly(g, new[] { new PointF(x - w * 0.08f, baseY), new PointF(x + w / 2f, c.Height * 0.12f), new PointF(x + w * 1.08f, baseY) },
                HToolPalettes.Tint(pal.Main, 0.8f));
            // 坡顶（受光/背光两面）
            float ridgeX = x + w / 2f, ridgeY = c.Height * 0.12f, eaveL = x - w * 0.08f, eaveR = x + w * 1.08f;
            Poly(g, new[] { new PointF(eaveL, baseY), new PointF(ridgeX, ridgeY), new PointF(ridgeX, ridgeY - c.Height * 0.03f), new PointF(eaveL, baseY - c.Height * 0.03f) }, pal.Main);
            Poly(g, new[] { new PointF(ridgeX, ridgeY), new PointF(eaveR, baseY), new PointF(eaveR, baseY - c.Height * 0.03f), new PointF(ridgeX, ridgeY - c.Height * 0.03f) }, pal.Dark);
            // 大门 + 窗
            Rect(g, x + w * 0.38f, baseY + (gy - baseY) * 0.32f, w * 0.24f, (gy - baseY) * 0.68f, pal.Accent);
            WinGrid(g, new RectangleF(x + w * 0.07f, baseY + (gy - baseY) * 0.14f, w * 0.20f, (gy - baseY) * 0.30f), 2, 2, WinColor, pal.Lighter);
            WinGrid(g, new RectangleF(x + w * 0.73f, baseY + (gy - baseY) * 0.14f, w * 0.20f, (gy - baseY) * 0.30f), 2, 2, WinColor, pal.Lighter);
            if (withChimney) DrawChimney(g, c, pal, c.Width * 0.80f, c.Height * 0.42f, c.Width * 0.05f, c.Height * 0.36f, true);
        }
        /// <summary>锯齿屋顶厂房：三座锯齿（竖面玻璃 + 斜面彩钢）。</summary>
        private void DrawSawTooth(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.10f, w = c.Width * 0.80f, baseY = c.Height * 0.56f, gy = c.Bottom - c.Height * 0.05f;
            Rect(g, x, baseY, w, gy - baseY, pal.Lighter);
            RectE(g, x, baseY, w, gy - baseY, pal.Edge);
            int n = 3;
            float toothW = w / n;
            for (int i = 0; i < n; i++)
            {
                float lx = x + toothW * i;
                PointF a = new PointF(lx, baseY), b = new PointF(lx, c.Height * 0.32f),
                    d2 = new PointF(lx + toothW, baseY), cc = new PointF(lx + toothW, c.Height * 0.40f);
                Poly(g, new[] { a, b, cc, d2 }, HToolPalettes.Tint(pal.Main, 0.82f));
                // 垂直玻璃面
                Poly(g, new[] { b, new PointF(lx + toothW * 0.22f, c.Height * 0.30f), cc, b }, WinColor);
                using (var p = new Pen(pal.Edge, 1.1f))
                    g.DrawPolygon(p, new[] { a, b, cc, d2 });
            }
            Rect(g, x + w * 0.40f, baseY + (gy - baseY) * 0.34f, w * 0.20f, (gy - baseY) * 0.66f, pal.Accent);
        }
        /// <summary>烟囱 + 烟团。</summary>
        private void DrawChimney(Graphics g, RectangleF c, HToolPalette pal, float x, float y, float w, float h, bool smoke)
        {
            var stk = new RectangleF(x, y, w, h);
            using (var b = HBarBase.CylinderH(stk, HToolPalettes.Shade(pal.Main, 0.25f), HToolPalettes.Tint(pal.Main, 0.4f)))
                g.FillRectangle(b, stk);
            RectE(g, x, y, w, h, pal.Edge);
            Rect(g, x - w * 0.12f, y - h * 0.04f, w * 1.24f, h * 0.05f, pal.Dark);
            using (var p = new Pen(HToolPalettes.Tint(pal.Dark, 0.4f), 1f))
                g.DrawLine(p, x + w * 0.2f, y + h * 0.3f, x + w * 0.2f, y + h);
            if (!smoke) return;
            using (var sb = new SolidBrush(Color.FromArgb(150, 214, 218, 224)))
                for (int i = 0; i < 3; i++)
                {
                    float r = w * (0.55f + i * 0.28f);
                    g.FillEllipse(sb, x - w * 0.2f - i * w * 0.35f, y - h * 0.16f - i * h * 0.10f - r * 0.4f, r, r);
                }
        }
        /// <summary>双曲线冷却塔（按 bounds 布局，供独立样式与火电厂组合复用）。</summary>
        private void DrawCoolingTower(Graphics g, RectangleF c, RectangleF bounds, HToolPalette pal)
        {
            float cx = bounds.X + bounds.Width * 0.5f, topY = bounds.Y + bounds.Height * 0.10f, topW = bounds.Width * 0.30f;
            float botY = bounds.Bottom - bounds.Height * 0.06f, botW = bounds.Width * 0.44f,
                waistY = bounds.Y + bounds.Height * 0.52f, waistW = bounds.Width * 0.20f;
            using (var path = new GraphicsPath())
            {
                path.AddBezier(cx - topW / 2f, topY, cx - waistW / 2f, waistY, cx - waistW / 2f, waistY, cx - botW / 2f, botY);
                path.AddLine(cx - botW / 2f, botY, cx + botW / 2f, botY);
                path.AddBezier(cx + botW / 2f, botY, cx + waistW / 2f, waistY, cx + waistW / 2f, waistY, cx + topW / 2f, topY);
                path.CloseFigure();
                using (var b = HBarBase.CylinderH(new RectangleF(cx - botW / 2f, topY, botW, botY - topY),
                    HToolPalettes.Shade(pal.Lighter, 0.08f), Color.White))
                    g.FillPath(b, path);
                using (var p = new Pen(pal.Edge, 1.3f))
                    g.DrawPath(p, path);
            }
            // 顶口椭圆
            using (var b = new SolidBrush(HToolPalettes.Shade(pal.Lighter, 0.2f)))
                g.FillEllipse(b, cx - topW / 2f, topY - bounds.Height * 0.03f, topW, bounds.Height * 0.06f);
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawEllipse(p, cx - topW / 2f, topY - bounds.Height * 0.03f, topW, bounds.Height * 0.06f);
            // 水汽
            using (var sb = new SolidBrush(Color.FromArgb(140, 230, 234, 238)))
                for (int i = 0; i < 3; i++)
                {
                    float r = bounds.Width * (0.05f + i * 0.025f);
                    g.FillEllipse(sb, cx - r / 2f + (i - 1) * bounds.Width * 0.05f, topY - r - i * bounds.Height * 0.03f, r, r);
                }
            // 底部支柱 + 水池
            for (int i = 0; i < 5; i++)
                Rect(g, cx - botW * 0.40f + botW * 0.80f * i / 4f - bounds.Width * 0.006f, botY - bounds.Height * 0.06f,
                    bounds.Width * 0.012f, bounds.Height * 0.07f, pal.Edge);
            Rect(g, cx - botW / 2f, botY + bounds.Height * 0.005f, botW, bounds.Height * 0.025f, HToolPalettes.Tint(pal.Main, 0.35f));
        }
        /// <summary>筒仓群：三座立筒 + 提升塔。</summary>
        private void DrawSiloFarm(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            float siloW = c.Width * 0.17f;
            float[] xs = { c.Width * 0.24f, c.Width * 0.425f, c.Width * 0.61f };
            for (int i = 0; i < 3; i++)
            {
                float x = xs[i], top = c.Height * (i == 1 ? 0.18f : 0.26f), h = gy - top;
                var body = new RectangleF(x, top + h * 0.10f, siloW, h * 0.90f);
                using (var b = HBarBase.CylinderH(body, pal.Main, HToolPalettes.Tint(pal.Main, 0.65f)))
                    g.FillRectangle(b, body);
                RectE(g, x, top + h * 0.10f, siloW, h * 0.90f, pal.Edge);
                // 锥顶
                Poly(g, new[]
                {
                    new PointF(x - siloW * 0.06f, top + h * 0.10f),
                    new PointF(x + siloW / 2f, top),
                    new PointF(x + siloW * 1.06f, top + h * 0.10f)
                }, pal.Dark);
                // 检修环
                using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.4f))
                    g.DrawEllipse(p, x, top + h * 0.35f, siloW, h * 0.06f);
            }
            // 提升塔
            Rect(g, c.Width * 0.80f, c.Height * 0.22f, c.Width * 0.06f, gy - c.Height * 0.22f, pal.MetalDark);
            Rect(g, c.Width * 0.795f, c.Height * 0.18f, c.Width * 0.07f, c.Height * 0.05f, pal.Dark);
        }
        /// <summary>太阳能屋顶厂：平顶 + 深蓝电池板阵列。</summary>
        private void DrawSolar(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.12f, w = c.Width * 0.76f, y = c.Height * 0.40f, gy = c.Bottom - c.Height * 0.05f;
            Rect(g, x, y, w, gy - y, pal.Lighter);
            RectE(g, x, y, w, gy - y, pal.Edge);
            Rect(g, x - w * 0.02f, y - c.Height * 0.03f, w * 1.04f, c.Height * 0.035f, pal.Dark);
            WinGrid(g, new RectangleF(x + w * 0.06f, y + (gy - y) * 0.2f, w * 0.88f, (gy - y) * 0.28f), 5, 2, WinColor, pal.Lighter);
            // 屋顶斜向电池板（三排）
            Color panel = Color.FromArgb(33, 58, 96);
            for (int i = 0; i < 3; i++)
            {
                var pr = new RectangleF(x + w * (0.08f + i * 0.30f), c.Height * 0.26f, w * 0.24f, c.Height * 0.10f);
                using (var p = new Pen(pal.Edge, 1f))
                {
                    g.FillRectangle(new SolidBrush(panel), pr.X, pr.Y, pr.Width, pr.Height);
                    for (int k = 1; k < 4; k++)
                        g.DrawLine(p, pr.X + pr.Width * k / 4f, pr.Y, pr.X + pr.Width * k / 4f, pr.Bottom);
                    g.DrawLine(p, pr.X, pr.Y + pr.Height / 2f, pr.Right, pr.Y + pr.Height / 2f);
                }
            }
        }
        /// <summary>玻璃幕墙高楼 + 裙房。</summary>
        private void DrawGlassTower(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 裙房
            Rect(g, c.Width * 0.16f, c.Height * 0.62f, c.Width * 0.68f, gy - c.Height * 0.62f, HToolPalettes.Tint(pal.Main, 0.72f));
            RectE(g, c.Width * 0.16f, c.Height * 0.62f, c.Width * 0.68f, gy - c.Height * 0.62f, pal.Edge);
            // 塔楼
            var tower = new RectangleF(c.Width * 0.34f, c.Height * 0.08f, c.Width * 0.32f, c.Height * 0.56f);
            WinGrid(g, tower, 5, 10, Color.FromArgb(58, 90, 118), HToolPalettes.Tint(pal.Main, 0.6f));
            RectE(g, tower.X, tower.Y, tower.Width, tower.Height, pal.Dark, 1.4f);
            // 屋顶设备间 + 天线
            Rect(g, tower.X + tower.Width * 0.36f, tower.Y - c.Height * 0.045f, tower.Width * 0.28f, c.Height * 0.045f, pal.MetalDark);
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawLine(p, tower.X + tower.Width * 0.5f, tower.Y - c.Height * 0.045f, tower.X + tower.Width * 0.5f, c.Height * 0.01f);
        }
        /// <summary>物流仓库：卷闸门 + 装卸月台雨棚。</summary>
        private void DrawWarehouse(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.08f, w = c.Width * 0.84f, y = c.Height * 0.30f, gy = c.Bottom - c.Height * 0.05f;
            Rect(g, x, y, w, gy - y, pal.Lighter);
            RectE(g, x, y, w, gy - y, pal.Edge);
            Rect(g, x, y, w, c.Height * 0.05f, pal.Dark);
            // 三个卷闸门（横纹）
            for (int i = 0; i < 3; i++)
            {
                float dx = x + w * (0.06f + i * 0.31f), dw = w * 0.24f;
                Rect(g, dx, y + c.Height * 0.12f, dw, gy - (y + c.Height * 0.12f), HToolPalettes.Tint(pal.Main, 0.5f));
                using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 1f))
                    for (int k = 1; k < 8; k++)
                        g.DrawLine(p, dx, y + c.Height * 0.12f + (gy - y - c.Height * 0.12f) * k / 8f,
                            dx + dw, y + c.Height * 0.12f + (gy - y - c.Height * 0.12f) * k / 8f);
            }
            // 月台雨棚
            Poly(g, new[]
            {
                new PointF(x, y + c.Height * 0.10f), new PointF(x + w, y + c.Height * 0.10f),
                new PointF(x + w, y + c.Height * 0.16f), new PointF(x, y + c.Height * 0.13f)
            }, pal.Accent);
        }
        /// <summary>炼油装置：精馏塔 + 平台 + 球罐 + 管廊。</summary>
        private void DrawRefinery(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 精馏塔
            float tx = c.Width * 0.42f, tw = c.Width * 0.12f, ty = c.Height * 0.08f;
            var tower = new RectangleF(tx, ty, tw, gy - ty);
            using (var b = HBarBase.CylinderH(tower, HToolPalettes.Shade(pal.Main, 0.12f), HToolPalettes.Tint(pal.Main, 0.6f)))
                g.FillRectangle(b, tower);
            RectE(g, tx, ty, tw, gy - ty, pal.Edge);
            using (var b = new SolidBrush(pal.Dark))
                g.FillEllipse(b, tx, ty - c.Height * 0.025f, tw, c.Height * 0.05f);
            // 三层平台
            using (var bp = new Pen(pal.Dark, 1.2f))
                for (int i = 0; i < 3; i++)
                {
                    float py = c.Height * (0.26f + i * 0.20f);
                    g.DrawLine(bp, tx - tw * 0.35f, py, tx + tw * 1.35f, py);
                    g.DrawLine(bp, tx - tw * 0.35f, py, tx - tw * 0.35f, py - c.Height * 0.04f);
                    g.DrawLine(bp, tx + tw * 1.35f, py, tx + tw * 1.35f, py - c.Height * 0.04f);
                }
            // 塔顶管
            using (var pp = new Pen(pal.MetalDark, Math.Max(2f, c.Width * 0.012f)))
            {
                g.DrawLine(pp, tx + tw / 2f, ty, tx + tw / 2f, ty - c.Height * 0.05f);
                g.DrawLine(pp, tx + tw / 2f, ty - c.Height * 0.05f, tx + tw * 1.6f, ty - c.Height * 0.05f);
            }
            // 球罐
            float sx = c.Width * 0.76f, sr = c.Width * 0.11f, sy = gy - sr * 1.25f;
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.55f)))
                g.FillEllipse(b, sx - sr, sy - sr, sr * 2f, sr * 2f);
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawEllipse(p, sx - sr, sy - sr, sr * 2f, sr * 2f);
            Rect(g, sx - sr * 0.5f, sy + sr * 0.7f, sr, gy - (sy + sr * 0.7f), pal.MetalDark);
            // 管廊
            using (var pp = new Pen(pal.MetalDark, c.Width * 0.018f))
            {
                g.DrawLine(pp, c.Width * 0.10f, gy - c.Height * 0.16f, c.Width * 0.90f, gy - c.Height * 0.16f);
                g.DrawLine(pp, c.Width * 0.10f, gy - c.Height * 0.11f, c.Width * 0.90f, gy - c.Height * 0.11f);
            }
        }
        /// <summary>拱形机库。</summary>
        private void DrawHangar(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.12f, w = c.Width * 0.76f, gy = c.Bottom - c.Height * 0.05f;
            float archTop = c.Height * 0.12f, springY = c.Height * 0.42f;
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, archTop, w, (springY - archTop) * 2f, 180f, 180f);
                path.AddLine(x + w, springY, x + w, gy);
                path.AddLine(x + w, gy, x, gy);
                path.AddLine(x, gy, x, springY);
                path.CloseFigure();
                using (var b = new SolidBrush(pal.Lighter))
                    g.FillPath(b, path);
                using (var p = new Pen(pal.Edge, 1.4f))
                    g.DrawPath(p, path);
            }
            // 拱内分瓣大门
            float doorW = w * 0.72f, doorX = x + (w - doorW) / 2f;
            Rect(g, doorX, springY, doorW, gy - springY, HToolPalettes.Tint(pal.Main, 0.45f));
            using (var p = new Pen(pal.Dark, 1.1f))
                for (int i = 1; i < 6; i++)
                    g.DrawLine(p, doorX + doorW * i / 6f, springY, doorX + doorW * i / 6f, gy);
            using (var ap = new Pen(pal.Edge, 1.3f))
                g.DrawArc(ap, doorX, archTop + (springY - archTop) * 0.55f, doorW,
                    (springY - archTop) * 0.9f, 180f, 180f);
        }
        /// <summary>穹顶厂房。</summary>
        private void DrawDome(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.18f, w = c.Width * 0.64f, wallY = c.Height * 0.44f, gy = c.Bottom - c.Height * 0.05f;
            Rect(g, x, wallY, w, gy - wallY, pal.Lighter);
            RectE(g, x, wallY, w, gy - wallY, pal.Edge);
            float domeY = c.Height * 0.12f;
            using (var b = new SolidBrush(pal.Main))
                g.FillEllipse(b, x, domeY, w, (wallY - domeY) * 2f);
            using (var p = new Pen(pal.Dark, 1.3f))
                g.DrawArc(p, x, domeY, w, (wallY - domeY) * 2f, 180f, 180f);
            // 穹顶分瓣线
            using (var p = new Pen(HToolPalettes.Tint(pal.Dark, 0.25f), 1f))
                for (int i = 1; i < 5; i++)
                {
                    float dx = x + w * i / 5f;
                    g.DrawLine(p, dx, wallY, x + w / 2f, domeY);
                }
            Rect(g, x + w * 0.38f, wallY + (gy - wallY) * 0.25f, w * 0.24f, (gy - wallY) * 0.75f, pal.Accent);
        }
        /// <summary>退台办公楼。</summary>
        private void DrawOffice(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            var tiers = new[]
            {
                new RectangleF(c.Width * 0.14f, c.Height * 0.60f, c.Width * 0.72f, gy - c.Height * 0.60f),
                new RectangleF(c.Width * 0.22f, c.Height * 0.38f, c.Width * 0.56f, c.Height * 0.22f),
                new RectangleF(c.Width * 0.32f, c.Height * 0.18f, c.Width * 0.36f, c.Height * 0.20f)
            };
            for (int i = 0; i < 3; i++)
            {
                WinGrid(g, tiers[i], 6 - i * 2, 3, Color.FromArgb(64, 96, 124), HToolPalettes.Tint(pal.Main, 0.65f));
                RectE(g, tiers[i].X, tiers[i].Y, tiers[i].Width, tiers[i].Height, pal.Dark);
                // 露台绿带
                Rect(g, tiers[i].X, tiers[i].Bottom - c.Height * 0.018f, tiers[i].Width, c.Height * 0.018f,
                    HToolPalettes.Tint(pal.Main, 0.35f));
            }
        }
        /// <summary>天窗通风车间。</summary>
        private void DrawWorkshop(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.08f, w = c.Width * 0.84f, y = c.Height * 0.42f, gy = c.Bottom - c.Height * 0.05f;
            Rect(g, x, y, w, gy - y, pal.Lighter);
            RectE(g, x, y, w, gy - y, pal.Edge);
            // 平屋顶 + 长采光带
            Rect(g, x - w * 0.02f, y - c.Height * 0.04f, w * 1.04f, c.Height * 0.045f, pal.Dark);
            WinGrid(g, new RectangleF(x + w * 0.10f, y - c.Height * 0.034f, w * 0.80f, c.Height * 0.03f), 8, 1,
                Color.FromArgb(96, 140, 168), pal.Dark);
            // 三个通风帽
            for (int i = 0; i < 3; i++)
            {
                float vx = x + w * (0.22f + i * 0.28f);
                Rect(g, vx - c.Width * 0.012f, y - c.Height * 0.10f, c.Width * 0.024f, c.Height * 0.06f, pal.MetalDark);
                using (var b = new SolidBrush(pal.Dark))
                    g.FillEllipse(b, vx - c.Width * 0.026f, y - c.Height * 0.12f, c.Width * 0.052f, c.Height * 0.035f);
            }
            WinGrid(g, new RectangleF(x + w * 0.06f, y + (gy - y) * 0.14f, w * 0.88f, (gy - y) * 0.32f), 8, 2, WinColor, pal.Lighter);
            Rect(g, x + w * 0.42f, y + (gy - y) * 0.52f, w * 0.16f, (gy - y) * 0.48f, pal.Accent);
        }
        /// <summary>储罐区：两座拱顶罐 + 防火堤 + 扶梯。</summary>
        private void DrawTankFarm(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 防火堤
            Rect(g, c.Width * 0.08f, gy - c.Height * 0.06f, c.Width * 0.84f, c.Height * 0.025f, HToolPalettes.Shade(pal.Main, 0.18f));
            float tankW = c.Width * 0.22f, tankH = c.Height * 0.56f;
            foreach (float tx in new[] { c.Width * 0.18f, c.Width * 0.58f })
            {
                float ty = gy - c.Height * 0.06f - tankH;
                var body = new RectangleF(tx, ty, tankW, tankH);
                using (var b = HBarBase.CylinderH(body, pal.Main, HToolPalettes.Tint(pal.Main, 0.68f)))
                    g.FillRectangle(b, body);
                RectE(g, tx, ty, tankW, tankH, pal.Edge);
                // 拱顶
                using (var b = new SolidBrush(pal.Dark))
                    g.FillEllipse(b, tx, ty - tankH * 0.07f, tankW, tankH * 0.14f);
                // 环箍
                using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1f))
                    for (int i = 1; i < 4; i++)
                        g.DrawLine(p, tx, ty + tankH * i / 4f, tx + tankW, ty + tankH * i / 4f);
                // 外扶梯
                using (var p = new Pen(pal.Edge, 1.1f))
                {
                    g.DrawLine(p, tx + tankW * 0.9f, ty, tx + tankW * 1.02f, ty - tankH * 0.08f);
                    g.DrawLine(p, tx + tankW * 1.02f, ty - tankH * 0.08f, tx + tankW * 1.02f, gy - c.Height * 0.06f);
                }
            }
        }
        /// <summary>风电厂房：厂房 + 三叶片风机。</summary>
        private void DrawWindmill(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 小厂房
            float x = c.Width * 0.10f, w = c.Width * 0.42f, y = c.Height * 0.58f;
            Rect(g, x, y, w, gy - y, pal.Lighter);
            RectE(g, x, y, w, gy - y, pal.Edge);
            Poly(g, new[]
            {
                new PointF(x - w * 0.05f, y), new PointF(x + w / 2f, c.Height * 0.44f),
                new PointF(x + w * 1.05f, y)
            }, pal.Main);
            WinGrid(g, new RectangleF(x + w * 0.1f, y + (gy - y) * 0.16f, w * 0.35f, (gy - y) * 0.26f), 3, 2, WinColor, pal.Lighter);
            // 塔筒
            float hx = c.Width * 0.74f, hubY = c.Height * 0.20f;
            Poly(g, new[]
            {
                new PointF(hx - c.Width * 0.012f, hubY), new PointF(hx + c.Width * 0.012f, hubY),
                new PointF(hx + c.Width * 0.026f, gy - c.Height * 0.05f), new PointF(hx - c.Width * 0.026f, gy - c.Height * 0.05f)
            }, pal.MetalDark);
            // 机舱 + 三叶片
            Rect(g, hx - c.Width * 0.05f, hubY - c.Height * 0.035f, c.Width * 0.07f, c.Height * 0.04f, pal.Dark);
            float blade = c.Height * 0.30f;
            using (var bp = new Pen(HToolPalettes.Tint(pal.Main, 0.55f), c.Width * 0.018f))
                for (int i = 0; i < 3; i++)
                {
                    double a = -Math.PI / 2d + i * Math.PI * 2d / 3d + 0.25d;
                    g.DrawLine(bp, hx, hubY, hx + (float)Math.Cos(a) * blade, hubY + (float)Math.Sin(a) * blade);
                }
            using (var b = new SolidBrush(pal.Edge))
                g.FillEllipse(b, hx - c.Width * 0.022f, hubY - c.Width * 0.022f, c.Width * 0.044f, c.Width * 0.044f);
        }
        /// <summary>水电站大坝。</summary>
        private void DrawDam(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 上游水面
            Rect(g, 0f, gy - c.Height * 0.14f, c.Width * 0.30f, c.Height * 0.12f, HToolPalettes.Tint(pal.Main, 0.55f));
            // 坝体（梯形）
            Poly(g, new[]
            {
                new PointF(c.Width * 0.30f, c.Height * 0.20f),
                new PointF(c.Width * 0.78f, c.Height * 0.30f),
                new PointF(c.Width * 0.86f, gy),
                new PointF(c.Width * 0.30f, gy)
            }, HToolPalettes.Tint(pal.Main, 0.72f));
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawPolygon(p, new[]
                {
                    new PointF(c.Width * 0.30f, c.Height * 0.20f),
                    new PointF(c.Width * 0.78f, c.Height * 0.30f),
                    new PointF(c.Width * 0.86f, gy),
                    new PointF(c.Width * 0.30f, gy)
                });
            // 坝顶公路
            using (var p = new Pen(pal.Dark, 2f))
                g.DrawLine(p, c.Width * 0.30f, c.Height * 0.20f, c.Width * 0.78f, c.Height * 0.30f);
            // 闸门楼
            Rect(g, c.Width * 0.48f, c.Height * 0.10f, c.Width * 0.08f, c.Height * 0.14f, pal.Lighter);
            RectE(g, c.Width * 0.48f, c.Height * 0.10f, c.Width * 0.08f, c.Height * 0.14f, pal.Edge);
            // 下泄水流
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.4f)))
                g.FillPolygon(b, new[]
                {
                    new PointF(c.Width * 0.56f, c.Height * 0.42f),
                    new PointF(c.Width * 0.72f, c.Height * 0.46f),
                    new PointF(c.Width * 0.80f, gy),
                    new PointF(c.Width * 0.60f, gy)
                });
        }
        /// <summary>核电站：安全壳 + 汽机房。</summary>
        private void DrawNuclear(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 汽机房
            Rect(g, c.Width * 0.08f, c.Height * 0.58f, c.Width * 0.44f, gy - c.Height * 0.58f, pal.Lighter);
            RectE(g, c.Width * 0.08f, c.Height * 0.58f, c.Width * 0.44f, gy - c.Height * 0.58f, pal.Edge);
            WinGrid(g, new RectangleF(c.Width * 0.10f, c.Height * 0.64f, c.Width * 0.40f, c.Height * 0.12f), 6, 2, WinColor, pal.Lighter);
            // 安全壳
            float cx = c.Width * 0.70f, w = c.Width * 0.26f, cylTop = c.Height * 0.34f;
            Rect(g, cx - w / 2f, cylTop, w, gy - cylTop, HToolPalettes.Tint(pal.Main, 0.8f));
            float domeH = c.Height * 0.26f;
            using (var b = new SolidBrush(pal.Main))
                g.FillEllipse(b, cx - w / 2f, cylTop - domeH, w, domeH * 2f);
            using (var p = new Pen(pal.Edge, 1.3f))
            {
                g.DrawEllipse(p, cx - w / 2f, cylTop - domeH, w, domeH * 2f);
                g.DrawLine(p, cx - w / 2f, cylTop, cx - w / 2f, gy);
                g.DrawLine(p, cx + w / 2f, cylTop, cx + w / 2f, gy);
                // 环梁
                g.DrawEllipse(p, cx - w * 0.28f, cylTop + (gy - cylTop) * 0.55f, w * 0.56f, (gy - cylTop) * 0.10f);
            }
        }
        /// <summary>玻璃温室。</summary>
        private void DrawGreenhouse(Graphics g, RectangleF c, HToolPalette pal)
        {
            float x = c.Width * 0.10f, w = c.Width * 0.80f, gy = c.Bottom - c.Height * 0.05f;
            float eaveY = c.Height * 0.52f, ridgeY = c.Height * 0.20f, ridgeX = x + w / 2f;
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(new[]
                {
                    new PointF(x, eaveY), new PointF(ridgeX, ridgeY), new PointF(x + w, eaveY),
                    new PointF(x + w, gy), new PointF(x, gy)
                });
                using (var b = new SolidBrush(Color.FromArgb(110, HToolPalettes.Tint(pal.Main, 0.62f))))
                    g.FillPath(b, path);
                using (var p = new Pen(pal.Edge, 1.4f))
                    g.DrawPath(p, path);
                // 坡顶与墙面分格
                using (var gp = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 1f))
                {
                    for (int i = 1; i < 8; i++)
                    {
                        float xx = x + w * i / 8f;
                        g.DrawLine(gp, xx, gy, xx, eaveY);
                        g.DrawLine(gp, xx, eaveY, ridgeX, ridgeY);
                    }
                    g.DrawLine(gp, x, eaveY, x + w, eaveY);
                }
            }
            // 苗床色块
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.25f)))
                for (int i = 0; i < 3; i++)
                    g.FillRectangle(b, x + w * (0.08f + i * 0.30f), gy - c.Height * 0.10f, w * 0.22f, c.Height * 0.05f);
        }
        /// <summary>火力发电厂：主厂房 + 冷却塔 + 烟囱。</summary>
        private void DrawThermal(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 主厂房
            Rect(g, c.Width * 0.06f, c.Height * 0.52f, c.Width * 0.40f, gy - c.Height * 0.52f, pal.Lighter);
            RectE(g, c.Width * 0.06f, c.Height * 0.52f, c.Width * 0.40f, gy - c.Height * 0.52f, pal.Edge);
            WinGrid(g, new RectangleF(c.Width * 0.08f, c.Height * 0.58f, c.Width * 0.36f, c.Height * 0.14f), 6, 2, WinColor, pal.Lighter);
            // 输煤廊
            using (var p = new Pen(pal.Dark, c.Width * 0.025f))
                g.DrawLine(p, c.Width * 0.02f, c.Height * 0.46f, c.Width * 0.46f, c.Height * 0.40f);
            // 小冷却塔（右侧局部区域）
            DrawCoolingTower(g, c, new RectangleF(c.Width * 0.54f, c.Height * 0.04f, c.Width * 0.42f, c.Height * 0.92f), pal);
            // 烟囱
            DrawChimney(g, c, pal, c.Width * 0.46f, c.Height * 0.16f, c.Width * 0.05f, gy - c.Height * 0.16f, true);
        }
        /// <summary>集装箱堆场：龙门吊 + 彩色箱子。</summary>
        private void DrawContainerYard(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.05f;
            // 两排集装箱（每排 3 个）
            Color[] boxColors = { pal.Main, pal.Accent, HToolPalettes.Tint(pal.Main, 0.35f),
                pal.Dark, HToolPalettes.Tint(pal.Accent, 0.4f), pal.Light };
            float bw = c.Width * 0.16f, bh = c.Height * 0.16f;
            for (int row = 0; row < 2; row++)
                for (int col = 0; col < 3; col++)
                {
                    float bx = c.Width * 0.10f + col * (bw + c.Width * 0.02f);
                    float by = gy - bh - row * (bh + c.Height * 0.012f);
                    Rect(g, bx, by, bw, bh, boxColors[row * 3 + col]);
                    RectE(g, bx, by, bw, bh, pal.Dark, 1f);
                    using (var p = new Pen(HToolPalettes.Tint(boxColors[row * 3 + col], 0.5f), 0.9f))
                        for (int k = 1; k < 5; k++)
                            g.DrawLine(p, bx + bw * k / 5f, by, bx + bw * k / 5f, by + bh);
                }
            // 龙门吊架
            float gx0 = c.Width * 0.06f, gx1 = c.Width * 0.70f, topY = c.Height * 0.10f;
            using (var p = new Pen(pal.MetalDark, c.Width * 0.016f))
            {
                g.DrawLine(p, gx0, topY, gx0, gy);
                g.DrawLine(p, gx1, topY, gx1, gy);
                g.DrawLine(p, gx0, topY, gx1, topY);
                // 斜撑
                g.DrawLine(p, gx0, topY + c.Height * 0.10f, gx0 + c.Width * 0.05f, gy);
                g.DrawLine(p, gx1, topY + c.Height * 0.10f, gx1 - c.Width * 0.05f, gy);
            }
            // 横梁纹 + 小车吊具
            for (int i = 0; i < 8; i++)
                Rect(g, gx0 + (gx1 - gx0) * i / 8f, topY - c.Height * 0.012f, (gx1 - gx0) / 16f, c.Height * 0.012f, pal.Dark);
            Rect(g, c.Width * 0.40f, topY - c.Height * 0.025f, c.Width * 0.05f, c.Height * 0.035f, pal.Accent);
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawLine(p, c.Width * 0.425f, topY + c.Height * 0.01f, c.Width * 0.425f, topY + c.Height * 0.12f);
        }
    }
}