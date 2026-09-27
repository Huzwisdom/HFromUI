using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.HUiKit
{
    /// <summary>
    /// 缩放控制条：白底圆角竖排小工具条，内置 放大/缩小/适配窗口/锁定 四个按钮，
    /// 悬停高亮、按下反馈。锁定按钮为开关态。用于画布/场景编排器左下角。
    /// </summary>
    [DefaultEvent("ZoomInClicked")]
    public class HZoomBar : Control
    {
        /// <summary>_btnHeight 字段。</summary>
        private int _btnHeight = 36;
        /// <summary>_hoverBtn 字段。</summary>
        private int _hoverBtn = -1;
        /// <summary>_downBtn 字段。</summary>
        private int _downBtn = -1;
        /// <summary>_locked 字段。</summary>
        private bool _locked = false;
        /// <summary>_barColor 字段。</summary>
        private Color _barColor = Color.White;
        /// <summary>_borderColor 字段。</summary>
        private Color _borderColor = Color.FromArgb(228, 231, 238);
        /// <summary>_glyphColor 字段。</summary>
        private Color _glyphColor = Color.FromArgb(96, 104, 118);

        /// <summary>条底色</summary>
        /// <summary>BarColor 成员。</summary>
        /// <summary>BarColor 字段。</summary>
        [Category("HFromUI"), Description("条底色")]
        public Color BarColor { get { return _barColor; } set { _barColor = value; Invalidate(); } }
        /// <summary>描边色</summary>
        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [Category("HFromUI"), Description("描边色")]
        public Color BorderColor { get { return _borderColor; } set { _borderColor = value; Invalidate(); } }
        /// <summary>图标颜色</summary>
        /// <summary>GlyphColor 成员。</summary>
        /// <summary>GlyphColor 字段。</summary>
        [Category("HFromUI"), Description("图标颜色")]
        public Color GlyphColor { get { return _glyphColor; } set { _glyphColor = value; Invalidate(); } }
        /// <summary>是否处于锁定态</summary>
        [Category("HFromUI"), Description("锁定态"), DefaultValue(false)]
        public bool Locked
        {
            get { return _locked; }
            set { _locked = value; Invalidate(); }
        }

        /// <summary>放大按钮点击</summary>
        public event EventHandler ZoomInClicked;
        /// <summary>缩小按钮点击</summary>
        public event EventHandler ZoomOutClicked;
        /// <summary>适配窗口按钮点击</summary>
        public event EventHandler FitClicked;
        /// <summary>锁定按钮点击（Locked 已切换）</summary>
        public event EventHandler LockClicked;

        public HZoomBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(38, 38 * 4 + 2);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle bar = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = HGlyph.RoundedRect(bar, 9))
            {
                using (SolidBrush b = new SolidBrush(_barColor)) g.FillPath(b, path);
                using (Pen p = new Pen(_borderColor, 1f)) g.DrawPath(p, path);
            }

            HGlyphKind[] glyphs = { HGlyphKind.Plus, HGlyphKind.Minus, HGlyphKind.Fit, HGlyphKind.Lock };
            for (int i = 0; i < 4; i++)
            {
                Rectangle br = new Rectangle(1, 1 + i * _btnHeight, Width - 2, _btnHeight);

                if (i == _downBtn)
                {
                    using (GraphicsPath hp = HGlyph.RoundedRect(new Rectangle(br.X + 2, br.Y + 2, br.Width - 4, br.Height - 4), 6))
                    using (SolidBrush b = new SolidBrush(HGlyph.Lighten(_glyphColor, 0.82f)))
                        g.FillPath(b, hp);
                }
                else if (i == _hoverBtn)
                {
                    using (GraphicsPath hp = HGlyph.RoundedRect(new Rectangle(br.X + 2, br.Y + 2, br.Width - 4, br.Height - 4), 6))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(242, 244, 247)))
                        g.FillPath(b, hp);
                }

                // 分隔线
                if (i > 0)
                    using (Pen p = new Pen(_borderColor, 1f))
                        g.DrawLine(p, br.X + 8, br.Y, br.Right - 8, br.Y);

                Color c = (i == 3 && _locked) ? Color.FromArgb(99, 102, 241) : _glyphColor;
                Rectangle gr = new Rectangle(br.X + (br.Width - 18) / 2, br.Y + (br.Height - 18) / 2, 18, 18);
                HGlyph.Draw(g, glyphs[i], gr, c, 2.0f);
            }
        }

        /// <summary>ButtonAt 方法。</summary>
        private int ButtonAt(Point p)
        {
            if (p.X < 0 || p.X >= Width) return -1;
            int idx = (p.Y - 1) / _btnHeight;
            return (idx >= 0 && idx < 4) ? idx : -1;
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = ButtonAt(e.Location);
            if (h != _hoverBtn) { _hoverBtn = h; Invalidate(); }
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverBtn != -1 || _downBtn != -1) { _hoverBtn = -1; _downBtn = -1; Invalidate(); }
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                int idx = ButtonAt(e.Location);
                if (idx >= 0) { _downBtn = idx; Invalidate(); }
            }
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int idx = ButtonAt(e.Location);
            if (_downBtn >= 0 && idx == _downBtn)
            {
                switch (idx)
                {
                    case 0: ZoomInClicked?.Invoke(this, EventArgs.Empty); break;
                    case 1: ZoomOutClicked?.Invoke(this, EventArgs.Empty); break;
                    case 2: FitClicked?.Invoke(this, EventArgs.Empty); break;
                    case 3:
                        _locked = !_locked;
                        LockClicked?.Invoke(this, EventArgs.Empty);
                        break;
                }
            }
            _downBtn = -1;
            Invalidate();
        }
    }
}
