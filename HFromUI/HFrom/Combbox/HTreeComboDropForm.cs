using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HFrom.Base;
using HFromUI.HFrom.Tables;
using HFromUI.HFrom.Text;

namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HLangage;
    using HFromUI.HFrom.HUiKit;
    /// <summary>
    /// HTreeComboBox 的下拉弹出窗。
    /// 顶部可放置 HSearchBox，主体为 HTreeViewEx。
    /// 双击或回车选中节点后引发 NodeSelected 事件并关闭。
    /// </summary>
    internal partial class HTreeComboDropForm : HAnchorBase
    {
        #region 字段

        /// <summary>showSearch 字段。</summary>
        private bool showSearch = true;
        /// <summary>maxItems 字段。</summary>
        private int maxItems = 10;

        #endregion

        #region 属性

        /// <summary>内部树控件</summary>
        [Browsable(false)]
        public HTreeViewEx Tree
        {
            get { return tree; }
        }

        /// <summary>是否显示搜索框</summary>
        [Browsable(false)]
        public bool ShowSearch
        {
            get { return showSearch; }
            set
            {
                showSearch = value;
                if (searchBox != null)
                {
                    searchBox.Visible = value;
                }
            }
        }

        /// <summary>最大可显示项数</summary>
        [Browsable(false)]
        public int MaxItems
        {
            get { return maxItems; }
            set { maxItems = value; }
        }

        #endregion

        #region 事件

        /// <summary>节点被选定时触发（双击或回车）</summary>
        public event EventHandler<TreeViewEventArgs> NodeSelected;

        #endregion

        #region 构造

        /// <summary>构造下拉弹出窗</summary>
        /// <param name="baseControl">触发的宿主控件（HTreeComboBox）</param>
        /// <param name="dropDownWidth">下拉宽度</param>
        /// <param name="dropDownHeight">下拉高度</param>
        /// <param name="showSearch">是否显示搜索框</param>
        public HTreeComboDropForm(Control baseControl, int dropDownWidth, int dropDownHeight, bool showSearch)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            base.BaseControl = baseControl;

            // 无边框弹窗
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            // 主题色
            this.BackColor = HUiTheme.CardBg;
            this.BorderColor = HUiTheme.Border;
            this.Radius = 4;

            // 搜索框样式
            searchBox.BackColor = HUiTheme.CardBg;
            searchBox.BaseColor = HUiTheme.InputBg;
            searchBox.BorderColor = HUiTheme.Border;
            searchBox.BorderWidth = 0F;
            searchBox.Radius = 0;
            searchBox.ShowBorder = false;
            searchBox.WatermarkText = HTranslation.GetContent("搜索...");
            searchBox.Visible = showSearch;
            searchBox.Search += SearchBox_Search;

            // 树样式
            tree.BackColor = HUiTheme.CardBg;
            tree.ForeColor = HUiTheme.TextMain;
            tree.BorderStyle = BorderStyle.None;
            tree.Font = HUiTheme.FontUi;
            tree.AfterSelect += Tree_AfterSelect;
            tree.DoubleClick += Tree_DoubleClick;
            tree.KeyDown += Tree_KeyDown;

            // 设置大小
            this.Width = dropDownWidth > 0 ? dropDownWidth : 200;
            this.Height = dropDownHeight > 0 ? dropDownHeight : 300;

            this.showSearch = showSearch;
        }

        #endregion

        #region 显示定位

        /// <summary>响应 Shown 事件。</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // HAnchorBase.OnVisibleChanged 已完成定位（SetDropLocation），
            // 这里再次校准宽度，确保宽度不低于宿主控件宽度
            if (BaseControl != null && this.Width < BaseControl.Width)
            {
                this.Width = BaseControl.Width;
            }
            ApplyLayout();
            // 让树获得焦点，便于键盘选择
            this.tree.Focus();
        }

        /// <summary>ApplyLayout 方法。</summary>
        private void ApplyLayout()
        {
            int searchH = showSearch ? searchBox.Height : 0;
            searchBox.Visible = showSearch;
            tree.Location = new Point(0, searchH);
            tree.Width = this.ClientSize.Width;
            tree.Height = this.ClientSize.Height - searchH;
            tree.Invalidate();
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            ApplyLayout();
        }

        #endregion

        #region 搜索

        /// <summary>SearchBox_Search 方法。</summary>
        private void SearchBox_Search(object sender, HSearchEventArgs e)
        {
            tree.SetFilter(e.SearchText);
        }

        #endregion

        #region 节点选择

        /// <summary>Tree_AfterSelect 方法。</summary>
        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // 单选模式下，单次点击也可选中并关闭
            HTreeComboBox host = BaseControl as HTreeComboBox;
            if (host != null && !host.AllowMultiSelect)
            {
                RaiseNodeSelected(e);
            }
        }

        /// <summary>Tree_DoubleClick 方法。</summary>
        private void Tree_DoubleClick(object sender, EventArgs e)
        {
            if (tree.SelectedNode != null)
            {
                RaiseNodeSelected(new TreeViewEventArgs(tree.SelectedNode, TreeViewAction.ByMouse));
            }
        }

        /// <summary>Tree_KeyDown 方法。</summary>
        private void Tree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tree.SelectedNode != null)
            {
                RaiseNodeSelected(new TreeViewEventArgs(tree.SelectedNode, TreeViewAction.ByKeyboard));
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>RaiseNodeSelected 方法。</summary>
        private void RaiseNodeSelected(TreeViewEventArgs e)
        {
            if (NodeSelected != null)
            {
                NodeSelected(this, e);
            }
        }

        #endregion
    }
}
