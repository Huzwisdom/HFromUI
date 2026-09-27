using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>
    /// HMI 仪表族共享 GDI+ 原语：圆角弧段、径向表盘、金属外圈、环向刻度、
    /// 指针多边形、中心轴帽、LCD 数字窗。供 HGauge/HDialPlate/HThermometer 等复用，
    /// 角度统一约定：以 12 点方向为 0°，顺时针为正。
    /// </summary>
    internal static class HHmiDraw
    {
        /// <summary>按像素大小创建字体（控件布局全部按物理像素计算，避免系统 DPI 缩放导致文字错位）。</summary>
        public static Font Pf(FontFamily family, float px, FontStyle style = FontStyle.Regular)
        {
            return new Font(family, px, style, GraphicsUnit.Pixel);
        }

        /// <summary>按像素大小创建字体。</summary>
        public static Font Pf(string family, float px, FontStyle style = FontStyle.Regular)
        {
            return new Font(family, px, style, GraphicsUnit.Pixel);
        }
        /// <summary>圆角弧段（粗弧）。clockFrom 为 12 点制起始角，clockSweep 顺时针扫角。</summary>
        public static void Arc(this Graphics g, float cx, float cy, float r, float width,
            Color c, float clockFrom, float clockSweep)
        {
            if (r <= 0.5f || width <= 0.5f || Math.Abs(clockSweep) < 0.01f) return;
            var rect = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            using (var p = new Pen(c, width))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                p.Alignment = PenAlignment.Center;
                g.DrawArc(p, rect, clockFrom - 90f, clockSweep);
            }
        }

        /// <summary>直角平头弧段（用于分段光柱）。</summary>
        public static void ArcFlat(this Graphics g, float cx, float cy, float r, float width,
            Color c, float clockFrom, float clockSweep)
        {
            if (r <= 0.5f || width <= 0.5f || Math.Abs(clockSweep) < 0.01f) return;
            var rect = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            using (var p = new Pen(c, width))
            {
                p.StartCap = LineCap.Flat;
                p.EndCap = LineCap.Flat;
                g.DrawArc(p, rect, clockFrom - 90f, clockSweep);
            }
        }

        /// <summary>圆周取点（12 点制）。</summary>
        public static PointF Polar(float cx, float cy, float r, float clockAngle)
        {
            double a = (clockAngle - 90d) * Math.PI / 180d;
            return new PointF(cx + r * (float)Math.Cos(a), cy + r * (float)Math.Sin(a));
        }

        /// <summary>径向渐变圆盘（中心 face → 边缘 edge）。</summary>
        public static void FaceDisk(this Graphics g, float cx, float cy, float r, Color face, Color edge)
        {
            if (r <= 0.5f) return;
            var rect = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(rect);
                using (var pgb = new PathGradientBrush(gp))
                {
                    pgb.CenterPoint = new PointF(cx - r * 0.18f, cy - r * 0.22f);
                    pgb.CenterColor = face;
                    pgb.SurroundColors = new[] { edge };
                    g.FillPath(pgb, gp);
                }
            }
        }

        /// <summary>金属外圈环：对角线性渐变填充的环带，内缘描细边。</summary>
        public static void BezelRing(this Graphics g, float cx, float cy, float rOuter, float width,
            Color light, Color dark, Color innerEdge)
        {
            if (width <= 0.5f || rOuter <= width) return;
            float rInner = rOuter - width;
            var outerR = new RectangleF(cx - rOuter, cy - rOuter, rOuter * 2f, rOuter * 2f);
            var innerR = new RectangleF(cx - rInner, cy - rInner, rInner * 2f, rInner * 2f);
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(outerR);
                gp.AddEllipse(innerR);
                using (var lgb = new LinearGradientBrush(
                    new PointF(cx - rOuter, cy - rOuter), new PointF(cx + rOuter, cy + rOuter),
                    light, dark))
                using (var b = new SolidBrush(dark))
                    g.FillPath(lgb, gp);
            }
            using (var p = new Pen(innerEdge, 1.2f))
                g.DrawEllipse(p, innerR);
        }

        /// <summary>圆内斜纹/网格纹理覆盖（碳纤维等），alpha 0..255。</summary>
        public static void FaceHatch(this Graphics g, float cx, float cy, float r, HatchStyle style, int alpha)
        {
            if (r <= 0.5f) return;
            var rect = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(rect);
                using (var hb = new HatchBrush(style, Color.FromArgb(alpha, 255, 255, 255), Color.Transparent))
                    g.FillPath(hb, gp);
            }
        }

        /// <summary>
        /// 环向刻度。from/sweep 为 12 点制角度；majorCount 主格数，每主格内 minorEach 个小格；
        /// rOut 为刻度外端半径。返回主刻度内端半径（供标签定位参考）。
        /// </summary>
        public static float Ticks(this Graphics g, float cx, float cy, float rOut,
            float from, float sweep, int majorCount, int minorEach,
            float majorLen, float minorLen, Color majorColor, Color minorColor,
            float majorW = 2f, float minorW = 1f)
        {
            int total = Math.Max(1, majorCount) * Math.Max(0, minorEach);
            using (var pm = new Pen(majorColor, majorW))
            using (var pn = new Pen(minorColor, minorW))
            {
                for (int i = 0; i <= total; i++)
                {
                    float t = (float)i / total;
                    float a = from + sweep * t;
                    bool major = minorEach == 0 || i % minorEach == 0;
                    float len = major ? majorLen : minorLen;
                    var p1 = Polar(cx, cy, rOut, a);
                    var p2 = Polar(cx, cy, rOut - len, a);
                    g.DrawLine(major ? pm : pn, p1, p2);
                }
            }
            return rOut - majorLen;
        }

        /// <summary>
        /// 环向主刻度标签（labelAt 由调用方按段号提供文本，null 段不画）。
        /// chip 非 Empty 时先在文字下垫同色圆角底板，使扫过的指针被底板遮住，文字永不压线。
        /// </summary>
        public static void TickLabels(this Graphics g, float cx, float cy, float r,
            float from, float sweep, int majorCount, Func<int, string> labelAt,
            Font font, Color color, Color chip)
        {
            if (font == null) return;
            bool hasChip = chip != Color.Empty;
            using (var b = new SolidBrush(color))
            using (var cb = hasChip ? new SolidBrush(chip) : null)
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i <= majorCount; i++)
                {
                    string s = labelAt == null ? null : labelAt(i);
                    if (string.IsNullOrEmpty(s)) continue;
                    float a = from + sweep * i / (float)Math.Max(1, majorCount);
                    var p = Polar(cx, cy, r, a);
                    var box = new RectangleF(p.X - r * 0.5f, p.Y - font.Height * 0.9f, r, font.Height * 1.8f);
                    box.X = p.X - box.Width / 2f;
                    if (hasChip)
                    {
                        SizeF sz = g.MeasureString(s, font);
                        var cbRect = new RectangleF(p.X - sz.Width / 2f - 3f,
                            p.Y - sz.Height / 2f - 1.5f, sz.Width + 6f, sz.Height + 3f);
                        using (var gp = RoundBox(cbRect, 3f))
                            g.FillPath(cb, gp);
                    }
                    g.DrawString(s, font, b, box, sf);
                }
            }
        }

        /// <summary>环向主刻度标签（无底版）。</summary>
        public static void TickLabels(this Graphics g, float cx, float cy, float r,
            float from, float sweep, int majorCount, Func<int, string> labelAt,
            Font font, Color color)
        {
            g.TickLabels(cx, cy, r, from, sweep, majorCount, labelAt, font, color, Color.Empty);
        }

        /// <summary>
        /// 指针：以表盘中心为原点本地坐标系（向上为 -Y）绘制后按 angle 顺时针旋转。
        /// kind：0 矛形针、1 细长针带配重尾、2 三角针、3 扁条针、4 宽剑针。
        /// </summary>
        public static void Needle(this Graphics g, float cx, float cy, float angle,
            float len, int kind, Color color)
        {
            if (len <= 0.5f) return;
            float w = Math.Max(1.6f, len * 0.022f);
            PointF[] pts;
            switch (kind)
            {
                case 1: // 细长针 + 配重尾
                    pts = new[]
                    {
                        new PointF(0, -len), new PointF(w * 0.7f, -len * 0.15f),
                        new PointF(w * 1.6f, len * 0.16f), new PointF(0, len * 0.26f),
                        new PointF(-w * 1.6f, len * 0.16f), new PointF(-w * 0.7f, -len * 0.15f)
                    };
                    break;
                case 2: // 三角针
                    pts = new[]
                    {
                        new PointF(0, -len), new PointF(w * 1.3f, len * 0.08f),
                        new PointF(-w * 1.3f, len * 0.08f)
                    };
                    break;
                case 3: // 扁条针
                    pts = new[]
                    {
                        new PointF(-w * 0.55f, -len), new PointF(w * 0.55f, -len),
                        new PointF(w * 0.55f, len * 0.14f), new PointF(-w * 0.55f, len * 0.14f)
                    };
                    break;
                case 4: // 宽剑针
                    pts = new[]
                    {
                        new PointF(0, -len), new PointF(w * 1.5f, -len * 0.12f),
                        new PointF(w * 0.9f, len * 0.10f), new PointF(0, len * 0.30f),
                        new PointF(-w * 0.9f, len * 0.10f), new PointF(-w * 1.5f, -len * 0.12f)
                    };
                    break;
                default: // 0 矛形针
                    pts = new[]
                    {
                        new PointF(0, -len), new PointF(w, -len * 0.16f),
                        new PointF(w * 0.62f, len * 0.12f), new PointF(0, len * 0.22f),
                        new PointF(-w * 0.62f, len * 0.12f), new PointF(-w, -len * 0.16f)
                    };
                    break;
            }
            var state = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            using (var b = new SolidBrush(color))
                g.FillPolygon(b, pts, FillMode.Winding);
            using (var p = new Pen(HMixer(color, Color.Black, 0.35f), 0.8f))
                g.DrawPolygon(p, pts);
            g.Restore(state);
        }

        /// <summary>中心轴帽：0 金属铆钉、1 双环帽、2 十字螺栓、3 圆芯点。</summary>
        public static void Hub(this Graphics g, float cx, float cy, float r, int kind, Color hi, Color lo)
        {
            if (r <= 0.5f) return;
            switch (kind)
            {
                case 1:
                    g.FaceDisk(cx, cy, r, lo, hi);
                    using (var p = new Pen(hi, r * 0.18f))
                        g.DrawEllipse(p, cx - r * 0.55f, cy - r * 0.55f, r * 1.1f, r * 1.1f);
                    g.Ell(cx, cy, r * 0.26f, r * 0.26f, hi);
                    break;
                case 2:
                    g.FaceDisk(cx, cy, r, hi, lo);
                    using (var p = new Pen(lo, r * 0.16f))
                    {
                        g.DrawLine(p, cx - r * 0.55f, cy, cx + r * 0.55f, cy);
                        g.DrawLine(p, cx, cy - r * 0.55f, cx, cy + r * 0.55f);
                    }
                    break;
                case 3:
                    g.Ell(cx, cy, r, r, hi);
                    break;
                default:
                    g.FaceDisk(cx, cy, r, hi, lo);
                    g.Ell(cx - r * 0.12f, cy - r * 0.12f, r * 0.30f, r * 0.30f,
                        Color.FromArgb(120, 255, 255, 255));
                    break;
            }
        }

        /// <summary>LCD 数字窗：圆角深色底 + 等宽高亮数字。</summary>
        public static void Lcd(this Graphics g, RectangleF box, string text, Font font,
            Color back, Color fore, Color edge)
        {
            float rad = Math.Min(6f, box.Height * 0.22f);
            using (var gp = RoundBox(box, rad))
            {
                using (var b = new SolidBrush(back)) g.FillPath(b, gp);
                using (var p = new Pen(edge, 1.2f)) g.DrawPath(p, gp);
            }
            using (var b = new SolidBrush(fore))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(text, font, b, box, sf);
        }

        /// <summary>圆角矩形路径。</summary>
        public static GraphicsPath RoundBox(RectangleF r, float rad)
        {
            var gp = new GraphicsPath();
            if (rad <= 0f || r.Width <= 0f || r.Height <= 0f)
            {
                gp.AddRectangle(r);
                return gp;
            }
            rad = Math.Min(rad, Math.Min(r.Width, r.Height) / 2f);
            float d = rad * 2f;
            gp.AddArc(r.X, r.Y, d, d, 180, 90);
            gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            gp.CloseFigure();
            return gp;
        }

        /// <summary>居中绘制字符串。</summary>
        public static void CenterText(this Graphics g, RectangleF box, string s, Font font, Color c)
        {
            if (string.IsNullOrEmpty(s) || font == null) return;
            using (var b = new SolidBrush(c))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(s, font, b, box, sf);
        }

        /// <summary>实心圆（半径非正跳过）。</summary>
        public static void Ell(this Graphics g, float cx, float cy, float rx, float ry, Color c)
        {
            if (rx <= 0f || ry <= 0f) return;
            using (var b = new SolidBrush(c))
                g.FillEllipse(b, cx - rx, cy - ry, rx * 2f, ry * 2f);
        }

        /// <summary>铆钉（外圈金属小点）。</summary>
        public static void Rivet(this Graphics g, float cx, float cy, float r, Color hi, Color lo)
        {
            g.FaceDisk(cx, cy, r, hi, lo);
            g.Ell(cx - r * 0.3f, cy - r * 0.3f, r * 0.35f, r * 0.35f, Color.FromArgb(140, 255, 255, 255));
        }

        /// <summary>颜色加深/混合（转发统一调色板）。</summary>
        public static Color HMixer(Color a, Color b, float t) => HToolPalettes.Mix(a, b, t);
    }
}
