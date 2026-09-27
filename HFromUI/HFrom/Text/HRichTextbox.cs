using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Text
{
    public partial class HRichTextbox : HControlBase
    {
        #region 属性定义

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("接受 Tab 键"), Browsable(true), HDescriptionLanguage("接受 Tab 键")]
        public bool AcceptsTab
        {
            get { return richTextBox1.AcceptsTab; }
            set { richTextBox1.AcceptsTab = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("自动字选择"), Browsable(true), HDescriptionLanguage("自动字选择")]
        public bool AutoWordSelection
        {
            get { return richTextBox1.AutoWordSelection; }
            set { richTextBox1.AutoWordSelection = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("项目符号缩进"), Browsable(true), HDescriptionLanguage("项目符号缩进")]
        public int BulletIndent
        {
            get { return richTextBox1.BulletIndent; }
            set { richTextBox1.BulletIndent = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("识别网址"), Browsable(true), HDescriptionLanguage("识别网址")]
        public bool DetectUrls
        {
            get { return richTextBox1.DetectUrls; }
            set { richTextBox1.DetectUrls = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("启用拖放"), Browsable(true), HDescriptionLanguage("启用拖放")]
        public bool EnableAutoDrogDrop
        {
            get { return richTextBox1.EnableAutoDragDrop; }
            set
            {
                richTextBox1.EnableAutoDragDrop = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("隐藏选择"), Browsable(true), HDescriptionLanguage("隐藏选择")]
        public bool HideSelection
        {
            get { return richTextBox1.HideSelection; }
            set
            {
                richTextBox1.HideSelection = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("最大长度"), Browsable(true), HDescriptionLanguage("最大长度")]
        public int MaxLength
        {
            get { return richTextBox1.MaxLength; }
            set
            {
                richTextBox1.MaxLength = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("多行"), Browsable(true), HDescriptionLanguage("多行")]
        public bool Multiline
        {
            get { return richTextBox1.Multiline; }
            set { richTextBox1.Multiline = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("只读"), Browsable(true), HDescriptionLanguage("只读")]
        public bool ReadOnly
        {
            get { return richTextBox1.ReadOnly; }
            set
            {
                richTextBox1.ReadOnly = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("启用快捷键"), Browsable(true), HDescriptionLanguage("启用快捷键")]
        public bool ShortcutsEnabled
        {
            get { return richTextBox1.ShortcutsEnabled; }
            set { richTextBox1.ShortcutsEnabled = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示选择外边距"), Browsable(true), HDescriptionLanguage("显示选择外边距")]
        public bool ShowSelectionMargin
        {
            get { return richTextBox1.ShowSelectionMargin; }
            set { richTextBox1.ShowSelectionMargin = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("自动换行"), Browsable(true), HDescriptionLanguage("自动换行")]
        public bool WordWrap
        {
            get { return richTextBox1.WordWrap; }
            set
            {
                richTextBox1.WordWrap = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("缩放系数"), Browsable(true), HDescriptionLanguage("缩放系数")]
        public float ZoomFactor
        {
            get { return richTextBox1.ZoomFactor; }
            set { richTextBox1.ZoomFactor = value; }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("文本"), Browsable(true), HDescriptionLanguage("文本")]
        public override string Text
        {
            get
            {
                return richTextBox1.Text;
            }

            set
            {
                richTextBox1.Text = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("前景颜色"), Browsable(true), HDescriptionLanguage("前景颜色")]
        public override Color ForeColor
        {
            get
            {
                return richTextBox1.ForeColor;
            }

            set
            {
                richTextBox1.ForeColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选择起始"), Browsable(false), HDescriptionLanguage("选择起始")]
        public int SelectionStart
        {
            get { return richTextBox1.SelectionStart; }
            set
            {
                richTextBox1.SelectionStart = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("选择颜色"), Browsable(false), HDescriptionLanguage("选择颜色")]
        public Color SelectionColor
        {
            get { return richTextBox1.SelectionColor; }
            set
            {
                richTextBox1.SelectionColor = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("滚动条"), Browsable(true), HDescriptionLanguage("滚动条")]
        public RichTextBoxScrollBars ScrollBars
        {
            get
            {
                return richTextBox1.ScrollBars;
            }
            set
            {
                richTextBox1.ScrollBars = value;
            }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条底色"), Browsable(true), HDescriptionLanguage("垂直滚动条底色")]
        public Color VScrollBar_BaseColor
        {
            get { return hvScrollBarExt1.BaseColor; }
            set { hvScrollBarExt1.BaseColor = value; }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条按钮颜色"), Browsable(true), HDescriptionLanguage("垂直滚动条按钮颜色")]
        public Color VScrollBar_ButtonColor
        {
            get { return hvScrollBarExt1.ButtonColor; }
            set { hvScrollBarExt1.ButtonColor = value; }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条按钮悬停色"), Browsable(true), HDescriptionLanguage("垂直滚动条按钮悬停色")]
        public Color VScrollBar_ButtonHoverColor
        {
            get { return hvScrollBarExt1.ButtonHoverColor; }
            set { hvScrollBarExt1.ButtonHoverColor = value; }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条圆角"), Browsable(true), HDescriptionLanguage("垂直滚动条圆角")]
        public int VScrollBar_BaseRadius
        {
            get { return hvScrollBarExt1.BaseRadius; }
            set { hvScrollBarExt1.BaseRadius = value; }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条按钮圆角"), Browsable(true), HDescriptionLanguage("垂直滚动条按钮圆角")]
        public int VScrollBar_ButtonRadius
        {
            get { return hvScrollBarExt1.ButtonRadius; }
            set { hvScrollBarExt1.ButtonRadius = value; }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条宽度"), Browsable(true), HDescriptionLanguage("垂直滚动条宽度")]
        public int VScrollBar_BaseSize
        {
            get { return hvScrollBarExt1.BaseSize; }
            set { hvScrollBarExt1.BaseSize = value; }
        }

        [HCategoryLanguage("滚动条"), HDisplayNameLanguage("垂直滚动条按钮大小"), Browsable(true), HDescriptionLanguage("垂直滚动条按钮大小")]
        public int VScrollBar_ButtonSize
        {
            get { return hvScrollBarExt1.ButtonSize; }
            set { hvScrollBarExt1.ButtonSize = value; }
        }

        #endregion 属性定义

        public HRichTextbox()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor, true);
            base.UpdateStyles();
            this.RadiusChanged += PPRichTextbox_RadiusChanged;
            hvScrollBarExt1.BindingControl(richTextBox1);
            hvScrollBarExt1.BringToFront();
        }

        /// <summary>PPRichTextbox_RadiusChanged 方法。</summary>
        private void PPRichTextbox_RadiusChanged(object sender, EventArgs e)
        {
            ResetSize();
        }

        /// <summary>清空。</summary>
        public void Clear()
        {
            richTextBox1.Invoke(new MethodInvoker(() => { richTextBox1.Clear(); }));
        }

        /// <summary>AppendText 方法。</summary>
        public void AppendText(string text)
        {
            richTextBox1.Invoke(new MethodInvoker(() => { richTextBox1.AppendText(text); }));
        }

        /// <summary>ScrollToCaret 方法。</summary>
        public void ScrollToCaret()
        {
            richTextBox1.Invoke(new MethodInvoker(() => { richTextBox1.ScrollToCaret(); }));
        }

        /// <summary>ResetSize 方法。</summary>
        private void ResetSize()
        {
            richTextBox1.Size = new Size((int)(this.Width - 0.3 * Radius - 2 * BorderWidth) - 2, (int)(this.Height - 0.3 * Radius - 2 * BorderWidth) - 2);
            richTextBox1.Location = new Point((int)(BorderWidth + 0.15 * Radius + 1), (int)(BorderWidth + 0.15 * Radius + 1));
            hvScrollBarExt1.Location = new Point(richTextBox1.Right - hvScrollBarExt1.Width, richTextBox1.Top);
            hvScrollBarExt1.Size = new Size(hvScrollBarExt1.Width, richTextBox1.Height);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ResetSize();
        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            ResetSize();
        }

        /// <summary>PPRichTextbox_BaseColorChanged 方法。</summary>
        private void PPRichTextbox_BaseColorChanged(object sender, EventArgs e)
        {
            richTextBox1.BackColor = this.BaseColor;
        }

        [Browsable(true), Bindable(true)]
        public new event EventHandler TextChanged;

        /// <summary>richTextBox1_TextChanged 方法。</summary>
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
            {
                TextChanged(this, new EventArgs());
            }
        }
    }
}