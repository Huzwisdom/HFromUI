using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    /// 绘制三角形
    /// </summary>
    public class DrawTriangleUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; }
    /// <summary>PointFA1 成员。</summary>
    public System.Drawing.PointF PointFA1 { set; get; }
    /// <summary>PointFA2 成员。</summary>
    public System.Drawing.PointF PointFA2 { set; get; }
    /// <summary>PointFA3 成员。</summary>
    public System.Drawing.PointF PointFA3 { set; get; }
    public DrawTriangleUtil()
    {

    }
    public DrawTriangleUtil(float x1, float y1, float x2, float y2, float x3, float y3)
    {
        PointFA1 = new System.Drawing.PointF(x1, y1);
        PointFA2 = new System.Drawing.PointF(x2, y2);
        PointFA3 = new System.Drawing.PointF(x3, y3);
    }
    public DrawTriangleUtil(System.Drawing.RectangleF rectangleGridF, int mode = 1)
    {
        switch (mode)
        {
            case 1:
                PointFA1 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width / 2, rectangleGridF.Y);
                PointFA2 = new System.Drawing.PointF(rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height);
                PointFA3 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height);
                break;
            case 2:
                PointFA1 = new System.Drawing.PointF(rectangleGridF.X, rectangleGridF.Y);
                PointFA2 = new System.Drawing.PointF(rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height);
                PointFA3 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height / 2);
                break;
            case 3:
                PointFA1 = new System.Drawing.PointF(rectangleGridF.X, rectangleGridF.Y);
                PointFA2 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y);
                PointFA3 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width / 2, rectangleGridF.Y + rectangleGridF.Height);
                break;
            case 4:
                PointFA1 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y);
                PointFA2 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height);
                PointFA3 = new System.Drawing.PointF(rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height / 2);
                break;
            default:
                PointFA1 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width / 2, rectangleGridF.Y);
                PointFA2 = new System.Drawing.PointF(rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height);
                PointFA3 = new System.Drawing.PointF(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height);
                break;
        }
    }

    /// <summary>DrawTriangle 方法。</summary>
    public virtual bool DrawTriangle(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            System.Drawing.PointF[] PointF3 = { PointFA1, PointFA2, PointFA3 };
            graphics.DrawPolygon(Pen, PointF3);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillTriangle 方法。</summary>
    public virtual bool FillTriangle(System.Drawing.Graphics graphics, Brush brush)
    {
        try
        {
            System.Drawing.PointF[] PointF3 = { PointFA1, PointFA2, PointFA3 };
            graphics.FillPolygon(brush, PointF3);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillTriangle 方法。</summary>
    public virtual bool FillTriangle(System.Drawing.Graphics graphics, Brush brush, FillMode fillMode)
    {
        try
        {
            System.Drawing.PointF[] PointF3 = { PointFA1, PointFA2, PointFA3 };
            graphics.FillPolygon(brush, PointF3, fillMode);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillTriangle 方法。</summary>
    public virtual bool FillTriangle(System.Drawing.Graphics graphics, Color color)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            System.Drawing.PointF[] PointF3 = { PointFA1, PointFA2, PointFA3 };
            graphics.FillPolygon(brush, PointF3);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillTriangle 方法。</summary>
    public virtual bool FillTriangle(System.Drawing.Graphics graphics, Color color, FillMode fillMode)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            System.Drawing.PointF[] PointF3 = { PointFA1, PointFA2, PointFA3 };
            graphics.FillPolygon(brush, PointF3, fillMode);
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
        graphicsPath.AddPolygon(new PointF[] { PointFA1 , PointFA2, PointFA3});
        return graphicsPath;
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF)
    {
        return GetGraphicsPath().IsVisible(pointF.X, pointF.Y);
    }
}
}
