using System.Drawing;
using System.Windows.Forms;

namespace HFromUI.HControl.Chart{
    using HFromUI.HLangage;
    using HFromUI.HControl.Chart.Menu;
    using HFromUI.HControl.Chart.Tips;
    /// <summary>HChart 分部类：右键菜单实现（提取至 HFrom\Chart\Menu）。</summary>
    public partial class HChart
    {
        /// <summary>
        /// HChart 内置右键菜单：框选/自适应/实时/标尺/系列/配色主题/图表类型/滚轮/保存/图例。
        /// 菜单样式固定圆角浅色，不提供外观切换；菜单图片懒加载（弹出/展开时才取图）。
        /// 作为 HChart 嵌套类可直接访问图表私有状态，构建与弹出同步逻辑整体从 HChart.cs 迁入。
        /// </summary>
        internal sealed class HChartMenu : ContextMenuStrip
        {
            /// <summary>所属图表。</summary>
            private readonly HChart _c;
            /// <summary>菜单固定几何样式（圆角）。</summary>
            private const HMenuSkin Skin = HMenuSkin.Rounded;
            /// <summary>菜单固定配色风格（浅色）。</summary>
            private const HMenuScheme Scheme = HMenuScheme.Light;
            /// <summary>9 张主题预览图是否已按需生成（首次展开"配色主题"时才画）。</summary>
            private bool _themeTiled;

            // ----- 功能菜单项引用（勾选/启用/文字在 Opening 同步） -----
            private ToolStripMenuItem _miRubber, _miFit, _miLive, _miLiveShowAll, _miRulerV, _miRulerH,
                _miClearRuler, _miShowAll, _miSave, _miLegend,
                _miWheel, _miWheelEnable, _miWheelBoth, _miWheelX, _miWheelY,
                _miRestore, _miChartType, miTheme,
                _miTypeLine, _miTypeSpline, _miTypeStep, _miTypeArea, _miTypeColumn, _miTypeBar,
                _miTypePoint, _miTypePie, _miTypeStackedArea, _miTypeStackedColumn, _miTypeRadar, _miTypeCandlestick,
                _miThLight, _miThDark, _miThSunset, _miThSand, _miThMatrix, _miThPaper,
                _miThAqua, _miThGrass, _miThDesert;

            /// <summary>构造：为指定图表构建整套右键菜单并接上图表（不预生成任何图片）。</summary>
            public HChartMenu(HChart chart)
            {
                _c = chart;

                ShowImageMargin = true;
                ShowCheckMargin = false;
                BuildItems();
                InitLook();
                Opening += Menu_Opening;
                _c.ContextMenuStrip = this;
            }

            /// <summary>当前配色板（取图着色用，固定浅色）。</summary>
            private HMenuPalette Pal => HMenuPalettes.Get(Scheme);

            /// <summary>按当前配色取菜单矢量图。</summary>
            private Bitmap Ico(HMenuGlyph g) => HMenuIcon.Get(g, Pal.GlyphInk, Pal.GlyphAccent);

