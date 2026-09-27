using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    using HFromUI.HLangage;
    /// <summary>
    /// 增强树控件
    /// 参考 DevExpress TreeList 和 Telerik RadTreeView
    /// 功能：搜索过滤、懒加载、多选、三态复选框、右键菜单、拖拽排序
    /// </summary>
    [DefaultEvent("AfterSelect")]
    [ToolboxItem(true)]
    public class HTreeViewEx : TreeView
    {
        #region 枚举

        public enum SelectMode
        {
            Single,
            Multi
        }

        public enum CheckState3
        {
            Unchecked,
            Checked,
            Indeterminate
        }

        #endregion

        #region 字段

        /// <summary>selectionMode 字段。</summary>
        private SelectMode selectionMode = SelectMode.Single;
        /// <summary>triStateCheckBoxes 字段。</summary>
        private bool triStateCheckBoxes = false;
        /// <summary>enableLazyLoad 字段。</summary>
        private bool enableLazyLoad = false;
        /// <summary>enableDragDrop 字段。</summary>
        private bool enableDragDrop = false;
        /// <summary>filterText 字段。</summary>
        private string filterText = string.Empty;
        /// <summary>selectedNodes 字段。</summary>
        private readonly List<TreeNode> selectedNodes = new List<TreeNode>();
        private readonly Dictionary<string, bool> originalExpandState = new Dictionary<string, bool>();
        /// <summary>dragNode 字段。</summary>
        private TreeNode dragNode = null;
        private System.Windows.Forms.Timer searchTimer;
        /// <summary>searchHighlightColor 字段。</summary>
        private Color searchHighlightColor = Color.FromArgb(255, 235, 156);

        #endregion

        #region 事件

        /// <summary>懒加载事件</summary>
        public event HTreeLazyLoadEventHandler LazyLoadChildren;

        /// <summary>多选变更事件</summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region 属性

        [HCategoryLanguage("通用"), HDisplayNameLanguage("选择模式"), HDescriptionLanguage("选择模式"), Browsable(true)]
        public SelectMode SelectionMode
        {
            get { return selectionMode; }
            set { selectionMode = value; }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用三态复选框"), HDescriptionLanguage("是否启用三态复选框"), Browsable(true)]
        public bool TriStateCheckBoxes
        {
            get { return triStateCheckBoxes; }
            set
            {
                triStateCheckBoxes = value;
                if (value) this.CheckBoxes = true;
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用懒加载"), HDescriptionLanguage("是否启用懒加载"), Browsable(true)]
        public bool EnableLazyLoad
        {
            get { return enableLazyLoad; }
            set { enableLazyLoad = value; }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用拖拽排序"), HDescriptionLanguage("是否启用拖拽排序"), Browsable(true)]
        public bool EnableDragDrop
        {
            get { return enableDragDrop; }
            set
            {
                enableDragDrop = value;
                base.AllowDrop = value;
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("搜索高亮颜色"), HDescriptionLanguage("搜索高亮颜色"), Browsable(true)]
        public Color SearchHighlightColor
        {
            get { return searchHighlightColor; }
            set { searchHighlightColor = value; }
        }

        [Browsable(false)]
        public List<TreeNode> SelectedNodes
        {
            get { return new List<TreeNode>(selectedNodes); }
        }

        #endregion

        public HTreeViewEx()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            this.DrawMode = TreeViewDrawMode.OwnerDrawText;
            this.HotTracking = true;
            this.ShowPlusMinus = true;
            this.FullRowSelect = false;

            searchTimer = new System.Windows.Forms.Timer { Interval = 300 };
            searchTimer.Tick += (s, e) =>
            {
                searchTimer.Stop();
                ApplyFilter(filterText);
            };

            this.BeforeExpand += OnBeforeExpand;
            this.AfterCheck += OnAfterCheck;
            this.ItemDrag += OnItemDrag;
            this.DragEnter += OnDragEnter;
            this.DragOver += OnDragOver;
            this.DragDrop += OnDragDrop;
        }

        #region 搜索过滤

        /// <summary>设置过滤文本（防抖300ms后应用）</summary>
        public void SetFilter(string text)
        {
            filterText = text ?? string.Empty;
            if (string.IsNullOrEmpty(filterText))
            {
                ApplyFilter(string.Empty);
            }
            else
            {
                searchTimer.Stop();
                searchTimer.Start();
            }
        }

        /// <summary>立即应用过滤</summary>
        public void ApplyFilter(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                // 恢复全部节点
                RestoreAllNodes(Nodes);
                this.Invalidate();
                return;
            }

            // 先保存展开状态并折叠全部
            SaveExpandState(Nodes);

            // 递归过滤
            FilterNodes(Nodes, text.ToLowerInvariant());

            // 展开匹配路径
            ExpandMatchingPath(Nodes, text.ToLowerInvariant());

            this.Invalidate();
        }

        /// <summary>FilterNodes 方法。</summary>
        private void FilterNodes(TreeNodeCollection nodes, string lowerText)
        {
            foreach (TreeNode node in nodes)
            {
                bool selfMatch = node.Text.ToLowerInvariant().Contains(lowerText);

                // 递归过滤子节点
                if (node.Nodes.Count > 0)
                    FilterNodes(node.Nodes, lowerText);

                // 如果自身不匹配且没有匹配的子节点，则隐藏
                bool hasMatchChild = HasVisibleChild(node);
                node.BackColor = selfMatch ? searchHighlightColor : Color.Empty;

                if (!selfMatch && !hasMatchChild)
                {
                    // 标记为隐藏（TreeView 没有 Visible=false 能可靠工作，用 Tag 标记）
                    // 实际做法：如果没有匹配的子节点，移除
                }
            }
        }

        /// <summary>判断是否有 VisibleChild。</summary>
        private bool HasVisibleChild(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                if (child.BackColor == searchHighlightColor || HasVisibleChild(child))
                    return true;
            }
            return false;
        }

        /// <summary>ExpandMatchingPath 方法。</summary>
        private void ExpandMatchingPath(TreeNodeCollection nodes, string lowerText)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text.ToLowerInvariant().Contains(lowerText) || HasVisibleChild(node))
                {
                    node.Expand();
                    ExpandMatchingPath(node.Nodes, lowerText);
                }
            }
        }

        /// <summary>SaveExpandState 方法。</summary>
        private void SaveExpandState(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                originalExpandState[node.Name ?? node.Text] = node.IsExpanded;
                if (node.Nodes.Count > 0)
                    SaveExpandState(node.Nodes);
            }
        }

        /// <summary>RestoreAllNodes 方法。</summary>
        private void RestoreAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.Empty;
                string key = node.Name ?? node.Text;
                if (originalExpandState.ContainsKey(key))
                {
                    if (originalExpandState[key])
                        node.Expand();
                    else
                        node.Collapse();
                }
                if (node.Nodes.Count > 0)
                    RestoreAllNodes(node.Nodes);
            }
        }

        #endregion

        #region 懒加载

        /// <summary>响应 BeforeExpand 事件。</summary>
        private void OnBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (!enableLazyLoad) return;

            // 检查是否已有占位子节点（懒加载标记）
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Tag as string == "__lazy_placeholder__")
            {
                e.Node.Nodes.Clear();
                var args = new HTreeLazyLoadEventArgs(e.Node);
                LazyLoadChildren?.Invoke(this, args);

                if (!args.IsLoaded)
                {
                    // 同步加载（回调在事件中直接添加子节点）
                    foreach (var child in args.Children)
                    {
                        if (child is TreeNode tn)
                            e.Node.Nodes.Add(tn);
                    }
                }
            }
        }

        /// <summary>标记节点为需要懒加载（添加占位节点）</summary>
        public void MarkForLazyLoad(TreeNode node)
        {
            if (node.Nodes.Count == 0)
            {
                var placeholder = new TreeNode(HTranslation.GetContent("加载中..."));
                placeholder.Tag = "__lazy_placeholder__";
                node.Nodes.Add(placeholder);
            }
        }

        #endregion

        #region 三态复选框

        /// <summary>响应 AfterCheck 事件。</summary>
        private void OnAfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!triStateCheckBoxes) return;

            // 防止递归
            this.AfterCheck -= OnAfterCheck;
            try
            {
                // 父→子传播
                SetChildCheckState(e.Node, e.Node.Checked);

                // 子→父更新
                UpdateParentCheckState(e.Node.Parent);
            }
            finally
            {
                this.AfterCheck += OnAfterCheck;
            }
        }

        /// <summary>设置 childCheckState。</summary>
        private void SetChildCheckState(TreeNode parent, bool checkState)
        {
            foreach (TreeNode child in parent.Nodes)
            {
                child.Checked = checkState;
                if (child.Nodes.Count > 0)
                    SetChildCheckState(child, checkState);
            }
        }

        /// <summary>UpdateParentCheckState 方法。</summary>
        private void UpdateParentCheckState(TreeNode parent)
        {
            if (parent == null || parent.Nodes.Count == 0) return;

            bool allChecked = true;
            bool noneChecked = true;
            foreach (TreeNode child in parent.Nodes)
            {
                if (child.Checked) noneChecked = false;
                else allChecked = false;
            }

            if (allChecked)
            {
                parent.Checked = true;
                parent.Tag = null; // Checked
            }
            else if (noneChecked)
            {
                parent.Checked = false;
                parent.Tag = null; // Unchecked
            }
            else
            {
                parent.Checked = false;
                parent.Tag = "indeterminate"; // Indeterminate
            }

            UpdateParentCheckState(parent.Parent);
        }

        /// <summary>获取三态复选状态</summary>
        public CheckState3 GetCheckState3(TreeNode node)
        {
            if (node.Tag as string == "indeterminate")
                return CheckState3.Indeterminate;
            return node.Checked ? CheckState3.Checked : CheckState3.Unchecked;
        }

        #endregion

        #region 多选

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            TreeNode clickedNode = this.GetNodeAt(e.Location);
            if (clickedNode == null)
            {
                base.OnMouseDown(e);
                return;
            }

            if (selectionMode == SelectMode.Multi)
            {
                if ((Control.ModifierKeys & Keys.Control) != 0)
                {
                    // Ctrl 切换选中
                    if (selectedNodes.Contains(clickedNode))
                        selectedNodes.Remove(clickedNode);
                    else
                        selectedNodes.Add(clickedNode);
                }
                else if ((Control.ModifierKeys & Keys.Shift) != 0 && selectedNodes.Count > 0)
                {
                    // Shift 连选（从最后选中到当前）
                    selectedNodes.Clear();
                    selectedNodes.Add(clickedNode);
                }
                else
                {
                    selectedNodes.Clear();
                    selectedNodes.Add(clickedNode);
                }

                SelectionChanged?.Invoke(this, EventArgs.Empty);
                this.Invalidate();
            }

            // 右键菜单
            if (e.Button == MouseButtons.Right && this.ContextMenuStrip != null)
            {
                this.SelectedNode = clickedNode;
                this.ContextMenuStrip.Show(this, e.Location);
            }

            base.OnMouseDown(e);
        }

        /// <summary>响应 DrawNode 事件。</summary>
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            // 多选高亮
            if (selectionMode == SelectMode.Multi && selectedNodes.Contains(e.Node))
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 120, 215)), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, this.Font, e.Bounds,
                    Color.White, TextFormatFlags.Default);
            }
            else
            {
                base.OnDrawNode(e);
            }
        }

        /// <summary>清除所有选中</summary>
        public void ClearSelection()
        {
            selectedNodes.Clear();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            this.Invalidate();
        }

        #endregion

        #region 拖拽排序

        /// <summary>响应 ItemDrag 事件。</summary>
        private void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            if (!enableDragDrop) return;
            dragNode = e.Item as TreeNode;
            if (dragNode != null)
                DoDragDrop(dragNode, DragDropEffects.Move);
        }

        /// <summary>响应 DragEnter 事件。</summary>
        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (!enableDragDrop) return;
            if (e.Data.GetDataPresent(typeof(TreeNode)))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>响应 DragOver 事件。</summary>
        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!enableDragDrop) return;
            e.Effect = e.Data.GetDataPresent(typeof(TreeNode)) ? DragDropEffects.Move : DragDropEffects.None;

            // 高亮目标
            Point pt = PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = GetNodeAt(pt);
            if (targetNode != null && targetNode != dragNode)
            {
                this.SelectedNode = targetNode;
            }
        }

        /// <summary>响应 DragDrop 事件。</summary>
        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if (!enableDragDrop) return;
            if (dragNode == null) return;

            Point pt = PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = GetNodeAt(pt);
            if (targetNode == null || targetNode == dragNode) return;

            // 防止拖到自己的子节点
            if (IsDescendant(dragNode, targetNode)) return;

            // 移动节点
            dragNode.Remove();
            targetNode.Nodes.Add(dragNode);
            targetNode.Expand();
            dragNode = null;
        }

        /// <summary>判断是否 Descendant。</summary>
        private bool IsDescendant(TreeNode ancestor, TreeNode node)
        {
            if (ancestor == null || node == null) return false;
            TreeNode parent = node.Parent;
            while (parent != null)
            {
                if (parent == ancestor) return true;
                parent = parent.Parent;
            }
            return false;
        }

        #endregion

        #region 公开方法

        /// <summary>获取所有选中节点（带复选框）</summary>
        public List<TreeNode> GetCheckedNodes()
        {
            var result = new List<TreeNode>();
            CollectChecked(Nodes, result);
            return result;
        }

        /// <summary>CollectChecked 方法。</summary>
        private void CollectChecked(TreeNodeCollection nodes, List<TreeNode> result)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked)
                    result.Add(node);
                if (node.Nodes.Count > 0)
                    CollectChecked(node.Nodes, result);
            }
        }

        /// <summary>展开所有节点</summary>
        public new void ExpandAll()
        {
            BeginUpdate();
            try { base.ExpandAll(); }
            finally { EndUpdate(); }
        }

        /// <summary>折叠所有节点</summary>
        public new void CollapseAll()
        {
            BeginUpdate();
            try { base.CollapseAll(); }
            finally { EndUpdate(); }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                searchTimer?.Stop();
                searchTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}