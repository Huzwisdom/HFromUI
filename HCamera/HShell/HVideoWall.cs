using System;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HCamera;
using HFromUI.HControl;
using HFromUI.HEnum;
using HFromUI.HFrom.Panel;
using HFromUI.HFrom.HUiKit;
using HFromUI.HFrom.Text;
using HFromUI.HControl.VisualWindow;

namespace HFromUI.HFrom.HShell
{
    using HFromUI.HLangage;
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 视频墙单元格：标题条（状态灯/通道名/分辨率fps/REC）+ 内容区。
    /// 内容区叠放相机画面（HVisualWindowA）与本地视频播放器（HVideoPlayer），
    /// 单击选中（蓝色描边）、双击放大/还原。
    /// </summary>
    public class HVideoCell : HPanel
    {
        private readonly HPanel _topBar;
        private readonly HStatusLed _led;
        private readonly HLabel _title;
        private readonly HLabel _info;
        private readonly HStatusLed _ledRec;
        private readonly Panel _content;
        private readonly HVisualWindowA _win;
        private readonly HVideoPlayer _player;
        private bool _selected;

        public HVideoCell(int index)
        {
            Index = index;
            Dock = DockStyle.Fill;
            Margin = new Padding(5);
            BaseColor = Color.FromArgb(16, 17, 22);
            BorderColor = Color.FromArgb(46, 51, 61);
            BorderWidth = 1;
            Radius = 8;
            ShowBorder = true;

            // 内容区
            _content = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };

            _win = new HVisualWindowA { Dock = DockStyle.Fill, BackColor = Color.Black };

            _player = new HVideoPlayer { Dock = DockStyle.Fill, Visible = false };

            _content.Controls.Add(_player);
            _content.Controls.Add(_win);

            // 标题条
            _topBar = new HPanel { Dock = DockStyle.Top, Height = 30, BackColor = Color.FromArgb(16, 17, 22) };
            HUiTheme.StylePanel(_topBar, Color.FromArgb(34, 37, 46), 8);
            _topBar.RoundStyle = HRoundStyle.Top;
            _topBar.ShowBorder = false;

            _led = new HStatusLed { Left = 10, Top = 7, Width = 16, Height = 16, State = HLedState.Off };

            _title = new HLabel { Left = 32, Top = 1, Width = 260, Height = 28 };
            HUiTheme.StyleLabel(_title, false, true);
            _title.TextAlign = ContentAlignment.MiddleLeft;
            _title.Text = HTranslation.GetContent("通道") + (index + 1) + HTranslation.GetContent(" - 未连接");
            _title.BackColor = Color.FromArgb(34, 37, 46);

            _info = new HLabel { Text = "", Width = 200, Height = 28, Left = 300, Visible = false };
            HUiTheme.StyleLabel(_info, true);
            _info.TextAlign = ContentAlignment.MiddleRight;
            _info.BackColor = Color.FromArgb(34, 37, 46);

            _ledRec = new HStatusLed
            {
                Width = 110,
                Height = 20,
                Text = "REC",
                State = HLedState.Red,
                Blink = true,
                Visible = false,
                Font = HUiTheme.FontSmall
            };
            _ledRec.ForeColor = HUiTheme.Danger;
            _topBar.Resize += (s, e) =>
            {
                _ledRec.Left = _topBar.Width - _ledRec.Width - 10;
                _ledRec.Top = (_topBar.Height - _ledRec.Height) / 2;
                _info.Left = _ledRec.Left - _info.Width - 12;
                _info.Top = 1;
                _info.Height = 28;
                _info.Visible = !string.IsNullOrEmpty(_info.Text) && _info.Left >= _title.Left + 120;
            };

            _topBar.Controls.Add(_ledRec);
            _topBar.Controls.Add(_info);
            _topBar.Controls.Add(_title);
            _topBar.Controls.Add(_led);

            Controls.Add(_content); // Fill 先加
            Controls.Add(_topBar);  // Top 后加
        }

