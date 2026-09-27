using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个标题看板控件，用来显示重点的标题信息，两侧可以设置其他的数据信息")]
public class HTitle : UserControl
{
    /// <summary>leftRightCenterColor 字段。</summary>
    private int leftRightCenterColor = 30;

    /// <summary>leftTextColor 字段。</summary>
    private Color leftTextColor = Color.Cyan;

    /// <summary>rightTextColor 字段。</summary>
    private Color rightTextColor = Color.Yellow;

    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>edgeColor 字段。</summary>
    private Color edgeColor = Color.LightGray;

    /// <summary>leftRightEdgeColor 字段。</summary>
    private Color leftRightEdgeColor = Color.DimGray;

    /// <summary>ValvesStyleH 字段。</summary>
    private DirectionStyleH ValvesStyleH = DirectionStyleH.Horizontal;

    /// <summary>widthPercent 字段。</summary>
    private float widthPercent = 0.5f;

    /// <summary>fontRight 字段。</summary>
    private Font fontRight = null;

    /// <summary>fontLeft 字段。</summary>
    private Font fontLeft = null;

    /// <summary>textRight 字段。</summary>
    private string textRight = "ControlH";

    /// <summary>textLeft 字段。</summary>
    private string textLeft = "00:00:00";

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
    [HDescriptionLanguage("获取或设置中间信息的边缘颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边缘颜色")]
    [DefaultValue(typeof(Color), "LightGray")]
    public Color EdgeColor
    {
        get
        {
            return edgeColor;
        }
        set
        {
            edgeColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置两侧的边缘颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左右边缘颜色")]
    [DefaultValue(typeof(Color), "DimGray")]
    public Color LeftRightEdgeColor
    {
        get
        {
            return leftRightEdgeColor;
        }
        set
        {
            leftRightEdgeColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置两侧的中间颜色的系数，范围 0-100，0表示等同于边缘色，100表示白色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左边距右边距居中颜色比例")]
    [DefaultValue(30)]
    public int LeftRightCenterColorScale
    {
        get
        {
            return leftRightCenterColor;
        }
        set
        {
            leftRightCenterColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置标题控件是否是横向的还是纵向的")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题样式")]
    [DefaultValue(typeof(DirectionStyleH), "Horizontal")]
    public DirectionStyleH TitleStyle
    {
        get
        {
            return ValvesStyleH;
        }
        set
        {
            ValvesStyleH = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置右侧文本的字体信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("字体右边距")]
    public Font FontRight
    {
        get
        {
            return fontRight;
        }
        set
        {
            fontRight = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置左侧文本的字体信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("字体左边距")]
    public Font FontLeft
    {
        get
        {
            return fontLeft;
        }
        set
        {
            fontLeft = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置左侧文本的颜色信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左边距文本颜色")]
    [DefaultValue(typeof(Color), "Cyan")]
    public Color LeftTextColor
    {
        get
        {
            return leftTextColor;
        }
        set
        {
            leftTextColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置右侧文本的颜色信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("右边距文本颜色")]
    [DefaultValue(typeof(Color), "Yellow")]
    public Color RightTextColor
    {
        get
        {
            return rightTextColor;
        }
        set
        {
            rightTextColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置右侧文本的信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本右边距")]
    [DefaultValue("ControlH")]
    public string TextRight
    {
        get
        {
            return textRight;
        }
        set
        {
            textRight = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置左侧文本的信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本左边距")]
    [DefaultValue("00:00:00")]
    public string TextLeft
    {
        get
        {
            return textLeft;
        }
        set
        {
            textLeft = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置中间文本的宽度百分比，0.5表示50%")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("宽度百分比")]
    [DefaultValue(0.5f)]
    public float WidthPercent
    {
        get
        {
            return widthPercent;
        }
        set
        {
            widthPercent = value;
            Invalidate();
        }
    }

    public HTitle()
    {
        InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            sf = new StringFormat();
        sf.Alignment = StringAlignment.Center;
        sf.LineAlignment = StringAlignment.Center;
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
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
        if (ValvesStyleH == DirectionStyleH.Horizontal)
        {
            PaintMain(g, width, height);
            return;
        }
        g.TranslateTransform(width, 0f);
        g.RotateTransform(90f);
        PaintMain(g, height, width);
        g.ResetTransform();
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, float width, float height)
    {
        ColorBlend colorBlend = new ColorBlend();
        colorBlend.Positions = new float[3]
        {
            0f,
            0.5f,
            1f
        };
        colorBlend.Colors = new Color[3]
        {
            leftRightEdgeColor,
            FromHelper.GetColorLight(leftRightEdgeColor, leftRightCenterColor),
            leftRightEdgeColor
        };
        LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(0f, 0f), new PointF(0f, height), Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
        linearGradientBrush.InterpolationColors = colorBlend;
        g.FillRectangle(linearGradientBrush, 0f, 0f, width - 1f, height - 1f);
        colorBlend.Colors = new Color[3]
        {
            edgeColor,
            FromHelper.GetColorLight(edgeColor),
            edgeColor
        };
        linearGradientBrush.InterpolationColors = colorBlend;
        if (width * 0.1f < height)
        {
            g.FillPolygon(linearGradientBrush, new PointF[4]
            {
                new PointF(width * (0.5f - widthPercent / 2f), 0f),
                new PointF(width * (0.5f + widthPercent / 2f), 0f),
                new PointF(width * (0.4f + widthPercent / 2f), height - 1f),
                new PointF(width * (0.6f - widthPercent / 2f), height - 1f)
            });
        }
        else
        {
            g.FillPolygon(linearGradientBrush, new PointF[4]
            {
                new PointF(width * (0.5f - widthPercent / 2f), 0f),
                new PointF(width * (0.5f + widthPercent / 2f), 0f),
                new PointF(width * (0.5f + widthPercent / 2f) - height, height - 1f),
                new PointF(width * (0.5f - widthPercent / 2f) + height, height - 1f)
            });
        }
        linearGradientBrush.Dispose();
        linearGradientBrush = new LinearGradientBrush(new PointF(0f, height * 0.5f - (float)Font.Height), new PointF(0f, height * 0.5f + (float)Font.Height), Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
        colorBlend.Colors = new Color[3]
        {
            FromHelper.GetColorLight(rightTextColor, -30),
            rightTextColor,
            FromHelper.GetColorLight(rightTextColor, -30)
        };
        linearGradientBrush.InterpolationColors = colorBlend;
        using (Brush brush = new SolidBrush(ForeColor))
        {
            g.DrawString(Text, Font, brush, new RectangleF(0f, 0f, width - 1f, height - 1f), sf);
        }
        if (!string.IsNullOrEmpty(textRight))
        {
            g.DrawString(textRight, fontRight ?? Font, linearGradientBrush, new RectangleF(width * (0.5f + widthPercent / 2f), 0f, width * (0.5f - widthPercent / 2f), height - 1f), sf);
        }
        colorBlend.Colors = new Color[3]
        {
            FromHelper.GetColorLight(leftTextColor, -30),
            leftTextColor,
            FromHelper.GetColorLight(leftTextColor, -30)
        };
        linearGradientBrush.InterpolationColors = colorBlend;
        if (!string.IsNullOrEmpty(textLeft))
        {
            g.DrawString(textLeft, fontLeft ?? Font, linearGradientBrush, new RectangleF(0f, 0f, width * (0.5f - widthPercent / 2f), height - 1f), sf);
        }
        linearGradientBrush.Dispose();
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
        base.Name = "TitleH";
        base.Size = new System.Drawing.Size(771, 65);
        ResumeLayout(false);
    }
}
}