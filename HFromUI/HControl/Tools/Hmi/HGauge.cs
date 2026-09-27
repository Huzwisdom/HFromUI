using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>汽车仪表（半圆/大弧）外形样式，共 23 种。</summary>
    public enum HGaugeStyle
    {
        Classic = 0,        // 经典汽车仪表（白盘红针）
        Sport = 1,          // 运动黑底
        Racing = 2,         // 赛车转速（碳纤+红线区）
        Digital = 3,        // 全液晶数字
        Segment = 4,        // 段码光柱
        Neon = 5,           // 霓虹发光
        Cyber = 6,          // 赛博朋克
        NightVision = 7,    // 夜视荧光绿
        Aviation = 8,       // 航空仪表
        Vintage = 9,        // 复古黄铜
        Minimal = 10,       // 极简细线
        HeavyDuty = 11,     // 重工厚框
        Steampunk = 12,     // 蒸汽朋克铆钉
        Flat = 13,          // 现代扁平
        Turbine = 14,       // 涡轮扇叶
        DualArc = 15,       // 双层彩弧
        ThinTech = 16,      // 细线科技
        Amber = 17,         // 琥珀色仪表
        PurpleHolo = 18,    // 紫色全息
        Tachometer = 19,    // 转速表（RPM）
        Speedometer = 20,   // 车速表（km/h）
        Marine = 21,        // 船用仪表
        Legacy = 22         // 旧版 HGauge 原样复刻（DimGray 五边形刻度、DodgerBlue 数字、Tomato 针、OrangeRed 虚线报警）
    }

    /// <summary>
    /// 汽车仪表盘控件：22 种外形 × 13 种色调，量程/单位/格式可配，
    /// 指针带缓动动画，支持报警红线区、自定义刻度文本、段码光柱与全液晶样式。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [HDescriptionLanguage("汽车仪表控件：22 种外形、13 种色调，支持缓动指针与报警红线区")]
    public class HGauge : HInstrumentBase
    {
        private HGaugeStyle _style = HGaugeStyle.Classic;
        private bool _showAlarmZone;
        private double _alarmRatio = 0.8d;
        private Color _needleColor = Color.Empty;
        private string _scaleLabels = string.Empty;

        /// <summary>默认尺寸按汽车半圆仪表比例（约 3:2）。</summary>
        public HGauge()
        {
            Size = new Size(240, 168);
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("汽车仪表外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HGaugeStyle.Classic)]
        public HGaugeStyle GaugeStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>是否显示报警红线区（弧尾端红色区段）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("报警红线区"), HDescriptionLanguage("是否显示弧尾端报警红线区"), Browsable(true)]
        [DefaultValue(false)]
        public bool ShowAlarmZone
        {
            get => _showAlarmZone;
            set { _showAlarmZone = value; Invalidate(); }
        }

        /// <summary>报警区起点在全量程中的比例（0..1）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("报警起点比例"), HDescriptionLanguage("报警红线区起点占全量程比例（0..1）"), Browsable(true)]
        [DefaultValue(0.8d)]
        public double AlarmRatio
        {
            get => _alarmRatio;
            set { _alarmRatio = Math.Max(0d, Math.Min(1d, value)); Invalidate(); }
        }

        /// <summary>指针自定义颜色；Empty 使用当前色调方案的指针色。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("指针颜色"), HDescriptionLanguage("指针自定义颜色，未设置使用色调方案指针色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color NeedleColor
        {
            get => _needleColor;
            set { _needleColor = value; Invalidate(); }
        }

        /// <summary>自定义刻度文本，分号分隔（如 0;20;40;60;80;100）；为空时按量程自动生成。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("自定义刻度文本"), HDescriptionLanguage("分号分隔的自定义刻度文本，为空按量程自动生成"), Browsable(true)]
        [DefaultValue("")]
        public string ScaleLabels
        {
            get => _scaleLabels;
            set { _scaleLabels = value ?? string.Empty; Invalidate(); }
        }

        // 每种外形的绘制参数
        private sealed class G
        {
            public bool Dark;
            public Color Acc;
            public bool CustomAcc;
            public int Needle = 0;          // 指针种类，-1 无指针
            public int TickMode = 0;        // 0 全刻度、1 仅主刻度、2 圆点、3 无刻度
            public int ArcMode = 1;         // 0 无彩弧、1 数值弧、2 轨道+数值、3 段码、4 双弧
            public int LcdMode = 1;         // 0 无数字窗、1 中窗、2 大全屏液晶
            public bool Hatch;
            public bool Rivets;
            public bool Turbine;
            public bool Redline;
            public bool Flat;
            public float Bezel = 0.085f;    // 外圈占半径比
            public float Sweep = 240f;
            public int MinorEach = 5;       // 每主格小格数
            public bool SquareTicks;
        }

        private static G StyleOf(HGaugeStyle s)
        {
            var g = new G();
            switch (s)
            {
                case HGaugeStyle.Sport:
                    g.Dark = true; g.Needle = 4; g.ArcMode = 2; g.Hatch = true; g.Redline = true; break;
                case HGaugeStyle.Racing:
                    g.Dark = true; g.Needle = 4; g.ArcMode = 2; g.Hatch = true;
                    g.Redline = true; g.Bezel = 0.10f; g.MinorEach = 2; g.Rivets = true; break;
                case HGaugeStyle.Digital:
                    g.Dark = true; g.Needle = -1; g.TickMode = 2; g.ArcMode = 0;
                    g.LcdMode = 2; g.Bezel = 0.07f; break;
                case HGaugeStyle.Segment:
                    g.Dark = true; g.Needle = -1; g.TickMode = 1; g.ArcMode = 3;
                    g.LcdMode = 2; g.Bezel = 0.07f; break;
                case HGaugeStyle.Neon:
                    g.Dark = true; g.Needle = 1; g.ArcMode = 2; g.LcdMode = 1; break;
                case HGaugeStyle.Cyber:
                    g.Dark = true; g.Needle = 1; g.ArcMode = 3; g.LcdMode = 2;
                    g.Hatch = true; g.MinorEach = 2;
                    g.Acc = Color.FromArgb(0, 229, 255); g.CustomAcc = true; break;
                case HGaugeStyle.NightVision:
                    g.Dark = true; g.Needle = 3; g.ArcMode = 2;
                    g.Acc = Color.FromArgb(96, 255, 150); g.CustomAcc = true; break;
                case HGaugeStyle.Aviation:
                    g.Needle = 2; g.ArcMode = 0; g.Bezel = 0.09f; g.Sweep = 270f;
                    g.SquareTicks = true; g.MinorEach = 6; g.LcdMode = 1; break;
                case HGaugeStyle.Vintage:
                    g.Needle = 2; g.ArcMode = 0; g.Bezel = 0.12f;
                    g.Acc = Color.FromArgb(176, 128, 58); g.CustomAcc = true; g.Rivets = true; break;
                case HGaugeStyle.Minimal:
                    g.Needle = 3; g.TickMode = 1; g.ArcMode = 1; g.Bezel = 0.025f;
                    g.LcdMode = 0; g.Flat = true; break;
                case HGaugeStyle.HeavyDuty:
                    g.Needle = 0; g.ArcMode = 2; g.Bezel = 0.15f; g.Rivets = true;
                    g.LcdMode = 1; break;
                case HGaugeStyle.Steampunk:
                    g.Needle = 2; g.ArcMode = 0; g.Bezel = 0.13f; g.Rivets = true;
                    g.Turbine = true;
                    g.Acc = Color.FromArgb(186, 132, 60); g.CustomAcc = true; break;
                case HGaugeStyle.Flat:
                    g.Needle = 3; g.ArcMode = 2; g.Bezel = 0.04f; g.Flat = true;
                    g.LcdMode = 0; break;
                case HGaugeStyle.Turbine:
                    g.Dark = true; g.Needle = 1; g.ArcMode = 2; g.Turbine = true; break;
                case HGaugeStyle.DualArc:
                    g.Dark = true; g.Needle = 1; g.ArcMode = 4; break;
                case HGaugeStyle.ThinTech:
                    g.Needle = 1; g.TickMode = 1; g.ArcMode = 1; g.Bezel = 0.03f;
                    g.LcdMode = 1; break;
                case HGaugeStyle.Amber:
                    g.Dark = true; g.Needle = 3; g.ArcMode = 2;
                    g.Acc = Color.FromArgb(255, 176, 40); g.CustomAcc = true; break;
                case HGaugeStyle.PurpleHolo:
                    g.Dark = true; g.Needle = 1; g.ArcMode = 4;
                    g.Acc = Color.FromArgb(192, 128, 255); g.CustomAcc = true; break;
                case HGaugeStyle.Tachometer:
                    g.Dark = true; g.Needle = 4; g.ArcMode = 2; g.Redline = true;
                    g.MinorEach = 2; g.Hatch = true; g.Bezel = 0.09f; break;
                case HGaugeStyle.Speedometer:
                    g.Needle = 0; g.ArcMode = 2; g.MinorEach = 2; g.Bezel = 0.09f; break;
                case HGaugeStyle.Marine:
                    g.Needle = 2; g.ArcMode = 0; g.Bezel = 0.10f; g.SquareTicks = true;
                    g.Acc = Color.FromArgb(24, 78, 116); g.CustomAcc = true; break;
                default:
                    g.Needle = 0; g.ArcMode = 1; break;
            }
            if (s == HGaugeStyle.Digital || s == HGaugeStyle.Segment) { /* 全屏液晶 */ }
            return g;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 12 || Height < 12) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (_style == HGaugeStyle.Legacy)
            {
                PaintLegacy(g);
                return;
            }

            G st = StyleOf(_style);
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : Scheme;
            Color acc = st.CustomAcc ? st.Acc : sc.Accent;
            float from = 120f + (270f - st.Sweep) / 2f;   // 居中对称：240→150，270→135

            // 按扫描角包络拟合半径与圆心
            float minX = 99f, maxX = -99f, minY = 99f, maxY = -99f;
            for (int i = 0; i <= 72; i++)
            {
                var p = HHmiDraw.Polar(0, 0, 1f, from + st.Sweep * i / 72f);
                minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
            }
            float bottomReserve = string.IsNullOrEmpty(Text) ? 10f : 28f;
            float r = Math.Max(8f, Math.Min(
                (Width - 8f) / (maxX - minX + 2f * st.Bezel + 0.02f),
                (Height - bottomReserve - 4f) / (maxY - minY + st.Bezel + 0.02f)));
            float cx = Width / 2f - (minX + maxX) * 0.5f * r;
            float cy = 4f + st.Bezel * r - minY * r;
            float rOut = r;
            float bezel = st.Bezel * r;
            float rFace = Math.Max(4f, rOut - bezel);

            // 外圈与表盘
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

            float rTrack = rFace * 0.89f, trackW = Math.Max(3f, rFace * 0.105f);
            float rTickOut = rFace * 0.80f;

            // 报警红线区
            bool redline = _showAlarmZone || st.Redline;
            if (redline && st.ArcMode != 3)
            {
                float a0 = from + st.Sweep * (float)_alarmRatio;
                g.Arc(cx, cy, rTrack, trackW, sc.Alarm, a0, from + st.Sweep - a0);
            }

            // 彩弧 / 轨道
            float ratio = ShownRatio;
            Color trackC = st.Dark ? Color.FromArgb(58, 64, 72) : Color.FromArgb(214, 220, 228);
            if (st.ArcMode == 1 || st.ArcMode == 2 || st.ArcMode == 4)
            {
                if (st.ArcMode == 2 || st.ArcMode == 4)
                    g.Arc(cx, cy, rTrack, trackW, trackC, from, st.Sweep);
                if (st.Dark) g.Arc(cx, cy, rTrack, trackW * 2.4f, sc.Glow, from, st.Sweep * ratio);
                g.Arc(cx, cy, rTrack, trackW, acc, from, st.Sweep * ratio);
                if (st.ArcMode == 4)
                {
                    float rIn = rFace * 0.74f, wIn = Math.Max(2f, rFace * 0.045f);
                    g.Arc(cx, cy, rIn, wIn,
                        Color.FromArgb(st.Dark ? 90 : 120, acc), from, st.Sweep * Math.Max(0f, ratio - 0.18f));
                }
            }
            else if (st.ArcMode == 3)
            {
                int bars = Math.Max(10, SegmentCount * 4);
                float step = st.Sweep / bars;
                for (int i = 0; i < bars; i++)
                {
                    float t0 = (i + 0.18f) * step;
                    float t1 = (i + 0.82f) * step;
                    Color bc = (float)(i + 1) / bars <= ratio ? acc : trackC;
                    if (redline && (float)i / bars >= (float)_alarmRatio && (float)(i + 1) / bars <= ratio)
                        bc = sc.Alarm;
                    g.ArcFlat(cx, cy, rTrack, trackW, bc, from + t0, t1 - t0);
                }
            }

            // 刻度
            float majorLen = rFace * (st.SquareTicks ? 0.13f : 0.115f);
            float minorLen = rFace * 0.055f;
            int seg = Math.Max(1, SegmentCount);
            if (st.TickMode == 0 || st.TickMode == 1)
            {
                g.Ticks(cx, cy, rTickOut, from, st.Sweep, seg,
                    st.TickMode == 1 ? 0 : st.MinorEach,
                    majorLen, minorLen, st.SquareTicks ? sc.Text : sc.Scale,
                    HHmiDraw.HMixer(sc.Scale, sc.Face, 0.25f),
                    st.SquareTicks ? 2.6f : 1.8f, 1f);
            }
            else if (st.TickMode == 2)
            {
                for (int i = 0; i <= seg; i++)
                {
                    var dp = HHmiDraw.Polar(cx, cy, rTickOut, from + st.Sweep * i / seg);
                    g.Ell(dp.X, dp.Y, Math.Max(1.2f, rFace * 0.012f), Math.Max(1.2f, rFace * 0.012f), sc.Scale);
                }
            }

            // 刻度标签（移到指针之后，用表盘底色底板遮住扫过的指针，文字不压线）
            string[] custom = null;
            if (!string.IsNullOrEmpty(_scaleLabels))
                custom = _scaleLabels.Split(';');
            // 标签环压在最外轨道环（报警红线）上：刻度尖之外、底板遮住红线，文字不与刻度/指针重合
            float labelR = rTrack - rFace * 0.02f;

            // 涡轮装饰
            if (st.Turbine)
            {
                using (var p = new Pen(Color.FromArgb(st.Dark ? 70 : 90, acc), Math.Max(1f, rFace * 0.02f)))
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * 30f;
                        var p1 = HHmiDraw.Polar(cx, cy, rFace * 0.12f, a);
                        var p2 = HHmiDraw.Polar(cx, cy, rFace * 0.34f, a + 24f);
                        g.DrawLine(p, p1, p2);
                    }
            }

            // 指针
            float angle = from + st.Sweep * ratio;
            if (st.Needle >= 0 && st.LcdMode != 2)
            {
                Color nc = _needleColor == Color.Empty ? sc.Needle : _needleColor;
                g.Needle(cx, cy, angle, rFace * 0.70f, st.Needle, nc);
                g.Hub(cx, cy, Math.Max(3f, rFace * 0.062f), st.SquareTicks ? 2 : 0,
                    sc.Bezel, sc.BezelDark);
            }
            else if (st.Needle >= 0)
            {
                g.Hub(cx, cy, Math.Max(3f, rFace * 0.06f), 3,
                    _needleColor == Color.Empty ? sc.Needle : _needleColor, sc.BezelDark);
            }

            // 指针之上绘制刻度标签（底板吃针，文字不压线）
            if (labelR > rFace * 0.2f && st.TickMode != 3)
            {
                float em = Math.Max(5.5f, Math.Min(13f, rFace * 0.082f));
                using (var lf = HHmiDraw.Pf(Font.FontFamily, em, FontStyle.Regular))
                    g.TickLabels(cx, cy, labelR, from, st.Sweep, seg,
                        i =>
                        {
                            if (custom != null && custom.Length > 0)
                                return i < custom.Length ? custom[i].Trim() : null;
                            if (i % 2 != 0 && seg > 12) return null;
                            return FormatValue(SegmentValue(i));
                        }, lf, sc.Text, sc.Face);
            }

            // 数字窗
            float mid = from + st.Sweep / 2f;
            var dcp = HHmiDraw.Polar(cx, cy, rFace * 0.30f, mid + 180f);
            if (st.LcdMode == 1)
            {
                float bw = rFace * 0.62f, bh = Math.Max(14f, rFace * 0.20f);
                var box = new RectangleF(dcp.X - bw / 2f, dcp.Y - bh / 2f, bw, bh);
                float em = Math.Min(bh * 0.52f, bw * 0.20f);
                using (var df = HHmiDraw.Pf("Consolas", Math.Max(9.3f, em), FontStyle.Bold))
                    g.Lcd(box, ComposeReadout(st), df, st.Dark ? Color.FromArgb(10, 14, 18) : Color.FromArgb(20, 26, 34),
                        st.Dark ? acc : Color.FromArgb(220, 230, 240), sc.BezelDark);
            }
            else if (st.LcdMode == 2)
            {
                float bw = rFace * 0.92f, bh = Math.Max(20f, rFace * 0.34f);
                var box = new RectangleF(dcp.X - bw / 2f, dcp.Y - bh / 2f, bw, bh);
                using (var gp = HHmiDraw.RoundBox(box, 6f))
                using (var bb = new SolidBrush(Color.FromArgb(8, 12, 16)))
                {
                    g.FillPath(bb, gp);
                    using (var p = new Pen(Color.FromArgb(60, acc.R, acc.G, acc.B), 1.2f))
                        g.DrawPath(p, gp);
                }
                float em = Math.Min(bh * 0.56f, bw * 0.165f);
                using (var df = HHmiDraw.Pf("Consolas", Math.Max(12f, em), FontStyle.Bold))
                    g.CenterText(box, ComposeReadout(st), df, acc);
            }

            // 标题（Text）：盘外底部独立带，不压盘面任何元素
            if (!string.IsNullOrEmpty(Text))
            {
                var tb = new RectangleF(0, Height - 22f, Width, 20f);
                using (var tf = HHmiDraw.Pf(Font.FontFamily, 10f, FontStyle.Bold))
                    g.CenterText(tb, Text, tf,
                        st.Dark ? Color.FromArgb(60, 66, 74) : sc.Text);
            }
        }

        // 旧版 HGauge 原样复刻：透明底、DimGray 五边形刻度、DodgerBlue 数字、Tomato 五边形针、OrangeRed 虚线报警弧
        private void PaintLegacy(Graphics g)
        {
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.SmoothingMode = SmoothingMode.HighQuality;
            int width = Width, height = Height;
            if (height <= 35 || width <= 20) return;
            int seg = Math.Max(2, SegmentCount);
            float R = height - 30;
            double k = (double)(width - 40) / 2.0 / R;
            double angleRad = Math.Acos(Math.Max(0d, Math.Min(1d, k)));
            float deg = (float)(angleRad * 180.0 / Math.PI);
            float sweep = 180f - 2f * deg;
            var pivot = new PointF(width / 2f, height - 10f);

            var border = Color.DimGray;
            var fore = Color.DodgerBlue;
            var pointer = _needleColor == Color.Empty ? Color.Tomato : _needleColor;
            var alarm = Color.OrangeRed;
            int pointerSize = 5;

            string[] custom = null;
            if (!string.IsNullOrEmpty(_scaleLabels))
                custom = _scaleLabels.Split(';');
            float em = 16.5f;   // 旧版 8.25pt 在 150% DPI 下的物理像素字号

            var state0 = g.Save();
            g.TranslateTransform(pivot.X, pivot.Y);

            // 报警虚弧（下/上两段，默认量程 20%、80%）
            var rect2 = new RectangleF(-R - 5f, -R - 5f, R * 2f + 10f, R * 2f + 10f);
            using (var ap = new Pen(alarm, 3f) { DashStyle = DashStyle.Custom, DashPattern = new[] { 5f, 1f } })
            {
                g.DrawArc(ap, rect2, deg - 180f, sweep * 0.2f);
                g.DrawArc(ap, rect2, deg - 180f + sweep * 0.8f, sweep * 0.2f);
            }

            // 五边形主刻度 + 小刻度
            int half = R > 200 ? 3 : 2;
            int majorLen = R > 100 ? 12 : 8;
            float step = sweep / seg;
            using (var mark = new SolidBrush(border))
            using (var pen = new Pen(border, 1f))
            {
                g.RotateTransform(deg - 90f);
                for (int i = 0; i <= seg; i++)
                {
                    g.FillPolygon(mark, new[]
                    {
                        new PointF(-half, -R), new PointF(-half, -R + majorLen),
                        new PointF(0f, -R + majorLen + majorLen / 2f - 1f),
                        new PointF(half, -R + majorLen), new PointF(half, -R),
                        new PointF(-half, -R)
                    });
                    for (int j = 0; j < 10; j++)
                    {
                        g.RotateTransform(step / 10f);
                        if (i != seg)
                        {
                            if (j == 4)
                            {
                                pen.Width = 2f;
                                g.DrawLine(pen, 0f, -R, 0f, -R + 6f);
                                pen.Width = 1f;
                            }
                            else if (j != 9)
                                g.DrawLine(pen, 0f, -R, 0f, -R + 4f);
                        }
                    }
                }
                g.RotateTransform(-step);
                g.RotateTransform(deg - 90f);
            }

            // 刻度数字（半径 R-12-字高，200×80 居中矩形）
            float numR = R - 12f - em - 3f;
            using (var foreB = new SolidBrush(fore))
            using (var nf = HHmiDraw.Pf(FontFamily.GenericSansSerif, em))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int k2 = 0; k2 <= seg; k2++)
                {
                    double v = SegmentValue(k2);
                    double a = (180.0 - deg) - (v - MinValue) / (MaxValue - MinValue) * (180.0 - 2.0 * deg);
                    if (double.IsNaN(a)) continue;
                    float px = numR * (float)Math.Cos(a * Math.PI / 180.0);
                    float py = -numR * (float)Math.Sin(a * Math.PI / 180.0);
                    string s = custom != null && k2 < custom.Length ? custom[k2].Trim() : FormatValue(v);
                    g.DrawString(s, nf, foreB, new RectangleF(px - 100f, py - 40f, 200f, 80f), sf);
                }

                // 数值 + 单位（针下方）
                float fh = em + 3f;
                var vr = new RectangleF(-200f, -(R * 3f / 5f - 3f), 400f, fh);
                if (width > 200)
                    g.DrawString(FormatValue(ShownValue), nf, foreB, vr, sf);
                if (!string.IsNullOrEmpty(UnitText))
                    g.DrawString(UnitText, nf, foreB,
                        new RectangleF(-200f, vr.Y + fh, 400f, fh), sf);
            }

            // 五边形指针 + 中心圆
            float ratio = ShownRatio;
            g.RotateTransform(deg - 90f);
            g.RotateTransform(sweep * ratio);
            using (var pb = new SolidBrush(pointer))
            {
                g.FillEllipse(pb, -pointerSize, -pointerSize, pointerSize * 2, pointerSize * 2);
                float sh = pointerSize / 2 < 1 ? 1 : pointerSize / 2;
                g.FillPolygon(pb, new[]
                {
                    new PointF(pointerSize, 0), new PointF(sh, -R + 20),
                    new PointF(0, -R), new PointF(-sh, -R + 20),
                    new PointF(-pointerSize, 0)
                });
            }
            g.Restore(state0);
        }

        // 数字窗文本：数值 + 单位
        private string ComposeReadout(G st)
        {
            string v = FormatValue(ShownValue);
            if (string.IsNullOrEmpty(UnitText)) return v;
            if (st.LcdMode == 2) return v + " " + UnitText;
            return v + (UnitText.Length > 6 ? "" : " " + UnitText);
        }
    }
}
