using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using HFromUI.HMath;
namespace HFromUI.HControl.Chart
{
    using HFromUI.HLangage;
    using HFromUI.HControl.Chart.Tips;
    /// <summary>
    /// HChart —— HFromUI 自研的 WinForms 图表控件，API 镜像 System.Windows.Forms.DataVisualization.Charting.Chart。
    /// 一个 Chart 可以拥有多个 ChartArea（多图表布局）、多个 Series、多个 Legend、多个 Title。
    /// 渲染基于 GDI+ 自绘，支持 Line/Column/Pie/Area/Bar/Scatter/Spline/StepLine/Radar/Candlestick/BoxPlot 等。
    /// </summary>
    public partial class HChart : Panel
    {
        // ===== 集合属性 =====
        /// <summary>Chart 配色方案。</summary>
        public HChartPalette Palette { get; set; } = HChartPalette.Default;
        /// <summary>快捷访问主 ChartArea（ChartAreas["ChartArea1"]）。</summary>
        public HChartArea ChartArea => ChartAreas["ChartArea1"];
        /// <summary>所有 Series 的集合（Chart.Series）。</summary>
        public HChartSeriesCollection Series { get; }
        /// <summary>所有 ChartArea 的集合。至少有一个 ChartArea 才能渲染。</summary>
        public HChartAreaCollection ChartAreas { get; }
        /// <summary>Legend 集合（默认有一个）。</summary>
        public HChartLegendCollection Legends { get; }
        /// <summary>Title 集合（默认为空，可添加）。</summary>
        public HChartTitleCollection Titles { get; }
        // ===== 右键菜单（实现提取至 HFrom\Chart\Menu\HChartMenu.cs） =====
        /// <summary>右键菜单实例（HChartMenu 嵌套类：构建/状态同步）。</summary>
        private HChartMenu _menu;
        // ===== 构造 =====
        /// <summary>构造图表控件：开启双缓冲/自绘/重绘重排，初始化默认 ChartArea、菜单、主题与字体。</summary>
        public HChart()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint
                | ControlStyles.Selectable, true);
            TabStop = true;
            ChartAreas = new HChartAreaCollection();
            ChartAreas.Add(new HChartArea { Name = "ChartArea1" });
            Series = new HChartSeriesCollection(ChartAreas[0]);
            Legends = new HChartLegendCollection();
            Legends.Add(new HChartLegend { Title = "" });
            Titles = new HChartTitleCollection();
            BackColor = Color.FromArgb(240, 242, 245); // 整体控件浅灰底，ChartArea 白色内板，层次分明
            Size = new Size(600, 400);
            // 右键菜单（提取至 HFrom\Chart\Menu\HChartMenu.cs，构造时自动挂载到本控件）
            _menu = new HChartMenu(this);
            // 实时数据：后台线程只加数据并置脏标记，100ms Timer 在 UI 线程轮询消费重绘（仿 HCoordinate）
            _realtimeTimer = new Timer { Interval = 100 };
            _realtimeTimer.Tick += RealtimeTimer_Tick;
            _realtimeTimer.Start();
        }
        /// <summary>实时暂停期间的待刷点缓存：暂停（框选/缩放/菜单暂停）时新点不进系列、先存这里，
        /// 点"继续实时刷新"时按采集顺序一次性补进各系列，数据不丢不删。
        /// 点的写入与缓存都在 UI 线程（AppendPoint 自动切回），无需加锁。</summary>
        private readonly List<PendingPoint> _pendingPoints = new List<PendingPoint>();
        private struct PendingPoint { public HChartSeries Series; public HChartDataPoint Point; }
        /// <summary>实时刷新脏标记（仿 HCoordinate.IsRefresh）：后台线程追加数据后置 true；
        /// 内部 Timer 轮询到 true 时置 false 并 Invalidate 重绘；外部也可手动置 true 请求一次重绘。</summary>
        private volatile bool _isRefresh;
        public bool IsRefresh
        {
            get => _isRefresh;
            set => _isRefresh = value;
        }
        /// <summary>轮询脏标记的 UI Timer（100ms）。</summary>
        private readonly Timer _realtimeTimer;
        private void RealtimeTimer_Tick(object sender, EventArgs e)
        {
            if (_isRefresh)
            {
                _isRefresh = false;
                Invalidate();
            }
        }
        /// <summary>
        /// 线程安全追加点（后台采集线程直接调用，历史点不删除，可累积到 10 万级）：
        /// X 坐标按所属区域 AxisX.TimeFormat 决定——为空用序号 index，非空用时间 time 的 OADate。
        /// 后台线程调用时统一切回 UI 线程写入（重绘/菜单/命中枚举都在 UI 线程），杜绝并发改集合；
        /// 实时暂停期间点不进图表、先进内部待刷列表，点"继续实时刷新"时一次性补绘。
        /// </summary>
        public void AppendPoint(HChartSeries sr, int index, DateTime time, double y)
        {
            if (IsHandleCreated && InvokeRequired)
            {
                BeginInvoke((Action)(() => AppendPointCore(sr, index, time, y)));
                return;
            }
            AppendPointCore(sr, index, time, y);
        }
        private void AppendPointCore(HChartSeries sr, int index, DateTime time, double y)
        {
            var ca = ChartAreas.FirstOrDefault(a => a.Name == sr.ChartArea) ?? ChartArea;
            double x = string.IsNullOrEmpty(ca.AxisX.TimeFormat) ? index : time.ToOADate();
            var pt = new HChartDataPoint(x, y);
            if (IsLiveMode && IsLivePaused)
                _pendingPoints.Add(new PendingPoint { Series = sr, Point = pt }); // 暂停中：进待刷列表，图表不动
            else
            {
                sr.Points.Add(pt);
                _isRefresh = true;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _realtimeTimer.Dispose();
            base.Dispose(disposing);
        }
        // ===== 交互字段 =====
        /// <summary>坐标系映射器（世界坐标 ↔ 屏幕坐标），由 HFromUI.HMath.HScreen 提供。</summary>
        private readonly HScreen _screen = new HScreen();
        /// <summary>当前渲染 ChartArea 的类别标签（类别轴用）。</summary>
        private readonly List<string> _catLabels = new List<string>();
        /// <summary>当前渲染 ChartArea 是否为横向条形（Bar）模式：类别在纵轴、数值在横轴。</summary>
        private bool _barMode;
        /// <summary>当前渲染 ChartArea 的类别数量。</summary>
        private int _catCount;
        /// <summary>堆叠面积图上下文（按共享 X 对齐的累计上下包络）。</summary>
        private readonly StackCtx _areaStack = new StackCtx();
        /// <summary>堆叠柱状图上下文（按类别索引对齐的累计上下包络）。</summary>
        private readonly StackCtx _colStack = new StackCtx();
        /// <summary>堆叠系列上下文：Lower/Upper 为每个系列在各槽位上的累计下界/上界值。</summary>
        private class StackCtx
        {
            /// <summary>本次绘制是否存在堆叠系列（PrepareStacks 计算后置 true，绘制方法据此决定是否使用包络）。</summary>
            public bool Active;
            /// <summary>堆叠对齐用的共享 X 值数组（面积堆叠按实际 X 值，柱状堆叠按类别槽位索引）。</summary>
            public double[] Xs;
            /// <summary>每个系列在各 X/槽位上的累计下包络值（该系列底部）。</summary>
            public readonly Dictionary<HChartSeries, double[]> Lower = new Dictionary<HChartSeries, double[]>();
            /// <summary>每个系列在各 X/槽位上的累计上包络值（该系列顶部）。</summary>
            public readonly Dictionary<HChartSeries, double[]> Upper = new Dictionary<HChartSeries, double[]>();
        }
        /// <summary>附加 Y 轴（ca.YAxes 各根）专用坐标映射器列表：与 _screen 共享 X 映射，Y 缩放各自独立；
        /// 下标与 ca.YAxes 一一对应，SetupScreens 时按需要自动扩容。</summary>
        private readonly List<HScreen> _yScreens = new List<HScreen>();
        /// <summary>当前渲染上下文 轴对象 → 映射器 对照表（SetupScreens 重建；未命中回退主映射器 _screen）。</summary>
        private readonly Dictionary<HChartAxis, HScreen> _screenByAxis = new Dictionary<HChartAxis, HScreen>();
        /// <summary>多 Y 轴布局回退常量：首次绘制前（无实测缓存）每根侧向 Y 轴的默认像素带宽。
        /// 正常渲染时带宽由 MeasureYAxisBands 按刻度数字实测宽度动态计算——数字越长带宽越大、
        /// 坐标系向内压缩，标题/单位始终完整、刻度数字绝不缩字号或截断。</summary>
        private const float YAxisBand = 56f;
        /// <summary>Y 轴刻度标签的回退默认宽度（无实测缓存时使用）。</summary>
        private const float YTickW = 32f;
        /// <summary>Y 轴线 → 刻度标签的间隙（像素）。</summary>
        private const float YTickGap = 4f;
        /// <summary>Y 轴刻度标签 → 竖排标题的间隙（像素，贴近日）。</summary>
        private const float YTitleGap = 2f;
        /// <summary>Y 轴实测带宽缓存：轴 → 轴线向外整带宽度（间隙+刻度数字+标题厚度+外侧余量），
        /// 每次绘制前由 MeasureYAxisBands 重建；命中测试/缩放布局读此缓存与画面保持一致。</summary>
        private readonly Dictionary<HChartAxis, float> _yBand = new Dictionary<HChartAxis, float>();
        /// <summary>Y 轴刻度数字实测最大宽度缓存：轴 → 最宽刻度文字像素宽（横向条形时为最宽类别名）。</summary>
        private readonly Dictionary<HChartAxis, float> _yLabelW = new Dictionary<HChartAxis, float>();
        /// <summary>X 轴标签是否顺时针转 90° 竖排（仅非实时图、横排刻度放不下时置位）：轴 → 是否竖排。</summary>
        private readonly Dictionary<HChartAxis, bool> _xRotate = new Dictionary<HChartAxis, bool>();
        /// <summary>X 轴竖排时的标签带宽：轴 → 最长刻度文字像素宽（即旋转后向下延伸的高度）。</summary>
        private readonly Dictionary<HChartAxis, float> _xRotBandW = new Dictionary<HChartAxis, float>();
        /// <summary>取 Y 轴刻度数字列实测宽度（缓存缺失回退默认 YTickW）。</summary>
        private float YLabelWidthOf(HChartAxis ay)
            => _yLabelW.TryGetValue(ay, out var w) ? w : YTickW;
        /// <summary>取 Y 轴实测整带宽度（缓存缺失回退默认 YAxisBand）。</summary>
        private float YBandWidthOf(HChartAxis ay)
            => _yBand.TryGetValue(ay, out var b) ? b : YAxisBand;
        /// <summary>第 yi 根 Y 轴的轴线相对 inner 边缘向外的像素偏移：同侧（同奇偶索引）各轴带宽自内向外累加。</summary>
        private float YAxisOffset(HChartArea ca, int yi)
        {
            float off = 0f;
            for (int k = 0; k < yi; k++)
            {
                if (YAxisSide(k) != YAxisSide(yi)) continue;
                var ak = YAxisAt(ca, k);
                if (ak.IsEnabled) off += YBandWidthOf(ak);
            }
            return off;
        }
        /// <summary>ChartArea 的 Y 轴总数（全部轴都在 ca.YAxes 列表中，[0] 为主轴）。</summary>
        private static int YAxisCount(HChartArea ca) => ca.YAxes.Count;
        /// <summary>按轴组索引取 Y 轴对象：0=主 Y（左1），1=右1，2=左2，3=右2……（与 Series.YAxisIndex 对应）。</summary>
        private static HChartAxis YAxisAt(HChartArea ca, int yi)
            => yi >= 0 && yi < ca.YAxes.Count ? ca.YAxes[yi] : ca.YAxes[0];
        /// <summary>轴组索引 → 所在侧：+1 右侧、-1 左侧（奇数索引在右、偶数在左，自内向外交替）。</summary>
        private static int YAxisSide(int yi) => (yi & 1) == 1 ? 1 : -1;
        /// <summary>轴组索引 → 在该侧的序号（0=贴绘图区，1=向外第二根，依此类推）。</summary>
        private static int YAxisOrdinal(int yi) => yi / 2;
        /// <summary>反查轴对象的轴组索引（未找到按主轴 0 处理）。</summary>
        private static int YAxisIndexOf(HChartArea ca, HChartAxis ay)
        {
            for (int i = 0; i < ca.YAxes.Count; i++)
                if (ReferenceEquals(ay, ca.YAxes[i])) return i;
            return 0;
        }
        /// <summary>按轴对象取对应坐标映射器（附加轴走各自独立映射，其余回退主映射器 _screen）。</summary>
        private HScreen ScreenFor(HChartAxis ay)
            => _screenByAxis.TryGetValue(ay, out var scr) ? scr : _screen;
        /// <summary>右键/中键拖拽 Pan：拖拽起始坐标。</summary>
        private Point _dragStart;
        /// <summary>拖拽 Pan 中标记。</summary>
        private bool _dragging;
        /// <summary>触发 Pan 的鼠标按键（右键或中键）。</summary>
        private MouseButtons _panButton;
        /// <summary>右键按下后若发生超过阈值的 Pan 拖动，松开时抑制一次上下文菜单弹出。</summary>
        private bool _suppressMenuOnce;
        /// <summary>
        /// 轴带拖动平移状态：在 Y 轴标签带内上下拖动只平移该 Y 轴、在 X 轴标签带内左右拖动只平移 X 轴。
        /// 拖动只平移量程、跨度始终不变（像拖放坐标系本身，与画布右键平移同一方向感）；改变比例只走滚轮/框选。
        /// 每帧都从按下时的起始量程一次性换算（不逐帧累积），鼠标回到按下点即精确还原。
        /// </summary>
        private class AxisDragState
        {
            /// <summary>所属图表区。</summary>
            public HChartArea CA;
            /// <summary>被拖的轴。</summary>
            public HChartAxis Axis;
            /// <summary>true=X 轴横向拖动；false=Y 轴纵向拖动。</summary>
            public bool IsX;
            /// <summary>按下点（控件客户区坐标）。</summary>
            public Point Start;
            /// <summary>按下时轴量程（平移基准，拖动中跨度 StartMax-StartMin 保持不变）。</summary>
            public double StartMin, StartMax;
            /// <summary>按下时坐标系像素宽/高（像素拖动量→世界平移量的换算系数）。</summary>
            public float InnerW, InnerH;
            /// <summary>被拖 Y 轴的组序号（取自动量程做平移边界）；X 轴为 -1。</summary>
            public int YIndex;
        }
        /// <summary>当前轴带拖动平移状态；未拖动为 null。</summary>
        private AxisDragState _axisDrag;
        /// <summary>图表交互模式（无 / 框选放大 / 选中标记 / 竖线标尺 / 横线标尺），由右键菜单切换。</summary>
        private enum InteractMode { None, RubberBand, Mark, RulerV, RulerH }
        /// <summary>当前交互模式。</summary>
        private InteractMode _mode = InteractMode.None;
        /// <summary>框选放大 / 选中标记：左键按下拖框中标记。</summary>
        private bool _rbDragging;
        /// <summary>拉框矩形起点/终点（控件客户区坐标）。</summary>
        private Point _rbStart, _rbEnd;
        /// <summary>选中标记模式松手后保留在图上的矩形框（客户区坐标，Empty=无）；只画框、不改变量程。
        /// 框选放大模式松手即缩放量程，不保留此框。</summary>
        private RectangleF _rbCommitted = RectangleF.Empty;
        /// <summary>鼠标当前位置（标尺跟随用；离开控件置空）。</summary>
        private Point? _mousePos;
        // ===== 缩放/Pan 边界 clamp =====
        /// <summary>ChartArea 自动量程缓存（ComputeAxis 每次写入；ZoomAxis 等做缩放钳制用）。</summary>
        private readonly Dictionary<string, AutoRange> _autoMinMax = new Dictionary<string, AutoRange>();
        /// <summary>
        /// ComputeAxis 数据统计缓存：拖动/缩放锚点迭代一帧内会重复调 ComputeAxis 十几次，
        /// 数据扫描（实时图可累积 10 万点 × 多系列）是主要卡顿源。签名（系列类型/启用/点数/首尾值/实时状态）
        /// 不变时直接复用扫描结果与堆叠包络，跳过全部逐点循环。
        /// </summary>
        private sealed class AreaDataCache
        {
            public int Sig;
            public bool BarMode, AnyAxis, HasCategories;
            public readonly List<string> CatLabels = new List<string>();
            public int CatCount;
            public bool LiveFollow;
            public double LiveLo, LiveHi;
            public double HMin, HMax;
            public double[] VMin, VMax;
            public bool[] HasY, ZeroBaseline;
            public int YCount;
            public readonly StackCtx AreaStack = new StackCtx();
            public readonly StackCtx ColStack = new StackCtx();
        }
        private readonly Dictionary<string, AreaDataCache> _areaDataCache = new Dictionary<string, AreaDataCache>();
        /// <summary>把堆叠上下文快照复制到目标（字典按系列引用为键，O(系列数)）。</summary>
        private static void CopyStackCtx(StackCtx src, StackCtx dst)
        {
            dst.Active = src.Active;
            dst.Xs = src.Xs;
            dst.Lower.Clear(); dst.Upper.Clear();
            foreach (var kv in src.Lower) dst.Lower[kv.Key] = kv.Value;
            foreach (var kv in src.Upper) dst.Upper[kv.Key] = kv.Value;
        }
        // ===== 挂起的缩放锚点 =====
        // 量程改变（滚轮/轴带/框选）后先记录"屏幕点 ↔ 世界点"，在下次绘制管线内实测轴带宽并
        // 迭代平移到几何收敛，保证最终渲染帧上该世界点恰好停在原屏幕位置。
        // 不能在交互回调里立即测量：平移量程会改变极值刻度文字、带宽跟着变，立即测与最终帧差一个状态。
        private HChartArea _anchorCA;
        private float _anchorSX, _anchorSY;
        private double? _anchorXW;
        private Dictionary<HChartAxis, double> _anchorYW;
        /// <summary>自动量程：X 轴一组 + 每根 Y 轴各一组（未启用的 Y 轴为 NaN；下标为轴组索引）。</summary>
        private class AutoRange
        {
            /// <summary>X 轴自动量程下/上限（数据完整跨度，缩放钳制以此为基准）。</summary>
            public double XMin, XMax;
            /// <summary>各 Y 轴（0=主Y左1，1=右1，2=左2……）的自动量程下限；未启用为 NaN。</summary>
            public readonly List<double> YMin = new List<double>();
            /// <summary>各 Y 轴的自动量程上限；未启用为 NaN。</summary>
            public readonly List<double> YMax = new List<double>();
            /// <summary>按 Y 轴总数扩容并填充 NaN（ComputeAxis 每次重建量程时调用）。</summary>
            public void Reset(int yCount)
            {
                YMin.Clear(); YMax.Clear();
                for (int i = 0; i < yCount; i++) { YMin.Add(double.NaN); YMax.Add(double.NaN); }
            }
        }
        // ===== 标尺（3 击循环：第1击画线A、第2击画线B+双头箭头、第3击全清，之后循环） =====
        /// <summary>竖线标尺已放置的屏幕 X 坐标（0/1/2 条；第 3 次左键点击时清空重来）。</summary>
        private readonly List<float> _rulerVX = new List<float>(2);
        /// <summary>横线标尺已放置的屏幕 Y 坐标（0/1/2 条；第 3 次左键点击时清空重来）。</summary>
        private readonly List<float> _rulerHY = new List<float>(2);
        /// <summary>竖线标尺中点双头箭头（水平）所在的屏幕 Y：可上下拖动，统计弹窗以它为垂直锚点跟随移动。</summary>
        private float _rulerVHandleY;
        /// <summary>横线标尺中点双头箭头（垂直）所在的屏幕 X：可左右拖动，统计弹窗以它为水平锚点跟随移动。</summary>
        private float _rulerHHandleX;
        /// <summary>统计弹窗最近一次绘制的外接矩形（弹窗盖住中间线中段，悬停/按住弹窗视同抓住中间线拖动）。</summary>
        private RectangleF _rulerHudBox = RectangleF.Empty;
        /// <summary>清除所有标尺线（右键菜单 / Esc / ToggleMode 退出标尺模式时调用）。</summary>
        private void ClearRulers() { _rulerVX.Clear(); _rulerHY.Clear(); _rulerDragIndex = -1; _rulerHudBox = RectangleF.Empty; Invalidate(); }
        /// <summary>
        /// 某标尺模式在当前图表上实际画出的线是否为竖线：
        /// 横坐标测量(RulerV)=竖线（量条长/X 值），纵坐标测量(RulerH)=横线（量类别/Y 值）。
        /// 所有图表（含横向条形图）几何形态保持一致，仅统计文案/口径在条形图下走 _barMode 专用分支。
        /// </summary>
        private bool RulerModeDrawsVertical(InteractMode mode)
            => mode == InteractMode.RulerV;
        /// <summary>正在拖动的标尺线：-1=未拖动；0/1=拖动 _rulerVX/_rulerHY 中对应那条线；
        /// -2=抓住两线中点的双头箭头整体拖动（两条线同位移，间距不变；同时箭头把手可垂直/水平拖动改变弹窗位置）。</summary>
        private int _rulerDragIndex = -1;
        /// <summary>整体拖动（_rulerDragIndex=-2）时上一次鼠标位置，用于算两线共同位移。</summary>
        private Point _rulerGrabPos;
        /// <summary>鼠标按住标尺线/中点箭头可拖动的像素容差。</summary>
        private const float RulerGrabTol = 8f;
        /// <summary>滚轮缩放方向模式：同时缩放 XY / 只缩放 X / 只缩放 Y（右键菜单切换）。</summary>
        public HChartWheelZoomMode WheelZoomMode { get; set; } = HChartWheelZoomMode.Both;
        // ===== 实时刷新（#19 Realtime 等动态图） =====
        /// <summary>是否为实时刷新模式（由宿主置位；框选放大后自动暂停刷新）。</summary>
        public bool IsLiveMode { get; set; }
        /// <summary>
        /// 非实时图 X 刻度横排放不下时是否自动顺时针转 90° 竖排（默认 true，竖排后刻度全部完整显示、绝不重叠）。
        /// 多 Y 轴窄绘图区的图（如 demo 21~24：刻度本身短）可由宿主置 false，保持横排抽稀。
        /// </summary>
        public bool XLabelAutoRotate { get; set; } = true;
        /// <summary>实时刷新是否处于暂停状态（框选放大后自动暂停，菜单可恢复）。</summary>
        public bool IsLivePaused { get; private set; }
        /// <summary>实时模式 X 轴是否显示全部历史数据（内部状态，由右键"显示所有数据"置 true、"继续实时刷新"复位）：
        /// true=暂停跟随并把从首点到最新点的全部数据展开总览；默认 false=X 轴只显示最近 LiveWindowSize 的流动窗口（数据不删，窗口外仍保留）。</summary>
        public bool LiveShowAll { get; private set; }
        /// <summary>实时流动窗口大小：时间轴（AxisX.TimeFormat 非空）单位为秒，数字/序号轴单位为点。默认 15。</summary>
        public double LiveWindowSize { get; set; } = 15;
        /// <summary>右键菜单是否允许"图表类型"切换（实时六轴等多轴图不希望被改成柱/饼，宿主可关闭）。默认 true。</summary>
        public bool AllowChartTypeChange { get; set; } = true;
        /// <summary>实时暂停/恢复状态变化事件（宿主可据此停/启 Timer）。</summary>
        public event EventHandler LivePauseChanged;
        /// <summary>暂停实时刷新（框选放大/平移/菜单暂停自动调用；宿主 Timer 回调应检查 IsLivePaused）。
        /// 暂停瞬间把当前实际量程固化为显式量程：视图冻结不动，新点进待刷列表，恢复时再统一清除。</summary>
        public void PauseLive()
        {
            if (!IsLiveMode || IsLivePaused) return;
            foreach (var ca in ChartAreas)
            {
                // 只固化尚无显式量程的轴；框选/缩放/平移已设好的量程保留不覆盖（全部 Y 轴统一遍历列表）
                if (ca.AxisX.Minimum == null) ca.AxisX.Minimum = ca.AxisX.ActualMinimum;
                if (ca.AxisX.Maximum == null) ca.AxisX.Maximum = ca.AxisX.ActualMaximum;
                foreach (var ay in ca.YAxes)
                {
                    if (!ay.IsEnabled) continue;
                    if (ay.Minimum == null) ay.Minimum = ay.ActualMinimum;
                    if (ay.Maximum == null) ay.Maximum = ay.ActualMaximum;
                }
            }
            IsLivePaused = true;
            LivePauseChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
        /// <summary>恢复实时刷新：暂停期间缓存在待刷列表里的点按采集顺序补进各系列（数据不丢），
        /// 并清除缩放/框选/平移/"显示所有数据"留下的固定量程，X 轴回到最近 LiveWindowSize 流动窗口跟随最新数据。</summary>
        public void ResumeLive()
        {
            if (!IsLivePaused) return;
            IsLivePaused = false;
            LiveShowAll = false; // 退出"显示所有数据"总览，回到 LiveWindowSize 流动窗口
            foreach (var pp in _pendingPoints) pp.Series.Points.Add(pp.Point);
            _pendingPoints.Clear();
            foreach (var ca in ChartAreas)
            {
                ca.AxisX.Minimum = ca.AxisX.Maximum = null;
                foreach (var ay in ca.YAxes) { ay.Minimum = null; ay.Maximum = null; }
            }
            LivePauseChanged?.Invoke(this, EventArgs.Empty);
            _isRefresh = true;
            Invalidate();
        }
        /// <summary>实时图右键"显示所有数据"：只切换视图，不影响实时运行——采集/刷新继续，
        /// X 轴每帧按全部历史点重算，新点自动纳入总览、右端持续扩展（数据只增不删）。
        /// 若此前因框选/滚轮/拖放处于暂停态，先继续（补入缓存点、清除固化量程）再进总览；
        /// 点"继续实时刷新"回到 LiveWindowSize 流动窗口。</summary>
        public void ShowAllLiveData()
        {
            if (!IsLiveMode) return;
            if (IsLivePaused) ResumeLive(); // 清掉拖放/框选留下的暂停与固化量程（会先把 LiveShowAll 复位为 false）
            LiveShowAll = true;
            _isRefresh = true;
            Invalidate();
        }
        /// <summary>右键菜单项勾选/取消"显示所有数据"：纯视图开关，采集与实时刷新始终不受影响。
        /// 勾选→全部历史总览（见 ShowAllLiveData）；取消→只清 X 轴固定量程，立即回到最近 LiveWindowSize 流动窗口。</summary>
        public void ToggleLiveShowAll()
        {
            if (!IsLiveMode) return;
            if (LiveShowAll)
            {
                LiveShowAll = false;
                foreach (var ca in ChartAreas) ca.AxisX.Minimum = ca.AxisX.Maximum = null;
            }
            else ShowAllLiveData();
            _isRefresh = true;
            Invalidate();
        }
        /// <summary>鼠标悬停时最近命中的数据点（用于 ToolTip + 高亮）。</summary>
        private HChartHitTestResult _lastHit;
        /// <summary>自绘悬停气泡的文本行（Tips 目录下的 HTipLine，圆角反色配色）；null 表示当前不显示气泡。</summary>
        private List<HTipLine> _tipLines;
        /// <summary>气泡锚点（鼠标客户区坐标，气泡在其右下方展开、越界自动翻侧）。</summary>
        private Point _tipAnchor;
        /// <summary>图例条目缓存（每次 OnPaint 重建），用于 HitTest 图例点击切换显隐。</summary>
        private readonly List<LegendEntry> _legendEntries = new List<LegendEntry>();
        /// <summary>图例滚动偏移（以行为单位）。鼠标滚轮在图例区域时调整。</summary>
        private int _legendScrollOffset = 0;
        /// <summary>图例最大可滚动行数（条目总数超出可见行时由 DrawLegend 计算，滚出窗口的条目不可点击）。</summary>
        private int _legendMaxOffset = 0;
        /// <summary>图例是否可滚动（条目超出可见行时为 true，用于决定是否绘制右侧滚动轨道与滑块）。</summary>
        private bool _legendCanScroll = false;
        /// <summary>图例框整体 Bounds（绝对屏幕坐标），用于 HitTest 命中图例框。</summary>
        private RectangleF _legendBoxBounds = RectangleF.Empty;
        /// <summary>饼图几何缓存：每次绘制后写入，HitTest 读取以保证命中区与画面像素级一致。</summary>
        private readonly Dictionary<HChartSeries, PieGeo> _pieGeoCache = new Dictionary<HChartSeries, PieGeo>();
        /// <summary>饼图几何信息缓存（绘制时写入，供 HitTest 像素级命中判断）。</summary>
        private class PieGeo
        {
            /// <summary>饼图所在图表区矩形。</summary>
            public RectangleF Area;
            /// <summary>饼图外半径（像素）。</summary>
            public float Radius;
            /// <summary>环形图内孔半径（像素）；普通 Pie 为 0。</summary>
            public float InnerR;
        }
        /// <summary>ToolTip 启用开关。</summary>
        public bool EnableToolTip { get; set; } = true;
        /// <summary>悬停气泡位置偏移（像素）：在默认定位（鼠标右下方展开、越界翻转）基础上整体平移，
        /// X 正向右移、Y 正向下移；越界翻转到另一侧时同向平移，最终仍夹在客户区内。默认 (0,0)。</summary>
        public Point TipOffset { get; set; } = Point.Empty;
        /// <summary>滚轮缩放开关的后备字段（对外通过 EnableZoom 属性访问，setter 联动 EnablePan）。</summary>
        private bool _enableZoom;
        /// <summary>
        /// 滚轮缩放启用开关（默认关闭）。启用后：
        ///   - 滚轮直接缩放（不需要 Ctrl）；
        ///   - 同时自动把 EnablePan 打开 —— 这样"启用缩放"就等价于"启用整套缩放+平移交互"；
        ///   - 右键菜单"滚轮缩放"子项点亮可用。
        /// </summary>
        public bool EnableZoom
        {
            get => _enableZoom;
            set
            {
                _enableZoom = value;
                EnablePan = value;  // 启用缩放 → 自动允许拖画布坐标系
                Invalidate();       // 菜单勾状态下次 Opening 刷新
            }
        }
        /// <summary>拖拽平移启用开关（默认关闭；通常由 EnableZoom 联动打开，也可单独设）。</summary>
        public bool EnablePan { get; set; } = false;
        /// <summary>
        /// 最大放大倍数（相对"完整数据量程"的 fit 比例为 1）。滚轮/框选放大时，
        /// 可见量程不会小于 数据量程 / ZoomScaleMax。默认 100000000（最深放大 1 亿倍），
        /// 足以深放到 X/Y 刻度完整显示 TickDecimalPlaces（默认 6）位小数；继续放大由
        /// TickLabelsDistinct 刻度文字唯一性校验兜底（相邻刻度文字会重复时拒绝），
        /// 不会把量程缩到 0/NaN 导致绘制异常（参考 HCoordinate 的 ViewScaleMax）。
        /// </summary>
        public double ZoomScaleMax { get; set; } = 100000000;
        /// <summary>
        /// 最小缩放倍数。缩小时可见量程不会大于 数据量程 / ZoomScaleMin。
        /// 默认 1E-300（等同不限制缩小，可一路缩到世界边界 WorldRangeLimit）；
        /// 如需恢复"最多缩小 50 倍"的旧行为，显式设为 0.02。
        /// </summary>
        public double ZoomScaleMin { get; set; } = 1e-300;
        /// <summary>
        /// 世界坐标绝对边界（±）：平移/缩放后的轴窗口整体不得越出该值（跨度不变、只回推位置）。
        /// 默认 1E300——double 能安全参与运算的极大值，实际等同"无边界"（需求中的 10⁹⁹⁹ 在 double
        /// 里是 Infinity 无法使用，1E300 已大于任何实用量程）；想限制可浏览范围时改小（如 1E6）。
        /// </summary>
        public double WorldRangeLimit { get; set; } = 1e300;
        // ============ 配色主题（9 选 1：整体底色/轴/网格/图例联动变化，颜色差异大）============
        /// <summary>内置配色主题枚举：切换 Theme 时控件底色、ChartArea 底色、轴线/标签/网格/图例配色整体联动。</summary>
        public enum HChartTheme
        {
            /// <summary>默认：白+浅灰，清爽干净。</summary>
            Light,
            /// <summary>深色：深灰底+白字，数字仪表风。</summary>
            Dark,
            /// <summary>夕橙：暖橙白底+赭橙轴，明快有活力。</summary>
            Sunset,
            /// <summary>护眼：米黄底+墨字，长时间看不累。</summary>
            Sand,
            /// <summary>矩阵：纯黑底+荧光绿，赛博风。</summary>
            Matrix,
            /// <summary>纸张：米纸+墨蓝，手账/报告风。</summary>
            Paper,
            /// <summary>晴海：浅冰蓝底+海蓝轴，通透冷静。</summary>
            Aqua,
            /// <summary>草绿：青绿底+墨绿字，清新自然。</summary>
            Grass,
            /// <summary>沙漠：暖棕橙底+深棕字，户外风。</summary>
            Desert
        }
        /// <summary>当前配色主题后备字段（对外通过 Theme 属性访问，setter 触发 ApplyTheme 联动重绘）。</summary>
        private HChartTheme _theme = HChartTheme.Light;
        /// <summary>
        /// 当前配色主题（切换时联动：控件 BackColor、ChartArea BackColor、轴/网格/图例/调色板同步变化）。
        /// 不影响 Series.ChartType（折线/柱/面积等类型切换仍由"图表类型"菜单负责）。
        /// </summary>
        public HChartTheme Theme
        {
            get => _theme;
            set { if (_theme == value) return; _theme = value; ApplyTheme(value); Invalidate(); }
        }
        /// <summary>把主题应用到整个图表：背景、轴、网格、图例。</summary>
        private void ApplyTheme(HChartTheme t)
        {
            // ---- 1) 控件外层 BackColor ----
            switch (t)
            {
                case HChartTheme.Light:    BackColor = Color.FromArgb(240, 242, 245); break;
                case HChartTheme.Dark:     BackColor = Color.FromArgb(32, 34, 38);  break;
                case HChartTheme.Sunset:   BackColor = Color.FromArgb(255, 240, 228); break;
                case HChartTheme.Sand:     BackColor = Color.FromArgb(240, 232, 215); break;
                case HChartTheme.Matrix:   BackColor = Color.FromArgb(0, 0, 0);     break;
                case HChartTheme.Paper:    BackColor = Color.FromArgb(245, 238, 220); break;
                case HChartTheme.Aqua:     BackColor = Color.FromArgb(228, 240, 248); break;
                case HChartTheme.Grass:    BackColor = Color.FromArgb(225, 240, 225); break;
                case HChartTheme.Desert:   BackColor = Color.FromArgb(235, 215, 180); break;
            }
            // ---- 2) ChartArea BackColor + 轴 + 网格 ----
            foreach (var ca in ChartAreas) ApplyThemeToArea(ca, t);
            // ---- 3) 图例（在 HChart.Legends 上） ----
            Color legendBg, legendFg;
            switch (t)
            {
                case HChartTheme.Light:       legendBg = Color.FromArgb(250,250,252);   legendFg = Color.FromArgb(50,50,60);   break;
                case HChartTheme.Dark:        legendBg = Color.FromArgb(55,58,64);      legendFg = Color.FromArgb(220,220,225); break;
                case HChartTheme.Sunset:      legendBg = Color.FromArgb(255,244,234);  legendFg = Color.FromArgb(120,55,18);   break;
                case HChartTheme.Sand:        legendBg = Color.FromArgb(245,238,220);   legendFg = Color.FromArgb(80,70,50);    break;
                case HChartTheme.Matrix:      legendBg = Color.FromArgb(10,20,12);      legendFg = Color.FromArgb(140,240,155); break;
                case HChartTheme.Paper:       legendBg = Color.FromArgb(248,242,225);   legendFg = Color.FromArgb(40,55,95);    break;
                case HChartTheme.Aqua:        legendBg = Color.FromArgb(236,246,251);  legendFg = Color.FromArgb(20,60,90);    break;
                case HChartTheme.Grass:       legendBg = Color.FromArgb(235,248,235);   legendFg = Color.FromArgb(30,80,45);    break;
                case HChartTheme.Desert:      legendBg = Color.FromArgb(245,228,195);   legendFg = Color.FromArgb(90,55,25);    break;
                default: return;
            }
            foreach (var lg in Legends)
            {
                lg.BackColor = legendBg;
                lg.ForeColor = legendFg;
            }
        }
        /// <summary>对单个 ChartArea 应用主题（背景 + 所有轴的 Line/Label/Title/Grid）。</summary>
        private static void ApplyThemeToArea(HChartArea ca, HChartTheme t)
        {
            Color areaBg, axisLine, axisLabel, axisTitle, gridLine;
            switch (t)
            {
                case HChartTheme.Light:
                    areaBg = Color.White;
                    axisLine = Color.FromArgb(120,120,130);
                    axisLabel = Color.FromArgb(50,50,60);
                    axisTitle = Color.FromArgb(30,30,40);
                    gridLine = Color.FromArgb(230,230,235); break;
                case HChartTheme.Dark:
                    areaBg = Color.FromArgb(42,44,48);
                    axisLine = Color.FromArgb(180,180,190);
                    axisLabel = Color.FromArgb(220,220,225);
                    axisTitle = Color.FromArgb(240,240,245);
                    gridLine = Color.FromArgb(60,62,68); break;
                case HChartTheme.Sunset:
                    areaBg = Color.FromArgb(255,248,240);
                    axisLine = Color.FromArgb(200,110,50);
                    axisLabel = Color.FromArgb(130,60,20);
                    axisTitle = Color.FromArgb(100,45,10);
                    gridLine = Color.FromArgb(245,220,198); break;
                case HChartTheme.Sand:
                    areaBg = Color.FromArgb(250,244,228);
                    axisLine = Color.FromArgb(150,135,110);
                    axisLabel = Color.FromArgb(80,70,50);
                    axisTitle = Color.FromArgb(60,50,30);
                    gridLine = Color.FromArgb(225,215,195); break;
                case HChartTheme.Matrix:
                    areaBg = Color.FromArgb(4,6,4);
                    axisLine = Color.FromArgb(60,180,80);
                    axisLabel = Color.FromArgb(120,230,140);
                    axisTitle = Color.FromArgb(160,255,170);
                    gridLine = Color.FromArgb(20,60,30); break;
                case HChartTheme.Paper:
                    areaBg = Color.FromArgb(253,248,235);
                    axisLine = Color.FromArgb(80,95,130);
                    axisLabel = Color.FromArgb(40,55,95);
                    axisTitle = Color.FromArgb(25,40,80);
                    gridLine = Color.FromArgb(230,222,200); break;
                case HChartTheme.Aqua:
                    areaBg = Color.FromArgb(240,248,252);
                    axisLine = Color.FromArgb(50,120,165);
                    axisLabel = Color.FromArgb(25,75,110);
                    axisTitle = Color.FromArgb(15,55,85);
                    gridLine = Color.FromArgb(210,228,240); break;
                case HChartTheme.Grass:
                    areaBg = Color.FromArgb(235,248,235);
                    axisLine = Color.FromArgb(80,130,95);
                    axisLabel = Color.FromArgb(30,80,45);
                    axisTitle = Color.FromArgb(15,60,30);
                    gridLine = Color.FromArgb(210,235,210); break;
                case HChartTheme.Desert:
                    areaBg = Color.FromArgb(248,232,200);
                    axisLine = Color.FromArgb(160,100,55);
                    axisLabel = Color.FromArgb(90,55,25);
                    axisTitle = Color.FromArgb(60,35,15);
                    gridLine = Color.FromArgb(225,205,170); break;
                default: return;
            }
            ca.BackColor = areaBg;
            // 主题色统一应用到 X/Y 轴列表里的每一根轴（含顶部副 X、动态副 Y）
            foreach (var ax in ca.XAxes)
            {
                ax.LineColor = axisLine;
                ax.LabelForeColor = axisLabel;
                ax.TitleForeColor = axisTitle;
                ax.MajorGridColor = gridLine;
            }
            foreach (var ax in ca.YAxes)
            {
                ax.LineColor = axisLine;
                ax.LabelForeColor = axisLabel;
                ax.TitleForeColor = axisTitle;
                ax.MajorGridColor = gridLine;
            }
        }
        /// <summary>重新计算所有 ChartArea 的数据轴范围/间隔并强制重绘（图表专用布局刷新，刻意隐藏基类同名方法）。</summary>
        public new void PerformLayout()
        {
            foreach (var ca in ChartAreas) ComputeAxis(ca);
            Invalidate();
        }
        // ============ 渲染主流程 ============
        /// <summary>自绘入口：先填控件背景，再依次绘制每个 ChartArea（标题/坐标轴/系列/图例/标尺/框选）。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.Clear(BackColor);
            // 首次绘制前记录每个系列的"初始图表类型"（用户代码设置的值），供"还原原图"使用
            foreach (var s in Series) s.CaptureOriginalType();
            // 1. ChartAreas（先 Fill 背景）
            foreach (var ca in ChartAreas)
            {
                if (!ca.IsEnabled) continue;
                DrawChartArea(g, ca, ClientRectangle);
            }
            // 2. Titles（在 ChartArea 背景之上画，不被 Fill 盖掉）
            foreach (var t in Titles) DrawTitle(g, t, ClientRectangle);
            // 注：图例（Legend）已在各 ChartArea 内、基于坐标系绘图区 inner 右上角绘制，
            // 不再相对整个控件 ClientRectangle 绘制，避免越出坐标系/遮挡轴标签。
            // 悬停时不额外画高亮圆圈——tooltip 已提供数据提示。
            // 3. 交互覆盖层：框选放大虚线矩形（含框范围）、竖线/横线标尺与统计信息
            DrawOverlay(g);
            // 4. 鼠标悬停气泡（圆角 HUD 同族冷色微调，压在所有图层最上面）
            DrawTip(g);
        }
        // ============ 交互：OnMouseDown/Up/Move/Wheel + HitTest + ResetZoom ============
        /// <summary>是否存在启用中的图例。交互判定一律实时查此状态，
        /// 不依赖上一帧绘制缓存——图例关闭瞬间（尚未重绘）原区域也不再响应任何图例鼠标行为。</summary>
        private bool IsAnyLegendEnabled()
        {
            foreach (var lg in Legends)
                if (lg.IsEnabled) return true;
            return false;
        }
        /// <summary>在图例条目中查找命中项（仅匹配当前可见、有 Bounds 的条目）。
        /// 图例已关闭时一律返回 null：缓存的旧条目不再拦截点击/悬停。</summary>
        private LegendEntry? HitTestLegend(Point screenPt)
        {
            if (!IsAnyLegendEnabled()) return null;
            var pt = new PointF(screenPt.X, screenPt.Y);
            for (int i = 0; i < _legendEntries.Count; i++)
            {
                var entry = _legendEntries[i];
                if (entry.Bounds.IsEmpty) continue; // 未渲染的（被滚出的）跳过
                if (entry.Bounds.Contains(pt))
                    return entry;
            }
            return null;
        }
        /// <summary>判断鼠标点是否在"启用中"的图例框整体区域内（用于触发滚动等交互）。
        /// 图例关闭后返回 false：滚轮事件原样透传给图表缩放/外层页面，世界坐标缩放不再被吞。</summary>
        private bool IsInLegendBox(Point screenPt)
        {
            if (!IsAnyLegendEnabled()) return false;
            return !_legendBoxBounds.IsEmpty && _legendBoxBounds.Contains(screenPt.X, screenPt.Y);
        }
        /// <summary>鼠标按下：标尺模式放/拖标尺线、框选起点、抓画布平移起点；右键弹菜单（平移中抑制）。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            // 左键点击图例条目 → 切换显隐（就地灰化，位置不变）
            if (e.Button == MouseButtons.Left)
            {
                var hit = HitTestLegend(e.Location);
                if (hit.HasValue)
                {
                    var entry = hit.Value;
                    if (entry.Point != null)
                    {
                        // Pie：如果当前是可见的，且这是最后一个可见的 → 阻止隐藏
                        if (!entry.Point.IsEmpty)
                        {
                            int visibleCount = 0;
                            foreach (var p in entry.Series.Points)
                                if (!p.IsEmpty) visibleCount++;
                            if (visibleCount <= 1) return; // 保留至少一个可见
                        }
                        entry.Point.IsEmpty = !entry.Point.IsEmpty;
                    }
                    else if (entry.Series != null)
                    {
                        entry.Series.IsEnabled = !entry.Series.IsEnabled;
                    }
                    _lastHit = null; // 清除 hover 状态
                    Invalidate();
                    return;
                }
                // 轴标签带内左键拖动 → 只平移该轴（Y 带上下拖、X 带左右拖），不改比例；框选/标尺模式下不抢占
                if (EnableZoom && _mode == InteractMode.None)
                {
                    _axisDrag = HitTestAxisBand(e.Location);
                    if (_axisDrag != null)
                    {
                        Cursor = _axisDrag.IsX ? Cursors.SizeWE : Cursors.SizeNS;
                        HideTip();
                        return;
                    }
                }
                // 框选放大 / 选中标记：左键在绘图区内按下开始拉框
                // （框选放大松手缩放到框选量程；选中标记松手只保留矩形框、不改量程）
                if ((_mode == InteractMode.RubberBand || _mode == InteractMode.Mark)
                    && PointInInner(e.Location, out _))
                {
                    _rbDragging = true;
                    _rbStart = e.Location;
                    _rbEnd = e.Location;
                    HideTip();
                    return;
                }
                // 标尺模式：左键在绘图区内操作。按住已有标尺线附近 → 拖动该线；
                // 点在空处 → 3 击循环：
                // 第1击放置线A（标签在线右侧 1/5 处）、第2击放置线B（两线中点画双头箭头+差值）、
                // 第3击清空全部标尺；第4击起重新从线A开始，以此类推
                if ((_mode == InteractMode.RulerV || _mode == InteractMode.RulerH)
                    && PointInInner(e.Location, out var rHit))
                {
                    var rInner = rHit.Item2;
                    if (RulerModeDrawsVertical(_mode))
                    {
                        // 竖线形态：两条线都在时，按住中点双头箭头 → 两条线整体联动拖动（横向），上下拖动箭头则移动弹窗
                        if (_rulerVX.Count == 2 && RulerPairHit(e.Location, rInner, true))
                        {
                            _rulerDragIndex = -2;
                            _rulerGrabPos = e.Location;
                            HideTip();
                            Cursor = Cursors.SizeAll;
                            return;
                        }
                        // 找最近的竖线：按住即拖动
                        int grab = -1; float best = RulerGrabTol;
                        for (int i = 0; i < _rulerVX.Count; i++)
                        {
                            float d = Math.Abs(_rulerVX[i] - e.X);
                            if (d <= best) { best = d; grab = i; }
                        }
                        if (grab >= 0)
                        {
                            _rulerDragIndex = grab; // 抓住线：不产生新点击，移动时直接拖
                            HideTip();
                            Cursor = Cursors.SizeWE;
                            return;
                        }
                        if (_rulerVX.Count >= 2) _rulerVX.Clear(); // 点空处·第3击：清空重来
                        else
                        {
                            _rulerVX.Add(Math.Max(rInner.Left, Math.Min(rInner.Right, e.X)));
                            // 第2条线落定：中点双头箭头默认在绘图区中线高度（之后可上下拖动改变弹窗位置）
                            if (_rulerVX.Count == 2) _rulerVHandleY = rInner.Top + rInner.Height / 2f;
                        }
                    }
                    else
                    {
                        // 横线形态：两条线都在时，按住中点双头箭头 → 两条线整体联动拖动（纵向），左右拖动箭头则移动弹窗
                        if (_rulerHY.Count == 2 && RulerPairHit(e.Location, rInner, false))
                        {
                            _rulerDragIndex = -2;
                            _rulerGrabPos = e.Location;
                            HideTip();
                            Cursor = Cursors.SizeAll;
                            return;
                        }
                        int grab = -1; float best = RulerGrabTol;
                        for (int i = 0; i < _rulerHY.Count; i++)
                        {
                            float d = Math.Abs(_rulerHY[i] - e.Y);
                            if (d <= best) { best = d; grab = i; }
                        }
                        if (grab >= 0)
                        {
                            _rulerDragIndex = grab;
                            HideTip();
                            Cursor = Cursors.SizeNS;
                            return;
                        }
                        if (_rulerHY.Count >= 2) _rulerHY.Clear();
                        else
                        {
                            _rulerHY.Add(Math.Max(rInner.Top, Math.Min(rInner.Bottom, e.Y)));
                            // 第2条线落定：中点双头箭头默认在绘图区中线宽度（之后可左右拖动改变弹窗位置）
                            if (_rulerHY.Count == 2) _rulerHHandleX = rInner.Left + rInner.Width / 2f;
                        }
                    }
                    HideTip();
                    Invalidate();
                    return;
                }
                // 默认模式：左键在绘图区内按住拖动 → 抓画布平移（同 HCoordinate 手感；
                // 平移时鼠标下的世界坐标保持不变）
                if (EnablePan && _mode == InteractMode.None
                    && PointInInner(e.Location, out _))
                {
                    _dragStart = e.Location;
                    _dragging = true;
                    _panButton = MouseButtons.Left;
                    _suppressMenuOnce = false;
                    Cursor = Cursors.SizeAll;
                    HideTip();
                    return;
                }
            }
            // 右键 / 中键拖拽 Pan（右键单击不拖动则照常弹出上下文菜单）
            if ((e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle) && EnablePan)
            {
                _dragStart = e.Location;
                _dragging = true;
                _panButton = e.Button;
                _suppressMenuOnce = false;
                Cursor = Cursors.SizeAll;
            }
        }
        /// <summary>鼠标松开：结束框选（框选放大）、结束画布平移/标尺拖动；右键松开按抑制标记决定是否弹菜单。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            // 轴带拖动平移结束
            if (_axisDrag != null && e.Button == MouseButtons.Left)
            {
                _axisDrag = null;
                UpdateModeCursor(e.Location);
                return;
            }
            // 标尺拖动结束（单线拖动 0/1，或中点箭头整体拖动 -2）
            if (_rulerDragIndex != -1 && e.Button == MouseButtons.Left)
            {
                _rulerDragIndex = -1;
                UpdateModeCursor(e.Location);
                return;
            }
            // 拉框松开：框选放大 → 把 X/Y 量程缩放到框选区域；选中标记 → 只保留选区框、不改变量程；再拖一次替换旧框
            if (e.Button == MouseButtons.Left && _rbDragging)
            {
                _rbDragging = false;
                var rect = RectangleF.FromLTRB(
                    Math.Min(_rbStart.X, _rbEnd.X), Math.Min(_rbStart.Y, _rbEnd.Y),
                    Math.Max(_rbStart.X, _rbEnd.X), Math.Max(_rbStart.Y, _rbEnd.Y));
                // 拉出 ≥4px 视为有效框；太小的框视为取消
                Tuple<HChartArea, RectangleF> rbArea = null;
                bool valid = (rect.Width >= 4 || rect.Height >= 4)
                    && PointInInner(_rbStart, out rbArea);
                if (_mode == InteractMode.RubberBand)
                {
                    _rbCommitted = RectangleF.Empty; // 框选放大不保留选区框
                    if (valid) ApplyRubberZoom(rbArea.Item1, rect);
                }
                else if (_mode == InteractMode.Mark)
                    _rbCommitted = valid ? rect : RectangleF.Empty;
                Invalidate();
                return;
            }
            if (_dragging && (e.Button == _panButton))
            {
                _dragging = false;
                // 右键发生过实际拖动时 OnMouseMove 已置位 _suppressMenuOnce，菜单 Opening 时取消弹出；
                // 右键单击（未拖动）_suppressMenuOnce 保持 false，上下文菜单正常弹出
                UpdateModeCursor(e.Location);
            }
        }
        /// <summary>鼠标移动：拖标尺线/框选/平移实时重绘；悬停更新光标、tooltip 与数据点高亮。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _mousePos = e.Location;
            // 中点箭头整体拖动：水平方向两条线同位移（夹在 inner 内，间距不变），
            // 垂直方向（竖线标尺）/水平方向（横线标尺）拖动箭头把手，统计弹窗跟随移动
            if (_rulerDragIndex == -2)
            {
                if (PointInInner(e.Location, out var rHit3))
                {
                    var ri = rHit3.Item2;
                    if (RulerModeDrawsVertical(_mode) && _rulerVX.Count == 2)
                    {
                        float lo = Math.Min(_rulerVX[0], _rulerVX[1]), hi = Math.Max(_rulerVX[0], _rulerVX[1]);
                        float d = e.X - _rulerGrabPos.X;
                        d = Math.Max(ri.Left - lo, Math.Min(ri.Right - hi, d)); // 位移夹在 inner 内
                        _rulerVX[0] += d; _rulerVX[1] += d;
                        // 箭头 Y 直接跟随鼠标（夹在 inner 内）：弹窗以此为垂直锚点
                        _rulerVHandleY = Math.Max(ri.Top, Math.Min(ri.Bottom, e.Y));
                    }
                    else if (!RulerModeDrawsVertical(_mode) && _rulerHY.Count == 2)
                    {
                        float lo = Math.Min(_rulerHY[0], _rulerHY[1]), hi = Math.Max(_rulerHY[0], _rulerHY[1]);
                        float d = e.Y - _rulerGrabPos.Y;
                        d = Math.Max(ri.Top - lo, Math.Min(ri.Bottom - hi, d));
                        _rulerHY[0] += d; _rulerHY[1] += d;
                        // 箭头 X 直接跟随鼠标（夹在 inner 内）：弹窗以此为水平锚点
                        _rulerHHandleX = Math.Max(ri.Left, Math.Min(ri.Right, e.X));
                    }
                    _rulerGrabPos = e.Location;
                    Invalidate();
                }
                return;
            }
            // 标尺单线拖动：被抓住的线跟随鼠标（夹在 inner 内），标签/HUD/箭头实时刷新
            if (_rulerDragIndex >= 0)
            {
                if (PointInInner(e.Location, out var rHit2))
                {
                    var ri = rHit2.Item2;
                    if (RulerModeDrawsVertical(_mode) && _rulerDragIndex < _rulerVX.Count)
                        _rulerVX[_rulerDragIndex] = Math.Max(ri.Left, Math.Min(ri.Right, e.X));
                    else if (!RulerModeDrawsVertical(_mode) && _rulerDragIndex < _rulerHY.Count)
                        _rulerHY[_rulerDragIndex] = Math.Max(ri.Top, Math.Min(ri.Bottom, e.Y));
                    Invalidate();
                }
                return;
            }
            // 轴带拖动 = 拖放坐标系（只平移、不改比例）：Y 带上下拖只平移该 Y 轴，X 带左右拖只平移 X 轴；
            // 方向与画布右键平移一致（抓住内容走：上拖量程变小、右拖量程变小），跨度恒定故刻度间隔/比例永不变化。
            // 自由平移不做任何世界范围钳制；缩放只由滚轮/框选处理。
            if (_axisDrag != null && e.Button == MouseButtons.Left)
            {
                var st = _axisDrag;
                double span = st.StartMax - st.StartMin;
                double worldD = st.IsX
                    ? -(e.X - st.Start.X) * span / st.InnerW
                    :  (e.Y - st.Start.Y) * span / st.InnerH;
                if (IsFinite(worldD) && st.InnerW > 0 && st.InnerH > 0)
                {
                    st.Axis.Minimum = st.StartMin + worldD;
                    st.Axis.Maximum = st.StartMax + worldD;
                    ClampToWorldLimit(st.Axis);
                    // 被拖轴标签位数变化会推动 inner：绘制帧内以光标为锚补偿，抓住的内容跟手不跳
                    AnchorAt(st.CA, e.X, e.Y, GetInnerPlotRect(st.CA));
                    Invalidate();
                }
                return;
            }
            if (_dragging && EnablePan)
            {
                var caPt = ScreenToChartArea(e.Location);
                if (caPt != null)
                {
                    var ca = caPt.Item1;
                    var areaPt = caPt.Item2;
                    var startPt = ScreenToChartArea(_dragStart);
                    // 必须仍在按下时的同一 ChartArea 内：跨区时两边 area 原点不同，
                    // 直接相减会得到带偏移的假增量，鼠标下世界点会跳一下
                    if (startPt != null && ReferenceEquals(caPt.Item1, startPt.Item1))
                    {
                        var pan = new PointF(areaPt.X - startPt.Item2.X, areaPt.Y - startPt.Item2.Y);
                        if (Math.Abs(e.X - _dragStart.X) + Math.Abs(e.Y - _dragStart.Y) > 3)
                            _suppressMenuOnce = _suppressMenuOnce || _panButton == MouseButtons.Right;
                        ApplyPan(ca, pan, e.Location);
                        _dragStart = e.Location;
                        Invalidate();
                    }
                }
                return;
            }
            // 框选放大：拉框中实时刷新虚线矩形
            if (_rbDragging)
            {
                _rbEnd = e.Location;
                Invalidate();
                return;
            }
            // 标尺模式：悬停在已有标尺线上显示拖拽方向光标，其余为十字（线本身不跟随鼠标）
            if (_mode == InteractMode.RulerV || _mode == InteractMode.RulerH)
            {
                Cursor = RulerHoverCursor(e.Location);
                Invalidate();
                return;
            }
            // 鼠标悬停在图例条目上：跳过下层数据 HitTest，设置手型光标
            var legendHit = HitTestLegend(e.Location);
            if (legendHit.HasValue)
            {
                if (Cursor != Cursors.Hand) Cursor = Cursors.Hand;
                // 清除可能存在的数据点 hover 状态
                if (_lastHit != null) { _lastHit = null; Invalidate(); }
                HideTip();
                return;
            }
            // 框选放大 / 选中标记模式下绘图区显示十字光标
            if ((_mode == InteractMode.RubberBand || _mode == InteractMode.Mark)
                && PointInInner(e.Location, out _))
            {
                if (Cursor != Cursors.Cross) Cursor = Cursors.Cross;
                if (_lastHit != null) { _lastHit = null; Invalidate(); }
                HideTip();
                return;
            }
            // 不在图例/特殊模式上：恢复默认光标
            if (Cursor != Cursors.Default) Cursor = Cursors.Default;
            // HitTest + ToolTip
            var hit = HitTest(e.Location);
            if (hit == null)
            {
                if (_lastHit != null) { _lastHit = null; Invalidate(); }
                HideTip();
                return;
            }
            if (_lastHit == null ||
                _lastHit.Series != hit.Series ||
                _lastHit.Point != hit.Point)
            {
                _lastHit = hit;
                if (EnableToolTip) ShowToolTip(hit, e.Location);
                Invalidate();
            }
        }
        /// <summary>鼠标离开：隐藏 tooltip、复位光标与悬停高亮。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _mousePos = null;
            if (_lastHit != null) { _lastHit = null; Invalidate(); }
            HideTip();
            if (_mode != InteractMode.None) Invalidate(); // 标尺线随鼠标离开而消失
        }
        /// <summary>Esc 退出框选放大/选中标记/标尺模式。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape && _mode != InteractMode.None)
            {
                _mode = InteractMode.None;
                _rbDragging = false;
                _rbCommitted = RectangleF.Empty; // Esc 退出时清掉选区框
                ClearRulers(); // Esc 退出时也清除所有标尺线
                Cursor = Cursors.Default;
                Invalidate();
                e.Handled = true;
            }
        }
        /// <summary>按当前交互模式刷新光标（拖动结束后调用）。</summary>
        private void UpdateModeCursor(Point pt)
        {
            if ((_mode == InteractMode.RubberBand || _mode == InteractMode.Mark
                || _mode == InteractMode.RulerV || _mode == InteractMode.RulerH)
                && PointInInner(pt, out _))
                Cursor = Cursors.Cross;
            else
                Cursor = Cursors.Default;
        }
        /// <summary>标尺模式悬停光标：在中点箭头上 → 四向拖动光标（横拖两线、竖/横拖弹窗）；
        /// 在单条标尺线容差内 → 水平/垂直双向光标；绘图区空处 → 十字；其余 → 默认。</summary>
        private Cursor RulerHoverCursor(Point pt)
        {
            if (!PointInInner(pt, out var hit)) return Cursors.Default;
            if (RulerModeDrawsVertical(_mode))
            {
                if (_rulerVX.Count == 2 && RulerPairHit(pt, hit.Item2, true)) return Cursors.SizeAll;
                foreach (var x in _rulerVX)
                    if (Math.Abs(x - pt.X) <= RulerGrabTol) return Cursors.SizeWE;
            }
            else
            {
                if (_rulerHY.Count == 2 && RulerPairHit(pt, hit.Item2, false)) return Cursors.SizeAll;
                foreach (var y in _rulerHY)
                    if (Math.Abs(y - pt.Y) <= RulerGrabTol) return Cursors.SizeNS;
            }
            return Cursors.Cross;
        }
        /// <summary>
        /// 两线中点双头箭头的命中判定：竖线标尺时箭头是 _rulerVHandleY 高度处、横跨两线之间的水平线段；
        /// 横线标尺时箭头是 _rulerHHandleX 宽度处、纵跨两线之间的垂直线段（把手位置可拖动）。
        /// 统计弹窗盖住中间线中段，因此弹窗矩形（外扩 2px）内也算命中——悬停光标四向十字、按住即整体拖动。
        /// </summary>
        private bool RulerPairHit(Point pt, RectangleF inner, bool vertical)
        {
            // 弹窗区域视同中间线（弹窗画在中间线上层，可见的"中间线"大部分是弹窗本身）
            var b = _rulerHudBox; b.Inflate(2f, 2f);
            if (b.Width > 0f && b.Contains(pt.X, pt.Y)) return true;
            if (vertical)
            {
                float xa = Math.Min(_rulerVX[0], _rulerVX[1]), xb = Math.Max(_rulerVX[0], _rulerVX[1]);
                return Math.Abs(pt.Y - _rulerVHandleY) <= RulerGrabTol + 6f && pt.X >= xa - 2f && pt.X <= xb + 2f;
            }
            float ya = Math.Min(_rulerHY[0], _rulerHY[1]), yb = Math.Max(_rulerHY[0], _rulerHY[1]);
            return Math.Abs(pt.X - _rulerHHandleX) <= RulerGrabTol + 6f && pt.Y >= ya - 2f && pt.Y <= yb + 2f;
        }
        /// <summary>判断屏幕点是否落在某个笛卡尔 ChartArea 的 inner 绘图区内（返回该区域与 inner）。</summary>
        private bool PointInInner(Point screenPt, out Tuple<HChartArea, RectangleF> hit)
        {
            hit = null;
            foreach (var ca in ChartAreas)
            {
                if (!ca.IsEnabled) continue;
                bool cart = false;
                foreach (var s in Series) if (s.ChartArea == ca.Name && IsCartesian(s.ChartType)) { cart = true; break; }
                if (!cart) continue;
                var inner = GetInnerPlotRect(ca);
                if (inner.Contains(screenPt.X, screenPt.Y))
                {
                    hit = Tuple.Create(ca, inner);
                    return true;
                }
            }
            return false;
        }
        /// <summary>滚轮：EnableZoom 时以鼠标世界坐标为锚点缩放量程（按 WheelZoomMode 决定 X/Y），并受上下限钳制。</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            // 图例框区域优先处理滚动（条目过多时滚动，替代省略截断）；
            // 图例无需滚动时不拦截事件（Ctrl+滚轮缩放、外层页面滚动照常工作）
            if (_legendCanScroll && IsInLegendBox(e.Location))
            {
                int dir = e.Delta > 0 ? -1 : 1; // 向上滚→offset减小，向下→offset增大
                _legendScrollOffset = Math.Max(0, Math.Min(_legendScrollOffset + dir, _legendMaxOffset));
                Invalidate();
                if (e is HandledMouseEventArgs he) he.Handled = true; // 阻止冒泡到外层滚动面板
                return;
            }
            if (!EnableZoom) return;
            var caPt0 = ScreenToChartArea(e.Location);
            if (caPt0 == null) return;
            var ca0 = caPt0.Item1;
            bool zoomIn = e.Delta > 0;
            // 鼠标在左/右 Y 轴标签带内滚动 → 只缩放对应 Y 轴量程（无需 Ctrl）
            var inner0 = GetInnerPlotRect(ca0);
            ComputeAxis(ca0);
            SetupScreens(inner0, ca0);
            // 每根启用的 Y 轴占一个实测标签带（与 DrawAxisY 的轴线偏移/带宽完全同源）：
            // 右侧轴带 [inner.Right + off, +band]；左侧轴带 [inner.Left - off - band, inner.Left - off]
            HChartAxis bandAxis = null;
            int bandYi = -1;
            float mx = e.Location.X;
            for (int yi = 0; yi < YAxisCount(ca0); yi++)
            {
                var ay = YAxisAt(ca0, yi);
                if (!ay.IsEnabled) continue;
                float off = YAxisOffset(ca0, yi);
                float band = YBandWidthOf(ay);
                bool inside = YAxisSide(yi) > 0
                    ? mx > inner0.Right + off && mx <= inner0.Right + off + band
                    : mx >= inner0.Left - off - band && mx < inner0.Left - off;
                if (inside) { bandAxis = ay; bandYi = yi; break; }
            }
            if ((ModifierKeys & Keys.Control) == 0 && bandAxis != null)
            {
                var autoBand = _autoMinMax[ca0.Name];
                // 锚点缩放前记录全部启用轴（别的轴不动也要在 inner 因带宽变化平移时补偿回位）
                double bandXW = ScreenToWorldX(ca0.AxisX, inner0, mx);
                var bandYW = new Dictionary<HChartAxis, double>();
                for (int byi = 0; byi < YAxisCount(ca0); byi++)
                {
                    var bya = YAxisAt(ca0, byi);
                    if (bya.IsEnabled) bandYW[bya] = ScreenToWorldY(bya, inner0, e.Location.Y);
                }
                // 套用与主路径相同的量程钳制（该轴自己的自动量程）；刻度会重复时本次缩放自动放弃
                if (bandYi >= 0 && bandYi < autoBand.YMin.Count)
                    CommitAxisZoom(bandAxis, bandYW[bandAxis], zoomIn ? 0.8 : 1.25,
                        autoBand.YMin[bandYi], autoBand.YMax[bandYi], 8);
                SetPendingAnchor(ca0, mx, e.Location.Y, bandXW, bandYW);
                PauseLive(); // 实时图：轴带缩放后暂停跟随
                Invalidate();
                if (e is HandledMouseEventArgs heY) heY.Handled = true;
                return;
            }
            // 滚轮直接缩放（不用 Ctrl）：按 WheelZoomMode 决定缩放方向（YOnly 时主/副 Y 轴同步缩放）
            ApplyZoom(ca0, e.Location, zoomIn,
                WheelZoomMode != HChartWheelZoomMode.YOnly,
                WheelZoomMode != HChartWheelZoomMode.XOnly);
            PauseLive(); // 实时图：滚轮缩放后暂停跟随，菜单"继续实时刷新"恢复
            Invalidate();
            if (e is HandledMouseEventArgs he2) he2.Handled = true; // 缩放时同样阻止页面滚动
        }
        /// <summary>
        /// 鼠标坐标命中测试：返回最近的 ChartArea + Series + DataPoint。
        /// 距离阈值 default 为 10 像素。
        /// </summary>
        public HChartHitTestResult HitTest(Point screenPt, int tolerance = 10)
        {
            return HitTestCore(screenPt, tolerance);
        }
        /// <summary>点到矩形中心的距离平方（多个柱体同时命中时，取中心离鼠标最近者）。</summary>
        private static double CenterDist2(RectangleF rect, Point pt)
        {
            double dx = (rect.Left + rect.Right) / 2.0 - pt.X;
            double dy = (rect.Top + rect.Bottom) / 2.0 - pt.Y;
            return dx * dx + dy * dy;
        }
        private HChartHitTestResult HitTestCore(Point screenPt, int tolerance = 10)
        {
            foreach (var ca in ChartAreas)
            {
                if (!ca.IsEnabled) continue;
                var rect = GetChartAreaRect(ca);
                if (!rect.Contains(screenPt)) continue;
                // 和 DrawChartArea 一致：先按 Position 偏移得到 area
                var area = new RectangleF(
                    rect.Left + rect.Width * ca.Position.Left / 100f,
                    rect.Top + rect.Height * ca.Position.Top / 100f,
                    rect.Width * ca.Position.Width / 100f,
                    rect.Height * ca.Position.Height / 100f);
                // ===== Pie / Doughnut 专用命中 =====
                foreach (var s in Series)
                {
                    if (!s.IsEnabled || s.ChartArea != ca.Name) continue;
                    if (s.ChartType != HChartType.Pie && s.ChartType != HChartType.Doughnut) continue;
                    double total = 0;
                    foreach (var p in s.Points) if (!p.IsEmpty) total += Math.Abs(p.YValue);
                    if (total <= 0) break;
                    // 与绘制同源：饼/环画在"控件减 Padding"内容框内
                    var freeRect = GetFreeLayoutRect(area);
                    float cx = freeRect.Left + freeRect.Width / 2f;
                    float cy = freeRect.Top + freeRect.Height / 2f;
                    // 优先读取绘制时缓存的几何（半径随标签留白动态变化），保证命中区与画面一致
                    float radius, innerR;
                    PieGeo geo;
                    if (_pieGeoCache.TryGetValue(s, out geo) && geo.Area == freeRect)
                    {
                        radius = geo.Radius;
                        innerR = geo.InnerR;
                    }
                    else
                    {
                        radius = GetPieRadius(freeRect, 0f, 0f, 12f, 4f);
                        innerR = s.ChartType == HChartType.Doughnut ? radius * 0.55f : 0;
                    }
                    double dx = screenPt.X - cx;
                    double dy = screenPt.Y - cy;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist > radius + tolerance || dist < innerR - tolerance) break;
                    // 角度：GDI+ 约定 0°=右边，顺时针增长（与 FillPie 的 startAngle/sweep 一致）
                    double angleRad = Math.Atan2(dy, dx);
                    double angleDeg = angleRad * 180 / Math.PI;
                    if (angleDeg < 0) angleDeg += 360;
                    if (angleDeg >= 360) angleDeg -= 360;
                    float startAngle = -90f;
                    for (int i = 0; i < s.Points.Count; i++)
                    {
                        var p = s.Points[i]; if (p.IsEmpty) continue;
                        float sweep = (float)(Math.Abs(p.YValue) / total * 360f);
                        float sa = startAngle < 0 ? startAngle + 360f : startAngle;
                        float ea = sa + sweep;
                        bool hit;
                        if (ea <= 360f) hit = angleDeg >= sa && angleDeg < ea;
                        else            hit = angleDeg >= sa || angleDeg < ea - 360f;
                        if (hit)
                        {
                            return new HChartHitTestResult
                            {
                                ChartArea = ca, Series = s, Point = p,
                                LocalPoint = new PointF((float)dx, (float)dy)
                            };
                        }
                        startAngle += sweep;
                    }
                    break;
                }
                // ===== Pie 命中结束 =====
                // ===== Funnel 梯形命中 =====
                foreach (var s in Series)
                {
                    if (!s.IsEnabled || s.ChartArea != ca.Name) continue;
                    if (s.ChartType != HChartType.Funnel) continue;
                    var ptsList = s.Points.FindAll(p => !p.IsEmpty);
                    if (ptsList.Count == 0) break;
                    // 与绘制共用同一几何（宽度含标签留白，内容框为"控件减 Padding"），避免命中错位
                    int fn = ptsList.Count;
                    float[] hwArr = GetFunnelGeometry(GetFreeLayoutRect(area), ptsList, out float fcx, out float fTopY, out float fSegH, out _);
                    for (int i = 0; i < fn; i++)
                    {
                        float hwTop = hwArr[i];
                        float hwBot = (i + 1 < fn) ? hwArr[i + 1] : hwTop * 0.7f;
                        float yTop = fTopY + i * fSegH;
                        float yBot = yTop + fSegH;
                        if (screenPt.Y >= yTop - tolerance && screenPt.Y <= yBot + tolerance)
                        {
                            float t = (screenPt.Y - yTop) / fSegH; // 0~1
                            float halfW = hwTop + (hwBot - hwTop) * t;
                            if (Math.Abs(screenPt.X - fcx) <= halfW + tolerance)
                            {
                                return new HChartHitTestResult
                                {
                                    ChartArea = ca, Series = s, Point = ptsList[i],
                                    LocalPoint = new PointF(screenPt.X - area.Left, screenPt.Y - area.Top)
                                };
                            }
                        }
                    }
                    break;
                }
                // ===== Funnel 命中结束 =====
                ComputeAxis(ca);
                var inner = GetInnerPlotRect(ca);
                SetupScreens(inner, ca);
                // 并排柱/条的组数只统计一次（与绘制侧 DrawChartArea 的 colCount/barCount 口径一致）
                var colSeries = Series.Where(x => x.IsEnabled && x.ChartArea == ca.Name && x.ChartType == HChartType.Column).ToList();
                var barSeries = Series.Where(x => x.IsEnabled && x.ChartArea == ca.Name && x.ChartType == HChartType.Bar).ToList();
                // 柱/条严格按柱体几何命中（不做外扩）：并排柱间距只有几像素，外扩 tolerance 会与邻柱
                // 大面积重叠，遍历时先碰到邻系列就报"在 A 上显 B"。同点多候选收集到列表，取柱体中心最近者。
                var barCandidates = new List<Tuple<HChartHitTestResult, double>>();
                foreach (var s in Series)
                {
                    if (!s.IsEnabled || s.ChartArea != ca.Name) continue;
                    if (s.ChartType == HChartType.Column || s.ChartType == HChartType.StackedColumn)
                    {
                        // 此系列在并排组里的排号（StackedColumn 不参与并排）
                        int idx = colSeries.IndexOf(s);
                        float slotW = SlotWidthX(ca.AxisX, inner);
                        float barW = s.ChartType == HChartType.Column && colSeries.Count > 1
                            ? slotW / colSeries.Count * 0.85f : slotW * 0.7f;
                        float groupOffset = s.ChartType == HChartType.Column && colSeries.Count > 1
                            ? -slotW / 2f + barW * idx + barW / 2f : 0;
                        // 系列绑定的 Y 轴（多 Y 轴命中与绘制保持一致）
                        var ayHit = YAxisAt(ca, s.YAxisIndex);
                        for (int i = 0; i < s.Points.Count; i++)
                        {
                            var p = s.Points[i];
                            if (p.IsEmpty) continue;
                            float xPx = MapX(p.XValue, ca.AxisX, inner) + groupOffset;
                            float top, bottom;
                            if (s.ChartType == HChartType.StackedColumn && _colStack.Upper.ContainsKey(s))
                            {
                                top = MapY(_colStack.Upper[s][i], ayHit, inner);
                                bottom = MapY(_colStack.Lower[s][i], ayHit, inner);
                            }
                            else
                            {
                                top = MapY(Math.Max(p.YValue, 0), ayHit, inner);
                                bottom = MapY(Math.Min(p.YValue, 0), ayHit, inner);
                            }
                            if (top > bottom) { float t = top; top = bottom; bottom = t; }
                            // 与 DrawColumnSeries/DrawStackedColumnSeries 完全一致的柱体矩形（无 tolerance 外扩）
                            var barRect = new RectangleF(xPx - barW / 2f, top, barW, bottom - top);
                            if (barRect.Contains(screenPt))
                                barCandidates.Add(Tuple.Create(new HChartHitTestResult
                                {
                                    ChartArea = ca, Series = s, Point = p,
                                    LocalPoint = new PointF(screenPt.X - inner.Left, screenPt.Y - inner.Top)
                                }, CenterDist2(barRect, screenPt)));
                        }
                        continue;
                    }
                    if (s.ChartType == HChartType.Bar)
                    {
                        // Bar：类别在纵轴，数值在横轴
                        int idx = barSeries.IndexOf(s);
                        float slotH = inner.Height / Math.Max(1, _catCount) * 0.9f;
                        float barH = barSeries.Count > 1 ? slotH / barSeries.Count * 0.85f : slotH * 0.7f;
                        float groupOffset = barSeries.Count > 1 ? -slotH / 2f + barH * idx + barH / 2f : 0;
                        float xZero = MapX(0, ca.AxisX, inner);
                        foreach (var p in s.Points)
                        {
                            if (p.IsEmpty) continue;
                            float yPx = MapY(p.XValue, ca.AxisY, inner) + groupOffset;
                            float xVal = MapX(p.YValue, ca.AxisX, inner);
                            float left = Math.Min(xZero, xVal);
                            float w = Math.Abs(xVal - xZero);
                            // 与 DrawBarSeries 完全一致的条体矩形（无 tolerance 外扩）
                            var barRect = new RectangleF(left, yPx - barH / 2f, w, barH);
                            if (barRect.Contains(screenPt))
                                barCandidates.Add(Tuple.Create(new HChartHitTestResult
                                {
                                    ChartArea = ca, Series = s, Point = p,
                                    LocalPoint = new PointF(screenPt.X - inner.Left, screenPt.Y - inner.Top)
                                }, CenterDist2(barRect, screenPt)));
                        }
                        continue;
                    }
                }
                if (barCandidates.Count > 0)
                {
                    HChartHitTestResult barBest = null;
                    double bestD2 = double.MaxValue;
                    foreach (var cand in barCandidates)
                        if (cand.Item2 < bestD2) { bestD2 = cand.Item2; barBest = cand.Item1; }
                    return barBest;
                }
                // 气泡：圆半径随第三维变化、常远大于通用 tolerance，按 DrawBubbleSeries 同一半径做圆内命中
                // （圆边缘也要能点出"气泡大小代表什么"）；多泡重叠取中心距最小者
                HChartHitTestResult bubBest = null;
                double bubBestD2 = double.MaxValue;
                foreach (var s in Series)
                {
                    if (!s.IsEnabled || s.ChartArea != ca.Name || s.ChartType != HChartType.Bubble) continue;
                    var ayBind = YAxisAt(ca, s.YAxisIndex);
                    var ayBub = ayBind.IsEnabled ? ayBind : ca.AxisY;
                    double maxSize = 1;
                    foreach (var pp in s.Points)
                        if (!pp.IsEmpty && pp.YValues.Length > 0) maxSize = Math.Max(maxSize, Math.Abs(pp.YValues[0]));
                    float maxR = Math.Min(inner.Width, inner.Height) * 0.14f;
                    foreach (var p in s.Points)
                    {
                        if (p.IsEmpty) continue;
                        float bcx = MapX(p.XValue, ca.AxisX, inner);
                        float bcy = MapY(p.YValue, ayBub, inner);
                        double size = p.YValues.Length > 0 ? Math.Abs(p.YValues[0]) : maxSize * 0.3;
                        float radius = (float)(maxR * Math.Sqrt(size / maxSize));
                        if (radius < 3) radius = 3;
                        double d2 = (bcx - screenPt.X) * (bcx - screenPt.X) + (bcy - screenPt.Y) * (bcy - screenPt.Y);
                        if (d2 <= radius * radius && d2 < bubBestD2)
                        {
                            bubBestD2 = d2;
                            bubBest = new HChartHitTestResult
                            {
                                ChartArea = ca, Series = s, Point = p,
                                LocalPoint = new PointF(screenPt.X - inner.Left, screenPt.Y - inner.Top)
                            };
                        }
                    }
                }
                if (bubBest != null) return bubBest;
                // 散点/线/面积等：按点到点距离命中
                double minDist = double.MaxValue;
                HChartSeries bestSeries = null;
                HChartDataPoint bestPoint = null;
                foreach (var s in Series)
                {
                    if (!s.IsEnabled || s.ChartArea != ca.Name) continue;
                    var ayHit = YAxisAt(ca, s.YAxisIndex);
                    foreach (var p in s.Points)
                    {
                        if (p.IsEmpty) continue;
                        float px = MapX(p.XValue, ca.AxisX, inner);
                        float py = MapY(p.YValue, ayHit, inner);
                        double dist = Math.Sqrt((px - screenPt.X) * (px - screenPt.X) + (py - screenPt.Y) * (py - screenPt.Y));
                        if (dist < minDist && dist <= tolerance)
                        {
                            minDist = dist;
                            bestSeries = s;
                            bestPoint = p;
                        }
                    }
                }
                if (bestPoint == null) return null;
                return new HChartHitTestResult
                {
                    ChartArea = ca,
                    Series = bestSeries,
                    Point = bestPoint,
                    LocalPoint = new PointF(screenPt.X - inner.Left, screenPt.Y - inner.Top)
                };
            }
            return null;
        }
        /// <summary>重置所有 ChartArea 的 Zoom/Pan（清掉用户量程，回到自适应最合适量程）。</summary>
        public void ResetZoom()
        {
            foreach (var ca in ChartAreas) ResetZoom(ca);
        }
        /// <summary>重置指定 ChartArea 的 Zoom/Pan：清掉 X 轴与全部 Y 轴的用户 Minimum/Maximum，回到自适应。</summary>
        public void ResetZoom(HChartArea ca)
        {
            ca.AxisX.Minimum = null; ca.AxisX.Maximum = null;
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                ay.Minimum = null; ay.Maximum = null;
            }
            Invalidate();
        }
        /// <summary>单轴以 worldCenter 为锚点缩放量程（factor&lt;1 放大、&gt;1 缩小）。量程非法或锚点非有限数时放弃本次缩放。</summary>
        private static void ZoomAxis(HChartAxis ax, double worldCenter, double factor)
        {
            if (!IsFinite(worldCenter) || !IsFinite(factor) || factor <= 0) return;
            double curMin = ax.Minimum ?? ax.ActualMinimum;
            double curMax = ax.Maximum ?? ax.ActualMaximum;
            double delta = curMax - curMin;
            // 量程非有限数或非正：映射器还没准备好（无数据/未计算），放弃，避免产生 NaN 量程
            if (!IsFinite(delta) || delta <= 0) return;
            double newRange = delta * factor;
            if (!IsFinite(newRange) || newRange <= 0) return;
            double rel = (worldCenter - curMin) / delta;
            double newMin = worldCenter - rel * newRange;
            double newMax = worldCenter + (1 - rel) * newRange;
            if (!IsFinite(newMin) || !IsFinite(newMax)) return;
            ax.Minimum = newMin;
            ax.Maximum = newMax;
        }
        /// <summary>
        /// 精确屏幕点 → X 世界坐标：直接用 inner 矩形与轴量程线性换算，
        /// 不走 HScreen.ScreenToWorld（它会按当前缩放级把坐标截断到固定小数位，深放大时锚点会丢像素）。
        /// 滚轮/框选的锚点一律用这组精确换算，保证光标下世界坐标像素级不动。
        /// </summary>
        private static double ScreenToWorldX(HChartAxis ax, RectangleF inner, float screenX)
        {
            double min = ax.Minimum ?? ax.ActualMinimum;
            double max = ax.Maximum ?? ax.ActualMaximum;
            double f = inner.Width > 0 ? (screenX - inner.Left) / inner.Width : 0.5;
            return min + f * (max - min);
        }
        /// <summary>精确屏幕点 → Y 世界坐标（Y 向上为正，屏幕 y 向下）。</summary>
        private static double ScreenToWorldY(HChartAxis ay, RectangleF inner, float screenY)
        {
            double min = ay.Minimum ?? ay.ActualMinimum;
            double max = ay.Maximum ?? ay.ActualMaximum;
            double f = inner.Height > 0 ? (inner.Bottom - screenY) / inner.Height : 0.5;
            return min + f * (max - min);
        }
        /// <summary>.NET Framework 无 double.IsFinite，统一用这个：非 NaN 且非 Infinity。</summary>
        private static bool IsFinite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        /// <summary>
        /// 刻度标签唯一性：给定可见量程，按 ComputeNiceInterval 求整齐等距间隔（1/2/5×10ⁿ——刻度永远等比例），
        /// 逐刻度 FormatTick；相邻刻度出现相同文字（间隔已小于 5 位小数可分辨精度）即返回 false。
        /// 缩放统一出口据此拒绝继续放大，保证屏幕上绝不显示两个一样的刻度数字。
        /// </summary>
        private bool TickLabelsDistinct(double min, double max, int targetTicks, HChartAxis ax = null)
        {
            if (!IsFinite(min) || !IsFinite(max) || max - min <= 0) return false;
            bool timeAxis = ax != null && IsTimeAxis(ax);
            double interval = timeAxis ? ComputeTimeInterval(max - min) : ComputeNiceInterval(max - min, targetTicks);
            double start;
            if (timeAxis)
            {
                double stepSec = interval * 86400.0;
                start = Math.Ceiling(min * 86400.0 / stepSec) * stepSec / 86400.0;
            }
            else start = Math.Ceiling(min / interval) * interval;
            var seen = new HashSet<string>();
            // 正常刻度数≈targetTicks（密度粗调只增不减），上限给到 8 倍防御
            for (int k = 0; k <= targetTicks * 8; k++)
            {
                double v = start + k * interval;
                if (v > max + interval * 1e-6) break;
                string text = timeAxis ? FormatAxisTick(ax, v) : FormatTick(v, ax != null ? ax.TickDecimalPlaces : 6);
                if (!seen.Add(text)) return false;
            }
            return true;
        }
        /// <summary>
        /// 单轴缩放统一出口（滚轮/轴带滚轮/轴带拖动/框选放大共用）：
        /// 锚点等比例缩放 → 自动量程边界钳制 → 刻度唯一性校验；
        /// 新量程会让刻度数字重复时恢复原量程（刻度一旦显示一样，就不再继续缩放）。
        /// </summary>
        private void CommitAxisZoom(HChartAxis ax, double worldCenter, double factor,
            double autoMin, double autoMax, int targetTicks)
        {
            double? oldMin = ax.Minimum, oldMax = ax.Maximum;
            ZoomAxis(ax, worldCenter, factor);
            // 钳制到量程上下限时同样以 worldCenter 为锚点，
            // 否则到顶后继续滚轮会把视图中心（而不是光标）钉住，光标下世界点漂移
            ClampAxisRange(ax, autoMin, autoMax, worldCenter);
            double vMin = ax.Minimum ?? ax.ActualMinimum;
            double vMax = ax.Maximum ?? ax.ActualMaximum;
            if (!TickLabelsDistinct(vMin, vMax, targetTicks, ax)) { ax.Minimum = oldMin; ax.Maximum = oldMax; }
        }
        // ============ 辅助：屏幕坐标 → ChartArea + inner 坐标 ============
        /// <summary>把屏幕点命中到对应 ChartArea，并换算为相对该 ChartArea 的坐标；不在任何区域返回 null。</summary>
        private Tuple<HChartArea, PointF> ScreenToChartArea(Point screenPt)
        {
            foreach (var ca in ChartAreas)
            {
                if (!ca.IsEnabled) continue;
                var rect = GetChartAreaRect(ca);
                if (rect.Contains(screenPt))
                    return Tuple.Create(ca, new PointF(screenPt.X - rect.Left, screenPt.Y - rect.Top));
            }
            return null;
        }
        /// <summary>
        /// 轴拖动带命中：鼠标在某根启用 Y 轴的标签带内（纵向与坐标系同高）→ 上下拖动缩放该 Y 轴；
        /// 在 X 轴底部标签带内（横向与坐标系同宽）→ 左右拖动缩放 X 轴；都不命中返回 null。
        /// 带位置/尺寸与 GetInnerPlotRect 的实测预留、DrawAxisX/Y 完全同源。
        /// </summary>
        private AxisDragState HitTestAxisBand(Point pt)
        {
            var caPt = ScreenToChartArea(pt);
            if (caPt == null) return null;
            var ca = caPt.Item1;
            ComputeAxis(ca);
            var inner = GetInnerPlotRect(ca);
            SetupScreens(inner, ca);
            // Y 轴带：y 与坐标系同高，x 落在各轴实测带内（区间与滚轮轴带缩放同源）
            if (pt.Y >= inner.Top - 2f && pt.Y <= inner.Bottom + 2f)
            {
                for (int yi = 0; yi < YAxisCount(ca); yi++)
                {
                    var ay = YAxisAt(ca, yi);
                    if (!ay.IsEnabled) continue;
                    float off = YAxisOffset(ca, yi);
                    float band = YBandWidthOf(ay);
                    bool inside = YAxisSide(yi) > 0
                        ? pt.X > inner.Right + off && pt.X <= inner.Right + off + band
                        : pt.X >= inner.Left - off - band && pt.X < inner.Left - off;
                    if (inside)
                        return NewAxisDrag(ca, ay, false, pt,
                            ay.Minimum ?? ay.ActualMinimum, ay.Maximum ?? ay.ActualMaximum,
                            inner.Width, inner.Height, yi);
                }
            }
            // X 轴带：x 与坐标系同宽，y 落在 inner 下方的刻度数字/标题带内
            if (ca.AxisX.IsEnabled && pt.X >= inner.Left && pt.X <= inner.Right
                && pt.Y > inner.Bottom && pt.Y <= inner.Bottom + XAxisBandHeight(ca.AxisX))
            {
                return NewAxisDrag(ca, ca.AxisX, true, pt,
                    ca.AxisX.Minimum ?? ca.AxisX.ActualMinimum,
                    ca.AxisX.Maximum ?? ca.AxisX.ActualMaximum,
                    inner.Width, inner.Height, -1);
            }
            return null;
        }
        /// <summary>构造轴拖动状态；量程非有限数或坐标系尺寸无效时返回 null（映射器未就绪，不允许拖）。</summary>
        private static AxisDragState NewAxisDrag(HChartArea ca, HChartAxis ax, bool isX, Point pt,
            double min, double max, float innerW, float innerH, int yi)
        {
            if (!IsFinite(min) || !IsFinite(max) || max - min <= 0 || innerW <= 0 || innerH <= 0) return null;
            return new AxisDragState
            {
                CA = ca, Axis = ax, IsX = isX, Start = pt,
                StartMin = min, StartMax = max, InnerW = innerW, InnerH = innerH, YIndex = yi
            };
        }
        /// <summary>取 ChartArea 在控件中的占位矩形（按 Position 百分比映射到 ClientRectangle）。</summary>
        private RectangleF GetChartAreaRect(HChartArea ca)
        {
            return new RectangleF(
                ClientRectangle.Left + ClientRectangle.Width * ca.Position.Left / 100f,
                ClientRectangle.Top + ClientRectangle.Height * ca.Position.Top / 100f,
                ClientRectangle.Width * ca.Position.Width / 100f,
                ClientRectangle.Height * ca.Position.Height / 100f);
        }
        /// <summary>
        /// 实测每根启用 Y 轴的刻度数字最大宽度与整带宽度并写入缓存（绘制前调用）：
        /// 带宽 = 轴线→标签间隙(YTickGap) + 实测数字列宽 + 标签→标题间隙(YTitleGap) + 竖排标题厚度 + 外侧余量 4；
        /// 数字越长带宽越大，GetInnerPlotRect 据此压缩坐标系——标题/单位永不隐藏、数字永不缩字号或截断。
        /// 测量取密度粗调前的完整刻度集合（绘制时只会抽稀间隔，宽度为安全超集）。
        /// </summary>
        private void MeasureAxisBands(Graphics g, HChartArea ca, float availW)
        {
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (!ay.IsEnabled) { _yBand.Remove(ay); _yLabelW.Remove(ay); continue; }
                float labelW;
                if (ay.IsCategoryAxis && _barMode)
                {
                    // 横向条形：纵轴是类别名，取最宽类别文字
                    labelW = 1f;
                    foreach (var name in _catLabels)
                        labelW = Math.Max(labelW, g.MeasureString(name, ay.LabelStyleFont).Width);
                }
                else
                {
                    // 数值轴：遍历全部主刻度，FormatTick 与 X 轴同一格式，取最宽文字
                    labelW = g.MeasureString("0", ay.LabelStyleFont).Width;
                    double interval = ay.ActualInterval > 0 ? ay.ActualInterval : 1.0;
                    double start = Math.Ceiling(ay.ActualMinimum / interval) * interval;
                    for (double v = start; v <= ay.ActualMaximum + 1e-9; v += interval)
                        labelW = Math.Max(labelW, g.MeasureString(FormatTick(v), ay.LabelStyleFont).Width);
                }
                labelW += 1f;   // 抗锯齿/文本度量余量
                bool hasHead = !string.IsNullOrEmpty(ay.Title) || !string.IsNullOrEmpty(ay.Unit);
                float thick = 0f;
                if (hasHead)
                    using (var uf = UnitFont(ay.TitleFont))
                        thick = Math.Max(ay.TitleFont.GetHeight(), uf.GetHeight());
                _yLabelW[ay] = labelW;
                _yBand[ay] = YTickGap + labelW + (hasHead ? YTitleGap + thick : 0f) + 4f;
            }
            MeasureXLabelBand(g, ca, availW);
        }
        /// <summary>类别标签是否全部是纯数字（整数/小数，至少有一个非空标签）：纯数字 X 标签永不竖排。</summary>
        private bool CatLabelsAllNumeric()
        {
            bool any = false;
            foreach (var s in _catLabels)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                any = true;
                if (!double.TryParse(s.Trim(), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                    return false;
            }
            return any;
        }
        /// <summary>
        /// 测 X 轴标签是否需要顺时针转 90° 竖排（仅非实时图且宿主允许 XLabelAutoRotate；实时图维持两行时间/抽稀）：
        /// 横排在当前刻度密度下放不下最宽标签时置位竖排；竖排带宽只按"实际会画出来的标签"量——
        /// 类别轴按字高抽稀后的命中项、数值/时间轴按竖排目标刻度数重选间隔后的落点——
        /// 既保证全部画出的标签完整不重叠，又不预留不存在的超长空白，轴标题紧贴刻度带。
        /// availW 传入 paddedArea 宽，内部扣除与 GetInnerPlotRect 相同的左右 Y 轴带宽，按真实 inner 宽判拥挤。
        /// </summary>
        private void MeasureXLabelBand(Graphics g, HChartArea ca, float availW)
        {
            var ax = ca.AxisX;
            _xRotate.Remove(ax);
            _xRotBandW.Remove(ax);
            if (IsLiveMode || !XLabelAutoRotate || !ax.IsEnabled) return;
            // 与 GetInnerPlotRect 同源：paddedArea 要先扣除左右启用 Y 轴带宽才是真实绘图宽，
            // 否则短标签会卡在"测量说横排放得下、绘制端按 inner 却只能抽稀"的边界（如 10 K线 D1..D10）
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ayb = YAxisAt(ca, yi);
                if (!ayb.IsEnabled) continue;
                availW -= YBandWidthOf(ayb);
            }
            availW = Math.Max(8f, availW);
            float lineH = ax.LabelStyleFont.GetHeight();
            float wmax = 1f;
            bool crowded;
            if (ax.IsCategoryAxis && !_barMode)
            {
                // 类别标签全部是数字（如 "1"、"12.5"）时与数值轴同口径：永不竖排，横排抽稀即可
                if (CatLabelsAllNumeric()) return;
                int n = _catLabels.Count;
                for (int i = 0; i < n; i++)
                    wmax = Math.Max(wmax, g.MeasureString(_catLabels[i], ax.LabelStyleFont).Width);
                float slotW = n > 0 ? availW / n : availW;
                crowded = slotW < wmax + 4f;
                if (crowded)
                {
                    // 与 DrawAxisX 类别竖排同一 stride：只量实际会画的标签（末尾项始终保留）
                    wmax = 1f;
                    int stride = Math.Max(1, (int)Math.Ceiling((lineH + 2f) / Math.Max(1f, slotW)));
                    for (int i = 0; i < n; i++)
                        if (i % stride == 0 || i == n - 1)
                            wmax = Math.Max(wmax, g.MeasureString(_catLabels[i], ax.LabelStyleFont).Width);
                }
            }
            else
            {
                bool timeAxis = IsTimeAxis(ax);
                // 数值轴（非时间）刻度全是数字：永远横排，靠密度翻倍+像素级抽稀保证不重叠，不竖排
                if (!timeAxis) return;
                // 粗判拥挤：取 min/max 两刻度最宽文字
                float wEdge = Math.Max(
                    g.MeasureString(FormatAxisTick(ax, ax.ActualMinimum).Replace('\n', ' '), ax.LabelStyleFont).Width,
                    g.MeasureString(FormatAxisTick(ax, ax.ActualMaximum).Replace('\n', ' '), ax.LabelStyleFont).Width);
                double interval0 = ax.ActualInterval > 0 ? ax.ActualInterval : 1.0;
                double ticks0 = (ax.ActualMaximum - ax.ActualMinimum) / interval0;
                crowded = ticks0 > 0 && availW / ticks0 < wEdge + 4f;
                if (crowded)
                {
                    // 与 DrawAxisX 竖排路径同一间隔算法，枚举实际落点量最宽文字
                    int targetTicks = Math.Max(2, (int)(availW / Math.Max(1f, lineH + 3f)));
                    double range = ax.ActualMaximum - ax.ActualMinimum;
                    double interval = timeAxis
                        ? ComputeTimeInterval(range, targetTicks)
                        : ComputeNiceInterval(range, targetTicks);
                    double start;
                    if (timeAxis)
                    {
                        double stepSec = interval * 86400.0;
                        start = Math.Ceiling(ax.ActualMinimum * 86400.0 / stepSec) * stepSec / 86400.0;
                    }
                    else start = Math.Ceiling(ax.ActualMinimum / interval) * interval;
                    for (double v = start; v <= ax.ActualMaximum + 1e-9; v += interval)
                        wmax = Math.Max(wmax, g.MeasureString(FormatAxisTick(ax, v).Replace('\n', ' '), ax.LabelStyleFont).Width);
                }
            }
            if (crowded)
            {
                _xRotate[ax] = true;
                _xRotBandW[ax] = wmax;
            }
        }
        /// <summary>
        /// 算 inner（动态内缩版）：从 area→paddedArea(4px padding)出发，
        /// 按 ca.InnerPlotPosition 算名义 inner，再按启用的侧轴数把 inner 往里推，
        /// 保证 DrawAxisY 里轴线/刻度/标题不落到 paddedArea 之外。
        /// 内部会幂等地调一次 ComputeAxis，保证 AxisY.IsEnabled 等最新；调用方无需预先调用。
        /// </summary>
        private RectangleF GetInnerPlotRect(HChartArea ca)
        {
            // 幂等：ComputeAxis 每次结果相同，DrawChartArea 里已经调过一次，这里再调也没事
            ComputeAxis(ca);
            var area = GetChartAreaRect(ca);
            const float pad = 4f;
            // ---- area 内缩 4px padding 得到 paddedArea ----
            var paddedArea = new RectangleF(area.Left + pad, area.Top + pad, area.Width - 2 * pad, area.Height - 2 * pad);
            // ---- 第一步：按 ca.InnerPlotPosition 算"名义 inner"（用户配置的百分比预留）----
            var nominal = new RectangleF(
                paddedArea.Left + paddedArea.Width * ca.InnerPlotPosition.Left / 100f,
                paddedArea.Top + paddedArea.Height * ca.InnerPlotPosition.Top / 100f,
                paddedArea.Width * ca.InnerPlotPosition.Width / 100f,
                paddedArea.Height * ca.InnerPlotPosition.Height / 100f);
            // ---- 第二步：按启用轴的实测带宽（刻度数字越长带宽越大）累加左右预留，坐标系向内压缩 ----
            float leftNeed = 0f, rightNeed = 0f;
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (!ay.IsEnabled) continue;
                if (YAxisSide(yi) > 0) rightNeed += YBandWidthOf(ay); else leftNeed += YBandWidthOf(ay);
            }
            // 顶部：Top 停靠标题紧贴坐标系上沿（小上留白 + 字高 + TitlePlotGap 间隙），
            // 无标题/自动字号过小则绘图区直接贴 Padding 顶
            float topNeed = 0f;
            foreach (var t in Titles)
            {
                if (t.Docking != HChartDocking.Top || string.IsNullOrEmpty(t.Text)) continue;
                var tf = ResolveTitleFont(t);
                if (tf != null) topNeed = Math.Max(topNeed, TitleTopInset + tf.Height + TitlePlotGap);
            }
            // 底部：X 轴刻度线 + 数字行高 + 间隙 + X 标题/单位行高（与 DrawAxisX/DrawAxisXTitle 坐标同源）
            float bottomNeed = ca.AxisX.IsEnabled ? XAxisBandHeight(ca.AxisX) : 0f;
            // ---- 第三步：从 nominal 与实测预留取较大边距（用户配置的 InnerPlotPosition 继续尊重）----
            float newLeft = Math.Max(nominal.Left, paddedArea.Left + leftNeed);
            float newRight = Math.Min(nominal.Right, paddedArea.Right - rightNeed);
            float newTop = Math.Max(nominal.Top, paddedArea.Top + topNeed);
            float newBottom = Math.Min(nominal.Bottom, paddedArea.Bottom - bottomNeed);
            const float minInner = 8f;   // 坐标系极小兜底宽/高
            // 宽度不足（轴多/数字极长）：两侧按实测需求比例分配，坐标系至少保留 8px
            if (newRight - newLeft < minInner)
            {
                float total = leftNeed + rightNeed;
                if (paddedArea.Width < minInner + 4f || total <= 0f) { newLeft = paddedArea.Left; newRight = paddedArea.Right; }
                else
                {
                    float sideW = paddedArea.Width - minInner;
                    newLeft = paddedArea.Left + sideW * leftNeed / total;
                    newRight = newLeft + minInner;
                }
            }
            // 高度不足（标题+X 标签带超过绘图高）：上下按实测需求比例分配
            if (newBottom - newTop < minInner)
            {
                float total = topNeed + bottomNeed;
                if (paddedArea.Height < minInner + 4f || total <= 0f) { newTop = paddedArea.Top; newBottom = paddedArea.Bottom; }
                else
                {
                    float sideH = paddedArea.Height - minInner;
                    newTop = paddedArea.Top + sideH * topNeed / total;
                    newBottom = newTop + minInner;
                }
            }
            return new RectangleF(newLeft, newTop, newRight - newLeft, newBottom - newTop);
        }
        /// <summary>
        /// X 轴底部标签带总高（GetInnerPlotRect 底部预留与 X 轴拖动命中同源）：
        /// 横排：刻度数字行高（时间轴可多行，如日期/时间两行）+ 8 间隙 + 标题/单位行高 + 2 外侧余量；
        /// 竖排（非实时、刻度放不下）：刻度线 5 + 最长标签带宽（旋转后向下延伸）+ 4 间隙 + 标题行 + 2 余量，坐标系相应压缩。
        /// </summary>
        private float XAxisBandHeight(HChartAxis ax)
        {
            float groupH = 0f;
            if (!string.IsNullOrEmpty(ax.Title) || !string.IsNullOrEmpty(ax.Unit))
                using (var uf = UnitFont(ax.TitleFont))
                    groupH = Math.Max(ax.TitleFont.GetHeight(), uf.GetHeight());
            if (_xRotate.TryGetValue(ax, out bool rot) && rot)
                return 5f + _xRotBandW[ax] + 4f + groupH + 2f;
            float labelH = ax.LabelStyleFont.GetHeight() * (IsTimeAxis(ax) ? TimeLabelLines(ax) : 1);
            return labelH + 8f + groupH + 2f;
        }
        // ============ Zoom / Pan 实现（统一走 HScreen；4 个 Y 轴各自映射）============
        /// <param name="zoomX">是否缩放 X 轴</param>
        /// <param name="zoomY">是否缩放 Y 轴（所有启用的 Y 轴同步缩放，保持对齐）</param>
        private void ApplyZoom(HChartArea ca, Point centerScreen, bool zoomIn, bool zoomX = true, bool zoomY = true)
        {
            ComputeAxis(ca);
            var inner = GetInnerPlotRect(ca);
            SetupScreens(inner, ca);
            double factor = zoomIn ? 0.8 : 1.25;
            var auto = _autoMinMax[ca.Name];
            // 所有轴的锚点世界坐标在缩放前记录；未参与缩放的轴同样要参与重锚定——
            // 别的轴刻度变宽会推动 inner 原点，不补偿它也会跟着漂
            double xAnchorW = ScreenToWorldX(ca.AxisX, inner, centerScreen.X);
            var yAnchors = new Dictionary<HChartAxis, double>();
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay0 = YAxisAt(ca, yi);
                if (ay0.IsEnabled) yAnchors[ay0] = ScreenToWorldY(ay0, inner, centerScreen.Y);
            }
            if (zoomX)
            {
                // 锚点精确换算（不经 HScreen 小数位截断），缩放后光标下世界坐标像素级不动
                CommitAxisZoom(ca.AxisX, xAnchorW, factor, auto.XMin, auto.XMax, 10);
            }
            if (zoomY)
            {
                // 所有启用 Y 轴各自以鼠标在该轴上的世界 Y 为锚点缩放
                for (int yi = 0; yi < YAxisCount(ca); yi++)
                {
                    var ay = YAxisAt(ca, yi);
                    if (!ay.IsEnabled || yi >= auto.YMin.Count || double.IsNaN(auto.YMin[yi])) continue;
                    CommitAxisZoom(ay, yAnchors[ay], factor, auto.YMin[yi], auto.YMax[yi], 8);
                }
            }
            // 锚点在绘制帧内收敛（刻度位数/轴带宽变化引起的 inner 平移一并补偿）
            SetPendingAnchor(ca, centerScreen.X, centerScreen.Y, xAnchorW, yAnchors);
        }
        /// <summary>
        /// 框选放大：把拖出的矩形（客户区坐标）换算成世界量程，直接设为该 ChartArea X 轴与各启用 Y 轴的
        /// 可见范围，再走统一钳制（自动量程边界/最深放大 1000 倍）与刻度唯一性校验（刻度文字会重复则该轴回退）；
        /// 以框中心为挂起锚点，绘制帧内重锚定后框中心世界点像素级不动；实时图进入暂停。
        /// </summary>
        private void ApplyRubberZoom(HChartArea ca, RectangleF rect)
        {
            ComputeAxis(ca);
            var inner = GetInnerPlotRect(ca);
            SetupScreens(inner, ca);
            rect.Intersect(inner);
            if (rect.Width < 3f || rect.Height < 3f) return;
            var auto = _autoMinMax[ca.Name];
            // 框中心的世界坐标必须在改量程前用旧 inner 精确换算（锚定用）
            float cx = (rect.Left + rect.Right) / 2f;
            float cy = (rect.Top + rect.Bottom) / 2f;
            double xcW = ScreenToWorldX(ca.AxisX, inner, cx);
            var yAnchors = new Dictionary<HChartAxis, double>();
            // X 轴：框左右两边即新量程
            double x0 = ScreenToWorldX(ca.AxisX, inner, rect.Left);
            double x1 = ScreenToWorldX(ca.AxisX, inner, rect.Right);
            if (x0 > x1) { double t = x0; x0 = x1; x1 = t; }
            double? oldXMin = ca.AxisX.Minimum, oldXMax = ca.AxisX.Maximum;
            ca.AxisX.Minimum = x0;
            ca.AxisX.Maximum = x1;
            ClampAxisRange(ca.AxisX, auto.XMin, auto.XMax, xcW);
            double xvMin = ca.AxisX.Minimum ?? ca.AxisX.ActualMinimum;
            double xvMax = ca.AxisX.Maximum ?? ca.AxisX.ActualMaximum;
            if (!TickLabelsDistinct(xvMin, xvMax, 10, ca.AxisX))
            { ca.AxisX.Minimum = oldXMin; ca.AxisX.Maximum = oldXMax; }
            // 各启用 Y 轴：框上下两边即新量程（屏幕下方对应世界下限）；未参与缩放的轴也记录锚点，参与重锚定补偿
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (!ay.IsEnabled) continue;
                yAnchors[ay] = ScreenToWorldY(ay, inner, cy);
                if (yi >= auto.YMin.Count || double.IsNaN(auto.YMin[yi])) continue;
                double y0 = ScreenToWorldY(ay, inner, rect.Bottom);
                double y1 = ScreenToWorldY(ay, inner, rect.Top);
                if (y0 > y1) { double t = y0; y0 = y1; y1 = t; }
                double? oldYMin = ay.Minimum, oldYMax = ay.Maximum;
                ay.Minimum = y0;
                ay.Maximum = y1;
                ClampAxisRange(ay, auto.YMin[yi], auto.YMax[yi], yAnchors[ay]);
                double yvMin = ay.Minimum ?? ay.ActualMinimum;
                double yvMax = ay.Maximum ?? ay.ActualMaximum;
                if (!TickLabelsDistinct(yvMin, yvMax, 8, ay))
                { ay.Minimum = oldYMin; ay.Maximum = oldYMax; }
            }
            SetPendingAnchor(ca, cx, cy, xcW, yAnchors);
            PauseLive();
        }
        /// <summary>
        /// 记录挂起锚点：量程已改完，锚定在下次绘制帧里完成（见 ApplyPendingAnchor）。
        /// screenX/screenY 为锚点屏幕坐标（滚轮=光标，框选=框中心），xWorld/yWorld 为其缩放前的世界坐标。
        /// </summary>
        private void SetPendingAnchor(HChartArea ca, float screenX, float screenY,
            double? xWorld, Dictionary<HChartAxis, double> yWorld)
        {
            _anchorCA = ca;
            _anchorSX = screenX;
            _anchorSY = screenY;
            _anchorXW = xWorld;
            _anchorYW = yWorld;
        }
        /// <summary>
        /// 绘制管线内应用挂起锚点并返回本帧最终 inner：按当前 inner 把各轴窗口平移到锚点世界坐标
        /// （跨度不动、只平移，刻度唯一性不受影响），再实测带宽取新 inner，反复迭代直到两轮 inner 重合；
        /// 这样最终 SetupScreens 用的 inner 与锚定计算用的 inner 严格同一，光标/框中心世界点像素级不动。
        /// </summary>
        private RectangleF ApplyPendingAnchor(Graphics g, HChartArea ca, RectangleF padded, RectangleF inner)
        {
            if (!ReferenceEquals(_anchorCA, ca) || _anchorYW == null) return inner;
            RectangleF geom = inner;
            for (int iter = 0; iter < 6; iter++)
            {
                TranslateAxesToAnchor(ca, geom);
                ComputeAxis(ca);
                MeasureAxisBands(g, ca, padded.Width);
                var geom2 = GetInnerPlotRect(ca);
                if (geom2.Width <= 0 || geom2.Height <= 0) break;
                geom = geom2;
                // 收敛判定：再按新 inner 平移后几何不变即为不动点（额外一次平移只做亚像素微调，不改字宽）
                TranslateAxesToAnchor(ca, geom2);
                ComputeAxis(ca);
                MeasureAxisBands(g, ca, padded.Width);
                var geom3 = GetInnerPlotRect(ca);
                if (GeomClose(geom2, geom3)) { geom = geom3; break; }
                geom = geom3;
            }
            _anchorCA = null;
            _anchorXW = null;
            _anchorYW = null;
            return geom;
        }
        /// <summary>按给定 inner 把各轴窗口整体平移，使锚点世界坐标落在锚点屏幕位置（跨度不变）。</summary>
        private void TranslateAxesToAnchor(HChartArea ca, RectangleF inner)
        {
            if (_anchorYW == null) return;
            if (_anchorXW.HasValue && ca.AxisX.IsEnabled)
            {
                var ax = ca.AxisX;
                double lo = ax.Minimum ?? ax.ActualMinimum;
                double hi = ax.Maximum ?? ax.ActualMaximum;
                double span = hi - lo;
                double f = (_anchorSX - inner.Left) / inner.Width;
                ax.Minimum = _anchorXW.Value - f * span;
                ax.Maximum = _anchorXW.Value + (1 - f) * span;
                ClampToWorldLimit(ax);
            }
            foreach (var kv in _anchorYW)
            {
                var ay = kv.Key;
                if (!ay.IsEnabled) continue;
                double lo = ay.Minimum ?? ay.ActualMinimum;
                double hi = ay.Maximum ?? ay.ActualMaximum;
                double span = hi - lo;
                double f = (inner.Bottom - _anchorSY) / inner.Height;
                ay.Minimum = kv.Value - f * span;
                ay.Maximum = kv.Value + (1 - f) * span;
                ClampToWorldLimit(ay);
            }
        }
        /// <summary>两个 inner 四边是否都只差 0.01px 以内（锚点迭代的不动点判定）。</summary>
        private static bool GeomClose(RectangleF a, RectangleF b)
        {
            return Math.Abs(a.Left - b.Left) < 0.01f && Math.Abs(a.Right - b.Right) < 0.01f
                && Math.Abs(a.Top - b.Top) < 0.01f && Math.Abs(a.Bottom - b.Bottom) < 0.01f;
        }
        /// <summary>
        /// 把轴窗口整体平移回 ±WorldRangeLimit 世界边界内（跨度不变，到边后继续拖/缩放不再外移）；
        /// 跨度本身比 2 倍边界还宽时（只可能发生在极端缩小）才压缩跨度填满边界。
        /// </summary>
        private void ClampToWorldLimit(HChartAxis ax)
        {
            double min = ax.Minimum ?? ax.ActualMinimum;
            double max = ax.Maximum ?? ax.ActualMaximum;
            if (!IsFinite(min) || !IsFinite(max) || max - min <= 0) return;
            double lim = IsFinite(WorldRangeLimit) && WorldRangeLimit > 0 ? WorldRangeLimit : double.MaxValue / 8;
            double span = max - min;
            if (span >= 2 * lim) { ax.Minimum = -lim; ax.Maximum = lim; return; }
            if (min < -lim) { min = -lim; max = min + span; }
            if (max > lim) { max = lim; min = max - span; }
            ax.Minimum = min; ax.Maximum = max;
        }
        /// <summary>
        /// 钳制轴可见量程（所有缩放路径的统一出口）：
        ///   - 量程不得小于 数据量程 / ZoomScaleMax（默认最深放大 1 亿倍，足够显示 6 位小数刻度），
        ///     更深的放大由 TickLabelsDistinct 兜底拒绝，双重防护防止缩到 0/NaN 画出大叉；
        ///   - 量程不得大于 数据量程 / ZoomScaleMin（默认 1E-300，等同不限制缩小）；
        ///   - 窗口位置最终夹在 ±WorldRangeLimit 世界边界内（默认 1E300，实际无限）；
        ///   - 当前量程或自动量程非有限数时，清掉用户量程回到自适应，彻底兜底。
        /// 给了 anchor（光标/框中心的世界坐标）时，锚点在屏幕上的相对位置保持不变；
        /// 不给锚点时退化为保持当前可见窗口中心不变。
        /// </summary>
        private void ClampAxisRange(HChartAxis ax, double autoMin, double autoMax, double? anchor = null)
        {
            // 自动量程无效（无数据/未计算）：任何用户量程都不可信，清回自适应
            if (!IsFinite(autoMin) || !IsFinite(autoMax) || autoMax - autoMin <= 0)
            {
                ax.Minimum = null; ax.Maximum = null;
                return;
            }
            double curMin = ax.Minimum ?? ax.ActualMinimum;
            double curMax = ax.Maximum ?? ax.ActualMaximum;
            // 用户量程非有限数（NaN/Infinity）：清回自适应，避免 NaN 传导到映射器
            if (!IsFinite(curMin) || !IsFinite(curMax) || curMax - curMin <= 0)
            {
                ax.Minimum = null; ax.Maximum = null;
                return;
            }
            double autoRange = autoMax - autoMin;
            double minRange = autoRange / Math.Max(1.0, ZoomScaleMax);   // 可见量程下限
            double maxRange = autoRange / (ZoomScaleMin > 0 ? ZoomScaleMin : 1e-300); // 可见量程上限（默认≈无限）
            double range = curMax - curMin;
            if (range < minRange) range = minRange;
            double lim = IsFinite(WorldRangeLimit) && WorldRangeLimit > 0 ? WorldRangeLimit : double.MaxValue / 8;
            if (range > 2 * lim) range = 2 * lim;
            if (range > maxRange) range = maxRange;
            if (anchor.HasValue && IsFinite(anchor.Value))
            {
                // 锚点在当前窗口中的相对位置（可落在 0..1 之外，光标在轴带内换算时会外推）保持不动
                double rel = (anchor.Value - curMin) / (curMax - curMin);
                ax.Minimum = anchor.Value - rel * range;
                ax.Maximum = anchor.Value + (1 - rel) * range;
            }
            else
            {
                double center = (curMin + curMax) / 2;
                ax.Minimum = center - range / 2;
                ax.Maximum = center + range / 2;
            }
            // 世界边界：整体平移回界内（跨度不变）
            ClampToWorldLimit(ax);
        }
        /// <summary>拖画布平移：把屏幕像素增量换算成世界坐标增量，整体平移各轴量程，鼠标下世界坐标保持不变。
        /// 平移本身不做数据范围吸附，仅夹到 ±WorldRangeLimit 世界边界（默认 1E300，等同无限远）。</summary>
        private void ApplyPan(HChartArea ca, PointF pixelDelta, Point cursor)
        {
            ComputeAxis(ca);
            var inner = GetInnerPlotRect(ca);
            SetupScreens(inner, ca);
            // X 轴平移（主映射器的 X 比例）
            HChartAxis ax = ca.AxisX;
            double curMinX = ax.Minimum ?? ax.ActualMinimum;
            double curMaxX = ax.Maximum ?? ax.ActualMaximum;
            double worldDx = -_screen.ScreenLengthToWorld(pixelDelta.X, false);
            ax.Minimum = curMinX + worldDx; ax.Maximum = curMaxX + worldDx;
            ClampToWorldLimit(ax);
            // 所有 Y 轴：Y 平移量按各自轴比例换算
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (!ay.IsEnabled) continue;
                double curMin = ay.Minimum ?? ay.ActualMinimum;
                double curMax = ay.Maximum ?? ay.ActualMaximum;
                double worldDy = ScreenFor(ay).ScreenLengthToWorld(pixelDelta.Y, true);
                ay.Minimum = curMin + worldDy; ay.Maximum = curMax + worldDy;
                ClampToWorldLimit(ay);
            }
            // 挂起锚点：拖动过程中刻度位数变化推动 inner 时，绘制帧内补偿，鼠标下世界点不跳
            AnchorAt(ca, cursor.X, cursor.Y, inner);
            PauseLive(); // 实时图：手动平移后暂停跟随，菜单"继续实时刷新"恢复
        }
        /// <summary>
        /// 按当前量程/inner 记录屏幕点 (sx,sy) 对应的全部启用轴世界坐标为挂起锚点；
        /// 绘制帧收敛后该世界点严格停在 (sx,sy)（滚轮/框选/平移/轴带拖动共用）。
        /// </summary>
        private void AnchorAt(HChartArea ca, float sx, float sy, RectangleF inner)
        {
            double? xw = ca.AxisX.IsEnabled ? (double?)ScreenToWorldX(ca.AxisX, inner, sx) : null;
            var yw = new Dictionary<HChartAxis, double>();
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (ay.IsEnabled) yw[ay] = ScreenToWorldY(ay, inner, sy);
            }
            SetPendingAnchor(ca, sx, sy, xw, yw);
        }
        /// <summary>隐藏自绘悬停气泡（气泡本来就没显示时不触发重绘）。</summary>
        private void HideTip()
        {
            if (_tipLines == null) return;
            _tipLines = null;
            Invalidate();
        }
        /// <summary>按绘制侧调色板顺序解析系列颜色（区域内每个启用系列占一个调色板位，与图例/气泡配色一致）。</summary>
        private Color PaletteColorOf(HChartSeries target)
        {
            int paletteIndex = 0;
            foreach (var s in Series)
            {
                if (s.ChartArea != target.ChartArea || !s.IsEnabled) continue;
                if (ReferenceEquals(s, target)) return s.ResolveColor(paletteIndex, Palette, ThemeIsDark);
                paletteIndex++;
            }
            return target.ResolveColor(0, Palette, ThemeIsDark);
        }
        /// <summary>在鼠标位置准备自绘气泡内容：X/Y 值、类别、系列名（系列色）、柱/条系列总数、附加标签。</summary>
        private void ShowToolTip(HChartHitTestResult hit, Point screen)
        {
            var sc = HTipPopup.Colors(HTipSchemeKind.Auto, ThemeIsDark);
            var lines = new List<HTipLine>();
            if (hit.Point == null)
            {
                lines.Add(new HTipLine(HTranslation.GetContent("无数据点"), sc.Text));
            }
            else
            {
                var p = hit.Point;
                // 时间轴 X 在弹窗里统一完整到毫秒 yyyy-MM-dd HH:mm:ss.fff；类别/数值轴按刻度格式（不换行）
                string xText = IsTimeAxis(hit.ChartArea.AxisX)
                    ? FormatPopupX(hit.ChartArea.AxisX, p.XValue)
                    : FormatAxisTick(hit.ChartArea.AxisX, p.XValue).Replace('\n', ' ');
                lines.Add(new HTipLine("X = " + xText, sc.TagText));
                lines.Add(new HTipLine("Y = " + FormatTick(p.YValue), sc.Text));
                // 气泡图：圆面积代表第三维（如库存），语义名取自 Series.SizeValueName，必须在弹窗里完整显示
                if (hit.Series != null && hit.Series.ChartType == HChartType.Bubble && p.YValues.Length > 0)
                {
                    string sizeName = string.IsNullOrEmpty(hit.Series.SizeValueName)
                        ? HTranslation.GetContent("气泡大小") : HTranslation.GetContent("气泡大小（") + hit.Series.SizeValueName + "）";
                    lines.Add(new HTipLine(sizeName + " = " + FormatTick(p.YValues[0]) + HTranslation.GetContent("（圆面积）"), sc.Text));
                }
                if (p.AxisLabel != null) lines.Add(new HTipLine(HTranslation.GetContent("类别 = ") + p.AxisLabel, sc.Text));
                if (hit.Series != null)
                {
                    // 系列名/总数用系列图例色（按气泡底色自适应），在气泡里一眼对上图例
                    var seriesColor = HTipPopup.Adapt(PaletteColorOf(hit.Series), sc);
                    lines.Add(new HTipLine(HTranslation.GetContent("系列 = ") + hit.Series.Name, seriesColor));
                    // 柱/条类（05 柱、06 堆叠柱、07 条形）：气泡里附该系列全部数据点的总数与项数
                    if (IsBarType(hit.Series.ChartType))
                    {
                        int cnt = 0; double sum = 0;
                        foreach (var pp in hit.Series.Points)
                            if (!pp.IsEmpty) { cnt++; sum += pp.YValue; }
                        lines.Add(new HTipLine(HTranslation.GetContent($"系列总数 = {FormatTick(sum)}（{cnt} 项）"), seriesColor));
                    }
                }
                if (!string.IsNullOrEmpty(p.Label)) lines.Add(new HTipLine(p.Label, sc.Text));
            }
            _tipLines = lines;
            _tipAnchor = screen;
            Invalidate();
        }
        /// <summary>绘制自绘悬停气泡：交给 Tips 目录下的 HTipPopup 渲染（固定经典外形 + Auto 配色 + 无角饰）。
        /// 与提取前内联绘制像素一致（pad6/lh16/r7/gap14/夹 2px）。</summary>
        private void DrawTip(Graphics g)
        {
            if (_tipLines == null || _tipLines.Count == 0 || _lastHit == null) return;
            HTipPopup.Render(g, _tipLines, RulerFont, _tipAnchor, ClientRectangle,
                HTipSkinKind.Classic, HTipSchemeKind.Auto, ThemeIsDark, HTipArt.None, TipOffset);
        }
        // ============ 坐标/轴计算 ============
        /// <summary>是否为笛卡尔坐标系图表（有 X/Y 轴）。Pie/Doughnut/Funnel/Radar 返回 false。</summary>
        private static bool IsCartesian(HChartType t)
        {
            switch (t)
            {
                case HChartType.Pie:
                case HChartType.Doughnut:
                case HChartType.Funnel:
                case HChartType.Radar: return false;
                default: return true;
            }
        }
        /// <summary>是否连续线类图（Line/Spline/Area/StackedArea/StepLine）：线在两数据点之间也有值，
        /// 标尺区间即使不含采样点也能按线段插值给出数据。</summary>
        private static bool IsContinuousLine(HChartType t)
        {
            return t == HChartType.Line || t == HChartType.Spline || t == HChartType.Area ||
                   t == HChartType.StackedArea || t == HChartType.StepLine;
        }
        /// <summary>连续线段上 x 处的 Y 值：StepLine 段内保持起点值（与 DrawStepLine 阶梯约定一致）；
        /// 其余连续线（Line/Spline/Area/StackedArea）在两数据点间线性插值（Spline 曲线为线性近似）。</summary>
        private static double SegmentY(HChartType t, HChartDataPoint a, HChartDataPoint b, double x)
        {
            if (t == HChartType.StepLine) return a.YValue;
            double dx = b.XValue - a.XValue;
            if (Math.Abs(dx) < 1e-12) return a.YValue;
            return a.YValue + (b.YValue - a.YValue) * ((x - a.XValue) / dx);
        }
        /// <summary>是否以 0 为基线（柱/条形/面积）：数值轴必须包含 0。</summary>
        private static bool ZeroBaseline(HChartType t)
        {
            switch (t)
            {
                case HChartType.Column:
                case HChartType.StackedColumn:
                case HChartType.Bar:
                case HChartType.Area:
                case HChartType.StackedArea: return true;
                default: return false;
            }
        }
        /// <summary>计算 ChartArea 全部轴的自适应量程（遍历各系列数据取 min/max，类别轴取 0..N-1），写入各轴 ActualMin/Max。</summary>
        private void ComputeAxis(HChartArea ca)
        {
            var list = new List<HChartSeries>();
            var allInArea = new List<HChartSeries>();
            foreach (var s in Series)
                if (s.ChartArea == ca.Name)
                {
                    allInArea.Add(s);
                    if (s.IsEnabled) list.Add(s);
                }
            // ---- 按系列绑定的 YAxisIndex 确保轴列表容量（[0] 已是主轴，需要多少根补多少根；横向条形只用主轴）----
            bool barMode0 = allInArea.Exists(s => s.ChartType == HChartType.Bar);
            int needAxis = 0;
            if (!barMode0)
                foreach (var s in allInArea)
                    if (IsCartesian(s.ChartType)) needAxis = Math.Max(needAxis, s.YAxisIndex);
            while (ca.YAxes.Count < needAxis + 1)
                ca.YAxes.Add(new HChartAxis { Name = "Y" + (ca.YAxes.Count + 1), IsEnabled = false });
            // ---- 数据统计签名：仅用 O(系列数) 信息（类型/启用/点数/Y轴绑定/首尾点值/实时状态）。
            //      拖动与锚点迭代一帧内重复调用十几次，签名不变就复用扫描结果，跳过全部逐点循环 ----
            int sig = unchecked((int)0x811C9DC5);
            unchecked
            {
                sig = sig * 397 ^ (IsLiveMode ? 13 : 7);
                sig = sig * 397 ^ (IsLivePaused ? 13 : 7);
                sig = sig * 397 ^ (LiveShowAll ? 13 : 7);
                sig = sig * 397 ^ LiveWindowSize.GetHashCode();
                sig = sig * 397 ^ allInArea.Count;
                foreach (var s in allInArea)
                {
                    sig = sig * 397 ^ (int)s.ChartType;
                    sig = sig * 397 ^ (s.IsEnabled ? 1 : 0);
                    sig = sig * 397 ^ (s.YAxisIndex + 17);
                    sig = sig * 397 ^ s.Points.Count;
                    if (s.Points.Count > 0) // 首尾点值：实时追加/改值立刻使签名失效
                    {
                        var p0 = s.Points[0];
                        var pn = s.Points[s.Points.Count - 1];
                        sig = sig * 397 ^ (p0.IsEmpty ? 3 : 1) ^ p0.XValue.GetHashCode() ^ p0.YValue.GetHashCode() ^ (p0.AxisLabel != null ? 5 : 0);
                        sig = sig * 397 ^ (pn.IsEmpty ? 3 : 1) ^ pn.XValue.GetHashCode() ^ pn.YValue.GetHashCode() ^ (pn.AxisLabel != null ? 5 : 0);
                    }
                }
            }
            AreaDataCache dc;
            bool cacheHit = _areaDataCache.TryGetValue(ca.Name, out dc) && dc != null && dc.Sig == sig;
            int yCount = YAxisCount(ca);
            bool anyAxis, hasCategories;
            bool liveFollow;
            double liveLo = 0, liveHi = 0;
            double hMin, hMax;
            double[] vMin, vMax;
            bool[] hasY, zeroBaseline;
            if (cacheHit)
            {
                // 复用结构判定/类别/堆叠包络/数据极值：拖动期间数据未变，逐点扫描全部跳过
                _barMode = dc.BarMode;
                anyAxis = dc.AnyAxis;
                hasCategories = dc.HasCategories;
                _catLabels.Clear();
                _catLabels.AddRange(dc.CatLabels);
                _catCount = dc.CatCount;
                CopyStackCtx(dc.AreaStack, _areaStack);
                CopyStackCtx(dc.ColStack, _colStack);
                yCount = dc.YCount;
                liveFollow = dc.LiveFollow; liveLo = dc.LiveLo; liveHi = dc.LiveHi;
                hMin = dc.HMin; hMax = dc.HMax;
                vMin = (double[])dc.VMin.Clone(); vMax = (double[])dc.VMax.Clone();
                hasY = (bool[])dc.HasY.Clone();
                zeroBaseline = (bool[])dc.ZeroBaseline.Clone();
            }
            else
            {
                // 轴方向/类别/基线等"结构"按区域内全部系列判定：
                // 即使系列全部被取消（IsEnabled=false），坐标系与类别标签仍然保留
                _barMode = barMode0;
                anyAxis = allInArea.Exists(s => IsCartesian(s.ChartType));
                hasCategories = allInArea.Exists(s => IsCartesian(s.ChartType) && s.Points.Exists(p => !p.IsEmpty && p.AxisLabel != null));
                // 类别标签与数量（取第一个带标签的系列；隐藏系列的类别仍然显示在轴上）
                _catLabels.Clear();
                _catCount = 0;
                HChartSeries catSrc = null;
                foreach (var s in allInArea)
                {
                    if (IsCartesian(s.ChartType))
                    {
                        if (hasCategories && catSrc == null && s.Points.Exists(p => p.AxisLabel != null)) catSrc = s;
                        _catCount = Math.Max(_catCount, s.Points.Count);
                    }
                }
                if (catSrc != null)
                    foreach (var p in catSrc.Points) _catLabels.Add(p.AxisLabel ?? "");
                PrepareStacks(list);
                // ---- 实时流动窗口：实时模式未暂停且未勾选"显示全部"时，X 轴只跟随最近一段数据（数据不删），
                //      最新点取各启用系列末点的最大 X；时间轴窗口单位秒、数字轴单位点 ----
                liveFollow = IsLiveMode && !IsLivePaused && !LiveShowAll && !hasCategories && !_barMode;
                if (liveFollow)
                {
                    double latest = double.MinValue;
                    foreach (var s in list)
                        if (IsCartesian(s.ChartType) && s.Points.Count > 0 && !s.Points[s.Points.Count - 1].IsEmpty)
                            latest = Math.Max(latest, s.Points[s.Points.Count - 1].XValue);
                    if (latest == double.MinValue) liveFollow = false;
                    else { liveHi = latest; liveLo = latest - (IsTimeAxis(ca.AxisX) ? LiveWindowSize / 86400.0 : LiveWindowSize); }
                }
                // ---- 收集数值范围：h=横轴数据，vMin/vMax=各 Y 轴各自的数据（0=主Y左1,1=右1,2=左2,3=右2……）----
                hMin = double.MaxValue; hMax = double.MinValue;
                vMin = new double[yCount]; vMax = new double[yCount];
                hasY = new bool[yCount];
                for (int i = 0; i < yCount; i++) { vMin[i] = double.MaxValue; vMax[i] = double.MinValue; }
                foreach (var s in list)
                {
                    if (!IsCartesian(s.ChartType)) continue;
                    // 横向条形模式只使用主 Y 轴（类别轴）；副 Y 轴不支持
                    int yi = _barMode ? 0 : s.YAxisIndex;
                    if (yi < 0 || yi >= yCount) yi = 0;
                    hasY[yi] = true;
                    foreach (var p in s.Points)
                    {
                        if (p.IsEmpty) continue;
                        if (liveFollow && p.XValue < liveLo) continue; // 流动窗口：窗口外的点不参与量程（X/Y 都只跟随最近窗口）
                        if (_barMode)
                        {
                            // 横向条形：数值在横轴(X)，类别索引在纵轴(Y)
                            hMin = Math.Min(hMin, p.YValue); hMax = Math.Max(hMax, p.YValue);
                            vMin[0] = Math.Min(vMin[0], p.XValue); vMax[0] = Math.Max(vMax[0], p.XValue);
                        }
                        else
                        {
                            hMin = Math.Min(hMin, p.XValue); hMax = Math.Max(hMax, p.XValue);
                            vMin[yi] = Math.Min(vMin[yi], p.YValue); vMax[yi] = Math.Max(vMax[yi], p.YValue);
                            foreach (var yv in p.YValues) { vMin[yi] = Math.Min(vMin[yi], yv); vMax[yi] = Math.Max(vMax[yi], yv); }
                        }
                    }
                }
                if (hMin > hMax) { hMin = 0; hMax = 1; }
                for (int i = 0; i < yCount; i++) if (vMin[i] > vMax[i]) { vMin[i] = 0; vMax[i] = 1; }
                // 堆叠累计上界计入对应数值轴（堆叠系列按绑定轴分组）
                if (_areaStack.Active)
                    foreach (var kv in _areaStack.Upper)
                        foreach (var u in kv.Value)
                        {
                            int yi = kv.Key.YAxisIndex;
                            if (yi < yCount) vMax[yi] = Math.Max(vMax[yi], u);
                        }
                if (_colStack.Active)
                    foreach (var kv in _colStack.Upper)
                        foreach (var u in kv.Value)
                        {
                            int yi = kv.Key.YAxisIndex;
                            if (yi < yCount) vMax[yi] = Math.Max(vMax[yi], u);
                        }
                // 各 Y 轴是否需要零基线（柱/面积类）
                zeroBaseline = new bool[yCount];
                for (int zi = 0; zi < yCount; zi++)
                {
                    int z0 = zi;
                    zeroBaseline[zi] = allInArea.Exists(s => ZeroBaseline(s.ChartType) && s.YAxisIndex == z0);
                }
                // 写入缓存供本帧后续十几次 ComputeAxis 复用
                if (dc == null) { dc = new AreaDataCache(); _areaDataCache[ca.Name] = dc; }
                dc.Sig = sig;
                dc.BarMode = _barMode; dc.AnyAxis = anyAxis; dc.HasCategories = hasCategories;
                dc.CatLabels.Clear(); dc.CatLabels.AddRange(_catLabels);
                dc.CatCount = _catCount;
                dc.LiveFollow = liveFollow; dc.LiveLo = liveLo; dc.LiveHi = liveHi;
                dc.HMin = hMin; dc.HMax = hMax;
                dc.VMin = (double[])vMin.Clone(); dc.VMax = (double[])vMax.Clone();
                dc.HasY = (bool[])hasY.Clone(); dc.ZeroBaseline = (bool[])zeroBaseline.Clone();
                dc.YCount = yCount;
                CopyStackCtx(_areaStack, dc.AreaStack);
                CopyStackCtx(_colStack, dc.ColStack);
            }
            // 数据自适应量程（与用户缩放无关，始终是"数据本身"应有的范围；缩放钳制/框选极限以它为准。
            // 绝不能取 Actual*：缩放后 Actual 就是当前视图范围，会把缩小也钳死、最深倍数也跟着漂移）
            double autoXLo, autoXHi;
            var autoYLo = new double[yCount];
            var autoYHi = new double[yCount];
            for (int ai = 0; ai < yCount; ai++) { autoYLo[ai] = double.NaN; autoYHi[ai] = double.NaN; }
            if (_barMode)
            {
                // 横向条形：AxisX=数值轴（含零，末端留 6% 余量），AxisY=类别轴
                double valMin, valMax;
                if (zeroBaseline[0])
                {
                    valMin = Math.Min(0, hMin); valMax = Math.Max(0, hMax);
                    double zpad = (valMax - valMin) * 0.06; if (zpad <= 0) zpad = 1;
                    valMax += zpad; if (valMin < 0) valMin -= zpad;
                }
                else
                {
                    valMin = hMin - (hMax - hMin) * 0.05;
                    valMax = hMax + (hMax - hMin) * 0.05;
                }
                if (valMin == valMax) { valMin -= 1; valMax += 1; }
                ca.AxisX.ActualMinimum = ca.AxisX.Minimum ?? valMin;
                ca.AxisX.ActualMaximum = ca.AxisX.Maximum ?? valMax;
                ca.AxisX.IsCategoryAxis = false;
                ca.AxisY.ActualMinimum = ca.AxisY.Minimum ?? -0.5;
                ca.AxisY.ActualMaximum = ca.AxisY.Maximum ?? Math.Max(1, _catCount) - 0.5;
                ca.AxisY.IsCategoryAxis = true;
                autoXLo = valMin; autoXHi = valMax;
                autoYLo[0] = -0.5; autoYHi[0] = Math.Max(1, _catCount) - 0.5;
            }
            else
            {
                if (liveFollow)
                {
                    // 实时流动窗口：右端严格对齐最新点、左端=最新点-窗口宽，不加 pad，波形匀速左移
                    ca.AxisX.ActualMinimum = liveLo;
                    ca.AxisX.ActualMaximum = liveHi;
                    ca.AxisX.IsCategoryAxis = false;
                    autoXLo = liveLo; autoXHi = liveHi;
                }
                else if (hasCategories)
                {
                    // 类别轴范围 -0.5 .. n-0.5，使每个类别居中于自己的槽位、边缘柱不被裁切
                    ca.AxisX.ActualMinimum = ca.AxisX.Minimum ?? -0.5;
                    ca.AxisX.ActualMaximum = ca.AxisX.Maximum ?? Math.Max(1, _catCount) - 0.5;
                    ca.AxisX.IsCategoryAxis = true;
                    autoXLo = -0.5; autoXHi = Math.Max(1, _catCount) - 0.5;
                }
                else
                {
                    double pad = (hMax - hMin) * 0.05; if (pad <= 0) pad = 1;
                    ca.AxisX.ActualMinimum = ca.AxisX.Minimum ?? (hMin - pad);
                    ca.AxisX.ActualMaximum = ca.AxisX.Maximum ?? (hMax + pad);
                    ca.AxisX.IsCategoryAxis = false;
                    autoXLo = hMin - pad; autoXHi = hMax + pad;
                }
                // 各 Y 轴各自独立量程（绑定该轴的系列数据决定范围）
                for (int yi = 0; yi < yCount; yi++)
                {
                    var ay = YAxisAt(ca, yi);
                    if (!hasY[yi]) continue;
                    double vLo, vHi;
                    if (zeroBaseline[yi])
                    {
                        // 数值轴必须含 0，且顶端留出约 6% 余量，避免最高柱/面积顶到绘图区边框
                        vLo = Math.Min(0, vMin[yi]); vHi = Math.Max(0, vMax[yi]);
                        double zpad = (vHi - vLo) * 0.06; if (zpad <= 0) zpad = 1;
                        vHi += zpad; if (vLo < 0) vLo -= zpad;
                    }
                    else
                    {
                        double pad = (vMax[yi] - vMin[yi]) * 0.1;
                        if (pad <= 0) pad = Math.Abs(vMax[yi]) * 0.1 + 0.1;
                        vLo = vMin[yi] - pad; vHi = vMax[yi] + pad;
                    }
                    if (vLo == vHi) { vLo -= 1; vHi += 1; }
                    ay.ActualMinimum = ay.Minimum ?? vLo;
                    ay.ActualMaximum = ay.Maximum ?? vHi;
                    ay.IsCategoryAxis = false;
                    autoYLo[yi] = vLo; autoYHi[yi] = vHi;
                }
            }
            double xRange = ca.AxisX.ActualMaximum - ca.AxisX.ActualMinimum;
            ca.AxisX.ActualInterval = IsTimeAxis(ca.AxisX)
                ? ComputeTimeInterval(xRange)
                : ComputeNiceInterval(xRange, 10);
            ca.AxisY.ActualInterval = ComputeNiceInterval(ca.AxisY.ActualMaximum - ca.AxisY.ActualMinimum, 8);
            ca.AxisX.IsEnabled = anyAxis;
            // 主 Y 轴（左1）与附加轴同口径：无绑定的启用系列时收起（不画刻度/标签/网格，左侧只留坐标框边线，
            // inner 不再预留左带宽）；横向条形模式主 Y 轴是类别轴，始终保留
            ca.AxisY.IsEnabled = anyAxis && (hasY[0] || _barMode);
            // 附加 Y 轴：有绑定系列且非横向条形时启用；系列全部取消后自动收起
            for (int yi = 1; yi < yCount; yi++)
            {
                var ay = YAxisAt(ca, yi);
                ay.IsEnabled = anyAxis && hasY[yi] && !_barMode;
                if (ay.IsEnabled)
                    ay.ActualInterval = ComputeNiceInterval(ay.ActualMaximum - ay.ActualMinimum, 8);
            }
            // 写入自动量程缓存（供缩放钳制/拖动边界约束；恒为数据自适应范围，未绑定数据的轴为 NaN）
            var auto = new AutoRange { XMin = autoXLo, XMax = autoXHi };
            auto.Reset(yCount);
            for (int yi = 0; yi < yCount; yi++)
            {
                if (!double.IsNaN(autoYLo[yi])) { auto.YMin[yi] = autoYLo[yi]; auto.YMax[yi] = autoYHi[yi]; }
            }
            _autoMinMax[ca.Name] = auto;
        }
        /// <summary>构建堆叠系列（StackedColumn / StackedArea）的累计上下包络。</summary>
        private void PrepareStacks(List<HChartSeries> list)
        {
            _areaStack.Active = false; _colStack.Active = false;
            _areaStack.Lower.Clear(); _areaStack.Upper.Clear();
            _colStack.Lower.Clear(); _colStack.Upper.Clear();
            var colSeries = list.FindAll(s => s.ChartType == HChartType.StackedColumn);
            if (colSeries.Count > 0)
            {
                int n = 0; foreach (var s in colSeries) n = Math.Max(n, s.Points.Count);
                _colStack.Xs = new double[n];
                for (int i = 0; i < n; i++) _colStack.Xs[i] = i;
                var cum = new double[n];
                foreach (var s in colSeries)
                {
                    var low = new double[n]; var up = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        double v = (i < s.Points.Count && !s.Points[i].IsEmpty) ? Math.Max(0, s.Points[i].YValue) : 0;
                        low[i] = cum[i]; up[i] = cum[i] + v; cum[i] = up[i];
                    }
                    _colStack.Lower[s] = low; _colStack.Upper[s] = up;
                }
                _colStack.Active = true;
            }
            var areaSeries = list.FindAll(s => s.ChartType == HChartType.StackedArea);
            if (areaSeries.Count > 0)
            {
                var xset = new SortedDictionary<double, double>();
                foreach (var s in areaSeries)
                    foreach (var p in s.Points)
                        if (!p.IsEmpty) xset[Math.Round(p.XValue, 6)] = p.XValue;
                double[] xs = new double[xset.Count];
                xset.Values.CopyTo(xs, 0);
                _areaStack.Xs = xs;
                var cum = new double[xs.Length];
                foreach (var s in areaSeries)
                {
                    var low = new double[xs.Length]; var up = new double[xs.Length];
                    for (int i = 0; i < xs.Length; i++)
                    {
                        double v = ValueNearX(s, xs[i]);
                        low[i] = cum[i]; up[i] = cum[i] + v; cum[i] = up[i];
                    }
                    _areaStack.Lower[s] = low; _areaStack.Upper[s] = up;
                }
                _areaStack.Active = true;
            }
        }
        /// <summary>查找系列中 X 最接近 x 的数据点的 Y 值（无匹配返回 0）。</summary>
        private static double ValueNearX(HChartSeries s, double x)
        {
            double best = 0, bd = double.MaxValue;
            foreach (var p in s.Points)
            {
                if (p.IsEmpty) continue;
                double d = Math.Abs(p.XValue - x);
                if (d < bd) { bd = d; best = p.YValue; }
            }
            return bd < 1e-6 ? best : 0;
        }
        /// <summary>按当前 ChartArea 的 inner 绘图区与轴范围配置 HScreen 坐标映射器（主 X/Y）。</summary>
        private void SetupScreen(RectangleF inner, HChartAxis ax, HChartAxis ay)
        {
            double rx = ax.ActualMaximum - ax.ActualMinimum;
            double ry = ay.ActualMaximum - ay.ActualMinimum;
            // NaN/Infinity/<=0 一律回退 1，0 基回退，保证 scale/offset 永远是有限数（否则点映射成 NaN 会画出大叉）
            if (!IsFinite(rx) || rx <= 0) rx = 1;
            if (!IsFinite(ry) || ry <= 0) ry = 1;
            double xMin = IsFinite(ax.ActualMinimum) ? ax.ActualMinimum : 0;
            double yMin = IsFinite(ay.ActualMinimum) ? ay.ActualMinimum : 0;
            double sx = inner.Width / rx;
            double sy = inner.Height / ry;
            _screen.ScreenRectangle = new HRect(inner.Left, inner.Top, inner.Width, inner.Height);
            _screen.ViewScaleX = sx;
            _screen.ViewScaleY = sy;
            _screen.ViewOffset = new HPoint(-xMin * sx, -yMin * sy);
        }
        /// <summary>配置主映射器 _screen 与全部附加 Y 轴映射器（X 映射共享，各 Y 缩放独立；未启用的轴占位回退）。</summary>
        private void SetupScreens(RectangleF inner, HChartArea ca)
        {
            var ax = ca.AxisX;
            SetupScreen(inner, ax, ca.AxisY);
            // 轴 → 映射器对照表重建：主轴（YAxes[0]）用 _screen，其余副轴复用 _yScreens（下标=轴索引-1）
            _screenByAxis.Clear();
            _screenByAxis[ca.YAxes[0]] = _screen;
            double rx = ax.ActualMaximum - ax.ActualMinimum;
            if (!IsFinite(rx) || rx <= 0) rx = 1;
            double sx = inner.Width / rx;
            for (int k = 1; k < ca.YAxes.Count; k++)
            {
                int j = k - 1;
                while (_yScreens.Count <= j) _yScreens.Add(new HScreen());
                var ay = ca.YAxes[k];
                ConfigureScreen(_yScreens[j], inner, ax, ay, sx);
                if (ay.IsEnabled) _screenByAxis[ay] = _yScreens[j];
            }
        }
        /// <summary>配置单个附加 Y 轴映射器；轴未启用时用量程 0~1 占位（不会被实际使用，仅保证比例有效）。</summary>
        private void ConfigureScreen(HScreen scr, RectangleF inner, HChartAxis ax, HChartAxis ay, double sx)
        {
            // 未启用时用量程 0~1 占位（不会被实际使用，仅保证映射器比例有效）
            double ry;
            double yMin;
            if (ay.IsEnabled) { ry = ay.ActualMaximum - ay.ActualMinimum; yMin = ay.ActualMinimum; }
            else { ry = 1; yMin = 0; }
            if (!IsFinite(ry) || ry <= 0) ry = 1;
            if (!IsFinite(yMin)) yMin = 0;
            double sy = inner.Height / ry;
            double xMin = IsFinite(ax.ActualMinimum) ? ax.ActualMinimum : 0;
            scr.ScreenRectangle = new HRect(inner.Left, inner.Top, inner.Width, inner.Height);
            scr.ViewScaleX = sx;
            scr.ViewScaleY = sy;
            scr.ViewOffset = new HPoint(-xMin * sx, -yMin * sy);
        }
        /// <summary>按量程和目标刻度数算"好看"的刻度步长（1/2/5 ×10ⁿ 序列），保证刻度稀疏适中、数值整齐。</summary>
        private double ComputeNiceInterval(double range, int targetTicks)
        {
            if (!IsFinite(range) || range <= 0) range = 1;
            double raw = range / targetTicks;
            double pow = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double frac = raw / pow;
            double nice = frac <= 1.0 ? 1.0 : frac <= 2.5 ? 2.0 : frac <= 5.0 ? 5.0 : 10.0;
            return nice * pow;
        }
        /// <summary>1/2/2.5/5 阶梯上"不大于 raw 的最大"好看步长（刻度需要加密时用，与 ComputeNiceInterval 互为反方向）。</summary>
        private static double NiceStepAtMost(double raw)
        {
            if (!IsFinite(raw) || raw <= 0) return 1;
            double pow = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double frac = raw / pow;
            if (frac >= 5.0) return 5.0 * pow;
            if (frac >= 2.5) return 2.5 * pow;
            if (frac >= 2.0) return 2.0 * pow;
            if (frac >= 1.0) return 1.0 * pow;
            return 5.0 * pow / 10.0; // 落到 0.5、0.05 …
        }
        /// <summary>该轴是否为时间轴（TimeFormat 非空：X 值是 OADate，按时间格式显示）。</summary>
        private static bool IsTimeAxis(HChartAxis ax) => !string.IsNullOrEmpty(ax.TimeFormat);
        /// <summary>柱/条类图表（气泡与标尺弹窗要显示系列总数）：Column / StackedColumn / Bar。</summary>
        private static bool IsBarType(HChartType t)
            => t == HChartType.Column || t == HChartType.StackedColumn || t == HChartType.Bar;
        /// <summary>时间长度格式化（入参为秒）：Xd Xh Xmin Xs，0 值单位省略（0d 不显示天，其余同理）；
        /// 不足 1 分钟（只剩 s 一个单位）且秒带 5 位小数时，改用 ms 为单位（实时图毫秒级测量）。</summary>
        private static string FormatDuration(double totalSec)
        {
            if (totalSec < 0) totalSec = -totalSec;
            long whole = (long)Math.Floor(totalSec);
            long days = whole / 86400;
            long hours = whole / 3600 % 24;
            long mins = whole / 60 % 60;
            // 扣掉 d/h/min 后的剩余秒（含整秒：4.32s 的 4 不能丢）
            double secs = totalSec - days * 86400 - hours * 3600 - mins * 60;
            var parts = new List<string>();
            if (days > 0) parts.Add(days + "d");
            if (hours > 0) parts.Add(hours + "h");
            if (mins > 0) parts.Add(mins + "min");
            string ss = FormatTick(secs);
            bool secZero = ss == "0";
            if (!secZero || parts.Count == 0)
            {
                // 只剩 s 且不足 1 秒：秒值带 5 位小数（毫秒级精度）改用 ms（4.5s 仍显示 s）
                int dot = ss.IndexOf('.');
                if (parts.Count == 0 && totalSec < 1.0 && dot >= 0 && ss.Length - dot - 1 >= 5)
                    return FormatTick(totalSec * 1000.0) + "ms";
                parts.Add(ss + "s");
            }
            return string.Join(" ", parts);
        }
        /// <summary>屏幕 X（控件坐标）→ 世界 X 的线性精确换算（不走 HScreen 的显示精度截断：
        /// 时间轴 OADate 截到 5 位小数会变成 0.864s 网格，毫秒级标尺无法测量）。</summary>
        private static double ScreenXToWorld(RectangleF inner, HChartAxis ax, float screenX)
        {
            double rx = ax.ActualMaximum - ax.ActualMinimum;
            return ax.ActualMinimum + (screenX - inner.Left) / inner.Width * rx;
        }
        /// <summary>时间格式串的行数（"\n" 或字面 \n 拆行，如日期/时间两行）。</summary>
        private static int TimeLabelLines(HChartAxis ax)
            => ax.TimeFormat.Replace("\\n", "\n").Split('\n').Length;
        /// <summary>轴刻度文字：时间轴按 TimeFormat 格式化 OADate（支持换行），
        /// 其余走数值统一格式 FormatTick，小数位数取该轴 TickDecimalPlaces（默认 6）。</summary>
        private static string FormatAxisTick(HChartAxis ax, double v)
        {
            if (string.IsNullOrEmpty(ax.TimeFormat)) return FormatTick(v, ax.TickDecimalPlaces);
            if (double.IsNaN(v) || double.IsInfinity(v)) return "";
            string fmt = ax.TimeFormat.Replace("\\n", "\n");
            return DateTime.FromOADate(v).ToString(fmt);
        }
        /// <summary>弹窗（悬停单点/标尺区间）用的 X 值文字：时间轴统一完整到毫秒
        /// yyyy-MM-dd HH:mm:ss.fff（与轴刻度的两行短格式无关），其余轴走数值统一格式。</summary>
        private static string FormatPopupX(HChartAxis ax, double v)
        {
            if (string.IsNullOrEmpty(ax.TimeFormat)) return FormatTick(v);
            if (double.IsNaN(v) || double.IsInfinity(v)) return "";
            return DateTime.FromOADate(v).ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        /// <summary>时间轴好看的刻度步长（单位：天）：默认约 10 个刻度。</summary>
        private static double ComputeTimeInterval(double rangeDays) => ComputeTimeInterval(rangeDays, 10);
        /// <summary>时间轴好看的刻度步长（单位：天）：在 1/2/5 的秒·分·时·天阶梯上取 ≥ 目标步长的第一档，刻度永远落在整秒边界。
        /// targetTicks 为期望刻度数（X 轴竖排时按横向可容纳的字高列数给出，比横排更密）。</summary>
        private static double ComputeTimeInterval(double rangeDays, int targetTicks)
        {
            // 目标步长（秒）；阶梯：1/2/5 × 10ⁿ 秒，到 60 秒后按 1/2/5 分、时、天递进
            double target = Math.Max(rangeDays, 1e-9) / Math.Max(1, targetTicks) * 86400.0;
            double[] ladderSec =
            {
                1, 2, 5, 10, 15, 30,
                60, 120, 300, 600, 900, 1800,
                3600, 7200, 10800, 21600, 43200, 86400
            };
            foreach (double sec in ladderSec)
                if (sec >= target) return sec / 86400.0;
            // 超过 1 天：按天走 1/2/5 ×10ⁿ
            double days = target / 86400.0;
            double pow = Math.Pow(10, Math.Floor(Math.Log10(days)));
            double frac = days / pow;
            double nice = frac <= 1.0 ? 1.0 : frac <= 2.5 ? 2.0 : frac <= 5.0 ? 5.0 : 10.0;
            return nice * pow;
        }
        /// <summary>
        /// 刻度/数值统一格式化（X/Y 轴、标尺、tooltip、柱标签全走这里，格式完全一致）：
        ///   - 绝对值 ≥10000：科学计数法 "d.d…×10ⁿ"（系数最多 places 位小数，Unicode 上标）；
        ///   - 其余（含很小的数）：普通小数，最多 places 位小数，自动去尾零；
        ///     小到 places 位小数四舍五入后为 0 时显示 "0"（不用负指数形式）；
        ///   - 0 显示 "0"；非有限数返回空串。
        /// places 为最多保留小数位数：轴刻度由 HChartAxis.TickDecimalPlaces 传入（默认 6），弹窗等默认 6。
        /// </summary>
        private static string FormatTick(double v, int places = 6)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return "";
            if (v == 0) return "0";
            string frac = new string('#', places);
            double a = Math.Abs(v);
            if (a >= 1e4)
            {
                int n = (int)Math.Floor(Math.Log10(a));
                double coef = v / Math.Pow(10, n);   // 带符号系数，|coef|∈[1,10)
                coef = Math.Round(coef, places);
                if (Math.Abs(coef) >= 10) { coef /= 10; n++; }   // 修约进位（如 9.999996→10）
                if (coef == 0) return "0";
                return coef.ToString("0." + frac) + "×10" + ToSuperscript(n);
            }
            return v.ToString("0." + frac);
        }
        /// <summary>整数转 Unicode 上标字符串（负号用 ⁻），用于科学计数法指数。</summary>
        private static string ToSuperscript(int n)
        {
            const string sup = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string s = n.ToString();
            var chars = new char[s.Length];
            for (int i = 0; i < s.Length; i++)
                chars[i] = s[i] == '-' ? '⁻' : sup[s[i] - '0'];
            return new string(chars);
        }
        /// <summary>
        /// 固定窄矩形（仅 X 类别轴 60px 槽位）宽度适配：原文放得下直接返回；放不下则截前缀 + "&gt;"，
        /// 前缀尽量长、至少保留 4 个字符（"4 位 + &gt;"）。
        /// 注意：X/Y 数值刻度的矩形宽度都按实测文字宽度给出，永不走此截短路径（数字完整显示）。
        /// </summary>
        private static string FitTickLabel(Graphics g, Font font, string text, float maxW)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (g.MeasureString(text, font).Width <= maxW) return text;
            for (int len = Math.Min(text.Length - 1, 8); len >= 4; len--)
            {
                string cand = text.Substring(0, len) + ">";
                if (g.MeasureString(cand, font).Width <= maxW) return cand;
            }
            return text.Substring(0, Math.Min(4, text.Length)) + ">";
        }
        /// <summary>
        /// 轴刻度文字绘制（X/Y 轴共用一套方法，保证字号/格式/对齐风格完全一致）：
        /// 先用轴字体量文字长宽，超出给定矩形宽度 → 降为 7f 小字号，仍超 → 按像素截短加 "&gt;" 兜底；
        /// 文字在矩形内垂直居中、水平按 align 对齐（X 数值轴 Center、Y 轴左侧 Far/右侧 Near），不换行、超长省略。
        /// 数值刻度（X/Y）的矩形宽均由调用方按实测文字宽度给出，一级即通过——数字永远以原字号完整显示，
        /// 小字号/截短兜底只可能出现在 X 类别轴的固定 60px 槽位。
        /// </summary>
        private static void DrawTickLabel(Graphics g, string text, Font font, Brush brush,
            RectangleF rect, StringAlignment align)
        {
            Font use = font;
            // 一级：原字号放得下直接用；二级：7f 小字号；三级：截短 + ">"
            if (g.MeasureString(text, use).Width > rect.Width)
            {
                use = YTickSmallFont;
                if (g.MeasureString(text, use).Width > rect.Width)
                    text = FitTickLabel(g, use, text, rect.Width);
            }
            var sf = new StringFormat
            {
                Alignment = align,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(text, use, brush, rect, sf);
        }
        // ============ 绘制单个 ChartArea ============
        /// <summary>
        /// 绘制单个图表区：背景/边框 → inner 绘图区 → 坐标映射器 → 轴/网格 → 各系列数据 → 图例。
        /// 只要有任何 Series 归属此区域就绘制框架（系列全部禁用时仍画坐标系与图例，方便点图例恢复）；
        /// 笛卡尔类型系列统一用 inner 矩形做裁剪，Pie/Doughnut/Funnel/Radar 按整个 area 自由布局。
        /// </summary>
        /// <param name="g">画布。</param>
        /// <param name="ca">要绘制的图表区。</param>
        /// <param name="rect">图表区在控件中的可用矩形（百分比位置 Position 以此为基准换算）。</param>
        private void DrawChartArea(Graphics g, HChartArea ca, RectangleF rect)
        {
            // 只要有任何 Series 属于此 ChartArea，就继续绘制 ChartArea 框架
            // （即使所有 Series 都 IsEnabled=false，也要画背景 + 图例，让用户能点图例恢复显示）
            var belongingSeries = new List<HChartSeries>();
            foreach (var s in Series) if (s.ChartArea == ca.Name) belongingSeries.Add(s);
            if (belongingSeries.Count == 0) return;
            // 实际启用、参与数据绘制的 Series
            var activeSeries = belongingSeries.FindAll(s => s.IsEnabled);
            // 先算 area / paddedArea / inner，再 ComputeAxis — 密度检查需要 inner 像素尺寸
            var area = new RectangleF(
                rect.Left + rect.Width * ca.Position.Left / 100f,
                rect.Top + rect.Height * ca.Position.Top / 100f,
                rect.Width * ca.Position.Width / 100f,
                rect.Height * ca.Position.Height / 100f);
            using (var bg = new SolidBrush(ca.BackColor)) g.FillRectangle(bg, area);
            // 整体四边向内缩 4px 作为 Padding，画边框线；后续 inner 基于缩后的 paddedArea 计算
            const float pad = 4f;
            var paddedArea = new RectangleF(area.Left + pad, area.Top + pad, area.Width - 2 * pad, area.Height - 2 * pad);
            using (var borderPen = new Pen(Color.FromArgb(180, 186, 196), 1f))
                g.DrawRectangle(borderPen, paddedArea.Left, paddedArea.Top, paddedArea.Width, paddedArea.Height);
            // inner 动态内缩：先实测各 Y 轴刻度数字宽度（数字长→带宽大→坐标系压缩），再按带宽预留
            // GetInnerPlotRect 内部会幂等地跑一次 ComputeAxis，SetupScreens/DrawAxisY 都基于这个 inner
            ComputeAxis(ca);
            MeasureAxisBands(g, ca, paddedArea.Width);
            var inner = GetInnerPlotRect(ca);
            // 缩放/平移挂起的世界锚点在此帧内收敛：测量带宽→平移量程→再测直到 inner 稳定
            inner = ApplyPendingAnchor(g, ca, paddedArea, inner);
            // 配置 HScreen 坐标映射器（世界↔屏幕）：主映射器 + 副 Y 轴映射器
            SetupScreens(inner, ca);
            // 硬保证：轴刻度文字、轴标题、自由布局系列（饼/环/漏斗/雷达）的标签、图例一律不越出
            // "控件减去 Padding"的边框（paddedArea）；布局本就把它们收在框内，clip 是最后一道防线
            var frameClip = g.Save();
            g.SetClip(paddedArea, CombineMode.Replace);
            // 轴：只要该区域结构上是笛卡尔图表就始终绘制（系列全部取消时也保留坐标系/网格/类别标签；
            // Pie/Doughnut/Funnel/Radar 区域的轴已被 ComputeAxis 置为 IsEnabled=false）
            // 多 Y 轴：yi=0 主Y 左1（带网格），1 右1、2 左2、3 右2…… 自内向外交替；副轴只画轴线/刻度/标签/标题
            // 主 Y 轴（含水平网格）先画，X 轴线压在网格之上，最后画附加 Y 轴（标签/标题在外围带内）
            if (ca.AxisY.IsEnabled) DrawAxisY(g, ca, ca.AxisY, inner, 0, true);
            if (ca.AxisX.IsEnabled) DrawAxisX(g, ca.AxisX, inner, true);
            for (int yi = 1; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (ay.IsEnabled) DrawAxisY(g, ca, ay, inner, yi, false);
            }
            // 某侧 Y 轴全部取消（系列都点掉后附加轴自动收起）：该侧无刻度/无标签，
            // 但沿 inner 边缘补一条坐标框边线——inner 已贴 paddedArea 内边，与其余三边框线齐平
            if (ca.AxisX.IsEnabled)
            {
                bool leftOn = false, rightOn = false;
                for (int yi = 0; yi < YAxisCount(ca); yi++)
                {
                    if (!YAxisAt(ca, yi).IsEnabled) continue;
                    if (YAxisSide(yi) > 0) rightOn = true; else leftOn = true;
                }
                if (!leftOn || !rightOn)
                    using (var edgePen = new Pen(ca.AxisX.LineColor, ca.AxisX.LineWidth))
                    {
                        if (!leftOn) g.DrawLine(edgePen, inner.Left, inner.Top, inner.Left, inner.Bottom);
                        if (!rightOn) g.DrawLine(edgePen, inner.Right, inner.Top, inner.Right, inner.Bottom);
                    }
            }
            // 雷达底图：即使所有雷达系列都被图例取消，也要画同心网格/辐条/维度标签（系列多边形由下方循环画）
            var radarAll = belongingSeries.FindAll(s => s.ChartType == HChartType.Radar);
            if (radarAll.Count > 0) DrawRadarFrame(g, paddedArea, ca, radarAll);
            // 饼/环标签避让图例：图例框几何与 DrawLegend 同源预计算（画系列前就有有效 Bounds，
            // 不再依赖"上一帧残留"导致首帧标签压在图例文字上）
            foreach (var lg in Legends) MeasureLegendBox(g, ca, inner, lg);
            // 系列绘制前用 inner 矩形做 clip：任何绘图内容（线/柱/面积/散点/K线/柱体/气泡等）
            // 一旦超出坐标系范围就被裁掉，保证数据任何情况下都不越过坐标轴框线
            bool clipData = !IsCartesian(activeSeries.FirstOrDefault()?.ChartType ?? HChartType.Line) ? false : true;
            // 但 Pie/Doughnut/Funnel/Radar 不走 inner 坐标系，它们不受 clip 影响
            // 这里简化处理：只要不是 Pie/Doughnut/Funnel/Radar 的区域就 clip inner
            foreach (var s in belongingSeries)
            {
                if (IsCartesian(s.ChartType)) { clipData = true; break; }
            }
            // 硬保证（frameClip）已在画轴前套上：下面数据再叠一层 inner clip
            GraphicsState gs = default;
            if (clipData)
            {
                gs = g.Save();
                // 内缩 0.5px：避免反锯齿笔画压到坐标轴框线上，数据严格画在坐标系里面
                g.SetClip(new RectangleF(inner.X + 0.5f, inner.Y + 0.5f, inner.Width - 1f, inner.Height - 1f),
                    CombineMode.Replace);
            }
            // 并排分组计数（堆叠类单独走累计包络，不参与并排）
            int colCount = activeSeries.FindAll(s => s.ChartType == HChartType.Column).Count;
            int barCount = activeSeries.FindAll(s => s.ChartType == HChartType.Bar).Count;
            int candleCount = activeSeries.FindAll(s => s.ChartType == HChartType.Candlestick).Count;
            int boxCount = activeSeries.FindAll(s => s.ChartType == HChartType.BoxPlot).Count;
            int piCol = 0, piBar = 0, piCandle = 0, piBox = 0;
            int paletteIndex = 0;
            var seriesColor = new Dictionary<HChartSeries, Color>();
            foreach (var s in activeSeries)
            {
                var color = s.ResolveColor(paletteIndex++, Palette, ThemeIsDark);
                seriesColor[s] = color;
                // 多 Y 轴：系列按 YAxisIndex 绑定到对应 Y 轴（绑定轴被收起时回退主轴；横向条形只用主轴）
                var ayBind = YAxisAt(ca, _barMode ? 0 : s.YAxisIndex);
                var ay = ayBind.IsEnabled ? ayBind : ca.AxisY;
                switch (s.ChartType)
                {
                    case HChartType.Line:
                    case HChartType.Spline:
                    case HChartType.StepLine:
                    case HChartType.Area:
                        DrawLineSeries(g, ca.AxisX, ay, inner, s, color); break;
                    case HChartType.StackedArea:
                        DrawStackedAreaSeries(g, ca.AxisX, ay, inner, s, color); break;
                    case HChartType.Column:
                        DrawColumnSeries(g, ca.AxisX, ay, inner, s, color, piCol++, colCount); break;
                    case HChartType.StackedColumn:
                        DrawStackedColumnSeries(g, ca.AxisX, ay, inner, s, color); break;
                    case HChartType.Bar:
                        DrawBarSeries(g, ca.AxisX, ay, inner, s, color, piBar++, barCount); break;
                    case HChartType.Candlestick:
                        DrawCandlestickSeries(g, ca.AxisX, ay, inner, s, piCandle++, candleCount); break;
                    case HChartType.BoxPlot:
                        DrawBoxPlotSeries(g, ca.AxisX, ay, inner, s, color, piBox++, boxCount); break;
                    case HChartType.Point:
                        DrawPointSeries(g, ca.AxisX, ay, inner, s, color); break;
                    case HChartType.Pie:
                    case HChartType.Doughnut:
                        // 自由布局内容框 = paddedArea：饼心/半径/标签全部收在"控件减 Padding"框内
                        DrawPieSeries(g, paddedArea, ca, s); break;
                    case HChartType.Radar:
                        DrawRadarSeries(g, paddedArea, activeSeries, s, color); break;
                    case HChartType.Bubble:
                        DrawBubbleSeries(g, ca.AxisX, ay, inner, s, color); break;
                    case HChartType.Funnel:
                        DrawFunnelSeries(g, paddedArea, ca, s); break;
                    case HChartType.PointAndFigure:
                        DrawPointAndFigureSeries(g, ca.AxisX, ay, inner, s, color); break;
                    default:
                        DrawPointSeries(g, ca.AxisX, ay, inner, s, color); break;
                }
            }
            // 恢复 clip（后续图例、叠加 HUD 不应被 inner 边界裁掉）
            if (clipData) g.Restore(gs);
            // 图例：内嵌在坐标系绘图区（inner）右上角，半透明、受限宽高、超出可滚动
            foreach (var lg in Legends) DrawLegend(g, lg, ca, inner, seriesColor);
            // 解除 paddedArea 硬裁剪（框选矩形/气泡等交互覆盖层允许画在 Padding 带上）
            g.Restore(frameClip);
        }
        // ============ 轴 ============
        /// <summary>
        /// 自研线型枚举转 GDI+ 的 DashStyle（网格线/边框线使用）。
        /// None（不画线）映射为 Custom 且不设置虚线图案，调用方通常直接跳过画线。
        /// </summary>
        private DashStyle ToDashStyle(HChartLineDashStyle s)
        {
            switch (s)
            {
                case HChartLineDashStyle.Dash: return DashStyle.Dash;
                case HChartLineDashStyle.Dot: return DashStyle.Dot;
                case HChartLineDashStyle.DashDot: return DashStyle.DashDot;
                case HChartLineDashStyle.DashDotDot: return DashStyle.DashDotDot;
                case HChartLineDashStyle.None: return DashStyle.Custom;
                default: return DashStyle.Solid;
            }
        }
        /// <summary>轴标题文字：设置了单位时渲染为“名称 (单位)”。</summary>
        /// <summary>拼接轴标题：Title 和 Unit 独立判空，null/空都不显示对应部分；都没有则返回 null。</summary>
        private static string AxisTitleText(HChartAxis ax)
        {
            bool hasTitle = !string.IsNullOrEmpty(ax.Title);
            bool hasUnit  = !string.IsNullOrEmpty(ax.Unit);
            if (hasTitle && hasUnit) return $"{ax.Title} ({ax.Unit})";
            if (hasTitle) return ax.Title;
            if (hasUnit)  return ax.Unit;  // 只有单位没标题的情况（少见但合理）
            return null;
        }
        /// <summary>单位字体：比轴名称字体小 1px（名称 9f → 单位 8f）。</summary>
        private static Font UnitFont(Font nameFont)
            => new Font(nameFont.FontFamily, Math.Max(6f, nameFont.Size - 1f), FontStyle.Regular);
        /// <summary>
        /// 绘制 X 轴：轴线 + 刻度线 + 垂直网格 + 刻度标签 + 轴标题。
        /// 类别轴（纵向）按槽位居中画类别名，槽位过密时按 stride 抽稀、末尾标签保留；
        /// 数值轴先按标签宽度把刻度间隔翻倍粗调，再像素级贪心跳过会重叠的标签；
        /// 横向条形图（_barMode）的类别标签由 DrawAxisY 侧绘制，本方法只画轴线。
        /// </summary>
        /// <param name="g">画布。</param>
        /// <param name="ax">X 轴对象。</param>
        /// <param name="inner">绘图区内矩形。</param>
        /// <param name="atBottom">true=轴在绘图区底部（默认）；false=顶部（预留）。</param>
        private void DrawAxisX(Graphics g, HChartAxis ax, RectangleF inner, bool atBottom)
        {
            float y = atBottom ? inner.Bottom : inner.Top;
            using (var pen = new Pen(ax.LineColor, ax.LineWidth))
                g.DrawLine(pen, inner.Left, y, inner.Right, y);
            var labelBrush = new SolidBrush(ax.LabelForeColor);
            try
            {
                if (ax.IsCategoryAxis && !_barMode)
                {
                    // 纵向图表的 X 类别轴：标签居中于各槽位；槽位太密时隔几个画一个，防止文字重叠
                    int n = _catLabels.Count;
                    // 相邻标签之间至少留出一个文字的空白（按字号字高计一个汉字见方），刻度文字绝不相邻/重叠
                    float charGap = ax.LabelStyleFont.Height;
                    // 测最宽类别标签，算最小槽位像素
                    float labelMaxW = 1f;
                    for (int i = 0; i < n; i++)
                        labelMaxW = Math.Max(labelMaxW, g.MeasureString(_catLabels[i], ax.LabelStyleFont).Width);
                    float slotW = n > 0 ? inner.Width / n : inner.Width;
                    bool rot = _xRotate.TryGetValue(ax, out bool rotCat) && rotCat;
                    float lineH = ax.LabelStyleFont.GetHeight();
                    // 横排按标签宽抽稀；竖排后每条 X 向只占一个字高，按字高抽稀（槽位够时全部显示）
                    int stride = rot
                        ? Math.Max(1, (int)Math.Ceiling((lineH + charGap) / Math.Max(1f, slotW)))
                        : Math.Max(1, (int)Math.Ceiling((labelMaxW + charGap) / Math.Max(1f, slotW)));
                    float lastRight = float.NegativeInfinity;   // 横排像素级贪心：与上一标签右缘间隔不足一个字就跳过
                    float lastStripRight = float.NegativeInfinity; // 竖排条带贪心：上一条带右缘
                    for (int i = 0; i < n; i++)
                    {
                        float xPx = MapX(i, ax, inner);
                        // 槽位中心已在坐标区窗外：网格/刻度/文字全部不画（缩放平移后不再把窗外标签夹进来）
                        if (xPx < inner.Left || xPx > inner.Right) continue;
                        // 末尾标签始终保留（与 stride 命中不重叠时）
                        bool strideHit = i % stride == 0 || i == n - 1;
                        if (ax.MajorGridEnabled)
                            using (var gp = new Pen(ax.MajorGridColor) { DashStyle = ToDashStyle(ax.MajorGridDashStyle) })
                                g.DrawLine(gp, xPx, inner.Top, xPx, inner.Bottom);
                        if (ax.MajorTickMarkEnabled) g.DrawLine(Pens.Gray, xPx, y, xPx, y + 5);
                        if (rot)
                        {
                            if (!strideHit) continue;
                            // 竖排条带水平宽=字高，中心夹住不与 Y 轴线重合
                            float cx = Math.Max(inner.Left + lineH / 2f, Math.Min(inner.Right - lineH / 2f, xPx));
                            // 像素级贪心：相邻条带间隔不足一个文字宽就跳过（间隔用自然刻度位置判定）
                            if (xPx - lineH / 2f < lastStripRight + charGap) continue;
                            lastStripRight = xPx + lineH / 2f;
                            DrawRotatedTick(g, _catLabels[i], ax.LabelStyleFont, labelBrush, cx, y + 5f);
                        }
                        else
                        {
                            // 文字不与 Y 轴重合：标签中心夹在 inner 范围内（位置/宽度按槽位计算，绘制走 X/Y 共用方法）
                            float w = g.MeasureString(_catLabels[i], ax.LabelStyleFont).Width;
                            float halfW = Math.Min(30f, w / 2f + 2f);
                            if (!strideHit) continue;
                            if (xPx - w / 2f < lastRight + charGap) continue;  // 与上一标签间隔不足一个文字 → 跳过
                            lastRight = xPx + w / 2f;
                            float cx = Math.Max(inner.Left + halfW, Math.Min(inner.Right - halfW, xPx));
                            DrawTickLabel(g, _catLabels[i], ax.LabelStyleFont, labelBrush,
                                new RectangleF(cx - 30, y + 5, 60, 17), StringAlignment.Center);
                        }
                    }
                }
                else
                {
                    bool timeAxis = IsTimeAxis(ax);
                    bool rot = _xRotate.TryGetValue(ax, out bool rotNum) && rotNum;
                    if (rot)
                    {
                        // 竖排路径（非实时）：标签转 90° 后 X 向只占字高，按"字高+一个文字间隔"重选更密间隔，
                        // 像素贪心再保一遍相邻条带至少一个文字空白；底部带宽已在 MeasureXLabelBand/XAxisBandHeight 预留
                        float lineH = ax.LabelStyleFont.GetHeight();
                        float charGapRot = ax.LabelStyleFont.Height;
                        int targetTicks = Math.Max(2, (int)(inner.Width / Math.Max(1f, lineH + charGapRot)));
                        double range = ax.ActualMaximum - ax.ActualMinimum;
                        ax.ActualInterval = timeAxis
                            ? ComputeTimeInterval(range, targetTicks)
                            : ComputeNiceInterval(range, targetTicks);
                        double startRot;
                        if (timeAxis)
                        {
                            double stepSec = ax.ActualInterval * 86400.0;
                            startRot = Math.Ceiling(ax.ActualMinimum * 86400.0 / stepSec) * stepSec / 86400.0;
                        }
                        else startRot = Math.Ceiling(ax.ActualMinimum / ax.ActualInterval) * ax.ActualInterval;
                        float lastStripRight = float.NegativeInfinity;
                        using (var gp = new Pen(ax.MajorGridColor) { DashStyle = ToDashStyle(ax.MajorGridDashStyle) })
                        {
                            for (double v = startRot; v <= ax.ActualMaximum + 1e-9; v += ax.ActualInterval)
                            {
                                float xPxRaw = MapX(v, ax, inner);
                                if (ax.MajorGridEnabled) g.DrawLine(gp, xPxRaw, inner.Top, xPxRaw, inner.Bottom);
                                if (ax.MajorTickMarkEnabled) g.DrawLine(Pens.Gray, xPxRaw, y, xPxRaw, y + 5);
                                // 条带水平居中于刻度、夹住不越 inner 左右边
                                float cx = Math.Max(inner.Left + lineH / 2f, Math.Min(inner.Right - lineH / 2f, xPxRaw));
                                // 相邻竖排条带间隔不足一个文字宽 → 只跳过文字（网格/刻度线照画）
                                if (xPxRaw - lineH / 2f < lastStripRight + charGapRot) continue;
                                lastStripRight = xPxRaw + lineH / 2f;
                                string lbl = FormatAxisTick(ax, v).Replace('\n', ' ');
                                DrawRotatedTick(g, lbl, ax.LabelStyleFont, labelBrush, cx, y + 5f);
                            }
                        }
                    }
                    else
                    {
                    // 密度按像素实测双向自适应：按当前步长量真实最宽标签，
                    // 挤了就在 1→2→2.5→5→10 阶梯上加大步长，明显稀疏（实际刻度不到容量一半）就减小加密，
                    // 不再固定"约 10 个刻度"只加不减——否则 0..25 永远停在步长 5，两个数字之间空 3、4 个数。
                    // 数值轴相邻数字之间留"3 个数字宽"的空白（按实测单个数字宽的 3 倍）；
                    // 时间轴两行标签按一个字高保留
                    float digitW = g.MeasureString("0", ax.LabelStyleFont).Width;
                    float charGapNum = timeAxis ? ax.LabelStyleFont.Height : digitW * 3f;
                    Func<double, string> tickTextAt = v =>
                    {
                        string lbl = FormatAxisTick(ax, v);
                        return lbl;
                    };
                    Func<double, double> tickStartOf = iv => timeAxis
                        ? Math.Ceiling(ax.ActualMinimum * 86400.0 / (iv * 86400.0)) * iv
                        : Math.Ceiling(ax.ActualMinimum / iv) * iv;
                    Func<double, double, float> tickLabelMaxW = (iv, stTick) =>
                    {
                        float w = 1f;
                        int k = 0;
                        for (double vv = stTick; vv <= ax.ActualMaximum + 1e-9 && k < 400; vv += iv, k++)
                            foreach (string line in tickTextAt(vv).Split('\n'))
                                w = Math.Max(w, g.MeasureString(line, ax.LabelStyleFont).Width);
                        return w;
                    };
                    double rangeX = ax.ActualMaximum - ax.ActualMinimum;
                    for (int guard = 0; guard < 8; guard++)
                    {
                        float widest = tickLabelMaxW(ax.ActualInterval, tickStartOf(ax.ActualInterval));
                        double fit = inner.Width / Math.Max(8f, widest + charGapNum); // 该宽度下一行放得下的标签数
                        double need = rangeX / ax.ActualInterval;                     // 当前步长实际要画的标签数
                        if (need > fit + 1.0)
                        {
                            // 拥挤：取能容纳 fit 个标签的"整齐步长"（≥ raw，保证下一轮量得下）
                            ax.ActualInterval = ComputeNiceInterval(rangeX, Math.Max(1, (int)fit));
                        }
                        else if (need < fit * 0.55)
                        {
                            // 稀疏：取加密一档；允许略超容量（1.25 倍内），交给下面的像素级贪心跳标签兜底防重叠
                            double smaller = NiceStepAtMost(rangeX / Math.Max(1.0, fit * 0.9));
                            if (smaller < ax.ActualInterval && rangeX / smaller <= fit * 1.25)
                                ax.ActualInterval = smaller;
                            else break;
                        }
                        else break;
                    }
                    // 时间轴刻度落在整秒边界（分钟/小时档自然对齐墙钟），数值轴沿用整间隔对齐
                    double start;
                    if (timeAxis)
                    {
                        double stepSec = ax.ActualInterval * 86400.0;
                        start = Math.Ceiling(ax.ActualMinimum * 86400.0 / stepSec) * stepSec / 86400.0;
                    }
                    else start = Math.Ceiling(ax.ActualMinimum / ax.ActualInterval) * ax.ActualInterval;
                    var tickX = new List<float>();
                    for (double v = start; v <= ax.ActualMaximum + 1e-9; v += ax.ActualInterval)
                        tickX.Add(MapX(v, ax, inner));
                    using (var gp = new Pen(ax.MajorGridColor) { DashStyle = ToDashStyle(ax.MajorGridDashStyle) })
                    {
                        foreach (float xPx in tickX)
                        {
                            if (ax.MajorGridEnabled) g.DrawLine(gp, xPx, inner.Top, xPx, inner.Bottom);
                            if (ax.MajorTickMarkEnabled) g.DrawLine(Pens.Gray, xPx, y, xPx, y + 4);
                        }
                    }
                    // 标签：数值轴走 DrawTickLabel（单行）；时间轴可多行（日期/时间各一行），
                    // 用 DrawString 居中绘制，矩形高度按行数给足，像素级贪心跳过保证互不重叠
                    float lineH = ax.LabelStyleFont.GetHeight();
                    float xLabelH = lineH * (timeAxis ? TimeLabelLines(ax) : 1) + 2f;
                    var sfCenter = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Near,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    float lastRight = float.NegativeInfinity;
                    for (int i = 0; i < tickX.Count; i++)
                    {
                        double v = start + i * ax.ActualInterval;
                        string label = tickTextAt(v);
                        float w = 1f;
                        foreach (string ln in label.Split('\n'))
                            w = Math.Max(w, g.MeasureString(ln, ax.LabelStyleFont).Width);
                        float half = w / 2f + 2f;
                        // 标签中心夹在 inner 左右边界内（不与 Y 轴线重合）
                        float cx = Math.Max(inner.Left + half, Math.Min(inner.Right - half, tickX[i]));
                        // 重叠判定按夹持后的"实际绘制位置"：首尾标签被夹进边界后会与相邻标签贴近，
                        // 此时跳过相邻的内侧标签，保证任意两个画出的标签之间都留足 charGapNum 空白
                        if (cx - w / 2f < lastRight + charGapNum) continue;
                        lastRight = cx + w / 2f;
                        var rect = new RectangleF(cx - w / 2f, y + 5f, w, xLabelH);
                        if (timeAxis)
                            g.DrawString(label, ax.LabelStyleFont, labelBrush, rect, sfCenter);
                        else
                            DrawTickLabel(g, label, ax.LabelStyleFont, labelBrush, rect, StringAlignment.Center);
                    }
                    }
                }
            }
            finally { labelBrush.Dispose(); }
            DrawAxisXTitle(g, ax, inner, atBottom);
        }
        /// <summary>
        /// 画一个顺时针转 90° 的 X 刻度标签：文字条带水平居中于刻度 xPx、从 yTop 向下延伸。
        /// 本地坐标系平移到 (xPx,yTop) 后旋转 90°：本地 +x 即屏幕向下，LineAlignment.Center 使条带横向居中。
        /// </summary>
        private static void DrawRotatedTick(Graphics g, string text, Font font, Brush brush, float xPx, float yTop)
        {
            var state = g.Save();
            g.TranslateTransform(xPx, yTop);
            g.RotateTransform(90f);
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, brush, 0f, 0f, sf);
            g.Restore(state);
        }
        /// <summary>绘制 X 轴标题：名称（名称字号）+ 单位（小 1px）水平排列、整体居中；太挤或越界则跳过。</summary>
        private void DrawAxisXTitle(Graphics g, HChartAxis ax, RectangleF inner, bool atBottom)
        {
            bool hasTitle = !string.IsNullOrEmpty(ax.Title);
            bool hasUnit = !string.IsNullOrEmpty(ax.Unit);
            if (!hasTitle && !hasUnit) return;
            using (var unitFont = UnitFont(ax.TitleFont))
            using (var titleBr = new SolidBrush(ax.TitleForeColor))
            {
                float nameW = hasTitle ? g.MeasureString(ax.Title, ax.TitleFont).Width : 0f;
                float unitW = hasUnit ? g.MeasureString(ax.Unit, unitFont).Width : 0f;
                float gap = (hasTitle && hasUnit) ? 5f : 0f;
                float groupW = nameW + gap + unitW;
                float nameH = ax.TitleFont.GetHeight();
                float groupH = Math.Max(nameH, hasUnit ? unitFont.GetHeight() : 0f);
                // 与 XAxisBandHeight 同源：标题顶偏移 = 横排数字行高+8；竖排=刻度线5+最长标签宽+4
                bool rot = _xRotate.TryGetValue(ax, out bool isRot) && isRot;
                float labelH = rot
                    ? 5f + _xRotBandW[ax] + 4f
                    : ax.LabelStyleFont.GetHeight() * (IsTimeAxis(ax) ? TimeLabelLines(ax) : 1) + 8f;
                float ty = atBottom ? inner.Bottom + labelH : inner.Top - labelH - groupH - 2f;
                // 越界/太挤 → 跳过（名称单位不重叠、不在控件外）
                if (ty < 0f || ty + groupH > ClientSize.Height) return;
                if (groupW > inner.Width * 0.95f) return;
                float cx = inner.Left + inner.Width / 2f;
                float x0 = cx - groupW / 2f;
                x0 = Math.Max(2f, Math.Min(x0, ClientSize.Width - groupW - 2f));
                if (x0 + groupW > ClientSize.Width - 2f) return;
                var sfNear = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                if (hasTitle) g.DrawString(ax.Title, ax.TitleFont, titleBr, x0, ty + groupH / 2f, sfNear);
                if (hasUnit)
                {
                    // 单位比名称小 1px，底部与名称基线略齐（向下偏 1px）
                    float ux = x0 + nameW + gap;
                    g.DrawString(ax.Unit, unitFont, titleBr, ux, ty + groupH / 2f + 1f, sfNear);
                }
            }
        }
        /// <summary>
        /// 绘制 Y 轴。yi=轴组索引（0=左1 主轴，1=右1，2=左2，3=右2……自内向外交替）；
        /// drawGrid=true 时画水平网格（仅主轴）。每根轴按实测带宽自内向外排列：
        /// 轴线 → 间隙(YTickGap) → 刻度数字(实测宽，完整不截短) → 间隙(YTitleGap，紧贴数字) → 竖排标题，标题必显。
        /// </summary>
        private void DrawAxisY(Graphics g, HChartArea ca, HChartAxis ay, RectangleF inner, int yi, bool drawGrid)
        {
            int side = YAxisSide(yi);
            // 轴线位置：侧向外按同侧各轴实测带宽累加偏移
            float x = side < 0 ? inner.Left - YAxisOffset(ca, yi) : inner.Right + YAxisOffset(ca, yi);
            using (var pen = new Pen(ay.LineColor, ay.LineWidth))
                g.DrawLine(pen, x, inner.Top, x, inner.Bottom);
            // 刻度数字列实测宽度（带宽据此预留，原文永远放得下，不缩字号不截短）
            float lbW = YLabelWidthOf(ay);
            var labelBrush = new SolidBrush(ay.LabelForeColor);
            try
            {
                if (ay.IsCategoryAxis && _barMode)
                {
                    // 横向条形图：类别在纵轴，标签居左、网格线水平（仅主轴）
                    int n = _catLabels.Count;
                    const float catHalfH = 9f;                 // 标签矩形半高（与 18f 同高同源）
                    // 相邻类别标签之间至少留一个文字的纵向空白（按字号字高计），绝不上下重叠
                    float charGapY = ay.LabelStyleFont.Height;
                    // 自屏幕顶向下贪心（类别索引越大越靠上，故 i 递减）：重叠判定用自然槽位 yPx，
                    // 边缘夹持只影响绘制位置，避免底边标签把上面一串全部误杀
                    float lastBottom = float.NegativeInfinity;
                    for (int ii = n - 1; ii >= 0; ii--)
                    {
                        float yPx = MapY(ii, ay, inner);
                        // 类别中心已在坐标区窗外（放大/平移后）：网格、刻度、文字一律不画，
                        // 不再让文字落到 X 轴线下方或越过坐标系顶线
                        if (yPx < inner.Top || yPx > inner.Bottom) continue;
                        if (yPx - catHalfH < lastBottom + charGapY) continue;  // 间隔不足一个文字 → 跳过，标签绝不重叠
                        lastBottom = yPx + catHalfH;
                        if (drawGrid && ay.MajorGridEnabled)
                            using (var gp = new Pen(ay.MajorGridColor) { DashStyle = ToDashStyle(ay.MajorGridDashStyle) })
                                g.DrawLine(gp, inner.Left, yPx, inner.Right, yPx);
                        if (ay.MajorTickMarkEnabled) g.DrawLine(Pens.Gray, x - 4, yPx, x, yPx);
                        // 文字中心纵向夹在坐标区内（边缘半个槽位的标签不越 inner 上下边）
                        float cy = Math.Max(inner.Top + catHalfH, Math.Min(inner.Bottom - catHalfH, yPx));
                        // 文字边缘距轴线 YTickGap，不与轴线重合；宽度取实测最宽类别名（绘制走 X/Y 共用方法）
                        float catRectX = side < 0 ? x - YTickGap - lbW : x + YTickGap;
                        DrawTickLabel(g, _catLabels[ii], ay.LabelStyleFont, labelBrush,
                            new RectangleF(catRectX, cy - catHalfH, lbW, catHalfH * 2f),
                            side < 0 ? StringAlignment.Far : StringAlignment.Near);
                    }
                }
                else
                {
                    // 密度粗调：防止缩放后 Y 轴刻度标签上下重叠（相邻刻度间距不足一个文字高+一个文字间隔就翻倍）
                    float labelH = ay.LabelStyleFont.GetHeight();
                    float charGapYNum = ay.LabelStyleFont.Height;
                    float ticks = (float)((ay.ActualMaximum - ay.ActualMinimum) / ay.ActualInterval);
                    while (ticks > 0 && inner.Height / ticks < labelH + charGapYNum)
                    {
                        ay.ActualInterval *= 2.0;
                        ticks = (float)((ay.ActualMaximum - ay.ActualMinimum) / ay.ActualInterval);
                    }
                    double start = Math.Ceiling(ay.ActualMinimum / ay.ActualInterval) * ay.ActualInterval;
                    // 先收集全部刻度（网格/刻度线对所有刻度画）
                    var tickY = new List<(float py, double val)>();
                    for (double v = start; v <= ay.ActualMaximum + 1e-9; v += ay.ActualInterval)
                        tickY.Add((MapY(v, ay, inner), v));
                    using (var gp = new Pen(ay.MajorGridColor) { DashStyle = ToDashStyle(ay.MajorGridDashStyle) })
                    {
                        foreach (var t in tickY)
                        {
                            if (drawGrid && ay.MajorGridEnabled) g.DrawLine(gp, inner.Left, t.py, inner.Right, t.py);
                            // 刻度：向外侧伸出
                            if (ay.MajorTickMarkEnabled)
                                g.DrawLine(Pens.Gray,
                                    side < 0 ? x - 4 : x, t.py,
                                    side < 0 ? x : x + 4, t.py);
                        }
                    }
                    // 标签像素级贪心跳过：自顶向下，与上一个已画标签底边缘间隔不足一个文字高就跳过
                    float labelH2 = ay.LabelStyleFont.GetHeight();
                    float halfH = labelH2 / 2f + 1f;   // 标签矩形半高
                    // 标签列：距轴线 YTickGap，宽度为实测最宽数字（带宽已据此预留，原文完整不截短）
                    float lbRectX = side < 0 ? x - YTickGap - lbW : x + YTickGap;
                    var lbAlign = side < 0 ? StringAlignment.Far : StringAlignment.Near;
                    // 按屏幕 y 自顶向下排序
                    tickY.Sort((a, b) => a.py.CompareTo(b.py));
                    float lastBottom = float.NegativeInfinity;
                    foreach (var t in tickY)
                    {
                        // 重叠判定用自然刻度位置 t.py；夹持只改绘制位置，避免边缘标签被夹后连带跳过后续标签
                        // 与上一标签空白不足一个文字高 → 跳过（Y 刻度文字永不相邻重叠）
                        if (t.py - halfH < lastBottom + charGapYNum) continue;
                        lastBottom = t.py + halfH;
                        // 文字中心纵向夹在坐标区内：边缘刻度的标签不越过 X 轴线/坐标系顶线（与 X 轴横向夹持同口径）
                        float cy = Math.Max(inner.Top + halfH, Math.Min(inner.Bottom - halfH, t.py));
                        // Y 刻度与 X 完全统一：同 FormatTick 格式（小数位取该轴 TickDecimalPlaces）、同字号、矩形宽=实测宽，不降级不截短
                        DrawTickLabel(g, FormatTick(t.val, ay.TickDecimalPlaces), ay.LabelStyleFont, labelBrush,
                            new RectangleF(lbRectX, cy - halfH, lbW, labelH2 + 2f), lbAlign);
                    }
                }
            }
            finally { labelBrush.Dispose(); }
            DrawAxisYTitle(g, ay, inner, yi, x);
        }
        /// <summary>
        /// 绘制 Y 轴标题（竖排）：名称（名称字号）+ 单位（小 1px）沿轴方向排列；
        /// 位于刻度数字外侧且紧贴数字列（轴线→YTickGap→实测数字列宽→YTitleGap(2px)→标题），位置随轴组外移。
        /// 水平厚度已在 MeasureYAxisBands 中计入带宽（坐标系预先压缩），标题/单位始终完整绘制，不做隐藏跳过；
        /// 沿轴方向过长时由窗口自然裁剪，也不隐藏。
        /// </summary>
        private void DrawAxisYTitle(Graphics g, HChartAxis ay, RectangleF inner, int yi, float axisX)
        {
            bool hasTitle = !string.IsNullOrEmpty(ay.Title);
            bool hasUnit = !string.IsNullOrEmpty(ay.Unit);
            if (!hasTitle && !hasUnit) return;
            int side = YAxisSide(yi);
            using (var unitFont = UnitFont(ay.TitleFont))
            {
                float nameW = hasTitle ? g.MeasureString(ay.Title, ay.TitleFont).Width : 0f;
                float unitW = hasUnit ? g.MeasureString(ay.Unit, unitFont).Width : 0f;
                float gap = (hasTitle && hasUnit) ? 5f : 0f;
                float groupLen = nameW + gap + unitW;          // 旋转后沿轴方向的长度
                float thick = Math.Max(ay.TitleFont.GetHeight(), hasUnit ? unitFont.GetHeight() : 0f); // 旋转后水平厚度
                // 标题中心 X：轴线 → YTickGap 间隙 → 刻度数字列(实测宽) → YTitleGap 间隙（紧贴数字）→ 标题厚度一半
                float lbW = YLabelWidthOf(ay);
                float tx = side < 0
                    ? axisX - YTickGap - lbW - YTitleGap - thick / 2f
                    : axisX + YTickGap + lbW + YTitleGap + thick / 2f;
                float ty = inner.Top + inner.Height / 2f;
                using (var titleBr = new SolidBrush(ay.TitleForeColor))
                {
                    var gs = g.Save();
                    g.TranslateTransform(tx, ty);
                    g.RotateTransform(-90);
                    // 旋转后局部 X 轴沿图表纵轴方向：名称在前、单位在后，整体居中
                    var sfNear = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                    if (hasTitle) g.DrawString(ay.Title, ay.TitleFont, titleBr, -groupLen / 2f, 0f, sfNear);
                    if (hasUnit) g.DrawString(ay.Unit, unitFont, titleBr, -groupLen / 2f + nameW + gap, 1f, sfNear);
                    g.Restore(gs);
                }
            }
        }
        // ============ 坐标映射（统一委托 HScreen；附加 Y 轴走 _yScreens/_screenByAxis）============
        /// <summary>
        /// 世界 X 值 → 屏幕像素 X（所有图表区共用主映射器 _screen，量程由 ComputeAxis/SetupScreen 配置）。
        /// </summary>
        private float MapX(double v, HChartAxis ax, RectangleF inner)
            => (float)_screen.WorldToScreen(new HPoint(v, 0)).X.Value;
        /// <summary>
        /// 世界 Y 值 → 屏幕像素 Y。按系列绑定的 Y 轴经 ScreenFor 选择对应映射器
        /// （各根 Y 轴各自独立量程，SetupScreens 时配置）。
        /// </summary>
        private float MapY(double v, HChartAxis ay, RectangleF inner)
        {
            // 按系列绑定的 Y 轴选择对应映射器（各根 Y 轴各自独立量程）
            return (float)ScreenFor(ay).WorldToScreen(new HPoint(0, v)).Y.Value;
        }
        // ============ 渲染 ============
        /// <summary>类别轴每个槽位的像素宽度（纵向图表用）；数值轴退化为 0.9 倍缩放宽度。</summary>
        private float SlotWidthX(HChartAxis ax, RectangleF inner)
        {
            if (ax.IsCategoryAxis) return inner.Width / Math.Max(1, _catCount) * 0.9f;
            double range = ax.ActualMaximum - ax.ActualMinimum; if (range <= 0) range = 1;
            return (float)(inner.Width / range * 0.9);
        }
        /// <summary>
        /// 屏幕点安全化：clamp 到 inner 外扩 margin 边界 + 屏幕空间抽稀。
        /// 深缩放两端都会让 GDI+ DrawCurve（Cardinal 样条）崩溃，必须双管齐下：
        ///   1) 放大到最深：可视区外远点屏幕坐标达几十万像素，样条张力求解算术溢出（OverflowException）
        ///      → clamp 到安全边界，远点本就被 clip 裁掉，视觉无损；
        ///   2) 缩小到最浅：scale 极小，大量数据点挤成几像素内的近重合点，样条对极度密集控制点
        ///      细分出爆炸数量微线段，内存分配失败（OutOfMemoryException）
        ///      → 相邻点屏幕距离 &lt; minDist 即合并（缩小时&lt;1.5px 人眼不可分辨，无损；放大时点稀疏全保留）。
        /// 调用前须保证点坐标都是有限数（NaN/Inf 先自行剔除）。
        /// </summary>
        private static PointF[] ClampScreenPoints(IList<PointF> pts, RectangleF inner, float margin = 4096f, float minDist = 1.5f)
        {
            float minX = inner.Left - margin, maxX = inner.Right + margin;
            float minY = inner.Top - margin, maxY = inner.Bottom + margin;
            float md2 = minDist * minDist;
            var kept = new List<PointF>(pts.Count);
            float lx = 0f, ly = 0f;
            bool has = false;
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                float cx = Math.Max(minX, Math.Min(maxX, p.X));
                float cy = Math.Max(minY, Math.Min(maxY, p.Y));
                if (!has)
                {
                    kept.Add(new PointF(cx, cy)); lx = cx; ly = cy; has = true;
                }
                else
                {
                    float dx = cx - lx, dy = cy - ly;
                    if (dx * dx + dy * dy >= md2) { kept.Add(new PointF(cx, cy)); lx = cx; ly = cy; }
                    // 否则与上一保留点屏幕距离 < minDist：重合/近重合点，合并跳过
                }
            }
            // 末端点务必保留（保证曲线/多边形闭合到最后一个数据点）
            if (has && pts.Count > 0)
            {
                var ep = pts[pts.Count - 1];
                float ex = Math.Max(minX, Math.Min(maxX, ep.X));
                float ey = Math.Max(minY, Math.Min(maxY, ep.Y));
                if (Math.Abs(ex - lx) > 0.01f || Math.Abs(ey - ly) > 0.01f) kept.Add(new PointF(ex, ey));
            }
            return kept.ToArray();
        }
        /// <summary>
        /// 视区裁剪（参考 HCoordinate：不在坐标系里的图形不显示、不参与运算）。
        /// 连线只需保留：进入坐标系前的最后一个外部点（前一个点）、坐标系内的全部点、
        /// 离开坐标系后的第一个外部点（后一个点），其余视区外点一律丢弃；
        /// 超出部分的线段由绘图区 clip 裁到 inner 边缘，线照样画满到坐标区边界。
        /// 兜底：两端都在区外、但连线真正穿过视区的斜掠线段仍保留其端点，避免丢线。
        /// </summary>
        private static List<PointF> CullToView(IList<PointF> raw, RectangleF inner)
        {
            const float margin = 1f;
            float x0 = inner.Left - margin, x1 = inner.Right + margin;
            float y0 = inner.Top - margin, y1 = inner.Bottom + margin;
            var kept = new List<PointF>(raw.Count);
            for (int i = 0; i < raw.Count; i++)
            {
                var p = raw[i];
                bool inBox = p.X >= x0 && p.X <= x1 && p.Y >= y0 && p.Y <= y1;
                if (inBox) { kept.Add(p); continue; }
                bool prevIn = i > 0 && InBox(raw[i - 1], x0, x1, y0, y1);
                bool nextIn = i < raw.Count - 1 && InBox(raw[i + 1], x0, x1, y0, y1);
                if (prevIn || nextIn) { kept.Add(p); continue; } // 紧邻边界的"前一个/后一个"外部点
                if ((i > 0 && SegmentMayCross(raw[i - 1], p, x0, x1, y0, y1)) ||
                    (i < raw.Count - 1 && SegmentMayCross(p, raw[i + 1], x0, x1, y0, y1)))
                    kept.Add(p); // 斜穿视区的兜底
            }
            return kept;
        }
        private static bool InBox(PointF p, float x0, float x1, float y0, float y1)
            => p.X >= x0 && p.X <= x1 && p.Y >= y0 && p.Y <= y1;
        /// <summary>
        /// 线段 a-b 是否与矩形框相交：先判端点是否在框内（含边界，在内必相交）；
        /// 两端都在框外时再用 Liang-Barsky 参数化裁剪精确判定——线段在框内的参数区间
        /// 与 [0,1] 有重叠即真正穿过框（替代旧的“非同侧即可能相交”粗略判定，角外斜掠线段不再误判）。
        /// </summary>
        private static bool SegmentMayCross(PointF a, PointF b, float x0, float x1, float y0, float y1)
        {
            // 1. 任一端点在框内（含边界）→ 必相交
            if ((a.X >= x0 && a.X <= x1 && a.Y >= y0 && a.Y <= y1) ||
                (b.X >= x0 && b.X <= x1 && b.Y >= y0 && b.Y <= y1)) return true;
            // 2. Liang-Barsky：p<0 为进入边、p>0 为离开边，t0=最晚进入参数、t1=最早离开参数；
            //    平行于边界(p=0)时 q<0 表示整段在框外；进入晚于离开(t0>t1)即不相交
            float t0 = 0f, t1 = 1f, dx = b.X - a.X, dy = b.Y - a.Y;
            bool Clip(float p, float q)
            {
                if (p == 0f) return q >= 0f;
                float t = q / p;
                if (p < 0f) { if (t > t1) return false; t0 = Math.Max(t0, t); } // 进入边
                else { if (t < t0) return false; t1 = Math.Min(t1, t); }        // 离开边
                return true;
            }
            return Clip(-dx, a.X - x0) && Clip(dx, x1 - a.X)
                && Clip(-dy, a.Y - y0) && Clip(dy, y1 - a.Y);
        }
        /// <summary>
        /// 绘制折线类系列（Line/Spline/StepLine/Area 共用）：
        /// 数据点映射到屏幕 → 剔除 NaN/Inf → 整体系列挤成一个点（&lt;1.5px）直接跳过 →
        /// 视区裁剪 CullToView → clamp 安全边界 + 抽稀 ClampScreenPoints →
        /// Spline 用 DrawCurve 平滑、StepLine 用阶梯线、其余 DrawLines 折线；Area 额外向 0 基线填充半透明区域；
        /// 最后只在 inner 外扩 64px 范围内绘制数据点标记（marker）。
        /// </summary>
        private void DrawLineSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color)
        {
            // 原始屏幕点：先剔除 NaN/Inf（映射器理论已防，双保险）
            var raw = new List<PointF>();
            float rMinX = float.MaxValue, rMaxX = float.MinValue, rMinY = float.MaxValue, rMaxY = float.MinValue;
            // 点数超过像素列 2 倍（如 10 万点实时曲线）→ 按像素列抽稀：
            // 每列保留首点 + 最高/最低点（按实际出现先后），绘制量与像素列数成正比且波形包络不失真
            bool dense = s.Points.Count > inner.Width * 2f;
            if (dense)
            {
                int cols = (int)inner.Width + 3;
                var first = new PointF[cols];
                var loPt = new PointF[cols];
                var hiPt = new PointF[cols];
                var used = new bool[cols];
                var phase = new byte[cols];   // bit1=最高点已离开首点, bit2=最低点已离开首点
                var hiFirst = new bool[cols]; // 该列先出现的是最高点（决定高/低输出先后，避免来回打结）
                int cMin = cols, cMax = -1;
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    float px = MapX(p.XValue, ax, inner);
                    float py = MapY(p.YValue, ay, inner);
                    if (!IsFinite(px) || !IsFinite(py)) continue;
                    if (px < rMinX) rMinX = px; if (px > rMaxX) rMaxX = px;
                    if (py < rMinY) rMinY = py; if (py > rMaxY) rMaxY = py;
                    int c = (int)(px - inner.Left) + 1;
                    if (c < 0) c = 0; else if (c >= cols) c = cols - 1;
                    if (!used[c])
                    {
                        used[c] = true; first[c] = loPt[c] = hiPt[c] = new PointF(px, py);
                        if (c < cMin) cMin = c; if (c > cMax) cMax = c;
                    }
                    else if (py > hiPt[c].Y)
                    {
                        if (phase[c] == 0) hiFirst[c] = true;
                        phase[c] |= 1; hiPt[c] = new PointF(px, py);
                    }
                    else if (py < loPt[c].Y)
                    {
                        if (phase[c] == 0) hiFirst[c] = false;
                        phase[c] |= 2; loPt[c] = new PointF(px, py);
                    }
                }
                for (int c = cMin; c <= cMax; c++)
                    if (used[c])
                    {
                        raw.Add(first[c]);
                        if (phase[c] != 0)
                        {
                            if (hiFirst[c]) { if ((phase[c] & 1) != 0) raw.Add(hiPt[c]); if ((phase[c] & 2) != 0) raw.Add(loPt[c]); }
                            else { if ((phase[c] & 2) != 0) raw.Add(loPt[c]); if ((phase[c] & 1) != 0) raw.Add(hiPt[c]); }
                        }
                    }
            }
            else
            {
                foreach (var p in s.Points)
                    if (!p.IsEmpty)
                    {
                        float px = MapX(p.XValue, ax, inner);
                        float py = MapY(p.YValue, ay, inner);
                        if (IsFinite(px) && IsFinite(py))
                        {
                            raw.Add(new PointF(px, py));
                            if (px < rMinX) rMinX = px; if (px > rMaxX) rMaxX = px;
                            if (py < rMinY) rMinY = py; if (py > rMaxY) rMaxY = py;
                        }
                    }
            }
            if (raw.Count == 0) return;
            // 图形在屏幕上小到挤成一个点（缩到最浅时全部数据压缩在中心几像素内）→ 不绘制、不做无谓运算
            if (rMaxX - rMinX < 1.5f && rMaxY - rMinY < 1.5f) return;
            // 视区裁剪：坐标系外的点直接丢弃（参考 HCoordinate），再 clamp 安全边界 + 屏幕抽稀
            var culled = CullToView(raw, inner);
            if (culled.Count == 0) return;
            var valid = ClampScreenPoints(culled, inner);
            using (var pen = new Pen(color, s.BorderWidth) { DashStyle = ToDashStyle(s.BorderDashStyle) })
            {
                if (valid.Length >= 2)
                {
                    if (s.ChartType == HChartType.Spline) g.DrawCurve(pen, valid);
                    else if (s.ChartType == HChartType.StepLine) DrawStepLine(g, pen, valid);
                    else g.DrawLines(pen, valid);
                    // 面积填充：向下填充到 0 值基线（MS Chart 行为）
                    if (s.ChartType == HChartType.Area)
                    {
                        float baseY = MapY(0, ay, inner);
                        if (!IsFinite(baseY)) baseY = inner.Bottom;
                        baseY = Math.Max(inner.Top - 4096f, Math.Min(inner.Bottom + 4096f, baseY));
                        using (var br = new SolidBrush(Color.FromArgb(s.BackColorAlpha, s.BackColor ?? color)))
                        {
                            var path = new List<PointF>(valid);
                            path.Insert(0, new PointF(valid[0].X, baseY));
                            path.Add(new PointF(valid[valid.Length - 1].X, baseY));
                            g.FillPolygon(br, path.ToArray());
                        }
                    }
                }
                // marker 用原始坐标：只画可视区附近（外扩 64px）的点，视区外一律不画。
                // 非密点（含实时流动窗口）逐点标注；密点（显示全部历史/10 万点）时 raw 已是像素列
                // 抽稀点，再按屏幕间距过滤重叠点，避免标记糊成团
                if (s.MarkerStyle != HChartMarkerStyle.None)
                {
                    float m = 64f;
                    float mmX = inner.Left - m, mmXX = inner.Right + m, mmY = inner.Top - m, mmYY = inner.Bottom + m;
                    float minGap2 = 0f, lastX = 0f, lastY = 0f;
                    bool hasLast = false;
                    if (dense) { float minGap = s.MarkerSize * 0.85f; minGap2 = minGap * minGap; }
                    foreach (var pt in raw)
                    {
                        if (pt.X < mmX || pt.X > mmXX || pt.Y < mmY || pt.Y > mmYY) continue;
                        if (dense && hasLast)
                        {
                            float ddx = pt.X - lastX, ddy = pt.Y - lastY;
                            if (ddx * ddx + ddy * ddy < minGap2) continue;
                        }
                        DrawMarker(g, pt, s.MarkerStyle, s.MarkerSize, color);
                        lastX = pt.X; lastY = pt.Y; hasLast = true;
                    }
                }
            }
        }
        /// <summary>堆叠面积图：按 PrepareStacks 计算的累计上/下包络填充与描边。</summary>
        private void DrawStackedAreaSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color)
        {
            if (!_areaStack.Active || !_areaStack.Upper.ContainsKey(s)) return;
            double[] xs = _areaStack.Xs;
            double[] lower = _areaStack.Lower[s];
            double[] upper = _areaStack.Upper[s];
            if (xs.Length < 2) return;
            var upperRaw = new List<PointF>(xs.Length);
            var lowerRaw = new List<PointF>(xs.Length);
            for (int i = 0; i < xs.Length; i++)
            {
                float ux = MapX(xs[i], ax, inner), uy = MapY(upper[i], ay, inner);
                float lx = MapX(xs[i], ax, inner), ly = MapY(lower[i], ay, inner);
                // 上下包络同生共死：任一坐标非有限则该索引整体跳过，保证多边形顶点对齐
                if (IsFinite(ux) && IsFinite(uy) && IsFinite(lx) && IsFinite(ly))
                {
                    upperRaw.Add(new PointF(ux, uy));
                    lowerRaw.Add(new PointF(lx, ly));
                }
            }
            if (upperRaw.Count < 2) return;
            // 视区裁剪（上下包络同步）：索引可见 ⇔ 上/下包络点在扩展视区内，或与邻段连线可能穿越视区
            float vx0 = inner.Left - inner.Width * 0.5f, vx1 = inner.Right + inner.Width * 0.5f;
            float vy0 = inner.Top - inner.Height * 0.5f, vy1 = inner.Bottom + inner.Height * 0.5f;
            bool InExpanded(PointF q) => q.X >= vx0 && q.X <= vx1 && q.Y >= vy0 && q.Y <= vy1;
            int n = upperRaw.Count;
            var upperVis = new List<PointF>(n);
            var lowerVis = new List<PointF>(n);
            for (int i = 0; i < n; i++)
            {
                bool vis = InExpanded(upperRaw[i]) || InExpanded(lowerRaw[i]);
                if (!vis)
                {
                    if (i > 0 && (SegmentMayCross(upperRaw[i - 1], upperRaw[i], vx0, vx1, vy0, vy1)
                               || SegmentMayCross(lowerRaw[i - 1], lowerRaw[i], vx0, vx1, vy0, vy1))) vis = true;
                    else if (i < n - 1 && (SegmentMayCross(upperRaw[i], upperRaw[i + 1], vx0, vx1, vy0, vy1)
                                        || SegmentMayCross(lowerRaw[i], lowerRaw[i + 1], vx0, vx1, vy0, vy1))) vis = true;
                }
                if (vis) { upperVis.Add(upperRaw[i]); lowerVis.Add(lowerRaw[i]); }
            }
            if (upperVis.Count < 2) return;
            // clamp 安全边界 + 抽稀（上下包络同步，保证多边形顶点对齐）
            var upperPts = ClampScreenPoints(upperVis, inner);
            var lowerPts = ClampScreenPoints(lowerVis, inner);
            using (var br = new SolidBrush(Color.FromArgb(s.BackColorAlpha, s.BackColor ?? color)))
            {
                var path = new List<PointF>(upperPts);
                for (int i = lowerPts.Length - 1; i >= 0; i--)
                    path.Add(lowerPts[i]);
                g.FillPolygon(br, path.ToArray());
            }
            using (var pen = new Pen(color, s.BorderWidth))
                g.DrawLines(pen, upperPts);
        }
        /// <summary>绘制阶梯线：相邻两点之间先水平走到下一点的 X、再垂直走到下一点的 Y（阶跃形状）。</summary>
        private void DrawStepLine(Graphics g, Pen pen, PointF[] pts)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                g.DrawLine(pen, pts[i].X, pts[i].Y, pts[i + 1].X, pts[i].Y);
                g.DrawLine(pen, pts[i + 1].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
            }
        }
        /// <summary>
        /// 在指定屏幕点绘制一个数据点标记：实心形状（圆/方/菱/三角/五角星/五边形/六边形）用画刷填充，
        /// 十字（Cross）用画笔描线；形状由 HChartMarkerStyle 决定，size 为外接正方形边长（像素）。
        /// </summary>
        private void DrawMarker(Graphics g, PointF pt, HChartMarkerStyle style, int size, Color color)
        {
            float r = size / 2f;
            using (var br = new SolidBrush(color))
            using (var pen = new Pen(color, size / 8f + 0.5f))
            {
                switch (style)
                {
                    case HChartMarkerStyle.Circle:
                        g.FillEllipse(br, pt.X - r, pt.Y - r, size, size); break;
                    case HChartMarkerStyle.Square:
                        g.FillRectangle(br, pt.X - r, pt.Y - r, size, size); break;
                    case HChartMarkerStyle.Diamond:
                        g.FillPolygon(br, new[] { new PointF(pt.X, pt.Y - r), new PointF(pt.X + r, pt.Y), new PointF(pt.X, pt.Y + r), new PointF(pt.X - r, pt.Y) }); break;
                    case HChartMarkerStyle.Triangle:
                        g.FillPolygon(br, new[] { new PointF(pt.X, pt.Y - r), new PointF(pt.X + r, pt.Y + r), new PointF(pt.X - r, pt.Y + r) }); break;
                    case HChartMarkerStyle.Star:
                        DrawStar(g, pen, br, pt, r); break;
                    case HChartMarkerStyle.Pentagon:
                        DrawPolygonN(g, br, pt, r, 5, -Math.PI / 2); break;
                    case HChartMarkerStyle.Hexagon:
                        DrawPolygonN(g, br, pt, r, 6, 0); break;
                    case HChartMarkerStyle.Cross:
                        g.DrawLine(pen, pt.X - r, pt.Y, pt.X + r, pt.Y);
                        g.DrawLine(pen, pt.X, pt.Y - r, pt.X, pt.Y + r); break;
                    default:
                        g.FillEllipse(br, pt.X - r, pt.Y - r, size, size); break;
                }
            }
        }
        /// <summary>绘制中心 c、外接半径 r 的正 n 边形（五边形/六边形标记用），startAngle 为第一个顶点的起始弧度。</summary>
        private static void DrawPolygonN(Graphics g, Brush br, PointF c, float r, int n, double startAngle)
        {
            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                double a = startAngle + 2 * Math.PI * i / n;
                pts[i] = new PointF((float)(c.X + r * Math.Cos(a)), (float)(c.Y + r * Math.Sin(a)));
            }
            g.FillPolygon(br, pts);
        }
        /// <summary>绘制五角星标记：10 个顶点交替使用外半径 r 与内半径 0.45r，从正上方（-90°）开始均布。</summary>
        private static void DrawStar(Graphics g, Pen pen, Brush br, PointF c, float r)
        {
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double rad = (i % 2 == 0) ? r : r * 0.45;
                double a = -Math.PI / 2 + Math.PI * i / 5;
                pts[i] = new PointF((float)(c.X + rad * Math.Cos(a)), (float)(c.Y + rad * Math.Sin(a)));
            }
            g.FillPolygon(br, pts);
        }
        // 数值标签：文字 + 短指引线 + 格式化（给 Column/StackedColumn/Bar 共用）
        /// <summary>
        /// 格式化柱/条上的数值标签（Column/StackedColumn/Bar 共用）：
        /// text 与轴刻度统一走 FormatTick（×10ⁿ 上标 / 最多 5 位小数去尾零）；
        /// integerOnly=true 表示该值适合用整数样式呈现（≥10000 或与整数差 &lt;1e-6），调用方可据此简化字号/布局判断。
        /// </summary>
        private static void fmtLabel(double v, out string text, out bool integerOnly)
        {
            double a = Math.Abs(v);
            integerOnly = a >= 10000 || Math.Abs(v - Math.Round(v)) < 1e-6;
            text = FormatTick(v);   // 与刻度统一格式（×10ⁿ / 5 位小数）
        }
        /// <summary>Column：并排柱顶端上方紧贴数字（正中央对齐柱体），正常不带指引线；
        /// 只有文字放不下（会出绘图区边界）才贴边并加短指引线。字号随柱高自适应，最大 20f。</summary>
        private void DrawColumnSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color, int groupIndex, int groupCount)
        {
            float slotWidth = SlotWidthX(ax, inner);
            float barWidth = groupCount > 1 ? slotWidth / groupCount * 0.85f : slotWidth * 0.7f;
            float groupOffset = groupCount > 1 ? -slotWidth / 2f + barWidth * groupIndex + barWidth / 2f : 0;
            using (var br = new SolidBrush(Color.FromArgb(s.BackColorAlpha, s.BackColor ?? color)))
            using (var pen = new Pen(color, 1))
            using (var leaderPen = new Pen(Color.FromArgb(160, 120, 120, 120), 1))
            using (var labelBr = new SolidBrush(Color.FromArgb(20, 20, 20)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                float maxFontSize = 20f;
                float baseFontSize = 7f;
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    float xPx = MapX(p.XValue, ax, inner) + groupOffset;
                    // 视区裁剪：柱体完全在坐标系外（外扩一个槽位）则不绘制、不做标签运算
                    if (xPx + barWidth / 2f < inner.Left - slotWidth || xPx - barWidth / 2f > inner.Right + slotWidth) continue;
                    float top = MapY(Math.Max(p.YValue, 0), ay, inner);
                    float bottom = MapY(Math.Min(p.YValue, 0), ay, inner);
                    float h = bottom - top; if (h < 1) h = 1;
                    g.FillRectangle(br, xPx - barWidth / 2f, top, barWidth, h);
                    g.DrawRectangle(pen, xPx - barWidth / 2f, top, barWidth, h);
                    string label; fmtLabel(p.YValue, out label, out _);
                    // 字号 = Min(柱宽, 柱高) / 3（Font.Height≈pt*1.2 → pt≈rect/3.6），封顶 20f
                    float fontPx = Math.Min(maxFontSize, Math.Max(baseFontSize, Math.Min(barWidth, h) / 3.6f));
                    using (var font = new Font(ax.LabelStyleFont.FontFamily, fontPx, FontStyle.Regular))
                    {
                        float textH = font.Height;
                        float gap = 1.5f;
                        // 正常位置 = 贴柱端上方 gap（正中央对齐柱）
                        float idealCenter;
                        if (p.YValue >= 0) idealCenter = top - gap - textH / 2f;
                        else                idealCenter = bottom + gap + textH / 2f;
                        float minCenter = inner.Top + textH / 2f;
                        float maxCenter = inner.Bottom - textH / 2f;
                        bool needLeader = idealCenter < minCenter || idealCenter > maxCenter;
                        float tagCenter = Math.Max(minCenter, Math.Min(maxCenter, idealCenter));
                        // 只有放不下时才画指引线
                        if (needLeader)
                        {
                            if (p.YValue >= 0)
                            {
                                float botY = tagCenter + textH / 2f;
                                if (botY < top - 0.5f) g.DrawLine(leaderPen, xPx, botY, xPx, top);
                            }
                            else
                            {
                                float topY = tagCenter - textH / 2f;
                                if (topY > bottom + 0.5f) g.DrawLine(leaderPen, xPx, topY, xPx, bottom);
                            }
                        }
                        var sz = g.MeasureString(label, font);
                        g.DrawString(label, font, labelBr,
                            new RectangleF(xPx - sz.Width / 2f, tagCenter - textH / 2f, sz.Width, textH), sf);
                    }
                }
            }
        }
        /// <summary>StackedColumn：每段（每个产品）内部居中显示贡献值。字号随段高自适应（上限封顶），段太小则跳过。</summary>
        private void DrawStackedColumnSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color)
        {
            if (!_colStack.Active || !_colStack.Upper.ContainsKey(s)) return;
            double[] xs = _colStack.Xs;
            double[] lower = _colStack.Lower[s];
            double[] upper = _colStack.Upper[s];
            float slotWidth = SlotWidthX(ax, inner);
            float barWidth = slotWidth * 0.7f;
            using (var br = new SolidBrush(Color.FromArgb(s.BackColorAlpha, s.BackColor ?? color)))
            using (var pen = new Pen(color, 1))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                float minH = 12f;              // 段高 < 12px 不画文字
                float maxFontSize = 20f;       // 字号上限（段足够大时）
                float baseFontSize = 7f;       // 最小基础字号
                for (int i = 0; i < xs.Length; i++)
                {
                    float xPx = MapX(xs[i], ax, inner);
                    float top = MapY(upper[i], ay, inner);
                    float bottom = MapY(lower[i], ay, inner);
                    float h = bottom - top; if (h < 0.5f) continue;
                    g.FillRectangle(br, xPx - barWidth / 2f, top, barWidth, h);
                    g.DrawRectangle(pen, xPx - barWidth / 2f, top, barWidth, h);
                    // 段太矮（< minH）跳过，防止文字放不下还挤在里面
                    if (h < minH) continue;
                    // 字号 = Min(段宽, 段高) / 3（Font.Height ≈ pt*1.2），封顶 20f
                    float fontPx = Math.Min(maxFontSize, Math.Max(baseFontSize, Math.Min(barWidth, h) / 3.6f));
                    using (var font = new Font(ax.LabelStyleFont.FontFamily, fontPx, FontStyle.Regular))
                    using (var labelBr = new SolidBrush(Color.FromArgb(255, 255, 255, 255))) // 白色字在彩色段上醒目
                    {
                        double seg = upper[i] - lower[i];
                        string label; fmtLabel(seg, out label, out _);
                        // 文字太长放不下段宽就跳过
                        var sz = g.MeasureString(label, font);
                        if (sz.Width + 2f > barWidth) continue;
                        g.DrawString(label, font, labelBr,
                            new RectangleF(xPx - sz.Width / 2f, top + (h - sz.Height) / 2f, sz.Width, sz.Height), sf);
                    }
                }
            }
        }
        /// <summary>Bar：横向条末端显示数值。字号随条高自适应（最大 20f），条太矮跳过；
        /// 正常贴条端外侧、不带指引线，只有放不下时才贴边并加短指引线。</summary>
        private void DrawBarSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color, int groupIndex, int groupCount)
        {
            float slotHeight = inner.Height / Math.Max(1, _catCount) * 0.9f;
            float barHeight = groupCount > 1 ? slotHeight / groupCount * 0.85f : slotHeight * 0.7f;
            float groupOffset = groupCount > 1 ? -slotHeight / 2f + barHeight * groupIndex + barHeight / 2f : 0;
            float xZero = MapX(0, ax, inner);
            using (var br = new SolidBrush(Color.FromArgb(s.BackColorAlpha, s.BackColor ?? color)))
            using (var pen = new Pen(color, 1))
            using (var leaderPen = new Pen(Color.FromArgb(160, 120, 120, 120), 1))
            using (var labelBr = new SolidBrush(Color.FromArgb(20, 20, 20)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                float maxFontSize = 20f;
                float baseFontSize = 7f;
                float minH = 12f;
                float gap = 1.5f;
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    float yPx = MapY(p.XValue, ay, inner) + groupOffset;
                    // 视区裁剪：条完全在坐标系外（外扩一个槽高）则不绘制、不做标签运算
                    if (yPx + barHeight / 2f < inner.Top - slotHeight || yPx - barHeight / 2f > inner.Bottom + slotHeight) continue;
                    float xVal = MapX(p.YValue, ax, inner);
                    float left = Math.Min(xZero, xVal);
                    float w = Math.Abs(xVal - xZero); if (w < 1) w = 1;
                    g.FillRectangle(br, left, yPx - barHeight / 2f, w, barHeight);
                    g.DrawRectangle(pen, left, yPx - barHeight / 2f, w, barHeight);
                    if (barHeight < minH) continue;  // 条太矮跳过
                    // 字号 = Min(条宽, 条高) / 3（Font.Height ≈ pt*1.2），封顶 20f
                    float fontPx = Math.Min(maxFontSize, Math.Max(baseFontSize, Math.Min(w, barHeight) / 3.6f));
                    using (var font = new Font(ax.LabelStyleFont.FontFamily, fontPx, FontStyle.Regular))
                    {
                        float textH = font.Height;
                        string label; fmtLabel(p.YValue, out label, out _);
                        var sz = g.MeasureString(label, font);
                        if (p.YValue >= 0)
                        {
                            // 理想：条右端 + gap
                            float idealLeft = xVal + gap;
                            bool needLeader = idealLeft + sz.Width > inner.Right - 1f;
                            float labelLeft = Math.Min(idealLeft, inner.Right - sz.Width - 1f);
                            float tagY = yPx - textH / 2f;
                            if (needLeader)
                            {
                                float leaderX = idealLeft - gap;  // 指引线末端在条端
                                g.DrawLine(leaderPen, leaderX + sz.Width, yPx, leaderX, yPx);
                            }
                            g.DrawString(label, font, labelBr, new RectangleF(labelLeft, tagY, sz.Width, textH), sf);
                        }
                        else
                        {
                            float idealRight = xVal - gap;
                            float idealLeft = idealRight - sz.Width;
                            bool needLeader = idealLeft < inner.Left + 1f;
                            float labelLeft = Math.Max(idealLeft, inner.Left + 1f);
                            float tagY = yPx - textH / 2f;
                            if (needLeader)
                            {
                                float leaderX = idealRight + gap;
                                g.DrawLine(leaderPen, leaderX, yPx, leaderX - sz.Width, yPx);
                            }
                            g.DrawString(label, font, labelBr, new RectangleF(labelLeft, tagY, sz.Width, textH), sf);
                        }
                    }
                }
            }
        }
        // ============ K 线 Candlestick ============
        /// <summary>数据约定：YValue=Close, YValues[0]=High, YValues[1]=Low, YValues[2]=Open。</summary>
        private void DrawCandlestickSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, int groupIndex, int groupCount)
        {
            float slotWidth = SlotWidthX(ax, inner);
            float candleWidth = groupCount > 1 ? slotWidth / groupCount * 0.7f : slotWidth * 0.55f;
            float groupOffset = groupCount > 1 ? -slotWidth / 2f + candleWidth * groupIndex + candleWidth / 2f : 0;
            Color upColor = Color.FromArgb(239, 68, 68);   // 红=涨（中国配色）
            Color downColor = Color.FromArgb(34, 197, 94); // 绿=跌
            using (var upBr = new SolidBrush(upColor))
            using (var downBr = new SolidBrush(downColor))
            using (var upPen = new Pen(upColor, 1))
            using (var downPen = new Pen(downColor, 1))
            {
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    double close = p.YValue;
                    double high = p.YValues.Length > 0 ? p.YValues[0] : close;
                    double low = p.YValues.Length > 1 ? p.YValues[1] : close;
                    double open = p.YValues.Length > 2 ? p.YValues[2] : close;
                    float xPx = MapX(p.XValue, ax, inner) + groupOffset;
                    float yHigh = MapY(high, ay, inner);
                    float yLow = MapY(low, ay, inner);
                    float yOpen = MapY(open, ay, inner);
                    float yClose = MapY(close, ay, inner);
                    bool up = close >= open;
                    var bodyBrush = up ? upBr : downBr;
                    var bodyPen = up ? upPen : downPen;
                    // 影线（High-Low 竖线），与实体同色
                    g.DrawLine(bodyPen, xPx, yHigh, xPx, yLow);
                    float bodyTop = Math.Min(yOpen, yClose);
                    float bodyBot = Math.Max(yOpen, yClose);
                    float bodyH = bodyBot - bodyTop; if (bodyH < 1) bodyH = 1;
                    g.FillRectangle(bodyBrush, xPx - candleWidth / 2f, bodyTop, candleWidth, bodyH);
                    g.DrawRectangle(bodyPen, xPx - candleWidth / 2f, bodyTop, candleWidth, bodyH);
                }
            }
        }
        // ============ 点数图 PointAndFigure ============
        /// <summary>数据约定：YValue=Close, YValues[0]=High, YValues[1]=Low（无 High/Low 时用 Close）。
        /// 对齐 MS Chart：价格按 BoxSize 分盒，上涨列画 X、下跌列画 O，反转需达 ReversalAmount 个盒另起一列。</summary>
        private void DrawPointAndFigureSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color)
        {
            // 收集每根数据的最高价、最低价
            var highs = new List<double>();
            var lows = new List<double>();
            foreach (var p in s.Points)
            {
                if (p.IsEmpty) continue;
                double close = p.YValue;
                double hi = p.YValues.Length > 0 ? p.YValues[0] : close;
                double lo = p.YValues.Length > 1 ? p.YValues[1] : close;
                if (hi < lo) { double t = hi; hi = lo; lo = t; }
                highs.Add(hi); lows.Add(lo);
            }
            if (highs.Count == 0) return;
            double rangeHi = highs[0], rangeLo = lows[0];
            for (int i = 1; i < highs.Count; i++)
            {
                if (highs[i] > rangeHi) rangeHi = highs[i];
                if (lows[i] < rangeLo) rangeLo = lows[i];
            }
            double priceRange = rangeHi - rangeLo; if (priceRange <= 0) priceRange = Math.Abs(rangeHi) * 0.1 + 1;
            double box = NiceBoxSize(priceRange / 30.0);   // 目标约 30 个价格盒
            int reversal = 3;                               // 反转阈值（盒数），与 MS Chart 默认一致
            // 生成列：每列 = (是否上涨, 该列占据的价格盒序号列表)
            var cols = new List<KeyValuePair<bool, List<int>>>();
            bool? up = null;
            int edge = 0;                       // 上涨列=最高盒序号，下跌列=最低盒序号
            List<int> cur = null;
            for (int i = 0; i < highs.Count; i++)
            {
                int hb = (int)Math.Floor(highs[i] / box);
                int lb = (int)Math.Floor(lows[i] / box);
                if (up == null)
                {
                    cur = new List<int>();
                    for (int b = lb; b <= hb; b++) cur.Add(b);
                    cols.Add(new KeyValuePair<bool, List<int>>(true, cur));
                    up = true; edge = hb;
                    continue;
                }
                if (up == true)
                {
                    if (hb >= edge + 1)
                    {
                        for (int b = edge + 1; b <= hb; b++) cur.Add(b);
                        edge = hb;
                    }
                    else if (lb <= edge - reversal)
                    {
                        cur = new List<int>();
                        for (int b = edge - reversal; b >= lb; b--) cur.Add(b);
                        cols.Add(new KeyValuePair<bool, List<int>>(false, cur));
                        up = false; edge = lb;
                    }
                }
                else
                {
                    if (lb <= edge - 1)
                    {
                        for (int b = edge - 1; b >= lb; b--) cur.Add(b);
                        edge = lb;
                    }
                    else if (hb >= edge + reversal)
                    {
                        cur = new List<int>();
                        for (int b = edge + reversal; b <= hb; b++) cur.Add(b);
                        cols.Add(new KeyValuePair<bool, List<int>>(true, cur));
                        up = true; edge = hb;
                    }
                }
            }
            if (cols.Count == 0) return;
            int m = cols.Count;
            int n = highs.Count;
            // 列横向铺满绘图区：列 c 中心对应 X 世界坐标 (c+0.5)*n/m（沿用点序 X 轴）
            double xScale = (double)n / m;
            // 单盒像素高度（世界 box → 屏幕）
            float boxPx = Math.Abs(MapY(box, ay, inner) - MapY(0, ay, inner));
            float colW = inner.Width / m;
            float glyph = Math.Max(3f, Math.Min(colW, boxPx) * 0.72f);
            Color upColor = Color.FromArgb(239, 68, 68);    // 红 X = 上涨
            Color downColor = Color.FromArgb(34, 197, 94);  // 绿 O = 下跌
            using (var upPen = new Pen(upColor, Math.Max(1.4f, glyph * 0.12f)))
            using (var downPen = new Pen(downColor, Math.Max(1.4f, glyph * 0.12f)))
            {
                for (int c = 0; c < m; c++)
                {
                    float cx = MapX((c + 0.5) * xScale, ax, inner);
                    bool isUp = cols[c].Key;
                    var pen = isUp ? upPen : downPen;
                    foreach (int b in cols[c].Value)
                    {
                        float cy = MapY((b + 0.5) * box, ay, inner);
                        if (isUp)
                        {
                            g.DrawLine(pen, cx - glyph / 2f, cy - glyph / 2f, cx + glyph / 2f, cy + glyph / 2f);
                            g.DrawLine(pen, cx - glyph / 2f, cy + glyph / 2f, cx + glyph / 2f, cy - glyph / 2f);
                        }
                        else
                        {
                            g.DrawEllipse(pen, cx - glyph / 2f, cy - glyph / 2f, glyph, glyph);
                        }
                    }
                }
            }
        }
        /// <summary>把价格盒尺寸取为 1/2/5 ×10^n 的"漂亮"数值。</summary>
        private static double NiceBoxSize(double raw)
        {
            if (raw <= 0 || double.IsNaN(raw) || double.IsInfinity(raw)) return 1;
            double pow = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double frac = raw / pow;
            double nice = frac < 1.5 ? 1 : frac < 3 ? 2 : frac < 7 ? 5 : 10;
            return nice * pow;
        }
        // ============ 箱线图 BoxPlot ============
        /// <summary>数据约定：YValue=Median, YValues[0]=Min, YValues[1]=Max, YValues[2]=Q1, YValues[3]=Q3；
        /// Samples 非空时在箱体宽度内画每个原始样本（蜂群错列、互不重叠）。</summary>
        private void DrawBoxPlotSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color, int groupIndex, int groupCount)
        {
            float slotWidth = SlotWidthX(ax, inner);
            float boxWidth = groupCount > 1 ? slotWidth / groupCount * 0.7f : slotWidth * 0.6f;
            float groupOffset = groupCount > 1 ? -slotWidth / 2f + boxWidth * groupIndex + boxWidth / 2f : 0;
            using (var br = new SolidBrush(ControlPaint.Light(color, 0.65f)))
            using (var pen = new Pen(color, 1.6f))
            using (var medPen = new Pen(Color.Red, 2f))
            using (var dotPen = new Pen(color, 1.2f))
            using (var dotBr = new SolidBrush(Color.White))
            {
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    double median = p.YValue;
                    double q1 = p.YValues.Length > 2 ? p.YValues[2] : median;
                    double q3 = p.YValues.Length > 3 ? p.YValues[3] : median;
                    double whiskerLow = p.YValues.Length > 0 ? p.YValues[0] : q1;
                    double whiskerHigh = p.YValues.Length > 1 ? p.YValues[1] : q3;
                    float xPx = MapX(p.XValue, ax, inner) + groupOffset;
                    float yQ1 = MapY(q1, ay, inner);
                    float yQ3 = MapY(q3, ay, inner);
                    float yMed = MapY(median, ay, inner);
                    float yWL = MapY(whiskerLow, ay, inner);
                    float yWH = MapY(whiskerHigh, ay, inner);
                    g.DrawLine(pen, xPx, yWL, xPx, yQ1);
                    g.DrawLine(pen, xPx, yQ3, xPx, yWH);
                    g.DrawLine(pen, xPx - boxWidth / 4f, yWL, xPx + boxWidth / 4f, yWL);
                    g.DrawLine(pen, xPx - boxWidth / 4f, yWH, xPx + boxWidth / 4f, yWH);
                    float boxTop = Math.Min(yQ1, yQ3);
                    float boxH = Math.Abs(yQ3 - yQ1); if (boxH < 2) boxH = 2;
                    g.FillRectangle(br, xPx - boxWidth / 2f, boxTop, boxWidth, boxH);
                    g.DrawRectangle(pen, xPx - boxWidth / 2f, boxTop, boxWidth, boxH);
                    g.DrawLine(medPen, xPx - boxWidth / 2f, yMed, xPx + boxWidth / 2f, yMed);
                    // 原始样本散点（每个学生一个点）：按 Y 排序后蜂群错列——
                    // 从中心列排起，圆与已放点重叠就向 +1,-1,+2,-2… 列依次外开，
                    // 列偏移被箱宽限死，点圆严格落在箱体宽度内、互不重叠
                    if (p.Samples.Count == 0) continue;
                    var vals = new List<double>(p.Samples);
                    vals.Sort();
                    const float dotR = 2.6f;
                    float pitch = dotR * 2f + 0.8f;
                    float maxOff = boxWidth / 2f - dotR - 0.5f;
                    var placed = new List<PointF>();
                    foreach (double v in vals)
                    {
                        float sy = MapY(v, ay, inner);
                        float off = 0f;
                        for (int k2 = 0; ; k2++)
                        {
                            float col = k2 == 0 ? 0f : (k2 % 2 == 1 ? (k2 + 1) / 2f : -k2 / 2f) * pitch;
                            if (col > maxOff) col = maxOff;
                            bool clash = false;
                            foreach (var q in placed)
                            {
                                float dx = q.X - col, dy = q.Y - sy;
                                if (dx * dx + dy * dy < pitch * pitch) { clash = true; break; }
                            }
                            if (!clash) { off = col; break; }
                            if (k2 > 60) { off = col; break; } // 样本极密、箱宽只够单列时兜底贴边
                        }
                        placed.Add(new PointF(off, sy));
                        float px = xPx + off;
                        g.FillEllipse(dotBr, px - dotR, sy - dotR, dotR * 2f, dotR * 2f);
                        g.DrawEllipse(dotPen, px - dotR, sy - dotR, dotR * 2f, dotR * 2f);
                    }
                }
            }
        }
        // ============ 气泡图 Bubble ============
        /// <summary>数据约定：XValue=X, YValue=Y, YValues[0]=气泡大小（面积参考值，自动归一化）。</summary>
        private void DrawBubbleSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color)
        {
            double maxSize = 1;
            foreach (var p in s.Points)
                if (!p.IsEmpty && p.YValues.Length > 0) maxSize = Math.Max(maxSize, Math.Abs(p.YValues[0]));
            float maxR = Math.Min(inner.Width, inner.Height) * 0.14f;
            using (var pen = new Pen(color, 1))
            {
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    float xPx = MapX(p.XValue, ax, inner);
                    float yPx = MapY(p.YValue, ay, inner);
                    double size = p.YValues.Length > 0 ? Math.Abs(p.YValues[0]) : maxSize * 0.3;
                    float radius = (float)(maxR * Math.Sqrt(size / maxSize));
                    if (radius < 3) radius = 3;
                    Color fill = p.Color ?? color;
                    using (var br = new SolidBrush(Color.FromArgb(160, fill)))
                        g.FillEllipse(br, xPx - radius, yPx - radius, radius * 2, radius * 2);
                    g.DrawEllipse(pen, xPx - radius, yPx - radius, radius * 2, radius * 2);
                }
            }
        }
        // ============ 漏斗图 Funnel ============
        /// <summary>漏斗几何（绘制与命中测试共用，保证一致）：
        /// 等高分段，段宽与数值成正比，最大值在最上方（MS Chart Funnel 行为）；
        /// 宽度同时受高度和两侧标签留白约束，标签左右交替、紧贴漏斗外缘。</summary>
        private static float[] GetFunnelGeometry(RectangleF area, IList<HChartDataPoint> ptsList,
            out float cx, out float topY, out float segH, out double maxVal)
        {
            maxVal = 1;
            foreach (var p in ptsList) maxVal = Math.Max(maxVal, Math.Abs(p.YValue));
            cx = area.Left + area.Width / 2f;
            topY = area.Top + area.Height * 0.10f;
            float totalH = area.Height * 0.80f;
            int n = ptsList.Count;
            segH = totalH / n;
            // 按两侧最宽标签给漏斗预留横向空间（标签左右交替排布）
            const float leaderOut = 8f, labelGap = 3f, padSide = 3f;
            float maxTwL = 0f, maxTwR = 0f;
            using (var labelFont = new Font("Microsoft YaHei UI", 8f))
            {
                for (int i = 0; i < n; i++)
                {
                    string name = ptsList[i].AxisLabel ?? ptsList[i].Label ?? HTranslation.GetContent($"段{i + 1}");
                    string text = $"{name} {Math.Abs(ptsList[i].YValue):#,0}";
                    var ts = TextRenderer.MeasureText(text, labelFont, Size.Empty,
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    float tw = ts.Width + 2f;
                    if (i % 2 == 1) { if (tw > maxTwR) maxTwR = tw; }
                    else { if (tw > maxTwL) maxTwL = tw; }
                }
            }
            float rByH = totalH * 0.45f;
            float rByWL = area.Width / 2f - (maxTwL + leaderOut + labelGap + padSide);
            float rByWR = area.Width / 2f - (maxTwR + leaderOut + labelGap + padSide);
            float maxHalfW = Math.Max(20f, Math.Min(rByH, Math.Min(rByWL, rByWR)));
            var halfW = new float[n];
            for (int i = 0; i < n; i++)
                halfW[i] = (float)(maxHalfW * Math.Abs(ptsList[i].YValue) / maxVal);
            return halfW;
        }
        /// <summary>
        /// 绘制漏斗图：自上而下每层等宽高度、半宽与数值成正比（最大值在顶部最宽），
        /// 层间梯形衔接；hover 时白色外圈加粗；外侧"名称 数值"标签左右交替排布，
        /// 带短指引线、背景色块遮挡，且整体不出 area 边界。
        /// </summary>
        private void DrawFunnelSeries(Graphics g, RectangleF area, HChartArea ca, HChartSeries s)
        {
            var ptsList = s.Points.FindAll(p => !p.IsEmpty);
            if (ptsList.Count == 0) return;
            int n = ptsList.Count;
            float[] halfW = GetFunnelGeometry(area, ptsList, out float cx, out float topY, out float segH, out double maxVal);
            const TextFormatFlags labelFlags =
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            using (var labelFont = new Font("Microsoft YaHei UI", 8f))
            using (var linePen = new Pen(Color.FromArgb(150, 150, 150)))
            using (var bgBrush = new SolidBrush(ca.BackColor))
            {
                for (int i = 0; i < n; i++)
                {
                    float wTop = halfW[i];
                    float wBot = (i + 1 < n) ? halfW[i + 1] : halfW[i] * 0.7f;
                    float yTop = topY + i * segH;
                    float yBot = yTop + segH;
                    Color c = ptsList[i].Color ?? ChartPalettes.GetColor(Palette, i, ThemeIsDark);
                    var poly = new PointF[]
                    {
                        new PointF(cx - wTop, yTop), new PointF(cx + wTop, yTop),
                        new PointF(cx + wBot, yBot), new PointF(cx - wBot, yBot),
                    };
                    using (var br = new SolidBrush(Color.FromArgb(225, c)))
                    using (var pen = new Pen(c, 1))
                    {
                        g.FillPolygon(br, poly);
                        g.DrawPolygon(pen, poly);
                    }
                    // hover 高亮：白色外圈加粗描边
                    bool isHover = _lastHit != null &&
                        ReferenceEquals(_lastHit.Series, s) &&
                        ReferenceEquals(_lastHit.Point, ptsList[i]);
                    if (isHover)
                    {
                        using (var hoverPen = new Pen(Color.White, 2f))
                            g.DrawPolygon(hoverPen, poly);
                    }
                    // === 外侧标签：紧贴漏斗斜边中点，左右交替，不出控件 ===
                    string name = ptsList[i].AxisLabel ?? ptsList[i].Label ?? HTranslation.GetContent($"段{i + 1}");
                    string text = $"{name} {Math.Abs(ptsList[i].YValue):#,0}";
                    var ts = TextRenderer.MeasureText(g, text, labelFont, Size.Empty,
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    float tw = ts.Width + 2f, th = ts.Height;
                    bool isRight = (i % 2 == 1);
                    float midY = (yTop + yBot) / 2f;
                    float midHalfW = (wTop + wBot) / 2f;
                    float tx;
                    if (isRight)
                    {
                        float edgeX = cx + midHalfW;
                        tx = Math.Min(edgeX + 8f + 3f, area.Right - 3f - tw);
                        var textRect = Rectangle.Round(new RectangleF(tx - 2f, midY - th / 2f - 2f, tw + 4f, th + 4f));
                        g.FillRectangle(bgBrush, textRect);
                        g.DrawLine(linePen, edgeX, midY, tx - 3f, midY);
                        TextRenderer.DrawText(g, text, labelFont, textRect, Color.FromArgb(60, 60, 60), labelFlags);
                    }
                    else
                    {
                        float edgeX = cx - midHalfW;
                        tx = Math.Max(edgeX - 8f - 3f - tw, area.Left + 3f);
                        var textRect = Rectangle.Round(new RectangleF(tx - 2f, midY - th / 2f - 2f, tw + 4f, th + 4f));
                        g.FillRectangle(bgBrush, textRect);
                        g.DrawLine(linePen, edgeX, midY, tx + tw + 3f, midY);
                        TextRenderer.DrawText(g, text, labelFont, textRect, Color.FromArgb(60, 60, 60), labelFlags);
                    }
                }
            }
        }
        /// <summary>
        /// 绘制散点图（Point）：每个数据点映射到 inner 坐标系后直接画 marker；
        /// MarkerStyle 为 None 时默认用实心圆、尺寸过小时放大到 8px，点颜色支持逐点自定义（p.Color）。
        /// </summary>
        private void DrawPointSeries(Graphics g, HChartAxis ax, HChartAxis ay, RectangleF inner, HChartSeries s, Color color)
        {
            var marker = s.MarkerStyle == HChartMarkerStyle.None ? HChartMarkerStyle.Circle : s.MarkerStyle;
            var size = s.MarkerSize <= 6 ? 8 : s.MarkerSize;
            foreach (var p in s.Points)
            {
                if (p.IsEmpty) continue;
                float xPx = MapX(p.XValue, ax, inner);
                float yPx = MapY(p.YValue, ay, inner);
                DrawMarker(g, new PointF(xPx, yPx), marker, size, p.Color ?? color);
            }
        }
        /// <summary>Pie / Doughnut 绘制：hover 扇形沿中心角方向轻微 explode + 白色外圈；
        /// 带指引线和外侧标签（AxisLabel + 百分比）；Doughnut 中心孔用背景色填充。
        /// area 为"控件减 Padding"的内容框（paddedArea）：饼心/半径/标签全部不出此框。</summary>
        private void DrawPieSeries(Graphics g, RectangleF area, HChartArea ca, HChartSeries s)
        {
            double total = 0;
            foreach (var p in s.Points) if (!p.IsEmpty) total += Math.Abs(p.YValue);
            if (total <= 0) { _pieGeoCache.Remove(s); return; }
            float cx = area.Left + area.Width / 2f;
            float cy = area.Top + area.Height / 2f;
            const float leaderOut = 12f;   // 沿扇形中线向外的短引线
            const float labelGap = 4f;     // 文字与卡片边缘/引线间隙
            const float labelRowH = 17f;   // 标签行高（8pt 字 + 行距）
            const float minPctForLabel = 6f;
            const float explodeDist = 6f;
            // ===== 1. 收集可见扇形 + 测量标签（用 GDI TextRenderer 测量，保证与实际绘制一致）=====
            var labels = new List<PieLabelLay>();
            float startAngle = -90f;
            using (var labelFont = new Font("Microsoft YaHei UI", 8f))
            {
                for (int i = 0; i < s.Points.Count; i++)
                {
                    var p = s.Points[i];
                    if (p.IsEmpty) continue;
                    float sweep = (float)(Math.Abs(p.YValue) / total * 360f);
                    float pct = (float)(Math.Abs(p.YValue) / total * 100);
                    float midDeg = startAngle + sweep / 2f;
                    double midRad = midDeg * Math.PI / 180.0;
                    double cosA = Math.Cos(midRad), sinA = Math.Sin(midRad);
                    if (pct >= minPctForLabel)
                    {
                        string name = p.AxisLabel ?? p.Label ?? HTranslation.GetContent($"点{i + 1}");
                        string text = $"{name} {pct:F1}%";
                        var ts = TextRenderer.MeasureText(g, text, labelFont, Size.Empty,
                            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                        labels.Add(new PieLabelLay
                        {
                            Point = p,
                            Cos = cosA, Sin = sinA,
                            Text = text,
                            Tw = ts.Width + 2f, Th = ts.Height,
                            IsRight = cosA >= 0,
                        });
                    }
                    startAngle += sweep;
                }
                // ===== 2. 半径：左右两侧各自按最宽标签预留空白，标签紧贴饼缘且不出控件 =====
                float maxTwL = 0f, maxTwR = 0f;
                foreach (var l in labels)
                {
                    if (l.IsRight) { if (l.Tw > maxTwR) maxTwR = l.Tw; }
                    else { if (l.Tw > maxTwL) maxTwL = l.Tw; }
                }
                float radius = GetPieRadius(area, maxTwL, maxTwR, leaderOut, labelGap);
                float innerR = s.ChartType == HChartType.Doughnut ? radius * 0.55f : 0f;
                _pieGeoCache[s] = new PieGeo { Area = area, Radius = radius, InnerR = innerR };
                // ===== 3. 画扇形（hover 时沿中心角 explode）=====
                startAngle = -90f;
                for (int i = 0; i < s.Points.Count; i++)
                {
                    var p = s.Points[i]; if (p.IsEmpty) continue;
                    float sweep = (float)(Math.Abs(p.YValue) / total * 360f);
                    Color sliceColor = p.Color ?? ChartPalettes.GetColor(Palette, i, ThemeIsDark);
                    bool isHover = _lastHit != null &&
                        ReferenceEquals(_lastHit.Series, s) &&
                        ReferenceEquals(_lastHit.Point, p);
                    double midRad = (startAngle + sweep / 2f) * Math.PI / 180.0;
                    float ox = isHover ? (float)(explodeDist * Math.Cos(midRad)) : 0f;
                    float oy = isHover ? (float)(explodeDist * Math.Sin(midRad)) : 0f;
                    using (var br = new SolidBrush(sliceColor))
                    {
                        g.FillPie(br, cx - radius + ox, cy - radius + oy, radius * 2, radius * 2, startAngle, sweep);
                        g.DrawArc(Pens.White, cx - radius + ox, cy - radius + oy, radius * 2, radius * 2, startAngle, sweep);
                    }
                    if (isHover)
                    {
                        using (var hoverPen = new Pen(Color.White, 2f))
                            g.DrawArc(hoverPen, cx - radius + ox, cy - radius + oy, radius * 2, radius * 2, startAngle, sweep);
                    }
                    startAngle += sweep;
                }
                // === Doughnut 内环 ===
                if (s.ChartType == HChartType.Doughnut)
                {
                    using (var bg = new SolidBrush(ca.BackColor))
                        g.FillEllipse(bg, cx - innerR, cy - innerR, innerR * 2, innerR * 2);
                }
                // ===== 4. 标签布局：文字列紧贴饼缘外侧，仅在逼近控件边缘时才外推，纵向避让 =====
                foreach (var l in labels)
                {
                    if (l.IsRight)
                    {
                        // 文字左缘贴饼缘 + 引线 + 间隙；超出右边界时向左收（始终在控件内）
                        float hugX = cx + radius + leaderOut + labelGap;
                        l.Tx = Math.Min(hugX, area.Right - labelGap - l.Tw);
                    }
                    else
                    {
                        // 文字右缘贴饼缘；超出左边界时向右收
                        float hugRight = cx - radius - leaderOut - labelGap;
                        l.Tx = Math.Max(hugRight - l.Tw, area.Left + labelGap);
                    }
                    l.Ty = (float)(cy + (radius + leaderOut) * l.Sin) - l.Th / 2f;
                }
                LayoutSideLabels(labels.FindAll(l => l.IsRight), area, labelRowH, avoidLegend: true);
                LayoutSideLabels(labels.FindAll(l => !l.IsRight), area, labelRowH, avoidLegend: false);
                // ===== 5. 画指引线 + 文字 =====
                // 文字统一走 GDI TextRenderer（网格对齐像素、颜色不透明），避免 GDI+ ClearType 在小块背景上发灰
                const TextFormatFlags labelFlags =
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
                using (var linePen = new Pen(Color.FromArgb(150, 150, 150)))
                using (var bgBrush = new SolidBrush(ca.BackColor))
                {
                    foreach (var l in labels)
                    {
                        bool isHover = _lastHit != null &&
                            ReferenceEquals(_lastHit.Series, s) &&
                            ReferenceEquals(_lastHit.Point, l.Point);
                        float ox = isHover ? (float)(explodeDist * l.Cos) : 0f;
                        float oy = isHover ? (float)(explodeDist * l.Sin) : 0f;
                        // P1 扇形边缘 → P2 向外短引线折点 → P3 文字列
                        float p1x = (float)(cx + ox + radius * l.Cos);
                        float p1y = (float)(cy + oy + radius * l.Sin);
                        float p2x = (float)(cx + ox + (radius + leaderOut) * l.Cos);
                        float p2y = (float)(cy + oy + (radius + leaderOut) * l.Sin);
                        float textMidY = l.Ty + l.Th / 2f;
                        float p3x = l.IsRight ? l.Tx - labelGap : l.Tx + l.Tw + labelGap;
                        g.DrawLine(linePen, p1x, p1y, p2x, p2y);
                        g.DrawLine(linePen, p2x, p2y, p3x, textMidY);
                        // 先铺不透明底（盖住穿过文字区的引导线/底图），再 GDI 画字
                        var textRect = Rectangle.Round(new RectangleF(l.Tx - 2f, l.Ty - 2f, l.Tw + 4f, l.Th + 4f));
                        g.FillRectangle(bgBrush, textRect);
                        TextRenderer.DrawText(g, l.Text, labelFont, textRect,
                            Color.FromArgb(60, 60, 60), labelFlags);
                    }
                }
            }
        }
        /// <summary>同一侧饼图标签纵向避让：按折点 Y 排序、最小行高推挤、整体夹在控件内；右侧额外避开图例框。</summary>
        private void LayoutSideLabels(List<PieLabelLay> side, RectangleF area, float labelRowH, bool avoidLegend)
        {
            if (side.Count == 0) return;
            side.Sort((a, b) => a.Ty.CompareTo(b.Ty));
            // 右侧标签避开图例框（图例锚定右上角）
            if (avoidLegend && !_legendBoxBounds.IsEmpty)
            {
                float legendBottom = _legendBoxBounds.Bottom + 2f;
                foreach (var l in side)
                {
                    if (l.Ty < legendBottom && l.Ty + labelRowH > _legendBoxBounds.Top - 2f)
                        l.Ty = legendBottom;
                }
            }
            // 顺序推挤，保证行间距
            for (int i = 1; i < side.Count; i++)
            {
                float prevBottom = side[i - 1].Ty + labelRowH;
                if (side[i].Ty < prevBottom) side[i].Ty = prevBottom;
            }
            // 整体夹在控件内（右侧避让图例时，上边界即图例框底：上移兜底也不会把标签抬回图例区）
            float minY = area.Top + 2f;
            if (avoidLegend && !_legendBoxBounds.IsEmpty)
                minY = Math.Max(minY, _legendBoxBounds.Bottom + 2f);
            float maxY = area.Bottom - labelRowH - 2f;
            float overflow = side[side.Count - 1].Ty + labelRowH - maxY;
            if (overflow > 0) foreach (var l in side) l.Ty -= overflow;
            if (side[0].Ty < minY)
            {
                float up = minY - side[0].Ty;
                foreach (var l in side) l.Ty += up;
            }
        }
        /// <summary>Pie/Doughnut 半径：高度方向取 41%，宽度方向左右各按该侧最宽标签留白（半径+引线+文字≤半宽）。</summary>
        private static float GetPieRadius(RectangleF area, float maxTwLeft, float maxTwRight, float leaderOut, float labelGap)
        {
            float rByH = area.Height * 0.41f;
            float padSide = 3f;
            float rByWL = area.Width / 2f - (maxTwLeft + leaderOut + labelGap + padSide);
            float rByWR = area.Width / 2f - (maxTwRight + leaderOut + labelGap + padSide);
            return Math.Max(28f, Math.Min(rByH, Math.Min(rByWL, rByWR)));
        }
        /// <summary>
        /// 自由布局系列（Pie/Doughnut/Funnel/Radar）的内容框：控件区四边内缩 4px Padding，
        /// 与 DrawChartArea 的 paddedArea 同源。绘制与命中测试都以此为基准，保证图形/标签不出框、命中不错位。
        /// </summary>
        private static RectangleF GetFreeLayoutRect(RectangleF area)
        {
            const float pad = 4f;
            return new RectangleF(area.Left + pad, area.Top + pad,
                area.Width - 2f * pad, area.Height - 2f * pad);
        }
        /// <summary>饼图标签布局临时结构（绘制时收集测量、排布后统一绘制）。</summary>
        private class PieLabelLay
        {
            /// <summary>标签对应的扇形数据点。</summary>
            public HChartDataPoint Point;
            /// <summary>扇形中线方向的单位向量（决定指引线朝向与标签左右侧）。</summary>
            public double Cos, Sin;
            /// <summary>标签文字（"名称 百分比%"）。</summary>
            public string Text;
            /// <summary>标签文字测量宽/高（像素）。</summary>
            public float Tw, Th;
            /// <summary>标签是否排布在饼图右侧（false=左侧）。</summary>
            public bool IsRight;
            /// <summary>标签最终绘制位置（文字左上角，像素）。</summary>
            public float Tx, Ty;
        }
        /// <summary>
        /// 雷达底图（与系列数据解耦）：同心多边形网格 + 辐条 + 维度标签。
        /// 只要 ChartArea 含雷达系列就画——所有系列都被图例取消时底图仍在；维度取点数最多的雷达系列。
        /// 网格/辐条色在主题轴色基础上向刻度文字色混合，与绘图区背景拉开对比，深底浅底主题都清晰可辨。
        /// </summary>
        private void DrawRadarFrame(Graphics g, RectangleF area, HChartArea ca, List<HChartSeries> radarAll)
        {
            HChartSeries baseS = radarAll[0];
            foreach (var rs in radarAll)
                if (rs.Points.Count > baseS.Points.Count) baseS = rs;
            int n = baseS.Points.Count; if (n < 3) return;
            var cx = area.Left + area.Width / 2f;
            var cy = area.Top + area.Height / 2f + 6;
            float radius = Math.Min(area.Width, area.Height) / 2f * 0.62f;
            var ax = ca.AxisX;
            // 主题网格色默认很浅（#E6E6E6），直接用会与 WhiteSmoke 绘图区背景融在一起；
            // 向刻度文字色混合 45%/35% 得到中灰网格/深灰辐条，保证底图清晰
            Color gridCol = MixColor(ax.MajorGridColor, ax.LabelForeColor, 0.45f);
            Color spokeCol = MixColor(ax.LineColor, ax.LabelForeColor, 0.35f);
            using (var gridPen = new Pen(gridCol))
            {
                // 同心多边形网格（4 圈）
                for (int ring = 1; ring <= 4; ring++)
                {
                    float rr = radius * ring / 4f;
                    var gp = new PointF[n];
                    for (int i = 0; i < n; i++)
                    {
                        double a = -Math.PI / 2 + 2 * Math.PI * i / n;
                        gp[i] = new PointF((float)(cx + rr * Math.Cos(a)), (float)(cy + rr * Math.Sin(a)));
                    }
                    g.DrawPolygon(gridPen, gp);
                }
            }
            using (var spokePen = new Pen(spokeCol))
            using (var labelBr = new SolidBrush(ax.LabelForeColor))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int i = 0; i < n; i++)
                {
                    double a = -Math.PI / 2 + 2 * Math.PI * i / n;
                    float ex = (float)(cx + radius * Math.Cos(a));
                    float ey = (float)(cy + radius * Math.Sin(a));
                    g.DrawLine(spokePen, cx, cy, ex, ey);
                    string label = baseS.Points[i].AxisLabel ?? i.ToString();
                    float lx = (float)(cx + (radius + 12) * Math.Cos(a));
                    float ly = (float)(cy + (radius + 12) * Math.Sin(a));
                    g.DrawString(label, ax.LabelStyleFont, labelBr, lx, ly, sf);
                }
            }
        }
        private void DrawRadarSeries(Graphics g, RectangleF area, List<HChartSeries> allSeries, HChartSeries s, Color color)
        {
            int n = s.Points.Count; if (n < 3) return;
            var cx = area.Left + area.Width / 2f;
            var cy = area.Top + area.Height / 2f + 6;
            float radius = Math.Min(area.Width, area.Height) / 2f * 0.62f;
            // 所有启用雷达系列共享最大值（底图网格由 DrawRadarFrame 独立绘制，与数据解耦）
            double maxY = 1;
            foreach (var rs in allSeries)
                if (rs.ChartType == HChartType.Radar)
                    foreach (var p in rs.Points)
                        if (!p.IsEmpty) maxY = Math.Max(maxY, Math.Abs(p.YValue));
            var pts = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                if (s.Points[i].IsEmpty) { pts[i] = new PointF(cx, cy); continue; }
                double a = -Math.PI / 2 + 2 * Math.PI * i / n;
                double dist = Math.Abs(s.Points[i].YValue) / maxY * radius;
                pts[i] = new PointF((float)(cx + dist * Math.Cos(a)), (float)(cy + dist * Math.Sin(a)));
            }
            // 填充固定 30% 不透明度（255*0.3≈77）：既能看出系列色块，底图网格/辐条也透得出来
            using (var br = new SolidBrush(Color.FromArgb(77, color)))
            using (var pen = new Pen(color, 2))
            {
                g.FillPolygon(br, pts);
                g.DrawPolygon(pen, pts);
            }
        }
        // ============ Legend / Title ============
        /// <summary>
        /// 图例框测量：内嵌在坐标系绘图区（inner）的右上角。
        /// 规则（对齐需求）：① 完全落在 inner 内；② 宽度不超过 inner 右半、高度不超过上半；
        /// ③ 系列过多时条目可在图例框内滚动（右侧滚动条指示器，鼠标悬停图例框滚轮切换）；
        /// ④ 文字过长用省略号；
        /// ⑤ 绘图区过小（连一行都放不下）时不显示；⑥ 背景仅 20% 不透明度（半透明）。
        /// 只测量不绘制：重建图例条目缓存、计算框 Bounds/滚动状态并写入 _legendBoxBounds。
        /// DrawChartArea 在画系列前先调一次——饼/环标签布局据此让出图例位置，不再依赖上一帧残留 Bounds。
        /// 图例不可见（禁用/无条目/绘图区太小）时返回 Empty。
        /// </summary>
        private RectangleF MeasureLegendBox(Graphics g, HChartArea ca, RectangleF inner, HChartLegend lg)
        {
            // 每次绘制重建图例条目缓存
            _legendEntries.Clear();
            _legendBoxBounds = RectangleF.Empty;
            _legendCanScroll = false;
            _legendMaxOffset = 0;
            if (!lg.IsEnabled) return RectangleF.Empty;
            // === 判断本 ChartArea 是否有 Pie/Doughnut 系列 ===
            var pieSeries = Series.FirstOrDefault(s =>
                s.ChartArea == ca.Name &&
                (s.ChartType == HChartType.Pie || s.ChartType == HChartType.Doughnut));
            // === 收集图例条目 ===
            if (pieSeries != null)
            {
                // Pie：每个 DataPoint 一条图例（包括已 IsEmpty 的，让用户可以重新显示）
                for (int i = 0; i < pieSeries.Points.Count; i++)
                {
                    var p = pieSeries.Points[i];
                    _legendEntries.Add(new LegendEntry
                    {
                        Series = pieSeries,
                        Point = p,
                        IsHidden = p.IsEmpty,
                    });
                }
            }
            else
            {
                // 普通图：每个 Series 一条图例（即使 IsEnabled=false 也显示，灰色+删除线）
                for (int i = 0; i < Series.Count; i++)
                {
                    var s = Series[i];
                    if (!s.IsVisibleInLegend || s.ChartArea != ca.Name) continue;
                    _legendEntries.Add(new LegendEntry
                    {
                        Series = s,
                        Point = null,
                        IsHidden = !s.IsEnabled,
                    });
                }
            }
            if (_legendEntries.Count == 0) return RectangleF.Empty;
            // 条目保持构建原序：取消/选中只在原位置灰化，位置不随显隐状态变化（取消不再沉底）
            int totalCount = _legendEntries.Count;
            const float pad = 6f;          // 图例框与绘图区边缘/内部留白
            const float swatchW = 14f;     // 色块宽
            const float swatchH = 10f;     // 色块高
            const float gap = 8f;          // 色块与文字间距
            const float scrollBarW = 8f;   // 滚动时右侧预留滚动条宽
            float itemH = lg.Font.Height + 3f;
            // 约束：不超过绘图区一半宽、一半高
            float maxW = inner.Width * 0.5f;
            float maxH = inner.Height * 0.5f;
            float textMaxW = maxW - pad * 2f - swatchW - gap - scrollBarW;
            // 绘图区太小 / 连一行文字都放不下 → 不显示图例
            if (inner.Width < 96f || inner.Height < 72f || textMaxW < 24f) return RectangleF.Empty;
            int maxRows = (int)Math.Floor((maxH - pad * 2f) / itemH);
            if (maxRows < 1) return RectangleF.Empty;
            // 可视行数固定（不随显隐/滚动变化，保证框高稳定）；滚动范围覆盖全部条目
            int visibleRows = Math.Min(totalCount, maxRows);
            bool needsScroll = totalCount > visibleRows;
            int maxOffset = Math.Max(0, totalCount - visibleRows);
            _legendCanScroll = needsScroll;
            _legendMaxOffset = maxOffset;
            if (!needsScroll) _legendScrollOffset = 0;
            if (_legendScrollOffset > maxOffset) _legendScrollOffset = maxOffset;
            if (_legendScrollOffset < 0) _legendScrollOffset = 0;
            // === 计算框宽：按全部条目文字最宽值测量（显隐/滚动都不改变框宽）===
            float measured = 0f;
            for (int i = 0; i < totalCount; i++)
                measured = Math.Max(measured, g.MeasureString(GetLegendItemText(_legendEntries[i]), lg.Font).Width);
            float textW = Math.Min(measured, textMaxW);
            float wExtra = needsScroll ? scrollBarW : 0f;
            float w = pad * 2f + swatchW + gap + textW + wExtra;
            float h = pad * 2f + visibleRows * itemH;
            // 右上角锚定（完全在 inner 内）
            float x = inner.Right - w - pad;
            float y = inner.Top + pad;
            // 记录图例框整体 Bounds（绝对屏幕坐标），用于 HitTest 与饼/环标签避让
            _legendBoxBounds = new RectangleF(x, y, w, h);
            return _legendBoxBounds;
        }
        /// <summary>绘制图例（框几何/条目缓存由 MeasureLegendBox 统一预算，保证与饼/环标签避让同源）。</summary>
        private void DrawLegend(Graphics g, HChartLegend lg, HChartArea ca, RectangleF inner, Dictionary<HChartSeries, Color> colorMap)
        {
            RectangleF box = MeasureLegendBox(g, ca, inner, lg);
            if (box.IsEmpty) return;
            int totalCount = _legendEntries.Count;
            bool needsScroll = _legendCanScroll;
            const float pad = 6f, swatchW = 14f, swatchH = 10f, gap = 8f, scrollBarW = 8f;
            float itemH = lg.Font.Height + 3f;
            float x = box.X, y = box.Y, w = box.Width, h = box.Height;
            int visibleRows = Math.Min(totalCount, (int)Math.Floor((h - pad * 2f) / itemH));
            float textW = w - pad * 2f - swatchW - gap - (needsScroll ? scrollBarW : 0f);
            // 半透明背景（20% 不透明度）+ 极淡边框
            using (var bg = new SolidBrush(Color.FromArgb(51, lg.BackColor)))
                g.FillRectangle(bg, x, y, w, h);
            using (var border = new Pen(Color.FromArgb(64, 120, 120, 120), 1f))
                g.DrawRectangle(border, x, y, w, h);
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            {
                // 仅渲染滚动窗口内的条目；窗口外条目 Bounds 保持 Empty（不可点击）
                for (int i = 0; i < totalCount; i++)
                {
                    var entry = _legendEntries[i];
                    if (i < _legendScrollOffset || i >= _legendScrollOffset + visibleRows)
                    {
                        entry.Bounds = RectangleF.Empty;
                        _legendEntries[i] = entry;
                        continue;
                    }
                    float cy = y + pad + (i - _legendScrollOffset) * itemH;
                    Color itemColor = GetLegendItemColor(entry, colorMap);
                    // 色块（隐藏条目用灰色）
                    Color swatchColor = entry.IsHidden
                        ? Color.FromArgb(180, 180, 180)
                        : itemColor;
                    using (var swBr = new SolidBrush(swatchColor))
                        g.FillRectangle(swBr, x + pad, cy + (itemH - swatchH) / 2f, swatchW, swatchH);
                    // 文字颜色（隐藏条目用灰色）
                    Color textColor = entry.IsHidden
                        ? Color.FromArgb(160, 160, 160)
                        : lg.ForeColor;
                    using (var textBr = new SolidBrush(textColor))
                    {
                        var textRect = new RectangleF(x + pad + swatchW + gap, cy, textW, itemH);
                        g.DrawString(GetLegendItemText(entry), lg.Font, textBr, textRect, sf);
                        // 隐藏条目加删除线（就地灰化，位置不变）
                        if (entry.IsHidden)
                        {
                            float strikeY = cy + itemH / 2f;
                            using (var strikePen = new Pen(Color.FromArgb(160, 160, 160), 1f))
                                g.DrawLine(strikePen, textRect.Left, strikeY, textRect.Right, strikeY);
                        }
                    }
                    // === 记录条目完整 Bounds（绝对屏幕坐标，HitTest 直接用）===
                    entry.Bounds = new RectangleF(x + pad, cy, swatchW + gap + textW, itemH);
                    _legendEntries[i] = entry;
                }
                // === 滚动指示器（滑块按全部条目数比例）===
                if (needsScroll)
                {
                    float trackX = x + w - 4f;
                    float trackY = y + pad;
                    float trackH = h - pad * 2f;
                    using (var trackBr = new SolidBrush(Color.FromArgb(30, 150, 150, 150)))
                        g.FillRectangle(trackBr, trackX - 1f, trackY, 2f, trackH);
                    float thumbH = Math.Max(6f, trackH * visibleRows / totalCount);
                    float thumbY = trackY;
                    if (_legendMaxOffset > 0)
                        thumbY = trackY + (trackH - thumbH) * _legendScrollOffset / _legendMaxOffset;
                    using (var thumbBr = new SolidBrush(Color.FromArgb(120, 150, 150, 150)))
                        g.FillRectangle(thumbBr, trackX - 2f, thumbY, 4f, thumbH);
                }
            }
        }
        /// <summary>获取图例条目的显示文字（Pie 用 AxisLabel，普通用 Series.LegendText）。</summary>
        private static string GetLegendItemText(LegendEntry entry)
        {
            if (entry.Point != null)
                return entry.Point.AxisLabel ?? entry.Point.Label ?? HTranslation.GetContent("点");
            return entry.Series?.LegendText ?? entry.Series?.Name ?? "";
        }
        /// <summary>获取图例条目的色块颜色。</summary>
        private Color GetLegendItemColor(LegendEntry entry, Dictionary<HChartSeries, Color> colorMap)
        {
            if (entry.Point != null)
            {
                // Pie：DataPoint 自带颜色或按调色板
                int idx = entry.Series?.Points.IndexOf(entry.Point) ?? 0;
                return entry.Point.Color ?? ChartPalettes.GetColor(Palette, idx, ThemeIsDark);
            }
            // 普通 Series
            if (entry.Series != null)
            {
                if (colorMap != null && colorMap.TryGetValue(entry.Series, out var mc)) return mc;
                int idx = Series.IndexOf(entry.Series);
                return entry.Series.ResolveColor(idx, Palette, ThemeIsDark);
            }
            return Color.Gray;
        }
        /// <summary>自动标题字号：随控件短边缩放（5%），夹在 [下限,上限]；短边过小返回 null（标题不绘制也不占边距）。</summary>
        private const float TitleAutoMinPx = 9f, TitleAutoMaxPx = 14f;
        /// <summary>顶停靠标题距绘图区顶边的间隙（标题紧贴坐标系上沿、略高于右上小图例）。</summary>
        private const float TitlePlotGap = 3f;
        /// <summary>顶停靠标题距 paddedArea 顶边的最小上留白。</summary>
        private const float TitleTopInset = 1f;
        private Font _autoTitleFont;
        private float _autoTitleFontSize = -1f;
        /// <summary>解析标题实际字体：显式赋过 Font 就固定使用；否则按控件尺寸自适应并缓存同一实例。</summary>
        private Font ResolveTitleFont(HChartTitle t)
        {
            if (t.Font != null) return t.Font;
            float size = Math.Min(ClientSize.Width, Math.Max(1, ClientSize.Height)) * 0.05f;
            if (size < TitleAutoMinPx) return null;
            size = Math.Min(size, TitleAutoMaxPx);
            if (_autoTitleFont == null || Math.Abs(_autoTitleFontSize - size) > 0.05f)
            {
                _autoTitleFont?.Dispose();
                _autoTitleFont = new Font("Microsoft YaHei UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
                _autoTitleFontSize = size;
            }
            return _autoTitleFont;
        }
        /// <summary>解析标题颜色：显式赋过 ForeColor 就固定；否则取启用图表区当前主题的轴标题色（切主题即变色）。</summary>
        private Color ResolveTitleColor(HChartTitle t)
        {
            if (!t.ForeColor.IsEmpty) return t.ForeColor;
            foreach (var ca in ChartAreas)
                if (ca.IsEnabled) return ca.AxisX.TitleForeColor;
            return Color.Black;
        }
        /// <summary>
        /// 绘制图表标题：按 Docking 停靠到 上/下/左/右 四边，按 Alignment 决定水平对齐；
        /// 停靠在左右两侧时文字整体旋转 -90° 竖排，上下侧正常横排。
        /// 未显式设置字体/颜色时：字号随控件尺寸自适应（过小不画）、颜色随配色主题。
        /// </summary>
        private void DrawTitle(Graphics g, HChartTitle t, RectangleF area)
        {
            if (string.IsNullOrEmpty(t.Text)) return;
            var font = ResolveTitleFont(t);
            if (font == null) return; // 自动字号低于下限：标题隐藏
            var sf = new StringFormat
            {
                Alignment = t.Alignment == HChartAlignment.Near ? StringAlignment.Near
                         : t.Alignment == HChartAlignment.Far ? StringAlignment.Far
                         : StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            float x, y;
            switch (t.Docking)
            {
                // Top：标题紧贴第一个启用图表区坐标系上沿（上方 TitleTopInset、下方 TitlePlotGap），
                // 视觉上正好压在坐标框上方、略高于右上角小图例；无图表区时退回控件顶边
                case HChartDocking.Top:
                {
                    float plotTop = area.Top + TitleTopInset + font.Height / 2f;
                    foreach (var ca in ChartAreas)
                        if (ca.IsEnabled) { plotTop = GetInnerPlotRect(ca).Top - TitlePlotGap - font.Height / 2f; break; }
                    y = plotTop; x = area.Width / 2f; break;
                }
                case HChartDocking.Bottom: y = area.Height - t.Padding - font.Height / 2f; x = area.Width / 2f; break;
                case HChartDocking.Left: y = area.Height / 2f; x = t.Padding + font.Height; break;
                case HChartDocking.Right: y = area.Height / 2f; x = area.Width - t.Padding; break;
                default: y = t.Padding + font.Height / 2f; x = area.Width / 2f; break;
            }
            using (var br = new SolidBrush(ResolveTitleColor(t)))
            {
                if (t.Docking == HChartDocking.Left || t.Docking == HChartDocking.Right)
                {
                    g.TranslateTransform(x, y);
                    g.RotateTransform(-90);
                    g.DrawString(t.Text, font, br, 0, 0, sf);
                    g.ResetTransform();
                }
                else
                {
                    g.DrawString(t.Text, font, br, x, y, sf);
                }
            }
        }
        // ============ 右键菜单动作（菜单本体已提取至 HFrom\Chart\Menu\HChartMenu.cs） ============
        /// <summary>切换交互模式（再次点击同一项退出；Esc 也可退出）。</summary>
        private void ToggleMode(InteractMode mode)
        {
            // 再次点击同一项 → 退出，同时清除所有标尺线
            if (_mode == mode)
            {
                _mode = InteractMode.None;
                _rbCommitted = RectangleF.Empty;
                ClearRulers();
            }
            else
            {
                // 进入新模式：框选/选中清旧选区框，标尺清对立方向的标尺线，互相独立切换
                _mode = mode;
                _rbDragging = false;
                _rbCommitted = RectangleF.Empty;
                if (mode == InteractMode.RulerV) _rulerHY.Clear();
                else if (mode == InteractMode.RulerH) _rulerVX.Clear();
                else ClearRulers(); // 框选放大 / 选中标记模式：标尺全部清掉
            }
            HideTip();
            Cursor = _mode == InteractMode.None ? Cursors.Default : Cursors.Cross;
            Invalidate();
        }
        /// <summary>显示所有系列：恢复所有被图例点掉的系列；饼图/环图的每个切片就是一个数据点，一并恢复。</summary>
        private void ShowAllData()
        {
            foreach (var s in Series)
            {
                s.IsEnabled = true;
                if (s.ChartType == HChartType.Pie || s.ChartType == HChartType.Doughnut)
                    foreach (var p in s.Points) p.IsEmpty = false;
            }
            Invalidate();
        }
        /// <summary>一键切换图表类型：所有启用系列改为指定类型（含饼图/雷达/K线/条形，之间可互相切换）。</summary>
        private void ApplyChartStyle(HChartType style)
        {
            foreach (var s in Series)
            {
                if (!s.IsEnabled) continue;
                s.ChartType = style;
            }
            // 切风格后量程/类别轴状态可能变化，复位缩放保证不残留在旧量程
            ResetZoom();
            Invalidate();
        }
        /// <summary>还原原图：把所有 Series 的 ChartType 恢复为首次绘制时记录的初始类型（未记录则按折线）。</summary>
        private void RestoreOriginalTypes()
        {
            foreach (var s in Series) s.ChartType = s.OriginalType ?? HChartType.Line;
            ResetZoom();
            Invalidate();
        }
        /// <summary>把当前图表保存为图片文件（PNG/JPG/BMP）。</summary>
        private void SaveChartImage()
        {
            using (var dlg = new SaveFileDialog
            {
                Title = HTranslation.GetContent("保存图片"),
                Filter = HTranslation.GetContent("PNG 图片|*.png|JPEG 图片|*.jpg|BMP 图片|*.bmp"),
                FileName = "HChart.png"
            })
            {
                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;
                using (var bmp = new Bitmap(Math.Max(1, Width), Math.Max(1, Height)))
                {
                    DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                    if (ext == ".jpg" || ext == ".jpeg") bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    else if (ext == ".bmp") bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    else bmp.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }
        // ============ 交互覆盖层：框选矩形 + 测量标尺 ============
        /// <summary>在所有图表绘制完成后绘制交互覆盖物。</summary>
        private void DrawOverlay(Graphics g)
        {
            // --- 框选放大 / 选中标记虚线矩形 ---
            // 拖框中：虚线框 + 框范围弹窗（弹窗在框外侧，绝不压框边）；
            // 选中标记松手后：只保留矩形框，不弹窗、不缩放量程；框选放大松手即缩放，不留框
            if (_mode == InteractMode.RubberBand || _mode == InteractMode.Mark)
            {
                RectangleF rect;
                bool draggingNow = _rbDragging;
                if (draggingNow)
                    rect = RectangleF.FromLTRB(
                        Math.Min(_rbStart.X, _rbEnd.X), Math.Min(_rbStart.Y, _rbEnd.Y),
                        Math.Max(_rbStart.X, _rbEnd.X), Math.Max(_rbStart.Y, _rbEnd.Y));
                else if (_mode == InteractMode.Mark && !_rbCommitted.IsEmpty)
                    rect = _rbCommitted;
                else
                    rect = RectangleF.Empty;
                if (rect.Width >= 2 && rect.Height >= 2 && PointInInner(_rbStart, out var rbHit))
                {
                    var ca = rbHit.Item1;
                    var inner = rbHit.Item2;
                    // 矩形夹在绘图区内
                    rect.Intersect(inner);
                    ComputeAxis(ca);
                    SetupScreens(inner, ca);
                    Color boxCol = Color.FromArgb(220, 70, 110, 200);   // 选区蓝框
                    using (var pen = new Pen(boxCol, 1.5f) { DashStyle = DashStyle.Dash })
                    {
                        g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                        using (var br = new SolidBrush(Color.FromArgb(28, boxCol.R, boxCol.G, boxCol.B)))
                            g.FillRectangle(br, rect);
                    }
                    // 仅拖框过程中显示框范围文字（世界坐标）；落定的选中框不弹窗
                    if (draggingNow)
                    {
                        var w0 = _screen.ScreenToWorld(new HPoint(rect.Left, rect.Bottom));
                        var w1 = _screen.ScreenToWorld(new HPoint(rect.Right, rect.Top));
                        double x0 = Math.Min(w0.X.Value, w1.X.Value), x1 = Math.Max(w0.X.Value, w1.X.Value);
                        double y0 = Math.Min(w0.Y.Value, w1.Y.Value), y1 = Math.Max(w0.Y.Value, w1.Y.Value);
                        var rbScheme = BuildHudScheme();
                        var lines = new List<HudLine>
                        {
                            new HudLine("X: " + FormatTick(x0) + " ~ " + FormatTick(x1) + HTranslation.GetContent("（宽 ") + FormatTick(x1 - x0) + "）", rbScheme.Text),
                            new HudLine("Y: " + FormatTick(y0) + " ~ " + FormatTick(y1) + HTranslation.GetContent("（高 ") + FormatTick(y1 - y0) + "）", rbScheme.Text),
                        };
                        // 弹窗整块放在框外（框正上方/正下方，选剩余空间更大的一侧），中心对齐框中心：
                        // 按整个控件客户区计空间，绘图区外的轴标签带也允许借用（弹窗只压标签不压框边）；
                        // 两侧都放不下（框几乎占满控件）时不显示，避免弹窗压住框边造成"缺一边"
                        float hudH = lines.Count * 16f + 10f;   // 与 DrawHud 行高/内边距同源
                        const float hudGap = 6f;
                        float roomTop = rect.Top - 2f;
                        float roomBottom = ClientSize.Height - 2f - rect.Bottom;
                        bool canTop = roomTop >= hudH + hudGap;
                        bool canBottom = roomBottom >= hudH + hudGap;
                        if (canTop || canBottom)
                        {
                            bool up = canTop && (!canBottom || roomTop >= roomBottom);
                            float ay2 = up
                                ? rect.Top - hudH / 2f - hudGap
                                : rect.Bottom + hudH / 2f + hudGap;
                            DrawHud(g, new PointF((rect.Left + rect.Right) / 2f, ay2),
                                lines, inner, rbScheme, true);
                        }
                    }
                }
            }
            // --- 测量标尺（3 击循环：1击=线A+标签，2击=线B+中点双头箭头与差值，3击=全清） ---
            if (_mode == InteractMode.RulerV || _mode == InteractMode.RulerH)
            {
                // 固定标尺线不依赖鼠标是否停在绘图区内：鼠标不在区内时回退到第一个 ChartArea
                HChartArea ca = null;
                RectangleF inner = default;
                if (_mousePos.HasValue && PointInInner(_mousePos.Value, out var hit)) { ca = hit.Item1; inner = hit.Item2; }
                else if (ChartAreas.Count > 0)
                {
                    ca = ChartAreas[0];
                    inner = GetInnerPlotRect(ca);
                }
                // 是否有已放置的线按实际形态判定（横测竖线、纵测横线）
                bool hasLines = RulerModeDrawsVertical(_mode) ? _rulerVX.Count > 0 : _rulerHY.Count > 0;
                if (ca != null && hasLines)
                {
                    ComputeAxis(ca);
                    SetupScreens(inner, ca);
                    DrawRulers(g, ca, inner);
                }
            }
        }
        /// <summary>标尺文字字体：与轴名称同字号（8.5f 常规，不加粗）。</summary>
        private static readonly Font RulerFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        /// <summary>固定窄矩形（X 类别轴槽位）放不下刻度文字时退化使用的小字号；X/Y 数值刻度不使用。</summary>
        private static readonly Font YTickSmallFont = new Font("Segoe UI", 7f, FontStyle.Regular);
        /// <summary>标尺线/箭头用橙色。</summary>
        private static readonly Color RulerColor = Color.FromArgb(220, 230, 120, 40);
        /// <summary>HUD 框/标尺标签框的反色方案：背景与绘图区背景呈反色调，保证框体跳脱、文字清晰。</summary>
        private struct HudScheme
        {
            /// <summary>弹框填充色（已含 0.9 alpha）。</summary>
            public Color Bg;
            /// <summary>弹框边框色。</summary>
            public Color Border;
            /// <summary>正文颜色。</summary>
            public Color Text;
            /// <summary>标尺标题/数值标签颜色（橙系，随深浅底调亮/调暗）。</summary>
            public Color TagText;
            /// <summary>true=弹框为深底（绘图区是浅底）；系列色需调亮。</summary>
            public bool DarkPopup;
        }
        /// <summary>当前主题是否为深底（深色仪表风、矩阵荧光两种黑类主题）。
        /// HUD/气泡/调色板都按主题深浅取反色，不再读取背景色。</summary>
        private bool ThemeIsDark => _theme == HChartTheme.Dark || _theme == HChartTheme.Matrix;
        /// <summary>按主题深浅构造 HUD 反色方案：浅底主题→深框白字，深底主题→浅框黑字。</summary>
        private HudScheme BuildHudScheme()
        {
            if (!ThemeIsDark)
            {
                // 浅底主题 → 弹框反色为深色
                return new HudScheme
                {
                    DarkPopup = true,
                    Bg = Color.FromArgb(230, 26, 30, 38),
                    Border = Color.FromArgb(160, 255, 255, 255),
                    Text = Color.FromArgb(246, 247, 249),
                    TagText = Color.FromArgb(255, 176, 96),
                };
            }
            // 深底主题 → 弹框反色为浅色
            return new HudScheme
            {
                DarkPopup = false,
                Bg = Color.FromArgb(230, 255, 253, 247),
                Border = Color.FromArgb(220, 200, 150, 90),
                Text = Color.FromArgb(35, 35, 40),
                TagText = Color.FromArgb(200, 95, 15),
            };
        }
        /// <summary>HUD 一行文字（带颜色）：标尺统计行按所属系列/轴的图例颜色着色。</summary>
        private struct HudLine
        {
            /// <summary>本行文字内容。</summary>
            public string Text;
            /// <summary>本行文字颜色（默认色或系列图例色）。</summary>
            public Color Color;
            /// <summary>构造：文字 + 颜色。</summary>
            public HudLine(string text, Color color) { Text = text; Color = color; }
        }
        /// <summary>颜色压暗（浅底弹框上，浅色图例文字乘 0.7 保证对比）。</summary>
        private static Color Darken(Color c, float f = 0.7f)
            => Color.FromArgb(Math.Min(255, (int)(c.R * f)), Math.Min(255, (int)(c.G * f)), Math.Min(255, (int)(c.B * f)));
        /// <summary>颜色调亮（深底弹框上，深色图例文字向白色混合 38% 保证对比）。</summary>
        private static Color Brighten(Color c, float f = 0.38f)
            => Color.FromArgb((int)(c.R + (255 - c.R) * f), (int)(c.G + (255 - c.G) * f), (int)(c.B + (255 - c.B) * f));
        /// <summary>两颜色按 t 线性混合（t=0 全 a，t=1 全 b）。</summary>
        private static Color MixColor(Color a, Color b, float t)
            => Color.FromArgb((int)(a.R + (b.R - a.R) * t),
                              (int)(a.G + (b.G - a.G) * t),
                              (int)(a.B + (b.B - a.B) * t));
        /// <summary>系列图例色按弹框深浅自适应：浅底弹框压暗、深底弹框调亮。</summary>
        private static Color AdaptForPopup(Color c, HudScheme sc)
            => sc.DarkPopup ? Brighten(c) : Darken(c);
        /// <summary>取某 Y 轴对应的图例颜色（该轴第一个启用系列的颜色，与右上角图例一致），并按弹框底色自适应；无系列时回退正文色。</summary>
        private Color AxisSeriesColor(HChartArea ca, HChartAxis ay, HudScheme sc)
        {
            int paletteIndex = 0;
            foreach (var s in Series)
            {
                if (s.ChartArea != ca.Name || !s.IsEnabled) continue;
                // 与 DrawChartArea 一致：区域内每个启用系列占一个调色板位
                var c = s.ResolveColor(paletteIndex, Palette, ThemeIsDark);
                paletteIndex++;
                if (IsCartesian(s.ChartType) && ReferenceEquals(YAxisAt(ca, _barMode ? 0 : s.YAxisIndex), ay))
                    return AdaptForPopup(c, sc);
            }
            return sc.Text;
        }
        /// <summary>圆角矩形 Path：用于 HUD 框/标尺标签框统一圆角视觉（半径 7px，足够柔和又不显得臃肿）。</summary>
        private static GraphicsPath RoundRectPath(float x, float y, float w, float h, float r)
        {
            float d = Math.Min(2 * r, Math.Min(w, h));
            if (d <= 2) { var p = new GraphicsPath(); p.AddRectangle(new RectangleF(x, y, w, h)); return p; }
            r = d / 2f;
            var path = new GraphicsPath();
            path.AddArc(x, y, d, d, 180, 90);                  // 左上
            path.AddArc(x + w - d, y, d, d, 270, 90);          // 右上
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);    // 右下
            path.AddArc(x, y + h - d, d, d, 90, 90);           // 左下
            path.CloseFigure();
            return path;
        }
        /// <summary>绘制标尺标签（单行小框：与 HUD 同风格，夹在 inner 内；文字与标尺线同色；圆角 + 半透明 0.4）。</summary>
        private static void DrawRulerTag(Graphics g, string text, float x, float y, RectangleF inner, HudScheme sc)
        {
            const float pad = 4f;
            const float r = 7f;
            var sz = TextRenderer.MeasureText(text, RulerFont);
            float bw = sz.Width + pad * 2f, bh = sz.Height + pad * 0.5f;
            // 夹在绘图区内
            float bx = Math.Max(inner.Left + 1f, Math.Min(x, inner.Right - bw - 1f));
            float by = Math.Max(inner.Top + 1f, Math.Min(y, inner.Bottom - bh - 1f));
            using (var path = RoundRectPath(bx, by, bw, bh, r))
            using (var br = new SolidBrush(sc.Bg))       // 反色底（0.9 alpha）
            using (var pn = new Pen(sc.Border, 1f))
            {
                g.FillPath(br, path);
                g.DrawPath(pn, path);
            }
            TextRenderer.DrawText(g, text, RulerFont, new Point((int)(bx + pad), (int)(by + pad * 0.5f)), sc.TagText);
        }
        /// <summary>
        /// 绘制标尺：竖线/横线 + 第1条线的数值标签（仅1条线时显示，2条线后Δ值在弹窗内）；
        /// 两条线时在两线中点连一条双头箭头（可拖动：沿箭头方向拖两线、垂直方向拖弹窗位置），
        /// 箭头画在统计弹窗下层（穿过弹窗的部分被盖住），Δ 差值已并入弹窗文字、不再单独浮动标签。
        /// </summary>
        private void DrawRulers(Graphics g, HChartArea ca, RectangleF inner)
        {
            // 实际画线形态：横测=竖线、纵测=横线，所有图表一致（见 RulerModeDrawsVertical）
            bool vertical = RulerModeDrawsVertical(_mode);
            var coords = vertical ? _rulerVX : _rulerHY;
            // 弹框配色：与绘图区背景反色调（浅底→深框白字，深底→浅框黑字）
            var sc = BuildHudScheme();
            using (var pen = new Pen(RulerColor, 1.5f) { DashStyle = DashStyle.Dash })
            // 两线之间的双头箭头连线同样用虚线（卡尺 3 条线全部虚线），两端保留实心箭头
            using (var arrowPen = new Pen(Color.FromArgb(235, RulerColor.R, RulerColor.G, RulerColor.B), 1.8f)
            {
                StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor,
                DashStyle = DashStyle.Dash
            })
            {
                if (vertical)
                {
                    // 竖线标尺
                    float x1 = Math.Max(inner.Left, Math.Min(inner.Right, coords[0]));
                    g.DrawLine(pen, x1, inner.Top, x1, inner.Bottom);
                    if (coords.Count == 2)
                    {
                        float x2 = Math.Max(inner.Left, Math.Min(inner.Right, coords[1]));
                        g.DrawLine(pen, x2, inner.Top, x2, inner.Bottom);
                        float xa = Math.Min(x1, x2), xb = Math.Max(x1, x2);
                        // 中点双头箭头（水平）的 Y：可上下拖动的把手，弹窗以它为垂直锚点
                        float hy = Math.Max(inner.Top, Math.Min(inner.Bottom, _rulerVHandleY));
                        // 1) 箭头先画——在统计弹窗下层，穿过弹窗的部分被弹窗盖住
                        g.DrawLine(arrowPen, xa + 3f, hy, xb - 3f, hy);
                        // 2) 统计 HUD（Δx 第一行 + X 最小/最大第二行 + 各 Y 轴统计）：以两线中点 X、箭头 Y 为中心；
                        //    记录弹窗矩形——弹窗盖住中间线中段，悬停/按住弹窗视同抓住中间线
                        _rulerHudBox = DrawHud(g, new PointF((xa + xb) / 2f, hy), RulerVLines(ca, inner, coords, sc), inner, sc, true);
                    }
                    else
                    {
                        // 仅1条线：显示该线数值标签（线右侧 1/5 高处；太靠右则放左边）
                        string v1 = FormatTick(ScreenXToWorld(inner, ca.AxisX, x1));
                        bool rightOk = x1 + 8f + 70f < inner.Right;
                        DrawRulerTag(g, v1, rightOk ? x1 + 8f : x1 - 78f,
                            inner.Top + inner.Height / 5f, inner, sc);
                    }
                }
                else
                {
                    // 横线标尺
                    float y1 = Math.Max(inner.Top, Math.Min(inner.Bottom, coords[0]));
                    g.DrawLine(pen, inner.Left, y1, inner.Right, y1);
                    if (coords.Count == 2)
                    {
                        float y2 = Math.Max(inner.Top, Math.Min(inner.Bottom, coords[1]));
                        g.DrawLine(pen, inner.Left, y2, inner.Right, y2);
                        float ya = Math.Min(y1, y2), yb = Math.Max(y1, y2);
                        // 中点双头箭头（垂直）的 X：可左右拖动的把手，弹窗以它为水平锚点
                        float hx = Math.Max(inner.Left, Math.Min(inner.Right, _rulerHHandleX));
                        // 1) 箭头先画——在统计弹窗下层
                        g.DrawLine(arrowPen, hx, ya + 3f, hx, yb - 3f);
                        // 2) 统计 HUD（Δy 第一行 + 各启用 Y 轴最小/最大）：以箭头 X、两线中点 Y 为中心；
                        //    记录弹窗矩形——弹窗盖住中间线中段，悬停/按住弹窗视同抓住中间线
                        _rulerHudBox = DrawHud(g, new PointF(hx, (ya + yb) / 2f), RulerHLines(ca, coords, sc), inner, sc, true);
                    }
                    else
                    {
                        // 仅1条线：显示该线数值标签（线下方 1/5 宽处；太靠下则放上方）
                        double wy1 = ScreenFor(ca.AxisY).ScreenToWorld(new HPoint(0, y1)).Y.Value;
                        // 条形图横线量的是类别轴：标签直接显示类别名而非槽位索引
                        string v1 = _barMode ? CategoryNameAt(wy1) : FormatTick(wy1);
                        bool belowOk = y1 + 8f + 20f < inner.Bottom;
                        DrawRulerTag(g, v1, inner.Left + inner.Width / 5f,
                            belowOk ? y1 + 6f : y1 - 22f, inner, sc);
                    }
                }
            }
        }
        /// <summary>竖线标尺（两条固定竖线）：显示 X 范围（标尺橙）+ 各系列 n/均值/方差/最值（用该系列图例颜色）。
        /// n 只数区间内的真实采样点；连续线在区间内一个采样点都没有、但线确实穿过区间时，
        /// 才用两个边界上的线段插值兜底统计，并明确标注"边界插值"。</summary>
        private List<HudLine> RulerVLines(HChartArea ca, RectangleF inner, List<float> screenXs, HudScheme sc)
        {
            var lines = new List<HudLine>();
            // 两条竖线的世界坐标 X 值（先排序方便写 min/max）；用线性精确换算，避开 HScreen 显示精度截断
            var wx0 = ScreenXToWorld(inner, ca.AxisX, screenXs[0]);
            var wx1 = ScreenXToWorld(inner, ca.AxisX, screenXs[1]);
            double xLo = Math.Min(wx0, wx1), xHi = Math.Max(wx0, wx1);
            // 统一按屏幕几何语义：竖线=横坐标测量，文案一律 Δx/X（条形图量的是条长轴，时间轴显示时长）
            string dText = IsTimeAxis(ca.AxisX)
                ? FormatDuration((xHi - xLo) * 86400.0)   // OADate 差（天）→ 秒
                : FormatTick(xHi - xLo);
            lines.Add(new HudLine($"Δx = {dText}", sc.TagText));
            // 时间轴第二行给完整毫秒区间（yyyy-MM-dd HH:mm:ss.fff ~ 同）；其余维持 最小/最大 写法
            string rangeText = IsTimeAxis(ca.AxisX)
                ? HTranslation.GetContent($"X 区间 {FormatPopupX(ca.AxisX, xLo)} ~ {FormatPopupX(ca.AxisX, xHi)}")
                : HTranslation.GetContent($"X 最小 {FormatTick(xLo)}   最大 {FormatTick(xHi)}");
            lines.Add(new HudLine(rangeText, sc.Text));
            int anyN = 0;
            int paletteIndex = 0;
            foreach (var s in Series)
            {
                if (s.ChartArea != ca.Name || !s.IsEnabled) continue;
                var c = AdaptForPopup(s.ResolveColor(paletteIndex, Palette, ThemeIsDark), sc);
                paletteIndex++;
                if (!IsCartesian(s.ChartType)) continue;
                bool cont = IsContinuousLine(s.ChartType);
                var pts = s.Points;
                var vals = new List<double>();           // 区间内真实采样点
                var interp = new List<double>();         // 连续线在两条边界上的插值（兜底用）
                for (int i = 0; i < pts.Count; i++)
                {
                    var p = pts[i];
                    if (p.IsEmpty) continue;
                    // 横向条形时数值在横(X)轴：区间判定与统计都按条长 YValue
                    double keyV = _barMode ? p.YValue : p.XValue;
                    if (keyV >= xLo - 1e-9 && keyV <= xHi + 1e-9) vals.Add(p.YValue);
                    if (!cont || i + 1 >= pts.Count) continue;
                    var q = pts[i + 1];
                    if (q.IsEmpty) continue;
                    double sx = Math.Min(p.XValue, q.XValue), ex = Math.Max(p.XValue, q.XValue);
                    if (sx < xLo && xLo < ex) interp.Add(SegmentY(s.ChartType, p, q, xLo));
                    else if (sx < xHi && xHi < ex) interp.Add(SegmentY(s.ChartType, p, q, xHi));
                }
                bool useInterp = vals.Count == 0 && interp.Count > 0;
                var stat = useInterp ? interp : vals;
                if (stat.Count == 0) continue;
                anyN++;
                int n = stat.Count;
                double mean = stat.Sum() / n;
                double variance = stat.Sum(x => (x - mean) * (x - mean)) / n; // 总体方差
                lines.Add(new HudLine(HTranslation.GetContent($"{s.Name}（n={n}{(useInterp ? "，边界插值" : "")}）均 {FormatTick(mean)}  方 {FormatTick(variance)}"), c));
                lines.Add(new HudLine(HTranslation.GetContent($"  区间 {FormatTick(stat.Min())} ~ {FormatTick(stat.Max())}"), c));
                // 柱/条（05 柱、06 堆叠柱、07 条形）：区间内柱体值的总数（与气泡"系列总数"口径对应）
                if (IsBarType(s.ChartType))
                    lines.Add(new HudLine(HTranslation.GetContent($"  总数 {FormatTick(stat.Sum())}（{n} 项）"), c));
            }
            if (anyN == 0) lines.Add(new HudLine(HTranslation.GetContent("区间内无数据"), sc.Text));
            return lines;
        }
        /// <summary>
        /// 横线标尺（两条固定横线）弹窗内容：Δy（主轴量程差）第一行；
        /// 每根启用 Y 轴给出标尺线的世界 Y 值（线最小/线最大）；随后逐系列给出
        /// 【当前 X 视区内】Y 值落在两横线之间的真实采样点 n/均值/总体方差（系列色），区间另起一行。
        /// 注意：必须限定 X 视区——标尺画在屏幕上，视区外历史点（如实时图全部历史）不可见，
        /// 计入会把振荡曲线多次穿越误报成多个点；n 只数真实采样点，不做线段插值。
        /// </summary>
        private List<HudLine> RulerHLines(HChartArea ca, List<float> screenYs, HudScheme sc)
        {
            // 条形图横线量的是类别轴：走独立的类别带统计
            if (_barMode) return RulerHLinesBar(ca, screenYs, sc);
            var lines = new List<HudLine>();
            // Δy 差值第一行（按主轴 Y 量程）
            var mScr = ScreenFor(ca.AxisY);
            var my0 = mScr.ScreenToWorld(new HPoint(0, screenYs[0])).Y.Value;
            var my1 = mScr.ScreenToWorld(new HPoint(0, screenYs[1])).Y.Value;
            lines.Add(new HudLine($"Δy = {FormatTick(Math.Abs(my1 - my0))}", sc.TagText));
            // 各轴两条横线的世界 Y 值带（标尺测量基准）
            var bands = new Dictionary<HChartAxis, Tuple<double, double>>();
            for (int yi = 0; yi < YAxisCount(ca); yi++)
            {
                var ay = YAxisAt(ca, yi);
                if (!ay.IsEnabled) continue;
                var scr = ScreenFor(ay);
                var wy0 = scr.ScreenToWorld(new HPoint(0, screenYs[0])).Y.Value;
                var wy1 = scr.ScreenToWorld(new HPoint(0, screenYs[1])).Y.Value;
                double yLo = Math.Min(wy0, wy1), yHi = Math.Max(wy0, wy1);
                bands[ay] = Tuple.Create(yLo, yHi);
                var c = AxisSeriesColor(ca, ay, sc);
                string name = AxisDisplayName(ay, AxisSideName(ca, ay));
                lines.Add(new HudLine(HTranslation.GetContent($"{name}  线最小 {FormatTick(yLo)}   线最大 {FormatTick(yHi)}"), c));
            }
            // 只统计当前 X 视区（屏幕 inner）内的点：视区外的历史点不可见、不参与
            double viewLo = ca.AxisX.ActualMinimum, viewHi = ca.AxisX.ActualMaximum;
            int anyN = 0;
            int paletteIndex = 0;
            foreach (var s in Series)
            {
                if (s.ChartArea != ca.Name || !s.IsEnabled) continue;
                var c = AdaptForPopup(s.ResolveColor(paletteIndex, Palette, ThemeIsDark), sc);
                paletteIndex++;
                if (!IsCartesian(s.ChartType)) continue;
                var ay = YAxisAt(ca, s.YAxisIndex);
                if (!bands.TryGetValue(ay, out var band)) continue;
                double yLo = band.Item1, yHi = band.Item2;
                var vals = new List<double>();
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    if (p.XValue < viewLo - 1e-9 || p.XValue > viewHi + 1e-9) continue; // 限定 X 视区
                    if (p.YValue >= yLo - 1e-9 && p.YValue <= yHi + 1e-9) vals.Add(p.YValue);
                }
                if (vals.Count == 0) continue;
                anyN++;
                int n = vals.Count;
                double mean = vals.Sum() / n;
                double variance = vals.Sum(x => (x - mean) * (x - mean)) / n; // 总体方差
                // n/均值/方差一行，区间另起一行（两空格缩进表示从属本系列）；
                // 柱/条（05 柱、06 堆叠柱、07 条形）首行末尾附区间内总数
                string barSum = IsBarType(s.ChartType) ? HTranslation.GetContent($"  总 {FormatTick(vals.Sum())}") : "";
                lines.Add(new HudLine(HTranslation.GetContent($"{s.Name}（n={n}）均 {FormatTick(mean)}  方 {FormatTick(variance)}{barSum}"), c));
                lines.Add(new HudLine($"  [{FormatTick(vals.Min())}~{FormatTick(vals.Max())}]", c));
            }
            if (anyN == 0) lines.Add(new HudLine(HTranslation.GetContent("视区内无数据"), sc.Text));
            return lines;
        }
        /// <summary>
        /// 条形图横线标尺（纵坐标测量）专用统计：两横线夹的是类别槽位带（屏幕垂直方向=类别轴）。
        /// Δy=两线的槽位跨度（个类别），第二行给起止类别名；随后逐系列统计类别中心落在带内的条：
        /// n/均值/总体方差一行、区间另起一行（缩进从属），条体系列首行末尾附条长总数。
        /// </summary>
        private List<HudLine> RulerHLinesBar(HChartArea ca, List<float> screenYs, HudScheme sc)
        {
            var lines = new List<HudLine>();
            var mScr = ScreenFor(ca.AxisY);
            var my0 = mScr.ScreenToWorld(new HPoint(0, screenYs[0])).Y.Value;
            var my1 = mScr.ScreenToWorld(new HPoint(0, screenYs[1])).Y.Value;
            double yLo = Math.Min(my0, my1), yHi = Math.Max(my0, my1);
            // 类别 i 的槽位中心在 i（类别轴量程 -0.5..n-0.5）：中心落在带内的类别才计入
            int iLo = Math.Max(0, (int)Math.Ceiling(yLo - 1e-9));
            int iHi = Math.Min(Math.Max(0, _catCount - 1), (int)Math.Floor(yHi + 1e-9));
            lines.Add(new HudLine(HTranslation.GetContent($"Δy = {FormatTick(yHi - yLo)} 个类别"), sc.TagText));
            if (iLo <= iHi)
                lines.Add(new HudLine(HTranslation.GetContent($"Y 最小 {CategoryNameAt(iLo)}   最大 {CategoryNameAt(iHi)}"), sc.Text));
            else
                lines.Add(new HudLine(HTranslation.GetContent("带内无类别"), sc.Text));
            int anyN = 0;
            int paletteIndex = 0;
            foreach (var s in Series)
            {
                if (s.ChartArea != ca.Name || !s.IsEnabled) continue;
                var c = AdaptForPopup(s.ResolveColor(paletteIndex, Palette, ThemeIsDark), sc);
                paletteIndex++;
                if (!IsCartesian(s.ChartType)) continue;
                var vals = new List<double>();
                foreach (var p in s.Points)
                {
                    if (p.IsEmpty) continue;
                    int ci = (int)Math.Round(p.XValue);
                    if (ci < iLo || ci > iHi) continue; // 条的类别槽位在带内才统计
                    vals.Add(p.YValue);
                }
                if (vals.Count == 0) continue;
                anyN++;
                int n = vals.Count;
                double mean = vals.Sum() / n;
                double variance = vals.Sum(x => (x - mean) * (x - mean)) / n;
                string barSum = IsBarType(s.ChartType) ? HTranslation.GetContent($"  总 {FormatTick(vals.Sum())}") : "";
                lines.Add(new HudLine(HTranslation.GetContent($"{s.Name}（n={n}）均 {FormatTick(mean)}  方 {FormatTick(variance)}{barSum}"), c));
                lines.Add(new HudLine($"  [{FormatTick(vals.Min())}~{FormatTick(vals.Max())}]", c));
            }
            if (anyN == 0 && iLo <= iHi) lines.Add(new HudLine(HTranslation.GetContent("带内无数据"), sc.Text));
            return lines;
        }
        /// <summary>类别槽位索引 → 类别名（无标签/越界时回退 #序号）。</summary>
        private string CategoryNameAt(double index)
        {
            int i = (int)Math.Round(index);
            if (i >= 0 && i < _catLabels.Count && !string.IsNullOrEmpty(_catLabels[i])) return _catLabels[i];
            return "#" + i;
        }
        /// <summary>Y 轴方位兜底名：左1/右1/左2/右2……（偶数索引在左、奇数在右，自内向外交替）。</summary>
        private static string AxisSideName(HChartArea ca, HChartAxis ay)
        {
            int yi = YAxisIndexOf(ca, ay);
            return (YAxisSide(yi) > 0 ? HTranslation.GetContent("右") : HTranslation.GetContent("左")) + (YAxisOrdinal(yi) + 1);
        }
        /// <summary>轴显示名：优先“名称 (单位)”，无标题时用 左轴/右轴 兜底。</summary>
        private static string AxisDisplayName(HChartAxis ay, string fallback)
        {
            var t = AxisTitleText(ay);
            return string.IsNullOrEmpty(t) ? fallback : t;
        }
        /// <summary>绘制半透明信息框（string[] 重载，文字统一用正文色），返回弹窗外接矩形（供差值标签避让）。</summary>
        private RectangleF DrawHud(Graphics g, PointF anchor, string[] lines, RectangleF inner, HudScheme sc, bool center = false)
        {
            var list = new List<HudLine>(lines.Length);
            foreach (var t in lines) list.Add(new HudLine(t, sc.Text));
            return DrawHud(g, anchor, list, inner, sc, center);
        }
        /// <summary>
        /// 绘制半透明圆角信息框（多行、每行可独立配色，标尺统计/框选 HUD 使用），返回弹窗外接矩形。
        /// 排版保证弹窗【完整落在控件客户区内】：
        /// ① 超过客户区宽度的长行优先在分隔符（空格、，、；、[、（ 等）处折行，
        ///    整个无分隔片段仍超宽时才按字符硬断（不会把数字拦腰截断，折出的行沿用原行颜色）；
        /// ② 行数超出客户区单列容量时自动换成多列（列间留 10px，整框仍为一个圆角块）；
        /// ③ center=true（标尺弹窗）→ 整块中心对齐锚点（中点箭头），再整体夹取；
        ///    center=false（框选矩形右下角）→ 整块在锚点左上方展开。
        /// inner 参数保留以兼容旧调用，夹取边界统一使用控件客户区（弹窗可跨绘图区边、落在轴带上）。
        /// 圆角 7px、反色底 0.9 alpha，文字用 TextRenderer 绘制保证清晰。
        /// </summary>
        private RectangleF DrawHud(Graphics g, PointF anchor, List<HudLine> lines, RectangleF inner, HudScheme sc, bool center = false)
        {
            // 统一用 RulerFont（8.5f Segoe UI），保证标尺标签/统计 HUD/框选 HUD 字号完全一致
            var font = RulerFont;
            const float pad = 6f, lh = 16f, r = 7f, colGap = 10f, m = 2f;
            // 可用区域：整个控件客户区四向留 2px（不再只夹绘图区 inner，避免弹窗被绘图区边缘裁切）
            float availX = m, availY = m, availW = Math.Max(40f, ClientSize.Width - 2f * m), availH = Math.Max(40f, ClientSize.Height - 2f * m);
            float maxTextW = availW - pad * 2f;
            // 1) 长行折行：贪心累加，超宽时优先在最近的分隔符处断开（数字/词不被拦腰截断），
            //    没有可用分隔符（整段无分隔的超长片段）才按字符硬断，保证任何一行都不宽过客户区
            var flat = new List<HudLine>();
            foreach (var hl in lines)
            {
                string acc = string.Empty;
                foreach (char ch in hl.Text)
                {
                    string test = acc + ch;
                    if (acc.Length > 0 && TextRenderer.MeasureText(test, font).Width > maxTextW)
                    {
                        int bp = LastBreakIndex(acc, maxTextW);
                        if (bp > 0)
                        {
                            // 在分隔符处断：head 去掉尾部空白，tail 去掉开头空白后继续累加
                            flat.Add(new HudLine(acc.Substring(0, bp).TrimEnd(), hl.Color));
                            acc = acc.Substring(bp).TrimStart();
                        }
                        else
                        {
                            flat.Add(new HudLine(acc, hl.Color));
                            acc = string.Empty;
                        }
                    }
                    acc += ch;
                }
                flat.Add(new HudLine(acc, hl.Color));
            }
            // 2) 分列：从"每列装满客户区高度"开始，若总宽超出客户区则减列（每列多装行），直到放得下；只剩一列时为止
            int capacity = Math.Max(1, (int)Math.Floor((availH - pad * 2f + 2f) / lh));
            int cols = Math.Max(1, (int)Math.Ceiling(flat.Count / (double)capacity));
            int rowsPerCol;
            float totalW;
            while (true)
            {
                rowsPerCol = Math.Max(1, (int)Math.Ceiling(flat.Count / (double)cols));
                totalW = 0f;
                for (int c = 0; c < cols; c++)
                {
                    float w = 1f;
                    for (int i = c * rowsPerCol; i < Math.Min(flat.Count, (c + 1) * rowsPerCol); i++)
                        w = Math.Max(w, TextRenderer.MeasureText(flat[i].Text, font).Width);
                    totalW += w + pad * 2f;
                    if (c > 0) totalW += colGap;
                }
                if (cols <= 1 || totalW <= availW) break;
                cols--;
            }
            // 每列实际宽度（独立按本列最长行量）
            var colWs = new float[cols];
            for (int c = 0; c < cols; c++)
            {
                float w = 1f;
                for (int i = c * rowsPerCol; i < Math.Min(flat.Count, (c + 1) * rowsPerCol); i++)
                    w = Math.Max(w, TextRenderer.MeasureText(flat[i].Text, font).Width);
                colWs[c] = w + pad * 2f;
            }
            totalW = 0f;
            for (int c = 0; c < cols; c++) totalW += colWs[c] + (c > 0 ? colGap : 0f);
            int usedRows = Math.Min(rowsPerCol, flat.Count);
            float bh = usedRows * lh + pad * 2f - 2f;
            float bw = totalW;
            // 3) 定位：居中模式整块中心对齐锚点；框选模式整块在锚点左上展开；再整体夹在客户区内
            float bx = center ? anchor.X - bw / 2f : anchor.X + 6f;
            float by = center ? anchor.Y - bh / 2f : anchor.Y - bh - 6f;
            bx = Math.Max(availX, Math.Min(bx, availX + availW - bw));
            by = Math.Max(availY, Math.Min(by, availY + availH - bh));
            // 4) 整体圆角底框 + 逐列逐行文字
            using (var path = RoundRectPath(bx, by, bw, bh, r))
            using (var br = new SolidBrush(sc.Bg))        // 反色底（0.9 alpha）
            using (var pen = new Pen(sc.Border, 1f))
            {
                g.FillPath(br, path);
                g.DrawPath(pen, path);
            }
            float colX = bx;
            for (int c = 0; c < cols; c++)
            {
                int from = c * rowsPerCol, to = Math.Min(flat.Count, from + rowsPerCol);
                for (int i = from; i < to; i++)
                    TextRenderer.DrawText(g, flat[i].Text, font,
                        new Point((int)(colX + pad), (int)(by + pad - 1f + (i - from) * lh)),
                        flat[i].Color);
                colX += colWs[c] + colGap;
            }
            return new RectangleF(bx, by, bw, bh);
        }
        /// <summary>
        /// 返回字符串 s 中【最后一个适合折行的位置】（断点之后的内容作为新行开头），没有则返回 -1。
        /// 断点规则：空格/，/、/；/：之后可断；[ 与 （ 之前可断（让"[最小~最大]""（n=3）"整体落到新行）。
        /// 同时要求断点两侧都有非空白内容，且断点前半段宽度不超 maxW（避免在超长无分隔词里断出超宽行）。
        /// </summary>
        private int LastBreakIndex(string s, float maxW)
        {
            int result = -1;
            for (int i = 0; i < s.Length; i++)
            {
                int p = -1;
                char c = s[i];
                if (c == ' ' || c == '，' || c == '、' || c == '；' || c == '：') p = i + 1; // 在分隔符之后断
                else if (c == '[' || c == '（') p = i;                                    // 在开括号之前断
                if (p <= 0 || p >= s.Length) continue;
                if (s.Substring(0, p).Trim().Length == 0 || s.Substring(p).Trim().Length == 0) continue;
                if (TextRenderer.MeasureText(s.Substring(0, p).TrimEnd(), RulerFont).Width <= maxW) result = p;
            }
            return result;
        }
    }
    /// <summary>ChartArea 集合。</summary>
    public class HChartAreaCollection : List<HChartArea>
    {
        public HChartArea this[string name]
        {
            get
            {
                foreach (var c in this) if (c.Name == name) return c;
                return null;
            }
        }
    }
    /// <summary>Legend 集合。</summary>
    public class HChartLegendCollection : List<HChartLegend>
    {
        public HChartLegend this[string name]
        {
            get
            {
                foreach (var l in this) if (l.Title == name) return l;
                return null;
            }
        }
    }
    /// <summary>Title 集合。</summary>
    public class HChartTitleCollection : List<HChartTitle> { }
    /// <summary>命中测试结果，封装一次鼠标 HitTest 的全部信息。</summary>
    public class HChartHitTestResult
    {
        /// <summary>命中所属的 ChartArea。</summary>
        public HChartArea ChartArea { get; set; }
        /// <summary>命中的数据系列（可能为 null，鼠标在轴上但不在数据点上时）。</summary>
        public HChartSeries Series { get; set; }
        /// <summary>命中的数据点（可能为 null）。</summary>
        public HChartDataPoint Point { get; set; }
        /// <summary>鼠标相对于 innerPlot 的像素坐标。</summary>
        public PointF LocalPoint { get; set; }
    }
    /// <summary>
    /// 图例条目：记录每次 OnPaint 时绘制的图例条目的屏幕区域 + 关联对象。
    /// 用于 HitTest 图例点击切换显隐。
    /// </summary>
    internal struct LegendEntry
    {
        /// <summary>条目在控件坐标系下的完整区域（色块 + 文字）。</summary>
        public RectangleF Bounds;
        /// <summary>关联的 Series（非 Pie 图）。</summary>
        public HChartSeries Series;
        /// <summary>关联的 DataPoint（Pie/Doughnut 图）。</summary>
        public HChartDataPoint Point;
        /// <summary>当前是否处于"被隐藏"状态（用于渲染灰色+删除线）。</summary>
        public bool IsHidden;
    }
    /// <summary>PointHover 事件参数：鼠标悬停命中数据点时触发，携带命中的系列/数据点与原始鼠标参数。</summary>
    public class HChartPointHoverEventArgs : EventArgs
    {
        /// <summary>命中的数据点所属系列。</summary>
        public HChartSeries Series { get; set; }
        /// <summary>命中的数据点（Pie/Doughnut 等图按点命中时也有效）。</summary>
        public HChartDataPoint Point { get; set; }
        /// <summary>原始鼠标事件参数（含屏幕坐标、按键状态，便于外部做自定义提示）。</summary>
        public System.Windows.Forms.MouseEventArgs MouseArgs { get; set; }
    }
}
