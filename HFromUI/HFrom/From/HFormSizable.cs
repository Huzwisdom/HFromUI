using HFromUI.HAttribute;

using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HColor;
    public partial class HFormSizable : Form
    {
        #region 私有属性
        /// <summary>shadowWidth 字段。</summary>
        private int shadowWidth = 7;
        #endregion

        #region private structs

        private struct _NonClientSizeInfo
        {
            public Size CaptionButtonSize;
            public Size BorderSize;
            public int CaptionHeight;
            public Rectangle CaptionRect;
            public Rectangle Rect;
            public Rectangle ClientRect;
            public int Width;
            public int Height;
        };

        private struct _RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        #endregion private structs

        #region constants

        private const int WM_NCACTIVATE = 0x86;
        private const int WM_NCPAINT = 0x85;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int WM_NCRBUTTONDOWN = 0x00A4;
        private const int WM_NCRBUTTONUP = 0x00A5;
        private const int WM_NCMOUSEMOVE = 0x00A0;
        private const int WM_NCLBUTTONUP = 0x00A2;
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCMOUSEHOVER = 0x02A0;
        private const int WM_NCMOUSELEAVE = 0x02A2;
        private const int WM_NCHITTEST = 0x0084;
        private const int WM_NCCREATE = 0x0081;
        //const int WM_RBUTTONUP = 0x0205;

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_CAPTURECHANGED = 0x0215;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_SETCURSOR = 0x0020;
        private const int WM_CLOSE = 0x0010;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_SIZE = 0x0005;
        private const int WM_SIZING = 0x0214;
        private const int WM_GETMINMAXINFO = 0x0024;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_WINDOWPOSCHANGING = 0x0046;

        // FOR WM_SIZING MSG WPARAM
        private const int WMSZ_BOTTOM = 6;

        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;

        // left mouse button is down.
        private const int MK_LBUTTON = 0x0001;

        private const int SC_CLOSE = 0xF060;
        private const int SC_MAXIMIZE = 0xF030;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_RESTORE = 0xF120;
        private const int SC_CONTEXTHELP = 0xF180;

        private const int HTCAPTION = 2;
        private const int HTCLOSE = 20;
        private const int HTHELP = 21;
        private const int HTMAXBUTTON = 9;
        private const int HTMINBUTTON = 8;
        private const int HTTOP = 12;

        private const int SM_CYBORDER = 6;
        private const int SM_CXBORDER = 5;
        private const int SM_CYCAPTION = 4;

        private const int CS_DropSHADOW = 0x20000;
        private const int GCL_STYLE = (-26);

        #endregion constants

        #region windows api

        [DllImport("User32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, ref _RECT rect);

        [DllImport("User32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);

        #endregion windows api

        [DefaultValue("")]
        [Browsable(true)]
        /// <summary>CaptionContextMenu 成员。</summary>
        /// <summary>CaptionContextMenu 字段。</summary>
        [HCategoryLanguage("控制箱"), HDisplayNameLanguage("标题栏右键菜单"), HDescriptionLanguage("CaptionContextMenu 成员。")]
        public virtual ContextMenuStrip CaptionContextMenu { get; set; }

        /// <summary>响应 CaptionContextMenu 事件。</summary>
        protected virtual void OnCaptionContextMenu(int x, int y)
        {
            if (this.CaptionContextMenu != null)
                this.CaptionContextMenu.Show(x, y);
        }

        #region 公共属性
        public new FormBorderStyle FormBorderStyle
        {
            get { return base.FormBorderStyle; }
            set
            {
                base.FormBorderStyle = FormBorderStyle.Sizable;
            }
        }

        [HCategoryLanguage("_阴影"), HDisplayNameLanguage("阴影颜色"), Browsable(true)]
        /// <summary>ShadowColor 成员。</summary>
        /// <summary>ShadowColor 字段。</summary>
        [HDescriptionLanguage("阴影颜色")]
        public Color ShadowColor { get; set; } = Color.Black;

        [HCategoryLanguage("_阴影"), HDisplayNameLanguage("阴影宽度"), Browsable(true)]
        [HDescriptionLanguage("阴影宽度")]
        public int ShadowWidth
        {
            get { return shadowWidth; }
            set
            {
                if (value < 7)
                    shadowWidth = 7;
                else
                    shadowWidth = value;
            }
        }

        [HCategoryLanguage("_阴影"), HDisplayNameLanguage("显示阴影"), Browsable(true)]
        /// <summary>ShowShadow 成员。</summary>
        /// <summary>ShowShadow 字段。</summary>
        [HDescriptionLanguage("是否显示阴影")]
        public bool ShowShadow { get; set; } = true;

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("关闭按钮图片"), Browsable(true)]
        [HDescriptionLanguage("关闭按钮图片")]
        /// <summary>CloseButtonImage 成员。</summary>
        /// <summary>CloseButtonImage 字段。</summary>
        [DesignOnly(true)]
        public Image CloseButtonImage { get; set; } = HPhoto.Get("FormClose");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("关闭按钮按下图片"), Browsable(true)]
        [HDescriptionLanguage("关闭按钮按下显示的图片")]
        /// <summary>CloseButtonPressDownImage 成员。</summary>
        /// <summary>CloseButtonPressDownImage 字段。</summary>
        [DesignOnly(true)]
        public Image CloseButtonPressDownImage { get; set; } = HPhoto.Get("FormClose");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("关闭按钮悬停图片"), Browsable(true)]
        [HDescriptionLanguage("关闭按钮鼠标移入时显示的图片")]
        /// <summary>CloseButtonHoverImage 成员。</summary>
        /// <summary>CloseButtonHoverImage 字段。</summary>
        [DesignOnly(true)]
        public Image CloseButtonHoverImage { get; set; } = HPhoto.Get("FormCloseIn");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("最大化按钮图片"), Browsable(true)]
        [HDescriptionLanguage("最大化按钮图片")]
        /// <summary>MaximumButtonImage 成员。</summary>
        /// <summary>MaximumButtonImage 字段。</summary>
        [DesignOnly(true)]
        public Image MaximumButtonImage { get; set; } = HPhoto.Get("FormMax");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("最大化按钮悬停图片"), Browsable(true)]
        [HDescriptionLanguage("最大化按钮鼠标移入时显示的图片")]
        /// <summary>MaximumButtonHoverImage 成员。</summary>
        /// <summary>MaximumButtonHoverImage 字段。</summary>
        [DesignOnly(true)]
        public Image MaximumButtonHoverImage { get; set; } = HPhoto.Get("FormMaxIn");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("最大化按钮按下图片"), Browsable(true)]
        [HDescriptionLanguage("最大化按钮按下时显示的图片")]
        /// <summary>MaximumButtonPressDownImage 成员。</summary>
        /// <summary>MaximumButtonPressDownImage 字段。</summary>
        [DesignOnly(true)]
        public Image MaximumButtonPressDownImage { get; set; } = HPhoto.Get("FormMax");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("常规最大化按钮图片"), Browsable(true)]
        [HDescriptionLanguage("最大化按钮还原状态时显示的图片")]
        /// <summary>MaximumNormalButtonImage 成员。</summary>
        /// <summary>MaximumNormalButtonImage 字段。</summary>
        [DesignOnly(true)]
        public Image MaximumNormalButtonImage { get; set; } = HPhoto.Get("FormRestore");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("常规最大化按钮悬停图片"), Browsable(true)]
        [HDescriptionLanguage("最大化按钮还原状态时鼠标移入时显示的图片")]
        /// <summary>MaximumNormalButtonHoverImage 成员。</summary>
        /// <summary>MaximumNormalButtonHoverImage 字段。</summary>
        [DesignOnly(true)]
        public Image MaximumNormalButtonHoverImage { get; set; } = HPhoto.Get("FormRestoreIn");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("常规最大化按钮按下图片"), Browsable(true)]
        [HDescriptionLanguage("最大化按钮还原状态时鼠标按下时显示的图片")]
        /// <summary>MaximumNormalButtonPressDownImage 成员。</summary>
        /// <summary>MaximumNormalButtonPressDownImage 字段。</summary>
        [DesignOnly(true)]
        public Image MaximumNormalButtonPressDownImage { get; set; } = HPhoto.Get("FormRestore");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("最小化按钮图片"), Browsable(true)]
        [HDescriptionLanguage("最小化按钮显示的图片")]
        /// <summary>MinimumButtonImage 成员。</summary>
        /// <summary>MinimumButtonImage 字段。</summary>
        [DesignOnly(true)]
        public Image MinimumButtonImage { get; set; } = HPhoto.Get("FormMin");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("最小化按钮悬停图片"), Browsable(true)]
        [HDescriptionLanguage("最小化按钮鼠标移入时显示的图片")]
        /// <summary>MinimumButtonHoverImage 成员。</summary>
        /// <summary>MinimumButtonHoverImage 字段。</summary>
        [DesignOnly(true)]
        public Image MinimumButtonHoverImage { get; set; } = HPhoto.Get("FormMinIn");

        [HCategoryLanguage("_按钮"), HDisplayNameLanguage("最小化按钮按下图片"), Browsable(true)]
        [HDescriptionLanguage("最小化按钮按下时显示的图片")]
        /// <summary>MinimumButtonPressDownImage 成员。</summary>
        /// <summary>MinimumButtonPressDownImage 字段。</summary>
        [DesignOnly(true)]
        public Image MinimumButtonPressDownImage { get; set; } = HPhoto.Get("FormMin");

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("标题背景颜色"), Browsable(true)]
        [HDescriptionLanguage("标题栏颜色")]
        /// <summary>CaptionBackgroundColor 成员。</summary>
        /// <summary>CaptionBackgroundColor 字段。</summary>
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color CaptionBackgroundColor { get; set; } = Color.DodgerBlue;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("标题起始背景颜色"), Browsable(true)]
        [HDescriptionLanguage("标题栏渐变色起始色")]
        /// <summary>CaptionStartBackgroundColor 成员。</summary>
        /// <summary>CaptionStartBackgroundColor 字段。</summary>
        [DefaultValue(typeof(Color), "DeepSkyBlue")]
        public Color CaptionStartBackgroundColor { get; set; } = Color.DeepSkyBlue;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("边框颜色"), Browsable(true)]
        [HDescriptionLanguage("边框颜色")]
        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [DefaultValue(typeof(Color), "Black")]
        public Color BorderColor { get; set; } = Color.DodgerBlue;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("显示边框"), Browsable(true)]
        /// <summary>ShowBorder 成员。</summary>
        /// <summary>ShowBorder 字段。</summary>
        [HDescriptionLanguage("显示边框")]
        public bool ShowBorder { get; set; } = true;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("圆角半径"), Browsable(true)]
        /// <summary>Radius 成员。</summary>
        /// <summary>Radius 字段。</summary>
        [HDescriptionLanguage("圆角半径")]
        public int Radius { get; set; } = 0;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("图标大小"), Browsable(true)]
        /// <summary>IconSize 成员。</summary>
        /// <summary>IconSize 字段。</summary>
        [HDescriptionLanguage("图标大小")]
        public Size IconSize { get; set; } = SystemInformation.SmallIconSize;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("图标偏移"), Browsable(true)]
        /// <summary>IconOffset 成员。</summary>
        /// <summary>IconOffset 字段。</summary>
        [HDescriptionLanguage("图标位置偏移")]
        public Point IconOffset { get; set; }= new Point(0,0);

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("标题偏移"), Browsable(true)]
        /// <summary>TitleOffset 成员。</summary>
        /// <summary>TitleOffset 字段。</summary>
        [HDescriptionLanguage("标题位置偏移")]
        public Point TitleOffset { get; set; } = new Point(0, 0);

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("显示标题"), Browsable(true)]
        /// <summary>ShowTitle 成员。</summary>
        /// <summary>ShowTitle 字段。</summary>
        [HDescriptionLanguage("是否显示标题栏")]
        public bool ShowTitle { get; set; }=true;

        [HCategoryLanguage("_无焦点"), HDisplayNameLanguage("非活动标题栏背景色"), Browsable(true)]
        [HDescriptionLanguage("窗体无焦点时标题栏颜色")]
        /// <summary>UnActiveCaptionBackgroundColor 成员。</summary>
        /// <summary>UnActiveCaptionBackgroundColor 字段。</summary>
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color UnActiveCaptionBackgroundColor { get; set; } = Color.DeepSkyBlue;

        [HCategoryLanguage("_无焦点"), HDisplayNameLanguage("非活动标题栏起始背景色"), Browsable(true)]
        [HDescriptionLanguage("窗体无焦点时标题栏渐变色的起始色")]
        /// <summary>UnActiveCaptionStartBackgroundColor 成员。</summary>
        /// <summary>UnActiveCaptionStartBackgroundColor 字段。</summary>
        [DefaultValue(typeof(Color), "DeepSkyBlue")]
        public Color UnActiveCaptionStartBackgroundColor { get; set; } = Color.DeepSkyBlue;

        [HCategoryLanguage("_无焦点"), HDisplayNameLanguage("非活动边框颜色"), Browsable(true)]
        [HDescriptionLanguage("窗体无焦点时边框颜色")]
        [DisplayName("CaptionBackgroundColor")]
        /// <summary>UnActiveBorderColor 成员。</summary>
        /// <summary>UnActiveBorderColor 字段。</summary>
        [DefaultValue(typeof(Color), "Black")]
        public Color UnActiveBorderColor { get; set; } = Color.Black;

        [HCategoryLanguage("_无焦点"), HDisplayNameLanguage("非活动时显示边框"), Browsable(true)]
        /// <summary>UnActiveShowBorder 成员。</summary>
        /// <summary>UnActiveShowBorder 字段。</summary>
        [HDescriptionLanguage("窗体无焦点时是否显示边框")]
        public bool UnActiveShowBorder { get; set; } = true;

        [HCategoryLanguage("_特效"), HDisplayNameLanguage("淡入"), Browsable(true)]
        /// <summary>Fade_In 成员。</summary>
        /// <summary>Fade_In 字段。</summary>
        [HDescriptionLanguage("是否启用淡入特效")]
        public bool Fade_In { get; set; } = false;

        [HCategoryLanguage("_特效"), HDisplayNameLanguage("淡入时间"), Browsable(true)]
        /// <summary>Fade_In_Time 成员。</summary>
        /// <summary>Fade_In_Time 字段。</summary>
        [HDescriptionLanguage("淡入特效执行时间(ms)")]
        public uint Fade_In_Time { get; set; } = 500;

        [HCategoryLanguage("_特效"), HDisplayNameLanguage("淡出"), Browsable(true)]
        /// <summary>Fade_Out 成员。</summary>
        /// <summary>Fade_Out 字段。</summary>
        [HDescriptionLanguage("是否启用淡出特效")]
        public bool Fade_Out { get; set; } = false;

        [HCategoryLanguage("_特效"), HDisplayNameLanguage("淡出时间"), Browsable(true)]
        /// <summary>Fade_Out_Time 成员。</summary>
        /// <summary>Fade_Out_Time 字段。</summary>
        [HDescriptionLanguage("淡出特效执行时间")]
        public uint Fade_Out_Time { get; set; } = 500;

        #endregion properties

        public HFormSizable()
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

            //SetClassLong(this.Handle, GCL_STYLE, GetClassLong(this.Handle, GCL_STYLE) |
            //CS_DropSHADOW); //API函数加载，实现窗体边框阴影效果
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;

        //        cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件

        //        return cp;
        //    }
        //}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int b = CaptionBackgroundColor.B + 5 > 255 ? CaptionBackgroundColor.B - 5 : CaptionBackgroundColor.B + 5;
            TransparencyKey = Color.FromArgb(CaptionBackgroundColor.R, CaptionBackgroundColor.G, b);

            #region 淡入

            if (Fade_In)
            {
                this.Opacity = 0;
                HAnimation anime = new HAnimation(this);
                Dictionary<string, float> dic = new Dictionary<string, float>();
                dic.Add("Opacity", 1);
                anime.AnimationType = AnimationType.UniformMotion;
                anime.AnimationControl(dic, (int)Fade_In_Time);
            }

            #endregion 淡入
        }

        HShadowFormSizable shadowForm;

        /// <summary>响应 Closed 事件。</summary>
        protected override void OnClosed(EventArgs e)
        {
            if (shadowForm != null && !shadowForm.IsDisposed)
            {
                shadowForm.Close();
            }

            #region 淡出

            if (Fade_Out)
            {
                HAnimation anime = new HAnimation(this);
                Dictionary<string, float> dic = new Dictionary<string, float>();
                anime.AnimationType = AnimationType.UniformMotion;
                anime.isBlock = true;
                dic.Add("Opacity", 0);
                anime.AnimationControl(dic, (int)Fade_Out_Time);
            }

            #endregion 淡出

            base.OnClosed(e);
        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                //if (showShadow)
                //{
                if (!DesignMode && (shadowForm == null || shadowForm.IsDisposed))
                {
                    shadowForm = new HShadowFormSizable(this);
                    shadowForm.Show(this);
                }
                //}
            }

            base.OnVisibleChanged(e);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Invalidate();
        }

        #region 

        /// <summary>获取 nonClientInfo。</summary>
        private _NonClientSizeInfo GetNonClientInfo(IntPtr hwnd)
        {
            _NonClientSizeInfo info = new _NonClientSizeInfo();
            info.CaptionButtonSize = SystemInformation.CaptionButtonSize;
            info.CaptionHeight = SystemInformation.CaptionHeight;

            switch (base.FormBorderStyle)
            {
                case System.Windows.Forms.FormBorderStyle.Fixed3D:
                    info.BorderSize = SystemInformation.FixedFrameBorderSize;
                    break;

                case System.Windows.Forms.FormBorderStyle.FixedDialog:
                    info.BorderSize = SystemInformation.FixedFrameBorderSize;
                    break;

                case System.Windows.Forms.FormBorderStyle.FixedSingle:
                    info.BorderSize = SystemInformation.FixedFrameBorderSize;
                    break;

                case System.Windows.Forms.FormBorderStyle.FixedToolWindow:
                    info.BorderSize = SystemInformation.FixedFrameBorderSize;
                    info.CaptionButtonSize = SystemInformation.ToolWindowCaptionButtonSize;
                    info.CaptionHeight = SystemInformation.ToolWindowCaptionHeight;
                    break;

                case System.Windows.Forms.FormBorderStyle.Sizable:
                    info.BorderSize = new Size(8, 8);// SystemInformation.FrameBorderSize;
                    //info.BorderSize = SystemInformation.SizingBorderWidth;
                    break;

                case System.Windows.Forms.FormBorderStyle.SizableToolWindow:
                    info.CaptionButtonSize = SystemInformation.ToolWindowCaptionButtonSize;
                    info.BorderSize = SystemInformation.FrameBorderSize;
                    info.CaptionHeight = SystemInformation.ToolWindowCaptionHeight;
                    break;

                default:
                    info.BorderSize = SystemInformation.BorderSize;
                    break;
            }

            _RECT areatRect = new _RECT();
            GetWindowRect(hwnd, ref areatRect);

            int width = areatRect.right - areatRect.left;
            int height = areatRect.bottom - areatRect.top;

            info.Width = width;
            info.Height = height;

            Point xy = new Point(areatRect.left, areatRect.top);
            xy.Offset(-areatRect.left, -areatRect.top);

            info.CaptionRect = new Rectangle(xy.X + info.BorderSize.Width, xy.Y + info.BorderSize.Height, width - 2 * info.BorderSize.Height, info.CaptionHeight);
            info.Rect = new Rectangle(xy.X, xy.Y, width, height);
            info.ClientRect = new Rectangle(xy.X + info.BorderSize.Width - 1,
                xy.Y + info.CaptionHeight + info.BorderSize.Height - 1,
                width - info.BorderSize.Width * 2 + 1,
                height - info.CaptionHeight - info.BorderSize.Height * 2 + 1);

            return info;
        }

        /// <summary>DrawCaption 方法。</summary>
        private bool DrawCaption(Message m, bool active)
        {
            IntPtr hwnd = m.HWnd;
            IntPtr dc;
            Graphics g;
            _NonClientSizeInfo ncInfo;

            dc = GetWindowDC(hwnd);
            ncInfo = GetNonClientInfo(hwnd);
            g = Graphics.FromHdc(dc);

            bool rtn = DrawCaption(g, ncInfo, m, active);

            g.Dispose();
            ReleaseDC(hwnd, dc);
            return rtn;
        }

        /// <summary>
        /// 画标题栏
        /// </summary>
        /// <param name="g"></param>
        /// <param name="ncInfo"></param>
        /// <param name="m"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private bool DrawCaption(Graphics g, _NonClientSizeInfo ncInfo, Message m, bool active)
        {
            bool rtn = false;
            g.SmoothingMode = SmoothingMode.HighQuality;

            //创建背景画布
            Bitmap bitmap = new Bitmap(ncInfo.Width+1, ncInfo.Height+1);
            Graphics bg = Graphics.FromImage(bitmap);
            bg.SmoothingMode = SmoothingMode.HighQuality;
            bg.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Brush transpbrush = new SolidBrush(TransparencyKey);
            Brush forebrush = new SolidBrush(this.ForeColor);

            //画最底层颜色，用以显示透明，与transparencyKey颜色一致
            GraphicsPath graphicsPath = new GraphicsPath();
            var clientRect = new Rectangle(ncInfo.ClientRect.X-2, ncInfo.ClientRect.Y-2, ncInfo.ClientRect.Width+4,ncInfo.ClientRect.Height+4);
            graphicsPath.AddRectangle(clientRect);
            Rectangle outerRect = new Rectangle(ncInfo.Rect.X - 1, ncInfo.Rect.Y - 1, ncInfo.Rect.Width + 2, ncInfo.Rect.Height + 2);
            graphicsPath.AddRectangle(outerRect);
            bg.FillPath(transpbrush, graphicsPath);

            graphicsPath.Reset();

            Rectangle innerRect = new Rectangle(ncInfo.Rect.X + 8, ncInfo.Rect.Y+5 , ncInfo.Rect.Width - 14, ncInfo.Rect.Height - 11);
            graphicsPath = HDrawPaint.CreatePath(innerRect,this.WindowState==FormWindowState.Maximized?0:Radius);
            clientRect= new Rectangle(ncInfo.ClientRect.X+1, ncInfo.ClientRect.Y+1, ncInfo.ClientRect.Width, ncInfo.ClientRect.Height);
            graphicsPath.AddRectangle(clientRect);

            if (active)
            {
                Brush captionbrush = new SolidBrush(CaptionBackgroundColor);
                bg.FillPath(captionbrush, graphicsPath);

                Rectangle captionRect = new Rectangle(innerRect.X, innerRect.Y, innerRect.Width, ncInfo.ClientRect.Top - innerRect.Top);

                LinearGradientBrush linearGradientBrush = new LinearGradientBrush(captionRect, CaptionStartBackgroundColor, CaptionBackgroundColor, 90);
                bg.FillRectangle(linearGradientBrush, captionRect);
                if (ShowBorder)
                {
                    graphicsPath = HDrawPaint.CreatePath(innerRect, this.WindowState == FormWindowState.Maximized ? 0 : Radius);
                    Pen pen = new Pen(BorderColor, 1);
                    bg.DrawPath(pen, graphicsPath);
                    pen.Dispose();
                }
                captionbrush.Dispose();
                linearGradientBrush.Dispose();
            }
            else
            {
                Brush captionbrush = new SolidBrush(UnActiveCaptionBackgroundColor);
                bg.FillPath(captionbrush, graphicsPath);

                Rectangle captionRect = new Rectangle(innerRect.X, innerRect.Y, innerRect.Width, ncInfo.ClientRect.Top - innerRect.Top);

                LinearGradientBrush linearGradientBrush = new LinearGradientBrush(captionRect, UnActiveCaptionStartBackgroundColor, UnActiveCaptionBackgroundColor, 90);
                bg.FillRectangle(linearGradientBrush, captionRect);
                if (UnActiveShowBorder)
                {
                    graphicsPath = HDrawPaint.CreatePath(innerRect, this.WindowState == FormWindowState.Maximized ? 0 : Radius);
                    Pen pen = new Pen(UnActiveBorderColor, 1);
                    bg.DrawPath(pen, graphicsPath);
                    pen.Dispose();
                }
                captionbrush.Dispose();
                linearGradientBrush.Dispose();
            }

            graphicsPath.Reset();
            graphicsPath = HDrawPaint.CreatePath(innerRect, this.WindowState == FormWindowState.Maximized ? 0 : Radius);
            graphicsPath.AddRectangle(outerRect);
            bg.FillPath(transpbrush, graphicsPath);

            graphicsPath.Dispose();

            #region 画标题
            Rectangle iconRect = new Rectangle(ncInfo.CaptionRect.Left+2+IconOffset.X, ncInfo.CaptionRect.Top + (ncInfo.CaptionHeight - IconSize.Height) / 2+IconOffset.Y, IconSize.Width, IconSize.Height);
            if(ShowIcon&&this.Icon!=null)
            {
                bg.DrawIcon(this.Icon, iconRect);
            }
            
            if(ShowTitle)
            {
                SizeF fontsize = TextRenderer.MeasureText(this.Text, this.Font);
                Rectangle textRect = new Rectangle((ShowIcon?iconRect.Right:2)+TitleOffset.X, ncInfo.CaptionRect.Top + (int)(ncInfo.CaptionHeight - fontsize.Height) / 2+TitleOffset.Y, (int)fontsize.Width, (int)fontsize.Height);
                bg.DrawString(this.Text, this.Font, forebrush, textRect);
            }
            #endregion 画标题

            #region 画按钮

            if (ControlBox)
            {
                int closeBtnPosX = ncInfo.CaptionRect.Right - ncInfo.CaptionButtonSize.Width;
                int maxBtnPosX = closeBtnPosX - ncInfo.CaptionButtonSize.Width;
                int minBtnPosX = maxBtnPosX - ncInfo.CaptionButtonSize.Width;
                int btnPosY = ncInfo.BorderSize.Height + (ncInfo.CaptionHeight - ncInfo.CaptionButtonSize.Height) / 2;

                Rectangle closeRect = new Rectangle(new Point(closeBtnPosX, btnPosY), ncInfo.CaptionButtonSize);
                Rectangle maxRect = new Rectangle(new Point(maxBtnPosX, btnPosY), ncInfo.CaptionButtonSize);
                Rectangle minRect = new Rectangle(new Point(minBtnPosX, btnPosY), ncInfo.CaptionButtonSize);

                closeRect = new Rectangle(closeRect.Left + (closeRect.Width - 25) / 2, closeRect.Top + (closeRect.Height - 25) / 2, 25, 25);
                maxRect = new Rectangle(maxRect.Left + (maxRect.Width - 25) / 2, maxRect.Top + (maxRect.Height - 25) / 2, 25, 25);
                minRect = new Rectangle(minRect.Left + (minRect.Width - 25) / 2, minRect.Top + (minRect.Height - 25) / 2, 25, 25);

                int posX, posY;
                int wp = m.WParam.ToInt32();
                long lp = m.LParam.ToInt64();
                posX = LOBYTE(lp);
                posY = HIBYTE(lp);

                if (m.Msg == WM_NCLBUTTONDOWN)
                {
                    if (wp == HTCLOSE)
                    {
                        bg.DrawImage(CloseButtonPressDownImage, closeRect);
                        rtn = true;
                    }
                    else
                    {
                        bg.DrawImage(CloseButtonImage, closeRect);
                    }

                    if (this.MaximizeBox || this.MinimizeBox)
                    {
                        if (this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.SizableToolWindow &&
                            this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.FixedToolWindow)
                        {
                            if (this.WindowState == FormWindowState.Maximized)
                            {
                                if (wp == HTMAXBUTTON && this.MaximizeBox)
                                {
                                    minBtnPosX = maxBtnPosX - ncInfo.CaptionButtonSize.Width;
                                    bg.DrawImage(MaximumNormalButtonPressDownImage, maxRect);
                                    rtn = true;
                                }
                                else
                                {
                                    bg.DrawImage(MaximumNormalButtonImage, maxRect);
                                }
                            }
                            else
                            {
                                if (wp == HTMAXBUTTON && this.MaximizeBox)
                                {
                                    minBtnPosX = maxBtnPosX - ncInfo.CaptionButtonSize.Width;
                                    bg.DrawImage(MaximumButtonPressDownImage, maxRect);
                                    rtn = true;
                                }
                                else
                                {
                                    bg.DrawImage(MaximumButtonImage, maxRect);
                                }
                            }
                            if (wp == HTMINBUTTON && this.MinimizeBox)
                            {
                                bg.DrawImage(MinimumButtonPressDownImage, minRect);
                                rtn = true;
                            }
                            else
                            {
                                bg.DrawImage(MinimumButtonImage, minRect);
                            }
                        }
                    }
                }
                else
                {
                    if (posX != HTCLOSE)
                    {
                        bg.DrawImage(CloseButtonImage, closeRect);
                    }
                    else if (MouseButtons != System.Windows.Forms.MouseButtons.Left)
                    {
                        bg.DrawImage(CloseButtonHoverImage, closeRect);
                    }
                    else
                    {
                        bg.DrawImage(CloseButtonPressDownImage, closeRect);
                    }

                    if (this.MaximizeBox || this.MinimizeBox)
                    {
                        if (this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.FixedToolWindow &&
                            this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.SizableToolWindow)
                        {
                            if (this.WindowState == FormWindowState.Maximized)
                            {
                                if (this.MaximizeBox)
                                {
                                    if (posX != HTMAXBUTTON)
                                    {
                                        bg.DrawImage(MaximumNormalButtonImage, maxRect);
                                    }
                                    else if (MouseButtons != System.Windows.Forms.MouseButtons.Left)
                                    {
                                        bg.DrawImage(MaximumNormalButtonHoverImage, maxRect);
                                    }
                                    else
                                    {
                                        bg.DrawImage(MaximumNormalButtonPressDownImage, maxRect);
                                    }
                                }
                                else
                                {
                                    bg.DrawImage(MaximumNormalButtonImage, maxRect);
                                }
                            }
                            else
                            {
                                if (this.MaximizeBox)
                                {
                                    if (posX != HTMAXBUTTON)
                                    {
                                        bg.DrawImage(MaximumButtonImage, maxRect);
                                    }
                                    else if (MouseButtons != System.Windows.Forms.MouseButtons.Left)
                                    {
                                        bg.DrawImage(MaximumButtonHoverImage, maxRect);
                                    }
                                    else
                                    {
                                        bg.DrawImage(MaximumButtonPressDownImage, maxRect);
                                    }
                                }
                                else
                                {
                                    bg.DrawImage(MaximumButtonImage, maxRect);
                                }
                            }

                            if (this.MinimizeBox)
                            {
                                if (posX != HTMINBUTTON)
                                {
                                    bg.DrawImage(MinimumButtonImage, minRect);
                                }
                                else if (MouseButtons != System.Windows.Forms.MouseButtons.Left)
                                {
                                    bg.DrawImage(MinimumButtonHoverImage, minRect);
                                }
                                else
                                {
                                    bg.DrawImage(MinimumButtonPressDownImage, minRect);
                                }
                            }
                            else
                            {
                                bg.DrawImage(MinimumButtonImage, minRect);
                            }
                        }
                    }
                }
            }

            #endregion 画按钮

            g.DrawImage(bitmap, new Point(-1, -1));

            bg.Dispose();
            bitmap.Dispose();

            return rtn;
        }

        #endregion help methods

        #region Major method WndProc

        /// <summary>LOBYTE 方法。</summary>
        private int LOBYTE(long p)
        { return (int)(p & 0x0000FFFF); }

        /// <summary>HIBYTE 方法。</summary>
        private int HIBYTE(long p)
        { return (int)(p >> 16); }

        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            if (this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.None)
            {
                switch (m.Msg)
                {
                    case WM_NCACTIVATE:
                        DrawCaption(m, m.WParam.ToInt32() > 0);

                        if (m.WParam == (IntPtr)0)
                        {
                            m.Result = (IntPtr)1;
                        }
                        return;

                    case WM_NCPAINT:
                        DrawCaption(m, ActiveForm == this);

                        return;

                    case WM_NCRBUTTONDOWN:
                        {
                            int posX, posY;
                            int wp = m.WParam.ToInt32();
                            long lp = m.LParam.ToInt64();
                            posX = LOBYTE(lp);
                            posY = HIBYTE(lp);

                            if (wp == HTCAPTION)
                            {
                                Point pt = this.PointToClient(new Point(posX, posY));
                                if (this.CaptionContextMenu != null)
                                {
                                    this.CaptionContextMenu.Show(posX, posY);
                                    return;
                                }
                            }
                            break;
                        }
                    case WM_SETCURSOR:
                        DrawCaption(m, Form.ActiveForm == this);
                        break;

                    case WM_NCLBUTTONUP:
                        {
                            int wp = m.WParam.ToInt32();
                            switch (wp)
                            {
                                case HTCLOSE:
                                    m.Msg = WM_SYSCOMMAND;
                                    m.WParam = new IntPtr(SC_CLOSE);
                                    break;

                                case HTMAXBUTTON:
                                    if (this.MaximizeBox)
                                    {
                                        m.Msg = WM_SYSCOMMAND;
                                        if (this.WindowState == FormWindowState.Maximized)
                                        {
                                            m.WParam = new IntPtr(SC_RESTORE);
                                        }
                                        else
                                        {
                                            m.WParam = new IntPtr(SC_MAXIMIZE);
                                        }
                                    }
                                    break;

                                case HTMINBUTTON:
                                    if (this.MinimizeBox)
                                    {
                                        m.Msg = WM_SYSCOMMAND;
                                        m.WParam = new IntPtr(SC_MINIMIZE);
                                    }
                                    break;

                                case HTHELP:
                                    m.Msg = WM_SYSCOMMAND;
                                    m.WParam = new IntPtr(SC_CONTEXTHELP);
                                    break;

                                default:
                                    break;
                            }
                            break;
                        }

                    case WM_NCLBUTTONDOWN:
                        bool rtn = DrawCaption(m, Form.ActiveForm == this);
                        if (rtn)
                        {
                            return;
                        }
                        break;
                }
            }
            base.WndProc(ref m);
        }

        #endregion Major method WndProc
    }
}