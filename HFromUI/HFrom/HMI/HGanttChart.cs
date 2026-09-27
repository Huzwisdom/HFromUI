using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个简单的甘特图控件，可以用来表示一天或是一个月里面各个环节占用的时间信息")]
public class HGanttChart : UserControl
{
    /// <summary>数据。</summary>
    private int[] data = new int[0];

    /// <summary>colors 字段。</summary>
    private Color[] colors = new Color[0];

    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>timeStart 字段。</summary>
    private int timeStart = 0;

    /// <summary>timeMax 字段。</summary>
    private int timeMax = 24;

    /// <summary>timeCount 字段。</summary>
    private int timeCount = 86400;

    /// <summary>timeSegment 字段。</summary>
    private int timeSegment = 6;

    /// <summary>timeFormate 字段。</summary>
    private string timeFormate = "{0}H";

    /// <summary>ganttBackColor 字段。</summary>
    private Color ganttBackColor = Color.LightGray;

    /// <summary>ganttBrush 字段。</summary>
    private Brush ganttBrush = new SolidBrush(Color.LightGray);

    /// <summary>leftRightOffect 字段。</summary>
    private int leftRightOffect = 20;

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
    [HDescriptionLanguage("获取或设置甘特图的背景颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("甘特图背景色")]
    [DefaultValue(typeof(Color), "LightGray")]
    public Color GanttBackColor
    {
        get
        {
            return ganttBackColor;
        }
        set
        {
            ganttBackColor = value;
            ganttBrush.Dispose();
            ganttBrush = new SolidBrush(ganttBackColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置甘特图的时间的分段信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("时间分段")]
    [DefaultValue(6)]
    public int TimeSegment
    {
        get
        {
            return timeSegment;
        }
        set
        {
            timeSegment = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置时间的起始信息，通常一天的起始为0，一个月的起始为1，年份的起始为自然年，比如2018")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("时间起始")]
    [DefaultValue(0)]
    public int TimeStart
    {
        get
        {
            return timeStart;
        }
        set
        {
            timeStart = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置时间的结束信息，通常一天的结束为24，一个月的结束为29,30,31，年份的结束为自然年，比如2019")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("时间最大")]
    [DefaultValue(24)]
    public int TimeMax
    {
        get
        {
            return timeMax;
        }
        set
        {
            timeMax = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置时间的总数信息，比如是按照天计算的甘特图，设置了1440，就是按照分钟计数，如果是86400，就是按照秒计数，这个传入的值密切相关")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("时间数量")]
    [DefaultValue(86400)]
    public int TimeCount
    {
        get
        {
            return timeCount;
        }
        set
        {
            timeCount = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置时间的显示格式，可以设置为显示小时，分钟，天等等信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("时间格式")]
    [DefaultValue("{0}H")]
    public string TimeFormate
    {
        get
        {
            return timeFormate;
        }
        set
        {
            timeFormate = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置两侧的距离宽度，当你的数字范围特别长的时候就有用，默认为20")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左右偏移")]
    [DefaultValue(20)]
    public int LeftRightOffect
    {
        get
        {
            return leftRightOffect;
        }
        set
        {
            leftRightOffect = value;
            Invalidate();
        }
    }

    public HGanttChart()
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
        int num = leftRightOffect;
        int num2 = Font.Height + 2;
        Brush brush = new SolidBrush(ForeColor);
        for (int i = 0; i <= timeSegment; i++)
        {
            int num3 = (timeMax - timeStart) * i / timeSegment + timeStart;
            float num4 = (width - (float)(2 * num)) * (float)i / (float)timeSegment + (float)num;
            g.DrawString(string.Format(timeFormate, num3), Font, brush, new Rectangle((int)num4 - 50, 0, 100, num2), sf);
        }
        g.SmoothingMode = SmoothingMode.None;
        RectangleF rect = new RectangleF(num, num2, width - (float)(2 * num), height - (float)num2);
        g.FillRectangle(ganttBrush, rect);
        int num5 = 0;
        for (int j = 0; j < data.Length; j++)
        {
            float num6 = (float)num5 * (width - (float)(2 * num)) / (float)timeCount + (float)num;
            float num7 = (float)data[j] * (width - (float)(2 * num)) / (float)timeCount;
            if (num7 > rect.Width - num6 + (float)num)
            {
                num7 = rect.Width - num6 + (float)num;
            }
            if (j < colors.Length)
            {
                using (Brush brush2 = new SolidBrush(colors[j]))
                {
                    g.FillRectangle(brush2, new RectangleF(num6, num2, num7, rect.Height));
                }
            }
            num5 += data[j];
            if (num5 > timeCount)
            {
                break;
            }
        }
        brush.Dispose();
    }

    /// <summary>设置 ganttChart。</summary>
    public void SetGanttChart(int[] data, Color[] colors)
    {
        if (data != null && colors != null)
        {
            this.data = data;
            this.colors = colors;
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
        base.Name = "GanttChartH";
        base.Size = new System.Drawing.Size(333, 65);
        ResumeLayout(false);
    }
}
}