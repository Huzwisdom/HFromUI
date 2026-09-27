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
    /// 滑动开关：圆钮沿轨道滑行，点击翻转 Checked 并触发 CheckedChanged。
    /// 无文字（Text 为 null/空）时，轨道铺满控件 Padding 内的整块区域；
    /// 有文字时轨道居左/居右，文字排在另一侧（CheckAlign 决定方位）。
    /// SwitchStyle 提供 64 种轨道/圆钮外观，CheckedColor/UncheckedColor 为开/关基准色，
    /// 圆钮滑行由自有定时器缓动驱动。
    /// </summary>
    [DefaultProperty("Checked")]
    [DefaultEvent("CheckedChanged")]
    public class HSlideSwitch : HButtonBase
    {
        private static readonly Size SizeSwitch = new Size(52, 24);

        private bool _checked;
        private HSlideSwitchStyle _style = HSlideSwitchStyle.IOS;
        private ContentAlignment _checkAlign = ContentAlignment.MiddleLeft;
        private string _onText = string.Empty;
        private string _offText = string.Empty;
        private Color _checkedColor = Color.FromArgb(59, 130, 246);
        private Color _uncheckedColor = Color.FromArgb(203, 213, 225);

        /// <summary>开关圆钮滑动进度（0 关 → 1 开）。</summary>
        private float _knob;
        private Timer _animTimer;
        private Font _tinyFont;

        public HSlideSwitch()
        {
            BackColor = Color.Transparent;
            Cursor = Cursors.Default;
            Size = SizeSwitch;
            Text = null;
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("开关样式"), HDescriptionLanguage("轨道与圆钮的外观样式（64 种）"), Browsable(true)]
        [DefaultValue(HSlideSwitchStyle.IOS)]
        public HSlideSwitchStyle SwitchStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("开关对齐"), HDescriptionLanguage("轨道指示符对齐（文字在另一侧）"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment CheckAlign
        {
            get => _checkAlign;
            set { _checkAlign = value; Invalidate(); }
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("开启轨道色"), HDescriptionLanguage("开启状态的轨道基准色（部分样式使用固定语义色）"), Browsable(true)]
        public Color CheckedColor
        {
            get => _checkedColor;
            set { _checkedColor = value; Invalidate(); }
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("关闭轨道色"), HDescriptionLanguage("关闭状态的轨道基准色"), Browsable(true)]
        public Color UncheckedColor
        {
            get => _uncheckedColor;
            set { _uncheckedColor = value; Invalidate(); }
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("开启文本"), HDescriptionLanguage("开关开启时轨道内短文字（如 开）"), Browsable(true)]
        [DefaultValue("")]
        public string OnText
        {
            get => _onText;
            set { _onText = value ?? string.Empty; Invalidate(); }
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("关闭文本"), HDescriptionLanguage("开关关闭时轨道内短文字（如 关）"), Browsable(true)]
        [DefaultValue("")]
        public string OffText
        {
            get => _offText;
            set { _offText = value ?? string.Empty; Invalidate(); }
        }

        [HCategoryLanguage("滑动开关"), HDisplayNameLanguage("选中态"), HDescriptionLanguage("是否开启"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                StartAnimation();
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>默认无文字：显示文本时文字排在轨道另一侧。</summary>
        [DefaultValue(null)]
        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        /// <summary>选中态变化事件。</summary>
        public event EventHandler CheckedChanged;

        /// <summary>滑动开关无圆角外框/文字对齐设置，隐藏基类无关属性。</summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int Radius
        {
            get => base.Radius;
            set => base.Radius = value;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int BorderWidth
        {
            get => base.BorderWidth;
            set => base.BorderWidth = value;
        }

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

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            _tinyFont?.Dispose();
            _tinyFont = null;
            Invalidate();
        }

        /// <summary>圆钮滑行缓动定时器（15ms 一帧）。</summary>
        private void StartAnimation()
        {
            if (_animTimer == null)
            {
                _animTimer = new Timer { Interval = 15 };
                _animTimer.Tick += (s, e) =>
                {
                    float target = _checked ? 1f : 0f;
                    if (Math.Abs(_knob - target) <= 0.01f)
                    {
                        _knob = target;
                        _animTimer.Stop();
                    }
                    else _knob += (target - _knob) * 0.3f;
                    Invalidate();
                };
            }
            _animTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (BackColor.A == 255)
                using (var bb = new SolidBrush(BackColor))
                    g.FillRectangle(bb, ClientRectangle);

            Padding pad = Padding;
            bool hasText = !string.IsNullOrEmpty(Text);
            bool labelRight = _checkAlign == ContentAlignment.MiddleLeft ||
                _checkAlign == ContentAlignment.BottomLeft || _checkAlign == ContentAlignment.TopLeft;

            // 无文字：轨道 = 控件尺寸减去 Padding；有文字：轨道固定比例贴左/贴右
            RectangleF track;
            if (!hasText)
            {
                track = new RectangleF(pad.Left, pad.Top,
                    Math.Max(1, Width - pad.Horizontal), Math.Max(1, Height - pad.Vertical));
            }
            else
            {
                int availH = Math.Max(1, Height - pad.Vertical);
                int trackH = Math.Min(Math.Max(availH, 10), 26);
                int trackW = (int)(trackH * 1.85f);
                int trackX = labelRight ? pad.Left : Width - pad.Right - trackW;
                int trackY = pad.Top + (availH - trackH) / 2;
                track = new Rectangle(trackX, trackY, trackW, trackH);
            }

            Font tiny = _tinyFont ?? (_tinyFont = new Font(Font.FontFamily, Math.Max(6.5f, Font.Size - 2f)));
            HSlideSwitchRenderer.Draw(g, track, _style, _knob,
                _checkedColor, _uncheckedColor, Enabled, _onText, _offText, tiny);

            if (hasText)
            {
                Rectangle textRect;
                if (labelRight)
                {
                    int x = (int)track.Right + 4;
                    textRect = new Rectangle(x, pad.Top, Width - pad.Right - x, Height - pad.Vertical);
                }
                else
                {
                    int right = (int)track.Left - 4;
                    textRect = new Rectangle(pad.Left, pad.Top, right - pad.Left, Height - pad.Vertical);
                }
                Color textColor = Enabled ? ForeColorIfUnset(Color.FromArgb(51, 65, 85)) : Color.FromArgb(160, 166, 175);
                TextRenderer.DrawText(g, Text, Font, textRect, textColor,
                    TextFormatFromAlign(labelRight ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight) | TextFormatFlags.EndEllipsis);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Dispose();
                _tinyFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
