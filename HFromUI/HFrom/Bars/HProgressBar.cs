using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HFrom.From;

namespace HFromUI.HFrom.Bars
{
    [DefaultEvent("ValueChanged")]
    public partial class HProgressBar : UserControl
    {
        #region 变量定义

        public event EventHandler ValueChanged;

        /// <summary>值。</summary>
        private long value = 0;
        /// <summary>minimun 字段。</summary>
        private long minimun = 0;
        /// <summary>maximun 字段。</summary>
        private long maximun = 100;
        /// <summary>step 字段。</summary>
        private long step = 1;
        /// <summary>radius 字段。</summary>
        private int radius = 0;
        /// <summary>showTip 字段。</summary>
        private bool showTip = false;
        /// <summary>showValue 字段。</summary>
        private bool showValue = true;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Color.LightGray;
        /// <summary>valueColor 字段。</summary>
        private Color valueColor = Color.DodgerBlue;
        /// <summary>valueType 字段。</summary>
        private VALUETYPE valueType = VALUETYPE.PERCENT;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("进度条值"), HDescriptionLanguage("进度条值"), Browsable(true)]
        public long Value
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
                if (ValueChanged != null)
                {
                    ValueChanged(this, new EventArgs());
                }
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

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示提示"), HDescriptionLanguage("是否显示tip"), Browsable(true)]
        public bool ShowTip
        {
            get { return showTip; }
            set
            {
                showTip = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("是否显示进度文字"), HDescriptionLanguage("是否显示进度文字"), Browsable(true)]
        public bool ShowValue
        {
            get { return showValue; }
            set
            {
                showValue = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set
            {
                if (value < 0 || value > Math.Min(this.Width, this.Height))
                    return;
                radius = value;
                //this.Region = new Region(CreateRound(new Rectangle(0, 0, this.Width, this.Height), radius));
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

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文字显示类型"), HDescriptionLanguage("文字显示类型，数值和百分比"), Browsable(true)]
        public VALUETYPE ValueType
        {
            get { return valueType; }
            set
            {
                valueType = value;
            }
        }

        public enum VALUETYPE
        {
            VALUE,
            PERCENT,
        }

        #endregion 变量定义

        /// <summary>TextDrawMode 成员。</summary>
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Clear;

        public HProgressBar()
        {
            InitializeComponent();

            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.ValueChanged += new EventHandler(this.Value_Changed);
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //this.Region = new Region(CreateRound(new Rectangle(0, 0, this.Width, this.Height), radius));
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (radius > Math.Min(this.Width, this.Height))
            {
                radius = Math.Min(this.Width, this.Height);
            }
            //this.Region = new Region(CreateRound(new Rectangle(0, 0, this.Width, this.Height), radius));
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = radius==0? System.Drawing.Drawing2D.SmoothingMode.Default:SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            float percent = (float)Math.Round((decimal)this.value / (this.maximun - this.minimun), 2);

            Brush baseBrush = new SolidBrush(baseColor);
            Brush valueBrush = new SolidBrush(valueColor);
            Brush backBrush = new SolidBrush(BackColor);

            GraphicsPath basePath = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius);
            g.FillPath(baseBrush, basePath);

            Rectangle valueRect = new Rectangle((int)(-1 * (1 - percent) * (this.Width - 1)), 0, this.Width, this.Height);
            GraphicsPath valeupath = HDrawPaint.CreatePath(valueRect, radius,HEnum.HRoundStyle.Left);
            g.FillPath(valueBrush, valeupath);

            if (showValue)
            {
                string text = (100 * percent).ToString() + "%";
                if (valueType == VALUETYPE.VALUE)
                {
                    text = value.ToString();
                }
                SizeF size = g.MeasureString(text, this.Font);
                Brush textBrush = new SolidBrush(this.ForeColor);
                g.DrawString(text, this.Font, textBrush, new Point(this.Width / 2 - (int)size.Width / 2 - 1, this.Height / 2 - (int)size.Height / 2 + 2));
                textBrush.Dispose();
            }
            basePath.AddRectangle(new Rectangle(-1, -1, this.Width + 1, this.Height + 1));
            
            g.FillPath(backBrush, basePath);

            baseBrush.Dispose();
            valueBrush.Dispose();
            backBrush.Dispose();
            basePath.Dispose();
            valueBrush.Dispose();

            //g.DrawPath(new Pen(this.BackColor,1f),PaintHelper.CreateRound(new Rectangle(1, 1, this.Width-1, this.Height-1), radius));
        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible)
            {
                if (frmTips != null && !frmTips.IsDisposed)
                {
                    frmTips.Close();
                    frmTips = null;
                }
            }
        }

        /// <summary>Value_Changed 方法。</summary>
        private void Value_Changed(object sender, EventArgs e)
        {
            ShowTips();
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

        /// <summary>获取 percentValue。</summary>
        /// <summary>获取 percentValue。</summary>
        [HDescriptionLanguage("获取当前进度条百分比0-1")]
        public float GetPercentValue()
        {
            float percent = (float)Math.Round((decimal)this.value / (this.maximun - this.minimun), 2);
            return percent;
        }

        /// <summary>frmTips 字段。</summary>
        private HAnchorTips frmTips = null;

        /// <summary>ShowTips 方法。</summary>
        private void ShowTips()
        {
            if (showTip)
            {
                float percent = (float)Math.Round((decimal)this.value / (this.maximun - this.minimun), 2);
                Rectangle rect = new Rectangle((int)(percent * this.Width) - 5, 0, 5, this.Height);
                string text = (100 * percent).ToString() + "%";
                if (valueType == VALUETYPE.VALUE)
                {
                    text = value.ToString();
                }

                var p = this.PointToScreen(new Point(rect.X, rect.Y));

                if (frmTips == null || frmTips.IsDisposed || !frmTips.Visible)
                {
                    frmTips = HAnchorTips.ShowTips(new Rectangle(p.X, p.Y, rect.Width, rect.Height), text, this.Font.Name, (int)this.Font.Size, AnchorTipsLocation.TOP, ValueColor, ForeColor, autoCloseTime: -1);
                }
                else
                {
                    frmTips.RectControl = new Rectangle(p.X, p.Y, rect.Width, rect.Height);
                    frmTips.StrMsg = text;
                }
            }
        }
    }
}