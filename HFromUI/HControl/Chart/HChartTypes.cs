using System;

namespace HFromUI.HControl.Chart
{
    /// <summary>图表类型枚举，镜像 System.Windows.Forms.DataVisualization.Charting.SeriesChartType。</summary>
    public enum HChartType
    {
        /// <summary>折线图。</summary>
        Line = 0,
        /// <summary>柱状图（纵向）。</summary>
        Column = 1,
        /// <summary>横向条形图。</summary>
        Bar = 2,
        /// <summary>散点图。</summary>
        Point = 3,
        /// <summary>饼图。</summary>
        Pie = 4,
        /// <summary>面积图。</summary>
        Area = 5,
        /// <summary>堆叠面积图。</summary>
        StackedArea = 6,
        /// <summary>堆叠柱状图。</summary>
        StackedColumn = 7,
        /// <summary>平滑折线图（贝塞尔曲线）。</summary>
        Spline = 8,
        /// <summary>阶梯线图。</summary>
        StepLine = 9,
        /// <summary>雷达图。</summary>
        Radar = 10,
        /// <summary>K 线蜡烛图。</summary>
        Candlestick = 11,
        /// <summary>箱线图。</summary>
        BoxPlot = 12,
        /// <summary>气泡图。</summary>
        Bubble = 13,
        /// <summary>漏斗图。</summary>
        Funnel = 14,
        /// <summary>玫瑰图（半径随值变化的饼图）。</summary>
        Doughnut = 15,
        /// <summary>标记点线图。</summary>
        PointAndFigure = 16,
    }

    /// <summary>图表配色方案枚举，镜像 ChartColorPalette。</summary>
    public enum HChartPalette
    {
        /// <summary>自定义颜色（通过 Series.Color 单独指定）。</summary>
        Custom = 0,
        /// <summary>默认 Microsoft 配色。</summary>
        Default = 1,
        /// <summary>暖色渐变（红橙黄）。</summary>
        Warm = 2,
        /// <summary>冷色渐变（青蓝紫）。</summary>
        Cool = 3,
        /// <summary>海洋色。</summary>
        Ocean = 4,
        /// <summary>自然/林地色。</summary>
        Earth = 5,
        /// <summary>日落色。</summary>
        SemiTransparent = 6,
    }

    /// <summary>停靠位置枚举（Legend/Title 的 Dock 属性）。</summary>
    public enum HChartDocking
    {
        /// <summary>停靠在顶部。</summary>
        Top = 0,
        /// <summary>停靠在底部。</summary>
        Bottom = 1,
        /// <summary>停靠在左侧。</summary>
        Left = 2,
        /// <summary>停靠在右侧。</summary>
        Right = 3,
    }

    /// <summary>对齐方式枚举（图例/标题在其停靠区域内的对齐）。</summary>
    public enum HChartAlignment
    {
        /// <summary>靠近起点（上/下停靠时为左，左/右停靠时为上）。</summary>
        Near = 0,
        /// <summary>居中。</summary>
        Center = 1,
        /// <summary>靠近终点（上/下停靠时为右，左/右停靠时为下）。</summary>
        Far = 2,
    }

    /// <summary>坐标轴类型（主轴或副轴）。</summary>
    public enum HChartAxisType
    {
        /// <summary>主 X 轴。</summary>
        PrimaryX = 0,
        /// <summary>主 Y 轴。</summary>
        PrimaryY = 1,
        /// <summary>副 X 轴（顶部）。</summary>
        SecondaryX = 2,
        /// <summary>副 Y 轴（右侧）。</summary>
        SecondaryY = 3,
        /// <summary>第三 Y 轴（左侧第二个，向外偏移）。</summary>
        ThirdY = 4,
        /// <summary>第四 Y 轴（右侧第二个，向外偏移）。</summary>
        FourthY = 5,
    }

    /// <summary>滚轮缩放方向模式（右键菜单切换）。</summary>
    public enum HChartWheelZoomMode
    {
        /// <summary>同时缩放 X / Y。</summary>
        Both = 0,
        /// <summary>只缩放 X 轴。</summary>
        XOnly = 1,
        /// <summary>只缩放 Y 轴。</summary>
        YOnly = 2,
    }
}
