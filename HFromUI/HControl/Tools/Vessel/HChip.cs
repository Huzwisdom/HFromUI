using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Vessel
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 芯片封装样式：Classic 为四边引脚 QFP，其余 21 种覆盖 BGA/CSP/PGA/LGA、
    /// SOP/DIP/QFN/PLCC、TO 功率管、片式阻容、排针、模块、COB、晶振、LED 与传感器等。
    /// </summary>
    public enum HChipStyle
    {
        Classic = 0,      // 四边引脚 QFP
        BGA = 1,          // 球栅阵列
        SOP = 2,          // 小外形贴片
        DIP = 3,          // 双列直插
        QFN = 4,          // 方形扁平无引脚
        SOT = 5,          // 小外形三极管
        TO92 = 6,         // 直插塑封三极管
        TO220 = 7,        // 带散热片功率管
        Diode = 8,        // 轴向二极管
        MELF = 9,         // 圆柱玻璃贴装元件
        ChipR = 10,       // 片式阻容
        PinHeader = 11,   // 单排排针
        Socket = 12,      // IC 锁紧座
        PowerModule = 13, // 功率模块
        COB = 14,         // 板上邦定芯片
        CSP = 15,         // 芯片级封装
        PLCC = 16,        // J 钩引脚载体
        PGA = 17,         // 插针网格阵列
        LGA = 18,         // 触点网格阵列
        Oscillator = 19,  // 金属封装晶振
        LED = 20,         // 发光二极管灯珠
        Sensor = 21       // 传感器模组
    }
    /// <summary>
    /// 芯片控件（继承 HToolAnimBase）：22 种元器件封装外形、13 种色调，静态图元；
    /// PinCount 控制 QFP 每边引脚数量。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("芯片控件：22 种元器件封装外形、13 种色调")]
    public class HChip : HToolAnimBase
    {
        private HChipStyle _style = HChipStyle.Classic;
        private int _pinCount = 7;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Teal);
        protected override HToolPalette ClassicPalette => _classic;
        public HChip()
        {
            Size = new Size(120, 120);
        }
        /// <summary>元器件封装外形。</summary>
        [HCategoryLanguage("芯片"), HDisplayNameLanguage("元器件封装外形"), HDescriptionLanguage("元器件封装外形"), Browsable(true)]
        [DefaultValue(HChipStyle.Classic)]
        public HChipStyle ChipStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>QFP 每边引脚数量（3..13）。</summary>
        [HCategoryLanguage("芯片"), HDisplayNameLanguage("引脚数量"), HDescriptionLanguage("QFP 每边引脚数量，3..13"), Browsable(true)]
        [DefaultValue(7)]
        public int PinCount
        {
            get => _pinCount;
            set { _pinCount = Math.Max(3, Math.Min(13, value)); Invalidate(); }
        }
        private HToolPalette Pal => Palette;
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Pal;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HChipStyle.BGA: DrawBGA(g, a, pal); break;
                case HChipStyle.SOP: DrawSOP(g, a, pal); break;
                case HChipStyle.DIP: DrawDIP(g, a, pal); break;
                case HChipStyle.QFN: DrawQFN(g, a, pal); break;
                case HChipStyle.SOT: DrawSOT(g, a, pal); break;
                case HChipStyle.TO92: DrawTO92(g, a, pal); break;
                case HChipStyle.TO220: DrawTO220(g, a, pal); break;
                case HChipStyle.Diode: DrawAxial(g, a, pal, true); break;
                case HChipStyle.MELF: DrawAxial(g, a, pal, false); break;
                case HChipStyle.ChipR: DrawChipR(g, a, pal); break;
                case HChipStyle.PinHeader: DrawPinHeader(g, a, pal); break;
                case HChipStyle.Socket: DrawSocket(g, a, pal); break;
                case HChipStyle.PowerModule: DrawPowerModule(g, a, pal); break;
                case HChipStyle.COB: DrawCOB(g, a, pal); break;
                case HChipStyle.CSP: DrawCSP(g, a, pal); break;
                case HChipStyle.PLCC: DrawPLCC(g, a, pal); break;
                case HChipStyle.PGA: DrawPGA(g, a, pal); break;
                case HChipStyle.LGA: DrawLGA(g, a, pal); break;
                case HChipStyle.Oscillator: DrawOscillator(g, a, pal); break;
                case HChipStyle.LED: DrawLED(g, a, pal); break;
                case HChipStyle.Sensor: DrawSensor(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>居中方形绘图区。</summary>
        private static RectangleF Square(RectangleF a, float ratio)
        {
            float s = Math.Min(a.Width, a.Height) * ratio;
            return new RectangleF(a.Left + (a.Width - s) / 2f, a.Top + (a.Height - s) / 2f, s, s);
        }
        /// <summary>封装本体（圆角 + 渐变 + 描边）。</summary>
        private static void Body(Graphics g, RectangleF box, HToolPalette pal, float radRatio)
        {
            using (var gp = HBarBase.RoundPath(box, box.Width * radRatio))
            using (var b = new LinearGradientBrush(box, HToolPalettes.Tint(pal.Main, 0.18f),
                HToolPalettes.Shade(pal.Main, 0.28f), 45f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.5f), gp);
            }
        }
        /// <summary>中心晶粒（深色 + 电路线）。</summary>
        private static void Die(Graphics g, RectangleF body, float ratio, HToolPalette pal)
        {
            float d = body.Width * ratio;
            var r = new RectangleF(body.X + (body.Width - d) / 2f, body.Y + (body.Height - d) / 2f, d, d);
            using (var db = new LinearGradientBrush(r, Color.FromArgb(38, 44, 56), Color.FromArgb(18, 22, 30), 45f))
                g.FillRectangle(db, r);
            g.DrawRectangle(new Pen(HToolPalettes.Tint(pal.Main, 0.2f), 1f), r.X, r.Y, r.Width, r.Height);
            using (var trace = new Pen(Color.FromArgb(110, pal.Accent), 1f))
                for (int i = 1; i < 4; i++)
                {
                    g.DrawLine(trace, r.X + d * i / 4f, r.Top + 3, r.X + d * i / 4f, r.Bottom - 3);
                    g.DrawLine(trace, r.Left + 3, r.Y + d * i / 4f, r.Right - 3, r.Y + d * i / 4f);
                }
        }
        // ----------------------------------------------------------------
        // Classic：四边引脚 QFP
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.62f);
            float pin = Math.Min(a.Width, a.Height) * 0.06f, gap = box.Width / (_pinCount + 1);
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
            {
                for (int i = 1; i <= _pinCount; i++)
                {
                    float p = box.X + gap * i, q = box.Y + gap * i;
                    g.FillRectangle(pinB, p - pin / 2f, box.Y - pin, pin, pin);
                    g.FillRectangle(pinB, p - pin / 2f, box.Bottom, pin, pin);
                    g.FillRectangle(pinB, box.X - pin, q - pin / 2f, pin, pin);
                    g.FillRectangle(pinB, box.Right, q - pin / 2f, pin, pin);
                }
            }
            Body(g, box, pal, 0.08f);
            Die(g, box, 0.44f, pal);
            g.Ell(box.X + box.Width * 0.13f, box.Y + box.Width * 0.13f,
                box.Width * 0.04f, box.Width * 0.04f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // BGA：球栅阵列（底部 + 右侧露出焊球阵列）
        // ----------------------------------------------------------------
        private void DrawBGA(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.58f);
            int n = 6;
            using (var ball = new SolidBrush(Color.FromArgb(150, 156, 168)))
            {
                // 右下两向露出的焊球
                for (int i = 0; i < n; i++)
                {
                    float bx = box.X + box.Width * (i + 0.5f) / n;
                    g.FillEllipse(ball, bx - 3f, box.Bottom + 1f, 6f, 6f);
                    float by = box.Y + box.Width * (i + 0.5f) / n;
                    g.FillEllipse(ball, box.Right + 1f, by - 3f, 6f, 6f);
                }
            }
            Body(g, box, pal, 0.06f);
            // 顶面隐约的球阵刻度
            using (var grid = new Pen(Color.FromArgb(70, pal.Accent), 0.8f))
                for (int i = 1; i < n; i++)
                {
                    float p = box.X + box.Width * i / n;
                    g.DrawLine(grid, p, box.Y + 5f, p, box.Bottom - 5f);
                    g.DrawLine(grid, box.X + 5f, p, box.Right - 5f, p);
                }
            g.Ell(box.X + box.Width * 0.12f, box.Y + box.Width * 0.12f,
                box.Width * 0.04f, box.Width * 0.04f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // SOP：两侧鸥翼引脚
        // ----------------------------------------------------------------
        private void DrawSOP(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.66f, h = a.Height * 0.34f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + (a.Height - h) / 2f, w, h);
            int n = 6;
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
                for (int i = 0; i < n; i++)
                {
                    float px = box.X + w * (i + 0.5f) / n, pw = w / n * 0.42f;
                    // 鸥翼：外伸 → 下折 → 贴脚
                    g.FillRectangle(pinB, px - pw / 2f, box.Bottom - 1f, pw, 4f);
                    g.FillRectangle(pinB, px - pw / 2f - 2f, box.Bottom + 2f, pw + 4f, 2.5f);
                    g.FillRectangle(pinB, px - pw / 2f, box.Y - 3f, pw, 4f);
                    g.FillRectangle(pinB, px - pw / 2f - 2f, box.Y - 4.5f, pw + 4f, 2.5f);
                }
            Body(g, box, pal, 0.16f);
            using (var d1 = new Font("Microsoft YaHei", 6.5f))
                g.DrawString("SOP", d1, Brushes.WhiteSmoke, box.X + 5f, box.Y + h * 0.32f);
        }
        // ----------------------------------------------------------------
        // DIP：双列直插长引脚
        // ----------------------------------------------------------------
        private void DrawDIP(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.56f, h = a.Height * 0.5f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + (a.Height - h) / 2f, w, h);
            int n = 8;
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
                for (int i = 0; i < n; i++)
                {
                    float py = box.Y + h * (i + 0.5f) / n;
                    g.FillRectangle(pinB, box.X - 9f, py - 1.6f, 9f, 3.2f);
                    g.FillRectangle(pinB, box.Right, py - 1.6f, 9f, 3.2f);
                }
            // 半圆缺口端
            using (var notch = new Pen(pal.Edge, 1.2f))
                g.DrawArc(notch, box.X + w / 2f - 6f, box.Y - 5f, 12f, 10f, 180f, 180f);
            Body(g, box, pal, 0.1f);
            g.Ell(box.X + box.Width * 0.1f, box.Y + box.Height * 0.1f,
                box.Width * 0.07f, box.Width * 0.07f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // QFN：四周平底焊盘
        // ----------------------------------------------------------------
        private void DrawQFN(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.6f);
            int n = 7;
            using (var pad = new SolidBrush(Color.FromArgb(170, 176, 186)))
                for (int i = 0; i < n; i++)
                {
                    float p = box.X + box.Width * (i + 0.5f) / n;
                    g.FillRectangle(pad, p - 3f, box.Y - 2.5f, 6f, 4f);
                    g.FillRectangle(pad, p - 3f, box.Bottom - 1.5f, 6f, 4f);
                    float q = box.Y + box.Width * (i + 0.5f) / n;
                    g.FillRectangle(pad, box.X - 2.5f, q - 3f, 4f, 6f);
                    g.FillRectangle(pad, box.Right - 1.5f, q - 3f, 4f, 6f);
                }
            Body(g, box, pal, 0.1f);
            // 散热焊盘（中心）
            g.Box(new RectangleF(box.X + box.Width * 0.3f, box.Y + box.Width * 0.3f,
                box.Width * 0.4f, box.Width * 0.4f), 2f,
                Color.FromArgb(150, 156, 168), pal.Edge, 1f);
        }
        // ----------------------------------------------------------------
        // SOT：小三极管（3 鸥翼脚）
        // ----------------------------------------------------------------
        private void DrawSOT(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.46f, h = a.Height * 0.3f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + a.Height * 0.3f, w, h);
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
            {
                foreach (float px in new[] { box.X + w * 0.28f, box.X + w * 0.72f })
                {
                    g.FillRectangle(pinB, px - 2.5f, box.Bottom - 1f, 5f, 6f);
                    g.FillRectangle(pinB, px - 4.5f, box.Bottom + 4f, 9f, 3f);
                }
                g.FillRectangle(pinB, box.X + w / 2f - 2.5f, box.Y - 7f, 5f, 7f);
                g.FillRectangle(pinB, box.X + w / 2f - 4.5f, box.Y - 10f, 9f, 3f);
            }
            Body(g, box, pal, 0.18f);
        }
        // ----------------------------------------------------------------
        // TO92：半圆塑封 + 三长脚
        // ----------------------------------------------------------------
        private void DrawTO92(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.42f, top = a.Top + a.Height * 0.3f, h = a.Height * 0.34f;
            using (var lead = new Pen(Color.FromArgb(190, 194, 200), 2.4f))
                foreach (float ox in new[] { -w * 0.26f, 0f, w * 0.26f })
                    g.DrawLine(lead, cx + ox, top, cx + ox * 1.5f, a.Top + a.Height * 0.12f);
            var box = new RectangleF(cx - w / 2f, top, w, h);
            using (var gp = new GraphicsPath())
            {
                gp.AddArc(box.X, box.Y, box.Width, box.Width * 0.9f, 180f, 180f);
                gp.AddRectangle(box);
                gp.CloseFigure();
                using (var b = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.12f)))
                    g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.4f), gp);
            }
        }
        // ----------------------------------------------------------------
        // TO220：金属散热片 + 塑封体 + 三脚
        // ----------------------------------------------------------------
        private void DrawTO220(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.44f, top = a.Top + a.Height * 0.16f;
            // 散热片
            var tab = new RectangleF(cx - w / 2f, top, w, a.Height * 0.42f);
            g.Box(tab, 2f, Color.FromArgb(176, 182, 192), pal.Edge, 1.3f);
            g.Ell(cx, top + 10f, 5f, 5f, Color.FromArgb(96, 102, 112));
            // 塑封体
            var body = new RectangleF(cx - w * 0.42f, top + a.Height * 0.36f, w * 0.84f, a.Height * 0.26f);
            Body(g, body, pal, 0.1f);
            // 引脚
            using (var lead = new Pen(Color.FromArgb(190, 194, 200), 3f))
                foreach (float ox in new[] { -w * 0.24f, 0f, w * 0.24f })
                    g.DrawLine(lead, cx + ox, body.Bottom, cx + ox, a.Bottom - a.Height * 0.1f);
        }
        // ----------------------------------------------------------------
        // Diode / MELF：轴向元件（diode 带色环，MELF 玻璃两端金属帽）
        // ----------------------------------------------------------------
        private void DrawAxial(Graphics g, RectangleF a, HToolPalette pal, bool diode)
        {
            float cy = a.Top + a.Height / 2f;
            float x1 = a.Left + a.Width * 0.16f, x2 = a.Right - a.Width * 0.16f;
            float r = a.Height * 0.13f;
            // 引线
            using (var lead = new Pen(Color.FromArgb(190, 194, 200), 2.2f))
            {
                g.DrawLine(lead, a.Left + a.Width * 0.06f, cy, x1, cy);
                g.DrawLine(lead, x2, cy, a.Right - a.Width * 0.06f, cy);
            }
            var body = new RectangleF(x1, cy - r, x2 - x1, r * 2f);
            Color c = diode ? HToolPalettes.Shade(pal.Main, 0.2f) : Color.FromArgb(96, 110, 86);
            g.FillCylH(body, HToolPalettes.Shade(c, 0.2f), HToolPalettes.Tint(c, 0.3f));
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), body.X, body.Y, body.Width, body.Height);
            // 端帽
            using (var cap = new SolidBrush(Color.FromArgb(170, 176, 186)))
            {
                g.FillRectangle(cap, x1, cy - r, 5f, r * 2f);
                g.FillRectangle(cap, x2 - 5f, cy - r, 5f, r * 2f);
            }
            if (diode)
                // 阴极色环
                using (var band = new SolidBrush(Color.FromArgb(220, 224, 230)))
                    g.FillRectangle(band, x2 - 11f, cy - r, 4f, r * 2f);
        }
        // ----------------------------------------------------------------
        // ChipR：片式阻容（0402 比例）
        // ----------------------------------------------------------------
        private void DrawChipR(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height / 2f;
            float w = a.Width * 0.56f, h = a.Height * 0.2f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, cy - h / 2f, w, h);
            using (var cap = new SolidBrush(Color.FromArgb(180, 186, 196)))
            {
                g.FillRectangle(cap, box.X, box.Y, w * 0.18f, h);
                g.FillRectangle(cap, box.Right - w * 0.18f, box.Y, w * 0.18f, h);
            }
            g.Box(new RectangleF(box.X + w * 0.18f, box.Y, w * 0.64f, h), h * 0.3f,
                HToolPalettes.Shade(pal.Main, 0.22f), pal.Edge, 1.1f);
            // 焊盘
            using (var pad = new SolidBrush(Color.FromArgb(214, 190, 120)))
            {
                g.FillRectangle(pad, box.X - 5f, cy - h * 0.7f, 7f, h * 1.4f);
                g.FillRectangle(pad, box.Right - 2f, cy - h * 0.7f, 7f, h * 1.4f);
            }
        }
        // ----------------------------------------------------------------
        // PinHeader：单排排针
        // ----------------------------------------------------------------
        private void DrawPinHeader(Graphics g, RectangleF a, HToolPalette pal)
        {
            int n = 8;
            float w = a.Width * 0.76f, h = a.Height * 0.22f;
            var strip = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + a.Height * 0.42f, w, h);
            g.Box(strip, 2f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.3f);
            using (var pinB = new SolidBrush(Color.FromArgb(200, 204, 212)))
                for (int i = 0; i < n; i++)
                {
                    float px = strip.X + w * (i + 0.5f) / n;
                    g.FillRectangle(pinB, px - 1.6f, strip.Y - a.Height * 0.16f, 3.2f, a.Height * 0.16f);
                    g.FillRectangle(pinB, px - 1.6f, strip.Bottom, 3.2f, a.Height * 0.08f);
                }
        }
        // ----------------------------------------------------------------
        // Socket：带锁紧杆的 IC 座
        // ----------------------------------------------------------------
        private void DrawSocket(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.66f);
            g.Box(box, 3f, HToolPalettes.Tint(pal.Main, 0.12f), pal.Edge, 1.5f);
            // 针孔阵列
            int n = 8;
            using (var hole = new SolidBrush(Color.FromArgb(50, 54, 62)))
                for (int i = 1; i < n; i++)
                    for (int j = 1; j < n; j++)
                        g.FillRectangle(hole, box.X + box.Width * i / n - 1.2f,
                            box.Y + box.Width * j / n - 1.2f, 2.4f, 2.4f);
            // 锁紧杆
            g.Line(pal.MetalDark, 3f, box.Right + 2f, box.Y + 6f, box.Right + 2f, box.Bottom - 6f);
            g.Line(pal.Metal, 4f, box.Right + 6f, box.Y + 10f, box.Right + 6f, box.Bottom - 10f);
        }
        // ----------------------------------------------------------------
        // PowerModule：大功率模块（螺栓孔 + 多引脚）
        // ----------------------------------------------------------------
        private void DrawPowerModule(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.72f, h = a.Height * 0.5f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + a.Height * 0.16f, w, h);
            g.Box(box, 4f, HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.6f);
            foreach (float cx in new[] { box.X + 10f, box.Right - 10f })
            {
                g.Ell(cx, box.Y + 9f, 4f, 4f, Color.FromArgb(70, 74, 82));
                g.Ell(cx, box.Bottom - 9f, 4f, 4f, Color.FromArgb(70, 74, 82));
            }
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
                for (int i = 0; i < 7; i++)
                {
                    float px = box.X + w * (i + 0.5f) / 7f;
                    g.FillRectangle(pinB, px - 2.2f, box.Bottom, 4.4f, a.Height * 0.12f);
                }
        }
        // ----------------------------------------------------------------
        // COB：PCB + 黑胶滴封 + 金线
        // ----------------------------------------------------------------
        private void DrawCOB(Graphics g, RectangleF a, HToolPalette pal)
        {
            var pcb = Square(a, 0.78f);
            g.Box(pcb, 3f, Color.FromArgb(58, 120, 74), pal.Edge, 1.3f);
            // 焊盘
            using (var pad = new SolidBrush(Color.FromArgb(214, 190, 120)))
                for (int i = 0; i < 6; i++)
                {
                    float px = pcb.X + pcb.Width * (i + 0.5f) / 6f;
                    g.FillRectangle(pad, px - 4f, pcb.Y + 4f, 8f, 5f);
                    g.FillRectangle(pad, px - 4f, pcb.Bottom - 9f, 8f, 5f);
                }
            // 黑胶滴
            float dr = pcb.Width * 0.34f;
            g.Ell(pcb.Left + pcb.Width / 2f, pcb.Top + pcb.Height / 2f, dr, dr * 0.82f,
                Color.FromArgb(24, 24, 28));
            g.DrawEllipse(new Pen(pal.Edge, 1.2f),
                pcb.Left + pcb.Width / 2f - dr, pcb.Top + pcb.Height / 2f - dr * 0.82f, dr * 2f, dr * 1.64f);
            // 金线
            using (var gold = new Pen(Color.FromArgb(230, 190, 80), 1.1f))
                for (int i = 0; i < 5; i++)
                {
                    float px = pcb.X + pcb.Width * (i + 0.5f) / 5f;
                    g.DrawLine(gold, px, pcb.Y + 9f, pcb.Left + pcb.Width / 2f + (i - 2) * 4f,
                        pcb.Top + pcb.Height / 2f - dr * 0.5f);
                }
        }
        // ----------------------------------------------------------------
        // CSP：芯片级封装（小型本体 + 边缘焊球）
        // ----------------------------------------------------------------
        private void DrawCSP(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.42f);
            using (var ball = new SolidBrush(Color.FromArgb(150, 156, 168)))
                for (int i = 0; i < 5; i++)
                {
                    float p = box.X + box.Width * (i + 0.5f) / 5f;
                    g.FillEllipse(ball, p - 2.4f, box.Bottom, 4.8f, 4.8f);
                    float q = box.Y + box.Width * (i + 0.5f) / 5f;
                    g.FillEllipse(ball, box.Right, q - 2.4f, 4.8f, 4.8f);
                }
            Body(g, box, pal, 0.1f);
            g.Ell(box.X + box.Width * 0.5f, box.Y + box.Width * 0.5f,
                box.Width * 0.12f, box.Width * 0.12f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // PLCC：四边 J 形钩脚
        // ----------------------------------------------------------------
        private void DrawPLCC(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.6f);
            int n = 7;
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
                for (int i = 0; i < n; i++)
                {
                    float p = box.X + box.Width * (i + 0.5f) / n;
                    g.FillRectangle(pinB, p - 2.5f, box.Bottom - 1f, 5f, 5f);
                    g.FillRectangle(pinB, p - 4.5f, box.Bottom + 3f, 9f, 3f);
                    g.FillRectangle(pinB, p - 2.5f, box.Y - 4f, 5f, 5f);
                    g.FillRectangle(pinB, p - 4.5f, box.Y - 6f, 9f, 3f);
                    float q = box.Y + box.Width * (i + 0.5f) / n;
                    g.FillRectangle(pinB, box.X - 4f, q - 2.5f, 5f, 5f);
                    g.FillRectangle(pinB, box.X - 6f, q - 4.5f, 3f, 9f);
                    g.FillRectangle(pinB, box.Right - 1f, q - 2.5f, 5f, 5f);
                    g.FillRectangle(pinB, box.Right + 3f, q - 4.5f, 3f, 9f);
                }
            Body(g, box, pal, 0.08f);
            g.Ell(box.X + box.Width * 0.13f, box.Y + box.Width * 0.13f,
                box.Width * 0.05f, box.Width * 0.05f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // PGA：插针网格阵列（顶面针阵示意 + 底排插针）
        // ----------------------------------------------------------------
        private void DrawPGA(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.6f);
            using (var pinB = new SolidBrush(Color.FromArgb(190, 194, 200)))
                for (int i = 0; i < 9; i++)
                {
                    float px = box.X + box.Width * (i + 0.5f) / 9f;
                    g.FillRectangle(pinB, px - 1.6f, box.Bottom, 3.2f, a.Height * 0.1f);
                }
            Body(g, box, pal, 0.05f);
            // 顶面针位网格
            using (var grid = new Pen(Color.FromArgb(90, pal.Accent), 0.8f))
                for (int i = 1; i < 8; i++)
                {
                    float p = box.X + box.Width * i / 8f;
                    g.DrawLine(grid, p, box.Y + 5f, p, box.Bottom - 5f);
                    g.DrawLine(grid, box.X + 5f, p, box.Right - 5f, p);
                }
        }
        // ----------------------------------------------------------------
        // LGA：平面触点网格阵列
        // ----------------------------------------------------------------
        private void DrawLGA(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = Square(a, 0.6f);
            Body(g, box, pal, 0.06f);
            int n = 8;
            using (var pad = new SolidBrush(Color.FromArgb(180, 186, 196)))
                for (int i = 1; i < n; i++)
                    for (int j = 1; j < n; j++)
                        g.FillEllipse(pad, box.X + box.Width * i / n - 2f,
                            box.Y + box.Width * j / n - 2f, 4f, 4f);
        }
        // ----------------------------------------------------------------
        // Oscillator：金属椭圆外壳晶振
        // ----------------------------------------------------------------
        private void DrawOscillator(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.58f, h = a.Height * 0.3f;
            var box = new RectangleF(a.Left + (a.Width - w) / 2f, a.Top + a.Height * 0.3f, w, h);
            using (var lead = new Pen(Color.FromArgb(190, 194, 200), 3f))
                foreach (float ox in new[] { -w * 0.3f, -w * 0.1f, w * 0.1f, w * 0.3f })
                    g.DrawLine(lead, box.X + w / 2f + ox, box.Bottom,
                        box.X + w / 2f + ox, box.Bottom + a.Height * 0.12f);
            using (var gp = HBarBase.RoundPath(box, h * 0.35f))
            using (var b = new LinearGradientBrush(box, Color.FromArgb(214, 218, 226),
                Color.FromArgb(140, 146, 160), 90f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.4f), gp);
            }
            using (var f = new Font("Microsoft YaHei", 6.5f))
                g.DrawString("XTAL", f, Brushes.DimGray, box.X + 6f, box.Y + h * 0.3f);
        }
        // ----------------------------------------------------------------
        // LED：带发光透镜的灯珠
        // ----------------------------------------------------------------
        private void DrawLED(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.46f;
            float r = Math.Min(a.Width, a.Height) * 0.26f;
            // 发光晕
            using (var halo = new SolidBrush(Color.FromArgb(60, pal.Accent)))
                g.FillEllipse(halo, cx - r * 1.5f, cy - r * 1.5f, r * 3f, r * 3f);
            // 透明透镜
            using (var lens = new SolidBrush(Color.FromArgb(150, HToolPalettes.Tint(pal.Accent, 0.4f))))
                g.FillEllipse(lens, cx - r, cy - r, r * 2f, r * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - r, cy - r, r * 2f, r * 2f);
            // 内部芯片 + 支架
            g.Ell(cx, cy + r * 0.1f, r * 0.28f, r * 0.28f, HToolPalettes.Shade(pal.Accent, 0.2f));
            using (var lead = new Pen(Color.FromArgb(190, 194, 200), 2.6f))
            {
                g.DrawLine(lead, cx - r * 0.34f, cy + r * 0.6f, cx - r * 0.5f, cy + r * 1.5f);
                g.DrawLine(lead, cx + r * 0.34f, cy + r * 0.6f, cx + r * 0.5f, cy + r * 1.5f);
            }
        }
        // ----------------------------------------------------------------
        // Sensor：PCB 模组 + 金属屏蔽盖（开窗）
        // ----------------------------------------------------------------
        private void DrawSensor(Graphics g, RectangleF a, HToolPalette pal)
        {
            var pcb = Square(a, 0.66f);
            g.Box(pcb, 3f, Color.FromArgb(58, 104, 120), pal.Edge, 1.3f);
            var lid = new RectangleF(pcb.X + pcb.Width * 0.16f, pcb.Y + pcb.Width * 0.12f,
                pcb.Width * 0.68f, pcb.Width * 0.6f);
            g.Box(lid, 4f, Color.FromArgb(176, 182, 192), pal.Edge, 1.4f);
            // 屏蔽盖开窗（传感孔阵列）
            using (var hole = new SolidBrush(Color.FromArgb(40, 44, 52)))
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        g.FillEllipse(hole,
                            lid.X + lid.Width * (0.24f + 0.26f * j) - 4f,
                            lid.Y + lid.Height * (0.24f + 0.26f * i) - 4f, 8f, 8f);
            // 外围贴片
            using (var smd = new SolidBrush(Color.FromArgb(40, 44, 52)))
            {
                g.FillRectangle(smd, pcb.X + 6f, pcb.Y + 6f, 10f, 5f);
                g.FillRectangle(smd, pcb.Right - 16f, pcb.Y + 6f, 10f, 5f);
                g.FillRectangle(smd, pcb.X + 6f, pcb.Bottom - 11f, 10f, 5f);
                g.FillRectangle(smd, pcb.Right - 16f, pcb.Bottom - 11f, 10f, 5f);
            }
        }
    }
}