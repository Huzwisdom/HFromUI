using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
namespace HFromUI.HControl.Draw
{
    public class DrawStripDataUtil
{
    /// <summary>overallRectangleF 字段。</summary>
    private RectangleF overallRectangleF = new RectangleF();

    /// <summary>gridX1 字段。</summary>
    private float gridX1 = 50;
    /// <summary>gridX2 字段。</summary>
    private float gridX2 = 50;
    /// <summary>gridY1 字段。</summary>
    private float gridY1 = 50;
    /// <summary>gridY2 字段。</summary>
    private float gridY2 = 80;
    /// <summary>gridA1 字段。</summary>
    private float gridA1 = 20;
    /// <summary>gridA2 字段。</summary>
    private float gridA2 = 30;
    /// <summary>splitX 字段。</summary>
    private int splitX = 15;
    /// <summary>splitY 字段。</summary>
    private int splitY = 10;
    public RectangleF OverallRectangleF
    {
        set
        {
            overallRectangleF = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
        }
        get
        {
         
            return overallRectangleF;
        }
    }
    /// <summary>名称。</summary>
    public string Name { set; get; }

    /// <summary>CurvePointList 成员。</summary>
    public List<CurvePoint> CurvePointList { set; get; } = new List<CurvePoint>();

    /// <summary>NameX 成员。</summary>
    public string NameX { set; get; }

    /// <summary>DataNameX 成员。</summary>
    public List<string> DataNameX { set; get; } = new List<string>();

    /// <summary>NameY 成员。</summary>
    public string NameY { set; get; } 

    /// <summary>DataNameY 成员。</summary>
    public List<string> DataNameY { set; get; } = new List<string>();
    public int SplitX
    {
        set
        {
            splitX = value;
        }
        get { return splitX; }
    } 
    public int SplitY
    {
        set
        {
            splitY = value;
        }
        get { return splitY; }
    }
    /// <summary>OriginAddLength 成员。</summary>
    public float OriginAddLength { set; get; } = 2;
    /// <summary>GridAddLength 成员。</summary>
    public float GridAddLength { set; get; } = 2;
    /// <summary>ArrowSizeF 成员。</summary>
    public SizeF ArrowSizeF { set; get; } = new SizeF(10,15);
    /// <summary>GridRectangleF 成员。</summary>
    public RectangleF GridRectangleF { private set; get; } 

    public float GridX1
    {
        set
        {
            gridX1 = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
         
        }
        get
        {
            return gridX1;
        }
    }

    public float GridX2
    {
        set
        {
            gridX2 = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
        
        }
        get
        {
            return gridX2;
        }
    }

    public float GridY1
    {
        set
        {
            gridY1 = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
          
        }
        get
        {
            return gridY1;
        }
    }

    public float GridY2
    {
        set
        {
            gridY2 = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
           
        }
        get
        {
            return gridY2;
        }
    }

    public float GridA1
    {
        set
        {
            gridA1 = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
           
        }
        get
        {
            return gridA1;
        }
    }

    public float GridA2
    {
        set
        {
            gridA2 = value;
            GridRectangleF = new RectangleF(overallRectangleF.X + gridX1, overallRectangleF.Y + gridY1 + gridA1, overallRectangleF.Width - gridX1 - gridX2 - gridA2, overallRectangleF.Height - gridY1 - gridY2 - gridA1);
           
        }
        get
        {
            return gridA2;
        }
    }
}

public class CurvePoint
{
    /// <summary>名称。</summary>
    public string Name { set; get; }

    /// <summary>值。</summary>
    public object Value { set; get; }

    /// <summary>CurveColor 成员。</summary>
    public Color CurveColor { set; get; }

    /// <summary>CurvePointF 成员。</summary>
    public System.Drawing.PointF CurvePointF { set; get; }
}
}
