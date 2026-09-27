using System;
using System.Drawing;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    ///   绘制平面两点的连线
    /// </summary>
    public class DrawLineUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; }
    /// <summary>PointFBegin 成员。</summary>
    public System.Drawing.PointF PointFBegin { set; get; }
    /// <summary>PointFEnd 成员。</summary>
    public System.Drawing.PointF PointFEnd { set; get; }
    public DrawLineUtil()
    {

    }
    public DrawLineUtil(System.Drawing.PointF pointFBegin, System.Drawing.PointF pointFEnd)
    {
        PointFBegin = pointFBegin;
        PointFEnd = pointFEnd;
    }
    public DrawLineUtil(Pen pen, System.Drawing.PointF pointFBegin, System.Drawing.PointF pointFEnd)
    {
        Pen = pen;
        PointFBegin = pointFBegin;
        PointFEnd = pointFEnd;
    }
    public DrawLineUtil(float x1, float y1, float x2, float y2)
    {
        PointFBegin = new PointF(x1, y1);
        PointFEnd = new PointF(x2, y2);
    }

    public DrawLineUtil(Pen pen, float x1, float y1, float x2, float y2)
    {
        Pen = pen;
        PointFBegin = new PointF(x1, y1);
        PointFEnd = new PointF(x2, y2);
    }
    public DrawLineUtil(int x1, int y1, int x2, int y2)
    {
        PointFBegin = new PointF(x1, y1);
        PointFEnd = new PointF(x2, y2);
    }

    public DrawLineUtil(Pen pen, int x1, int y1, int x2, int y2)
    {
        Pen = pen;
        PointFBegin = new PointF(x1, y1);
        PointFEnd = new PointF(x2, y2);
    }
    /// <summary>绘制直线。</summary>
    public virtual bool DrawLine(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen==null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawLine(Pen, PointFBegin, PointFEnd);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>获取 centerPointF。</summary>
    public virtual PointF GetCenterPointF()
    {
        return new PointF(PointFBegin.X + (PointFEnd.X - PointFBegin.X) / 2, PointFBegin.Y + (PointFEnd.Y - PointFBegin.Y) / 2);
    }

    /// <summary>获取 partPointF。</summary>
    public virtual PointF GetPartPointF(float part)
    {
        return new PointF(PointFBegin.X + (PointFEnd.X - PointFBegin.X) * part, PointFBegin.Y + (PointFEnd.Y - PointFBegin.Y) * part);
    }

    /// <summary>获取 graphicsPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddLine(PointFBegin, PointFEnd);
        return graphicsPath;
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF, System.Drawing.Pen pen)
    {
        return GetGraphicsPath().IsOutlineVisible(pointF, pen);
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF)
    {
        return GetGraphicsPath().IsOutlineVisible(pointF, Pen);
    }
}
}
