using HFromUI.HAttribute;
using HFromUI.HData;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Bars
{
    using HFromUI.HData.Win;
    public partial class HVScrollBarExt : HBarBase
    {
        #region 私有参数
        /// <summary>值。</summary>
        private int value = 0;
        /// <summary>maximum 字段。</summary>
        private int maximum = 100;
        /// <summary>minimum 字段。</summary>
        private int minimum = 0;
        /// <summary>visibleValue 字段。</summary>
        private int visibleValue = 10;
        /// <summary>smallChange 字段。</summary>
        private int smallChange = 1;
        /// <summary>largeChange 字段。</summary>
        private int largeChange = 10;

        /// <summary>baseSize 字段。</summary>
        private int baseSize = 2;
        /// <summary>baseRadius 字段。</summary>
        private int baseRadius = 0;
        /// <summary>buttonSize 字段。</summary>
        private int buttonSize = 10;
        /// <summary>buttonRadius 字段。</summary>
        private int buttonRadius = 10;

        /// <summary>buttonColor 字段。</summary>
        private Color buttonColor = Color.DodgerBlue;
        /// <summary>buttonHoverColor 字段。</summary>
        private Color buttonHoverColor = Color.DeepSkyBlue;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.Gainsboro;



        protected bool isMouseHover = false;//表示鼠标是否移入滚动按钮
        protected bool isMouseDown = false;//表示鼠标是否在滚动按钮按下
        protected int mouseDownPoint = 0;//记录鼠标在滚动按钮按下时鼠标距离按钮上边缘的距离

        /// <summary>bindControl 字段。</summary>
        private Control bindControl = null;
        public Control BindControl
        {
            get { return bindControl; }
        }

        #endregion

        #region 公共参数
        [HCategoryLanguage("通用"), HDisplayNameLanguage("滚动条值"), HDescriptionLanguage("滚动条值"), Browsable(true)]
        public new int Value
        {
            get { return this.value; }
            set
            {
                if (value > maximum - visibleValue)
                {
                    this.value = maximum - visibleValue;
                }
                else if (value < minimum)
                {
                    this.value = minimum;
                }
                else
                {
                    this.value = value;
                }
                SyncBaseState(minimum, maximum, this.value);
                this.Invalidate();

            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("滚动条最大值"), HDescriptionLanguage("滚动条最大值"), Browsable(true)]
        public int Maximum
        {
            get { return maximum; }
            set
            {
                if (value < minimum)
                    return;
                this.maximum = value;
                if (this.value > maximum)
                {
                    this.value = maximum;
                }
                //visibleValue =(int)((this.Height - 1) * ((float)largeChange / (maximun - minimun)));
                SyncBaseState(minimum, maximum, this.value);
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("滚动条最小值"), HDescriptionLanguage("滚动条最小值"), Browsable(true)]
        public int Minimum
        {
            get { return this.minimum; }
            set
            {
                if (value > maximum)
                    return;
                this.minimum = value;
                if (this.value < minimum)
                {
                    this.value = minimum;
                }
                SyncBaseState(minimum, maximum, this.value);
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("单击箭头的变化幅度"), HDescriptionLanguage("单击箭头的变化幅度"), Browsable(true)]
        public int SmallChange
        {
            get { return smallChange; }
            set
            {
                if (value < 0 || value > (maximum - minimum))
                    return;
                this.smallChange = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("单击滚动条的变化幅度"), HDescriptionLanguage("单击滚动条的变化幅度"), Browsable(true)]
        public int LargeChange
        {
            get { return largeChange; }
            set
            {
                if (value < 0 || value > (maximum - minimum))
                    return;
                this.largeChange = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("可见值"), HDescriptionLanguage("绑定控件当前能显示的高度"), Browsable(true)]
        public int VisibleValue
        {
            get { return visibleValue; }
            set
            {

                this.visibleValue = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("背景的宽度"), HDescriptionLanguage("背景的宽度"), Browsable(true)]
        public int BaseSize
        {
            get { return baseSize; }
            set
            {

                this.baseSize = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("背景的圆角"), HDescriptionLanguage("背景的圆角"), Browsable(true)]
        public int BaseRadius
        {
            get { return baseRadius; }
            set
            {

                this.baseRadius = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("滚动按钮的宽度"), HDescriptionLanguage("滚动按钮的宽度"), Browsable(true)]
        public int ButtonSize
        {
            get { return buttonSize; }
            set
            {

                this.buttonSize = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("滚动按钮的圆角"), HDescriptionLanguage("滚动按钮的圆角"), Browsable(true)]
        public int ButtonRadius
        {
            get { return buttonRadius; }
            set
            {

                this.buttonRadius = value;
                this.Invalidate();
            }
        }


        [HCategoryLanguage("通用"), HDisplayNameLanguage("滚动按钮颜色"), HDescriptionLanguage("滚动按钮颜色"), Browsable(true)]
        public Color ButtonColor
        {
            get { return buttonColor; }
            set
            {
                buttonColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("按钮悬停颜色"), HDescriptionLanguage("滚动按钮鼠标移入的颜色"), Browsable(true)]
        public Color ButtonHoverColor
        {
            get { return buttonHoverColor; }
            set
            {
                buttonHoverColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景颜色"), Browsable(true)]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                this.Invalidate();
            }
        }
        #endregion

        public HVScrollBarExt()
        {
            InitializeComponent();
            SyncBaseState(minimum, maximum, value);
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

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            SolidBrush baseBrush = new SolidBrush(baseColor);
            SolidBrush buttonBrush = new SolidBrush(buttonColor);
            SolidBrush buttonHoverBrush = new SolidBrush(buttonHoverColor);


            GraphicsPath basepath = HDrawPaint.CreatePath(new RectangleF((float)(this.Width - baseSize) / 2, 0, baseSize, this.Height), baseRadius);
            g.FillPath(baseBrush, basepath);//画背景


            RectangleF buttonRect = GetButtonRect();
            GraphicsPath buttonPath = HDrawPaint.CreatePath(buttonRect, buttonRadius);
            if (isMouseHover)//判断鼠标是否移入按钮
            {
                g.FillPath(buttonHoverBrush, buttonPath);
            }
            else
            {
                g.FillPath(buttonBrush, buttonPath);
            }

            basepath.Dispose();
            buttonPath.Dispose();
            baseBrush.Dispose();
            buttonBrush.Dispose();
            buttonHoverBrush.Dispose();
        }

        /// <summary>响应 MouseWheel 事件。</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!isMouseDown)//鼠标未按下
            {
                if (e.Delta > 0)
                {
                    this.Value -= smallChange;
                }
                else
                {
                    this.Value += smallChange;
                }
                this.Invalidate();
                this.BeginInvoke(new Action(() => { OnValueChanged(EventArgs.Empty); }));
            }

        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                RectangleF buttonRect = GetButtonRect();
                Point mousePoint = e.Location;//获取鼠标相对于控件的位置

                if (buttonRect.Contains(mousePoint))
                {
                    isMouseDown = true;
                    mouseDownPoint = (int)(mousePoint.Y - buttonRect.Top);
                }
                else
                {
                    float buttonHeight = (float)((this.Height - 1) * ((float)visibleValue / (maximum - minimum)));
                    float y = mousePoint.Y - buttonHeight / 2;
                    this.Value = (int)((maximum - minimum - visibleValue) * y / (this.Height - buttonHeight));
                    this.BeginInvoke(new Action(() => { OnValueChanged(EventArgs.Empty); }));
                    this.Invalidate();
                }
            }

        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            RectangleF buttonRect = GetButtonRect();
            Point mousePoint = e.Location;//获取鼠标相对于控件的位置
            bool ishover = isMouseHover;
            if (!isMouseDown)
            {
                if (buttonRect.Contains(mousePoint))
                {
                    this.Cursor = Cursors.Hand;
                    isMouseHover = true;
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    isMouseHover = false;
                }
            }
            if (!(ishover && isMouseHover))
                this.Invalidate();

            if (isMouseDown)
            {

                int currentY = mousePoint.Y;

                int newTop = currentY - mouseDownPoint;



                this.Value = (int)((maximum - minimum - visibleValue) * (newTop) / (this.Height - 1 - buttonRect.Height));
                //Console.WriteLine("downY:{0},currentY:{1},delta:{2},value:{3}", downY, currentY, delta, Value);
                this.Invalidate();
                this.BeginInvoke(new Action(() => { OnValueChanged(EventArgs.Empty); }));
            }
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isMouseDown = false;
            RectangleF buttonRect = GetButtonRect();
            Point mousePoint = e.Location;//获取鼠标相对于控件的位置
            bool ishover = isMouseHover;
            if (buttonRect.Contains(mousePoint))
            {
                this.Cursor = Cursors.Hand;
                isMouseHover = true;
            }
            else
            {
                this.Cursor = Cursors.Default;
                isMouseHover = false;
            }
            if (!(ishover && isMouseHover))
                this.Invalidate();
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!isMouseDown)
            {
                isMouseHover = false;
            }
            this.Cursor = Cursors.Default;
            this.Invalidate();
        }

        /// <summary>获取 buttonRect。</summary>
        private RectangleF GetButtonRect()
        {
            float buttonHeight = (float)((this.Height ) * ((float)visibleValue / (maximum - minimum)));
            if (buttonHeight < 10)
                buttonHeight = 10;
            RectangleF buttonRect = new RectangleF((this.Width - buttonSize) / 2, (float)((this.Height + 1 - buttonHeight) * ((float)value / (maximum - minimum - visibleValue))), buttonSize, buttonHeight);//获取按钮区域
            return buttonRect;
        }

        /// <summary>
        /// 将滚动条绑定控件
        /// </summary>
        /// <param name="control"></param>
        public void BindingControl(Control control)
        {
            if (control == null || control.IsDisposed || bindControl != null)
                return;
            bindControl = control;
            this.ValueChanged += PPVScrollBarExt_ValueChanged;
            bindControl.Disposed += BindControl_Disposed;
            bindControl.MouseWheel += BindControl_MouseWheel;
            bindControl.SizeChanged += BindControl_SizeChanged;
            bindControl.VisibleChanged += BindControl_VisibleChanged;

            if (bindControl is RichTextBox)
            {
                (bindControl as RichTextBox).TextChanged += BindControl_TextChanged;
                (bindControl as RichTextBox).SelectionChanged += BindControl_SelectionChanged;
                (bindControl as RichTextBox).VScroll += BindControl_VScroll;
            }
            else if (bindControl is DataGridView)
            {
                (bindControl as DataGridView).RowHeightChanged += BindControl_RowHeightChanged;
                (bindControl as DataGridView).RowsAdded += BindControl_RowsAdded;
                (bindControl as DataGridView).RowsRemoved += BindControl_RowsRemoved;
            }
            else if(bindControl is TreeView)
            {
                (bindControl as TreeView).AfterExpand += BindControl_AfterExpand;
            }

            SetScrollBarNum();
        }

        /// <summary>BindControl_AfterExpand 方法。</summary>
        private void BindControl_AfterExpand(object sender, TreeViewEventArgs e)
        {
            SetScrollBarNum();
        }

        /// <summary>BindControl_VisibleChanged 方法。</summary>
        private void BindControl_VisibleChanged(object sender, EventArgs e)
        {
            SetScrollBarNum();
        }

        /// <summary>BindControl_VScroll 方法。</summary>
        private void BindControl_VScroll(object sender, EventArgs e)
        {
            SetScrollBarNum();
        }

        /// <summary>BindControl_SizeChanged 方法。</summary>
        private void BindControl_SizeChanged(object sender, EventArgs e)
        {
            if (bindControl == null || bindControl.IsDisposed)
                return;
            SetScrollBarNum();
        }

        /// <summary>BindControl_MouseWheel 方法。</summary>
        private void BindControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (bindControl != null && !bindControl.IsDisposed && (bindControl is RichTextBox||bindControl is FlowLayoutPanel))
            {
                SetScrollBarNum();
                return;
            }
            else
            {
                if (e.Delta > 0)
                {
                    this.Value -= largeChange;
                }
                else
                {
                    this.Value += largeChange;
                }
                this.Invalidate();

                if (bindControl == null || bindControl.IsDisposed)
                    return;
                SetControlNum();
            }
               
        }

        /// <summary>BindControl_Disposed 方法。</summary>
        private void BindControl_Disposed(object sender, EventArgs e)
        {
            bindControl.Disposed -= BindControl_Disposed;
            bindControl.MouseWheel -= BindControl_MouseWheel;
            bindControl.SizeChanged -= BindControl_SizeChanged;
            bindControl.VisibleChanged -= BindControl_VisibleChanged;
            if (bindControl is RichTextBox)
            {
                (bindControl as RichTextBox).TextChanged -= BindControl_TextChanged;
                (bindControl as RichTextBox).SelectionChanged -= BindControl_SelectionChanged;
                (bindControl as RichTextBox).VScroll -= BindControl_VScroll;
            }
            else if (bindControl is DataGridView)
            {
                (bindControl as DataGridView).RowHeightChanged -= BindControl_RowHeightChanged;
                (bindControl as DataGridView).RowsAdded -= BindControl_RowsAdded;
                (bindControl as DataGridView).RowsRemoved -= BindControl_RowsRemoved;
            }
            else if (bindControl is TreeView)
            {
                (bindControl as TreeView).AfterExpand -= BindControl_AfterExpand;
            }

            bindControl = null;
        }

        /// <summary>BindControl_TextChanged 方法。</summary>
        private void BindControl_TextChanged(object sender,EventArgs e)
        {
            if (bindControl == null || bindControl.IsDisposed)
                return;
            SetScrollBarNum();
        }

        /// <summary>BindControl_SelectionChanged 方法。</summary>
        private void BindControl_SelectionChanged(object sender,EventArgs e)
        {
            if (bindControl == null || bindControl.IsDisposed)
                return;
            SetScrollBarNum();
        }

        /// <summary>BindControl_RowHeightChanged 方法。</summary>
        private void BindControl_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
        {
            ResetDataGridViewScrollBarNum();
        }

        /// <summary>BindControl_RowsAdded 方法。</summary>
        private void BindControl_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            ResetDataGridViewScrollBarNum();
        }

        /// <summary>BindControl_RowsRemoved 方法。</summary>
        private void BindControl_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            ResetDataGridViewScrollBarNum();
        }

        /// <summary>PPVScrollBarExt_ValueChanged 方法。</summary>
        private void PPVScrollBarExt_ValueChanged(object sender, EventArgs e)
        {
            if (bindControl == null || bindControl.IsDisposed)
                return;
            SetControlNum();
        }

        /// <summary>设置 scrollBarNum。</summary>
        public void SetScrollBarNum()
        {
            if (bindControl == null || bindControl.IsDisposed)
                return;
            if (bindControl is DataGridView)
            {
                ResetDataGridViewScrollBarNum();
            }
            else if(bindControl is FlowLayoutPanel)
            {
                HWin32.Scrollinfo si = new HWin32.Scrollinfo();
                si.cbSize = (uint)Marshal.SizeOf(si);
                si.fMask = (int)(HWin32.ScrollInfoMask.SIF_DISABLENOSCROLL | HWin32.ScrollInfoMask.SIF_ALL);
                HWin32.GetScrollInfo(bindControl.Handle, (int)HWin32.ScrollBarDirection.SB_VERT, ref si);
                this.visibleValue = (int)si.nPage - 1; 
                this.Maximum = si.nMax;
                this.Minimum = si.nMin;
                this.Value = si.nPos;
                this.Visible = (bindControl as FlowLayoutPanel).VerticalScroll.Visible;
                this.BringToFront();
            }
            else if(bindControl is RichTextBox)
            {
                HWin32.Scrollinfo si = new HWin32.Scrollinfo();
                si.cbSize = (uint)Marshal.SizeOf(si);
                si.fMask = (int)(HWin32.ScrollInfoMask.SIF_DISABLENOSCROLL | HWin32.ScrollInfoMask.SIF_ALL);
                HWin32.GetScrollInfo(bindControl.Handle, (int)HWin32.ScrollBarDirection.SB_VERT, ref si);

                this.visibleValue = (int)si.nPage - 1;
                this.Maximum = si.nMax;
                this.Minimum = si.nMin;
                this.Value = si.nPos;
                RichTextBox richTextBox = (bindControl as RichTextBox);

                if (richTextBox.ScrollBars == RichTextBoxScrollBars.ForcedVertical || richTextBox.ScrollBars == RichTextBoxScrollBars.ForcedBoth)
                {
                    this.Visible = true;
                    this.BringToFront();
                }
                else if(richTextBox.ScrollBars == RichTextBoxScrollBars.Vertical || richTextBox.ScrollBars == RichTextBoxScrollBars.Both)
                {
                    if (si.nPage == 0||visibleValue>=Maximum)
                    {
                        this.Visible = false;
                    }
                    else
                    {
                        if (richTextBox.Text == "")
                        {
                            this.Visible = false;
                        }
                        else
                        {
                            int height = richTextBox.Font.Height;// TextRenderer.MeasureText("AA", richTextBox.Font).Height;
                            int pageline = richTextBox.Height / height;//能显示的行数
                            int line = richTextBox.GetLineFromCharIndex(richTextBox.TextLength)+2;//当前总行数
                            if(line>=pageline)
                            {
                                this.Visible = true;
                                this.BringToFront();
                            }
                            else
                            {
                                this.Visible = false;
                            }

                        }
                      
                    }
                }
                else
                {
                    this.Visible = false;
                }

            }
            else
            { 
                HWin32.Scrollinfo si = new HWin32.Scrollinfo();
                si.cbSize = (uint)Marshal.SizeOf(si);
                si.fMask = (int)(HWin32.ScrollInfoMask.SIF_DISABLENOSCROLL | HWin32.ScrollInfoMask.SIF_ALL);
                HWin32.GetScrollInfo(bindControl.Handle, (int)HWin32.ScrollBarDirection.SB_VERT, ref si);
               
                this.visibleValue = (int)si.nPage-1;
                this.Maximum = si.nMax;
                this.Minimum = si.nMin;
                this.Value = si.nPos;
                if(si.nPage==0 || visibleValue >= Maximum)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Visible = true;
                    this.BringToFront();
                }
                //Console.WriteLine(string.Format("nPage:{0};nMax:{1};nMin:{2};nPose:{3};Max:{4};Min:{5};VisValue:{6};value:{7}", si.nPage, si.nMax, si.nMin, si.nPos,Maximum,Minimun,visibleValue,value));
            }
        }

        /// <summary>设置 controlNum。</summary>
        private void SetControlNum()
        {
            if (bindControl == null || bindControl.IsDisposed)
                return;
            if (bindControl is DataGridView)
            {
                DataGridView d = (bindControl as DataGridView);
                if(d.Rows.Count>0&&d.Rows.Count>=this.Value+1)
                {
                    if (d.Rows.Count > 0)
                        d.FirstDisplayedScrollingRowIndex = this.Value;
                }
                
            }
            else
            {
                HWin32.Scrollinfo si = new HWin32.Scrollinfo();
                si.cbSize = (uint)Marshal.SizeOf(si);
                si.fMask = (int)(HWin32.ScrollInfoMask.SIF_DISABLENOSCROLL | HWin32.ScrollInfoMask.SIF_ALL);
                HWin32.GetScrollInfo(bindControl.Handle, (int)HWin32.ScrollBarDirection.SB_VERT, ref si);
                si.nPos = this.Value;
                HWin32.SetScrollInfo(bindControl.Handle, (int)HWin32.ScrollBarDirection.SB_VERT, ref si, true);
                IntPtr aa = new IntPtr(HWin32.MakeLong((short)HWin32.SB_THUMBTRACK, (short)(si.nPos)));
                HWin32.SendMessage(bindControl.Handle, HWin32.WM_VSCROLL, aa, new IntPtr());
            }
                
        }

        /// <summary>ResetDataGridViewScrollBarNum 方法。</summary>
        private void ResetDataGridViewScrollBarNum()
        {
            DataGridView d = (bindControl as DataGridView);
            if (d.Rows.Count > 0)
            {
                int pageRowCount = 0;
                int pageHeight = 0;
                pageHeight += d.ColumnHeadersHeight;
                for (int i = d.FirstDisplayedScrollingRowIndex; i < d.RowCount; i++)
                {
                    if (i == -1)
                        continue;
                    if (pageHeight >= d.Height)
                    { break; }
                    pageRowCount += 1;
                    pageHeight += d.Rows[i].Height;
                }

                this.Maximum = d.Rows.Count + 1;// (d.AllowUserToAddRows ? 1 : 0);
                this.Minimum = 0;
                this.visibleValue = pageRowCount;
                this.Value = d.FirstDisplayedScrollingRowIndex;
            }
            else
            {
                this.visibleValue = 0;
            }

            if(this.visibleValue==0)
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
            }
        }
    }
}