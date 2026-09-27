using System;
        using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.UiKit
{
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 现代卡片容器：白色圆角卡片 + 柔和投影，可选彩色描边（紫/橙/红等）与右上角状态圆点。
    /// 用于设备卡片、协议卡片、信息面板。子控件直接放入卡片内（推荐 Dock + Padding）。
    /// </summary>
    [DefaultProperty("AccentColor")]
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(System.ComponentModel.Design.IDesigner))]
    public class HCard : Panel
    {
        /// <summary>_radius 字段。</summary>
        private int _radius = 10;
        /// <summary>_shadowSize 字段。</summary>
        private int _shadowSize = 6;
        /// <summary>_cardColor 字段。</summary>
        private Color _cardColor = Color.White;
        /// <summary>_borderColor 字段。</summary>
        private Color _borderColor = Color.FromArgb(228, 231, 238);
        /// <summary>_accentColor 字段。</summary>
        private Color _accentColor = Color.Empty;
        /// <summary>_statusColor 字段。</summary>
        private Color _statusColor = Color.FromArgb(34, 197, 94);
        /// <summary>_statusDotVisible 字段。</summary>
        private bool _statusDotVisible = false;

        /// <summary>卡片圆角半径</summary>
        [Category("HFromUI"), Description("卡片圆角半径"), DefaultValue(10)]
        public int Radius
        {
            get { return _radius; }
            set { _radius = value; Invalidate(); }
        }

        /// <summary>投影厚度（0 关闭阴影）</summary>
        [Category("HFromUI"), Description("投影厚度，0 关闭阴影"), DefaultValue(6)]
        public int ShadowSize
        {
            get { return _shadowSize; }
            set { _shadowSize = value; UpdatePadding(); Invalidate(); }
        }

        /// <summary>卡片底色</summary>
        [Category("HFromUI"), Description("卡片底色")]
        public Color CardColor
        {
            get { return _cardColor; }
            set { _cardColor = value; Invalidate(); }
        }

        /// <summary>普通描边色（未设置 AccentColor 时使用）</summary>
        [Category("HFromUI"), Description("普通描边色")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }

        /// <summary>强调描边色（设置后卡片以该色描边，如紫=正常/橙=警告/红=报警；Empty 使用 BorderColor）</summary>
        [Category("HFromUI"), Description("强调描边色（紫/橙/红等），Empty 用普通描边")]
        public Color AccentColor
        {
            get { return _accentColor; }
            set { _accentColor = value; Invalidate(); }
        }

        /// <summary>右上角状态圆点颜色</summary>
        [Category("HFromUI"), Description("右上角状态圆点颜色")]
        public Color StatusColor
        {
            get { return _statusColor; }
            set { _statusColor = value; Invalidate(); }
        }

        /// <summary>是否显示右上角状态圆点（设备在线绿点）</summary>
        [Category("HFromUI"), Description("显示右上角状态圆点"), DefaultValue(false)]
        public bool StatusDotVisible
        {
            get { return _statusDotVisible; }
            set { _statusDotVisible = value; Invalidate(); }
        }

        public HCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(245, 247, 250);
            UpdatePadding();
        }

        /// <summary>UpdatePadding 方法。</summary>
        private void UpdatePadding()
        {
            int s = _shadowSize + 2;
            Padding = new Padding(s + 10, s + 10, s + 10, s + 10);
        }

        /// <summary>卡片实际区域（去掉阴影占位）</summary>
        public Rectangle CardRectangle
        {
            get { return new Rectangle(_shadowSize, _shadowSize, Width - _shadowSize * 2 - 1, Height - _shadowSize * 2 - 1); }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle card = CardRectangle;

            // 柔和投影：由外向内多层半透明圆角
            if (_shadowSize > 0)
            {
                for (int i = _shadowSize; i >= 1; i--)
                {
                    Rectangle sr = Rectangle.Inflate(card, i, i);
                    sr.Offset(0, 1);
                    int alpha = 6 + (_shadowSize - i) * 3;
                    using (GraphicsPath sp = HGlyph.RoundedRect(sr, _radius + i))
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(alpha, 30, 41, 59)))
                        g.FillPath(sb, sp);
                }
            }

            // 卡片底
            using (GraphicsPath path = HGlyph.RoundedRect(card, _radius))
            {
                using (SolidBrush b = new SolidBrush(_cardColor))
                    g.FillPath(b, path);

                Color bc = _accentColor.IsEmpty ? _borderColor : _accentColor;
                float bw = _accentColor.IsEmpty ? 1f : 1.6f;
                using (Pen p = new Pen(bc, bw))
                    g.DrawPath(p, path);
            }

            // 右上角状态圆点（白环 + 实心点，骑在卡片上边框上）
            if (_statusDotVisible)
            {
                int r = 5;
                Point dot = new Point(card.Right - 16, card.Top + 2);
                using (SolidBrush wb = new SolidBrush(_cardColor))
                    g.FillEllipse(wb, dot.X - r - 2, dot.Y - r - 2, r * 2 + 4, r * 2 + 4);
                using (SolidBrush b = new SolidBrush(_statusColor))
                    g.FillEllipse(b, dot.X - r, dot.Y - r, r * 2, r * 2);
            }
        }
    }
}
