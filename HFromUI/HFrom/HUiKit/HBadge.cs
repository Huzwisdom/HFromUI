using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HMath;

namespace HFromUI.HFrom.HUiKit
{
    using HFromUI.HLangage;
    /// <summary>徽章语义色</summary>
    public enum HBadgeKind
    {
        /// <summary>成功/在线（绿）</summary>
        Success = 0,
        /// <summary>信息/主色（蓝）</summary>
        Info = 1,
        /// <summary>警告（橙）</summary>
        Warning = 2,
        /// <summary>危险/离线（红）</summary>
        Danger = 3,
        /// <summary>品牌紫</summary>
        Purple = 4,
        /// <summary>中性灰</summary>
        Gray = 5
    }

    /// <summary>
    /// 状态徽章：圆角胶囊 + 状态圆点 + 文字，如“运行中/已连接/已停止”。
    /// 浅色 SaaS 风格：浅底色 + 同色系描边 + 深色文字，深底界面也可通过属性改色。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class HBadge : Control
    {
        /// <summary>_kind 字段。</summary>
        private HBadgeKind _kind = HBadgeKind.Success;
        /// <summary>_dotVisible 字段。</summary>
        private bool _dotVisible = true;
        /// <summary>_autoSize 字段。</summary>
        private bool _autoSize = true;

        /// <summary>徽章语义色（自动配套 浅底/描边/文字/圆点）</summary>
        [Category("HFromUI"), Description("徽章语义色"), DefaultValue(HBadgeKind.Success)]
        public HBadgeKind Kind
        {
            get { return _kind; }
            set { _kind = value; UpdateSize(); Invalidate(); }
        }

        /// <summary>是否显示左侧状态圆点</summary>
        [Category("HFromUI"), Description("是否显示状态圆点"), DefaultValue(true)]
        public bool DotVisible
        {
            get { return _dotVisible; }
            set { _dotVisible = value; UpdateSize(); Invalidate(); }
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

        public HBadge()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Font = new Font("微软雅黑", 8.5F);
            Text = HTranslation.GetContent("运行中");
            Size = new Size(64, 22);
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
                    case HBadgeKind.Info: return Color.FromArgb(59, 130, 246);
                    case HBadgeKind.Warning: return Color.FromArgb(245, 158, 11);
                    case HBadgeKind.Danger: return Color.FromArgb(239, 68, 68);
                    case HBadgeKind.Purple: return Color.FromArgb(99, 102, 241);
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
                int w = (int)Math.Ceiling(ts.Width) + 20;
                if (_dotVisible) w += 12;
                int h = (int)Math.Ceiling(ts.Height) + 8;
                if (h < 22) h = 22;
                Size = new Size(w, h);
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color main = MainColor;
            Color bg = HGlyph.Lighten(main, 0.88f);
            Color border = HGlyph.Lighten(main, 0.62f);
            Color text = HGlyph.Darken(main, 0.25f);

            // 胶囊：外轮廓固定像素盒，弧外接边长为控件高（r+0.5=H），边框 1px 向内填充环
            float rd = Height - 0.5f;
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, rd))
            {
                using (SolidBrush b = new SolidBrush(bg)) g.FillPath(b, path);
                HDrawPaint.FillRoundedBorder(g, Width, Height, rd, 1f, border);
            }

            int textX = 10;
            if (_dotVisible)
            {
                int dotR = 4;
                int dotX = 12;
                int dotY = Height / 2;
                using (SolidBrush b = new SolidBrush(main))
                    g.FillEllipse(b, dotX - dotR, dotY - dotR, dotR * 2, dotR * 2);
                textX = dotX + dotR + 5;
            }

            TextRenderer.DrawText(g, Text, Font,
                new Rectangle(textX, 0, Width - textX - 8, Height), text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
        }
    }
}
