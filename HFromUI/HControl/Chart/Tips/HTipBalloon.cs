using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HData.Win;

namespace HFromUI.HControl.Chart.Tips
{
    /// <summary>
    /// 跨控件悬停气泡：复用 HChart 悬停气泡渲染体系（Classic 外形 + Auto 配色，由 HTipShape 绘制），
    /// 承载于 WS_EX_LAYERED 真透明顶层窗——圆角与半透明底与图表内气泡观感一致，且不抢焦点、不显示在任务栏。
    /// 全局单例 <see cref="Default"/> 供任意控件共用，同一时刻只显示一个气泡；
    /// 时序对齐 System.Windows.Forms.ToolTip：悬停 <see cref="InitialDelay"/> 后出现，
    /// 停留 <see cref="AutoPopDelay"/> 自动消失，控件重复请求同一内容不打断计时，离开立即消失。
    /// </summary>
    public sealed class HTipBalloon : IDisposable
    {
        /// <summary>全局共享实例（同一时刻只可能有一个悬停气泡）。</summary>
        public static readonly HTipBalloon Default = new HTipBalloon();

        /// <summary>悬停出现延迟（毫秒，对应 ToolTip.InitialDelay）。</summary>
        public int InitialDelay = 1000;
        /// <summary>气泡自动消失时长（毫秒，对应 ToolTip.AutoPopDelay）。</summary>
        public int AutoPopDelay = 2700;

        /// <summary>出现延迟定时器。</summary>
        private readonly Timer _delayTimer;
        /// <summary>自动消失定时器。</summary>
        private readonly Timer _popTimer;
        /// <summary>分层承载窗（首次显示时懒创建，之后复用）。</summary>
        private TipHostForm _host;
        /// <summary>当前请求归属控件。</summary>
        private Control _owner;
        /// <summary>当前请求文本。</summary>
        private string _text;
        /// <summary>气泡是否已弹出。</summary>
        private bool _popped;

        /// <summary>私有构造：单例。</summary>
        private HTipBalloon()
        {
            _delayTimer = new Timer();
            _delayTimer.Tick += DelayTimer_Tick;
            _popTimer = new Timer();
            _popTimer.Tick += PopTimer_Tick;
        }

        /// <summary>
        /// 请求显示气泡（通常由控件的轮询刷新在鼠标悬停时反复调用）。
        /// 重复请求同一控件同一文本时保持现状（等待中不重置延迟、显示中不提前消失）；
        /// 控件或文本变化时重新计时；文本为空时等同于 <see cref="Hide()"/>。
        /// </summary>
        /// <param name="owner">悬停归属控件（销毁时气泡自动消失）。</param>
        /// <param name="text">气泡文本（支持 \n 多行）。</param>
        public void Show(Control owner, string text)
        {
            if (owner == null || string.IsNullOrEmpty(text))
            {
                Hide();
                return;
            }
            bool same = _owner == owner && _text == text;
            if (same && (_popped || _delayTimer.Enabled)) return;
            if (!same)
            {
                BindOwner(owner);
                _text = text;
                if (_popped && _host != null) _host.Hide();   // 旧内容先撤，等新延迟
                _popped = false;
            }
            _popTimer.Stop();
            _delayTimer.Stop();
            _delayTimer.Interval = InitialDelay;
            _delayTimer.Start();
        }

        /// <summary>立即隐藏气泡并取消所有计时（鼠标离开归属控件时调用）。</summary>
        public void Hide()
        {
            _delayTimer.Stop();
            _popTimer.Stop();
            if (_host != null && _host.Visible) _host.Hide();
            UnbindOwner();
            _text = null;
            _popped = false;
        }

        /// <summary>仅当气泡归属于指定控件时隐藏（多个悬停区域之间互不误熄）。</summary>
        /// <param name="owner">调用方控件。</param>
        public void Hide(Control owner)
        {
            if (_owner == owner) Hide();
        }

        /// <summary>延迟到期：在当前鼠标热点处弹出气泡。</summary>
        private void DelayTimer_Tick(object sender, EventArgs e)
        {
            _delayTimer.Stop();
            if (_owner == null || _owner.IsDisposed || string.IsNullOrEmpty(_text)) return;
            if (_host == null) _host = new TipHostForm();
            _host.Present(_text, Control.MousePosition);
            _popped = true;
            _popTimer.Interval = AutoPopDelay;
            _popTimer.Start();
        }

