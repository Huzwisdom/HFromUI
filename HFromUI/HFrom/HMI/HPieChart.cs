using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    using HFromUI.HLangage;
    [HDescriptionLanguage("饼图控件，支持设置颜色，是否显示百分比功能")]
public class HPieChart : UserControl
{
    /// <summary>percentColor 字段。</summary>
    private Color percentColor = Color.DodgerBlue;

    /// <summary>percentBrush 字段。</summary>
    private Brush percentBrush = new SolidBrush(Color.DodgerBlue);

    /// <summary>borderColor 字段。</summary>
    private Color borderColor = Color.DodgerBlue;

    /// <summary>borderPen 字段。</summary>
    private Pen borderPen = new Pen(Color.DodgerBlue, 1f);

    /// <summary>pieItems 字段。</summary>
    private PieItemH[] pieItems = new PieItemH[0];

    /// <summary>random 字段。</summary>
    private Random random = null;

    /// <summary>margin 字段。</summary>
    private int margin = 26;

    /// <summary>hHoverIndex 字段。</summary>
    private int hHoverIndex = -1;

    /// <summary>hMousePt 字段。</summary>
    private Point hMousePt = Point.Empty;

    /// <summary>m_IsRenderPercent 字段。</summary>
    private bool m_IsRenderPercent = false;

    /// <summary>m_IsRenderSmall 字段。</summary>
    private bool m_IsRenderSmall = true;

    /// <summary>percenFormat 字段。</summary>
    private string percenFormat = "{0:F2}%";

    /// <summary>startAngle 字段。</summary>
    private float startAngle = 0f;

