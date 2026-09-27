using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 滑动开关：胶囊轨道 + 白色圆钮缓动，点击翻转 Checked。
    /// 可选文字时轨道居左、文字在右；无文字时控件即为轨道。
    /// </summary>
    [DefaultProperty("Checked")]
    [DefaultEvent("CheckedChanged")]
    public class HFluentToggle : HFluentBase
    {
        private static readonly Size SizeSwitch = new Size(46, 24);

        private bool _checked;
        private Color _checkedColor = HFluentPalette.Accent;
        private Color _uncheckedColor = Color.FromArgb(120, 120, 120);
        private float _knob;
        private readonly Timer _timer;

        /// <summary>选中态改变。</summary>
        public event EventHandler CheckedChanged;

        /// <summary>初始化。</summary>
        public HFluentToggle()
        {
            Size = SizeSwitch;
            Cursor = Cursors.Hand;
            _timer = new Timer { Interval = 15 };
            _timer.Tick += (s, e) =>
            {
                float target = _checked ? 1f : 0f;
                _knob += (target - _knob) * 0.35f;
                if (Math.Abs(target - _knob) < 0.01f) { _knob = target; _timer.Stop(); }
                Invalidate();
            };
        }

        /// <summary>是否开启。</summary>
        [HCategoryLanguage("Fluent 开关"), HDisplayNameLanguage("开启"), HDescriptionLanguage("开关是否开启"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                _timer.Start();
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>开启轨道色。</summary>
        [HCategoryLanguage("Fluent 开关"), HDisplayNameLanguage("开启色"), HDescriptionLanguage("开启状态轨道颜色"), Browsable(true)]
        public Color CheckedColor
        {
            get => _checkedColor;
            set { _checkedColor = value; Invalidate(); }
        }

        /// <summary>关闭轨道色。</summary>
        [HCategoryLanguage("Fluent 开关"), HDisplayNameLanguage("关闭色"), HDescriptionLanguage("关闭状态轨道颜色"), Browsable(true)]
        public Color UncheckedColor
        {
            get => _uncheckedColor;
            set { _uncheckedColor = value; Invalidate(); }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !_checked;
        }

        /// <summary>自绘轨道与圆钮。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool hasText = !string.IsNullOrEmpty(Text);
            int trackW = hasText ? SizeSwitch.Width : Width;
            int trackH = hasText ? SizeSwitch.Height : Height;
            int tx = 0;
            int ty = (Height - trackH) / 2;

            Color trackColor = BlendColor(_uncheckedColor, _checkedColor, _knob);
            using (var path = new GraphicsPath())
            {
                path.AddArc(tx, ty, trackH, trackH, 90, 180);
                path.AddArc(tx + trackW - trackH, ty, trackH, trackH, 270, 180);
                path.CloseFigure();
                using (var bb = new SolidBrush(trackColor))
                    g.FillPath(bb, path);
            }

            float pad = 3f;
            float d = trackH - pad * 2f;
            float kx = tx + pad + _knob * (trackW - trackH);
            using (var kp = new SolidBrush(Color.White))
                g.FillEllipse(kp, kx, ty + pad, d, d);

            if (hasText)
                TextRenderer.DrawText(g, Text, Font,
                    new Rectangle(trackW + 8, 0, Width - trackW - 8, Height),
                    ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }

        private static Color BlendColor(Color a, Color b, float t)
        {
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }
    }
}
