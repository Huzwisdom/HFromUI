using HFromUI.HData;
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
    using HFromUI.HLangage;
    using HFromUI.HData.Win;
    /// <summary>
    /// 此窗体用来显示主窗体的四边阴影，与PPForm匹配
    /// </summary>
    internal partial class HShadowForm : Form
    {
        //控件层
        private HForm Main;

        /// <summary>menualSizeFlag 字段。</summary>
        private bool menualSizeFlag = false;

        //带参构造
        public HShadowForm(HForm main)
        {
            //将控制层传值过来
            this.Main = main;
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            Init();
            //menualSizeFlag = false;
        }

        #region 初始化

        /// <summary>初始化。</summary>
        private void Init()
        {
            try
            {
                //置顶窗体
                TopMost = Main.TopMost;

                Main.BringToFront();

                //是否在任务栏显示
                ShowInTaskbar = false;
                //无边框模式
                FormBorderStyle = FormBorderStyle.None;
                //设置绘图层显示位置
                //this.Location = new Point(Main.Location.X - 5, Main.Location.Y - 5);
                SetLocation();
                //设置ICO
                Icon = Main.Icon;
                ShowIcon = Main.ShowIcon;
                //设置大小
                //Width = Main.Width + 10;
                //Height = Main.Height + 10;

                SetSize();
                //设置标题名
                Text = Main.Text;
                //绘图层窗体移动
                Main.LocationChanged += new EventHandler(Main_LocationChanged);
                Main.SizeChanged += new EventHandler(Main_SizeChanged);
                Main.VisibleChanged += new EventHandler(Main_VisibleChanged);
                //还原任务栏右键菜单
                //CommonClass.SetTaskMenu(Main);
                //加载背景
                SetBits();
                //窗口鼠标穿透效果
                CanPenetrate();
                SetStyles();
            }
            catch
            {
            }
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.MinimumSize = Main.MinimumSize == Size.Empty ? Size.Empty : new Size(Main.MinimumSize.Width + 2 * Main.ShadowWidth, Main.MinimumSize.Height + 2 * Main.ShadowWidth);
            this.MaximumSize = Main.MaximumSize == Size.Empty ? Size.Empty : new Size(Main.MaximumSize.Width + 2 * Main.ShadowWidth, Main.MaximumSize.Height + 2 * Main.ShadowWidth);
            SetLocation();
        }

        #endregion 初始化

        #region 还原任务栏右键菜单

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cParms = base.CreateParams;
                cParms.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cParms;
            }
        }

        public class CommonClass
        {
            [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
            private static extern int GetWindowLong(HandleRef hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
            private static extern IntPtr SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

            public const int WS_SYSMENU = 0x00080000;
            public const int WS_MINIMIZEBOX = 0x20000;

            /// <summary>设置 taskMenu。</summary>
            public static void SetTaskMenu(Form form)
            {
                int windowLong = (GetWindowLong(new HandleRef(form, form.Handle), -16));
                SetWindowLong(new HandleRef(form, form.Handle), -16, windowLong | WS_SYSMENU | WS_MINIMIZEBOX);
            }
        }

        #endregion 还原任务栏右键菜单

        #region 减少闪烁

        /// <summary>设置 styles。</summary>
        private void SetStyles()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer, true);
            //强制分配样式重新应用到控件上
            UpdateStyles();
            base.AutoScaleMode = AutoScaleMode.None;
        }

        #endregion 减少闪烁

        #region 控件层相关事件

        //移动主窗体时
        private void Main_LocationChanged(object sender, EventArgs e)
        {
            if (!menualSizeFlag)
            {
                SetLocation();
            }
        }

        //主窗体大小改变时
        private void Main_SizeChanged(object sender, EventArgs e)
        {
            if (!menualSizeFlag)
            {
                SetSize();
                SetBits();
            }
        }

        /// <summary>设置 location。</summary>
        public void SetLocation()
        {
            int x = 0;
            int y = 0;
            //if (Main.Width < 90)
            //{
            //    x = Main.Left-((int)(10 * ((float)Main.Width / 90))) / 2;
            //}
            //else
            //{
            x = Main.Left - Main.ShadowWidth;
            //}

            //if (Main.Height < 90)
            //{
            //    y = Main.Top-((int)(10 * ((float)Main.Height / 90)))/2;

            //}
            //else
            //{
            y = Main.Top - Main.ShadowWidth;
            //}

            Location = new Point(x, y);
        }

        /// <summary>设置 size。</summary>
        private void SetSize()
        {
            ////设置大小
            //if (Main.Width < 90)
            //{
            //    Width = Main.Width + (int)(10 * ((float)Main.Width / 90));
            //}
            //else
            //{
            Width = Main.Width + 2 * Main.ShadowWidth;
            //}

            //if (Main.Height < 90)
            //{
            //    Height = Main.Height+(int)(10*((float)Main.Height/90));

            //}
            //else
            //{
            Height = Main.Height + 2 * Main.ShadowWidth - 1;
            //}

            SetLocation();
        }

        //主窗体显示或隐藏时
        private void Main_VisibleChanged(object sender, EventArgs e)
        {
            this.Visible = Main.Visible;
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Rectangle screenRect = Screen.FromHandle(Main.Handle).WorkingArea;
            if (Main.EnableSizeChange && !(Main.Size == screenRect.Size || (Main.Width == screenRect.Width / 2 && Main.Height == screenRect.Height)))
            {
                int n = Main.ShadowWidth;

                Rectangle left = new Rectangle(0, n, n, this.Height - 2 * n);
                Rectangle right = new Rectangle(this.Width - n, n, n, this.Height - 2 * n);
                Rectangle top = new Rectangle(n, 0, this.Width - 2 * n, n);
                Rectangle bottom = new Rectangle(n, this.Height - n, this.Width - 2 * n, n);

                Rectangle topleft = new Rectangle(0, 0, n, n);
                Rectangle topright = new Rectangle(this.Width - n, 0, n, n);
                Rectangle bottomleft = new Rectangle(0, this.Height - n, n, n);
                Rectangle bottomright = new Rectangle(this.Width - n, this.Height - n, n, n);

                if (left.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeLeft(this.Handle);
                }
                else if (right.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeRight(this.Handle);
                }
                else if (top.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeTop(this.Handle);
                }
                else if (bottom.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeBottom(this.Handle);
                }
                else if (topleft.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeTopLeft(this.Handle);
                }
                else if (topright.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeTopRight(this.Handle);
                }
                else if (bottomleft.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeBottomLeft(this.Handle);
                }
                else if (bottomright.Contains(e.X, e.Y))
                {
                    menualSizeFlag = true;
                    HWin32.WindowSizeBottomRight(this.Handle);
                }
                menualSizeFlag = false;
            }
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Rectangle screenRect = Screen.FromHandle(Main.Handle).WorkingArea;
            if (Main.EnableSizeChange && !(Main.Size == screenRect.Size || (Main.Width == screenRect.Width / 2 && Main.Height == screenRect.Height)))
            {
                int n = Main.ShadowWidth;

                Rectangle left = new Rectangle(0, n, n, this.Height - 2 * n);
                Rectangle right = new Rectangle(this.Width - n, n, n, this.Height - 2 * n);
                Rectangle top = new Rectangle(n, 0, this.Width - 2 * n, n);
                Rectangle bottom = new Rectangle(n, this.Height - n, this.Width - 2 * n, n);

                Rectangle topleft = new Rectangle(0, 0, n, n);
                Rectangle topright = new Rectangle(this.Width - n, 0, n, n);
                Rectangle bottomleft = new Rectangle(0, this.Height - n, n, n);
                Rectangle bottomright = new Rectangle(this.Width - n, this.Height - n, n, n);

                Point p = PointToClient(MousePosition);

                if ((left.Contains(p) || right.Contains(p)))
                {
                    this.Cursor = Cursors.SizeWE;
                }
                else if ((top.Contains(p) || bottom.Contains(p)))
                {
                    this.Cursor = Cursors.SizeNS;
                }
                else if ((topleft.Contains(p) || bottomright.Contains(p)))
                {
                    this.Cursor = Cursors.SizeNWSE;
                }
                else if ((bottomleft.Contains(p) || topright.Contains(p)))
                {
                    this.Cursor = Cursors.SizeNESW;
                }
                else
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (menualSizeFlag)
            {
                Main.Width = this.Width - 2 * Main.ShadowWidth;
                Main.Height = this.Height - 2 * Main.ShadowWidth + 1;
                Main.Left = this.Left + Main.ShadowWidth;
                Main.Top = this.Top + Main.ShadowWidth;
                SetBits();
            }
        }

        #endregion 控件层相关事件

        #region 使窗口有鼠标穿透功能

        /// <summary>
        /// 使窗口有鼠标穿透功能
        /// </summary>
        private void CanPenetrate()
        {
            int intExTemp = HWin32.GetWindowLong(this.Handle, HWin32.GWL_EXSTYLE);
            int oldGWLEx = HWin32.SetWindowLong(this.Handle, HWin32.GWL_EXSTYLE, HWin32.WS_EX_TRANSPARENT | HWin32.WS_EX_LAYERED);
        }

        #endregion 使窗口有鼠标穿透功能

        #region 不规则无毛边方法

        /// <summary>设置 bits。</summary>
        public void SetBits()
        {
            //绘制绘图层背景
            Bitmap bitmap = new Bitmap(Main.Width + 2 * Main.ShadowWidth, Main.Height + 2 * Main.ShadowWidth);
            Rectangle _BacklightLTRB = new Rectangle(Main.ShadowWidth + 10, Main.ShadowWidth + 10, 50, 50);//窗体光泽重绘边界
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.HighQuality; //高质量
            g.PixelOffsetMode = PixelOffsetMode.HighQuality; //高像素偏移质量

            Bitmap shadowBmp = new Bitmap(100, 100);
            if (Main.ShowShadow)
            {
                shadowBmp = ImageDrawRect.DrawShadowBmp(Main.ShadowColor, Main.Radius, this.Size, Main.ShadowWidth);
            }
            else
            {
                shadowBmp = ImageDrawRect.DrawShadowBmp(Color.Transparent, Main.Radius, this.Size, Main.ShadowWidth);
            }
            ImageDrawRect.DrawRect(g, shadowBmp, ClientRectangle, Rectangle.FromLTRB(_BacklightLTRB.X, _BacklightLTRB.Y, _BacklightLTRB.Width, _BacklightLTRB.Height), 1, 1);

            shadowBmp.Dispose();

            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException(HTranslation.GetContent("图片必须是32位带Alhpa通道的图片。"));
            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = HWin32.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = HWin32.CreateCompatibleDC(screenDC);

            try
            {
                HWin32.Point topLoc = new HWin32.Point(Left, Top);
                HWin32.Size bitMapSize = new HWin32.Size(Width, Height);
                HWin32.BLENDFUNCTION blendFunc = new HWin32.BLENDFUNCTION();
                HWin32.Point srcLoc = new HWin32.Point(0, 0);

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = HWin32.SelectObject(memDc, hBitmap);

                blendFunc.BlendOp = HWin32.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = Byte.Parse("255");
                blendFunc.AlphaFormat = HWin32.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;

                HWin32.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc, HWin32.ULW_ALPHA);

                bitmap.Dispose();
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    HWin32.SelectObject(memDc, oldBits);
                    HWin32.DeleteObject(hBitmap);
                }
                HWin32.ReleaseDC(IntPtr.Zero, screenDC);
                HWin32.DeleteDC(memDc);
            }
        }

        #endregion 不规则无毛边方法

        /// <summary>响应 Closed 事件。</summary>
        protected override void OnClosed(EventArgs e)
        {
            Main.LocationChanged -= new EventHandler(Main_LocationChanged);
            Main.SizeChanged -= new EventHandler(Main_SizeChanged);
            Main.VisibleChanged -= new EventHandler(Main_VisibleChanged);
            base.OnClosed(e);
        }
    }

    public partial class ImageDrawRect
    {
        /// <summary>anyRight 成员。</summary>
        public static ContentAlignment anyRight = ContentAlignment.BottomRight | (ContentAlignment.MiddleRight | ContentAlignment.TopRight);
        /// <summary>anyTop 成员。</summary>
        public static ContentAlignment anyTop = ContentAlignment.TopRight | (ContentAlignment.TopCenter | ContentAlignment.TopLeft);
        /// <summary>anyBottom 成员。</summary>
        public static ContentAlignment anyBottom = ContentAlignment.BottomRight | (ContentAlignment.BottomCenter | ContentAlignment.BottomLeft);
        /// <summary>anyCenter 成员。</summary>
        public static ContentAlignment anyCenter = ContentAlignment.BottomCenter | (ContentAlignment.MiddleCenter | ContentAlignment.TopCenter);
        /// <summary>anyMiddle 成员。</summary>
        public static ContentAlignment anyMiddle = ContentAlignment.MiddleRight | (ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft);

        /// <summary>
        /// 绘图对像，九宫格
        /// </summary>
        /// <param name="g">绘图对像</param>
        /// <param name="img">图片</param>
        /// <param name="r">绘置的图片大小、坐标</param>
        /// <param name="lr">绘置的图片边界</param>
        /// <param name="index">当前状态</param>
        /// <param name="Totalindex">状态总数</param>
        public static void DrawRect(Graphics g, Bitmap img, Rectangle r, Rectangle lr, int index, int Totalindex)
        {
            if (img == null) return;
            Rectangle r1, r2;
            int x = (index - 1) * img.Width / Totalindex;
            int y = 0;
            int x1 = r.Left;
            int y1 = r.Top;

            if (r.Height > img.Height && r.Width <= img.Width / Totalindex)
            {
                r1 = new Rectangle(x, y, img.Width / Totalindex, lr.Top);
                r2 = new Rectangle(x1, y1, r.Width, lr.Top);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                r1 = new Rectangle(x, y + lr.Top, img.Width / Totalindex, img.Height - lr.Top - lr.Bottom);
                r2 = new Rectangle(x1, y1 + lr.Top, r.Width, r.Height - lr.Top - lr.Bottom);
                if ((lr.Top + lr.Bottom) == 0) r1.Height = r1.Height - 1;
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                r1 = new Rectangle(x, y + img.Height - lr.Bottom, img.Width / Totalindex, lr.Bottom);
                r2 = new Rectangle(x1, y1 + r.Height - lr.Bottom, r.Width, lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);
            }
            else
                if (r.Height <= img.Height && r.Width > img.Width / Totalindex)
            {
                r1 = new Rectangle(x, y, lr.Left, img.Height);
                r2 = new Rectangle(x1, y1, lr.Left, r.Height);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);
                r1 = new Rectangle(x + lr.Left, y, img.Width / Totalindex - lr.Left - lr.Right, img.Height);
                r2 = new Rectangle(x1 + lr.Left, y1, r.Width - lr.Left - lr.Right, r.Height);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);
                r1 = new Rectangle(x + img.Width / Totalindex - lr.Right, y, lr.Right, img.Height);
                r2 = new Rectangle(x1 + r.Width - lr.Right, y1, lr.Right, r.Height);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);
            }
            else
                    if (r.Height <= img.Height && r.Width <= img.Width / Totalindex)
            {
                r1 = new Rectangle((index - 1) * img.Width / Totalindex, 0, img.Width / Totalindex, img.Height - 1);
                g.DrawImage(img, new Rectangle(x1, y1, r.Width, r.Height), r1, GraphicsUnit.Pixel);
            }
            else if (r.Height > img.Height && r.Width > img.Width / Totalindex)
            {
                //top-left
                r1 = new Rectangle(x, y, lr.Left, lr.Top);
                r2 = new Rectangle(x1, y1, lr.Left, lr.Top);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //top-bottom
                r1 = new Rectangle(x, y + img.Height - lr.Bottom, lr.Left, lr.Bottom);
                r2 = new Rectangle(x1, y1 + r.Height - lr.Bottom, lr.Left, lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //left
                r1 = new Rectangle(x, y + lr.Top, lr.Left, img.Height - lr.Top - lr.Bottom);
                r2 = new Rectangle(x1, y1 + lr.Top, lr.Left, r.Height - lr.Top - lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //top
                r1 = new Rectangle(x + lr.Left, y,
                    img.Width / Totalindex - lr.Left - lr.Right, lr.Top);
                r2 = new Rectangle(x1 + lr.Left, y1,
                    r.Width - lr.Left - lr.Right, lr.Top);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //right-top
                r1 = new Rectangle(x + img.Width / Totalindex - lr.Right, y, lr.Right, lr.Top);
                r2 = new Rectangle(x1 + r.Width - lr.Right, y1, lr.Right, lr.Top);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //Right
                r1 = new Rectangle(x + img.Width / Totalindex - lr.Right, y + lr.Top,
                    lr.Right, img.Height - lr.Top - lr.Bottom);
                r2 = new Rectangle(x1 + r.Width - lr.Right, y1 + lr.Top,
                    lr.Right, r.Height - lr.Top - lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //right-bottom
                r1 = new Rectangle(x + img.Width / Totalindex - lr.Right, y + img.Height - lr.Bottom,
                    lr.Right, lr.Bottom);
                r2 = new Rectangle(x1 + r.Width - lr.Right, y1 + r.Height - lr.Bottom,
                    lr.Right, lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //bottom
                r1 = new Rectangle(x + lr.Left, y + img.Height - lr.Bottom,
                    img.Width / Totalindex - lr.Left - lr.Right, lr.Bottom);
                r2 = new Rectangle(x1 + lr.Left, y1 + r.Height - lr.Bottom,
                    r.Width - lr.Left - lr.Right, lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);

                //Center
                r1 = new Rectangle(x + lr.Left, y + lr.Top,
                    img.Width / Totalindex - lr.Left - lr.Right, img.Height - lr.Top - lr.Bottom);
                r2 = new Rectangle(x1 + lr.Left, y1 + lr.Top,
                    r.Width - lr.Left - lr.Right, r.Height - lr.Top - lr.Bottom);
                g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// 绘图对像
        /// </summary>
        /// <param name="g">绘图对像</param>
        /// <param name="obj">图片对像</param>
        /// <param name="r">绘置的图片大小、坐标</param>
        /// <param name="index">当前状态</param>
        /// <param name="Totalindex">状态总数</param>
        public static void DrawRect(Graphics g, Bitmap img, Rectangle r, int index, int Totalindex)
        {
            if (img == null) return;
            int width = img.Width / Totalindex;
            int height = img.Height;
            Rectangle r1, r2;
            int x = (index - 1) * width;
            int y = 0;
            int x1 = r.Left;
            int y1 = r.Top;
            r1 = new Rectangle(x, y, width, height);
            r2 = new Rectangle(x1, y1, r.Width, r.Height);
            g.DrawImage(img, r2, r1, GraphicsUnit.Pixel);
        }

        /// <summary>HAlignWithin 方法。</summary>
        public static Rectangle HAlignWithin(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            if ((align & anyRight) != (ContentAlignment)0)
            {
                withinThis.X += (withinThis.Width - alignThis.Width);
            }
            else if ((align & anyCenter) != ((ContentAlignment)0))
            {
                withinThis.X += ((withinThis.Width - alignThis.Width + 1) / 2);
            }
            withinThis.Width = alignThis.Width;
            return withinThis;
        }

        /// <summary>VAlignWithin 方法。</summary>
        public static Rectangle VAlignWithin(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            if ((align & anyBottom) != ((ContentAlignment)0))
            {
                withinThis.Y += (withinThis.Height - alignThis.Height);
            }
            else if ((align & anyMiddle) != ((ContentAlignment)0))
            {
                withinThis.Y += ((withinThis.Height - alignThis.Height + 1) / 2);
            }
            withinThis.Height = alignThis.Height;
            return withinThis;
        }

        /// <summary>
        /// 绘制阴影图片
        /// </summary>
        /// <param name="shadowColor"></param>
        /// <param name="radius"></param>
        /// <param name="bmpsize"></param>
        /// <param name="shadowWidth"></param>
        /// <returns></returns>
        public static Bitmap DrawShadowBmp(Color shadowColor, int radius, Size bmpsize, int shadowWidth)
        {
            Bitmap bmp = new Bitmap(2 * shadowWidth + 100, 2 * shadowWidth + 100);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float alpah = 0;
            float peralpah = 150 / (float)shadowWidth;

            Pen pen = new Pen(Color.FromArgb(1, 1, 1, 1));
            GraphicsPath path = new GraphicsPath();
            var bound = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var cradius = radius == 0 ? 5 : radius;

            for (int i = 0; i <= shadowWidth; i++)
            {
                int dradius = cradius > 0 ? (cradius + ((shadowWidth - i) <= 0 ? 4 : cradius + (shadowWidth - i))) : cradius;
                if (shadowColor == Color.Transparent)
                {
                    pen = new Pen(Color.FromArgb(1, 1, 1, 1));
                    path = HDrawPaint.CreatePath(new Rectangle(i, i, bmp.Width - 1 - 2 * i, bmp.Height - 1 - 2 * i), dradius);
                    g.DrawPath(pen, path);
                }
                else
                {
                    if (i < (float)shadowWidth / 5)
                    {
                        alpah += peralpah / 4;
                    }
                    else if (i < 2 * (float)shadowWidth / 5 && i >= (float)shadowWidth / 5)
                    {
                        alpah += 2 * peralpah / 4;
                    }
                    else if (i < 3 * (float)shadowWidth / 5 && i >= 2 * (float)shadowWidth / 5)
                    {
                        alpah += 3 * peralpah / 4;
                    }
                    else if (i < 4 * (float)shadowWidth / 5 && i >= 3 * (float)shadowWidth / 5)
                    {
                        alpah += peralpah;
                    }
                    else
                    {
                        alpah += 5 * peralpah / 4;
                    }
                    pen = new Pen(Color.FromArgb((int)alpah, shadowColor));
                    path = HDrawPaint.CreatePath(new Rectangle(i, i, bmp.Width - 1 - 2 * i, bmp.Height - 1 - 2 * i), dradius);
                    g.DrawPath(pen, path);
                }
            }


            //将主窗体区域覆盖为透明色


            var p = HDrawPaint.CreatePath(new Rectangle(shadowWidth, shadowWidth, bmp.Width - 2 * shadowWidth, bmp.Height - 2 * shadowWidth+1), radius);
            if(radius==0)
            {
                p=new GraphicsPath();
                p.AddRectangle(new Rectangle(shadowWidth, shadowWidth, bmp.Width - 2 * shadowWidth-1, bmp.Height - 2 * shadowWidth));
            }
            
            g.CompositingMode = CompositingMode.SourceCopy;
            var brush = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
            g.FillPath(brush, p);
            g.CompositingMode = CompositingMode.SourceOver;

            brush.Dispose();
            pen.Dispose();
            path.Dispose();

            return bmp;
        }
    }
}
