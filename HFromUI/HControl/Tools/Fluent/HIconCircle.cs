using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// 圆形图标钮：纯色圆盘 + 居中白色矢量图标（√/×/i/?/★/! 等），
    /// 悬停提亮、按下压暗并下沉 1px。
    /// </summary>
    [DefaultProperty("Glyph")]
    [DefaultEvent("Click")]
    public class HIconCircle : HFluentBase
    {
        private HFluentGlyph _glyph = HFluentGlyph.Info;
        private Color _circleColor = HFluentPalette.Info;
        private bool _fillGlyph = true;
        private bool _hover, _pressed;

        /// <summary>初始化默认尺寸。</summary>
        public HIconCircle()
        {
            Size = new Size(44, 44);
            Cursor = Cursors.Hand;
        }

        /// <summary>居中图标。</summary>
        [HCategoryLanguage("圆形图标钮"), HDisplayNameLanguage("图标"), HDescriptionLanguage("圆盘中央的矢量图标"), Browsable(true)]
        [DefaultValue(HFluentGlyph.Info)]
        public HFluentGlyph Glyph
        {
            get => _glyph;
            set { _glyph = value; Invalidate(); }
        }

        /// <summary>圆盘颜色。</summary>
        [HCategoryLanguage("圆形图标钮"), HDisplayNameLanguage("圆盘颜色"), HDescriptionLanguage("实心圆盘背景色"), Browsable(true)]
        public Color CircleColor
        {
            get => _circleColor;
            set { _circleColor = value; Invalidate(); }
        }

        /// <summary>图标是否填充（仅星形等填充图标有区别）。</summary>
        [HCategoryLanguage("圆形图标钮"), HDisplayNameLanguage("图标填充"), HDescriptionLanguage("图标是否实心填充"), Browsable(true)]
        [DefaultValue(true)]
        public bool FillGlyph
        {
            get => _fillGlyph;
            set { _fillGlyph = value; Invalidate(); }
        }

        protected override void OnMouseEnter(System.EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(System.EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        /// <summary>自绘圆盘与图标。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color c = _circleColor;
            if (_pressed) c = PressColor(c);
            else if (_hover) c = HoverColor(c);

            int inset = _pressed ? 1 : 0;
            var box = new RectangleF(inset, inset, Width - 1f - inset * 2, Height - 1f - inset * 2);
            using (var bb = new SolidBrush(c))
                g.FillEllipse(bb, box);

            float gs = Width * 0.52f;
            var glyphBox = new RectangleF((Width - gs) / 2f, (Height - gs) / 2f, gs, gs);
            HFluentGlyphDraw.Draw(g, _glyph, glyphBox, Color.White, Math.Max(1.8f, gs * 0.1f), _fillGlyph);
        }
    }
}
