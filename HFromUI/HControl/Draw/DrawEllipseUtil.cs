using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    /// 绘制一个由边框（该边框由一对坐标、高度和宽度指定）定义的椭圆
    /// </summary>
    public class DrawEllipseUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; }
    /// <summary>RectangleF 成员。</summary>
    public System.Drawing.RectangleF RectangleF { set; get; }
    public DrawEllipseUtil(System.Drawing.RectangleF rectangleF)
    {
        RectangleF = rectangleF;
    }
    public DrawEllipseUtil(float x, float y, float width, float height)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
    }
    public DrawEllipseUtil(int x, int y, int width, int height)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
    }
    /// <summary>绘制矩形。</summary>
    public virtual bool DrawRectangle(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawEllipse(Pen, RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillRectangle 方法。</summary>
    public virtual bool FillRectangle(System.Drawing.Graphics graphics, Brush brush)
    {
        try
        {
            graphics.FillEllipse(brush, RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height);
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
    public bool IsVisible(System.Drawing.Graphics graphics, System.Drawing.PointF pointF)
    {
        return graphics.IsVisible(pointF);
    }

    /// <summary>获取 centerPointF。</summary>
    public virtual PointF GetCenterPointF()
    {
        return new PointF(RectangleF.X + RectangleF.Width / 2, RectangleF.Y + RectangleF.Height / 2);
    }
    /// <summary>获取 partPointF。</summary>
    public virtual PointF GetPartPointF(float partX, float partY)
    {
        return new PointF(RectangleF.X + RectangleF.Width * partX, RectangleF.Y + RectangleF.Height * partY);
    }

    /// <summary>获取 graphicsPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddEllipse(RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height);
        return graphicsPath;
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF)
    {
        return GetGraphicsPath().IsVisible(pointF.X, pointF.Y);
    }
    /// <summary>获取 modePointF。</summary>
    public virtual System.Drawing.RectangleF GetModePointF(int mode, bool IsSet = true)
    {
        RectangleFConvertUtil rectangleFConvertUtil = new RectangleFConvertUtil(RectangleF);
        if (mode > 0)
        {
            if (IsSet)
            {
                RectangleF = rectangleFConvertUtil.SubRectangleF(mode);
            }
            return rectangleFConvertUtil.SubRectangleF(mode);
        }
        else
        {
            if (IsSet)
            {
                RectangleF = rectangleFConvertUtil.AddRectangleF(mode);
            }
            return rectangleFConvertUtil.AddRectangleF(mode);
        }
    }
}
}
