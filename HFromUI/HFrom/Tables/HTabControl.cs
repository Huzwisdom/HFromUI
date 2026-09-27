using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    [Obsolete("请使用 HTabControlExt，功能更完善", false)]
    public partial class HTabControl : TabControl
    {
        #region 属性定义

        /// <summary>tabBackColor 字段。</summary>
        private Color tabBackColor = Color.WhiteSmoke;
        /// <summary>tabSelectBackColor 字段。</summary>
        private Color tabSelectBackColor = Color.White;
        /// <summary>tabBorderColor 字段。</summary>
        private Color tabBorderColor = Color.LightGray;
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
        /// <summary>titleBaseColor 字段。</summary>
        private Color titleBaseColor = Color.WhiteSmoke;
        /// <summary>tabSelectLineColor 字段。</summary>
        private Color tabSelectLineColor = Color.DodgerBlue;

        /// <summary>tabFont 字段。</summary>
        private Font tabFont = new Font("微软雅黑", 10f);
        /// <summary>tabTextOffset 字段。</summary>
        private Point tabTextOffset = new Point(0, 0);
        /// <summary>showTabBorder 字段。</summary>
        private bool showTabBorder = false;
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
        private TextAlign tabTextAlign = TextAlign.CENTER;

        public enum TextAlign
        {
            LEFT,
            CENTER,
        }

        /// <summary>showTab 字段。</summary>
        private bool showTab = true;

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

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡标题栏背景色"), HDescriptionLanguage("tab标题栏背景色"), Browsable(true)]
        public Color TitleBaseColor
        {
            get { return titleBaseColor; }
            set
            {
                titleBaseColor = value;
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

        /// <summary>SelectLineHeight 成员。</summary>
        /// <summary>SelectLineHeight 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡下划线高度"), HDescriptionLanguage("tab下划线高度"), Browsable(true)]
        public int SelectLineHeight { get; set; } = 1;

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

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示选项卡按钮"), HDescriptionLanguage("是否显示tab按钮"), Browsable(true)]
        public bool ShowTab
        {
            get { return showTab; }
            set { showTab = value; this.Invalidate(); }
        }

        #endregion 属性定义

        /// <summary>TextDrawMode 成员。</summary>
        public drawMode TextDrawMode { get; set; } = drawMode.Anti;

        public enum drawMode
        {
            Anti,
            Clear
        }

        public HTabControl()
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

        private const Int32 TCM_FIRST = 0x1300;
        private const Int32 TCM_ADJUSTRECT = (TCM_FIRST + 40);

        private struct RECT
        {
            public Int32 Left;
            public Int32 Top;
            public Int32 Right;
            public Int32 Bottom;
        }

        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328 && !showTab)
            {
                m.Result = (IntPtr)1;
                return;
            }
            else if (m.Msg == TCM_ADJUSTRECT)
            {
                RECT rc = (RECT)m.GetLParam(typeof(RECT));
                //Adjust these values to suit, dependant upon Appearance
                rc.Left -= 4;
                rc.Right += 4;
                rc.Top -= 4;
                rc.Bottom += 4;
                Marshal.StructureToPtr(rc, m.LParam, true);
            }
            base.WndProc(ref m);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextDrawMode == drawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            LinearGradientBrush brush = new LinearGradientBrush(this.Bounds, baseColor, baseColor, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, new Rectangle(0, 0, this.Width, this.Height));
            SolidBrush brush2 = new SolidBrush(titleBaseColor);
            Rectangle titleBaseRect = new Rectangle(0, 0, this.Width, ItemSize.Height);
            switch (this.Alignment)
            {
                case TabAlignment.Top: titleBaseRect = new Rectangle(0, 0, this.Width, ItemSize.Height + 2); break;
                case TabAlignment.Left: titleBaseRect = new Rectangle(0, 0, ItemSize.Height + 4, this.Height); break;
                case TabAlignment.Right: titleBaseRect = new Rectangle(this.Width - ItemSize.Height - 4, 0, ItemSize.Height + 4, this.Height); break;
                case TabAlignment.Bottom: titleBaseRect = new Rectangle(0, this.Height - ItemSize.Height - 4, this.Width, ItemSize.Height + 4); break;
            }

            Bitmap bmp = new Bitmap(titleBaseRect.Width, titleBaseRect.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(TitleBaseColor);
            e.Graphics.DrawImage(bmp, titleBaseRect);
            //e.Graphics.FillRectangle(brush2, titleBaseRect);
            g.Dispose();
            bmp.Dispose();
            if (this.TabCount > 0 && showTab)
            {
                for (int i = 0; i < TabCount; i++)
                {
                    PaintTab(e, i);
                }
            }

            //if (showAddButton)
            //{
            //    if (mouseInAdd)
            //    {
            //        PaintAddButton(e, tabInAddButtonColor);
            //    }
            //    else
            //    {
            //        PaintAddButton(e, tabAddButtonColor);
            //    }
            //}
        }

        /// <summary>PaintTab 方法。</summary>
        private void PaintTab(PaintEventArgs e, int index)
        {
            #region 背景

            GraphicsPath tabPath = this.GetTabPath(index);
            Rectangle TabRect = this.GetTabRectFix(index);
            if (index > this.SelectedIndex && (this.Alignment == TabAlignment.Top || this.Alignment == TabAlignment.Bottom))
            {
                TabRect.X -= 1;
            }

            if (TabRect.Width <= 0 || TabRect.Height <= 0)
                return;

            Bitmap bmp = new Bitmap(TabRect.Width, TabRect.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextDrawMode == drawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (this.SelectedIndex == index)
            {
                g.Clear(tabSelectBackColor);

                if (showSelectLine)
                {
                    Brush brush_selectline = new SolidBrush(tabSelectLineColor);
                    Pen pen_selectline = new Pen(tabSelectLineColor, 0.0001f);
                    Rectangle selectlineRect = new Rectangle(0, bmp.Height - SelectLineHeight, bmp.Width, SelectLineHeight);
                    if (this.Alignment == TabAlignment.Top)
                    {
                        selectlineRect = new Rectangle(0, bmp.Height - SelectLineHeight, bmp.Width, SelectLineHeight);
                    }
                    else if (this.Alignment == TabAlignment.Bottom)
                    {
                        selectlineRect = new Rectangle(0, 0, bmp.Width, SelectLineHeight);
                    }
                    else if (this.Alignment == TabAlignment.Left)
                    {
                        selectlineRect = new Rectangle(0, 0, SelectLineHeight, bmp.Height);
                    }
                    else if (this.Alignment == TabAlignment.Right)
                    {
                        selectlineRect = new Rectangle(bmp.Width - SelectLineHeight, 0, SelectLineHeight, bmp.Height);
                    }

                    g.FillRectangle(brush_selectline, selectlineRect);
                    g.DrawRectangle(pen_selectline, selectlineRect);
                    brush_selectline.Dispose();
                    pen_selectline.Dispose();
                }

                if (showTabBorder)
                {
                    Pen pen_selectborder = new Pen(tabSelectBorderColor, 1f);
                    if (index != this.TabPages.Count - 1 && (this.Alignment == TabAlignment.Top || this.Alignment == TabAlignment.Bottom))
                    {
                        g.DrawRectangle(pen_selectborder, new Rectangle(0, 0, bmp.Width - 2, bmp.Height - 1));
                    }
                    else
                    {
                        g.DrawRectangle(pen_selectborder, new Rectangle(0, 0, bmp.Width - 1, bmp.Height - 1));
                    }
                    pen_selectborder.Dispose();
                }
            }
            else
            {
                if (index == 0 && index != this.SelectedIndex - 1)
                {
                    if (this.Alignment == TabAlignment.Top || this.Alignment == TabAlignment.Bottom)
                    {
                        g.Clear(tabBackColor);
                        if (showTabBorder)
                        {
                            Pen pen = new Pen(tabBorderColor, 1f);
                            g.DrawLine(pen, 0, 0, bmp.Width - 1, 0);
                            g.DrawLine(pen, 0, 0, 0, bmp.Height - 1);
                            g.DrawLine(pen, bmp.Width - 1, 0, bmp.Width - 1, bmp.Height - 1);
                            g.DrawLine(pen, 0, bmp.Height - 1, bmp.Width - 1, bmp.Height - 1);
                            pen.Dispose();
                        }
                    }
                    else if (this.Alignment == TabAlignment.Left || this.Alignment == TabAlignment.Right)
                    {
                        g.Clear(tabBackColor);
                        if (showTabBorder)
                        {
                            Pen pen = new Pen(tabBorderColor, 1f);
                            g.DrawLine(pen, 0, 0, bmp.Width - 1, 0);
                            g.DrawLine(pen, 0, 0, 0, bmp.Height - 1);
                            g.DrawLine(pen, bmp.Width - 1, 0, bmp.Width - 1, bmp.Height - 1);
                            g.DrawLine(pen, 0, bmp.Height - 1, bmp.Width - 1, bmp.Height - 1);
                            pen.Dispose();
                        }
                    }
                }
                else if (index == 0 && index == this.SelectedIndex - 1)
                {
                    if (this.Alignment == TabAlignment.Top || this.Alignment == TabAlignment.Bottom)
                    {
                        g.Clear(tabBackColor);
                        if (showTabBorder)
                        {
                            Pen pen = new Pen(tabBorderColor, 1f);
                            g.DrawLine(pen, 0, 0, bmp.Width - 1, 0);
                            g.DrawLine(pen, 0, 0, 0, bmp.Height - 1);
                            g.DrawLine(pen, 0, bmp.Height - 1, bmp.Width - 1, bmp.Height - 1);
                            pen.Dispose();
                        }
                    }
                    else if (this.Alignment == TabAlignment.Left || this.Alignment == TabAlignment.Right)
                    {
                        g.Clear(tabBackColor);
                        if (showTabBorder)
                        {
                            Pen pen = new Pen(tabBorderColor, 1f);
                            g.DrawLine(pen, 0, 0, bmp.Width - 1, 0);
                            g.DrawLine(pen, 0, 0, 0, bmp.Height - 1);
                            g.DrawLine(pen, bmp.Width - 1, 0, bmp.Width - 1, bmp.Height - 1);

                            pen.Dispose();
                        }
                    }
                }
                else
                {
                    if (this.Alignment == TabAlignment.Top || this.Alignment == TabAlignment.Bottom)
                    {
                        g.Clear(tabBackColor);
                        if (showTabBorder)
                        {
                            Pen pen = new Pen(tabBorderColor, 1f);
                            if (index == this.SelectedIndex - 1)
                            {
                                g.DrawLine(pen, 0, 0, bmp.Width, 0);
                                g.DrawLine(pen, 0, bmp.Height - 1, bmp.Width, bmp.Height - 1);
                            }
                            else
                            {
                                g.DrawLine(pen, 0, 0, bmp.Width - 1, 0);
                                g.DrawLine(pen, bmp.Width - 1, 0, bmp.Width - 1, bmp.Height - 1);
                                g.DrawLine(pen, bmp.Width - 1, bmp.Height - 1, 0, bmp.Height - 1);
                            }

                            pen.Dispose();
                        }
                    }
                    else if (this.Alignment == TabAlignment.Left || this.Alignment == TabAlignment.Right)
                    {
                        g.Clear(tabBackColor);
                        if (showTabBorder)
                        {
                            Pen pen = new Pen(tabBorderColor, 1f);
                            if (index == this.SelectedIndex - 1)
                            {
                                g.DrawLine(pen, 0, 0, 0, bmp.Height - 1);
                                g.DrawLine(pen, bmp.Width - 1, 0, bmp.Width - 1, bmp.Height - 1);
                            }
                            else
                            {
                                g.DrawLine(pen, bmp.Width - 1, 0, bmp.Width - 1, bmp.Height - 1);
                                g.DrawLine(pen, bmp.Width - 1, bmp.Height - 1, 0, bmp.Height - 1);
                                g.DrawLine(pen, 0, bmp.Height - 1, 0, 0);
                            }

                            pen.Dispose();
                        }
                    }
                }
            }

            #endregion 背景

            #region 文本

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

            if (tabTextAlign == TextAlign.CENTER)
            {
                format.Alignment = StringAlignment.Center;
            }
            g.DrawString(tabtext, tabFont, brush, new Rectangle(0, 0, bmp.Width, bmp.Height), format);
            brush.Dispose();

            #endregion 文本

            #region 关闭按钮

            if (showCloseButton)
            {
                if (index != 0)
                {
                    Bitmap closebmp = new Bitmap(10, 10);
                    Graphics bg = Graphics.FromImage(closebmp);
                    bg.SmoothingMode = SmoothingMode.AntiAlias;
                    Pen closePen = new Pen(index == MouseInButtonIndex ? tabInCloseButtonColor : tabCloseButtonColor, 1f);
                    bg.DrawLine(closePen, 0f, 0f, 9f, 9f);
                    bg.DrawLine(closePen, 9f, 0f, 0f, 9f);

                    Point p = new Point(bmp.Width - closebmp.Width - 5, (bmp.Height - closebmp.Height) / 2);
                    g.DrawImage(closebmp, new Rectangle(p, closebmp.Size));
                    bg.Dispose();
                    closebmp.Dispose();
                    closePen.Dispose();
                }
            }

            #endregion 关闭按钮

            e.Graphics.DrawImage(bmp, TabRect.Location);
            bmp.Dispose();
            g.Dispose();
        }

        public event EventHandler CloseButtonClicked;

        public event EventHandler AddButtonClicked;

        /// <summary>PPTabControl_MouseDown 方法。</summary>
        private void PPTabControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (showCloseButton)
                {
                    if (this.TabPages.Count > 1 && SelectedIndex != 0)
                    {
                        int x = e.X, y = e.Y;
                        //计算关闭区域
                        Rectangle TabRect = this.GetTabRectFix(this.SelectedIndex);
                        Rectangle closeRect = new Rectangle(TabRect.X + TabRect.Width - CLOSE_BTN_SIZE - 2, TabRect.Y + (TabRect.Height - CLOSE_BTN_SIZE) / 2, CLOSE_BTN_SIZE, CLOSE_BTN_SIZE);

                        bool isClose = closeRect.Contains(x, y);
                        if (isClose == true)
                        {
                            //this.TabPages.Remove(this.SelectedTab);
                            if (CloseButtonClicked != null)
                            {
                                CloseButtonClicked(this, new EventArgs());
                            }
                        }
                    }
                }

                if (showAddButton)
                {
                    Rectangle tabrect = this.GetTabRectFix(this.TabPages.Count - 1);
                    Rectangle addRect = new Rectangle(tabrect.Right, (tabrect.Height - ADD_BTN_SIZE) / 2, ADD_BTN_SIZE, ADD_BTN_SIZE);
                    if (addRect.Contains(e.Location))
                    {
                        if (AddButtonClicked != null)
                        {
                            AddButtonClicked(this, new EventArgs());
                        }

                        //TabPage tp = new TabPage();
                        //tp.Text = "hTabPage";
                        //this.TabPages.Add(tp);
                        //this.SelectedIndex = this.TabPages.Count - 1;
                        //this.Invalidate();
                    }
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
            if (showCloseButton)
            {
                int x = e.X;
                int y = e.Y;
                if (this.TabCount > 0)
                {
                    for (int i = 1; i < TabCount; i++)
                    {
                        Rectangle TabRect = this.GetTabRectFix(i);
                        Rectangle closeRect = new Rectangle(TabRect.X + TabRect.Width - CLOSE_BTN_SIZE - 5, TabRect.Y + (TabRect.Height - CLOSE_BTN_SIZE) / 2, CLOSE_BTN_SIZE, CLOSE_BTN_SIZE);
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

        /// <summary>PPTabControl_MouseLeave 方法。</summary>
        private void PPTabControl_MouseLeave(object sender, EventArgs e)
        {
            MouseInButtonIndex = -1;
            this.Refresh();
        }

        /// <summary>获取 tabPath。</summary>
        private GraphicsPath GetTabPath(int index)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.Reset();

            Rectangle rect = this.GetTabRectFix(index);

            switch (Alignment)
            {
                case TabAlignment.Top:

                    break;

                case TabAlignment.Bottom:

                    break;

                case TabAlignment.Left:

                    break;

                case TabAlignment.Right:

                    break;
            }

            path.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom + 1);
            path.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
            path.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom + 1);
            path.AddLine(rect.Right, rect.Bottom + 1, rect.Left, rect.Bottom + 1);

            return path;
        }

        /// <summary>获取 tabRectFix。</summary>
        private Rectangle GetTabRectFix(int index)
        {
            Rectangle rect = GetTabRect(index);

            if (this.Alignment == TabAlignment.Top)
            {
                if (index == 0)
                {
                    rect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 2, rect.Height + 2);
                }
                else
                {
                    rect = new Rectangle(rect.X, rect.Y - 2, rect.Width, rect.Height + 2);
                }
            }
            else if (this.Alignment == TabAlignment.Bottom)
            {
                if (index == 0)
                {
                    rect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 2, rect.Height + 4);
                }
                else
                {
                    rect = new Rectangle(rect.X, rect.Y - 2, rect.Width, rect.Height + 4);
                }
            }
            else if (this.Alignment == TabAlignment.Left || this.Alignment == TabAlignment.Right)
            {
                if (index == 0)
                {
                    rect = new Rectangle(rect.X - 2, rect.Y - 2, rect.Width + 4, rect.Height + 2);
                }
                else
                {
                    rect = new Rectangle(rect.X - 2, rect.Y, rect.Width + 4, rect.Height);
                }
            }

            return rect;
        }
    }
}