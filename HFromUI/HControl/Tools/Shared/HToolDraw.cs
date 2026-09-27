using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace HFromUI.HControl.Tools.Shared
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 工业图元共享绘制原语（Tools 内部使用）：圆角盒、圆柱、风轮、电机、法兰支腿等，
    /// 供各设备控件的 22 种 Style 绘制方法复用，保持外形体系统一、代码精简。
    /// </summary>
    internal static class HToolDraw
    {
        /// <summary>圆角矩形填充。</summary>
        public static void FillR(this Graphics g, RectangleF r, float rad, Color c)
        {
            using (var gp = HBarBase.RoundPath(r, rad))
            using (var b = new SolidBrush(c))
                g.FillPath(b, gp);
        }
        /// <summary>圆角矩形描边。</summary>
        public static void DrawR(this Graphics g, RectangleF r, float rad, Color c, float w = 1.4f)
        {
            using (var gp = HBarBase.RoundPath(r, rad))
            using (var p = new Pen(c, w))
                g.DrawPath(p, gp);
        }
        /// <summary>圆角矩形填充 + 描边。</summary>
        public static void Box(this Graphics g, RectangleF r, float rad, Color fill, Color edge, float w = 1.4f)
        {
            using (var gp = HBarBase.RoundPath(r, rad))
            {
                using (var b = new SolidBrush(fill)) g.FillPath(b, gp);
                using (var p = new Pen(edge, w)) g.DrawPath(p, gp);
            }
        }
        /// <summary>水平圆柱渐变填充。</summary>
        public static void FillCylH(this Graphics g, RectangleF r, Color edge, Color center)
        {
            using (var b = HBarBase.CylinderH(r, edge, center))
                g.FillRectangle(b, r);
        }
        /// <summary>垂直三段渐变填充。</summary>
        public static void FillCylV(this Graphics g, RectangleF r, Color top, Color mid, Color bottom)
        {
            using (var b = HBarBase.CylinderV(r, top, mid, bottom))
                g.FillRectangle(b, r);
        }
        /// <summary>直线。</summary>
        public static void Line(this Graphics g, Color c, float w, float x1, float y1, float x2, float y2)
        {
            using (var p = new Pen(c, w))
                g.DrawLine(p, x1, y1, x2, y2);
        }
        /// <summary>多边形填充（可同时描边）。</summary>
        public static void Poly(this Graphics g, PointF[] pts, Color fill, Color edge, float w = 1.3f)
        {
            using (var b = new SolidBrush(fill))
                g.FillPolygon(b, pts);
            if (w > 0f)
                using (var p = new Pen(edge, w))
                    g.DrawPolygon(p, pts);
        }
        /// <summary>椭圆填充（半径非正直接跳过，杜绝退化矩形异常）。</summary>
        public static void Ell(this Graphics g, float cx, float cy, float rx, float ry, Color c)
        {
            if (rx <= 0f || ry <= 0f) return;
            using (var b = new SolidBrush(c))
                g.FillEllipse(b, cx - rx, cy - ry, rx * 2f, ry * 2f);
        }
        /// <summary>双坡屋顶。</summary>
        public static void GableRoof(this Graphics g, float x, float yTop, float w, float yEave, Color fill, Color edge)
        {
            g.Poly(new[]
            {
                new PointF(x - 8f, yEave), new PointF(x + w / 2f, yTop),
                new PointF(x + w + 8f, yEave)
            }, fill, edge, 1.4f);
        }
        /// <summary>支腿（单根）。</summary>
        public static void Leg(this Graphics g, float x, float y1, float y2, Color c, float w = 4f)
            => g.Line(c, w, x, y1, x, y2);
        /// <summary>螺栓小点。</summary>
        public static void Bolt(this Graphics g, float cx, float cy, float r, Color c)
            => g.Ell(cx, cy, r, r, c);
        /// <summary>
        /// 电机：横向圆柱机身 + 后端盖 + 散热筋，cx1/cx2 为轴向范围，cy 为中心，rh 为半径。
        /// 空间不足（窄/矮单元）时自动收缩并反向占位，保证任何尺寸下都不产生退化矩形。
        /// </summary>
        public static void Motor(this Graphics g, float x1, float x2, float cy, float rh, HToolPalette pal)
        {
            if (rh <= 0f) rh = 1f;
            float len = x2 - x1;
            // 给定轴向空间为负或过窄：以 x2 为右端反向布置一台最小电机
            if (len < rh * 2f)
            {
                len = Math.Max(rh * 2f, 6f);
                x1 = x2 - len;
            }
            // 长度不足直径时收缩半径
            rh = Math.Min(rh, len / 2f);
            var r = new RectangleF(x1, cy - rh, len, rh * 2f);
            using (var gp = HBarBase.RoundPath(r, rh * 0.9f))
            using (var b = HBarBase.CylinderH(r, HToolPalettes.Shade(pal.Main, 0.28f),
                HToolPalettes.Tint(pal.Main, 0.18f)))
            {
                g.FillPath(b, gp);
                using (var ep = new Pen(pal.Edge, 1.3f))
                    g.DrawPath(ep, gp);
            }
            using (var fin = new Pen(HToolPalettes.Shade(pal.Main, 0.12f), 1.3f))
                for (int i = 1; i < 5; i++)
                    g.DrawLine(fin, x1 + len * i / 5f, cy - rh + 3,
                        x1 + len * i / 5f, cy + rh - 3);
        }
        /// <summary>
        /// 风轮/叶轮：圆盘内 n 片弧形叶片 + 轴帽。angle 为旋转角，run 控制叶片深浅。
        /// </summary>
        public static void FanWheel(this Graphics g, float cx, float cy, float r, int n,
            float angle, bool run, HToolPalette pal)
        {
            using (var plate = new SolidBrush(Color.FromArgb(238, 240, 242)))
                g.FillEllipse(plate, cx - r, cy - r, r * 2f, r * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r, cy - r, r * 2f, r * 2f);
            float ri = r * 0.26f, ro = r * 0.9f;
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            using (var blade = new SolidBrush(run ? Color.FromArgb(74, 80, 88) : Color.FromArgb(150, 154, 160)))
            {
                for (int i = 0; i < n; i++)
                {
                    double a0 = i * Math.PI * 2.0 / n;
                    g.FillPolygon(blade, new[]
                    {
                        new PointF((float)Math.Cos(a0 - 0.18) * ri, (float)Math.Sin(a0 - 0.18) * ri),
                        new PointF((float)Math.Cos(a0 + 0.16) * ro, (float)Math.Sin(a0 + 0.16) * ro),
                        new PointF((float)Math.Cos(a0 + 0.62) * ro, (float)Math.Sin(a0 + 0.62) * ro),
                        new PointF((float)Math.Cos(a0 + 0.30) * ri, (float)Math.Sin(a0 + 0.30) * ri)
                    });
                }
            }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.16f, r * 0.16f, pal.Accent);
        }
        /// <summary>方形法兰盘 + 两螺栓。</summary>
        public static void Flange(this Graphics g, RectangleF r, HToolPalette pal)
        {
            g.Box(r, 2f, pal.Metal, pal.Edge, 1.2f);
            g.Bolt(r.Left + r.Width / 2f, r.Top + 3f, 2f, pal.Edge);
            g.Bolt(r.Left + r.Width / 2f, r.Bottom - 3f, 2f, pal.Edge);
        }
        /// <summary>底座（梯形机座）。</summary>
        public static void BasePlate(this Graphics g, float cx, float w, float y, float h, Color c)
        {
            g.Poly(new[]
            {
                new PointF(cx - w / 2f, y), new PointF(cx + w / 2f, y),
                new PointF(cx + w * 0.42f, y + h), new PointF(cx - w * 0.42f, y + h)
            }, c, c, 0f);
        }
        /// <summary>运行状态指示灯。</summary>
        public static void Lamp(this Graphics g, float cx, float cy, bool run)
            => g.Ell(cx, cy, 3.6f, 3.6f, run ? Color.FromArgb(80, 210, 120) : Color.FromArgb(210, 60, 55));
    }
}
