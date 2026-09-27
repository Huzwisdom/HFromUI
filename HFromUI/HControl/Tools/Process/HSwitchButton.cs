using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Process
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 开关按钮外形：22 种工业操作元件——拨杆、翘板、平头/照光按钮、钥匙开关、
    /// 选择开关、脚踏、限位、摇杆、拨码、旋钮、滑动、触摸发光按钮等。
    /// </summary>
    public enum HSwitchButtonStyle
    {
        Toggle = 0,        // 拨杆开关
        Rocker = 1,        // 翘板开关
        PushButton = 2,    // 平头按钮
        Illuminated = 3,   // 照光按钮
        KeySwitch = 4,     // 钥匙开关
        Selector2 = 5,     // 两档选择开关
        Selector3 = 6,     // 三档选择开关
        FootSwitch = 7,    // 脚踏开关
        LimitSwitch = 8,   // 限位开关（滚轮摇臂）
        Magnetic = 9,      // 电磁开关
        FlipCover = 10,    // 翻盖保护开关
        StartButton = 11,  // 绿色启动按钮
        StopButton = 12,   // 红色停止按钮
        Joystick = 13,     // 摇杆
        DipSwitch = 14,    // 四联拨码
        PushPull = 15,     // 推拉开关
        RotaryKnob = 16,   // 旋钮
        SlideSwitch = 17,  // 滑动开关
        PushLock = 18,     // 自锁按钮
        Momentary = 19,    // 点动按钮
        HeavyDuty = 20,    // 重型工业按钮
        SmartTouch = 21    // 触摸发光按钮
    }
    /// <summary>
    /// 开关按钮控件（继承 HToolAnimBase）：22 种操作元件外形，SwitchedOn 表示接通/按下状态，
    /// 点击控件切换状态；启动类按钮接通为绿色，停止类为红色，其余随主题色调。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("开关按钮控件：22 种操作元件外形，点击切换通断")]
    public class HSwitchButton : HToolAnimBase
    {
        private HSwitchButtonStyle _style = HSwitchButtonStyle.Toggle;
        private bool _on;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CGreen = Color.FromArgb(52, 168, 83);
        private static readonly Color CGreenDk = Color.FromArgb(30, 110, 52);
        private static readonly Color CRed = Color.FromArgb(214, 52, 52);
        private static readonly Color CRedDk = Color.FromArgb(138, 28, 28);
        private static readonly Color CPanel = Color.FromArgb(64, 68, 74);
        private static readonly Color CPanelLt = Color.FromArgb(96, 100, 106);
        private static readonly Color CMetal = Color.FromArgb(196, 200, 206);
        private static readonly Color CMetalDk = Color.FromArgb(112, 116, 124);
        /// <summary>开关按钮外形。</summary>
        [HCategoryLanguage("开关按钮"), HDisplayNameLanguage("开关按钮外形样式"), HDescriptionLanguage("开关按钮外形样式"), Browsable(true)]
        [DefaultValue(HSwitchButtonStyle.Toggle)]
        public HSwitchButtonStyle SwitchStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>是否接通/按下。</summary>
        [HCategoryLanguage("开关按钮"), HDisplayNameLanguage("已开启"), HDescriptionLanguage("接通/按下状态，点击可切换"), Browsable(true)]
        [DefaultValue(false)]
        public bool SwitchedOn
        {
            get => _on;
            set { _on = value; Invalidate(); }
        }
        protected override void OnClick(EventArgs e)
        {
            _on = !_on;
            Invalidate();
            base.OnClick(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            if (a.Width <= 2f || a.Height <= 2f) { PaintPlacedText(g); return; }
            switch (_style)
            {
                case HSwitchButtonStyle.Rocker: DrawRocker(g, a, pal); break;
                case HSwitchButtonStyle.PushButton: DrawPush(g, a, pal, false); break;
                case HSwitchButtonStyle.Illuminated: DrawPush(g, a, pal, true); break;
                case HSwitchButtonStyle.KeySwitch: DrawKey(g, a, pal); break;
                case HSwitchButtonStyle.Selector2: DrawSelector(g, a, pal, 2); break;
                case HSwitchButtonStyle.Selector3: DrawSelector(g, a, pal, 3); break;
                case HSwitchButtonStyle.FootSwitch: DrawFoot(g, a, pal); break;
                case HSwitchButtonStyle.LimitSwitch: DrawLimit(g, a, pal); break;
                case HSwitchButtonStyle.Magnetic: DrawMagnetic(g, a, pal); break;
                case HSwitchButtonStyle.FlipCover: DrawFlipCover(g, a, pal); break;
                case HSwitchButtonStyle.StartButton: DrawSemanticButton(g, a, true); break;
                case HSwitchButtonStyle.StopButton: DrawSemanticButton(g, a, false); break;
                case HSwitchButtonStyle.Joystick: DrawJoystick(g, a, pal); break;
                case HSwitchButtonStyle.DipSwitch: DrawDip(g, a, pal); break;
                case HSwitchButtonStyle.PushPull: DrawPushPull(g, a, pal); break;
                case HSwitchButtonStyle.RotaryKnob: DrawKnob(g, a, pal); break;
                case HSwitchButtonStyle.SlideSwitch: DrawSlide(g, a, pal); break;
                case HSwitchButtonStyle.PushLock: DrawPush(g, a, pal, true); break;
                case HSwitchButtonStyle.Momentary: DrawPush(g, a, pal, false); break;
                case HSwitchButtonStyle.HeavyDuty: DrawHeavy(g, a, pal); break;
                case HSwitchButtonStyle.SmartTouch: DrawTouch(g, a, pal); break;
                default: DrawToggle(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // 拨杆开关
        // ----------------------------------------------------------------
        private void DrawToggle(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = a.Height * 0.26f;
            var baseR = new RectangleF(a.Left + a.Width * 0.14f, a.Bottom - baseH,
                a.Width * 0.72f, baseH * 0.72f);
            using (var bb = new LinearGradientBrush(baseR, CPanelLt, CPanel, 90f))
                g.FillRectangle(bb, baseR);
            // 螺纹圈
            using (var col = new SolidBrush(CMetalDk))
                g.FillEllipse(col, cx - a.Width * 0.2f, a.Bottom - baseH * 1.15f, a.Width * 0.4f, baseH * 0.6f);
            // 拨杆（on 向上右倾，off 向左倾）
            float pivotX = cx, pivotY = a.Bottom - baseH * 0.85f;
            float len = a.Height * 0.62f;
            double ang = _on ? -Math.PI / 2.6 : -Math.PI + Math.PI / 2.6;
            var st = g.Save();
            g.TranslateTransform(pivotX, pivotY);
            g.RotateTransform((float)(ang * 180 / Math.PI) + 90f);
            using (var lever = new SolidBrush(CMetal))
                g.FillRectangle(lever, -a.Width * 0.05f, -len, a.Width * 0.1f, len);
            using (var tip = new SolidBrush(_on ? CGreen : CRed))
                g.FillEllipse(tip, -a.Width * 0.1f, -len - a.Width * 0.1f, a.Width * 0.2f, a.Width * 0.2f);
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 翘板开关
        // ----------------------------------------------------------------
        private void DrawRocker(Graphics g, RectangleF a, HToolPalette pal)
        {
            var frame = RectangleF.Inflate(a, -a.Width * 0.08f, -a.Height * 0.2f);
            using (var fb = new LinearGradientBrush(frame, CPanelLt, CPanel, 90f))
            using (var path = HBarBase.RoundPath(frame, Math.Min(6f, frame.Height * 0.2f)))
                g.FillPath(fb, path);
            var key = RectangleF.Inflate(frame, -frame.Width * 0.12f, -frame.Height * 0.14f);
            // 翘板：上半/下半分别着色，中间折线表达倾斜
            var hi = new RectangleF(key.Left, key.Top, key.Width, key.Height / 2f);
            var lo = new RectangleF(key.Left, key.Top + key.Height / 2f, key.Width, key.Height / 2f);
            using (var hb = new SolidBrush(_on ? CGreen : CPanelLt))
                g.FillRectangle(hb, hi);
            using (var lb = new SolidBrush(!_on ? CRed : CPanel))
                g.FillRectangle(lb, lo);
            using (var edge = new Pen(CMetalDk, 1f))
                g.DrawRectangle(edge, key.X, key.Y, key.Width, key.Height);
        }
        // ----------------------------------------------------------------
        // 平头/照光/点动/自锁按钮
        // ----------------------------------------------------------------
        private void DrawPush(Graphics g, RectangleF a, HToolPalette pal, bool lit)
        {
            float d = Math.Min(a.Width, a.Height) * 0.78f;
            var r = new RectangleF(a.Left + (a.Width - d) / 2f, a.Top + (a.Height - d) / 2f, d, d);
            // 安装环
            using (var ring = new LinearGradientBrush(r, CMetal, CMetalDk, 45f))
                g.FillEllipse(ring, RectangleF.Inflate(r, d * 0.1f, d * 0.1f));
            // 按钮帽
            float sink = _on ? d * 0.05f : 0f;
            var cap = RectangleF.Inflate(r, -d * 0.14f, -d * 0.14f);
            cap.Offset(0, sink);
            Color top, bottom;
            if (lit && _on) { top = HToolPalettes.Tint(pal.Accent, 0.4f); bottom = pal.Accent; }
            else { top = HToolPalettes.Tint(pal.Main, 0.3f); bottom = pal.Dark; }
            using (var cb = new LinearGradientBrush(cap, top, bottom, 90f))
                g.FillEllipse(cb, cap);
            using (var ep = new Pen(pal.Edge, Math.Max(1f, d * 0.02f)))
                g.DrawEllipse(ep, cap);
            using (var hl = new SolidBrush(Color.FromArgb(90, Color.White)))
                g.FillEllipse(hl, cap.Left + cap.Width * 0.2f, cap.Top + cap.Height * 0.14f,
                    cap.Width * 0.24f, cap.Height * 0.18f);
        }
        // ----------------------------------------------------------------
        // 启动（绿）/停止（红）语义按钮
        // ----------------------------------------------------------------
        private void DrawSemanticButton(Graphics g, RectangleF a, bool start)
        {
            bool lit = start ? _on : !_on;
            Color c = start ? CGreen : CRed;
            Color cd = start ? CGreenDk : CRedDk;
            var plate = RectangleF.Inflate(a, -a.Width * 0.06f, -a.Height * 0.16f);
            using (var pb = new SolidBrush(Color.FromArgb(226, 228, 232)))
            using (var path = HBarBase.RoundPath(plate, 4f))
                g.FillPath(pb, path);
            float d = Math.Min(plate.Width, plate.Height) * 0.72f;
            var cap = new RectangleF(plate.Left + (plate.Width - d) / 2f,
                plate.Top + (plate.Height - d) / 2f + (_on ? d * 0.04f : 0f), d, d);
            using (var cb = new LinearGradientBrush(cap,
                lit ? HToolPalettes.Tint(c, 0.3f) : HToolPalettes.Shade(c, 0.4f), cd, 90f))
                g.FillEllipse(cb, cap);
            using (var hl = new SolidBrush(Color.FromArgb(80, Color.White)))
                g.FillEllipse(hl, cap.Left + cap.Width * 0.22f, cap.Top + cap.Height * 0.15f,
                    cap.Width * 0.22f, cap.Height * 0.16f);
        }
        // ----------------------------------------------------------------
        // 钥匙开关
        // ----------------------------------------------------------------
        private void DrawKey(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            var plate = new RectangleF(a.Left + a.Width * 0.1f, a.Top + a.Height * 0.16f,
                a.Width * 0.8f, a.Height * 0.6f);
            using (var pb = new LinearGradientBrush(plate, CPanelLt, CPanel, 90f))
                g.FillEllipse(pb, plate);
            float r = Math.Min(a.Width, a.Height) * 0.22f;
            using (var disc = new SolidBrush(CMetal))
                g.FillEllipse(disc, cx - r, cy - r, r * 2, r * 2);
            // 钥匙槽（随 on 转 90°）
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(_on ? 90f : 0f);
            using (var slot = new Pen(CPanel, Math.Max(1.5f, r * 0.22f)))
                g.DrawLine(slot, 0, 0, r * 0.7f, 0);
            g.Restore(st);
            using (var bp = new Pen(CMetalDk, 1f))
            {
                g.DrawArc(bp, plate.Left + 4f, plate.Top + 4f, plate.Width - 8f, plate.Height - 8f, 210f, 120f);
            }
        }
        // ----------------------------------------------------------------
        // 选择开关（2/3 档）
        // ----------------------------------------------------------------
        private void DrawSelector(Graphics g, RectangleF a, HToolPalette pal, int positions)
        {
            var plate = RectangleF.Inflate(a, -a.Width * 0.1f, -a.Height * 0.22f);
            using (var pb = new LinearGradientBrush(plate, Color.FromArgb(210, 213, 218),
                Color.FromArgb(166, 170, 176), 90f))
            using (var path = HBarBase.RoundPath(plate, 4f))
                g.FillPath(pb, path);
            float cx = a.Left + a.Width / 2f, cy = plate.Top + plate.Height * 0.52f;
            float r = Math.Min(plate.Width, plate.Height) * 0.2f;
            using (var knob = new SolidBrush(CPanel))
                g.FillEllipse(knob, cx - r, cy - r, r * 2, r * 2);
            // 档位标记点
            using (var dot = new SolidBrush(CMetalDk))
                for (int i = 0; i < positions; i++)
                {
                    float t = positions == 2 ? (i == 0 ? 0.28f : 0.72f) : (0.22f + i * 0.28f);
                    g.FillEllipse(dot, plate.Left + plate.Width * t - 1.5f,
                        plate.Top + plate.Height * 0.2f - 1.5f, 3f, 3f);
                }
            // 指针
            int pos = _on ? positions - 1 : 0;
            float tt = positions == 2 ? (pos == 0 ? 0.28f : 0.72f) : (0.22f + pos * 0.28f);
            using (var ptr = new Pen(CMetal, Math.Max(1.5f, r * 0.2f)) { StartCap = LineCap.Round })
                g.DrawLine(ptr, cx, cy, plate.Left + plate.Width * tt, plate.Top + plate.Height * 0.24f);
        }
        // ----------------------------------------------------------------
        // 脚踏开关
        // ----------------------------------------------------------------
        private void DrawFoot(Graphics g, RectangleF a, HToolPalette pal)
        {
            var baseR = new RectangleF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.56f,
                a.Width * 0.84f, a.Height * 0.32f);
            using (var bb = new LinearGradientBrush(baseR, CMetal, CMetalDk, 90f))
                g.FillRectangle(bb, baseR);
            var pedal = new[]
            {
                new PointF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.62f),
                new PointF(a.Right - a.Width * 0.16f, a.Top + a.Height * 0.62f),
                new PointF(a.Right - a.Width * 0.26f, a.Top + (_on ? a.Height * 0.52f : a.Height * 0.36f)),
                new PointF(a.Left + a.Width * 0.3f, a.Top + (_on ? a.Height * 0.52f : a.Height * 0.36f))
            };
            Color pc = _on ? HToolPalettes.Shade(pal.Main, 0.2f) : pal.Main;
            using (var cb = new LinearGradientBrush(a, HToolPalettes.Tint(pc, 0.25f), pal.Dark, 90f))
                g.FillPolygon(cb, pedal);
        }
        // ----------------------------------------------------------------
        // 限位开关（滚轮摇臂）
        // ----------------------------------------------------------------
        private void DrawLimit(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = new RectangleF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.42f,
                a.Width * 0.56f, a.Height * 0.4f);
            using (var bb = new LinearGradientBrush(box, CPanelLt, CPanel, 90f))
                g.FillRectangle(bb, box);
            float px = a.Left + a.Width * 0.72f, py = a.Top + a.Height * 0.46f;
            double ang = _on ? -0.5 : -1.1;
            float len = a.Height * 0.42f;
            float ex = px + (float)Math.Cos(ang) * len, ey = py + (float)Math.Sin(ang) * len;
            using (var arm = new Pen(CMetalDk, Math.Max(2f, a.Width * 0.05f)))
                g.DrawLine(arm, px, py, ex, ey);
            float rw = a.Width * 0.11f;
            using (var wheel = new SolidBrush(CMetal))
                g.FillEllipse(wheel, ex - rw, ey - rw, rw * 2, rw * 2);
            using (var axle = new SolidBrush(CPanel))
                g.FillEllipse(axle, ex - rw * 0.3f, ey - rw * 0.3f, rw * 0.6f, rw * 0.6f);
        }
        // ----------------------------------------------------------------
        // 电磁开关（线圈 + 触点）
        // ----------------------------------------------------------------
        private void DrawMagnetic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var coil = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.2f,
                a.Width * 0.4f, a.Height * 0.5f);
            using (var cb = new LinearGradientBrush(coil, HToolPalettes.Tint(pal.Main, 0.3f), pal.Dark, 90f))
                g.FillRectangle(cb, coil);
            using (var wire = new Pen(CPanel, Math.Max(1f, a.Width * 0.02f)))
                for (int i = 0; i < 4; i++)
                    g.DrawLine(wire, coil.Left + coil.Width * 0.15f,
                        coil.Top + coil.Height * (0.2f + i * 0.18f),
                        coil.Right - coil.Width * 0.15f,
                        coil.Top + coil.Height * (0.2f + i * 0.18f));
            // 触点
            float cx = a.Left + a.Width * 0.74f;
            float y1 = a.Top + a.Height * 0.32f, y2 = a.Top + a.Height * 0.62f;
            using (var con = new Pen(CMetalDk, Math.Max(1.5f, a.Width * 0.03f)))
            {
                g.DrawLine(con, cx - a.Width * 0.12f, y1, cx, y1);
                g.DrawLine(con, cx - a.Width * 0.12f, y2, cx, y2);
                g.DrawLine(con, cx, y1, cx + (_on ? 0f : a.Width * 0.06f), _on ? y2 : y2 - a.Height * 0.1f);
            }
            using (var dot = new SolidBrush(_on ? CGreen : CRed))
                g.FillEllipse(dot, cx - 2.5f, y1 - 2.5f, 5f, 5f);
        }
        // ----------------------------------------------------------------
        // 翻盖保护开关
        // ----------------------------------------------------------------
        private void DrawFlipCover(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.1f, -a.Height * 0.2f);
            using (var bb = new SolidBrush(Color.FromArgb(214, 200, 60)))
            using (var path = HBarBase.RoundPath(box, 6f))
                g.FillPath(bb, path);
            float d = Math.Min(box.Width, box.Height) * 0.42f;
            using (var cap = new SolidBrush(_on ? CGreen : CRed))
                g.FillEllipse(cap, box.Left + (box.Width - d) / 2f,
                    box.Bottom - d - box.Height * 0.08f, d, d);
            // 翻盖（on 打开翻到顶部）
            var cover = new RectangleF(box.Left + box.Width * 0.08f,
                _on ? box.Top - box.Height * 0.06f : box.Top + box.Height * 0.1f,
                box.Width * 0.84f, box.Height * 0.5f);
            using (var cv = new SolidBrush(Color.FromArgb(_on ? 140 : 220, 240, 224, 80)))
                g.FillRectangle(cv, cover);
            using (var edge = new Pen(CYellowDk(), 1.5f))
                g.DrawRectangle(edge, cover.X, cover.Y, cover.Width, cover.Height);
        }
        private static Color CYellowDk() => Color.FromArgb(172, 140, 20);
        // ----------------------------------------------------------------
        // 摇杆
        // ----------------------------------------------------------------
        private void DrawJoystick(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseY = a.Bottom - a.Height * 0.12f;
            float baseW = a.Width * 0.56f, baseH = a.Height * 0.2f;
            using (var bb = new LinearGradientBrush(
                new RectangleF(cx - baseW / 2f, baseY - baseH, baseW, baseH),
                CPanelLt, CPanel, 90f))
                g.FillEllipse(bb, cx - baseW / 2f, baseY - baseH, baseW, baseH);
            // 杆
            double ang = _on ? -0.5 : 0.18;
            float len = a.Height * 0.58f;
            float bx = cx, by = baseY - baseH * 0.5f;
            float tx = bx + (float)Math.Sin(ang) * len, ty = by - (float)Math.Cos(ang) * len;
            using (var rod = new Pen(CMetalDk, Math.Max(2f, a.Width * 0.06f)) { StartCap = LineCap.Round })
                g.DrawLine(rod, bx, by, tx, ty);
            float gh = a.Width * 0.22f;
            using (var grip = new SolidBrush(pal.Dark))
                g.FillEllipse(grip, tx - gh / 2f, ty - gh / 2f, gh, gh);
        }
        // ----------------------------------------------------------------
        // 四联拨码
        // ----------------------------------------------------------------
        private void DrawDip(Graphics g, RectangleF a, HToolPalette pal)
        {
            var frame = RectangleF.Inflate(a, -a.Width * 0.06f, -a.Height * 0.26f);
            using (var fb = new SolidBrush(CPanel))
                g.FillRectangle(fb, frame);
            int n = 4;
            float gap = frame.Width * 0.05f;
            float sw = (frame.Width - gap * (n + 1)) / n;
            for (int i = 0; i < n; i++)
            {
                bool on = _on ? i < 3 : i == 0;
                var cell = new RectangleF(frame.Left + gap + i * (sw + gap),
                    frame.Top + frame.Height * 0.12f, sw, frame.Height * 0.76f);
                using (var cb = new SolidBrush(Color.FromArgb(70, 74, 80)))
                    g.FillRectangle(cb, cell);
                var thumb = new RectangleF(cell.Left + cell.Width * 0.12f,
                    on ? cell.Top + cell.Height * 0.08f : cell.Bottom - cell.Height * 0.44f,
                    cell.Width * 0.76f, cell.Height * 0.36f);
                using (var tb = new SolidBrush(on ? CGreen : Color.FromArgb(150, 60, 60)))
                    g.FillRectangle(tb, thumb);
            }
        }
        // ----------------------------------------------------------------
        // 推拉开关
        // ----------------------------------------------------------------
        private void DrawPushPull(Graphics g, RectangleF a, HToolPalette pal)
        {
            var barrel = new RectangleF(a.Left + a.Width * 0.22f, a.Top + a.Height * 0.14f,
                a.Width * 0.56f, a.Height * 0.72f);
            using (var bb = new LinearGradientBrush(barrel, CPanelLt, CPanel, 0f))
                g.FillRectangle(bb, barrel);
            float stemH = a.Height * (_on ? 0.3f : 0.12f);
            var stem = new RectangleF(a.Left + a.Width * 0.44f, a.Top + a.Height * 0.04f,
                a.Width * 0.12f, stemH);
            using (var sb = new SolidBrush(CMetal))
                g.FillRectangle(sb, stem);
            var grip = new RectangleF(a.Left + a.Width * 0.34f, a.Top, a.Width * 0.32f, a.Height * 0.16f);
            using (var gb = new SolidBrush(_on ? CGreen : CRed))
                g.FillRectangle(gb, grip);
        }
        // ----------------------------------------------------------------
        // 旋钮
        // ----------------------------------------------------------------
        private void DrawKnob(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.52f;
            // 刻度
            using (var tick = new Pen(CPanel, Math.Max(1f, a.Width * 0.02f)))
                for (int i = -3; i <= 3; i++)
                {
                    double ang = -Math.PI * 0.75 + (i + 3) / 6.0 * Math.PI * 1.5;
                    float r1 = Math.Min(a.Width, a.Height) * 0.42f;
                    float r2 = r1 * 0.82f;
                    g.DrawLine(tick,
                        cx + (float)Math.Cos(ang) * r1, cy + (float)Math.Sin(ang) * r1,
                        cx + (float)Math.Cos(ang) * r2, cy + (float)Math.Sin(ang) * r2);
                }
            float r = Math.Min(a.Width, a.Height) * 0.28f;
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(_on ? 120f : -120f);
            using (var kb = new LinearGradientBrush(new RectangleF(-r, -r, r * 2, r * 2),
                HToolPalettes.Tint(pal.Main, 0.35f), pal.Dark, 45f))
            using (var pen = new Pen(pal.Edge, 1.5f))
            {
                g.FillEllipse(kb, -r, -r, r * 2, r * 2);
                g.DrawEllipse(pen, -r, -r, r * 2, r * 2);
                using (var mark = new Pen(CPanel, Math.Max(1.5f, r * 0.16f)))
                    g.DrawLine(mark, 0, 0, 0, -r * 0.7f);
            }
            g.Restore(st);
        }
        // ----------------------------------------------------------------
        // 滑动开关
        // ----------------------------------------------------------------
        private void DrawSlide(Graphics g, RectangleF a, HToolPalette pal)
        {
            var track = new RectangleF(a.Left + a.Width * 0.1f, a.Top + a.Height * 0.36f,
                a.Width * 0.8f, a.Height * 0.28f);
            using (var tb = new SolidBrush(CPanel))
            using (var path = HBarBase.RoundPath(track, track.Height / 2f))
                g.FillPath(tb, path);
            if (_on)
            {
                var fill = new RectangleF(track.Left, track.Top,
                    track.Width * 0.6f, track.Height);
                using (var fb = new SolidBrush(pal.Accent))
                using (var fp = HBarBase.RoundPath(fill, fill.Height / 2f))
                    g.FillPath(fb, fp);
            }
            float thumbR = track.Height * 0.72f;
            float tx = _on ? track.Right - thumbR * 0.7f : track.Left + thumbR * 0.7f - thumbR;
            using (var hb = new SolidBrush(CMetal))
                g.FillEllipse(hb, tx, track.Top + (track.Height - thumbR) / 2f, thumbR, thumbR);
        }
        // ----------------------------------------------------------------
        // 重型工业按钮（黄护圈 + 大按钮帽）
        // ----------------------------------------------------------------
        private void DrawHeavy(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float d = Math.Min(a.Width, a.Height) * 0.92f;
            using (var guard = new Pen(Color.FromArgb(240, 196, 40), d * 0.12f))
                g.DrawEllipse(guard, cx - d / 2f, cy - d / 2f, d, d);
            var cap = new RectangleF(cx - d * 0.34f + (_on ? 0 : 0), cy - d * 0.3f + (_on ? d * 0.05f : 0),
                d * 0.68f, d * 0.6f);
            Color c = _on ? pal.Dark : pal.Main;
            using (var cb = new LinearGradientBrush(cap, HToolPalettes.Tint(c, 0.3f), pal.Dark, 90f))
                g.FillEllipse(cb, cap);
        }
        // ----------------------------------------------------------------
        // 触摸发光按钮
        // ----------------------------------------------------------------
        private void DrawTouch(Graphics g, RectangleF a, HToolPalette pal)
        {
            var panel = RectangleF.Inflate(a, -a.Width * 0.08f, -a.Height * 0.18f);
            using (var pb = new LinearGradientBrush(panel, Color.FromArgb(46, 50, 56), Color.FromArgb(26, 28, 32), 90f))
            using (var path = HBarBase.RoundPath(panel, panel.Height * 0.22f))
                g.FillPath(pb, path);
            float cx = a.Left + a.Width / 2f, cy = panel.Top + panel.Height / 2f;
            float r = Math.Min(panel.Width, panel.Height) * 0.3f;
            Color c = _on ? pal.Accent : HToolPalettes.Mix(pal.Accent, Color.Gray, 0.6f);
            if (_on)
                for (int k = 3; k >= 1; k--)
                    using (var halo = new SolidBrush(Color.FromArgb(16, c)))
                        g.FillEllipse(halo, cx - r - k * r * 0.18f, cy - r - k * r * 0.18f,
                            (r + k * r * 0.18f) * 2f, (r + k * r * 0.18f) * 2f);
            using (var ring = new Pen(c, Math.Max(1.5f, r * 0.12f)))
                g.DrawEllipse(ring, cx - r, cy - r, r * 2f, r * 2f);
            using (var dot = new SolidBrush(Color.FromArgb(_on ? 90 : 40, c)))
                g.FillEllipse(dot, cx - r * 0.5f, cy - r * 0.5f, r, r);
        }
    }
}