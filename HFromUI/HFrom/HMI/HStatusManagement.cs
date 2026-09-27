using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个状态集显示控件，例如用来显示10×20仓库库位信息，还可以设置每个单元的提示消息")]
public class HStatusManagement : UserControl
{
    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>row 字段。</summary>
    private int row = 2;

    /// <summary>col 字段。</summary>
    private int col = 10;

    /// <summary>offect 字段。</summary>
    private float offect = 4f;

    private Color[,] arrayColor;

    private string[,] HDescriptionLanguages;

    /// <summary>borderColor 字段。</summary>
    private Color borderColor = Color.DimGray;

    /// <summary>borderPen 字段。</summary>
    private Pen borderPen = new Pen(Color.DimGray);

    /// <summary>isRenderBorder 字段。</summary>
    private bool isRenderBorder = true;

    /// <summary>renderStyle 字段。</summary>
    private RenderStyleH renderStyle = RenderStyleH.Rectangle;

    /// <summary>defaultColor 字段。</summary>
    private Color defaultColor = Color.LightGray;

    /// <summary>activePoint 字段。</summary>
    private Point activePoint = new Point(-1, -1);

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
    [HDescriptionLanguage("获取或设置状态间距信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("间距偏移")]
    [DefaultValue(4f)]
    public float SpacingOffect
    {
        get
        {
            return offect;
        }
        set
        {
            offect = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置状态集的行数信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("行数量")]
    [DefaultValue(2)]
    public int RowCount
    {
        get
        {
            return row;
        }
        set
        {
            row = value;
            ArrayInni();
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置状态集的列数信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("列数")]
    [DefaultValue(10)]
    public int ColCount
    {
        get
        {
            return col;
        }
        set
        {
            col = value;
            ArrayInni();
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置是否显示边框信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制边框")]
    [DefaultValue(true)]
    public bool IsRenderBorder
    {
        get
        {
            return isRenderBorder;
        }
        set
        {
            isRenderBorder = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置状态集的显示样式，支持矩形，椭圆，菱形")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制样式")]
    [DefaultValue(RenderStyleH.Rectangle)]
    public RenderStyleH RenderStyle
    {
        get
        {
            return renderStyle;
        }
        set
        {
            renderStyle = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置所有状态集合体的默认背景色，设置本数据将会强制所有的颜色恢复初始化")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("常规颜色")]
    [DefaultValue(typeof(Color), "LightGray")]
    public Color GeneralColor
    {
        get
        {
            return defaultColor;
        }
        set
        {
            defaultColor = value;
            ArrayInni();
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置所有状态集合体的边框颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框颜色")]
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
            borderPen.Dispose();
            borderPen = new Pen(borderColor);
            Invalidate();
        }
    }

    public HStatusManagement()
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
        ArrayInni();
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        PaintControlsH(graphics, base.Width, base.Height);
        base.OnPaint(e);
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        PaintMain(g, width, height);
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, float width, float height)
    {
        float num = (width - (float)col * offect) / (float)col;
        float num2 = (height - (float)row * offect) / (float)row;
        if (renderStyle == RenderStyleH.Rectangle)
        {
            g.SmoothingMode = SmoothingMode.None;
        }
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                Rectangle rect = new Rectangle((int)(offect / 2f + (num + offect) * (float)j), (int)(offect / 2f + (num2 + offect) * (float)i), (int)num, (int)num2);
                using (Brush brush = new SolidBrush(arrayColor[i, j]))
                {
                    if (renderStyle == RenderStyleH.Rectangle)
                    {
                        g.FillRectangle(brush, rect);
                        if (isRenderBorder)
                        {
                            g.DrawRectangle(borderPen, rect);
                        }
                    }
                    else if (renderStyle == RenderStyleH.Ellipse)
                    {
                        g.FillEllipse(brush, rect);
                        if (isRenderBorder)
                        {
                            g.DrawEllipse(borderPen, rect);
                        }
                    }
                    else
                    {
                        g.FillPolygon(brush, FromHelper.GetRhombusFromRectangle(rect));
                        if (isRenderBorder)
                        {
                            g.DrawPolygon(borderPen, FromHelper.GetRhombusFromRectangle(rect));
                        }
                    }
                }
            }
        }
    }

    /// <summary>设置 colorByLocation。</summary>
    public void SetColorByLocation(int rowIndex, int colIndex, Color color)
    {
        arrayColor[rowIndex, colIndex] = color;
        Invalidate();
    }

    /// <summary>ResetColorByLocation 方法。</summary>
    public void ResetColorByLocation(int rowIndex, int colIndex)
    {
        arrayColor[rowIndex, colIndex] = defaultColor;
        Invalidate();
    }

    /// <summary>设置 colorByLocation。</summary>
    public void SetColorByLocation(Point point, Color color)
    {
        SetColorByLocation(point.X, point.Y, color);
    }

    /// <summary>ResetColorByLocation 方法。</summary>
    public void ResetColorByLocation(Point point)
    {
        ResetColorByLocation(point.X, point.Y);
    }

    /// <summary>设置 colorByLocation。</summary>
    public void SetColorByLocation(Point[] points, Color color)
    {
        if (points != null)
        {
            for (int i = 0; i < points.Length; i++)
            {
                arrayColor[points[i].X, points[i].Y] = color;
            }
            Invalidate();
        }
    }

    /// <summary>ResetColorByLocation 方法。</summary>
    public void ResetColorByLocation(Point[] points)
    {
        if (points != null)
        {
            for (int i = 0; i < points.Length; i++)
            {
                arrayColor[points[i].X, points[i].Y] = defaultColor;
            }
            Invalidate();
        }
    }

    /// <summary>设置 colorByRow。</summary>
    public void SetColorByRow(int rowIndex, Color color)
    {
        for (int i = 0; i < col; i++)
        {
            arrayColor[rowIndex, i] = color;
        }
        Invalidate();
    }

    /// <summary>设置 colorByRow。</summary>
    public void SetColorByRow(int rowIndex, Color[] colors)
    {
        for (int i = 0; i < col; i++)
        {
            if (i < colors.Length)
            {
                arrayColor[rowIndex, i] = colors[i];
            }
        }
        Invalidate();
    }

    /// <summary>ResetColorByRow 方法。</summary>
    public void ResetColorByRow(int rowIndex)
    {
        for (int i = 0; i < col; i++)
        {
            arrayColor[rowIndex, i] = defaultColor;
        }
        Invalidate();
    }

    /// <summary>设置 colorByCol。</summary>
    public void SetColorByCol(int colIndex, Color color)
    {
        for (int i = 0; i < row; i++)
        {
            arrayColor[i, colIndex] = color;
        }
        Invalidate();
    }

    /// <summary>设置 colorByCol。</summary>
    public void SetColorByCol(int colIndex, Color[] colors)
    {
        for (int i = 0; i < row; i++)
        {
            if (i < colors.Length)
            {
                arrayColor[i, colIndex] = colors[i];
            }
        }
        Invalidate();
    }

    /// <summary>ResetColorByCol 方法。</summary>
    public void ResetColorByCol(int colIndex)
    {
        for (int i = 0; i < row; i++)
        {
            arrayColor[i, colIndex] = defaultColor;
        }
        Invalidate();
    }

    /// <summary>设置 colorAll。</summary>
    public void SetColorAll(Color color)
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                arrayColor[i, j] = color;
            }
        }
        Invalidate();
    }

    /// <summary>设置 colorAll。</summary>
    public void SetColorAll(Color[] colors)
    {
        int num = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (num >= colors.Length)
                {
                    break;
                }
                arrayColor[i, j] = colors[num];
                num++;
            }
        }
        Invalidate();
    }

