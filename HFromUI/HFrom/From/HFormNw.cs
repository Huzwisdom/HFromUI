using HFromUI.HData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HData.Win;
    public partial class HFormNw : Form
    {
        #region 属性
        /// <summary>
        /// 是否开启阴影
        /// </summary>
        protected bool IsOpenShadow { get; set; } = true;
        /// <summary>
        /// 是否窗体拉伸
        /// </summary>
        protected bool IsWindowsStretch { get; set; } = true;

        /// <summary>
        /// 启用鼠标拖动窗口停靠
        /// </summary>
        public bool EnableScreenDock { get; set; } = true;
        #endregion

        #region 变量
        /// <summary>
        /// 是否是拖动窗口时鼠标按下
        /// </summary>
        private bool formMoveMouseDown = false;

        /// <summary>
        /// 拖动窗体时记录鼠标按下的位置
        /// </summary>
        private Point moveMouseDownPoint = new Point(0, 0);

        /// <summary>lastClientRect 字段。</summary>
        private Rectangle lastClientRect=new Rectangle(0,0,400,300);
        #endregion
        public HFormNw()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
        }

        #region 窗体阴影

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;                     // 盒影变量
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;          // 箱形阴影
        private const int WM_ACTIVATEAPP = 0x001C;

        private struct MARGINS                           // 箱形阴影结构
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int WM_NCHITTEST = 0x84;          // 拖动窗体的变量
        private const int HTCLIENT = 0x1;

        /// <summary>
        /// 检查Aero是否可用
        /// </summary>
        /// <returns></returns>
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        #endregion

        #region 窗体拉伸
        const int Guying_HTLEFT = 10;
        const int Guying_HTRIGHT = 11;
        const int Guying_HTTOP = 12;
        const int Guying_HTTOPLEFT = 13;
        const int Guying_HTTOPRIGHT = 14;
        const int Guying_HTBOTTOM = 15;
        const int Guying_HTBOTTOMLEFT = 0x10;
        const int Guying_HTBOTTOMRIGHT = 17;
        const int WM_STRETCH = 0x0084;//窗体拉伸
        const int WM_LEFTDOWN = 0x0201;//鼠标左键按下的消息
        #endregion

        #region 最小化 最大化
        Point? _currLocation = null;
        /// <summary>
        /// 最大化窗体
        /// </summary>
        protected void MaxForm()
        {
            if (this.Cursor != Cursors.Default)
                return;

            this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width,
               Screen.PrimaryScreen.WorkingArea.Height);
            if (this.WindowState != FormWindowState.Maximized)
            {
                _currLocation = this.Location;
                this.Location = new Point(0, 0);
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                if (_currLocation != null)
                {
                    this.Location = (Point)_currLocation;
                }
                this.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// 最小化窗体
        /// </summary>
        protected void MinForm()
        {
            if (this.Cursor == Cursors.Default)
                this.WindowState = FormWindowState.Minimized;
        }

        #endregion


        //创建参数
        protected override CreateParams CreateParams
        {
            get
            {
                //允许最小化操作
                const int WS_MINIMIZEBOX = 0x00020000;  // Winuser.h中定义   
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX;   // 允许最小化操作 

                //窗体阴影
                m_aeroEnabled = CheckAeroEnabled();
                if (!m_aeroEnabled && IsOpenShadow)
                    cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // 箱形阴影
                    if (m_aeroEnabled && IsOpenShadow)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    base.WndProc(ref m);
                    break;
                case WM_STRETCH:                        //窗体拉伸
                    base.WndProc(ref m);
                    if (IsWindowsStretch)
                    {
                        Point vPoint = new Point((int)m.LParam & 0xFFFF,
                        (int)m.LParam >> 16 & 0xFFFF);
                        vPoint = PointToClient(vPoint);
                        if (vPoint.X <= 5)
                            if (vPoint.Y <= 5)
                                m.Result = (IntPtr)Guying_HTTOPLEFT;
                            else if (vPoint.Y >= ClientSize.Height - 5)
                                m.Result = (IntPtr)Guying_HTBOTTOMLEFT;
                            else m.Result = (IntPtr)Guying_HTLEFT;
                        else if (vPoint.X >= ClientSize.Width - 5)
                            if (vPoint.Y <= 5)
                                m.Result = (IntPtr)Guying_HTTOPRIGHT;
                            else if (vPoint.Y >= ClientSize.Height - 5)
                                m.Result = (IntPtr)Guying_HTBOTTOMRIGHT;
                            else m.Result = (IntPtr)Guying_HTRIGHT;
                        else if (vPoint.Y <= 5)
                            m.Result = (IntPtr)Guying_HTTOP;
                        else if (vPoint.Y >= ClientSize.Height - 5)
                            m.Result = (IntPtr)Guying_HTBOTTOM;
                    }
                    break;
                //case WM_LEFTDOWN: //鼠标左键按下的消息
                //    //if (IsWindowsStretch)
                //    //{
                //    //    m.Msg = 0x00A1; //更改消息为非客户区按下鼠标
                //    //    m.LParam = IntPtr.Zero; //默认值
                //    //    m.WParam = new IntPtr(2);//鼠标放在标题栏内
                //    //}
                //    base.WndProc(ref m);
                //    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.Button==MouseButtons.Left&&e.Clicks==1)
            {
                Rectangle bodyRect = new Rectangle(2, 2, this.Width - 4, this.Height - 4);
                Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
                if (bodyRect.Contains(e.X, e.Y))
                {
                    formMoveMouseDown = true;
                    moveMouseDownPoint = PointToScreen(e.Location);
                    if (!((this.Width == screenRect.Width || this.Width == screenRect.Width / 2) && this.Height == screenRect.Height))
                    {
                        HWin32.WindowMove(this.Handle);
                    }
                }
            }
                
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            formMoveMouseDown = false;
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
            if (EnableScreenDock)//若启用屏幕停靠
            {
                if (formMoveMouseDown)//表示此时鼠标按下且在进行拖动操作
                {
                    var p = PointToScreen(e.Location);
                    System.Diagnostics.Debug.WriteLine($"{p.X},{p.Y}");
                    if (PointToScreen(e.Location).Y <= 10 && !(this.Width == screenRect.Width && this.Height == screenRect.Height))//正常大小时向顶部边缘拖动
                    {
                        this.DesktopBounds = new Rectangle(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height);
                        //formMoveMouseDown = false;
                    }
                    else if (PointToScreen(e.Location).X <= 0 && !(this.Width == screenRect.Width && this.Height == screenRect.Height))//正常大小时向左侧边缘拖动
                    {
                        this.DesktopBounds = new Rectangle(screenRect.Left, screenRect.Top, screenRect.Width / 2, screenRect.Height);
                        //formMoveMouseDown = false;
                    }
                    else if (PointToScreen(e.Location).X >= screenRect.Left + screenRect.Width - 1 && !(this.Width == screenRect.Width && this.Height == screenRect.Height))//正常大小时向右侧边缘拖动
                    {
                        this.DesktopBounds = new Rectangle(screenRect.Left + screenRect.Width / 2, screenRect.Top, screenRect.Width / 2, screenRect.Height);
                        //formMoveMouseDown = false;
                    }
                    else if (PointToScreen(e.Location).Y - moveMouseDownPoint.Y >= 1 && ((this.Width == screenRect.Width && this.Height == screenRect.Height) || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))//最大化或左侧最大化或右侧最大化时向下拖恢复正常大小
                    {
                        this.DesktopBounds = new Rectangle(PointToScreen(e.Location).X - lastClientRect.Width / 2, 2, lastClientRect.Width, lastClientRect.Height);
                        HWin32.mouse_event(HWin32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);//模拟鼠标按下
                    }
                    formMoveMouseDown = false;
                }
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
            if (!((this.Width == screenRect.Width && this.Height == screenRect.Height) || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            {
                lastClientRect.Size = this.Size;
            }
        }

        /// <summary>响应 LocationChanged 事件。</summary>
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
            if (!((this.Width == screenRect.Width && this.Height == screenRect.Height) || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            {
                lastClientRect.Location = this.Location;
            }
        }


    }
}
