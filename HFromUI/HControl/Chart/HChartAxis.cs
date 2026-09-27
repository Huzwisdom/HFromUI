using System;
using System.Drawing;

namespace HFromUI.HControl.Chart
{
    /// <summary>
    /// 图表坐标轴，镜像 System.Windows.Forms.DataVisualization.Charting.Axis。
    /// 控制刻度范围、间隔、主/次网格、标题、标签样式、启用/禁用等。
    /// </summary>
    public class HChartAxis
    {
        /// <summary>坐标轴名称/角色（用于在多 Axis 场景识别）。</summary>
        public string Name { get; set; }

        /// <summary>启用状态。false 时该轴及其网格/刻度全部不渲染。</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>最小值；null 时从数据自动计算。</summary>
        public double? Minimum { get; set; }

        /// <summary>最大值；null 时从数据自动计算。</summary>
        public double? Maximum { get; set; }

        /// <summary>主刻度间隔；null 时自适应。</summary>
        public double? Interval { get; set; }

        /// <summary>主刻度间隔类型（自动为 Number 时 Interval 是倍数；设为 Auto 自动计算）。</summary>
        public HChartIntervalType IntervalType { get; set; } = HChartIntervalType.Auto;

        /// <summary>坐标轴标题文字（轴名称）。</summary>
        public string Title { get; set; }

        /// <summary>坐标轴单位（如 ℃、%、m/s）；设置后标题自动渲染为“名称 (单位)”。</summary>
        public string Unit { get; set; }

        /// <summary>标题字体（轴名称；常规不加粗，字号与标尺 HUD 文字一致 8.5f）。</summary>
        public Font TitleFont { get; set; } = new Font("Segoe UI", 8.5f, FontStyle.Regular);

        /// <summary>标题颜色。</summary>
        public Color TitleForeColor { get; set; } = Color.Black;

        /// <summary>刻度标签字体。</summary>
        public Font LabelStyleFont { get; set; } = new Font("Segoe UI", 8f);

        /// <summary>刻度标签颜色。</summary>
        public Color LabelForeColor { get; set; } = Color.Black;

        /// <summary>数值刻度标签保留的最多小数位数（自动去尾零），默认 6；时间轴不受此设置影响。
        /// X/Y 每根轴独立可设。</summary>
        public int TickDecimalPlaces { get; set; } = 6;

        /// <summary>主网格线启用状态。</summary>
        public bool MajorGridEnabled { get; set; } = true;

        /// <summary>主网格线颜色。</summary>
        public Color MajorGridColor { get; set; } = Color.FromArgb(230, 230, 230);

        /// <summary>主网格线样式。</summary>
        public HChartLineDashStyle MajorGridDashStyle { get; set; } = HChartLineDashStyle.Dash;

        /// <summary>主刻度标记启用状态。</summary>
        public bool MajorTickMarkEnabled { get; set; } = true;

        /// <summary>主刻度颜色。</summary>
        public Color MajorTickMarkColor { get; set; } = Color.Gray;

        /// <summary>轴线条颜色。</summary>
        public Color LineColor { get; set; } = Color.Gray;

        /// <summary>轴线条宽度。</summary>
        public int LineWidth { get; set; } = 1;

        /// <summary>自动计算后的实际最小值（渲染阶段填充）。</summary>
        internal double ActualMinimum { get; set; }

        /// <summary>自动计算后的实际最大值（渲染阶段填充）。</summary>
        internal double ActualMaximum { get; set; }

        /// <summary>自动计算后的实际间隔（渲染阶段填充）。</summary>
        internal double ActualInterval { get; set; }

        /// <summary>是否是类别轴（X 轴显示字符串标签）。类别轴 Minimum=0, Maximum=N-1。</summary>
        public bool IsCategoryAxis { get; set; } = false;

        /// <summary>
        /// X 轴时间格式串：null/空 = X 值按数字显示（如序号）；非空时 X 值按 DateTime.FromOADate 解释、用该格式显示时间。
        /// 格式串中可写 "\n" 换行（如 "yyyy-MM-dd\nHH:mm:ss" 日期/时间分两行），刻度间隔自动按 1/2/5 的秒分时天取整。
        /// </summary>
        public string TimeFormat { get; set; }
    }

    /// <summary>轴间隔类型枚举。</summary>
    public enum HChartIntervalType
    {
        /// <summary>自动计算合适间隔。</summary>
        Auto = 0,
        /// <summary>数值间隔（Interval 直接是数值量）。</summary>
        Number = 1,
        /// <summary>时间间隔（Interval 单位为天）。</summary>
        Days = 2,
        /// <summary>小时。</summary>
        Hours = 3,
        /// <summary>分钟。</summary>
        Minutes = 4,
        /// <summary>秒。</summary>
        Seconds = 5,
        /// <summary>百分比（0-100 范围）。</summary>
        Percentage = 6,
    }
}
