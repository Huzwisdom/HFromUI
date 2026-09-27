using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    using HFromUI.HLangage;
    [HDescriptionLanguage("实时曲线控件，主要用来显示实时的曲线信息，不支持光标移动显示数据信息，支持简单的交互，显示隐藏曲线")]
public class HCurve : UserControl
{
    private const int value_count_max = 4096;

    /// <summary>value_Segment 字段。</summary>
    private int value_Segment = 5;

    /// <summary>value_IsAbscissaStrech 字段。</summary>
    private bool value_IsAbscissaStrech = false;

    /// <summary>value_StrechDataCountMax 字段。</summary>
    private int value_StrechDataCountMax = 300;

    /// <summary>value_IsRenderDashLine 字段。</summary>
    private bool value_IsRenderDashLine = true;

    /// <summary>textFormat 字段。</summary>
    private string textFormat = "HH:mm";

    /// <summary>value_IntervalAbscissaText 字段。</summary>
    private int value_IntervalAbscissaText = 100;

    /// <summary>random 字段。</summary>
    private Random random = null;

    /// <summary>value_title 字段。</summary>
    private string value_title = "";

    private ReferenceAxis referenceAxisLeft;

    private ReferenceAxis referenceAxisRight;

    /// <summary>leftRight 字段。</summary>
    private int leftRight = 50;

    /// <summary>upDown 字段。</summary>
    private int upDown = 25;

    private Dictionary<string, CurveItemH> data_list = null;

    /// <summary>data_text 字段。</summary>
    private string[] data_text = null;

    private List<AuxiliaryLine> auxiliary_lines;

    private List<AuxiliaryLable> auxiliary_Labels;

    private List<MarkTextH> MarkTextHs;

    /// <summary>font_size9 字段。</summary>
    private Font font_size9 = null;

    /// <summary>brush_deep 字段。</summary>
    private Brush brush_deep = null;

    /// <summary>pen_normal 字段。</summary>
    private Pen pen_normal = null;

    /// <summary>pen_dash 字段。</summary>
    private Pen pen_dash = null;

    /// <summary>color_normal 字段。</summary>
    private Color color_normal = Color.DeepPink;

    /// <summary>color_deep 字段。</summary>
    private Color color_deep = Color.DimGray;

    /// <summary>color_dash 字段。</summary>
    private Color color_dash = Color.Gray;

    /// <summary>color_mark_font 字段。</summary>
    private Color color_mark_font = Color.DodgerBlue;

    /// <summary>brush_mark_font 字段。</summary>
    private Brush brush_mark_font = Brushes.DodgerBlue;

    /// <summary>format_left 字段。</summary>
    private StringFormat format_left = null;

    /// <summary>format_right 字段。</summary>
    private StringFormat format_right = null;

    /// <summary>format_center 字段。</summary>
    private StringFormat format_center = null;

    /// <summary>isRenderRightCoordinate 字段。</summary>
    private bool isRenderRightCoordinate = true;

    /// <summary>curveNameWidth 字段。</summary>
    private int curveNameWidth = 100;

