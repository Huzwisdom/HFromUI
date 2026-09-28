using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.Editors
{
    using HFromUI.HLangage;
    using HFromUI.HControl.Base;

    /// <summary>前后缀文字的纵向摆放位置：上 / 中 / 下。</summary>
    public enum HAffixVerticalAlign
    {
        /// <summary>贴内容区顶部。</summary>
        Top = 0,
        /// <summary>内容区垂直居中（默认）。</summary>
        Middle = 1,
        /// <summary>贴内容区底部。</summary>
        Bottom = 2
    }

    /// <summary>
    /// 带前缀/后缀的纯自绘文本框（继承 HTextBoxBase）：
    /// 前缀、后缀为不可编辑的装饰文字，测量与绘制同基类一样走 GDI（TextAdvance + ExtTextOut），
    /// 通过装饰带扩展点让编辑区自动让位，插入符/选区/水印/滚动永远不会压到前后缀。
    /// 前缀或后缀为空字符串时宽度为 0、完全不占位；二者各自可独立设置字体、颜色，
    /// 纵向位置支持上/中/下三档；RightToLeft.Yes 时前缀与后缀随显示镜像交换左右（仅显示）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("TextChanged")]
    [HDescriptionLanguage("前后缀文本框：编辑区前后可带独立文字装饰，留空不占位；前后缀各自可设字体、颜色与上中下位置")]
    public class HAffixTextBox : HTextBoxBase
    {
        /// <summary>前后缀存在时与编辑区之间的固定间隔（像素）；留空时为 0，不给任何空间。</summary>
        private const int AffixGap = 4;

        // 与基类同一套口径：高度量用 TextRenderer（仅取高度），横向步进宽用 GDI TextAdvance
        private const TextFormatFlags TAffixMeasure =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        private string _prefix = string.Empty;
        private string _suffix = string.Empty;
        private Font _prefixFont;
        private Font _suffixFont;
        private Color _prefixColor = Color.Empty;
        private Color _suffixColor = Color.Empty;
        private HAffixVerticalAlign _prefixAlign = HAffixVerticalAlign.Middle;
        private HAffixVerticalAlign _suffixAlign = HAffixVerticalAlign.Middle;

        // 缓存的装饰带宽度（含间隔），属性/字体/句柄/DPI 变化时重算
        private int _prefixW;
        private int _suffixW;

        private Graphics _measureGraphics;
        private IntPtr _measureHdc;

        #region 前后缀属性

        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("前缀文字"), HDescriptionLanguage("前缀显示文字（不可编辑）；为空时前缀完全不占空间"), Browsable(true)]
        [DefaultValue("")]
        public string PrefixText
        {
            get => _prefix;
            set
            {
                value = value ?? string.Empty;
                if (_prefix == value) return;
                _prefix = value;
                RecalcAffix();
                RefreshLayout();
            }
        }

        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("后缀文字"), HDescriptionLanguage("后缀显示文字（不可编辑）；为空时后缀完全不占空间"), Browsable(true)]
        [DefaultValue("")]
        public string SuffixText
        {
            get => _suffix;
            set
            {
                value = value ?? string.Empty;
                if (_suffix == value) return;
                _suffix = value;
                RecalcAffix();
                RefreshLayout();
            }
        }

        /// <summary>前缀字体；未单独设置时跟随控件 Font（环境字体）。</summary>
        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("前缀字体"), HDescriptionLanguage("前缀的字体（含字号、粗斜体）；未设置时跟随文本框字体，可单独改字号"), Browsable(true)]
        public Font PrefixFont
        {
            get => _prefixFont ?? Font;
            set
            {
                if (ReferenceEquals(_prefixFont, value)) return;
                _prefixFont?.Dispose();
                _prefixFont = value;
                RecalcAffix();
                RefreshLayout();
            }
        }

        /// <summary>后缀字体；未单独设置时跟随控件 Font（环境字体）。</summary>
        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("后缀字体"), HDescriptionLanguage("后缀的字体（含字号、粗斜体）；未设置时跟随文本框字体，可单独改字号"), Browsable(true)]
        public Font SuffixFont
        {
            get => _suffixFont ?? Font;
            set
            {
                if (ReferenceEquals(_suffixFont, value)) return;
                _suffixFont?.Dispose();
                _suffixFont = value;
                RecalcAffix();
                RefreshLayout();
            }
        }

        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("前缀颜色"), HDescriptionLanguage("前缀文字颜色；未设置时跟随文本框 ForeColor"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color PrefixColor
        {
            get => _prefixColor;
            set
            {
                _prefixColor = value;
                Invalidate();
            }
        }

        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("后缀颜色"), HDescriptionLanguage("后缀文字颜色；未设置时跟随文本框 ForeColor"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color SuffixColor
        {
            get => _suffixColor;
            set
            {
                _suffixColor = value;
                Invalidate();
            }
        }

        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("前缀纵向位置"), HDescriptionLanguage("前缀文字的纵向位置：上 / 中（默认）/ 下"), Browsable(true)]
        [DefaultValue(HAffixVerticalAlign.Middle)]
        public HAffixVerticalAlign PrefixAlign
        {
            get => _prefixAlign;
            set { _prefixAlign = value; Invalidate(); }
        }

        [HCategoryLanguage("前后缀文本框"), HDisplayNameLanguage("后缀纵向位置"), HDescriptionLanguage("后缀文字的纵向位置：上 / 中（默认）/ 下"), Browsable(true)]
        [DefaultValue(HAffixVerticalAlign.Middle)]
        public HAffixVerticalAlign SuffixAlign
        {
            get => _suffixAlign;
            set { _suffixAlign = value; Invalidate(); }
        }

        // 设计器序列化：环境字体/环境颜色未显式设置时不生成代码
        private bool ShouldSerializePrefixFont() => _prefixFont != null;
        private void ResetPrefixFont() { _prefixFont?.Dispose(); _prefixFont = null; RecalcAffix(); RefreshLayout(); }
        private bool ShouldSerializeSuffixFont() => _suffixFont != null;
        private void ResetSuffixFont() { _suffixFont?.Dispose(); _suffixFont = null; RecalcAffix(); RefreshLayout(); }

        #endregion

        #region 装饰带布局

        /// <summary>RightToLeft.Yes 时仅显示镜像：前缀视觉靠右、后缀视觉靠左。</summary>
        private bool IsRtl => RightToLeft == RightToLeft.Yes;

        protected override int AdornmentLeftWidth => IsRtl ? _suffixW : _prefixW;
        protected override int AdornmentRightWidth => IsRtl ? _prefixW : _suffixW;

        /// <summary>重算前后缀装饰带宽：空文本严格为 0；非空为 GDI 步进宽 + 固定间隔。</summary>
        private void RecalcAffix()
        {
            _prefixW = MeasureBand(_prefix, PrefixFont);
            _suffixW = MeasureBand(_suffix, SuffixFont);
        }

        private int MeasureBand(string text, Font font)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            EnsureMeasureHdc();
            return HDrawPaint.TextAdvance(text, font, _measureHdc) + AffixGap;
        }

        #endregion

        #region 绘制

        protected override void DrawAdornments(Graphics g)
        {
            var cr = ContentRectBox;
            if (cr.Height <= 0) return;

            int leftW = Math.Max(0, AdornmentLeftWidth);
            int rightW = Math.Max(0, AdornmentRightWidth);
            var leftBand = new Rectangle(cr.X - leftW, cr.Y, leftW, cr.Height);
            var rightBand = new Rectangle(cr.Right, cr.Y, rightW, cr.Height);

            // RTL 镜像：前缀在右带贴右缘，后缀在左带贴左缘；LTR 反之
            IntPtr hdc = g.GetHdc();
            try
            {
                if (IsRtl)
                {
                    DrawAffix(hdc, _suffix, SuffixFont, ResolveColor(_suffixColor), _suffixAlign, leftBand, false);
                    DrawAffix(hdc, _prefix, PrefixFont, ResolveColor(_prefixColor), _prefixAlign, rightBand, true);
                }
                else
                {
                    DrawAffix(hdc, _prefix, PrefixFont, ResolveColor(_prefixColor), _prefixAlign, leftBand, false);
                    DrawAffix(hdc, _suffix, SuffixFont, ResolveColor(_suffixColor), _suffixAlign, rightBand, true);
                }
            }
            finally { g.ReleaseHdc(hdc); }
        }

        private Color ResolveColor(Color own)
            => !Enabled ? SystemColors.GrayText : (own.IsEmpty ? ForeColor : own);

        /// <summary>
        /// 在装饰带内绘制一段前后缀：文字始终贴“外侧”边缘（与边框间只隔基类内边距），
        /// AffixGap 留在靠编辑区一侧；纵向按上/中/下摆放。GDI 裁剪到带内，间隔给 overhang 留余量。
        /// </summary>
        private void DrawAffix(IntPtr hdc, string text, Font font, Color color,
            HAffixVerticalAlign align, Rectangle band, bool rightSide)
        {
            if (string.IsNullOrEmpty(text) || band.Width <= 0) return;

            int tw = HDrawPaint.TextAdvance(text, font, hdc);
            int rh = TextRenderer.MeasureText(HTranslation.GetContent("Ag中"), font, Size.Empty, TAffixMeasure).Height;
            int boxH = rh + 2; // 与基类行高口径一致（渲染高 +2）
            int x = rightSide ? band.Right - tw : band.Left;
            int y;
            if (align == HAffixVerticalAlign.Top)
                y = band.Y + 2;
            else if (align == HAffixVerticalAlign.Bottom)
                y = band.Bottom - rh - 2;
            else
                y = band.Y + (band.Height - boxH) / 2 + 1;

            using (var tc = HDrawPaint.BeginGdiText(hdc, font, band, IsRtl))
                tc.DrawRun(text, x, y, color);
        }

        #endregion

        #region 测量句柄与生命周期

        // 与基类同因：跨 DPI 显示器必须在控件自身窗口 DC 上计量，宽度才与实际绘制一致
        private void EnsureMeasureHdc()
        {
            if (_measureHdc != IntPtr.Zero || !IsHandleCreated) return;
            _measureGraphics = Graphics.FromHwnd(Handle);
            _measureHdc = _measureGraphics.GetHdc();
        }

        private void ReleaseMeasureHdc()
        {
            if (_measureGraphics == null) return;
            try { _measureGraphics.ReleaseHdc(_measureHdc); } catch { }
            _measureGraphics.Dispose();
            _measureGraphics = null;
            _measureHdc = IntPtr.Zero;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            RecalcAffix(); // 句柄就绪后改用窗口 DC 同 DPI 计量，随后基类 RefreshLayout 读到准确带宽
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ReleaseMeasureHdc();
            base.OnHandleDestroyed(e);
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            ReleaseMeasureHdc();
            RecalcAffix();
            base.OnDpiChangedAfterParent(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            RecalcAffix(); // 环境字体变化影响跟随字体的前后缀带宽
            base.OnFontChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseMeasureHdc();
                _prefixFont?.Dispose();
                _suffixFont?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
