using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{
    using HFromUI.HLangage;
    /// <summary>
    /// 日期时间显示串中的一个可编辑数字段（yyyy / MM / dd / HH / mm / ss / fff）。
    /// Start/Length 为其在显示串中的位置与位数，Min/Max 为取值范围，Wrap 决定增减越界时回绕还是夹取。
    /// </summary>
    public sealed class HDateTimeField
    {
        /// <summary>在显示串中的起始字符下标。</summary>
        public int Start;
        /// <summary>数字位数。</summary>
        public int Length;
        /// <summary>最小值。</summary>
        public int Min;
        /// <summary>最大值（随当前值动态变化时由字段提供方每次重建）。</summary>
        public int Max;
        /// <summary>增减越界时是否回绕（时/分/秒/毫秒回绕，年/月/日夹取）。</summary>
        public bool Wrap;

        public HDateTimeField(int start, int length, int min, int max, bool wrap)
        {
            Start = start;
            Length = length;
            Min = min;
            Max = max;
            Wrap = wrap;
        }
    }

    /// <summary>
    /// 日期时间分段编辑框基类：仿 Windows 日期时间选取框的纯自绘实现。
    /// 显示串由派生类按格式提供（如 "yyyy-MM-dd"、"HH:mm:ss.fff"），数字段可点选，
    /// 支持 ↑/↓ 与鼠标滚轮增减（回绕/夹取按字段定义）、←/→ 切换字段、直接键入数字
    /// （输满自动跳下一段，停顿约 0.9 秒提交），右侧可选上下微调钮或下拉箭头。
    /// 获焦后显示与系统文本框一致的闪烁插入符，键入中的数字即时显示、插入符随输入移动。
    /// Borderless 无边框嵌入模式供范围选择控件内嵌两个编辑框使用。
    /// </summary>
    [DefaultEvent("ValueEdited")]
    public class HDateTimeFieldBox : Control
    {
        private const TextFormatFlags TFlags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

        #region Win32 系统插入符

        [DllImport("user32.dll")]
        private static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
        [DllImport("user32.dll")]
        private static extern bool DestroyCaret();
        [DllImport("user32.dll")]
        private static extern bool SetCaretPos(int X, int Y);
        [DllImport("user32.dll")]
        private static extern bool ShowCaret(IntPtr hWnd);

        #endregion

        private Func<string> _textProvider;
        private Func<HDateTimeField[]> _fieldsProvider;
        private Action<int, int> _applyField;

        private int _selected;
        private int _radius = 6;
        private int _borderWidth = 1;
        private Color _shapeBackColor = Color.Empty;
        private bool _hover;
        private bool _showSpin;
        private bool _showDropArrow;
        private bool _borderless;
        private bool _popupOpen;
        private bool _caretCreated;
        private Graphics _measureGraphics;
        private IntPtr _measureHdc;

        // 微调钮长按连发
        private readonly Timer _spinTimer;
        private int _spinDir;
        private bool _spinSlow;
        private bool _spinPressed;
        private bool _dropPressed;

        // 数字键入缓冲（满位或超时提交）
        private readonly Timer _digitTimer;
        private int _digitField = -1;
        private string _digitBuffer = string.Empty;

        public HDateTimeFieldBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);
            BackColor = Color.White;
            ForeColor = Color.FromArgb(48, 52, 59);
            Font = new Font("微软雅黑", 9f);
            Size = new Size(150, 30);
            TabStop = true;
            _spinTimer = new Timer { Interval = 400 };
            _spinTimer.Tick += SpinTick;
            _digitTimer = new Timer { Interval = 900 };
            _digitTimer.Tick += (s, e) => { CommitPendingDigit(); PositionCaret(); Invalidate(); };
        }

        /// <summary>强调色（选中段、聚焦边框、微调/箭头悬停统一橙色主调）。</summary>
        public Color AccentColor { get; set; } = Color.FromArgb(249, 115, 22);

        /// <summary>圆角半径（0 为直角）。</summary>
        [DefaultValue(6)]
        public int Radius
        {
            get => _radius;
            set { _radius = Math.Max(0, value); UpdateRegion(); Invalidate(); }
        }

        /// <summary>边框宽度（像素）。</summary>
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = Math.Max(1, value); UpdateRegion(); Invalidate(); }
        }

        /// <summary>圆角/椭圆/胶囊外形内部的背景色；Color.Empty 表示沿用 BackColor，颜色只在外形内、圆角外不染色。</summary>
        [DefaultValue(typeof(Color), "")]
        public Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }

        /// <summary>右侧是否显示上下微调钮（时间控件）。</summary>
        [DefaultValue(false)]
        public bool ShowSpin
        {
            get => _showSpin;
            set { _showSpin = value; Invalidate(); }
        }

        /// <summary>右侧是否显示下拉箭头（日期控件）。</summary>
        [DefaultValue(false)]
        public bool ShowDropArrow
        {
            get => _showDropArrow;
            set { _showDropArrow = value; Invalidate(); }
        }

        /// <summary>无边框嵌入模式：透明底、不描边，供范围控件内嵌。</summary>
        [DefaultValue(false)]
        public bool Borderless
        {
            get => _borderless;
            set
            {
                _borderless = value;
                BackColor = value ? Color.Transparent : Color.White;
                UpdateRegion();
                Invalidate();
            }
        }

        /// <summary>关联弹出层是否打开（打开时边框为强调色、箭头朝上）。</summary>
        [Browsable(false)]
        public bool IsPopupOpen
        {
            get => _popupOpen;
            set { _popupOpen = value; Invalidate(); }
        }

        /// <summary>当前选中的字段下标。</summary>
        [Browsable(false)]
        public int SelectedField
        {
            get => _selected;
            set => SelectField(value);
        }

        /// <summary>点击下拉箭头（含 Alt+↓ / F4）。</summary>
        public event EventHandler DropClicked;

        /// <summary>弹层打开时按下 Esc/Enter（范围控件内嵌场景下由外层容器接管关闭日历）。</summary>
        public event EventHandler PopupCloseRequested;

        /// <summary>字段值被增减或键入提交后触发。</summary>
        public event EventHandler ValueEdited;

        /// <summary>
        /// 派生类绑定显示串与字段读写：textProvider 返回完整格式串，fieldsProvider 返回当前各数字段，
        /// applyField(fieldIndex, newValue) 负责把夹取/回绕后的新值写回实际数据。
        /// </summary>
        protected void Initialize(Func<string> textProvider, Func<HDateTimeField[]> fieldsProvider,
            Action<int, int> applyField)
        {
            _textProvider = textProvider;
            _fieldsProvider = fieldsProvider;
            _applyField = applyField;
        }

        /// <summary>通知值已被编辑：复位键入缓冲、重绘并触发事件。</summary>
        protected void RaiseValueEdited()
        {
            ResetDigitBuffer();
            PositionCaret();
            Invalidate();
            ValueEdited?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>把当前字段增减 delta（跨字段进位由 BumpField 的派生重写决定）。</summary>
        public void Bump(int delta)
        {
            if (!Enabled || _textProvider == null || delta == 0) return;
            var f = CurrentField();
            if (f == null) return;
            BumpField(_selected, delta);
            RaiseValueEdited();
        }

        /// <summary>
        /// 对指定字段施加增减：默认按字段定义在本字段内回绕/夹取（不跨字段进位）；
        /// 派生类可重写实现 Windows 式进位（月日跨月、时分秒毫秒向高位进位）。
        /// </summary>
        protected virtual void BumpField(int field, int delta)
        {
            var f = _fieldsProvider()[field];
            string text = _textProvider();
            int v = int.Parse(text.Substring(f.Start, f.Length));
            int nv;
            if (f.Wrap)
            {
                int range = f.Max - f.Min + 1;
                nv = f.Min + ((v - f.Min + delta) % range + range) % range;
            }
            else
            {
                nv = Math.Max(f.Min, Math.Min(f.Max, v + delta));
            }
            _applyField(field, nv);
        }

        /// <summary>选中指定字段（越界自动夹取）。</summary>
        public void SelectField(int index)
        {
            var fields = _fieldsProvider?.Invoke();
            if (fields == null || fields.Length == 0) return;
            _selected = Math.Max(0, Math.Min(fields.Length - 1, index));
            ResetDigitBuffer();
            PositionCaret();
            Invalidate();
        }

        /// <summary>
        /// 按当前选中字段与键入缓冲定位系统闪烁插入符：整段选中时竖线在段尾，
        /// 键入中竖线跟随已输入数字。与 HTextBoxBase 使用同一套系统插入符。
        /// </summary>
        private void PositionCaret()
        {
            if (!Focused || !IsHandleCreated || _textProvider == null) return;
            var f = CurrentField();
            if (f == null) return;
            string text = _textProvider();
            if (text.Length == 0) return;
            int[] xs = CharOrigins(text);
            var cr = ContentRect;
            int x = FieldEditing
                ? cr.X + xs[f.Start + Math.Min(_digitBuffer.Length, f.Length)]
                : cr.X + xs[Math.Min(f.Start + f.Length, xs.Length - 1)];
            int lineH = LineHeight;
            int y = (Height - lineH) / 2 + 1;
            if (!_caretCreated)
            {
                CreateCaret(Handle, IntPtr.Zero, 1, Math.Max(1, lineH - 2));
                ShowCaret(Handle);
                _caretCreated = true;
            }
            SetCaretPos(x, y);
        }

        /// <summary>F4 / Alt+↓ / Esc / Enter 等对话框键钩子：派生类可重写以控制弹出层。</summary>
        protected virtual bool OnDialogKey(Keys keyData) => false;

        #region 布局与命中

        private int RightZoneWidth => _showSpin ? 20 : (_showDropArrow ? 28 : 0);

        private Rectangle SpinZone => new Rectangle(Width - 20, 0, 20, Height);
        private Rectangle DropZone => new Rectangle(Width - 28, 0, 28, Height);

        private Rectangle ContentRect
        {
            get
            {
                int left = _borderless ? 2 : 9;
                int right = RightZoneWidth + (_borderless ? 2 : 8);
                return new Rectangle(left, 0, Math.Max(0, Width - left - right), Height);
            }
        }

        private int LineHeight => TextRenderer.MeasureText(HTranslation.GetContent("Ag中"), Font, Size.Empty, TFlags).Height;

        /// <summary>窗口 DC 上的严格步进宽（与实际绘制同 DPI，跨显示器/大字号下插入符不漂移）。</summary>
        private int TextWidth(string s)
        {
            EnsureMeasureHdc();
            return HDrawPaint.TextAdvance(s, Font, _measureHdc);
        }

        private void EnsureMeasureHdc()
        {
            if (_measureHdc != IntPtr.Zero || !IsHandleCreated) return;
            _measureGraphics = Graphics.FromHwnd(Handle);
            _measureHdc = _measureGraphics.GetHdc();
        }

        private void ReleaseMeasureHdc()
        {
            if (_measureGraphics == null) return;
            try { _measureGraphics.ReleaseHdc(_measureHdc); } catch { }
            _measureGraphics.Dispose();
            _measureGraphics = null;
            _measureHdc = IntPtr.Zero;
        }

        /// <summary>逐字符测量每个字符的起点 X（相对内容区左上）。</summary>
        private int[] CharOrigins(string text)
        {
            // 必须用【整串前缀】宽度定位，不能逐字符测量再累加：
            // GetTextExtentPoint32 对单字符返回 A+B+C（含尾部悬垂），而串内字形按 A+B 推进、
            // 后字的 A 悬垂与前字的 C 悬垂相互重叠（如数字 '1' 单量 14px、串内步进仅 9px）。
            // 逐字累加会把 1/7 等 overhang 数字排得偏松，且与 ExtTextOut 实绘位置错开。
            var xs = new int[text.Length + 1];
            for (int i = 0; i < text.Length; i++)
                xs[i + 1] = TextWidth(text.Substring(0, i + 1));
            return xs;
        }

        /// <summary>当前选中字段是否处于逐位编辑态（键入中或 Backspace 删除中；缓冲为空表示整段被删空，等待重新输入）。</summary>
        private bool FieldEditing => _digitField >= 0 && _digitField == _selected;

        private HDateTimeField CurrentField()
        {
            var fields = _fieldsProvider?.Invoke();
            if (fields == null || fields.Length == 0) return null;
            _selected = Math.Max(0, Math.Min(fields.Length - 1, _selected));
            return fields[_selected];
        }

        /// <summary>按客户区 X 命中字段，未命中返回 -1。</summary>
        private int FieldAtX(int x)
        {
            if (_textProvider == null) return -1;
            string text = _textProvider();
            var fields = _fieldsProvider();
            var xs = CharOrigins(text);
            int baseX = ContentRect.X;
            for (int i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                if (x >= baseX + xs[f.Start] - 4 && x <= baseX + xs[f.Start + f.Length] + 4)
                    return i;
            }
            return -1;
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
            // 鼠标移出控件即把键入/退格中的数字定稿（自动补全），不必停顿等超时；
            // 缓冲为空（整段删空）时回退原值，日期始终保持完整
            if (FieldEditing)
            {
                CommitPendingDigit();
                PositionCaret();
            }
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled || e.Button != MouseButtons.Left) return;
            Focus();

            if (_showSpin && SpinZone.Contains(e.Location))
            {
                int mid = Height / 2;
                _spinDir = e.Y < mid ? +1 : -1;
                _spinPressed = true;
                _spinSlow = true;
                Capture = true;
                CommitPendingDigit();
                Bump(_spinDir);
                _spinTimer.Interval = 400;
                _spinTimer.Start();
                Invalidate();
                return;
            }

            if (_showDropArrow && DropZone.Contains(e.Location))
            {
                _dropPressed = true;
                Capture = true;
                Invalidate();
                return;
            }

            int hit = FieldAtX(e.X);
            if (hit >= 0)
            {
                CommitPendingDigit();
                SelectField(hit);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            StopSpin();
            if (_dropPressed)
            {
                bool inside = DropZone.Contains(e.Location);
                _dropPressed = false;
                Capture = false;
                Invalidate();
                if (inside) DropClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hand = (_showSpin && SpinZone.Contains(e.Location)) ||
                        (_showDropArrow && DropZone.Contains(e.Location));
            Cursor = hand ? Cursors.Hand : Cursors.IBeam;
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            StopSpin();
            _dropPressed = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (Enabled)
            {
                Bump(e.Delta > 0 ? +1 : -1);
                ((HandledMouseEventArgs)e).Handled = true;
            }
        }

        private void StopSpin()
        {
            if (!_spinPressed) return;
            _spinPressed = false;
            _spinTimer.Stop();
            Capture = false;
            Invalidate();
        }

        private void SpinTick(object sender, EventArgs e)
        {
            Bump(_spinDir);
            if (_spinSlow)
            {
                _spinSlow = false;
                _spinTimer.Interval = 80;
            }
        }

        #endregion

        #region 键盘交互

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (OnDialogKey(keyData)) return true;
            Keys code = keyData & Keys.KeyCode;
            // 弹层打开时 Esc/Enter 请求关闭（单控件由派生类 OnDialogKey 处理，内嵌框由外层容器订阅）
            if (_popupOpen && (code == Keys.Escape || code == Keys.Enter))
            {
                PopupCloseRequested?.Invoke(this, EventArgs.Empty);
                return true;
            }
            // Alt+↓ 与 F4 触发下拉动作（范围控件内嵌时由外层容器接管，即使箭头隐藏也转发）
            if (keyData == (Keys.Alt | Keys.Down) || code == Keys.F4)
            {
                OnDropClicked();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        /// <summary>下拉动作触发点（箭头点击或 Alt+↓ / F4）。</summary>
        protected virtual void OnDropClicked() => DropClicked?.Invoke(this, EventArgs.Empty);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!Enabled) return;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    CommitPendingDigit();
                    SelectField(_selected - 1);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Right:
                    CommitPendingDigit();
                    SelectField(_selected + 1);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    CommitPendingDigit();
                    Bump(+1);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Down:
                    CommitPendingDigit();
                    Bump(-1);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Home:
                    // 同 Windows 日期时间框：Home 跳到第一个字段
                    CommitPendingDigit();
                    SelectField(0);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.End:
                    // End 跳到最后一个字段
                    CommitPendingDigit();
                    SelectField((_fieldsProvider?.Invoke()?.Length ?? 1) - 1);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Back:
                    // 退格逐位删除：第一次按先把当前显示值装进编辑缓冲，再删末位。
                    // 年可从 4 位删到 3/2/1/0 位，月/日/时/分/秒/毫秒同理；删空后停在空白等待输入，离开时回退原值。
                    BackspaceField();
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Delete:
                    // Delete 一次清空整段数字（进入 0 位待输入态）
                    ClearFieldDigits();
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        /// <summary>把当前显示值的本段数字取进编辑缓冲（用于首次 Backspace）：所见即所得，含前导零。</summary>
        private string CurrentFieldDigits()
        {
            var f = CurrentField();
            if (f == null || _textProvider == null) return string.Empty;
            string text = _textProvider();
            return f.Start + f.Length > text.Length ? string.Empty : text.Substring(f.Start, f.Length);
        }

        private void EnterFieldEdit(string digits)
        {
            _digitField = _selected;
            _digitBuffer = digits ?? string.Empty;
        }

        private void RestartEditTimer()
        {
            _digitTimer.Stop();
            // 缓冲非空：停顿超时按部分值提交；删空后不计时，保留 0 位等用户输入
            if (_digitBuffer.Length > 0) _digitTimer.Start();
        }

        private void BackspaceField()
        {
            if (CurrentField() == null) return;
            if (!FieldEditing) EnterFieldEdit(CurrentFieldDigits());
            if (_digitBuffer.Length > 0)
                _digitBuffer = _digitBuffer.Substring(0, _digitBuffer.Length - 1);
            RestartEditTimer();
            PositionCaret();
            Invalidate();
        }

        private void ClearFieldDigits()
        {
            if (CurrentField() == null) return;
            EnterFieldEdit(string.Empty);
            _digitTimer.Stop();
            PositionCaret();
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (!Enabled || !char.IsDigit(e.KeyChar)) return;
            var f = CurrentField();
            if (f == null) return;

            if (_digitField != _selected || _digitBuffer.Length >= f.Length)
            {
                _digitBuffer = string.Empty;
                _digitField = _selected;
            }
            _digitBuffer += e.KeyChar;
            int val = int.Parse(_digitBuffer);

            // 输满位数立即提交并跳到下一段；数值已超上限也立即按夹取提交；否则等待超时
            if (_digitBuffer.Length >= f.Length || val > f.Max)
            {
                bool full = _digitBuffer.Length >= f.Length;
                ApplyTypedValue(Math.Max(f.Min, Math.Min(f.Max, val)));
                ResetDigitBuffer();
                if (full) SelectField(_selected + 1);
            }
            else
            {
                _digitTimer.Stop();
                _digitTimer.Start();
            }
            PositionCaret();
            Invalidate();
            e.Handled = true;
        }

        private void ApplyTypedValue(int value)
        {
            var f = CurrentField();
            if (f == null) return;
            _applyField(_selected, Math.Max(f.Min, Math.Min(f.Max, value)));
            RaiseValueEdited();
        }

        /// <summary>停顿超时或切换字段前，把已键入/删除后剩余的数字按当前值提交；缓冲为空（删空）则回退原值。</summary>
        private void CommitPendingDigit()
        {
            if (_digitField < 0) return;
            var fields = _fieldsProvider();
            if (_digitBuffer.Length > 0 && fields != null && _digitField < fields.Length)
            {
                int val = int.Parse(_digitBuffer);
                var f = fields[_digitField];
                _applyField(_digitField, Math.Max(f.Min, Math.Min(f.Max, val)));
                RaiseValueEdited();
            }
            ResetDigitBuffer();
        }

        private void ResetDigitBuffer()
        {
            _digitTimer.Stop();
            _digitBuffer = string.Empty;
            _digitField = -1;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            PositionCaret();
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            CommitPendingDigit();
            DestroyCaret();
            _caretCreated = false;
            Invalidate();
        }

        #endregion

        #region 绘制

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (_borderless || _radius <= 0)
            {
                base.OnPaintBackground(pevent);
                return;
            }
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            HDrawPaint.FillParentBackdrop(g, Width, Height, Parent?.BackColor ?? SystemColors.Control);
            Color fill = Enabled ? (_shapeBackColor.IsEmpty ? BackColor : _shapeBackColor)
                                 : Color.FromArgb(245, 246, 248);
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            using (var bb = new SolidBrush(fill))
                g.FillPath(bb, path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawBorder(g);
            DrawSegments(g);
            if (_showSpin) DrawSpin(g);
            if (_showDropArrow) DrawDropArrow(g);
        }

        private void DrawBorder(Graphics g)
        {
            if (_borderless) return;
            Color border = !Enabled ? Color.FromArgb(224, 226, 230) :
                Focused || _popupOpen ? AccentColor :
                _hover ? Color.FromArgb(249, 166, 99) : Color.FromArgb(214, 217, 223);

            // 外轮廓固定像素盒，边框厚度 w 全部向内（外路径挖空内路径，Alternate 奇偶填充）：
            // 改线宽外沿不动，直边整整一列不透明像素，灰/橙/悬停各状态视觉宽度一致。
            float w = _borderWidth;
            var outer = new RectangleF(-0.5f, -0.5f, Width, Height);
            if (_radius <= 0)
            {
                var inner = new RectangleF(w - 0.5f, w - 0.5f, Width - 2f * w, Height - 2f * w);
                using (var ring = new GraphicsPath(FillMode.Alternate))
                {
                    ring.AddRectangle(outer);
                    ring.AddRectangle(inner);
                    using (var bb = new SolidBrush(border))
                        g.FillPath(bb, ring);
                }
            }
            else
            {
                HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, w, border);
            }
        }

        private void DrawSegments(Graphics g)
        {
            if (_textProvider == null) return;
            string text = _textProvider();
            if (text.Length == 0) return;
            var fields = _fieldsProvider();
            var cr = ContentRect;
            int[] xs = CharOrigins(text);
            int lineH = LineHeight;
            int y = (Height - lineH) / 2;

            Color fg = Enabled ? ForeColor : SystemColors.GrayText;
            Color sep = Enabled ? Color.FromArgb(150, 154, 162) : SystemColors.GrayText;

            // 选中字段：橙色圆角衬底 + 白字
            var sf = fields[Math.Max(0, Math.Min(fields.Length - 1, _selected))];
            if (Enabled && Focused)
            {
                int hx = cr.X + xs[sf.Start] - 4;
                int hw = xs[sf.Start + sf.Length] - xs[sf.Start] + 8;
                var hl = new Rectangle(hx, (Height - lineH - 6) / 2, hw, lineH + 6);
                using (GraphicsPath hp = HDrawPaint.CreatePath(hl, 4, HRoundStyle.All, false))
                using (var hb = new SolidBrush(AccentColor))
                    g.FillPath(hb, hp);
            }

            // 各字符归属：数字段标 1..N（用于选中白字），分隔符为 0
            var fieldByChar = new int[text.Length];
            for (int i = 0; i < fields.Length; i++)
                for (int c = fields[i].Start; c < fields[i].Start + fields[i].Length; c++)
                    fieldByChar[c] = i + 1;

            // 文字走 GDI ExtTextOut：字符原点（CharOrigins）按 GetTextExtentPoint32 前缀宽定位，
            // DrawTextEx 会把数字 1/7 等 overhang 字形的悬垂展开成空隙，与原点错位。
            IntPtr hdc = g.GetHdc();
            try
            {
                using (var tc = HDrawPaint.BeginGdiText(hdc, Font))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        int fi = fieldByChar[i] - 1;
                        // 逐位编辑态（键入/退格删除）：敲什么显示什么，未输入槽位留空——编辑中不补零
                        if (Enabled && Focused && FieldEditing && fi == _selected)
                        {
                            int k = i - sf.Start;
                            if (k < _digitBuffer.Length)
                                tc.DrawRun(_digitBuffer.Substring(k, 1), cr.X + xs[i], y, Color.White);
                            continue;
                        }
                        // 定稿态（含鼠标离开后）：一律按标准格式串原样绘制（yyyy-MM-dd / HH:mm:ss.fff，含前导零）
                        bool inSelected = Enabled && Focused && fi == _selected;
                        Color c = fi >= 0 ? (inSelected ? Color.White : fg) : sep;
                        tc.DrawRun(text.Substring(i, 1), cr.X + xs[i], y, c);
                    }
                }
            }
            finally { g.ReleaseHdc(hdc); }
        }

        /// <summary>右侧上下微调钮（上下半区各一支箭头，按下区浅橙底）。</summary>
        private void DrawSpin(Graphics g)
        {
            var z = SpinZone;
            if (_spinPressed)
            {
                var pressedRect = new Rectangle(z.X + 2, _spinDir > 0 ? 2 : z.Height / 2,
                    z.Width - 4, z.Height / 2 - 2);
                using (GraphicsPath pp = HDrawPaint.CreatePath(pressedRect, 3, HRoundStyle.All, false))
                using (var pb = new SolidBrush(Color.FromArgb(38, AccentColor)))
                    g.FillPath(pb, pp);
            }
            Color ink = Enabled ? Color.FromArgb(120, 124, 130) : SystemColors.GrayText;
            using (var pen = new Pen(ink, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                int cx = z.X + z.Width / 2;
                int upY = Height / 4;
                int downY = Height * 3 / 4;
                g.DrawLines(pen, new[] { new Point(cx - 4, upY + 2), new Point(cx, upY - 2), new Point(cx + 4, upY + 2) });
                g.DrawLines(pen, new[] { new Point(cx - 4, downY - 2), new Point(cx, downY + 2), new Point(cx + 4, downY - 2) });
            }
            using (var pen = new Pen(Color.FromArgb(214, 217, 223), 1f))
                g.DrawLine(pen, z.X, 6, z.X, Height - 7);
        }

        /// <summary>右侧下拉箭头：弹出时朝上，否则朝下。</summary>
        private void DrawDropArrow(Graphics g)
        {
            var z = DropZone;
            if (_dropPressed || _popupOpen)
            {
                var bg = new Rectangle(z.X + 2, 3, z.Width - 4, Height - 6);
                using (GraphicsPath pp = HDrawPaint.CreatePath(bg, Math.Max(2, _radius - 3), HRoundStyle.All, false))
                using (var pb = new SolidBrush(_popupOpen ? Color.FromArgb(30, AccentColor) : Color.FromArgb(20, 0, 0, 0)))
                    g.FillPath(pb, pp);
            }
            int cx = z.X + z.Width / 2;
            int cy = Height / 2;
            Color ink = Enabled ? (_popupOpen || _hover ? AccentColor : Color.FromArgb(120, 124, 130))
                                : SystemColors.GrayText;
            using (var pen = new Pen(ink, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                Point[] tri = _popupOpen
                    ? new[] { new Point(cx - 5, cy + 2), new Point(cx, cy - 3), new Point(cx + 5, cy + 2) }
                    : new[] { new Point(cx - 5, cy - 2), new Point(cx, cy + 3), new Point(cx + 5, cy - 2) };
                g.DrawLines(pen, tri);
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
            PositionCaret();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DestroyCaret();
            _caretCreated = false;
            ReleaseMeasureHdc();
            base.OnHandleDestroyed(e);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            ReleaseMeasureHdc();
            PositionCaret();
            Invalidate();
        }

        /// <summary>圆角四角由 OnPaintBackground 铺父色透出，不再裁剪窗口 Region（HRgn 会削弧线出折角）。</summary>
        private void UpdateRegion()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spinTimer.Dispose();
                _digitTimer.Dispose();
                ReleaseMeasureHdc();
            }
            DestroyCaret();
            base.Dispose(disposing);
        }

        #endregion
    }
}
