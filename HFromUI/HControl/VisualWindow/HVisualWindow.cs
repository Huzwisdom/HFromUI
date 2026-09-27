using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    using HFromUI.HColor;
    using HFromUI.HEnum;
    /// <summary>
    /// 可视窗口控件：用于显示图像、图形和进行交互操作。
    /// 优化版本：支持线程安全的图像更新、自动释放旧图、高频刷新。
    /// </summary>
    public class HVisualWindow : Panel
    {
        /// <summary>ScreenImage 成员。</summary>
        public HScreenImage ScreenImage = new HScreenImage();
        /// <summary>EnableKeys 成员。</summary>
        public bool EnableKeys = true;

        // ------------------------------------------------------------
        // 线程同步锁：保护图像属性的并发访问
        // ------------------------------------------------------------
        private readonly object _imageLock = new object();

        // 消息常量（键盘处理）
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_CHAR = 0x0102;

        // 键盘事件
        public event KeyEventHandler CoordinateKeyDown;
        public event KeyEventHandler CoordinateKeyUp;
        public event KeyPressEventHandler CoordinateKeyPress;

        // 定时器：用于周期性检查刷新和GC
        public Timer timer;

        // ------------------------------------------------------------
        // 构造函数：初始化控件样式、事件钩子
        // ------------------------------------------------------------
        public HVisualWindow()
        {
            // 使控件可选中，接收键盘输入
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            this.DoubleBuffered = true;    // 双缓冲减少闪烁
            this.Enabled = true;

            // 设计时直接返回，避免初始化运行时逻辑
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (!this.DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                // 设置字体
                ScreenImage.RulerFont = Font;
                ScreenImage.PromptShowFont = Font;

                // 背景色
                BackColor = HColors.Blacks.Soot;

                // 事件绑定
                CoordinateKeyPress += (s, e) => { HFromUICoordinate_KeyPress(this, e); };
                CoordinateKeyDown += (s, e) => { HFromUICoordinate_KeyDown(this, e); };
                CoordinateKeyUp += (s, e) => { HFromUICoordinate_KeyUp(this, e); };
                Paint += HFromUICoordinate_Paint;
                MouseWheel += HFromUICoordinate_MouseWheel;
                MouseDown += HFromUICoordinate_MouseDown;
                MouseMove += HFromUICoordinate_MouseMove;
                MouseUp += HFromUICoordinate_MouseUp;
                DoubleClick += HVisualWindow_DoubleClick;
                Resize += (s, e) => Refresh();
                SizeChanged += HCoordinate_SizeChanged;
                ScreenImage.ImageChanged += ScreenImage_ImageChanged;

                // 启动定时器
                if (timer == null)
                {
                    timer = new Timer();
                    timer.Interval = 50;   // 20Hz
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
            }
        }
  
        // ------------------------------------------------------------
        // 图像属性：线程安全的 Bitmap 访问，自动释放旧图
        // ------------------------------------------------------------
        /// <summary>
        /// 获取或设置当前显示的图像。
        /// 设置新图像时会自动释放旧图像，避免内存泄漏。
        /// </summary>
        [HCategoryLanguage("图片"), HDisplayNameLanguage("图片"), HDescriptionLanguage("图片"), Browsable(true)]
        public Bitmap Image
        {
            get
            {
                
                lock (_imageLock)
                {
                    return ScreenImage.Image;
                }
            }
            set
            {
                lock (_imageLock)
                {
                    var oldImage = ScreenImage.Image;
                    ScreenImage.Image = value;

                    // 如果旧图存在且与新图不是同一对象，则释放旧图
                    if (oldImage != null && !ReferenceEquals(oldImage, value))
                    {
                        oldImage.Dispose();
                    }
                }

                // 触发图像更新事件，立即重绘
                ScreenImage_ImageChanged(this, EventArgs.Empty);
            }
        }

        // ------------------------------------------------------------
        // HBITMAP 句柄属性：用于需要原生句柄的场景（如大华SDK）
        // ------------------------------------------------------------
        /// <summary>
        /// 获取或设置图像的 GDI 句柄（HBITMAP）。
        /// 获取时返回新句柄，调用者负责释放。
        /// 设置时根据 TakeOwnership 决定是否接管句柄所有权。
        /// </summary>
        [Browsable(false)]
        public IntPtr ImageIntPtr
        {
            get
            {
                lock (_imageLock)
                {
                    if (ScreenImage.Image == null) return IntPtr.Zero;
                    return ScreenImage.Image.GetHbitmap(); // 新句柄，调用者需释放
                }
            }
            set
            {
                SetImageFromHBitmap(value, TakeOwnership);
            }
        }

        /// <summary>
        /// 当通过 ImageIntPtr 设置图像时，是否接管传入句柄的所有权。
        /// 若为 true，则内部会调用 DeleteObject 释放句柄；
        /// 若为 false，调用者负责释放。
        /// </summary>
        /// <summary>TakeOwnership 成员。</summary>
        /// <summary>TakeOwnership 字段。</summary>
        [Browsable(false)]
        public bool TakeOwnership { get; set; } = true;

        /// <summary>
        /// 从 HBITMAP 句柄设置图像，并管理所有权（不抛出异常）。
        /// 内部会创建 Bitmap 对象，并根据 takeOwnership 决定是否释放句柄。
        /// </summary>
        /// <param name="hBitmap">GDI 位图句柄</param>
        /// <param name="takeOwnership">是否接管句柄所有权</param>
        public void SetImageFromHBitmap(IntPtr hBitmap, bool takeOwnership)
        {
            if (hBitmap == IntPtr.Zero)
            {
                Image = null;
                return;
            }

            Bitmap newImage = null;
            try
            {
                // 1. 从句柄创建 Bitmap（不自动接管所有权）
                newImage = Bitmap.FromHbitmap(hBitmap);

                // 2. 先尝试设置图像（此时尚未释放句柄，若失败可安全清理）
                Image = newImage;

                // 3. 设置成功后，如果接管所有权，释放传入句柄
                if (takeOwnership)
                {
                    DeleteObject(hBitmap);
                }
            }
            catch (Exception ex)
            {
                // 记录异常信息，但不重新抛出，保证调用方不会中断
                System.Diagnostics.Debug.WriteLine(HTranslation.GetContent($"SetImageFromHBitmap 异常: {ex.Message}"));

                // 清理临时资源
                // 如果 newImage 未被成功赋值给 Image，则手动释放
                if (newImage != null && !ReferenceEquals(Image, newImage))
                {
                    newImage.Dispose();
                }

                // 如果接管所有权且句柄仍未被释放，则释放句柄
                if (takeOwnership && hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        // GDI 函数：删除对象句柄
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // ------------------------------------------------------------
        // 是否开启GC回收（周期性强制GC，可缓解内存碎片）
        // ------------------------------------------------------------
        /// <summary>IsEnableGC 成员。</summary>
        /// <summary>IsEnableGC 字段。</summary>
        [HCategoryLanguage("选择"), HDisplayNameLanguage("是否开启GC回收"), HDescriptionLanguage("是否开启GC回收"), Browsable(true)]
        public bool IsEnableGC { get; set; }

        // ------------------------------------------------------------
        // 图像改变事件处理：直接失效重绘，不受刷新频率限制
        // 这样高频视频帧可以立即显示，不会因 RefreshHz 而延迟
        // ------------------------------------------------------------
        private int _refreshPending = 0;

        /// <summary>ScreenImage_ImageChanged 方法。</summary>
        private void ScreenImage_ImageChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                if (System.Threading.Interlocked.Exchange(ref _refreshPending, 1) == 0)
                {
                    BeginInvoke(new Action(() =>
                    {
                        System.Threading.Interlocked.Exchange(ref _refreshPending, 0);
                        Invalidate();
                    }));
                }
            }
            else
            {
                Invalidate();
            }
        }

        /// <summary>oldDateTimeRefresh 字段。</summary>
        private DateTime oldDateTimeRefresh = DateTime.Now;

        /// <summary>
        /// 重写 Refresh：用于用户交互触发的刷新（如缩放、平移），
        /// 会受 RefreshHz 频率限制，避免过度重绘。
        /// </summary>
        public override void Refresh()
        {
            if (ScreenImage.Image != null)
            {
                ScreenImage.PromptShowName = "ImageX:" + ScreenImage.ImagePoint.X.Value.ToString() + "  ImageY:" + ScreenImage.ImagePoint.Y.Value.ToString();
            }
            if (ScreenImage.RefreshHz > 0)
            {
                if ((DateTime.Now - oldDateTimeRefresh).TotalMilliseconds < ScreenImage.RefreshHz)
                {
                    ScreenImage.IsRefresh = true;
                    return; // 频率未到，不刷新
                }
                ScreenImage.IsRefresh = false;
                oldDateTimeRefresh = DateTime.Now;
            }
            Invalidate();
        }

        // ------------------------------------------------------------
        // 尺寸变化处理：更新屏幕矩形
        // ------------------------------------------------------------
        private void HCoordinate_SizeChanged(object sender, EventArgs e)
        {
            if (ScreenImage.ScreenRectangle == null)
            {
                ScreenImage.ScreenRectangle = new HRect(0, 0, this.Width, this.Height);
            }
            else
            {
                if (ScreenImage.ScreenRectangle.Width != this.Width ||
                    ScreenImage.ScreenRectangle.Height != this.Height)
                {
                    ScreenImage.ScreenRectangle.X = 0;
                    ScreenImage.ScreenRectangle.Y = 0;
                    ScreenImage.ScreenRectangle.Width = this.Width;
                    ScreenImage.ScreenRectangle.Height = this.Height;
                }
            }
        }

        /// <summary>GCINT 字段。</summary>
        private int GCINT = 0;
        /// <summary>oldNearWorldPoint 字段。</summary>
        private DateTime oldNearWorldPoint = DateTime.Now;

        /// <summary>
        /// 定时器滴答：检查尺寸变化、执行延迟刷新、可选GC
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 尺寸变化检测
            if (ScreenImage.ScreenRectangle.Width != this.Width ||
                ScreenImage.ScreenRectangle.Height != this.Height)
            {
                HCoordinate_SizeChanged(null, null);
                ScreenImage.IsRefresh = true;
            }

            // 如果标记了需要刷新，则调用 Refresh（受频率限制）
            if (ScreenImage.IsRefresh)
            {
                Refresh();
            }

            // 可选GC（缓解长时间运行的内存压力）
            if (IsEnableGC)
            {
                GCINT++;
                if (GCINT > 200) // 约每10秒一次（200*50ms）
                {
                    GCINT = 0;
                    GC.Collect();
                }
            }
        }

        // ------------------------------------------------------------
        // 鼠标、键盘事件处理（保持不变，仅保留原有逻辑）
        // ------------------------------------------------------------
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
        }

        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            bool handled = false;
            switch (m.Msg)
            {
                case WM_KEYDOWN:
                    var keyDown = (Keys)(int)m.WParam;
                    var eDown = new KeyEventArgs(keyDown);
                    CoordinateKeyDown?.Invoke(this, eDown);
                    handled = eDown.Handled;
                    break;
                case WM_CHAR:
                    char keyChar = (char)m.WParam;
                    var ePress = new KeyPressEventArgs(keyChar);
                    CoordinateKeyPress?.Invoke(this, ePress);
                    handled = ePress.Handled;
                    break;
                case WM_KEYUP:
                    var keyUp = (Keys)(int)m.WParam;
                    var eUp = new KeyEventArgs(keyUp);
                    CoordinateKeyUp?.Invoke(this, eUp);
                    handled = eUp.Handled;
                    break;
            }
            if (!handled) base.WndProc(ref m);
        }

        /// <summary>判断是否 InputKey。</summary>
        protected override bool IsInputKey(Keys keyData) => true;

        /// <summary>HVisualWindow_DoubleClick 方法。</summary>
        private void HVisualWindow_DoubleClick(object sender, EventArgs e)
        {
            ScreenImage.FitWorldRectangle(ScreenImage.GetRectWorld().ToHRectangle(), true);
        }

        /// <summary>HFromUICoordinate_KeyPress 方法。</summary>
        private void HFromUICoordinate_KeyPress(object sender, KeyPressEventArgs e) { }

        /// <summary>HFromUICoordinate_KeyDown 方法。</summary>
        private void HFromUICoordinate_KeyDown(object sender, KeyEventArgs e)
        {
            if (!EnableKeys) return;
            if (e.KeyData == Keys.F2)
            {
                ScreenImage.OpenImageDialog();
            }
        }

        /// <summary>HFromUICoordinate_KeyUp 方法。</summary>
        private void HFromUICoordinate_KeyUp(object sender, KeyEventArgs e) { }

        /// <summary>HFromUICoordinate_Paint 方法。</summary>
        private void HFromUICoordinate_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BackColor);
                HCoordinate_SizeChanged(null, null);
                g.SetClip(ScreenImage.ScreenRectangle.ToRectangleF());
                ScreenImage.DrawImage(g);
                ScreenImage.DrawImageRect(g);
                ScreenImage.DrawPromptShowNameA(g);
                g.ResetClip();
            }
            catch { }
        }

        /// <summary>HFromUICoordinate_MouseWheel 方法。</summary>
        private void HFromUICoordinate_MouseWheel(object sender, MouseEventArgs e)
        {
            HPoint mouseScreen = ScreenImage.ScreenPoint = new HPoint(e.Location);
            HPoint worldUnderMouse = ScreenImage.WorldPoint;
            HDouble oldScale = ScreenImage.ViewScale;

            HDouble newScale = (ScreenImage.ViewScale * (e.Delta > 0 ? 1.2 : 0.8));
            // newScale 是 HDouble，边界比较需先取 .Value；结果 double 经隐式转换赋回
            newScale = Math.Max(ScreenImage.ViewScaleMin.Value, Math.Min(ScreenImage.ViewScaleMax.Value, newScale.Value));

            HDouble newOffsetX = mouseScreen.X.Value - worldUnderMouse.X.Value * newScale;
            HDouble newOffsetY = this.Height - mouseScreen.Y.Value - worldUnderMouse.Y.Value * newScale;

            ScreenImage.ViewScale = newScale;
            ScreenImage.ViewOffset = new HPoint(newOffsetX, newOffsetY);
            ScreenImage.ScreenPoint = new HPoint(e.Location);
            ScreenImage.ClampViewOffsetToBounds();
            Refresh();
        }

        /// <summary>HFromUICoordinate_MouseDown 方法。</summary>
        private void HFromUICoordinate_MouseDown(object sender, MouseEventArgs e)
        {
            ScreenImage.ScreenPoint = new HPoint(e.Location);
            ScreenImage.MouseDown = true;

            if (e.Button == MouseButtons.Left)
            {
                ScreenImage.LastMousePositionWorldPoint = ScreenImage.WorldPoint.Clone();
                switch (ScreenImage.SelectedShapeType)
                {
                    case HShapeType.Select:
                        ScreenImage.SelectScreenPoint1 = ScreenImage.ScreenPoint;
                        ScreenImage.SelectScreenPoint2 = null;
                        break;
                    case HShapeType.None:
                        if (e.Button == MouseButtons.Left)
                        {
                            ScreenImage.LastMousePosition = new HPoint(e.Location);
                            ScreenImage.LastViewOffsetPosition = ScreenImage.ViewOffset.Clone();
                        }
                        break;
                }
            }
            Refresh();
        }

        /// <summary>HFromUICoordinate_MouseMove 方法。</summary>
        private void HFromUICoordinate_MouseMove(object sender, MouseEventArgs e)
        {
            ScreenImage.ScreenPoint = new HPoint(e.Location);
            if (ScreenImage.LastViewOffsetPosition == null || ScreenImage.LastMousePosition == null)
            {
                Refresh();
                return;
            }
            if (ScreenImage.MouseDown)
            {
                HDouble ViewOffsetx = ScreenImage.LastViewOffsetPosition.X.Value + 1d * (e.X - ScreenImage.LastMousePosition.X.Value);
                HDouble ViewOffsety = ScreenImage.LastViewOffsetPosition.Y.Value + -1d * (e.Y - ScreenImage.LastMousePosition.Y.Value);
                ScreenImage.ViewOffset = new HPoint(ViewOffsetx, ViewOffsety);
                ScreenImage.ClampViewOffsetToBounds();

                ScreenImage.LastViewOffsetPosition = ScreenImage.ViewOffset.Clone();
                ScreenImage.LastMousePosition = new HPoint(e.Location);
            }
            Refresh();
        }

        /// <summary>HFromUICoordinate_MouseUp 方法。</summary>
        private void HFromUICoordinate_MouseUp(object sender, MouseEventArgs e)
        {
            ScreenImage.ScreenPoint = new HPoint(e.Location);
            ScreenImage.MouseDown = false;
            Refresh();
        }
    }
}