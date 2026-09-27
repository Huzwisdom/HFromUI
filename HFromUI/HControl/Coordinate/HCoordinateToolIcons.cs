using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Coordinate
{
    /// <summary>
    /// HCoordinateA 工具栏矢量图标生成器：以 24 逻辑坐标网格统一绘制黑（主）+橙（辅）两色图标，
    /// 输出带 Alpha 通道的位图。仅负责按钮图标，不涉及按钮布局与交互。
    /// </summary>
    internal static class HCoordinateToolIcons
    {
        /// <summary>图标主色：近黑。</summary>
        private static readonly Color Black = Color.FromArgb(38, 38, 38);

        /// <summary>图标辅色：橙色。</summary>
        private static readonly Color Orange = Color.FromArgb(255, 140, 0);

        /// <summary>输出位图边长（像素），24 逻辑单位对应 48 像素。</summary>
        private const int OutputSize = 48;

        private const float U = OutputSize / 24f;

        private static readonly Dictionary<string, Bitmap> Cache = new Dictionary<string, Bitmap>();

        /// <summary>
        /// 按按钮名称后缀（toolSbtn_ 之后的部分）获取对应图标位图，缓存复用。
        /// </summary>
        /// <param name="key">按钮名称后缀，如 Mouse、Open、Save。</param>
        /// <returns>48×48 带透明背景的图标位图。</returns>
        public static Bitmap Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            Bitmap bmp;
            if (!Cache.TryGetValue(key, out bmp))
            {
                bmp = Render(key);
                Cache[key] = bmp;
            }

            return bmp;
        }

        /// <summary>
        /// 遍历工具栏，将所有已定义图标的工具按钮替换为新的矢量图标，并释放旧位图。
        /// </summary>
        /// <param name="strip">需要替换图标的工具栏。</param>
        public static void Apply(ToolStrip strip)
        {
            if (strip == null)
            {
                return;
            }

            foreach (ToolStripItem item in strip.Items)
            {
                var btn = item as ToolStripButton;
                if (btn == null || string.IsNullOrEmpty(btn.Name))
                {
                    continue;
                }

                const string prefix = "toolSbtn_";
                if (!btn.Name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string key = btn.Name.Substring(prefix.Length);
                if (!HasIcon(key))
                {
                    continue;
                }

                Image old = btn.Image;
                btn.Image = Get(key);
                if (old != null)
                {
                    old.Dispose();
                }
            }
        }

        /// <summary>是否为该按钮名称定义了图标。</summary>
        private static bool HasIcon(string key)
        {
            switch (key)
            {
                case "Enable":
                case "Open":
                case "Save":
                case "Del":
                case "Forward":
                case "backward":
                case "GoLast":
                case "GoNext":
                case "Format":
                case "GoFormat":
                case "Appropriate":
                case "Last":
                case "Next":
                case "Line":
                case "Continuous":
                case "3pArc":
                case "Group":
                case "UnGroup":
                case "ShowPrestore":
                case "Prestore":
                case "Set":
                case "Top":
                case "Bottom":
                case "GCode":
                case "Start":
                case "Stop":
                case "Mouse":
                case "Select":
                case "Point":
                case "Number":
                case "DecimalPlaces":
                case "Layer":
                case "DXF":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>在 24 逻辑坐标网格上渲染指定图标。</summary>
        private static Bitmap Render(string key)
        {
            var bmp = new Bitmap(OutputSize, OutputSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.ScaleTransform(U, U);

                Draw(g, key);
            }

            return bmp;
        }

        /// <summary>按图标名称分发到具体绘制方法。</summary>
        private static void Draw(Graphics g, string key)
        {
            switch (key)
            {
                case "Enable": DrawEnable(g); break;
                case "Open": DrawOpen(g); break;
                case "Save": DrawSave(g); break;
                case "Del": DrawDel(g); break;
                case "Forward": DrawHistory(g, false); break;
                case "backward": DrawHistory(g, true); break;
                case "GoLast": DrawStepChevron(g, false); break;
                case "GoNext": DrawStepChevron(g, true); break;
                case "Format": DrawFormat(g); break;
                case "GoFormat": DrawGoFormat(g); break;
                case "Appropriate": DrawAppropriate(g); break;
                case "Last": DrawPaging(g, false); break;
                case "Next": DrawPaging(g, true); break;
                case "Line": DrawLine(g); break;
                case "Continuous": DrawContinuous(g); break;
                case "3pArc": DrawThreePointArc(g); break;
                case "Group": DrawGroup(g); break;
                case "UnGroup": DrawUnGroup(g); break;
                case "ShowPrestore": DrawShowPrestore(g); break;
                case "Prestore": DrawPrestore(g); break;
                case "Set": DrawSet(g); break;
                case "Top": DrawZOrder(g, true); break;
                case "Bottom": DrawZOrder(g, false); break;
                case "GCode": DrawGCode(g); break;
                case "Start": DrawStart(g); break;
                case "Stop": DrawStop(g); break;
                case "Mouse": DrawCursor(g, 1f, 0f, 0f); break;
                case "Select": DrawSelect(g); break;
                case "Point": DrawPoint(g); break;
                case "Number": DrawNumber(g); break;
                case "DecimalPlaces": DrawDecimalPlaces(g); break;
                case "Layer": DrawLayer(g); break;
                case "DXF": DrawDXF(g); break;
            }
        }

        #region 基础图元

        private static PointF P(float x, float y)
        {
            return new PointF(x, y);
        }

        private static Pen Pen(Color color, float width)
        {
            return new Pen(color, width)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
        }

        private static void L(Graphics g, Color color, float width, float x1, float y1, float x2, float y2)
        {
            using (var p = Pen(color, width))
            {
                g.DrawLine(p, x1, y1, x2, y2);
            }
        }

        private static void S(Graphics g, Color color, float width, params float[] xy)
        {
            using (var p = Pen(color, width))
            using (var path = new GraphicsPath())
            {
                var pts = new PointF[xy.Length / 2];
                for (int i = 0; i < pts.Length; i++)
                {
                    pts[i] = P(xy[i * 2], xy[i * 2 + 1]);
                }

                path.AddLines(pts);
                g.DrawPath(p, path);
            }
        }

        private static void FP(Graphics g, Color color, params float[] xy)
        {
            var pts = new PointF[xy.Length / 2];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = P(xy[i * 2], xy[i * 2 + 1]);
            }

            using (var brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, pts);
            }
        }

        private static void Ring(Graphics g, Color color, float width, float cx, float cy, float d)
        {
            using (var p = Pen(color, width))
            {
                g.DrawEllipse(p, cx - d / 2f, cy - d / 2f, d, d);
            }
        }

        private static void Dot(Graphics g, Color color, float cx, float cy, float r)
        {
            using (var brush = new SolidBrush(color))
            {
                g.FillEllipse(brush, cx - r, cy - r, r * 2f, r * 2f);
            }
        }

        private static void Arc(Graphics g, Color color, float width, float x, float y, float w, float h, float start, float sweep)
        {
            using (var p = Pen(color, width))
            {
                g.DrawArc(p, x, y, w, h, start, sweep);
            }
        }

        private static GraphicsPath RoundedPath(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            float d = r * 2f;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void RR(Graphics g, Color color, float width, float x, float y, float w, float h, float r)
        {
            using (var p = Pen(color, width))
            using (GraphicsPath path = RoundedPath(x, y, w, h, r))
            {
                g.DrawPath(p, path);
            }
        }

        private static void RRFill(Graphics g, Color color, float x, float y, float w, float h, float r)
        {
            using (var brush = new SolidBrush(color))
            using (GraphicsPath path = RoundedPath(x, y, w, h, r))
            {
                g.FillPath(brush, path);
            }
        }

        private static void Txt(Graphics g, string text, float cx, float cy, float em, Color color)
        {
            using (var font = new Font("Segoe UI", em, FontStyle.Bold, GraphicsUnit.World))
            using (var brush = new SolidBrush(color))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };
                var rect = new RectangleF(cx - em * 2.4f, cy - em * 0.8f, em * 4.8f, em * 1.6f);
                g.DrawString(text, font, brush, rect, sf);
                sf.Dispose();
            }
        }

        #endregion 基础图元

        #region 具体图标

        // 使能绘图：铅笔
        private static void DrawEnable(Graphics g)
        {
            FP(g, Black, 4f, 20f, 6.3f, 15.6f, 9f, 18.3f);
            L(g, Black, 2.8f, 6.4f, 17.6f, 15f, 9f);
            L(g, Orange, 2.8f, 15f, 9f, 18f, 6f);
            L(g, Black, 1.6f, 13.7f, 7.7f, 16.7f, 10.7f);
        }

        // 打开文件：文件夹 + 橙色向下箭头
        private static void DrawOpen(Graphics g)
        {
            S(g, Black, 1.8f, 3f, 9.5f, 9.5f, 9.5f, 11.2f, 6.8f, 21f, 6.8f, 21f, 9.5f);
            L(g, Black, 1.8f, 3f, 9.5f, 21f, 9.5f);
            L(g, Black, 1.8f, 3f, 9.5f, 3f, 19f);
            L(g, Black, 1.8f, 21f, 9.5f, 21f, 19f);
            L(g, Black, 1.8f, 3f, 19f, 21f, 19f);
            L(g, Orange, 2f, 12f, 10.8f, 12f, 16.4f);
            S(g, Orange, 2f, 9.4f, 13.8f, 12f, 16.4f, 14.6f, 13.8f);
        }

        // 保存文件：软盘
        private static void DrawSave(Graphics g)
        {
            RR(g, Black, 1.8f, 4f, 3.5f, 16f, 17f, 1.5f);
            S(g, Black, 1.6f, 8f, 3.5f, 8f, 8.5f, 16f, 8.5f, 16f, 3.5f);
            RRFill(g, Orange, 7.5f, 12f, 9f, 6f, 1f);
        }

        // 删除所选线：垃圾桶
        private static void DrawDel(Graphics g)
        {
            S(g, Black, 1.8f, 9.5f, 7.5f, 10f, 5f, 14f, 5f, 14.5f, 7.5f);
            L(g, Black, 1.8f, 5f, 7.5f, 19f, 7.5f);
            S(g, Black, 1.8f, 6.5f, 7.5f, 7.3f, 19.5f, 16.7f, 19.5f, 17.5f, 7.5f);
            L(g, Orange, 1.6f, 10f, 10.5f, 10f, 17f);
            L(g, Orange, 1.6f, 14f, 10.5f, 14f, 17f);
        }

        // 向前撤回 / 向后撤回：圆环 + 橙箭头（左 / 右）
        private static void DrawHistory(Graphics g, bool toRight)
        {
            Ring(g, Black, 1.8f, 12f, 12f, 16.5f);
            if (toRight)
            {
                L(g, Orange, 2f, 9.2f, 12f, 15.2f, 12f);
                S(g, Orange, 2f, 13f, 8.8f, 15.2f, 12f, 13f, 15.2f);
            }
            else
            {
                L(g, Orange, 2f, 14.8f, 12f, 8.8f, 12f);
                S(g, Orange, 2f, 11f, 8.8f, 8.8f, 12f, 11f, 15.2f);
            }
        }

        // 线段前移 / 后移：上 / 下尖括号
        private static void DrawStepChevron(Graphics g, bool down)
        {
            if (down)
            {
                S(g, Black, 2.2f, 5f, 9.5f, 12f, 16.5f, 19f, 9.5f);
                Dot(g, Orange, 12f, 6.4f, 1.2f);
            }
            else
            {
                S(g, Black, 2.2f, 5f, 14.5f, 12f, 7.5f, 19f, 14.5f);
                Dot(g, Orange, 12f, 17.6f, 1.2f);
            }
        }

        // 设置幅面：幅面纸
        private static void DrawFormat(Graphics g)
        {
            RR(g, Black, 1.8f, 3.5f, 3.5f, 17f, 17f, 1.8f);
            L(g, Black, 1.8f, 3.5f, 8.5f, 20.5f, 8.5f);
            Dot(g, Orange, 7.5f, 6f, 1.05f);
            Dot(g, Orange, 12f, 6f, 1.05f);
            Dot(g, Orange, 16.5f, 6f, 1.05f);
            L(g, Black, 1.6f, 7f, 12f, 17f, 12f);
            L(g, Black, 1.6f, 7f, 15.5f, 17f, 15.5f);
        }

        // 去幅面中心位置：定位图钉
        private static void DrawGoFormat(Graphics g)
        {
            Ring(g, Black, 1.8f, 12f, 9.5f, 9f);
            S(g, Black, 1.8f, 8.7f, 13.9f, 12f, 20.6f, 15.3f, 13.9f);
            Dot(g, Orange, 12f, 9.5f, 2.25f);
        }

        // 去合适的位置：四角取景框
        private static void DrawAppropriate(Graphics g)
        {
            S(g, Black, 1.9f, 8f, 3f, 3f, 3f, 3f, 8f);
            S(g, Black, 1.9f, 16f, 3f, 21f, 3f, 21f, 8f);
            S(g, Black, 1.9f, 3f, 16f, 3f, 21f, 8f, 21f);
            S(g, Black, 1.9f, 21f, 16f, 21f, 21f, 16f, 21f);
            Dot(g, Orange, 12f, 12f, 1.25f);
        }

        // 上一个 / 下一个：双尖括号 + 橙色竖条
        private static void DrawPaging(Graphics g, bool toRight)
        {
            if (toRight)
            {
                S(g, Black, 2.1f, 11f, 5f, 17.5f, 12f, 11f, 19f);
                S(g, Black, 2.1f, 5.5f, 5f, 12f, 12f, 5.5f, 19f);
                L(g, Orange, 2f, 20f, 6.5f, 20f, 17.5f);
            }
            else
            {
                S(g, Black, 2.1f, 13f, 5f, 6.5f, 12f, 13f, 19f);
                S(g, Black, 2.1f, 18.5f, 5f, 12f, 12f, 18.5f, 19f);
                L(g, Orange, 2f, 4f, 6.5f, 4f, 17.5f);
            }
        }

        // 画线：斜线 + 橙色端点
        private static void DrawLine(Graphics g)
        {
            L(g, Black, 2.2f, 5.5f, 18.5f, 18.5f, 5.5f);
            Dot(g, Orange, 5.5f, 18.5f, 2f);
            Dot(g, Orange, 18.5f, 5.5f, 2f);
        }

        // 连续画线：折线 + 橙色顶点
        private static void DrawContinuous(Graphics g)
        {
            S(g, Black, 2f, 3.5f, 18.5f, 8.5f, 8.5f, 13f, 13.5f, 20.5f, 4.5f);
            Dot(g, Orange, 3.5f, 18.5f, 1.7f);
            Dot(g, Orange, 8.5f, 8.5f, 1.7f);
            Dot(g, Orange, 13f, 13.5f, 1.7f);
            Dot(g, Orange, 20.5f, 4.5f, 1.7f);
        }

        // 画圆弧（三点弧）：弧 + 三个橙色点
        private static void DrawThreePointArc(Graphics g)
        {
            Arc(g, Black, 2f, 4f, 4f, 16f, 16f, 200f, 130f);
            Dot(g, Orange, 4.48f, 9.26f, 1.8f);
            Dot(g, Orange, 11.3f, 4.03f, 1.8f);
            Dot(g, Orange, 18.93f, 8f, 1.8f);
        }

        // 群组：黑人形在后、橙人形在前
        private static void DrawGroup(Graphics g)
        {
            Ring(g, Black, 1.7f, 9.3f, 8.8f, 4.4f);
            Arc(g, Black, 1.7f, 3.8f, 13f, 11f, 11f, 200f, 140f);
            Ring(g, Orange, 1.7f, 15.2f, 10f, 4.4f);
            Arc(g, Orange, 1.7f, 9.7f, 14.2f, 11f, 11f, 200f, 140f);
        }

        // 解除群组：两个错开的圆角方框 + 橙色断连叉
        private static void DrawUnGroup(Graphics g)
        {
            RR(g, Black, 1.8f, 4f, 6.5f, 11f, 11f, 2f);
            RR(g, Black, 1.8f, 9f, 10.5f, 11f, 11f, 2f);
            L(g, Orange, 1.7f, 10.7f, 12.2f, 13.3f, 14.8f);
            L(g, Orange, 1.7f, 13.3f, 12.2f, 10.7f, 14.8f);
        }

        // 是否显示预存：取景框 + 眼睛
        private static void DrawShowPrestore(Graphics g)
        {
            S(g, Black, 1.7f, 8f, 3f, 3.5f, 3f, 3.5f, 7.5f);
            S(g, Black, 1.7f, 16f, 3f, 20.5f, 3f, 20.5f, 7.5f);
            S(g, Black, 1.7f, 3.5f, 16.5f, 3.5f, 21f, 8f, 21f);
            S(g, Black, 1.7f, 20.5f, 16.5f, 20.5f, 21f, 16f, 21f);
            Arc(g, Black, 1.8f, 5.5f, 8.5f, 13f, 7f, 180f, 180f);
            Arc(g, Black, 1.8f, 5.5f, 8.5f, 13f, 7f, 0f, 180f);
            Dot(g, Orange, 12f, 12f, 2.2f);
        }

        // 保存预存：云上传
        private static void DrawPrestore(Graphics g)
        {
            Arc(g, Black, 1.8f, 4f, 11f, 7f, 6f, 180f, 180f);
            Arc(g, Black, 1.8f, 7.5f, 6.5f, 11f, 9.5f, 180f, 180f);
            L(g, Black, 1.8f, 4.5f, 14f, 4.5f, 18f);
            L(g, Black, 1.8f, 18.5f, 11.25f, 18.5f, 18f);
            L(g, Black, 1.8f, 4.5f, 18f, 18.5f, 18f);
            L(g, Orange, 2f, 11.5f, 16.8f, 11.5f, 10.5f);
            S(g, Orange, 2f, 8.8f, 13.2f, 11.5f, 10.5f, 14.2f, 13.2f);
        }

        // 设置：齿轮
        private static void DrawSet(Graphics g)
        {
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0;
                float dx = (float)Math.Cos(a);
                float dy = (float)Math.Sin(a);
                L(g, Black, 2f, 12f + dx * 6.8f, 12f + dy * 6.8f, 12f + dx * 8.6f, 12f + dy * 8.6f);
            }

            Ring(g, Black, 1.8f, 12f, 12f, 11.5f);
            Dot(g, Orange, 12f, 12f, 2.3f);
        }

        // 置顶 / 置底：前后两张卡片
        private static void DrawZOrder(Graphics g, bool toTop)
        {
            if (toTop)
            {
                RR(g, Orange, 1.7f, 8f, 4.5f, 11.5f, 11f, 1.5f);
                RR(g, Black, 1.8f, 4f, 9f, 12f, 11f, 1.5f);
                L(g, Black, 1.5f, 7f, 12.8f, 13f, 12.8f);
                L(g, Black, 1.5f, 7f, 15.6f, 13f, 15.6f);
            }
            else
            {
                RR(g, Orange, 1.7f, 4.5f, 9f, 11.5f, 11f, 1.5f);
                RR(g, Black, 1.8f, 8f, 4.5f, 11.5f, 11f, 1.5f);
            }
        }

        // G代码：折角文档 + G
        private static void DrawGCode(Graphics g)
        {
            FP(g, Orange, 13.5f, 2.5f, 19.5f, 8.5f, 13.5f, 8.5f);
            RR(g, Black, 1.8f, 4.5f, 2.5f, 15f, 19f, 1.6f);
            L(g, Black, 1.7f, 13.5f, 8.5f, 19.5f, 8.5f);
            L(g, Black, 1.7f, 13.5f, 2.5f, 13.5f, 8.5f);
            Txt(g, "G", 12f, 14.6f, 8.5f, Black);
        }

        // 模拟启动：圆环 + 橙色播放三角
        private static void DrawStart(Graphics g)
        {
            Ring(g, Black, 1.8f, 12f, 12f, 18f);
            FP(g, Orange, 9f, 6f, 9f, 18f, 18.5f, 12f);
        }

        // 模拟停止：圆环 + 橙色圆角方块
        private static void DrawStop(Graphics g)
        {
            Ring(g, Black, 1.8f, 12f, 12f, 18f);
            RRFill(g, Orange, 8.8f, 8.8f, 6.9f, 6.9f, 1.4f);
        }

        // Windows 标准箭头光标（左缘垂直、轮廓端正），黑填充 + 橙色描边
        private static void DrawCursor(Graphics g, float scale, float ox, float oy)
        {
            // 仿 IDC_ARROW 比例：尖端(0,0) → 直下 → 右拐 → 尾部两翼
            float[] src = new float[]
            {
                0f, 0f,
                0f, 15.9f,
                5f, 12.1f,
                7.9f, 19.8f,
                10.9f, 18.9f,
                8f, 11.3f,
                14.9f, 11.3f
            };

            var pts = new PointF[src.Length / 2];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = P(5.6f + ox + src[i * 2] * scale, 2.8f + oy + src[i * 2 + 1] * scale);
            }

            using (var pen = Pen(Orange, 2f * scale))
            {
                g.DrawPolygon(pen, pts);
            }

            using (var brush = new SolidBrush(Black))
            {
                g.FillPolygon(brush, pts);
            }
        }

        // 选择：十字捕捉 + 小箭头
        private static void DrawSelect(Graphics g)
        {
            L(g, Black, 1.8f, 9f, 2.6f, 9f, 10.6f);
            L(g, Black, 1.8f, 4.6f, 7f, 13.4f, 7f);
            Dot(g, Orange, 9f, 7f, 1.4f);
            DrawCursor(g, 0.62f, 12.2f, 9.4f);
        }

        // 画点：靶心
        private static void DrawPoint(Graphics g)
        {
            Ring(g, Black, 1.8f, 12f, 12f, 12.5f);
            L(g, Black, 1.7f, 12f, 3f, 12f, 6.5f);
            L(g, Black, 1.7f, 12f, 17.5f, 12f, 21f);
            L(g, Black, 1.7f, 3f, 12f, 6.5f, 12f);
            L(g, Black, 1.7f, 17.5f, 12f, 21f, 12f);
            Dot(g, Orange, 12f, 12f, 2.1f);
        }

        // 捕捉：带十字的捕捉圆
        private static void DrawNumber(Graphics g)
        {
            L(g, Black, 1.8f, 12f, 2.8f, 12f, 21.2f);
            L(g, Black, 1.8f, 2.8f, 12f, 21.2f, 12f);
            Ring(g, Black, 1.9f, 12f, 12f, 10.5f);
            Dot(g, Orange, 12f, 12f, 2f);
        }

        // 小数点设置：黑色圆角牌 + 0 橙点 0
        private static void DrawDecimalPlaces(Graphics g)
        {
            RRFill(g, Black, 2.5f, 2.5f, 19f, 19f, 3.5f);
            Txt(g, "0", 7.4f, 12.4f, 9f, Color.White);
            Dot(g, Orange, 12f, 14.4f, 1.35f);
            Txt(g, "0", 16.6f, 12.4f, 9f, Color.White);
        }

        // 图层：叠放卡片
        private static void DrawLayer(Graphics g)
        {
            RR(g, Orange, 1.7f, 8.5f, 3.5f, 12f, 11.5f, 1.5f);
            RR(g, Black, 1.8f, 3.5f, 8.5f, 12f, 12f, 1.5f);
            L(g, Black, 1.5f, 6.5f, 12.5f, 12.5f, 12.5f);
            L(g, Black, 1.5f, 6.5f, 15.3f, 12.5f, 15.3f);
            L(g, Black, 1.5f, 6.5f, 18.1f, 12.5f, 18.1f);
        }

        // 导入DXF：折角文档 + 折线 + DXF
        private static void DrawDXF(Graphics g)
        {
            FP(g, Orange, 14f, 2.5f, 20f, 8.5f, 14f, 8.5f);
            RR(g, Black, 1.8f, 4f, 2.5f, 16f, 19f, 1.5f);
            L(g, Black, 1.7f, 14f, 8.5f, 20f, 8.5f);
            L(g, Black, 1.7f, 14f, 2.5f, 14f, 8.5f);
            S(g, Black, 1.4f, 7f, 10.5f, 9.5f, 8f, 12f, 10.5f, 14.5f, 8f, 17f, 10.5f);
            Dot(g, Orange, 7f, 10.5f, 1f);
            Dot(g, Orange, 17f, 10.5f, 1f);
            Txt(g, "DXF", 12f, 15.8f, 6f, Orange);
        }

        #endregion 具体图标
    }
}
