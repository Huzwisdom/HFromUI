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
    /// 花的种类（22 种）：玫瑰、郁金香、百合、向日葵、雏菊、康乃馨、牡丹、梅花、
    /// 樱花、荷花、兰花、菊花、水仙、鸢尾、风信子、蒲公英、牵牛花、三色堇、
    /// 绣球、薰衣草、桃花、山茶花。
    /// </summary>
    public enum HFlowerKind
    {
        Rose = 0,           // 玫瑰
        Tulip = 1,          // 郁金香
        Lily = 2,           // 百合
        Sunflower = 3,      // 向日葵
        Daisy = 4,          // 雏菊
        Carnation = 5,      // 康乃馨
        Peony = 6,          // 牡丹
        PlumBlossom = 7,    // 梅花
        Sakura = 8,         // 樱花
        Lotus = 9,          // 荷花
        Orchid = 10,        // 兰花
        Chrysanthemum = 11, // 菊花
        Narcissus = 12,     // 水仙
        Iris = 13,          // 鸢尾
        Hyacinth = 14,      // 风信子
        Dandelion = 15,     // 蒲公英
        MorningGlory = 16,  // 牵牛花
        Pansy = 17,         // 三色堇
        Hydrangea = 18,     // 绣球
        Lavender = 19,      // 薰衣草
        PeachBlossom = 20,  // 桃花
        Camellia = 21       // 山茶花
    }
    /// <summary>
    /// 鲜花样式（22 种构图/风格）：单枝、特写、花束、花瓶、盆栽、花环、藤条、
    /// 花田、双枝、徽章，以及线描、水墨、霓虹、鎏金、剪纸、水彩、彩窗、极简、
    /// 几何、浮雕、团花、心形花丛。
    /// </summary>
    public enum HFlowerStyle
    {
        Classic = 0,      // 单枝
        Closeup = 1,      // 花朵特写
        Bouquet = 2,      // 花束
        Vase = 3,         // 花瓶
        Pot = 4,          // 盆栽
        Wreath = 5,       // 花环
        Garland = 6,      // 藤条
        Field = 7,        // 花田
        Twin = 8,         // 双枝
        Badge = 9,        // 扁平圆徽章
        LineArt = 10,     // 线描
        InkWash = 11,     // 水墨
        Neon = 12,        // 霓虹
        GoldFoil = 13,    // 鎏金
        PaperCut = 14,    // 剪纸
        Watercolor = 15,  // 水彩
        StainedGlass = 16,// 彩窗
        Minimal = 17,     // 极简
        Geometric = 18,   // 几何
        Emboss = 19,      // 浮雕
        Ornament = 20,    // 团花纹样
        HeartBloom = 21   // 心形花丛
    }
    /// <summary>渲染风格（由 FlowerStyle 映射）。</summary>
    internal enum HFlowerRender
    {
        Normal, Line, Ink, Neon, Gold, Paper, Water, Glass, Minimal, Geo, Emboss
    }
    /// <summary>花瓣形状。</summary>
    internal enum FPetalShape
    {
        Round, Point, Tongue, Notched, Spiral, Cup, Fluff, Tube, BellSpike, Cluster, Orchid
    }
    /// <summary>花芯类型。</summary>
    internal enum FCenter
    {
        Dot, Disc, Cup, Tuft, None
    }
    internal struct FSpec
    {
        public byte Petals;
        public FPetalShape Shape;
        public Color Main;
        public Color Inner;
        public Color Center;
        public FCenter CenterKind;
        public FSpec(byte petals, FPetalShape shape, Color main, Color inner, Color center, FCenter ck)
        {
            Petals = petals; Shape = shape; Main = main; Inner = inner; Center = center; CenterKind = ck;
        }
    }
    /// <summary>
    /// 鲜花动画控件（继承 HToolAnimBase）：22 种花 × 22 种构图/样式双维度自由组合，
    /// Running 时花枝轻摆、花环旋转。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("鲜花动画控件：22 种花 × 22 种样式，运行时轻摆/旋转")]
    public class HFlower : HToolAnimBase
    {
        private HFlowerKind _kind = HFlowerKind.Rose;
        private HFlowerStyle _style = HFlowerStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Rose);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CStem = Color.FromArgb(72, 138, 62);
        private static readonly Color CStemDk = Color.FromArgb(44, 96, 42);
        private static readonly Color CLeaf = Color.FromArgb(96, 162, 72);
        private static readonly Color CGold = Color.FromArgb(214, 178, 84);
        private static readonly Color CGoldLt = Color.FromArgb(246, 226, 150);
        private static readonly Color CInk = Color.FromArgb(52, 48, 56);
        private static readonly FSpec[] Specs =
        {
            new FSpec(8, FPetalShape.Spiral, Color.FromArgb(214,48,68), Color.FromArgb(244,110,120), Color.FromArgb(150,30,50), FCenter.None),    // 玫瑰
            new FSpec(3, FPetalShape.Cup, Color.FromArgb(226,64,78), Color.FromArgb(248,140,140), Color.FromArgb(240,210,70), FCenter.None),       // 郁金香
            new FSpec(6, FPetalShape.Point, Color.FromArgb(250,246,238), Color.FromArgb(238,200,210), Color.FromArgb(190,130,60), FCenter.Dot),    // 百合
            new FSpec(16, FPetalShape.Tongue, Color.FromArgb(250,196,40), Color.FromArgb(255,226,100), Color.FromArgb(96,58,28), FCenter.Disc),    // 向日葵
            new FSpec(14, FPetalShape.Round, Color.FromArgb(252,250,244), Color.FromArgb(240,236,220), Color.FromArgb(244,190,40), FCenter.Dot),    // 雏菊
            new FSpec(12, FPetalShape.Round, Color.FromArgb(238,100,130), Color.FromArgb(250,160,178), Color.FromArgb(200,70,90), FCenter.Dot),     // 康乃馨
            new FSpec(10, FPetalShape.Round, Color.FromArgb(226,80,140), Color.FromArgb(246,150,190), Color.FromArgb(250,210,80), FCenter.Dot),     // 牡丹
            new FSpec(5, FPetalShape.Round, Color.FromArgb(248,220,228), Color.FromArgb(255,240,244), Color.FromArgb(240,190,60), FCenter.Dot),     // 梅花
            new FSpec(5, FPetalShape.Notched, Color.FromArgb(252,205,220), Color.FromArgb(255,232,238), Color.FromArgb(240,170,60), FCenter.Dot),   // 樱花
            new FSpec(8, FPetalShape.Point, Color.FromArgb(248,210,224), Color.FromArgb(255,236,240), Color.FromArgb(244,206,80), FCenter.Dot),     // 荷花
            new FSpec(5, FPetalShape.Orchid, Color.FromArgb(172,100,200), Color.FromArgb(214,160,235), Color.FromArgb(250,220,90), FCenter.Dot),    // 兰花
            new FSpec(18, FPetalShape.Tongue, Color.FromArgb(240,180,40), Color.FromArgb(255,220,90), Color.FromArgb(200,130,40), FCenter.Dot),     // 菊花
            new FSpec(6, FPetalShape.Round, Color.FromArgb(250,248,240), Color.FromArgb(235,230,210), Color.FromArgb(248,206,60), FCenter.Cup),     // 水仙
            new FSpec(6, FPetalShape.Point, Color.FromArgb(110,90,200), Color.FromArgb(160,140,230), Color.FromArgb(250,220,90), FCenter.Dot),      // 鸢尾
            new FSpec(10, FPetalShape.BellSpike, Color.FromArgb(150,110,210), Color.FromArgb(190,160,235), Color.FromArgb(250,220,120), FCenter.Dot),// 风信子
            new FSpec(20, FPetalShape.Fluff, Color.FromArgb(248,246,235), Color.White, Color.FromArgb(180,160,90), FCenter.Tuft),                    // 蒲公英
            new FSpec(5, FPetalShape.Tube, Color.FromArgb(90,90,210), Color.FromArgb(150,140,235), Color.White, FCenter.None),                       // 牵牛花
            new FSpec(5, FPetalShape.Round, Color.FromArgb(140,80,190), Color.FromArgb(200,160,230), Color.FromArgb(250,220,80), FCenter.Dot),       // 三色堇
            new FSpec(5, FPetalShape.Cluster, Color.FromArgb(150,170,225), Color.FromArgb(200,190,235), Color.FromArgb(240,220,120), FCenter.None),  // 绣球
            new FSpec(10, FPetalShape.BellSpike, Color.FromArgb(140,120,200), Color.FromArgb(180,165,225), Color.FromArgb(250,230,150), FCenter.None),// 薰衣草
            new FSpec(5, FPetalShape.Round, Color.FromArgb(250,170,190), Color.FromArgb(255,210,220), Color.FromArgb(244,190,60), FCenter.Dot),      // 桃花
            new FSpec(8, FPetalShape.Round, Color.FromArgb(220,60,90), Color.FromArgb(244,130,150), Color.FromArgb(250,210,90), FCenter.Dot)         // 山茶花
        };
        /// <summary>花的种类。</summary>
        [HCategoryLanguage("花朵"), HDisplayNameLanguage("花的种类"), HDescriptionLanguage("花的种类（22 种）"), Browsable(true)]
        [DefaultValue(HFlowerKind.Rose)]
        public HFlowerKind FlowerKind
        {
            get => _kind;
            set { _kind = value; Invalidate(); }
        }
        /// <summary>鲜花构图/样式。</summary>
        [HCategoryLanguage("花朵"), HDisplayNameLanguage("鲜花构图与绘画样式"), HDescriptionLanguage("鲜花构图与绘画样式（22 种）"), Browsable(true)]
        [DefaultValue(HFlowerStyle.Classic)]
        public HFlowerStyle FlowerStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        private FSpec Spec => Specs[(int)_kind];
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            if (a.Width <= 3f || a.Height <= 3f) { PaintPlacedText(g); return; }
            switch (_style)
            {
                case HFlowerStyle.Closeup:
                    DrawBloom(g, a.Left + a.Width / 2f, a.Top + a.Height / 2f,
                        Math.Min(a.Width, a.Height) * 0.48f, RenderOf(_style), 0f);
                    break;
                case HFlowerStyle.Bouquet: DrawBouquet(g, a); break;
                case HFlowerStyle.Vase: DrawVase(g, a); break;
                case HFlowerStyle.Pot: DrawPot(g, a); break;
                case HFlowerStyle.Wreath: DrawWreath(g, a); break;
                case HFlowerStyle.Garland: DrawGarland(g, a); break;
                case HFlowerStyle.Field: DrawField(g, a); break;
                case HFlowerStyle.Twin: DrawTwin(g, a); break;
                case HFlowerStyle.Badge: DrawBadge(g, a); break;
                case HFlowerStyle.Neon: DrawNeonScene(g, a); break;
                case HFlowerStyle.PaperCut: DrawPaperScene(g, a); break;
                case HFlowerStyle.Ornament: DrawOrnament(g, a); break;
                case HFlowerStyle.HeartBloom: DrawHeartBloom(g, a); break;
                case HFlowerStyle.LineArt:
                case HFlowerStyle.InkWash:
                case HFlowerStyle.GoldFoil:
                case HFlowerStyle.Watercolor:
                case HFlowerStyle.StainedGlass:
                case HFlowerStyle.Minimal:
                case HFlowerStyle.Geometric:
                case HFlowerStyle.Emboss:
                    DrawSingleStem(g, a, RenderOf(_style));
                    break;
                default:
                    DrawSingleStem(g, a, HFlowerRender.Normal);
                    break;
            }
            PaintPlacedText(g);
        }
        private static HFlowerRender RenderOf(HFlowerStyle s)
        {
            switch (s)
            {
                case HFlowerStyle.LineArt: return HFlowerRender.Line;
                case HFlowerStyle.InkWash: return HFlowerRender.Ink;
                case HFlowerStyle.Neon: return HFlowerRender.Neon;
                case HFlowerStyle.GoldFoil: return HFlowerRender.Gold;
                case HFlowerStyle.PaperCut: return HFlowerRender.Paper;
                case HFlowerStyle.Watercolor: return HFlowerRender.Water;
                case HFlowerStyle.StainedGlass: return HFlowerRender.Glass;
                case HFlowerStyle.Minimal: return HFlowerRender.Minimal;
                case HFlowerStyle.Geometric: return HFlowerRender.Geo;
                case HFlowerStyle.Emboss: return HFlowerRender.Emboss;
                default: return HFlowerRender.Normal;
            }
        }
        // 花枝轻摆角度
        private float Sway(float amp) => Running ? (float)Math.Sin(Phase) * amp : 0f;
        // ----------------------------------------------------------------
        // 颜色工厂：按渲染风格取花瓣色/茎秆色
        // ----------------------------------------------------------------
        private Color PetalColor(FSpec s, HFlowerRender r, float shade)
        {
            Color c = shade > 0.5f ? s.Inner : s.Main;
            switch (r)
            {
                case HFlowerRender.Ink: return Color.FromArgb((int)(40 + shade * 70), CInk);
                case HFlowerRender.Gold: return HToolPalettes.Mix(CGoldLt, CGold, shade * 0.6f);
                case HFlowerRender.Paper: return HToolPalettes.Mix(Color.FromArgb(220, 64, 74), Color.FromArgb(160, 30, 44), shade);
                case HFlowerRender.Emboss:
                    Color gv = HToolPalettes.Mix(Color.FromArgb(205, 208, 214), Color.FromArgb(120, 124, 132), shade);
                    return gv;
                case HFlowerRender.Water: return Color.FromArgb(150, c);
                case HFlowerRender.Glass: return c;
                default: return c;
            }
        }
        private Color StemColor(HFlowerRender r)
        {
            switch (r)
            {
                case HFlowerRender.Ink:
                case HFlowerRender.Line:
                case HFlowerRender.Minimal: return CInk;
                case HFlowerRender.Gold: return CGold;
                case HFlowerRender.Neon: return Color.FromArgb(90, 120, 255, 170);
                case HFlowerRender.Paper: return Color.FromArgb(60, 90, 60);
                case HFlowerRender.Emboss: return Color.FromArgb(130, 134, 140);
                default: return CStem;
            }
        }
        private float LineW(float r) => Math.Max(1f, r * 0.03f);
        // ----------------------------------------------------------------
        // 花头
        // ----------------------------------------------------------------
        private void DrawBloom(Graphics g, float cx, float cy, float r, HFlowerRender render, float spin)
        {
            if (r < 2f) return;
            var spec = Spec;
            switch (spec.Shape)
            {
                case FPetalShape.Spiral: DrawRose(g, cx, cy, r, spec, render, spin); break;
                case FPetalShape.Cup: DrawTulip(g, cx, cy, r, spec, render); break;
                case FPetalShape.Fluff: DrawDandelion(g, cx, cy, r, spec, render); break;
                case FPetalShape.Tube: DrawTrumpet(g, cx, cy, r, spec, render); break;
                case FPetalShape.BellSpike: DrawBellSpike(g, cx, cy, r, spec, render); break;
                case FPetalShape.Cluster: DrawCluster(g, cx, cy, r, spec, render, spin); break;
                case FPetalShape.Orchid: DrawOrchid(g, cx, cy, r, spec, render); break;
                default: DrawGeneric(g, cx, cy, r, spec, render, spin); break;
            }
        }
        private bool OutlineOnly(HFlowerRender r)
            => r == HFlowerRender.Line || r == HFlowerRender.Minimal || r == HFlowerRender.Neon;
        // 通用放射状花瓣花
        private void DrawGeneric(Graphics g, float cx, float cy, float r, FSpec s,
            HFlowerRender render, float spin)
        {
            int n = s.Petals;
            float pw = r * (s.Shape == FPetalShape.Tongue ? 0.16f : 0.5f);
            float len = r * (s.Shape == FPetalShape.Tongue ? 0.98f : 0.92f);
            bool geo = render == HFlowerRender.Geo;
            // 外层
            for (int layer = 1; layer >= 0; layer--)
            {
                float lr = r - layer * r * 0.34f;
                float lpw = pw * (layer == 0 ? 0.8f : 1f);
                for (int i = 0; i < n; i++)
                {
                    double ang = spin + i * 360.0 / n + (layer == 0 ? 180.0 / n : 0);
                    var st = g.Save();
                    g.TranslateTransform(cx, cy);
                    g.RotateTransform((float)ang);
                    DrawPetal(g, lr, lpw, s, render, geo, layer == 0);
                    g.Restore(st);
                }
            }
            DrawFlowerCenter(g, cx, cy, r, s, render);
        }
        // 已在“指向正右/上方”的旋转坐标系中画花瓣（沿 +Y 负方向伸出：我们用沿 -Y）
        private void DrawPetal(Graphics g, float len, float pw, FSpec s,
            HFlowerRender render, bool geo, bool innerLayer)
        {
            var rect = new RectangleF(-pw / 2f, -len, pw, len);
            if (OutlineOnly(render))
            {
                Color oc = render == HFlowerRender.Neon
                    ? Color.FromArgb(255, HToolPalettes.Tint(s.Main, 0.4f))
                    : (render == HFlowerRender.Minimal ? Color.FromArgb(110, 90, 100) : CInk);
                float lw = render == HFlowerRender.Neon ? len * 0.06f : LineW(len);
                using (var glow = render == HFlowerRender.Neon
                    ? new Pen(Color.FromArgb(70, s.Main), lw * 2.4f) : null)
                {
                    if (geo) DrawGeoPetal(g, rect, null, glow);
                    else g.DrawEllipse(glow, rect);
                }
                using (var pen = new Pen(oc, lw))
                {
                    if (s.Shape == FPetalShape.Point)
                        g.DrawPolygon(pen, PointPetal(rect));
                    else if (geo) DrawGeoPetal(g, rect, null, pen);
                    else g.DrawEllipse(pen, rect);
                }
                return;
            }
            Color pc = PetalColor(s, render, innerLayer ? 0.7f : 0.15f);
            if (s.Shape == FPetalShape.Point)
            {
                using (var b = new SolidBrush(pc))
                    g.FillPolygon(b, PointPetal(rect));
                using (var p = new Pen(EdgeColor(s, render), LineW(len)))
                    g.DrawPolygon(p, PointPetal(rect));
            }
            else if (geo)
            {
                DrawGeoPetal(g, rect, new SolidBrush(pc), new Pen(EdgeColor(s, render), LineW(len)));
            }
            else
            {
                using (var lb = new LinearGradientBrush(rect,
                    HToolPalettes.Tint(pc, 0.3f), HToolPalettes.Shade(pc, 0.12f), 90f))
                    g.FillEllipse(lb, rect);
                using (var p = new Pen(EdgeColor(s, render), LineW(len)))
                    g.DrawEllipse(p, rect);
            }
            // 尖瓣脉纹
            if (s.Shape == FPetalShape.Tongue && len > 8f && !geo)
                using (var vein = new Pen(Color.FromArgb(70, HToolPalettes.Shade(s.Main, 0.2f)), 0.8f))
                    g.DrawLine(vein, 0, -len * 0.15f, 0, -len * 0.82f);
            // 樱花缺口
            if (s.Shape == FPetalShape.Notched)
                using (var notch = new SolidBrush(Color.FromArgb(255, 238, 244)))
                    g.FillPolygon(notch, new[]
                    {
                        new PointF(-pw * 0.18f, -len * 0.96f),
                        new PointF(0, -len * 0.8f),
                        new PointF(pw * 0.18f, -len * 0.96f)
                    });
        }
        private Color EdgeColor(FSpec s, HFlowerRender r)
        {
            switch (r)
            {
                case HFlowerRender.Glass: return Color.FromArgb(60, 40, 70);
                case HFlowerRender.Gold: return HToolPalettes.Shade(CGold, 0.3f);
                case HFlowerRender.Paper: return Color.FromArgb(120, 20, 28);
                case HFlowerRender.Emboss: return Color.FromArgb(100, 104, 110);
                default: return Color.FromArgb(120, HToolPalettes.Shade(s.Main, 0.35f));
            }
        }
        // 尖瓣多边形
        private static PointF[] PointPetal(RectangleF r)
        {
            return new[]
            {
                new PointF(0, r.Bottom),
                new PointF(r.Left, r.Top + r.Height * 0.32f),
                new PointF(r.Left + r.Width * 0.18f, r.Top),
                new PointF(r.Right - r.Width * 0.18f, r.Top),
                new PointF(r.Right, r.Top + r.Height * 0.32f)
            };
        }
        // 几何瓣（菱形）
        private void DrawGeoPetal(Graphics g, RectangleF r, Brush b, Pen p)
        {
            var pts = new[]
            {
                new PointF(0, r.Bottom), new PointF(r.Left, r.Top + r.Height * 0.4f),
                new PointF(0, r.Top), new PointF(r.Right, r.Top + r.Height * 0.4f)
            };
            if (b != null) g.FillPolygon(b, pts);
            if (p != null) g.DrawPolygon(p, pts);
        }
        // 花芯
        private void DrawFlowerCenter(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            float cr = r * (s.CenterKind == FCenter.Disc ? 0.34f : 0.16f);
            switch (s.CenterKind)
            {
                case FCenter.None: return;
                case FCenter.Cup:
                    // 水仙杯
                    var cup = new RectangleF(cx - cr * 1.3f, cy - cr, cr * 2.6f, cr * 2f);
                    using (var cb = new SolidBrush(render == HFlowerRender.Gold ? CGoldLt : Color.FromArgb(250, 205, 60)))
                        g.FillEllipse(cb, cup);
                    using (var cp = new Pen(Color.FromArgb(200, 160, 40), LineW(r)))
                        g.DrawEllipse(cp, cup);
                    break;
                case FCenter.Disc:
                    using (var db = new SolidBrush(render == HFlowerRender.Gold
                        ? CGoldLt : HToolPalettes.Mix(s.Center, HToolPalettes.Shade(s.Center, 0.3f), 0.4f)))
                        g.FillEllipse(db, cx - cr, cy - cr, cr * 2, cr * 2);
                    // 葵花籽纹理
                    if (cr > 6f)
                        using (var seed = new SolidBrush(Color.FromArgb(90, Color.Black)))
                            for (int i = 0; i < 12; i++)
                            {
                                double ang = i * 1.7;
                                float d = (i % 4 + 1) * cr * 0.18f;
                                g.FillEllipse(seed, cx + (float)Math.Cos(ang) * d - 0.8f,
                                    cy + (float)Math.Sin(ang) * d - 0.8f, 1.6f, 1.6f);
                            }
                    break;
                default:
                    using (var db = new SolidBrush(render == HFlowerRender.Gold ? CGoldLt
                        : render == HFlowerRender.Ink ? Color.FromArgb(120, CInk) : s.Center))
                        g.FillEllipse(db, cx - cr, cy - cr, cr * 2, cr * 2);
                    using (var hi = new SolidBrush(Color.FromArgb(120, Color.White)))
                        g.FillEllipse(hi, cx - cr * 0.4f, cy - cr * 0.5f, cr * 0.5f, cr * 0.4f);
                    break;
            }
        }
        // 玫瑰（螺旋内芯 + 层层弧瓣）
        private void DrawRose(Graphics g, float cx, float cy, float r, FSpec s,
            HFlowerRender render, float spin)
        {
            if (OutlineOnly(render))
            {
                using (var pen = new Pen(render == HFlowerRender.Neon
                    ? Color.FromArgb(255, HToolPalettes.Tint(s.Main, 0.4f)) : CInk, LineW(r)))
                {
                    g.DrawEllipse(pen, cx - r * 0.7f, cy - r * 0.7f, r * 1.4f, r * 1.4f);
                    var st0 = g.Save();
                    g.TranslateTransform(cx, cy);
                    g.RotateTransform(spin);
                    using (var spiral = new Pen(pen.Color, pen.Width))
                        g.DrawArc(spiral, -r * 0.34f, -r * 0.34f, r * 0.68f, r * 0.68f, 30f, 300f);
                    g.Restore(st0);
                }
                return;
            }
            // 外层 5 大瓣
            for (int i = 0; i < 5; i++)
            {
                var st = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(spin + i * 72f);
                var rect = new RectangleF(-r * 0.3f, -r * 0.98f, r * 0.6f, r * 0.8f);
                using (var pb = new LinearGradientBrush(rect,
                    HToolPalettes.Tint(PetalColor(s, render, 0.2f), 0.2f),
                    PetalColor(s, render, 0.6f), 90f))
                    g.FillEllipse(pb, rect);
                g.Restore(st);
            }
            // 内层螺旋
            var st2 = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(spin);
            for (int i = 0; i < 3; i++)
            {
                float rr2 = r * (0.52f - i * 0.13f);
                using (var pb = new SolidBrush(PetalColor(s, render, i * 0.25f)))
                    g.FillEllipse(pb, -rr2 / 2, -rr2 / 2, rr2, rr2);
            }
            using (var sp = new Pen(HToolPalettes.Shade(s.Main, 0.4f), LineW(r)))
                g.DrawArc(sp, -r * 0.3f, -r * 0.3f, r * 0.6f, r * 0.6f, 20f, 310f);
            g.Restore(st2);
        }
        // 郁金香（杯状 3 瓣）
        private void DrawTulip(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            float w = r * 0.72f, h = r * 1.1f;
            var left = new RectangleF(cx - w / 2, cy - h, w, h);
            var mid = new RectangleF(cx - w / 3, cy - h * 1.12f, w * 0.66f, h * 1.05f);
            if (OutlineOnly(render))
            {
                Color oc = render == HFlowerRender.Neon ? HToolPalettes.Tint(s.Main, 0.4f) : CInk;
                using (var pen = new Pen(oc, LineW(r)))
                {
                    g.DrawEllipse(pen, left);
                    g.DrawArc(pen, mid, 180f, 180f);
                }
                return;
            }
            using (var lb = new SolidBrush(PetalColor(s, render, 0.3f)))
                g.FillEllipse(lb, left);
            using (var mb = new LinearGradientBrush(mid,
                HToolPalettes.Tint(PetalColor(s, render, 0.05f), 0.25f),
                PetalColor(s, render, 0.5f), 90f))
            {
                g.FillPie(mb, mid.X, mid.Y, mid.Width, mid.Height, 180f, 180f);
                g.FillRectangle(mb, mid.X, cy - h * 0.15f, mid.Width, h * 0.15f);
            }
        }
        // 蒲公英（绒球）
        private void DrawDandelion(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            var rnd = new Random((int)(cx * 13 + cy * 7 + r));
            Color line = render == HFlowerRender.Ink ? CInk
                : render == HFlowerRender.Gold ? CGold : Color.FromArgb(170, 160, 120);
            using (var pen = new Pen(line, Math.Max(0.7f, r * 0.015f)))
            using (var dot = new SolidBrush(render == HFlowerRender.Gold ? CGoldLt : Color.FromArgb(225, 220, 190)))
            {
                for (int i = 0; i < 26; i++)
                {
                    double ang = rnd.NextDouble() * Math.PI * 2;
                    float rr = r * (0.75f + (float)rnd.NextDouble() * 0.22f);
                    float ex = cx + (float)Math.Cos(ang) * rr;
                    float ey = cy + (float)Math.Sin(ang) * rr;
                    g.DrawLine(pen, cx, cy, ex, ey);
                    // 绒毛叉
                    g.DrawLine(pen, ex, ey, ex + (float)Math.Cos(ang + 0.4) * r * 0.1f,
                        ey + (float)Math.Sin(ang + 0.4) * r * 0.1f);
                    g.DrawLine(pen, ex, ey, ex + (float)Math.Cos(ang - 0.4) * r * 0.1f,
                        ey + (float)Math.Sin(ang - 0.4) * r * 0.1f);
                    g.FillEllipse(dot, ex - 0.9f, ey - 0.9f, 1.8f, 1.8f);
                }
                g.FillEllipse(dot, cx - r * 0.14f, cy - r * 0.14f, r * 0.28f, r * 0.28f);
            }
        }
        // 牵牛花（喇叭筒）
        private void DrawTrumpet(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            float wr = r * 0.78f;
            var mouth = new RectangleF(cx - wr, cy - wr * 0.72f, wr * 2, wr * 1.44f);
            if (OutlineOnly(render))
            {
                using (var pen = new Pen(render == HFlowerRender.Neon ? HToolPalettes.Tint(s.Main, 0.4f) : CInk, LineW(r)))
                {
                    g.DrawEllipse(pen, mouth);
                    PointF a1 = new PointF(cx - wr * 0.5f, cy), a2 = new PointF(cx + wr * 0.5f, cy);
                    g.DrawLine(pen, a1.X, a1.Y, cx - r * 0.14f, cy + r * 0.9f);
                    g.DrawLine(pen, a2.X, a2.Y, cx + r * 0.14f, cy + r * 0.9f);
                }
                return;
            }
            using (var mb = new LinearGradientBrush(mouth,
                HToolPalettes.Tint(PetalColor(s, render, 0.1f), 0.25f),
                PetalColor(s, render, 0.55f), 0f))
                g.FillEllipse(mb, mouth);
            using (var tube = new SolidBrush(HToolPalettes.Shade(PetalColor(s, render, 0.4f), 0.2f)))
                g.FillPolygon(tube, new[]
                {
                    new PointF(cx - wr * 0.5f, cy), new PointF(cx + wr * 0.5f, cy),
                    new PointF(cx + r * 0.14f, cy + r * 0.9f), new PointF(cx - r * 0.14f, cy + r * 0.9f)
                });
            using (var throat = new SolidBrush(Color.FromArgb(220, s.Inner)))
                g.FillEllipse(throat, cx - wr * 0.3f, cy - wr * 0.24f, wr * 0.6f, wr * 0.48f);
            using (var ep = new Pen(EdgeColor(s, render), LineW(r)))
                g.DrawEllipse(ep, mouth);
        }
        // 穗状钟花（风信子/薰衣草）
        private void DrawBellSpike(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            bool lavender = s.Main.B < s.Main.R + 20 && s.Main.R < 170;
            int n = lavender ? 11 : 9;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                float y = cy + r * 0.95f - t * r * 1.7f;
                float spread = (1f - t) * r * 0.34f * (float)Math.Sin(t * Math.PI);
                float br = r * (0.16f - t * 0.04f) + r * 0.03f;
                for (int side = -1; side <= 1; side += 2)
                {
                    float bx = cx + side * spread + (i % 2 == 0 ? 0 : side * br * 0.3f);
                    Color bc = PetalColor(s, render, (i % 3) * 0.18f);
                    var bell = new RectangleF(bx - br, y - br, br * 2, br * 1.6f);
                    if (OutlineOnly(render))
                        using (var pen = new Pen(render == HFlowerRender.Neon
                            ? HToolPalettes.Tint(s.Main, 0.4f) : CInk, LineW(r) * 0.7f))
                            g.DrawArc(pen, bell, 0f, 180f);
                    else
                        using (var bb = new SolidBrush(bc))
                            g.FillPie(bb, bell.X, bell.Y, bell.Width, bell.Height, 0f, 180f);
                }
            }
        }
        // 绣球（多小花球）
        private void DrawCluster(Graphics g, float cx, float cy, float r, FSpec s,
            HFlowerRender render, float spin)
        {
            var rnd = new Random(5);
            for (int i = 0; i < 13; i++)
            {
                double ang = rnd.NextDouble() * Math.PI * 2;
                float d = (float)Math.Sqrt(rnd.NextDouble()) * r * 0.7f;
                float bx = cx + (float)Math.Cos(ang) * d;
                float by = cy + (float)Math.Sin(ang) * d;
                float br = r * (0.2f + (1 - d / r) * 0.08f);
                var mini = new FSpec(4, FPetalShape.Round,
                    HToolPalettes.Mix(s.Main, s.Inner, (float)rnd.NextDouble() * 0.8f),
                    s.Inner, s.Center, FCenter.Dot);
                DrawMiniFour(g, bx, by, br, mini, render);
            }
        }
        // 可重入的小花绘制（避免改 Specs 表）
        private void DrawMiniFour(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            if (OutlineOnly(render))
            {
                using (var pen = new Pen(CInk, Math.Max(0.6f, LineW(r) * 0.7f)))
                    for (int i = 0; i < 4; i++)
                    {
                        var st = g.Save();
                        g.TranslateTransform(cx, cy);
                        g.RotateTransform(i * 90f);
                        g.DrawEllipse(pen, -r * 0.28f, -r, r * 0.56f, r * 0.9f);
                        g.Restore(st);
                    }
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                var st = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(i * 90f);
                var rect = new RectangleF(-r * 0.3f, -r, r * 0.6f, r * 0.9f);
                using (var pb = new SolidBrush(PetalColor(s, render, i % 2 * 0.3f)))
                    g.FillEllipse(pb, rect);
                g.Restore(st);
            }
            using (var cb = new SolidBrush(s.Center))
                g.FillEllipse(cb, cx - r * 0.12f, cy - r * 0.12f, r * 0.24f, r * 0.24f);
        }
        // 兰花（特化唇瓣）
        private void DrawOrchid(Graphics g, float cx, float cy, float r, FSpec s, HFlowerRender render)
        {
            // 上 3 大瓣 + 下 2 侧瓣 + 唇瓣
            float[] angs = { -90f, -150f, -30f, 150f, 30f };
            float[] lens = { 0.9f, 0.78f, 0.78f, 0.7f, 0.7f };
            for (int i = 0; i < 5; i++)
            {
                var st = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(angs[i]);
                var rect = new RectangleF(-r * 0.26f, -r * lens[i], r * 0.52f, r * lens[i]);
                if (OutlineOnly(render))
                    using (var pen = new Pen(render == HFlowerRender.Neon ? HToolPalettes.Tint(s.Main, 0.4f) : CInk, LineW(r)))
                        g.DrawEllipse(pen, rect);
                else
                    using (var pb = new SolidBrush(PetalColor(s, render, i * 0.12f)))
                        g.FillEllipse(pb, rect);
                g.Restore(st);
            }
            // 唇瓣
            var lip = new RectangleF(cx - r * 0.24f, cy + r * 0.08f, r * 0.48f, r * 0.42f);
            if (!OutlineOnly(render))
                using (var lb = new SolidBrush(HToolPalettes.Shade(s.Main, 0.25f)))
                    g.FillEllipse(lb, lip);
            using (var col = new SolidBrush(render == HFlowerRender.Line ? CInk : s.Center))
                g.FillEllipse(col, cx - r * 0.07f, cy - r * 0.07f, r * 0.14f, r * 0.14f);
        }
        // ----------------------------------------------------------------
        // 茎 + 叶
        // ----------------------------------------------------------------
        private void DrawStem(Graphics g, float xTop, float yTop, float xBot, float yBot,
            HFlowerRender render, bool leaves)
        {
            Color sc = StemColor(render);
            using (var pen = new Pen(sc, Math.Max(1f, (yBot - yTop) * 0.025f)))
                g.DrawBezier(pen, xTop, yTop,
                    xTop + (xBot - xTop) * 0.3f - 6f, yTop + (yBot - yTop) * 0.4f,
                    xBot + (xTop - xBot) * 0.2f + 6f, yBot - (yBot - yTop) * 0.3f,
                    xBot, yBot);
            if (!leaves) return;
            DrawLeaf(g, xTop + (xBot - xTop) * 0.32f, yTop + (yBot - yTop) * 0.46f,
                (yBot - yTop) * 0.2f, true, render);
            DrawLeaf(g, xTop + (xBot - xTop) * 0.6f, yTop + (yBot - yTop) * 0.66f,
                (yBot - yTop) * 0.17f, false, render);
        }
        private void DrawLeaf(Graphics g, float x, float y, float s, bool leftSide, HFlowerRender render)
        {
            var st = g.Save();
            g.TranslateTransform(x, y);
            g.RotateTransform(leftSide ? -32f : 148f);
            var rect = new RectangleF(0, -s * 0.22f, s, s * 0.44f);
            if (OutlineOnly(render))
                using (var pen = new Pen(StemColor(render), Math.Max(0.8f, s * 0.05f)))
                    g.DrawEllipse(pen, rect);
            else
            {
                Color lc = render == HFlowerRender.Gold ? CGold
                    : render == HFlowerRender.Emboss ? Color.FromArgb(150, 154, 160) : CLeaf;
                using (var lb = new LinearGradientBrush(rect, HToolPalettes.Tint(lc, 0.25f),
                    HToolPalettes.Shade(lc, 0.2f), 0f))
                    g.FillEllipse(lb, rect);
                using (var vein = new Pen(Color.FromArgb(80, CStemDk), 0.8f))
                    g.DrawLine(vein, 0, 0, s * 0.92f, 0);
            }
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 构图：单枝
        // ----------------------------------------------------------------
        private void DrawSingleStem(Graphics g, RectangleF a, HFlowerRender render)
        {
            float br = Math.Min(a.Width, a.Height * 0.72f) * 0.34f;
            float cx = a.Left + a.Width / 2f + Sway(a.Width * 0.02f);
            float cy = a.Top + br + a.Height * 0.04f;
            DrawStem(g, cx, cy + br * 0.5f, a.Left + a.Width / 2f, a.Bottom - 2f, render, true);
            DrawBloom(g, cx, cy, br, render, Running ? Sway(7f) : 0f);
        }
        // 花束
        private void DrawBouquet(Graphics g, RectangleF a)
        {
            float baseX = a.Left + a.Width / 2f, baseY = a.Bottom - a.Height * 0.06f;
            var heads = new (float dx, float dy, float rr)[]
            {
                (-0.26f, -0.34f, 0.24f), (0f, -0.46f, 0.27f), (0.26f, -0.34f, 0.24f),
                (-0.12f, -0.22f, 0.2f), (0.13f, -0.2f, 0.2f)
            };
            float unit = Math.Min(a.Width, a.Height);
            foreach (var h in heads)
                DrawStem(g, baseX + h.dx * unit, baseY + h.dy * unit + h.rr * unit,
                    baseX, baseY, HFlowerRender.Normal, false);
            foreach (var h in heads)
                DrawBloom(g, baseX + h.dx * unit + Sway(2f), baseY + h.dy * unit,
                    h.rr * unit, HFlowerRender.Normal, Sway(5f));
            // 扎带
            using (var tie = new SolidBrush(Palette.Accent))
                g.FillPolygon(tie, new[]
                {
                    new PointF(baseX - unit * 0.1f, baseY - unit * 0.06f),
                    new PointF(baseX + unit * 0.1f, baseY - unit * 0.06f),
                    new PointF(baseX + unit * 0.04f, baseY + unit * 0.08f),
                    new PointF(baseX - unit * 0.04f, baseY + unit * 0.08f)
                });
        }
        // 花瓶
        private void DrawVase(Graphics g, RectangleF a)
        {
            var pal = Palette;
            float vw = a.Width * 0.4f, vh = a.Height * 0.32f;
            float vx = a.Left + (a.Width - vw) / 2f, vy = a.Bottom - vh;
            using (var vb = new LinearGradientBrush(new RectangleF(vx, vy, vw, vh),
                HToolPalettes.Tint(pal.Main, 0.5f), pal.Dark, 0f))
            {
                g.FillRectangle(vb, vx + vw * 0.22f, vy, vw * 0.56f, vh * 0.2f);
                g.FillPolygon(vb, new[]
                {
                    new PointF(vx + vw * 0.22f, vy + vh * 0.12f),
                    new PointF(vx + vw * 0.78f, vy + vh * 0.12f),
                    new PointF(vx + vw * 0.92f, vy + vh),
                    new PointF(vx + vw * 0.08f, vy + vh)
                });
            }
            float mouthY = vy + vh * 0.06f;
            var heads = new (float fx, float rr)[]
            {
                (0.28f, 0.17f), (0.5f, 0.21f), (0.72f, 0.17f), (0.38f, 0.15f), (0.62f, 0.15f)
            };
            foreach (var h in heads)
            {
                float hx = a.Left + a.Width * h.fx;
                DrawStem(g, hx, mouthY + a.Height * h.rr,
                    a.Left + a.Width / 2f, mouthY + 4f, HFlowerRender.Normal, false);
                DrawBloom(g, hx + Sway(2f), mouthY - a.Height * (h.rr * 1.2f),
                    a.Height * h.rr, HFlowerRender.Normal, Sway(5f));
            }
        }
        // 盆栽
        private void DrawPot(Graphics g, RectangleF a)
        {
            var pal = Palette;
            float pw = a.Width * 0.48f, ph = a.Height * 0.26f;
            float px = a.Left + (a.Width - pw) / 2f, py = a.Bottom - ph;
            using (var pot = new LinearGradientBrush(new RectangleF(px, py, pw, ph),
                HToolPalettes.Tint(pal.Accent, 0.2f), HToolPalettes.Shade(pal.Accent, 0.25f), 90f))
                g.FillPolygon(pot, new[]
                {
                    new PointF(px + pw * 0.08f, py), new PointF(px + pw * 0.92f, py),
                    new PointF(px + pw * 0.8f, py + ph), new PointF(px + pw * 0.2f, py + ph)
                });
            using (var rim = new SolidBrush(HToolPalettes.Shade(pal.Accent, 0.15f)))
                g.FillRectangle(rim, px, py, pw, ph * 0.16f);
            float soilY = py + ph * 0.1f;
            var heads = new (float fx, float rr)[] { (0.3f, 0.17f), (0.52f, 0.21f), (0.74f, 0.17f) };
            foreach (var h in heads)
            {
                float hx = a.Left + a.Width * h.fx;
                DrawStem(g, hx, soilY + a.Height * h.rr,
                    a.Left + a.Width / 2f, soilY, HFlowerRender.Normal, true);
                DrawBloom(g, hx + Sway(2f), soilY - a.Height * (h.rr * 1.15f),
                    a.Height * h.rr, HFlowerRender.Normal, Sway(6f));
            }
        }
        // 花环
        private void DrawWreath(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float rad = Math.Min(a.Width, a.Height) * 0.36f;
            // 藤环
            using (var vine = new Pen(CStemDk, Math.Max(2f, rad * 0.12f)))
                g.DrawEllipse(vine, cx - rad, cy - rad * 0.92f, rad * 2, rad * 1.84f);
            int n = 8;
            float rot = Running ? SpinAngle * 0.5f : 0f;
            for (int i = 0; i < n; i++)
            {
                double ang = rot * Math.PI / 180 + i * Math.PI * 2 / n;
                float fx = cx + (float)Math.Cos(ang) * rad;
                float fy = cy + (float)Math.Sin(ang) * rad * 0.92f;
                // 小叶
                DrawLeaf(g, fx, fy, rad * 0.4f, i % 2 == 0, HFlowerRender.Normal);
                DrawBloom(g, fx, fy, rad * 0.3f, HFlowerRender.Normal, 0f);
            }
        }
        // 藤条
        private void DrawGarland(Graphics g, RectangleF a)
        {
            int n = 5;
            using (var vine = new Pen(CStemDk, Math.Max(2f, a.Height * 0.02f)))
            {
                var prev = new PointF(a.Left + 4f, a.Top + a.Height * 0.5f);
                for (int i = 1; i <= n * 2; i++)
                {
                    float t = i / (n * 2f);
                    var next = new PointF(a.Left + a.Width * t,
                        a.Top + a.Height * (0.42f + 0.16f * (float)Math.Sin(t * Math.PI * 2 + Sway(0.4f))));
                    g.DrawLine(vine, prev, next);
                    prev = next;
                }
            }
            for (int i = 0; i < n; i++)
            {
                float t = (i + 0.5f) / n;
                float fx = a.Left + a.Width * t;
                float fy = a.Top + a.Height * (0.42f + 0.16f * (float)Math.Sin(t * Math.PI * 2));
                DrawLeaf(g, fx, fy + a.Height * 0.05f, Math.Min(a.Width, a.Height) * 0.16f,
                    i % 2 == 0, HFlowerRender.Normal);
                DrawBloom(g, fx, fy - a.Height * 0.08f, Math.Min(a.Width, a.Height) * 0.15f,
                    HFlowerRender.Normal, Sway(5f));
            }
        }
        // 花田
        private void DrawField(Graphics g, RectangleF a)
        {
            using (var ground = new LinearGradientBrush(
                new RectangleF(a.Left, a.Top + a.Height * 0.7f, a.Width, a.Height * 0.3f),
                Color.FromArgb(130, 180, 100), Color.FromArgb(90, 140, 70), 90f))
                g.FillRectangle(ground, a.Left, a.Top + a.Height * 0.7f, a.Width, a.Height * 0.3f);
            var rnd = new Random(3);
            for (int i = 0; i < 6; i++)
            {
                float fx = a.Left + a.Width * (0.08f + rnd.Next(0, 84) / 100f);
                float rr = a.Height * (0.11f + rnd.Next(0, 8) / 100f);
                float headY = a.Top + a.Height * (0.44f + rnd.Next(0, 18) / 100f);
                DrawStem(g, fx, headY + rr, fx + (rnd.Next(-6, 6)), a.Top + a.Height * 0.72f,
                    HFlowerRender.Normal, i % 2 == 0);
                DrawBloom(g, fx + Sway(1.5f), headY, rr, HFlowerRender.Normal, Sway(6f));
            }
        }
        // 双枝
        private void DrawTwin(Graphics g, RectangleF a)
        {
            float r = Math.Min(a.Width, a.Height) * 0.2f;
            DrawStem(g, a.Left + a.Width * 0.36f, a.Top + a.Height * 0.24f + r,
                a.Left + a.Width * 0.42f, a.Bottom - 2f, HFlowerRender.Normal, true);
            DrawStem(g, a.Left + a.Width * 0.66f, a.Top + a.Height * 0.34f + r,
                a.Left + a.Width * 0.58f, a.Bottom - 2f, HFlowerRender.Normal, true);
            DrawBloom(g, a.Left + a.Width * 0.36f, a.Top + a.Height * 0.24f, r,
                HFlowerRender.Normal, Sway(7f));
            DrawBloom(g, a.Left + a.Width * 0.66f, a.Top + a.Height * 0.34f, r * 0.85f,
                HFlowerRender.Normal, -Sway(7f));
        }
        // 徽章
        private void DrawBadge(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float d = Math.Min(a.Width, a.Height) * 0.94f;
            using (var disc = new LinearGradientBrush(
                new RectangleF(cx - d / 2, cy - d / 2, d, d),
                HToolPalettes.Tint(Palette.Main, 0.35f), Palette.Dark, 45f))
                g.FillEllipse(disc, cx - d / 2, cy - d / 2, d, d);
            using (var rim = new Pen(HToolPalettes.Shade(Palette.Main, 0.4f), d * 0.04f))
                g.DrawEllipse(rim, cx - d * 0.44f, cy - d * 0.44f, d * 0.88f, d * 0.88f);
            DrawBloom(g, cx, cy, d * 0.3f, HFlowerRender.Normal, Sway(5f));
        }
        // 霓虹场景（深色圆底 + 霓虹花）
        private void DrawNeonScene(Graphics g, RectangleF a)
        {
            float d = Math.Min(a.Width, a.Height) * 0.96f;
            var bg = new RectangleF(a.Left + (a.Width - d) / 2f, a.Top + (a.Height - d) / 2f, d, d);
            using (var bb = new LinearGradientBrush(bg, Color.FromArgb(30, 18, 44), Color.FromArgb(12, 8, 24), 90f))
                g.FillEllipse(bb, bg);
            float br = Math.Min(a.Width, a.Height) * 0.3f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + br + a.Height * 0.06f;
            DrawStem(g, cx, cy + br * 0.5f, cx, a.Bottom - a.Height * 0.08f,
                HFlowerRender.Neon, true);
            DrawBloom(g, cx, cy, br, HFlowerRender.Neon, 0f);
        }
        // 剪纸场景（深红方底）
        private void DrawPaperScene(Graphics g, RectangleF a)
        {
            using (var bg = new SolidBrush(Color.FromArgb(255, 46, 24, 32)))
            using (var path = HBarBase.RoundPath(a, Math.Min(10f, Math.Min(a.Width, a.Height) * 0.08f)))
                g.FillPath(bg, path);
            float br = Math.Min(a.Width, a.Height) * 0.3f;
            float cx = a.Left + a.Width / 2f, cy = a.Top + br + a.Height * 0.06f;
            DrawStem(g, cx, cy + br * 0.5f, cx, a.Bottom - a.Height * 0.06f,
                HFlowerRender.Paper, true);
            DrawBloom(g, cx, cy, br, HFlowerRender.Paper, 0f);
        }
        // 团花纹样
        private void DrawOrnament(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            using (var ring = new Pen(HToolPalettes.Shade(Spec.Main, 0.3f),
                Math.Max(1.5f, Math.Min(a.Width, a.Height) * 0.02f)))
                g.DrawEllipse(ring, cx - a.Height * 0.38f, cy - a.Height * 0.38f,
                    a.Height * 0.76f, a.Height * 0.76f);
            for (int i = 0; i < 6; i++)
            {
                double ang = i * Math.PI / 3 + (Running ? SpinAngle * Math.PI / 360f : 0);
                        float lx = cx + (float)Math.Cos(ang) * a.Height * 0.26f;
                float ly = cy + (float)Math.Sin(ang) * a.Height * 0.26f;
                DrawLeaf(g, lx, ly, a.Height * 0.2f, i % 2 == 0, HFlowerRender.Normal);
            }
            DrawBloom(g, cx, cy, Math.Min(a.Width, a.Height) * 0.26f, HFlowerRender.Normal, 0f);
        }
        // 心形花丛
        private void DrawHeartBloom(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float s = Math.Min(a.Width, a.Height * 1.15f) * 0.42f;
            int n = 10;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                // 心形参数（近似）：左右两段贝塞尔采样点
                PointF pt = HeartPoint(t, cx, cy, s);
                float br = s * (0.12f + (i % 3) * 0.02f);
                DrawBloom(g, pt.X + Sway(1.2f), pt.Y, br, HFlowerRender.Normal, Sway(6f));
            }
            DrawBloom(g, cx, cy - s * 0.02f, s * 0.18f, HFlowerRender.Normal, 0f);
        }
        // 心形路径采样点（t 0..1 环绕一周）
        private static PointF HeartPoint(float t, float cx, float cy, float s)
        {
            // 经典心形参数方程
            double tt = t * Math.PI * 2;
            double x = 16 * Math.Pow(Math.Sin(tt), 3);
            double y = 13 * Math.Cos(tt) - 5 * Math.Cos(2 * tt) - 2 * Math.Cos(3 * tt) - Math.Cos(4 * tt);
            return new PointF(cx + (float)(x / 32.0 * s), cy - (float)(y / 32.0 * s));
        }
    }
}