    /// <summary>ResetColorAll 方法。</summary>
    public void ResetColorAll()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                arrayColor[i, j] = defaultColor;
            }
        }
        Invalidate();
    }

    /// <summary>获取 colorFromPosition。</summary>
    public Color GetColorFromPosition(int rowIndex, int colIndex)
    {
        return arrayColor[rowIndex, colIndex];
    }

    /// <summary>设置 hDescriptionLanguage。</summary>
    public void SetHDescriptionLanguage(int rowIndex, int colIndex, string HDescriptionLanguage)
    {
        HDescriptionLanguages[rowIndex, colIndex] = HDescriptionLanguage;
    }

    /// <summary>设置 allHDescriptionLanguage。</summary>
    public void SetAllHDescriptionLanguage(string HDescriptionLanguage)
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                HDescriptionLanguages[i, j] = HDescriptionLanguage;
            }
        }
    }

    /// <summary>ClearAllHDescriptionLanguage 方法。</summary>
    public void ClearAllHDescriptionLanguage()
    {
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                HDescriptionLanguages[i, j] = string.Empty;
            }
        }
    }

    /// <summary>设置 allHDescriptionLanguage。</summary>
    public void SetAllHDescriptionLanguage(string[] descs)
    {
        int num = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                if (num >= descs.Length)
                {
                    break;
                }
                HDescriptionLanguages[i, j] = descs[num];
                num++;
            }
        }
    }

    /// <summary>ArrayInni 方法。</summary>
    private void ArrayInni()
    {
        arrayColor = new Color[row, col];
        HDescriptionLanguages = new string[row, col];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                arrayColor[i, j] = defaultColor;
                HDescriptionLanguages[i, j] = string.Empty;
            }
        }
    }

    /// <summary>StatusManagementH_Load 方法。</summary>
    private void StatusManagementH_Load(object sender, EventArgs e)
    {
        base.MouseMove += StatusManagementH_MouseMove;
        base.MouseLeave += StatusManagementH_MouseLeave;
    }

    /// <summary>StatusManagementH_MouseLeave 方法。</summary>
    private void StatusManagementH_MouseLeave(object sender, EventArgs e)
    {
        activePoint = new Point(-1, -1);
        Refresh();
    }

    /// <summary>StatusManagementH_MouseMove 方法。</summary>
    private void StatusManagementH_MouseMove(object sender, MouseEventArgs e)
    {
        activePoint = new Point(e.X, e.Y);
        Refresh();
    }

    /// <summary>StatusManagementH_Paint 方法。</summary>
    private void StatusManagementH_Paint(object sender, PaintEventArgs e)
    {
        if (activePoint.X < 0 || activePoint.Y < 0 || base.Width < 10 || base.Height < 10)
        {
            return;
        }
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        float num = (float)base.Width * 1f / (float)col;
        float num2 = (float)base.Height * 1f / (float)row;
        int num3 = (int)((float)activePoint.Y / num2);
        int num4 = (int)((float)activePoint.X / num);
        if (!((float)activePoint.X < (float)num4 * num + offect / 2f) && !((float)activePoint.X > (float)(num4 + 1) * num - offect / 2f) && !((float)activePoint.Y < (float)num3 * num2 + offect / 2f) && !((float)activePoint.Y > (float)(num3 + 1) * num2 - offect / 2f) && num3 < row && num4 < col && !string.IsNullOrEmpty(HDescriptionLanguages[num3, num4]))
        {
            float width = graphics.MeasureString(HDescriptionLanguages[num3, num4], Font).Width;
            RectangleF rectangleF = new RectangleF(activePoint.X + 10, activePoint.Y - 10, width + 20f, Font.Height + 10);
            if ((float)activePoint.X > (float)base.Width - width - 20f)
            {
                rectangleF.X = (float)activePoint.X - width - 20f;
            }
            if (activePoint.Y > base.Height - Font.Height - 10)
            {
                activePoint.Y = activePoint.Y - Font.Height - 10;
            }
            graphics.FillRectangle(Brushes.DimGray, rectangleF);
            graphics.DrawString(HDescriptionLanguages[num3, num4], Font, Brushes.Yellow, rectangleF, sf);
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
        base.Name = "StatusManagementH";
        base.Size = new System.Drawing.Size(375, 124);
        base.Load += new System.EventHandler(StatusManagementH_Load);
        base.Paint += new System.Windows.Forms.PaintEventHandler(StatusManagementH_Paint);
        ResumeLayout(false);
    }
}
}