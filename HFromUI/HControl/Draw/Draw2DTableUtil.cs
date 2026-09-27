using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;


namespace HFromUI.HControl.Draw
{
    public class Draw2DTableUtil
    {
        /// <summary>isAssistGrid 字段。</summary>
        private bool isAssistGrid = true;

        /// <summary>isSplitGrid 字段。</summary>
        private bool isSplitGrid = true;
        /// <summary>错误描述。</summary>
        public string StrError { get; private set; }


        /// <summary>GraphicsColor 成员。</summary>
        public Color GraphicsColor { set; get; } = Color.WhiteSmoke;

        /// <summary>TitleNameFont 成员。</summary>
        public Font TitleNameFont { set; get; } = new Font("宋体", 1f * 30 * 10000 / 16875, FontStyle.Bold);
        /// <summary>TitleNameFontColor 成员。</summary>
        public Color TitleNameFontColor { set; get; } = Color.Black;
        /// <summary>AxisNameFont 成员。</summary>
        public Font AxisNameFont { set; get; } = new Font("黑体", 1f * 20 * 10000 / 16875, FontStyle.Regular);
        /// <summary>AxisNameFontColor 成员。</summary>
        public Color AxisNameFontColor { set; get; } = Color.Black;

        public bool IsSplitGrid
        {
            get
            {
                return isSplitGrid;
            }
            set
            {
                isSplitGrid = value;
            }
        }
        public bool IsAssistGrid
        {
            get
            {
                return isAssistGrid;
            }
            set
            {
                isAssistGrid = value;
            }
        }

        /// <summary>DrawStripData 成员。</summary>
        public DrawStripDataUtil DrawStripData { set; get; } = new DrawStripDataUtil();

        /// <summary>DrawStrip 成员。</summary>
        public DrawStripUtil DrawStrip { set; get; } = new DrawStripUtil();

