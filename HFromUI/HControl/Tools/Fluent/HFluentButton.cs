using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>按钮外观：实心/描边/渐变/彩虹/暗色/弱化。</summary>
    public enum HFluentButtonKind
    {
        /// <summary>强调色实心按钮。</summary>
        Solid = 0,
        /// <summary>透明底 + 强调色描边与文字。</summary>
        Outline = 1,
        /// <summary>粉紫斜向渐变实心。</summary>
        Gradient = 2,
        /// <summary>六色横向彩虹实心。</summary>
        Rainbow = 3,
        /// <summary>深灰实心（暗色窗体上的登录/更新按钮）。</summary>
        Dark = 4,
        /// <summary>透明弱化文字按钮（暗色面板上的次要操作）。</summary>
        Subtle = 5
    }

    /// <summary>
    /// Fluent 风格按钮：圆角、悬停提亮/按下压暗，可选左侧矢量图标、副标题（双行按钮），
    /// 按住超过 500ms 触发 LongPress（LongPressRepeat=true 时持续重发，用于 Press &amp; hold）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class HFluentButton : HButtonBase
    {
        private HFluentButtonKind _kind = HFluentButtonKind.Solid;
        private HFluentGlyph _glyph;
        private Color _glyphColor = Color.Empty;
        private string _subtitle = string.Empty;
        private Color _accentColor = HFluentPalette.Accent;
        private bool _longPressRepeat;

        private readonly Timer _holdTimer;
        private int _holdTicks;

        /// <summary>长按触发（按住约 500ms）。</summary>
        public event EventHandler LongPress;

        /// <summary>初始化默认尺寸与圆角。</summary>
        public HFluentButton()
        {
            _radius = 8;
            Size = new Size(120, 38);
            ForeColor = Color.White;
            _holdTimer = new Timer { Interval = 120 };
            _holdTimer.Tick += (s, e) => HoldTick();
        }

        /// <summary>按钮外观。</summary>
        [HCategoryLanguage("Fluent 按钮"), HDisplayNameLanguage("按钮外观"), HDescriptionLanguage("实心/描边/渐变/彩虹/暗色/弱化"), Browsable(true)]
        [DefaultValue(HFluentButtonKind.Solid)]
        public HFluentButtonKind ButtonKind
        {
            get => _kind;
            set { _kind = value; Invalidate(); }
        }

        /// <summary>左侧矢量图标。</summary>
        [HCategoryLanguage("Fluent 按钮"), HDisplayNameLanguage("矢量图标"), HDescriptionLanguage("按钮文字左侧的内置矢量图标"), Browsable(true)]
        [DefaultValue(HFluentGlyph.None)]
        public HFluentGlyph Glyph
        {
            get => _glyph;
            set { _glyph = value; Invalidate(); }
        }

        /// <summary>图标颜色（默认随文字色）。</summary>
        [HCategoryLanguage("Fluent 按钮"), HDisplayNameLanguage("图标颜色"), HDescriptionLanguage("矢量图标颜色，未设置时使用文字色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color GlyphColor
        {
            get => _glyphColor;
            set { _glyphColor = value; Invalidate(); }
        }

        /// <summary>副标题（第二行小字，非空时按钮为双行样式，如 Search / Search topics online）。</summary>
        [HCategoryLanguage("Fluent 按钮"), HDisplayNameLanguage("副标题"), HDescriptionLanguage("第二行小字副标题"), Browsable(true)]
        [DefaultValue("")]
        public string Subtitle
        {
            get => _subtitle;
            set { _subtitle = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>强调色（实心/描边按钮）。</summary>
        [HCategoryLanguage("Fluent 按钮"), HDisplayNameLanguage("强调色"), HDescriptionLanguage("实心填充与描边按钮的基准色"), Browsable(true)]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        /// <summary>长按时是否持续重发（false 只触发一次）。</summary>
        [HCategoryLanguage("Fluent 按钮"), HDisplayNameLanguage("长按重发"), HDescriptionLanguage("按住时是否持续触发长按事件"), Browsable(true)]
        [DefaultValue(false)]
        public bool LongPressRepeat
        {
            get => _longPressRepeat;
            set => _longPressRepeat = value;
        }

        /// <summary>解析各外观的基准填充色。</summary>
        private Color BaseColor()
        {
            switch (_kind)
            {
                case HFluentButtonKind.Solid: return _accentColor;
                case HFluentButtonKind.Dark: return HFluentPalette.DarkCard;
                case HFluentButtonKind.Gradient: return HFluentPalette.Purple;
                case HFluentButtonKind.Rainbow: return HFluentPalette.Purple;
                case HFluentButtonKind.Outline:
                case HFluentButtonKind.Subtle:
                    return Color.Transparent;
                default:
                    return Color.Transparent;
            }
        }

        private static Color HoverOf(Color c) => ControlPaint.Light(c, 0.12f);

        /// <summary>解析文字色：描边按钮跟随强调色，其余白色，弱化按钮浅灰。</summary>
        private Color ResolveTextColor()
        {
            if (_kind == HFluentButtonKind.Outline) return _accentColor;
            if (_kind == HFluentButtonKind.Subtle)
                return _hover ? Color.White : Color.FromArgb(209, 213, 219);
            return Color.White;
        }

        /// <summary>自绘按钮。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            RectangleF box = new RectangleF(0, 0, Width - 1, Height - 1);
            using (var path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            {
                switch (_kind)
                {
                    case HFluentButtonKind.Gradient:
                        using (var gb = HFluentPalette.PinkPurple(box))
                            g.FillPath(gb, path);
                        break;
                    case HFluentButtonKind.Rainbow:
                        using (var rb = HFluentPalette.RainbowBrush(box))
                            g.FillPath(rb, path);
                        break;
                    case HFluentButtonKind.Outline:
                        using (var ob = new SolidBrush(Color.FromArgb(_hover ? 28 : 0, _accentColor)))
                            g.FillPath(ob, path);
                        HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, 1.6f,
                            _hover ? HoverOf(_accentColor) : _accentColor);
                        break;
                    case HFluentButtonKind.Subtle:
                        if (_hover)
                            using (var sb = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                                g.FillPath(sb, path);
                        if (_pressed)
                            using (var pb2 = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                                g.FillPath(pb2, path);
                        break;
                    default:
                        Color fill = BaseColor();
                        using (var bb = new SolidBrush(fill))
                            g.FillPath(bb, path);
                        if (_hover)
                            using (var hl = new SolidBrush(Color.FromArgb(28, 255, 255, 255)))
                                g.FillPath(hl, path);
                        if (_pressed)
                            using (var pk = new SolidBrush(Color.FromArgb(26, 0, 0, 0)))
                                g.FillPath(pk, path);
                        break;
                }
            }

            if (Focused && ShowFocusCues)
            {
                using (var fp = new Pen(Color.FromArgb(120, ResolveTextColor()), 1.4f) { DashStyle = DashStyle.Dot })
                using (var fpath = HDrawPaint.CreateOuterBoxPath(Width - 4, Height - 4, Math.Max(2, _radius - 2)))
                {
                    var old = g.Save();
                    g.TranslateTransform(2f, 2f);
                    g.DrawPath(fp, fpath);
                    g.Restore(old);
                }
            }

            DrawContent(g);
        }

        /// <summary>图标 + 标题/副标题布局绘制，按下整体下沉 1px。</summary>
        private void DrawContent(Graphics g)
        {
            bool twoLine = !string.IsNullOrEmpty(_subtitle);
            Color textColor = ResolveTextColor();
            Color glyphColor = _glyphColor.IsEmpty ? textColor : _glyphColor;
            int dy = _pressed ? 1 : 0;

            float glyphBox = _glyph == HFluentGlyph.None ? 0 : Height * (twoLine ? 0.42f : 0.46f);
            float gap = glyphBox > 0 ? 8 : 0;

            Font titleFont = Font;
            Font subFont = null;
            if (twoLine)
            {
                titleFont = new Font(Font.FontFamily, Font.SizeInPoints + 0.5f, FontStyle.Bold);
                subFont = new Font(Font.FontFamily, Font.SizeInPoints - 1.5f);
            }
            Size titleSize = TextRenderer.MeasureText(g, Text ?? string.Empty, titleFont,
                new Size(Width, Height), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            Size subSize = twoLine ? TextRenderer.MeasureText(g, _subtitle, subFont,
                new Size(Width, Height), TextFormatFlags.SingleLine | TextFormatFlags.NoPadding) : Size.Empty;

            float contentW = glyphBox + gap + Math.Max(titleSize.Width, subSize.Width);
            float startX = (Width - contentW) / 2f;
            float cy = Height / 2f + dy;

            if (glyphBox > 0)
            {
                var gr = new RectangleF(startX, cy - glyphBox / 2f, glyphBox, glyphBox);
                HFluentGlyphDraw.Draw(g, _glyph, gr, glyphColor, Math.Max(1.6f, glyphBox * 0.12f), true);
            }

            float textX = startX + glyphBox + gap;
            if (twoLine)
            {
                float blockH = titleSize.Height + subSize.Height + 1;
                float top = cy - blockH / 2f;
                TextRenderer.DrawText(g, Text, titleFont,
                    new Rectangle((int)textX, (int)top, (int)(Width - textX - 8), titleSize.Height + 2),
                    textColor, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(g, _subtitle, subFont,
                    new Rectangle((int)textX, (int)(top + titleSize.Height + 1), (int)(Width - textX - 8), subSize.Height + 2),
                    Color.FromArgb(205, textColor.R, textColor.G, textColor.B),
                    TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
            else
            {
                TextRenderer.DrawText(g, Text ?? string.Empty, titleFont,
                    new Rectangle((int)textX, 0, (int)(Width - textX - 8), Height),
                    textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
            }
            if (twoLine)
            {
                titleFont.Dispose();
                subFont.Dispose();
            }
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            if (mevent.Button == MouseButtons.Left)
            {
                _holdTicks = 0;
                _holdTimer.Start();
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _holdTimer.Stop();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _holdTimer.Stop();
        }

        private void HoldTick()
        {
            _holdTicks++;
            // 约 500ms（4 拍后）首次触发
            if (_holdTicks < 4) return;
            LongPress?.Invoke(this, EventArgs.Empty);
            if (!_longPressRepeat) _holdTimer.Stop();
        }
    }
}