        /// <summary>通道下标（0-3）。</summary>
        public int Index { get; private set; }

        /// <summary>相机画面窗。</summary>
        public HVisualWindowA Window { get { return _win; } }

        /// <summary>本地视频播放器。</summary>
        public HVideoPlayer Player { get { return _player; } }

        /// <summary>标题条（用于挂双击/右键事件）。</summary>
        public HPanel TopBar { get { return _topBar; } }

        /// <summary>标题文字。</summary>
        public string TitleText { get { return _title.Text; } set { _title.Text = value; } }

        /// <summary>右侧附加信息（分辨率/fps）。</summary>
        public string InfoText { get { return _info.Text; } set { _info.Text = value; _info.Visible = !string.IsNullOrEmpty(value); } }

        /// <summary>在线状态灯。</summary>
        public HLedState LedState { get { return _led.State; } set { _led.State = value; } }

        /// <summary>状态灯闪烁。</summary>
        public bool LedBlink { get { return _led.Blink; } set { _led.Blink = value; } }

        /// <summary>REC 灯是否可见。</summary>
        public bool RecVisible { get { return _ledRec.Visible; } set { _ledRec.Visible = value; } }

        /// <summary>REC 灯文字。</summary>
        public string RecText { get { return _ledRec.Text; } set { _ledRec.Text = value; } }

