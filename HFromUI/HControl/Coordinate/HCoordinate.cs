using HFromUI;
using HFromUI.HAttribute;
using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HEnum;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HControl.Coordinate
{
    using HFromUI.HColor;
    using HFromUI.HData.Win;
    using HFromUI.HLangage;
    public class HCoordinate : Panel
    {

        /// <summary>关联的坐标系屏幕。</summary>
        public HScreen CoordinateScreen = new HScreen();
        /// <summary>关联的绘图列表。</summary>
        public HDrawList CoordinateDrawList = new HDrawList();
        /// <summary>ShowStringBuilder 字段。</summary>
        private StringBuilder ShowStringBuilder  = new StringBuilder();
        /// <summary>ShowString 字段。</summary>
        private string ShowString = string.Empty;
        /// <summary>ShowStringPoint 字段。</summary>
        private HPoint ShowStringPoint { set; get; }
        /// <summary>ShowStringFont 成员。</summary>
        public Font ShowStringFont { set; get; }
        /// <summary>ShowStringPen 成员。</summary>
        public Pen ShowStringPen { get; set; } = new Pen(Color.FromArgb(200, 200, 200), 2f);

        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_CHAR = 0x0102;

        public event KeyEventHandler CoordinateKeyDown;
        public event KeyEventHandler CoordinateKeyUp;
        public event KeyPressEventHandler CoordinateKeyPress;

        public Timer timer;
        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
        }
        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            bool handled = false;
            switch (m.Msg)
            {
                case WM_KEYDOWN:
                    var keyDown = (Keys)(int)m.WParam;
                    var eDown = new KeyEventArgs(keyDown);
                    CoordinateKeyDown?.Invoke(this, eDown);
                    handled = eDown.Handled;
                    break;

                case WM_CHAR:
                    char keyChar = (char)m.WParam;
                    var ePress = new KeyPressEventArgs(keyChar);
                    CoordinateKeyPress?.Invoke(this, ePress);
                    handled = ePress.Handled;
                    break;

                case WM_KEYUP:
                    var keyUp = (Keys)(int)m.WParam;
                    var eUp = new KeyEventArgs(keyUp);
                    CoordinateKeyUp?.Invoke(this, eUp);
                    handled = eUp.Handled;
                    break;
            }

            if (!handled) base.WndProc(ref m);
        }
        /// <summary>判断是否 InputKey。</summary>
        protected override bool IsInputKey(Keys keyData) => true;


        public HCoordinate()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
            this.DoubleBuffered = true; this.Enabled = true;

            if (!this.DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                CoordinateScreen.RulerFont = Font;

                CoordinateScreen.PromptShowFont = Font;

                ShowStringFont = new Font(Font.FontFamily, Font.Size*2);

                BackColor = HColors.Blacks.Soot;
                ShowStringPen.DashStyle = DashStyle.DashDot;

                CoordinateKeyPress += (s, e) =>
                {
                    HFromUICoordinate_KeyPress(this, e);
                };
                CoordinateKeyDown += (s, e) =>
                {
                    HFromUICoordinate_KeyDown(this, e);
                };
                CoordinateKeyUp += (s, e) =>
                {
                    HFromUICoordinate_KeyUp(this, e);
                };
                Paint += HFromUICoordinate_Paint;
                MouseWheel += HFromUICoordinate_MouseWheel;
                MouseDown += HFromUICoordinate_MouseDown;
                MouseMove += HFromUICoordinate_MouseMove;
                MouseUp += HFromUICoordinate_MouseUp;

                Resize += (s, e) => Refresh();
                SizeChanged += HCoordinate_SizeChanged;



                if (timer == null)
                {
                    timer = new Timer();
                    timer.Interval = 50;
                    timer.Tick += Timer_Tick;
                    timer.Start();
                }
            }

   
        }
        /// <summary>oldDateTimeRefresh 字段。</summary>
        private DateTime oldDateTimeRefresh = DateTime.Now;
        /// <summary>刷新。</summary>
        public override void Refresh()
        {
            CoordinateScreen.PromptShowName = (CoordinateScreen.EnableDraw ? "" : ">") + $@"X:{CoordinateScreen.WorldPoint.X.SetValue(CoordinateScreen.GetDecimalPlaces(false, true)).Value.ToString("f5")}mm Y:{(CoordinateScreen.WorldPoint.Y.SetValue(CoordinateScreen.GetDecimalPlaces(true, true)).Value).ToString("f5")}mm Total:{CoordinateDrawList.Shapes.Count}" + (CoordinateDrawList.IsContinuous ? "★" : "");
            if (CoordinateScreen.RefreshHz > 0 && (CoordinateDrawList.ShowShapes.Count > CoordinateDrawList.ShowShapesInt && CoordinateDrawList.Shapes.Count > CoordinateDrawList.ShowShapesInt))
            {
                if ((DateTime.Now - oldDateTimeRefresh).TotalMilliseconds < CoordinateScreen.RefreshHz)
                {
                    CoordinateScreen.IsRefresh = true;
                    return;
                }
                CoordinateScreen.IsRefresh = false;
                oldDateTimeRefresh = DateTime.Now;
            }
            Invalidate();
            // base.Refresh();
        }
        /// <summary>HCoordinate_SizeChanged 方法。</summary>
        private void HCoordinate_SizeChanged(object sender, EventArgs e)
        {
            if (CoordinateScreen.ScreenRectangle == null)
            {
                CoordinateScreen.ScreenRectangle = new HRect(0, 0, this.Width, this.Height);
            }
            else
            {
                if (CoordinateScreen.ScreenRectangle.Width != this.Width ||
       CoordinateScreen.ScreenRectangle.Height != this.Height)
                {
                    CoordinateScreen.ScreenRectangle.X = 0;
                    CoordinateScreen.ScreenRectangle.Y = 0;
                    CoordinateScreen.ScreenRectangle.Width = this.Width;
                    CoordinateScreen.ScreenRectangle.Height = this.Height;
                }
            }
        }
        /// <summary>GCINT 字段。</summary>
        private int GCINT = 0;
        /// <summary>oldNearWorldPoint 字段。</summary>
        private DateTime oldNearWorldPoint = DateTime.Now;
        /// <summary>Timer_Tick 方法。</summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                CoordinateScreen.PromptShowName = (CoordinateScreen.EnableDraw ? "" : ">") + $@"X:{CoordinateScreen.WorldPoint.X.SetValue(CoordinateScreen.GetDecimalPlaces(false, true)).Value.ToString("f5")}mm Y:{(CoordinateScreen.WorldPoint.Y.SetValue(CoordinateScreen.GetDecimalPlaces(true, true)).Value).ToString("f5")}mm Total:{CoordinateDrawList.Shapes.Count}" + (CoordinateDrawList.IsContinuous ? "★" : "");
                if (CoordinateScreen.ScreenRectangle.Width != this.Width ||
                    CoordinateScreen.ScreenRectangle.Height != this.Height)
                {
                    HCoordinate_SizeChanged(null, null);
                }




                HList<HDrawBase> showShapess = new HList<HDrawBase>();
                HRectangle World = CoordinateScreen.WorldRectangle.ToHRectangle();
                if (CoordinateDrawList.IsRunShapes&& CoordinateDrawList.RunShapes!=null&& CoordinateDrawList.RunShapes.Count>0)
                {
                    foreach (var item in CoordinateDrawList.RunShapes)
                    {
                        if (CoordinateDrawList.IsEnableLayer)
                        {
                            if (World.ContainPointLineRect(item.GetPoint(2, CoordinateScreen.ViewScale), 1))
                            {
                                showShapess.Add(item);
                            }
                        }
                        else
                        {
                            if (CoordinateDrawList.Layer == item.Layer)
                            {
                                if (World.ContainPointLineRect(item.GetPoint(2, CoordinateScreen.ViewScale), 1))
                                {
                                    showShapess.Add(item);
                                }
                            }
                        }

                    }
                }
                else
                {
                    foreach (var item in CoordinateDrawList.Shapes)
                    {
                        if (CoordinateDrawList.IsEnableLayer)
                        {
                            if (World.ContainPointLineRect(item.GetPoint(2, CoordinateScreen.ViewScale), 1))
                            {
                                showShapess.Add(item);
                            }
                        }
                        else
                        {
                            if (CoordinateDrawList.Layer == item.Layer)
                            {
                                if (World.ContainPointLineRect(item.GetPoint(2, CoordinateScreen.ViewScale), 1))
                                {
                                    showShapess.Add(item);
                                }
                            }
                        }

                    }
                }

                CoordinateDrawList.ShowShapes = showShapess;

                List<HPoint3D> startEndPoints = new List<HPoint3D>();
                foreach (var item in CoordinateDrawList.ShowShapes)
                {
                    startEndPoints.AddRange(item.GetPoint(0, CoordinateScreen.ViewScale));
                }
                CoordinateDrawList.StartEndPoints = startEndPoints;

                if (CoordinateDrawList.IsCapture && CoordinateScreen.EnableDraw)
                {
                    HList<HPoint3D> hPoints = new HList<HPoint3D>();
                    hPoints.Add(CoordinateDrawList.StartEndPoints);
                    hPoints.Add(CoordinateScreen.GetNearWorldPoint3D());
                    bool ischPoints = false;
                    foreach (var item in hPoints)
                    {
                        if (Math.Abs(HDrawList.Distance(CoordinateScreen.WorldPoint, item).Value) < CoordinateScreen.ScreenLengthToWorld(CoordinateScreen.NearWorldPointHeight.Value, false))
                        {
                            ischPoints = true;
                            CoordinateScreen.NearWorldPoint = item.Clone();
                            break;
                        }
                    }
                    if (!ischPoints)
                    {
                        CoordinateScreen.NearWorldPoint = null;
                    }
                    ischPoints = false;
                    if (CoordinateScreen.NearWorldPoint == null)
                    {
                        CoordinateScreen.IsSetNearWorld = false;
                        oldNearWorldPoint = DateTime.Now;
                    }
                    else
                    {
                        if (!CoordinateScreen.IsSetNearWorld)
                        {
                            double timebuffer = (DateTime.Now - oldNearWorldPoint).TotalMilliseconds;
                            if (timebuffer > CoordinateScreen.NearWorldPointTime)
                            {
                                // ControlToWindowsScreen 返回 GDI Point，需显式包装为 HPoint
            HPoint hPoint = new HPoint(HScreen.ControlToWindowsScreen(this, CoordinateScreen.WorldToScreen(CoordinateScreen.NearWorldPoint).ToPoint()));
                                HScreen.SetMouseWindowsScreenPosition(hPoint.ToPoint());
                                CoordinateScreen.IsSetNearWorld = true;
                                CoordinateScreen.SetWorldPoint(CoordinateScreen.NearWorldPoint);
                                Refresh();
                            }
                        }
                        if (CoordinateScreen.IsSetNearWorld)
                        {
                            CoordinateScreen.SetWorldPoint(CoordinateScreen.NearWorldPoint);
                        }

                    }
                }
                else
                {
                    CoordinateScreen.IsSetNearWorld = false;
                }

                if (CoordinateScreen.IsRefresh)
                {
                    Refresh();
                }
            }
