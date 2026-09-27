using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Vessel
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 实验室玻璃器皿样式：Classic 为带倾倒嘴与刻度的玻璃烧杯，其余 21 种覆盖
    /// 锥形瓶、圆底/平底烧瓶、容量瓶、量筒、试剂瓶、培养皿、漏斗、冷凝管、试管等。
    /// </summary>
    public enum HBeakerStyle
    {
        Classic = 0,        // 带嘴刻度烧杯
        Erlenmeyer = 1,     // 锥形瓶
        RoundBottom = 2,    // 圆底烧瓶
        Volumetric = 3,     // 容量瓶
        Cylinder = 4,       // 量筒
        Reagent = 5,        // 磨口试剂瓶
        Petri = 6,          // 培养皿
        Florence = 7,       // 平底烧瓶
        Funnel = 8,         // 三角漏斗
        Separatory = 9,     // 分液漏斗
        Condenser = 10,     // 冷凝管
        TestTube = 11,      // 试管
        TubeRack = 12,      // 试管架
        Dropper = 13,       // 胶头滴管
        Burette = 14,       // 滴定管
        Kjeldahl = 15,      // 凯氏烧瓶
        Buchner = 16,       // 布氏漏斗
        WashBottle = 17,    // 洗瓶
        Vial = 18,          // 样品瓶
        WatchGlass = 19,    // 表面皿
        EvapDish = 20,      // 蒸发皿
        GasJar = 21         // 集气瓶
    }
    /// <summary>
    /// 烧杯控件（继承 HBarBase）：22 种实验室玻璃器皿外形、13 种色调，
    /// Value 为液位百分比（0..100），液体用 BarColor 着色，玻璃半透明高光。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [HDescriptionLanguage("玻璃器皿控件：22 种实验器皿外形、13 种色调，Value 为液位百分比")]
    public class HBeaker : HBarBase
    {
        private HToolTheme _theme = HToolTheme.Classic;
        private HBeakerStyle _style = HBeakerStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        public HBeaker()
        {
            Size = new Size(100, 120);
            Value = 50;
            BarColor = Color.FromArgb(72, 156, 224);
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("烧杯"), HDisplayNameLanguage("玻璃器皿色调"), HDescriptionLanguage("玻璃器皿色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>玻璃器皿外形样式。</summary>
        [HCategoryLanguage("烧杯"), HDisplayNameLanguage("玻璃器皿外形样式"), HDescriptionLanguage("玻璃器皿外形样式"), Browsable(true)]
        [DefaultValue(HBeakerStyle.Classic)]
        public HBeakerStyle BeakerStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        private HToolPalette Pal => _theme == HToolTheme.Classic ? _classic : HToolPalettes.Get((int)_theme);
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Pal;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HBeakerStyle.Erlenmeyer: DrawErlenmeyer(g, a, pal); break;
                case HBeakerStyle.RoundBottom: DrawBulb(g, a, pal, false, 0.28f, 0.27f); break;
                case HBeakerStyle.Volumetric: DrawVolumetric(g, a, pal); break;
                case HBeakerStyle.Cylinder: DrawCylinder(g, a, pal); break;
                case HBeakerStyle.Reagent: DrawReagent(g, a, pal); break;
                case HBeakerStyle.Petri: DrawPetri(g, a, pal); break;
                case HBeakerStyle.Florence: DrawFlorence(g, a, pal); break;
                case HBeakerStyle.Funnel: DrawFunnel(g, a, pal); break;
                case HBeakerStyle.Separatory: DrawSeparatory(g, a, pal); break;
                case HBeakerStyle.Condenser: DrawCondenser(g, a, pal); break;
                case HBeakerStyle.TestTube: DrawSingleTube(g, a, pal); break;
                case HBeakerStyle.TubeRack: DrawTubeRack(g, a, pal); break;
                case HBeakerStyle.Dropper: DrawDropper(g, a, pal); break;
                case HBeakerStyle.Burette: DrawBurette(g, a, pal); break;
                case HBeakerStyle.Kjeldahl: DrawBulb(g, a, pal, false, 0.4f, 0.25f); break;
                case HBeakerStyle.Buchner: DrawBuchner(g, a, pal); break;
                case HBeakerStyle.WashBottle: DrawWashBottle(g, a, pal); break;
                case HBeakerStyle.Vial: DrawVial(g, a, pal); break;
                case HBeakerStyle.WatchGlass: DrawWatch(g, a, pal); break;
                case HBeakerStyle.EvapDish: DrawEvapDish(g, a, pal); break;
                case HBeakerStyle.GasJar: DrawGasJar(g, a, pal); break;
                default: DrawClassic(g, a); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：带倾倒嘴与刻度的玻璃烧杯（已验收绘法，原样保留）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width;
            float glyphH = a.Height;
            float w = Width * 0.62f;
            float top = glyphH * 0.1f, bot = glyphH * 0.92f;
            float lx = (Width - w) / 2f, rx = lx + w;
            float lip = 7f, rim = 5f;
            // 杯身轮廓（左上口带倾倒嘴）
            var spoutTip = new PointF(lx - lip, top - 6f);
            var leftRim = new PointF(lx, top + 1f);
            using (var glass = new GraphicsPath())
            {
                glass.StartFigure();
                glass.AddLine(spoutTip, leftRim);
                glass.AddLine(leftRim, new PointF(lx + 2f, top + lip));
                glass.AddLine(new PointF(lx + 2f, top + lip), new PointF(lx + 2f, bot - rim));
                glass.AddArc(lx + 2f, bot - rim * 2f, rim * 2f, rim * 2f, 90f, 90f);
                glass.AddLine(new PointF(lx + 2f + rim, bot), new PointF(rx - 2f - rim, bot));
                glass.AddArc(rx - 2f - rim * 2f, bot - rim * 2f, rim * 2f, rim * 2f, 0f, 90f);
                glass.AddLine(new PointF(rx - 2f, bot - rim), new PointF(rx - 2f, top + lip));
                glass.AddLine(new PointF(rx - 2f, top + lip), new PointF(rx, top));
                glass.AddLine(new PointF(rx, top), spoutTip);
                glass.CloseFigure();
                var old = g.Clip;
                g.SetClip(glass);
                // 液体
                float p = Percent;
                float liqTop = bot - rim - p * (bot - rim - (top + lip + 3f));
                var lr = new RectangleF(lx - 2f, liqTop, w + 4f, bot - liqTop);
                using (var lb = new LinearGradientBrush(lr, Color.FromArgb(120, HToolPalettes.Tint(BarColor, 0.45f)),
                    Color.FromArgb(200, BarColor), 90f))
                    g.FillRectangle(lb, lr);
                // 液面线
                if (p > 0.01f)
                    g.DrawLine(new Pen(Color.FromArgb(160, HToolPalettes.Shade(BarColor, 0.3f)), 1.4f),
                        lx + 1f, liqTop, rx - 1f, liqTop);
                g.Clip = old;
                // 玻璃底
                using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                    g.FillPath(gb, glass);
                g.DrawPath(new Pen(Color.FromArgb(140, 160, 175), 1.6f), glass);
            }
            // 刻度线
            using (var tick = new Pen(Color.FromArgb(150, 110, 125, 140), 1.2f))
            {
                for (int i = 1; i < 5; i++)
                {
                    float y = bot - rim - (bot - rim - top - lip) * i / 5f;
                    g.DrawLine(tick, rx - 9f, y, rx - (i == 1 || i == 4 ? 1f : 4f), y);
                }
            }
            // 玻璃高光
            using (var hl = new Pen(Color.FromArgb(120, Color.White), 2.2f))
                g.DrawLine(hl, lx + w * 0.16f, top + lip + 4f, lx + w * 0.16f, bot - rim - 3f);
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // 1 锥形瓶：细颈 + 锥身 + 圆底角
        // ----------------------------------------------------------------
        private void DrawErlenmeyer(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 8f, ny = top + a.Height * 0.2f;
            float by = top + a.Height * 0.42f, bot = a.Bottom - 8f;
            using (var path = ConeFlask(cx, a.Width * 0.055f, top, ny,
                a.Width * 0.3f, by, bot, 8f))
            {
                Vessel(g, path, new RectangleF(cx - a.Width * 0.3f + 3f, ny + 2f,
                    a.Width * 0.6f - 6f, bot - ny - 12f), pal);
            }
            Mouth(g, cx, top, a.Width * 0.055f, pal.GlassEdge);
            HL(g, cx - a.Width * 0.16f, by + 6f, bot - 12f);
        }
        // ----------------------------------------------------------------
        // 2/15 细颈球瓶：圆底（长颈比例由参数控制，供圆底烧瓶/凯氏瓶复用）
        // ----------------------------------------------------------------
        private void DrawBulb(Graphics g, RectangleF a, HToolPalette pal, bool flat,
            float neckRatio, float radiusRatio)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 6f;
            float nHalf = a.Width * 0.06f, ny = top + a.Height * neckRatio;
            float r = a.Width * radiusRatio;
            float bcy = a.Bottom - 8f - r;
            using (var path = BulbFlask(cx, nHalf, top, ny, bcy, r, flat ? a.Bottom - 8f : 0f))
            {
                Vessel(g, path, new RectangleF(cx - r + 2f, bcy - r + 2f,
                    r * 2f - 4f, r * 2f - 4f), pal);
            }
            Mouth(g, cx, top, nHalf, pal.GlassEdge);
            HL(g, cx - r * 0.55f, bcy - r * 0.5f, bcy + r * 0.35f);
        }
        // ----------------------------------------------------------------
        // 3 容量瓶：细长颈（环形刻度）+ 梨形腹
        // ----------------------------------------------------------------
        private void DrawVolumetric(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 6f, ny = top + a.Height * 0.42f;
            float shY = ny + a.Height * 0.08f, bellyY = top + a.Height * 0.68f;
            float bot = a.Bottom - 8f, bh = a.Width * 0.27f, cr = 10f;
            float nHalf = a.Width * 0.035f, sHalf = bh * 0.7f;
            using (var p = new GraphicsPath())
            {
                p.StartFigure();
                p.AddLine(cx - nHalf, top, cx + nHalf, top);
                p.AddLine(cx + nHalf, top, cx + nHalf, ny);
                p.AddLine(cx + nHalf, ny, cx + sHalf, shY);
                p.AddLine(cx + sHalf, shY, cx + bh, bellyY);
                p.AddLine(cx + bh, bellyY, cx + bh * 0.92f, bot - cr);
                p.AddArc(cx + bh * 0.92f - cr * 2f, bot - cr * 2f, cr * 2f, cr * 2f, 0f, 90f);
                p.AddLine(cx + bh * 0.92f - cr, bot, cx - bh * 0.92f + cr, bot);
                p.AddArc(cx - bh * 0.92f, bot - cr * 2f, cr * 2f, cr * 2f, 90f, 90f);
                p.AddLine(cx - bh * 0.92f, bot - cr, cx - bh, bellyY);
                p.AddLine(cx - bh, bellyY, cx - sHalf, shY);
                p.AddLine(cx - sHalf, shY, cx - nHalf, ny);
                p.AddLine(cx - nHalf, ny, cx - nHalf, top);
                p.CloseFigure();
                Vessel(g, p, new RectangleF(cx - bh + 3f, shY + 2f,
                    bh * 2f - 6f, bot - shY - cr - 4f), pal);
            }
            // 颈部环形刻度
            g.Line(pal.Edge, 1.3f, cx - nHalf - 3f, top + a.Height * 0.24f,
                cx + nHalf + 3f, top + a.Height * 0.24f);
            Mouth(g, cx, top, nHalf, pal.GlassEdge);
        }
        // ----------------------------------------------------------------
        // 4 量筒：高直筒 + 六角底座 + 十档刻度
        // ----------------------------------------------------------------
        private void DrawCylinder(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 8f, lip = 6f, rim = 3f;
            float tw = a.Width * 0.32f, lx = cx - tw / 2f, rx = cx + tw / 2f;
            float bot = a.Bottom - a.Height * 0.13f;
            using (var p = new GraphicsPath())
            {
                // 上口微倾倾倒嘴
                p.StartFigure();
                p.AddLine(lx - 5f, top - 4f, lx, top + 1f);
                p.AddLine(lx, top + 1f, lx + 2f, top + lip);
                p.AddLine(lx + 2f, top + lip, lx + 2f, bot - rim);
                p.AddArc(lx + 2f, bot - rim * 2f, rim * 2f, rim * 2f, 90f, 90f);
                p.AddLine(lx + 2f + rim, bot, rx - 2f - rim, bot);
                p.AddArc(rx - 2f - rim * 2f, bot - rim * 2f, rim * 2f, rim * 2f, 0f, 90f);
                p.AddLine(rx - 2f, bot - rim, rx - 2f, top);
                p.AddLine(rx - 2f, top, lx - 5f, top - 4f);
                p.CloseFigure();
                Vessel(g, p, new RectangleF(lx + 1f, top + lip + 2f, tw - 3f,
                    bot - top - lip - rim - 3f), pal);
            }
            ScaleTicks(g, rx - 1f, top + lip + 6f, bot - 6f, 10, pal.Edge);
            HL(g, lx + tw * 0.22f, top + lip + 5f, bot - 6f);
            // 六角宽底座
            g.Poly(new[]
            {
                new PointF(lx - 7f, bot - 2f), new PointF(rx + 7f, bot - 2f),
                new PointF(rx + 13f, bot + 11f), new PointF(lx - 13f, bot + 11f)
            }, Color.FromArgb(180, pal.Glass), pal.GlassEdge, 1.3f);
        }
        // ----------------------------------------------------------------
        // 5 磨口试剂瓶：宽肩瓶身 + 玻璃塞
        // ----------------------------------------------------------------
        private void DrawReagent(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 12f, ny = top + a.Height * 0.1f;
            float shY = top + a.Height * 0.2f, bot = a.Bottom - 8f;
            float nHalf = a.Width * 0.09f, bHalf = a.Width * 0.27f, cr = 8f;
            using (var p = ShoulderedBottle(cx, nHalf, top, ny, bHalf, shY, bot, cr))
            {
                Vessel(g, p, new RectangleF(cx - bHalf + 3f, shY + 2f,
                    bHalf * 2f - 6f, bot - shY - cr - 3f), pal);
            }
            // 磨口玻璃塞
            g.Box(new RectangleF(cx - nHalf - 3f, top - 9f, nHalf * 2f + 6f, 11f), 2.5f,
                Color.FromArgb(200, pal.Glass), pal.GlassEdge, 1.3f);
            HL(g, cx - bHalf * 0.6f, shY + 8f, bot - 12f);
        }
        // ----------------------------------------------------------------
        // 6 培养皿：浅圆盘 + 略大的皿盖，液位为浅层培养基
        // ----------------------------------------------------------------
        private void DrawPetri(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float cy = a.Top + a.Height * 0.52f, rx = a.Width * 0.3f, ry = a.Height * 0.09f;
            var dish = new RectangleF(cx - rx, cy - ry, rx * 2f, ry * 2f);
            using (var ep = new GraphicsPath())
            {
                ep.AddEllipse(dish);
                var old = g.Clip;
                g.SetClip(ep);
                // 浅层培养基（最多占盘深 45%）
                if (Percent > 0.01f)
                {
                    float top = cy + ry - Percent * ry * 0.9f;
                    var lr = new RectangleF(dish.X, top, dish.Width, cy + ry - top);
                    using (var lb = new LinearGradientBrush(lr,
                        Color.FromArgb(110, HToolPalettes.Tint(BarColor, 0.5f)),
                        Color.FromArgb(180, BarColor), 90f))
                        g.FillRectangle(lb, lr);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                    g.FillEllipse(gb, dish);
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                    g.DrawEllipse(gp, dish);
            }
            // 皿盖（大一圈、高 3px，只描可见弧边）
            using (var lid = new Pen(HToolPalettes.Tint(pal.GlassEdge, 0.2f), 1.3f))
                g.DrawArc(lid, dish.X - 4f, dish.Y - 4f, dish.Width + 8f, dish.Height + 4f, 185f, 170f);
        }
        // ----------------------------------------------------------------
        // 7 平底烧瓶：球形腹切平底
        // ----------------------------------------------------------------
        private void DrawFlorence(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 8f, nHalf = a.Width * 0.06f, ny = top + a.Height * 0.28f;
            float r = a.Width * 0.27f, flatBot = a.Bottom - 8f;
            float bcy = flatBot - r * 1.05f;
            using (var path = BulbFlask(cx, nHalf, top, ny, bcy, r, flatBot))
            {
                Vessel(g, path, new RectangleF(cx - r + 2f, bcy - r + 2f,
                    r * 2f - 4f, r * 2f - 6f), pal);
            }
            Mouth(g, cx, top, nHalf, pal.GlassEdge);
            HL(g, cx - r * 0.55f, bcy - r * 0.5f, bcy + r * 0.3f);
        }
        // ----------------------------------------------------------------
        // 8 三角漏斗：锥形斗 + 宽口沿 + 短颈管
        // ----------------------------------------------------------------
        private void DrawFunnel(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 12f, half = a.Width * 0.3f;
            float coneBot = top + a.Height * 0.5f, sw = a.Width * 0.06f;
            float stemBot = a.Bottom - 6f;
            var cone = new[]
            {
                new PointF(cx - half, top), new PointF(cx + half, top),
                new PointF(cx, coneBot)
            };
            // 锥内液体
            using (var cp = new GraphicsPath())
            {
                cp.AddPolygon(cone);
                cp.CloseFigure();
                var old = g.Clip;
                g.SetClip(cp);
                if (Percent > 0.01f)
                {
                    float fy = coneBot - Percent * (coneBot - top - 6f);
                    var lr = new RectangleF(cx - half, fy, half * 2f, coneBot - fy);
                    using (var lb = new LinearGradientBrush(lr,
                        Color.FromArgb(120, HToolPalettes.Tint(BarColor, 0.45f)),
                        Color.FromArgb(190, BarColor), 90f))
                        g.FillRectangle(lb, lr);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                    g.FillPolygon(gb, cone);
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                {
                    g.DrawLine(gp, cx - half, top, cx, coneBot);
                    g.DrawLine(gp, cx, coneBot, cx + half, top);
                }
            }
            // 口沿 + 颈管
            using (var rim = new Pen(pal.GlassEdge, 1.5f))
                g.DrawEllipse(rim, cx - half, top - 3f, half * 2f, 6f);
            using (var stem = new GraphicsPath())
            {
                stem.AddRectangle(new RectangleF(cx - sw, coneBot - 4f, sw * 2f, stemBot - coneBot + 8f));
                using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                    g.FillPath(gb, stem);
                using (var gp = new Pen(pal.GlassEdge, 1.5f))
                    g.DrawPath(gp, stem);
            }
        }
        // ----------------------------------------------------------------
        // 9 分液漏斗：梨形腹 + 玻璃塞 + 活塞 + 长茎
        // ----------------------------------------------------------------
        private void DrawSeparatory(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 12f, ny = top + a.Height * 0.08f;
            float shY = ny + a.Height * 0.08f, bellyY = top + a.Height * 0.44f;
            float tapY = top + a.Height * 0.64f, stemBot = a.Bottom - 6f;
            float nHalf = a.Width * 0.045f, bHalf = a.Width * 0.25f, sHalf = bHalf * 0.72f;
            var bulb = new[]
            {
                new PointF(cx - nHalf, ny), new PointF(cx + nHalf, ny),
                new PointF(cx + sHalf, shY), new PointF(cx + bHalf, bellyY),
                new PointF(cx + nHalf, tapY), new PointF(cx - nHalf, tapY),
                new PointF(cx - bHalf, bellyY), new PointF(cx - sHalf, shY)
            };
            using (var bp = new GraphicsPath())
            using (var sp = new GraphicsPath())
            {
                bp.AddPolygon(bulb);
                bp.CloseFigure();
                sp.StartFigure();
                sp.AddRectangle(new RectangleF(cx - nHalf, tapY - 2f, nHalf * 2f, stemBot - tapY + 6f));
                var old = g.Clip;
                g.SetClip(bp);
                if (Percent > 0.01f)
                {
                    float fy = tapY - Percent * (tapY - shY);
                    var lr = new RectangleF(cx - bHalf, fy, bHalf * 2f, tapY - fy);
                    using (var lb = new LinearGradientBrush(lr,
                        Color.FromArgb(120, HToolPalettes.Tint(BarColor, 0.45f)),
                        Color.FromArgb(200, BarColor), 90f))
                        g.FillRectangle(lb, lr);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                {
                    g.FillPath(gb, bp);
                    g.FillPath(gb, sp);
                }
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                {
                    g.DrawPath(gp, bp);
                    g.DrawPath(gp, sp);
                }
            }
            // 玻璃塞 + 活塞
            g.Box(new RectangleF(cx - nHalf - 3f, top - 6f, nHalf * 2f + 6f, 9f), 2f,
                Color.FromArgb(200, pal.Glass), pal.GlassEdge, 1.2f);
            g.Box(new RectangleF(cx - 7f, tapY - 4f, 14f, 8f), 2f, pal.Metal, pal.Edge, 1.2f);
            g.Line(pal.Edge, 1.6f, cx - 8f, tapY - 6f, cx + 8f, tapY - 6f);
        }
        // ----------------------------------------------------------------
        // 10 冷凝管：水平外水套 + 内管 + 上下水口，内管液体从左端进入
        // ----------------------------------------------------------------
        private void DrawCondenser(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.46f;
            var jacket = new RectangleF(a.Left + a.Width * 0.16f, cy - a.Height * 0.12f,
                a.Width * 0.68f, a.Height * 0.24f);
            // 水口（一上一下）
            g.Box(new RectangleF(jacket.Left + 6f, jacket.Top - 11f, 8f, 12f), 2f,
                Color.FromArgb(160, pal.Glass), pal.GlassEdge, 1.2f);
            g.Box(new RectangleF(jacket.Right - 14f, jacket.Bottom - 1f, 8f, 12f), 2f,
                Color.FromArgb(160, pal.Glass), pal.GlassEdge, 1.2f);
            // 水套中的冷却水（浅染）
            using (var jp = HBarBase.RoundPath(jacket, jacket.Height / 2f))
            {
                var old = g.Clip;
                g.SetClip(jp);
                using (var cb = new SolidBrush(Color.FromArgb(34, HToolPalettes.Tint(BarColor, 0.6f))))
                    g.FillRectangle(cb, jacket);
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                    g.FillPath(gb, jp);
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                    g.DrawPath(gp, jp);
            }
            // 内管（两端伸出水套并扩口）
            float iy = cy - 3.5f, iw = a.Width * 0.9f, ix = a.Left + a.Width * 0.05f;
            using (var ip = HBarBase.RoundPath(new RectangleF(ix, iy, iw, 7f), 3.5f))
            {
                var old = g.Clip;
                g.SetClip(ip);
                if (Percent > 0.01f)
                    using (var lb = new SolidBrush(Color.FromArgb(190, BarColor)))
                        g.FillRectangle(lb, ix, iy, iw * Percent, 7f);
                g.Clip = old;
                using (var gp = new Pen(pal.GlassEdge, 1.5f))
                    g.DrawPath(gp, ip);
            }
            // 两端扩口
            g.Line(pal.GlassEdge, 1.5f, ix, iy + 1f, ix - 5f, iy - 2f);
            g.Line(pal.GlassEdge, 1.5f, ix, iy + 6f, ix - 5f, iy + 9f);
            g.Line(pal.GlassEdge, 1.5f, ix + iw, iy + 1f, ix + iw + 5f, iy - 2f);
            g.Line(pal.GlassEdge, 1.5f, ix + iw, iy + 6f, ix + iw + 5f, iy + 9f);
        }
        // ----------------------------------------------------------------
        // 11 单支试管：圆底长管 + 翻口沿
        // ----------------------------------------------------------------
        private void DrawSingleTube(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 8f, tw = a.Width * 0.24f;
            float lx = cx - tw / 2f, straightBot = a.Bottom - a.Height * 0.14f;
            using (var p = RoundTube(lx, top, tw, straightBot))
            {
                Vessel(g, p, new RectangleF(lx + 2f, top + 5f, tw - 4f,
                    straightBot - top - 4f), pal);
            }
            using (var lip = new Pen(pal.GlassEdge, 1.6f))
                g.DrawEllipse(lip, lx - 2f, top - 3f, tw + 4f, 6f);
            HL(g, lx + tw * 0.24f, top + 7f, straightBot - 4f);
        }
        // ----------------------------------------------------------------
        // 12 试管架：上下层架板 + 三支试管
        // ----------------------------------------------------------------
        private void DrawTubeRack(Graphics g, RectangleF a, HToolPalette pal)
        {
            float topY = a.Bottom - a.Height * 0.3f;
            float botY = a.Bottom - a.Height * 0.12f;
            float tw = a.Width * 0.15f, straightBot = botY + a.Height * 0.03f;
            // 先画三支试管
            for (int k = -1; k <= 1; k++)
            {
                float tx = a.Left + a.Width / 2f + k * a.Width * 0.22f - tw / 2f;
                using (var p = RoundTube(tx, a.Top + 6f, tw, straightBot))
                {
                    Vessel(g, p, new RectangleF(tx + 2f, a.Top + 12f, tw - 4f,
                        straightBot - a.Top - 12f), pal);
                }
                using (var lip = new Pen(pal.GlassEdge, 1.3f))
                    g.DrawEllipse(lip, tx - 1f, a.Top + 4f, tw + 2f, 5f);
            }
            // 架板（遮挡试管中下部）+ 立柱
            Color rack = HToolPalettes.Shade(pal.Main, 0.22f);
            g.Box(new RectangleF(a.Left + a.Width * 0.08f, topY, a.Width * 0.84f, a.Height * 0.07f),
                3f, rack, pal.Edge, 1.3f);
            g.Box(new RectangleF(a.Left + a.Width * 0.1f, botY, a.Width * 0.8f, a.Height * 0.06f),
                3f, rack, pal.Edge, 1.3f);
            foreach (float px in new[] { a.Left + a.Width * 0.12f, a.Right - a.Width * 0.12f })
                g.Line(pal.Edge, 3.4f, px, topY, px, botY + a.Height * 0.06f);
        }
        // ----------------------------------------------------------------
        // 13 胶头滴管：橡胶吸头 + 细长玻璃管 + 尖嘴液柱
        // ----------------------------------------------------------------
        private void DrawDropper(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 8f, bx = a.Width * 0.09f, by = top + a.Height * 0.17f;
            // 橡胶吸头
            g.Ell(cx, top + a.Height * 0.08f, bx, a.Height * 0.09f, pal.Dark);
            using (var ep = new Pen(pal.Edge, 1.3f))
                g.DrawEllipse(ep, cx - bx, top - a.Height * 0.01f, bx * 2f, a.Height * 0.18f);
            // 玻璃管（下端收成尖嘴）
            float tw = a.Width * 0.05f, tip = a.Bottom - 6f;
            using (var p = new GraphicsPath())
            {
                p.StartFigure();
                p.AddLine(cx - tw, by, cx + tw, by);
                p.AddLine(cx + tw, by, cx + tw * 0.7f, tip - 8f);
                p.AddLine(cx + tw * 0.7f, tip - 8f, cx, tip);
                p.AddLine(cx, tip, cx - tw * 0.7f, tip - 8f);
                p.AddLine(cx - tw * 0.7f, tip - 8f, cx - tw, by);
                p.CloseFigure();
                var old = g.Clip;
                g.SetClip(p);
                // 尖嘴附近液柱（Value 0..50% 管长）
                if (Percent > 0.01f)
                {
                    float fh = (tip - by) * Math.Min(Percent, 0.6f) + 4f;
                    using (var lb = new SolidBrush(Color.FromArgb(190, BarColor)))
                        g.FillRectangle(lb, cx - tw, tip - fh, tw * 2f, fh);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                    g.FillPath(gb, p);
                using (var gp = new Pen(pal.GlassEdge, 1.5f))
                    g.DrawPath(gp, p);
            }
        }
        // ----------------------------------------------------------------
        // 14 滴定管：喇叭口 + 长刻度管 + 活塞 + 滴嘴（液位自下而上）
        // ----------------------------------------------------------------
        private void DrawBurette(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 6f, tw = a.Width * 0.1f;
            float lx = cx - tw / 2f, rx = cx + tw / 2f;
            float fBot = top + 8f, straightBot = top + a.Height * 0.7f;
            using (var p = new GraphicsPath())
            {
                // 喇叭口 + 直管 + 圆管底
                p.StartFigure();
                p.AddLine(lx - 5f, top, rx + 5f, top);
                p.AddLine(rx + 5f, top, rx + 5f, fBot);
                p.AddLine(rx + 5f, fBot, rx, fBot);
                p.AddLine(rx, fBot, rx, straightBot);
                p.AddArc(lx, straightBot - tw, tw, tw, 0f, 180f);
                p.AddLine(lx, straightBot, lx, fBot);
                p.AddLine(lx, fBot, lx - 5f, fBot);
                p.CloseFigure();
                Vessel(g, p, new RectangleF(lx + 1f, fBot + 2f, tw - 2f,
                    straightBot - fBot - 3f), pal);
            }
            ScaleTicks(g, rx + 6f, fBot + 6f, straightBot - 4f, 12, pal.Edge);
            // 活塞 + 滴嘴
            g.Box(new RectangleF(cx - 7f, straightBot - 3f, 14f, 8f), 2f, pal.Metal, pal.Edge, 1.2f);
            g.Line(pal.Edge, 1.8f, cx - 8f, straightBot - 5f, cx + 8f, straightBot - 5f);
            using (var tip = new Pen(pal.GlassEdge, 2.4f))
            {
                g.DrawLine(tip, cx - 3f, straightBot + 5f, cx, straightBot + 15f);
                g.DrawLine(tip, cx + 3f, straightBot + 5f, cx, straightBot + 15f);
            }
        }
        // ----------------------------------------------------------------
        // 16 布氏漏斗：宽直筒（多孔板）+ 锥斗 + 真空侧管
        // ----------------------------------------------------------------
        private void DrawBuchner(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 10f, cw = a.Width * 0.42f, ch = a.Height * 0.16f;
            float lx = cx - cw / 2f, rx = cx + cw / 2f, cupBot = top + ch;
            float coneBot = top + a.Height * 0.42f, sw = a.Width * 0.06f;
            // 筒内浅层料液
            using (var cup = new GraphicsPath())
            {
                cup.AddRectangle(new RectangleF(lx, top, cw, ch));
                cup.CloseFigure();
                var old = g.Clip;
                g.SetClip(cup);
                if (Percent > 0.01f)
                    using (var lb = new SolidBrush(Color.FromArgb(170, BarColor)))
                        g.FillRectangle(lb, lx, cupBot - Percent * ch * 0.6f, cw, ch);
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                    g.FillPath(gb, cup);
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                {
                    g.DrawPath(gp, cup);
                    // 锥斗 + 颈管
                    g.DrawLine(gp, lx + 3f, cupBot, cx - sw, coneBot);
                    g.DrawLine(gp, rx - 3f, cupBot, cx + sw, coneBot);
                    g.DrawRectangle(gp, cx - sw, coneBot - 3f, sw * 2f, a.Bottom - 6f - coneBot + 7f);
                }
            }
            // 多孔滤板
            using (var dot = new SolidBrush(Color.FromArgb(150, pal.Edge)))
                for (int i = -2; i <= 2; i++)
                    g.FillEllipse(dot, cx + i * cw / 5.4f - 1.6f, cupBot - 5f, 3.2f, 3.2f);
            // 真空侧管（斜上）
            g.Line(pal.GlassEdge, 2.6f, rx - 2f, cupBot + 2f, rx + 10f, cupBot - 12f);
            g.Line(pal.GlassEdge, 1.4f, rx + 6f, cupBot - 7f, rx + 14f, cupBot - 7f);
        }
        // ----------------------------------------------------------------
        // 17 洗瓶：圆角瓶身 + 斜口导流管
        // ----------------------------------------------------------------
        private void DrawWashBottle(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 8f, neckY = top + a.Height * 0.14f;
            float shY = neckY + a.Height * 0.1f, bot = a.Bottom - 8f;
            float nHalf = a.Width * 0.06f, bHalf = a.Width * 0.24f, cr = 10f;
            using (var p = ShoulderedBottle(cx, nHalf, neckY, shY, bHalf, shY + a.Height * 0.06f, bot, cr))
            {
                Vessel(g, p, new RectangleF(cx - bHalf + 3f, shY + a.Height * 0.08f,
                    bHalf * 2f - 6f, bot - shY - a.Height * 0.1f), pal);
            }
            // 瓶盖 + 导流管（内插到底、外弯斜下）
            g.Box(new RectangleF(cx - nHalf - 2f, neckY - 8f, nHalf * 2f + 4f, 9f), 2f,
                pal.Dark, pal.Edge, 1.2f);
            using (var tube = new Pen(pal.GlassEdge, 2f))
            {
                g.DrawLine(tube, cx - 2f, neckY - 4f, cx - 2f, shY + a.Height * 0.2f);
                g.DrawBezier(tube, cx - 2f, neckY - 4f, cx + bHalf * 0.6f, neckY - 2f,
                    cx + bHalf * 0.9f, neckY + 2f, cx + bHalf * 0.95f, neckY + a.Height * 0.12f);
            }
            HL(g, cx - bHalf * 0.6f, shY + a.Height * 0.12f, bot - 12f);
        }
        // ----------------------------------------------------------------
        // 18 样品瓶：短圆柱瓶身 + 宽瓶盖
        // ----------------------------------------------------------------
        private void DrawVial(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float bot = a.Bottom - 8f, bodyH = a.Height * 0.52f;
            float bodyTop = bot - bodyH, bHalf = a.Width * 0.2f;
            using (var p = new GraphicsPath())
            {
                p.StartFigure();
                p.AddArc(cx - bHalf, bodyTop, bHalf * 2f, 12f, 180f, 180f);
                p.AddLine(cx + bHalf, bodyTop + 6f, cx + bHalf, bot - 5f);
                p.AddArc(cx - bHalf, bot - 10f, bHalf * 2f, 10f, 0f, 90f);
                p.AddArc(cx - bHalf, bot - 10f, bHalf * 2f, 10f, 90f, 90f);
                p.AddLine(cx - bHalf, bodyTop + 6f, cx - bHalf, bodyTop + 6f);
                p.CloseFigure();
                Vessel(g, p, new RectangleF(cx - bHalf + 2f, bodyTop + 8f,
                    bHalf * 2f - 4f, bodyH - 12f), pal);
            }
            // 短颈 + 瓶盖
            g.Box(new RectangleF(cx - a.Width * 0.07f, bodyTop - 8f, a.Width * 0.14f, 9f), 2f,
                Color.FromArgb(200, pal.Glass), pal.GlassEdge, 1.2f);
            g.Box(new RectangleF(cx - a.Width * 0.11f, bodyTop - 20f, a.Width * 0.22f, 13f), 3f,
                pal.Dark, pal.Edge, 1.3f);
        }
        // ----------------------------------------------------------------
        // 19 表面皿：浅凹透镜片 + 微量液体
        // ----------------------------------------------------------------
        private void DrawWatch(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float cy = a.Top + a.Height * 0.55f, rx = a.Width * 0.32f, ry = a.Height * 0.07f;
            var lens = new RectangleF(cx - rx, cy - ry, rx * 2f, ry * 2f);
            using (var ep = new GraphicsPath())
            {
                ep.AddEllipse(lens);
                var old = g.Clip;
                g.SetClip(ep);
                if (Percent > 0.01f)
                {
                    float top = cy + ry - Percent * ry * 0.8f;
                    using (var lb = new SolidBrush(Color.FromArgb(150, BarColor)))
                        g.FillRectangle(lb, lens.X, top, lens.Width, cy + ry - top);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    g.FillEllipse(gb, lens);
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                {
                    g.DrawEllipse(gp, lens);
                    g.DrawArc(gp, lens.X + 2f, lens.Y + 2f, lens.Width - 4f, lens.Height - 2f, 200f, 140f);
                }
            }
        }
        // ----------------------------------------------------------------
        // 20 蒸发皿：半球碗 + 倾倒唇，液位在碗内
        // ----------------------------------------------------------------
        private void DrawEvapDish(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float r = a.Width * 0.3f, rimY = a.Top + a.Height * 0.42f;
            var bowl = new RectangleF(cx - r, rimY - r, r * 2f, r * 2f);
            using (var bp = new GraphicsPath())
            {
                bp.AddArc(bowl, 0f, 180f);
                bp.CloseFigure();
                var old = g.Clip;
                g.SetClip(bp);
                if (Percent > 0.01f)
                {
                    float top = rimY + r - Percent * r * 0.92f;
                    var lr = new RectangleF(cx - r, top, r * 2f, rimY + r - top);
                    using (var lb = new LinearGradientBrush(lr,
                        Color.FromArgb(110, HToolPalettes.Tint(BarColor, 0.5f)),
                        Color.FromArgb(190, BarColor), 90f))
                        g.FillRectangle(lb, lr);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                    g.FillPath(gb, bp);
                using (var gp = new Pen(pal.GlassEdge, 1.8f))
                    g.DrawArc(gp, bowl, 0f, 180f);
            }
            // 口沿线 + 倾倒唇 + 底足
            g.Line(pal.GlassEdge, 1.6f, cx - r, rimY, cx + r, rimY);
            g.Line(pal.GlassEdge, 1.5f, cx - r, rimY, cx - r - 6f, rimY - 5f);
            g.Ell(cx, rimY + r - 2f, r * 0.36f, 4f, Color.FromArgb(150, pal.Glass));
        }
        // ----------------------------------------------------------------
        // 21 集气瓶：直筒瓶 + 磨砂玻璃盖板，气体浅色填充
        // ----------------------------------------------------------------
        private void DrawGasJar(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + 12f, bot = a.Bottom - 8f, bHalf = a.Width * 0.24f, cr = 6f;
            using (var p = new GraphicsPath())
            {
                p.StartFigure();
                p.AddLine(cx - bHalf, top, cx + bHalf, top);
                p.AddLine(cx + bHalf, top, cx + bHalf, bot - cr);
                p.AddArc(cx + bHalf - cr * 2f, bot - cr * 2f, cr * 2f, cr * 2f, 0f, 90f);
                p.AddLine(cx + bHalf - cr, bot, cx - bHalf + cr, bot);
                p.AddArc(cx - bHalf, bot - cr * 2f, cr * 2f, cr * 2f, 90f, 90f);
                p.AddLine(cx - bHalf, bot - cr, cx - bHalf, top);
                p.CloseFigure();
                var old = g.Clip;
                g.SetClip(p);
                // 气体低透明度
                if (Percent > 0.01f)
                {
                    float top2 = bot - cr - Percent * (bot - cr - top - 3f);
                    var lr = new RectangleF(cx - bHalf, top2, bHalf * 2f, bot - top2);
                    using (var lb = new LinearGradientBrush(lr,
                        Color.FromArgb(26, BarColor), Color.FromArgb(70, BarColor), 90f))
                        g.FillRectangle(lb, lr);
                }
                g.Clip = old;
                using (var gb = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                    g.FillPath(gb, p);
                using (var gp = new Pen(pal.GlassEdge, 1.6f))
                    g.DrawPath(gp, p);
            }
            // 磨砂盖板（错位半搭）
            g.Box(new RectangleF(cx - bHalf - 5f, top - 6f, bHalf * 2f + 8f, 6f), 1.5f,
                Color.FromArgb(210, pal.Glass), pal.GlassEdge, 1.2f);
        }
        // ----------------------------------------------------------------
        // 共享原语
        // ----------------------------------------------------------------
        /// <summary>玻璃器皿统一渲染：按轮廓裁剪液位（BarColor），再罩玻璃色并描边。</summary>
        private void Vessel(Graphics g, GraphicsPath glass, RectangleF cavity, HToolPalette pal)
        {
            var old = g.Clip;
            g.SetClip(glass);
            if (Percent > 0.001f && cavity.Height > 0f)
            {
                float fillTop = cavity.Bottom - Percent * cavity.Height;
                var lr = new RectangleF(cavity.X, fillTop, cavity.Width, cavity.Bottom - fillTop + 1f);
                using (var lb = new LinearGradientBrush(lr,
                    Color.FromArgb(120, HToolPalettes.Tint(BarColor, 0.45f)),
                    Color.FromArgb(200, BarColor), 90f))
                    g.FillRectangle(lb, lr);
                using (var lp = new Pen(Color.FromArgb(160, HToolPalettes.Shade(BarColor, 0.3f)), 1.4f))
                    g.DrawLine(lp, cavity.X, fillTop, cavity.Right, fillTop);
            }
            g.Clip = old;
            using (var gb = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                g.FillPath(gb, glass);
            using (var gp = new Pen(pal.GlassEdge, 1.6f))
                g.DrawPath(gp, glass);
        }
        /// <summary>瓶口翻边椭圆口沿。</summary>
        private static void Mouth(Graphics g, float cx, float top, float nHalf, Color edge)
        {
            using (var rim = new Pen(edge, 1.5f))
                g.DrawEllipse(rim, cx - nHalf, top - 3f, nHalf * 2f, 6f);
            using (var b = new SolidBrush(Color.FromArgb(90, Color.White)))
                g.FillEllipse(b, cx - nHalf + 1f, top - 2f, nHalf * 2f - 2f, 4f);
        }
        /// <summary>玻璃竖面高光。</summary>
        private static void HL(Graphics g, float x, float y1, float y2)
        {
            if (y2 <= y1) return;
            using (var hl = new Pen(Color.FromArgb(110, Color.White), 2f))
                g.DrawLine(hl, x, y1, x, y2);
        }
        /// <summary>量筒/滴定管刻度：n 档，双数档为长刻线。</summary>
        private static void ScaleTicks(Graphics g, float rx, float y1, float y2, int n, Color c)
        {
            using (var tick = new Pen(Color.FromArgb(150, c), 1.1f))
                for (int i = 1; i < n; i++)
                {
                    float y = y2 - (y2 - y1) * i / (float)n;
                    g.DrawLine(tick, rx - 8f, y, rx - (i % 2 == 0 ? 1f : 4.5f), y);
                }
        }
        /// <summary>直筒圆底管（试管）轮廓：左右直壁 + 底部半圆。</summary>
        private static GraphicsPath RoundTube(float l, float top, float w, float straightBot)
        {
            float r = l + w;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddLine(l, top, r, top);
            p.AddLine(r, top, r, straightBot);
            p.AddArc(l, straightBot - w / 2f, w, w, 0f, 180f);
            p.AddLine(l, straightBot, l, top);
            p.CloseFigure();
            return p;
        }
        /// <summary>锥肩瓶（锥形瓶）轮廓：细颈 + 斜肩 + 直壁 + 圆角底。</summary>
        private static GraphicsPath ConeFlask(float cx, float nHalf, float top, float ny,
            float bHalf, float by, float bot, float cr)
        {
            float nL = cx - nHalf, nR = cx + nHalf, bL = cx - bHalf, bR = cx + bHalf;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddLine(nL, top, nR, top);
            p.AddLine(nR, top, nR, ny);
            p.AddLine(nR, ny, bR, by);
            p.AddLine(bR, by, bR, bot - cr);
            p.AddArc(bR - cr * 2f, bot - cr * 2f, cr * 2f, cr * 2f, 0f, 90f);
            p.AddLine(bR - cr, bot, bL + cr, bot);
            p.AddArc(bL, bot - cr * 2f, cr * 2f, cr * 2f, 90f, 90f);
            p.AddLine(bL, bot - cr, bL, by);
            p.AddLine(bL, by, nL, ny);
            p.AddLine(nL, ny, nL, top);
            p.CloseFigure();
            return p;
        }
        /// <summary>
        /// 细颈球瓶轮廓：颈部斜线接球面。flatBot 为 0 时圆底，大于 0 时球底在该高度切平。
        /// </summary>
        private static GraphicsPath BulbFlask(float cx, float nHalf, float top, float ny,
            float bcy, float r, float flatBot)
        {
            float nL = cx - nHalf, nR = cx + nHalf;
            double uR = -55.0 * Math.PI / 180.0, uL = -125.0 * Math.PI / 180.0;
            float jRX = cx + (float)Math.Cos(uR) * r, jRY = bcy + (float)Math.Sin(uR) * r;
            float jLX = cx + (float)Math.Cos(uL) * r, jLY = bcy + (float)Math.Sin(uL) * r;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddLine(nL, top, nR, top);
            p.AddLine(nR, top, nR, ny);
            p.AddLine(nR, ny, jRX, jRY);
            if (flatBot <= 0f)
            {
                // 圆底：右肩 305° 顺时针绕底至左肩 235°
                p.AddArc(cx - r, bcy - r, r * 2f, r * 2f, -55f, 290f);
            }
            else
            {
                // 平底：55°/125° 以下竖直切平
                double bR = 55.0 * Math.PI / 180.0, bL = 125.0 * Math.PI / 180.0;
                float bRX = cx + (float)Math.Cos(bR) * r, bRY = bcy + (float)Math.Sin(bR) * r;
                float bLX = cx + (float)Math.Cos(bL) * r, bLY = bcy + (float)Math.Sin(bL) * r;
                p.AddArc(cx - r, bcy - r, r * 2f, r * 2f, -55f, 110f);
                p.AddLine(bRX, bRY, bRX, flatBot);
                p.AddLine(bRX, flatBot, bLX, flatBot);
                p.AddLine(bLX, flatBot, bLX, bLY);
                p.AddArc(cx - r, bcy - r, r * 2f, r * 2f, 125f, 110f);
            }
            p.AddLine(jLX, jLY, nL, ny);
            p.AddLine(nL, ny, nL, top);
            p.CloseFigure();
            return p;
        }
        /// <summary>窄颈宽肩瓶（试剂瓶/洗瓶）轮廓。</summary>
        private static GraphicsPath ShoulderedBottle(float cx, float nHalf, float top, float ny,
            float bHalf, float shY, float bot, float cr)
        {
            float nL = cx - nHalf, nR = cx + nHalf, bL = cx - bHalf, bR = cx + bHalf;
            var p = new GraphicsPath();
            p.StartFigure();
            p.AddLine(nL, top, nR, top);
            p.AddLine(nR, top, nR, ny);
            p.AddLine(nR, ny, bR, shY);
            p.AddLine(bR, shY, bR, bot - cr);
            p.AddArc(bR - cr * 2f, bot - cr * 2f, cr * 2f, cr * 2f, 0f, 90f);
            p.AddLine(bR - cr, bot, bL + cr, bot);
            p.AddArc(bL, bot - cr * 2f, cr * 2f, cr * 2f, 90f, 90f);
            p.AddLine(bL, bot - cr, bL, shY);
            p.AddLine(bL, shY, nL, ny);
            p.AddLine(nL, ny, nL, top);
            p.CloseFigure();
            return p;
        }
    }
}