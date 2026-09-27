using System;
using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart
{
    /// <summary>
    /// 图表数据点，镜像 System.Windows.Forms.DataVisualization.Charting.DataPoint。
    /// 封装 X 值、Y 值（支持多 Y：High/Low/Open/Close 用于 Candle/Box 图）、样式、标签等。
    /// </summary>
    public class HChartDataPoint
    {
        /// <summary>数据点的 X 值（数值坐标）。对 Pie/Radar 等极坐标图，X 可为 0 表示等分布局。</summary>
        public double XValue { get; set; }

        /// <summary>主要 Y 值（第一维）。</summary>
        public double YValue { get; set; }

        /// <summary>附加 Y 值数组（用于 K 线、箱线图、气泡图等多值图表）。</summary>
        /// <remarks>
        /// 约定：YValues[0]=High, YValues[1]=Low, YValues[2]=Open, YValues[3]=Close（K 线）；
        /// 或 YValues[0]=Lower, YValues[1]=Upper, YValues[2]=Middle, YValues[3]=WhiskerLow, YValues[4]=WhiskerHigh（箱线图）。
        /// </remarks>
        public double[] YValues { get; set; } = new double[0];

        /// <summary>可选标签文字，用于 Column/Bar 顶部标注或 Pie 切片标签。</summary>
        public string Label { get; set; }

        /// <summary>可选 X 轴标签文字（字符串类别值），覆盖 XValue 的数值显示。</summary>
        public string AxisLabel { get; set; }

        /// <summary>可选工具提示文字（鼠标悬停时显示）。</summary>
        public string ToolTip { get; set; }

        /// <summary>数据点自定义颜色；null 时使用 Series.Color 或调色板。</summary>
        public Color? Color { get; set; }

        /// <summary>标记点样式（Line/Point/Spline 等有 MarkerType 的图表）。</summary>
        public HChartMarkerStyle MarkerStyle { get; set; } = HChartMarkerStyle.Circle;

        /// <summary>标记点大小（像素）。</summary>
        public int MarkerSize { get; set; } = 6;

        /// <summary>是否在图表中渲染此数据点。</summary>
        public bool IsEmpty { get; set; } = false;

        /// <summary>
        /// 箱线图原始样本值（每个学生/个体一个点）。非空时在箱体宽度内按蜂群布局散点显示：
        /// 同类别点沿 X 向左右错列、圆点互不重叠，且严格限制在箱体宽度内。不参与五数概括计算。
        /// </summary>
        public List<double> Samples { get; } = new List<double>();

        /// <summary>从 X+Y 构造。</summary>
        public HChartDataPoint(double x, double y)
        {
            XValue = x;
            YValue = y;
        }

        /// <summary>从 Y 构造（X 自动为索引）。</summary>
        public HChartDataPoint(double y) : this(0, y) { }

        /// <summary>从多 Y 值构造（K 线/箱线图）。</summary>
        public HChartDataPoint(double x, params double[] yValues)
        {
            XValue = x;
            if (yValues != null && yValues.Length > 0)
            {
                YValue = yValues[0];
                if (yValues.Length > 1)
                    YValues = new double[yValues.Length - 1];
                Array.Copy(yValues, 1, YValues, 0, yValues.Length - 1);
            }
        }
    }

    /// <summary>标记点样式枚举。</summary>
    public enum HChartMarkerStyle
    {
        /// <summary>无标记。</summary>
        None = 0,
        /// <summary>圆形。</summary>
        Circle = 1,
        /// <summary>正方形。</summary>
        Square = 2,
        /// <summary>菱形。</summary>
        Diamond = 3,
        /// <summary>三角形。</summary>
        Triangle = 4,
        /// <summary>十字。</summary>
        Cross = 5,
        /// <summary>五角星。</summary>
        Star = 6,
        /// <summary>五边形。</summary>
        Pentagon = 7,
        /// <summary>六边形。</summary>
        Hexagon = 8,
    }
}
