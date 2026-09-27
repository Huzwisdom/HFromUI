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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HFrom.Base
{
    using HFromUI.HData.Win;
    public partial class HAnchorBase : Form
    {
        #region 属性
        [HDescriptionLanguage("圆角")]
        /// <summary>Radius 成员。</summary>
        /// <summary>Radius 字段。</summary>
        [DefaultValue(5)]
        public int Radius { get; set; } 

        [HDescriptionLanguage("是否显示边框")]
        /// <summary>ShowBorder 成员。</summary>
        /// <summary>ShowBorder 字段。</summary>
        [DefaultValue(true)]
        public bool ShowBorder { get; set; }

        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [HDescriptionLanguage("边框颜色")]
        public Color BorderColor { get; set; } = Color.LightGray;

        [HDescriptionLanguage("边框厚度")]
        /// <summary>BorderThickness 成员。</summary>
        /// <summary>BorderThickness 字段。</summary>
        [DefaultValue(1)]
        public int BorderThickness { get; set; } = 1;

        [HDescriptionLanguage("绑定的控件")]
        public Control BaseControl;

        /// <summary>ShowPosition 成员。</summary>
        /// <summary>ShowPosition 字段。</summary>
        [HDescriptionLanguage("默认显示的位置，当未绑定控件时生效")]
        public Point ShowPosition { get; set; }=new Point(0,0);
        #endregion

        /// <summary>checkActiveTimer 字段。</summary>
        private System.Windows.Forms.Timer checkActiveTimer = new System.Windows.Forms.Timer();

        

        /// <summary>
        /// 停靠窗体，下拉框，tip等基类窗体
        /// </summary>
        public HAnchorBase()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            //this.HandleDestroyed += FrmAnchorBase_HandleDestroyed;
            //this.HandleCreated += FrmAnchorBase_HandleCreated;
            this.ShowInTaskbar = false;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            checkActiveTimer.Tick += CheckActiveTimer_Tick;
            checkActiveTimer.Interval = 50;
            
        }


        /// <summary>CheckActiveTimer_Tick 方法。</summary>
        private void CheckActiveTimer_Tick(object sender, EventArgs e)
        {
            if(!DesignMode)
            {
                if (this.Owner != null)
                {
                    Form frm = this.Owner as Form;
                    IntPtr _ptr = HWin32.GetForegroundWindow();
                    if (_ptr != frm.Handle && _ptr != this.Handle)
                    {
                        this.Close();
                    }
                }
                else
                {
                    IntPtr intPtr = HWin32.GetForegroundWindow();
                    if (intPtr != this.Handle)
                    {
                        this.Close();
                    }
                }
            }

        }


        //private void FrmAnchorBase_HandleCreated(object sender, EventArgs e)
        //{
        //    if(!DesignMode)
        //        Application.AddMessageFilter(this);
        //}

        //private void FrmAnchorBase_HandleDestroyed(object sender, EventArgs e)
        //{
        //    if (!DesignMode)
        //        Application.RemoveMessageFilter(this);
        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);
            HDrawPaint.SetGraphicsHighQuality(g);

            if(ShowBorder)
            {
                GraphicsPath borderpath = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), Radius + 1);
                var borderPen = new Pen(this.BorderColor, BorderThickness);
                g.DrawPath(borderPen, borderpath);
                borderPen.Dispose();
                borderpath.Dispose();
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            HWin32.SetFormRoundRectRgn(this, Radius);
        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            //checkActiveTimer.Enabled = this.Visible;
            if(this.Visible&&!DesignMode)
            {
                checkActiveTimer.Enabled = true;
                this.Focus();
                this.BringToFront();
                HWin32.SetFormRoundRectRgn(this, Radius);
                SetDropLocation();

                var baseControlY = 0;
                if(BaseControl!=null)
                {
                    baseControlY = BaseControl.PointToScreen(new Point(0, 0)).Y;
                }
                else
                {
                    baseControlY = ShowPosition.Y;
                }
                if(this.Location.Y<baseControlY)
                {
                    //向上滑出
                    HWin32.AnimateWindow(this.Handle, 100, HWin32.AW_SLIDE + HWin32.AW_VER_NEGATIVE);
                }
                else
                {
                    //向下滑出
                    HWin32.AnimateWindow(this.Handle,100, HWin32.AW_SLIDE + HWin32.AW_VER_POSITIVE);
                }
            }
        }

        /// <summary>响应 Closing 事件。</summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            checkActiveTimer.Enabled = false;
            base.OnClosing(e);
        }

        /// <summary>
        /// 设置停靠窗体显示的位置
        /// </summary>
        private void SetDropLocation()
        {
            if(BaseControl!=null)
            {
                var screenPoint = BaseControl.PointToScreen(new Point(0, 0));
                var DropPoint = new Point(screenPoint.X + 1, screenPoint.Y + BaseControl.Height+2);
                Rectangle screenRect = Screen.FromControl(BaseControl).WorkingArea;
                if (screenRect.Bottom - DropPoint.Y < this.Height)
                {
                    DropPoint = new Point(screenPoint.X + 1, screenPoint.Y - this.Height-2);
                }
                this.Location = DropPoint;
            }
            else
            {
                this.Location = ShowPosition;
            }
        }


    }
}
