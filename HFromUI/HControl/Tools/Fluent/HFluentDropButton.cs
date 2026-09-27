using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// Fluent 下拉按钮：弱化文字样式 + 下箭头，点击弹出圆角选项卡片，
    /// 选中后按钮文字同步为选项并触发 SelectedIndexChanged。
    /// </summary>
    [DefaultProperty("Items")]
    [DefaultEvent("SelectedIndexChanged")]
    public class HFluentDropButton : HFluentButton
    {
        private readonly List<string> _items = new List<string>();
        private int _selectedIndex = -1;
        private bool _dark = true;
        private ToolStripDropDown _drop;
        private HFluentDropList _list;

        /// <summary>选中项改变。</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>初始化。</summary>
        public HFluentDropButton()
        {
            ButtonKind = HFluentButtonKind.Subtle;
            Glyph = HFluentGlyph.None;
            Text = "Options";
            Size = new Size(120, 34);
        }

        /// <summary>基类绘制文字后，在右侧补一个下拉箭头。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color chevron = _hover ? Color.White : Color.FromArgb(156, 163, 175);
            HFluentGlyphDraw.Draw(g, HFluentGlyph.ChevronDown,
                new RectangleF(Width - 24, Height / 2f - 6, 12, 12), chevron, 1.8f, false);
        }

        /// <summary>选项集合（设计器可编辑）。</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [HCategoryLanguage("Fluent 下拉"), HDisplayNameLanguage("选项集合"), HDescriptionLanguage("下拉选项文本列表"), Browsable(true)]
        public List<string> Items
        {
            get => _items;
        }

        /// <summary>选中索引（-1 未选择）。</summary>
        [HCategoryLanguage("Fluent 下拉"), HDisplayNameLanguage("选中索引"), HDescriptionLanguage("当前选中项索引"), Browsable(true)]
        [DefaultValue(-1)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < -1 || value >= _items.Count) return;
                _selectedIndex = value;
                if (value >= 0) Text = _items[value];
                Invalidate();
            }
        }

        /// <summary>当前选中项文本。</summary>
        [Browsable(false)]
        public string SelectedItem => _selectedIndex >= 0 ? _items[_selectedIndex] : null;

        /// <summary>弹层是否暗色主题（默认暗色）。</summary>
        [HCategoryLanguage("Fluent 下拉"), HDisplayNameLanguage("暗色弹层"), HDescriptionLanguage("下拉卡片是否暗色主题"), Browsable(true)]
        [DefaultValue(true)]
        public bool DarkDrop
        {
            get => _dark;
            set => _dark = value;
        }

        /// <summary>点击弹出选项卡片。</summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowDrop();
        }

        private void ShowDrop()
        {
            if (_items.Count == 0) return;
            _list = new HFluentDropList(_items, _selectedIndex, _dark, Width);
            _list.ItemPicked += idx =>
            {
                _drop.Close();
                SelectedIndex = idx;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            };

            var host = new ToolStripControlHost(_list)
            {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Size = new Size(_list.Width, _list.Height)
            };
            _drop = new ToolStripDropDown
            {
                AutoSize = false,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Renderer = new HFluentNoBorderRenderer(),
                Size = new Size(_list.Width, _list.Height)
            };
            _drop.Items.Add(host);
            _drop.Show(this, new Point(0, Height));
        }
    }

    /// <summary>去除 ToolStripDropDown 默认边框，仅保留自绘圆角卡片。</summary>
    internal class HFluentNoBorderRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // 不绘制系统边框
        }
    }

    /// <summary>下拉选项卡片：圆角、悬停高亮、当前选中项带强调色小圆点。</summary>
    internal class HFluentDropList : Panel
    {
        private const int ItemH = 30;
        private const int Pad = 6;

        private readonly List<string> _items;
        private readonly bool _dark;
        private int _hover = -1;
        private readonly int _selected;

        public event Action<int> ItemPicked;

        public HFluentDropList(List<string> items, int selected, bool dark, int minWidth)
        {
            _items = items;
            _selected = selected;
            _dark = dark;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            BackColor = dark ? HFluentPalette.DarkCard : Color.White;
            Font = new Font("微软雅黑", 9f);
            using (var g = CreateGraphics())
            {
                int w = minWidth;
                foreach (var s in items)
                    w = Math.Max(w, TextRenderer.MeasureText(g, s, Font, Size.Empty,
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width + 34);
                Width = w;
            }
            Height = items.Count * ItemH + Pad * 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color back = _dark ? HFluentPalette.DarkCard : Color.White;
            using (var path = new GraphicsPath())
            {
                int r = 8;
                path.AddArc(0, 0, r, r, 180, 90);
                path.AddArc(Width - 1 - r, 0, r, r, 270, 90);
                path.AddArc(Width - 1 - r, Height - 1 - r, r, r, 0, 90);
                path.AddArc(0, Height - 1 - r, r, r, 90, 90);
                path.CloseFigure();
                using (var bb = new SolidBrush(back)) g.FillPath(bb, path);
                using (var pen = new Pen(_dark ? HFluentPalette.DarkBorder : Color.FromArgb(226, 228, 232)))
                    g.DrawPath(pen, path);
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var row = new Rectangle(Pad, Pad + i * ItemH, Width - Pad * 2, ItemH - 2);
                bool hover = i == _hover;
                bool selected = i == _selected;
                if (hover)
                    using (var hb = new SolidBrush(_dark
                        ? Color.FromArgb(38, 255, 255, 255)
                        : Color.FromArgb(243, 244, 246)))
                    using (var rp = Round(row, 6))
                        g.FillPath(hb, rp);
                Color textColor = _dark ? Color.FromArgb(229, 231, 235) : Color.FromArgb(31, 41, 55);
                if (selected)
                    using (var ab = new SolidBrush(HFluentPalette.Accent))
                        g.FillEllipse(ab, row.Left + 6, row.Top + row.Height / 2f - 3, 6, 6);
                TextRenderer.DrawText(g, _items[i], Font,
                    new Rectangle(row.Left + 18, row.Top, row.Width - 22, row.Height),
                    textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }

        private static GraphicsPath Round(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = radius * 2f;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = (e.Y - Pad) / ItemH;
            if (idx >= 0 && idx < _items.Count)
            {
                if (_hover != idx) { _hover = idx; Invalidate(); }
            }
            else if (_hover != -1) { _hover = -1; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != -1) { _hover = -1; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int idx = (e.Y - Pad) / ItemH;
            if (e.Button == MouseButtons.Left && idx >= 0 && idx < _items.Count)
                ItemPicked?.Invoke(idx);
        }
    }
}
