using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    [HDescriptionLanguage("一个用于签名的控件，方便实现用户签名，并对签名内容进行保存")]
public class HSignature : UserControl
{
    /// <summary>boderColor 字段。</summary>
    private Color boderColor = Color.FromArgb(205, 205, 205);

    /// <summary>penBorder 字段。</summary>
    private Pen penBorder = new Pen(Color.FromArgb(205, 205, 205));

    /// <summary>sf 字段。</summary>
    private StringFormat sf = null;

    /// <summary>ValvesStyleH 字段。</summary>
    private DirectionStyleH ValvesStyleH = DirectionStyleH.Horizontal;

    /// <summary>totalPoints 字段。</summary>
    private List<List<PointF>> totalPoints = new List<List<PointF>>();

    /// <summary>currPoints 字段。</summary>
    private List<PointF> currPoints = new List<PointF>();

    /// <summary>isEnableSign 字段。</summary>
    private bool isEnableSign = true;

    /// <summary>isMouseDown 字段。</summary>
    private bool isMouseDown = false;

    /// <summary>signWidth 字段。</summary>
    private float signWidth = 1f;

    /// <summary>signColor 字段。</summary>
    private Color signColor = Color.Black;

    /// <summary>components 字段。</summary>
    private IContainer components = null;

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置控件的背景色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("背景颜色")]
    [DefaultValue(typeof(Color), "White")]
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
    [HDescriptionLanguage("获取或设置当前控件的是否允许签名，默认是允许签名的")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("允许签名")]
    [DefaultValue(true)]
    public bool EnableSign
    {
        get
        {
            return isEnableSign;
        }
        set
        {
            isEnableSign = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置当前签名控件的宽度信息")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("签名线宽")]
    [DefaultValue(1f)]
    public float SignWidth
    {
        get
        {
            return signWidth;
        }
        set
        {
            signWidth = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [HDescriptionLanguage("获取或设置当前签名的颜色")]
    [HCategoryLanguage("自定义"), HDisplayNameLanguage("签名颜色")]
    [DefaultValue(typeof(Color), "Black")]
    public Color SignColor
    {
        get
        {
            return signColor;
        }
        set
        {
            signColor = value;
            Invalidate();
        }
    }

    public HSignature()
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

    /// <summary>SignatureH_Load 方法。</summary>
    private void SignatureH_Load(object sender, EventArgs e)
    {
        base.MouseDown += SignatureH_MouseDown;
        base.MouseUp += SignatureH_MouseUp;
        base.MouseMove += SignatureH_MouseMove;
    }

    /// <summary>SignatureH_MouseMove 方法。</summary>
    private void SignatureH_MouseMove(object sender, MouseEventArgs e)
    {
        if (isMouseDown && isEnableSign)
        {
            currPoints.Add(new PointF(e.X, e.Y));
            Invalidate();
        }
    }

    /// <summary>SignatureH_MouseUp 方法。</summary>
    private void SignatureH_MouseUp(object sender, MouseEventArgs e)
    {
        if (isEnableSign && e.Button == MouseButtons.Left)
        {
            isMouseDown = false;
            currPoints.Add(new PointF(e.X, e.Y));
            totalPoints.Add(currPoints);
            currPoints = new List<PointF>();
            Invalidate();
        }
    }

    /// <summary>SignatureH_MouseDown 方法。</summary>
    private void SignatureH_MouseDown(object sender, MouseEventArgs e)
    {
        if (isEnableSign)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                currPoints.Add(new PointF(e.X, e.Y));
            }
            else if (e.Button == MouseButtons.Right)
            {
                ClearSign();
            }
        }
    }

    /// <summary>ClearSign 方法。</summary>
    public void ClearSign()
    {
        currPoints.Clear();
        totalPoints.Clear();
        Invalidate();
    }

    /// <summary>SaveBitmap 方法。</summary>
    public Image SaveBitmap()
    {
        Bitmap bitmap = new Bitmap(base.Width, base.Height);
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        graphics.Clear(BackColor);
        PaintMain(graphics, bitmap.Width, bitmap.Height);
        graphics.Dispose();
        return bitmap;
    }

    /// <summary>SaveBitmap 方法。</summary>
    public void SaveBitmap(string fileName)
    {
        SaveBitmap().Save(fileName);
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
        using (Pen pen = new Pen(signColor, signWidth))
        {
            foreach (List<PointF> totalPoint in totalPoints)
            {
                PointF[] array = totalPoint.ToArray();
                if (array.Length > 1)
                {
                    g.DrawCurve(pen, array);
                }
            }
            PointF[] array2 = currPoints.ToArray();
            if (array2.Length > 1)
            {
                g.DrawCurve(pen, array2);
            }
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
        base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
        base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        base.Name = "SignatureH";
        base.Size = new System.Drawing.Size(487, 177);
        base.Load += new System.EventHandler(SignatureH_Load);
        ResumeLayout(false);
    }
}
}