            // ============ 菜单构建 ============
            /// <summary>构建全部菜单项（文字/排序/功能与提取前一致，图标改走 HMenuIcon 矢量库）。</summary>
            private void BuildItems()
            {
                _miRubber = new ToolStripMenuItem(HTranslation.GetContent("框选放大")) { Tag = HMenuGlyph.Rubber };
                _miRubber.Click += (s, e) => ToggleMode(InteractMode.RubberBand);
                _miFit = new ToolStripMenuItem(HTranslation.GetContent("自适应")) { Tag = HMenuGlyph.Fit };
                _miFit.Click += (s, e) => ResetZoom();
                _miLive = new ToolStripMenuItem(HTranslation.GetContent("继续实时刷新")) { Tag = HMenuGlyph.Play };
                _miLive.Click += (s, e) => { if (_c.IsLivePaused) _c.ResumeLive(); else _c.PauseLive(); };
                _miLiveShowAll = new ToolStripMenuItem(HTranslation.GetContent("显示所有数据")) { Tag = HMenuGlyph.VGridDots };
                _miLiveShowAll.Click += (s, e) => _c.ToggleLiveShowAll();
                _miRulerV = new ToolStripMenuItem(HTranslation.GetContent("横坐标测量")) { Tag = HMenuGlyph.RulerV };
                _miRulerV.Click += (s, e) => ToggleMode(InteractMode.RulerV);
                _miRulerH = new ToolStripMenuItem(HTranslation.GetContent("纵坐标测量")) { Tag = HMenuGlyph.RulerH };
                _miRulerH.Click += (s, e) => ToggleMode(InteractMode.RulerH);
                _miClearRuler = new ToolStripMenuItem(HTranslation.GetContent("清除标尺线")) { Tag = HMenuGlyph.Clear };
                _miClearRuler.Click += (s, e) => ClearRulers();
                _miShowAll = new ToolStripMenuItem(HTranslation.GetContent("显示所有系列")) { Tag = HMenuGlyph.Eye };
                _miShowAll.Click += (s, e) => ShowAllData();
                _miSave = new ToolStripMenuItem(HTranslation.GetContent("保存图片…")) { Tag = HMenuGlyph.Save };
                _miSave.Click += (s, e) => SaveChartImage();
                _miLegend = new ToolStripMenuItem(HTranslation.GetContent("显示图例")) { Tag = HMenuGlyph.Legend };
                _miLegend.Click += (s, e) =>
                {
                    if (_c.Legends.Count > 0) _c.Legends[0].IsEnabled = !_c.Legends[0].IsEnabled;
                    _c.Invalidate();
                };

                _miWheel = new ToolStripMenuItem(HTranslation.GetContent("滚轮缩放")) { Tag = HMenuGlyph.Wheel };
                _miWheelEnable = new ToolStripMenuItem(HTranslation.GetContent("启用滚轮缩放")) { Tag = HMenuGlyph.VToggleOn };
                _miWheelEnable.Click += (s, e) => _c.EnableZoom = !_c.EnableZoom;
                _miWheelBoth = new ToolStripMenuItem(HTranslation.GetContent("同时缩放 X / Y")) { Tag = HMenuGlyph.VZoomXY };
                _miWheelBoth.Click += (s, e) => _c.WheelZoomMode = HChartWheelZoomMode.Both;
                _miWheelX = new ToolStripMenuItem(HTranslation.GetContent("只缩放 X 轴")) { Tag = HMenuGlyph.AxisX };
                _miWheelX.Click += (s, e) => _c.WheelZoomMode = HChartWheelZoomMode.XOnly;
                _miWheelY = new ToolStripMenuItem(HTranslation.GetContent("只缩放 Y 轴")) { Tag = HMenuGlyph.AxisY };
                _miWheelY.Click += (s, e) => _c.WheelZoomMode = HChartWheelZoomMode.YOnly;
                _miWheel.DropDownItems.AddRange(new ToolStripItem[]
                {
                    _miWheelEnable, new ToolStripSeparator(), _miWheelBoth, _miWheelX, _miWheelY,
                });

                // 配色主题：9 种（mini 坐标系预览图首次展开时才按需生成，固定主题色，不随菜单配色变）
                miTheme = new ToolStripMenuItem(HTranslation.GetContent("配色主题")) { Tag = HMenuGlyph.Palette };
                _miThLight = ThemeItem(HTranslation.GetContent("默认 浅色"), HChartTheme.Light);
                _miThDark = ThemeItem(HTranslation.GetContent("深色 仪表风"), HChartTheme.Dark);
                _miThSunset = ThemeItem(HTranslation.GetContent("夕橙 暖橙白"), HChartTheme.Sunset);
                _miThSand = ThemeItem(HTranslation.GetContent("护眼 米黄底"), HChartTheme.Sand);
                _miThMatrix = ThemeItem(HTranslation.GetContent("矩阵 纯黑荧光"), HChartTheme.Matrix);
                _miThPaper = ThemeItem(HTranslation.GetContent("纸张 米纸墨蓝"), HChartTheme.Paper);
                _miThAqua = ThemeItem(HTranslation.GetContent("晴海 浅冰蓝"), HChartTheme.Aqua);
                _miThGrass = ThemeItem(HTranslation.GetContent("草绿 清新"), HChartTheme.Grass);
                _miThDesert = ThemeItem(HTranslation.GetContent("沙漠 暖橙棕"), HChartTheme.Desert);
                // 预览图参数登记（图本身在首次展开主题菜单时才生成）
                _themeSpecs = new System.Collections.Generic.List<ThemeTileSpec>(9)
                {
                    new ThemeTileSpec(_miThLight,  Color.White,                    Color.FromArgb(120,120,130), Color.FromArgb(230,230,235), HMenuGlyph.VSun2),
                    new ThemeTileSpec(_miThDark,   Color.FromArgb(42,44,48),       Color.FromArgb(180,180,190), Color.FromArgb(60,62,68),    HMenuGlyph.VMoon2),
                    new ThemeTileSpec(_miThSunset, Color.FromArgb(255,248,240),   Color.FromArgb(200,110,50),  Color.FromArgb(245,220,198), HMenuGlyph.VSunset),
                    new ThemeTileSpec(_miThSand,   Color.FromArgb(250,244,228),   Color.FromArgb(150,135,110), Color.FromArgb(225,215,195), HMenuGlyph.VDune),
                    new ThemeTileSpec(_miThMatrix, Color.FromArgb(4,6,4),         Color.FromArgb(60,180,80),   Color.FromArgb(20,60,30),    HMenuGlyph.VMatrix2),
                    new ThemeTileSpec(_miThPaper,  Color.FromArgb(253,248,235),   Color.FromArgb(80,95,130),   Color.FromArgb(230,222,200), HMenuGlyph.VScroll),
                    new ThemeTileSpec(_miThAqua,   Color.FromArgb(240,248,252),   Color.FromArgb(50,120,165),  Color.FromArgb(210,228,240), HMenuGlyph.VDroplet),
                    new ThemeTileSpec(_miThGrass,  Color.FromArgb(235,248,235),   Color.FromArgb(80,130,95),   Color.FromArgb(210,235,210), HMenuGlyph.VLeaf),
                    new ThemeTileSpec(_miThDesert, Color.FromArgb(248,232,200),   Color.FromArgb(160,100,55),  Color.FromArgb(225,205,170), HMenuGlyph.VCactus),
                };
                miTheme.DropDownItems.AddRange(new ToolStripItem[]
                {
                    _miThLight, _miThDark, _miThSunset, _miThSand, _miThMatrix,
                    _miThPaper, _miThAqua, _miThGrass, _miThDesert,
                });

                // 图表类型
                _miChartType = new ToolStripMenuItem(HTranslation.GetContent("图表类型")) { Tag = HMenuGlyph.ChartType };
                _miRestore = new ToolStripMenuItem(HTranslation.GetContent("还原原图")) { Tag = HMenuGlyph.Restore };
                _miRestore.Click += (s, e) => RestoreOriginalTypes();
                _miTypeLine = TypeItem(HTranslation.GetContent("折线图"), HChartType.Line, HMenuGlyph.TypeLine);
                _miTypeSpline = TypeItem(HTranslation.GetContent("平滑曲线"), HChartType.Spline, HMenuGlyph.TypeSpline);
                _miTypeStep = TypeItem(HTranslation.GetContent("阶梯线"), HChartType.StepLine, HMenuGlyph.TypeStep);
                _miTypeArea = TypeItem(HTranslation.GetContent("面积图"), HChartType.Area, HMenuGlyph.TypeArea);
                _miTypeColumn = TypeItem(HTranslation.GetContent("柱状图"), HChartType.Column, HMenuGlyph.TypeColumn);
                _miTypeBar = TypeItem(HTranslation.GetContent("条形图"), HChartType.Bar, HMenuGlyph.TypeBar);
                _miTypePoint = TypeItem(HTranslation.GetContent("散点图"), HChartType.Point, HMenuGlyph.TypePoint);
                _miTypePie = TypeItem(HTranslation.GetContent("饼图"), HChartType.Pie, HMenuGlyph.TypePie);
                _miTypeStackedArea = TypeItem(HTranslation.GetContent("堆叠面积图"), HChartType.StackedArea, HMenuGlyph.TypeStackedArea);
                _miTypeStackedColumn = TypeItem(HTranslation.GetContent("堆叠柱状图"), HChartType.StackedColumn, HMenuGlyph.TypeStackedColumn);
                _miTypeRadar = TypeItem(HTranslation.GetContent("雷达图"), HChartType.Radar, HMenuGlyph.TypeRadar);
                _miTypeCandlestick = TypeItem(HTranslation.GetContent("K线蜡烛图"), HChartType.Candlestick, HMenuGlyph.TypeCandlestick);
                _miChartType.DropDownItems.AddRange(new ToolStripItem[]
                {
                    _miRestore, new ToolStripSeparator(),
                    _miTypeLine, _miTypeSpline, _miTypeStep,
                    _miTypeArea, _miTypeColumn, _miTypeBar,
                    _miTypePoint, _miTypePie, _miTypeStackedArea, _miTypeStackedColumn,
                    _miTypeRadar, _miTypeCandlestick,
                });

                // 子菜单图片懒加载：各下拉首次展开时才给本层项取图（HMenuIcon 内部字典缓存，用到才添加）
                _miWheel.DropDownOpening += (s, e) => EnsureLayer(_miWheel.DropDownItems);
                miTheme.DropDownOpening += (s, e) => EnsureThemeTiles();
                _miChartType.DropDownOpening += (s, e) => EnsureLayer(_miChartType.DropDownItems);

                Items.AddRange(new ToolStripItem[]
                {
                    _miRubber, _miFit, _miLive, _miLiveShowAll,
                    new ToolStripSeparator(),
                    _miRulerV, _miRulerH, _miClearRuler,
                    new ToolStripSeparator(),
                    _miShowAll, miTheme, _miChartType,
                    new ToolStripSeparator(),
                    _miSave, _miLegend, _miWheel,
                });
            }

