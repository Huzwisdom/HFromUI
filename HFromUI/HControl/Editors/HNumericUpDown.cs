using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Tools.Nature;
using HFromUI.HMath;
namespace HFromUI.HControl.Editors
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 数值框外观风格：22 种——圆角、扁平、胶囊、下划线、3D 凹凸、工业/暗黑/霓虹面板、
    /// 语义色边框、分段按钮、步进圆钮、LCD 液晶屏等。
    /// </summary>
    public enum HEditStyle
    {
        Rounded = 0,       // 经典圆角
        FlatSquare = 1,    // 扁平直角
        Pill = 2,          // 胶囊
        Underline = 3,     // 下划线
        Sunken = 4,        // 3D 凹陷
        Raised = 5,        // 3D 凸起
        Industrial = 6,    // 工业深色面板
        Dark = 7,          // 暗黑
        Neon = 8,          // 霓虹
        GradientBorder = 9,// 渐变描边
        Glass = 10,        // 玻璃
        Minimal = 11,      // 极简单线
        ShadowCard = 12,   // 卡片投影
        Tech = 13,         // 科技蓝
        Gold = 14,         // 鎏金边
        Success = 15,      // 绿色
        Danger = 16,       // 红色
        Warning = 17,      // 琥珀
        Segmented = 18,    // 分段按钮
        Stepper = 19,      // 圆钮步进器
        Lcd = 20,          // LCD 液晶屏
        Transparent = 21   // 无边框
    }
    /// <summary>数值调节按钮排布。</summary>
    public enum HSpinLayout
    {
        EndVertical = 0,   // 都在后面，上下叠放（经典）
        EndHorizontal = 1, // 都在后面，左右并排
        FrontVertical = 2, // 都在前面，上下叠放
        FrontHorizontal = 3,// 都在前面，左右并排
        Split = 4          // 一前一后（前减后加）
    }
    /// <summary>
    /// 纯自绘数值框（继承 HTextBoxBase）：22 种外观风格；两个调节箭头可自由选用
    /// <see cref="HArrowType"/>（默认 V 形），五种排布；支持小数位、千分位、键盘上下键、
    /// 滚轮与长按连发。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [HDescriptionLanguage("数值框：22种外观风格，5种箭头排布，箭头种类可选，支持小数/千分位")]
    public class HNumericUpDown : HTextBoxBase
    {
        #region 字段
        private HEditStyle _style = HEditStyle.Rounded;
        private HSpinLayout _layout = HSpinLayout.EndVertical;
        private HArrowType _upType = HArrowType.Chevron;
        private HArrowType _downType = HArrowType.Chevron;
        private HArrowStyle _arrowStyle = HArrowStyle.Flat;
        private decimal _value;
        private decimal _minimum;
        private decimal _maximum = 100m;
        private decimal _increment = 1m;
        private int _decimalPlaces;
        private bool _thousandsSeparator;
        private bool _trimTrailingZeros = true;
        private float _iconScale = 1f;
        private bool _syncing;
        private RectangleF[] _btns = new RectangleF[2];
        private int _hover = -1;
        private int _press = -1;
        private readonly Timer _repeat;
        private int _repeatStep;
        #endregion
        public HNumericUpDown()
        {
            Radius = 0;
            TextAlign = ContentAlignment.MiddleCenter;
            Size = new Size(132, 32);
            ApplyStyleColors();
            _repeat = new Timer { Interval = 70 };
            _repeat.Tick += RepeatTick;
            Text = FormatValue(_value, true);
        }
        #region 属性
        /// <summary>外观风格（22 种）。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("数值框外观风格"), HDescriptionLanguage("数值框外观风格（22种）"), Browsable(true)]
        [DefaultValue(HEditStyle.Rounded)]
        public HEditStyle NumStyle
        {
            get => _style;
            set { _style = value; ApplyStyleColors(); UpdateRegion(); Invalidate(); }
        }
        /// <summary>调节箭头排布方式。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("调节箭头排布"), HDescriptionLanguage("调节箭头排布：后上下/后并排/前上下/前并排/一前一后"), Browsable(true)]
        [DefaultValue(HSpinLayout.EndVertical)]
        public HSpinLayout SpinLayout
        {
            get => _layout;
            set { _layout = value; RefreshLayout(); Invalidate(); }
        }
        /// <summary>上调（加号）按钮的箭头种类，可从 HArrow 的 84 种箭头/符号中选择。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("上调按钮箭头种类"), HDescriptionLanguage("上调按钮箭头种类（84种箭头/加减乘除符号）"), Browsable(true)]
        [DefaultValue(HArrowType.Chevron)]
        public HArrowType UpArrowType
        {
            get => _upType;
            set { _upType = value; Invalidate(); }
        }
        /// <summary>下调（减号）按钮的箭头种类。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("下调按钮箭头种类"), HDescriptionLanguage("下调按钮箭头种类（84种箭头/加减乘除符号）"), Browsable(true)]
        [DefaultValue(HArrowType.Chevron)]
        public HArrowType DownArrowType
        {
            get => _downType;
            set { _downType = value; Invalidate(); }
        }
        /// <summary>按钮箭头渲染风格。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("按钮箭头渲染风格"), HDescriptionLanguage("按钮箭头渲染风格（22种）"), Browsable(true)]
        [DefaultValue(HArrowStyle.Flat)]
        public HArrowStyle ArrowStyleKind
        {
            get => _arrowStyle;
            set { _arrowStyle = value; Invalidate(); }
        }
        /// <summary>当前数值。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("当前数值"), HDescriptionLanguage("当前数值"), Browsable(true)]
        public decimal Value
        {
            get => _value;
            set => SetValue(value, true);
        }
        /// <summary>最小值。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("最小值"), HDescriptionLanguage("最小值"), Browsable(true)]
        public decimal Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                if (_maximum < _minimum) _maximum = _minimum;
                SetValue(_value, true);
            }
        }
        /// <summary>最大值。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("最大值"), HDescriptionLanguage("最大值"), Browsable(true)]
        public decimal Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                if (_minimum > _maximum) _minimum = _maximum;
                SetValue(_value, true);
            }
        }
        /// <summary>步长。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("每次调节步长"), HDescriptionLanguage("每次调节步长"), Browsable(true)]
        public decimal Increment
        {
            get => _increment;
            set => _increment = Math.Max(0m, value);
        }
        /// <summary>小数位数（0..6）。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("小数显示位数"), HDescriptionLanguage("小数显示位数"), Browsable(true)]
        [DefaultValue(0)]
        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set
            {
                _decimalPlaces = Math.Max(0, Math.Min(6, value));
                SetValue(_value, true);
            }
        }
        /// <summary>是否显示千分位分隔符。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("是否显示千分位分隔符"), HDescriptionLanguage("是否显示千分位分隔符"), Browsable(true)]
        [DefaultValue(false)]
        public bool ThousandsSeparator
        {
            get => _thousandsSeparator;
            set { _thousandsSeparator = value; SetValue(_value, true); }
        }
        /// <summary>
        /// 失焦后是否去掉小数末尾多余的 0（如 90.000 显示为 90、90.500 显示为 90.5）。
        /// 仅在离开控件（或外部赋值且未聚焦）时生效；输入过程中始终保留实际按键内容，不做格式化。
        /// </summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("失焦去掉末尾零"), HDescriptionLanguage("离开控件后去掉小数末尾多余的0（90.000 显示为 90）；输入过程中不处理"), Browsable(true)]
        [DefaultValue(true)]
        public bool TrimTrailingZeros
        {
            get => _trimTrailingZeros;
            set { _trimTrailingZeros = value; SetValue(_value, true); }
        }
        /// <summary>调节按钮箭头大小比例：1 为默认大小，按比例放大或缩小（0.5~3）。</summary>
        [HCategoryLanguage("数值选择框"), HDisplayNameLanguage("图标大小比例"), HDescriptionLanguage("调节按钮箭头相对默认大小的倍数，1 为默认"), Browsable(true)]
        [DefaultValue(1f)]
        public float IconScale
        {
            get => _iconScale;
            set { _iconScale = Math.Max(0.5f, Math.Min(3f, value)); Invalidate(); }
        }
        /// <summary>数值变化事件。</summary>
        public event EventHandler ValueChanged;
        #endregion
        #region 数值与文本
        private string FormatValue(decimal v, bool trim)
        {
            if (trim && _trimTrailingZeros)
            {
                // 去尾零（90.000→90、90.500→90.5、90.001→90.001）；保留千分位习惯
                string s = v.ToString(_thousandsSeparator ? "N" + _decimalPlaces : "F" + _decimalPlaces,
                    CultureInfo.CurrentCulture);
                string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                int dot = s.IndexOf(sep, StringComparison.Ordinal);
                if (dot >= 0)
                {
                    int end = s.Length;
                    while (end > dot && s[end - 1] == '0') end--;
                    if (end == dot + sep.Length) end -= sep.Length;
                    s = s.Substring(0, end);
                }
                return s;
            }
            if (_decimalPlaces > 0)
                return v.ToString("N" + _decimalPlaces, CultureInfo.CurrentCulture);
            return _thousandsSeparator
                ? v.ToString("N0", CultureInfo.CurrentCulture)
                : v.ToString(CultureInfo.CurrentCulture);
        }
        private void SetValue(decimal v, bool syncText)
        {
            v = Math.Max(_minimum, Math.Min(_maximum, v));
            if (_decimalPlaces > 0)
                v = Math.Round(v, _decimalPlaces);
            bool changed = v != _value;
            _value = v;
            if (syncText)
            {
                // 聚焦中（含步进按钮、滚轮、键盘调节与正在输入）保留定宽格式；
                // 失焦/回车提交或未聚焦时的外部赋值才去尾零
                _syncing = true;
                Text = FormatValue(v, !Focused);
                _syncing = false;
            }
            if (changed) { try { ValueChanged?.Invoke(this, EventArgs.Empty); } catch { } }
            Invalidate();
        }
        /// <summary>提交正在编辑的文本（回车/失焦），非法则回退；提交后立即按规则去尾零。</summary>
        private void CommitText()
        {
            decimal v;
            if (decimal.TryParse(Text, NumberStyles.Number, CultureInfo.CurrentCulture, out v))
            {
                v = Math.Round(Math.Max(_minimum, Math.Min(_maximum, v)), _decimalPlaces);
                bool changed = v != _value;
                _value = v;
                _syncing = true;
                Text = FormatValue(v, true);
                _syncing = false;
                if (changed) { try { ValueChanged?.Invoke(this, EventArgs.Empty); } catch { } }
                Invalidate();
            }
            else
            {
                _syncing = true;
                Text = FormatValue(_value, true);
                _syncing = false;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (_syncing) return;
            // 键入过程中只做范围收敛，不打断输入；回车/失焦再格式化
            decimal v;
            if (decimal.TryParse(Text, NumberStyles.Number, CultureInfo.CurrentCulture, out v))
            {
                bool changed = Math.Round(v, _decimalPlaces) != _value;
                _value = Math.Max(_minimum, Math.Min(_maximum, v));
                if (changed) { try { ValueChanged?.Invoke(this, EventArgs.Empty); } catch { } }
            }
        }
        private void Step(int dir)
        {
            Focus();
            SetValue(_value + _increment * dir, true);
        }
        #endregion
        #region 键盘 / 滚轮
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up: Step(1); e.SuppressKeyPress = true; return;
                case Keys.Down: Step(-1); e.SuppressKeyPress = true; return;
                case Keys.PageUp: SetValue(_value + _increment * 10, true); e.SuppressKeyPress = true; return;
                case Keys.PageDown: SetValue(_value - _increment * 10, true); e.SuppressKeyPress = true; return;
                case Keys.Enter: CommitText(); e.SuppressKeyPress = true; return;
            }
            base.OnKeyDown(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string grp = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            bool ok = char.IsControl(c) || char.IsDigit(c) || c == '-' ||
                     (_decimalPlaces > 0 && sep.IndexOf(c) >= 0) ||
                     (_thousandsSeparator && grp.IndexOf(c) >= 0);
            if (!ok) { e.Handled = true; return; }
            base.OnKeyPress(e);
        }
        protected override void OnLostFocus(EventArgs e)
        {
            CommitText();
            base.OnLostFocus(e);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Step(e.Delta > 0 ? 1 : -1);
            // 不调用 base：单行基类无滚轮动作，但避免触发下划线等无关逻辑
        }
        #endregion
        #region 装饰按钮
        protected override bool UseDefaultFrame => false;
        private void ApplyStyleColors()
        {
            BackColor = HEditFrame.BackColor(_style);
            ForeColor = HEditFrame.ForeColor(_style);
        }
        // 底色只在框体外形内（四角的父色铺底由基类 OnPaintBackground 完成）
        protected override void FillShapeBackground(Graphics g, Rectangle rect)
            => HEditFrame.FillBackground(g, rect, _style, ShapeFillColor, Enabled, Radius);
        // 自绘框满高：内容区上下等距，文字/数字在框内严格垂直居中（九方位以此为准）
        protected override Rectangle AdjustContentRect(Rectangle cr)
            => new Rectangle(cr.X, 2, cr.Width, Math.Max(0, Height - 4));
        private int BandWidth
        {
            get
            {
                int h = Math.Max(10, Height);
                switch (_layout)
                {
                    case HSpinLayout.EndVertical:
                    case HSpinLayout.FrontVertical:
                        return Math.Max(16, (int)(h * 0.62f));
                    case HSpinLayout.Split:
                        return Math.Max(16, (int)(h * 0.55f));
                    default:
                        return Math.Max(32, (int)(h * 1.05f));
                }
            }
        }
        protected override int AdornmentLeftWidth
        {
            get
            {
                switch (_layout)
                {
                    case HSpinLayout.FrontVertical:
                    case HSpinLayout.FrontHorizontal:
                    case HSpinLayout.Split:
                        return BandWidth + 2;
                    default:
                        return 0;
                }
            }
        }
        protected override int AdornmentRightWidth
        {
            get
            {
                switch (_layout)
                {
                    case HSpinLayout.EndVertical:
                    case HSpinLayout.EndHorizontal:
                    case HSpinLayout.Split:
                        return BandWidth + 2;
                    default:
                        return 0;
                }
            }
        }
        private void LayoutButtons()
        {
            int bw = BandWidth;
            int h = Height;
            const int m = 2;
            switch (_layout)
            {
                case HSpinLayout.EndVertical:
                case HSpinLayout.FrontVertical:
                    {
                        int x = _layout == HSpinLayout.EndVertical ? Width - bw - m : m;
                        float bh = (h - m * 2) / 2f;
                        _btns[0] = new RectangleF(x, m, bw - 1, bh - 0.5f);
                        _btns[1] = new RectangleF(x, m + bh + 0.5f, bw - 1, bh - 0.5f);
                    }
                    break;
                case HSpinLayout.EndHorizontal:
                case HSpinLayout.FrontHorizontal:
                    {
                        float btnW = (bw - 1) / 2f;
                        int x0 = _layout == HSpinLayout.EndHorizontal ? Width - bw - m : m;
                        _btns[0] = new RectangleF(x0 + btnW, m, btnW, h - m * 2 - 1);
                        _btns[1] = new RectangleF(x0, m, btnW, h - m * 2 - 1);
                    }
                    break;
                case HSpinLayout.Split:
                    {
                        _btns[1] = new RectangleF(m, m, bw - 1, h - m * 2 - 1);
                        _btns[0] = new RectangleF(Width - bw - m, m, bw - 1, h - m * 2 - 1);
                    }
                    break;
            }
        }
        protected override void DrawAdornments(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            LayoutButtons();
            HEditFrame.DrawFrame(g, ClientRectangle, _style, Focused, Enabled, _hover >= 0 || _press >= 0, Radius);
            // 装饰按钮统一裁到框体外形内：大圆角/胶囊时悬停底色不得溢出圆角外
            Region oldClip = g.Clip;
            using (GraphicsPath clip = HEditFrame.CreateRegionPath(new RectangleF(0, 0, Width, Height), _style, Radius))
            {
                if (clip != null) g.SetClip(clip, CombineMode.Intersect);
                DrawOneButton(g, 0, _upType);
                DrawOneButton(g, 1, _downType);
            }
            g.Clip = oldClip;
        }
        private void DrawOneButton(Graphics g, int idx, HArrowType type)
        {
            var r = _btns[idx];
            if (r.Width <= 1f || r.Height <= 1f) return;
            int state = _press == idx ? 2 : _hover == idx ? 1 : 0;
            bool verticalStack = _layout == HSpinLayout.EndVertical || _layout == HSpinLayout.FrontVertical;
            HArrowDirection dir;
            if ((int)type >= 70) dir = HArrowDirection.Right;   // 运算符号无方向
            else if (verticalStack) dir = idx == 0 ? HArrowDirection.Up : HArrowDirection.Down;
            else dir = idx == 0 ? HArrowDirection.Right : HArrowDirection.Left;
            HEditFrame.DrawButton(g, r, _style, state, Enabled, idx, _layout);
            Color glyph = HEditFrame.GlyphColor(_style, state, Enabled);
            var glyphArea = RectangleF.Inflate(r, -r.Width * 0.22f, -r.Height * 0.22f);
            // V 形类图元本身只占约 1/3 边长，在按钮里整体放大 1.3 倍（默认），
            // 水平并排（窄高矩形）时尤其需要，否则只剩一个小点；IconScale 在默认基础上等比缩放，
            // 比例过大会被钳制在按钮区内
            float m = Math.Min(glyphArea.Width, glyphArea.Height);
            float fit = (m * 0.5f - 1f) / (0.45f * m * 0.5f);
            float zoom = 1.3f * Math.Min(_iconScale, Math.Max(0.5f, fit));
            float cx = glyphArea.X + glyphArea.Width / 2f;
            float cy = glyphArea.Y + glyphArea.Height / 2f;
            GraphicsState gs = g.Save();
            g.TranslateTransform(cx, cy);
            g.ScaleTransform(zoom, zoom);
            g.TranslateTransform(-cx, -cy);
            HArrowGlyph.Draw(g, glyphArea, type, _arrowStyle, dir, glyph, 0f);
            g.Restore(gs);
        }
        protected override bool AdornmentMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return false;
            LayoutButtons();
            for (int i = 0; i < 2; i++)
            {
                if (_btns[i].Contains(e.Location))
                {
                    _press = i;
                    _hover = i;
                    Capture = true;
                    Step(i == 0 ? 1 : -1);
                    _repeatStep = i == 0 ? 1 : -1;
                    var t = new Timer { Interval = 380 };
                    t.Tick += (s, ev) =>
                    {
                        t.Stop();
                        t.Dispose();
                        if (_press >= 0 && Capture) _repeat.Start();
                    };
                    t.Start();
                    _pending = t;
                    Invalidate();
                    return true;
                }
            }
            return false;
        }
        private Timer _pending;
        protected override void AdornmentMouseUp(MouseEventArgs e)
        {
            StopRepeat();
            if (_press >= 0) { _press = -1; Capture = false; Invalidate(); }
        }
        protected override void AdornmentMouseMove(MouseEventArgs e)
        {
            LayoutButtons();
            int oldHover = _hover, oldPress = _press;
            int h = -1;
            for (int i = 0; i < 2; i++)
                if (_btns[i].Contains(e.Location)) h = i;
            _hover = h;
            if (_press >= 0) _press = h;
            if (oldHover != _hover || oldPress != _press) Invalidate();
        }
        protected override void AdornmentMouseLeave()
        {
            if (_hover >= 0 && _press < 0) { _hover = -1; Invalidate(); }
        }
        private void RepeatTick(object sender, EventArgs e)
        {
            if (_press >= 0) Step(_repeatStep);
            else StopRepeat();
        }
        private void StopRepeat()
        {
            _repeat.Stop();
            if (_pending != null) { _pending.Stop(); _pending.Dispose(); _pending = null; }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopRepeat();
                _repeat.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
    /// <summary>数值框/搜索框共用的 22 风格边框与按钮绘制。</summary>
    internal static class HEditFrame
    {
        private static readonly Color[] Accents =
        {
            Color.FromArgb(74,111,165), // 0 Rounded
            Color.FromArgb(120,126,134),// 1 Flat
            Color.FromArgb(74,111,165), // 2 Pill
            Color.FromArgb(66,130,220),// 3 Underline
            Color.FromArgb(120,126,134),// 4 Sunken
            Color.FromArgb(120,126,134),// 5 Raised
            Color.FromArgb(245,176,65),// 6 Industrial
            Color.FromArgb(96,140,220),// 7 Dark
            Color.FromArgb(64,224,230),// 8 Neon
            Color.FromArgb(74,111,165),// 9 Gradient
            Color.FromArgb(190,205,230),//10 Glass
            Color.FromArgb(150,155,162),//11 Minimal
            Color.FromArgb(74,111,165),//12 Shadow
            Color.FromArgb(47,125,225),//13 Tech
            Color.FromArgb(198,156,60),//14 Gold
            Color.FromArgb(46,168,90), //15 Success
            Color.FromArgb(214,64,64), //16 Danger
            Color.FromArgb(232,150,32),//17 Warning
            Color.FromArgb(74,111,165),//18 Segmented
            Color.FromArgb(47,125,225),//19 Stepper
            Color.FromArgb(120,220,150),//20 LCD
            Color.FromArgb(120,126,134)//21 Transparent
        };
        public static Color Accent(HEditStyle s) => Accents[(int)s];
        public static Color BackColor(HEditStyle s)
        {
            switch (s)
            {
                case HEditStyle.Industrial: return Color.FromArgb(59, 64, 71);
                case HEditStyle.Dark: return Color.FromArgb(32, 36, 42);
                case HEditStyle.Neon: return Color.FromArgb(15, 20, 27);
                case HEditStyle.Lcd: return Color.FromArgb(10, 30, 20);
                // 无闭合框体的风格不铺底色，背景完全透出父容器
                case HEditStyle.Transparent:
                case HEditStyle.Underline:
                case HEditStyle.Minimal:
                    return Color.Transparent;
                default: return Color.White;
            }
        }
        /// <summary>框体外形：无框（下划线/极简/透明）、直角矩形、圆角。</summary>
        private enum FrameShapeKind { None, Rect, Round }
        private static FrameShapeKind ShapeKind(HEditStyle s)
        {
            switch (s)
            {
                case HEditStyle.Underline:
                case HEditStyle.Minimal:
                case HEditStyle.Transparent:
                    return FrameShapeKind.None;
                case HEditStyle.FlatSquare:
                case HEditStyle.Sunken:
                case HEditStyle.Raised:
                case HEditStyle.Industrial:
                case HEditStyle.Dark:
                    return FrameShapeKind.Rect;
                default:
                    return FrameShapeKind.Round;
            }
        }
        /// <summary>圆角外形的直径参数（与 DrawFrame 描边半径保持一致，避免底出框）；radiusOverride&gt;0 时覆盖风格默认值。</summary>
        private static float ShapeRadius(RectangleF r, HEditStyle s, float radiusOverride)
        {
            if (radiusOverride > 0) return radiusOverride;
            switch (s)
            {
                case HEditStyle.Pill: return r.Height / 2f - 1f;
                case HEditStyle.Glass: return r.Height / 2.5f;
                case HEditStyle.Lcd: return 6f;
                default: return 8f;
            }
        }
        /// <summary>
        /// 窗口裁剪外形：圆角风格返回圆角路径（框外区域完全透明、不接收鼠标），
        /// 直角/无框风格返回 null（矩形窗口，无框风格靠透明底色透出父容器）。
        /// </summary>
        public static GraphicsPath CreateRegionPath(RectangleF r, HEditStyle s, float radiusOverride = 0f)
        {
            if (ShapeKind(s) != FrameShapeKind.Round) return null;
            // 与全库统一：外轮廓固定像素盒 (-0.5,-0.5)-(W-0.5,H-0.5)
            return HDrawPaint.CreateOuterBoxPath(r.Width, r.Height, ShapeRadius(r, s, radiusOverride));
        }
        /// <summary>
        /// 框内底色填充：圆角风格只在圆角路径内填底，直角风格整矩形填，无框风格不填。
        /// 这是“背景颜色只在框里、框外 Transparent”的绘制半边（另半边是窗口 Region）。
        /// radiusOverride&gt;0（控件 Radius 属性）时覆盖风格默认圆角直径。
        /// </summary>
        public static void FillBackground(Graphics g, Rectangle r, HEditStyle s, Color color, bool enabled, float radiusOverride = 0f)
        {
            FrameShapeKind kind = ShapeKind(s);
            if (kind == FrameShapeKind.None || color.A == 0) return;
            Color c = enabled ? color : Mix(color, SystemColors.Control, 0.5);
            using (var b = new SolidBrush(c))
            {
                if (kind == FrameShapeKind.Rect)
                {
                    // 外扩 1px 再由客户区裁剪，避免双缓冲合成时四边半像素黑晕
                    g.FillRectangle(b, -1, -1, r.Width + 2, r.Height + 2);
                    return;
                }
                using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(r.Width, r.Height,
                              ShapeRadius(new RectangleF(0, 0, r.Width, r.Height), s, radiusOverride)))
                    g.FillPath(b, path);
            }
        }
        public static Color ForeColor(HEditStyle s)
        {
            switch (s)
            {
                case HEditStyle.Industrial:
                case HEditStyle.Dark:
                case HEditStyle.Neon:
                    return Color.FromArgb(228, 232, 238);
                case HEditStyle.Lcd: return Color.FromArgb(134, 255, 160);
                default: return Color.FromArgb(33, 38, 45);
            }
        }
        private static bool DarkPanel(HEditStyle s)
            => s == HEditStyle.Industrial || s == HEditStyle.Dark || s == HEditStyle.Neon;
        public static Color GlyphColor(HEditStyle s, int state, bool enabled)
        {
            if (!enabled) return SystemColors.GrayText;
            Color c;
            if (s == HEditStyle.Lcd) c = state == 0 ? Color.FromArgb(70, 76, 84) : Color.FromArgb(30, 60, 110);
            else if (s == HEditStyle.Stepper) c = Color.White;
            else if (DarkPanel(s)) c = state == 0 ? Color.FromArgb(190, 196, 204) : Color.White;
            else c = state == 0 ? Color.FromArgb(96, 102, 110) : Accent(s);
            return c;
        }
        public static void DrawFrame(Graphics g, Rectangle r, HEditStyle s, bool focused, bool enabled, bool hover, float radiusOverride = 0f)
        {
            if (r.Width <= 2 || r.Height <= 2) return;
            Color acc = enabled ? Accent(s) : SystemColors.ControlDark;
            RectangleF rf = new RectangleF(0, 0.5f, r.Width - 1, r.Height - 2);
            switch (s)
            {
                case HEditStyle.FlatSquare:
                    using (var p = new Pen(FocusedOr(focused, hover, acc, Color.FromArgb(190, 195, 202))))
                        g.DrawRectangle(p, 0, 0, r.Width - 1, r.Height - 1);
                    break;
                case HEditStyle.Rounded:
                case HEditStyle.Gold:
                case HEditStyle.Success:
                case HEditStyle.Danger:
                case HEditStyle.Warning:
                case HEditStyle.Tech:
                case HEditStyle.Pill:
                    {
                        // 实体圆角边框统一为像素盒向内填充环：改边宽外沿不动，四边等宽
                        float rad = ShapeRadius(r, s, radiusOverride);
                        Color border = focused ? acc : Color.FromArgb(201, 206, 214);
                        HDrawPaint.FillRoundedBorder(g, r.Width, r.Height, rad, focused ? 1.6f : 1f, border);
                    }
                    break;
                case HEditStyle.Underline:
                case HEditStyle.Minimal:
                    {
                        float y = r.Bottom - (s == HEditStyle.Underline ? 2.5f : 1f);
                        using (var b = new SolidBrush(focused ? acc : Color.FromArgb(185, 190, 198)))
                            g.FillRectangle(b, 1, y, r.Width - 2, s == HEditStyle.Underline ? 2f : 1f);
                    }
                    break;
                case HEditStyle.Sunken:
                    ControlPaint.DrawBorder3D(g, r, Border3DStyle.Sunken);
                    break;
                case HEditStyle.Raised:
                    ControlPaint.DrawBorder3D(g, r, Border3DStyle.Raised);
                    break;
                case HEditStyle.Industrial:
                    using (var p = new Pen(Color.FromArgb(28, 31, 36))) g.DrawRectangle(p, 0, 0, r.Width - 1, r.Height - 1);
                    break;
                case HEditStyle.Dark:
                    using (var p = new Pen(focused ? acc : Color.FromArgb(64, 70, 78)))
                        g.DrawRectangle(p, 0.5f, 0.5f, r.Width - 2, r.Height - 2);
                    break;
                case HEditStyle.Neon:
                    DrawGlowFrame(g, rf, acc, focused ? 90 : 34, 6f, ShapeRadius(rf, s, radiusOverride));
                    DrawGlowFrame(g, rf, acc, focused ? 160 : 80, 1.4f, ShapeRadius(rf, s, radiusOverride));
                    break;
                case HEditStyle.GradientBorder:
                    using (var path = Round(rf, ShapeRadius(rf, s, radiusOverride)))
                    using (var lb = new LinearGradientBrush(rf, Light(acc, 0.45f), Dark(acc, 0.25f), 0f))
                    using (var p = new Pen(lb, focused ? 2f : 1.3f))
                        g.DrawPath(p, path);
                    break;
                case HEditStyle.Glass:
                    using (var path = Round(rf, ShapeRadius(rf, s, radiusOverride)))
                    using (var p = new Pen(Color.FromArgb(focused ? 200 : 120, 255, 255, 255), 1.2f))
                        g.DrawPath(p, path);
                    break;
                case HEditStyle.ShadowCard:
                    // 卡片投影全部收在圆角区域内，避免被窗口外形裁掉
                    for (int i = 0; i < 3; i++)
                        using (var b = new SolidBrush(Color.FromArgb(22 - i * 6, 40, 44, 52)))
                            g.FillRectangle(b, 6 + i, r.Bottom - 4 + i * 0.8f, r.Width - 12 - i * 2f, 1.6f);
                    break;
                case HEditStyle.Segmented:
                    HDrawPaint.FillRoundedBorder(g, r.Width, r.Height,
                        ShapeRadius(r, s, radiusOverride), 1f, focused ? acc : Color.FromArgb(201, 206, 214));
                    break;
                case HEditStyle.Stepper:
                    HDrawPaint.FillRoundedBorder(g, r.Width, r.Height,
                        ShapeRadius(r, s, radiusOverride), 1f, Color.FromArgb(222, 226, 232));
                    break;
                case HEditStyle.Lcd:
                    // 液晶屏 + 银灰金属外框（填充环，外轮廓固定像素盒）
                    using (var b = new LinearGradientBrush(r, Color.FromArgb(206, 211, 219), Color.FromArgb(150, 156, 165), 90f))
                        g.FillRectangle(b, 0, 0, 2, r.Height);
                    HDrawPaint.FillRoundedBorder(g, r.Width, r.Height, ShapeRadius(r, s, radiusOverride), 2f, Color.FromArgb(120, 126, 134));
                    break;
                case HEditStyle.Transparent:
                    break;
            }
        }
        private static Color FocusedOr(bool focused, bool hover, Color acc, Color normal)
            => focused ? acc : hover ? Color.FromArgb(150, acc) : normal;
        public static void DrawButton(Graphics g, RectangleF r, HEditStyle s, int state, bool enabled,
            int idx, HSpinLayout layout)
        {
            if (r.Width <= 1f || r.Height <= 1f) return;
            if (s == HEditStyle.Transparent && state == 0) return;
            Color acc = Accent(s);
            if (s == HEditStyle.Stepper)
            {
                float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;
                float rad = Math.Min(r.Width, r.Height) * 0.42f;
                Color c = !enabled ? Color.FromArgb(180, 184, 190)
                    : state == 2 ? Dark(acc, 0.25f) : state == 1 ? Light(acc, 0.15f) : acc;
                var box = new RectangleF(cx - rad, cy - rad, rad * 2f, rad * 2f);
                using (var b = new SolidBrush(c))
                    g.FillEllipse(b, box);
                return;
            }
            if (s == HEditStyle.Lcd)
            {
                var box = RectangleF.Inflate(r, -1.5f, -1.5f);
                Color c = state == 2 ? Color.FromArgb(140, 146, 154)
                    : state == 1 ? Color.FromArgb(205, 210, 218) : Color.FromArgb(170, 176, 184);
                using (var b = new LinearGradientBrush(box, Light(c, 0.18f), Dark(c, 0.12f), 90f))
                using (var path = Round(box, 4f))
                    g.FillPath(b, path);
                return;
            }
            bool chip = s == HEditStyle.Segmented;
            bool dark = DarkPanel(s);
            int alpha = state == 2 ? 80 : state == 1 ? 42 : 0;
            if (s == HEditStyle.Neon && state > 0) alpha += 20;
            if (alpha > 0)
            {
                Color fill = dark ? Color.FromArgb(alpha, acc) : Color.FromArgb(alpha, acc);
                float rr = chip || s == HEditStyle.Industrial ? 5f : 3f;
                if (layout == HSpinLayout.EndVertical || layout == HSpinLayout.FrontVertical)
                    rr = 4f;
                using (var b = new SolidBrush(fill))
                using (var path = Round(RectangleF.Inflate(r, -1f, -1f), rr))
                    g.FillPath(b, path);
            }
            else if (chip)
            {
                using (var b = new SolidBrush(Color.FromArgb(241, 243, 246)))
                using (var path = Round(RectangleF.Inflate(r, -1.5f, -2f), 5f))
                    g.FillPath(b, path);
                using (var p = new Pen(Color.FromArgb(212, 216, 222)))
                using (var path = Round(RectangleF.Inflate(r, -1.5f, -2f), 5f))
                    g.DrawPath(p, path);
            }
            // 上下叠放时按钮分隔线
            if ((layout == HSpinLayout.EndVertical || layout == HSpinLayout.FrontVertical)
                && s != HEditStyle.Segmented && s != HEditStyle.Stepper)
            {
                Color sep = dark ? Color.FromArgb(70, 76, 84) : Color.FromArgb(222, 225, 230);
                using (var p = new Pen(sep))
                {
                    if (idx == 0) g.DrawLine(p, r.X + 3f, r.Bottom, r.Right - 3f, r.Bottom);
                }
            }
        }
        private static GraphicsPath Round(RectangleF r, float rad)
        {
            var path = new GraphicsPath();
            rad = Math.Max(0, Math.Min(rad, Math.Min(r.Width, r.Height) / 2f - 0.5f));
            if (rad <= 0.5f) { path.AddRectangle(r); return path; }
            float d = rad * 2f;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
        private static void DrawGlowFrame(Graphics g, RectangleF rf, Color c, int alpha, float w, float rad)
        {
            using (var path = Round(rf, rad))
            using (var p = new Pen(Color.FromArgb(alpha, c), w))
                g.DrawPath(p, path);
        }
        private static Color Mix(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
        private static Color Light(Color c, double t) => Mix(c, Color.White, t);
        private static Color Dark(Color c, double t) => Mix(c, Color.Black, t);
    }
}