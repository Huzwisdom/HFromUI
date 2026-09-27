using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;
namespace HFromUI.HControl.Tools.Button
{
    using HFromUI.HControl.Base;
    using HFromUI.HControl.Chart.Tips;
    /// <summary>
    /// 按钮外观引擎：122 种预设风格、Custom 三态色、矢量 Glyph/图片、形状、悬停混色动画与悬停气泡。
    /// HPushButton 与 HToggleButton 都直接继承 HButtonBase，以组合方式各持一个本引擎实例，外观逻辑只此一份。
    /// </summary>
    internal sealed class HPushButtonEngine : IDisposable
    {
        #region 字段
        private readonly HButtonBase _owner;
        private HPushButtonShape _shape = HPushButtonShape.Round;
        private HPushButtonStyle _buttonStyle = HPushButtonStyle.Primary;
        private Image _image;
        private int _imageSize = 16;
        private int _imageTextGap = 4;
        private Image _hoverImage;
        private Image _checkedImage;
        private HPushButtonGlyph _glyph = HPushButtonGlyph.None;
        private HPushButtonGlyph _checkedGlyph = HPushButtonGlyph.None;
        // Custom 三态色（Empty = 自动推导）
        private Color _normalBack = Color.Empty, _normalBorder = Color.Empty, _normalFore = Color.Empty;
        private Color _hoverBack = Color.Empty, _hoverBorder = Color.Empty, _hoverFore = Color.Empty;
        private Color _pressBack = Color.Empty, _pressBorder = Color.Empty, _pressFore = Color.Empty;
        private string _tipText = string.Empty;
        private HTipSchemeKind _tipScheme = HTipSchemeKind.Auto;
        private bool _hover;
        private float _hoverMix;
        private Timer _animTimer;
        private Timer _tipDelay;
        private HPushButtonTip _tip;
        #endregion
        public HPushButtonEngine(HButtonBase owner)
        {
            _owner = owner;
            _owner.EnabledChanged += (s, e) =>
            {
                if (!_owner.Enabled) { HideTip(); _hover = false; }
            };
        }
        /// <summary>保持按下着色态：HToggleButton 同步自己的 Checked，HPushButton 恒为假。</summary>
        public bool IsCheckedDown { get; private set; }
        /// <summary>同步保持按下状态并立即启动混色动画。</summary>
        public void SetChecked(bool value)
        {
            IsCheckedDown = value;
            StartAnimation();
        }
        #region 外观状态
        /// <summary>外框形状：直角/圆角/胶囊/正圆。</summary>
        public HPushButtonShape Shape { get => _shape; set { _shape = value; _owner.Invalidate(); } }
        /// <summary>预设风格（122 种：30 色 × 实心/描边/立体/渐变），Custom 时使用自定义三态色。</summary>
        public HPushButtonStyle ButtonStyle { get => _buttonStyle; set { _buttonStyle = value; _owner.Invalidate(); } }
        /// <summary>常规状态图片（矢量 Glyph 优先于它；另有悬停/选中专用图片）。</summary>
        public Image Image { get => _image; set { _image = value; _owner.Invalidate(); } }
        /// <summary>悬停时显示的图片（缺省回落到 Image）。</summary>
        public Image HoverImage { get => _hoverImage; set { _hoverImage = value; _owner.Invalidate(); } }
        /// <summary>保持按下时显示的图片（双态按钮载体，如开始/暂停）。</summary>
        public Image CheckedImage { get => _checkedImage; set { _checkedImage = value; _owner.Invalidate(); } }
        /// <summary>内置矢量图标（129 种，颜色跟随文字色）；设置后优先于 Image。</summary>
        public HPushButtonGlyph Glyph { get => _glyph; set { _glyph = value; _owner.Invalidate(); } }
        /// <summary>保持按下后切换显示的矢量图标（如 播放↔暂停）。</summary>
        public HPushButtonGlyph CheckedGlyph { get => _checkedGlyph; set { _checkedGlyph = value; _owner.Invalidate(); } }
        /// <summary>图标边长（正方形）。</summary>
        public int ImageSize { get => _imageSize; set { _imageSize = Math.Max(1, value); _owner.Invalidate(); } }
        /// <summary>图文间距。</summary>
        public int ImageTextGap { get => _imageTextGap; set { _imageTextGap = Math.Max(0, value); _owner.Invalidate(); } }
        /// <summary>常规底色（空为自动浅灰）。</summary>
        public Color NormalBackColor { get => _normalBack; set { _normalBack = value; _owner.Invalidate(); } }
        /// <summary>常规边色（空为同底色）。</summary>
        public Color NormalBorderColor { get => _normalBorder; set { _normalBorder = value; _owner.Invalidate(); } }
        /// <summary>常规字色（空为按底色亮度自动黑白）。</summary>
        public Color NormalForeColor { get => _normalFore; set { _normalFore = value; _owner.Invalidate(); } }
        /// <summary>悬停底色（空为常规色自动推导）。</summary>
        public Color HoverBackColor { get => _hoverBack; set { _hoverBack = value; _owner.Invalidate(); } }
        /// <summary>悬停边色（空为自动推导）。</summary>
        public Color HoverBorderColor { get => _hoverBorder; set { _hoverBorder = value; _owner.Invalidate(); } }
        /// <summary>悬停字色（空为常规字色）。</summary>
        public Color HoverForeColor { get => _hoverFore; set { _hoverFore = value; _owner.Invalidate(); } }
        /// <summary>按下底色（空为常规色自动压暗）。</summary>
        public Color PressedBackColor { get => _pressBack; set { _pressBack = value; _owner.Invalidate(); } }
        /// <summary>按下边色（空为自动推导）。</summary>
        public Color PressedBorderColor { get => _pressBorder; set { _pressBorder = value; _owner.Invalidate(); } }
        /// <summary>按下字色（空为常规字色）。</summary>
        public Color PressedForeColor { get => _pressFore; set { _pressFore = value; _owner.Invalidate(); } }
        /// <summary>悬停气泡文字（空则不显示）。</summary>
        public string TipText { get => _tipText; set => _tipText = value ?? string.Empty; }
        /// <summary>悬停气泡配色（Chart 气泡体系）。</summary>
        public HTipSchemeKind TipScheme { get => _tipScheme; set => _tipScheme = value; }
        #endregion
        #region 交互（由宿主控件的鼠标重写转发进入）
        /// <summary>鼠标进入：启动悬停混色，安排气泡。</summary>
        public void Enter()
        {
            _hover = true;
            StartAnimation();
            if (_tipText.Length > 0 && _owner.Visible) ScheduleTip();
        }
        /// <summary>鼠标离开：收气泡，悬停混色回零。</summary>
        public void Leave()
        {
            HideTip();
            _hover = false;
            StartAnimation();
        }
        /// <summary>鼠标按下：收气泡（按下配色由宿主传入 Paint）。</summary>
        public void Press() => HideTip();
        #endregion
        #region 动画与提示窗
        private void StartAnimation()
        {
            if (_animTimer == null)
            {
                _animTimer = new Timer { Interval = 15 };
                _animTimer.Tick += AnimTick;
            }
            _animTimer.Start();
        }
        private void AnimTick(object sender, EventArgs e)
        {
            float hoverTarget = _hover && _owner.Enabled ? 1f : 0f;
            bool done = Math.Abs(_hoverMix - hoverTarget) <= 0.01f;
            if (!done) _hoverMix += (hoverTarget - _hoverMix) * 0.25f;
            else _hoverMix = hoverTarget;
            _owner.Invalidate();
            if (done) _animTimer.Stop();
        }
        private void ScheduleTip()
        {
            if (_tipDelay == null)
            {
                _tipDelay = new Timer { Interval = 350 };
                _tipDelay.Tick += (s, e) =>
                {
                    _tipDelay.Stop();
                    if (_hover && _tipText.Length > 0) ShowTip();
                };
            }
            _tipDelay.Stop();
            _tipDelay.Start();
        }
        private void ShowTip()
        {
            if (_tip == null) _tip = new HPushButtonTip();
            _tip.ShowFor(_owner, _tipText, _tipScheme);
        }
        private void HideTip()
        {
            _tipDelay?.Stop();
            _tip?.Hide();
        }
        /// <summary>释放动画/延时定时器与气泡窗（宿主 Dispose 时调用）。</summary>
        public void Dispose()
        {
            _animTimer?.Stop();
            _animTimer?.Dispose();
            _tipDelay?.Stop();
            _tipDelay?.Dispose();
            _tip?.Close();
            _tip?.Dispose();
        }
        #endregion
        #region 绘制
        /// <summary>按钮整体外观（宿主 OnPaint 调用；焦点虚框由宿主另行调用 PaintFocus）。</summary>
        public void Paint(Graphics g, bool pressed)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            PaintButton(g, pressed);
        }
        /// <summary>沿当前形状内缩 3px 的虚线焦点框（宿主判断 Focused/ShowFocusCues 后调用）。</summary>
        public void PaintFocus(Graphics g)
        {
            const int inset = 3;
            using (var pen = new Pen(SystemColors.ControlDark, 1f) { DashStyle = DashStyle.Dot })
            {
                if (_shape == HPushButtonShape.Circle)
                    g.DrawEllipse(pen, inset, inset, _owner.Width - inset * 2 - 1, _owner.Height - inset * 2 - 1);
                else
                {
                    float r = Math.Max(0, ShapeRadius(_owner.Width, _owner.Height) - inset);
                    using (GraphicsPath frame = HDrawPaint.CreateFramePath(
                            _owner.Width - inset * 2, _owner.Height - inset * 2, r, 1))
                    {
                        frame.Transform(new System.Drawing.Drawing2D.Matrix(1, 0, 0, 1, inset, inset));
                        g.DrawPath(pen, frame);
                    }
                }
            }
        }
        /// <summary>
        /// 不透明底色只允许落在按钮形状内，圆角/异形四角回退为父级底色，
        /// 杜绝形状外再套一个直角矩形色块。透明背景由宿主走基类机制，不在此处理。
        /// </summary>
        public void PaintBackground(Graphics g)
        {
            Color parentBack = _owner.Parent?.BackColor ?? SystemColors.Control;
            using (var pb = new SolidBrush(parentBack))
                g.FillRectangle(pb, _owner.ClientRectangle);
            using (var path = BuildShapePath(_owner.Width, _owner.Height))
            using (var bb = new SolidBrush(_owner.BackColor))
                g.FillPath(bb, path);
        }
        /// <summary>普通按钮 / 保持按下：形状底 + 边框 + 图文（按下整体下沉 1px）。</summary>
        private void PaintButton(Graphics g, bool pressed)
        {
            var mode = HPushButtonScheme.Mode(_buttonStyle);
            var p0 = ResolvePalette(false, false);
            var pH = ResolvePalette(true, false);
            var pP = ResolvePalette(false, true);
            var pal = LerpPalette(p0, pH, _hoverMix);
            if (pressed || IsCheckedDown) pal = pP;
            Rectangle rect = _owner.ClientRectangle;
            using (GraphicsPath path = BuildShapePath(rect.Width, rect.Height))
            {
                if (pal.Back != Color.Empty)
                {
                    if (mode == HPushButtonMode.Gradient)
                    {
                        // 纵向渐变（上基准色 → 下端压暗色），裁剪在按钮外形内
                        var oldClip = g.Clip;
                        using (var region = new Region(path))
                        using (var brush = new LinearGradientBrush(rect, pal.Back,
                            HPushButtonScheme.GradientEnd(pal.Back), 90f))
                        {
                            g.SetClip(region, CombineMode.Replace);
                            g.FillRectangle(brush, rect);
                        }
                        g.Clip = oldClip;
                        oldClip.Dispose();
                    }
                    else
                    {
                        using (var brush = new SolidBrush(pal.Back)) g.FillPath(brush, path);
                    }
                }
                if (mode == HPushButtonMode.ThreeD)
                {
                    // 底部高光带（裁剪在按钮外路径内），按下时变薄并随内容下沉
                    int band = pressed ? 2 : 4;
                    var oldClip = g.Clip;
                    using (var region = new Region(path))
                    {
                        g.SetClip(region, CombineMode.Replace);
                        using (var bb = new SolidBrush(HPushButtonScheme.Shade(pal.Back, 0.18f)))
                            g.FillRectangle(bb, 0, rect.Bottom - band, rect.Width, band);
                    }
                    g.Clip = oldClip;
                    oldClip.Dispose();
                }
                if (_owner.BorderWidth > 0 && mode != HPushButtonMode.Link && pal.Border != Color.Empty)
                {
                    // 边框环：外轮廓固定、厚度全部向内，四向抗锯齿覆盖一致
                    using (GraphicsPath inner = BuildInnerPath(rect.Width, rect.Height, _owner.BorderWidth))
                        HDrawPaint.FillBorderRing(g, path, inner, pal.Border);
                }
            }
            int dy = pressed ? 1 : 0;
            DrawImageAndText(g, rect, pal.Fore, dy, mode == HPushButtonMode.Link && _hover);
        }
        #endregion
        #region 图文与矢量图标
        /// <summary>绘制图文组合（矢量 Glyph 优先；否则取当前状态图：保持按下图 > 悬停图 > 常规图）。</summary>
        private void DrawImageAndText(Graphics g, Rectangle rect, Color fore, int dy, bool underline)
        {
            HPushButtonGlyph glyph = _glyph;
            if (glyph != HPushButtonGlyph.None && IsCheckedDown && _checkedGlyph != HPushButtonGlyph.None)
                glyph = _checkedGlyph;
            Image img = glyph != HPushButtonGlyph.None ? null : _image;
            if (img != null)
            {
                if (IsCheckedDown && _checkedImage != null) img = _checkedImage;
                else if (_hover && _hoverImage != null) img = _hoverImage;
            }
            Size textSize = TextRenderer.MeasureText(g, _owner.Text, _owner.Font);
            Size imgSize = glyph != HPushButtonGlyph.None || img != null ? new Size(_imageSize, _imageSize) : Size.Empty;
            int gap = imgSize != Size.Empty && _owner.Text.Length > 0 ? _imageTextGap : 0;
            var total = new Size(imgSize.Width + gap + textSize.Width, Math.Max(imgSize.Height, textSize.Height));
            Point p = HButtonBase.AlignOrigin(rect, total, _owner.TextAlign);
            p.Offset(0, dy);
            if (glyph != HPushButtonGlyph.None)
                DrawGlyph(g, new Rectangle(p.X, p.Y + (total.Height - imgSize.Height) / 2, imgSize.Width, imgSize.Height), glyph, fore);
            else if (img != null)
                g.DrawImage(img, new Rectangle(p.X, p.Y + (total.Height - imgSize.Height) / 2, imgSize.Width, imgSize.Height),
                    0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
            if (_owner.Text.Length > 0)
            {
                int tx = p.X + imgSize.Width + gap;
                var textRect = new Rectangle(tx, p.Y + (total.Height - textSize.Height) / 2, textSize.Width + 1, textSize.Height);
                TextRenderer.DrawText(g, _owner.Text, _owner.Font, textRect, fore,
                    HButtonBase.TextFormatFromAlign(_owner.TextAlign) | TextFormatFlags.EndEllipsis);
                if (underline)
                {
                    using (var pen = new Pen(fore, 1f))
                        g.DrawLine(pen, textRect.X, textRect.Bottom - 2, textRect.Right - 1, textRect.Bottom - 2);
                }
            }
        }
        /// <summary>形状对应的圆角半径（正圆/胶囊取半宽高）。</summary>
        private float ShapeRadius(float width, float height)
        {
            switch (_shape)
            {
                case HPushButtonShape.Capsule:
                case HPushButtonShape.Circle:
                    return Math.Min(width, height) / 2f;
                case HPushButtonShape.Rect:
                    return 0;
                default:
                    return Math.Min(_owner.Radius, Math.Min(width, height) / 2f);
            }
        }
        /// <summary>
        /// 按形状构造外轮廓路径，对齐控件像素盒 (-0.5,-0.5)-(W-0.5,H-0.5)：
        /// 直边填充实像素、无虚边，四向对称。底色填充、形状裁剪共用此路径；
        /// 边框环在此轮廓之内填充，改线宽外轮廓不动。
        /// </summary>
        private GraphicsPath BuildShapePath(float width, float height)
        {
            var box = new RectangleF(-0.5f, -0.5f, width, height);
            if (_shape == HPushButtonShape.Circle)
            {
                var p = new GraphicsPath();
                p.AddEllipse(box);
                return p;
            }
            if (_shape == HPushButtonShape.Rect)
            {
                var p = new GraphicsPath();
                p.AddRectangle(box);
                return p;
            }
            return HDrawPaint.CreatePath(box, ShapeRadius(width, height) + 0.5f, HRoundStyle.All, false);
        }

        /// <summary>
        /// 构造边框环内轮廓：像素盒 (bw-0.5,..)，相对外路径等距内缩 bw
        /// （圆角/圆弧半径同步减）。内缩后无正面积时返回 null。
        /// </summary>
        private GraphicsPath BuildInnerPath(float width, float height, float bw)
        {
            var inner = new RectangleF(bw - 0.5f, bw - 0.5f, width - bw * 2f, height - bw * 2f);
            if (inner.Width <= 0 || inner.Height <= 0) return null;
            if (_shape == HPushButtonShape.Circle)
            {
                var p = new GraphicsPath();
                p.AddEllipse(inner);
                return p;
            }
            if (_shape == HPushButtonShape.Rect)
            {
                var p = new GraphicsPath();
                p.AddRectangle(inner);
                return p;
            }
            return HDrawPaint.CreatePath(inner, ShapeRadius(width, height) + 0.5f - bw, HRoundStyle.All, false);
        }
        /// <summary>
        /// 矢量图标绘制：全部基于 24 单位盒，按目标矩形等比缩放，圆角笔锋；
        /// 线性图标描边、媒体/图表类实心，颜色随文字三态色。
        /// </summary>
        private static void DrawGlyph(Graphics g, Rectangle r, HPushButtonGlyph glyph, Color color)
        {
            if (r.Width <= 0 || r.Height <= 0 || glyph == HPushButtonGlyph.None) return;
            var oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float sx = r.Width / 24f, sy = r.Height / 24f, u = Math.Min(sx, sy);
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(color, 1.75f * u) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                PointF P(float x, float y) => new PointF(r.X + x * sx, r.Y + y * sy);
                RectangleF B(float x, float y, float w, float h) => new RectangleF(r.X + x * sx, r.Y + y * sy, w * sx, h * sy);
                void L(float x1, float y1, float x2, float y2) => g.DrawLine(pen, P(x1, y1), P(x2, y2));
                void S(params float[] c)
                {
                    var pts = new PointF[c.Length / 2];
                    for (int i = 0; i < pts.Length; i++) pts[i] = P(c[i * 2], c[i * 2 + 1]);
                    g.DrawLines(pen, pts);
                }
                void FP(params float[] c)
                {
                    var pts = new PointF[c.Length / 2];
                    for (int i = 0; i < pts.Length; i++) pts[i] = P(c[i * 2], c[i * 2 + 1]);
                    g.FillPolygon(brush, pts);
                }
                void Ring(float cx, float cy, float d) => g.DrawEllipse(pen, B(cx - d / 2, cy - d / 2, d, d));
                void Dot(float cx, float cy, float d) => g.FillEllipse(brush, B(cx - d / 2, cy - d / 2, d, d));
                void Arc(float x, float y, float w, float h, float start, float sweep) => g.DrawArc(pen, B(x, y, w, h), start, sweep);
                void RR(float x, float y, float w, float h, float rad, bool fill = false)
                {
                    using (var p = HDrawPaint.CreatePath(B(x, y, w, h), rad * u, HRoundStyle.All, false))
                    {
                        if (fill) g.FillPath(brush, p);
                        else g.DrawPath(pen, p);
                    }
                }
                void Txt(string s, float cx, float cy, float em)
                {
                    using (var f = new Font("Segoe UI", em * u, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        var sz = TextRenderer.MeasureText(s, f);
                        TextRenderer.DrawText(g, s, f,
                            new Point((int)(r.X + cx * sx - sz.Width / 2f), (int)(r.Y + cy * sy - sz.Height / 2f)),
                            color, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    }
                }
                switch (glyph)
                {
                    case HPushButtonGlyph.ArrowLeft: L(19, 12, 5, 12); S(12, 6, 5, 12, 12, 18); break;
                    case HPushButtonGlyph.ArrowRight: L(5, 12, 19, 12); S(12, 6, 19, 12, 12, 18); break;
                    case HPushButtonGlyph.ArrowUp: L(12, 19, 12, 5); S(6, 12, 12, 5, 18, 12); break;
                    case HPushButtonGlyph.ArrowDown: L(12, 5, 12, 19); S(6, 12, 12, 19, 18, 12); break;
                    case HPushButtonGlyph.Play: FP(7, 5, 7, 19, 19, 12); break;
                    case HPushButtonGlyph.Pause: RR(6.5f, 5, 3.8f, 14, 1, true); RR(13.7f, 5, 3.8f, 14, 1, true); break;
                    case HPushButtonGlyph.Stop: RR(6.5f, 6.5f, 11, 11, 2, true); break;
                    case HPushButtonGlyph.Check: S(4, 12.5f, 9.5f, 18, 20, 6); break;
                    case HPushButtonGlyph.Save:
                        RR(4, 3.5f, 16, 17, 1.5f); S(8.5f, 3.5f, 8.5f, 8, 15.5f, 8, 15.5f, 3.5f); RR(8, 13, 8, 7.5f, 1);
                        break;
                    case HPushButtonGlyph.ChevronLeft: S(15, 6, 9, 12, 15, 18); break;
                    case HPushButtonGlyph.ChevronRight: S(9, 6, 15, 12, 9, 18); break;
                    case HPushButtonGlyph.ChevronUp: S(6, 15, 12, 9, 18, 15); break;
                    case HPushButtonGlyph.ChevronDown: S(6, 9, 12, 15, 18, 9); break;
                    case HPushButtonGlyph.ChevronsLeft: S(11, 17, 6, 12, 11, 7); S(18, 17, 13, 12, 18, 7); break;
                    case HPushButtonGlyph.ChevronsRight: S(13, 7, 18, 12, 13, 17); S(6, 7, 11, 12, 6, 17); break;
                    case HPushButtonGlyph.ChevronsUp: S(17, 11, 12, 6, 7, 11); S(17, 17, 12, 12, 7, 17); break;
                    case HPushButtonGlyph.ChevronsDown: S(7, 13, 12, 18, 17, 13); S(7, 7, 12, 12, 17, 7); break;
                    case HPushButtonGlyph.ArrowUpRight: L(7.5f, 16.5f, 16.5f, 7.5f); S(10, 7.5f, 16.5f, 7.5f, 16.5f, 14); break;
                    case HPushButtonGlyph.ArrowDownLeft: L(16.5f, 7.5f, 7.5f, 16.5f); S(14, 16.5f, 7.5f, 16.5f, 7.5f, 10); break;
                    case HPushButtonGlyph.Login:
                        RR(10, 3, 11, 18, 2); S(10, 17, 15, 12, 10, 7); L(15, 12, 3, 12);
                        break;
                    case HPushButtonGlyph.Logout:
                        RR(3, 3, 11, 18, 2); S(14, 17, 19, 12, 14, 7); L(9, 12, 19, 12);
                        break;
                    case HPushButtonGlyph.Undo:
                        S(9, 14, 4, 9, 9, 4);
                        g.DrawBezier(pen, P(4, 9), P(13, 8.5f), P(18, 12.5f), P(17, 18.5f));
                        break;
                    case HPushButtonGlyph.Redo:
                        S(15, 14, 20, 9, 15, 4);
                        g.DrawBezier(pen, P(20, 9), P(11, 8.5f), P(6, 12.5f), P(7, 18.5f));
                        break;
                    case HPushButtonGlyph.Refresh:
                        S(15, 8, 21, 8, 21, 2); Arc(3, 3, 18, 18, 228, 105);
                        S(9, 16, 3, 16, 3, 22); Arc(3, 3, 18, 18, 48, 105);
                        break;
                    // 250° 缺角环，缺口在顶部；箭头在弧线起点且两翼平分切线反方向（45°）
                    case HPushButtonGlyph.RotateCw: S(13.8f, 5.6f, 18.4f, 5.6f, 18.4f, 1f); Arc(3, 3, 18, 18, 315, 250); break;
                    case HPushButtonGlyph.RotateCcw: S(10.2f, 5.6f, 5.6f, 5.6f, 5.6f, 1f); Arc(3, 3, 18, 18, 225, -250); break;
                    case HPushButtonGlyph.Move:
                        S(5, 9, 2, 12, 5, 15); S(9, 5, 12, 2, 15, 5);
                        S(9, 19, 12, 22, 15, 19); S(19, 9, 22, 12, 19, 15);
                        L(2, 12, 22, 12); L(12, 2, 12, 22);
                        break;
                    case HPushButtonGlyph.Expand:
                        S(8, 3, 3, 3, 3, 8); S(16, 3, 21, 3, 21, 8);
                        S(3, 16, 3, 21, 8, 21); S(21, 16, 21, 21, 16, 21);
                        break;
                    case HPushButtonGlyph.Shrink:
                        S(8, 3, 8, 8, 3, 8); S(16, 3, 16, 8, 21, 8);
                        S(3, 16, 8, 16, 8, 21); S(21, 16, 16, 16, 16, 21);
                        break;
                    case HPushButtonGlyph.ExternalLink:
                        RR(4, 7, 13, 13, 2); L(12, 12, 21, 3); S(15, 3, 21, 3, 21, 9);
                        break;
                    case HPushButtonGlyph.SortAsc:
                        L(7, 20, 7, 4); S(3, 8, 7, 4, 11, 8);
                        L(14, 8, 21, 8); L(14, 12, 21, 12); L(14, 16, 19, 16); L(14, 20, 17, 20);
                        break;
                    case HPushButtonGlyph.SortDesc:
                        L(7, 4, 7, 20); S(3, 16, 7, 20, 11, 16);
                        L(14, 4, 21, 4); L(14, 8, 21, 8); L(14, 12, 19, 12); L(14, 16, 17, 16);
                        break;
                    case HPushButtonGlyph.Filter:
                        S(22, 3, 2, 3, 10, 12.5f, 10, 19, 14, 21, 14, 12.5f, 22, 3);
                        break;
                    case HPushButtonGlyph.Shuffle:
                        S(16, 3, 21, 3, 21, 8); L(4, 20, 20, 4);
                        S(21, 16, 21, 21, 16, 21); L(15, 15, 21, 21); L(4, 4, 9, 9);
                        break;
                    case HPushButtonGlyph.Repeat:
                        L(7, 6, 21, 6); S(17, 2, 21, 6, 17, 10);
                        L(17, 18, 3, 18); S(7, 14, 3, 18, 7, 22);
                        break;
                    case HPushButtonGlyph.Menu: L(3, 6, 21, 6); L(3, 12, 21, 12); L(3, 18, 21, 18); break;
                    case HPushButtonGlyph.SkipBack: FP(18, 5, 9, 12, 18, 19); L(6, 5, 6, 19); break;
                    case HPushButtonGlyph.SkipForward: FP(6, 5, 15, 12, 6, 19); L(18, 5, 18, 19); break;
                    case HPushButtonGlyph.FastForward: FP(5, 5, 12, 12, 5, 19); FP(13, 5, 21, 12, 13, 19); break;
                    case HPushButtonGlyph.Rewind: FP(19, 5, 12, 12, 19, 19); FP(11, 5, 3, 12, 11, 19); break;
                    case HPushButtonGlyph.Eject: FP(4, 14.5f, 12, 5.5f, 20, 14.5f); RR(4, 17, 16, 2.5f, 1, true); break;
                    case HPushButtonGlyph.Record: Dot(12, 12, 13); break;
                    case HPushButtonGlyph.Volume:
                        S(11, 5, 6, 9, 2.5f, 9, 2.5f, 15, 6, 15, 11, 19, 11, 5);
                        Arc(13, 8.5f, 4, 7, 270, 180); Arc(14.5f, 6, 7, 12, 270, 180);
                        break;
                    case HPushButtonGlyph.VolumeLow:
                        S(11, 5, 6, 9, 2.5f, 9, 2.5f, 15, 6, 15, 11, 19, 11, 5);
                        Arc(13, 8.5f, 4, 7, 270, 180);
                        break;
                    case HPushButtonGlyph.VolumeMute:
                        S(11, 5, 6, 9, 2.5f, 9, 2.5f, 15, 6, 15, 11, 19, 11, 5);
                        L(15.5f, 9.5f, 21.5f, 15.5f); L(21.5f, 9.5f, 15.5f, 15.5f);
                        break;
                    case HPushButtonGlyph.Microphone:
                        RR(9, 2.5f, 6, 11, 3); Arc(6, 9, 12, 10, 0, 180);
                        L(12, 19, 12, 22); L(8.5f, 22, 15.5f, 22);
                        break;
                    case HPushButtonGlyph.Camera:
                        RR(2, 7, 20, 13, 2); S(9.3f, 7, 10.3f, 4, 13.7f, 4, 14.7f, 7);
                        L(10.3f, 4, 13.7f, 4); Ring(12, 13.5f, 6);
                        break;
                    case HPushButtonGlyph.Video: RR(2.5f, 6, 13.5f, 12, 2); FP(16, 8.5f, 22, 12, 16, 15.5f); break;
                    case HPushButtonGlyph.Film:
                        RR(3, 3, 18, 18, 2); L(3, 9, 21, 9); L(3, 15, 21, 15); L(8, 3, 8, 21); L(16, 3, 16, 21);
                        break;
                    case HPushButtonGlyph.Picture:
                        RR(3, 4, 18, 16, 2); Ring(9, 9.5f, 3.2f); S(4, 18, 9, 12, 13, 16.5f, 17, 12, 20, 16.5f);
                        break;
                    case HPushButtonGlyph.Music:
                        L(9, 18, 9, 5); L(9, 5, 21, 3); L(21, 3, 21, 16);
                        Ring(9, 18, 3.5f); Ring(21, 16, 3.5f);
                        break;
                    case HPushButtonGlyph.Bell:
                        Arc(6, 3, 12, 12, 180, 180); L(6, 9, 3.5f, 17.5f); L(18, 9, 20.5f, 17.5f);
                        L(3.5f, 17.5f, 20.5f, 17.5f); Arc(9.5f, 17.5f, 5, 4, 0, 180);
                        break;
                    case HPushButtonGlyph.BellOff:
                        Arc(6, 3, 12, 12, 180, 180); L(6, 9, 4, 17); L(18, 9, 20, 17); L(4, 17, 14, 17);
                        L(3.5f, 3.5f, 20.5f, 20.5f);
                        break;
                    case HPushButtonGlyph.X: S(6, 6, 18, 18); S(18, 6, 6, 18); break;
                    case HPushButtonGlyph.Plus: L(12, 5, 12, 19); L(5, 12, 19, 12); break;
                    case HPushButtonGlyph.Minus: L(5, 12, 19, 12); break;
                    case HPushButtonGlyph.Search: Ring(10.5f, 10.5f, 12); L(15, 15, 20.5f, 20.5f); break;
                    case HPushButtonGlyph.Pencil:
                        S(16.5f, 3.5f, 19.5f, 6.5f, 7, 19, 3, 20, 4, 16, 16.5f, 3.5f); L(12, 20, 21, 20);
                        break;
                    case HPushButtonGlyph.Copy: RR(8, 8, 13, 13, 2); RR(3, 3, 13, 13, 2); break;
                    case HPushButtonGlyph.Scissors:
                        Ring(6, 6, 3.2f); Ring(6, 18, 3.2f);
                        L(8.2f, 8.2f, 12, 12); L(19.5f, 4.5f, 8.4f, 15.6f); L(14.3f, 14.3f, 19.5f, 19.5f);
                        break;
                    case HPushButtonGlyph.Clipboard:
                        RR(5, 4.5f, 14, 16.5f, 2); RR(8.5f, 2.5f, 7, 4, 1);
                        L(9, 11, 15, 11); L(9, 15, 15, 15);
                        break;
                    case HPushButtonGlyph.File: RR(4, 3, 16, 18, 1.5f); S(14, 3, 14, 8.5f, 20, 8.5f); break;
                    case HPushButtonGlyph.FilePlus:
                        RR(4, 3, 16, 18, 1.5f); S(14, 3, 14, 8.5f, 20, 8.5f);
                        L(12, 11.5f, 12, 17.5f); L(9, 14.5f, 15, 14.5f);
                        break;
                    case HPushButtonGlyph.Folder: RR(2, 6, 20, 14, 2); S(2, 9.5f, 8.5f, 9.5f, 10.5f, 6, 2, 6); break;
                    case HPushButtonGlyph.FolderCheck:
                        RR(2, 6, 20, 14, 2); S(2, 9.5f, 8.5f, 9.5f, 10.5f, 6, 2, 6);
                        S(8.8f, 13.5f, 11, 15.8f, 15.8f, 10.5f);
                        break;
                    case HPushButtonGlyph.Trash:
                        S(3, 6, 21, 6); S(6, 6, 6.8f, 19.2f, 17.2f, 19.2f, 18, 6);
                        S(9, 6, 9, 3.8f, 15, 3.8f, 15, 6); L(10, 10.5f, 10, 17); L(14, 10.5f, 14, 17);
                        break;
                    case HPushButtonGlyph.Printer:
                        S(6, 9, 6, 2.5f, 18, 2.5f, 18, 9); RR(2.5f, 8, 19, 8, 2); RR(6, 14, 12, 7, 1.5f);
                        break;
                    case HPushButtonGlyph.Download:
                        S(3, 14.5f, 3, 18.5f, 21, 18.5f, 21, 14.5f); S(7, 10, 12, 15, 17, 10); L(12, 15, 12, 3.5f);
                        break;
                    case HPushButtonGlyph.Upload:
                        S(3, 14.5f, 3, 18.5f, 21, 18.5f, 21, 14.5f); S(17, 8.5f, 12, 3.5f, 7, 8.5f); L(12, 3.5f, 12, 14.5f);
                        break;
                    case HPushButtonGlyph.Cloud:
                        Arc(4.5f, 7.5f, 9, 9, 180, 180); Arc(11, 4.5f, 9.5f, 9.5f, 180, 180);
                        L(20.5f, 9.2f, 20.5f, 15); L(20.5f, 15, 4.5f, 15); L(4.5f, 15, 4.5f, 12);
                        break;
                    case HPushButtonGlyph.Link:
                        Arc(12.5f, 3.5f, 8.5f, 8.5f, 300, 250); Arc(3, 12, 8.5f, 8.5f, 120, 250);
                        break;
                    case HPushButtonGlyph.Unlink:
                        Arc(12.5f, 3.5f, 8.5f, 8.5f, 300, 250); Arc(3, 12, 8.5f, 8.5f, 120, 250); L(3, 21, 21, 3);
                        break;
                    case HPushButtonGlyph.Bold: Txt("B", 12, 12.5f, 14); break;
                    case HPushButtonGlyph.Italic: L(13.8f, 4.5f, 10.2f, 19.5f); L(9, 4.5f, 15, 4.5f); L(6, 19.5f, 12, 19.5f); break;
                    case HPushButtonGlyph.Underline:
                        L(7, 5, 7, 14.2f); Arc(7, 9.4f, 10, 9.6f, 180, 180); L(17, 14.2f, 17, 5); L(6, 19.5f, 18, 19.5f);
                        break;
                    case HPushButtonGlyph.AlignLeft: L(4, 6, 20, 6); L(4, 12, 16, 12); L(4, 18, 20, 18); break;
                    case HPushButtonGlyph.AlignCenter: L(3, 6, 21, 6); L(6, 12, 18, 12); L(4, 18, 20, 18); break;
                    case HPushButtonGlyph.AlignRight: L(4, 6, 20, 6); L(8, 12, 20, 12); L(4, 18, 20, 18); break;
                    case HPushButtonGlyph.List:
                        Dot(5, 6, 1.8f); Dot(5, 12, 1.8f); Dot(5, 18, 1.8f);
                        L(9.5f, 6, 21, 6); L(9.5f, 12, 21, 12); L(9.5f, 18, 21, 18);
                        break;
                    case HPushButtonGlyph.ListOrdered:
                        Txt("1", 4.2f, 6, 7); Txt("2", 4.2f, 12, 7); Txt("3", 4.2f, 18, 7);
                        L(9.5f, 6, 21, 6); L(9.5f, 12, 21, 12); L(9.5f, 18, 21, 18);
                        break;
                    case HPushButtonGlyph.IndentRight:
                        L(3, 6, 21, 6); L(11, 12, 21, 12); L(11, 18, 21, 18); S(3.5f, 11, 7.5f, 15, 3.5f, 19);
                        break;
                    case HPushButtonGlyph.IndentLeft:
                        L(3, 6, 21, 6); L(3, 12, 13, 12); L(3, 18, 13, 18); S(11.5f, 11, 7.5f, 15, 11.5f, 19);
                        break;
                    case HPushButtonGlyph.Table:
                        RR(3, 4, 18, 16, 1.5f); L(3, 10, 21, 10); L(3, 16, 21, 16); L(9, 4, 9, 20); L(15, 4, 15, 20);
                        break;
                    case HPushButtonGlyph.Grid: RR(3, 3, 18, 18, 1); L(12, 3, 12, 21); L(3, 12, 21, 12); break;
                    case HPushButtonGlyph.Eraser:
                        S(12.8f, 3.6f, 18.4f, 9.2f, 11.4f, 18, 7.6f, 14, 12.8f, 3.6f);
                        L(5, 21, 21, 21); L(7.6f, 14, 14.4f, 20.8f);
                        break;
                    case HPushButtonGlyph.Brush:
                        L(4, 19, 9, 14); S(9, 14, 14.5f, 5.5f, 19, 10, 13.5f, 19.5f, 9, 14); FP(3, 21, 4.5f, 16.5f, 7.5f, 19.5f, 3, 21);
                        break;
                    case HPushButtonGlyph.Home:
                        S(3, 10.5f, 12, 3, 21, 10.5f); S(6, 10, 6, 20, 18, 20, 18, 10);
                        S(10, 20, 10, 15, 14, 15, 14, 20);
                        break;
                    case HPushButtonGlyph.Settings:
                        Ring(12, 12, 8.5f); Ring(12, 12, 3);
                        for (int k = 0; k < 8; k++)
                        {
                            double a = k * Math.PI / 4;
                            L(12 + (float)Math.Cos(a) * 8.5f, 12 + (float)Math.Sin(a) * 8.5f,
                              12 + (float)Math.Cos(a) * 11, 12 + (float)Math.Sin(a) * 11);
                        }
                        break;
                    case HPushButtonGlyph.User:
                        Ring(12, 8, 6); Arc(5, 12, 14, 14, 180, 180); S(5, 19, 5, 21, 19, 21, 19, 19);
                        break;
                    case HPushButtonGlyph.Users:
                        Ring(15.5f, 8, 5); Arc(11.5f, 13, 8, 8, 180, 180); L(11.5f, 17, 11.5f, 20); L(19.5f, 17, 19.5f, 20);
                        Ring(8.5f, 9, 5.5f); Arc(2, 13.5f, 13, 12, 180, 180); S(2, 19.5f, 2, 21, 16, 21, 16, 19.5f);
                        break;
                    case HPushButtonGlyph.Star:
                        S(12, 2, 15.1f, 8.6f, 22, 9.3f, 17, 14.1f, 18.3f, 21, 12, 17.7f, 5.7f, 21, 7, 14.1f, 2, 9.3f, 8.9f, 8.6f, 12, 2);
                        break;
                    case HPushButtonGlyph.Heart:
                        Arc(3, 3.5f, 8, 8, 180, 180); Arc(13, 3.5f, 8, 8, 180, 180);
                        S(21, 7.5f, 12, 19, 3, 7.5f);
                        break;
                    case HPushButtonGlyph.Bookmark: S(5, 3, 19, 3, 19, 21, 12, 17, 5, 21); break;
                    case HPushButtonGlyph.Tag:
                        S(12, 2, 2, 2, 2, 12, 11.3f, 21.3f, 20, 12.6f, 12, 2); Ring(7, 7, 2.5f);
                        break;
                    case HPushButtonGlyph.Flag: L(5, 3, 5, 21); S(5, 4, 20, 4, 16.5f, 9, 20, 14, 5, 14); break;
                    case HPushButtonGlyph.Mail: RR(3, 5, 18, 14, 2); S(3.5f, 7.5f, 12, 13, 20.5f, 7.5f); break;
                    case HPushButtonGlyph.Message: RR(3, 4, 18, 14, 3); S(8, 18, 8, 21.5f, 12, 18); break;
                    case HPushButtonGlyph.Phone:
                        using (var ph = new GraphicsPath())
                        {
                            ph.AddBezier(P(5.5f, 3.2f), P(8, 3), P(9, 5.5f), P(9.6f, 7));
                            ph.AddBezier(P(9.6f, 7), P(11, 9), P(13, 11), P(15, 14.4f));
                            ph.AddBezier(P(15, 14.4f), P(15.6f, 13.6f), P(17.5f, 13.5f), P(20, 15.2f));
                            ph.AddBezier(P(20, 15.2f), P(21.4f, 16), P(21.5f, 18.5f), P(19.8f, 20));
                            ph.AddBezier(P(19.8f, 20), P(13, 18.5f), P(6, 11.5f), P(3.6f, 6.5f));
                            ph.AddBezier(P(3.6f, 6.5f), P(2.4f, 4.5f), P(3.6f, 3.6f), P(5.5f, 3.2f));
                            ph.CloseFigure();
                            g.DrawPath(pen, ph);
                        }
                        break;
                    case HPushButtonGlyph.Calendar:
                        RR(3, 4.5f, 18, 16, 2); L(3, 9.5f, 21, 9.5f); L(8, 2.5f, 8, 6.5f); L(16, 2.5f, 16, 6.5f);
                        Dot(8, 13.5f, 1.3f); Dot(12, 13.5f, 1.3f); Dot(16, 13.5f, 1.3f);
                        Dot(8, 17, 1.3f); Dot(12, 17, 1.3f); Dot(16, 17, 1.3f);
                        break;
                    case HPushButtonGlyph.Clock: Ring(12, 12, 9); L(12, 7, 12, 12); L(12, 12, 16, 14); break;
                    case HPushButtonGlyph.Lock:
                        RR(5, 10, 14, 11, 2); Arc(8, 5, 8, 9, 180, 180); Dot(12, 14.8f, 2); L(12, 15.8f, 12, 18.5f);
                        break;
                    case HPushButtonGlyph.Unlock:
                        RR(5, 10, 14, 11, 2); Arc(8, 5, 8, 9, 180, 110); L(13.4f, 5.7f, 16.5f, 5.2f);
                        Dot(12, 14.8f, 2); L(12, 15.8f, 12, 18.5f);
                        break;
                    case HPushButtonGlyph.Key:
                        Ring(7.5f, 12, 5); L(11, 15.5f, 21, 5.5f); L(15.5f, 10, 18, 12.5f); L(18, 7.5f, 20.5f, 10);
                        break;
                    case HPushButtonGlyph.Eye:
                        using (var eye = new GraphicsPath())
                        {
                            eye.AddBezier(P(2, 12), P(6, 5), P(18, 5), P(22, 12));
                            eye.AddBezier(P(22, 12), P(18, 19), P(6, 19), P(2, 12));
                            eye.CloseFigure();
                            g.DrawPath(pen, eye);
                        }
                        Ring(12, 12, 3.2f);
                        break;
                    case HPushButtonGlyph.EyeOff:
                        using (var eye = new GraphicsPath())
                        {
                            eye.AddBezier(P(2, 12), P(6, 5), P(18, 5), P(22, 12));
                            eye.AddBezier(P(22, 12), P(18, 19), P(6, 19), P(2, 12));
                            eye.CloseFigure();
                            g.DrawPath(pen, eye);
                        }
                        L(3.5f, 3.5f, 20.5f, 20.5f);
                        break;
                    case HPushButtonGlyph.Info: Ring(12, 12, 9); Dot(12, 8, 2); L(12, 11, 12, 17); break;
                    case HPushButtonGlyph.Alert:
                        S(12, 3, 21.5f, 20, 2.5f, 20, 12, 3); L(12, 9, 12, 14.5f); Dot(12, 17.3f, 1.6f);
                        break;
                    case HPushButtonGlyph.CheckCircle: Ring(12, 12, 18); S(8, 12.5f, 11, 15.5f, 16.5f, 9.5f); break;
                    case HPushButtonGlyph.XCircle: Ring(12, 12, 18); S(9, 9, 15, 15); S(15, 9, 9, 15); break;
                    case HPushButtonGlyph.PlusCircle:
                        Ring(12, 12, 18); L(12, 7.5f, 12, 16.5f); L(7.5f, 12, 16.5f, 12);
                        break;
                    case HPushButtonGlyph.MinusCircle: Ring(12, 12, 18); L(7.5f, 12, 16.5f, 12); break;
                    case HPushButtonGlyph.HelpCircle:
                        Ring(12, 12, 18); Arc(9, 6.5f, 6, 5.5f, 200, 220); L(12, 13.5f, 12, 15.5f); Dot(12, 17.8f, 1.4f);
                        break;
                    case HPushButtonGlyph.Shield: S(12, 2, 20, 5, 20, 11, 12, 22, 4, 11, 4, 5, 12, 2); break;
                    case HPushButtonGlyph.Target: Ring(12, 12, 18); Ring(12, 12, 11); Dot(12, 12, 3.5f); break;
                    case HPushButtonGlyph.Crosshair:
                        Ring(12, 12, 18);
                        L(12, 1.5f, 12, 5); L(12, 19, 12, 22.5f); L(1.5f, 12, 5, 12); L(19, 12, 22.5f, 12);
                        Dot(12, 12, 2);
                        break;
                    case HPushButtonGlyph.Zap: S(13, 2, 4, 13.5f, 11, 13.5f, 10, 22, 20, 10.5f, 13, 10.5f, 13, 2); break;
                    case HPushButtonGlyph.Sun:
                        Ring(12, 12, 5.5f);
                        for (int k = 0; k < 8; k++)
                        {
                            double a = k * Math.PI / 4;
                            L(12 + (float)Math.Cos(a) * 8.5f, 12 + (float)Math.Sin(a) * 8.5f,
                              12 + (float)Math.Cos(a) * 11, 12 + (float)Math.Sin(a) * 11);
                        }
                        break;
                    case HPushButtonGlyph.Moon:
                        using (var moon = new GraphicsPath())
                        {
                            moon.AddArc(B(3, 4, 16, 16), 70, 220);
                            moon.AddBezier(P(13.74f, 19.52f), P(19, 16), P(20, 8), P(13.74f, 4.48f));
                            moon.CloseFigure();
                            g.DrawPath(pen, moon);
                        }
                        break;
                    case HPushButtonGlyph.MapPin:
                        using (var pin = new GraphicsPath())
                        {
                            pin.AddArc(B(4, 2, 16, 16), 180, -180);
                            pin.AddLines(new[] { P(20, 10), P(12, 22), P(4, 10) });
                            pin.CloseFigure();
                            g.DrawPath(pen, pin);
                        }
                        Ring(12, 10, 5);
                        break;
                    case HPushButtonGlyph.Gift:
                        RR(3, 8, 18, 13, 1.5f); L(3, 12, 21, 12); L(12, 8, 12, 21);
                        Arc(4.5f, 3, 6, 5, 180, 180); Arc(13.5f, 3, 6, 5, 180, 180);
                        L(4.5f, 5.5f, 12, 8); L(19.5f, 5.5f, 12, 8); L(4.5f, 5.5f, 19.5f, 5.5f);
                        break;
                    case HPushButtonGlyph.Award:
                        Ring(12, 9, 11); S(8.5f, 13.8f, 7, 21, 10, 19.5f, 12, 21); S(15.5f, 13.8f, 17, 21, 14, 19.5f, 12, 21);
                        break;
                    case HPushButtonGlyph.ThumbUp:
                        RR(2.5f, 10.5f, 4.5f, 9.5f, 1.2f);
                        S(7, 20, 7, 11, 10.8f, 11, 12.2f, 6.2f, 14, 6.6f, 13, 11, 18.6f, 11, 20.2f, 12.6f, 18.2f, 20, 7, 20);
                        break;
                    case HPushButtonGlyph.ThumbDown:
                        RR(2.5f, 4, 4.5f, 9.5f, 1.2f);
                        S(7, 4, 7, 13, 10.8f, 13, 12.2f, 17.8f, 14, 17.4f, 13, 13, 18.6f, 13, 20.2f, 11.4f, 18.2f, 4, 7, 4);
                        break;
                    case HPushButtonGlyph.BarChart:
                        L(3.5f, 20.5f, 20.5f, 20.5f);
                        RR(6, 12, 3, 8, 0.5f, true); RR(10.8f, 8, 3, 12, 0.5f, true); RR(15.6f, 5, 3, 15, 0.5f, true);
                        break;
                    case HPushButtonGlyph.PieChart:
                        Ring(12, 12, 19); FP(12, 12, 12, 2.5f, 20.5f, 8); L(12, 12, 12, 3); L(12, 12, 20.5f, 8);
                        break;
                    case HPushButtonGlyph.ClipboardCheck:
                        RR(5, 4.5f, 14, 16.5f, 2); RR(8.5f, 2.5f, 7, 4, 1); S(8.5f, 13, 11, 15.5f, 16, 9.5f);
                        break;
                    case HPushButtonGlyph.Coffee:
                        RR(4, 7, 13, 11, 2); Arc(16, 9, 5.5f, 7, 270, 180);
                        L(3, 21, 20, 21); L(7, 2, 7, 4.5f); L(11, 2, 11, 4.5f); L(15, 2, 15, 4.5f);
                        break;
                    case HPushButtonGlyph.Wrench:
                        Arc(11, 2, 10.5f, 10.5f, 30, 250); L(12.6f, 10.8f, 4.2f, 19.2f);
                        L(3.2f, 17.8f, 6.2f, 20.8f); L(4.8f, 20.5f, 2.2f, 21.2f);
                        break;
                }
            }
            g.SmoothingMode = oldMode;
        }
        #endregion
        #region 绘制辅助
        private HPushButtonPalette ResolvePalette(bool hover, bool pressed)
        {
            return HPushButtonScheme.Resolve(_buttonStyle, hover, pressed, !_owner.Enabled,
                IsCheckedDown,
                new HPushButtonPalette { Back = _normalBack, Border = _normalBorder, Fore = _normalFore },
                new HPushButtonPalette { Back = _hoverBack, Border = _hoverBorder, Fore = _hoverFore },
                new HPushButtonPalette { Back = _pressBack, Border = _pressBorder, Fore = _pressFore });
        }
        private static HPushButtonPalette LerpPalette(HPushButtonPalette a, HPushButtonPalette b, float t)
        {
            return new HPushButtonPalette
            {
                Back = LerpColor(a.Back, b.Back, t),
                Border = LerpColor(a.Border, b.Border, t),
                Fore = LerpColor(a.Fore, b.Fore, t)
            };
        }
        private static Color LerpColor(Color a, Color b, float t)
        {
            if (a == Color.Empty) return t > 0.5f ? b : a;
            if (b == Color.Empty) return a;
            return HPushButtonScheme.Mix(a, b, t);
        }
        #endregion
    }
    /// <summary>
    /// 普通按钮：点击回弹，不保持选中态。
    /// 122 种预设风格 + Custom 三态色，129 种矢量图标，4 种外框形状，全部 GDI+ 矢量绘制。
    /// 外观能力由内部 HPushButtonEngine 实现；点击保持按下的双态按钮见 HToggleButton。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    public class HPushButton : HButtonBase
    {
        private readonly HPushButtonEngine _engine;
        public HPushButton()
        {
            _engine = new HPushButtonEngine(this);
            BackColor = Color.Transparent;
            Radius = 7;
            Size = new Size(75, 30);
            Cursor = Cursors.Hand;
        }
        /// <summary>异形与透明背景完全自绘，不使用基类的窗口 Region 裁剪。</summary>
        protected override void UpdateRegion() { }
        /// <summary>按钮默认圆角 7（基类默认 0 为直角原生按钮）。</summary>
        [DefaultValue(7)]
        public override int Radius
        {
            get => base.Radius;
            set => base.Radius = Math.Max(0, value);
        }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("外框形状"), HDescriptionLanguage("外框形状：直角/圆角/胶囊/正圆"), Browsable(true)]
        [DefaultValue(HPushButtonShape.Round)]
        public HPushButtonShape Shape { get => _engine.Shape; set => _engine.Shape = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("预设风格"), HDescriptionLanguage("预设风格（122 种：30 色 × 实心/描边/立体/渐变），Custom 时使用自定义三态色"), Browsable(true)]
        [DefaultValue(HPushButtonStyle.Primary)]
        public HPushButtonStyle ButtonStyle { get => _engine.ButtonStyle; set => _engine.ButtonStyle = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("常规状态图标"), HDescriptionLanguage("常规状态图标（矢量 Glyph 优先于它；另有悬停专用图标）"), Browsable(true)]
        public Image Image { get => _engine.Image; set => _engine.Image = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("悬停时显示的图标"), HDescriptionLanguage("悬停时显示的图标（缺省回落到 Image）"), Browsable(true)]
        public Image HoverImage { get => _engine.HoverImage; set => _engine.HoverImage = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("内置矢量图标"), HDescriptionLanguage("内置矢量图标（129 种，不使用资源图片，颜色跟随文字色）；设置后优先于 Image"), Browsable(true)]
        [DefaultValue(HPushButtonGlyph.None)]
        public HPushButtonGlyph Glyph { get => _engine.Glyph; set => _engine.Glyph = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("图标边长"), HDescriptionLanguage("图标边长（正方形）"), Browsable(true)]
        [DefaultValue(16)]
        public int ImageSize { get => _engine.ImageSize; set => _engine.ImageSize = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("图文间距"), HDescriptionLanguage("图文间距"), Browsable(true)]
        [DefaultValue(4)]
        public int ImageTextGap { get => _engine.ImageTextGap; set => _engine.ImageTextGap = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("常规底色"), HDescriptionLanguage("常规底色（空为自动浅灰）"), Browsable(true)]
        public Color NormalBackColor { get => _engine.NormalBackColor; set => _engine.NormalBackColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("常规边色"), HDescriptionLanguage("常规边色（空为同底色）"), Browsable(true)]
        public Color NormalBorderColor { get => _engine.NormalBorderColor; set => _engine.NormalBorderColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("常规字色"), HDescriptionLanguage("常规字色（空为按底色亮度自动黑白）"), Browsable(true)]
        public Color NormalForeColor { get => _engine.NormalForeColor; set => _engine.NormalForeColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("悬停底色"), HDescriptionLanguage("悬停底色（空为常规色自动推导）"), Browsable(true)]
        public Color HoverBackColor { get => _engine.HoverBackColor; set => _engine.HoverBackColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("悬停边色"), HDescriptionLanguage("悬停边色（空为自动推导）"), Browsable(true)]
        public Color HoverBorderColor { get => _engine.HoverBorderColor; set => _engine.HoverBorderColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("悬停字色"), HDescriptionLanguage("悬停字色（空为常规字色）"), Browsable(true)]
        public Color HoverForeColor { get => _engine.HoverForeColor; set => _engine.HoverForeColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("按下底色"), HDescriptionLanguage("按下底色（空为常规色自动压暗）"), Browsable(true)]
        public Color PressedBackColor { get => _engine.PressedBackColor; set => _engine.PressedBackColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("按下边色"), HDescriptionLanguage("按下边色（空为自动推导）"), Browsable(true)]
        public Color PressedBorderColor { get => _engine.PressedBorderColor; set => _engine.PressedBorderColor = value; }
        [HCategoryLanguage("按钮自定义色"), HDisplayNameLanguage("按下字色"), HDescriptionLanguage("按下字色（空为常规字色）"), Browsable(true)]
        public Color PressedForeColor { get => _engine.PressedForeColor; set => _engine.PressedForeColor = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("悬停气泡文字"), HDescriptionLanguage("悬停气泡文字（空则不显示）"), Browsable(true)]
        [DefaultValue("")]
        public string TipText { get => _engine.TipText; set => _engine.TipText = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("悬停气泡配色"), HDescriptionLanguage("悬停气泡配色（Chart 气泡体系）"), Browsable(true)]
        [DefaultValue(HTipSchemeKind.Auto)]
        public HTipSchemeKind TipScheme { get => _engine.TipScheme; set => _engine.TipScheme = value; }
        protected override void OnPaint(PaintEventArgs e)
        {
            _engine.Paint(e.Graphics, _pressed);
            if (Focused && ShowFocusCues) _engine.PaintFocus(e.Graphics);
        }
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (BackColor.A < 255) { base.OnPaintBackground(pevent); return; }
            _engine.PaintBackground(pevent.Graphics);
        }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _engine.Enter(); }
        protected override void OnMouseLeave(EventArgs e) { _engine.Leave(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _engine.Press(); base.OnMouseDown(mevent); }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _engine.Dispose();
            base.Dispose(disposing);
        }
    }
    /// <summary>
    /// 按钮族专用悬停气泡窗：无焦点、不进任务栏，Chart 气泡配色，圆角矩形+小三角，全部自绘。
    /// </summary>
    internal class HPushButtonTip : Form
    {
        private const int PadX = 10, PadY = 6, ArrowH = 6, Radius = 6;
        private HTipColors _colors;
        private string _text = string.Empty;
        private bool _below = true;
        private int _arrowX;
        public HPushButtonTip()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
        /// <summary>不夺取焦点。</summary>
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                return cp;
            }
        }
        /// <summary>按所属按钮定位（下方优先，空间不足翻到上方）并显示。</summary>
        public void ShowFor(Control owner, string text, HTipSchemeKind scheme)
        {
            _text = text;
            _colors = HTipPalettes.Get(scheme, false);
            Font = owner.Font;
            var sz = TextRenderer.MeasureText(text, Font);
            Size = new Size(sz.Width + PadX * 2, sz.Height + PadY * 2 + ArrowH);
            var anchorBottom = owner.PointToScreen(new Point(owner.Width / 2, owner.Height));
            var work = Screen.FromPoint(anchorBottom).WorkingArea;
            int x = anchorBottom.X - Width / 2;
            if (x < work.Left + 2) x = work.Left + 2;
            if (x + Width > work.Right - 2) x = work.Right - 2 - Width;
            _below = anchorBottom.Y + 4 + Height <= work.Bottom;
            int y = _below ? anchorBottom.Y + 4 : owner.PointToScreen(new Point(owner.Width / 2, 0)).Y - Height - 4;
            Location = new Point(x, y);
            _arrowX = Math.Max(12, Math.Min(Width - 12, anchorBottom.X - x));
            Region = new Region(BuildBubblePath());
            if (!Visible) Show(owner);
            else Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle body = _below
                ? new Rectangle(0, ArrowH, Width - 1, Height - ArrowH - 1)
                : new Rectangle(0, 0, Width - 1, Height - ArrowH - 1);
            // 小三角
            Point tip = _below
                ? new Point(_arrowX, 0)
                : new Point(_arrowX, Height - 1);
            Point b1 = _below ? new Point(_arrowX - 6, ArrowH + 1) : new Point(_arrowX - 6, Height - ArrowH - 1);
            Point b2 = _below ? new Point(_arrowX + 6, ArrowH + 1) : new Point(_arrowX + 6, Height - ArrowH - 1);
            using (var tri = new SolidBrush(_colors.Bg))
                g.FillPolygon(tri, new[] { tip, b1, b2 });
            // 气泡体
            using (GraphicsPath path = HDrawPaint.CreatePath(body, Radius, HRoundStyle.All))
            using (var brush = new SolidBrush(_colors.Bg))
                g.FillPath(brush, path);
            using (GraphicsPath path = HDrawPaint.CreatePath(body, Radius, HRoundStyle.All))
            using (var pen = new Pen(_colors.Border, 1f))
                g.DrawPath(pen, path);
            using (var pen = new Pen(_colors.Border, 1f))
            {
                g.DrawLine(pen, tip, b1);
                g.DrawLine(pen, tip, b2);
            }
            // 用三角中线盖住朝体内的两条边，形成“气泡开口”
            using (var pen = new Pen(_colors.Bg, 2f))
                g.DrawLine(pen, b1, b2);
            TextRenderer.DrawText(g, _text, Font, body, _colors.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
        /// <summary>气泡体 + 三角合成外形（用于异形 Region）。</summary>
        private GraphicsPath BuildBubblePath()
        {
            Rectangle body = _below
                ? new Rectangle(0, ArrowH, Width - 1, Height - ArrowH - 1)
                : new Rectangle(0, 0, Width - 1, Height - ArrowH - 1);
            var path = HDrawPaint.CreatePath(body, Radius, HRoundStyle.All);
            Point tip = _below ? new Point(_arrowX, 0) : new Point(_arrowX, Height - 1);
            Point b1 = _below ? new Point(_arrowX - 6, ArrowH) : new Point(_arrowX - 6, Height - ArrowH);
            Point b2 = _below ? new Point(_arrowX + 6, ArrowH) : new Point(_arrowX + 6, Height - ArrowH);
            path.AddPolygon(new[] { tip, b1, b2 });
            return path;
        }
    }
}