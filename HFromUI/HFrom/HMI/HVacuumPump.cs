using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个真空泵控件，支持两个方向的旋转操作，支持颜色设置")]
public class HVacuumPump : UserControl
{
    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>ValvesStyleH 字段。</summary>
    private DirectionStyleH ValvesStyleH = DirectionStyleH.Vertical;

    /// <summary>entrance 字段。</summary>
    private int entrance = 1;

    /// <summary>export 字段。</summary>
    private int export = 4;

    /// <summary>color1 字段。</summary>
    private Color color1 = Color.FromArgb(0, 151, 0);

    /// <summary>brush1 字段。</summary>
    private Brush brush1 = new SolidBrush(Color.FromArgb(0, 151, 0));

    /// <summary>color2 字段。</summary>
    private Color color2 = Color.FromArgb(76, 76, 76);

    /// <summary>brush2 字段。</summary>
    private Brush brush2 = new SolidBrush(Color.FromArgb(76, 76, 76));

    /// <summary>color3 字段。</summary>
    private Color color3 = Color.FromArgb(255, 255, 255);

    /// <summary>brush3 字段。</summary>
    private Brush brush3 = new SolidBrush(Color.FromArgb(255, 255, 255));

    /// <summary>color4 字段。</summary>
    private Color color4 = Color.FromArgb(151, 151, 151);

    /// <summary>brush4 字段。</summary>
    private Brush brush4 = new SolidBrush(Color.FromArgb(151, 151, 151));

    /// <summary>moveSpeed 字段。</summary>
    private float moveSpeed = 0.3f;

    /// <summary>startAngle 字段。</summary>
    private float startAngle = -47f;

    /// <summary>timer 字段。</summary>
    private Timer timer = null;

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
    [HDescriptionLanguage("获取或设置泵控件是否是横向的还是纵向的")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("泵样式")]
    [DefaultValue(typeof(DirectionStyleH), "Vertical")]
    public DirectionStyleH PumpStyle
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
    [HDescriptionLanguage("获取或设置传送带流动的速度，0为静止，正数为正向流动，负数为反向流动")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("移动速度")]
    [DefaultValue(0.3f)]
    public float MoveSpeed
    {
        get
        {
            return moveSpeed;
        }
        set
        {
            moveSpeed = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色1")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 1")]
    [DefaultValue(typeof(Color), "[0, 151, 0]")]
    public Color Color1
    {
        get
        {
            return color1;
        }
        set
        {
            color1 = value;
            brush1.Dispose();
            brush1 = new SolidBrush(value);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色2")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 2")]
    [DefaultValue(typeof(Color), "[76, 76, 76]")]
    public Color Color2
    {
        get
        {
            return color2;
        }
        set
        {
            color2 = value;
            brush2.Dispose();
            brush2 = new SolidBrush(value);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色3")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 3")]
    [DefaultValue(typeof(Color), "[255, 255, 255]")]
    public Color Color3
    {
        get
        {
            return color3;
        }
        set
        {
            color3 = value;
            brush3.Dispose();
            brush3 = new SolidBrush(value);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色4")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 4")]
    [DefaultValue(typeof(Color), "[151, 151, 151]")]
    public Color Color4
    {
        get
        {
            return color4;
        }
        set
        {
            color4 = value;
            brush4.Dispose();
            brush4 = new SolidBrush(value);
            Invalidate();
        }
    }

