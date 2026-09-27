using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.HUiKit
{
    /// <summary>状态灯颜色语义</summary>
    public enum HLedState
    {
        /// <summary>灰色（离线/未使用）</summary>
        Off = 0,
        /// <summary>绿色（在线/正常）</summary>
        Green = 1,
        /// <summary>红色（报警/录像）</summary>
        Red = 2,
        /// <summary>黄色（警告/连接中）</summary>
        Yellow = 3,
        /// <summary>蓝色（信息/主色）</summary>
        Blue = 4
    }

    /// <summary>
    /// 紧凑型状态指示灯：圆形发光 LED，支持状态语义色、呼吸闪烁（如录像红点）。
    /// 可设置 Text 在灯右侧显示说明文字。深色/浅色界面均适用。
    /// </summary>
    [DefaultEvent("Click")]
    public class HStatusLed : Control
    {
        /// <summary>_state 字段。</summary>
        private HLedState _state = HLedState.Off;
        /// <summary>_customColor 字段。</summary>
        private Color _customColor = Color.Gray;
        /// <summary>_blink 字段。</summary>
        private bool _blink = false;
        /// <summary>_blinkInterval 字段。</summary>
        private int _blinkInterval = 500;
        /// <summary>_blinkLit 字段。</summary>
        private bool _blinkLit = true;
        private readonly System.Windows.Forms.Timer _timer;

        /// <summary>灯状态（自动切换语义色）</summary>
        [Category("HFromUI"), Description("灯状态")]
        public HLedState State
        {
            get { return _state; }
            set
            {
                _state = value;
                Invalidate();
            }
        }

        /// <summary>自定义灯颜色（State 取 Off 以外时按语义色；需要特殊颜色时设置此属性）</summary>
        [Category("HFromUI"), Description("自定义灯颜色")]
        public Color CustomColor
        {
            get { return _customColor; }
            set { _customColor = value; Invalidate(); }
        }

        /// <summary>是否闪烁（如录像指示）</summary>
        [Category("HFromUI"), Description("是否闪烁")]
        public bool Blink
        {
            get { return _blink; }
            set
            {
                _blink = value;
                _blinkLit = true;
                if (_blink)
                {
                    _timer.Interval = BlinkInterval;
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
                Invalidate();
            }
        }

        /// <summary>闪烁间隔（毫秒）</summary>
        [Category("HFromUI"), Description("闪烁间隔毫秒")]
        public int BlinkInterval
        {
            get { return _blinkInterval; }
            set
            {
                _blinkInterval = value < 100 ? 100 : value;
                if (_timer.Enabled) _timer.Interval = _blinkInterval;
            }
        }

        /// <summary>说明文字（显示在灯右侧）</summary>
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get { return base.Text; }
            set { base.Text = value; Invalidate(); }
        }

        public HStatusLed()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Size = new Size(16, 16);
            Font = new Font("微软雅黑", 9F);
            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += (s, e) =>
            {
                _blinkLit = !_blinkLit;
                Invalidate();
            };
        }

        /// <summary>状态语义色</summary>
        public static Color StateColor(HLedState state)
        {
            switch (state)
            {
                case HLedState.Green: return Color.FromArgb(48, 209, 88);
                case HLedState.Red: return Color.FromArgb(255, 69, 58);
                case HLedState.Yellow: return Color.FromArgb(255, 179, 0);
                case HLedState.Blue: return Color.FromArgb(45, 127, 249);
                default: return Color.FromArgb(110, 116, 128);
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Color lit = StateColor(_state);
            bool dark = _blink && !_blinkLit;
            Color draw = dark ? Color.FromArgb(90, 96, 106) : lit;

            // 有文字时灯直径=高度-2，文字画右侧；无文字时灯居中铺满
            int d;
            float ledX;
            if (!string.IsNullOrEmpty(Text))
            {
                d = Height - 2;
                ledX = 1;
            }
            else
            {
                d = Math.Min(Width, Height) - 2;
                ledX = (Width - d) / 2f;
            }
            float ledY = (Height - d) / 2f;
            RectangleF ledRect = new RectangleF(ledX, ledY, d, d);

            // 外发光（半透明环）
            if (!dark || _state == HLedState.Off)
            {
                using (GraphicsPath glow = new GraphicsPath())
                {
                    glow.AddEllipse(ledRect);
                    using (PathGradientBrush pb = new PathGradientBrush(glow))
                    {
                        pb.CenterColor = Color.FromArgb(_state == HLedState.Off ? 20 : 70, draw);
                        pb.SurroundColors = new Color[] { Color.FromArgb(0, draw) };
                        RectangleF glowRect = RectangleF.Inflate(ledRect, d * 0.35f, d * 0.35f);
                        g.FillEllipse(pb, glowRect);
                    }
                }
            }

            // 灯体（径向渐变：中心亮、边缘深）
            using (GraphicsPath body = new GraphicsPath())
            {
                body.AddEllipse(ledRect);
                using (PathGradientBrush pb = new PathGradientBrush(body))
                {
                    Color edge = dark ? draw : Color.FromArgb((int)(draw.R * 0.75), (int)(draw.G * 0.75), (int)(draw.B * 0.75));
                    pb.CenterColor = ControlPaint.Light(draw, 0.55f);
                    pb.SurroundColors = new Color[] { edge };
                    pb.CenterPoint = new PointF(ledRect.X + d * 0.35f, ledRect.Y + d * 0.32f);
                    g.FillPath(pb, body);
                }
                using (Pen pen = new Pen(ControlPaint.Dark(draw, 0.25f)))
                {
                    g.DrawEllipse(pen, ledRect);
                }
            }

            // 高光点
            using (SolidBrush hl = new SolidBrush(Color.FromArgb(110, 255, 255, 255)))
            {
                g.FillEllipse(hl, ledRect.X + d * 0.22f, ledRect.Y + d * 0.16f, d * 0.22f, d * 0.18f);
            }

            // 右侧文字
            if (!string.IsNullOrEmpty(Text))
            {
                using (SolidBrush tb = new SolidBrush(ForeColor))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
                {
                    RectangleF tr = new RectangleF(ledX + d + 4, 0, Width - ledX - d - 4, Height);
                    g.DrawString(Text, Font, tb, tr, sf);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
