using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace HFromUI.HFrom.Tables
{
    public class HDataGridViewButtonCell : DataGridViewButtonCell
    {
        /// <summary>regularBaseColor 字段。</summary>
        private Color regularBaseColor = Color.White;
        /// <summary>regularLineColor 字段。</summary>
        private Color regularLineColor = Color.LightGray;
        /// <summary>regularTextColor 字段。</summary>
        private Color regularTextColor = Color.Black;

        /// <summary>mouseInBaseColor 字段。</summary>
        private Color mouseInBaseColor = Color.LightGray;
        /// <summary>mouseInLineColor 字段。</summary>
        private Color mouseInLineColor = Color.Silver;
        /// <summary>mouseInTextColor 字段。</summary>
        private Color mouseInTextColor = Color.Black;

        /// <summary>mouseDownBaseColor 字段。</summary>
        private Color mouseDownBaseColor = Color.DimGray;
        /// <summary>mouseDownLineColor 字段。</summary>
        private Color mouseDownLineColor = Color.Silver;
        /// <summary>mouseDownTextColor 字段。</summary>
        private Color mouseDownTextColor = Color.Black;

        /// <summary>radius 字段。</summary>
        private int radius = 5;
        /// <summary>线宽。</summary>
        private int lineWidth = 1;
        /// <summary>xoffset 字段。</summary>
        private int xoffset = 0;
        /// <summary>yoffset 字段。</summary>
        private int yoffset = 0;
        /// <summary>xoffsetIco 字段。</summary>
        private int xoffsetIco = 0;
        /// <summary>yoffsetIco 字段。</summary>
        private int yoffsetIco = 0;

        /// <summary>icoRegular 字段。</summary>
        private Image icoRegular = null;
        /// <summary>icoIn 字段。</summary>
        private Image icoIn = null;
        /// <summary>icoDown 字段。</summary>
        private Image icoDown = null;
        /// <summary>icoSize 字段。</summary>
        private Size icoSize = new Size(15, 15);

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set
            {
                radius = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度,只能是奇数"), Browsable(true)]
        public int LineWidth
        {
            get { return lineWidth; }
            set
            {
                lineWidth = value;// % 2 == 0 ? value + 1 : value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本x方向偏移量"), HDescriptionLanguage("文本x方向偏移量，为正向右，为负向左"), Browsable(true)]
        public int Xoffset
        {
            get { return xoffset; }
            set
            {
                xoffset = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本y方向偏移量"), HDescriptionLanguage("文本y方向偏移量，为正向下，为负向上"), Browsable(true)]
        public int Yoffset
        {
            get { return yoffset; }
            set
            {
                yoffset = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("正常背景色"), HDescriptionLanguage("正常背景色"), Browsable(true)]
        public Color RegularBaseColor
        {
            get { return regularBaseColor; }
            set
            {
                regularBaseColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("正常边框色"), HDescriptionLanguage("正常边框色"), Browsable(true)]
        public Color RegularLineColor
        {
            get { return regularLineColor; }
            set
            {
                regularLineColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("正常文本色"), HDescriptionLanguage("正常文本色"), Browsable(true)]
        public Color RegularTextColor
        {
            get { return regularTextColor; }
            set
            {
                regularTextColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标选中背景色"), HDescriptionLanguage("鼠标选中背景色"), Browsable(true)]
        public Color MouseInBaseColor
        {
            get { return mouseInBaseColor; }
            set
            {
                mouseInBaseColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标选中边框色"), HDescriptionLanguage("鼠标选中边框色"), Browsable(true)]
        public Color MouseInLineColor
        {
            get { return mouseInLineColor; }
            set
            {
                mouseInLineColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标选中文本色"), HDescriptionLanguage("鼠标选中文本色"), Browsable(true)]
        public Color MouseInTextColor
        {
            get { return mouseInTextColor; }
            set
            {
                mouseInTextColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标按下背景色"), HDescriptionLanguage("鼠标按下背景色"), Browsable(true)]
        public Color MouseDownBaseColor
        {
            get { return mouseDownBaseColor; }
            set
            {
                mouseDownBaseColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标按下边框色"), HDescriptionLanguage("鼠标按下边框色"), Browsable(true)]
        public Color MouseDownLineColor
        {
            get { return mouseDownLineColor; }
            set
            {
                mouseDownLineColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标按下文本色"), HDescriptionLanguage("鼠标按下文本色"), Browsable(true)]
        public Color MouseDownTextColor
        {
            get { return mouseDownTextColor; }
            set
            {
                mouseDownTextColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("正常显示图标"), HDescriptionLanguage("正常显示图标"), Browsable(true)]
        public Image IcoRegular
        {
            get { return icoRegular; }
            set
            {
                icoRegular = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标选中图标"), HDescriptionLanguage("鼠标选中图标"), Browsable(true)]
        public Image IcoIn
        {
            get { return icoIn; }
            set
            {
                icoIn = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("鼠标按下图标"), HDescriptionLanguage("鼠标按下图标"), Browsable(true)]
        public Image IcoDown
        {
            get { return icoDown; }
            set
            {
                icoDown = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图标大小"), HDescriptionLanguage("图标大小"), Browsable(true)]
        public Size IcoSize
        {
            get { return icoSize; }
            set
            {
                icoSize = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图标x方向偏移量"), HDescriptionLanguage("图标x方向偏移量，为正向下，为负向上"), Browsable(true)]
        public int XoffsetIco
        {
            get { return xoffsetIco; }
            set
            {
                xoffsetIco = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图标y方向偏移量"), HDescriptionLanguage("图标y方向偏移量，为正向下，为负向上"), Browsable(true)]
        public int YoffsetIco
        {
            get { return yoffsetIco; }
            set
            {
                yoffsetIco = value;
            }
        }

        /// <summary>
        /// 如果按钮文本没有单独设置的话，将使用column中设置的文本
        /// </summary>
        private string m_text = string.Empty;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("按钮文本"), HDescriptionLanguage("按钮文本"), Browsable(true), Localizable(true)]
        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(m_text))
                    m_text = (this.OwningColumn as DataGridViewButtonColumn).Text;
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }

        /// <summary>buttonSize 字段。</summary>
        private Size buttonSize = new Size(60, 25);

        public Size ButtonSize
        {
            get { return buttonSize; }
            set
            {
                buttonSize = value;
            }
        }

        /// <summary>AutoSize 成员。</summary>
        public bool AutoSize { get; set; } = false;

        /// <summary>m_buttonRegion 字段。</summary>
        protected Rectangle m_buttonRegion = Rectangle.Empty;// 当前按钮位置大小

        /// <summary>m_absBtnRegion 字段。</summary>
        protected Rectangle m_absBtnRegion = Rectangle.Empty;// 当前按钮绝对位置大小

        /// <summary>m_curBtnState 字段。</summary>
        protected PushButtonState m_curBtnState = PushButtonState.Normal;// 当前按钮状态

        public HDataGridView PPDataGridView
        {
            get { return this.DataGridView as HDataGridView; }
        }

        public HDataGridViewButtonCell()
        {
            m_curBtnState = PushButtonState.Normal;// 当前按钮状态
        }

        /// <summary>克隆。</summary>
        public override object Clone()
        {
            return base.Clone();
        }

        /// <summary>绘制。</summary>
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rect = GetSmallRectOfRectangle(cellBounds, buttonSize, out this.m_absBtnRegion);
            DrawButton(graphics, rect, m_curBtnState);
            //ButtonRenderer.DrawButton(graphics, rect, m_curBtnState);
            //TextRenderer.DrawText(graphics, this.Text, this.PPDataGridView.CellFont, rect, this.PPDataGridView.CellForeColor);
            //base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }

        /// <summary>DrawButton 方法。</summary>
        private void DrawButton(Graphics g, Rectangle rect, PushButtonState buttonstate)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            if (this.PPDataGridView.TextDrawMode == HDataGridView.DrawMode.Clear)
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            }
            Color backColor = regularBaseColor;
            Color lineColor = regularLineColor;
            Color foreColor = regularTextColor;
            Image img = icoRegular;
            if (buttonstate == PushButtonState.Hot)
            {
                backColor = mouseInBaseColor;
                lineColor = mouseInLineColor;
                foreColor = mouseInTextColor;
                img = icoIn;
            }
            else if (buttonstate == PushButtonState.Pressed)
            {
                backColor = mouseDownBaseColor;
                lineColor = mouseDownLineColor;
                foreColor = mouseDownTextColor;
                img = icoDown;
            }

            GraphicsPath path1 = CreateRound(rect, radius);
            g.FillPath(new SolidBrush(backColor), path1);
            g.DrawPath(new Pen(lineColor, lineWidth), path1);
            Size fontsize = TextRenderer.MeasureText(this.Text, this.PPDataGridView.CellFont);
            g.DrawString(this.Text, this.PPDataGridView.CellFont, new SolidBrush(foreColor), new Point(rect.X + rect.Width / 2 - fontsize.Width / 2 + 2 + xoffset, rect.Y + rect.Height / 2 - fontsize.Height / 2 + yoffset));
            if (img != null)
            {
                g.DrawImage(img, new Rectangle(new Point(rect.X + xoffsetIco, rect.Y + yoffsetIco), icoSize));
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        /// <summary>CreateRound 方法。</summary>
        private GraphicsPath CreateRound(Rectangle rect, int radius)
        {
            GraphicsPath roundRect = new GraphicsPath();
            int halfradius = radius % 2 == 0 ? radius / 2 : radius / 2 + 1;
            if (radius != 0)
            {
                //顶端
                //roundRect.AddLine(rect.Left + halfradius-1, rect.Top, rect.Right - halfradius, rect.Top);
                //右上角
                roundRect.AddArc(rect.Right - radius, rect.Top, radius, radius, 270f, 90f);
                //右边
                //roundRect.AddLine(rect.Right, rect.Top + halfradius, rect.Right, rect.Bottom - halfradius);
                //右下角

                roundRect.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0f, 90f);
                //底边
                //roundRect.AddLine(rect.Right - halfradius, rect.Bottom, rect.Left + halfradius, rect.Bottom);
                //左下角
                roundRect.AddArc(rect.Left, rect.Bottom - radius, radius, radius, 90f, 90f);
                //左边
                //roundRect.AddLine(rect.Left, rect.Top + halfradius, rect.Left, rect.Bottom - halfradius);
                //左上角
                roundRect.AddArc(rect.Left, rect.Top, radius, radius, 180f, 90f);

                roundRect.CloseFigure();
            }
            else
            {
                //顶端
                roundRect.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
                //右边
                roundRect.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
                //底边
                roundRect.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
                //左边
                roundRect.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom);
            }

            return roundRect;
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsInRegion(e.Location, e.ColumnIndex, e.RowIndex))
            {
                this.m_curBtnState = PushButtonState.Hot;
                this.DataGridView.Cursor = Cursors.Hand;
            }
            else
            {
                this.m_curBtnState = PushButtonState.Normal;
                this.DataGridView.Cursor = Cursors.Default;
            }
            this.DataGridView.InvalidateCell(this);
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);
            this.m_curBtnState = PushButtonState.Normal;
            this.DataGridView.Cursor = Cursors.Default;
            this.DataGridView.InvalidateCell(this);
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (IsInRegion(e.Location, e.ColumnIndex, e.RowIndex))
            {
                this.m_curBtnState = PushButtonState.Pressed;
            }
            else
            {
                this.m_curBtnState = PushButtonState.Normal;
            }
            this.DataGridView.InvalidateCell(this);
        }

        /// <summary>响应 MouseClick 事件。</summary>
        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            if (this.PPDataGridView != null)
                this.PPDataGridView.OnButtonClicked(e.ColumnIndex, e.RowIndex, this.Value, e.Button, e.Clicks);
            base.OnMouseClick(e);
        }

        /// <summary>响应 MouseDoubleClick 事件。</summary>
        protected override void OnMouseDoubleClick(DataGridViewCellMouseEventArgs e)
        {
            if (this.PPDataGridView != null)
                this.PPDataGridView.OnButtonClicked(e.ColumnIndex, e.RowIndex, this.Value, e.Button, e.Clicks);
            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// 是否在Button按钮区域
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected bool IsInRegion(Point p, int columnIndex, int rowIndex)
        {
            Rectangle cellBounds = DataGridView[columnIndex, rowIndex].ContentBounds;
            GetSmallRectOfRectangle(cellBounds, buttonSize, out this.m_absBtnRegion);
            return this.m_absBtnRegion.Contains(p);
        }

        /// <summary>获取 smallRectOfRectangle。</summary>
        private Rectangle GetSmallRectOfRectangle(Rectangle rectangle, Size smallSize, out Rectangle absRectangle)
        {
            Rectangle rect = new Rectangle();
            if (!AutoSize)
            {
                absRectangle = new Rectangle();
                absRectangle.Size = smallSize;
                absRectangle.X = (rectangle.Width - smallSize.Width) / 2;
                absRectangle.Y = (rectangle.Height - smallSize.Height) / 2;
                rect.Size = smallSize;
                rect.X = absRectangle.X + rectangle.X;
                rect.Y = absRectangle.Y + rectangle.Y;
            }
            else
            {
                absRectangle = rectangle;
                rect.X = rectangle.X + 2;
                rect.Y = rectangle.Y + 2;
                rect.Width = rectangle.Width - 6;
                rect.Height = rectangle.Height - 6;
            }

            return rect;
        }
    }

    public class PPDataGridViewButtonColumn : DataGridViewButtonColumn
    {
        public PPDataGridViewButtonColumn()
        {
            this.CellTemplate = new HDataGridViewButtonCell();
        }

        public PPDataGridViewButtonColumn(string HeaderText)
        {
            this.CellTemplate = new HDataGridViewButtonCell();
            this.HeaderText = HeaderText;
            this.Text = HeaderText;
        }
    }
}