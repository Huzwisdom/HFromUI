using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Tables
{
    using HFromUI.HLangage;
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 分页导航条
    /// 参考 DevExpress PaginationControl 和 SunnyUI UIPagination
    /// </summary>
    [DefaultEvent("PageChanged")]
    [ToolboxItem(true)]
    public partial class HPagination : HControlBase
    {
        #region 控件

        private Button btnFirst;
        private Button btnPrev;
        private Button btnNext;
        private Button btnLast;
        private ComboBox cmbPageSize;
        private Label lblInfo;
        private NumericUpDown nudJump;
        private Button btnJump;
        private Panel pnlNumbers;

        #endregion

        #region 字段

        /// <summary>currentPage 字段。</summary>
        private int currentPage = 1;
        /// <summary>pageSize 字段。</summary>
        private int pageSize = 20;
        /// <summary>totalRecords 字段。</summary>
        private int totalRecords = 0;
        /// <summary>pageCount 字段。</summary>
        private int pageCount = 0;
        /// <summary>pageSizeOptions 字段。</summary>
        private int[] pageSizeOptions = { 10, 20, 50, 100 };
        /// <summary>maxVisiblePages 字段。</summary>
        private int maxVisiblePages = 9;
        /// <summary>buttonColor 字段。</summary>
        private Color buttonColor = Color.FromArgb(45, 45, 48);
        /// <summary>buttonHoverColor 字段。</summary>
        private Color buttonHoverColor = Color.FromArgb(70, 70, 74);
        /// <summary>buttonActiveColor 字段。</summary>
        private Color buttonActiveColor = Color.FromArgb(0, 120, 215);
        /// <summary>textColor 字段。</summary>
        private Color textColor = Color.White;
        /// <summary>pageButtons 字段。</summary>
        private readonly List<Control> pageButtons = new List<Control>();

        #endregion

        #region 事件

        [HCategoryLanguage("通用"), HDisplayNameLanguage("页码变化事件"), HDescriptionLanguage("页码变化事件"), Browsable(true)]
        public event EventHandler<HPagingEventArgs> PageChanged;

        #endregion

        #region 属性

        [HCategoryLanguage("通用"), HDisplayNameLanguage("当前页码"), HDescriptionLanguage("当前页码(1开始)"), Browsable(true)]
        public int CurrentPage
        {
            get { return currentPage; }
            set
            {
                int v = Math.Max(1, Math.Min(value, Math.Max(1, pageCount)));
                if (currentPage != v)
                {
                    currentPage = v;
                    RefreshUI();
                    OnPageChanged();
                }
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("每页记录数"), HDescriptionLanguage("每页记录数"), Browsable(true)]
        public int PageSize
        {
            get { return pageSize; }
            set
            {
                if (pageSize != value)
                {
                    pageSize = Math.Max(1, value);
                    pageCount = (int)Math.Ceiling((double)totalRecords / pageSize);
                    currentPage = Math.Min(currentPage, Math.Max(1, pageCount));
                    RefreshUI();
                    OnPageChanged();
                }
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("总记录数"), HDescriptionLanguage("总记录数"), Browsable(true)]
        public int TotalRecords
        {
            get { return totalRecords; }
            set
            {
                totalRecords = Math.Max(0, value);
                pageCount = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (currentPage > pageCount) currentPage = Math.Max(1, pageCount);
                RefreshUI();
            }
        }

        /// <summary>PageCount 成员。</summary>
        /// <summary>PageCount 字段。</summary>
        [Browsable(false)]
        public int PageCount => pageCount;

        /// <summary>Offset 成员。</summary>
        /// <summary>Offset 字段。</summary>
        [Browsable(false)]
        public int Offset => (currentPage - 1) * pageSize;

        /// <summary>Limit 成员。</summary>
        /// <summary>Limit 字段。</summary>
        [Browsable(false)]
        public int Limit => pageSize;

        #endregion

        public HPagination()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            InitControls();
            this.SizeChanged += (s, e) => LayoutControls();
            this.Height = 40;
            LayoutControls();
            RefreshUI();
        }

        /// <summary>InitControls 方法。</summary>
        private void InitControls()
        {
            // 首页
            btnFirst = CreateNavButton("<<");
            btnFirst.Click += (s, e) => CurrentPage = 1;

            // 上页
            btnPrev = CreateNavButton("<");
            btnPrev.Click += (s, e) => CurrentPage = currentPage - 1;

            // 下页
            btnNext = CreateNavButton(">");
            btnNext.Click += (s, e) => CurrentPage = currentPage + 1;

            // 末页
            btnLast = CreateNavButton(">>");
            btnLast.Click += (s, e) => CurrentPage = pageCount;

            // 页大小选择
            cmbPageSize = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 9),
                Size = new Size(60, 24)
            };
            foreach (var ps in pageSizeOptions)
                cmbPageSize.Items.Add(ps.ToString());
            cmbPageSize.SelectedIndex = Array.IndexOf(pageSizeOptions, pageSize);
            if (cmbPageSize.SelectedIndex < 0) cmbPageSize.SelectedIndex = 1;
            cmbPageSize.SelectedIndexChanged += (s, e) =>
            {
                if (cmbPageSize.SelectedItem != null && int.TryParse(cmbPageSize.SelectedItem.ToString(), out int ps))
                    PageSize = ps;
            };

            // 信息标签
            lblInfo = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9),
                ForeColor = textColor
            };

            // 跳转
            nudJump = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 999999,
                Width = 50,
                Font = new Font("微软雅黑", 9)
            };

            btnJump = CreateNavButton(HTranslation.GetContent("跳转"));
            btnJump.Click += (s, e) => CurrentPage = (int)nudJump.Value;

            // 页码面板
            pnlNumbers = new Panel
            {
                AutoSize = false,
                BackColor = Color.Transparent
            };

            this.Controls.Add(btnFirst);
            this.Controls.Add(btnPrev);
            this.Controls.Add(pnlNumbers);
            this.Controls.Add(btnNext);
            this.Controls.Add(btnLast);
            this.Controls.Add(cmbPageSize);
            this.Controls.Add(lblInfo);
            this.Controls.Add(nudJump);
            this.Controls.Add(btnJump);
        }

        /// <summary>CreateNavButton 方法。</summary>
        private Button CreateNavButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(32, 28),
                Font = new Font("微软雅黑", 9),
                BackColor = buttonColor,
                ForeColor = textColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = buttonHoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = buttonColor;
            return btn;
        }

        /// <summary>LayoutControls 方法。</summary>
        private void LayoutControls()
        {
            if (btnFirst == null) return;
            int x = 4;
            int y = (this.Height - 28) / 2;

            btnFirst.Location = new Point(x, y); x += 36;
            btnPrev.Location = new Point(x, y); x += 36;

            // 页码面板占据中间空间
            int pnlX = x;
            int pnlW = Math.Max(120, this.Width - x - 36 - 36 - 70 - 100 - 60 - 60 - 8);
            pnlNumbers.Location = new Point(pnlX, y);
            pnlNumbers.Size = new Size(pnlW, 28);
            x = pnlX + pnlW;

            btnNext.Location = new Point(x, y); x += 36;
            btnLast.Location = new Point(x, y); x += 36;

            cmbPageSize.Location = new Point(x, y - 2); x += 64;
            lblInfo.Location = new Point(x, y);
            lblInfo.Size = new Size(100, 28); x += 104;

            nudJump.Location = new Point(x, y - 2); x += 54;
            btnJump.Location = new Point(x, y); x += 60;
        }

        /// <summary>刷新所有UI状态</summary>
        public void RefreshUI()
        {
            if (lblInfo == null) return;

            lblInfo.Text = HTranslation.GetContent($"共 {totalRecords} 条  第 {currentPage}/{Math.Max(1, pageCount)} 页");

            btnFirst.Enabled = currentPage > 1;
            btnPrev.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < pageCount;
            btnLast.Enabled = currentPage < pageCount;

            nudJump.Maximum = Math.Max(1, pageCount);
            if (nudJump.Value > nudJump.Maximum) nudJump.Value = nudJump.Maximum;

            RebuildPageButtons();
            Invalidate();
        }

        /// <summary>RebuildPageButtons 方法。</summary>
        private void RebuildPageButtons()
        {
            // 清除旧按钮和省略号标签（统一为 Control，避免 null 占位导致空引用）
            foreach (var ctl in pageButtons)
            {
                if (ctl != null)
                {
                    pnlNumbers.Controls.Remove(ctl);
                    ctl.Dispose();
                }
            }
            pageButtons.Clear();

            if (pageCount <= 0) return;

            // 计算可见页码范围
            int startPage, endPage;
            if (pageCount <= maxVisiblePages)
            {
                startPage = 1;
                endPage = pageCount;
            }
            else
            {
                int half = maxVisiblePages / 2;
                startPage = Math.Max(1, currentPage - half);
                endPage = Math.Min(pageCount, startPage + maxVisiblePages - 1);
                if (endPage - startPage + 1 < maxVisiblePages)
                    startPage = Math.Max(1, endPage - maxVisiblePages + 1);
            }

            int x = 0;
            // 前省略号
            if (startPage > 1)
            {
                var lbl = new Label
                {
                    Text = "...",
                    AutoSize = false,
                    Size = new Size(20, 28),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("微软雅黑", 9),
                    ForeColor = textColor
                };
                lbl.Location = new Point(x, 0);
                pnlNumbers.Controls.Add(lbl);
                pageButtons.Add(lbl);
                x += 22;

                // 第1页按钮
                x = AddPageButton(1, x);
                if (startPage > 2)
                {
                    var lbl2 = new Label
                    {
                        Text = "...",
                        AutoSize = false,
                        Size = new Size(20, 28),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("微软雅黑", 9),
                        ForeColor = textColor
                    };
                    lbl2.Location = new Point(x, 0);
                    pnlNumbers.Controls.Add(lbl2);
                    pageButtons.Add(lbl2);
                    x += 22;
                }
            }

            // 页码按钮
            for (int i = startPage; i <= endPage; i++)
            {
                x = AddPageButton(i, x);
            }

            // 后省略号
            if (endPage < pageCount)
            {
                if (endPage < pageCount - 1)
                {
                    var lbl = new Label
                    {
                        Text = "...",
                        AutoSize = false,
                        Size = new Size(20, 28),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("微软雅黑", 9),
                        ForeColor = textColor
                    };
                    lbl.Location = new Point(x, 0);
                    pnlNumbers.Controls.Add(lbl);
                    pageButtons.Add(lbl);
                    x += 22;
                }
                x = AddPageButton(pageCount, x);
            }
        }

        /// <summary>AddPageButton 方法。</summary>
        private int AddPageButton(int pageNum, int x)
        {
            var btn = new Button
            {
                Text = pageNum.ToString(),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(28, 28),
                Font = new Font("微软雅黑", 9),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Tag = pageNum
            };
            btn.FlatAppearance.BorderSize = 0;

            if (pageNum == currentPage)
            {
                btn.BackColor = buttonActiveColor;
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = buttonColor;
                btn.ForeColor = textColor;
                btn.MouseEnter += (s, e) => btn.BackColor = buttonHoverColor;
                btn.MouseLeave += (s, e) => btn.BackColor = buttonColor;
            }
            btn.Click += (s, e) =>
            {
                int p = (int)btn.Tag;
                if (p != currentPage)
                {
                    currentPage = p;
                    RefreshUI();
                    OnPageChanged();
                }
            };
            btn.Location = new Point(x, 0);
            pnlNumbers.Controls.Add(btn);
            pageButtons.Add(btn);
            return x + 30;
        }

        /// <summary>响应 PageChanged 事件。</summary>
        protected virtual void OnPageChanged()
        {
            PageChanged?.Invoke(this, new HPagingEventArgs(currentPage, pageSize, Offset, Limit));
        }
    }

    /// <summary>分页事件参数</summary>
    public class HPagingEventArgs : EventArgs
    {
        /// <summary>Page 成员。</summary>
        public int Page { get; }
        /// <summary>PageSize 成员。</summary>
        public int PageSize { get; }
        /// <summary>Offset 成员。</summary>
        public int Offset { get; }
        /// <summary>Limit 成员。</summary>
        public int Limit { get; }

        public HPagingEventArgs(int page, int pageSize, int offset, int limit)
        {
            Page = page;
            PageSize = pageSize;
            Offset = offset;
            Limit = limit;
        }
    }
}