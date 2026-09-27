using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.DateTime
{
    using HFromUI.HLangage;
    using DateTime = System.DateTime;
    /// <summary>
    /// 时间范围选择控件：一个圆角外框内嵌两个无边框时间框（HH:mm:ss.fff 至 HH:mm:ss.fff），
    /// 右侧一组上下微调钮作用于最近获得焦点（或开始端）的时间框；
    /// 各段同样支持 ↑↓、滚轮与直接键入。开始时间晚于结束时间时自动把另一端拉齐。
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("StartValue")]
    public class HTimeRangePicker : Control
    {
        private const int SpinW = 20;

        private readonly HTimePicker _startBox;
        private readonly HTimePicker _endBox;
        private HTimePicker _active;
        private int _radius = 6;
        private int _borderWidth = 1;
        private Color _shapeBackColor = Color.Empty;
        private bool _hover;
        private bool _spinPressed;
        private int _spinDir;
        private bool _spinSlow;
        private bool _sync;
        private readonly Timer _spinTimer;

        public HTimeRangePicker()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 9f);
            Size = new Size(380, 30);

            _startBox = CreateBox();
            _endBox = CreateBox();
            _active = _startBox;
            Controls.Add(_startBox);
            Controls.Add(_endBox);

            _startBox.ValueChanged += (s, e) => OnStartEdited();
            _endBox.ValueChanged += (s, e) => OnEndEdited();

            _spinTimer = new Timer { Interval = 400 };
            _spinTimer.Tick += SpinTick;

            LayoutBoxes();
        }

        private HTimePicker CreateBox()
        {
            var box = new HTimePicker
            {
                Borderless = true,
                ShowSpin = false,
                TabStop = true
            };
            box.GotFocus += (s, e) => { _active = box; Invalidate(); };
            box.LostFocus += (s, e) => Invalidate();
            return box;
        }

        /// <summary>开始时间。</summary>
        public TimeSpan StartValue
        {
            get => _startBox.Value;
            set => _startBox.Value = value;
        }

        /// <summary>结束时间（不早于开始时间）。</summary>
        public TimeSpan EndValue
        {
            get => _endBox.Value;
            set => _endBox.Value = value;
        }

        /// <summary>任一时间变化后触发。</summary>
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged(EventArgs e)
        {
            if (!_sync) ValueChanged?.Invoke(this, e);
        }

        /// <summary>强调色与内嵌时间框同源（橙色主调）。</summary>
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

        #region 顺序约束

        private void OnStartEdited()
        {
            if (_startBox.Value > _endBox.Value)
            {
                _sync = true;
                _endBox.Value = _startBox.Value;
                _sync = false;
            }
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
            OnValueChanged(EventArgs.Empty);
        }

        #endregion

        #region 布局

        private int BoxWidth() => HDrawPaint.TextAdvance("HH:mm:ss.fff", Font) + 20;
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
            int x2 = x1 + boxW + sepW;
            int rightLimit = Width - SpinW - 4;
            if (_startBox != null)
                _startBox.Bounds = new Rectangle(x1, 0, Math.Min(boxW, rightLimit - x1), Height);
            if (_endBox != null)
                _endBox.Bounds = new Rectangle(x2, 0, Math.Max(0, Math.Min(boxW, rightLimit - x2)), Height);
        }

        private Rectangle SeparatorRect
        {
            get
            {
                int x = 8 + BoxWidth();
                return new Rectangle(x, 0, SepWidth(), Height);
            }
        }

        private Rectangle SpinZone => new Rectangle(Width - SpinW, 0, SpinW, Height);

        #endregion

        #region 鼠标与微调连发

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            StopSpin();
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !SpinZone.Contains(e.Location)) return;
            _spinDir = e.Y < Height / 2 ? +1 : -1;
            _spinPressed = true;
            _spinSlow = true;
            Capture = true;
            _active.Bump(_spinDir);
            _spinTimer.Interval = 400;
            _spinTimer.Start();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            StopSpin();
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            StopSpin();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = SpinZone.Contains(e.Location) ? Cursors.Hand : Cursors.Default;
        }

        private void StopSpin()
        {
            if (!_spinPressed) return;
            _spinPressed = false;
            _spinTimer.Stop();
            Capture = false;
            Invalidate();
        }

        private void SpinTick(object sender, EventArgs e)
        {
            _active.Bump(_spinDir);
            if (_spinSlow)
            {
                _spinSlow = false;
                _spinTimer.Interval = 80;
            }
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

            bool active = _startBox.Focused || _endBox.Focused;
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

            TextRenderer.DrawText(g, HTranslation.GetContent("至"), Font, SeparatorRect, Color.FromArgb(150, 154, 162),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            DrawSpin(g);
        }

        private void DrawSpin(Graphics g)
        {
            var z = SpinZone;
            if (_spinPressed)
            {
                var pressedRect = new Rectangle(z.X + 2, _spinDir > 0 ? 2 : z.Height / 2,
                    z.Width - 4, z.Height / 2 - 2);
                using (GraphicsPath pp = HDrawPaint.CreatePath(pressedRect, 3, HRoundStyle.All, false))
                using (var pb = new SolidBrush(Color.FromArgb(38, AccentColor)))
                    g.FillPath(pb, pp);
            }
            Color ink = Enabled ? Color.FromArgb(120, 124, 130) : SystemColors.GrayText;
            using (var pen = new Pen(ink, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int cx = z.X + z.Width / 2;
                int upY = Height / 4;
                int downY = Height * 3 / 4;
                g.DrawLines(pen, new[] { new Point(cx - 4, upY + 2), new Point(cx, upY - 2), new Point(cx + 4, upY + 2) });
                g.DrawLines(pen, new[] { new Point(cx - 4, downY - 2), new Point(cx, downY + 2), new Point(cx + 4, downY - 2) });
            }
            using (var pen = new Pen(Color.FromArgb(214, 217, 223), 1f))
                g.DrawLine(pen, z.X, 6, z.X, Height - 7);
        }

        // 圆角四角由 OnPaintBackground 铺父色透出，不再裁剪窗口 Region（HRgn 会削弧线出折角）
        private void UpdateRegion()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _spinTimer.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
