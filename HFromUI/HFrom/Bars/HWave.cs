using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Bars
{
    public partial class HWave : UserControl
    {
        #region 定义

        /// <summary>timer 字段。</summary>
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private int waveSpeed = 10;//波浪的速度，数值越大越慢，即timer的延时
        private int waveWidth = 100;//波浪宽度
        private int waveHeight = 20;//波浪高度
        private int waveTop = 1;//波浪垂直位置
        private int waveLeft = -100;//波浪水平位置
        private int radius = 0;//圆角直径
        /// <summary>waveColor 字段。</summary>
        private Color waveColor = Color.DodgerBlue;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("波浪速度"), HDescriptionLanguage("波浪速度,越大越慢"), Browsable(true)]
        public int WaveSpeed
        {
            get { return waveSpeed; }
            set
            {
                if (value <= 0)
                    return;
                waveSpeed = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("波浪宽度"), HDescriptionLanguage("波浪宽度"), Browsable(true)]
        public int WaveWidth
        {
            get { return waveWidth; }
            set
            {
                if (value <= 5)
                    return;
                waveWidth = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("波浪高度"), HDescriptionLanguage("波浪高度"), Browsable(true)]
        public int WaveHeight
        {
            get { return waveHeight; }
            set
            {
                if (value <= 0)
                    return;
                waveHeight = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("波浪垂直位置"), HDescriptionLanguage("波浪垂直位置"), Browsable(true)]
        public int WaveTop
        {
            get { return waveTop; }
            set
            {
                if (value < waveHeight * -1)
                    return;
                waveTop = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("波浪颜色"), HDescriptionLanguage("波浪颜色"), Browsable(true)]
        public Color WaveColor
        {
            get { return waveColor; }
            set
            {
                waveColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆角直径"), HDescriptionLanguage("圆角直径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set
            {
                if (value < 0)
                    return;
                radius = value;
                this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));
            }
        }

        public event PaintEventHandler Painted;

        #endregion 定义

        public HWave()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            timer.Interval = waveSpeed;
            timer.Tick += new EventHandler(this.Timer_Ticked);
        }

        /// <summary>PPWave_Load 方法。</summary>
        private void PPWave_Load(object sender, EventArgs e)
        {
            this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));
        }

        /// <summary>Timer_Ticked 方法。</summary>
        private void Timer_Ticked(object sender, EventArgs e)
        {
            waveLeft -= 3;
            if (waveLeft == -2 * waveWidth)
                waveLeft = -1 * waveWidth;
            this.Invalidate();
        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            timer.Enabled = Visible;
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            List<Point> lst1 = new List<Point>();
            List<Point> lst2 = new List<Point>();
            int m_intX = waveLeft;
            while (true)
            {
                lst1.Add(new Point(m_intX, waveTop));
                lst1.Add(new Point(m_intX + waveWidth / 2, waveHeight + waveTop));

                lst2.Add(new Point(m_intX + waveWidth / 2, waveTop));
                lst2.Add(new Point(m_intX + waveWidth / 2 + waveWidth / 2, waveHeight + waveTop));
                m_intX += waveWidth;
                if (m_intX > this.Width + waveWidth)
                    break;
            }

            GraphicsPath path1 = new GraphicsPath();
            path1.AddCurve(lst1.ToArray(), 0.5F);
            path1.AddLine(this.Width + 1, -1, this.Width + 1, this.Height);
            path1.AddLine(this.Width + 1, this.Height, -1, this.Height);
            path1.AddLine(-1, this.Height, -1, -1);

            GraphicsPath path2 = new GraphicsPath();
            path2.AddCurve(lst2.ToArray(), 0.5F);
            path2.AddLine(this.Width + 1, -1, this.Width + 1, this.Height);
            path2.AddLine(this.Width + 1, this.Height, -1, this.Height);
            path2.AddLine(-1, this.Height, -1, -1);

            Brush brush1 = new SolidBrush(Color.FromArgb(220, waveColor.R, waveColor.G, waveColor.B));
            Brush brush2 = new SolidBrush(Color.FromArgb(220, waveColor.R, waveColor.G, waveColor.B));
            Pen pen = new Pen(this.BackColor);

            g.FillPath(brush1, path1);
            g.FillPath(brush2, path2);
            g.DrawPath(pen, HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));

            brush1.Dispose();
            brush2.Dispose();
            pen.Dispose();
            path1.Dispose();
            path2.Dispose();

            if (Painted != null)
            {
                Painted(this, e);
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));
        }
    }
}