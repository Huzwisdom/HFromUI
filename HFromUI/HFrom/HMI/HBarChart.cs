using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    using HFromUI.HLangage;
    [HDescriptionLanguage("一个柱状图控件，支持显示柱状图信息，方便的设置数据源，然后显示出来，支持渐变色")]
public class HBarChart : UserControl
{
    private List<AuxiliaryLine> auxiliary_lines;

    /// <summary>value_max_left 字段。</summary>
    private int value_max_left = -1;

    /// <summary>value_min_left 字段。</summary>
    private int value_min_left = 0;

    /// <summary>value_Segment 字段。</summary>
    private int value_Segment = 5;

    private Dictionary<string, double[]> dict_datas;

    /// <summary>data_texts 字段。</summary>
    private string[] data_texts = null;

    /// <summary>data_colors 字段。</summary>
    private Color[] data_colors = null;

    /// <summary>brush_deep 字段。</summary>
    private Brush brush_deep = null;

    /// <summary>pen_normal 字段。</summary>
    private Pen pen_normal = null;

    /// <summary>pen_dash 字段。</summary>
    private Pen pen_dash = null;

    /// <summary>barBackColor 字段。</summary>
    private Color barBackColor = Color.DodgerBlue;

    /// <summary>useGradient 字段。</summary>
    private bool useGradient = false;

    /// <summary>color_deep 字段。</summary>
    private Color color_deep = Color.DimGray;

    /// <summary>color_dash 字段。</summary>
    private Color color_dash = Color.LightGray;

    /// <summary>value_IsRenderDashLine 字段。</summary>
    private bool value_IsRenderDashLine = true;

    /// <summary>isShowBarValue 字段。</summary>
    private bool isShowBarValue = true;

    /// <summary>showBarValueFormat 字段。</summary>
    private string showBarValueFormat = "{0}";

    /// <summary>value_title 字段。</summary>
    private string value_title = "";

    /// <summary>barPercentWidth 字段。</summary>
    private float barPercentWidth = 0.8f;

