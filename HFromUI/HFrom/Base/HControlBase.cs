using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Base
{
    public partial class HControlBase : UserControl,IContainer
    {
        /// <summary>radius 字段。</summary>
        private int radius = 5;
        /// <summary>borderWidth 字段。</summary>
        private float borderWidth = 1;
        /// <summary>baseColor 字段。</summary>
        private Color baseColor = Control.DefaultBackColor;
        /// <summary>borderColor 字段。</summary>
        private Color borderColor = Color.LightGray;
        /// <summary>showBorder 字段。</summary>
        private bool showBorder = true;
        /// <summary>roundStyle 字段。</summary>
        private HFromUI.HEnum.HRoundStyle roundStyle = HFromUI.HEnum.HRoundStyle.All;

        /// <summary>
        /// 圆角半径
        /// </summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径"), Browsable(true)]
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
                this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius, roundStyle));
                if (copy != radius)
                {
                    RadiusChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("圆角类型"), HDescriptionLanguage("圆角类型"), Browsable(true)]
        public HFromUI.HEnum.HRoundStyle RoundStyle
        {
            get { return roundStyle; }
            set
            {
                roundStyle = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 边框宽度
        /// </summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度"), Browsable(true)]
        public float BorderWidth
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
        [HCategoryLanguage("通用"), HDisplayNameLanguage("底色"), HDescriptionLanguage("背景色"), Browsable(true)]
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
        [HCategoryLanguage("通用"), HDisplayNameLanguage("边框色"), HDescriptionLanguage("边框色"), Browsable(true)]
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

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示边框"), HDescriptionLanguage("是否显示边框"), Browsable(true)]
        public bool ShowBorder
        {
            get { return showBorder; }
            set
            {
                showBorder = value;
                this.Invalidate();
            }
        }

        /// <summary>Components 成员。</summary>
        public ComponentCollection Components => throw new NotImplementedException();

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

        public HControlBase()
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
            this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius,roundStyle));
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            HDrawPaint.SetGraphicsHighQuality(g);

            using (Pen pen = new Pen(borderColor, borderWidth))
            using (SolidBrush brush = new SolidBrush(baseColor))
            using (GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius, roundStyle))
            using (GraphicsPath path2 = HDrawPaint.CreatePath(new RectangleF(borderWidth, borderWidth, this.Width - 2f * borderWidth, this.Height - 2f * borderWidth), radius, roundStyle))
            {
                g.FillPath(brush, path2);
                if (showBorder)
                {
                    g.DrawPath(pen, path2);
                }
                using (Pen backPen = new Pen(this.BackColor))
                {
                    g.DrawPath(backPen, path);
                }
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Region = new Region(HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), radius, roundStyle));
            if (SizeChangeded != null)
            {
                SizeChangeded(this, e);
            }
        }

        /// <summary>添加。</summary>
        public void Add(IComponent component)
        {
            components.Add(component);
        }

        /// <summary>添加。</summary>
        public void Add(IComponent component, string name)
        {
            components.Add(component, name);
        }

        /// <summary>移除。</summary>
        public void Remove(IComponent component)
        {
            components.Remove(component);
        }
    }
}