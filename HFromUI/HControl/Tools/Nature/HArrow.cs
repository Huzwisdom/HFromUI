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
    /// 箭头种类：84 种——直线/双/多箭头、V 形、弯曲/环形、四向、交换、下载、
    /// 指示牌、箭靶、光标、导入导出、趋势等，末尾 14 种为加、减、乘、除等运算符号。
    /// </summary>
    public enum HArrowType
    {
        Arrow = 0,            // 普通三角箭头
        ThinArrow = 1,        // 细箭头
        ThickArrow = 2,       // 粗箭头
        OpenArrow = 3,        // 开口箭头
        BlockArrow = 4,       // 块状箭头
        HollowArrow = 5,      // 空心箭头
        DartArrow = 6,        // 飞镖箭头
        SpearArrow = 7,       // 长矛箭头
        LongArrow = 8,        // 长箭头
        ShortArrow = 9,       // 短箭头
        DashedArrow = 10,     // 虚线箭头
        DottedArrow = 11,     // 点线箭头
        TailedArrow = 12,     // 羽尾箭头
        NotchedArrow = 13,    // 燕尾箭头
        FancyArrow = 14,      // 花式双线箭头
        CometArrow = 15,      // 彗尾箭头
        QuillArrow = 16,      // 箭矢
        RoundedArrow = 17,    // 圆角粗箭头
        TinyHeadArrow = 18,   // 小头箭头
        BigHeadArrow = 19,    // 大头箭头
        TriangleOnly = 20,    // 仅三角形
        HarpoonArrow = 21,    // 鱼叉箭头
        DoubleLineArrow = 22, // 双线杆箭头
        ZigzagArrow = 23,     // 折线箭头
        WavyArrow = 24,       // 波浪杆箭头
        CurvedArrow = 25,     // 弯曲箭头
        BentArrow = 26,       // 直角转向箭头
        LoopArrow = 27,       // 环形刷新箭头
        UTurnArrow = 28,      // 掉头箭头
        RedoArrow = 29,       // 重做弯钩
        ExpandArrow = 30,     // 对角外扩
        ShrinkArrow = 31,     // 对角内收
        DoubleArrow = 32,     // 双向箭头
        DoubleOpenArrow = 33, // 双向开口
        DoubleBlockArrow = 34,// 双向块状
        DoubleThinArrow = 35, // 双向细箭头
        DoubleDashedArrow = 36,// 双向虚线
        TripleArrow = 37,     // 三支并行箭头
        QuadArrow = 38,       // 四支并行箭头
        Chevron = 39,         // V 形箭头
        ChevronDouble = 40,   // 双 V
        ChevronTriple = 41,   // 三 V
        ChevronRound = 42,    // 圆角 V
        UpDownArrow = 43,     // 竖向双箭头
        FourWayArrow = 44,    // 四向移动箭头
        ResizeHorizontal = 45,// 水平调整
        ResizeVertical = 46,  // 垂直调整
        SwapVertical = 47,    // 上下交换
        SwapHorizontal = 48,  // 左右交换
        ShuffleArrow = 49,    // 无序交错
        DownloadArrow = 50,   // 下载
        UploadArrow = 51,     // 上传
        SignPost = 52,        // 指路牌
        TargetArrow = 53,     // 箭中靶心
        BowArrow = 54,        // 弓箭
        CursorPointer = 55,   // 鼠标光标
        CalloutArrow = 56,    // 气泡指引
        ImportArrow = 57,     // 导入
        ExportArrow = 58,     // 导出
        CornerInArrow = 59,   // 四角向内
        CornerOutArrow = 60,  // 四角向外
        ExternalLink = 61,    // 外部链接
        RetweetArrow = 62,    // 双向循环转发
        MergeArrow = 63,      // 合流
        BranchArrow = 64,     // 分流
        SplitArrow = 65,      // 一进三出
        TrendUp = 66,         // 上升趋势
        TrendDown = 67,       // 下降趋势
        SortAsc = 68,         // 排序箭头
        SpiralArrow = 69,     // 螺旋箭头
        Plus = 70,            // 加 ＋
        Minus = 71,           // 减 －
        Multiply = 72,        // 乘 ×
        Divide = 73,          // 除 ÷
        PlusMinus = 74,       // 正负 ±
        MinusPlus = 75,       // 负正 ∓
        Equal = 76,           // 等号 ＝
        NotEqual = 77,        // 不等号 ≠
        Approximately = 78,   // 约等号 ≈
        Percent = 79,         // 百分号 %
        LessThan = 80,        // 小于 &lt;
        GreaterThan = 81,     // 大于 &gt;
        LessEqual = 82,       // 小于等于 ≤
        GreaterEqual = 83     // 大于等于 ≥
    }
    /// <summary>箭头渲染风格：22 种上色/描边/质感。</summary>
    public enum HArrowStyle
    {
        Classic = 0,     // 经典实心描边
        Outline = 1,     // 线性描边
        Flat = 2,        // 纯色扁平
        Gradient = 3,    // 渐变
        Neon = 4,        // 霓虹辉光
        Emboss = 5,      // 浮雕
        Shadow = 6,      // 投影
        HandDrawn = 7,   // 手绘墨感
        Tech = 8,        // 科技方角
        Glossy = 9,      // 高光釉面
        Chalk = 10,      // 粉笔
        InkBrush = 11,   // 浓墨
        Gold = 12,       // 鎏金
        Glass = 13,      // 玻璃
        Minimal = 14,    // 极细
        Geometric = 15,  // 几何
        Watercolor = 16, // 水彩
        Stamp = 17,      // 印章
        Sticker = 18,    // 白边贴纸
        Comet = 19,      // 流光拖尾
        Laser = 20,      // 激光
        Antique = 21     // 古铜
    }
    /// <summary>箭头指向（八方向），运算符号不受方向影响。</summary>
    public enum HArrowDirection
    {
        Right = 0,
        DownRight = 1,
        Down = 2,
        DownLeft = 3,
        Left = 4,
        UpLeft = 5,
        Up = 6,
        UpRight = 7
    }
    /// <summary>
    /// 箭头控件（继承 HToolAnimBase）：84 种箭头/符号种类 × 22 种渲染风格 × 8 个方向；
    /// Running 时虚线流动、彗尾/霓虹/激光相位产生动画。绘制核心见 <see cref="HArrowGlyph"/>，
    /// 数值框等按钮可直接复用。
    /// </summary>
    [DefaultProperty("ArrowType")]
    [HDescriptionLanguage("箭头控件：84种箭头/加减乘除符号，22种风格，8个方向")]
    public class HArrow : HToolAnimBase
    {
        private HArrowType _type = HArrowType.Arrow;
        private HArrowStyle _style = HArrowStyle.Classic;
        private HArrowDirection _dir = HArrowDirection.Right;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.SeaBlue);
        protected override HToolPalette ClassicPalette => _classic;
        /// <summary>箭头/符号种类。</summary>
        [HCategoryLanguage("箭头"), HDisplayNameLanguage("箭头或运算符号种类"), HDescriptionLanguage("箭头或运算符号种类（84种）"), Browsable(true)]
        [DefaultValue(HArrowType.Arrow)]
        public HArrowType ArrowType
        {
            get => _type;
            set { _type = value; Invalidate(); }
        }
        /// <summary>渲染风格。</summary>
        [HCategoryLanguage("箭头"), HDisplayNameLanguage("箭头渲染风格"), HDescriptionLanguage("箭头渲染风格（22种）"), Browsable(true)]
        [DefaultValue(HArrowStyle.Classic)]
        public HArrowStyle ArrowStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>指向（八方向）。</summary>
        [HCategoryLanguage("箭头"), HDisplayNameLanguage("箭头方向"), HDescriptionLanguage("箭头指向，八个方向"), Browsable(true)]
        [DefaultValue(HArrowDirection.Right)]
        public HArrowDirection ArrowDirection
        {
            get => _dir;
            set { _dir = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            if (a.Width <= 3f || a.Height <= 3f) { PaintPlacedText(g); return; }
            HArrowGlyph.Draw(g, a, _type, _style, _dir, Palette.Main, Running ? Phase : 0f);
            PaintPlacedText(g);
        }
    }
    /// <summary>
    /// 箭头图元绘制核心（程序集内部复用）：在指定矩形内按 种类×风格×方向 绘制，
    /// 全程对退化尺寸、负宽高、空路径免疫。
    /// </summary>
    internal static class HArrowGlyph
    {
        private const float Deg2Rad = (float)(Math.PI / 180.0);
        // 直线族参数：头型、尾型、杆型、线宽级别(0细/1常/2粗)、杆缩进(0满长/1短)
        private struct Spec
        {
            public byte Head, Tail, Shaft, W, Inset;
            public Spec(byte h, byte t, byte s, byte w, byte inset)
            { Head = h; Tail = t; Shaft = s; W = w; Inset = inset; }
        }
        private static readonly Spec[] Straight =
        {
            new Spec(0,0,0,1,0),   // 0 Arrow
            new Spec(6,0,0,0,0),   // 1 ThinArrow
            new Spec(7,0,1,2,0),   // 2 ThickArrow
            new Spec(1,0,0,1,0),   // 3 OpenArrow
            new Spec(2,1,9,2,0),   // 4 BlockArrow（9=实心块杆）
            new Spec(4,0,1,1,0),   // 5 HollowArrow
            new Spec(3,0,0,1,0),   // 6 DartArrow
            new Spec(3,2,1,1,0),   // 7 SpearArrow
            new Spec(0,0,0,1,2),   // 8 LongArrow（外伸）
            new Spec(0,0,0,1,1),   // 9 ShortArrow
            new Spec(0,0,3,1,0),   // 10 DashedArrow
            new Spec(0,0,4,1,0),   // 11 DottedArrow
            new Spec(0,2,0,1,0),   // 12 TailedArrow
            new Spec(5,0,0,1,0),   // 13 NotchedArrow
            new Spec(0,2,2,0,0),   // 14 FancyArrow
            new Spec(0,0,8,1,0),   // 15 CometArrow
            new Spec(3,2,7,1,0),   // 16 QuillArrow
            new Spec(0,0,1,2,0),   // 17 RoundedArrow
            new Spec(6,0,1,0,0),   // 18 TinyHeadArrow
            new Spec(7,0,1,2,0),   // 19 BigHeadArrow
            new Spec(0,9,9,1,0),   // 20 TriangleOnly（无杆，头放大）
            new Spec(8,0,0,1,0),   // 21 HarpoonArrow
            new Spec(0,0,2,1,0),   // 22 DoubleLineArrow
            new Spec(0,0,5,1,0),   // 23 ZigzagArrow
            new Spec(0,0,6,1,0),   // 24 WavyArrow
        };
        private struct Look
        {
            public Color Main;
            public Color Main2;
            public Color Edge;
            public float PenScale;
            public bool Fill;
            public bool Gradient;
            public int GlowAlpha;
            public bool ShadowPass;
            public bool StickerPass;
        }
        public static void Draw(Graphics g, RectangleF area, HArrowType type, HArrowStyle style,
            HArrowDirection dir, Color baseColor, float phase)
        {
            if (area.Width <= 2f || area.Height <= 2f) return;
            float L = Math.Min(area.Width, area.Height) * 0.90f;
            if (L < 4f) return;
            // 运算符号（70 起）没有方向语义，不随 ArrowDirection 旋转
            bool isOperator = (int)type >= 70;
            float angle = isOperator ? 0f : (int)dir * 45f;
            var state = g.BeginContainer();
            g.SetClip(area, CombineMode.Intersect);
            g.TranslateTransform(area.X + area.Width / 2f, area.Y + area.Height / 2f);
            if (Math.Abs(angle) > 0.01f) g.RotateTransform(angle);
            try
            {
                var look = Resolve(style, baseColor, phase);
                // 特效底版：投影 / 白边贴纸 / 辉光
                if (style == HArrowStyle.Shadow)
                {
                    g.TranslateTransform(L * 0.05f, L * 0.07f);
                    Render(g, type, ShadowLook(), L, phase);
                    g.TranslateTransform(-L * 0.05f, -L * 0.07f);
                }
                else if (style == HArrowStyle.Sticker)
                {
                    Render(g, type, StickerLook(), L, phase);
                }
                else if (look.GlowAlpha > 0)
                {
                    Render(g, type, GlowLook(baseColor, look.GlowAlpha, 2.6f), L, phase);
                    Render(g, type, GlowLook(baseColor, look.GlowAlpha / 2, 1.7f), L, phase);
                }
                Render(g, type, look, L, phase);
                // 风格叠加
                if (style == HArrowStyle.Comet) DrawCometTrail(g, L, baseColor, phase);
                else if (style == HArrowStyle.Tech) DrawTechTicks(g, L, baseColor);
                else if (style == HArrowStyle.Glossy) DrawGloss(g, L, baseColor);
            }
            finally
            {
                g.EndContainer(state);
            }
        }
        #region 风格解析
        private static Color Mix(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            int r = (int)(a.R + (b.R - a.R) * t);
            int gg = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, gg, bl);
        }
        private static Color Light(Color c, double t) => Mix(c, Color.White, t);
        private static Color Dark(Color c, double t) => Mix(c, Color.Black, t);
        private static Color Alpha(Color c, int a) => Color.FromArgb(Math.Max(0, Math.Min(255, a)), c.R, c.G, c.B);
        private static Look Resolve(HArrowStyle s, Color c, float phase)
        {
            var lk = new Look
            {
                Main = c,
                Main2 = Light(c, 0.35),
                Edge = Dark(c, 0.38),
                PenScale = 1f,
                Fill = true
            };
            int pulse = (int)(Math.Sin(phase) * 30);
            switch (s)
            {
                case HArrowStyle.Outline:
                    lk.Fill = false; lk.Edge = c; lk.PenScale = 1.35f; break;
                case HArrowStyle.Flat:
                    lk.Edge = Color.Empty; break;
                case HArrowStyle.Gradient:
                    lk.Gradient = true; lk.Main = Light(c, 0.4); lk.Main2 = Dark(c, 0.2); break;
                case HArrowStyle.Neon:
                    lk.Main = Light(c, 0.25); lk.Edge = Color.White;
                    lk.GlowAlpha = 130 + pulse; lk.PenScale = 1.1f; break;
                case HArrowStyle.Emboss:
                    lk.Main = Light(c, 0.12); lk.Edge = Dark(c, 0.3); lk.Main2 = Dark(c, 0.18);
                    lk.Gradient = true; break;
                case HArrowStyle.HandDrawn:
                    lk.Main = Dark(c, 0.12); lk.Edge = Dark(c, 0.45); lk.PenScale = 1.05f; break;
                case HArrowStyle.Tech:
                    lk.Edge = Dark(c, 0.35); break;
                case HArrowStyle.Glossy:
                    lk.Gradient = true; lk.Main = Light(c, 0.55); lk.Main2 = c; lk.Edge = Dark(c, 0.3); break;
                case HArrowStyle.Chalk:
                    lk.Main = Alpha(c, 205); lk.Edge = Color.Empty; lk.PenScale = 1.25f; break;
                case HArrowStyle.InkBrush:
                    lk.Main = Dark(c, 0.3); lk.Edge = Dark(c, 0.55); lk.PenScale = 1.3f; break;
                case HArrowStyle.Gold:
                    lk.Gradient = true;
                    lk.Main = Color.FromArgb(247, 215, 110);
                    lk.Main2 = Color.FromArgb(176, 120, 24);
                    lk.Edge = Color.FromArgb(122, 80, 14); break;
                case HArrowStyle.Glass:
                    lk.Main = Alpha(c, 85); lk.Edge = c; lk.PenScale = 1.05f; break;
                case HArrowStyle.Minimal:
                    lk.Fill = false; lk.Edge = c; lk.PenScale = 0.6f; break;
                case HArrowStyle.Watercolor:
                    lk.Main = Alpha(c, 140); lk.Edge = Color.Empty; lk.Main2 = Alpha(c, 100);
                    lk.Gradient = true; break;
                case HArrowStyle.Stamp:
                    lk.Main = Alpha(c, 205); lk.Edge = Alpha(Dark(c, 0.3), 200); break;
                case HArrowStyle.Comet:
                    lk.Edge = Dark(c, 0.25); break;
                case HArrowStyle.Laser:
                    lk.Main = Light(c, 0.55); lk.Edge = Color.White;
                    lk.GlowAlpha = 150 + pulse; lk.PenScale = 0.85f; break;
                case HArrowStyle.Antique:
                    lk.Gradient = true;
                    lk.Main = Color.FromArgb(176, 138, 78);
                    lk.Main2 = Color.FromArgb(110, 78, 38);
                    lk.Edge = Color.FromArgb(84, 60, 30); break;
            }
            return lk;
        }
        private static Look ShadowLook() => new Look
        {
            Main = Color.FromArgb(110, 20, 24, 30),
            Edge = Color.FromArgb(110, 20, 24, 30),
            PenScale = 1f,
            Fill = true
        };
        private static Look StickerLook() => new Look
        {
            Main = Color.White,
            Edge = Color.White,
            PenScale = 3.2f,
            Fill = true
        };
        private static Look GlowLook(Color c, int alpha, float pen) => new Look
        {
            Main = Alpha(c, alpha / 3),
            Edge = Alpha(c, alpha),
            PenScale = pen,
            Fill = true
        };
        #endregion
        #region 基础绘制原语
        private static float BasePen(float L, Look lk) => L * 0.085f * lk.PenScale;
        private static Pen NewPen(Look lk, float w, float L, float phase, bool dash)
        {
            var pen = new Pen(lk.Edge.IsEmpty ? lk.Main : lk.Edge, Math.Max(1f, w))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            if (dash)
            {
                pen.DashStyle = DashStyle.Dash;
                float period = Math.Max(4f, w * 6f);
                pen.DashOffset = (phase / (float)(2.0 * Math.PI)) * period;
            }
            return pen;
        }
        private static Brush FillBrush(Look lk, RectangleF box)
        {
            if (lk.Gradient && box.Width > 1f && box.Height > 1f)
            {
                var b = new RectangleF(box.X, box.Y, box.Width, box.Height);
                return new LinearGradientBrush(b, lk.Main, lk.Main2, 0f);
            }
            return new SolidBrush(lk.Main);
        }
        private static void FillPoly(Graphics g, Look lk, params PointF[] pts)
        {
            if (pts == null || pts.Length < 3) return;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int i = 0; i < pts.Length; i++)
            {
                if (pts[i].X < x0) x0 = pts[i].X;
                if (pts[i].Y < y0) y0 = pts[i].Y;
                if (pts[i].X > x1) x1 = pts[i].X;
                if (pts[i].Y > y1) y1 = pts[i].Y;
            }
            if (x1 - x0 < 0.5f || y1 - y0 < 0.5f) return;
            using (var path = new GraphicsPath())
            {
                path.AddLines(pts);
                path.CloseFigure();
                if (lk.Fill)
                {
                    using (var b = FillBrush(lk, new RectangleF(x0, y0, x1 - x0, y1 - y0)))
                        g.FillPath(b, path);
                }
                if (!lk.Edge.IsEmpty)
                {
                    float pw = BasePen((x1 - x0) + Math.Abs(x0), lk) * 0.5f;
                    using (var p = new Pen(lk.Edge, Math.Max(0.75f, Math.Min(3f, pw))) { LineJoin = LineJoin.Round })
                        g.DrawPath(p, path);
                }
            }
        }
        private static void StrokeLine(Graphics g, Look lk, float pw, float x1, float y1, float x2, float y2,
            bool dash = false, float phase = 0f, float L = 100f, Color? color = null)
        {
            Color c = color ?? (lk.Edge.IsEmpty ? lk.Main : lk.Edge);
            if (color.HasValue) c = color.Value;
            using (var p = new Pen(c, Math.Max(1f, pw)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (dash)
                {
                    p.DashStyle = DashStyle.Dash;
                    float period = Math.Max(4f, pw * 6f);
                    p.DashOffset = (phase / (float)(2.0 * Math.PI)) * period;
                }
                g.DrawLine(p, x1, y1, x2, y2);
            }
        }
        /// <summary>在 (tip) 处沿 +x 方向绘制指定头型。</summary>
        private static void DrawHead(Graphics g, Look lk, int kind, float tip, float hs, float pw)
        {
            float k = 1f, half = 0.62f, len = 1.0f;
            switch (kind)
            {
                case 1: // 开口 V
                    StrokeLine(g, lk, pw * 1.15f, tip, 0, tip - hs * 0.95f, -hs * 0.72f);
                    StrokeLine(g, lk, pw * 1.15f, tip, 0, tip - hs * 0.95f, hs * 0.72f);
                    return;
                case 3: k = 1.25f; half = 0.34f; break;   // 飞镖窄长
                case 4: // 空心：只描边
                    using (var path = new GraphicsPath())
                    {
                        var bp = new[]
                        {
                            new PointF(tip, 0),
                            new PointF(tip - hs, -hs * 0.62f),
                            new PointF(tip - hs, hs * 0.62f)
                        };
                        path.AddLines(bp);
                        path.CloseFigure();
                        var hollow = lk; hollow.Fill = false;
                        FillPoly(g, hollow, bp);
                    }
                    return;
                case 5: // 燕尾：尾部凹口
                    FillPoly(g, lk,
                        new PointF(tip, 0),
                        new PointF(tip - hs, -hs * 0.62f),
                        new PointF(tip - hs * 0.55f, 0),
                        new PointF(tip - hs, hs * 0.62f));
                    return;
                case 6: k = 0.6f; break;                 // 小头
                case 7: k = 1.3f; half = 0.7f; break;    // 大头
                case 8: // 鱼叉
                    StrokeLine(g, lk, pw * 1.1f, tip, 0, tip - hs * 0.9f, -hs * 0.8f);
                    StrokeLine(g, lk, pw * 1.1f, tip, 0, tip - hs * 0.9f, hs * 0.8f);
                    StrokeLine(g, lk, pw, tip, 0, tip - hs * 0.85f, 0);
                    return;
                case 9: return;                          // 无头
            }
            float h = hs * k, bk = tip - h * len;
            FillPoly(g, lk,
                new PointF(tip, 0),
                new PointF(bk, -h * half),
                new PointF(bk, h * half));
        }
        /// <summary>在任意点按角度（弧度）绘制三角头。</summary>
        private static void DrawHeadAt(Graphics g, Look lk, PointF tip, float ang, float hs, float half = 0.62f)
        {
            float dx = (float)Math.Cos(ang), dy = (float)Math.Sin(ang);
            float px = -dy, py = dx;
            var bc = new PointF(tip.X - dx * hs, tip.Y - dy * hs);
            FillPoly(g, lk,
                tip,
                new PointF(bc.X + px * hs * half, bc.Y + py * hs * half),
                new PointF(bc.X - px * hs * half, bc.Y - py * hs * half));
        }
        private static void DrawTail(Graphics g, Look lk, int kind, float x, float half, float pw)
        {
            switch (kind)
            {
                case 1:
                    StrokeLine(g, lk, pw * 1.1f, x, -half, x, half);
                    break;
                case 2:
                    StrokeLine(g, lk, pw * 1.05f, x, 0, x + half * 0.9f, -half * 0.8f);
                    StrokeLine(g, lk, pw * 1.05f, x, 0, x + half * 0.9f, half * 0.8f);
                    break;
                case 3:
                    StrokeLine(g, lk, pw * 1.2f, x, -half * 1.35f, x, half * 1.35f);
                    break;
                case 4:
                    using (var b = new SolidBrush(lk.Edge.IsEmpty ? lk.Main : lk.Edge))
                        g.FillEllipse(b, x - pw, -pw, pw * 2f, pw * 2f);
                    break;
            }
        }
        private static PointF[] ArcPoints(float cx, float cy, float r, float a0, float a1, int n)
        {
            var pts = new PointF[n + 1];
            for (int i = 0; i <= n; i++)
            {
                double a = a0 + (a1 - a0) * i / n;
                pts[i] = new PointF(cx + r * (float)Math.Cos(a), cy + r * (float)Math.Sin(a));
            }
            return pts;
        }
        private static void StrokePath(Graphics g, Look lk, float pw, PointF[] pts)
        {
            if (pts == null || pts.Length < 2) return;
            Color c = lk.Edge.IsEmpty ? lk.Main : lk.Edge;
            using (var p = new Pen(c, Math.Max(1f, pw)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var path = new GraphicsPath())
            {
                path.AddLines(pts);
                g.DrawPath(p, path);
            }
        }
        private static void FillEllipseBox(Graphics g, Look lk, RectangleF box, float pw)
        {
            if (box.Width <= 1f || box.Height <= 1f) return;
            using (var b = FillBrush(lk, box))
                g.FillEllipse(b, box);
            if (!lk.Edge.IsEmpty)
                using (var p = new Pen(lk.Edge, Math.Max(0.75f, pw)))
                    g.DrawEllipse(p, box);
        }
        #endregion
        #region 渲染分发
        private static void Render(Graphics g, HArrowType t, Look lk, float L, float phase)
        {
            int i = (int)t;
            float pw = BasePen(L, lk);
            float H = L / 2f;
            if (i < Straight.Length) DrawStraight(g, lk, Straight[i], L, pw, phase);
            else if (i <= 31) DrawCurvedFamily(g, t, lk, L, pw, phase);
            else if (i <= 51) DrawMultiFamily(g, t, lk, L, pw, phase);
            else if (i <= 69) DrawSymbolic(g, t, lk, L, pw, phase);
            else DrawOperator(g, t, lk, L, pw);
        }
        #endregion
        #region 直线族
        private static void DrawStraight(Graphics g, Look lk, Spec s, float L, float pw0, float phase)
        {
            float H = L / 2f;
            float pw = pw0 * (s.W == 0 ? 0.62f : s.W == 2 ? 1.7f : 1f);
            float hs = L * 0.20f * (s.W == 2 ? 1.15f : 1f);
            float xTip = H * 0.94f;
            float xBack = s.Inset == 1 ? -H * 0.12f : -H * 0.92f;
            if (s.Inset == 2) xBack = -H * 1.02f;
            float shaftEnd = xTip - hs * 0.92f;
            float half = Math.Max(pw * 1.3f, hs * 0.3f);
            if (s.Shaft == 9)
            {
                // 实心块杆：仅三角形（TriangleOnly）或三角 + 矩形一体（BlockArrow）
                if (s.Head == 0 && s.Tail == 9)
                {
                    DrawHead(g, lk, 0, xTip, hs * 1.9f, pw);
                    return;
                }
                float bodyHalf = hs * 0.42f;
                FillPoly(g, lk,
                    new PointF(xBack, -bodyHalf),
                    new PointF(shaftEnd, -bodyHalf),
                    new PointF(shaftEnd, bodyHalf),
                    new PointF(xBack, bodyHalf));
                FillPoly(g, lk,
                    new PointF(xTip, 0),
                    new PointF(shaftEnd, -bodyHalf * 1.55f),
                    new PointF(shaftEnd, bodyHalf * 1.55f));
                DrawTail(g, lk, s.Tail, xBack, bodyHalf, pw);
                return;
            }
            DrawShaft(g, lk, s.Shaft, xBack, shaftEnd, half, pw, phase, L);
            DrawHead(g, lk, s.Head, xTip, hs, pw);
            DrawTail(g, lk, s.Tail, xBack, hs * 0.5f, pw);
        }
        private static void DrawShaft(Graphics g, Look lk, int kind, float x0, float x1, float half,
            float pw, float phase, float L)
        {
            Color c = lk.Edge.IsEmpty ? lk.Main : lk.Edge;
            switch (kind)
            {
                case 1: // 粗胶囊
                    StrokeLine(g, lk, pw * 1.9f, x0, 0, x1, 0, false, phase, L);
                    break;
                case 2: // 双线杆
                    StrokeLine(g, lk, pw * 0.8f, x0, -half * 0.85f, x1, -half * 0.85f);
                    StrokeLine(g, lk, pw * 0.8f, x0, half * 0.85f, x1, half * 0.85f);
                    break;
                case 3:
                    StrokeLine(g, lk, pw, x0, 0, x1, 0, true, phase, L);
                    break;
                case 4:
                    using (var p = new Pen(c, Math.Max(1f, pw * 1.2f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        p.DashStyle = DashStyle.Dot;
                        g.DrawLine(p, x0, 0, x1, 0);
                    }
                    break;
                case 5: // 折线
                    {
                        int n = 6;
                        var pts = new PointF[n + 1];
                        for (int i = 0; i <= n; i++)
                        {
                            float t = i / (float)n;
                            pts[i] = new PointF(x0 + (x1 - x0) * t, (i % 2 == 0 ? -1 : 1) * half * 0.7f);
                        }
                        StrokePath(g, lk, pw * 0.9f, pts);
                    }
                    break;
                case 6: // 波浪
                    {
                        using (var path = new GraphicsPath())
                        {
                            float amp = half * 0.8f, seg = (x1 - x0) / 4f;
                            path.AddLine(x0, 0, x0 + seg * 0.5f, 0);
                            for (float x = x0; x < x1 - seg * 0.9f; x += seg)
                            {
                                path.AddBezier(x + seg * 0.5f, 0, x + seg * 0.55f, -amp,
                                    x + seg * 0.95f, -amp, x + seg, 0);
                                path.AddBezier(x + seg, 0, x + seg * 1.05f, amp,
                                    x + seg * 1.45f, amp, x + seg * 1.5f, 0);
                            }
                            path.AddLine(Math.Min(x1, x0 + (x1 - x0) - seg * 0.5f), 0, x1, 0);
                            using (var p = new Pen(c, Math.Max(1f, pw)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                                g.DrawPath(p, path);
                        }
                    }
                    break;
                case 7: // 锥感线
                    StrokeLine(g, lk, pw * 1.3f, x0, 0, x1, 0);
                    break;
                case 8: // 彗尾：逐段淡出
                    {
                        int n = 5;
                        for (int i = 0; i < n; i++)
                        {
                            float t0 = i / (float)n, t1 = (i + 0.55f) / n;
                            int a = (int)(40 + 180 * (i / (float)n));
                            StrokeLine(g, lk, pw * (0.5f + 0.55f * i / n),
                                x0 + (x1 - x0) * t0, 0, x0 + (x1 - x0) * t1, 0,
                                false, phase, L, Alpha(c, a));
                        }
                    }
                    break;
                default:
                    StrokeLine(g, lk, pw, x0, 0, x1, 0);
                    break;
            }
        }
        #endregion
        #region 弯曲族（25..31）
        private static void DrawCurvedFamily(Graphics g, HArrowType t, Look lk, float L, float pw, float phase)
        {
            float H = L / 2f;
            switch (t)
            {
                case HArrowType.CurvedArrow:
                    {
                        float cx = 0, cy = H * 0.15f, r = H * 0.82f;
                        float a0 = 158f * Deg2Rad, a1 = -18f * Deg2Rad;
                        var pts = ArcPoints(cx, cy, r, a0, a1, 36);
                        StrokePath(g, lk, pw, pts);
                        var tip = pts[pts.Length - 1];
                        float tan = a1 + Deg2Rad * 90f;
                        DrawHeadAt(g, lk, tip, tan, L * 0.18f);
                    }
                    break;
                case HArrowType.BentArrow:
                    {
                        using (var path = new GraphicsPath())
                        {
                            float cx = -H * 0.1f, cy = H * 0.45f, r = H * 0.5f;
                            path.AddLine(-H * 0.92f, H * 0.45f, cx, H * 0.45f);
                            // 四分之一弧：从下方(90°)转到右方(0°)
                            var arc = ArcPoints(cx, cy - r, r, 90f * Deg2Rad, 0, 16);
                            path.AddLines(arc);
                            path.AddLine(cx + r, cy - r, H * 0.62f, cy - r);
                            using (var p = PenFor(lk, pw)) g.DrawPath(p, path);
                            DrawHead(g, lk, 0, H * 0.94f, L * 0.18f, pw);
                        }
                    }
                    break;
                case HArrowType.LoopArrow:
                    {
                        float r = H * 0.66f, gap = 42f * Deg2Rad;
                        float a1 = -gap + (phase * 0f);
                        var pts = ArcPoints(0, 0, r, gap, (float)(Math.PI * 2 - gap * 0.6), 48);
                        StrokePath(g, lk, pw, pts);
                        var tip = pts[pts.Length - 1];
                        float endA = (float)(Math.PI * 2 - gap * 0.6);
                        DrawHeadAt(g, lk, tip, endA + Deg2Rad * 90f, L * 0.17f);
                    }
                    break;
                case HArrowType.UTurnArrow:
                    {
                        using (var path = new GraphicsPath())
                        {
                            float r = H * 0.38f, top = -H * 0.28f;
                            path.AddLine(-H * 0.48f, H * 0.72f, -H * 0.48f, top);
                            var arc = ArcPoints(0, top, r, 180f * Deg2Rad, 0, 24);
                            path.AddLines(arc);
                            path.AddLine(H * 0.48f, top, H * 0.48f, H * 0.5f);
                            using (var p = PenFor(lk, pw)) g.DrawPath(p, path);
                            DrawHeadAt(g, lk, new PointF(H * 0.48f, H * 0.66f), Deg2Rad * 90f, L * 0.17f);
                        }
                    }
                    break;
                case HArrowType.RedoArrow:
                    {
                        using (var path = new GraphicsPath())
                        {
                            float cx = -H * 0.05f, cy = -H * 0.02f, r = H * 0.5f;
                            path.AddLine(-H * 0.78f, H * 0.42f, cx, H * 0.42f);
                            var arc = ArcPoints(cx, cy, r, 90f * Deg2Rad, -32f * Deg2Rad, 22);
                            path.AddLines(arc);
                            using (var p = PenFor(lk, pw)) g.DrawPath(p, path);
                            var tip = arc[arc.Length - 1];
                            DrawHeadAt(g, lk, tip, -32f * Deg2Rad + Deg2Rad * 90f, L * 0.17f);
                        }
                    }
                    break;
                case HArrowType.ExpandArrow:
                    DoubleLineAtAngle(g, lk, 45f, L, pw);
                    break;
                case HArrowType.ShrinkArrow:
                    {
                        // 两支对角短箭头，头都指向中心
                        DrawHeadAt(g, lk, new PointF(-H * 0.12f, -H * 0.12f), 225f * Deg2Rad, L * 0.15f);
                        StrokeLine(g, lk, pw, -H * 0.62f, -H * 0.62f, -H * 0.2f, -H * 0.2f);
                        DrawHeadAt(g, lk, new PointF(H * 0.12f, H * 0.12f), 45f * Deg2Rad, L * 0.15f);
                        StrokeLine(g, lk, pw, H * 0.62f, H * 0.62f, H * 0.2f, H * 0.2f);
                    }
                    break;
            }
        }
        private static Pen PenFor(Look lk, float pw)
            => new Pen(lk.Edge.IsEmpty ? lk.Main : lk.Edge, Math.Max(1f, pw))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
        private static void DoubleLineAtAngle(Graphics g, Look lk, float deg, float L, float pw)
        {
            float H = L / 2f * 0.82f, a = deg * Deg2Rad;
            float dx = (float)Math.Cos(a), dy = (float)Math.Sin(a);
            var p1 = new PointF(-dx * H, -dy * H);
            var p2 = new PointF(dx * H, dy * H);
            StrokeLine(g, lk, pw, p1.X, p1.Y, p2.X, p2.Y);
            DrawHeadAt(g, lk, p2, a, L * 0.16f);
            DrawHeadAt(g, lk, p1, a + (float)Math.PI, L * 0.16f);
        }
        #endregion
        #region 多箭头族（32..51）
        private static void DrawMultiFamily(Graphics g, HArrowType t, Look lk, float L, float pw, float phase)
        {
            float H = L / 2f;
            switch (t)
            {
                case HArrowType.DoubleArrow:
                    DoubleShaft(g, lk, L, pw, 0, false, phase);
                    break;
                case HArrowType.DoubleOpenArrow:
                    DoubleShaft(g, lk, L, pw, 1, false, phase);
                    break;
                case HArrowType.DoubleBlockArrow:
                    DoubleShaft(g, lk, L, pw * 1.5f, 2, false, phase);
                    break;
                case HArrowType.DoubleThinArrow:
                    DoubleShaft(g, lk, L, pw * 0.6f, 6, false, phase);
                    break;
                case HArrowType.DoubleDashedArrow:
                    DoubleShaft(g, lk, L, pw, 0, true, phase);
                    break;
                case HArrowType.TripleArrow:
                case HArrowType.QuadArrow:
                    {
                        int n = t == HArrowType.TripleArrow ? 3 : 4;
                        for (int k = 0; k < n; k++)
                        {
                            float yy = n == 3 ? (k - 1) * H * 0.42f : (k - 1.5f) * H * 0.34f;
                            float len = L * (n == 3 ? 0.84f : 0.80f);
                            float x0 = -len * 0.46f, x1 = len * 0.46f, hs = L * 0.15f;
                            StrokeLine(g, lk, pw * 0.85f, x0, yy, x1 - hs, yy);
                            DrawHeadAt(g, lk, new PointF(x1, yy), 0, hs);
                        }
                    }
                    break;
                case HArrowType.Chevron:
                    DrawChevrons(g, lk, L, pw, 1, false);
                    break;
                case HArrowType.ChevronDouble:
                    DrawChevrons(g, lk, L, pw, 2, false);
                    break;
                case HArrowType.ChevronTriple:
                    DrawChevrons(g, lk, L, pw, 3, false);
                    break;
                case HArrowType.ChevronRound:
                    DrawChevrons(g, lk, L, pw, 1, true);
                    break;
                case HArrowType.UpDownArrow:
                    DoubleShaft(g, lk, L * 0.86f, pw, 0, false, phase, true);
                    break;
                case HArrowType.FourWayArrow:
                    DoubleShaft(g, lk, L * 0.9f, pw * 0.9f, 0, false, phase, false);
                    DoubleShaft(g, lk, L * 0.9f, pw * 0.9f, 0, false, phase, true);
                    break;
                case HArrowType.ResizeHorizontal:
                    DoubleShaft(g, lk, L * 0.86f, pw, 0, false, phase);
                    foreach (float k in new[] { -0.28f, 0.28f })
                        StrokeLine(g, lk, pw * 0.7f, H * k, -H * 0.12f, H * k, H * 0.12f);
                    break;
                case HArrowType.ResizeVertical:
                    DoubleShaft(g, lk, L * 0.86f, pw, 0, false, phase, true);
                    foreach (float k in new[] { -0.28f, 0.28f })
                        StrokeLine(g, lk, pw * 0.7f, -H * 0.12f, H * k, H * 0.12f, H * k);
                    break;
                case HArrowType.SwapVertical:
                    SwapArrows(g, lk, L, pw, false);
                    break;
                case HArrowType.SwapHorizontal:
                    SwapArrows(g, lk, L, pw, true);
                    break;
                case HArrowType.ShuffleArrow:
                    {
                        StrokeLine(g, lk, pw * 0.85f, -H * 0.7f, -H * 0.28f, H * 0.55f, -H * 0.28f);
                        DrawHeadAt(g, lk, new PointF(H * 0.7f, -H * 0.28f), 0, L * 0.14f);
                        StrokeLine(g, lk, pw * 0.85f, -H * 0.7f, H * 0.28f, H * 0.55f, H * 0.28f);
                        DrawHeadAt(g, lk, new PointF(H * 0.7f, H * 0.28f), 0, L * 0.14f);
                        var arc = ArcPoints(0, -H * 0.05f, H * 0.42f, -28f * Deg2Rad, 208f * Deg2Rad, 28);
                        StrokePath(g, lk, pw * 0.75f, arc);
                        DrawHeadAt(g, lk, arc[arc.Length - 1], 208f * Deg2Rad + Deg2Rad * 90f, L * 0.13f);
                    }
                    break;
                case HArrowType.DownloadArrow:
                case HArrowType.UploadArrow:
                    DrawUploadDownload(g, t == HArrowType.UploadArrow, lk, L, pw);
                    break;
            }
        }
        private static void DoubleShaft(Graphics g, Look lk, float L, float pw, int head, bool dash, float phase, bool vertical = false)
        {
            float h = L / 2f;
            float hs = L * 0.16f, x0 = -h * 0.84f, x1 = h * 0.84f;
            if (vertical)
            {
                StrokeLine(g, lk, pw, 0, x0, 0, x1, dash, phase, L);
                DrawHeadAt(g, lk, new PointF(0, x1), Deg2Rad * 90f, hs);
                DrawHeadAt(g, lk, new PointF(0, x0), -Deg2Rad * 90f, hs);
            }
            else
            {
                StrokeLine(g, lk, pw, x0, 0, x1, 0, dash, phase, L);
                if (head == 1)
                {
                    DrawHead(g, lk, 1, x1, hs, pw);
                    var st = g.Save();
                    g.RotateTransform(180f, MatrixOrder.Append);
                    DrawHead(g, lk, 1, x1, hs, pw);
                    g.Restore(st);
                }
                else
                {
                    DrawHead(g, lk, head == 2 ? 2 : (head == 6 ? 6 : 0), x1, hs * (head == 2 ? 1.2f : 1f), pw);
                    var st = g.Save();
                    g.RotateTransform(180f, MatrixOrder.Append);
                    DrawHead(g, lk, head == 2 ? 2 : (head == 6 ? 6 : 0), x1, hs * (head == 2 ? 1.2f : 1f), pw);
                    g.Restore(st);
                }
            }
        }
        private static void DrawChevrons(Graphics g, Look lk, float L, float pw, int count, bool round)
        {
            float H = L / 2f, s = H * 0.34f, gap = H * 0.42f;
            Color c = lk.Edge.IsEmpty ? lk.Main : lk.Edge;
            for (int i = 0; i < count; i++)
            {
                float cx = (i - (count - 1) / 2f) * gap;
                using (var p = new Pen(c, Math.Max(1f, pw * 1.35f))
                {
                    StartCap = round ? LineCap.Round : LineCap.Flat,
                    EndCap = round ? LineCap.Round : LineCap.Flat,
                    LineJoin = round ? LineJoin.Round : LineJoin.Miter
                })
                {
                    g.DrawLine(p, cx - s * 0.55f, -s, cx + s * 0.45f, 0);
                    g.DrawLine(p, cx + s * 0.45f, 0, cx - s * 0.55f, s);
                }
            }
        }
        private static void SwapArrows(Graphics g, Look lk, float L, float pw, bool horizontal)
        {
            float H = L / 2f, o = H * 0.3f, hs = L * 0.14f;
            if (!horizontal)
            {
                StrokeLine(g, lk, pw, -H * 0.62f, -o, H * 0.55f, -o);
                DrawHeadAt(g, lk, new PointF(H * 0.7f, -o), 0, hs);
                StrokeLine(g, lk, pw, H * 0.62f, o, -H * 0.55f, o);
                DrawHeadAt(g, lk, new PointF(-H * 0.7f, o), Deg2Rad * 180f, hs);
            }
            else
            {
                StrokeLine(g, lk, pw, -o, -H * 0.62f, -o, H * 0.55f);
                DrawHeadAt(g, lk, new PointF(-o, H * 0.7f), Deg2Rad * 90f, hs);
                StrokeLine(g, lk, pw, o, H * 0.62f, o, -H * 0.55f);
                DrawHeadAt(g, lk, new PointF(o, -H * 0.7f), -Deg2Rad * 90f, hs);
            }
        }
        private static void DrawUploadDownload(Graphics g, bool up, Look lk, float L, float pw)
        {
            float H = L / 2f;
            float dir = up ? -1f : 1f;
            // 杆
            StrokeLine(g, lk, pw, 0, -H * 0.55f * dir, 0, H * 0.35f * dir);
            DrawHeadAt(g, lk, new PointF(0, H * 0.58f * dir), dir * Deg2Rad * 90f, L * 0.17f);
            // 托盘
            float ty = H * 0.78f;
            StrokeLine(g, lk, pw * 1.1f, -H * 0.5f, ty, H * 0.5f, ty);
            StrokeLine(g, lk, pw, -H * 0.5f, ty, -H * 0.5f, ty - H * 0.14f);
            StrokeLine(g, lk, pw, H * 0.5f, ty, H * 0.5f, ty - H * 0.14f);
        }
        #endregion
        #region 语义图形（52..69）
        private static void DrawSymbolic(Graphics g, HArrowType t, Look lk, float L, float pw, float phase)
        {
            float H = L / 2f;
            switch (t)
            {
                case HArrowType.SignPost:
                    {
                        FillPoly(g, lk,
                            new PointF(-H * 0.42f, -H * 0.78f),
                            new PointF(-H * 0.26f, -H * 0.78f),
                            new PointF(-H * 0.26f, H * 0.8f),
                            new PointF(-H * 0.42f, H * 0.8f));
                        FillPoly(g, lk,
                            new PointF(-H * 0.34f, -H * 0.62f),
                            new PointF(H * 0.42f, -H * 0.62f),
                            new PointF(H * 0.78f, -H * 0.4f),
                            new PointF(H * 0.42f, -H * 0.18f),
                            new PointF(-H * 0.34f, -H * 0.18f));
                    }
                    break;
                case HArrowType.TargetArrow:
                    {
                        var ring = lk; ring.Fill = false;
                        for (int k = 0; k < 3; k++)
                        {
                            float r = H * (0.3f + 0.18f * k);
                            using (var p = new Pen(lk.Edge.IsEmpty ? lk.Main : lk.Edge, Math.Max(1f, pw * 0.8f)))
                                g.DrawEllipse(p, -r, -r, r * 2f, r * 2f);
                        }
                        float a = 205f * Deg2Rad;
                        var s = new PointF((float)Math.Cos(a) * H * 0.85f, (float)Math.Sin(a) * H * 0.85f);
                        StrokeLine(g, lk, pw, s.X, s.Y, 0, 0);
                        DrawHeadAt(g, lk, PointF.Empty, a, L * 0.15f);
                    }
                    break;
                case HArrowType.BowArrow:
                    {
                        var arc = ArcPoints(H * 0.42f, 0, H * 0.72f, -105f * Deg2Rad, 105f * Deg2Rad, 30);
                        StrokePath(g, lk, pw * 1.1f, arc);
                        var top = arc[0]; var bot = arc[arc.Length - 1];
                        StrokeLine(g, lk, pw * 0.6f, top.X, top.Y, bot.X, bot.Y);
                        StrokeLine(g, lk, pw * 0.9f, -H * 0.7f, 0, H * 0.7f, 0);
                        DrawHeadAt(g, lk, new PointF(H * 0.8f, 0), 0, L * 0.15f);
                        DrawTail(g, lk, 2, -H * 0.7f, H * 0.16f, pw);
                    }
                    break;
                case HArrowType.CursorPointer:
                    FillPoly(g, lk,
                        new PointF(-H * 0.55f, -H * 0.75f),
                        new PointF(H * 0.42f, -H * 0.18f),
                        new PointF(-H * 0.08f, -H * 0.08f),
                        new PointF(H * 0.02f, H * 0.52f),
                        new PointF(-H * 0.2f, H * 0.44f),
                        new PointF(-H * 0.3f, -H * 0.02f),
                        new PointF(-H * 0.6f, H * 0.05f));
                    break;
                case HArrowType.CalloutArrow:
                    {
                        var box = new RectangleF(-H * 0.82f, -H * 0.6f, H * 1.5f, H * 0.82f);
                        float rad = H * 0.2f;
                        using (var path = HDRound(box, rad))
                        using (var b = FillBrush(lk, box))
                            g.FillPath(b, path);
                        if (!lk.Edge.IsEmpty)
                            using (var path = HDRound(box, rad))
                            using (var p = new Pen(lk.Edge, Math.Max(1f, pw)))
                                g.DrawPath(p, path);
                        FillPoly(g, lk,
                            new PointF(-H * 0.42f, H * 0.18f),
                            new PointF(-H * 0.1f, H * 0.2f),
                            new PointF(-H * 0.4f, H * 0.5f));
                    }
                    break;
                case HArrowType.ImportArrow:
                    DrawBoxArrow(g, lk, L, pw, true);
                    break;
                case HArrowType.ExportArrow:
                    DrawBoxArrow(g, lk, L, pw, false);
                    break;
                case HArrowType.CornerInArrow:
                case HArrowType.CornerOutArrow:
                    DrawCorners(g, t == HArrowType.CornerInArrow, lk, L, pw);
                    break;
                case HArrowType.ExternalLink:
                    {
                        var box = new RectangleF(-H * 0.85f, -H * 0.1f, H * 0.85f, H * 0.85f);
                        using (var p = new Pen(lk.Edge.IsEmpty ? lk.Main : lk.Edge, Math.Max(1f, pw)))
                            g.DrawRectangle(p, box.X, box.Y, box.Width, box.Height);
                        StrokeLine(g, lk, pw, -H * 0.25f, H * 0.42f, H * 0.5f, -H * 0.35f);
                        DrawHeadAt(g, lk, new PointF(H * 0.62f, -H * 0.47f), -45f * Deg2Rad, L * 0.14f);
                        StrokeLine(g, lk, pw, H * 0.05f, -H * 0.62f, H * 0.66f, -H * 0.62f);
                        StrokeLine(g, lk, pw, H * 0.66f, -H * 0.62f, H * 0.66f, -H * 0.02f);
                    }
                    break;
                case HArrowType.RetweetArrow:
                    {
                        var a1 = ArcPoints(0, H * 0.06f, H * 0.6f, 165f * Deg2Rad, 20f * Deg2Rad, 30);
                        StrokePath(g, lk, pw * 0.95f, a1);
                        DrawHeadAt(g, lk, a1[a1.Length - 1], 20f * Deg2Rad + Deg2Rad * 90f, L * 0.14f);
                        var a2 = ArcPoints(0, -H * 0.06f, H * 0.6f, -15f * Deg2Rad, -160f * Deg2Rad, 30);
                        StrokePath(g, lk, pw * 0.95f, a2);
                        DrawHeadAt(g, lk, a2[a2.Length - 1], -160f * Deg2Rad + Deg2Rad * 90f, L * 0.14f);
                    }
                    break;
                case HArrowType.MergeArrow:
                    StrokeLine(g, lk, pw, -H * 0.72f, -H * 0.5f, H * 0.25f, 0);
                    StrokeLine(g, lk, pw, -H * 0.72f, H * 0.5f, H * 0.25f, 0);
                    StrokeLine(g, lk, pw, H * 0.25f, 0, H * 0.6f, 0);
                    DrawHeadAt(g, lk, new PointF(H * 0.74f, 0), 0, L * 0.15f);
                    break;
                case HArrowType.BranchArrow:
                    StrokeLine(g, lk, pw, -H * 0.6f, 0, -H * 0.25f, 0);
                    StrokeLine(g, lk, pw, -H * 0.25f, 0, H * 0.72f, -H * 0.5f);
                    StrokeLine(g, lk, pw, -H * 0.25f, 0, H * 0.72f, H * 0.5f);
                    DrawHeadAt(g, lk, new PointF(H * 0.78f, -H * 0.53f), -32f * Deg2Rad, L * 0.14f);
                    DrawHeadAt(g, lk, new PointF(H * 0.78f, H * 0.53f), 32f * Deg2Rad, L * 0.14f);
                    break;
                case HArrowType.SplitArrow:
                    StrokeLine(g, lk, pw, -H * 0.78f, 0, -H * 0.2f, 0);
                    foreach (float yy in new[] { -H * 0.5f, 0f, H * 0.5f })
                    {
                        StrokeLine(g, lk, pw, -H * 0.2f, 0, H * 0.55f, yy);
                        DrawHeadAt(g, lk, new PointF(H * 0.72f, yy), yy == 0 ? 0 : (yy < 0 ? -28f : 28f) * Deg2Rad, L * 0.13f);
                    }
                    break;
                case HArrowType.TrendUp:
                case HArrowType.TrendDown:
                    {
                        float f = t == HArrowType.TrendUp ? 1f : -1f;
                        var pts = new[]
                        {
                            new PointF(-H*0.78f, f*H*0.42f),
                            new PointF(-H*0.3f, -f*H*0.02f),
                            new PointF(H*0.05f, f*H*0.22f),
                            new PointF(H*0.55f, -f*H*0.38f)
                        };
                        StrokePath(g, lk, pw, pts);
                        DrawHeadAt(g, lk, new PointF(H*0.78f, -f*H*0.5f), f*-32f*Deg2Rad, L*0.15f);
                    }
                    break;
                case HArrowType.SortAsc:
                    {
                        StrokeLine(g, lk, pw, -H * 0.55f, H * 0.62f, -H * 0.55f, -H * 0.42f);
                        DrawHeadAt(g, lk, new PointF(-H * 0.55f, -H * 0.56f), -Deg2Rad * 90f, L * 0.13f);
                        for (int k = 0; k < 3; k++)
                        {
                            float y = -H * 0.4f + k * H * 0.42f;
                            float w = H * (0.55f - k * 0.16f);
                            StrokeLine(g, lk, pw * 0.85f, -H * 0.2f, y, -H * 0.2f + w, y);
                        }
                    }
                    break;
                case HArrowType.SpiralArrow:
                    {
                        var pts = new System.Collections.Generic.List<PointF>();
                        float turns = 1.6f;
                        int n = 60;
                        for (int i = 0; i <= n; i++)
                        {
                            float tt = i / (float)n;
                            float a = tt * turns * (float)(Math.PI * 2) - Deg2Rad * 90f;
                            float r = H * 0.72f * (1f - tt * 0.72f);
                            pts.Add(new PointF(r * (float)Math.Cos(a), r * (float)Math.Sin(a)));
                        }
                        StrokePath(g, lk, pw * 0.9f, pts.ToArray());
                        var last = pts[pts.Count - 1]; var prev = pts[pts.Count - 3];
                        float ang = (float)Math.Atan2(last.Y - prev.Y, last.X - prev.X);
                        DrawHeadAt(g, lk, last, ang, L * 0.13f);
                    }
                    break;
            }
        }
        private static GraphicsPath HDRound(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = Math.Min(rad * 2f, Math.Min(r.Width, r.Height));
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        private static void DrawBoxArrow(Graphics g, Look lk, float L, float pw, bool import)
        {
            float H = L / 2f;
            var box = import
                ? new RectangleF(H * 0.05f, -H * 0.62f, H * 0.85f, H * 1.24f)
                : new RectangleF(-H * 0.9f, -H * 0.62f, H * 0.85f, H * 1.24f);
            using (var p = new Pen(lk.Edge.IsEmpty ? lk.Main : lk.Edge, Math.Max(1f, pw)))
                g.DrawRectangle(p, box.X, box.Y, box.Width, box.Height);
            if (import)
            {
                StrokeLine(g, lk, pw, -H * 0.85f, 0, box.Left + H * 0.05f, 0);
                DrawHeadAt(g, lk, new PointF(box.Left + H * 0.2f, 0), 0f, L * 0.14f);
            }
            else
            {
                StrokeLine(g, lk, pw, box.Right - H * 0.05f, 0, H * 0.85f, 0);
                DrawHeadAt(g, lk, new PointF(H * 0.85f, 0), 0f, L * 0.14f);
            }
        }
        private static void DrawCorners(Graphics g, bool inward, Look lk, float L, float pw)
        {
            float H = L / 2f, d = H * 0.34f;
            // 四角 L 标记
            for (int cx = -1; cx <= 1; cx += 2)
                for (int cy = -1; cy <= 1; cy += 2)
                {
                    float bx = cx * H * 0.72f, by = cy * H * 0.72f;
                    StrokeLine(g, lk, pw * 0.8f, bx, by, bx - cx * d, by);
                    StrokeLine(g, lk, pw * 0.8f, bx, by, bx, by - cy * d);
                }
            float dir = inward ? -1f : 1f;
            DrawHeadAt(g, lk, new PointF(H * 0.22f * dir, H * 0.22f * dir), 45f * Deg2Rad, L * 0.13f);
            DrawHeadAt(g, lk, new PointF(-H * 0.22f * dir, -H * 0.22f * dir), 225f * Deg2Rad, L * 0.13f);
            if (!inward)
            {
                StrokeLine(g, lk, pw, 0, 0, H * 0.55f, H * 0.55f);
                StrokeLine(g, lk, pw, 0, 0, -H * 0.55f, -H * 0.55f);
            }
            else
            {
                StrokeLine(g, lk, pw, H * 0.55f, H * 0.55f, H * 0.3f, H * 0.3f);
                StrokeLine(g, lk, pw, -H * 0.55f, -H * 0.55f, -H * 0.3f, -H * 0.3f);
            }
        }
        #endregion
        #region 运算符号（70..83）
        private static void DrawOperator(Graphics g, HArrowType t, Look lk, float L, float pw0)
        {
            float H = L * 0.30f;       // 笔画半跨
            float pw = pw0 * 1.7f;
            float s = H * 0.72f;       // 半长
            Color c = lk.Edge.IsEmpty ? lk.Main : lk.Edge;
            switch (t)
            {
                case HArrowType.Plus:
                    StrokeLine(g, lk, pw, -s, 0, s, 0, false, 0, L, c);
                    StrokeLine(g, lk, pw, 0, -s, 0, s, false, 0, L, c);
                    break;
                case HArrowType.Minus:
                    StrokeLine(g, lk, pw, -s, 0, s, 0, false, 0, L, c);
                    break;
                case HArrowType.Multiply:
                    StrokeLine(g, lk, pw, -s, -s, s, s, false, 0, L, c);
                    StrokeLine(g, lk, pw, s, -s, -s, s, false, 0, L, c);
                    break;
                case HArrowType.Divide:
                    StrokeLine(g, lk, pw, -s, 0, s, 0, false, 0, L, c);
                    Dot(g, c, pw, 0, -s);
                    Dot(g, c, pw, 0, s);
                    break;
                case HArrowType.PlusMinus:
                    StrokeLine(g, lk, pw, -s, -s * 0.55f, s, -s * 0.55f, false, 0, L, c);
                    StrokeLine(g, lk, pw, 0, -s * 1.35f, 0, s * 0.25f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s, s * 0.75f, s, s * 0.75f, false, 0, L, c);
                    break;
                case HArrowType.MinusPlus:
                    StrokeLine(g, lk, pw, -s, -s * 0.75f, s, -s * 0.75f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s, s * 0.55f, s, s * 0.55f, false, 0, L, c);
                    StrokeLine(g, lk, pw, 0, -s * 0.25f, 0, s * 1.35f, false, 0, L, c);
                    break;
                case HArrowType.Equal:
                    StrokeLine(g, lk, pw, -s, -s * 0.45f, s, -s * 0.45f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s, s * 0.45f, s, s * 0.45f, false, 0, L, c);
                    break;
                case HArrowType.NotEqual:
                    StrokeLine(g, lk, pw, -s, -s * 0.45f, s, -s * 0.45f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s, s * 0.45f, s, s * 0.45f, false, 0, L, c);
                    StrokeLine(g, lk, pw * 0.9f, -s * 0.9f, s * 1.05f, s * 0.9f, -s * 1.05f, false, 0, L, c);
                    break;
                case HArrowType.Approximately:
                    DrawWavyStroke(g, lk, pw * 0.85f, -s, -s * 0.4f, s, c, L);
                    DrawWavyStroke(g, lk, pw * 0.85f, -s, s * 0.4f, s, c, L);
                    break;
                case HArrowType.Percent:
                {
                    // 斜杠 + 右上/左下两个小圆环；圆环半径固定为 L 的比例，
                    // 不能随笔宽走，否则两个圆会重叠成 8 字形
                    float rr = L * 0.075f;
                    float ox = s * 0.95f, oy = s * 0.95f;
                    StrokeLine(g, lk, pw * 0.9f, -ox, oy, ox, -oy, false, 0, L, c);
                    Ring(g, c, pw * 0.85f, ox, -oy, rr);
                    Ring(g, c, pw * 0.85f, -ox, oy, rr);
                }
                    break;
                case HArrowType.LessThan:
                    StrokeLine(g, lk, pw, s * 0.6f, -s, -s * 0.6f, 0, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s * 0.6f, 0, s * 0.6f, s, false, 0, L, c);
                    break;
                case HArrowType.GreaterThan:
                    StrokeLine(g, lk, pw, -s * 0.6f, -s, s * 0.6f, 0, false, 0, L, c);
                    StrokeLine(g, lk, pw, s * 0.6f, 0, -s * 0.6f, s, false, 0, L, c);
                    break;
                case HArrowType.LessEqual:
                    StrokeLine(g, lk, pw, s * 0.6f, -s * 1.1f, -s * 0.6f, -s * 0.1f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s * 0.6f, -s * 0.1f, s * 0.6f, s * 0.9f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s * 0.6f, s * 1.2f, s * 0.6f, s * 1.2f, false, 0, L, c);
                    break;
                case HArrowType.GreaterEqual:
                    StrokeLine(g, lk, pw, -s * 0.6f, -s * 1.1f, s * 0.6f, -s * 0.1f, false, 0, L, c);
                    StrokeLine(g, lk, pw, s * 0.6f, -s * 0.1f, -s * 0.6f, s * 0.9f, false, 0, L, c);
                    StrokeLine(g, lk, pw, -s * 0.6f, s * 1.2f, s * 0.6f, s * 1.2f, false, 0, L, c);
                    break;
            }
        }
        private static void Dot(Graphics g, Color c, float pw, float x, float y)
        {
            float d = pw * 1.5f;
            using (var b = new SolidBrush(c))
                g.FillEllipse(b, x - d / 2f, y - d / 2f, d, d);
        }
        private static void Ring(Graphics g, Color c, float pw, float x, float y, float r)
        {
            using (var p = new Pen(c, Math.Max(1f, pw)))
                g.DrawEllipse(p, x - r, y - r, r * 2f, r * 2f);
        }
        private static void DrawWavyStroke(Graphics g, Look lk, float pw, float x0, float y, float x1, Color c, float L)
        {
            float amp = L * 0.035f, seg = (x1 - x0) / 2f;
            using (var path = new GraphicsPath())
            using (var p = new Pen(c, Math.Max(1f, pw)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                path.AddBezier(x0, y, x0 + seg * 0.3f, y - amp, x0 + seg * 0.7f, y - amp, x0 + seg, y);
                path.AddBezier(x0 + seg, y, x0 + seg * 1.3f, y + amp, x1 - seg * 0.3f, y + amp, x1, y);
                g.DrawPath(p, path);
            }
        }
        #endregion
        #region 风格叠加
        private static void DrawCometTrail(Graphics g, float L, Color c, float phase)
        {
            float H = L / 2f;
            for (int i = 0; i < 3; i++)
            {
                float off = H * (0.55f + 0.22f * i) + (float)Math.Sin(phase) * H * 0.06f;
                float s = H * (0.16f - i * 0.03f);
                int a = 120 - i * 35;
                Color cc = Alpha(c, a);
                using (var p = new Pen(cc, Math.Max(1f, L * 0.05f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(p, -off, 0, -off - s * 0.6f, -s);
                    g.DrawLine(p, -off, 0, -off - s * 0.6f, s);
                }
            }
        }
        private static void DrawTechTicks(Graphics g, float L, Color c)
        {
            float H = L / 2f, s = L * 0.05f;
            using (var b = new SolidBrush(c))
            {
                for (int i = -1; i <= 1; i++)
                {
                    float x = i * H * 0.4f;
                    g.FillRectangle(b, x - s / 2f, -s / 2f, s, s);
                }
            }
        }
        private static void DrawGloss(Graphics g, float L, Color c)
        {
            float H = L / 2f;
            using (var b = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                g.FillEllipse(b, H * 0.18f, -H * 0.34f, H * 0.22f, H * 0.13f);
        }
        #endregion
    }
}