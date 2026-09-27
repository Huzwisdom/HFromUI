using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Panel
{
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 30 种滚动条样式
    /// </summary>
    public enum ScrollBarStyle
    {
        ClassicGray,
        DarkMinimal,
        SteelBlue,
        ModernBlue,
        DarkTrack,
        OrangeDark,
        FullWidthSilver,
        TransparentOverlay,
        Pink,
        Green,
        Yellow,
        Cyan,
        Salmon,
        Purple,
        ThinBlue,
        NoArrowGray,
        RoyalBlueRounded,
        LimeDark,
        LightOnDark,
        Brown,
        Tomato,
        LavenderPurple,
        Teal,
        Cornflower,
        DarkSlateBlue,
        Chocolate,
        Olive,
        IndianRed,
        OliveGreen,
        BlackSnow
    }

    /// <summary>
    /// 完美的可垂直滚动面板，内建 30 种自定义滚动条风格，支持鼠标滚轮和拖拽。
    /// </summary>
    public class HScrollablePanel : Panel
    {
        // -------------------- 内部组件 --------------------
        private Panel _contentPanel;
        private VScrollBarEx _vScrollBar;
        /// <summary>_loaded 字段。</summary>
        private bool _loaded = false;

        // -------------------- 属性 --------------------
        private ScrollBarStyle _barStyle = ScrollBarStyle.ClassicGray;
        /// <summary>_smoothScroll 字段。</summary>
        private bool _smoothScroll = true;

        [Category("滚动条")]
        public ScrollBarStyle BarStyle
        {
            get { return _barStyle; }
            set { _barStyle = value; if (_vScrollBar != null) _vScrollBar.Style = value; }
        }

        [Category("滚动条")]
        [Description("是否启用平滑滚动动画")]
        public bool SmoothScroll
        {
            get { return _smoothScroll; }
            set { _smoothScroll = value; }
        }

        [Category("滚动条")]
        [Description("滚动条宽度")]
        public int ScrollBarWidth
        {
            get { return _vScrollBar != null ? _vScrollBar.Width : 12; }
            set { if (_vScrollBar != null) { _vScrollBar.Width = value; AdjustLayout(); } }
        }

        [Browsable(false)]
        public Panel ContentPanel
        {
            get { return _contentPanel; }
        }

        // -------------------- 构造函数 --------------------
        public HScrollablePanel()
        {
            // 关闭系统滚动
            this.AutoScroll = false;
            this.HorizontalScroll.Maximum = 0;
            this.VerticalScroll.Maximum = 0;

            // 创建内容面板
            _contentPanel = new Panel
            {
                Location = Point.Empty,
                Name = "ContentPanel",
                BackColor = this.BackColor
            };
            base.Controls.Add(_contentPanel);

            // 创建自定义滚动条
            _vScrollBar = new VScrollBarEx { Style = _barStyle, Visible = false };
            _vScrollBar.Scroll += VScrollBar_Scroll;
            base.Controls.Add(_vScrollBar);

            // 事件绑定
            this.SizeChanged += (s, e) => AdjustLayout();
            this.MouseWheel += OnMouseWheel;
            _contentPanel.MouseWheel += OnMouseWheel;
            this.BackColorChanged += (s, e) =>
            {
                if (_contentPanel != null) _contentPanel.BackColor = this.BackColor;
            };
        }

        // -------------------- 设计时/运行时控件管理 --------------------
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // 确保不在设计模式下且尚未加载过
            if (!DesignMode && !_loaded)
            {
                TransferControlsToContentPanel();
                _loaded = true;
                AdjustLayout();
            }
        }

        /// <summary>
        /// 将直接添加到面板上的控件转移到内部可滚动内容面板
        /// </summary>
        private void TransferControlsToContentPanel()
        {
            if (_contentPanel == null) return;

            var controlsToMove = new List<Control>();
            foreach (Control c in Controls)
            {
                if (c != _contentPanel && c != _vScrollBar)
                    controlsToMove.Add(c);
            }

            this.SuspendLayout();
            foreach (Control c in controlsToMove)
            {
                Controls.Remove(c);
                _contentPanel.Controls.Add(c);
                c.LocationChanged += ChildChanged;
                c.SizeChanged += ChildChanged;
            }
            this.ResumeLayout(true);
        }

        // 运行时动态添加控件时自动转移到内容面板
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control == _contentPanel || e.Control == _vScrollBar)
                return;

            // 如果已完成初始加载且不在设计模式下，立即转移
            if (!DesignMode && _loaded)
            {
                if (Controls.Contains(e.Control))
                    Controls.Remove(e.Control);
                _contentPanel.Controls.Add(e.Control);
                e.Control.LocationChanged += ChildChanged;
                e.Control.SizeChanged += ChildChanged;
                AdjustLayout();
            }
            // 否则控件暂时留在外层，由 TransferControlsToContentPanel 统一处理
        }

        /// <summary>响应 ControlRemoved 事件。</summary>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            if (e.Control == _contentPanel || e.Control == _vScrollBar) return;
            e.Control.LocationChanged -= ChildChanged;
            e.Control.SizeChanged -= ChildChanged;
            AdjustLayout();
        }

        /// <summary>ChildChanged 方法。</summary>
        private void ChildChanged(object sender, EventArgs e)
        {
            AdjustLayout();
        }

        // -------------------- 核心布局 --------------------
        private void AdjustLayout()
        {
            if (_contentPanel == null || _vScrollBar == null) return;

            // 计算内容实际高度
            int maxBottom = 0;
            foreach (Control c in _contentPanel.Controls)
            {
                int b = c.Bottom + c.Margin.Bottom;
                if (b > maxBottom) maxBottom = b;
            }

            int clientH = this.ClientSize.Height;
            int contentH = Math.Max(maxBottom, clientH);
            _contentPanel.Height = contentH;

            bool needScroll = contentH > clientH;
            if (needScroll)
            {
                _vScrollBar.Visible = true;
                _vScrollBar.Maximum = contentH - clientH;
                _vScrollBar.LargeChange = clientH;
                _vScrollBar.SmallChange = 20;

                int newWidth = Math.Max(0, this.ClientSize.Width - _vScrollBar.Width);
                _contentPanel.Width = newWidth;
                _vScrollBar.Location = new Point(this.ClientSize.Width - _vScrollBar.Width, 0);
                _vScrollBar.Height = clientH;
                ScrollTo(_vScrollBar.Value);
            }
            else
            {
                _vScrollBar.Visible = false;
                _contentPanel.Width = this.ClientSize.Width;
                _contentPanel.Top = 0;
            }
        }

        /// <summary>ScrollTo 方法。</summary>
        private void ScrollTo(int value)
        {
            if (_contentPanel == null) return;
            int maxOffset = _contentPanel.Height - this.ClientSize.Height;
            if (value < 0) value = 0;
            if (value > maxOffset) value = maxOffset;
            _contentPanel.Top = -value;
        }

        // -------------------- 滚动条事件与动画 --------------------
        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (_smoothScroll)
                AnimateScrollTo(_vScrollBar.Value);
            else
                ScrollTo(_vScrollBar.Value);
        }

        /// <summary>响应 MouseWheel 事件。</summary>
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (!_vScrollBar.Visible) return;
            int newVal = _vScrollBar.Value - e.Delta / 3;
            if (newVal < 0) newVal = 0;
            if (newVal > _vScrollBar.Maximum) newVal = _vScrollBar.Maximum;
            _vScrollBar.Value = newVal;
            VScrollBar_Scroll(null, new ScrollEventArgs(ScrollEventType.ThumbTrack, newVal));
        }

        private Timer _scrollTimer;
        private int _targetScrollValue;
        private const int ANIM_STEP = 10;

        /// <summary>AnimateScrollTo 方法。</summary>
        private void AnimateScrollTo(int targetValue)
        {
            if (_scrollTimer == null)
            {
                _scrollTimer = new Timer { Interval = 10 };
                _scrollTimer.Tick += (s, e) =>
                {
                    int cur = -_contentPanel.Top;
                    int diff = _targetScrollValue - cur;
                    if (Math.Abs(diff) <= ANIM_STEP)
                    {
                        ScrollTo(_targetScrollValue);
                        _scrollTimer.Stop();
                    }
                    else
                    {
                        int step = diff > 0 ? ANIM_STEP : -ANIM_STEP;
                        ScrollTo(cur + step);
                        _vScrollBar.SilentSetValue(cur + step);
                    }
                };
            }
            _targetScrollValue = targetValue;
            if (!_scrollTimer.Enabled) _scrollTimer.Start();
        }
    }

    // ================== 内部自定义垂直滚动条 ==================
    internal class VScrollBarEx : Control
    {
        /// <summary>_value 字段。</summary>
        private int _value = 0;
        /// <summary>_maximum 字段。</summary>
        private int _maximum = 100;
        /// <summary>_largeChange 字段。</summary>
        private int _largeChange = 10;
        /// <summary>_smallChange 字段。</summary>
        private int _smallChange = 1;
        /// <summary>_style 字段。</summary>
        private ScrollBarStyle _style = ScrollBarStyle.ClassicGray;

        /// <summary>_isThumbDragging 字段。</summary>
        private bool _isThumbDragging = false;
        private int _dragStartY;
        private int _dragStartValue;

        public event ScrollEventHandler Scroll;

        public VScrollBarEx()
        {
            this.Width = 12;
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        public int Value
        {
            get { return _value; }
            set
            {
                if (value < 0) value = 0;
                if (value > _maximum) value = _maximum;
                if (_value != value)
                {
                    _value = value;
                    Invalidate();
                    Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbTrack, _value));
                }
            }
        }

        /// <summary>SilentSetValue 方法。</summary>
        public void SilentSetValue(int val)
        {
            if (val < 0) val = 0;
            if (val > _maximum) val = _maximum;
            _value = val;
            Invalidate();
        }

        /// <summary>Maximum 成员。</summary>
        public int Maximum { get { return _maximum; } set { _maximum = value; Invalidate(); } }
        /// <summary>LargeChange 成员。</summary>
        public int LargeChange { get { return _largeChange; } set { _largeChange = value; } }
        /// <summary>SmallChange 成员。</summary>
        public int SmallChange { get { return _smallChange; } set { _smallChange = value; } }

        public ScrollBarStyle Style
        {
            get { return _style; }
            set { _style = value; Invalidate(); }
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Rectangle thumbRect = GetThumbRect();
            if (thumbRect.Contains(e.Location))
            {
                _isThumbDragging = true;
                _dragStartY = e.Y;
                _dragStartValue = _value;
            }
            else
            {
                if (e.Y < thumbRect.Top) Value -= LargeChange;
                else if (e.Y > thumbRect.Bottom) Value += LargeChange;
            }
            base.OnMouseDown(e);
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isThumbDragging)
            {
                int trackHeight = this.Height - 30;
                if (trackHeight <= 0) return;
                int deltaY = e.Y - _dragStartY;
                float ratio = (float)_maximum / trackHeight;
                int newVal = _dragStartValue + (int)(deltaY * ratio);
                if (newVal < 0) newVal = 0;
                if (newVal > _maximum) newVal = _maximum;
                Value = newVal;
            }
            base.OnMouseMove(e);
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isThumbDragging = false;
            base.OnMouseUp(e);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = ClientRectangle;

            Color trackColor, thumbColor, arrowColor = Color.Black;
            int cornerRadius = 2;
            bool showArrows = true;
            int thumbWidth = rect.Width - 2;

            ChooseColors(out trackColor, out thumbColor, out arrowColor,
                         ref cornerRadius, ref showArrows, ref thumbWidth);

            // 轨道
            using (SolidBrush brush = new SolidBrush(trackColor))
            {
                if (cornerRadius > 0)
                {
                    using (GraphicsPath path = GetRoundRect(rect, cornerRadius))
                        g.FillPath(brush, path);
                }
                else g.FillRectangle(brush, rect);
            }

            // 箭头
            if (showArrows)
            {
                int midX = rect.Width / 2;
                using (SolidBrush arrowBrush = new SolidBrush(arrowColor))
                {
                    Point[] up = { new Point(midX, 4), new Point(midX - 4, 12), new Point(midX + 4, 12) };
                    g.FillPolygon(arrowBrush, up);
                    Point[] down = { new Point(midX, rect.Height - 4), new Point(midX - 4, rect.Height - 12), new Point(midX + 4, rect.Height - 12) };
                    g.FillPolygon(arrowBrush, down);
                }
            }

            // 滑块
            if (_maximum > 0)
            {
                Rectangle thumbRect = GetThumbRect();
                using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
                {
                    if (cornerRadius > 0)
                    {
                        using (GraphicsPath path = GetRoundRect(thumbRect, cornerRadius))
                            g.FillPath(thumbBrush, path);
                    }
                    else g.FillRectangle(thumbBrush, thumbRect);
                }
                using (Pen pen = new Pen(Color.FromArgb(80, thumbColor.R / 2, thumbColor.G / 2, thumbColor.B / 2)))
                {
                    if (cornerRadius > 0)
                    {
                        using (GraphicsPath path = GetRoundRect(thumbRect, cornerRadius))
                            g.DrawPath(pen, path);
                    }
                    else g.DrawRectangle(pen, thumbRect);
                }
            }
        }

        /// <summary>获取 thumbRect。</summary>
        private Rectangle GetThumbRect()
        {
            int trackTop = 15;
            int trackBottom = this.Height - 15;
            int trackHeight = trackBottom - trackTop;
            if (trackHeight <= 0) return Rectangle.Empty;

            float pageRatio = (float)LargeChange / (Maximum + LargeChange);
            int thumbHeight = Math.Max(20, (int)(trackHeight * pageRatio));
            int thumbTop = trackTop + (int)((float)Value / Maximum * (trackHeight - thumbHeight));
            return new Rectangle(1, thumbTop, this.Width - 2, thumbHeight);
        }

        /// <summary>ChooseColors 方法。</summary>
        private void ChooseColors(out Color track, out Color thumb, out Color arrow,
                                  ref int radius, ref bool arrows, ref int thumbWidth)
        {
            track = Color.LightGray; thumb = Color.Gray; arrow = Color.Black;
            radius = 2; arrows = true; thumbWidth = this.Width - 2;

            switch (_style)
            {
                case ScrollBarStyle.ClassicGray: track = Color.FromArgb(240, 240, 240); thumb = Color.FromArgb(180, 180, 180); break;
                case ScrollBarStyle.DarkMinimal: track = Color.FromArgb(230, 230, 230); thumb = Color.FromArgb(100, 100, 100); radius = 4; break;
                case ScrollBarStyle.SteelBlue: track = Color.WhiteSmoke; thumb = Color.SteelBlue; break;
                case ScrollBarStyle.ModernBlue: track = Color.White; thumb = Color.FromArgb(0, 122, 204); radius = 6; break;
                case ScrollBarStyle.DarkTrack: track = Color.FromArgb(30, 30, 30); thumb = Color.FromArgb(80, 80, 80); arrow = Color.White; break;
                case ScrollBarStyle.OrangeDark: track = Color.Black; thumb = Color.DarkOrange; arrow = Color.White; radius = 4; break;
                case ScrollBarStyle.FullWidthSilver: track = Color.FromArgb(245, 245, 245); thumb = Color.Silver; thumbWidth = this.Width; break;
                case ScrollBarStyle.TransparentOverlay: track = Color.Transparent; thumb = Color.FromArgb(150, 0, 0, 0); radius = 8; arrows = false; break;
                case ScrollBarStyle.Pink: track = Color.Pink; thumb = Color.DeepPink; break;
                case ScrollBarStyle.Green: track = Color.LightGreen; thumb = Color.Green; break;
                case ScrollBarStyle.Yellow: track = Color.LightYellow; thumb = Color.Gold; break;
                case ScrollBarStyle.Cyan: track = Color.LightCyan; thumb = Color.DarkCyan; break;
                case ScrollBarStyle.Salmon: track = Color.LightSalmon; thumb = Color.OrangeRed; break;
                case ScrollBarStyle.Purple: track = Color.Plum; thumb = Color.Purple; radius = 5; break;
                case ScrollBarStyle.ThinBlue: track = Color.LightBlue; thumb = Color.Blue; thumbWidth = 10; break;
                case ScrollBarStyle.NoArrowGray: track = Color.Silver; thumb = Color.DimGray; arrows = false; break;
                case ScrollBarStyle.RoyalBlueRounded: track = Color.White; thumb = Color.RoyalBlue; radius = 10; break;
                case ScrollBarStyle.LimeDark: track = Color.Black; thumb = Color.LimeGreen; arrow = Color.White; break;
                case ScrollBarStyle.LightOnDark: track = Color.DarkSlateGray; thumb = Color.LightGray; arrow = Color.White; break;
                case ScrollBarStyle.Brown: track = Color.AntiqueWhite; thumb = Color.SaddleBrown; radius = 3; break;
                case ScrollBarStyle.Tomato: track = Color.MistyRose; thumb = Color.Tomato; break;
                case ScrollBarStyle.LavenderPurple: track = Color.Lavender; thumb = Color.MediumPurple; thumbWidth = 14; break;
                case ScrollBarStyle.Teal: track = Color.Honeydew; thumb = Color.SeaGreen; radius = 7; break;
                case ScrollBarStyle.Cornflower: track = Color.AliceBlue; thumb = Color.CornflowerBlue; break;
                case ScrollBarStyle.DarkSlateBlue: track = Color.GhostWhite; thumb = Color.DarkSlateBlue; radius = 4; break;
                case ScrollBarStyle.Chocolate: track = Color.Beige; thumb = Color.Chocolate; break;
                case ScrollBarStyle.Olive: track = Color.Ivory; thumb = Color.IndianRed; break;
                case ScrollBarStyle.IndianRed: track = Color.Ivory; thumb = Color.IndianRed; break;
                case ScrollBarStyle.OliveGreen: track = Color.Linen; thumb = Color.Olive; radius = 3; break;
                case ScrollBarStyle.BlackSnow: track = Color.Snow; thumb = Color.Black; radius = 2; break;
            }
        }

        /// <summary>获取 roundRect。</summary>
        private GraphicsPath GetRoundRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            Rectangle arc = new Rectangle(rect.Location, new Size(d, d));
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - d;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - d;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
