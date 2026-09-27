using HFromUI.HAttribute;

using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.MenuStrip
{
    using MenuStrip = System.Windows.Forms.MenuStrip;
    using HFromUI.HColor;
    public class HMenuStripRenderer : ToolStripProfessionalRenderer
    {
        #region 私有属性

        /// <summary>radius 字段。</summary>
        private int radius = 10;

        #endregion 私有属性

        #region 公共属性

        [HDescriptionLanguage("MenuStrip的边框圆角")]
        public int Radius
        {
            get { return radius; }
            set { if (value > 0) radius = value; }
        }

        /// <summary>背景颜色。</summary>
        /// <summary>背景颜色。</summary>
        [HDescriptionLanguage("背景色")]
        public Color BackColor { get; set; } = Color.White;

        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [HDescriptionLanguage("边框色")]
        public Color BorderColor { get; set; } = Color.LightGray;

        /// <summary>ItemBorderColor 成员。</summary>
        /// <summary>ItemBorderColor 字段。</summary>
        [HDescriptionLanguage("选项边框色")]
        public Color ItemBorderColor { get; set; } = Color.White;

        /// <summary>ItemSelectBackColor 成员。</summary>
        /// <summary>ItemSelectBackColor 字段。</summary>
        [HDescriptionLanguage("选项选中背景色")]
        public Color ItemSelectBackColor { get; set; } = Color.DodgerBlue;

        /// <summary>ItemSelectBorderColor 成员。</summary>
        /// <summary>ItemSelectBorderColor 字段。</summary>
        [HDescriptionLanguage("选项选中边框色")]
        public Color ItemSelectBorderColor { get; set; } = Color.DodgerBlue;

        /// <summary>ItemSelectForeColor 成员。</summary>
        /// <summary>ItemSelectForeColor 字段。</summary>
        [HDescriptionLanguage("选项选中字体色")]
        public Color ItemSelectForeColor { get; set; } = Color.White;

        /// <summary>ItemDisabledBackColor 成员。</summary>
        /// <summary>ItemDisabledBackColor 字段。</summary>
        [HDescriptionLanguage("选项Disable时背景色")]
        public Color ItemDisabledBackColor { get; set; } = Control.DefaultBackColor;

        /// <summary>ItemDisabledBorderColor 成员。</summary>
        /// <summary>ItemDisabledBorderColor 字段。</summary>
        [HDescriptionLanguage("选项Disable时边框色")]
        public Color ItemDisabledBorderColor { get; set; } = Control.DefaultBackColor;

        /// <summary>ItemDisbaledForeColor 成员。</summary>
        /// <summary>ItemDisbaledForeColor 字段。</summary>
        [HDescriptionLanguage("选项Disable时字体色")]
        public Color ItemDisbaledForeColor { get; set; } = Color.LightGray;

        /// <summary>ArrowColor 成员。</summary>
        /// <summary>ArrowColor 字段。</summary>
        [HDescriptionLanguage("选项箭头颜色")]
        public Color ArrowColor { get; set; } = Color.DodgerBlue;

        /// <summary>ArrowSelectColor 成员。</summary>
        /// <summary>ArrowSelectColor 字段。</summary>
        [HDescriptionLanguage("选项选中时箭头颜色")]
        public Color ArrowSelectColor { get; set; } = Color.White;

        /// <summary>SeparatorColor 成员。</summary>
        /// <summary>SeparatorColor 字段。</summary>
        [HDescriptionLanguage("选项分隔线颜色")]
        public Color SeparatorColor { get; set; } = Color.DodgerBlue;

        /// <summary>ArrowImage 成员。</summary>
        /// <summary>ArrowImage 字段。</summary>
        [HDescriptionLanguage("选项箭头自定义图标")]
        public Image ArrowImage { get; set; } = HPhoto.Get("rightBlue");

        /// <summary>ArrowSelectImage 成员。</summary>
        /// <summary>ArrowSelectImage 字段。</summary>
        [HDescriptionLanguage("选项选中时箭头自定义图标")]
        public Image ArrowSelectImage { get; set; } = HPhoto.Get("right");

        /// <summary>TextDrawMode 成员。</summary>
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Anti;


        #endregion 公共属性

        public HMenuStripRenderer()
        {
        }

        /// <summary>响应 RenderToolStripBackground 事件。</summary>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            ToolStrip toolStrip = e.ToolStrip;
            Graphics g = e.Graphics;
            HDrawPaint.SetGraphicsHighQuality(g);
            Rectangle bounds = e.AffectedBounds;
            LinearGradientBrush lgbrush = new LinearGradientBrush(new Point(0, 0), new Point(0, toolStrip.Height), Color.FromArgb(255, BackColor), Color.FromArgb(150, BackColor));
            if (toolStrip is MenuStrip)
            {
                //由menuStrip的Paint方法定义 这里不做操作
            }
            else if (toolStrip is ToolStripDropDown)
            {
                GraphicsPath path = HDrawPaint.CreatePath(bounds, radius);
                toolStrip.Region = new Region(path);
                g.FillPath(lgbrush, path);
            }
            else
            {
                base.OnRenderToolStripBackground(e);
            }
        }

        /// <summary>响应 RenderToolStripBorder 事件。</summary>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
            Graphics g = e.Graphics;
            HDrawPaint.SetGraphicsHighQuality(g);
            Rectangle bounds = e.AffectedBounds;

            GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), radius);
            g.DrawPath(new Pen(BorderColor), path);
        }

        /// <summary>响应 RenderArrow 事件。</summary>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            if (e.Item.Enabled)
            {
                if (e.Item.Selected)
                {
                    if (ArrowSelectImage != null)
                    {
                        Rectangle rect = e.ArrowRectangle;
                        Graphics g = e.Graphics;
                        HDrawPaint.SetGraphicsHighQuality(g);
                        g.DrawImage(ArrowSelectImage, new Rectangle(rect.Left + (rect.Width - ArrowSelectImage.Width) / 2, rect.Top + (rect.Height - ArrowSelectImage.Height) / 2, ArrowSelectImage.Width, ArrowSelectImage.Height));
                    }
                    else
                    {
                        e.ArrowColor = ArrowSelectColor;
                        base.OnRenderArrow(e);
                    }
                }
                else
                {
                    if (ArrowImage != null)
                    {
                        Rectangle rect = e.ArrowRectangle;
                        Graphics g = e.Graphics;
                        HDrawPaint.SetGraphicsHighQuality(g);
                        g.DrawImage(ArrowImage, new Rectangle(rect.Left + (rect.Width - ArrowImage.Width) / 2, rect.Top + (rect.Height - ArrowImage.Height) / 2, ArrowImage.Width, ArrowImage.Height));
                    }
                    else
                    {
                        e.ArrowColor = ArrowColor;
                        base.OnRenderArrow(e);
                    }
                }
            }
            else
            {
                if (ArrowImage != null)
                {
                    Rectangle rect = e.ArrowRectangle;
                    Graphics g = e.Graphics;
                    HDrawPaint.SetGraphicsHighQuality(g);
                    g.DrawImage(ArrowImage, new Rectangle(rect.Left + (rect.Width - ArrowImage.Width) / 2, rect.Top + (rect.Height - ArrowImage.Height) / 2, ArrowImage.Width, ArrowImage.Height));
                }
                else
                {
                    e.ArrowColor = ArrowColor;
                    base.OnRenderArrow(e);
                }
            }
        }

        /// <summary>响应 RenderMenuItemBackground 事件。</summary>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Graphics g = e.Graphics;
            ToolStripItem item = e.Item;
            ToolStrip toolstrip = e.ToolStrip;
            // PaintHelper.SetGraphicsHighQuality(g);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle bound = new Rectangle(0, 0, item.Width, item.Height);
            RectangleF textbound = HDrawPaint.GetTextRectInRect(item.Text, item.Font, bound, item.TextAlign);
            //渲染顶级项
            if (toolstrip is MenuStrip)
            {
                LinearGradientBrush lgbrush = new LinearGradientBrush(new Point(0, 0), new Point(0, item.Height), Color.FromArgb(100, Color.White), Color.FromArgb(0, Color.White));
                Brush brushFore = new SolidBrush(item.ForeColor);
                GraphicsPath gp = HDrawPaint.CreatePath(new Rectangle(new Point(0, 0), item.Size), 5);
                if (item.Selected)
                {
                    g.FillPath(lgbrush, gp);
                    g.DrawString(item.Text, item.Font, brushFore, textbound);
                }
                else if (item.Pressed)
                {
                    g.FillPath(Brushes.White, gp);
                    g.DrawString(item.Text, item.Font, brushFore, textbound);
                }
                else
                {
                    g.DrawString(item.Text, item.Font, brushFore, textbound);
                }
                lgbrush.Dispose();
                brushFore.Dispose();
                gp.Dispose();
            }
            //渲染下拉项
            else if (toolstrip is ToolStripDropDown)
            {
                LinearGradientBrush lgbrush = new LinearGradientBrush(new Point(0, 0), new Point(item.Width, 0), ItemSelectBackColor, ItemSelectBackColor);
                GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(3, 0, item.Width - 5, item.Height), radius);

                if (item.Enabled)
                {
                    if (item.Selected)
                    {
                        Pen penSelect = new Pen(ItemSelectBorderColor);
                        Brush brushSelect = new SolidBrush(ItemSelectForeColor);
                        g.FillPath(lgbrush, path);
                        g.DrawPath(penSelect, path);
                        g.DrawString(item.Text, toolstrip.Font, brushSelect, textbound);
                        penSelect.Dispose();
                        brushSelect.Dispose();
                    }
                    else
                    {
                        Pen penBorder = new Pen(ItemBorderColor);
                        Brush brushFore = new SolidBrush(item.ForeColor);
                        lgbrush = new LinearGradientBrush(new Point(0, 0), new Point(item.Width, 0), item.BackColor, item.BackColor);
                        g.FillPath(lgbrush, path);
                        g.DrawPath(penBorder, path);
                        g.DrawString(item.Text, toolstrip.Font, brushFore, textbound);

                        penBorder.Dispose();
                        brushFore.Dispose();
                    }
                }
                else
                {
                    Pen penDisable = new Pen(ItemDisabledBorderColor);
                    Brush brushDisable = new SolidBrush(ItemDisbaledForeColor);

                    lgbrush = new LinearGradientBrush(new Point(0, 0), new Point(item.Width, 0), ItemDisabledBackColor, ItemDisabledBackColor);
                    g.FillPath(lgbrush, path);
                    g.DrawPath(penDisable, path);
                    g.DrawString(item.Text, toolstrip.Font, brushDisable, textbound);

                    penDisable.Dispose();
                    brushDisable.Dispose();
                }
                lgbrush.Dispose();
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        /// <summary>响应 RenderSeparator 事件。</summary>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Graphics g = e.Graphics;

            LinearGradientBrush lgbrush = new LinearGradientBrush(new Point(0, 0), new Point(e.Item.Width, 0), SeparatorColor, SeparatorColor);
            g.FillRectangle(lgbrush, new Rectangle(3, e.Item.Height / 2, e.Item.Width, 1));
            lgbrush.Dispose();
            //base.OnRenderSeparator(e);
        }

        /// <summary>响应 RenderImageMargin 事件。</summary>
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            //base.OnRenderImageMargin(e);
        }

        /// <summary>响应 RenderItemText 事件。</summary>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            //base.OnRenderItemText(e);
        }
    }
}
