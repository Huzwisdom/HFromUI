using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.HUiKit
{
    /// <summary>侧边菜单项</summary>
    public class HSideMenuItem
    {
        /// <summary>显示文字</summary>
        public string Text { get; set; }
        /// <summary>矢量图标</summary>
        public HGlyphKind Icon { get; set; }
        /// <summary>自定义数据</summary>
        public object Tag { get; set; }

        public HSideMenuItem() { Text = ""; }
        public HSideMenuItem(string text, HGlyphKind icon) { Text = text; Icon = icon; }
    }

    /// <summary>
    /// 侧边导航菜单：图标 + 文字的竖向菜单，选中项浅色胶囊高亮（默认浅绿），悬停浅灰。
    /// 参考现代 SaaS 后台布局（仪表盘/设备管理/协议服务…）。
    /// </summary>
    [DefaultEvent("SelectedIndexChanged")]
    public class HSideMenu : Control
    {
        /// <summary>_items 字段。</summary>
        private readonly List<HSideMenuItem> _items = new List<HSideMenuItem>();
        /// <summary>_selectedIndex 字段。</summary>
        private int _selectedIndex = -1;
        /// <summary>_hoverIndex 字段。</summary>
        private int _hoverIndex = -1;
        /// <summary>_itemHeight 字段。</summary>
        private int _itemHeight = 46;
        /// <summary>_activeColor 字段。</summary>
        private Color _activeColor = Color.FromArgb(34, 197, 94);
        /// <summary>_hoverColor 字段。</summary>
        private Color _hoverColor = Color.FromArgb(242, 244, 247);
        /// <summary>_menuBackColor 字段。</summary>
        private Color _menuBackColor = Color.White;
        /// <summary>_textColor 字段。</summary>
        private Color _textColor = Color.FromArgb(96, 104, 118);
        /// <summary>_activeTextColor 字段。</summary>
        private Color _activeTextColor = Color.Empty;

        /// <summary>菜单项集合（代码添加：Items.Add(new HSideMenuItem("仪表盘", HGlyphKind.Dashboard))）</summary>
        [Category("HFromUI"), Description("菜单项集合")]
        /// <summary>Items 成员。</summary>
        /// <summary>Items 字段。</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<HSideMenuItem> Items { get { return _items; } }

        /// <summary>当前选中项索引（-1 无选中）</summary>
        [Category("HFromUI"), Description("当前选中项索引"), DefaultValue(-1)]
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>每项高度</summary>
        [Category("HFromUI"), Description("每项高度"), DefaultValue(46)]
        public int ItemHeight
        {
            get { return _itemHeight; }
            set { _itemHeight = value; Invalidate(); }
        }

        /// <summary>选中项高亮色（胶囊底色为浅色，文字/图标为该色深色版）</summary>
        [Category("HFromUI"), Description("选中项高亮色")]
        public Color ActiveColor
        {
            get { return _activeColor; }
            set { _activeColor = value; Invalidate(); }
        }

        /// <summary>悬停底色</summary>
        [Category("HFromUI"), Description("悬停底色")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set { _hoverColor = value; Invalidate(); }
        }

        /// <summary>选中项文字颜色（Empty 自动用 ActiveColor 深色版）</summary>
        [Category("HFromUI"), Description("选中项文字颜色")]
        public Color ActiveTextColor
        {
            get { return _activeTextColor; }
            set { _activeTextColor = value; Invalidate(); }
        }

        /// <summary>菜单背景色</summary>
        [Category("HFromUI"), Description("菜单背景色")]
        public Color MenuBackColor
        {
            get { return _menuBackColor; }
            set { _menuBackColor = value; Invalidate(); }
        }

        /// <summary>普通文字颜色</summary>
        [Category("HFromUI"), Description("普通文字颜色")]
        public Color MenuTextColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }

        /// <summary>选中项变化事件</summary>
        public event EventHandler SelectedIndexChanged;

        public HSideMenu()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 10F);
            Width = 240;
        }

        /// <summary>快捷添加菜单项</summary>
        public void AddItem(string text, HGlyphKind icon)
        {
            _items.Add(new HSideMenuItem(text, icon));
            Invalidate();
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush b = new SolidBrush(_menuBackColor))
                g.FillRectangle(b, ClientRectangle);

            Color activeText = _activeTextColor.IsEmpty ? HGlyph.Darken(_activeColor, 0.2f) : _activeTextColor;
            Color activeBg = HGlyph.Lighten(_activeColor, 0.86f);

            for (int i = 0; i < _items.Count; i++)
            {
                Rectangle itemRect = new Rectangle(8, i * _itemHeight + 4, Width - 16, _itemHeight - 8);

                if (i == _selectedIndex)
                {
                    using (GraphicsPath path = HGlyph.RoundedRect(itemRect, 8))
                    using (SolidBrush b = new SolidBrush(activeBg))
                        g.FillPath(b, path);
                }
                else if (i == _hoverIndex)
                {
                    using (GraphicsPath path = HGlyph.RoundedRect(itemRect, 8))
                    using (SolidBrush b = new SolidBrush(_hoverColor))
                        g.FillPath(b, path);
                }

                HSideMenuItem it = _items[i];
                Color c = (i == _selectedIndex) ? activeText : _textColor;

                Rectangle iconRect = new Rectangle(itemRect.X + 12, itemRect.Y + (itemRect.Height - 22) / 2, 22, 22);
                HGlyph.Draw(g, it.Icon, iconRect, c, 1.9f);

                TextRenderer.DrawText(g, it.Text, Font,
                    new Rectangle(itemRect.X + 44, itemRect.Y, itemRect.Width - 50, itemRect.Height),
                    c, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>ItemAt 方法。</summary>
        private int ItemAt(Point p)
        {
            if (p.X < 8 || p.X > Width - 8) return -1;
            int idx = (p.Y - 4) / _itemHeight;
            if (idx < 0 || idx >= _items.Count) return -1;
            // 每项内边距 4px
            int inner = (p.Y - 4) % _itemHeight;
            if (inner < 4 || inner > _itemHeight - 4) return -1;
            return idx;
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
            if (e.Button == MouseButtons.Left)
            {
                int idx = ItemAt(e.Location);
                if (idx >= 0) SelectedIndex = idx;
            }
        }

        /// <summary>响应 HandleCreated 事件。</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_items.Count > 0 && Height < _items.Count * _itemHeight + 8)
                Height = _items.Count * _itemHeight + 8;
        }
    }
}
