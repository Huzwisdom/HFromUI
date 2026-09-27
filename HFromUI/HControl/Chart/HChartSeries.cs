using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart
{
    /// <summary>
    /// 数据点集合（Series.Points），镜像 System.Windows.Forms.DataVisualization.Charting.DataPointCollection。
    /// </summary>
    public class HChartDataPointCollection : List<HChartDataPoint>
    {
        /// <summary>所属的 Series（Add(y) 时用它的点数做隐式 X 索引）。</summary>
        private readonly HChartSeries _owner;

        /// <summary>关联的 Series（用于隐式分配 X 索引）。</summary>
        public HChartSeries Owner => _owner;

        /// <summary>构造：绑定所属 Series。</summary>
        public HChartDataPointCollection(HChartSeries owner) { _owner = owner; }

        /// <summary>添加 (X, Y) 数据点。</summary>
        public HChartDataPoint AddXY(double x, double y)
        {
            var p = new HChartDataPoint(x, y);
            Add(p);
            return p;
        }

        /// <summary>添加 Y 数据点（X 自动使用 Count 索引）。</summary>
        public HChartDataPoint Add(double y)
        {
            var p = new HChartDataPoint(Count, y);
            Add(p);
            return p;
        }

        /// <summary>添加多 Y 值数据点（K 线/箱线图：High, Low, Open, Close）。</summary>
        public HChartDataPoint AddXY(double x, params double[] yValues)
        {
            var p = new HChartDataPoint(x, yValues);
            Add(p);
            return p;
        }

        /// <summary>添加带类别字符串 X 的数据点。</summary>
        public HChartDataPoint AddXY(string category, double y)
        {
            var p = new HChartDataPoint(Count, y) { AxisLabel = category };
            Add(p);
            return p;
        }
    }

    /// <summary>
    /// 数据系列，镜像 System.Windows.Forms.DataVisualization.Charting.Series。
    /// 一个 Series 是一组同类图表类型的数据点，拥有独立的样式、坐标轴关联、数据绑定等。
    /// </summary>
    public class HChartSeries
    {
        /// <summary>Series 名称（用于 Legend 显示）。</summary>
        public string Name { get; set; } = "Series1";

        /// <summary>图例文本（默认同 Name）。</summary>
        public string LegendText { get; set; }

        /// <summary>图表类型。</summary>
        public HChartType ChartType { get; set; } = HChartType.Line;

        /// <summary>
        /// 图表类型的初始值（默认 null=尚未记录）。首次绘制时自动记录当时的 ChartType，
        /// 即用户代码设置好的"初始类型"；右键菜单"还原原图"恢复到它，菜单切类型不影响它。
        /// </summary>
        public HChartType? OriginalType { get; internal set; }

        /// <summary>由 HChart 在首次绘制前调用：只记录一次初始图表类型。</summary>
        internal void CaptureOriginalType()
        {
            if (OriginalType == null) OriginalType = ChartType;
        }

        /// <summary>系列颜色；null 时使用调色板自动分配。</summary>
        public Color? Color { get; set; }

        /// <summary>线条宽度（像素）。Line/Spline/Area/StepLine 适用。</summary>
        public int BorderWidth { get; set; } = 2;

        /// <summary>线条虚线样式。</summary>
        public HChartLineDashStyle BorderDashStyle { get; set; } = HChartLineDashStyle.Solid;

        /// <summary>数据填充颜色（Column/Bar/Area/Pie 适用）；null 时使用 Color 或调色板。</summary>
        public Color? BackColor { get; set; }

        /// <summary>填充透明度 0-255（255=完全不透明）。</summary>
        public int BackColorAlpha { get; set; } = 180;

        /// <summary>标记点样式。</summary>
        public HChartMarkerStyle MarkerStyle { get; set; } = HChartMarkerStyle.None;

        /// <summary>标记点大小。</summary>
        public int MarkerSize { get; set; } = 6;

        /// <summary>X 轴类型（Primary/Secondary）。</summary>
        public HChartAxisType XAxisType { get; set; } = HChartAxisType.PrimaryX;

        /// <summary>
        /// 绑定的 Y 轴序号：0=主 Y 轴（左1），1=右1，2=左2，3=右2，4=左3，5=右3……
        /// 偶数在左、奇数在右，自内向外交替排列，支持任意根数（4 根、100 根皆可）。
        /// </summary>
        public int YAxisIndex { get; set; } = 0;

        /// <summary>
        /// Y 轴类型（兼容旧 API）：与 YAxisIndex 双向映射
        /// （PrimaryY=0 左1 / SecondaryY=1 右1 / ThirdY=2 左2 / FourthY=3 右2）；
        /// 超过 4 根轴时请直接使用 YAxisIndex。
        /// </summary>
        public HChartAxisType YAxisType
        {
            get => YAxisIndex == 1 ? HChartAxisType.SecondaryY
                 : YAxisIndex == 2 ? HChartAxisType.ThirdY
                 : YAxisIndex >= 3 ? HChartAxisType.FourthY
                 : HChartAxisType.PrimaryY;
            set => YAxisIndex =
                value == HChartAxisType.SecondaryY ? 1 :
                value == HChartAxisType.ThirdY ? 2 :
                value == HChartAxisType.FourthY ? 3 : 0;
        }

        /// <summary>此 Series 所属的 ChartArea 名称。</summary>
        public string ChartArea { get; set; } = "ChartArea1";

        /// <summary>是否显示在 Legend 中。</summary>
        public bool IsVisibleInLegend { get; set; } = true;

        /// <summary>是否启用 Series。false 时渲染时跳过。</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>排序顺序（用于 Stacked 系列的覆盖顺序）。</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>关联的数据点集合。</summary>
        public HChartDataPointCollection Points { get; }

        /// <summary>可选 DataSource 绑定（List 或 DataTable）。绑定后 Points 在渲染时自动填充。</summary>
        public object DataSource { get; set; }

        /// <summary>可选 X 字段名（DataTable/DataGridView 场景）。</summary>
        public string XValueMember { get; set; }

        /// <summary>可选 Y 字段名（DataTable/DataGridView 场景），支持逗号分隔多个 Y 列。</summary>
        public string YValueMembers { get; set; }

        /// <summary>气泡图第三维（YValues[0]，控制圆面积）的语义名，如"库存"；悬停弹窗显示"大小（库存）= xx"。不设则显示"大小 = xx"。</summary>
        public string SizeValueName { get; set; }

        /// <summary>构造：默认名 Series1、折线图、自建空 Points 集合，图例文字同名。</summary>
        public HChartSeries() { Points = new HChartDataPointCollection(this); LegendText = Name; }
        /// <summary>构造：指定系列名（其余默认）。</summary>
        public HChartSeries(string name) : this() { Name = name; LegendText = name; }

        /// <summary>获取计算后的颜色（Color ?? 调色板自动分配的颜色）。
        /// dark=true 时自动分配色按深底提亮；显式指定的 Color 原样使用。</summary>
        internal Color ResolveColor(int paletteIndex, HChartPalette palette, bool dark = false)
        {
            if (Color.HasValue) return Color.Value;
            return ChartPalettes.GetColor(palette, paletteIndex, dark);
        }
    }

    /// <summary>
    /// 系列集合（Chart.Series），镜像 SeriesCollection。
    /// </summary>
    public class HChartSeriesCollection : List<HChartSeries>
    {
        /// <summary>所属的 ChartArea（新增系列默认归属它）。</summary>
        private readonly HChartArea _area;

        /// <summary>构造：绑定所属 ChartArea。</summary>
        public HChartSeriesCollection(HChartArea area) { _area = area; }

        /// <summary>按名称查找 Series。</summary>
        public HChartSeries this[string name]
        {
            get
            {
                foreach (var s in this) if (s.Name == name) return s;
                return null;
            }
        }

        /// <summary>添加一个空的 Series 并返回（方便链式 Add().AddXY()）。</summary>
        public HChartSeries Add(string name)
        {
            var s = new HChartSeries(name);
            Add(s);
            return s;
        }
    }

    /// <summary>线条虚线样式枚举。</summary>
    public enum HChartLineDashStyle
    {
        /// <summary>实线。</summary>
        Solid = 0,
        /// <summary>长虚线。</summary>
        Dash = 1,
        /// <summary>点线。</summary>
        Dot = 2,
        /// <summary>点划线。</summary>
        DashDot = 3,
        /// <summary>双点划线。</summary>
        DashDotDot = 4,
        /// <summary>不画线（仅填充/标记）。</summary>
        None = 5,
    }
}
