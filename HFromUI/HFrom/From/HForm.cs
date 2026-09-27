using HFromUI.HAttribute;
using HFromUI.HData;

using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HColor;
    using HFromUI.HData.Win;
    public partial class HForm : Form
    {
        #region 属性定义
        /// <summary>
        /// 阴影窗体
        /// </summary>
        private HShadowForm shadowForm;

        #region 私有属性

        /// <summary>animeTimeClose 字段。</summary>
        private int animeTimeClose = 500;
        /// <summary>animeTimeLoad 字段。</summary>
        private int animeTimeLoad = 500;
        /// <summary>animeTypeClose 字段。</summary>
        private AnimeType animeTypeClose = AnimeType.NULL;
        /// <summary>animeTypeLoad 字段。</summary>
        private AnimeType animeTypeLoad = AnimeType.NULL;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.White;
        /// <summary>borderColor 字段。</summary>
        private Color borderColor = Color.DodgerBlue;
        /// <summary>borderWidth 字段。</summary>
        private float borderWidth = 0.5f;
        /// <summary>buttonCloseDownPic 字段。</summary>
        private Bitmap buttonCloseDownPic = HPhoto.Get("FormCloseIn");
        /// <summary>buttonCloseInPic 字段。</summary>
        private Bitmap buttonCloseInPic = HPhoto.Get("FormCloseIn");
        /// <summary>ButtonCloseMouseState 字段。</summary>
        private MouseState ButtonCloseMouseState = MouseState.Normal;
        /// <summary>buttonCloseNormalPic 字段。</summary>
        private Bitmap buttonCloseNormalPic = HPhoto.Get("FormClose");
        /// <summary>ButtonCloseRect 字段。</summary>
        private Rectangle ButtonCloseRect = new Rectangle(374, 1, 25, 25);
        /// <summary>buttonMaxDownPic 字段。</summary>
        private Bitmap buttonMaxDownPic = HPhoto.Get("FormMaxIn");
        /// <summary>buttonMaxInPic 字段。</summary>
        private Bitmap buttonMaxInPic = HPhoto.Get("FormMaxIn");
        /// <summary>ButtonMaxMouseState 字段。</summary>
        private MouseState ButtonMaxMouseState = MouseState.Normal;
        /// <summary>buttonMaxNormalPic 字段。</summary>
        private Bitmap buttonMaxNormalPic = HPhoto.Get("FormMax");
        //关闭按钮区域
        private Rectangle ButtonMaxRect = new Rectangle(346, 1, 25, 25);

        /// <summary>buttonMinDownPic 字段。</summary>
        private Bitmap buttonMinDownPic = HPhoto.Get("FormMinIn");
        /// <summary>buttonMinInPic 字段。</summary>
        private Bitmap buttonMinInPic = HPhoto.Get("FormMinIn");
        /// <summary>ButtonMinMouseState 字段。</summary>
        private MouseState ButtonMinMouseState = MouseState.Normal;
        /// <summary>buttonMinNormalPic 字段。</summary>
        private Bitmap buttonMinNormalPic = HPhoto.Get("FormMin");
        //最大化按钮区域
        private Rectangle ButtonMinRect = new Rectangle(318, 1, 25, 25);

        /// <summary>buttonRestoreDownPic 字段。</summary>
        private Bitmap buttonRestoreDownPic = HPhoto.Get("FormRestoreIn");
        /// <summary>buttonRestoreInPic 字段。</summary>
        private Bitmap buttonRestoreInPic = HPhoto.Get("FormRestoreIn");
        //最小化按钮区域
        private Bitmap buttonRestoreNormalPic = HPhoto.Get("FormRestore");

        /// <summary>closeBox 字段。</summary>
        private bool closeBox = true;
        /// <summary>enableDoubleBuffer 字段。</summary>
        private bool enableDoubleBuffer = true;
        /// <summary>enableDoubleCilckSizeChange 字段。</summary>
        private bool enableDoubleCilckSizeChange = true;
        /// <summary>enableParamsDoubleBuffer 字段。</summary>
        private bool enableParamsDoubleBuffer = true;
        /// <summary>enableScreenDock 字段。</summary>
        private bool enableScreenDock = true;
        /// <summary>enableSizeChange 字段。</summary>
        private bool enableSizeChange = true;
        /// <summary>maximizeBox 字段。</summary>
        private bool maximizeBox = true;
        /// <summary>minimizeBox 字段。</summary>
        private bool minimizeBox = true;
        /// <summary>mouseMovePosition 字段。</summary>
        private MovePosition mouseMovePosition = MovePosition.FULLSCREEN;
        /// <summary>radius 字段。</summary>
        private int radius = 11;
        /// <summary>shadowColor 字段。</summary>
        private Color shadowColor = Color.Black;
        /// <summary>showBorder 字段。</summary>
        private bool showBorder = true;
        /// <summary>showShadow 字段。</summary>
        private bool showShadow = true;
        /// <summary>showTitleIcon 字段。</summary>
        private bool showTitleIcon = true;
        /// <summary>showTitleText 字段。</summary>
        private bool showTitleText = true;
        /// <summary>titleAlignment 字段。</summary>
        private Alignment titleAlignment = Alignment.LEFT;
        /// <summary>titleColor 字段。</summary>
        private Color titleColor = Color.DodgerBlue;
        /// <summary>titleHeight 字段。</summary>
        private int titleHeight = 32;
        /// <summary>titleOffset 字段。</summary>
        private Point titleOffset = new Point(0, 0);
        //屏幕停靠

        private bool FormMoveMouseDown = false;

        /// <summary>maxSize 字段。</summary>
        private Size maxSize = Size.Empty;

        /// <summary>minSize 字段。</summary>
        private Size minSize = Size.Empty;

        /// <summary>MoveMouseDownPoint 字段。</summary>
        private Point MoveMouseDownPoint = new Point(0, 0);
        /// <summary>
        /// 记录窗体最后改变的大小，便于最大化后恢复
        /// </summary>
        private Rectangle SavedRect = new Rectangle(0, 0, 400, 250);

        /// <summary>SpecialLoadtimer 字段。</summary>
        private System.Windows.Forms.Timer SpecialLoadtimer = new System.Windows.Forms.Timer();
        /// <summary>
        /// 动画
        /// </summary>
        private HAnimation anime;
        #endregion 私有属性

        #region 公共属性
        [HCategoryLanguage("_动画"), HDisplayNameLanguage("窗体关闭时的动画时间"), HDescriptionLanguage("窗体关闭时的动画时间"), Browsable(true)]
        public int AnimeTimeClose
        {
            get { return animeTimeClose; }
            set { if (value >= 20) animeTimeClose = value; }
        }

        [HCategoryLanguage("_动画"), HDisplayNameLanguage("窗体加载时的动画时间"), HDescriptionLanguage("窗体加载时的动画时间"), Browsable(true)]
        public int AnimeTimeLoad
        {
            get { return animeTimeLoad; }
            set { if (value >= 20) animeTimeLoad = value; }
        }

        [HCategoryLanguage("_动画"), HDisplayNameLanguage("窗体关闭时的动画"), HDescriptionLanguage("窗体关闭时的动画"), Browsable(true)]
        public AnimeType AnimeTypeClose
        {
            get { return animeTypeClose; }
            set { animeTypeClose = value; }
        }

        [HCategoryLanguage("_动画"), HDisplayNameLanguage("窗体加载时的动画"), HDescriptionLanguage("窗体加载时的动画"), Browsable(true)]
        public AnimeType AnimeTypeLoad
        {
            get { return animeTypeLoad; }
            set { animeTypeLoad = value; }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景色"), Browsable(true)]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("_边框"), HDisplayNameLanguage("边框色"), HDescriptionLanguage("边框色"), Browsable(true)]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮关闭下图片"), HDescriptionLanguage("关闭按钮鼠标按下显示图片"), Browsable(true)]
        public Bitmap ButtonCloseDownPic
        {
            get { return buttonCloseDownPic; }
            set
            {
                buttonCloseDownPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮关闭入图片"), HDescriptionLanguage("关闭按钮鼠标移入显示图片"), Browsable(true)]
        public Bitmap ButtonCloseInPic
        {
            get { return buttonCloseInPic; }
            set
            {
                buttonCloseInPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("关闭按钮正常显示图片"), HDescriptionLanguage("关闭按钮正常显示图片"), Browsable(true)]
        public Bitmap ButtonCloseNormalPic
        {
            get { return buttonCloseNormalPic; }
            set
            {
                buttonCloseNormalPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮最大下图片"), HDescriptionLanguage("最大化按钮鼠标按下显示图片"), Browsable(true)]
        public Bitmap ButtonMaxDownPic
        {
            get { return buttonMaxDownPic; }
            set
            {
                buttonMaxDownPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮最大入图片"), HDescriptionLanguage("最大化按钮鼠标移入显示图片"), Browsable(true)]
        public Bitmap ButtonMaxInPic
        {
            get { return buttonMaxInPic; }
            set
            {
                buttonMaxInPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮最大常规图片"), HDescriptionLanguage("最大化按钮正常显示图片"), Browsable(true)]
        public Bitmap ButtonMaxNormalPic
        {
            get { return buttonMaxNormalPic; }
            set
            {
                buttonMaxNormalPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮最小下图片"), HDescriptionLanguage("最小化按钮鼠标按下显示图片"), Browsable(true)]
        public Bitmap ButtonMinDownPic
        {
            get { return buttonMinDownPic; }
            set
            {
                buttonMinDownPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮最小入图片"), HDescriptionLanguage("最小化按钮鼠标移入显示图片"), Browsable(true)]
        public Bitmap ButtonMinInPic
        {
            get { return buttonMinInPic; }
            set
            {
                buttonMinInPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("按钮最小常规图片"), HDescriptionLanguage("最小化按钮正常显示图片"), Browsable(true)]
        public Bitmap ButtonMinNormalPic
        {
            get { return buttonMinNormalPic; }
            set
            {
                buttonMinNormalPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("还原按钮按下图片"), HDescriptionLanguage("恢复按钮鼠标按下显示图片"), Browsable(true)]
        public Bitmap ButtonRestoreDownPic
        {
            get { return buttonRestoreDownPic; }
            set
            {
                buttonRestoreDownPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("还原按钮悬停图片"), HDescriptionLanguage("恢复按钮鼠标移入显示图片"), Browsable(true)]
        public Bitmap ButtonRestoreInPic
        {
            get { return buttonRestoreInPic; }
            set
            {
                buttonRestoreInPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("恢复按钮正常显示图片"), HDescriptionLanguage("恢复按钮正常显示图片"), Browsable(true)]
        public Bitmap ButtonRestoreNormalPic
        {
            get { return buttonRestoreNormalPic; }
            set
            {
                buttonRestoreNormalPic = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("是否显示关闭按钮"), HDescriptionLanguage("是否显示关闭按钮"), Browsable(true)]
        public bool CloseBox
        {
            get { return closeBox; }
            set
            {
                closeBox = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用双重缓冲"), HDescriptionLanguage("是否启用双重缓冲"), Browsable(true)]
        public bool EnableDoubleBuffer
        {
            get { return enableDoubleBuffer; }
            set
            {
                enableDoubleBuffer = value;
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("双击切换大小"), HDescriptionLanguage("是否启用双击窗体最大化还原"), Browsable(true)]
        public bool EnableDoubleCilckSizeChange
        {
            get { return enableDoubleCilckSizeChange; }
            set
            {
                enableDoubleCilckSizeChange = value;
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("参数双缓冲"), HDescriptionLanguage("是否启用CreateParams双重缓冲"), Browsable(true)]
        public bool EnableParamsDoubleBuffer
        {
            get { return enableParamsDoubleBuffer; }
            set
            {
                enableParamsDoubleBuffer = value;
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用屏幕停靠"), HDescriptionLanguage("是否启用屏幕停靠"), Browsable(true)]
        public bool EnableScreenDock
        {
            get { return enableScreenDock; }
            set
            {
                enableScreenDock = value;
            }
        }
        [HCategoryLanguage("_边框"), HDisplayNameLanguage("允许调整大小"), HDescriptionLanguage("是否可以拖动边框修改窗口大小"), Browsable(true)]
        public bool EnableSizeChange
        {
            get { return enableSizeChange; }
            set
            {
                enableSizeChange = value;
            }
        }

        /// <summary>IconSize 成员。</summary>
        /// <summary>IconSize 字段。</summary>
        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("图标大小"), HDescriptionLanguage("图标大小"), Browsable(true)]
        public Size IconSize { get; set; } = new Size(20, 20);

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("是否显示最大化按钮"), HDescriptionLanguage("是否显示最大化按钮"), Browsable(true)]
        public new bool MaximizeBox
        {
            get { return maximizeBox; }
            set
            {
                maximizeBox = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_控制"), HDisplayNameLanguage("是否显示最小化按钮"), HDescriptionLanguage("是否显示最小化按钮"), Browsable(true)]
        public new bool MinimizeBox
        {
            get { return minimizeBox; }
            set
            {
                minimizeBox = value;
                base.Invalidate();
            }
        }

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("鼠标移动位置"), HDescriptionLanguage("定义鼠标可以移动窗体的区域"), Browsable(true)]
        public MovePosition MouseMovePosition
        {
            get { return mouseMovePosition; }
            set
            {
                mouseMovePosition = value;
            }
        }

        [HCategoryLanguage("_边框"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set
            {
                if (radius < 0)
                    return;
                radius = value;
                this.Invalidate();
                //Win32.SetFormRoundRectRgn(this, radius);
                SetReion(radius);
                //this.Region = new Region(CreateRound(new Rectangle(0, 0, this.Width , this.Height), radius));
            }
        }

        [HCategoryLanguage("_边框"), HDisplayNameLanguage("阴影颜色"), HDescriptionLanguage("阴影颜色"), Browsable(true)]
        public Color ShadowColor
        {
            get
            {
                return shadowColor;
            }
            set
            {
                shadowColor = value;
                if (shadowForm != null && !shadowForm.IsDisposed)
                {
                    shadowForm.SetBits();
                }
            }
        }

        /// <summary>ShadowWidth 成员。</summary>
        /// <summary>ShadowWidth 字段。</summary>
        [HCategoryLanguage("_边框"), HDisplayNameLanguage("阴影宽度"), HDescriptionLanguage("阴影宽度"), Browsable(true)]
        public int ShadowWidth { get; set; } = 5;

        [HCategoryLanguage("_边框"), HDisplayNameLanguage("是否显示边框"), HDescriptionLanguage("是否显示边框"), Browsable(true)]
        public bool ShowBorder
        {
            get { return showBorder; }
            set
            {
                showBorder = value;
                this.Invalidate();
            }
        }
        [HCategoryLanguage("_边框"), HDisplayNameLanguage("是否显示阴影"), HDescriptionLanguage("是否显示阴影"), Browsable(true)]
        public bool ShowShadow
        {
            get { return showShadow; }
            set { showShadow = value; }
        }
        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("是否显示标题栏图标"), HDescriptionLanguage("是否显示标题栏图标"), Browsable(true)]
        public bool ShowTitleIcon
        {
            get { return showTitleIcon; }
            set
            {
                showTitleIcon = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("是否显示标题"), HDescriptionLanguage("是否显示标题"), Browsable(true)]
        public bool ShowTitleText
        {
            get { return showTitleText; }
            set
            {
                showTitleText = value;
                this.Invalidate();
            }
        }

        /// <summary>TextDrawMode 成员。</summary>
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Anti;

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("标题显示的位置"), HDescriptionLanguage("标题显示的位置"), Browsable(true)]
        public Alignment TitleAlignment
        {
            get { return titleAlignment; }
            set
            {
                titleAlignment = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("定义标题框的颜色"), HDescriptionLanguage("定义标题框的颜色"), Browsable(true)]
        public Color TitleColor
        {
            get { return titleColor; }
            set
            {
                titleColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("定义标题栏的高度"), HDescriptionLanguage("定义标题栏的高度"), Browsable(true)]
        public int TitleHeight
        {
            get { return titleHeight; }
            set
            {
                if (value < 0)
                    return;
                titleHeight = value;
                ReGetControlBoxRect();
                this.Invalidate();
            }
        }
        [HCategoryLanguage("_标题栏"), HDisplayNameLanguage("标题显示的位置偏移"), HDescriptionLanguage("标题显示的位置偏移"), Browsable(true)]
        public Point TitleOffset
        {
            get { return titleOffset; }
            set
            {
                titleOffset = value;
                this.Refresh();
            }
        }
        #endregion 公共属性

        #endregion 属性定义

        #region 枚举
        /// <summary>
        /// 鼠标状态
        /// </summary>
        private enum MouseState
        {
            Normal,
            MouseIn,
            MouseDown
        }

        /// <summary>
        /// 标题字符位置
        /// </summary>
        public enum Alignment
        {
            LEFT,
            CENTER,
        }

        /// <summary>
        /// 动画类型
        /// </summary>
        public enum AnimeType
        {
            NULL,
            AW_BLEND,
            AW_SLIDE_AW_HOR_POSITIVE,
            AW_SLIDE_AW_HOR_NEGATIVE,
            AW_SLIDE_AW_VER_POSITIVE,
            AW_SLIDE_AW_VER_NEGATIVE
        }

        /// <summary>
        /// 鼠标拖拽窗体类型
        /// </summary>
        public enum MovePosition
        {
            FULLSCREEN,
            TITLE,
            NONE
        }
        #endregion

       

        public HForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            if (enableDoubleBuffer)
            {
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.DoubleBuffer, true);
                this.SetStyle(ControlStyles.ResizeRedraw, true);
                this.SetStyle(ControlStyles.Selectable, true);
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.UpdateStyles();
            }

            //设置点击任务栏图标可以实现最小化和还原
            HWin32.WindowTaskBarMinimsizeAndRestore(this);

            //设置圆角
            //Win32.SetFormRoundRectRgn(this, radius);
            SetReion(radius);

            //初始化动画类
            anime = new HAnimation(this);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | HWin32.WS_MINIMIZEBOX;//允许点任务栏图标最小化操作
                if (enableParamsDoubleBuffer && !DesignMode)
                {
                    cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
                }

                return cp;
            }
        }

        //窗体加载淡入特效定时器
        protected override void OnLoad(EventArgs e)
        {
            if (StartPosition == FormStartPosition.CenterScreen)
            {
                Rectangle screenRect = Screen.PrimaryScreen.WorkingArea;
                this.Location = new Point((screenRect.Width - this.Width) / 2, (screenRect.Height - this.Height) / 2);
            }

            ReGetControlBoxRect();

            Rectangle rect = this.Bounds;
            minSize = this.MinimumSize;
            maxSize = this.MaximumSize;
            switch (animeTypeLoad)
            {
                case AnimeType.AW_BLEND:
                    this.Opacity = 0;
                    //SpecialLoadtimer.Interval = animeTimeLoad / 20;
                    //SpecialLoadtimer.Start();
                    Dictionary<string, float> dic = new Dictionary<string, float>();
                    dic.Add("Opacity", 1);
                    anime.isBlock=false;
                    anime.AnimationType = AnimationType.UniformMotion;
                    anime.AnimationControl(dic, AnimeTimeLoad);
                    break;//定时器实现淡入淡出
                case AnimeType.AW_SLIDE_AW_HOR_POSITIVE:
                    this.MinimumSize = Size.Empty;
                    this.MaximumSize = Size.Empty;
                    this.Width = 1;
                    anime.AnimationRectangle(rect, AnimeTimeLoad);
                    break;//动画组件实现弹出
                case AnimeType.AW_SLIDE_AW_HOR_NEGATIVE:
                    this.MinimumSize = Size.Empty;
                    this.MaximumSize = Size.Empty;
                    this.Left = this.Right - 1;
                    this.Width = 1;
                    anime.AnimationRectangle(rect, AnimeTimeLoad);
                    break;

                case AnimeType.AW_SLIDE_AW_VER_POSITIVE:
                    this.MinimumSize = Size.Empty;
                    this.MaximumSize = Size.Empty;
                    this.Height = 1;
                    anime.AnimationRectangle(rect, AnimeTimeLoad);
                    break;

                case AnimeType.AW_SLIDE_AW_VER_NEGATIVE:
                    this.MinimumSize = Size.Empty;
                    this.MaximumSize = Size.Empty;
                    this.Top = this.Bottom - 1;
                    this.Height = 1;
                    anime.AnimationRectangle(rect, AnimeTimeLoad);
                    break;
            }

            //new Thread(delegate ()
            //{
            Thread.Sleep(this.AnimeTimeLoad);
            try
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new MethodInvoker(() => { this.MinimumSize = minSize; this.MaximumSize = maxSize; }));
                }
            }
            catch
            {
            }
            //}).Start();

            base.OnLoad(e);
        }

        /// <summary>响应 LocationChanged 事件。</summary>
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
            if (!((this.Width == screenRect.Width && this.Height == screenRect.Height) || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            {
                SavedRect.Location = this.Location;
            }
        }

        /// <summary>响应 MaximumSizeChanged 事件。</summary>
        protected override void OnMaximumSizeChanged(EventArgs e)
        {
            base.OnMaximumSizeChanged(e);
            if (shadowForm != null && !shadowForm.IsDisposed)
                shadowForm.MaximumSize = this.MaximumSize == Size.Empty ? Size.Empty : new Size(this.MaximumSize.Width + 10, this.MaximumSize.Height + 10);
        }

        /// <summary>响应 MinimumSizeChanged 事件。</summary>
        protected override void OnMinimumSizeChanged(EventArgs e)
        {
            base.OnMinimumSizeChanged(e);
            if (shadowForm != null && !shadowForm.IsDisposed)
                shadowForm.MinimumSize = this.MinimumSize == Size.Empty ? Size.Empty : new Size(this.MinimumSize.Width + 10, this.MinimumSize.Height + 10);
        }

        /// <summary>响应 MouseClick 事件。</summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (closeBox && ButtonCloseRect.Contains(e.Location))
            {
                ButtonCloseMouseState = MouseState.Normal;
                this.Invalidate();
                this.Close();
            }
            else if (maximizeBox && ButtonMaxRect.Contains(e.Location))
            {
                ButtonMaxMouseState = MouseState.Normal;
                this.Invalidate();
                Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
                if (this.Width == screenRect.Width && this.Height == screenRect.Height)
                {
                    this.DesktopBounds = SavedRect;
                }
                else
                {
                    this.DesktopBounds = new Rectangle(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height);
                }
            }
            else if (minimizeBox && ButtonMinRect.Contains(e.Location))
            {
                ButtonMinMouseState = MouseState.Normal;
                this.Invalidate();
                this.WindowState = FormWindowState.Minimized;
            }
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks <= 1)
                {
                    if (ButtonCloseRect.Contains(e.Location))//修改控制按钮按下状态
                    {
                        ButtonCloseMouseState = MouseState.MouseDown;
                        base.Invalidate();
                    }
                    if (ButtonMaxRect.Contains(e.Location))
                    {
                        ButtonMaxMouseState = MouseState.MouseDown;
                        base.Invalidate();
                    }
                    if (ButtonMinRect.Contains(e.Location))
                    {
                        ButtonMinMouseState = MouseState.MouseDown;
                        base.Invalidate();
                    }

                    Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;

                    #region 修改大小的方法實現放到shadowform

                    //if (!(this.Width == screenRect.Width && this.Height == screenRect.Height))//最大化时不能拖动修改大小
                    //{
                    //    if (enableSizeChange)
                    //    {
                    //        Rectangle left = new Rectangle(0, 2, 2, this.Height - 4);
                    //        Rectangle right = new Rectangle(this.Width - 2, 2, 2, this.Height - 4);
                    //        Rectangle top = new Rectangle(2, 0, this.Width - 4, 2);
                    //        Rectangle bottom = new Rectangle(2, this.Height - 2, this.Width - 4, 2);

                    // int n = radius <= 0 ? 2 : radius / 2;

                    // Rectangle topleft = new Rectangle(0, 0, n, n); Rectangle topright = new
                    // Rectangle(this.Width - n, 0, n, n); Rectangle bottomleft = new Rectangle(0,
                    // this.Height - n, n, n); Rectangle bottomright = new Rectangle(this.Width - n,
                    // this.Height - n, n, n);

                    //        if (left.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeLeft(this.Handle);
                    //        }
                    //        else if (right.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeRight(this.Handle);
                    //        }
                    //        else if (top.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeTop(this.Handle);
                    //        }
                    //        else if (bottom.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeBottom(this.Handle);
                    //        }
                    //        else if (topleft.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeTopLeft(this.Handle);
                    //        }
                    //        else if (topright.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeTopRight(this.Handle);
                    //        }
                    //        else if (bottomleft.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeBottomLeft(this.Handle);
                    //        }
                    //        else if (bottomright.Contains(e.X, e.Y))
                    //        {
                    //            Win32.WindowSizeBottomRight(this.Handle);
                    //        }
                    //    }
                    //}

                    #endregion 修改大小的方法實現放到shadowform

                    if (mouseMovePosition == MovePosition.TITLE)
                    {
                        Rectangle titleRect = new Rectangle(2, 2, this.Width - 4, titleHeight - 2);
                        if (titleRect.Contains(e.X, e.Y) && !ButtonCloseRect.Contains(e.Location) && !ButtonMaxRect.Contains(e.Location) && !ButtonMinRect.Contains(e.Location))
                        {
                            FormMoveMouseDown = true;
                            MoveMouseDownPoint = PointToScreen(e.Location);
                            if (!((this.Width == screenRect.Width || this.Width == screenRect.Width / 2) && this.Height == screenRect.Height))
                            {
                                HWin32.WindowMove(this.Handle);
                            }
                        }
                    }
                    else if (mouseMovePosition == MovePosition.FULLSCREEN)
                    {
                        Rectangle bodyRect = new Rectangle(2, 2, this.Width - 4, this.Height - 4);

                        if (bodyRect.Contains(e.X, e.Y) && !ButtonCloseRect.Contains(e.Location) && !ButtonMaxRect.Contains(e.Location) && !ButtonMinRect.Contains(e.Location))
                        {
                            FormMoveMouseDown = true;
                            MoveMouseDownPoint = PointToScreen(e.Location);
                            if (!((this.Width == screenRect.Width || this.Width == screenRect.Width / 2) && this.Height == screenRect.Height))
                            {
                                HWin32.WindowMove(this.Handle);
                            }
                        }
                    }
                }
                else if (e.Clicks == 2)
                {
                    if (enableDoubleCilckSizeChange)
                    {
                        if (mouseMovePosition == MovePosition.TITLE)
                        {
                            Rectangle titleRect = new Rectangle(0, 0, this.Width, this.titleHeight);
                            if (!titleRect.Contains(e.X, e.Y))
                            {
                                return;
                            }
                        }

                        Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
                        if (this.Width == screenRect.Width && this.Height == screenRect.Height)
                        {
                            this.DesktopBounds = SavedRect;
                        }
                        else
                        {
                            this.DesktopBounds = new Rectangle(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height);
                        }
                    }
                }
            }
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ButtonCloseMouseState = MouseState.Normal;
            ButtonMaxMouseState = MouseState.Normal;
            ButtonMinMouseState = MouseState.Normal;
            this.Invalidate();
            this.Cursor = Cursors.Default;
        }

        //记录鼠标按下且是拖动时的鼠标位置
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Point p = PointToClient(MousePosition);
            Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;

            #region 修改大小的方法實現放到shadowform

            //Rectangle left = new Rectangle(0, 2, 2, this.Height - 4);
            //Rectangle right = new Rectangle(this.Width - 2, 2, 2, this.Height - 4);
            //Rectangle top = new Rectangle(2, 0, this.Width - 4, 2);
            //Rectangle bottom = new Rectangle(2, this.Height - 2, this.Width - 4, 2);

            //int n = radius <= 0 ? 2 : radius / 2;

            //Rectangle topleft = new Rectangle(0, 0, n, n);
            //Rectangle topright = new Rectangle(this.Width - n, 0, n, n);
            //Rectangle bottomleft = new Rectangle(0, this.Height - n, n, n);
            //Rectangle bottomright = new Rectangle(this.Width - n, this.Height - n, n, n);

            //if ((left.Contains(p) || right.Contains(p)) && enableSizeChange && !(this.Size == screenRect.Size || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            //{
            //    this.Cursor = Cursors.SizeWE;
            //}
            //else if ((top.Contains(p) || bottom.Contains(p)) && enableSizeChange && !(this.Size == screenRect.Size || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            //{
            //    this.Cursor = Cursors.SizeNS;
            //}
            //else if ((topleft.Contains(p) || bottomright.Contains(p)) && enableSizeChange && !(this.Size == screenRect.Size || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            //{
            //    this.Cursor = Cursors.SizeNWSE;
            //}
            //else if ((bottomleft.Contains(p) || topright.Contains(p)) && enableSizeChange && !(this.Size == screenRect.Size || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            //{
            //    this.Cursor = Cursors.SizeNESW;
            //}

            #endregion 修改大小的方法實現放到shadowform

            if (ButtonCloseRect.Contains(p) || ButtonMaxRect.Contains(p) || ButtonMinRect.Contains(p))
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }

            MouseState mouseState = ButtonCloseMouseState;//修改控制按钮的鼠标移入状态
            if (closeBox)
            {
                if (ButtonCloseRect.Contains(p))
                {
                    ButtonCloseMouseState = MouseState.MouseIn;
                }
                else
                {
                    ButtonCloseMouseState = MouseState.Normal;
                }

                if (mouseState != ButtonCloseMouseState)
                    base.Invalidate();
            }

            mouseState = ButtonMaxMouseState;
            if (maximizeBox)
            {
                if (ButtonMaxRect.Contains(p))
                {
                    ButtonMaxMouseState = MouseState.MouseIn;
                }
                else
                {
                    ButtonMaxMouseState = MouseState.Normal;
                }

                if (mouseState != ButtonMaxMouseState)
                    base.Invalidate();
            }

            mouseState = ButtonMinMouseState;
            if (minimizeBox)
            {
                if (ButtonMinRect.Contains(p))
                {
                    ButtonMinMouseState = MouseState.MouseIn;
                }
                else
                {
                    ButtonMinMouseState = MouseState.Normal;
                }

                if (mouseState != ButtonMinMouseState)
                    base.Invalidate();
            }

            if (enableScreenDock)//若启用屏幕停靠
            {
                if (FormMoveMouseDown)//表示此时鼠标按下且在进行拖动操作
                {
                    if (PointToScreen(e.Location).Y <= 0 && !(this.Width == screenRect.Width && this.Height == screenRect.Height))//正常大小时向顶部边缘拖动
                    {
                        this.DesktopBounds = new Rectangle(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height);
                        FormMoveMouseDown = false;
                    }
                    else if (PointToScreen(e.Location).X <= 0 && !(this.Width == screenRect.Width && this.Height == screenRect.Height))//正常大小时向左侧边缘拖动
                    {
                        this.DesktopBounds = new Rectangle(screenRect.Left, screenRect.Top, screenRect.Width / 2, screenRect.Height);
                        FormMoveMouseDown = false;
                    }
                    else if (PointToScreen(e.Location).X >= screenRect.Left + screenRect.Width - 1 && !(this.Width == screenRect.Width && this.Height == screenRect.Height))//正常大小时向右侧边缘拖动
                    {
                        this.DesktopBounds = new Rectangle(screenRect.Left + screenRect.Width / 2, screenRect.Top, screenRect.Width / 2, screenRect.Height);
                        FormMoveMouseDown = false;
                    }
                    else if (PointToScreen(e.Location).Y - MoveMouseDownPoint.Y >= 1 && ((this.Width == screenRect.Width && this.Height == screenRect.Height) || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))//最大化或左侧最大化或右侧最大化时向下拖恢复正常大小
                    {
                        this.DesktopBounds = new Rectangle(PointToScreen(e.Location).X - SavedRect.Width / 2, 2, SavedRect.Width, SavedRect.Height);
                        HWin32.mouse_event(HWin32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);//模拟鼠标按下
                    }
                }
            }
        }

        //表示鼠标按下且是在拖动窗体
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            FormMoveMouseDown = false;
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            int width = this.Width;
            int height = this.Height;
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            HDrawPaint.SetGraphicsHighQuality(g);
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Pen pen = new Pen(new SolidBrush(borderColor), borderWidth);
            SolidBrush brush = new SolidBrush(baseColor);

            GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius);
            GraphicsPath path2 = HDrawPaint.CreatePath(new RectangleF(0, 0, (float)this.Width, (float)this.Height), radius + 1);

            g.FillPath(brush, path);//填充基色

            using (var titlebrush= new SolidBrush(TitleColor))
            {
                Rectangle titleRect = new Rectangle(-1, -1, this.Width + 1, titleHeight + 1);
                g.SmoothingMode = SmoothingMode.Default;
                g.FillRectangle(titlebrush, titleRect);//填充标题栏
            }


            HDrawPaint.SetGraphicsHighQuality(g);
            if (showBorder)
            {
                g.DrawPath(pen, path2);//画边框
            }

            if (showTitleIcon)
            {
                g.DrawIcon(this.Icon, new Rectangle(new Point(5, (titleHeight - IconSize.Height) / 2), IconSize));//画图标
            }

            if (showTitleText)//画标题
            {
                Rectangle textRect = new Rectangle(5, 0, this.Width - 5, titleHeight);
                if (showTitleIcon)
                {
                    textRect.Offset(IconSize.Width + 5, 0);
                    textRect.Width -= IconSize.Width + 5;
                }

                if (minimizeBox)
                {
                    textRect.Width = ButtonMinRect.Left - textRect.Left;
                }
                else
                {
                    if (maximizeBox)
                    {
                        textRect.Width = ButtonMaxRect.Left - textRect.Left;
                    }
                    else
                    {
                        if (closeBox)
                        {
                            textRect.Width = ButtonCloseRect.Left - textRect.Left;
                        }
                    }
                }

                StringFormat stringFormat = new StringFormat();
                stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.Alignment = titleAlignment == Alignment.LEFT ? StringAlignment.Near : StringAlignment.Center;
                stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                Brush brush_title = new SolidBrush(this.ForeColor);
                g.DrawString(this.Text, this.Font, brush_title, textRect, stringFormat);
                brush_title.Dispose();
                stringFormat.Dispose();

                //SizeF fontsize = g.MeasureString(this.Text, this.Font);
                //switch (titleAlignment)
                //{
                //    case Alignment.LEFT:

                // g.DrawString(this.Text, this.Font, new SolidBrush(this.ForeColor), new
                // PointF(titleOffset.X + (showTitleIcon ? 30 : 5), titleOffset.Y + ((titleHeight -
                // fontsize.Height) / 2))); break;

                // case Alignment.CENTER: g.DrawString(this.Text, this.Font, new
                // SolidBrush(this.ForeColor), new PointF(titleOffset.X + ((this.Width -
                // fontsize.Width) / 2), titleOffset.Y + ((titleHeight - fontsize.Height) / 2)));
                // break; }
            }

            #region 画按钮

            PaintControlBox(g);

            #endregion 画按钮

            base.OnPaint(e);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            ReGetControlBoxRect();

            Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
            if (!((this.Width == screenRect.Width && this.Height == screenRect.Height) || (this.Width == screenRect.Width / 2 && this.Height == screenRect.Height)))
            {
                SavedRect.Size = this.Size;
            }

            if (this.Width == screenRect.Width && this.Height == screenRect.Height)
            {
                HWin32.SetFormRoundRectRgn(this, 0);//全屏时无圆角
                SetReion(0);
                if(shadowForm!=null&& !shadowForm.IsDisposed)
                    shadowForm.Visible = false;
            }
            else
            {
                //Win32.SetFormRoundRectRgn(this, radius);
                SetReion(radius);
                if (shadowForm != null&&!shadowForm.IsDisposed)
                    shadowForm.Visible = true;
            }

            this.Invalidate();
        }

        /// <summary>
        /// 绘制最大化最小化关闭按钮
        /// </summary>
        /// <param name="g"></param>
        private void PaintControlBox(Graphics g)
        {
            if (closeBox)
            {
                switch (ButtonCloseMouseState)
                {
                    case MouseState.Normal: g.DrawImage(buttonCloseNormalPic, ButtonCloseRect); break;
                    case MouseState.MouseIn: g.DrawImage(buttonCloseInPic, ButtonCloseRect); break;
                    case MouseState.MouseDown: g.DrawImage(buttonCloseDownPic, ButtonCloseRect); break;
                    default: g.DrawImage(buttonCloseNormalPic, ButtonCloseRect); break;
                }
            }

            if (maximizeBox)
            {
                Rectangle screenRect = Screen.FromHandle(this.Handle).WorkingArea;
                if (this.Width == screenRect.Width && this.Height == screenRect.Height)
                {
                    switch (ButtonMaxMouseState)
                    {
                        case MouseState.Normal: g.DrawImage(buttonRestoreNormalPic, ButtonMaxRect); break;
                        case MouseState.MouseIn: g.DrawImage(buttonRestoreInPic, ButtonMaxRect); break;
                        case MouseState.MouseDown: g.DrawImage(buttonRestoreDownPic, ButtonMaxRect); break;
                        default: g.DrawImage(buttonRestoreNormalPic, ButtonMaxRect); break;
                    }
                }
                else
                {
                    switch (ButtonMaxMouseState)
                    {
                        case MouseState.Normal: g.DrawImage(buttonMaxNormalPic, ButtonMaxRect); break;
                        case MouseState.MouseIn: g.DrawImage(buttonMaxInPic, ButtonMaxRect); break;
                        case MouseState.MouseDown: g.DrawImage(buttonMaxDownPic, ButtonMaxRect); break;
                        default: g.DrawImage(buttonMaxNormalPic, ButtonMaxRect); break;
                    }
                }
            }

            if (minimizeBox)
            {
                switch (ButtonMinMouseState)
                {
                    case MouseState.Normal: g.DrawImage(buttonMinNormalPic, ButtonMinRect); break;
                    case MouseState.MouseIn: g.DrawImage(buttonMinInPic, ButtonMinRect); break;
                    case MouseState.MouseDown: g.DrawImage(buttonMinDownPic, ButtonMinRect); break;
                    default: g.DrawImage(buttonMinNormalPic, ButtonMinRect); break;
                }
            }
        }

        /// <summary>
        /// 更新获取按钮的位置
        /// </summary>
        private void ReGetControlBoxRect()
        {
            int boxY = 0;

            if (closeBox)
            {
                boxY = titleHeight >= ButtonCloseRect.Height ? (titleHeight - ButtonCloseRect.Height) / 2 : 1;

                ButtonCloseRect.Location = new Point(this.Width - (ButtonCloseRect.Width + 1), boxY);
            }
            else
            {
                ButtonCloseRect.Location = new Point(this.Width + (ButtonCloseRect.Width + 1), (titleHeight - ButtonCloseRect.Height) / 2);
            }

            if (maximizeBox)
            {
                boxY = titleHeight >= ButtonMaxRect.Height ? (titleHeight - ButtonMaxRect.Height) / 2 : 1;
                if (closeBox)
                {
                    ButtonMaxRect.Location = new Point(this.Width - 2 * (ButtonMaxRect.Width + 1), boxY);
                }
                else
                {
                    ButtonMaxRect.Location = new Point(this.Width - (ButtonMaxRect.Width + 1), boxY);
                }
            }
            else
            {
                ButtonMaxRect.Location = new Point(this.Width + 2 * (ButtonMaxRect.Width + 1), (titleHeight - ButtonMaxRect.Height) / 2);
            }

            if (minimizeBox)
            {
                boxY = titleHeight >= ButtonMinRect.Height ? (titleHeight - ButtonMinRect.Height) / 2 : 1;

                if (closeBox && maximizeBox)
                {
                    ButtonMinRect.Location = new Point(this.Width - 3 * (ButtonMinRect.Width + 1), (titleHeight - ButtonMinRect.Height) / 2);
                }
                else if ((closeBox && !maximizeBox) || (!closeBox && maximizeBox))
                {
                    ButtonMinRect.Location = new Point(this.Width - 2 * (ButtonMinRect.Width + 1), (titleHeight - ButtonMinRect.Height) / 2);
                }
                else if (!closeBox && !maximizeBox)
                {
                    ButtonMinRect.Location = new Point(this.Width - (ButtonMinRect.Width + 1), (titleHeight - ButtonMinRect.Height) / 2);
                }
            }
            else
            {
                ButtonMinRect.Location = new Point(this.Width + 3 * (ButtonMinRect.Width + 1), (titleHeight - ButtonMinRect.Height) / 2);
            }
        }

        #region 窗体四边阴影

        /// <summary>响应 Closed 事件。</summary>
        protected override void OnClosed(EventArgs e)
        {
            //先关闭阴影窗体
            if (shadowForm != null && !shadowForm.IsDisposed)
            {
                shadowForm.Close();
            }

            this.enableParamsDoubleBuffer = false;

            Dictionary<string, float> dic = new Dictionary<string, float>();
            anime.AnimationType = AnimationType.UniformMotion;
            anime.isBlock = true;
            switch (animeTypeClose)
            {
                case AnimeType.AW_BLEND:
                    //Win32.AnimateWindow(this.Handle, animeTimeClose, Win32.AW_BLEND + Win32.AW_HIDE);
                    //SpecialClose();
                    dic.Add("Opacity", 0);
                    anime.AnimationControl(dic, AnimeTimeClose);
                    break;

                case AnimeType.AW_SLIDE_AW_HOR_POSITIVE:
                    //ShowShadow = false;
                    //shadowForm.Refresh();
                    //Win32.AnimateWindow(this.Handle, animeTimeClose, Win32.AW_SLIDE + Win32.AW_HOR_POSITIVE + Win32.AW_HIDE);
                    Rectangle rect = new Rectangle(this.Right - 1, this.Top, 1, this.Height);
                    anime.AnimationRectangle(rect, AnimeTimeClose);
                    break;

                case AnimeType.AW_SLIDE_AW_HOR_NEGATIVE:
                    //ShowShadow = false;
                    //shadowForm.Refresh();
                    //Win32.AnimateWindow(this.Handle, animeTimeClose, Win32.AW_SLIDE + Win32.AW_HOR_NEGATIVE + Win32.AW_HIDE);
                    rect = new Rectangle(this.Left, this.Top, 1, this.Height);
                    anime.AnimationRectangle(rect, AnimeTimeClose);
                    break;

                case AnimeType.AW_SLIDE_AW_VER_POSITIVE:
                    //ShowShadow = false;
                    //shadowForm.Refresh();
                    //Win32.AnimateWindow(this.Handle, animeTimeClose, Win32.AW_SLIDE + Win32.AW_VER_POSITIVE + Win32.AW_HIDE);
                    rect = new Rectangle(this.Left, this.Bottom - 1, this.Width, 1);
                    anime.AnimationRectangle(rect, AnimeTimeClose);
                    break;

                case AnimeType.AW_SLIDE_AW_VER_NEGATIVE:
                    //ShowShadow = false;
                    //shadowForm.Refresh();
                    //Win32.AnimateWindow(this.Handle, animeTimeClose, Win32.AW_SLIDE + Win32.AW_VER_NEGATIVE + Win32.AW_HIDE);
                    rect = new Rectangle(this.Left, this.Top, this.Width, 1);
                    anime.AnimationRectangle(rect, AnimeTimeClose);
                    break;
            }

            base.OnClosed(e);
        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                if (!DesignMode && (shadowForm == null || shadowForm.IsDisposed))
                {
                    shadowForm = new HShadowForm(this);
                    shadowForm.Show(this);
                }
            }
            base.OnVisibleChanged(e);
        }
        #endregion 窗体四边阴影

        /// <summary>设置 reion。</summary>
        private void SetReion(int radius)
        {
            using (GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(Point.Empty, base.Size), radius))
            {
                Region region = new Region(path);
                path.Widen(Pens.White);
                region.Union(path);
                this.Region = region;
            }
        }
    }
}