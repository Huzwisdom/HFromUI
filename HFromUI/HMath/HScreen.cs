using HFromUI.HData;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HMath
{
    public class HScreen
    {
        /// <summary>
     /// 根据控件客户区大小更新绘图区域（默认不留边距）
     /// </summary>
        public void FitToControl(int width, int height)
        {
            ScreenRectangle = new HRect(0, 0, width, height);
        }

        /// <summary>
        /// 自定义边距
        /// </summary>
        public void FitToControl(int width, int height, int marginX, int marginY)
        {
            ScreenRectangle = new HRect(marginX, marginY, width - 2 * marginX, height - 2 * marginY);
        }
        /// <summary>
        /// 测量字符串在指定字体下的渲染尺寸（以像素为单位）。
        /// </summary>
        /// <param name="text">待测量的字符串。</param>
        /// <param name="font">使用的字体，不可为 null。</param>
        /// <param name="width">输出宽度（像素）。</param>
        /// <param name="height">输出高度（像素）。</param>
        public static Size MeasureString(string text, Font font)
        {
            if (font == null) throw new ArgumentNullException(nameof(font));

           return TextRenderer.MeasureText(text ?? string.Empty, font);
        }
        /// <summary>
        /// 标签颜色
        /// </summary>
        public Pen TitlePen { get; set; } = new Pen(Color.FromArgb(200, 200, 200), 2f);
        /// <summary>框选画笔。</summary>
        public Pen SelectPen { get; set; } = new Pen(Color.FromArgb(200, 200, 200), 2f);
        /// <summary>
        /// 提示词颜色
        /// </summary>
        public Pen PromptShowPen { get; set; } = new Pen(Color.FromArgb(200, 200, 200), 2f);
        /// <summary>GridSmallPen 成员。</summary>
        public Pen GridSmallPen { set; get; } = new Pen(Color.FromArgb(120, 60, 70), 0.1f);
        /// <summary>GridBigPen 成员。</summary>
        public Pen GridBigPen { set; get; } = new Pen(Color.FromArgb(120, 60, 70), 1f);
        /// <summary>RulerSmallPen 成员。</summary>
        public Pen RulerSmallPen { set; get; } = new Pen(Color.FromArgb(60, 60, 70), 0.1f);
        /// <summary>RulerBigPen 成员。</summary>
        public Pen RulerBigPen { set; get; } = new Pen(Color.FromArgb(200, 60, 70), 2f);
        /// <summary>RulerFont 成员。</summary>
        public Font RulerFont { set; get; } 

        /// <summary>PromptShowFont 成员。</summary>
        public Font PromptShowFont { set; get; }

        /// <summary>PromptShowName 成员。</summary>
        public string PromptShowName { set; get; }
        /// <summary>RulerTextBrush 成员。</summary>
        public SolidBrush RulerTextBrush { set; get; } = new SolidBrush(Color.FromArgb(200, 200, 220));
        /// <summary>RulerLineHeight 成员。</summary>
        public HDouble RulerLineHeight { set; get; } = 10d;

        // 世界坐标原点在屏幕上的像素坐标（相对于屏幕左上角）
        private HPoint viewOffset = new HPoint();
        /// <summary>屏幕坐标点。</summary>
        private HPoint screenPoint = new HPoint();
        /// <summary>世界坐标点。</summary>
        private HPoint worldPoint = new HPoint();
        /// <summary>X 方向视图缩放。</summary>
        private HDouble viewScaleX = 50, viewScaleY=50;
        /// <summary>nowDecimalPlacesX 字段。</summary>
        private int nowDecimalPlacesX=5, nowDecimalPlacesY=5;

        /// <summary>DrawPoints 成员。</summary>
        public HList<HPoint> DrawPoints=new HList<HPoint>();
        /// <summary>AddDrawPoint 方法。</summary>
        public int AddDrawPoint(HPoint hPoint)
        {
            hPoint.X = hPoint.X.SetValue(GetDecimalPlaces(false, true));
            hPoint.Y = hPoint.Y.SetValue(GetDecimalPlaces(true, true));
            DrawPoints.Add(hPoint);
            return DrawPoints.Count;
        }
        /// <summary>添加一个绘制点：可选择取当前屏幕点或当前世界点（坐标按显示精度截断）。</summary>
        /// <param name="isScreen">true=取当前屏幕点；false（默认）=取当前世界点。</param>
        /// <returns>添加后的绘制点总数。</returns>
        public int AddDrawPoint(bool isScreen=false)
        {
            if (isScreen)
            {
                DrawPoints.Add(ScreenPoint.Clone());
            }
            else
            {
                HPoint hPoint = new HPoint();
                hPoint.X = WorldPoint.X.SetValue(GetDecimalPlaces(false,true));
                hPoint.Y = WorldPoint.Y.SetValue(GetDecimalPlaces(true, true));
                DrawPoints.Add(hPoint);
            }
            return DrawPoints.Count;
        }

        /// <summary>已采集绘制点的数量。</summary>
        public int DrawPointsCount
        {
            get
            { 
            return DrawPoints.Count;
            }
        }

        /// <summary>靠近吸附时使用的世界坐标点。</summary>
        public HPoint NearWorldPoint { set; get; }
        /// <summary>靠近吸附的判定范围（屏幕长度换算后的世界距离）。</summary>
        public HDouble NearWorldPointHeight { set; get; } = 6;
        /// <summary>靠近判定的时间窗口（毫秒）。</summary>
        public HDouble NearWorldPointTime { set; get; } = 1500;
        /// <summary>是否已设置吸附世界点。</summary>
        public bool IsSetNearWorld { set; get; } = false;

        /// <summary>框选区域的第一个屏幕点。</summary>
        public HPoint SelectScreenPoint1=null ;
        /// <summary>框选区域的第二个屏幕点。</summary>
        public HPoint SelectScreenPoint2 =null;
        /// <summary>鼠标当前是否处于按下状态。</summary>
        public HBool MouseDown = false;

        /// <summary>框选命中的绘制点集合。</summary>
        public HList<HPoint> SelectDrawPoints =new HList<HPoint>();

        /// <summary>标尺主刻度间距（X 方向，屏幕像素）。</summary>
        public HDouble RulerSpacingX { set; get; } = 5d;

        /// <summary>标尺主刻度间距（Y 方向，屏幕像素）。</summary>
        public HDouble RulerSpacingY { set; get; } = 5d;

        /// <summary>是否允许绘制坐标系（坐标轴、刻度、网格）。</summary>
        public HBool EnableDraw = true;
        /// <summary>框选时是否追加选择（true=追加，false=替换）。</summary>
        public HBool IsAddSelect { set; get; }
        /// <summary>获取框选区域的屏幕矩形；两点为空或重合时返回 null。</summary>
        public HRect GetSelectScreenRect()
        {
            if (SelectScreenPoint1==null|| SelectScreenPoint2==null)
            {
                return null;
            }
            if (SelectScreenPoint1.X.Value == SelectScreenPoint2.X.Value && SelectScreenPoint1.Y.Value == SelectScreenPoint2.Y.Value)
            {
                return null;
            }
            return new HRectangle((SelectScreenPoint1), (SelectScreenPoint2)).ToHRect();
        }
        /// <summary>获取框选区域映射到世界坐标的矩形；两点为空或重合时返回 null。</summary>
        public HRect GetSelectWorldRect()
        {
            if (SelectScreenPoint1 == null || SelectScreenPoint2 == null)
            {
                return null;
            }
            if (SelectScreenPoint1.X.Value== SelectScreenPoint2.X.Value&& SelectScreenPoint1.Y.Value== SelectScreenPoint2.Y.Value)
            {
                return null;
            }
            return new HRectangle(ScreenToWorld(SelectScreenPoint1), ScreenToWorld(SelectScreenPoint2)).ToHRect();
        }

        public HDouble ViewScaleX
        {
            get
            { 
                return viewScaleX; 
            }
            set
            {
                viewScaleX = value;
                nowDecimalPlacesX = (1 / ViewScaleX.ToNextPowerOfTen()).GetDecimalPlaces(DecimalPlacesMax);

            }
        }
        public HDouble ViewScaleY
        {
            get
            { 
                return viewScaleY;
            }
            set
            {
                viewScaleY = value;
                nowDecimalPlacesY = (1 / ViewScaleY.ToNextPowerOfTen()).GetDecimalPlaces(DecimalPlacesMax);

            }
        }
        /// <summary>TitleName 成员。</summary>
        public string TitleName { set; get; }
        /// <summary>NameX 成员。</summary>
        public string NameX { set; get; }
        /// <summary>NameY 成员。</summary>
        public string NameY { set; get; }
        /// <summary>UnitX 成员。</summary>
        public string UnitX { set; get; }
        /// <summary>UnitY 成员。</summary>
        public string UnitY { set; get; }
        /// <summary>IsRefresh 成员。</summary>
        public bool IsRefresh{ set; get; }
        /// <summary>RefreshHz 成员。</summary>
        public virtual int RefreshHz { set; get; } = 100;
        /// <summary>SetDecimalPlaces 成员。</summary>
        public HInt SetDecimalPlaces { set; get; } = 5;
        /// <summary>DecimalPlacesMax 成员。</summary>
        public int DecimalPlacesMax { set; get; } = 5;
        public int NowDecimalPlacesX
        {
            get
            {
                return nowDecimalPlacesX;
            }
        }
        public int NowDecimalPlacesY
        {
            get
            {
                return nowDecimalPlacesY;
            }
        }
        /// <summary>绘图时坐标的显示精度（小数位数）。</summary>
        public HDouble DrawDecimalNumber { set; get; }
        /// <summary>按当前缩放级别动态计算坐标显示的小数位数。</summary>
        /// <param name="isYAxis">true=按 Y 轴计算；false=按 X 轴计算。</param>
        /// <param name="isadd">true=取设置精度与动态精度的较小值（限制上限）。</param>
        /// <returns>应使用的小数位数。</returns>
        public int GetDecimalPlaces(bool isYAxis, bool isadd = false)
        {
            if (isYAxis)
            {
                if (isadd)
                {
                    if (SetDecimalPlaces< NowDecimalPlacesY)
                    {
                        return SetDecimalPlaces.Value;
                    }
                }
                return NowDecimalPlacesY;
            }
            else
            {
                if (isadd)
                {
                    if (SetDecimalPlaces < NowDecimalPlacesX)
                    {
                        return SetDecimalPlaces.Value;
                    }
                }
                return NowDecimalPlacesX;
            }
         
        }
        /// <summary>视图最小缩放。</summary>
        public virtual HDouble ViewScaleMin { set; get; } = 5;
        /// <summary>视图最大缩放。</summary>
        public virtual HDouble ViewScaleMax { set; get; } = 10000;
        public HDouble ViewScale
        {
            get
            {
                if (ViewScaleX == ViewScaleY)
                {
                    return ViewScaleX;
                }
                return (ViewScaleX + ViewScaleY) / 2;
            }
            set
            {
                ViewScaleX = ViewScaleY = value;
            }
        }
        /// <summary>屏幕矩形区域。</summary>
        public HRect ScreenRectangle { set; get; }
        /// <summary>
        /// 获取或设置视图偏移（世界原点在屏幕上的像素坐标，相对于屏幕左上角）
        /// </summary>
        public HPoint ViewOffset
        {
            get
            {

                return viewOffset; 
            }
            set 
            {
                if (value == null)
                {
                    viewOffset= new HPoint();
                }
                viewOffset = value;
            }
        }

        public HPoint ScreenPoint
        {
            get => screenPoint;
            set
            {
                if (value== screenPoint)
                {
                    return;
                }
                screenPoint = value;
                double rx = screenPoint.X.Value - ScreenRectangle.X.Value;
                double ry = screenPoint.Y.Value - ScreenRectangle.Y.Value;
                worldPoint.X = ((rx - ViewOffset.X.Value) / ViewScaleX).SetValue(GetDecimalPlaces(false));
                worldPoint.Y = ((ScreenRectangle.Height - ry - ViewOffset.Y.Value) / ViewScaleY).SetValue(GetDecimalPlaces(true));
            }
        }
        /// <summary>
        /// 获取网格最接近的4个点
        /// </summary>
        /// <returns></returns>
        public HPoint[] GetNearWorldPoint()
        { 
            HList<HPoint> hWPoints = new HList<HPoint>();
            HDouble x = WorldPoint.X.TruncateByUnit(DrawDecimalNumber.Value);
            HDouble y = WorldPoint.Y.TruncateByUnit(DrawDecimalNumber.Value);
            hWPoints.Add(new HPoint(x,y));
            hWPoints.Add(new HPoint(x+ DrawDecimalNumber, y));
            hWPoints.Add(new HPoint(x , y + DrawDecimalNumber));
            hWPoints.Add(new HPoint(x + DrawDecimalNumber, y + DrawDecimalNumber));
            return hWPoints.ToArray();
        }
        /// <summary>
        /// 获取网格最接近的4个点
        /// </summary>
        /// <returns></returns>
        public HPoint3D[] GetNearWorldPoint3D()
        {
            HList<HPoint3D> hWPoints = new HList<HPoint3D>();
            HDouble x = WorldPoint.X.TruncateByUnit(DrawDecimalNumber.Value);
            HDouble y = WorldPoint.Y.TruncateByUnit(DrawDecimalNumber.Value);
            hWPoints.Add(new HPoint3D(x.Value, y.Value,0));
            hWPoints.Add(new HPoint3D((x + DrawDecimalNumber).Value, y.Value,0));
            hWPoints.Add(new HPoint3D(x.Value, (y + DrawDecimalNumber).Value, 0));
            hWPoints.Add(new HPoint3D((x + DrawDecimalNumber).Value, (y + DrawDecimalNumber).Value, 0));
            return hWPoints.ToArray();
        }
        public HPoint WorldPoint
        {
            get
            {
                return worldPoint;
            }
            set
            {
                worldPoint = value;
                double sx = worldPoint.X.Value * ViewScaleX.Value + ViewOffset.X.Value;
                double sy = ScreenRectangle.Height.Value - (worldPoint.Y.Value * ViewScaleY.Value + ViewOffset.Y.Value);
                screenPoint.X = sx + ScreenRectangle.X.Value;
                screenPoint.Y = sy + ScreenRectangle.Y.Value;
            }
        }
        /// <summary>直接设置当前世界坐标点（不联动更新屏幕坐标）。</summary>
        /// <param name="hPoint">世界坐标点。</param>
        public void SetWorldPoint(HPoint hPoint)
        {
            worldPoint=hPoint;
        }
        /// <summary>直接设置当前屏幕坐标点（不联动更新世界坐标）。</summary>
        /// <param name="hPoint">屏幕坐标点。</param>
        public void SetScreenPoint(HPoint hPoint)
        {
            screenPoint = hPoint;
        }
        /// <summary>
        /// 屏幕坐标 -> 世界坐标。
        /// 屏幕坐标是相对于整个控件(0,0)的，转换时先平移到 ScreenRectangle 内部。
        /// </summary>
        public HPoint ScreenToWorld(HPoint screenPt)
        {
            // 减去矩形左上角偏移，得到相对于绘图区域左上角的坐标
            double rx = screenPt.X.Value - ScreenRectangle.X.Value;
            double ry = screenPt.Y.Value - ScreenRectangle.Y.Value;

            return new HPoint(
                ((rx - ViewOffset.X.Value) / ViewScaleX).SetValue(GetDecimalPlaces(false)),
                ((ScreenRectangle.Height - ry - ViewOffset.Y.Value) / ViewScaleY).SetValue(GetDecimalPlaces(true))
            );
        }

        /// <summary>
        /// 世界坐标 -> 屏幕坐标
        /// </summary>
        public HPoint WorldToScreen(HPoint worldPt)
        {
            // 先计算在绘图区域内的相对坐标，再加上区域偏移
            double sx = worldPt.X.Value * ViewScaleX.Value + ViewOffset.X.Value;
            double sy = ScreenRectangle.Height.Value - (worldPt.Y.Value * ViewScaleY.Value + ViewOffset.Y.Value);

            return new HPoint(
                sx + ScreenRectangle.X.Value,
                sy + ScreenRectangle.Y.Value
            );
        }
      
        /// <summary>
        /// 将屏幕长度（像素）转换为世界坐标长度。指定是否Y轴，否则为X轴。
        /// </summary>
        public double ScreenLengthToWorld(double screenLength, bool isYAxis)
        {
            double scale = isYAxis ? ViewScaleY.Value : ViewScaleX.Value;
            return screenLength / scale;
        }
        
        /// <summary>
        /// 将世界坐标长度转换为屏幕长度（像素）。指定是否Y轴，否则为X轴。
        /// </summary>
        public double WorldLengthToScreen(double worldLength, bool isYAxis)
        {
            double scale = isYAxis ? ViewScaleY.Value : ViewScaleX.Value;
            return worldLength * scale;
        }

        /// <summary>
        /// 获取鼠标当前的全局屏幕坐标。
        /// </summary>
        public static Point GetMouseWindowsScreenPosition()
        {
            return Cursor.Position;
        }

        /// <summary>
        /// 设置鼠标全局屏幕坐标（移动鼠标光标到指定位置）。
        /// </summary>
        public static void SetMouseWindowsScreenPosition(Point screenPos)
        {
            Cursor.Position = screenPos;
        }

        /// <summary>
        /// 将控件客户区坐标转换为全局屏幕坐标。
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="clientPoint">控件客户区内的坐标</param>
        /// <returns>对应的全局屏幕坐标</returns>
        public static Point ControlToWindowsScreen(Control control, Point clientPoint)
        {
            if (control == null) return Point.Empty;
            return control.PointToScreen(clientPoint);
        }

        /// <summary>
        /// 将全局屏幕坐标转换为控件客户区坐标。
        /// </summary>
        /// <param name="control">目标控件</param>
        /// <param name="screenPoint">全局屏幕坐标</param>
        /// <returns>对应的控件客户区坐标</returns>
        public static Point WindowsScreenToControl(Control control, Point screenPoint)
        {
            if (control == null) return Point.Empty;
            return control.PointToClient(screenPoint);
        }

        public HRect WorldRectangle
        {
            get
            {
                HPoint hPoint1 = ScreenToWorld(new HPoint(ScreenRectangle.X.Value, ScreenRectangle.Y.Value));
                HPoint hPoint2 = ScreenToWorld(new HPoint(ScreenRectangle.X.Value + ScreenRectangle.Width, ScreenRectangle.Y.Value + ScreenRectangle.Height));
                HRectangle rect = new HRectangle(hPoint1, hPoint2);
                return rect.ToHRect();
            }
        }

        /// <summary>在绘图区右上角绘制提示文字（PromptShowName）。</summary>
        /// <param name="g">绘图对象。</param>
        public void DrawPromptShowNameA(Graphics g)
        {
            if (string.IsNullOrWhiteSpace(PromptShowName))
            {
                return;
            }
            if (ScreenPoint.X.Value > ScreenRectangle.X.Value && ScreenPoint.X.Value < ScreenRectangle.Width+ ScreenRectangle.X.Value && ScreenPoint.Y.Value > ScreenRectangle.Y.Value && ScreenPoint.Y.Value < ScreenRectangle.Height+ ScreenRectangle.Y.Value)
            {
                SizeF textSize = g.MeasureString(PromptShowName, PromptShowFont);
                g.DrawString(PromptShowName, PromptShowFont, RulerTextBrush, (ScreenRectangle.X.Value+ ScreenRectangle.Width - textSize.Width - 20).ToSingle(), textSize.Height + 5);
            }
        }

        /// <summary>绘制完整坐标系：边框、坐标轴、刻度线、标尺文字与网格。</summary>
        /// <param name="g">绘图对象。</param>
        public void DrawCoordinateA(Graphics g)
        {
        
            bool isDrawCoordinate = false;

      
             if (ViewScale > 4000)
            {
            DrawCoordinate:
                bool isshow = false;
                HRect worldRectangleF = WorldRectangle;
                HDouble startFloat = Math.Floor(worldRectangleF.X.Value);
                for (HDouble i = startFloat; i < worldRectangleF.X.Value + worldRectangleF.Width; i = i + 0.01)
                {
                    DrawDecimalNumber = 0.01;
                       HPoint start1 = new HPoint(i, worldRectangleF.Y.Value + worldRectangleF.Height);
                    HPoint end1 = new HPoint(i, worldRectangleF.Y.Value);
                    HPoint start2 = WorldToScreen(start1);
                    HPoint end2 = WorldToScreen(end1);

                    string label = i.Value.ToString("f2");
                    SizeF textSize = g.MeasureString(label, RulerFont);
                    if (Math.Abs((i.Value * 100) % 100) > 1e-9)
                    {
                        GridSmallPen.DashStyle = DashStyle.Dash;
                        if (!isDrawCoordinate)
                        {
                            if (textSize.Width + RulerSpacingX < WorldLengthToScreen(0.1, false))
                            {
                                isshow = true;

                                isDrawCoordinate = true;
                                goto DrawCoordinate;
                            }

                        }
                        else
                        {
                            isshow = true;
                        }
                        if (isshow)
                        {
                            g.DrawLine(GridSmallPen, start2, end2);
                            g.DrawLine(RulerSmallPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight / 2), end2);
                            g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight / 2 - textSize.Height - 2).ToSingle());

                        }

                    }

                }
                startFloat = Math.Floor(worldRectangleF.Y.Value);
                for (HDouble i = startFloat; i < worldRectangleF.Y.Value + worldRectangleF.Height; i = i + 0.01)
                {
                    HPoint start1 = new HPoint(worldRectangleF.X.Value, i);
                    HPoint end1 = new HPoint(worldRectangleF.X.Value + worldRectangleF.Width, i);
                    HPoint start2 = WorldToScreen(start1);
                    HPoint end2 = WorldToScreen(end1);
                    string label = i.Value.ToString("f2");
                    SizeF textSize = g.MeasureString(label, RulerFont);
                    if (Math.Abs((i.Value * 100) % 100) > 1e-9)
                    {
                        if (isshow)
                        {
                            g.DrawLine(GridSmallPen, start2, end2);
                            g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight / 2, start2.Y.Value));
                            g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight / 2 + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));

                        }

                    }
                }
            }
            else if (ViewScale > 400)
            {
            DrawCoordinate:
                bool isshow = false;
                HRect worldRectangleF = WorldRectangle;
                HDouble startFloat = Math.Floor(worldRectangleF.X.Value);
                for (HDouble i = startFloat; i < worldRectangleF.X.Value + worldRectangleF.Width; i = i + 0.1)
                {
                    DrawDecimalNumber = 0.1;
                    HPoint start1 = new HPoint(i, worldRectangleF.Y.Value + worldRectangleF.Height);
                    HPoint end1 = new HPoint(i, worldRectangleF.Y.Value);
                    HPoint start2 = WorldToScreen(start1);
                    HPoint end2 = WorldToScreen(end1);

                    string label = i.Value.ToString("f1");
                    SizeF textSize = g.MeasureString(label, RulerFont);
                    if (Math.Abs((i.Value * 10) % 10) > 1e-9)
                    {
                        GridSmallPen.DashStyle = DashStyle.Dash;
                        if (!isDrawCoordinate)
                        {
                            if (textSize.Width + RulerSpacingX < WorldLengthToScreen(0.1, false))
                            {
                                isshow = true;

                                isDrawCoordinate = true;
                                goto DrawCoordinate;
                            }

                        }
                        else
                        {
                            isshow = true;
                        }
                        if (isshow)
                        {
                            g.DrawLine(GridSmallPen, start2, end2);
                            g.DrawLine(RulerSmallPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight / 2), end2);
                            g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight / 2 - textSize.Height - 2).ToSingle());

                        }

                    }

                }
                startFloat = Math.Floor(worldRectangleF.Y.Value);
                for (HDouble i = startFloat; i < worldRectangleF.Y.Value + worldRectangleF.Height; i = i + 0.1)
                {
                    HPoint start1 = new HPoint(worldRectangleF.X.Value, i);
                    HPoint end1 = new HPoint(worldRectangleF.X.Value + worldRectangleF.Width, i);
                    HPoint start2 = WorldToScreen(start1);
                    HPoint end2 = WorldToScreen(end1);
                    string label = i.Value.ToString("f1");
                    SizeF textSize = g.MeasureString(label, RulerFont);
                    if (Math.Abs((i.Value * 10) % 10) > 1e-9)
                    {
                        if (isshow)
                        {
                            g.DrawLine(GridSmallPen, start2, end2);
                            g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight / 2, start2.Y.Value));
                            g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight / 2 + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));

                        }
                    }
                }
            }
            else
            {
            DrawCoordinate:
                HRect worldRectangleF = WorldRectangle;
                HDouble startFloat = Math.Floor(worldRectangleF.X.Value);
                bool isshow = false;
                for (HDouble i = startFloat; i < worldRectangleF.X.Value + worldRectangleF.Width.Value; i = i + 1)
                {
                    HPoint start1 = new HPoint(i, worldRectangleF.Y.Value + worldRectangleF.Height);
                    HPoint end1 = new HPoint(i, worldRectangleF.Y.Value);
                    HPoint start2 = WorldToScreen(start1);
                    HPoint end2 = WorldToScreen(end1);
                 
                    string label = i.Value.ToString("0");
                    SizeF textSize = g.MeasureString(label, RulerFont);
                    if (Convert.ToInt32(i.Value) % 10 == 0)
                    {
                        g.DrawLine(GridBigPen, start2, end2);

                        if (textSize.Width + RulerSpacingX < WorldLengthToScreen(10, false))
                        {
                            g.DrawLine(RulerBigPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight), end2);
                            g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight - textSize.Height - 2).ToSingle());
                        }
                        else if (textSize.Width + RulerSpacingX < WorldLengthToScreen(100, false))
                        {
                            if (Convert.ToInt32(i.Value) % 100 == 0)
                            {
                                g.DrawLine(RulerBigPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight), end2);
                                g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight - textSize.Height - 2).ToSingle());
                            }
                        }
                        else if (textSize.Width + RulerSpacingX < WorldLengthToScreen(1000, false))
                        {
                            if (Convert.ToInt32(i.Value) % 1000 == 0)
                            {
                            
                                g.DrawLine(RulerBigPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight), end2);
                                g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight - textSize.Height - 2).ToSingle());
                            }
                        }
                        else
                        {
                            g.DrawLine(RulerBigPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight), end2);
                            g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight - textSize.Height - 2).ToSingle());
                        }
                    }
                    else
                    {
                       
                        GridSmallPen.DashStyle = DashStyle.Dash;
                        if (!isDrawCoordinate)
                        {
                            if (textSize.Width + RulerSpacingX < WorldLengthToScreen(1, false))
                            {
                                isshow = true;

                                isDrawCoordinate = true;
                                goto DrawCoordinate;
                            }

                        }
                        else
                        {
                            isshow = true;
                        }
                        if (isshow)
                        {
                            DrawDecimalNumber = 1;
                            g.DrawLine(GridSmallPen, start2, end2);
                            g.DrawLine(RulerSmallPen, new HPoint(end2.X.Value, end2.Y.Value - RulerLineHeight / 2), end2);
                            g.DrawString(label, RulerFont, RulerTextBrush, (float)(end2.X.Value - textSize.Width / 2), (end2.Y.Value - RulerLineHeight / 2 - textSize.Height - 2).ToSingle());

                        }
                        else
                        {
                            DrawDecimalNumber = 10;
                        }
                    }

                }
                startFloat = Math.Floor(worldRectangleF.Y.Value);
                for (HDouble i = startFloat; i < worldRectangleF.Y.Value + worldRectangleF.Height.Value; i = i + 1)
                {
                    HPoint start1 = new HPoint(worldRectangleF.X.Value, i);
                    HPoint end1 = new HPoint(worldRectangleF.X.Value + worldRectangleF.Width, i);
                    HPoint start2 = WorldToScreen(start1);
                    HPoint end2 = WorldToScreen(end1);
                    string label = (i).Value.ToString("0");
                    SizeF textSize = g.MeasureString(label, RulerFont);
                    if (Convert.ToInt32(i.Value) % 10 == 0)
                    {
                        g.DrawLine(GridBigPen, start2, end2);
                        if (i != startFloat)
                        {
                            if (textSize.Height + RulerSpacingY < WorldLengthToScreen(10, true))
                            {
                                g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight, start2.Y.Value));
                                g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));
                            }
                            else if (textSize.Height + RulerSpacingY < WorldLengthToScreen(100, true))
                            {
                                if (Convert.ToInt32(i.Value) % 100 == 0)
                                {
                                    g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight, start2.Y.Value));
                                    g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));
                                }
                            }
                            else if (textSize.Height + RulerSpacingY < WorldLengthToScreen(1000, true))
                            {
                                if (Convert.ToInt32(i.Value) % 1000 == 0)
                                {
                                    g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight, start2.Y.Value));
                                    g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));
                                }
                            }
                            else
                            {
                                g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight, start2.Y.Value));
                                g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));
                            }

                        }
                    }
                    else
                    {
                        GridSmallPen.DashStyle = DashStyle.Dash;

                        if (isshow)
                        {
                            g.DrawLine(GridSmallPen, start2, end2);
                            g.DrawLine(RulerBigPen, start2, new HPoint(start2.X.Value + RulerLineHeight / 2, start2.Y.Value));
                            g.DrawString(label, RulerFont, RulerTextBrush, (start2.X.Value + RulerLineHeight / 2 + 2).ToSingle(), (float)(start2.Y.Value - textSize.Height / 2));

                        }
                    }
                }
            }

       
            g.DrawLine(RulerBigPen, new HPoint(ScreenRectangle.X.Value, ScreenRectangle.Height+ ScreenRectangle.Y.Value - RulerBigPen.Width / 2), new HPoint(ScreenRectangle.X.Value+ ScreenRectangle.Width, ScreenRectangle.Y.Value+ ScreenRectangle.Height - RulerBigPen.Width / 2));
            g.DrawLine(RulerBigPen, new HPoint(ScreenRectangle.X.Value, ScreenRectangle.Y.Value), new HPoint(ScreenRectangle.X.Value, ScreenRectangle.Y.Value+ ScreenRectangle.Height));

           
        }

        /// <summary>调整视图偏移，使指定世界点居中显示在视口中。</summary>
        /// <param name="x1">中心点世界 X 坐标。</param>
        /// <param name="y1">中心点世界 Y 坐标。</param>
        public virtual void ViewOffsetCenter(HDouble x1, HDouble y1)
        {
            HRect worldRectangle = WorldRectangle;
            ViewOffset = WorldToScreen(new HPoint(worldRectangle.X.Value + x1 * -1 + worldRectangle.Width / 2, (worldRectangle.Y.Value + worldRectangle.Height) + y1 - worldRectangle.Height / 2));
        }

        /// <summary>
        /// 调整视图（缩放和偏移），使指定的世界矩形在绘图区域内尽可能大地完整显示。
        /// 缩放比例会被限制在 ViewScaleMin 和 ViewScaleMax 之间。
        /// </summary>
        /// <param name="worldRect">要显示的世界坐标矩形（HRectangle 会自动规范方向）</param>
        /// <param name="maintainAspectRatio">是否保持 X/Y 缩放比例一致（默认 true）</param>
        public void FitWorldRectangle(HRectangle worldRect, bool maintainAspectRatio = true)
        {
            if (worldRect == null || ScreenRectangle == null || worldRect.Width <= 0 || worldRect.Height <= 0)
                return;

            // 1. 世界矩形的宽高
            double worldWidth = worldRect.Width;
            double worldHeight = worldRect.Height;

            // 2. 绘图区域像素尺寸
            double screenWidth = ScreenRectangle.Width.Value;
            double screenHeight = ScreenRectangle.Height.Value;

            // 3. 计算理想缩放比例
            double scaleX = screenWidth / worldWidth;
            double scaleY = screenHeight / worldHeight;

            // 4. 根据是否保持纵横比，选择缩放值并应用范围限制
            if (maintainAspectRatio)
            {
                // 取较小缩放，确保矩形完整可见
                double scale = Math.Min(scaleX, scaleY);
                // 限制在 [ViewScaleMin, ViewScaleMax] 范围内
                scale = Math.Max(ViewScaleMin.Value, Math.Min(ViewScaleMax.Value, scale));
                ViewScaleX = scale;
                ViewScaleY = scale;
            }
            else
            {
                // 分别限制 X、Y 方向的缩放
                double limitedScaleX = Math.Max(ViewScaleMin.Value, Math.Min(ViewScaleMax.Value, scaleX));
                double limitedScaleY = Math.Max(ViewScaleMin.Value, Math.Min(ViewScaleMax.Value, scaleY));
                ViewScaleX = limitedScaleX;
                ViewScaleY = limitedScaleY;
            }

            // 5. 计算世界矩形中心点（HRectangle.X.Value/Y 已是规范化后的左下角）
            double centerWorldX = worldRect.X.Value + worldWidth / 2.0;
            double centerWorldY = worldRect.Y.Value + worldHeight / 2.0;

            // 6. 设置偏移，使矩形中心与绘图区域中心对齐
            double offsetX = screenWidth / 2.0 - centerWorldX * ViewScaleX.Value;
            double offsetY = screenHeight / 2.0 - centerWorldY * ViewScaleY.Value;
            ViewOffset = new HPoint(offsetX, offsetY);
        }
        /// <summary>
        /// 世界坐标下的拖动边界矩形（null 表示无限制）。
        /// </summary>
        public HRectangle ViewBounds { get; set; }

        /// <summary>
        /// 将 ViewOffset 限制在 ViewBounds 内。
        /// - 视口小于边界：视口不能移出边界。
        /// - 视口大于边界：边界必须完全可见（即视口包含边界）。
        /// - 绝不强制居中。
        /// </summary>
        public void ClampViewOffsetToBounds()
        {
            if (ViewBounds == null || ScreenRectangle == null)
                return;

            var b = ViewBounds;
            double viewW = ScreenRectangle.Width.Value / ViewScaleX.Value;
            double viewH = ScreenRectangle.Height.Value / ViewScaleY.Value;

            double ox = ViewOffset.X.Value;
            double oy = ViewOffset.Y.Value;

            // ---------- X 方向 ----------
            if (viewW <= b.Width)
            {
                // 视口比边界窄 → 视口必须在边界内
                double minX = (ScreenRectangle.Width - b.Right * ViewScaleX).Value;   // 视口右边界 = 边界右
                double maxX = (-b.Left * ViewScaleX).Value;                          // 视口左边界 = 边界左
                ox = Math.Max(minX, Math.Min(maxX, ox));
            }
            else
            {
                // 视口比边界宽 → 边界必须完全在视口内
                double minX = (ScreenRectangle.Width - b.Right * ViewScaleX).Value;   // 视口右边界 = 边界右（边界不能超出视口右边）
                double maxX = (-b.Left * ViewScaleX).Value;                          // 视口左边界 = 边界左（边界不能超出视口左边）
                                                                             // 此时 maxX < minX，需限制 ox 在 [maxX, minX] 之间
                ox = Math.Max(maxX, Math.Min(minX, ox));
            }

            // ---------- Y 方向（世界 Y 向上）----------
            if (viewH <= b.Height)
            {
                // 视口比边界矮 → 视口必须在边界内
                double minY = (ScreenRectangle.Height - b.Top * ViewScaleY).Value;    // 视口上边界 = 边界上
                double maxY = (-b.Bottom * ViewScaleY).Value;                         // 视口下边界 = 边界下
                oy = Math.Max(minY, Math.Min(maxY, oy));
            }
            else
            {
                // 视口比边界高 → 边界必须完全在视口内
                double minY = (ScreenRectangle.Height - b.Top * ViewScaleY).Value;
                double maxY = (-b.Bottom * ViewScaleY).Value;
                oy = Math.Max(maxY, Math.Min(minY, oy));
            }

            ViewOffset = new HPoint(ox, oy);
        }




    }




}
