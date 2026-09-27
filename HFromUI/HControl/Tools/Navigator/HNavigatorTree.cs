using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HControl.Base;
namespace HFromUI.HControl.Tools.Navigator
{
    using HFromUI.HControl.Tools.UiKit;
    /// <summary>
    /// 经典风格树节点：图标 + 文字，可无限级嵌套。
    /// 用法：root.Nodes.Add("归档", HGlyphKind.Database);
    /// </summary>
    [DefaultProperty("Text")]
    public class HNavigatorNode
    {
        internal string hText;
        internal HGlyphKind hIcon;
        private object hTag;
        internal bool hExpanded;
        internal HNavigatorNode hParent;
        /// <summary>hChildren 成员。</summary>
        internal readonly List<HNavigatorNode> hChildren = new List<HNavigatorNode>();
        public HNavigatorNode() : this("") { }
        public HNavigatorNode(string text) : this(text, HGlyphKind.Layers) { }
        public HNavigatorNode(string text, HGlyphKind icon)
        {
            hText = text ?? "";
            hIcon = icon;
        }
        /// <summary>节点文字</summary>
        /// <summary>文本。</summary>
        /// <summary>文本。</summary>
        [Category("HFromUI"), Description("节点文字")]
        public string Text { get { return hText; } set { hText = value ?? ""; } }
        /// <summary>矢量图标</summary>
        /// <summary>Icon 成员。</summary>
        /// <summary>Icon 字段。</summary>
        [Category("HFromUI"), Description("矢量图标")]
        public HGlyphKind Icon { get { return hIcon; } set { hIcon = value; } }
        /// <summary>自定义数据</summary>
        /// <summary>Tag 成员。</summary>
        /// <summary>Tag 字段。</summary>
        [Category("HFromUI"), Description("自定义数据")]
        public object Tag { get { return hTag; } set { hTag = value; } }
        /// <summary>是否已展开</summary>
        /// <summary>Expanded 成员。</summary>
        /// <summary>Expanded 字段。</summary>
        [Category("HFromUI"), Description("是否已展开"), DefaultValue(false)]
        public bool Expanded { get { return hExpanded; } set { hExpanded = value; } }
        /// <summary>父节点（根节点为 null）</summary>
        /// <summary>Parent 成员。</summary>
        /// <summary>Parent 字段。</summary>
        [Browsable(false)]
        public HNavigatorNode Parent { get { return hParent; } }
        /// <summary>子节点集合</summary>
        /// <summary>Nodes 成员。</summary>
        /// <summary>Nodes 字段。</summary>
        [Browsable(false)]
        public List<HNavigatorNode> Nodes { get { return hChildren; } }
        /// <summary>是否有子节点</summary>
        /// <summary>HasChildren 成员。</summary>
        /// <summary>HasChildren 字段。</summary>
        [Browsable(false)]
        public bool HasChildren { get { return hChildren.Count > 0; } }
        /// <summary>层级（根为 0）</summary>
        [Browsable(false)]
        public int Level
        {
            get
            {
                int lv = 0;
                HNavigatorNode p = hParent;
                while (p != null) { lv++; p = p.hParent; }
                return lv;
            }
        }
        /// <summary>添加子节点</summary>
        public HNavigatorNode Add(HNavigatorNode node)
        {
            if (node == null) return null;
            if (node.hParent != null) node.hParent.hChildren.Remove(node);
            node.hParent = this;
            hChildren.Add(node);
            return node;
        }
        /// <summary>快捷创建并添加子节点，返回新节点</summary>
        public HNavigatorNode Add(string text, HGlyphKind icon)
        {
            return Add(new HNavigatorNode(text, icon));
        }
        /// <summary>移除子节点</summary>
        public void Remove(HNavigatorNode node)
        {
            if (node == null) return;
            if (hChildren.Remove(node)) node.hParent = null;
        }
        /// <summary>清空子节点</summary>
        public void Clear()
        {
            foreach (HNavigatorNode n in hChildren) n.hParent = null;
            hChildren.Clear();
        }
    }
    /// <summary>
    /// 经典工控风格树控件（参考 WinCC flexible 导航树）：
    /// 点状连接线 + [+]/[-]方框 + 矢量图标 + 蓝色渐变选中高亮。
    /// 纯 GDI+ 自绘，支持键盘导航、滚轮、自绘滚动条。
    /// </summary>
    [DefaultEvent("AfterSelect")]
    [ToolboxItem(true)]
    public class HNavigatorTree : HLabelBase
    {
        #region 内部结构
        private class HRowInfo
        {
            public HNavigatorNode Node;
            public int Y;      // 行顶
            public int Level;
        }
        #endregion
        #region 字段
        /// <summary>hNodes 字段。</summary>
        private List<HNavigatorNode> hNodes = new List<HNavigatorNode>();
        /// <summary>hRows 字段。</summary>
        private readonly List<HRowInfo> hRows = new List<HRowInfo>();
        private HNavigatorNode hSelected;
        private HNavigatorNode hHover;
        private int hScrollPx;          // 滚动像素偏移
        /// <summary>hItemHeight 字段。</summary>
        private int hItemHeight = 24;
        /// <summary>hIndent 字段。</summary>
        private int hIndent = 19;
        private bool hMouseDownOnThumb;
        private int hThumbDragOffset;
        private bool hShowScroll;
        // 外观
        private Color hLineColor = Color.FromArgb(96, 102, 114);
        /// <summary>hTextColor 字段。</summary>
        private Color hTextColor = Color.FromArgb(28, 28, 28);
        /// <summary>hIconColor 字段。</summary>
        private Color hIconColor = Color.FromArgb(60, 105, 170);
        /// <summary>hSelColor 字段。</summary>
        private Color hSelColor = Color.FromArgb(21, 74, 214);        // WinCC 经典选中深蓝
        /// <summary>hSelTextColor 字段。</summary>
        private Color hSelTextColor = Color.White;
        /// <summary>hHoverColor 字段。</summary>
        private Color hHoverColor = Color.FromArgb(232, 240, 252);
        /// <summary>hPlusBoxBorder 字段。</summary>
        private Color hPlusBoxBorder = Color.FromArgb(118, 128, 142);
        /// <summary>hScrollTrack 字段。</summary>
        private Color hScrollTrack = Color.FromArgb(250, 251, 252);
        /// <summary>hScrollThumb 字段。</summary>
        private Color hScrollThumb = Color.FromArgb(196, 210, 224);
        /// <summary>hScrollThumbBorder 字段。</summary>
        private Color hScrollThumbBorder = Color.FromArgb(159, 180, 200);
        #endregion
        #region 事件
        /// <summary>选中节点变化后触发</summary>
        public event EventHandler<HNavigatorNodeEventArgs> AfterSelect;
        /// <summary>节点展开后触发</summary>
        public event EventHandler<HNavigatorNodeEventArgs> AfterExpand;
        /// <summary>节点折叠后触发</summary>
        public event EventHandler<HNavigatorNodeEventArgs> AfterCollapse;
        /// <summary>节点被点击（左右键均触发）</summary>
        public event EventHandler<HNavigatorNodeEventArgs> NodeClick;
        #endregion
        public HNavigatorTree()
        {
            // 基类 HLabelBase 默认为 AutoSize 标签，树为定高列表，先关闭自动尺寸
            AutoSize = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 9F);
            Width = 200;
            Height = 160;
            TabStop = true;
        }
        #region 属性
        /// <summary>根节点集合</summary>
        /// <summary>Nodes 成员。</summary>
        /// <summary>Nodes 字段。</summary>
        [Browsable(false)]
        public List<HNavigatorNode> Nodes { get { return hNodes; } }
        /// <summary>当前选中节点（null 表示无选中）</summary>
        [Browsable(false)]
        public HNavigatorNode SelectedNode
        {
            get { return hSelected; }
            set
            {
                if (hSelected == value) return;
                hSelected = value;
                Invalidate();
                AfterSelect?.Invoke(this, new HNavigatorNodeEventArgs(value));
            }
        }
        /// <summary>行高</summary>
        [Category("HFromUI"), Description("行高"), DefaultValue(24)]
        public int ItemHeight
        {
            get { return hItemHeight; }
            set { hItemHeight = Math.Max(16, value); Invalidate(); }
        }
        /// <summary>每级缩进</summary>
        [Category("HFromUI"), Description("每级缩进"), DefaultValue(19)]
        public int Indent
        {
            get { return hIndent; }
            set { hIndent = Math.Max(12, value); Invalidate(); }
        }
        /// <summary>连接线颜色</summary>
        [Category("HFromUI"), Description("连接线颜色")]
        public Color LineColor
        {
            get { return hLineColor; }
            set { hLineColor = value; Invalidate(); }
        }
        /// <summary>图标颜色</summary>
        [Category("HFromUI"), Description("图标颜色")]
        public Color IconColor
        {
            get { return hIconColor; }
            set { hIconColor = value; Invalidate(); }
        }
        /// <summary>文字颜色</summary>
        [Category("HFromUI"), Description("文字颜色")]
        public Color TextColor
        {
            get { return hTextColor; }
            set { hTextColor = value; Invalidate(); }
        }
        /// <summary>选中底色（经典 WinCC 深蓝实底）</summary>
        [Category("HFromUI"), Description("选中底色")]
        public Color SelectionColor
        {
            get { return hSelColor; }
            set { hSelColor = value; Invalidate(); }
        }
        /// <summary>选中文字颜色</summary>
        [Category("HFromUI"), Description("选中文字颜色")]
        public Color SelectionTextColor
        {
            get { return hSelTextColor; }
            set { hSelTextColor = value; Invalidate(); }
        }
        /// <summary>悬停底色</summary>
        [Category("HFromUI"), Description("悬停底色")]
        public Color HoverColor
        {
            get { return hHoverColor; }
            set { hHoverColor = value; Invalidate(); }
        }
        #endregion
        #region 公开方法
        /// <summary>添加根节点</summary>
        public HNavigatorNode AddNode(HNavigatorNode node)
        {
            if (node == null) return null;
            if (node.hParent != null) node.hParent.hChildren.Remove(node);
            node.hParent = null;
            hNodes.Add(node);
            Invalidate();
            return node;
        }
        /// <summary>快捷创建并添加根节点</summary>
        public HNavigatorNode AddNode(string text, HGlyphKind icon)
        {
            return AddNode(new HNavigatorNode(text, icon));
        }
        /// <summary>
        /// 切换为另一组根节点（HNavigator 上下栏目整栏互换时使用）：
        /// 树视图本身不持有数据，只是节点列表的渲染器，切换引用后重置滚动/悬停/选中。
        /// </summary>
        internal void AttachNodes(List<HNavigatorNode> nodes, HNavigatorNode selected = null)
        {
            hNodes = nodes ?? new List<HNavigatorNode>();
            hSelected = selected;
            hHover = null;
            hScrollPx = 0;
            Invalidate();
        }
        /// <summary>移除节点（含其子树）</summary>
        public void RemoveNode(HNavigatorNode node)
        {
            if (node == null) return;
            if (node.hParent != null) { node.hParent.Remove(node); }
            else { hNodes.Remove(node); node.hParent = null; }
            if (hSelected == node) hSelected = null;
            Invalidate();
        }
        /// <summary>展开全部</summary>
        public void ExpandAll()
        {
            foreach (HNavigatorNode n in hNodes) SetExpandedDeep(n, true);
            Invalidate();
        }
        /// <summary>折叠全部</summary>
        public void CollapseAll()
        {
            foreach (HNavigatorNode n in hNodes) SetExpandedDeep(n, false);
            Invalidate();
        }
        /// <summary>展开从根到该节点的路径并保证可见</summary>
        public void EnsureVisible(HNavigatorNode node)
        {
            if (node == null) return;
            HNavigatorNode p = node.Parent;
            while (p != null)
            {
                p.hExpanded = true;
                p = p.Parent;
            }
            Invalidate();
            RebuildRows();
            int idx = RowsIndexOf(node);
            if (idx >= 0)
            {
                int top = idx * hItemHeight;
                int viewH = ContentViewHeight;
                if (top < hScrollPx) hScrollPx = top;
                else if (top + hItemHeight > hScrollPx + viewH) hScrollPx = top + hItemHeight - viewH;
                ClampScroll();
                Invalidate();
            }
        }
        /// <summary>设置 expandedDeep。</summary>
        private static void SetExpandedDeep(HNavigatorNode n, bool expanded)
        {
            n.hExpanded = expanded;
            foreach (HNavigatorNode c in n.hChildren) SetExpandedDeep(c, expanded);
        }
        #endregion
        #region 布局
        private int ContentViewWidth
        {
            get { return hShowScroll ? Width - 13 : Width; }
        }
        private int ContentViewHeight
        {
            get { return Height; }
        }
        /// <summary>PlusBoxX 方法。</summary>
        private int PlusBoxX(int level)
        {
            return 6 + level * hIndent;
        }
        /// <summary>IconX 方法。</summary>
        private int IconX(int level)
        {
            return PlusBoxX(level) + 20;
        }
        /// <summary>RebuildRows 方法。</summary>
        private void RebuildRows()
        {
            hRows.Clear();
            CollectRows(hNodes, 0);
            int maxScroll = Math.Max(0, hRows.Count * hItemHeight - ContentViewHeight);
            hShowScroll = hRows.Count * hItemHeight > ContentViewHeight;
            if (hScrollPx > maxScroll) hScrollPx = maxScroll;
            if (hScrollPx < 0) hScrollPx = 0;
        }
        /// <summary>CollectRows 方法。</summary>
        private void CollectRows(List<HNavigatorNode> list, int level)
        {
            foreach (HNavigatorNode n in list)
            {
                hRows.Add(new HRowInfo { Node = n, Y = hRows.Count * hItemHeight, Level = level });
                if (n.hExpanded && n.hChildren.Count > 0)
                    CollectRows(n.hChildren, level + 1);
            }
        }
        /// <summary>RowsIndexOf 方法。</summary>
        private int RowsIndexOf(HNavigatorNode node)
        {
            for (int i = 0; i < hRows.Count; i++)
                if (hRows[i].Node == node) return i;
            return -1;
        }
        /// <summary>ClampScroll 方法。</summary>
        private void ClampScroll()
        {
            int maxScroll = Math.Max(0, hRows.Count * hItemHeight - ContentViewHeight);
            if (hScrollPx > maxScroll) hScrollPx = maxScroll;
            if (hScrollPx < 0) hScrollPx = 0;
        }
        #endregion
        #region 绘制
        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush b = new SolidBrush(BackColor))
                g.FillRectangle(b, ClientRectangle);
            RebuildRows();
            if (hRows.Count == 0) return;
            Rectangle viewRect = new Rectangle(0, 0, ContentViewWidth, Height);
            g.SetClip(viewRect);
            // ---- 第一遍：点状连接线 ----
            DrawConnectorLines(g);
            // ---- 第二遍：行内容 ----
            int first = Math.Max(0, hScrollPx / hItemHeight - 1);
            int last = Math.Min(hRows.Count - 1, (hScrollPx + Height) / hItemHeight + 1);
            for (int i = first; i <= last; i++)
            {
                HRowInfo row = hRows[i];
                DrawRow(g, row, i);
            }
            g.ResetClip();
            // ---- 滚动条 ----
            if (hShowScroll) DrawScrollbar(g);
        }
        /// <summary>点状连接线：每个有可见子节点的父节点画一条纵脊，子节点画横向肘线</summary>
        private void DrawConnectorLines(Graphics g)
        {
            // 方点虚线（WinCC 经典点状连接）：1.4px 实点 + 3.2px 间隔
            using (Pen pen = new Pen(hLineColor, 1.4f)
            {
                DashStyle = DashStyle.Custom,
                DashPattern = new float[] { 1f, 2.6f },
                DashCap = DashCap.Flat
            })
            {
                for (int i = 0; i < hRows.Count; i++)
                {
                    HRowInfo row = hRows[i];
                    HNavigatorNode node = row.Node;
                    int rowBottom = row.Y + hItemHeight;
                    int cx = PlusBoxX(row.Level) + 5;
                    // 纵脊：从本行底部到最后一个可见直接子行的中心线
                    if (node.hExpanded && node.HasChildren)
                    {
                        int lastChildIdx = LastVisibleChildRowIndex(i, node);
                        if (lastChildIdx > i)
                        {
                            int startY = rowBottom - 1 - hScrollPx;
                            int endY = hRows[lastChildIdx].Y + hItemHeight / 2 - hScrollPx;
                            g.DrawLine(pen, cx, startY, cx, endY);
                        }
                    }
                    // 横向肘线
                    int midY2 = row.Y + hItemHeight / 2 - hScrollPx;
                    if (node.Parent != null)
                    {
                        // 非根节点：从父脊到本节点的加减框（或叶子的图标）
                        int parentCx = PlusBoxX(row.Level - 1) + 5;
                        int endX = node.HasChildren ? cx : IconX(row.Level) - 3;
                        g.DrawLine(pen, parentCx, midY2, endX, midY2);
                    }
                    else if (node.HasChildren)
                    {
                        // 根节点：从加减框右边缘到图标
                        int boxRight = PlusBoxX(row.Level) + 12;
                        g.DrawLine(pen, boxRight, midY2, IconX(row.Level) - 3, midY2);
                    }
                }
            }
        }
        /// <summary>第 rowIndex 行节点的最后一个可见直接子节点所在行</summary>
        private int LastVisibleChildRowIndex(int rowIndex, HNavigatorNode node)
        {
            if (node.hChildren.Count == 0) return rowIndex;
            HNavigatorNode lastChild = node.hChildren[node.hChildren.Count - 1];
            // 最后子节点的可见行 = 从 rowIndex+1 开始向下找（子树按序展开，最后一个直接子节点必然是最后一个可见后代）
            int last = rowIndex;
            for (int i = rowIndex + 1; i < hRows.Count; i++)
            {
                HNavigatorNode n = hRows[i].Node;
                if (IsDescendantOf(n, node)) last = i; else break;
            }
            return last;
        }
        /// <summary>判断是否 DescendantOf。</summary>
        private static bool IsDescendantOf(HNavigatorNode n, HNavigatorNode ancestor)
        {
            HNavigatorNode p = n.Parent;
            while (p != null)
            {
                if (p == ancestor) return true;
                p = p.Parent;
            }
            return false;
        }
        /// <summary>DrawRow 方法。</summary>
        private void DrawRow(Graphics g, HRowInfo row, int rowIndex)
        {
            HNavigatorNode node = row.Node;
            int y = row.Y - hScrollPx;
            if (y + hItemHeight < 0 || y > Height) return;
            int midY = y + hItemHeight / 2;
            bool isSelected = (node == hSelected);
            bool isHover = (node == hHover && !isSelected);
            int px = PlusBoxX(row.Level);
            int ix = IconX(row.Level);
            // 测量文字宽
            SizeF tsz = TextRenderer.MeasureText(g, node.hText, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
            float textW = tsz.Width;
            int textX = ix + 17 + 6;
            // 悬停底色（整行淡蓝，不包图标外区域太多）
            if (isHover)
            {
                Rectangle hvRect = new Rectangle(px - 2, y + 2, textX - px + (int)textW + 8, hItemHeight - 4);
                using (SolidBrush hb = new SolidBrush(hHoverColor))
                    g.FillRectangle(hb, hvRect);
            }
            // +/- 方框
            if (node.HasChildren)
            {
                Rectangle box = new Rectangle(px, midY - 5, 11, 11);
                using (SolidBrush bb = new SolidBrush(Color.White))
                    g.FillRectangle(bb, box);
                using (Pen bp = new Pen(hPlusBoxBorder, 1f))
                    g.DrawRectangle(bp, box.X, box.Y, box.Width - 1, box.Height - 1);
                using (Pen mp = new Pen(Color.FromArgb(60, 70, 84), 1.2f))
                {
                    g.DrawLine(mp, box.X + 2, midY, box.X + 8, midY);               // 横线（-）
                    if (!node.hExpanded)
                        g.DrawLine(mp, box.X + 5, box.Y + 2, box.X + 5, box.Y + 8); // 竖线（+）
                }
            }
            // 图标（彩色 3D 风格，未实现的种类退化为蓝色线性）
            Rectangle iconRect = new Rectangle(ix - 1, midY - 9, 18, 18);
            HGlyph.DrawColor(g, node.hIcon, iconRect);
            // 选中高亮：深蓝实底只包文字 + 白色点状虚线框，文字白色加粗（WinCC 经典选中）
            Color textColor = hTextColor;
            Font textFont = Font;
            if (isSelected)
            {
                Rectangle selRect = new Rectangle(textX - 4, y + 2, (int)textW + 9, hItemHeight - 4);
                using (SolidBrush sb = new SolidBrush(hSelColor))
                    g.FillRectangle(sb, selRect);
                using (Pen dp = new Pen(Color.FromArgb(220, 230, 255), 1f)
                {
                    DashStyle = DashStyle.Custom,
                    DashPattern = new float[] { 1.2f, 1.8f },
                    DashCap = DashCap.Flat
                })
                {
                    g.DrawRectangle(dp, selRect.X, selRect.Y, selRect.Width - 1, selRect.Height - 1);
                }
                textColor = hSelTextColor;
                textFont = new Font(Font, FontStyle.Bold);
            }
            // 文字
            Rectangle textRect = new Rectangle(textX, y, ContentViewWidth - textX - 2, hItemHeight);
            TextRenderer.DrawText(g, node.hText, textFont, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
            if (isSelected && textFont != Font) textFont.Dispose();
        }
        /// <summary>DrawScrollbar 方法。</summary>
        private void DrawScrollbar(Graphics g)
        {
            int trackW = 13;
            Rectangle track = new Rectangle(Width - trackW, 0, trackW, Height);
            using (SolidBrush tb = new SolidBrush(hScrollTrack))
                g.FillRectangle(tb, track);
            using (Pen lp = new Pen(Color.FromArgb(228, 232, 236), 1f))
                g.DrawLine(lp, track.X, 0, track.X, Height);
            Rectangle thumb = ThumbRect(track);
            using (GraphicsPath path = HGlyph.RoundedRect(thumb, 4))
            {
                using (SolidBrush sb = new SolidBrush(hScrollThumb))
                    g.FillPath(sb, path);
                using (Pen sp = new Pen(hScrollThumbBorder, 1f))
                    g.DrawPath(sp, path);
            }
        }
        /// <summary>ThumbRect 方法。</summary>
        private Rectangle ThumbRect(Rectangle track)
        {
            int totalH = hRows.Count * hItemHeight;
            if (totalH <= 0) return new Rectangle(track.X + 2, 2, track.Width - 4, 30);
            int viewH = ContentViewHeight;
            int th = Math.Max(18, (int)((float)viewH / totalH * track.Height) - 4);
            int range = track.Height - th - 4;
            int ty = 2;
            if (totalH > viewH && range > 0)
                ty = 2 + (int)((float)hScrollPx / (totalH - viewH) * range);
            return new Rectangle(track.X + 2, ty, track.Width - 4, th);
        }
        #endregion
        #region 交互
        /// <summary>HitRow 方法。</summary>
        private HRowInfo HitRow(Point p)
        {
            if (p.X < 0 || p.X >= ContentViewWidth) return null;
            int viewY = p.Y + hScrollPx;
            int idx = viewY / hItemHeight;
            if (idx < 0 || idx >= hRows.Count) return null;
            return hRows[idx];
        }
        /// <summary>PlusRect 方法。</summary>
        private static Rectangle PlusRect(HRowInfo row, int itemHeight)
        {
            int px = 4 + row.Level * 19;
            return new Rectangle(px, row.Y + itemHeight / 2 - 5, 11, 11);
        }
        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            // 滚动条交互
            if (hShowScroll && e.X >= Width - 13)
            {
                Rectangle track = new Rectangle(Width - 13, 0, 13, Height);
                Rectangle thumb = ThumbRect(track);
                if (thumb.Contains(e.Location))
                {
                    hMouseDownOnThumb = true;
                    hThumbDragOffset = e.Y - thumb.Y;
                    Capture = true;
                }
                else if (e.Y < thumb.Y)
                {
                    hScrollPx -= ContentViewHeight; // 上翻页
                    ClampScroll(); Invalidate();
                }
                else
                {
                    hScrollPx += ContentViewHeight; // 下翻页
                    ClampScroll(); Invalidate();
                }
                return;
            }
            RebuildRows();
            HRowInfo row = HitRow(e.Location);
            if (row == null) return;
            // 点击 +/- 方框 → 展开/折叠
            int px = PlusBoxX(row.Level);
            Rectangle pr = new Rectangle(px, row.Y + hItemHeight / 2 - 5 - hScrollPx + hScrollPx, 11, 11);
            pr.Y = row.Y - hScrollPx + hItemHeight / 2 - 5;
            if (row.Node.HasChildren && pr.Contains(e.Location))
            {
                ToggleExpand(row.Node);
                return;
            }
            // 点击行 → 选中
            SelectedNode = row.Node;
            NodeClick?.Invoke(this, new HNavigatorNodeEventArgs(row.Node));
        }
        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            hMouseDownOnThumb = false;
            Capture = false;
        }
        /// <summary>
        /// 双击父节点行（文字/图标区域）切换展开/折叠，资源管理器式行为；
        /// 双击 [+]/[-] 框不处理（两次单击本身已各切换一次，避免三重翻转）；叶子节点仅选中不折叠。
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button != MouseButtons.Left) return;
            RebuildRows();
            HRowInfo row = HitRow(e.Location);
            if (row == null || !row.Node.HasChildren) return;
            int px = PlusBoxX(row.Level);
            Rectangle plusRect = new Rectangle(px, row.Y - hScrollPx + hItemHeight / 2 - 5, 11, 11);
            if (!plusRect.Contains(e.Location)) ToggleExpand(row.Node);
        }
        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (hMouseDownOnThumb)
            {
                Rectangle track = new Rectangle(Width - 13, 0, 13, Height);
                int totalH = hRows.Count * hItemHeight;
                int range = track.Height - ThumbRect(track).Height - 4;
                if (totalH > ContentViewHeight && range > 0)
                {
                    int ty = e.Y - hThumbDragOffset - 2;
                    hScrollPx = (int)((float)ty / range * (totalH - ContentViewHeight));
                    ClampScroll();
                    Invalidate();
                }
                return;
            }
            RebuildRows();
            HRowInfo row = HitRow(e.Location);
            HNavigatorNode n = row != null ? row.Node : null;
            if (n != hHover) { hHover = n; Invalidate(); }
        }
        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hHover != null) { hHover = null; Invalidate(); }
        }
        /// <summary>响应 MouseWheel 事件。</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int lines = -e.Delta / 120 * 3;
            hScrollPx += lines * hItemHeight;
            ClampScroll();
            Invalidate();
        }
        /// <summary>展开/折叠节点</summary>
        public void ToggleExpand(HNavigatorNode node)
        {
            if (node == null || !node.HasChildren) return;
            node.hExpanded = !node.hExpanded;
            Invalidate();
            var args = new HNavigatorNodeEventArgs(node);
            if (node.hExpanded) AfterExpand?.Invoke(this, args);
            else AfterCollapse?.Invoke(this, args);
        }
        /// <summary>判断是否 InputKey。</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
        /// <summary>响应 KeyDown 事件。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            RebuildRows();
            if (hRows.Count == 0) return;
            int idx = hSelected != null ? RowsIndexOf(hSelected) : -1;
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (idx < hRows.Count - 1) SelectedNode = hRows[idx + 1].Node;
                    break;
                case Keys.Up:
                    if (idx > 0) SelectedNode = hRows[idx - 1].Node;
                    else if (idx < 0) SelectedNode = hRows[0].Node;
                    break;
                case Keys.Right:
                    if (hSelected != null)
                    {
                        if (!hSelected.hExpanded && hSelected.HasChildren) ToggleExpand(hSelected);
                        else if (idx < hRows.Count - 1) SelectedNode = hRows[idx + 1].Node;
                    }
                    break;
                case Keys.Left:
                    if (hSelected != null)
                    {
                        if (hSelected.hExpanded && hSelected.HasChildren) ToggleExpand(hSelected);
                        else if (hSelected.Parent != null) SelectedNode = hSelected.Parent;
                    }
                    break;
                case Keys.Home:
                    SelectedNode = hRows[0].Node;
                    break;
                case Keys.End:
                    SelectedNode = hRows[hRows.Count - 1].Node;
                    break;
            }
            // 让选中节点保持可见
            int selIdx = hSelected != null ? RowsIndexOf(hSelected) : -1;
            if (selIdx >= 0)
            {
                int top = selIdx * hItemHeight;
                if (top < hScrollPx) hScrollPx = top;
                else if (top + hItemHeight > hScrollPx + ContentViewHeight)
                    hScrollPx = top + hItemHeight - ContentViewHeight;
            }
            Invalidate();
            e.Handled = true;
        }
        /// <summary>响应 GotFocus 事件。</summary>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }
        /// <summary>响应 LostFocus 事件。</summary>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
        /// <summary>响应 Resize 事件。</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampScroll();
            Invalidate();
        }
        #endregion
    }
    /// <summary>树节点事件参数</summary>
    public class HNavigatorNodeEventArgs : EventArgs
    {
        /// <summary>关联节点</summary>
        public HNavigatorNode Node { get; private set; }
        public HNavigatorNodeEventArgs(HNavigatorNode node)
        {
            Node = node;
        }
    }
}
