using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    /// 绘制多边形
    /// </summary>
    public class DrawPolygonUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; }
    /// <summary>PointFAll 成员。</summary>
    public System.Drawing.PointF[] PointFAll { set; get; }
    public DrawPolygonUtil()
    {

    }
    public DrawPolygonUtil(System.Drawing.Pen pen)
    {
        Pen = pen;
    }
    /// <summary>DrawPolygon 方法。</summary>
    public virtual bool DrawPolygon(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawPolygon(Pen, PointFAll);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.Graphics graphics, System.Drawing.RectangleF rectangleF)
    {
        return graphics.IsVisible(rectangleF);
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.Graphics graphics, System.Drawing.PointF pointF)
    {
        return graphics.IsVisible(pointF);
    }
    /// <summary>获取 graphicsPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddPolygon(PointFAll);
        return graphicsPath;
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF)
    {
        return GetGraphicsPath().IsVisible(pointF.X, pointF.Y);
    }

}
}