    public HVacuumPump()
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
        timer = new Timer();
        timer.Interval = 50;
        timer.Tick += Timer_Tick;
        timer.Start();
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
        float num = 0f;
        float num2 = 0f;
        if (ValvesStyleH == DirectionStyleH.Vertical)
        {
            if (height >= width * 112 / 80)
            {
                num = width - 1;
                num2 = width * 112 / 80 - 1;
            }
            else
            {
                num2 = height - 1;
                num = height * 80 / 112 - 1;
            }
            PaintMain(g, num, num2);
            return;
        }
        if (width >= height * 112 / 80)
        {
            num2 = height - 1;
            num = height * 112 / 80 - 1;
        }
        else
        {
            num = width - 1;
            num2 = width * 80 / 112 - 1;
        }
        g.TranslateTransform(num, 0f);
        g.RotateTransform(90f);
        PaintMain(g, num2, num);
        g.ResetTransform();
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, float width, float height)
    {
        ColorBlend colorBlend = new ColorBlend();
        colorBlend.Positions = new float[3]
        {
            0f,
            0.51f,
            1f
        };
        colorBlend.Colors = new Color[3]
        {
            color1,
            Color.WhiteSmoke,
            color1
        };
        LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(0f, 0f), new PointF(width, 0f), Color.Wheat, Color.White);
        linearGradientBrush.InterpolationColors = colorBlend;
        g.FillEllipse(linearGradientBrush, new RectangleF(0f, 0f, width, width));
        g.FillEllipse(linearGradientBrush, new RectangleF(0f, height - width, width, width));
        if (height - width > 0f)
        {
            g.FillRectangle(linearGradientBrush, new RectangleF(0f, width / 2f, width, height - width));
        }
        g.FillEllipse(brush2, new RectangleF(width * 0.15f, width * 0.15f, width * 0.7f, width * 0.7f));
        g.FillEllipse(brush2, new RectangleF(width * 0.15f, height - width + width * 0.15f, width * 0.7f, width * 0.7f));
        if (height - width > 0f)
        {
            g.FillRectangle(brush2, new RectangleF(0f, width / 2f, width, height - width));
        }
        g.TranslateTransform(width / 2f, width / 2f);
        g.RotateTransform(startAngle);
        PointF[] points = new PointF[20]
        {
            new PointF(0f, (0f - width) * 0.08f),
            new PointF((0f - width) * 0.1f, (0f - width) * 0.12f),
            new PointF((0f - width) * 0.2f, (0f - width) * 0.13f),
            new PointF((0f - width) * 0.25f, (0f - width) * 0.11f),
            new PointF((0f - width) * 0.28f, (0f - width) * 0.07f),
            new PointF((0f - width) * 0.3f, 0f),
            new PointF((0f - width) * 0.28f, width * 0.07f),
            new PointF((0f - width) * 0.25f, width * 0.11f),
            new PointF((0f - width) * 0.2f, width * 0.13f),
            new PointF((0f - width) * 0.1f, width * 0.12f),
            new PointF(0f, width * 0.08f),
            new PointF(width * 0.1f, width * 0.12f),
            new PointF(width * 0.2f, width * 0.13f),
            new PointF(width * 0.25f, width * 0.11f),
            new PointF(width * 0.28f, width * 0.07f),
            new PointF(width * 0.3f, 0f),
            new PointF(width * 0.28f, (0f - width) * 0.07f),
            new PointF(width * 0.25f, (0f - width) * 0.11f),
            new PointF(width * 0.2f, (0f - width) * 0.13f),
            new PointF(width * 0.1f, (0f - width) * 0.12f)
        };
        linearGradientBrush.Dispose();
        colorBlend = new ColorBlend();
        colorBlend.Positions = new float[3]
        {
            0f,
            0.5f,
            1f
        };
        colorBlend.Colors = new Color[3]
        {
            color4,
            color3,
            color4
        };
        linearGradientBrush = new LinearGradientBrush(new PointF((0f - width) * 0.3f, 0f), new PointF(width * 0.3f, 0f), Color.Wheat, Color.White);
        linearGradientBrush.InterpolationColors = colorBlend;
        g.FillClosedCurve(linearGradientBrush, points);
        g.FillEllipse(Brushes.Black, new RectangleF((0f - width) * 0.04f, (0f - width) * 0.04f, width * 0.08f, width * 0.08f));
        g.RotateTransform(0f - startAngle);
        g.TranslateTransform((0f - width) / 2f, (0f - width) / 2f);
        g.TranslateTransform(width / 2f, height - width / 2f);
        g.RotateTransform(startAngle * -1f - 83f);
        g.FillClosedCurve(linearGradientBrush, points);
        g.FillEllipse(Brushes.Black, new RectangleF((0f - width) * 0.04f, (0f - width) * 0.04f, width * 0.08f, width * 0.08f));
        g.RotateTransform(0f - (startAngle * -1f - 83f));
        g.TranslateTransform((0f - width) / 2f, 0f - height + width / 2f);
        linearGradientBrush.Dispose();
    }

    /// <summary>获取 pointsFrom。</summary>
    private PointF[] GetPointsFrom(string points, float width, float height, float dx = 0f, float dy = 0f)
    {
        return FromHelper.GetPointsFrom(points, 80.7f, 112.5f, width, height, dx, dy);
    }

    /// <summary>Timer_Tick 方法。</summary>
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (moveSpeed != 0f)
        {
            startAngle = (float)((double)startAngle + (double)(moveSpeed * 180f) / Math.PI / 10.0);
            if (startAngle <= -360f)
            {
                startAngle += 360f;
            }
            else if (startAngle >= 360f)
            {
                startAngle -= 360f;
            }
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
        base.Name = "VacuumPumpH";
        base.Size = new System.Drawing.Size(109, 151);
        ResumeLayout(false);
    }
}
}