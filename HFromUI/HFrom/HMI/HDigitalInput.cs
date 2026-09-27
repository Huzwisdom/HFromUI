using HFromUI.HAttribute;
using HFromUI.HControl;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using HFromUI.HControl.Tools.Button;
namespace HFromUI.HFrom.HMI
{
    using Panel = System.Windows.Forms.Panel;
    [HDescriptionLanguage("数字键盘控件，支持数字，小数点，支持掩码操作，可用来触摸输入一些数据信息")]
public class HDigitalInput : UserControl
{
    /// <summary>sb 字段。</summary>
    private StringBuilder sb = new StringBuilder("0");

    /// <summary>buttonColor 字段。</summary>
    private Color buttonColor = Color.Lavender;

    /// <summary>textForeColor 字段。</summary>
    private Color textForeColor = Color.DimGray;

    /// <summary>enableSpot 字段。</summary>
    private bool enableSpot = true;

    /// <summary>enableNegative 字段。</summary>
    private bool enableNegative = true;

    /// <summary>digitalMask 字段。</summary>
    private char digitalMask = '\0';

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    private SplitContainer splitContainer1;

    private Panel panel1;

    private HLedDisplay LedDisplayH1;

    private TableLayoutPanel tableLayoutPanel1;

    private HPushButton ButtonH15;

    private HPushButton ButtonH14;

    private HPushButton ButtonH13;

    private HPushButton ButtonH12;

    private HPushButton ButtonH11;

    private HPushButton ButtonH10;

    private HPushButton ButtonH9;

    private HPushButton ButtonH8;

    private HPushButton ButtonH7;

    private HPushButton ButtonH6;

    private HPushButton ButtonH5;

    private HPushButton ButtonH4;

    private HPushButton ButtonH3;

    private HPushButton ButtonH2;

    private HPushButton ButtonH1;

