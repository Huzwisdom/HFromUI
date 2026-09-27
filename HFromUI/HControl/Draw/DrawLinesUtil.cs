using System;
using System.Collections.Generic;
using System.Drawing;
namespace HFromUI.HControl.Draw
{
    /// <summary>
    /// 绘制平面多条两点的连线
    /// </summary>
    public class DrawLinesUtil
{
    /// <summary>错误描述。</summary>
    public string StrError { get; private set; }

    /// <summary>linesUtilList 成员。</summary>
    public List<DrawLineUtil> linesUtilList = new List<DrawLineUtil>();

    /// <summary>DrawLines 方法。</summary>
    public virtual bool DrawLines(System.Drawing.Graphics graphics)
    {
        try
        {
            if (linesUtilList == null || linesUtilList.Count == 0)
            {
                return true;
            }
            foreach (DrawLineUtil linesUtil in linesUtilList)
            {
                linesUtil.DrawLine(graphics);
            }
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