    /// <summary>pointsRadius 字段。</summary>
    private int pointsRadius = 0;

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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("参考轴左边距")]
    [HDescriptionLanguage("获取或设置图形的左轴的坐标轴信息")]
    [Browsable(true)]
    [TypeConverter(typeof(ReferenceAxisConverter))]
    /// <summary>ReferenceAxisLeft 成员。</summary>
    /// <summary>ReferenceAxisLeft 字段。</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ReferenceAxis ReferenceAxisLeft => referenceAxisLeft;

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("参考轴右边距")]
    [HDescriptionLanguage("获取或设置图形的右轴的坐标轴信息")]
    [Browsable(true)]
    [TypeConverter(typeof(ReferenceAxisConverter))]
    /// <summary>ReferenceAxisRight 成员。</summary>
    /// <summary>ReferenceAxisRight 字段。</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ReferenceAxis ReferenceAxisRight => referenceAxisRight;

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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("横坐标拉伸")]
    [HDescriptionLanguage("获取或设置所有的数据是否强制在一个界面里显示")]
    [Browsable(true)]
    [DefaultValue(false)]
    public virtual bool IsAbscissaStrech
    {
        get
        {
            return value_IsAbscissaStrech;
        }
        set
        {
            value_IsAbscissaStrech = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("拉伸最大数据量")]
    [HDescriptionLanguage("获取或设置拉伸模式下的最大数据量")]
    [Browsable(true)]
    [DefaultValue(300)]
    public virtual int StrechDataCountMax
    {
        get
        {
            return value_StrechDataCountMax;
        }
        set
        {
            value_StrechDataCountMax = value;
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("虚线颜色")]
    [HDescriptionLanguage("获取或设置虚线的颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "Gray")]
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("横坐标文字间隔")]
    [HDescriptionLanguage("获取或设置纵向虚线的分隔情况，单位为多少个数据")]
    [Browsable(true)]
    [DefaultValue(100)]
    public virtual int IntervalAbscissaText
    {
        get
        {
            return value_IntervalAbscissaText;
        }
        set
        {
            value_IntervalAbscissaText = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本添加格式")]
    [HDescriptionLanguage("获取或设置实时数据新增时文本相对应于时间的格式化字符串，默认HH:mm")]
    [Browsable(true)]
    [DefaultValue("HH:mm")]
    public virtual string TextAddFormat
    {
        get
        {
            return textFormat;
        }
        set
        {
            textFormat = value;
            Invalidate();
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制右侧坐标")]
    [HDescriptionLanguage("获取或设置是否显示右侧的坐标系信息")]
    [Browsable(true)]
    [DefaultValue(true)]
    public virtual bool IsRenderRightCoordinate
    {
        get
        {
            return isRenderRightCoordinate;
        }
        set
        {
            isRenderRightCoordinate = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置曲线名称的布局宽度，默认为150")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("曲线名称宽度")]
    [DefaultValue(100)]
    public virtual int CurveNameWidth
    {
        get
        {
            return curveNameWidth;
        }
        set
        {
            if (value > 10)
            {
                curveNameWidth = value;
            }
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置所有的数据点显示的半径大小，默认是0，不显示数据点")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("数据点半径")]
    [DefaultValue(0)]
    public virtual int PointsRadius
    {
        get
        {
            return pointsRadius;
        }
        set
        {
            pointsRadius = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置Y轴刻度文本的字体信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("字体校准")]
    public virtual Font FontCalibration
    {
        get
        {
            return font_size9;
        }
        set
        {
            font_size9 = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置左右两侧的坐标轴的宽度，以像素为单位")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左边距右边距宽度")]
    [DefaultValue(50)]
    public int LeftRightWidth
    {
        get
        {
            return leftRight;
        }
        set
        {
            leftRight = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置上下两侧的空白的宽度，以像素为单位")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("上下高度")]
    [DefaultValue(25)]
    public int UpDownHeight
    {
        get
        {
            return upDown;
        }
        set
        {
            upDown = value;
            Invalidate();
        }
    }

    public HCurve()
    {
        InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            random = new Random();
        data_list = new Dictionary<string, CurveItemH>();
        auxiliary_lines = new List<AuxiliaryLine>();
        MarkTextHs = new List<MarkTextH>();
        auxiliary_Labels = new List<AuxiliaryLable>();
        format_left = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Near
        };
        format_right = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Far
        };
        format_center = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };
        font_size9 = new Font("微软雅黑", 9f);
        InitializationColor();
        pen_dash = new Pen(color_deep);
        pen_dash.DashStyle = DashStyle.Custom;
        pen_dash.DashPattern = new float[2]
        {
            5f,
            5f
        };
        referenceAxisLeft = new ReferenceAxis(this);
        referenceAxisRight = new ReferenceAxis(this);
        referenceAxisLeft.Color = Color.DimGray;
        referenceAxisRight.Color = Color.DimGray;
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
    }

    /// <summary>InitializationColor 方法。</summary>
    private void InitializationColor()
    {
        pen_normal?.Dispose();
        brush_deep?.Dispose();
        pen_normal = new Pen(color_deep);
        brush_deep = new SolidBrush(color_deep);
    }

    /// <summary>设置 curveText。</summary>
    public void SetCurveText(string[] HDescriptionLanguages)
    {
        data_text = HDescriptionLanguages;
        Invalidate();
    }

    /// <summary>设置 leftCurve。</summary>
    public void SetLeftCurve(string key, float[] data)
    {
        SetLeftCurve(key, data, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
    }

    /// <summary>设置 leftCurve。</summary>
    public void SetLeftCurve(string key, float[] data, Color lineColor)
    {
        SetCurve(key, 0, data, lineColor, 1f, CurveStyle.LineSegment);
    }

    /// <summary>设置 leftCurve。</summary>
    public void SetLeftCurve(string key, float[] data, Color lineColor, CurveStyle style)
    {
        SetCurve(key, 0, data, lineColor, 1f, style);
    }

    /// <summary>设置 rightCurve。</summary>
    public void SetRightCurve(string key, float[] data)
    {
        SetRightCurve(key, data, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
    }

    /// <summary>设置 rightCurve。</summary>
    public void SetRightCurve(string key, float[] data, Color lineColor)
    {
        SetCurve(key, 1, data, lineColor, 1f, CurveStyle.LineSegment);
    }

    /// <summary>设置 rightCurve。</summary>
    public void SetRightCurve(string key, float[] data, Color lineColor, CurveStyle style)
    {
        SetCurve(key, 1, data, lineColor, 1f, style);
    }

    /// <summary>设置 curve。</summary>
    public void SetCurve(string key, int referenceAxis, float[] data, Color lineColor, float thickness, CurveStyle style)
    {
        if (data_list.ContainsKey(key))
        {
            if (data == null)
            {
                data = new float[0];
            }
            data_list[key].Data = data;
            if (data_text != null && data_text.Length != data.Length)
            {
                data_text = new string[data.Length];
            }
        }
        else
        {
            if (data == null)
            {
                data = new float[0];
            }
            data_list.Add(key, new CurveItemH
            {
                Data = data,
                MarkText = new string[data.Length],
                LineThickness = thickness,
                LineColor = lineColor,
                ReferenceAxisIndex = referenceAxis,
                Style = style
            });
            if (data_text == null || data_text.Length != data.Length)
            {
                data_text = new string[data.Length];
            }
        }
        Invalidate();
    }

    /// <summary>RemoveCurve 方法。</summary>
    public void RemoveCurve(string key)
    {
        if (data_list.ContainsKey(key))
        {
            data_list.Remove(key);
        }
        if (data_list.Count == 0)
        {
            data_text = new string[0];
        }
        Invalidate();
    }

    /// <summary>RemoveAllCurve 方法。</summary>
    public void RemoveAllCurve()
    {
        int count = data_list.Count;
        data_list.Clear();
        if (data_list.Count == 0)
        {
            data_text = new string[0];
        }
        if (count > 0)
        {
            Invalidate();
        }
    }

    /// <summary>RemoveAllCurveData 方法。</summary>
    public void RemoveAllCurveData()
    {
        int count = data_list.Count;
        foreach (KeyValuePair<string, CurveItemH> item in data_list)
        {
            item.Value.Data = new float[0];
            item.Value.MarkText = new string[0];
        }
        data_text = new string[0];
        if (count > 0)
        {
            Invalidate();
        }
    }

    /// <summary>获取 curveItem。</summary>
    public CurveItemH GetCurveItem(string key)
    {
        if (data_list.ContainsKey(key))
        {
            return data_list[key];
        }
        return null;
    }

    /// <summary>SaveToBitmap 方法。</summary>
    public Bitmap SaveToBitmap()
    {
        return SaveToBitmap(base.Width, base.Height);
    }

    /// <summary>SaveToBitmap 方法。</summary>
    public Bitmap SaveToBitmap(int width, int height)
    {
        Bitmap bitmap = new Bitmap(width, height);
        Graphics graphics = Graphics.FromImage(bitmap);
        OnPaint(new PaintEventArgs(graphics, new Rectangle(0, 0, width, height)));
        return bitmap;
    }

    /// <summary>AddCurveData 方法。</summary>
    private void AddCurveData(string key, float[] values, string[] markTexts, bool isUpdateUI)
    {
        if ((values != null && values.Length < 1) || !data_list.ContainsKey(key))
        {
            return;
        }
        CurveItemH CurveItemH = data_list[key];
        if (CurveItemH.Data != null)
        {
            if (value_IsAbscissaStrech)
            {
                FromHelper.AddArrayData(ref CurveItemH.Data, values, value_StrechDataCountMax);
                FromHelper.AddArrayData(ref CurveItemH.MarkText, markTexts, value_StrechDataCountMax);
            }
            else
            {
                FromHelper.AddArrayData(ref CurveItemH.Data, values, 4096);
                FromHelper.AddArrayData(ref CurveItemH.MarkText, markTexts, 4096);
            }
            if (isUpdateUI)
            {
                Invalidate();
            }
        }
    }

    /// <summary>AddCurveTime 方法。</summary>
    private void AddCurveTime(int count)
    {
        AddCurveTime(count, DateTime.Now.ToString(textFormat));
    }

    /// <summary>AddCurveTime 方法。</summary>
    private void AddCurveTime(int count, string text)
    {
        if (data_text != null)
        {
            string[] array = new string[count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = text;
            }
            if (value_IsAbscissaStrech)
            {
                FromHelper.AddArrayData(ref data_text, array, value_StrechDataCountMax);
            }
            else
            {
                FromHelper.AddArrayData(ref data_text, array, 4096);
            }
        }
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string key, float value)
    {
        AddCurveData(key, new float[1]
        {
            value
        });
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string key, float value, string markText)
    {
        AddCurveData(key, new float[1]
        {
            value
        }, new string[1]
        {
            markText
        });
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string key, float[] values)
    {
        AddCurveData(key, values, null);
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string key, float[] values, string[] markTexts)
    {
        if (markTexts == null)
        {
            markTexts = new string[values.Length];
        }
        AddCurveData(key, values, markTexts, isUpdateUI: false);
        if (values != null && values.Length != 0)
        {
            AddCurveTime(values.Length);
        }
        Invalidate();
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string[] keys, float[] values)
    {
        AddCurveData(keys, values, null);
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string axisText, string[] keys, float[] values)
    {
        AddCurveData(axisText, keys, values, null);
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string[] keys, float[] values, string[] markTexts)
    {
        if (keys == null)
        {
            throw new ArgumentNullException(HTranslation.GetContent("键集合不能为空"));
        }
        if (values == null)
        {
            throw new ArgumentNullException(HTranslation.GetContent("值集合不能为空"));
        }
        if (markTexts == null)
        {
            markTexts = new string[keys.Length];
        }
        if (keys.Length != values.Length)
        {
            throw new Exception(HTranslation.GetContent("两个参数的数组长度不一致。"));
        }
        if (keys.Length != markTexts.Length)
        {
            throw new Exception(HTranslation.GetContent("两个参数的数组长度不一致。"));
        }
        for (int i = 0; i < keys.Length; i++)
        {
            AddCurveData(keys[i], new float[1]
            {
                values[i]
            }, new string[1]
            {
                markTexts[i]
            }, isUpdateUI: false);
        }
        AddCurveTime(1);
        Invalidate();
    }

    /// <summary>AddCurveData 方法。</summary>
    public void AddCurveData(string axisText, string[] keys, float[] values, string[] markTexts)
    {
        if (keys == null)
        {
            throw new ArgumentNullException(HTranslation.GetContent("键集合不能为空"));
        }
        if (values == null)
        {
            throw new ArgumentNullException(HTranslation.GetContent("值集合不能为空"));
        }
        if (markTexts == null)
        {
            markTexts = new string[keys.Length];
        }
        if (keys.Length != values.Length)
        {
            throw new Exception(HTranslation.GetContent("两个参数的数组长度不一致。"));
        }
        if (keys.Length != markTexts.Length)
        {
            throw new Exception(HTranslation.GetContent("两个参数的数组长度不一致。"));
        }
        for (int i = 0; i < keys.Length; i++)
        {
            AddCurveData(keys[i], new float[1]
            {
                values[i]
            }, new string[1]
            {
                markTexts[i]
            }, isUpdateUI: false);
        }
        AddCurveTime(1, axisText);
        Invalidate();
    }

    /// <summary>设置 curveVisible。</summary>
    public void SetCurveVisible(string key, bool visible)
    {
        if (data_list.ContainsKey(key))
        {
            CurveItemH CurveItemH = data_list[key];
            CurveItemH.Visible = visible;
            Invalidate();
        }
    }

    /// <summary>设置 curveVisible。</summary>
    public void SetCurveVisible(string[] keys, bool visible)
    {
        foreach (string key in keys)
        {
            if (data_list.ContainsKey(key))
            {
                CurveItemH CurveItemH = data_list[key];
                CurveItemH.Visible = visible;
            }
        }
        Invalidate();
    }

    public Dictionary<string, CurveItemH> GetAllCurve()
    {
        return data_list;
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public AuxiliaryLine AddLeftAuxiliary(float value)
    {
        return AddLeftAuxiliary(value, ColorLinesAndText);
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public AuxiliaryLine AddLeftAuxiliary(float value, Color lineColor)
    {
        return AddLeftAuxiliary(value, lineColor, 1f, isDashLine: true);
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public AuxiliaryLine AddLeftAuxiliary(float value, Color lineColor, float lineThickness, bool isDashLine)
    {
        return AddAuxiliary(value, lineColor, lineThickness, isDashLine, isLeft: true);
    }

    /// <summary>AddRightAuxiliary 方法。</summary>
    public AuxiliaryLine AddRightAuxiliary(float value)
    {
        return AddRightAuxiliary(value, ColorLinesAndText);
    }

    /// <summary>AddRightAuxiliary 方法。</summary>
    public AuxiliaryLine AddRightAuxiliary(float value, Color lineColor)
    {
        return AddRightAuxiliary(value, lineColor, 1f, isDashLine: true);
    }

    /// <summary>AddRightAuxiliary 方法。</summary>
    public AuxiliaryLine AddRightAuxiliary(float value, Color lineColor, float lineThickness, bool isDashLine)
    {
        return AddAuxiliary(value, lineColor, lineThickness, isDashLine, isLeft: false);
    }

    /// <summary>AddAuxiliary 方法。</summary>
    private AuxiliaryLine AddAuxiliary(float value, Color lineColor, float lineThickness, bool isDashLine, bool isLeft)
    {
        AuxiliaryLine auxiliaryLine = new AuxiliaryLine();
        auxiliaryLine.Value = value;
        auxiliaryLine.LineColor = lineColor;
        auxiliaryLine.PenDash = new Pen(lineColor)
        {
            DashStyle = DashStyle.Custom,
            DashPattern = new float[2]
            {
                5f,
                5f
            }
        };
        auxiliaryLine.PenSolid = new Pen(lineColor);
        auxiliaryLine.IsDashStyle = isDashLine;
        auxiliaryLine.IsLeftFrame = isLeft;
        auxiliaryLine.LineThickness = lineThickness;
        auxiliaryLine.LineTextBrush = new SolidBrush(lineColor);
        AuxiliaryLine auxiliaryLine2 = auxiliaryLine;
        auxiliary_lines.Add(auxiliaryLine2);
        Invalidate();
        return auxiliaryLine2;
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

    /// <summary>RemoveAuxiliary 方法。</summary>
    public void RemoveAuxiliary(AuxiliaryLine auxiliary)
    {
        if (auxiliary_lines.Remove(auxiliary))
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

    /// <summary>AddAuxiliaryLabel 方法。</summary>
    public void AddAuxiliaryLabel(AuxiliaryLable auxiliaryLable)
    {
        auxiliary_Labels.Add(auxiliaryLable);
    }

    /// <summary>RemoveAuxiliaryLable 方法。</summary>
    public void RemoveAuxiliaryLable(AuxiliaryLable auxiliaryLable)
    {
        if (auxiliary_Labels.Remove(auxiliaryLable))
        {
            Invalidate();
        }
    }

    /// <summary>RemoveAllAuxiliaryLable 方法。</summary>
    public void RemoveAllAuxiliaryLable()
    {
        int count = auxiliary_Labels.Count;
        auxiliary_Labels.Clear();
        if (count > 0)
        {
            Invalidate();
        }
    }

    /// <summary>AddMarkText 方法。</summary>
    public void AddMarkText(MarkTextH markText)
    {
        MarkTextHs.Add(markText);
        if (data_list.Count > 0)
        {
            Invalidate();
        }
    }

    /// <summary>RemoveMarkText 方法。</summary>
    public void RemoveMarkText(MarkTextH markText)
    {
        MarkTextHs.Remove(markText);
        if (data_list.Count > 0)
        {
            Invalidate();
        }
    }

    /// <summary>RemoveAllMarkText 方法。</summary>
    public void RemoveAllMarkText()
    {
        MarkTextHs.Clear();
        if (data_list.Count > 0)
        {
            Invalidate();
        }
    }

    /// <summary>响应 MouseMove 事件。</summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool flag = false;
        foreach (KeyValuePair<string, CurveItemH> item in data_list)
        {
            if (item.Value.TitleRegion.Contains(e.Location))
            {
                flag = true;
                break;
            }
        }
        Cursor = (flag ? Cursors.Hand : Cursors.Arrow);
    }

    /// <summary>响应 MouseDown 事件。</summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        foreach (KeyValuePair<string, CurveItemH> item in data_list)
        {
            if (item.Value.TitleRegion.Contains(e.Location))
            {
                item.Value.LineRenderVisiable = !item.Value.LineRenderVisiable;
                Invalidate();
                break;
            }
        }
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
        if (BackColor != Color.Transparent)
        {
            g.Clear(BackColor);
        }
        int num = height - upDown - upDown;
        if (width < 120 || height < 60)
        {
            return;
        }
        Point[] array = new Point[4]
        {
            new Point(leftRight - 1, upDown - 8),
            new Point(leftRight - 1, height - upDown),
            new Point(width - leftRight, height - upDown),
            new Point(width - leftRight, upDown - 8)
        };
        g.DrawLine(pen_normal, array[1], array[2]);
        g.DrawLine(referenceAxisLeft.GetPen(), array[0], array[1]);
        if (isRenderRightCoordinate)
        {
            g.DrawLine(referenceAxisRight.GetPen(), array[2], array[3]);
        }
        if (!string.IsNullOrEmpty(value_title))
        {
            g.DrawString(value_title, font_size9, brush_deep, new Rectangle(0, 0, width - 1, 20), format_center);
        }
        else if (data_list.Count > 0)
        {
            float num2 = leftRight + 10;
            foreach (KeyValuePair<string, CurveItemH> item in data_list)
            {
                if (item.Value.Visible)
                {
                    Pen pen = item.Value.LineRenderVisiable ? new Pen(item.Value.LineColor) : new Pen(Color.FromArgb(80, item.Value.LineColor));
                    g.DrawLine(pen, num2, upDown / 2 - 1, num2 + 30f, upDown / 2 - 1);
                    g.DrawEllipse(pen, num2 + 8f, upDown / 2 - 8, 14f, 14f);
                    pen.Dispose();
                    SolidBrush solidBrush = item.Value.LineRenderVisiable ? new SolidBrush(item.Value.LineColor) : new SolidBrush(Color.FromArgb(80, item.Value.LineColor));
                    g.DrawString(item.Key, Font, solidBrush, new RectangleF(num2 + 35f, upDown / 2 - 9, 120f, 18f), format_left);
                    item.Value.TitleRegion = new RectangleF(num2, 2f, 60f, 18f);
                    solidBrush.Dispose();
                    num2 += (float)curveNameWidth;
                }
            }
        }
        for (int i = 0; i < auxiliary_Labels.Count; i++)
        {
            if (!string.IsNullOrEmpty(auxiliary_Labels[i].Text))
            {
                int num3 = (auxiliary_Labels[i].LocationX > 1f) ? ((int)auxiliary_Labels[i].LocationX) : ((int)(auxiliary_Labels[i].LocationX * (float)width));
                int num4 = (int)g.MeasureString(auxiliary_Labels[i].Text, Font).Width + 3;
                Point[] points = new Point[6]
                {
                    new Point(num3, 11),
                    new Point(num3 + 10, 20),
                    new Point(num3 + num4 + 10, 20),
                    new Point(num3 + num4 + 10, 0),
                    new Point(num3 + 10, 0),
                    new Point(num3, 11)
                };
                g.FillPolygon(auxiliary_Labels[i].TextBack, points);
                g.DrawString(auxiliary_Labels[i].Text, Font, auxiliary_Labels[i].TextBrush, new Rectangle(num3 + 7, 0, num4 + 3, 20), format_center);
            }
        }
        FromHelper.PaintTriangle(g, referenceAxisLeft.Brush, new Point(leftRight - 1, upDown - 8), 4, GraphDirection.Upward);
        if (isRenderRightCoordinate)
        {
            FromHelper.PaintTriangle(g, referenceAxisRight.Brush, new Point(width - leftRight, upDown - 8), 4, GraphDirection.Upward);
        }
        if (!string.IsNullOrEmpty(referenceAxisLeft.Unit))
        {
            g.DrawString(referenceAxisLeft.Unit, font_size9, referenceAxisLeft.Brush, new RectangleF(0f, 0f, leftRight - 4, upDown - 5), format_right);
        }
        if (isRenderRightCoordinate && !string.IsNullOrEmpty(referenceAxisRight.Unit))
        {
            g.DrawString(referenceAxisRight.Unit, font_size9, referenceAxisRight.Brush, new RectangleF(width - leftRight + 4, 0f, leftRight - 4, upDown - 5), format_left);
        }
        for (int j = 0; j < auxiliary_lines.Count; j++)
        {
            if (auxiliary_lines[j].IsLeftFrame)
            {
                auxiliary_lines[j].PaintValue = FromHelper.ComputePaintLocationY(referenceAxisLeft, num, auxiliary_lines[j].Value) + (float)upDown;
            }
            else
            {
                auxiliary_lines[j].PaintValue = FromHelper.ComputePaintLocationY(referenceAxisRight, num, auxiliary_lines[j].Value) + (float)upDown;
            }
        }
        for (int k = 0; k <= value_Segment; k++)
        {
            float value = (float)((double)k * (double)(referenceAxisLeft.Max - referenceAxisLeft.Min) / (double)value_Segment + (double)referenceAxisLeft.Min);
            float num5 = FromHelper.ComputePaintLocationY(referenceAxisLeft.Max, referenceAxisLeft.Min, num, value) + (float)upDown;
            if (IsNeedPaintDash(num5))
            {
                g.DrawLine(referenceAxisLeft.GetPen(), leftRight - 4, num5, leftRight - 1, num5);
                RectangleF layoutRectangle = new RectangleF(0f, num5 - 9f, leftRight - 4, 20f);
                g.DrawString(FromHelper.GetFormatString(referenceAxisLeft.Format, value), font_size9, referenceAxisLeft.Brush, layoutRectangle, format_right);
                if (isRenderRightCoordinate)
                {
                    float value2 = (float)((double)k * (double)(referenceAxisRight.Max - referenceAxisRight.Min) / (double)value_Segment + (double)referenceAxisRight.Min);
                    g.DrawLine(referenceAxisRight.GetPen(), width - leftRight + 1, num5, width - leftRight + 4, num5);
                    layoutRectangle.Location = new PointF(width - leftRight + 4, num5 - 9f);
                    g.DrawString(FromHelper.GetFormatString(referenceAxisRight.Format, value2), font_size9, referenceAxisRight.Brush, layoutRectangle, format_left);
                }
                if (k > 0 && value_IsRenderDashLine)
                {
                    g.DrawLine(pen_dash, leftRight, num5, width - leftRight, num5);
                }
            }
        }
        if (value_IsRenderDashLine)
        {
            if (value_IsAbscissaStrech)
            {
                float num6 = (float)(width - leftRight * 2) * 1f / (float)(value_StrechDataCountMax - 1);
                int num7 = CalculateDataCountByOffect(num6);
                for (int l = 0; l < value_StrechDataCountMax; l += num7)
                {
                    if (l > 0 && l < value_StrechDataCountMax - 1)
                    {
                        g.DrawLine(pen_dash, (float)l * num6 + (float)leftRight, upDown, (float)l * num6 + (float)leftRight, height - upDown - 1);
                    }
                    if (data_text != null && l < data_text.Length && (float)l * num6 + (float)leftRight < (float)(data_text.Length - 1) * num6 + (float)leftRight - 40f)
                    {
                        g.DrawString(layoutRectangle: new Rectangle((int)((float)l * num6), height - upDown + 1, leftRight * 2, upDown), s: data_text[l], font: font_size9, brush: brush_deep, format: format_center);
                    }
                }
                string[] array2 = data_text;
                if (array2 != null && array2.Length > 1)
                {
                    if (data_text.Length < value_StrechDataCountMax)
                    {
                        g.DrawLine(pen_dash, (float)(data_text.Length - 1) * num6 + (float)leftRight, upDown, (float)(data_text.Length - 1) * num6 + (float)leftRight, height - upDown - 1);
                    }
                    g.DrawString(layoutRectangle: new Rectangle((int)((float)(data_text.Length - 1) * num6 + (float)leftRight) - leftRight, height - upDown + 1, leftRight * 2, upDown), s: data_text[data_text.Length - 1], font: font_size9, brush: brush_deep, format: format_center);
                }
            }
            else
            {
                int num8 = width - 2 * leftRight + 1;
                if (value_IntervalAbscissaText > 0)
                {
                    for (int m = leftRight; m < width - leftRight; m += value_IntervalAbscissaText)
                    {
                        if (m != leftRight)
                        {
                            g.DrawLine(pen_dash, m, upDown, m, height - upDown - 1);
                        }
                        if (data_text == null)
                        {
                            continue;
                        }
                        int num9 = (num8 > data_text.Length) ? data_text.Length : num8;
                        if (m - leftRight < data_text.Length && num9 - (m - leftRight) > 40)
                        {
                            if (data_text.Length <= num8)
                            {
                                g.DrawString(layoutRectangle: new Rectangle(m - leftRight, height - upDown + 1, leftRight * 2, upDown), s: data_text[m - leftRight], font: font_size9, brush: brush_deep, format: format_center);
                            }
                            else
                            {
                                g.DrawString(layoutRectangle: new Rectangle(m - leftRight, height - upDown + 1, leftRight * 2, upDown), s: data_text[m - leftRight + data_text.Length - num8], font: font_size9, brush: brush_deep, format: format_center);
                            }
                        }
                    }
                }
                string[] array3 = data_text;
                if (array3 != null && array3.Length > 1)
                {
                    if (data_text.Length >= num8)
                    {
                        g.DrawString(layoutRectangle: new Rectangle(width - leftRight - leftRight, height - upDown + 1, leftRight * 2, upDown), s: data_text[data_text.Length - 1], font: font_size9, brush: brush_deep, format: format_center);
                    }
                    else
                    {
                        g.DrawLine(pen_dash, data_text.Length + leftRight - 1, upDown, data_text.Length + leftRight - 1, height - upDown - 1);
                        g.DrawString(layoutRectangle: new Rectangle(data_text.Length + leftRight - 1 - leftRight, height - upDown + 1, leftRight * 2, upDown), s: data_text[data_text.Length - 1], font: font_size9, brush: brush_deep, format: format_center);
                    }
                }
            }
        }
        for (int n = 0; n < auxiliary_lines.Count; n++)
        {
            if (auxiliary_lines[n].IsLeftFrame)
            {
                g.DrawLine(auxiliary_lines[n].GetPen(), leftRight - 4, auxiliary_lines[n].PaintValue, leftRight - 1, auxiliary_lines[n].PaintValue);
                g.DrawString(layoutRectangle: new RectangleF(0f, auxiliary_lines[n].PaintValue - 9f, leftRight - 4, 20f), s: FromHelper.GetFormatString(referenceAxisLeft.Format, auxiliary_lines[n].Value), font: font_size9, brush: auxiliary_lines[n].LineTextBrush, format: format_right);
            }
            else
            {
                g.DrawLine(auxiliary_lines[n].GetPen(), width - leftRight + 1, auxiliary_lines[n].PaintValue, width - leftRight + 4, auxiliary_lines[n].PaintValue);
                g.DrawString(layoutRectangle: new RectangleF(width - leftRight + 4, auxiliary_lines[n].PaintValue - 9f, leftRight - 4, 20f), s: FromHelper.GetFormatString(referenceAxisRight.Format, auxiliary_lines[n].Value), font: font_size9, brush: auxiliary_lines[n].LineTextBrush, format: format_left);
            }
            g.DrawLine(auxiliary_lines[n].GetPen(), leftRight, auxiliary_lines[n].PaintValue, width - leftRight, auxiliary_lines[n].PaintValue);
        }
        if (value_IsAbscissaStrech)
        {
            foreach (MarkTextH MarkTextH in MarkTextHs)
            {
                foreach (KeyValuePair<string, CurveItemH> item2 in data_list)
                {
                    if (item2.Value.Visible && item2.Value.LineRenderVisiable && !(item2.Key != MarkTextH.CurveKey))
                    {
                        float[] data = item2.Value.Data;
                        if (data != null && data.Length > 1)
                        {
                            float num10 = (float)(width - leftRight * 2) * 1f / (float)(value_StrechDataCountMax - 1);
                            if (MarkTextH.Index >= 0 && MarkTextH.Index < item2.Value.Data.Length)
                            {
                                PointF center = new PointF((float)leftRight + (float)MarkTextH.Index * num10, FromHelper.ComputePaintLocationY((item2.Value.ReferenceAxisIndex == 0) ? referenceAxisLeft.Max : referenceAxisRight.Max, (item2.Value.ReferenceAxisIndex == 0) ? referenceAxisLeft.Min : referenceAxisRight.Min, num, item2.Value.Data[MarkTextH.Index]) + (float)upDown);
                                MarkTextPositionStyle markTextPosition = (MarkTextH.PositionStyle == MarkTextPositionStyle.Auto) ? MarkTextH.CalculateDirectionFromDataIndex(item2.Value.Data, MarkTextH.Index) : MarkTextH.PositionStyle;
                                CurveHelperH.DrawMarkTextHPoint(g, MarkTextH, center, Font, markTextPosition);
                            }
                        }
                    }
                }
            }
            foreach (CurveItemH value3 in data_list.Values)
            {
                if (value3.Visible && value3.LineRenderVisiable)
                {
                    float[] data2 = value3.Data;
                    if (data2 != null && data2.Length > 1)
                    {
                        float num11 = (float)(width - leftRight * 2) * 1f / (float)(value_StrechDataCountMax - 1);
                        List<PointF> list = new List<PointF>(value3.Data.Length);
                        for (int num12 = 0; num12 < value3.Data.Length; num12++)
                        {
                            if (!float.IsNaN(value3.Data[num12]))
                            {
                                PointF pointF = default(PointF);
                                pointF.X = (float)leftRight + (float)num12 * num11;
                                pointF.Y = FromHelper.ComputePaintLocationY((value3.ReferenceAxisIndex == 0) ? referenceAxisLeft.Max : referenceAxisRight.Max, (value3.ReferenceAxisIndex == 0) ? referenceAxisLeft.Min : referenceAxisRight.Min, num, value3.Data[num12]) + (float)upDown;
                                list.Add(pointF);
                                if (!string.IsNullOrEmpty(value3.MarkText[num12]))
                                {
                                    using (Brush brush = new SolidBrush(value3.LineColor))
                                    {
                                        g.FillEllipse(brush, new RectangleF(pointF.X - 3f, pointF.Y - 3f, 6f, 6f));
                                        MarkTextPositionStyle markTextPosition2 = MarkTextH.CalculateDirectionFromDataIndex(value3.Data, num12);
                                        CurveHelperH.DrawTextByPoint(g, value3.MarkText[num12], pointF, Font, brush, markTextPosition2, 5);
                                    }
                                }
                            }
                            else
                            {
                                CurveHelperH.DrawLineCore(g, value3, list, pointsRadius);
                                list.Clear();
                            }
                        }
                        CurveHelperH.DrawLineCore(g, value3, list, pointsRadius);
                    }
                }
            }
        }
        else
        {
            foreach (MarkTextH MarkTextH2 in MarkTextHs)
            {
                foreach (KeyValuePair<string, CurveItemH> item3 in data_list)
                {
                    if (item3.Value.Visible && item3.Value.LineRenderVisiable && !(item3.Key != MarkTextH2.CurveKey))
                    {
                        float[] data3 = item3.Value.Data;
                        if (data3 != null && data3.Length > 1 && MarkTextH2.Index >= 0 && MarkTextH2.Index < item3.Value.Data.Length)
                        {
                            PointF center2 = new PointF(leftRight + MarkTextH2.Index, FromHelper.ComputePaintLocationY((item3.Value.ReferenceAxisIndex == 0) ? referenceAxisLeft.Max : referenceAxisRight.Max, (item3.Value.ReferenceAxisIndex == 0) ? referenceAxisLeft.Min : referenceAxisRight.Min, num, item3.Value.Data[MarkTextH2.Index]) + (float)upDown);
                            MarkTextPositionStyle markTextPosition3 = (MarkTextH2.PositionStyle == MarkTextPositionStyle.Auto) ? MarkTextH.CalculateDirectionFromDataIndex(item3.Value.Data, MarkTextH2.Index) : MarkTextH2.PositionStyle;
                            CurveHelperH.DrawMarkTextHPoint(g, MarkTextH2, center2, Font, markTextPosition3);
                        }
                    }
                }
            }
            foreach (CurveItemH value4 in data_list.Values)
            {
                if (value4.Visible && value4.LineRenderVisiable)
                {
                    float[] data4 = value4.Data;
                    if (data4 != null && data4.Length > 1)
                    {
                        int num13 = width - 2 * leftRight + 1;
                        List<PointF> list2 = new List<PointF>(value4.Data.Length);
                        if (value4.Data.Length <= num13)
                        {
                            for (int num14 = 0; num14 < value4.Data.Length; num14++)
                            {
                                if (!float.IsNaN(value4.Data[num14]))
                                {
                                    PointF pointF2 = default(PointF);
                                    pointF2.X = leftRight + num14;
                                    pointF2.Y = FromHelper.ComputePaintLocationY((value4.ReferenceAxisIndex == 0) ? referenceAxisLeft.Max : referenceAxisRight.Max, (value4.ReferenceAxisIndex == 0) ? referenceAxisLeft.Min : referenceAxisRight.Min, num, value4.Data[num14]) + (float)upDown;
                                    list2.Add(pointF2);
                                    DrawMarkPoint(g, value4.MarkText[num14], pointF2, value4.LineColor, MarkTextH.CalculateDirectionFromDataIndex(value4.Data, num14));
                                }
                                else
                                {
                                    CurveHelperH.DrawLineCore(g, value4, list2, pointsRadius);
                                    list2.Clear();
                                }
                            }
                        }
                        else
                        {
                            for (int num15 = 0; num15 < num13; num15++)
                            {
                                int num16 = num15 + value4.Data.Length - num13;
                                if (!float.IsNaN(value4.Data[num16]))
                                {
                                    PointF pointF3 = default(PointF);
                                    pointF3.X = leftRight + num15;
                                    pointF3.Y = FromHelper.ComputePaintLocationY((value4.ReferenceAxisIndex == 0) ? referenceAxisLeft.Max : referenceAxisRight.Max, (value4.ReferenceAxisIndex == 0) ? referenceAxisLeft.Min : referenceAxisRight.Min, num, value4.Data[num16]) + (float)upDown;
                                    list2.Add(pointF3);
                                    DrawMarkPoint(g, value4.MarkText[num16], pointF3, value4.LineColor, MarkTextH.CalculateDirectionFromDataIndex(value4.Data, num16));
                                }
                                else
                                {
                                    CurveHelperH.DrawLineCore(g, value4, list2, pointsRadius);
                                    list2.Clear();
                                }
                            }
                        }
                        CurveHelperH.DrawLineCore(g, value4, list2, pointsRadius);
                    }
                }
            }
        }
    }

    /// <summary>DrawMarkPoint 方法。</summary>
    private void DrawMarkPoint(Graphics g, string markText, PointF center, Color color, MarkTextPositionStyle markTextPosition)
    {
        if (!string.IsNullOrEmpty(markText))
        {
            using (Brush brush = new SolidBrush(color))
            {
                DrawMarkPoint(g, markText, center, brush, markTextPosition);
            }
        }
    }

    /// <summary>DrawMarkPoint 方法。</summary>
    private void DrawMarkPoint(Graphics g, string markText, PointF center, Brush brush, MarkTextPositionStyle markTextPosition)
    {
        if (!string.IsNullOrEmpty(markText))
        {
            g.FillEllipse(brush, new RectangleF(center.X - 3f, center.Y - 3f, 6f, 6f));
            CurveHelperH.DrawTextByPoint(g, markText, center, Font, brush, markTextPosition, 5);
        }
    }

    /// <summary>判断是否 NeedPaintDash。</summary>
    private bool IsNeedPaintDash(float paintValue)
    {
        for (int i = 0; i < auxiliary_lines.Count; i++)
        {
            if (Math.Abs(auxiliary_lines[i].PaintValue - paintValue) < (float)font_size9.Height)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>CalculateDataCountByOffect 方法。</summary>
    private int CalculateDataCountByOffect(float offect)
    {
        if (value_IntervalAbscissaText > 0)
        {
            return value_IntervalAbscissaText;
        }
        if (offect > 40f)
        {
            return 1;
        }
        offect = 40f / offect;
        return (int)Math.Ceiling(offect);
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
        base.Name = "CurveH";
        base.Size = new System.Drawing.Size(417, 205);
        ResumeLayout(false);
    }
}
}