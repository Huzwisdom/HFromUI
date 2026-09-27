using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Text
{
    [HDescriptionLanguage("圆角textbox，可自定义粗细及颜色"), DefaultEvent("TextChanged")]
    public partial class HTextbox : HControlBase
    {
        /// <summary>tip 字段。</summary>
        private ToolTip tip = new ToolTip();

        #region 属性定义

        private string watermarkText;//水印
        /// <summary>showTip 字段。</summary>
        private bool showTip = false;
        private string regex = "";//正则表达式

        /// <summary>
        /// 水印文字
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("水印文字"), HDescriptionLanguage("水印文字"), Browsable(true)]
        public string WatermarkText
        {
            get { return watermarkText; }
            set
            {
                watermarkText = value;
                textBox1.WatermarkText = watermarkText;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 字符串
        /// </summary>
        [HDescriptionLanguage("字符串")]
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本")]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), EditorBrowsable(EditorBrowsableState.Always), Bindable(true)]
        public override string Text
        {
            get { return textBox1.Text; }
            set
            {
                textBox1.Text = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本对齐方式"), HDescriptionLanguage("文本对齐方式"), Browsable(true)]
        public HorizontalAlignment TextAlign
        {
            get { return textBox1.TextAlign; }
            set
            {
                textBox1.TextAlign = value;
            }
        }

        /// <summary>
        /// 字符串颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("字符串颜色"), HDescriptionLanguage("字符串颜色"), Browsable(true)]
        public override Color ForeColor
        {
            get
            {
                return textBox1.ForeColor;
            }

            set
            {
                base.ForeColor = value;
                textBox1.ForeColor = base.ForeColor;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("最大长度"), HDescriptionLanguage("最大字符串长度"), Browsable(true)]
        public int MaxLength
        {
            get { return textBox1.MaxLength; }
            set
            {
                textBox1.MaxLength = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("密码字符"), HDescriptionLanguage("该字符用来屏蔽单行Textbox中的密码字符"), Browsable(true)]
        public char PasswordChar
        {
            get { return textBox1.PasswordChar; }
            set
            {
                textBox1.PasswordChar = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("只读"), HDescriptionLanguage("是否只读"), Browsable(true)]
        public bool ReadOnly
        {
            get { return textBox1.ReadOnly; }
            set
            {
                textBox1.ReadOnly = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示提示"), HDescriptionLanguage("是否显示悬浮提示"), Browsable(true)]
        public bool ShowTip
        {
            get { return showTip; }
            set
            {
                showTip = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("使用正则表达式"), HDescriptionLanguage("使用正则表达式，为空则不使用"), Browsable(true)]
        public string Regex
        {
            get { return regex; }
            set
            {
                regex = value;
            }
        }

        #endregion 属性定义

        public HTextbox()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.UpdateStyles();
            this.RadiusChanged += new EventHandler(This_RadiusChanged);
            this.BaseColorChanged += new EventHandler(This_BaseColorChanged);
        }

        /// <summary>
        /// 重写CreateParams方法 解决控件过多加载闪烁问题(会导致视频无法播放)
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            textBox1.Width = this.Width - Radius - 4;
            textBox1.Location = new Point(Radius / 2 + 2, (this.Height - textBox1.Height) / 2);
        }

        /// <summary>This_RadiusChanged 方法。</summary>
        private void This_RadiusChanged(object sender, EventArgs e)
        {
            //this.Height = textBox1.Height + (int)(2 * BorderWidth) + 4;
            textBox1.Width = this.Width - Radius - 4;
            textBox1.Location = new Point(Radius / 2 + 2, (this.Height - textBox1.Height) / 2);
        }

        /// <summary>This_BaseColorChanged 方法。</summary>
        private void This_BaseColorChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = BaseColor;
        }

        /// <summary>响应 BackColorChanged 事件。</summary>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            textBox1.BackColor = BaseColor;
        }

        /// <summary>响应 FontChanged 事件。</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            this.textBox1.Font = base.Font;
            this.Invalidate();
            base.OnFontChanged(e);
        }

        /// <summary>响应 ForeColorChanged 事件。</summary>
        protected override void OnForeColorChanged(EventArgs e)
        {
            textBox1.ForeColor = base.ForeColor;
            this.Invalidate();
            base.OnForeColorChanged(e);
        }

        /// <summary>响应 EnabledChanged 事件。</summary>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            this.Invalidate();
        }

        /// <summary>textBox1_SizeChanged 方法。</summary>
        private void textBox1_SizeChanged(object sender, EventArgs e)
        {
            //this.Height = textBox1.Height + (int)(2 * BorderWidth) + 4;
            textBox1.Width = this.Width - Radius - 4;
            textBox1.Location = new Point(Radius / 2 + 2, (this.Height - textBox1.Height) / 2);
        }

        /// <summary>清空。</summary>
        public void Clear()
        {
            textBox1.Clear();
        }

        [Browsable(true)]
        public new event EventHandler TextChanged;

        /// <summary>textBox1_TextChanged 方法。</summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(this, new EventArgs());
            }
        }

        /// <summary>textBox1_MouseEnter 方法。</summary>
        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            if (showTip)
            {
                tip.Show(textBox1.Text, textBox1);
            }
        }

        /// <summary>textBox1_MouseLeave 方法。</summary>
        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(textBox1);
        }

        /// <summary>选择。</summary>
        public void Select(int start, int length)
        {
            textBox1.Select(start, length);
        }

        /// <summary>textBox1_KeyPress 方法。</summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (regex != "")
            {
                System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(regex);
                string str = e.KeyChar.ToString();
                if (!reg1.IsMatch(str) && !(e.KeyChar == 8 || e.KeyChar == 37 || e.KeyChar == 38 || e.KeyChar == 39 || e.KeyChar == 40))//不满足正则表达式且不是退格和方向键
                {
                    e.Handled = true;
                }
            }

            base.OnKeyPress(e);
        }
    }
}