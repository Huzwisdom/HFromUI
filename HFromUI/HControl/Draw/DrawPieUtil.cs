using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    ///  绘制椭圆定义的扇形
    /// </summary>
    public class DrawPieUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; }
    /// <summary>RectangleF 成员。</summary>
    public System.Drawing.RectangleF RectangleF { set; get; }
    /// <summary>StartAngle 成员。</summary>
    public float StartAngle { set; get; } = 0;
    /// <summary>SweepAngle 成员。</summary>
    public float SweepAngle { set; get; } = 360;
    public DrawPieUtil(float x, float y, float width, float height)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
    }
    public DrawPieUtil(System.Drawing.RectangleF rectangleF)
    {
        RectangleF = rectangleF;
    }
    public DrawPieUtil(System.Drawing.RectangleF rectangleF, float startAngle, float sweepAngle)
    {
        RectangleF = rectangleF;
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
    }
    public DrawPieUtil(float x, float y, float width, float height, float startAngle, float sweepAngle)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
    }
    public DrawPieUtil(int x, int y, int width, int height)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
    }
    public DrawPieUtil(int x, int y, int width, int height, float startAngle, float sweepAngle)
    {
        RectangleF = new System.Drawing.RectangleF(x, y, width, height);
        StartAngle = startAngle;
        SweepAngle = sweepAngle;
    }
    /// <summary>DrawPie 方法。</summary>
    public virtual bool DrawPie(System.Drawing.Graphics graphics)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawPie(Pen, RectangleF, StartAngle, SweepAngle);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>DrawPieAngle 方法。</summary>
    public virtual bool DrawPieAngle(System.Drawing.Graphics graphics, float startAngle, float endAngle)
    {
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawPie(Pen, RectangleF, startAngle, endAngle - startAngle);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>DrawPies 方法。</summary>
    public virtual bool DrawPies(System.Drawing.Graphics graphics, float[] angle, Brush[] brush)
    {
        try
        {
            for (int num = 0; num < angle.Length - 1; num++)
            {
                DrawPieAngle(graphics, angle[num], angle[num + 1]);
                if (brush != null && num < brush.Length)
                {
                    FillPieAngle(graphics, brush[num], angle[num], angle[num + 1]);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>获取 pieCenterPointF。</summary>
    public virtual PointF GetPieCenterPointF()
    {
        float CenterX = 0;
        float CenterY = 0;
        float PieX = (RectangleF.X + RectangleF.Width) / 2;
        float PieY = (RectangleF.Y + RectangleF.Height) / 2;
        float PieA = StartAngle + SweepAngle / 2;
        CenterX = PieX + PieX / 2 * Convert.ToSingle(Math.Cos((PieA % 360) * Math.PI / 180));
        CenterY = PieY + PieY / 2 * Convert.ToSingle(Math.Sin((PieA % 360) * Math.PI / 180));
        return new PointF(CenterX, CenterY);
    }

    /// <summary>获取 pieCenterPartPointF。</summary>
    public virtual PointF GetPieCenterPartPointF(float partX, float partY)
    {
        float CenterX = 0;
        float CenterY = 0;
        float PieX = (RectangleF.X + RectangleF.Width) / 2;
        float PieY = (RectangleF.Y + RectangleF.Height) / 2;
        float PieA = StartAngle + SweepAngle / 2;
        CenterX = PieX + PieX * partX * Convert.ToSingle(Math.Cos((PieA % 360) * Math.PI / 180));
        CenterY = PieY + PieY * partY * Convert.ToSingle(Math.Sin((PieA % 360) * Math.PI / 180));
        return new PointF(CenterX, CenterY);
    }
    /// <summary>获取 piePartPointF。</summary>
    public virtual PointF GetPiePartPointF(float partX, float partY, float partAngle)
    {
        float CenterX = 0;
        float CenterY = 0;
        float PieX = (RectangleF.X + RectangleF.Width) / 2;
        float PieY = (RectangleF.Y + RectangleF.Height) / 2;
        float PieA = StartAngle + SweepAngle * partAngle;
        CenterX = PieX + PieX * partX * Convert.ToSingle(Math.Cos((PieA % 360) * Math.PI / 180));
        CenterY = PieY + PieY * partY * Convert.ToSingle(Math.Sin((PieA % 360) * Math.PI / 180));
        return new PointF(CenterX, CenterY);
    }

    /// <summary>FillPie 方法。</summary>
    public virtual bool FillPie(System.Drawing.Graphics graphics, Brush brush)
    {
        try
        {
            graphics.FillPie(brush, RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height, StartAngle, SweepAngle);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>FillPieAngle 方法。</summary>
    public virtual bool FillPieAngle(System.Drawing.Graphics graphics, Brush brush, float startAngle, float endAngle)
    {
        try
        {
            graphics.FillPie(brush, RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height, startAngle, endAngle - startAngle);
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

    /// <summary>获取 graphicsPath。</summary>
    public virtual System.Drawing.Drawing2D.GraphicsPath GetGraphicsPath()
    {
        System.Drawing.Drawing2D.GraphicsPath graphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
        graphicsPath.AddPie(RectangleF.X, RectangleF.Y, RectangleF.Width, RectangleF.Height, StartAngle, SweepAngle);
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