            /// <summary>图表类型菜单项简写（点击一键切类型，glyph 随菜单配色着色）。</summary>
            private ToolStripMenuItem TypeItem(string text, HChartType type, HMenuGlyph glyph)
            {
                var mi = new ToolStripMenuItem(text) { Tag = glyph };
                mi.Click += (s, e) => ApplyChartStyle(type);
                return mi;
            }

            /// <summary>配色主题菜单项：预览图延迟到首次展开主题菜单时才生成，这里只挂点击切主题。</summary>
            private ToolStripMenuItem ThemeItem(string text, HChartTheme theme)
            {
                var mi = new ToolStripMenuItem(text);
                mi.Click += (s, e) => _c.Theme = theme;
                return mi;
            }

            /// <summary>9 张主题预览图记录（主题色/glyph 在生成时使用）。</summary>
            private sealed class ThemeTileSpec
            {
                public ToolStripMenuItem Item;
                public Color Bg, Axis, Grid;
                public HMenuGlyph Glyph;
                public ThemeTileSpec(ToolStripMenuItem item, Color bg, Color axis, Color grid, HMenuGlyph glyph)
                { Item = item; Bg = bg; Axis = axis; Grid = grid; Glyph = glyph; }
            }
            /// <summary>9 个主题项与其预览图参数（首次展开时才画图，此后复用不重建）。</summary>
            private System.Collections.Generic.List<ThemeTileSpec> _themeSpecs;

