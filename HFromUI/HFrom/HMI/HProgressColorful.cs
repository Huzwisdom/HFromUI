using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("进度条控件，支持横向，竖向两种方向，支持颜色渐变色")]
public class HProgressColorful : UserControl
{
    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>colorBorder 字段。</summary>
    private Color colorBorder = Color.Silver;

    /// <summary>colorCenter 字段。</summary>
    private Color colorCenter = Color.WhiteSmoke;

    /// <summary>colorTmp 字段。</summary>
    private Color colorTmp = Color.Tomato;

    /// <summary>backBrush 字段。</summary>
    private Brush backBrush = new SolidBrush(Color.DimGray);

    /// <summary>max 字段。</summary>
    private int max = 100;

    /// <summary>m_value 字段。</summary>
    private int m_value = 50;

    /// <summary>m_actual 字段。</summary>
    private int m_actual = 50;

    /// <summary>m_speed 字段。</summary>
    private int m_speed = 1;

    /// <summary>useAnimation 字段。</summary>
    private bool useAnimation = false;

    /// <summary>m_version 字段。</summary>
    private int m_version = 0;

    /// <summary>m_progressStyle 字段。</summary>
    private ProgressStyleH m_progressStyle = ProgressStyleH.Vertical;

    /// <summary>isTextRender 字段。</summary>
    private bool isTextRender = true;

