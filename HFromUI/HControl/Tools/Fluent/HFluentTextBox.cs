using System.ComponentModel;
using System.Drawing;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 圆角输入框：继承 <see cref="HTextBoxBase"/> 的全部编辑能力
    /// （插入符/选区/IME/撤销等），仅统一样式为圆角描边、强调色聚焦描边，
    /// Dark=true 时为暗色输入框（Send a message 风格）。
    /// </summary>
    public class HFluentTextBox : HTextBoxBase
    {
        private bool _dark = true;

        /// <summary>初始化圆角与强调色。</summary>
        public HFluentTextBox()
        {
            Size = new Size(220, 34);
            Radius = 16;
            BorderWidth = 2;
            UnderlineColor = Color.FromArgb(75, 85, 99);
            UnderlineHoverColor = Color.FromArgb(156, 163, 175);
            UnderlineActiveColor = HFluentPalette.Accent;
            ApplyTheme();
        }

        /// <summary>是否暗色主题（默认暗色）。</summary>
        [Category("Fluent 输入框")]
        [Description("暗色窗体上使用深色底浅文字，false 为浅色底深文字")]
        [DefaultValue(true)]
        public bool Dark
        {
            get => _dark;
            set { _dark = value; ApplyTheme(); Invalidate(); }
        }

        private void ApplyTheme()
        {
            if (_dark)
            {
                ShapeBackColor = Color.FromArgb(48, 48, 48);
                BackColor = ShapeBackColor;
                ForeColor = Color.FromArgb(229, 231, 235);
                WatermarkColor = Color.FromArgb(120, 125, 133);
            }
            else
            {
                ShapeBackColor = Color.White;
                BackColor = Color.White;
                ForeColor = Color.FromArgb(31, 41, 55);
                WatermarkColor = Color.FromArgb(156, 163, 175);
            }
        }
    }
}
