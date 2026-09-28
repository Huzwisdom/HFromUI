using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Base
{
    using HFromUI.HLangage;
    /// <summary>
    /// 标签基类：继承 Control 全自绘（同 HBadge 风格），具备 Windows Label 的常用能力
    /// （AutoSize、TextAlign、BorderStyle、&amp; 助记符），并支持圆角背景。
    /// RightToLeft.Yes 时仅显示镜像：水平对齐左右翻转并按 RTL 阅读顺序绘制阿拉伯/希伯来文字。
    /// 圆角通过 Radius 设置：小于等于 0（0 或负数）即直角矩形，不使用枚举或开关。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class HLabelBase : Control
    {
        private ContentAlignment _textAlign = ContentAlignment.TopLeft;
        private BorderStyle _borderStyle = BorderStyle.None;
        private bool _autoSize = true;
        private bool _useMnemonic = true;
        private int _radius;
        private int _borderWidth = 1;
        private Color _shapeBackColor = Color.Empty;

        public HLabelBase()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            Font = new Font("微软雅黑", 9f);
            Size = new Size(100, 23);
            UpdateAutoSize();
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("圆角半径"), HDescriptionLanguage("圆角半径，0 或负数为直角矩形"), Browsable(true)]
        [DefaultValue(0)]
        public int Radius
        {
            get => _radius;
            set { _radius = value; Invalidate(); }
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度（像素），0 或负数不描边；仅 FixedSingle/圆角 Fixed3D 生效，描边向内收半个线宽不缺边"), Browsable(true)]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                UpdateAutoSize();
                Invalidate();
            }
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("框内背景色"), HDescriptionLanguage("圆角/椭圆/胶囊外形内部的背景色；未设置时使用控件背景色。颜色只填充外形内部，圆角外不染色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color ShapeBackColor
        {
            get => _shapeBackColor;
            set { _shapeBackColor = value; Invalidate(); }
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("文字对齐"), HDescriptionLanguage("文字对齐（九宫格）"), Browsable(true)]
        [DefaultValue(ContentAlignment.TopLeft)]
        public ContentAlignment TextAlign
        {
            get => _textAlign;
            set { _textAlign = value; Invalidate(); }
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("自动大小"), HDescriptionLanguage("是否随文字自动调整大小"), Browsable(true)]
        [DefaultValue(true)]
        public override bool AutoSize
        {
            get => _autoSize;
            set
            {
                _autoSize = value;
                // 打开自动尺寸时立即贴合一次；关闭时保留当前尺寸（与 Windows Label 一致）
                if (value) UpdateAutoSize();
                Invalidate();
            }
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("边框样式"), HDescriptionLanguage("边框样式：无边框/单线/三维"), Browsable(true)]
        [DefaultValue(BorderStyle.None)]
        public BorderStyle BorderStyle
        {
            get => _borderStyle;
            set { _borderStyle = value; UpdateAutoSize(); Invalidate(); }
        }

        [HCategoryLanguage("标签基础设置"), HDisplayNameLanguage("启用助记符"), HDescriptionLanguage("是否将 & 字符解释为助记符（按下 Alt+字符时焦点跳到下一控件）"), Browsable(true)]
        [DefaultValue(true)]
        public bool UseMnemonic
        {
            get => _useMnemonic;
            set { _useMnemonic = value; UpdateAutoSize(); Invalidate(); }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            UpdateAutoSize();
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateAutoSize();
            Invalidate();
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            UpdateAutoSize();
            Invalidate();
        }

        /// <summary>RightToLeft 切换（含父控件 Inherit 联动）后按镜像对齐重排重绘。</summary>
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            UpdateAutoSize();
            Invalidate();
        }

        /// <summary>助记符语义同 Windows Label：自身不取焦点，把焦点交给 Tab 序中的下一控件。</summary>
        protected override bool ProcessMnemonic(char charCode)
        {
            if (_useMnemonic && IsMnemonic(charCode, Text) && Parent != null)
            {
                Parent.SelectNextControl(this, true, true, true, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 圆角分支在背景阶段整块铺父容器底色（外扩 1px 防双缓冲黑晕）再填外形底色，
        /// 圆角四角透出父容器；直角分支走系统 GDI 不透明擦除。
        /// 放在 OnPaintBackground（而非 OnPaint），避免双缓冲两阶段半透明合成产生余晕。
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (_radius <= 0)
            {
                base.OnPaintBackground(pevent);
                return;
            }
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            HDrawPaint.FillParentBackdrop(g, Width, Height, Parent?.BackColor ?? SystemColors.Control);
            Color fill = _shapeBackColor.IsEmpty ? BackColor : _shapeBackColor;
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            using (var bg = new SolidBrush(fill))
                g.FillPath(bg, path);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = HDrawPaint.CreateOuterBoxPath(Width, Height, _radius))
            {
                // 裁剪在外形内，防止文字溢出圆角；文字区扣除内边距与边框
                var oldClip = g.Clip;
                g.SetClip(new Region(path), CombineMode.Replace);
                int inset = _borderStyle == BorderStyle.None ? 0
                    : (_borderStyle == BorderStyle.FixedSingle ? Math.Max(1, _borderWidth) : 2);
                // 文字区只扣除内边距与边框，不手缩像素：DrawText 的 overhang 衬距由其内部处理，
                // 控件宽度在 UpdateAutoSize 中按同一 TextRenderer 口径测量，二者必须同源
                // （曾用 GDI TextAdvance 严格宽定宽 + DrawText 绘制，CJK 排版宽大于严格宽，
                // 实测“中/Hg”被横向挤削 1~2px、首字发淡）
                var textRect = new Rectangle(
                    Padding.Left + inset, Padding.Top + inset,
                    Math.Max(0, Width - Padding.Horizontal - inset * 2),
                    Math.Max(0, Height - Padding.Vertical - inset * 2));
                TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, BuildTextFlags());
                g.Clip = oldClip;

                DrawBorder(g, rect);
            }
        }

        /// <summary>按边框样式描边：单线/圆角三维沿内缩路径绘制（粗边完整不缺边）；直角三维用原生立体边。</summary>
        private void DrawBorder(Graphics g, Rectangle rect)
        {
            switch (_borderStyle)
            {
                case BorderStyle.FixedSingle:
                    DrawRoundedFrame(g, SystemColors.ControlDark);
                    break;
                case BorderStyle.Fixed3D:
                    if (_radius > 0) DrawRoundedFrame(g, SystemColors.ControlDark);
                    else ControlPaint.DrawBorder3D(g, rect, Border3DStyle.Sunken);
                    break;
            }
        }

        /// <summary>圆角用向内填充环（外轮廓固定像素盒、改线宽外沿不动）；直角保留内缩居中笔。</summary>
        private void DrawRoundedFrame(Graphics g, Color color)
        {
            if (_borderWidth <= 0) return;
            if (_radius > 0)
            {
                HDrawPaint.FillRoundedBorder(g, Width, Height, _radius, _borderWidth, color);
                return;
            }
            using (GraphicsPath path = HDrawPaint.CreateFramePath(Width, Height, 0, _borderWidth))
            using (var pen = new Pen(color, _borderWidth))
                g.DrawPath(pen, path);
        }

        /// <summary>
        /// AutoSize 时按文字实际显示尺寸调整控件大小：测量与绘制必须同为 TextRenderer.DrawText
        /// 口径（含其内部 overhang 衬距，同 Windows 原生 Label），控件尺寸直接取该测量值再计入
        /// 边框内缩与内边距——绘制矩形永远不小于排版矩形，CJK/斜体/弯引号等字形都不会被挤削。
        /// 切勿改用 GDI TextAdvance 严格步进宽定宽：DrawTextEx 对 CJK 等字符的排版宽明显大于
        /// 严格步进宽（微软雅黑 9pt“中”实测 20px vs 12px），混用即出现横向削字。
        /// </summary>
        private void UpdateAutoSize()
        {
            if (!_autoSize) return;
            Size = CalcAutoSize();
        }

        /// <summary>
        /// 按当前字体/DPI 与文字量计算 AutoSize 期望尺寸（与 UpdateAutoSize、GetPreferredSize 同源）。
        /// </summary>
        private Size CalcAutoSize()
        {
            string shown = _useMnemonic ? StripMnemonic(Text ?? string.Empty) : (Text ?? string.Empty);
            // 宽度按实际显示文本量；空文本也给一个字高宽位（同原生 Label）
            Size ts = TextRenderer.MeasureText(shown.Length == 0 ? " " : shown, Font, Size.Empty,
                BuildMeasureFlags());
            // 高度取“Ag中”统一行高（含拉丁大写/基线/全角），不随具体文本变化
            int textH = TextRenderer.MeasureText(HTranslation.GetContent("Ag中"), Font, Size.Empty,
                BuildMeasureFlags()).Height;
            int inset = _borderStyle == BorderStyle.None ? 0
                : (_borderStyle == BorderStyle.FixedSingle ? Math.Max(1, _borderWidth) : 2);
            return new Size(
                ts.Width + Padding.Horizontal + inset * 2,
                textH + Padding.Vertical + inset * 2);
        }

        /// <summary>
        /// 窗体自动缩放（AutoScaleMode.Font/Dpi：设计器在 144DPI 下布局、进程在 96DPI 虚拟化
        /// 运行时会按约 0.67 因子乘算全部子控件边界）只缩边界、不按文字重新排版，基类缩放后
        /// AutoSize 标签的尺寸会小于文字实际宽度而被横向削字。缩放完成后按当前字体/DPI 重新
        /// 测量贴合一次；AutoSize 关闭时保留用户/缩放后的固定尺寸（超长走省略号）。
        /// </summary>
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            if (_autoSize) UpdateAutoSize();
        }

        /// <summary>AutoSize 时向布局引擎（FlowLayoutPanel/TableLayoutPanel 等）报告文字期望尺寸。</summary>
        public override Size GetPreferredSize(Size proposedSize)
        {
            return _autoSize ? CalcAutoSize() : base.GetPreferredSize(proposedSize);
        }

        /// <summary>AutoSize 测量标志：与 BuildTextFlags 的单行绘制口径一致（对齐/换行不影响无限宽单行测量）。</summary>
        private TextFormatFlags BuildMeasureFlags()
        {
            var flags = TextFormatFlags.SingleLine;
            if (!_useMnemonic) flags |= TextFormatFlags.NoPrefix;
            return flags;
        }

        /// <summary>按 Windows Label 语义去掉助记符标记：单个 &amp; 跳过不显示，连续两个 &amp; 显示为一个。</summary>
        private static string StripMnemonic(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '&' && i + 1 < s.Length) sb.Append(s[++i]);
                else sb.Append(s[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 九宫格对齐与换行/省略号/助记符标志（非自动尺寸时允许换行并以省略号截断）。
        /// 使用 DrawText 默认 overhang 衬距（不加 NoPadding），与 UpdateAutoSize 的 MeasureText
        /// 同口径；RightToLeft.Yes 时水平对齐镜像（左↔右）并附加 RTL 阅读顺序，垂直方向不变。
        /// </summary>
        private TextFormatFlags BuildTextFlags()
        {
            var flags = TextFormatFlags.WordBreak;
            bool rtl = RightToLeft == RightToLeft.Yes;
            string a = _textAlign.ToString();
            // RTL 时水平语义镜像：逻辑左对齐视觉靠右、逻辑右对齐视觉靠左、居中不变
            bool visualLeft = !rtl && a.EndsWith("Left") || rtl && a.EndsWith("Right");
            bool visualRight = !rtl && a.EndsWith("Right") || rtl && a.EndsWith("Left");
            if (visualLeft) flags |= TextFormatFlags.Left;
            else if (visualRight) flags |= TextFormatFlags.Right;
            else flags |= TextFormatFlags.HorizontalCenter;
            if (a.StartsWith("Top")) flags |= TextFormatFlags.Top;
            else if (a.StartsWith("Bottom")) flags |= TextFormatFlags.Bottom;
            else flags |= TextFormatFlags.VerticalCenter;

            if (_autoSize)
                flags |= TextFormatFlags.SingleLine;
            else
                flags |= TextFormatFlags.EndEllipsis;
            if (!_useMnemonic)
                flags |= TextFormatFlags.NoPrefix;
            // 实测 Left/Center/Right 三档与 RightToLeft 组合均合法，RTL 下一律声明 RTL 阅读顺序，
            // 保证阿拉伯/希伯来文字在任何对齐档位都能双向排版
            if (rtl)
                flags |= TextFormatFlags.RightToLeft;
            return flags;
        }
    }
}