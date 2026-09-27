using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace HFromUI.HFrom.HShell
{
    using HFromUI.HFrom.HUiKit;
    /// <summary>顶部导航页签项（图标 + 文字）。</summary>
    public class HNavItem
    {
        /// <summary>页签文字（由调用方传入，控件内不硬编码业务文案）。</summary>
        public string Text { get; set; }
        /// <summary>矢量图标种类。</summary>
        public HGlyphKind Glyph { get; set; }
        /// <summary>业务关联数据。</summary>
        public object Tag { get; set; }
        public HNavItem() { Text = ""; }
        public HNavItem(string text, HGlyphKind glyph) { Text = text; Glyph = glyph; }
    }
    /// <summary>
    /// 深色顶部横向导航条（iVMS-4200 风格）：图标+文字页签横向排列，
    /// 选中项顶部 3px 强调条 + 高亮底色，悬停浅色高亮；点击/方向键切换。
    /// 配色取 HUiTheme 深色监控主题。
    /// </summary>
    public class HTopNav : Control
    {
        /// <summary>_items 字段。</summary>
        private readonly List<HNavItem> _items = new List<HNavItem>();
        /// <summary>_selectedIndex 字段。</summary>
        private int _selectedIndex = -1;
        /// <summary>_hoverIndex 字段。</summary>
        private int _hoverIndex = -1;
        private Font _itemFont;
        /// <summary>选中页签变化事件。</summary>
        public event EventHandler SelectedIndexChanged;
        public HTopNav()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw
                | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = HUiTheme.PanelBg;
            ForeColor = HUiTheme.TextMain;
            Height = 48;
            Dock = DockStyle.Top;
            _itemFont = new Font("微软雅黑", 10.5F, FontStyle.Bold);
            Font = _itemFont;
        }
        /// <summary>页签集合。</summary>
        public List<HNavItem> Items { get { return _items; } }
        /// <summary>当前选中页签下标（-1 无选中）。</summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                int v = Math.Max(-1, Math.Min(value, _items.Count - 1));
                if (_selectedIndex == v) return;
                _selectedIndex = v;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>添加一个页签，返回其下标。</summary>
        public int AddItem(string text, HGlyphKind glyph, object tag = null)
        {
            _items.Add(new HNavItem(text, glyph) { Tag = tag });
            Invalidate();
            return _items.Count - 1;
        }
        /// <summary>计算各页签矩形（左起排列，宽度随文字自适应）。</summary>
        private Rectangle ItemRect(int index)
        {
            if (index < 0 || index >= _items.Count) return Rectangle.Empty;
            using (Graphics g = CreateGraphics())
            {
                int x = 0;
                for (int i = 0; i < index; i++) x += ItemWidth(g, _items[i]);
                return new Rectangle(x, 0, ItemWidth(g, _items[index]), Height);
            }
        }
        /// <summary>ItemWidth 方法。</summary>
        private int ItemWidth(Graphics g, HNavItem item)
        {
            SizeF ts = g.MeasureString(item.Text ?? "", _itemFont);
            int w = (int)Math.Ceiling(ts.Width) + 22 /*glyph*/ + 34 /*左右内边距*/;
            return Math.Max(w, 96);
        }
        /// <summary>ItemAt 方法。</summary>
        private int ItemAt(Point p)
        {
            for (int i = 0; i < _items.Count; i++)
                if (ItemRect(i).Contains(p)) return i;
            return -1;
        }
        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int h = ItemAt(e.Location);
            if (h != _hoverIndex) { _hoverIndex = h; Invalidate(); }
        }
        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverIndex != -1) { _hoverIndex = -1; Invalidate(); }
        }
        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int i = ItemAt(e.Location);
            if (i >= 0)
            {
                SelectedIndex = i;
                Focus();
            }
        }
        /// <summary>判断是否 InputKey。</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Home || keyData == Keys.End)
                return true;
            return base.IsInputKey(keyData);
        }
        /// <summary>响应 KeyDown 事件。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_items.Count == 0) return;
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Up)
            {
                SelectedIndex = _selectedIndex <= 0 ? _items.Count - 1 : _selectedIndex - 1;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Down)
            {
                SelectedIndex = _selectedIndex >= _items.Count - 1 ? 0 : _selectedIndex + 1;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Home) { SelectedIndex = 0; e.Handled = true; }
            else if (e.KeyCode == Keys.End) { SelectedIndex = _items.Count - 1; e.Handled = true; }
        }
        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush bg = new SolidBrush(BackColor))
                g.FillRectangle(bg, ClientRectangle);
            for (int i = 0; i < _items.Count; i++)
            {
                Rectangle r = ItemRect(i);
                bool selected = i == _selectedIndex;
                bool hover = i == _hoverIndex;
                if (selected)
                {
                    // 选中底色（Accent 18% 透明叠加感）
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(36, HUiTheme.Accent)))
                        g.FillRectangle(b, r);
                    // 顶部强调条
                    using (SolidBrush b = new SolidBrush(HUiTheme.Accent))
                        g.FillRectangle(b, new Rectangle(r.X, 0, r.Width, 4));
                }
                else if (hover)
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(24, HUiTheme.GhostHover)))
                        g.FillRectangle(b, r);
                }
                Color glyphColor = selected ? Color.White : (hover ? HUiTheme.TextMain : HUiTheme.TextSub);
                Color textColor = selected ? Color.White : (hover ? HUiTheme.TextMain : HUiTheme.TextSub);
                // 图标 + 文字 垂直居中、整体水平居中
                SizeF ts = g.MeasureString(_items[i].Text ?? "", _itemFont);
                int glyphSz = 18;
                int gap = 6;
                int totalW = glyphSz + gap + (int)Math.Ceiling(ts.Width);
                int cx = r.X + (r.Width - totalW) / 2;
                int cy = r.Y + (r.Height - glyphSz) / 2;
                HGlyph.Draw(g, _items[i].Glyph, new Rectangle(cx, cy, glyphSz, glyphSz), glyphColor, 2.0f);
                using (SolidBrush tb = new SolidBrush(textColor))
                    g.DrawString(_items[i].Text ?? "", _itemFont, tb,
                        new RectangleF(cx + glyphSz + gap, r.Y, ts.Width + 2, r.Height),
                        new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });
            }
            // 底部分隔线
            using (Pen p = new Pen(HUiTheme.Border))
                g.DrawLine(p, 0, Height - 1, Width, Height - 1);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _itemFont?.Dispose();
            base.Dispose(disposing);
        }
    }
}
