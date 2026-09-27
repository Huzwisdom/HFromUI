using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart
{
    /// <summary>
    /// 图例，镜像 System.Windows.Forms.DataVisualization.Charting.Legend。
    /// 显示 Series 名称 + 颜色方格里的标识。
    /// </summary>
    public class HChartLegend
    {
        /// <summary>图例启用状态。</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>停靠位置（Top/Bottom/Left/Right）。</summary>
        public HChartDocking Docking { get; set; } = HChartDocking.Top;

        /// <summary>对齐方式（配合 Top/Bottom 使用 Near=左对齐、Far=右对齐、Center=居中）。</summary>
        public HChartAlignment Alignment { get; set; } = HChartAlignment.Far;

        /// <summary>图例标题（可选）。</summary>
        public string Title { get; set; }

        /// <summary>图例标题字体。</summary>
        public Font TitleFont { get; set; } = new Font("Segoe UI", 9f, FontStyle.Bold);

        /// <summary>图例字体（条目文字）。</summary>
        public Font Font { get; set; } = new Font("Segoe UI", 8f);

        /// <summary>条目文字颜色。</summary>
        public Color ForeColor { get; set; } = Color.Black;

        /// <summary>图例背景颜色。</summary>
        public Color BackColor { get; set; } = Color.WhiteSmoke;

        /// <summary>边框颜色。</summary>
        public Color BorderColor { get; set; } = Color.LightGray;

        /// <summary>边框宽度。0=无边框。</summary>
        public int BorderWidth { get; set; } = 1;

        /// <summary>图例与绘图区域之间的内边距（像素）。</summary>
        public int Padding { get; set; } = 10;
    }

    /// <summary>
    /// 标题，镜像 System.Windows.Forms.DataVisualization.Charting.Title。
    /// 用于 Chart 顶部/底部/左右显示标题文字。
    /// </summary>
    public class HChartTitle
    {
        /// <summary>标题文字。</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>停靠位置。</summary>
        public HChartDocking Docking { get; set; } = HChartDocking.Top;

        /// <summary>对齐方式。</summary>
        public HChartAlignment Alignment { get; set; } = HChartAlignment.Center;

        private Font _font;
        /// <summary>
        /// 字体。null（默认）= 自适应：字号随坐标系大小缩放（有上限，太小则不显示标题）；
        /// 一旦显式赋值即固定，不再随尺寸变化。
        /// </summary>
        public Font Font { get => _font; set => _font = value; }

        /// <summary>
        /// 文字颜色。Color.Empty（默认）= 自动：随当前配色主题的标题色变化；
        /// 一旦显式赋值即固定，不随主题变化。
        /// </summary>
        public Color ForeColor { get; set; } = Color.Empty;

        /// <summary>标题与图表区域之间的内边距。</summary>
        public int Padding { get; set; } = 8;
    }

    /// <summary>
    /// 图表区域，镜像 System.Windows.Forms.DataVisualization.Charting.ChartArea。
    /// 一个 HChart 可持有多个 HChartArea（多图表布局），每个 HChartArea 有独立的 X/Y 轴、坐标变换、Series 关联。
    /// </summary>
    public class HChartArea
    {
        /// <summary>图表区名称。</summary>
        public string Name { get; set; } = "ChartArea1";

        /// <summary>启用状态。false 时此 ChartArea 不渲染。</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>图表区相对位置（百分比：Left/Top/Width/Height 0-100）。多 ChartArea 布局时使用。</summary>
        public RectangleF Position { get; set; } = new RectangleF(0f, 0f, 100f, 100f);

        /// <summary>绘图区相对位置（相对 ChartArea 的内绘图区域百分比）。
        /// 默认 0,0,100,100：减去 Padding 后填满整个控件；HChart 再按轴刻度数字/标题的实测尺寸自动向内压缩。
        /// 仅当用户需要额外留白时显式设置（自动压缩与该值取较大边距）。</summary>
        public RectangleF InnerPlotPosition { get; set; } = new RectangleF(0f, 0f, 100f, 100f);

        /// <summary>背景颜色。</summary>
        public Color BackColor { get; set; } = Color.White;

        /// <summary>
        /// 全部 X 轴（同类对象一律列表，不做编号属性）：
        /// [0]=底部主轴，[1]=顶部副轴（默认禁用）；需要更多直接 Add。
        /// </summary>
        public List<HChartAxis> XAxes { get; } = new List<HChartAxis>
        {
            new HChartAxis { Name = "X" },
            new HChartAxis { Name = "X2", IsEnabled = false },
        };

        /// <summary>
        /// 全部 Y 轴（同类对象一律列表，不做编号属性）：
        /// [0]=左侧主轴，[1]=右1，[2]=左2，[3]=右2……左右交替向外排列，需要多少根就 Add 多少根（4 根、100 根皆可）；
        /// 副轴默认禁用，有系列绑定（Series.YAxisIndex）时 HChart 自动启用，无绑定自动收起。
        /// </summary>
        public List<HChartAxis> YAxes { get; } = new List<HChartAxis>
        {
            new HChartAxis { Name = "Y" },
        };

        /// <summary>主 X 轴（底部）= XAxes[0]。</summary>
        public HChartAxis AxisX => XAxes[0];

        /// <summary>主 Y 轴（左侧）= YAxes[0]。</summary>
        public HChartAxis AxisY => YAxes[0];
    }
}