catch { }

            GCINT++;
            if (GCINT > 200)
            {
                GCINT = 0;
                GC.Collect();
            }
        }

        /// <summary>HFromUICoordinate_KeyPress 方法。</summary>
        private void HFromUICoordinate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            if (!CoordinateScreen.EnableDraw)
            {
                CoordinateDrawList.SelectedShapeType = HShapeType.None;
                return;
            }
            if ((Keys)e.KeyChar == Keys.Escape)
            {
                if (CoordinateDrawList.SelectGUID.Count > 0 && CoordinateDrawList.SelectedShapeType == HShapeType.Select)
                {
                    int index = CoordinateDrawList.SelectGUID.Count - 1;
                    CoordinateDrawList.SelectGUID.RemoveAt(index);
                }
                Refresh();
                return;
            }
            if ((Keys)e.KeyChar == Keys.Back || (Keys)e.KeyChar == Keys.Enter || (Keys)e.KeyChar == Keys.Tab)
            {
                if ((Keys)e.KeyChar == Keys.Enter)
                {
                    try
                    {
                        string buffer = ShowStringBuilder.ToString();
                        if (buffer.ToUpper() == "DEL")
                        {
                            HList<string> bufferDel = new HList<string>(CoordinateDrawList.SelectGUID);
                            CoordinateDrawList.SelectGUID.Clear();
                            CoordinateDrawList.SelectGUID.Add(CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count-1].GuidCode);
                            DeleteShapes();
                            CoordinateDrawList.SelectGUID = bufferDel;
                        }
                        else if (buffer.ToUpper() == "DELALL")
                        {
                            CoordinateDrawList.DrawChangeClear();
                            CoordinateDrawList.Shapes.Clear();
                            Clear();SelectShape(-1);
                        }
                        else if (buffer.ToUpper() == "ENABLE" || buffer.ToUpper() == "E")
                        {
                            CoordinateScreen.EnableDraw = !CoordinateScreen.EnableDraw;
                            if (!CoordinateScreen.EnableDraw)
                            {
                                CoordinateDrawList.SelectedShapeType = HShapeType.None;
                            }
                        }
                        else if (buffer.ToUpper().StartsWith("O"))
                        {
                            if (buffer.ToUpper() == "O")
                            {
                                CoordinateScreen.ViewOffsetCenter(0, 0);
                            }
                            else
                            {
                                string[] buffers = buffer.Replace("o", "").Replace("O", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (buffers.Length == 2)
                                {
                                    CoordinateScreen.ViewOffsetCenter(Convert.ToDouble(buffers[0]), Convert.ToDouble(buffers[1]));
                                }
                            }

                        }
                        else
                        {
                            if (buffer.Contains(","))
                            {
                                string[] buffers = buffer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (buffers.Length == 2)
                                {
                                    double x = Convert.ToDouble(buffers[0]); double y = Convert.ToDouble(buffers[1]);
                                    CoordinateDrawList.EndPoint= new HPoint(x, y);
                                    AddPoint(true);
                                }
                            }
                        }
                        ShowStringBuilder.Clear();
                    }
catch { }
                    Invalidate();
                }

            }
            else
            {
                ShowStringBuilder.Append(e.KeyChar);
                if (ShowStringPoint!=null)
                {
                    HIme.SetCandidatePosition(this, ShowStringPoint.ToPoint());
                }
            
            }
        }

        /// <summary>HFromUICoordinate_KeyDown 方法。</summary>
        private void HFromUICoordinate_KeyDown(object sender, KeyEventArgs e)
        {
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            if (!CoordinateScreen.EnableDraw)
            {
                CoordinateDrawList.SelectedShapeType = HShapeType.None;
                return;
            }
            if (e.KeyCode == Keys.Delete)
            {
                DeleteShapes(); Clear();
                ShowStringBuilder.Clear();
                CoordinateScreen.IsRefresh = true;
            }

            if (e.KeyData == Keys.Up || e.KeyData == Keys.Down || e.KeyData == Keys.Left || e.KeyData == Keys.Right)
            {
                if (e.KeyData == Keys.Up)
                {
                    MoveShape(106);
                }
                if (e.KeyData == Keys.Down)
                {
                    MoveShape(107);
                }
                if (e.KeyData == Keys.Left)
                {
                    MoveShape(105);
                }
                if (e.KeyData == Keys.Right)
                {
                    MoveShape(104);
                }
            }

            
            if (e.KeyData == Keys.Back)
            {
                if (ShowStringBuilder.Length > 0)
                {
                    ShowStringBuilder.Remove(ShowStringBuilder.Length - 1, 1);
                }
                CoordinateScreen.IsRefresh = true;
            }
            if (e.KeyData == Keys.F3)
            {
                CoordinateDrawList.SelectedShapeType = HShapeType.Select;
                Clear();SelectShape(-1);
                CoordinateScreen.IsRefresh = true;
            }
            if (e.KeyData == Keys.F1 || (e.KeyData == Keys.F2))
            {
                Clear(); SelectShape(-1);
                if (e.KeyData == Keys.F1)
                {
                    CoordinateDrawList.IsContinuous = !CoordinateDrawList.IsContinuous;
                }

                switch (CoordinateDrawList.SelectedShapeType)
                {
                    case HShapeType.None:
                        if (e.KeyData == Keys.F2)
                        {
                            CoordinateDrawList.SelectedShapeType = HShapeType.Line;
                        }
                        break;
                    case HShapeType.Line:
                        if (e.KeyData == Keys.F2)
                        {
                            CoordinateDrawList.SelectedShapeType = HShapeType.Point;
                        }
                        break;
                    case HShapeType.Point:
                        if (e.KeyData == Keys.F2)
                        {
                            CoordinateDrawList.SelectedShapeType = HShapeType.Arc3P;
                        }
                        break;
                    case HShapeType.Arc3P:
                        if (e.KeyData == Keys.F2)
                        {
                            CoordinateDrawList.SelectedShapeType = HShapeType.None;
                        }
                        break;
                    default:
                        CoordinateDrawList.SelectedShapeType = HShapeType.None;
                        break;
                }
                CoordinateScreen.IsRefresh = true;
            }
            if (e.KeyData == Keys.F4|| e.KeyData == Keys.F5|| e.KeyData == Keys.F6)
            {
                SelectShape(-1); Clear();
                if (e.KeyData == Keys.F4)
                {
                    CoordinateDrawList.SelectedShapeType = HShapeType.Line;
                }
                if (e.KeyData == Keys.F5)
                {
                    CoordinateDrawList.SelectedShapeType = HShapeType.Arc3P;
                }
                if (e.KeyData == Keys.F6)
                {
                    CoordinateDrawList.SelectedShapeType = HShapeType.Point;
                }
                if (CoordinateDrawList.IsContinuous)
                {
                    AddPoint(true);
                }
            }

        }
        /// <summary>HFromUICoordinate_KeyUp 方法。</summary>
        private void HFromUICoordinate_KeyUp(object sender, KeyEventArgs e)
        {
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
        }

        /// <summary>HFromUICoordinate_Paint 方法。</summary>
        private void HFromUICoordinate_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BackColor);
                HCoordinate_SizeChanged(null, null);
                g.SetClip(CoordinateScreen.ScreenRectangle.ToRectangleF());
                CoordinateScreen.DrawCoordinateA(g);
                CoordinateDrawList.DrawFormat(g, CoordinateScreen.WorldToScreen, true, CoordinateScreen.ViewScale);
                CoordinateScreen.DrawPromptShowNameA(g);
                foreach (var shape in CoordinateDrawList.VirtualShapes)
                {
                    shape.PenMode = DashStyle.Dash;
                    shape.Draw(g, CoordinateScreen.WorldToScreen, false, CoordinateScreen.ViewScale);
                }
                if (CoordinateDrawList.TempShape != null)
                {
                    CoordinateDrawList.TempShape.Draw(g, CoordinateScreen.WorldToScreen, false, CoordinateScreen.ViewScale);
                }
                foreach (var item in CoordinateDrawList.ShowShapes)
                {
                    bool isSelect = false;
                    foreach (var guid in CoordinateDrawList.SelectGUID)
                    {
                        if (item.GuidCode == guid)
                        {
                            isSelect = true;
                            break;
                        }
                    }
                    item.Draw(g, CoordinateScreen.WorldToScreen, isSelect, CoordinateScreen.ViewScale);
                }

                HRect hRectSelect = CoordinateScreen.GetSelectScreenRect();
                if (hRectSelect != null)
                {
                    g.DrawRectangle(CoordinateScreen.SelectPen, hRectSelect.ToRectangle());
                }
                CoordinateDrawList.DrawShowPoint(g, CoordinateScreen.WorldToScreen, true, CoordinateScreen.ViewScale);
                CoordinateDrawList.DrawRunShapes(g, CoordinateScreen.WorldToScreen, false, CoordinateScreen.ViewScale);
                DrawShowString(g);
                g.ResetClip();

            }
