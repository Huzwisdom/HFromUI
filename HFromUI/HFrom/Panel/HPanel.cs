using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Panel
{
    using Panel = System.Windows.Forms.Panel;
    public partial class HPanel : System.Windows.Forms.Panel
    {
        /// <summary>radius 字段。</summary>
        private int radius = 5;
        /// <summary>borderWidth 字段。</summary>
        private int borderWidth = 1;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Control.DefaultBackColor;
        /// <summary>borderColor 字段。</summary>
        private Color borderColor = Color.LightGray;
        /// <summary>showBorder 字段。</summary>
        private bool showBorder = true;
        /// <summary>roundStyle 字段。</summary>
        private HEnum.HRoundStyle roundStyle=HEnum.HRoundStyle.All;

        /// <summary>
        /// 圆角半径
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set
            {
                if (radius < 0)
                    return;
                int copy = radius;
                radius = value;
                this.Invalidate();
                this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius));
                if (copy != radius)
                {
                    if (RadiusChanged != null)
                    {
                        RadiusChanged(this, new EventArgs());
                    }
                }
            }
        }

        /// <summary>
        /// 边框宽度
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度"), Browsable(true)]
        public int BorderWidth
        {
            get { return borderWidth; }
            set
            {
                if (value <= 0)
                    return;
                borderWidth = value;// % 2 == 0 ? value + 1 : value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景色"), Browsable(true)]
        public Color BaseColor
        {
            get { return baseColor; }
            set
            {
                baseColor = value;
                this.Invalidate();
                if (BaseColorChanged != null)
                {
                    BaseColorChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// 边框色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框色"), HDescriptionLanguage("边框色"), Browsable(true)]
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                this.Invalidate();
                if (LineColorChanged != null)
                {
                    LineColorChanged(this, new EventArgs());
                }
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("是否显示边框"), HDescriptionLanguage("是否显示边框"), Browsable(true)]
        public bool ShowBorder
        {
            get { return showBorder; }
            set
            {
                showBorder = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆角样式"), HDescriptionLanguage("圆角样式"), Browsable(true)]
        public HEnum.HRoundStyle RoundStyle
        {
            get { return roundStyle; }
            set
            {
                roundStyle = value;
                this.Invalidate(true);
                SetReion(radius);
            }
        }

        /// <summary>
        /// 尺寸变化事件
        /// </summary>
        [HDescriptionLanguage("尺寸变化事件")]
        public event EventHandler SizeChangeded;

        /// <summary>
        /// 圆角直径变化事件
        /// </summary>
        [HDescriptionLanguage("圆角直径变化事件")]
        public event EventHandler RadiusChanged;

        /// <summary>
        /// 内部颜色变化事件
        /// </summary>
        [HDescriptionLanguage("内部颜色变化事件")]
        public event EventHandler BaseColorChanged;

        /// <summary>
        /// 边框颜色变化事件
        /// </summary>
        [HDescriptionLanguage("边框颜色变化事件")]
        public event EventHandler LineColorChanged;

        public HPanel()
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
            SetReion(radius);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            int width = this.Width;
            int height = this.Height;
            int halflinewidth = borderWidth % 2 == 0 ? borderWidth / 2 : borderWidth / 2 + 1;
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Pen pen = new Pen(borderColor, (float)borderWidth);
            SolidBrush brush = new SolidBrush(baseColor);

            GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius,roundStyle);
            GraphicsPath path2 = HDrawPaint.CreatePath(new RectangleF(borderWidth, borderWidth, this.Width - 2f * borderWidth, this.Height - 2f * borderWidth), radius,roundStyle);
            g.FillPath(brush, path2);
            if (showBorder)
            {
                g.DrawPath(pen, path2);
            }
            g.DrawPath(new Pen(this.BackColor), path);

            pen.Dispose();
            brush.Dispose();
            path.Dispose();
            path2.Dispose();

            base.OnPaint(e);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SetReion(radius);
            if (SizeChangeded != null)
            {
                SizeChangeded(this, e);
            }
        }

        /// <summary>设置 reion。</summary>
        private void SetReion(int radius)
        {
            using (GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(Point.Empty, base.Size), radius,roundStyle))
            {
                Region region = new Region(path);
                path.Widen(Pens.White);
                region.Union(path);
                this.Region = region;
            }
        }
    }
}