        /// <summary>选中态（蓝色描边）。</summary>
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value) return;
                _selected = value;
                BorderWidth = value ? 2 : 1;
                BorderColor = value ? HUiTheme.Accent : Color.FromArgb(46, 51, 61);
                Invalidate(true);
            }
        }

        /// <summary>切换相机/回放模式：true=播放器铺满画面区，false=恢复相机画面。</summary>
        public void SetPlaybackMode(bool playback)
        {
            if (playback)
            {
                _player.Visible = true;
                _player.BringToFront();
                _win.Visible = false;
            }
            else
            {
                _player.Stop();
                _player.Visible = false;
                _win.Visible = true;
                _win.BringToFront();
            }
        }
    }

    /// <summary>
    /// 视频墙：1/2/4 分屏网格，承载 4 个 HVideoCell；
    /// 支持单击选中、双击放大/Esc 还原、回放模式切换。深色监控主题。
    /// </summary>
    public class HVideoWall : UserControl
    {
        private readonly TableLayoutPanel _grid;
        /// <summary>_cells 字段。</summary>
        private readonly HVideoCell[] _cells = new HVideoCell[4];
        /// <summary>_selectedIndex 字段。</summary>
        private int _selectedIndex = -1;
        /// <summary>_layout 字段。</summary>
        private int _layout = 4;
        /// <summary>_zoomedIndex 字段。</summary>
        private int _zoomedIndex = -1;

        /// <summary>选中单元格变化。</summary>
        public event EventHandler SelectedIndexChanged;

        /// <summary>双击单元格（放大/还原由控件内部处理，同时通知宿主）。</summary>
        public event EventHandler<int> CellDoubleClicked;

        public HVideoWall()
        {
            BackColor = HUiTheme.WindowBg;
            _grid = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = HUiTheme.WindowBg };
            Controls.Add(_grid);

            for (int i = 0; i < 4; i++)
            {
                HVideoCell cell = new HVideoCell(i);
                // 单击（标题条或画面）选中
                MouseEventHandler selectHandler = (s, e) => SelectedIndex = i;
                cell.TopBar.MouseDown += selectHandler;
                cell.Window.MouseDown += selectHandler;
                cell.Player.MouseDown += selectHandler;
                // 双击放大/还原
                EventHandler dblHandler = (s, e) =>
                {
                    SelectedIndex = i;
                    CellDoubleClicked?.Invoke(this, i);
                };
                cell.Window.DoubleClick += dblHandler;
                cell.TopBar.DoubleClick += dblHandler;
                _cells[i] = cell;
            }

            SetLayout(4);
        }

        /// <summary>4 个单元格。</summary>
        public HVideoCell[] Cells { get { return _cells; } }

        /// <summary>当前选中单元格下标。</summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                int v = value < -1 ? -1 : (value > 3 ? 3 : value);
                if (_selectedIndex == v) return;
                _selectedIndex = v;
                for (int i = 0; i < 4; i++) _cells[i].Selected = (i == v);
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>当前分屏布局（1/2/4）。</summary>
        public  int Layout { get { return _layout; } }

        /// <summary>放大中的单元格下标（-1 未放大）。</summary>
        public int ZoomedIndex { get { return _zoomedIndex; } }

        /// <summary>获取相机画面窗。</summary>
        public HVisualWindowA GetWindow(int i) { return _cells[i].Window; }

        /// <summary>获取播放器。</summary>
        public HVideoPlayer GetPlayer(int i) { return _cells[i].Player; }

        /// <summary>切换单元格回放/相机模式。</summary>
        public void SetPlaybackMode(int i, bool playback) { _cells[i].SetPlaybackMode(playback); }

        /// <summary>单元格是否回放模式。</summary>
        public bool IsPlaybackMode(int i) { return _cells[i].Player.Visible; }

        /// <summary>设置 1/2/4 分屏布局。</summary>
        public void SetLayout(int n)
        {
            _layout = n;
            _zoomedIndex = -1;
            _grid.SuspendLayout();
            _grid.Controls.Clear();
            _grid.ColumnStyles.Clear();
            _grid.RowStyles.Clear();

            if (n == 1)
            {
                _grid.ColumnCount = 1; _grid.RowCount = 1;
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                _grid.Controls.Add(_cells[0], 0, 0);
                for (int i = 1; i < 4; i++) _cells[i].Visible = false;
                _cells[0].Visible = true;
            }
            else if (n == 2)
            {
                _grid.ColumnCount = 2; _grid.RowCount = 1;
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                _grid.Controls.Add(_cells[0], 0, 0);
                _grid.Controls.Add(_cells[1], 1, 0);
                _cells[0].Visible = true; _cells[1].Visible = true;
                _cells[2].Visible = false; _cells[3].Visible = false;
            }
            else
            {
                _grid.ColumnCount = 2; _grid.RowCount = 2;
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                _grid.Controls.Add(_cells[0], 0, 0);
                _grid.Controls.Add(_cells[1], 1, 0);
                _grid.Controls.Add(_cells[2], 0, 1);
                _grid.Controls.Add(_cells[3], 1, 1);
                for (int i = 0; i < 4; i++) _cells[i].Visible = true;
            }
            _grid.ResumeLayout(true);
        }

        /// <summary>放大指定单元格（单画面铺满）。</summary>
        public void ZoomCell(int idx)
        {
            _zoomedIndex = idx;
            _grid.SuspendLayout();
            _grid.Controls.Clear();
            _grid.ColumnStyles.Clear();
            _grid.RowStyles.Clear();
            _grid.ColumnCount = 1;
            _grid.RowCount = 1;
            _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) _cells[i].Visible = (i == idx);
            _grid.Controls.Add(_cells[idx], 0, 0);
            _grid.ResumeLayout(true);
        }

        /// <summary>还原放大，回到当前布局。</summary>
        public void RestoreLayout()
        {
            if (_zoomedIndex >= 0) SetLayout(_layout);
        }

        /// <summary>双击切换：已放大则还原，否则放大。</summary>
        public void ToggleZoom(int idx)
        {
            if (_zoomedIndex == idx) RestoreLayout();
            else ZoomCell(idx);
        }

        /// <summary>给单元格及其子控件挂右键菜单。</summary>
        public void SetCellContextMenu(int i, ContextMenuStrip menu)
        {
            _cells[i].ContextMenuStrip = menu;
            _cells[i].TopBar.ContextMenuStrip = menu;
            _cells[i].Window.ContextMenuStrip = menu;
            _cells[i].Player.ContextMenuStrip = menu;
        }
    }
}