catch { }
        }
        /// <summary>DrawShowString 方法。</summary>
        private void DrawShowString(Graphics g)
        {
            if (!CoordinateScreen.EnableDraw)
            {
                return;
            }
            bool isSelect = false;
            switch (CoordinateDrawList.SelectedShapeType)
            {
                case HShapeType.Select:
                    ShowString = "Select";
                    isSelect = true;
                    break;
                case HShapeType.None:
                    ShowString = "Draw";
                    break;
                case HShapeType.Line:
                    ShowString = "Line";
                    break;
                case HShapeType.Arc3P:
                    ShowString = "Arc";
                    break;
                case HShapeType.Point:
                    ShowString = "Point";
                    break;
                default:
                    ShowString = "None";
                    break;
            }

            string  labelName = $@"{ShowString}.{(isSelect ? (CoordinateDrawList.SelectGUID.Count ):(CoordinateScreen.DrawPointsCount + 1)).ToString()}:" + ShowStringBuilder.ToString();
            if (ShowStringFont==null)
            {
                ShowStringFont = new Font(Font.FontFamily, Font.Size * 2);
            }
            SizeF showtextSize = g.MeasureString(labelName,ShowStringFont);
            if (showtextSize.Width < 150)
            {
                showtextSize.Width = 150;
            }
            float xshow = Width / 2 - showtextSize.Width / 2 - 2; float yshow = Height - showtextSize.Height - 30;
            g.DrawString(labelName,ShowStringFont, CoordinateScreen.RulerTextBrush, xshow, yshow);
            g.DrawRectangle(ShowStringPen, xshow-5,yshow - 5, showtextSize.Width+10, showtextSize.Height + 10);
            ShowStringPoint = new HPoint(xshow+ showtextSize.Width/2, yshow + showtextSize.Height);
        }
        /// <summary>HFromUICoordinate_MouseDown 方法。</summary>
        private void HFromUICoordinate_MouseDown(object sender, MouseEventArgs e)
        {
            CoordinateScreen.ScreenPoint = new HPoint(e.Location);
            CoordinateScreen.MouseDown = true;


            if (e.Button == MouseButtons.Right)
            {
                Clear(); SelectShape(-1);
            }
            if (e.Button == MouseButtons.Left)
            {

                CoordinateDrawList.LastMousePositionWorldPoint = CoordinateScreen.WorldPoint.Clone();
                switch (CoordinateDrawList.SelectedShapeType)
                {
                    case HShapeType.Select:
                        if (!CoordinateDrawList.IsRunShapes && MoveShape(-1).IsSuccess && CoordinateScreen.EnableDraw.Value)
                        {
                            if (!MoveShape(1))
                            {
                                Clear(); SelectShape(-1);
                            }
                            if (CoordinateDrawList.SelectDrawBase != null)
                            {
                                CoordinateDrawList.SelectDrawBase.Move(null, 6);
                            }
                        }
                        else
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                SelectShape();
                            }
                            CoordinateScreen.SelectScreenPoint1 = CoordinateScreen.ScreenPoint;
                            CoordinateScreen.SelectScreenPoint2 = null;
                        }
                        break;
                    case HShapeType.None:
                        if (!CoordinateDrawList.IsRunShapes && MoveShape(-1).IsSuccess && CoordinateScreen.EnableDraw.Value)
                        {
                            if (!MoveShape(1))
                            {
                                Clear(); SelectShape(-1);
                            }
                            if (CoordinateDrawList.SelectDrawBase != null)
                            {
                                CoordinateDrawList.SelectDrawBase.Move(null, 6);
                            }
                        }
                        else
                        {
                            Clear();
                            if (e.Button == MouseButtons.Left)
                            {
                                CoordinateDrawList.LastMousePosition = new HPoint(e.Location);
                                CoordinateDrawList.LastViewOffsetPosition = CoordinateScreen.ViewOffset.Clone();
                                CoordinateDrawList.IsDragging = true;


                                SelectShape();
                            }
                        }

                        break;
                    case HShapeType.Point:
                        AddPoint(CoordinateDrawList.IsContinuous.Value);
                        break;
                    case HShapeType.Line:
                    case HShapeType.Arc3P:
                        AddPoint();
                        break;
                    default:
                        break;
                }

            }

            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            Refresh();
        }

        /// <summary>HFromUICoordinate_MouseMove 方法。</summary>
        private void HFromUICoordinate_MouseMove(object sender, MouseEventArgs e)
        {
            CoordinateScreen.ScreenPoint = new HPoint(e.Location);

            switch (CoordinateDrawList.SelectedShapeType)
            {
                case HShapeType.None:
                    if (!CoordinateDrawList.IsRunShapes && MoveShape(-1).IsSuccess && CoordinateScreen.EnableDraw.Value)
                    {
                        if (CoordinateScreen.MouseDown)
                        {
                            MoveShape();
                        }
                    }
                    else
                    {
                        if (CoordinateDrawList.IsDragging)
                        {
                            HDouble ViewOffsetx = CoordinateDrawList.LastViewOffsetPosition.X.Value + 1d * (e.X - CoordinateDrawList.LastMousePosition.X.Value);
                            HDouble ViewOffsety = CoordinateDrawList.LastViewOffsetPosition.Y.Value + -1d * (e.Y - CoordinateDrawList.LastMousePosition.Y.Value);
                            CoordinateScreen.ViewOffset = new HPoint(ViewOffsetx, ViewOffsety);
                        }
                    }

                    break;
                case HShapeType.Line:
                    if (CoordinateScreen.DrawPointsCount == 1)
                    {
                        if (CoordinateDrawList.TempShape == null)
                        {
                            CoordinateDrawList.TempShape = new HDrawLine(new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value), new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value), CoordinateDrawList.ColorLine, CoordinateDrawList.LineWidth, HTranslation.GetContent(""));
                        }
                        ((HDrawLine)CoordinateDrawList.TempShape).End = new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value);
                    }
                    break;
                case HShapeType.Point:
                    if (CoordinateScreen.DrawPointsCount == 0)
                    {
                        if (CoordinateDrawList.TempShape == null)
                        {
                            CoordinateDrawList.TempShape = new HDrawPoint(new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value), Color.DodgerBlue, 2, HTranslation.GetContent(""));
                        }
                        if (CoordinateDrawList.IsContinuous && CoordinateDrawList.EndPoint != null)
                        {
                            ((HDrawPoint)CoordinateDrawList.TempShape).Center = new HPoint3D(CoordinateDrawList.EndPoint, CoordinateDrawList.LastZ.Value);
                        }
                        else
                        {
                            ((HDrawPoint)CoordinateDrawList.TempShape).Center = new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value);
                        }

                    }
                    break;
                case HShapeType.Arc3P:
                    CoordinateDrawList.Direction = false;
                    if (CoordinateScreen.DrawPointsCount == 1)
                    {
                        if (CoordinateDrawList.VirtualShapes.Count == 0)
                        {
                            HDrawBase virtualShape = new HDrawLine(new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value), new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value), Color.Orange, 1, HTranslation.GetContent(""));
                            CoordinateDrawList.VirtualShapes.Add(virtualShape);
                        }

                    }
                    if (CoordinateDrawList.VirtualShapes.Count > 0)
                    {
                        ((HDrawLine)CoordinateDrawList.VirtualShapes[0]).End = new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value);
                    }
                    if (CoordinateScreen.DrawPointsCount == 2)
                    {
                        ((HDraw3PArc)CoordinateDrawList.TempShape).Middle = new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value);
                    }
                    break;
                case HShapeType.Select:
                    if (!CoordinateDrawList.IsRunShapes && MoveShape(-1).IsSuccess && CoordinateScreen.EnableDraw.Value)
                    {
                        if (CoordinateScreen.MouseDown)
                        {
                            MoveShape();
                        }
                    }
                    else
                    {
                        if (CoordinateScreen.MouseDown)
                        {
                            CoordinateScreen.SelectScreenPoint2 = CoordinateScreen.ScreenPoint;
                        }
                    }
                    break;
                default:
                    break;
            }
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            Refresh();
        }

        /// <summary>HFromUICoordinate_MouseUp 方法。</summary>
        private void HFromUICoordinate_MouseUp(object sender, MouseEventArgs e)
        {
            CoordinateScreen.ScreenPoint = new HPoint(e.Location);

            switch (CoordinateDrawList.SelectedShapeType)
            {
                case HShapeType.None:
                    if (MoveShape(-1))
                    {
                        CoordinateDrawList.OnSelectValueChanged(CoordinateDrawList.SelectDrawBase, null);
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        CoordinateDrawList.IsDragging = false;
                    }
                    MoveShape(3);
                    break;
                case HShapeType.Select:
                    if (MoveShape(-1))
                    {
                        CoordinateDrawList.OnSelectValueChanged(CoordinateDrawList.SelectDrawBase, null);
                    }
                    else
                    {
                        SelectShape(1);
                        Clear();
                    }
                    MoveShape(3);
                    break;
                default:
                    break;
            }


            CoordinateScreen.MouseDown = false;
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            if (CoordinateDrawList.IsRunShapes)
            {

                return;
            }
            Refresh();
        }
        /// <summary>SelectShape 方法。</summary>
        public void SelectShape(int mode = 0)
        {
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            if (mode != 1)
            {
                if (!CoordinateScreen.IsAddSelect)
                {
                    CoordinateDrawList.SelectDrawBase = null;
                    CoordinateDrawList.SelectGUID.Clear();
                    if (CoordinateDrawList.ShowPoint != null)
                    {
                        CoordinateDrawList.ShowPoint.Clear();
                    }
                    CoordinateDrawList.ShowPoint = null;

                }
            }
            if (mode == 0)
            {
                foreach (var item in CoordinateDrawList.ShowShapes)
                {

                    if (item.HitTest(CoordinateScreen.WorldPoint, CoordinateScreen.ScreenLengthToWorld(CoordinateDrawList.SelectLineHeight.Value, false)))
                    {
                        CoordinateDrawList.AddSelectGUID(item.GuidCode);
                        break;
                    }
                }
            }
            else if (mode == 1)
            {
                HRect hRect = CoordinateScreen.GetSelectWorldRect();
                if (hRect != null)
                {
                    if (!CoordinateScreen.IsAddSelect)
                    {
                        CoordinateDrawList.SelectGUID.Clear();
                    }
                    HRectangle hRectangle = hRect.ToHRectangle();
                    foreach (var item in CoordinateDrawList.ShowShapes)
                    {
                        if (hRectangle.ContainPointLineRect(item.GetPoint(1, CoordinateScreen.ViewScale)))
                        {
                            CoordinateDrawList.AddSelectGUID(item.GuidCode);
                        }
                    }
                    if (CoordinateDrawList.SelectGUID.Count != 1)
                    {
                        CoordinateDrawList.SelectDrawBase = null;
                    }
                }
            }
        }
        /// <summary>HFromUICoordinate_MouseWheel 方法。</summary>
        private void HFromUICoordinate_MouseWheel(object sender, MouseEventArgs e)
        {
            HPoint mouseScreen = CoordinateScreen.ScreenPoint = new HPoint(e.Location);
            HPoint worldUnderMouse = CoordinateScreen.WorldPoint;
            HDouble oldScale = CoordinateScreen.ViewScale;

            HDouble newScale = (CoordinateScreen.ViewScale * (e.Delta > 0 ? 1.2 : 0.8));
            newScale = Math.Max(CoordinateScreen.ViewScaleMin.Value, Math.Min(CoordinateScreen.ViewScaleMax.Value, newScale.Value));

            HDouble newOffsetX = mouseScreen.X.Value - worldUnderMouse.X.Value * newScale;
            HDouble newOffsetY = this.Height - mouseScreen.Y.Value - worldUnderMouse.Y.Value * newScale;

            CoordinateScreen.ViewScale = newScale;
            CoordinateScreen.ViewOffset = new HPoint(newOffsetX, newOffsetY);

            // 更新鼠标位置属性（可选）
            CoordinateScreen.ScreenPoint = new HPoint(e.Location); // 这会用新变换更新 worldPos
            Refresh();
        }
        /// <summary>清空。</summary>
        public void Clear()
        {

            CoordinateScreen.DrawPoints.Clear();
            CoordinateDrawList.VirtualShapes.Clear();
            CoordinateDrawList.TempShape = null;
            CoordinateDrawList.Direction = false;
            CoordinateDrawList.IsDragging = false;
            CoordinateScreen.SelectScreenPoint1 = CoordinateScreen.SelectScreenPoint2 = null;

        }
        /// <summary>AddPoint 方法。</summary>
        public void AddPoint(bool IsEnd = false)
        {
            if (CoordinateDrawList.IsRunShapes)
            {
                return;
            }
            if (CoordinateDrawList.Shapes.Count == 0)
            {
                CoordinateDrawList.LastZ = 0;
            }
            else
            {
                if (CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1] is HDraw3PArc)
                {
                    HDraw3PArc draw3PArc = CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1] as HDraw3PArc;
                    CoordinateDrawList.LastZ = draw3PArc.End.Z.Value;
                }
                else if (CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1] is HDrawLine)
                {
                    HDrawLine draw3PArc = CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1] as HDrawLine;
                    CoordinateDrawList.LastZ = draw3PArc.End.Z.Value;
                }
                else if (CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1] is HDrawPoint)
                {
                    HDrawPoint draw3PArc = CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1] as HDrawPoint;
                    CoordinateDrawList.LastZ = draw3PArc.Center.Z.Value;
                }
            }

            if (!CoordinateScreen.EnableDraw)
            {
                CoordinateDrawList.SelectedShapeType = HShapeType.None;
            }
            switch (CoordinateDrawList.SelectedShapeType)
            {
                case HShapeType.None:
                    break;
                case HShapeType.Line:
                    if (IsEnd)
                    {
                        if (CoordinateDrawList.EndPoint != null)
                        {
                            CoordinateScreen.AddDrawPoint(CoordinateDrawList.EndPoint);
                        }

                    }
                    else
                    {
                        CoordinateScreen.AddDrawPoint();
                    }

                    if (CoordinateScreen.DrawPointsCount == 2)
                    {
                        CoordinateDrawList.EndPoint = ((HDrawLine)CoordinateDrawList.TempShape).End = new HPoint3D(CoordinateScreen.DrawPoints[1], CoordinateDrawList.LastZ.Value);
                        if (HDrawList.Distance(((HDrawLine)CoordinateDrawList.TempShape).End, ((HDrawLine)CoordinateDrawList.TempShape).Start) < HAppData.Epsilon)
                        {
                            Clear();
                        }
                        else
                        {
                            AddShapes();

                        }

                        Clear();
                        Refresh();
                        if (CoordinateDrawList.IsContinuous)
                        {
                            CoordinateScreen.AddDrawPoint(CoordinateDrawList.EndPoint);
                        }
                    }
                    if (CoordinateScreen.DrawPointsCount == 1)
                    {
                        CoordinateDrawList.TempShape = new HDrawLine(new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value), new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value), CoordinateDrawList.ColorLine, CoordinateDrawList.LineWidth, HTranslation.GetContent(""));
                    }
                    break;
                case HShapeType.Point:
                    if (IsEnd)
                    {
                        if (CoordinateDrawList.EndPoint != null)
                        {
                            CoordinateScreen.AddDrawPoint(CoordinateDrawList.EndPoint);
                        }
                        else
                        {
                            CoordinateScreen.AddDrawPoint();
                        }
                    }
                    else
                    {
                        CoordinateScreen.AddDrawPoint();
                    }
                    if (CoordinateScreen.DrawPointsCount == 1)
                    {
                        if (CoordinateDrawList.TempShape == null)
                        {
                            CoordinateDrawList.TempShape = new HDrawPoint(new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value), CoordinateDrawList.ColorPoint, CoordinateDrawList.LineWidth, HTranslation.GetContent(""));
                        }
                        CoordinateDrawList.EndPoint = ((HDrawPoint)CoordinateDrawList.TempShape).Center = new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value);
                        AddShapes();

                        Clear();
                        Refresh();
                        if (CoordinateDrawList.IsContinuous)
                        {
                            CoordinateScreen.AddDrawPoint(CoordinateDrawList.EndPoint);
                        }
                    }
                    break;
                case HShapeType.Arc3P:
                    if (IsEnd)
                    {
                        if (CoordinateDrawList.EndPoint != null)
                        {
                            CoordinateScreen.AddDrawPoint(CoordinateDrawList.EndPoint);
                        }
                    }
                    else
                    {
                        CoordinateScreen.AddDrawPoint();
                    }
                    if (CoordinateScreen.DrawPointsCount == 3)
                    {
                        HPoint3D start = new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value);
                        HPoint3D end = new HPoint3D(CoordinateScreen.DrawPoints[1], CoordinateDrawList.LastZ.Value);
                        HPoint3D mid = new HPoint3D(CoordinateScreen.DrawPoints[2], CoordinateDrawList.LastZ.Value);
                      
                        if (mid!=end&&mid!=start&& start!=null && end != null && mid != null)
                        {
                            ((HDraw3PArc)CoordinateDrawList.TempShape).Middle = mid;
                            ((HDraw3PArc)CoordinateDrawList.TempShape).Start = start;
                            CoordinateDrawList.EndPoint = ((HDraw3PArc)CoordinateDrawList.TempShape).End = end;
                            if (((HDraw3PArc)CoordinateDrawList.TempShape).CircleCenter!=null)
                            {
                                AddShapes();

                            }
                        }
                        Clear();
                        Refresh();
                        if (CoordinateDrawList.IsContinuous)
                        {
                            CoordinateScreen.AddDrawPoint(CoordinateDrawList.EndPoint);
                        }
                    }
                    if (CoordinateScreen.DrawPointsCount == 1)
                    {
                        HDrawBase virtualShape = new HDrawLine(new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value), new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value), Color.Orange, 1, HTranslation.GetContent(""));
                        CoordinateDrawList.VirtualShapes.Add(virtualShape);
                    }
                    if (CoordinateScreen.DrawPointsCount == 2)
                    {
                        CoordinateDrawList.VirtualShapes.Clear();
                        HPoint3D start = new HPoint3D(CoordinateScreen.DrawPoints[0], CoordinateDrawList.LastZ.Value);
                        HPoint3D end = new HPoint3D(CoordinateScreen.DrawPoints[1], CoordinateDrawList.LastZ.Value);
                        HPoint3D mid = new HPoint3D(CoordinateScreen.WorldPoint, CoordinateDrawList.LastZ.Value);
                        CoordinateDrawList.TempShape = new HDraw3PArc(start, end, mid, CoordinateDrawList.Color3PArc, CoordinateDrawList.LineWidth, HTranslation.GetContent(""));
                        if (HDrawList.Distance(start, end) < HAppData.Epsilon)
                        {
                            Clear();
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private HDrawChange EndDrawChange;
        /// <summary>movePoint 字段。</summary>
        private bool movePoint = false;
        /// <summary>isCanAddChange 字段。</summary>
        private bool isCanAddChange = true;
        /// <summary>MoveShape 方法。</summary>
        public OK MoveShape(int mode = 0)
        {
            if (mode!=104&& mode != 105&& mode != 106&& mode != 107)
            {
                if (CoordinateDrawList.SelectDrawBase == null)
                {
                    EndDrawChange = null;
                    return false;

                }
            }

            if (mode == -1)//判断是不是为null
            {
                return true;
            }
            OK oK = false;
            if (!CoordinateScreen.EnableDraw)
            {
                return oK;
            }
            if (mode == 0)
            {
                if (CoordinateDrawList.SelectDrawBase is HDrawGroup)
                {
                    movePoint = false;
                }
                if (movePoint)
                {
                    if (EndDrawChange != null || CoordinateDrawList.SelectDrawBase is HDrawGroup)
                    {
                        if (isCanAddChange)
                        {
                            isCanAddChange = false;
                            CoordinateDrawList.DrawChangeAdd(HDrawChange.Update(CoordinateDrawList.GetShapesIndex(CoordinateDrawList.SelectDrawBase.GuidCode), CoordinateDrawList.SelectDrawBase.Clone()));

                        }
                        if (CoordinateDrawList.SelectDrawBase is HDraw3PArc)
                        {
                            HDraw3PArc HDraw3PArc = CoordinateDrawList.SelectDrawBase as HDraw3PArc;
                            HDouble z = HDraw3PArc[EndDrawChange.Name].Z.Value;
                            CoordinateDrawList.ShowPoint[EndDrawChange.Name] = HDraw3PArc[EndDrawChange.Name] = new HPoint3D(CoordinateScreen.WorldPoint, z.Value);

                        }
                        else if (CoordinateDrawList.SelectDrawBase is HDrawLine)
                        {
                            HDrawLine HDrawLine = CoordinateDrawList.SelectDrawBase as HDrawLine;
                            HDouble z = HDrawLine[EndDrawChange.Name].Z.Value;
                            CoordinateDrawList.ShowPoint[EndDrawChange.Name] = HDrawLine[EndDrawChange.Name] = new HPoint3D(CoordinateScreen.WorldPoint, z.Value);
                        }
                        else if (CoordinateDrawList.SelectDrawBase is HDrawPoint)
                        {
                            HDrawPoint HDrawPoint = CoordinateDrawList.SelectDrawBase as HDrawPoint;
                            HDouble z = HDrawPoint[EndDrawChange.Name].Z.Value;
                            CoordinateDrawList.ShowPoint[EndDrawChange.Name] = HDrawPoint[EndDrawChange.Name] = new HPoint3D(CoordinateScreen.WorldPoint, z.Value);
                        }
                    
                    }
                }
                else
                {
                    HPoint hPoint = CoordinateScreen.WorldPoint - CoordinateDrawList.LastMousePositionWorldPoint;
                    if (hPoint.X.Value != 0 && hPoint.Y.Value != 0)
                    {
                        if (isCanAddChange)
                        {
                            isCanAddChange = false;
                            CoordinateDrawList.DrawChangeAdd(HDrawChange.Update(CoordinateDrawList.GetShapesIndex(CoordinateDrawList.SelectDrawBase.GuidCode), CoordinateDrawList.SelectDrawBase.Clone()));

                        }
                        CoordinateDrawList.SelectDrawBase.Move(new HPoint3D[] { new HPoint3D(hPoint, 0) }, 7);
                        CoordinateDrawList.ShowPoint.Clear();
                    }


                }
            }
            else if (mode == 1)//判断是那一个点
            {
                if (CoordinateDrawList.SelectDrawBase is HDrawGroup)
                {
                    HDrawGroup HDrawGroup = CoordinateDrawList.SelectDrawBase as HDrawGroup;
                    if (HDrawGroup.HitTest(CoordinateScreen.WorldPoint, CoordinateScreen.ScreenLengthToWorld(CoordinateDrawList.SelectPointHeight.Value, false)))
                    {
                        return true;
                    }
                    HDrawGroup.Move(null, 8);
                    return false;
                }
                else
                {
                    foreach (var item in CoordinateDrawList.ShowPoint.Keys)
                    {
                        if (Math.Abs(HDrawList.Distance(CoordinateScreen.WorldPoint, CoordinateDrawList.ShowPoint[item]).Value) < CoordinateScreen.ScreenLengthToWorld(CoordinateDrawList.SelectPointHeight.Value, false))
                        {
                            movePoint = true;
                            EndDrawChange = new HDrawChange();
                            EndDrawChange.Name = item;
                            EndDrawChange.Data.GetOrAdd(0, CoordinateDrawList.SelectDrawBase);
                            EndDrawChange.OperationMode = HOperationMode.Update;
                            return oK = true;
                        }
                    }
                    if (CoordinateDrawList.SelectDrawBase.HitTest(CoordinateScreen.WorldPoint, CoordinateScreen.ScreenLengthToWorld(CoordinateDrawList.SelectPointHeight.Value, false)))
                    {
                        movePoint = false;
                        return true;
                    }
                    CoordinateDrawList.SelectDrawBase.Move(null, 8);


                    return false;
                }


            }
            else if (mode == 3)
            {
                isCanAddChange = true;
                if (!movePoint && CoordinateDrawList.SelectDrawBase != null)
                {
                    CoordinateDrawList.SelectDrawBase.Move(null, 8);
                    CoordinateDrawList.SelectDrawBase = CoordinateDrawList.SelectDrawBase;
                }
            }
            else if (mode == 104|| mode == 105 || mode == 106 || mode == 107)
            {
                if (CoordinateDrawList.Shapes.Count==1)
                {
                    return oK;
                }
                List<int> indexs = new List<int>(); List<HDrawBase> HDrawBases = new List<HDrawBase>();
                for (int i = 0; i < CoordinateDrawList.Shapes.Count; i++)
                {
                    bool isSelectedshape = false;
                    foreach (var item in CoordinateDrawList.SelectGUID)
                    {
                        if (CoordinateDrawList.Shapes[i].GuidCode == item)
                        {
                            isSelectedshape = true;
                            break;
                        }
                    }
                    if (isSelectedshape)
                    {
                        indexs.Add(i); HDrawBases.Add(CoordinateDrawList.Shapes[i].Clone());
                        if (mode == 104 || mode == 105)
                        {
                            CoordinateDrawList.Shapes[i].Move(new HPoint3D[] { new HPoint3D(((mode == 104 ? 1d : -1d) * CoordinateDrawList.KeysMoveLength).Value, 0, 0) }, 2);
                        }
                        else if (mode == 106 || mode == 107)
                        {
                            CoordinateDrawList.Shapes[i].Move(new HPoint3D[] { new HPoint3D( 0, ((mode == 106 ? 1d : -1d) * CoordinateDrawList.KeysMoveLength).Value, 0) }, 2);
                        }
                     
                    }


                }
                CoordinateDrawList.DrawChangeAdd(HDrawChange.Update(indexs.ToArray(), HDrawBases.ToArray()));
                MoveShape(3);
                Refresh();
            }
            return oK;
        }
        /// <summary>AddShapes 方法。</summary>
        public void AddShapes()
        {
            CoordinateDrawList.TempShape.GuidCode = String.Empty;
            if (CoordinateDrawList.TempShape is HDraw3PArc)
            {
                HDraw3PArc HDraw3PArc = CoordinateDrawList.TempShape as HDraw3PArc;
                if (CoordinateDrawList.SharedDraw3PArc == null)
                {
                    CoordinateDrawList.SharedDraw3PArc = (HDraw3PArc)HDraw3PArc.Clone();
                }
                CoordinateDrawList.SharedDraw3PArc.Layer = CoordinateDrawList.Layer;
                CoordinateDrawList.SharedDraw3PArc.End = HDraw3PArc.End.Clone3D();
                CoordinateDrawList.SharedDraw3PArc.Start = HDraw3PArc.Start.Clone3D();
                CoordinateDrawList.SharedDraw3PArc.Middle = HDraw3PArc.Middle.Clone3D();
                CoordinateDrawList.SharedDraw3PArc.Color = CoordinateDrawList.Color3PArc;
                if (CoordinateDrawList.SharedDraw3PArc.LineWidth <= 0)
                {
                    CoordinateDrawList.SharedDraw3PArc.LineWidth = HDraw3PArc.LineWidth;
                }
                CoordinateDrawList.SharedDraw3PArc.GuidCode = Guid.NewGuid().ToString("N");
                CoordinateDrawList.Shapes.Add(CoordinateDrawList.SharedDraw3PArc.Clone());
                CoordinateDrawList.DrawChangeAdd(HDrawChange.Add(CoordinateDrawList.Shapes.Count - 1, CoordinateDrawList.SharedDraw3PArc.Clone()));
            }
            else if (CoordinateDrawList.TempShape is HDrawPoint)
            {
                HDrawPoint HDrawPoint = CoordinateDrawList.TempShape as HDrawPoint;
                if (CoordinateDrawList.SharedDrawPoint == null)
                {
                    CoordinateDrawList.SharedDrawPoint = (HDrawPoint)HDrawPoint.Clone();
                }
                CoordinateDrawList.SharedDrawPoint.Layer = CoordinateDrawList.Layer;
                CoordinateDrawList.SharedDrawPoint.Center = HDrawPoint.Center.Clone3D();
                CoordinateDrawList.SharedDrawPoint.Color = CoordinateDrawList.ColorPoint;
                if (CoordinateDrawList.SharedDrawPoint.LineWidth <= 0)
                {
                    CoordinateDrawList.SharedDrawPoint.LineWidth = HDrawPoint.LineWidth;
                }
                CoordinateDrawList.SharedDrawPoint.GuidCode = Guid.NewGuid().ToString("N");
                CoordinateDrawList.Shapes.Add(CoordinateDrawList.SharedDrawPoint.Clone());
                CoordinateDrawList.DrawChangeAdd(HDrawChange.Add(CoordinateDrawList.Shapes.Count - 1, CoordinateDrawList.SharedDrawPoint.Clone()));
            }
            else if (CoordinateDrawList.TempShape is HDrawLine)
            {

                HDrawLine HDrawLine = CoordinateDrawList.TempShape as HDrawLine;
                if (CoordinateDrawList.SharedDrawLine == null)
                {
                    CoordinateDrawList.SharedDrawLine = (HDrawLine)HDrawLine.Clone();
                }
                CoordinateDrawList.SharedDrawLine.Layer = CoordinateDrawList.Layer;
                CoordinateDrawList.SharedDrawLine.End = HDrawLine.End.Clone3D();
                CoordinateDrawList.SharedDrawLine.Start = HDrawLine.Start.Clone3D();
                CoordinateDrawList.SharedDrawLine.Color = CoordinateDrawList.ColorLine;
                if (CoordinateDrawList.SharedDrawLine.LineWidth <= 0)
                {
                    CoordinateDrawList.SharedDrawLine.LineWidth = HDrawLine.LineWidth;
                }
                CoordinateDrawList.SharedDrawLine.GuidCode = Guid.NewGuid().ToString("N");
                CoordinateDrawList.Shapes.Add(CoordinateDrawList.SharedDrawLine.Clone());
                CoordinateDrawList.DrawChangeAdd(HDrawChange.Add(CoordinateDrawList.Shapes.Count - 1, CoordinateDrawList.SharedDrawLine.Clone()));
            }
          
        }
        /// <summary>MovePointShapes 方法。</summary>
        public OK MovePointShapes(HPoint3D[] hPoint3D, HInt mode)
        {
            OK oK = true;
            if (!CoordinateScreen.EnableDraw)
            {
                CoordinateDrawList.SelectedShapeType = HShapeType.None;
            }
            if (CoordinateDrawList.SelectGUID.Count == 0)
            {
                return oK;
            }
            for (int i = 0; i < CoordinateDrawList.Shapes.Count; i++)
            {
                bool isSelectedshape = false;
                foreach (var item in CoordinateDrawList.SelectGUID)
                {
                    if (CoordinateDrawList.Shapes[i].GuidCode == item)
                    {
                        isSelectedshape = true;
                        break;
                    }
                }
                if (isSelectedshape)
                {
                    oK = CoordinateDrawList.Shapes[i].Move(hPoint3D, mode);
                }
            }
            return oK;
        }
        /// <summary>DeleteShapes 方法。</summary>
        public void DeleteShapes()
        {
            if (!CoordinateScreen.EnableDraw)
            {
                CoordinateDrawList.SelectedShapeType = HShapeType.None;
            }
            if (CoordinateDrawList.SelectGUID.Count == 0)
            {
                return;
            }
            List<int> indexs = new List<int>();
          

            HList<HDrawBase> HDrawBases = new HList<HDrawBase>();
         
            for (int i = CoordinateDrawList.Shapes.Count - 1; i >= 0; i--)
            {
                bool isSelectedshape = false;
                foreach (var item in CoordinateDrawList.SelectGUID)
                {
                    if (CoordinateDrawList.Shapes[i].GuidCode == item)
                    {
                        isSelectedshape = true;
                        break;
                    }
                }
                if (isSelectedshape)
                {
                    indexs.Add(i);
                    HDrawBases.Add(CoordinateDrawList.Shapes[i].Clone());
                    CoordinateDrawList.Shapes[i].Clear();
                    CoordinateDrawList.Shapes[i] = null;
                    CoordinateDrawList.Shapes.RemoveAt(i);
                }
            }
            CoordinateDrawList.DrawChangeAdd(HDrawChange.Remove(indexs.ToArray(), HDrawBases.ToArray()));
            if (CoordinateDrawList.Shapes.Count == 0)
            {
                CoordinateDrawList.EndPoint = null;
            }
            else
            {
                CoordinateDrawList.EndPoint = CoordinateDrawList.Shapes[CoordinateDrawList.Shapes.Count - 1].GetPoint(9, CoordinateScreen.ViewScale)[0];
            }
            Clear();
            SelectShape(-1);

        }
        /// <summary>ViewOffsetCenter 方法。</summary>
        public void ViewOffsetCenter(HDouble x1, HDouble y1)
        {
            HRect WorldRectangle = CoordinateScreen.WorldRectangle;
            CoordinateScreen.ViewOffset = CoordinateScreen.WorldToScreen(new HPoint(WorldRectangle.X.Value + x1 * -1 + WorldRectangle.Width / 2, (WorldRectangle.Y.Value + WorldRectangle.Height) + y1 - WorldRectangle.Height / 2));
        }


    }
}
