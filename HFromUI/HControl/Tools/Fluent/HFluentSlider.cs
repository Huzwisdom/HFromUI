using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 单值滑块：灰色细轨 + 强调色已选段 + 等分刻度缺口 + 圆环滑块
    /// （滑块中心深色、强调色外环），可选滑块上方数值气泡。支持鼠标拖动与方向键。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    public class HFluentSlider : HFluentBase
    {
        private const int ThumbR = 10;
        private const int BubbleH = 22;

        private double _minValue, _maxValue = 100.0, _value = 60.0;
        private int _tickCount = 5;
        private bool _showBubble;
        private Color _accentColor = HFluentPalette.Accent;
        private Color _tickColor = Color.White;
        private bool _dark;
        private bool _dragging;

        /// <summary>值改变。</summary>
        public event EventHandler ValueChanged;

        /// <summary>初始化。</summary>
        public HFluentSlider()
        {
            Size = new Size(240, 40);
            MinimumSize = new Size(100, 34);
        }

        /// <summary>最小值。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("最小值"), HDescriptionLanguage("最小值"), Browsable(true)]
        public double MinValue
        {
            get => _minValue;
            set { _minValue = value; Clamp(); Invalidate(); }
        }

        /// <summary>最大值。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("最大值"), HDescriptionLanguage("最大值"), Browsable(true)]
        [DefaultValue(100.0)]
        public double MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; Clamp(); Invalidate(); }
        }

        /// <summary>当前值。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("当前值"), HDescriptionLanguage("滑块当前值"), Browsable(true)]
        public double Value
        {
            get => _value;
            set
            {
                value = Math.Round(Math.Max(_minValue, Math.Min(_maxValue, value)) * 100.0) / 100.0;
                if (Math.Abs(value - _value) < double.Epsilon) return;
                _value = value;
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>刻度数（含两端，≤1 不显示刻度）。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("刻度数"), HDescriptionLanguage("刻度分段数（含两端）"), Browsable(true)]
        [DefaultValue(5)]
        public int TickCount
        {
            get => _tickCount;
            set { _tickCount = value; Invalidate(); }
        }

        /// <summary>是否显示滑块上方数值气泡。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("数值气泡"), HDescriptionLanguage("拖动/悬停时滑块上方显示数值气泡（常驻）"), Browsable(true)]
        [DefaultValue(false)]
        public bool ShowBubble
        {
            get => _showBubble;
            set { _showBubble = value; Invalidate(); }
        }

        /// <summary>强调色。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("已选段与滑块颜色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        /// <summary>是否暗色主题（轨道深灰、刻度浅灰）。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("暗色主题"), HDescriptionLanguage("暗色面板上使用深灰轨道"), Browsable(true)]
        [DefaultValue(false)]
        public bool Dark
        {
            get => _dark;
            set { _dark = value; Invalidate(); }
        }

        /// <summary>刻度缺口颜色（默认白，使灰轨上呈缺口感）。</summary>
        [HCategoryLanguage("Fluent 滑块"), HDisplayNameLanguage("刻度颜色"), HDescriptionLanguage("轨道刻度缺口颜色"), Browsable(true)]
        public Color TickColor
        {
            get => _tickColor;
            set { _tickColor = value; Invalidate(); }
        }

        private void Clamp()
        {
            _value = Math.Max(_minValue, Math.Min(_maxValue, _value));
        }

        private float PadX => ThumbR + 1f;
        private float TrackY => _showBubble ? BubbleH + ThumbR + 2 : Height / 2f;

        private float ValueToX(double v)
        {
            double span = _maxValue - _minValue;
            float t = span <= 0 ? 0 : (float)((v - _minValue) / span);
            return PadX + t * (Width - PadX * 2);
        }

        private double XToValue(float x)
        {
            float t = (x - PadX) / (Width - PadX * 2);
            t = Math.Max(0f, Math.Min(1f, t));
            return _minValue + t * (_maxValue - _minValue);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Focus();
            _dragging = true;
            Value = XToValue(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging) Value = XToValue(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            double step = (_maxValue - _minValue) / 20.0;
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down) { Value = _value - step; e.Handled = true; }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up) { Value = _value + step; e.Handled = true; }
        }

        /// <summary>自绘轨道、刻度、已选段、滑块。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float y = TrackY;
            float x0 = PadX, x1 = Width - PadX;
            float xv = ValueToX(_value);

            Color trackColor = _dark ? Color.FromArgb(64, 64, 64) : HFluentPalette.Track;
            using (var bb = new SolidBrush(trackColor))
                FillCapsule(g, bb, x0, x1, y);
            using (var ab = new SolidBrush(_accentColor))
                FillCapsule(g, ab, x0, xv, y);

            // 刻度缺口（短竖线在轨道上层，露出底色）
            if (_tickCount > 1)
            {
                Color tick = _dark ? Color.FromArgb(96, 96, 96) : _tickColor;
                using (var tickPen = new Pen(tick, 1.4f))
                    for (int i = 0; i < _tickCount; i++)
                    {
                        float t = i / (float)(_tickCount - 1);
                        float tx = x0 + t * (x1 - x0);
                        g.DrawLine(tickPen, tx, y - 4, tx, y + 4);
                    }
            }

            // 滑块：白/浅底圆 + 强调色外环 + 深色中心点
            using (var kb = new SolidBrush(Color.White))
                g.FillEllipse(kb, xv - ThumbR, y - ThumbR, ThumbR * 2, ThumbR * 2);
            using (var pen = new Pen(_accentColor, 3f))
                g.DrawEllipse(pen, xv - ThumbR, y - ThumbR, ThumbR * 2, ThumbR * 2);
            using (var dot = new SolidBrush(Color.FromArgb(55, 65, 81)))
                g.FillEllipse(dot, xv - 3, y - 3, 6, 6);

            if (_showBubble)
            {
                string text = Math.Round(_value).ToString("0");
                Size ts = TextRenderer.MeasureText(g, text, Font, new Size(200, BubbleH),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                int bw = ts.Width + 16;
                int bx = (int)Math.Max(2, Math.Min(Width - bw - 2, xv - bw / 2f));
                using (var path = Rounded(bx, 0, bw, BubbleH, 7))
                using (var bub = new SolidBrush(HFluentPalette.Bubble))
                    g.FillPath(bub, path);
                TextRenderer.DrawText(g, text, Font, new Rectangle(bx, 0, bw, BubbleH), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        private static void FillCapsule(Graphics g, Brush brush, float x0, float x1, float cy)
        {
            if (x1 - x0 < 1f) return;
            const float h = 5f;
            using (var p = new GraphicsPath())
            {
                p.AddArc(x0, cy - h / 2f, h, h, 90, 180);
                p.AddArc(x1 - h, cy - h / 2f, h, h, 270, 180);
                p.CloseFigure();
                g.FillPath(brush, p);
            }
        }

        private static GraphicsPath Rounded(float x, float y, float w, float h, float r)
        {
            var p = new GraphicsPath();
            float d = r * 2f;
            p.AddArc(x, y, d, d, 180, 90);
            p.AddArc(x + w - d, y, d, d, 270, 90);
            p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            p.AddArc(x, y + h - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
