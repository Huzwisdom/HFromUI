using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
namespace HFromUI.HControl.Draw
{
    using HFromUI.HLangage;
    public class DrawImageUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }

    /// <summary>RectangleF 成员。</summary>
    public System.Drawing.RectangleF RectangleF { set; get; }

    /// <summary>strPath 成员。</summary>
    public string strPath { set; get; }

    /// <summary>关联的图片对象。</summary>
    public System.Drawing.Image Image { set; get; }

    public DrawImageUtil()
    {

    }
    public DrawImageUtil(string strpath, System.Drawing.RectangleF rectangleF)
    {
        strPath = strpath;
        RectangleF = rectangleF;
        GetImage();
    }
    public DrawImageUtil(Image image, System.Drawing.RectangleF rectangleF)
    {
        RectangleF = rectangleF;
        Image = image;
    }
    /// <summary>获取 image。</summary>
    public bool GetImage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(strPath))
            {
                StrError = HTranslation.GetContent("请确认图片地址是否正确（为空） Please make sure that the image address is correct (empty)");
                return false;
            }
            if (strPath.Contains(@"\"))
            {
                Image = System.Drawing.Image.FromFile(strPath);
            }
            else
            {
                Image = System.Drawing.Image.FromFile(HFromUI.HData.HAppData.AppPath + @"\" + strPath);
            }
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
        }
        return false;
    }
    /// <summary>绘制图片。</summary>
    public virtual bool DrawImage(System.Drawing.Graphics graphics)
    {
        try
        {
            graphics.DrawImage(Image, RectangleF);
            return true;
        }
        catch (Exception ex)
        {
            StrError = ex.Message;
            return false;
        }
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
}
}
