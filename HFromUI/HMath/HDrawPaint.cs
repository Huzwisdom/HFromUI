using HFromUI.HEnum;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HFromUI.HMath
{
    public static class HDrawPaint
    {
        /// <summary>
        /// 设置GDI高质量模式抗锯齿
        /// </summary>
        /// <param name="g"></param>
        public static void SetGraphicsHighQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;  //使绘图质量最高，即消除锯齿
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        }

        /// <summary>
        /// 设置GDI默认值
        /// </summary>
        /// <param name="g"></param>
        public static void SetGraphicsDefaultQuality(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.Default;
            g.InterpolationMode = InterpolationMode.Default;
            g.CompositingQuality = CompositingQuality.Default;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetTextExtentPoint32W(IntPtr hdc, string str, int len, out Size size);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern bool ExtTextOutW(IntPtr hdc, int x, int y, uint fuOptions,
            IntPtr lprc, string str, int count, int[] lpDx);
        [DllImport("gdi32.dll")]
        private static extern uint SetTextColor(IntPtr hdc, uint color);
        [DllImport("gdi32.dll")]
        private static extern int SetBkMode(IntPtr hdc, int mode);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr obj);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern int SaveDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern bool RestoreDC(IntPtr hdc, int savedState);
        [DllImport("gdi32.dll")]
        private static extern int IntersectClipRect(IntPtr hdc, int x1, int y1, int x2, int y2);

        private const int GdiTransparent = 1;

        /// <summary>
        /// 进入 GDI 文本绘制上下文：把 font 选入 hdc 并设透明背景，Dispose 时恢复原字体。
        /// 与 <see cref="TextAdvance"/>（GetTextExtentPoint32）配套使用：绘制走 ExtTextOutW、
        /// 测量走 GetTextExtentPoint32W，两者同属系统 EDIT 的原生排版引擎，按 ABC 宽度推进，
        /// overhang 字形（引号、弯引号、斜体 f、数字 1/7 等 C 宽度悬垂）与相邻字正常重叠。
        /// 切勿在这种场景混用 TextRenderer.DrawText——DrawTextEx 会把 overhang 展开成可见空隙
        /// （微软雅黑 12pt 下直引号视觉占位从 8px 变成约 24px，约 3 个字符宽），
        /// 而插入符按 TextAdvance 定位，于是出现“字很松、光标不贴字”。
        /// </summary>
        public static GdiTextContext BeginGdiText(IntPtr hdc, Font font) => new GdiTextContext(hdc, font, null);

        /// <summary>
        /// 同 <see cref="BeginGdiText(IntPtr, Font)"/>，并把 GDI 裁剪区交集到 clip：
        /// GDI+ 的 Graphics.SetClip 不会安装到 GetHdc 返回的 DC，ExtTextOutW 只认 DC 裁剪区，
        /// 故滚动绘制长文本时必须用 GDI IntersectClipRect 显式约束，文字才不会越界压到装饰图标。
        /// </summary>
        public static GdiTextContext BeginGdiText(IntPtr hdc, Font font, Rectangle? clip) => new GdiTextContext(hdc, font, clip);

        public sealed class GdiTextContext : IDisposable
        {
            private readonly IntPtr _hdc;
            private readonly IntPtr _hf;
            private readonly int _saved;
            private bool _disposed;

            internal GdiTextContext(IntPtr hdc, Font font, Rectangle? clip)
            {
                _hdc = hdc;
                _saved = SaveDC(hdc);
                _hf = font.ToHfont();
                SelectObject(hdc, _hf);
                SetBkMode(hdc, GdiTransparent);
                // GDI 裁剪矩形右/下边界为开区间，用 Right+1/Bottom+1 与内容区像素列对齐
                if (clip.HasValue)
                {
                    Rectangle r = clip.Value;
                    IntersectClipRect(hdc, r.X, r.Y, r.Right + 1, r.Bottom + 1);
                }
            }

            /// <summary>在 (x,y) 绘制一段文本，透明背景；坐标为该段左上角，与 ExtTextOut 默认对齐一致。</summary>
            public void DrawRun(string s, int x, int y, Color color)
            {
                if (string.IsNullOrEmpty(s)) return;
                SetTextColor(_hdc, (uint)ColorTranslator.ToWin32(color));
                ExtTextOutW(_hdc, x, y, 0, IntPtr.Zero, s, s.Length, null);
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                RestoreDC(_hdc, _saved);
                if (_hf != IntPtr.Zero) DeleteObject(_hf);
            }
        }

        /// <summary>
        /// 测量字符串的 GDI 严格步进宽度（像素）。底层为 GetTextExtentPoint32，
        /// 与系统 EDIT 控件定位插入符使用同一 API：只含字符推进宽度，
        /// 不含 TextRenderer.MeasureText 末尾预留的安全余量，插入符可紧贴最后一个字形。
        /// </summary>
        public static int TextAdvance(string s, Font font) => TextAdvance(s, font, IntPtr.Zero);

        /// <summary>
        /// 按步进宽度把 s 截断到 avail 像素内并追加省略号“…”；放得下时原样返回。
        /// 供 GDI（ExtTextOut）绘制路径替代 TextFormatFlags.EndEllipsis——后者属 DrawTextEx 体系，
        /// 与 GetTextExtentPoint32 的步进宽度不同源。
        /// </summary>
        public static string EllipsisToFit(string s, Font font, int avail, IntPtr hdc = default)
        {
            if (string.IsNullOrEmpty(s) || avail <= 0) return string.Empty;
            if (TextAdvance(s, font, hdc) <= avail) return s;
            const string ellipsis = "…";
            int ew = TextAdvance(ellipsis, font, hdc);
            if (ew >= avail) return string.Empty;
            int budget = avail - ew;
            int lo = 0, hi = s.Length;
            while (lo < hi)
            {
                int mid = lo + (hi - lo + 1) / 2;
                if (TextAdvance(s.Substring(0, mid), font, hdc) <= budget) lo = mid;
                else hi = mid - 1;
            }
            return lo == 0 ? string.Empty : s.Substring(0, lo) + ellipsis;
        }

        /// <summary>
        /// 在指定 DC 上测量严格步进宽度。跨 DPI（PerMonitorV2 多显示器）时必须传控件自身窗口 DC
        /// （Graphics.FromHwnd(handle).GetHdc()）：屏幕 DC（GetDC(NULL)）按主显示器 DPI 计量，
        /// 控件在异 DPI 显示器上时其宽度与 OnPaint 实际绘制不一致，插入符/IME 候选窗会随字号放大而偏移。
        /// hdc 传 Zero 时退回屏幕 DC（仅建议 96DPI/单显示器场景使用）。
        /// </summary>
        public static int TextAdvance(string s, Font font, IntPtr hdc)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            bool ownDc = false;
            if (hdc == IntPtr.Zero)
            {
                hdc = GetDC(IntPtr.Zero);
                ownDc = true;
            }
            try
            {
                IntPtr hf = font.ToHfont();
                try
                {
                    IntPtr old = SelectObject(hdc, hf);
                    GetTextExtentPoint32W(hdc, s, s.Length, out Size sz);
                    SelectObject(hdc, old);
                    return sz.Width;
                }
                finally { DeleteObject(hf); }
            }
            finally
            {
                if (ownDc) ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        #region IME 拼音组合窗定位

        [StructLayout(LayoutKind.Sequential)]
        private struct ImePoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ImeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CompositionForm
        {
            public int dwStyle;
            public ImePoint ptCurrentPos;
            public ImeRect rcArea;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ImeCharPosition
        {
            public uint dwSize;
            public uint dwCharPosition;
            public ImePoint pt;
            public int cLineHeight;
            public ImeRect rcDocument;
        }

        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        [DllImport("imm32.dll")]
        private static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref CompositionForm lpCompForm);

        private const int CFS_POINT = 0x0002;
        public const int WM_IME_REQUEST = 0x0288;
        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        private const int IMR_QUERYCHARPOSITION = 0x0005;

        /// <summary>
        /// 把 IME 拼音组合窗定位到指定客户区坐标（通常传插入符左下角）。
        /// 原生 EDIT/COMBOBOX 每次移动插入符都会这样告知 IME，拼音条与候选窗便贴在 | 周边。
        /// </summary>
        public static void SetImeCompositionPoint(IntPtr hWnd, Point pt)
        {
            IntPtr hIMC = ImmGetContext(hWnd);
            if (hIMC == IntPtr.Zero) return;
            try
            {
                var form = new CompositionForm
                {
                    dwStyle = CFS_POINT,
                    ptCurrentPos = new ImePoint { X = pt.X, Y = pt.Y }
                };
                ImmSetCompositionWindow(hIMC, ref form);
            }
            finally { ImmReleaseContext(hWnd, hIMC); }
        }

        /// <summary>
        /// 处理 WM_IME_REQUEST / IMR_QUERYCHARPOSITION：现代 TSF 输入法（如微软拼音）
        /// 用该消息查询字符位置来摆放候选窗。命中时填好结构并返回 true，调用方应停止继续分发。
        /// </summary>
        /// <param name="m">窗口消息（Msg 必须为 WM_IME_REQUEST，WParam 为 IMR_QUERYCHARPOSITION）。</param>
        /// <param name="charPoint">字符索引 → 该字符左上角客户区坐标。</param>
        /// <param name="lineHeight">行高（像素）。</param>
        /// <param name="documentRect">文档区域（一般为客户区矩形）。</param>
        public static bool TryAnswerImeCharPosition(ref Message m, Func<int, Point> charPoint,
            int lineHeight, Rectangle documentRect)
        {
            if (m.Msg != WM_IME_REQUEST || m.WParam.ToInt32() != IMR_QUERYCHARPOSITION ||
                m.LParam == IntPtr.Zero)
                return false;

            var data = (ImeCharPosition)Marshal.PtrToStructure(m.LParam, typeof(ImeCharPosition));
            Point p = charPoint((int)data.dwCharPosition);
            data.dwSize = (uint)Marshal.SizeOf(typeof(ImeCharPosition));
            data.pt = new ImePoint { X = p.X, Y = p.Y };
            data.cLineHeight = lineHeight;
            data.rcDocument = new ImeRect
            {
                Left = documentRect.Left,
                Top = documentRect.Top,
                Right = documentRect.Right,
                Bottom = documentRect.Bottom
            };
            Marshal.StructureToPtr(data, m.LParam, false);
            m.Result = (IntPtr)1;
            return true;
        }

        #endregion

        // <summary>
        /// 建立带有圆角样式的路径。
        /// </summary>
        /// <param name="rect">用来建立路径的矩形。</param>
        /// <param name="_radius">圆角的大小。</param>
        /// <param name="style">圆角的样式。</param>
        /// <param name="correction">是否把矩形长宽减 1,以便画出边框。</param>
        /// <returns>建立的路径。</returns>
        public static GraphicsPath CreatePath(Rectangle rect, int radius, HRoundStyle style = HRoundStyle.All, bool correction = true)
        {
            GraphicsPath path = new GraphicsPath();
            int radiusCorrection = correction ? 1 : 0;
            if (radius <= 0)
                style = HRoundStyle.None;
            switch (style)
            {
                case HRoundStyle.None:
                    path.AddRectangle(new Rectangle(rect.X, rect.Y, rect.Width - radiusCorrection, rect.Height - radiusCorrection));
                    break;
                case HRoundStyle.All:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius, 0, 90);
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    break;
                case HRoundStyle.Left:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Y,
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection);
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    break;
                case HRoundStyle.Right:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddArc(
                       rect.Right - radius - radiusCorrection,
                       rect.Bottom - radius - radiusCorrection,
                       radius,
                       radius,
                       0,
                       90);
                    path.AddLine(rect.X, rect.Bottom - radiusCorrection, rect.X, rect.Y);
                    break;
                case HRoundStyle.Top:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection,
                        rect.X, rect.Bottom - radiusCorrection);
                    break;
                case HRoundStyle.Bottom:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        0,
                        90);
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    path.AddLine(rect.X, rect.Y, rect.Right - radiusCorrection, rect.Y);
                    break;
                case HRoundStyle.BottomLeft:
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    path.AddLine(rect.X, rect.Y, rect.Right - radiusCorrection, rect.Y);
                    path.AddLine(
                        rect.Right - radiusCorrection,
                        rect.Y,
                        rect.Right - radiusCorrection,
                        rect.Bottom - radiusCorrection);
                    break;
                case HRoundStyle.BottomRight:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        0,
                        90);
                    path.AddLine(rect.X, rect.Bottom - radiusCorrection, rect.X, rect.Y);
                    path.AddLine(rect.X, rect.Y, rect.Right - radiusCorrection, rect.Y);
                    break;

                case HRoundStyle.TopLeft:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddLine(
                        rect.Right - radiusCorrection,
                        rect.Y,
                        rect.Right - radiusCorrection,
                        rect.Bottom - radiusCorrection);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection,
                        rect.X, rect.Bottom - radiusCorrection);
                    break;
                case HRoundStyle.TopRight:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection,
                        rect.X, rect.Bottom - radiusCorrection);
                    path.AddLine(rect.X, rect.Bottom - radiusCorrection, rect.X, rect.Y);
                    break;
            }
            path.CloseFigure();
            return path;
        }

        // <summary>
        /// 建立带有圆角样式的路径。
        /// </summary>
        /// <param name="rect">用来建立路径的矩形。</param>
        /// <param name="_radius">圆角的大小。</param>
        /// <param name="style">圆角的样式。</param>
        /// <param name="correction">是否把矩形长宽减 1,以便画出边框。</param>
        /// <returns>建立的路径。</returns>
        public static GraphicsPath CreatePath(RectangleF rect, float radius, HRoundStyle style = HRoundStyle.All, bool correction = true)
        {
            GraphicsPath path = new GraphicsPath();
            int radiusCorrection = correction ? 1 : 0;
            if (radius <= 0)
                style = HRoundStyle.None;
            switch (style)
            {
                case HRoundStyle.None:
                    path.AddRectangle(new RectangleF(rect.X, rect.Y, rect.Width - radiusCorrection, rect.Height - radiusCorrection));
                    break;
                case HRoundStyle.All:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius, 0, 90);
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    break;
                case HRoundStyle.Left:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Y,
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection);
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    break;
                case HRoundStyle.Right:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddArc(
                       rect.Right - radius - radiusCorrection,
                       rect.Bottom - radius - radiusCorrection,
                       radius,
                       radius,
                       0,
                       90);
                    path.AddLine(rect.X, rect.Bottom - radiusCorrection, rect.X, rect.Y);
                    break;
                case HRoundStyle.Top:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection,
                        rect.X, rect.Bottom - radiusCorrection);
                    break;
                case HRoundStyle.Bottom:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        0,
                        90);
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    path.AddLine(rect.X, rect.Y, rect.Right - radiusCorrection, rect.Y);
                    break;
                case HRoundStyle.BottomLeft:
                    path.AddArc(
                        rect.X,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        90,
                        90);
                    path.AddLine(rect.X, rect.Y, rect.Right - radiusCorrection, rect.Y);
                    path.AddLine(
                        rect.Right - radiusCorrection,
                        rect.Y,
                        rect.Right - radiusCorrection,
                        rect.Bottom - radiusCorrection);
                    break;
                case HRoundStyle.BottomRight:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Bottom - radius - radiusCorrection,
                        radius,
                        radius,
                        0,
                        90);
                    path.AddLine(rect.X, rect.Bottom - radiusCorrection, rect.X, rect.Y);
                    path.AddLine(rect.X, rect.Y, rect.Right - radiusCorrection, rect.Y);
                    break;

                case HRoundStyle.TopLeft:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddLine(
                        rect.Right - radiusCorrection,
                        rect.Y,
                        rect.Right - radiusCorrection,
                        rect.Bottom - radiusCorrection);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection,
                        rect.X, rect.Bottom - radiusCorrection);
                    break;
                case HRoundStyle.TopRight:
                    path.AddArc(
                        rect.Right - radius - radiusCorrection,
                        rect.Y,
                        radius,
                        radius,
                        270,
                        90);
                    path.AddLine(
                        rect.Right - radiusCorrection, rect.Bottom - radiusCorrection,
                        rect.X, rect.Bottom - radiusCorrection);
                    path.AddLine(rect.X, rect.Bottom - radiusCorrection, rect.X, rect.Y);
                    break;

            }
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 填充外路径与内路径之间的边框环：内路径（Alternate 奇偶规则）形成空洞。
        /// 边框厚度全部在外轮廓之内，改线宽时外轮廓不动；填充不受 Inset 笔在
        /// 弧+直线通用路径上右/下抗锯齿缺失的影响，四向覆盖对称。
        /// innerPath 传 null 表示线宽已盖满内部，直接填充外路径。
        /// </summary>
        public static void FillBorderRing(Graphics g, GraphicsPath outerPath, GraphicsPath innerPath, Color borderColor)
        {
            using (var ring = new GraphicsPath(FillMode.Alternate))
            {
                ring.AddPath(outerPath, false);
                if (innerPath != null) ring.AddPath(innerPath, false);
                using (var brush = new SolidBrush(borderColor))
                    g.FillPath(brush, ring);
            }
        }

        /// <summary>
        /// 圆角控件在 OnPaintBackground 阶段整块铺父容器底色：矩形外扩 1px。
        /// 双缓冲（WS_EX_COMPOSITED 合成）下从 (0,0) 起填的矩形在顶/左边缘只覆盖半个像素，
        /// 未覆盖部分按黑色底衬合成，圆角四角会压暗出黑晕；外扩后顶/左行列为 100% 不透明。
        /// </summary>
        public static void FillParentBackdrop(Graphics g, float width, float height, Color parentColor)
        {
            using (var brush = new SolidBrush(parentColor))
                g.FillRectangle(brush, -1f, -1f, width + 2f, height + 2f);
        }

        /// <summary>
        /// 控件外形外轮廓路径：矩形对齐到像素盒 (-0.5,-0.5)-(W-0.5,H-0.5)，
        /// 圆角半径取 radius+0.5。该口径下填充/环带边缘正好压在像素分界上，
        /// 直边得到全实像素、无抗锯齿虚边，且四边四向对称。
        /// </summary>
        public static GraphicsPath CreateOuterBoxPath(float width, float height, float radius)
        {
            var box = new RectangleF(-0.5f, -0.5f, width, height);
            return radius > 0
                ? CreatePath(box, radius + 0.5f, HRoundStyle.All, false)
                : CreatePath(box, 0, HRoundStyle.None, false);
        }

        /// <summary>
        /// 填充圆角边框环：外轮廓固定在控件像素盒边缘 (-0.5,-0.5)-(W-0.5,H-0.5)，
        /// 边框厚度 borderWidth 全部向内（外路径挖空内路径，Alternate 奇偶填充）。
        /// 改线宽外轮廓不动；直边为全实像素，四边等宽，圆角弧线四向对称。
        /// </summary>
        public static void FillRoundedBorder(Graphics g, float width, float height, float radius, float borderWidth, Color borderColor)
        {
            if (borderWidth <= 0 || radius <= 0) return;
            using (GraphicsPath outer = CreateOuterBoxPath(width, height, radius))
            {
                float iw = width - borderWidth * 2f, ih = height - borderWidth * 2f;
                if (iw <= 0 || ih <= 0)
                {
                    using (var brush = new SolidBrush(borderColor))
                        g.FillPath(brush, outer);
                    return;
                }
                // 内轮廓像素盒 (bw-0.5,..)，弧半径 = 外半径-bw，角部环宽与直边一致
                var innerRect = new RectangleF(borderWidth - 0.5f, borderWidth - 0.5f, iw, ih);
                using (GraphicsPath inner = CreatePath(innerRect, radius + 0.5f - borderWidth, HRoundStyle.All, false))
                    FillBorderRing(g, outer, inner, borderColor);
            }
        }


        /// <summary>
        /// 建立“向内描边”的边框中心线路径：中心线在半个线宽之外再内收半像素（d=线宽/2+0.5），
        /// 圆角处弧线圆心与外边保持一致、半径减 d，得到真正的等距偏移曲线。
        /// 多收的半像素让笔芯落在完整像素列上，描边外沿不会越过控件边界被裁剪，
        /// 避免 1px 边框直边被裁成半透明细线、与圆角弧线深浅不一。
        /// </summary>
        /// <param name="width">控件像素宽（外沿右边界）。</param>
        /// <param name="height">控件像素高（外沿下边界）。</param>
        /// <param name="radius">外圆角半径，0 或负数为直角。</param>
        /// <param name="borderWidth">描边笔宽。</param>
        public static GraphicsPath CreateFramePath(float width, float height, float radius, float borderWidth)
        {
            var path = new GraphicsPath();
            float d = borderWidth / 2f + 0.5f;
            float ri = Math.Max(0, radius - d);
            if (radius <= 0 || ri <= 0)
            {
                // 直角或线宽超过圆角直径：退化为居中内缩矩形
                path.AddRectangle(new RectangleF(d, d, width - d * 2, height - d * 2));
                return path;
            }

            float r = radius;
            // 顺序：上直边 → 右上弧 → 右直边 → 右下弧 → 下直边 → 左下弧 → 左直边 → 左上弧，首尾严丝合缝
            path.AddLine(r, d, width - r, d);
            path.AddArc(width - r - ri, r - ri, ri * 2, ri * 2, 270, 90);
            path.AddLine(width - d, r + ri, width - d, height - r - ri);
            path.AddArc(width - r - ri, height - r - ri, ri * 2, ri * 2, 0, 90);
            path.AddLine(width - r, height - d, r, height - d);
            path.AddArc(r - ri, height - r - ri, ri * 2, ri * 2, 90, 90);
            path.AddLine(d, height - r - ri, d, r + ri);
            path.AddArc(r - ri, r - ri, ri * 2, ri * 2, 180, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 获取文本在矩形框中的居中位置
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static RectangleF GetTextCenterInRect(string text, Font font, RectangleF rect)
        {
            //Bitmap bmp = new Bitmap((int)rect.Width, (int)rect.Height);
            //Graphics g = Graphics.FromImage(bmp);
            SizeF fontsize = TextRenderer.MeasureText(text, font);

            PointF centerPoint = new PointF(rect.Left + (rect.Width - fontsize.Width) / 2, rect.Top + (rect.Height - fontsize.Height) / 2);

            //g.Dispose();
            //bmp.Dispose();

            return new RectangleF(centerPoint, fontsize);
        }

        /// <summary>
        /// 获取文本在矩形框中的位置
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="rect"></param>
        /// <param name="textAlign"></param>
        /// <returns></returns>
        public static RectangleF GetTextRectInRect(string text, Font font, RectangleF rect, ContentAlignment textAlign)
        {
            Bitmap bmp = new Bitmap((int)rect.Width, (int)rect.Height);
            Graphics g = Graphics.FromImage(bmp);
            SizeF fontsize = g.MeasureString(text, font);
            PointF textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width) / 2, rect.Top + (rect.Height - fontsize.Height) / 2); ;
            switch (textAlign)
            {
                case ContentAlignment.MiddleLeft: textPoint = new PointF(rect.Left, rect.Top + (rect.Height - fontsize.Height) / 2); break;
                case ContentAlignment.MiddleRight: textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width), rect.Top + (rect.Height - fontsize.Height) / 2); break;
                case ContentAlignment.MiddleCenter: textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width) / 2, rect.Top + (rect.Height - fontsize.Height) / 2); break;
                case ContentAlignment.TopLeft: textPoint = new PointF(rect.Left, rect.Top); break;
                case ContentAlignment.TopRight: textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width), rect.Top); break;
                case ContentAlignment.TopCenter: textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width) / 2, rect.Top); break;
                case ContentAlignment.BottomLeft: textPoint = new PointF(rect.Left, rect.Top + (rect.Height - fontsize.Height)); break;
                case ContentAlignment.BottomRight: textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width), rect.Top + (rect.Height - fontsize.Height)); break;
                case ContentAlignment.BottomCenter: textPoint = new PointF(rect.Left + (rect.Width - fontsize.Width) / 2, rect.Top + (rect.Height - fontsize.Height)); break;
            }

            return new RectangleF(textPoint, fontsize);
        }

        /// <summary>
        /// 获取文本对应字体的大小
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static SizeF GetTextSize(string text, Font font)
        {
            Bitmap bmp = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(bmp);
            SizeF fontsize = g.MeasureString(text, font);

            g.Dispose();
            bmp.Dispose();

            return fontsize;
        }

        /// <summary>
        /// 获取一个固定大小矩形在一个矩形中心的Rect
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static RectangleF GetCenterRectInRect(RectangleF rect, SizeF size)
        {
            PointF centerPoint = new PointF(rect.Left + (rect.Width - size.Width) / 2, rect.Top + (rect.Height - size.Height) / 2);
            return new RectangleF(centerPoint, size);
        }

        /// <summary>
        /// 获取矩形的中心点
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Point GetCenterPointInRect(Rectangle rect)
        {
            return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        /// <summary>
        /// 将图片转为圆形
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="transparentColor">要转为圆形周围的透明颜色，确保图片中没有此颜色</param>
        /// <returns></returns>
        public static Bitmap GetRoundBmp(Bitmap bmp, Color transparentColor)
        {
            Bitmap bitmap = bmp.Clone() as Bitmap;
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(new Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1));
            path.AddRectangle(new Rectangle(-1, -1, bitmap.Width + 1, bitmap.Height + 1));
            SolidBrush solidBrush = new SolidBrush(transparentColor);
            g.FillPath(solidBrush, path);
            bitmap.MakeTransparent(transparentColor);
            path.Dispose();
            g.Dispose();
            solidBrush.Dispose();
            return bitmap;
        }

        /// <summary>
        /// 剪切图片为正方形，以最短的边长为边长，位置居中
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap CutBmpToSquare(Bitmap bmp)
        {
            Rectangle rectangle;
            if (bmp.Width > bmp.Height)
            {
                rectangle = new Rectangle((bmp.Width - bmp.Height) / 2, 0, bmp.Height, bmp.Height);
            }
            else if (bmp.Width < bmp.Height)
            {
                rectangle = new Rectangle(0, (bmp.Height - bmp.Width) / 2, bmp.Width, bmp.Width);
            }
            else
            {
                return bmp;
            }

            return bmp.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// 图片放大镜，将图片像素放大显示其像素位图
        /// </summary>
        /// <param name="srcbitmap">源图片</param>
        /// <param name="multiple">放大倍数</param>
        /// <returns></returns>
        public static Bitmap BitmapMagnifier(Bitmap srcbitmap, int multiple)
        {
            if (multiple <= 0) { multiple = 0; return srcbitmap; }
            Bitmap bitmap = new Bitmap(srcbitmap.Size.Width * multiple, srcbitmap.Size.Height * multiple);
            BitmapData srcbitmapdata = srcbitmap.LockBits(new Rectangle(new Point(0, 0), srcbitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* srcbyte = (byte*)(srcbitmapdata.Scan0.ToPointer());
                byte* sourcebyte = (byte*)(bitmapdata.Scan0.ToPointer());
                for (int y = 0; y < bitmapdata.Height; y++)
                {
                    for (int x = 0; x < bitmapdata.Width; x++)
                    {
                        long index = (x / multiple) * 4 + (y / multiple) * srcbitmapdata.Stride;
                        sourcebyte[0] = srcbyte[index];
                        sourcebyte[1] = srcbyte[index + 1];
                        sourcebyte[2] = srcbyte[index + 2];
                        sourcebyte[3] = srcbyte[index + 3];
                        sourcebyte += 4;
                    }
                }
            }
            srcbitmap.UnlockBits(srcbitmapdata);
            bitmap.UnlockBits(bitmapdata);
            return bitmap;
        }
    }
}
