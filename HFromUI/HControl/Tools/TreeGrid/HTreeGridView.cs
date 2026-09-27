using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.TreeGrid
{
    using HFromUI.HControl.Base;
    using HFromUI.HFrom.HUiKit;

    /// <summary>单元格文字对齐方式。</summary>
    public enum HTreeGridAlign
    {
        /// <summary>左对齐。</summary>
        Left,
        /// <summary>居中。</summary>
        Center,
        /// <summary>右对齐。</summary>
        Right
    }

    /// <summary>单元格内置标记：无 / 勾选 √ / 叉 ×。</summary>
    public enum HTreeGridMark
    {
        /// <summary>不画标记，按普通文字值绘制。</summary>
        None,
        /// <summary>黑色加粗对勾（启用列）。</summary>
        Check,
        /// <summary>黑色加粗叉号（机台不可用列）。</summary>
        Cross
    }

    /// <summary>树形列节点前缀小图标（吸嘴/印章形，参考工艺表拾取动作）。</summary>
    public enum HTreeGridNodeGlyph
    {
        /// <summary>无图标。</summary>
        None,
        /// <summary>吸嘴/冲压印章形图标。</summary>
        Stamp
    }

    /// <summary>
    /// 树表格列定义：表头文字、列宽（像素）、内容对齐。
    /// 任意一列都可通过 <see cref="HTreeGridView.TreeColumnIndex"/> 指定为承载缩进与折叠框的树形列。
    /// </summary>
    public class HTreeGridColumn
    {
        /// <summary>构造一列。</summary>
        public HTreeGridColumn(string text, int width) : this(text, width, HTreeGridAlign.Center) { }
        /// <summary>构造一列并指定对齐。</summary>
        public HTreeGridColumn(string text, int width, HTreeGridAlign align)
        {
            Text = text ?? "";
            Width = Math.Max(24, width);
            Align = align;
        }

        /// <summary>表头文字。</summary>
        [Category("HFromUI"), Description("表头文字")]
        public string Text { get; set; }

        /// <summary>列宽（像素，可由表头分隔线拖拽改变）。</summary>
        [Category("HFromUI"), Description("列宽（像素）"), DefaultValue(120)]
        public int Width { get; set; }

        /// <summary>内容对齐方式。</summary>
        [Category("HFromUI"), Description("内容对齐"), DefaultValue(HTreeGridAlign.Center)]
        public HTreeGridAlign Align { get; set; }

        /// <summary>自定义数据。</summary>
        [DefaultValue(null)]
        public object Tag { get; set; }
    }

    /// <summary>
    /// 树表格节点：对应一行，Values 按列序存放单元格值（string/数值直接显示；bool=true 显示对勾），
    /// Mark 可强制把某列画成对勾或叉号；Children 下挂子行，父行即分组行（金色底，可折叠子行）。
    /// 用法：
    ///   var load = grid.AddNode("Load", true);
    ///   load.Add("拾取ld", true, "HIS-400G-LD", "中转台穴位6");
    /// </summary>
    [DefaultProperty("Text")]
    public class HTreeGridNode
    {
        internal object[] hValues;
        internal HTreeGridMark[] hMarks;
        internal HTreeGridNodeGlyph hGlyph;
        internal bool hExpanded;
        internal HTreeGridNode hParent;
        internal readonly List<HTreeGridNode> hChildren = new List<HTreeGridNode>();

        /// <summary>构造空节点。</summary>
        public HTreeGridNode() : this(new object[0]) { }

        /// <summary>按列值构造节点（可任意个数，不足列留空）。</summary>
        public HTreeGridNode(params object[] values)
        {
            hValues = values ?? new object[0];
        }

        /// <summary>自定义数据。</summary>
        [DefaultValue(null)]
        public object Tag { get; set; }

        /// <summary>树形列文字（第 0 列值的快捷读写）。</summary>
        [Browsable(false)]
        public string Text
        {
            get { return GetValue(0) as string; }
            set { Ensure(1); hValues[0] = value; }
        }

        /// <summary>树形列前缀图标。</summary>
        [Category("HFromUI"), Description("树形列前缀图标"), DefaultValue(HTreeGridNodeGlyph.None)]
        public HTreeGridNodeGlyph Glyph
        {
            get { return hGlyph; }
            set { hGlyph = value; }
        }

        /// <summary>是否已展开（有子行时生效）。</summary>
        [Category("HFromUI"), Description("是否已展开"), DefaultValue(false)]
        public bool Expanded
        {
            get { return hExpanded; }
            set { hExpanded = value; }
        }

        /// <summary>父行（根行为 null）。</summary>
        [Browsable(false)]
        public HTreeGridNode Parent { get { return hParent; } }

        /// <summary>子行集合。</summary>
        [Browsable(false)]
        public List<HTreeGridNode> Nodes { get { return hChildren; } }

        /// <summary>是否有子行。</summary>
        [Browsable(false)]
        public bool HasChildren { get { return hChildren.Count > 0; } }

        /// <summary>层级（根行为 0）。</summary>
        [Browsable(false)]
        public int Level
        {
            get
            {
                int lv = 0;
                HTreeGridNode p = hParent;
                while (p != null) { lv++; p = p.hParent; }
                return lv;
            }
        }

        private void Ensure(int count)
        {
            if (hValues.Length < count) Array.Resize(ref hValues, count);
        }

        private void EnsureMarks(int count)
        {
            if (hMarks == null) hMarks = new HTreeGridMark[count];
            else if (hMarks.Length < count) Array.Resize(ref hMarks, count);
        }

        /// <summary>取第 index 列的值（越界返回 null）。</summary>
        public object GetValue(int index)
        {
            if (index < 0 || index >= hValues.Length) return null;
            return hValues[index];
        }

        /// <summary>设置第 index 列的值；bool=true 自动画对勾。</summary>
        public HTreeGridNode SetValue(int index, object value)
        {
            if (index < 0) return this;
            Ensure(index + 1);
            hValues[index] = value;
            return this;
        }

        /// <summary>取第 index 列的强制标记。</summary>
        public HTreeGridMark GetMark(int index)
        {
            if (hMarks == null || index < 0 || index >= hMarks.Length) return HTreeGridMark.None;
            return hMarks[index];
        }

        /// <summary>强制第 index 列画成对勾/叉号（设回 None 恢复普通文字）。</summary>
        public HTreeGridNode SetMark(int index, HTreeGridMark mark)
        {
            if (index < 0) return this;
            EnsureMarks(index + 1);
            hMarks[index] = mark;
            return this;
        }

        /// <summary>添加已构造的子行，返回该子行。</summary>
        public HTreeGridNode Add(HTreeGridNode node)
        {
            if (node == null) return null;
            if (node.hParent != null) node.hParent.hChildren.Remove(node);
            node.hParent = this;
            hChildren.Add(node);
            return node;
        }

        /// <summary>按列值快捷创建并添加子行，返回新行。</summary>
        public HTreeGridNode Add(params object[] values)
        {
            return Add(new HTreeGridNode(values));
        }

        /// <summary>移除子行。</summary>
        public void Remove(HTreeGridNode node)
        {
            if (node == null) return;
            if (hChildren.Remove(node)) node.hParent = null;
        }

        /// <summary>清空子行。</summary>
        public void Clear()
        {
            foreach (HTreeGridNode n in hChildren) n.hParent = null;
            hChildren.Clear();
        }
    }

    /// <summary>树表格行事件参数。</summary>
    public class HTreeGridNodeEventArgs : EventArgs
    {
        /// <summary>构造事件参数。</summary>
        public HTreeGridNodeEventArgs(HTreeGridNode node) { Node = node; }
        /// <summary>触发事件的行。</summary>
        public HTreeGridNode Node { get; private set; }
    }

    /// <summary>树表格单元格事件参数。</summary>
    public class HTreeGridCellEventArgs : EventArgs
    {
        /// <summary>构造事件参数。</summary>
        public HTreeGridCellEventArgs(HTreeGridNode node, int columnIndex)
        {
            Node = node;
            ColumnIndex = columnIndex;
        }
        /// <summary>所在行。</summary>
        public HTreeGridNode Node { get; private set; }
        /// <summary>所在列索引。</summary>
        public int ColumnIndex { get; private set; }
    }

    /// <summary>列集合（Add 时自动通知所属表格重排）。</summary>
    public class HTreeGridColumnCollection : List<HTreeGridColumn>
    {
        private readonly HTreeGridView hOwner;
        internal HTreeGridColumnCollection(HTreeGridView owner) { hOwner = owner; }
        /// <summary>追加一列（默认居中），返回新列。</summary>
        public HTreeGridColumn Add(string text, int width)
        {
            var col = new HTreeGridColumn(text, width);
            Add(col);
            hOwner.OnColumnsChanged();
            return col;
        }
        /// <summary>追加一列并指定对齐，返回新列。</summary>
        public HTreeGridColumn Add(string text, int width, HTreeGridAlign align)
        {
            var col = new HTreeGridColumn(text, width, align);
            Add(col);
            hOwner.OnColumnsChanged();
            return col;
        }
    }

    /// <summary>
    /// 树形折叠表格（参考工艺配置表）：
    /// 表头 + 多列数据行，可指定任意一列为“树形列”——分组行在该列显示 [+]/[-] 折叠框、层级缩进与树连接线，
    /// 折叠后其下子行整体隐藏。分组行金色底加粗，子行浅灰底；单元格支持对勾/叉号标记。
    /// 纯 GDI+ 自绘，继承 HControl.Base.HLabelBase，支持列宽拖拽、垂直/水平滚动条、滚轮与键盘导航。
    /// </summary>
    [DefaultEvent("RowClick")]
    [ToolboxItem(true)]
    public class HTreeGridView : HLabelBase
    {
        #region 内部结构
        private class HRow
        {
            public HTreeGridNode Node;
            public int Level;
            public int Y;          // 行顶（含滚动偏移）
            public int Height;
            /// <summary>各级祖先是否还有后继兄弟（控制树竖向连接线），长度等于 Level。</summary>
            public bool[] Continuing;
        }
        #endregion

        #region 字段
        // 字段初始化器先于基类构造体执行：基类 HLabelBase 构造时设置 Font 会触发 OnResize，届时列集合必须已存在
        private readonly HTreeGridColumnCollection hColumns = new HTreeGridColumnCollection(null);
        private readonly List<HTreeGridNode> hNodes = new List<HTreeGridNode>();
        private readonly List<HRow> hRows = new List<HRow>();
        private Font hBoldFont;

        private int hTreeColumnIndex;
        private int hHeaderHeight = 40;
        private int hGroupRowHeight = 46;
        private int hDataRowHeight = 46;
        private int hIndent = 22;

        private HTreeGridNode hSelected;
        private HRow hHoverRow;
        private int hScrollY;
        private int hScrollX;
        private int hPressedColumn = -1;   // 鼠标按下的列分隔条（列宽拖拽）
        private int hDragColumn = -1;
        private int hDragStartX;
        private int hDragStartWidth;
        private bool hThumbV;
        private bool hThumbH;
        private int hThumbDragOffset;
        private bool hShowVBar;
        private bool hShowHBar;

        // 外观
        private Color hHeaderBackColor = Color.White;
        private Color hHeaderForeColor = Color.FromArgb(166, 166, 166);
        private Color hGridLineColor = Color.FromArgb(230, 230, 230);
        private Color hBorderColor = Color.FromArgb(221, 221, 221);
        private Color hGroupBackColor = Color.FromArgb(246, 183, 60);
        private Color hGroupForeColor = Color.FromArgb(45, 40, 30);
        private Color hRowBackColor = Color.FromArgb(242, 242, 242);
        private Color hRowForeColor = Color.FromArgb(38, 38, 38);
        private Color hMarkColor = Color.FromArgb(20, 20, 20);
        private Color hTreeLineColor = Color.FromArgb(122, 105, 232);
        private Color hBoxBackColor = Color.White;
        private Color hBoxBorderColor = Color.FromArgb(170, 170, 170);
        private Color hSelectionColor = Color.FromArgb(38, 34, 102, 238);
        private Color hScrollThumbColor = Color.FromArgb(201, 201, 201);
        #endregion

        #region 事件
        /// <summary>选中行变化后触发。</summary>
        public event EventHandler<HTreeGridNodeEventArgs> AfterSelect;
        /// <summary>分组行展开后触发。</summary>
        public event EventHandler<HTreeGridNodeEventArgs> AfterExpand;
        /// <summary>分组行折叠后触发。</summary>
        public event EventHandler<HTreeGridNodeEventArgs> AfterCollapse;
        /// <summary>行被点击。</summary>
        public event EventHandler<HTreeGridNodeEventArgs> RowClick;
        /// <summary>单元格被点击（参数含列索引）。</summary>
        public event EventHandler<HTreeGridCellEventArgs> CellClick;
        /// <summary>列宽拖拽结束后触发。</summary>
        public event EventHandler ColumnWidthChanged;
        #endregion

        /// <summary>构造树形折叠表格。</summary>
        public HTreeGridView()
        {
            // 基类默认 AutoSize=true 且按标签测量尺寸，表格为定高容器，必须先关闭再设尺寸
            AutoSize = false;
            SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 10F);
            Size = new Size(720, 420);
            TabStop = true;
            // 替换为真实宿主（字段初始化阶段先放过空宿主，保证基类构造期间 OnResize 不踩空）
            hColumns = new HTreeGridColumnCollection(this);
        }

        #region 属性
        /// <summary>列集合（Add(text, width) 追加列）。</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("HFromUI"), Description("列集合")]
        public HTreeGridColumnCollection Columns { get { return hColumns; } }

        /// <summary>根行集合。</summary>
        [Browsable(false)]
        public List<HTreeGridNode> Nodes { get { return hNodes; } }

        /// <summary>当前选中行（null 表示无选中）。</summary>
        [Browsable(false)]
        public HTreeGridNode SelectedNode
        {
            get { return hSelected; }
            set
            {
                if (hSelected == value) return;
                hSelected = value;
                EnsureVisible(value);
                Invalidate();
                AfterSelect?.Invoke(this, new HTreeGridNodeEventArgs(value));
            }
        }

        /// <summary>承载折叠框与缩进的树形列索引（默认第 0 列，可指定任意列）。</summary>
        [Category("HFromUI"), Description("树形结构所在列索引"), DefaultValue(0)]
        public int TreeColumnIndex
        {
            get { return hTreeColumnIndex; }
            set
            {
                int v = value;
                if (hColumns.Count > 0) v = Math.Max(0, Math.Min(hColumns.Count - 1, v));
                if (hTreeColumnIndex == v) return;
                hTreeColumnIndex = v;
                Invalidate();
            }
        }

        /// <summary>表头高度。</summary>
        [Category("HFromUI"), Description("表头高度"), DefaultValue(40)]
        public int HeaderHeight
        {
            get { return hHeaderHeight; }
            set { hHeaderHeight = Math.Max(24, value); RebuildAndInvalidate(); }
        }

        /// <summary>分组行高。</summary>
        [Category("HFromUI"), Description("分组行高"), DefaultValue(46)]
        public int GroupRowHeight
        {
            get { return hGroupRowHeight; }
            set { hGroupRowHeight = Math.Max(24, value); RebuildAndInvalidate(); }
        }

        /// <summary>数据子行高。</summary>
        [Category("HFromUI"), Description("数据子行高"), DefaultValue(46)]
        public int DataRowHeight
        {
            get { return hDataRowHeight; }
            set { hDataRowHeight = Math.Max(20, value); RebuildAndInvalidate(); }
        }

        /// <summary>树形列每级缩进像素。</summary>
        [Category("HFromUI"), Description("树形列每级缩进"), DefaultValue(22)]
        public int Indent
        {
            get { return hIndent; }
            set { hIndent = Math.Max(12, value); Invalidate(); }
        }

        /// <summary>表头背景色。</summary>
        [Category("HFromUI 外观"), Description("表头背景色")]
        public Color HeaderBackColor { get { return hHeaderBackColor; } set { hHeaderBackColor = value; Invalidate(); } }
        /// <summary>表头文字颜色。</summary>
        [Category("HFromUI 外观"), Description("表头文字颜色")]
        public Color HeaderForeColor { get { return hHeaderForeColor; } set { hHeaderForeColor = value; Invalidate(); } }
        /// <summary>网格线颜色。</summary>
        [Category("HFromUI 外观"), Description("网格线颜色")]
        public Color GridLineColor { get { return hGridLineColor; } set { hGridLineColor = value; Invalidate(); } }
        /// <summary>分组行金色底色。</summary>
        [Category("HFromUI 外观"), Description("分组行底色")]
        public Color GroupBackColor { get { return hGroupBackColor; } set { hGroupBackColor = value; Invalidate(); } }
        /// <summary>分组行文字颜色。</summary>
        [Category("HFromUI 外观"), Description("分组行文字颜色")]
        public Color GroupForeColor { get { return hGroupForeColor; } set { hGroupForeColor = value; Invalidate(); } }
        /// <summary>数据子行底色。</summary>
        [Category("HFromUI 外观"), Description("数据子行底色")]
        public Color RowBackColor { get { return hRowBackColor; } set { hRowBackColor = value; Invalidate(); } }
        /// <summary>数据子行文字颜色。</summary>
        [Category("HFromUI 外观"), Description("数据子行文字颜色")]
        public Color RowForeColor { get { return hRowForeColor; } set { hRowForeColor = value; Invalidate(); } }
        /// <summary>对勾/叉号颜色。</summary>
        [Category("HFromUI 外观"), Description("对勾叉号颜色")]
        public Color MarkColor { get { return hMarkColor; } set { hMarkColor = value; Invalidate(); } }
        /// <summary>树连接线颜色。</summary>
        [Category("HFromUI 外观"), Description("树连接线颜色")]
        public Color TreeLineColor { get { return hTreeLineColor; } set { hTreeLineColor = value; Invalidate(); } }
        /// <summary>选中行覆盖色（建议带透明度）。</summary>
        [Category("HFromUI 外观"), Description("选中行覆盖色")]
        public Color SelectionColor { get { return hSelectionColor; } set { hSelectionColor = value; Invalidate(); } }
        #endregion

        #region 公共方法
        /// <summary>快捷添加根行，返回新行。</summary>
        public HTreeGridNode AddNode(params object[] values)
        {
            var node = new HTreeGridNode(values);
            hNodes.Add(node);
            RebuildAndInvalidate();
            return node;
        }

        /// <summary>全部展开。</summary>
        public void ExpandAll()
        {
            foreach (HTreeGridNode root in hNodes) ExpandRecursive(root);
            ClampScroll();
            RebuildAndInvalidate();
        }

        /// <summary>全部折叠。</summary>
        public void CollapseAll()
        {
            foreach (HTreeGridNode root in hNodes) CollapseRecursive(root);
            ClampScroll();
            RebuildAndInvalidate();
        }

        /// <summary>沿祖先链展开并滚动到指定行。</summary>
        public void EnsureVisible(HTreeGridNode node)
        {
            if (node == null) return;
            bool changed = false;
            HTreeGridNode p = node.hParent;
            while (p != null)
            {
                if (!p.hExpanded) { p.hExpanded = true; changed = true; }
                p = p.hParent;
            }
            RebuildRows();
            HRow row = FindRow(node);
            if (row != null)
            {
                int bodyTop = hHeaderHeight;
                int bodyH = BodyHeight();
                if (row.Y < bodyTop) hScrollY += row.Y - bodyTop - 2;
                else if (row.Y + row.Height > bodyTop + bodyH)
                    hScrollY += row.Y + row.Height - (bodyTop + bodyH) + 2;
                ClampScroll();
                RebuildRows();
            }
            else if (changed)
            {
                ClampScroll();
                RebuildRows();
            }
            Invalidate();
        }

        internal void OnColumnsChanged()
        {
            if (hTreeColumnIndex > hColumns.Count - 1) hTreeColumnIndex = Math.Max(0, hColumns.Count - 1);
            ClampScroll();
            RebuildRows();
            Invalidate();
        }
        #endregion

        #region 布局与展平
        private static void ExpandRecursive(HTreeGridNode node)
        {
            if (node.hChildren.Count == 0) return;
            node.hExpanded = true;
            foreach (HTreeGridNode c in node.hChildren) ExpandRecursive(c);
        }

        private static void CollapseRecursive(HTreeGridNode node)
        {
            if (node.hChildren.Count == 0) return;
            node.hExpanded = false;
            foreach (HTreeGridNode c in node.hChildren) CollapseRecursive(c);
        }

        private int ContentWidth()
        {
            int w = 0;
            foreach (HTreeGridColumn c in hColumns) w += c.Width;
            return w;
        }

        private int BodyHeight()
        {
            int h = ClientSize.Height - hHeaderHeight - (hShowHBar ? ScrollBarThickness : 0);
            return Math.Max(0, h);
        }

        private int ViewWidth()
        {
            return ClientSize.Width - (hShowVBar ? ScrollBarThickness : 0);
        }

        private int ContentHeight()
        {
            int h = 0;
            foreach (HTreeGridNode root in hNodes)
            {
                h += RowH(root);
                if (root.hExpanded) h += SubtreeHeight(root);
            }
            return h;
        }

        private int SubtreeHeight(HTreeGridNode node)
        {
            int h = 0;
            foreach (HTreeGridNode c in node.hChildren)
            {
                h += RowH(c);
                if (c.hExpanded) h += SubtreeHeight(c);
            }
            return h;
        }

        private int RowH(HTreeGridNode node)
        {
            return node.hChildren.Count > 0 ? hGroupRowHeight : hDataRowHeight;
        }

        /// <summary>两遍计算滚动条可见性：先按全宽判横条，再据此判竖条，最后回头校正横条，避免互相递归。</summary>
        private void UpdateScrollBars()
        {
            bool hMaybe = ContentWidth() > ClientSize.Width;
            int availH = Math.Max(0, ClientSize.Height - hHeaderHeight - (hMaybe ? ScrollBarThickness : 0));
            hShowVBar = ContentHeight() > availH;
            hShowHBar = ContentWidth() > ClientSize.Width - (hShowVBar ? ScrollBarThickness : 0);
        }

        private void ClampScroll()
        {
            UpdateScrollBars();
            int maxY = Math.Max(0, ContentHeight() - BodyHeight());
            if (hScrollY > maxY) hScrollY = maxY;
            if (hScrollY < 0) hScrollY = 0;
            int maxX = Math.Max(0, ContentWidth() - ViewWidth());
            if (hScrollX > maxX) hScrollX = maxX;
            if (hScrollX < 0) hScrollX = 0;
        }

        private void RebuildRows()
        {
            hRows.Clear();
            int y = hHeaderHeight - hScrollY;
            foreach (HTreeGridNode root in hNodes)
                y = Collect(root, 0, y, new bool[0]);
        }

        private int Collect(HTreeGridNode node, int level, int y, bool[] continuing)
        {
            int rh = RowH(node);
            hRows.Add(new HRow { Node = node, Level = level, Y = y, Height = rh, Continuing = continuing });
            y += rh;
            if (node.hExpanded && node.hChildren.Count > 0)
            {
                for (int i = 0; i < node.hChildren.Count; i++)
                {
                    bool[] childCont = new bool[level + 1];
                    Array.Copy(continuing, childCont, level);
                    childCont[level] = i < node.hChildren.Count - 1;
                    y = Collect(node.hChildren[i], level + 1, y, childCont);
                }
            }
            return y;
        }

        private void RebuildAndInvalidate()
        {
            ClampScroll();
            RebuildRows();
            Invalidate();
        }

        private HRow FindRow(HTreeGridNode node)
        {
            foreach (HRow r in hRows) if (r.Node == node) return r;
            return null;
        }

        /// <summary>列左缘 X（含横向滚动）。</summary>
        private int ColumnX(int index)
        {
            int x = -hScrollX;
            for (int i = 0; i < index && i < hColumns.Count; i++) x += hColumns[i].Width;
            return x;
        }

        /// <summary>命中列（-1 表示在表体之外）。</summary>
        private int ColumnAt(int x)
        {
            int acc = -hScrollX;
            for (int i = 0; i < hColumns.Count; i++)
            {
                acc += hColumns[i].Width;
                if (x < acc) return i;
            }
            return -1;
        }
        #endregion

        #region 滚动条几何
        private const int ScrollBarThickness = 9;

        private bool ShowVBar() { return hShowVBar; }
        private bool ShowHBar() { return hShowHBar; }

        private Rectangle VBarRect()
        {
            return new Rectangle(ClientSize.Width - ScrollBarThickness, hHeaderHeight,
                ScrollBarThickness, ClientSize.Height - hHeaderHeight - (ShowHBar() ? ScrollBarThickness : 0));
        }

        private Rectangle HBarRect()
        {
            return new Rectangle(0, ClientSize.Height - ScrollBarThickness,
                ClientSize.Width - (ShowVBar() ? ScrollBarThickness : 0), ScrollBarThickness);
        }

        private Rectangle VThumbRect()
        {
            Rectangle bar = VBarRect();
            int content = Math.Max(1, ContentHeight());
            int thumbH = Math.Max(24, (int)Math.Round(bar.Height * (double)BodyHeight() / content));
            int max = Math.Max(0, content - BodyHeight());
            int y = max <= 0 ? bar.Top : bar.Top + (int)Math.Round((bar.Height - thumbH) * (double)hScrollY / max);
            return new Rectangle(bar.X + 1, y, bar.Width - 2, thumbH);
        }

        private Rectangle HThumbRect()
        {
            Rectangle bar = HBarRect();
            int content = Math.Max(1, ContentWidth());
            int thumbW = Math.Max(24, (int)Math.Round(bar.Width * (double)ViewWidth() / content));
            int max = Math.Max(0, content - ViewWidth());
            int x = max <= 0 ? bar.Left : bar.Left + (int)Math.Round((bar.Width - thumbW) * (double)hScrollX / max);
            return new Rectangle(x, bar.Y + 1, thumbW, bar.Height - 2);
        }
        #endregion

        #region 交互
        private Rectangle ToggleBoxRect(HRow row)
        {
            int colX = ColumnX(Math.Min(hTreeColumnIndex, hColumns.Count - 1));
            int x = colX + 8 + row.Level * hIndent;
            int y = row.Y + (row.Height - 14) / 2;
            return new Rectangle(x, y, 14, 14);
        }

        private HRow HitRow(Point pt)
        {
            if (pt.Y < hHeaderHeight || pt.X < 0 || pt.X > ClientSize.Width) return null;
            for (int i = hRows.Count - 1; i >= 0; i--)
            {
                HRow r = hRows[i];
                if (pt.Y >= r.Y && pt.Y < r.Y + r.Height) return r;
            }
            return null;
        }

        /// <summary>表头分隔拖拽条命中（返回右侧列索引）。</summary>
        private int DividerAt(Point pt)
        {
            if (pt.Y >= hHeaderHeight) return -1;
            int acc = -hScrollX;
            for (int i = 0; i < hColumns.Count; i++)
            {
                acc += hColumns[i].Width;
                if (Math.Abs(pt.X - acc) <= 4 && i < hColumns.Count - 1) return i;
            }
            return -1;
        }

        private bool ToggleNode(HTreeGridNode node)
        {
            bool expand = !node.hExpanded;
            node.hExpanded = expand;
            ClampScroll();
            RebuildRows();
            Invalidate();
            if (expand) AfterExpand?.Invoke(this, new HTreeGridNodeEventArgs(node));
            else AfterCollapse?.Invoke(this, new HTreeGridNodeEventArgs(node));
            return expand;
        }

        private void SelectRow(HTreeGridNode node)
        {
            if (hSelected == node) return;
            hSelected = node;
            Invalidate();
            AfterSelect?.Invoke(this, new HTreeGridNodeEventArgs(node));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button != MouseButtons.Left) return;

            if (ShowVBar() && VBarRect().Contains(e.Location))
            {
                Rectangle thumb = VThumbRect();
                if (thumb.Contains(e.Location)) { hThumbV = true; hThumbDragOffset = e.Y - thumb.Top; }
                else
                {
                    hScrollY += e.Y < thumb.Top ? -BodyHeight() + 20 : BodyHeight() - 20;
                    ClampScroll(); RebuildRows(); Invalidate();
                }
                return;
            }
            if (ShowHBar() && HBarRect().Contains(e.Location))
            {
                Rectangle thumb = HThumbRect();
                if (thumb.Contains(e.Location)) { hThumbH = true; hThumbDragOffset = e.X - thumb.Left; }
                else
                {
                    hScrollX += e.X < thumb.Left ? -ViewWidth() + 20 : ViewWidth() - 20;
                    ClampScroll(); RebuildRows(); Invalidate();
                }
                return;
            }

            int div = DividerAt(e.Location);
            if (div >= 0)
            {
                hPressedColumn = div;
                hDragColumn = div;
                hDragStartX = e.X;
                hDragStartWidth = hColumns[div].Width;
                Capture = true;
                return;
            }

            HRow row = HitRow(e.Location);
            if (row != null) Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            if (hThumbV || hThumbH)
            {
                hThumbV = false;
                hThumbH = false;
                Invalidate();
                return;
            }

            if (hDragColumn >= 0)
            {
                hPressedColumn = -1;
                hDragColumn = -1;
                Capture = false;
                ClampScroll();
                RebuildRows();
                Invalidate();
                ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            HRow row = HitRow(e.Location);
            if (row != null)
            {
                HTreeGridNode node = row.Node;
                bool onBox = node.hChildren.Count > 0 && ToggleBoxRect(row).Contains(e.Location);
                int col = ColumnAt(e.X);
                RowClick?.Invoke(this, new HTreeGridNodeEventArgs(node));
                if (col >= 0) CellClick?.Invoke(this, new HTreeGridCellEventArgs(node, col));
                SelectRow(node);
                if (onBox || node.hChildren.Count > 0) ToggleNode(node);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (hThumbV)
            {
                Rectangle bar = VBarRect();
                Rectangle thumb = VThumbRect();
                int trackH = bar.Height - thumb.Height;
                int max = Math.Max(0, ContentHeight() - BodyHeight());
                if (trackH > 0 && max > 0)
                {
                    hScrollY = (int)Math.Round((e.Y - bar.Top - hThumbDragOffset) * (double)max / trackH);
                    ClampScroll(); RebuildRows(); Invalidate();
                }
                return;
            }
            if (hThumbH)
            {
                Rectangle bar = HBarRect();
                Rectangle thumb = HThumbRect();
                int trackW = bar.Width - thumb.Width;
                int max = Math.Max(0, ContentWidth() - ViewWidth());
                if (trackW > 0 && max > 0)
                {
                    hScrollX = (int)Math.Round((e.X - bar.Left - hThumbDragOffset) * (double)max / trackW);
                    ClampScroll(); RebuildRows(); Invalidate();
                }
                return;
            }

            if (hDragColumn >= 0)
            {
                int nw = Math.Max(40, hDragStartWidth + (e.X - hDragStartX));
                if (nw != hColumns[hDragColumn].Width)
                {
                    hColumns[hDragColumn].Width = nw;
                    ClampScroll();
                    RebuildRows();
                    Invalidate();
                }
                return;
            }

            if (e.Y < hHeaderHeight)
            {
                Cursor = DividerAt(e.Location) >= 0 ? Cursors.VSplit : Cursors.Default;
                if (hHoverRow != null) { hHoverRow = null; Invalidate(); }
                return;
            }
            HRow row = HitRow(e.Location);
            if (row != hHoverRow)
            {
                hHoverRow = row;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hHoverRow != null) { hHoverRow = null; Invalidate(); }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int lines = SystemInformation.MouseWheelScrollLines;
            if (lines <= 0) lines = 3;
            int step = hDataRowHeight * lines;
            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                hScrollX -= (e.Delta / 120) * step;
            else
                hScrollY -= (e.Delta / 120) * step;
            ClampScroll();
            RebuildRows();
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys k = keyData & Keys.KeyCode;
            if (k == Keys.Up || k == Keys.Down || k == Keys.Left || k == Keys.Right
                || k == Keys.Home || k == Keys.End || k == Keys.PageUp || k == Keys.PageDown)
                return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (hRows.Count == 0) return;
            int idx = hSelected == null ? -1 : IndexOfRow(hSelected);
            switch (e.KeyCode)
            {
                case Keys.Up:
                    idx = idx <= 0 ? hRows.Count - 1 : idx - 1;
                    SelectedNode = hRows[idx].Node;
                    e.Handled = true;
                    break;
                case Keys.Down:
                    idx = idx < 0 ? 0 : (idx + 1) % hRows.Count;
                    SelectedNode = hRows[idx].Node;
                    e.Handled = true;
                    break;
                case Keys.Right:
                    if (idx >= 0 && hRows[idx].Node.HasChildren && !hRows[idx].Node.hExpanded)
                        ToggleNode(hRows[idx].Node);
                    e.Handled = true;
                    break;
                case Keys.Left:
                    if (idx >= 0 && hRows[idx].Node.HasChildren && hRows[idx].Node.hExpanded)
                        ToggleNode(hRows[idx].Node);
                    else if (idx >= 0 && hRows[idx].Node.hParent != null)
                        SelectedNode = hRows[idx].Node.hParent;
                    e.Handled = true;
                    break;
                case Keys.Home:
                    SelectedNode = hRows[0].Node;
                    e.Handled = true;
                    break;
                case Keys.End:
                    SelectedNode = hRows[hRows.Count - 1].Node;
                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    hScrollY -= BodyHeight();
                    ClampScroll(); RebuildRows(); Invalidate();
                    e.Handled = true;
                    break;
                case Keys.PageDown:
                    hScrollY += BodyHeight();
                    ClampScroll(); RebuildRows(); Invalidate();
                    e.Handled = true;
                    break;
            }
        }

        private int IndexOfRow(HTreeGridNode node)
        {
            for (int i = 0; i < hRows.Count; i++)
                if (hRows[i].Node == node) return i;
            return -1;
        }
        #endregion

        #region 绘制
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampScroll();
            RebuildRows();
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Font old = hBoldFont;
            hBoldFont = new Font(Font, FontStyle.Bold);
            if (old != null) old.Dispose();
            Invalidate();
        }

        /// <summary>整表底色由 OnPaintBackground 不透明擦除，这里完全覆盖基类标签绘制。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (hColumns.Count == 0) return;
            if (hTreeColumnIndex >= hColumns.Count) hTreeColumnIndex = hColumns.Count - 1;
            ClampScroll();
            RebuildRows();

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int viewW = ViewWidth();
            int bodyTop = hHeaderHeight;
            int bodyBottom = ClientSize.Height - (ShowHBar() ? ScrollBarThickness : 0);

            // 表体底（白色，数据行再各自铺金/灰底，下方空白行保留白色与网格线）
            using (var b = new SolidBrush(BackColor))
                g.FillRectangle(b, new Rectangle(0, bodyTop, viewW, bodyBottom - bodyTop));

            // ---- 数据行 ----
            foreach (HRow row in hRows)
            {
                if (row.Y + row.Height < bodyTop || row.Y > bodyBottom) continue;
                DrawRow(g, row, viewW);
            }

            // ---- 空白区域行网格线 ----
            DrawEmptyGrid(g, viewW, bodyTop, bodyBottom);

            // ---- 列竖向分隔线（贯穿表体） ----
            using (var pen = new Pen(hGridLineColor, 1f))
            {
                int x = -hScrollX;
                for (int i = 0; i < hColumns.Count; i++)
                {
                    x += hColumns[i].Width;
                    if (x > 0 && x < viewW)
                        g.DrawLine(pen, x, bodyTop, x, bodyBottom);
                }
            }

            // ---- 表头 ----
            DrawHeader(g, viewW);

            // ---- 滚动条 ----
            if (ShowVBar()) DrawVBar(g);
            if (ShowHBar()) DrawHBar(g);

            // ---- 外边框 ----
            using (var pen = new Pen(hBorderColor, 1f))
                g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        private void DrawHeader(Graphics g, int viewW)
        {
            Rectangle head = new Rectangle(0, 0, viewW, hHeaderHeight);
            using (var b = new SolidBrush(hHeaderBackColor))
                g.FillRectangle(b, head);

            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            for (int i = 0; i < hColumns.Count; i++)
            {
                HTreeGridColumn col = hColumns[i];
                Rectangle cell = new Rectangle(ColumnX(i), 0, col.Width, hHeaderHeight);
                if (cell.Right < 0 || cell.Left > viewW) continue;
                TextFormatFlags f = flags | AlignFlag(col.Align);
                TextRenderer.DrawText(g, col.Text, hBoldFont,
                    new Rectangle(cell.X + 12, cell.Y, cell.Width - 24, cell.Height),
                    hHeaderForeColor, f);
            }

            // 表头分隔线
            using (var pen = new Pen(hGridLineColor, 1f))
            {
                int x = -hScrollX;
                for (int i = 0; i < hColumns.Count; i++)
                {
                    x += hColumns[i].Width;
                    if (x > 0 && x < viewW) g.DrawLine(pen, x, 0, x, hHeaderHeight);
                }
            }
            using (var pen = new Pen(hGridLineColor, 1.4f))
                g.DrawLine(pen, 0, hHeaderHeight - 1, viewW, hHeaderHeight - 1);
        }

        private void DrawRow(Graphics g, HRow row, int viewW)
        {
            HTreeGridNode node = row.Node;
            bool isGroup = node.hChildren.Count > 0;
            Color back = isGroup ? hGroupBackColor : hRowBackColor;
            Color fore = isGroup ? hGroupForeColor : hRowForeColor;

            Rectangle rowRect = new Rectangle(0, row.Y, Math.Max(viewW, ContentWidth()), row.Height);
            using (var b = new SolidBrush(back))
                g.FillRectangle(b, rowRect);

            var oldClip = g.Clip;
            g.SetClip(new Rectangle(0, row.Y, viewW, row.Height), CombineMode.Intersect);

            // 选中覆盖
            if (node == hSelected)
            {
                using (var b = new SolidBrush(hSelectionColor))
                    g.FillRectangle(b, new Rectangle(0, row.Y, viewW, row.Height));
            }

            // 行底线
            using (var pen = new Pen(hGridLineColor, 1f))
                g.DrawLine(pen, 0, row.Y + row.Height - 1, viewW, row.Y + row.Height - 1);

            // 单元格内容
            for (int i = 0; i < hColumns.Count; i++)
            {
                HTreeGridColumn col = hColumns[i];
                Rectangle cell = new Rectangle(ColumnX(i), row.Y, col.Width, row.Height);
                if (cell.Right < 0 || cell.Left > viewW) continue;
                if (i == hTreeColumnIndex)
                    DrawTreeCell(g, row, cell, fore, isGroup);
                else
                    DrawValueCell(g, node, i, cell, col.Align, fore, isGroup);
            }

            g.Clip = oldClip;
        }

        /// <summary>绘制空白区域的横向网格线（与数据行同高的虚拟行）。</summary>
        private void DrawEmptyGrid(Graphics g, int viewW, int bodyTop, int bodyBottom)
        {
            int bottom = hRows.Count > 0 ? hRows[hRows.Count - 1].Y + hRows[hRows.Count - 1].Height : bodyTop;
            if (bottom < bodyTop) bottom = bodyTop;
            using (var pen = new Pen(hGridLineColor, 1f))
            {
                for (int y = bottom; y < bodyBottom; y += hDataRowHeight)
                    g.DrawLine(pen, 0, y, viewW, y);
            }
        }

        private void DrawValueCell(Graphics g, HTreeGridNode node, int colIndex,
            Rectangle cell, HTreeGridAlign align, Color fore, bool isGroup)
        {
            HTreeGridMark mark = node.GetMark(colIndex);
            object value = node.GetValue(colIndex);
            if (mark == HTreeGridMark.None && value is bool)
                mark = (bool)value ? HTreeGridMark.Check : HTreeGridMark.None;

            if (mark != HTreeGridMark.None)
            {
                DrawMark(g, mark, cell);
                return;
            }
            string text = value == null ? "" : Convert.ToString(value);
            if (text.Length == 0) return;
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | AlignFlag(align);
            Font font = isGroup ? hBoldFont : Font;
            TextRenderer.DrawText(g, text, font,
                new Rectangle(cell.X + 12, cell.Y, cell.Width - 24, cell.Height),
                fore, flags);
        }

        private void DrawTreeCell(Graphics g, HRow row, Rectangle cell, Color fore, bool isGroup)
        {
            HTreeGridNode node = row.Node;
            int level = row.Level;

            // 树连接线
            if (level > 0) DrawTreeGuides(g, row, cell);

            // 折叠框 / 图标 / 文字
            int textX;
            if (isGroup)
            {
                Rectangle box = ToggleBoxRect(row);
                DrawToggleBox(g, box, node.hExpanded);
                textX = box.Right + 8;
            }
            else
            {
                int glyphX = cell.X + 8 + level * hIndent + 4;
                if (node.hGlyph != HTreeGridNodeGlyph.None)
                {
                    DrawStamp(g, new Rectangle(glyphX, row.Y + (row.Height - 20) / 2, 20, 20), fore);
                    textX = glyphX + 20 + 8;
                }
                else
                {
                    textX = glyphX + 10;
                }
            }

            string text = Convert.ToString(node.GetValue(hTreeColumnIndex) ?? "");
            if (text.Length > 0)
            {
                var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                    | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
                TextRenderer.DrawText(g, text, isGroup ? hBoldFont : Font,
                    new Rectangle(textX, row.Y, cell.Right - textX - 8, row.Height), fore, flags);
            }
        }

        /// <summary>竖向层级线 + 末行肘形线（├ / └）。</summary>
        private void DrawTreeGuides(Graphics g, HRow row, Rectangle treeCell)
        {
            int level = row.Level;
            using (var pen = new Pen(hTreeLineColor, 1.2f))
            {
                // 祖先列竖线
                for (int d = 0; d < level - 1; d++)
                {
                    if (!row.Continuing[d]) continue;
                    int cx = treeCell.X + 8 + d * hIndent + 7;
                    g.DrawLine(pen, cx, row.Y, cx, row.Y + row.Height);
                }
                // 当前父级：├ 全竖线 + 横线；└ 半竖线 + 横线
                int pd = level - 1;
                int pcx = treeCell.X + 8 + pd * hIndent + 7;
                int cy = row.Y + row.Height / 2;
                int endY = row.Continuing[pd] ? row.Y + row.Height : cy;
                g.DrawLine(pen, pcx, row.Y, pcx, endY);
                int glyphX = treeCell.X + 8 + level * hIndent + 4;
                g.DrawLine(pen, pcx, cy, glyphX - 3, cy);
            }
        }

        private void DrawToggleBox(Graphics g, Rectangle box, bool expanded)
        {
            using (GraphicsPath path = HGlyph.RoundedRect(box, 2))
            using (var b = new SolidBrush(hBoxBackColor))
                g.FillPath(b, path);
            using (var pen = new Pen(hBoxBorderColor, 1f))
                g.DrawRectangle(pen, box.X, box.Y, box.Width - 1, box.Height - 1);
            int cx = box.X + box.Width / 2;
            int cy = box.Y + box.Height / 2;
            int half = 3;
            using (var pen = new Pen(hGroupForeColor, 1.5f))
            {
                g.DrawLine(pen, cx - half, cy, cx + half, cy);          // 横（-）
                if (!expanded) g.DrawLine(pen, cx, cy - half, cx, cy + half); // 竖（+）
            }
        }

        /// <summary>对勾 / 叉号。</summary>
        private void DrawMark(Graphics g, HTreeGridMark mark, Rectangle cell)
        {
            int s = 12;
            int cx = cell.X + cell.Width / 2;
            int cy = cell.Y + cell.Height / 2;
            using (var pen = new Pen(hMarkColor, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (mark == HTreeGridMark.Check)
                {
                    Point[] pts =
                    {
                        new Point(cx - s / 2, cy),
                        new Point(cx - 2, cy + s / 2 - 1),
                        new Point(cx + s / 2, cy - s / 2 + 1)
                    };
                    g.DrawLines(pen, pts);
                }
                else
                {
                    g.DrawLine(pen, cx - s / 2, cy - s / 2, cx + s / 2, cy + s / 2);
                    g.DrawLine(pen, cx + s / 2, cy - s / 2, cx - s / 2, cy + s / 2);
                }
            }
        }

        /// <summary>吸嘴/冲压印章形小图标（深灰填充）。</summary>
        private void DrawStamp(Graphics g, Rectangle r, Color color)
        {
            int x = r.X, y = r.Y, w = r.Width, h = r.Height;
            using (GraphicsPath p = new GraphicsPath())
            using (var b = new SolidBrush(color))
            {
                // 顶部按柄
                int handleW = w * 7 / 12;
                p.AddArc(x + (w - handleW) / 2, y, handleW, 5, 180, 180);
                p.AddRectangle(new Rectangle(x + (w - handleW) / 2, y + 2, handleW, 3));
                // 中部收缩颈
                int neckW = w * 5 / 12;
                p.AddRectangle(new Rectangle(x + (w - neckW) / 2, y + 5, neckW, 3));
                // 下部梯形主体到底座
                Point[] trap =
                {
                    new Point(x + (w - neckW) / 2, y + 8),
                    new Point(x + (w + neckW) / 2, y + 8),
                    new Point(x + w - 2, y + h - 5),
                    new Point(x + 2, y + h - 5)
                };
                p.AddPolygon(trap);
                g.FillPath(b, p);
                // 底部橡胶条
                g.FillRectangle(b, new Rectangle(x + 1, y + h - 5, w - 2, 4));
            }
        }

        private void DrawVBar(Graphics g)
        {
            Rectangle bar = VBarRect();
            Rectangle thumb = VThumbRect();
            using (GraphicsPath path = HGlyph.RoundedRect(thumb, thumb.Width / 2))
            using (var b = new SolidBrush(hScrollThumbColor))
                g.FillPath(b, path);
        }

        private void DrawHBar(Graphics g)
        {
            Rectangle bar = HBarRect();
            Rectangle thumb = HThumbRect();
            using (GraphicsPath path = HGlyph.RoundedRect(thumb, thumb.Height / 2))
            using (var b = new SolidBrush(hScrollThumbColor))
                g.FillPath(b, path);
        }

        private static TextFormatFlags AlignFlag(HTreeGridAlign align)
        {
            if (align == HTreeGridAlign.Left) return TextFormatFlags.Left;
            if (align == HTreeGridAlign.Right) return TextFormatFlags.Right;
            return TextFormatFlags.HorizontalCenter;
        }
        #endregion

        /// <summary>释放加粗字体。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && hBoldFont != null) hBoldFont.Dispose();
            base.Dispose(disposing);
        }
    }
}
