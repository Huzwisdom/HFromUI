using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>时钟表盘外形样式，共 22 种。</summary>
    public enum HClockStyle
    {
        Classic = 0,        // 经典三针白盘
        Roman = 1,          // 罗马数字
        Modern = 2,         // 极简两针
        Digital = 3,        // 液晶全数字
        Flip = 4,           // 翻页钟
        Neon = 5,           // 霓虹
        Cyber = 6,          // 赛博朋克
        NightVision = 7,    // 夜视荧光绿
        Vintage = 8,        // 复古黄铜
        Pocket = 9,         // 怀表（吊环表冠）
        Wall = 10,          // 木质挂钟
        Sport = 11,         // 运动电子
        Binary = 12,        // 二进制灯列
        Square = 13,        // 方形仪表钟
        ArabicBig = 14,     // 大阿拉伯数字
        Railroad = 15,      // 轨道刻度
        Chronograph = 16,   // 三眼计时
        Marine = 17,        // 船钟
        Amber = 18,         // 琥珀色
        PurpleHolo = 19,    // 紫色全息
        Flat = 20,          // 现代扁平
        Minimal = 21        // 无线条款
    }

    /// <summary>
    /// 时钟控件：22 种表盘 × 13 种色调，实时走时（可停在指定时刻），
    /// 指针扫秒/跳秒、日期窗、罗马数字、二进制灯列、液晶/翻页/电子等形态。
    /// </summary>
    [DefaultProperty("Live")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("时钟控件：22 种表盘、13 种色调，实时走时与多种数字形态")]
    public class HClock : HLabelBase
    {
        private static readonly string[] Roman = { "XII", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI" };

        private readonly Timer _timer;
        private HClockStyle _style = HClockStyle.Classic;
        private HToolTheme _theme = HToolTheme.Classic;
        private bool _live = true;
        private System.DateTime _fixedTime = System.DateTime.Now;
        private bool _showSecond = true;
        private bool _smoothSecond;
        private bool _showDate = true;

        /// <summary>默认方形表盘。</summary>
        public HClock()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(170, 170);
            _timer = new Timer { Interval = 50 };
            _timer.Tick += delegate { if (!IsDisposed) Invalidate(); };
        }

        /// <summary>表盘样式。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("表盘样式"), HDescriptionLanguage("时钟表盘样式，共 22 种"), Browsable(true)]
        [DefaultValue(HClockStyle.Classic)]
        public HClockStyle ClockStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>图元色调。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>是否实时走时；false 时停在 <see cref="FixedTime"/>。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("实时走时"), HDescriptionLanguage("true 实时走时，false 显示指定时刻"), Browsable(true)]
        [DefaultValue(true)]
        public bool Live
        {
            get => _live;
            set { _live = value; Invalidate(); }
        }

        /// <summary>非实时模式下显示的时刻。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("指定时刻"), HDescriptionLanguage("非实时模式下显示的时刻"), Browsable(true)]
        public System.DateTime FixedTime
        {
            get => _fixedTime;
            set { _fixedTime = value; Invalidate(); }
        }

        /// <summary>是否显示秒针/秒数。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("秒针"), HDescriptionLanguage("是否显示秒针或秒数"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowSecond
        {
            get => _showSecond;
            set { _showSecond = value; Invalidate(); }
        }

        /// <summary>true 秒针连续扫秒，false 整秒跳动。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("扫秒"), HDescriptionLanguage("true 连续扫秒，false 整秒跳秒"), Browsable(true)]
        [DefaultValue(false)]
        public bool SmoothSecond
        {
            get => _smoothSecond;
            set { _smoothSecond = value; Invalidate(); }
        }

        /// <summary>是否显示日期窗/日期行。</summary>
        [HCategoryLanguage("时钟"), HDisplayNameLanguage("显示日期"), HDescriptionLanguage("是否显示日期窗或日期行"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowDate
        {
            get => _showDate;
            set { _showDate = value; Invalidate(); }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _timer.Start();
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && IsHandleCreated) _timer.Start();
            else _timer.Stop();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Stop(); _timer.Dispose(); }
            base.Dispose(disposing);
        }

        private sealed class C
        {
            public bool Dark;
            public bool Glow;
            public bool Digital;
            public bool Flip;
            public bool Binary;
            public bool Sport;
            public int Numerals;      // 0 阿拉伯、1 罗马、2 无
            public int TickSet;      // 0 全刻度、1 粗时刻、2 轨道点、3 无
            public int Ring;         // 0 普通、1 木框、2 厚金属、3 细边、4 无
            public bool SubDials;
            public bool DateWin = true;
            public bool Pocket;
            public bool Square;
            public bool Flat;
            public Color Acc;
            public bool CustomAcc;
            public bool SecondDefaultOn = true;
        }

        private static C StyleOf(HClockStyle s)
        {
            var c = new C();
            switch (s)
            {
                case HClockStyle.Roman: c.Numerals = 1; c.Ring = 2; break;
                case HClockStyle.Modern: c.Numerals = 2; c.TickSet = 3; c.Ring = 3;
                    c.SecondDefaultOn = false; break;
                case HClockStyle.Digital: c.Digital = true; c.Dark = true; c.Numerals = 2; break;
                case HClockStyle.Flip: c.Flip = true; c.Numerals = 2; c.TickSet = 3; break;
                case HClockStyle.Neon: c.Dark = true; c.Glow = true; c.Numerals = 2; break;
                case HClockStyle.Cyber: c.Dark = true; c.Glow = true;
                    c.Acc = Color.FromArgb(0, 229, 255); c.CustomAcc = true; break;
                case HClockStyle.NightVision: c.Dark = true; c.Glow = true;
                    c.Acc = Color.FromArgb(96, 255, 150); c.CustomAcc = true; break;
                case HClockStyle.Vintage: c.Ring = 2;
                    c.Acc = Color.FromArgb(176, 128, 58); c.CustomAcc = true; break;
                case HClockStyle.Pocket: c.Pocket = true; c.Numerals = 1; c.Ring = 2; break;
                case HClockStyle.Wall: c.Ring = 1; break;
                case HClockStyle.Sport: c.Digital = true; c.Dark = true; c.Sport = true; break;
                case HClockStyle.Binary: c.Binary = true; c.Dark = true; c.Numerals = 2; c.TickSet = 3; break;
                case HClockStyle.Square: c.Square = true; c.Ring = 2; break;
                case HClockStyle.ArabicBig: c.Numerals = 0; c.TickSet = 1; break;
                case HClockStyle.Railroad: c.TickSet = 2; c.Numerals = 0; break;
                case HClockStyle.Chronograph: c.SubDials = true; c.Ring = 2; c.Numerals = 1; break;
                case HClockStyle.Marine: c.Numerals = 0; c.Ring = 2;
                    c.Acc = Color.FromArgb(24, 78, 116); c.CustomAcc = true; break;
                case HClockStyle.Amber: c.Dark = true; c.Glow = true;
                    c.Acc = Color.FromArgb(255, 176, 40); c.CustomAcc = true; break;
                case HClockStyle.PurpleHolo: c.Dark = true; c.Glow = true;
                    c.Acc = Color.FromArgb(192, 128, 255); c.CustomAcc = true; break;
                case HClockStyle.Flat: c.Flat = true; c.Numerals = 0; c.Ring = 3; break;
                case HClockStyle.Minimal: c.Numerals = 2; c.TickSet = 1; c.Ring = 4;
                    c.SecondDefaultOn = false; break;
            }
            return c;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 14 || Height < 14) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            C st = StyleOf(_style);
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : HHmiPalettes.Get(Theme);
            Color acc = st.CustomAcc ? st.Acc : sc.Accent;
            System.DateTime dt = _live ? System.DateTime.Now : _fixedTime;

            if (st.Digital) { PaintDigital(g, st, sc, acc, dt, sport: st.Sport); return; }
            if (st.Flip) { PaintFlip(g, st, sc, acc, dt); return; }
            if (st.Binary) { PaintBinary(g, st, acc, dt); return; }

            float cx = Width / 2f, cy = Height / 2f;
            float r = Math.Min(Width, Height) / 2f - (st.Pocket ? 9f : 3f);

            // 怀表吊环与表冠
            if (st.Pocket)
            {
                using (var p = new Pen(sc.Bezel, 2.6f))
                    g.DrawArc(p, cx - 6f, 0f, 12f, 12f, 180f, 180f);
                g.FaceDisk(cx, 8f, 2.6f, sc.Bezel, sc.BezelDark);
            }

            // 外圈
            if (st.Ring == 1)
            {
                Color wood = Color.FromArgb(126, 82, 44), woodD = Color.FromArgb(78, 48, 24);
                using (var lgb = new LinearGradientBrush(new RectangleF(cx - r, cy - r, r * 2f, r * 2f),
                    HHmiDraw.HMixer(wood, Color.White, 0.18f), woodD, 50f))
                    g.FillEllipse(lgb, cx - r, cy - r, r * 2f, r * 2f);
            }
            else if (st.Ring == 2)
            {
                if (st.Square)
                {
                    var outer = new RectangleF(cx - r, cy - r, r * 2f, r * 2f);
                    using (var gp = HHmiDraw.RoundBox(outer, r * 0.18f))
                    using (var b = new LinearGradientBrush(outer,
                        HHmiDraw.HMixer(sc.Bezel, Color.White, 0.25f), sc.BezelDark, 55f))
                        g.FillPath(b, gp);
                }
                else
                    g.BezelRing(cx, cy, r, Math.Max(3f, r * 0.10f),
                        HHmiDraw.HMixer(sc.Bezel, Color.White, 0.25f), sc.BezelDark, sc.BezelDark);
            }
            else if (st.Ring == 3)
                using (var p = new Pen(st.Flat ? acc : sc.Bezel, 1.6f))
                    g.DrawEllipse(p, cx - r + 1, cy - r + 1, (r - 1) * 2f, (r - 1) * 2f);

            float rFace = st.Ring == 2 ? r - r * 0.10f : (st.Ring == 1 ? r - r * 0.12f : r - 2f);
            if (st.Square)
            {
                var sq = new RectangleF(cx - rFace, cy - rFace, rFace * 2f, rFace * 2f);
                using (var gp = HHmiDraw.RoundBox(sq, rFace * 0.16f))
                using (var b = new SolidBrush(sc.Face))
                    g.FillPath(b, gp);
            }
            else
                g.FaceDisk(cx, cy, rFace, sc.Face, st.Dark ? sc.Face : sc.FaceDark);

            // 刻度
            if (st.TickSet != 3)
            {
                using (var pm = new Pen(st.TickSet == 2 ? sc.Text : sc.Scale, st.TickSet == 2 ? 1.2f : 2.4f))
                using (var pn = new Pen(HHmiDraw.HMixer(sc.Scale, sc.Face, 0.2f), 1f))
                    for (int i = 0; i < 60; i++)
                    {
                        float a = i * 6f;
                        bool hour = i % 5 == 0;
                        float len = hour ? rFace * 0.10f : (st.TickSet == 0 || st.TickSet == 2 ? rFace * 0.045f : 0f);
                        if (len <= 0f) continue;
                        var pen = hour ? pm : (st.TickSet == 2 ? pm : pn);
                        g.DrawLine(pen, HHmiDraw.Polar(cx, cy, rFace * 0.96f, a),
                            HHmiDraw.Polar(cx, cy, rFace * 0.96f - len, a));
                        if (st.TickSet == 2 && !hour)
                        {
                            var dp = HHmiDraw.Polar(cx, cy, rFace * 0.935f, a);
                            g.Ell(dp.X, dp.Y, 0.9f, 0.9f, sc.Scale);
                        }
                    }
            }

            // 数字
            if (st.Numerals != 2)
            {
                float em = rFace * (st.Numerals == 1 ? 0.105f : 0.135f);
                using (var nf = HHmiDraw.Pf(Font.FontFamily, em, FontStyle.Bold))
                    for (int i = 1; i <= 12; i++)
                    {
                        string s = st.Numerals == 1 ? Roman[i % 12] : i.ToString();
                        var p = HHmiDraw.Polar(cx, cy, rFace * 0.74f, i * 30f);
                        var box = new RectangleF(p.X - rFace * 0.3f, p.Y - rFace * 0.12f,
                            rFace * 0.6f, rFace * 0.24f);
                        g.CenterText(box, s, nf, st.Dark ? sc.Text : (st.CustomAcc ? acc : sc.Text));
                    }
            }

            // 三眼装饰
            if (st.SubDials)
                for (int k = 0; k < 3; k++)
                {
                    float a = new[] { 270f, 0f, 180f }[k];
                    var sp = HHmiDraw.Polar(cx, cy, rFace * 0.42f, a);
                    float sr = rFace * 0.17f;
                    g.FaceDisk(sp.X, sp.Y, sr, HHmiDraw.HMixer(sc.Face, sc.FaceDark, 0.4f), sc.FaceDark);
                    using (var p = new Pen(sc.Scale, 1f))
                    {
                        g.DrawEllipse(p, sp.X - sr, sp.Y - sr, sr * 2f, sr * 2f);
                        for (int i = 0; i < 12; i++)
                        {
                            float ta = i * 30f;
                            g.DrawLine(p, HHmiDraw.Polar(sp.X, sp.Y, sr * 0.82f, ta),
                                HHmiDraw.Polar(sp.X, sp.Y, sr * 0.96f, ta));
                        }
                    }
                }

            // 日期窗（3 点位）
            if (_showDate && st.DateWin && !st.SubDials)
            {
                var wp = HHmiDraw.Polar(cx, cy, rFace * 0.56f, 90f);
                var win = new RectangleF(wp.X - rFace * 0.12f, wp.Y - rFace * 0.075f,
                    rFace * 0.24f, rFace * 0.15f);
                using (var gp = HHmiDraw.RoundBox(win, 2f))
                using (var b = new SolidBrush(st.Dark ? Color.FromArgb(10, 14, 18) : Color.White))
                    g.FillPath(b, gp);
                using (var df = HHmiDraw.Pf(Font.FontFamily, rFace * 0.085f, FontStyle.Bold))
                    g.CenterText(win, dt.Day.ToString(), df, st.Dark ? acc : sc.Text);
            }

            // 指针
            double sec = _smoothSecond ? dt.Second + dt.Millisecond / 1000.0 : dt.Second;
            float aSec = (float)(sec * 6.0);
            float aMin = dt.Minute * 6f + (float)sec * 0.1f;
            float aHour = (dt.Hour % 12) * 30f + dt.Minute * 0.5f;
            Color handC = st.Dark ? Color.FromArgb(232, 238, 246) : Color.FromArgb(34, 38, 44);
            DrawHand(g, cx, cy, rFace * 0.52f, rFace * 0.030f, aHour, handC);
            DrawHand(g, cx, cy, rFace * 0.74f, rFace * 0.022f, aMin, handC);
            bool secOn = _showSecond || (st.SecondDefaultOn && _showSecond);
            if (secOn)
            {
                if (st.Glow)
                    using (var p = new Pen(Color.FromArgb(90, acc.R, acc.G, acc.B), rFace * 0.03f))
                        g.DrawLine(p, HHmiDraw.Polar(cx, cy, rFace * 0.16f, aSec + 180f),
                            HHmiDraw.Polar(cx, cy, rFace * 0.78f, aSec));
                using (var p = new Pen(acc, Math.Max(1f, rFace * 0.012f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(p, HHmiDraw.Polar(cx, cy, rFace * 0.18f, aSec + 180f),
                        HHmiDraw.Polar(cx, cy, rFace * 0.80f, aSec));
                }
            }
            g.Hub(cx, cy, Math.Max(2.5f, rFace * 0.05f), st.Ring == 2 ? 0 : 3,
                st.CustomAcc ? acc : sc.Bezel, sc.BezelDark);
        }

        // 菱形/矛形时针分针
        private static void DrawHand(Graphics g, float cx, float cy, float len, float w, float angle, Color c)
        {
            var state = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);
            using (var b = new SolidBrush(c))
                g.FillPolygon(b, new[]
                {
                    new PointF(0, -len), new PointF(w, -len * 0.12f),
                    new PointF(w * 0.5f, len * 0.16f), new PointF(0, len * 0.26f),
                    new PointF(-w * 0.5f, len * 0.16f), new PointF(-w, -len * 0.12f)
                });
            g.Restore(state);
        }

        // 液晶/电子钟
        private void PaintDigital(Graphics g, C st, HHmiScheme sc, Color acc, System.DateTime dt, bool sport)
        {
            var box = new RectangleF(4f, 6f, Width - 8f, Height - 12f);
            using (var gp = HHmiDraw.RoundBox(box, sport ? 10f : 6f))
            {
                using (var b = new SolidBrush(sport ? Color.FromArgb(18, 22, 28) : Color.FromArgb(8, 12, 16)))
                    g.FillPath(b, gp);
                using (var p = new Pen(Color.FromArgb(70, acc.R, acc.G, acc.B), 1.2f))
                    g.DrawPath(p, gp);
            }

            string t = _showSecond
                ? string.Format("{0:00}:{1:00}:{2:00}", dt.Hour, dt.Minute, dt.Second)
                : string.Format("{0:00}:{1:00}", dt.Hour, dt.Minute);
            float tem = Math.Min(box.Width * 0.22f, box.Height * 0.34f);
            Font df = HHmiDraw.Pf("Consolas", tem, FontStyle.Bold);
            while (g.MeasureString(t, df).Width > box.Width * 0.94f && tem > 7f)
            {
                df.Dispose();
                tem -= 0.5f;
                df = HHmiDraw.Pf("Consolas", tem, FontStyle.Bold);
            }
            using (df)
                g.CenterText(new RectangleF(box.X, box.Y + box.Height * 0.12f, box.Width, box.Height * 0.5f),
                    t, df, acc);

            if (_showDate)
            {
                string d = sport
                    ? string.Format("{0:yyyy-MM-dd}  {1}", dt, "日一二三四五六"[dt.DayOfWeek == DayOfWeek.Sunday ? 0 : (int)dt.DayOfWeek].ToString())
                    : dt.ToString("yyyy-MM-dd");
                float dem = box.Height * 0.13f;
                Font df2 = HHmiDraw.Pf("Consolas", dem, FontStyle.Regular);
                while (g.MeasureString(d, df2).Width > box.Width * 0.94f && dem > 5.5f)
                {
                    df2.Dispose();
                    dem -= 0.5f;
                    df2 = HHmiDraw.Pf("Consolas", dem, FontStyle.Regular);
                }
                using (df2)
                    g.CenterText(new RectangleF(box.X, box.Bottom - box.Height * 0.30f, box.Width, box.Height * 0.2f),
                        d, df2, HHmiDraw.HMixer(acc, Color.White, 0.25f));
            }
        }

        // 翻页钟
        private void PaintFlip(Graphics g, C st, HHmiScheme sc, Color acc, System.DateTime dt)
        {
            var groups = new[]
            {
                dt.Hour.ToString("00"), dt.Minute.ToString("00"),
                _showSecond ? dt.Second.ToString("00") : null
            };
            int n = _showSecond ? 3 : 2;
            float gap = 4f;
            float totalW = Width - 12f;
            float gw = (totalW - gap * (n - 1)) / n;
            float gh = Math.Min(Height - 30f, gw * 0.92f);
            float x0 = 6f, y0 = (Height - gh) / 2f - 4f;
            for (int i = 0; i < n; i++)
            {
                var card = new RectangleF(x0 + i * (gw + gap), y0, gw, gh);
                using (var gp = HHmiDraw.RoundBox(card, 6f))
                using (var b = new LinearGradientBrush(card, Color.FromArgb(46, 50, 58), Color.FromArgb(22, 26, 32), 90f))
                    g.FillPath(b, gp);
                // 中缝线（翻页轴）
                using (var p = new Pen(Color.FromArgb(10, 12, 16), 1.6f))
                    g.DrawLine(p, card.X, card.Y + gh / 2f, card.Right, card.Y + gh / 2f);
                using (var nf = HHmiDraw.Pf("Consolas", gh * 0.46f, FontStyle.Bold))
                    g.CenterText(card, groups[i], nf, Color.FromArgb(232, 238, 246));
                if (i < n - 1)
                    using (var cf2 = HHmiDraw.Pf("Consolas", gh * 0.3f, FontStyle.Bold))
                        g.CenterText(new RectangleF(card.Right, y0, gap, gh), ":", cf2, acc);
            }
            if (_showDate)
                using (var df = HHmiDraw.Pf(Font.FontFamily, 10.7f))
                    g.CenterText(new RectangleF(0, Height - 18f, Width, 14f),
                        dt.ToString("yyyy-MM-dd"), df, sc.Text);
        }

        // 二进制灯列（时/分/秒各 6 位）
        private void PaintBinary(Graphics g, C st, Color acc, System.DateTime dt)
        {
            int[] vals = { dt.Hour, dt.Minute, _showSecond ? dt.Second : 0 };
            string[] names = { "H", "M", "S" };
            float colW = (Width - 16f) / 3f;
            float top = Height * 0.16f;
            float gap = (Height * 0.66f) / 5f;
            float dr = Math.Min(colW * 0.26f, gap * 0.30f);
            using (var cf = HHmiDraw.Pf(Font.FontFamily, 10.7f, FontStyle.Bold))
                for (int c = 0; c < 3; c++)
                {
                    if (c == 2 && !_showSecond) continue;
                    float cx = 8f + colW * (c + 0.5f);
                    g.CenterText(new RectangleF(cx - colW / 2f, 4f, colW, top - 2f), names[c], cf, acc);
                    for (int bit = 0; bit < 6; bit++)
                    {
                        bool on = (vals[c] & (1 << (5 - bit))) != 0;
                        float cy = top + bit * gap;
                        if (on) g.Ell(cx, cy, dr * 1.5f, dr * 1.5f, Color.FromArgb(60, acc));
                        g.Ell(cx, cy, dr, dr, on ? acc : Color.FromArgb(58, 64, 72));
                    }
                }
        }
    }
}
