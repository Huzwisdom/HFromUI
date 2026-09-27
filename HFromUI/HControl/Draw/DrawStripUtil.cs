using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    public class DrawStripUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }

    /// <summary>rectangleAllF 字段。</summary>
    private System.Drawing.RectangleF rectangleAllF = new System.Drawing.RectangleF();
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; } 
    /// <summary>PointTopLeftF 成员。</summary>
    public System.Drawing.PointF PointTopLeftF { get; private set; }
    public System.Drawing.RectangleF RectangleAllF
    {
        get
        {
            return rectangleAllF;
        }
        set
        {
            PointTopLeftF = new System.Drawing.PointF(value.X, value.Y);
            rectangleAllF = value;
        }
    }
    /// <summary>获取 rectangleGridF。</summary>
    public System.Drawing.RectangleF GetRectangleGridF(float x1, float x2, float y1, float y2, float a1, float a2)
    {
        System.Drawing.RectangleF rectangleGridF = new System.Drawing.RectangleF(RectangleAllF.X + x1, RectangleAllF.Y + y1 + a1, RectangleAllF.Width - x1 - x2 - a2, RectangleAllF.Height - y1 - y2 - a1);
        return rectangleGridF;
    }

    /// <summary>DrawStrip 方法。</summary>
    public void DrawStrip(System.Drawing.Graphics g, float x, float y, float width, float height, int stripX, int stripY)
    {
        if (Pen==null)
        {
            Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
        }
        DrawLinesUtil drawLinesUtil_Grid = new DrawLinesUtil();
        for (int stripNum = 0; stripNum <= stripX; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(x + stripNum * (width /stripX ), y, x + stripNum * (width /stripX ), y + height);
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        for (int stripNum = 0; stripNum <= stripY; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(x, y + stripNum * (height / stripY ), x + width, y + stripNum * (height / stripY ));
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        drawLinesUtil_Grid.DrawLines(g);
    }

    /// <summary>DrawStrip 方法。</summary>
    public void DrawStrip(System.Drawing.Graphics g, float x, float y, float width, float height, int stripX, int stripY, float addLong)
    {
        if (Pen == null)
        {
            Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
        }
        DrawLinesUtil drawLinesUtil_Grid = new DrawLinesUtil();
        for (int stripNum = 0; stripNum <= stripX; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(x + stripNum * (width /stripX ), y - addLong, x + stripNum * (width /stripX ), y + height + addLong);
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        for (int stripNum = 0; stripNum <= stripY; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(x - addLong, y + stripNum * (height / stripY ), x + width + addLong, y + stripNum * (height / stripY ));
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        drawLinesUtil_Grid.DrawLines(g);
    }

    /// <summary>DrawStrip 方法。</summary>
    public void DrawStrip(System.Drawing.Graphics g, System.Drawing.RectangleF rectangleGridF, int stripX, int stripY)
    {
        if (Pen == null)
        {
            Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
        }
        DrawLinesUtil drawLinesUtil_Grid = new DrawLinesUtil();
        for (int stripNum = 0; stripNum <= stripX; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(rectangleGridF.X + stripNum * (rectangleGridF.Width /stripX ), rectangleGridF.Y, rectangleGridF.X + stripNum * (rectangleGridF.Width /stripX ), rectangleGridF.Y + rectangleGridF.Height);
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        for (int stripNum = 0; stripNum <= stripY; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(rectangleGridF.X, rectangleGridF.Y + stripNum * (rectangleGridF.Height / stripY ), rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + stripNum * (rectangleGridF.Height / stripY ));
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        drawLinesUtil_Grid.DrawLines(g);
    }

    /// <summary>DrawStrip 方法。</summary>
    public void DrawStrip(System.Drawing.Graphics g, System.Drawing.RectangleF rectangleGridF, int stripX, int stripY, float addLong)
    {
        if (Pen == null)
        {
            Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
        }
        DrawLinesUtil drawLinesUtil_Grid = new DrawLinesUtil();
        for (int stripNum = 0; stripNum <= stripX; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(rectangleGridF.X + stripNum * (rectangleGridF.Width /stripX ), rectangleGridF.Y - addLong, rectangleGridF.X + stripNum * (rectangleGridF.Width /stripX ), rectangleGridF.Y + rectangleGridF.Height + addLong);
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        for (int stripNum = 0; stripNum <= stripY; stripNum++)
        {
            DrawLineUtil drawLineUtil = new DrawLineUtil(rectangleGridF.X - addLong, rectangleGridF.Y + stripNum * (rectangleGridF.Height / stripY ), rectangleGridF.X + rectangleGridF.Width + addLong, rectangleGridF.Y + stripNum * (rectangleGridF.Height / stripY ));
            drawLineUtil.Pen = Pen;
            drawLinesUtil_Grid.linesUtilList.Add(drawLineUtil);
        }
        drawLinesUtil_Grid.DrawLines(g);
    }

    /// <summary>
    /// 画坐标系轴X，Y
    /// </summary>
    /// <param name="g">一个 GDI+ 绘图图面</param>
    /// <param name="rectangleGridF">绘图坐标系的有效区域</param>
    /// <param name="beyondX">X轴超出有效区域长度</param>
    /// <param name="beyondY">Y轴超出有效区域长度</param>
    /// <param name="originAdd">超出原点有效区域长度</param>
    /// <param name="arrowheadW">箭头长度</param>
    /// <param name="arrowheadH">箭头宽度</param>
    public void DrawCoordinate(System.Drawing.Graphics g, System.Drawing.RectangleF rectangleGridF, float beyondX, float beyondY, float originAdd, float arrowheadW, float arrowheadH)
    {
        if (Pen == null)
        {
            Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
        }
        DrawLinesUtil drawLinesUtil_Grid = new DrawLinesUtil();
        DrawLineUtil drawLineUtilX = new DrawLineUtil(rectangleGridF.X, rectangleGridF.Y - beyondX, rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height + originAdd);
        drawLinesUtil_Grid.linesUtilList.Add(drawLineUtilX);
        DrawLineUtil drawLineUtilY = new DrawLineUtil(rectangleGridF.X - originAdd, rectangleGridF.Y + rectangleGridF.Height, rectangleGridF.X + rectangleGridF.Width + beyondY, rectangleGridF.Y + rectangleGridF.Height);
        drawLinesUtil_Grid.linesUtilList.Add(drawLineUtilY);
        drawLinesUtil_Grid.DrawLines(g);
        RectangleFConvertUtil RectangleFConvertUtil = new RectangleFConvertUtil(rectangleGridF.X, rectangleGridF.Y - beyondX, arrowheadW, arrowheadH);
        DrawTriangleUtil drawTriangleUtilX = new DrawTriangleUtil(RectangleFConvertUtil.SubRectangleF(8));
        drawTriangleUtilX.FillTriangle(g, Pen.Color);
        RectangleFConvertUtil = new RectangleFConvertUtil(rectangleGridF.X + rectangleGridF.Width + beyondY, rectangleGridF.Y + rectangleGridF.Height, arrowheadH, arrowheadW);
        DrawTriangleUtil drawTriangleUtilY = new DrawTriangleUtil(RectangleFConvertUtil.SubRectangleF(4), 2);
        drawTriangleUtilY.FillTriangle(g, Pen.Color);
    }

    /// <summary>获取 punctuation。</summary>
    public System.Drawing.PointF GetPunctuation(System.Drawing.RectangleF rectangleGridF, double part, int mode = 1)
    {
        return GetPunctuation(rectangleGridF, (float)part, mode);
    }
    /// <summary>获取 punctuation。</summary>
    public System.Drawing.PointF GetPunctuation(System.Drawing.RectangleF rectangleGridF,float part,int mode=1)
    {
        System.Drawing.PointF pointF ; ;
        DrawLineUtil drawLineUtilX ; ;
       switch (mode)
        {
            case 1:
                drawLineUtilX = new DrawLineUtil(rectangleGridF.X, rectangleGridF.Y+ rectangleGridF.Height, rectangleGridF.X+ rectangleGridF.Width, rectangleGridF.Y+ rectangleGridF.Height);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case 2:
                drawLineUtilX = new DrawLineUtil(rectangleGridF.X, rectangleGridF.Y , rectangleGridF.X , rectangleGridF.Y + rectangleGridF.Height);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case 3:
                drawLineUtilX = new DrawLineUtil(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y , rectangleGridF.X, rectangleGridF.Y );
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case 4:
                drawLineUtilX = new DrawLineUtil(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height, rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y );
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case -1:
                drawLineUtilX = new DrawLineUtil(rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height,rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case -2:
                drawLineUtilX = new DrawLineUtil( rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height,rectangleGridF.X, rectangleGridF.Y);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case -3:
                drawLineUtilX = new DrawLineUtil( rectangleGridF.X, rectangleGridF.Y,rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            case -4:
                drawLineUtilX = new DrawLineUtil( rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y,rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
            default:
                drawLineUtilX = new DrawLineUtil(rectangleGridF.X, rectangleGridF.Y + rectangleGridF.Height, rectangleGridF.X + rectangleGridF.Width, rectangleGridF.Y + rectangleGridF.Height);
                pointF = drawLineUtilX.GetPartPointF(part);
                break;
        }
        return pointF;
    }

    /// <summary>获取 routineCoordinatePointF。</summary>
    public System.Drawing.PointF GetRoutineCoordinatePointF(System.Drawing.RectangleF rectangleGridF,float partX, float partY)
    {
        return new System.Drawing.PointF(GetPunctuation(rectangleGridF, partX,1).X, GetPunctuation(rectangleGridF, partY,-2).Y);
    }
    /// <summary>获取 routineCoordinatePointF。</summary>
    public System.Drawing.PointF GetRoutineCoordinatePointF(System.Drawing.RectangleF rectangleGridF, double partX, double partY)
    {
        return new System.Drawing.PointF(GetPunctuation(rectangleGridF, (float)partX, 1).X, GetPunctuation(rectangleGridF, (float)partY, -2).Y);
    }
}
}
