using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HFromUI.HFrom.ScreenShot
{
    using HFromUI.HLangage;
    public partial class HScreenShotForm : Form
    {
        private Bitmap screenBmp;
        /// <summary>toolTip 字段。</summary>
        private ToolTip toolTip=new ToolTip();

        /// <summary>SecectRectColor 成员。</summary>
        public Color SecectRectColor { get; set; } = Color.Red;

        private bool isShot = false;//已划定截图范围
        private bool isMouseDown = false;//截图画范围时鼠标按下
        /// <summary>mouseDownPoint 字段。</summary>
        private Point mouseDownPoint = new Point(0, 0);
        /// <summary>mouseMovePoint 字段。</summary>
        private Point mouseMovePoint = new Point(0, 0);

        /// <summary>
        /// 光标状态
        /// </summary>
        private enum EnumMousePointPosition
        {
            /// <summary>
            /// 无
            /// </summary>
            MouseSizeNone = 0, //'无

            /// <summary>
            /// 拉伸右边框
            /// </summary>
            MouseSizeRight = 1, //'拉伸右边框

            /// <summary>
            /// 拉伸左边框
            /// </summary>
            MouseSizeLeft = 2, //'拉伸左边框

            /// <summary>
            /// 拉伸下边框
            /// </summary>
            MouseSizeBottom = 3, //'拉伸下边框

            /// <summary>
            /// 拉伸上边框
            /// </summary>
            MouseSizeTop = 4, //'拉伸上边框

            /// <summary>
            /// 拉伸左上角
            /// </summary>
            MouseSizeTopLeft = 5, //'拉伸左上角

            /// <summary>
            /// 拉伸右上角
            /// </summary>
            MouseSizeTopRight = 6, //'拉伸右上角

            /// <summary>
            /// 拉伸左下角
            /// </summary>
            MouseSizeBottomLeft = 7, //'拉伸左下角

            /// <summary>
            /// 拉伸右下角
            /// </summary>
            MouseSizeBottomRight = 8, //'拉伸右下角

            /// <summary>
            /// 鼠标拖动
            /// </summary>
            MouseDrag = 9   // '鼠标拖动
        }

        public HScreenShotForm()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.UpdateStyles();

            Screen screen = Screen.FromPoint(MousePosition);
            Bitmap bitmap = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.CopyFromScreen(screen.Bounds.Location, new Point(0, 0), screen.Bounds.Size);
            screenBmp = bitmap;
            this.Bounds = screen.Bounds;

            toolTip.SetToolTip(Button_COPY, HTranslation.GetContent("复制"));
            toolTip.SetToolTip(Button_SAVE, HTranslation.GetContent("保存"));
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        /// <summary>PPScreenShotForm_Load 方法。</summary>
        private void PPScreenShotForm_Load(object sender, EventArgs e)
        {
        }

        /// <summary>PPScreenShotForm_MouseClick 方法。</summary>
        private void PPScreenShotForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (mouseDownPoint != new Point(0, 0) || mouseMovePoint != new Point(0, 0))
                {
                    mouseDownPoint = new Point(0, 0);
                    mouseMovePoint = new Point(0, 0);
                    isShot = false;
                    Panel_control.Visible = false;
                    this.Invalidate();
                }
                else
                {
                    this.Close();
                }
            }
        }

        /// <summary>PPScreenShotForm_Paint 方法。</summary>
        private void PPScreenShotForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (screenBmp != null)
            {
                g.DrawImage(screenBmp, 0, 0);
                using (var brush= new SolidBrush(Color.FromArgb(100, 50, 50, 50)))//遮罩层
                {
                    g.FillRectangle(brush, new Rectangle(0, 0, screenBmp.Width, screenBmp.Height));
                }
                    

                Rectangle selectRect = new Rectangle(mouseDownPoint, new Size(mouseMovePoint.X - mouseDownPoint.X, mouseMovePoint.Y - mouseDownPoint.Y));

                if (mouseMovePoint.X < mouseDownPoint.X)
                {
                    selectRect.X = mouseMovePoint.X;
                    selectRect.Width = mouseDownPoint.X - mouseMovePoint.X;
                }

                if (mouseMovePoint.Y < mouseDownPoint.Y)
                {
                    selectRect.Y = mouseMovePoint.Y;
                    selectRect.Height = mouseDownPoint.Y - mouseMovePoint.Y;
                }

                if (selectRect.Width > 0 && selectRect.Height > 0)
                {
                    using (Bitmap selectBmp = screenBmp.Clone(selectRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        g.DrawImage(selectBmp, selectRect);
                        using (Pen pen= new Pen(SecectRectColor, 1f))
                        {
                            g.DrawRectangle(pen, selectRect);

                            if(isShot)
                            {
                                using (SolidBrush brush = new SolidBrush(SecectRectColor))
                                {
                                    var rect = new Rectangle(selectRect.X - blend, selectRect.Y - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.X - blend, selectRect.Y + selectRect.Height / 2 - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.X - blend, selectRect.Bottom - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.X + selectRect.Width / 2 - blend, selectRect.Y + -blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.X + selectRect.Width / 2 - blend, selectRect.Bottom - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.Right - blend, selectRect.Y - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.Right - blend, selectRect.Y + selectRect.Height / 2 - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                    rect = new Rectangle(selectRect.Right - blend, selectRect.Bottom - blend, 2 * blend, 2 * blend);
                                    g.FillRectangle(brush, rect);
                                }
                            }

                        }
                            
                    }
                        
                }

                //画鼠标区域放大
                if (!isShot)
                {
                    Point mousePoint = this.PointToClient(MousePosition);
                    int zoomWidth = 100;
                    int infoWidth = 120;
                    Bitmap zoomInBmp = new Bitmap(20, 20);
                    Rectangle zoomInRect = new Rectangle(mousePoint.X - 10, mousePoint.Y - 10, 20, 20);
                    Rectangle zoomRect = new Rectangle(mousePoint.X + 2, mousePoint.Y - zoomWidth, zoomWidth, zoomWidth);
                    Rectangle infoRect = new Rectangle(zoomRect.Right, zoomRect.Top, infoWidth, zoomRect.Height);
                    Color pointcolor = screenBmp.GetPixel(mousePoint.X,mousePoint.Y);
                    
                    //左边缘
                    if (zoomInRect.X < 0 && zoomInRect.X >= -10 && zoomInRect.Y >= 0 && zoomInRect.Bottom <= screenBmp.Height)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(0, 0, -1 * zoomInRect.X, 20);//填充黑色矩形
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(0, zoomInRect.Y, 20 - (-1 * zoomInRect.X), 20), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(-1 * zoomInRect.X, 0, 20 - (-1 * zoomInRect.X), 20);
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //上边缘
                    else if (zoomInRect.X >= 0 && zoomInRect.Y < 0 && zoomInRect.Y >= -10 && zoomInRect.Right <= screenBmp.Width)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(0, 0, 20, -1 * zoomInRect.Y);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(zoomInRect.X, 0, 20, 20 - (-1 * zoomInRect.Y)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(0, -1 * zoomInRect.Y, 20, 20 - (-1 * zoomInRect.Y));
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //左上角
                    else if (zoomInRect.X < 0 && zoomInRect.Y < 0 && zoomInRect.X >= -10 && zoomInRect.Y >= -10)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(0, 0, -1 * zoomInRect.X, 20);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        fillrect = new Rectangle(0, 0, 20, -1 * zoomInRect.Y);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(0, 0, 20 - (-1 * zoomInRect.X), 20 - (-1 * zoomInRect.Y)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(-1 * zoomInRect.X, -1 * zoomInRect.Y, 20 - (-1 * zoomInRect.X), 20 - (-1 * zoomInRect.Y));
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //右边缘
                    else if (zoomInRect.Right > screenBmp.Width && zoomInRect.Right <= screenBmp.Width + 10 && zoomInRect.Bottom <= screenBmp.Height && zoomInRect.Y >= 0)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(20 - (zoomInRect.Right - screenBmp.Width), 0, (zoomInRect.Right - screenBmp.Width), 20);
                        bg.FillRectangle(Brushes.Black, fillrect);

                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(screenBmp.Width - (20 - (zoomInRect.Right - screenBmp.Width)), zoomInRect.Y, 20 - (zoomInRect.Right - screenBmp.Width), 20), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(0, 0, 20 - (zoomInRect.Right - screenBmp.Width), 20);
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //下边缘
                    else if (zoomInRect.Bottom > screenBmp.Height && zoomInRect.Bottom <= screenBmp.Height + 10 && zoomInRect.Right <= screenBmp.Width && zoomInRect.X >= 0)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(0, 20 - (zoomInRect.Bottom - screenBmp.Height), 20, zoomInRect.Bottom - screenBmp.Height);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(zoomInRect.X, screenBmp.Height - (20 - (zoomInRect.Bottom - screenBmp.Height)), 20, 20 - (zoomInRect.Bottom - screenBmp.Height)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(0, 0, 20, 20 - (zoomInRect.Bottom - screenBmp.Height));
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //右下角
                    else if (zoomInRect.Right > screenBmp.Width && zoomInRect.Right <= screenBmp.Width + 10 && zoomInRect.Bottom > screenBmp.Height && zoomInRect.Bottom <= screenBmp.Height + 10)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(20 - (zoomInRect.Right - screenBmp.Width), 0, (zoomInRect.Right - screenBmp.Width), 20);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        fillrect = new Rectangle(0, 20 - (zoomInRect.Bottom - screenBmp.Height), 20, zoomInRect.Bottom - screenBmp.Height);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(screenBmp.Width - (20 - (zoomInRect.Right - screenBmp.Width)), screenBmp.Height - (20 - (zoomInRect.Bottom - screenBmp.Height)), 20 - (zoomInRect.Right - screenBmp.Width), 20 - (zoomInRect.Bottom - screenBmp.Height)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(0, 0, 20 - (zoomInRect.Right - screenBmp.Width), 20 - (zoomInRect.Bottom - screenBmp.Height));
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //左下角
                    else if (zoomInRect.X < 0 && zoomInRect.X >= -10 && zoomInRect.Bottom > screenBmp.Height && zoomInRect.Bottom <= screenBmp.Height + 10)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(0, 0, -1 * zoomInRect.X, 20);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        fillrect = new Rectangle(0, 20 - (zoomInRect.Bottom - screenBmp.Height), 20, zoomInRect.Bottom - screenBmp.Height);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(0, screenBmp.Height - (20 - (zoomInRect.Bottom - screenBmp.Height)), 20 - (-1 * zoomInRect.X), 20 - (zoomInRect.Bottom - screenBmp.Height)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(-1 * zoomInRect.X, 0, 20 - (-1 * zoomInRect.X), 20 - (zoomInRect.Bottom - screenBmp.Height));
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    //右上角
                    else if (zoomInRect.Y < 0 && zoomInRect.Y >= -10 && zoomInRect.Right > screenBmp.Width && zoomInRect.Right <= screenBmp.Width + 10)
                    {
                        zoomInBmp = new Bitmap(20, 20);
                        Graphics bg = Graphics.FromImage(zoomInBmp);
                        Rectangle fillrect = new Rectangle(0, 0, 20, -1 * zoomInRect.Y);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        fillrect = new Rectangle(20 - (zoomInRect.Right - screenBmp.Width), 0, zoomInRect.Right - screenBmp.Width, 20);
                        bg.FillRectangle(Brushes.Black, fillrect);
                        Bitmap fillbmp = screenBmp.Clone(new Rectangle(screenBmp.Width - (20 - (zoomInRect.Right - screenBmp.Width)), 0, 20 - (zoomInRect.Right - screenBmp.Width), 20 - (-1 * zoomInRect.Y)), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        Rectangle fillbmpRect = new Rectangle(0, -1 * zoomInRect.Y, 20 - (zoomInRect.Right - screenBmp.Width), 20 - (-1 * zoomInRect.Y));
                        bg.DrawImage(fillbmp, fillbmpRect);
                        fillbmp.Dispose();
                        bg.Dispose();
                    }
                    else if (zoomInRect.X >= 0 && zoomInRect.Y >= 0 && zoomInRect.Right <= screenBmp.Width && zoomInRect.Bottom <= screenBmp.Height)
                    {
                        zoomInBmp = screenBmp.Clone(zoomInRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }

                    if (mousePoint.Y <= zoomWidth && mousePoint.X < screenBmp.Width - zoomWidth-infoWidth)
                    {
                        zoomRect = new Rectangle(mousePoint.X + 2, mousePoint.Y, zoomWidth, zoomWidth);
                        infoRect= new Rectangle(zoomRect.Right, zoomRect.Top, infoWidth, zoomRect.Height);
                    }
                    else if (mousePoint.X >= screenBmp.Width - zoomWidth-infoWidth && mousePoint.Y > zoomWidth)
                    {
                        zoomRect = new Rectangle(mousePoint.X - zoomWidth-2, mousePoint.Y - zoomWidth, zoomWidth, zoomWidth);
                        infoRect = new Rectangle(zoomRect.Left-infoWidth, zoomRect.Top, infoWidth, zoomRect.Height);
                    }
                    else if (mousePoint.X >= screenBmp.Width - zoomWidth-infoWidth && mousePoint.Y <= zoomWidth)
                    {
                        zoomRect = new Rectangle(mousePoint.X - zoomWidth-2, mousePoint.Y, zoomWidth, zoomWidth);
                        infoRect = new Rectangle(zoomRect.Left-infoWidth, zoomRect.Top, infoWidth, zoomRect.Height);
                    }

                    Pen pen = new Pen(Color.Black, 1);
                    g.DrawImage(Magnifier(zoomInBmp, 5), zoomRect);
                    g.DrawRectangle(pen, zoomRect);
                    g.DrawLine(pen, zoomRect.Left, zoomRect.Top + zoomRect.Height / 2, zoomRect.Right, zoomRect.Top + zoomRect.Height / 2);
                    g.DrawLine(pen, zoomRect.Left + zoomRect.Width / 2, zoomRect.Top, zoomRect.Left + zoomRect.Width / 2, zoomRect.Bottom);

                    using (var brush= new SolidBrush(Color.FromArgb(100, 55, 55, 55)))
                    {
                        g.FillRectangle(brush, infoRect);
                        g.DrawRectangle(pen, infoRect);
                    }

                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    g.DrawString($"X:{mousePoint.X},Y:{mousePoint.Y}", this.Font, Brushes.White, new Point(infoRect.Left + 5, infoRect.Top + 20));
                    g.DrawString($"R:{pointcolor.R},G:{pointcolor.G},B:{pointcolor.B}", this.Font, Brushes.White, new Point(infoRect.Left + 5, infoRect.Top + infoRect.Height / 2 + 20));


                    pen.Dispose();
                    zoomInBmp.Dispose();
                }
            }
        }

        Point p=new Point(0,0);
        EnumMousePointPosition mousePosition;
        int blend = 3;
        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                if (!isShot)
                {
                    isMouseDown = true;
                    mouseDownPoint = e.Location;
                }
                else
                {
                    p.X = e.X;
                    p.Y=e.Y;
                }
            }
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!isShot)
            {
                this.Cursor = Cursors.Arrow;        //'箭头
                if (isMouseDown)
                {
                    if (e.X > this.Width || e.Y > this.Height)
                    {
                        Point p = e.Location;
                        if (e.X > this.Width)
                        {
                            p.X = this.Width;
                        }

                        if (e.Y > this.Height)
                        {
                            p.Y = this.Height;
                        }

                        mouseMovePoint = p;
                    }
                    else
                    {
                        mouseMovePoint = e.Location;
                    }
                }
                this.Invalidate();
            }
            else
            {
                
                if (e.Button==MouseButtons.Left)
                {
                    Rectangle selectRect = new Rectangle(mouseDownPoint, new Size(mouseMovePoint.X - mouseDownPoint.X, mouseMovePoint.Y - mouseDownPoint.Y));

                    if (mouseMovePoint.X < mouseDownPoint.X)
                    {
                        selectRect.X = mouseMovePoint.X;
                        selectRect.Width = mouseDownPoint.X - mouseMovePoint.X;
                    }

                    if (mouseMovePoint.Y < mouseDownPoint.Y)
                    {
                        selectRect.Y = mouseMovePoint.Y;
                        selectRect.Height = mouseDownPoint.Y - mouseMovePoint.Y;
                    }


                    switch (mousePosition)
                    {
                        case EnumMousePointPosition.MouseDrag:

                            if(selectRect.Left + e.X - p.X < 0 || (selectRect.Top + e.Y - p.Y) < 0 || selectRect.Left + e.X - p.X + selectRect.Width > this.Width || selectRect.Top + e.Y - p.Y + selectRect.Height > this.Height)
                            {
                                CheckBorder(selectRect,e);
                            }
                            else
                            {
                                selectRect.X = selectRect.Left + e.X - p.X;
                                selectRect.Y=  selectRect.Top + e.Y - p.Y;
                            }
                            p.X = e.X;
                            p.Y = e.Y;
                            break;

                        case EnumMousePointPosition.MouseSizeBottom:
                            if (selectRect.Top + selectRect.Height + e.Y - p.Y > this.Height)
                            {
                                selectRect.Height = this.Height - selectRect.Top;
                            }
                            else
                            {
                                selectRect.Height = selectRect.Height + e.Y - p.Y;
                            }
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeBottomRight:
                            if (selectRect.Top + selectRect.Height + e.Y - p.Y > this.Height || selectRect.Left + selectRect.Width + e.X - p.X > this.Width)
                            {
                                if (selectRect.Top + selectRect.Height + e.Y - p.Y > this.Height)
                                {
                                    selectRect.Height = this.Height - selectRect.Top;
                                }
                                else
                                {
                                    selectRect.Height = selectRect.Height + e.Y - p.Y;
                                }

                                if (selectRect.Left + selectRect.Width + e.X - p.X > this.Width)
                                {
                                    selectRect.Width = this.Width - selectRect.Left;
                                }
                                else
                                {
                                    selectRect.Width = selectRect.Width + e.X - p.X;
                                }
                            }
                            else
                            {
                                selectRect.Width = selectRect.Width + e.X - p.X;
                                selectRect.Height = selectRect.Height + e.Y - p.Y;
                            }

                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeRight:

                            if (selectRect.Left + selectRect.Width + e.X - p.X > this.Width)
                            {
                                selectRect.Width = this.Width - selectRect.Left;
                            }
                            else
                            {
                                selectRect.Width = selectRect.Width + e.X - p.X;
                            }

                            // selectRect.Height = selectRect.Height + e.Y - p1.Y;
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeTop:
                            if (selectRect.Top + e.Y - p.Y < 0)
                            {
                                selectRect.Y = 0;
                            }
                            else
                            {

                                selectRect.Y = selectRect.Top + (e.Y - p.Y);
                                selectRect.Height = selectRect.Height - (e.Y - p.Y);
                                
                            }
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeLeft:
                            if (selectRect.Left + e.X - p.X < 0)
                            {
                                selectRect.X = 0;
                            }
                            else
                            {

                                selectRect.X = selectRect.Left + e.X - p.X;
                                selectRect.Width = selectRect.Width - (e.X - p.X);

                            }
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            if (selectRect.Left + e.X - p.X < 0 || selectRect.Top + selectRect.Height + e.Y - p.Y > this.Height)
                            {
                                if (selectRect.Left + e.X - p.X < 0)
                                {
                                    selectRect.X = 0;
                                }
                                else
                                {
                                    selectRect.X = selectRect.Left + e.X - p.X;
                                    selectRect.Width = selectRect.Width - (e.X - p.X);
                                }

                                if (selectRect.Top + selectRect.Height + e.Y - p.Y > this.Height)
                                {
                                    selectRect.Height = this.Height - selectRect.Top;
                                }
                                else
                                {
                                    selectRect.Height = selectRect.Height + e.Y - p.Y;
                                }
                            }
                            else
                            {
                                selectRect.X = selectRect.Left + e.X - p.X;
                                selectRect.Width = selectRect.Width - (e.X - p.X);
                                selectRect.Height = selectRect.Height + e.Y - p.Y;
                            }
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeTopRight:
                            if (selectRect.Top + e.Y - p.Y < 0 || selectRect.Left + selectRect.Width + e.X - p.X > this.Width)
                            {
                                if (selectRect.Top + e.Y - p.Y < 0)
                                {
                                    selectRect.Y = 0;
                                }
                                else
                                {
                                    selectRect.Y = selectRect.Top + (e.Y - p.Y);
                                    selectRect.Height = selectRect.Height - (e.Y - p.Y);
                                }

                                if (selectRect.Left + selectRect.Width + e.X - p.X > this.Width)
                                {
                                    selectRect.Width = this.Width - selectRect.Left;
                                }
                                else
                                {
                                    selectRect.Width = selectRect.Width + e.X - p.X;
                                }
                            }
                            else
                            {
                                selectRect.Y = selectRect.Top + (e.Y - p.Y);
                                selectRect.Width = selectRect.Width + (e.X - p.X);
                                selectRect.Height = selectRect.Height - (e.Y - p.Y);
                            }
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        case EnumMousePointPosition.MouseSizeTopLeft:
                            if (selectRect.Top + e.Y - p.Y < 0 || selectRect.Left + e.X - p.X < 0)
                            {
                                if (selectRect.Top + e.Y - p.Y < 0)
                                {
                                    selectRect.Y = 0;
                                }
                                else
                                {
                                    selectRect.Y = selectRect.Top + (e.Y - p.Y);
                                    selectRect.Height = selectRect.Height - (e.Y - p.Y);
                                }
                                if (selectRect.Left + e.X - p.X < 0)
                                {
                                    selectRect.X = 0;
                                }
                                else
                                {
                                    selectRect.X = selectRect.Left + e.X - p.X;
                                    selectRect.Width = selectRect.Width - (e.X - p.X);
                                }
                            }
                            else
                            {
                                selectRect.X = selectRect.Left + e.X - p.X;
                                selectRect.Y = selectRect.Top + (e.Y - p.Y);
                                selectRect.Width = selectRect.Width - (e.X - p.X);
                                selectRect.Height = selectRect.Height - (e.Y - p.Y);
                            }
                            p.X = e.X;
                            p.Y = e.Y; //'记录光标拖动的当前点
                            break;

                        default:
                            break;
                    }
                    mouseDownPoint = selectRect.Location;

                    mouseMovePoint = new Point(selectRect.Right, selectRect.Bottom);

                    if (mouseMovePoint.Y + Panel_control.Height > this.Height)
                    {
                        Panel_control.Location = new Point(Panel_control.Location.X, mouseMovePoint.Y - Panel_control.Height - 2);
                    }
                    else
                    {
                        Panel_control.Location = new Point(mouseMovePoint.X - Panel_control.Width, mouseMovePoint.Y + 2);
                    }
                    this.Invalidate();

                }
                else
                {
                    mousePosition = MousePointPosition(e.Location);
                    switch (mousePosition)   //'改变光标
                    {
                        case EnumMousePointPosition.MouseSizeNone:
                            this.Cursor = Cursors.Arrow;        //'箭头
                            break;

                        case EnumMousePointPosition.MouseDrag:
                            this.Cursor = Cursors.SizeAll;      //'四方向
                            break;

                        case EnumMousePointPosition.MouseSizeBottom:
                            this.Cursor = Cursors.SizeNS;       //'南北
                            break;

                        case EnumMousePointPosition.MouseSizeTop:
                            this.Cursor = Cursors.SizeNS;       //'南北
                            break;

                        case EnumMousePointPosition.MouseSizeLeft:
                            this.Cursor = Cursors.SizeWE;       //'东西
                            break;

                        case EnumMousePointPosition.MouseSizeRight:
                            this.Cursor = Cursors.SizeWE;       //'东西
                            break;

                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            this.Cursor = Cursors.SizeNESW;     //'东北到南西
                            break;

                        case EnumMousePointPosition.MouseSizeBottomRight:
                            this.Cursor = Cursors.SizeNWSE;     //'东南到西北
                            break;

                        case EnumMousePointPosition.MouseSizeTopLeft:
                            this.Cursor = Cursors.SizeNWSE;     //'东南到西北
                            break;

                        case EnumMousePointPosition.MouseSizeTopRight:
                            this.Cursor = Cursors.SizeNESW;     //'东北到南西
                            break;

                        default:
                            break;
                    }
                }
                

            }
        }

        /// <summary>CheckBorder 方法。</summary>
        private void CheckBorder(Rectangle rectangle, System.Windows.Forms.MouseEventArgs e)
        {
            //lCtrl.Left = (int)(XToRatio(lCtrl.Left) * base.Width / 100);
            //lCtrl.Top = (int)(YToRatio(lCtrl.Top) * base.Height / 100);
            // lCtrl.Width = lCtrl.Left + (int)(XToRatio(lCtrl.Width) * base.Width / resolutionX) > base.Width ? (int)((XToRatio(lCtrl.Width) - 1) * base.Width / resolutionX) : (int)(XToRatio(lCtrl.Width) * base.Width / resolutionX);
            // lCtrl.Height = lCtrl.Top + (int)(YToRatio(lCtrl.Height) * base.Height / resolutionY) > base.Height ? (int)((YToRatio(lCtrl.Height) - 1) * base.Height / resolutionY) : (int)(YToRatio(lCtrl.Height) * base.Height / resolutionY);
            if (rectangle.Left + e.X - p.X < 0)
            {
                rectangle.X = 0;
                if ((rectangle.Top + e.Y - p.Y) < 0)
                {
                    rectangle.Y = 0;
                }
                else if ((rectangle.Top + e.Y - p.Y + rectangle.Height) > base.Height)
                {
                    rectangle.Y = this.Height - rectangle.Height;
                }
                else
                {
                    rectangle.Y = rectangle.Top + e.Y - p.Y;
                }
            }
            else if (rectangle.Left + e.X - p.X + rectangle.Width > base.Width)
            {
                rectangle.X = base.Width - rectangle.Width;
                if ((rectangle.Top + e.Y - p.Y) < 0)
                {
                    rectangle.Y = 0;
                }
                else if ((rectangle.Top + e.Y - p.Y + rectangle.Height) > base.Height)
                {
                    rectangle.Y = base.Height - rectangle.Height;
                }
                else
                {
                    rectangle.Y = rectangle.Top + e.Y - p.Y;
                }
            }

            if (rectangle.Top + e.Y - p.Y < 0)
            {
                rectangle.Y = 0;
                if (rectangle.Left + e.X - p.X < 0)
                {
                    rectangle.X = 0;
                }
                else if (rectangle.Left + e.X - p.X + rectangle.Width > base.Width)
                {
                    rectangle.X = base.Width - rectangle.Width;
                }
                else
                {
                    rectangle.X = rectangle.Left + e.X - p.X;
                }
            }
            else if (rectangle.Top + e.Y - p.Y + rectangle.Height > base.Height)
            {
                rectangle.Y = base.Height - rectangle.Height;
                if (rectangle.Left + e.X - p.X < 0)
                {
                    rectangle.X = 0;
                }
                else if (rectangle.Left + e.X - p.X + rectangle.Width > base.Width)
                {
                    rectangle.X = base.Width - rectangle.Width;
                }
                else
                {
                    rectangle.X = rectangle.Left + e.X - p.X;
                }
            }
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isMouseDown)
            {
                if (e.X > this.Width || e.Y > this.Height)
                {
                    Point p = e.Location;
                    if (e.X > this.Width)
                    {
                        p.X = this.Width;
                    }

                    if (e.Y > this.Height)
                    {
                        p.Y = this.Height;
                    }

                    mouseMovePoint = p;
                }
                else
                {
                    mouseMovePoint = e.Location;
                }


                Rectangle selectRect = new Rectangle(mouseDownPoint, new Size(mouseMovePoint.X - mouseDownPoint.X, mouseMovePoint.Y - mouseDownPoint.Y));

                if (mouseMovePoint.X < mouseDownPoint.X)
                {
                    selectRect.X = mouseMovePoint.X;
                    selectRect.Width = mouseDownPoint.X - mouseMovePoint.X;
                }

                if (mouseMovePoint.Y < mouseDownPoint.Y)
                {
                    selectRect.Y = mouseMovePoint.Y;
                    selectRect.Height = mouseDownPoint.Y - mouseMovePoint.Y;
                }

                mouseDownPoint = selectRect.Location;

                mouseMovePoint=new Point(selectRect.Right, selectRect.Bottom);


                isMouseDown = false;

                isShot = true;

                if (mouseMovePoint.Y + Panel_control.Height > this.Height)
                {
                    Panel_control.Location = new Point(Panel_control.Location.X, mouseMovePoint.Y - Panel_control.Height - 2);
                }
                else
                {
                    Panel_control.Location = new Point(mouseMovePoint.X - Panel_control.Width, mouseMovePoint.Y + 2);
                }
                Panel_control.Visible = true;

                this.Invalidate();
            }
        }

        /// <summary>响应 KeyDown 事件。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if(e.KeyCode==Keys.Escape)
            {
                if (mouseDownPoint != new Point(0, 0) || mouseMovePoint != new Point(0, 0))
                {
                    mouseDownPoint = new Point(0, 0);
                    mouseMovePoint = new Point(0, 0);
                    isShot = false;
                    Panel_control.Visible = false;
                    this.Invalidate();
                }
                else
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// 获取截图后鼠标位置在截图拉伸的位置
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private EnumMousePointPosition MousePointPosition(Point p)
        {
            if(isShot)
            {
                Rectangle selectRect = new Rectangle(mouseDownPoint, new Size(mouseMovePoint.X - mouseDownPoint.X, mouseMovePoint.Y - mouseDownPoint.Y));

                if (mouseMovePoint.X < mouseDownPoint.X)
                {
                    selectRect.X = mouseMovePoint.X;
                    selectRect.Width = mouseDownPoint.X - mouseMovePoint.X;
                }

                if (mouseMovePoint.Y < mouseDownPoint.Y)
                {
                    selectRect.Y = mouseMovePoint.Y;
                    selectRect.Height = mouseDownPoint.Y - mouseMovePoint.Y;
                }

                if (selectRect.Width > 0 && selectRect.Height > 0)
                {
                    #region 
                    //if (p.X<selectRect.X&&p.X>selectRect.X-blend)
                    //{
                    //    if(p.Y<selectRect.Y&&p.Y>selectRect.Y-blend)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeTopLeft;
                    //    }
                    //    else if(p.Y > selectRect.Bottom && p.Y < selectRect.Bottom + blend)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeBottomLeft;
                    //    }
                    //    else if(p.Y>=selectRect.Y&&p.Y<=selectRect.Bottom)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeLeft;
                    //    }
                    //}
                    //else if(p.X>=selectRect.X&&p.X<=selectRect.Right)
                    //{
                    //    if (p.Y < selectRect.Y && p.Y > selectRect.Y - blend)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeTop;
                    //    }
                    //    else if (p.Y > selectRect.Bottom && p.Y < selectRect.Bottom + blend)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeBottom;
                    //    }
                    //    else if (p.Y >= selectRect.Y && p.Y <= selectRect.Bottom)
                    //    {
                    //        return EnumMousePointPosition.MouseDrag;
                    //    }
                    //}
                    //else if(p.X>selectRect.Right&&p.X<selectRect.Right+blend)
                    //{
                    //    if (p.Y < selectRect.Y && p.Y > selectRect.Y - blend)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeTopRight;
                    //    }
                    //    else if (p.Y > selectRect.Bottom && p.Y < selectRect.Bottom + blend)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeBottomRight;
                    //    }
                    //    else if (p.Y >= selectRect.Y && p.Y <= selectRect.Bottom)
                    //    {
                    //        return EnumMousePointPosition.MouseSizeRight;
                    //    }
                    //}
                    //else
                    //{
                    //    return EnumMousePointPosition.MouseSizeNone;
                    //}
                    #endregion
                    if(p.X>selectRect.X-blend&&p.X<selectRect.Right+blend&&p.Y>selectRect.Y-blend&&p.Y<selectRect.Bottom+blend)
                    {
                        if(new Rectangle(selectRect.X-blend,selectRect.Y-blend,2*blend,2*blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeTopLeft;
                        }
                        else if(new Rectangle(selectRect.X-blend,selectRect.Y+selectRect.Height/2-blend,2*blend,2*blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeLeft;
                        }
                        else if(new Rectangle(selectRect.X - blend, selectRect.Bottom - blend, 2 * blend, 2 * blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeBottomLeft;
                        }
                        else if(new Rectangle(selectRect.X+selectRect.Width/2 - blend, selectRect.Y +  - blend, 2 * blend, 2 * blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeTop;
                        }
                        else if(new Rectangle(selectRect.X+selectRect.Width/2 - blend, selectRect.Bottom - blend, 2 * blend, 2 * blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeBottom;
                        }
                        else if(new Rectangle(selectRect.Right - blend, selectRect.Y - blend, 2 * blend, 2 * blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeTopRight;
                        }
                        else if(new Rectangle(selectRect.Right - blend, selectRect.Y+selectRect.Height/2 - blend, 2 * blend, 2 * blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeRight;
                        }
                        else if(new Rectangle(selectRect.Right - blend, selectRect.Bottom - blend, 2 * blend, 2 * blend).Contains(p))
                        {
                            return EnumMousePointPosition.MouseSizeBottomRight;
                        }
                        else if(selectRect.Contains(p))
                        {
                            return EnumMousePointPosition.MouseDrag;
                        }
                        else
                        {
                            return EnumMousePointPosition.MouseSizeNone;
                        }
                    }

                }

                
            }
            return EnumMousePointPosition.MouseSizeNone;
        }

        /// <summary>Button_COPY_Click 方法。</summary>
        private void Button_COPY_Click(object sender, EventArgs e)
        {
            Rectangle selectRect = new Rectangle(mouseDownPoint, new Size(mouseMovePoint.X - mouseDownPoint.X, mouseMovePoint.Y - mouseDownPoint.Y));
            if (mouseMovePoint.X < mouseDownPoint.X)
            {
                selectRect.X = mouseMovePoint.X;
                selectRect.Width = mouseDownPoint.X - mouseMovePoint.X;
            }

            if (mouseMovePoint.Y < mouseDownPoint.Y)
            {
                selectRect.Y = mouseMovePoint.Y;
                selectRect.Height = mouseDownPoint.Y - mouseMovePoint.Y;
            }
            Bitmap selectBmp = screenBmp.Clone(selectRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Clipboard.Clear();
            Clipboard.SetImage(selectBmp);
            this.Close();
        }

        /// <summary>Button_SAVE_Click 方法。</summary>
        private void Button_SAVE_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(*.png)|*.png|(*.jpg)|*.jpg|(*.bmp)|*.bmp";
            saveFileDialog.FileName = string.Format(HTranslation.GetContent("PP截图{0}"), DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Rectangle selectRect = new Rectangle(mouseDownPoint, new Size(mouseMovePoint.X - mouseDownPoint.X, mouseMovePoint.Y - mouseDownPoint.Y));
                if (mouseMovePoint.X < mouseDownPoint.X)
                {
                    selectRect.X = mouseMovePoint.X;
                    selectRect.Width = mouseDownPoint.X - mouseMovePoint.X;
                }

                if (mouseMovePoint.Y < mouseDownPoint.Y)
                {
                    selectRect.Y = mouseMovePoint.Y;
                    selectRect.Height = mouseDownPoint.Y - mouseMovePoint.Y;
                }
                Bitmap selectBmp = screenBmp.Clone(selectRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                selectBmp.Save(saveFileDialog.FileName);
                this.Close();
            }
        }

        /// <summary>Magnifier 方法。</summary>
        public Bitmap Magnifier(Bitmap srcbitmap, int multiple)
        {
            if (multiple <= 0) { multiple = 0; return srcbitmap; }
            Bitmap bitmap = new Bitmap(srcbitmap.Size.Width * multiple, srcbitmap.Size.Height * multiple);
            BitmapData srcbitmapdata = srcbitmap.LockBits(new Rectangle(new Point(0, 0), srcbitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bitmapdata = bitmap.LockBits(new Rectangle(new Point(0, 0), bitmap.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* srcbyte = (byte*)(srcbitmapdata.Scan0.ToPointer());
                byte* sourcebyte = (byte*)(bitmapdata.Scan0.ToPointer());
                for (int y = 0; y < bitmapdata.Height; y++)
                {
                    for (int x = 0; x < bitmapdata.Width; x++)
                    {
                        long index = (x / multiple) * 4 + (y / multiple) * srcbitmapdata.Stride;
                        sourcebyte[0] = srcbyte[index];
                        sourcebyte[1] = srcbyte[index + 1];
                        sourcebyte[2] = srcbyte[index + 2];
                        sourcebyte[3] = srcbyte[index + 3];
                        sourcebyte += 4;
                    }
                }
            }
            srcbitmap.UnlockBits(srcbitmapdata);
            bitmap.UnlockBits(bitmapdata);
            return bitmap;
        }
    }


    public static class PPScreenShot
    {
        /// <summary>ShowScreenShot 方法。</summary>
        public static void ShowScreenShot(int argb = -65536)
        {
            var color=Color.FromArgb(argb);
            HScreenShotForm pPScreenShotForm = new HScreenShotForm() { SecectRectColor=color};
            pPScreenShotForm.Show();
        }
    }
}
