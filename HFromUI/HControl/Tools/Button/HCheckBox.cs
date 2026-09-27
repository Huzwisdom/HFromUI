using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Button
{
    /// <summary>
    /// 多选框：圆角方框 + 矢量对勾，点击翻转 Checked 并触发 CheckedChanged。
    /// CheckAlign 决定方框与文字的方位（九宫格）；Radius&gt;0 时整体套圆角边框。
    /// 滑动开关见 HSlideSwitch。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("CheckedChanged")]
    public class HCheckBox : HButtonBase
    {
        private const int IndicatorGap = 4;

        private static readonly Size SizeCheck = new Size(90, 22);

        private bool _checked;
        private ContentAlignment _checkAlign = ContentAlignment.MiddleLeft;
        private Color _checkedColor = Color.FromArgb(59, 130, 246);
        private float _iconScale = 1f;
        private HIndicatorStyle _indicatorStyle = HIndicatorStyle.Classic;

        public HCheckBox()
        {
            BackColor = Color.Transparent;
            Cursor = Cursors.Default;
            Size = SizeCheck;
        }

        [HCategoryLanguage("复选框"), HDisplayNameLanguage("勾选对齐"), HDescriptionLanguage("方框指示符对齐（文字在另一侧）"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get => _checkAlign;
            set { _checkAlign = value; Invalidate(); }
        }

        [HCategoryLanguage("复选框"), HDisplayNameLanguage("选中强调色"), HDescriptionLanguage("选中时方框底色与对勾描边的颜色"), Browsable(true)]
        public Color CheckedColor
        {
            get => _checkedColor;
            set { _checkedColor = value; Invalidate(); }
        }

        /// <summary>方框与对勾指示符的大小比例：1 为默认（跟随字号），按比例放大或缩小（0.5~3）。</summary>
        [HCategoryLanguage("复选框"), HDisplayNameLanguage("图标大小比例"), HDescriptionLanguage("方框与对勾指示符相对默认大小的倍数，1 为默认（跟随字号）"), Browsable(true)]
        [DefaultValue(1f)]
        public float IconScale
        {
            get => _iconScale;
            set { _iconScale = Math.Max(0.5f, Math.Min(3f, value)); Invalidate(); }
        }

        /// <summary>勾选指示符的外观样式（43 种：方框/圆/菱形/描边/渐变/光效/星心等）。</summary>
        [HCategoryLanguage("复选框"), HDisplayNameLanguage("指示符样式"), HDescriptionLanguage("勾选指示符的外观样式（43种）"), Browsable(true)]
        [DefaultValue(HIndicatorStyle.Classic)]
        public HIndicatorStyle IndicatorStyle
        {
            get => _indicatorStyle;
            set { _indicatorStyle = value; Invalidate(); }
        }

        [HCategoryLanguage("复选框"), HDisplayNameLanguage("选中态"), HDescriptionLanguage("是否勾选"), Browsable(true)]
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

        /// <summary>选中态变化事件。</summary>
        public event EventHandler CheckedChanged;

        /// <summary>文字位置统一由 CheckAlign 表达，基类 TextAlign 对本控件无意义。</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContentAlignment TextAlign
        {
            get => base.TextAlign;
            set => base.TextAlign = value;
        }

        /// <summary>点击即翻转选中态。</summary>
        protected override void OnClick(EventArgs e)
        {
            if (Enabled) Checked = !Checked;
            base.OnClick(e);
        }

        /// <summary>
        /// 不使用窗口区域：曲线路径转像素掩码会沿弧线保守舍入，削掉描边外侧 1~2px，
        /// 圆角会呈现明显折角。本控件透明底、角外不绘内容，由父级画面直接透出。
        /// </summary>
        protected override void UpdateRegion() { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color line = Enabled ? Color.FromArgb(156, 163, 175) : Color.FromArgb(203, 213, 225);
            Color on = Enabled ? _checkedColor : Color.FromArgb(203, 213, 225);

            Rectangle content = PaintCheckFrame(g, _checked ? on : line);

            int box = (int)Math.Round(Math.Min(Math.Max(Font.Height, 14), 18) * _iconScale);
            box = Math.Max(8, Math.Min(64, box));
            Size textSize = TextRenderer.MeasureText(g, Text, Font);
            LayoutIndicator(content, box, textSize, IndicatorGap, _checkAlign, out Rectangle boxRect, out Rectangle textRect);

            HIndicatorRenderer.Draw(g, boxRect, _indicatorStyle, _checked, _checkedColor, Enabled);

            TextRenderer.DrawText(g, Text, Font, textRect,
                Enabled ? ForeColorIfUnset(Color.FromArgb(51, 65, 85)) : Color.FromArgb(160, 166, 175),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (Focused && ShowFocusCues)
            {
                var focus = Rectangle.Union(boxRect, textRect);
                focus.Inflate(2, 1);
                ControlPaint.DrawFocusRectangle(g, focus);
            }
        }
    }
}
