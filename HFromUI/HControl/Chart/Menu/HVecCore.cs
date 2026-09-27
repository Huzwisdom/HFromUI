using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Chart.Menu
{
    /// <summary>
    /// 扩展图标矢量引擎（数据驱动）：HMenuGlyph 编号 130..539 的 410 个图标全部由
    /// “方向/样式/容器/形状”等少量参数实时生成，墨色 k 描线、强调色 a 实底，随菜单配色变色。
    /// 本文件为核心族（箭头/雪佛龙/折角/容器符号/数字/三角/多边形/月相），
    /// 其余手绘语义族在 HVecArt.cs、HVecBiz.cs（同类 partial）。
    /// </summary>
    internal static partial class HVec
    {
        // ============ 基础小工具 ============

        /// <summary>圆头实线笔。</summary>
        private static Pen Pen(Color c, float w, bool round = true)
        {
            var p = new Pen(c, w);
            if (round) { p.StartCap = LineCap.Round; p.EndCap = LineCap.Round; }
            return p;
        }

        /// <summary>绕中心旋转执行绘制（angleDeg=0 朝右，顺时针）。</summary>
        private static void Rot(Graphics g, float angleDeg, Action body)
        {
            var st = g.Save();
            g.TranslateTransform(8f, 8f);
            g.RotateTransform(angleDeg);
            g.TranslateTransform(-8f, -8f);
            body();
            g.Restore(st);
        }

        /// <summary>在填充色上取黑/白对比色（实底容器里画符号用）。</summary>
        private static Color ContrastOn(Color c)
            => (c.R * 299 + c.G * 587 + c.B * 114) / 1000 > 150 ? Color.FromArgb(40, 40, 45) : Color.White;

        /// <summary>方向序号 0..7（上/右上/右/右下/下/左下/左/左上）→ 旋转角。</summary>
        private static float DirAngle(int dir) => 270f - dir * 45f;

        /// <summary>正多边形顶点（pointUp 控制顶点朝上）。</summary>
        private static PointF[] Poly(int n, float r, float rotDeg, float cx = 8f, float cy = 8f)
        {
            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                double ang = (rotDeg + i * 360f / n) * Math.PI / 180.0;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * r, cy + (float)Math.Sin(ang) * r);
            }
            return pts;
        }

        /// <summary>多角星顶点（内外半径交替）。</summary>
        private static PointF[] Star(int n, float rOut, float rIn, float rotDeg = -90f, float cx = 8f, float cy = 8f)
        {
            var pts = new PointF[n * 2];
            for (int i = 0; i < n * 2; i++)
            {
                double ang = (rotDeg + i * 180f / n) * Math.PI / 180.0;
                float r = (i & 1) == 0 ? rOut : rIn;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * r, cy + (float)Math.Sin(ang) * r);
            }
            return pts;
        }

        // ============ 总调度（按枚举编号区段分派，编号与枚举分组注释严格对应） ============
        public static void Paint(HMenuGlyph id, Graphics g, Color k, Color a)
        {
            int v = (int)id;
            if (v >= 130 && v < 242)
            {
                if (v < 194) Arrow(g, k, a, v - 130);
                else if (v < 226) Chevron(g, k, a, v - 194);
                else Corner(g, k, a, v - 226);
            }
            else if (v < 346)
            {
                if (v < 314) Contained(g, k, a, v - 242);
                else if (v < 334) NumBadge(g, k, a, v - 314);
                else TriPlay(g, k, a, v - 334);
            }
            else if (v < 386)
            {
                if (v < 378) Shape(g, k, a, v - 346);
                else Moon(g, k, a, v - 378);
            }
            else PaintRest(v, g, k, a); // 386..539 手绘语义族
        }

        // ============ 箭头族（8 方向 × 8 样式） ============
        private static void Arrow(Graphics g, Color k, Color a, int q)
        {
            int dir = q % 8, style = q / 8;
            float ang = DirAngle(dir);
            Rot(g, ang, () =>
            {
                switch (style)
                {
                    case 0: // 普通
                        using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 3.2f, 8, 13.2f, 8);
                        break;
                    case 1: // 虚线
                        using (var p = new Pen(a, 1.5f) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 3.2f, 8, 13.2f, 8);
                        break;
                    case 2: // 细线
                        using (var p = new Pen(a, 1.05f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 2.8f, 8, 13.4f, 8);
                        break;
                    case 3: // 粗线
                        using (var p = new Pen(a, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 3.5f, 8, 12.8f, 8);
                        break;
                    case 4: // 双向
                        using (var p = new Pen(a, 1.6f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 2.6f, 8, 13.4f, 8);
                        break;
                    case 5: // 弯弧
                        using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                        { g.DrawArc(p, 3.5f, 2.5f, 10f, 10f, 135, 175); g.DrawArc(p, 2.5f, 3.5f, 10f, 10f, 220, 175); }
                        break;
                    case 6: // 圆圈 + 箭头
                        using (var p = new Pen(k, 1.2f)) g.DrawEllipse(p, 1.6f, 1.6f, 12.8f, 12.8f);
                        using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 4.5f, 11.5f, 12.6f, 3.4f);
                        break;
                    case 7: // 方框 + 箭头
                        using (var p = new Pen(k, 1.2f)) g.DrawRectangle(p, 2.1f, 2.1f, 11.8f, 11.8f);
                        using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                            g.DrawLine(p, 4.5f, 11.5f, 12.2f, 3.8f);
                        break;
                }
            });
        }

        // ============ 雪佛龙/指针族（8 方向 × 4 样式） ============
        private static void Chevron(Graphics g, Color k, Color a, int q)
        {
            int dir = q % 8, style = q / 8;
            float ang = DirAngle(dir);
            Rot(g, ang, () =>
            {
                if (style == 3)
                {
                    // 实底三角指针
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(3.5f, 3f), new PointF(13f, 8f), new PointF(3.5f, 13f) });
                    return;
                }
                int count = style == 0 ? 1 : style == 1 ? 2 : 3;
                using (var p = new Pen(a, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    for (int i = 0; i < count; i++)
                    {
                        float x = 4f + i * 3.6f;
                        g.DrawLines(p, new[] { new PointF(x, 3.6f), new PointF(x + 4f, 8f), new PointF(x, 12.4f) });
                    }
            });
        }

        // ============ 折角回转箭头族（4 角 × 4 样式） ============
        private static void Corner(Graphics g, Color k, Color a, int q)
        {
            int cn = q % 4, style = q / 4;
            float rot = cn * 90f; // UL → UR → LR → LL
            Rot(g, rot, () =>
            {
                void L(float inset)
                {
                    using (var p = new Pen(style == 1 ? a : k, style == 1 ? 2.2f : 1.5f)
                    { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                        g.DrawLines(p, new[] { new PointF(13f, 13f - inset), new PointF(5f, 13f - inset), new PointF(5f, 3.2f + inset) });
                }
                if (style == 2)
                {
                    using (var p = new Pen(a, 1.4f) { DashStyle = DashStyle.Dash, EndCap = LineCap.ArrowAnchor })
                        g.DrawLines(p, new[] { new PointF(13f, 13f), new PointF(5f, 13f), new PointF(5f, 3.2f) });
                }
                else if (style == 3) { L(0); L(2.6f); }
                else L(0);
            });
        }

        // ============ 容器符号族（12 符号 × 6 容器） ============
        private static void Contained(Graphics g, Color k, Color a, int q)
        {
            int sym = q % 12, box = q / 12;
            bool filled = box % 2 == 1;
            Color body = filled ? a : k;
            Color fg = filled ? ContrastOn(a) : a;
            // 容器：0/1 圆  2/3 方  4/5 圆角方
            RectangleF r = box < 2 ? new RectangleF(1.3f, 1.3f, 13.4f, 13.4f)
                        : new RectangleF(1.9f, 1.9f, 12.2f, 12.2f);
            if (filled)
            {
                using (var br = new SolidBrush(a))
                {
                    if (box < 2) g.FillEllipse(br, r);
                    else using (var path = RoundR(r, box >= 4 ? 3.2f : 0f)) g.FillPath(br, path);
                }
            }
            else
            {
                using (var p = new Pen(k, 1.3f))
                {
                    if (box < 2) g.DrawEllipse(p, r);
                    else using (var path = RoundR(r, box >= 4 ? 3.2f : 0f)) g.DrawPath(p, path);
                }
            }
            DrawSym(g, sym, fg);
        }

        /// <summary>圆角矩形路径。</summary>
        private static GraphicsPath RoundR(RectangleF r, float d)
        {
            var p = new GraphicsPath();
            if (d <= 0.1f) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        /// <summary>容器内 12 种符号（坐标中心 8,8，幅面约 8px）。</summary>
        private static void DrawSym(Graphics g, int sym, Color c)
        {
            switch (sym)
            {
                case 0: // 加
                    using (var p = Pen(c, 1.7f)) { g.DrawLine(p, 8, 4.6f, 8, 11.4f); g.DrawLine(p, 4.6f, 8, 11.4f, 8); }
                    break;
                case 1: // 减
                    using (var p = Pen(c, 1.7f)) g.DrawLine(p, 4.6f, 8, 11.4f, 8);
                    break;
                case 2: // 叉
                    using (var p = Pen(c, 1.6f)) { g.DrawLine(p, 5.2f, 5.2f, 10.8f, 10.8f); g.DrawLine(p, 10.8f, 5.2f, 5.2f, 10.8f); }
                    break;
                case 3: // 勾
                    using (var p = new Pen(c, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawLines(p, new[] { new PointF(4.8f, 8.2f), new PointF(7f, 10.6f), new PointF(11.4f, 5.4f) });
                    break;
                case 4: // 问号
                    using (var b = new SolidBrush(c))
                    using (var f = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Point))
                        Text(g, "?", f, b);
                    break;
                case 5: // 叹号
                    using (var p = Pen(c, 1.9f)) g.DrawLine(p, 8, 4.6f, 8, 9.6f);
                    using (var br = new SolidBrush(c)) g.FillEllipse(br, 7.35f, 10.7f, 1.3f, 1.3f);
                    break;
                case 6: // 信息 i
                    using (var p = Pen(c, 1.9f)) g.DrawLine(p, 8, 7.6f, 8, 11.2f);
                    using (var br = new SolidBrush(c)) { g.FillEllipse(br, 7.35f, 4.5f, 1.3f, 1.3f); g.FillEllipse(br, 7.35f, 10.75f, 1.3f, 1.3f); }
                    break;
                case 7: // 圆点
                    using (var br = new SolidBrush(c)) g.FillEllipse(br, 6.3f, 6.3f, 3.4f, 3.4f);
                    break;
                case 8: // 刷新环
                    using (var p = new Pen(c, 1.7f) { EndCap = LineCap.ArrowAnchor })
                    { g.DrawArc(p, 4.2f, 4.2f, 7.6f, 7.6f, 40, 280); }
                    break;
                case 9: // 房子
                    using (var p = Pen(c, 1.5f))
                    {
                        g.DrawLines(p, new[] { new PointF(4.4f, 8f), new PointF(8f, 4.6f), new PointF(11.6f, 8f) });
                        g.DrawRectangle(p, 5.4f, 7.8f, 5.2f, 3.8f);
                    }
                    break;
                case 10: FillStar(g, 5, c); break;
                case 11: Heart(g, c); break;
            }
        }

        /// <summary>居中绘制字符串。</summary>
        private static void Text(Graphics g, string s, Font f, Brush b, float y = 0.4f)
        {
            var sz = g.MeasureString(s, f);
            g.DrawString(s, f, b, 8f - sz.Width / 2f, 8f - sz.Height / 2f + y);
        }

        /// <summary>小五角星（实底）。</summary>
        private static void FillStar(Graphics g, int n, Color c, float rOut = 3.5f, float rIn = 1.5f)
        {
            using (var br = new SolidBrush(c)) g.FillPolygon(br, Star(n, rOut, rIn));
        }

        /// <summary>小爱心（实底）。</summary>
        private static void Heart(Graphics g, Color c)
        {
            using (var br = new SolidBrush(c))
            using (var path = new GraphicsPath())
            {
                path.AddBezier(4.6f, 5.4f, 4.2f, 4.2f, 5.6f, 4f, 6.6f, 5f);
                path.AddBezier(6.6f, 5f, 7.6f, 4f, 9.2f, 4.2f, 11.2f, 5.6f);
                path.AddBezier(11.2f, 5.6f, 12.6f, 7.2f, 10.4f, 9.6f, 8f, 11.6f);
                path.AddBezier(8f, 11.6f, 5.6f, 9.6f, 3.4f, 7.4f, 4.6f, 5.4f);
                path.CloseFigure();
                g.FillPath(br, path);
            }
        }

        // ============ 数字圆牌族（0..9 描边/实底） ============
        private static void NumBadge(Graphics g, Color k, Color a, int q)
        {
            bool fill = q >= 10;
            int n = fill ? q - 10 : q;
            if (fill) using (var br = new SolidBrush(a)) g.FillEllipse(br, 1.3f, 1.3f, 13.4f, 13.4f);
            using (var p = new Pen(fill ? a : k, 1.3f)) g.DrawEllipse(p, 1.3f, 1.3f, 13.4f, 13.4f);
            using (var b = new SolidBrush(fill ? ContrastOn(a) : a))
            using (var f = new Font("Segoe UI", 8f, FontStyle.Bold, GraphicsUnit.Point))
                Text(g, n.ToString(), f, b, 0.3f);
        }

        // ============ 三角播放族（4 方向 × 3 样式） ============
        private static void TriPlay(Graphics g, Color k, Color a, int q)
        {
            int dir = q % 4, style = q / 4; // R,D,L,U
            Rot(g, dir * 90f, () =>
            {
                var tri = new[] { new PointF(4.5f, 3f), new PointF(12.5f, 8f), new PointF(4.5f, 13f) };
                if (style == 0) using (var p = new Pen(a, 1.6f) { LineJoin = LineJoin.Round }) g.DrawPolygon(p, tri);
                else if (style == 1) using (var br = new SolidBrush(a)) g.FillPolygon(br, tri);
                else
                {
                    using (var br = new SolidBrush(a)) g.FillPolygon(br, tri);
                    using (var p = new Pen(k, 1.6f)) g.DrawLine(p, 3.2f, 3f, 3.2f, 13f);
                }
            });
        }

        // ============ 多边形族（8 形状 × 4 样式） ============
        private static void Shape(Graphics g, Color k, Color a, int q)
        {
            int shp = q % 8, style = q / 8;
            PointF[] pts;
            switch (shp)
            {
                case 0: // 圆单独走椭圆
                    if (style == 1) using (var br = new SolidBrush(a)) g.FillEllipse(br, 2f, 2f, 12f, 12f);
                    else if (style == 2) using (var p = new Pen(a, 2.8f)) g.DrawEllipse(p, 2f, 2f, 12f, 12f);
                    else if (style == 3) { using (var p = new Pen(a, 1.5f)) { g.DrawEllipse(p, 2f, 2f, 12f, 12f); g.DrawEllipse(p, 4.4f, 4.4f, 7.2f, 7.2f); } }
                    else using (var p = new Pen(k, 1.5f)) g.DrawEllipse(p, 2f, 2f, 12f, 12f);
                    return;
                case 1: pts = Poly(4, 6f, -45f); break;
                case 2: pts = Poly(4, 6.2f, 0f); break;
                case 3: pts = Poly(3, 6.4f, -90f, 8f, 8.6f); break;
                case 4: pts = Poly(5, 6.2f, -90f); break;
                case 5: pts = Poly(6, 6.2f, -90f); break;
                case 6: pts = Star(5, 6.6f, 2.8f); break;
                default: pts = Star(6, 6.4f, 3.4f, -90f); break;
            }
            if (style == 1) using (var br = new SolidBrush(a)) g.FillPolygon(br, pts);
            else if (style == 2) using (var p = new Pen(a, 2.6f) { LineJoin = LineJoin.Round }) g.DrawPolygon(p, pts);
            else if (style == 3)
            {
                var sml = Scale(pts, 0.62f);
                using (var p = new Pen(a, 1.5f) { LineJoin = LineJoin.Round }) { g.DrawPolygon(p, pts); g.DrawPolygon(p, sml); }
            }
            else using (var p = new Pen(k, 1.5f) { LineJoin = LineJoin.Round }) g.DrawPolygon(p, pts);
        }

        /// <summary>点列绕中心缩放。</summary>
        private static PointF[] Scale(PointF[] pts, float f)
        {
            var r = new PointF[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                r[i] = new PointF(8f + (pts[i].X - 8f) * f, 8f + (pts[i].Y - 8f) * f);
            return r;
        }

        // ============ 月相族 8 ============
        // 亮区路径：亮侧半圆弧（盘边）+ 明暗界（直线=弦月 / 凸入亮侧=娥眉 / 凹进暗侧=凸月）
        private static void Moon(Graphics g, Color k, Color a, int phase)
        {
            const float x0 = 2.2f, y0 = 2.2f, d = 11.6f, mid = 8f;
            if (phase != 0)
            {
                bool rightLit = phase >= 1 && phase <= 3;
                using (var path = new GraphicsPath())
                {
                    if (phase == 4) path.AddEllipse(x0, y0, d, d);                    // 满月
                    else
                    {
                        float term = phase == 2 || phase == 6 ? 0f : 2.3f;           // 分界椭圆半宽
                        bool gibb = phase == 3 || phase == 5;
                        // 亮侧盘边：右亮用右半圆（-90→90），左亮镜像
                        path.AddArc(x0, y0, d, d, rightLit ? -90f : 90f, 180f);
                        if (term <= 0.01f)
                            path.AddLine(rightLit ? mid : mid, y0, rightLit ? mid : mid, y0 + d);
                        else
                        {
                            // 娥眉月界弧凸向亮侧、凸月凹向暗侧：用分界椭圆的左/右半圆闭合
                            float tx = mid - term, tw = term * 2f;
                            bool termRight = rightLit ^ gibb;                        // 娥眉在亮侧、凸月在暗侧
                            path.AddArc(tx, y0, tw, d, termRight ? 90f : -90f, 180f);
                        }
                        path.CloseFigure();
                    }
                    using (var br = new SolidBrush(a)) g.FillPath(br, path);
                }
            }
            using (var p = new Pen(k, 1.3f)) g.DrawEllipse(p, x0, y0, d, d);
        }
    }
}
