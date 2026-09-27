using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    public class CProgressBar : ProgressBar
{
    /// <summary>progressStyle 字段。</summary>
    private ProgressStyle progressStyle = ProgressStyle.None;

    /// <summary>progressColor 字段。</summary>
    private Color progressColor = Color.Transparent;

    /// <summary>inflateWidth 字段。</summary>
    private int inflateWidth = 3;

    /// <summary>useBackColor 字段。</summary>
    private bool useBackColor = false;

    /// <summary>useSmallSquares 字段。</summary>
    private bool useSmallSquares = false;

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置显示的文本样式")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度样式")]
    [DefaultValue(typeof(ProgressStyle), "None")]
    public ProgressStyle ProgressStyle
    {
        get
        {
            return progressStyle;
        }
        set
        {
            progressStyle = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件缩进的宽度信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("外扩宽度")]
    [DefaultValue(3)]
    public int InflateWidth
    {
        get
        {
            return inflateWidth;
        }
        set
        {
            inflateWidth = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置进度条的前景色，如果为透明，则使用系统的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度颜色")]
    [DefaultValue(typeof(Color), "Transparent")]
    public Color ProgressColor
    {
        get
        {
            return progressColor;
        }
        set
        {
            progressColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置是否使用BackColor颜色来绘制背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("使用背景颜色")]
    [DefaultValue(false)]
    public bool UseBackColor
    {
        get
        {
            return useBackColor;
        }
        set
        {
            useBackColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置是否使用小方块来表示进度条的显示效果，当 ProgressColor 不为 Color.Transparent 时有效")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("使用小方块")]
    [DefaultValue(false)]
    public bool UseSmallSquares
    {
        get
        {
            return useSmallSquares;
        }
        set
        {
            useSmallSquares = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    public Func<int, int, string> CustmerTextTranslate
    {
        get;
        set;
    }

    public CProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
    }

    /// <summary>SizeToString 方法。</summary>
    private string SizeToString(long size, long sizeBase = -1L)
    {
        if (sizeBase < 0)
        {
            sizeBase = size;
        }
        if (sizeBase < 1024)
        {
            return size.ToString() + " B";
        }
        if (sizeBase < 1048576)
        {
            return ((float)size / 1024f).ToString("F1") + " K";
        }
        return ((float)size / 1048576f).ToString("F2") + " M";
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle clientRectangle = base.ClientRectangle;
        Graphics graphics = e.Graphics;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        PaintControlsH(graphics, base.ClientRectangle.Width, base.ClientRectangle.Height);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        Rectangle rectangle = new Rectangle(0, 0, width, height);
        if (useBackColor)
        {
            using (Brush brush = new SolidBrush(BackColor))
            {
                g.FillRectangle(brush, rectangle);
                using (Pen pen = new Pen(FromHelper.GetColorLight(BackColor, 30)))
                {
                    g.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                }
            }
        }
        else
        {
            ProgressBarRenderer.DrawHorizontalBar(g, rectangle);
        }
        rectangle.Inflate(-InflateWidth, -InflateWidth);
        if (base.Value > 0)
        {
            Rectangle rectangle2 = new Rectangle(rectangle.X, rectangle.Y, (int)Math.Round((double)base.Value * (double)rectangle.Width / (double)base.Maximum), rectangle.Height);
            if (progressColor == Color.Transparent)
            {
                ProgressBarRenderer.DrawHorizontalChunks(g, rectangle2);
            }
            else
            {
                using (Brush brush2 = new SolidBrush(progressColor))
                {
                    if (UseSmallSquares)
                    {
                        int i;
                        for (i = 0; i + 8 <= rectangle2.Width; i += 10)
                        {
                            g.FillRectangle(brush2, new Rectangle(rectangle2.X + i, rectangle2.Y, 8, rectangle2.Height));
                        }
                        int num = rectangle2.Width - i;
                        if (num > 0)
                        {
                            g.FillRectangle(brush2, new Rectangle(rectangle2.X + i, rectangle2.Y, num, rectangle2.Height));
                        }
                    }
                    else
                    {
                        g.FillRectangle(brush2, rectangle2);
                    }
                }
            }
        }
        using (Brush brush3 = new SolidBrush(ForeColor))
        {
            if (progressStyle != 0)
            {
                if (progressStyle == ProgressStyle.Number)
                {
                    string s = $"{base.Value}/{base.Maximum}";
                    g.DrawString(s, Font, brush3, rectangle, FromHelper.StringFormatCenter);
                }
                else if (progressStyle == ProgressStyle.Size)
                {
                    string s2 = SizeToString(base.Value, base.Maximum) + "/" + SizeToString(base.Maximum, -1L);
                    g.DrawString(s2, Font, brush3, rectangle, FromHelper.StringFormatCenter);
                }
                else if (progressStyle == ProgressStyle.Percent)
                {
                    string s3 = ((int)((long)base.Value * 100L / base.Maximum)).ToString() + "%";
                    g.DrawString(s3, Font, brush3, rectangle, FromHelper.StringFormatCenter);
                }
                else if (progressStyle == ProgressStyle.Customer)
                {
                    string text = (CustmerTextTranslate == null) ? string.Empty : CustmerTextTranslate(base.Value, base.Maximum);
                    if (!string.IsNullOrEmpty(text))
                    {
                        g.DrawString(text, Font, brush3, rectangle, FromHelper.StringFormatCenter);
                    }
                }
            }
        }
    }
}
}