            /// <summary>主题预览图：主题画布底色方块 + 主题轴色绘制的语义 glyph，右下角强调点。</summary>
            private static Bitmap ThemeTile(Color areaBg, Color axisLine, Color gridLine, HMenuGlyph glyph)
            {
                var bmp = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var br = new SolidBrush(areaBg)) g.FillRectangle(br, 0, 0, 16, 16);
                    // glyph 用主题坐标轴线色、强调色用网格色，整体居中略偏上给右下强调点留位
                    var st = g.Save();
                    g.TranslateTransform(0.5f, 0f);
                    HMenuIcon.DrawIcon(glyph, g, axisLine, gridLine);
                    g.Restore(st);
                    using (var br = new SolidBrush(gridLine)) g.FillEllipse(br, 11.5f, 11.5f, 3f, 3f);
                }
                return bmp;
            }

            // ============ 固定外观 + 图片懒加载 ============
            /// <summary>菜单固定外观初始化：圆角浅色渲染器 + 项布局（不预生成任何图片，避免大量图表时无谓画图）。</summary>
            private void InitLook()
            {
                var st = HMenuSkins.Get(Skin);
                var renderer = new HMenuFlexRenderer(Skin, Scheme);
                // 悬停块四边内缩严格 1px，专用小圆角 3（外框圆角仍为 5），避免贴图标一侧圆弧显得过大
                renderer.HoverPadding = new Padding(1);
                renderer.HoverRadius = 3;
                Renderer = renderer;
                ShowImageMargin = st.ShowImageMargin;
                ShowCheckMargin = false;
                // 隐藏图标栏的样式左侧多留白；标题栏样式顶部留出横带
                Padding = new Padding(st.ShowImageMargin ? 1 : 6,
                    st.Drop == HMenuDrop.HeaderBar ? 9 : 4, 1, 4);
                ApplyMetrics(Items, st);
            }

