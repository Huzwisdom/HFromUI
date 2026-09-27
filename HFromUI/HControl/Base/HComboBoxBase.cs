using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{
    using HFromUI.HLangage;
    /// <summary>下拉框样式：仅选择（DropDownList）或可编辑（DropDown）。</summary>
    public enum HComboStyle
    {
        /// <summary>只能从下拉列表选择，不能自由输入（Windows ComboBox 的 DropDownList）。</summary>
        DropDownList = 0,
        /// <summary>可以自由输入文字，也可从下拉列表选择（Windows ComboBox 的 DropDown）。</summary>
        DropDown = 1
    }

    /// <summary>
    /// 下拉框基类：直接继承 Control 纯自绘（不内嵌任何原生 ComboBox），
    /// 自绘外框、下拉箭头、弹出列表（悬停高亮、自绘滚动条、滚轮/拖拽翻页）、
    /// 键盘导航（F4/Alt+方向键、上下键、翻页、回车/Esc）、可编辑模式的系统插入符与文字编辑、
    /// 以及输入前缀自动匹配。圆角通过 Radius 设置：0 或负数为 Windows 原生直角立体样式。
    /// </summary>
    [DefaultProperty("Items")]
    [DefaultEvent("SelectedIndexChanged")]
    public class HComboBoxBase : Control
    {
        #region Win32 系统插入符 / 异步按键状态

        [DllImport("user32.dll")]
        private static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
        [DllImport("user32.dll")]
        private static extern bool DestroyCaret();
        [DllImport("user32.dll")]
        private static extern bool SetCaretPos(int X, int Y);
        [DllImport("user32.dll")]
        private static extern bool ShowCaret(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        #endregion

        private const TextFormatFlags ItemFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                                                 TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

        // 编辑文字的测量与绘制必须使用完全相同的标志，否则插入符与字形之间会出现错位
        // 编辑态测量/绘制共用标志：SingleLine 必须存在，否则多行测量模式给宽度加换行余量，插入符虚偏。
        private const TextFormatFlags EditFlags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        private readonly HComboItemCollection _items;
        private int _selectedIndex = -1;
        private int _radius;
        private int _borderWidth = 1;
        private Color _shapeBackColor = Color.Empty;
        private HComboStyle _dropDownStyle = HComboStyle.DropDownList;
        private int _maxDropDownItems = 8;
        private int _dropDownWidth;
        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;

        private bool _hover;
        private bool _buttonPressed;
        private bool _caretCreated;

        // 可编辑模式文本状态（_anchor 为选区固定端，_caret 为插入符端）
        private string _editText = string.Empty;
        private int _anchor;
        private int _caret;
        private int _scrollX;
        private bool _selectDragging;

        // 可编辑模式撤销/重做快照
        private readonly Stack<EditSnapshot> _editUndo = new Stack<EditSnapshot>();
        private readonly Stack<EditSnapshot> _editRedo = new Stack<EditSnapshot>();
        private DateTime _lastCharEdit = DateTime.MinValue;

        private struct EditSnapshot
        {
            public string Text;
            public int Anchor;
            public int Caret;
            public EditSnapshot(string text, int anchor, int caret) { Text = text; Anchor = anchor; Caret = caret; }
        }

        // 键盘增量前缀搜索（DropDownList）
        private string _incrPrefix = string.Empty;
        private DateTime _incrTick = DateTime.MinValue;

        private HDropForm _drop;
        private Form _ownerForm;

        public HComboBoxBase()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);
            _items = new HComboItemCollection(this);
            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;
            Font = new Font("微软雅黑", 9f);
            Size = new Size(120, 28);
        }

        #region 外观属性

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径，0 或负数为 Windows 原生直角立体边"), Browsable(true)]
        [DefaultValue(0)]
        public virtual int Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateRegion();
                Invalidate();
            }
        }

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("圆角边框宽度"), HDescriptionLanguage("圆角边框宽度（像素），0 或负数不描边；仅圆角模式生效。描边向内收半个线宽，粗边也完整不缺边"), Browsable(true)]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                UpdateRegion();
                Invalidate();
            }
        }

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("框内背景色"), HDescriptionLanguage("圆角/椭圆/胶囊外形内部的背景色；未设置时使用控件背景色。颜色只填充外形内部，圆角外不染色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }

        /// <summary>外形内部实际底色：设置了框内背景色用新色，否则用控件背景色。</summary>
        private Color ShapeFillColor => _shapeBackColor.IsEmpty ? BackColor : _shapeBackColor;

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("下拉最大项数"), HDescriptionLanguage("下拉列表最大显示项数，超出后出现滚动条"), Browsable(true)]
        [DefaultValue(8)]
        public int MaxDropDownItems
        {
            get => _maxDropDownItems;
            set => _maxDropDownItems = Math.Max(1, value);
        }

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("下拉列表宽度"), HDescriptionLanguage("下拉列表宽度（像素），0 表示与控件同宽；长选项时可设置得更宽"), Browsable(true)]
        [DefaultValue(0)]
        public int DropDownWidth
        {
            get => _dropDownWidth;
            set => _dropDownWidth = Math.Max(0, value);
        }

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("下拉框样式"), HDescriptionLanguage("下拉框样式：仅选择 / 可编辑"), Browsable(true)]
        [DefaultValue(HComboStyle.DropDownList)]
        public HComboStyle DropDownStyle
        {
            get => _dropDownStyle;
            set
            {
                if (_dropDownStyle == value) return;
                _dropDownStyle = value;
                if (DesignMode) return;
                CloseDrop();
                if (value == HComboStyle.DropDown)
                {
                    _editText = SelectedText;
                    _anchor = _caret = _editText.Length;
                    _scrollX = 0;
                }
                else
                {
                    DestroyCaret();
                    _caretCreated = false;
                }
                Invalidate();
            }
        }

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("文字对齐"), HDescriptionLanguage("文字对齐（九宫格，与 HLabelBase 一致）；长文本放不下时退化为左对齐并横向滚动"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TextAlign
        {
            get => _textAlign;
            set
            {
                _textAlign = value;
                EnsureCaretVisible();
                PositionCaret();
                Invalidate();
            }
        }

        #endregion

        #region 数据属性

        [HCategoryLanguage("下拉框基础设置"), HDisplayNameLanguage("下拉选项集合"), HDescriptionLanguage("下拉选项集合"), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design",
                "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public HComboItemCollection Items => _items;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < -1 || value >= _items.Count)
                    throw new ArgumentOutOfRangeException(nameof(value));
                SetSelected(value, false);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get => _selectedIndex >= 0 ? _items[_selectedIndex] : null;
            set
            {
                int i = value == null ? -1 : _items.IndexOf(value);
                SetSelected(i, false);
            }
        }

        /// <summary>当前选中项的显示文本（未选中时为空串）。</summary>
        [Browsable(false)]
        public string SelectedText => _selectedIndex >= 0 ? GetItemText(_items[_selectedIndex]) : string.Empty;

        [Browsable(false)]
        public bool DroppedDown
        {
            get => _drop != null && _drop.Visible;
            set
            {
                if (value) OpenDrop();
                else CloseDrop();
            }
        }

        public override string Text
        {
            get => _dropDownStyle == HComboStyle.DropDown ? _editText : SelectedText;
            set
            {
                value = value ?? string.Empty;
                if (_dropDownStyle == HComboStyle.DropDown)
                {
                    if (_editText == value) return;
                    _editText = value;
                    _anchor = _caret = value.Length;
                    _editUndo.Clear();
                    _editRedo.Clear();
                    EnsureCaretVisible();
                    PositionCaret();
                    SyncSelectionWithEdit();
                    Invalidate();
                    OnTextChanged(EventArgs.Empty);
                }
                else
                {
                    int match = FindExact(value);
                    if (match != _selectedIndex) SetSelected(match, false);
                }
            }
        }

        #endregion

        #region 事件

        public event EventHandler SelectedIndexChanged;
        public event EventHandler SelectionChangeCommitted;
        public event EventHandler DropDown;
        public event EventHandler DropDownClosed;

        protected virtual void OnSelectedIndexChanged(EventArgs e) => SelectedIndexChanged?.Invoke(this, e);
        protected virtual void OnSelectionChangeCommitted(EventArgs e) => SelectionChangeCommitted?.Invoke(this, e);
        protected virtual void OnDropDown(EventArgs e) => DropDown?.Invoke(this, e);
        protected virtual void OnDropDownClosed(EventArgs e) => DropDownClosed?.Invoke(this, e);

        #endregion

        #region 集合变化与选中

        /// <summary>Items 集合增删后回调：夹取选中下标并重绘。</summary>
        internal void ItemsChanged()
        {
            if (_selectedIndex >= _items.Count) SetSelected(-1, false);
            Invalidate();
        }

        private void SetSelected(int index, bool committed)
        {
            if (index == _selectedIndex)
            {
                if (committed) OnSelectionChangeCommitted(EventArgs.Empty);
                return;
            }
            _selectedIndex = index;
            Invalidate();
            _drop?.Invalidate();
            OnSelectedIndexChanged(EventArgs.Empty);
            if (committed) OnSelectionChangeCommitted(EventArgs.Empty);
        }

        /// <summary>列表项/回车确认选中：同步编辑文本并关闭下拉。</summary>
        private void CommitDrop(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                CloseDrop();
                return;
            }
            bool changed = index != _selectedIndex;
            _selectedIndex = index;
            if (_dropDownStyle == HComboStyle.DropDown)
            {
                _editText = GetItemText(_items[index]);
                _anchor = _caret = _editText.Length;
                _scrollX = 0;
            }
            CloseDrop();
            Invalidate();
            PositionCaret();
            if (changed) OnSelectedIndexChanged(EventArgs.Empty);
            OnSelectionChangeCommitted(EventArgs.Empty);
        }

        private string GetItemText(object item) => item?.ToString() ?? string.Empty;

        #endregion

        #region 下拉开关

        private Rectangle ButtonRect
        {
            get
            {
                int w = Math.Max(18, Height - 4);
                return new Rectangle(Width - w, 0, w, Height);
            }
        }

        private Rectangle ContentRect => new Rectangle(6, 0, Width - ButtonRect.Width - 8, Height);

        private int ItemHeight => TextRenderer.MeasureText("Ay", Font, Size.Empty,
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding).Height + 8;

        private void OpenDrop()
        {
            if (DesignMode || !Enabled || _items.Count == 0 || IsDisposed) return;
            if (DroppedDown) return;
            if (_drop == null) _drop = new HDropForm(this);
            _drop.Font = Font;

            int itemH = ItemHeight;
            int visible = Math.Min(_maxDropDownItems, _items.Count);
            int width = _dropDownWidth > 0 ? _dropDownWidth : Width;
            int height = visible * itemH + 2;

            Point topLeft = PointToScreen(new Point(0, 0));
            Point bottom = PointToScreen(new Point(0, Height));
            Rectangle work = Screen.FromControl(this).WorkingArea;

            int x = bottom.X;
            int y = bottom.Y + 1;
            if (y + height > work.Bottom)
            {
                int aboveY = topLeft.Y - height - 1;
                if (aboveY >= work.Top) y = aboveY;
                else height = Math.Max(itemH + 2, work.Bottom - y);
            }
            if (x + width > work.Right) x = work.Right - width;
            if (x < work.Left) x = work.Left;

            _drop.Prepare(width, height, itemH, visible);
            _drop.HoverIndex = _selectedIndex >= 0 ? _selectedIndex : 0;
            _drop.Location = new Point(x, y);
            _drop.Show(this);
            Invalidate();
            OnDropDown(EventArgs.Empty);

            _ownerForm = FindForm();
            if (_ownerForm != null) _ownerForm.Deactivate += OwnerForm_Deactivate;
        }

        private void CloseDrop()
        {
            if (_drop == null || !_drop.Visible) return;
            _drop.Hide();
            _drop.StopWatch();
            if (_ownerForm != null)
            {
                _ownerForm.Deactivate -= OwnerForm_Deactivate;
                _ownerForm = null;
            }
            Invalidate();
            OnDropDownClosed(EventArgs.Empty);
        }

        private void OwnerForm_Deactivate(object sender, EventArgs e) => CloseDrop();

        /// <summary>点击列表外部（弹出窗轮询到外部鼠标按下）：取消，不改变选中。</summary>
        internal void CancelDrop()
        {
            if (_dropDownStyle == HComboStyle.DropDown && _selectedIndex >= 0)
            {
                _editText = GetItemText(_items[_selectedIndex]);
                _anchor = _caret = _editText.Length;
            }
            CloseDrop();
            Invalidate();
            PositionCaret();
        }

        #endregion

        #region 鼠标交互

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _buttonPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !Enabled) return;
            bool wasFocused = Focused;
            Focus();

            // 已弹出时点控件任何位置都只负责收起（与 Windows ComboBox 一致）
            if (DroppedDown)
            {
                CloseDrop();
                return;
            }

            if (ButtonRect.Contains(e.Location))
            {
                _buttonPressed = true;
                Invalidate();
                return;
            }

            if (_dropDownStyle == HComboStyle.DropDown)
            {
                // 首次点入（焦点不在本控件）：保持获焦时的整串全选，不按点击点落插入符；
                // 已获焦后的再次点击才把插入符放到点击位置（Windows ComboBox 语义）
                if (!wasFocused)
                {
                    EditSelectAll();
                    _selectDragging = true;
                    Capture = true;
                    return;
                }
                int idx = EditIndexFromPoint(e.Location);
                if (e.Clicks >= 2) SelectWordAt(idx);
                else _anchor = _caret = idx;
                _selectDragging = true;
                Capture = true;
                EnsureCaretVisible();
                PositionCaret();
                Invalidate();
            }
            else
            {
                OpenDrop();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_selectDragging)
            {
                _selectDragging = false;
                Capture = false;
            }
            if (!_buttonPressed) return;
            _buttonPressed = false;
            Invalidate();
            OpenDrop();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_selectDragging || e.Button != MouseButtons.Left) return;

            // 拖到内容区左右边缘时按字符宽度自动横向滚动
            var cr = ContentRect;
            int step = TextRenderer.MeasureText(HTranslation.GetContent("汉"), Font, Size.Empty, EditFlags).Width;
            if (e.X < cr.X) { _scrollX = Math.Max(0, _scrollX - step); }
            else if (e.X > cr.Right) { _scrollX += step; }
            ClampEditScroll();

            int idx = EditIndexFromPoint(e.Location);
            if (idx != _caret)
            {
                _caret = idx;
                EnsureCaretVisible();
                PositionCaret();
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (DroppedDown)
            {
                _drop.ScrollBy(-Math.Sign(e.Delta) * 3);
                ((HandledMouseEventArgs)e).Handled = true;
            }
        }

        #endregion

        #region 键盘交互

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                // 方向键默认被当作对话框键吞掉，必须显式声明为输入键，OnKeyDown 才收得到
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Home:
                case Keys.End:
                case Keys.Delete:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Alt+↓ 弹出、Alt+↑ 收起（Alt 组合属于对话框键，普通 KeyDown 收不到）
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                Keys code = keyData & Keys.KeyCode;
                if (code == Keys.Down) { ToggleDrop(); return true; }
                if (code == Keys.Up && DroppedDown) { CloseDrop(); return true; }
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!Enabled) return;

            switch (e.KeyCode)
            {
                case Keys.F4:
                    ToggleDrop();
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                case Keys.Down:
                    KeyDownNext(+1);
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                case Keys.Up:
                    KeyDownNext(-1);
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                case Keys.PageDown:
                    if (DroppedDown) _drop.MoveHover(_maxDropDownItems - 1);
                    else KeyDownNext(+1);
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                case Keys.PageUp:
                    if (DroppedDown) _drop.MoveHover(-(_maxDropDownItems - 1));
                    else KeyDownNext(-1);
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                case Keys.Enter:
                    if (DroppedDown) CommitDrop(_drop.HoverIndex);
                    else if (_dropDownStyle == HComboStyle.DropDown) CommitEditText();
                    e.Handled = e.SuppressKeyPress = true;
                    return;
                case Keys.Escape:
                    if (DroppedDown) CancelDrop();
                    e.Handled = e.SuppressKeyPress = true;
                    return;
            }

            // 以下为可编辑模式的完整文字编辑键
            if (_dropDownStyle != HComboStyle.DropDown) return;

            bool shift = e.Shift;
            bool ctrl = e.Control;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    EditMoveCaret(ctrl ? PrevWordBoundary(_caret) : _caret - 1, shift);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Right:
                    EditMoveCaret(ctrl ? NextWordBoundary(_caret) : _caret + 1, shift);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Home:
                    // 弹开时 Home/End 操作列表首末项（Windows ComboBox 语义），收起时移动插入符
                    if (DroppedDown) _drop.HoverIndex = 0;
                    else EditMoveCaret(0, shift);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.End:
                    if (DroppedDown) _drop.HoverIndex = _items.Count - 1;
                    else EditMoveCaret(_editText.Length, shift);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Back:
                    EditBackspace(ctrl);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Delete:
                    EditDeleteForward(ctrl);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Insert:
                    // Shift+Insert 粘贴、Ctrl+Insert 复制（Windows 经典快捷键）
                    if (shift) EditPaste();
                    else if (ctrl) EditCopy();
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                // 注意：只有 Ctrl 组合键才拦截；普通按 A/C/X/V/Z/Y 必须放行给 OnKeyPress 正常出字
                case Keys.A:
                    if (ctrl) { EditSelectAll(); e.Handled = e.SuppressKeyPress = true; }
                    break;
                case Keys.C:
                    if (ctrl) { EditCopy(); e.Handled = e.SuppressKeyPress = true; }
                    break;
                case Keys.X:
                    if (ctrl) { EditCut(); e.Handled = e.SuppressKeyPress = true; }
                    break;
                case Keys.V:
                    if (ctrl) { EditPaste(); e.Handled = e.SuppressKeyPress = true; }
                    break;
                case Keys.Z:
                    if (ctrl) { EditUndo(); e.Handled = e.SuppressKeyPress = true; }
                    break;
                case Keys.Y:
                    if (ctrl) { EditRedo(); e.Handled = e.SuppressKeyPress = true; }
                    break;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (!Enabled || char.IsControl(e.KeyChar)) return;

            if (_dropDownStyle == HComboStyle.DropDownList)
            {
                int found = IncrementalSearch(e.KeyChar);
                if (found >= 0)
                {
                    if (DroppedDown) _drop.HoverIndex = found;
                    else SetSelected(found, true);
                }
            }
            else
            {
                // 可打印字符（含 IME 上屏）写入编辑框；下拉打开时同步悬停项
                EditInsertString(e.KeyChar.ToString());
                if (DroppedDown)
                {
                    int match = FindPrefix(_editText, 0);
                    if (match >= 0) _drop.HoverIndex = match;
                }
            }
            e.Handled = true;
        }

        private void KeyDownNext(int delta)
        {
            if (DroppedDown)
            {
                _drop.MoveHover(delta);
                return;
            }
            if (_items.Count == 0) return;

            // Windows ComboBox 语义（真机对照原生控件确认）：收起状态 ↑/↓ 原地切换项、不弹列表。
            // 自定义文本（无选中项）时 ↓ 取首项、↑ 取末项；到达端点后不回绕。
            int target;
            if (delta > 0)
                target = _selectedIndex < 0 ? 0 : Math.Min(_items.Count - 1, _selectedIndex + 1);
            else
                target = _selectedIndex <= 0
                    ? (_selectedIndex < 0 ? _items.Count - 1 : 0)
                    : _selectedIndex - 1;

            if (_dropDownStyle == HComboStyle.DropDown)
            {
                // 可编辑模式：替换编辑文本并整串选中（插入符停在末尾），随后打字直接覆盖整串
                _editText = GetItemText(_items[target]);
                _anchor = 0;
                _caret = _editText.Length;
                _scrollX = 0;
                SetSelected(target, true);
                EnsureCaretVisible();
                PositionCaret();
                Invalidate();
                OnTextChanged(EventArgs.Empty);
            }
            else
            {
                // 仅选择模式：收起状态下方向键直接切换并提交
                SetSelected(target, true);
            }
        }

        private void ToggleDrop()
        {
            if (DroppedDown) CloseDrop();
            else OpenDrop();
        }

        #endregion

        #region 仅选择模式：增量前缀搜索

        private int IncrementalSearch(char ch)
        {
            if (DateTime.Now - _incrTick > TimeSpan.FromMilliseconds(800))
                _incrPrefix = string.Empty;
            _incrTick = DateTime.Now;
            _incrPrefix += char.ToLower(ch);

            int start = (_selectedIndex + 1) % Math.Max(1, _items.Count);
            for (int step = 0; step < _items.Count; step++)
            {
                int i = (start + step) % _items.Count;
                if (GetItemText(_items[i]).ToLower().StartsWith(_incrPrefix)) return i;
            }
            // 整串无匹配：以本次单字符重新起头
            _incrPrefix = ch.ToString().ToLower();
            return FindPrefix(_incrPrefix, 0);
        }

        private int FindPrefix(string prefix, int start)
        {
            for (int i = start; i < _items.Count; i++)
                if (GetItemText(_items[i]).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return i;
            return -1;
        }

        private int FindExact(string text)
        {
            for (int i = 0; i < _items.Count; i++)
                if (string.Equals(GetItemText(_items[i]), text, StringComparison.Ordinal)) return i;
            return -1;
        }

        #endregion

        #region 可编辑模式：几何、选区、编辑与插入符

        private int EditSelStart => Math.Min(_anchor, _caret);
        private int EditSelEnd => Math.Max(_anchor, _caret);
        private int EditSelLength => Math.Abs(_caret - _anchor);

        // 编辑串宽走 GDI GetTextExtentPoint32 严格步进宽（无 MeasureText 尾部余量），插入符紧贴字形。
        private int EditWidth(string s) => HDrawPaint.TextAdvance(s, Font);

        private int EditLineHeight => TextRenderer.MeasureText(HTranslation.GetContent("Ag中"), Font, Size.Empty, EditFlags).Height;

        /// <summary>编辑文字行矩形：按九宫格摆放；文字宽超过内容区时退化为左对齐横向滚动。</summary>
        private Rectangle EditLineRect()
        {
            var cr = ContentRect;
            int w = EditWidth(_editText);
            int h = EditLineHeight;

            int x;
            if (w >= cr.Width) x = cr.X - _scrollX;
            else
            {
                string a = _textAlign.ToString();
                if (a.EndsWith("Right")) x = cr.Right - w;
                else if (a.EndsWith("Center")) x = cr.X + (cr.Width - w) / 2;
                else x = cr.X;
            }

            int y;
            string va = _textAlign.ToString();
            if (va.StartsWith("Top")) y = cr.Y + 1;
            else if (va.StartsWith("Bottom")) y = cr.Bottom - h - 1;
            else y = cr.Y + (cr.Height - h) / 2;

            return new Rectangle(x, y, w, h);
        }

        /// <summary>全文索引对应的客户区坐标（插入符位置）。</summary>
        private Point EditPointFromIndex(int index)
        {
            index = Math.Max(0, Math.Min(index, _editText.Length));
            var lr = EditLineRect();
            return new Point(lr.X + EditWidth(_editText.Substring(0, index)), lr.Y);
        }

        /// <summary>鼠标客户区坐标 → 最近的字符插入位置。</summary>
        private int EditIndexFromPoint(Point p)
        {
            int localX = p.X - EditLineRect().X;
            if (localX <= 0) return 0;
            int lo = 0, hi = _editText.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (EditWidth(_editText.Substring(0, mid)) <= localX) lo = mid;
                else hi = mid - 1;
            }
            if (lo < _editText.Length)
            {
                int a = EditWidth(_editText.Substring(0, lo));
                int b = EditWidth(_editText.Substring(0, lo + 1));
                if (Math.Abs(localX - a) > Math.Abs(b - localX)) lo++;
            }
            return lo;
        }

        private void ClampEditScroll()
        {
            int max = Math.Max(0, EditWidth(_editText) - ContentRect.Width);
            _scrollX = Math.Max(0, Math.Min(_scrollX, max));
        }

        private void EnsureCaretVisible()
        {
            if (_dropDownStyle != HComboStyle.DropDown) return;
            var cr = ContentRect;
            int x = EditPointFromIndex(_caret).X;
            if (x < cr.X) _scrollX -= cr.X - x + 1;
            else if (x > cr.Right) _scrollX += x - cr.Right + 1;
            ClampEditScroll();
        }

        private void PositionCaret()
        {
            if (_dropDownStyle != HComboStyle.DropDown || !Focused || !IsHandleCreated) return;
            var lr = EditLineRect();
            int x = lr.X + EditWidth(_editText.Substring(0, _caret));
            if (!_caretCreated)
            {
                CreateCaret(Handle, IntPtr.Zero, 1, lr.Height);
                ShowCaret(Handle);
                _caretCreated = true;
            }
            SetCaretPos(x, lr.Y);
            // 同步告知 IME 插入符位置，拼音组合窗/候选窗贴在 | 周边（原生 ComboBox 行为）
            HDrawPaint.SetImeCompositionPoint(Handle, new Point(x, lr.Bottom));
        }

        /// <summary>IME 查询字符位置（TSF 输入法据此摆放候选窗）与开始组字时的定位；仅可编辑模式。</summary>
        protected override void WndProc(ref Message m)
        {
            if (_dropDownStyle == HComboStyle.DropDown &&
                HDrawPaint.TryAnswerImeCharPosition(ref m,
                    idx => EditPointFromIndex(Math.Max(0, Math.Min(idx, _editText.Length))),
                    EditLineHeight, ClientRectangle))
                return;
            if (m.Msg == HDrawPaint.WM_IME_STARTCOMPOSITION &&
                _dropDownStyle == HComboStyle.DropDown && Focused)
            {
                var lr = EditLineRect();
                int x = lr.X + EditWidth(_editText.Substring(0, Math.Max(0, Math.Min(_caret, _editText.Length))));
                HDrawPaint.SetImeCompositionPoint(Handle, new Point(x, lr.Bottom));
            }
            base.WndProc(ref m);
        }

        /// <summary>移动插入符（Shift 按下时扩展选区）。</summary>
        private void EditMoveCaret(int index, bool extend)
        {
            index = Math.Max(0, Math.Min(index, _editText.Length));
            if (!extend) _anchor = index;
            _caret = index;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        private void EditSelectAll()
        {
            _anchor = 0;
            _caret = _editText.Length;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        private int PrevWordBoundary(int index)
        {
            int i = index;
            while (i > 0 && char.IsWhiteSpace(_editText[i - 1])) i--;
            while (i > 0 && IsWordChar(_editText[i - 1])) i--;
            return i;
        }

        private int NextWordBoundary(int index)
        {
            int i = index;
            while (i < _editText.Length && char.IsWhiteSpace(_editText[i])) i++;
            while (i < _editText.Length && IsWordChar(_editText[i])) i++;
            return i;
        }

        private static bool IsWordChar(char c)
            => !char.IsWhiteSpace(c) && !char.IsControl(c) && !char.IsPunctuation(c);

        /// <summary>双击选中光标下的单词（非空白非标点的连续字符）。</summary>
        private void SelectWordAt(int index)
        {
            if (_editText.Length == 0) return;
            index = Math.Max(0, Math.Min(index, _editText.Length - 1));
            if (!IsWordChar(_editText[index]))
            {
                // 点在空格/标点上时就近选词：优先右侧词，再退回左侧词（原生编辑框语义）
                int r = index;
                while (r < _editText.Length && !IsWordChar(_editText[r])) r++;
                if (r < _editText.Length) index = r;
                else
                {
                    int l = index;
                    while (l >= 0 && !IsWordChar(_editText[l])) l--;
                    if (l < 0) return;
                    index = l;
                }
            }
            int s = index, e2 = index + 1;
            while (s > 0 && IsWordChar(_editText[s - 1])) s--;
            while (e2 < _editText.Length && IsWordChar(_editText[e2])) e2++;
            _anchor = s;
            _caret = e2;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        private void EditInsertString(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            // 单行编辑：粘贴的换行符直接丢弃
            input = input.Replace("\r", string.Empty).Replace("\n", string.Empty);
            if (input.Length == 0) return;

            int start = EditSelStart, len = EditSelLength;
            EditPushUndo(true);
            _editText = _editText.Remove(start, len).Insert(start, input);
            EditCommit(start + input.Length);
        }

        private void EditBackspace(bool word)
        {
            if (EditSelLength > 0) { EditDeleteRange(); return; }
            if (_caret == 0) return;
            int to = word ? PrevWordBoundary(_caret) : _caret - 1;
            EditPushUndo(false);
            _editText = _editText.Remove(to, _caret - to);
            EditCommit(to);
        }

        private void EditDeleteForward(bool word)
        {
            if (EditSelLength > 0) { EditDeleteRange(); return; }
            if (_caret >= _editText.Length) return;
            int to = word ? NextWordBoundary(_caret) : _caret + 1;
            EditPushUndo(false);
            _editText = _editText.Remove(_caret, to - _caret);
            EditCommit(_caret);
        }

        private void EditDeleteRange()
        {
            int start = EditSelStart, len = EditSelLength;
            if (len <= 0) return;
            EditPushUndo(false);
            _editText = _editText.Remove(start, len);
            EditCommit(start);
        }

        /// <summary>编辑后统一收尾：端点归位、滚动、插入符、选中项同步与事件。</summary>
        private void EditCommit(int caret)
        {
            _anchor = _caret = caret;
            EnsureCaretVisible();
            PositionCaret();
            SyncSelectionWithEdit();
            Invalidate();
            OnTextChanged(EventArgs.Empty);
        }

        /// <summary>连续单字符输入 800ms 内合并为一次撤销。</summary>
        private void EditPushUndo(bool typed)
        {
            DateTime now = DateTime.Now;
            if (!(typed && (now - _lastCharEdit).TotalMilliseconds < 800 && _editUndo.Count > 0))
                _editUndo.Push(new EditSnapshot(_editText, _anchor, _caret));
            _lastCharEdit = now;
            _editRedo.Clear();
        }

        private void EditUndo()
        {
            if (_editUndo.Count == 0) return;
            _lastCharEdit = DateTime.MinValue;
            _editRedo.Push(new EditSnapshot(_editText, _anchor, _caret));
            ApplyEditSnapshot(_editUndo.Pop());
        }

        private void EditRedo()
        {
            if (_editRedo.Count == 0) return;
            _lastCharEdit = DateTime.MinValue;
            _editUndo.Push(new EditSnapshot(_editText, _anchor, _caret));
            ApplyEditSnapshot(_editRedo.Pop());
        }

        private void ApplyEditSnapshot(EditSnapshot s)
        {
            _editText = s.Text;
            _anchor = Math.Max(0, Math.Min(s.Anchor, _editText.Length));
            _caret = Math.Max(0, Math.Min(s.Caret, _editText.Length));
            ClampEditScroll();
            PositionCaret();
            SyncSelectionWithEdit();
            Invalidate();
            OnTextChanged(EventArgs.Empty);
        }

        private void EditCopy()
        {
            if (EditSelLength > 0)
                Clipboard.SetText(_editText.Substring(EditSelStart, EditSelLength), TextDataFormat.UnicodeText);
        }

        private void EditCut()
        {
            if (EditSelLength == 0) return;
            EditCopy();
            EditDeleteRange();
        }

        private void EditPaste()
        {
            if (Clipboard.ContainsText())
                EditInsertString(Clipboard.GetText(TextDataFormat.UnicodeText));
        }

        /// <summary>编辑文本与某项完全一致（忽略大小写）时同步选中项，否则清空选中。</summary>
        private void SyncSelectionWithEdit()
        {
            int match = -1;
            for (int i = 0; i < _items.Count; i++)
            {
                if (string.Equals(GetItemText(_items[i]), _editText, StringComparison.OrdinalIgnoreCase))
                {
                    match = i;
                    break;
                }
            }
            if (match != _selectedIndex) SetSelected(match, false);
        }

        /// <summary>回车确认输入：精确匹配则提交选中项，否则保留原文。</summary>
        private void CommitEditText()
        {
            int exact = FindExact(_editText);
            if (exact >= 0) CommitDrop(exact);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
            // Windows ComboBox 语义（含 Tab 获焦）：进入编辑区时整串文本全选，插入符停在末尾
            if (_dropDownStyle == HComboStyle.DropDown) EditSelectAll();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            CloseDrop();
            DestroyCaret();
            _caretCreated = false;
            // 离开控件时按文本匹配一次选中项（Windows ComboBox 语义）
            if (_dropDownStyle == HComboStyle.DropDown && _selectedIndex < 0 && _editText.Length > 0)
            {
                int match = FindPrefix(_editText, 0);
                if (match >= 0 && string.Equals(GetItemText(_items[match]), _editText, StringComparison.OrdinalIgnoreCase))
                    SetSelected(match, true);
            }
            Invalidate();
        }

        #endregion

        #region 绘制

        /// <summary>
        /// 圆角分支在背景阶段铺父色（外扩 1px 防双缓冲黑晕）并填外形底色；
        /// 直角分支走系统默认不透明擦除。
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (_radius <= 0)
            {
                base.OnPaintBackground(pevent);
                return;
            }
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            HDrawPaint.FillParentBackdrop(g, Width, Height, Parent?.BackColor ?? SystemColors.Control);
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            using (var brush = new SolidBrush(Enabled ? ShapeFillColor : SystemColors.Control))
                g.FillPath(brush, path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var btn = ButtonRect;
            bool btnDown = _buttonPressed || DroppedDown;

            if (_radius <= 0)
            {
                using (var brush = new SolidBrush(Enabled ? ShapeFillColor : SystemColors.Control))
                    g.FillRectangle(brush, -1, -1, Width + 2, Height + 2);
                ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Sunken);
                ControlPaint.DrawBorder3D(g, new Rectangle(btn.X, 1, btn.Width - 1, Height - 2),
                    btnDown ? Border3DStyle.Sunken : Border3DStyle.Raised);
            }
            else
            {
                // 父色铺底与外形填白在 OnPaintBackground 完成；这里只画向内填充的边框环
                if (_borderWidth > 0)
                {
                    Color border = !Enabled ? SystemColors.ControlDark :
                        _hover || Focused ? SystemColors.Highlight : SystemColors.ControlDarkDark;
                    HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, _borderWidth, border);
                }
                // 圆角模式下箭头区按下底色与竖分隔线（随边框宽度内让，bw=1 时与旧几何一致）
                var btnInner = new Rectangle(btn.X, _borderWidth + 1, btn.Width - 2, Height - 2 * (_borderWidth + 1));
                if (btnDown)
                {
                    using (GraphicsPath bp = HDrawPaint.CreatePath(
                            btnInner, Math.Max(2, _radius - _borderWidth - 1), HRoundStyle.All))
                    using (var bb = new SolidBrush(Color.FromArgb(229, 241, 251)))
                        g.FillPath(bb, bp);
                }
                using (var pen = new Pen(Color.FromArgb(180, 180, 180), 1f))
                    g.DrawLine(pen, btn.X, _borderWidth + 3, btn.X, Height - _borderWidth - 4);
            }

            DrawArrow(g, btn, btnDown);
            DrawContent(g);
        }

        /// <summary>画下拉箭头：弹出时朝上，否则朝下。</summary>
        private void DrawArrow(Graphics g, Rectangle btn, bool btnDown)
        {
            int cx = btn.X + btn.Width / 2;
            int cy = btn.Height / 2 + (btnDown ? 1 : 0);
            int s = 4;
            Point[] tri = DroppedDown
                ? new[] { new Point(cx - s, cy + 2), new Point(cx + s, cy + 2), new Point(cx, cy - 3) }
                : new[] { new Point(cx - s, cy - 2), new Point(cx + s, cy - 2), new Point(cx, cy + 3) };
            using (var brush = new SolidBrush(Enabled ? SystemColors.ControlDarkDark : SystemColors.GrayText))
                g.FillPolygon(brush, tri);
        }

        private void DrawContent(Graphics g)
        {
            var cr = ContentRect;
            if (_dropDownStyle == HComboStyle.DropDown)
            {
                // 系统插入符由系统绘制，这里只负责选区高亮与文字；裁剪防止横向滚动时文字溢出
                Color fg = Enabled ? ForeColor : SystemColors.GrayText;
                var oldClip = g.Clip;
                g.SetClip(cr, CombineMode.Replace);
                var lr = EditLineRect();
                int gs = Focused ? EditSelStart : 0;
                int ge = Focused ? EditSelEnd : 0;

                // Pass 1（GDI+）：选区高亮
                if (ge > gs)
                {
                    int sx = lr.X + EditWidth(_editText.Substring(0, gs));
                    int ex = lr.X + EditWidth(_editText.Substring(0, ge));
                    using (var b = new SolidBrush(SystemColors.Highlight))
                        g.FillRectangle(b, new Rectangle(sx, lr.Y, ex - sx, lr.Height));
                }

                // Pass 2（GDI ExtTextOut）：选区把一行拆成“前段-选中段-后段”。
                // 插入符按 EditWidth(GetTextExtentPoint32) 定位，绘制必须同引擎，
                // 否则引号等 overhang 字符会出现 DrawTextEx 特有的额外空隙。
                IntPtr hdc = g.GetHdc();
                try
                {
                    using (var tc = HDrawPaint.BeginGdiText(hdc, Font))
                    {
                        DrawEditRun(tc, 0, gs, lr.X, lr.Y, fg);
                        if (ge > gs)
                            DrawEditRun(tc, gs, ge, lr.X + EditWidth(_editText.Substring(0, gs)), lr.Y, SystemColors.HighlightText);
                        DrawEditRun(tc, ge, _editText.Length, lr.X + EditWidth(_editText.Substring(0, ge)), lr.Y, fg);
                    }
                }
                finally { g.ReleaseHdc(hdc); }
                g.Clip = oldClip;
            }
            else
            {
                string text = SelectedText;
                if (text.Length > 0)
                {
                    // 仅选择样式：九宫格对齐 + 超长省略；同样走 GDI 与编辑态观感一致
                    Color color = Enabled ? ForeColor : SystemColors.GrayText;
                    var oldClip = g.Clip;
                    g.SetClip(cr, CombineMode.Replace);
                    string fit = HDrawPaint.EllipsisToFit(text, Font, cr.Width);
                    int w = EditWidth(fit);
                    int h = EditLineHeight;
                    string a = _textAlign.ToString();
                    int x = a.EndsWith("Right") ? cr.Right - w
                          : a.EndsWith("Center") ? cr.X + (cr.Width - w) / 2
                          : cr.X;
                    int y = a.StartsWith("Top") ? cr.Y + 1
                          : a.StartsWith("Bottom") ? cr.Bottom - h - 1
                          : cr.Y + (cr.Height - h) / 2;
                    IntPtr hdc = g.GetHdc();
                    try
                    {
                        using (var tc = HDrawPaint.BeginGdiText(hdc, Font))
                            tc.DrawRun(fit, x, y, color);
                    }
                    finally { g.ReleaseHdc(hdc); }
                    g.Clip = oldClip;
                }
            }

            if (Focused && ShowFocusCues && _dropDownStyle == HComboStyle.DropDownList)
                ControlPaint.DrawFocusRectangle(g, new Rectangle(3, 3, Width - ButtonRect.Width - 5, Height - 6));
        }

        private void DrawEditRun(HDrawPaint.GdiTextContext tc, int from, int to, int x, int y, Color color)
        {
            if (to <= from) return;
            tc.DrawRun(_editText.Substring(from, to - from), x, y, color);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (_drop != null) _drop.Font = Font;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
            PositionCaret();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if (!Enabled) CloseDrop();
            Invalidate();
        }

        /// <summary>
        /// 圆角不再裁剪窗口区域：曲线路径转 HRgn 会沿弧线保守舍入削掉 1~2px，圆角出折角。
        /// 四角由 OnPaint 铺父色透出；异形自绘的派生类仍可覆盖为空。
        /// </summary>
        protected virtual void UpdateRegion()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _drop?.Dispose();
                _drop = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region 弹出列表窗（不激活、不夺焦、自绘列表与滚动条）

        private sealed class HDropForm : Form
        {
            private readonly HComboBoxBase _owner;
            private int _itemH = 24;
            private int _visible = 8;
            private int _hover = -1;
            private int _topIndex;
            private bool _sbVisible;
            private Rectangle _sbTrack, _sbThumb;
            private bool _sbDragging;
            private bool _prevOutsideDown;
            private readonly Timer _watch;

            public HDropForm(HComboBoxBase owner)
            {
                _owner = owner;
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.Manual;
                AutoScaleMode = AutoScaleMode.None;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                _watch = new Timer { Interval = 12 };
                _watch.Tick += WatchOutsideClick;
            }

            /// <summary>不夺取焦点，键盘消息继续由主控件处理。</summary>
            protected override bool ShowWithoutActivation => true;

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                    cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                    return cp;
                }
            }

            public int HoverIndex
            {
                get => _hover;
                set
                {
                    _hover = Math.Max(0, Math.Min(_owner._items.Count - 1, value));
                    EnsureVisible(_hover);
                    Invalidate();
                }
            }

            public void Prepare(int width, int height, int itemH, int visibleCount)
            {
                _itemH = itemH;
                _visible = visibleCount;
                _topIndex = Math.Min(_topIndex, Math.Max(0, _owner._items.Count - _visible));
                Size = new Size(width, height);
                if (_owner._radius <= 0)
                {
                    Region = null;
                }
                else
                {
                    // 顶层弹窗必须裁剪外形：Region 与填充共用像素盒外轮廓，避免圆角外露出直角块
                    Region = new Region(HDrawPaint.CreateOuterBoxPath(width, height, _owner._radius));
                }
                CalcScrollbar();
            }

            public void MoveHover(int delta)
            {
                HoverIndex = _hover + delta;
            }

            public void ScrollBy(int lines)
            {
                _topIndex = Math.Max(0, Math.Min(Math.Max(0, _owner._items.Count - _visible), _topIndex + lines));
                CalcScrollbar();
                Invalidate();
            }

            public void StopWatch() => _watch.Stop();

            private void EnsureVisible(int index)
            {
                if (index < _topIndex) _topIndex = index;
                else if (index >= _topIndex + _visible) _topIndex = index - _visible + 1;
                _topIndex = Math.Max(0, Math.Min(Math.Max(0, _owner._items.Count - _visible), _topIndex));
                CalcScrollbar();
            }

            private void CalcScrollbar()
            {
                int count = _owner._items.Count;
                _sbVisible = count > _visible;
                if (!_sbVisible) return;
                _sbTrack = new Rectangle(Width - 9, 2, 7, Height - 4);
                int thumbH = Math.Max(24, _sbTrack.Height * _visible / count);
                int range = Math.Max(1, _sbTrack.Height - thumbH);
                int thumbY = _sbTrack.Y + (count <= _visible ? 0 :
                    _topIndex * range / (count - _visible));
                _sbThumb = new Rectangle(_sbTrack.X, thumbY, 7, thumbH);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                int listWidth = Width - (_sbVisible ? 10 : 2);
                // 圆角弹窗只在外形路径内铺白底，外形外交给窗口 Region 裁掉
                if (_owner._radius <= 0)
                {
                    using (var brush = new SolidBrush(SystemColors.Window))
                        g.FillRectangle(brush, ClientRectangle);
                }
                else
                {
                    using (GraphicsPath shape = HDrawPaint.CreateOuterBoxPath(Width, Height, _owner._radius))
                    using (var brush = new SolidBrush(SystemColors.Window))
                        g.FillPath(brush, shape);
                }

                int count = _owner._items.Count;
                for (int i = _topIndex; i < Math.Min(count, _topIndex + _visible); i++)
                {
                    var row = new Rectangle(1, 1 + (i - _topIndex) * _itemH, listWidth, _itemH);
                    bool hovered = i == _hover;
                    bool selected = i == _owner._selectedIndex;
                    Color back = hovered ? SystemColors.Highlight :
                        selected ? Color.FromArgb(234, 242, 251) : SystemColors.Window;
                    Color fore = hovered ? SystemColors.HighlightText : SystemColors.WindowText;
                    using (var bb = new SolidBrush(back))
                        g.FillRectangle(bb, row);
                    var textRect = new Rectangle(row.X + 7, row.Y, row.Width - 12, row.Height);
                    TextRenderer.DrawText(g, _owner.GetItemText(_owner._items[i]), Font, textRect, fore, ItemFlags);
                }

                if (_sbVisible)
                {
                    using (var tb = new SolidBrush(Color.FromArgb(220, 220, 220)))
                        g.FillRectangle(tb, _sbTrack);
                    using (var hb = new SolidBrush(Color.FromArgb(140, 140, 140)))
                        g.FillRectangle(hb, _sbThumb);
                }

                // 边框（圆角跟随主控件，0/负数为直角；外形 Region 已在 Prepare 中裁剪）
                if (_owner._radius <= 0)
                {
                    using (var pen = new Pen(SystemColors.ControlDark, 1f))
                        g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
                else if (_owner._borderWidth > 0)
                {
                    // 与主控件同一套像素盒填充环，粗边同样完整、四边等宽
                    HDrawPaint.FillRoundedBorder(g, Width, Height,
                        _owner._radius, _owner._borderWidth, SystemColors.ControlDark);
                }
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);
                if (Visible)
                {
                    CalcScrollbar();
                    _prevOutsideDown = false;
                    _watch.Start();
                }
                else _watch.Stop();
            }

            private int IndexFromY(int y)
            {
                int idx = (y - 1) / _itemH + _topIndex;
                return Math.Max(0, Math.Min(_owner._items.Count - 1, idx));
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button != MouseButtons.Left) return;

                if (_sbVisible && _sbTrack.Contains(e.Location))
                {
                    if (_sbThumb.Contains(e.Location))
                    {
                        _sbDragging = true;
                        Capture = true;
                    }
                    else
                    {
                        ScrollBy(e.Y < _sbThumb.Y ? -(_visible - 1) : (_visible - 1));
                    }
                    return;
                }
                _owner.CommitDrop(IndexFromY(e.Y));
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (_sbDragging)
                {
                    int count = _owner._items.Count;
                    int range = Math.Max(1, _sbTrack.Height - _sbThumb.Height);
                    _topIndex = Math.Max(0, Math.Min(count - _visible,
                        (e.Y - _sbTrack.Y - _sbThumb.Height / 2) * (count - _visible) / range));
                    CalcScrollbar();
                    Invalidate();
                    return;
                }
                int idx = IndexFromY(e.Y);
                if (idx != _hover)
                {
                    _hover = idx;
                    Invalidate();
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (_sbDragging)
                {
                    _sbDragging = false;
                    Capture = false;
                }
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);
                ScrollBy(-Math.Sign(e.Delta) * 3);
            }

            /// <summary>轮询外部鼠标按下：在列表与主控件之外按下则取消关闭（窗口不激活，收不到失焦消息）。</summary>
            private void WatchOutsideClick(object sender, EventArgs e)
            {
                bool down = (GetAsyncKeyState(0x01) & 0x8000) != 0 ||
                            (GetAsyncKeyState(0x02) & 0x8000) != 0;
                if (down && !_prevOutsideDown)
                {
                    var p = Cursor.Position;
                    var ownerScreen = _owner.RectangleToScreen(_owner.ClientRectangle);
                    if (!Bounds.Contains(p) && !ownerScreen.Contains(p))
                        _owner.CancelDrop();
                }
                _prevOutsideDown = down;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _watch.Dispose();
                base.Dispose(disposing);
            }
        }

        #endregion
    }

    /// <summary>下拉框选项集合：内部以 object 保存，显示时取 ToString；设计器以字符串集合方式编辑。</summary>
    public sealed class HComboItemCollection : IList
    {
        private readonly List<object> _list = new List<object>();
        private readonly HComboBoxBase _owner;

        internal HComboItemCollection(HComboBoxBase owner)
        {
            _owner = owner;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => ((ICollection)_list).SyncRoot;

        public object this[int index]
        {
            get => _list[index];
            set { _list[index] = value; _owner.ItemsChanged(); }
        }

        public int Add(object value)
        {
            _list.Add(value);
            _owner.ItemsChanged();
            return _list.Count - 1;
        }

        /// <summary>批量添加字符串选项。</summary>
        public void AddRange(params string[] values)
        {
            _list.AddRange(values.Cast<object>());
            _owner.ItemsChanged();
        }

        public void Clear()
        {
            if (_list.Count == 0) return;
            _list.Clear();
            _owner.ItemsChanged();
        }

        public bool Contains(object value) => _list.Contains(value);
        public int IndexOf(object value) => _list.IndexOf(value);

        public void Insert(int index, object value)
        {
            _list.Insert(index, value);
            _owner.ItemsChanged();
        }

        public void Remove(object value)
        {
            if (_list.Remove(value)) _owner.ItemsChanged();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            _owner.ItemsChanged();
        }

        public void CopyTo(Array array, int index) => ((ICollection)_list).CopyTo(array, index);
        public IEnumerator GetEnumerator() => _list.GetEnumerator();
    }
}