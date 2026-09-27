using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.MenuStrip
{
    using HFromUI.HFrom.HUiKit;
    /// <summary>
    /// 右键菜单图标种类（16x16，矢量绘制，无图片资源文件）。
    /// </summary>
    public enum HMenuIconKind { Snapshot, Record, Connect, Play, Stop, Zoom, Info }

    /// <summary>
    /// 右键菜单深色主题工具：图标绘制 + 深色配色表 + 深色渲染器。
    /// 用于 ContextMenuStrip 的深色主题美化，悬停圆角高亮、深色文字。
    /// </summary>
    public static class HDarkMenuTheme
    {
        /// <summary>IconColor 字段。</summary>
        private static readonly Color IconColor = Color.FromArgb(176, 182, 194);
        /// <summary>IconAccent 字段。</summary>
        private static readonly Color IconAccent = Color.FromArgb(90, 150, 249);
        /// <summary>IconRecord 字段。</summary>
        private static readonly Color IconRecord = Color.FromArgb(229, 72, 77);

        /// <summary>绘制 16x16 菜单图标（复用 HGlyph 矢量图标 + 自绘录像红点/信息圈）</summary>
        public static Bitmap CreateIcon(HMenuIconKind kind)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, 16, 16);
                switch (kind)
                {
                    case HMenuIconKind.Snapshot: // 相机机身 + 镜头
                        using (Pen p = new Pen(IconColor, 1.5f))
                        {
                            using (GraphicsPath body = HGlyph.RoundedRect(new Rectangle(1, 4, 14, 10), 3))
                                g.DrawPath(p, body);
                            g.DrawEllipse(p, 5.2f, 6.2f, 5.6f, 5.6f);
                            g.DrawLine(p, 5.5f, 4f, 6.3f, 2.4f);
                            g.DrawLine(p, 10.5f, 4f, 9.7f, 2.4f);
                        }
                        break;
                    case HMenuIconKind.Record: // 录像红点
                        using (SolidBrush b = new SolidBrush(IconRecord))
                            g.FillEllipse(b, 3.5f, 3.5f, 9f, 9f);
                        break;
                    case HMenuIconKind.Connect: // 插头/链路：两段圆角线 + 中点
                        using (Pen p = new Pen(IconAccent, 1.6f))
                        {
                            p.StartCap = LineCap.Round; p.EndCap = LineCap.Round;
                            g.DrawArc(p, 1.5f, 4.5f, 8f, 7f, 90f, 180f);
                            g.DrawLine(p, 5.5f, 8f, 10.5f, 8f);
                            g.DrawArc(p, 6.5f, 4.5f, 8f, 7f, 270f, 180f);
                        }
                        break;
                    case HMenuIconKind.Play:
                        HGlyph.Draw(g, HGlyphKind.Play, r, IconColor, 1.8f);
                        break;
                    case HMenuIconKind.Stop:
                        HGlyph.Draw(g, HGlyphKind.Stop, r, IconColor, 1.8f);
                        break;
                    case HMenuIconKind.Zoom:
                        HGlyph.Draw(g, HGlyphKind.Fit, r, IconColor, 1.8f);
                        break;
                    case HMenuIconKind.Info: // 信息圈 i
                        using (Pen p = new Pen(IconColor, 1.5f))
                        {
                            g.DrawEllipse(p, 1.5f, 1.5f, 13f, 13f);
                            g.DrawLine(p, 8f, 7.2f, 8f, 11.5f);
                        }
                        using (SolidBrush b = new SolidBrush(IconColor))
                            g.FillEllipse(b, 7.2f, 4f, 1.8f, 1.8f);
                        break;
                }
            }
            return bmp;
        }
    }

    /// <summary>右键菜单深色配色（ProfessionalColorTable：底色/悬停/分隔线/边框全深色）</summary>
    public sealed class HDarkMenuColors : ProfessionalColorTable
    {
        /// <summary>Bg 字段。</summary>
        private static Color Bg { get { return HUiTheme.PanelBg; } }
        /// <summary>Hover 字段。</summary>
        private static Color Hover { get { return Color.FromArgb(42, 52, 68); } }
        /// <summary>Border 字段。</summary>
        private static Color Border { get { return Color.FromArgb(58, 63, 74); } }

        /// <summary>ToolStripDropDownBackground 成员。</summary>
        public override Color ToolStripDropDownBackground { get { return Bg; } }
        /// <summary>ImageMarginGradientBegin 成员。</summary>
        public override Color ImageMarginGradientBegin { get { return Bg; } }
        /// <summary>ImageMarginGradientMiddle 成员。</summary>
        public override Color ImageMarginGradientMiddle { get { return Bg; } }
        /// <summary>ImageMarginGradientEnd 成员。</summary>
        public override Color ImageMarginGradientEnd { get { return Bg; } }
        /// <summary>MenuBorder 成员。</summary>
        public override Color MenuBorder { get { return Border; } }
        /// <summary>MenuItemBorder 成员。</summary>
        public override Color MenuItemBorder { get { return Color.Transparent; } }
        /// <summary>MenuItemSelected 成员。</summary>
        public override Color MenuItemSelected { get { return Hover; } }
        /// <summary>MenuItemSelectedGradientBegin 成员。</summary>
        public override Color MenuItemSelectedGradientBegin { get { return Hover; } }
        /// <summary>MenuItemSelectedGradientEnd 成员。</summary>
        public override Color MenuItemSelectedGradientEnd { get { return Hover; } }
        /// <summary>MenuItemPressedGradientBegin 成员。</summary>
        public override Color MenuItemPressedGradientBegin { get { return Bg; } }
        /// <summary>MenuItemPressedGradientEnd 成员。</summary>
        public override Color MenuItemPressedGradientEnd { get { return Bg; } }
        /// <summary>SeparatorDark 成员。</summary>
        public override Color SeparatorDark { get { return Border; } }
        /// <summary>SeparatorLight 成员。</summary>
        public override Color SeparatorLight { get { return Border; } }
        /// <summary>CheckBackground 成员。</summary>
        public override Color CheckBackground { get { return Color.Transparent; } }
        /// <summary>CheckSelectedBackground 成员。</summary>
        public override Color CheckSelectedBackground { get { return Color.Transparent; } }
        /// <summary>CheckPressedBackground 成员。</summary>
        public override Color CheckPressedBackground { get { return Color.Transparent; } }
    }

    /// <summary>右键菜单深色渲染器：悬停圆角高亮、深色文字、禁用项灰字</summary>
    public sealed class HDarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public HDarkMenuRenderer() : base(new HDarkMenuColors()) { }

        /// <summary>响应 RenderMenuItemBackground 事件。</summary>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected && e.Item.Enabled)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rc = new Rectangle(2, 1, e.Item.Width - 5, e.Item.Height - 2);
                using (GraphicsPath path = HGlyph.RoundedRect(rc, 4))
                using (SolidBrush b = new SolidBrush(Color.FromArgb(42, 52, 68)))
                    g.FillPath(b, path);
                return;
            }
            base.OnRenderMenuItemBackground(e);
        }

        /// <summary>响应 RenderItemText 事件。</summary>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (!e.Item.Enabled) e.TextColor = Color.FromArgb(110, 116, 128);
            else e.TextColor = Color.FromArgb(226, 230, 236);
            base.OnRenderItemText(e);
        }
    }
}
