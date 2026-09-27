using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// 分段选择控件：胶囊轨道内若干等分文字段，选中段为强调色滑块，
    /// 点击切换，滑块带缓动。Dark=true 时为暗色面板样式（默认）。
    /// </summary>
    [DefaultProperty("Items")]
    [DefaultEvent("SelectedIndexChanged")]
    public class HSegmented : HFluentBase
    {
        private readonly List<string> _items = new List<string>();
        private int _selectedIndex;
        private int _hover = -1;
        private bool _dark = true;
        private Color _accentColor = HFluentPalette.Accent;
        private float _slideX;
        private readonly Timer _timer;

        /// <summary>选中项改变。</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>初始化。</summary>
        public HSegmented()
        {
            Size = new Size(240, 38);
            Cursor = Cursors.Hand;
            _timer = new Timer { Interval = 15 };
            _timer.Tick += (s, e) =>
            {
                float target = TargetX();
                _slideX += (target - _slideX) * 0.3f;
                if (Math.Abs(target - _slideX) < 0.6f) { _slideX = target; _timer.Stop(); }
                Invalidate();
            };
        }

        /// <summary>分段项集合（至少 1 项）。</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [HCategoryLanguage("分段选择"), HDisplayNameLanguage("分段项"), HDescriptionLanguage("分段文字集合"), Browsable(true)]
        public List<string> Items
        {
            get => _items;
        }

        /// <summary>选中索引。</summary>
        [HCategoryLanguage("分段选择"), HDisplayNameLanguage("选中索引"), HDescriptionLanguage("当前选中分段索引"), Browsable(true)]
        [DefaultValue(0)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                int count = _items.Count;
                if (count == 0) return;
                value = Math.Max(0, Math.Min(count - 1, value));
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                if (IsHandleCreated) _timer.Start();
                else { _slideX = TargetX(); Invalidate(); }
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>是否暗色主题（轨道深灰、未选文字浅灰）。</summary>
        [HCategoryLanguage("分段选择"), HDisplayNameLanguage("暗色主题"), HDescriptionLanguage("暗色面板上使用"), Browsable(true)]
        [DefaultValue(true)]
        public bool Dark
        {
            get => _dark;
            set { _dark = value; Invalidate(); }
        }

        /// <summary>选中滑块强调色。</summary>
        [HCategoryLanguage("分段选择"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("选中滑块颜色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        private int Count => _items.Count;
        private int SegW => Count == 0 ? Width : Width / Count;

        private float TargetX()
        {
            return Count == 0 ? 3f : 3f + _selectedIndex * SegW;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _slideX = TargetX();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _slideX = TargetX();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = Count == 0 ? -1 : Math.Min(Count - 1, e.X / SegW);
            if (idx != _hover) { _hover = idx; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != -1) { _hover = -1; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && Count > 0)
                SelectedIndex = Math.Min(Count - 1, e.X / SegW);
        }

        /// <summary>自绘轨道、滑块与各段文字。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (Count == 0) return;

            int r = Height / 2;
            using (var track = new GraphicsPath())
            {
                track.AddArc(0, 0, Height - 1, Height - 1, 90, 180);
                track.AddArc(Width - Height, 0, Height - 1, Height - 1, 270, 180);
                track.CloseFigure();
                using (var bb = new SolidBrush(_dark ? HFluentPalette.DarkCard : HFluentPalette.Track))
                    g.FillPath(bb, track);
            }

            int sw = SegW, sh = Height - 6;
            using (var pill = new GraphicsPath())
            {
                pill.AddArc(_slideX, 3, sh, sh, 90, 180);
                pill.AddArc(_slideX + sw - sh, 3, sh, sh, 270, 180);
                pill.CloseFigure();
                using (var bb = new SolidBrush(_accentColor))
                    g.FillPath(bb, pill);
            }

            for (int i = 0; i < Count; i++)
            {
                bool selected = i == _selectedIndex;
                Color c = selected ? Color.White :
                    (_dark ? Color.FromArgb(209, 213, 219) : HFluentPalette.MutedText);
                var rect = new Rectangle(i * sw, 0, sw, Height);
                TextRenderer.DrawText(g, _items[i], Font, rect, c,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
