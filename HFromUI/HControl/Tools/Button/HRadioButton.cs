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
    /// 单选按钮：圆圈 + 内圆点指示，同容器同 GroupName（空串也视为同组）自动互斥。
    /// CheckAlign 决定指示符与文字的方位（九宫格）；Radius&gt;0 时整体套圆角边框。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("CheckedChanged")]
    public class HRadioButton : HButtonBase
    {
        private const int IndicatorGap = 4;

        private static readonly Size SizeRadio = new Size(90, 22);

        /// <summary>同组互斥保护：取消兄弟时防止其再次反向联动。</summary>
        private bool _mutexGuard;
        private bool _checked;
        private string _groupName = string.Empty;
        private ContentAlignment _checkAlign = ContentAlignment.MiddleLeft;
        private Color _checkedColor = Color.FromArgb(59, 130, 246);
        private float _iconScale = 1f;
        private HIndicatorStyle _indicatorStyle = HIndicatorStyle.RingDot;

        public HRadioButton()
        {
            BackColor = Color.Transparent;
            Cursor = Cursors.Default;
            Size = SizeRadio;
        }

        [HCategoryLanguage("单选按钮"), HDisplayNameLanguage("单选分组名"), HDescriptionLanguage("单选分组名（空串表示同容器内自动成组）"), Browsable(true)]
        [DefaultValue("")]
        public string GroupName
        {
            get => _groupName;
            set => _groupName = value ?? string.Empty;
        }

        [HCategoryLanguage("单选按钮"), HDisplayNameLanguage("圆点指示符对齐"), HDescriptionLanguage("圆点指示符对齐（文字在另一侧）"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get => _checkAlign;
            set { _checkAlign = value; Invalidate(); }
        }

        [HCategoryLanguage("单选按钮"), HDisplayNameLanguage("选中强调色"), HDescriptionLanguage("选中时圆圈与内圆点的颜色"), Browsable(true)]
        public Color CheckedColor
        {
            get => _checkedColor;
            set { _checkedColor = value; Invalidate(); }
        }

        /// <summary>圆圈与内圆点指示符的大小比例：1 为默认（跟随字号），按比例放大或缩小（0.5~3）。</summary>
        [HCategoryLanguage("单选按钮"), HDisplayNameLanguage("图标大小比例"), HDescriptionLanguage("圆圈与内圆点指示符相对默认大小的倍数，1 为默认（跟随字号）"), Browsable(true)]
        [DefaultValue(1f)]
        public float IconScale
        {
            get => _iconScale;
            set { _iconScale = Math.Max(0.5f, Math.Min(3f, value)); Invalidate(); }
        }

        /// <summary>圆点指示符的外观样式（43 种：圆圈/方框/描边/渐变/光效/星心等）。</summary>
        [HCategoryLanguage("单选按钮"), HDisplayNameLanguage("指示符样式"), HDescriptionLanguage("圆点指示符的外观样式（43种）"), Browsable(true)]
        [DefaultValue(HIndicatorStyle.RingDot)]
        public HIndicatorStyle IndicatorStyle
        {
            get => _indicatorStyle;
            set { _indicatorStyle = value; Invalidate(); }
        }

        [HCategoryLanguage("单选按钮"), HDisplayNameLanguage("选中态"), HDescriptionLanguage("是否选中（同组自动互斥）"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                if (value) SyncRadioSiblings();
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

        /// <summary>点击未选中的单选才置中，已选中不回弹。</summary>
        protected override void OnClick(EventArgs e)
        {
            if (Enabled && !_checked) Checked = true;
            base.OnClick(e);
        }

        /// <summary>
        /// 不使用窗口区域：曲线路径转像素掩码会沿弧线保守舍入，削掉描边外侧 1~2px，
        /// 圆角会呈现明显折角。本控件透明底、角外不绘内容，由父级画面直接透出。
        /// </summary>
        protected override void UpdateRegion() { }

        /// <summary>选中时联动取消同组其他单选（原生 RadioButton 语义的矢量实现）。</summary>
        private void SyncRadioSiblings()
        {
            if (Parent == null || _mutexGuard) return;
            foreach (Control c in Parent.Controls)
            {
                if (c is HRadioButton b && b != this && b._groupName == _groupName && b.Checked)
                {
                    b._mutexGuard = true;
                    b.Checked = false;
                    b._mutexGuard = false;
                }
            }
        }

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
