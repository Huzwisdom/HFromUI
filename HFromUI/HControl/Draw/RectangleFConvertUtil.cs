using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    public class RectangleFConvertUtil
{
    /*  1  2  3
        4  5  6
        7  8  9
    */

    /// <summary>RectangleFh 成员。</summary>
    public System.Drawing.RectangleF RectangleFh { set; get; }
    public RectangleFConvertUtil(System.Drawing.RectangleF rectangleF)
    {
        RectangleFh = rectangleF;
    }
    public RectangleFConvertUtil(float x, float y, float width, float height)
    {
        RectangleFh = new RectangleF(x,y,width,height);
    }
    /// <summary>SubPointF 方法。</summary>
    public virtual System.Drawing.PointF SubPointF(int mode)
    {
        System.Drawing.PointF pointF = new PointF(RectangleFh.X, RectangleFh.Y);
        switch (mode)
        {
            case 1:
                break;
            case 2:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case 3:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y;
                break;
            case 4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case 5:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case 6:
                pointF.X = pointF.X - RectangleFh.Width ;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case 7:
                pointF.X = pointF.X ;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            case 8:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y - RectangleFh.Height ;
                break;
            case 9:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            default:
                break;
        }
        return pointF;
    }
    /// <summary>SubRectangleF 方法。</summary>
    public virtual System.Drawing.RectangleF SubRectangleF(int mode)
    {
        System.Drawing.PointF pointF = new PointF(RectangleFh.X, RectangleFh.Y);
        System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(RectangleFh.X, RectangleFh.Y, RectangleFh.Width, RectangleFh.Height);
        switch (mode)
        {
            case 1:
                break;
            case 2:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case 3:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y;
                break;
            case 4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case 5:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case 6:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case 7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            case 8:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            case 9:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            default:
                break;
        }
        rectangleF.X = pointF.X;
        rectangleF.Y= pointF.Y;
        return rectangleF;
    }
    /// <summary>SubRectangleF 方法。</summary>
    public virtual System.Drawing.RectangleF SubRectangleF(System.Drawing.ContentAlignment mode)
    {
        System.Drawing.PointF pointF = new PointF(RectangleFh.X, RectangleFh.Y);
        System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(RectangleFh.X, RectangleFh.Y, RectangleFh.Width, RectangleFh.Height);
        switch (mode)
        {
            case ContentAlignment.TopLeft:
                break;
            case ContentAlignment.TopCenter:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case ContentAlignment.TopRight:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y;
                break;
            case ContentAlignment.MiddleLeft:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case ContentAlignment.MiddleCenter:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case ContentAlignment.MiddleRight:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y - RectangleFh.Height / 2;
                break;
            case ContentAlignment.BottomLeft:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            case ContentAlignment.BottomCenter:
                pointF.X = pointF.X - RectangleFh.Width / 2;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            case ContentAlignment.BottomRight:
                pointF.X = pointF.X - RectangleFh.Width;
                pointF.Y = pointF.Y - RectangleFh.Height;
                break;
            default:
                break;
        }
        rectangleF.X = pointF.X;
        rectangleF.Y = pointF.Y;
        return rectangleF;
    }
    /// <summary>AddPointF 方法。</summary>
    public virtual System.Drawing.PointF AddPointF(int mode)
    {
        System.Drawing.PointF pointF = new PointF(RectangleFh.X, RectangleFh.Y);
        switch (mode)
        {
            case 1:
                break;
            case 2:
                pointF.X = pointF.X + RectangleFh.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case 3:
                pointF.X = pointF.X + RectangleFh.Width;
                pointF.Y = pointF.Y;
                break;
            case 4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + RectangleFh.Height / 2;
                break;
            case 5:
                pointF.X = pointF.X + RectangleFh.Width / 2;
                pointF.Y = pointF.Y + RectangleFh.Height / 2;
                break;
            case 6:
                pointF.X = pointF.X + RectangleFh.Width;
                pointF.Y = pointF.Y + RectangleFh.Height / 2;
                break;
            case 7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + RectangleFh.Height;
                break;
            case 8:
                pointF.X = pointF.X + RectangleFh.Width / 2;
                pointF.Y = pointF.Y + RectangleFh.Height;
                break;
            case 9:
                pointF.X = pointF.X + RectangleFh.Width;
                pointF.Y = pointF.Y + RectangleFh.Height;
                break;
            default:
                break;
        }
        return pointF;
    }
    /// <summary>AddRectangleF 方法。</summary>
    public virtual System.Drawing.RectangleF AddRectangleF(int mode)
    {
        System.Drawing.PointF pointF = new PointF(RectangleFh.X, RectangleFh.Y);
        System.Drawing.RectangleF rectangleF = new System.Drawing.RectangleF(RectangleFh.X, RectangleFh.Y, RectangleFh.Width, RectangleFh.Height);
        switch (mode)
        {
            case 1:
                break;
            case 2:
                pointF.X = pointF.X + RectangleFh.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case 3:
                pointF.X = pointF.X + RectangleFh.Width;
                pointF.Y = pointF.Y;
                break;
            case 4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + RectangleFh.Height / 2;
                break;
            case 5:
                pointF.X = pointF.X + RectangleFh.Width / 2;
                pointF.Y = pointF.Y + RectangleFh.Height / 2;
                break;
            case 6:
                pointF.X = pointF.X + RectangleFh.Width;
                pointF.Y = pointF.Y + RectangleFh.Height / 2;
                break;
            case 7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + RectangleFh.Height;
                break;
            case 8:
                pointF.X = pointF.X + RectangleFh.Width / 2;
                pointF.Y = pointF.Y + RectangleFh.Height;
                break;
            case 9:
                pointF.X = pointF.X + RectangleFh.Width;
                pointF.Y = pointF.Y + RectangleFh.Height;
                break;
            default:
                break;
        }
        rectangleF.X = pointF.X;
        rectangleF.Y = pointF.Y;
        return rectangleF;
    }
    /// <summary>获取 mode。</summary>
    public int GetMode(System.Drawing.ContentAlignment mode)
    {
        switch (mode)
        {
            case ContentAlignment.TopLeft:
                return 1;
            case ContentAlignment.TopCenter:
                return 2;
            case ContentAlignment.TopRight:
                return 3;
            case ContentAlignment.MiddleLeft:
                return 4;
            case ContentAlignment.MiddleCenter:
                return 5;
            case ContentAlignment.MiddleRight:
                return 6;
            case ContentAlignment.BottomLeft:
                return 7;
            case ContentAlignment.BottomCenter:
                return 8;
            case ContentAlignment.BottomRight:
                return 9;
            default:
                return 0;
        }
    }
}
}
