using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;
namespace HFromUI.HControl.Tools.DateTime
{
    using HFromUI.HLangage;
    using DateTime = System.DateTime;
    /// <summary>
    /// 日期范围选择控件：一个圆角外框内嵌两个无边框日期框（yyyy-MM-dd 至 yyyy-MM-dd），
    /// 右侧箭头展开单个月历，先点开始日、翻到目标月份再点结束日，区间橙色色带连通并带悬停预览。
    /// 任一端手动编辑越过另一端时自动把另一端拉齐，保证开始日期不晚于结束日期。
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("StartValue")]
    public class HDateRangePicker : Control
    {
        private const int ArrowW = 28;
        private readonly HDatePicker _startBox;
        private readonly HDatePicker _endBox;
        private HCalendarDrop _drop;
        private int _radius = 6;
        private int _borderWidth = 1;
        private Color _shapeBackColor = Color.Empty;
        private bool _hover;
        private bool _arrowPressed;
        private bool _sync;
        public HDateRangePicker()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 9f);
            Size = new Size(372, 30);
            _startBox = CreateBox();
            _endBox = CreateBox();
            Controls.Add(_startBox);
            Controls.Add(_endBox);
            DateTime today = DateTime.Today;
            _startBox.Value = today.AddDays(-7);
            _endBox.Value = today;
            _startBox.ValueChanged += (s, e) => OnStartEdited();
            _endBox.ValueChanged += (s, e) => OnEndEdited();
            _startBox.DropClicked += (s, e) => OpenDrop();
            _endBox.DropClicked += (s, e) => OpenDrop();
            LayoutBoxes();
        }
        private HDatePicker CreateBox()
        {
            var box = new HDatePicker
            {
                Borderless = true,
                ShowDropArrow = false,
                ShowCalendar = false,
                TabStop = true
            };
            box.GotFocus += (s, e) => Invalidate();
            box.LostFocus += (s, e) => Invalidate();
            box.PopupCloseRequested += (s, e) => CloseDrop();
            return box;
        }
        /// <summary>开始日期。</summary>
        public DateTime StartValue
        {
            get => _startBox.Value;
            set => _startBox.Value = value.Date;
        }
        /// <summary>结束日期（不早于开始日期）。</summary>
        public DateTime EndValue
        {
            get => _endBox.Value;
            set => _endBox.Value = value.Date;
        }
        /// <summary>任一端日期变化后触发。</summary>
        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (!_sync) ValueChanged?.Invoke(this, e);
        }
        /// <summary>强调色与内嵌日期框同源（橙色主调）。</summary>
        public Color AccentColor
        {
            get => _startBox.AccentColor;
            set { _startBox.AccentColor = value; _endBox.AccentColor = value; Invalidate(); }
        }
        [DefaultValue(6)]
        public int Radius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
        }
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = Math.Max(1, value); UpdateRegion(); Invalidate(); }
        }
        /// <summary>圆角外形内部的背景色；Color.Empty 表示沿用 BackColor，颜色只在外形内、圆角外不染色。</summary>
        [DefaultValue(typeof(Color), "")]
        public Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }
        [Browsable(false)]
        public bool IsPopupOpen => _drop != null && _drop.IsOpen;
        #region 顺序约束
        private void OnStartEdited()
        {
            if (_startBox.Value > _endBox.Value)
            {
                _sync = true;
                _endBox.Value = _startBox.Value;
                _sync = false;
            }
            // 单月日历打开时手动改了内嵌框（非日历回写）：弹层状态已陈旧，收起
            if (!_sync && IsPopupOpen) CloseDrop();
            OnValueChanged(EventArgs.Empty);
        }
        private void OnEndEdited()
        {
            if (_endBox.Value < _startBox.Value)
            {
                _sync = true;
                _startBox.Value = _endBox.Value;
                _sync = false;
            }
            if (!_sync && IsPopupOpen) CloseDrop();
            OnValueChanged(EventArgs.Empty);
        }
        #endregion
        #region 布局
        private int BoxWidth() => HDrawPaint.TextAdvance("yyyy-MM-dd", Font) + 20;
        private int SepWidth() => HDrawPaint.TextAdvance(HTranslation.GetContent("至"), Font) + 18;
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutBoxes();
            UpdateRegion();
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            LayoutBoxes();
            UpdateRegion();
        }
        private void LayoutBoxes()
        {
            int boxW = BoxWidth();
            int sepW = SepWidth();
            int x1 = 8;
            int xSep = x1 + boxW;
            int x2 = xSep + sepW;
            int rightLimit = Width - ArrowW - 4;
            if (_startBox != null)
                _startBox.Bounds = new Rectangle(x1, 0, Math.Min(boxW, rightLimit - x1), Height);
            if (_endBox != null)
            {
                int w2 = Math.Min(boxW, rightLimit - x2);
                _endBox.Bounds = new Rectangle(x2, 0, Math.Max(0, w2), Height);
            }
        }
        private Rectangle SeparatorRect
        {
            get
            {
                int x = 8 + BoxWidth();
                return new Rectangle(x, 0, SepWidth(), Height);
            }
        }
        private Rectangle ArrowRect => new Rectangle(Width - ArrowW, 0, ArrowW, Height);
        #endregion
        #region 鼠标与弹出层
        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _arrowPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && ArrowRect.Contains(e.Location))
            {
                _arrowPressed = true;
                Capture = true;
                Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_arrowPressed)
            {
                bool inside = ArrowRect.Contains(e.Location);
                _arrowPressed = false;
                Capture = false;
                Invalidate();
                if (inside) ToggleDrop();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = ArrowRect.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
        }
        private void ToggleDrop()
        {
            if (IsPopupOpen) CloseDrop();
            else OpenDrop();
        }
        private void OpenDrop()
        {
            CloseDrop();
            _drop = new HCalendarDrop(true, _startBox.Value, _endBox.Value);
            _drop.RangePicked += (s, e) =>
            {
                _sync = true;
                _startBox.Value = e.Start;
                _endBox.Value = e.End;
                _sync = false;
                OnValueChanged(EventArgs.Empty);
            };
            _drop.PopupClosed += (s, e) =>
            {
                _startBox.IsPopupOpen = false;
                _endBox.IsPopupOpen = false;
                var d = _drop;
                _drop = null;
                // 关闭回调正执行在弹层自己的调用栈上，借弹层句柄延后释放，避免重入 Dispose
                if (d != null) d.BeginInvoke(new Action(() => d.Dispose()));
                Invalidate();
            };
            _drop.ShowPopup(this, _drop.Size);
            _startBox.IsPopupOpen = true;
            _endBox.IsPopupOpen = true;
            Invalidate();
        }
        private void CloseDrop()
        {
            var d = _drop;
            if (d == null) return;
            // PopupClosed 回调会把 _drop 置空并安排释放
            d.ClosePopup();
            _startBox.IsPopupOpen = false;
            _endBox.IsPopupOpen = false;
            Invalidate();
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            CloseDrop();
            base.OnHandleDestroyed(e);
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            Keys code = keyData & Keys.KeyCode;
            if (IsPopupOpen && (code == Keys.Escape || code == Keys.Enter))
            {
                CloseDrop();
                return true;
            }
            if (keyData == (Keys.Alt | Keys.Down) || code == Keys.F4)
            {
                ToggleDrop();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
        #endregion
        #region 绘制
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (_radius <= 0)
            {
                base.OnPaintBackground(pevent);
                if (Enabled && !_shapeBackColor.IsEmpty)
                    using (var sb = new SolidBrush(_shapeBackColor))
                        pevent.Graphics.FillRectangle(sb, ClientRectangle);
                return;
            }
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            HDrawPaint.FillParentBackdrop(g, Width, Height, Parent?.BackColor ?? SystemColors.Control);
            Color fill = Enabled ? (_shapeBackColor.IsEmpty ? BackColor : _shapeBackColor)
                                 : Color.FromArgb(245, 246, 248);
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            using (var bb = new SolidBrush(fill))
                g.FillPath(bb, path);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            bool active = _startBox.Focused || _endBox.Focused || IsPopupOpen;
            Color border = !Enabled ? Color.FromArgb(224, 226, 230) :
                active ? AccentColor :
                _hover ? Color.FromArgb(249, 166, 99) : Color.FromArgb(214, 217, 223);
            if (_radius <= 0)
            {
                using (var pen = new Pen(border, _borderWidth))
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
            else
            {
                HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, _borderWidth, border);
            }
            // 分隔字“至”
            TextRenderer.DrawText(g, HTranslation.GetContent("至"), Font, SeparatorRect, Color.FromArgb(150, 154, 162),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            DrawArrow(g);
        }
        private void DrawArrow(Graphics g)
        {
            var z = ArrowRect;
            if (_arrowPressed || IsPopupOpen)
            {
                var bg = new Rectangle(z.X + 2, 3, z.Width - 4, Height - 6);
                using (GraphicsPath pp = HDrawPaint.CreatePath(bg, Math.Max(2, _radius - 3), HRoundStyle.All, false))
                using (var pb = new SolidBrush(IsPopupOpen ? Color.FromArgb(30, AccentColor) : Color.FromArgb(20, 0, 0, 0)))
                    g.FillPath(pb, pp);
            }
            int cx = z.X + z.Width / 2;
            int cy = Height / 2;
            Color ink = IsPopupOpen || _hover ? AccentColor : Color.FromArgb(120, 124, 130);
            using (var pen = new Pen(ink, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                Point[] tri = IsPopupOpen
                    ? new[] { new Point(cx - 5, cy + 2), new Point(cx, cy - 3), new Point(cx + 5, cy + 2) }
                    : new[] { new Point(cx - 5, cy - 2), new Point(cx, cy + 3), new Point(cx + 5, cy - 2) };
                g.DrawLines(pen, tri);
            }
        }
        // 圆角四角由 OnPaintBackground 铺父色透出，不再裁剪窗口 Region（HRgn 会削弧线出折角）
        private void UpdateRegion()
        {
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) CloseDrop();
            base.Dispose(disposing);
        }
        #endregion
    }
}
