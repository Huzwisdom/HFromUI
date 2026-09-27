using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    using HFromUI.HLangage;
    [HDescriptionLanguage("文本组合控件，支持一个文本，数据，单位的同时显示操作")]
public class HLabelCombo : UserControl
{
    /// <summary>format_center 字段。</summary>
    private StringFormat format_center = null;

    /// <summary>format_left 字段。</summary>
    private StringFormat format_left = null;

    /// <summary>textInfo 字段。</summary>
    private string textInfo = "Infomation:";

    /// <summary>textValue 字段。</summary>
    private string textValue = "0.00";

    /// <summary>textValueWidth 字段。</summary>
    private float textValueWidth = 0.3f;

    /// <summary>textUnit 字段。</summary>
    private string textUnit = "Kg";

    /// <summary>locationInfo 字段。</summary>
    private float locationInfo = 0.02f;

    /// <summary>locationValue 字段。</summary>
    private float locationValue = 0.6f;

    /// <summary>locationUnit 字段。</summary>
    private float locationUnit = 0.91f;

    /// <summary>textValueBorderColor 字段。</summary>
    private Color textValueBorderColor = Color.LightGray;

    /// <summary>backcolorValue 字段。</summary>
    private Color backcolorValue = Color.Wheat;

    /// <summary>arrowDirection 字段。</summary>
    private ArrowDirection arrowDirection = ArrowDirection.Left;

    /// <summary>arrowColor 字段。</summary>
    private Color arrowColor = Color.Green;

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

