using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// 可折叠面板（Minimised Panel）：圆角卡片 + 标题栏 + 旋转箭头，
    /// 点击标题栏展开/折叠；折叠时高度收缩为标题栏高，内容区被裁掉。
    /// 子控件放在标题栏以下的内容区。
    /// </summary>
    [DefaultProperty("Title")]
    [DefaultEvent("CollapsedChanged")]
    public class HCollapsePanel : HFluentBase
    {
        private string _title = "Title";
        private bool _collapsed, _dark = true;
        private int _headerHeight = 38;
        private int _radius = 10;
        private int _expandedHeight = 160;
        private float _chevronAngle;
        private bool _hoverHeader;
        private readonly Timer _timer;

        /// <summary>折叠态改变。</summary>
        public event EventHandler CollapsedChanged;

        /// <summary>初始化容器样式。</summary>
        public HCollapsePanel()
        {
            SetStyle(ControlStyles.ContainerControl, true);
            Size = new Size(260, 160);
            BackColor = HFluentPalette.DarkCard;
            Padding = new Padding(12, 46, 12, 12);
            _timer = new Timer { Interval = 15 };
            _timer.Tick += (s, e) =>
            {
                float target = _collapsed ? 180f : 0f;
                _chevronAngle += (target - _chevronAngle) * 0.25f;
                if (Math.Abs(target - _chevronAngle) < 1f) { _chevronAngle = target; _timer.Stop(); }
                Invalidate();
            };
        }

        /// <summary>标题栏文字。</summary>
        [HCategoryLanguage("Fluent 折叠面板"), HDisplayNameLanguage("标题"), HDescriptionLanguage("标题栏文字"), Browsable(true)]
        [DefaultValue("Title")]
        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>是否折叠。</summary>
        [HCategoryLanguage("Fluent 折叠面板"), HDisplayNameLanguage("折叠"), HDescriptionLanguage("折叠时仅显示标题栏"), Browsable(true)]
        [DefaultValue(false)]
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                if (_collapsed == value) return;
                if (value) _expandedHeight = Height;
                _collapsed = value;
                _timer.Start();
                Height = value ? _headerHeight : Math.Max(_expandedHeight, _headerHeight + 40);
                Invalidate();
                CollapsedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>是否暗色卡片（默认暗色）。</summary>
        [HCategoryLanguage("Fluent 折叠面板"), HDisplayNameLanguage("暗色卡片"), HDescriptionLanguage("暗色卡片底"), Browsable(true)]
        [DefaultValue(true)]
        public bool Dark
        {
            get => _dark;
            set { _dark = value; Invalidate(); }
        }

        /// <summary>标题栏高度。</summary>
        [HCategoryLanguage("Fluent 折叠面板"), HDisplayNameLanguage("标题栏高度"), HDescriptionLanguage("标题栏高度（折叠后整体高度）"), Browsable(true)]
        [DefaultValue(38)]
        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = Math.Max(24, value); Invalidate(); }
        }

        /// <summary>圆角半径。</summary>
        [HCategoryLanguage("Fluent 折叠面板"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("卡片圆角半径"), Browsable(true)]
        [DefaultValue(10)]
        public int CardRadius
        {
            get => _radius;
            set { _radius = value; Invalidate(); }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!_collapsed) _expandedHeight = Height;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hover = e.Y <= _headerHeight;
            if (hover != _hoverHeader) { _hoverHeader = hover; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverHeader) { _hoverHeader = false; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && e.Y <= _headerHeight)
                Collapsed = !_collapsed;
        }

        /// <summary>自绘卡片、标题与旋转箭头。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color card = _dark ? HFluentPalette.DarkCard : Color.White;
            using (var path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            {
                using (var bb = new SolidBrush(card))
                    g.FillPath(bb, path);
                using (var pen = new Pen(_dark ? HFluentPalette.DarkBorder : Color.FromArgb(226, 228, 232)))
                    g.DrawPath(pen, path);

                // 标题栏悬停底色
                if (_hoverHeader)
                    using (var hb = new SolidBrush(_dark ? Color.FromArgb(22, 255, 255, 255) : Color.FromArgb(243, 244, 246)))
                    using (var hp = HeaderPath())
                        g.FillPath(hb, hp);
            }

            Color textColor = _dark ? Color.White : Color.FromArgb(31, 41, 55);
            using (var titleFont = new Font(Font.FontFamily, Font.SizeInPoints + 0.5f, FontStyle.Bold))
                TextRenderer.DrawText(g, _title, titleFont,
                    new Rectangle(14, 0, Width - 44, _headerHeight), textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            // 右侧旋转箭头
            var state = g.Save();
            g.TranslateTransform(Width - 22, _headerHeight / 2f);
            g.RotateTransform(_chevronAngle);
            HFluentGlyphDraw.Draw(g, HFluentGlyph.ChevronUp, new RectangleF(-6, -6, 12, 12), textColor, 2f, false);
            g.Restore(state);
        }

        private GraphicsPath HeaderPath()
        {
            var p = new GraphicsPath();
            int r = _radius;
            p.AddArc(0, 0, r, r, 180, 90);
            p.AddArc(Width - 1 - r, 0, r, r, 270, 90);
            p.AddLine(Width, _headerHeight, 0, _headerHeight);
            p.CloseFigure();
            return p;
        }
    }
}
