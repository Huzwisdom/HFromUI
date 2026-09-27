using HFromUI.HAttribute;
using HFromUI.HData;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    using HFromUI.HColor;
    using HFromUI.HData.Win;
    public partial class HTreeView : TreeView
    {
        #region 静态缓存位图（避免每次绘制 Clone 导致 GDI 句柄泄漏）

        /// <summary>_arrowRight 字段。</summary>
        private static readonly Bitmap _arrowRight = HPhoto.Get("rightBlue");
        /// <summary>_arrowDown 字段。</summary>
        private static readonly Bitmap _arrowDown = HPhoto.Get("downBlue");
        /// <summary>_checkNormal 字段。</summary>
        private static readonly Bitmap _checkNormal = HPhoto.Get("CheckNormal");
        /// <summary>_checkSelect 字段。</summary>
        private static readonly Bitmap _checkSelect = HPhoto.Get("CheckSelect");

        #endregion

        #region 属性定义

        /// <summary>rootFont 字段。</summary>
        private Font rootFont = new Font("微软雅黑", 12);
        /// <summary>nodeFont 字段。</summary>
        private Font nodeFont = new Font("微软雅黑", 11);
        /// <summary>rootTextColor 字段。</summary>
        private Color rootTextColor = Color.DodgerBlue;
        /// <summary>rootBackColor 字段。</summary>
        private Color rootBackColor = ColorTranslator.FromHtml("#333333");
        /// <summary>nodeTextColor 字段。</summary>
        private Color nodeTextColor = Color.White;
        /// <summary>nodeBackColor 字段。</summary>
        private Color nodeBackColor = ColorTranslator.FromHtml("#333333");
        /// <summary>nodeBorderColor 字段。</summary>
        private Color nodeBorderColor = Color.Gray;
        /// <summary>nodeMouseInBackColor 字段。</summary>
        private Color nodeMouseInBackColor = Color.DimGray;
        /// <summary>nodeSelectTextColor 字段。</summary>
        private Color nodeSelectTextColor = Color.White;
        /// <summary>nodeSelectBackColor 字段。</summary>
        private Color nodeSelectBackColor = Color.DimGray;
        /// <summary>nodeSelectBorderColor 字段。</summary>
        private Color nodeSelectBorderColor = Color.Gray;
        /// <summary>线颜色。</summary>
        private Color lineColor = ColorTranslator.FromHtml("#333333");
        /// <summary>iconOffset 字段。</summary>
        private Point iconOffset = new Point(0, 0);
        /// <summary>iconSize 字段。</summary>
        private Size iconSize = new Size(30, 30);

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("根节点字体"), HDescriptionLanguage("根节点字体"), Browsable(true)]
        public Font RootFont
        {
            get { return rootFont; }
            set
            {
                rootFont = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点字体"), HDescriptionLanguage("子节点字体"), Browsable(true)]
        public Font NodeFont
        {
            get { return nodeFont; }
            set
            {
                nodeFont = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("根节点文本颜色"), HDescriptionLanguage("根节点文本颜色"), Browsable(true)]
        public Color RootTextColor
        {
            get { return rootTextColor; }
            set
            {
                rootTextColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("根节点背景色"), HDescriptionLanguage("根节点背景色"), Browsable(true)]
        public Color RootBackColor
        {
            get { return rootBackColor; }
            set
            {
                rootBackColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点文本颜色"), HDescriptionLanguage("子节点文本颜色"), Browsable(true)]
        public Color NodeTextColor
        {
            get { return nodeTextColor; }
            set
            {
                nodeTextColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点背景色"), HDescriptionLanguage("子节点背景色"), Browsable(true)]
        public Color NodeBackColor
        {
            get { return nodeBackColor; }
            set
            {
                nodeBackColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点边框色"), HDescriptionLanguage("子节点边框色"), Browsable(true)]
        public Color NodeBorderColor
        {
            get { return nodeBorderColor; }
            set
            {
                nodeBorderColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点鼠标悬停背景色"), HDescriptionLanguage("子节点鼠标悬停背景色"), Browsable(true)]
        public Color NodeMouseInBackColor
        {
            get { return nodeMouseInBackColor; }
            set
            {
                nodeMouseInBackColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点选中文本色"), HDescriptionLanguage("子节点选中文本色"), Browsable(true)]
        public Color NodeSelectTextColor
        {
            get { return nodeSelectTextColor; }
            set
            {
                nodeSelectTextColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点选中背景色"), HDescriptionLanguage("子节点选中背景色"), Browsable(true)]
        public Color NodeSelectBackColor
        {
            get { return nodeSelectBackColor; }
            set
            {
                nodeSelectBackColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点选中边框色"), HDescriptionLanguage("子节点选中边框色"), Browsable(true)]
        public Color NodeSelectBorderColor
        {
            get { return nodeSelectBorderColor; }
            set
            {
                nodeSelectBorderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>NodeSelectBarColor 成员。</summary>
        /// <summary>NodeSelectBarColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("子节点选中左侧条颜色"), HDescriptionLanguage("子节点选中左侧条颜色"), Browsable(true)]
        public Color NodeSelectBarColor { get; set; } = Color.DodgerBlue;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("线条颜色"), HDescriptionLanguage("根节点与子节点连接线颜色"), Browsable(true)]
        public new Color LineColor
        {
            get { return lineColor; }
            set
            {
                lineColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("根节点图标的位置偏移"), HDescriptionLanguage("根节点图标的位置偏移"), Browsable(true)]
        public Point IconOffset
        {
            get { return iconOffset; }
            set
            {
                iconOffset = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("根节点图标的大小"), HDescriptionLanguage("根节点图标的大小"), Browsable(true)]
        public Size IconSize
        {
            get { return iconSize; }
            set
            {
                iconSize = value;
                this.Invalidate();
            }
        }

        /// <summary>PPImageList 成员。</summary>
        /// <summary>PPImageList 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("自定义图片列表"), HDescriptionLanguage("自定义图片列表"), Browsable(true)]
        public ImageList PPImageList { get; set; } = null;

        /// <summary>ShowCheckBox 成员。</summary>
        /// <summary>ShowCheckBox 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示勾选框"), HDescriptionLanguage("是否显示checkbox"), Browsable(true)]
        public bool ShowCheckBox { get; set; } = false;

        /// <summary>GlyphStyle 成员。</summary>
        /// <summary>GlyphStyle 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图标样式"), HDescriptionLanguage("展开/折叠(+/-)图形样式，共24种"), Browsable(true)]
        public HTreeGlyphStyle GlyphStyle { get; set; } = HTreeGlyphStyle.CircleOutline;

        /// <summary>ShowGlyph 成员。</summary>
        /// <summary>ShowGlyph 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示图标"), HDescriptionLanguage("是否显示左侧展开/折叠(+/-)图形"), Browsable(true)]
        public bool ShowGlyph { get; set; } = true;

        /// <summary>GlyphColor 成员。</summary>
        /// <summary>GlyphColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图标颜色"), HDescriptionLanguage("展开/折叠(+/-)图形颜色"), Browsable(true)]
        public Color GlyphColor { get; set; } = Color.DodgerBlue;

        /// <summary>ShowConnectorLines 成员。</summary>
        /// <summary>ShowConnectorLines 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示连接线"), HDescriptionLanguage("是否显示树形虚线连接线"), Browsable(true)]
        public bool ShowConnectorLines { get; set; } = true;

        /// <summary>ConnectorColor 成员。</summary>
        /// <summary>ConnectorColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("树形虚线连接线颜色"), HDescriptionLanguage("树形虚线连接线颜色"), Browsable(true)]
        public Color ConnectorColor { get; set; } = Color.FromArgb(96, 104, 122);

        #endregion 属性定义

        #region 展开/折叠图形样式（24种）

        /// <summary>展开/折叠指示符图形样式：收起态画 + /向右，展开态画 − /向下。</summary>
        public enum HTreeGlyphStyle
        {
            /// <summary>经典方形框 +/−（Windows 资源管理器风格）</summary>
            ClassicBox = 0,
            /// <summary>圆形描边 +/−</summary>
            CircleOutline = 1,
            /// <summary>实心圆 白色 +/−</summary>
            CircleFilled = 2,
            /// <summary>空心三角（右/下旋转）</summary>
            TriangleHollow = 3,
            /// <summary>实心三角（右/下旋转）</summary>
            TriangleFilled = 4,
            /// <summary>V 形箭头（›/˅）</summary>
            Chevron = 5,
            /// <summary>现代扁平粗 +/−（无框）</summary>
            FlatPlus = 6,
            /// <summary>菱形描边 +/−</summary>
            DiamondOutline = 7,
            /// <summary>实心菱形 白色 +/−</summary>
            DiamondFilled = 8,
            /// <summary>圆角方形描边 +/−</summary>
            RoundedSquareOutline = 9,
            /// <summary>实心圆角方形 白色 +/−</summary>
            RoundedSquareFilled = 10,
            /// <summary>六边形描边 +/−</summary>
            HexagonOutline = 11,
            /// <summary>实心六边形 白色 +/−</summary>
            HexagonFilled = 12,
            /// <summary>八边形描边 +/−</summary>
            OctagonOutline = 13,
            /// <summary>实心八边形 白色 +/−</summary>
            OctagonFilled = 14,
            /// <summary>双圆描边 +/−</summary>
            DoubleCircle = 15,
            /// <summary>粗箭头（→/↓，无框）</summary>
            BoldArrows = 16,
            /// <summary>方框内小三角（▶/▼）</summary>
            CaretInSquare = 17,
            /// <summary>实心徽章方块 白色 +/−</summary>
            SolidBadge = 18,
            /// <summary>极简细线 +/−（无框）</summary>
            Minimal = 19,
            /// <summary>圆形内 V 形箭头</summary>
            CircleChevron = 20,
            /// <summary>五边形描边 +/−</summary>
            PentagonOutline = 21,
            /// <summary>实心圆 白色三角（▶/▼）</summary>
            CaretCircleFilled = 22,
            /// <summary>虚线方框 +/−</summary>
            DashedBox = 23
        }

        /// <summary>每层缩进像素。</summary>
        private const int TreeIndent = 20;
        /// <summary>+/- 图形外框尺寸。</summary>
        private const int GlyphBoxSize = 12;

        /// <summary>指定层级图形左上角 X。</summary>
        private static int GlyphLeft(int level) { return level * TreeIndent + 4; }
        /// <summary>指定层级图形中心点 X。</summary>
        private static float GlyphCenterX(int level) { return level * TreeIndent + 4 + GlyphBoxSize / 2f; }
        /// <summary>指定层级内容（复选框/图标/文字）起始 X。</summary>
        private static int ContentX(int level) { return level * TreeIndent + TreeIndent; }

        #endregion

        /// <summary>TextDrawMode 成员。</summary>
        public drawMode TextDrawMode { get; set; } = drawMode.Anti;

        public enum drawMode
        {
            Anti,
            Clear
        }

        public HTreeView()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.SetStyle(
            ControlStyles.DoubleBuffer |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor, true);
            base.UpdateStyles();
            DoubleBuffered = true;
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            this.HotTracking = true;
            this.CheckBoxes = false;
            this.ImageList = null;
            this.ShowPlusMinus = false;
            this.FullRowSelect = false;
        }

        /// <summary>
        /// 重写CreateParams方法 解决控件过多加载闪烁问题
        /// </summary>
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
        //        return cp;
        //    }
        //}

        /// <summary>
        /// SizeChange导致treeNode闪屏
        /// </summary>
        private const int TVM_SETEXTENDEDSTYLE = 0x112C;

        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        /// <summary>UpdateExtendedStyles 方法。</summary>
        private void UpdateExtendedStyles()
        {
            int Style = 0;

            if (DoubleBuffered)
                Style |= TVS_EX_DOUBLEBUFFER;

            if (Style != 0)
                HWin32.SendMessage(Handle, TVM_SETEXTENDEDSTYLE, new IntPtr(TVS_EX_DOUBLEBUFFER), new IntPtr(Style));
        }

        /// <summary>响应 HandleCreated 事件。</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateExtendedStyles();
        }

        /// <summary>响应 DrawNode 事件。</summary>
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (e.Bounds.IsEmpty)
                return;

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Bitmap bmp = null;
            Graphics g = null;
            try
            {
                bmp = new Bitmap(this.Width, e.Bounds.Height);
                g = Graphics.FromImage(bmp);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.TextRenderingHint = TextDrawMode == drawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                if (e.Node.Level == 0)//根节点
                {
                    DrawRootNode(g, bmp, e);
                }
                else
                {
                    if (!e.Node.Bounds.IsEmpty)//如果子节点的Bounds属性不为空(Empty），绘制该节点
                    {
                        DrawChildNode(g, bmp, e);
                    }
                }
                e.Graphics.DrawImage(bmp, 0, e.Bounds.Top);
            }
            finally
            {
                g?.Dispose();
                if (bmp != null) bmp.Dispose();
            }
        }

        /// <summary>DrawRootNode 方法。</summary>
        private void DrawRootNode(Graphics g, Bitmap bmp, DrawTreeNodeEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            using (SolidBrush brush = new SolidBrush(rootTextColor))
            {
                try
                {
                    g.Clear(rootBackColor);

                    int contentStart = 0;
                    if (ShowGlyph && e.Node.Nodes.Count > 0)
                    {
                        contentStart = ContentX(0);
                        bool hot = (e.State & TreeNodeStates.Hot) != 0;
                        DrawGlyph(g, GlyphLeft(0), (bmp.Height - GlyphBoxSize) / 2, e.Node.IsExpanded, hot);
                        // 展开时从图形底部向下画半行虚线段，与子节点行的竖线衔接
                        if (ShowConnectorLines && e.Node.IsExpanded)
                            DrawConnectorStub(g, GlyphCenterX(0), bmp.Height / 2f, bmp.Height);
                    }

                    if (ShowCheckBox)
                    {
                        Rectangle checkRect = new Rectangle(contentStart + 2, (bmp.Height - 15) / 2, 15, 15);
                        g.DrawImage(e.Node.Checked ? _checkNormal : _checkSelect, checkRect);
                    }

                    if (PPImageList != null && e.Node.ImageIndex != -1 && PPImageList.Images.Count - 1 >= e.Node.ImageIndex)
                    {
                        Rectangle imageRect = new Rectangle(new Point(contentStart + iconOffset.X + (ShowCheckBox ? 18 : 0), (bmp.Height - IconSize.Height) / 2 + iconOffset.Y), IconSize);
                        g.DrawImage(this.PPImageList.Images[e.Node.ImageIndex], imageRect);
                    }

                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    sf.FormatFlags = StringFormatFlags.NoClip;

                    int boundLeft = contentStart + (ShowCheckBox ? 18 : 0) + ((PPImageList != null) ? IconSize.Width : 0);
                    Rectangle rect = new Rectangle(boundLeft, 0, bmp.Width - boundLeft, bmp.Height);
                    g.DrawString(e.Node.Text, rootFont, brush, rect, sf);
                }
                catch { }
            }
        }

        /// <summary>DrawChildNode 方法。</summary>
        private void DrawChildNode(Graphics g, Bitmap bmp, DrawTreeNodeEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            using (SolidBrush textbrush = new SolidBrush(e.Node.IsSelected ? nodeSelectTextColor : nodeTextColor))
            using (SolidBrush barBrush = new SolidBrush(NodeSelectBarColor))
            {
                try
                {
                    int level = e.Node.Level;
                    int contentStart = ContentX(level);

                    if (e.Node.IsSelected)
                    {
                        g.Clear(nodeSelectBackColor);
                        g.FillRectangle(barBrush, 0, 0, 4, bmp.Height);
                    }
                    else
                    {
                        if ((e.State & TreeNodeStates.Hot) != 0)
                            g.Clear(nodeMouseInBackColor);
                        else
                            g.Clear(nodeBackColor);
                    }

                    // 同树层级之间的虚线连接（竖向贯穿线 + 指向本节点的水平拐角）
                    if (ShowConnectorLines)
                        DrawConnectors(g, e.Node, bmp.Height);

                    bool hasChildren = e.Node.Nodes.Count > 0;
                    if (ShowGlyph && hasChildren)
                    {
                        bool hot = (e.State & TreeNodeStates.Hot) != 0;
                        DrawGlyph(g, GlyphLeft(level), (bmp.Height - GlyphBoxSize) / 2, e.Node.IsExpanded, hot);
                        // 本节点展开时，图形底部到行底画半段虚线，衔接子节点行竖线
                        if (ShowConnectorLines && e.Node.IsExpanded)
                            DrawConnectorStub(g, GlyphCenterX(level), bmp.Height / 2f, bmp.Height);
                    }

                    if (ShowCheckBox)
                    {
                        Rectangle checkRect = new Rectangle(contentStart + 2, (bmp.Height - 15) / 2, 15, 15);
                        g.DrawImage(e.Node.Checked ? _checkNormal : _checkSelect, checkRect);
                    }

                    if (PPImageList != null && e.Node.ImageIndex != -1 && PPImageList.Images.Count - 1 >= e.Node.ImageIndex)
                    {
                        Rectangle imageRect = new Rectangle(new Point(contentStart + iconOffset.X + (ShowCheckBox ? 18 : 0), (bmp.Height - IconSize.Height) / 2 + iconOffset.Y), IconSize);
                        g.DrawImage(this.PPImageList.Images[e.Node.ImageIndex], imageRect);
                    }

                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    sf.FormatFlags = StringFormatFlags.NoClip;

                    int boundLeft = contentStart + (ShowCheckBox ? 18 : 0) + ((PPImageList != null) ? IconSize.Width : 0);
                    Rectangle rect = new Rectangle(boundLeft, 0, bmp.Width - boundLeft, bmp.Height);
                    g.DrawString(e.Node.Text, NodeFont, textbrush, rect, sf);
                }
                catch { }
            }
        }

        /// <summary>
        /// 绘制树形虚线连接线：对每个祖先层级，若该分支后续还有兄弟节点，
        /// 则在祖先图形中心列画贯穿本行的竖向虚线；紧邻父级再画水平拐角虚线。
        /// </summary>
        private void DrawConnectors(Graphics g, TreeNode node, int rowH)
        {
            float midY = rowH / 2f;
            using (Pen pen = new Pen(ConnectorColor, 1f) { DashStyle = DashStyle.Dash })
            {
                for (int k = 0; k < node.Level; k++)
                {
                    float cx = GlyphCenterX(k);
                    TreeNode pathChild = PathNodeAtLevel(node, k + 1);
                    if (pathChild == null) continue;
                    if (!IsLastSibling(pathChild))
                    {
                        // 该层级竖线在本行继续向下延伸
                        g.DrawLine(pen, cx, 0, cx, rowH);
                    }
                    if (k == node.Level - 1)
                    {
                        // 父级竖线到本节点图形的水平拐角
                        g.DrawLine(pen, cx, midY, GlyphCenterX(node.Level), midY);
                    }
                }
            }
        }

        /// <summary>展开状态下，从图形中心底部到行底的半段竖虚线（衔接子行）。</summary>
        private void DrawConnectorStub(Graphics g, float cx, float midY, int rowH)
        {
            using (Pen pen = new Pen(ConnectorColor, 1f) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(pen, cx, midY + GlyphBoxSize / 2f - 1, cx, rowH);
            }
        }

        /// <summary>取节点路径上位于指定层级的祖先节点（level=节点自身层级时返回自身）。</summary>
        private static TreeNode PathNodeAtLevel(TreeNode node, int level)
        {
            TreeNode n = node;
            while (n != null && n.Level > level) n = n.Parent;
            return n;
        }

        /// <summary>节点是否为其父节点（或根集合）的最后一个子节点。</summary>
        private static bool IsLastSibling(TreeNode n)
        {
            TreeNodeCollection col = n.Parent != null ? n.Parent.Nodes : n.TreeView != null ? n.TreeView.Nodes : null;
            if (col == null || col.Count == 0) return true;
            return col[col.Count - 1] == n;
        }

        /// <summary>
        /// 按 GlyphStyle 绘制展开/折叠指示符：收起态画 + /向右，展开态画 − /向下。
        /// 图形统一绘制在 (x,y) 起、GlyphBoxSize 见方的区域内，垂直方向调用方已居中。
        /// </summary>
        private void DrawGlyph(Graphics g, int x, int y, bool expanded, bool hot)
        {
            float s = GlyphBoxSize;
            float cx = x + s / 2f, cy = y + s / 2f;
            Color c = GlyphColor;
            Color fill = hot ? ControlPaint.Light(GlyphColor, 0.25f) : GlyphColor;
            RectangleF box = new RectangleF(x, y, s, s);
            float lw = 1.2f;

            using (Pen pen = new Pen(c, lw))
            using (Pen whitePen = new Pen(Color.White, lw + 0.3f))
            using (SolidBrush brushFill = new SolidBrush(fill))
            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                whitePen.StartCap = whitePen.EndCap = LineCap.Round;

                // 外框形状（描边类）
                switch (GlyphStyle)
                {
                    case HTreeGlyphStyle.CircleOutline:
                    case HTreeGlyphStyle.CircleFilled:
                    case HTreeGlyphStyle.CircleChevron:
                    case HTreeGlyphStyle.CaretCircleFilled:
                    case HTreeGlyphStyle.DoubleCircle:
                        if (GlyphStyle == HTreeGlyphStyle.CircleFilled || GlyphStyle == HTreeGlyphStyle.CaretCircleFilled)
                            g.FillEllipse(brushFill, box);
                        else
                        {
                            g.DrawEllipse(pen, box);
                            if (GlyphStyle == HTreeGlyphStyle.DoubleCircle)
                                g.DrawEllipse(pen, x + 2.2f, y + 2.2f, s - 4.4f, s - 4.4f);
                        }
                        break;
                    case HTreeGlyphStyle.DiamondOutline:
                    case HTreeGlyphStyle.DiamondFilled:
                        DrawDiamond(g, box, GlyphStyle == HTreeGlyphStyle.DiamondFilled ? brushFill : null, pen);
                        break;
                    case HTreeGlyphStyle.RoundedSquareOutline:
                    case HTreeGlyphStyle.RoundedSquareFilled:
                        using (GraphicsPath rr = RoundedRect(box, 3f))
                        {
                            if (GlyphStyle == HTreeGlyphStyle.RoundedSquareFilled)
                                g.FillPath(brushFill, rr);
                            else
                                g.DrawPath(pen, rr);
                        }
                        break;
                    case HTreeGlyphStyle.HexagonOutline:
                    case HTreeGlyphStyle.HexagonFilled:
                        DrawPolygonN(g, box, 6, GlyphStyle == HTreeGlyphStyle.HexagonFilled ? brushFill : null, pen);
                        break;
                    case HTreeGlyphStyle.OctagonOutline:
                    case HTreeGlyphStyle.OctagonFilled:
                        DrawPolygonN(g, box, 8, GlyphStyle == HTreeGlyphStyle.OctagonFilled ? brushFill : null, pen);
                        break;
                    case HTreeGlyphStyle.PentagonOutline:
                        DrawPolygonN(g, box, 5, null, pen);
                        break;
                    case HTreeGlyphStyle.CaretInSquare:
                        g.DrawRectangle(pen, x, y, s, s);
                        break;
                    case HTreeGlyphStyle.SolidBadge:
                        g.FillRectangle(brushFill, box);
                        break;
                    case HTreeGlyphStyle.DashedBox:
                        using (Pen dashPen = new Pen(c, lw) { DashStyle = DashStyle.Dash })
                            g.DrawRectangle(dashPen, x, y, s, s);
                        break;
                    case HTreeGlyphStyle.ClassicBox:
                        g.DrawRectangle(pen, x + 0.5f, y + 0.5f, s - 1f, s - 1f);
                        break;
                }

                // 内部符号：+/− 或 三角/V 形/箭头
                bool filledShape = GlyphStyle == HTreeGlyphStyle.CircleFilled
                    || GlyphStyle == HTreeGlyphStyle.DiamondFilled
                    || GlyphStyle == HTreeGlyphStyle.RoundedSquareFilled
                    || GlyphStyle == HTreeGlyphStyle.HexagonFilled
                    || GlyphStyle == HTreeGlyphStyle.OctagonFilled
                    || GlyphStyle == HTreeGlyphStyle.SolidBadge
                    || GlyphStyle == HTreeGlyphStyle.CaretCircleFilled;
                Pen symPen = filledShape ? whitePen : pen;
                Brush symBrush = filledShape ? whiteBrush : brushFill;
                float arm = s * 0.26f;   // +/- 臂长
                float th = 1.4f;        // 符号线宽

                switch (GlyphStyle)
                {
                    case HTreeGlyphStyle.TriangleHollow:
                    case HTreeGlyphStyle.TriangleFilled:
                        DrawTriangle(g, cx, cy, s * 0.72f, expanded,
                            GlyphStyle == HTreeGlyphStyle.TriangleFilled ? symBrush : null,
                            GlyphStyle == HTreeGlyphStyle.TriangleFilled ? null : symPen);
                        break;
                    case HTreeGlyphStyle.Chevron:
                        DrawChevron(g, cx, cy, arm, expanded, symPen);
                        break;
                    case HTreeGlyphStyle.CircleChevron:
                        DrawChevron(g, cx, cy, arm * 0.85f, expanded, symPen);
                        break;
                    case HTreeGlyphStyle.BoldArrows:
                        DrawArrow(g, cx, cy, arm, expanded, symPen);
                        break;
                    case HTreeGlyphStyle.CaretInSquare:
                    case HTreeGlyphStyle.CaretCircleFilled:
                        DrawTriangle(g, cx, cy, s * 0.5f, expanded, symBrush, GlyphStyle == HTreeGlyphStyle.CaretInSquare ? symPen : null);
                        break;
                    case HTreeGlyphStyle.FlatPlus:
                        DrawPlusMinus(g, cx, cy, arm + 1.2f, expanded, new Pen(c, th) { StartCap = LineCap.Round, EndCap = LineCap.Round });
                        break;
                    case HTreeGlyphStyle.Minimal:
                        DrawPlusMinus(g, cx, cy, arm - 0.6f, expanded, new Pen(c, 1f));
                        break;
                    default:
                        // 所有框形/多边形：画 +/−
                        DrawPlusMinus(g, cx, cy, arm, expanded,
                            filledShape ? new Pen(Color.White, th) { StartCap = LineCap.Round, EndCap = LineCap.Round }
                                        : new Pen(c, th) { StartCap = LineCap.Round, EndCap = LineCap.Round });
                        break;
                }
            }
        }

        /// <summary>画 +（收起）/ −（展开）。</summary>
        private static void DrawPlusMinus(Graphics g, float cx, float cy, float arm, bool expanded, Pen pen)
        {
            g.DrawLine(pen, cx - arm, cy, cx + arm, cy);       // 横线
            if (!expanded) g.DrawLine(pen, cx, cy - arm, cx, cy + arm); // 收起时再画竖线
        }

        /// <summary>画 V 形：收起 ›（向右），展开 ˅（向下）。</summary>
        private static void DrawChevron(Graphics g, float cx, float cy, float arm, bool expanded, Pen pen)
        {
            float a = arm * 0.8f;
            if (!expanded)
            {
                g.DrawLine(pen, cx - a * 0.7f, cy - a, cx + a * 0.7f, cy);
                g.DrawLine(pen, cx + a * 0.7f, cy, cx - a * 0.7f, cy + a);
            }
            else
            {
                g.DrawLine(pen, cx - a, cy - a * 0.7f, cx, cy + a * 0.7f);
                g.DrawLine(pen, cx, cy + a * 0.7f, cx + a, cy - a * 0.7f);
            }
        }

        /// <summary>画粗箭头：收起 →，展开 ↓。</summary>
        private static void DrawArrow(Graphics g, float cx, float cy, float arm, bool expanded, Pen pen)
        {
            float a = arm * 1.05f;
            if (!expanded)
            {
                g.DrawLine(pen, cx - a, cy, cx + a, cy);
                g.DrawLine(pen, cx + a, cy, cx + a * 0.35f, cy - a * 0.65f);
                g.DrawLine(pen, cx + a, cy, cx + a * 0.35f, cy + a * 0.65f);
            }
            else
            {
                g.DrawLine(pen, cx, cy - a, cx, cy + a);
                g.DrawLine(pen, cx, cy + a, cx - a * 0.65f, cy + a * 0.35f);
                g.DrawLine(pen, cx, cy + a, cx + a * 0.65f, cy + a * 0.35f);
            }
        }

        /// <summary>画三角：收起向右，展开向下。brush 非空填充，pen 非空描边。</summary>
        private static void DrawTriangle(Graphics g, float cx, float cy, float size, bool expanded, Brush brush, Pen pen)
        {
            float r = size / 2f;
            PointF[] pts;
            if (!expanded)
            {
                pts = new PointF[]
                {
                    new PointF(cx - r * 0.55f, cy - r * 0.8f),
                    new PointF(cx + r * 0.85f, cy),
                    new PointF(cx - r * 0.55f, cy + r * 0.8f)
                };
            }
            else
            {
                pts = new PointF[]
                {
                    new PointF(cx - r * 0.8f, cy - r * 0.55f),
                    new PointF(cx + r * 0.8f, cy - r * 0.55f),
                    new PointF(cx, cy + r * 0.85f)
                };
            }
            if (brush != null) g.FillPolygon(brush, pts);
            if (pen != null) g.DrawPolygon(pen, pts);
        }

        /// <summary>DrawDiamond 方法。</summary>
        private static void DrawDiamond(Graphics g, RectangleF box, Brush fill, Pen pen)
        {
            PointF[] pts =
            {
                new PointF(box.Left + box.Width / 2f, box.Top),
                new PointF(box.Right, box.Top + box.Height / 2f),
                new PointF(box.Left + box.Width / 2f, box.Bottom),
                new PointF(box.Left, box.Top + box.Height / 2f)
            };
            if (fill != null) g.FillPolygon(fill, pts);
            else g.DrawPolygon(pen, pts);
        }

        /// <summary>画正多边形（外接矩形 box，n 边；顶点朝上）。</summary>
        private static void DrawPolygonN(Graphics g, RectangleF box, int n, Brush fill, Pen pen)
        {
            PointF[] pts = new PointF[n];
            float cx = box.Left + box.Width / 2f, cy = box.Top + box.Height / 2f;
            float r = box.Width / 2f;
            for (int i = 0; i < n; i++)
            {
                double ang = -Math.PI / 2 + i * 2 * Math.PI / n;
                pts[i] = new PointF(cx + (float)(r * Math.Cos(ang)), cy + (float)(r * Math.Sin(ang)));
            }
            if (fill != null) g.FillPolygon(fill, pts);
            else g.DrawPolygon(pen, pts);
        }

        /// <summary>RoundedRect 方法。</summary>
        private static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            float d = radius * 2;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.Left, r.Top, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        /// <summary>响应 MouseClick 事件。</summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //base.OnMouseClick(e);
                TreeNode tn = this.GetNodeAt(e.Location);
                if (tn == null)
                    return;

                if (0 != tn.Level)//点击一级节点不使二级节点的选中效果消失
                {
                    this.SelectedNode = tn;
                }

                if (this.CheckBoxes)
                {
                    Rectangle checkRect = new Rectangle(ContentX(tn.Level) + 2, tn.Bounds.Top + (tn.Bounds.Height - 15) / 2, 15, 15);
                    if (checkRect.Contains(e.Location))
                    {
                        tn.Checked = !tn.Checked;
                    }
                }

                //图标中心点向右的区域也能单击折叠与展开
                Rectangle bounds = new Rectangle(tn.Bounds.Left, tn.Bounds.Y, this.Width - tn.Bounds.Left, this.ItemHeight);
                if (tn != null && bounds.Contains(e.Location) == true)
                {
                    if (tn.IsExpanded == false)
                        tn.Expand();
                    else
                        tn.Collapse();
                }

                int h = 0;
                foreach (TreeNode node in this.Nodes)
                {
                    h += getheight(node);
                }

                if (h > this.Height)
                {
                    isScrollShow = true;
                }
                else
                {
                    isScrollShow = false;
                }
            }
        }

        /// <summary>currentNode 字段。</summary>
        private TreeNode currentNode = null;

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            TreeNode tn = this.GetNodeAt(e.Location);
            Graphics g = null;
            try
            {
                g = this.CreateGraphics();
                if (currentNode != tn)
                {
                    if (null != tn)
                    {
                        OnDrawNode(new DrawTreeNodeEventArgs(g, tn, new Rectangle(0, tn.Bounds.Y, this.Width - 4, tn.Bounds.Height), TreeNodeStates.Hot));
                    }
                    if (null != currentNode)
                    {
                        OnDrawNode(new DrawTreeNodeEventArgs(g, currentNode, new Rectangle(0, currentNode.Bounds.Y, this.Width - 4, currentNode.Bounds.Height), TreeNodeStates.Default));
                    }
                }

                if (tn != null)
                {
                    this.Cursor = (tn.Level == 1) ? Cursors.Hand : Cursors.Default;
                }
                currentNode = tn;
            }
            finally
            {
                g?.Dispose();
            }
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (currentNode != null)
            {
                Graphics g = null;
                try
                {
                    g = this.CreateGraphics();
                    OnDrawNode(new DrawTreeNodeEventArgs(g, currentNode, new Rectangle(0, currentNode.Bounds.Y, this.Width - 4, currentNode.Bounds.Height), TreeNodeStates.Default));
                    currentNode = null;
                }
                finally
                {
                    g?.Dispose();
                }
            }
        }

        /// <summary>isScrollShow 字段。</summary>
        private bool isScrollShow = false;

        /// <summary>响应 AfterExpand 事件。</summary>
        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            base.OnAfterExpand(e);
            int h = 0;
            foreach (TreeNode node in this.Nodes)
            {
                h += getheight(node);
            }

            if (h > this.Height)
            {
                isScrollShow = true;
            }
            else
            {
                isScrollShow = false;
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            int h = 0;
            foreach (TreeNode node in this.Nodes)
            {
                h += getheight(node);
            }

            if (h > this.Height)
            {
                isScrollShow = true;
            }
            else
            {
                isScrollShow = false;
            }
        }

        /// <summary>获取 height。</summary>
        private int getheight(TreeNode treeNode)
        {
            if (treeNode == null)
                return 0;
            int h = this.ItemHeight;
            foreach (TreeNode node in treeNode.Nodes)
            {
                if (node.IsVisible)
                {
                    h += getheight(node);
                }
            }
            return h;
        }
    }
}