        /// <summary>DrawStripUtilAssistGrid 成员。</summary>
        public DrawStripUtil DrawStripUtilAssistGrid { set; get; } = new DrawStripUtil();
        /// <summary>初始化。</summary>
        public bool Initialize(Graphics g)
        {
            try
            {
                g.Clear(GraphicsColor);

                if (isAssistGrid)
                {
                    if (DrawStripUtilAssistGrid.Pen == null)
                    {
                        DrawStripUtilAssistGrid.Pen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 1F);
                    }
                    DrawStripUtilAssistGrid.DrawStrip(g, DrawStripData.GridRectangleF, DrawStripData.SplitX * 10, DrawStripData.SplitY * 10, 0);
                }
                if (isSplitGrid)
                {
                    DrawStrip.DrawStrip(g, DrawStripData.GridRectangleF, DrawStripData.SplitX, DrawStripData.SplitY, DrawStripData.GridAddLength);
                }
                DrawStrip.DrawCoordinate(g, DrawStripData.GridRectangleF, DrawStripData.GridA1, DrawStripData.GridA2, DrawStripData.OriginAddLength, DrawStripData.ArrowSizeF.Width, DrawStripData.ArrowSizeF.Height);
                DrawStringUtil DrawStringName = new DrawStringUtil(DrawStripData.NameX);
                DrawStringName.Font = AxisNameFont;
                DrawStringName.PointF = new PointF(DrawStripData.GridRectangleF.X - DrawStripData.GridAddLength, DrawStripData.GridRectangleF.Y - DrawStripData.GridA1 / 3);
                DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                DrawStringName.PointOffset(9);
                DrawStringName.DrawString(g, AxisNameFontColor);

                DrawStringName.Content = DrawStripData.Name;
                DrawStringName.Font = TitleNameFont;
                DrawStringName.PointF = new PointF(DrawStripData.GridRectangleF.X + (DrawStripData.GridRectangleF.Width) / 2, DrawStripData.GridRectangleF.Y - DrawStripData.GridA1 / 2);
                DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                DrawStringName.PointOffset(5);
                if (DrawStripData.GridRectangleF.Y - DrawStripData.GridAddLength - DrawStringName.SizeF.Height - DrawStringName.PointF.Y <= 0)
                {
                    if (DrawStripData.GridAddLength == 0)
                    {
                        DrawStringName.PointF = new PointF(DrawStripData.GridRectangleF.X + (DrawStripData.GridRectangleF.Width) / 2, DrawStripData.GridRectangleF.Y - DrawStripData.OriginAddLength);
                    }
                    else
                    {
                        DrawStringName.PointF = new PointF(DrawStripData.GridRectangleF.X + (DrawStripData.GridRectangleF.Width) / 2, DrawStripData.GridRectangleF.Y - DrawStripData.GridAddLength);
                    }
                    DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                    DrawStringName.PointOffset(8);
                }
                DrawStringName.DrawString(g, TitleNameFontColor);
                DrawStringName.Content = DrawStripData.NameY;
                DrawStringName.Font = AxisNameFont;
                DrawStringName.PointF = new PointF(DrawStripData.GridRectangleF.X + DrawStripData.GridRectangleF.Width + DrawStripData.GridA2 / 8, DrawStripData.GridRectangleF.Y + DrawStripData.GridRectangleF.Height + DrawStripData.GridAddLength);
                DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                DrawStringName.PointOffset(1);
                DrawStringName.DrawString(g, AxisNameFontColor);
                bool IsDrawStripOrigin = DrawStripData.DataNameX.Count > 0 && DrawStripData.DataNameY.Count > 0 && DrawStripData.DataNameX[0] == DrawStripData.DataNameY[0];
                if (IsDrawStripOrigin)
                {
                    DrawStringName.Content = DrawStripData.DataNameX[0];
                    DrawStringName.Font = AxisNameFont;
                    DrawStringName.PointF = new PointF(DrawStripData.GridRectangleF.X - DrawStripData.OriginAddLength, DrawStripData.GridRectangleF.Y + DrawStripData.GridRectangleF.Height + DrawStripData.OriginAddLength);
                    DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                    DrawStringName.PointOffset(3);
                    DrawStringName.DrawString(g, AxisNameFontColor);
                }
                for (int numSplitP = 0; numSplitP <= DrawStripData.SplitX; numSplitP++)
                {
                    if (IsDrawStripOrigin && numSplitP != 0)
                    {
                        if (DrawStripData.DataNameX.Count > numSplitP)
                        {
                            DrawStringName.Content = DrawStripData.DataNameX[numSplitP];
                            DrawStringName.Font = AxisNameFont;
                            DrawStringName.PointF = new PointF(DrawStrip.GetPunctuation(DrawStripData.GridRectangleF, 1f * numSplitP / DrawStripData.SplitX, 1).X, DrawStrip.GetPunctuation(DrawStripData.GridRectangleF, 1f * numSplitP / DrawStripData.SplitX, 1).Y + DrawStripData.GridAddLength);
                            DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                            DrawStringName.PointOffset(2);
                            DrawStringName.DrawString(g, AxisNameFontColor);
                        }
                    }
                }
                for (int numSplitP = 0; numSplitP <= DrawStripData.SplitY; numSplitP++)
                {
                    if (IsDrawStripOrigin && numSplitP != 0)
                    {
                        if (DrawStripData.DataNameY.Count > numSplitP)
                        {
                            DrawStringName.Content = DrawStripData.DataNameY[numSplitP];
                            DrawStringName.Font = AxisNameFont;
                            DrawStringName.PointF = new PointF(DrawStrip.GetPunctuation(DrawStripData.GridRectangleF, 1f * numSplitP / DrawStripData.SplitY, -2).X - DrawStripData.GridAddLength, DrawStrip.GetPunctuation(DrawStripData.GridRectangleF, 1f * numSplitP / DrawStripData.SplitY, -2).Y);
                            DrawStringName.SizeF = DrawStringName.GetSizeF(g);
                            DrawStringName.PointOffset(6);
                            DrawStringName.DrawString(g, AxisNameFontColor);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return false;
        }

        /// <summary>DrawTableCurve 方法。</summary>
        public bool DrawTableCurve(Graphics g)
        {
            try
            {
                DrawCurveUtil DrawCurve = new DrawCurveUtil();
                if (DrawCurve.PointFList == null)
                {
                    DrawCurve.PointFList = new List<PointF>();
                }
                DrawCurve.PointFList.Clear();
                foreach (var item in DrawStripData.CurvePointList)
                {
                    DrawCurve.PointFList.Add(item.CurvePointF);
                }
                if (DrawCurve.DrawCurve(g))
                {
                    return true;
                }
                StrError = DrawCurve.StrError;
            }
            catch (Exception ex)
            {
                StrError = ex.Message;
            }
            return false;
        }

    }
}
