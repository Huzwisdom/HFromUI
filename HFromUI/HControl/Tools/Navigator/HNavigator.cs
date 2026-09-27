using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using HFromUI.HControl.Base;
namespace HFromUI.HControl.Tools.Navigator
{
    using HFromUI.HControl.Tools.UiKit;
    /// <summary>底部菜单点击委托（参数为菜单项索引）</summary>
    public delegate void HNavigatorMenuClickHandler(object sender, int index);
    /// <summary>
    /// 工控导航面板（参考 WinCC flexible 导航器）：
    /// 深蓝醒目标题栏（当前栏目图标 + 白色加粗名称）+ 当前栏目树区（HNavigatorTree）+ "......" 分隔条 + 底部固定顺序栏目行。
    /// 栏目行顺序永不改变：点击某一行，该栏目的树上浮淡入树区，原栏目树下压淡出（约 200ms，只平移不缩放图标字体），
    /// 被激活的行保持金色高亮。树节点支持双击父节点展开/折叠。
    /// 用法：
    ///   var sec = navigator.AddSection("变量管理", HGlyphKind.BindersOrange);   // 首个栏目自动激活
    ///   sec.Nodes.Add(new HNavigatorNode("定时器", HGlyphKind.Stopwatch));
    ///   navigator.ActiveSectionChanged += ...;
    /// </summary>
    [DefaultEvent("SelectedMenuChanged")]
    [ToolboxItem(true)]
    public class HNavigator : HLabelBase
    {
        #region 常量与字段
        private const int HeaderHeight = 26;
        private const int SeparatorHeight = 13;
        private const int MenuItemHeight = 36;
        /// <summary>hTree 字段。</summary>
        private readonly HNavigatorTree hTree = new HNavigatorTree();
        /// <summary>栏目集合：顺序固定永不重排，每个栏目始终拥有自己的固定底部行</summary>
        private readonly List<HNavigatorSection> hSections = new List<HNavigatorSection>();
        /// <summary>底部栏目行镜像（与 hSections 同序，含当前激活栏目行）</summary>
        private readonly List<HSideMenuItem> hMenuItems = new List<HSideMenuItem>();
        /// <summary>当前在树区显示的栏目索引（栏目顺序固定，只切换激活项）</summary>
        private int hActiveIndex = -1;
        /// <summary>hTitle 字段。</summary>
        private string hTitle = "";
        private bool hCollapsed;
        /// <summary>hMenuHover 字段。</summary>
        private int hMenuHover = -1;
        // 栏目切换动画：整幅树位图平移 + 交叉淡入淡出（不缩放图标/字体）
        private const int AnimSteps = 13;       // 帧数（Timer 15ms ≈ 195ms）
        private const int AnimSlide = 96;       // 树图层纵向滑动距离（逻辑像素）
        private readonly Timer hAnimTimer;
        private bool hAnimating;
        private int hAnimStep;
        private Bitmap hBmpOld;
        private Bitmap hBmpNew;
        // 标题栏（激活栏目醒目态：深蓝渐变 + 白色加粗标题 + 白色图标底托）
        private Color hHeaderColor1 = Color.FromArgb(46, 99, 153);
        /// <summary>hHeaderColor2 字段。</summary>
        private Color hHeaderColor2 = Color.FromArgb(28, 69, 113);
        /// <summary>hHeaderLineColor 字段。</summary>
        private Color hHeaderLineColor = Color.FromArgb(15, 35, 60);
        /// <summary>hTitleColor 字段。</summary>
        private Color hTitleColor = Color.White;
        /// <summary>标题栏图标白色底托色</summary>
        private readonly Color hHeaderBadgeColor = Color.FromArgb(245, 248, 252);
        // 面板
        private Color hBorderColor = Color.FromArgb(150, 158, 168);
        /// <summary>hSeparatorDotColor 字段。</summary>
        private Color hSeparatorDotColor = Color.FromArgb(120, 128, 138);
        // 底部菜单
        private Color hMenuBackColor = Color.White;
        /// <summary>hMenuLineColor 字段。</summary>
        private Color hMenuLineColor = Color.FromArgb(226, 230, 234);
        /// <summary>hMenuTextColor 字段。</summary>
        private Color hMenuTextColor = Color.FromArgb(30, 33, 38);
        /// <summary>hMenuHoverColor1 字段。</summary>
        private Color hMenuHoverColor1 = Color.FromArgb(255, 251, 238);
        /// <summary>hMenuHoverColor2 字段。</summary>
        private Color hMenuHoverColor2 = Color.FromArgb(255, 236, 184);
        /// <summary>hMenuSelColor1 字段。</summary>
        private Color hMenuSelColor1 = Color.FromArgb(253, 240, 210);
        /// <summary>hMenuSelColor2 字段。</summary>
        private Color hMenuSelColor2 = Color.FromArgb(243, 184, 86);
        /// <summary>hMenuSelBorderColor 字段。</summary>
        private Color hMenuSelBorderColor = Color.FromArgb(208, 145, 60);
        #endregion
        #region 事件
        /// <summary>底部菜单选中项变化</summary>
        public event EventHandler SelectedMenuChanged;
        /// <summary>底部菜单项被点击（每次点击均触发）</summary>
        public event EventHandler<int> MenuItemClick;
        /// <summary>标题栏折叠按钮点击后触发（Collapsed 变化）</summary>
        public event EventHandler CollapsedChanged;
        /// <summary>上下栏目整栏互换完成（新栏目已上移到树区）后触发</summary>
        public event EventHandler ActiveSectionChanged;
        #endregion
        public HNavigator()
        {
            // 基类 HLabelBase 默认为 AutoSize 标签，导航面板是定高容器：关闭自动尺寸并打开容器样式
            AutoSize = false;
            SetStyle(ControlStyles.ContainerControl | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 9F);
            Width = 220;
            Height = 420;
            hAnimTimer = new Timer { Interval = 15 };
            hAnimTimer.Tick += AnimTimer_Tick;
            hTree.Location = new Point(1, HeaderHeight);
            Controls.Add(hTree);
            hTree.AfterSelect += (s, e) =>
            {
                if (hActiveIndex >= 0) hSections[hActiveIndex].SelectedNode = e.Node;
            };
            PerformLayoutTree();
        }
        /// <summary>释放切换动画 Timer 与位图。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hAnimTimer.Dispose();
                hBmpOld?.Dispose();
                hBmpNew?.Dispose();
            }
            base.Dispose(disposing);
        }
        #region 属性
        /// <summary>面板标题（顶部栏文字）；为空时标题栏显示当前上方栏目名</summary>
        [Category("HFromUI"), Description("面板标题（为空显示当前栏目名）")]
        public string Title
        {
            get { return hTitle; }
            set { hTitle = value ?? ""; Invalidate(); }
        }
        /// <summary>树控件（渲染当前上方栏目的节点）</summary>
        [Browsable(false)]
        public HNavigatorTree Tree { get { return hTree; } }
        /// <summary>全部栏目：顺序固定，当前激活栏目显示在树区，同时在底部行高亮</summary>
        [Browsable(false)]
        public List<HNavigatorSection> Sections { get { return hSections; } }
        /// <summary>当前在树区显示的栏目</summary>
        [Browsable(false)]
        public HNavigatorSection ActiveSection
        {
            get { return (hActiveIndex >= 0 && hActiveIndex < hSections.Count) ? hSections[hActiveIndex] : null; }
        }
        /// <summary>当前激活栏目的固定索引</summary>
        [Browsable(false)]
        public int ActiveIndex { get { return hActiveIndex; } }
        /// <summary>底部栏目行集合（与栏目同序固定，含激活行，只读使用；添加请用 AddSection）</summary>
        [Browsable(false)]
        public List<HSideMenuItem> MenuItems { get { return hMenuItems; } }
        /// <summary>当前激活栏目的固定索引（兼容旧 API；赋值立即切换，无切换动画）</summary>
        [Category("HFromUI"), Description("当前激活栏目索引"), DefaultValue(-1)]
        public int SelectedMenuIndex
        {
            get { return hActiveIndex; }
            set
            {
                if (hActiveIndex == value || value < 0 || value >= hSections.Count) return;
                ActivateSection(value, false);
                SelectedMenuChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>是否折叠树区（仅代码设置，界面无折叠按钮）</summary>
        [Category("HFromUI"), Description("是否折叠树区"), DefaultValue(false)]
        public bool Collapsed
        {
            get { return hCollapsed; }
            set
            {
                if (hCollapsed == value) return;
                hCollapsed = value;
                PerformLayoutTree();
                Invalidate();
                CollapsedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>标题栏高度</summary>
        [Category("HFromUI"), Description("标题栏高度")]
        public int HeaderH
        {
            get { return HeaderHeight; }
        }
        /// <summary>面板边框颜色</summary>
        [Category("HFromUI"), Description("面板边框颜色")]
        public Color BorderColor
        {
            get { return hBorderColor; }
            set { hBorderColor = value; Invalidate(); }
        }
        #endregion
        #region 公开方法
        /// <summary>
        /// 按固定顺序追加一个栏目（自带独立的树节点集合）。
        /// 第一个添加的栏目自动成为当前激活栏目（显示在树区）；栏目顺序此后永不改变。
        /// </summary>
        public HNavigatorSection AddSection(string text, HGlyphKind icon)
        {
            HNavigatorSection sec = new HNavigatorSection(text, icon);
            hSections.Add(sec);
            hMenuItems.Add(sec.Item);
            if (hActiveIndex < 0)
            {
                hActiveIndex = 0;
                hTree.AttachNodes(sec.Nodes);
            }
            PerformLayoutTree();
            Invalidate();
            return sec;
        }
        /// <summary>快捷添加栏目（兼容旧 API），返回其底部栏目行</summary>
        public HSideMenuItem AddMenuItem(string text, HGlyphKind icon)
        {
            HNavigatorSection sec = AddSection(text, icon);
            return sec.Item;
        }
        /// <summary>展开当前栏目树全部节点</summary>
        public void ExpandAll() { hTree.ExpandAll(); }
        /// <summary>折叠当前栏目树全部节点</summary>
        public void CollapseAll() { hTree.CollapseAll(); }
        #endregion
        #region 栏目切换动画
        /// <summary>底部第 index 栏目的行矩形（面板客户区坐标，顺序固定）</summary>
        private Rectangle MenuRowRect(int index)
        {
            int top = Height - 1 - MenuAreaHeight;
            if (top < HeaderHeight) top = HeaderHeight;
            return new Rectangle(0, top + index * MenuItemHeight, Width, MenuItemHeight);
        }
        /// <summary>
        /// 切换激活栏目：withAnim=true 时播放"旧树下沉淡出 / 新树上浮淡入"动画。
        /// 栏目本身顺序不变，只有树区内容与标题/选中行变化；动画全程只平移整幅树位图，不缩放图标与字体。
        /// </summary>
        private void ActivateSection(int target, bool withAnim)
        {
            if (target < 0 || target >= hSections.Count || target == hActiveIndex) return;
            if (!withAnim || hCollapsed || hTree.Width <= 0 || hTree.Height <= 0)
            {
                if (hAnimating) FinishAnimation();
                hActiveIndex = target;
                HNavigatorSection sec = hSections[target];
                hTree.AttachNodes(sec.Nodes, sec.SelectedNode);
                PerformLayoutTree();
                Invalidate();
                ActiveSectionChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            if (hAnimating) FinishAnimation();
            // 切换前截取旧栏目树画面
            hBmpOld = CaptureTree();
            hActiveIndex = target;
            HNavigatorSection up = hSections[target];
            hTree.AttachNodes(up.Nodes, up.SelectedNode);
            hTree.Update();           // 强制同步完成新数据的一次绘制
            hBmpNew = CaptureTree();  // 切换后截取新栏目树画面
            hTree.Visible = false;
            hMenuHover = -1;
            hAnimating = true;
            hAnimStep = 0;
            hAnimTimer.Start();
            Invalidate();
            ActiveSectionChanged?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>把当前树控件画面原位截成位图（保持图标/字体原始大小）</summary>
        private Bitmap CaptureTree()
        {
            int w = hTree.Width;
            int h = hTree.Height;
            if (w <= 0 || h <= 0) return null;
            Bitmap bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            hTree.DrawToBitmap(bmp, new Rectangle(0, 0, w, h));
            return bmp;
        }
        private static float EaseInCubic(float t) { return t * t * t; }
        private static float EaseOutCubic(float t) { t -= 1f; return t * t * t + 1f; }
        private void AnimTimer_Tick(object sender, EventArgs e)
        {
            hAnimStep++;
            if (hAnimStep >= AnimSteps) { FinishAnimation(); return; }
            Invalidate(new Rectangle(0, HeaderHeight, Width, Height - HeaderHeight));
        }
        /// <summary>结束动画：恢复真实树控件，释放过渡位图</summary>
        private void FinishAnimation()
        {
            hAnimTimer.Stop();
            hAnimating = false;
            if (hBmpOld != null) { hBmpOld.Dispose(); hBmpOld = null; }
            if (hBmpNew != null) { hBmpNew.Dispose(); hBmpNew = null; }
            hTree.Visible = !hCollapsed;
            PerformLayoutTree();
            Invalidate();
        }
        /// <summary>绘制交叉过渡的两幅树位图（旧树下沉淡出，新树上浮淡入）</summary>
        private void DrawTransitionLayers(Graphics g)
        {
            if (hBmpOld == null && hBmpNew == null) return;
            float p = (float)hAnimStep / AnimSteps;
            Rectangle clip = new Rectangle(0, HeaderHeight, Width, Height - HeaderHeight);
            Region oldClip = g.Clip;
            g.SetClip(clip);
            int treeW = hTree.Width > 0 ? hTree.Width : Math.Max(0, Width - 2);
            int treeH = hTree.Height > 0 ? hTree.Height : clip.Height;
            int baseY = HeaderHeight;
            // 旧栏目：缓动下沉并淡出
            if (hBmpOld != null)
            {
                int dy = (int)Math.Round(AnimSlide * EaseInCubic(p));
                DrawImageAlpha(g, hBmpOld, new Rectangle(1, baseY + dy, treeW, treeH), 1f - p);
            }
            // 新栏目：从下方缓动上浮并淡入，底边带一点投影强化浮起层次
            if (hBmpNew != null)
            {
                int dy = (int)Math.Round(AnimSlide * (1f - EaseOutCubic(p)));
                Rectangle dst = new Rectangle(1, baseY + dy, treeW, treeH);
                DrawImageAlpha(g, hBmpNew, dst, p);
                using (Pen edge = new Pen(Color.FromArgb(70, 150, 158, 168), 1f))
                    g.DrawRectangle(edge, dst.X, dst.Y, dst.Width - 1, dst.Height - 1);
            }
            g.Clip = oldClip;
        }
        /// <summary>按指定整体不透明度绘制位图</summary>
        private static void DrawImageAlpha(Graphics g, Image img, Rectangle dst, float alpha)
        {
            if (img == null || dst.Width <= 0 || dst.Height <= 0 || alpha <= 0f) return;
            using (ImageAttributes ia = new ImageAttributes())
            {
                ColorMatrix cm = new ColorMatrix();
                cm.Matrix33 = Math.Min(1f, alpha);
                ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(img, dst, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
            }
        }
        #endregion
        #region 布局
        private int MenuAreaHeight
        {
            get { return hMenuItems.Count * MenuItemHeight; }
        }
        /// <summary>PerformLayoutTree 方法。</summary>
        private void PerformLayoutTree()
        {
            // 切换动画中树的画面由过渡位图负责，位置不能被布局复位
            if (hAnimating) return;
            int top = HeaderHeight;
            int bottomReserve = MenuAreaHeight + 2;
            int avail = Height - top - 1 - (hCollapsed ? 0 : SeparatorHeight) - bottomReserve;
            if (avail < 0) avail = 0;
            hTree.SetBounds(1, top, Math.Max(0, Width - 2), avail);
            hTree.Visible = !hCollapsed && avail > 0;
        }
        /// <summary>响应 Resize 事件。</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (hAnimating) FinishAnimation();   // 尺寸变化后旧位图失效，直接落到新栏目
            PerformLayoutTree();
            Invalidate();
        }
        /// <summary>
        /// 跨不同 DPI 显示器移动后，WinForms 会自动缩放内部树区控件，
        /// 其结果可能与本面板的菜单/分隔预留高度不一致，这里按最终尺寸重新布局一次。
        /// </summary>
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            if (hAnimating) FinishAnimation();
            PerformLayoutTree();
            Invalidate();
        }
        /// <summary>响应 FontChanged 事件。</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            hTree.Font = Font;
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
            DrawHeader(g);
            // 切换动画期间树位图经过分隔条区域，抓手圆点隐藏，避免错位
            if (!hCollapsed && !hAnimating) DrawSeparator(g);
            DrawMenu(g);
            // 动画过渡图层最后画（浮于菜单行之上），旧树下压淡出 / 新树上浮淡入
            if (hAnimating && !hCollapsed) DrawTransitionLayers(g);
            DrawBorder(g);
        }
        /// <summary>构造圆角矩形路径（标题栏图标底托用）</summary>
        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
        /// <summary>DrawHeader 方法：深蓝醒目标题栏 = 栏目图标白底托 + 白色加粗标题</summary>
        private void DrawHeader(Graphics g)
        {
            Rectangle header = new Rectangle(0, 0, Width, HeaderHeight);
            using (LinearGradientBrush gb = new LinearGradientBrush(header, hHeaderColor1, hHeaderColor2, LinearGradientMode.Vertical))
                g.FillRectangle(gb, header);
            using (Pen lp = new Pen(hHeaderLineColor, 1f))
                g.DrawLine(lp, 0, HeaderHeight - 1, Width - 1, HeaderHeight - 1);
            HNavigatorSection active = ActiveSection;
            // 栏目图标：白色圆角底托 + 彩色矢量图标（与底部行同款图标）
            if (active != null)
            {
                Rectangle badge = new Rectangle(6, 3, 20, 20);
                using (GraphicsPath bp = RoundedRect(badge, 5))
                using (SolidBrush bb = new SolidBrush(hHeaderBadgeColor))
                    g.FillPath(bb, bp);
                Rectangle iconRect = new Rectangle(badge.X + 2, badge.Y + 2, 16, 16);
                HGlyph.DrawColor(g, active.Icon, iconRect);
            }
            // 标题（白色加粗）：未显式设置 Title 时显示当前栏目名
            string headerText = !string.IsNullOrEmpty(hTitle) ? hTitle
                : (active != null ? active.Text : "");
            int textLeft = active != null ? 32 : 8;
            using (Font titleFont = new Font(Font.FontFamily, Font.Size + 1.5f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, headerText, titleFont, new Rectangle(textLeft, 0, Width - textLeft - 8, HeaderHeight),
                    hTitleColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
        }
        /// <summary>DrawSeparator 方法。</summary>
        private void DrawSeparator(Graphics g)
        {
            int y = hTree.Bottom + SeparatorHeight / 2 + 1;
            using (SolidBrush db = new SolidBrush(hSeparatorDotColor))
            {
                // 中央 6 个圆点（WinCC 风格抓手）
                int dotR = 2;
                int gap = 7;
                int total = 6 * dotR * 2 + 5 * gap;
                int startX = Width / 2 - total / 2;
                for (int i = 0; i < 6; i++)
                {
                    int cx = startX + i * (dotR * 2 + gap) + dotR;
                    g.FillEllipse(db, cx - dotR, y - dotR, dotR * 2, dotR * 2);
                }
            }
        }
        /// <summary>DrawMenu 方法。</summary>
        private void DrawMenu(Graphics g)
        {
            int top = Height - 1 - MenuAreaHeight;
            if (top < HeaderHeight) top = HeaderHeight;
            for (int i = 0; i < hMenuItems.Count; i++)
            {
                Rectangle row = new Rectangle(0, top + i * MenuItemHeight, Width, MenuItemHeight);
                if (row.Y > Height) break;
                HSideMenuItem it = hMenuItems[i];
                bool selected = (i == hActiveIndex);
                bool hovered = (i == hMenuHover);
                if (selected)
                {
                    using (LinearGradientBrush gb = new LinearGradientBrush(
                        new Rectangle(row.X, row.Y, row.Width, Math.Max(1, row.Height)),
                        hMenuSelColor1, hMenuSelColor2, LinearGradientMode.Vertical))
                        g.FillRectangle(gb, row);
                    using (Pen bp = new Pen(hMenuSelBorderColor, 1f))
                        g.DrawRectangle(bp, row.X, row.Y, row.Width - 1, row.Height - 1);
                }
                else if (hovered)
                {
                    using (LinearGradientBrush hb = new LinearGradientBrush(
                        new Rectangle(row.X, row.Y, row.Width, Math.Max(1, row.Height)),
                        hMenuHoverColor1, hMenuHoverColor2, LinearGradientMode.Vertical))
                        g.FillRectangle(hb, new Rectangle(row.X + 1, row.Y + 1, row.Width - 2, row.Height - 2));
                }
                else
                {
                    using (SolidBrush bb = new SolidBrush(hMenuBackColor))
                        g.FillRectangle(bb, new Rectangle(row.X + 1, row.Y + 1, row.Width - 2, row.Height - 2));
                }
                // 分隔线
                using (Pen lp = new Pen(hMenuLineColor, 1f))
                    g.DrawLine(lp, row.X + 1, row.Bottom - 1, row.Right - 2, row.Bottom - 1);
                // 彩色图标 + 文字（当前激活行加粗，与顶部醒目标题呼应）
                Rectangle iconRect = new Rectangle(11, row.Y + (row.Height - 22) / 2, 22, 22);
                HGlyph.DrawColor(g, it.Icon, iconRect);
                Rectangle textRect = new Rectangle(iconRect.Right + 10, row.Y, row.Width - iconRect.Right - 16, row.Height);
                if (selected)
                {
                    using (Font boldFont = new Font(Font, FontStyle.Bold))
                        TextRenderer.DrawText(g, it.Text, boldFont, textRect,
                            hMenuTextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }
                else
                {
                    TextRenderer.DrawText(g, it.Text, Font, textRect,
                        hMenuTextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }
            }
        }
        /// <summary>DrawBorder 方法。</summary>
        private void DrawBorder(Graphics g)
        {
            using (Pen bp = new Pen(hBorderColor, 1f))
                g.DrawRectangle(bp, 0, 0, Width - 1, Height - 1);
        }
        #endregion
        #region 交互
        /// <summary>HitMenuItem 方法。</summary>
        private int HitMenuItem(Point p)
        {
            int top = Height - 1 - MenuAreaHeight;
            if (top < HeaderHeight) top = HeaderHeight;
            if (p.X < 0 || p.X >= Width) return -1;
            int idx = (p.Y - top) / MenuItemHeight;
            if (idx < 0 || idx >= hMenuItems.Count) return -1;
            if (p.Y < top + idx * MenuItemHeight) return -1;
            return idx;
        }
        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (hAnimating) return;   // 切换动画中冻结悬停高亮
            int mh = hCollapsed ? -1 : HitMenuItem(e.Location);
            if (mh != hMenuHover) { hMenuHover = mh; Invalidate(); }
        }
        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hMenuHover != -1) { hMenuHover = -1; Invalidate(); }
        }
        /// <summary>响应 MouseUp 事件：点击固定栏目行即切换树区内容（栏目顺序不变）</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left || hAnimating || hCollapsed) return;
            int idx = HitMenuItem(e.Location);
            if (idx >= 0 && idx != hActiveIndex)
            {
                // 按固定索引通知外部，然后播放"新栏树上浮 / 旧栏树下沉"过渡动画
                MenuItemClick?.Invoke(this, idx);
                ActivateSection(idx, true);
            }
        }
        #endregion
    }
    /// <summary>
    /// HNavigator 栏目：固定顺序的底部栏目行（图标+文字）+ 该栏目自己的树节点集合。
    /// 所有栏目始终保留在底部固定行位置；当前激活栏目同时显示在上方树区并在行上高亮。
    /// </summary>
    public class HNavigatorSection
    {
        /// <summary>栏目名（标题栏/底部行文字）</summary>
        public string Text { get; set; }
        /// <summary>栏目图标</summary>
        public HGlyphKind Icon { get; set; }
        /// <summary>用户自定义数据</summary>
        public object Tag { get; set; }
        /// <summary>该栏目的树节点集合（栏目位于上方树区时显示）</summary>
        public List<HNavigatorNode> Nodes { get; } = new List<HNavigatorNode>();
        /// <summary>该栏目上次在树区中的选中节点（互换回来时恢复高亮）</summary>
        internal HNavigatorNode SelectedNode;
        /// <summary>底部栏目行镜像</summary>
        internal readonly HSideMenuItem Item;
        /// <summary>构造</summary>
        public HNavigatorSection(string text, HGlyphKind icon)
        {
            Text = text;
            Icon = icon;
            Item = new HSideMenuItem(text, icon);
        }
    }
}
