using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HFromUI.HFrom.Text
{
    using HFromUI.HColor;
    /// <summary>
    /// 完全自绘的 G 代码编辑器控件。
    /// 特性：无闪烁、语法高亮、行号、选择、剪贴板、双击选词、三击选行、行背景色（包括行号区域）、滚动居中。
    /// </summary>
    public class HGCodeEditor : Control
    {
        #region 内部数据
        /// <summary>_lines 字段。</summary>
        private List<string> _lines = new List<string>();
        /// <summary>_scrollOffsetY 字段。</summary>
        private int _scrollOffsetY = 0;
        /// <summary>_maxScrollY 字段。</summary>
        private int _maxScrollY = 0;
        private int _visibleLines;
        private int _lineHeight;

        /// <summary>_cursorLine 字段。</summary>
        private int _cursorLine = 0;
        /// <summary>_cursorCol 字段。</summary>
        private int _cursorCol = 0;
        /// <summary>_cursorVisible 字段。</summary>
        private bool _cursorVisible = true;
        private Timer _cursorTimer;
        private VScrollBar _vScrollBar;

        // 选择
        private bool _isSelecting = false;
        /// <summary>_selStartLine 字段。</summary>
        private int _selStartLine = -1, _selStartCol = -1;
        /// <summary>_selEndLine 字段。</summary>
        private int _selEndLine = -1, _selEndCol = -1;

        // 外观属性
        private bool _showLineNumbers = true;
        /// <summary>_lineNumberPercent 字段。</summary>
        private int _lineNumberPercent = 8;
        /// <summary>_minLineNumberWidth 字段。</summary>
        private int _minLineNumberWidth = 40;
        /// <summary>_currentLineNumberWidth 字段。</summary>
        private int _currentLineNumberWidth = 0;

        // 功能开关
        private bool _autoUpperCaseOnEnter = true;
        /// <summary>_autoIndentEnabled 字段。</summary>
        private bool _autoIndentEnabled = true;
        /// <summary>_indentSpaces 字段。</summary>
        private int _indentSpaces = 4;

        // 自定义行背景色（键为 1‑based 行号）—— 会同时影响行号区域和文本区域
        private Dictionary<int, Color> _lineBackgrounds = new Dictionary<int, Color>();

        // 颜色（使用 HColors）
        private Color _lineNumberColor = HColors.Grays.DimGray;
        /// <summary>_lineNumberBackColor 字段。</summary>
        private Color _lineNumberBackColor = HColors.Grays.Gainsboro;
        /// <summary>_gCodeColor 字段。</summary>
        private Color _gCodeColor = HColors.Blues.Blue;
        /// <summary>_mCodeColor 字段。</summary>
        private Color _mCodeColor = HColors.Oranges.DarkOrange;
        /// <summary>_axisWordColor 字段。</summary>
        private Color _axisWordColor = HColors.Cyans.DarkCyan;
        /// <summary>_commentColor 字段。</summary>
        private Color _commentColor = HColors.Greens.Green;
        /// <summary>_numberColor 字段。</summary>
        private Color _numberColor = HColors.Magentas.Magenta;
        /// <summary>_otherWordColor 字段。</summary>
        private Color _otherWordColor = HColors.Reds.DarkRed;
        /// <summary>_macroColor 字段。</summary>
        private Color _macroColor = HColors.Pinks.DeepPink;

        private const TextFormatFlags MeasureFlags =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        private const int BottomMarginLines = 3;
        #endregion

        #region 公开属性
        [Category("G代码编辑器"), DefaultValue(true)]
        public bool ShowLineNumbers
        {
            get => _showLineNumbers;
            set { _showLineNumbers = value; RecalcMetrics(); Invalidate(); }
        }
        [Category("G代码编辑器"), DefaultValue(8)]
        public int LineNumberPercent
        {
            get => _lineNumberPercent;
            set
            {
                if (value < 1) value = 1;
                if (value > 30) value = 30;
                _lineNumberPercent = value;
                RecalcMetrics();
                Invalidate();
            }
        }
        [Category("G代码编辑器"), DefaultValue(40)]
        public int MinLineNumberWidth
        {
            get => _minLineNumberWidth;
            set { _minLineNumberWidth = value; RecalcMetrics(); Invalidate(); }
        }
        [Category("G代码编辑器"), DefaultValue(true)]
        public bool AutoUpperCaseOnEnter
        {
            get => _autoUpperCaseOnEnter;
            set => _autoUpperCaseOnEnter = value;
        }
        [Category("G代码编辑器"), DefaultValue(true)]
        public bool AutoIndentEnabled
        {
            get => _autoIndentEnabled;
            set => _autoIndentEnabled = value;
        }
        [Category("G代码编辑器"), DefaultValue(4)]
        public int IndentSpaces
        {
            get => _indentSpaces;
            set { if (value < 0) value = 0; _indentSpaces = value; }
        }

        [Category("G代码编辑器颜色")] public Color GCodeColor { get => _gCodeColor; set => _gCodeColor = value; }
        [Category("G代码编辑器颜色")] public Color MCodeColor { get => _mCodeColor; set => _mCodeColor = value; }
        [Category("G代码编辑器颜色")] public Color AxisWordColor { get => _axisWordColor; set => _axisWordColor = value; }
        [Category("G代码编辑器颜色")] public Color CommentColor { get => _commentColor; set => _commentColor = value; }
        [Category("G代码编辑器颜色")] public Color NumberColor { get => _numberColor; set => _numberColor = value; }
        [Category("G代码编辑器颜色")] public Color OtherWordColor { get => _otherWordColor; set => _otherWordColor = value; }
        [Category("G代码编辑器颜色")] public Color MacroColor { get => _macroColor; set => _macroColor = value; }
        [Category("G代码编辑器颜色")] public Color LineNumberColor { get => _lineNumberColor; set => _lineNumberColor = value; }
        [Category("G代码编辑器颜色")] public Color LineNumberBackColor { get => _lineNumberBackColor; set => _lineNumberBackColor = value; }

        [Browsable(false)]
        public override string Text
        {
            get => string.Join(Environment.NewLine, _lines);
            set
            {
                _lines = new List<string>(value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
                if (_lines.Count == 0) _lines.Add(string.Empty);
                ClearSelection();
                ClampCursor();
                RecalcMetrics();
                Invalidate();
            }
        }

        [Browsable(false)]
        public string SelectedText
        {
            get
            {
                if (!HasSelection) return "";
                var (sL, sC, eL, eC) = GetOrderedSelection();
                if (eC > _lines[eL].Length) eC = _lines[eL].Length;
                return GetTextRange(sL, sC, eL, eC);
            }
        }
        #endregion

        #region 构造与初始化
        public HGCodeEditor()
        {
            // 启用 StandardDoubleClick 以接收双击和三击事件
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.Selectable |
                     ControlStyles.StandardDoubleClick, true);

            Font = new Font("Consolas", 10.5f);
            BackColor = Color.White;
            ForeColor = Color.Black;

            _cursorTimer = new Timer { Interval = 500 };
            _cursorTimer.Tick += (s, e) => { _cursorVisible = !_cursorVisible; InvalidateCursorArea(); };
            _cursorTimer.Start();

            _vScrollBar = new VScrollBar { SmallChange = 1, LargeChange = 5, Dock = DockStyle.Right };
            _vScrollBar.Scroll += (s, e) => { _scrollOffsetY = e.NewValue; Invalidate(); };
            Controls.Add(_vScrollBar);

            _lines.Add(string.Empty);
        }

        /// <summary>响应 HandleCreated 事件。</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RecalcMetrics();
            Invalidate();
        }
        #endregion

        #region 输入键
        /// <summary>判断是否 InputKey。</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            switch (keyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.Tab:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
        #endregion

        #region 鼠标滚轮
        /// <summary>响应 MouseWheel 事件。</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_vScrollBar.Visible)
            {
                int newValue = _vScrollBar.Value - e.Delta / 120 * _vScrollBar.LargeChange;
                newValue = Math.Max(0, Math.Min(newValue, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1));
                if (newValue != _vScrollBar.Value)
                {
                    _vScrollBar.Value = newValue;
                    _scrollOffsetY = newValue;
                    Invalidate();
                }
            }
            base.OnMouseWheel(e);
        }
        #endregion

        #region 选择与文本操作
        /// <summary>HasSelection 字段。</summary>
        private bool HasSelection =>
            _selStartLine != -1 && _selEndLine != -1 &&
            !(_selStartLine == _selEndLine && _selStartCol == _selEndCol);

        /// <summary>ClearSelection 方法。</summary>
        private void ClearSelection() { _selStartLine = _selEndLine = -1; _selStartCol = _selEndCol = -1; }

        private (int, int, int, int) GetOrderedSelection()
        {
            if (!HasSelection) return (0, 0, 0, 0);
            if (_selStartLine < _selEndLine || (_selStartLine == _selEndLine && _selStartCol <= _selEndCol))
                return (_selStartLine, _selStartCol, _selEndLine, _selEndCol);
            else
                return (_selEndLine, _selEndCol, _selStartLine, _selStartCol);
        }

        /// <summary>设置 selection。</summary>
        private void SetSelection(int aL, int aC, int cL, int cC)
        {
            _selStartLine = aL; _selStartCol = aC;
            _selEndLine = cL; _selEndCol = cC;
            _cursorLine = cL; _cursorCol = cC;
        }

        /// <summary>获取 textRange。</summary>
        private string GetTextRange(int sL, int sC, int eL, int eC)
        {
            if (sL == eL)
            {
                if (eC > _lines[sL].Length) eC = _lines[sL].Length;
                return _lines[sL].Substring(sC, eC - sC);
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(_lines[sL].Substring(sC));
            for (int i = sL + 1; i < eL; i++) sb.AppendLine(_lines[i]);
            if (eC > _lines[eL].Length) eC = _lines[eL].Length;
            sb.Append(_lines[eL].Substring(0, eC));
            return sb.ToString();
        }

        /// <summary>DeleteSelection 方法。</summary>
        private void DeleteSelection()
        {
            if (!HasSelection) return;
            var (sL, sC, eL, eC) = GetOrderedSelection();
            if (eC > _lines[eL].Length) eC = _lines[eL].Length;
            if (sL == eL) _lines[sL] = _lines[sL].Remove(sC, eC - sC);
            else { _lines[sL] = _lines[sL].Substring(0, sC) + _lines[eL].Substring(eC); _lines.RemoveRange(sL + 1, eL - sL); }
            ClearSelection();
            _cursorLine = sL; _cursorCol = sC;
            AdjustScrollBar();
            Invalidate();
        }
        #endregion

        #region 测量
        /// <summary>MeasureStringWidth 方法。</summary>
        private int MeasureStringWidth(Graphics g, string text) =>
            TextRenderer.MeasureText(g, text, Font, Size.Empty, MeasureFlags).Width;

        /// <summary>获取 columnX。</summary>
        private int GetColumnX(Graphics g, string line, int col)
        {
            if (col <= 0) return 0;
            if (col >= line.Length) return MeasureStringWidth(g, line);
            return MeasureStringWidth(g, line.Substring(0, col));
        }
        #endregion

        #region 绘制（行背景色同时影响行号区域）
        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (_showLineNumbers && _currentLineNumberWidth <= 0) { RecalcMetrics(); if (_currentLineNumberWidth <= 0) return; }
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int lineNumWidth = _currentLineNumberWidth;
            int textStartX = lineNumWidth + 2;
            int textAreaWidth = ClientSize.Width - lineNumWidth - (_vScrollBar.Visible ? _vScrollBar.Width : 0) - 2;

            // 绘制行号区域背景（默认颜色）和文本区域背景
            if (_showLineNumbers && lineNumWidth > 0)
                using (SolidBrush bg = new SolidBrush(_lineNumberBackColor))
                    g.FillRectangle(bg, 0, 0, lineNumWidth, ClientSize.Height);

            using (SolidBrush textBg = new SolidBrush(BackColor))
                g.FillRectangle(textBg, textStartX, 0, textAreaWidth, ClientSize.Height);

            int startLine = _scrollOffsetY;
            int endLine = Math.Min(_lines.Count, startLine + _visibleLines + 1);

            // 绘制自定义行背景（同时覆盖行号区域和文本区域）
            for (int i = startLine; i < endLine; i++)
            {
                if (_lineBackgrounds.TryGetValue(i + 1, out Color customColor))
                {
                    int y = (i - startLine) * _lineHeight;
                    // 行号区域
                    if (_showLineNumbers && lineNumWidth > 0)
                        using (SolidBrush b = new SolidBrush(customColor))
                            g.FillRectangle(b, 0, y, lineNumWidth, _lineHeight);
                    // 文本区域
                    using (SolidBrush b = new SolidBrush(customColor))
                        g.FillRectangle(b, textStartX, y, textAreaWidth, _lineHeight);
                }
            }

            // 绘制选区背景（包含空行的高亮处理）
            if (HasSelection)
                DrawSelectionBackground(g, textStartX);

            // 行号和文本
            for (int i = startLine; i < endLine; i++)
            {
                int y = (i - startLine) * _lineHeight;
                string line = _lines[i];
                // 行号文字颜色保持不变，背景已处理
                if (_showLineNumbers && lineNumWidth > 0)
                {
                    string num = (i + 1).ToString();
                    Rectangle numRect = new Rectangle(0, y, lineNumWidth - 4, _lineHeight);
                    TextRenderer.DrawText(g, num, Font, numRect, _lineNumberColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
                }
                DrawHighlightedLine(g, line, textStartX, y, i);
            }

            if (_showLineNumbers && lineNumWidth > 0)
                using (Pen pen = new Pen(Color.LightGray))
                    g.DrawLine(pen, lineNumWidth - 1, 0, lineNumWidth - 1, ClientSize.Height);

            if (Focused && _cursorVisible && !HasSelection)
                DrawCursor(g, textStartX);
        }

        /// <summary>DrawSelectionBackground 方法。</summary>
        private void DrawSelectionBackground(Graphics g, int textStartX)
        {
            var (sL, sC, eL, eC) = GetOrderedSelection();
            for (int i = sL; i <= eL; i++)
            {
                if (i < _scrollOffsetY || i >= _scrollOffsetY + _visibleLines + 1) continue;
                int y = (i - _scrollOffsetY) * _lineHeight;
                string line = _lines[i];
                int col1 = (i == sL) ? sC : 0;
                int col2 = (i == eL) ? Math.Min(eC, line.Length) : line.Length;

                // 空行或选区为零宽度时，显示一个字符宽度的蓝色高亮
                if (line.Length == 0 || col1 >= col2)
                {
                    int x = textStartX + GetColumnX(g, line, 0);
                    int w = MeasureStringWidth(g, "X");
                    using (SolidBrush brush = new SolidBrush(SystemColors.Highlight))
                        g.FillRectangle(brush, x, y, w, _lineHeight);
                    continue;
                }

                int x1 = textStartX + GetColumnX(g, line, col1);
                int x2 = textStartX + GetColumnX(g, line, col2);
                using (SolidBrush brush = new SolidBrush(SystemColors.Highlight))
                    g.FillRectangle(brush, new Rectangle(x1, y, x2 - x1, _lineHeight));
            }
        }

        /// <summary>DrawHighlightedLine 方法。</summary>
        private void DrawHighlightedLine(Graphics g, string line, int startX, int y, int lineIdx)
        {
            if (string.IsNullOrEmpty(line)) return;
            int x = startX;
            Regex regex = new Regex(@"(\([^\)]*\))|([A-Za-z#][+-]?\d+(\.\d+)?)|([+-]?\d+\.?\d*)|([A-Za-z]+)");
            MatchCollection matches = regex.Matches(line);
            int lastIdx = 0;
            bool inSel = HasSelection;
            var (sL, sC, eL, eC) = GetOrderedSelection();
            int localStart = (inSel && lineIdx >= sL && lineIdx <= eL) ? (lineIdx == sL ? sC : 0) : -1;
            int localEnd = (inSel && lineIdx >= sL && lineIdx <= eL) ? (lineIdx == eL ? Math.Min(eC, line.Length) : line.Length) : -1;

            foreach (Match m in matches)
            {
                if (m.Index > lastIdx)
                {
                    string before = line.Substring(lastIdx, m.Index - lastIdx);
                    DrawTextSegment(g, before, ref x, y, lineIdx, lastIdx, m.Index - 1, localStart, localEnd);
                }
                Color tokColor = ForeColor;
                if (m.Groups[1].Success) tokColor = _commentColor;
                else if (m.Groups[2].Success)
                {
                    char first = m.Value[0];
                    if (first == '#') tokColor = _macroColor;
                    else
                    {
                        char upper = char.ToUpperInvariant(first);
                        if (upper == 'G') tokColor = _gCodeColor;
                        else if (upper == 'M') tokColor = _mCodeColor;
                        else if ("XYZABCUVWIJK".IndexOf(upper) >= 0) tokColor = _axisWordColor;
                        else if (upper == 'N') tokColor = _numberColor;
                        else if ("FSTHDPQR".IndexOf(upper) >= 0) tokColor = _otherWordColor;
                    }
                }
                DrawTextSegment(g, m.Value, ref x, y, lineIdx, m.Index, m.Index + m.Length - 1, localStart, localEnd, tokColor);
                lastIdx = m.Index + m.Length;
            }
            if (lastIdx < line.Length)
                DrawTextSegment(g, line.Substring(lastIdx), ref x, y, lineIdx, lastIdx, line.Length - 1, localStart, localEnd);
        }

        /// <summary>DrawTextSegment 方法。</summary>
        private void DrawTextSegment(Graphics g, string text, ref int x, int y, int lineIdx,
            int segS, int segE, int localSelStart, int localSelEnd, Color? normalColor = null)
        {
            Color color = normalColor ?? ForeColor;
            if (localSelStart != -1 && localSelEnd > localSelStart)
            {
                int ovS = Math.Max(segS, localSelStart), ovE = Math.Min(segE + 1, localSelEnd);
                if (ovS < ovE)
                {
                    if (ovS > segS) { string ns = text.Substring(0, ovS - segS); int w = MeasureStringWidth(g, ns); TextRenderer.DrawText(g, ns, Font, new Point(x, y), color, MeasureFlags); x += w; }
                    string sel = text.Substring(ovS - segS, ovE - ovS); int ws = MeasureStringWidth(g, sel); TextRenderer.DrawText(g, sel, Font, new Point(x, y), SystemColors.HighlightText, MeasureFlags); x += ws;
                    if (ovE < segE + 1) { string af = text.Substring(ovE - segS); int w = MeasureStringWidth(g, af); TextRenderer.DrawText(g, af, Font, new Point(x, y), color, MeasureFlags); x += w; }
                    return;
                }
            }
            int width = MeasureStringWidth(g, text);
            TextRenderer.DrawText(g, text, Font, new Point(x, y), color, MeasureFlags);
            x += width;
        }

        /// <summary>DrawCursor 方法。</summary>
        private void DrawCursor(Graphics g, int textStartX)
        {
            if (_cursorLine < 0 || _cursorLine >= _lines.Count) return;
            int lineIdx = _cursorLine - _scrollOffsetY;
            if (lineIdx < 0 || lineIdx >= _visibleLines + 1) return;
            string line = _lines[_cursorLine];
            int col = Math.Min(_cursorCol, line.Length);
            int cursorX = textStartX + GetColumnX(g, line, col);
            int y = lineIdx * _lineHeight;
            using (Pen pen = new Pen(ForeColor, 1))
                g.DrawLine(pen, cursorX, y, cursorX, y + _lineHeight);
        }

        /// <summary>InvalidateCursorArea 方法。</summary>
        private void InvalidateCursorArea()
        {
            if (!IsHandleCreated) return;
            int lineIdx = _cursorLine - _scrollOffsetY;
            if (lineIdx < 0 || lineIdx >= _visibleLines + 1) return;
            string line = _lines[_cursorLine];
            int col = Math.Min(_cursorCol, line.Length);
            using (Graphics g = CreateGraphics())
            {
                int cursorX = _currentLineNumberWidth + 2 + GetColumnX(g, line, col);
                int y = lineIdx * _lineHeight;
                Invalidate(new Rectangle(cursorX - 1, y, 3, _lineHeight));
            }
        }
        #endregion

        #region 尺寸与滚动（底部留空）
        /// <summary>RecalcMetrics 方法。</summary>
        private void RecalcMetrics()
        {
            if (!IsHandleCreated || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            _lineHeight = Font.Height;
            _visibleLines = Math.Max(1, ClientSize.Height / _lineHeight);
            _currentLineNumberWidth = CalcLineNumberWidth();
            AdjustScrollBar();
        }

        /// <summary>CalcLineNumberWidth 方法。</summary>
        private int CalcLineNumberWidth()
        {
            if (!_showLineNumbers) return 0;
            int needed = 0;
            if (_lines.Count > 0)
                using (Graphics g = CreateGraphics())
                    needed = TextRenderer.MeasureText(_lines.Count.ToString(), Font, Size.Empty, MeasureFlags).Width + 8;
            int pct = (int)(ClientSize.Width * (_lineNumberPercent / 100.0));
            return Math.Max(Math.Max(pct, needed), _minLineNumberWidth);
        }

        /// <summary>AdjustScrollBar 方法。</summary>
        private void AdjustScrollBar()
        {
            int total = _lines.Count;
            _maxScrollY = Math.Max(0, total - 1 + BottomMarginLines);
            _vScrollBar.Maximum = _maxScrollY + _vScrollBar.LargeChange - 1;
            _vScrollBar.Value = Math.Min(_vScrollBar.Value, _maxScrollY);
            _vScrollBar.Visible = total + BottomMarginLines > _visibleLines;
            _scrollOffsetY = _vScrollBar.Value;
            _currentLineNumberWidth = CalcLineNumberWidth();
        }

        /// <summary>EnsureCursorVisible 方法。</summary>
        private void EnsureCursorVisible()
        {
            int visible = _visibleLines;
            if (visible <= 0) return;
            int keepMargin = Math.Min(3, visible / 2);
            if (_cursorLine < _scrollOffsetY + keepMargin)
                _scrollOffsetY = Math.Max(0, _cursorLine - keepMargin);
            else if (_cursorLine >= _scrollOffsetY + visible - keepMargin)
            {
                int newScroll = _cursorLine - visible + keepMargin + 1;
                if (newScroll > _maxScrollY - BottomMarginLines) newScroll = _maxScrollY - BottomMarginLines;
                _scrollOffsetY = Math.Max(0, newScroll);
            }
            _scrollOffsetY = Math.Max(0, Math.Min(_scrollOffsetY, _maxScrollY));
            _vScrollBar.Value = _scrollOffsetY;
        }

        /// <summary>ClampCursor 方法。</summary>
        private void ClampCursor()
        {
            if (_lines.Count == 0) _lines.Add(string.Empty);
            if (_cursorLine >= _lines.Count) _cursorLine = _lines.Count - 1;
            if (_cursorLine < 0) _cursorLine = 0;
            if (_cursorCol > _lines[_cursorLine].Length) _cursorCol = _lines[_cursorLine].Length;
        }

        /// <summary>ClampCursorCol 方法。</summary>
        private void ClampCursorCol()
        {
            if (_lines.Count == 0) return;
            if (_cursorCol > _lines[_cursorLine].Length) _cursorCol = _lines[_cursorLine].Length;
        }
        #endregion

        #region 键盘与剪贴板（含自动缩进）
        /// <summary>响应 KeyDown 事件。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool handled = true;
            bool shift = e.Shift, ctrl = e.Control;
            if (ctrl)
            {
                switch (e.KeyCode)
                {
                    case Keys.A: SelectAll(); break;
                    case Keys.C: Copy(); break;
                    case Keys.X: Cut(); break;
                    case Keys.V: Paste(); break;
                    default: handled = false; break;
                }
                if (handled) { e.Handled = true; Invalidate(); return; }
            }

            int newL = _cursorLine, newC = _cursorCol;
            switch (e.KeyCode)
            {
                case Keys.Left: if (_cursorCol > 0) newC--; else if (_cursorLine > 0) { newL--; newC = _lines[newL].Length; } break;
                case Keys.Right: if (_cursorCol < _lines[_cursorLine].Length) newC++; else if (_cursorLine < _lines.Count - 1) { newL++; newC = 0; } break;
                case Keys.Up: if (_cursorLine > 0) newL--; break;
                case Keys.Down: if (_cursorLine < _lines.Count - 1) newL++; break;
                case Keys.Home: newC = 0; break;
                case Keys.End: newC = _lines[_cursorLine].Length; break;
                case Keys.Enter:
                    if (HasSelection) DeleteSelection();
                    InsertNewLine();
                    if (_autoUpperCaseOnEnter && _cursorLine > 0) _lines[_cursorLine - 1] = _lines[_cursorLine - 1].ToUpperInvariant();
                    if (_autoIndentEnabled && _cursorLine > 0)
                    {
                        string prev = _lines[_cursorLine - 1];
                        string indent = GetIndent(prev);
                        if (prev.TrimEnd().EndsWith("(")) indent += new string(' ', _indentSpaces);
                        if (indent.Length > 0) { _lines[_cursorLine] = indent + _lines[_cursorLine]; _cursorCol = indent.Length; }
                    }
                    break;
                case Keys.Back: if (HasSelection) DeleteSelection(); else DeleteBack(); break;
                case Keys.Delete: if (HasSelection) DeleteSelection(); else DeleteForward(); break;
                case Keys.Tab: InsertText(new string(' ', _indentSpaces)); e.SuppressKeyPress = true; break;
                default: handled = false; break;
            }

            if (handled || shift)
            {
                if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
                {
                    if (!shift) ClearSelection();
                    else if (!HasSelection) SetSelection(_cursorLine, _cursorCol, _cursorLine, _cursorCol);
                    _cursorLine = newL; _cursorCol = newC; ClampCursorCol();
                    if (shift) { _selEndLine = _cursorLine; _selEndCol = _cursorCol; }
                }
                EnsureCursorVisible(); AdjustScrollBar(); Invalidate();
                e.Handled = true;
            }
            else base.OnKeyDown(e);
        }

        /// <summary>获取 indent。</summary>
        private string GetIndent(string line)
        {
            int i = 0;
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
            return line.Substring(0, i);
        }

        /// <summary>响应 KeyPress 事件。</summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (char.IsControl(e.KeyChar)) return;
            if (HasSelection) DeleteSelection();
            InsertText(e.KeyChar.ToString());
            EnsureCursorVisible(); AdjustScrollBar(); Invalidate();
            e.Handled = true;
        }

        /// <summary>InsertText 方法。</summary>
        private void InsertText(string text) { if (_lines.Count == 0) _lines.Add(string.Empty); string line = _lines[_cursorLine]; _lines[_cursorLine] = line.Substring(0, _cursorCol) + text + line.Substring(_cursorCol); _cursorCol += text.Length; }
        /// <summary>InsertNewLine 方法。</summary>
        private void InsertNewLine() { string line = _lines[_cursorLine]; _lines[_cursorLine] = line.Substring(0, _cursorCol); _lines.Insert(_cursorLine + 1, line.Substring(_cursorCol)); _cursorLine++; _cursorCol = 0; }
        /// <summary>DeleteBack 方法。</summary>
        private void DeleteBack() { if (_cursorCol > 0) { _lines[_cursorLine] = _lines[_cursorLine].Remove(_cursorCol - 1, 1); _cursorCol--; } else if (_cursorLine > 0) { string prev = _lines[_cursorLine - 1]; _lines.RemoveAt(_cursorLine); _lines[_cursorLine - 1] = prev + _lines[_cursorLine - 1]; _cursorLine--; _cursorCol = prev.Length; } }
        /// <summary>DeleteForward 方法。</summary>
        private void DeleteForward() { if (_cursorCol < _lines[_cursorLine].Length) _lines[_cursorLine] = _lines[_cursorLine].Remove(_cursorCol, 1); else if (_cursorLine < _lines.Count - 1) { _lines[_cursorLine] += _lines[_cursorLine + 1]; _lines.RemoveAt(_cursorLine + 1); } }
        /// <summary>SelectAll 方法。</summary>
        private void SelectAll()
        {
            _selStartLine = 0; _selStartCol = 0;
            _selEndLine = _lines.Count - 1;
            _selEndCol = _lines[_lines.Count - 1].Length;
            if (_selEndCol == 0) _selEndCol = 1;
            _cursorLine = _selEndLine; _cursorCol = _selEndCol;
        }
        /// <summary>Copy 方法。</summary>
        private void Copy() { if (HasSelection) Clipboard.SetText(SelectedText); }
        /// <summary>Cut 方法。</summary>
        private void Cut() { if (HasSelection) { Copy(); DeleteSelection(); Invalidate(); } }
        /// <summary>Paste 方法。</summary>
        private void Paste() { try { string text = Clipboard.GetText(); if (string.IsNullOrEmpty(text)) return; if (HasSelection) DeleteSelection(); string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None); if (lines.Length == 1) InsertText(lines[0]); else { string line = _lines[_cursorLine]; string left = line.Substring(0, _cursorCol), right = line.Substring(_cursorCol); _lines[_cursorLine] = left + lines[0]; for (int i = 1; i < lines.Length - 1; i++) _lines.Insert(_cursorLine + i, lines[i]); int pos = _cursorLine + lines.Length - 1; _lines.Insert(pos, lines[lines.Length - 1] + right); _cursorLine = pos; _cursorCol = lines[lines.Length - 1].Length; } EnsureCursorVisible(); AdjustScrollBar(); Invalidate(); } catch { } }
        #endregion

        #region 鼠标（已修复双击/三击）
        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Focused) Focus();

            // 三击：选中整行（空行也会显示高亮）
            if (e.Clicks == 3)
            {
                int lineNumW = _currentLineNumberWidth; if (e.X < lineNumW) return;
                int lineIdx = _scrollOffsetY + e.Y / _lineHeight;
                if (lineIdx < 0 || lineIdx >= _lines.Count) return;
                string line = _lines[lineIdx];
                int endCol = line.Length;
                if (endCol == 0) endCol = 1; // 空行则设置虚拟列
                SetSelection(lineIdx, 0, lineIdx, endCol);
                _isSelecting = false;
                Invalidate();
                return;
            }

            // 双击：由 OnMouseDoubleClick 处理，这里不处理
            if (e.Clicks == 2) return;

            // 单击：定位光标并开始拖选
            int lineNumW1 = _currentLineNumberWidth; if (e.X < lineNumW1) return;
            int lineIdx1 = _scrollOffsetY + e.Y / _lineHeight;
            if (lineIdx1 >= _lines.Count) lineIdx1 = _lines.Count - 1;
            if (lineIdx1 < 0) lineIdx1 = 0;
            string line1 = _lines[lineIdx1];
            int targetX = e.X - (lineNumW1 + 2), col = 0;
            using (Graphics g = CreateGraphics()) { while (col < line1.Length && GetColumnX(g, line1, col + 1) <= targetX) col++; }
            _cursorLine = lineIdx1; _cursorCol = col;
            ClearSelection();
            _isSelecting = true;
            SetSelection(lineIdx1, col, lineIdx1, col);
            Invalidate();
        }

        /// <summary>响应 MouseDoubleClick 事件。</summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int lineNumW = _currentLineNumberWidth; if (e.X < lineNumW) return;
            int lineIdx = _scrollOffsetY + e.Y / _lineHeight;
            if (lineIdx < 0 || lineIdx >= _lines.Count) return;
            string line = _lines[lineIdx];
            int targetX = e.X - (lineNumW + 2), col = 0;
            using (Graphics g = CreateGraphics()) { while (col < line.Length && GetColumnX(g, line, col + 1) <= targetX) col++; }
            int start = col, end = col;
            while (start > 0 && (char.IsLetterOrDigit(line[start - 1]) || line[start - 1] == '_')) start--;
            while (end < line.Length && (char.IsLetterOrDigit(line[end]) || line[end] == '_')) end++;
            if (start < end) { SetSelection(lineIdx, start, lineIdx, end); _isSelecting = false; Invalidate(); }
            else if (line.Length == 0) { SetSelection(lineIdx, 0, lineIdx, 1); _isSelecting = false; Invalidate(); }
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_isSelecting) return;
            int lineNumW = _currentLineNumberWidth; if (e.X < lineNumW) return;
            int lineIdx = _scrollOffsetY + e.Y / _lineHeight;
            if (lineIdx < 0) lineIdx = 0; if (lineIdx >= _lines.Count) lineIdx = _lines.Count - 1;
            string line = _lines[lineIdx];
            int targetX = e.X - (lineNumW + 2), col = 0;
            using (Graphics g = CreateGraphics()) { while (col < line.Length && GetColumnX(g, line, col + 1) <= targetX) col++; }
            _selEndLine = lineIdx; _selEndCol = col;
            _cursorLine = lineIdx; _cursorCol = col;
            Invalidate();
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isSelecting = false;
            if (HasSelection && _selStartLine == _selEndLine && _selStartCol == _selEndCol)
                ClearSelection();
            Invalidate();
        }
        #endregion

        #region 公开方法：行背景色（影响整行，包括行号区域）、滚动居中
        /// <summary>设置指定行的背景色（行号从1开始），同时影响行号区域和文本区域。</summary>
        public void SetLineBackground(int lineNumber, Color color)
        {
            if (lineNumber > 0 && lineNumber <= _lines.Count)
                _lineBackgrounds[lineNumber] = color;
            Invalidate();
        }

        /// <summary>清除所有自定义行背景色。</summary>
        public void ClearLineBackgrounds()
        {
            _lineBackgrounds.Clear();
            Invalidate();
        }

        /// <summary>将指定行滚动到垂直居中位置（行号从1开始）。</summary>
        public void ScrollToLineCenter(int lineNumber)
        {
            if (lineNumber > 0 && lineNumber <= _lines.Count)
            {
                int target = lineNumber - 1;
                int newScroll = target - _visibleLines / 2;
                newScroll = Math.Max(0, Math.Min(newScroll, _maxScrollY));
                _scrollOffsetY = newScroll;
                _vScrollBar.Value = newScroll;
                Invalidate();
            }
        }
        #endregion

        /// <summary>响应 GotFocus 事件。</summary>
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); _cursorVisible = true; InvalidateCursorArea(); }
        /// <summary>响应 LostFocus 事件。</summary>
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); InvalidateCursorArea(); }
        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); RecalcMetrics(); Invalidate(); }

        /// <summary>清空。</summary>
        public void Clear()
        {
            _lines = new List<string> { string.Empty };
            ClearSelection();
            _cursorLine = 0; _cursorCol = 0; _scrollOffsetY = 0;
            RecalcMetrics();
            Invalidate();
        }

        /// <summary>AppendText 方法。</summary>
        public void AppendText(string text)
        {
            string[] newLines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (_lines.Count > 0 && string.IsNullOrEmpty(_lines[_lines.Count - 1]))
                _lines.RemoveAt(_lines.Count - 1);
            _lines.AddRange(newLines);
            if (_lines.Count == 0) _lines.Add(string.Empty);
            RecalcMetrics();
            Invalidate();
        }

        /// <summary>ScrollToCaret 方法。</summary>
        public void ScrollToCaret()
        {
            EnsureCursorVisible();
            AdjustScrollBar();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cursorTimer?.Stop();
                _cursorTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
