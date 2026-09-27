using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{
    /// <summary>
    /// 按钮基类：继承 Control 全自绘，外观交互与 Windows Button 一致
    /// （凸起/按下立体边、悬停高亮、焦点虚框、空格触发、&amp; 助记符、DialogResult），
    /// 并支持圆角背景。圆角通过 Radius 设置：0 或负数即原生直角样式，不使用枚举或开关。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class HButtonBase : Control
    {
        /// <summary>文字对齐（派生类共用）。</summary>
        protected ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        private DialogResult _dialogResult = DialogResult.None;
        /// <summary>圆角半径（派生类共用）。</summary>
        protected int _radius;
        /// <summary>圆角边框宽度（像素，派生类共用）。</summary>
        protected int _borderWidth = 1;
        /// <summary>外形内部背景色（Color.Empty 表示沿用 BackColor，派生类共用）。</summary>
        protected Color _shapeBackColor = Color.Empty;
        /// <summary>鼠标悬停/按下状态（派生类共用）。</summary>
        protected bool _hover, _pressed;

        public HButtonBase()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable |
                     ControlStyles.StandardClick, true);
            Font = new Font("微软雅黑", 9f);
            Size = new Size(75, 28);
        }

        [HCategoryLanguage("按钮基础设置"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径，0 或负数为 Windows 原生直角按钮"), Browsable(true)]
        [DefaultValue(0)]
        public virtual int Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                UpdateRegion();
                Invalidate();
            }
        }

        [HCategoryLanguage("按钮基础设置"), HDisplayNameLanguage("圆角边框宽度"), HDescriptionLanguage("圆角边框宽度（像素），0 或负数不描边；仅圆角模式生效。描边向内收半个线宽，粗边也完整不缺边"), Browsable(true)]
        [DefaultValue(1)]
        public virtual int BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; Invalidate(); }
        }

        [HCategoryLanguage("按钮基础设置"), HDisplayNameLanguage("框内背景色"), HDescriptionLanguage("圆角/椭圆/胶囊外形内部的背景色；未设置时使用控件背景色。颜色只填充外形内部，圆角外不染色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public virtual Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }

        [HCategoryLanguage("按钮基础设置"), HDisplayNameLanguage("文字对齐"), HDescriptionLanguage("文字对齐（九宫格）"), Browsable(true)]
        [DefaultValue(ContentAlignment.MiddleCenter)]
        public virtual ContentAlignment TextAlign
        {
            get => _textAlign;
            set { _textAlign = value; Invalidate(); }
        }

        [HCategoryLanguage("按钮基础设置"), HDisplayNameLanguage("对话框结果"), HDescriptionLanguage("点击后返回父窗体的对话框结果（None 不关闭窗体）"), Browsable(true)]
        [DefaultValue(DialogResult.None)]
        public DialogResult DialogResult
        {
            get => _dialogResult;
            set => _dialogResult = value;
        }

        /// <summary>以编程方式触发一次点击（等价于用户按下按钮）。</summary>
        public void PerformClick()
        {
            if (Enabled && Visible) OnClick(EventArgs.Empty);
        }

        /// <summary>助记符触发点击。</summary>
        protected override bool ProcessMnemonic(char charCode)
        {
            if (Enabled && Visible && IsMnemonic(charCode, Text))
            {
                PerformClick();
                return true;
            }
            return false;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (Enabled && _dialogResult != DialogResult.None)
            {
                var form = FindForm();
                if (form != null) form.DialogResult = _dialogResult;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        /// <summary>空格键模拟按下/抬起（Windows Button 键盘语义）。</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _pressed = true;
                Invalidate();
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _pressed = false;
                Invalidate();
                if (Enabled) OnClick(EventArgs.Empty);
            }
            base.OnKeyUp(e);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            _pressed = false;
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            if (_radius <= 0)
            {
                // 直角：完全沿用 Windows 原生立体按钮观感
                using (var brush = new SolidBrush(ResolveFillColor()))
                    g.FillRectangle(brush, rect);
                ControlPaint.DrawBorder3D(g, rect,
                    !Enabled ? Border3DStyle.SunkenOuter :
                    _pressed ? Border3DStyle.Sunken :
                    Focused ? Border3DStyle.RaisedInner : Border3DStyle.Raised);
            }
            else
            {
                // 圆角：外轮廓固定在像素盒边缘，底色填满后边框环按线宽全部向内，
                // 改线宽外轮廓不动，直边全实像素、四边等宽。
                using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
                {
                    using (var brush = new SolidBrush(ResolveFillColor()))
                        g.FillPath(brush, path);
                    if (_borderWidth > 0)
                    {
                        var borderColor = !Enabled ? SystemColors.ControlDark :
                            _pressed || _hover ? SystemColors.Highlight : SystemColors.ControlDarkDark;
                        HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, _borderWidth, borderColor);
                    }
                }
            }

            // 按下整体下沉 1px，文字禁用变灰，焦点内缩虚线框
            int dy = _pressed ? 1 : 0;
            var textRect = new Rectangle(0, dy, Width, Height);
            Color textColor = Enabled ? ForeColor : SystemColors.GrayText;
            TextRenderer.DrawText(g, Text, Font, textRect, textColor, TextFormatFromAlign(_textAlign));

            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(rect, -4, -4));
        }

        /// <summary>未显式设置 ForeColor（仍为系统默认文字色）时回退到指定色。</summary>
        protected Color ForeColorIfUnset(Color fallback) => ForeColor == SystemColors.ControlText ? fallback : ForeColor;

        /// <summary>九宫格对齐 → 组合内容左上角（按钮外观引擎同程序集共用）。</summary>
        internal static Point AlignOrigin(Rectangle container, Size content, ContentAlignment align)
        {
            int x, y;
            string a = align.ToString();
            if (a.EndsWith("Left")) x = container.Left;
            else if (a.EndsWith("Right")) x = container.Right - content.Width;
            else x = container.Left + (container.Width - content.Width) / 2;
            if (a.StartsWith("Top")) y = container.Top;
            else if (a.StartsWith("Bottom")) y = container.Bottom - content.Height;
            else y = container.Top + (container.Height - content.Height) / 2;
            return new Point(x, y);
        }

        /// <summary>九宫格对齐 → 文本格式标志。</summary>
        internal static TextFormatFlags TextFormatFromAlign(ContentAlignment align)
        {
            TextFormatFlags f = TextFormatFlags.EndEllipsis;
            string a = align.ToString();
            if (a.EndsWith("Left")) f |= TextFormatFlags.Left;
            else if (a.EndsWith("Right")) f |= TextFormatFlags.Right;
            else f |= TextFormatFlags.HorizontalCenter;
            if (a.StartsWith("Top")) f |= TextFormatFlags.Top;
            else if (a.StartsWith("Bottom")) f |= TextFormatFlags.Bottom;
            else f |= TextFormatFlags.VerticalCenter;
            return f;
        }

        /// <summary>
        /// 选择型控件（单选/复选）外框：Radius&gt;0 铺圆角底色并描边，否则只铺不透明底色；
        /// 返回内部内容区（圆角时向内留出描边与间隙）。
        /// </summary>
        protected Rectangle PaintCheckFrame(Graphics g, Color borderColor)
        {
            var rect = ClientRectangle;
            int bw = Math.Max(0, _borderWidth);
            if (_radius > 0)
            {
                // 外轮廓固定在控件像素盒边缘；边框厚度 bw 全部从外轮廓向内（填充环），
                // 改线宽外轮廓不动，直边为全实像素、四向等宽对称。
                Color fill = ShapeFillColor;
                using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
                    if (fill.A == 255)
                        using (var bb = new SolidBrush(fill)) g.FillPath(bb, path);
                if (bw > 0)
                    HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, bw, borderColor);
                return Rectangle.Inflate(rect, -(bw + 3), -(bw + 3));
            }
            Color flatFill = ShapeFillColor;
            if (flatFill.A == 255)
                using (var bb = new SolidBrush(flatFill)) g.FillRectangle(bb, rect);
            return rect;
        }

        /// <summary>指示符+文字布局：按九宫格返回指示符框与文字框（Left 系列指示符在左，Right 系列在右）。</summary>
        protected static void LayoutIndicator(Rectangle container, int box, Size textSize, int gap,
            ContentAlignment align, out Rectangle indicator, out Rectangle text)
        {
            int packH = Math.Max(box, textSize.Height);
            int totalW = box + (textSize.Width <= 0 ? 0 : gap + textSize.Width);
            Point origin = AlignOrigin(container, new Size(totalW, packH), align);
            // 指示符与文字都在包裹内垂直居中：两者中心恒为 origin.Y + packH/2，
            // 图标放大（box>文字高）时文字不再贴顶偏上
            int boxY = origin.Y + (packH - box) / 2;
            int textY = origin.Y + (packH - textSize.Height) / 2;
            if (!align.ToString().EndsWith("Right"))
            {
                indicator = new Rectangle(origin.X, boxY, box, box);
                text = new Rectangle(origin.X + box + gap, textY, textSize.Width, textSize.Height);
            }
            else
            {
                text = new Rectangle(origin.X, textY, textSize.Width, textSize.Height);
                indicator = new Rectangle(origin.X + totalW - box, boxY, box, box);
            }
        }

        /// <summary>外形内部底色：设置了框内背景色用新色，否则用控件背景色。</summary>
        protected Color ShapeFillColor => _shapeBackColor.IsEmpty ? BackColor : _shapeBackColor;

        /// <summary>按交互态解析底色：默认灰色按钮用 Windows 视觉样式经典蓝灰，自定义底色则明暗推导。</summary>
        private Color ResolveFillColor()
        {
            if (!Enabled) return SystemColors.Control;
            Color fill = ShapeFillColor;
            if (fill == SystemColors.Control)
            {
                if (_pressed) return Color.FromArgb(204, 228, 247);
                if (_hover) return Color.FromArgb(229, 241, 251);
                return fill;
            }
            if (_pressed) return ControlPaint.Dark(fill, 0.06f);
            if (_hover) return ControlPaint.Light(fill, 0.15f);
            return fill;
        }

        /// <summary>圆角时把窗口外形裁剪为圆角区域，直角时还原为矩形；完全异形自绘的派生类可覆盖为空。</summary>
        protected virtual void UpdateRegion()
        {
            if (!IsHandleCreated) return;
            if (_radius <= 0)
            {
                Region = null;
                return;
            }
            // 绘制路径外沿在 0.5..H-1.5，抗锯齿要覆盖到最外一列/行；窗口区域按像素中心
            // 裁剪、曲线路径转 HRgn 时右/下还会再收缩约 1px，故区域右/下外扩到 W+1/H+1
            // （超出窗口的部分会被窗口本身裁掉），保证 1~3px 描边四边虚边都完整。
            using (GraphicsPath path = HDrawPaint.CreatePath(
                    new RectangleF(0, 0, Width + 1f, Height + 1f), _radius, HRoundStyle.All, false))
                Region = new Region(path);
        }
    }
}