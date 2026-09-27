using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个水泵控件，允许设置进口和出口的位置，以及是否转动的操作")]
public class HPumpOne : UserControl
{
    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>ValvesStyleH 字段。</summary>
    private DirectionStyleH ValvesStyleH = DirectionStyleH.Horizontal;

    /// <summary>entrance 字段。</summary>
    private int entrance = 1;

    /// <summary>export 字段。</summary>
    private int export = 4;

    /// <summary>pen_1 字段。</summary>
    private Pen pen_1 = new Pen(Color.FromArgb(92, 100, 111), 3f);

    /// <summary>color1 字段。</summary>
    private Color color1 = Color.FromArgb(209, 218, 227);

    /// <summary>brush1 字段。</summary>
    private Brush brush1 = new SolidBrush(Color.FromArgb(209, 218, 227));

    /// <summary>color2 字段。</summary>
    private Color color2 = Color.FromArgb(157, 164, 173);

    /// <summary>brush2 字段。</summary>
    private Brush brush2 = new SolidBrush(Color.FromArgb(157, 164, 173));

    /// <summary>color3 字段。</summary>
    private Color color3 = Color.FromArgb(195, 200, 207);

    /// <summary>brush3 字段。</summary>
    private Brush brush3 = new SolidBrush(Color.FromArgb(195, 200, 207));

    /// <summary>color4 字段。</summary>
    private Color color4 = Color.FromArgb(204, 208, 214);

    /// <summary>brush4 字段。</summary>
    private Brush brush4 = new SolidBrush(Color.FromArgb(204, 208, 214));

    /// <summary>color5 字段。</summary>
    private Color color5 = Color.FromArgb(208, 213, 220);

    /// <summary>brush5 字段。</summary>
    private Brush brush5 = new SolidBrush(Color.FromArgb(208, 213, 220));

    /// <summary>color6 字段。</summary>
    private Color color6 = Color.FromArgb(153, 160, 169);

    /// <summary>brush6 字段。</summary>
    private Brush brush6 = new SolidBrush(Color.FromArgb(153, 160, 169));

    /// <summary>color7 字段。</summary>
    private Color color7 = Color.FromArgb(92, 100, 111);

    /// <summary>brush7 字段。</summary>
    private Brush brush7 = new SolidBrush(Color.FromArgb(92, 100, 111));

    /// <summary>moveSpeed 字段。</summary>
    private float moveSpeed = 0.3f;

