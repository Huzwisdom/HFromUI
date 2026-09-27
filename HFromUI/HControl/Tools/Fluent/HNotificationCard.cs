using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 通知卡片：左上圆形图标 + 标题/正文，右上角关闭，底部实心操作按钮
    /// （You have recieved a new message! 样式）。ActionClick/CloseClick 回调外部动作。
    /// </summary>
    [DefaultProperty("Title")]
    [DefaultEvent("ActionClick")]
    public class HNotificationCard : HFluentBase
    {
        private string _title = "You have recieved a new message!";
        private string _message = string.Empty;
        private HFluentGlyph _glyph = HFluentGlyph.Info;
        private Color _glyphColor = HFluentPalette.Info;
        private string _actionText = "Okay";
        private bool _dark = true;
        private bool _closeHover;

        private readonly HFluentButton _action;

        /// <summary>操作按钮点击。</summary>
        public event EventHandler ActionClick;
        /// <summary>关闭点击。</summary>
        public event EventHandler CloseClick;

        /// <summary>初始化布局与子按钮。</summary>
        public HNotificationCard()
        {
            SetStyle(ControlStyles.ContainerControl, true);
            BackColor = HFluentPalette.DarkPanel;

            _action = new HFluentButton
            {
                Text = "Okay",
                Glyph = HFluentGlyph.Check,
                ButtonKind = HFluentButtonKind.Solid,
                Size = new Size(120, 36)
            };
            _action.Click += (s, e) => ActionClick?.Invoke(this, EventArgs.Empty);
            Controls.Add(_action);
            Size = new Size(300, 172);
            _action.Bounds = new Rectangle(16, Height - 50, Width - 32, 36);
        }

        /// <summary>标题（首行大字）。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("标题"), HDescriptionLanguage("通知标题"), Browsable(true)]
        [DefaultValue("You have recieved a new message!")]
        public string Title
        {
            get => _title;
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>正文（标题下小字，可空）。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("正文"), HDescriptionLanguage("通知正文小字"), Browsable(true)]
        [DefaultValue("")]
        public string Message
        {
            get => _message;
            set { _message = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>左上图标。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("图标"), HDescriptionLanguage("左上圆形图标"), Browsable(true)]
        [DefaultValue(HFluentGlyph.Info)]
        public HFluentGlyph Glyph
        {
            get => _glyph;
            set { _glyph = value; Invalidate(); }
        }

        /// <summary>图标圆盘颜色。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("图标颜色"), HDescriptionLanguage("左上图标圆盘颜色"), Browsable(true)]
        public Color GlyphColor
        {
            get => _glyphColor;
            set { _glyphColor = value; Invalidate(); }
        }

        /// <summary>操作按钮文字（空则隐藏按钮）。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("操作按钮文字"), HDescriptionLanguage("底部操作按钮文字，空则隐藏"), Browsable(true)]
        [DefaultValue("Okay")]
        public string ActionText
        {
            get => _actionText;
            set
            {
                _actionText = value ?? string.Empty;
                _action.Text = _actionText;
                _action.Visible = !string.IsNullOrEmpty(_actionText);
                Invalidate();
            }
        }

        /// <summary>操作按钮外观。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("操作按钮外观"), HDescriptionLanguage("底部操作按钮颜色风格"), Browsable(true)]
        [DefaultValue(HFluentButtonKind.Solid)]
        public HFluentButtonKind ActionKind
        {
            get => _action.ButtonKind;
            set => _action.ButtonKind = value;
        }

        /// <summary>是否暗色卡片（默认暗色）。</summary>
        [HCategoryLanguage("Fluent 通知卡片"), HDisplayNameLanguage("暗色卡片"), HDescriptionLanguage("暗色卡片底"), Browsable(true)]
        [DefaultValue(true)]
        public bool Dark
        {
            get => _dark;
            set { _dark = value; Invalidate(); }
        }

        /// <summary>暴露操作按钮供外部进一步配置。</summary>
        [Browsable(false)]
        public HFluentButton ActionButton => _action;

        private Rectangle CloseRect => new Rectangle(Width - 34, 10, 24, 24);

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_action == null || string.IsNullOrEmpty(_actionText)) return;
            _action.Bounds = new Rectangle(16, Height - 50, Width - 32, 36);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hover = CloseRect.Contains(e.Location);
            if (hover != _closeHover) { _closeHover = hover; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_closeHover) { _closeHover = false; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && CloseRect.Contains(e.Location))
                CloseClick?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>自绘卡片、图标、文字与关闭号。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color card = _dark ? HFluentPalette.DarkPanel : Color.White;
            Color titleColor = _dark ? Color.White : Color.FromArgb(31, 41, 55);
            Color bodyColor = _dark ? Color.FromArgb(156, 163, 175) : HFluentPalette.MutedText;

            using (var path = HDrawPaint.CreateOuterBoxPath(Width, Height, 12))
            {
                using (var bb = new SolidBrush(card))
                    g.FillPath(bb, path);
                using (var pen = new Pen(_dark ? HFluentPalette.DarkBorder : Color.FromArgb(226, 228, 232)))
                    g.DrawPath(pen, path);
            }

            // 左上图标圆
            var iconBox = new RectangleF(16, 16, 34, 34);
            using (var ib = new SolidBrush(_glyphColor))
                g.FillEllipse(ib, iconBox);
            HFluentGlyphDraw.Draw(g, _glyph, new RectangleF(22, 22, 22, 22), Color.White, 2f, false);

            // 标题与正文
            var titleRect = new Rectangle(62, 14, Width - 62 - 40, 38);
            using (var titleFont = new Font(Font.FontFamily, Font.SizeInPoints + 1.5f, FontStyle.Bold))
                TextRenderer.DrawText(g, _title, titleFont,
                    titleRect, titleColor, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            if (!string.IsNullOrEmpty(_message))
                TextRenderer.DrawText(g, _message, Font,
                    new Rectangle(62, 54, Width - 62 - 20, Math.Max(20, Height - 110)),
                    bodyColor, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            // 关闭号
            var cr = CloseRect;
            HFluentGlyphDraw.Draw(g, HFluentGlyph.Close,
                new RectangleF(cr.X + 3, cr.Y + 3, 18, 18),
                _closeHover ? Color.White : Color.FromArgb(156, 163, 175), 2f, false);
        }
    }
}
