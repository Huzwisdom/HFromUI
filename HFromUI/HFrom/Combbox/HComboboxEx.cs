using HFromUI.HAttribute;

using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HColor;
    [DefaultEvent("SelectedIndexChanged")]
    public partial class HComboboxEx : UserControl
    {
        

        #region 私有属性

        /// <summary>align 字段。</summary>
        private Align align = Align.LEFT;
        /// <summary>dropAlign 字段。</summary>
        private Align dropAlign = Align.LEFT;
        /// <summary>radius 字段。</summary>
        private int radius = 5;
        /// <summary>borderWidth 字段。</summary>
        private float borderWidth = 1;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.White;
        /// <summary>borderColor 字段。</summary>
        private Color borderColor = Color.LightGray;
        /// <summary>arrowIco 字段。</summary>
        private Image arrowIco = HPhoto.Get("down");
        /// <summary>arrowIcoSize 字段。</summary>
        private Size arrowIcoSize = new Size(13, 13);
        /// <summary>roundStyle 字段。</summary>
        private HEnum.HRoundStyle roundStyle = HEnum.HRoundStyle.All;

        /// <summary>maxDropDownItems 字段。</summary>
        private int maxDropDownItems = 8;
        /// <summary>dropbaseColor 字段。</summary>
        private Color dropbaseColor = Color.White;
        /// <summary>dropborderColor 字段。</summary>
        private Color dropborderColor = Color.LightGray;
        /// <summary>dropbuttonColor 字段。</summary>
        private Color dropbuttonColor = Color.White;
        /// <summary>dropbuttonMouseInColor 字段。</summary>
        private Color dropbuttonMouseInColor = Color.LightGray;
        /// <summary>dropbuttonMouseDownColor 字段。</summary>
        private Color dropbuttonMouseDownColor = Color.DimGray;
        /// <summary>dropbuttonSelectColor 字段。</summary>
        private Color dropbuttonSelectColor = Color.DimGray;
        /// <summary>dropbuttonRadius 字段。</summary>
        private int dropbuttonRadius = 5;
        /// <summary>dropformRadius 字段。</summary>
        private int dropformRadius = 5;

        #endregion 私有属性

        #region 公共属性

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("项集合"), Browsable(true), HDescriptionLanguage("项集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public ComboBox.ObjectCollection Items
        {
            get
            {
                return comboBox1.Items;
            }
            set
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add(value);
                if (value.Count == 0)
                {
                    comboBox1.SelectedIndex = -1;
                    comboBox1.SelectedItem = null;
                    comboBox1.SelectedText = "";
                    comboBox1.SelectedValue = null;
                }

                this.Invalidate();
            }
        }

        [Browsable(false)]
        public int SelectedIndex
        {
            get { return comboBox1.SelectedIndex; }
            set
            {
                comboBox1.SelectedIndex = value;
                this.Invalidate();
            }
        }

        [Browsable(false)]
        public string SelectedText
        {
            get { return comboBox1.SelectedText; }
            set
            {
                comboBox1.SelectedText = value;
                this.Invalidate();
            }
        }

        [Browsable(false)]
        public object SelectedValue
        {
            get { return comboBox1.SelectedValue; }
            set
            {
                comboBox1.SelectedValue = value;
                this.Invalidate();
            }
        }

        [Browsable(false)]
        public object SelectedItem
        {
            get { return comboBox1.SelectedItem; }
            set
            {
                comboBox1.SelectedItem = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("文字显示位置"), HDescriptionLanguage("文字显示位置"), Browsable(true)]
        public Align TextAlign
        {
            get { return align; }
            set { align = value; this.Invalidate(); }
        }

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set
            {
                if (radius < 0)
                    return;
                int copy = radius;
                radius = value;
                this.Invalidate();
                this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));
                if (copy != radius)
                {
                    if (RadiusChanged != null)
                    {
                        RadiusChanged(this, new EventArgs());
                    }
                }
            }
        }

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("圆角类型"), HDescriptionLanguage("圆角类型"), Browsable(true)]
        public HEnum.HRoundStyle RoundStyle
        {
            get { return roundStyle; }
            set
            {
                roundStyle = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度"), Browsable(true)]
        public float BorderWidth
        {
            get { return borderWidth; }
            set
            {
                if (value <= 0)
                    return;
                borderWidth = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景色"), Browsable(true)]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                this.Invalidate();
                if (BaseColorChanged != null)
                {
                    BaseColorChanged(this, new EventArgs());
                }
            }
        }

        [HCategoryLanguage("下拉框"), HDisplayNameLanguage("边框色"), HDescriptionLanguage("边框色"), Browsable(true)]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                this.Invalidate();
                if (LineColorChanged != null)
                {
                    LineColorChanged(this, new EventArgs());
                }
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉最大项数"), HDescriptionLanguage("下拉框显示选项的最大个数"), Browsable(true)]
        public int MaxDropDownItems
        {
            get { return maxDropDownItems; }
            set { if (value > 0) maxDropDownItems = value; }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框文字显示位置"), HDescriptionLanguage("下拉框文字显示位置"), Browsable(true)]
        public Align DropTextAlign
        {
            get { return dropAlign; }
            set { dropAlign = value; }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框背景色"), HDescriptionLanguage("下拉框背景色"), Browsable(true)]
        public Color DropbaseColor
        {
            get { return dropbaseColor; }
            set
            {
                dropbaseColor = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框边框色"), HDescriptionLanguage("下拉框边框色"), Browsable(true)]
        public Color DropborderColor
        {
            get { return dropborderColor; }
            set
            {
                dropborderColor = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框按钮正常色"), HDescriptionLanguage("下拉框按钮正常色"), Browsable(true)]
        public Color DropbuttonColor
        {
            get { return dropbuttonColor; }
            set
            {
                dropbuttonColor = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框按钮鼠标移入色"), HDescriptionLanguage("下拉框按钮鼠标移入色"), Browsable(true)]
        public Color DropbuttonMouseInColor
        {
            get { return dropbuttonMouseInColor; }
            set
            {
                dropbuttonMouseInColor = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框按钮鼠标按下色"), HDescriptionLanguage("下拉框按钮鼠标按下色"), Browsable(true)]
        public Color DropbuttonMouseDownColor
        {
            get { return dropbuttonMouseDownColor; }
            set
            {
                dropbuttonMouseDownColor = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框按钮鼠标选中色"), HDescriptionLanguage("下拉框按钮鼠标选中色"), Browsable(true)]
        public Color DropbuttonSelectColor
        {
            get { return dropbuttonSelectColor; }
            set
            {
                dropbuttonSelectColor = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框按钮圆角"), HDescriptionLanguage("下拉框按钮圆角"), Browsable(true)]
        public int DropbuttonRadius
        {
            get { return dropbuttonRadius; }
            set
            {
                dropbuttonRadius = value;
            }
        }

        [HCategoryLanguage("下拉窗体"), HDisplayNameLanguage("下拉框圆角"), HDescriptionLanguage("下拉框圆角"), Browsable(true)]
        public int DropformRadius
        {
            get { return dropformRadius; }
            set
            {
                dropformRadius = value;
            }
        }

        public enum Align
        {
            LEFT,
            CENTER
        }

        /// <summary>TextDrawMode 成员。</summary>
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Anti;

        #endregion 公共属性

        #region 事件

        /// <summary>
        /// 尺寸变化事件
        /// </summary>
        [HDescriptionLanguage("尺寸变化事件")]
        public event EventHandler SizeChangeded;

        /// <summary>
        /// 圆角直径变化事件
        /// </summary>
        [HDescriptionLanguage("圆角直径变化事件")]
        public event EventHandler RadiusChanged;

        /// <summary>
        /// 内部颜色变化事件
        /// </summary>
        [HDescriptionLanguage("内部颜色变化事件")]
        public event EventHandler BaseColorChanged;

        /// <summary>
        /// 边框颜色变化事件
        /// </summary>
        [HDescriptionLanguage("边框颜色变化事件")]
        public event EventHandler LineColorChanged;

        /// <summary>
        /// 选项变化事件
        /// </summary>
        [HDescriptionLanguage("选项变化事件")]
        public event EventHandler SelectedIndexChanged;

        /// <summary>
        /// 当从下拉列表选择项，而下拉列表关闭时发生
        /// </summary>
        [HDescriptionLanguage("当从下拉列表选择项，而下拉列表关闭时发生")]
        public event EventHandler SelectionChangeCommitted;

        #endregion 事件

        public HComboboxEx()
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
            this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius,roundStyle));
            
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
            
        }

        /// <summary>响应 Click 事件。</summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            HComboboxDropForm dropform = new HComboboxDropForm(this);
            dropform.Show();
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            //if (dropform.Visible)
            //{
            //    dropform.Hide();
            //}
            //else
            //{
            //    dropform.Show();
            //}
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int width = this.Width;
            int height = this.Height;
            //int halflinewidth = borderWidth % 2 == 0 ? borderWidth / 2 : borderWidth / 2 + 1;
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            HDrawPaint.SetGraphicsHighQuality(g);
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Pen pen = new Pen(borderColor, borderWidth);
            Pen penBack = new Pen(this.BackColor);
            Brush brush = new SolidBrush(baseColor);
            Brush brushFore = new SolidBrush(this.ForeColor);

            GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius, roundStyle);
            GraphicsPath path2 = HDrawPaint.CreatePath(new RectangleF(borderWidth, borderWidth, this.Width - 2f * borderWidth, this.Height - 2f * borderWidth), radius,roundStyle);
            g.FillPath(brush, path2);
            g.DrawPath(pen, path2);
            g.DrawPath(penBack, path);

            string text = this.SelectedIndex >= 0 && this.Items.Count > 0 ? (string)this.SelectedItem : "";
            Brush Textbrush = new SolidBrush(this.ForeColor);
            Rectangle textrect = new Rectangle(this.radius / 2, (int)this.borderWidth, this.Width - radius - arrowIcoSize.Width, (int)(this.Height - 2 * this.borderWidth));
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = TextAlign == Align.CENTER ? StringAlignment.Center : StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
            g.DrawString(text, this.Font, Textbrush, textrect, stringFormat);
            Rectangle imgrect = new Rectangle(textrect.Right, (this.Height - arrowIcoSize.Height) / 2, arrowIcoSize.Width, arrowIcoSize.Height);
            g.DrawImage(arrowIco, imgrect);

            pen.Dispose();
            brush.Dispose();
            path.Dispose();
            path2.Dispose();
            penBack.Dispose();
            Textbrush.Dispose();
            stringFormat.Dispose();
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.Width < radius || this.Height < radius || this.Width - arrowIcoSize.Width - 2 * borderWidth - radius <= 1)
            {
                if (this.Width - arrowIcoSize.Width - 2 * borderWidth - radius <= 1)
                {
                    this.Width = arrowIcoSize.Width + (int)(2 * borderWidth) + radius + 1;
                }
                else if (this.Width < radius)
                {
                    this.Width = radius;
                }
                else if (this.Height < radius)
                {
                    this.Height = radius;
                }
                return;
            }
            base.OnSizeChanged(e);
            this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));
            if (SizeChangeded != null)
            {
                SizeChangeded(this, e);
            }
        }

        /// <summary>SelectionChangeCommitted_Event 方法。</summary>
        public void SelectionChangeCommitted_Event()
        {
            if (SelectionChangeCommitted != null)
            {
                SelectionChangeCommitted(this, new EventArgs());
            }
        }

        /// <summary>comboBox1_SelectedIndexChanged 方法。</summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate ()
            {
                this.Refresh();
                if (SelectedIndexChanged != null)
                {
                    SelectedIndexChanged(this, new EventArgs());
                }
            }));
        }

        /// <summary>
        /// 返回以指定字符串开头的 HFromUI.ComboBoxEx 中的第一项的索引
        /// </summary>
        /// <param name="s">要搜索的 System.String。</param>
        /// <returns>找到的第一个项的从零开始的索引；如果未找到匹配项，则返回 -1</returns>
        public int FindString(string s)
        {
            return comboBox1.FindString(s);
        }

        /// <summary>
        /// 返回 HFromUI.ComboBoxEx 中包含该指定字符串的指定索引之外的第一项的索引。该搜索不区分大小写。
        /// <para>异常:System.ArgumentOutOfRangeException:startIndex 小于 -1。- 或 -startIndex 大于集合中的最后一个索引。</para>
        /// </summary>
        /// <param name="s">要搜索的 System.String</param>
        /// <param name="startIndex">项的从零开始的索引，该项在要搜索的第一个项之前。设置为 -1，以从控件的开始处搜索。</param>
        /// <returns>找到的第一个项的从零开始的索引；如果未找到匹配项，则返回 -1，或者，如果 s 参数指定 System.String.Empty，则返回 0。</returns>
        public int FindString(string s, int startIndex)
        {
            return comboBox1.FindString(s, startIndex);
        }

        /// <summary>
        /// 查找组合框中与指定字符串匹配的第一项。
        /// </summary>
        /// <param name="s">要搜索的 System.String。</param>
        /// <returns>找到的第一个项的从零开始的索引；如果未找到匹配项，则返回 -1，或者，如果 s 参数指定 System.String.Empty，则返回 0。</returns>
        public int FindStringExact(string s)
        {
            return comboBox1.FindStringExact(s);
        }
    }
}