using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Chart.Menu
{
    /// <summary>
    /// 扩展图标引擎·语义手绘族 A：天气氛围 24（386..409）+ 数学符号 30（410..439）。
    /// 全部沿用墨色 k 描线、强调色 a 实底，保证与整套菜单图标同族配色。
    /// </summary>
    internal static partial class HVec
    {
        /// <summary>386..539 语义族总入口（编号分段见枚举注释）。</summary>
        private static void PaintRest(int v, Graphics g, Color k, Color a)
        {
            if (v < 410) Weather(g, k, a, v - 386);
            else if (v < 440) MathSym(g, k, a, v - 410);
            else if (v < 476) ChartEx(g, k, a, v - 440);
            else if (v < 500) UiSym(g, k, a, v - 476);
            else if (v < 516) Login(g, k, a, v - 500);
            else Misc(g, k, a, v - 516);
        }

        // ============ 天气氛围族 24 ============
        private static void Weather(Graphics g, Color k, Color a, int q)
        {
            switch (q)
            {
                case 0: // 晴
                    SunRays(g, k, 8f, 8f, 2.3f);
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 5.4f, 5.4f, 5.2f, 5.2f);
                    break;
                case 1: // 晴间多云（阳+云）
                    SunRays(g, k, 5.5f, 5f, 1.6f);
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 3.6f, 3.4f, 3.8f, 3.8f);
                    CloudShape(g, k, 2.5f, 7.5f, 11f, 5.5f, true);
                    break;
                case 2: CloudShape(g, k, 2.2f, 4.5f, 11.6f, 8f, false); break;
                case 3: CloudShape(g, k, 2.2f, 2f, 11.6f, 8f, false); Rain(g, a, 3, false); break;
                case 4: CloudShape(g, k, 2.2f, 1.5f, 11.6f, 8f, false); Rain(g, a, 5, false); break;
                case 5: CloudShape(g, k, 2.2f, 2.5f, 11.6f, 7.5f, false); Rain(g, a, 3, true); break;
                case 6: CloudShape(g, k, 2.2f, 2f, 11.6f, 8f, false); SnowDots(g, a, 4); break;
                case 7:
                    CloudShape(g, k, 2.2f, 1.5f, 11.6f, 7.5f, false);
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(8.2f, 9f), new PointF(6.6f, 12f), new PointF(8f, 12f), new PointF(6.8f, 14.5f), new PointF(10f, 10.8f), new PointF(8.6f, 10.8f) });
                    break;
                case 8: // 雾
                    CloudShape(g, k, 2.2f, 1.5f, 9f, 6f, false);
                    using (var p = Pen(k, 1.3f))
                        for (int i = 0; i < 3; i++) g.DrawLine(p, 2.5f, 9f + i * 2.1f, 13.5f - i * 1.5f, 9f + i * 2.1f);
                    break;
                case 9: // 风
                    using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawArc(p, 1.5f, 2.5f, 7f, 5f, 200, 150);
                        g.DrawArc(p, 4f, 6f, 9f, 5f, 170, 170);
                        g.DrawLine(p, 2.5f, 12.5f, 13f, 12.5f);
                    }
                    break;
                case 10: // 伞
                    using (var p = new Pen(k, 1.4f))
                    {
                        g.DrawArc(p, 2.5f, 1.8f, 11f, 11f, 180, 180);
                        g.DrawLine(p, 8f, 2.4f, 8f, 12.6f);
                        g.DrawArc(p, 8f, 10.6f, 2.4f, 3f, 20, 130);
                        g.DrawLine(p, 4.3f, 4f, 8f, 2.4f); g.DrawLine(p, 11.7f, 4f, 8f, 2.4f);
                    }
                    using (var br = new SolidBrush(Color.FromArgb(60, a.R, a.G, a.B))) g.FillPie(br, 2.5f, 1.8f, 11f, 11f, 180, 180);
                    break;
                case 11: // 月夜星空
                    using (var path = new GraphicsPath(FillMode.Alternate))
                    { path.AddEllipse(2.5f, 1.5f, 10f, 12f); path.AddEllipse(5f, 0.8f, 10f, 12f);
                        using (var br = new SolidBrush(a)) g.FillPath(br, path); }
                    using (var br = new SolidBrush(a))
                    {
                        g.FillPolygon(br, Star(4, 1.1f, 0.45f, -90f, 12.6f, 3.2f));
                        g.FillPolygon(br, Star(4, 0.9f, 0.35f, -90f, 13.2f, 7f));
                    }
                    break;
                case 12: // 白天多云
                    SunRays(g, k, 10.5f, 4.6f, 1.5f);
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 8.6f, 3f, 3.6f, 3.6f);
                    CloudShape(g, k, 1.6f, 7f, 12f, 6.5f, true);
                    break;
                case 13: // 夜间多云
                    using (var path = new GraphicsPath(FillMode.Alternate))
                    { path.AddEllipse(8f, 2.2f, 6.5f, 7.5f); path.AddEllipse(9.6f, 1.7f, 6.5f, 7.5f);
                        using (var br = new SolidBrush(a)) g.FillPath(br, path); }
                    CloudShape(g, k, 1.4f, 8f, 11.4f, 6f, true);
                    break;
                case 14: // 彩虹
                    using (var p1 = new Pen(a, 1.5f)) g.DrawArc(p1, 1.5f, 1.5f, 13f, 13f, 180, 180);
                    using (var p2 = new Pen(k, 1.3f)) { g.DrawArc(p2, 3.3f, 3.3f, 9.4f, 9.4f, 180, 180); g.DrawLine(p2, 1.5f, 8f, 14.5f, 8f); }
                    break;
                case 15: // 龙卷风
                    using (var p = new Pen(k, 1.4f))
                    {
                        g.DrawLines(p, new[] { new PointF(3, 2.5f), new PointF(13, 2.5f), new PointF(10.6f, 5.5f), new PointF(5.4f, 5.5f) });
                        g.DrawLines(p, new[] { new PointF(6.2f, 5.5f), new PointF(9.8f, 5.5f), new PointF(8.8f, 9f), new PointF(7.2f, 9f) });
                        g.DrawLines(p, new[] { new PointF(7.6f, 9f), new PointF(8.4f, 9f), new PointF(9f, 12.5f), new PointF(7f, 12.5f) });
                    }
                    break;
                case 16: Thermo(g, k, a, true); break;
                case 17: Thermo(g, k, a, false); break;
                case 18: // 单水滴
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(8f, 1.8f), new PointF(12.2f, 9f), new PointF(3.8f, 9f) });
                    using (var p = new Pen(k, 1.1f)) g.DrawPolygon(p, new[] { new PointF(8f, 1.8f), new PointF(12.2f, 9f), new PointF(3.8f, 9f) });
                    break;
                case 19:
                    Droplet(g, k, a, 4.6f, 7.4f, 3.4f); Droplet(g, k, a, 10.4f, 9.6f, 2.6f);
                    break;
                case 20: CloudShape(g, k, 2.5f, 3f, 11f, 7f, false); CloudArrow(g, a, true); break;
                case 21: CloudShape(g, k, 2.5f, 1.5f, 11f, 7f, false); CloudArrow(g, a, false); break;
                case 22: // 闪电
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(9f, 1f), new PointF(4.4f, 8.4f), new PointF(7.6f, 8.4f), new PointF(6f, 15f), new PointF(12f, 6.2f), new PointF(8.6f, 6.2f) });
                    break;
                case 23: Snowflake(g, k, a); break;
            }
        }

        /// <summary>太阳光芒短线。</summary>
        private static void SunRays(Graphics g, Color k, float cx, float cy, float rr)
        {
            using (var p = Pen(k, 1.2f))
                for (int i = 0; i < 8; i++)
                {
                    double ang = i * Math.PI / 4;
                    g.DrawLine(p, cx + (float)Math.Cos(ang) * (rr + 1.1f), cy + (float)Math.Sin(ang) * (rr + 1.1f),
                                  cx + (float)Math.Cos(ang) * (rr + 2.1f), cy + (float)Math.Sin(ang) * (rr + 2.1f));
                }
        }

        /// <summary>云形（x,y,w 云团外接框；fill=true 浅强调色填充）。</summary>
        private static void CloudShape(Graphics g, Color k, float x, float y, float w, float h, bool fill)
        {
            float cy = y + h * 0.42f;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(x + w * 0.06f, cy, w * 0.34f, h * 0.55f);
                path.AddEllipse(x + w * 0.30f, y, w * 0.38f, h * 0.72f);
                path.AddEllipse(x + w * 0.58f, cy - h * 0.04f, w * 0.34f, h * 0.6f);
                path.AddRectangle(new RectangleF(x + w * 0.08f, cy + h * 0.2f, w * 0.82f, h * 0.4f));
                if (fill) using (var br = new SolidBrush(Color.FromArgb(55, k.R, k.G, k.B))) g.FillPath(br, path);
                using (var p = new Pen(k, 1.25f)) g.DrawPath(p, path);
            }
        }

        /// <summary>云下雨滴：n 条竖线（drizzle=true 短虚线）。</summary>
        private static void Rain(Graphics g, Color a, int n, bool drizzle)
        {
            float[] xs = n == 3 ? new[] { 4.8f, 8f, 11.2f } : new[] { 3.8f, 5.9f, 8f, 10.1f, 12.2f };
            using (var p = new Pen(a, 1.3f) { StartCap = LineCap.Round, EndCap = LineCap.Round, DashStyle = drizzle ? DashStyle.Dot : DashStyle.Solid })
                foreach (var x in xs) g.DrawLine(p, x, 10.4f, x - 0.8f, drizzle ? 12f : 13.4f);
        }

        /// <summary>云下雪点。</summary>
        private static void SnowDots(Graphics g, Color a, int n)
        {
            float[] xs = { 4.5f, 8f, 11.5f, 6.2f };
            float[] ys = { 10.6f, 11.6f, 10.6f, 13.2f };
            using (var br = new SolidBrush(a))
                for (int i = 0; i < n; i++) g.FillEllipse(br, xs[i] - 0.8f, ys[i] - 0.8f, 1.6f, 1.6f);
        }

        /// <summary>温度计（hot=红液高，cold=蓝液低 + 顶雪花点）。</summary>
        private static void Thermo(Graphics g, Color k, Color a, bool hot)
        {
            using (var p = new Pen(k, 1.3f))
            {
                g.DrawRectangle(p, 6.4f, 1.8f, 3.2f, 9f);
                g.DrawEllipse(p, 5.4f, 10.2f, 5.2f, 5.2f);
            }
            using (var br = new SolidBrush(a))
            {
                float top = hot ? 3.2f : 8.2f;
                g.FillRectangle(br, 7.2f, top, 1.6f, 11.2f - top);
                g.FillEllipse(br, 6.2f, 11f, 3.6f, 3.6f);
            }
        }

        /// <summary>独立水滴。</summary>
        private static void Droplet(Graphics g, Color k, Color a, float cx, float cy, float r)
        {
            var pts = new[] { new PointF(cx, cy - r), new PointF(cx + r * 0.85f, cy + r * 0.55f), new PointF(cx - r * 0.85f, cy + r * 0.55f) };
            using (var br = new SolidBrush(Color.FromArgb(90, a.R, a.G, a.B))) g.FillPolygon(br, pts);
            using (var p = new Pen(k, 1.1f)) g.DrawPolygon(p, pts);
        }

        /// <summary>云侧上下箭头（云同步）。</summary>
        private static void CloudArrow(Graphics g, Color a, bool up)
        {
            Rot(g, up ? 270f : 90f, () =>
            {
                using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                    g.DrawLine(p, 5.2f, 12f, 10.8f, 12f);
            });
        }

        /// <summary>六瓣雪花。</summary>
        private static void Snowflake(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(a, 1.3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 3; i++)
                {
                    double ang = i * Math.PI / 3;
                    float dx = (float)Math.Cos(ang) * 6f, dy = (float)Math.Sin(ang) * 6f;
                    g.DrawLine(p, 8f - dx, 8f - dy, 8f + dx, 8f + dy);
                    foreach (int s in new[] { -1, 1 })
                    {
                        float bx = 8f + dx * 0.55f * s, by = 8f + dy * 0.55f * s;
                        g.DrawLine(p, bx, by, bx - dx * 0.22f - dy * 0.22f, by - dy * 0.22f + dx * 0.22f);
                        g.DrawLine(p, bx, by, bx - dx * 0.22f + dy * 0.22f, by - dy * 0.22f - dx * 0.22f);
                    }
                }
        }

        // ============ 数学符号族 30 ============
        private static void MathSym(Graphics g, Color k, Color a, int q)
        {
            // 适合直接排版的字符（Segoe UI 符号字体在 Win10/11 自带）
            string glyph = null;
            switch (q)
            {
                case 10: glyph = "\u221E"; break;  // ∞
                case 11: glyph = "\u2211"; break;  // Σ
                case 12: glyph = "\u222B"; break;  // ∫
                case 14: glyph = "\u03C0"; break;  // π
                case 15: glyph = "\u0394"; break;  // Δ
                case 16: glyph = "\u2220"; break;  // ∠
                case 17: glyph = "\u2225"; break;  // ∥
                case 18: glyph = "\u27C2"; break;  // ⟂
                case 9: glyph = "\u2030"; break;   // ‰
                case 22: glyph = "&"; break;
                case 24: glyph = "@"; break;
                case 26: glyph = "\u223C"; break;  // ∼
                case 27: glyph = "\u00B0"; break;  // °
                case 28: glyph = "\u2032"; break;  // ′
            }
            if (glyph != null)
            {
                using (var b = new SolidBrush(a))
                using (var f = new Font(q == 12 ? "Segoe UI" : "Segoe UI Symbol", q == 12 ? 11f : 11.5f, FontStyle.Regular, GraphicsUnit.Point))
                    Text(g, glyph, f, b, 0.2f);
                return;
            }
            using (var pen = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                switch (q)
                {
                    case 0: g.DrawLine(pen, 3.5f, 5.6f, 12.5f, 5.6f); g.DrawLine(pen, 3.5f, 10.4f, 12.5f, 10.4f); break;
                    case 1: g.DrawLine(pen, 3.5f, 5.6f, 12.5f, 5.6f); g.DrawLine(pen, 3.5f, 10.4f, 12.5f, 10.4f); g.DrawLine(pen, 11.5f, 3.2f, 4.5f, 12.8f); break;
                    case 2: g.DrawArc(pen, 3.5f, 4f, 4f, 3f, 190, 160); g.DrawArc(pen, 8.5f, 9f, 4f, 3f, 10, 160); break;
                    case 3: g.DrawLines(pen, new[] { new PointF(11f, 3.4f), new PointF(4.6f, 8f), new PointF(11f, 12.6f) }); break;
                    case 4: g.DrawLines(pen, new[] { new PointF(5f, 3.4f), new PointF(11.4f, 8f), new PointF(5f, 12.6f) }); break;
                    case 5: g.DrawLines(pen, new[] { new PointF(11f, 3.4f), new PointF(4.6f, 8f), new PointF(11f, 12.6f) }); g.DrawLine(pen, 3.6f, 12.6f, 12.4f, 12.6f); break;
                    case 6: g.DrawLines(pen, new[] { new PointF(5f, 3.4f), new PointF(11.4f, 8f), new PointF(5f, 12.6f) }); g.DrawLine(pen, 3.6f, 3.4f, 12.4f, 3.4f); break;
                    case 7: g.DrawLine(pen, 3.6f, 3.4f, 12.4f, 12.6f); g.FillEllipse(new SolidBrush(a), 3f, 2.8f, 1.5f, 1.5f); g.FillEllipse(new SolidBrush(a), 11.8f, 11.7f, 1.5f, 1.5f); break;
                    case 8: // 百分号
                        g.DrawLine(pen, 12f, 3.2f, 4f, 12.8f);
                        g.DrawEllipse(pen, 3.6f, 3.4f, 3.4f, 3.4f); g.DrawEllipse(pen, 9f, 9.2f, 3.4f, 3.4f);
                        break;
                    case 13: // 根号
                        g.DrawLines(pen, new[] { new PointF(2.2f, 7.6f), new PointF(4.6f, 12.4f), new PointF(7f, 2.8f), new PointF(13.6f, 2.8f) }); break;
                    case 19: // 正弦
                        using (var p2 = new Pen(k, 1f)) g.DrawLine(p2, 1.5f, 13.2f, 14.5f, 13.2f);
                        g.DrawBezier(pen, 2f, 8f, 4.5f, 2.5f, 7f, 2.5f, 8f, 8f);
                        g.DrawBezier(pen, 8f, 8f, 9f, 13.5f, 11.5f, 13.5f, 14f, 8f);
                        break;
                    case 20: // f(x)
                        using (var b = new SolidBrush(a))
                        using (var f = new Font("Segoe UI", 9.5f, FontStyle.Italic, GraphicsUnit.Point))
                            Text(g, "f(x)", f, b, 0.4f);
                        break;
                    case 21: // #
                        using (var p2 = new Pen(a, 1.6f))
                        { p2.StartCap = LineCap.Flat; p2.EndCap = LineCap.Flat;
                          g.DrawLine(p2, 5.2f, 2.6f, 3.8f, 13.4f); g.DrawLine(p2, 11.2f, 2.6f, 9.8f, 13.4f);
                          g.DrawLine(p2, 2.4f, 5.8f, 13.6f, 4.6f); g.DrawLine(p2, 2.4f, 10.8f, 13.6f, 9.6f); }
                        break;
                    case 23: // 星号
                        for (int i = 0; i < 3; i++)
                        {
                            double ang = i * Math.PI / 3;
                            g.DrawLine(pen, 8f - (float)Math.Cos(ang) * 4.2f, 8f - (float)Math.Sin(ang) * 4.2f,
                                           8f + (float)Math.Cos(ang) * 4.2f, 8f + (float)Math.Sin(ang) * 4.2f);
                        }
                        break;
                    case 25: g.DrawLine(pen, 3.6f, 8f, 12.4f, 8f); g.FillEllipse(new SolidBrush(a), 12.6f, 7.2f, 1.6f, 1.6f); break;
                    case 29: // …
                        using (var br = new SolidBrush(a))
                            for (int i = -1; i <= 1; i++) g.FillEllipse(br, 8f + i * 3.2f - 1f, 7f, 2f, 2f);
                        break;
                }
            }
        }
    }
}
