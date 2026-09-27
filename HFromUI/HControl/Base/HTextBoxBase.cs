using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{
    using HFromUI.HLangage;
    /// <summary>
    /// 文本框基类：直接继承 Control 纯自绘（不内嵌任何原生 TextBox），
    /// 自实现系统插入符、选区、鼠标拖选、双击选词、键盘编辑、多行自动换行、
    /// 垂直滚动（含滚动条）、撤销/重做、复制/剪切/粘贴与 IME 上屏。
    /// 圆角通过 Radius 设置：0 或负数为 Windows 原生直角立体边。
    /// AutoFitFactor 大于 0 后字号（像素 em 高）直接取“控件高度 × 倍数”，随控件高度等比缩放
    /// （与文字长短无关，超长单行横向滚动，绘制严格裁剪在内容区内，不越下划线也不压装饰图标）；
    /// 小于等于 0 不自动缩放；仅单行模式生效，多行模式固定使用 Font。
    /// 底部下划线在鼠标悬停/激活时变色。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("TextChanged")]
    public class HTextBoxBase : Control
    {
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

        /// <summary>一个可视行：硬换行或软折行后的单行片段。</summary>
        private sealed class VLine
        {
            public string Text;
            public int Start;   // 在全文中的起始索引
        }

        // 行高仍用 TextRenderer 测量（仅取高度维度，无 overhang 横向问题）；
        // 横向宽度与文字绘制一律走 GDI：TextAdvance(GetTextExtentPoint32W) + ExtTextOutW，
        // 见 HDrawPaint.BeginGdiText——DrawTextEx 会展开引号等字符的 ABC 悬垂，与插入符定位不同源。
        private const TextFormatFlags TMeasure = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        private string _text = string.Empty;
        private int _anchor, _caret;
        private int _desiredX = -1;

        private int _radius;
        private int _borderWidth = 1;
        private Color _shapeBackColor = Color.Empty;
        private double _autoFitFactor;
        private Font _fitFont;
        private bool _hover;
        private bool _caretCreated;
        private Graphics _measureGraphics;
        private IntPtr _measureHdc;

        private readonly List<VLine> _lines = new List<VLine>();
        private int _scrollX, _scrollY;
        private bool _dragging, _sbDragging;
        private int _sbDragOffset;
        private bool _sbVisible;
        private Rectangle _sbTrack, _sbThumb;
        private int _lineH = 20;

        private readonly Stack<Snapshot> _undo = new Stack<Snapshot>();
        private readonly Stack<Snapshot> _redo = new Stack<Snapshot>();
        private DateTime _lastCharEdit = DateTime.MinValue;

        private Color _underlineColor = Color.FromArgb(170, 170, 170);
        private Color _underlineHoverColor = Color.DodgerBlue;
        private Color _underlineActiveColor = Color.DodgerBlue;

        private string _watermarkText;
        private Color _watermarkColor = Color.Gray;

        private struct Snapshot
        {
            public string Text;
            public int Caret;
            public Snapshot(string text, int caret) { Text = text; Caret = caret; }
        }

        public HTextBoxBase()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);
            BackColor = SystemColors.Window;
            ForeColor = SystemColors.WindowText;
            Font = new Font("微软雅黑", 9f);
            TabStop = true;
            Size = new Size(120, 28);
        }

        #region 外观属性

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径，0 或负数为 Windows 原生直角立体边"), Browsable(true)]
        [DefaultValue(0)]
        public int Radius
        {
            get => _radius;
            set { _radius = value; UpdateRegion(); Invalidate(); }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("圆角边框宽度"), HDescriptionLanguage("圆角边框宽度（像素），0 或负数不描边；仅圆角模式生效。描边向内收半个线宽，粗边也完整不缺边"), Browsable(true)]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; RefreshLayout(); UpdateRegion(); Invalidate(); }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("框内背景色"), HDescriptionLanguage("圆角/椭圆/胶囊外形内部的背景色；未设置时使用控件背景色。颜色只填充外形内部，圆角外不染色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }

        /// <summary>外形内部实际底色：设置了框内背景色用新色，否则用控件背景色（自绘框体子类共用）。</summary>
        protected Color ShapeFillColor => _shapeBackColor.IsEmpty ? BackColor : _shapeBackColor;

        /// <summary>
        /// 字号相对控件高度的倍数：小于等于 0 不自动缩放，统一使用 Font；
        /// 大于 0 时单行模式字号（em 像素高）直接取“控件高度 × 倍数”，随控件高度等比变化
        /// （如 0.5 即半个控件高），与文字长短、控件宽无关；多行模式不生效。
        /// 倍数过大、字形渲染行高放不进内容区时自动封顶，保证文字不越过下划线。
        /// </summary>
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("字号高度倍数"), HDescriptionLanguage("字号占控件高度的倍数；小于等于 0 不自动缩放，大于 0 字号按控件高度乘以倍数等比缩放（仅单行）"), Browsable(true)]
        [DefaultValue(0.0)]
        public double AutoFitFactor
        {
            get => _autoFitFactor;
            set
            {
                _autoFitFactor = value;
                RefreshLayout(); // ApplyFitFont 负责开启时重算、关闭或多行时丢弃适配字体
                Invalidate();
            }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("下划线常规颜色"), HDescriptionLanguage("下划线常规颜色"), Browsable(true)]
        public Color UnderlineColor
        {
            get => _underlineColor;
            set { _underlineColor = value; Invalidate(); }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("鼠标悬停时下划线颜色"), HDescriptionLanguage("鼠标悬停时下划线颜色"), Browsable(true)]
        public Color UnderlineHoverColor
        {
            get => _underlineHoverColor;
            set { _underlineHoverColor = value; Invalidate(); }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("控件激活"), HDescriptionLanguage("控件激活（获得焦点）时下划线颜色"), Browsable(true)]
        public Color UnderlineActiveColor
        {
            get => _underlineActiveColor;
            set { _underlineActiveColor = value; Invalidate(); }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("水印提示文字"), HDescriptionLanguage("水印提示文字（内容为空时显示）"), Browsable(true)]
        [DefaultValue("")]
        public string WatermarkText
        {
            get => _watermarkText ?? string.Empty;
            set { _watermarkText = value; Invalidate(); }
        }

        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("水印提示文字颜色"), HDescriptionLanguage("水印提示文字颜色"), Browsable(true)]
        public Color WatermarkColor
        {
            get => _watermarkColor;
            set { _watermarkColor = value; Invalidate(); }
        }

        #endregion

        #region 装饰区扩展点（按钮/图标带子类）

        /// <summary>左侧装饰区宽度（如按钮/图标带），文字排版自动让开，返回值不得为负。</summary>
        protected virtual int AdornmentLeftWidth => 0;

        /// <summary>右侧装饰区宽度，文字排版自动让开，返回值不得为负。</summary>
        protected virtual int AdornmentRightWidth => 0;

        /// <summary>是否绘制基类默认外框与下划线；子类完全自绘外框时覆盖为 false。</summary>
        protected virtual bool UseDefaultFrame => true;

        /// <summary>
        /// 控件外形路径（像素盒坐标）：背景只在路径内填充，路径外四角铺父色透出。
        /// 外轮廓固定在像素盒边缘 (-0.5,-0.5)-(W-0.5,H-0.5)，与边框环同一几何口径。
        /// 返回 null 表示矩形窗口（直角）。子类自绘圆角/胶囊等外框时覆盖。
        /// </summary>
        protected virtual GraphicsPath CreateOuterShapePath(RectangleF bounds)
        {
            return _radius > 0
                ? HDrawPaint.CreateOuterBoxPath(bounds.Width, bounds.Height, _radius)
                : null;
        }

        /// <summary>
        /// 背景绘制：先整块铺父容器底色（外扩 1px，杜绝双缓冲合成时顶/左黑晕），
        /// 再在外形内填框体底色。必须在 OnPaintBackground 阶段完成——双缓冲下 OnPaint
        /// 阶段会与背景阶段半透明合成，铺父色会留下余晕。不用窗口 Region（HRgn 削弧折角）。
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            HDrawPaint.FillParentBackdrop(g, Width, Height, Parent?.BackColor ?? SystemColors.Control);
            FillShapeBackground(g, ClientRectangle);
        }

        /// <summary>仅填充框体外形内的底色；自绘框体子类可覆盖（无框风格不填、透明底等）。</summary>
        protected virtual void FillShapeBackground(Graphics g, Rectangle rect)
        {
            Color bg = Enabled ? ShapeFillColor : SystemColors.Control;
            using (var brush = new SolidBrush(bg))
            using (GraphicsPath path = CreateOuterShapePath(new RectangleF(0, 0, Width, Height)))
            {
                if (path != null) g.FillPath(brush, path);
                else g.FillRectangle(brush, -1, -1, Width + 2, Height + 2);
            }
        }

        /// <summary>
        /// 内容区微调：基类内容区按原生凹陷边/圆角描边预留上下边距；
        /// 自绘满高框体的子类可覆盖为上下等距，使文字/水印在框内垂直居中。
        /// </summary>
        protected virtual Rectangle AdjustContentRect(Rectangle cr) => cr;

        /// <summary>在背景之上、文字之下绘制装饰（按钮/图标等），装饰不会被文字覆盖。</summary>
        protected virtual void DrawAdornments(Graphics g) { }

        /// <summary>装饰区鼠标按下：返回 true 表示命中装饰、不再放置插入符。</summary>
        protected virtual bool AdornmentMouseDown(MouseEventArgs e) => false;

        /// <summary>装饰区鼠标抬起。</summary>
        protected virtual void AdornmentMouseUp(MouseEventArgs e) { }

        /// <summary>装饰区鼠标移动（悬停态/按下拖动）。</summary>
        protected virtual void AdornmentMouseMove(MouseEventArgs e) { }

        /// <summary>鼠标离开控件，装饰复位悬停态。</summary>
        protected virtual void AdornmentMouseLeave() { }

        #endregion

        #region 文本与编辑属性

        public override string Text
        {
            get => _text;
            set
            {
                _text = Normalize(value);
                _anchor = _caret = _text.Length;
                _undo.Clear();
                _redo.Clear();
                _desiredX = -1;
                RefreshLayout();
                OnTextChanged(EventArgs.Empty);
            }
        }

        private bool _multiline;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("多行"), HDescriptionLanguage("是否多行编辑（多行时自动换行并可垂直滚动）"), Browsable(true)]
        [DefaultValue(false)]
        public bool Multiline
        {
            get => _multiline;
            set { _multiline = value; RefreshLayout(); Invalidate(); }
        }

        private char _passwordChar;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("密码屏蔽字符"), HDescriptionLanguage("密码屏蔽字符（0 表示不屏蔽）"), Browsable(true)]
        [DefaultValue('\0')]
        public char PasswordChar
        {
            get => _passwordChar;
            set { _passwordChar = value; Invalidate(); }
        }

        private bool _useSystemPasswordChar;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("是否使用系统密码字符"), HDescriptionLanguage("是否使用系统密码字符（实心圆点）"), Browsable(true)]
        [DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get => _useSystemPasswordChar;
            set { _useSystemPasswordChar = value; RefreshLayout(); Invalidate(); }
        }

        private bool _readOnly;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("只读"), HDescriptionLanguage("是否只读（仍可选择与复制）"), Browsable(true)]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => _readOnly;
            set { _readOnly = value; Invalidate(); }
        }

        private int _maxLength = 32767;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("最大长度"), HDescriptionLanguage("最大输入字符数"), Browsable(true)]
        [DefaultValue(32767)]
        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = Math.Max(0, value);
        }

        private ContentAlignment _textAlign = ContentAlignment.MiddleLeft;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("文字对齐"), HDescriptionLanguage("文字对齐（九宫格，与 HLabelBase 一致）；多行时仅水平分量生效，长文本放不下时退化为左对齐并横向滚动"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleLeft)]
        public ContentAlignment TextAlign
        {
            get => _textAlign;
            set { _textAlign = value; RefreshLayout(); }
        }

        private CharacterCasing _characterCasing;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("字符大小写模式"), HDescriptionLanguage("字符大小写模式"), Browsable(true)]
        [DefaultValue(CharacterCasing.Normal)]
        public CharacterCasing CharacterCasing
        {
            get => _characterCasing;
            set { _characterCasing = value; }
        }

        private ScrollBars _scrollBars;
        [HCategoryLanguage("文本框基础设置"), HDisplayNameLanguage("滚动条"), HDescriptionLanguage("多行时的滚动条（Vertical 表示内容超出时显示垂直滚动条）"), Browsable(true)]
        [DefaultValue(ScrollBars.None)]
        public ScrollBars ScrollBars
        {
            get => _scrollBars;
            set { _scrollBars = value; RefreshLayout(); Invalidate(); }
        }

        [Browsable(false)]
        public int SelectionStart
        {
            get => Math.Min(_anchor, _caret);
            set => SetSelection(value, SelectionLength, false);
        }

        [Browsable(false)]
        public int SelectionLength
        {
            get => Math.Abs(_caret - _anchor);
            set => SetSelection(SelectionStart, value, false);
        }

        [Browsable(false)]
        public string SelectedText => SelectionLength == 0 ? string.Empty :
            _text.Substring(Math.Min(_anchor, _caret), Math.Abs(_caret - _anchor));

        [Browsable(false)]
        public bool CanUndo => _undo.Count > 0;

        #endregion

        #region 公开方法

        public void AppendText(string text)
        {
            int end = _text.Length;
            _anchor = _caret = end;
            InsertString(text, false);
        }

        public void Clear()
        {
            Text = string.Empty;
        }

        public void SelectAll()
        {
            _anchor = 0;
            _caret = _text.Length;
            EnsureCaretVisible();
            Invalidate();
        }

        public void Select(int start, int length)
        {
            SetSelection(start, length, false);
        }

        public void Copy()
        {
            if (SelectionLength > 0)
                Clipboard.SetText(SelectedText, TextDataFormat.UnicodeText);
        }

        public void Cut()
        {
            if (!_readOnly && SelectionLength > 0)
            {
                Copy();
                DeleteRange(SelectionStart, SelectionLength);
            }
        }

        public void Paste()
        {
            if (!_readOnly && Clipboard.ContainsText())
                InsertString(Clipboard.GetText(TextDataFormat.UnicodeText), false);
        }

        public void Undo()
        {
            if (_undo.Count == 0) return;
            _lastCharEdit = DateTime.MinValue;
            _redo.Push(new Snapshot(_text, _caret));
            var s = _undo.Pop();
            ApplyState(s.Text, s.Caret);
        }

        public void Redo()
        {
            if (_redo.Count == 0) return;
            _lastCharEdit = DateTime.MinValue;
            _undo.Push(new Snapshot(_text, _caret));
            var s = _redo.Pop();
            ApplyState(s.Text, s.Caret);
        }

        /// <summary>滚动使插入符可见。</summary>
        public void ScrollToCaret() => EnsureCaretVisible();

        #endregion

        #region 状态与布局

        private Font ActiveFont => AutoFitActive && _fitFont != null ? _fitFont : Font;

        /// <summary>自动字号当前是否生效：倍数大于 0 且是单行模式。</summary>
        private bool AutoFitActive => _autoFitFactor > 0.0 && !_multiline;

        private bool ShouldShowPasswordChar => _passwordChar != '\0' || _useSystemPasswordChar;

        private char PasswordBullet => _useSystemPasswordChar ? '\u25CF' : _passwordChar;

        /// <summary>文字内容区（客户区坐标），扣除边框、内边距与下划线。</summary>
        private Rectangle ContentRect
        {
            get
            {
                // 圆角时让内容区避开整支描边笔（旧值固定 1，粗边框会压到文字）
                int frame = _radius > 0 ? Math.Max(1, _borderWidth) : 2;
                int padX = _radius > 0 ? Math.Max(6, _radius / 2 + 4) : 4;
                int leftAdorn = Math.Max(0, AdornmentLeftWidth);
                int rightAdorn = Math.Max(0, AdornmentRightWidth);
                int x = Padding.Left + padX + leftAdorn;
                int top = Padding.Top + frame + 2;
                int bottom = Height - Padding.Bottom - 7 - frame;
                int w = Width - x - (Padding.Right + padX + rightAdorn);
                if (_sbVisible) w -= 10;
                return AdjustContentRect(
                    new Rectangle(x, top, Math.Max(0, w), Math.Max(0, bottom - top)));
            }
        }

        private int SelStart => Math.Min(_anchor, _caret);

        private int SelEnd => Math.Max(_anchor, _caret);

        private static string Normalize(string s)
        {
            return string.IsNullOrEmpty(s) ? string.Empty : s.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private char ApplyCase(char c)
        {
            if (_characterCasing == CharacterCasing.Upper) return char.ToUpper(c);
            if (_characterCasing == CharacterCasing.Lower) return char.ToLower(c);
            return c;
        }

        private void SetSelection(int start, int length, bool extend)
        {
            start = Math.Max(0, Math.Min(start, _text.Length));
            length = Math.Max(0, Math.Min(length, _text.Length - start));
            if (extend)
                _caret = start + length;
            else
            {
                _anchor = start;
                _caret = start + length;
            }
            _desiredX = -1;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        /// <summary>重建可视行（硬换行 + 多行软折行），并计算滚动条与滚动范围。</summary>
        private void RebuildLines()
        {
            _lines.Clear();
            Font f = ActiveFont;
            _lineH = TextRenderer.MeasureText(HTranslation.GetContent("Ag中"), f, Size.Empty, TMeasure).Height + 2;

            var cr = ContentRect;
            string[] hard = _text.Split('\n');
            int pos = 0;
            foreach (string part in hard)
            {
                if (_multiline && cr.Width > 0)
                    WrapSegment(part, pos, f, cr.Width);
                else
                    _lines.Add(new VLine { Text = part, Start = pos });
                pos += part.Length + 1;
            }

            // 滚动条可见性可能因行数变化而改变，改变后按更窄宽度重排一次
            bool need = _multiline && _scrollBars == ScrollBars.Vertical && TotalHeight() > cr.Height;
            if (need != _sbVisible)
            {
                _sbVisible = need;
                RebuildLines();
                return;
            }
            ClampScroll();
        }

        /// <summary>按英文单词边界/中文逐字的贪心折行。</summary>
        private void WrapSegment(string part, int start, Font f, int maxW)
        {
            int i = 0;
            while (i < part.Length)
            {
                int j = i, lastSpace = -1;
                while (j < part.Length)
                {
                    if (part[j] == ' ') lastSpace = j;
                    if (TextWidth(part.Substring(i, j - i + 1), f) > maxW) break;
                    j++;
                }
                if (j == part.Length)
                {
                    _lines.Add(new VLine { Text = part.Substring(i), Start = start + i });
                    return;
                }
                int br;
                if (lastSpace > i) br = lastSpace;          // 超宽且行内有空格：在最后一个空格处折行
                else if (j == i) br = i + 1;                // 单字就超宽（长英文词/大字）：硬切一字
                else br = j;
                _lines.Add(new VLine { Text = part.Substring(i, br - i), Start = start + i });
                i = br < part.Length && part[br] == ' ' ? br + 1 : br;
            }
            _lines.Add(new VLine { Text = string.Empty, Start = start + part.Length });
        }

        // 宽度统一走 GDI GetTextExtentPoint32（严格步进宽）：与 TextRenderer 绘制出的字形推进一致，
        // 且没有 MeasureText 的尾部安全余量，插入符定位、折行判断都以此为准。
        // 必须在控件自身窗口 DC 上计量：PerMonitorV2 跨显示器时窗口 DC 与控件实际绘制同 DPI，
        // 用屏幕 DC 会导致插入符位置与 IME 拼音/候选窗位置随字号放大而偏移。
        private int TextWidth(string s, Font f)
        {
            EnsureMeasureHdc();
            return HDrawPaint.TextAdvance(s, f, _measureHdc);
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

        // 自适应字号按行高调档时用：TextRenderer 与 OnPaint 的 DrawText 同坐标系，量实际渲染行高。
        private static int FitLineHeight(Font f)
            => TextRenderer.MeasureText(HTranslation.GetContent("Ag中"), f, Size.Empty, TMeasure).Height + 2;

        private int TotalHeight() => _lines.Count * _lineH;

        /// <summary>文本变化/尺寸变化后的完整刷新：自适应字号 → 重排 → 滚动 → 光标 → 重绘。</summary>
        protected void RefreshLayout()
        {
            ApplyFitFont();
            RebuildLines();
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        private void DisposeFitFont()
        {
            if (_fitFont == null) return;
            _fitFont.Dispose();
            _fitFont = null;
            // 强制下次重算：缓存键随字号对象一起失效
            _fitKeyH = -1;
        }

        /// <summary>
        /// 按当前模式决定自适应字号：单行且开关打开 → FitFontSize；
        /// 多行或关闭开关 → 丢弃适配字体，退回 Font。
        /// </summary>
        private void ApplyFitFont()
        {
            if (AutoFitActive) FitFontSize();
            else DisposeFitFont();
        }

        // 适配结果缓存键：字号只取决于控件高度、内容区高度、倍数与基础字体（族/样式）。
        // 宽度、文字内容均不参与；用像素单位建字体，跨 DPI 显示器渲染等比一致。
        private int _fitKeyH = -1;
        private int _fitKeyCrH = -1;
        private double _fitKeyFactor = -1.0;
        private string _fitKeyFamily;
        private FontStyle _fitKeyStyle = (FontStyle)(-1);

        /// <summary>
        /// 自适应字号：字号（em 像素高）直接取“控件高度 × AutoFitFactor”，随控件高度等比变化，
        /// 与文字长短、控件宽无关——文字变长不缩字（单行超出部分横向滚动、绘制严格裁剪在内容区内），
        /// 清空文字后字号保持不变。倍数过大、字体实际渲染行高（含上下留白）放不进内容区时，
        /// 按 0.5px 步进向下封顶，保证大字不压住、不越过下划线，也不侵入左右装饰带。
        /// 键不变直接复用缓存，击键零开销。
        /// </summary>
        private void FitFontSize()
        {
            var cr = ContentRect;
            if (cr.Height <= 0 || Height <= 0) return; // 高度尚未布局完成时保留上次字号

            if (_fitFont != null
                && _fitKeyH == Height
                && _fitKeyCrH == cr.Height
                && Math.Abs(_fitKeyFactor - _autoFitFactor) < 1e-4
                && _fitKeyFamily == Font.FontFamily.Name
                && _fitKeyStyle == Font.Style)
                return;

            // 字号即控件高度的倍数（像素），不再反推行高
            float size = Height * (float)_autoFitFactor;
            while (size > 1f)
            {
                using (var t = new Font(Font.FontFamily, size, Font.Style, GraphicsUnit.Pixel))
                    if (FitLineHeight(t) <= cr.Height) break;
                size -= 0.5f; // 倍数过大时向下封顶，文字不越过下划线
            }

            _fitKeyH = Height;
            _fitKeyCrH = cr.Height;
            _fitKeyFactor = _autoFitFactor;
            _fitKeyFamily = Font.FontFamily.Name;
            _fitKeyStyle = Font.Style;

            if (_fitFont != null && Math.Abs(_fitFont.Size - size) < 0.01f) return;
            _fitFont?.Dispose();
            _fitFont = new Font(Font.FontFamily, size, Font.Style, GraphicsUnit.Pixel);
        }

        private void ClampScroll()
        {
            var cr = ContentRect;
            _scrollX = Math.Max(0, Math.Min(_scrollX, Math.Max(0, SingleLineWidth() - cr.Width)));
            _scrollY = Math.Max(0, Math.Min(_scrollY, Math.Max(0, TotalHeight() - cr.Height)));
        }

        private int SingleLineWidth()
        {
            if (_lines.Count == 0) return 0;
            return TextWidth(DisplayOf(_lines[0].Text), ActiveFont);
        }

        #endregion

        #region 坐标换算与命中测试

        /// <summary>密码屏蔽后的显示串（与原文等长，索引一一对应）。</summary>
        private string DisplayOf(string s)
        {
            if (!ShouldShowPasswordChar || s.Length == 0) return s;
            var sb = new StringBuilder(s.Length);
            sb.Append(PasswordBullet, s.Length);
            return sb.ToString();
        }

        /// <summary>查找包含全文索引的可视行，返回行号；越界时返回最近的首/末行。</summary>
        private int FindLineIndex(int index)
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                int end = _lines[i].Start + _lines[i].Text.Length;
                if (index <= end || i == _lines.Count - 1) return i;
            }
            return 0;
        }

        /// <summary>可视行顶部 Y：多行按行号与垂直滚动；单行按九宫格垂直分量摆放。</summary>
        private int LineTop(int i, Rectangle cr)
        {
            if (_multiline) return cr.Y + i * _lineH - _scrollY;
            string a = _textAlign.ToString();
            if (a.StartsWith("Top")) return cr.Y + 1;
            if (a.StartsWith("Bottom")) return cr.Bottom - _lineH - 1;
            return cr.Y + (cr.Height - _lineH) / 2;
        }

        /// <summary>可视行文字的起始 X（单行按九宫格水平分量对齐并支持横向滚动，多行固定左侧）。</summary>
        private int LineBaseX(int i, Rectangle cr, Font f)
        {
            int w = TextWidth(DisplayOf(_lines[i].Text), f);
            // 多行或文字宽超出内容区：固定左对齐，超出部分横向滚动
            if (_multiline || w >= cr.Width) return cr.X - _scrollX;
            string a = _textAlign.ToString();
            if (a.EndsWith("Right")) return cr.Right - w;
            if (a.EndsWith("Center")) return cr.X + (cr.Width - w) / 2;
            return cr.X;
        }

        /// <summary>全文索引对应的客户区坐标（插入符位置）。</summary>
        private Point PointFromIndex(int index)
        {
            if (_lines.Count == 0) return Point.Empty;
            var cr = ContentRect;
            Font f = ActiveFont;
            int li = FindLineIndex(index);
            var line = _lines[li];
            int off = Math.Max(0, Math.Min(index - line.Start, line.Text.Length));
            int x = LineBaseX(li, cr, f) + TextWidth(DisplayOf(line.Text.Substring(0, off)), f);
            int y = LineTop(li, cr);
            return new Point(x, y);
        }

        /// <summary>按可视行内像素 X 求最近的字符偏移（二分）。</summary>
        private int OffsetAtX(string display, int localX, Font f)
        {
            if (localX <= 0) return 0;
            int lo = 0, hi = display.Length;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (TextWidth(display.Substring(0, mid), f) <= localX) lo = mid;
                else hi = mid - 1;
            }
            if (lo < display.Length)
            {
                int a = TextWidth(display.Substring(0, lo), f);
                int b = TextWidth(display.Substring(0, lo + 1), f);
                if (Math.Abs(localX - a) > Math.Abs(b - localX)) lo++;
            }
            return lo;
        }

        /// <summary>鼠标客户区坐标 → 全文索引。</summary>
        private int IndexFromPoint(Point p)
        {
            if (_lines.Count == 0) return 0;
            var cr = ContentRect;
            Font f = ActiveFont;

            int li;
            if (_multiline)
            {
                li = (p.Y - cr.Y + _scrollY) / _lineH;
                li = Math.Max(0, Math.Min(li, _lines.Count - 1));
            }
            else li = 0;

            var line = _lines[li];
            // LineBaseX 已把横向滚动量计入（左对齐超长时为 cr.X - _scrollX），这里直接相减
            int localX = p.X - LineBaseX(li, cr, f);
            return line.Start + OffsetAtX(DisplayOf(line.Text), localX, f);
        }

        #endregion

        #region 滚动与插入符

        private void EnsureCaretVisible()
        {
            if (_lines.Count == 0) return;
            var cr = ContentRect;
            int li = FindLineIndex(_caret);
            int top = LineTop(li, cr);

            if (_multiline)
            {
                if (top < cr.Y) _scrollY -= cr.Y - top;
                else if (top + _lineH > cr.Bottom) _scrollY += top + _lineH - cr.Bottom;
                ClampScroll();
            }
            else
            {
                int x = PointFromIndex(_caret).X;
                if (x < cr.X) _scrollX -= cr.X - x + 1;
                else if (x > cr.Right) _scrollX += x - cr.Right + 1;
                ClampScroll();
            }
        }

        /// <summary>焦点在本控件时，按插入符位置定位系统插入符（获焦时创建，之后只移动）。</summary>
        private void PositionCaret()
        {
            if (!Focused || !IsHandleCreated || _lines.Count == 0) return;
            Point p = PointFromIndex(_caret);
            if (!_caretCreated)
            {
                CreateCaret(Handle, IntPtr.Zero, 1, _lineH - 2);
                ShowCaret(Handle);
                _caretCreated = true;
            }
            SetCaretPos(p.X, p.Y + 1);
            // 同步告知 IME 插入符位置，拼音组合窗/候选窗贴在 | 周边（原生 EDIT 行为）
            HDrawPaint.SetImeCompositionPoint(Handle, new Point(p.X, p.Y + _lineH));
        }

        /// <summary>IME 查询字符位置（TSF 输入法据此摆放候选窗）与开始组字时的定位。</summary>
        protected override void WndProc(ref Message m)
        {
            if (HDrawPaint.TryAnswerImeCharPosition(ref m,
                    idx => PointFromIndex(Math.Max(0, Math.Min(idx, _text.Length))),
                    _lineH, ClientRectangle))
                return;
            if (m.Msg == HDrawPaint.WM_IME_STARTCOMPOSITION && Focused && _lines.Count > 0)
            {
                Point p = PointFromIndex(_caret);
                HDrawPaint.SetImeCompositionPoint(Handle, new Point(p.X, p.Y + _lineH));
            }
            base.WndProc(ref m);
        }

        private void ScrollbarMetrics()
        {
            var cr = ContentRect;
            _sbTrack = new Rectangle(cr.Right + 3, cr.Y, 8, cr.Height);
            int total = TotalHeight();
            int thumbH = Math.Max(20, cr.Height * cr.Height / Math.Max(1, total));
            int range = Math.Max(1, cr.Height - thumbH);
            int thumbY = cr.Y + (total <= cr.Height ? 0 :
                _scrollY * range / (total - cr.Height));
            _sbThumb = new Rectangle(_sbTrack.X, thumbY, 8, thumbH);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_multiline) return;
            _scrollY = Math.Max(0, _scrollY - e.Delta / 120 * _lineH * 3);
            ClampScroll();
            PositionCaret();
            Invalidate();
        }

        #endregion

        #region 鼠标交互

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled) return;
            Focus();

            if (_sbVisible)
            {
                ScrollbarMetrics();
                if (_sbTrack.Contains(e.Location))
                {
                    if (_sbThumb.Contains(e.Location))
                    {
                        _sbDragging = true;
                        _sbDragOffset = e.Y - _sbThumb.Y;
                    }
                    else
                    {
                        _scrollY += e.Y < _sbThumb.Y ? -ContentRect.Height : ContentRect.Height;
                        ClampScroll();
                        Invalidate();
                    }
                    return;
                }
            }

            // 子类装饰按钮/图标优先命中（不放置插入符、不开始文字拖选）
            if (AdornmentMouseDown(e)) return;

            int idx = IndexFromPoint(e.Location);
            if (e.Clicks >= 2)
            {
                SelectWordAt(idx);
            }
            else
            {
                _anchor = _caret = idx;
                _desiredX = PointFromIndex(idx).X - ContentRect.X;
            }
            _dragging = true;
            Capture = true;
            PositionCaret();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            AdornmentMouseMove(e);
            var cr = ContentRect;

            if (_sbDragging)
            {
                int total = TotalHeight();
                if (total > cr.Height)
                {
                    int thumbY = e.Y - _sbDragOffset;
                    _scrollY = (thumbY - cr.Y) * (total - cr.Height) / Math.Max(1, cr.Height - _sbThumb.Height);
                    ClampScroll();
                    PositionCaret();
                    Invalidate();
                }
                return;
            }

            if (!_dragging || e.Button != MouseButtons.Left) return;

            // 拖到内容区上下边缘时自动滚动
            if (_multiline)
            {
                if (e.Y < cr.Y) { _scrollY -= _lineH; ClampScroll(); }
                else if (e.Y > cr.Bottom) { _scrollY += _lineH; ClampScroll(); }
            }
            _caret = IndexFromPoint(e.Location);
            _desiredX = -1;
            PositionCaret();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            AdornmentMouseUp(e);
            _dragging = _sbDragging = false;
            Capture = false;
        }

        /// <summary>双击选中光标下的单词（非空白非标点的连续字符）。</summary>
        private void SelectWordAt(int index)
        {
            if (_text.Length == 0) return;
            index = Math.Max(0, Math.Min(index, _text.Length - 1));
            if (!IsWordChar(_text[index]))
            {
                // 点在空格/标点上时就近选词：优先右侧词，再退回左侧词（原生编辑框语义）
                int r = index;
                while (r < _text.Length && !IsWordChar(_text[r])) r++;
                if (r < _text.Length) index = r;
                else
                {
                    int l = index;
                    while (l >= 0 && !IsWordChar(_text[l])) l--;
                    if (l < 0) return;
                    index = l;
                }
            }
            int s = index, e2 = index + 1;
            while (s > 0 && IsWordChar(_text[s - 1])) s--;
            while (e2 < _text.Length && IsWordChar(_text[e2])) e2++;
            _anchor = s;
            _caret = e2;
        }

        private static bool IsWordChar(char c)
            => !char.IsWhiteSpace(c) && !char.IsControl(c) && !char.IsPunctuation(c);

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            AdornmentMouseLeave();
            Invalidate();
            base.OnMouseLeave(e);
        }

        #endregion

        #region 键盘与编辑

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
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Delete:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool shift = e.Shift;
            bool ctrl = e.Control;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    MoveCaret(ctrl ? PrevWordBoundary(_caret) : _caret - 1, shift);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Right:
                    MoveCaret(ctrl ? NextWordBoundary(_caret) : _caret + 1, shift);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    MoveVertical(-1, shift, ctrl);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Down:
                    MoveVertical(1, shift, ctrl);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Home:
                    MoveCaret(ctrl ? 0 : LineBoundary(_caret, false), shift);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.End:
                    MoveCaret(ctrl ? _text.Length : LineBoundary(_caret, true), shift);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.PageUp:
                    MoveVertical(-Math.Max(1, ContentRect.Height / _lineH), shift);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.PageDown:
                    MoveVertical(Math.Max(1, ContentRect.Height / _lineH), shift);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Back:
                    if (!_readOnly) Backspace(ctrl);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Delete:
                    if (!_readOnly) DeleteForward(ctrl);
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Enter:
                    if (_multiline && !_readOnly)
                    {
                        InsertString("\n", true);
                        e.SuppressKeyPress = true;
                    }
                    break;
                case Keys.A:
                    if (ctrl) { SelectAll(); e.SuppressKeyPress = true; }
                    break;
                case Keys.C:
                    if (ctrl) { Copy(); e.SuppressKeyPress = true; }
                    break;
                case Keys.X:
                    if (ctrl) { Cut(); e.SuppressKeyPress = true; }
                    break;
                case Keys.V:
                    if (ctrl) { Paste(); e.SuppressKeyPress = true; }
                    break;
                case Keys.Z:
                    if (ctrl) { Undo(); e.SuppressKeyPress = true; }
                    break;
                case Keys.Y:
                    if (ctrl) { Redo(); e.SuppressKeyPress = true; }
                    break;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (char.IsControl(e.KeyChar)) return;
            if (_readOnly || !Enabled)
            {
                e.Handled = true;
                return;
            }
            InsertString(e.KeyChar.ToString(), true);
            e.Handled = true;
        }

        /// <summary>移动插入符（Shift 按下时为扩展选区）。</summary>
        private void MoveCaret(int index, bool extend)
        {
            index = Math.Max(0, Math.Min(index, _text.Length));
            if (!extend) _anchor = index;
            _caret = index;
            _desiredX = PointFromIndex(index).X - ContentRect.X;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        /// <summary>多行上下移动，保持上次水平位置；跨行数由 lines 参数给定。</summary>
        private void MoveVertical(int lines, bool extend, bool wordJump = false)
        {
            if (_lines.Count == 0) return;
            var cr = ContentRect;
            if (!_multiline)
            {
                // 真机对照原生单行 EDIT：单行无上下行可走，↑/↓ 退化为 ←/→ 逐字移动
                // （Ctrl 组合为逐词，Shift 扩展选区），而不是跳到行首行尾。
                int index = lines < 0
                    ? (wordJump ? PrevWordBoundary(_caret) : _caret - 1)
                    : (wordJump ? NextWordBoundary(_caret) : _caret + 1);
                MoveCaret(index, extend);
                return;
            }
            int cur = FindLineIndex(_caret);
            int target = Math.Max(0, Math.Min(cur + lines, _lines.Count - 1));
            int wantX = _desiredX >= 0 ? cr.X + _desiredX : PointFromIndex(_caret).X;
            var line = _lines[target];
            int off = OffsetAtX(DisplayOf(line.Text), wantX - cr.X + _scrollX, ActiveFont);
            if (!extend) _anchor = line.Start + off;
            _caret = line.Start + off;
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
        }

        /// <summary>当前可视行的行首/行尾索引。</summary>
        private int LineBoundary(int index, bool end)
        {
            if (_lines.Count == 0 || !_multiline) return end ? _text.Length : 0;
            int li = FindLineIndex(index);
            var line = _lines[li];
            return end ? line.Start + line.Text.Length : line.Start;
        }

        private int PrevWordBoundary(int index)
        {
            int i = index;
            while (i > 0 && char.IsWhiteSpace(_text[i - 1])) i--;
            while (i > 0 && IsWordChar(_text[i - 1])) i--;
            return i;
        }

        private int NextWordBoundary(int index)
        {
            int i = index;
            while (i < _text.Length && char.IsWhiteSpace(_text[i])) i++;
            while (i < _text.Length && IsWordChar(_text[i])) i++;
            return i;
        }

        /// <summary>在当前选区处插入文本（受 MaxLength 与大小写约束）。</summary>
        private void InsertString(string input, bool typed)
        {
            if (string.IsNullOrEmpty(input)) return;
            input = Normalize(input);
            if (_characterCasing != CharacterCasing.Normal)
            {
                var sb = new StringBuilder(input.Length);
                foreach (char c in input) sb.Append(ApplyCase(c));
                input = sb.ToString();
            }

            int start = SelStart, len = SelEnd - SelStart;
            int room = Math.Max(0, _maxLength - (_text.Length - len));
            if (room == 0) return;
            if (input.Length > room) input = input.Substring(0, room);

            PushUndo(typed);
            _text = _text.Remove(start, len).Insert(start, input);
            int pos = start + input.Length;
            _anchor = _caret = pos;
            CommitEdit(pos);
        }

        private void Backspace(bool word)
        {
            if (SelectionLength > 0)
            {
                DeleteRange(SelStart, SelectionLength);
                return;
            }
            if (_caret == 0) return;
            int to = word ? PrevWordBoundary(_caret) : _caret - 1;
            PushUndo(false);
            _text = _text.Remove(to, _caret - to);
            CommitEdit(to);
        }

        private void DeleteForward(bool word)
        {
            if (SelectionLength > 0)
            {
                DeleteRange(SelStart, SelectionLength);
                return;
            }
            if (_caret >= _text.Length) return;
            int to = word ? NextWordBoundary(_caret) : _caret + 1;
            PushUndo(false);
            _text = _text.Remove(_caret, to - _caret);
            CommitEdit(_caret);
        }

        private void DeleteRange(int start, int length)
        {
            if (length <= 0) return;
            PushUndo(false);
            _text = _text.Remove(start, length);
            CommitEdit(start);
        }

        /// <summary>编辑后统一收尾：重排、字号自适应、滚动、光标与事件。</summary>
        private void CommitEdit(int caret)
        {
            _anchor = _caret = caret;
            // 字号只取决于控件高度与基础字体，文字变化不改变结果；走缓存，击键近乎零开销
            ApplyFitFont();
            RebuildLines();
            EnsureCaretVisible();
            PositionCaret();
            Invalidate();
            OnTextChanged(EventArgs.Empty);
        }

        /// <summary>连续单字符输入 800ms 内合并为一次撤销。</summary>
        private void PushUndo(bool typed)
        {
            DateTime now = DateTime.Now;
            if (!(typed && (now - _lastCharEdit).TotalMilliseconds < 800 && _undo.Count > 0))
                _undo.Push(new Snapshot(_text, _caret));
            _lastCharEdit = now;
            _redo.Clear();
        }

        private void ApplyState(string text, int caret)
        {
            _text = text;
            _anchor = _caret = Math.Max(0, Math.Min(caret, _text.Length));
            RefreshLayout();
            OnTextChanged(EventArgs.Empty);
        }

        #endregion

        #region 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // 边框
            if (UseDefaultFrame)
            {
                if (_radius > 0)
                {
                    // 外轮廓固定像素盒，边框厚度全部向内（填充环）：改线宽外轮廓不动，四边等宽
                    Color border = !Enabled ? SystemColors.ControlDark :
                        Focused ? _underlineActiveColor :
                        _hover ? _underlineHoverColor : SystemColors.ControlDark;
                    HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, _borderWidth, border);
                }
                else if (Enabled)
                {
                    // 直角沿用 Windows TextBox 原生凹陷立体边
                    ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Sunken);
                }
            }

            // 子类装饰区（按钮/图标），文字已裁剪到内容区不会与之重叠
            DrawAdornments(g);

            DrawTextAndSelection(g);
            if (UseDefaultFrame) DrawUnderline(g, rect);
            if (_sbVisible) DrawScrollbar(g);
        }

        /// <summary>裁剪到内容区后按滚动偏移绘制选区高亮与文字。</summary>
        private void DrawTextAndSelection(Graphics g)
        {
            var cr = ContentRect;
            Font f = ActiveFont;
            Color fg = Enabled ? ForeColor : SystemColors.GrayText;
            Color selBg = SystemColors.Highlight;
            Color selFg = SystemColors.HighlightText;

            var oldClip = g.Clip;
            g.SetClip(cr, CombineMode.Replace);

            // Pass 1（GDI+）：选区高亮背景。文字随后统一走 GDI ExtTextOut，
            // 故所有 GDI+ 绘制必须在 GetHdc 之前完成。
            int gs = Focused && Enabled ? SelStart : 0;
            int ge = Focused && Enabled ? SelEnd : 0;
            var runs = new List<(string text, int x, int y, Color color)>();
            for (int i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                string disp = DisplayOf(line.Text);
                int baseX = LineBaseX(i, cr, f);
                int top = LineTop(i, cr);
                if (top + _lineH < cr.Y || top > cr.Bottom) continue;

                int ls = Math.Max(line.Start, gs);
                int le = Math.Min(line.Start + line.Text.Length, ge);
                int o1 = Math.Max(0, Math.Min(disp.Length, ls - line.Start));
                int o2 = Math.Max(0, Math.Min(disp.Length, le - line.Start));
                if (o2 < o1) o2 = o1;
                int sx = baseX + TextWidth(disp.Substring(0, o1), f);
                int ex = baseX + TextWidth(disp.Substring(0, o2), f);
                if (o2 > o1)
                {
                    using (var b = new SolidBrush(selBg))
                        g.FillRectangle(b, new Rectangle(sx, top, ex - sx, _lineH - 2));
                }
                int y = top + 1;
                // 选区把一行拆成“前段-选中段-后段”分别上色（无选区时 o1=o2=0，整行走后段且 x=baseX）
                if (o1 > 0) runs.Add((disp.Substring(0, o1), baseX, y, fg));
                if (o2 > o1) runs.Add((disp.Substring(o1, o2 - o1), sx, y, selFg));
                if (o2 < disp.Length) runs.Add((disp.Substring(o2), ex, y, fg));
            }

            // 水印：内容为空时在内容区内绘制（单行垂直居中，水平跟随 TextAlign）
            bool drawWatermark = _text.Length == 0 && !string.IsNullOrEmpty(_watermarkText) && cr.Width > 0;
            int wmX = 0, wmY = 0;
            string wmText = null;
            if (drawWatermark)
            {
                EnsureMeasureHdc();
                string wm = HDrawPaint.EllipsisToFit(_watermarkText, f, cr.Width, _measureHdc);
                int ww = TextWidth(wm, f);
                string al = _textAlign.ToString();
                if (!_multiline && al.EndsWith("Right")) wmX = cr.Right - ww;
                else if (!_multiline && al.EndsWith("Center")) wmX = cr.X + (cr.Width - ww) / 2;
                else wmX = cr.X;
                wmY = (_multiline ? cr.Y : cr.Y + (cr.Height - _lineH) / 2) + 1;
                wmText = wm;
            }

            // Pass 2（GDI）：文字绘制。必须用 ExtTextOut 而非 TextRenderer.DrawText——
            // 插入符/选区/滚动全部按 GetTextExtentPoint32 的步进宽度定位，
            // DrawTextEx 会把引号等 overhang 字形的悬垂展开成额外空隙，二者无法逐像素对齐。
            IntPtr hdc = g.GetHdc();
            try
            {
                using (var tc = HDrawPaint.BeginGdiText(hdc, f, cr))
                {
                    foreach (var run in runs)
                        tc.DrawRun(run.text, run.x, run.y, run.color);
                    if (wmText != null)
                        tc.DrawRun(wmText, wmX, wmY, Enabled ? _watermarkColor : SystemColors.GrayText);
                }
            }
            finally { g.ReleaseHdc(hdc); }

            g.Clip = oldClip;
        }

        /// <summary>
        /// 下划线：常态灰，悬停/激活变色，圆角端点。
        /// 纵向只有上/中/下三种位置，贴着文字行走位；长度恒定为内容区通栏（控件宽减去左右内边距），
        /// 与文字长短、水平对齐无关。多行时保持内容区底部通栏。
        /// </summary>
        private void DrawUnderline(Graphics g, Rectangle rect)
        {
            Color color = !Enabled ? SystemColors.ControlDark :
                Focused ? _underlineActiveColor :
                _hover ? _underlineHoverColor : _underlineColor;

            int x1, x2, y;
            if (!_multiline && _lines.Count > 0)
            {
                var cr = ContentRect;
                x1 = cr.X;
                x2 = cr.Right;
                y = LineTop(0, cr) + _lineH + 1;
            }
            else
            {
                y = rect.Bottom - 3;
                // 端点随描边宽度内让：旧值 _radius+1 在粗边框下会压到圆角笔画
                int underlineInset = _radius > 0 ? _radius + Math.Max(1, _borderWidth) : 2;
                x1 = underlineInset;
                x2 = rect.Right - underlineInset;
            }
            using (var pen = new Pen(color, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(pen, x1, y, x2, y);
        }

        private void DrawScrollbar(Graphics g)
        {
            ScrollbarMetrics();
            using (var b = new SolidBrush(Color.FromArgb(40, SystemColors.ControlDark)))
                g.FillRectangle(b, _sbTrack);
            using (var b = new SolidBrush(SystemColors.ControlDark))
                g.FillRectangle(b, _sbThumb);
        }

        #endregion

        #region 通用事件与外形

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            PositionCaret();
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DestroyCaret();
            _caretCreated = false;
            Invalidate();
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
            // 窗口 DC 的 DPI 随显示器改变，缓存必须作废后按新 DPI 重排
            ReleaseMeasureHdc();
            RefreshLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ReleaseMeasureHdc();
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
            RefreshLayout();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            RefreshLayout();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            RefreshLayout(); // ApplyFitFont 内部按基础字体变化重算，或丢弃适配字体
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateRegion();
            RefreshLayout();
        }

        /// <summary>
        /// 不再用窗口 Region 裁剪圆角：曲线路径转 HRgn 会沿弧线保守舍入削掉 1~2px，圆角出折角。
        /// 四角改由 OnPaintBackground 铺父色透出（与 HPushButton/HRadioButton 同一方案）。
        /// </summary>
        protected void UpdateRegion()
        {
        }

        #endregion
    }
}