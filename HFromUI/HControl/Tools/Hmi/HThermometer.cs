using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>温度计外形样式，共 23 种。</summary>
    public enum HThermometerStyle
    {
        Classic = 0,        // 经典玻璃温度计
        Lab = 1,            // 实验室刻度温度计
        Industrial = 2,     // 工业金属套管
        Thermowell = 3,     // 护套管式（开槽金属套）
        Outdoor = 4,        // 户外挂壁表
        Vintage = 5,        // 复古木牌温度计
        Medical = 6,        // 医用体温计（粗短）
        Refrigerator = 7,   // 冷库温度计（蓝白）
        Furnace = 8,        // 炉温计（火红渐变）
        Digital = 9,        // 电子温度计（数显探针）
        Segment = 10,       // 段码光柱
        Horizontal = 11,    // 横向玻璃温度计
        Neon = 12,          // 霓虹液柱
        Cyber = 13,         // 赛博朋克
        NightVision = 14,    // 夜视荧光绿
        Minimal = 15,       // 极简
        Flat = 16,          // 现代扁平
        DualTube = 17,      // 双管（背景量程管 + 液柱）
        Probe = 18,         // 探针式
        Amber = 19,         // 琥珀色
        PurpleHolo = 20,    // 紫色全息
        IceFire = 21,       // 冰火双色（蓝→红渐变）
        Legacy = 22         // 旧版 HThermometer 原样复刻（银灰玻璃管、双侧 ℃/°F 刻度、Tomato 液柱渐变球）
    }

    /// <summary>
    /// 温度计控件：22 种外形 × 13 种色调，竖/横两用，液柱缓动动画，
    /// 左右双单位刻度（默认 ℃/°F 自动换算），支持电子数显、段码、套管与冰火渐变。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [HDescriptionLanguage("温度计控件：22 种外形、13 种色调，液柱缓动、双单位刻度")]
    public class HThermometer : HInstrumentBase
    {
        private HThermometerStyle _style = HThermometerStyle.Classic;
        private string _rightUnitText = "°F";
        private double _rightGain = 1.8d;
        private double _rightOffset = 32d;

        /// <summary>默认量程 -20~60℃，8 段刻度。</summary>
        public HThermometer()
        {
            Size = new Size(64, 260);
            MinValue = -20d;
            MaxValue = 60d;
            UnitText = "℃";
            SegmentCount = 8;
            ValueFormat = "{0:0}";
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("温度计外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HThermometerStyle.Classic)]
        public HThermometerStyle ThermometerStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>右侧第二单位文本（默认 °F），为空不显示右刻度。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("第二单位"), HDescriptionLanguage("右侧第二单位文本（默认华氏度），为空不显示"), Browsable(true)]
        [DefaultValue("°F")]
        public string RightUnitText
        {
            get => _rightUnitText;
            set { _rightUnitText = value ?? string.Empty; Invalidate(); }
        }

        /// <summary>第二刻度增益：次值 = 主值 × 增益 + 偏移（℃→°F 为 1.8）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("第二刻度增益"), HDescriptionLanguage("第二刻度换算增益"), Browsable(true)]
        [DefaultValue(1.8d)]
        public double RightGain
        {
            get => _rightGain;
            set { _rightGain = value; Invalidate(); }
        }

        /// <summary>第二刻度偏移（℃→°F 为 32）。</summary>
        [HCategoryLanguage("仪表"), HDisplayNameLanguage("第二刻度偏移"), HDescriptionLanguage("第二刻度换算偏移"), Browsable(true)]
        [DefaultValue(32d)]
        public double RightOffset
        {
            get => _rightOffset;
            set { _rightOffset = value; Invalidate(); }
        }

        private sealed class T
        {
            public bool Dark;
            public Color Acc;
            public bool CustomAcc;
            public bool Horizontal;
            public bool Digital;
            public bool Segmented;
            public bool Sheath;
            public bool Plate;
            public bool Wood;
            public bool Flat;
            public bool Glow;
            public bool Gradient;       // 液柱纵向渐变
            public bool DualTube;
            public bool ProbeTip;
            public int Bulb = 1;         // 0 无泡圆底、1 圆球、2 探针尖
            public float TubeWidth = 1f;
        }

        private static T StyleOf(HThermometerStyle s)
        {
            var t = new T();
            switch (s)
            {
                case HThermometerStyle.Lab:
                    t.TubeWidth = 0.85f; break;
                case HThermometerStyle.Industrial:
                    t.Sheath = true; break;
                case HThermometerStyle.Thermowell:
                    t.Sheath = true; t.Dark = true; break;
                case HThermometerStyle.Outdoor:
                    t.Plate = true; break;
                case HThermometerStyle.Vintage:
                    t.Plate = true; t.Wood = true;
                    t.Acc = Color.FromArgb(176, 96, 48); t.CustomAcc = true; break;
                case HThermometerStyle.Medical:
                    t.TubeWidth = 1.35f; t.Flat = true;
                    t.Acc = Color.FromArgb(214, 96, 110); t.CustomAcc = true; break;
                case HThermometerStyle.Refrigerator:
                    t.Plate = true;
                    t.Acc = Color.FromArgb(52, 130, 210); t.CustomAcc = true; break;
                case HThermometerStyle.Furnace:
                    t.Gradient = true; t.Glow = true; t.Dark = true;
                    t.Acc = Color.FromArgb(232, 78, 34); t.CustomAcc = true; break;
                case HThermometerStyle.Digital:
                    t.Digital = true; t.Dark = true; t.Bulb = 2; t.ProbeTip = true; break;
                case HThermometerStyle.Segment:
                    t.Segmented = true; t.Dark = true; break;
                case HThermometerStyle.Horizontal:
                    t.Horizontal = true; break;
                case HThermometerStyle.Neon:
                    t.Glow = true; t.Dark = true; break;
                case HThermometerStyle.Cyber:
                    t.Dark = true; t.Glow = true; t.Segmented = true;
                    t.Acc = Color.FromArgb(0, 229, 255); t.CustomAcc = true; break;
                case HThermometerStyle.NightVision:
                    t.Dark = true; t.Glow = true;
                    t.Acc = Color.FromArgb(96, 255, 150); t.CustomAcc = true; break;
                case HThermometerStyle.Minimal:
                    t.Flat = true; t.TubeWidth = 0.8f; break;
                case HThermometerStyle.Flat:
                    t.Flat = true; break;
                case HThermometerStyle.DualTube:
                    t.DualTube = true; break;
                case HThermometerStyle.Probe:
                    t.ProbeTip = true; t.Bulb = 2; t.Sheath = true; break;
                case HThermometerStyle.Amber:
                    t.Dark = true; t.Glow = true;
                    t.Acc = Color.FromArgb(255, 176, 40); t.CustomAcc = true; break;
                case HThermometerStyle.PurpleHolo:
                    t.Dark = true; t.Glow = true;
                    t.Acc = Color.FromArgb(192, 128, 255); t.CustomAcc = true; break;
                case HThermometerStyle.IceFire:
                    t.Gradient = true;
                    t.Acc = Color.FromArgb(232, 78, 34); t.CustomAcc = true; break;
            }
            return t;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 16 || Height < 16) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            if (_style == HThermometerStyle.Legacy)
            {
                PaintLegacy(g, Width, Height);
                return;
            }

            T st = StyleOf(_style);
            if (st.Horizontal)
            {
                var state = g.Save();
                g.TranslateTransform(Width, 0f);
                g.RotateTransform(90f);
                PaintVertical(g, st, Height, Width);
                g.Restore(state);
            }
            else PaintVertical(g, st, Width, Height);
        }

        // 竖向绘制（Horizontal 样式经 90° 旋转后复用）
        private void PaintVertical(Graphics g, T st, float W, float H)
        {
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : Scheme;
            Color acc = st.CustomAcc ? st.Acc : sc.Accent;
            float ratio = ShownRatio;

            // 挂板/木牌底板
            if (st.Plate)
            {
                var plate = new RectangleF(3f, 6f, W - 6f, H - 12f);
                Color pc = st.Wood ? Color.FromArgb(128, 86, 46) : Color.FromArgb(238, 241, 245);
                Color pe = st.Wood ? Color.FromArgb(78, 50, 24) : Color.FromArgb(186, 193, 201);
                using (var gp = HHmiDraw.RoundBox(plate, 8f))
                using (var b = new LinearGradientBrush(plate,
                    HHmiDraw.HMixer(pc, Color.White, 0.14f),
                    HHmiDraw.HMixer(pc, Color.Black, 0.14f), 45f))
                {
                    g.FillPath(b, gp);
                    using (var p = new Pen(pe, 1.6f)) g.DrawPath(p, gp);
                }
            }

            // 标题（独占顶部一行，下方为 单位/读数窗 头部带，刻度数字在其下，互不重叠）
            float titleH = string.IsNullOrEmpty(Text) ? 0f : 18f;
            if (titleH > 0f)
                using (var tf = HHmiDraw.Pf(Font.FontFamily, Math.Max(9.3f, Math.Min(16f, W * 0.16f)), FontStyle.Bold))
                    g.CenterText(new RectangleF(0, 2f, W, 16f), Text, tf,
                        (st.Dark || (st.Plate && !st.Wood)) ? Color.FromArgb(60, 66, 74) : sc.Text);

            float topY = titleH + 30f;
            float bottomY = H - 26f;
            float cx = W * 0.5f;
            float bulbR = Math.Max(8f, Math.Min(W * 0.17f * st.TubeWidth, 24f));
            float th = Math.Max(3f, bulbR * 0.42f * Math.Min(1.35f, st.TubeWidth));

            // 电子温度计：方屏 + 探针
            if (st.Digital)
            {
                float bodyW = W * 0.72f;
                var body = new RectangleF(cx - bodyW / 2f, titleH + 10f, bodyW, H * 0.40f);
                using (var gp = HHmiDraw.RoundBox(body, 7f))
                using (var b = new SolidBrush(Color.FromArgb(30, 35, 42)))
                {
                    g.FillPath(b, gp);
                    using (var p = new Pen(sc.BezelDark, 1.4f)) g.DrawPath(p, gp);
                }
                var screen = new RectangleF(body.X + 5f, body.Y + 8f, body.Width - 10f, body.Height * 0.52f);
                string valText = FormatValue(ShownValue);
                float em = screen.Height * 0.72f;
                Font df = HHmiDraw.Pf("Consolas", em, FontStyle.Bold);
                while (g.MeasureString(valText, df).Width > screen.Width * 0.92f && em > 6f)
                {
                    df.Dispose();
                    em -= 0.5f;
                    df = HHmiDraw.Pf("Consolas", em, FontStyle.Bold);
                }
                using (df)
                    g.Lcd(screen, valText, df,
                        Color.FromArgb(8, 14, 16), acc, Color.FromArgb(60, acc.R, acc.G, acc.B));
                using (var uf = HHmiDraw.Pf(Font.FontFamily, 8.7f))
                    g.CenterText(new RectangleF(body.X, body.Bottom - 16f, body.Width, 14f),
                        UnitText, uf, Color.FromArgb(170, 178, 188));
                // 探针杆与尖
                float probeTop = body.Bottom - 2f;
                using (var b = new SolidBrush(sc.Bezel))
                    g.FillRectangle(b, cx - th * 0.8f, probeTop, th * 1.6f, H - probeTop - 14f);
                using (var b = new SolidBrush(HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.3f)))
                {
                    var tri = new[]
                    {
                        new PointF(cx - th * 0.8f, H - 14f), new PointF(cx + th * 0.8f, H - 14f),
                        new PointF(cx, H - 3f)
                    };
                    g.FillPolygon(b, tri);
                }
                return;
            }

            // 段码柱
            if (st.Segmented)
            {
                int bars = Math.Max(8, SegmentCount * 5);
                float usable = bottomY - topY;
                float gap = 2f;
                float bh = usable / bars - gap;
                Color trackC = Color.FromArgb(58, 64, 72);
                for (int i = 0; i < bars; i++)
                {
                    float y = bottomY - (i + 1) * (bh + gap);
                    var rr = new RectangleF(cx - th * 1.7f, y, th * 3.4f, bh);
                    Color bc = (float)(i + 1) / bars <= ratio ? acc : trackC;
                    using (var gp = HHmiDraw.RoundBox(rr, Math.Min(2f, bh / 2f)))
                    using (var b = new SolidBrush(bc))
                        g.FillPath(b, gp);
                }
                g.Ell(cx, bottomY + bulbR * 0.6f, bulbR * 1.05f, bulbR * 1.05f,
                    ratio > 0.02f ? acc : trackC);
                DrawScaleLabels(g, st, sc, acc, cx, topY, bottomY,
                    th * 1.7f + 4f, W, true);
                DrawHeader(g, st, sc, acc, W, titleH);
                return;
            }

            // 金属护套
            if (st.Sheath)
            {
                float jw = th * 2.6f;
                var jacket = new RectangleF(cx - jw / 2f, topY - 10f, jw, bottomY - topY + bulbR + 8f);
                using (var gp = HHmiDraw.RoundBox(jacket, jw / 2f))
                using (var b = new LinearGradientBrush(jacket,
                    HHmiDraw.HMixer(sc.Bezel, Color.White, 0.28f),
                    HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.32f), 0f))
                {
                    g.FillPath(b, gp);
                    using (var p = new Pen(sc.BezelDark, 1.3f)) g.DrawPath(p, gp);
                }
            }

            float tubeTop = topY;
            float tubeBottom = bottomY;
            var channel = new RectangleF(cx - th, tubeTop, th * 2f, tubeBottom - tubeTop + th * 2f);
            using (var channelPath = HHmiDraw.RoundBox(channel, th))
            {
                // 管槽底
                using (var b = new SolidBrush(st.Flat
                    ? (st.Dark ? Color.FromArgb(44, 49, 56) : Color.FromArgb(226, 230, 235))
                    : Color.FromArgb(238, 240, 243)))
                    g.FillPath(b, channelPath);

                // 球体（泡）
                float bulbCy = tubeBottom + bulbR * 0.66f;
                if (st.Bulb == 1) g.FaceDisk(cx, bulbCy, bulbR,
                    HHmiDraw.HMixer(acc, Color.White, 0.55f), HHmiDraw.HMixer(acc, Color.Black, 0.2f));

                // 液柱裁剪区 = 管槽 + 球
                float yv = tubeBottom - (tubeBottom - tubeTop) * ratio;
                using (var clip = new GraphicsPath())
                {
                    clip.AddPath(channelPath, false);
                    if (st.Bulb == 1) clip.AddEllipse(cx - bulbR, bulbCy - bulbR, bulbR * 2f, bulbR * 2f);
                    clip.FillMode = FillMode.Winding;
                    var state = g.Save();
                    g.SetClip(clip, CombineMode.Replace);

                    // 背景量程管（双管样式）
                    if (st.DualTube)
                        using (var b = new SolidBrush(Color.FromArgb(46, acc.R, acc.G, acc.B)))
                            g.FillRectangle(b, cx - th, tubeTop, th * 2f, tubeBottom + th * 2f - tubeTop);

                    if (st.Glow)
                        using (var b = new SolidBrush(Color.FromArgb(70, acc.R, acc.G, acc.B)))
                            g.FillRectangle(b, cx - th * 1.9f, yv - 2f, th * 3.8f,
                                tubeBottom + bulbR * 1.4f - yv);

                    // 液柱本体
                    Color liquid = acc;
                    Brush liquidBrush;
                    if (st.Gradient)
                        liquidBrush = new LinearGradientBrush(
                            new RectangleF(cx - th, tubeTop, th * 2f, tubeBottom - tubeTop),
                            Color.FromArgb(90, 160, 235), acc, 90f);
                    else
                        liquidBrush = new SolidBrush(liquid);
                    using (liquidBrush)
                    {
                        g.FillRectangle(liquidBrush, cx - th, yv, th * 2f, tubeBottom + th * 2f - yv);
                        if (st.Bulb == 1) g.FillEllipse(liquidBrush, cx - bulbR, bulbCy - bulbR, bulbR * 2f, bulbR * 2f);
                    }
                    // 液柱顶高光弧
                    using (var p = new Pen(Color.FromArgb(150, 255, 255, 255), Math.Max(1f, th * 0.5f)))
                        g.DrawLine(p, cx - th * 0.5f, yv, cx + th * 0.5f, yv);
                    g.Restore(state);
                }

                // 探针尖
                if (st.ProbeTip)
                {
                    using (var b = new SolidBrush(ratio > 0.02f ? acc : sc.BezelDark))
                    {
                        var tri = new[]
                        {
                            new PointF(cx - th * 0.9f, tubeBottom + th),
                            new PointF(cx + th * 0.9f, tubeBottom + th),
                            new PointF(cx, tubeBottom + th * 3.4f)
                        };
                        g.FillPolygon(b, tri);
                    }
                }

                // 管描边
                Color edge = st.Dark ? Color.FromArgb(120, 128, 138) : Color.FromArgb(158, 166, 176);
                using (var p = new Pen(edge, st.Flat ? 1.2f : 1.5f))
                    g.DrawPath(p, channelPath);
                if (st.Bulb == 1)
                    using (var p = new Pen(edge, 1.5f))
                        g.DrawEllipse(p, cx - bulbR, bulbCy - bulbR, bulbR * 2f, bulbR * 2f);
                // 玻璃高光
                if (!st.Flat)
                    using (var p = new Pen(Color.FromArgb(110, 255, 255, 255), Math.Max(1f, th * 0.34f)))
                        g.DrawLine(p, cx - th * 0.45f, tubeTop + th, cx - th * 0.45f, tubeBottom - th);
            }

            DrawScaleLabels(g, st, sc, acc, cx, topY, bottomY, th + 5f, W, st.Bulb == 1);
            DrawHeader(g, st, sc, acc, W, titleH);
        }

        // 左右刻度线与数字（右刻度按第二单位换算）
        private void DrawScaleLabels(Graphics g, T st, HHmiScheme sc, Color acc,
            float cx, float topY, float bottomY, float gap, float W, bool rightSide)
        {
            int seg = Math.Max(1, SegmentCount);
            bool dualRight = rightSide && !string.IsNullOrEmpty(_rightUnitText);
            float majorLen = Math.Min(12f, W * 0.10f);
            float rightLen = majorLen * (dualRight ? 0.62f : 1f);   // 双列数字时右侧刻度缩短给数字让位
            float minorLen = majorLen * 0.5f;
            float em = Math.Max(5.5f, Math.Min(9f, W * 0.105f));
            bool showNums = W > 46f;
            using (var pm = new Pen(sc.Scale, 1.3f))
            using (var pn = new Pen(HHmiDraw.HMixer(sc.Scale, sc.Face, 0.3f), 1f))
            using (var bf = new SolidBrush(st.Dark ? sc.Text : sc.Text))
            using (var sf = new StringFormat())
            using (var nf = HHmiDraw.Pf(Font.FontFamily, em))
            {
                Font nf2 = HHmiDraw.Pf(Font.FontFamily, em * 0.82f);   // 右列换算数字小一号，保证三位数不被裁
                try
                {
                // 右列：可用宽度 = 右刻度线终点到控件边缘，按最宽数字缩字号，杜绝贴边削字
                float rightX = cx + gap + rightLen + 1f;
                if (dualRight)
                {
                    float avail = W - rightX - 2f;
                    float maxW = 0f;
                    for (int k2 = 0; k2 <= seg; k2++)
                        maxW = Math.Max(maxW, g.MeasureString(
                            FormatValue(SegmentValue(k2) * _rightGain + _rightOffset), nf2).Width);
                    if (maxW > avail)
                    {
                        float em2 = Math.Max(5f, em * 0.82f * avail / maxW);
                        nf2.Dispose();
                        nf2 = HHmiDraw.Pf(Font.FontFamily, em2);
                    }
                }
                for (int i = 0; i <= seg * 5; i++)
                {
                    float ty = bottomY - (bottomY - topY) * i / (seg * 5);
                    bool major = i % 5 == 0;
                    float len = major ? majorLen : minorLen;
                    g.DrawLine(major ? pm : pn, cx - gap, ty, cx - gap - len, ty);
                    if (rightSide)
                        g.DrawLine(major ? pm : pn, cx + gap, ty, cx + gap + (major ? rightLen : minorLen), ty);
                    if (major && showNums)
                    {
                        int k = i / 5;
                        double v = SegmentValue(k);
                        sf.Alignment = StringAlignment.Far;
                        sf.LineAlignment = StringAlignment.Center;
                        g.DrawString(FormatValue(v), nf, bf,
                            new RectangleF(0, ty - 8f, cx - gap - len - 2f, 16f), sf);
                        if (dualRight)
                        {
                            // Far 对齐贴右缘（矩形起于右刻度线终点），三位数不裁切
                            g.DrawString(FormatValue(v * _rightGain + _rightOffset), nf2, bf,
                                new RectangleF(rightX, ty - 8f, W - rightX - 2f, 16f), sf);
                        }
                    }
                }
                }
                finally { nf2.Dispose(); }
            }
        }

        // 头部带：左单位 / 中读数窗 / 右单位，三列不相交，位于刻度数字上方
        private void DrawHeader(Graphics g, T st, HHmiScheme sc, Color acc, float W, float titleH)
        {
            float y = titleH + 3f;
            float uem = Math.Max(8f, Math.Min(12f, W * 0.11f));
            using (var uf = HHmiDraw.Pf(Font.FontFamily, uem, FontStyle.Bold))
            using (var sf = new StringFormat { LineAlignment = StringAlignment.Center })
            {
                sf.Alignment = StringAlignment.Near;
                g.DrawString(UnitText, uf, new SolidBrush(st.Dark ? acc : sc.Text),
                    new RectangleF(4f, y, W * 0.24f - 4f, 16f), sf);
                if (!string.IsNullOrEmpty(_rightUnitText))
                {
                    sf.Alignment = StringAlignment.Far;
                    g.DrawString(_rightUnitText, uf, new SolidBrush(
                            st.Dark ? Color.FromArgb(170, 178, 188) : sc.Text),
                        new RectangleF(W * 0.76f, y, W * 0.24f - 4f, 16f), sf);
                }
            }
            // 中读数窗
            float bw = Math.Min(62f, W * 0.46f);
            if (bw >= 34f)
            {
                var box = new RectangleF(W / 2f - bw / 2f, y + 0.5f, bw, 15f);
                using (var df = HHmiDraw.Pf("Consolas", 9.3f, FontStyle.Bold))
                    g.Lcd(box, FormatValue(ShownValue) + " " + UnitText, df,
                        st.Dark ? Color.FromArgb(8, 12, 16) : Color.FromArgb(20, 26, 34),
                        st.Dark ? acc : Color.FromArgb(220, 230, 240), sc.BezelDark);
            }
        }

        // 旧版 HThermometer 原样复刻：银灰玻璃管、双侧长刻度与 ℃/°F 双列数字、Tomato 液柱与底部渐变球
        private void PaintLegacy(Graphics g, float width, float height)
        {
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int seg = Math.Max(2, SegmentCount);
            float num = width * 0.1f;
            float num2 = width * 0.2f;
            float num3 = num + 40f;
            float num4 = height - num3 - 20f - num2 * 2f;
            if (num4 <= 8f) return;
            float cx = width / 2f;
            double span = MaxValue - MinValue;

            var colorBorder = Color.Silver;
            var colorCenter = Color.WhiteSmoke;
            var colorTmp = Color.Tomato;
            var colorTmpBack = Color.LightGray;

            var blend = new ColorBlend
            {
                Positions = new[] { 0f, 0.3f, 0.8f, 1f },
                Colors = new[] { colorCenter, colorBorder, colorCenter, colorBorder }
            };
            var tubeBrush = new LinearGradientBrush(
                new PointF(cx - num * 0.7f, 0f), new PointF(cx + num * 0.7f, 0f),
                colorBorder, colorCenter) { InterpolationColors = blend };
            using (tubeBrush)
            using (var backB = new SolidBrush(colorTmpBack))
            using (var foreB = new SolidBrush(Color.Black))   // 旧版刻度文字固定黑色
            using (var pen = new Pen(Color.Black, 1f))
            using (var nf = HHmiDraw.Pf(FontFamily.GenericSansSerif, 16.5f))   // 旧版 8.25pt 在 150% DPI 下的物理像素字号
            using (var sf = new StringFormat())
            {
                // 背景管形
                g.FillEllipse(backB, cx - num, num3 - num, num * 2f, num * 2f);
                g.FillEllipse(backB, cx - num2, num3 + num4, num2 * 2f, num2 * 2f);
                g.FillRectangle(backB, cx - num, num3, num * 2f, num4 + num2);

                // 双侧刻度与数字
                for (int i = 0; i <= seg; i++)
                {
                    float v = (float)SegmentValue(i);
                    float num6 = num4 - 10f - (v - (float)MinValue) / (float)span * (num4 - 10f) + num3;
                    if (i > 0)
                    {
                        for (int j = 1; j < 10; j++)
                        {
                            float num7 = (num4 - 10f) / seg / 10f * j;
                            float ext = j == 5 ? width * 0.14f : width * 0.1f;
                            g.DrawLine(pen, cx - num - ext, num6 + num7, cx - num - 4f, num6 + num7);
                            g.DrawLine(pen, cx + num + ext, num6 + num7, cx + num + 4f, num6 + num7);
                        }
                    }
                    g.DrawLine(pen, cx - num - width * 0.3f, num6, cx - num - 4f, num6);
                    g.DrawLine(pen, cx + num + 4f, num6, cx + num + width * 0.3f, num6);

                    sf.Alignment = StringAlignment.Far;
                    g.DrawString(_rightUnitText, nf, foreB, new RectangleF(5f, 0f, width - 10f, num3 - num), sf);
                    g.DrawString(v.ToString("0.##"), nf, foreB,
                        new RectangleF(0f, num6 - nf.Height - 1f, cx - num - width * 0.14f, nf.Height + 1f), sf);
                    sf.Alignment = StringAlignment.Near;
                    g.DrawString(UnitText, nf, foreB, new RectangleF(5f, 0f, width - 10f, num3 - num), sf);
                    g.DrawString(Math.Round(v * _rightGain + _rightOffset).ToString("0"), nf, foreB,
                        new RectangleF(cx + num + width * 0.1f + 5f, num6 - nf.Height - 1f, width * 0.4f, nf.Height + 1f), sf);
                }

                // 银灰管身
                g.FillEllipse(tubeBrush, cx - num * 0.7f, num3 - num * 0.7f, num * 1.4f, num * 1.4f);
                g.FillRectangle(tubeBrush, cx - num * 0.7f, num3, num * 1.4f, num4 + num2);

                // 液柱
                float num8 = num4 - 10f - ((float)ShownValue - (float)MinValue) / (float)span * (num4 - 10f) + num3;
                num8 = Math.Max(num3 - 5f, Math.Min(num3 + num4 + 5f, num8));
                blend.Colors = new[] { colorTmp, colorTmp, colorCenter, colorTmp };
                tubeBrush.InterpolationColors = blend;
                g.FillRectangle(tubeBrush, cx - num * 0.7f, num8, num * 1.4f, num4 + num2 + num3 - num8);
                using (var tb = new SolidBrush(colorTmp))
                    g.FillEllipse(tb, cx - num * 0.7f, num8 - 1f, num * 1.4f, 3f);

                // 底部渐变球
                var rect = new RectangleF(cx - num2 * 0.85f, num3 + num4 + num2 * 0.15f, num2 * 1.7f, num2 * 1.7f);
                using (var gp = new GraphicsPath())
                {
                    gp.AddEllipse(rect);
                    using (var pgb = new PathGradientBrush(gp))
                    {
                        pgb.CenterPoint = new PointF(cx + num2 * 0.2f, num3 + num4 + num2 * 0.7f);
                        pgb.InterpolationColors = new ColorBlend
                        {
                            Positions = new[] { 0f, 1f },
                            Colors = new[] { colorTmp, colorCenter }
                        };
                        g.FillEllipse(pgb, rect);
                    }
                    using (var pp = new Pen(colorTmp))
                        g.DrawEllipse(pp, rect);
                }
            }
        }
    }
}
