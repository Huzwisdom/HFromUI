using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Indicator
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 工业警示牌图号：22 种警告标志图形（参照 GB 2894 / ISO 7010 W 系列）——
    /// 当心触电、机械伤人、吊物、坠落、叉车、弧光、噪声、烫伤、腐蚀、滑跌、
    /// 挤压、电离辐射、激光、爆炸、中毒、高压、自动启动、低温、行车、坑洞、粉尘、台阶。
    /// </summary>
    public enum HWarningSignStyle
    {
        ElectricShock = 0,  // 当心触电
        Machinery = 1,      // 当心机械伤人
        FallingLoad = 2,    // 当心吊物
        FallingPerson = 3,  // 当心坠落
        Forklift = 4,       // 当心车辆（叉车）
        ArcFlash = 5,       // 当心弧光
        Noise = 6,          // 当心噪声
        HotSurface = 7,     // 当心烫伤
        Corrosion = 8,      // 当心腐蚀
        Slippery = 9,       // 当心滑跌
        Crushing = 10,      // 当心挤压
        Radiation = 11,     // 当心电离辐射
        Laser = 12,         // 当心激光
        Explosion = 13,     // 当心爆炸
        Poison = 14,        // 当心中毒
        HighVoltage = 15,   // 高压危险
        AutoStart = 16,     // 当心自动启动
        LowTemperature = 17,// 当心低温
        OverheadCrane = 18, // 当心行车
        Trench = 19,        // 当心坑洞
        Dust = 20,          // 当心粉尘
        WatchStep = 21      // 注意台阶
    }
    /// <summary>
    /// 工业警示牌控件（继承 HToolAnimBase）：22 种警告图形，等边三角黄底黑边范式；
    /// Text 作为牌面辅助文字随统一方位系统绘制（默认在牌下方横排）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("工业警示牌：22 种当心警告图形，黄底黑边三角牌")]
    public class HWarningSign : HToolAnimBase
    {
        private HWarningSignStyle _style = HWarningSignStyle.ElectricShock;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Amber);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CYellow = Color.FromArgb(248, 208, 30);
        private static readonly Color CYellowLt = Color.FromArgb(255, 226, 80);
        private static readonly Color CBlack = Color.FromArgb(24, 24, 24);
        /// <summary>警示牌图号。</summary>
        [HCategoryLanguage("警示牌"), HDisplayNameLanguage("警示牌警告图形样式"), HDescriptionLanguage("警示牌警告图形样式"), Browsable(true)]
        [DefaultValue(HWarningSignStyle.ElectricShock)]
        public HWarningSignStyle SignStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var a = PlacedGlyphArea();
            if (a.Width <= 4f || a.Height <= 4f) { PaintPlacedText(g); return; }
            // 等边三角形（顶点在上），按绘图区居中
            float s = Math.Min(a.Width, a.Height * 1.08f);
            if (s < 6f) s = 6f;
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + (a.Height - s * 0.92f) / 2f;
            var tri = new[]
            {
                new PointF(cx, top),
                new PointF(cx - s / 2f, top + s * 0.866f),
                new PointF(cx + s / 2f, top + s * 0.866f)
            };
            // 阴影
            var shadow = new[]
            {
                new PointF(tri[0].X + s * 0.015f, tri[0].Y + s * 0.03f),
                new PointF(tri[1].X + s * 0.015f, tri[1].Y + s * 0.03f),
                new PointF(tri[2].X + s * 0.015f, tri[2].Y + s * 0.03f)
            };
            using (var shb = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                g.FillPolygon(shb, shadow);
            // 黑边 + 黄底
            using (var bb = new SolidBrush(CBlack))
                g.FillPolygon(bb, tri);
            var inner = new[]
            {
                new PointF(cx, top + s * 0.085f),
                new PointF(cx - s * 0.415f, top + s * 0.805f),
                new PointF(cx + s * 0.415f, top + s * 0.805f)
            };
            using (var yb = new LinearGradientBrush(
                new RectangleF(cx - s / 2f, top, s, s * 0.866f), CYellowLt, CYellow, 90f))
                g.FillPolygon(yb, inner);
            // 牌内图形区（三角形内的有效矩形）
            var symbol = new RectangleF(cx - s * 0.245f, top + s * 0.30f, s * 0.49f, s * 0.42f);
            DrawSymbol(g, symbol, _style);
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // 22 种黑色象形图形
        // ----------------------------------------------------------------
        private void DrawSymbol(Graphics g, RectangleF r, HWarningSignStyle style)
        {
            if (r.Width < 2f || r.Height < 2f) return;
            using (var b = new SolidBrush(CBlack))
            using (var p = new Pen(CBlack, Math.Max(1.2f, r.Width * 0.05f))
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                p.LineJoin = LineJoin.Round;
                float w = r.Width, h = r.Height;
                switch (style)
                {
                    case HWarningSignStyle.ElectricShock:
                    case HWarningSignStyle.HighVoltage:
                        DrawBolt(g, r, b, style == HWarningSignStyle.HighVoltage);
                        break;
                    case HWarningSignStyle.Machinery:
                        DrawGear(g, b, p, r, 8);
                        // 夹点箭头
                        g.DrawLine(p, r.Left - w * 0.06f, r.Top + h * 0.12f, r.Left - w * 0.06f, r.Bottom - h * 0.12f);
                        g.DrawLine(p, r.Right + w * 0.06f, r.Top + h * 0.12f, r.Right + w * 0.06f, r.Bottom - h * 0.12f);
                        break;
                    case HWarningSignStyle.FallingLoad:
                        // 吊钩
                        g.DrawLine(p, r.Left + w * 0.3f, r.Top, r.Left + w * 0.3f, r.Top + h * 0.34f);
                        g.DrawArc(p, r.Left + w * 0.3f, r.Top + h * 0.3f, w * 0.26f, h * 0.26f, 0f, 250f);
                        // 落物
                        g.FillRectangle(b, r.Left + w * 0.52f, r.Top + h * 0.14f, w * 0.34f, h * 0.26f);
                        // 下箭头
                        g.DrawLine(p, r.Left + w * 0.69f, r.Top + h * 0.46f, r.Left + w * 0.69f, r.Bottom - h * 0.1f);
                        g.DrawLine(p, r.Left + w * 0.56f, r.Bottom - h * 0.24f, r.Left + w * 0.69f, r.Bottom - h * 0.1f);
                        g.DrawLine(p, r.Left + w * 0.82f, r.Bottom - h * 0.24f, r.Left + w * 0.69f, r.Bottom - h * 0.1f);
                        break;
                    case HWarningSignStyle.FallingPerson:
                        DrawPerson(g, b, p, r, true);
                        break;
                    case HWarningSignStyle.Forklift:
                        DrawForklift(g, b, p, r);
                        break;
                    case HWarningSignStyle.ArcFlash:
                        // 人脸侧影
                        g.FillEllipse(b, r.Left + w * 0.08f, r.Top + h * 0.16f, w * 0.26f, h * 0.3f);
                        g.FillRectangle(b, r.Left + w * 0.1f, r.Top + h * 0.4f, w * 0.2f, h * 0.3f);
                        // 弧光射线
                        for (int i = 0; i < 5; i++)
                        {
                            double ang = -Math.PI / 3 + i * Math.PI / 6;
                            float x1 = r.Left + w * 0.46f, y1 = r.Top + h * 0.34f;
                            g.DrawLine(p, x1, y1,
                                x1 + (float)Math.Cos(ang) * w * 0.4f,
                                y1 + (float)Math.Sin(ang) * h * 0.4f);
                        }
                        break;
                    case HWarningSignStyle.Noise:
                        // 喇叭 + 声波
                        g.FillRectangle(b, r.Left + w * 0.1f, r.Top + h * 0.4f, w * 0.16f, h * 0.2f);
                        g.FillPolygon(b, new[]
                        {
                            new PointF(r.Left + w * 0.24f, r.Top + h * 0.32f),
                            new PointF(r.Left + w * 0.46f, r.Top + h * 0.22f),
                            new PointF(r.Left + w * 0.46f, r.Top + h * 0.78f),
                            new PointF(r.Left + w * 0.24f, r.Top + h * 0.68f)
                        });
                        g.DrawArc(p, r.Left + w * 0.5f, r.Top + h * 0.32f, w * 0.22f, h * 0.36f, -60f, 120f);
                        g.DrawArc(p, r.Left + w * 0.58f, r.Top + h * 0.24f, w * 0.34f, h * 0.52f, -60f, 120f);
                        break;
                    case HWarningSignStyle.HotSurface:
                        for (int i = 0; i < 3; i++)
                            g.DrawArc(p, r.Left + w * (0.16f + i * 0.28f), r.Top + h * 0.08f,
                                w * 0.2f, h * 0.5f, 200f, 140f);
                        g.FillRectangle(b, r.Left + w * 0.1f, r.Top + h * 0.62f, w * 0.8f, h * 0.2f);
                        break;
                    case HWarningSignStyle.Corrosion:
                        // 试管
                        g.DrawLine(p, r.Left + w * 0.28f, r.Top, r.Left + w * 0.28f, r.Top + h * 0.5f);
                        g.DrawLine(p, r.Left + w * 0.46f, r.Top, r.Left + w * 0.46f, r.Top + h * 0.5f);
                        g.DrawArc(p, r.Left + w * 0.28f, r.Top + h * 0.32f, w * 0.18f, h * 0.3f, 0f, 180f);
                        // 液滴
                        g.FillEllipse(b, r.Left + w * 0.34f, r.Top + h * 0.56f, w * 0.08f, h * 0.12f);
                        // 腐蚀面（手形横条 + 蚀坑）
                        g.FillRectangle(b, r.Left + w * 0.12f, r.Bottom - h * 0.14f, w * 0.76f, h * 0.1f);
                        g.FillEllipse(b, r.Left + w * 0.56f, r.Bottom - h * 0.12f, w * 0.12f, h * 0.08f);
                        break;
                    case HWarningSignStyle.Slippery:
                        // 滑倒人形
                        g.FillEllipse(b, r.Left + w * 0.12f, r.Top + h * 0.12f, w * 0.18f, h * 0.2f);
                        g.DrawLine(p, r.Left + w * 0.26f, r.Top + h * 0.34f,
                            r.Left + w * 0.6f, r.Top + h * 0.54f);
                        g.DrawLine(p, r.Left + w * 0.6f, r.Top + h * 0.54f,
                            r.Right - w * 0.06f, r.Top + h * 0.78f);
                        g.DrawLine(p, r.Left + w * 0.4f, r.Top + h * 0.42f,
                            r.Left + w * 0.66f, r.Top + h * 0.3f);
                        // 滑动弧线
                        g.DrawArc(p, r.Left + w * 0.1f, r.Bottom - h * 0.26f, w * 0.8f, h * 0.3f, 200f, 140f);
                        break;
                    case HWarningSignStyle.Crushing:
                        // 两夹板相向
                        g.FillRectangle(b, r.Left, r.Top, w * 0.16f, h);
                        g.FillRectangle(b, r.Right - w * 0.16f, r.Top, w * 0.16f, h);
                        // 相向箭头
                        g.DrawLine(p, r.Left + w * 0.24f, r.Top + h * 0.5f, r.Left + w * 0.42f, r.Top + h * 0.5f);
                        g.DrawLine(p, r.Left + w * 0.3f, r.Top + h * 0.4f, r.Left + w * 0.42f, r.Top + h * 0.5f);
                        g.DrawLine(p, r.Left + w * 0.3f, r.Top + h * 0.6f, r.Left + w * 0.42f, r.Top + h * 0.5f);
                        g.DrawLine(p, r.Right - w * 0.24f, r.Top + h * 0.5f, r.Right - w * 0.42f, r.Top + h * 0.5f);
                        g.DrawLine(p, r.Right - w * 0.3f, r.Top + h * 0.4f, r.Right - w * 0.42f, r.Top + h * 0.5f);
                        g.DrawLine(p, r.Right - w * 0.3f, r.Top + h * 0.6f, r.Right - w * 0.42f, r.Top + h * 0.5f);
                        break;
                    case HWarningSignStyle.Radiation:
                        DrawRadiation(g, b, r);
                        break;
                    case HWarningSignStyle.Laser:
                        // 眼睛
                        g.DrawArc(p, r.Left + w * 0.1f, r.Top + h * 0.28f, w * 0.8f, h * 0.44f, 200f, 140f);
                        g.FillEllipse(b, r.Left + w * 0.42f, r.Top + h * 0.38f, w * 0.16f, h * 0.24f);
                        // 光束
                        g.DrawLine(p, r.Left + w * 0.5f, r.Top + h * 0.5f, r.Left - w * 0.04f, r.Top);
                        g.DrawLine(p, r.Left + w * 0.5f, r.Top + h * 0.5f, r.Left - w * 0.04f, r.Bottom);
                        break;
                    case HWarningSignStyle.Explosion:
                        DrawBurst(g, b, r, 12);
                        break;
                    case HWarningSignStyle.Poison:
                        DrawSkull(g, b, p, r);
                        break;
                    case HWarningSignStyle.AutoStart:
                        DrawGear(g, b, p, new RectangleF(r.Left + w * 0.28f, r.Top + h * 0.24f,
                            w * 0.44f, h * 0.44f), 8);
                        // 循环弧箭头
                        g.DrawArc(p, r.Left + w * 0.12f, r.Top + h * 0.12f, w * 0.76f, h * 0.76f, 30f, 250f);
                        g.DrawLine(p, r.Left + w * 0.16f, r.Top + h * 0.06f,
                            r.Left + w * 0.26f, r.Top + h * 0.2f);
                        break;
                    case HWarningSignStyle.LowTemperature:
                        DrawSnowflake(g, p, r);
                        break;
                    case HWarningSignStyle.OverheadCrane:
                        // 吊车梁 + 吊钩
                        g.DrawLine(p, r.Left, r.Top + h * 0.14f, r.Right, r.Top + h * 0.14f);
                        g.DrawLine(p, r.Left + w * 0.1f, r.Top + h * 0.14f, r.Left + w * 0.1f, r.Top + h * 0.26f);
                        g.DrawLine(p, r.Right - w * 0.1f, r.Top + h * 0.14f, r.Right - w * 0.1f, r.Top + h * 0.26f);
                        g.FillRectangle(b, r.Left + w * 0.36f, r.Top + h * 0.14f, w * 0.28f, h * 0.12f);
                        g.DrawLine(p, r.Left + w * 0.5f, r.Top + h * 0.26f, r.Left + w * 0.5f, r.Top + h * 0.56f);
                        g.DrawArc(p, r.Left + w * 0.42f, r.Top + h * 0.52f, w * 0.18f, h * 0.22f, 0f, 250f);
                        break;
                    case HWarningSignStyle.Trench:
                        // 地面线 + 坑口
                        g.DrawLine(p, r.Left, r.Bottom - h * 0.3f, r.Left + w * 0.3f, r.Bottom - h * 0.3f);
                        g.DrawLine(p, r.Right - w * 0.24f, r.Bottom - h * 0.3f, r.Right, r.Bottom - h * 0.3f);
                        g.DrawLine(p, r.Left + w * 0.3f, r.Bottom - h * 0.3f,
                            r.Left + w * 0.36f, r.Bottom - h * 0.06f);
                        g.DrawLine(p, r.Left + w * 0.36f, r.Bottom - h * 0.06f,
                            r.Right - w * 0.3f, r.Bottom - h * 0.06f);
                        g.DrawLine(p, r.Right - w * 0.3f, r.Bottom - h * 0.06f,
                            r.Right - w * 0.24f, r.Bottom - h * 0.3f);
                        // 坠落人形
                        g.FillEllipse(b, r.Left + w * 0.42f, r.Top + h * 0.44f, w * 0.14f, h * 0.16f);
                        g.DrawLine(p, r.Left + w * 0.48f, r.Top + h * 0.58f,
                            r.Left + w * 0.42f, r.Bottom - h * 0.18f);
                        break;
                    case HWarningSignStyle.Dust:
                        // 尘云
                        g.FillEllipse(b, r.Left + w * 0.14f, r.Top + h * 0.4f, w * 0.3f, h * 0.3f);
                        g.FillEllipse(b, r.Left + w * 0.34f, r.Top + h * 0.28f, w * 0.34f, h * 0.36f);
                        g.FillEllipse(b, r.Left + w * 0.56f, r.Top + h * 0.44f, w * 0.28f, h * 0.28f);
                        g.FillRectangle(b, r.Left + w * 0.2f, r.Top + h * 0.56f, w * 0.6f, h * 0.18f);
                        // 飞尘点
                        g.FillEllipse(b, r.Left + w * 0.06f, r.Top + h * 0.18f, w * 0.07f, h * 0.08f);
                        g.FillEllipse(b, r.Right - w * 0.14f, r.Top + h * 0.16f, w * 0.06f, h * 0.07f);
                        g.FillEllipse(b, r.Left + w * 0.7f, r.Top + h * 0.06f, w * 0.05f, h * 0.06f);
                        break;
                    case HWarningSignStyle.WatchStep:
                        // 落差台阶
                        g.DrawLine(p, r.Left, r.Top + h * 0.4f, r.Left + w * 0.5f, r.Top + h * 0.4f);
                        g.DrawLine(p, r.Left + w * 0.5f, r.Top + h * 0.4f,
                            r.Left + w * 0.5f, r.Bottom - h * 0.1f);
                        g.DrawLine(p, r.Left + w * 0.5f, r.Bottom - h * 0.1f, r.Right, r.Bottom - h * 0.1f);
                        // 下台阶叹号人形（简化为脚 + 箭头）
                        g.DrawLine(p, r.Left + w * 0.62f, r.Top + h * 0.5f,
                            r.Left + w * 0.62f, r.Bottom - h * 0.3f);
                        g.FillEllipse(b, r.Left + w * 0.57f, r.Bottom - h * 0.3f, w * 0.1f, h * 0.1f);
                        break;
                }
            }
        }
        // 闪电（hv=true 时加向下箭头强调高压）
        private static void DrawBolt(Graphics g, RectangleF r, Brush b, bool hv)
        {
            float w = r.Width, h = r.Height;
            float top = hv ? r.Top : r.Top;
            PointF[] bolt =
            {
                new PointF(r.Left + w * 0.56f, top),
                new PointF(r.Left + w * 0.26f, top + h * 0.52f),
                new PointF(r.Left + w * 0.47f, top + h * 0.52f),
                new PointF(r.Left + w * 0.38f, top + h),
                new PointF(r.Left + w * 0.74f, top + h * 0.42f),
                new PointF(r.Left + w * 0.52f, top + h * 0.42f)
            };
            g.FillPolygon(b, bolt);
            if (hv)
            {
                using (var p = new Pen(Color.FromArgb(24, 24, 24), Math.Max(1f, w * 0.04f))
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawLine(p, r.Left + w * 0.86f, top, r.Left + w * 0.86f, top + h * 0.9f);
            }
        }
        // 齿轮
        private static void DrawGear(Graphics g, Brush b, Pen p, RectangleF r, int teeth)
        {
            float cx = r.Left + r.Width / 2f, cy = r.Top + r.Height / 2f;
            float ro = Math.Min(r.Width, r.Height) * 0.5f;
            float ri = ro * 0.72f;
            var pts = new PointF[teeth * 2];
            for (int i = 0; i < teeth * 2; i++)
            {
                double ang = -Math.PI / 2 + i * Math.PI / teeth;
                float rad = i % 2 == 0 ? ro : ri;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * rad, cy + (float)Math.Sin(ang) * rad);
            }
            g.FillPolygon(b, pts);
            float hr = ro * 0.3f;
            using (var hole = new SolidBrush(CYellow))
                g.FillEllipse(hole, cx - hr, cy - hr, hr * 2, hr * 2);
        }
        // 坠落人形（平台 + 翻倒人）
        private static void DrawPerson(Graphics g, Brush b, Pen p, RectangleF r, bool falling)
        {
            float w = r.Width, h = r.Height;
            if (falling)
            {
                // 平台
                g.DrawLine(p, r.Left, r.Bottom - h * 0.12f, r.Left + w * 0.42f, r.Bottom - h * 0.12f);
                // 翻倒人形
                g.FillEllipse(b, r.Right - w * 0.34f, r.Top + h * 0.12f, w * 0.2f, h * 0.22f);
                g.DrawLine(p, r.Right - w * 0.24f, r.Top + h * 0.34f,
                    r.Left + w * 0.42f, r.Top + h * 0.62f);
                g.DrawLine(p, r.Left + w * 0.42f, r.Top + h * 0.62f,
                    r.Left + w * 0.3f, r.Bottom - h * 0.24f);
                g.DrawLine(p, r.Left + w * 0.42f, r.Top + h * 0.62f,
                    r.Left + w * 0.56f, r.Bottom - h * 0.2f);
                g.DrawLine(p, r.Right - w * 0.3f, r.Top + h * 0.42f,
                    r.Right - w * 0.08f, r.Top + h * 0.28f);
            }
        }
        // 叉车侧影
        private static void DrawForklift(Graphics g, Brush b, Pen p, RectangleF r)
        {
            float w = r.Width, h = r.Height;
            // 车身
            g.FillRectangle(b, r.Left + w * 0.28f, r.Top + h * 0.42f, w * 0.4f, h * 0.26f);
            g.FillRectangle(b, r.Left + w * 0.28f, r.Top + h * 0.22f, w * 0.24f, h * 0.22f);
            // 门架
            g.DrawLine(p, r.Right - w * 0.14f, r.Top + h * 0.1f, r.Right - w * 0.14f, r.Top + h * 0.66f);
            // 货叉
            g.DrawLine(p, r.Right - w * 0.14f, r.Top + h * 0.62f, r.Right + w * 0.06f, r.Top + h * 0.62f);
            // 轮
            g.FillEllipse(b, r.Left + w * 0.3f, r.Bottom - h * 0.28f, w * 0.2f, h * 0.24f);
            g.FillEllipse(b, r.Left + w * 0.56f, r.Bottom - h * 0.28f, w * 0.2f, h * 0.24f);
        }
        // 电离辐射三叶标志
        private static void DrawRadiation(Graphics g, Brush b, RectangleF r)
        {
            float cx = r.Left + r.Width / 2f, cy = r.Top + r.Height * 0.56f;
            float ro = Math.Min(r.Width, r.Height) * 0.46f;
            var path = new GraphicsPath();
            for (int i = 0; i < 3; i++)
            {
                double a0 = -Math.PI / 2 + i * Math.PI * 2 / 3 + 0.28;
                double a1 = -Math.PI / 2 + i * Math.PI * 2 / 3 - 0.28;
                var pts = new[]
                {
                    new PointF(cx + (float)Math.Cos(a0) * ro * 0.32f, cy + (float)Math.Sin(a0) * ro * 0.32f),
                    new PointF(cx + (float)Math.Cos(a0) * ro, cy + (float)Math.Sin(a0) * ro),
                    new PointF(cx + (float)Math.Cos(a1) * ro, cy + (float)Math.Sin(a1) * ro),
                    new PointF(cx + (float)Math.Cos(a1) * ro * 0.32f, cy + (float)Math.Sin(a1) * ro * 0.32f)
                };
                path.StartFigure();
                path.AddLines(pts);
                path.CloseFigure();
            }
            g.FillPath(b, path);
            g.FillEllipse(b, cx - ro * 0.2f, cy - ro * 0.2f, ro * 0.4f, ro * 0.4f);
            path.Dispose();
        }
        // 爆炸星
        private static void DrawBurst(Graphics g, Brush b, RectangleF r, int points)
        {
            float cx = r.Left + r.Width / 2f, cy = r.Top + r.Height / 2f;
            float ro = Math.Min(r.Width, r.Height) * 0.48f, ri = ro * 0.62f;
            var pts = new PointF[points * 2];
            for (int i = 0; i < points * 2; i++)
            {
                double ang = -Math.PI / 2 + i * Math.PI / points;
                float rad = i % 2 == 0 ? ro : ri;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * rad, cy + (float)Math.Sin(ang) * rad);
            }
            g.FillPolygon(b, pts);
        }
        // 骷髅
        private static void DrawSkull(Graphics g, Brush b, Pen p, RectangleF r)
        {
            float w = r.Width, h = r.Height;
            g.FillEllipse(b, r.Left + w * 0.22f, r.Top, w * 0.56f, h * 0.56f);
            g.FillRectangle(b, r.Left + w * 0.32f, r.Top + h * 0.48f, w * 0.36f, h * 0.18f);
            // 眼洞（用黄色挖空）
            g.FillEllipse(new SolidBrush(CYellow), r.Left + w * 0.3f, r.Top + h * 0.22f, w * 0.14f, h * 0.16f);
            g.FillEllipse(new SolidBrush(CYellow), r.Left + w * 0.56f, r.Top + h * 0.22f, w * 0.14f, h * 0.16f);
            g.FillPolygon(new SolidBrush(CYellow), new[]
            {
                new PointF(r.Left + w * 0.46f, r.Top + h * 0.4f),
                new PointF(r.Left + w * 0.54f, r.Top + h * 0.4f),
                new PointF(r.Left + w * 0.5f, r.Top + h * 0.5f)
            });
            // 交叉骨
            g.DrawLine(p, r.Left + w * 0.16f, r.Bottom, r.Right - w * 0.16f, r.Top + h * 0.62f);
            g.DrawLine(p, r.Right - w * 0.16f, r.Bottom, r.Left + w * 0.16f, r.Top + h * 0.62f);
        }
        // 雪花
        private static void DrawSnowflake(Graphics g, Pen p, RectangleF r)
        {
            float cx = r.Left + r.Width / 2f, cy = r.Top + r.Height / 2f;
            float rad = Math.Min(r.Width, r.Height) * 0.46f;
            for (int i = 0; i < 6; i++)
            {
                double ang = -Math.PI / 2 + i * Math.PI / 3;
                float x = cx + (float)Math.Cos(ang) * rad;
                float y = cy + (float)Math.Sin(ang) * rad;
                g.DrawLine(p, cx, cy, x, y);
                // 分支
                for (int k = 1; k <= 2; k++)
                {
                    float t = rad * (0.32f + k * 0.22f);
                    float bx = cx + (float)Math.Cos(ang) * t;
                    float by = cy + (float)Math.Sin(ang) * t;
                    for (int s = -1; s <= 1; s += 2)
                    {
                        double ba = ang + s * Math.PI * 0.72;
                        g.DrawLine(p, bx, by,
                            bx + (float)Math.Cos(ba) * rad * 0.16f,
                            by + (float)Math.Sin(ba) * rad * 0.16f);
                    }
                }
            }
        }
    }
}