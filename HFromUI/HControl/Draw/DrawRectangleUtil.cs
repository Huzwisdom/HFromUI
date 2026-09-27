using System;
using System.Drawing;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    ///  绘制结构指定的矩形
    /// </summary>
    public class DrawRectangleUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; } 
    /// <summary>RectangleF 成员。</summary>
    public System.Drawing.RectangleF RectangleF { set; get; }
    public DrawRectangleUtil(System.Drawing.RectangleF rectangleF)
    {
        RectangleF = rectangleF;
    }
    public DrawRectangleUtil(float x, float y, float width, float height)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
    }
    public DrawRectangleUtil(int x, int y, int width, int height)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
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
    /// <summary>绘制矩形。</summary>
    public virtual bool DrawRectangle(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawRectangle(Pen, RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height);
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
            graphics.FillRectangle(brush, RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height);
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
    /// <summary>获取 graphicsPathRadius。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPathRadius(float side = 1)
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddArc(RectangleF.X - side, RectangleF.Y - side, RectangleF.Width - 2 * side, RectangleF.Height - 2 * side, 0, 360);
        return graphicsPath;
    }
    /// <summary>
    /// 表示一系列相互连接的直线和曲线.获取位图范围
    /// </summary>
    /// <param name="radius">圆角半径</param>
    /// <param name="topLeft">上左是否圆角处理</param>
    /// <param name="topRight">上右是否圆角处理</param>
    /// <param name="buttomRight">下右是否圆角处理</param>
    /// <param name="buttomLeft">下左是否圆角处理</param>
    /// <returns>表示一系列相互连接的直线和曲线.获取位图范围</returns>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPathRadius(int radius, bool topLeft, bool topRight, bool buttomRight, bool buttomLeft)
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        DrawLineUtil drawLineUtil1 = new DrawLineUtil(RectangleF.X + (topLeft ? radius : 0), RectangleF.Y, RectangleF.X + RectangleF.Width - 1 - (topRight ? radius : 0), RectangleF.Y);
        graphicsPath.AddLine(drawLineUtil1.PointFBegin, drawLineUtil1.PointFEnd);
        if (topRight && radius > 0)
        {
            graphicsPath.AddArc(RectangleF.X + RectangleF.Width - radius * 2 - 1, RectangleF.Y, radius * 2, radius * 2, 270f, 90f);
        }
        DrawLineUtil drawLineUtil2 = new DrawLineUtil(RectangleF.X + RectangleF.Width - 1, RectangleF.Y + (topRight ? radius : 0), RectangleF.X + RectangleF.Width - 1, RectangleF.Y + RectangleF.Height - 1 - (buttomRight ? radius : 0));
        graphicsPath.AddLine(drawLineUtil2.PointFBegin, drawLineUtil2.PointFEnd);
        if (buttomRight && radius > 0)
        {
            graphicsPath.AddArc(RectangleF.X + RectangleF.Width - radius * 2 - 1, RectangleF.Y + RectangleF.Height - radius * 2 - 1, radius * 2, radius * 2, 0f, 90f);
        }
        DrawLineUtil drawLineUtil3 = new DrawLineUtil(RectangleF.X + RectangleF.Width - 1 - (buttomRight ? radius : 0), RectangleF.Y + RectangleF.Height - 1, RectangleF.X + (buttomLeft ? radius : 0), RectangleF.Y + RectangleF.Height - 1);
        graphicsPath.AddLine(drawLineUtil3.PointFBegin, drawLineUtil3.PointFEnd);
        if (buttomLeft && radius > 0)
        {
            graphicsPath.AddArc(RectangleF.X, RectangleF.Y + RectangleF.Height - radius * 2 - 1, radius * 2, radius * 2, 90f, 90f);
        }
        DrawLineUtil drawLineUtil4 = new DrawLineUtil(RectangleF.X, RectangleF.Y + RectangleF.Height - 1 - (buttomLeft ? radius : 0), RectangleF.X, RectangleF.Y + (topLeft ? radius : 0));
        graphicsPath.AddLine(drawLineUtil4.PointFBegin, drawLineUtil4.PointFEnd);
        if (topLeft && radius > 0)
        {
            graphicsPath.AddArc(RectangleF.X, RectangleF.Y, radius * 2, radius * 2, 180f, 90f);
        }
        return graphicsPath;
    }
    /// <summary>获取 roundedRectPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(float radius)
    {
        RectangleF rectangleB = new RectangleF(RectangleF.Location, new SizeF(radius, radius));
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddArc(rectangleB, 180f, 90f);
        rectangleB.X = RectangleF.Right - radius;
        graphicsPath.AddArc(rectangleB, 270f, 90f);
        rectangleB.Y = RectangleF.Bottom - radius;
        rectangleB.Width += 1;
        rectangleB.Height += 1;
        graphicsPath.AddArc(rectangleB, 360f, 90f);
        rectangleB.X = RectangleF.Left;
        graphicsPath.AddArc(rectangleB, 90f, 90f);
        graphicsPath.CloseFigure();
        return graphicsPath;
    }
    /// <summary>获取 roundedRectPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(RectangleF rectangleF ,float radius)
    {
        RectangleF rectangleB = new RectangleF(rectangleF.Location, new SizeF(radius, radius));
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddArc(rectangleB, 180f, 90f);
        rectangleB.X = rectangleF.Right - radius;
        graphicsPath.AddArc(rectangleB, 270f, 90f);
        rectangleB.Y = rectangleF.Bottom - radius;
        rectangleB.Width += 1;
        rectangleB.Height += 1;
        graphicsPath.AddArc(rectangleB, 360f, 90f);
        rectangleB.X = rectangleF.Left;
        graphicsPath.AddArc(rectangleB, 90f, 90f);
        graphicsPath.CloseFigure();
        return graphicsPath;
    }
    /// <summary>获取 graphicsPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddRectangle(RectangleF);
        return graphicsPath;
    }
    /// <summary>判断是否 Visible。</summary>
    public virtual bool IsVisible(System.Drawing.PointF pointF)
    {
        return GetGraphicsPath().IsVisible(pointF.X, pointF.Y);
    }
    /// <summary>DrawRoundedRectPath 方法。</summary>
    public virtual bool DrawRoundedRectPath(System.Drawing.Graphics graphics, float radius)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawPath(Pen, GetRoundedRectPath(radius));
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>DrawRoundedRectPath 方法。</summary>
    public virtual bool DrawRoundedRectPath(System.Drawing.Graphics graphics,Color colorLine,float widthLine, float radius)
    {
        try
        {
            SolidBrush brush = new SolidBrush(colorLine);
            System.Drawing.Drawing2D.GraphicsPath graphicsPath = GetRoundedRectPath(new System.Drawing.RectangleF(RectangleF.X + widthLine , RectangleF.Y + widthLine , RectangleF.Width - 2*widthLine, RectangleF.Height - 2 * widthLine), radius);
            graphicsPath.AddPath(GetRoundedRectPath(radius),true);
            graphicsPath.CloseFigure();
            graphics.FillPath(brush, GetRoundedRectPath(radius));
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillRoundedRectPathString 方法。</summary>
    public virtual bool FillRoundedRectPathString(System.Drawing.Graphics graphics, Color color, string content, Color contentColor,float radius)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillPath(brush, GetRoundedRectPath(radius));
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = GetCenterPointF();
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
    /// <summary>FillRoundedRectPathString 方法。</summary>
    public virtual bool FillRoundedRectPathString(System.Drawing.Graphics graphics, Color color, string content, Font contentFont, Color contentColor, float radius)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillPath(brush, GetRoundedRectPath(radius));
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = GetCenterPointF();
            drawStringUtil.Font = contentFont;
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
    /// <summary>FillRoundedRectPathString 方法。</summary>
    public virtual bool FillRoundedRectPathString(System.Drawing.Graphics graphics, Color color, string content, Font contentFont, Color contentColor, float radius,float offsetX,float offsetY)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillPath(brush, GetRoundedRectPath(radius));
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = GetCenterPointF();
            drawStringUtil.Font = contentFont;
            drawStringUtil.SizeF = drawStringUtil.GetSizeF(graphics);
            drawStringUtil.PointOffset(5);
            drawStringUtil.PointF = new PointF(drawStringUtil.PointF.X- offsetX, drawStringUtil.PointF.Y- offsetY);
            drawStringUtil.DrawString(graphics, contentColor);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillRoundedRectPathString 方法。</summary>
    public virtual bool FillRoundedRectPathString(System.Drawing.Graphics graphics, Color color, string content, Font contentFont, Color contentColor, float radius, float offsetX, float offsetY, System.Drawing.ContentAlignment textAlign)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillPath(brush, GetRoundedRectPath(radius));
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = GetCenterPointF();
            drawStringUtil.Font = contentFont;
            drawStringUtil.SizeF = drawStringUtil.GetSizeF(graphics);
            drawStringUtil.PointOffset(drawStringUtil.GetMode(textAlign));
            drawStringUtil.PointF = new PointF(drawStringUtil.PointF.X - offsetX, drawStringUtil.PointF.Y - offsetY);
            drawStringUtil.DrawString(graphics, contentColor);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>FillRectangleString 方法。</summary>
    public virtual bool FillRectangleString(System.Drawing.Graphics graphics, Color color, string content, Color contentColor)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillRectangle(brush, RectangleF);
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = GetCenterPointF();
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

    /// <summary>FillRectangleString 方法。</summary>
    public virtual bool FillRectangleString(System.Drawing.Graphics graphics, Color color, string content, Font contentFont, Color contentColor)
    {
        try
        {
            SolidBrush brush = new SolidBrush(color);
            graphics.FillRectangle(brush, RectangleF);
            DrawStringUtil drawStringUtil = new DrawStringUtil(content);
            drawStringUtil.PointF = GetCenterPointF();
            drawStringUtil.Font = contentFont;
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
}
}
