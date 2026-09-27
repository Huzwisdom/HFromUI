using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一种加工中心的控件，支持开关门，信号灯的显示")]
public class HMachineCenter : UserControl
{
    /// <summary>doorOpenStatus 字段。</summary>
    private bool doorOpenStatus = false;

    /// <summary>lightRed 字段。</summary>
    private bool lightRed = false;

    /// <summary>lightYellow 字段。</summary>
    private bool lightYellow = false;

    /// <summary>lightGreen 字段。</summary>
    private bool lightGreen = false;

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
    [HDescriptionLanguage("门的开关状态")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("门打开状态")]
    [DefaultValue(false)]
    public bool DoorOpenStatus
    {
        get
        {
            return doorOpenStatus;
        }
        set
        {
            doorOpenStatus = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("是否显示红灯的信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("红灯")]
    [DefaultValue(false)]
    public bool LightRed
    {
        get
        {
            return lightRed;
        }
        set
        {
            lightRed = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("是否显示黄灯的信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("黄灯")]
    [DefaultValue(false)]
    public bool LightYellow
    {
        get
        {
            return lightYellow;
        }
        set
        {
            lightYellow = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("是否显示绿灯的信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绿灯")]
    [DefaultValue(false)]
    public bool LightGreen
    {
        get
        {
            return lightGreen;
        }
        set
        {
            lightGreen = value;
            Invalidate();
        }
    }

    public HMachineCenter()
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
        PaintMain(g, width, height);
    }

    /// <summary>获取 pointsFrom。</summary>
    private PointF[] GetPointsFrom(string points, float width, float height, float dx = 0f, float dy = 0f)
    {
        return FromHelper.GetPointsFrom(points, 999f, 999f, width, height, dx, dy);
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, float width, float height)
    {
        float width2 = (width / 300f > 1f) ? (width / 300f) : 1f;
        g.TranslateTransform(20f / 333f * width, 2f / 111f * height);
        using (Pen pen = new Pen(Color.Black, (width / 300f > 1f) ? (width / 300f) : 1f))
        {
            PointF[] pointsFrom = GetPointsFrom("52,322 52,250 32,250 32,60 78,60 78,250 58,250 58,322", width, height);
            using (Brush brush = new SolidBrush(Color.FromArgb(76, 76, 76)))
            {
                g.FillPolygon(brush, pointsFrom);
                g.DrawLines(pen, pointsFrom);
            }
            using (Brush brush2 = new SolidBrush(Color.FromArgb(LightRed ? 255 : 32, 255, 0, 0)))
            {
                g.FillPolygon(brush2, GetPointsFrom("35,63 75,63 75,124 35,124 35,63", width, height));
            }
            using (Brush brush3 = new SolidBrush(Color.FromArgb(LightYellow ? 255 : 32, 255, 255, 0)))
            {
                g.FillPolygon(brush3, GetPointsFrom("35,124 75,124 75,185 35,185", width, height));
            }
            using (Brush brush4 = new SolidBrush(Color.FromArgb(LightGreen ? 255 : 32, 0, 255, 0)))
            {
                g.FillPolygon(brush4, GetPointsFrom("35,185 75,185 75,247 35,247", width, height));
            }
        }
        g.TranslateTransform(-20f / 333f * width, -2f / 111f * height);
        using (Brush brush5 = new SolidBrush(Color.FromArgb(229, 229, 229)))
        {
            g.FillPolygon(brush5, GetPointsFrom("46,343 27,814 46,950 343,971 655,971 952,950 971,814 952,343 46,343", width, height));
        }
        using (Brush brush6 = new SolidBrush(Color.FromArgb(224, 224, 224)))
        {
            g.FillPolygon(brush6, GetPointsFrom("27,814 46,950 343,971 655,971 952,950 971,814 27,814", width, height));
        }
        using (Brush brush7 = new SolidBrush(Color.FromArgb(50, 178, 178)))
        {
            g.FillPolygon(brush7, GetPointsFrom("33,765 965,765 965,735 33,735 33,765", width, height));
        }
        g.FillPolygon(Brushes.Black, GetPointsFrom("416,961 420,961 420,965 416,965 416,961", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("416,912 420,912 420,918 416,918 416,912", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("416,865 420,865 420,871 416,871 416,865", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("416,818 420,818 420,822 416,822 416,818", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("416,769 420,769 420,774 416,774 416,769", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("597,961 603,961 603,965 597,965 597,961", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("597,912 603,912 603,918 597,918 597,912", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("597,865 603,865 603,871 597,871 597,865", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("597,818 603,818 603,822 597,822 597,818", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("597,769 603,769 603,774 597,774 597,769", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("482,769 488,769 488,774 482,774 482,769", width, height));
        g.FillPolygon(Brushes.Black, GetPointsFrom("508,769 514,769 514,774 508,774 508,769", width, height));
        using (Pen pen2 = new Pen(Color.FromArgb(178, 178, 178), width2))
        {
            g.DrawLines(pen2, GetPointsFrom("612,971 612,765", width, height));
            g.DrawLines(pen2, GetPointsFrom("401,971 401,765", width, height));
            g.DrawLines(pen2, GetPointsFrom("46,931 401,931", width, height));
            g.DrawLines(pen2, GetPointsFrom("616,931 952,931", width, height));
        }
        using (Pen pen3 = new Pen(Color.FromArgb(255, 255, 255), width2))
        {
            g.DrawLines(pen3, GetPointsFrom("616,971 616,765", width, height));
            g.DrawLines(pen3, GetPointsFrom("405,971 405,765", width, height));
            g.DrawLines(pen3, GetPointsFrom("695,380 695,695 303,695", width, height));
        }
        using (Pen pen4 = new Pen(Color.FromArgb(76, 76, 76), width2))
        {
            g.DrawLines(pen4, GetPointsFrom("303,695 303,380 695,380", width, height));
        }
        using (Brush brush8 = new SolidBrush(Color.FromArgb(229, 229, 229)))
        {
            g.FillPolygon(brush8, GetPointsFrom("361,27 518,27 518,343 361,343 361,27", width, height));
        }
        using (Brush brush9 = new SolidBrush(Color.FromArgb(224, 224, 224)))
        {
            g.FillPolygon(brush9, GetPointsFrom("361,250 518,120 518,343 361,343 361,250", width, height));
        }
        using (Brush brush10 = new SolidBrush(Color.FromArgb(219, 219, 219)))
        {
            g.FillPolygon(brush10, GetPointsFrom("361,263 518,133 518,343 361,343 361,263", width, height));
        }
        using (Brush brush11 = new SolidBrush(Color.FromArgb(214, 214, 214)))
        {
            g.FillPolygon(brush11, GetPointsFrom("361,284 518,152 518,343 361,343 361,284", width, height));
        }
        using (Pen pen5 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen5, GetPointsFrom("361,27 518,27 518,343 361,343 361,27", width, height));
        }
        using (Brush brush12 = new SolidBrush(Color.FromArgb(229, 229, 229)))
        {
            g.FillPolygon(brush12, GetPointsFrom("205,282 361,282 361,343 205,343 205,282", width, height));
        }
        using (Pen pen6 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen6, GetPointsFrom("46,343 27,814 46,950 343,971 655,971 952,950 971,814 952,343 46,343", width, height));
        }
        if (doorOpenStatus)
        {
            g.FillPolygon(Brushes.White, GetPointsFrom("303,695 342,676 656,676 695,695 303,695", width, height));
            using (Pen pen7 = new Pen(Color.FromArgb(0, 0, 0), width2))
            {
                g.DrawLines(pen7, GetPointsFrom("303,695 342,676 656,676 695,695 303,695", width, height));
            }
            using (Brush brush13 = new SolidBrush(Color.FromArgb(204, 204, 204)))
            {
                g.FillPolygon(brush13, GetPointsFrom("303,695 342,676 342,401 303,380 303,695", width, height));
            }
            using (Pen pen8 = new Pen(Color.FromArgb(0, 0, 0), width2))
            {
                g.DrawLines(pen8, GetPointsFrom("303,695 342,676 342,401 303,380 303,695", width, height));
            }
            using (Brush brush14 = new SolidBrush(Color.FromArgb(204, 204, 204)))
            {
                g.FillPolygon(brush14, GetPointsFrom("695,695 657,676 657,401 695,380 695,695", width, height));
            }
            using (Pen pen9 = new Pen(Color.FromArgb(0, 0, 0), width2))
            {
                g.DrawLines(pen9, GetPointsFrom("695,695 657,676 657,401 695,380 695,695", width, height));
            }
            using (Brush brush15 = new SolidBrush(Color.FromArgb(178, 178, 178)))
            {
                g.FillPolygon(brush15, GetPointsFrom("303,380 342,401 656,401 695,380 303,380", width, height));
            }
            using (Pen pen10 = new Pen(Color.FromArgb(0, 0, 0), width2))
            {
                g.DrawLines(pen10, GetPointsFrom("303,380 342,401 656,401 695,380 303,380", width, height));
            }
            using (Brush brush16 = new SolidBrush(Color.FromArgb(217, 217, 217)))
            {
                g.FillPolygon(brush16, GetPointsFrom("695,695 665,695 665,380 695,382 695,695", width, height));
                g.FillPolygon(brush16, GetPointsFrom("303,380 331,380 331,695 303,695 303,380", width, height));
            }
            g.FillPolygon(Brushes.Black, GetPointsFrom("312,578 312,499 322,499 322,578 312,578", width, height));
            g.FillPolygon(Brushes.Black, GetPointsFrom("676,578 676,499 686,499 686,578 676,578", width, height));
            using (Pen pen11 = new Pen(Color.FromArgb(127, 127, 127), width2))
            {
                g.DrawLines(pen11, GetPointsFrom("665,695 665,382", width, height));
                g.DrawLines(pen11, GetPointsFrom("331,380 331,695", width, height));
            }
            using (Pen pen12 = new Pen(Color.FromArgb(255, 255, 255), width2))
            {
                g.DrawLines(pen12, GetPointsFrom("695,382 695,695 303,695", width, height));
            }
            using (Pen pen13 = new Pen(Color.FromArgb(76, 76, 76), width2))
            {
                g.DrawLines(pen13, GetPointsFrom("303,695 303,382 695,382", width, height));
            }
        }
        else
        {
            using (Pen pen14 = new Pen(Color.FromArgb(178, 178, 178), width2))
            {
                g.DrawLines(pen14, GetPointsFrom("499,380 499,695", width, height));
            }
            g.FillPolygon(Brushes.Black, GetPointsFrom("480,578 480,499 490,499 490,578 480,578", width, height));
            g.FillPolygon(Brushes.Black, GetPointsFrom("508,578 508,499 518,499 518,578 508,578", width, height));
            using (Brush brush17 = new SolidBrush(Color.FromArgb(51, 51, 51)))
            {
                g.FillPolygon(brush17, GetPointsFrom("556,650 544,639 544,435 558,424 658,424 669,435 671,639 659,650 556,650", width, height));
                g.FillPolygon(brush17, GetPointsFrom("339,650 327,639 329,435 340,424 440,424 452,435 454,639 442,650 339,650", width, height));
            }
            using (Pen pen15 = new Pen(Color.FromArgb(178, 178, 178), width2))
            {
                g.DrawLines(pen15, GetPointsFrom("350,595 433,488", width, height));
                g.DrawLines(pen15, GetPointsFrom("359,542 416,471", width, height));
                g.DrawLines(pen15, GetPointsFrom("339,476 371,440", width, height));
                g.DrawLines(pen15, GetPointsFrom("565,595 648,488", width, height));
                g.DrawLines(pen15, GetPointsFrom("577,542 633,471", width, height));
                g.DrawLines(pen15, GetPointsFrom("556,476 586,440", width, height));
            }
        }
        using (Brush brush18 = new SolidBrush(Color.FromArgb(178, 178, 178)))
        {
            g.FillPolygon(brush18, GetPointsFrom("401,144 480,144 480,224 401,224 401,144", width, height));
        }
        using (Pen pen16 = new Pen(Color.FromArgb(127, 127, 127), width2))
        {
            g.DrawLines(pen16, GetPointsFrom("401,144 480,144 480,224 401,224 401,144", width, height));
        }
        using (Brush brush19 = new SolidBrush(Color.FromArgb(178, 178, 178)))
        {
            g.FillPolygon(brush19, GetPointsFrom("263,282 303,282 303,303 263,303 263,282", width, height));
        }
        using (Pen pen17 = new Pen(Color.FromArgb(127, 127, 127), width2))
        {
            g.DrawLines(pen17, GetPointsFrom("263,282 303,282 303,303 263,303 263,282", width, height));
        }
        using (Pen pen18 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen18, GetPointsFrom("205,282 361,282 361,343 205,343 205,282", width, height));
            g.DrawLines(pen18, GetPointsFrom("704,493 704,508 793,518 793,499", width, height));
        }
        using (Brush brush20 = new SolidBrush(Color.FromArgb(229, 229, 229)))
        {
            g.FillPolygon(brush20, GetPointsFrom("518,243 729,214 729,243 518,263 518,243", width, height));
        }
        using (Pen pen19 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen19, GetPointsFrom("518,243 729,214 729,243 518,263 518,243", width, height));
        }
        using (Brush brush21 = new SolidBrush(Color.FromArgb(255, 255, 255)))
        {
            g.FillPolygon(brush21, GetPointsFrom("769,214 729,214 729,243 769,243 769,214", width, height));
        }
        using (Pen pen20 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen20, GetPointsFrom("769,214 729,214 729,243 769,243 769,214", width, height));
        }
        using (Brush brush22 = new SolidBrush(Color.FromArgb(204, 204, 204)))
        {
            g.FillPolygon(brush22, GetPointsFrom("765,244 735,244 735,376 765,376 765,244", width, height));
        }
        using (Brush brush23 = new SolidBrush(Color.FromArgb(229, 229, 229)))
        {
            g.FillPolygon(brush23, GetPointsFrom("761,244 739,244 739,376 761,376 761,244", width, height));
        }
        using (Brush brush24 = new SolidBrush(Color.FromArgb(242, 242, 242)))
        {
            g.FillPolygon(brush24, GetPointsFrom("757,244 742,244 742,376 757,376 757,244", width, height));
        }
        using (Brush brush25 = new SolidBrush(Color.FromArgb(255, 255, 255)))
        {
            g.FillPolygon(brush25, GetPointsFrom("754,244 746,244 746,376 754,376 754,244", width, height));
        }
        using (Pen pen21 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen21, GetPointsFrom("765,244 735,244 735,376 765,376 765,244", width, height));
        }
        using (Brush brush26 = new SolidBrush(Color.FromArgb(204, 204, 204)))
        {
            g.FillPolygon(brush26, GetPointsFrom("686,376 804,367 804,505 686,493 686,376", width, height));
        }
        using (Pen pen22 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen22, GetPointsFrom("686,376 804,367 804,505 686,493 686,376", width, height));
        }
        using (Brush brush27 = new SolidBrush(Color.FromArgb(255, 255, 255)))
        {
            g.FillPolygon(brush27, GetPointsFrom("823,376 804,367 804,505 823,493 823,376", width, height));
        }
        using (Pen pen23 = new Pen(Color.FromArgb(0, 0, 0), width2))
        {
            g.DrawLines(pen23, GetPointsFrom("823,376 804,367 804,505 823,493 823,376", width, height));
        }
        using (Brush brush28 = new SolidBrush(Color.FromArgb(102, 102, 102)))
        {
            g.FillPolygon(brush28, GetPointsFrom("706,388 782,382 782,471 706,463 706,388", width, height));
        }
        using (Brush brush29 = new SolidBrush(Color.FromArgb(51, 51, 51)))
        {
            g.FillPolygon(brush29, GetPointsFrom("710,391 778,386 778,465 710,459 710,391", width, height));
        }
        using (Pen pen24 = new Pen(Color.FromArgb(127, 127, 127), width2))
        {
            g.DrawLines(pen24, GetPointsFrom("686,469 803,480", width, height));
            g.DrawLines(pen24, GetPointsFrom("741,499 741,475", width, height));
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
        base.Name = "MachineCenterH";
        base.Size = new System.Drawing.Size(372, 293);
        ResumeLayout(false);
    }
}
}