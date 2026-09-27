using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    using HFromUI.HLangage;
    [HDescriptionLanguage("历史曲线控件，支持光标移动，显示x,y轴数据信息，支持多条曲线，支持左右两个坐标轴")]
[ToolboxBitmap(typeof(HCurveHistory), "Resources.CurveHistoryH.bmp")]
public class HCurveHistory : UserControl
{
    public delegate void CurveDoubleClick(HCurveHistory curveH, int index, DateTime dateTime);

    public delegate void CurveCustomerDoubleClick(HCurveHistory curveH, int index, string customer);

    public delegate void CurveMouseMove(HCurveHistory curveH, int x, int y);

    public delegate void CurveRangeSelect(HCurveHistory curveH, MarkForeSectionH markForeSection);

    public delegate void CurveScollScaleChanged(HCurveHistory curveH, int scrollX, float scale, int offsetPaintScrollX);

    /// <summary>paintCount 字段。</summary>
    private int paintCount = 0;

    private List<AuxiliaryLable> auxiliary_Labels;

    private List<MarkLineH> MarkLineHs;

    private List<MarkTextH> MarkTextHs;

    private List<MarkImageH> MarkImageHs;

    private Dictionary<string, CurveItemH> data_dicts = null;

    /// <summary>data_lists 字段。</summary>
    private List<CurveItemH> data_lists = null;

    /// <summary>data_times 字段。</summary>
    private List<DateTime> data_times = null;

    /// <summary>data_customer 字段。</summary>
    private List<string> data_customer = null;

    private List<AuxiliaryLine> auxiliary_lines;

    /// <summary>data_ScaleX_Render 字段。</summary>
    private float data_ScaleX_Render = 1f;

    /// <summary>scale_x_options 字段。</summary>
    private float[] scale_x_options = new float[10]
    {
        0.0625f,
        0.125f,
        0.25f,
        0.5f,
        1f,
        2f,
        4f,
        8f,
        16f,
        32f
    };

    /// <summary>scale_x_index 字段。</summary>
    private int scale_x_index = 4;

    /// <summary>data_ScaleY_Render 字段。</summary>
    private float data_ScaleY_Render = 1f;

    /// <summary>scale_y_options 字段。</summary>
    private float[] scale_y_options = new float[6]
    {
        1f,
        2f,
        4f,
        8f,
        16f,
        32f
    };

    /// <summary>scale_y_index 字段。</summary>
    private int scale_y_index = 0;

    /// <summary>data_count 字段。</summary>
    private int data_count = 0;

    /// <summary>data_time_formate 字段。</summary>
    private string data_time_formate = "yyyy-MM-dd HH:mm:ss";

    /// <summary>mouse_hover_time_formate 字段。</summary>
    private string mouse_hover_time_formate = "HH:mm:ss";

    /// <summary>mouse_scroll_location 字段。</summary>
    private Point mouse_scroll_location = new Point(-1, -1);

    /// <summary>is_mouse_click_scroll 字段。</summary>
    private bool is_mouse_click_scroll = false;

    /// <summary>is_mouse_on_picture 字段。</summary>
    private bool is_mouse_on_picture = false;

    /// <summary>mouse_location 字段。</summary>
    private Point mouse_location = new Point(-1, -1);

    /// <summary>m_RowBetweenStart 字段。</summary>
    private int m_RowBetweenStart = -1;

    /// <summary>m_RowBetweenEnd 字段。</summary>
    private int m_RowBetweenEnd = -1;

    /// <summary>m_RowBetweenStartHeight 字段。</summary>
    private int m_RowBetweenStartHeight = -1;

    /// <summary>m_RowBetweenHeight 字段。</summary>
    private int m_RowBetweenHeight = -1;

    /// <summary>m_IsMouseLeftDown 字段。</summary>
    private bool m_IsMouseLeftDown = false;

    /// <summary>m_IsMouseMiddleDown 字段。</summary>
    private bool m_IsMouseMiddleDown = false;

    /// <summary>mouse_right_location 字段。</summary>
    private Point mouse_right_location = new Point(-1, -1);

    /// <summary>markBackSections 字段。</summary>
    private List<MarkBackSectionH> markBackSections = null;

    /// <summary>markForeSections 字段。</summary>
    private List<MarkForeSectionH> markForeSections = null;

    /// <summary>markForeSectionsTmp 字段。</summary>
    private List<MarkForeSectionH> markForeSectionsTmp = null;

    /// <summary>markForeActiveSections 字段。</summary>
    private List<MarkForeSectionH> markForeActiveSections = null;

    /// <summary>背景颜色。</summary>
    private Color backColor = Color.White;

    /// <summary>random 字段。</summary>
    private Random random = null;

    /// <summary>coordinateColor 字段。</summary>
    private Color coordinateColor = Color.LightGray;

    /// <summary>coordinateBrush 字段。</summary>
    private Brush coordinateBrush = new SolidBrush(Color.LightGray);

    /// <summary>coordinatePen 字段。</summary>
    private Pen coordinatePen = new Pen(Color.LightGray);

    /// <summary>coordinateDashColor 字段。</summary>
    private Color coordinateDashColor = Color.FromArgb(72, 72, 72);

    /// <summary>coordinateDashPen 字段。</summary>
    private Pen coordinateDashPen = null;

    /// <summary>markLineColor 字段。</summary>
    private Color markLineColor = Color.Cyan;

    /// <summary>markLinePen 字段。</summary>
    private Pen markLinePen = new Pen(Color.Cyan);

    /// <summary>markTextColor 字段。</summary>
    private Color markTextColor = Color.Yellow;

    /// <summary>markTextBrush 字段。</summary>
    private Brush markTextBrush = new SolidBrush(Color.Yellow);

    /// <summary>moveLineColor 字段。</summary>
    private Color moveLineColor = Color.White;

    /// <summary>moveLinePen 字段。</summary>
    private Pen moveLinePen = new Pen(Color.White);

    /// <summary>data_tip_width 字段。</summary>
    private int data_tip_width = 150;

    /// <summary>curveNameWidth 字段。</summary>
    private int curveNameWidth = 150;

    /// <summary>value_IntervalAbscissaText 字段。</summary>
    private int value_IntervalAbscissaText = 200;

    /// <summary>mouseHoverBackBrush 字段。</summary>
    private Brush mouseHoverBackBrush = new SolidBrush(Color.FromArgb(220, Color.FromArgb(52, 52, 52)));

    /// <summary>hoverBackColor 字段。</summary>
    private Color hoverBackColor = Color.FromArgb(52, 52, 52);

    /// <summary>markBorderColor 字段。</summary>
    private Color markBorderColor = Color.HotPink;

    /// <summary>markBorderPen 字段。</summary>
    private Pen markBorderPen = new Pen(Color.HotPink);

    private ReferenceAxis referenceAxisLeft;

    private ReferenceAxis referenceAxisRight;

    /// <summary>isOtherAxisHide 字段。</summary>
    private bool isOtherAxisHide = false;

    /// <summary>leftWidth 字段。</summary>
    private int leftWidth = 50;

    /// <summary>rightWidth 字段。</summary>
    private int rightWidth = 50;

    /// <summary>mouseHoverTimeWidth 字段。</summary>
    private int mouseHoverTimeWidth = 100;

    /// <summary>topHeadHeight 字段。</summary>
    private int topHeadHeight = 30;

    /// <summary>buttomHeight 字段。</summary>
    private int buttomHeight = 20;

    /// <summary>scrollHeight 字段。</summary>
    private int scrollHeight = 15;

    /// <summary>scrollX 字段。</summary>
    private int scrollX = 0;

    /// <summary>scrollMaxX 字段。</summary>
    private int scrollMaxX = 0;

    /// <summary>scrollWidth 字段。</summary>
    private int scrollWidth = 10;

    /// <summary>offsetPaintScrollX 字段。</summary>
    private int offsetPaintScrollX = 0;

    /// <summary>offsetPaintScrollY 字段。</summary>
    private int offsetPaintScrollY = 0;

    /// <summary>scrollRectage 字段。</summary>
    private Rectangle scrollRectage = new Rectangle(0, 0, 0, 0);

    /// <summary>scrollRectageBack 字段。</summary>
    private Rectangle scrollRectageBack = new Rectangle(0, 0, 0, 0);

    /// <summary>scrollColor 字段。</summary>
    private Color scrollColor = Color.DimGray;

    /// <summary>scaleMode 字段。</summary>
    private ScaleMode scaleMode = ScaleMode.OnlyX;

    /// <summary>renderInvertedTriangle 字段。</summary>
    private bool renderInvertedTriangle = true;

    /// <summary>renderScaleInfo 字段。</summary>
    private bool renderScaleInfo = true;

    /// <summary>rightRemainWidth 字段。</summary>
    private int rightRemainWidth = 200;

    /// <summary>markLineVisible 字段。</summary>
    private bool markLineVisible = true;

    /// <summary>value_Segment 字段。</summary>
    private int value_Segment = 5;

    /// <summary>pointsRadius 字段。</summary>
    private int pointsRadius = -1;

    /// <summary>isShowTextInfomation 字段。</summary>
    private bool isShowTextInfomation = true;

    /// <summary>isRederRightCoordinate 字段。</summary>
    private bool isRederRightCoordinate = true;

    /// <summary>isAllowSelectSection 字段。</summary>
    private bool isAllowSelectSection = true;

    /// <summary>isRenderTimeData 字段。</summary>
    private bool isRenderTimeData = false;

    /// <summary>isMouseFreeze 字段。</summary>
    private bool isMouseFreeze = false;

    /// <summary>isAoordinateRoundInt 字段。</summary>
    private bool isAoordinateRoundInt = false;

    private HCurveHistory syncCurveHistoryH;

    private ReferenceAxisCollection referenceAxes;

