using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Bars
{
    [DefaultEvent("ValueChanged")]
    public partial class HWaveProgressBar : HBarBase
    {
        /// <summary>timer 字段。</summary>
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        #region 定义变量

        private int waveSpeed = 10;//波浪的速度，数值越大越慢，即timer的延时
        private int waveWidth = 100;//波浪宽度
        private int waveHeight = 20;//波浪高度
        private int waveTop = 1;//波浪垂直位置
        private int waveLeft = -100;//波浪水平位置
        private int radius = 0;//圆角直径
        /// <summary>waveColor 字段。</summary>
        private Color waveColor = Color.DodgerBlue;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.White;
        /// <summary>valueType 字段。</summary>
        private HValueType valueType = HValueType.Percent;

        /// <summary>值。</summary>
        private long value = 0;
        /// <summary>minimun 字段。</summary>
        private long minimun = 0;
        /// <summary>maximun 字段。</summary>
        private long maximun = 100;
        /// <summary>step 字段。</summary>
        private long step = 1;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度条值"), HDescriptionLanguage("进度条值"), Browsable(true)]
        public new long Value
        {
            get { return value; }
            set
            {
                if (this.value == value)
                    return;
                if (value > maximun)
                {
                    this.value = maximun;
                }
                else if (value < minimun)
                {
                    this.value = minimun;
                }
                else
                {
                    this.value = value;
                }
                SyncBaseState(minimun, maximun, this.value);
                OnValueChanged(EventArgs.Empty);
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度条上限"), HDescriptionLanguage("进度条上限"), Browsable(true)]
        public long Maximum
        {
            get { return maximun; }
            set
            {
                if (value < minimun)
                {
                    this.maximun = minimun;
                }
                else
                {
                    this.maximun = value;
                }
                SyncBaseState(minimun, maximun, this.value);
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度条下限"), HDescriptionLanguage("进度条下限"), Browsable(true)]
        public long Minimum
        {
            get { return minimun; }
            set
            {
                if (value > maximun)
                {
                    this.minimun = maximun;
                }
                else
                {
                    minimun = value;
                }
                SyncBaseState(minimun, maximun, this.value);
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("步进增量"), HDescriptionLanguage("步进增量"), Browsable(true)]
        public long Step
        {
            get { return step; }
            set
            {
                if (value > maximun)
                {
                    step = maximun;
                }
                step = value;
            }
        }

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

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景颜色颜色"), Browsable(true)]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆角直径"), HDescriptionLanguage("圆角直径"), Browsable(true)]
        public new int Radius
        {
            get { return radius; }
            set
            {
                if (value < 0)
                    return;
                radius = value;
                //this.Region = new Region(CreateRound(new Rectangle(0, 0, this.Width, this.Height), radius));
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文字显示类型"), HDescriptionLanguage("文字显示类型，数值和百分比"), Browsable(true)]
        public HValueType ValueType
        {
            get { return valueType; }
            set
            {
                valueType = value;
            }
        }

        public enum HValueType
        {
            Value,
            Percent,
        }

        #endregion 定义变量

        public HWaveProgressBar()
        {
            InitializeComponent();
            SyncBaseState(minimun, maximun, value);
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
            this.ValueChanged += new EventHandler(this.This_ValueChanged);
            timer.Interval = waveSpeed;
            timer.Tick += new EventHandler(this.Timer_Ticked);
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
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
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
            g.Clear(baseColor);
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
            Brush brushText = new SolidBrush(this.ForeColor);
            Brush brushBack = new SolidBrush(this.BackColor);
            Pen pen = new Pen(this.BackColor);

            g.FillPath(brush1, path1);
            g.FillPath(brush2, path2);

            double percent = (double)Math.Round((decimal)this.value / (this.maximun - this.minimun), 2);
            string text = ((int)(100 * percent)).ToString() + "%";
            if (valueType == HValueType.Value)
            {
                text = value.ToString();
            }
            SizeF size = e.Graphics.MeasureString(text, base.Font);
            g.DrawString(text, this.Font, brushText, new Point(this.Width / 2 - (int)size.Width / 2 - 1, this.Height / 2 - (int)size.Height / 2 + 2));

            GraphicsPath backPath = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius);
            backPath.AddRectangle(new Rectangle(-1, -1, this.Width + 1, this.Height + 1));

            g.FillPath(brushBack, backPath);

            brush1.Dispose();
            brush2.Dispose();
            brushBack.Dispose();
            brushText.Dispose();
            path1.Dispose();
            path2.Dispose();
            backPath.Dispose();
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            //this.Region = new Region(CreateRound(new Rectangle(0, 0, this.Width, this.Height), radius));
        }

        /// <summary>Timer_Ticked 方法。</summary>
        private void Timer_Ticked(object sender, EventArgs e)
        {
            waveLeft -= 3;
            if (waveLeft == -2 * waveWidth)
                waveLeft = -1 * waveWidth;
            this.Invalidate();
        }

        /// <summary>This_ValueChanged 方法。</summary>
        private void This_ValueChanged(object sender, EventArgs e)
        {
            double percent = (double)Math.Round((decimal)this.value / (this.maximun - this.minimun), 2);//    (double)(this.value*100 / (this.maximun - this.minimun));
            int top = (int)(this.Height - this.Height * percent) - WaveHeight;
            this.WaveTop = top;
            this.Invalidate();
        }

        /// <summary>AddStep 方法。</summary>
        public void AddStep()
        {
            if (value + step > maximun)
            {
                Value = maximun;
            }
            else
            {
                Value += step;
            }
        }

        /// <summary>ReduceStep 方法。</summary>
        public void ReduceStep()
        {
            if (value - step < minimun)
            {
                Value = minimun;
            }
            else
            {
                Value -= step;
            }
        }

        /// <summary>CreateRound 方法。</summary>
        private GraphicsPath CreateRound(Rectangle rect, int radius)
        {
            GraphicsPath roundRect = new GraphicsPath();

            if (radius != 0)
            {
                //顶端
                roundRect.AddLine(rect.Left + radius - 1, rect.Top - 1, rect.Right - radius, rect.Top - 1);
                //右上角
                roundRect.AddArc(rect.Right - radius, rect.Top - 1, radius, radius, 270, 90);
                //右边
                roundRect.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom - radius);
                //右下角

                roundRect.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                //底边
                roundRect.AddLine(rect.Right - radius, rect.Bottom, rect.Left + radius, rect.Bottom);
                //左下角
                roundRect.AddArc(rect.Left - 1, rect.Bottom - radius, radius, radius, 90, 90);
                //左边
                roundRect.AddLine(rect.Left - 1, rect.Top + radius, rect.Left - 1, rect.Bottom - radius);
                //左上角
                roundRect.AddArc(rect.Left - 1, rect.Top - 1, radius, radius, 180, 90);
            }
            else
            {
                //顶端
                roundRect.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
                //右边
                roundRect.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
                //底边
                roundRect.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
                //左边
                roundRect.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom);
            }

            return roundRect;
        }
    }
}