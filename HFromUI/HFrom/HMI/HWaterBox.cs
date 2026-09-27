using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个水箱控件，可以设置水箱的高度，设置颜色信息")]
public class HWaterBox : UserControl
{
    /// <summary>edgeWidth 字段。</summary>
    private float edgeWidth = 10f;

    /// <summary>edgeColor 字段。</summary>
    private Color edgeColor = Color.FromArgb(253, 233, 208);

    /// <summary>brushEdge 字段。</summary>
    private Brush brushEdge = new SolidBrush(Color.FromArgb(253, 233, 208));

    /// <summary>boderColor 字段。</summary>
    private Color boderColor = Color.FromArgb(234, 205, 152);

    /// <summary>penBorder 字段。</summary>
    private Pen penBorder = new Pen(Color.FromArgb(234, 205, 152));

    /// <summary>waterColor 字段。</summary>
    private Color waterColor = Color.FromArgb(207, 235, 246);

    /// <summary>brushWater 字段。</summary>
    private Brush brushWater = new SolidBrush(Color.FromArgb(207, 235, 246));

    /// <summary>waterValue 字段。</summary>
    private float waterValue = 96f;

    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>ValvesStyleH 字段。</summary>
    private DirectionStyleH ValvesStyleH = DirectionStyleH.Horizontal;

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
    [HDescriptionLanguage("获取或设置水池的高度信息，值为百分比的内容。")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("值")]
    [DefaultValue(96f)]
    public float Value
    {
        get
        {
            return waterValue;
        }
        set
        {
            waterValue = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置水池边框的颜色。")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框颜色")]
    [DefaultValue(typeof(Color), "[234, 205, 152]")]
    public Color BorderColor
    {
        get
        {
            return boderColor;
        }
        set
        {
            boderColor = value;
            penBorder?.Dispose();
            penBorder = new Pen(boderColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置水池边界的颜色。")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边缘颜色")]
    [DefaultValue(typeof(Color), "[253, 233, 208]")]
    public Color EdgeColor
    {
        get
        {
            return edgeColor;
        }
        set
        {
            edgeColor = value;
            brushEdge?.Dispose();
            brushEdge = new SolidBrush(edgeColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置水池的颜色。")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("水颜色")]
    [DefaultValue(typeof(Color), "[207,235, 246]")]
    public Color WaterColor
    {
        get
        {
            return waterColor;
        }
        set
        {
            waterColor = value;
            brushWater?.Dispose();
            brushWater = new SolidBrush(waterColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置水池边界的宽度。")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边缘宽度")]
    [DefaultValue(10f)]
    public float EdgeWidth
    {
        get
        {
            return edgeWidth;
        }
        set
        {
            edgeWidth = value;
            if (edgeWidth < 0f)
            {
                edgeWidth = 0f;
            }
            Invalidate();
        }
    }

    public HWaterBox()
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
        if (!(height < edgeWidth + 2f))
        {
            g.TranslateTransform(width / 2f, 0f);
            float num = waterValue;
            if (num > 100f)
            {
                num = 100f;
            }
            if (num < 0f)
            {
                num = 0f;
            }
            float num2 = (height - edgeWidth - 1f) * num / 100f;
            g.FillRectangle(rect: new RectangleF((0f - width) / 2f, height - num2 - edgeWidth - 1f, width, num2), brush: brushWater);
            g.TranslateTransform((0f - width) / 2f, 0f);
            PointF[] points = new PointF[9]
            {
                new PointF(0f, 0f),
                new PointF(edgeWidth, 0f),
                new PointF(edgeWidth, height - edgeWidth - 1f),
                new PointF(width - edgeWidth, height - edgeWidth - 1f),
                new PointF(width - edgeWidth, 0f),
                new PointF(width - 1f, 0f),
                new PointF(width - 1f, height - 1f),
                new PointF(0f, height - 1f),
                new PointF(0f, 0f)
            };
            g.FillPolygon(brushEdge, points);
            g.DrawPolygon(penBorder, points);
            if (!string.IsNullOrEmpty(Text))
            {
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    g.DrawString(Text, Font, brush, new RectangleF(0f, 0f, width, height), sf);
                }
            }
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
        base.Name = "WaterBoxH";
        base.Size = new System.Drawing.Size(134, 119);
        ResumeLayout(false);
    }
}
}