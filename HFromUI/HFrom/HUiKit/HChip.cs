using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.HUiKit
{
    /// <summary>
    /// 标签芯片：浅底色 + 同色系文字的小圆角标签，如协议标签 “modbus_tcp / mqtt / fanuc”。
    /// 与 HBadge 区别：无圆点、更小更紧凑、用于分类标记。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class HChip : Control
    {
        /// <summary>_kind 字段。</summary>
        private HBadgeKind _kind = HBadgeKind.Info;
        /// <summary>_autoSize 字段。</summary>
        private bool _autoSize = true;

        /// <summary>芯片配色（复用 HBadgeKind 语义色）</summary>
        [Category("HFromUI"), Description("芯片配色"), DefaultValue(HBadgeKind.Info)]
        public HBadgeKind Kind
        {
            get { return _kind; }
            set { _kind = value; UpdateSize(); Invalidate(); }
        }

        /// <summary>是否随文字自动调整大小</summary>
        [Category("HFromUI"), Description("自动调整大小"), DefaultValue(true)]
        public override bool AutoSize
        {
            get { return _autoSize; }
            set { _autoSize = value; UpdateSize(); Invalidate(); }
        }

        /// <summary>背景颜色。</summary>
        /// <summary>背景颜色。</summary>
        [Browsable(false)]
        public override Color BackColor { get { return Color.Transparent; } set { } }

        public HChip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Font = new Font("微软雅黑", 8.5F);
            Text = "tag";
            Size = new Size(54, 20);
        }

        /// <summary>响应 TextChanged 事件。</summary>
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); UpdateSize(); Invalidate(); }
        /// <summary>响应 FontChanged 事件。</summary>
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); UpdateSize(); Invalidate(); }

        private Color MainColor
        {
            get
            {
                switch (_kind)
                {
                    case HBadgeKind.Success: return Color.FromArgb(34, 197, 94);
                    case HBadgeKind.Info: return Color.FromArgb(91, 143, 249);
                    case HBadgeKind.Warning: return Color.FromArgb(245, 158, 11);
                    case HBadgeKind.Danger: return Color.FromArgb(239, 68, 68);
                    case HBadgeKind.Purple: return Color.FromArgb(124, 108, 246);
                    default: return Color.FromArgb(156, 163, 175);
                }
            }
        }

        /// <summary>UpdateSize 方法。</summary>
        private void UpdateSize()
        {
            if (!_autoSize) return;
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SizeF ts = g.MeasureString(Text ?? "", Font);
                Size = new Size((int)Math.Ceiling(ts.Width) + 16, (int)Math.Ceiling(ts.Height) + 6);
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color main = MainColor;
            Color bg = HGlyph.Lighten(main, 0.87f);
            Color text = HGlyph.Darken(main, 0.18f);

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = HGlyph.RoundedRect(r, 4))
            using (SolidBrush b = new SolidBrush(bg))
                g.FillPath(b, path);

            TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height), text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }
}
