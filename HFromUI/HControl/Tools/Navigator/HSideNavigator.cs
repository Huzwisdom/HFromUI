using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Navigator
{
    using HFromUI.HControl.Base;
    using HFromUI.HControl.Tools.UiKit;

    /// <summary>
    /// 深蓝侧边导航树节点：一级栏目（矢量图标 + 加粗标题 + 展开箭头）下挂缩进的子项，可多级嵌套。
    /// 用法：var node = nav.Nodes.Add("设备管理", HGlyphKind.Box); node.Nodes.Add("设备台账", HGlyphKind.Layers);
    /// </summary>
    [DefaultProperty("Text")]
    public class HSideNavNode
    {
        internal string hText;
        internal HGlyphKind hIcon;
        private object hTag;
        internal bool hExpanded;
        internal HSideNavNode hParent;
        internal readonly List<HSideNavNode> hChildren = new List<HSideNavNode>();

        /// <summary>构造一个空节点。</summary>
        public HSideNavNode() : this("") { }
        /// <summary>按文字构造叶子节点。</summary>
        public HSideNavNode(string text) : this(text, HGlyphKind.Layers) { }
        /// <summary>按文字与矢量图标构造节点。</summary>
        public HSideNavNode(string text, HGlyphKind icon)
        {
            hText = text ?? "";
            hIcon = icon;
        }

        /// <summary>节点文字。</summary>
        [Category("HFromUI"), Description("节点文字")]
        public string Text { get { return hText; } set { hText = value ?? ""; } }
        /// <summary>一级栏目的矢量图标（子项不显示图标，仅前缀短横线）。</summary>
        [Category("HFromUI"), Description("矢量图标")]
        public HGlyphKind Icon { get { return hIcon; } set { hIcon = value; } }
        /// <summary>自定义数据。</summary>
        [Category("HFromUI"), Description("自定义数据"), DefaultValue(null)]
        public object Tag { get { return hTag; } set { hTag = value; } }
        /// <summary>是否已展开。</summary>
        [Category("HFromUI"), Description("是否已展开"), DefaultValue(false)]
        public bool Expanded { get { return hExpanded; } set { hExpanded = value; } }
        /// <summary>父节点（根节点为 null）。</summary>
        [Browsable(false)]
        public HSideNavNode Parent { get { return hParent; } }
        /// <summary>子节点集合。</summary>
        [Browsable(false)]
        public List<HSideNavNode> Nodes { get { return hChildren; } }
        /// <summary>是否有子节点。</summary>
        [Browsable(false)]
        public bool HasChildren { get { return hChildren.Count > 0; } }
        /// <summary>层级（根为 0）。</summary>
        [Browsable(false)]
        public int Level
        {
            get
            {
                int lv = 0;
                HSideNavNode p = hParent;
                while (p != null) { lv++; p = p.hParent; }
                return lv;
            }
        }

        /// <summary>添加子节点，返回被添加的节点。</summary>
        public HSideNavNode Add(HSideNavNode node)
        {
            if (node == null) return null;
            if (node.hParent != null) node.hParent.hChildren.Remove(node);
            node.hParent = this;
            hChildren.Add(node);
            return node;
        }

        /// <summary>快捷创建并添加子节点，返回新节点。</summary>
        public HSideNavNode Add(string text, HGlyphKind icon)
        {
            return Add(new HSideNavNode(text, icon));
        }

        /// <summary>快捷创建无子图标的叶子节点（仅一级栏目需要图标）。</summary>
        public HSideNavNode Add(string text)
        {
            return Add(new HSideNavNode(text, HGlyphKind.Layers));
        }

        /// <summary>移除子节点。</summary>
        public void Remove(HSideNavNode node)
        {
            if (node == null) return;
            if (hChildren.Remove(node)) node.hParent = null;
        }

        /// <summary>清空子节点。</summary>
        public void Clear()
        {
            foreach (HSideNavNode n in hChildren) n.hParent = null;
            hChildren.Clear();
        }
    }

    /// <summary>深蓝侧边导航树节点事件参数。</summary>
    public class HSideNavNodeEventArgs : EventArgs
    {
        /// <summary>构造事件参数。</summary>
        public HSideNavNodeEventArgs(HSideNavNode node) { Node = node; }
        /// <summary>触发事件的节点。</summary>
        public HSideNavNode Node { get; private set; }
    }

    /// <summary>
    /// 深蓝企业风侧边树状导航（参考 PLM/MES 系统侧边栏）：
    /// 整栏深藏青底，一级栏目 = 白色线性图标 + 白色加粗文字 + 右侧 chevron（展开 ^ / 折叠 v / 叶子 &gt;），
    /// 展开后子项缩进并前缀短横线、文字偏灰；选中项与其所属一级栏目同时显示亮蓝色圆角胶囊高亮。
    /// 纯 GDI+ 自绘，继承 HControl.Base.HLabelBase，支持展开缓动、滚轮、自绘细滚动条与键盘导航。
    /// </summary>
    [DefaultEvent("AfterSelect")]
    [ToolboxItem(true)]
    public class HSideNavigator : HLabelBase
    {
        #region 内部结构
        /// <summary>展平后的可见行（Y 为动画过程中的浮点行顶）。</summary>
        private class HSideRow
        {
            public HSideNavNode Node;
            public int Level;
            public double Y;
            public int Height;
            /// <summary>受展开动画裁剪时该行内容允许显示到的底边（double.MaxValue 表示不裁剪）。</summary>
            public double RevealBottom;
            public Rectangle Rect { get { return new Rectangle(0, (int)Math.Round(Y), 0, Height); } }
        }
        #endregion

        #region 字段
        private readonly List<HSideNavNode> hNodes = new List<HSideNavNode>();
        private readonly List<HSideRow> hRows = new List<HSideRow>();
        private readonly Dictionary<HSideNavNode, double> hAnimRatio = new Dictionary<HSideNavNode, double>();
        private readonly Timer hAnimTimer;
        private readonly List<HSideNavNode> hAnimating = new List<HSideNavNode>();
        private Font hBoldFont;

        private HSideNavNode hSelected;
        private HSideRow hHoverRow;
        private int hScrollPx;
        private bool hMouseDownOnThumb;
        private int hThumbDragOffset;
        private bool hMouseDownOnRow;

        // 布局
        private int hHeaderHeight = 50;
        private int hItemHeight = 42;
        private int hTopPadding = 8;
        private int hPillInset = 12;
        private int hPillVInset = 5;
        private int hPillRadius = 10;
        private int hIconLeft = 24;
        private int hTextLeft = 60;
        private int hChildStep = 26;
        private bool hOneAtATime;
        private bool hAnimate = true;

        // 外观
        private Color hAccentColor = Color.FromArgb(34, 102, 238);
        private Color hHeaderForeColor = Color.White;
        private Color hSubForeColor = Color.FromArgb(159, 176, 207);
        private Color hHoverColor = Color.FromArgb(26, 255, 255, 255);
        private Color hChevronColor = Color.FromArgb(168, 183, 210);
        private Color hDashColor = Color.FromArgb(123, 141, 173);
        private Color hScrollThumbColor = Color.FromArgb(70, 255, 255, 255);
        #endregion

        #region 事件
        /// <summary>选中节点变化后触发。</summary>
        public event EventHandler<HSideNavNodeEventArgs> AfterSelect;
        /// <summary>节点展开后触发。</summary>
        public event EventHandler<HSideNavNodeEventArgs> AfterExpand;
        /// <summary>节点折叠后触发。</summary>
        public event EventHandler<HSideNavNodeEventArgs> AfterCollapse;
        /// <summary>节点被点击（左键按下并抬起于同一行）。</summary>
        public event EventHandler<HSideNavNodeEventArgs> NodeClick;
        #endregion

        /// <summary>构造深蓝侧边导航树。</summary>
        public HSideNavigator()
        {
            // 基类默认 AutoSize=true 且按标签测量尺寸，导航树为定高列表，必须先关闭再设尺寸
            AutoSize = false;
            SetStyle(ControlStyles.Selectable, true);
            BackColor = Color.FromArgb(11, 31, 69);
            Font = new Font("微软雅黑", 10F);
            Width = 220;
            Height = 520;
            TabStop = true;
            hBoldFont = new Font(Font, FontStyle.Bold);
            hAnimTimer = new Timer { Interval = 15 };
            hAnimTimer.Tick += AnimTimer_Tick;
        }

        #region 属性
        /// <summary>根节点（一级栏目）集合。</summary>
        [Browsable(false)]
        public List<HSideNavNode> Nodes { get { return hNodes; } }

        /// <summary>当前选中节点（null 表示无选中）；选中子项时其一级栏目同步胶囊高亮。</summary>
        [Browsable(false)]
        public HSideNavNode SelectedNode
        {
            get { return hSelected; }
            set
            {
                if (hSelected == value) return;
                hSelected = value;
                EnsureVisible(value);
                Invalidate();
                AfterSelect?.Invoke(this, new HSideNavNodeEventArgs(value));
            }
        }

        /// <summary>一级栏目行高。</summary>
        [Category("HFromUI"), Description("一级栏目行高"), DefaultValue(50)]
        public int HeaderHeight
        {
            get { return hHeaderHeight; }
            set { hHeaderHeight = Math.Max(28, value); RebuildAndInvalidate(); }
        }

        /// <summary>子项行高。</summary>
        [Category("HFromUI"), Description("子项行高"), DefaultValue(42)]
        public int ItemHeight
        {
            get { return hItemHeight; }
            set { hItemHeight = Math.Max(20, value); RebuildAndInvalidate(); }
        }

        /// <summary>是否同一时刻只允许展开一个一级栏目（手风琴）。默认 false，与参考图一致可多栏同开。</summary>
        [Category("HFromUI"), Description("是否同一时刻只展开一个栏目"), DefaultValue(false)]
        public bool OneAtATime
        {
            get { return hOneAtATime; }
            set { hOneAtATime = value; }
        }

        /// <summary>展开/折叠时是否播放高度缓动动画。</summary>
        [Category("HFromUI"), Description("展开折叠是否播放缓动动画"), DefaultValue(true)]
        public bool Animate
        {
            get { return hAnimate; }
            set { hAnimate = value; }
        }

        /// <summary>选中胶囊亮蓝色。</summary>
        [Category("HFromUI"), Description("选中项胶囊高亮色")]
        public Color AccentColor
        {
            get { return hAccentColor; }
            set { hAccentColor = value; Invalidate(); }
        }

        /// <summary>一级栏目标题颜色。</summary>
        [Category("HFromUI"), Description("一级栏目标题颜色")]
        public Color HeaderForeColor
        {
            get { return hHeaderForeColor; }
            set { hHeaderForeColor = value; Invalidate(); }
        }

        /// <summary>子项文字颜色。</summary>
        [Category("HFromUI"), Description("子项文字颜色")]
        public Color SubForeColor
        {
            get { return hSubForeColor; }
            set { hSubForeColor = value; Invalidate(); }
        }

        /// <summary>鼠标悬停行的覆盖色（半透明白）。</summary>
        [Category("HFromUI"), Description("鼠标悬停行覆盖色")]
        public Color HoverColor
        {
            get { return hHoverColor; }
            set { hHoverColor = value; Invalidate(); }
        }

        /// <summary>栏目右侧 chevron 箭头颜色。</summary>
        [Category("HFromUI"), Description("展开折叠箭头颜色")]
        public Color ChevronColor
        {
            get { return hChevronColor; }
            set { hChevronColor = value; Invalidate(); }
        }

        /// <summary>子项前缀短横线颜色。</summary>
        [Category("HFromUI"), Description("子项前缀短横线颜色")]
        public Color DashColor
        {
            get { return hDashColor; }
            set { hDashColor = value; Invalidate(); }
        }

        /// <summary>是否应显示自定义滚动条（内容超出可视区时）。</summary>
        private bool ShowScrollBar { get { return ContentHeight() > ClientSize.Height; } }
        #endregion

        #region 公共方法
        /// <summary>快捷添加一级栏目，返回新节点。</summary>
        public HSideNavNode AddNode(string text, HGlyphKind icon)
        {
            var node = new HSideNavNode(text, icon);
            node.hParent = null;
            hNodes.Add(node);
            RebuildAndInvalidate();
            return node;
        }

        /// <summary>全部展开。</summary>
        public void ExpandAll()
        {
            foreach (HSideNavNode root in hNodes) ExpandRecursive(root);
            hAnimating.Clear();
            hAnimRatio.Clear();
            hAnimTimer.Stop();
            RebuildAndInvalidate();
        }

        /// <summary>全部折叠。</summary>
        public void CollapseAll()
        {
            foreach (HSideNavNode root in hNodes) CollapseRecursive(root);
            hAnimating.Clear();
            hAnimRatio.Clear();
            hAnimTimer.Stop();
            ClampScroll();
            RebuildAndInvalidate();
        }

        /// <summary>滚动到指定节点（沿祖先链展开）。</summary>
        public void EnsureVisible(HSideNavNode node)
        {
            if (node == null) return;
            HSideNavNode p = node.hParent;
            bool changed = false;
            while (p != null)
            {
                if (!p.hExpanded) { p.hExpanded = true; changed = true; }
                p = p.hParent;
            }
            if (changed || hAnimating.Count > 0)
            {
                hAnimating.Clear();
                hAnimTimer.Stop();
                foreach (HSideNavNode n in hAnimRatio.Keys)
                    if (n.hExpanded) hAnimRatio[n] = 1.0;
            }
            RebuildRows();
            HSideRow row = FindRow(node);
            if (row != null)
            {
                if (row.Y < hTopPadding) hScrollPx -= (int)(hTopPadding - row.Y);
                else if (row.Y + row.Height > ClientSize.Height - hTopPadding)
                    hScrollPx += (int)(row.Y + row.Height - (ClientSize.Height - hTopPadding));
                ClampScroll();
                // 滚动偏移改变后必须按新偏移重新展平，否则命中测试与绘制仍用旧行位置
                RebuildRows();
            }
            Invalidate();
        }
        #endregion

        #region 布局
        private static void ExpandRecursive(HSideNavNode node)
        {
            if (node.hChildren.Count == 0) return;
            node.hExpanded = true;
            foreach (HSideNavNode c in node.hChildren) ExpandRecursive(c);
        }

        private static void CollapseRecursive(HSideNavNode node)
        {
            if (node.hChildren.Count == 0) return;
            node.hExpanded = false;
            foreach (HSideNavNode c in node.hChildren) CollapseRecursive(c);
        }

        /// <summary>节点子树当前（受动画影响）占用的高度。</summary>
        private double BlockHeight(HSideNavNode node, int level)
        {
            if (!node.hExpanded && !hAnimRatio.ContainsKey(node)) return 0;
            double ratio = GetRatio(node);
            if (ratio <= 0.001) return 0;
            double full = 0;
            foreach (HSideNavNode c in node.hChildren)
            {
                full += RowHeight(level);
                if (c.hChildren.Count > 0) full += BlockHeight(c, level + 1);
            }
            return full * ratio;
        }

        private int RowHeight(int level) { return level == 0 ? hHeaderHeight : hItemHeight; }

        private double GetRatio(HSideNavNode node)
        {
            double r;
            if (hAnimRatio.TryGetValue(node, out r)) return r;
            return node.hExpanded ? 1.0 : 0.0;
        }

        private int ContentHeight()
        {
            double h = hTopPadding;
            foreach (HSideNavNode root in hNodes)
            {
                h += hHeaderHeight;
                h += BlockHeight(root, 1);
            }
            return (int)Math.Ceiling(h) + hTopPadding;
        }

        private void ClampScroll()
        {
            int max = Math.Max(0, ContentHeight() - ClientSize.Height);
            if (hScrollPx > max) hScrollPx = max;
            if (hScrollPx < 0) hScrollPx = 0;
        }

        private void RebuildRows()
        {
            hRows.Clear();
            double y = hTopPadding - hScrollPx;
            foreach (HSideNavNode root in hNodes)
            {
                y = CollectRow(root, 0, y, double.MaxValue);
            }
        }

        /// <summary>收集一个节点行及其展开的子孙行，返回下一起点 Y。</summary>
        private double CollectRow(HSideNavNode node, int level, double y, double revealBottom)
        {
            int rh = RowHeight(level);
            var row = new HSideRow { Node = node, Level = level, Y = y, Height = rh, RevealBottom = revealBottom };
            hRows.Add(row);
            y += rh;
            if (node.hChildren.Count > 0)
            {
                double ratio = GetRatio(node);
                if (ratio > 0.001)
                {
                    double full = SubtreeFullHeight(node, level + 1);
                    double childBottom = y + full * ratio;
                    foreach (HSideNavNode c in node.hChildren)
                        y = CollectRow(c, level + 1, y, Math.Min(revealBottom, childBottom));
                    // 子树占用按动画高度结算（子孙行展开后的偏移已在递归中计算，这里校正差值）
                    y = row.Y + rh + BlockHeight(node, level + 1);
                }
            }
            return y;
        }

        /// <summary>子孙全部展开时的完整高度。</summary>
        private double SubtreeFullHeight(HSideNavNode node, int level)
        {
            double full = 0;
            foreach (HSideNavNode c in node.hChildren)
            {
                full += RowHeight(level);
                if (c.hChildren.Count > 0) full += SubtreeFullHeight(c, level + 1);
            }
            return full;
        }

        private HSideRow FindRow(HSideNavNode node)
        {
            foreach (HSideRow r in hRows)
                if (r.Node == node) return r;
            return null;
        }

        private void RebuildAndInvalidate()
        {
            ClampScroll();
            RebuildRows();
            Invalidate();
        }
        #endregion

        #region 动画
        private void StartExpandAnimation(HSideNavNode node, bool expand)
        {
            hAnimRatio[node] = expand ? 0.0 : 1.0;
            if (!hAnimating.Contains(node)) hAnimating.Add(node);
            hAnimTimer.Start();
        }

        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            for (int i = hAnimating.Count - 1; i >= 0; i--)
            {
                HSideNavNode node = hAnimating[i];
                double cur = GetRatio(node);
                double target = node.hExpanded ? 1.0 : 0.0;
                double next = cur + (target - cur) * 0.28;
                if (Math.Abs(target - next) < 0.02)
                {
                    next = target;
                    hAnimRatio.Remove(node);
                    hAnimating.RemoveAt(i);
                    if (node.hExpanded) AfterExpand?.Invoke(this, new HSideNavNodeEventArgs(node));
                    else AfterCollapse?.Invoke(this, new HSideNavNodeEventArgs(node));
                }
                else
                {
                    hAnimRatio[node] = next;
                }
            }
            ClampScroll();
            RebuildRows();
            Invalidate();
            if (hAnimating.Count == 0)
            {
                hAnimTimer.Stop();
                ClampScroll();
                RebuildRows();
                Invalidate();
            }
        }
        #endregion

        #region 交互
        /// <summary>命中测试：返回指定客户区坐标对应的可见行（动画裁剪区外的子行不可点）。</summary>
        private HSideRow HitRow(Point pt)
        {
            for (int i = hRows.Count - 1; i >= 0; i--)
            {
                HSideRow r = hRows[i];
                int top = (int)Math.Round(r.Y);
                if (pt.Y >= top && pt.Y <= top + r.Height && pt.X >= 0 && pt.X <= ClientSize.Width)
                {
                    if (pt.Y <= r.RevealBottom) return r;
                }
            }
            return null;
        }

        /// <summary>展开或折叠节点（带动画），返回最终展开状态。</summary>
        private bool ToggleNode(HSideNavNode node)
        {
            bool expand = !node.hExpanded;
            node.hExpanded = expand;
            if (expand && hOneAtATime && node.hParent == null)
            {
                foreach (HSideNavNode other in hNodes)
                {
                    if (other != node && other.hExpanded)
                    {
                        other.hExpanded = false;
                        if (hAnimate) StartExpandAnimation(other, false);
                        else AfterCollapse?.Invoke(this, new HSideNavNodeEventArgs(other));
                    }
                }
            }
            if (hAnimate) StartExpandAnimation(node, expand);
            else
            {
                hAnimRatio.Remove(node);
                if (expand) AfterExpand?.Invoke(this, new HSideNavNodeEventArgs(node));
                else AfterCollapse?.Invoke(this, new HSideNavNodeEventArgs(node));
            }
            ClampScroll();
            RebuildRows();
            Invalidate();
            return expand;
        }

        /// <summary>处理行点击：父行切换展开，叶子/子行执行选中。</summary>
        private void HandleRowClick(HSideRow row)
        {
            HSideNavNode node = row.Node;
            NodeClick?.Invoke(this, new HSideNavNodeEventArgs(node));
            if (node.hChildren.Count > 0)
            {
                // 点击一级栏目的图标/文字区域：既切换展开，也把该栏目作为高亮项
                if (hSelected != node)
                {
                    hSelected = node;
                    AfterSelect?.Invoke(this, new HSideNavNodeEventArgs(node));
                }
                ToggleNode(node);
            }
            else
            {
                if (hSelected != node)
                {
                    hSelected = node;
                    Invalidate();
                    AfterSelect?.Invoke(this, new HSideNavNodeEventArgs(node));
                }
            }
        }

        /// <summary>取选中节点所属的一级栏目（选中本身是一级栏目时返回它自身）。</summary>
        private HSideNavNode RootOf(HSideNavNode node)
        {
            if (node == null) return null;
            while (node.hParent != null) node = node.hParent;
            return node;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button != MouseButtons.Left) return;
            Rectangle bar = ScrollBarRect();
            if (ShowScrollBar && bar.Contains(e.Location))
            {
                Rectangle thumb = ThumbRect();
                if (thumb.Contains(e.Location))
                {
                    hMouseDownOnThumb = true;
                    hThumbDragOffset = e.Y - thumb.Top;
                }
                else
                {
                    // 点击轨道翻页
                    int delta = ClientSize.Height - 20;
                    hScrollPx += e.Y < thumb.Top ? -delta : delta;
                    ClampScroll();
                    RebuildRows();
                    Invalidate();
                }
                return;
            }
            HSideRow row = HitRow(e.Location);
            if (row != null)
            {
                hMouseDownOnRow = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            bool wasThumb = hMouseDownOnThumb;
            bool wasRow = hMouseDownOnRow;
            hMouseDownOnThumb = false;
            hMouseDownOnRow = false;
            if (e.Button != MouseButtons.Left) return;
            if (!wasThumb)
            {
                HSideRow row = HitRow(e.Location);
                if (wasRow && row != null) HandleRowClick(row);
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (hMouseDownOnThumb && ShowScrollBar)
            {
                Rectangle bar = ScrollBarRect();
                Rectangle thumb = ThumbRect();
                int trackH = bar.Height - thumb.Height;
                int maxScroll = Math.Max(0, ContentHeight() - ClientSize.Height);
                if (trackH > 0)
                {
                    hScrollPx = (int)Math.Round((e.Y - bar.Top - hThumbDragOffset) * (double)maxScroll / trackH);
                    ClampScroll();
                    RebuildRows();
                    Invalidate();
                }
                return;
            }
            HSideRow row = HitRow(e.Location);
            if (row != hHoverRow)
            {
                hHoverRow = row;
                Cursor = (row != null || (ShowScrollBar && ScrollBarRect().Contains(e.Location)))
                    ? Cursors.Hand : Cursors.Default;
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
            hScrollPx -= (e.Delta / 120) * hItemHeight * lines;
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
            }
        }

        private int IndexOfRow(HSideNavNode node)
        {
            for (int i = 0; i < hRows.Count; i++)
                if (hRows[i].Node == node) return i;
            return -1;
        }
        #endregion

        #region 滚动条几何
        private Rectangle ScrollBarRect()
        {
            return new Rectangle(ClientSize.Width - 8, 4, 6, ClientSize.Height - 8);
        }

        private Rectangle ThumbRect()
        {
            Rectangle bar = ScrollBarRect();
            int content = ContentHeight();
            int max = Math.Max(0, content - ClientSize.Height);
            int thumbH = Math.Max(24, (int)Math.Round(bar.Height * (double)ClientSize.Height / content));
            int y = max <= 0 ? bar.Top : bar.Top + (int)Math.Round((bar.Height - thumbH) * (double)hScrollPx / max);
            return new Rectangle(bar.X, y, bar.Width, thumbH);
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

        /// <summary>整栏底色由 OnPaintBackground 不透明擦除，这里只画行与滚动条，完全覆盖基类标签绘制。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (hRows.Count == 0) RebuildRows();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var clipRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
            g.SetClip(clipRect);

            HSideNavNode activeRoot = RootOf(hSelected);
            foreach (HSideRow row in hRows)
            {
                int top = (int)Math.Round(row.Y);
                if (top + row.Height < 0 || top > ClientSize.Height) continue;
                DrawRow(g, row, top, activeRoot);
            }

            if (ShowScrollBar) DrawScrollBar(g);
        }

        private void DrawRow(Graphics g, HSideRow row, int top, HSideNavNode activeRoot)
        {
            HSideNavNode node = row.Node;
            bool level0 = row.Level == 0;
            bool isSelected = node == hSelected;
            bool isActiveHeader = level0 && node == activeRoot && hSelected != null && hSelected.hParent != null;
            bool isHover = hHoverRow == row && !isSelected && !isActiveHeader;
            Rectangle pill = new Rectangle(hPillInset, top + hPillVInset,
                ClientSize.Width - hPillInset * 2, row.Height - hPillVInset * 2);

            // 动画裁剪：子树展开过程中不越界绘制到下一个栏目
            var oldClip = g.Clip;
            if (row.RevealBottom < double.MaxValue)
            {
                int rb = (int)Math.Round(row.RevealBottom);
                var reveal = new Rectangle(0, 0, ClientSize.Width, Math.Min(ClientSize.Height, rb));
                g.SetClip(reveal, CombineMode.Intersect);
            }

            if (isSelected || isActiveHeader)
            {
                using (GraphicsPath path = HGlyph.RoundedRect(pill, hPillRadius))
                using (var brush = new SolidBrush(hAccentColor))
                    g.FillPath(brush, path);
            }
            else if (isHover)
            {
                using (GraphicsPath path = HGlyph.RoundedRect(pill, hPillRadius))
                using (var brush = new SolidBrush(hHoverColor))
                    g.FillPath(brush, path);
            }

            var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine
                        | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            if (level0)
            {
                // 一级栏目：白色图标 + 加粗白字 + 右侧 chevron
                int iconSize = 20;
                var iconRect = new Rectangle(hIconLeft, top + (row.Height - iconSize) / 2, iconSize, iconSize);
                HGlyph.Draw(g, node.hIcon, iconRect, isSelected || isActiveHeader ? Color.White : hHeaderForeColor, 1.8f);

                int chevronW = 20;
                var textRect = new Rectangle(hTextLeft, top, ClientSize.Width - hTextLeft - chevronW - 8, row.Height);
                TextRenderer.DrawText(g, node.hText, hBoldFont, textRect,
                    isSelected || isActiveHeader ? Color.White : hHeaderForeColor, flags);
                DrawChevron(g, node, top, row.Height);
            }
            else
            {
                // 子项：缩进短横线 + 浅色文字（选中变白加粗）
                int indent = hTextLeft + (row.Level - 1) * hChildStep;
                using (var pen = new Pen(isSelected ? Color.White : hDashColor, 1.6f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    int cy = top + row.Height / 2;
                    g.DrawLine(pen, hIconLeft + 1, cy, hIconLeft + 13, cy);
                }
                var textRect = new Rectangle(indent, top, ClientSize.Width - indent - hPillInset, row.Height);
                Font font = isSelected ? hBoldFont : Font;
                TextRenderer.DrawText(g, node.hText, font, textRect,
                    isSelected ? Color.White : hSubForeColor, flags);
            }

            g.Clip = oldClip;
        }

        /// <summary>绘制栏目右侧 chevron：展开 ^、折叠 v、叶子 &gt;。</summary>
        private void DrawChevron(Graphics g, HSideNavNode node, int top, int rowHeight)
        {
            if (node.hChildren.Count == 0)
            {
                // 叶子一级项显示淡淡的 >，表示可直接进入
                using (var pen = new Pen(Color.FromArgb(110, hChevronColor.R, hChevronColor.G, hChevronColor.B), 1.7f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    int cx = ClientSize.Width - 22;
                    int cy = top + rowHeight / 2;
                    Point[] pts = { new Point(cx - 3, cy - 5), new Point(cx + 2, cy), new Point(cx - 3, cy + 5) };
                    g.DrawLines(pen, pts);
                }
                return;
            }
            using (var pen = new Pen(hChevronColor, 1.9f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int cx = ClientSize.Width - 20;
                int cy = top + rowHeight / 2;
                if (node.hExpanded)
                {
                    Point[] pts = { new Point(cx - 5, cy + 2), new Point(cx, cy - 3), new Point(cx + 5, cy + 2) };
                    g.DrawLines(pen, pts);
                }
                else
                {
                    Point[] pts = { new Point(cx - 5, cy - 2), new Point(cx, cy + 3), new Point(cx + 5, cy - 2) };
                    g.DrawLines(pen, pts);
                }
            }
        }

        private void DrawScrollBar(Graphics g)
        {
            Rectangle thumb = ThumbRect();
            using (GraphicsPath path = HGlyph.RoundedRect(thumb, thumb.Width / 2))
            using (var brush = new SolidBrush(hScrollThumbColor))
                g.FillPath(brush, path);
        }
        #endregion

        /// <summary>释放动画 Timer 与加粗字体。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hAnimTimer.Stop();
                hAnimTimer.Dispose();
                if (hBoldFont != null) hBoldFont.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
