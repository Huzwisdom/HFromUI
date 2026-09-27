using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Tools.Nature;
namespace HFromUI.HControl.Tools.Editors
{
    using HFromUI.HLangage;
    using HFromUI.HControl.Base;
    /// <summary>搜索图标所在方位。</summary>
    public enum HSearchIconPosition
    {
        /// <summary>图标在前（左侧）。</summary>
        Front = 0,
        /// <summary>图标在后（右侧）。</summary>
        Back = 1
    }
    /// <summary>搜索事件参数。</summary>
    public class HSearchQueryEventArgs : EventArgs
    {
        /// <summary>搜索关键词。</summary>
        public string SearchText { get; }
        public HSearchQueryEventArgs(string text) { SearchText = text ?? string.Empty; }
    }
    /// <summary>
    /// 纯自绘搜索框（继承 HTextBoxBase）：22 种外观风格与 HNumericUpDown 一致；
    /// 搜索放大镜图标可置于前/后，另一侧带一键清除；支持防抖触发、回车立即搜索、Esc 清空。
    /// </summary>
    [DefaultProperty("WatermarkText")]
    [DefaultEvent("Search")]
    [HDescriptionLanguage("搜索框：22种外观风格，搜索图标可在前或在后，带防抖与清除按钮")]
    public class HSearchBox : HTextBoxBase
    {
        private HEditStyle _style = HEditStyle.Rounded;
        private HSearchIconPosition _iconPos = HSearchIconPosition.Front;
        private bool _showClearButton = true;
        private float _iconScale = 1f;
        private int _debounceMs = 300;
        private Color _iconColor = Color.Gray;
        private readonly Timer _debounce;
        private RectangleF _iconRect;
        private RectangleF _clearRect;
        private bool _iconHover, _clearHover, _clearPress;
        public HSearchBox()
        {
            Radius = 0;
            Size = new Size(180, 32);
            TextAlign = ContentAlignment.MiddleLeft;
            WatermarkText = HTranslation.GetContent("搜索...");
            ApplyStyleColors();
            _debounce = new Timer();
            _debounce.Tick += (s, e) => { _debounce.Stop(); OnSearch(); };
        }
        #region 属性
        /// <summary>外观风格（与数值框相同的 22 种）。</summary>
        [HCategoryLanguage("搜索框"), HDisplayNameLanguage("搜索框外观风格"), HDescriptionLanguage("搜索框外观风格（22种）"), Browsable(true)]
        [DefaultValue(HEditStyle.Rounded)]
        public HEditStyle SearchStyle
        {
            get => _style;
            set { _style = value; ApplyStyleColors(); UpdateRegion(); Invalidate(); }
        }
        /// <summary>搜索图标方位：前（左）或后（右）。</summary>
        [HCategoryLanguage("搜索框"), HDisplayNameLanguage("搜索图标方位"), HDescriptionLanguage("搜索图标方位：前面（左）或后面（右）"), Browsable(true)]
        [DefaultValue(HSearchIconPosition.Front)]
        public HSearchIconPosition IconPosition
        {
            get => _iconPos;
            set { _iconPos = value; RefreshLayout(); Invalidate(); }
        }
        /// <summary>是否显示一键清除按钮（有内容时出现于图标的对侧）。</summary>
        [HCategoryLanguage("搜索框"), HDisplayNameLanguage("是否显示一键清除按钮"), HDescriptionLanguage("有内容时是否显示清除按钮"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowClearButton
        {
            get => _showClearButton;
            set { _showClearButton = value; RefreshLayout(); Invalidate(); }
        }
        /// <summary>防抖延迟（毫秒），0 表示输入即触发。</summary>
        [HCategoryLanguage("搜索框"), HDisplayNameLanguage("防抖延迟毫秒数"), HDescriptionLanguage("防抖延迟毫秒数，0 表示不防抖"), Browsable(true)]
        [DefaultValue(300)]
        public int DebounceMs
        {
            get => _debounceMs;
            set => _debounceMs = Math.Max(0, value);
        }
        /// <summary>搜索图标颜色（不随风格变化时可自定义）。</summary>
        [HCategoryLanguage("搜索框"), HDisplayNameLanguage("搜索图标颜色"), HDescriptionLanguage("搜索图标颜色"), Browsable(true)]
        public Color IconColor
        {
            get => _iconColor;
            set { _iconColor = value; Invalidate(); }
        }
        /// <summary>放大镜与清除按钮图标的大小比例：1 为默认大小，按比例放大或缩小（0.5~3）。</summary>
        [HCategoryLanguage("搜索框"), HDisplayNameLanguage("图标大小比例"), HDescriptionLanguage("放大镜与清除按钮图标相对默认大小的倍数，1 为默认"), Browsable(true)]
        [DefaultValue(1f)]
        public float IconScale
        {
            get => _iconScale;
            set { _iconScale = Math.Max(0.5f, Math.Min(3f, value)); Invalidate(); }
        }
        /// <summary>搜索事件（防抖后或回车/点图标立即触发）。</summary>
        public event EventHandler<HSearchQueryEventArgs> Search;
        #endregion
        #region 触发
        /// <summary>立即触发搜索（跳过防抖）。</summary>
        public void SearchNow()
        {
            _debounce.Stop();
            OnSearch();
        }
        private void OnSearch()
        {
            try { Search?.Invoke(this, new HSearchQueryEventArgs(Text)); } catch { }
        }
        private void ApplyStyleColors()
        {
            BackColor = HEditFrame.BackColor(_style);
            ForeColor = HEditFrame.ForeColor(_style);
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (_debounceMs > 0)
            {
                _debounce.Stop();
                _debounce.Interval = _debounceMs;
                _debounce.Start();
            }
            else OnSearch();
            Invalidate();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _debounce.Stop();
                OnSearch();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                _debounce.Stop();
                if (Text.Length > 0) Text = string.Empty;
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) _debounce.Dispose();
            base.Dispose(disposing);
        }
        #endregion
        #region 装饰区
        protected override bool UseDefaultFrame => false;
        // 底色只在框体外形内（四角的父色铺底由基类 OnPaintBackground 完成）
        protected override void FillShapeBackground(Graphics g, Rectangle rect)
            => HEditFrame.FillBackground(g, rect, _style, ShapeFillColor, Enabled, Radius);
        // 自绘框满高：内容区上下等距，文字与水印默认即在框内垂直居中
        // （TextAlign 默认 MiddleLeft，仍可选九方位）
        protected override Rectangle AdjustContentRect(Rectangle cr)
            => new Rectangle(cr.X, 2, cr.Width, Math.Max(0, Height - 4));
        private bool ClearVisible => _showClearButton && Text.Length > 0;
        private int IconBand => Math.Max(20, (int)(Height * 0.92f));
        private int ClearBand => ClearVisible ? Math.Max(18, (int)(Height * 0.7f)) : 0;
        protected override int AdornmentLeftWidth
            => (_iconPos == HSearchIconPosition.Front ? IconBand : 0)
             + (_iconPos == HSearchIconPosition.Back && ClearVisible ? ClearBand : 0);
        protected override int AdornmentRightWidth
            => (_iconPos == HSearchIconPosition.Back ? IconBand : 0)
             + (_iconPos == HSearchIconPosition.Front && ClearVisible ? ClearBand : 0);
        private void LayoutAdorn()
        {
            int h = Height;
            int ib = IconBand, cb = ClearBand;
            if (_iconPos == HSearchIconPosition.Front)
            {
                _iconRect = new RectangleF(2, 0, ib, h);
                _clearRect = ClearVisible ? new RectangleF(Width - cb - 2, 0, cb, h) : RectangleF.Empty;
            }
            else
            {
                _iconRect = new RectangleF(Width - ib - 2, 0, ib, h);
                _clearRect = ClearVisible ? new RectangleF(2, 0, cb, h) : RectangleF.Empty;
            }
        }
        protected override void DrawAdornments(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            LayoutAdorn();
            HEditFrame.DrawFrame(g, ClientRectangle, _style, Focused, Enabled, _iconHover || _clearHover, Radius);
            // 图标与叉号统一裁到框体外形内：大圆角/胶囊时悬停底色不得溢出圆角外
            Region oldClip = g.Clip;
            using (GraphicsPath clip = HEditFrame.CreateRegionPath(new RectangleF(0, 0, Width, Height), _style, Radius))
            {
                if (clip != null) g.SetClip(clip, CombineMode.Intersect);
                DrawMagnifier(g, _iconRect);
                if (ClearVisible) DrawClear(g, _clearRect);
            }
            g.Clip = oldClip;
        }
        private void DrawMagnifier(Graphics g, RectangleF r)
        {
            if (r.Width <= 2f) return;
            Color c = !Enabled ? SystemColors.GrayText
                : _iconHover ? HEditFrame.Accent(_style) : _iconColor;
            float u = Math.Min(r.Width, r.Height);
            float cx = r.X + r.Width / 2f, cy = r.Y + r.Height / 2f;
            float rr = u * 0.26f;
            float pw = Math.Max(1.4f, u * 0.07f);
            // 图元（含手柄与线宽）在比例 1 时约占装饰带半宽的 0.65，超出时钳制，避免越过边框压住文字
            float fit = (u * 0.5f - 1.5f) / (1.25f * rr + pw * 0.5f);
            float s = Math.Min(_iconScale, Math.Max(0.5f, fit));
            GraphicsState gs = g.Save();
            g.TranslateTransform(cx, cy);
            g.ScaleTransform(s, s);
            g.TranslateTransform(-cx, -cy);
            using (var p = new Pen(c, pw) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawEllipse(p, cx - rr - rr * 0.25f, cy - rr - rr * 0.25f, rr * 2f, rr * 2f);
                float a = 42f * (float)(Math.PI / 180.0);
                float x1 = cx - rr * 0.25f + (float)Math.Cos(a) * rr * 0.92f;
                float y1 = cy - rr * 0.25f + (float)Math.Sin(a) * rr * 0.92f;
                g.DrawLine(p, x1, y1, x1 + rr * 0.75f, y1 + rr * 0.75f);
            }
            g.Restore(gs);
        }
        private void DrawClear(Graphics g, RectangleF r)
        {
            if (r.Width <= 2f) return;
            int state = _clearPress ? 2 : _clearHover ? 1 : 0;
            HEditFrame.DrawButton(g, r, _style, state, Enabled, 0, HSpinLayout.EndVertical);
            Color glyph = HEditFrame.GlyphColor(_style, state, Enabled);
            var area = RectangleF.Inflate(r, -r.Width * 0.3f, -r.Height * 0.3f);
            // 叉号在比例 1 时约占清除带半宽的 0.32，过大时收到带内，避免越过边框
            float u = Math.Min(r.Width, r.Height);
            float sc = Math.Min(_iconScale, Math.Max(0.5f, (u * 0.5f - 1f) / (0.32f * u * 0.5f)));
            GraphicsState gs = g.Save();
            g.TranslateTransform(area.X + area.Width / 2f, area.Y + area.Height / 2f);
            g.ScaleTransform(sc, sc);
            g.TranslateTransform(-(area.X + area.Width / 2f), -(area.Y + area.Height / 2f));
            HArrowGlyph.Draw(g, area, HArrowType.Multiply, HArrowStyle.Flat,
                HArrowDirection.Right, glyph, 0f);
            g.Restore(gs);
        }
        protected override bool AdornmentMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return false;
            LayoutAdorn();
            if (ClearVisible && _clearRect.Contains(e.Location))
            {
                _clearPress = true;
                Capture = true;
                Invalidate();
                return true;
            }
            if (_iconRect.Contains(e.Location))
            {
                _iconHover = true;
                SearchNow();
                Invalidate();
                return true;
            }
            return false;
        }
        protected override void AdornmentMouseUp(MouseEventArgs e)
        {
            if (_clearPress)
            {
                _clearPress = false;
                Capture = false;
                if (ClearVisible && _clearRect.Contains(e.Location))
                {
                    Text = string.Empty;
                    Focus();
                }
                Invalidate();
            }
        }
        protected override void AdornmentMouseMove(MouseEventArgs e)
        {
            LayoutAdorn();
            bool ih = _iconRect.Contains(e.Location);
            bool ch = ClearVisible && _clearRect.Contains(e.Location);
            if (ih != _iconHover || ch != _clearHover)
            {
                _iconHover = ih;
                _clearHover = ch && !_clearPress || (_clearPress && ch);
                Cursor = (ih || ch) ? Cursors.Hand : Cursors.IBeam;
                Invalidate();
            }
        }
        protected override void AdornmentMouseLeave()
        {
            if (_iconHover || _clearHover)
            {
                _iconHover = _clearHover = false;
                Cursor = Cursors.Default;
                Invalidate();
            }
        }
        #endregion
    }
}