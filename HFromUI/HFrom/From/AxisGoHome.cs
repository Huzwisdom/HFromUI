using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HLangage;
    /// <summary>
    /// 无边框圆角窗口，用于显示旋转图标、闪烁文字和警示边框，后台执行回零任务。
    /// 背景色、窗口尺寸、文字内容均可自定义。
    /// 窗口关闭后自动释放所有资源。
    /// 窗口图标由 SVG 图形生成。
    /// 文字大小会根据可用空间自动适应。
    /// 
    /// 注意：本类使用 Thread.Abort 实现强制终止，仅适用于 .NET Framework。
    /// 在 .NET Core / .NET 5+ 中，Thread.Abort 会抛出 PlatformNotSupportedException，
    /// 此时只能依赖协作取消或改用进程隔离方案。
    /// </summary>
    public partial class AxisGoHome : Form
    {
        // ============================================================
        // 任务控制字段
        // ============================================================

        /// <summary>取消令牌源，用于协作取消后台回零任务</summary>
        private CancellationTokenSource cts;

        /// <summary>外层管理任务，负责整体流程控制（等待内部线程结束）</summary>
        private Task task;

        /// <summary>实际执行回零循环的内部线程，可被强制 Abort</summary>
        private Thread workerThread;

        /// <summary>窗口上显示的文字内容（默认中英文）</summary>
        private string showText = HTranslation.GetContent("回零中 (双击退出)   Homing (Dbl-click exit)");

        /// <summary>回零检测委托，返回 true 表示回零完成</summary>
        private Func<bool> funcGoHome;

        /// <summary>标记窗口是否允许任务完成后自动关闭，防止重复关闭</summary>
        private bool isLoad = false;

        /// <summary>窗口宽度（内部变量）</summary>
        private int formWidth = 630;

        /// <summary>窗口高度（内部变量）</summary>
        private int formHeight = 100;

        // ============================================================
        // 动画资源字段
        // ============================================================

        /// <summary>驱动旋转、闪烁、边框变色等动画的定时器</summary>
        private System.Windows.Forms.Timer animationTimer;

        /// <summary>图标当前旋转角度（度）</summary>
        private float rotationAngle = 0f;

        /// <summary>文字的当前透明度</summary>
        private float textOpacity = 1f;

        /// <summary>上一帧时间戳，用于计算时间差</summary>
        private DateTime lastTickTime;

        /// <summary>上次边框颜色切换的时间</summary>
        private DateTime lastColorChangeTime;

        /// <summary>当前边框颜色在警示颜色数组中的索引</summary>
        private int borderColorIndex = 0;

        /// <summary>警示边框颜色循环数组</summary>
        private readonly Color[] warningColors = { Color.Red, Color.Orange, Color.Yellow };

        // ============================================================
        // 图标路径与边界字段（来自 SVG）
        // ============================================================

        /// <summary>圆环路径（仅描边）</summary>
        private GraphicsPath circlePath;

        /// <summary>复杂曲线路径（填充）</summary>
        private GraphicsPath complexPath;

        /// <summary>两个路径合并后的边界矩形，用于缩放绘制</summary>
        private RectangleF iconBounds;

        // ============================================================
        // 绘图对象字段（预缓存以减少 GC）
        // ============================================================

        /// <summary>背景画刷</summary>
        private SolidBrush bgBrush;

        /// <summary>图标填充画刷</summary>
        private SolidBrush iconFillBrush;

        /// <summary>图标描边笔</summary>
        private Pen iconStrokePen;

        /// <summary>窗口边框笔</summary>
        private Pen borderPen;

        /// <summary>边框宽度（6像素）</summary>
        private const float BorderWidth = 6f;

        /// <summary>边框内缩量（笔宽的一半），用于防止边框被 Region 裁剪</summary>
        private const float BorderInset = BorderWidth / 2f;

        /// <summary>窗口自定义背景色，默认青色</summary>
        private Color formBackColor = Color.FromArgb(255, 0x28, 0x92, 0x90);

        /// <summary>由 SVG 生成的窗口图标</summary>
        private Icon windowIcon;

        // ==================== 构造函数 ====================

        /// <summary>使用默认尺寸（630x100）创建窗口</summary>
        public AxisGoHome() : this(630, 100) { }

        /// <summary>使用指定宽高创建窗口</summary>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        public AxisGoHome(int width, int height)
        {
            formWidth = width;
            formHeight = height;
            InitializeComponent();
            // 设计时模式不执行后续操作，防止异常
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;
            InitDrawingResources();
            InitPaths();
            CreateWindowIcon();
            StartAnimation();
        }

        /// <summary>传入回零判断函数，使用默认背景色和尺寸</summary>
        /// <param name="func">回零检测委托</param>
        public AxisGoHome(Func<bool> func) : this()
        {
            FuncGoHome = func;
        }

        /// <summary>传入回零函数、宽高和背景色</summary>
        /// <param name="func">回零检测委托</param>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <param name="backColor">自定义背景色</param>
        public AxisGoHome(Func<bool> func, int width, int height, Color backColor) : this(width, height)
        {
            FuncGoHome = func;
            formBackColor = backColor;
            bgBrush.Color = formBackColor;
        }

        // ==================== 公开属性 ====================

        /// <summary>窗口上显示的文本（支持从其他线程安全更新）</summary>
        public string ShowText
        {
            get => showText;
            set
            {
                showText = value;
                if (IsHandleCreated)
                {
                    if (InvokeRequired) BeginInvoke(new Action(Invalidate));
                    else Invalidate();
                }
            }
        }

        /// <summary>外部定时触发动作，例如更新进度显示</summary>
        public Action<AxisGoHome> ShowAction { get; set; }

        /// <summary>回零检测函数，返回 true 表示回零完成，窗口自动关闭</summary>
        public Func<bool> FuncGoHome
        {
            get => funcGoHome;
            set
            {
                funcGoHome = value;
                Run();
            }
        }

        /// <summary>回零超时时间（秒），负数表示无超时限制</summary>
        public double GoHomeTimeout { get; set; } = -1;

        /// <summary>文字自动适应时的最大高度比例（相对于窗口高度），默认0.54</summary>
        public float AutoFontSize { get; set; } = 0.54f;

        /// <summary>窗口宽度</summary>
        public int FormWidth
        {
            get => formWidth;
            set { formWidth = value; this.Width = value; UpdateRegion(); }
        }

        /// <summary>窗口高度</summary>
        public int FormHeight
        {
            get => formHeight;
            set { formHeight = value; this.Height = value; UpdateRegion(); }
        }

        /// <summary>自定义背景色</summary>
        public Color FormBackColor
        {
            get => formBackColor;
            set { formBackColor = value; bgBrush.Color = formBackColor; Invalidate(); }
        }

        // ==================== 初始化方法 ====================

        /// <summary>初始化窗体组件和事件</summary>
        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(formWidth, formHeight);
            this.TopMost = true;
            this.BackColor = Color.Fuchsia;
            this.TransparencyKey = Color.Fuchsia;
            this.DoubleBuffered = true;

            // 设计时模式不绑定事件
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            this.MouseDoubleClick += AxisGoHome_MouseDoubleClick;
            this.Load += (s, e) => UpdateRegion();
            this.Resize += (s, e) => UpdateRegion();
            this.FormClosed += (s, e) => this.Dispose();
        }

        /// <summary>初始化绘图资源（画刷、笔等）</summary>
        private void InitDrawingResources()
        {
            bgBrush = new SolidBrush(formBackColor);
            iconFillBrush = new SolidBrush(Color.LawnGreen);
            iconStrokePen = new Pen(Color.RosyBrown, 3);
            borderPen = new Pen(warningColors[0], BorderWidth);
        }

        /// <summary>更新窗口 Region，实现圆角裁剪</summary>
        private void UpdateRegion()
        {
            using (GraphicsPath path = GetRoundedRectPath(0))
                this.Region = new Region(path);
        }

        /// <summary>初始化 SVG 路径并计算整体边界</summary>
        private void InitPaths()
        {
            circlePath = ParseSvgPath("M617.344 512.042667a105.301333 105.301333 0 1 1-105.301333-105.301334 105.301333 105.301333 0 0 1 105.173333 105.301334z");

            string complexData = "M399.232 907.477333l-12.8-15.061333a21.333333 21.333333 0 0 1 2.389333-29.866667L426.666667 832a334.208 334.208 0 0 1-142.08-72.533333 333.056 333.056 0 0 1-99.968-151.850667l-3.2-10.24 58.624-18.133333 3.2 10.026666a273.066667 273.066667 0 0 0 182.4 179.754667l-19.541334-23.594667a21.802667 21.802667 0 0 1-5.077333-4.778666 21.333333 21.333333 0 0 1 4.266667-29.866667l13.909333-10.538667a21.333333 21.333333 0 0 1 29.866667 4.266667 21.333333 21.333333 0 0 1 2.816 4.906667l87.978666 108.672-112.085333 93.866666-0.298667-0.426666a21.333333 21.333333 0 0 1-11.904 3.669333 21.333333 21.333333 0 0 1-16.341333-7.722667z m187.733333-128.256l10.154667-3.285333a275.626667 275.626667 0 0 0 179.669333-182.485333l-23.509333 19.328a25.258667 25.258667 0 0 1-2.688 2.517333 25.045333 25.045333 0 0 1-2.133333 1.450667l-2.005334 1.621333-0.341333-0.426667a21.333333 21.333333 0 0 1-25.6-6.314666l-10.709333-13.824a21.333333 21.333333 0 0 1-0.597334-25.6l-0.426666-0.554667 2.602666-2.048a20.650667 20.650667 0 0 1 2.090667-1.877333 21.333333 21.333333 0 0 1 1.92-1.322667l108.202667-86.229333 94.421333 112.298666 0.213333 0.298667a21.333333 21.333333 0 0 1-3.754666 29.866667l-13.738667 10.794666a21.333333 21.333333 0 0 1-29.866667-3.754666 21.973333 21.973333 0 0 1-2.133333-3.285334l-29.269333-36.096a332.8 332.8 0 0 1-72.533334 142.08 333.781333 333.781333 0 0 1-151.765333 100.096l-10.154667 3.157334zM113.92 449.450667a22.570667 22.570667 0 0 1-3.242667-3.2 22.186667 22.186667 0 0 1-2.133333-3.242667l-2.090667-2.517333 0.725334-0.597334a21.333333 21.333333 0 0 1 6.698666-23.68l13.610667-11.093333a21.333333 21.333333 0 0 1 23.552-2.176l0.981333-0.853333 2.730667 3.413333a20.608 20.608 0 0 1 2.730667 2.773333 20.224 20.224 0 0 1 2.218666 3.328l23.082667 28.714667a334.848 334.848 0 0 1 72.533333-142.165333 335.061333 335.061333 0 0 1 151.850667-100.266667l10.069333-3.2 18.133334 55.082667-10.24 3.242666a273.066667 273.066667 0 0 0-179.541334 183.466667l25.045334-20.736 0.981333-0.896 0.768-0.554667 3.370667-2.816 0.426666 0.597334a21.333333 21.333333 0 0 1 25.6 5.930667l11.008 13.610667a21.333333 21.333333 0 0 1-3.2 29.866667 19.498667 19.498667 0 0 1-4.266666 2.773333l-104.704 88.149333z m662.570667-18.56a273.066667 273.066667 0 0 0-182.570667-179.456l21.333333 25.6a19.968 19.968 0 0 1 2.432 2.432 20.266667 20.266667 0 0 1 2.218667 3.2l0.298667 0.384a21.333333 21.333333 0 0 1-5.12 26.453333l-13.44 11.264a21.333333 21.333333 0 0 1-27.733334-0.298667l-1.109333-1.408-0.938667-1.024a19.498667 19.498667 0 0 1-1.322666-1.749333l-89.6-110.549333 106.666666-89.216a25.258667 25.258667 0 0 1 1.877334-1.834667 20.224 20.224 0 0 1 2.261333-1.664l0.896-0.768a21.333333 21.333333 0 0 1 26.752 4.906667l11.178667 13.44a21.333333 21.333333 0 0 1-2.645334 29.866666 21.802667 21.802667 0 0 1-4.266666 2.816l-30.933334 25.002667a333.482667 333.482667 0 0 1 142.08 72.533333 333.994667 333.994667 0 0 1 100.266667 151.850667l3.285333 10.325333-58.624 17.664z";
            complexPath = ParseSvgPath(complexData);

            using (GraphicsPath combined = new GraphicsPath())
            {
                combined.AddPath(circlePath, false);
                combined.AddPath(complexPath, false);
                iconBounds = combined.GetBounds();
            }
        }

        // ==================== SVG 解析器 ====================

        /// <summary>将 SVG 路径字符串拆分为命令字母和数字标记</summary>
        private List<string> TokenizeSvgPath(string data)
        {
            List<string> tokens = new List<string>();
            int i = 0;
            while (i < data.Length)
            {
                char c = data[i];
                // 跳过空白字符和逗号
                if (char.IsWhiteSpace(c) || c == ',') { i++; continue; }
                if (char.IsLetter(c))
                {
                    tokens.Add(c.ToString());
                    i++;
                }
                else if (c == '-' || c == '+' || c == '.' || char.IsDigit(c))
                {
                    int start = i;
                    if (c == '-' || c == '+') i++;
                    while (i < data.Length && (char.IsDigit(data[i]) || data[i] == '.')) i++;
                    tokens.Add(data.Substring(start, i - start));
                }
                else { i++; }
            }
            return tokens;
        }

        /// <summary>将 SVG 路径字符串转换为 GraphicsPath 对象</summary>
        private GraphicsPath ParseSvgPath(string data)
        {
            GraphicsPath path = new GraphicsPath();
            if (string.IsNullOrEmpty(data)) return path;

            List<string> tokens = TokenizeSvgPath(data);
            int i = 0;
            float curX = 0, curY = 0, startX = 0, startY = 0;
            char lastCmd = '\0';

            while (i < tokens.Count)
            {
                string token = tokens[i];
                bool isCmd = char.IsLetter(token[0]);
                char cmd = isCmd ? token[0] : lastCmd;
                bool relative = isCmd ? char.IsLower(token[0]) : char.IsLower(lastCmd);
                if (isCmd) i++;

                switch (char.ToUpper(cmd))
                {
                    case 'M': // 移动到点
                        float xM = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float yM = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        if (relative) { xM += curX; yM += curY; }
                        curX = xM; curY = yM; startX = xM; startY = yM;
                        path.StartFigure();
                        path.AddLine(xM, yM, xM, yM);
                        break;
                    case 'L': // 直线到点
                        float xL = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float yL = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        if (relative) { xL += curX; yL += curY; }
                        path.AddLine(curX, curY, xL, yL);
                        curX = xL; curY = yL;
                        break;
                    case 'A': // 椭圆弧
                        float rx = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float ry = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float phi = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float fA = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float fS = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float xA = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        float yA = float.Parse(tokens[i++], CultureInfo.InvariantCulture);
                        if (relative) { xA += curX; yA += curY; }
                        DrawArc(path, curX, curY, rx, ry, phi, fA, fS, xA, yA);
                        curX = xA; curY = yA;
                        break;
                    case 'Z':
                    case 'z': // 闭合路径
                        path.CloseFigure();
                        curX = startX; curY = startY;
                        break;
                }
                lastCmd = cmd;
            }
            return path;
        }

        /// <summary>向路径添加符合 SVG 规范的椭圆弧</summary>
        private void DrawArc(GraphicsPath path, float x1, float y1, float rx, float ry,
                             float phi, float fA, float fS, float x2, float y2)
        {
            if (Math.Abs(x1 - x2) < 1e-6 && Math.Abs(y1 - y2) < 1e-6) return;
            rx = Math.Abs(rx); ry = Math.Abs(ry);
            if (rx < 1e-6 || ry < 1e-6) { path.AddLine(x1, y1, x2, y2); return; }

            double phiRad = phi * Math.PI / 180.0;
            double cosPhi = Math.Cos(phiRad), sinPhi = Math.Sin(phiRad);
            double dx = (x1 - x2) / 2.0, dy = (y1 - y2) / 2.0;
            double x1p = cosPhi * dx + sinPhi * dy;
            double y1p = -sinPhi * dx + cosPhi * dy;

            double lam = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry);
            if (lam > 1) { double s = Math.Sqrt(lam); rx *= (float)s; ry *= (float)s; }

            double rxSq = rx * rx, rySq = ry * ry;
            double num = rxSq * rySq - rxSq * y1p * y1p - rySq * x1p * x1p;
            num = Math.Max(0, num);
            double coeff = (fA == fS ? -1 : 1) * Math.Sqrt(num / (rxSq * y1p * y1p + rySq * x1p * x1p));
            double cxp = coeff * rx * y1p / ry;
            double cyp = coeff * (-ry * x1p / rx);
            double cx = cosPhi * cxp - sinPhi * cyp + (x1 + x2) / 2.0;
            double cy = sinPhi * cxp + cosPhi * cyp + (y1 + y2) / 2.0;

            double th1 = Angle(1, 0, (x1p - cxp) / rx, (y1p - cyp) / ry);
            double dth = Angle((x1p - cxp) / rx, (y1p - cyp) / ry, (-x1p - cxp) / rx, (-y1p - cyp) / ry);
            dth %= 2 * Math.PI;
            if (fS == 0 && dth > 0) dth -= 2 * Math.PI;
            else if (fS == 1 && dth < 0) dth += 2 * Math.PI;

            float startAngle = (float)(th1 * 180.0 / Math.PI);
            float sweepAngle = (float)(dth * 180.0 / Math.PI);
            RectangleF rect = new RectangleF((float)(cx - rx), (float)(cy - ry), (float)(2 * rx), (float)(2 * ry));
            path.AddArc(rect, startAngle, sweepAngle);
        }

        /// <summary>计算两个向量之间的夹角（带符号）</summary>
        private static double Angle(double ux, double uy, double vx, double vy)
        {
            double dot = ux * vx + uy * vy;
            double len = Math.Sqrt(ux * ux + uy * uy) * Math.Sqrt(vx * vx + vy * vy);
            double ang = Math.Acos(Math.Max(-1, Math.Min(1, dot / len)));
            double sign = ux * vy - uy * vx;
            return sign < 0 ? -ang : ang;
        }

        // ==================== 窗口图标生成 ====================

        /// <summary>使用 SVG 路径生成 32x32 的透明背景窗口图标</summary>
        private void CreateWindowIcon()
        {
            int iconSize = 32;
            using (Bitmap bmp = new Bitmap(iconSize, iconSize, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    float scale = (iconSize * 0.85f) / Math.Max(iconBounds.Width, iconBounds.Height);
                    float offsetX = (iconSize - iconBounds.Width * scale) / 2f - iconBounds.X * scale;
                    float offsetY = (iconSize - iconBounds.Height * scale) / 2f - iconBounds.Y * scale;

                    g.TranslateTransform(offsetX, offsetY);
                    g.ScaleTransform(scale, scale);

                    using (Pen p = new Pen(Color.RosyBrown, 3f / scale))
                        g.DrawPath(p, circlePath);
                    using (SolidBrush b = new SolidBrush(Color.LawnGreen))
                        g.FillPath(b, complexPath);
                }

                IntPtr hIcon = bmp.GetHicon();
                windowIcon = Icon.FromHandle(hIcon);
            }
            this.Icon = windowIcon;
        }

        // ==================== 动画与绘制 ====================

        /// <summary>启动动画定时器</summary>
        private void StartAnimation()
        {
            lastTickTime = DateTime.Now;
            lastColorChangeTime = DateTime.Now;
            animationTimer = new System.Windows.Forms.Timer { Interval = 20 }; // 约50fps
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }

        /// <summary>定时器回调：更新旋转角度、文字透明度、边框颜色</summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double elapsed = (now - lastTickTime).TotalSeconds;
            lastTickTime = now;

            float rotSpeed = 360f / 1.2f;
            rotationAngle -= (float)(rotSpeed * elapsed);
            rotationAngle = (rotationAngle % 360 + 360) % 360;

            double phase = (now.Ticks / (double)TimeSpan.TicksPerMillisecond) % 1000 / 500.0;
            if (phase > 1) phase = 2 - phase;
            textOpacity = 0.3f + (float)(phase * 0.7f);

            if ((now - lastColorChangeTime).TotalSeconds >= 0.3)
            {
                lastColorChangeTime = now;
                borderColorIndex = (borderColorIndex + 1) % warningColors.Length;
                borderPen.Color = warningColors[borderColorIndex];
            }

            Invalidate();
        }

        /// <summary>重写 OnPaint 方法，绘制所有界面元素</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (GraphicsPath bgPath = GetRoundedRectPath(0))
                g.FillPath(bgBrush, bgPath);

            using (GraphicsPath borderPath = GetRoundedRectPath(BorderInset))
                g.DrawPath(borderPen, borderPath);

            DrawRotatedIcon(g, 55, Height / 2);

            float iconCenterX = 55f;
            float iconHalfSize = 28f;
            float iconRight = iconCenterX + iconHalfSize + 40f;
            float rightBoundary = Width - BorderInset;
            float availableWidth = rightBoundary - iconRight;

            float availableHeight = Height * 0.6f;
            if (AutoFontSize > 0)
            {
                availableHeight = Height * AutoFontSize;
            }

            float fontSize = GetAutoFontSize(g, showText, availableWidth, availableHeight, FontStyle.Bold);

            using (Font font = new Font("Microsoft YaHei", fontSize, FontStyle.Bold))
            {
                int alpha = (int)(textOpacity * 255);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 245, 245, 245)))
                {
                    SizeF textSize = g.MeasureString(showText, font);
                    float textX = iconRight + (availableWidth - textSize.Width) / 2f;
                    if (textX < iconRight) textX = iconRight;
                    float textY = (Height - textSize.Height) / 2f;

                    g.DrawString(showText, font, brush, textX, textY);
                }
            }
        }

        /// <summary>根据可用宽度和高度，通过二分查找计算最大可能的字体大小（点）</summary>
        private float GetAutoFontSize(Graphics g, string text, float maxWidth, float maxHeight, FontStyle style)
        {
            if (string.IsNullOrEmpty(text)) return 1f;

            float minSize = 6f;
            float maxSize = maxHeight * 0.9f;
            float bestSize = minSize;

            for (int i = 0; i < 20; i++)
            {
                float mid = (minSize + maxSize) / 2f;
                using (Font testFont = new Font("Microsoft YaHei", mid, style))
                {
                    SizeF size = g.MeasureString(text, testFont);
                    if (size.Width <= maxWidth && size.Height <= maxHeight)
                    {
                        bestSize = mid;
                        minSize = mid;
                    }
                    else
                    {
                        maxSize = mid;
                    }
                }
            }
            return Math.Max(minSize, bestSize);
        }

        /// <summary>获取圆角矩形路径</summary>
        private GraphicsPath GetRoundedRectPath(float inset)
        {
            int radius = 30;
            float left = inset, top = inset;
            float right = Width - inset, bottom = Height - inset;
            float diameter = (radius - inset) * 2;
            if (diameter < 0) diameter = 0;

            GraphicsPath path = new GraphicsPath();
            path.AddArc(left, top, diameter, diameter, 180, 90);
            path.AddArc(right - diameter, top, diameter, diameter, 270, 90);
            path.AddArc(right - diameter, bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(left, bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>在指定坐标绘制旋转后的 SVG 图标</summary>
        private void DrawRotatedIcon(Graphics g, int centerX, int centerY)
        {
            Matrix oldTransform = g.Transform;
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(rotationAngle);

            float desiredSize = 56f;
            float scale = desiredSize / Math.Max(iconBounds.Width, iconBounds.Height);
            g.TranslateTransform(-iconBounds.Width / 2 * scale, -iconBounds.Height / 2 * scale);
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(-iconBounds.X, -iconBounds.Y);

            iconStrokePen.Width = 3f / scale;
            g.DrawPath(iconStrokePen, circlePath);
            g.FillPath(iconFillBrush, complexPath);
            g.Transform = oldTransform;
        }

        // ==================== 任务控制（Task + 内部 Thread） ====================

        /// <summary>
        /// 记录调试日志，自动添加类名前缀，并同时输出中英文。
        /// </summary>
        private void LogDebug(string chineseMessage, string englishMessage, Exception ex = null)
        {
            string log = $"[{nameof(AxisGoHome)}] {chineseMessage} / {englishMessage}";
            if (ex != null)
                log += $": {ex.Message}";
            System.Diagnostics.Debug.WriteLine(log);
        }

        /// <summary>
        /// 取消并终止后台回零任务。
        /// 先尝试协作取消，如果线程未及时退出则强制 Abort。
        /// </summary>
        private void TaskCancel()
        {
            try
            {
                // 1. 发出协作取消请求
                cts?.Cancel();

                // 2. 如果工作线程还活着，等待一小段时间后强制终止
                if (workerThread != null && workerThread.IsAlive)
                {
                    // 给线程 100ms 响应取消
                    if (!workerThread.Join(100))
                    {
                        // 线程未响应，强制终止（仅 .NET Framework 有效）
                        try
                        {
                            workerThread.Abort();
                            // 等待线程真正结束，最多 3 秒，避免无限阻塞 UI
                            if (!workerThread.Join(3000))
                            {
                                LogDebug(HTranslation.GetContent("强制终止线程超时，后台线程可能仍在运行。"),
                                         "Timeout while aborting thread, the worker thread may still be running.");
                            }
                        }
                        catch (PlatformNotSupportedException)
                        {
                            LogDebug(HTranslation.GetContent("当前环境不支持 Thread.Abort，无法强制终止线程。"),
                                     "Thread.Abort is not supported in the current environment, cannot force terminate thread.");
                            // 如果环境不支持 Abort，等待线程自然结束（最多 3 秒）
                            if (!workerThread.Join(3000))
                            {
                                LogDebug(HTranslation.GetContent("等待线程自然结束超时。"), "Timeout waiting for thread to finish naturally.");
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            // 线程被 Abort 时，Join 可能抛出此异常，可忽略
                        }
                    }
                    workerThread = null;
                }

                // 3. 等待外层 Task 结束（内部线程已停止，Task 应很快完成）
                if (task != null)
                {
                    try
                    {
                        task.Wait(1000); // 最多等 1 秒
                    }
                    catch (AggregateException)
                    {
                        // 忽略任务中的异常
                    }
                    finally
                    {
                        task.Dispose();
                        task = null;
                    }
                }

                // 4. 释放 CancellationTokenSource
                cts?.Dispose();
                cts = null;
            }
            catch (Exception ex)
            {
                LogDebug(HTranslation.GetContent("取消任务时出错"), "Error while cancelling task", ex);
            }
        }

        /// <summary>记录任务开始的时刻，用于超时判断</summary>
        private DateTime oldDateTime = DateTime.Now;

        /// <summary>ShowAction 专用定时器</summary>
        private System.Windows.Forms.Timer timer;

        /// <summary>
        /// 启动后台回零检测任务。
        /// 外层使用 Task 管理流程，内部使用独立 Thread 执行回零循环。
        /// </summary>
        private void Run()
        {
            bool ok = true;
            isLoad = true;
            TaskCancel(); // 确保上一个任务已清理

            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            // 清理旧 timer，避免重复创建
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            timer = new System.Windows.Forms.Timer { Interval = 500 };
            timer.Tick += Timer_Tick;
            timer.Start();

            task = Task.Run(() =>
            {
                // 创建实际执行回零的线程
                workerThread = new Thread(() =>
                {
                    try
                    {
                        // 等待窗口句柄创建
                        while (!this.IsHandleCreated && !token.IsCancellationRequested)
                            Thread.Sleep(10);
                        if (token.IsCancellationRequested) return;

                        oldDateTime = DateTime.Now;

                        // 循环调用回零检测函数，直到成功或超时或取消
                        while (!token.IsCancellationRequested)
                        {
                            if (FuncGoHome != null)
                            {
                                ok = FuncGoHome();
                            }

                            Thread.Sleep(20);

                            if (ok) break;
                            if (GoHomeTimeout > 0)
                            {
                                if ((DateTime.Now - oldDateTime).TotalSeconds > GoHomeTimeout)
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        // 线程被强制终止，让异常继续传播，确保线程真正结束
                        // 注意：不调用 Thread.ResetAbort()
                    }
                    catch (Exception ex)
                    {
                        LogDebug(HTranslation.GetContent("回零线程异常"), "Worker thread exception", ex);
                    }
                });

                workerThread.IsBackground = true;
                workerThread.Start();

                // 等待工作线程结束（可能被 Abort）
                workerThread.Join();

                // 线程结束后，在 UI 线程上关闭窗口
                if (isLoad && this.IsHandleCreated)
                {
                    if (timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                        timer = null;
                    }
                    BeginInvoke(new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            DialogResult = ok ? DialogResult.OK : DialogResult.Cancel;
                            Close();
                        }
                    }));
                }
            }, token);
        }

        /// <summary>定时器回调，触发 ShowAction</summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            ShowAction?.Invoke(this);
        }

        /// <summary>双击窗口任意位置取消回零并关闭</summary>
        private void AxisGoHome_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
            isLoad = false;
            TaskCancel();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>释放所有资源（GDI 对象、定时器、任务）</summary>
        protected override void Dispose(bool disposing)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
            if (disposing)
            {
                TaskCancel();
                animationTimer?.Stop();
                animationTimer?.Dispose();
                circlePath?.Dispose();
                complexPath?.Dispose();
                bgBrush?.Dispose();
                iconFillBrush?.Dispose();
                iconStrokePen?.Dispose();
                borderPen?.Dispose();
                windowIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
