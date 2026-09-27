using HFromUI.HAttribute;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Date
{
    using HFromUI.HColor;
    public partial class HDateTimePicker : HControlBase
    {
        #region 私有变量

        /// <summary>值。</summary>
        private DateTime value = DateTime.Now;
        /// <summary>minDate 字段。</summary>
        private DateTime minDate = new DateTime(1753, 1, 1);
        /// <summary>maxDate 字段。</summary>
        private DateTime maxDate = DateTime.MaxValue;
        /// <summary>customFormat 字段。</summary>
        private string customFormat = "";
        /// <summary>BTN_SIZE 字段。</summary>
        private int BTN_SIZE = 26;
        /// <summary>buttonImage 字段。</summary>
        private Bitmap buttonImage = HPhoto.Get("Date");

        #endregion 私有变量

        #region 公共变量
        [HCategoryLanguage("样式"), HDisplayNameLanguage("日期图标"), HDescriptionLanguage("日期图标"), Browsable(true)]
        public Bitmap ButtonImage
        {
            get { return buttonImage; }
            set
            {
                if (value != null)
                    buttonImage = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("样式"), HDisplayNameLanguage("日期图标大小"), HDescriptionLanguage("日期图标大小"), Browsable(true)]
        public int ButtonSize
        {
            get { return BTN_SIZE; }
            set
            {
                if (value > 0 && value <= this.Height)
                {
                    BTN_SIZE = value;
                    this.Invalidate();
                }
            }
        }

        [HCategoryLanguage("数据"), HDisplayNameLanguage("当前选择日期"), HDescriptionLanguage("当前选择日期"), Browsable(true)]
        public DateTime Value
        {
            get { return this.value; }
            set
            {
                int v1 = value.CompareTo(minDate);
                int v2 = value.CompareTo(maxDate);
                if (v1 >= 0 && v2 <= 0)
                {
                    this.value = value;
                    this.Invalidate();
                }
            }
        }

        [HCategoryLanguage("数据"), HDisplayNameLanguage("可选择的最大日期"), HDescriptionLanguage("可选择的最大日期"), Browsable(true)]
        public DateTime MaxDate
        {
            get { return maxDate; }
            set
            {
                if (value.CompareTo(minDate) >= 0)
                {
                    maxDate = value;
                    if (this.Value.CompareTo(maxDate) > 0)
                    {
                        this.Value = maxDate;
                    }
                }
            }
        }

        [HCategoryLanguage("数据"), HDisplayNameLanguage("可选择的最小日期"), HDescriptionLanguage("可选择的最小日期"), Browsable(true)]
        public DateTime MinDate
        {
            get { return minDate; }
            set
            {
                if (value.CompareTo(maxDate) <= 0)
                {
                    minDate = value;
                    if (this.Value.CompareTo(minDate) < 0)
                    {
                        this.Value = minDate;
                    }
                }
            }
        }

        /// <summary>PickTime 成员。</summary>
        /// <summary>PickTime 字段。</summary>
        [HCategoryLanguage("数据"), HDisplayNameLanguage("选择时间"), HDescriptionLanguage("选择时间"), Browsable(true)]
        public bool PickTime { get; set; } = false;

        [HCategoryLanguage("数据"), HDisplayNameLanguage("自定义格式"), HDescriptionLanguage("自定义格式"), Browsable(true)]
        public string CustomFormat
        {
            get { return customFormat; }
            set
            {
                try
                {
                    string f = this.Value.ToString(value);
                    customFormat = value;
                    this.Invalidate();
                }
                catch
                {
                }
            }
        }

        /// <summary>DropformRadius 成员。</summary>
        /// <summary>DropformRadius 字段。</summary>
        [HCategoryLanguage("弹出窗体"), HDisplayNameLanguage("弹出窗体圆角"), HDescriptionLanguage("弹出窗体圆角"), Browsable(true)]
        public int DropformRadius { get; set; } = 5;

        /// <summary>DropbaseColor 成员。</summary>
        /// <summary>DropbaseColor 字段。</summary>
        [HCategoryLanguage("弹出窗体"), HDisplayNameLanguage("弹出窗体背景色"), HDescriptionLanguage("弹出窗体背景色"), Browsable(true)]
        public Color DropbaseColor { get; set; } = Color.White;

        /// <summary>DropborderColor 成员。</summary>
        /// <summary>DropborderColor 字段。</summary>
        [HCategoryLanguage("弹出窗体"), HDisplayNameLanguage("弹出窗体边框色"), HDescriptionLanguage("弹出窗体边框色"), Browsable(true)]
        public Color DropborderColor { get; set; } = Color.LightGray;


        #endregion 公共变量

        public HDateTimePicker()
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
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            //dropForm.Opacity = 0;
            //dropForm.Show();
            //dropForm.Hide();
            //dropForm.Opacity = 1;
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            //g.Clear(this.BackColor);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            string format = "yyyy-MM-dd";
            if (customFormat != "")
                format = customFormat;
            string text = this.Value.ToString(format);

            Brush Textbrush = new SolidBrush(this.ForeColor);
            Rectangle textrect = new Rectangle(this.Radius / 2, (int)this.BorderWidth, this.Width - Radius - BTN_SIZE, (int)(this.Height - 2 * this.BorderWidth));
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
            g.DrawString(text, this.Font, Textbrush, textrect, stringFormat);

            Rectangle imgrect = new Rectangle(textrect.Right, (this.Height - BTN_SIZE) / 2, BTN_SIZE, BTN_SIZE);
            g.DrawImage(buttonImage, imgrect);

            Textbrush.Dispose();
            stringFormat.Dispose();
        }

        /// <summary>响应 Click 事件。</summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            HDateTimeDropForm dropForm = new HDateTimeDropForm(this);

            dropForm.Show();
        }

    }
}