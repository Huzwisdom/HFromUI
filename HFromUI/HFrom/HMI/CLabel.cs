using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace HFromUI.HFrom.HMI
{
    public class CLabel : Label
{
    public CLabel()
    {
        AutoSize = false;
        base.Size = new Size(200, 20);
        TextAlign = ContentAlignment.MiddleLeft;
        ForeColor = Color.White;
        BackColor = Color.DodgerBlue;
        base.Padding = new Padding(5, 0, 0, 0);
    }

    /// <summary>响应 PaintBackground 事件。</summary>
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Point(0, 20), new Point(base.Width - 1, 20), Color.FromArgb(142, 196, 216), Color.FromArgb(240, 240, 240));
        ColorBlend colorBlend = new ColorBlend();
        colorBlend.Positions = new float[2]
        {
            0f,
            1f
        };
        colorBlend.Colors = new Color[2]
        {
            BackColor,
            FromHelper.GetColorLight(FromHelper.GetColorLight(BackColor))
        };
        linearGradientBrush.InterpolationColors = colorBlend;
        pevent.Graphics.FillRectangle(linearGradientBrush, 0, 0, base.Width, base.Height);
        linearGradientBrush.Dispose();
    }
}
}