    /// <summary>isRenderYTip 字段。</summary>
    private bool isRenderYTip = true;

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("背景颜色")]
    [DefaultValue(typeof(Color), "46, 46, 46")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override Color BackColor
    {
        get
        {
            return base.BackColor;
        }
        set
        {
            backColor = value;
            base.BackColor = value;
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("前景颜色")]
    [Bindable(true)]
    [DefaultValue(typeof(Color), "Yellow")]
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

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置曲线控件是否显示右侧的坐标轴")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("右侧坐标绘制")]
    [DefaultValue(true)]
    public virtual bool RenderRightCoordinate
    {
        get
        {
            return isRederRightCoordinate;
        }
        set
        {
            isRederRightCoordinate = value;
            Invalidate();
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
            isShowTextInfomation = true;
            base.Text = value;
            scrollX = 0;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("或者或设置所有的实线坐标轴的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("坐标颜色")]
    [DefaultValue(typeof(Color), "LightGray")]
    public virtual Color CoordinateColor
    {
        get
        {
            return coordinateColor;
        }
        set
        {
            coordinateColor = value;
            coordinatePen.Dispose();
            coordinatePen = new Pen(coordinateColor);
            coordinateBrush.Dispose();
            coordinateBrush = new SolidBrush(coordinateColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("或者或设置所有的虚线坐标轴的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("虚线坐标颜色")]
    [DefaultValue(typeof(Color), "72, 72, 72")]
    public virtual Color DashCoordinateColor
    {
        get
        {
            return coordinateDashColor;
        }
        set
        {
            coordinateDashColor = value;
            coordinateDashPen.Dispose();
            coordinateDashPen = new Pen(coordinateDashColor);
            coordinateDashPen.DashPattern = new float[2]
            {
                5f,
                5f
            };
            coordinateDashPen.DashStyle = DashStyle.Custom;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("或者或设置所有的区间标记的线条颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("标记线条颜色")]
    [DefaultValue(typeof(Color), "Cyan")]
    public virtual Color MarkLineColor
    {
        get
        {
            return markLineColor;
        }
        set
        {
            markLineColor = value;
            markLinePen.Dispose();
            markLinePen = new Pen(markLineColor);
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("或者或设置所有的区间标记的文本颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("标记文本颜色")]
    [DefaultValue(typeof(Color), "Cyan")]
    public virtual Color MarkTextColor
    {
        get
        {
            return markTextColor;
        }
        set
        {
            markTextColor = value;
            markTextBrush.Dispose();
            markTextBrush = new SolidBrush(markTextColor);
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("或者或设置光标移动时显示信息的边框颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("标记边框颜色")]
    [DefaultValue(typeof(Color), "HotPink")]
    public virtual Color MarkBorderColor
    {
        get
        {
            return markBorderColor;
        }
        set
        {
            markBorderColor = value;
            markBorderPen?.Dispose();
            markBorderPen = new Pen(markBorderColor);
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置鼠标移动的时候，显示的提示信息的背景颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("悬停背景颜色")]
    [DefaultValue(typeof(Color), "52, 52, 52")]
    public Color HoverBackColor
    {
        get
        {
            return hoverBackColor;
        }
        set
        {
            hoverBackColor = value;
            mouseHoverBackBrush?.Dispose();
            mouseHoverBackBrush = new SolidBrush(Color.FromArgb(220, hoverBackColor));
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("或者或设置鼠标移动过程中的提示线的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("移动线条颜色")]
    [DefaultValue(typeof(Color), "White")]
    public virtual Color MoveLineColor
    {
        get
        {
            return moveLineColor;
        }
        set
        {
            moveLineColor = value;
            moveLinePen.Dispose();
            moveLinePen = new Pen(moveLineColor);
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置当前滚动条的颜色信息，默认为 Color.DimGray 颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("滚动颜色")]
    [DefaultValue(typeof(Color), "DimGray")]
    public virtual Color ScrollColor
    {
        get
        {
            return scrollColor;
        }
        set
        {
            scrollColor = value;
            InvalidateH();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置鼠标移动过程中提示信息的宽度")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("数据提示宽度")]
    [DefaultValue(150)]
    public virtual int DataTipWidth
    {
        get
        {
            return data_tip_width;
        }
        set
        {
            if (value > 20 && value < 800)
            {
                data_tip_width = value;
            }
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置曲线名称的布局宽度，默认为150")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("曲线名称宽度")]
    [DefaultValue(150)]
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("横坐标文字间隔")]
    [HDescriptionLanguage("获取或设置纵向虚线的分隔情况，单位为多少个像素点")]
    [Browsable(true)]
    [DefaultValue(200)]
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左边距宽度")]
    [HDescriptionLanguage("获取或设置左坐标轴的宽度信息，单位为多少个像素点")]
    [Browsable(true)]
    [DefaultValue(50)]
    public virtual int LeftWidth
    {
        get
        {
            return leftWidth;
        }
        set
        {
            leftWidth = value;
            InvalidateH();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("右边距宽度")]
    [HDescriptionLanguage("获取或设置右坐标轴的宽度信息，单位为多少个像素点")]
    [Browsable(true)]
    [DefaultValue(50)]
    public virtual int RightWidth
    {
        get
        {
            return rightWidth;
        }
        set
        {
            rightWidth = value;
            InvalidateH();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("顶边距曲线名称高度")]
    [HDescriptionLanguage("获取或设置顶部曲线名称部分的高度信息，单位为多少个像素点")]
    [Browsable(true)]
    [DefaultValue(30)]
    public virtual int TopCurveNameHeight
    {
        get
        {
            return topHeadHeight;
        }
        set
        {
            topHeadHeight = value;
            InvalidateH();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("底部高度")]
    [HDescriptionLanguage("获取或设置底部部分的高度信息，单位为多少个像素点")]
    [Browsable(true)]
    [DefaultValue(20)]
    public virtual int ButtomHeight
    {
        get
        {
            return buttomHeight;
        }
        set
        {
            buttomHeight = value;
            InvalidateH();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("滚动高度")]
    [HDescriptionLanguage("获取或设置底部滚动条部分的高度信息，单位为多少个像素点")]
    [Browsable(true)]
    [DefaultValue(15)]
    public virtual int ScrollHeight
    {
        get
        {
            return scrollHeight;
        }
        set
        {
            scrollHeight = value;
            InvalidateH();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("日期时间格式")]
    [HDescriptionLanguage("获取或设置曲线图里所有的时间的格式，默认是 yyyy-MM-dd HH:mm:ss")]
    [Browsable(true)]
    [DefaultValue("yyyy-MM-dd HH:mm:ss")]
    public virtual string DateTimeFormate
    {
        get
        {
            return data_time_formate;
        }
        set
        {
            data_time_formate = value;
            InvalidateH();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("悬停日期时间格式")]
    [HDescriptionLanguage("获取或设置曲线图里光标移动时下方显示的时间格式，默认是HH:mm:ss")]
    [Browsable(true)]
    [DefaultValue("HH:mm:ss")]
    public virtual string HoverDateTimeFormate
    {
        get
        {
            return mouse_hover_time_formate;
        }
        set
        {
            mouse_hover_time_formate = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("悬停日期时间宽度")]
    [HDescriptionLanguage("获取或设置曲线图里光标移动时下方显示的矩形宽度信息，默认100")]
    [Browsable(true)]
    [DefaultValue(100)]
    public virtual int HoverDateTimeWidth
    {
        get
        {
            return mouseHoverTimeWidth;
        }
        set
        {
            mouseHoverTimeWidth = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("允许选择区间")]
    [HDescriptionLanguage("获取或设置曲线是否禁止鼠标选择区间的功能。")]
    [Browsable(true)]
    [DefaultValue(true)]
    public virtual bool IsAllowSelectSection
    {
        get
        {
            return isAllowSelectSection;
        }
        set
        {
            isAllowSelectSection = value;
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
            InvalidateH();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置所有的数据点显示的半径大小，默认是-1，不显示数据点")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("数据点半径")]
    [DefaultValue(-1)]
    public virtual int PointsRadius
    {
        get
        {
            return pointsRadius;
        }
        set
        {
            pointsRadius = value;
            InvalidateH();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置鼠标移动的时候，是否显示鼠标的标记线，这个标记线跟随着光标移动")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("标记线条可见")]
    [DefaultValue(true)]
    public bool MarkLineVisible
    {
        get
        {
            return markLineVisible;
        }
        set
        {
            markLineVisible = value;
            InvalidateH();
        }
    }

    [HDescriptionLanguage("获取或设置其他的坐标轴信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("参考轴"), Browsable(true)]
    [TypeConverter(typeof(CollectionConverter))]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public virtual ReferenceAxisCollection ReferenceAxis
    {
        get
        {
            return referenceAxes;
        }
        set
        {
            referenceAxes = value;
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置是否显示倒三角的信息，默认为True，如果显示不限制倒三角，则顶部会连线")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制倒三角")]
    [DefaultValue(true)]
    public virtual bool RenderInvertedTriangle
    {
        get
        {
            return renderInvertedTriangle;
        }
        set
        {
            renderInvertedTriangle = value;
            InvalidateH();
        }
    }

    [HDescriptionLanguage("获取或设置是否显示缩放倍率信息，默认为True，如果想要不显示右上角的倍率信息，则设置为False")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制刻度信息"), Browsable(true)]
    [DefaultValue(true)]
    public virtual bool RenderScaleInfo
    {
        get
        {
            return renderScaleInfo;
        }
        set
        {
            renderScaleInfo = value;
            InvalidateH();
        }
    }

    [HDescriptionLanguage("获取或设置当前曲线的缩放模式，默认只有X轴，还可以选择同时缩放，或是先缩放X轴后缩放Y轴")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("比例模式"), Browsable(true)]
    [DefaultValue(typeof(ScaleMode), "OnlyX")]
    public virtual ScaleMode ScaleMode
    {
        get
        {
            return scaleMode;
        }
        set
        {
            scaleMode = value;
        }
    }

    [HDescriptionLanguage("获取或设置当前的曲线控件右侧的预留的空白宽度信息，像素为单位，默认200")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("右侧保留宽度"), Browsable(true)]
    [DefaultValue(200)]
    public int RightRemainWidth
    {
        get
        {
            return rightRemainWidth;
        }
        set
        {
            rightRemainWidth = value;
            RenderCurveUI();
        }
    }

    [HDescriptionLanguage("获取或设置当前的曲线控件的额外的轴是否隐藏，额外的轴主要指在属性 ReferenceAxis 配置的第三轴，第四轴，等等")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("其他轴自动隐藏"), Browsable(true)]
    [DefaultValue(false)]
    public bool IsOtherAxisHide
    {
        get
        {
            return isOtherAxisHide;
        }
        set
        {
            isOtherAxisHide = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置光标移动时，是否在每个Y轴上显示光标位置对应的值（这个值反计算时存在一定的微小误差），默认为 true，显示")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制 Y 轴提示"), Browsable(true)]
    [DefaultValue(true)]
    public bool IsRenderYTip
    {
        get
        {
            return isRenderYTip;
        }
        set
        {
            isRenderYTip = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public CurveRangeH[] CurveRanges
    {
        get;
        set;
    }


    [Category("Mouse")]
    [HDescriptionLanguage("当鼠标在曲线上双击时触发，由此获取到点击的数据的索引位置，时间坐标")]
    public event CurveDoubleClick onCurveDoubleClick;

    [Category("Mouse")]
    [HDescriptionLanguage("当鼠标在曲线上双击时触发，由此获取到点击的数据的索引位置，用户自定义的坐标")]
    public event CurveCustomerDoubleClick onCurveCustomerDoubleClick;

    [Category("Mouse")]
    [HDescriptionLanguage("当鼠标在曲线上双击时触发，由此获取到鼠标的移动位置")]
    public event CurveMouseMove onCurveMouseMove;

    [Category("Mouse")]
    [HDescriptionLanguage("当鼠标在曲线上选择了一个区间，由此出发了一个选择事件，注意，包含两侧的端点。")]
    public event CurveRangeSelect onCurveRangeSelect;

    public event CurveScollScaleChanged OnScaleChanged;

    public HCurveHistory()
    {
        auxiliary_lines = new List<AuxiliaryLine>();
        data_dicts = new Dictionary<string, CurveItemH>();
        data_lists = new List<CurveItemH>();
        markBackSections = new List<MarkBackSectionH>();
        markForeSections = new List<MarkForeSectionH>();
        markForeSectionsTmp = new List<MarkForeSectionH>();
        markForeActiveSections = new List<MarkForeSectionH>();
        MarkTextHs = new List<MarkTextH>();
        MarkImageHs = new List<MarkImageH>();
        MarkLineHs = new List<MarkLineH>();
        auxiliary_Labels = new List<AuxiliaryLable>();
        data_times = new List<DateTime>(0);
        data_customer = new List<string>();
        coordinateDashPen = new Pen(Color.FromArgb(72, 72, 72));
        coordinateDashPen.DashPattern = new float[2]
        {
            5f,
            5f
        };
        coordinateDashPen.DashStyle = DashStyle.Custom;
        random = new Random();
        referenceAxes = new ReferenceAxisCollection(this);
        referenceAxisLeft = new ReferenceAxis(this);
        referenceAxisRight = new ReferenceAxis(this);
        InitializeComponent();
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
        base.BackColor = Color.FromArgb(46, 46, 46);
        base.ForeColor = Color.Yellow;
    }

    /// <summary>InvalidateH 方法。</summary>
    private void InvalidateH()
    {
        Invalidate();
    }

    /// <summary>响应 Load 事件。</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        base.MouseMove += PictureBox3_MouseMove;
        base.MouseLeave += PictureBox3_MouseLeave;
        base.MouseEnter += PictureBox3_MouseEnter;
        base.MouseDown += PictureBox3_MouseDown;
        base.MouseUp += PictureBox3_MouseUp;
        base.Paint += PictureBox3_Paint;
        base.MouseDoubleClick += PictureBox3_MouseDoubleClick;
    }

    /// <summary>响应 KeyDown 事件。</summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (syncCurveHistoryH != null)
        {
            syncCurveHistoryH.OnKeyDown(e);
            return;
        }
        if (e.KeyCode == Keys.Left)
        {
            scrollX -= 5;
            if (scrollX < 0)
            {
                scrollX = 0;
            }
            InvalidateH();
        }
        else if (e.KeyCode == Keys.Right)
        {
            scrollX += 5;
            if (scrollX > scrollMaxX)
            {
                scrollX = scrollMaxX;
            }
            InvalidateH();
        }
        base.OnKeyDown(e);
    }

    /// <summary>响应 MouseWheel 事件。</summary>
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (syncCurveHistoryH != null)
        {
            syncCurveHistoryH.OnMouseWheel(e);
            return;
        }
        if (ScaleMode != 0)
        {
            if (mouse_location.X < 0)
            {
                return;
            }
            int actualLeftWidth = GetActualLeftWidth();
            int num = mouse_location.X + offsetPaintScrollX;
            int num2 = mouse_location.Y - 20 + offsetPaintScrollY;
            if (scale_x_index >= scale_x_options.Length)
            {
                scale_x_index = scale_x_options.Length - 1;
            }
            if (scale_y_index >= scale_y_options.Length)
            {
                scale_y_index = scale_y_options.Length - 1;
            }
            if (e.Delta == 120)
            {
                if (scale_x_index < scale_x_options.Length - 1)
                {
                    scale_x_index++;
                    data_ScaleX_Render = scale_x_options[scale_x_index];
                    if (ScaleMode == ScaleMode.Both && data_ScaleX_Render > 0.95f)
                    {
                        data_ScaleY_Render = data_ScaleX_Render;
                        offsetPaintScrollY = EnsureOffsetPaintScrollY(Convert.ToInt32((double)num2 * 1.0 * (double)scale_x_options[scale_x_index] / (double)scale_x_options[scale_x_index - 1]) - (mouse_location.Y - 20));
                    }
                    int num3 = CalculatePaintWidthAndScrollMax(base.Width - actualLeftWidth - rightWidth);
                    if (num3 - (base.Width - actualLeftWidth - rightWidth) == 0)
                    {
                        SetScrollXNewValue(0);
                        offsetPaintScrollX = 0;
                    }
                    else
                    {
                        SetScrollXNewValue(Convert.ToInt32(((double)num * 1.0 * (double)scale_x_options[scale_x_index] / (double)scale_x_options[scale_x_index - 1] - (double)mouse_location.X) * 1.0 * (double)scrollMaxX * 1.0 / (double)(num3 - (base.Width - actualLeftWidth - rightWidth))));
                        offsetPaintScrollX = Convert.ToInt32((double)num * 1.0 * (double)scale_x_options[scale_x_index] / (double)scale_x_options[scale_x_index - 1]) - mouse_location.X;
                        offsetPaintScrollX = FromHelper.Middle(0, offsetPaintScrollX, num3 - (base.Width - actualLeftWidth - rightWidth));
                    }
                    InvalidateH();
                }
                else if (ScaleMode == ScaleMode.XThenY && scale_y_index < scale_y_options.Length - 1)
                {
                    scale_y_index++;
                    data_ScaleY_Render = scale_y_options[scale_y_index];
                    offsetPaintScrollY = EnsureOffsetPaintScrollY(Convert.ToInt32((double)num2 * 1.0 * (double)scale_y_options[scale_y_index] / (double)scale_y_options[scale_y_index - 1]) - (mouse_location.Y - 20));
                    InvalidateH();
                }
            }
            else if (e.Delta == -120)
            {
                if (ScaleMode == ScaleMode.XThenY && scale_y_index > 0)
                {
                    scale_y_index--;
                    data_ScaleY_Render = scale_y_options[scale_y_index];
                    offsetPaintScrollY = EnsureOffsetPaintScrollY(Convert.ToInt32((double)num2 * 1.0 * (double)scale_y_options[scale_y_index] / (double)scale_y_options[scale_y_index + 1]) - (mouse_location.Y - 20));
                    InvalidateH();
                }
                else if (scale_x_index > 0)
                {
                    scale_x_index--;
                    data_ScaleX_Render = scale_x_options[scale_x_index];
                    if (ScaleMode == ScaleMode.Both && data_ScaleX_Render > 0.95f)
                    {
                        data_ScaleY_Render = data_ScaleX_Render;
                        offsetPaintScrollY = EnsureOffsetPaintScrollY(Convert.ToInt32((double)num2 * 1.0 * (double)scale_x_options[scale_x_index] / (double)scale_x_options[scale_x_index + 1]) - (mouse_location.Y - 20));
                    }
                    int num4 = CalculatePaintWidthAndScrollMax(base.Width - actualLeftWidth - rightWidth);
                    if (num4 - (base.Width - actualLeftWidth - rightWidth) <= 0)
                    {
                        SetScrollXNewValue(0);
                        offsetPaintScrollX = 0;
                    }
                    else
                    {
                        SetScrollXNewValue(Convert.ToInt32(((double)num * 1.0 * (double)scale_x_options[scale_x_index] / (double)scale_x_options[scale_x_index + 1] - (double)mouse_location.X) * 1.0 * (double)scrollMaxX * 1.0 / (double)(num4 - (base.Width - actualLeftWidth - rightWidth))));
                        offsetPaintScrollX = Convert.ToInt32((double)num * 1.0 * (double)scale_x_options[scale_x_index] / (double)scale_x_options[scale_x_index + 1]) - mouse_location.X;
                        offsetPaintScrollX = FromHelper.Middle(0, offsetPaintScrollX, num4 - (base.Width - actualLeftWidth - rightWidth));
                    }
                    InvalidateH();
                }
            }
            this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
        }
        base.OnMouseWheel(e);
    }

    /// <summary>EnsureOffsetPaintScrollY 方法。</summary>
    private int EnsureOffsetPaintScrollY(int offsetPaintScrollY)
    {
        int num = (int)((float)(base.Height - topHeadHeight - buttomHeight - 40) * data_ScaleY_Render);
        offsetPaintScrollY = FromHelper.Middle(0, offsetPaintScrollY, num - (base.Height - topHeadHeight - buttomHeight - 40));
        return offsetPaintScrollY;
    }

    /// <summary>设置 scrollPosition。</summary>
    public void SetScrollPosition(ScrollEventArgs e)
    {
        SetScrollXNewValue(e.NewValue);
        Refresh();
        this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
    }

    /// <summary>ScrollToRight 方法。</summary>
    public void ScrollToRight()
    {
        int actualLeftWidth = GetActualLeftWidth();
        CalculatePaintWidthAndScrollMax(base.Width - actualLeftWidth - rightWidth);
        SetScrollXNewValue(scrollMaxX);
        InvalidateH();
        this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
    }

    /// <summary>设置 syncCurveHistoryH。</summary>
    public void SetSyncCurveHistoryH(HCurveHistory CurveHistoryH)
    {
        syncCurveHistoryH = CurveHistoryH;
        if (CurveHistoryH != null)
        {
            CurveHistoryH.OnScaleChanged += delegate (HCurveHistory curve, int i, float j, int k)
            {
                data_ScaleX_Render = j;
                SetScrollXNewValue(i);
                offsetPaintScrollX = k;
                Refresh();
            };
            CurveHistoryH.onCurveMouseMove += delegate (HCurveHistory curve, int x, int y)
            {
                SetCurveMousePosition(x, y);
            };
        }
    }

    /// <summary>获取 acutalSegment。</summary>
    private int GetAcutalSegment(float m_data_ScaleY_Render)
    {
        return value_Segment * ((m_data_ScaleY_Render < 3.95f) ? 1 : ((int)(m_data_ScaleY_Render / 4f) * 2));
    }

    /// <summary>DrawCoordinate 方法。</summary>
    private void DrawCoordinate(Graphics g, bool isLeft, float dx, float dy, ReferenceAxis referenceAxis, bool needPaintDash)
    {
        StringFormat format = isLeft ? FromHelper.StringFormatRight : FromHelper.StringFormatLeft;
        float max = referenceAxis.Max;
        float min = referenceAxis.Min;
        string unit = referenceAxis.Unit;
        Brush brush = referenceAxis.Brush;
        Pen pen = new Pen(referenceAxis.Brush);
        g.TranslateTransform(dx, dy);
        float num = isShowTextInfomation ? 1f : data_ScaleY_Render;
        float num2 = base.Height - topHeadHeight - buttomHeight;
        float num3 = isLeft ? leftWidth : rightWidth;
        int num4 = (!isShowTextInfomation) ? offsetPaintScrollY : 0;
        if (isLeft)
        {
            g.DrawLine(pen, num3 - 1f, 5f, num3 - 1f, num2 - 15f);
        }
        else
        {
            g.DrawLine(pen, 0f, 5f, 0f, num2 - 15f);
        }
        if (RenderInvertedTriangle)
        {
            if (isLeft)
            {
                FromHelper.PaintTriangle(g, brush, new PointF(num3 - 1f, 10f), 5, GraphDirection.Upward);
            }
            else
            {
                FromHelper.PaintTriangle(g, brush, new PointF(0f, 10f), 5, GraphDirection.Upward);
            }
        }
        g.DrawString(unit, Font, brush, new RectangleF(0f, -15f, num3 - 5f, 30f), FromHelper.StringFormatRight);
        g.TranslateTransform(0f, -num4 + 20);
        int acutalSegment = GetAcutalSegment(num);
        for (int i = 0; i <= acutalSegment; i++)
        {
            float num5 = (float)((double)i * (double)(max - min) / (double)acutalSegment + (double)min);
            if (IsAoordinateRoundInt)
            {
                num5 = (float)Math.Round(num5, 0);
            }
            float num6 = FromHelper.ComputePaintLocationY(max, min, (num2 - 40f) * num, num5);
            if ((needPaintDash || IsNeedPaintDash(num6)) && !(num6 - (float)num4 + 20f < 0f) && !(num6 - (float)num4 + 20f > num2 - 20f))
            {
                if (isLeft)
                {
                    g.DrawLine(pen, num3 - 5f, num6, num3 - 2f, num6);
                    g.DrawString(layoutRectangle: new RectangleF(0f, num6 - 9f, num3 - 4f, 20f), s: FromHelper.GetFormatString(referenceAxis.Format, num5), font: Font, brush: brush, format: format);
                }
                else
                {
                    g.DrawLine(pen, 1f, num6, 5f, num6);
                    g.DrawString(layoutRectangle: new RectangleF(5f, num6 - 9f, num3 - 3f, 20f), s: FromHelper.GetFormatString(referenceAxis.Format, num5), font: Font, brush: brush, format: format);
                }
            }
        }
        if (!needPaintDash)
        {
            for (int j = 0; j < auxiliary_lines.Count; j++)
            {
                if (!auxiliary_lines[j].IsLeftFrame)
                {
                    continue;
                }
                float num7 = FromHelper.ComputePaintLocationY(max, min, (num2 - 40f) * num, auxiliary_lines[j].Value);
                if (!(num7 - (float)num4 + 20f < 0f) && !(num7 - (float)num4 + 20f > num2 - 20f))
                {
                    if (isLeft)
                    {
                        g.DrawLine(pen, num3 - 5f, num7, num3 - 2f, num7);
                        g.DrawString(layoutRectangle: new RectangleF(0f, num7 - 9f, num3 - 4f, 20f), s: FromHelper.GetFormatString(referenceAxis.Format, auxiliary_lines[j].Value), font: Font, brush: brush, format: format);
                    }
                    else
                    {
                        g.DrawLine(pen, 1f, num7, 5f, num7);
                        g.DrawString(layoutRectangle: new RectangleF(6f, num7 - 9f, num3 - 3f, 20f), s: FromHelper.GetFormatString(referenceAxis.Format, auxiliary_lines[j].Value), font: Font, brush: brush, format: format);
                    }
                }
            }
        }
        if (isRenderYTip && is_mouse_on_picture && !isShowTextInfomation && markLineVisible)
        {
            RectangleF rectangleF = new RectangleF(1f, mouse_location.Y - 9 + num4 - 20, num3 - 3f, 20f);
            g.FillRectangle(mouseHoverBackBrush, rectangleF);
            g.DrawRectangles(markBorderPen, new RectangleF[1]
            {
                rectangleF
            });
            rectangleF = (isLeft ? new RectangleF(0f, mouse_location.Y - 9 + num4 - 20, num3 - 4f, 20f) : new RectangleF(5f, mouse_location.Y - 9 + num4 - 20, num3 - 6f, 20f));
            float value = FromHelper.ComputeValueFromPaintLocationY(max, min, (num2 - 40f) * num, mouse_location.Y + num4 - 20);
            g.DrawString((referenceAxis.Format == "{0}") ? value.ToString("F2") : FromHelper.GetFormatString(referenceAxis.Format, value), Font, brush, rectangleF, format);
        }
        g.TranslateTransform(0f, num4 - 20);
        g.TranslateTransform(0f - dx, 0f - dy);
        pen.Dispose();
    }

    /// <summary>PaintFromString 方法。</summary>
    private void PaintFromString(Graphics g, string text)
    {
        int actualLeftWidth = GetActualLeftWidth();
        int num = base.Width - actualLeftWidth - rightWidth;
        int num2 = base.Height - topHeadHeight - buttomHeight;
        Font font = new Font(Font.FontFamily, 18f);
        if (text != null && text.Length > 400)
        {
            font = new Font(Font.FontFamily, 12f);
        }
        int num3 = num2 - 40;
        g.DrawLine(coordinatePen, 0, num2 - 20, num - 1, num2 - 20);
        for (int i = 1; i <= value_Segment; i++)
        {
            float value = (float)((double)i * (double)(referenceAxisLeft.Max - referenceAxisLeft.Min) / (double)value_Segment + (double)referenceAxisLeft.Min);
            float num4 = FromHelper.ComputePaintLocationY(referenceAxisLeft.Max, referenceAxisLeft.Min, num3, value) + 20f;
            if (IsNeedPaintDash(num4))
            {
                g.DrawLine(coordinateDashPen, 0f, num4, num - 1, num4);
            }
        }
        for (int j = 0; j < auxiliary_lines.Count; j++)
        {
            if (!(auxiliary_lines[j].PaintValue - (float)offsetPaintScrollY + 20f < 0f) && !(auxiliary_lines[j].PaintValue - (float)offsetPaintScrollY + 20f > (float)(num2 - 20)))
            {
                g.DrawLine(auxiliary_lines[j].GetPen(), 0f, auxiliary_lines[j].PaintValue, num - 1, auxiliary_lines[j].PaintValue);
            }
        }
        for (int k = value_IntervalAbscissaText; k < num; k += value_IntervalAbscissaText)
        {
            g.DrawLine(coordinateDashPen, k, num2 - 20, k, 0);
        }
        Rectangle r = new Rectangle(0, 0, num, num2);
        using (Brush brush = new SolidBrush(ForeColor))
        {
            g.DrawString(text, font, brush, r, FromHelper.StringFormatCenter);
        }
        font.Dispose();
    }

    /// <summary>判断是否 NeedPaintDash。</summary>
    private bool IsNeedPaintDash(float paintValue)
    {
        for (int i = 0; i < auxiliary_lines.Count; i++)
        {
            if (Math.Abs(auxiliary_lines[i].PaintValue - paintValue) < (float)Font.Height)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>获取 actualLeftWidth。</summary>
    private int GetActualLeftWidth()
    {
        if (isOtherAxisHide)
        {
            return leftWidth;
        }
        return leftWidth + leftWidth * referenceAxes.Count;
    }

    /// <summary>获取 curvePaintActualWidth。</summary>
    private int GetCurvePaintActualWidth()
    {
        return base.Width - GetActualLeftWidth() - rightWidth;
    }

    /// <summary>获取 curvePaintActualWidth。</summary>
    private int GetCurvePaintActualWidth(int width)
    {
        return width - GetActualLeftWidth() - rightWidth;
    }

    /// <summary>获取 curvePaintActualHeight。</summary>
    private int GetCurvePaintActualHeight(int height)
    {
        return height - topHeadHeight - buttomHeight;
    }

    /// <summary>判断是否 PaintxInRenderRegion。</summary>
    private bool IsPaintxInRenderRegion(int x, int width, int offset = 10)
    {
        if (data_ScaleX_Render >= 1f)
        {
            return (float)x >= (float)offsetPaintScrollX - (float)offset * data_ScaleX_Render && (float)x <= (float)(offsetPaintScrollX + width) + (float)offset * data_ScaleX_Render;
        }
        return x >= offsetPaintScrollX - offset && x <= offsetPaintScrollX + width + offset;
    }

    /// <summary>判断是否 RectangleInRenderRegion。</summary>
    private bool IsRectangleInRenderRegion(Rectangle rectangle, int width)
    {
        if (IsPaintxInRenderRegion(rectangle.X, width) || IsPaintxInRenderRegion(rectangle.X + rectangle.Width, width))
        {
            return true;
        }
        if (rectangle.X <= offsetPaintScrollX && rectangle.X + rectangle.Width >= offsetPaintScrollX + width)
        {
            return true;
        }
        return false;
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        PaintControlsH(graphics, base.Width, base.Height);
        base.OnPaint(e);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        if (data_ScaleX_Render < 0.1f)
        {
            g.SmoothingMode = SmoothingMode.HighSpeed;
        }
        else
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
        }
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        PaintMain(g, width, height);
    }

    /// <summary>PaintMain 方法。</summary>
    public void PaintMain(Graphics g, int width, int height)
    {
        DateTime now = DateTime.Now;
        paintCount++;
        int actualLeftWidth = GetActualLeftWidth();
        int curvePaintActualWidth = GetCurvePaintActualWidth(width);
        int curvePaintActualHeight = GetCurvePaintActualHeight(height);
        g.TranslateTransform(actualLeftWidth, topHeadHeight);
        if (isShowTextInfomation)
        {
            CalculateAuxiliaryPaintY(20);
            PaintFromString(g, Text);
        }
        else
        {
            g.SetClip(new Rectangle(0, 0, curvePaintActualWidth, height - topHeadHeight));
            GetRenderCurveMain(g, curvePaintActualWidth, curvePaintActualHeight);
            g.ResetClip();
        }
        g.TranslateTransform(-actualLeftWidth, -topHeadHeight);
        if (isShowTextInfomation)
        {
            CalculateAuxiliaryPaintY(0);
        }
        DrawCoordinate(g, true, (!isOtherAxisHide) ? (leftWidth * referenceAxes.Count) : 0, topHeadHeight, referenceAxisLeft, needPaintDash: false);
        if (isRederRightCoordinate)
        {
            DrawCoordinate(g,  false, width - rightWidth, topHeadHeight, referenceAxisRight, needPaintDash: false);
        }
        if (!isOtherAxisHide)
        {
            for (int i = 0; i < referenceAxes.Count; i++)
            {
                DrawCoordinate(g, true, leftWidth * i, topHeadHeight, referenceAxes[i], needPaintDash: true);
            }
        }
        if (!RenderInvertedTriangle)
        {
            g.DrawLine(coordinatePen, leftWidth + leftWidth * referenceAxes.Count - 1, topHeadHeight + 5, width - rightWidth, topHeadHeight + 5);
        }
        PaintHeadText(g, actualLeftWidth);
        if (ReferenceAxis.Count > 0)
        {
            g.DrawString(layoutRectangle: new Rectangle(GetActualLeftWidth() - leftWidth, base.Height - scrollHeight - 5, leftWidth, scrollHeight), s: isOtherAxisHide ? "<<" : ">>", font: Font, brush: coordinateBrush, format: FromHelper.StringFormatCenter);
        }
    }

    /// <summary>PaintHeadText 方法。</summary>
    private void PaintHeadText(Graphics g, int actualLeftWidth)
    {
        if (topHeadHeight <= 0)
        {
            return;
        }
        float num = actualLeftWidth + 1;
        float num2 = 11f;
        foreach (KeyValuePair<string, CurveItemH> data_dict in data_dicts)
        {
            if (data_dict.Value.Visible)
            {
                Pen pen = data_dict.Value.LineRenderVisiable ? new Pen(data_dict.Value.LineColor) : new Pen(Color.FromArgb(70, data_dict.Value.LineColor));
                g.DrawLine(pen, num, num2, num + 30f, num2);
                g.DrawEllipse(pen, num + 8f, num2 - 7f, 14f, 14f);
                pen.Dispose();
                SolidBrush solidBrush = data_dict.Value.LineRenderVisiable ? new SolidBrush(data_dict.Value.LineColor) : new SolidBrush(Color.FromArgb(70, data_dict.Value.LineColor));
                g.DrawString(data_dict.Key, Font, solidBrush, new RectangleF(num + 35f, num2 - 10f, curveNameWidth - 35, 20f), FromHelper.StringFormatLeft);
                data_dict.Value.TitleRegion = new RectangleF(num, num2 - 10f, curveNameWidth, 20f);
                solidBrush.Dispose();
                num += (float)curveNameWidth;
                if (num >= (float)(base.Width - rightWidth - 30))
                {
                    num = actualLeftWidth + 1;
                    num2 += 22f;
                }
            }
        }
        for (int i = 0; i < auxiliary_Labels.Count; i++)
        {
            if (!string.IsNullOrEmpty(auxiliary_Labels[i].Text))
            {
                int num3 = (auxiliary_Labels[i].LocationX > 1f) ? ((int)auxiliary_Labels[i].LocationX) : ((int)(auxiliary_Labels[i].LocationX * (float)base.Width));
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
                g.DrawString(auxiliary_Labels[i].Text, Font, auxiliary_Labels[i].TextBrush, new Rectangle(num3 + 7, 0, num4 + 3, 20), FromHelper.StringFormatCenter);
            }
        }
        if (renderScaleInfo)
        {
            using (Brush brush = new SolidBrush(scrollColor))
            {
                g.DrawString($"Scale:X{data_ScaleX_Render} Y{data_ScaleY_Render}", Font, brush, new RectangleF(base.Width - 205, 0f, 200f, Font.Height + 2), FromHelper.StringFormatRight);
            }
        }
    }

    /// <summary>获取 scale。</summary>
    private string GetScale()
    {
        if (data_ScaleX_Render > 0.8f)
        {
            return data_ScaleX_Render.ToString("F0");
        }
        if (data_ScaleX_Render > 0.3f)
        {
            return data_ScaleX_Render.ToString("F1");
        }
        return data_ScaleX_Render.ToString();
    }

    /// <summary>RenderCurveUI 方法。</summary>
    public void RenderCurveUI()
    {
        isShowTextInfomation = false;
        InvalidateH();
    }

    /// <summary>CalculatePaintWidthAndScrollMax 方法。</summary>
    private int CalculatePaintWidthAndScrollMax(int width)
    {
        int num = Math.Max(Convert.ToInt32((float)data_count * data_ScaleX_Render) + rightRemainWidth, width);
        scrollWidth = width * width / num;
        if (scrollWidth < 10)
        {
            scrollWidth = 10;
        }
        scrollMaxX = width - scrollWidth;
        return num;
    }

    /// <summary>设置 scrollXNewValue。</summary>
    private void SetScrollXNewValue(int scrollValue)
    {
        int curvePaintActualWidth = GetCurvePaintActualWidth();
        int num = CalculatePaintWidthAndScrollMax(curvePaintActualWidth);
        if (scrollValue > scrollMaxX)
        {
            scrollValue = scrollMaxX;
        }
        if (scrollValue < 0)
        {
            scrollValue = 0;
        }
        if (scrollX != scrollValue)
        {
            scrollX = scrollValue;
        }
        if (num > curvePaintActualWidth && scrollMaxX > 0)
        {
            offsetPaintScrollX = Convert.ToInt32((double)((long)(num - curvePaintActualWidth) * (long)scrollX) * 1.0 / (double)scrollMaxX);
        }
        else
        {
            offsetPaintScrollX = 0;
        }
    }

    /// <summary>获取 referenceAxisByIndex。</summary>
    private ReferenceAxis GetReferenceAxisByIndex(int index)
    {
        switch (index)
        {
            case 0:
                return new ReferenceAxis(referenceAxisLeft.Max, referenceAxisLeft.Min);
            case 1:
                return new ReferenceAxis(referenceAxisRight.Max, referenceAxisRight.Min);
            default:
                if (index - 2 < referenceAxes.Count)
                {
                    return referenceAxes[index - 2];
                }
                return null;
        }
    }

    /// <summary>获取 renderCurveMain。</summary>
    private void GetRenderCurveMain(Graphics g, int width, int height)
    {
        int num = CalculatePaintWidthAndScrollMax(width);
        if (num > width)
        {
            g.TranslateTransform(-offsetPaintScrollX, 0f);
        }
        for (int i = 0; i < markBackSections.Count; i++)
        {
            Rectangle rectangle = new Rectangle(Convert.ToInt32((float)markBackSections[i].StartIndex * data_ScaleX_Render), 0, Convert.ToInt32((float)(markBackSections[i].EndIndex - markBackSections[i].StartIndex) * data_ScaleX_Render), height - 20);
            if (IsRectangleInRenderRegion(rectangle, width))
            {
                using (Brush brush = new SolidBrush(markBackSections[i].BackColor))
                {
                    g.FillRectangle(brush, rectangle);
                }
                g.DrawRectangle(Pens.DimGray, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                string text = markBackSections[i].MarkText ?? string.Empty;
                if (markBackSections[i].StartIndex < data_times.Count && markBackSections[i].EndIndex < data_times.Count)
                {
                    text = text + " (" + (data_times[markBackSections[i].EndIndex] - data_times[markBackSections[i].StartIndex]).TotalMinutes.ToString("F1") + HTranslation.GetContent(" 分钟)");
                }
                g.DrawString(text, Font, Brushes.DimGray, new RectangleF(rectangle.X, 3f, rectangle.Width, height - 20), FromHelper.StringFormatTopCenter);
            }
        }
        g.TranslateTransform(0f, -offsetPaintScrollY + 20);
        foreach (MarkImageH MarkImageH in MarkImageHs)
        {
            if (MarkImageH.MarkImage != null)
            {
                Point point = default(Point);
                point.X = Convert.ToInt32((double)MarkImageH.Index * 1.0 * (double)data_ScaleX_Render);
                point.Y = Convert.ToInt32((MarkImageH.OffsetY < 1f) ? (MarkImageH.OffsetY * (float)(height - 40) * data_ScaleY_Render) : (MarkImageH.OffsetY * data_ScaleY_Render));
                int width2 = MarkImageH.ScaleEnable ? Convert.ToInt32((float)MarkImageH.MarkImage.Width * data_ScaleX_Render) : MarkImageH.MarkImage.Width;
                int height2 = MarkImageH.ScaleEnable ? Convert.ToInt32((float)MarkImageH.MarkImage.Height * data_ScaleY_Render) : MarkImageH.MarkImage.Height;
                Rectangle rectangle2 = default(Rectangle);
                if (MarkImageH.ReferencePoint == ContentAlignment.TopLeft)
                {
                    rectangle2 = new Rectangle(point.X, point.Y, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.TopCenter)
                {
                    rectangle2 = new Rectangle(point.X - MarkImageH.MarkImage.Width / 2, point.Y, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.TopRight)
                {
                    rectangle2 = new Rectangle(point.X - MarkImageH.MarkImage.Width, point.Y, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.MiddleLeft)
                {
                    rectangle2 = new Rectangle(point.X, point.Y - MarkImageH.MarkImage.Height / 2, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.MiddleCenter)
                {
                    rectangle2 = new Rectangle(point.X - MarkImageH.MarkImage.Width / 2, point.Y - MarkImageH.MarkImage.Height / 2, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.MiddleRight)
                {
                    rectangle2 = new Rectangle(point.X - MarkImageH.MarkImage.Width, point.Y - MarkImageH.MarkImage.Height / 2, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.BottomLeft)
                {
                    rectangle2 = new Rectangle(point.X, point.Y - MarkImageH.MarkImage.Height, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.BottomCenter)
                {
                    rectangle2 = new Rectangle(point.X - MarkImageH.MarkImage.Width / 2, point.Y - MarkImageH.MarkImage.Height, width2, height2);
                }
                else if (MarkImageH.ReferencePoint == ContentAlignment.BottomRight)
                {
                    rectangle2 = new Rectangle(point.X - MarkImageH.MarkImage.Width, point.Y - MarkImageH.MarkImage.Height, width2, height2);
                }
                if (IsRectangleInRenderRegion(rectangle2, width))
                {
                    g.DrawImage(MarkImageH.MarkImage, rectangle2);
                }
            }
        }
        g.TranslateTransform(0f, offsetPaintScrollY - 20);
        if (num > width)
        {
            g.TranslateTransform(offsetPaintScrollX, 0f);
        }
        g.DrawLine(coordinatePen, 0, height - 20, width, height - 20);
        g.TranslateTransform(0f, -offsetPaintScrollY + 20);
        for (int j = 0; j < auxiliary_lines.Count; j++)
        {
            if (auxiliary_lines[j].IsLeftFrame)
            {
                auxiliary_lines[j].PaintValue = FromHelper.ComputePaintLocationY(referenceAxisLeft.Max, referenceAxisLeft.Min, (float)(height - 40) * data_ScaleY_Render, auxiliary_lines[j].Value);
            }
            else
            {
                auxiliary_lines[j].PaintValue = FromHelper.ComputePaintLocationY(referenceAxisRight.Max, referenceAxisRight.Min, (float)(height - 40) * data_ScaleY_Render, auxiliary_lines[j].Value);
            }
        }
        int acutalSegment = GetAcutalSegment(isShowTextInfomation ? 1f : data_ScaleY_Render);
        int num2 = (!isShowTextInfomation) ? offsetPaintScrollY : 0;
        for (int k = 1; k <= acutalSegment; k++)
        {
            float num3 = (float)((double)k * (double)(referenceAxisLeft.Max - referenceAxisLeft.Min) / (double)acutalSegment + (double)referenceAxisLeft.Min);
            if (IsAoordinateRoundInt)
            {
                num3 = (float)Math.Round(num3, 0);
            }
            float num4 = FromHelper.ComputePaintLocationY(referenceAxisLeft.Max, referenceAxisLeft.Min, (float)(height - 40) * data_ScaleY_Render, num3);
            if (IsNeedPaintDash(num4) && !(num4 - (float)num2 + 20f < 0f) && !(num4 - (float)num2 + 20f > (float)(height - 20)))
            {
                g.DrawLine(coordinateDashPen, 0f, num4, width, num4);
            }
        }
        for (int l = 0; l < auxiliary_lines.Count; l++)
        {
            if (!(auxiliary_lines[l].PaintValue - (float)offsetPaintScrollY + 20f < 0f) && !(auxiliary_lines[l].PaintValue - (float)offsetPaintScrollY + 20f > (float)(height - 20)))
            {
                g.DrawLine(auxiliary_lines[l].GetPen(), 0f, auxiliary_lines[l].PaintValue, width, auxiliary_lines[l].PaintValue);
            }
        }
        g.TranslateTransform(0f, offsetPaintScrollY - 20);
        if (num > width)
        {
            g.TranslateTransform(-offsetPaintScrollX, 0f);
        }
        for (int m = value_IntervalAbscissaText; m < num; m += value_IntervalAbscissaText)
        {
            if (num > width && (m < offsetPaintScrollX - 10 || m > offsetPaintScrollX + width + 10))
            {
                continue;
            }
            int num5 = Convert.ToInt32((float)m / data_ScaleX_Render);
            g.DrawLine(coordinateDashPen, m, height - 18, m, (!RenderInvertedTriangle) ? 5 : 0);
            Rectangle r = new Rectangle(m - 100, height - 18, 200, 17);
            if (buttomHeight > 20)
            {
                r.Height += buttomHeight - 20;
            }
            if (isRenderTimeData)
            {
                if (num5 < data_times.Count)
                {
                    g.DrawString(data_times[num5].ToString(data_time_formate, CultureInfo.InvariantCulture), Font, coordinateBrush, r, FromHelper.StringFormatCenter);
                }
            }
            else if (num5 < data_customer.Count)
            {
                g.DrawString(data_customer[num5], Font, coordinateBrush, r, FromHelper.StringFormatCenter);
            }
        }
        g.TranslateTransform(0f, -offsetPaintScrollY + 20);
        foreach (MarkTextH MarkTextH in MarkTextHs)
        {
            foreach (KeyValuePair<string, CurveItemH> data_dict in data_dicts)
            {
                if (data_dict.Value.Visible && data_dict.Value.LineRenderVisiable && !(data_dict.Key != MarkTextH.CurveKey))
                {
                    float[] data = data_dict.Value.Data;
                    if (data != null && data.Length > 1 && MarkTextH.Index >= 0 && MarkTextH.Index < data_dict.Value.Data.Length && !float.IsNaN(data_dict.Value.Data[MarkTextH.Index]))
                    {
                        PointF center = new PointF((float)MarkTextH.Index * data_ScaleX_Render, FromHelper.ComputePaintLocationY(GetReferenceAxisByIndex(data_dict.Value.ReferenceAxisIndex).Max, GetReferenceAxisByIndex(data_dict.Value.ReferenceAxisIndex).Min, (float)(height - 40) * data_ScaleY_Render, data_dict.Value.Data[MarkTextH.Index]));
                        MarkTextPositionStyle markTextPosition = (MarkTextH.PositionStyle == MarkTextPositionStyle.Auto) ? MarkTextH.CalculateDirectionFromDataIndex(data_dict.Value.Data, MarkTextH.Index) : MarkTextH.PositionStyle;
                        if (IsPaintxInRenderRegion((int)center.X, width))
                        {
                            if ((int)center.X <= offsetPaintScrollX + 10)
                            {
                                markTextPosition = MarkTextPositionStyle.Right;
                            }
                            DrawMarkTextPointH(g, MarkTextH, center, Font, markTextPosition);
                        }
                    }
                }
            }
        }
        foreach (MarkLineH MarkLineH in MarkLineHs)
        {
            PointF[] points = MarkLineH.Points;
            if (points != null && points.Length > 1)
            {
                PointF[] array = new PointF[MarkLineH.Points.Length];
                for (int n = 0; n < MarkLineH.Points.Length; n++)
                {
                    array[n].X = MarkLineH.Points[n].X * data_ScaleX_Render;
                    array[n].Y = FromHelper.ComputePaintLocationY(MarkLineH.IsLeftFrame ? referenceAxisLeft.Max : referenceAxisRight.Max, MarkLineH.IsLeftFrame ? referenceAxisLeft.Min : referenceAxisRight.Min, (float)(height - 40) * data_ScaleY_Render, MarkLineH.Points[n].Y);
                    MarkTextPositionStyle markTextPosition2 = MarkTextH.CalculateDirectionFromDataIndex(MarkLineH.Points, n);
                    g.FillEllipse(MarkLineH.CircleBrush, new RectangleF(array[n].X - 3f, array[n].Y - 3f, 6f, 6f));
                    if (MarkLineH.Marks != null)
                    {
                        DrawTextByPoint(g, MarkLineH.Marks[n], array[n], Font, MarkLineH.TextBrush, markTextPosition2, 5);
                    }
                }
                if (MarkLineH.IsLineClosed)
                {
                    g.DrawLines(MarkLineH.LinePen, array);
                    g.DrawLine(MarkLineH.LinePen, array[0], array[array.Length - 1]);
                }
                else
                {
                    g.DrawLines(MarkLineH.LinePen, array);
                }
            }
        }
        int num6 = 0;
        if (offsetPaintScrollX > 20)
        {
            num6 = Convert.ToInt32((double)offsetPaintScrollX * 1.0 / (double)data_ScaleX_Render);
        }
        num6 = ((num6 > 10) ? (num6 - 10) : 0);
        CurveRangeH[] curveRanges = CurveRanges;
        if (curveRanges != null && curveRanges.Length != 0)
        {
            for (int num7 = 0; num7 < CurveRanges.Length; num7++)
            {
                CurveRangeH CurveRangeH = CurveRanges[num7];
                GraphicsPath graphicsPath = new GraphicsPath();
                List<PointF> list = new List<PointF>(Convert.ToInt32((float)width / data_ScaleX_Render) + 10);
                List<PointF> list2 = new List<PointF>(Convert.ToInt32((float)width / data_ScaleX_Render) + 10);
                for (int num8 = num6; num8 < CurveRangeH.Upper.Length; num8++)
                {
                    PointF item = default(PointF);
                    item.X = (float)num8 * data_ScaleX_Render;
                    item.Y = FromHelper.ComputePaintLocationY(GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Max, GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Min, (float)(height - 40) * data_ScaleY_Render, CurveRangeH.Upper[num8]);
                    PointF item2 = default(PointF);
                    if (CurveRangeH.Lower != null)
                    {
                        item2.X = (float)num8 * data_ScaleX_Render;
                        item2.Y = FromHelper.ComputePaintLocationY(GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Max, GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Min, (float)(height - 40) * data_ScaleY_Render, CurveRangeH.Lower[num8]);
                    }
                    else
                    {
                        item2.X = (float)num8 * data_ScaleX_Render;
                        item2.Y = FromHelper.ComputePaintLocationY(GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Max, GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Min, (float)(height - 40) * data_ScaleY_Render, GetReferenceAxisByIndex(CurveRangeH.ReferenceAxisIndex).Min);
                    }
                    if (num <= width)
                    {
                        list.Add(item);
                        list2.Add(item2);
                    }
                    else if (item.X >= (float)offsetPaintScrollX - 10f * data_ScaleX_Render)
                    {
                        if (!(item.X < (float)(offsetPaintScrollX + width) + 10f * data_ScaleX_Render))
                        {
                            break;
                        }
                        list.Add(item);
                        list2.Add(item2);
                    }
                }
                list2.Reverse();
                if (CurveRangeH.Style == CurveStyle.LineSegment)
                {
                    graphicsPath.AddLines(list.ToArray());
                }
                else
                {
                    graphicsPath.AddCurve(list.ToArray());
                }
                graphicsPath.AddLine(list[list.Count - 1], list2[0]);
                if (CurveRangeH.Style == CurveStyle.LineSegment)
                {
                    graphicsPath.AddLines(list2.ToArray());
                }
                else
                {
                    graphicsPath.AddCurve(list2.ToArray());
                }
                graphicsPath.AddLine(list2[list2.Count - 1], list[0]);
                using (Brush brush2 = new SolidBrush(Color.FromArgb(48, CurveRangeH.LineColor)))
                {
                    g.FillPath(brush2, graphicsPath);
                }
                using (Pen pen = new Pen(CurveRangeH.LineColor))
                {
                    g.DrawPath(pen, graphicsPath);
                }
            }
        }
        foreach (CurveItemH value in data_dicts.Values)
        {
            if (value.Visible && value.LineRenderVisiable)
            {
                float[] data2 = value.Data;
                if (data2 != null && data2.Length > 1)
                {
                    List<PointF> list3 = new List<PointF>(Convert.ToInt32((float)width / data_ScaleX_Render) + 10);
                    for (int num9 = num6; num9 < value.Data.Length; num9++)
                    {
                        if (!float.IsNaN(value.Data[num9]))
                        {
                            PointF item3 = default(PointF);
                            item3.X = (float)num9 * data_ScaleX_Render;
                            item3.Y = FromHelper.ComputePaintLocationY(GetReferenceAxisByIndex(value.ReferenceAxisIndex).Max, GetReferenceAxisByIndex(value.ReferenceAxisIndex).Min, (float)(height - 40) * data_ScaleY_Render, value.Data[num9]);
                            if (num <= width)
                            {
                                list3.Add(item3);
                            }
                            else if (item3.X >= (float)offsetPaintScrollX - 10f * data_ScaleX_Render)
                            {
                                if (!(item3.X < (float)(offsetPaintScrollX + width) + 10f * data_ScaleX_Render))
                                {
                                    break;
                                }
                                list3.Add(item3);
                            }
                        }
                        else
                        {
                            DrawLineCore(g, value, list3, GetPointsRadius(), (float)(height - 40) * data_ScaleY_Render);
                            list3.Clear();
                        }
                    }
                    DrawLineCore(g, value, list3, GetPointsRadius(), (float)(height - 40) * data_ScaleY_Render);
                }
            }
        }
        for (int num10 = 0; num10 < markForeSections.Count; num10++)
        {
            DrawMarkForeSection(g, markForeSections[num10], Font, paintMain: true);
        }
        g.TranslateTransform(0f, offsetPaintScrollY - 20);
        if (num > width)
        {
            g.TranslateTransform(offsetPaintScrollX, 0f);
        }
        g.SmoothingMode = SmoothingMode.None;
        if (buttomHeight <= 0)
        {
            scrollRectage = new Rectangle(-1, -1, 1, 1);
            scrollRectageBack = new Rectangle(0, 0, 0, 0);
        }
        else
        {
            using (Brush brush3 = new SolidBrush(backColor))
            {
                g.FillRectangle(brush3, 0, base.Height - topHeadHeight - scrollHeight - 5, width, scrollHeight + 3);
            }
            if (num <= width)
            {
                scrollRectage = new Rectangle(-1, -1, 1, 1);
                scrollRectageBack = new Rectangle(0, 0, 0, 0);
                scrollX = 0;
                this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
            }
            else
            {
                scrollRectage = new Rectangle(scrollX, base.Height - topHeadHeight - scrollHeight - 3, scrollWidth, scrollHeight);
                scrollRectageBack = new Rectangle(0, base.Height - topHeadHeight - scrollHeight - 3, width, scrollHeight);
                using (Brush brush4 = new SolidBrush(Color.FromArgb(80, scrollColor)))
                {
                    g.FillRectangle(brush4, scrollRectageBack);
                }
                using (Brush brush5 = new SolidBrush(scrollColor))
                {
                    g.FillRectangle(brush5, scrollRectage);
                }
            }
        }
        g.SmoothingMode = SmoothingMode.AntiAlias;
    }

    /// <summary>响应 DockChanged 事件。</summary>
    protected override void OnDockChanged(EventArgs e)
    {
        base.OnDockChanged(e);
        OnAutoSizeChanged(e);
    }
    /// <summary>DrawLineCore 方法。</summary>
    private void DrawLineCore(Graphics g, CurveItemH line, List<PointF> listPoints, int pointsRadius, float referenceY = -1f)
    {
        if (listPoints.Count > 1)
        {
            using (Pen pen = new Pen(line.LineColor, line.LineThickness))
            {
                if (line.Style == CurveStyle.LineSegment)
                {
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.Curve)
                {
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.LineDot)
                {
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.CurveDot)
                {
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.LineDash)
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.CurveDash)
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.LineLongDath)
                {
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = new float[2]
                    {
                    5f,
                    5f
                    };
                    g.DrawLines(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.CurveLongDath)
                {
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = new float[2]
                    {
                    5f,
                    5f
                    };
                    g.DrawCurve(pen, listPoints.ToArray());
                }
                else if (line.Style == CurveStyle.Section)
                {
                    g.DrawCurve(pen, listPoints.ToArray());
                    if (listPoints.Count > 0)
                    {
                        GraphicsPath graphicsPath = new GraphicsPath();
                        graphicsPath.AddCurve(listPoints.ToArray());
                        graphicsPath.AddLines(new PointF[4]
                        {
                        listPoints[listPoints.Count - 1],
                        new PointF(listPoints[listPoints.Count - 1].X, referenceY),
                        new PointF(listPoints[0].X, referenceY),
                        new PointF(listPoints[0].X, listPoints[0].Y)
                        });
                        using (Brush brush = new SolidBrush(Color.FromArgb(64, pen.Color)))
                        {
                            g.FillPath(brush, graphicsPath);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < listPoints.Count - 1; i++)
                    {
                        PointF pointF = new PointF(listPoints[i + 1].X, listPoints[i].Y);
                        g.DrawLine(pen, listPoints[i], pointF);
                        if (line.Style == CurveStyle.StepLine)
                        {
                            g.DrawLine(pen, pointF, listPoints[i + 1]);
                        }
                    }
                }
            }
            if (pointsRadius > 0)
            {
                using (Brush brush2 = new SolidBrush(line.LineColor))
                {
                    for (int j = 0; j < listPoints.Count; j++)
                    {
                        g.FillEllipse(brush2, listPoints[j].X - (float)pointsRadius, listPoints[j].Y - (float)pointsRadius, pointsRadius * 2, pointsRadius * 2);
                    }
                }
            }
        }
    }
    /// <summary>获取 pointsRadius。</summary>
    private int GetPointsRadius()
    {
        if (data_ScaleX_Render >= 15f && pointsRadius == -1)
        {
            return 2;
        }
        return pointsRadius;
    }

    /// <summary>CalculateAuxiliaryPaintY 方法。</summary>
    private void CalculateAuxiliaryPaintY(int offset)
    {
        for (int i = 0; i < auxiliary_lines.Count; i++)
        {
            if (auxiliary_lines[i].IsLeftFrame)
            {
                auxiliary_lines[i].PaintValue = FromHelper.ComputePaintLocationY(referenceAxisLeft.Max, referenceAxisLeft.Min, base.Height - 40 - topHeadHeight - buttomHeight, auxiliary_lines[i].Value) + (float)offset;
            }
            else
            {
                auxiliary_lines[i].PaintValue = FromHelper.ComputePaintLocationY(referenceAxisRight.Max, referenceAxisRight.Min, base.Height - 40 - topHeadHeight - buttomHeight, auxiliary_lines[i].Value) + (float)offset;
            }
        }
    }

    /// <summary>AddLeftAuxiliary 方法。</summary>
    public AuxiliaryLine AddLeftAuxiliary(float value)
    {
        return AddLeftAuxiliary(value, coordinateColor);
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
        return AddRightAuxiliary(value, coordinateColor);
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

    /// <summary>AddMarkLine 方法。</summary>
    public void AddMarkLine(MarkLineH markLine)
    {
        MarkLineHs.Add(markLine);
    }

    /// <summary>RemoveMarkLine 方法。</summary>
    public void RemoveMarkLine(MarkLineH markLine)
    {
        MarkLineHs.Remove(markLine);
    }

    /// <summary>RemoveAllMarkLine 方法。</summary>
    public void RemoveAllMarkLine()
    {
        MarkLineHs.Clear();
    }

    /// <summary>AddMarkText 方法。</summary>
    public void AddMarkText(MarkTextH markText)
    {
        MarkTextHs.Add(markText);
    }

    /// <summary>RemoveMarkText 方法。</summary>
    public void RemoveMarkText(MarkTextH markText)
    {
        MarkTextHs.Remove(markText);
    }

    /// <summary>RemoveAllMarkText 方法。</summary>
    public void RemoveAllMarkText()
    {
        MarkTextHs.Clear();
    }

    /// <summary>AddMarkImage 方法。</summary>
    public void AddMarkImage(MarkImageH markImage)
    {
        MarkImageHs.Add(markImage);
    }

    /// <summary>RemoveMarkImage 方法。</summary>
    public void RemoveMarkImage(MarkImageH markImage)
    {
        MarkImageHs.Remove(markImage);
    }

    /// <summary>RemoveAllMarkImages 方法。</summary>
    public void RemoveAllMarkImages()
    {
        MarkImageHs.Clear();
    }

    /// <summary>CalculateCurveDataMax 方法。</summary>
    private void CalculateCurveDataMax()
    {
        data_count = 0;
        for (int i = 0; i < data_lists.Count; i++)
        {
            if (data_count < data_lists[i].Data.Length)
            {
                data_count = data_lists[i].Data.Length;
            }
        }
    }

    /// <summary>设置 dateTimes。</summary>
    public void SetDateTimes(DateTime[] times)
    {
        data_times = new List<DateTime>(times);
        isRenderTimeData = true;
    }

    /// <summary>设置 dateCustomer。</summary>
    public void SetDateCustomer(string[] customers)
    {
        data_customer = new List<string>(customers);
        isRenderTimeData = false;
    }

    /// <summary>设置 scaleByXAxis。</summary>
    public void SetScaleByXAxis(float scale)
    {
        data_ScaleX_Render = scale;
        SetScrollXNewValue(scrollX);
        this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
    }

    /// <summary>设置 scaleXOptions。</summary>
    public void SetScaleXOptions(float[] scales, int index)
    {
        scale_x_options = scales;
        if (index >= 0 && index < scale_x_options.Length)
        {
            scale_x_index = index;
            data_ScaleX_Render = scale_x_options[index];
        }
    }

    /// <summary>设置 scaleYOptions。</summary>
    public void SetScaleYOptions(float[] scales, int index)
    {
        scale_y_options = scales;
        if (index >= 0 && index < scale_y_options.Length)
        {
            scale_y_index = index;
            data_ScaleY_Render = scale_y_options[index];
        }
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

    /// <summary>设置 leftCurve。</summary>
    public void SetLeftCurve(string key, float[] data, Color lineColor, CurveStyle style, string renderFormat)
    {
        SetCurve(key, 0, data, lineColor, 1f, style, renderFormat);
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

    /// <summary>设置 rightCurve。</summary>
    public void SetRightCurve(string key, float[] data, Color lineColor, CurveStyle style, string renderFormat)
    {
        SetCurve(key, 1, data, lineColor, 1f, style, renderFormat);
    }

    /// <summary>设置 curve。</summary>
    public void SetCurve(string key, int referenceAxis, float[] data, Color lineColor, float thickness, CurveStyle style)
    {
        SetCurve(key, referenceAxis, data, lineColor, thickness, style, "{0}");
    }

    /// <summary>设置 curve。</summary>
    public void SetCurve(string key, int referenceAxis, float[] data, Color lineColor, float thickness, CurveStyle style, string renderFormat)
    {
        if (data_dicts.ContainsKey(key))
        {
            if (data == null)
            {
                data = new float[0];
            }
            data_dicts[key].Data = data;
        }
        else
        {
            if (data == null)
            {
                data = new float[0];
            }
            data_dicts.Add(key, new CurveItemH
            {
                Data = data,
                LineThickness = thickness,
                LineColor = lineColor,
                ReferenceAxisIndex = referenceAxis,
                Style = style,
                RenderFormat = renderFormat
            });
            data_lists.Add(data_dicts[key]);
        }
        CalculateCurveDataMax();
    }

    /// <summary>设置 curve。</summary>
    public void SetCurve(string key, CurveItemH curveItem)
    {
        data_dicts.Add(key, curveItem);
        CalculateCurveDataMax();
    }

    /// <summary>设置 curveLineColor。</summary>
    public void SetCurveLineColor(string key, Color lineColor)
    {
        if (data_dicts.ContainsKey(key))
        {
            data_dicts[key].LineColor = lineColor;
        }
    }

    /// <summary>设置 curveLineThickness。</summary>
    public void SetCurveLineThickness(string key, float thickness)
    {
        if (data_dicts.ContainsKey(key))
        {
            data_dicts[key].LineThickness = thickness;
        }
    }

    /// <summary>设置 curveLineCurveStyle。</summary>
    public void SetCurveLineCurveStyle(string key, CurveStyle style)
    {
        if (data_dicts.ContainsKey(key))
        {
            data_dicts[key].Style = style;
        }
    }

    /// <summary>设置 curveLineRenderFormat。</summary>
    public void SetCurveLineRenderFormat(string key, string renderFormat)
    {
        if (data_dicts.ContainsKey(key))
        {
            data_dicts[key].RenderFormat = renderFormat;
        }
    }

    /// <summary>RemoveCurve 方法。</summary>
    public void RemoveCurve(string key)
    {
        if (data_dicts.ContainsKey(key))
        {
            data_lists.Remove(data_dicts[key]);
            data_dicts.Remove(key);
        }
        if (data_dicts.Count == 0)
        {
            data_times = new List<DateTime>(0);
            data_customer = new List<string>();
        }
        CalculateCurveDataMax();
    }

    public Dictionary<string, CurveItemH> GetAllCurve()
    {
        return data_dicts;
    }

    /// <summary>RemoveAllCurve 方法。</summary>
    public void RemoveAllCurve()
    {
        data_dicts.Clear();
        data_lists.Clear();
        markBackSections.Clear();
        markForeSections.Clear();
        markForeSectionsTmp.Clear();
        markForeActiveSections.Clear();
        MarkTextHs.Clear();
        auxiliary_Labels.Clear();
        MarkLineHs.Clear();
        if (data_dicts.Count == 0)
        {
            data_times = new List<DateTime>(0);
        }
        CalculateCurveDataMax();
        SetScrollXNewValue(0);
        this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
    }

    /// <summary>设置 curveVisible。</summary>
    public void SetCurveVisible(string key, bool visible, bool lineRenderVisiable = true)
    {
        if (data_dicts.ContainsKey(key))
        {
            CurveItemH CurveItemH = data_dicts[key];
            CurveItemH.Visible = visible;
            CurveItemH.LineRenderVisiable = lineRenderVisiable;
        }
    }

    /// <summary>设置 curveVisible。</summary>
    public void SetCurveVisible(string[] keys, bool visible, bool lineRenderVisiable = true)
    {
        foreach (string key in keys)
        {
            if (data_dicts.ContainsKey(key))
            {
                CurveItemH CurveItemH = data_dicts[key];
                CurveItemH.Visible = visible;
                CurveItemH.LineRenderVisiable = lineRenderVisiable;
            }
        }
    }

    /// <summary>PictureBox3_MouseEnter 方法。</summary>
    private void PictureBox3_MouseEnter(object sender, EventArgs e)
    {
        if (syncCurveHistoryH != null)
        {
            syncCurveHistoryH.OnMouseEnter(e);
        }
        else
        {
            is_mouse_on_picture = true;
        }
    }

    /// <summary>PictureBox3_MouseLeave 方法。</summary>
    private void PictureBox3_MouseLeave(object sender, EventArgs e)
    {
        if (syncCurveHistoryH != null)
        {
            syncCurveHistoryH.OnMouseLeave(e);
            return;
        }
        if (!isMouseFreeze)
        {
            is_mouse_on_picture = false;
            this.onCurveMouseMove?.Invoke(this, -1, -1);
        }
        InvalidateH();
    }

    /// <summary>PictureBox3_MouseDoubleClick 方法。</summary>
    private void PictureBox3_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (syncCurveHistoryH != null)
        {
            syncCurveHistoryH.OnMouseDoubleClick(e);
        }
        else
        {
            if (isShowTextInfomation || !IsPointInDataRegion(e.Location))
            {
                return;
            }
            int actualLeftWidth = GetActualLeftWidth();
            if (e.X - actualLeftWidth + offsetPaintScrollX < 0)
            {
                return;
            }
            mouse_location = new Point(e.Location.X - actualLeftWidth, e.Location.Y - topHeadHeight);
            isMouseFreeze = true;
            int num = Convert.ToInt32((float)(e.X - actualLeftWidth + offsetPaintScrollX) / data_ScaleX_Render);
            if (isRenderTimeData)
            {
                if (num >= 0 && num < data_times.Count)
                {
                    this.onCurveDoubleClick?.Invoke(this, num, data_times[num]);
                }
            }
            else if (num >= 0 && num < data_customer.Count)
            {
                this.onCurveCustomerDoubleClick?.Invoke(this, num, data_customer[num]);
            }
        }
    }

    /// <summary>PictureBox3_MouseUp 方法。</summary>
    private void PictureBox3_MouseUp(object sender, MouseEventArgs e)
    {
        if (is_mouse_click_scroll)
        {
            is_mouse_click_scroll = false;
            InvalidateH();
        }
        else if (e.Button == MouseButtons.Left && isAllowSelectSection)
        {
            MarkForeSectionH MarkForeSectionH = new MarkForeSectionH
            {
                StartIndex = m_RowBetweenStart,
                EndIndex = m_RowBetweenEnd,
                Height = m_RowBetweenHeight,
                StartHeight = m_RowBetweenStartHeight,
                LinePen = markLinePen,
                FontBrush = markTextBrush
            };
            markForeSectionsTmp.Add(MarkForeSectionH);
            m_IsMouseLeftDown = false;
            InvalidateH();
            this.onCurveRangeSelect?.Invoke(this, MarkForeSectionH);
            m_RowBetweenStart = -1;
            m_RowBetweenEnd = -1;
            m_RowBetweenHeight = -1;
            m_RowBetweenStartHeight = -1;
        }
        else if (e.Button == MouseButtons.Middle)
        {
            if (syncCurveHistoryH != null)
            {
                syncCurveHistoryH.OnMouseUp(e);
                return;
            }
            m_IsMouseMiddleDown = false;
            mouse_right_location = new Point(-1, -1);
        }
    }

    /// <summary>PictureBox3_MouseDown 方法。</summary>
    private void PictureBox3_MouseDown(object sender, MouseEventArgs e)
    {
        int actualLeftWidth = GetActualLeftWidth();
        foreach (KeyValuePair<string, CurveItemH> data_dict in data_dicts)
        {
            if (data_dict.Value.TitleRegion.Contains(e.Location))
            {
                data_dict.Value.LineRenderVisiable = !data_dict.Value.LineRenderVisiable;
                InvalidateH();
                return;
            }
        }
        if (ReferenceAxis.Count > 0 && new Rectangle(GetActualLeftWidth() - leftWidth, base.Height - scrollHeight - 5, leftWidth, scrollHeight).Contains(e.Location))
        {
            isOtherAxisHide = !isOtherAxisHide;
            InvalidateH();
        }
        else if (scrollRectage.X >= 0 && scrollRectage.Contains(new Point(e.Location.X - actualLeftWidth, e.Location.Y - topHeadHeight)) && e.Button == MouseButtons.Left)
        {
            is_mouse_click_scroll = true;
            mouse_scroll_location = e.Location;
        }
        else if (scrollRectageBack.Width > 0 && scrollRectageBack.Contains(new Point(e.Location.X - actualLeftWidth, e.Location.Y - topHeadHeight)) && e.Button == MouseButtons.Left)
        {
            is_mouse_click_scroll = true;
            mouse_scroll_location = e.Location;
            SetScrollXNewValue(e.Location.X - actualLeftWidth);
            this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
            Refresh();
        }
        else if (e.Button == MouseButtons.Right)
        {
            isMouseFreeze = false;
            markForeSectionsTmp.Clear();
            m_RowBetweenStart = -1;
            m_RowBetweenEnd = -1;
            m_RowBetweenHeight = -1;
            m_RowBetweenStartHeight = -1;
            InvalidateH();
        }
        else if (e.Button == MouseButtons.Middle)
        {
            if (syncCurveHistoryH != null)
            {
                syncCurveHistoryH.OnMouseDown(e);
                return;
            }
            m_IsMouseMiddleDown = true;
            mouse_right_location = e.Location;
        }
        else if (isAllowSelectSection && e != null && e.X - actualLeftWidth + offsetPaintScrollX >= 0 && IsPointInDataRegion(e.Location))
        {
            m_IsMouseLeftDown = true;
            m_RowBetweenStart = Convert.ToInt32((float)(e.X - actualLeftWidth + offsetPaintScrollX) / data_ScaleX_Render);
            m_RowBetweenStartHeight = e.Y - topHeadHeight;
            m_RowBetweenHeight = e.Y - topHeadHeight;
        }
    }

    /// <summary>PictureBox3_MouseMove 方法。</summary>
    private void PictureBox3_MouseMove(object sender, MouseEventArgs e)
    {
        if (syncCurveHistoryH != null)
        {
            syncCurveHistoryH.OnMouseMove(e);
            return;
        }
        bool flag = false;
        foreach (KeyValuePair<string, CurveItemH> data_dict in data_dicts)
        {
            if (data_dict.Value.TitleRegion.Contains(e.Location))
            {
                flag = true;
                break;
            }
        }
        if (ReferenceAxis.Count > 0 && new Rectangle(GetActualLeftWidth() - leftWidth, base.Height - scrollHeight - 5, leftWidth, scrollHeight).Contains(e.Location))
        {
            flag = true;
        }
        Cursor = (flag ? Cursors.Hand : Cursors.Arrow);
        if (is_mouse_click_scroll)
        {
            int num = e.Location.X - mouse_scroll_location.X;
            mouse_scroll_location = new Point(e.Location.X, e.Location.Y);
            SetScrollXNewValue(scrollX + num);
            Refresh();
            this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
            return;
        }
        if (IsPointInDataRegion(e.Location))
        {
            int actualLeftWidth = GetActualLeftWidth();
            int curvePaintActualWidth = GetCurvePaintActualWidth();
            is_mouse_on_picture = true;
            if (is_mouse_on_picture && !isShowTextInfomation && !isMouseFreeze)
            {
                mouse_location = new Point(e.Location.X - actualLeftWidth, e.Location.Y - topHeadHeight);
                this.onCurveMouseMove?.Invoke(this, mouse_location.X, mouse_location.Y);
                Refresh();
            }
            if (!m_IsMouseMiddleDown)
            {
                return;
            }
            int num2 = e.Location.X - mouse_right_location.X;
            int num3 = (data_ScaleY_Render > 1.1f) ? (e.Location.Y - mouse_right_location.Y) : 0;
            mouse_right_location = e.Location;
            int num4 = CalculatePaintWidthAndScrollMax(curvePaintActualWidth);
            if (num4 > curvePaintActualWidth)
            {
                int num5 = offsetPaintScrollX - num2;
                SetScrollXNewValue(Convert.ToInt32((float)((long)(offsetPaintScrollX - num2) * (long)scrollMaxX) * 1f / (float)(num4 - curvePaintActualWidth)));
                offsetPaintScrollX = num5;
                if (offsetPaintScrollX >= num4 - curvePaintActualWidth)
                {
                    offsetPaintScrollX = num4 - curvePaintActualWidth;
                }
                if (offsetPaintScrollX < 0)
                {
                    offsetPaintScrollX = 0;
                }
                offsetPaintScrollY = EnsureOffsetPaintScrollY(offsetPaintScrollY - num3);
                Refresh();
                this.OnScaleChanged?.Invoke(this, scrollX, data_ScaleX_Render, offsetPaintScrollX);
            }
            return;
        }
        if (!isMouseFreeze)
        {
            mouse_location = new Point(-2);
            if (is_mouse_on_picture)
            {
                this.onCurveMouseMove?.Invoke(this, -1, -1);
            }
            is_mouse_on_picture = false;
        }
        InvalidateH();
    }

    /// <summary>设置 curveMousePosition。</summary>
    public void SetCurveMousePosition(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            is_mouse_on_picture = false;
        }
        else
        {
            is_mouse_on_picture = true;
        }
        mouse_location.X = x;
        mouse_location.Y = y;
        Refresh();
    }

    /// <summary>判断是否 PointInDataRegion。</summary>
    private bool IsPointInDataRegion(Point point)
    {
        int actualLeftWidth = GetActualLeftWidth();
        return point.X >= actualLeftWidth && point.X <= base.Width - rightWidth && point.Y >= topHeadHeight && point.Y <= base.Height - buttomHeight;
    }

    /// <summary>PictureBox3_Paint 方法。</summary>
    private void PictureBox3_Paint(object sender, PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        int actualLeftWidth = GetActualLeftWidth();
        graphics.TranslateTransform(actualLeftWidth, topHeadHeight);
        graphics.Clip = new Region(new Rectangle(0, 0, base.Width - actualLeftWidth - rightWidth + 1, base.Height - topHeadHeight - scrollHeight + 1));
        Font font = new Font(Font.FontFamily, 12f);
        int num = (int)((float)(mouse_location.X + offsetPaintScrollX) / data_ScaleX_Render);
        graphics.TranslateTransform(-offsetPaintScrollX, 0f);
        for (int i = 0; i < markForeSectionsTmp.Count; i++)
        {
            DrawMarkForeSection(graphics, markForeSectionsTmp[i], font);
        }
        if (m_IsMouseLeftDown && m_RowBetweenStart != -1 && num < data_count)
        {
            m_RowBetweenEnd = Convert.ToInt32((float)(mouse_location.X + offsetPaintScrollX) / data_ScaleX_Render);
            m_RowBetweenHeight = mouse_location.Y;
        }
        MarkForeSectionH markForeSection = new MarkForeSectionH
        {
            StartIndex = m_RowBetweenStart,
            EndIndex = m_RowBetweenEnd,
            Height = m_RowBetweenHeight,
            StartHeight = m_RowBetweenStartHeight,
            LinePen = markLinePen,
            FontBrush = markTextBrush
        };
        DrawMarkForeSection(graphics, markForeSection, font);
        for (int j = 0; j < markForeActiveSections.Count; j++)
        {
            if (IsPointInDataRegion(new Point(mouse_location.X + actualLeftWidth, mouse_location.Y + topHeadHeight)) && is_mouse_on_picture && num >= markForeActiveSections[j].StartIndex && num <= markForeActiveSections[j].EndIndex)
            {
                DrawMarkForeSection(graphics, markForeActiveSections[j], font);
            }
        }
        graphics.TranslateTransform(offsetPaintScrollX, 0f);
        if (is_mouse_on_picture && !isShowTextInfomation && markLineVisible)
        {
            graphics.DrawLine(moveLinePen, mouse_location.X, 15, mouse_location.X, base.Height - topHeadHeight - buttomHeight - 18);
            if (isRenderYTip)
            {
                graphics.DrawLine(moveLinePen, 0, mouse_location.Y, base.Width - rightWidth, mouse_location.Y);
            }
            if (num >= data_count || num < 0)
            {
                return;
            }
            int num2 = mouse_location.Y - data_dicts.Count * 20 - 20;
            if (num2 < 5)
            {
                num2 = 5;
            }
            int num3 = mouse_location.X;
            if (num3 + data_tip_width + 20 > base.Width - rightWidth - actualLeftWidth)
            {
                num3 = mouse_location.X - data_tip_width - 10;
            }
            int num4 = 0;
            foreach (KeyValuePair<string, CurveItemH> data_dict in data_dicts)
            {
                if (data_dict.Value.LineRenderVisiable)
                {
                    num4++;
                }
            }
            Rectangle rectangle = new Rectangle(num3 + 5, num2 - 5, data_tip_width, num4 * 20 + 10);
            using (GraphicsPath path = FromHelper.GetRoundRectange(rectangle, 4, topLeft: true, topRight: true, buttomRight: true, buttomLeft: true))
            {
                graphics.FillPath(mouseHoverBackBrush, path);
                graphics.DrawPath(markBorderPen, path);
            }
            foreach (KeyValuePair<string, CurveItemH> data_dict2 in data_dicts)
            {
                if (data_dict2.Value.LineRenderVisiable)
                {
                    Rectangle r = new Rectangle(rectangle.X + 3, num2, data_tip_width - 6, 20);
                    graphics.DrawString(data_dict2.Key, font, markTextBrush, r, FromHelper.StringFormatLeft);
                    if (num < data_dict2.Value.Data.Length)
                    {
                        graphics.DrawString(string.Format(data_dict2.Value.RenderFormat, data_dict2.Value.Data[num]), font, markTextBrush, r, FromHelper.StringFormatRight);
                    }
                    num2 += 20;
                }
            }
            num2 = mouse_location.Y + 25;
            for (int k = 0; k < markForeActiveSections.Count; k++)
            {
                if (num >= markForeActiveSections[k].StartIndex && num <= markForeActiveSections[k].EndIndex)
                {
                    float num5 = 0f;
                    foreach (KeyValuePair<string, string> cursorText in markForeActiveSections[k].CursorTexts)
                    {
                        num5 += graphics.MeasureString(cursorText.Key + " : " + cursorText.Value, Font, 244).Height + 8f;
                    }
                    rectangle = new Rectangle(num3 + 5, num2 - 5, 250, (int)num5 + 10);
                    if (num3 < mouse_location.X)
                    {
                        rectangle.X = mouse_location.X - 250 - 10;
                    }
                    graphics.FillRectangle(mouseHoverBackBrush, rectangle);
                    graphics.DrawRectangle(markBorderPen, rectangle);
                    foreach (KeyValuePair<string, string> cursorText2 in markForeActiveSections[k].CursorTexts)
                    {
                        num5 = graphics.MeasureString(cursorText2.Key + " : " + cursorText2.Value, Font, 244).Height;
                        graphics.DrawString(layoutRectangle: new Rectangle(rectangle.X + 3, num2, 244, 200), s: cursorText2.Key + " : " + cursorText2.Value, font: font, brush: Brushes.Yellow, format: FromHelper.StringFormatDefault);
                        num2 += (int)num5 + 8;
                    }
                    break;
                }
            }
            Rectangle rectangle2 = new Rectangle(mouse_location.X - mouseHoverTimeWidth / 2, base.Height - topHeadHeight - buttomHeight - 18, mouseHoverTimeWidth, 17);
            if (rectangle2.X < 0)
            {
                rectangle2.X = 0;
            }
            if (rectangle2.X > base.Width - actualLeftWidth - rightWidth - mouseHoverTimeWidth - 1)
            {
                rectangle2.X = base.Width - actualLeftWidth - rightWidth - mouseHoverTimeWidth - 1;
            }
            if (buttomHeight > 20)
            {
                rectangle2.Height += buttomHeight - 20;
            }
            if (isRenderTimeData)
            {
                if (num < data_times.Count)
                {
                    graphics.FillRectangle(mouseHoverBackBrush, rectangle2);
                    graphics.DrawRectangle(markBorderPen, rectangle2);
                    graphics.DrawString(data_times[num].ToString(mouse_hover_time_formate, CultureInfo.InvariantCulture), Font, markTextBrush, rectangle2, FromHelper.StringFormatCenter);
                }
            }
            else if (num < data_customer.Count)
            {
                graphics.FillRectangle(mouseHoverBackBrush, rectangle2);
                graphics.DrawRectangle(markBorderPen, rectangle2);
                graphics.DrawString(data_customer[num], Font, markTextBrush, rectangle2, FromHelper.StringFormatCenter);
            }
        }
        font.Dispose();
        graphics.TranslateTransform(-actualLeftWidth, -topHeadHeight);
    }

    /// <summary>AddMarkBackSection 方法。</summary>
    public void AddMarkBackSection(MarkBackSectionH markBackSection)
    {
        markBackSections.Add(markBackSection);
    }

    /// <summary>RemoveMarkBackSection 方法。</summary>
    public void RemoveMarkBackSection(MarkBackSectionH markBackSection)
    {
        markBackSections.Remove(markBackSection);
    }

    /// <summary>RemoveAllMarkBackSection 方法。</summary>
    public void RemoveAllMarkBackSection()
    {
        markBackSections.Clear();
    }

    /// <summary>AddMarkForeSection 方法。</summary>
    public void AddMarkForeSection(MarkForeSectionH markForeSection)
    {
        markForeSections.Add(markForeSection);
    }

    /// <summary>RemoveMarkForeSection 方法。</summary>
    public void RemoveMarkForeSection(MarkForeSectionH markForeSection)
    {
        markForeSections.Remove(markForeSection);
    }

    /// <summary>RemoveAllMarkForeSection 方法。</summary>
    public void RemoveAllMarkForeSection()
    {
        markForeSections.Clear();
    }

    /// <summary>RemoveAllMarkMouseSection 方法。</summary>
    public void RemoveAllMarkMouseSection()
    {
        markForeSectionsTmp.Clear();
        m_RowBetweenStart = -1;
        m_RowBetweenEnd = -1;
        m_RowBetweenHeight = -1;
        m_RowBetweenStartHeight = -1;
        InvalidateH();
    }

    /// <summary>AddMarkActiveSection 方法。</summary>
    public void AddMarkActiveSection(MarkForeSectionH markActiveSection)
    {
        markForeActiveSections.Add(markActiveSection);
    }
    /// <summary>DrawMarkTextPointH 方法。</summary>
    private void DrawMarkTextPointH(Graphics g, MarkTextH markText, PointF center, Font font, MarkTextPositionStyle markTextPosition)
    {
        if (markText == null)
        {
            return;
        }
        g.FillEllipse(markText.CircleBrush, new RectangleF(center.X - 3f, center.Y - 3f, 6f, 6f));
        if (!string.IsNullOrEmpty(markText.MarkText))
        {
            DrawTextByPoint(g, markText.MarkText, center, font, markText.TextBrush, markTextPosition, markText.MarkTextOffect);
        }
        if (markText.MarkImage != null)
        {
            switch (markTextPosition)
            {
                case MarkTextPositionStyle.Left:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)markText.MarkImage.Width - (float)markText.MarkTextOffect, center.Y - (float)markText.MarkImage.Height - (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Up:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)(markText.MarkImage.Width / 2), center.Y - (float)markText.MarkImage.Height - (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Right:
                    g.DrawImage(markText.MarkImage, new PointF(center.X + (float)markText.MarkTextOffect, center.Y - (float)markText.MarkImage.Height - (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Down:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)(markText.MarkImage.Width / 2), center.Y + (float)markText.MarkTextOffect));
                    break;
                case MarkTextPositionStyle.Center:
                    g.DrawImage(markText.MarkImage, new PointF(center.X - (float)(markText.MarkImage.Width / 2), center.Y - (float)(markText.MarkImage.Height / 2)));
                    break;
            }
        }
    }
    /// <summary>DrawTextByPoint 方法。</summary>
    private void DrawTextByPoint(Graphics g, string text, PointF center, Font font, Brush brush, MarkTextPositionStyle markTextPosition, int markTextOffect)
    {
        if (!string.IsNullOrEmpty(text))
        {
            switch (markTextPosition)
            {
                case MarkTextPositionStyle.Left:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y - (float)font.Height - (float)markTextOffect, 100 - markTextOffect, font.Height + markTextOffect), FromHelper.StringFormatRight);
                    break;
                case MarkTextPositionStyle.Up:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y - (float)font.Height - (float)markTextOffect, 200f, font.Height + 2), FromHelper.StringFormatCenter);
                    break;
                case MarkTextPositionStyle.Right:
                    g.DrawString(text, font, brush, new RectangleF(center.X + (float)markTextOffect, center.Y - (float)font.Height - (float)markTextOffect, 100f, font.Height + markTextOffect), FromHelper.StringFormatLeft);
                    break;
                case MarkTextPositionStyle.Down:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y + (float)markTextOffect, 200f, font.Height + 2), FromHelper.StringFormatCenter);
                    break;
                case MarkTextPositionStyle.Center:
                    g.DrawString(text, font, brush, new RectangleF(center.X - 100f, center.Y - (float)font.Height, 200f, font.Height * 2), FromHelper.StringFormatCenter);
                    break;
            }
        }
    }

    /// <summary>DrawMarkForeSection 方法。</summary>
    private void DrawMarkForeSection(Graphics g, MarkForeSectionH markForeSection, Font font, bool paintMain = false)
    {
        if (markForeSection == null)
        {
            return;
        }
        float num = 0f;
        num = ((!paintMain) ? ((markForeSection.Height > 1f) ? markForeSection.Height : ((float)(base.Height - topHeadHeight - buttomHeight - 40) * markForeSection.Height + 20f)) : ((markForeSection.Height > 1f) ? markForeSection.Height : ((float)(base.Height - topHeadHeight - buttomHeight - 40) * data_ScaleY_Render * markForeSection.Height)));
        float num2 = 0f;
        num2 = ((!paintMain) ? ((markForeSection.StartHeight > 1f) ? markForeSection.StartHeight : ((float)(base.Height - topHeadHeight - buttomHeight - 40) * markForeSection.StartHeight + 20f)) : ((markForeSection.StartHeight > 1f) ? markForeSection.StartHeight : ((float)(base.Height - topHeadHeight - buttomHeight - 40) * data_ScaleY_Render * markForeSection.StartHeight)));
        if (markForeSection.StartIndex == -1 || markForeSection.EndIndex == -1 || markForeSection.EndIndex <= markForeSection.StartIndex || !(num2 < num))
        {
            return;
        }
        int num3 = Convert.ToInt32((float)markForeSection.StartIndex * data_ScaleX_Render);
        int num4 = Convert.ToInt32((float)markForeSection.EndIndex * data_ScaleX_Render);
        if (markForeSection.StartIndex < data_count && markForeSection.EndIndex < data_count && markForeSection.StartIndex < data_times.Count && markForeSection.EndIndex < data_times.Count)
        {
            g.DrawLine(markForeSection.LinePen, new PointF(num3, num2), new PointF(num3, num + 30f));
            g.DrawLine(markForeSection.LinePen, new PointF(num4, num2), new PointF(num4, num + 30f));
            int num5 = markForeSection.IsRenderTimeText ? 20 : 0;
            int num6 = (num4 - num3 > 100) ? num5 : 110;
            int num7 = num4 - num3;
            g.DrawLine(markForeSection.LinePen, new PointF(num3 - num6, num), new PointF(num4 + num5, num));
            g.DrawLines(markForeSection.LinePen, new PointF[3]
            {
                new PointF(num3 + 20, num + 10f),
                new PointF(num3, num),
                new PointF(num3 + 20, num - 10f)
            });
            g.DrawLines(markForeSection.LinePen, new PointF[3]
            {
                new PointF(num4 - 20, num - 10f),
                new PointF(num4, num),
                new PointF(num4 - 20, num + 10f)
            });
            TimeSpan timeSpan = data_times[markForeSection.EndIndex] - data_times[markForeSection.StartIndex];
            string empty = string.Empty;
            empty = ((timeSpan.TotalMinutes > 2.0) ? (timeSpan.TotalMinutes.ToString("F1") + HTranslation.GetContent(" 分钟")) : ((!(timeSpan.TotalSeconds > 1.0)) ? (((int)timeSpan.TotalMilliseconds).ToString() + HTranslation.GetContent(" 毫秒")) : (timeSpan.TotalSeconds.ToString("F1") + HTranslation.GetContent(" 秒"))));
            if (num4 - num3 <= 100)
            {
                g.DrawString(empty, font, markForeSection.FontBrush, new PointF(num3 - 100, num - 17f));
            }
            else
            {
                g.DrawString(empty, font, markForeSection.FontBrush, new RectangleF(num3, num - (float)font.Height - 2f, num4 - num3, font.Height), FromHelper.StringFormatCenter);
            }
            if (!string.IsNullOrEmpty(markForeSection.MarkText))
            {
                g.DrawString(markForeSection.MarkText, font, markForeSection.FontBrush, new RectangleF(num3, num + 3f, num4 - num3, font.Height), FromHelper.StringFormatCenter);
            }
            if (markForeSection.IsRenderTimeText)
            {
                g.DrawString(HTranslation.GetContent("开始 ") + data_times[markForeSection.StartIndex].ToString(mouse_hover_time_formate), font, markForeSection.FontBrush, new PointF(num4 + 5, num - 17f));
                g.DrawString(HTranslation.GetContent("结束 ") + data_times[markForeSection.EndIndex].ToString(mouse_hover_time_formate), font, markForeSection.FontBrush, new PointF(num4 + 5, num + 2f));
            }
        }
    }

    /// <summary>SaveToBitmap 方法。</summary>
    public Bitmap SaveToBitmap(bool isCurrentRegion = false)
    {
        int actualLeftWidth = GetActualLeftWidth();
        if (isShowTextInfomation)
        {
            Bitmap bitmap = new Bitmap(base.Width, base.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            graphics.Clear(backColor);
            PaintMain(graphics, base.Width, base.Height);
            graphics.TranslateTransform(actualLeftWidth, topHeadHeight);
            PaintFromString(graphics, Text);
            graphics.TranslateTransform(-actualLeftWidth, -topHeadHeight);
            graphics.Dispose();
            return bitmap;
        }
        int num = (int)((float)data_count * data_ScaleX_Render) + rightRemainWidth;
        if (num < base.Width - actualLeftWidth - rightWidth)
        {
            num = base.Width - actualLeftWidth - rightWidth;
        }
        int num2 = base.Height - topHeadHeight - buttomHeight;
        Bitmap bitmap2 = new Bitmap(isCurrentRegion ? base.Width : (num + actualLeftWidth + rightWidth), base.Height);
        Graphics graphics2 = Graphics.FromImage(bitmap2);
        graphics2.SmoothingMode = SmoothingMode.AntiAlias;
        graphics2.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        graphics2.Clear(backColor);
        PaintMain(graphics2, bitmap2.Width, bitmap2.Height);
        if (isCurrentRegion)
        {
            PictureBox3_Paint(this, new PaintEventArgs(graphics2, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height)));
        }
        graphics2.Dispose();
        return bitmap2;
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
        BackColor = System.Drawing.Color.FromArgb(46, 46, 46);
        base.Name = "CurveHistoryH";
        base.Size = new System.Drawing.Size(852, 478);
        ResumeLayout(false);
    }
}
}