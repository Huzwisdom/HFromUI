using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace HFromUI.HControl.Chart.Menu
{
    /// <summary>
    /// 菜单灵活渲染器：按 HMenuSkinStyle（几何）+ HMenuPalette（颜色）自绘
    /// 下拉底色/图标栏/边框/悬停块/勾选记号/箭头/分隔线，支撑 20 样式 × 12 配色的全部组合。
    /// </summary>
    public sealed class HMenuFlexRenderer : ToolStripProfessionalRenderer
    {
        /// <summary>几何样式参数。</summary>
        private readonly HMenuSkinStyle _st;
        /// <summary>颜色配色板。</summary>
        private readonly HMenuPalette _p;
        /// <summary>左侧图标栏矩形（绘制图标栏背景时记录，供图标水平居中使用）。</summary>
        private Rectangle _imageMarginBox = Rectangle.Empty;
        /// <summary>当前正被鼠标按住的菜单项（下拉项的 Pressed 属性在按住期间不可靠，自行跟踪按下态）。</summary>
        private readonly System.Collections.Generic.HashSet<ToolStripItem> _pressedItems =
            new System.Collections.Generic.HashSet<ToolStripItem>();
        /// <summary>已接过鼠标事件的菜单项（每项只接一次）。</summary>
        private readonly System.Collections.Generic.HashSet<ToolStripItem> _wiredItems =
            new System.Collections.Generic.HashSet<ToolStripItem>();
        /// <summary>已接过 Closed 事件的下拉（菜单关闭时清空按下集合）。</summary>
        private readonly System.Collections.Generic.HashSet<ToolStripDropDown> _wiredDrops =
            new System.Collections.Generic.HashSet<ToolStripDropDown>();

        /// <summary>按样式 + 配色构造渲染器。</summary>
        public HMenuFlexRenderer(HMenuSkin skin, HMenuScheme scheme)
            : base(new HMenuFlexColors(HMenuPalettes.Get(scheme)))
        {
            _st = HMenuSkins.Get(skin);
            _p = HMenuPalettes.Get(scheme);
            RoundedEdges = _st.Radius > 0;
            // 悬停块四边内缩：常规皮肤统一 1px（严格居中贴行）；窄悬停皮肤沿用其左 7 右 2 的几何
            HoverPadding = _st.NarrowHover ? new Padding(7, 1, 2, 1) : new Padding(1);
        }

        /// <summary>当前配色板（供菜单外部同步文字/图标用）。</summary>
        public HMenuPalette Palette => _p;
        /// <summary>当前几何样式。</summary>
        public HMenuSkinStyle Style => _st;

        /// <summary>
        /// 悬停/勾选高亮块相对菜单项四边的内缩间距（像素），可按需调整；
        /// 默认四边 1px——高亮块在整行内严格上下左右对称居中，圆角由皮肤 Radius 决定。
        /// </summary>
        public Padding HoverPadding { get; set; }

        /// <summary>
        /// 悬停/勾选高亮块专用圆角半径（像素）；-1 表示跟随皮肤外框 Radius。
        /// 菜单外框圆角可以较大，高亮块通常需要更小的圆角（贴图标一侧不显得圆弧过大）。
        /// </summary>
        public int HoverRadius { get; set; } = -1;

        // ============ 下拉整体背景 ============
        /// <summary>绘制下拉菜单底色：纯平/纵向渐变/玻璃高光/柔光/近黑/标题横带。</summary>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            if (!(e.ToolStrip is ToolStripDropDownMenu)) { base.OnRenderToolStripBackground(e); return; }
            // 下拉首次绘制时接 Closed：关闭即清空按下状态，避免残留深色块
            if (e.ToolStrip is ToolStripDropDown dd && _wiredDrops.Add(dd))
                dd.Closed += (s, ev) => _pressedItems.Clear();
            var g = e.Graphics;
            var r = e.AffectedBounds;
            using (var br = new SolidBrush(_p.Back)) g.FillRectangle(br, r);

            switch (_st.Drop)
            {
                case HMenuDrop.Gradient:
                    using (var br = new LinearGradientBrush(r, Mix(_p.Back, _p.TopLight, 0.25f), _p.Back, 90f))
                        g.FillRectangle(br, r);
                    break;
                case HMenuDrop.Glass:
                    using (var br = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, r.Height / 2),
                        Color.FromArgb(110, _p.TopLight), Color.FromArgb(0, _p.TopLight), 90f))
                        g.FillRectangle(br, new Rectangle(r.X, r.Y, r.Width, r.Height / 2));
                    break;
                case HMenuDrop.Neumorph:
                    using (var br = new SolidBrush(Color.FromArgb(60, _p.TopLight)))
                        g.FillRectangle(br, new Rectangle(r.X, r.Y, r.Width, r.Height / 2));
                    break;
                case HMenuDrop.Glow:
                    using (var br = new SolidBrush(Color.FromArgb(28, _p.Accent)))
                        g.FillRectangle(br, new Rectangle(r.X, r.Y, r.Width, 5));
                    break;
                case HMenuDrop.HeaderBar:
                    using (var br = new LinearGradientBrush(new Rectangle(r.X, r.Y, r.Width, 7),
                        _p.Accent, Mix(_p.Accent, _p.Back, 0.35f), 90f))
                        g.FillRectangle(br, new Rectangle(r.X, r.Y, r.Width, 7));
                    break;
            }
        }

        /// <summary>左侧图标栏：整体铺图标栏底色；标题栏样式铺强调色渐变。</summary>
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            var g = e.Graphics;
            var r = e.AffectedBounds;
            // 记录本下拉的图标栏矩形（同一渲染器的各下拉度量一致），图标按它居中
            _imageMarginBox = r;
            if (_st.Drop == HMenuDrop.HeaderBar)
            {
                using (var br = new LinearGradientBrush(r, Mix(_p.Accent, _p.Back, 0.15f),
                    Mix(_p.Accent, _p.Margin, 0.25f), 90f))
                    g.FillRectangle(br, r);
            }
            else
            {
                using (var br = new SolidBrush(_p.Margin)) g.FillRectangle(br, r);
            }
            // 图标栏与文字区的分界细线
            using (var pen = new Pen(_p.Border, 1f))
                g.DrawLine(pen, r.Right - 1, r.Top, r.Right - 1, r.Bottom);
        }

        /// <summary>
        /// 以项图标的实际尺寸在图标栏正中求目标矩形；勾选项绘制时同样按图标尺寸外扩 1px 作方框。
        /// 注意：图标栏矩形是弹层坐标，而绘制图形上下文是菜单项相对坐标，水平方向要减去项 Bounds.X。
        /// </summary>
        private Rectangle CenteredImageRect(ToolStripItemImageRenderEventArgs e)
        {
            int w = e.Item.Image?.Width ?? e.ImageRectangle.Width;
            int h = e.Item.Image?.Height ?? e.ImageRectangle.Height;
            return new Rectangle(
                _imageMarginBox.X + (_imageMarginBox.Width - w) / 2 - e.Item.Bounds.X,
                e.ImageRectangle.Y + (e.ImageRectangle.Height - h) / 2, w, h);
        }

        /// <summary>菜单项图标：水平居中于左侧图标栏、垂直居中于整行（默认绘制受自定义 Padding 影响会贴左）。</summary>
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Image == null || _imageMarginBox.Width <= 0) { base.OnRenderItemImage(e); return; }

            // 垂直沿用布局已居中的 Y，只把水平位置改为图标栏正中
            var dst = CenteredImageRect(e);

            if (e.Item.Enabled)
            {
                e.Graphics.DrawImage(e.Image, dst);
                return;
            }

            // 禁用项：灰度 + 半透明，观感与禁用文字一致
            var cm = new ColorMatrix(new float[][]
            {
                new float[] { 0.299f, 0.299f, 0.299f, 0f, 0f },
                new float[] { 0.587f, 0.587f, 0.587f, 0f, 0f },
                new float[] { 0.114f, 0.114f, 0.114f, 0f, 0f },
                new float[] { 0f, 0f, 0f, 0.45f, 0f },
                new float[] { 0f, 0f, 0f, 0f, 1f },
            });
            using (var ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm);
                e.Graphics.DrawImage(e.Image, dst, 0, 0, e.Image.Width, e.Image.Height, GraphicsUnit.Pixel, ia);
            }
        }

        /// <summary>菜单外边框：无边框样式跳过；霓虹画发光边；新拟态画亮/暗双边；其余圆角/直角单色边。</summary>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (!_st.Border) return;
            var g = e.Graphics;
            var r = e.AffectedBounds;
            r.Width -= 1; r.Height -= 1;
            if (_st.Hover == HMenuHover.Neumorph)
            {
                using (var pl = new Pen(_p.TopLight, 1f))
                using (var pd = new Pen(_p.EdgeDark, 1f))
                {
                    g.DrawLine(pl, r.X + 2, r.Y, r.Right - 2, r.Y);
                    g.DrawLine(pl, r.X, r.Y + 2, r.X, r.Bottom - 2);
                    g.DrawLine(pd, r.X + 2, r.Bottom, r.Right - 2, r.Bottom);
                    g.DrawLine(pd, r.Right, r.Y + 2, r.Right, r.Bottom - 2);
                }
                return;
            }
            Color edge = _st.Drop == HMenuDrop.Glow ? Color.FromArgb(160, _p.Accent) : _p.Border;
            using (var pen = new Pen(edge, 1f))
            {
                if (_st.Radius <= 0) g.DrawRectangle(pen, r);
                else using (var path = RoundPath(r, _st.Radius)) g.DrawPath(pen, path);
            }
            // 玻璃/Aero 顶部补一条高光
            if (_st.Drop == HMenuDrop.Glass)
                using (var pen = new Pen(Color.FromArgb(150, _p.TopLight), 1f))
                    g.DrawLine(pen, r.X + _st.Radius, r.Y + 1, r.Right - _st.Radius, r.Y + 1);
        }

        /// <summary>勾选项方框：默认绘制会越过图标栏分隔线，这里按居中后的图标矩形重画（蓝色圆角细框，与原观感一致）。</summary>
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            if (_imageMarginBox.Width <= 0) { base.OnRenderItemCheck(e); return; }

            var box = CenteredImageRect(e);
            box.Inflate(1, 1);
            using (var path = RoundPath(box, 2))
            {
                using (var br = new SolidBrush(_p.Margin)) e.Graphics.FillPath(br, path);
                using (var pen = new Pen(Color.FromArgb(0, 120, 212), 1f)) e.Graphics.DrawPath(pen, path);
            }
        }

        // ============ 菜单项背景（悬停/勾选/按下） ============
        /// <summary>为单个菜单项接一次鼠标按下/抬起/移出事件，维护自定义按下态（按住时背景加深）。</summary>
        private void WirePress(ToolStripItem item)
        {
            if (!_wiredItems.Add(item)) return;
            item.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                _pressedItems.Add(item);
                item.Invalidate();
            };
            item.MouseUp += (s, e) =>
            {
                if (_pressedItems.Remove(item)) item.Invalidate();
            };
            item.MouseLeave += (s, e) => _pressedItems.Remove(item);
        }

        /// <summary>悬停项与勾选项按样式画块：纯块/渐变/左竖条/描边/药丸/下划线/磁贴/斜面/柔光；鼠标按住时再加深一档。</summary>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var item = e.Item as ToolStripMenuItem;
            if (item == null || !item.Enabled) return;
            WirePress(item);
            bool hot = item.Selected;
            bool chk = item.Checked && !hot;
            if (!hot && !chk) return;
            DrawHover(e.Graphics, new Rectangle(Point.Empty, item.Size), hot, _pressedItems.Contains(item));
        }

        /// <summary>按悬停画法在指定 bounds（项相对坐标，原点为项左上角）内画高亮块（勾选/按下态复用同画法）。</summary>
        private void DrawHover(Graphics g, Rectangle bounds, bool hot, bool press)
        {
            // 四边严格按 HoverPadding 内缩（默认四边 1px），高亮块在整行内完全对称居中
            var hp = HoverPadding;
            var r = Rectangle.FromLTRB(hp.Left, hp.Top, bounds.Width - hp.Right, bounds.Height - hp.Bottom);
            // 高亮块圆角：HoverRadius 指定时用专用小圆角，否则跟随皮肤外框 Radius
            int hr = HoverRadius >= 0 ? HoverRadius : _st.Radius;

            Color ca = hot ? _p.HoverStart : _p.Checked;
            Color cb = hot ? _p.HoverEnd : _p.Checked;
            // 按下（鼠标按住未松手）：悬停色向强调色混深 35%，给出明显的"按下去"反馈
            if (press) { ca = Mix(ca, _p.Accent, 0.35f); cb = Mix(cb, _p.Accent, 0.35f); }

            switch (_st.Hover)
            {
                case HMenuHover.Solid:
                    FillR(g, r, hr, ca, null);
                    break;
                case HMenuHover.Gradient:
                    using (var br = new LinearGradientBrush(r, ca, cb, 90f))
                        FillR(g, r, hr, null, br);
                    break;
                case HMenuHover.LeftBar:
                    FillR(g, r, 3, ca, null);
                    using (var br = new SolidBrush(_p.Accent))
                        FillR(g, new Rectangle(r.X + 1, r.Y + 1, 3, r.Height - 2), 1, _p.Accent, null);
                    break;
                case HMenuHover.Outline:
                    if (_st.Drop == HMenuDrop.Glow)
                    {
                        // 霓虹发光：三层由粗到细、由淡到浓
                        using (var p3 = new Pen(Color.FromArgb(28, _p.Accent), 4f))
                        using (var p2 = new Pen(Color.FromArgb(60, _p.Accent), 2.4f))
                        using (var p1 = new Pen(Color.FromArgb(220, _p.Accent), 1.2f))
                        using (var path = RoundPath(r, hr))
                        { g.DrawPath(p3, path); g.DrawPath(p2, path); g.DrawPath(p1, path); }
                    }
                    else
                    {
                        using (var br = new SolidBrush(Color.FromArgb(26, _p.Accent)))
                            FillR(g, r, hr, Color.FromArgb(26, _p.Accent), null);
                        using (var pen = new Pen(Color.FromArgb(hot ? 220 : 140, _p.Accent), 1.2f))
                        using (var path = RoundPath(r, hr))
                            g.DrawPath(pen, path);
                    }
                    break;
                case HMenuHover.Pill:
                    FillR(g, r, r.Height / 2, ca, null);
                    break;
                case HMenuHover.Underline:
                    using (var br = new SolidBrush(hot ? Color.FromArgb(36, _p.Accent) : _p.Checked))
                        g.FillRectangle(br, r);
                    using (var br = new SolidBrush(_p.Accent))
                        g.FillRectangle(br, new Rectangle(r.X + 2, r.Bottom - 2, r.Width - 4, hot ? 3 : 2));
                    break;
                case HMenuHover.Tile:
                    FillR(g, r, hr, _p.Accent, null);
                    break;
                case HMenuHover.Bevel:
                    using (var br = new LinearGradientBrush(r, Mix(ca, _p.TopLight, 0.35f),
                        Mix(cb, _p.EdgeDark, 0.2f), 90f))
                        FillR(g, r, hr, null, br);
                    using (var pl = new Pen(Color.FromArgb(170, _p.TopLight), 1f))
                    using (var pd = new Pen(Color.FromArgb(150, _p.EdgeDark), 1f))
                    {
                        g.DrawLine(pl, r.X + 2, r.Y + 1, r.Right - 2, r.Y + 1);
                        g.DrawLine(pd, r.X + 2, r.Bottom, r.Right - 2, r.Bottom);
                    }
                    break;
                case HMenuHover.Neumorph:
                    FillR(g, r, hr, _p.Back, null);
                    using (var pl = new Pen(Color.FromArgb(120, _p.TopLight), 1.2f))
                    using (var pd = new Pen(Color.FromArgb(120, _p.EdgeDark), 1.2f))
                    using (var path = RoundPath(r, hr))
                    {
                        g.DrawLine(pl, r.X + 3, r.Y + 1, r.Right - 3, r.Y + 1);
                        g.DrawLine(pd, r.X + 3, r.Bottom, r.Right - 3, r.Bottom);
                    }
                    break;
            }
        }

        // ============ 文字 / 勾选 / 箭头 ============
        /// <summary>文字着色：禁用灰、磁贴悬停白、其余正文色；随后走默认排版（含助记符/省略号）。</summary>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            var item = e.Item as ToolStripMenuItem;
            var g = e.Graphics;
            if (item != null && !item.Enabled)
                e.TextColor = _p.TextDisabled;
            else if (item != null && item.Selected && (_st.Hover == HMenuHover.Tile))
                e.TextColor = _p.HoverText;
            else
                e.TextColor = _p.Text;
            base.OnRenderItemText(e);

            // 勾选记号画在右侧（ShowCheckMargin=false，默认勾选符不出现）
            if (item != null && item.Checked && _st.Check != HMenuCheck.None)
            {
                var box = new Rectangle(item.Width - 24, 0, 18, item.Height);
                Color mark = item.Selected && _st.Hover == HMenuHover.Tile ? _p.HoverText : _p.Accent;
                if (_st.Check == HMenuCheck.Dot)
                {
                    using (var br = new SolidBrush(mark))
                        g.FillEllipse(br, box.X + 6, box.Y + box.Height / 2 - 3, 6, 6);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, "✓", e.TextFont, box, mark,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
                }
            }
        }

        /// <summary>父菜单右侧小箭头：统一用强调色实心三角。</summary>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = e.Item.Enabled
                ? (e.Item.Selected && _st.Hover == HMenuHover.Tile ? _p.HoverText : _p.Accent)
                : _p.TextDisabled;
            base.OnRenderArrow(e);
        }

        // ============ 分隔线 ============
        /// <summary>分隔线：单线/左右内缩/立体渐变/无。</summary>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            if (_st.Separator == HMenuSep.None) return;
            var g = e.Graphics;
            int y = e.Item.Height / 2;
            int x1 = 2, x2 = e.Item.Width - 3;
            if (_st.Separator == HMenuSep.Inset) { x1 += 20; x2 -= 8; }
            if (_st.Separator == HMenuSep.Gradient)
            {
                using (var pd = new Pen(_p.EdgeDark, 1f)) g.DrawLine(pd, x1, y, x2, y);
                using (var pl = new Pen(_p.TopLight, 1f)) g.DrawLine(pl, x1, y + 1, x2, y + 1);
                return;
            }
            using (var pen = new Pen(_p.Separator, 1f)) g.DrawLine(pen, x1, y, x2, y);
        }

        // ============ 共享工具 ============
        /// <summary>圆角路径。</summary>
        internal static GraphicsPath RoundPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (radius <= 0 || d > r.Width || d > r.Height) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>填充圆角矩形（实心画刷或渐变画刷二选一）。</summary>
        private static void FillR(Graphics g, Rectangle r, int radius, Color? solid, Brush custom)
        {
            using (var path = RoundPath(r, radius))
            using (var br = custom ?? new SolidBrush(solid.Value))
                g.FillPath(br, path);
        }

        /// <summary>两色按比例混合（t=0 返回 a）。</summary>
        internal static Color Mix(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
    }

    /// <summary>配色表：把配色板颜色接到 ToolStripProfessionalRenderer 的基类属性，避免任何系统色漏出。</summary>
    internal sealed class HMenuFlexColors : ProfessionalColorTable
    {
        private readonly HMenuPalette _p;
        public HMenuFlexColors(HMenuPalette p) { _p = p; }

        public override Color ToolStripDropDownBackground => _p.Back;
        public override Color ImageMarginGradientBegin => _p.Margin;
        public override Color ImageMarginGradientMiddle => _p.Margin;
        public override Color ImageMarginGradientEnd => _p.Margin;
        public override Color MenuBorder => _p.Border;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => _p.HoverStart;
        public override Color MenuItemSelectedGradientBegin => _p.HoverStart;
        public override Color MenuItemSelectedGradientEnd => _p.HoverEnd;
        public override Color SeparatorDark => _p.Separator;
        public override Color SeparatorLight => _p.Separator;
    }
}
