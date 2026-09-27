using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 进度条：细圆角灰轨 + 强调色填充；Indeterminate=true 时为循环流动的短块
    /// （Loading files 样式）。
    /// </summary>
    [DefaultProperty("Value")]
    public class HFluentProgressBar : HFluentBase
    {
        private double _value = 40.0;
        private bool _indeterminate;
        private bool _dark;
        private Color _accentColor = HFluentPalette.Accent;
        private readonly Timer _timer;
        private float _indX;

        /// <summary>初始化。</summary>
        public HFluentProgressBar()
        {
            Size = new Size(260, 6);
            _timer = new Timer { Interval = 15 };
            _timer.Tick += (s, e) =>
            {
                if (!_indeterminate || !Visible) return;
                _indX += Width * 0.02f;
                if (_indX > Width) _indX = -Width * 0.4f;
                Invalidate();
            };
        }

        /// <summary>进度值（0~100）。</summary>
        [HCategoryLanguage("Fluent 进度条"), HDisplayNameLanguage("进度值"), HDescriptionLanguage("0 到 100 的进度百分比"), Browsable(true)]
        public double Value
        {
            get => _value;
            set { _value = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        /// <summary>是否不确定模式（循环流动短块）。</summary>
        [HCategoryLanguage("Fluent 进度条"), HDisplayNameLanguage("不确定模式"), HDescriptionLanguage("无确定进度时循环流动"), Browsable(true)]
        [DefaultValue(false)]
        public bool Indeterminate
        {
            get => _indeterminate;
            set
            {
                _indeterminate = value;
                if (value && Visible) _timer.Start();
                else _timer.Stop();
                Invalidate();
            }
        }

        /// <summary>是否暗色主题（轨道深灰）。</summary>
        [HCategoryLanguage("Fluent 进度条"), HDisplayNameLanguage("暗色主题"), HDescriptionLanguage("暗色面板上使用深灰轨道"), Browsable(true)]
        [DefaultValue(false)]
        public bool Dark
        {
            get => _dark;
            set { _dark = value; Invalidate(); }
        }

        /// <summary>填充强调色。</summary>
        [HCategoryLanguage("Fluent 进度条"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("已完成段颜色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_indeterminate && Visible) _timer.Start();
            else if (!Visible) _timer.Stop();
        }

        /// <summary>自绘轨道与填充。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int h = Math.Min(6, Height);
            int y = (Height - h) / 2;

            using (var track = Capsule(0, y, Width, h))
            using (var tb = new SolidBrush(_dark ? Color.FromArgb(64, 64, 64) : HFluentPalette.Track))
                g.FillPath(tb, track);

            if (_indeterminate)
            {
                float w = Width * 0.38f;
                float x = _indX;
                using (var fill = Capsule(x, y, w, h))
                using (var bb = new SolidBrush(_accentColor))
                    g.FillPath(bb, fill);
            }
            else if (_value > 0)
            {
                float w = (float)(Width * _value / 100.0);
                using (var fill = Capsule(0, y, w, h))
                using (var bb = new SolidBrush(_accentColor))
                    g.FillPath(bb, fill);
            }
        }

        private static GraphicsPath Capsule(float x, float y, float w, float h)
        {
            var p = new GraphicsPath();
            if (w <= h) return p;
            p.AddArc(x, y, h, h, 90, 180);
            p.AddArc(x + w - h, y, h, h, 270, 180);
            p.CloseFigure();
            return p;
        }
    }
}
