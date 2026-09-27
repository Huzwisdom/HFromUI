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
    /// Fluent 复选框：圆角方框 + 白色对勾，点击在 Checked 两态间翻转。
    /// 指示符可在左/右（CheckAlign），AutoSize 时贴合文字。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("CheckedChanged")]
    public class HFluentCheckBox : HFluentBase
    {
        private const int BoxSize = 18;
        private const int Gap = 8;

        private bool _checked, _autoCheck = true, _hover;
        private ContentAlignment _checkAlign = ContentAlignment.MiddleLeft;
        private Color _accentColor = HFluentPalette.Accent;

        /// <summary>选中态改变。</summary>
        public event EventHandler CheckedChanged;

        /// <summary>初始化。</summary>
        public HFluentCheckBox()
        {
            Size = new Size(130, 24);
            Cursor = Cursors.Hand;
        }

        /// <summary>是否选中。</summary>
        [HCategoryLanguage("Fluent 复选框"), HDisplayNameLanguage("选中"), HDescriptionLanguage("是否选中"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>点击是否自动翻转（false 时仅由代码控制）。</summary>
        [HCategoryLanguage("Fluent 复选框"), HDisplayNameLanguage("自动翻转"), HDescriptionLanguage("点击控件时自动翻转选中态"), Browsable(true)]
        [DefaultValue(true)]
        public bool AutoCheck
        {
            get => _autoCheck;
            set => _autoCheck = value;
        }

        /// <summary>指示符方位（文字另一侧）。</summary>
        [HCategoryLanguage("Fluent 复选框"), HDisplayNameLanguage("指示符方位"), HDescriptionLanguage("勾选框在文字的哪一侧"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get => _checkAlign;
            set { _checkAlign = value; Invalidate(); }
        }

        /// <summary>强调色。</summary>
        [HCategoryLanguage("Fluent 复选框"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("勾选框选中填充色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (_autoCheck) Checked = !_checked;
        }

        /// <summary>自绘勾选框与文字。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Size ts = TextRenderer.MeasureText(g, Text, Font, new Size(Width, Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            bool right = _checkAlign.ToString().EndsWith("Right");
            int x = right ? Width - BoxSize - 2 : 2;
            int by = (Height - BoxSize) / 2;
            int tx = right ? 2 : BoxSize + Gap + 2;
            int tw = Width - BoxSize - Gap - 4;

            var state = g.Save();
            g.TranslateTransform(x, by);
            using (var path = HDrawPaint.CreateOuterBoxPath(BoxSize, BoxSize, 5))
            {
                if (_checked)
                    using (var bb = new SolidBrush(_accentColor))
                        g.FillPath(bb, path);
                else
                {
                    Color border = _hover ? HFluentPalette.Accent : Color.FromArgb(156, 163, 175);
                    using (var pen = new Pen(border, 1.6f))
                        g.DrawPath(pen, path);
                }
            }
            g.Restore(state);

            if (_checked)
                HFluentGlyphDraw.Draw(g, HFluentGlyph.Check,
                    new RectangleF(x + 2f, by + 2f, BoxSize - 4f, BoxSize - 4f), Color.White, 2.4f, false);

            TextRenderer.DrawText(g, Text, Font, new Rectangle(tx, 0, tw, Height),
                ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
