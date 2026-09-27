using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace HFromUI.HFrom.Text
{
    using HFromUI.HLangage;
    using HFromUI.HFrom.Base;
    /// <summary>
    /// 带防抖功能的搜索输入框
    /// 参考 DevExpress SearchControl 和 SunnyUI UISearchBox
    /// </summary>
    [DefaultEvent("Search")]
    [ToolboxItem(true)]
    public partial class HSearchBox : HControlBase
    {
        #region 组件
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Button btnClear;
        #endregion
        #region 字段
        /// <summary>debounceMs 字段。</summary>
        private int debounceMs = 300;
        private System.Windows.Forms.Timer debounceTimer;
        /// <summary>showClearButton 字段。</summary>
        private bool showClearButton = true;
        /// <summary>watermarkText 字段。</summary>
        private string watermarkText = HTranslation.GetContent("搜索...");
        /// <summary>watermarkColor 字段。</summary>
        private Color watermarkColor = Color.Gray;
        /// <summary>iconColor 字段。</summary>
        private Color iconColor = Color.Gray;
        #endregion
        #region 事件
        /// <summary>搜索事件（防抖后触发或 Enter 即时触发）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("搜索事件"), HDescriptionLanguage("搜索事件"), Browsable(true)]
        public event EventHandler<HSearchEventArgs> Search;
        #endregion
        #region 属性
        [HCategoryLanguage("通用"), HDisplayNameLanguage("防抖延迟毫秒数"), HDescriptionLanguage("防抖延迟毫秒数"), Browsable(true)]
        public int DebounceMs
        {
            get { return debounceMs; }
            set { debounceMs = Math.Max(0, value); }
        }
        [HCategoryLanguage("通用"), HDisplayNameLanguage("搜索文本"), HDescriptionLanguage("搜索文本"), Browsable(true)]
        public string SearchText
        {
            get { return textBox?.Text ?? string.Empty; }
            set { if (textBox != null) textBox.Text = value; }
        }
        [HCategoryLanguage("通用"), HDisplayNameLanguage("水印文字"), HDescriptionLanguage("水印文字"), Browsable(true)]
        public string WatermarkText
        {
            get { return watermarkText; }
            set { watermarkText = value; Invalidate(); }
        }
        [HCategoryLanguage("通用"), HDisplayNameLanguage("水印颜色"), HDescriptionLanguage("水印颜色"), Browsable(true)]
        public Color WatermarkColor
        {
            get { return watermarkColor; }
            set { watermarkColor = value; Invalidate(); }
        }
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示清除按钮"), HDescriptionLanguage("是否显示清除按钮"), Browsable(true)]
        public bool ShowClearButton
        {
            get { return showClearButton; }
            set
            {
                showClearButton = value;
                if (btnClear != null) btnClear.Visible = value && textBox.Text.Length > 0;
            }
        }
        public override string Text
        {
            get { return SearchText; }
            set { SearchText = value; }
        }
        #endregion
        public HSearchBox()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            // 初始化内部控件
            textBox = new System.Windows.Forms.TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(28, 8),
                Width = this.Width - 56,
                Font = new Font("微软雅黑", 10),
                BackColor = this.BackColor,
                ForeColor = this.ForeColor
            };
            textBox.TextChanged += OnTextBoxTextChanged;
            textBox.KeyDown += OnTextBoxKeyDown;
            btnClear = new System.Windows.Forms.Button
            {
                Text = "×",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(20, 20),
                Cursor = Cursors.Hand,
                BackColor = this.BackColor,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) =>
            {
                textBox.Text = string.Empty;
                OnSearchRequested();
            };
            this.Controls.Add(textBox);
            this.Controls.Add(btnClear);
            debounceTimer = new System.Windows.Forms.Timer { Interval = debounceMs };
            debounceTimer.Tick += (s, e) =>
            {
                debounceTimer.Stop();
                OnSearchRequested();
            };
            this.SizeChanged += (s, e) => LayoutControls();
            LayoutControls();
        }
        /// <summary>LayoutControls 方法。</summary>
        private void LayoutControls()
        {
            if (textBox == null || btnClear == null) return;
            int h = this.Height;
            int textH = textBox.Font.Height;
            int textY = (h - textH) / 2;
            textBox.Location = new Point(28, textY);
            textBox.Width = this.Width - 28 - 24 - 4;
            btnClear.Location = new Point(this.Width - 24, (h - 20) / 2);
        }
        /// <summary>响应 TextBoxTextChanged 事件。</summary>
        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            if (btnClear != null) btnClear.Visible = showClearButton && textBox.Text.Length > 0;
            if (debounceMs > 0)
            {
                debounceTimer.Stop();
                debounceTimer.Interval = debounceMs;
                debounceTimer.Start();
            }
            else
            {
                OnSearchRequested();
            }
        }
        /// <summary>响应 TextBoxKeyDown 事件。</summary>
        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                debounceTimer.Stop();
                OnSearchRequested();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                textBox.Text = string.Empty;
                debounceTimer.Stop();
                OnSearchRequested();
                e.SuppressKeyPress = true;
            }
        }
        /// <summary>立即触发搜索（跳过防抖）</summary>
        public void SearchNow()
        {
            debounceTimer?.Stop();
            OnSearchRequested();
        }
        /// <summary>响应 SearchRequested 事件。</summary>
        protected virtual void OnSearchRequested()
        {
            Search?.Invoke(this, new HSearchEventArgs(textBox?.Text ?? string.Empty));
        }
        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            // 绘制搜索图标（放大镜）
            using (Pen iconPen = new Pen(iconColor, 1.5f))
            {
                int cx = 10, cy = this.Height / 2;
                g.DrawEllipse(iconPen, cx - 3, cy - 4, 8, 8);
                g.DrawLine(iconPen, cx + 5, cy + 4, cx + 8, cy + 7);
            }
            // 绘制水印
            if (textBox != null && string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrEmpty(watermarkText))
            {
                using (SolidBrush wb = new SolidBrush(watermarkColor))
                {
                    g.DrawString(watermarkText, textBox.Font, wb, new Rectangle(28, 0, this.Width - 52, this.Height),
                        new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                }
            }
        }
    }
    /// <summary>搜索事件参数</summary>
    public class HSearchEventArgs : EventArgs
    {
        /// <summary>SearchText 成员。</summary>
        public string SearchText { get; }
        public HSearchEventArgs(string searchText) { SearchText = searchText; }
    }
}