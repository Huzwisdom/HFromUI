using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HMath;
namespace HFromUI.HControl.Tools.DateTime
{
    using HFromUI.HLangage;
    using HFromUI.HControl.Base;
    using DateTime = System.DateTime;
    /// <summary>日历选中单日事件参数。</summary>
    internal class DateChosenEventArgs : EventArgs
    {
        public DateTime Date;
        public DateChosenEventArgs(DateTime date) { Date = date; }
    }
    /// <summary>日历选中日期区间事件参数。</summary>
    internal class RangeChosenEventArgs : EventArgs
    {
        public DateTime Start;
        public DateTime End;
        public RangeChosenEventArgs(DateTime start, DateTime end) { Start = start; End = end; }
    }
    /// <summary>日历层级视图：日 / 月（12 个月）/ 年（十年份网格）。</summary>
    internal enum CalView
    {
        Days = 0,
        Months = 1,
        Years = 2
    }
    /// <summary>
    /// 日历弹出层：HDropDownPopup 的日历实现。单月面板，单日模式点选单日（底部带“今天”快捷项），
    /// 区间模式在同一个月历上先点开始日、再点结束日（期间可用 ‹ › 翻到别的月份），
    /// 区间与悬停预览用橙色色带连通。仿 Windows 日历：标题里点“年”进十年份网格选年、
    /// 点“月”进 12 月网格选月，月/年视图用 ‹ › 翻页，上箭头返回上一层。
    /// </summary>
    internal class HCalendarDrop : HDropDownPopup
    {
        private const int PanelW = 224;
        private const int TitleH = 32;
        private const int WeekH = 22;
        private const int CellW = 30;
        private const int CellH = 26;
        private const int Rows = 6;
        private const int DaysH = CellH * Rows;
        private const int FooterH = 32;
        private const int PopupH = TitleH + WeekH + DaysH + FooterH;
        private const int CellOffsetX = (PanelW - CellW * 7) / 2;
        // 月/年网格相对单面板的区域（3 列 × 4 行）
        private static readonly Rectangle GridAreaRel =
            new Rectangle(16, TitleH + 10, PanelW - 32, WeekH + DaysH - 20);
        private static readonly string[] WeekNames = { HTranslation.GetContent("日"), HTranslation.GetContent("一"), HTranslation.GetContent("二"), HTranslation.GetContent("三"), HTranslation.GetContent("四"), HTranslation.GetContent("五"), HTranslation.GetContent("六") };
        private static readonly string[] MonthNames =
            { HTranslation.GetContent("一月"), HTranslation.GetContent("二月"), HTranslation.GetContent("三月"), HTranslation.GetContent("四月"), HTranslation.GetContent("五月"), HTranslation.GetContent("六月"), HTranslation.GetContent("七月"), HTranslation.GetContent("八月"), HTranslation.GetContent("九月"), HTranslation.GetContent("十月"), HTranslation.GetContent("十一月"), HTranslation.GetContent("十二月") };
        private static readonly Color Ink = Color.FromArgb(48, 52, 59);
        private static readonly Color OutInk = Color.FromArgb(183, 186, 191);
        private static readonly Color BandColor = Color.FromArgb(34, 249, 115, 22);
        private readonly bool _range;
        private readonly Font _titleFont;
        private readonly Font _weekFont;
        private readonly Font _dayFont;
        private readonly Font _dayBoldFont;
        private readonly Font _gridFont;
        private CalView _view = CalView.Days;
        private DateTime _viewMonth;   // 日视图为当前显示月份（取 1 号）；月/年视图标记已选年月
        private int _decadeStart;      // 年视图当前展示的十年段起点（与已选年解耦，翻页不改选择）
        private DateTime _selected;
        private DateTime? _rangeStart;
        private DateTime? _rangeEnd;
        private DateTime? _hoverDate;
        private int _hoverNav;         // 0 无，1 «，2 ‹，3 ›，4 »，5 上箭头
        private int _hoverTitle;       // 0 无，1 年区，2 月区
        private int _hoverGrid = -1;   // 月/年网格悬停项 0..11
        public event EventHandler<DateChosenEventArgs> DatePicked;
        public event EventHandler<RangeChosenEventArgs> RangePicked;
        public HCalendarDrop(bool rangeMode, DateTime initial, DateTime initialEnd)
        {
            _range = rangeMode;
            Radius = 8;
            Size = new Size(PanelW + 16, PopupH);
            initial = initial.Date;
            initialEnd = initialEnd.Date;
            _selected = initial;
            _rangeStart = initial;
            _rangeEnd = initialEnd >= initial ? initialEnd : initial;
            _viewMonth = new DateTime(initial.Year, initial.Month, 1);
            _decadeStart = initial.Year - initial.Year % 10;
            _titleFont = new Font("微软雅黑", 9.5f, FontStyle.Bold);
            _weekFont = new Font("微软雅黑", 8.5f);
            _dayFont = new Font("微软雅黑", 9f);
            _dayBoldFont = new Font("微软雅黑", 9f, FontStyle.Bold);
            _gridFont = new Font("微软雅黑", 9.5f);
        }
        /// <summary>区间模式下当前是否正在等待选择结束日。</summary>
        private bool WaitingEnd => _range && _rangeStart.HasValue && !_rangeEnd.HasValue;
        #region 面板与命中几何
        private static Rectangle PanelRect => new Rectangle(8, 0, PanelW, TitleH + WeekH + DaysH);
        private Rectangle DayCellRect(int col, int row)
            => new Rectangle(PanelRect.X + CellOffsetX + col * CellW,
                             PanelRect.Y + TitleH + WeekH + row * CellH, CellW, CellH);
        /// <summary>
        /// 构建 6×7 日期网格；触及 DateTime 边界（公元 1 年之前 / 9999 年之后）时越界格为 null：
        /// 留空、不绘制、不响应命中，杜绝 AddDays 越界抛 ArgumentOutOfRangeException。
        /// </summary>
        private static DateTime?[] BuildDayGrid(DateTime month)
        {
            var grid = new DateTime?[Rows * 7];
            var first = new DateTime(month.Year, month.Month, 1);
            int offset = (int)first.DayOfWeek;
            for (int i = 0; i < grid.Length; i++)
            {
                long ticks = first.Ticks + (long)(i - offset) * TimeSpan.TicksPerDay;
                if (ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks)
                    grid[i] = new DateTime(ticks);
            }
            return grid;
        }
        private Rectangle TodayRect => new Rectangle(0, Height - FooterH, Width, FooterH);
        /// <summary>命中日历格，未命中返回 null。</summary>
        private DateTime? HitDay(Point p)
        {
            var panel = PanelRect;
            int daysTop = panel.Y + TitleH + WeekH;
            if (p.X < panel.X + CellOffsetX || p.X >= panel.X + CellOffsetX + CellW * 7 ||
                p.Y < daysTop || p.Y >= daysTop + DaysH) return null;
            int col = (p.X - panel.X - CellOffsetX) / CellW;
            int row = (p.Y - daysTop) / CellH;
            if (col < 0 || col > 6 || row < 0 || row >= Rows) return null;
            return BuildDayGrid(_viewMonth)[row * 7 + col];
        }
        /// <summary>命中导航箭头：1 « 2 ‹ 3 › 4 »（仅日视图），5 上箭头（月/年视图），未命中 0。</summary>
        private int HitNav(Point p)
        {
            if (p.Y < 0 || p.Y > TitleH) return 0;
            var panel = PanelRect;
            if (_view != CalView.Days)
            {
                if (new Rectangle(panel.X + 4, 8, 16, 16).Contains(p)) return 5;
                if (new Rectangle(panel.X + 22, 8, 16, 16).Contains(p)) return 2;
                if (new Rectangle(panel.Right - 38, 8, 16, 16).Contains(p)) return 3;
                return 0;
            }
            if (new Rectangle(panel.X + 4, 8, 16, 16).Contains(p)) return 1;
            if (new Rectangle(panel.X + 22, 8, 16, 16).Contains(p)) return 2;
            if (new Rectangle(panel.Right - 38, 8, 16, 16).Contains(p)) return 3;
            if (new Rectangle(panel.Right - 20, 8, 16, 16).Contains(p)) return 4;
            return 0;
        }
        /// <summary>日视图标题“年/月”分区命中：1 年区，2 月区，0 无。</summary>
        private int HitTitle(Point p)
        {
            if (_view == CalView.Days)
            {
                var (yr, mo) = DayTitleRects();
                if (yr.Contains(p)) return 1;
                if (mo.Contains(p)) return 2;
            }
            else
            {
                // 月视图点年份进年视图
                var yr = NonDayTitleRect();
                if (yr.Contains(p)) return 1;
            }
            return 0;
        }
        private (Rectangle year, Rectangle month) DayTitleRects()
        {
            var panel = PanelRect;
            string ys = _viewMonth.Year + HTranslation.GetContent(" 年");
            string ms = _viewMonth.Month + HTranslation.GetContent(" 月");
            int wy = TextRenderer.MeasureText(ys, _titleFont, Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            int wm = TextRenderer.MeasureText(ms, _titleFont, Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            const int gap = 6;
            int startX = panel.X + (PanelW - wy - gap - wm) / 2;
            return (new Rectangle(startX, 6, wy, TitleH - 8),
                    new Rectangle(startX + wy + gap, 6, wm, TitleH - 8));
        }
        private Rectangle NonDayTitleRect()
        {
            var panel = PanelRect;
            // 避开左侧上箭头/‹ 与右侧 ›
            return new Rectangle(panel.X + 42, 6, PanelW - 84, TitleH - 8);
        }
        /// <summary>月/年网格命中，返回 0..11，未命中 -1。</summary>
        private int HitGridCell(Point p)
        {
            var area = GridAreaAbs();
            if (!area.Contains(p)) return -1;
            int cw = area.Width / 3, ch = area.Height / 4;
            int col = (p.X - area.X) / cw;
            int row = (p.Y - area.Y) / ch;
            if (col < 0 || col > 2 || row < 0 || row > 3) return -1;
            return row * 3 + col;
        }
        private Rectangle GridAreaAbs()
        {
            var panel = PanelRect;
            return new Rectangle(panel.X + GridAreaRel.X, GridAreaRel.Y, GridAreaRel.Width, GridAreaRel.Height);
        }
        private Rectangle GridCellRect(int index)
        {
            var area = GridAreaAbs();
            int cw = area.Width / 3, ch = area.Height / 4;
            return new Rectangle(area.X + (index % 3) * cw, area.Y + (index / 3) * ch, cw, ch);
        }
        private int DecadeStart => _decadeStart;
        #endregion
        #region 鼠标交互
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int nav = HitNav(e.Location);
            int title = HitTitle(e.Location);
            int grid = _view != CalView.Days && nav == 0 && title == 0 ? HitGridCell(e.Location) : -1;
            DateTime? day = (nav == 0 && title == 0 && _view == CalView.Days) ? HitDay(e.Location) : null;
            if (nav != _hoverNav || title != _hoverTitle || grid != _hoverGrid || day != _hoverDate)
            {
                _hoverNav = nav;
                _hoverTitle = title;
                _hoverGrid = grid;
                _hoverDate = day;
                Cursor = (nav != 0 || title != 0 || day != null || grid >= 0) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            int nav = HitNav(e.Location);
            if (nav != 0)
            {
                switch (_view)
                {
                    case CalView.Days:
                        if (nav == 1) ShiftMonth(-12);
                        else if (nav == 2) ShiftMonth(-1);
                        else if (nav == 3) ShiftMonth(1);
                        else if (nav == 4) ShiftMonth(12);
                        break;
                    case CalView.Months:
                        if (nav == 5) _view = CalView.Days;
                        else if (nav == 2) ShiftYear(-1);
                        else if (nav == 3) ShiftYear(1);
                        break;
                    case CalView.Years:
                        if (nav == 5) _view = CalView.Months;
                        else if (nav == 2) ShiftDecade(-10);
                        else if (nav == 3) ShiftDecade(10);
                        break;
                }
                Invalidate();
                return;
            }
            int title = HitTitle(e.Location);
            if (title == 1)
            {
                _view = CalView.Years;
                // 进入年视图时十年段定位到已选年所在页（翻页只改展示页，不动已选年）
                _decadeStart = _viewMonth.Year - _viewMonth.Year % 10;
                _hoverGrid = -1;
                Invalidate();
                return;
            }
            if (title == 2 && _view == CalView.Days)
            {
                _view = CalView.Months;
                _hoverGrid = -1;
                Invalidate();
                return;
            }
            if (_view == CalView.Months)
            {
                int idx = HitGridCell(e.Location);
                if (idx >= 0)
                {
                    int day = Math.Min(_viewMonth.Day, DateTime.DaysInMonth(_viewMonth.Year, idx + 1));
                    _viewMonth = new DateTime(_viewMonth.Year, idx + 1, day);
                    _view = CalView.Days;
                    _hoverGrid = -1;
                    Invalidate();
                }
                return;
            }
            if (_view == CalView.Years)
            {
                int idx = HitGridCell(e.Location);
                if (idx >= 0)
                {
                    int y = DecadeStart - 1 + idx;
                    if (y < 1 || y > 9999) return;   // 相邻十年的灰字越界格不可选
                    int day = Math.Min(_viewMonth.Day, DateTime.DaysInMonth(y, _viewMonth.Month));
                    _viewMonth = new DateTime(y, _viewMonth.Month, day);
                    _view = CalView.Months;
                    _hoverGrid = -1;
                    Invalidate();
                }
                return;
            }
            DateTime? picked = HitDay(e.Location);
            if (picked.HasValue)
            {
                ChooseDay(picked.Value);
                return;
            }
            if (TodayRect.Contains(e.Location))
            {
                if (_range) return; // 区间模式底部为提示文字，不可点
                DateTime today = DateTime.Today;
                DatePicked?.Invoke(this, new DateChosenEventArgs(today));
                ClosePopup();
            }
        }
        private void ShiftMonth(int delta)
        {
            // 用“年月序号”夹取到 0001-01 .. 9999-12，边界上继续翻月停在边界，AddMonths 不会越界
            // 年月序号 = 年*12+(月-1)；注意 0001-01 的序号是 12，不是 0，
            // 下夹到 0 会反算出“年 0”导致 DateTime 构造抛 ArgumentOutOfRangeException
            long index = _viewMonth.Year * 12L + _viewMonth.Month - 1 + delta;
            if (index < 12L) index = 12L;
            if (index > 9999L * 12 + 11) index = 9999L * 12 + 11;
            _viewMonth = new DateTime((int)(index / 12), (int)(index % 12) + 1, 1);
            _hoverDate = null;
        }
        private void ShiftYear(int delta)
        {
            int y = _viewMonth.Year + delta;
            if (y < 1) y = 1;
            if (y > 9999) y = 9999;
            int day = Math.Min(_viewMonth.Day, DateTime.DaysInMonth(y, _viewMonth.Month));
            _viewMonth = new DateTime(y, _viewMonth.Month, day);
        }
        /// <summary>年视图翻十年页：只改展示的十年段起点，已选年月保持不变（同 Windows 日历）。</summary>
        private void ShiftDecade(int delta)
        {
            int d = _decadeStart + delta;
            if (d < 0) d = 0;
            if (d > 9990) d = 9990;
            _decadeStart = d;
        }
        private void ChooseDay(DateTime date)
        {
            if (_range)
            {
                // 单面板：点到非本月日子直接把视图跳到该月再开始/完成区间
                if (date.Year != _viewMonth.Year || date.Month != _viewMonth.Month)
                    _viewMonth = new DateTime(date.Year, date.Month, 1);
                if (WaitingStart() || date < _rangeStart.Value)
                {
                    // 第一次点击，或新点的日子早于已选开始日：重选开始日
                    _rangeStart = date;
                    _rangeEnd = null;
                    Invalidate();
                }
                else
                {
                    _rangeEnd = date;
                    Invalidate();
                    RangePicked?.Invoke(this, new RangeChosenEventArgs(_rangeStart.Value, date));
                    ClosePopup();
                }
                return;
            }
            // 点到非本月灰字日：先翻到该月再选
            if (date.Year != _viewMonth.Year || date.Month != _viewMonth.Month)
                _viewMonth = new DateTime(date.Year, date.Month, 1);
            _selected = date;
            DatePicked?.Invoke(this, new DateChosenEventArgs(date));
            ClosePopup();
        }
        /// <summary>开始日尚未选择，或一轮选择已完成需要重选时为真。</summary>
        private bool WaitingStart() => !_rangeStart.HasValue || _rangeEnd.HasValue;
        protected override void OnPopupClosed(EventArgs e)
        {
            base.OnPopupClosed(e);
            _hoverDate = null;
            _hoverNav = 0;
            _hoverTitle = 0;
            _hoverGrid = -1;
            _view = CalView.Days;
        }
        #endregion
        #region 绘制
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            switch (_view)
            {
                case CalView.Days:
                    PaintDays(g);
                    break;
                case CalView.Months:
                    PaintMonths(g);
                    break;
                case CalView.Years:
                    PaintYears(g);
                    break;
            }
            PaintFooter(g);
        }
        private void PaintDays(Graphics g)
        {
            var panel = PanelRect;
            DateTime month = _viewMonth;
            // 标题：年区 / 月区 分别可点
            var (yr, mo) = DayTitleRects();
            Color yearColor = _hoverTitle == 1 ? Color.FromArgb(249, 115, 22) : Ink;
            Color monthColor = _hoverTitle == 2 ? Color.FromArgb(249, 115, 22) : Ink;
            TextRenderer.DrawText(g, month.Year + HTranslation.GetContent(" 年"), _titleFont, yr, yearColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, month.Month + HTranslation.GetContent(" 月"), _titleFont, mo, monthColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            DrawChevron(g, new Rectangle(panel.X + 4, 8, 16, 16), true, _hoverNav == 1, true);
            DrawChevron(g, new Rectangle(panel.X + 22, 8, 16, 16), true, _hoverNav == 2, false);
            DrawChevron(g, new Rectangle(panel.Right - 38, 8, 16, 16), false, _hoverNav == 3, false);
            DrawChevron(g, new Rectangle(panel.Right - 20, 8, 16, 16), false, _hoverNav == 4, true);
            // 星期表头
            for (int col = 0; col < 7; col++)
            {
                var r = new Rectangle(panel.X + CellOffsetX + col * CellW, panel.Y + TitleH, CellW, WeekH);
                Color wc = col == 0 || col == 6 ? Color.FromArgb(232, 119, 74) : Color.FromArgb(132, 136, 142);
                TextRenderer.DrawText(g, WeekNames[col], _weekFont, r, wc,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            DateTime?[] dayGrid = BuildDayGrid(month);
            DateTime today = DateTime.Today;
            DateTime? bandEnd = WaitingEnd && _hoverDate.HasValue && _hoverDate.Value > _rangeStart.Value
                ? _hoverDate.Value : (DateTime?)null;
            // 第一层：区间色带（先铺底，文字/端点后画）
            if (_range && _rangeStart.HasValue)
            {
                DateTime bEnd = bandEnd ?? _rangeEnd ?? _rangeStart.Value;
                using (var band = new SolidBrush(BandColor))
                {
                    for (int i = 0; i < Rows * 7; i++)
                    {
                        DateTime? cellDate = dayGrid[i];
                        if (!cellDate.HasValue) continue;
                        DateTime d = cellDate.Value;
                        if (d < _rangeStart.Value || d > bEnd || bEnd <= _rangeStart.Value) continue;
                        int col = i % 7, row = i / 7;
                        var cell = DayCellRect(col, row);
                        int bandY = cell.Y + 4, bandH = CellH - 8;
                        if (d == _rangeStart.Value)
                            g.FillRectangle(band, cell.X + CellW / 2, bandY, CellW / 2, bandH);
                        else if (d == bEnd)
                            g.FillRectangle(band, cell.X, bandY, CellW / 2 + 1, bandH);
                        else
                            g.FillRectangle(band, cell.X, bandY, CellW, bandH);
                    }
                }
            }
            // 第二层：日期文字 / 今日环 / 悬停底 / 选中端点
            for (int i = 0; i < Rows * 7; i++)
            {
                DateTime? cellDate = dayGrid[i];
                if (!cellDate.HasValue) continue;   // 边界越界格留空
                DateTime d = cellDate.Value;
                int col = i % 7, row = i / 7;
                var cell = DayCellRect(col, row);
                bool inMonth = d.Year == month.Year && d.Month == month.Month;
                bool isStart = _range && _rangeStart == d;
                bool isEnd = _range && ((bandEnd.HasValue && bandEnd.Value == d && WaitingEnd) || _rangeEnd == d);
                bool isSingleSel = !_range && _selected == d;
                bool endpoint = isStart || isEnd;
                if (!endpoint && _hoverDate == d && inMonth)
                {
                    var hover = new Rectangle(cell.X + 2, cell.Y + 2, CellW - 4, CellH - 4);
                    using (GraphicsPath hp = HDrawPaint.CreatePath(hover, 6, HRoundStyle.All, false))
                    using (var hb = new SolidBrush(Color.FromArgb(26, 249, 115, 22)))
                        g.FillPath(hb, hp);
                }
                // 端点橙丸只画在所属月份的格子上；相邻月溢出格里的同一天只保留色带与灰字
                if (isSingleSel || (endpoint && inMonth))
                {
                    var pill = new Rectangle(cell.X + 3, cell.Y + 3, CellW - 6, CellH - 6);
                    using (GraphicsPath pp = HDrawPaint.CreatePath(pill, 6, HRoundStyle.All, false))
                    using (var pb = new SolidBrush(Color.FromArgb(249, 115, 22)))
                        g.FillPath(pb, pp);
                    TextRenderer.DrawText(g, d.Day.ToString(), _dayBoldFont, pill, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
                else
                {
                    if (d == today && inMonth)
                    {
                        var ring = new Rectangle(cell.X + 3, cell.Y + 3, CellW - 7, CellH - 7);
                        using (GraphicsPath rp = HDrawPaint.CreatePath(ring, 6, HRoundStyle.All, false))
                        using (var pen = new Pen(Color.FromArgb(249, 115, 22), 1.4f))
                            g.DrawPath(pen, rp);
                    }
                    Color c = inMonth ? Ink : OutInk;
                    TextRenderer.DrawText(g, d.Day.ToString(), _dayFont, cell, c,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            }
        }
        private void PaintMonths(Graphics g)
        {
            var panel = PanelRect;
            DrawUpChevron(g, new Rectangle(panel.X + 4, 8, 16, 16), _hoverNav == 5);
            DrawChevron(g, new Rectangle(panel.X + 22, 8, 16, 16), true, _hoverNav == 2, false);
            DrawChevron(g, new Rectangle(panel.Right - 38, 8, 16, 16), false, _hoverNav == 3, false);
            string title = _viewMonth.Year + HTranslation.GetContent(" 年");
            TextRenderer.DrawText(g, title, _titleFont, NonDayTitleRect(),
                _hoverTitle == 1 ? Color.FromArgb(249, 115, 22) : Ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            DateTime today = DateTime.Today;
            for (int i = 0; i < 12; i++)
            {
                var cell = GridCellRect(i);
                bool selected = i + 1 == _viewMonth.Month;
                bool isTodayMonth = today.Year == _viewMonth.Year && today.Month == i + 1;
                PaintGridCell(g, cell, MonthNames[i], selected, isTodayMonth, i == _hoverGrid);
            }
        }
        private void PaintYears(Graphics g)
        {
            var panel = PanelRect;
            DrawUpChevron(g, new Rectangle(panel.X + 4, 8, 16, 16), _hoverNav == 5);
            DrawChevron(g, new Rectangle(panel.X + 22, 8, 16, 16), true, _hoverNav == 2, false);
            DrawChevron(g, new Rectangle(panel.Right - 38, 8, 16, 16), false, _hoverNav == 3, false);
            int ds = DecadeStart;
            string title = ds + " - " + (ds + 9);
            TextRenderer.DrawText(g, title, _titleFont, NonDayTitleRect(), Ink,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            DateTime today = DateTime.Today;
            for (int i = 0; i < 12; i++)
            {
                var cell = GridCellRect(i);
                int y = ds - 1 + i;
                bool inDecade = i > 0 && i < 11;   // 首尾两个为相邻十年的灰字年
                bool selected = y == _viewMonth.Year;
                bool isTodayYear = today.Year == y;
                // 公元 1 年之前没有有效日期：首十年段的第 0 格（y=0/-1）留空且不可点
                string label = y >= 1 ? y.ToString() : string.Empty;
                PaintGridCell(g, cell, label, selected, isTodayYear, i == _hoverGrid, inDecade);
            }
        }
        /// <summary>月/年网格通用格子：选中橙丸白字、今日橙环、悬停浅橙底，inDecade=false 灰字。</summary>
        private void PaintGridCell(Graphics g, Rectangle cell, string label, bool selected,
            bool isToday, bool hovered, bool inDecade = true)
        {
            var box = new Rectangle(cell.X + 4, cell.Y + 4, cell.Width - 8, cell.Height - 8);
            if (selected)
            {
                using (GraphicsPath pp = HDrawPaint.CreatePath(box, 6, HRoundStyle.All, false))
                using (var pb = new SolidBrush(Color.FromArgb(249, 115, 22)))
                    g.FillPath(pb, pp);
                TextRenderer.DrawText(g, label, _gridFont, box, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                return;
            }
            if (hovered)
            {
                using (GraphicsPath hp = HDrawPaint.CreatePath(box, 6, HRoundStyle.All, false))
                using (var hb = new SolidBrush(Color.FromArgb(26, 249, 115, 22)))
                    g.FillPath(hb, hp);
            }
            if (isToday && inDecade)
            {
                var ring = new Rectangle(box.X + 1, box.Y + 1, box.Width - 3, box.Height - 3);
                using (GraphicsPath rp = HDrawPaint.CreatePath(ring, 6, HRoundStyle.All, false))
                using (var pen = new Pen(Color.FromArgb(249, 115, 22), 1.4f))
                    g.DrawPath(pen, rp);
            }
            TextRenderer.DrawText(g, label, _gridFont, box, inDecade ? Ink : OutInk,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
        /// <summary>画 ‹/›/«/» 导航箭头。</summary>
        private void DrawChevron(Graphics g, Rectangle r, bool pointLeft, bool hovered, bool doubleChevron)
        {
            int cx = r.X + r.Width / 2;
            int cy = r.Y + r.Height / 2;
            Color ink = hovered ? Color.FromArgb(249, 115, 22) : Color.FromArgb(120, 124, 130);
            using (var pen = new Pen(ink, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (doubleChevron)
                {
                    DrawOneChevron(g, pen, pointLeft ? cx - 1 : cx - 3, cy, pointLeft);
                    DrawOneChevron(g, pen, pointLeft ? cx + 4 : cx + 2, cy, pointLeft);
                }
                else
                {
                    DrawOneChevron(g, pen, cx + (pointLeft ? 2 : -2), cy, pointLeft);
                }
            }
        }
        /// <summary>月/年视图的返回上一层上箭头（^）。</summary>
        private void DrawUpChevron(Graphics g, Rectangle r, bool hovered)
        {
            int cx = r.X + r.Width / 2;
            int cy = r.Y + r.Height / 2;
            Color ink = hovered ? Color.FromArgb(249, 115, 22) : Color.FromArgb(120, 124, 130);
            using (var pen = new Pen(ink, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLines(pen, new[]
                {
                    new Point(cx - 4, cy + 2), new Point(cx, cy - 3), new Point(cx + 4, cy + 2)
                });
        }
        private static void DrawOneChevron(Graphics g, Pen pen, int cx, int cy, bool pointLeft)
        {
            Point[] pts = pointLeft
                ? new[] { new Point(cx + 3, cy - 4), new Point(cx - 2, cy), new Point(cx + 3, cy + 4) }
                : new[] { new Point(cx - 3, cy - 4), new Point(cx + 2, cy), new Point(cx - 3, cy + 4) };
            g.DrawLines(pen, pts);
        }
        private void PaintFooter(Graphics g)
        {
            int y = Height - FooterH;
            using (var pen = new Pen(Color.FromArgb(232, 234, 238), 1f))
                g.DrawLine(pen, 8, y, Width - 9, y);
            if (_range)
            {
                string hint = WaitingEnd ? HTranslation.GetContent("请选择结束日期") : HTranslation.GetContent("请选择开始日期");
                TextRenderer.DrawText(g, hint, _weekFont, new Rectangle(0, y, Width, FooterH),
                    Color.FromArgb(150, 154, 162),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            else
            {
                TextRenderer.DrawText(g, HTranslation.GetContent("今天"), _dayFont, new Rectangle(0, y, Width, FooterH),
                    Color.FromArgb(249, 115, 22),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _titleFont.Dispose();
                _weekFont.Dispose();
                _dayFont.Dispose();
                _dayBoldFont.Dispose();
                _gridFont.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
