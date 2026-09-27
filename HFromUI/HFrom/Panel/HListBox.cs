using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Panel
{
    using Panel = System.Windows.Forms.Panel;
    public partial class HListBox : ListBox
    {
        /// <summary>tip 字段。</summary>
        private ToolTip tip = new ToolTip();

        /// <summary>itemBaseColor 字段。</summary>
        private Color itemBaseColor = Color.White;
        /// <summary>itemTextColor 字段。</summary>
        private Color itemTextColor = Color.Black;
        /// <summary>itemSelectBaseColor 字段。</summary>
        private Color itemSelectBaseColor = Color.LightGray;
        /// <summary>itemSelectTextColor 字段。</summary>
        private Color itemSelectTextColor = Color.Black;

        /// <summary>ShowSerialNumber 成员。</summary>
        /// <summary>ShowSerialNumber 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示序号"), HDescriptionLanguage("是否显示序号"), Browsable(true)]
        public bool ShowSerialNumber { get; set; } = false;

        [HCategoryLanguage("通用"), HDisplayNameLanguage("正常背景色"), HDescriptionLanguage("正常背景色"), Browsable(true)]
        public Color ItemBaseColor
        {
            get { return itemBaseColor; }
            set
            {
                itemBaseColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("正常文本色"), HDescriptionLanguage("正常文本色"), Browsable(true)]
        public Color ItemTextColor
        {
            get { return itemTextColor; }
            set
            {
                itemTextColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("鼠标选中背景色"), HDescriptionLanguage("鼠标选中背景色"), Browsable(true)]
        public Color ItemSelectBaseColor
        {
            get { return itemSelectBaseColor; }
            set
            {
                itemSelectBaseColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("鼠标选中文本色"), HDescriptionLanguage("鼠标选中文本色"), Browsable(true)]
        public Color ItemSelectTextColor
        {
            get { return itemSelectTextColor; }
            set
            {
                itemSelectTextColor = value;
                this.Invalidate();
            }
        }

        /// <summary>ChooseIndex 成员。</summary>
        /// <summary>ChooseIndex 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中序号"), HDescriptionLanguage("选中序号"), Browsable(true)]
        public int ChooseIndex { get; set; } = -1;

        /// <summary>ChooseTextColor 成员。</summary>
        /// <summary>ChooseTextColor 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中文本色"), HDescriptionLanguage("选中文本色"), Browsable(true)]
        public Color ChooseTextColor { get; set; } = Color.DodgerBlue;

        /// <summary>TextDrawMode 成员。</summary>
        /// <summary>TextDrawMode 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("文本绘制模式"), HDescriptionLanguage("文本绘制模式"), Browsable(true)]
        public textDrawMode TextDrawMode { get; set; } = textDrawMode.Anti;

        public enum textDrawMode
        {
            Anti,
            Clear
        }

        /// <summary>TextAlign 成员。</summary>
        /// <summary>TextAlign 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("文本对齐方式"), HDescriptionLanguage("文本对齐方式"), Browsable(true)]
        public TextAligns TextAlign { get; set; } = TextAligns.Left;

        public enum TextAligns
        {
            Left,
            Center,
            Right
        }

        public HListBox()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            //this.SetStyle(ControlStyles.Selectable, true);
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
        //        return cp;
        //    }
        //}

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // base.OnDrawItem(e);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextDrawMode == textDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            StringFormat stringFormat = new StringFormat();
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags = StringFormatFlags.LineLimit;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;

            switch (TextAlign)
            {
                case TextAligns.Left: stringFormat.Alignment = StringAlignment.Near; break;
                case TextAligns.Center: stringFormat.Alignment = StringAlignment.Center; break;
                case TextAligns.Right: stringFormat.Alignment = StringAlignment.Far; break;
            }

            Brush backbrush = new SolidBrush(ItemBaseColor);
            Brush textbrush = new SolidBrush(ItemTextColor);

            if (this.SelectedIndex == e.Index)
            {
                backbrush = new SolidBrush(ItemSelectBaseColor);
                textbrush = new SolidBrush(ItemSelectTextColor);
            }

            if (e.Index == ChooseIndex)
            {
                textbrush = new SolidBrush(ChooseTextColor);
            }

            Bitmap bitmap = new Bitmap(e.Bounds.Width, e.Bounds.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.TextRenderingHint = TextDrawMode == textDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(backbrush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            if (this.Items.Count > 0)
            {
                string text = this.GetItemText(this.Items[e.Index]);
                if (ShowSerialNumber)
                {
                    text = (e.Index + 1).ToString() + "." + text;
                }

                g.DrawString(text, e.Font, textbrush, new Rectangle(0, 0, bitmap.Width, bitmap.Height), stringFormat);
            }
            e.Graphics.DrawImage(bitmap, e.Bounds);
            g.Dispose();
            bitmap.Dispose();

            backbrush.Dispose();
            textbrush.Dispose();
            stringFormat.Dispose();
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int mouseIndex = IndexFromPoint(this.PointToClient(MousePosition));
            if (mouseIndex != -1)
            {
                string text = this.GetItemText(this.Items[mouseIndex]);
                if (ShowSerialNumber)
                {
                    text = (mouseIndex + 1).ToString() + "." + text;
                }

                Size textsize = TextRenderer.MeasureText(text, this.Font);

                if (textsize.Width > this.Width)
                {
                    if (tip.GetToolTip(this) != text)
                    {
                        tip.SetToolTip(this, text);
                    }
                }
                else
                {
                    tip.SetToolTip(this, "");
                }
            }
            else
            {
                tip.SetToolTip(this, "");
            }
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            tip.SetToolTip(this, "");
        }

        /// <summary>响应 MouseDoubleClick 事件。</summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            int mouseIndex = IndexFromPoint(e.Location);
            if (mouseIndex != -1)
            {
                ChooseIndex = mouseIndex;
                this.Invalidate();
            }
            base.OnMouseDoubleClick(e);
        }
    }
}