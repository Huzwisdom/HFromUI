using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;
using HFromUI.HControl.Chart.Tips;
using HFromUI.HEnum;

namespace HFromUI.HControl.Tools.Button
{
    /// <summary>
    /// 保持按下按钮：点击翻转 Checked 并保持按下，再点弹起（可做开始/暂停双态，配合 CheckedGlyph/CheckedImage）。
    /// 选中期间使用按下态配色；风格/矢量图标/外框形状能力与 HPushButton 共用同一个外观引擎，直接继承 HButtonBase。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("CheckedChanged")]
    public class HToggleButton : HButtonBase
    {
        private readonly HPushButtonEngine _engine;
        private bool _checked;

        public HToggleButton()
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

        /// <summary>保持按下态：点击翻转，选中期间按按下态配色。</summary>
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("选中态"), HDescriptionLanguage("选中态（保持按下，再点弹起）"), Browsable(true)]
        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                _engine.SetChecked(value);
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>选中态变化事件。</summary>
        public event EventHandler CheckedChanged;

        [HCategoryLanguage("按钮"), HDisplayNameLanguage("外框形状"), HDescriptionLanguage("外框形状：直角/圆角/胶囊/正圆"), Browsable(true)]
        [DefaultValue(HPushButtonShape.Round)]
        public HPushButtonShape Shape { get => _engine.Shape; set => _engine.Shape = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("预设风格"), HDescriptionLanguage("预设风格（122 种：30 色 × 实心/描边/立体/渐变），Custom 时使用自定义三态色"), Browsable(true)]
        [DefaultValue(HPushButtonStyle.Primary)]
        public HPushButtonStyle ButtonStyle { get => _engine.ButtonStyle; set => _engine.ButtonStyle = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("常规状态图标"), HDescriptionLanguage("常规状态图标（矢量 Glyph 优先于它；另有悬停/选中专用图标）"), Browsable(true)]
        public Image Image { get => _engine.Image; set => _engine.Image = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("悬停时显示的图标"), HDescriptionLanguage("悬停时显示的图标（缺省回落到 Image）"), Browsable(true)]
        public Image HoverImage { get => _engine.HoverImage; set => _engine.HoverImage = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("选中时显示的图标"), HDescriptionLanguage("选中时显示的图标（双态按钮载体，如开始/暂停）"), Browsable(true)]
        public Image CheckedImage { get => _engine.CheckedImage; set => _engine.CheckedImage = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("内置矢量图标"), HDescriptionLanguage("内置矢量图标（129 种，不使用资源图片，颜色跟随文字色）；设置后优先于 Image"), Browsable(true)]
        [DefaultValue(HPushButtonGlyph.None)]
        public HPushButtonGlyph Glyph { get => _engine.Glyph; set => _engine.Glyph = value; }
        [HCategoryLanguage("按钮"), HDisplayNameLanguage("选中图标"), HDescriptionLanguage("保持按下选中后切换显示的矢量图标（如 播放↔暂停）"), Browsable(true)]
        [DefaultValue(HPushButtonGlyph.None)]
        public HPushButtonGlyph CheckedGlyph { get => _engine.CheckedGlyph; set => _engine.CheckedGlyph = value; }
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

        /// <summary>点击翻转选中态；DialogResult/PerformClick/助记符仍由 HButtonBase 统一处理。</summary>
        protected override void OnClick(EventArgs e)
        {
            if (Enabled) Checked = !Checked;
            base.OnClick(e);
        }

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
}
