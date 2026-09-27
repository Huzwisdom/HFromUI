using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Tools.UiKit
{
    /// <summary>
    /// HFrom 控件库内置矢量图标种类（24x24 网格线性图标，GDI+ 绘制，任意缩放不模糊）。
    /// </summary>
    public enum HGlyphKind
    {
        /// <summary>仪表盘（首页/总览）</summary>
        Dashboard = 0,
        /// <summary>包装箱（设备管理）</summary>
        Box = 1,
        /// <summary>层叠（协议服务）</summary>
        Layers = 2,
        /// <summary>图片场景（仿真场景）</summary>
        Scene = 3,
        /// <summary>节点编排（场景编排）</summary>
        Flow = 4,
        /// <summary>四宫格（模板市场）</summary>
        Grid = 5,
        /// <summary>勾选框（仿真测试）</summary>
        CheckSquare = 6,
        /// <summary>文档日志（实时日志/调试日志）</summary>
        Log = 7,
        /// <summary>双向箭头（联调集成/数据转发）</summary>
        Transfer = 8,
        /// <summary>齿轮（系统设置）</summary>
        Gear = 9,
        /// <summary>盾牌（审计日志/安全）</summary>
        Shield = 10,
        /// <summary>下载（备份恢复）</summary>
        Download = 11,
        /// <summary>CPU 芯片（PLC/工控）</summary>
        Chip = 12,
        /// <summary>网络节点（OPC/联网协议）</summary>
        Network = 13,
        /// <summary>数据库（MQTT/存储）</summary>
        Database = 14,
        /// <summary>服务器（服务端）</summary>
        Server = 15,
        /// <summary>挂锁（门锁/加密）</summary>
        Lock = 16,
        /// <summary>放大镜（搜索）</summary>
        Search = 17,
        /// <summary>用户（登录/账户）</summary>
        User = 18,
        /// <summary>播放（启动）</summary>
        Play = 19,
        /// <summary>停止方块</summary>
        Stop = 20,
        /// <summary>加号（放大）</summary>
        Plus = 21,
        /// <summary>减号（缩小）</summary>
        Minus = 22,
        /// <summary>四角全屏（适配窗口）</summary>
        Fit = 23,
        /// <summary>视频监控（GB28181/视频）</summary>
        Video = 24,
        /// <summary>仪表（BACnet/工业测量）</summary>
        Gauge = 25,
        /// <summary>全球（HTTP REST/多语言）</summary>
        Globe = 26,
        /// <summary>三个档案夹（白色，归档/存档树节点）</summary>
        Binders = 27,
        /// <summary>秒表（定时器，红指针）</summary>
        Stopwatch = 28,
        /// <summary>竖尺（根节点测量图标）</summary>
        Ruler = 29,
        /// <summary>三个档案夹（蓝色，变量管理菜单）</summary>
        BindersBlue = 30,
        /// <summary>黄色信封 + 蓝色铅笔（报警记录）</summary>
        EnvelopePencil = 31,
        /// <summary>三个档案夹（橙色，变量记录菜单）</summary>
        BindersOrange = 32,
        /// <summary>蓝色表格（文本库）</summary>
        Table = 33,
        /// <summary>三人群组（用户管理器）</summary>
        UsersGroup = 34,
        /// <summary>紫色喇叭（报警器）</summary>
        Speaker = 35,
        /// <summary>画面拓扑树（画面树）</summary>
        ScreenTree = 36
    }

    /// <summary>
    /// HFrom 矢量图标 + 绘制工具：内置 27 种线性图标，颜色/大小随控件主题，
    /// 无需任何图片资源文件。所有图标基于 24x24 虚拟网格绘制。
    /// </summary>
    public static class HGlyph
    {
        /// <summary>构造圆角矩形路径</summary>
        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            if (d < 1) d = 1;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>颜色与白色混合（amount=0 原色，1 全白）</summary>
        public static Color Lighten(Color c, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            int r = (int)(c.R + (255 - c.R) * amount);
            int g = (int)(c.G + (255 - c.G) * amount);
            int b = (int)(c.B + (255 - c.B) * amount);
            return Color.FromArgb(c.A, r, g, b);
        }

        /// <summary>颜色与黑色混合（amount=0 原色，1 全黑）</summary>
        public static Color Darken(Color c, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            int r = (int)(c.R * (1 - amount));
            int g = (int)(c.G * (1 - amount));
            int b = (int)(c.B * (1 - amount));
            return Color.FromArgb(c.A, r, g, b);
        }

        /// <summary>在指定矩形内居中绘制图标（自动缩放，保持正方形）</summary>
        public static void Draw(Graphics g, HGlyphKind kind, Rectangle bounds, Color color, float stroke = 1.9f)
        {
            int size = Math.Min(bounds.Width, bounds.Height);
            if (size <= 2) return;
            float scale = size / 24f;
            GraphicsState old = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TranslateTransform(bounds.X + (bounds.Width - size) / 2f, bounds.Y + (bounds.Height - size) / 2f);
            g.ScaleTransform(scale, scale);

            using (Pen pen = new Pen(color, stroke) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (Brush brush = new SolidBrush(color))
            {
                DrawKind(g, kind, pen, brush);
            }
            g.Restore(old);
        }

        /// <summary>
        /// 彩色 3D 风格图标（WinCC 经典导航器风格：档案夹/秒表/尺子/信封等），
        /// 内置配色不随主题变化。未实现彩色绘制的种类自动退化为蓝色线性图标。
        /// 基于 24x24 虚拟网格绘制。
        /// </summary>
        public static void DrawColor(Graphics g, HGlyphKind kind, Rectangle bounds)
        {
            int size = Math.Min(bounds.Width, bounds.Height);
            if (size <= 2) return;
            float scale = size / 24f;
            GraphicsState old = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TranslateTransform(bounds.X + (bounds.Width - size) / 2f, bounds.Y + (bounds.Height - size) / 2f);
            g.ScaleTransform(scale, scale);

            bool handled = DrawColorKind(g, kind);
            g.Restore(old);
            if (!handled)
                Draw(g, kind, bounds, Color.FromArgb(60, 105, 170), 1.8f);
        }

        /// <summary>DrawColorKind 方法。</summary>
        private static bool DrawColorKind(Graphics g, HGlyphKind kind)
        {
            switch (kind)
            {
                case HGlyphKind.Binders:
                    DrawBinders(g, Color.FromArgb(250, 251, 253), Color.FromArgb(126, 136, 156), Color.FromArgb(52, 74, 122));
                    return true;
                case HGlyphKind.BindersBlue:
                    DrawBinders(g, Color.FromArgb(74, 122, 214), Color.FromArgb(42, 82, 172), Color.White);
                    return true;
                case HGlyphKind.BindersOrange:
                    DrawBinders(g, Color.FromArgb(255, 178, 56), Color.FromArgb(216, 132, 22), Color.White);
                    return true;
                case HGlyphKind.Stopwatch:
                    DrawStopwatch(g);
                    return true;
                case HGlyphKind.Ruler:
                    DrawRuler(g);
                    return true;
                case HGlyphKind.EnvelopePencil:
                    DrawEnvelopePencil(g);
                    return true;
                case HGlyphKind.Table:
                    DrawTable(g);
                    return true;
                case HGlyphKind.UsersGroup:
                    DrawUsersGroup(g);
                    return true;
                case HGlyphKind.Speaker:
                    DrawSpeaker(g);
                    return true;
                case HGlyphKind.ScreenTree:
                    DrawScreenTree(g);
                    return true;
            }
            return false;
        }

        /// <summary>三个并排的档案夹（环装装订盒）</summary>
        private static void DrawBinders(Graphics g, Color body, Color edge, Color dot)
        {
            using (SolidBrush bb = new SolidBrush(body))
            using (Pen ep = new Pen(edge, 1f))
            using (SolidBrush db = new SolidBrush(dot))
            using (SolidBrush shade = new SolidBrush(Color.FromArgb(60, edge)))
            {
                float[] xs = { 3.6f, 9.8f, 16f };
                foreach (float x in xs)
                {
                    RectangleF binder = new RectangleF(x, 4.6f, 4.6f, 15.2f);
                    using (GraphicsPath p = RoundedRect(Rectangle.Round(binder), 1))
                    {
                        g.FillPath(bb, p);
                        g.DrawPath(ep, p);
                    }
                    // 左侧书脊阴影条
                    g.FillRectangle(shade, x + 0.5f, 5.6f, 1f, 13.2f);
                    // 底部 3D 阴影
                    g.FillRectangle(shade, x + 0.8f, 17.6f, 3f, 1.6f);
                    // 装订环孔（两个圆点）
                    g.FillEllipse(db, x + 1.55f, 8.2f, 1.5f, 1.5f);
                    g.FillEllipse(db, x + 1.55f, 11.6f, 1.5f, 1.5f);
                }
            }
        }

        /// <summary>秒表：白底灰边、红色指针、顶部按钮</summary>
        private static void DrawStopwatch(Graphics g)
        {
            using (Pen rim = new Pen(Color.FromArgb(120, 126, 136), 1.3f))
            using (SolidBrush face = new SolidBrush(Color.White))
            using (SolidBrush metal = new SolidBrush(Color.FromArgb(150, 156, 166)))
            using (Pen hand = new Pen(Color.FromArgb(214, 48, 48), 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (SolidBrush red = new SolidBrush(Color.FromArgb(214, 48, 48)))
            {
                // 表冠与侧钮
                using (GraphicsPath crown = RoundedRect(Rectangle.Round(new RectangleF(10.4f, 2.6f, 3.2f, 2.8f)), 1))
                    g.FillPath(metal, crown);
                g.DrawLine(new Pen(Color.FromArgb(150, 156, 166), 1.6f) { StartCap = LineCap.Round }, 17.6f, 8.6f, 19.2f, 7.2f);
                // 表盘
                g.FillEllipse(face, 4.6f, 6.8f, 14.8f, 14.8f);
                g.DrawEllipse(rim, 4.6f, 6.8f, 14.8f, 14.8f);
                // 刻度
                using (Pen tick = new Pen(Color.FromArgb(150, 156, 166), 1f))
                {
                    for (int i = 0; i < 12; i++)
                    {
                        double a = i * Math.PI / 6.0;
                        float x1 = (float)(12 + Math.Cos(a) * 6.0);
                        float y1 = (float)(14.2 + Math.Sin(a) * 6.0);
                        float x2 = (float)(12 + Math.Cos(a) * 6.8);
                        float y2 = (float)(14.2 + Math.Sin(a) * 6.8);
                        g.DrawLine(tick, x1, y1, x2, y2);
                    }
                }
                // 红指针 + 中心
                g.DrawLine(hand, 12f, 14.2f, 15.6f, 9.8f);
                g.FillEllipse(red, 11.1f, 13.3f, 1.8f, 1.8f);
            }
        }

        /// <summary>竖尺：深色尺身 + 白色刻度</summary>
        private static void DrawRuler(Graphics g)
        {
            RectangleF body = new RectangleF(9.2f, 2.6f, 5.6f, 18.8f);
            using (GraphicsPath p = RoundedRect(Rectangle.Round(body), 2))
            using (SolidBrush bb = new SolidBrush(Color.FromArgb(58, 64, 74)))
            {
                g.FillPath(bb, p);
                using (Pen ep = new Pen(Color.FromArgb(90, 96, 108), 1f))
                    g.DrawPath(ep, p);
            }
            using (Pen wp = new Pen(Color.White, 1.1f))
            {
                for (int i = 0; i <= 6; i++)
                {
                    float y = 5.4f + i * 2.5f;
                    float len = (i % 2 == 0) ? 2.6f : 1.6f;
                    g.DrawLine(wp, 9.6f, y, 9.6f + len, y);
                    g.DrawLine(wp, 14.4f, y, 14.4f - len, y);
                }
            }
        }

        /// <summary>黄色信封 + 蓝色铅笔（报警记录）</summary>
        private static void DrawEnvelopePencil(Graphics g)
        {
            // 信封
            RectangleF env = new RectangleF(2.6f, 9.2f, 17.8f, 11.8f);
            using (GraphicsPath ep2 = RoundedRect(Rectangle.Round(env), 2))
            using (SolidBrush eb = new SolidBrush(Color.FromArgb(255, 214, 64)))
            using (Pen epen = new Pen(Color.FromArgb(206, 150, 20), 1.1f))
            {
                g.FillPath(eb, ep2);
                g.DrawPath(epen, ep2);
            }
            // 封盖折线
            using (Pen flap = new Pen(Color.FromArgb(196, 138, 18), 1.2f))
            {
                g.DrawLine(flap, 3.4f, 10.2f, 11.5f, 15.8f);
                g.DrawLine(flap, 11.5f, 15.8f, 19.6f, 10.2f);
            }
            // 铅笔（斜放右上角）：橡皮头 + 蓝笔杆 + 深色笔尖
            using (Pen shaft = new Pen(Color.FromArgb(52, 96, 204), 2.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(shaft, 14.6f, 4.4f, 19.4f, 9.2f);
            using (SolidBrush tip = new SolidBrush(Color.FromArgb(70, 56, 40)))
                g.FillPolygon(tip, P(19.4f, 9.2f, 21f, 10.8f, 19.6f, 11.2f));
            using (SolidBrush eraser = new SolidBrush(Color.FromArgb(232, 96, 110)))
                g.FillEllipse(eraser, 13.2f, 3.2f, 2.4f, 2.4f);
        }

        /// <summary>蓝色表格（文本库）</summary>
        private static void DrawTable(Graphics g)
        {
            RectangleF tbl = new RectangleF(3.2f, 3.6f, 17.6f, 16.8f);
            using (SolidBrush bb = new SolidBrush(Color.FromArgb(52, 104, 204)))
                g.FillRectangle(bb, Rectangle.Round(tbl));
            using (Pen wp = new Pen(Color.White, 1.1f))
            {
                g.DrawRectangle(wp, 3.2f, 3.6f, 17.6f, 16.8f);
                g.DrawLine(wp, 9.4f, 3.6f, 9.4f, 20.4f);       // 表头竖线
                g.DrawLine(wp, 3.2f, 7.6f, 20.8f, 7.6f);       // 表头下线
                g.DrawLine(wp, 3.2f, 11.9f, 20.8f, 11.9f);
                g.DrawLine(wp, 3.2f, 16.2f, 20.8f, 16.2f);
            }
        }

        /// <summary>三人群组（用户管理器）</summary>
        private static void DrawUsersGroup(Graphics g)
        {
            using (SolidBrush back = new SolidBrush(Color.FromArgb(146, 178, 236)))
            using (SolidBrush front = new SolidBrush(Color.FromArgb(42, 92, 200)))
            {
                // 后面两人
                g.FillEllipse(back, 2.4f, 5.2f, 4.2f, 4.2f);
                g.FillEllipse(back, 17.4f, 5.2f, 4.2f, 4.2f);
                g.FillEllipse(back, 0.6f, 11.6f, 8f, 9.4f);
                g.FillEllipse(back, 15.4f, 11.6f, 8f, 9.4f);
                // 前面一人
                g.FillEllipse(front, 8.6f, 5.6f, 6.8f, 6.8f);
                g.FillEllipse(front, 4.8f, 12.8f, 14.4f, 10.2f);
            }
        }

        /// <summary>紫色喇叭 + 声波（报警器）</summary>
        private static void DrawSpeaker(Graphics g)
        {
            using (SolidBrush sb = new SolidBrush(Color.FromArgb(122, 78, 222)))
            using (Pen sp = new Pen(Color.FromArgb(122, 78, 222), 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.FillPolygon(sb, P(4f, 10f, 8f, 10f, 13.6f, 5.6f, 13.6f, 18.4f, 8f, 14f, 4f, 14f));
                g.DrawArc(sp, 14.6f, 8.4f, 5f, 7.2f, -55, 110);
                g.DrawArc(sp, 16.4f, 6.4f, 7.4f, 11.2f, -55, 110);
            }
        }

        /// <summary>画面拓扑树（画面树）</summary>
        private static void DrawScreenTree(Graphics g)
        {
            Color blue = Color.FromArgb(52, 104, 204);
            Color screen = Color.FromArgb(222, 235, 252);
            using (Pen bp = new Pen(blue, 1.3f))
            using (SolidBrush sb = new SolidBrush(screen))
            {
                // 顶部画面
                using (GraphicsPath top = RoundedRect(Rectangle.Round(new RectangleF(8.2f, 2.8f, 7.6f, 6f)), 1))
                {
                    g.FillPath(sb, top);
                    g.DrawPath(bp, top);
                }
                // 分支线
                g.DrawLine(bp, 12f, 8.8f, 12f, 12.2f);
                g.DrawLine(bp, 6f, 12.2f, 18f, 12.2f);
                g.DrawLine(bp, 6f, 12.2f, 6f, 13.8f);
                g.DrawLine(bp, 12f, 12.2f, 12f, 13.8f);
                g.DrawLine(bp, 18f, 12.2f, 18f, 13.8f);
                // 三个子画面
                foreach (float x in new[] { 3.4f, 9.4f, 15.4f })
                {
                    using (GraphicsPath node = RoundedRect(Rectangle.Round(new RectangleF(x, 13.8f, 5.2f, 4.6f)), 1))
                    {
                        g.FillPath(sb, node);
                        g.DrawPath(bp, node);
                    }
                }
            }
        }

        /// <summary>P 方法。</summary>
        private static PointF[] P(params float[] xy)
        {
            PointF[] pts = new PointF[xy.Length / 2];
            for (int i = 0; i < pts.Length; i++) pts[i] = new PointF(xy[i * 2], xy[i * 2 + 1]);
            return pts;
        }

        /// <summary>DrawKind 方法。</summary>
        private static void DrawKind(Graphics g, HGlyphKind kind, Pen pen, Brush brush)
        {
            switch (kind)
            {
                case HGlyphKind.Dashboard:
                    g.DrawArc(pen, 3.5f, 4.5f, 17f, 17f, 135, 270);
                    g.DrawLine(pen, 12, 13, 16.2f, 9.5f);
                    g.FillEllipse(brush, 10.9f, 11.9f, 2.2f, 2.2f);
                    break;
                case HGlyphKind.Box:
                    g.DrawPolygon(pen, P(12, 3, 20, 7, 20, 17, 12, 21, 4, 17, 4, 7));
                    g.DrawLine(pen, 4, 7, 12, 11);
                    g.DrawLine(pen, 20, 7, 12, 11);
                    g.DrawLine(pen, 12, 11, 12, 21);
                    break;
                case HGlyphKind.Layers:
                    g.DrawPolygon(pen, P(12, 3.5f, 20.5f, 9, 12, 14.5f, 3.5f, 9));
                    g.DrawLines(pen, P(3.5f, 13, 12, 18.5f, 20.5f, 13));
                    g.DrawLines(pen, P(3.5f, 17, 12, 22.5f, 20.5f, 17));
                    break;
                case HGlyphKind.Scene:
                    g.DrawRectangle(pen, 3.5f, 5f, 17f, 14f);
                    g.FillEllipse(brush, 8.2f, 9.2f, 2f, 2f);
                    g.DrawLines(pen, P(4, 17, 9.5f, 11.5f, 13.5f, 15.5f, 16.5f, 12.5f, 20.5f, 17));
                    break;
                case HGlyphKind.Flow:
                    g.FillEllipse(brush, 3.5f, 4.5f, 4.4f, 4.4f);
                    g.FillEllipse(brush, 16.1f, 4.5f, 4.4f, 4.4f);
                    g.FillEllipse(brush, 9.8f, 15.1f, 4.4f, 4.4f);
                    g.DrawLine(pen, 7.5f, 8.5f, 10.5f, 15.5f);
                    g.DrawLine(pen, 16.5f, 8.5f, 13.5f, 15.5f);
                    g.DrawLine(pen, 8f, 6.7f, 16f, 6.7f);
                    break;
                case HGlyphKind.Grid:
                    g.DrawRectangle(pen, 4f, 4f, 7f, 7f);
                    g.DrawRectangle(pen, 13f, 4f, 7f, 7f);
                    g.DrawRectangle(pen, 4f, 13f, 7f, 7f);
                    g.DrawRectangle(pen, 13f, 13f, 7f, 7f);
                    break;
                case HGlyphKind.CheckSquare:
                    g.DrawRectangle(pen, 3.5f, 3.5f, 17f, 17f);
                    g.DrawLines(pen, P(7.5f, 12.3f, 10.8f, 15.6f, 16.8f, 8.8f));
                    break;
                case HGlyphKind.Log:
                    g.DrawLines(pen, P(6.5f, 3.5f, 14.5f, 3.5f, 19.5f, 8.5f, 19.5f, 20.5f, 6.5f, 20.5f, 6.5f, 3.5f));
                    g.DrawLines(pen, P(14.5f, 3.5f, 14.5f, 8.5f, 19.5f, 8.5f));
                    g.DrawLine(pen, 9.5f, 12.5f, 16.5f, 12.5f);
                    g.DrawLine(pen, 9.5f, 15.5f, 16.5f, 15.5f);
                    g.DrawLine(pen, 9.5f, 18.5f, 13.5f, 18.5f);
                    break;
                case HGlyphKind.Transfer:
                    g.DrawLine(pen, 4f, 8.5f, 16.5f, 8.5f);
                    g.DrawLines(pen, P(13.5f, 5.5f, 16.5f, 8.5f, 13.5f, 11.5f));
                    g.DrawLine(pen, 20f, 15.5f, 7.5f, 15.5f);
                    g.DrawLines(pen, P(10.5f, 12.5f, 7.5f, 15.5f, 10.5f, 18.5f));
                    break;
                case HGlyphKind.Gear:
                    g.DrawEllipse(pen, 8.2f, 8.2f, 7.6f, 7.6f);
                    g.DrawEllipse(pen, 10.6f, 10.6f, 2.8f, 2.8f);
                    for (int i = 0; i < 8; i++)
                    {
                        double a = i * Math.PI / 4.0;
                        float x1 = (float)(12 + Math.Cos(a) * 5.0);
                        float y1 = (float)(12 + Math.Sin(a) * 5.0);
                        float x2 = (float)(12 + Math.Cos(a) * 6.6);
                        float y2 = (float)(12 + Math.Sin(a) * 6.6);
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                    break;
                case HGlyphKind.Shield:
                    g.DrawPolygon(pen, P(12, 3.5f, 19.5f, 6.5f, 19.5f, 12.5f, 12, 21f, 4.5f, 12.5f, 4.5f, 6.5f));
                    g.DrawLines(pen, P(8.8f, 12f, 11f, 14.4f, 15.4f, 9.6f));
                    break;
                case HGlyphKind.Download:
                    g.DrawLine(pen, 12f, 4f, 12f, 14.5f);
                    g.DrawLines(pen, P(8f, 10.8f, 12f, 14.8f, 16f, 10.8f));
                    g.DrawLines(pen, P(4.5f, 17f, 4.5f, 19.5f, 19.5f, 19.5f, 19.5f, 17f));
                    break;
                case HGlyphKind.Chip:
                    g.DrawRectangle(pen, 7f, 7f, 10f, 10f);
                    g.DrawLine(pen, 10, 7, 10, 4);
                    g.DrawLine(pen, 14, 7, 14, 4);
                    g.DrawLine(pen, 10, 17, 10, 20);
                    g.DrawLine(pen, 14, 17, 14, 20);
                    g.DrawLine(pen, 7, 10, 4, 10);
                    g.DrawLine(pen, 7, 14, 4, 14);
                    g.DrawLine(pen, 17, 10, 20, 10);
                    g.DrawLine(pen, 17, 14, 20, 14);
                    break;
                case HGlyphKind.Network:
                    g.FillEllipse(brush, 3.2f, 16.2f, 4.2f, 4.2f);
                    g.FillEllipse(brush, 16.6f, 16.2f, 4.2f, 4.2f);
                    g.FillEllipse(brush, 9.9f, 3.2f, 4.2f, 4.2f);
                    g.DrawLine(pen, 7.2f, 16.5f, 10.8f, 7f);
                    g.DrawLine(pen, 16.8f, 16.5f, 13.2f, 7f);
                    break;
                case HGlyphKind.Database:
                    g.DrawEllipse(pen, 4.5f, 4f, 15f, 6f);
                    g.DrawLine(pen, 4.5f, 7f, 4.5f, 17f);
                    g.DrawLine(pen, 19.5f, 7f, 19.5f, 17f);
                    g.DrawArc(pen, 4.5f, 8.5f, 15f, 6f, 0, 180);
                    g.DrawArc(pen, 4.5f, 14f, 15f, 6f, 0, 180);
                    break;
                case HGlyphKind.Server:
                    g.DrawRectangle(pen, 4f, 5f, 16f, 6f);
                    g.DrawRectangle(pen, 4f, 13f, 16f, 6f);
                    g.FillEllipse(brush, 7.2f, 7.2f, 1.7f, 1.7f);
                    g.FillEllipse(brush, 7.2f, 15.2f, 1.7f, 1.7f);
                    g.DrawLine(pen, 11, 8, 17, 8);
                    g.DrawLine(pen, 11, 16, 17, 16);
                    break;
                case HGlyphKind.Lock:
                    g.DrawArc(pen, 7.5f, 4.5f, 9f, 9f, 180, 180);
                    g.DrawRectangle(pen, 5f, 10.5f, 14f, 9.5f);
                    g.FillEllipse(brush, 11.2f, 13.8f, 1.6f, 1.6f);
                    g.DrawLine(pen, 12, 15.4f, 12, 17.5f);
                    break;
                case HGlyphKind.Search:
                    g.DrawEllipse(pen, 4.5f, 4.5f, 12f, 12f);
                    g.DrawLine(pen, 13.5f, 13.5f, 20f, 20f);
                    break;
                case HGlyphKind.User:
                    g.DrawArc(pen, 8f, 3.5f, 8f, 8f, 180, 180);
                    g.DrawArc(pen, 8f, 2.5f, 8f, 8f, 180, 180);
                    g.DrawArc(pen, 4.5f, 13f, 15f, 10f, 180, 180);
                    g.DrawLine(pen, 4.5f, 20.5f, 19.5f, 20.5f);
                    break;
                case HGlyphKind.Play:
                    g.FillPolygon(brush, P(8, 5.5f, 18.5f, 12, 8, 18.5f));
                    break;
                case HGlyphKind.Stop:
                    g.FillRectangle(brush, 6.5f, 6.5f, 11f, 11f);
                    break;
                case HGlyphKind.Plus:
                    g.DrawLine(pen, 12f, 5f, 12f, 19f);
                    g.DrawLine(pen, 5f, 12f, 19f, 12f);
                    break;
                case HGlyphKind.Minus:
                    g.DrawLine(pen, 5f, 12f, 19f, 12f);
                    break;
                case HGlyphKind.Fit:
                    g.DrawLines(pen, P(4f, 9f, 4f, 4f, 9f, 4f));
                    g.DrawLines(pen, P(15f, 4f, 20f, 4f, 20f, 9f));
                    g.DrawLines(pen, P(20f, 15f, 20f, 20f, 15f, 20f));
                    g.DrawLines(pen, P(9f, 20f, 4f, 20f, 4f, 15f));
                    break;
                case HGlyphKind.Video:
                    g.DrawRectangle(pen, 3.5f, 6f, 13f, 12f);
                    g.DrawPolygon(pen, P(16.5f, 10f, 21f, 7f, 21f, 17f, 16.5f, 14f));
                    break;
                case HGlyphKind.Gauge:
                    g.DrawArc(pen, 4f, 6f, 16f, 14f, 180, 180);
                    g.DrawLine(pen, 12, 13, 15.5f, 9.5f);
                    g.FillEllipse(brush, 11f, 12f, 2f, 2f);
                    g.DrawLine(pen, 4f, 20f, 20f, 20f);
                    break;
                case HGlyphKind.Globe:
                    g.DrawEllipse(pen, 4f, 4f, 16f, 16f);
                    g.DrawEllipse(pen, 9.3f, 4f, 5.4f, 16f);
                    g.DrawLine(pen, 4.5f, 12f, 19.5f, 12f);
                    g.DrawArc(pen, 4f, 7f, 16f, 10f, 20, 140);
                    g.DrawArc(pen, 4f, 7f, 16f, 10f, 200, 140);
                    break;
            }
        }
    }
}
