using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Valve
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 线缆样式：Classic 为双芯电缆带接线盒，其余 21 种覆盖双绞、三芯、同轴、排线、
    /// 架空、桥架、铠装、弹簧线、插头、端子排、水晶头、光纤、母线、拖链等。
    /// </summary>
    public enum HWireStyle
    {
        Classic = 0,        // 双芯电缆 + 接线盒
        Twisted = 1,        // 双绞线缆
        TriCore = 2,        // 三芯电缆
        Coax = 3,           // 同轴电缆
        Ribbon = 4,         // 排线
        Overhead = 5,       // 架空线（电杆）
        Tray = 6,           // 走线桥架
        Shielded = 7,       // 屏蔽电缆
        Armored = 8,        // 铠装电缆
        Coiled = 9,         // 螺旋弹簧线
        Plug = 10,          // 插头电源线
        Terminal = 11,      // 接线端子排
        Ethernet = 12,      // 网线水晶头
        Fiber = 13,         // 光纤跳线
        Busbar = 14,        // 母线排
        DragChain = 15,     // 拖链电缆
        HVTerminal = 16,    // 高压电缆终端
        Ground = 17,        // 接地线
        YSplit = 18,        // Y 型分叉
        Conduit = 19,       // 穿线管
        Reel = 20,          // 卷线盘
        Jumper = 21         // 面包板跳线
    }
    /// <summary>
    /// 电线控件（继承 HToolAnimBase）：22 种线缆外形、13 种色调，
    /// Energized 时沿线有黄色电流脉冲；支持文本标签。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("电线控件：22 种线缆外形、13 种色调，带电时沿线有黄色电流脉冲")]
    public class HWire : HToolAnimBase
    {
        private HWireStyle _style = HWireStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CoreRed = Color.FromArgb(205, 70, 60);
        private static readonly Color CoreBlue = Color.FromArgb(60, 110, 200);
        private static readonly Color Pulse = Color.FromArgb(250, 200, 60);
        public HWire() { Size = new Size(160, 66); }
        /// <summary>线缆外形样式。</summary>
        [HCategoryLanguage("导线"), HDisplayNameLanguage("线缆外形样式"), HDescriptionLanguage("线缆外形样式"), Browsable(true)]
        [DefaultValue(HWireStyle.Classic)]
        public HWireStyle WireStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>是否带电，带电时线缆上有电流脉冲。</summary>
        [HCategoryLanguage("导线"), HDisplayNameLanguage("是否带电"), HDescriptionLanguage("是否带电，带电时线缆上有电流脉冲"), Browsable(true)]
        [DefaultValue(false)]
        public bool Energized
        {
            get => Running;
            set => Running = value;
        }
        [Browsable(false)]
        public new bool Running
        {
            get => base.Running;
            set => base.Running = value;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HWireStyle.Twisted: DrawTwisted(g, a); break;
                case HWireStyle.TriCore: DrawMultiCore(g, a, 3); break;
                case HWireStyle.Coax: DrawCoax(g, a); break;
                case HWireStyle.Ribbon: DrawRibbon(g, a); break;
                case HWireStyle.Overhead: DrawOverhead(g, a); break;
                case HWireStyle.Tray: DrawTray(g, a); break;
                case HWireStyle.Shielded: DrawShielded(g, a, false); break;
                case HWireStyle.Armored: DrawShielded(g, a, true); break;
                case HWireStyle.Coiled: DrawCoiled(g, a); break;
                case HWireStyle.Plug: DrawPlug(g, a); break;
                case HWireStyle.Terminal: DrawTerminal(g, a); break;
                case HWireStyle.Ethernet: DrawEthernet(g, a); break;
                case HWireStyle.Fiber: DrawFiber(g, a); break;
                case HWireStyle.Busbar: DrawBusbar(g, a); break;
                case HWireStyle.DragChain: DrawDragChain(g, a); break;
                case HWireStyle.HVTerminal: DrawHVT(g, a); break;
                case HWireStyle.Ground: DrawGround(g, a); break;
                case HWireStyle.YSplit: DrawYSplit(g, a); break;
                case HWireStyle.Conduit: DrawConduit(g, a); break;
                case HWireStyle.Reel: DrawReel(g, a); break;
                case HWireStyle.Jumper: DrawJumper(g, a); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>沿直线运动的人字形电流脉冲。</summary>
        private void PulseLine(Graphics g, float x1, float y1, float x2, float y2)
        {
            if (!Running) return;
            float len = (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            if (len < 4f) return;
            float ux = (x2 - x1) / len, uy = (y2 - y1) / len, per = 5f;
            int n = 4;
            float off = (Phase / (float)Math.PI * 0.5f * len) % (len / n);
            using (var p = new Pen(Pulse, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                for (int i = 0; i < n; i++)
                {
                    float d = i * (len / n) + off;
                    if (d > len - 8f) continue;
                    float bx = x1 + ux * d, by = y1 + uy * d;
                    float tx = bx + ux * 7f, ty = by + uy * 7f;
                    p.Color = Color.FromArgb(225, Pulse);
                    g.DrawLine(p, bx - uy * per, by + ux * per, tx, ty);
                    g.DrawLine(p, bx + uy * per, by - ux * per, tx, ty);
                }
            }
        }
        // ----------------------------------------------------------------
        // Classic：双芯电缆 + 接线盒
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.56f, x1 = a.Left + 4f, x2 = a.Right - 4f;
            // 外护套
            var jacket = new RectangleF(x1, cy - 9f, x2 - x1, 18f);
            g.FillR(jacket, 8f, Color.FromArgb(58, 62, 68));
            // 双芯
            using (var rb = new SolidBrush(CoreRed))
                g.FillEllipse(rb, x1, cy - 6f, 12f, 7f);
            using (var bb = new SolidBrush(CoreBlue))
                g.FillEllipse(bb, x1, cy, 12f, 7f);
            g.Line(CoreRed, 5f, x1 + 6f, cy - 2.5f, x2 - 6f, cy - 2.5f);
            g.Line(CoreBlue, 5f, x1 + 6f, cy + 3.5f, x2 - 6f, cy + 3.5f);
            using (var rb2 = new SolidBrush(CoreRed))
                g.FillEllipse(rb2, x2 - 12f, cy - 6f, 12f, 7f);
            using (var bb2 = new SolidBrush(CoreBlue))
                g.FillEllipse(bb2, x2 - 12f, cy, 12f, 7f);
            // 接线盒
            var box = new RectangleF(a.Left + a.Width * 0.42f, cy - 14f, a.Width * 0.16f, 28f);
            g.Box(box, 3f, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.4f);
            g.Bolt(box.Left + 5f, box.Top + 5f, 1.8f, pal.Edge);
            g.Bolt(box.Right - 5f, box.Bottom - 5f, 1.8f, pal.Edge);
            PulseLine(g, x1 + 8f, cy - 2.5f, box.Left - 2f, cy - 2.5f);
            PulseLine(g, box.Right + 2f, cy + 3.5f, x2 - 8f, cy + 3.5f);
        }
        // ----------------------------------------------------------------
        // 新式线缆
        // ----------------------------------------------------------------
        private void DrawTwisted(Graphics g, RectangleF a)
        {
            float x1 = a.Left + 8f, x2 = a.Right - 8f, cy = a.Top + a.Height * 0.52f;
            int turns = 9;
            using (var rp = new Pen(CoreRed, 3.6f))
            using (var bp = new Pen(CoreBlue, 3.6f))
            {
                for (int i = 0; i < turns; i++)
                {
                    float xa = x1 + (x2 - x1) * i / turns, xb = x1 + (x2 - x1) * (i + 0.5f) / turns,
                        xc = x1 + (x2 - x1) * (i + 1f) / turns;
                    g.DrawBezier(rp, xa, cy - 6f, xb, cy - 6f, xb, cy + 6f, xc, cy + 6f);
                    g.DrawBezier(bp, xa, cy + 6f, xb, cy + 6f, xb, cy - 6f, xc, cy - 6f);
                }
            }
            PulseLine(g, x1, cy, x2, cy);
        }
        private void DrawMultiCore(Graphics g, RectangleF a, int n)
        {
            float cy = a.Top + a.Height * 0.56f, x1 = a.Left + 6f, x2 = a.Right - 6f;
            var jacket = new RectangleF(x1, cy - 11f, x2 - x1, 22f);
            g.FillR(jacket, 10f, Color.FromArgb(64, 68, 74));
            var cols = new[] { CoreRed, CoreBlue, Color.FromArgb(230, 200, 70), Color.FromArgb(120, 200, 90), Color.Black };
            for (int k = 0; k < n; k++)
            {
                float yy = cy - 7f + k * (14f / Math.Max(1, n - 1));
                if (n == 1) yy = cy;
                g.Line(cols[k % cols.Length], 4f, x1 + 6f, yy, x2 - 6f, yy);
                PulseLine(g, x1 + 8f, yy, x2 - 8f, yy);
            }
        }
        private void DrawCoax(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = a.Left + 6f, x2 = a.Right - 6f;
            g.FillR(new RectangleF(x1, cy - 10f, x2 - x1, 20f), 9f, Color.FromArgb(50, 54, 60));
            // 铜网屏蔽层
            using (var sh = new Pen(Color.FromArgb(150, 154, 160), 1.3f))
                for (float x = x1 + 4f; x < x2 - 4f; x += 5f)
                {
                    g.DrawLine(sh, x, cy - 7f, x + 5f, cy + 7f);
                    g.DrawLine(sh, x + 5f, cy - 7f, x, cy + 7f);
                }
            g.Line(Color.FromArgb(210, 150, 60), 3.4f, x1 + 4f, cy, x2 - 4f, cy);
            PulseLine(g, x1 + 6f, cy, x2 - 6f, cy);
        }
        private void DrawRibbon(Graphics g, RectangleF a)
        {
            float x1 = a.Left + 6f, x2 = a.Right - 6f, cy = a.Top + a.Height * 0.52f;
            var cols = new[] { CoreRed, Color.FromArgb(60, 64, 70), CoreBlue, Color.FromArgb(60, 64, 70), Color.FromArgb(60, 64, 70),
                Color.FromArgb(60, 64, 70), Color.FromArgb(60, 64, 70), Color.FromArgb(60, 64, 70) };
            using (var b = new SolidBrush(Color.FromArgb(80, 86, 94)))
                g.FillRectangle(b, x1, cy - 14f, x2 - x1, 28f);
            for (int k = 0; k < 8; k++)
            {
                float yy = cy - 12f + k * 3.4f;
                g.Line(cols[k], 1.6f, x1 + 2f, yy, x2 - 2f, yy);
            }
            PulseLine(g, x1 + 4f, cy - 10.3f, x2 - 4f, cy - 10.3f);
        }
        private void DrawOverhead(Graphics g, RectangleF a)
        {
            float px = a.Left + a.Width * 0.5f, top = a.Top + 4f, bot = a.Bottom - 4f;
            // 锥形电杆
            g.Poly(new[] { new PointF(px - 2.5f, top), new PointF(px + 2.5f, top),
                new PointF(px + 5f, bot), new PointF(px - 5f, bot) },
                Color.FromArgb(110, 96, 70), Color.FromArgb(80, 68, 50), 1.2f);
            float armY = top + a.Height * 0.2f, armW = a.Width * 0.4f;
            g.Line(Color.FromArgb(90, 78, 58), 3.4f, px - armW / 2f, armY, px + armW / 2f, armY);
            float wireY1 = armY + 4f, wireY2 = armY + 14f;
            // 瓷瓶
            foreach (float k in new[] { -0.42f, 0f, 0.42f })
                g.Ell(px + armW * k, armY + 1f, 3f, 3f, Color.FromArgb(120, 140, 160));
            using (var wire = new Pen(Color.FromArgb(60, 64, 70), 1.6f))
            {
                g.DrawLine(wire, a.Left + 4f, wireY1, a.Right - 4f, wireY1);
                g.DrawLine(wire, a.Left + 4f, wireY2, a.Right - 4f, wireY2);
            }
            PulseLine(g, a.Left + 4f, wireY1, a.Right - 4f, wireY1);
            PulseLine(g, a.Left + 4f, wireY2, a.Right - 4f, wireY2);
            // 横担斜撑
            g.Line(Color.FromArgb(90, 78, 58), 1.8f, px - armW * 0.3f, armY + 2f, px, armY + 12f);
            g.Line(Color.FromArgb(90, 78, 58), 1.8f, px + armW * 0.3f, armY + 2f, px, armY + 12f);
        }
        private void DrawTray(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = a.Left + 4f, x2 = a.Right - 4f;
            // 梯式桥架（上下帮 + 横档）
            g.Line(Color.FromArgb(120, 124, 130), 2.6f, x1, cy - 12f, x2, cy - 12f);
            g.Line(Color.FromArgb(120, 124, 130), 2.6f, x1, cy + 12f, x2, cy + 12f);
            using (var rung = new Pen(Color.FromArgb(140, 144, 150), 1.8f))
                for (float x = x1; x < x2; x += 14f)
                    g.DrawLine(rung, x, cy - 12f, x, cy + 12f);
            // 槽内三根电缆
            for (int k = -1; k <= 1; k++)
            {
                float yy = cy + k * 7f;
                g.Line(k == 0 ? CoreRed : (k < 0 ? CoreBlue : Color.FromArgb(80, 84, 90)),
                    3f, x1 + 4f, yy, x2 - 4f, yy);
                if (k == 0) PulseLine(g, x1 + 6f, yy, x2 - 6f, yy);
            }
        }
        private void DrawShielded(Graphics g, RectangleF a, bool armored)
        {
            float cy = a.Top + a.Height * 0.54f, x1 = a.Left + 6f, x2 = a.Right - 6f;
            var jacket = new RectangleF(x1, cy - 12f, x2 - x1, 24f);
            g.FillR(jacket, armored ? 4f : 11f, Color.FromArgb(56, 60, 66));
            if (armored)
            {
                // 钢带铠装斜纹
                using (var sp = new Pen(Color.FromArgb(150, 154, 160), 2f))
                    for (float x = x1; x < x2; x += 8f)
                    {
                        g.DrawLine(sp, x, cy - 11f, x + 10f, cy + 11f);
                        g.DrawLine(sp, x + 10f, cy - 11f, x, cy + 11f);
                    }
            }
            // 金属屏蔽网
            using (var sh = new Pen(Color.FromArgb(160, 164, 170), 1.1f))
                for (float x = x1 + 3f; x < x2 - 3f; x += 6f)
                    g.DrawLine(sh, x, cy - 7f, x + 5f, cy + 7f);
            g.Line(CoreRed, 3f, x1 + 5f, cy - 3f, x2 - 5f, cy - 3f);
            g.Line(CoreBlue, 3f, x1 + 5f, cy + 4f, x2 - 5f, cy + 4f);
            PulseLine(g, x1 + 7f, cy - 3f, x2 - 7f, cy - 3f);
        }
        private void DrawCoiled(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = a.Left + 10f, x2 = a.Right - 10f;
            // 两端直段
            g.Line(Color.FromArgb(58, 62, 68), 5f, a.Left + 2f, cy, x1 + 6f, cy);
            g.Line(Color.FromArgb(58, 62, 68), 5f, x2 - 6f, cy, a.Right - 2f, cy);
            // 螺旋段
            using (var p = new Pen(Color.FromArgb(58, 62, 68), 4f))
            {
                int loops = 12;
                for (int i = 0; i < loops; i++)
                {
                    float xa = x1 + (x2 - x1 - 12f) * i / loops + 6f;
                    float xb = x1 + (x2 - x1 - 12f) * (i + 1f) / loops + 6f;
                    g.DrawArc(p, xa, cy - 9f, xb - xa + 2f, 18f, 0f, 180f);
                    g.DrawArc(p, xa, cy - 9f, xb - xa + 2f, 18f, 180f, 180f);
                }
            }
            PulseLine(g, a.Left + 4f, cy, a.Right - 4f, cy);
        }
        private void DrawPlug(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.5f;
            // 墙壁插座
            var plate = new RectangleF(a.Right - 34f, cy - 18f, 30f, 36f);
            g.Box(plate, 4f, Color.FromArgb(235, 236, 240), Color.FromArgb(150, 154, 160), 1.3f);
            g.FillR(new RectangleF(plate.Left + 7f, plate.Top + 9f, 5f, 9f), 1f, Color.FromArgb(80, 84, 90));
            g.FillR(new RectangleF(plate.Left + 18f, plate.Top + 9f, 5f, 9f), 1f, Color.FromArgb(80, 84, 90));
            // 插头 + 线缆
            g.Box(new RectangleF(plate.Left - 12f, cy - 9f, 14f, 18f), 3f, Color.FromArgb(70, 74, 80), Color.FromArgb(40, 44, 50), 1.2f);
            g.Line(Color.FromArgb(58, 62, 68), 7f, a.Left + 2f, cy, plate.Left - 12f, cy);
            g.Line(CoreBlue, 4.4f, a.Left + 4f, cy, plate.Left - 10f, cy);
            PulseLine(g, a.Left + 6f, cy, plate.Left - 12f, cy);
        }
        private void DrawTerminal(Graphics g, RectangleF a)
        {
            float x1 = a.Left + 6f, x2 = a.Right - 6f, cy = a.Top + a.Height * 0.54f;
            int n = 8;
            float w = (x2 - x1) / n;
            for (int i = 0; i < n; i++)
            {
                var cell = new RectangleF(x1 + i * w, cy - 14f, w - 1f, 28f);
                g.Box(cell, 1f, i % 2 == 0 ? Color.FromArgb(232, 226, 210) : Color.FromArgb(220, 214, 198),
                    Color.FromArgb(150, 144, 120), 1f);
                // 螺钉
                g.Ell(cell.GetCenterX(), cell.Top + 7f, 3f, 3f, Color.FromArgb(120, 124, 130));
                g.Line(Color.FromArgb(90, 94, 100), 1.2f, cell.GetCenterX() - 2f, cell.Top + 7f,
                    cell.GetCenterX() + 2f, cell.Top + 7f);
            }
            // 进线 + 出线
            g.Line(CoreRed, 3f, a.Left + 2f, cy - 5f, x1 + w, cy - 5f);
            g.Line(CoreBlue, 3f, x2 - w, cy + 5f, a.Right - 2f, cy + 5f);
            PulseLine(g, a.Left + 4f, cy - 5f, x1 + w, cy - 5f);
        }
        private void DrawEthernet(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.54f;
            // 两个水晶头
            foreach (float dir in new[] { 0f, 1f })
            {
                float x = dir == 0 ? a.Left + 4f : a.Right - 30f;
                var hd = new RectangleF(x, cy - 8f, 26f, 16f);
                g.FillR(hd, 2f, Color.FromArgb(225, 230, 236));
                g.DrawR(hd, 2f, Color.FromArgb(120, 150, 180), 1.2f);
                // 弹片
                g.Line(Color.FromArgb(150, 170, 190), 1.6f, x + 4f, cy - 8f, x + 14f, cy - 14f);
                // 8 芯
                var cc = new[] { Color.White, Color.FromArgb(240, 160, 60), CoreRed, CoreBlue,
                    Color.White, Color.FromArgb(240, 160, 60), Color.FromArgb(120, 80, 40), Color.FromArgb(80, 140, 90) };
                for (int k = 0; k < 8; k++)
                    g.Line(cc[k], 1.1f, x + 3f + k * 2.6f, cy - 3f, x + 3f + k * 2.6f, cy + 5f);
            }
            g.Line(Color.FromArgb(58, 62, 68), 6f, a.Left + 30f, cy, a.Right - 30f, cy);
            PulseLine(g, a.Left + 32f, cy, a.Right - 32f, cy);
        }
        private void DrawFiber(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = a.Left + 8f, x2 = a.Right - 8f;
            // 黄色外被
            g.FillR(new RectangleF(x1, cy - 7f, x2 - x1, 14f), 6f, Color.FromArgb(230, 190, 60));
            // 玻璃纤芯（亮白）
            g.Line(Color.FromArgb(220, 240, 255), 2.2f, x1 + 4f, cy, x2 - 4f, cy);
            // 两端 LC 接头（蓝）
            g.Box(new RectangleF(x1 - 8f, cy - 6f, 12f, 12f), 2f, Color.FromArgb(90, 150, 210), Color.FromArgb(60, 100, 150), 1.1f);
            g.Box(new RectangleF(x2 - 4f, cy - 6f, 12f, 12f), 2f, Color.FromArgb(90, 150, 210), Color.FromArgb(60, 100, 150), 1.1f);
            if (Running)
                using (var p = new Pen(Color.FromArgb(230, Color.Red), 2f))
                {
                    float span = x2 - x1, off = (Phase / (float)Math.PI * 0.5f * span) % 26f;
                    for (float x = x1 - 26f + off; x < x2; x += 26f)
                        g.DrawLine(p, x, cy, x + 12f, cy);
                }
        }
        private void DrawBusbar(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.54f, x1 = a.Left + 8f, x2 = a.Right - 8f;
            var cols = new[] { Color.FromArgb(200, 160, 60), Color.FromArgb(180, 184, 190), Color.FromArgb(170, 90, 40) };
            for (int k = -1; k <= 1; k++)
            {
                float yy = cy + k * 9f;
                g.FillRectangle(new SolidBrush(cols[k + 1]), x1, yy - 3f, x2 - x1, 6f);
                g.DrawRectangle(new Pen(Color.FromArgb(120, 100, 60), 1f), x1, yy - 3f, x2 - x1, 6f);
                // 搭接螺栓
                foreach (float bx in new[] { x1 + 10f, x2 - 10f })
                {
                    g.Ell(bx, yy, 2.4f, 2.4f, Color.FromArgb(80, 84, 90));
                }
            }
            // 绝缘子
            foreach (float k in new[] { 0.25f, 0.75f })
            {
                float bx = a.Left + a.Width * k;
                g.FillR(new RectangleF(bx - 3f, a.Bottom - 12f, 6f, 8f), 2f, Color.FromArgb(150, 80, 60));
            }
            PulseLine(g, x1 + 14f, cy - 9f, x2 - 14f, cy - 9f);
        }
        private void DrawDragChain(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.5f, x1 = a.Left + 6f, x2 = a.Right - 6f;
            int n = 16;
            float w = (x2 - x1) / n;
            using (var link = new Pen(Color.FromArgb(110, 114, 120), 2.4f))
                for (int i = 0; i <= n; i++)
                {
                    float x = x1 + i * w;
                    g.DrawLine(link, x, cy - 12f, x + w * 0.7f, cy + 12f);
                    g.DrawLine(link, x, cy + 12f, x + w * 0.7f, cy - 12f);
                }
            // 内部电缆束
            g.Line(CoreBlue, 3f, x1 + 4f, cy, x2 - 4f, cy);
            g.Line(Color.FromArgb(80, 84, 90), 2.4f, x1 + 4f, cy + 5f, x2 - 4f, cy + 5f);
            PulseLine(g, x1 + 6f, cy, x2 - 6f, cy);
        }
        private void DrawHVT(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width * 0.34f, cy = a.Top + a.Height * 0.62f;
            // 瓷瓶终端（伞裙）
            using (var ins = new Pen(Color.FromArgb(150, 110, 90), 2.4f))
                for (int i = 0; i < 5; i++)
                    g.DrawArc(ins, cx - 11f, cy - 34f + i * 8f, 22f, 10f, 0f, 180f);
            g.FillR(new RectangleF(cx - 6f, cy - 38f, 12f, 46f), 4f, Color.FromArgb(200, 196, 186));
            // 接线端子
            g.Ell(cx, cy - 40f, 4f, 4f, Color.FromArgb(180, 140, 60));
            g.Line(Color.FromArgb(60, 64, 70), 3f, cx, cy - 44f, cx + a.Width * 0.3f, cy - 44f);
            // 下部电缆
            g.FillR(new RectangleF(cx - 9f, cy + 8f, 18f, a.Bottom - cy - 12f), 4f, Color.FromArgb(50, 54, 60));
            g.Line(CoreRed, 2.6f, cx, cy + 10f, cx, a.Bottom - 6f);
            // 高压警示
            var tag = new RectangleF(a.Right - 30f, a.Top + 4f, 26f, 26f);
            g.Poly(new[] { new PointF(tag.Left, tag.Bottom), new PointF(tag.Right, tag.Bottom),
                new PointF(tag.GetCenterX(), tag.Top) }, Color.FromArgb(230, 180, 40), Color.FromArgb(160, 120, 20), 1.4f);
            g.DrawString("⚡", new Font("微软雅黑", 10f, FontStyle.Bold), Brushes.Black, tag.GetCenterX() - 6f, tag.Top + 8f);
            PulseLine(g, cx, cy - 44f, cx + a.Width * 0.28f, cy - 44f);
        }
        private void DrawGround(Graphics g, RectangleF a)
        {
            float x1 = a.Left + 6f, gx = a.Right - a.Width * 0.22f, cy = a.Top + a.Height * 0.52f;
            // 黄绿双色线
            g.Line(Color.FromArgb(90, 160, 70), 4.4f, x1, cy, gx, cy);
            using (var stripe = new Pen(Color.FromArgb(230, 200, 60), 2.2f) { DashStyle = DashStyle.Dash })
                g.DrawLine(stripe, x1, cy, gx, cy);
            // 接地符号（三横）
            g.Line(Color.FromArgb(60, 64, 70), 2.6f, gx, cy, gx, cy - 8f);
            g.Line(Color.FromArgb(60, 64, 70), 3f, gx - 14f, cy - 8f, gx + 14f, cy - 8f);
            g.Line(Color.FromArgb(60, 64, 70), 2.4f, gx - 9f, cy - 14f, gx + 9f, cy - 14f);
            g.Line(Color.FromArgb(60, 64, 70), 1.8f, gx - 4f, cy - 20f, gx + 4f, cy - 20f);
            if (Running) PulseLine(g, x1 + 4f, cy, gx - 16f, cy);
        }
        private void DrawYSplit(Graphics g, RectangleF a)
        {
            float jx = a.Left + a.Width * 0.4f, jy = a.Top + a.Height * 0.52f;
            float y1 = jy - 12f, y2 = jy + 12f, ex = a.Right - 8f;
            g.Line(Color.FromArgb(58, 62, 68), 7f, a.Left + 4f, jy, jx, jy);
            g.Line(CoreRed, 3f, a.Left + 6f, jy, jx, jy);
            // 分叉节点盒
            g.Ell(jx, jy, 5f, 5f, Color.FromArgb(70, 74, 80));
            g.Line(Color.FromArgb(58, 62, 68), 5.4f, jx, jy, ex, y1);
            g.Line(Color.FromArgb(58, 62, 68), 5.4f, jx, jy, ex, y2);
            g.Line(CoreRed, 2.6f, jx, jy, ex, y1);
            g.Line(CoreBlue, 2.6f, jx, jy, ex, y2);
            PulseLine(g, a.Left + 8f, jy, jx - 4f, jy);
            PulseLine(g, jx + 4f, jy - 2f, ex - 4f, y1);
            PulseLine(g, jx + 4f, jy + 2f, ex - 4f, y2);
        }
        private void DrawConduit(Graphics g, RectangleF a)
        {
            float cy = a.Top + a.Height * 0.6f, x2 = a.Right - 8f;
            // 水平管
            g.FillR(new RectangleF(a.Left + 6f, cy - 8f, a.Width * 0.42f, 16f), 3f, Color.FromArgb(180, 184, 190));
            // 90° 弯头上行
            using (var path = new GraphicsPath())
            {
                float x0 = a.Left + 6f + a.Width * 0.42f, r = 16f;
                path.AddArc(x0 - r, cy - r * 2f, r * 2f, r * 2f, 270f, 90f);
                using (var p = new Pen(Color.FromArgb(180, 184, 190), 15f)) g.DrawPath(p, path);
                using (var p = new Pen(Color.FromArgb(120, 124, 130), 1.4f)) g.DrawPath(p, path);
            }
            g.Line(Color.FromArgb(180, 184, 190), 15f, a.Left + 6f + a.Width * 0.42f + 16f, cy - 16f,
                a.Left + 6f + a.Width * 0.42f + 16f, a.Top + 6f);
            // 管内导线
            g.Line(CoreRed, 2.2f, a.Left + 10f, cy - 2f, x2, cy - 2f - 0f);
            PulseLine(g, a.Left + 12f, cy - 2f, a.Left + a.Width * 0.5f, cy - 2f);
            // 管卡
            g.Line(Color.FromArgb(90, 94, 100), 2.4f, a.Left + a.Width * 0.2f, cy + 8f, a.Left + a.Width * 0.2f, a.Bottom - 4f);
        }
        private void DrawReel(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width * 0.36f, cy = a.Top + a.Height * 0.52f, r = a.Height * 0.36f;
            // 两侧挡板 + 绕线
            g.Ell(cx, cy, r, r * 1.05f, Color.FromArgb(230, 190, 60));
            g.Ell(cx, cy, r * 0.78f, r * 0.82f, Color.FromArgb(60, 64, 70));
            using (var coil = new Pen(Color.FromArgb(40, 44, 50), 2f))
                for (float rr = r * 0.3f; rr < r * 0.7f; rr += 4.5f)
                    g.DrawEllipse(coil, cx - rr, cy - rr * 1.05f, rr * 2f, rr * 2.1f);
            g.Ell(cx, cy, r * 0.16f, r * 0.16f, Color.FromArgb(120, 124, 130));
            // 摇把
            g.Line(Color.FromArgb(90, 94, 100), 3f, cx + r, cy, cx + r + 12f, cy - 10f);
            // 引出线 + 插头
            g.Line(Color.FromArgb(58, 62, 68), 5f, cx - r, cy + r * 0.5f, a.Right - 24f, a.Bottom - 10f);
            g.Box(new RectangleF(a.Right - 24f, a.Bottom - 18f, 18f, 12f), 2f, Color.FromArgb(70, 74, 80), Color.FromArgb(40, 44, 50), 1.1f);
            PulseLine(g, cx - r + 2f, cy + r * 0.5f, a.Right - 26f, a.Bottom - 10f);
        }
        private void DrawJumper(Graphics g, RectangleF a)
        {
            float y = a.Top + a.Height * 0.62f, x1 = a.Left + 8f, x2 = a.Right - 8f;
            // 面包板
            g.Box(new RectangleF(x1, y - 6f, x2 - x1, a.Height * 0.34f), 3f,
                Color.FromArgb(240, 242, 245), Color.FromArgb(150, 154, 160), 1.2f);
            using (var hole = new SolidBrush(Color.FromArgb(120, 124, 130)))
                for (float x = x1 + 8f; x < x2 - 6f; x += 10f)
                {
                    g.FillEllipse(hole, x - 1.5f, y - 1.5f, 3f, 3f);
                    g.FillEllipse(hole, x - 1.5f, y + 9f, 3f, 3f);
                }
            // 拱形跳线（红/蓝/黄）
            var cols = new[] { CoreRed, CoreBlue, Pulse };
            for (int k = 0; k < 3; k++)
            {
                float sx = x1 + 14f + k * 26f, ex2 = sx + 34f, top = y - 14f - k * 5f;
                using (var p = new Pen(cols[k], 2.4f))
                {
                    g.DrawLine(p, sx, y, sx, top);
                    g.DrawLine(p, sx, top, ex2, top);
                    g.DrawLine(p, ex2, top, ex2, y);
                }
                if (Running && k == 2) PulseLine(g, sx, top, ex2, top);
            }
        }
    }
}