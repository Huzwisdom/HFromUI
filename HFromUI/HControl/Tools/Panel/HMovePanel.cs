using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Panel
{
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 该控件用于放置可由用户自己设置大小和位置的控件，继承自Panel,实现了IControlsMove接口
    /// 要使该容器内的控件可移动和改变，需要在程序加载时调用该控件的InitMouseAndContolStyle(XmlDocument XmlDoc)方法
    /// </summary>
    public partial class HMovePanel : Panel
    {


        /// <summary>
        /// 光标状态
        /// </summary>
        private enum EnumMousePointPosition
        {
            /// <summary>
            /// 无
            /// </summary>
            MouseSizeNone = 0, //'无

            /// <summary>
            /// 拉伸右边框
            /// </summary>
            MouseSizeRight = 1, //'拉伸右边框

            /// <summary>
            /// 拉伸左边框
            /// </summary>
            MouseSizeLeft = 2, //'拉伸左边框

            /// <summary>
            /// 拉伸下边框
            /// </summary>
            MouseSizeBottom = 3, //'拉伸下边框

            /// <summary>
            /// 拉伸上边框
            /// </summary>
            MouseSizeTop = 4, //'拉伸上边框

            /// <summary>
            /// 拉伸左上角
            /// </summary>
            MouseSizeTopLeft = 5, //'拉伸左上角

            /// <summary>
            /// 拉伸右上角
            /// </summary>
            MouseSizeTopRight = 6, //'拉伸右上角

            /// <summary>
            /// 拉伸左下角
            /// </summary>
            MouseSizeBottomLeft = 7, //'拉伸左下角

            /// <summary>
            /// 拉伸右下角
            /// </summary>
            MouseSizeBottomRight = 8, //'拉伸右下角

            /// <summary>
            /// 鼠标拖动
            /// </summary>
            MouseDrag = 9   // '鼠标拖动
        }

        #region 属性
        private const int Band = 5;
        private const int MinWidth = 10;
        private const int MinHeight = 10;
        private EnumMousePointPosition m_MousePointPosition;
        private Point p, p1;
        private Size BaseSize;
        /// <summary>resolutionX 字段。</summary>
        private int resolutionX = 1920;
        /// <summary>resolutionY 字段。</summary>
        private int resolutionY = 1080;
        /// <summary>moveLock 字段。</summary>
        private bool moveLock = false;

        [HDescriptionLanguage("控件宽高比的宽"), Browsable(true)]
        public int ResolutionX
        {
            get { return resolutionX; }
            set
            {
                resolutionX = value;
            }
        }

        [HDescriptionLanguage("控件宽高比的高"), Browsable(true)]
        public int ResolutionY
        {
            get { return resolutionY; }
            set
            {
                resolutionY = value;
            }
        }

        /// <summary>
        /// 是否允许拖动修改大小
        /// </summary>
        [HDescriptionLanguage("是否允许拖动修改大小")]
        public bool MoveLock
        {
            get { return moveLock; }
            set
            {
                moveLock = value;
            }
        }

        /// <summary>
        /// 定义子控件锁定的字符串，子控件的Tag属性是此字符串时不可移动
        /// </summary>
        /// <summary>ChildLockStr 成员。</summary>
        /// <summary>ChildLockStr 字段。</summary>
        [HDescriptionLanguage("定义子控件锁定的字符串，子控件的Tag属性是此字符串时不可移动")]
        public string ChildLockStr { get; set; } = "MoveLock";

        #endregion 属性

        public HMovePanel()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.BackColor = DefaultBackColor;
            BaseSize = base.Size;
        }

        #region 改变控件大小和移动位置用到的方法

        /// <summary>MyMouseDown 方法。</summary>
        private void MyMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            p.X = e.X;
            p.Y = e.Y;
            p1.X = e.X;
            p1.Y = e.Y;
            if (!this.MoveLock)
            {
                var c = (sender as Control);
                if (c.Tag==null||c.Tag.ToString()!= ChildLockStr)
                    c.BringToFront();
            }
        }

        /// <summary>MyMouseUp 方法。</summary>
        private void MyMouseUp(object sender, MouseEventArgs e)
        {
            if (this.MoveLock)
                return;
            Control lCtrl = sender as Control;
            if (lCtrl.Tag == null || lCtrl.Tag.ToString() != ChildLockStr)
            {
                lCtrl.Left = (int)(XToRatio(lCtrl.Left) * base.Width / resolutionX);
                lCtrl.Top = (int)(YToRatio(lCtrl.Top) * base.Height / resolutionY);
                int width = lCtrl.Left + (int)(XToRatio(lCtrl.Width) * base.Width / resolutionX) > base.Width ? (int)((XToRatio(lCtrl.Width) - 1) * base.Width / resolutionX) : (int)(XToRatio(lCtrl.Width) * base.Width / resolutionX);
                int height = lCtrl.Top + (int)(YToRatio(lCtrl.Height) * base.Height / resolutionY) > base.Height ? (int)((YToRatio(lCtrl.Height) - 1) * base.Height / resolutionY) : (int)(YToRatio(lCtrl.Height) * base.Height / resolutionY);
                if (lCtrl.Left + width > base.Width)
                {
                    lCtrl.Width = base.Width - lCtrl.Left;
                }
                else
                {
                    lCtrl.Width = width;
                }
                if (lCtrl.Top + height > base.Height)
                {
                    lCtrl.Height = base.Height - lCtrl.Top;
                }
                else
                {
                    lCtrl.Height = height;
                }
            }
                
        }

        /// <summary>
        /// 鼠标离开事件需要改进
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMouseLeave(object sender, EventArgs e)
        {
            m_MousePointPosition = EnumMousePointPosition.MouseSizeNone;
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>MousePointPosition 方法。</summary>
        private EnumMousePointPosition MousePointPosition(Size size, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.X >= -1 * Band) | (e.X <= size.Width) | (e.Y >= -1 * Band) | (e.Y <= size.Height))
            {
                if (e.X < Band)
                {
                    if (e.Y < Band) { return EnumMousePointPosition.MouseSizeTopLeft; }
                    else
                    {
                        if (e.Y > -1 * Band + size.Height)
                        { return EnumMousePointPosition.MouseSizeBottomLeft; }
                        else
                        { return EnumMousePointPosition.MouseSizeLeft; }
                    }
                }
                else
                {
                    if (e.X > -1 * Band + size.Width)
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTopRight; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottomRight; }
                            else
                            { return EnumMousePointPosition.MouseSizeRight; }
                        }
                    }
                    else
                    {
                        if (e.Y < Band)
                        { return EnumMousePointPosition.MouseSizeTop; }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return EnumMousePointPosition.MouseSizeBottom; }
                            else
                            { return EnumMousePointPosition.MouseDrag; }
                        }
                    }
                }
            }
            else
            { return EnumMousePointPosition.MouseSizeNone; }
        }

        /// <summary>
        /// 子控件移动和修改大小方法，添加了限制范围，不能超出本控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (moveLock)
                return;
            Control lCtrl = (sender as Control);
            if (lCtrl.Tag == null || lCtrl.Tag.ToString() != ChildLockStr)
            {
                if (e.Button == MouseButtons.Left)
                {
                    switch (m_MousePointPosition)
                    {
                        case EnumMousePointPosition.MouseDrag:
                            if (lCtrl.Left + e.X - p.X < 0 || (lCtrl.Top + e.Y - p.Y) < 0 || lCtrl.Left + e.X - p.X + lCtrl.Width > base.Width || lCtrl.Top + e.Y - p.Y + lCtrl.Height > base.Height)
                            {
                                CheckBorder(lCtrl, e);
                            }
                            else
                            {
                                lCtrl.Left = lCtrl.Left + e.X - p.X;
                                lCtrl.Top = lCtrl.Top + e.Y - p.Y;
                            }
                            CheckMagnetBorder(lCtrl);

                            break;

                        case EnumMousePointPosition.MouseSizeBottom:
                            if (lCtrl.Top + lCtrl.Height + e.Y - p1.Y > base.Height)
                            {
                                lCtrl.Height = base.Height - lCtrl.Top;
                            }
                            else
                            {
                                lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                            }
                            p1.X = e.X;
                            p1.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeBottomRight:
                            if (lCtrl.Top + lCtrl.Height + e.Y - p1.Y > base.Height || lCtrl.Left + lCtrl.Width + e.X - p1.X > base.Width)
                            {
                                if (lCtrl.Top + lCtrl.Height + e.Y - p1.Y > base.Height)
                                {
                                    lCtrl.Height = base.Height - lCtrl.Top;
                                }
                                else
                                {
                                    lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                                }

                                if (lCtrl.Left + lCtrl.Width + e.X - p1.X > base.Width)
                                {
                                    lCtrl.Width = base.Width - lCtrl.Left;
                                }
                                else
                                {
                                    lCtrl.Width = lCtrl.Width + e.X - p1.X;
                                }
                            }
                            else
                            {
                                lCtrl.Width = lCtrl.Width + e.X - p1.X;
                                lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                            }

                            p1.X = e.X;
                            p1.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeRight:

                            if (lCtrl.Left + lCtrl.Width + e.X - p1.X > base.Width)
                            {
                                lCtrl.Width = base.Width - lCtrl.Left;
                            }
                            else
                            {
                                lCtrl.Width = lCtrl.Width + e.X - p1.X;
                            }

                            // lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                            p1.X = e.X;
                            p1.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeTop:
                            if (lCtrl.Top + e.Y - p.Y < 0)
                            {
                                lCtrl.Top = 0;
                            }
                            else
                            {
                                if (lCtrl.Height - (e.Y - p.Y) >= lCtrl.MinimumSize.Height)
                                {
                                    lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                                    lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                                }
                            }
                            break;

                        case EnumMousePointPosition.MouseSizeLeft:
                            if (lCtrl.Left + e.X - p.X < 0)
                            {
                                lCtrl.Left = 0;
                            }
                            else
                            {
                                if (lCtrl.Width - (e.X - p.X) >= lCtrl.MinimumSize.Width)
                                {
                                    lCtrl.Left = lCtrl.Left + e.X - p.X;
                                    lCtrl.Width = lCtrl.Width - (e.X - p.X);
                                }
                            }
                            break;

                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            if (lCtrl.Left + e.X - p.X < 0 || lCtrl.Top + lCtrl.Height + e.Y - p1.Y > base.Height)
                            {
                                if (lCtrl.Left + e.X - p.X < 0)
                                {
                                    lCtrl.Left = 0;
                                }
                                else
                                {
                                    lCtrl.Left = lCtrl.Left + e.X - p.X;
                                    lCtrl.Width = lCtrl.Width - (e.X - p.X);
                                }

                                if (lCtrl.Top + lCtrl.Height + e.Y - p1.Y > base.Height)
                                {
                                    lCtrl.Height = base.Height - lCtrl.Top;
                                }
                                else
                                {
                                    lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                                }
                            }
                            else
                            {
                                lCtrl.Left = lCtrl.Left + e.X - p.X;
                                lCtrl.Width = lCtrl.Width - (e.X - p.X);
                                lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                            }
                            p1.X = e.X;
                            p1.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeTopRight:
                            if (lCtrl.Top + e.Y - p.Y < 0 || lCtrl.Left + lCtrl.Width + e.X - p1.X > base.Width)
                            {
                                if (lCtrl.Top + e.Y - p.Y < 0)
                                {
                                    lCtrl.Top = 0;
                                }
                                else
                                {
                                    lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                                    lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                                }

                                if (lCtrl.Left + lCtrl.Width + e.X - p1.X > base.Width)
                                {
                                    lCtrl.Width = base.Width - lCtrl.Left;
                                }
                                else
                                {
                                    lCtrl.Width = lCtrl.Width + e.X - p1.X;
                                }
                            }
                            else
                            {
                                lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                                lCtrl.Width = lCtrl.Width + (e.X - p1.X);
                                lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                            }
                            p1.X = e.X;
                            p1.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeTopLeft:
                            if (lCtrl.Top + e.Y - p.Y < 0 || lCtrl.Left + e.X - p.X < 0)
                            {
                                if (lCtrl.Top + e.Y - p.Y < 0)
                                {
                                    lCtrl.Top = 0;
                                }
                                else
                                {
                                    lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                                    lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                                }
                                if (lCtrl.Left + e.X - p.X < 0)
                                {
                                    lCtrl.Left = 0;
                                }
                                else
                                {
                                    lCtrl.Left = lCtrl.Left + e.X - p.X;
                                    lCtrl.Width = lCtrl.Width - (e.X - p.X);
                                }
                            }
                            else
                            {
                                lCtrl.Left = lCtrl.Left + e.X - p.X;
                                lCtrl.Top = lCtrl.Top + (e.Y - p.Y);
                                lCtrl.Width = lCtrl.Width - (e.X - p.X);
                                lCtrl.Height = lCtrl.Height - (e.Y - p.Y);
                            }

                            break;

                        default:
                            break;
                    }
                    if (lCtrl.Width < MinWidth) lCtrl.Width = MinWidth;
                    if (lCtrl.Height < MinHeight) lCtrl.Height = MinHeight;
                }
                else
                {
                    m_MousePointPosition = MousePointPosition(lCtrl.Size, e);   //'判断光标的位置状态
                    switch (m_MousePointPosition)   //'改变光标
                    {
                        case EnumMousePointPosition.MouseSizeNone:
                            this.Cursor = Cursors.Arrow;        //'箭头
                            break;

                        case EnumMousePointPosition.MouseDrag:
                            this.Cursor = Cursors.SizeAll;      //'四方向
                            break;

                        case EnumMousePointPosition.MouseSizeBottom:
                            this.Cursor = Cursors.SizeNS;       //'南北
                            break;

                        case EnumMousePointPosition.MouseSizeTop:
                            this.Cursor = Cursors.SizeNS;       //'南北
                            break;

                        case EnumMousePointPosition.MouseSizeLeft:
                            this.Cursor = Cursors.SizeWE;       //'东西
                            break;

                        case EnumMousePointPosition.MouseSizeRight:
                            this.Cursor = Cursors.SizeWE;       //'东西
                            break;

                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            this.Cursor = Cursors.SizeNESW;     //'东北到南西
                            break;

                        case EnumMousePointPosition.MouseSizeBottomRight:
                            this.Cursor = Cursors.SizeNWSE;     //'东南到西北
                            break;

                        case EnumMousePointPosition.MouseSizeTopLeft:
                            this.Cursor = Cursors.SizeNWSE;     //'东南到西北
                            break;

                        case EnumMousePointPosition.MouseSizeTopRight:
                            this.Cursor = Cursors.SizeNESW;     //'东北到南西
                            break;

                        default:
                            break;
                    }
                }
            }
                
        }

        /// <summary>
        /// 移动控件时判断不能出边界
        /// </summary>
        /// <param name="lCtrl"></param>
        /// <param name="e"></param>
        private void CheckBorder(Control lCtrl, System.Windows.Forms.MouseEventArgs e)
        {
            //lCtrl.Left = (int)(XToRatio(lCtrl.Left) * base.Width / 100);
            //lCtrl.Top = (int)(YToRatio(lCtrl.Top) * base.Height / 100);
            // lCtrl.Width = lCtrl.Left + (int)(XToRatio(lCtrl.Width) * base.Width / resolutionX) > base.Width ? (int)((XToRatio(lCtrl.Width) - 1) * base.Width / resolutionX) : (int)(XToRatio(lCtrl.Width) * base.Width / resolutionX);
            // lCtrl.Height = lCtrl.Top + (int)(YToRatio(lCtrl.Height) * base.Height / resolutionY) > base.Height ? (int)((YToRatio(lCtrl.Height) - 1) * base.Height / resolutionY) : (int)(YToRatio(lCtrl.Height) * base.Height / resolutionY);
            if (lCtrl.Left + e.X - p.X < 0)
            {
                lCtrl.Left = 0;
                if ((lCtrl.Top + e.Y - p.Y) < 0)
                {
                    lCtrl.Top = 0;
                }
                else if ((lCtrl.Top + e.Y - p.Y + lCtrl.Height) > base.Height)
                {
                    lCtrl.Top = base.Height - lCtrl.Height;
                }
                else
                {
                    lCtrl.Top = lCtrl.Top + e.Y - p.Y;
                }
            }
            else if (lCtrl.Left + e.X - p.X + lCtrl.Width > base.Width)
            {
                lCtrl.Left = base.Width - lCtrl.Width;
                if ((lCtrl.Top + e.Y - p.Y) < 0)
                {
                    lCtrl.Top = 0;
                }
                else if ((lCtrl.Top + e.Y - p.Y + lCtrl.Height) > base.Height)
                {
                    lCtrl.Top = base.Height - lCtrl.Height;
                }
                else
                {
                    lCtrl.Top = lCtrl.Top + e.Y - p.Y;
                }
            }

            if (lCtrl.Top + e.Y - p.Y < 0)
            {
                lCtrl.Top = 0;
                if (lCtrl.Left + e.X - p.X < 0)
                {
                    lCtrl.Left = 0;
                }
                else if (lCtrl.Left + e.X - p.X + lCtrl.Width > base.Width)
                {
                    lCtrl.Left = base.Width - lCtrl.Width;
                }
                else
                {
                    lCtrl.Left = lCtrl.Left + e.X - p.X;
                }
            }
            else if (lCtrl.Top + e.Y - p.Y + lCtrl.Height > base.Height)
            {
                lCtrl.Top = base.Height - lCtrl.Height;
                if (lCtrl.Left + e.X - p.X < 0)
                {
                    lCtrl.Left = 0;
                }
                else if (lCtrl.Left + e.X - p.X + lCtrl.Width > base.Width)
                {
                    lCtrl.Left = base.Width - lCtrl.Width;
                }
                else
                {
                    lCtrl.Left = lCtrl.Left + e.X - p.X;
                }
            }
        }

        /// <summary>
        /// 移动控件时判断是否靠近边界，靠近就贴上
        /// </summary>
        /// <param name="lCtrl"></param>
        /// <param name="e"></param>
        private void CheckMagnetBorder(Control lCtrl)
        {
            //lCtrl.Left = (int)(XToRatio(lCtrl.Left) * base.Width / 100);
            //lCtrl.Top = (int)(YToRatio(lCtrl.Top) * base.Height / 100);
            //lCtrl.Width = (int)(XToRatio(lCtrl.Width) * base.Width / 100);
            //lCtrl.Height = (int)(YToRatio(lCtrl.Height) * base.Height / 100);

            int pixel = 5;
            if (lCtrl.Left <= pixel)
            {
                lCtrl.Left = 0;
            }
            if (lCtrl.Top <= pixel)
            {
                lCtrl.Top = 0;
            }

            if (base.Width - lCtrl.Left - lCtrl.Width <= pixel)
            {
                lCtrl.Left = base.Width - lCtrl.Width;
            }

            if (base.Height - lCtrl.Top - lCtrl.Height <= pixel)
            {
                lCtrl.Top = base.Height - lCtrl.Height;
            }

            if (Math.Abs(base.Width / 2 - lCtrl.Left - lCtrl.Width) <= pixel)
            {
                lCtrl.Left = base.Width / 2 - lCtrl.Width;
            }

            if (Math.Abs(lCtrl.Left - base.Width / 2) <= pixel)
            {
                lCtrl.Left = base.Width / 2;
            }

            if (Math.Abs(base.Height / 2 - lCtrl.Top - lCtrl.Height) <= pixel)
            {
                lCtrl.Top = base.Height / 2 - lCtrl.Height;
            }

            if (Math.Abs(lCtrl.Top - base.Height / 2) <= pixel)
            {
                lCtrl.Top = base.Height / 2;
            }
        }

        /// <summary>XToRatio 方法。</summary>
        private int XToRatio(int x)//x坐标转为0~100的等比
        {
            try
            {
                int X = resolutionX * x / this.Width;
                int r = resolutionX * x % this.Width;
                if (r > base.Width / (2 * resolutionX))
                {
                    X = X + 1;
                }
                return X;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>YToRatio 方法。</summary>
        private int YToRatio(int y)
        {
            try
            {
                int Y = resolutionY * y / base.Height;
                int r = resolutionY * y % base.Height;
                if (r > base.Height / (2 * resolutionY))
                {
                    Y = Y + 1;
                }
                return Y;
            }
            catch
            {
                return 0;
            }
        }

        #endregion 改变控件大小和移动位置用到的方法

        #region 初始化鼠标事件委托和控件大小和移动
        /// <summary>响应 ControlAdded 事件。</summary>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.MouseDown += new System.Windows.Forms.MouseEventHandler(MyMouseDown);
            e.Control.MouseLeave += new System.EventHandler(MyMouseLeave);
            e.Control.MouseMove += new System.Windows.Forms.MouseEventHandler(MyMouseMove);
            e.Control.MouseUp += new System.Windows.Forms.MouseEventHandler(MyMouseUp);

        }

        /// <summary>响应 ControlRemoved 事件。</summary>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);
            try
            {
                e.Control.Dispose();
            }
            catch
            {
            }
        }

        #endregion 初始化鼠标事件委托和控件大小和移动


        /// <summary>first 字段。</summary>
        private bool first = true;

        /// <summary>响应 Resize 事件。</summary>
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
        }

        /// <summary>
        /// 子控件按比例缩放
        /// </summary>
        /// <summary>RatioSize 方法。</summary>
        /// <summary>RatioSize 方法。</summary>
        [HDescriptionLanguage("子控件按比例缩放大小和位置")]
        public void RatioSize()
        {
            if (base.Width > 0 && base.Height > 0)//若大小为0则不进行等比缩放
            {
                if (!first)
                {
                    int width = base.Width;
                    int height = base.Height;

                    float widthScale = (float)width / BaseSize.Width;
                    float heightScale = (float)height / BaseSize.Height;

                    for (int i = 0; i < this.Controls.Count; i++)//实现子控件的等比缩放
                    {
                        this.Controls[i].Location = new Point((int)(this.Controls[i].Location.X * widthScale), (int)(this.Controls[i].Location.Y * heightScale));
                        this.Controls[i].Size = new Size((int)(this.Controls[i].Width * widthScale), (int)(this.Controls[i].Height * heightScale));
                        Controls[i].Left = (int)(XToRatio(Controls[i].Left) * base.Width / resolutionX);
                        Controls[i].Top = (int)(YToRatio(Controls[i].Top) * base.Height / resolutionY);
                        Controls[i].Width = (int)(XToRatio(Controls[i].Width) * base.Width / resolutionX);
                        Controls[i].Height = (int)(YToRatio(Controls[i].Height) * base.Height / resolutionY);
                        CheckMagnetBorder(Controls[i]);
                    }
                }
                first = false;
                BaseSize = base.Size;
            }
        }
    }
}
