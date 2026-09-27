using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    ///  绘制一个由中心点和直径定义的圆
    /// </summary>
    public class DrawRoundUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; }
    /// <summary>PointFCenter 成员。</summary>
    public System.Drawing.PointF PointFCenter { set; get; }
    /// <summary>Radius 成员。</summary>
    public float Radius { set; get; } = 10;

    public DrawRoundUtil(float centerX, float centerY)
    {
        PointFCenter = new System.Drawing.PointF(centerX, centerY);
    }
    public DrawRoundUtil(PointF pointF)
    {
        PointFCenter = pointF;
    }
    public DrawRoundUtil(PointF pointF, float radius)
    {
        PointFCenter = pointF;
        Radius = radius;
    }
    public DrawRoundUtil(float centerX, float centerY, float radius)
    {
        PointFCenter = new System.Drawing.PointF(centerX, centerY);
        Radius = radius;
    }
    /// <summary>DrawRound 方法。</summary>
    public virtual bool DrawRound(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawEllipse(Pen, PointFCenter.X - Radius / 2, PointFCenter.Y - Radius / 2, Radius, Radius);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>FillRound 方法。</summary>
    public virtual bool FillRound(System.Drawing.Graphics graphics, Brush brush)
    {
        try
        {
            graphics.FillEllipse(brush, PointFCenter.X - Radius / 2, PointFCenter.Y - Radius / 2, Radius, Radius);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillRound 方法。</summary>
    public virtual bool FillRound(System.Drawing.Graphics graphics, Color color)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillEllipse(brush, PointFCenter.X - Radius / 2, PointFCenter.Y - Radius / 2, Radius, Radius);
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
    /// <summary>FillRoundString 方法。</summary>
    public virtual bool FillRoundString(System.Drawing.Graphics graphics, Color color, string content, Color contentColor)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillEllipse(brush, PointFCenter.X - Radius / 2, PointFCenter.Y - Radius / 2, Radius, Radius);
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = PointFCenter;
            drawStringUtil.SizeF = drawStringUtil.GetSizeF(graphics);
            drawStringUtil.PointOffset(5);
            drawStringUtil.DrawString(graphics, contentColor);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillRoundString 方法。</summary>
    public virtual bool FillRoundString(System.Drawing.Graphics graphics, Color color, string content,Font contentFont, Color contentColor)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillEllipse(brush, PointFCenter.X - Radius / 2, PointFCenter.Y - Radius / 2, Radius, Radius);
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.Font = contentFont;
            drawStringUtil.PointF = PointFCenter;
            drawStringUtil.SizeF = drawStringUtil.GetSizeF(graphics);
            drawStringUtil.PointOffset(5);
            drawStringUtil.DrawString(graphics, contentColor);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>获取 graphicsPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddEllipse(PointFCenter.X - Radius / 2, PointFCenter.Y - Radius / 2, Radius, Radius);
        return graphicsPath;
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF)
    {
        return GetGraphicsPath().IsVisible(pointF.X, pointF.Y);
    }

    /// <summary>获取 rectangleF。</summary>
    public virtual System.Drawing.RectangleF GetRectangleF(float percentage)
    {
      return new System.Drawing.RectangleF(PointFCenter.X- percentage * Radius/2, PointFCenter.Y - percentage * Radius / 2, percentage* Radius, percentage * Radius);
    }
}
}
