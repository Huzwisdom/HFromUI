using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    public partial class HTabControlExt : TabControl
    {
        #region 属性定义

        /// <summary>tabBackColor 字段。</summary>
        private Color tabBackColor = Color.WhiteSmoke;
        /// <summary>tabSelectBackColor 字段。</summary>
        private Color tabSelectBackColor = Color.White;
        /// <summary>tabBorderColor 字段。</summary>
        private Color tabBorderColor = Color.WhiteSmoke;
        /// <summary>tabSelectBorderColor 字段。</summary>
        private Color tabSelectBorderColor = Color.DarkGray;
        /// <summary>tabTextColor 字段。</summary>
        private Color tabTextColor = Color.Black;
        /// <summary>tabSelectTextColor 字段。</summary>
        private Color tabSelectTextColor = Color.DodgerBlue;
        /// <summary>tabCloseButtonColor 字段。</summary>
        private Color tabCloseButtonColor = Color.DimGray;
        /// <summary>tabInCloseButtonColor 字段。</summary>
        private Color tabInCloseButtonColor = Color.DodgerBlue;
        /// <summary>tabAddButtonColor 字段。</summary>
        private Color tabAddButtonColor = Color.DimGray;
        /// <summary>tabInAddButtonColor 字段。</summary>
        private Color tabInAddButtonColor = Color.DodgerBlue;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.WhiteSmoke;
        /// <summary>tabSelectLineColor 字段。</summary>
        private Color tabSelectLineColor = Color.DodgerBlue;

        /// <summary>tabFont 字段。</summary>
        private Font tabFont = new Font("微软雅黑", 10f);
        /// <summary>tabTextOffset 字段。</summary>
        private Point tabTextOffset = new Point(0, 0);
        /// <summary>showTabBorder 字段。</summary>
        private bool showTabBorder = true;
        /// <summary>showCloseButton 字段。</summary>
        private bool showCloseButton = true;
        /// <summary>showAddButton 字段。</summary>
        private bool showAddButton = true;
        /// <summary>showSelectLine 字段。</summary>
        private bool showSelectLine = false;
        /// <summary>CLOSE_BTN_SIZE 字段。</summary>
        private int CLOSE_BTN_SIZE = 10;
        /// <summary>ADD_BTN_SIZE 字段。</summary>
        private int ADD_BTN_SIZE = 11;
        private int MouseInButtonIndex = -1;//鼠标移动到关闭按钮的tab序号
        /// <summary>tabTextAlign 字段。</summary>
        private TextAlign tabTextAlign = TextAlign.LEFT;

        public enum TextAlign
        {
            LEFT,
            CENTER,
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡背景色"), HDescriptionLanguage("tab背景色"), Browsable(true)]
        public Color TabBackColor
        {
            get { return tabBackColor; }
            set
            {
                tabBackColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡选中背景色"), HDescriptionLanguage("tab选中背景色"), Browsable(true)]
        public Color TabSelectBackColor
        {
            get { return tabSelectBackColor; }
            set
            {
                tabSelectBackColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡边框色"), HDescriptionLanguage("tab边框色"), Browsable(true)]
        public Color TabBorderColor
        {
            get { return tabBorderColor; }
            set
            {
                tabBorderColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡选中边框色"), HDescriptionLanguage("tab选中边框色"), Browsable(true)]
        public Color TabSelectBorderColor
        {
            get { return tabSelectBorderColor; }
            set
            {
                tabSelectBorderColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡标题颜色"), HDescriptionLanguage("tab标题色"), Browsable(true)]
        public Color TabTextColor
        {
            get { return tabTextColor; }
            set
            {
                tabTextColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡选中标题色"), HDescriptionLanguage("tab选中标题色"), Browsable(true)]
        public Color TabSelectTextColor
        {
            get { return tabSelectTextColor; }
            set
            {
                tabSelectTextColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡关闭按钮颜色"), HDescriptionLanguage("tab关闭按钮色"), Browsable(true)]
        public Color TabCloseButtonColor
        {
            get { return tabCloseButtonColor; }
            set
            {
                tabCloseButtonColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡关闭按钮颜色"), HDescriptionLanguage("tab鼠标移入关闭按钮色"), Browsable(true)]
        public Color TabInCloseButtonColor
        {
            get { return tabInCloseButtonColor; }
            set
            {
                tabInCloseButtonColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景色"), Browsable(true)]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡选中下划线颜色"), HDescriptionLanguage("tab选中下划线色"), Browsable(true)]
        public Color TabSelectLineColor
        {
            get { return tabSelectLineColor; }
            set
            {
                tabSelectLineColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡标题字体"), HDescriptionLanguage("tab标题字体"), Browsable(true)]
        public Font TabFont
        {
            get { return tabFont; }
            set
            {
                tabFont = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡标题偏移"), HDescriptionLanguage("tab标题偏移"), Browsable(true)]
        public Point TabTextOffset
        {
            get { return tabTextOffset; }
            set
            {
                tabTextOffset = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示选项卡边框"), HDescriptionLanguage("tab是否显示边框"), Browsable(true)]
        public bool ShowTabBorder
        {
            get { return showTabBorder; }
            set
            {
                showTabBorder = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示关闭按钮"), HDescriptionLanguage("tab是否显示关闭按钮"), Browsable(true)]
        public bool ShowCloseButton
        {
            get { return showCloseButton; }
            set
            {
                showCloseButton = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示选择线条"), HDescriptionLanguage("tab是否显示选中下划线"), Browsable(true)]
        public bool ShowSelectLine
        {
            get { return showSelectLine; }
            set
            {
                showSelectLine = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡文本对齐"), HDescriptionLanguage("tab标题文字对齐方式"), Browsable(true)]
        public TextAlign TabTextAlign
        {
            get { return tabTextAlign; }
            set
            {
                tabTextAlign = value;
                this.Invalidate();
            }
        }

        #endregion 属性定义

        /// <summary>TextDrawMode 成员。</summary>
        public drawMode TextDrawMode { get; set; } = drawMode.Anti;

        public enum drawMode
        {
            Anti,
            Clear
        }

        public HTabControlExt()
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
            this.Size = new Size(400, 250);
            this.SizeMode = TabSizeMode.Fixed;
            this.ItemSize = new Size(100, 30);
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

        /// <summary>mouseInAdd 字段。</summary>
        private bool mouseInAdd = false;

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextDrawMode == drawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            LinearGradientBrush brush = new LinearGradientBrush(this.Bounds, baseColor, baseColor, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, new Rectangle(0, 0, this.Width, this.Height));

            if (this.TabCount > 0)
            {
                for (int i = 0; i < TabCount; i++)
                {
                    PaintTab(e, i);
                }
            }

            if (showAddButton)
            {
                if (mouseInAdd)
                {
                    PaintAddButton(e, tabInAddButtonColor);
                }
                else
                {
                    PaintAddButton(e, tabAddButtonColor);
                }
            }
        }

        /// <summary>PaintTab 方法。</summary>
        private void PaintTab(PaintEventArgs e, int index)
        {
            try
            {
                this.PaintTabBackGround(e, index);
                this.PaintTabText(e, index);
                if (showCloseButton)
                {
                    if (index == MouseInButtonIndex)
                    {
                        this.PaintTabCloseButton(e, index, tabInCloseButtonColor);
                    }
                    else
                    {
                        this.PaintTabCloseButton(e, index, tabCloseButtonColor);
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>PaintTabBackGround 方法。</summary>
        private void PaintTabBackGround(PaintEventArgs e, int index)
        {
            GraphicsPath tabPath = this.GetTabPath(index, 10f, true);
            GraphicsPath tabPathUnClose = this.GetTabPath(index, 10f, false);
            Rectangle TabRect = this.GetTabRect(index);
            if (this.SelectedIndex == index)
            {
                e.Graphics.FillPath(new SolidBrush(tabSelectBackColor), tabPath);
                if (showTabBorder)
                {
                    e.Graphics.DrawPath(new Pen(tabSelectBorderColor, 1f), tabPathUnClose);
                    //e.Graphics.DrawRectangle(new Pen(tabSelectBorderColor, 1f), TabRect);
                }

                if (showSelectLine)
                {
                    e.Graphics.DrawLine(new Pen(tabSelectLineColor), TabRect.Left, TabRect.Top + TabRect.Height, TabRect.Left + TabRect.Width, TabRect.Top + TabRect.Height);
                }
            }
            else
            {
                e.Graphics.FillPath(new SolidBrush(tabBackColor), tabPath);
                if (showTabBorder)
                {
                    e.Graphics.DrawPath(new Pen(tabBorderColor, 1f), tabPathUnClose);
                }
            }
        }

        /// <summary>PaintTabText 方法。</summary>
        private void PaintTabText(PaintEventArgs e, int index)
        {
            string tabtext = this.TabPages[index].Text;
            System.Drawing.StringFormat format = new System.Drawing.StringFormat();
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Center;
            format.Trimming = StringTrimming.EllipsisCharacter;
            SolidBrush brush;

            if (index == this.SelectedIndex)
            {
                brush = new SolidBrush(tabSelectTextColor);
            }
            else
            {
                brush = new SolidBrush(tabTextColor);
            }

            Rectangle rect = this.GetTabRect(index);
            SizeF fontsize = e.Graphics.MeasureString(this.TabPages[index].Text, this.Font);

            Rectangle rect2 = new Rectangle(rect.Left + (int)(rect.Width - fontsize.Width) / 2 - 2 + tabTextOffset.X, rect.Top + tabTextOffset.Y, rect.Width, rect.Height);
            if (tabTextAlign == TextAlign.LEFT)
            {
                rect2 = new Rectangle(9 + rect.Left + tabTextOffset.X, rect.Top + tabTextOffset.Y, rect.Width, rect.Height);
            }
            e.Graphics.DrawString(tabtext, tabFont, brush, rect2, format);
        }

        /// <summary>PaintTabCloseButton 方法。</summary>
        private void PaintTabCloseButton(PaintEventArgs e, int index, Color color)
        {
            Rectangle TabRect = this.GetTabRect(index);

            Bitmap bmp = new Bitmap(CLOSE_BTN_SIZE, CLOSE_BTN_SIZE);
            Graphics bg = Graphics.FromImage(bmp);
            bg.SmoothingMode = SmoothingMode.AntiAlias;
            Pen closePen = new Pen(color, 1f);
            bg.DrawLine(closePen, 1f, 1f, 9f, 9f);
            bg.DrawLine(closePen, 9f, 1f, 1f, 9f);

            Point p = new Point(TabRect.X + TabRect.Width - bmp.Width - 11, TabRect.Y + (TabRect.Height - bmp.Height) / 2);
            if (showAddButton)
            {
                p.X -= ADD_BTN_SIZE;
            }
            e.Graphics.DrawImage(bmp, new Rectangle(p, bmp.Size));
        }

        /// <summary>PaintAddButton 方法。</summary>
        private void PaintAddButton(PaintEventArgs e, Color color)
        {
            if (this.TabPages.Count > 0)
            {
                int index = this.TabPages.Count - 1;
                Rectangle rect = this.GetTabRect(index);

                Bitmap bmp = new Bitmap(ADD_BTN_SIZE, ADD_BTN_SIZE);
                Graphics bg = Graphics.FromImage(bmp);
                bg.SmoothingMode = SmoothingMode.AntiAlias;
                bg.DrawLine(new Pen(color), (float)bmp.Width / 2, 1f, (float)bmp.Width / 2, bmp.Height - 1f);
                bg.DrawLine(new Pen(color), 1f, (float)bmp.Height / 2, (float)bmp.Width - 1f, (float)bmp.Height / 2);

                e.Graphics.DrawImage(bmp, rect.Right - ADD_BTN_SIZE, rect.Y + (rect.Height - bmp.Height) / 2);
            }
        }

        /// <summary>PPTabControl_MouseDown 方法。</summary>
        private void PPTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (showCloseButton)
                {
                    if (this.TabPages.Count > 1)
                    {
                        int x = e.X, y = e.Y;
                        //计算关闭区域
                        Rectangle TabRect = this.GetTabRect(this.SelectedIndex);
                        Rectangle closeRect = new Rectangle(TabRect.X + TabRect.Width - CLOSE_BTN_SIZE - 11, TabRect.Y + (TabRect.Height - CLOSE_BTN_SIZE) / 2, CLOSE_BTN_SIZE, CLOSE_BTN_SIZE);
                        if (showAddButton)
                        {
                            closeRect.X -= ADD_BTN_SIZE;
                        }
                        bool isClose = closeRect.Contains(x, y);
                        if (isClose == true)
                        {
                            this.TabPages.Remove(this.SelectedTab);
                        }
                    }
                }

                if (showAddButton)
                {
                    Rectangle tabrect = this.GetTabRect(this.TabPages.Count - 1);
                    Rectangle addRect = new Rectangle(tabrect.Right - ADD_BTN_SIZE, (tabrect.Height - ADD_BTN_SIZE) / 2, ADD_BTN_SIZE, ADD_BTN_SIZE);
                    if (addRect.Contains(e.Location))
                    {
                        TabPage tp = new TabPage();
                        tp.Text = "hTabPage";
                        this.TabPages.Add(tp);
                        this.SelectedIndex = this.TabPages.Count - 1;
                        this.Invalidate();
                    }
                }
            }
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //base.OnMouseMove(e);
            if (showAddButton)
            {
                Rectangle tabrect = this.GetTabRect(this.TabPages.Count - 1);
                Rectangle addRect = new Rectangle(tabrect.Right - ADD_BTN_SIZE, (tabrect.Height - ADD_BTN_SIZE) / 2, ADD_BTN_SIZE, ADD_BTN_SIZE);
                if (addRect.Contains(e.Location))
                {
                    mouseInAdd = true;
                    this.Cursor = Cursors.Hand;
                    this.Invalidate();
                    return;
                }
                else
                {
                    mouseInAdd = false;
                }
            }

            if (showCloseButton)
            {
                int x = e.X;
                int y = e.Y;
                if (this.TabCount > 0)
                {
                    for (int i = 0; i < TabCount; i++)
                    {
                        Rectangle TabRect = this.GetTabRect(i);
                        Rectangle closeRect = new Rectangle(TabRect.X + TabRect.Width - CLOSE_BTN_SIZE - 11, TabRect.Y + (TabRect.Height - CLOSE_BTN_SIZE) / 2, CLOSE_BTN_SIZE, CLOSE_BTN_SIZE);
                        if (showAddButton)
                        {
                            closeRect.X -= ADD_BTN_SIZE;
                        }
                        if (closeRect.Contains(x, y))
                        {
                            MouseInButtonIndex = i;
                            this.Cursor = Cursors.Hand;
                            this.Refresh();
                            return;
                        }
                    }
                    MouseInButtonIndex = -1;
                    this.Cursor = Cursors.Default;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// 鼠标移动到关闭按钮上改变颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PPTabControl_MouseMove(object sender, MouseEventArgs e)
        {
        }

        /// <summary>获取 tabPath。</summary>
        private GraphicsPath GetTabPath(int index, float radius, bool close)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.Reset();

            Rectangle rect = this.GetTabRect(index);
            if (showAddButton)
            {
                rect.Width -= ADD_BTN_SIZE;
            }

            float len = 1.414213f * radius / 4;
            path.AddCurve(new PointF[] { new PointF(rect.Left + 1, rect.Bottom + 1), new PointF(rect.Left + len, rect.Bottom - radius / 2 + len), new PointF(rect.Left + radius / 2, rect.Top + radius / 2), new PointF(rect.Left + radius - len, rect.Top + radius / 2 - len), new PointF(rect.Left + radius, rect.Top) });
            path.AddLine(rect.Left + radius, rect.Top, rect.Right - radius, rect.Top);
            path.AddCurve(new PointF[] { new PointF(rect.Right - radius, rect.Top), new PointF(rect.Right - radius + len, rect.Top + radius / 2 - len), new PointF(rect.Right - radius / 2, rect.Top + radius / 2), new PointF(rect.Right - len, rect.Bottom - radius / 2 + len), new PointF(rect.Right - 1, rect.Bottom + 1) });
            if (close)
            {
                path.CloseFigure();
            }

            return path;
        }
    }
}