    [HDescriptionLanguage("获取或设置当前的绘制的左侧的文本信息，通常是描述数据的内容")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本信息"), Browsable(true)]
    [DefaultValue("Infomation:")]
    public virtual string TextInfo
    {
        get
        {
            return textInfo;
        }
        set
        {
            textInfo = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置当前的中间的值文本，通常是数值信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本值"), Browsable(true)]
    [DefaultValue("0.00")]
    public virtual string TextValue
    {
        get
        {
            return textValue;
        }
        set
        {
            textValue = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置右侧的文本信息，通常用来表示单位信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本单位"), Browsable(true)]
    [DefaultValue("Kg")]
    public virtual string TextUnit
    {
        get
        {
            return textUnit;
        }
        set
        {
            textUnit = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置左侧的文本的位置，如果小于1，表示总宽度的百分比，如果大于1表示绝对的像素位置")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("位置信息"), Browsable(true)]
    [DefaultValue(0.02f)]
    public virtual float LocationInfo
    {
        get
        {
            return locationInfo;
        }
        set
        {
            locationInfo = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置中间的文本框的位置，如果小于1，表示总宽度的百分比，如果大于1表示绝对的像素位置")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("位置值"), Browsable(true)]
    [DefaultValue(0.6f)]
    public virtual float LocationValue
    {
        get
        {
            return locationValue;
        }
        set
        {
            locationValue = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置右侧的文本的位置，如果小于1，表示总宽度的百分比，如果大于1表示绝对的像素位置")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("位置单位"), Browsable(true)]
    [DefaultValue(0.91f)]
    public virtual float LocationUnit
    {
        get
        {
            return locationUnit;
        }
        set
        {
            locationUnit = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置中间文本框的宽度信息，如果小于1，表示总宽度的百分比，如果大于1表示绝对的像素位置")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本值宽度"), Browsable(true)]
    [DefaultValue(0.3f)]
    public virtual float TextValueWidth
    {
        get
        {
            return textValueWidth;
        }
        set
        {
            textValueWidth = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置中间文本框的背景颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本值背景色"), Browsable(true)]
    [DefaultValue(typeof(Color), "Wheat")]
    public virtual Color TextValueBackcolor
    {
        get
        {
            return backcolorValue;
        }
        set
        {
            if (backcolorValue != value)
            {
                backcolorValue = value;
                Invalidate();
            }
        }
    }

    [HDescriptionLanguage("获取或设置箭头的放向信息，只有向上或是向下会显示箭头，否则不显示")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("箭头方向"), Browsable(true)]
    [DefaultValue(typeof(ArrowDirection), "Left")]
    public virtual ArrowDirection ArrowDirection
    {
        get
        {
            return arrowDirection;
        }
        set
        {
            if (arrowDirection != value)
            {
                arrowDirection = value;
                Invalidate();
            }
        }
    }

    [HDescriptionLanguage("获取或设置箭头的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("箭头颜色"), Browsable(true)]
    [DefaultValue(typeof(Color), "Green")]
    public virtual Color ArrowColor
    {
        get
        {
            return arrowColor;
        }
        set
        {
            if (arrowColor != value)
            {
                arrowColor = value;
                Invalidate();
            }
        }
    }

    [HDescriptionLanguage("获取或设置中间文本框的的边框颜色信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本值边框颜色"), Browsable(true)]
    [DefaultValue(typeof(Color), "LightGray")]
    public virtual Color TextValueBorderColor
    {
        get
        {
            return textValueBorderColor;
        }
        set
        {
            if (textValueBorderColor != value)
            {
                textValueBorderColor = value;
                Invalidate();
            }
        }
    }

    [Browsable(false)]
    public Func<string, Color> TextValueColorTransFun
    {
        get;
        set;
    }

    public HLabelCombo()
    {
        InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            format_center = new StringFormat();
        format_center.Alignment = StringAlignment.Center;
        format_center.LineAlignment = StringAlignment.Center;
        format_left = new StringFormat();
        format_left.Alignment = StringAlignment.Near;
        format_left.LineAlignment = StringAlignment.Center;
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        PaintControlsH(e.Graphics, base.Width, base.Height);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        PaintMain(g, width, height);
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, int width, int height)
    {
        SolidBrush solidBrush = new SolidBrush(ForeColor);
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        RectangleF layoutRectangle = new RectangleF((locationInfo < 1f) ? (locationInfo * (float)width) : locationInfo, 0f, width, height - 1);
        g.DrawString(textInfo, Font, solidBrush, layoutRectangle, format_left);
        RectangleF rectangleF = new RectangleF((locationValue < 1f) ? (locationValue * (float)width) : locationValue, 1f, (textValueWidth < 1f) ? (textValueWidth * (float)width) : textValueWidth, height - 3);
        Color color = (TextValueColorTransFun == null) ? backcolorValue : TextValueColorTransFun(textValue);
        using (LinearGradientBrush brush = new LinearGradientBrush(new PointF(0f, height - 1), new PointF(0f, 1f), color, FromHelper.GetColorOffset(color, (height - 1) * 2 / 3)))
        {
            g.FillRectangle(brush, rectangleF);
        }
        using (Pen pen = new Pen(textValueBorderColor))
        {
            g.DrawRectangles(pen, new RectangleF[1]
            {
                    rectangleF
            });
        }
        g.DrawString(textValue, Font, solidBrush, rectangleF, format_center);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        float num = 6f;
        float num2 = rectangleF.X + rectangleF.Width - num - 1f;
        if (arrowDirection == ArrowDirection.Up)
        {
            PointF[] points = new PointF[5]
            {
                    new PointF(num2, (float)height - num - 2f),
                    new PointF(num2, num),
                    new PointF(num2 - 5f, num + 5f),
                    new PointF(num2, num),
                    new PointF(num2 + 5f, num + 5f)
            };
            using (Pen pen2 = new Pen(arrowColor))
            {
                g.DrawLines(pen2, points);
            }
        }
        else if (arrowDirection == ArrowDirection.Down)
        {
            PointF[] points2 = new PointF[5]
            {
                    new PointF(num2, num),
                    new PointF(num2, (float)height - num - 2f),
                    new PointF(num2 - 5f, (float)height - num - 2f - 5f),
                    new PointF(num2, (float)height - num - 2f),
                    new PointF(num2 + 5f, (float)height - num - 2f - 5f)
            };
            using (Pen pen3 = new Pen(arrowColor))
            {
                g.DrawLines(pen3, points2);
            }
        }
        RectangleF layoutRectangle2 = new RectangleF((locationUnit < 1f) ? (locationUnit * (float)width) : locationUnit, 0f, width, height - 1);
        g.DrawString(textUnit, Font, solidBrush, layoutRectangle2, format_left);
        solidBrush.Dispose();
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
        Font = new System.Drawing.Font(HTranslation.GetContent("微软雅黑"), 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
        base.Name = "LabelComboH";
        base.Size = new System.Drawing.Size(304, 33);
        ResumeLayout(false);
    }
}
}