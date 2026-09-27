using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>
    /// HMI 量测仪表基类（HGauge/HDialPlate/HThermometer 共用）：量程上下限、当前值、
    /// 单位、刻度段数、13 色调色板，以及指针/液柱缓动动画。派生类只负责按 Style 自绘外形。
    /// </summary>
    public abstract class HInstrumentBase : HLabelBase
    {
        private readonly Timer _animTimer;
        private HToolTheme _theme = HToolTheme.Classic;
        private double _minValue;
        private double _maxValue = 100d;
        private double _value;
        private double _shown;
        private bool _smooth = true;
        private string _unitText = string.Empty;
        private string _valueFormat = "{0:0}";
        private int _segmentCount = 10;

        /// <summary>初始化缓动定时器与透明底双缓冲。</summary>
        protected HInstrumentBase()
        {
            AutoSize = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            _animTimer = new Timer { Interval = 20 };
            _animTimer.Tick += AnimTick;
        }

        /// <summary>图元色调，Classic 为白盘红针的工业经典配色。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调，Classic 为工业经典白盘红针"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>量程下限（刻度最小值）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("量程下限"), HDescriptionLanguage("量程下限，刻度最小值"), Browsable(true)]
        public double MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                if (_maxValue <= _minValue) _maxValue = _minValue + 1d;
                ClampValue();
                Invalidate();
            }
        }

        /// <summary>量程上限（刻度最大值）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("量程上限"), HDescriptionLanguage("量程上限，刻度最大值"), Browsable(true)]
        public double MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                if (_minValue >= _maxValue) _minValue = _maxValue - 1d;
                ClampValue();
                Invalidate();
            }
        }

        /// <summary>当前量测值，超出量程自动夹取；开启缓动时指针/液柱平滑移动。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("当前值"), HDescriptionLanguage("当前量测值，超出量程自动夹取"), Browsable(true)]
        public double Value
        {
            get => _value;
            set
            {
                double v = Math.Max(_minValue, Math.Min(_maxValue, value));
                if (Math.Abs(v - _value) < 0.000001d) return;
                _value = v;
                OnValueChanged(EventArgs.Empty);
                if (_smooth && IsHandleCreated) _animTimer.Start();
                else { _shown = v; Invalidate(); }
            }
        }

        /// <summary>是否启用指针/液柱缓动动画，false 时值变化立即到位。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("缓动动画"), HDescriptionLanguage("是否启用指针/液柱缓动动画"), Browsable(true)]
        [DefaultValue(true)]
        public bool SmoothAnimation
        {
            get => _smooth;
            set
            {
                _smooth = value;
                if (!value) { _shown = _value; _animTimer.Stop(); Invalidate(); }
            }
        }

        /// <summary>单位文本（如 rpm、MPa、℃），显示在表盘数值旁。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("单位文本"), HDescriptionLanguage("单位文本，显示在数值旁"), Browsable(true)]
        [DefaultValue("")]
        public string UnitText
        {
            get => _unitText;
            set { _unitText = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>数值显示格式（标准 .NET 复合格式，如 {0:0.0}）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("数值格式"), HDescriptionLanguage("数值显示格式，如 {0:0.0}"), Browsable(true)]
        [DefaultValue("{0:0}")]
        public string ValueFormat
        {
            get => _valueFormat;
            set { _valueFormat = string.IsNullOrEmpty(value) ? "{0:0}" : value; Invalidate(); }
        }

        /// <summary>主刻度段数（夹取 1..100）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("刻度段数"), HDescriptionLanguage("主刻度分段数量"), Browsable(true)]
        [DefaultValue(10)]
        public int SegmentCount
        {
            get => _segmentCount;
            set
            {
                _segmentCount = Math.Max(1, Math.Min(100, value));
                Invalidate();
            }
        }

        /// <summary>当前值变化事件。</summary>
        public event EventHandler ValueChanged;

        /// <summary>当前色调对应的完整配色方案。</summary>
        protected HHmiScheme Scheme => HHmiPalettes.Get(_theme);

        /// <summary>动画当前显示值（缓动过程中介于旧值与目标值之间）。</summary>
        protected double ShownValue => _shown;

        /// <summary>按格式字符串渲染当前值文本。</summary>
        protected string FormatValue(double v)
        {
            try { return string.Format(System.Globalization.CultureInfo.CurrentCulture, _valueFormat, v); }
            catch (FormatException) { return v.ToString("0.###"); }
        }

        /// <summary>量测值映射到 0..1 比例。</summary>
        protected float RatioOf(double v)
        {
            double span = _maxValue - _minValue;
            return span <= 0d ? 0f : (float)((v - _minValue) / span);
        }

        /// <summary>当前值（动画值）映射到 0..1 比例。</summary>
        protected float ShownRatio => RatioOf(_shown);

        /// <summary>0..1 比例映射到量程值。</summary>
        protected double ValueAtRatio(float t) => _minValue + (_maxValue - _minValue) * Math.Max(0f, Math.Min(1f, t));

        /// <summary>圆周上取点：angleDeg 为以 12 点方向为 0°、顺时针角度。</summary>
        protected static PointF Polar(float cx, float cy, float radius, float angleDeg)
        {
            double a = (angleDeg - 90d) * Math.PI / 180d;
            return new PointF(cx + radius * (float)Math.Cos(a), cy + radius * (float)Math.Sin(a));
        }

        /// <summary>主刻度 i（0..SegmentCount）对应的量测值。</summary>
        protected double SegmentValue(int i) => ValueAtRatio((float)i / Math.Max(1, _segmentCount));

        /// <summary>触发 <see cref="ValueChanged"/>。</summary>
        protected virtual void OnValueChanged(EventArgs e) => ValueChanged?.Invoke(this, e);

        private void ClampValue()
        {
            _value = Math.Max(_minValue, Math.Min(_maxValue, _value));
            _shown = _value;
        }

        private void AnimTick(object sender, EventArgs e)
        {
            if (IsDisposed) { _animTimer.Stop(); return; }
            double delta = _value - _shown;
            if (Math.Abs(delta) < Math.Max(0.01d, (_maxValue - _minValue) * 0.0008d))
            {
                _shown = _value;
                _animTimer.Stop();
            }
            else _shown += delta * 0.22d;
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _shown = _value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
