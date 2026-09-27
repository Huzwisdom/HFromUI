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
    /// 数据筛选应用事件参数
    /// </summary>
    public class HFilterAppliedEventArgs : EventArgs
    {
        /// <summary>选中的值集合，为 null 表示清除筛选</summary>
        public HashSet<string> CheckedValues { get; private set; }

        public HFilterAppliedEventArgs(HashSet<string> checkedValues)
        {
            CheckedValues = checkedValues;
        }
    }

    /// <summary>
    /// Excel 风格的列自动筛选下拉窗体
    /// </summary>
    public partial class HDataFilterForm : HAnchorBase
    {
        #region 主题颜色

        /// <summary>bgColor 字段。</summary>
        private Color bgColor = Color.FromArgb(45, 45, 48);
        /// <summary>textColor 字段。</summary>
        private Color textColor = Color.White;
        /// <summary>buttonColor 字段。</summary>
        private Color buttonColor = Color.FromArgb(60, 60, 64);
        /// <summary>buttonHoverColor 字段。</summary>
        private Color buttonHoverColor = Color.FromArgb(80, 80, 84);
        /// <summary>buttonActiveColor 字段。</summary>
        private Color buttonActiveColor = Color.FromArgb(0, 120, 215);
        /// <summary>listBackColor 字段。</summary>
        private Color listBackColor = Color.FromArgb(30, 30, 33);

        #endregion

        #region 控件字段

        private Panel pnlTop;
        private Panel pnlBottom;
        private CheckedListBox clbValues;
        private Button btnSelectAll;
        private Button btnClearAll;
        private Button btnOK;
        private Button btnClear;

        #endregion

        #region 字段

        /// <summary>allValues 字段。</summary>
        private HashSet<string> allValues = new HashSet<string>();

        #endregion

        #region 事件

        /// <summary>
        /// 筛选应用事件，参数 CheckedValues 为 null 表示清除筛选
        /// </summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("筛选应用事件"), HDescriptionLanguage("筛选应用事件"), Browsable(true)]
        public event EventHandler<HFilterAppliedEventArgs> FilterApplied;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseControl">需要停靠在下方的绑定控件</param>
        public HDataFilterForm(Control baseControl)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            // 设置停靠绑定的控件
            this.BaseControl = baseControl;

            InitControls();
            LayoutControls();
            this.SizeChanged += (s, e) => LayoutControls();
        }

        /// <summary>InitControls 方法。</summary>
        private void InitControls()
        {
            // 顶部按钮面板：全选 / 取消全选
            pnlTop = new Panel
            {
                Height = 34,
                Dock = DockStyle.Top,
                BackColor = bgColor
            };

            btnSelectAll = CreateButton(HTranslation.GetContent("全选"));
            btnClearAll = CreateButton(HTranslation.GetContent("取消全选"));
            pnlTop.Controls.Add(btnClearAll);
            pnlTop.Controls.Add(btnSelectAll);

            // 中间复选框列表
            clbValues = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BackColor = listBackColor,
                ForeColor = textColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("微软雅黑", 9),
                CheckOnClick = true,
                ScrollAlwaysVisible = false,
                IntegralHeight = false
            };

            // 底部按钮面板：确定 / 清除
            pnlBottom = new Panel
            {
                Height = 38,
                Dock = DockStyle.Bottom,
                BackColor = bgColor
            };

            btnOK = CreateButton(HTranslation.GetContent("确定"), true);
            btnClear = CreateButton(HTranslation.GetContent("清除"));
            pnlBottom.Controls.Add(btnClear);
            pnlBottom.Controls.Add(btnOK);

            // 停靠顺序：先 Fill，再 Bottom，最后 Top，保证布局正确
            this.Controls.Add(clbValues);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);

            // 事件绑定
            btnSelectAll.Click += BtnSelectAll_Click;
            btnClearAll.Click += BtnClearAll_Click;
            btnOK.Click += BtnOK_Click;
            btnClear.Click += BtnClear_Click;
        }

        /// <summary>CreateButton 方法。</summary>
        private Button CreateButton(string text, bool primary = false)
        {
            var btn = new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 9),
                BackColor = primary ? buttonActiveColor : buttonColor,
                ForeColor = textColor,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            if (!primary)
            {
                btn.MouseEnter += (s, e) => btn.BackColor = buttonHoverColor;
                btn.MouseLeave += (s, e) => btn.BackColor = buttonColor;
            }
            return btn;
        }

        /// <summary>LayoutControls 方法。</summary>
        private void LayoutControls()
        {
            if (pnlTop == null || pnlBottom == null) return;

            // 顶部两个按钮并排
            int topH = pnlTop.Height - 4;
            btnSelectAll.Location = new Point(2, 2);
            btnSelectAll.Size = new Size((pnlTop.Width - 6) / 2, topH);
            btnClearAll.Location = new Point(btnSelectAll.Right + 2, 2);
            btnClearAll.Size = new Size(pnlTop.Width - btnClearAll.Left - 2, topH);

            // 底部两个按钮并排
            int bottomH = pnlBottom.Height - 4;
            btnOK.Location = new Point(2, 2);
            btnOK.Size = new Size((pnlBottom.Width - 6) / 2, bottomH);
            btnClear.Location = new Point(btnOK.Right + 2, 2);
            btnClear.Size = new Size(pnlBottom.Width - btnClear.Left - 2, bottomH);
        }

        #region 公开方法

        /// <summary>
        /// 设置下拉列表的可选值及默认选中项
        /// </summary>
        /// <param name="values">所有可选值</param>
        /// <param name="initiallyChecked">初始选中的值集合，可为 null</param>
        public void SetValues(IEnumerable<string> values, HashSet<string> initiallyChecked)
        {
            clbValues.BeginUpdate();
            clbValues.Items.Clear();
            allValues.Clear();
            if (values != null)
            {
                foreach (var v in values)
                {
                    if (v == null) continue;
                    allValues.Add(v);
                    bool isChecked = initiallyChecked != null && initiallyChecked.Contains(v);
                    clbValues.Items.Add(v, isChecked);
                }
            }
            clbValues.EndUpdate();
        }

        /// <summary>
        /// 获取当前选中的值集合
        /// </summary>
        /// <returns>选中值的集合</returns>
        public HashSet<string> GetChecked()
        {
            var result = new HashSet<string>();
            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                if (clbValues.GetItemChecked(i))
                {
                    string item = clbValues.Items[i] as string;
                    if (item != null)
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        #endregion

        #region 事件处理

        /// <summary>BtnSelectAll_Click 方法。</summary>
        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                clbValues.SetItemChecked(i, true);
            }
        }

        /// <summary>BtnClearAll_Click 方法。</summary>
        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbValues.Items.Count; i++)
            {
                clbValues.SetItemChecked(i, false);
            }
        }

        /// <summary>BtnOK_Click 方法。</summary>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            var checkedValues = GetChecked();
            OnFilterApplied(new HFilterAppliedEventArgs(checkedValues));
            this.Close();
        }

        /// <summary>BtnClear_Click 方法。</summary>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            // null 表示清除筛选
            OnFilterApplied(new HFilterAppliedEventArgs(null));
            this.Close();
        }

        /// <summary>
        /// 触发筛选应用事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected virtual void OnFilterApplied(HFilterAppliedEventArgs e)
        {
            EventHandler<HFilterAppliedEventArgs> handler = FilterApplied;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion
    }
}