    /// <summary>startAngle 字段。</summary>
    private float startAngle = 0f;

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
    [DefaultValue(typeof(DirectionStyleH), "Horizontal")]
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
    [DefaultValue(typeof(Color), "[199, 205, 211]")]
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
    [DefaultValue(typeof(Color), "[135, 144, 156]")]
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
    [DefaultValue(typeof(Color), "[208, 213, 220]")]
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
    [DefaultValue(typeof(Color), "[153, 160, 169]")]
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

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色5")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 5")]
    [DefaultValue(typeof(Color), "[92, 100, 111]")]
    public Color Color5
    {
        get
        {
            return color5;
        }
        set
        {
            color5 = value;
            brush5.Dispose();
            brush5 = new SolidBrush(value);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色6")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 6")]
    [DefaultValue(typeof(Color), "[108, 114, 121]")]
    public Color Color6
    {
        get
        {
            return color6;
        }
        set
        {
            color6 = value;
            brush6.Dispose();
            brush6 = new SolidBrush(value);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置颜色7")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("颜色 7")]
    [DefaultValue(typeof(Color), "[158, 165, 173]")]
    public Color Color7
    {
        get
        {
            return color7;
        }
        set
        {
            color7 = value;
            brush7.Dispose();
            brush7 = new SolidBrush(value);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("入口管道的位置")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("入口")]
    [DefaultValue(1)]
    public int Entrance
    {
        get
        {
            return entrance;
        }
        set
        {
            if (value > 0 && value < 7)
            {
                entrance = value;
                Invalidate();
            }
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("出口管道的位置")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("导出")]
    [DefaultValue(4)]
    public int Export
    {
        get
        {
            return export;
        }
        set
        {
            if (value > 0 && value < 7)
            {
                export = value;
                Invalidate();
            }
        }
    }

    public HPumpOne()
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
        float num = Math.Min(width, height);
        if (ValvesStyleH == DirectionStyleH.Horizontal)
        {
            PaintMain(g, num, num);
            return;
        }
        g.TranslateTransform(width, 0f);
        g.RotateTransform(90f);
        PaintMain(g, num, num);
        g.ResetTransform();
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, float width, float height)
    {
        g.TranslateTransform(width / 2f, height / 2f);
        DrawPipe(g, width, height, entrance);
        DrawPipe(g, width, height, export);
        PointF[] points = new PointF[4]
        {
            new PointF(0f, height * 0.1f),
            new PointF((0f - width) * 0.25f, height * 0.5f - 1f),
            new PointF(width * 0.25f, height * 0.5f - 1f),
            new PointF(0f, height * 0.1f)
        };
        g.FillPolygon(brush1, points);
        g.FillRectangle(brush2, new RectangleF((0f - width) * 0.28f, height * 0.46f, width * 0.56f, height * 0.04f));
        g.FillEllipse(brush3, new RectangleF((0f - width) * 0.3f, (0f - height) * 0.3f, width * 0.6f, height * 0.6f));
        g.FillEllipse(brush4, new RectangleF((0f - width) * 0.24f, (0f - height) * 0.24f, width * 0.48f, height * 0.48f));
        g.RotateTransform(startAngle);
        if (width < 50f)
        {
            pen_1.Width = 1f;
        }
        else if (width < 100f)
        {
            pen_1.Width = 2f;
        }
        else
        {
            pen_1.Width = 3f;
        }
        for (int i = 0; i < 4; i++)
        {
            g.DrawLine(pen_1, (0f - width) * 0.2f, 0f, width * 0.2f, 0f);
            g.RotateTransform(45f);
        }
        g.RotateTransform(-180f);
        g.RotateTransform(0f - startAngle);
        g.FillEllipse(brush5, (0f - width) * 0.09f, (0f - width) * 0.09f, width * 0.18f, width * 0.18f);
        g.FillEllipse(brush6, (0f - width) * 0.08f, (0f - width) * 0.08f, width * 0.16f, width * 0.16f);
        g.FillEllipse(brush5, (0f - width) * 0.02f, (0f - width) * 0.02f, width * 0.04f, width * 0.04f);
        g.FillEllipse(brush7, (0f - width) * 0.01f, (0f - width) * 0.01f, width * 0.02f, width * 0.02f);
        using (Brush brush = new SolidBrush(ForeColor))
        {
            g.DrawString(Text, Font, brush, new RectangleF((0f - width) / 2f, height * 0.38f, width, height * 0.55f), sf);
        }
        g.TranslateTransform((0f - width) / 2f, (0f - height) / 2f);
    }

    /// <summary>DrawPipe 方法。</summary>
    private void DrawPipe(Graphics g, float width, float height, int direction)
    {
        ColorBlend colorBlend = new ColorBlend();
        colorBlend.Positions = new float[3]
        {
            0f,
            0.4f,
            1f
        };
        colorBlend.Colors = new Color[3]
        {
            color6,
            Color.WhiteSmoke,
            color6
        };
        float num = width * 0.02f;
        if (num < 3f)
        {
            num = 3f;
        }
        float num2 = height * 0.25f + 10f;
        if (direction == 1 || direction == 2 || direction == 5 || direction == 6)
        {
            RectangleF rect = default(RectangleF);
            RectangleF rect2 = default(RectangleF);
            switch (direction)
            {
                case 1:
                    rect = new RectangleF((0f - width) / 2f + 1f, height * 0.05f, width * 0.5f, height * 0.25f);
                    rect2 = new RectangleF((0f - width) / 2f, height * 0.05f - 5f, num, num2);
                    break;
                case 2:
                    rect = new RectangleF((0f - width) / 2f + 1f, (0f - height) * 0.3f, width * 0.5f, height * 0.25f);
                    rect2 = new RectangleF((0f - width) / 2f, (0f - height) * 0.3f - 5f, num, num2);
                    break;
                case 5:
                    rect = new RectangleF(0f, (0f - height) * 0.3f, width * 0.5f, height * 0.25f);
                    rect2 = new RectangleF(width * 0.5f - num, (0f - height) * 0.3f - 5f, num, num2);
                    break;
                case 6:
                    rect = new RectangleF(0f, height * 0.05f, width * 0.5f, height * 0.25f);
                    rect2 = new RectangleF(width * 0.5f - num, height * 0.05f - 5f, num, num2);
                    break;
            }
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(rect.Location.X, rect.Location.Y + rect.Height), new PointF(rect.Location.X, rect.Location.Y), Color.Wheat, Color.White);
            linearGradientBrush.InterpolationColors = colorBlend;
            g.FillRectangle(linearGradientBrush, rect);
            linearGradientBrush.Dispose();
            g.FillRectangle(Brushes.DimGray, rect2);
        }
        else if (direction == 3 || direction == 4)
        {
            RectangleF rect3 = default(RectangleF);
            RectangleF rect4 = default(RectangleF);
            switch (direction)
            {
                case 3:
                    rect3 = new RectangleF((0f - width) * 0.3f, (0f - height) * 0.5f, width * 0.25f, height * 0.5f);
                    rect4 = new RectangleF((0f - width) * 0.3f - 5f, (0f - height) * 0.5f, num2, num);
                    break;
                case 4:
                    rect3 = new RectangleF(width * 0.05f, (0f - height) * 0.5f, width * 0.25f, height * 0.5f);
                    rect4 = new RectangleF(width * 0.05f - 5f, (0f - height) * 0.5f, num2, num);
                    break;
            }
            LinearGradientBrush linearGradientBrush2 = new LinearGradientBrush(new PointF(rect3.Location.X, rect3.Location.Y), new PointF(rect3.Location.X + rect3.Width, rect3.Location.Y), Color.Wheat, Color.White);
            linearGradientBrush2.InterpolationColors = colorBlend;
            g.FillRectangle(linearGradientBrush2, rect3);
            linearGradientBrush2.Dispose();
            g.FillRectangle(Brushes.DimGray, rect4);
        }
    }

    /// <summary>Timer_Tick 方法。</summary>
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (moveSpeed != 0f)
        {
            for (startAngle = (float)((double)startAngle + (double)(moveSpeed * 180f) / Math.PI / 10.0); startAngle <= -360f; startAngle += 360f)
            {
            }
            while (startAngle >= 360f)
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
        base.Name = "PumpOneH";
        base.Size = new System.Drawing.Size(162, 131);
        ResumeLayout(false);
    }
}
}