using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个袋式除尘器控件，支持设置颜色")]
public class HBagFilter : UserControl
{
    /// <summary>widthDesign 字段。</summary>
    private float widthDesign = 636f;

    /// <summary>heightDesign 字段。</summary>
    private float heightDesign = 562f;

    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>boderColor 字段。</summary>
    private Color boderColor = Color.FromArgb(150, 150, 150);

    /// <summary>borderPen 字段。</summary>
    private Pen borderPen = new Pen(Color.FromArgb(150, 150, 150));

    /// <summary>edgeColor 字段。</summary>
    private Color edgeColor = Color.FromArgb(176, 176, 176);

    /// <summary>centerColor 字段。</summary>
    private Color centerColor = Color.FromArgb(229, 229, 229);

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
    [HDescriptionLanguage("获取或设置分类器控件的边缘颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边缘颜色")]
    [DefaultValue(typeof(Color), "[176, 176, 176]")]
    public virtual Color EdgeColor
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
    [HDescriptionLanguage("获取或设置分类器控件的中心颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("居中颜色")]
    [DefaultValue(typeof(Color), "[229, 229, 229]")]
    public virtual Color CenterColor
    {
        get
        {
            return centerColor;
        }
        set
        {
            centerColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置分类器控件的边缘颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框颜色")]
    [DefaultValue(typeof(Color), "[150, 150, 150]")]
    public virtual Color BorderColor
    {
        get
        {
            return boderColor;
        }
        set
        {
            boderColor = value;
            borderPen.Dispose();
            borderPen = new Pen(boderColor);
            Invalidate();
        }
    }

    public HBagFilter()
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
        if (width < 15|| height < 15)
        {
            return;
        }
        if ((float)width > widthDesign / heightDesign * (float)height)
        {
            PaintMain(g, widthDesign / heightDesign * (float)height, height);
        }
        else
        {
            PaintMain(g, width, heightDesign / widthDesign * (float)width);
        }
    }

    /// <summary>PaintRectangle 方法。</summary>
    private void PaintRectangle(Graphics g, RectangleF rectangle, ColorBlend colorBlend)
    {
        rectangle = new RectangleF(rectangle.X * (float)base.Width / widthDesign, rectangle.Y * (float)base.Height / heightDesign, rectangle.Width * (float)base.Width / widthDesign, rectangle.Height * (float)base.Height / heightDesign);
        LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(rectangle.X, 0f), new PointF(rectangle.X + rectangle.Width, 0f), Color.FromArgb(142, 196, 216), Color.FromArgb(240, 240, 240));
        linearGradientBrush.InterpolationColors = colorBlend;
        g.FillRectangle(linearGradientBrush, rectangle);
        g.DrawRectangle(borderPen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        linearGradientBrush.Dispose();
    }

    /// <summary>PaintPolygon 方法。</summary>
    private void PaintPolygon(Graphics g, PointF[] points, ColorBlend colorBlend)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new PointF(points[i].X * (float)base.Width / widthDesign, points[i].Y * (float)base.Height / heightDesign);
        }
        LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(points[0].X, points[0].Y), new PointF(points[1].X, points[0].Y), Color.FromArgb(142, 196, 216), Color.FromArgb(240, 240, 240));
        linearGradientBrush.InterpolationColors = colorBlend;
        g.FillPolygon(linearGradientBrush, points);
        g.DrawPolygon(borderPen, points);
        linearGradientBrush.Dispose();
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, float width, float height)
    {
        ColorBlend colorBlend = new ColorBlend();
        colorBlend.Positions = new float[3]
        {
            0f,
            0.45f,
            1f
        };
        colorBlend.Colors = new Color[3]
        {
            edgeColor,
            centerColor,
            edgeColor
        };
        PaintRectangle(g, new RectangleF(0f, 0f, 50f, 7f), colorBlend);
        PaintRectangle(g, new RectangleF(8f, 7f, 6f, 30f), colorBlend);
        PaintRectangle(g, new RectangleF(38f, 7f, 6f, 30f), colorBlend);
        PaintRectangle(g, new RectangleF(48f, 86f, 504f, 224f), colorBlend);
        PaintRectangle(g, new RectangleF(48f, 310f, 504f, 18f), colorBlend);
        for (int i = 0; i < 10; i++)
        {
            PaintRectangle(g, new RectangleF(92 + 47 * i, 87f, 6f, 222f), colorBlend);
        }
        PaintRectangle(g, new RectangleF(48f, 328f, 18f, 233f), colorBlend);
        PaintRectangle(g, new RectangleF(292f, 328f, 18f, 233f), colorBlend);
        PaintRectangle(g, new RectangleF(537f, 328f, 16f, 233f), colorBlend);
        PaintPolygon(g, new PointF[6]
        {
            new PointF(0f, 35f),
            new PointF(49f, 35f),
            new PointF(49f, 281f),
            new PointF(17f, 281f),
            new PointF(0f, 254f),
            new PointF(0f, 35f)
        }, colorBlend);
        PaintPolygon(g, new PointF[5]
        {
            new PointF(551f, 55f),
            new PointF(635f, 140f),
            new PointF(635f, 226f),
            new PointF(551f, 309f),
            new PointF(551f, 55f)
        }, colorBlend);
        PaintPolygon(g, new PointF[5]
        {
            new PointF(64f, 328f),
            new PointF(292f, 328f),
            new PointF(191f, 502f),
            new PointF(162f, 502f),
            new PointF(64f, 328f)
        }, colorBlend);
        PaintPolygon(g, new PointF[5]
        {
            new PointF(308f, 328f),
            new PointF(536f, 328f),
            new PointF(435f, 502f),
            new PointF(405f, 502f),
            new PointF(308f, 328f)
        }, colorBlend);
        PaintPolygon(g, new PointF[4]
        {
            new PointF(64f, 328f),
            new PointF(97f, 328f),
            new PointF(64f, 361f),
            new PointF(64f, 328f)
        }, colorBlend);
        PaintPolygon(g, new PointF[4]
        {
            new PointF(263f, 328f),
            new PointF(292f, 328f),
            new PointF(292f, 361f),
            new PointF(263f, 328f)
        }, colorBlend);
        PaintPolygon(g, new PointF[4]
        {
            new PointF(308f, 328f),
            new PointF(339f, 328f),
            new PointF(308f, 361f),
            new PointF(308f, 328f)
        }, colorBlend);
        PaintPolygon(g, new PointF[4]
        {
            new PointF(505f, 328f),
            new PointF(537f, 328f),
            new PointF(537f, 361f),
            new PointF(505f, 328f)
        }, colorBlend);
        colorBlend.Colors = new Color[3]
        {
            boderColor,
            centerColor,
            boderColor
        };
        PaintRectangle(g, new RectangleF(49f, 55f, 502f, 31f), colorBlend);
        PaintPolygon(g, new PointF[5]
        {
            new PointF(126f, 439f),
            new PointF(228f, 439f),
            new PointF(220f, 452f),
            new PointF(134f, 452f),
            new PointF(126f, 439f)
        }, colorBlend);
        PaintPolygon(g, new PointF[5]
        {
            new PointF(369f, 439f),
            new PointF(472f, 439f),
            new PointF(464f, 452f),
            new PointF(377f, 452f),
            new PointF(369f, 439f)
        }, colorBlend);
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
        base.Name = "BagFilterH";
        base.Size = new System.Drawing.Size(349, 335);
        ResumeLayout(false);
    }
}
}