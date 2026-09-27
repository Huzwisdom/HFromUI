using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{
    /// <summary>
    /// 弹出层基类：无边框、不夺焦、不在任务栏显示图标，点击弹层外部或所属窗体失活时自动关闭，
    /// 下方空间不足时自动翻转到锚点上方。统一负责圆角外形、白底与描边，子类只绘制内容。
    /// 典型用途：日历、下拉面板等需要保留键盘焦点在触发控件上的浮层。
    /// </summary>
    public abstract class HDropDownPopup : Form
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        private Control _anchor;
        private Form _ownerForm;
        private readonly Timer _watch;
        private bool _prevDown;
        private int _radius = 6;
        private Color _shapeBackColor = Color.Empty;
        private OutsideClickFilter _outsideFilter;
        private bool _filterInstalled;

        protected HDropDownPopup()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            Font = new Font("微软雅黑", 9f);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _watch = new Timer { Interval = 12 };
            _watch.Tick += WatchOutside;
        }

        /// <summary>不夺取焦点，键盘消息继续发给锚点控件。</summary>
        protected override bool ShowWithoutActivation => true;

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEACTIVATE = 0x0021;
            const int MA_NOACTIVATE = 3;
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                // WS_EX_NOACTIVATE 只阻止“打开弹层”时激活；真实鼠标点击弹层仍会走 WM_MOUSEACTIVATE，
                // WinForms 默认返回 MA_ACTIVATE，会把键盘焦点从锚点控件抢到弹层（主窗体随之 Deactivate），
                // 此后 Esc/↑↓/键入全部失效。显式不激活，点击照常派发，焦点始终留在锚点控件。
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                return cp;
            }
        }

        /// <summary>圆角半径（0 为直角）。</summary>
        [DefaultValue(6)]
        public int Radius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); ApplyRegion(); }
        }

        /// <summary>弹层描边色。</summary>
        public Color PopupBorderColor { get; set; } = Color.FromArgb(214, 217, 223);

        /// <summary>圆角外形内部的背景色；Color.Empty 表示沿用 BackColor，颜色只在外形内、圆角外不染色。</summary>
        [DefaultValue(typeof(Color), "")]
        public Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }

        /// <summary>触发本弹层的锚点控件。</summary>
        protected Control AnchorControl => _anchor;

        /// <summary>弹层是否处于打开可见状态。</summary>
        public bool IsOpen => Visible;

        /// <summary>弹层关闭后触发（外部点击、失活或主动关闭都会触发一次）。</summary>
        public event EventHandler PopupClosed;

        /// <summary>在锚点控件下方弹出（空间不足翻上方，宽度显式给定）。</summary>
        public void ShowPopup(Control anchor, Size size)
        {
            _anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            if (size.Width <= 0 || size.Height <= 0) throw new ArgumentOutOfRangeException(nameof(size));

            Size = size;
            ApplyRegion();

            Point topLeft = anchor.PointToScreen(Point.Empty);
            Point bottom = anchor.PointToScreen(new Point(0, anchor.Height));
            Rectangle work = Screen.FromControl(anchor).WorkingArea;

            int x = topLeft.X;
            int y = bottom.Y + 1;
            if (y + size.Height > work.Bottom)
            {
                int aboveY = topLeft.Y - size.Height - 1;
                y = aboveY >= work.Top ? aboveY : Math.Max(work.Top, work.Bottom - size.Height);
            }
            if (x + size.Width > work.Right) x = work.Right - size.Width;
            if (x < work.Left) x = work.Left;
            Location = new Point(x, y);

            _ownerForm = anchor.FindForm();
            if (_ownerForm != null) _ownerForm.Deactivate += OwnerFormDeactivate;

            Show(anchor);
            _watch.Start();
            if (!_filterInstalled)
            {
                _outsideFilter = new OutsideClickFilter(this);
                Application.AddMessageFilter(_outsideFilter);
                _filterInstalled = true;
            }
            OnPopupOpened(EventArgs.Empty);
        }

        /// <summary>关闭弹层。</summary>
        public void ClosePopup()
        {
            if (_filterInstalled)
            {
                Application.RemoveMessageFilter(_outsideFilter);
                _outsideFilter = null;
                _filterInstalled = false;
            }
            if (!Visible && !_watch.Enabled) return;
            _watch.Stop();
            if (Visible) Hide();
            if (_ownerForm != null)
            {
                _ownerForm.Deactivate -= OwnerFormDeactivate;
                _ownerForm = null;
            }
            OnPopupClosed(EventArgs.Empty);
        }

        /// <summary>弹层打开后回调（子类可在此做状态初始化）。</summary>
        protected virtual void OnPopupOpened(EventArgs e) { }

        /// <summary>弹层关闭后回调。</summary>
        protected virtual void OnPopupClosed(EventArgs e) => PopupClosed?.Invoke(this, e);

        /// <summary>在弹层与锚点之外按下鼠标：默认直接关闭。</summary>
        protected virtual void OnOutsidePressed() => ClosePopup();

        /// <summary>
        /// 鼠标按下消息过滤器：弹层打开期间，发往本线程其他窗口（锚点之外的任意控件/窗体）
        /// 的左/右键按下即视为外部点击。轮询 GetAsyncKeyState 对极快的合成点击会漏按键沿，
        /// 过滤器随消息派发同步触发，可保证关闭；跨进程点击仍由 OwnerForm 失活兜底。
        /// </summary>
        private sealed class OutsideClickFilter : IMessageFilter
        {
            private readonly HDropDownPopup _owner;

            public OutsideClickFilter(HDropDownPopup owner) { _owner = owner; }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg != 0x0201 && m.Msg != 0x0204) return false; // WM_LBUTTONDOWN / WM_RBUTTONDOWN
                if (!_owner.IsHandleCreated) return false;

                IntPtr target = m.HWnd;
                if (target == IntPtr.Zero) return false;

                bool inPopup = target == _owner.Handle || IsChild(_owner.Handle, target);
                bool inAnchor = _owner._anchor != null && _owner._anchor.IsHandleCreated &&
                                (target == _owner._anchor.Handle || IsChild(_owner._anchor.Handle, target));
                if (!inPopup && !inAnchor)
                    _owner.OnOutsidePressed();
                return false;
            }
        }

        /// <summary>轮询外部鼠标按下（窗口不激活，收不到失焦消息，仿系统下拉行为）。</summary>
        private void WatchOutside(object sender, EventArgs e)
        {
            bool down = (GetAsyncKeyState(0x01) & 0x8000) != 0 ||
                        (GetAsyncKeyState(0x02) & 0x8000) != 0;
            if (down && !_prevDown)
            {
                Point p = Cursor.Position;
                Rectangle anchorScreen = _anchor != null
                    ? _anchor.RectangleToScreen(_anchor.ClientRectangle)
                    : Rectangle.Empty;
                if (!Bounds.Contains(p) && !anchorScreen.Contains(p))
                    OnOutsidePressed();
            }
            _prevDown = down;
        }

        private void OwnerFormDeactivate(object sender, EventArgs e)
        {
            // 点击 WS_EX_NOACTIVATE 弹层时主窗体会先收到一次 Deactivate；
            // 鼠标仍在弹层或锚点内说明是在与弹层交互，不能关（双月区间要连续点两次）。
            Point p = Cursor.Position;
            if (Bounds.Contains(p)) return;
            if (_anchor != null && _anchor.RectangleToScreen(_anchor.ClientRectangle).Contains(p)) return;
            ClosePopup();
        }

        /// <summary>
        /// 圆角外形裁剪（顶层弹窗必须裁剪，否则圆角外露出直角块）。
        /// Region 与 OnPaint 填充共用像素盒外轮廓，直角时还原矩形区域。
        /// </summary>
        private void ApplyRegion()
        {
            if (!IsHandleCreated) return;
            if (_radius <= 0)
            {
                Region = null;
                return;
            }
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
                Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color fill = _shapeBackColor.IsEmpty ? BackColor : _shapeBackColor;
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            using (var bb = new SolidBrush(fill))
                g.FillPath(bb, path);

            // 子类内容由重写方在 base.OnPaint 之后绘制；最后统一描边压住内容边缘
            if (_radius <= 0)
            {
                using (var pen = new Pen(PopupBorderColor, 1f))
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
            else
            {
                // 边框厚度 1px 全部向内（填充环），外轮廓固定像素盒，与 Region 同形
                HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, 1f, PopupBorderColor);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_filterInstalled)
                {
                    Application.RemoveMessageFilter(_outsideFilter);
                    _outsideFilter = null;
                    _filterInstalled = false;
                }
                _watch.Stop();
                _watch.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
