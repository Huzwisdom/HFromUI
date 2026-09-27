using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Indicator
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 报警灯外形：22 种工业声光报警外形——经典旋转警灯、圆顶灯、爆闪灯、
    /// 声光一体、警灯条、防爆护网、蜂鸣器、三脚架便携灯等。
    /// </summary>
    public enum HAlarmLampStyle
    {
        Classic = 0,        // 经典柱形旋转警灯
        Dome = 1,           // 圆顶警灯
        MiniBeacon = 2,     // 小型圆顶警示灯
        Strobe = 3,         // 方形爆闪灯
        SoundLight = 4,     // 声光一体灯
        Tower = 5,          // 三层报警塔灯
        Horn = 6,           // 旋转喇叭警灯
        DualBeacon = 7,     // 双联圆顶灯
        WarningBar = 8,     // 长条警灯条
        Revolver = 9,       // 六角反光杯旋转灯
        SirenTower = 10,    // 警灯蜂鸣塔
        LowProfile = 11,    // 薄型圆顶灯
        ForkBeacon = 12,    // 叉车支架警示灯
        ExplosionProof = 13,// 防爆护网警灯
        TrafficWarn = 14,   // 施工路障灯
        AlarmPost = 15,     // 高柱报警器
        Ceiling = 16,       // 吸顶式警灯
        WallMount = 17,     // 壁挂式警灯
        SignalStack = 18,   // 三色信号柱
        Buzzer = 19,        // 蜂鸣器
        IndicatorPanel = 20,// 报警指示盘
        Portable = 21       // 三脚架便携警灯
    }
    /// <summary>警灯灯罩语义色。</summary>
    public enum HAlarmColor
    {
        Red = 0,        // 红（默认，最高级别报警）
        Amber = 1,      // 琥珀（警告）
        Blue = 2,       // 蓝（紧急/医疗）
        Green = 3,      // 绿（正常/疏散）
        White = 4,      // 白（爆闪）
        Magenta = 5     // 品红（特殊气体）
    }
    /// <summary>
    /// 报警灯控件（继承 HToolAnimBase）：22 种声光报警外形、6 种灯罩语义色；
    /// Running=true 时警灯光束旋转/闪烁/爆闪，停止时灯面灰暗。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("报警灯控件：22 种声光报警外形，运行时旋转闪烁")]
    public class HAlarmLamp : HToolAnimBase
    {
        private HAlarmLampStyle _style = HAlarmLampStyle.Classic;
        private HAlarmColor _alarmColor = HAlarmColor.Red;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color[] LampColors =
        {
            Color.FromArgb(224, 44, 44), Color.FromArgb(245, 166, 35), Color.FromArgb(59, 125, 226),
            Color.FromArgb(52, 168, 83), Color.FromArgb(238, 242, 248), Color.FromArgb(214, 72, 186)
        };
        private static readonly Color CMetal = Color.FromArgb(176, 180, 186);
        private static readonly Color CMetalDk = Color.FromArgb(108, 112, 120);
        /// <summary>报警灯外形。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("报警灯外形样式"), HDescriptionLanguage("报警灯外形样式"), Browsable(true)]
        [DefaultValue(HAlarmLampStyle.Classic)]
        public HAlarmLampStyle AlarmStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>灯罩语义色。</summary>
        [HCategoryLanguage("报警灯"), HDisplayNameLanguage("灯罩语义色"), HDescriptionLanguage("灯罩语义色：红/琥珀/蓝/绿/白/品红"), Browsable(true)]
        [DefaultValue(HAlarmColor.Red)]
        public HAlarmColor LampColorKind
        {
            get => _alarmColor;
            set { _alarmColor = value; Invalidate(); }
        }
        private Color Lamp => LampColors[(int)_alarmColor];
        // 闪烁强度（0..1）：Running 时按相位脉动，停止为 0
        private float Flash => Running ? 0.55f + 0.45f * (float)Math.Abs(Math.Sin(Phase * 1.7)) : 0f;
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            if (a.Width <= 2f || a.Height <= 2f) { PaintPlacedText(g); return; }
            switch (_style)
            {
                case HAlarmLampStyle.Dome: DrawDome(g, a, pal, 0.42f); break;
                case HAlarmLampStyle.MiniBeacon: DrawDome(g, a, pal, 0.5f); break;
                case HAlarmLampStyle.Strobe: DrawStrobe(g, a, pal); break;
                case HAlarmLampStyle.SoundLight: DrawSoundLight(g, a, pal); break;
                case HAlarmLampStyle.Tower: DrawStack(g, a, pal, 3); break;
                case HAlarmLampStyle.Horn: DrawHornLamp(g, a, pal); break;
                case HAlarmLampStyle.DualBeacon: DrawDual(g, a, pal); break;
                case HAlarmLampStyle.WarningBar: DrawBar(g, a, pal); break;
                case HAlarmLampStyle.Revolver: DrawRevolver(g, a, pal); break;
                case HAlarmLampStyle.SirenTower: DrawSirenTower(g, a, pal); break;
                case HAlarmLampStyle.LowProfile: DrawDome(g, a, pal, 0.62f); break;
                case HAlarmLampStyle.ForkBeacon: DrawFork(g, a, pal); break;
                case HAlarmLampStyle.ExplosionProof: DrawExplosion(g, a, pal); break;
                case HAlarmLampStyle.TrafficWarn: DrawTrafficWarn(g, a, pal); break;
                case HAlarmLampStyle.AlarmPost: DrawPost(g, a, pal); break;
                case HAlarmLampStyle.Ceiling: DrawCeiling(g, a, pal); break;
                case HAlarmLampStyle.WallMount: DrawWall(g, a, pal); break;
                case HAlarmLampStyle.SignalStack: DrawStack(g, a, pal, 4); break;
                case HAlarmLampStyle.Buzzer: DrawBuzzer(g, a, pal); break;
                case HAlarmLampStyle.IndicatorPanel: DrawPanel(g, a, pal); break;
                case HAlarmLampStyle.Portable: DrawPortable(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // 经典柱形旋转警灯：底座 + 柱形灯罩 + 旋转光束
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = Math.Min(a.Height * 0.16f, 14f);
            var lens = new RectangleF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.10f,
                a.Width * 0.68f, a.Height * 0.66f);
            if (lens.Width <= 3f || lens.Height <= 3f) return;
            DrawBase(g, cx, a.Bottom - baseH, a.Width * 0.6f, baseH);
            DrawBeam(g, cx, lens.Top + lens.Height * 0.5f, lens.Width * 0.95f);
            DrawCylinderLens(g, lens);
        }
        // 柱形灯罩（竖棱玻璃罩 + 内反光杯）
        private void DrawCylinderLens(Graphics g, RectangleF r)
        {
            float rim = Math.Min(r.Width, r.Height) * 0.08f;
            // 灯罩
            var gr = new RectangleF(r.X, r.Y + rim, r.Width, r.Height - rim * 2f);
            Color lc = ShadeByFlash(Lamp, 0.35f);
            using (var lb = new LinearGradientBrush(gr, HToolPalettes.Tint(lc, 0.5f), HToolPalettes.Shade(lc, 0.3f), 0f))
                g.FillRectangle(lb, gr);
            // 圆顶 + 圆底
            using (var lb = new SolidBrush(lc))
            {
                g.FillEllipse(lb, r.X, r.Y, r.Width, rim * 2f);
                g.FillEllipse(lb, r.X, r.Bottom - rim * 2f, r.Width, rim * 2f);
            }
            // 竖棱
            using (var rib = new Pen(Color.FromArgb(60, HToolPalettes.Shade(Lamp, 0.5f)), 1f))
                for (float x = r.Left + r.Width * 0.18f; x < r.Right; x += r.Width * 0.18f)
                    g.DrawLine(rib, x, gr.Top + 1f, x, gr.Bottom - 1f);
            // 顶圈
            using (var cap = new SolidBrush(CMetalDk))
                g.FillEllipse(cap, r.X + r.Width * 0.18f, r.Y - rim * 0.3f, r.Width * 0.64f, rim * 0.9f);
            // 中央高光
            using (var hi = new SolidBrush(Color.FromArgb((int)(Flash * 130f), Color.White)))
                g.FillRectangle(hi, r.X + r.Width * 0.34f, gr.Top + rim * 0.4f, r.Width * 0.12f, gr.Height * 0.8f);
        }
        // 旋转光束
        private void DrawBeam(Graphics g, float cx, float cy, float rad)
        {
            if (!Running) return;
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var beam = new SolidBrush(Color.FromArgb(38, Lamp)))
                g.FillPie(beam, -rad, -rad * 0.62f, rad * 2f, rad * 1.24f, -22f, 44f);
            g.Restore(st);
        }
        // 梯形/矩形底座
        private void DrawBase(Graphics g, float cx, float top, float w, float h)
        {
            PointF[] pts =
            {
                new PointF(cx - w * 0.5f, top),
                new PointF(cx + w * 0.5f, top),
                new PointF(cx + w * 0.62f, top + h),
                new PointF(cx - w * 0.62f, top + h)
            };
            using (var b = new LinearGradientBrush(new RectangleF(cx - w * 0.62f, top, w * 1.24f, h), CMetal, CMetalDk, 90f))
                g.FillPolygon(b, pts);
        }
        // ----------------------------------------------------------------
        // 圆顶（穹顶）警灯：flat 越大越扁
        // ----------------------------------------------------------------
        private void DrawDome(Graphics g, RectangleF a, HToolPalette pal, float flat)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = Math.Min(a.Height * 0.14f, 12f);
            float dw = a.Width * 0.72f;
            float dh = a.Height * (0.66f - flat * 0.25f);
            var dome = new RectangleF(cx - dw / 2f, a.Top + a.Height * 0.12f, dw, Math.Max(6f, dh));
            DrawBase(g, cx, a.Bottom - baseH, dw, baseH);
            DrawBeam(g, cx, dome.Bottom - dome.Height * 0.2f, dw * 0.55f);
            // 穹顶（上半圆 + 裙边）
            Color lc = ShadeByFlash(Lamp, 0.35f);
            using (var lb = new LinearGradientBrush(dome, HToolPalettes.Tint(lc, 0.55f), HToolPalettes.Shade(lc, 0.32f), 90f))
            {
                g.FillEllipse(lb, dome);
                using (var skirt = new SolidBrush(lc))
                    g.FillRectangle(skirt, dome.X + 1f, dome.Bottom - dome.Height * 0.22f, dome.Width - 2f, dome.Height * 0.22f);
            }
            using (var rim = new Pen(HToolPalettes.Shade(lc, 0.5f), Math.Max(1f, dw * 0.02f)))
                g.DrawEllipse(rim, dome);
            // 高光
            using (var hi = new SolidBrush(Color.FromArgb((int)(40f + Flash * 120f), Color.White)))
                g.FillEllipse(hi, dome.Left + dw * 0.2f, dome.Top + dh * 0.12f, dw * 0.16f, dh * 0.22f);
            // 反光杯棱
            if (dw > 14f)
                using (var rib = new Pen(Color.FromArgb(50, Color.Black), 1f))
                {
                    g.DrawLine(rib, cx, dome.Bottom - dh * 0.28f, cx, dome.Bottom - 2f);
                    g.DrawLine(rib, cx - dw * 0.18f, dome.Bottom - dh * 0.26f, cx - dw * 0.1f, dome.Bottom - 2f);
                    g.DrawLine(rib, cx + dw * 0.18f, dome.Bottom - dh * 0.26f, cx + dw * 0.1f, dome.Bottom - 2f);
                }
        }
        private Color ShadeByFlash(Color c, float grey)
            => Running ? HToolPalettes.Mix(c, Color.White, 0.18f * Flash) : HToolPalettes.Mix(c, Color.Gray, grey);
        // ----------------------------------------------------------------
        // 方形爆闪灯
        // ----------------------------------------------------------------
        private void DrawStrobe(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.12f, -a.Height * 0.18f);
            if (box.Width <= 3f) return;
            using (var body = new LinearGradientBrush(box, Color.FromArgb(72, 76, 82), Color.FromArgb(34, 37, 42), 90f))
            using (var path = HBarBase.RoundPath(box, Math.Min(6f, box.Width * 0.14f)))
                g.FillPath(body, path);
            var win = RectangleF.Inflate(box, -box.Width * 0.14f, -box.Height * 0.18f);
            Color c = LampColors[(int)HAlarmColor.White];
            using (var wb = new SolidBrush(Running
                ? Color.FromArgb((int)(120 + Flash * 135), HToolPalettes.Tint(c, Flash * 0.3f))
                : Color.FromArgb(90, 120, 120, 124)))
            using (var wp = HBarBase.RoundPath(win, Math.Min(4f, win.Width * 0.12f)))
                g.FillPath(wb, wp);
            if (Running && Flash > 0.85f)
                using (var burst = new SolidBrush(Color.FromArgb(120, c)))
                    for (int k = 0; k < 8; k++)
                    {
                        double ang = k * Math.PI / 4;
                        g.FillEllipse(burst,
                            box.Left + box.Width / 2f + (float)Math.Cos(ang) * box.Width * 0.36f - 2f,
                            box.Top + box.Height / 2f + (float)Math.Sin(ang) * box.Height * 0.36f - 2f, 4f, 4f);
                    }
        }
        // ----------------------------------------------------------------
        // 声光一体：圆顶灯 + 右侧/底部喇叭
        // ----------------------------------------------------------------
        private void DrawSoundLight(Graphics g, RectangleF a, HToolPalette pal)
        {
            var lampArea = new RectangleF(a.X, a.Y, a.Width * 0.58f, a.Height);
            DrawDome(g, lampArea, pal, 0.42f);
            // 喇叭
            float hx = lampArea.Right - a.Width * 0.02f, hy = a.Top + a.Height * 0.42f;
            float hw = a.Width * 0.36f, hh = a.Height * 0.3f;
            using (var horn = new SolidBrush(CMetalDk))
            {
                g.FillRectangle(horn, hx, hy + hh * 0.32f, hw * 0.3f, hh * 0.36f);
                PointF[] tri =
                {
                    new PointF(hx + hw * 0.28f, hy),
                    new PointF(hx + hw, hy + hh / 2f),
                    new PointF(hx + hw * 0.28f, hy + hh)
                };
                g.FillPolygon(horn, tri);
            }
            if (Running)
                using (var wave = new Pen(Color.FromArgb((int)(Flash * 190f), Lamp), Math.Max(1f, a.Width * 0.02f)))
                {
                    g.DrawArc(wave, hx + hw * 0.85f, hy - hh * 0.15f, hh * 0.5f, hh * 0.5f, -40f, 80f);
                    g.DrawArc(wave, hx + hw * 0.95f, hy - hh * 0.3f, hh * 0.8f, hh * 0.8f, -40f, 80f);
                }
        }
        // ----------------------------------------------------------------
        // 多层信号塔（报警色分层）
        // ----------------------------------------------------------------
        private void DrawStack(Graphics g, RectangleF a, HToolPalette pal, int n)
        {
            Color[] layerColors = n == 3
                ? new[] { LampColors[0], LampColors[1], LampColors[2] }
                : new[] { LampColors[0], LampColors[1], LampColors[2], LampColors[3] };
            float cx = a.Left + a.Width / 2f;
            float baseH = Math.Min(a.Height * 0.12f, 12f);
            float top = a.Top + a.Height * 0.05f;
            float stackH = a.Height * 0.78f;
            float lh = stackH / n;
            float lw = Math.Min(a.Width * 0.66f, lh * 1.5f);
            if (lw < 5f) lw = Math.Min(5f, a.Width);
            for (int i = 0; i < n; i++)
            {
                var r = new RectangleF(cx - lw / 2f, top + i * lh, lw, lh - 1.5f);
                Color lc = layerColors[i];
                bool on = Running && (i == 0 || (i == 1 && Flash > 0.5f));
                using (var body = new LinearGradientBrush(r,
                    on ? HToolPalettes.Tint(lc, 0.4f) : HToolPalettes.Mix(lc, Color.Gray, 0.6f),
                    on ? HToolPalettes.Shade(lc, 0.2f) : HToolPalettes.Shade(lc, 0.5f), 0f))
                    g.FillRectangle(body, r);
                using (var edge = new Pen(CMetalDk, 1f))
                    g.DrawRectangle(edge, r.X, r.Y, r.Width, r.Height);
            }
            DrawBase(g, cx, a.Bottom - baseH, lw * 1.1f, baseH);
        }
        // ----------------------------------------------------------------
        // 喇叭形旋转警灯
        // ----------------------------------------------------------------
        private void DrawHornLamp(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            var hub = new RectangleF(cx - a.Width * 0.12f, a.Top + a.Height * 0.3f, a.Width * 0.24f, a.Height * 0.4f);
            var st = g.Save();
            g.TranslateTransform(cx, a.Top + a.Height * 0.5f);
            if (Running) g.RotateTransform(SpinAngle);
            using (var horn = new LinearGradientBrush(
                new RectangleF(-a.Width * 0.44f, -a.Height * 0.3f, a.Width * 0.88f, a.Height * 0.6f),
                CMetal, CMetalDk, 0f))
            {
                PointF[] tri =
                {
                    new PointF(a.Width * 0.02f, -a.Height * 0.14f),
                    new PointF(a.Width * 0.42f, -a.Height * 0.3f),
                    new PointF(a.Width * 0.42f, a.Height * 0.3f),
                    new PointF(a.Width * 0.02f, a.Height * 0.14f)
                };
                g.FillPolygon(horn, tri);
            }
            g.Restore(st);
            float d = Math.Min(a.Width * 0.3f, a.Height * 0.3f);
            DrawDomeLens(g, new RectangleF(cx - d / 2f, a.Top + a.Height * 0.14f, d, d * 0.8f));
            g.FillEllipse(Brushes.DimGray, hub);
            float baseH = Math.Min(a.Height * 0.12f, 10f);
            DrawBase(g, cx, a.Bottom - baseH, a.Width * 0.5f, baseH);
        }
        private void DrawDomeLens(Graphics g, RectangleF r)
        {
            if (r.Width <= 2f) return;
            Color lc = ShadeByFlash(Lamp, 0.35f);
            using (var lb = new LinearGradientBrush(r, HToolPalettes.Tint(lc, 0.5f), HToolPalettes.Shade(lc, 0.3f), 90f))
                g.FillEllipse(lb, r);
            using (var hi = new SolidBrush(Color.FromArgb((int)(40 + Flash * 120), Color.White)))
                g.FillEllipse(hi, r.Left + r.Width * 0.2f, r.Top + r.Height * 0.15f, r.Width * 0.18f, r.Height * 0.25f);
        }
        // ----------------------------------------------------------------
        // 双联圆顶
        // ----------------------------------------------------------------
        private void DrawDual(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.44f;
            DrawDomeIn(g, new RectangleF(a.Left, a.Top, w, a.Height), LampColors[0]);
            DrawDomeIn(g, new RectangleF(a.Right - w, a.Top, w, a.Height), LampColors[1]);
        }
        private void DrawDomeIn(Graphics g, RectangleF a, Color lamp)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = Math.Min(a.Height * 0.14f, 10f);
            var dome = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.14f,
                a.Width * 0.76f, a.Height * 0.6f);
            DrawBase(g, cx, a.Bottom - baseH, a.Width * 0.62f, baseH);
            Color lc = Running ? HToolPalettes.Tint(lamp, 0.2f * Flash) : HToolPalettes.Mix(lamp, Color.Gray, 0.6f);
            using (var lb = new LinearGradientBrush(dome, HToolPalettes.Tint(lc, 0.5f), HToolPalettes.Shade(lc, 0.3f), 90f))
                g.FillEllipse(lb, dome);
            using (var hi = new SolidBrush(Color.FromArgb((int)(Flash * 140f), Color.White)))
                g.FillEllipse(hi, dome.Left + dome.Width * 0.2f, dome.Top + dome.Height * 0.14f,
                    dome.Width * 0.16f, dome.Height * 0.2f);
        }
        // ----------------------------------------------------------------
        // 长条警灯条：多段交替闪
        // ----------------------------------------------------------------
        private void DrawBar(Graphics g, RectangleF a, HToolPalette pal)
        {
            bool vert = a.Height > a.Width * 1.4f;
            var box = RectangleF.Inflate(a, -Math.Min(a.Width, a.Height) * 0.06f, -Math.Min(a.Width, a.Height) * 0.06f);
            using (var body = new SolidBrush(Color.FromArgb(30, 33, 38)))
            using (var path = HBarBase.RoundPath(box, Math.Min(6f, Math.Min(box.Width, box.Height) * 0.18f)))
                g.FillPath(body, path);
            int segs = 6;
            for (int i = 0; i < segs; i++)
            {
                RectangleF sr;
                Color c = i < segs / 2 ? Lamp : LampColors[(int)HAlarmColor.Blue];
                if (vert)
                {
                    float sh = (box.Height - box.Height * 0.08f) / segs;
                    sr = new RectangleF(box.Left + box.Width * 0.18f, box.Top + box.Height * 0.04f + i * sh,
                        box.Width * 0.64f, sh * 0.84f);
                }
                else
                {
                    float sw = (box.Width - box.Width * 0.06f) / segs;
                    sr = new RectangleF(box.Left + box.Width * 0.03f + i * sw, box.Top + box.Height * 0.22f,
                        sw * 0.9f, box.Height * 0.56f);
                }
                bool on = Running && (i % 2 == (Phase * 2f % Math.PI > Math.PI / 2 ? 0 : 1));
                using (var lb = new SolidBrush(on ? HToolPalettes.Tint(c, 0.25f + Flash * 0.25f)
                    : HToolPalettes.Mix(c, Color.Gray, 0.65f)))
                    g.FillRectangle(lb, sr);
            }
        }
        // ----------------------------------------------------------------
        // 六角反光杯旋转灯（柱罩内 6 棱杯旋转）
        // ----------------------------------------------------------------
        private void DrawRevolver(Graphics g, RectangleF a, HToolPalette pal)
        {
            float baseH = Math.Min(a.Height * 0.14f, 12f);
            var lens = new RectangleF(a.Left + a.Width * 0.18f, a.Top + a.Height * 0.08f,
                a.Width * 0.64f, a.Height * 0.68f);
            DrawCylinderLens(g, lens);
            float cx = a.Left + a.Width / 2f;
            var st = g.Save();
            g.TranslateTransform(cx, lens.Top + lens.Height * 0.55f);
            g.RotateTransform(SpinAngle);
            float rr = lens.Width * 0.42f;
            using (var cup = new SolidBrush(Color.FromArgb(Running ? 220 : 120, HToolPalettes.Tint(Lamp, 0.5f))))
                for (int i = 0; i < 6; i++)
                {
                    g.RotateTransform(60f);
                    g.FillPolygon(cup, new[]
                    {
                        new PointF(0, -rr * 0.18f), new PointF(rr * 0.8f, 0),
                        new PointF(0, rr * 0.18f)
                    });
                }
            g.Restore(st);
            DrawBase(g, cx, a.Bottom - baseH, a.Width * 0.56f, baseH);
        }
        // ----------------------------------------------------------------
        // 警灯蜂鸣塔：圆顶灯 + 蜂鸣箱 + 立柱
        // ----------------------------------------------------------------
        private void DrawSirenTower(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = a.Height * 0.1f;
            using (var rod = new SolidBrush(CMetalDk))
                g.FillRectangle(rod, cx - a.Width * 0.04f, a.Top + a.Height * 0.52f,
                    a.Width * 0.08f, a.Height * 0.38f);
            var buzzer = new RectangleF(cx - a.Width * 0.26f, a.Top + a.Height * 0.46f,
                a.Width * 0.52f, a.Height * 0.16f);
            using (var bb = new LinearGradientBrush(buzzer, CMetal, CMetalDk, 90f))
                g.FillRectangle(bb, buzzer);
            using (var slot = new Pen(Color.FromArgb(60, 60, 66), 1.2f))
                for (float x = buzzer.Left + 2f; x < buzzer.Right - 1f; x += 3f)
                    g.DrawLine(slot, x, buzzer.Top + 2f, x, buzzer.Bottom - 2f);
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.1f, a.Top, a.Width * 0.8f, a.Height * 0.5f), Lamp);
            DrawBase(g, cx, a.Bottom - baseH, a.Width * 0.5f, baseH);
        }
        // ----------------------------------------------------------------
        // 叉车支架灯
        // ----------------------------------------------------------------
        private void DrawFork(Graphics g, RectangleF a, HToolPalette pal)
        {
            using (var rod = new SolidBrush(CMetalDk))
            {
                g.FillRectangle(rod, a.Left + a.Width * 0.44f, a.Top + a.Height * 0.55f,
                    a.Width * 0.12f, a.Height * 0.3f);
                g.FillRectangle(rod, a.Left + a.Width * 0.2f, a.Bottom - a.Height * 0.16f,
                    a.Width * 0.6f, a.Height * 0.07f);
            }
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.12f, a.Top, a.Width * 0.76f, a.Height * 0.62f), Lamp);
        }
        // ----------------------------------------------------------------
        // 防爆护网警灯
        // ----------------------------------------------------------------
        private void DrawExplosion(Graphics g, RectangleF a, HToolPalette pal)
        {
            float baseH = Math.Min(a.Height * 0.13f, 12f);
            var lens = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.1f,
                a.Width * 0.76f, a.Height * 0.66f);
            DrawDomeLens(g, lens);
            // 金属护网（横竖交叉弧）
            using (var mesh = new Pen(Color.FromArgb(190, 90, 94, 100), Math.Max(1f, a.Width * 0.025f)))
            {
                float v = lens.Height * 0.78f;
                g.DrawArc(mesh, lens.Left - lens.Width * 0.06f, lens.Top, lens.Width * 1.12f, v, 180f, 180f);
                g.DrawArc(mesh, lens.Left + lens.Width * 0.12f, lens.Top + lens.Height * 0.1f,
                    lens.Width * 0.76f, v * 0.8f, 180f, 180f);
                for (int i = -2; i <= 2; i++)
                {
                    float x = lens.Left + lens.Width / 2f + i * lens.Width * 0.18f;
                    float dy = (float)Math.Sqrt(Math.Max(0f, 1f - (i * 0.36f) * (i * 0.36f))) * v / 2f;
                    g.DrawLine(mesh, x, lens.Bottom - 2f, x, lens.Top + v / 2f - dy + 2f);
                }
            }
            DrawBase(g, a.Left + a.Width / 2f, a.Bottom - baseH, a.Width * 0.72f, baseH);
        }
        // ----------------------------------------------------------------
        // 施工路障灯（黑黄条纹立柱）
        // ----------------------------------------------------------------
        private void DrawTrafficWarn(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float pw = a.Width * 0.16f;
            var post = new RectangleF(cx - pw / 2f, a.Top + a.Height * 0.4f, pw, a.Height * 0.5f);
            using (var yellow = new SolidBrush(Color.FromArgb(240, 190, 40)))
                g.FillRectangle(yellow, post);
            using (var black = new SolidBrush(Color.FromArgb(40, 40, 40)))
                for (float y = post.Top; y < post.Bottom; y += post.Width)
                    g.FillPolygon(black, new[]
                    {
                        new PointF(post.Left, y), new PointF(post.Right, y + post.Width * 0.6f),
                        new PointF(post.Right, y + post.Width), new PointF(post.Left, y + post.Width * 0.4f)
                    });
            g.FillRectangle(Brushes.DimGray, cx - a.Width * 0.26f, post.Bottom - 3f, a.Width * 0.52f, a.Height * 0.08f);
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.14f, a.Top, a.Width * 0.72f, a.Height * 0.46f),
                LampColors[(int)HAlarmColor.Amber]);
        }
        // ----------------------------------------------------------------
        // 高柱报警器
        // ----------------------------------------------------------------
        private void DrawPost(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = a.Height * 0.09f;
            using (var rod = new SolidBrush(CMetalDk))
            {
                g.FillRectangle(rod, cx - a.Width * 0.035f, a.Top + a.Height * 0.42f,
                    a.Width * 0.07f, a.Height * 0.49f);
                g.FillEllipse(rod, cx - a.Width * 0.24f, a.Bottom - baseH, a.Width * 0.48f, baseH);
            }
            // 蜂鸣箱
            var box = new RectangleF(cx - a.Width * 0.26f, a.Top + a.Height * 0.34f,
                a.Width * 0.52f, a.Height * 0.14f);
            using (var bb = new SolidBrush(Color.FromArgb(120, 124, 130)))
                g.FillRectangle(bb, box);
            using (var slot = new Pen(Color.FromArgb(70, 70, 74), 1f))
                for (float x = box.Left + 2f; x < box.Right - 1f; x += 3f)
                    g.DrawLine(slot, x, box.Top + 2f, x, box.Bottom - 2f);
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.1f, a.Top, a.Width * 0.8f, a.Height * 0.38f), Lamp);
        }
        // ----------------------------------------------------------------
        // 吸顶式
        // ----------------------------------------------------------------
        private void DrawCeiling(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float plateH = Math.Min(a.Height * 0.14f, 10f);
            using (var plate = new SolidBrush(CMetal))
                g.FillEllipse(plate, new RectangleF(a.Left + a.Width * 0.08f, a.Top,
                    a.Width * 0.84f, plateH * 1.6f));
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.18f, a.Top + plateH,
                a.Width * 0.64f, a.Height * 0.72f), Lamp);
        }
        // ----------------------------------------------------------------
        // 壁挂式（三角支架）
        // ----------------------------------------------------------------
        private void DrawWall(Graphics g, RectangleF a, HToolPalette pal)
        {
            using (var wall = new SolidBrush(Color.FromArgb(150, 154, 160)))
                g.FillRectangle(wall, a.Left, a.Top + a.Height * 0.1f, Math.Max(2.5f, a.Width * 0.08f),
                    a.Height * 0.62f);
            using (var arm = new SolidBrush(CMetalDk))
                g.FillPolygon(arm, new[]
                {
                    new PointF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.66f),
                    new PointF(a.Left + a.Width * 0.42f, a.Top + a.Height * 0.5f),
                    new PointF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.34f)
                });
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.28f, a.Top + a.Height * 0.14f,
                a.Width * 0.66f, a.Height * 0.56f), Lamp);
        }
        // ----------------------------------------------------------------
        // 纯蜂鸣器
        // ----------------------------------------------------------------
        private void DrawBuzzer(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.14f, -a.Height * 0.22f);
            if (box.Width <= 3f) return;
            using (var body = new LinearGradientBrush(box, Color.FromArgb(226, 150, 60), Color.FromArgb(160, 92, 30), 90f))
            using (var path = HBarBase.RoundPath(box, Math.Min(8f, box.Width * 0.16f)))
                g.FillPath(body, path);
            var inner = RectangleF.Inflate(box, -box.Width * 0.12f, -box.Height * 0.16f);
            using (var face = new SolidBrush(Color.FromArgb(40, 42, 46)))
                g.FillEllipse(face, inner);
            using (var slot = new Pen(Color.FromArgb(180, 180, 184), Math.Max(1f, box.Width * 0.018f)))
                for (int i = -3; i <= 3; i++)
                {
                    float y = inner.Top + inner.Height / 2f + i * inner.Height * 0.12f;
                    float dx = inner.Width / 2f * (1f - Math.Abs(i) * 0.18f) * 0.72f;
                    g.DrawLine(slot, inner.Left + inner.Width / 2f - dx, y,
                        inner.Left + inner.Width / 2f + dx, y);
                }
            if (Running)
                using (var wave = new Pen(Color.FromArgb((int)(Flash * 200f), Color.White), Math.Max(1f, a.Width * 0.02f)))
                    for (int k = 0; k < 2; k++)
                        g.DrawArc(wave, box.Right - 2f + k * 4f, box.Top - 2f - k * 4f,
                            box.Height * 0.5f + k * 8f, box.Height * 0.5f + k * 8f, -50f, 100f);
        }
        // ----------------------------------------------------------------
        // 报警指示盘（三灯 + 铭牌）
        // ----------------------------------------------------------------
        private void DrawPanel(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.06f, -a.Height * 0.1f);
            using (var body = new LinearGradientBrush(box, Color.FromArgb(70, 74, 80), Color.FromArgb(32, 35, 40), 90f))
            using (var path = HBarBase.RoundPath(box, 6f))
                g.FillPath(body, path);
            var colors = new[] { LampColors[0], LampColors[1], LampColors[2] };
            float d = Math.Min(box.Width * 0.22f, box.Height * 0.36f);
            float y = box.Top + box.Height * 0.12f;
            for (int i = 0; i < 3; i++)
            {
                float x = box.Left + box.Width * (0.18f + i * 0.32f) - d / 2f;
                bool on = Running && i == 0 ? Flash > 0.45f : (!Running && i == 2);
                using (var lb = new SolidBrush(on ? colors[i] : HToolPalettes.Mix(colors[i], Color.Gray, 0.6f)))
                    g.FillEllipse(lb, x, y, d, d);
                using (var rim = new Pen(CMetalDk, 1.2f))
                    g.DrawEllipse(rim, x, y, d, d);
            }
            using (var plate = new SolidBrush(Color.FromArgb(210, 214, 220)))
                g.FillRectangle(plate, box.Left + box.Width * 0.1f, box.Bottom - box.Height * 0.26f,
                    box.Width * 0.8f, box.Height * 0.14f);
        }
        // ----------------------------------------------------------------
        // 三脚架便携警灯
        // ----------------------------------------------------------------
        private void DrawPortable(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float topY = a.Top;
            float hipY = a.Top + a.Height * 0.52f;
            float footY = a.Bottom - 1f;
            using (var leg = new Pen(CMetalDk, Math.Max(1.5f, a.Width * 0.04f)))
            {
                g.DrawLine(leg, cx, hipY, a.Left + a.Width * 0.14f, footY);
                g.DrawLine(leg, cx, hipY, a.Right - a.Width * 0.14f, footY);
                g.DrawLine(leg, cx, hipY, cx, footY - a.Height * 0.06f);
            }
            DrawDomeIn(g, new RectangleF(a.Left + a.Width * 0.14f, topY,
                a.Width * 0.72f, a.Height * 0.52f), LampColors[(int)HAlarmColor.Amber]);
        }
    }
}