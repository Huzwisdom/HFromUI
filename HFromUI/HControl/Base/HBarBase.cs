using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{

    /// <summary>
    /// 图元文字方位：上/下方位文字横排，左/右方位文字竖排（逐字正立），
    /// Middle 为文字居中覆盖在图元中央。图元本体始终占据控件大部分区域。
    /// </summary>
    public enum HTextPlacement
    {
        /// <summary>下方（文字横排）。</summary>
        Bottom = 0,
        /// <summary>上方（文字横排）。</summary>
        Top = 1,
        /// <summary>左侧（文字竖排）。</summary>
        Left = 2,
        /// <summary>右侧（文字竖排）。</summary>
        Right = 3,
        /// <summary>居中覆盖在图元中央。</summary>
        Middle = 4
    }

    /// <summary>
    /// bar 基类：继承 <see cref="HLabelBase"/>，参考 HTrackBar 的值体系（最小/最大/当前值、
    /// 轨道色/值色、值提示），但作为纯展示型圆角进度条基类（不处理拖动），
    /// 供瓶/电池等带液位/电量语义的工具控件继承。Radius&lt;=0 时轨道默认半高全圆角。
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [DefaultProperty("Value")]
    public class HBarBase : HLabelBase
    {
        private double _minValue;
        private double _maxValue = 100.0;
        private double _value = 50.0;
        private HBarOrientation _orientation = HBarOrientation.Horizontal;
        private HTextPlacement _textPlacement = HTextPlacement.Bottom;
        private Color _trackColor = Color.FromArgb(230, 233, 238);
        private Color _barColor = Color.DodgerBlue;
        private bool _showValueText = true;
        private string _valueFormat = "{0:0}%";

        /// <summary>值改变事件。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("当前值改变时触发"), HDescriptionLanguage("当前值改变时触发"), Browsable(true)]
        public event EventHandler ValueChanged;

        /// <summary>触发 ValueChanged 事件（派生类使用 int/long 等自有值体系时由此统一触发，保证事件只有一份）。</summary>
        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        /// <summary>
        /// 派生类静默同步基类值域与当前值（不触发 ValueChanged），使 Percent/基类 Value 与派生值保持一致。
        /// 用于自带 int/long 值体系的进度条/滚动条控件，避免基类赋值引发事件重入。
        /// </summary>
        protected void SyncBaseState(double min, double max, double value)
        {
            _minValue = min;
            _maxValue = Math.Max(min, max);
            _value = Math.Max(_minValue, Math.Min(_maxValue, value));
        }

        public HBarBase()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(250, 24);
        }

        /// <summary>最小值。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("最小值"), HDescriptionLanguage("最小值"), Browsable(true)]
        [DefaultValue(0.0)]
        public double MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                if (_maxValue < _minValue) _maxValue = _minValue;
                ClampValue();
                Invalidate();
            }
        }

        /// <summary>最大值。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("最大值"), HDescriptionLanguage("最大值"), Browsable(true)]
        [DefaultValue(100.0)]
        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                if (_maxValue < _minValue) _minValue = _maxValue;
                ClampValue();
                Invalidate();
            }
        }

        /// <summary>当前值（超出范围自动夹取）。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("当前值"), HDescriptionLanguage("当前值，超出最小/最大范围自动夹取"), Browsable(true)]
        [DefaultValue(50.0)]
        public virtual double Value
        {
            get => _value;
            set
            {
                double v = Math.Max(_minValue, Math.Min(_maxValue, value));
                if (Math.Abs(v - _value) < double.Epsilon) return;
                _value = v;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>bar 方向。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("方向"), HDescriptionLanguage("bar 方向：横向值从左到右，纵向值从下到上"), Browsable(true)]
        [DefaultValue(HBarOrientation.Horizontal)]
        public HBarOrientation Orientation
        {
            get => _orientation;
            set { _orientation = value; Invalidate(); }
        }

        /// <summary>轨道背景色。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("轨道背景色"), HDescriptionLanguage("轨道背景色"), Browsable(true)]
        [DefaultValue(typeof(Color), "230, 233, 238")]
        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; Invalidate(); }
        }

        /// <summary>值填充色。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("值填充色"), HDescriptionLanguage("值填充色"), Browsable(true)]
        [DefaultValue(typeof(Color), "DodgerBlue")]
        public Color BarColor
        {
            get => _barColor;
            set { _barColor = value; Invalidate(); }
        }

        /// <summary>是否在条上绘制数值文本。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("显示值文本"), HDescriptionLanguage("是否在条上绘制数值文本"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowValueText
        {
            get => _showValueText;
            set { _showValueText = value; Invalidate(); }
        }

        /// <summary>数值文本格式（一个 {0} 占位，接收 0..100 百分比）。</summary>
        [HCategoryLanguage("图形基础设置"), HDisplayNameLanguage("数值文本格式"), HDescriptionLanguage("数值文本格式，{0} 为百分比"), Browsable(true)]
        [DefaultValue("{0:0}%")]
        public string ValueFormat
        {
            get => _valueFormat;
            set { _valueFormat = string.IsNullOrEmpty(value) ? "{0:0}%" : value; Invalidate(); }
        }

        /// <summary>当前值百分比（0..1）。</summary>
        [Browsable(false)]
        public float Percent
        {
            get
            {
                double span = _maxValue - _minValue;
                if (span <= 0) return 0f;
                return (float)Math.Max(0.0, Math.Min(1.0, (_value - _minValue) / span));
            }
        }

        /// <summary>文字方位：上/下横排、左/右竖排（逐字正立）、Middle 居中覆盖，图元始终占主体。</summary>
        [HCategoryLanguage("工具"), HDisplayNameLanguage("文字方位"), HDescriptionLanguage("文字方位：上/下横排，左/右竖排，居中覆盖在图元中央"), Browsable(true)]
        [DefaultValue(HTextPlacement.Bottom)]
        public HTextPlacement TextPlacement
        {
            get => _textPlacement;
            set { _textPlacement = value; Invalidate(); }
        }

        /// <summary>按当前文字方位计算图元绘制区（无文字或居中覆盖时占满整个控件）。</summary>
        protected RectangleF PlacedGlyphArea()
            => PlacedGlyphArea(Width, Height, Text, Font, _textPlacement);

        /// <summary>按当前文字方位绘制文字（无文字不绘制）。</summary>
        protected void PaintPlacedText(Graphics g)
            => PaintPlacedText(g, Width, Height, Text, Font, ForeColor, _textPlacement);

        private void ClampValue()
        {
            double v = Math.Max(_minValue, Math.Min(_maxValue, _value));
            if (Math.Abs(v - _value) >= double.Epsilon)
            {
                _value = v;
                OnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>格式化百分比文本。</summary>
        protected string FormatPercent()
        {
            try { return string.Format(_valueFormat, Percent * 100f); }
            catch { return ((int)(Percent * 100)).ToString(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            DrawDefaultBar(g);
        }

        /// <summary>
        /// 默认圆角进度条外观；派生控件整体重绘时不调用即可。圆角/边框规则全局统一：
        /// 外轮廓固定在像素盒边缘 (-0.5,-0.5)-(W-0.5,H-0.5)，边框厚度全部向内（填充环），
        /// Radius&lt;=0 时按控件高全圆角（胶囊），BorderStyle/BorderWidth 决定是否描边及边宽。
        /// </summary>
        protected void DrawDefaultBar(Graphics g)
        {
            float w = Width, h = Height;
            // 传入 CreateOuterBoxPath 的是弧外接框边长（直径）；胶囊取控件高（r+0.5 后弧外接边=H）
            float r = Radius > 0 ? Radius : Math.Min(w, h) - 0.5f;

            // 描边判定与 HLabelBase.DrawBorder 一致：FixedSingle 描边；圆角 Fixed3D 描边，直角 Fixed3D 原生立体边
            bool frame = BorderStyle == BorderStyle.FixedSingle
                || (BorderStyle == BorderStyle.Fixed3D && Radius > 0);
            int inset = BorderStyle == BorderStyle.None ? 0 : (frame ? Math.Max(1, BorderWidth) : 2);

            using (GraphicsPath track = HDrawPaint.CreateOuterBoxPath(w, h, r))
            using (var tb = new SolidBrush(_trackColor))
                g.FillPath(tb, track);

            float p = Percent;
            if (p > 0f)
            {
                // 填充收在边框内侧（像素盒 bw-0.5 起），圆角弧随内缩同步减小
                float bw = inset;
                var content = new RectangleF(bw - 0.5f, bw - 0.5f, w - bw * 2f, h - bw * 2f);
                float cr = Math.Max(0f, Math.Min(r + 0.5f - bw, Math.Min(content.Width, content.Height)));

                RectangleF fr;
                HRoundStyle fs;
                if (_orientation == HBarOrientation.Horizontal)
                {
                    bool full = content.Width * p >= content.Width - 0.5f;
                    fr = new RectangleF(content.X, content.Y, content.Width * p, content.Height);
                    fs = full ? HRoundStyle.All : HRoundStyle.Left;
                    if (!full) cr = Math.Min(cr, fr.Width);
                }
                else
                {
                    bool full = content.Height * p >= content.Height - 0.5f;
                    fr = new RectangleF(content.X, content.Bottom - content.Height * p,
                        content.Width, content.Height * p);
                    fs = full ? HRoundStyle.All : HRoundStyle.Bottom;
                    if (!full) cr = Math.Min(cr, fr.Height);
                }
                using (GraphicsPath fill = HDrawPaint.CreatePath(fr, cr, fs, false))
                using (var bb = new SolidBrush(_barColor))
                    g.FillPath(bb, fill);
            }

            if (_showValueText)
            {
                string s = FormatPercent();
                var textRect = new Rectangle(inset, inset, Width - inset * 2, Height - inset * 2);
                TextRenderer.DrawText(g, s, Font, textRect, ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }

            if (frame)
            {
                // 边框环：厚度 inset 全部在外轮廓之内，改边宽外沿不动、四边等宽
                HDrawPaint.FillRoundedBorder(g, w, h, r, inset, SystemColors.ControlDark);
            }
            else if (BorderStyle == BorderStyle.Fixed3D)
            {
                ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, Width, Height), Border3DStyle.Sunken);
            }
        }

        // ------------------------------------------------------------------
        // 工具控件共享绘制工具（HControl/Tools 内部使用）
        // ------------------------------------------------------------------

        /// <summary>构造圆角矩形 path（四角同半径）。矩形退化（宽/高≤0）时返回空 path，绝不抛异常。</summary>
        internal static GraphicsPath RoundPath(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            if (r.Width <= 0f || r.Height <= 0f) return path;
            if (radius <= 0f)
            {
                path.AddRectangle(r);
                return path;
            }
            radius = Math.Min(radius, Math.Min(r.Width, r.Height) / 2f);
            float d = radius * 2f;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>水平方向 边-中-边 渐变画刷（模拟圆柱/玻璃罐质感），调用方释放。</summary>
        internal static LinearGradientBrush CylinderH(RectangleF r, Color edge, Color center)
        {
            // 退化矩形时两点重合会抛异常，钳出至少 1px 的有效宽度
            float x1 = r.Left, x2 = r.Right;
            if (x2 - x1 < 1f) x2 = x1 + 1f;
            var b = new LinearGradientBrush(new PointF(x1, 0), new PointF(x2, 0), edge, edge);
            var cb = new ColorBlend { Positions = new[] { 0f, 0.5f, 1f }, Colors = new[] { edge, center, edge } };
            b.InterpolationColors = cb;
            return b;
        }

        /// <summary>垂直方向 顶亮-中-底暗 渐变画刷，调用方释放。</summary>
        internal static LinearGradientBrush CylinderV(RectangleF r, Color top, Color middle, Color bottom)
        {
            float y1 = r.Top, y2 = r.Bottom;
            if (y2 - y1 < 1f) y2 = y1 + 1f;
            var b = new LinearGradientBrush(new PointF(0, y1), new PointF(0, y2), top, bottom);
            var cb = new ColorBlend { Positions = new[] { 0f, 0.5f, 1f }, Colors = new[] { top, middle, bottom } };
            b.InterpolationColors = cb;
            return b;
        }

        /// <summary>任意角度线性渐变画刷：退化矩形（宽/高≤0）自动钳到 1px，绝不抛异常，调用方释放。</summary>
        internal static LinearGradientBrush Gradient(RectangleF r, Color c1, Color c2, float angle)
        {
            if (r.Width <= 0f) r = new RectangleF(r.X, r.Y, 1f, r.Height);
            if (r.Height <= 0f) r = new RectangleF(r.X, r.Y, r.Width, 1f);
            return new LinearGradientBrush(r, c1, c2, angle);
        }

        /// <summary>九宫格文本（默认居中）。</summary>
        internal static void DrawText(Graphics g, string text, Font font, Color color, RectangleF rect,
            ContentAlignment align = ContentAlignment.MiddleCenter)
        {
            if (string.IsNullOrEmpty(text)) return;
            var flags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
            string a = align.ToString();
            if (a.EndsWith("Left")) flags |= TextFormatFlags.Left;
            else if (a.EndsWith("Right")) flags |= TextFormatFlags.Right;
            else flags |= TextFormatFlags.HorizontalCenter;
            if (a.StartsWith("Top")) flags |= TextFormatFlags.Top;
            else if (a.StartsWith("Bottom")) flags |= TextFormatFlags.Bottom;
            else flags |= TextFormatFlags.VerticalCenter;
            TextRenderer.DrawText(g, text, font, Rectangle.Round(rect), color, flags);
        }

        // ------------------------------------------------------------------
        // 图元文字方位（上/下/左/右/中）统一布局，保证文字任何方位都完整可见
        // ------------------------------------------------------------------
        private const TextFormatFlags TfFit =
            TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;
        private const TextFormatFlags TfCenter =
            TfFit | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;

        /// <summary>横排文字带高度 / 竖排文字带宽度：随字号缩放，且不超过控件对应边长的一半。</summary>
        internal static float TextBandSize(int span, Font font)
            => Math.Min(Math.Max(font.Height + 6f, 16f), span * 0.5f);

        /// <summary>按文字方位计算图元绘制区（无文字或居中覆盖时占满整个控件）。</summary>
        internal static RectangleF PlacedGlyphArea(int w, int h, string text, Font font, HTextPlacement p)
        {
            if (string.IsNullOrEmpty(text) || p == HTextPlacement.Middle)
                return new RectangleF(0f, 0f, w, h);
            switch (p)
            {
                case HTextPlacement.Top:
                {
                    float band = TextBandSize(h, font);
                    return new RectangleF(0f, band, w, Math.Max(1f, h - band));
                }
                case HTextPlacement.Left:
                {
                    float band = TextBandSize(w, font);
                    return new RectangleF(band, 0f, Math.Max(1f, w - band), h);
                }
                case HTextPlacement.Right:
                {
                    float band = TextBandSize(w, font);
                    return new RectangleF(0f, 0f, Math.Max(1f, w - band), h);
                }
                default:
                {
                    float band = TextBandSize(h, font);
                    return new RectangleF(0f, 0f, w, Math.Max(1f, h - band));
                }
            }
        }

        /// <summary>按方位绘制图元文字：上/下横排、左/右逐字竖排、Middle 居中半透明铭牌覆盖。</summary>
        internal static void PaintPlacedText(Graphics g, int w, int h, string text, Font font,
            Color color, HTextPlacement p)
        {
            if (string.IsNullOrEmpty(text)) return;
            switch (p)
            {
                case HTextPlacement.Top:
                case HTextPlacement.Bottom:
                {
                    float band = TextBandSize(h, font);
                    float y = p == HTextPlacement.Top ? 0f : h - band;
                    DrawHorizontal(g, text, font, color, new RectangleF(0f, y, w, band));
                    break;
                }
                case HTextPlacement.Left:
                case HTextPlacement.Right:
                {
                    float band = TextBandSize(w, font);
                    float x = p == HTextPlacement.Left ? 0f : w - band;
                    DrawVertical(g, text, font, color, new RectangleF(x, 0f, band, h));
                    break;
                }
                default:
                    DrawMiddle(g, text, font, w, h);
                    break;
            }
        }

        /// <summary>横排：自动缩字号保证整条文字完整放入文字带。</summary>
        private static void DrawHorizontal(Graphics g, string text, Font font, Color color, RectangleF band)
        {
            Font f = FitFont(text, font, band.Width - 4f, band.Height - 2f, out bool owned);
            try { TextRenderer.DrawText(g, text, f, Rectangle.Round(band), color, TfCenter); }
            finally { if (owned) f.Dispose(); }
        }

        /// <summary>竖排：逐字正立排列，自动缩字号/行距保证所有字完整放入文字带。</summary>
        private static void DrawVertical(Graphics g, string text, Font font, Color color, RectangleF band)
        {
            int n = text.Length;
            float step = (band.Height - 2f) / n;
            float maxCharW = band.Width - 2f;
            Font f = FitStackedFont(text, font, step, maxCharW);
            try
            {
                for (int i = 0; i < n; i++)
                {
                    var cell = new RectangleF(band.X, band.Y + 1f + i * step, band.Width, step);
                    TextRenderer.DrawText(g, text[i].ToString(), f, Rectangle.Round(cell), color, TfCenter);
                }
            }
            finally { if (f != font) f.Dispose(); }
        }

        /// <summary>居中：半透明深色圆角铭牌 + 白字，随文字尺寸自适应，不遮挡图元主体。</summary>
        private static void DrawMiddle(Graphics g, string text, Font font, int w, int h)
        {
            Font f = FitFont(text, font, w * 0.9f, h * 0.6f, out bool owned);
            try
            {
                Size ts = TextRenderer.MeasureText(g, text, f, Size.Empty, TfFit);
                float pw = Math.Min(ts.Width + 12f, w - 2f);
                float ph = Math.Min(ts.Height + 5f, h - 2f);
                var plate = new RectangleF((w - pw) / 2f, (h - ph) / 2f, pw, ph);
                using (var gp = RoundPath(plate, 4f))
                using (var b = new SolidBrush(Color.FromArgb(150, 22, 24, 28)))
                    g.FillPath(b, gp);
                TextRenderer.DrawText(g, text, f, Rectangle.Round(plate), Color.White, TfCenter);
            }
            finally { if (owned) f.Dispose(); }
        }

        /// <summary>缩小字号直到整段文本满足宽高限制；无需缩小时返回原字体（owned=false，不可释放）。</summary>
        private static Font FitFont(string text, Font baseFont, float maxW, float maxH, out bool owned)
        {
            Size s = TextRenderer.MeasureText(text, baseFont, Size.Empty, TfFit);
            if (s.Width <= maxW && s.Height <= maxH) { owned = false; return baseFont; }
            float em = baseFont.Size;
            while (em > 5.5f)
            {
                em -= 0.5f;
                var f = new Font(baseFont.FontFamily, em, baseFont.Style);
                Size s2 = TextRenderer.MeasureText(text, f, Size.Empty, TfFit);
                if (s2.Width <= maxW && s2.Height <= maxH) { owned = true; return f; }
                f.Dispose();
            }
            owned = true;
            return new Font(baseFont.FontFamily, 5.5f, baseFont.Style);
        }

        /// <summary>竖排字号：同时满足行距高度与最宽单字宽度。</summary>
        private static Font FitStackedFont(string text, Font baseFont, float step, float maxCharW)
        {
            bool fits(Font f)
            {
                if (f.Height > step + 1f) return false;
                foreach (char c in text)
                    if (TextRenderer.MeasureText(c.ToString(), f, Size.Empty, TfFit).Width > maxCharW)
                        return false;
                return true;
            }
            if (fits(baseFont)) return baseFont;
            float em = baseFont.Size;
            while (em > 5.5f)
            {
                em -= 0.5f;
                var f = new Font(baseFont.FontFamily, em, baseFont.Style);
                if (fits(f)) return f;
                f.Dispose();
            }
            return new Font(baseFont.FontFamily, 5.5f, baseFont.Style);
        }

        /// <summary>电量/液位状态色：高→低 绿/青绿/橙/番茄/红（与旧 HBattery 五区一致）。</summary>
        internal static Color StatusColor(float percent,
            Color c1, Color c2, Color c3, Color c4, Color c5,
            float s1, float s2, float s3, float s4)
        {
            if (percent > s1) return c1;
            if (percent > s2) return c2;
            if (percent > s3) return c3;
            if (percent > s4) return c4;
            return c5;
        }
    }

    /// <summary>bar 方向：横向（值从左到右）/ 纵向（值从下到上）。</summary>
    public enum HBarOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    /// <summary>
    /// 工具控件统一调色板：主色 + 亮/暗派生色 + 玻璃色 + 金属色 + 强调色。
    /// 各工具控件的“经典”色调用自身字段（与旧版逐色一致），其余色调取 <see cref="HToolPalettes"/>。
    /// </summary>
    public struct HToolPalette
    {
        /// <summary>主色（液体/填充/主体）。</summary>
        public Color Main;
        /// <summary>主色提亮（高光面）。</summary>
        public Color Light;
        /// <summary>主色再提亮（玻璃/顶面）。</summary>
        public Color Lighter;
        /// <summary>主色加深（暗面）。</summary>
        public Color Dark;
        /// <summary>边缘描边色。</summary>
        public Color Edge;
        /// <summary>玻璃/罐体亮色。</summary>
        public Color Glass;
        /// <summary>玻璃/罐体边缘色。</summary>
        public Color GlassEdge;
        /// <summary>金属亮面。</summary>
        public Color Metal;
        /// <summary>金属暗面。</summary>
        public Color MetalDark;
        /// <summary>强调色（指示灯/对撞装饰）。</summary>
        public Color Accent;
    }

    /// <summary>
    /// 12 种现代色调（各工具控件枚举值 1..12 对应）。颜色由主色派生亮/暗面，
    /// 保证瓶/电池/阀门/厂房/电机在同一色调下明暗体系一致。
    /// </summary>
    public static class HToolPalettes
    {
        private struct PRaw
        {
            public int Main;
            public int Accent;
            public PRaw(int main, int accent) { Main = main; Accent = accent; }
        }

        // 顺序对应 HToolTheme 1..12：SkyBlue、SeaBlue、Emerald、Teal、Amber、Orange、Rose、Red、Purple、Magenta、Coffee、Graphite
        private static readonly PRaw[] Raws =
        {
            new PRaw(unchecked((int)0xFF449FE2), unchecked((int)0xFFF08C28)),
            new PRaw(unchecked((int)0xFF1C6CAE), unchecked((int)0xFFF0B428)),
            new PRaw(unchecked((int)0xFF3CAE67), unchecked((int)0xFFF08C28)),
            new PRaw(unchecked((int)0xFF009688), unchecked((int)0xFFFF7043)),
            new PRaw(unchecked((int)0xFFF0B428), unchecked((int)0xFF1C6CAE)),
            new PRaw(unchecked((int)0xFFEE8428), unchecked((int)0xFF1C6CAE)),
            new PRaw(unchecked((int)0xFFE04C5C), unchecked((int)0xFF009688)),
            new PRaw(unchecked((int)0xFFD22D2D), unchecked((int)0xFFF0B428)),
            new PRaw(unchecked((int)0xFF765ABE), unchecked((int)0xFFF0B428)),
            new PRaw(unchecked((int)0xFFE064AA), unchecked((int)0xFF3949AB)),
            new PRaw(unchecked((int)0xFF96643C), unchecked((int)0xFFF0B428)),
            new PRaw(unchecked((int)0xFF464C54), unchecked((int)0xFF26C6DA))
        };

        /// <summary>按 1 基色调序号（1..12）取调色板，越界夹取。</summary>
        public static HToolPalette Get(int themeIndex)
        {
            int i = Math.Max(0, Math.Min(Raws.Length - 1, themeIndex - 1));
            Color m = Color.FromArgb(Raws[i].Main);
            Color a = Color.FromArgb(Raws[i].Accent);
            return new HToolPalette
            {
                Main = m,
                Light = Mix(m, Color.White, 0.55f),
                Lighter = Mix(m, Color.White, 0.86f),
                Dark = Mix(m, Color.Black, 0.32f),
                Edge = Mix(m, Color.Black, 0.50f),
                Glass = Mix(m, Color.White, 0.90f),
                GlassEdge = Mix(m, Color.White, 0.62f),
                Metal = Color.FromArgb(228, 230, 233),
                MetalDark = Color.FromArgb(148, 152, 158),
                Accent = a
            };
        }

        /// <summary>两色按 t（0=base，1=other）线性混合。</summary>
        public static Color Mix(Color baseColor, Color other, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int r = (int)(baseColor.R + (other.R - baseColor.R) * t);
            int g = (int)(baseColor.G + (other.G - baseColor.G) * t);
            int b = (int)(baseColor.B + (other.B - baseColor.B) * t);
            return Color.FromArgb(r, g, b);
        }

        /// <summary>与白色混合（提亮）。</summary>
        public static Color Tint(Color c, float amt) => Mix(c, Color.White, amt);

        /// <summary>与黑色混合（加深）。</summary>
        public static Color Shade(Color c, float amt) => Mix(c, Color.Black, amt);
    }

}