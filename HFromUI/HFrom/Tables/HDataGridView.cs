using HFromUI.HAttribute;

using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    using HFromUI.HLangage;
    using HFromUI.HColor;
    public partial class HDataGridView : DataGridView
    {
        /// <summary>
        /// DataGridViewButtonColumnEx按钮点击事件委托
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DataGridViewButtonClicked(object sender, DataGridViewButtonClickEventArgs e);

        #region 属性定义

        /// <summary>columnBackColorBegin 字段。</summary>
        private Color columnBackColorBegin = Color.White;
        /// <summary>columnBackColorEnd 字段。</summary>
        private Color columnBackColorEnd = Color.LightGray;
        /// <summary>columnForeColor 字段。</summary>
        private Color columnForeColor = Color.Black;

        /// <summary>cellDefaultColor 字段。</summary>
        private Color cellDefaultColor = Color.White;
        /// <summary>cellSelectBackColorBegin 字段。</summary>
        private Color cellSelectBackColorBegin = Color.White;
        /// <summary>cellSelectBackColorEnd 字段。</summary>
        private Color cellSelectBackColorEnd = Color.Silver;
        /// <summary>cellForeColor 字段。</summary>
        private Color cellForeColor = Color.Black;

        /// <summary>textalign 字段。</summary>
        private textAlign textalign = textAlign.Center;
        /// <summary>textaligncolumn 字段。</summary>
        private textAlign textaligncolumn = textAlign.Center;
        /// <summary>textalignrowhead 字段。</summary>
        private textAlign textalignrowhead = textAlign.Center;
        /// <summary>columnFont 字段。</summary>
        private Font columnFont = new Font("微软雅黑", 11f);
        /// <summary>cellFont 字段。</summary>
        private Font cellFont = new Font("微软雅黑", 10f);
        /// <summary>showCellBorder 字段。</summary>
        private bool showCellBorder = true;
        /// <summary>autoWidth 字段。</summary>
        private bool autoWidth = true;
        /// <summary>firstColumnText 字段。</summary>
        private string firstColumnText = "Num.";

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("列表头颜色1"), HDescriptionLanguage("列表头颜色1"), Browsable(true)]
        public Color ColumnBackColorBegin
        {
            get { return columnBackColorBegin; }
            set
            {
                columnBackColorBegin = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("列表头颜色2"), HDescriptionLanguage("列表头颜色2"), Browsable(true)]
        public Color ColumnBackColorEnd
        {
            get { return columnBackColorEnd; }
            set
            {
                columnBackColorEnd = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("列表头字体颜色"), HDescriptionLanguage("列表头字体颜色"), Browsable(true)]
        public Color ColumnForeColor
        {
            get { return columnForeColor; }
            set
            {
                columnForeColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("背景颜色"), HDescriptionLanguage("背景颜色"), Browsable(true)]
        public Color CellDefaultColor
        {
            get { return cellDefaultColor; }
            set
            {
                cellDefaultColor = value;
                this.Invalidate();
            }
        }

        /// <summary>CellEvenColor 成员。</summary>
        /// <summary>CellEvenColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("偶数行背景颜色"), HDescriptionLanguage("偶数行背景颜色"), Browsable(true)]
        public Color CellEvenColor { get; set; } = Color.WhiteSmoke;

        /// <summary>EnableEvenBackColor 成员。</summary>
        /// <summary>EnableEvenBackColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("偶数行背景色"), HDescriptionLanguage("是否启用偶数行背景颜色"), Browsable(true)]
        public bool EnableEvenBackColor { get; set; } = false;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选中颜色1"), HDescriptionLanguage("选中颜色1"), Browsable(true)]
        public Color CellSelectBackColorBegin
        {
            get { return cellSelectBackColorBegin; }
            set
            {
                cellSelectBackColorBegin = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选中颜色2"), HDescriptionLanguage("选中颜色2"), Browsable(true)]
        public Color CellSelectBackColorEnd
        {
            get { return cellSelectBackColorEnd; }
            set
            {
                cellSelectBackColorEnd = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("字体颜色"), HDescriptionLanguage("字体颜色"), Browsable(true)]
        public Color CellForeColor
        {
            get { return cellForeColor; }
            set
            {
                cellForeColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文字对齐方向"), HDescriptionLanguage("文字对齐方向"), Browsable(true)]
        public textAlign TextAlign
        {
            get { return textalign; }
            set
            {
                textalign = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题栏文字对齐方向"), HDescriptionLanguage("标题栏文字对齐方向"), Browsable(true)]
        public textAlign TextAlignColumn
        {
            get { return textaligncolumn; }
            set
            {
                textaligncolumn = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("行头文字对齐方向"), HDescriptionLanguage("行头文字对齐方向"), Browsable(true)]
        public textAlign TextAlignRowHead
        {
            get { return textalignrowhead; }
            set
            {
                textalignrowhead = value;
                this.Invalidate();
            }
        }

        public enum textAlign
        {
            Left,
            Center,
            Right
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题栏文字字体"), HDescriptionLanguage("标题栏文字字体"), Browsable(true)]
        public Font ColumnFont
        {
            get { return columnFont; }
            set
            {
                columnFont = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("内容文字字体"), HDescriptionLanguage("内容文字字体"), Browsable(true)]
        public Font CellFont
        {
            get { return cellFont; }
            set
            {
                cellFont = value;
                this.Font = cellFont;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("是否显示内容边框线"), HDescriptionLanguage("是否显示内容边框线"), Browsable(true)]
        public bool ShowCellBorder
        {
            get { return showCellBorder; }
            set
            {
                showCellBorder = value;
                this.Invalidate();
            }
        }

        /// <summary>ShowBorder 成员。</summary>
        /// <summary>ShowBorder 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("是否显示边框线"), HDescriptionLanguage("是否显示边框线"), Browsable(true)]
        public bool ShowBorder { get; set; } = true;

        /// <summary>BorderRadius 成员。</summary>
        /// <summary>BorderRadius 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框线圆角"), HDescriptionLanguage("边框线圆角"), Browsable(true)]
        public int BorderRadius { get; set; } = 10;

        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框线颜色"), HDescriptionLanguage("边框线颜色"), Browsable(true)]
        public Color BorderColor { get; set; } = Color.LightGray;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("自动宽度"), HDescriptionLanguage("是否自动根据文本长度调整Cell宽度"), Browsable(true)]
        public bool AutoWidth
        {
            get { return autoWidth; }
            set
            {
                autoWidth = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("第一列文本"), HDescriptionLanguage("序号栏标题框显示的文字"), Browsable(true)]
        public string FirstColumnText
        {
            get { return firstColumnText; }
            set
            {
                firstColumnText = value;
                this.Invalidate();
            }
        }

        /// <summary>ShowColumnCheckBox 成员。</summary>
        /// <summary>ShowColumnCheckBox 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示列勾选框"), HDescriptionLanguage("是否在column显示全选的checkbox"), Browsable(true)]
        public bool ShowColumnCheckBox { get; set; } = false;

        /// <summary>TextDrawMode 成员。</summary>
        /// <summary>TextDrawMode 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文字绘制模式"), HDescriptionLanguage("文字绘制模式"), Browsable(true)]
        public DrawMode TextDrawMode { get; set; } = DrawMode.Anti;

        #endregion 属性定义

        public enum DrawMode
        {
            Anti,
            Clear
        }

        public enum Style
        {
            Default,
            Style1
        }

        public HDataGridView()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor, true);
            base.UpdateStyles();
            //this.RowTemplate.Resizable = DataGridViewTriState.True;
            //this.RowTemplate.Height = 40;
            //this.ColumnHeadersHeight = 30;
            //this.RowHeadersWidth = 50;
        }

        /// <summary>
        /// 重写CreateParams方法 解决控件过多加载闪烁问题
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

        /// <summary>响应 CellPainting 事件。</summary>
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            // base.OnCellPainting(e);

            e.Graphics.TextRenderingHint = TextDrawMode == DrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            try
            {
                if (e.ColumnIndex == -1 && e.RowIndex == -1)
                {
                    Rectangle brect1 = e.CellBounds;// new Rectangle(0, 0, 1000, 400);
                    Rectangle brect2 = new Rectangle(e.CellBounds.X, e.CellBounds.Y + e.CellBounds.Height / 2, e.CellBounds.Width, e.CellBounds.Height - e.CellBounds.Height / 2);//new Rectangle(0, 200, 1000, 200);

                    LinearGradientBrush brush1 = new LinearGradientBrush(brect1, ColumnBackColorBegin, ColumnBackColorEnd, LinearGradientMode.Vertical);
                    LinearGradientBrush brush2 = new LinearGradientBrush(brect2, ColumnBackColorEnd, ColumnBackColorBegin, LinearGradientMode.Vertical);

                    e.Graphics.FillRectangle(brush1, brect1);
                    //e.Graphics.FillRectangle(brush2, brect2);

                    Size fsize = TextRenderer.MeasureText(firstColumnText, columnFont);
                    float addx = 0;
                    if (textaligncolumn == textAlign.Left)
                    {
                        addx = 0;
                    }
                    else if (textaligncolumn == textAlign.Center)
                    {
                        addx = (e.CellBounds.Width - fsize.Width) / 2;
                    }
                    else
                    {
                        addx = e.CellBounds.Width - fsize.Width;
                    }

                    Brush brushFore = new SolidBrush(columnForeColor);
                    e.Graphics.DrawString(firstColumnText, columnFont, brushFore, e.CellBounds.X + addx, e.CellBounds.Y + (e.CellBounds.Height - fsize.Height) / 2);

                    if (showCellBorder)
                    {
                        Rectangle border = e.CellBounds;
                        border.Offset(new Point(-1, -1));
                        Pen pen = new Pen(GridColor, 1f);
                        e.Graphics.DrawRectangle(pen, border);
                        pen.Dispose();
                    }
                    brush1.Dispose();
                    brush2.Dispose();
                    brushFore.Dispose();

                    e.Handled = true;
                }
                else if (e.RowIndex == -1)
                {
                    //标题行

                    Rectangle brect1 = e.CellBounds;// new Rectangle(0, 0, 1000, 400);
                    Rectangle brect2 = new Rectangle(e.CellBounds.X, e.CellBounds.Y + e.CellBounds.Height / 2, e.CellBounds.Width, e.CellBounds.Height - e.CellBounds.Height / 2);//new Rectangle(0, 200, 1000, 200);

                    LinearGradientBrush brush1 = new LinearGradientBrush(brect1, ColumnBackColorBegin, ColumnBackColorEnd, LinearGradientMode.Vertical);
                    LinearGradientBrush brush2 = new LinearGradientBrush(brect2, ColumnBackColorEnd, ColumnBackColorBegin, LinearGradientMode.Vertical);

                    e.Graphics.FillRectangle(brush1, brect1);

                    if (this.Columns[e.ColumnIndex].CellType.Name == "DataGridViewCheckBoxCell" && ShowColumnCheckBox)
                    {
                        if (this.Columns[e.ColumnIndex].Tag != null && (bool)this.Columns[e.ColumnIndex].Tag == true)
                        {
                            Bitmap bitmap = HPhoto.Get("CheckSelect");
                            e.Graphics.DrawImage(bitmap, e.CellBounds.X + (e.CellBounds.Width - 15) / 2, e.CellBounds.Y + (e.CellBounds.Height - 15) / 2, 15, 15);
                        }
                        else
                        {
                            Bitmap bitmap = HPhoto.Get("CheckNormal");
                            e.Graphics.DrawImage(bitmap, e.CellBounds.X + (e.CellBounds.Width - 15) / 2, e.CellBounds.Y + (e.CellBounds.Height - 15) / 2, 15, 15);
                        }
                    }
                    else
                    {
                        string t = (string)e.Value;
                        Size fsize = TextRenderer.MeasureText(t, columnFont);

                        float addx = 0;
                        if (textaligncolumn == textAlign.Left)
                        {
                            addx = 0;
                        }
                        else if (textaligncolumn == textAlign.Center)
                        {
                            addx = (e.CellBounds.Width - fsize.Width) / 2;
                        }
                        else
                        {
                            addx = e.CellBounds.Width - fsize.Width;
                        }

                        Brush brushFore = new SolidBrush(columnForeColor);
                        e.Graphics.DrawString((string)e.Value, columnFont, brushFore, e.CellBounds.X + addx, e.CellBounds.Y + (e.CellBounds.Height - fsize.Height) / 2);
                        brushFore.Dispose();
                    }

                    if (showCellBorder)
                    {
                        Rectangle border = e.CellBounds;
                        // border.Width += 1;
                        //border.Height += 1;
                        border.Offset(new Point(-1, -1));
                        Pen pen = new Pen(GridColor, 1);
                        e.Graphics.DrawRectangle(pen, border);
                        pen.Dispose();
                    }
                    brush1.Dispose();
                    brush2.Dispose();

                    e.Handled = true;
                }
                else if (e.ColumnIndex == -1)
                {
                    //标题列

                    if ((e.State & DataGridViewElementStates.Selected) != 0)//被选中
                    {
                        Rectangle brect1 = e.CellBounds;// new Rectangle(0, 0, 1000, 400);
                        Rectangle brect2 = new Rectangle(e.CellBounds.X, e.CellBounds.Y + e.CellBounds.Height / 2, e.CellBounds.Width, e.CellBounds.Height - e.CellBounds.Height / 2);//new Rectangle(0, 200, 1000, 200);
                        brect1.Height -= 1;

                        LinearGradientBrush brush1 = new LinearGradientBrush(brect1, cellSelectBackColorBegin, cellSelectBackColorEnd, LinearGradientMode.Vertical);
                        LinearGradientBrush brush2 = new LinearGradientBrush(brect2, cellSelectBackColorEnd, cellSelectBackColorBegin, LinearGradientMode.Vertical);

                        e.Graphics.FillRectangle(brush1, e.CellBounds);
                        //e.Graphics.FillRectangle(brush2, brect2);

                        string t = (e.RowIndex + 1).ToString();
                        Size fsize = TextRenderer.MeasureText(t, cellFont);

                        float addx = 0;
                        if (textalignrowhead == textAlign.Left)
                        {
                            addx = 0;
                        }
                        else if (textalignrowhead == textAlign.Center)
                        {
                            addx = (e.CellBounds.Width - fsize.Width) / 2;
                        }
                        else
                        {
                            addx = e.CellBounds.Width - fsize.Width;
                        }
                        Brush brushFore = new SolidBrush(columnForeColor);
                        e.Graphics.DrawString(t, cellFont, brushFore, e.CellBounds.X + addx, e.CellBounds.Y + (e.CellBounds.Height - fsize.Height) / 2);
                        brushFore.Dispose();

                        if (showCellBorder)
                        {
                            Rectangle border = e.CellBounds;
                            border.Offset(new Point(-1, -1));
                            Pen pen = new Pen(GridColor, 1);
                            e.Graphics.DrawRectangle(pen, border);
                            pen.Dispose();
                        }

                        brush1.Dispose();
                        brush2.Dispose();

                        e.Handled = true;
                    }
                    else
                    {
                        SolidBrush brush = new SolidBrush(cellDefaultColor);
                        Brush brushFore = new SolidBrush(cellForeColor);
                        if (EnableEvenBackColor && (e.RowIndex + 1) % 2 == 0)
                        {
                            brush = new SolidBrush(CellEvenColor);
                        }

                        e.Graphics.FillRectangle(brush, e.CellBounds);
                        string t = (e.RowIndex + 1).ToString();
                        Size fsize = TextRenderer.MeasureText(t, cellFont);

                        float addx = 0;
                        if (textalignrowhead == textAlign.Left)
                        {
                            addx = 0;
                        }
                        else if (textalignrowhead == textAlign.Center)
                        {
                            addx = (e.CellBounds.Width - fsize.Width) / 2;
                        }
                        else
                        {
                            addx = e.CellBounds.Width - fsize.Width;
                        }

                        e.Graphics.DrawString(t, cellFont, brushFore, e.CellBounds.X + addx, e.CellBounds.Y + (e.CellBounds.Height - fsize.Height) / 2);

                        if (showCellBorder)
                        {
                            Rectangle border = e.CellBounds;
                            //border.Width += 1;
                            //border.Height += 1;
                            border.Offset(new Point(-1, -1));
                            Pen pen = new Pen(GridColor, 1);
                            e.Graphics.DrawRectangle(pen, border);
                            pen.Dispose();
                        }

                        brush.Dispose();
                        brushFore.Dispose();

                        e.Handled = true;
                    }
                }
                else
                {
                    //if (this.Columns[e.ColumnIndex].GetType().Name == "DataGridTextBoxColumn")
                    //{
                    if ((e.State & DataGridViewElementStates.Selected) != 0)//被选中
                    {
                        RectangleF brect1 = e.CellBounds;// new Rectangle(0, 0, 1000, 400);
                        Rectangle brect2 = new Rectangle(e.CellBounds.X, e.CellBounds.Y + e.CellBounds.Height / 2, e.CellBounds.Width, e.CellBounds.Height - e.CellBounds.Height / 2);//new Rectangle(0, 200, 1000, 200);

                        brect1.Height -= 1;//防止选中时上边框出现一条线
                        LinearGradientBrush brush1 = new LinearGradientBrush(brect1, cellSelectBackColorBegin, cellSelectBackColorEnd, LinearGradientMode.Vertical);

                        e.Graphics.FillRectangle(brush1, e.CellBounds);
                        brush1.Dispose();
                    }
                    else
                    {
                        SolidBrush brush = new SolidBrush(cellDefaultColor);
                        if (EnableEvenBackColor && (e.RowIndex + 1) % 2 == 0)
                        {
                            brush = new SolidBrush(CellEvenColor);
                        }
                        e.Graphics.FillRectangle(brush, e.CellBounds);
                        brush.Dispose();
                    }
                    //e.Graphics.FillRectangle(brush2, brect2);

                    if (this.Columns[e.ColumnIndex].CellType.Name == "DataGridViewTextBoxCell")
                    {
                        string t = string.Empty;
                        if (e.Value != null && e.Value.GetType() != typeof(DBNull))
                        {
                            if (e.Value.GetType() == typeof(DateTime))
                            {
                                t = ((DateTime)e.Value).ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            else
                            {
                                t = (string)e.Value;
                            }
                        }

                        Size fsize = TextRenderer.MeasureText(t, cellFont);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.LineAlignment = StringAlignment.Center;
                        stringFormat.FormatFlags = StringFormatFlags.LineLimit;
                        stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                        float addx = 0;
                        if (textalign == textAlign.Left)
                        {
                            addx = 0;
                            stringFormat.Alignment = StringAlignment.Near;
                        }
                        else if (textalign == textAlign.Center)
                        {
                            addx = (e.CellBounds.Width - fsize.Width) / 2;
                            stringFormat.Alignment = StringAlignment.Center;
                        }
                        else
                        {
                            addx = e.CellBounds.Width - fsize.Width;
                            stringFormat.Alignment = StringAlignment.Far;
                        }

                        //e.Graphics.DrawString(t, cellFont, new SolidBrush(cellForeColor), e.CellBounds.X + addx, e.CellBounds.Y + (e.CellBounds.Height - fsize.Height) / 2);

                        Brush brushFore = new SolidBrush(cellForeColor);
                        e.Graphics.DrawString(t, CellFont, brushFore, e.CellBounds, stringFormat);
                        brushFore.Dispose();
                        if (autoWidth)
                        {
                            if (e.CellBounds.Width < fsize.Width)
                            {
                                this.Columns[e.ColumnIndex].Width = fsize.Width + 30;
                            }
                        }
                    }
                    else if (this.Columns[e.ColumnIndex].CellType.Name == "DataGridViewCheckBoxCell")
                    {
                        if (e.Value != null && (bool)e.Value == true)
                        {
                            Bitmap bitmap = HPhoto.Get("CheckSelect");
                            e.Graphics.DrawImage(bitmap, e.CellBounds.X + (e.CellBounds.Width - 15) / 2, e.CellBounds.Y + (e.CellBounds.Height - 15) / 2, 15, 15);
                        }
                        else
                        {
                            Bitmap bitmap = HPhoto.Get("CheckNormal");
                            e.Graphics.DrawImage(bitmap, e.CellBounds.X + (e.CellBounds.Width - 15) / 2, e.CellBounds.Y + (e.CellBounds.Height - 15) / 2, 15, 15);
                        }
                    }
                    else
                    {
                        e.PaintContent(e.CellBounds);
                    }

                    if (showCellBorder)
                    {
                        Rectangle border = e.CellBounds;

                        border.Offset(new Point(-1, -1));
                        Pen pen = new Pen(GridColor, 1);
                        e.Graphics.DrawRectangle(pen, border);
                        pen.Dispose();
                    }

                    e.Handled = true;
                }
            }
            catch
            {
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Pen pen = new Pen(BorderColor);
            Brush brush = new SolidBrush(this.BackgroundColor);

            GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), BorderRadius);
            GraphicsPath borderPath = path.Clone() as GraphicsPath;
            path.AddRectangle(new Rectangle(-1, -1, this.Width + 1, this.Height + 1));
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            //e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            e.Graphics.FillPath(brush, path);
            if (!ShowBorder)
            {
                pen = new Pen(this.BackgroundColor);
            }

            e.Graphics.DrawPath(pen, borderPath);
            pen.Dispose();
            brush.Dispose();
            path.Dispose();
            borderPath.Dispose();
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
        }

        /// <summary>响应 MouseClick 事件。</summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            //this.Focus();
        }

        /// <summary>响应 CellClick 事件。</summary>
        protected override void OnCellClick(DataGridViewCellEventArgs e)
        {
            int columnIndex = e.ColumnIndex;
            int rowIndex = e.RowIndex;

            if (columnIndex >= 0 && rowIndex >= 0 && this.Columns[columnIndex].CellType.Name == "DataGridViewCheckBoxCell")
            {
                if ((bool)this.Rows[rowIndex].Cells[columnIndex].FormattedValue == true)
                {
                    this.Rows[rowIndex].Cells[columnIndex].Value = false;
                }
                else
                {
                    this.Rows[rowIndex].Cells[columnIndex].Value = true;
                }
                return;
            }
            base.OnCellClick(e);
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("单元格按钮单击"), Browsable(true), HDescriptionLanguage("单元格按钮单击")]
        public event DataGridViewButtonClicked CellButtonClicked;

        /// <summary>响应 OnButtonClicked 事件。</summary>
        internal void OnButtonClicked(int columnIndex, int rowIndex, object value, MouseButtons buttons, int clicks)
        {
            this.OnCellButtonClicked(columnIndex, rowIndex, value, buttons, clicks);
        }

        /// <summary>响应 CellButtonClicked 事件。</summary>
        protected void OnCellButtonClicked(int columnIndex, int rowIndex, object value, MouseButtons buttons, int clicks)
        {
            if (CellButtonClicked != null)
                CellButtonClicked(this, new DataGridViewButtonClickEventArgs(columnIndex, rowIndex, value, buttons, clicks));
        }

        /// <summary>LoadStyle 方法。</summary>
        public void LoadStyle(Style style)
        {
            switch (style)
            {
                case Style.Default:
                    this.AutoWidth = true;
                    this.BackgroundColor = System.Drawing.Color.White;
                    this.BorderStyle = System.Windows.Forms.BorderStyle.None;
                    this.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
                    this.CellDefaultColor = System.Drawing.Color.White;
                    this.CellEvenColor = System.Drawing.Color.WhiteSmoke;
                    this.CellForeColor = System.Drawing.Color.Black;
                    this.CellSelectBackColorBegin = System.Drawing.Color.Gainsboro;
                    this.CellSelectBackColorEnd = System.Drawing.Color.Gainsboro;
                    this.ColumnBackColorBegin = System.Drawing.Color.LightGray;
                    this.ColumnBackColorEnd = System.Drawing.Color.LightGray;
                    this.ColumnForeColor = System.Drawing.Color.Black;
                    this.ColumnHeadersHeight = 32;
                    this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                    this.EnableEvenBackColor = true;
                    this.GridColor = System.Drawing.Color.White;
                    this.RowHeadersWidth = 50;
                    this.RowTemplate.Height = 40;
                    this.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
                    this.ScrollBars = System.Windows.Forms.ScrollBars.None;
                    this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                    this.ShowCellBorder = false;
                    this.ShowColumnCheckBox = false;
                    this.TextAlign = HDataGridView.textAlign.Center;
                    this.TextAlignColumn = HDataGridView.textAlign.Center;
                    this.TextAlignRowHead = HDataGridView.textAlign.Center;
                    break;

                case Style.Style1:
                    this.AutoWidth = true;
                    this.BackgroundColor = System.Drawing.Color.White;
                    this.BorderStyle = System.Windows.Forms.BorderStyle.None;
                    this.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
                    this.CellDefaultColor = System.Drawing.Color.White;
                    this.CellEvenColor = System.Drawing.Color.White;
                    this.CellFont = new System.Drawing.Font(HTranslation.GetContent("微软雅黑"), 10F);
                    this.CellForeColor = System.Drawing.Color.Black;
                    this.CellSelectBackColorBegin = System.Drawing.Color.Gainsboro;
                    this.CellSelectBackColorEnd = System.Drawing.Color.Gainsboro;
                    this.ColumnBackColorBegin = System.Drawing.Color.LightGray;
                    this.ColumnBackColorEnd = System.Drawing.Color.LightGray;
                    this.ColumnFont = new System.Drawing.Font(HTranslation.GetContent("微软雅黑"), 11F);
                    this.ColumnForeColor = System.Drawing.Color.Black;
                    this.ColumnHeadersHeight = 32;
                    this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                    this.EnableEvenBackColor = true;
                    this.FirstColumnText = "Num.";
                    this.Font = new System.Drawing.Font(HTranslation.GetContent("微软雅黑"), 10F);
                    this.GridColor = System.Drawing.Color.White;
                    this.RowHeadersWidth = 50;
                    this.RowTemplate.Height = 40;
                    this.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
                    this.ScrollBars = System.Windows.Forms.ScrollBars.None;
                    this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
                    this.ShowCellBorder = false;
                    this.ShowColumnCheckBox = false;
                    this.TextAlign = HDataGridView.textAlign.Center;
                    this.TextAlignColumn = HDataGridView.textAlign.Center;
                    this.TextAlignRowHead = HDataGridView.textAlign.Center;
                    break;
            }
        }
    }
}