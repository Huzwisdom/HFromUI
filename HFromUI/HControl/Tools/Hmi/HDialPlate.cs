using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>压力/流量类圆形表盘外形样式，共 23 种。</summary>
    public enum HDialPlateStyle
    {
        Classic = 0,        // 经典压力表（白盘铜芯）
        Industrial = 1,     // 工业黑边
        Vacuum = 2,         // 真空表
        Hydraulic = 3,      // 液压表（深盘）
        Oxygen = 4,         // 氧气表（白绿）
        Regulator = 5,      // 减压阀
        Barometer = 6,      // 气压计（黄铜）
        Digital = 7,        // 液晶数显
        Segment = 8,        // 段码环
        Neon = 9,           // 霓虹发光
        Cyber = 10,         // 赛博朋克
        NightVision = 11,    // 夜视荧光绿
        Minimal = 12,       // 极简
        HeavyDuty = 13,     // 厚法兰螺栓耳
        Steampunk = 14,     // 蒸汽朋克
        Flat = 15,          // 现代扁平
        Precision = 16,     // 精密校验表（镜面刻度带）
        DualScale = 17,     // 双刻度（MPa/psi）
        Marine = 18,        // 船用
        Amber = 19,        // 琥珀色
        PurpleHolo = 20,    // 紫色全息
        Aviation = 21,      // 航空舱压
        Legacy = 22         // 旧版 HDialPlate 原样复刻（黑粗框、浅灰内环、贝塞尔黑针、黄铜芯）
    }

    /// <summary>
    /// 压力/流量类圆形表盘控件：22 种外形 × 13 种色调，全圆/270° 刻度、
    /// 缓动指针、高/低报警色带、双刻度换算（如 MPa↔psi）与液晶读数。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [HDescriptionLanguage("压力类圆形表盘控件：22 种外形、13 种色调，支持报警色带与双刻度")]
    public class HDialPlate : HInstrumentBase
    {
        private HDialPlateStyle _style = HDialPlateStyle.Classic;
        private bool _showHighAlarm = true;
        private double _highAlarmRatio = 0.85d;
        private bool _showLowAlarm;
        private double _lowAlarmRatio = 0.15d;
        private Color _needleColor = Color.Empty;
        private string _scaleLabels = string.Empty;
        private string _secondUnitText = string.Empty;
        private double _secondGain = 1d;
        private double _secondOffset;

        /// <summary>默认 8 段刻度、一位小数。</summary>
        public HDialPlate()
        {
            Size = new Size(200, 200);
            SegmentCount = 8;
            ValueFormat = "{0:0.0}";
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("压力表盘外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HDialPlateStyle.Classic)]
        public HDialPlateStyle DialPlateStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>是否显示高端报警色带。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("高端报警色带"), HDescriptionLanguage("是否在刻度环上显示高端报警色带"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowHighAlarm
        {
            get => _showHighAlarm;
            set { _showHighAlarm = value; Invalidate(); }
        }

        /// <summary>高端报警起点占全量程比例（0..1）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("高端报警比例"), HDescriptionLanguage("高端报警色带起点占全量程比例"), Browsable(true)]
        [DefaultValue(0.85d)]
        public double HighAlarmRatio
        {
            get => _highAlarmRatio;
            set { _highAlarmRatio = Math.Max(0d, Math.Min(1d, value)); Invalidate(); }
        }

        /// <summary>是否显示低端报警色带。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("低端报警色带"), HDescriptionLanguage("是否在刻度环上显示低端报警色带"), Browsable(true)]
        [DefaultValue(false)]
        public bool ShowLowAlarm
        {
            get => _showLowAlarm;
            set { _showLowAlarm = value; Invalidate(); }
        }

        /// <summary>低端报警终点占全量程比例（0..1）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("低端报警比例"), HDescriptionLanguage("低端报警色带终点占全量程比例"), Browsable(true)]
        [DefaultValue(0.15d)]
        public double LowAlarmRatio
        {
            get => _lowAlarmRatio;
            set { _lowAlarmRatio = Math.Max(0d, Math.Min(1d, value)); Invalidate(); }
        }

        /// <summary>指针自定义颜色；Empty 使用色调方案指针色。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("指针颜色"), HDescriptionLanguage("指针自定义颜色，未设置使用色调方案指针色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color NeedleColor
        {
            get => _needleColor;
            set { _needleColor = value; Invalidate(); }
        }

        /// <summary>自定义刻度文本，分号分隔；为空按量程自动生成。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("自定义刻度文本"), HDescriptionLanguage("分号分隔的自定义刻度文本，为空按量程自动生成"), Browsable(true)]
        [DefaultValue("")]
        public string ScaleLabels
        {
            get => _scaleLabels;
            set { _scaleLabels = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>第二刻度单位（如 psi），为空不显示内圈刻度。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("第二刻度单位"), HDescriptionLanguage("第二刻度单位文本，为空不显示内圈刻度"), Browsable(true)]
        [DefaultValue("")]
        public string SecondUnitText
        {
            get => _secondUnitText;
            set { _secondUnitText = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>第二刻度换算增益：次刻度值 = 主值 × 增益 + 偏移。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("第二刻度增益"), HDescriptionLanguage("第二刻度换算增益，次值 = 主值 × 增益 + 偏移"), Browsable(true)]
        [DefaultValue(1d)]
        public double SecondGain
        {
            get => _secondGain;
            set { _secondGain = value; Invalidate(); }
        }

        /// <summary>第二刻度换算偏移。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("第二刻度偏移"), HDescriptionLanguage("第二刻度换算偏移，次值 = 主值 × 增益 + 偏移"), Browsable(true)]
        [DefaultValue(0d)]
        public double SecondOffset
        {
            get => _secondOffset;
            set { _secondOffset = value; Invalidate(); }
        }

        private sealed class D
        {
            public bool Dark;
            public Color Acc;
            public bool CustomAcc;
            public int Needle = 0;
            public int TickMode = 0;        // 0 全刻度、1 仅主刻度、2 圆点、3 无
            public int RingMode;            // 0 无环、1 数值细环、2 段码环
            public int LcdMode = 1;         // 0 无、1 小窗、2 大液晶
            public bool Hatch;
            public bool Rivets;
            public bool Lugs;
            public bool Flat;
            public bool Mirror;
            public bool Dual;
            public float Bezel = 0.085f;
            public float Sweep = 270f;      // 270 工业表或 360 全圆
            public int MinorEach = 5;
            public bool SquareTicks;
        }

        private static D StyleOf(HDialPlateStyle s)
        {
            var d = new D();
            switch (s)
            {
                case HDialPlateStyle.Industrial:
                    d.Bezel = 0.10f; d.SquareTicks = true; d.LcdMode = 1; break;
                case HDialPlateStyle.Vacuum:
                    d.Acc = Color.FromArgb(96, 128, 196); d.CustomAcc = true; d.LcdMode = 0; break;
                case HDialPlateStyle.Hydraulic:
                    d.Dark = true; d.Needle = 4; d.Bezel = 0.10f; d.RingMode = 1; break;
                case HDialPlateStyle.Oxygen:
                    d.Acc = Color.FromArgb(48, 150, 110); d.CustomAcc = true; d.SquareTicks = true;
                    d.LcdMode = 0; d.Bezel = 0.09f; break;
                case HDialPlateStyle.Regulator:
                    d.SquareTicks = true; d.Bezel = 0.11f; d.Rivets = true; d.LcdMode = 0; break;
                case HDialPlateStyle.Barometer:
                    d.Acc = Color.FromArgb(176, 128, 58); d.CustomAcc = true; d.Bezel = 0.12f;
                    d.Rivets = true; d.Sweep = 360f; d.LcdMode = 0; break;
                case HDialPlateStyle.Digital:
                    d.Dark = true; d.Needle = -1; d.TickMode = 2; d.LcdMode = 2; d.Bezel = 0.07f; break;
                case HDialPlateStyle.Segment:
                    d.Dark = true; d.Needle = -1; d.TickMode = 1; d.RingMode = 2;
                    d.LcdMode = 2; d.Bezel = 0.07f; break;
                case HDialPlateStyle.Neon:
                    d.Dark = true; d.Needle = 1; d.RingMode = 1; break;
                case HDialPlateStyle.Cyber:
                    d.Dark = true; d.Needle = 1; d.RingMode = 2; d.Hatch = true; d.MinorEach = 2;
                    d.Acc = Color.FromArgb(0, 229, 255); d.CustomAcc = true; break;
                case HDialPlateStyle.NightVision:
                    d.Dark = true; d.Needle = 3; d.RingMode = 1;
                    d.Acc = Color.FromArgb(96, 255, 150); d.CustomAcc = true; break;
                case HDialPlateStyle.Minimal:
                    d.Needle = 3; d.TickMode = 1; d.Bezel = 0.025f; d.Flat = true; d.LcdMode = 1; break;
                case HDialPlateStyle.HeavyDuty:
                    d.Bezel = 0.13f; d.Lugs = true; d.Needle = 0; d.SquareTicks = true; break;
                case HDialPlateStyle.Steampunk:
                    d.Acc = Color.FromArgb(186, 132, 60); d.CustomAcc = true; d.Bezel = 0.13f;
                    d.Rivets = true; d.Sweep = 360f; d.LcdMode = 0; break;
                case HDialPlateStyle.Flat:
                    d.Needle = 3; d.RingMode = 1; d.Bezel = 0.04f; d.Flat = true; d.LcdMode = 0; break;
                case HDialPlateStyle.Precision:
                    d.Bezel = 0.09f; d.Mirror = true; d.Needle = 1; d.MinorEach = 10;
                    d.Sweep = 360f; d.LcdMode = 0; break;
                case HDialPlateStyle.DualScale:
                    d.Dual = true; d.Bezel = 0.09f; d.LcdMode = 1; break;
                case HDialPlateStyle.Marine:
                    d.Acc = Color.FromArgb(24, 78, 116); d.CustomAcc = true; d.SquareTicks = true;
                    d.Bezel = 0.10f; d.Sweep = 360f; d.LcdMode = 0; break;
                case HDialPlateStyle.Amber:
                    d.Dark = true; d.Needle = 3; d.RingMode = 1;
                    d.Acc = Color.FromArgb(255, 176, 40); d.CustomAcc = true; break;
                case HDialPlateStyle.PurpleHolo:
                    d.Dark = true; d.Needle = 1; d.RingMode = 1;
                    d.Acc = Color.FromArgb(192, 128, 255); d.CustomAcc = true; break;
                case HDialPlateStyle.Aviation:
                    d.SquareTicks = true; d.Bezel = 0.09f; d.MinorEach = 6; d.Sweep = 360f;
                    d.LcdMode = 0; break;
                default:
                    d.Sweep = 360f; d.Needle = 0; d.LcdMode = 0; break;
            }
            return d;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 14 || Height < 14) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (_style == HDialPlateStyle.Legacy)
            {
                PaintLegacy(g);
                return;
            }

            D st = StyleOf(_style);
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : Scheme;
            Color acc = st.CustomAcc ? st.Acc : sc.Accent;

            float from = st.Sweep >= 359f ? 90f : 135f;
            // 标题占盘外底部独立带，盘面整体上移，保证标题不压针、不压刻度
            float band = string.IsNullOrEmpty(Text) ? 4f : 22f;
            float size = Math.Min(Width, Height - band);
            float cx = Width / 2f, cy = (Height - band) / 2f;
            float rOut = size / 2f - 2f;
            float bezel = st.Bezel * rOut;
            float rFace = Math.Max(6f, rOut - bezel);

            // 法兰螺栓耳
            if (st.Lugs)
                for (int i = 0; i < 4; i++)
                {
                    var lp = HHmiDraw.Polar(cx, cy, rOut - bezel * 0.2f, i * 90f + 45f);
                    g.FaceDisk(lp.X, lp.Y, bezel * 0.62f, sc.Bezel, sc.BezelDark);
                    g.Ell(lp.X, lp.Y, bezel * 0.22f, bezel * 0.22f, sc.BezelDark);
                }

            if (st.Flat)
            {
                g.FaceDisk(cx, cy, rFace + 2f, sc.Face, sc.Face);
                using (var p = new Pen(sc.Bezel, 1.6f))
                    g.DrawEllipse(p, cx - rFace, cy - rFace, rFace * 2f, rFace * 2f);
            }
            else
            {
                g.BezelRing(cx, cy, rOut, Math.Max(2f, bezel), sc.Bezel, sc.BezelDark, sc.BezelDark);
                g.FaceDisk(cx, cy, rFace, sc.Face, st.Dark ? sc.Face : sc.FaceDark);
            }
            if (st.Hatch) g.FaceHatch(cx, cy, rFace * 0.98f, HatchStyle.DarkUpwardDiagonal, st.Dark ? 26 : 18);
            if (st.Rivets)
                for (int i = 0; i < 8; i++)
                {
                    var rp = HHmiDraw.Polar(cx, cy, (rOut + rFace) / 2f, i * 45f);
                    g.Rivet(rp.X, rp.Y, Math.Max(1.8f, bezel * 0.30f), sc.Bezel, sc.BezelDark);
                }

            float rTickOut = rFace * 0.82f;
            float majorLen = rFace * (st.SquareTicks ? 0.13f : 0.115f);
            float minorLen = rFace * 0.055f;
            int seg = Math.Max(1, SegmentCount);

            // 报警色带（刻度外侧环带）
            float bandR = rTickOut + majorLen + rFace * 0.025f;
            if (_showHighAlarm && st.RingMode != 2)
                g.ArcFlat(cx, cy, bandR, Math.Max(2f, rFace * 0.035f), sc.Alarm,
                    from + st.Sweep * (float)_highAlarmRatio,
                    st.Sweep * (1f - (float)_highAlarmRatio));
            if (_showLowAlarm && st.RingMode != 2)
                g.ArcFlat(cx, cy, bandR, Math.Max(2f, rFace * 0.035f), sc.Alarm,
                    from, st.Sweep * (float)_lowAlarmRatio);

            // 精密表镜面刻度带
            if (st.Mirror)
                using (var p = new Pen(Color.FromArgb(110, sc.Scale), 1f))
                    g.DrawEllipse(p, cx - rTickOut + majorLen, cy - rTickOut + majorLen,
                        (rTickOut - majorLen) * 2f, (rTickOut - majorLen) * 2f);

            // 数值环 / 段码环
            float ratio = ShownRatio;
            Color trackC = st.Dark ? Color.FromArgb(58, 64, 72) : Color.FromArgb(214, 220, 228);
            float rRing = rFace * 0.92f, ringW = Math.Max(2.4f, rFace * 0.06f);
            if (st.RingMode == 1)
            {
                if (st.Dark) g.Arc(cx, cy, rRing, ringW * 2.4f, sc.Glow, from, st.Sweep * ratio);
                g.Arc(cx, cy, rRing, ringW, acc, from, st.Sweep * ratio);
            }
            else if (st.RingMode == 2)
            {
                int bars = Math.Max(12, seg * 4);
                float step = st.Sweep / bars;
                for (int i = 0; i < bars; i++)
                {
                    Color bc = (float)(i + 1) / bars <= ratio ? acc : trackC;
                    g.ArcFlat(cx, cy, rRing, ringW, bc,
                        from + (i + 0.16f) * step, step * 0.68f);
                }
            }

            // 刻度
            if (st.TickMode == 0 || st.TickMode == 1)
                g.Ticks(cx, cy, rTickOut, from, st.Sweep, seg,
                    st.TickMode == 1 ? 0 : st.MinorEach,
                    majorLen, minorLen, st.SquareTicks ? sc.Text : sc.Scale,
                    HHmiDraw.HMixer(sc.Scale, sc.Face, 0.25f),
                    st.SquareTicks ? 2.6f : 1.8f, 1f);
            else if (st.TickMode == 2)
                for (int i = 0; i <= seg; i++)
                {
                    var dp = HHmiDraw.Polar(cx, cy, rTickOut, from + st.Sweep * i / seg);
                    g.Ell(dp.X, dp.Y, Math.Max(1.2f, rFace * 0.012f), Math.Max(1.2f, rFace * 0.012f), sc.Scale);
                }

            // 主刻度标签参数（标签在指针之后绘制，垫表盘底色吃针）
            string[] custom = null;
            if (!string.IsNullOrEmpty(_scaleLabels)) custom = _scaleLabels.Split(';');
            bool fullCircle = st.Sweep >= 359f;

            // 第二刻度内圈刻度线（与外圈主标签留出两圈间距）
            bool dual = st.Dual && !string.IsNullOrEmpty(_secondUnitText);
            float labelR = rTickOut - majorLen - rFace * (dual ? 0.085f : 0.13f);
            float em = Math.Max(5.5f, Math.Min(13f, rFace * 0.085f));
            float r2 = rFace * 0.50f;
            if (dual)
                using (var p = new Pen(HHmiDraw.HMixer(acc, sc.Face, 0.1f), 1.2f))
                    for (int i = 0; i <= seg; i++)
                    {
                        float a = from + st.Sweep * i / seg;
                        if (fullCircle && i == seg) continue;
                        g.DrawLine(p, HHmiDraw.Polar(cx, cy, r2, a),
                            HHmiDraw.Polar(cx, cy, r2 - rFace * 0.04f, a));
                    }

            // 指针
            float angle = from + st.Sweep * ratio;
            if (st.Needle >= 0 && st.LcdMode != 2)
            {
                Color nc = _needleColor == Color.Empty ? sc.Needle : _needleColor;
                g.Needle(cx, cy, angle, rFace * 0.72f, st.Needle, nc);
                g.Hub(cx, cy, Math.Max(3.5f, rFace * 0.07f), st.SquareTicks ? 2 : 0,
                    sc.Bezel, sc.BezelDark);
            }

            // 主刻度标签（底板遮住扫过的指针，文字不压线）
            using (var lf = HHmiDraw.Pf(Font.FontFamily, em, FontStyle.Regular))
                g.TickLabels(cx, cy, labelR, from, st.Sweep, seg,
                    i =>
                    {
                        if (fullCircle && i == seg) return null;   // 全圆表首尾重叠
                        if (custom != null && custom.Length > 0)
                            return i < custom.Length ? custom[i].Trim() : null;
                        if (i % 2 != 0 && seg > 12) return null;
                        return FormatValue(SegmentValue(i));
                    }, lf, sc.Text, sc.Face);

            // 第二刻度内圈标签（0.385r 内环，整数取整避免与外圈标签环相撞）
            if (dual)
                using (var f2 = HHmiDraw.Pf(Font.FontFamily, em * 0.78f, FontStyle.Regular))
                    g.TickLabels(cx, cy, r2 - rFace * 0.115f, from, st.Sweep, seg,
                        i => (fullCircle && i == seg) ? null
                            : Math.Round(SegmentValue(i) * _secondGain + _secondOffset, 1).ToString("0.##"),
                        f2, HHmiDraw.HMixer(acc, st.Dark ? Color.White : Color.Black, 0.25f), sc.Face);

            // 读数窗
            if (st.LcdMode == 1)
            {
                // 置于 270° 表盘的顶部无刻度开口内：针扫不到、不压刻度数字
                float bw = rFace * 0.52f, bh = Math.Max(16f, rFace * 0.18f);
                var box = new RectangleF(cx - bw / 2f, cy - rFace * 0.56f - bh / 2f, bw, bh);
                Color lcdBack = st.Dark ? Color.FromArgb(10, 14, 18) : Color.FromArgb(20, 26, 34);
                Color lcdFore = st.Dark ? acc : Color.FromArgb(220, 230, 240);
                using (var gp = HHmiDraw.RoundBox(box, Math.Min(6f, bh * 0.2f)))
                using (var bb = new SolidBrush(lcdBack))
                {
                    g.FillPath(bb, gp);
                    using (var p = new Pen(sc.BezelDark, 1.2f)) g.DrawPath(p, gp);
                }
                using (var df = HHmiDraw.Pf("Consolas", Math.Max(9.3f, bh * 0.44f), FontStyle.Bold))
                    g.CenterText(new RectangleF(box.X, box.Y + 1f, box.Width, box.Height * 0.62f),
                        FormatValue(ShownValue), df, lcdFore);
                if (!string.IsNullOrEmpty(UnitText))
                    using (var uf = HHmiDraw.Pf(Font.FontFamily, Math.Max(7.5f, bh * 0.21f), FontStyle.Regular))
                        g.CenterText(new RectangleF(box.X, box.Bottom - box.Height * 0.36f, box.Width, box.Height * 0.32f),
                            UnitText, uf, HHmiDraw.HMixer(lcdFore, st.Dark ? Color.Black : Color.White, 0.15f));
            }
            else if (st.LcdMode == 2)
            {
                float bw = rFace * 0.90f, bh = Math.Max(20f, rFace * 0.34f);
                var box = new RectangleF(cx - bw / 2f, cy + rFace * 0.06f - bh / 2f, bw, bh);
                using (var gp = HHmiDraw.RoundBox(box, 6f))
                using (var bb = new SolidBrush(Color.FromArgb(8, 12, 16)))
                {
                    g.FillPath(bb, gp);
                    using (var p = new Pen(Color.FromArgb(60, acc.R, acc.G, acc.B), 1.2f))
                        g.DrawPath(p, gp);
                }
                using (var df = HHmiDraw.Pf("Consolas", Math.Max(12f, bh * 0.50f), FontStyle.Bold))
                    g.CenterText(box, string.IsNullOrEmpty(UnitText)
                        ? FormatValue(ShownValue) : FormatValue(ShownValue) + " " + UnitText, df, acc);
            }

            // 标题：盘外底部独立带
            if (!string.IsNullOrEmpty(Text))
                using (var tf = HHmiDraw.Pf(Font.FontFamily, Math.Max(9.3f, em * 0.9f), FontStyle.Bold))
                    g.CenterText(new RectangleF(0f, Height - band + 1f, Width, band - 2f),
                        Text, tf, st.Dark ? Color.FromArgb(60, 66, 74) : sc.Text);
        }

        // 旧版 HDialPlate 原样复刻：白盘 + 黑粗框 + 浅灰内环 + 对侧刻度数字 + 贝塞尔黑针 + 黄铜轴芯
        private void PaintLegacy(Graphics g)
        {
            if (Width <= 15 || Height <= 15) return;
            int seg = Math.Max(1, SegmentCount);
            float w = Math.Min(Width, Height);
            float cx = Width / 2f, cy = w / 2f;   // 旧版圆心严格按 width 定（非 Height）

            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.HighQuality;
            var black = Color.FromArgb(0, 0, 0);
            var ringGray = Color.FromArgb(228, 229, 229);
            var smallGray = Color.Gray;
            var white = Color.White;
            var fore = Color.Black;   // 旧版刻度文字固定黑色

            // 旧版以 Point 为单位，150% DPI 下物理像素字号 = w/30 * 1.5 = w/20
            float em = w / 20f;
            var state = g.Save();
            g.TranslateTransform(cx, cy);

            // 白盘
            using (var bb = new SolidBrush(white))
                g.FillEllipse(bb, new RectangleF(-w / 2f + 1f, -w / 2f + 1f, w - 3f, w - 3f));

            // 浅灰内环
            using (var bp = new Pen(ringGray, w / 14f))
                g.DrawEllipse(bp, new RectangleF(-w / 2f + bp.Width / 2f, -w / 2f + bp.Width / 2f,
                    w - bp.Width - 1f, w - bp.Width - 1f));

            // 刻度（主刻度 0.365→0.40，中刻度 0.378，小刻度 0.39）
            Font font = HHmiDraw.Pf(FontFamily.GenericSansSerif, em);
            using (var brush = new SolidBrush(fore))
            using (var pen = new Pen(black, 1f))
            {
                float segAngle = 360f / seg;
                for (int i = 0; i < seg; i++)
                {
                    float val = (float)(MinValue + (MaxValue - MinValue) * i / seg);
                    float labelR = w * 0.36f - font.Height;
                    float a = segAngle * i;
                    g.RotateTransform(a);
                    g.DrawLine(pen, 0f, w * 0.365f, 0f, w * 0.4f);
                    pen.Color = smallGray;
                    for (int j = 0; j < 10; j++)
                    {
                        g.RotateTransform(segAngle / 10f);
                        if (j < 9)
                            g.DrawLine(pen, 0f, j == 4 ? w * 0.378f : w * 0.39f, 0f, w * 0.4f);
                    }
                    pen.Color = black;
                    g.RotateTransform(-segAngle);
                    g.RotateTransform(-a);
                    float lx = labelR * (float)Math.Cos((a + 90f) / 360.0 * 2.0 * Math.PI);
                    float ly = labelR * (float)Math.Sin((a + 90f) / 360.0 * 2.0 * Math.PI);
                    g.DrawString(val.ToString("0.##"), font, brush,
                        new RectangleF(lx - 100f, ly - font.Height / 2f, 200f, font.Height),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

                // 中心半透明灰盘（与旧版相同的双椭圆相交区域）
                using (var gp1 = new GraphicsPath())
                using (var gp2 = new GraphicsPath())
                {
                    gp1.AddEllipse(new RectangleF(-w / 2f + 1f, -w / 2f + 1f, w - 3f, w - 3f));
                    gp2.AddEllipse(new RectangleF(-w / 2f + 1f - w * 0.2f, -w / 2f + 1f - w * 0.2f, w - 3f, w - 3f));
                    using (var region = new Region(gp1))
                    {
                        region.Intersect(gp2);
                        using (var ab = new SolidBrush(Color.FromArgb(60, Color.Gray)))
                            g.FillRegion(ab, region);
                    }
                }

                // 黑粗外框
                using (var bp = new Pen(black, w / 20f))
                    g.DrawEllipse(bp, new RectangleF(-w / 2f + bp.Width / 2f, -w / 2f + bp.Width / 2f,
                        w - bp.Width - 1f, w - bp.Width - 1f));

                // 贝塞尔双片黑针（长端指向对侧数字）
                float needleA = (float)((ShownValue - MinValue) / (MaxValue - MinValue) * 360.0);
                g.RotateTransform(needleA);
                using (var nb = new SolidBrush(black))
                {
                    g.FillEllipse(nb, -w * 0.04f, -w * 0.04f, w * 0.08f, w * 0.08f);
                    for (int t = 0; t < 2; t++)
                    {
                        using (var path = new GraphicsPath())
                        {
                            path.AddLine(0f, w * 0.41f, w * 0.02f, 0f);
                            path.AddBezier(new PointF(w * 0.02f, 0f), new PointF(w * 0.03f, -w * 0.1f),
                                new PointF(w * 0.04f, -w * 0.11f), new PointF(w * 0.05f, -w * 0.13f));
                            path.AddBezier(new PointF(w * 0.05f, -w * 0.13f), new PointF(w * 0.06f, -w * 0.15f),
                                new PointF(w * 0.06f, -w * 0.15f), new PointF(w * 0.04f, -w * 0.15f));
                            path.AddLine(new PointF(w * 0.04f, -w * 0.15f), new PointF(-1f, -w * 0.15f));
                            path.CloseFigure();
                            g.FillPath(nb, path);
                        }
                        g.ScaleTransform(-1f, 1f);
                    }
                }
                g.RotateTransform(-needleA);

                // 黄铜渐变轴芯
                var hubRect = new RectangleF(-w * 0.022f, -w * 0.022f, w * 0.044f, w * 0.044f);
                using (var lgb = new LinearGradientBrush(
                    new PointF(w * 0.022f, w * 0.022f), new PointF(-w * 0.022f, -w * 0.022f),
                    Color.FromArgb(161, 141, 108), Color.FromArgb(255, 232, 190)))
                {
                    lgb.InterpolationColors = new ColorBlend
                    {
                        Colors = new[] { Color.FromArgb(161, 141, 108), Color.FromArgb(255, 232, 190), Color.FromArgb(161, 141, 108) },
                        Positions = new[] { 0f, 0.3f, 1f }
                    };
                    g.FillEllipse(lgb, hubRect);
                }

                // 单位文本（旧版固定位置）
                if (!string.IsNullOrEmpty(UnitText))
                    g.DrawString(UnitText, font, brush,
                        new RectangleF(w * 0.12f - 100f, w * (w / 5000f + 0.01f), 200f, font.Height),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            font.Dispose();
            g.Restore(state);
        }
    }
}