        /// <summary>停留到期：自动消失（保持已弹出状态，重复 Show 同一内容不再复出，与 ToolTip 一致）。</summary>
        private void PopTimer_Tick(object sender, EventArgs e)
        {
            _popTimer.Stop();
            if (_host != null && _host.Visible) _host.Hide();
        }

        /// <summary>绑定新归属控件（旧控件解除订阅）。</summary>
        private void BindOwner(Control owner)
        {
            UnbindOwner();
            _owner = owner;
            owner.HandleDestroyed += Owner_HandleDestroyed;
        }

        /// <summary>解除归属控件订阅。</summary>
        private void UnbindOwner()
        {
            if (_owner != null)
            {
                _owner.HandleDestroyed -= Owner_HandleDestroyed;
                _owner = null;
            }
        }

        /// <summary>归属控件句柄销毁：立即清理。</summary>
        private void Owner_HandleDestroyed(object sender, EventArgs e)
        {
            _delayTimer.Stop();
            _popTimer.Stop();
            if (_host != null && _host.Visible) _host.Hide();
            UnbindOwner();
            _text = null;
            _popped = false;
        }

        /// <summary>释放定时器与承载窗。</summary>
        public void Dispose()
        {
            _delayTimer.Dispose();
            _popTimer.Dispose();
            if (_host != null)
            {
                _host.Dispose();
                _host = null;
            }
        }

        /// <summary>
        /// 气泡分层承载窗：WS_EX_LAYERED + UpdateLayeredWindow（32 位 ARGB 位图，ULW_ALPHA）实现真透明圆角；
        /// WS_EX_NOACTIVATE + WS_EX_TOOLWINDOW 保证不抢焦点、不入任务栏/Alt+Tab。
        /// </summary>
        private sealed class TipHostForm : Form
        {
            /// <summary>WS_EX_LAYERED：分层窗。</summary>
            private const int WsExLayered = 0x00080000;
            /// <summary>WS_EX_NOACTIVATE：显示不激活。</summary>
            private const int WsExNoActivate = 0x08000000;
            /// <summary>WS_EX_TOOLWINDOW：工具窗（不入任务栏/Alt+Tab）。</summary>
            private const int WsExToolWindow = 0x00000080;
            /// <summary>行高（与 HTipPopup 一致）。</summary>
            private const float LineHeight = 16f;
            /// <summary>气泡距鼠标锚点间距（与 HTipPopup 一致）。</summary>
            private const float Gap = 6f;
            /// <summary>气泡字体（与 HChart.RulerFont 一致：Segoe UI 8.5）。</summary>
            private static readonly Font TipFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);

            /// <summary>最近一次呈现的位图（同内容重定位时直接复用）。</summary>
            private Bitmap _bitmap;

            /// <summary>构造分层工具窗。</summary>
            public TipHostForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                Size = new Size(1, 1);
            }