            /// <summary>本层菜单项按需取图：只给还没图片的 glyph 项取图（HMenuIcon 内部按 glyph+配色字典缓存）。
            /// 不递归子菜单——各下拉首次展开时各自加载本层，做到"用到才添加，没用的不添加"。</summary>
            private void EnsureLayer(ToolStripItemCollection items)
            {
                foreach (ToolStripItem it in items)
                {
                    if (it.Image == null && it.Tag is HMenuGlyph g) it.Image = Ico(g);
                }
            }

            /// <summary>9 张主题预览图在首次展开"配色主题"时一次性生成，此后直接复用不重建。</summary>
            private void EnsureThemeTiles()
            {
                if (_themeTiled) return;
                _themeTiled = true;
                foreach (var sp in _themeSpecs)
                    sp.Item.Image = ThemeTile(sp.Bg, sp.Axis, sp.Grid, sp.Glyph);
            }

            /// <summary>按样式调整项的卡片间距/加高（递归到各子菜单）。</summary>
            private void ApplyMetrics(ToolStripItemCollection items, HMenuSkinStyle st)
            {
                foreach (ToolStripItem it in items)
                {
                    if (it is ToolStripMenuItem mi)
                    {
                        // 卡片间距四边统一 1：配合悬停块 HoverPadding(1) 保证高亮块四边外距完全一致、左右对称
                        mi.Margin = st.CardMargin ? new Padding(1) : Padding.Empty;
                        mi.Padding = st.TallItems ? new Padding(0, 4, 0, 4) : Padding.Empty;
                        ApplyMetrics(mi.DropDownItems, st);
                    }
                }
            }

