using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// 双端范围滑块：灰底轨道 + 蓝色选中区间 + 两个白色圆环滑块，
    /// 滑块正上方为黑底圆角数值气泡，轨道两端显示最小/最大值与单位。
    /// 鼠标按下/拖动选择最近的滑块，键盘左右方向微调聚焦滑块。
    /// </summary>
    [DefaultProperty("HighValue")]
    [DefaultEvent("RangeChanged")]
    public class HRangeSlider : HFluentBase
    {
        private const int BubbleH = 26;
        private const int ThumbR = 11;
        private const int Gap = 6;

        private double _minValue, _maxValue = 100.0;
        private double _lowValue = 25.0, _highValue = 75.0;
        private string _unitText = string.Empty;
        private string _valueFormat = "0";
        private bool _showBubbles = true;
        private bool _showEndLabels = true;
        private Color _accentColor = HFluentPalette.Blue;
        private int _drag = -1;
        private int _focusThumb;

        /// <summary>低端值变化。</summary>
        public event EventHandler LowValueChanged;
        /// <summary>高端值变化。</summary>
        public event EventHandler HighValueChanged;
        /// <summary>任一端值变化。</summary>
        public event EventHandler RangeChanged;

        /// <summary>初始化尺寸。</summary>
        public HRangeSlider()
        {
            Size = new Size(360, 66);
            MinimumSize = new Size(120, 60);
        }

        /// <summary>最小值。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("最小值"), HDescriptionLanguage("范围下限"), Browsable(true)]
        public double MinValue
        {
            get => _minValue;
            set { _minValue = value; ClampValues(); Invalidate(); }
        }

        /// <summary>最大值。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("最大值"), HDescriptionLanguage("范围上限"), Browsable(true)]
        public double MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; ClampValues(); Invalidate(); }
        }

        /// <summary>低端当前值。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("低端值"), HDescriptionLanguage("低端滑块当前值"), Browsable(true)]
        public double LowValue
        {
            get => _lowValue;
            set { SetLow(value); }
        }

        /// <summary>高端当前值。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("高端值"), HDescriptionLanguage("高端滑块当前值"), Browsable(true)]
        public double HighValue
        {
            get => _highValue;
            set { SetHigh(value); }
        }

        /// <summary>单位文本（如 C、kPa、%），拼在数值后。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("单位文本"), HDescriptionLanguage("单位文本，显示在数值与端值后"), Browsable(true)]
        [DefaultValue("")]
        public string UnitText
        {
            get => _unitText;
            set { _unitText = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>数值格式（默认整数 0，0.0 保留一位小数）。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("数值格式"), HDescriptionLanguage("数值格式串，如 0 或 0.0"), Browsable(true)]
        [DefaultValue("0")]
        public string ValueFormat
        {
            get => _valueFormat;
            set { _valueFormat = string.IsNullOrEmpty(value) ? "0" : value; Invalidate(); }
        }

        /// <summary>是否显示滑块上方数值气泡。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("显示数值气泡"), HDescriptionLanguage("滑块上方黑底圆角数值气泡"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowBubbles
        {
            get => _showBubbles;
            set { _showBubbles = value; Invalidate(); }
        }

        /// <summary>是否显示轨道两端最小/最大值。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("显示端标签"), HDescriptionLanguage("轨道两端显示最小/最大值与单位"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowEndLabels
        {
            get => _showEndLabels;
            set { _showEndLabels = value; Invalidate(); }
        }

        /// <summary>强调色（选中区间与滑块描边）。</summary>
        [HCategoryLanguage("范围滑块"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("选中区间与滑块圆环颜色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        private float TrackY => _showBubbles ? BubbleH + ThumbR + 4 : Height / 2f;
        private float PadX => ThumbR + 2f;

        private string Format(double v)
        {
            string s = v.ToString(_valueFormat);
            return string.IsNullOrEmpty(_unitText) ? s : s + " " + _unitText;
        }

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

        private void SetLow(double v)
        {
            v = Math.Round(v * 100.0) / 100.0;
            v = Math.Max(_minValue, Math.Min(_maxValue, v));
            if (v > _highValue) { _highValue = v; Invalidate(); }   // 越过高端时推动高端，避免属性设置顺序导致钳制
            if (Math.Abs(v - _lowValue) < double.Epsilon) return;
            _lowValue = v;
            Invalidate();
            LowValueChanged?.Invoke(this, EventArgs.Empty);
            RangeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetHigh(double v)
        {
            v = Math.Round(v * 100.0) / 100.0;
            v = Math.Min(_maxValue, Math.Max(_minValue, v));
            if (v < _lowValue) { _lowValue = v; Invalidate(); }
            if (Math.Abs(v - _highValue) < double.Epsilon) return;
            _highValue = v;
            Invalidate();
            HighValueChanged?.Invoke(this, EventArgs.Empty);
            RangeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ClampValues()
        {
            _lowValue = Math.Max(_minValue, Math.Min(_maxValue, _lowValue));
            _highValue = Math.Max(_lowValue, Math.Min(_maxValue, _highValue));
        }

        /// <summary>命中最近滑块开始拖动。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Focus();
            float xl = ValueToX(_lowValue), xh = ValueToX(_highValue);
            float dl = Math.Abs(e.X - xl), dh = Math.Abs(e.X - xh);
            if (dl <= ThumbR + 6 || dh <= ThumbR + 6)
                _drag = dl <= dh ? 0 : 1;
            else if (e.Y >= TrackY - 10 && e.Y <= TrackY + 10)
                _drag = dl <= dh ? 0 : 1;
            _focusThumb = _drag;
            if (_drag >= 0) MoveThumb(e.X);
        }

        /// <summary>拖动滑块。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_drag >= 0) MoveThumb(e.X);
        }

        /// <summary>结束拖动。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _drag = -1;
        }

        private void MoveThumb(float x)
        {
            double v = XToValue(x);
            if (_drag == 0) SetLow(v);
            else SetHigh(v);
        }

        /// <summary>方向键微调聚焦滑块。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            double step = (_maxValue - _minValue) / 50.0;
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Down)
            {
                if (_focusThumb == 1) SetHigh(_highValue - step);
                else SetLow(_lowValue - step);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right || e.KeyCode == Keys.Up)
            {
                if (_focusThumb == 1) SetHigh(_highValue + step);
                else SetLow(_lowValue + step);
                e.Handled = true;
            }
        }

        /// <summary>自绘轨道、区间、滑块、气泡与端标签。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float y = TrackY;
            float xl = ValueToX(_lowValue), xh = ValueToX(_highValue);
            float tx = PadX - 2, tw = Width - (PadX - 2) * 2;

            // 底色轨道（半高全圆角胶囊）
            using (var trackPath = RoundedRect(tx, y - 3, tw, 6, 3))
            using (var trackBrush = new SolidBrush(HFluentPalette.Track))
                g.FillPath(trackBrush, trackPath);
            // 选中区间
            if (xh > xl)
                using (var rangePath = RoundedRect(xl, y - 3, xh - xl, 6, 3))
                using (var rangeBrush = new SolidBrush(_accentColor))
                    g.FillPath(rangeBrush, rangePath);

            if (_showEndLabels)
            {
                using (var font = new Font(Font.FontFamily, Font.SizeInPoints - 0.5f))
                {
                    TextRenderer.DrawText(g, Format(_minValue), font,
                        new Rectangle(0, (int)y + 8, 90, 18), HFluentPalette.MutedText,
                        TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
                    TextRenderer.DrawText(g, Format(_maxValue), font,
                        new Rectangle(Width - 90, (int)y + 8, 90, 18), HFluentPalette.MutedText,
                        TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);
                }
            }

            DrawThumb(g, xl, y, _lowValue);
            DrawThumb(g, xh, y, _highValue);
        }

        private static GraphicsPath RoundedRect(float x, float y, float w, float h, float r)
        {
            var p = new GraphicsPath();
            if (w <= 0) return p;
            float d = r * 2f;
            p.AddArc(x, y, d, d, 180, 90);
            p.AddArc(x + w - d, y, d, d, 270, 90);
            p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            p.AddArc(x, y + h - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private void DrawThumb(Graphics g, float x, float y, double v)
        {
            if (_showBubbles)
            {
                string text = Format(v);
                Size ts = TextRenderer.MeasureText(g, text, Font, new Size(400, BubbleH),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int bw = ts.Width + 20;
                int bx = (int)(x - bw / 2f);
                bx = Math.Max(2, Math.Min(Width - bw - 2, bx));
                using (var path = RoundedRect(bx, 0, bw, BubbleH, 8))
                using (var bb = new SolidBrush(HFluentPalette.Bubble))
                    g.FillPath(bb, path);
                // 气泡底部小三角
                g.FillPolygon(new SolidBrush(HFluentPalette.Bubble), new[]
                {
                    new PointF(x - 4, BubbleH - 1),
                    new PointF(x + 4, BubbleH - 1),
                    new PointF(x, BubbleH + 3)
                });
                TextRenderer.DrawText(g, text, Font, new Rectangle(bx, 0, bw, BubbleH), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }

            // 外环白色底 + 强调色描边 + 中心实点
            g.FillEllipse(Brushes.White, x - ThumbR, y - ThumbR, ThumbR * 2, ThumbR * 2);
            using (var pen = new Pen(_accentColor, 3f))
                g.DrawEllipse(pen, x - ThumbR, y - ThumbR, ThumbR * 2, ThumbR * 2);
            using (var dot = new SolidBrush(_accentColor))
                g.FillEllipse(dot, x - Gap / 2f, y - Gap / 2f, Gap, Gap);
        }
    }
}
