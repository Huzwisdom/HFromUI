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
    public partial class HGroupBox : GroupBox
    {
        /// <summary>文本。</summary>
        private string text = "HGroupbox";

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题"), HDescriptionLanguage("标题"), Browsable(true)]
        public override string Text
        {
            get { return text; }
            set
            {
                text = value;
                this.Invalidate();
            }
        }

        /// <summary>titleOffset 字段。</summary>
        private int titleOffset = 0;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题x轴位置偏移"), HDescriptionLanguage("标题x轴位置偏移"), Browsable(true)]
        public int TilteOffset
        {
            get { return titleOffset; }
            set
            {
                this.titleOffset = value;
                this.Invalidate();
            }
        }

        /// <summary>radius 字段。</summary>
        private int radius = 10;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框圆角半径"), HDescriptionLanguage("边框圆角半径"), Browsable(true)]
        public int Radius
        {
            get { return radius; }
            set { this.radius = value; this.Invalidate(); }
        }

        /// <summary>titleRadius 字段。</summary>
        private int titleRadius = 10;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题边框圆角半径"), HDescriptionLanguage("标题边框圆角半径"), Browsable(true)]
        public int TitleRadius
        {
            get { return titleRadius; }
            set { this.titleRadius = value; this.Invalidate(); }
        }

        /// <summary>borderColor 字段。</summary>
        private Color borderColor = Color.Gray;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框颜色"), HDescriptionLanguage("边框颜色"), Browsable(true)]
        public Color BorderColor
        {
            get { return borderColor; }
            set { this.borderColor = value; this.Invalidate(); }
        }

        /// <summary>titleBorderColor 字段。</summary>
        private Color titleBorderColor = Color.Gray;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("标题边框颜色"), HDescriptionLanguage("标题边框颜色"), Browsable(true)]
        public Color TitleBorderColor
        {
            get { return titleBorderColor; }
            set { this.titleBorderColor = value; this.Invalidate(); }
        }

        /// <summary>BaseColor 成员。</summary>
        /// <summary>BaseColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("底色"), HDescriptionLanguage("主题背景色"), Browsable(true)]
        public Color BaseColor { get; set; } = Color.White;

        /// <summary>TitleBaseColor 成员。</summary>
        /// <summary>TitleBaseColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选项卡标题栏背景色"), HDescriptionLanguage("标题背景色"), Browsable(true)]
        public Color TitleBaseColor { get; set; } = Color.White;

        /// <summary>BorderWidth 成员。</summary>
        /// <summary>BorderWidth 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度"), Browsable(true)]
        public int BorderWidth { get; set; } = 1;

        /// <summary>TextDrawMode 成员。</summary>
        public DrawMode TextDrawMode { get; set; } = DrawMode.Anti;

        public enum DrawMode
        {
            Anti,
            Clear
        }

        /// <summary>TextAlignment 成员。</summary>
        public StringAlignment TextAlignment { get; set; } = StringAlignment.Center;

        public HGroupBox()
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
            this.UpdateStyles();
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextDrawMode == DrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.Clear(this.BackColor);
            SizeF fontsize = g.MeasureString(this.Text, this.Font);
            fontsize.Width += 8;
            fontsize.Height += 6;

            float x = 10;
            switch (TextAlignment)
            {
                case StringAlignment.Center: x = (this.Width - fontsize.Width) / 2; break;
                case StringAlignment.Near: x = radius; break;
                case StringAlignment.Far: x = this.Width - fontsize.Width - radius; break;
            }
            Rectangle textRect = new Rectangle((int)(x + titleOffset - BorderWidth), BorderWidth, (int)(fontsize.Width + 2f * BorderWidth), (int)(fontsize.Height + 2f * BorderWidth));
            Rectangle boxRect = new Rectangle(BorderWidth, (int)(textRect.Height / 2 + 0.5f * BorderWidth), (int)(this.Width - 2f * BorderWidth ), (int)(this.Height - textRect.Height / 2 - 1.5f * BorderWidth));

            Pen borderPen = new Pen(borderColor, BorderWidth);
            Pen titleBorderPen = new Pen(titleBorderColor, BorderWidth);

            GraphicsPath path1 = HDrawPaint.CreatePath(boxRect, radius);
            GraphicsPath path2 = HDrawPaint.CreatePath(textRect, titleRadius);

            Brush basebrush = new SolidBrush(this.BaseColor);
            Brush titleBasebrush = new SolidBrush(this.TitleBaseColor);
            Brush textbrush = new SolidBrush(this.ForeColor);
            Brush backbrush = new SolidBrush(this.BackColor);

            g.FillPath(basebrush, path1);
            g.DrawPath(borderPen, path1);
            path1.AddRectangle(new Rectangle(-1, -1, this.Width + 1, this.Height + 1));
            g.FillPath(backbrush, path1);

            g.FillPath(titleBasebrush, path2);
            g.DrawPath(titleBorderPen, path2);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            g.DrawString(this.Text, this.Font, textbrush, textRect, stringFormat);

            borderPen.Dispose();
            titleBorderPen.Dispose();
            basebrush.Dispose();
            titleBasebrush.Dispose();
            textbrush.Dispose();
            backbrush.Dispose();

            path1.Dispose();
            path2.Dispose();

            borderPen.Dispose();
            titleBorderPen.Dispose();
        }
    }
}