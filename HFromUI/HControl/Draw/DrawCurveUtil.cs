using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace HFromUI.HControl.Draw
{
    using HFromUI.HLangage;
    /// <summary>
    /// 绘制一段曲线
    /// </summary>
    public class DrawCurveUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }
    /// <summary>画笔。</summary>
    public System.Drawing.Pen Pen { set; get; } 
    /// <summary>PointFList 成员。</summary>
    public List<System.Drawing.PointF> PointFList { set; get; } = new List<System.Drawing.PointF>();

    public DrawCurveUtil()
    {

    }

    /// <summary>AddPoint 方法。</summary>
    public void AddPoint(System.Drawing.PointF pointF)
    {
        PointFList.Add(pointF);
    }


    /// <summary>AddPoints 方法。</summary>
    public void AddPoints(System.Drawing.PointF[] pointFs)
    {
        PointFList.AddRange(pointFs);
    }

    /// <summary>AddPoint 方法。</summary>
    public void AddPoint(float x, float y)
    {
        System.Drawing.PointF pointF = new System.Drawing.PointF(x, y);
        PointFList.Add(pointF);
    }
    /// <summary>AddPoint 方法。</summary>
    public void AddPoint(int x, int y)
    {
        System.Drawing.PointF pointF = new System.Drawing.PointF(x, y);
        PointFList.Add(pointF);
    }
    /// <summary>清空。</summary>
    public void Clear()
    {
        PointFList.Clear();
    }

    /// <summary>DrawCurve 方法。</summary>
    public virtual bool DrawCurve(System.Drawing.Graphics graphics)
    {
        if (PointFList.Count <= 0)
        {
            StrError = HTranslation.GetContent("无点位信息 No point information");
            return false;
        }
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawCurve(Pen, PointFList.ToArray());
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>DrawCurve 方法。</summary>
    public virtual bool DrawCurve(System.Drawing.Graphics graphics, float tension)
    {
        if (PointFList.Count <= 0)
        {
            StrError = HTranslation.GetContent("无点位信息 No point information");
            return false;
        }
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawCurve(Pen, PointFList.ToArray(), tension);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>DrawCurve 方法。</summary>
    public virtual bool DrawCurve(System.Drawing.Graphics graphics, int offset, int numberOfSegments)
    {
        if (PointFList.Count <= 0)
        {
            StrError = HTranslation.GetContent("无点位信息 No point information");
            return false;
        }
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawCurve(Pen, PointFList.ToArray(), offset, numberOfSegments);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
    }

    /// <summary>DrawCurve 方法。</summary>
    public virtual bool DrawCurve(System.Drawing.Graphics graphics, int offset, int numberOfSegments, float tension)
    {
        if (PointFList.Count <= 0)
        {
            StrError = HTranslation.GetContent("无点位信息 No point information");
            return false;
        }
        try
        {
            if (Pen == null)
            {
                Pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 1F);
            }
            graphics.DrawCurve(Pen, PointFList.ToArray(), offset, numberOfSegments, tension);
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
        graphicsPath.AddCurve(PointFList.ToArray());
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
