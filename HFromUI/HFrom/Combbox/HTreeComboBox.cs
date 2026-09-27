using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HLangage;
    using HFromUI.HFrom.HUiKit;
    using HFromUI.HFrom.Tables;
    /// <summary>
    /// 树形下拉选择框（Telerik RadDropDownTree 风格）。
    /// 内部由只读文本框 + 下拉箭头组成，点击展开 HTreeComboDropForm 弹出窗。
    /// </summary>
    [DefaultEvent("AfterSelect")]
    [ToolboxItem(true)]
    public partial class HTreeComboBox : HControlBase
    {
        #region 字段

        // 当前选中的节点（单选）
        private TreeNode selectedNode;
        // 多选时保存的节点列表
        private readonly System.Collections.Generic.List<TreeNode> selectedNodes = new System.Collections.Generic.List<TreeNode>();
        // 下拉弹出窗
        private HTreeComboDropForm dropForm;

        /// <summary>allowMultiSelect 字段。</summary>
        private bool allowMultiSelect = false;
        /// <summary>showSearch 字段。</summary>
        private bool showSearch = true;
        /// <summary>dropDownWidth 字段。</summary>
        private int dropDownWidth = 200;
        /// <summary>dropDownHeight 字段。</summary>
        private int dropDownHeight = 300;
        /// <summary>maxDropDownItems 字段。</summary>
        private int maxDropDownItems = 10;

        #endregion

        #region 属性

        /// <summary>当前选中的节点</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("当前选中的节点"), HDescriptionLanguage("当前选中的节点")]
        [Browsable(false)]
        public TreeNode SelectedNode
        {
            get { return selectedNode; }
            set
            {
                selectedNode = value;
                UpdateDisplayText();
            }
        }

        /// <summary>选中节点的值（取自节点的 Tag）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中节点的值"), HDescriptionLanguage("选中节点的值")]
        [Browsable(false)]
        public object SelectedValue
        {
            get
            {
                return selectedNode != null ? selectedNode.Tag : null;
            }
        }

        /// <summary>选中节点的路径（从根到选中节点的文本，以斜杠连接）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中节点的路径"), HDescriptionLanguage("选中节点的路径")]
        [Browsable(false)]
        public string SelectedPath
        {
            get
            {
                if (selectedNode == null)
                {
                    return string.Empty;
                }
                return BuildPath(selectedNode);
            }
        }

        /// <summary>是否允许多选</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否允许多选"), HDescriptionLanguage("是否允许多选"), Browsable(true)]
        [DefaultValue(false)]
        public bool AllowMultiSelect
        {
            get { return allowMultiSelect; }
            set { allowMultiSelect = value; }
        }

        /// <summary>下拉窗是否显示搜索框</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("下拉窗是否显示搜索框"), HDescriptionLanguage("下拉窗是否显示搜索框"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowSearch
        {
            get { return showSearch; }
            set { showSearch = value; }
        }

        /// <summary>下拉窗宽度</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("下拉窗宽度"), HDescriptionLanguage("下拉窗宽度"), Browsable(true)]
        [DefaultValue(200)]
        public int DropDownWidth
        {
            get { return dropDownWidth; }
            set { dropDownWidth = value; }
        }

        /// <summary>下拉窗高度</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("下拉窗高度"), HDescriptionLanguage("下拉窗高度"), Browsable(true)]
        [DefaultValue(300)]
        public int DropDownHeight
        {
            get { return dropDownHeight; }
            set { dropDownHeight = value; }
        }

        /// <summary>下拉窗最大可显示项数</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("下拉最大项数"), HDescriptionLanguage("下拉窗最大可显示项数"), Browsable(true)]
        [DefaultValue(10)]
        public int MaxDropDownItems
        {
            get { return maxDropDownItems; }
            set { maxDropDownItems = value; }
        }

        /// <summary>内部树节点集合（委托给内部 TreeView）</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("树节点集合"), HDescriptionLanguage("树节点集合"), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TreeNodeCollection Nodes
        {
            get { return innerTreeView.Nodes; }
        }

        /// <summary>显示文本</summary>
        [Browsable(false)]
        public string DisplayText
        {
            get { return txtDisplay.Text; }
        }

        #endregion

        #region 事件

        /// <summary>节点选中后事件</summary>
        [HDescriptionLanguage("节点选中后事件")]
        public event TreeViewEventHandler AfterSelect;

        #endregion

        #region 构造

        public HTreeComboBox()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            this.txtDisplay.Font = HUiTheme.FontUi;
            this.txtDisplay.ForeColor = HUiTheme.TextMain;
            this.txtDisplay.BackColor = HUiTheme.InputBg;

            this.BackColor = HUiTheme.CardBg;
            this.BaseColor = HUiTheme.InputBg;
            this.BorderColor = HUiTheme.Border;
            this.BorderWidth = 1F;
            this.Radius = 4;

            this.btnArrow.ForeColor = HUiTheme.TextSub;
            this.btnArrow.Text = "▾";

            // 内部 TreeView 仅作为节点容器，不显示
            innerTreeView.Visible = false;

            this.SizeChangeded += HTreeComboBox_SizeChangeded;
            this.BaseColorChanged += HTreeComboBox_BaseColorChanged;
            this.SizeChanged += HTreeComboBox_SizeChanged;
        }

        #endregion

        #region 布局

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // 双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        /// <summary>HTreeComboBox_SizeChanged 方法。</summary>
        private void HTreeComboBox_SizeChanged(object sender, EventArgs e)
        {
            LayoutChildren();
        }

        /// <summary>HTreeComboBox_SizeChangeded 方法。</summary>
        private void HTreeComboBox_SizeChangeded(object sender, EventArgs e)
        {
            LayoutChildren();
        }

        /// <summary>HTreeComboBox_BaseColorChanged 方法。</summary>
        private void HTreeComboBox_BaseColorChanged(object sender, EventArgs e)
        {
            txtDisplay.BackColor = BaseColor;
        }

        /// <summary>LayoutChildren 方法。</summary>
        private void LayoutChildren()
        {
            int arrowSize = 18;
            int padding = 6;
            btnArrow.Location = new Point(this.Width - arrowSize - padding, (this.Height - arrowSize) / 2);
            btnArrow.Size = new Size(arrowSize, arrowSize);
            txtDisplay.Location = new Point(padding + 2, (this.Height - txtDisplay.PreferredHeight) / 2);
            txtDisplay.Width = this.Width - arrowSize - padding * 2 - 4;
            txtDisplay.BackColor = BaseColor;
            this.Invalidate();
        }

        /// <summary>响应 FontChanged 事件。</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            txtDisplay.Font = base.Font;
            base.OnFontChanged(e);
            LayoutChildren();
        }

        /// <summary>响应 ForeColorChanged 事件。</summary>
        protected override void OnForeColorChanged(EventArgs e)
        {
            txtDisplay.ForeColor = base.ForeColor;
            base.OnForeColorChanged(e);
        }

        #endregion

        #region 下拉弹出

        /// <summary>DisplayArea_Click 方法。</summary>
        private void DisplayArea_Click(object sender, EventArgs e)
        {
            ShowDropDown();
        }

        /// <summary>显示下拉弹出窗</summary>
        public void ShowDropDown()
        {
            if (dropForm != null && !dropForm.IsDisposed)
            {
                dropForm.Close();
                dropForm = null;
                return;
            }

            dropForm = new HTreeComboDropForm(this, dropDownWidth, dropDownHeight, showSearch);
            dropForm.Tree.SelectionMode = allowMultiSelect
                ? HTreeViewEx.SelectMode.Multi
                : HTreeViewEx.SelectMode.Single;
            dropForm.MaxItems = maxDropDownItems;
            dropForm.ShowSearch = showSearch;

            // 将内部节点同步进弹出窗的树（克隆节点，避免污染原树）
            SyncNodesToDropForm();

            // 多选模式下回填已选
            if (allowMultiSelect && selectedNodes.Count > 0)
            {
                foreach (TreeNode sn in selectedNodes)
                {
                    TreeNode matched = FindNodeByText(dropForm.Tree.Nodes, sn.Text);
                    if (matched != null)
                    {
                        dropForm.Tree.SelectedNodes.Add(matched);
                    }
                }
                dropForm.Tree.Invalidate();
            }
            else if (selectedNode != null)
            {
                TreeNode matched = FindNodeByText(dropForm.Tree.Nodes, selectedNode.Text);
                if (matched != null)
                {
                    dropForm.Tree.SelectedNode = matched;
                }
            }

            dropForm.NodeSelected += DropForm_NodeSelected;
            dropForm.Show(this.FindForm());
        }

        /// <summary>SyncNodesToDropForm 方法。</summary>
        private void SyncNodesToDropForm()
        {
            dropForm.Tree.BeginUpdate();
            try
            {
                dropForm.Tree.Nodes.Clear();
                foreach (TreeNode node in innerTreeView.Nodes)
                {
                    dropForm.Tree.Nodes.Add((TreeNode)node.Clone());
                }
            }
            finally
            {
                dropForm.Tree.EndUpdate();
            }
        }

        /// <summary>DropForm_NodeSelected 方法。</summary>
        private void DropForm_NodeSelected(object sender, TreeViewEventArgs e)
        {
            if (allowMultiSelect)
            {
                // 多选：从弹出窗收集选中节点
                selectedNodes.Clear();
                foreach (TreeNode sn in dropForm.Tree.SelectedNodes)
                {
                    selectedNodes.Add(sn);
                }
                if (selectedNodes.Count == 0 && e.Node != null)
                {
                    selectedNodes.Add(e.Node);
                }
                selectedNode = selectedNodes.Count > 0 ? selectedNodes[0] : null;
            }
            else
            {
                selectedNode = e.Node;
                selectedNodes.Clear();
                if (selectedNode != null)
                {
                    selectedNodes.Add(selectedNode);
                }
            }

            UpdateDisplayText();

            if (AfterSelect != null)
            {
                AfterSelect(this, e);
            }

            if (dropForm != null && !dropForm.IsDisposed)
            {
                dropForm.Close();
                dropForm = null;
            }
        }

        #endregion

        #region 显示文本

        /// <summary>更新显示文本</summary>
        private void UpdateDisplayText()
        {
            if (allowMultiSelect)
            {
                if (selectedNodes.Count == 0)
                {
                    txtDisplay.Text = string.Empty;
                }
                else if (selectedNodes.Count == 1)
                {
                    txtDisplay.Text = selectedNodes[0].Text;
                }
                else
                {
                    txtDisplay.Text = selectedNodes.Count + HTranslation.GetContent(" 项已选");
                }
            }
            else
            {
                txtDisplay.Text = selectedNode != null ? selectedNode.Text : string.Empty;
            }
            txtDisplay.SelectionStart = 0;
            txtDisplay.SelectionLength = 0;
            this.Invalidate();
        }

        /// <summary>构建从根到节点的路径文本</summary>
        private static string BuildPath(TreeNode node)
        {
            System.Collections.Generic.Stack<string> stack = new System.Collections.Generic.Stack<string>();
            TreeNode current = node;
            while (current != null)
            {
                stack.Push(current.Text);
                current = current.Parent;
            }
            return string.Join("/", stack.ToArray());
        }

        /// <summary>在节点集合中按文本查找节点（仅第一层匹配）</summary>
        private static TreeNode FindNodeByText(TreeNodeCollection nodes, string text)
        {
            foreach (TreeNode n in nodes)
            {
                if (n.Text == text)
                {
                    return n;
                }
                TreeNode child = FindNodeByText(n.Nodes, text);
                if (child != null)
                {
                    return child;
                }
            }
            return null;
        }

        #endregion

        #region 公开方法

        /// <summary>清空选中</summary>
        public void ClearSelection()
        {
            selectedNode = null;
            selectedNodes.Clear();
            UpdateDisplayText();
        }

        #endregion
    }
}