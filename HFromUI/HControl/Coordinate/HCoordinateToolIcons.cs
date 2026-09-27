using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Coordinate
{
    /// <summary>
    /// 工具栏按钮图标的显示状态：常态（煤炭黑）、激活（橙家族，表示工具选中或使能开启）、
    /// 运行（绿家族，表示模拟运行中）。
    /// </summary>
    internal enum CoordinateIconState
    {
        /// <summary>常态：煤炭黑主体，需要层次时只用煤炭近色（炭灰）。</summary>
        Normal,

        /// <summary>激活：橙色主体，层次用橙色近色（深橙）。</summary>
        Active,

        /// <summary>运行：绿色主体（模拟运行中的启动/停止按钮）。</summary>
        Running
    }

    /// <summary>
    /// HCoordinatePad 工具栏矢量图标生成器：以 24 逻辑坐标网格统一绘制。
    /// 配色原则：煤炭黑为主基调，需要“散发”（层次/点缀）时只取主色的相近色，
    /// 不在各图标中分散使用橙色；橙色仅用于激活态整体强调，绿色仅用于运行态。
    /// </summary>
    internal static class HCoordinateToolIcons
    {
        // —— 煤炭家族 ——
        private static readonly Color Coal = Color.FromArgb(38, 38, 38);
        private static readonly Color CoalSoft = Color.FromArgb(108, 108, 108);
        private static readonly Color CoalLight = Color.FromArgb(178, 178, 178);

        // —— 橙色家族（激活态） ——
        private static readonly Color Orange = Color.FromArgb(255, 140, 0);
        private static readonly Color OrangeDeep = Color.FromArgb(204, 96, 0);

        // —— 绿色家族（运行态） ——
        private static readonly Color Green = Color.FromArgb(46, 150, 70);
        private static readonly Color GreenDeep = Color.FromArgb(24, 104, 50);

        /// <summary>输出位图边长（像素），24 逻辑单位对应 48 像素。</summary>
        private const int OutputSize = 48;

        private const float U = OutputSize / 24f;

        /// <summary>一套图标的取色方案：主体、主体近色（层次）、深底上的浅色、强调色。</summary>
        private sealed class Palette
        {
            public Color Main;
            public Color Soft;
            public Color Light;
            public Color Accent;
        }

        private static readonly Palette NormalPalette = new Palette
        {
            Main = Coal,
            Soft = CoalSoft,
            Light = CoalLight,
            Accent = Orange
        };

        private static readonly Palette ActivePalette = new Palette
        {
            Main = Orange,
            Soft = OrangeDeep,
            Light = Orange,
            Accent = OrangeDeep
        };

        private static readonly Palette RunningPalette = new Palette
        {
            Main = Green,
            Soft = GreenDeep,
            Light = Green,
            Accent = Green
        };

        private static readonly Dictionary<string, Bitmap> Cache = new Dictionary<string, Bitmap>();

        /// <summary>记录缓存位图，避免切换状态时误释放共享位图。</summary>
        private static readonly HashSet<Image> CacheImages = new HashSet<Image>();

        /// <summary>记录每个按钮当前的图标状态，状态未变时不重复赋图。</summary>
        private static readonly Dictionary<string, int> ButtonStates = new Dictionary<string, int>();

        /// <summary>
        /// 按按钮名称后缀（toolSbtn_ 之后的部分）获取常态图标位图，缓存复用。
        /// </summary>
        /// <param name="key">按钮名称后缀，如 Mouse、Open、Save。</param>
        /// <returns>48×48 带透明背景的图标位图。</returns>
        public static Bitmap Get(string key)
        {
            return Get(key, CoordinateIconState.Normal);
        }

        /// <summary>
        /// 按按钮名称后缀与显示状态获取图标位图，缓存复用（同一图同一状态全局共享）。
        /// </summary>
        public static Bitmap Get(string key, CoordinateIconState state)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            string cacheKey = state + "|" + key;
            Bitmap bmp;
            if (!Cache.TryGetValue(cacheKey, out bmp))
            {
                bmp = Render(key, ResolvePalette(state));
                Cache[cacheKey] = bmp;
                CacheImages.Add(bmp);
            }

            return bmp;
        }

        /// <summary>
        /// 遍历工具栏，将所有已定义图标的工具按钮替换为新的矢量常态图标，
        /// 并释放 resx 中载入的原始位图（缓存的共享位图不会被释放）。
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

                string key = ExtractKey(btn.Name);
                if (key == null || !HasIcon(key))
                {
                    continue;
                }

                Image old = btn.Image;
                btn.Image = Get(key, CoordinateIconState.Normal);
                ButtonStates[btn.Name] = (int)CoordinateIconState.Normal;
                if (old != null && !CacheImages.Contains(old))
                {
                    old.Dispose();
                }
            }
        }

        /// <summary>
        /// 切换工具按钮图标的显示状态（常态/激活/运行）。只更换共享缓存位图的引用，不释放位图。
        /// </summary>
        /// <param name="btn">目标工具按钮。</param>
        /// <param name="state">目标显示状态。</param>
        public static void SetState(ToolStripButton btn, CoordinateIconState state)
        {
            if (btn == null || string.IsNullOrEmpty(btn.Name))
            {
                return;
            }

            string key = ExtractKey(btn.Name);
            if (key == null || !HasIcon(key))
            {
                return;
            }

            int stateValue = (int)state;
            int current;
            if (ButtonStates.TryGetValue(btn.Name, out current) && current == stateValue)
            {
                return;
            }

            btn.Image = Get(key, state);
            ButtonStates[btn.Name] = stateValue;
        }

        /// <summary>从按钮名取出 toolSbtn_ 后缀；不匹配返回 null。</summary>
        private static string ExtractKey(string buttonName)
        {
            const string prefix = "toolSbtn_";
            return buttonName.StartsWith(prefix, StringComparison.Ordinal)
                ? buttonName.Substring(prefix.Length)
                : null;
        }

        private static Palette ResolvePalette(CoordinateIconState state)
        {
            switch (state)
            {
                case CoordinateIconState.Active:
                    return ActivePalette;
                case CoordinateIconState.Running:
                    return RunningPalette;
                default:
                    return NormalPalette;
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
        private static Bitmap Render(string key, Palette pl)
        {
            var bmp = new Bitmap(OutputSize, OutputSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.ScaleTransform(U, U);

                Draw(g, key, pl);
            }

            return bmp;
        }

        /// <summary>按图标名称分发到具体绘制方法。</summary>
        private static void Draw(Graphics g, string key, Palette pl)
        {
            switch (key)
            {
                case "Enable": DrawEnable(g, pl); break;
                case "Open": DrawOpen(g, pl); break;
                case "Save": DrawSave(g, pl); break;
                case "Del": DrawDel(g, pl); break;
                case "Forward": DrawHistory(g, pl, false); break;
                case "backward": DrawHistory(g, pl, true); break;
                case "GoLast": DrawStepChevron(g, pl, false); break;
                case "GoNext": DrawStepChevron(g, pl, true); break;
                case "Format": DrawFormat(g, pl); break;
                case "GoFormat": DrawGoFormat(g, pl); break;
                case "Appropriate": DrawAppropriate(g, pl); break;
                case "Last": DrawPaging(g, pl, false); break;
                case "Next": DrawPaging(g, pl, true); break;
                case "Line": DrawLine(g, pl); break;
                case "Continuous": DrawContinuous(g, pl); break;
                case "3pArc": DrawThreePointArc(g, pl); break;
                case "Group": DrawGroup(g, pl); break;
                case "UnGroup": DrawUnGroup(g, pl); break;
                case "ShowPrestore": DrawShowPrestore(g, pl); break;
                case "Prestore": DrawPrestore(g, pl); break;
                case "Set": DrawSet(g, pl); break;
                case "Top": DrawZOrder(g, pl, true); break;
                case "Bottom": DrawZOrder(g, pl, false); break;
                case "GCode": DrawGCode(g, pl); break;
                case "Start": DrawStart(g, pl); break;
                case "Stop": DrawStop(g, pl); break;
                case "Mouse": DrawCursor(g, pl, 1f, 0f, 0f); break;
                case "Select": DrawSelect(g, pl); break;
                case "Point": DrawPoint(g, pl); break;
                case "Number": DrawNumber(g, pl); break;
                case "DecimalPlaces": DrawDecimalPlaces(g, pl); break;
                case "Layer": DrawLayer(g, pl); break;
                case "DXF": DrawDXF(g, pl); break;
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

        // 使能绘图：铅笔（激活态整体变橙）
        private static void DrawEnable(Graphics g, Palette pl)
        {
            FP(g, pl.Main, 4f, 20f, 6.3f, 15.6f, 9f, 18.3f);
            L(g, pl.Main, 2.8f, 6.4f, 17.6f, 15f, 9f);
            L(g, pl.Main, 2.8f, 15f, 9f, 18f, 6f);
            L(g, pl.Soft, 1.6f, 13.7f, 7.7f, 16.7f, 10.7f);
        }

        // 打开文件：文件夹 + 向下箭头
        private static void DrawOpen(Graphics g, Palette pl)
        {
            S(g, pl.Main, 1.8f, 3f, 9.5f, 9.5f, 9.5f, 11.2f, 6.8f, 21f, 6.8f, 21f, 9.5f);
            L(g, pl.Main, 1.8f, 3f, 9.5f, 21f, 9.5f);
            L(g, pl.Main, 1.8f, 3f, 19f, 21f, 19f);
            L(g, pl.Main, 1.8f, 21f, 9.5f, 21f, 19f);
            L(g, pl.Main, 2f, 12f, 10.8f, 12f, 16.4f);
            S(g, pl.Main, 2f, 9.4f, 13.8f, 12f, 16.4f, 14.6f, 13.8f);
        }

        // 保存文件：软盘（标签块用煤炭近色）
        private static void DrawSave(Graphics g, Palette pl)
        {
            RR(g, pl.Main, 1.8f, 4f, 3.5f, 16f, 17f, 1.5f);
            S(g, pl.Main, 1.6f, 8f, 3.5f, 8f, 8.5f, 16f, 8.5f, 16f, 3.5f);
            RRFill(g, pl.Soft, 7.5f, 12f, 9f, 6f, 1f);
        }

        // 删除所选线：垃圾桶（内筋用近色）
        private static void DrawDel(Graphics g, Palette pl)
        {
            S(g, pl.Main, 1.8f, 9.5f, 7.5f, 10f, 5f, 14f, 5f, 14.5f, 7.5f);
            L(g, pl.Main, 1.8f, 5f, 7.5f, 19f, 7.5f);
            S(g, pl.Main, 1.8f, 6.5f, 7.5f, 7.3f, 19.5f, 16.7f, 19.5f, 17.5f, 7.5f);
            L(g, pl.Soft, 1.6f, 10f, 10.5f, 10f, 17f);
            L(g, pl.Soft, 1.6f, 14f, 10.5f, 14f, 17f);
        }

        // 向前撤回 / 向后撤回：圆环 + 箭头（左 / 右）
        private static void DrawHistory(Graphics g, Palette pl, bool toRight)
        {
            Ring(g, pl.Main, 1.8f, 12f, 12f, 16.5f);
            if (toRight)
            {
                L(g, pl.Main, 2f, 9.2f, 12f, 15.2f, 12f);
                S(g, pl.Main, 2f, 13f, 8.8f, 15.2f, 12f, 13f, 15.2f);
            }
            else
            {
                L(g, pl.Main, 2f, 14.8f, 12f, 8.8f, 12f);
                S(g, pl.Main, 2f, 11f, 8.8f, 8.8f, 12f, 11f, 15.2f);
            }
        }

        // 线段前移 / 后移：上 / 下尖括号
        private static void DrawStepChevron(Graphics g, Palette pl, bool down)
        {
            if (down)
            {
                S(g, pl.Main, 2.2f, 5f, 9.5f, 12f, 16.5f, 19f, 9.5f);
            }
            else
            {
                S(g, pl.Main, 2.2f, 5f, 14.5f, 12f, 7.5f, 19f, 14.5f);
            }
        }

        // 设置幅面：幅面纸（装订点用近色）
        private static void DrawFormat(Graphics g, Palette pl)
        {
            RR(g, pl.Main, 1.8f, 3.5f, 3.5f, 17f, 17f, 1.8f);
            L(g, pl.Main, 1.8f, 3.5f, 8.5f, 20.5f, 8.5f);
            Dot(g, pl.Soft, 7.5f, 6f, 1.05f);
            Dot(g, pl.Soft, 12f, 6f, 1.05f);
            Dot(g, pl.Soft, 16.5f, 6f, 1.05f);
            L(g, pl.Main, 1.6f, 7f, 12f, 17f, 12f);
            L(g, pl.Main, 1.6f, 7f, 15.5f, 17f, 15.5f);
        }

        // 去幅面中心位置：定位图钉（圆心用近色）
        private static void DrawGoFormat(Graphics g, Palette pl)
        {
            Ring(g, pl.Main, 1.8f, 12f, 9.5f, 9f);
            S(g, pl.Main, 1.8f, 8.7f, 13.9f, 12f, 20.6f, 15.3f, 13.9f);
            Dot(g, pl.Soft, 12f, 9.5f, 2.25f);
        }

        // 去合适的位置：四角取景框（中心点近色）
        private static void DrawAppropriate(Graphics g, Palette pl)
        {
            S(g, pl.Main, 1.9f, 8f, 3f, 3f, 3f, 3f, 8f);
            S(g, pl.Main, 1.9f, 16f, 3f, 21f, 3f, 21f, 8f);
            S(g, pl.Main, 1.9f, 3f, 16f, 3f, 21f, 8f, 21f);
            S(g, pl.Main, 1.9f, 21f, 16f, 21f, 21f, 16f, 21f);
            Dot(g, pl.Soft, 12f, 12f, 1.25f);
        }

        // 上一个 / 下一个：双尖括号 + 竖条
        private static void DrawPaging(Graphics g, Palette pl, bool toRight)
        {
            if (toRight)
            {
                S(g, pl.Main, 2.1f, 11f, 5f, 17.5f, 12f, 11f, 19f);
                S(g, pl.Main, 2.1f, 5.5f, 5f, 12f, 12f, 5.5f, 19f);
                L(g, pl.Main, 2f, 20f, 6.5f, 20f, 17.5f);
            }
            else
            {
                S(g, pl.Main, 2.1f, 13f, 5f, 6.5f, 12f, 13f, 19f);
                S(g, pl.Main, 2.1f, 18.5f, 5f, 12f, 12f, 18.5f, 19f);
                L(g, pl.Main, 2f, 4f, 6.5f, 4f, 17.5f);
            }
        }

        // 画线：斜线 + 近色端点
        private static void DrawLine(Graphics g, Palette pl)
        {
            L(g, pl.Main, 2.2f, 5.5f, 18.5f, 18.5f, 5.5f);
            Dot(g, pl.Soft, 5.5f, 18.5f, 2f);
            Dot(g, pl.Soft, 18.5f, 5.5f, 2f);
        }

        // 连续画线：折线 + 近色顶点
        private static void DrawContinuous(Graphics g, Palette pl)
        {
            S(g, pl.Main, 2f, 3.5f, 18.5f, 8.5f, 8.5f, 13f, 13.5f, 20.5f, 4.5f);
            Dot(g, pl.Soft, 3.5f, 18.5f, 1.7f);
            Dot(g, pl.Soft, 8.5f, 8.5f, 1.7f);
            Dot(g, pl.Soft, 13f, 13.5f, 1.7f);
            Dot(g, pl.Soft, 20.5f, 4.5f, 1.7f);
        }

        // 画圆弧（三点弧）：弧 + 三个近色点
        private static void DrawThreePointArc(Graphics g, Palette pl)
        {
            Arc(g, pl.Main, 2f, 4f, 4f, 16f, 16f, 200f, 130f);
            Dot(g, pl.Soft, 4.48f, 9.26f, 1.8f);
            Dot(g, pl.Soft, 11.3f, 4.03f, 1.8f);
            Dot(g, pl.Soft, 18.93f, 8f, 1.8f);
        }

        // 群组：后人形主体色、前人形近色
        private static void DrawGroup(Graphics g, Palette pl)
        {
            Ring(g, pl.Main, 1.7f, 9.3f, 8.8f, 4.4f);
            Arc(g, pl.Main, 1.7f, 3.8f, 13f, 11f, 11f, 200f, 140f);
            Ring(g, pl.Soft, 1.7f, 15.2f, 10f, 4.4f);
            Arc(g, pl.Soft, 1.7f, 9.7f, 14.2f, 11f, 11f, 200f, 140f);
        }

        // 解除群组：两个错开的圆角方框 + 近色断连叉
        private static void DrawUnGroup(Graphics g, Palette pl)
        {
            RR(g, pl.Main, 1.8f, 4f, 6.5f, 11f, 11f, 2f);
            RR(g, pl.Main, 1.8f, 9f, 10.5f, 11f, 11f, 2f);
            L(g, pl.Soft, 1.7f, 10.7f, 12.2f, 13.3f, 14.8f);
            L(g, pl.Soft, 1.7f, 13.3f, 12.2f, 10.7f, 14.8f);
        }

        // 是否显示预存：取景框 + 眼睛（瞳孔近色）
        private static void DrawShowPrestore(Graphics g, Palette pl)
        {
            S(g, pl.Main, 1.7f, 8f, 3f, 3.5f, 3f, 3.5f, 7.5f);
            S(g, pl.Main, 1.7f, 16f, 3f, 20.5f, 3f, 20.5f, 7.5f);
            S(g, pl.Main, 1.7f, 3.5f, 16.5f, 3.5f, 21f, 8f, 21f);
            S(g, pl.Main, 1.7f, 20.5f, 16.5f, 20.5f, 21f, 16f, 21f);
            Arc(g, pl.Main, 1.8f, 5.5f, 8.5f, 13f, 7f, 180f, 180f);
            Arc(g, pl.Main, 1.8f, 5.5f, 8.5f, 13f, 7f, 0f, 180f);
            Dot(g, pl.Soft, 12f, 12f, 2.2f);
        }

        // 保存预存：云上传（单色）
        private static void DrawPrestore(Graphics g, Palette pl)
        {
            Arc(g, pl.Main, 1.8f, 4f, 11f, 7f, 6f, 180f, 180f);
            Arc(g, pl.Main, 1.8f, 7.5f, 6.5f, 11f, 9.5f, 180f, 180f);
            L(g, pl.Main, 1.8f, 4.5f, 14f, 4.5f, 18f);
            L(g, pl.Main, 1.8f, 18.5f, 11.25f, 18.5f, 18f);
            L(g, pl.Main, 1.8f, 4.5f, 18f, 18.5f, 18f);
            L(g, pl.Main, 2f, 11.5f, 16.8f, 11.5f, 10.5f);
            S(g, pl.Main, 2f, 8.8f, 13.2f, 11.5f, 10.5f, 14.2f, 13.2f);
        }

        // 设置：齿轮（中心近色）
        private static void DrawSet(Graphics g, Palette pl)
        {
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0;
                float dx = (float)Math.Cos(a);
                float dy = (float)Math.Sin(a);
                L(g, pl.Main, 2f, 12f + dx * 6.8f, 12f + dy * 6.8f, 12f + dx * 8.6f, 12f + dy * 8.6f);
            }

            Ring(g, pl.Main, 1.8f, 12f, 12f, 11.5f);
            Dot(g, pl.Soft, 12f, 12f, 2.3f);
        }

        // 置顶 / 置底：前卡主体色、后卡近色
        private static void DrawZOrder(Graphics g, Palette pl, bool toTop)
        {
            if (toTop)
            {
                RR(g, pl.Soft, 1.7f, 8f, 4.5f, 11.5f, 11f, 1.5f);
                RR(g, pl.Main, 1.8f, 4f, 9f, 12f, 11f, 1.5f);
                L(g, pl.Main, 1.5f, 7f, 12.8f, 13f, 12.8f);
                L(g, pl.Main, 1.5f, 7f, 15.6f, 13f, 15.6f);
            }
            else
            {
                RR(g, pl.Soft, 1.7f, 4.5f, 9f, 11.5f, 11f, 1.5f);
                RR(g, pl.Main, 1.8f, 8f, 4.5f, 11.5f, 11f, 1.5f);
            }
        }

        // G代码：折角文档 + G（折角近色）
        private static void DrawGCode(Graphics g, Palette pl)
        {
            FP(g, pl.Soft, 13.5f, 2.5f, 19.5f, 8.5f, 13.5f, 8.5f);
            RR(g, pl.Main, 1.8f, 4.5f, 2.5f, 15f, 19f, 1.6f);
            L(g, pl.Main, 1.7f, 13.5f, 8.5f, 19.5f, 8.5f);
            L(g, pl.Main, 1.7f, 13.5f, 2.5f, 13.5f, 8.5f);
            Txt(g, "G", 12f, 14.6f, 8.5f, pl.Main);
        }

        // 模拟启动：圆环 + 播放三角；常态三角为橙，运行态整体为绿
        private static void DrawStart(Graphics g, Palette pl)
        {
            Ring(g, pl.Main, 1.8f, 12f, 12f, 18f);
            FP(g, pl.Accent, 9f, 6f, 9f, 18f, 18.5f, 12f);
        }

        // 模拟停止：圆环 + 圆角方块；常态方块为橙，运行态整体为绿
        private static void DrawStop(Graphics g, Palette pl)
        {
            Ring(g, pl.Main, 1.8f, 12f, 12f, 18f);
            RRFill(g, pl.Accent, 8.8f, 8.8f, 6.9f, 6.9f, 1.4f);
        }

        /// <summary>
        /// Windows 标准箭头光标：左缘垂直、右缘约 45° 收拢、尾部带翼，形状挺拔不歪斜。
        /// 纯煤炭色填充（激活/运行态随调色板变色），不使用异色描边。
        /// </summary>
        private static void DrawCursor(Graphics g, Palette pl, float scale, float ox, float oy)
        {
            // 仿 IDC_ARROW 比例（热点 0,0）：左缘垂直到底 → 腰 → 尾翼 → 右肩 45° 回尖
            float[] src = new float[]
            {
                0f, 0f,
                0f, 16.5f,
                4.6f, 12.9f,
                7.6f, 20.8f,
                10.7f, 19.8f,
                7.7f, 11.8f,
                12.7f, 11.8f
            };

            var pts = new PointF[src.Length / 2];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = P(5.65f + ox + src[i * 2] * scale, 1.6f + oy + src[i * 2 + 1] * scale);
            }

            using (var brush = new SolidBrush(pl.Main))
            {
                g.FillPolygon(brush, pts);
            }

            // 同色细描边让缩小后边缘更密实（描边取主体色本身，不产生异色毛边）
            using (var pen = Pen(pl.Main, 0.9f * scale))
            {
                g.DrawPolygon(pen, pts);
            }
        }

        // 选择：十字捕捉 + 小箭头（激活态整体变橙）
        private static void DrawSelect(Graphics g, Palette pl)
        {
            L(g, pl.Main, 1.8f, 2.6f, 9f, 9.4f, 9f);
            L(g, pl.Main, 1.8f, 6f, 5.6f, 6f, 12.4f);
            Dot(g, pl.Soft, 6f, 9f, 1.3f);
            DrawCursor(g, pl, 0.62f, 7f, 8.2f);
        }

        // 画点：靶心（中心点近色）
        private static void DrawPoint(Graphics g, Palette pl)
        {
            Ring(g, pl.Main, 1.8f, 12f, 12f, 12.5f);
            L(g, pl.Main, 1.7f, 12f, 3f, 12f, 6.5f);
            L(g, pl.Main, 1.7f, 12f, 17.5f, 12f, 21f);
            L(g, pl.Main, 1.7f, 3f, 12f, 6.5f, 12f);
            L(g, pl.Main, 1.7f, 17.5f, 12f, 21f, 12f);
            Dot(g, pl.Soft, 12f, 12f, 2.1f);
        }

        // 捕捉：带十字的捕捉圆（中心近色）
        private static void DrawNumber(Graphics g, Palette pl)
        {
            L(g, pl.Main, 1.8f, 12f, 2.8f, 12f, 21.2f);
            L(g, pl.Main, 1.8f, 2.8f, 12f, 21.2f, 12f);
            Ring(g, pl.Main, 1.9f, 12f, 12f, 10.5f);
            Dot(g, pl.Soft, 12f, 12f, 2f);
        }

        // 小数点设置：煤炭色圆角牌 + 白字 0.0（点用白色，保持同系克制）
        private static void DrawDecimalPlaces(Graphics g, Palette pl)
        {
            RRFill(g, pl.Main, 2.5f, 2.5f, 19f, 19f, 3.5f);
            Txt(g, "0", 7.4f, 12.4f, 9f, Color.White);
            Dot(g, Color.White, 12f, 14.4f, 1.3f);
            Txt(g, "0", 16.6f, 12.4f, 9f, Color.White);
        }

        // 图层：前卡主体色、后卡近色
        private static void DrawLayer(Graphics g, Palette pl)
        {
            RR(g, pl.Soft, 1.7f, 8.5f, 3.5f, 12f, 11.5f, 1.5f);
            RR(g, pl.Main, 1.8f, 3.5f, 8.5f, 12f, 12f, 1.5f);
            L(g, pl.Main, 1.5f, 6.5f, 12.5f, 12.5f, 12.5f);
            L(g, pl.Main, 1.5f, 6.5f, 15.3f, 12.5f, 15.3f);
            L(g, pl.Main, 1.5f, 6.5f, 18.1f, 12.5f, 18.1f);
        }

        // 导入DXF：折角文档 + 折线 + DXF（折角/端点/文字用近色）
        private static void DrawDXF(Graphics g, Palette pl)
        {
            FP(g, pl.Soft, 14f, 2.5f, 20f, 8.5f, 14f, 8.5f);
            RR(g, pl.Main, 1.8f, 4f, 2.5f, 16f, 19f, 1.5f);
            L(g, pl.Main, 1.7f, 14f, 8.5f, 20f, 8.5f);
            L(g, pl.Main, 1.7f, 14f, 2.5f, 14f, 8.5f);
            S(g, pl.Main, 1.4f, 7f, 10.5f, 9.5f, 8f, 12f, 10.5f, 14.5f, 8f, 17f, 10.5f);
            Dot(g, pl.Soft, 7f, 10.5f, 1f);
            Dot(g, pl.Soft, 17f, 10.5f, 1f);
            Txt(g, "DXF", 12f, 15.8f, 6f, pl.Soft);
        }

        #endregion 具体图标
    }
}