    public override Font Font
    {
        get
        {
            return base.Font;
        }
        set
        {
            base.Font = value;
            ButtonH1.Font = value;
            ButtonH2.Font = value;
            ButtonH3.Font = value;
            ButtonH4.Font = value;
            ButtonH5.Font = value;
            ButtonH6.Font = value;
            ButtonH7.Font = value;
            ButtonH8.Font = value;
            ButtonH9.Font = value;
            ButtonH10.Font = value;
            ButtonH11.Font = value;
            ButtonH12.Font = value;
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Action<string> OnOk
    {
        get;
        set;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<string, bool> InputCheck
    {
        get;
        set;
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("指示灯背景颜色")]
    [DefaultValue(typeof(Color), "[46, 46, 46]")]
    public Color LedBackColor
    {
        get
        {
            return LedDisplayH1.BackColor;
        }
        set
        {
            LedDisplayH1.BackColor = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示数字"), Browsable(true)]
    [DefaultValue(8)]
    [HDescriptionLanguage("获取或设置数码管显示的位数")]
    public int DisplayNumber
    {
        get
        {
            return LedDisplayH1.DisplayNumber;
        }
        set
        {
            LedDisplayH1.DisplayNumber = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("指示灯数量大小"), Browsable(true)]
    [DefaultValue(6)]
    [HDescriptionLanguage("获取或设置数码管显示的大小")]
    public int LedNumberSize
    {
        get
        {
            return LedDisplayH1.LedNumberSize;
        }
        set
        {
            LedDisplayH1.LedNumberSize = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示文本"), Browsable(true)]
    [DefaultValue("0")]
    [HDescriptionLanguage("获取或设置数码管显示的内容")]
    public string DisplayText
    {
        get
        {
            return sb.ToString();
        }
        set
        {
            sb = new StringBuilder(value);
            LedDisplayH1.DisplayText = GetMaskTest(value);
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示区背景色"), Browsable(true)]
    [DefaultValue(typeof(Color), "[62, 62, 62]")]
    [HDescriptionLanguage("获取或设置数码管数字的背景色")]
    public Color DisplayBackColor
    {
        get
        {
            return LedDisplayH1.DisplayBackColor;
        }
        set
        {
            LedDisplayH1.DisplayBackColor = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("指示灯前景颜色"), Browsable(true)]
    [DefaultValue(typeof(Color), "Tomato")]
    [HDescriptionLanguage("获取或设置数码管数字的前景色")]
    public Color LedForeColor
    {
        get
        {
            return LedDisplayH1.ForeColor;
        }
        set
        {
            LedDisplayH1.ForeColor = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("单位文本"), Browsable(true)]
    [DefaultValue("")]
    [HDescriptionLanguage("获取或设置数码管数字的单位文本")]
    public string UnitText
    {
        get
        {
            return LedDisplayH1.UnitText;
        }
        set
        {
            LedDisplayH1.UnitText = value;
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("掩码字符"), Browsable(true)]
    [DefaultValue('\0')]
    [HDescriptionLanguage("当需要设置密码的时候，设置为掩码，就不显示真实的数据信息")]
    public char MaskChar
    {
        get
        {
            return digitalMask;
        }
        set
        {
            digitalMask = value;
            DisplayText = sb.ToString();
        }
    }

    [HCategoryLanguage("自定义"), HDisplayNameLanguage("左右偏移"), Browsable(true)]
    [DefaultValue(10)]
    [HDescriptionLanguage("获取或设置数码管两端的空闲长度")]
    public int LeftRightOffect
    {
        get
        {
            return LedDisplayH1.LeftRightOffect;
        }
        set
        {
            LedDisplayH1.LeftRightOffect = value;
        }
    }

    public bool EnableSpot
    {
        get
        {
            return enableSpot;
        }
        set
        {
            enableSpot = value;
            ButtonH12.Enabled = value;
        }
    }

    public bool EnableNegative
    {
        get
        {
            return enableNegative;
        }
        set
        {
            enableNegative = value;
            ButtonH10.Enabled = value;
        }
    }

    [HDescriptionLanguage("获取或设置所有按钮的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("按钮颜色"), Browsable(true)]
    [DefaultValue(typeof(Color), "Lavender")]
    public Color ButtonColor
    {
        get
        {
            return buttonColor;
        }
        set
        {
            buttonColor = value;
            ButtonH1.NormalBackColor = value;
            ButtonH2.NormalBackColor = value;
            ButtonH3.NormalBackColor = value;
            ButtonH4.NormalBackColor = value;
            ButtonH5.NormalBackColor = value;
            ButtonH6.NormalBackColor = value;
            ButtonH7.NormalBackColor = value;
            ButtonH8.NormalBackColor = value;
            ButtonH9.NormalBackColor = value;
            ButtonH10.NormalBackColor = value;
            ButtonH11.NormalBackColor = value;
            ButtonH12.NormalBackColor = value;
            ButtonH13.NormalBackColor = value;
            ButtonH14.NormalBackColor = value;
            ButtonH15.NormalBackColor = value;
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置当前控件的文本的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("前景颜色")]
    [DefaultValue(typeof(Color), "DimGray")]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public override Color ForeColor
    {
        get
        {
            return textForeColor;
        }
        set
        {
            textForeColor = value;
            ButtonH1.NormalForeColor = value;
            ButtonH2.NormalForeColor = value;
            ButtonH3.NormalForeColor = value;
            ButtonH4.NormalForeColor = value;
            ButtonH5.NormalForeColor = value;
            ButtonH6.NormalForeColor = value;
            ButtonH7.NormalForeColor = value;
            ButtonH8.NormalForeColor = value;
            ButtonH9.NormalForeColor = value;
            ButtonH10.NormalForeColor = value;
            ButtonH11.NormalForeColor = value;
            ButtonH12.NormalForeColor = value;
            ButtonH13.NormalForeColor = value;
            ButtonH14.NormalForeColor = value;
            ButtonH15.NormalForeColor = value;
            Invalidate();
        }
    }

    public HDigitalInput()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
        {
            return;
        }
        // 统一数字键盘外观：自定义浅紫底圆角按键，悬停浅蓝
        var border = Color.FromArgb(170, 170, 170);
        foreach (var btn in new[] { ButtonH1, ButtonH2, ButtonH3, ButtonH4, ButtonH5, ButtonH6, ButtonH7,
                                    ButtonH8, ButtonH9, ButtonH10, ButtonH11, ButtonH12, ButtonH13, ButtonH14, ButtonH15 })
        {
            btn.ButtonStyle = HPushButtonStyle.Custom;
            btn.Shape = HPushButtonShape.Round;
            btn.Radius = 4;
            btn.BorderWidth = 1;
            btn.NormalBackColor = buttonColor;
            btn.NormalBorderColor = border;
            btn.HoverBackColor = Color.AliceBlue;
            btn.HoverBorderColor = border;
            btn.PressedBackColor = Color.LightBlue;
            btn.PressedBorderColor = border;
            btn.NormalForeColor = textForeColor;
            btn.HoverForeColor = textForeColor;
            btn.PressedForeColor = textForeColor;
        }
    }

    /// <summary>响应 Load 事件。</summary>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ButtonH1.Click += ButtonH1_Click;
        ButtonH2.Click += ButtonH2_Click;
        ButtonH3.Click += ButtonH3_Click;
        ButtonH4.Click += ButtonH4_Click;
        ButtonH5.Click += ButtonH5_Click;
        ButtonH6.Click += ButtonH6_Click;
        ButtonH7.Click += ButtonH7_Click;
        ButtonH8.Click += ButtonH8_Click;
        ButtonH9.Click += ButtonH9_Click;
        ButtonH10.Click += ButtonH10_Click;
        ButtonH11.Click += ButtonH11_Click;
        ButtonH12.Click += ButtonH12_Click;
        ButtonH13.Click += ButtonH13_Click;
        ButtonH14.Click += ButtonH14_Click;
        ButtonH15.Click += ButtonH15_Click;
        LedDisplayH1.DisplayText = "0";
    }

    /// <summary>ButtonH15_Click 方法。</summary>
    private void ButtonH15_Click(object sender, EventArgs e)
    {
        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
        }
        if (sb.Length == 0)
        {
            sb.Append("0");
        }
        DisplayText = sb.ToString();
    }

    /// <summary>ButtonH14_Click 方法。</summary>
    private void ButtonH14_Click(object sender, EventArgs e)
    {
        sb = new StringBuilder();
        sb.Append("0");
        DisplayText = sb.ToString();
    }

    /// <summary>ButtonH13_Click 方法。</summary>
    private void ButtonH13_Click(object sender, EventArgs e)
    {
        if (sb.ToString().EndsWith("."))
        {
            sb.Remove(sb.Length - 1, 1);
        }
        if (InputCheck != null && !InputCheck(sb.ToString()))
        {
            MessageBox.Show("Input Wrong, Please input again!");
        }
        else
        {
            OnOk?.Invoke(sb.ToString());
        }
    }

    /// <summary>ButtonH12_Click 方法。</summary>
    private void ButtonH12_Click(object sender, EventArgs e)
    {
        if (sb.ToString().IndexOf('.') < 0)
        {
            sb.Append(".");
            DisplayText = sb.ToString();
        }
    }

    /// <summary>ButtonH11_Click 方法。</summary>
    private void ButtonH11_Click(object sender, EventArgs e)
    {
        if (sb.ToString().IndexOf('.') >= 0 || (!sb.ToString().StartsWith("0") && !sb.ToString().StartsWith("-0")))
        {
            sb.Append("0");
            DisplayText = sb.ToString();
        }
    }

    /// <summary>ButtonH10_Click 方法。</summary>
    private void ButtonH10_Click(object sender, EventArgs e)
    {
        if (sb.ToString().StartsWith("-"))
        {
            sb.Remove(0, 1);
        }
        else
        {
            sb.Insert(0, "-");
        }
        DisplayText = sb.ToString();
    }

    /// <summary>LedAddText 方法。</summary>
    private void LedAddText(string number)
    {
        if (sb.ToString().IndexOf(".") <= 0)
        {
            if (sb.ToString().StartsWith("0"))
            {
                sb.Remove(0, 1);
            }
            if (sb.ToString().StartsWith("-0"))
            {
                sb.Remove(1, 1);
            }
        }
        sb.Append(number);
        DisplayText = sb.ToString();
    }

    /// <summary>ButtonH9_Click 方法。</summary>
    private void ButtonH9_Click(object sender, EventArgs e)
    {
        LedAddText("9");
    }

    /// <summary>ButtonH8_Click 方法。</summary>
    private void ButtonH8_Click(object sender, EventArgs e)
    {
        LedAddText("8");
    }

    /// <summary>ButtonH7_Click 方法。</summary>
    private void ButtonH7_Click(object sender, EventArgs e)
    {
        LedAddText("7");
    }

    /// <summary>ButtonH6_Click 方法。</summary>
    private void ButtonH6_Click(object sender, EventArgs e)
    {
        LedAddText("6");
    }

    /// <summary>ButtonH5_Click 方法。</summary>
    private void ButtonH5_Click(object sender, EventArgs e)
    {
        LedAddText("5");
    }

    /// <summary>ButtonH4_Click 方法。</summary>
    private void ButtonH4_Click(object sender, EventArgs e)
    {
        LedAddText("4");
    }

    /// <summary>ButtonH3_Click 方法。</summary>
    private void ButtonH3_Click(object sender, EventArgs e)
    {
        LedAddText("3");
    }

    /// <summary>ButtonH2_Click 方法。</summary>
    private void ButtonH2_Click(object sender, EventArgs e)
    {
        LedAddText("2");
    }

    /// <summary>ButtonH1_Click 方法。</summary>
    private void ButtonH1_Click(object sender, EventArgs e)
    {
        LedAddText("1");
    }

    /// <summary>获取 maskTest。</summary>
    private string GetMaskTest(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        if (digitalMask > '\0')
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsDigit(value[i]))
                {
                    stringBuilder.Append(digitalMask);
                }
                else
                {
                    stringBuilder.Append(value[i]);
                }
            }
            return stringBuilder.ToString();
        }
        return value;
    }

    /// <summary>DigitalInputH_Load 方法。</summary>
    private void DigitalInputH_Load(object sender, EventArgs e)
    {
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
        splitContainer1 = new System.Windows.Forms.SplitContainer();
        panel1 = new System.Windows.Forms.Panel();
        tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        LedDisplayH1 = new HLedDisplay();
        ButtonH15 = new HPushButton();
        ButtonH14 = new HPushButton();
        ButtonH13 = new HPushButton();
        ButtonH12 = new HPushButton();
        ButtonH11 = new HPushButton();
        ButtonH10 = new HPushButton();
        ButtonH9 = new HPushButton();
        ButtonH8 = new HPushButton();
        ButtonH7 = new HPushButton();
        ButtonH6 = new HPushButton();
        ButtonH5 = new HPushButton();
        ButtonH4 = new HPushButton();
        ButtonH3 = new HPushButton();
        ButtonH2 = new HPushButton();
        ButtonH1 = new HPushButton();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        panel1.SuspendLayout();
        tableLayoutPanel1.SuspendLayout();
        SuspendLayout();
        splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        splitContainer1.Location = new System.Drawing.Point(0, 0);
        splitContainer1.Name = "splitContainer1";
        splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
        splitContainer1.Panel1.Controls.Add(panel1);
        splitContainer1.Panel2.Controls.Add(tableLayoutPanel1);
        splitContainer1.Size = new System.Drawing.Size(286, 305);
        splitContainer1.SplitterDistance = 56;
        splitContainer1.TabIndex = 0;
        panel1.Controls.Add(LedDisplayH1);
        panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        panel1.Location = new System.Drawing.Point(0, 0);
        panel1.Name = "panel1";
        panel1.Size = new System.Drawing.Size(286, 56);
        panel1.TabIndex = 0;
        tableLayoutPanel1.ColumnCount = 3;
        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333f));
        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334f));
        tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334f));
        tableLayoutPanel1.Controls.Add(ButtonH15, 0, 4);
        tableLayoutPanel1.Controls.Add(ButtonH14, 0, 4);
        tableLayoutPanel1.Controls.Add(ButtonH13, 0, 4);
        tableLayoutPanel1.Controls.Add(ButtonH12, 2, 3);
        tableLayoutPanel1.Controls.Add(ButtonH11, 1, 3);
        tableLayoutPanel1.Controls.Add(ButtonH10, 0, 3);
        tableLayoutPanel1.Controls.Add(ButtonH9, 2, 2);
        tableLayoutPanel1.Controls.Add(ButtonH8, 1, 2);
        tableLayoutPanel1.Controls.Add(ButtonH7, 0, 2);
        tableLayoutPanel1.Controls.Add(ButtonH6, 2, 1);
        tableLayoutPanel1.Controls.Add(ButtonH5, 1, 1);
        tableLayoutPanel1.Controls.Add(ButtonH4, 0, 1);
        tableLayoutPanel1.Controls.Add(ButtonH3, 2, 0);
        tableLayoutPanel1.Controls.Add(ButtonH2, 1, 0);
        tableLayoutPanel1.Controls.Add(ButtonH1, 0, 0);
        tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
        tableLayoutPanel1.Name = "tableLayoutPanel1";
        tableLayoutPanel1.RowCount = 5;
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20f));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20f));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20f));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20f));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20f));
        tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
        tableLayoutPanel1.Size = new System.Drawing.Size(286, 245);
        tableLayoutPanel1.TabIndex = 0;
        LedDisplayH1.BackColor = System.Drawing.Color.FromArgb(46, 46, 46);
        LedDisplayH1.DisplayBackColor = System.Drawing.Color.FromArgb(62, 62, 62);
        LedDisplayH1.DisplayNumber = 8;
        LedDisplayH1.Dock = System.Windows.Forms.DockStyle.Fill;
        LedDisplayH1.LedNumberSize = 6;
        LedDisplayH1.Location = new System.Drawing.Point(0, 0);
        LedDisplayH1.Name = "LedDisplayH1";
        LedDisplayH1.Size = new System.Drawing.Size(286, 56);
        LedDisplayH1.TabIndex = 0;
        ButtonH15.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH15.Location = new System.Drawing.Point(3, 199);
        ButtonH15.Name = "ButtonH15";
        ButtonH15.Size = new System.Drawing.Size(89, 43);
        ButtonH15.TabIndex = 14;
        ButtonH15.Text = "Back";
        ButtonH14.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH14.Location = new System.Drawing.Point(98, 199);
        ButtonH14.Name = "ButtonH14";
        ButtonH14.Size = new System.Drawing.Size(89, 43);
        ButtonH14.TabIndex = 13;
        ButtonH14.Text = "Clear";
        ButtonH13.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH13.Location = new System.Drawing.Point(193, 199);
        ButtonH13.Name = "ButtonH13";
        ButtonH13.Size = new System.Drawing.Size(90, 43);
        ButtonH13.TabIndex = 12;
        ButtonH13.Text = "OK";
        ButtonH12.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH12.Location = new System.Drawing.Point(193, 150);
        ButtonH12.Name = "ButtonH12";
        ButtonH12.Size = new System.Drawing.Size(90, 43);
        ButtonH12.TabIndex = 11;
        ButtonH12.Text = ".";
        ButtonH11.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH11.Location = new System.Drawing.Point(98, 150);
        ButtonH11.Name = "ButtonH11";
        ButtonH11.Size = new System.Drawing.Size(89, 43);
        ButtonH11.TabIndex = 10;
        ButtonH11.Text = "0";
        ButtonH10.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH10.Location = new System.Drawing.Point(3, 150);
        ButtonH10.Name = "ButtonH10";
        ButtonH10.Size = new System.Drawing.Size(89, 43);
        ButtonH10.TabIndex = 9;
        ButtonH10.Text = "-";
        ButtonH9.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH9.Location = new System.Drawing.Point(193, 101);
        ButtonH9.Name = "ButtonH9";
        ButtonH9.Size = new System.Drawing.Size(90, 43);
        ButtonH9.TabIndex = 8;
        ButtonH9.Text = "9";
        ButtonH8.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH8.Location = new System.Drawing.Point(98, 101);
        ButtonH8.Name = "ButtonH8";
        ButtonH8.Size = new System.Drawing.Size(89, 43);
        ButtonH8.TabIndex = 7;
        ButtonH8.Text = "8";
        ButtonH7.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH7.Location = new System.Drawing.Point(3, 101);
        ButtonH7.Name = "ButtonH7";
        ButtonH7.Size = new System.Drawing.Size(89, 43);
        ButtonH7.TabIndex = 6;
        ButtonH7.Text = "7";
        ButtonH6.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH6.Location = new System.Drawing.Point(193, 52);
        ButtonH6.Name = "ButtonH6";
        ButtonH6.Size = new System.Drawing.Size(90, 43);
        ButtonH6.TabIndex = 5;
        ButtonH6.Text = "6";
        ButtonH5.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH5.Location = new System.Drawing.Point(98, 52);
        ButtonH5.Name = "ButtonH5";
        ButtonH5.Size = new System.Drawing.Size(89, 43);
        ButtonH5.TabIndex = 4;
        ButtonH5.Text = "5";
        ButtonH4.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH4.Location = new System.Drawing.Point(3, 52);
        ButtonH4.Name = "ButtonH4";
        ButtonH4.Size = new System.Drawing.Size(89, 43);
        ButtonH4.TabIndex = 3;
        ButtonH4.Text = "4";
        ButtonH3.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH3.Location = new System.Drawing.Point(193, 3);
        ButtonH3.Name = "ButtonH3";
        ButtonH3.Size = new System.Drawing.Size(90, 43);
        ButtonH3.TabIndex = 2;
        ButtonH3.Text = "3";
        ButtonH2.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH2.Location = new System.Drawing.Point(98, 3);
        ButtonH2.Name = "ButtonH2";
        ButtonH2.Size = new System.Drawing.Size(89, 43);
        ButtonH2.TabIndex = 1;
        ButtonH2.Text = "2";
        ButtonH1.Dock = System.Windows.Forms.DockStyle.Fill;
        ButtonH1.Location = new System.Drawing.Point(3, 3);
        ButtonH1.Name = "ButtonH1";
        ButtonH1.Size = new System.Drawing.Size(89, 43);
        ButtonH1.TabIndex = 0;
        ButtonH1.Text = "1";
        base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        BackColor = System.Drawing.Color.Transparent;
        base.Controls.Add(splitContainer1);
        base.Name = "DigitalInputH";
        base.Size = new System.Drawing.Size(286, 305);
        base.Load += new System.EventHandler(DigitalInputH_Load);
        splitContainer1.Panel1.ResumeLayout(false);
        splitContainer1.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
        splitContainer1.ResumeLayout(false);
        panel1.ResumeLayout(false);
        tableLayoutPanel1.ResumeLayout(false);
        ResumeLayout(false);
    }
}
}