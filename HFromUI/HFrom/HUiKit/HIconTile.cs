using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.HUiKit
{
    /// <summary>
    /// 彩色圆角图标块：纯色圆角方块 + 白色矢量图标（或自定义图片），
    /// 用于卡片/列表标题前的分类标识，如协议卡片“Modbus/S7/OPC-UA”图标。
    /// 内置 27 种矢量图标，无需图片资源；也可通过 Image 属性使用自定义图片。
    /// </summary>
    [DefaultProperty("Glyph")]
    [DefaultEvent("Click")]
    public class HIconTile : Control
    {
        /// <summary>_glyph 字段。</summary>
        private HGlyphKind _glyph = HGlyphKind.Layers;
        /// <summary>_tileColor 字段。</summary>
        private Color _tileColor = Color.FromArgb(124, 108, 246);
        /// <summary>_radius 字段。</summary>
        private int _radius = 10;
        /// <summary>_image 字段。</summary>
        private Image _image = null;

        /// <summary>矢量图标种类</summary>
        [Category("HFromUI"), Description("矢量图标种类"), DefaultValue(HGlyphKind.Layers)]
        public HGlyphKind Glyph
        {
            get { return _glyph; }
            set { _glyph = value; Invalidate(); }
        }

        /// <summary>方块底色（图标为白色）</summary>
        [Category("HFromUI"), Description("方块底色")]
        public Color TileColor
        {
            get { return _tileColor; }
            set { _tileColor = value; Invalidate(); }
        }

        /// <summary>圆角半径</summary>
        [Category("HFromUI"), Description("圆角半径"), DefaultValue(10)]
        public int Radius
        {
            get { return _radius; }
            set { _radius = value; Invalidate(); }
        }

        /// <summary>自定义图片（设置后优先于矢量图标，自动按白色蒙版着色）</summary>
        [Category("HFromUI"), Description("自定义图片（可选，覆盖矢量图标）")]
        public Image Image
        {
            get { return _image; }
            set { _image = value; Invalidate(); }
        }

        /// <summary>背景颜色。</summary>
        /// <summary>背景颜色。</summary>
        [Browsable(false)]
        public override Color BackColor { get { return Color.Transparent; } set { } }

        public HIconTile()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(40, 40);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = HGlyph.RoundedRect(r, _radius))
            {
                // 轻微纵向渐变，增加立体感
                using (LinearGradientBrush lb = new LinearGradientBrush(r,
                    HGlyph.Lighten(_tileColor, 0.08f), HGlyph.Darken(_tileColor, 0.06f), 90f))
                {
                    g.FillPath(lb, path);
                }
            }

            int iconSize = (int)(Math.Min(Width, Height) * 0.56f);
            Rectangle iconRect = new Rectangle((Width - iconSize) / 2, (Height - iconSize) / 2, iconSize, iconSize);
            if (_image != null)
            {
                g.DrawImage(_image, iconRect);
            }
            else
            {
                HGlyph.Draw(g, _glyph, iconRect, Color.White, 2.0f);
            }
        }
    }
}