    private Action m_UpdateAction;

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    [HDescriptionLanguage("获取或设置进度条的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("背景颜色")]
    [DefaultValue(typeof(Color), "DimGray")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    public override Color BackColor
    {
        get
        {
            return base.BackColor;
        }
        set
        {
            base.BackColor = value;
            backBrush?.Dispose();
            backBrush = new SolidBrush(value);
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置进度条的前景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "Tomato")]
    public Color ProgressColor
    {
        get
        {
            return colorTmp;
        }
        set
        {
            colorTmp = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置进度条的最大值，默认为100")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("最大")]
    [Browsable(true)]
    [DefaultValue(100)]
    public int Max
    {
        get
        {
            return max;
        }
        set
        {
            if (value > 1)
            {
                max = value;
            }
            if (m_value > max)
            {
                m_value = max;
            }
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置当前进度条的值")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("值")]
    [Browsable(true)]
    [DefaultValue(50)]
    public int Value
    {
        get
        {
            return m_value;
        }
        set
        {
            if (value >= 0 && value <= max && value != m_value)
            {
                m_value = value;
                if (UseAnimation)
                {
                    int num = Interlocked.Increment(ref m_version);
                    ThreadPool.QueueUserWorkItem(ThreadPoolUpdateProgress, num);
                }
                else
                {
                    m_actual = value;
                    Invalidate();
                }
            }
        }
    }

    [HDescriptionLanguage("获取或设置是否显示进度文本")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制文本")]
    [Browsable(true)]
    [DefaultValue(true)]
    public bool IsTextRender
    {
        get
        {
            return isTextRender;
        }
        set
        {
            isTextRender = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置进度条的边框颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "Silver")]
    public Color BorderColor
    {
        get
        {
            return colorBorder;
        }
        set
        {
            colorBorder = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置进度条中间的过渡色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("居中颜色")]
    [Browsable(true)]
    [DefaultValue(typeof(Color), "WhiteSmoke")]
    public Color CenterColor
    {
        get
        {
            return colorCenter;
        }
        set
        {
            colorCenter = value;
            Invalidate();
        }
    }

    [HDescriptionLanguage("获取或设置进度条的变化进度")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("数值变化速度")]
    [Browsable(true)]
    [DefaultValue(1)]
    public int ValueChangeSpeed
    {
        get
        {
            return m_speed;
        }
        set
        {
            if (value >= 1)
            {
                m_speed = value;
            }
        }
    }

    [HDescriptionLanguage("获取或设置进度条变化的时候是否采用动画效果")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("使用动画")]
    [Browsable(true)]
    [DefaultValue(false)]
    public bool UseAnimation
    {
        get
        {
            return useAnimation;
        }
        set
        {
            useAnimation = value;
        }
    }

    [HDescriptionLanguage("获取或设置进度条的样式")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度样式")]
    [Browsable(true)]
    [DefaultValue(typeof(ProgressStyleH), "Vertical")]
    public ProgressStyleH ProgressStyle
    {
        get
        {
            return m_progressStyle;
        }
        set
        {
            m_progressStyle = value;
            Invalidate();
        }
    }

    public HProgressColorful()
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
        m_UpdateAction = UpdateRender;
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
        try
        {
            g.FillRectangle(rect: new Rectangle(0, 0, width - 1, height - 1), brush: backBrush);
            ColorBlend colorBlend = new ColorBlend();
            colorBlend.Positions = new float[3]
            {
                    0f,
                    0.8f,
                    1f
            };
            colorBlend.Colors = new Color[3]
            {
                    colorBorder,
                    colorCenter,
                    colorBorder
            };
            switch (m_progressStyle)
            {
                case ProgressStyleH.Vertical:
                    {
                        LinearGradientBrush linearGradientBrush2 = new LinearGradientBrush(new PointF(0f, 0f), new PointF(width - 1, 0f), colorBorder, colorCenter);
                        linearGradientBrush2.InterpolationColors = colorBlend;
                        g.FillEllipse(linearGradientBrush2, 0, 0, width - 1, width - 1);
                        g.FillRectangle(linearGradientBrush2, 0f, (float)width / 2f, width - 1, height - width);
                        colorBlend.Colors = new Color[3]
                        {
                        colorTmp,
                        FromHelper.GetColorLightFive(colorTmp),
                        colorTmp
                        };
                        linearGradientBrush2.InterpolationColors = colorBlend;
                        float num2 = (int)((long)m_actual * (long)(height - width) / max);
                        g.FillRectangle(linearGradientBrush2, 0f, (float)height - num2 - 1f - (float)(width / 2), width - 1, num2);
                        g.FillEllipse(linearGradientBrush2, 0f, (float)height - num2 - (float)width, width - 1, width - 1);
                        g.FillEllipse(linearGradientBrush2, 0, height - width, width - 1, width - 1);
                        linearGradientBrush2.Dispose();
                        break;
                    }
                case ProgressStyleH.Horizontal:
                    {
                        LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new PointF(0f, 0f), new PointF(0f, height - 1), colorBorder, colorCenter);
                        linearGradientBrush.InterpolationColors = colorBlend;
                        g.FillEllipse(linearGradientBrush, width - height, 0, height - 1, height - 1);
                        g.FillRectangle(linearGradientBrush, height / 2, 0, width - height - 1, height - 1);
                        colorBlend.Colors = new Color[3]
                        {
                        colorTmp,
                        FromHelper.GetColorLightFive(colorTmp),
                        colorTmp
                        };
                        linearGradientBrush.InterpolationColors = colorBlend;
                        float num = (int)((long)m_actual * (long)(width - height) / max);
                        g.FillRectangle(linearGradientBrush, height / 2, 0f, num, height - 1);
                        g.FillEllipse(linearGradientBrush, num, 0f, height - 1, height - 1);
                        g.FillEllipse(linearGradientBrush, 0, 0, height - 1, height - 1);
                        linearGradientBrush.Dispose();
                        break;
                    }
            }
            Rectangle r = new Rectangle(0, 0, width - 1, height - 1);
            if (isTextRender)
            {
                string s = ((long)m_actual * 100L / max).ToString() + "%";
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    if (m_progressStyle != ProgressStyleH.Circular)
                    {
                        g.DrawString(s, Font, brush, r, sf);
                    }
                    else
                    {
                        g.DrawString("Not supported", Font, brush, r, sf);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>ThreadPoolUpdateProgress 方法。</summary>
    private void ThreadPoolUpdateProgress(object obj)
    {
        try
        {
            int num = (int)obj;
            if (m_speed < 1)
            {
                m_speed = 1;
            }
            while (m_actual != m_value)
            {
                Thread.Sleep(17);
                if (num != m_version)
                {
                    break;
                }
                int num2 = 0;
                if (m_actual > m_value)
                {
                    int num3 = m_actual - m_value;
                    if (num3 > m_speed)
                    {
                        num3 = m_speed;
                    }
                    num2 = m_actual - num3;
                }
                else
                {
                    int num4 = m_value - m_actual;
                    if (num4 > m_speed)
                    {
                        num4 = m_speed;
                    }
                    num2 = m_actual + num4;
                }
                m_actual = num2;
                if (num != m_version)
                {
                    break;
                }
                if (base.IsHandleCreated)
                {
                    Invoke(m_UpdateAction);
                }
            }
        }
        catch (Exception)
        {
        }
    }

    /// <summary>UpdateRender 方法。</summary>
    private void UpdateRender()
    {
        Invalidate();
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
        base.Name = "ProgressColorfulH";
        base.Size = new System.Drawing.Size(421, 17);
        ResumeLayout(false);
    }
}
}