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
    using HFromUI.HColor;
    [HDescriptionLanguage("圆角textbox，可自定义粗细及颜色")]
    public partial class HNumericUpDown : HControlBase
    {
        #region 私有属性

        /// <summary>maxinum 字段。</summary>
        private int maxinum = 100;

        /// <summary>mininum 字段。</summary>
        private int mininum = 0;

        /// <summary>值。</summary>
        private int value = 0;

        /// <summary>padleftCount 字段。</summary>
        private int padleftCount = 0;

        /// <summary>buttonSize 字段。</summary>
        private Size buttonSize = new Size(13, 13);

        #endregion 私有属性

        /// <summary>
        /// 字符串颜色
        /// </summary>
        [HDescriptionLanguage("字符串颜色")]
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

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("显示位数"), HDescriptionLanguage("显示位数"), Browsable(true)]
        public int PadLeftCount
        {
            get { return padleftCount; }
            set
            {
                if (value >= 0)
                    padleftCount = value;
            }
        }

        /// <summary>值变化事件（按钮调节、键盘输入、代码赋值均会触发；值未真正改变时不触发）。</summary>
        public event EventHandler ValueChanged;

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("值"), HDescriptionLanguage("值"), Browsable(true)]
        public int Value
        {
            get { return value; }
            set
            {
                if (value <= maxinum && value >= mininum)
                {
                    bool changed = this.value != value;
                    this.value = value;
                    if (padleftCount > 0)
                    {
                        textBox1.Text = this.value.ToString().PadLeft(padleftCount, '0');
                    }
                    else
                    {
                        textBox1.Text = this.value.ToString();
                    }
                    if (changed)
                    {
                        try { ValueChanged?.Invoke(this, EventArgs.Empty); } catch { }
                    }
                }
            }
        }

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("最大值"), HDescriptionLanguage("最大值"), Browsable(true)]
        public int Maxinum
        {
            get { return maxinum; }
            set
            {
                maxinum = value;
                if (maxinum < mininum)
                {
                    mininum = maxinum;
                }
            }
        }

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("最小值"), HDescriptionLanguage("最小值"), Browsable(true)]
        public int Mininum
        {
            get { return mininum; }
            set
            {
                mininum = value;
                if (mininum > maxinum)
                {
                    maxinum = mininum;
                }
            }
        }

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("按钮大小"), HDescriptionLanguage("按钮大小"), Browsable(true)]
        public Size ButtonSize
        {
            get { return buttonSize; }
            set
            {
                if (value.Width > 0 && value.Width < this.Width - Radius - 4 && value.Height < this.Height / 2)
                {
                    buttonSize = value;
                    pictureBox1.Size = pictureBox2.Size = value;
                    pictureBox1.Location = new Point(base.Width - buttonSize.Width - Radius / 2, 0 + (this.Height / 2 - buttonSize.Height) / 2);
                    pictureBox2.Location = new Point(base.Width - buttonSize.Width - Radius / 2, this.Height / 2 + (this.Height / 2 - buttonSize.Height) / 2);
                }
            }
        }

        /// <summary>buttonPicUp 字段。</summary>
        private Bitmap buttonPicUp = HPhoto.Get("up");
        /// <summary>buttonPicUpHover 字段。</summary>
        private Bitmap buttonPicUpHover = HPhoto.Get("upBlue");
        /// <summary>buttonPicDown 字段。</summary>
        private Bitmap buttonPicDown = HPhoto.Get("down");
        /// <summary>buttonPicDownHover 字段。</summary>
        private Bitmap buttonPicDownHover = HPhoto.Get("downBlue");

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("上按钮图片"), HDescriptionLanguage("上按钮图片"), Browsable(true)]
        public Bitmap ButtonPicUP
        {
            get
            {
                return buttonPicUp;
            }
            set
            {
                if (value != null)
                {
                    buttonPicUp = value;
                    pictureBox1.Image = buttonPicUp;
                }
            }
        }

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("上调按钮悬停图片"), HDescriptionLanguage("上按钮Hover图片"), Browsable(true)]
        public Bitmap ButtonPicUPHover
        {
            get
            {
                return buttonPicUpHover;
            }
            set
            {
                if (value != null)
                {
                    buttonPicUpHover = value;
                }
            }
        }

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("下按钮图片"), HDescriptionLanguage("下按钮图片"), Browsable(true)]
        public Bitmap ButtonPicDown
        {
            get
            {
                return buttonPicDown;
            }
            set
            {
                if (value != null)
                {
                    buttonPicDown = value;
                    pictureBox2.Image = buttonPicDown;
                }
            }
        }

        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("下调按钮悬停图片"), HDescriptionLanguage("下按钮Hover图片"), Browsable(true)]
        public Bitmap ButtonPicDownHover
        {
            get
            {
                return buttonPicDownHover;
            }
            set
            {
                if (value != null)
                {
                    buttonPicDownHover = value;
                }
            }
        }

        public HNumericUpDown()
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
            this.RadiusChanged += PPNumericUpDown_RadiusChanged;

            this.BorderStyle = BorderStyle.None;
            textBox1.Width = this.Width - this.Height - 20;
            textBox1.Location = new Point(this.Height / 2 + 2, 5);
            pictureBox1.Size = new Size(13, 13);
            pictureBox2.Size = new Size(13, 13);
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
            if (this.Width <= this.Radius + buttonSize.Width + 4)
            {
                this.Width = this.Radius + buttonSize.Width + 5;
            }

            if (this.Height <= buttonSize.Height * 2)
            {
                this.Height = buttonSize.Height * 2;
            }

            if (this.Height <= textBox1.Height + 2)
            {
                this.Height = textBox1.Height + 2;
            }

            textBox1.Width = this.Width - Radius - buttonSize.Width - 4;
            textBox1.Location = new Point(this.Radius / 2 + 2, (this.Height - textBox1.Height) / 2);

            pictureBox1.Location = new Point(base.Width - buttonSize.Width - Radius / 2, 0 + (this.Height / 2 - buttonSize.Height) / 2);
            pictureBox2.Location = new Point(base.Width - buttonSize.Width - Radius / 2, this.Height / 2 + (this.Height / 2 - buttonSize.Height) / 2);
        }

        /// <summary>响应 BackColorChanged 事件。</summary>
        protected override void OnBackColorChanged(EventArgs e)
        {
            textBox1.BackColor = this.BackColor;
            this.Invalidate();
            base.OnBackColorChanged(e);
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

        /// <summary>PPNumericUpDown_RadiusChanged 方法。</summary>
        private void PPNumericUpDown_RadiusChanged(object sender, EventArgs e)
        {
            if (this.Width <= this.Radius + buttonSize.Width + 4)
            {
                this.Width = this.Radius + buttonSize.Width + 5;
            }

            if (this.Height <= buttonSize.Height * 2)
            {
                this.Height = buttonSize.Height * 2;
            }

            if (this.Height <= textBox1.Height + 2)
            {
                this.Height = textBox1.Height + 2;
            }

            textBox1.Width = this.Width - Radius - buttonSize.Width - 4;
            textBox1.Location = new Point(this.Radius / 2 + 2, (this.Height - textBox1.Height) / 2);

            pictureBox1.Location = new Point(base.Width - buttonSize.Width - Radius / 2, 0 + (this.Height / 2 - buttonSize.Height) / 2);
            pictureBox2.Location = new Point(base.Width - buttonSize.Width - Radius / 2, this.Height / 2 + (this.Height / 2 - buttonSize.Height) / 2);
        }

        /// <summary>textBox1_SizeChanged 方法。</summary>
        private void textBox1_SizeChanged(object sender, EventArgs e)
        {
            if (this.Width <= this.Radius + buttonSize.Width + 4)
            {
                this.Width = this.Radius + buttonSize.Width + 5;
            }

            if (this.Height <= buttonSize.Height * 2)
            {
                this.Height = buttonSize.Height * 2;
            }

            if (this.Height <= textBox1.Height + 2)
            {
                this.Height = textBox1.Height + 2;
            }

            textBox1.Width = this.Width - Radius - buttonSize.Width - 4;
            textBox1.Location = new Point(this.Radius / 2 + 2, (this.Height - textBox1.Height) / 2);

            pictureBox1.Location = new Point(base.Width - buttonSize.Width - Radius / 2, 0 + (this.Height / 2 - buttonSize.Height) / 2);
            pictureBox2.Location = new Point(base.Width - buttonSize.Width - Radius / 2, this.Height / 2 + (this.Height / 2 - buttonSize.Height) / 2);
        }

        /// <summary>清空。</summary>
        public void Clear()
        {
            textBox1.Clear();
        }

        /// <summary>textBox1_TextChanged 方法。</summary>
        [Browsable(true)]
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num; if (!int.TryParse(textBox1.Text, out num)) return;
                if (num >= mininum && num <= maxinum)
                {
                    Value = num;
                }
                else if (num > maxinum)
                {
                    Value = maxinum;
                }
                else if (num < mininum)
                {
                    Value = mininum;
                }
            }
            catch
            {
            }
        }

        /// <summary>textBox1_KeyPress 方法。</summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != (char)8;  //允许输入数字
        }

        /// <summary>pictureBox1_MouseEnter 方法。</summary>
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Image = buttonPicUpHover;
        }

        /// <summary>pictureBox1_MouseLeave 方法。</summary>
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = buttonPicUp;
        }

        /// <summary>pictureBox2_MouseEnter 方法。</summary>
        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = buttonPicDownHover;
        }

        /// <summary>pictureBox2_MouseLeave 方法。</summary>
        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = buttonPicDown;
        }

        /// <summary>pictureBox1_Click 方法。</summary>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Value += 1;
        }

        /// <summary>pictureBox2_Click 方法。</summary>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Value -= 1;
        }

        /// <summary>PPNumericUpDown_BaseColorChanged 方法。</summary>
        private void PPNumericUpDown_BaseColorChanged(object sender, EventArgs e)
        {
            textBox1.BackColor = this.BaseColor;
        }
    }
}