            // ============ 弹出前状态同步（原 Menu_Opening 迁移） ============
            /// <summary>菜单弹出前：按图表状态启用/禁用项、同步勾选与实时项文字。</summary>
            private void Menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
            {
                // 右键发生过 Pan 拖动 → 本次不弹菜单（右键单击才弹）
                if (_c._suppressMenuOnce) { _c._suppressMenuOnce = false; e.Cancel = true; return; }

                // 顶层菜单图片首次弹出时按需加载（此后缓存命中，不重复画图）
                EnsureLayer(Items);

                bool cartesian = false, hasData = false, anyHidden = false;
                foreach (var s in _c.Series)
                {
                    if (IsCartesian(s.ChartType))
                        cartesian = true;
                    bool hasPts = false;
                    foreach (var p in s.Points) if (!p.IsEmpty) { hasPts = true; break; }
                    if (s.IsEnabled && hasPts && IsCartesian(s.ChartType)) hasData = true;
                    if (!s.IsEnabled || !hasPts) anyHidden = true;
                }

                _miRubber.Enabled = cartesian;
                _miRubber.Checked = _c._mode == InteractMode.RubberBand;
                _miFit.Enabled = cartesian;
                _miRulerV.Enabled = hasData;
                _miRulerV.Checked = _c._mode == InteractMode.RulerV;
                _miRulerV.Text = HTranslation.GetContent("横坐标测量");
                _miRulerH.Enabled = hasData;
                _miRulerH.Checked = _c._mode == InteractMode.RulerH;
                _miRulerH.Text = HTranslation.GetContent("纵坐标测量");
                _miShowAll.Enabled = anyHidden;
                _miClearRuler.Enabled = _c._rulerVX.Count > 0 || _c._rulerHY.Count > 0;
                _miLegend.Checked = _c.Legends.Count > 0 && _c.Legends[0].IsEnabled;
                _miWheelEnable.Checked = _c.EnableZoom;
                _miWheelBoth.Checked = _c.WheelZoomMode == HChartWheelZoomMode.Both;
                _miWheelX.Checked = _c.WheelZoomMode == HChartWheelZoomMode.XOnly;
                _miWheelY.Checked = _c.WheelZoomMode == HChartWheelZoomMode.YOnly;
                _miWheelBoth.Enabled = _c.EnableZoom;
                _miWheelX.Enabled = _c.EnableZoom;
                _miWheelY.Enabled = _c.EnableZoom;

                _miLive.Visible = _c.IsLiveMode;
                _miLiveShowAll.Visible = _c.IsLiveMode;
                _miLiveShowAll.Enabled = true;
                _miLiveShowAll.Checked = _c.LiveShowAll;
                _miChartType.Visible = _c.AllowChartTypeChange;
                if (_c.IsLiveMode)
                {
                    _miLive.Text = _c.IsLivePaused ? HTranslation.GetContent("继续实时刷新") : HTranslation.GetContent("暂停实时刷新");
                    var glyph = _c.IsLivePaused ? HMenuGlyph.Play : HMenuGlyph.Pause;
                    _miLive.Tag = glyph;
                    _miLive.Image = Ico(glyph);
                }

                // 图表类型：按第一个启用系列的当前类型勾选；所有项恒可用
                HChartType? styleCur = null;
                foreach (var s in _c.Series)
                {
                    if (s.IsEnabled) { styleCur = s.ChartType; break; }
                }

                _miTypeLine.Enabled = _miTypeSpline.Enabled = _miTypeStep.Enabled =
                    _miTypeArea.Enabled = _miTypeColumn.Enabled = _miTypeBar.Enabled =
                    _miTypePoint.Enabled = _miTypePie.Enabled = _miTypeStackedArea.Enabled =
                    _miTypeStackedColumn.Enabled = _miTypeRadar.Enabled = _miTypeCandlestick.Enabled = true;

                _miTypeLine.Checked = styleCur == HChartType.Line;
                _miTypeSpline.Checked = styleCur == HChartType.Spline;
                _miTypeStep.Checked = styleCur == HChartType.StepLine;
                _miTypeArea.Checked = styleCur == HChartType.Area;
                _miTypeColumn.Checked = styleCur == HChartType.Column;
                _miTypeBar.Checked = styleCur == HChartType.Bar;
                _miTypePoint.Checked = styleCur == HChartType.Point;
                _miTypePie.Checked = styleCur == HChartType.Pie;
                _miTypeStackedArea.Checked = styleCur == HChartType.StackedArea;
                _miTypeStackedColumn.Checked = styleCur == HChartType.StackedColumn;
                _miTypeRadar.Checked = styleCur == HChartType.Radar;
                _miTypeCandlestick.Checked = styleCur == HChartType.Candlestick;
                _miRestore.Enabled = true;

                // 配色主题：9 种勾当前
                _miThLight.Checked = _c.Theme == HChartTheme.Light;
                _miThDark.Checked = _c.Theme == HChartTheme.Dark;
                _miThSunset.Checked = _c.Theme == HChartTheme.Sunset;
                _miThSand.Checked = _c.Theme == HChartTheme.Sand;
                _miThMatrix.Checked = _c.Theme == HChartTheme.Matrix;
                _miThPaper.Checked = _c.Theme == HChartTheme.Paper;
                _miThAqua.Checked = _c.Theme == HChartTheme.Aqua;
                _miThGrass.Checked = _c.Theme == HChartTheme.Grass;
                _miThDesert.Checked = _c.Theme == HChartTheme.Desert;
            }

            // ----- 图表功能转发（嵌套类可直接访问 HChart 私有/静态成员，这里仅做同名薄封装便于阅读） -----
            private void ToggleMode(InteractMode mode) => _c.ToggleMode(mode);
            private void ResetZoom() => _c.ResetZoom();
            private void ClearRulers() => _c.ClearRulers();
            private void ShowAllData() => _c.ShowAllData();
            private void SaveChartImage() => _c.SaveChartImage();
            private void ApplyChartStyle(HChartType t) => _c.ApplyChartStyle(t);
            private void RestoreOriginalTypes() => _c.RestoreOriginalTypes();
            private static bool IsCartesian(HChartType t) => HChart.IsCartesian(t);
        }
    }
}
