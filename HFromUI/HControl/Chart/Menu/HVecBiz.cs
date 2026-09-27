using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Chart.Menu
{
    /// <summary>
    /// 扩展图标引擎·语义手绘族 B：扩展图表 36（440..475）+ 排版对齐 24（476..499）
    /// + 进出登录 16（500..515）+ 杂项语义 23（516..538）。
    /// </summary>
    internal static partial class HVec
    {
        /// <summary>小坐标系底（灰墨轴线，给扩展图表族统一用）。</summary>
        private static void MiniAxes(Graphics g, Color k, bool y = true)
        {
            using (var p = new Pen(Color.FromArgb(110, k.R, k.G, k.B), 0.9f))
            {
                g.DrawLine(p, 1.6f, 14.2f, 14.4f, 14.2f);
                if (y) g.DrawLine(p, 1.6f, 1.6f, 1.6f, 14.2f);
            }
        }

        private static Color A50(Color a) => Color.FromArgb(70, a.R, a.G, a.B);
        private static Color A30(Color a) => Color.FromArgb(45, a.R, a.G, a.B);

        // ============ 扩展图表族 36 ============
        private static void ChartEx(Graphics g, Color k, Color a, int q)
        {
            switch (q)
            {
                case 0: // 仪表盘
                    using (var p = new Pen(k, 1.3f)) g.DrawArc(p, 2.2f, 3f, 11.6f, 11.6f, 180, 180);
                    using (var p = new Pen(A50(a), 3f)) g.DrawArc(p, 3.6f, 4.4f, 8.8f, 8.8f, 200, 55);
                    using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round }) g.DrawLine(p, 8f, 8.8f, 11.6f, 5.6f);
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 7.3f, 8.1f, 1.5f, 1.5f);
                    break;
                case 1: // 环形
                    using (var p = new Pen(A50(a), 2.6f)) g.DrawArc(p, 2f, 2f, 12f, 12f, 0, 360);
                    using (var p = new Pen(a, 2.6f)) g.DrawArc(p, 2f, 2f, 12f, 12f, -70, 130);
                    break;
                case 2: // 漏斗
                    using (var br = new SolidBrush(A50(a)))
                    using (var p = new Pen(k, 1.1f))
                        for (int i = 0; i < 3; i++)
                        {
                            float y = 2.5f + i * 3.4f, inset = i * 1.9f;
                            var pts = new[] { new PointF(2.5f + inset, y), new PointF(13.5f - inset, y),
                                              new PointF(13.5f - inset - 1.9f, y + 3f), new PointF(2.5f + inset + 1.9f, y + 3f) };
                            g.FillPolygon(br, pts); g.DrawPolygon(p, pts);
                        }
                    break;
                case 3: // 金字塔
                    using (var p = new Pen(k, 1.1f))
                    using (var br = new SolidBrush(A50(a)))
                        for (int i = 0; i < 3; i++)
                        {
                            float y1 = 3f + i * 3.6f, y2 = y1 + 3.4f, w1 = 4.6f + i * 2.3f, w2 = 4.6f + (i + 1) * 2.3f;
                            var pts = new[] { new PointF(8f - w1, y1), new PointF(8f + w1, y1), new PointF(8f + w2, y2), new PointF(8f - w2, y2) };
                            g.FillPolygon(br, pts); g.DrawPolygon(p, pts);
                        }
                    break;
                case 4:
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(a))
                    {
                        var pts = new[] { new PointF(3.4f, 11f), new PointF(6.5f, 7.5f), new PointF(9.6f, 9.6f), new PointF(12.4f, 5f) };
                        foreach (var p2 in pts) g.FillEllipse(br, p2.X - 1.1f, p2.Y - 1.1f, 2.2f, 2.2f);
                    }
                    break;
                case 5:
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(A50(a)))
                    using (var p = new Pen(a, 1f))
                    {
                        g.FillEllipse(br, 3f, 9f, 4f, 4f); g.DrawEllipse(p, 3f, 9f, 4f, 4f);
                        g.FillEllipse(br, 8f, 6f, 5.4f, 5.4f); g.DrawEllipse(p, 8f, 6f, 5.4f, 5.4f);
                        g.FillEllipse(br, 6.4f, 4.2f, 2.8f, 2.8f); g.DrawEllipse(p, 6.4f, 4.2f, 2.8f, 2.8f);
                    }
                    break;
                case 6: // 热力 3×3
                    for (int r = 0; r < 3; r++) for (int c = 0; c < 3; c++)
                        {
                            int al = 40 + (r * 3 + c) * 22;
                            using (var br = new SolidBrush(Color.FromArgb(Math.Min(235, al), a.R, a.G, a.B)))
                                g.FillRectangle(br, 2.6f + c * 3.7f, 2.6f + r * 3.7f, 3.5f, 3.5f);
                        }
                    using (var p = new Pen(k, 1f)) g.DrawRectangle(p, 2.6f, 2.6f, 10.9f, 10.9f);
                    break;
                case 7: // 矩形树图
                    using (var p = new Pen(Color.White, 1.2f))
                    using (var b1 = new SolidBrush(A50(a)))
                    using (var b2 = new SolidBrush(A30(a)))
                    {
                        g.FillRectangle(b1, 1.5f, 1.5f, 8f, 13f); g.FillRectangle(b2, 9.5f, 1.5f, 5f, 6.2f);
                        g.FillRectangle(b1, 9.5f, 7.7f, 5f, 6.8f);
                        g.DrawRectangle(p, 1.5f, 1.5f, 13f, 13f); g.DrawLine(p, 9.5f, 1.5f, 9.5f, 14.5f); g.DrawLine(p, 9.5f, 7.7f, 14.5f, 7.7f);
                    }
                    break;
                case 8: // 桑基流
                    using (var p1 = new Pen(Color.FromArgb(120, a.R, a.G, a.B), 2.4f))
                    using (var p2 = new Pen(Color.FromArgb(70, a.R, a.G, a.B), 2.4f))
                    {
                        g.DrawLine(p1, 1.8f, 3.5f, 14.2f, 5.5f);
                        g.DrawLine(p2, 1.8f, 8f, 14.2f, 8f);
                        g.DrawLine(p2, 1.8f, 12.5f, 14.2f, 10.5f);
                    }
                    break;
                case 9: // KPI 卡
                    using (var path = RoundR(new RectangleF(1.5f, 3f, 13f, 9f), 3f))
                    {
                        using (var br = new SolidBrush(A30(a))) g.FillPath(br, path);
                        using (var p = new Pen(k, 1.1f)) g.DrawPath(p, path);
                    }
                    using (var p = new Pen(a, 1.4f) { EndCap = LineCap.ArrowAnchor }) g.DrawLine(p, 9.5f, 11.6f, 12.8f, 8.2f);
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Segoe UI", 7f, FontStyle.Bold, GraphicsUnit.Point))
                        g.DrawString("KPI", f, b, 3.2f, 4.6f);
                    break;
                case 10: // 直方图（紧贴柱）
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(A50(a)))
                    using (var p = new Pen(a, 1f))
                        for (int i = 0; i < 5; i++) { float h = 3f + (i % 3) * 2.2f; g.FillRectangle(br, 2.6f + i * 2.3f, 14f - h, 2.2f, h); g.DrawRectangle(p, 2.6f + i * 2.3f, 14f - h, 2.2f, h); }
                    break;
                case 11: // 阶梯面积
                    MiniAxes(g, k);
                    using (var path = new GraphicsPath())
                    {
                        var steps = new[] { new PointF(2, 11f), new PointF(5f, 11f), new PointF(5f, 7f), new PointF(9f, 7f), new PointF(9f, 4.5f), new PointF(14f, 4.5f) };
                        path.AddLines(steps); path.AddLine(14f, 14f, 2f, 14f); path.CloseFigure();
                        using (var br = new SolidBrush(A50(a))) g.FillPath(br, path);
                        using (var p = new Pen(a, 1.3f)) g.DrawLines(p, steps);
                    }
                    break;
                case 12: // 折线+端点
                    MiniAxes(g, k);
                    var lp = new[] { new PointF(3f, 11f), new PointF(7f, 8f), new PointF(11f, 9.5f), new PointF(13.4f, 4.6f) };
                    using (var p = new Pen(a, 1.4f)) g.DrawLines(p, lp);
                    using (var br = new SolidBrush(Color.White))
                    using (var p = new Pen(a, 1.1f))
                        foreach (var p2 in lp) { g.FillEllipse(br, p2.X - 1.2f, p2.Y - 1.2f, 2.4f, 2.4f); g.DrawEllipse(p, p2.X - 1.2f, p2.Y - 1.2f, 2.4f, 2.4f); }
                    break;
                case 13:
                    MiniAxes(g, k);
                    using (var p = new Pen(a, 1.4f)) g.DrawLines(p, new[] { new PointF(2.5f, 10f), new PointF(6f, 6f), new PointF(9f, 8f), new PointF(13.5f, 4f) });
                    using (var p = new Pen(k, 1.3f)) g.DrawLines(p, new[] { new PointF(2.5f, 12.5f), new PointF(6f, 11f), new PointF(10f, 11.5f), new PointF(13.5f, 9f) });
                    break;
                case 14: // 柱+线
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(A50(a)))
                        for (int i = 0; i < 3; i++) { float h = 4f + i * 2f; g.FillRectangle(br, 3f + i * 3.6f, 14f - h, 2.6f, h); }
                    using (var p = new Pen(a, 1.4f)) g.DrawLines(p, new[] { new PointF(3.5f, 8.6f), new PointF(7f, 7f), new PointF(10.6f, 4.5f), new PointF(13.6f, 3f) });
                    break;
                case 15: // 区间柱（高低线+柱段）
                    MiniAxes(g, k);
                    using (var p = new Pen(k, 1.1f))
                    using (var br = new SolidBrush(A50(a)))
                        for (int i = 0; i < 3; i++)
                        {
                            float x = 4f + i * 4f;
                            g.DrawLine(p, x, 4f + i, x, 12.5f - i);
                            g.FillRectangle(br, x - 1.1f, 6.5f, 2.2f, 4f);
                        }
                    break;
                case 16: // 误差棒
                    MiniAxes(g, k);
                    using (var p = new Pen(a, 1.3f))
                    using (var br = new SolidBrush(a))
                        for (int i = 0; i < 3; i++)
                        {
                            float x = 4f + i * 4f, y = 6f + (i == 1 ? 3f : 0f);
                            g.DrawLine(p, x, y - 2.2f, x, y + 2.2f); g.DrawLine(p, x - 0.9f, y - 2.2f, x + 0.9f, y - 2.2f); g.DrawLine(p, x - 0.9f, y + 2.2f, x + 0.9f, y + 2.2f);
                            g.FillEllipse(br, x - 1f, y - 1f, 2f, 2f);
                        }
                    break;
                case 17:
                    MiniAxes(g, k);
                    using (var p = new Pen(a, 1.6f) { EndCap = LineCap.ArrowAnchor })
                        g.DrawLines(p, new[] { new PointF(2.6f, 4.6f), new PointF(7f, 8f), new PointF(13.2f, 12.4f) });
                    break;
                case 18:
                    MiniAxes(g, k);
                    using (var p = new Pen(a, 1.6f)) g.DrawLine(p, 2.6f, 8.5f, 13.4f, 8.5f);
                    using (var br = new SolidBrush(a)) { g.FillEllipse(br, 2f, 7.8f, 1.5f, 1.5f); g.FillEllipse(br, 12.8f, 7.8f, 1.5f, 1.5f); }
                    break;
                case 19: // 最大最小虚线
                    MiniAxes(g, k);
                    using (var p = new Pen(a, 1.1f) { DashStyle = DashStyle.Dash }) { g.DrawLine(p, 2.4f, 4.6f, 14f, 4.6f); g.DrawLine(p, 2.4f, 12f, 14f, 12f); }
                    using (var p = new Pen(k, 1.3f)) g.DrawLines(p, new[] { new PointF(3f, 7f), new PointF(6.5f, 9.5f), new PointF(10f, 6f), new PointF(13f, 8.5f) });
                    break;
                case 20: // 均值线
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(a))
                        foreach (var p2 in new[] { new PointF(3.5f, 6f), new PointF(7f, 10f), new PointF(10.5f, 7.5f), new PointF(13f, 9f) })
                            g.FillEllipse(br, p2.X - 1f, p2.Y - 1f, 2f, 2f);
                    using (var p = new Pen(a, 1.2f) { DashStyle = DashStyle.Dash }) g.DrawLine(p, 2.4f, 8.2f, 14f, 8.2f);
                    break;
                case 21: // 阈值线 + 柱
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(A50(a)))
                        for (int i = 0; i < 3; i++) { float h = 5f + i * 2.4f; g.FillRectangle(br, 3f + i * 3.6f, 14f - h, 2.6f, h); }
                    using (var p = new Pen(a, 1.4f)) g.DrawLine(p, 2.4f, 5.6f, 14f, 5.6f);
                    break;
                case 22: // 脉冲标记
                    using (var p = new Pen(Color.FromArgb(120, a.R, a.G, a.B), 1.2f)) { g.DrawEllipse(p, 3.6f, 3.6f, 8.8f, 8.8f); g.DrawEllipse(p, 5.6f, 5.6f, 4.8f, 4.8f); }
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 6.6f, 6.6f, 2.8f, 2.8f);
                    break;
                case 23: // 气泡里的小折线
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(1f, 1.5f, 12f, 9.5f);
                        path.AddPolygon(new[] { new PointF(4.5f, 10.6f), new PointF(7f, 10.6f), new PointF(3.2f, 14.6f) });
                        using (var br = new SolidBrush(A30(a))) g.FillPath(br, path);
                        using (var p = new Pen(k, 1.2f)) g.DrawPath(p, path);
                    }
                    using (var p = new Pen(a, 1.4f)) g.DrawLines(p, new[] { new PointF(3f, 7.6f), new PointF(5.6f, 5.4f), new PointF(8f, 6.8f), new PointF(11f, 4f) });
                    break;
                case 24:
                    MiniAxes(g, k);
                    using (var p = new Pen(a, 1.2f) { DashStyle = DashStyle.Dash }) g.DrawRectangle(p, 4.5f, 4.5f, 7f, 7.5f);
                    break;
                case 25: // 分屏图
                    using (var p = new Pen(k, 1f))
                    {
                        g.DrawRectangle(p, 1.8f, 2f, 12.4f, 5.4f); g.DrawRectangle(p, 1.8f, 8.4f, 12.4f, 5.2f);
                        g.DrawLine(p, 1.8f, 7.6f, 14.2f, 7.6f);
                    }
                    using (var p = new Pen(a, 1.3f))
                    {
                        g.DrawLines(p, new[] { new PointF(3f, 5.8f), new PointF(6f, 4f), new PointF(10f, 5f), new PointF(13f, 3.4f) });
                        g.DrawLines(p, new[] { new PointF(3f, 11.6f), new PointF(7f, 10f), new PointF(12.8f, 11.8f) });
                    }
                    break;
                case 26:
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(Color.FromArgb(150, a.R, a.G, a.B)))
                        for (int r = 0; r < 3; r++) for (int c = 0; c < 4; c++)
                                g.FillEllipse(br, 3.4f + c * 3f + -0.6f, 4.2f + r * 3.2f - 0.6f, 1.2f, 1.2f);
                    break;
                case 27:
                    MiniAxes(g, k);
                    using (var p = new Pen(Color.FromArgb(120, a.R, a.G, a.B), 0.9f) { DashStyle = DashStyle.Dash })
                        for (int i = 1; i < 4; i++) { g.DrawLine(p, 1.6f, 2f + i * 3f, 14f, 2f + i * 3f); g.DrawLine(p, 2f + i * 3f, 2f, 2f + i * 3f, 14f); }
                    break;
                case 28: // 3D 轴
                    using (var p = new Pen(a, 1.5f) { EndCap = LineCap.ArrowAnchor })
                    {
                        g.DrawLine(p, 2.5f, 13f, 2.5f, 2.6f);
                        g.DrawLine(p, 2.5f, 13f, 13.4f, 13f);
                        g.DrawLine(p, 2.5f, 13f, 6.5f, 9f);
                    }
                    break;
                case 29: // 半圆饼
                    using (var br = new SolidBrush(A50(a))) g.FillPie(br, 2f, 4f, 12f, 12f, 180, 180);
                    using (var br = new SolidBrush(a)) g.FillPie(br, 2f, 4f, 12f, 12f, 180, 70);
                    using (var p = new Pen(k, 1.1f)) { g.DrawArc(p, 2f, 4f, 12f, 12f, 180, 180); g.DrawLine(p, 2f, 10f, 14f, 10f); }
                    break;
                case 30:
                    var rp = new[] { new PointF(8f, 2.4f), new PointF(13.2f, 6f), new PointF(11f, 12.8f), new PointF(5f, 12.8f), new PointF(2.8f, 6f) };
                    using (var p = new Pen(Color.FromArgb(90, k.R, k.G, k.B), 0.9f))
                        for (int i = 0; i < 5; i++) { g.DrawLine(p, 8f, 8f, rp[i].X, rp[i].Y); }
                    using (var p2 = new Pen(a, 1.3f)) g.DrawPolygon(p2, rp);
                    using (var br = new SolidBrush(A50(a))) g.FillPolygon(br, rp);
                    break;
                case 31: // 平滑面积
                    MiniAxes(g, k);
                    using (var path = new GraphicsPath())
                    {
                        path.AddBezier(2f, 12f, 5f, 4f, 8f, 12f, 11f, 5f);
                        path.AddBezier(11f, 5f, 12.4f, 2.4f, 13.4f, 5f, 14f, 6f);
                        path.AddLine(14f, 14f, 2f, 14f); path.CloseFigure();
                        using (var br = new SolidBrush(A50(a))) g.FillPath(br, path);
                    }
                    break;
                case 32: // 瀑布
                    MiniAxes(g, k);
                    using (var br = new SolidBrush(A50(a)))
                    {
                        g.FillRectangle(br, 2.8f, 8.5f, 2.6f, 5.5f);
                        g.FillRectangle(br, 6.4f, 5.5f, 2.6f, 3f);
                        g.FillRectangle(br, 10f, 6.8f, 2.6f, 4.2f);
                    }
                    break;
                case 33: // 点图
                    using (var p = new Pen(k, 1f)) g.DrawLine(p, 1.8f, 8f, 14.2f, 8f);
                    using (var br = new SolidBrush(a))
                        for (int i = 0; i < 6; i++)
                            g.FillEllipse(br, 2.6f + i * 1.9f, 7.4f - (i % 2) * 0.0f + (i > 2 ? 1.6f : 0f), 1.5f, 1.5f);
                    break;
                case 34: // 南丁格尔玫瑰
                    using (var p = new Pen(Color.White, 0.8f))
                        for (int i = 0; i < 6; i++)
                        {
                            float r = 3.5f + (i % 3) * 1.7f;
                            var pts = new[] { new PointF(8f, 8f), Poly(1, r, i * 60f - 90f)[0], Poly(1, r, (i + 1) * 60f - 90f)[0] };
                            using (var br = new SolidBrush(i % 2 == 0 ? a : A50(a))) g.FillPolygon(br, pts);
                            g.DrawPolygon(p, pts);
                        }
                    break;
                case 35: // XY 十字缩放
                    using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 2.4f, 2.4f, 11.2f, 11.2f);
                    using (var p = new Pen(a, 1.6f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor })
                    { g.DrawLine(p, 5f, 8f, 11f, 8f); g.DrawLine(p, 8f, 5f, 8f, 11f); }
                    break;
            }
        }

        // ============ 排版/对齐族 24 ============
        private static void UiSym(Graphics g, Color k, Color a, int q)
        {
            switch (q)
            {
                case 0: Str(g, a, "B", FontStyle.Bold, 10f); break;
                case 1: Str(g, a, "I", FontStyle.Italic, 10f); break;
                case 2:
                    Str(g, a, "U", FontStyle.Underline, 9.5f);
                    using (var p = new Pen(a, 1.5f)) g.DrawLine(p, 3.6f, 12.6f, 12.4f, 12.6f); break;
                case 3:
                    Str(g, a, "S", FontStyle.Strikeout, 9.5f); break;
                case 4:
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Segoe UI", 10.5f, FontStyle.Bold, GraphicsUnit.Point)) Text(g, "T", f, b, 0.2f);
                    break;
                case 5:
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Segoe UI", 9f, FontStyle.Bold, GraphicsUnit.Point)) Text(g, "H1", f, b, 0.2f);
                    using (var p = new Pen(k, 1.1f)) g.DrawLine(p, 3.2f, 11.6f, 12.8f, 11.6f);
                    break;
                case 6:
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Segoe UI", 6.5f, FontStyle.Bold, GraphicsUnit.Point))
                        for (int i = 0; i < 3; i++) g.DrawString((i + 1) + ".", f, b, 2f, 3.4f + i * 3.5f);
                    using (var p = new Pen(k, 1.2f)) for (int i = 0; i < 3; i++) g.DrawLine(p, 6f, 5.4f + i * 3.5f, 14f, 5.4f + i * 3.5f);
                    break;
                case 7:
                    using (var p = new Pen(a, 1.4f))
                        for (int i = 0; i < 3; i++) g.DrawLines(p, new[] { new PointF(2.2f, 5f + i * 3.5f), new PointF(3.4f, 6.2f + i * 3.5f), new PointF(5f, 4f + i * 3.5f) });
                    using (var p2 = new Pen(k, 1.2f)) for (int i = 0; i < 3; i++) g.DrawLine(p2, 6.5f, 5.2f + i * 3.5f, 14f, 5.2f + i * 3.5f);
                    break;
                case 8:
                    using (var br = new SolidBrush(a)) g.FillRectangle(br, 2.4f, 3f, 1.8f, 10f);
                    using (var p = new Pen(k, 1.2f)) { g.DrawLine(p, 6f, 5.4f, 13.5f, 5.4f); g.DrawLine(p, 6f, 8.4f, 13.5f, 8.4f); g.DrawLine(p, 6f, 11.4f, 11f, 11.4f); }
                    break;
                case 9: Align(g, k, a, 0); break;
                case 10: Align(g, k, a, 1); break;
                case 11: Align(g, k, a, 2); break;
                case 12: Align(g, k, a, 3); break;
                case 13: AlignV(g, k, a, 0); break;
                case 14: AlignV(g, k, a, 1); break;
                case 15: AlignV(g, k, a, 2); break;
                case 16: Dist(g, k, a, true); break;
                case 17: Dist(g, k, a, false); break;
                case 18: Indent(g, k, a, true); break;
                case 19: Indent(g, k, a, false); break;
                case 20:
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Segoe UI", 10f, FontStyle.Bold, GraphicsUnit.Point)) Text(g, "A", f, b, 0.6f);
                    using (var p = new Pen(k, 1.2f))
                    {
                        g.DrawLine(p, 10f, 3f, 13.4f, 3f); g.DrawLine(p, 12f, 2.2f, 13.4f, 3f); g.DrawLine(p, 12f, 3.8f, 13.4f, 3f);
                        g.DrawLine(p, 10f, 12.5f, 13.4f, 12.5f); g.DrawLine(p, 12f, 11.7f, 13.4f, 12.5f); g.DrawLine(p, 12f, 13.3f, 13.4f, 12.5f);
                    }
                    break;
                case 21:
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point)) Text(g, "Aa", f, b, 0.4f);
                    break;
                case 22:
                    using (var br = new SolidBrush(A50(a))) g.FillRectangle(br, 2.4f, 8.6f, 11.2f, 2.8f);
                    using (var p = new Pen(k, 1.2f)) g.DrawLine(p, 3f, 6f, 12f, 11f);
                    using (var p = new Pen(a, 1.4f)) g.DrawLine(p, 2.4f, 9.9f, 13.6f, 9.9f);
                    break;
                case 23:
                    Str(g, a, "\u00B6", FontStyle.Regular, 11f); break;
            }
        }

        /// <summary>排版字符串。</summary>
        private static void Str(Graphics g, Color a, string s, FontStyle st, float sz)
        {
            using (var b = new SolidBrush(a))
            using (var f = new Font("Segoe UI", sz, st, GraphicsUnit.Point))
                Text(g, s, f, b, 0.3f);
        }

        /// <summary>水平对齐：0 左 1 中 2 右 3 两端。</summary>
        private static void Align(Graphics g, Color k, Color a, int mode)
        {
            float xs, xe;
            if (mode == 0) { xs = 2.5f; xe = 9f; }
            else if (mode == 2) { xs = 7f; xe = 13.5f; }
            else { xs = 4.5f; xe = 11.5f; }
            using (var p = new Pen(k, 1.3f))
            {
                float[] ys = { 3.4f, 7.4f, 11.4f };
                for (int i = 0; i < 3; i++)
                {
                    float e = mode == 3 ? 13.5f : (i == 2 ? xe - 2f : xe);
                    g.DrawLine(p, mode == 3 ? 2.5f : xs, ys[i], e, ys[i]);
                }
            }
        }

        /// <summary>垂直对齐：0 顶 1 中 2 底（转 90° 复用水平）。</summary>
        private static void AlignV(Graphics g, Color k, Color a, int mode)
        {
            var st = g.Save();
            g.TranslateTransform(8f, 8f); g.RotateTransform(90f); g.TranslateTransform(-8f, -8f);
            Align(g, k, a, mode);
            g.Restore(st);
        }

        /// <summary>横向/纵向等距分布：等距双箭头 + 三个块。</summary>
        private static void Dist(Graphics g, Color k, Color a, bool horizontal)
        {
            using (var br = new SolidBrush(A50(a)))
            {
                if (horizontal)
                    for (int i = 0; i < 3; i++) g.FillRectangle(br, 2.4f + i * 4.6f, 2.6f, 2.8f, 3f);
                else
                    for (int i = 0; i < 3; i++) g.FillRectangle(br, 2.6f, 2.4f + i * 4.6f, 3f, 2.8f);
            }
            using (var p = new Pen(a, 1.4f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor })
            {
                if (horizontal) g.DrawLine(p, 3f, 10.8f, 13f, 10.8f);
                else g.DrawLine(p, 10.8f, 3f, 10.8f, 13f);
            }
        }

        /// <summary>缩进/凸出：三行线 + 左折箭头。</summary>
        private static void Indent(Graphics g, Color k, Color a, bool inward)
        {
            using (var p = new Pen(k, 1.2f))
                for (int i = 0; i < 3; i++) g.DrawLine(p, inward ? 6.5f : 2.5f, 4f + i * 3.6f, 13.5f, 4f + i * 3.6f);
            Rot(g, inward ? 0f : 180f, () =>
            {
                using (var p = new Pen(a, 1.4f) { EndCap = LineCap.ArrowAnchor })
                { g.DrawLine(p, 1.6f, 8f, 6f, 8f); g.DrawLine(p, 3.4f, 5.6f, 6f, 8f); g.DrawLine(p, 3.4f, 10.4f, 6f, 8f); }
            });
        }

        // ============ 进出/登录族 16 ============
        private static void Login(Graphics g, Color k, Color a, int q)
        {
            int dir = q % 4;          // R,L,T,B
            int kind = q / 4;         // 0 登录 1 退出 2 进入 3 离开
            float rot = dir * 90f;
            bool into = kind == 0 || kind == 2;
            bool door = kind >= 2;
            Rot(g, rot, () =>
            {
                using (var p = new Pen(k, 1.4f))
                {
                    if (door)
                    {
                        // 门框：左柱 + 上下楣，中间开口
                        g.DrawLine(p, 5f, 2.6f, 5f, 13.4f);
                        g.DrawLine(p, 5f, 2.6f, 13.2f, 2.6f);
                        g.DrawLine(p, 5f, 13.4f, 13.2f, 13.4f);
                    }
                    else
                    {
                        g.DrawRectangle(p, 4.6f, 2.6f, 8.8f, 10.8f);
                        if (!into) using (var pp = new Pen(k, 1.2f)) g.DrawLine(pp, 9f, 2.6f, 9f, 13.4f);
                    }
                }
                using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                {
                    if (into) g.DrawLine(p, 1.4f, 8f, 9.6f, 8f);       // 箭头朝右进盒
                    else g.DrawLine(p, 9f, 8f, 14.4f, 8f);             // 箭头出盒
                }
            });
        }

        // ============ 杂项语义族 23 ============
        private static void Misc(Graphics g, Color k, Color a, int q)
        {
            switch (q)
            {
                case 0: // 夕阳（半日落地平线）
                    using (var p = new Pen(k, 1.3f)) g.DrawLine(p, 1.6f, 9.6f, 14.4f, 9.6f);
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 4.8f, 5.6f, 6.4f, 6.4f);
                    using (var p = new Pen(k, 1.2f)) g.DrawArc(p, 4.8f, 5.6f, 6.4f, 6.4f, 180, 180);
                    using (var p = new Pen(a, 1.3f)) g.DrawLine(p, 2.6f, 5.6f, 4.2f, 5.6f);
                    break;
                case 1: // 沙丘
                    using (var p = new Pen(k, 1.3f))
                    {
                        g.DrawArc(p, -2f, 6.5f, 11f, 12f, 200, 130);
                        g.DrawArc(p, 5f, 7.5f, 13f, 12f, 200, 130);
                    }
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 11.6f, 2.6f, 2.4f, 2.4f);
                    break;
                case 2: // 矩阵终端
                    using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 2f, 2.5f, 12f, 11f);
                    using (var b = new SolidBrush(a))
                    using (var f = new Font("Consolas", 7f, FontStyle.Bold, GraphicsUnit.Point))
                    { g.DrawString("10", f, b, 3.4f, 4f); g.DrawString("01", f, b, 7.4f, 7f); }
                    break;
                case 3: // 卷轴
                    using (var p = new Pen(k, 1.2f))
                    {
                        g.DrawRectangle(p, 3f, 4f, 9f, 8.5f);
                        g.DrawEllipse(p, 2.4f, 3.2f, 10.2f, 1.8f);
                        g.DrawLine(p, 5f, 7.4f, 10.5f, 7.4f); g.DrawLine(p, 5f, 9.6f, 10.5f, 9.6f);
                    }
                    using (var br = new SolidBrush(A50(a))) g.FillEllipse(br, 2.4f, 3.2f, 10.2f, 1.8f);
                    break;
                case 4: // 叶
                    using (var path = new GraphicsPath())
                    {
                        path.AddBezier(2.5f, 13f, 2f, 5f, 8f, 1.8f, 13.6f, 2.6f);
                        path.AddBezier(13.6f, 2.6f, 11f, 9f, 6.5f, 12.8f, 2.5f, 13f);
                        path.CloseFigure();
                        using (var br = new SolidBrush(A50(a))) g.FillPath(br, path);
                        using (var p = new Pen(k, 1.2f)) g.DrawPath(p, path);
                    }
                    using (var p2 = new Pen(a, 1.2f)) g.DrawBezier(p2, 3.4f, 12f, 6.5f, 8.6f, 9.5f, 5.6f, 12.6f, 3.4f);
                    break;
                case 5: // 仙人掌
                    using (var p = new Pen(a, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLine(p, 8f, 3f, 8f, 13.4f);
                        g.DrawLine(p, 4.8f, 9.5f, 4.8f, 7f); g.DrawLine(p, 4.8f, 7f, 8f, 7f);
                        g.DrawLine(p, 11.2f, 11f, 11.2f, 8.6f); g.DrawLine(p, 8f, 8.6f, 11.2f, 8.6f);
                    }
                    using (var p2 = new Pen(k, 1.2f)) g.DrawLine(p2, 3f, 13.6f, 13f, 13.6f);
                    break;
                case 6: // 色板
                    using (var br = new SolidBrush(a))
                    {
                        g.FillEllipse(br, 2.4f, 2.6f, 4.4f, 4.4f);
                        g.FillEllipse(br, 9.2f, 2.6f, 4.4f, 4.4f);
                        g.FillEllipse(br, 2.4f, 9.2f, 4.4f, 4.4f);
                        g.FillEllipse(br, 9.2f, 9.2f, 4.4f, 4.4f);
                    }
                    break;
                case 7: // 对话气泡 + 三点
                    using (var path = new GraphicsPath())
                    {
                        path.AddEllipse(1.4f, 2f, 13.2f, 9f);
                        path.AddPolygon(new[] { new PointF(5f, 10.4f), new PointF(7.6f, 10.4f), new PointF(3.6f, 14f) });
                        using (var br = new SolidBrush(A30(a))) g.FillPath(br, path);
                        using (var p = new Pen(k, 1.3f)) g.DrawPath(p, path);
                    }
                    using (var br = new SolidBrush(a))
                        for (int i = -1; i <= 1; i++) g.FillEllipse(br, 8f + i * 2.6f - 1f, 5.6f, 2f, 2f);
                    break;
                case 8: // 开关
                    using (var path = RoundR(new RectangleF(1.6f, 5.2f, 12.8f, 6.2f), 3.1f))
                    using (var br = new SolidBrush(a)) g.FillPath(br, path);
                    using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, 9.6f, 5.9f, 4.8f, 4.8f);
                    break;
                case 9: // 四角闪光
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(8f, 1.4f), new PointF(9.6f, 6.4f), new PointF(14.6f, 8f), new PointF(9.6f, 9.6f),
                                                   new PointF(8f, 14.6f), new PointF(6.4f, 9.6f), new PointF(1.4f, 8f), new PointF(6.4f, 6.4f) });
                    using (var br = new SolidBrush(A50(a))) g.FillPolygon(br, Star(4, 1.4f, 0.5f, -90f, 13f, 3.4f));
                    break;
                case 10: Battery(g, k, a, 0f, false); break;
                case 11: Battery(g, k, a, 0.25f, false); break;
                case 12: Battery(g, k, a, 0.55f, false); break;
                case 13: Battery(g, k, a, 1f, false); break;
                case 14: Battery(g, k, a, 1f, true); break;
                case 15: Signal(g, k, a, 0); break;
                case 16: Signal(g, k, a, 1); break;
                case 17: Signal(g, k, a, 2); break;
                case 18: Signal(g, k, a, 3); break;
                case 19: // 麦克风
                    using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 5.6f, 1.8f, 4.8f, 7.4f);
                    using (var p = new Pen(a, 1.6f))
                    {
                        g.DrawArc(p, 3.8f, 3.2f, 8.4f, 8.4f, 200, 140);
                        g.DrawLine(p, 8f, 11.6f, 8f, 14f); g.DrawLine(p, 5.6f, 14f, 10.4f, 14f);
                    }
                    break;
                case 20: // 耳机
                    using (var p = new Pen(a, 1.6f)) g.DrawArc(p, 2.8f, 2f, 10.4f, 10.4f, 180, 180);
                    using (var p = new Pen(k, 1.3f))
                    {
                        g.DrawRectangle(p, 2.2f, 7.4f, 2.6f, 4.6f);
                        g.DrawRectangle(p, 11.2f, 7.4f, 2.6f, 4.6f);
                    }
                    break;
                case 21: // 键盘
                    using (var p = new Pen(k, 1.2f))
                    {
                        g.DrawRectangle(p, 1.8f, 5f, 12.4f, 6.6f);
                        for (int i = 0; i < 5; i++) g.DrawLine(p, 4.2f + i * 1.9f, 7.4f, 4.2f + i * 1.9f, 7.4f);
                        g.DrawLine(p, 4.4f, 9.4f, 11.6f, 9.4f);
                    }
                    break;
                case 22: // 投影仪
                    using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 3f, 6.4f, 9f, 5.4f);
                    using (var br = new SolidBrush(a)) g.FillEllipse(br, 10.2f, 7.8f, 2.6f, 2.6f);
                    using (var p = new Pen(a, 1.2f)) { g.DrawLine(p, 2.2f, 4f, 3.4f, 6f); g.DrawLine(p, 5f, 2.8f, 5.8f, 6f); g.DrawLine(p, 13.4f, 5.4f, 12.4f, 7f); }
                    break;
            }
        }

        /// <summary>电池：level 0..1 电量比，charge 闪电。</summary>
        private static void Battery(Graphics g, Color k, Color a, float level, bool charge)
        {
            using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 2f, 4.6f, 10.6f, 7f);
            using (var p = new Pen(k, 1.4f)) { g.DrawLine(p, 12.6f, 6.8f, 13.8f, 6.8f); g.DrawLine(p, 12.6f, 9.4f, 13.8f, 9.4f); }
            float fw = 8.8f * level;
            if (fw > 0.1f) using (var br = new SolidBrush(level < 0.3f ? Color.FromArgb(210, 90, 70) : a))
                    g.FillRectangle(br, 2.9f, 5.5f, fw - 0.2f, 5.2f);
            if (charge)
                using (var br = new SolidBrush(Color.FromArgb(255, 214, 90)))
                    g.FillPolygon(br, new[] { new PointF(8.4f, 5.6f), new PointF(6.2f, 8.6f), new PointF(7.7f, 8.6f), new PointF(7f, 10.8f), new PointF(9.4f, 7.6f), new PointF(7.9f, 7.6f) });
        }

        /// <summary>手机信号格：bars 0..3。</summary>
        private static void Signal(Graphics g, Color k, Color a, int bars)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = 2.6f + i * 2.9f, h = 2.6f + i * 2.8f, y = 13.6f - h;
                using (var br = new SolidBrush(i < bars ? a : A50(k)))
                    g.FillRectangle(br, x, y, 2f, h);
            }
        }
    }
}
