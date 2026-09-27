using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Base
{
    /// <summary>
    /// 工具控件统一色调：Classic 为各控件自定义经典配色，1..12 与瓶/电机/阀门共用
    /// <see cref="HToolPalettes"/> 现代调色板。
    /// </summary>
    public enum HToolTheme
    {
        Classic = 0,   // 经典（各控件自定义工业配色）
        SkyBlue = 1,   // 天蓝
        SeaBlue = 2,   // 海蓝
        Emerald = 3,   // 翠绿
        Teal = 4,      // 墨青
        Amber = 5,     // 琥珀
        Orange = 6,    // 橙
        Rose = 7,      // 玫瑰
        Red = 8,       // 红
        Purple = 9,    // 紫
        Magenta = 10,  // 品红
        Coffee = 11,   // 咖啡
        Graphite = 12  // 石墨
    }

    /// <summary>
    /// 带动画的工具图元基类：继承 <see cref="HLabelBase"/>，统一透明底、双缓冲自绘、
    /// 13 色调色板与 Running 开关。Running 为 true 时按固定节拍推进 <see cref="SpinAngle"/>
    /// （旋转部件）与 <see cref="Phase"/>（流动/往返相位），派生类重写 OnPaint 即可。
    /// </summary>
    public abstract class HToolAnimBase : HLabelBase
    {
        private readonly Timer _timer;
        private HToolTheme _theme = HToolTheme.Classic;
        private HTextPlacement _textPlacement = HTextPlacement.Bottom;
        private bool _running;
        private float _spinStep = 12f;
        private float _phaseStep = 0.18f;

        protected HToolAnimBase()
        {
            AutoSize = false;
            BackColor = System.Drawing.Color.Transparent;
            _timer = new Timer { Interval = 50 };
            _timer.Tick += AnimTick;
        }

        /// <summary>色调，Classic 为各控件自身定义的经典工业配色。</summary>
        [HCategoryLanguage("工具"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调，Classic 为经典工业配色"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>运行状态：true 时旋转/流动/火焰等部件产生动画，false 时静止。</summary>
        [HCategoryLanguage("工具"), HDisplayNameLanguage("运行状态"), HDescriptionLanguage("运行状态，运行时图元产生动画"), Browsable(true)]
        [DefaultValue(false)]
        public bool Running
        {
            get => _running;
            set
            {
                if (_running == value) return;
                _running = value;
                if (value && IsHandleCreated && Visible) _timer.Start();
                else _timer.Stop();
                OnRunningChanged(EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>文字方位：上/下横排、左/右竖排（逐字正立）、Middle 居中覆盖，图元始终占主体。</summary>
        [HCategoryLanguage("工具"), HDisplayNameLanguage("文字方位"), HDescriptionLanguage("文字方位：上/下横排，左/右竖排，居中覆盖在图元中央"), Browsable(true)]
        [DefaultValue(HTextPlacement.Bottom)]
        public HTextPlacement TextPlacement
        {
            get => _textPlacement;
            set { _textPlacement = value; Invalidate(); }
        }

        /// <summary>按当前文字方位计算图元绘制区（无文字或居中覆盖时占满整个控件）。</summary>
        protected RectangleF PlacedGlyphArea()
            => HBarBase.PlacedGlyphArea(Width, Height, Text, Font, _textPlacement);

        /// <summary>按当前文字方位绘制文字（无文字不绘制）。</summary>
        protected void PaintPlacedText(Graphics g)
            => HBarBase.PaintPlacedText(g, Width, Height, Text, Font, ForeColor, _textPlacement);

        /// <summary>动画节拍间隔（毫秒）。</summary>
        [HCategoryLanguage("工具"), HDisplayNameLanguage("动画节拍间隔"), HDescriptionLanguage("动画节拍间隔（毫秒）"), Browsable(true)]
        [DefaultValue(50)]
        public int Interval
        {
            get => _timer.Interval;
            set { _timer.Interval = Math.Max(10, value); }
        }

        /// <summary>旋转部件当前角度（度）。</summary>
        protected float SpinAngle { get; private set; }

        /// <summary>流动/往返相位（弧度，0..2π 循环）。</summary>
        protected float Phase { get; private set; }

        /// <summary>每拍旋转角度增量。</summary>
        protected float SpinStep
        {
            get => _spinStep;
            set => _spinStep = value;
        }

        /// <summary>每拍相位增量。</summary>
        protected float PhaseStep
        {
            get => _phaseStep;
            set => _phaseStep = value;
        }

        /// <summary>当前调色板：Classic 取派生类经典配色，其余取现代色调表。</summary>
        protected HToolPalette Palette => _theme == HToolTheme.Classic
            ? ClassicPalette
            : HToolPalettes.Get((int)_theme);

        /// <summary>Classic 经典配色，派生类可覆盖（默认石墨灰工业色）。</summary>
        protected virtual HToolPalette ClassicPalette => HToolPalettes.Get((int)HToolTheme.Graphite);

        protected virtual void OnRunningChanged(EventArgs e) { }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_running && Visible && IsHandleCreated) _timer.Start();
            else _timer.Stop();
        }

        private void AnimTick(object sender, EventArgs e)
        {
            SpinAngle = (SpinAngle + _spinStep) % 360f;
            Phase += _phaseStep;
            if (Phase > (float)Math.PI * 2f) Phase -= (float)Math.PI * 2f;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}