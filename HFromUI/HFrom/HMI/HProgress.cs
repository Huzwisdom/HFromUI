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
    [HDescriptionLanguage("进度条控件，支持横向，竖向，圆形三种样式")]
public class HProgress : UserControl
{
    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>backBrush 字段。</summary>
    private Brush backBrush = new SolidBrush(Color.DimGray);

    /// <summary>progressColor 字段。</summary>
    private Color progressColor = Color.Tomato;

    /// <summary>progressBrush 字段。</summary>
    private Brush progressBrush = new SolidBrush(Color.Tomato);

    /// <summary>borderColor 字段。</summary>
    private Color borderColor = Color.DimGray;

    /// <summary>borderPen 字段。</summary>
    private Pen borderPen = new Pen(Color.DimGray, 1f);

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

    /// <summary>textRenderFormat 字段。</summary>
    private string textRenderFormat = string.Empty;

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
            return progressColor;
        }
        set
        {
            progressColor = value;
            progressBrush?.Dispose();
            progressBrush = new SolidBrush(value);
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
    [DefaultValue(typeof(Color), "DimGray")]
    public Color BorderColor
    {
        get
        {
            return borderColor;
        }
        set
        {
            borderColor = value;
            borderPen?.Dispose();
            borderPen = new Pen(value);
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

    [HDescriptionLanguage("获取或设置自定义的格式化文本信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本绘制格式")]
    [Browsable(true)]
    [DefaultValue("")]
    public string TextRenderFormat
    {
        get
        {
            return textRenderFormat;
        }
        set
        {
            textRenderFormat = value;
            Invalidate();
        }
    }

    public HProgress()
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
        PaintControlsH(e.Graphics, base.Width, base.Height);
        base.OnPaint(e);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        try
        {
            g.FillRectangle(rect: new Rectangle(0, 0, width - 1, height - 1), brush: backBrush);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            switch (m_progressStyle)
            {
                case ProgressStyleH.Vertical:
                    {
                        int num2 = (int)((long)m_actual * (long)(height - 2) / max);
                        g.FillRectangle(rect: new Rectangle(0, height - 1 - num2, width - 1, num2), brush: progressBrush);
                        break;
                    }
                case ProgressStyleH.Horizontal:
                    {
                        int num = (int)((long)m_actual * (long)(width - 2) / max);
                        g.FillRectangle(rect: new Rectangle(0, 0, num + 1, height - 1), brush: progressBrush);
                        break;
                    }
            }
            Rectangle rectangle = new Rectangle(0, 0, width - 1, height - 1);
            if (isTextRender)
            {
                string s = string.IsNullOrEmpty(textRenderFormat) ? (((long)m_actual * 100L / max).ToString() + "%") : string.Format(textRenderFormat, Value, Max);
                using (Brush brush = new SolidBrush(ForeColor))
                {
                    if (m_progressStyle != ProgressStyleH.Circular)
                    {
                        g.DrawString(s, Font, brush, rectangle, sf);
                    }
                    else
                    {
                        g.DrawString("Not supported", Font, brush, rectangle, sf);
                    }
                }
            }
            if (m_progressStyle != ProgressStyleH.Circular)
            {
                g.DrawRectangle(borderPen, rectangle);
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
        BackColor = System.Drawing.Color.DimGray;
        base.Name = "ProgressH";
        base.Size = new System.Drawing.Size(120, 97);
        ResumeLayout(false);
    }
}
}