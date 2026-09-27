using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    using HFromUI.HLangage;
    #region 枚举

    /// <summary>排序方向</summary>
    public enum HTreeListSortOrder
    {
        /// <summary>不排序</summary>
        None,
        /// <summary>升序</summary>
        Ascending,
        /// <summary>降序</summary>
        Descending
    }

    /// <summary>三态复选状态</summary>
    public enum HTreeListCheckState
    {
        /// <summary>未选中</summary>
        Unchecked,
        /// <summary>已选中</summary>
        Checked,
        /// <summary>不确定（子节点状态不一致）</summary>
        Indeterminate
    }

    /// <summary>汇总类型</summary>
    public enum HTreeListSummaryType
    {
        /// <summary>不汇总</summary>
        None,
        /// <summary>求和</summary>
        Sum,
        /// <summary>平均值</summary>
        Average,
        /// <summary>计数</summary>
        Count
    }

    #endregion

    /// <summary>
    /// HTreeList 多列树控件
    /// 参考 DevExpress TreeList 风格，采用自定义 GDI+ 渲染（非 TreeView）。
    /// 支持虚拟渲染（仅绘制可见行，可承载 10 万+ 节点）、列头排序、列宽拖拽、
    /// 三态复选框、懒加载、汇总行、自定义滚动条、行选择与展开/折叠。
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("AfterSelect")]
    public class HTreeList : Control
    {
        #region 常量

        /// <summary>懒加载占位节点的标记值</summary>
        private const string LazyPlaceholderTag = "__lazy_placeholder__";
        /// <summary>展开/折叠图标边长</summary>
        private const int ExpandIconSize = 16;
        /// <summary>复选框边长</summary>
        private const int CheckBoxSize = 16;
        /// <summary>图标与文本之间的间距</summary>
        private const int IconTextGap = 4;
        /// <summary>列分隔条拖拽命中容差</summary>
        private const int ResizeHitTolerance = 4;
        /// <summary>最小列宽</summary>
        private const int MinColumnWidth = 10;

        #endregion

        #region 嵌套类：HTreeListNode

        /// <summary>
        /// 轻量级树节点
        /// 文本以字典形式按列名（FieldName/Name）存储
        /// </summary>
        public class HTreeListNode
        {
            private readonly Dictionary<string, string> _textValues;
            private readonly List<HTreeListNode> _childNodes;
            private HTreeListNode _parent;
            private HTreeList _owner;
            private bool _expanded;
            /// <summary>_checkedState 字段。</summary>
            private HTreeListCheckState _checkedState = HTreeListCheckState.Unchecked;
            private object _tag;

            /// <summary>初始化 HTreeListNode 的新实例</summary>
            public HTreeListNode()
            {
                _textValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _childNodes = new List<HTreeListNode>();
            }

            /// <summary>按列名（FieldName/Name）存储的文本字典</summary>
            [Browsable(false)]
            public Dictionary<string, string> Text
            {
                get { return _textValues; }
            }

            /// <summary>关联的自定义数据对象</summary>
            public object Tag
            {
                get { return _tag; }
                set { _tag = value; }
            }

            /// <summary>子节点集合</summary>
            [Browsable(false)]
            public List<HTreeListNode> ChildNodes
            {
                get { return _childNodes; }
            }

            /// <summary>父节点（根节点为 null）</summary>
            [Browsable(false)]
            public HTreeListNode Parent
            {
                get { return _parent; }
                internal set { _parent = value; }
            }

            /// <summary>是否展开</summary>
            public bool Expanded
            {
                get { return _expanded; }
                set
                {
                    if (_expanded != value)
                    {
                        _expanded = value;
                        if (_owner != null)
                        {
                            _owner.OnNodeExpandedChanged(this);
                        }
                    }
                }
            }

            /// <summary>三态复选状态</summary>
            public HTreeListCheckState CheckedState
            {
                get { return _checkedState; }
                set
                {
                    if (_checkedState != value)
                    {
                        _checkedState = value;
                        if (_owner != null)
                        {
                            _owner.OnNodeCheckedChanged(this);
                        }
                    }
                }
            }

            /// <summary>层级（根节点为 0）</summary>
            [Browsable(false)]
            public int Level
            {
                get
                {
                    int level = 0;
                    HTreeListNode p = _parent;
                    while (p != null)
                    {
                        level++;
                        p = p._parent;
                    }
                    return level;
                }
            }

            /// <summary>是否可见（所有祖先节点均已展开）</summary>
            [Browsable(false)]
            public bool IsVisible
            {
                get
                {
                    HTreeListNode p = _parent;
                    while (p != null)
                    {
                        if (!p._expanded)
                        {
                            return false;
                        }
                        p = p._parent;
                    }
                    return true;
                }
            }

            /// <summary>是否存在子节点</summary>
            [Browsable(false)]
            public bool HasChildNodes
            {
                get { return _childNodes.Count > 0; }
            }

            /// <summary>是否为懒加载占位节点</summary>
            [Browsable(false)]
            public bool IsLazyPlaceholder
            {
                get { return string.Equals(_tag as string, LazyPlaceholderTag, StringComparison.Ordinal); }
            }

            /// <summary>获取指定列的文本</summary>
            public string GetText(string columnName)
            {
                if (string.IsNullOrEmpty(columnName))
                {
                    return string.Empty;
                }
                string v;
                if (_textValues.TryGetValue(columnName, out v))
                {
                    return v;
                }
                return string.Empty;
            }

            /// <summary>设置指定列的文本</summary>
            public void SetText(string columnName, string value)
            {
                if (string.IsNullOrEmpty(columnName))
                {
                    return;
                }
                _textValues[columnName] = value;
                if (_owner != null)
                {
                    _owner.OnNodeTextChanged(this);
                }
            }

            /// <summary>设置所属控件与父节点（递归应用到子树）</summary>
            internal void OnAddedTo(HTreeListNode parent, HTreeList owner)
            {
                _parent = parent;
                _owner = owner;
                for (int i = 0; i < _childNodes.Count; i++)
                {
                    _childNodes[i].OnAddedTo(this, owner);
                }
            }

            /// <summary>返回节点的显示文本</summary>
            public override string ToString()
            {
                foreach (KeyValuePair<string, string> kv in _textValues)
                {
                    return kv.Value;
                }
                return "HTreeListNode";
            }
        }

        #endregion

        #region 嵌套类：HTreeListNodeCollection

        /// <summary>
        /// 根节点集合（用于 HTreeList.Nodes 属性）
        /// 添加/移除时自动维护父节点与所属控件引用，并通知控件重建可见列表
        /// </summary>
        public class HTreeListNodeCollection
        {
            private readonly List<HTreeListNode> _list;
            private readonly HTreeList _owner;
            private readonly HTreeListNode _parent;

            internal HTreeListNodeCollection(HTreeList owner, HTreeListNode parent)
            {
                _owner = owner;
                _parent = parent;
                _list = new List<HTreeListNode>();
            }

            /// <summary>内部列表（供排序使用）</summary>
            internal List<HTreeListNode> InnerList
            {
                get { return _list; }
            }

            /// <summary>节点数量</summary>
            public int Count
            {
                get { return _list.Count; }
            }

            /// <summary>获取指定索引处的节点</summary>
            public HTreeListNode this[int index]
            {
                get { return _list[index]; }
            }

            /// <summary>添加一个节点</summary>
            public HTreeListNode Add(HTreeListNode node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(HTranslation.GetContent("节点不能为空"));
                }
                node.OnAddedTo(_parent, _owner);
                _list.Add(node);
                if (_owner != null)
                {
                    _owner.OnStructureChanged();
                }
                return node;
            }

            /// <summary>添加一个仅含文本的节点（文本写入第一列）</summary>
            public HTreeListNode Add(string text)
            {
                HTreeListNode node = new HTreeListNode();
                node.SetText(_owner != null ? _owner.GetFirstColumnName() : "Name", text);
                return Add(node);
            }

            /// <summary>批量添加节点</summary>
            public void AddRange(IEnumerable<HTreeListNode> nodes)
            {
                if (nodes == null)
                {
                    return;
                }
                foreach (HTreeListNode n in nodes)
                {
                    if (n == null)
                    {
                        continue;
                    }
                    n.OnAddedTo(_parent, _owner);
                    _list.Add(n);
                }
                if (_owner != null)
                {
                    _owner.OnStructureChanged();
                }
            }

            /// <summary>在指定索引处插入节点</summary>
            public void Insert(int index, HTreeListNode node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(HTranslation.GetContent("节点不能为空"));
                }
                node.OnAddedTo(_parent, _owner);
                _list.Insert(index, node);
                if (_owner != null)
                {
                    _owner.OnStructureChanged();
                }
            }

            /// <summary>移除节点</summary>
            public bool Remove(HTreeListNode node)
            {
                bool r = _list.Remove(node);
                if (r && _owner != null)
                {
                    _owner.OnStructureChanged();
                }
                return r;
            }

            /// <summary>移除指定索引处的节点</summary>
            public void RemoveAt(int index)
            {
                _list.RemoveAt(index);
                if (_owner != null)
                {
                    _owner.OnStructureChanged();
                }
            }

            /// <summary>清空集合</summary>
            public void Clear()
            {
                _list.Clear();
                if (_owner != null)
                {
                    _owner.OnStructureChanged();
                }
            }

            /// <summary>是否包含指定节点</summary>
            public bool Contains(HTreeListNode node)
            {
                return _list.Contains(node);
            }

            /// <summary>获取指定节点的索引</summary>
            public int IndexOf(HTreeListNode node)
            {
                return _list.IndexOf(node);
            }

            /// <summary>返回枚举器</summary>
            public IEnumerator<HTreeListNode> GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }

        #endregion

        #region 字段

        // 数据
        private readonly List<HTreeListColumn> _columns;
        private readonly HTreeListNodeCollection _rootNodes;
        private readonly List<HTreeListNode> _visibleList;
        private readonly Dictionary<string, HTreeListSummaryType> _summaryTypes;

        // 滚动条
        private readonly VScrollBar _vScrollBar;
        // 注意：显式限定为标准 WinForms 水平滚动条，避免解析到命名空间内已过时的自定义 HScrollBar
        private readonly System.Windows.Forms.HScrollBar _hScrollBar;

        // 主题色
        private Color _backColor = Color.FromArgb(45, 45, 48);
        /// <summary>_headerColor 字段。</summary>
        private Color _headerColor = Color.FromArgb(62, 62, 66);
        /// <summary>_rowColor 字段。</summary>
        private Color _rowColor = Color.FromArgb(45, 45, 48);
        /// <summary>_altRowColor 字段。</summary>
        private Color _altRowColor = Color.FromArgb(37, 37, 38);
        /// <summary>_selectColor 字段。</summary>
        private Color _selectColor = Color.FromArgb(0, 122, 204);
        /// <summary>_textColor 字段。</summary>
        private Color _textColor = Color.White;
        /// <summary>_lineColor 字段。</summary>
        private Color _lineColor = Color.FromArgb(90, 90, 90);

        // 布局
        private int _rowHeight = 24;
        /// <summary>_headerHeight 字段。</summary>
        private int _headerHeight = 25;
        /// <summary>_summaryHeight 字段。</summary>
        private int _summaryHeight = 22;
        /// <summary>_indentSize 字段。</summary>
        private int _indentSize = 20;
        /// <summary>_showTreeLines 字段。</summary>
        private bool _showTreeLines = false;
        /// <summary>_showCheckBoxes 字段。</summary>
        private bool _showCheckBoxes = false;
        /// <summary>_showSummaryRow 字段。</summary>
        private bool _showSummaryRow = false;
        /// <summary>_enableLazyLoad 字段。</summary>
        private bool _enableLazyLoad = false;

        // 状态
        private HTreeListNode _selectedNode;
        /// <summary>_sortColumnIndex 字段。</summary>
        private int _sortColumnIndex = -1;
        /// <summary>_sortOrder 字段。</summary>
        private HTreeListSortOrder _sortOrder = HTreeListSortOrder.None;
        /// <summary>_firstVisibleIndex 字段。</summary>
        private int _firstVisibleIndex = 0;
        /// <summary>_horizontalOffset 字段。</summary>
        private int _horizontalOffset = 0;

        // 列宽拖拽
        private int _resizingColumnIndex = -1;
        private int _resizeStartX;
        private int _resizeStartWidth;

        // 更新控制
        private int _updateCount = 0;

        #endregion

        #region 事件

        /// <summary>节点选择后触发</summary>
        public event EventHandler AfterSelect;

        /// <summary>节点展开后触发</summary>
        public event EventHandler AfterExpand;

        /// <summary>节点折叠后触发</summary>
        public event EventHandler AfterCollapse;

        /// <summary>选择变更时触发</summary>
        public event EventHandler SelectionChanged;

        /// <summary>懒加载子节点事件（启用 EnableLazyLoad 时于展开前触发）</summary>
        public event HTreeLazyLoadEventHandler LazyLoadChildren;

        #endregion

        #region 构造函数

        /// <summary>初始化 HTreeList 的新实例</summary>
        public HTreeList()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);
            DoubleBuffered = true;

            base.BackColor = _backColor;
            base.Font = new Font("微软雅黑", 9f);

            _columns = new List<HTreeListColumn>();
            _rootNodes = new HTreeListNodeCollection(this, null);
            _visibleList = new List<HTreeListNode>();
            _summaryTypes = new Dictionary<string, HTreeListSummaryType>(StringComparer.OrdinalIgnoreCase);

            _vScrollBar = new VScrollBar();
            _vScrollBar.Visible = false;
            _vScrollBar.Scroll += OnVScroll;
            Controls.Add(_vScrollBar);

            _hScrollBar = new System.Windows.Forms.HScrollBar();
            _hScrollBar.Visible = false;
            _hScrollBar.Scroll += OnHScroll;
            Controls.Add(_hScrollBar);
        }

        #endregion

        #region 属性

        /// <summary>列集合</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("列集合"), HDescriptionLanguage("列集合"), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [MergableProperty(false)]
        public List<HTreeListColumn> Columns
        {
            get { return _columns; }
        }

        /// <summary>根节点集合</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public HTreeListNodeCollection Nodes
        {
            get { return _rootNodes; }
        }

        /// <summary>当前选中节点</summary>
        [Browsable(false)]
        public HTreeListNode SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (!ReferenceEquals(_selectedNode, value))
                {
                    _selectedNode = value;
                    if (value != null)
                    {
                        EnsureVisible(value);
                    }
                    Invalidate();
                    EventHandler sel = AfterSelect;
                    if (sel != null)
                    {
                        sel(this, EventArgs.Empty);
                    }
                    EventHandler sc = SelectionChanged;
                    if (sc != null)
                    {
                        sc(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>是否显示三态复选框</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示复选框"), HDescriptionLanguage("是否显示复选框"), Browsable(true)]
        [DefaultValue(false)]
        public bool ShowCheckBoxes
        {
            get { return _showCheckBoxes; }
            set { _showCheckBoxes = value; Invalidate(); }
        }

        /// <summary>是否显示树形连接线</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示树形线"), HDescriptionLanguage("是否显示树形线"), Browsable(true)]
        [DefaultValue(false)]
        public bool ShowTreeLines
        {
            get { return _showTreeLines; }
            set { _showTreeLines = value; Invalidate(); }
        }

        /// <summary>是否显示汇总行</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否显示汇总行"), HDescriptionLanguage("是否显示汇总行"), Browsable(true)]
        [DefaultValue(false)]
        public bool ShowSummaryRow
        {
            get { return _showSummaryRow; }
            set { _showSummaryRow = value; LayoutScrollBars(); Invalidate(); }
        }

        /// <summary>是否启用懒加载</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("是否启用懒加载"), HDescriptionLanguage("是否启用懒加载"), Browsable(true)]
        [DefaultValue(false)]
        public bool EnableLazyLoad
        {
            get { return _enableLazyLoad; }
            set { _enableLazyLoad = value; }
        }

        /// <summary>每级缩进像素</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("缩进大小"), HDescriptionLanguage("缩进大小"), Browsable(true)]
        [DefaultValue(20)]
        public int IndentSize
        {
            get { return _indentSize; }
            set { _indentSize = value < 0 ? 0 : value; Invalidate(); }
        }

        /// <summary>行高</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("行高"), HDescriptionLanguage("行高"), Browsable(true)]
        [DefaultValue(24)]
        public int RowHeight
        {
            get { return _rowHeight; }
            set
            {
                _rowHeight = value < 12 ? 12 : value;
                LayoutScrollBars();
                Invalidate();
            }
        }

        /// <summary>列头高度</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("列头高度"), HDescriptionLanguage("列头高度"), Browsable(true)]
        [DefaultValue(25)]
        public int HeaderHeight
        {
            get { return _headerHeight; }
            set { _headerHeight = value < 16 ? 16 : value; Invalidate(); }
        }

        /// <summary>背景色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("背景色"), HDescriptionLanguage("背景色"), Browsable(true)]
        public override Color BackColor
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                base.BackColor = value;
                Invalidate();
            }
        }
        /// <summary>ShouldSerializeBackColor 方法。</summary>
        private bool ShouldSerializeBackColor() { return _backColor != Color.FromArgb(45, 45, 48); }

        /// <summary>列头背景色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("列头颜色"), HDescriptionLanguage("列头颜色"), Browsable(true)]
        public Color HeaderColor
        {
            get { return _headerColor; }
            set { _headerColor = value; Invalidate(); }
        }
        /// <summary>ShouldSerializeHeaderColor 方法。</summary>
        private bool ShouldSerializeHeaderColor() { return _headerColor != Color.FromArgb(62, 62, 66); }

        /// <summary>行背景色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("行颜色"), HDescriptionLanguage("行颜色"), Browsable(true)]
        public Color RowColor
        {
            get { return _rowColor; }
            set { _rowColor = value; Invalidate(); }
        }
        /// <summary>ShouldSerializeRowColor 方法。</summary>
        private bool ShouldSerializeRowColor() { return _rowColor != Color.FromArgb(45, 45, 48); }

        /// <summary>交替行背景色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("交替行颜色"), HDescriptionLanguage("交替行颜色"), Browsable(true)]
        public Color AltRowColor
        {
            get { return _altRowColor; }
            set { _altRowColor = value; Invalidate(); }
        }
        /// <summary>ShouldSerializeAltRowColor 方法。</summary>
        private bool ShouldSerializeAltRowColor() { return _altRowColor != Color.FromArgb(37, 37, 38); }

        /// <summary>选中行背景色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中颜色"), HDescriptionLanguage("选中颜色"), Browsable(true)]
        public Color SelectColor
        {
            get { return _selectColor; }
            set { _selectColor = value; Invalidate(); }
        }
        /// <summary>ShouldSerializeSelectColor 方法。</summary>
        private bool ShouldSerializeSelectColor() { return _selectColor != Color.FromArgb(0, 122, 204); }

        /// <summary>文本颜色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("文本颜色"), HDescriptionLanguage("文本颜色"), Browsable(true)]
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; Invalidate(); }
        }
        /// <summary>ShouldSerializeTextColor 方法。</summary>
        private bool ShouldSerializeTextColor() { return _textColor != Color.White; }

        /// <summary>线条颜色</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("线条颜色"), HDescriptionLanguage("线条颜色"), Browsable(true)]
        public Color LineColor
        {
            get { return _lineColor; }
            set { _lineColor = value; Invalidate(); }
        }
        /// <summary>ShouldSerializeLineColor 方法。</summary>
        private bool ShouldSerializeLineColor() { return _lineColor != Color.FromArgb(90, 90, 90); }

        /// <summary>当前排序列索引</summary>
        [Browsable(false)]
        public int SortColumnIndex
        {
            get { return _sortColumnIndex; }
        }

        /// <summary>当前排序方向</summary>
        [Browsable(false)]
        public HTreeListSortOrder SortOrder
        {
            get { return _sortOrder; }
        }

        #endregion

        #region 汇总设置

        /// <summary>设置列的汇总类型</summary>
        public void SetSummary(string columnName, HTreeListSummaryType type)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return;
            }
            _summaryTypes[columnName] = type;
            Invalidate();
        }

        /// <summary>获取列的汇总类型</summary>
        public HTreeListSummaryType GetSummary(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return HTreeListSummaryType.None;
            }
            HTreeListSummaryType t;
            if (_summaryTypes.TryGetValue(columnName, out t))
            {
                return t;
            }
            return HTreeListSummaryType.None;
        }

        #endregion

        #region 节点管理

        /// <summary>添加根节点</summary>
        public HTreeListNode AddRootNode(HTreeListNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("节点不能为空"));
            }
            node.OnAddedTo(null, this);
            _rootNodes.InnerList.Add(node);
            OnStructureChanged();
            return node;
        }

        /// <summary>添加子节点</summary>
        public HTreeListNode AddChildNode(HTreeListNode parent, HTreeListNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(HTranslation.GetContent("节点不能为空"));
            }
            if (parent == null)
            {
                return AddRootNode(node);
            }
            node.OnAddedTo(parent, this);
            parent.ChildNodes.Add(node);
            OnStructureChanged();
            return node;
        }

        /// <summary>清空所有节点</summary>
        public void Clear()
        {
            _rootNodes.Clear();
            _selectedNode = null;
            _firstVisibleIndex = 0;
            _horizontalOffset = 0;
        }

        /// <summary>获取第一列名（优先 FieldName，其次 Name）</summary>
        internal string GetFirstColumnName()
        {
            if (_columns.Count > 0)
            {
                string f = _columns[0].FieldName;
                if (!string.IsNullOrEmpty(f))
                {
                    return f;
                }
                return _columns[0].Name;
            }
            return "Name";
        }

        #endregion

        #region 展开/折叠

        /// <summary>切换节点展开/折叠状态</summary>
        public void Toggle(HTreeListNode node)
        {
            if (node == null)
            {
                return;
            }
            if (node.Expanded)
            {
                Collapse(node);
            }
            else
            {
                Expand(node);
            }
        }

        /// <summary>展开节点（懒加载于展开前完成）</summary>
        public void Expand(HTreeListNode node)
        {
            if (node == null || node.Expanded)
            {
                return;
            }
            if (_enableLazyLoad)
            {
                LoadLazyChildren(node);
            }
            node.Expanded = true;
        }

        /// <summary>折叠节点</summary>
        public void Collapse(HTreeListNode node)
        {
            if (node == null || !node.Expanded)
            {
                return;
            }
            node.Expanded = false;
        }

        /// <summary>展开所有节点</summary>
        public void ExpandAll()
        {
            BeginUpdate();
            try
            {
                ExpandAllRecursive(_rootNodes.InnerList);
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>折叠所有节点</summary>
        public void CollapseAll()
        {
            BeginUpdate();
            try
            {
                CollapseAllRecursive(_rootNodes.InnerList);
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>ExpandAllRecursive 方法。</summary>
        private void ExpandAllRecursive(List<HTreeListNode> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                HTreeListNode n = list[i];
                if (_enableLazyLoad)
                {
                    LoadLazyChildren(n);
                }
                n.Expanded = true;
                ExpandAllRecursive(n.ChildNodes);
            }
        }

        /// <summary>CollapseAllRecursive 方法。</summary>
        private void CollapseAllRecursive(List<HTreeListNode> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                HTreeListNode n = list[i];
                n.Expanded = false;
                CollapseAllRecursive(n.ChildNodes);
            }
        }

        /// <summary>滚动并展开祖先，使指定节点可见</summary>
        public void EnsureVisible(HTreeListNode node)
        {
            if (node == null)
            {
                return;
            }
            BeginUpdate();
            try
            {
                HTreeListNode p = node.Parent;
                while (p != null)
                {
                    if (_enableLazyLoad)
                    {
                        LoadLazyChildren(p);
                    }
                    p.Expanded = true;
                    p = p.Parent;
                }
                RebuildVisibleList();
                int idx = _visibleList.IndexOf(node);
                if (idx >= 0)
                {
                    int visibleRows = Math.Max(1, (GetContentBottom() - _headerHeight) / _rowHeight);
                    if (idx < _firstVisibleIndex)
                    {
                        _firstVisibleIndex = idx;
                    }
                    else if (idx >= _firstVisibleIndex + visibleRows)
                    {
                        _firstVisibleIndex = idx - visibleRows + 1;
                    }
                    if (_firstVisibleIndex < 0)
                    {
                        _firstVisibleIndex = 0;
                    }
                    if (_vScrollBar.Visible)
                    {
                        _vScrollBar.Value = Math.Min(_firstVisibleIndex, _vScrollBar.Maximum);
                    }
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        #endregion

        #region 懒加载

        /// <summary>标记节点为需要懒加载（添加占位子节点以显示展开图标）</summary>
        public void MarkForLazyLoad(HTreeListNode node)
        {
            if (node == null)
            {
                return;
            }
            if (GetPlaceholder(node) != null)
            {
                return;
            }
            if (node.ChildNodes.Count > 0)
            {
                return;
            }
            HTreeListNode ph = new HTreeListNode();
            ph.SetText(GetFirstColumnName(), HTranslation.GetContent("加载中..."));
            ph.Tag = LazyPlaceholderTag;
            AddChildNode(node, ph);
        }

        /// <summary>LoadLazyChildren 方法。</summary>
        private void LoadLazyChildren(HTreeListNode node)
        {
            HTreeListNode placeholder = GetPlaceholder(node);
            if (placeholder == null)
            {
                return;
            }
            // 批量更新：避免每次 AddChildNode 都触发一次重建（懒加载可能返回大量子节点）
            BeginUpdate();
            try
            {
                // 先移除占位节点，避免重复加载
                node.ChildNodes.Remove(placeholder);
                HTreeLazyLoadEventArgs args = new HTreeLazyLoadEventArgs(node);
                HTreeLazyLoadEventHandler handler = LazyLoadChildren;
                if (handler != null)
                {
                    handler(this, args);
                }
                if (args.Children != null)
                {
                    foreach (object o in args.Children)
                    {
                        HTreeListNode child = o as HTreeListNode;
                        if (child != null)
                        {
                            AddChildNode(node, child);
                        }
                    }
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>获取 placeholder。</summary>
        private HTreeListNode GetPlaceholder(HTreeListNode node)
        {
            if (node == null)
            {
                return null;
            }
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                HTreeListNode c = node.ChildNodes[i];
                if (string.Equals(c.Tag as string, LazyPlaceholderTag, StringComparison.Ordinal))
                {
                    return c;
                }
            }
            return null;
        }

        #endregion

        #region 三态复选框

        /// <summary>切换节点复选状态（并向子节点传播、向祖先回算）</summary>
        public void ToggleCheck(HTreeListNode node)
        {
            if (node == null)
            {
                return;
            }
            HTreeListCheckState ns = (node.CheckedState == HTreeListCheckState.Checked)
                ? HTreeListCheckState.Unchecked
                : HTreeListCheckState.Checked;
            BeginUpdate();
            try
            {
                SetCheckStateRecursive(node, ns);
                UpdateAncestorCheckState(node.Parent);
            }
            finally
            {
                EndUpdate();
            }
        }

        /// <summary>设置 checkStateRecursive。</summary>
        private void SetCheckStateRecursive(HTreeListNode node, HTreeListCheckState state)
        {
            node.CheckedState = state;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                SetCheckStateRecursive(node.ChildNodes[i], state);
            }
        }

        /// <summary>UpdateAncestorCheckState 方法。</summary>
        private void UpdateAncestorCheckState(HTreeListNode node)
        {
            while (node != null)
            {
                HTreeListCheckState computed = ComputeAncestorState(node);
                if (node.CheckedState != computed)
                {
                    node.CheckedState = computed;
                }
                node = node.Parent;
            }
        }

        /// <summary>ComputeAncestorState 方法。</summary>
        private HTreeListCheckState ComputeAncestorState(HTreeListNode node)
        {
            if (node.ChildNodes.Count == 0)
            {
                return node.CheckedState;
            }
            bool allChecked = true;
            bool noneChecked = true;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                HTreeListCheckState cs = node.ChildNodes[i].CheckedState;
                if (cs != HTreeListCheckState.Checked)
                {
                    allChecked = false;
                }
                if (cs != HTreeListCheckState.Unchecked)
                {
                    noneChecked = false;
                }
            }
            if (allChecked)
            {
                return HTreeListCheckState.Checked;
            }
            if (noneChecked)
            {
                return HTreeListCheckState.Unchecked;
            }
            return HTreeListCheckState.Indeterminate;
        }

        #endregion

        #region 排序

        /// <summary>按指定列排序（递归排序各级子节点）</summary>
        public void SortColumn(int columnIndex, HTreeListSortOrder order)
        {
            if (columnIndex < 0 || columnIndex >= _columns.Count)
            {
                return;
            }
            _sortColumnIndex = columnIndex;
            _sortOrder = order;
            if (order == HTreeListSortOrder.None)
            {
                RebuildVisibleList();
                Invalidate();
                return;
            }
            HTreeListColumn col = _columns[columnIndex];
            string fieldName = string.IsNullOrEmpty(col.FieldName) ? col.Name : col.FieldName;
            SortListRecursive(_rootNodes.InnerList, fieldName, order);
            RebuildVisibleList();
            Invalidate();
        }

        /// <summary>SortListRecursive 方法。</summary>
        private void SortListRecursive(List<HTreeListNode> list, string fieldName, HTreeListSortOrder order)
        {
            if (list.Count < 2)
            {
                return;
            }
            list.Sort((a, b) => CompareNodesByField(a, b, fieldName, order));
            for (int i = 0; i < list.Count; i++)
            {
                SortListRecursive(list[i].ChildNodes, fieldName, order);
            }
        }

        /// <summary>CompareNodesByField 方法。</summary>
        private int CompareNodesByField(HTreeListNode a, HTreeListNode b, string fieldName, HTreeListSortOrder order)
        {
            string ta = a.GetText(fieldName);
            string tb = b.GetText(fieldName);
            double da, db;
            bool na = double.TryParse(ta, out da);
            bool nb = double.TryParse(tb, out db);
            int cmp;
            if (na && nb)
            {
                cmp = da.CompareTo(db);
            }
            else
            {
                cmp = string.Compare(ta, tb, StringComparison.OrdinalIgnoreCase);
            }
            if (order == HTreeListSortOrder.Descending)
            {
                cmp = -cmp;
            }
            return cmp;
        }

        #endregion

        #region 批量更新

        /// <summary>开始批量更新（挂起重绘与重建）</summary>
        public void BeginUpdate()
        {
            _updateCount++;
        }

        /// <summary>结束批量更新（重建可见列表并重绘）</summary>
        public void EndUpdate()
        {
            if (_updateCount > 0)
            {
                _updateCount--;
            }
            if (_updateCount == 0)
            {
                RebuildVisibleList();
                LayoutScrollBars();
                Invalidate();
            }
        }

        /// <summary>重新挂钩列事件（运行时新增列后调用）</summary>
        public void RefreshColumns()
        {
            EnsureColumnsHooked();
            RebuildVisibleList();
            Invalidate();
        }

        #endregion

        #region 内部通知

        /// <summary>响应 OnStructureChanged 事件。</summary>
        internal void OnStructureChanged()
        {
            if (_updateCount > 0)
            {
                return;
            }
            RebuildVisibleList();
            Invalidate();
        }

        /// <summary>响应 OnNodeExpandedChanged 事件。</summary>
        internal void OnNodeExpandedChanged(HTreeListNode node)
        {
            if (_updateCount > 0)
            {
                return;
            }
            RebuildVisibleList();
            Invalidate();
            if (node.Expanded)
            {
                EventHandler ev = AfterExpand;
                if (ev != null)
                {
                    ev(this, EventArgs.Empty);
                }
            }
            else
            {
                EventHandler ev = AfterCollapse;
                if (ev != null)
                {
                    ev(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>响应 OnNodeCheckedChanged 事件。</summary>
        internal void OnNodeCheckedChanged(HTreeListNode node)
        {
            if (_updateCount > 0)
            {
                return;
            }
            Invalidate();
        }

        /// <summary>响应 OnNodeTextChanged 事件。</summary>
        internal void OnNodeTextChanged(HTreeListNode node)
        {
            if (_updateCount > 0)
            {
                return;
            }
            Invalidate();
        }

        #endregion

        #region 可见列表与滚动

        /// <summary>重建可见节点平铺列表（仅包含已展开可见的节点）</summary>
        private void RebuildVisibleList()
        {
            _visibleList.Clear();
            for (int i = 0; i < _rootNodes.Count; i++)
            {
                AddVisibleRecursive(_rootNodes[i]);
            }
            LayoutScrollBars();
        }

        /// <summary>AddVisibleRecursive 方法。</summary>
        private void AddVisibleRecursive(HTreeListNode node)
        {
            if (node == null)
            {
                return;
            }
            if (node.IsLazyPlaceholder)
            {
                return;
            }
            _visibleList.Add(node);
            if (node.Expanded && node.ChildNodes.Count > 0)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    AddVisibleRecursive(node.ChildNodes[i]);
                }
            }
        }

        /// <summary>获取 totalColumnWidth。</summary>
        private int GetTotalColumnWidth()
        {
            int total = 0;
            for (int i = 0; i < _columns.Count; i++)
            {
                if (_columns[i].Visible)
                {
                    total += _columns[i].Width;
                }
            }
            return total;
        }

        /// <summary>获取 contentBottom。</summary>
        private int GetContentBottom()
        {
            int b = ClientSize.Height;
            if (_showSummaryRow)
            {
                b -= _summaryHeight;
            }
            if (_hScrollBar != null && _hScrollBar.Visible)
            {
                b -= _hScrollBar.Height;
            }
            return b;
        }

        /// <summary>LayoutScrollBars 方法。</summary>
        private void LayoutScrollBars()
        {
            if (_vScrollBar == null || _hScrollBar == null || !IsHandleCreated)
            {
                return;
            }
            int vWidth = SystemInformation.VerticalScrollBarWidth;
            int hHeight = SystemInformation.HorizontalScrollBarHeight;

            int contentHeight = ClientSize.Height - _headerHeight - (_showSummaryRow ? _summaryHeight : 0);
            int totalRowsHeight = _visibleList.Count * _rowHeight;
            bool needV = totalRowsHeight > contentHeight;

            int availWidth = ClientSize.Width - (needV ? vWidth : 0);
            int totalColWidth = GetTotalColumnWidth();
            bool needH = totalColWidth > availWidth;

            if (needH)
            {
                int contentHeight2 = ClientSize.Height - _headerHeight - hHeight - (_showSummaryRow ? _summaryHeight : 0);
                needV = totalRowsHeight > contentHeight2;
            }

            _vScrollBar.Visible = needV;
            _hScrollBar.Visible = needH;

            _vScrollBar.Location = new Point(ClientSize.Width - vWidth, 0);
            _vScrollBar.Size = new Size(vWidth, ClientSize.Height - (needH ? hHeight : 0));

            _hScrollBar.Location = new Point(0, ClientSize.Height - hHeight);
            _hScrollBar.Size = new Size(ClientSize.Width - (needV ? vWidth : 0), hHeight);

            if (needV)
            {
                int visibleRows = Math.Max(1, contentHeight / _rowHeight);
                _vScrollBar.Minimum = 0;
                _vScrollBar.Maximum = Math.Max(0, _visibleList.Count - 1);
                _vScrollBar.LargeChange = visibleRows;
                _vScrollBar.SmallChange = 1;
                int maxVal = Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
                if (_firstVisibleIndex > maxVal)
                {
                    _firstVisibleIndex = maxVal;
                }
                if (_firstVisibleIndex < 0)
                {
                    _firstVisibleIndex = 0;
                }
                if (_firstVisibleIndex < _vScrollBar.Minimum)
                {
                    _vScrollBar.Value = _vScrollBar.Minimum;
                }
                else if (_firstVisibleIndex > maxVal)
                {
                    _vScrollBar.Value = maxVal;
                }
                else
                {
                    _vScrollBar.Value = _firstVisibleIndex;
                }
            }
            if (needH)
            {
                _hScrollBar.Minimum = 0;
                _hScrollBar.Maximum = Math.Max(0, totalColWidth - 1);
                _hScrollBar.LargeChange = Math.Max(1, availWidth);
                _hScrollBar.SmallChange = 20;
                int maxH = Math.Max(0, _hScrollBar.Maximum - _hScrollBar.LargeChange + 1);
                if (_horizontalOffset > maxH)
                {
                    _horizontalOffset = maxH;
                }
                if (_horizontalOffset < 0)
                {
                    _horizontalOffset = 0;
                }
                _hScrollBar.Value = _horizontalOffset;
            }
        }

        /// <summary>响应 VScroll 事件。</summary>
        private void OnVScroll(object sender, ScrollEventArgs e)
        {
            int newVal = e.NewValue;
            int maxVal = Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
            if (newVal < 0)
            {
                newVal = 0;
            }
            if (newVal > maxVal)
            {
                newVal = maxVal;
            }
            _firstVisibleIndex = newVal;
            Invalidate();
        }

        /// <summary>响应 HScroll 事件。</summary>
        private void OnHScroll(object sender, ScrollEventArgs e)
        {
            int newVal = e.NewValue;
            int maxVal = Math.Max(0, _hScrollBar.Maximum - _hScrollBar.LargeChange + 1);
            if (newVal < 0)
            {
                newVal = 0;
            }
            if (newVal > maxVal)
            {
                newVal = maxVal;
            }
            _horizontalOffset = newVal;
            Invalidate();
        }

        #endregion

        #region 重写

        /// <summary>响应 HandleCreated 事件。</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            EnsureColumnsHooked();
            LayoutScrollBars();
        }

        /// <summary>响应 FontChanged 事件。</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        /// <summary>响应 Resize 事件。</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutScrollBars();
            Invalidate();
        }

        /// <summary>响应 MouseWheel 事件。</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_vScrollBar == null || !_vScrollBar.Visible)
            {
                return;
            }
            int delta = e.Delta > 0 ? -3 : 3;
            int newVal = _firstVisibleIndex + delta;
            int maxVal = Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
            if (newVal < 0)
            {
                newVal = 0;
            }
            if (newVal > maxVal)
            {
                newVal = maxVal;
            }
            _firstVisibleIndex = newVal;
            if (_vScrollBar.Visible)
            {
                _vScrollBar.Value = newVal;
            }
            Invalidate();
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            Focus();
            Point pt = e.Location;

            // 列头区域：排序或列宽调整
            if (pt.Y < _headerHeight)
            {
                HandleHeaderMouseDown(pt, e);
                return;
            }

            // 节点行
            int visibleIndex = GetVisibleIndexAtY(pt.Y);
            if (visibleIndex < 0 || visibleIndex >= _visibleList.Count)
            {
                return;
            }
            HTreeListNode node = _visibleList[visibleIndex];

            // 展开/折叠图标
            if (node.HasChildNodes && GetExpandIconRect(node, visibleIndex).Contains(pt))
            {
                Toggle(node);
                return;
            }
            // 复选框
            if (_showCheckBoxes && GetCheckBoxRect(node, visibleIndex).Contains(pt))
            {
                ToggleCheck(node);
                return;
            }
            // 选择
            SelectedNode = node;
            if (e.Clicks >= 2 && node.HasChildNodes)
            {
                Toggle(node);
            }
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_resizingColumnIndex >= 0 && _resizingColumnIndex < _columns.Count)
            {
                int delta = e.X - _resizeStartX;
                int newWidth = _resizeStartWidth + delta;
                if (newWidth < MinColumnWidth)
                {
                    newWidth = MinColumnWidth;
                }
                _columns[_resizingColumnIndex].Width = newWidth;
                return;
            }
            if (e.Y < _headerHeight && GetSeparatorAtX(e.X) >= 0)
            {
                Cursor = Cursors.VSplit;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_resizingColumnIndex >= 0)
            {
                _resizingColumnIndex = -1;
                Capture = false;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush backBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(backBrush, e.ClipRectangle);
            }

            DrawHeader(g);
            DrawRows(g);
            if (_showSummaryRow)
            {
                DrawSummary(g);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_vScrollBar != null)
                {
                    _vScrollBar.Scroll -= OnVScroll;
                }
                if (_hScrollBar != null)
                {
                    _hScrollBar.Scroll -= OnHScroll;
                }
                if (_columns != null)
                {
                    for (int i = 0; i < _columns.Count; i++)
                    {
                        HTreeListColumn c = _columns[i];
                        c.Changed -= OnColumnChanged;
                        c.WidthChanged -= OnColumnWidthChanged;
                    }
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 列事件

        /// <summary>EnsureColumnsHooked 方法。</summary>
        private void EnsureColumnsHooked()
        {
            if (_columns == null)
            {
                return;
            }
            for (int i = 0; i < _columns.Count; i++)
            {
                HTreeListColumn c = _columns[i];
                c.Changed -= OnColumnChanged;
                c.WidthChanged -= OnColumnWidthChanged;
                c.Changed += OnColumnChanged;
                c.WidthChanged += OnColumnWidthChanged;
            }
        }

        /// <summary>响应 ColumnChanged 事件。</summary>
        private void OnColumnChanged(object sender, EventArgs e)
        {
            if (_updateCount > 0)
            {
                return;
            }
            RebuildVisibleList();
            Invalidate();
        }

        /// <summary>响应 ColumnWidthChanged 事件。</summary>
        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            if (_updateCount > 0)
            {
                return;
            }
            LayoutScrollBars();
            Invalidate();
        }

        #endregion

        #region 命中测试

        /// <summary>获取 visibleIndexAtY。</summary>
        private int GetVisibleIndexAtY(int y)
        {
            if (y < _headerHeight)
            {
                return -1;
            }
            int rel = y - _headerHeight;
            return _firstVisibleIndex + rel / _rowHeight;
        }

        /// <summary>获取 columnAtX。</summary>
        private int GetColumnAtX(int x)
        {
            int cx = -_horizontalOffset;
            for (int i = 0; i < _columns.Count; i++)
            {
                if (!_columns[i].Visible)
                {
                    continue;
                }
                int w = _columns[i].Width;
                if (x >= cx && x < cx + w)
                {
                    return i;
                }
                cx += w;
            }
            return -1;
        }

        /// <summary>获取 separatorAtX。</summary>
        private int GetSeparatorAtX(int x)
        {
            int cx = -_horizontalOffset;
            for (int i = 0; i < _columns.Count; i++)
            {
                if (!_columns[i].Visible)
                {
                    continue;
                }
                cx += _columns[i].Width;
                if (Math.Abs(cx - x) <= ResizeHitTolerance)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>获取 expandIconRect。</summary>
        private Rectangle GetExpandIconRect(HTreeListNode node, int visibleIndex)
        {
            int baseX = -_horizontalOffset;
            int x = baseX + node.Level * _indentSize;
            int y = _headerHeight + (visibleIndex - _firstVisibleIndex) * _rowHeight + (_rowHeight - ExpandIconSize) / 2;
            return new Rectangle(x, y, ExpandIconSize, ExpandIconSize);
        }

        /// <summary>获取 checkBoxRect。</summary>
        private Rectangle GetCheckBoxRect(HTreeListNode node, int visibleIndex)
        {
            int baseX = -_horizontalOffset;
            int indent = node.Level * _indentSize;
            int x = baseX + indent + (node.HasChildNodes ? ExpandIconSize + IconTextGap : 0);
            int y = _headerHeight + (visibleIndex - _firstVisibleIndex) * _rowHeight + (_rowHeight - CheckBoxSize) / 2;
            return new Rectangle(x, y, CheckBoxSize, CheckBoxSize);
        }

        /// <summary>HandleHeaderMouseDown 方法。</summary>
        private void HandleHeaderMouseDown(Point pt, MouseEventArgs e)
        {
            int sepIdx = GetSeparatorAtX(pt.X);
            if (sepIdx >= 0)
            {
                _resizingColumnIndex = sepIdx;
                _resizeStartX = pt.X;
                _resizeStartWidth = _columns[sepIdx].Width;
                Capture = true;
                return;
            }
            int colIdx = GetColumnAtX(pt.X);
            if (colIdx >= 0)
            {
                HTreeListColumn col = _columns[colIdx];
                if (col.Sortable)
                {
                    HTreeListSortOrder newOrder;
                    if (colIdx == _sortColumnIndex)
                    {
                        newOrder = (_sortOrder == HTreeListSortOrder.Ascending)
                            ? HTreeListSortOrder.Descending
                            : HTreeListSortOrder.Ascending;
                    }
                    else
                    {
                        newOrder = HTreeListSortOrder.Ascending;
                    }
                    SortColumn(colIdx, newOrder);
                }
            }
        }

        #endregion

        #region 绘制

        /// <summary>DrawHeader 方法。</summary>
        private void DrawHeader(Graphics g)
        {
            if (_columns == null || _columns.Count == 0)
            {
                return;
            }
            using (SolidBrush headerBrush = new SolidBrush(_headerColor))
            using (Pen linePen = new Pen(_lineColor))
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Near;
                int x = -_horizontalOffset;
                for (int i = 0; i < _columns.Count; i++)
                {
                    HTreeListColumn col = _columns[i];
                    if (!col.Visible)
                    {
                        continue;
                    }
                    int w = col.Width;
                    Rectangle rect = new Rectangle(x, 0, w, _headerHeight);
                    if (rect.Right <= 0)
                    {
                        x += w;
                        continue;
                    }
                    if (rect.Left > ClientSize.Width)
                    {
                        break;
                    }
                    g.FillRectangle(headerBrush, rect);
                    string header = string.IsNullOrEmpty(col.HeaderText) ? col.Name : col.HeaderText;
                    Rectangle textRect = new Rectangle(rect.X + 4, rect.Y, rect.Width - 8, rect.Height);
                    g.DrawString(header, Font, textBrush, textRect, sf);
                    // 排序标志
                    if (i == _sortColumnIndex && _sortOrder != HTreeListSortOrder.None)
                    {
                        SizeF hs = g.MeasureString(header, Font);
                        float gx = textRect.X + hs.Width + 6;
                        float gy = textRect.Y + textRect.Height / 2f - 3;
                        DrawSortGlyph(g, _sortOrder, gx, gy, _textColor);
                    }
                    // 列分隔线
                    g.DrawLine(linePen, rect.Right - 1, 0, rect.Right - 1, _headerHeight);
                    x += w;
                }
                // 列头底线
                g.DrawLine(linePen, 0, _headerHeight - 1, ClientSize.Width, _headerHeight - 1);
            }
        }

        /// <summary>DrawSortGlyph 方法。</summary>
        private void DrawSortGlyph(Graphics g, HTreeListSortOrder order, float x, float y, Color color)
        {
            PointF[] pts;
            if (order == HTreeListSortOrder.Ascending)
            {
                pts = new PointF[]
                {
                    new PointF(x, y + 6),
                    new PointF(x + 7, y + 6),
                    new PointF(x + 3.5f, y)
                };
            }
            else
            {
                pts = new PointF[]
                {
                    new PointF(x, y),
                    new PointF(x + 7, y),
                    new PointF(x + 3.5f, y + 6)
                };
            }
            using (SolidBrush b = new SolidBrush(color))
            {
                g.FillPolygon(b, pts);
            }
        }

        /// <summary>DrawRows 方法。</summary>
        private void DrawRows(Graphics g)
        {
            int top = _headerHeight;
            int bottom = GetContentBottom();
            if (bottom <= top)
            {
                return;
            }
            int maxRow = (bottom - top) / _rowHeight + 1;
            using (SolidBrush rowBrush = new SolidBrush(_rowColor))
            using (SolidBrush altBrush = new SolidBrush(_altRowColor))
            using (SolidBrush selectBrush = new SolidBrush(_selectColor))
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            using (Pen linePen = new Pen(_lineColor))
            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                for (int row = 0; row <= maxRow; row++)
                {
                    int index = _firstVisibleIndex + row;
                    if (index < 0 || index >= _visibleList.Count)
                    {
                        break;
                    }
                    int y = top + row * _rowHeight;
                    if (y >= bottom)
                    {
                        break;
                    }
                    HTreeListNode node = _visibleList[index];
                    Rectangle rowRect = new Rectangle(0, y, ClientSize.Width, _rowHeight);
                    bool selected = ReferenceEquals(node, _selectedNode);
                    if (selected)
                    {
                        g.FillRectangle(selectBrush, rowRect);
                    }
                    else
                    {
                        g.FillRectangle((index % 2) == 0 ? rowBrush : altBrush, rowRect);
                    }
                    DrawRowCells(g, node, y, textBrush, sf, linePen);
                    g.DrawLine(linePen, 0, y + _rowHeight - 1, ClientSize.Width, y + _rowHeight - 1);
                }
            }
        }

        /// <summary>DrawRowCells 方法。</summary>
        private void DrawRowCells(Graphics g, HTreeListNode node, int y, Brush textBrush, StringFormat sf, Pen linePen)
        {
            int x = -_horizontalOffset;
            for (int i = 0; i < _columns.Count; i++)
            {
                HTreeListColumn col = _columns[i];
                if (!col.Visible)
                {
                    continue;
                }
                int w = col.Width;
                Rectangle cellRect = new Rectangle(x, y, w, _rowHeight);
                if (cellRect.Right <= 0)
                {
                    x += w;
                    continue;
                }
                if (cellRect.Left > ClientSize.Width)
                {
                    break;
                }
                if (i == 0)
                {
                    DrawTreeCell(g, node, cellRect, textBrush, sf, linePen);
                }
                else
                {
                    string fieldName = string.IsNullOrEmpty(col.FieldName) ? col.Name : col.FieldName;
                    string val = FormatValue(node, col, node.GetText(fieldName));
                    sf.Alignment = ToStringAlignment(col.TextAlign);
                    Rectangle textRect = new Rectangle(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height);
                    g.DrawString(val, Font, textBrush, textRect, sf);
                }
                g.DrawLine(linePen, cellRect.Right - 1, y, cellRect.Right - 1, y + _rowHeight);
                x += w;
            }
        }

        /// <summary>DrawTreeCell 方法。</summary>
        private void DrawTreeCell(Graphics g, HTreeListNode node, Rectangle cellRect, Brush textBrush, StringFormat sf, Pen linePen)
        {
            int level = node.Level;
            int indent = level * _indentSize;
            int iconX = cellRect.X + indent;
            int iconY = cellRect.Y + (_rowHeight - ExpandIconSize) / 2;

            // 树形连接线
            if (_showTreeLines)
            {
                DrawTreeLines(g, node, cellRect, linePen);
            }

            // 展开/折叠图标
            if (node.HasChildNodes)
            {
                DrawExpandIcon(g, node, iconX, iconY, linePen);
                iconX += ExpandIconSize + IconTextGap;
            }

            // 复选框
            if (_showCheckBoxes)
            {
                DrawCheckBox(g, node, iconX, iconY, linePen);
                iconX += CheckBoxSize + IconTextGap;
            }

            // 文本
            sf.Alignment = StringAlignment.Near;
            string fieldName = (_columns.Count > 0)
                ? (string.IsNullOrEmpty(_columns[0].FieldName) ? _columns[0].Name : _columns[0].FieldName)
                : "Name";
            string val = node.GetText(fieldName);
            Rectangle textRect = new Rectangle(iconX, cellRect.Y, cellRect.Right - iconX - 4, cellRect.Height);
            if (textRect.Width > 0)
            {
                g.DrawString(val, Font, textBrush, textRect, sf);
            }
        }

        /// <summary>DrawExpandIcon 方法。</summary>
        private void DrawExpandIcon(Graphics g, HTreeListNode node, int x, int y, Pen pen)
        {
            Rectangle r = new Rectangle(x, y, ExpandIconSize, ExpandIconSize);
            g.DrawRectangle(pen, r);
            int mid = y + ExpandIconSize / 2;
            g.DrawLine(pen, x + 3, mid, x + ExpandIconSize - 3, mid);
            if (!node.Expanded)
            {
                int ctr = x + ExpandIconSize / 2;
                g.DrawLine(pen, ctr, y + 3, ctr, y + ExpandIconSize - 3);
            }
        }

        /// <summary>DrawCheckBox 方法。</summary>
        private void DrawCheckBox(Graphics g, HTreeListNode node, int x, int y, Pen pen)
        {
            Rectangle r = new Rectangle(x, y, CheckBoxSize, CheckBoxSize);
            g.DrawRectangle(pen, r);
            switch (node.CheckedState)
            {
                case HTreeListCheckState.Checked:
                    g.DrawLine(pen, x + 3, y + 8, x + 6, y + 12);
                    g.DrawLine(pen, x + 6, y + 12, x + 13, y + 3);
                    break;
                case HTreeListCheckState.Indeterminate:
                    using (SolidBrush b = new SolidBrush(_textColor))
                    {
                        g.FillRectangle(b, x + 4, y + 4, CheckBoxSize - 8, CheckBoxSize - 8);
                    }
                    break;
            }
        }

        /// <summary>DrawTreeLines 方法。</summary>
        private void DrawTreeLines(Graphics g, HTreeListNode node, Rectangle cellRect, Pen pen)
        {
            int level = node.Level;
            for (int l = 0; l < level; l++)
            {
                int lx = cellRect.X + l * _indentSize + _indentSize / 2;
                g.DrawLine(pen, lx, cellRect.Top, lx, cellRect.Bottom);
            }
            int nodeLineX = cellRect.X + level * _indentSize + _indentSize / 2;
            int half = cellRect.Top + _rowHeight / 2;
            g.DrawLine(pen, nodeLineX, cellRect.Top, nodeLineX, half);
            g.DrawLine(pen, nodeLineX, half, cellRect.X + level * _indentSize, half);
        }

        /// <summary>DrawSummary 方法。</summary>
        private void DrawSummary(Graphics g)
        {
            int y = ClientSize.Height - _summaryHeight - (_hScrollBar.Visible ? _hScrollBar.Height : 0);
            if (y < _headerHeight)
            {
                return;
            }
            using (SolidBrush b = new SolidBrush(_headerColor))
            using (SolidBrush textBrush = new SolidBrush(_textColor))
            using (Pen pen = new Pen(_lineColor))
            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                g.FillRectangle(b, 0, y, ClientSize.Width, _summaryHeight);
                int x = -_horizontalOffset;
                for (int i = 0; i < _columns.Count; i++)
                {
                    HTreeListColumn col = _columns[i];
                    if (!col.Visible)
                    {
                        continue;
                    }
                    int w = col.Width;
                    Rectangle cellRect = new Rectangle(x, y, w, _summaryHeight);
                    string fieldName = string.IsNullOrEmpty(col.FieldName) ? col.Name : col.FieldName;
                    HTreeListSummaryType st = GetSummary(fieldName);
                    string val = ComputeSummary(fieldName, st);
                    sf.Alignment = (st == HTreeListSummaryType.Count) ? StringAlignment.Center : StringAlignment.Far;
                    Rectangle textRect = new Rectangle(cellRect.X + 4, cellRect.Y, cellRect.Width - 8, cellRect.Height);
                    g.DrawString(val, Font, textBrush, textRect, sf);
                    g.DrawLine(pen, cellRect.Right - 1, y, cellRect.Right - 1, y + _summaryHeight);
                    x += w;
                }
            }
        }

        /// <summary>ComputeSummary 方法。</summary>
        private string ComputeSummary(string fieldName, HTreeListSummaryType st)
        {
            if (st == HTreeListSummaryType.None)
            {
                return string.Empty;
            }
            if (st == HTreeListSummaryType.Count)
            {
                return _visibleList.Count.ToString();
            }
            double sum = 0;
            int cnt = 0;
            for (int i = 0; i < _visibleList.Count; i++)
            {
                string v = _visibleList[i].GetText(fieldName);
                double d;
                if (double.TryParse(v, out d))
                {
                    sum += d;
                    cnt++;
                }
            }
            if (st == HTreeListSummaryType.Sum)
            {
                return sum.ToString();
            }
            if (st == HTreeListSummaryType.Average)
            {
                return cnt > 0 ? (sum / cnt).ToString() : "0";
            }
            return string.Empty;
        }

        /// <summary>FormatValue 方法。</summary>
        private string FormatValue(HTreeListNode node, HTreeListColumn col, string raw)
        {
            if (string.IsNullOrEmpty(col.Format) || string.IsNullOrEmpty(raw))
            {
                return raw;
            }
            double dnum;
            if (double.TryParse(raw, out dnum))
            {
                try
                {
                    return dnum.ToString(col.Format);
                }
                catch
                {
                    return raw;
                }
            }
            DateTime dt;
            if (DateTime.TryParse(raw, out dt))
            {
                try
                {
                    return dt.ToString(col.Format);
                }
                catch
                {
                    return raw;
                }
            }
            return raw;
        }

        /// <summary>转换为 StringAlignment。</summary>
        private static StringAlignment ToStringAlignment(HorizontalAlignment a)
        {
            switch (a)
            {
                case HorizontalAlignment.Right:
                    return StringAlignment.Far;
                case HorizontalAlignment.Center:
                    return StringAlignment.Center;
                default:
                    return StringAlignment.Near;
            }
        }

        #endregion
    }
}