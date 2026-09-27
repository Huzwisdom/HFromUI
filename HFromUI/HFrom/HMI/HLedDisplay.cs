using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("数码管显示控件，支持数字，小数点和特殊的英文字幕显示，支持显示单位")]
public class HLedDisplay : UserControl
{
    /// <summary>背景颜色。</summary>
    private Color backColor = Color.FromArgb(62, 62, 62);

    /// <summary>backBrush 字段。</summary>
    private Brush backBrush = new SolidBrush(Color.FromArgb(62, 62, 62));

    /// <summary>foreBrush 字段。</summary>
    private Brush foreBrush = new SolidBrush(Color.Tomato);

    /// <summary>displayNumber 字段。</summary>
    private int displayNumber = 6;

    /// <summary>ledNumberSize 字段。</summary>
    private int ledNumberSize = 12;

    /// <summary>displayText 字段。</summary>
    private string displayText = "100.0";

    /// <summary>supportChars 字段。</summary>
    private string supportChars = "0123456789AbCcdEFHhJLoPrU: -";

    /// <summary>unitText 字段。</summary>
    private string unitText = string.Empty;

    /// <summary>leftRightOffect 字段。</summary>
    private int leftRightOffect = 10;

    /// <summary>ledPointSize 字段。</summary>
    private int ledPointSize = 2;

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("背景颜色")]
    [DefaultValue(typeof(Color), "[46, 46, 46]")]
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示数字"), Browsable(true)]
    [DefaultValue(6)]
    [HDescriptionLanguage("获取或设置数码管显示的位数")]
    public int DisplayNumber
    {
        get
        {
            return displayNumber;
        }
        set
        {
            if (value >= 0 && value < 100)
            {
                displayNumber = value;
                Invalidate();
            }
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("指示灯数量大小"), Browsable(true)]
    [DefaultValue(12)]
    [HDescriptionLanguage("获取或设置数码管显示的大小")]
    public int LedNumberSize
    {
        get
        {
            return ledNumberSize;
        }
        set
        {
            ledNumberSize = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示文本"), Browsable(true)]
    [DefaultValue("100.0")]
    [HDescriptionLanguage("获取或设置数码管显示的内容")]
    public string DisplayText
    {
        get
        {
            return displayText;
        }
        set
        {
            displayText = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示区背景色"), Browsable(true)]
    [DefaultValue(typeof(Color), "[62, 62, 62]")]
    [HDescriptionLanguage("获取或设置数码管数字的背景色")]
    public Color DisplayBackColor
    {
        get
        {
            return backColor;
        }
        set
        {
            backColor = value;
            backBrush.Dispose();
            backBrush = new SolidBrush(backColor);
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("前景颜色"), Browsable(true)]
    [DefaultValue(typeof(Color), "Tomato")]
    [HDescriptionLanguage("获取或设置数码管数字的前景色")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override Color ForeColor
    {
        get
        {
            return base.ForeColor;
        }
        set
        {
            base.ForeColor = value;
            foreBrush.Dispose();
            foreBrush = new SolidBrush(value);
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("单位文本"), Browsable(true)]
    [DefaultValue("")]
    [HDescriptionLanguage("获取或设置数码管数字的单位文本")]
    public string UnitText
    {
        get
        {
            return unitText;
        }
        set
        {
            unitText = value;
            Invalidate();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左右偏移"), Browsable(true)]
    [DefaultValue(10)]
    [HDescriptionLanguage("获取或设置数码管两端的空闲长度")]
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

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("指示灯点大小"), Browsable(true)]
    [DefaultValue(2)]
    [HDescriptionLanguage("获取或设置数码管小数点的大小")]
    public int LedPointSize
    {
        get
        {
            return ledPointSize;
        }
        set
        {
            ledPointSize = value;
            Invalidate();
        }
    }

    public HLedDisplay()
    {
        InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
        base.ForeColor = Color.Tomato;
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        PaintControlsH(e.Graphics, base.Width, base.Height);
        base.OnPaint(e);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        if (width < 3 || height < 3)
        {
            return;
        }
        int num = leftRightOffect;
        int num2 = leftRightOffect;
        if (!string.IsNullOrEmpty(unitText))
        {
            num2 = 2 + (int)g.MeasureString(unitText, Font).Width;
        }
        float num3 = (float)(width - num - num2) / 1f / (float)displayNumber;
        int num4 = SumCharCount(displayText);
        for (int i = 0; i < displayNumber; i++)
        {
            DrawNumber(g, (int)((float)i * num3) + num, 0, (int)num3, height, ' ', hasSpot: false);
        }
        if (string.IsNullOrEmpty(displayText))
        {
            return;
        }
        if (displayNumber >= num4)
        {
            int num5 = displayNumber - 1;
            for (int num6 = displayText.Length - 1; num6 >= 0; num6--)
            {
                if (supportChars.Contains(displayText[num6]))
                {
                    DrawNumber(g, (int)((float)num5 * num3) + num, 0, (int)num3, height, displayText[num6], IsCharAfterSpot(displayText, num6));
                    num5--;
                    if (num5 < 0)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            int num7 = 0;
            for (int j = 0; j < displayText.Length; j++)
            {
                if (supportChars.Contains(displayText[j]))
                {
                    DrawNumber(g, (int)((float)num7 * num3) + num, 0, (int)num3, height, displayText[j], IsCharAfterSpot(displayText, j));
                    num7++;
                    if (num7 >= displayNumber)
                    {
                        break;
                    }
                }
            }
        }
        if (!string.IsNullOrEmpty(unitText))
        {
            g.DrawString(unitText, Font, foreBrush, width - num2 - 2, 3f);
        }
    }

    /// <summary>SumCharCount 方法。</summary>
    private int SumCharCount(string value)
    {
        int num = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (supportChars.Contains(value[i]))
            {
                num++;
            }
        }
        return num;
    }

    /// <summary>判断是否 CharAfterSpot。</summary>
    private bool IsCharAfterSpot(string value, int index)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }
        if (index >= value.Length - 1)
        {
            return false;
        }
        if (value[index + 1] == '.')
        {
            return true;
        }
        return false;
    }

    /// <summary>DrawHorizontalItem 方法。</summary>
    private void DrawHorizontalItem(Graphics g, Brush brush, int x, int y, int width)
    {
        Point[] points = new Point[7]
        {
            new Point(x, y),
            new Point(x - ledNumberSize / 2, y + ledNumberSize / 2),
            new Point(x, y + ledNumberSize),
            new Point(x + width, y + ledNumberSize),
            new Point(x + width + ledNumberSize / 2, y + ledNumberSize / 2),
            new Point(x + width, y),
            new Point(x, y)
        };
        g.FillPolygon(brush, points);
    }

    /// <summary>DrawVerticalItem 方法。</summary>
    private void DrawVerticalItem(Graphics g, Brush brush, int x, int y, int height)
    {
        Point[] points = new Point[7]
        {
            new Point(x, y),
            new Point(x, y + height),
            new Point(x + ledNumberSize / 2, y + height + ledNumberSize / 2),
            new Point(x + ledNumberSize, y + height),
            new Point(x + ledNumberSize, y),
            new Point(x + ledNumberSize / 2, y - ledNumberSize / 2),
            new Point(x, y)
        };
        g.FillPolygon(brush, points);
    }

    /// <summary>DrawNumber 方法。</summary>
    private void DrawNumber(Graphics g, int x, int y, int width, int height, char charValue, bool hasSpot)
    {
        bool[] array = new bool[9];
        if (charValue == supportChars[0])
        {
            bool[] obj = new bool[9]
            {
                true,
                true,
                true,
                false,
                true,
                true,
                true,
                false,
                false
            };
            obj[7] = hasSpot;
            array = obj;
        }
        else if (charValue == supportChars[1])
        {
            array = new bool[9]
            {
                false,
                false,
                true,
                false,
                false,
                true,
                false,
                hasSpot,
                false
            };
        }
        else if (charValue == supportChars[2])
        {
            bool[] obj2 = new bool[9]
            {
                true,
                false,
                true,
                true,
                true,
                false,
                true,
                false,
                false
            };
            obj2[7] = hasSpot;
            array = obj2;
        }
        else if (charValue == supportChars[3])
        {
            bool[] obj3 = new bool[9]
            {
                true,
                false,
                true,
                true,
                false,
                true,
                true,
                false,
                false
            };
            obj3[7] = hasSpot;
            array = obj3;
        }
        else if (charValue == supportChars[4])
        {
            bool[] obj4 = new bool[9]
            {
                false,
                true,
                true,
                true,
                false,
                true,
                false,
                false,
                false
            };
            obj4[7] = hasSpot;
            array = obj4;
        }
        else if (charValue == supportChars[5])
        {
            bool[] obj5 = new bool[9]
            {
                true,
                true,
                false,
                true,
                false,
                true,
                true,
                false,
                false
            };
            obj5[7] = hasSpot;
            array = obj5;
        }
        else if (charValue == supportChars[6])
        {
            bool[] obj6 = new bool[9]
            {
                true,
                true,
                false,
                true,
                true,
                true,
                true,
                false,
                false
            };
            obj6[7] = hasSpot;
            array = obj6;
        }
        else if (charValue == supportChars[7])
        {
            bool[] obj7 = new bool[9]
            {
                true,
                false,
                true,
                false,
                false,
                true,
                false,
                false,
                false
            };
            obj7[7] = hasSpot;
            array = obj7;
        }
        else if (charValue == supportChars[8])
        {
            bool[] obj8 = new bool[9]
            {
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                false,
                false
            };
            obj8[7] = hasSpot;
            array = obj8;
        }
        else if (charValue == supportChars[9])
        {
            bool[] obj9 = new bool[9]
            {
                true,
                true,
                true,
                true,
                false,
                true,
                true,
                false,
                false
            };
            obj9[7] = hasSpot;
            array = obj9;
        }
        else if (charValue == supportChars[10])
        {
            bool[] obj10 = new bool[9]
            {
                true,
                true,
                true,
                true,
                true,
                true,
                false,
                false,
                false
            };
            obj10[7] = hasSpot;
            array = obj10;
        }
        else if (charValue == supportChars[11])
        {
            bool[] obj11 = new bool[9]
            {
                false,
                true,
                false,
                true,
                true,
                true,
                true,
                false,
                false
            };
            obj11[7] = hasSpot;
            array = obj11;
        }
        else if (charValue == supportChars[12])
        {
            bool[] obj12 = new bool[9]
            {
                true,
                true,
                false,
                false,
                true,
                false,
                true,
                false,
                false
            };
            obj12[7] = hasSpot;
            array = obj12;
        }
        else if (charValue == supportChars[13])
        {
            bool[] obj13 = new bool[9]
            {
                false,
                false,
                false,
                true,
                true,
                false,
                true,
                false,
                false
            };
            obj13[7] = hasSpot;
            array = obj13;
        }
        else if (charValue == supportChars[14])
        {
            bool[] obj14 = new bool[9]
            {
                false,
                false,
                true,
                true,
                true,
                true,
                true,
                false,
                false
            };
            obj14[7] = hasSpot;
            array = obj14;
        }
        else if (charValue == supportChars[15])
        {
            bool[] obj15 = new bool[9]
            {
                true,
                true,
                false,
                true,
                true,
                false,
                true,
                false,
                false
            };
            obj15[7] = hasSpot;
            array = obj15;
        }
        else if (charValue == supportChars[16])
        {
            bool[] obj16 = new bool[9]
            {
                true,
                true,
                false,
                true,
                true,
                false,
                false,
                false,
                false
            };
            obj16[7] = hasSpot;
            array = obj16;
        }
        else if (charValue == supportChars[17])
        {
            bool[] obj17 = new bool[9]
            {
                false,
                true,
                true,
                true,
                true,
                true,
                false,
                false,
                false
            };
            obj17[7] = hasSpot;
            array = obj17;
        }
        else if (charValue == supportChars[18])
        {
            bool[] obj18 = new bool[9]
            {
                false,
                true,
                false,
                true,
                true,
                true,
                false,
                false,
                false
            };
            obj18[7] = hasSpot;
            array = obj18;
        }
        else if (charValue == supportChars[19])
        {
            bool[] obj19 = new bool[9]
            {
                false,
                false,
                true,
                false,
                false,
                true,
                true,
                false,
                false
            };
            obj19[7] = hasSpot;
            array = obj19;
        }
        else if (charValue == supportChars[20])
        {
            bool[] obj20 = new bool[9]
            {
                false,
                true,
                false,
                false,
                true,
                false,
                true,
                false,
                false
            };
            obj20[7] = hasSpot;
            array = obj20;
        }
        else if (charValue == supportChars[21])
        {
            bool[] obj21 = new bool[9]
            {
                false,
                false,
                false,
                true,
                true,
                true,
                true,
                false,
                false
            };
            obj21[7] = hasSpot;
            array = obj21;
        }
        else if (charValue == supportChars[22])
        {
            bool[] obj22 = new bool[9]
            {
                true,
                true,
                true,
                true,
                true,
                false,
                false,
                false,
                false
            };
            obj22[7] = hasSpot;
            array = obj22;
        }
        else if (charValue == supportChars[23])
        {
            array = new bool[9]
            {
                false,
                false,
                false,
                true,
                true,
                false,
                false,
                hasSpot,
                false
            };
        }
        else if (charValue == supportChars[24])
        {
            bool[] obj23 = new bool[9]
            {
                false,
                true,
                true,
                false,
                true,
                true,
                true,
                false,
                false
            };
            obj23[7] = hasSpot;
            array = obj23;
        }
        else if (charValue == supportChars[25])
        {
            array = new bool[9]
            {
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                true
            };
        }
        else if (charValue == supportChars[26])
        {
            array = new bool[9];
        }
        else if (charValue == supportChars[27])
        {
            array = new bool[9]
            {
                false,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                false
            };
        }
        DrawNumber(g, x, y, width, height, array);
    }

    /// <summary>DrawNumber 方法。</summary>
    private void DrawNumber(Graphics g, int x, int y, int width, int height, bool[] array)
    {
        if (array == null || array.Length < 9)
        {
            return;
        }
        int num = 5;
        int num2 = 5;
        int num3 = 3;
        int num4 = 4;
        int num5 = 2;
        if (ledNumberSize < 6)
        {
            num3 = 2;
            num4 = 3;
            num5 = 1;
        }
        if (ledNumberSize > 12)
        {
            num3 = 5;
            num4 = 6;
            num5 = 3;
        }
        int width2 = width - num3 - num4 - 2 * ledNumberSize - 2 * num5;
        int num6 = (height - num - num2 - 3 * ledNumberSize - 4 * num5) / 2;
        if (array[0])
        {
            DrawHorizontalItem(g, foreBrush, x + num3 + ledNumberSize + num5, y + num, width2);
        }
        else
        {
            DrawHorizontalItem(g, backBrush, x + num3 + ledNumberSize + num5, y + num, width2);
        }
        if (array[1])
        {
            DrawVerticalItem(g, foreBrush, x + num3, y + num + ledNumberSize + num5, num6);
        }
        else
        {
            DrawVerticalItem(g, backBrush, x + num3, y + num + ledNumberSize + num5, num6);
        }
        if (array[2])
        {
            DrawVerticalItem(g, foreBrush, x + width - num4 - ledNumberSize, y + num + ledNumberSize + num5, num6);
        }
        else
        {
            DrawVerticalItem(g, backBrush, x + width - num4 - ledNumberSize, y + num + ledNumberSize + num5, num6);
        }
        if (array[3])
        {
            DrawHorizontalItem(g, foreBrush, x + num3 + ledNumberSize + num5, y + num + ledNumberSize + num6 + 2 * num5, width2);
        }
        else
        {
            DrawHorizontalItem(g, backBrush, x + num3 + ledNumberSize + num5, y + num + ledNumberSize + num6 + 2 * num5, width2);
        }
        if (array[4])
        {
            DrawVerticalItem(g, foreBrush, x + num3, y + num + 2 * ledNumberSize + 3 * num5 + num6, num6);
        }
        else
        {
            DrawVerticalItem(g, backBrush, x + num3, y + num + 2 * ledNumberSize + 3 * num5 + num6, num6);
        }
        if (array[5])
        {
            DrawVerticalItem(g, foreBrush, x + width - num4 - ledNumberSize, y + num + 2 * ledNumberSize + 3 * num5 + num6, num6);
        }
        else
        {
            DrawVerticalItem(g, backBrush, x + width - num4 - ledNumberSize, y + num + 2 * ledNumberSize + 3 * num5 + num6, num6);
        }
        if (array[6])
        {
            DrawHorizontalItem(g, foreBrush, x + num3 + ledNumberSize + num5, y + height - num2 - ledNumberSize, width2);
        }
        else
        {
            DrawHorizontalItem(g, backBrush, x + num3 + ledNumberSize + num5, y + height - num2 - ledNumberSize, width2);
        }
        if (x + width - num5 * 2 < base.Width - 20)
        {
            if (array[7])
            {
                g.FillEllipse(foreBrush, x + width - ledPointSize, y + height - num2 - ledPointSize * 2 + 1, ledPointSize * 2, ledPointSize * 2);
            }
            else
            {
                g.FillEllipse(backBrush, x + width - ledPointSize, y + height - num2 - ledPointSize * 2 + 1, ledPointSize * 2, ledPointSize * 2);
            }
        }
        if (array[8])
        {
            Rectangle rect = new Rectangle(x + width / 2 - num5 * 2, y + num + ledNumberSize + num5 + num6 - num5 * 4, num5 * 4, num5 * 4);
            g.FillEllipse(foreBrush, rect);
            rect.Y += num5 + num5 + ledNumberSize + num5 + num5 * 4;
            g.FillEllipse(foreBrush, rect);
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
        BackColor = System.Drawing.Color.FromArgb(46, 46, 46);
        base.Name = "LedDisplayH";
        base.Size = new System.Drawing.Size(325, 58);
        ResumeLayout(false);
    }
}
}