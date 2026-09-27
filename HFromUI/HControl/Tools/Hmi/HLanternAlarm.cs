using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Hmi
{
    /// <summary>报警灯外形样式，共 22 种。</summary>
    public enum HLanternAlarmStyle
    {
        Classic = 0,        // 经典灯柱+半圆顶
        Beacon = 1,         // 旋转光束警灯
        StackTower = 2,     // 三色塔灯
        FlatPanel = 3,      // 平板报警器
        WallBracket = 4,    // 壁挂式
        LowProfile = 5,     // 扁半球警灯
        Buzzer = 6,         // 蜂鸣器（带孔）
        Xenon = 7,          // 氙气星芒闪光灯
        LedRing = 8,        // LED 环闪
        HeavyBase = 9,      // 重型底座
        DualSiren = 10,     // 双灯红蓝
        TrafficBar = 11,    // 三色横条
        Tripod = 12,        // 三脚架便携灯
        Cage = 13,          // 防护网罩灯
        Minimal = 14,       // 极简小半球
        GlassDome = 15,     // 透明玻璃罩
        Industrial = 16,    // 工业方盒
        Neon = 17,          // 霓虹警灯
        Cyber = 18,         // 赛博朋克
        NightVision = 19,   // 夜视荧光
        Amber = 20,         // 琥珀警灯
        PurpleHolo = 21     // 紫色全息
    }

    /// <summary>
    /// 报警信号灯控件：22 种外形 × 13 种色调，报警时按设定间隔闪烁，
    /// 旋转光束、氙气星芒、LED 环闪、三色塔灯/横条等动画，正常时显示常态灰/绿灯。
    /// </summary>
    [DefaultProperty("IsAlarm")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("报警信号灯控件：22 种外形、13 种色调，报警闪烁与光束动画")]
    public class HLanternAlarm : HLabelBase
    {
        private readonly Timer _timer;
        private HLanternAlarmStyle _style = HLanternAlarmStyle.Classic;
        private HToolTheme _theme = HToolTheme.Classic;
        private bool _isAlarm;
        private int _flashInterval = 500;
        private Color _alarmColor = Color.Empty;
        private int _tick;
        private bool _flashOn;

        /// <summary>默认竖装灯柱比例。</summary>
        public HLanternAlarm()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(56, 96);
            _timer = new Timer { Interval = 50 };
            _timer.Tick += Tick;
        }

        /// <summary>外形样式。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("外形样式"), HDescriptionLanguage("报警灯外形样式，共 22 种"), Browsable(true)]
        [DefaultValue(HLanternAlarmStyle.Classic)]
        public HLanternAlarmStyle AlarmStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }

        /// <summary>图元色调。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("图元色调"), HDescriptionLanguage("图元色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }

        /// <summary>是否处于报警状态（启动闪烁/旋转动画）。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("报警状态"), HDescriptionLanguage("报警时灯光闪烁/光束旋转"), Browsable(true)]
        [DefaultValue(false)]
        public bool IsAlarm
        {
            get => _isAlarm;
            set { _isAlarm = value; Invalidate(); }
        }

        /// <summary>闪烁间隔（毫秒）。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("闪烁间隔"), HDescriptionLanguage("报警闪烁间隔（毫秒）"), Browsable(true)]
        [DefaultValue(500)]
        public int FlashInterval
        {
            get => _flashInterval;
            set => _flashInterval = Math.Max(100, value);
        }

        /// <summary>报警灯光自定义颜色；Empty 使用色调方案报警色。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("报警灯光"), HDescriptionLanguage("报警灯光自定义颜色，未设置使用色调报警色"), Browsable(true)]
        [DefaultValue(typeof(Color), "")]
        public Color AlarmColor
        {
            get => _alarmColor;
            set { _alarmColor = value; Invalidate(); }
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

        private void Tick(object sender, EventArgs e)
        {
            if (IsDisposed) { _timer.Stop(); return; }
            _tick++;
            int n = Math.Max(2, _flashInterval / 50);
            bool f = (_tick % (n * 2)) < n;
            if (f != _flashOn) { _flashOn = f; Invalidate(); }
            if (_isAlarm) Invalidate();
        }

        private sealed class A
        {
            public int Kind;
            public bool Dark;
            public bool Glow;
            public bool Beam;          // 旋转光束
            public bool Cage;
            public bool Tripod;
            public bool Bracket;
            public bool Square;
            public Color Lamp;
            public bool CustomLamp;
        }

        private static A StyleOf(HLanternAlarmStyle s)
        {
            var a = new A();
            switch (s)
            {
                case HLanternAlarmStyle.Beacon: a.Beam = true; break;
                case HLanternAlarmStyle.StackTower: a.Kind = 2; break;
                case HLanternAlarmStyle.FlatPanel: a.Kind = 3; break;
                case HLanternAlarmStyle.WallBracket: a.Kind = 4; a.Bracket = true; break;
                case HLanternAlarmStyle.LowProfile: a.Kind = 5; break;
                case HLanternAlarmStyle.Buzzer: a.Kind = 6; a.Square = true; break;
                case HLanternAlarmStyle.Xenon: a.Kind = 7; a.Beam = true; a.Glow = true; break;
                case HLanternAlarmStyle.LedRing: a.Kind = 8; a.Glow = true; break;
                case HLanternAlarmStyle.HeavyBase: a.Kind = 9; break;
                case HLanternAlarmStyle.DualSiren: a.Kind = 10; break;
                case HLanternAlarmStyle.TrafficBar: a.Kind = 11; break;
                case HLanternAlarmStyle.Tripod: a.Kind = 12; a.Tripod = true; break;
                case HLanternAlarmStyle.Cage: a.Kind = 13; a.Cage = true; break;
                case HLanternAlarmStyle.Minimal: a.Kind = 14; break;
                case HLanternAlarmStyle.GlassDome: a.Kind = 15; break;
                case HLanternAlarmStyle.Industrial: a.Kind = 16; a.Square = true; break;
                case HLanternAlarmStyle.Neon: a.Glow = true; a.Dark = true; break;
                case HLanternAlarmStyle.Cyber:
                    a.Dark = true; a.Glow = true; a.Square = true; a.Kind = 16;
                    a.Lamp = Color.FromArgb(0, 229, 255); a.CustomLamp = true; break;
                case HLanternAlarmStyle.NightVision:
                    a.Dark = true; a.Glow = true;
                    a.Lamp = Color.FromArgb(96, 255, 150); a.CustomLamp = true; break;
                case HLanternAlarmStyle.Amber:
                    a.Glow = true;
                    a.Lamp = Color.FromArgb(255, 176, 40); a.CustomLamp = true; break;
                case HLanternAlarmStyle.PurpleHolo:
                    a.Dark = true; a.Glow = true;
                    a.Lamp = Color.FromArgb(192, 128, 255); a.CustomLamp = true; break;
            }
            return a;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 12 || Height < 12) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            A st = StyleOf(_style);
            HHmiScheme sc = st.Dark ? HHmiPalettes.Dark(Theme) : HHmiPalettes.Get(Theme);
            Color red = st.CustomLamp ? st.Lamp :
                (_alarmColor != Color.Empty ? _alarmColor : sc.Alarm);
            bool lit = _isAlarm && _flashOn;

            switch (st.Kind)
            {
                case 2: PaintStack(g, st, sc, red); break;
                case 3: PaintPanel(g, st, sc, red, lit); break;
                case 4: PaintWall(g, st, sc, red, lit); break;
                case 6: PaintBuzzer(g, st, sc, red, lit); break;
                case 10: PaintDual(g, st, sc); break;
                case 11: PaintTraffic(g, st, sc); break;
                case 12: PaintTripod(g, st, sc, red, lit); break;
                case 16: PaintIndustrialBox(g, st, sc, red, lit); break;
                default: PaintDomeLamp(g, st, sc, red, lit); break;
            }

            if (!string.IsNullOrEmpty(Text))
                using (var tf = HHmiDraw.Pf(Font.FontFamily, 10f, FontStyle.Bold))
                    g.CenterText(new RectangleF(0, Height - 13f, Width, 12f), Text, tf, ForeColor);
        }

        // 经典/半球/光束/星芒/玻璃罩/网罩/极简/重型底座
        private void PaintDomeLamp(Graphics g, A st, HHmiScheme sc, Color red, bool lit)
        {
            float cx = Width / 2f;
            float r = Math.Min(Width * (st.Kind == 5 ? 0.40f : 0.34f), 26f);
            if (st.Kind == 14) r = Math.Min(Width * 0.30f, 16f);
            float top = st.Kind == 5 ? Height * 0.22f : 6f;
            float domeH = st.Kind == 5 ? r * 0.62f : r;
            float bodyTop = top + domeH;
            float bodyBottom = Height - (st.Tripod ? 22f : 18f);
            Color metal = sc.Bezel, metalD = sc.BezelDark;

            // 三脚架
            if (st.Tripod)
                using (var p = new Pen(metalD, 2.5f))
                {
                    g.DrawLine(p, cx, bodyBottom, cx - r * 0.9f, Height - 4f);
                    g.DrawLine(p, cx, bodyBottom, cx + r * 0.9f, Height - 4f);
                    g.DrawLine(p, cx, bodyBottom, cx, Height - 5f);
                }

            // 灯柱
            if (st.Kind != 14)
            {
                using (var b = new LinearGradientBrush(new RectangleF(cx - r, bodyTop, r * 2f, bodyBottom - bodyTop),
                    HHmiDraw.HMixer(metal, Color.White, 0.25f), HHmiDraw.HMixer(metal, Color.Black, 0.3f), 0f))
                    g.FillRectangle(b, cx - r * 0.82f, bodyTop, r * 1.64f, bodyBottom - bodyTop);
                // 底座
                PointF[] base1 =
                {
                    new PointF(cx - r * 0.82f, bodyBottom), new PointF(cx + r * 0.82f, bodyBottom),
                    new PointF(cx + r * 1.05f, bodyBottom + 5f), new PointF(cx - r * 1.05f, bodyBottom + 5f)
                };
                using (var b = new SolidBrush(metalD)) g.FillPolygon(b, base1);
                PointF[] base2 =
                {
                    new PointF(cx - r * 1.05f, bodyBottom + 5f), new PointF(cx + r * 1.05f, bodyBottom + 5f),
                    new PointF(cx + r * 1.25f, bodyBottom + 10f), new PointF(cx - r * 1.25f, bodyBottom + 10f)
                };
                using (var b = new SolidBrush(metal)) g.FillPolygon(b, base2);
            }

            var domeRect = new RectangleF(cx - r, top, r * 2f, domeH * 2f);
            using (var dome = new GraphicsPath())
            {
                dome.AddArc(domeRect, 180f, 180f);
                dome.AddLine(cx - r, top + domeH, cx + r, top + domeH);
                dome.CloseFigure();

                // 辉光
                if (st.Glow && lit)
                    for (int k = 4; k >= 1; k--)
                        using (var b = new SolidBrush(Color.FromArgb(Math.Max(8, 30 - k * 6), red)))
                            g.FillEllipse(b, cx - r - k * 3, top - k * 3, (r + k * 3) * 2f, domeH * 2f + k * 6);

                // 灯体颜色
                Color dc = lit ? red : Color.FromArgb(196, 200, 206);
                using (var b = lit && !st.Glow
                    ? (Brush)new LinearGradientBrush(domeRect,
                        HHmiDraw.HMixer(red, Color.White, 0.55f),
                        HHmiDraw.HMixer(red, Color.Black, 0.2f), 90f)
                    : new SolidBrush(dc))
                    g.FillPath(b, dome);

                // 玻璃罩：内部灯泡
                if (st.Kind == 15)
                {
                    g.Ell(cx, top + domeH * 0.65f, r * 0.30f, r * 0.30f,
                        lit ? HHmiDraw.HMixer(red, Color.White, 0.5f) : Color.FromArgb(150, 154, 160));
                    using (var b = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                        g.FillPath(b, dome);
                }

                // 旋转光束（裁进灯罩）
                if (st.Beam && lit)
                {
                    var state = g.Save();
                    g.SetClip(dome);
                    float ang = _tick * 9f % 360f;
                    using (var b = new SolidBrush(Color.FromArgb(120, Color.White)))
                    {
                        g.TranslateTransform(cx, top + domeH);
                        g.RotateTransform(ang);
                        g.FillPie(b, -r, -r * 1.6f, r * 2f, r * 2.2f, -22f, 44f);
                    }
                    g.Restore(state);
                }

                // 氙气星芒
                if (st.Kind == 7 && lit)
                {
                    float lx = cx, ly = top + domeH * 0.55f;
                    using (var p = new Pen(Color.FromArgb(220, HHmiDraw.HMixer(red, Color.White, 0.5f)), 2f))
                        for (int i = 0; i < 8; i++)
                        {
                            float a = i * 45f;
                            g.DrawLine(p, HHmiDraw.Polar(lx, ly, r * 0.35f, a),
                                HHmiDraw.Polar(lx, ly, r * (1.05f + 0.18f * (float)Math.Sin(_tick * 0.5)), a));
                        }
                }

                using (var pen = new Pen(HHmiDraw.HMixer(metalD, Color.Black, 0.2f), 1.3f))
                    g.DrawPath(pen, dome);

                // LED 环
                if (st.Kind == 8)
                    for (int i = 0; i < 16; i++)
                    {
                        float a = 180f + i * 180f / 15f;
                        bool on = lit && Math.Abs(((a - _tick * 6f) % 360f + 540f) % 360f - 180f) < 46f;
                        var lp = HHmiDraw.Polar(cx, top + domeH, r * 0.92f, a);
                        g.Ell(lp.X, lp.Y, 1.8f, 1.8f, on ? red : Color.FromArgb(110, 116, 122));
                    }

                // 防护网
                if (st.Cage)
                    using (var p = new Pen(Color.FromArgb(170, metalD), 1.4f))
                    {
                        for (int i = -3; i <= 3; i++)
                        {
                            float x = cx + i * r / 3f;
                            float e = (float)Math.Sqrt(Math.Max(0, 1 - (i / 3.2f) * (i / 3.2f))) * domeH;
                            g.DrawLine(p, x, top + domeH, x, top + domeH - e);
                        }
                        g.DrawArc(p, domeRect, 180f, 180f);
                    }
            }
        }

        // 三色塔灯
        private void PaintStack(Graphics g, A st, HHmiScheme sc, Color red)
        {
            float cx = Width / 2f;
            float r = Math.Min(Width * 0.30f, 18f);
            float top = 8f, chH = Math.Min(22f, (Height - 30f) / 3f);
            Color[] cols = { Color.FromArgb(228, 74, 58), Color.FromArgb(240, 176, 40), Color.FromArgb(72, 196, 96) };
            int active = _isAlarm ? 0 : 2;
            for (int i = 0; i < 3; i++)
            {
                var box = new RectangleF(cx - r, top + i * chH, r * 2f, chH);
                bool flash = i == 0 && _isAlarm && _flashOn;
                bool steady = i == active && !_isAlarm;
                Color dim = Color.FromArgb(120, cols[i].R, cols[i].G, cols[i].B);
                using (var gp = HHmiDraw.RoundBox(box, i == 0 ? r : 0f))
                {
                    using (var b = new SolidBrush((flash || steady) ? cols[i] : dim))
                        g.FillPath(b, gp);
                    using (var p = new Pen(sc.BezelDark, 1.1f)) g.DrawPath(p, gp);
                }
            }
            // 顶帽与底座
            g.FaceDisk(cx, top - 2f, r * 0.5f, sc.Bezel, sc.BezelDark);
            var by = top + chH * 3;
            using (var b = new SolidBrush(sc.Bezel))
                g.FillRectangle(b, cx - r * 1.1f, by, r * 2.2f, 6f);
        }

        // 平板报警器
        private void PaintPanel(Graphics g, A st, HHmiScheme sc, Color red, bool lit)
        {
            var box = new RectangleF(4f, 4f, Width - 8f, Height - 18f);
            using (var gp = HHmiDraw.RoundBox(box, 8f))
            using (var b = new LinearGradientBrush(box,
                HHmiDraw.HMixer(sc.Bezel, Color.White, 0.2f),
                HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.3f), 60f))
                g.FillPath(b, gp);
            float lr = Math.Min(box.Width, box.Height) * 0.26f;
            float lcx = box.X + box.Width / 2f, lcy = box.Y + box.Height * 0.38f;
            if (lit)
                for (int k = 3; k >= 1; k--)
                    g.Ell(lcx, lcy, lr + k * 4f, lr + k * 4f, Color.FromArgb(24, red));
            g.FaceDisk(lcx, lcy, lr, lit ? red : Color.FromArgb(180, 184, 190),
                HHmiDraw.HMixer(red, Color.Black, 0.3f));
            string stateText = _isAlarm ? "ALARM" : "NORMAL";
            float em = 8f;
            Font tf = HHmiDraw.Pf(Font.FontFamily, em, FontStyle.Bold);
            while (g.MeasureString(stateText, tf).Width > box.Width * 0.9f && em > 5.5f)
            {
                tf.Dispose();
                em -= 0.5f;
                tf = HHmiDraw.Pf(Font.FontFamily, em, FontStyle.Bold);
            }
            using (tf)
                g.CenterText(new RectangleF(box.X, lcy + lr + 4f, box.Width, 18f),
                    stateText, tf,
                    _isAlarm ? Color.FromArgb(240, 240, 240) : Color.FromArgb(120, 126, 134));
        }

        // 壁挂式
        private void PaintWall(Graphics g, A st, HHmiScheme sc, Color red, bool lit)
        {
            using (var p = new Pen(sc.BezelDark, 3f))
                g.DrawLine(p, 2f, 12f, Width * 0.34f, 12f);
            g.Ell(Width * 0.34f, 12f, 3.5f, 3.5f, sc.Bezel);
            var saved = new A { Kind = 5, Glow = st.Glow };
            PaintDomeLamp(g, saved, sc, red, lit);
        }

        // 蜂鸣器
        private void PaintBuzzer(Graphics g, A st, HHmiScheme sc, Color red, bool lit)
        {
            var box = new RectangleF(4f, Height * 0.18f, Width - 8f, Height * 0.62f);
            using (var gp = HHmiDraw.RoundBox(box, 6f))
            using (var b = new LinearGradientBrush(box, Color.FromArgb(70, 76, 84), Color.FromArgb(36, 40, 46), 90f))
                g.FillPath(b, gp);
            float cols = 5f, rows = 4f;
            float dx = box.Width / (cols + 1), dy = box.Height / (rows + 1);
            using (var b = new SolidBrush(Color.FromArgb(16, 18, 22)))
                for (int i = 1; i <= cols; i++)
                    for (int j = 1; j <= rows; j++)
                        g.FillEllipse(b, box.X + i * dx - 2.2f, box.Y + j * dy - 2.2f, 4.4f, 4.4f);
            if (lit)
                using (var p = new Pen(Color.FromArgb(200, red), 2.4f))
                    g.DrawArc(p, box.X - 3f, box.Y - 3f, box.Width + 6f, box.Height + 6f,
                        (int)(_tick * 12f) % 360, 120f);
        }

        // 双灯红蓝
        private void PaintDual(Graphics g, A st, HHmiScheme sc)
        {
            Color blue = Color.FromArgb(52, 124, 232);
            DrawOneSiren(g, Width * 0.28f, red: blue, on: _isAlarm && !_flashOn, sc, left: true);
            DrawOneSiren(g, Width * 0.72f, red: Color.FromArgb(232, 64, 58), on: _isAlarm && _flashOn, sc, left: false);
            float r = Width * 0.22f, top = Height * 0.20f, baseLine = top + r;
            var housing = new RectangleF(Width * 0.12f, baseLine - 1f, Width * 0.76f, Height - 14f - baseLine);
            using (var gp = HHmiDraw.RoundBox(housing, 3f))
            using (var b = new LinearGradientBrush(housing,
                HHmiDraw.HMixer(sc.Bezel, Color.White, 0.22f),
                HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.30f), 0f))
                g.FillPath(b, gp);
            using (var b = new SolidBrush(sc.BezelDark))
                g.FillRectangle(b, Width * 0.20f, Height - 14f, Width * 0.60f, 6f);
        }

        private void DrawOneSiren(Graphics g, float cx, Color red, bool on, HHmiScheme sc, bool left)
        {
            float r = Width * 0.22f, top = Height * 0.20f;
            var rect = new RectangleF(cx - r, top, r * 2f, r * 2f);
            using (var dome = new GraphicsPath())
            {
                dome.AddArc(rect, 180f, 180f);
                dome.AddLine(cx - r, top + r, cx + r, top + r);
                dome.CloseFigure();
                using (var b = new SolidBrush(on ? red : Color.FromArgb(160, 166, 174)))
                    g.FillPath(b, dome);
                if (on)
                {
                    var state = g.Save();
                    g.SetClip(dome);
                    using (var b = new SolidBrush(Color.FromArgb(110, Color.White)))
                    {
                        g.TranslateTransform(cx, top + r);
                        g.RotateTransform(_tick * 10f);
                        g.FillPie(b, -r, -r * 1.6f, r * 2f, r * 2.2f, -20f, 40f);
                    }
                    g.Restore(state);
                }
                using (var p = new Pen(sc.BezelDark, 1.2f)) g.DrawPath(p, dome);
            }
        }

        // 三色横条
        private void PaintTraffic(Graphics g, A st, HHmiScheme sc)
        {
            var bar = new RectangleF(3f, Height * 0.30f, Width - 6f, Height * 0.34f);
            using (var gp = HHmiDraw.RoundBox(bar, bar.Height / 2f))
            using (var b = new SolidBrush(Color.FromArgb(34, 38, 44)))
                g.FillPath(b, gp);
            Color[] cols = { Color.FromArgb(228, 74, 58), Color.FromArgb(240, 176, 40), Color.FromArgb(72, 196, 96) };
            for (int i = 0; i < 3; i++)
            {
                float lcx = bar.X + bar.Width * (i + 1) / 4f;
                float lcy = bar.Y + bar.Height / 2f, lr = bar.Height * 0.28f;
                bool on = _isAlarm ? (i == 0 && _flashOn) : i == 2;
                if (i == 1 && !_isAlarm) on = false;
                Color dim2 = Color.FromArgb(90, cols[i].R, cols[i].G, cols[i].B);
                if (on) g.Ell(lcx, lcy, lr * 1.5f, lr * 1.5f, Color.FromArgb(40, cols[i].R, cols[i].G, cols[i].B));
                g.Ell(lcx, lcy, lr, lr, on ? cols[i] : dim2);
            }
        }

        // 三脚架便携灯
        private void PaintTripod(Graphics g, A st, HHmiScheme sc, Color red, bool lit)
        {
            PaintDomeLamp(g, new A { Kind = 12, Tripod = true, Glow = true }, sc, red, lit);
        }

        // 工业方盒
        private void PaintIndustrialBox(Graphics g, A st, HHmiScheme sc, Color red, bool lit)
        {
            var box = new RectangleF(4f, 6f, Width - 8f, Height - 22f);
            using (var gp = HHmiDraw.RoundBox(box, 7f))
            using (var b = new LinearGradientBrush(box,
                HHmiDraw.HMixer(sc.Bezel, Color.White, 0.22f),
                HHmiDraw.HMixer(sc.Bezel, Color.Black, 0.34f), 60f))
                g.FillPath(b, gp);
            for (int i = 0; i < 4; i++)
            {
                float bx = (i % 2 == 0 ? box.X + 6 : box.Right - 6);
                float by = (i < 2 ? box.Y + 6 : box.Bottom - 6);
                g.Rivet(bx, by, 2.2f, sc.Bezel, sc.BezelDark);
            }
            float lr = Math.Min(box.Width, box.Height) * 0.28f;
            float lcx = box.X + box.Width / 2f, lcy = box.Y + box.Height * 0.42f;
            if (lit && st.Glow)
                for (int k = 3; k >= 1; k--)
                    g.Ell(lcx, lcy, lr + k * 4f, lr + k * 4f, Color.FromArgb(26, red));
            g.FaceDisk(lcx, lcy, lr, lit ? red : Color.FromArgb(182, 186, 192),
                HHmiDraw.HMixer(red, Color.Black, 0.3f));
        }
    }
}