    /// <summary>isAoordinateRoundInt 字段。</summary>
    private bool isAoordinateRoundInt = false;

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("背景颜色")]
    [DefaultValue(typeof(Color), "Transparent")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override Color BackColor
    {
        get
        {
            return base.BackColor;
        }
        set
        {
            base.BackColor = value;
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置当前控件的文本")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Bindable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public override string Text
    {
        get
        {
            return base.Text;
        }
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的前景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("前景颜色")]
    [DefaultValue(typeof(Color), "Black")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override Color ForeColor
    {
        get
        {
            return base.ForeColor;
        }
        set
        {
            base.ForeColor = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("坐标与文字颜色")]
    [HDescriptionLanguage("获取或设置坐标轴及相关信息文本的颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "DimGray")]
    public virtual Color ColorLinesAndText
    {
        get
        {
            return color_deep;
        }
        set
        {
            color_deep = value;
            InitializationColor();
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("使用渐变")]
    [HDescriptionLanguage("获取或设置本条形图控件是否使用渐进色")]
    [Browsable(true)]
    [DefaultValue(false)]
    public virtual bool UseGradient
    {
        get
        {
            return useGradient;
        }
        set
        {
            useGradient = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("条背景颜色")]
    [HDescriptionLanguage("获取或设置柱状图的背景颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "DodgerBlue")]
    public virtual Color BarBackColor
    {
        get
        {
            return barBackColor;
        }
        set
        {
            barBackColor = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("虚线颜色")]
    [HDescriptionLanguage("获取或设置虚线的颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "LightGray")]
    public virtual Color ColorDashLines
    {
        get
        {
            return color_dash;
        }
        set
        {
            color_dash = value;
            pen_dash?.Dispose();
            pen_dash = new Pen(color_dash);
            pen_dash.DashStyle = DashStyle.Custom;
            pen_dash.DashPattern = new float[2]
            {
                5f,
                5f
            };
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制虚线")]
    [HDescriptionLanguage("获取或设置虚线是否进行显示")]
    [Browsable(true)]
    [DefaultValue(true)]
    public virtual bool IsRenderDashLine
    {
        get
        {
            return value_IsRenderDashLine;
        }
        set
        {
            value_IsRenderDashLine = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("数值分段")]
    [HDescriptionLanguage("获取或设置图形的纵轴分段数")]
    [Browsable(true)]
    [DefaultValue(5)]
    public virtual int ValueSegment
    {
        get
        {
            return value_Segment;
        }
        set
        {
            value_Segment = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("值最大左边距")]
    [HDescriptionLanguage("获取或设置图形的左纵坐标的最大值，该值必须大于最小值，该值为负数，最大值即为自动适配。")]
    [Browsable(true)]
    [DefaultValue(-1)]
    public virtual int ValueMaxLeft
    {
        get
        {
            return value_max_left;
        }
        set
        {
            value_max_left = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("值最小左边距")]
    [HDescriptionLanguage("获取或设置图形的左纵坐标的最小值，该值必须小于最大值")]
    [Browsable(true)]
    [DefaultValue(0)]
    public virtual int ValueMinLeft
    {
        get
        {
            return value_min_left;
        }
        set
        {
            if (value < value_max_left)
            {
                value_min_left = value;
                Invalidate();
            }
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题")]
    [HDescriptionLanguage("获取或设置图标的标题信息")]
    [Browsable(true)]
    [DefaultValue("")]
    public virtual string Title
    {
        get
        {
            return value_title;
        }
        set
        {
            value_title = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示条值")]
    [HDescriptionLanguage("获取或设置是否显示柱状图的值文本")]
    [Browsable(true)]
    [DefaultValue(true)]
    public virtual bool IsShowBarValue
    {
        get
        {
            return isShowBarValue;
        }
        set
        {
            isShowBarValue = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示条值格式")]
    [HDescriptionLanguage("获取或设置柱状图显示值的格式化信息，可以带单位")]
    [Browsable(true)]
    [DefaultValue("")]
    public virtual string ShowBarValueFormat
    {
        get
        {
            return showBarValueFormat;
        }
        set
        {
            showBarValueFormat = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("条百分比宽度")]
    [HDescriptionLanguage("获取或设置柱状图占平均宽度的百分比，默认0.8，即80%")]
    [Browsable(true)]
    [DefaultValue(0.8f)]
    public virtual float BarPercentWidth
    {
        get
        {
            return barPercentWidth;
        }
        set
        {
            if (value > 0f && value <= 1f)
            {
                barPercentWidth = value;
                Invalidate();
            }
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("坐标取整")]
    [HDescriptionLanguage("获取或设置纵轴是否强制使用整型。")]
    [Browsable(true)]
    [DefaultValue(false)]
    public virtual bool IsAoordinateRoundInt
    {
        get
        {
            return isAoordinateRoundInt;
        }
        set
        {
            isAoordinateRoundInt = value;
            Invalidate();
        }
    }

    public HBarChart()
    {
        InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            auxiliary_lines = new List<AuxiliaryLine>();
        pen_dash = new Pen(color_dash);
        pen_dash.DashStyle = DashStyle.Custom;
        pen_dash.DashPattern = new float[2]
        {
            5f,
            5f
        };
        InitializationColor();
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
        ForeColor = Color.Black;
        dict_datas = new Dictionary<string, double[]>();
        if (GetService(typeof(IDesignerHost)) != null || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            dict_datas.Add(HTranslation.GetContent("产量"), new double[5]
            {
                4.0,
                2.0,
                1.0,
                3.0,
                2.0
            });
        }
    }

    /// <summary>InitializationColor 方法。</summary>
    private void InitializationColor()
    {
        pen_normal?.Dispose();
        brush_deep?.Dispose();
        pen_normal = new Pen(color_deep);
        brush_deep = new SolidBrush(color_deep);
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(int[] data)
    {
        if (data == null)
        {
            dict_datas = null;
        }
        else
        {
            dict_datas = new Dictionary<string, double[]>
            {
                {
                    "",
                    FromHelper.TranlateArrayToDouble(data)
                }
            };
        }
        data_texts = null;
        data_colors = null;
        Invalidate();
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(double[] data)
    {
        if (data == null)
        {
            dict_datas = null;
        }
        else
        {
            dict_datas = new Dictionary<string, double[]>
            {
                {
                    "",
                    data
                }
            };
        }
        data_texts = null;
        data_colors = null;
        Invalidate();
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(int[] data, string[] texts)
    {
        if (data == null)
        {
            dict_datas = null;
        }
        else
        {
            dict_datas = new Dictionary<string, double[]>
            {
                {
                    "",
                    FromHelper.TranlateArrayToDouble(data)
                }
            };
        }
        data_texts = texts;
        data_colors = null;
        Invalidate();
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(double[] data, string[] texts)
    {
        if (data == null)
        {
            dict_datas = null;
        }
        else
        {
            dict_datas = new Dictionary<string, double[]>
            {
                {
                    "",
                    data
                }
            };
        }
        data_texts = texts;
        data_colors = null;
        Invalidate();
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(int[] data, string[] texts, Color[] colors)
    {
        if (data == null)
        {
            dict_datas = null;
        }
        else
        {
            dict_datas = new Dictionary<string, double[]>
            {
                {
                    "",
                    FromHelper.TranlateArrayToDouble(data)
                }
            };
        }
        data_texts = texts;
        data_colors = colors;
        Invalidate();
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(double[] data, string[] texts, Color[] colors)
    {
        if (data == null)
        {
            dict_datas = null;
        }
        else
        {
            dict_datas = new Dictionary<string, double[]>
            {
                {
                    "",
                    data
                }
            };
        }
        data_texts = texts;
        data_colors = colors;
        Invalidate();
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(Dictionary<string, double[]> datas, string[] texts, Color[] colors)
    {
        dict_datas = datas;
        data_texts = texts;
        data_colors = colors;
        Invalidate();
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        PaintControlsH(graphics, base.Width, base.Height);
        base.OnPaint(e);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        Dictionary<string, double[]> dictionary = dict_datas;
        int num = (dictionary != null && dictionary.Count > 0) ? FromHelper.CalculateMaxSectionFrom(dict_datas) : 5;
        if (value_max_left > 0)
        {
            num = value_max_left;
        }
        int num2 = (int)g.MeasureString(num.ToString(), Font).Width + 3;
        if (num2 < 50)
        {
            num2 = 50;
        }
        int num3 = 10;
        int num4 = 25;
        int num5 = 25;
        Point[] array = new Point[3]
        {
            new Point(num2, num4 - 8),
            new Point(num2, height - num5),
            new Point(width - num3, height - num5)
        };
        g.DrawLine(pen_normal, array[0], array[1]);
        g.DrawLine(pen_normal, array[1], array[2]);
        FromHelper.PaintTriangle(g, brush_deep, new Point(num2, num4 - 8), 4, GraphDirection.Upward);
        for (int i = 0; i < auxiliary_lines.Count; i++)
        {
            auxiliary_lines[i].PaintValue = FromHelper.ComputePaintLocationY(num, value_min_left, height - num4 - num5, auxiliary_lines[i].Value) + (float)num4;
        }
        for (int j = 0; j <= value_Segment; j++)
        {
            float num6 = (float)((double)j * (double)(num - value_min_left) / (double)value_Segment + (double)value_min_left);
            if (isAoordinateRoundInt)
            {
                num6 = (float)Math.Round(num6, 0);
            }
            float num7 = FromHelper.ComputePaintLocationY(num, value_min_left, height - num4 - num5, num6) + (float)num4;
            if (IsNeedPaintDash(num7))
            {
                g.DrawLine(pen_normal, num2 - 4, num7, num2 - 1, num7);
                g.DrawString(layoutRectangle: new RectangleF(0f, num7 - 19f, num2 - 4, 40f), s: num6.ToString(), font: Font, brush: brush_deep, format: FromHelper.StringFormatRight);
                if (j > 0 && value_IsRenderDashLine)
                {
                    g.DrawLine(pen_dash, num2, num7, width - num3, num7);
                }
            }
        }
        for (int k = 0; k < auxiliary_lines.Count; k++)
        {
            g.DrawLine(auxiliary_lines[k].GetPen(), num2 - 4, auxiliary_lines[k].PaintValue, num2 - 1, auxiliary_lines[k].PaintValue);
            g.DrawString(layoutRectangle: new RectangleF(0f, auxiliary_lines[k].PaintValue - 9f, num2 - 4, 20f), s: auxiliary_lines[k].Value.ToString(), font: Font, brush: auxiliary_lines[k].LineTextBrush, format: FromHelper.StringFormatRight);
            g.DrawLine(auxiliary_lines[k].GetPen(), num2, auxiliary_lines[k].PaintValue, width - num3, auxiliary_lines[k].PaintValue);
        }
        if (dict_datas == null || dict_datas.Count == 0)
        {
            return;
        }
        int num8 = dict_datas.Values.Select((double[] m) => m.Length).Max();
        float num9 = (float)(width - num2 - 1 - num3) * 1f / (float)num8;
        for (int l = 0; l < num8; l++)
        {
            int num10 = 0;
            foreach (KeyValuePair<string, double[]> dict_data in dict_datas)
            {
                float num11 = num9 * barPercentWidth;
                num11 = ((dict_datas.Count == 1) ? num11 : (num11 / (float)dict_datas.Count - (float)(3 * dict_datas.Count) + 3f));
                if (num11 < 1f)
                {
                    num11 = 1f;
                }
                float num12 = FromHelper.ComputePaintLocationY(num, value_min_left, height - num4 - num5, (float)dict_data.Value[l]) + (float)num4;
                RectangleF rect = new RectangleF((float)l * num9 + (1f - barPercentWidth) / 2f * num9 + (float)num2 + (float)num10 * (num9 * barPercentWidth / (float)dict_datas.Count), num12, num11, (float)(height - num5) - num12);
                Color color = barBackColor;
                if (data_colors != null)
                {
                    if (dict_datas.Count == 1)
                    {
                        if (l < data_colors.Length)
                        {
                            color = data_colors[l];
                        }
                    }
                    else if (num10 < data_colors.Length)
                    {
                        color = data_colors[num10];
                    }
                }
                if (useGradient)
                {
                    if (rect.Height > 0f)
                    {
                        using (LinearGradientBrush brush = new LinearGradientBrush(new PointF(rect.X, rect.Y + rect.Height), new PointF(rect.X, rect.Y), FromHelper.GetColorLight(color), color))
                        {
                            g.FillRectangle(brush, rect);
                        }
                    }
                }
                else
                {
                    using (Brush brush2 = new SolidBrush(color))
                    {
                        g.FillRectangle(brush2, rect);
                    }
                }
                if (isShowBarValue)
                {
                    using (Brush brush3 = new SolidBrush(ForeColor))
                    {
                        g.DrawString(layoutRectangle: new RectangleF(rect.X - 50f, num12 - (float)Font.Height - 2f, rect.Width + 100f, Font.Height + 2), s: string.Format(showBarValueFormat, dict_data.Value[l]), font: Font, brush: brush3, format: FromHelper.StringFormatCenter);
                    }
                }
                num10++;
            }
            if (data_texts != null && l < data_texts.Length)
            {
                g.DrawString(layoutRectangle: new RectangleF((float)l * num9 + (float)num2 - 50f, height - num5 - 1, num9 + 100f, num5 + 1), s: data_texts[l], font: Font, brush: brush_deep, format: FromHelper.StringFormatCenter);
            }
        }
        if (!string.IsNullOrEmpty(value_title))
        {
            g.DrawString(value_title, Font, brush_deep, new Rectangle(0, 0, width - 1, num4), FromHelper.StringFormatCenter);
            return;
        }
        int num13 = 0;
        int num14 = num2 + 100 * num13 + 10;
        foreach (KeyValuePair<string, double[]> dict_data2 in dict_datas)
        {
            if (!string.IsNullOrEmpty(dict_data2.Key))
            {
                Color color2 = barBackColor;
                if (data_colors != null && num13 < data_colors.Length)
                {
                    color2 = data_colors[num13];
                }
                using (Brush brush4 = new SolidBrush(color2))
                {
                    g.FillRectangle(brush4, new Rectangle(num14, 4, 20, num4 - 12));
                    num14 += 25;
                    g.DrawString(dict_data2.Key, Font, brush4, new Rectangle(num14, 3, 500, num4 - 8), FromHelper.StringFormatLeft);
                    num14 += (int)g.MeasureString(dict_data2.Key, Font).Width + 10;
                }
                num13++;
            }
        }
    }

    /// <summary>判断是否 NeedPaintDash。</summary>
    private bool IsNeedPaintDash(float paintValue)
    {
        if (dict_datas == null)
        {
            return true;
        }
        for (int i = 0; i < auxiliary_lines.Count; i++)
        {
            if (Math.Abs(auxiliary_lines[i].PaintValue - paintValue) < (float)Font.Height)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>calculatMaxTextWidth 方法。</summary>
    private float calculatMaxTextWidth(Graphics g)
    {
        string[] array = data_texts;
        if (array != null && array.Length != 0)
        {
            float num = 0f;
            for (int i = 0; i < data_texts.Length; i++)
            {
                float width = g.MeasureString(data_texts[i], Font).Width;
                if (num < width)
                {
                    num = width;
                }
            }
            return num;
        }
        return 1f;
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public void AddLeftAuxiliary(float value)
    {
        AddLeftAuxiliary(value, ColorLinesAndText);
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public void AddLeftAuxiliary(float value, Color lineColor)
    {
        AddLeftAuxiliary(value, lineColor, 1f, isDashLine: true);
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public void AddLeftAuxiliary(float value, Color lineColor, float lineThickness, bool isDashLine)
    {
        AddAuxiliary(value, lineColor, lineThickness, isDashLine, isLeft: true);
    }

    /// <summary>AddRightAuxiliary 方法。</summary>
    public void AddRightAuxiliary(float value)
    {
        AddRightAuxiliary(value, ColorLinesAndText);
    }

    /// <summary>AddRightAuxiliary 方法。</summary>
    public void AddRightAuxiliary(float value, Color lineColor)
    {
        AddRightAuxiliary(value, lineColor, 1f, isDashLine: true);
    }

    /// <summary>AddRightAuxiliary 方法。</summary>
    public void AddRightAuxiliary(float value, Color lineColor, float lineThickness, bool isDashLine)
    {
        AddAuxiliary(value, lineColor, lineThickness, isDashLine, isLeft: false);
    }

    /// <summary>AddAuxiliary 方法。</summary>
    private void AddAuxiliary(float value, Color lineColor, float lineThickness, bool isDashLine, bool isLeft)
    {
        auxiliary_lines.Add(new AuxiliaryLine
        {
            Value = value,
            LineColor = lineColor,
            PenDash = new Pen(lineColor)
            {
                DashStyle = DashStyle.Custom,
                DashPattern = new float[2]
                {
                    5f,
                    5f
                }
            },
            PenSolid = new Pen(lineColor),
            IsDashStyle = isDashLine,
            IsLeftFrame = isLeft,
            LineThickness = lineThickness,
            LineTextBrush = new SolidBrush(lineColor)
        });
        Invalidate();
    }

    /// <summary>RemoveAuxiliary 方法。</summary>
    public void RemoveAuxiliary(float value)
    {
        int num = 0;
        for (int num2 = auxiliary_lines.Count - 1; num2 >= 0; num2--)
        {
            if (auxiliary_lines[num2].Value == value)
            {
                auxiliary_lines[num2].Dispose();
                auxiliary_lines.RemoveAt(num2);
                num++;
            }
        }
        if (num > 0)
        {
            Invalidate();
        }
    }

    /// <summary>RemoveAllAuxiliary 方法。</summary>
    public void RemoveAllAuxiliary()
    {
        int count = auxiliary_lines.Count;
        auxiliary_lines.Clear();
        if (count > 0)
        {
            Invalidate();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>InitializeComponent 方法。</summary>
    private void InitializeComponent()
    {
        SuspendLayout();
        base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        BackColor = System.Drawing.Color.Transparent;
        ForeColor = System.Drawing.Color.Black;
        base.Name = "BarChartH";
        base.Size = new System.Drawing.Size(440, 264);
        ResumeLayout(false);
    }

}
}