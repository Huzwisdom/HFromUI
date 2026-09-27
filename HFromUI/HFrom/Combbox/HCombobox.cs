using HFromUI.HAttribute;
using HFromUI.HFrom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Combbox
{
    [DefaultEvent("SelectedIndexChanged")]
    public partial class HCombobox : HControlBase
    {
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("项集合"), Browsable(true), HDescriptionLanguage("项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public ComboBox.ObjectCollection Items
        {
            get
            {
                return hNoBoraderCombobox1.Items;
            }
            set
            {
                hNoBoraderCombobox1.Items.Clear();
                hNoBoraderCombobox1.Items.Add(value);
            }
        }

        [Browsable(false)]
        public int SelectedIndex
        {
            get { return hNoBoraderCombobox1.SelectedIndex; }
            set
            {
                hNoBoraderCombobox1.SelectedIndex = value;
            }
        }

        [Browsable(false)]
        public string SelectedText
        {
            get { return hNoBoraderCombobox1.SelectedText; }
            set
            {
                hNoBoraderCombobox1.SelectedText = value;
            }
        }

        [Browsable(false)]
        public object SelectedValue
        {
            get { return hNoBoraderCombobox1.SelectedValue; }
            set
            {
                hNoBoraderCombobox1.SelectedValue = value;
            }
        }

        [Browsable(false)]
        public object SelectedItem
        {
            get { return hNoBoraderCombobox1.SelectedItem; }
            set
            {
                hNoBoraderCombobox1.SelectedItem = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("箭头图片"), HDescriptionLanguage("箭头图片"), Browsable(true)]
        public Image ArrowImage
        {
            get { return hNoBoraderCombobox1.ArrowImage; }
            set
            {
                hNoBoraderCombobox1.ArrowImage = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图片大小"), HDescriptionLanguage("图片大小"), Browsable(true)]
        public Size ImageSize
        {
            get { return hNoBoraderCombobox1.ImageSize; }
            set
            {
                hNoBoraderCombobox1.ImageSize = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图片位置"), HDescriptionLanguage("图片位置"), Browsable(true)]
        public Point ImageOffset
        {
            get { return hNoBoraderCombobox1.ImageOffset; }
            set
            {
                hNoBoraderCombobox1.ImageOffset = value;
                this.Invalidate();
            }
        }

        public HCombobox()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.DoubleBuffer, true);
            //this.SetStyle(ControlStyles.ResizeRedraw, true);
            //this.SetStyle(ControlStyles.Selectable, true);
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            //this.UpdateStyles();
            this.SizeChangeded += new EventHandler(This_SizeChangeded);
            this.RadiusChanged += new EventHandler(This_RadiusChanged);
            this.BaseColorChanged += new EventHandler(This_BaseColorChanged);
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

        /// <summary>This_SizeChangeded 方法。</summary>
        private void This_SizeChangeded(object sender, EventArgs e)
        {
            this.Height = hNoBoraderCombobox1.Height + (int)(2 * BorderWidth) + 2;
            hNoBoraderCombobox1.Width = this.Width - Radius - 3;
            hNoBoraderCombobox1.Location = new Point(Radius / 2 + 2, (int)BorderWidth + 1);
        }

        /// <summary>This_RadiusChanged 方法。</summary>
        private void This_RadiusChanged(object sender, EventArgs e)
        {
            this.Height = hNoBoraderCombobox1.Height + (int)(2 * BorderWidth) + 2;
            hNoBoraderCombobox1.Width = this.Width - Radius - 3;
            hNoBoraderCombobox1.Location = new Point(Radius / 2 + 2, (int)BorderWidth + 1);
        }

        /// <summary>This_BaseColorChanged 方法。</summary>
        private void This_BaseColorChanged(object sender, EventArgs e)
        {
            hNoBoraderCombobox1.BackColor = BaseColor;
        }

        /// <summary>响应 BackColorChanged 事件。</summary>
        protected override void OnBackColorChanged(EventArgs e)
        {
            this.Invalidate();
            base.OnBackColorChanged(e);
            hNoBoraderCombobox1.BackColor = BaseColor;
        }

        /// <summary>响应 FontChanged 事件。</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            this.hNoBoraderCombobox1.Font = base.Font;
            this.Invalidate();
            base.OnFontChanged(e);
        }

        /// <summary>响应 ForeColorChanged 事件。</summary>
        protected override void OnForeColorChanged(EventArgs e)
        {
            hNoBoraderCombobox1.ForeColor = base.ForeColor;
            this.Invalidate();
            base.OnForeColorChanged(e);
        }

        /// <summary>hNoBoraderCombobox1_SizeChanged 方法。</summary>
        private void hNoBoraderCombobox1_SizeChanged(object sender, EventArgs e)
        {
            this.Height = hNoBoraderCombobox1.Height + (int)(2 * BorderWidth) + 2;
            hNoBoraderCombobox1.Width = this.Width - Radius - 3;
            hNoBoraderCombobox1.Location = new Point(Radius / 2 + 2, (int)BorderWidth + 1);
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("下拉框样式"), HDescriptionLanguage("下拉框样式（可编辑/仅选择）"), Browsable(true)]
        public ComboBoxStyle DropDownStyle
        {
            get { return hNoBoraderCombobox1.DropDownStyle; }
            set { hNoBoraderCombobox1.DropDownStyle = value; }
        }

        /// <summary>下拉列表宽度（像素），可大于控件本身宽度，避免长选项被截断。</summary>
        public int DropDownWidth
        {
            get { return hNoBoraderCombobox1.DropDownWidth; }
            set { hNoBoraderCombobox1.DropDownWidth = value; }
        }

        public event EventHandler SelectedIndexChanged;

        /// <summary>hNoBoraderCombobox1_SelectedIndexChanged 方法。</summary>
        private void hNoBoraderCombobox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
            {
                SelectedIndexChanged(this, new EventArgs());
            }
        }

        public event EventHandler SelectedValueChanged;

        /// <summary>hNoBoraderCombobox1_SelectedValueChanged 方法。</summary>
        private void hNoBoraderCombobox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (SelectedValueChanged != null)
            {
                SelectedValueChanged(this, new EventArgs());
            }
        }

        public event EventHandler SelectionChangeCommitted;

        /// <summary>hNoBoraderCombobox1_SelectionChangeCommitted 方法。</summary>
        private void hNoBoraderCombobox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (SelectionChangeCommitted != null)
            {
                SelectionChangeCommitted(this, new EventArgs());
            }
        }
    }
}