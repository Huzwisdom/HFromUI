using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 旋转等待点：八个圆点绕圈旋转，强调色为当前点、其余渐隐；
    /// 可选右侧文字（Randomise 按钮内等待样式），文字显示在 Text。
    /// </summary>
    [DefaultProperty("Text")]
    public class HFluentSpinner : HFluentBase
    {
        private bool _running = true;
        private int _angle;
        private Color _accentColor = HFluentPalette.Accent;
        private readonly Timer _timer;

        /// <summary>初始化。</summary>
        public HFluentSpinner()
        {
            Size = new Size(120, 30);
            ForeColor = Color.FromArgb(209, 213, 219);
            _timer = new Timer { Interval = 70 };
            _timer.Tick += (s, e) => { _angle = (_angle + 45) % 360; Invalidate(); };
        }

        /// <summary>是否旋转。</summary>
        [HCategoryLanguage("Fluent 等待点"), HDisplayNameLanguage("旋转中"), HDescriptionLanguage("是否持续旋转"), Browsable(true)]
        [DefaultValue(true)]
        public bool Running
        {
            get => _running;
            set
            {
                _running = value;
                UpdateTimer();
                Invalidate();
            }
        }

        /// <summary>圆点强调色。</summary>
        [HCategoryLanguage("Fluent 等待点"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("当前圆点颜色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UpdateTimer();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (_running && Visible && IsHandleCreated) _timer.Start();
            else _timer.Stop();
        }

        /// <summary>自绘旋转点与文字。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float d = Math.Min(Width, Height);
            float r = d / 2f;
            float cx = r, cy = Height / 2f;
            float dotR = d * 0.075f;
            float orbit = r - dotR - 1f;

            for (int i = 0; i < 8; i++)
            {
                double ang = (_angle + i * 45) * Math.PI / 180.0;
                float dx = cx + (float)Math.Cos(ang) * orbit - dotR;
                float dy = cy + (float)Math.Sin(ang) * orbit - dotR;
                // i=0 当前点最实，其余按距离渐隐
                int alpha = i == 0 ? 255 : Math.Max(30, 255 - i * 28);
                using (var bb = new SolidBrush(Color.FromArgb(alpha, _accentColor)))
                    g.FillEllipse(bb, dx, dy, dotR * 2f, dotR * 2f);
            }

            if (!string.IsNullOrEmpty(Text))
                TextRenderer.DrawText(g, Text, Font,
                    new Rectangle((int)d + 6, 0, Width - (int)d - 6, Height),
                    ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
        }
    }
}