            /// <summary>分层 + 不激活 + 工具窗扩展样式。</summary>
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= WsExLayered | WsExNoActivate | WsExToolWindow;
                    return cp;
                }
            }

            /// <summary>显示时不夺取归属控件焦点。</summary>
            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }

            /// <summary>
            /// 渲染文本并把窗口定位到屏幕鼠标热点旁（右下展开，靠右/靠下越界翻转，整体夹在屏幕工作区内），
            /// 定位与配色规则与 HTipPopup.Render 完全一致。
            /// </summary>
            /// <param name="content">气泡文本（\n 拆多行）。</param>
            /// <param name="mouseScreen">鼠标热点（屏幕坐标）。</param>
            public void Present(string content, Point mouseScreen)
            {
                List<string> lines = SplitLines(content);
                HTipColors pc = HTipPalettes.Get(HTipSchemeKind.Auto, false);
                HTSkinDef def = HTipSkins.Get(HTipSkinKind.Classic);

                float dpiX;
                using (Graphics screen = Graphics.FromHwnd(IntPtr.Zero))
                    dpiX = screen.DpiX;

                // 1) 量正文（TextRenderer，与 HTipPopup 同规则）
                float cw = 0f;
                foreach (string line in lines)
                    cw = Math.Max(cw, TextRenderer.MeasureText(line, TipFont).Width);
                float ch = lines.Count * LineHeight;
                SizeF size = HTipShape.Measure(def, cw, ch, false);
                int w = Math.Max(1, (int)Math.Ceiling(size.Width));
                int h = Math.Max(1, (int)Math.Ceiling(size.Height));

                // 2) 屏幕定位：默认右下展开，越界翻转，再夹到工作区 2px 内
                Rectangle area = Screen.FromPoint(mouseScreen).WorkingArea;
                float bx = mouseScreen.X + Gap;
                float by = mouseScreen.Y + Gap;
                if (bx + w > area.Right - 2f) bx = mouseScreen.X - Gap - w;
                if (by + h > area.Bottom - 2f) by = mouseScreen.Y - Gap - h;
                bx = Math.Max(area.Left + 2f, Math.Min(bx, area.Right - w - 2f));
                by = Math.Max(area.Top + 2f, Math.Min(by, area.Bottom - h - 2f));
                Bounds = new Rectangle((int)bx, (int)by, w, h);

                // 3) 渲染到 32 位 ARGB 位图（位图分辨率对齐屏幕 DPI，Point 字号与图表画布等大）
                Bitmap oldBitmap = _bitmap;
                _bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                _bitmap.SetResolution(dpiX, dpiX);
                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    // 分层窗位图上 ClearType 会按黑底子像素混合产生彩边，统一用灰度抗锯齿
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    RectangleF box = new RectangleF(0, 0, w, h);
                    HTipShape.Paint(g, def, box, HTTail.None, pc);
                    RectangleF body = HTipShape.BodyRect(def, box, HTTail.None);
                    PointF ori = HTipShape.TextOrigin(def, cw, ch, body, false);
                    for (int i = 0; i < lines.Count; i++)
                        TextRenderer.DrawText(g, lines[i], TipFont,
                            new Point((int)ori.X, (int)(ori.Y + i * LineHeight)), pc.Text);
                }
                SetBits(_bitmap);
                if (oldBitmap != null) oldBitmap.Dispose();
                if (!Visible) Show();
            }

            /// <summary>按 \n 拆行（吞掉 \r），空文本至少一行。</summary>
            private static List<string> SplitLines(string content)
            {
                var lines = new List<string>();
                foreach (string raw in content.Split('\n'))
                    lines.Add(raw.TrimEnd('\r'));
                if (lines.Count == 0) lines.Add(string.Empty);
                return lines;
            }

            /// <summary>UpdateLayeredWindow 提交位图（与 HUpdateLayeredForm.SetBits 同模式）。</summary>
            private void SetBits(Bitmap bitmap)
            {
                IntPtr screenDc = HWin32.GetDC(IntPtr.Zero);
                IntPtr memDc = HWin32.CreateCompatibleDC(screenDc);
                IntPtr hBitmap = IntPtr.Zero;
                IntPtr oldBits = IntPtr.Zero;
                try
                {
                    hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                    oldBits = HWin32.SelectObject(memDc, hBitmap);
                    HWin32.Point topLoc = new HWin32.Point(Left, Top);
                    HWin32.Size bitMapSize = new HWin32.Size(Width, Height);
                    HWin32.BLENDFUNCTION blend = new HWin32.BLENDFUNCTION
                    {
                        BlendOp = HWin32.AC_SRC_OVER,
                        SourceConstantAlpha = 255,
                        AlphaFormat = HWin32.AC_SRC_ALPHA,
                        BlendFlags = 0
                    };
                    HWin32.Point srcLoc = new HWin32.Point(0, 0);
                    HWin32.UpdateLayeredWindow(Handle, screenDc, ref topLoc, ref bitMapSize,
                        memDc, ref srcLoc, 0, ref blend, HWin32.ULW_ALPHA);
                }
                finally
                {
                    if (hBitmap != IntPtr.Zero)
                    {
                        HWin32.SelectObject(memDc, oldBits);
                        HWin32.DeleteObject(hBitmap);
                    }
                    HWin32.ReleaseDC(IntPtr.Zero, screenDc);
                    HWin32.DeleteDC(memDc);
                }
            }

            /// <summary>窗销毁时释放静态字体外的位图资源。</summary>
            protected override void Dispose(bool disposing)
            {
                if (disposing && _bitmap != null)
                {
                    _bitmap.Dispose();
                    _bitmap = null;
                }
                base.Dispose(disposing);
            }
        }
    }
}
