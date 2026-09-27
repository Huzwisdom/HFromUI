using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Bars
{
    [DefaultEvent("ValueChanged")]
    public partial class HRoundProgressBar : HBarBase
    {
        /// <summary>值。</summary>
        private long value = 0;
        /// <summary>minimun 字段。</summary>
        private long minimun = 0;
        /// <summary>maximun 字段。</summary>
        private long maximun = 100;
        /// <summary>step 字段。</summary>
        private long step = 1;
        /// <summary>circleWidth 字段。</summary>
        private int circleWidth = 16;
        /// <summary>startAngle 字段。</summary>
        private int startAngle = 0;
        /// <summary>radius 字段。</summary>
        private int radius = 10;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.LightGray;
        /// <summary>valueColor 字段。</summary>
        private Color valueColor = Color.DodgerBlue;
        /// <summary>insideColor 字段。</summary>
        private Color insideColor = Color.LightGray;
        /// <summary>showType 字段。</summary>
        private HShowType showType = HShowType.Ring;
        /// <summary>direction 字段。</summary>
        private HDirection direction = HDirection.ClockWise;
        /// <summary>valueType 字段。</summary>
        private HValueType valueType = HValueType.Percent;

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
                this.Invalidate();
                OnValueChanged(EventArgs.Empty);

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

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆环宽度"), HDescriptionLanguage("圆环宽度"), Browsable(true)]
        public int CircleWidth
        {
            get { return circleWidth; }
            set
            {
                //if(value>base.Width/2)
                //{
                //    circleWidth = base.Width / 2;
                //}
                //else if(value<=0)
                //{
                //    circleWidth = 1;
                //}
                //else
                //{
                circleWidth = value;
                //}
                base.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("起始角度"), HDescriptionLanguage("起始角度"), Browsable(true)]
        public int StartAngle
        {
            get { return startAngle; }
            set
            {
                if (value < 0)
                {
                    startAngle = 0;
                }
                else if (value > 360)
                {
                    startAngle = 360;
                }
                else
                {
                    startAngle = value;
                }
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("底色"), HDescriptionLanguage("底色"), Browsable(true)]
        public Color BaseColor
        {
            get { return this.baseColor; }
            set
            {
                this.baseColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("有值的颜色"), HDescriptionLanguage("有值的颜色"), Browsable(true)]
        public Color ValueColor
        {
            get { return this.valueColor; }
            set
            {
                this.valueColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("内部圆的颜色"), HDescriptionLanguage("内部圆的颜色，"), Browsable(true)]
        public Color InsideColor
        {
            get { return this.insideColor; }
            set
            {
                this.insideColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示类型"), HDescriptionLanguage("显示类型，扇形或环形"), Browsable(true)]
        public HShowType ShowType
        {
            get { return showType; }
            set
            {
                showType = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度条方向"), HDescriptionLanguage("进度条方向"), Browsable(true)]
        public HDirection Direction
        {
            get { return direction; }
            set
            {
                direction = value;
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

        /// <summary>圆环显示类型。</summary>
        public enum HShowType
        {
            /// <summary>扇形。</summary>
            Sector,
            /// <summary>环形。</summary>
            Ring,
        }

        /// <summary>进度方向。</summary>
        public enum HDirection
        {
            /// <summary>顺时针。</summary>
            ClockWise,
            /// <summary>逆时针。</summary>
            AntiClockWise,
        }

        /// <summary>进度文字显示类型。</summary>
        public enum HValueType
        {
            /// <summary>原始数值。</summary>
            Value,
            /// <summary>百分比。</summary>
            Percent,
        }

        /// <summary>TextDrawMode 成员。</summary>
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Anti;

        public HRoundProgressBar()
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
            this.radius = Math.Min(this.Width, this.Height);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.radius = Math.Min(this.Width, this.Height);
            if (this.Width != this.Height)
            {
                this.Size = new Size(Math.Min(this.Width, this.Height), Math.Min(this.Width, this.Height));
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            Brush brushBase = new SolidBrush(baseColor);
            Brush brushValue = new SolidBrush(valueColor);
            Brush brushText = new SolidBrush(this.ForeColor);
            Pen penBack = new Pen(this.BackColor);

            g.FillEllipse(brushBase, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
            float percent = (float)Math.Round((decimal)this.value / (this.maximun - this.minimun), 2);
            if (direction == HDirection.ClockWise)//顺时针
            {
                g.FillPie(brushValue, new Rectangle(0, 0, this.Width - 1, this.Height - 1), (float)(startAngle + 270), (float)(360 * percent));
            }
            else//逆时针
            {
                g.FillPie(brushValue, new Rectangle(0, 0, this.Width - 1, this.Height - 1), (float)(startAngle + 270 - (360 * percent)), (float)(360 * percent));
            }

            if (showType == HShowType.Ring)
            {
                Brush brushInside = new SolidBrush(insideColor);
                g.FillEllipse(brushInside, new Rectangle(circleWidth, circleWidth, this.Width - 1 - 2 * circleWidth, this.Height - 1 - 2 * circleWidth));
                brushInside.Dispose();
            }
            string text = (100 * percent).ToString() + "%";
            if (valueType == HValueType.Value)
            {
                text = value.ToString();
            }
            SizeF size = g.MeasureString(text, this.Font);
            g.DrawString(text, this.Font, brushText, new Point(this.Width / 2 - (int)size.Width / 2 - 1, this.Height / 2 - (int)size.Height / 2 + 2));
            GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius);
            g.DrawPath(penBack, path);

            brushBase.Dispose();
            brushText.Dispose();
            brushValue.Dispose();
            penBack.Dispose();
            path.Dispose();
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
    }
}