    /// <summary>rotateDirection 字段。</summary>
    private RotateDirection rotateDirection = RotateDirection.AntiClockWise;

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("绘制百分比")]
    [DefaultValue(false)]
    [HDescriptionLanguage("获取或设置是否显示百分比占用")]
    public bool IsRenderPercent
    {
        get
        {
            return m_IsRenderPercent;
        }
        set
        {
            m_IsRenderPercent = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("紧凑绘制")]
    [HDescriptionLanguage("获取或设置是否显示占比很小的文本信息")]
    [DefaultValue(true)]
    public bool IsRenderSmall
    {
        get
        {
            return m_IsRenderSmall;
        }
        set
        {
            m_IsRenderSmall = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本外边距")]
    [HDescriptionLanguage("获取或设置文本距离，单位为像素，默认26")]
    [DefaultValue(26)]
    public int TextMargin
    {
        get
        {
            return margin;
        }
        set
        {
            margin = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("百分比颜色")]
    [HDescriptionLanguage("获取或设置文百分比文字的颜色信息")]
    [DefaultValue(typeof(Color), "DodgerBlue")]
    public Color PercentColor
    {
        get
        {
            return percentColor;
        }
        set
        {
            percentColor = value;
            percentBrush.Dispose();
            percentBrush = new SolidBrush(percentColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框颜色")]
    [HDescriptionLanguage("获取或设置边框的颜色信息")]
    [DefaultValue(typeof(Color), "DodgerBlue")]
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
            borderPen = new Pen(borderColor);
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("百分比格式")]
    [HDescriptionLanguage("获取或设置文百分比文字的格式化信息")]
    [DefaultValue("{0:F2}%")]
    public string PercentFormat
    {
        get
        {
            return percenFormat;
        }
        set
        {
            percenFormat = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("起始角度")]
    [HDescriptionLanguage("获取或设置初始的角度信息，默认为0，可以设置 0 - 360 范围")]
    [DefaultValue("0")]
    public float StartAngle
    {
        get
        {
            return startAngle;
        }
        set
        {
            for (startAngle = value; startAngle < 0f; startAngle += 360f)
            {
            }
            while (startAngle > 360f)
            {
                startAngle -= 360f;
            }
            Invalidate();
        }
    }

    [Browsable(true)]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("旋转方向")]
    [HDescriptionLanguage("获取或设置当前饼图的旋转方向，选择 ClockWise 为顺时针，AntiClockWise 为逆时针")]
    [DefaultValue(typeof(RotateDirection), "AntiClockWise")]
    public RotateDirection RotateDirection
    {
        get
        {
            return rotateDirection;
        }
        set
        {
            rotateDirection = value;
            Invalidate();
        }
    }

    public HPieChart()
    {
        InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            random = new Random();
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, value: true);
        SetStyle(ControlStyles.ResizeRedraw, value: true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
        pieItems = new PieItemH[0];
        if (GetService(typeof(IDesignerHost)) != null || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            pieItems = new PieItemH[5]
            {
                new PieItemH
                {
                    Name = HTranslation.GetContent("数据一"),
                    Value = 30,
                    PieColor = Color.DodgerBlue
                },
                new PieItemH
                {
                    Name = HTranslation.GetContent("数据二"),
                    Value = 10,
                    PieColor = Color.Purple
                },
                new PieItemH
                {
                    Name = HTranslation.GetContent("数据三"),
                    Value = 16,
                    PieColor = Color.Tomato
                },
                new PieItemH
                {
                    Name = HTranslation.GetContent("数据四"),
                    Value = 15,
                    PieColor = Color.Orange
                },
                new PieItemH
                {
                    PieColor = Color.Wheat,
                    Name = HTranslation.GetContent("数据五"),
                    Value = 12
                }
            };
        }
    }

    /// <summary>获取 centerPoint。</summary>
    private Point GetCenterPoint(int width, int height, out int squareWidth)
    {
        if (width > height)
        {
            squareWidth = height / 2 - margin - 8;
            return new Point(width / 2 - 1, height / 2 - 1);
        }
        squareWidth = width / 2 - margin - 8;
        return new Point(width / 2 - 1, height / 2 - 1);
    }

    /// <summary>获取 randomColor。</summary>
    private Color GetRandomColor()
    {
        int num = random.Next(230);
        int num2 = random.Next(230);
        int blue = (num + num2 > 350) ? random.Next(100) : (random.Next(100) + 100);
        return Color.FromArgb(num, num2, blue);
    }

    /// <summary>响应 Paint 事件。</summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        PaintControlsH(e.Graphics, base.Width, base.Height);
        base.OnPaint(e);
    }

    /// <summary>
    /// 命中测试：返回鼠标所在扇形索引（-1=饼图外）。
    /// 绘制时基准变换为 T(cx,cy)*R(90)，扇形 i 在该坐标系下覆盖
    /// [startAngle + 累计前 i 项扫角, +自身扫角]，方向系数 dir=顺时针 1/逆时针 -1。
    /// </summary>
    private int HitTestSector(Point pt)
    {
        if (pieItems == null || pieItems.Length == 0) return -1;
        int squareWidth;
        Point c = GetCenterPoint(Width, Height, out squareWidth);
        if (squareWidth <= 0) return -1;
        // 悬停扇形半径会外扩，命中判定同步放宽 12%
        double rr = squareWidth * 1.12;
        double dx = pt.X - c.X;
        double dy = pt.Y - c.Y;
        double rad = Math.Sqrt(dx * dx + dy * dy);
        if (rad > rr || rad < 1) return -1;
        // 逆 R(90)：屏幕坐标 → 基准绘制坐标
        double tx = dy, ty = -dx;
        double ang = Math.Atan2(ty, tx) * 180.0 / Math.PI; // GDI 角度（顺时针为正）
        int total = pieItems.Sum(it => it.Value);
        float acc = startAngle;
        float dir = (rotateDirection == RotateDirection.AntiClockWise) ? -1f : 1f;
        for (int i = 0; i < pieItems.Length; i++)
        {
            float sweep = total != 0
                ? (float)(pieItems[i].Value * 1.0 / total * 360.0)
                : 360f / pieItems.Length;
            float a0 = acc, a1 = acc + dir * sweep;
            float lo = Math.Min(a0, a1), hi = Math.Max(a0, a1);
            // 角度归一到 [lo, lo+360) 后判定
            double t = ang;
            while (t < lo) t += 360;
            while (t >= lo + 360) t -= 360;
            if (t >= lo - 1e-4 && t <= hi + 1e-4) return i;
            acc = a1;
        }
        return -1;
    }

    /// <summary>响应 MouseMove 事件。</summary>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        hMousePt = e.Location;
        int hit = HitTestSector(e.Location);
        if (hit != hHoverIndex)
        {
            hHoverIndex = hit;
            Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }
    }

    /// <summary>响应 MouseLeave 事件。</summary>
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (hHoverIndex >= 0)
        {
            hHoverIndex = -1;
            Cursor = Cursors.Default;
            Invalidate();
        }
    }

    /// <summary>PaintControlsH 方法。</summary>
    public void PaintControlsH(Graphics g, int width, int height)
    {
        PaintMain(g, width, height);
    }

    /// <summary>PaintMain 方法。</summary>
    private void PaintMain(Graphics g, int width, int height)
    {
        int squareWidth;
        Point centerPoint = GetCenterPoint(width, height, out squareWidth);
        Rectangle rectangle = new Rectangle(centerPoint.X - squareWidth, centerPoint.Y - squareWidth, squareWidth + squareWidth, squareWidth + squareWidth);
        if (squareWidth > 0 && pieItems.Length != 0)
        {
            g.FillEllipse(Brushes.AliceBlue, rectangle);
            g.DrawEllipse(borderPen, rectangle);
            Rectangle rect = new Rectangle(rectangle.X - centerPoint.X, rectangle.Y - centerPoint.Y, rectangle.Width, rectangle.Height);
            g.TranslateTransform(centerPoint.X, centerPoint.Y);
            g.RotateTransform(90f);
            g.DrawLine(Pens.DimGray, 0, 0, squareWidth, 0);
            int num = pieItems.Sum((PieItemH item) => item.Value);
            float num2 = 0f - startAngle;
            float num3 = num2 - 90f;
            for (int i = 0; i < pieItems.Length; i++)
            {
                Pen pen = new Pen(pieItems[i].LineColor, 1f);
                SolidBrush solidBrush = new SolidBrush(pieItems[i].PieColor);
                SolidBrush solidBrush2 = new SolidBrush(pieItems[i].FontColor);
                float num4 = g.MeasureString(pieItems[i].Name, Font).Width + 5f;
                float num5 = 0f;
                num5 = ((num != 0) ? Convert.ToSingle((double)pieItems[i].Value * 1.0 / (double)num * 360.0) : ((float)(360 / pieItems.Length)));
                float num6 = (rotateDirection != RotateDirection.AntiClockWise) ? 1 : (-1);
                // 悬停扇形：沿中线外移 + 半径整体放大（“扇形变大”）
                bool hover = (i == hHoverIndex);
                GraphicsState hoverState = null;
                Rectangle drawRect = rect;
                if (hover && num5 > 0.1f)
                {
                    float grow = squareWidth * 0.10f + 2f;       // 半径外扩
                    float explode = squareWidth * 0.06f + 2f;    // 沿中线弹出
                    drawRect = new Rectangle(rect.X - (int)grow, rect.Y - (int)grow,
                                             rect.Width + (int)grow * 2, rect.Height + (int)grow * 2);
                    hoverState = g.Save();
                    g.RotateTransform(num6 * num5 / 2f);
                    g.TranslateTransform(explode, 0f);
                    g.RotateTransform(-num6 * num5 / 2f);
                }
                g.FillPie(solidBrush, drawRect, startAngle, num6 * num5);
                if (hover)
                {
                    using (Pen hi = new Pen(Color.FromArgb(255, 255, 255), 2f))
                        g.DrawPie(hi, drawRect, startAngle, num6 * num5);
                    g.Restore(hoverState);
                }
                g.RotateTransform(startAngle);
                g.RotateTransform(num6 * num5 / 2f);
                if (num5 < 2f && !IsRenderSmall)
                {
                    num2 += (0f - num6) * num5;
                }
                else
                {
                    num2 += (0f - num6) * num5 / 2f;
                    float num7 = 0f;
                    num7 = ((rotateDirection != RotateDirection.AntiClockWise) ? ((num2 + 360f < 0f) ? (num2 + 360f + 360f) : (num2 + 360f)) : ((num2 < 0f) ? (num2 + 360f) : num2));
                    int num8 = 15;
                    if (num7 < 45f || num7 > 315f)
                    {
                        num8 = 20;
                    }
                    if (num7 > 135f && num7 < 225f)
                    {
                        num8 = 20;
                    }
                    g.DrawLine(pen, squareWidth * 2 / 3, 0, squareWidth + num8, 0);
                    g.TranslateTransform(squareWidth + num8, 0f);
                    if (Math.Abs(num2 - num3) < 5f)
                    {
                    }
                    num3 = num2;
                    if (num7 < 180f)
                    {
                        g.RotateTransform(num7 - 90f);
                        g.DrawLine(pen, 0f, 0f, num4, 0f);
                        g.DrawString(pieItems[i].Name, Font, solidBrush2, new Point(0, -Font.Height));
                        if (IsRenderPercent)
                        {
                            g.DrawString(string.Format(percenFormat, num5 * 100f / 360f), Font, percentBrush, new Point(0, 1));
                        }
                        g.RotateTransform(90f - num7);
                    }
                    else
                    {
                        g.RotateTransform(num7 - 270f);
                        g.DrawLine(pen, 0f, 0f, num4, 0f);
                        g.TranslateTransform(num4 - 3f, 0f);
                        g.RotateTransform(180f);
                        g.DrawString(pieItems[i].Name, Font, solidBrush2, new Point(0, -Font.Height));
                        if (IsRenderPercent)
                        {
                            g.DrawString(string.Format(percenFormat, num5 * 100f / 360f), Font, percentBrush, new Point(0, 1));
                        }
                        g.RotateTransform(-180f);
                        g.TranslateTransform(0f - num4 + 3f, 0f);
                        g.RotateTransform(270f - num7);
                    }
                    g.TranslateTransform(-squareWidth - num8, 0f);
                    g.RotateTransform(num6 * num5 / 2f);
                    num2 += (0f - num6) * num5 / 2f;
                }
                g.RotateTransform(0f - startAngle);
                solidBrush.Dispose();
                pen.Dispose();
                solidBrush2.Dispose();
            }
            g.ResetTransform();
        }
        else
        {
            g.FillEllipse(Brushes.AliceBlue, rectangle);
            g.DrawEllipse(borderPen, rectangle);
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                g.DrawString(HTranslation.GetContent("空"), Font, brush, rectangle, FromHelper.StringFormatCenter);
            }
        }

        // 悬停详细信息提示框（名称/数值/占比）
        if (hHoverIndex >= 0 && hHoverIndex < pieItems.Length)
            DrawHoverTip(g, hHoverIndex, width, height);
    }

    /// <summary>鼠标悬停扇形时绘制详细信息圆角提示框（颜色块 + 名称 + 数值 + 占比）。</summary>
    private void DrawHoverTip(Graphics g, int idx, int width, int height)
    {
        PieItemH item = pieItems[idx];
        int total = pieItems.Sum(it => it.Value);
        double pct = total != 0 ? item.Value * 100.0 / total : 0;
        string line1 = item.Name ?? "";
        string line2 = HTranslation.GetContent("数值：") + item.Value;
        string line3 = HTranslation.GetContent("占比：") + string.Format(percenFormat, pct);

        Font f = Font;
        float pad = 8f;
        float w1 = g.MeasureString(line1, f).Width;
        float w2 = g.MeasureString(line2, f).Width;
        float w3 = g.MeasureString(line3, f).Width;
        float boxW = Math.Max(Math.Max(w1, w2), w3) + pad * 2 + 16f;
        float lineH = f.Height + 4f;
        float boxH = lineH * 3 + pad * 2 - 2f;

        float tx = hMousePt.X + 14f;
        float ty = hMousePt.Y + 14f;
        if (tx + boxW > width - 4) tx = hMousePt.X - 14f - boxW;
        if (ty + boxH > height - 4) ty = hMousePt.Y - 14f - boxH;
        if (tx < 4) tx = 4;
        if (ty < 4) ty = 4;

        RectangleF box = new RectangleF(tx, ty, boxW, boxH);
        using (GraphicsPath gp = RoundedBox(box, 6))
        {
            using (SolidBrush bb = new SolidBrush(Color.FromArgb(238, 32, 36, 44)))
                g.FillPath(bb, gp);
            using (Pen bp = new Pen(item.PieColor, 1.5f))
                g.DrawPath(bp, gp);
        }
        // 颜色块
        using (SolidBrush chip = new SolidBrush(item.PieColor))
            g.FillRectangle(chip, tx + pad, ty + pad + 2, 10, 10);
        float textX = tx + pad + 16f;
        g.DrawString(line1, f, Brushes.White, textX, ty + pad);
        g.DrawString(line2, f, Brushes.LightGray, textX, ty + pad + lineH);
        g.DrawString(line3, f, Brushes.LightGray, textX, ty + pad + lineH * 2);
    }

    /// <summary>RoundedBox 方法。</summary>
    private static GraphicsPath RoundedBox(RectangleF r, float radius)
    {
        float d = radius * 2;
        if (d > r.Width) d = r.Width;
        if (d > r.Height) d = r.Height;
        GraphicsPath path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(PieItemH[] source)
    {
        if (source != null)
        {
            pieItems = source;
            Invalidate();
        }
    }

    /// <summary>设置 dataSource。</summary>
    public void SetDataSource(string[] names, int[] values)
    {
        if (names == null)
        {
            throw new ArgumentNullException(HTranslation.GetContent("名称集合不能为空"));
        }
        if (values == null)
        {
            throw new ArgumentNullException(HTranslation.GetContent("值集合不能为空"));
        }
        if (names.Length != values.Length)
        {
            throw new Exception(HTranslation.GetContent("两个数组的长度不一致！"));
        }
        pieItems = new PieItemH[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            pieItems[i] = new PieItemH
            {
                Name = names[i],
                Value = values[i],
                PieColor = GetRandomColor()
            };
        }
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
        base.Name = "PieChartH";
        ResumeLayout(false);
    }
}
}