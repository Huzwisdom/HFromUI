using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 单选钮：空心圆环 + 选中强调色内点，同容器内互斥
    /// （点击选中后自动取消同一父容器中其它 Fluent 单选钮）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("CheckedChanged")]
    public class HFluentRadioButton : HFluentBase
    {
        private const int Circle = 20;
        private const int Gap = 8;

        private bool _checked, _autoCheck = true, _hover;
        private Color _accentColor = HFluentPalette.Accent;

        /// <summary>选中态改变。</summary>
        public event EventHandler CheckedChanged;

        /// <summary>初始化。</summary>
        public HFluentRadioButton()
        {
            Size = new Size(130, 26);
            Cursor = Cursors.Hand;
        }

        /// <summary>是否选中。</summary>
        [HCategoryLanguage("Fluent 单选钮"), HDisplayNameLanguage("选中"), HDescriptionLanguage("是否选中"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set => SetChecked(value, true);
        }

        /// <summary>点击是否自动选中（false 时仅由代码控制）。</summary>
        [HCategoryLanguage("Fluent 单选钮"), HDisplayNameLanguage("自动选中"), HDescriptionLanguage("点击控件时自动选中"), Browsable(true)]
        [DefaultValue(true)]
        public bool AutoCheck
        {
            get => _autoCheck;
            set => _autoCheck = value;
        }

        /// <summary>强调色。</summary>
        [HCategoryLanguage("Fluent 单选钮"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("选中圆环与内点颜色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        private void SetChecked(bool value, bool fire)
        {
            if (_checked == value) return;
            _checked = value;
            Invalidate();
            if (value && Parent != null)
            {
                // 同容器互斥：取消其它单选钮
                foreach (Control c in Parent.Controls)
                {
                    if (!ReferenceEquals(c, this) && c is HFluentRadioButton other && other.Checked)
                        other.SetChecked(false, true);
                }
            }
            if (fire) CheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_autoCheck && !_checked) SetChecked(true, true);
        }

        /// <summary>自绘圆环与文字。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int cy = Height / 2;
            var outer = new RectangleF(1, cy - Circle / 2f, Circle, Circle);
            Color ring = _checked ? _accentColor : (_hover ? HFluentPalette.Accent : Color.FromArgb(156, 163, 175));
            using (var pen = new Pen(ring, 2f))
                g.DrawEllipse(pen, outer);
            if (_checked)
            {
                float d = Circle * 0.42f;
                using (var bb = new SolidBrush(_accentColor))
                    g.FillEllipse(bb, 1 + (Circle - d) / 2f, cy - d / 2f, d, d);
            }

            TextRenderer.DrawText(g, Text, Font,
                new Rectangle(Circle + Gap + 2, 0, Width - Circle - Gap - 4, Height),
                ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
