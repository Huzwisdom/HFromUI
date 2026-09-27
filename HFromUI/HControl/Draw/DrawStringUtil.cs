using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    public class DrawStringUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>字体。</summary>
    public System.Drawing.Font Font { set; get; } 
    /// <summary>PointF 成员。</summary>
    public System.Drawing.PointF PointF { set; get; } = new PointF();
    /// <summary>SizeF 成员。</summary>
    public System.Drawing.SizeF SizeF { set; get; }
    /// <summary>Length 成员。</summary>
    public int Length { get { return Content.Length; } }
    /// <summary>Content 成员。</summary>
    public string Content { set; get; }

    public DrawStringUtil(string content)
    {
        Content = content;
    }
    public DrawStringUtil()
    {

    }
    /// <summary>绘制文本。</summary>
    public virtual bool DrawString(System.Drawing.Graphics graphics, Color color)
    {
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            SolidBrush brush = new SolidBrush(color);
            if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
            {
                graphics.DrawString(Content, Font, brush, PointF);
            }
            else
            {
                System.Drawing.RectangleF RectangleF = new System.Drawing.RectangleF(PointF.X, PointF.Y, SizeF.Width, SizeF.Height);
                graphics.DrawString(Content, Font, brush, RectangleF);
            }
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>绘制文本。</summary>
    public virtual bool DrawString(System.Drawing.Graphics graphics, Color color, System.Drawing.PointF rotationPointF, float angle)
    {
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            SolidBrush brush = new SolidBrush(color);
            System.Drawing.Drawing2D.Matrix matrix = graphics.Transform;
            matrix.RotateAt(angle, rotationPointF);
            graphics.Transform = matrix;
            if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
            {
                graphics.DrawString(Content, Font, brush, PointF);
            }
            else
            {
                System.Drawing.RectangleF RectangleF = new System.Drawing.RectangleF(PointF.X, PointF.Y, SizeF.Width, SizeF.Height);
                graphics.DrawString(Content, Font, brush, RectangleF);
            }
            graphics.ResetTransform();
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>绘制文本。</summary>
    public virtual bool DrawString(System.Drawing.Graphics graphics, Brush brush)
    {
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
            {
                graphics.DrawString(Content, Font, brush, PointF);
            }
            else
            {
                System.Drawing.RectangleF RectangleF = new System.Drawing.RectangleF(PointF.X, PointF.Y, SizeF.Width, SizeF.Height);
                graphics.DrawString(Content, Font, brush, RectangleF);
            }
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>绘制文本。</summary>
    public virtual bool DrawString(System.Drawing.Graphics graphics, Brush brush, System.Drawing.PointF rotationPointF, float angle)
    {
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            System.Drawing.Drawing2D.Matrix matrix = graphics.Transform;
            matrix.RotateAt(angle, rotationPointF);
            graphics.Transform = matrix;
            if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
            {
                graphics.DrawString(Content, Font, brush, PointF);
            }
            else
            {
                System.Drawing.RectangleF RectangleF = new System.Drawing.RectangleF(PointF.X, PointF.Y, SizeF.Width, SizeF.Height);
                graphics.DrawString(Content, Font, brush, RectangleF);
            }
            graphics.ResetTransform();
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>绘制文本。</summary>
    public virtual bool DrawString(System.Drawing.Graphics graphics, Brush brush, StringFormat format)
    {
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
            {
                graphics.DrawString(Content, Font, brush, PointF.X, PointF.Y, format);
            }
            else
            {
                System.Drawing.RectangleF RectangleF = new System.Drawing.RectangleF(PointF.X, PointF.Y, SizeF.Width, SizeF.Height);
                graphics.DrawString(Content, Font, brush, RectangleF, format);
            }
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>绘制文本。</summary>
    public virtual bool DrawString(System.Drawing.Graphics graphics, Brush brush, StringFormat format, System.Drawing.PointF rotationPointF, float angle)
    {
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            System.Drawing.Drawing2D.Matrix matrix = graphics.Transform;
            matrix.RotateAt(angle, rotationPointF);
            graphics.Transform = matrix;
            if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
            {
                graphics.DrawString(Content, Font, brush, PointF.X, PointF.Y, format);
            }
            else
            {
                System.Drawing.RectangleF RectangleF = new System.Drawing.RectangleF(PointF.X, PointF.Y, SizeF.Width, SizeF.Height);
                graphics.DrawString(Content, Font, brush, RectangleF, format);
            }
            graphics.ResetTransform();
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }
    /// <summary>获取 mode。</summary>
    public virtual int GetMode(System.Drawing.ContentAlignment mode)
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

    /// <summary>PointOffset 方法。</summary>
    public virtual void PointOffset(int mode)
    {
        if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
        {
            return;
        }
        System.Drawing.PointF pointF = new PointF(PointF.X, PointF.Y);
        switch (mode)
        {
            case -1:
                break;
            case -2:
                pointF.X = pointF.X + SizeF.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case -3:
                pointF.X = pointF.X + SizeF.Width;
                pointF.Y = pointF.Y;
                break;
            case -4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + SizeF.Height / 2;
                break;
            case -5:
                pointF.X = pointF.X + SizeF.Width / 2;
                pointF.Y = pointF.Y + SizeF.Height / 2;
                break;
            case -6:
                pointF.X = pointF.X + SizeF.Width;
                pointF.Y = pointF.Y + SizeF.Height / 2;
                break;
            case -7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + SizeF.Height;
                break;
            case -8:
                pointF.X = pointF.X + SizeF.Width / 2;
                pointF.Y = pointF.Y + SizeF.Height;
                break;
            case -9:
                pointF.X = pointF.X + SizeF.Width;
                pointF.Y = pointF.Y + SizeF.Height;
                break;
            case 1:
                break;
            case 2:
                pointF.X = pointF.X - SizeF.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case 3:
                pointF.X = pointF.X - SizeF.Width;
                pointF.Y = pointF.Y;
                break;
            case 4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - SizeF.Height / 2;
                break;
            case 5:
                pointF.X = pointF.X - SizeF.Width / 2;
                pointF.Y = pointF.Y - SizeF.Height / 2;
                break;
            case 6:
                pointF.X = pointF.X - SizeF.Width;
                pointF.Y = pointF.Y - SizeF.Height / 2;
                break;
            case 7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - SizeF.Height;
                break;
            case 8:
                pointF.X = pointF.X - SizeF.Width / 2;
                pointF.Y = pointF.Y - SizeF.Height;
                break;
            case 9:
                pointF.X = pointF.X - SizeF.Width;
                pointF.Y = pointF.Y - SizeF.Height;
                break;
            default:
                break;
        }
        PointF = pointF;
    }

    /// <summary>获取 modePointF。</summary>
    public virtual System.Drawing.PointF GetModePointF(int mode)
    {
        if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
        {
            return PointF;
        }
        System.Drawing.PointF pointF = new PointF(PointF.X, PointF.Y);
        switch (mode)
        {
            case -1:
                break;
            case -2:
                pointF.X = pointF.X + SizeF.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case -3:
                pointF.X = pointF.X + SizeF.Width;
                pointF.Y = pointF.Y;
                break;
            case -4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + SizeF.Height / 2;
                break;
            case -5:
                pointF.X = pointF.X + SizeF.Width / 2;
                pointF.Y = pointF.Y + SizeF.Height / 2;
                break;
            case -6:
                pointF.X = pointF.X + SizeF.Width;
                pointF.Y = pointF.Y + SizeF.Height / 2;
                break;
            case -7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y + SizeF.Height;
                break;
            case -8:
                pointF.X = pointF.X + SizeF.Width / 2;
                pointF.Y = pointF.Y + SizeF.Height;
                break;
            case -9:
                pointF.X = pointF.X + SizeF.Width;
                pointF.Y = pointF.Y + SizeF.Height;
                break;
            case 1:
                break;
            case 2:
                pointF.X = pointF.X - SizeF.Width / 2;
                pointF.Y = pointF.Y;
                break;
            case 3:
                pointF.X = pointF.X - SizeF.Width;
                pointF.Y = pointF.Y;
                break;
            case 4:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - SizeF.Height / 2;
                break;
            case 5:
                pointF.X = pointF.X - SizeF.Width / 2;
                pointF.Y = pointF.Y - SizeF.Height / 2;
                break;
            case 6:
                pointF.X = pointF.X - SizeF.Width;
                pointF.Y = pointF.Y - SizeF.Height / 2;
                break;
            case 7:
                pointF.X = pointF.X;
                pointF.Y = pointF.Y - SizeF.Height;
                break;
            case 8:
                pointF.X = pointF.X - SizeF.Width / 2;
                pointF.Y = pointF.Y - SizeF.Height;
                break;
            case 9:
                pointF.X = pointF.X - SizeF.Width;
                pointF.Y = pointF.Y - SizeF.Height;
                break;
            default:
                break;
        }
      return pointF;
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return GetSizeF;
        }
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics, StringFormat stringFormat)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font,PointF, stringFormat);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return GetSizeF;
        }
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics, int width)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font, width);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return GetSizeF;
        }
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics, int width, StringFormat stringFormat)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font, width, stringFormat);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return GetSizeF;
        }
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics ,System.Drawing.SizeF layoutArea)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font, layoutArea);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return GetSizeF;
        }
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics, System.Drawing.SizeF layoutArea, StringFormat stringFormat)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font, layoutArea, stringFormat);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return GetSizeF;
        }
    }
    /// <summary>获取 sizeF。</summary>
    public virtual System.Drawing.SizeF GetSizeF(System.Drawing.Graphics graphics, System.Drawing.SizeF layoutArea,  StringFormat stringFormat, out int charactersFitted, out int linesFilled)
    {
        System.Drawing.SizeF GetSizeF = new SizeF();
        try
        {
            if (Font == null)
            {
                Font = new Font("黑体", 9, FontStyle.Regular);
            }
            GetSizeF = graphics.MeasureString(Content, Font, layoutArea, stringFormat,out charactersFitted,out linesFilled);
            return GetSizeF;
        }
        catch (Exception ex)
        {
            charactersFitted = 0;
            linesFilled = 0;
            StrError = ex.Message;
            return GetSizeF;
        }
    }

    /// <summary>获取 centerPointF。</summary>
    public virtual System.Drawing.PointF GetCenterPointF()
    {
        if (SizeF == null || (SizeF.Width == 0 && SizeF.Height == 0))
        {
            return new System.Drawing.PointF(PointF.X , PointF.Y);
        }
        return new System.Drawing.PointF(PointF.X + SizeF.Width/2, PointF.Y + SizeF.Height/2);
    }


}
}
