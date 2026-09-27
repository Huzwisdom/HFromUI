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
    /// 信号灯外形：22 种工业信号指示外形——单灯、多层塔灯、交通灯、箭头灯、
    /// LED 电平条、环形扫描灯、灯簇等。Running 表示点亮/运行，停止时灯面灰暗。
    /// </summary>
    public enum HSignalLampStyle
    {
        Classic = 0,       // 圆形单灯
        Tower = 1,         // 三层塔灯（红黄蓝）
        Square = 2,        // 方形指示灯
        Panel = 3,         // 四孔信号盘
        TrafficV = 4,      // 立式三色交通灯
        TrafficH = 5,      // 卧式三色交通灯
        ArrowGo = 6,       // 直行箭头灯
        ArrowLeft = 7,     // 左转箭头灯
        ArrowRight = 8,    // 右转箭头灯
        LedBar = 9,        // LED 电平条
        Stack = 10,        // 两色叠层灯
        Beacon = 11,       // 小型警示灯
        Ring = 12,         // 环形扫描指示灯
        Column = 13,       // 竖直状态柱
        Dual = 14,         // 双联圆灯
        Gauge = 15,        // 仪表盘信号灯组
        Board = 16,        // 矩形信号板
        Beads = 17,        // 微型灯珠排
        Post = 18,         // 信号灯柱
        Bracket = 19,      // 支架灯
        Cluster = 20,      // 蜂窝灯簇
        SmartTower = 21    // 四层智能塔灯
    }
    /// <summary>
    /// 信号灯控件（继承 HToolAnimBase）：22 种信号指示外形、13 种色调；
    /// Running=true 点亮，灯色随主题，多层/交通灯使用工业语义色（红/琥珀/绿/蓝）。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("信号灯控件：22 种信号指示外形，运行时点亮")]
    public class HSignalLamp : HToolAnimBase
    {
        private HSignalLampStyle _style = HSignalLampStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Red);
        protected override HToolPalette ClassicPalette => _classic;
        // 工业语义灯色
        private static readonly Color CRed = Color.FromArgb(226, 59, 59);
        private static readonly Color CAmber = Color.FromArgb(245, 166, 35);
        private static readonly Color CGreen = Color.FromArgb(52, 168, 83);
        private static readonly Color CBlue = Color.FromArgb(59, 125, 226);
        private static readonly Color CWhite = Color.FromArgb(238, 242, 248);
        private static readonly Color CPanel = Color.FromArgb(38, 42, 48);
        /// <summary>信号灯外形。</summary>
        [HCategoryLanguage("信号灯"), HDisplayNameLanguage("信号灯外形样式"), HDescriptionLanguage("信号灯外形样式"), Browsable(true)]
        [DefaultValue(HSignalLampStyle.Classic)]
        public HSignalLampStyle LampStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
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
                case HSignalLampStyle.Tower: DrawTower(g, a, pal, new[] { CRed, CAmber, CGreen }, false, false); break;
                case HSignalLampStyle.Square: DrawLamp(g, a, pal.Main, Running, pal, true); break;
                case HSignalLampStyle.Panel: DrawPanel(g, a, pal, 2, 2); break;
                case HSignalLampStyle.TrafficV: DrawTraffic(g, a, pal, false); break;
                case HSignalLampStyle.TrafficH: DrawTraffic(g, a, pal, true); break;
                case HSignalLampStyle.ArrowGo: DrawArrowBoard(g, a, pal, 0); break;
                case HSignalLampStyle.ArrowLeft: DrawArrowBoard(g, a, pal, 1); break;
                case HSignalLampStyle.ArrowRight: DrawArrowBoard(g, a, pal, 2); break;
                case HSignalLampStyle.LedBar: DrawLedBar(g, a, pal, false); break;
                case HSignalLampStyle.Stack: DrawTower(g, a, pal, new[] { CRed, CGreen }, false, false); break;
                case HSignalLampStyle.Beacon: DrawBeacon(g, a, pal, CRed); break;
                case HSignalLampStyle.Ring: DrawRing(g, a, pal); break;
                case HSignalLampStyle.Column: DrawLedBar(g, a, pal, true); break;
                case HSignalLampStyle.Dual: DrawDual(g, a, pal); break;
                case HSignalLampStyle.Gauge: DrawPanel(g, a, pal, 1, 3); break;
                case HSignalLampStyle.Board: DrawPanel(g, a, pal, 1, 3); break;
                case HSignalLampStyle.Beads: DrawBeads(g, a, pal); break;
                case HSignalLampStyle.Post: DrawPost(g, a, pal); break;
                case HSignalLampStyle.Bracket: DrawBracket(g, a, pal); break;
                case HSignalLampStyle.Cluster: DrawCluster(g, a, pal); break;
                case HSignalLampStyle.SmartTower: DrawTower(g, a, pal, new[] { CRed, CAmber, CGreen, CBlue }, true, true); break;
                default: DrawLamp(g, a, pal.Main, Running, pal, false); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // 基础灯头：金属外圈 + 玻璃罩渐变 + 高光；off 时灰化
        // ----------------------------------------------------------------
        private void DrawLamp(Graphics g, RectangleF r, Color color, bool on, HToolPalette pal, bool square)
        {
            if (r.Width <= 2f || r.Height <= 2f) return;
            Color glass = on ? color : HToolPalettes.Mix(color, Color.FromArgb(96, 96, 96), 0.62f);
            Color glow = HToolPalettes.Tint(glass, 0.55f);
            Color dark = HToolPalettes.Shade(glass, 0.42f);
            if (on)
            {
                // 外发光晕
                for (int k = 3; k >= 1; k--)
                {
                    float dx = r.Width * 0.10f * k, dy = r.Height * 0.10f * k;
                    using (var halo = new SolidBrush(Color.FromArgb(18, color)))
                        FillLampShape(g, RectangleF.Inflate(r, dx, dy), square, halo);
                }
            }
            // 金属外壳
            using (var metal = new LinearGradientBrush(RectangleF.Inflate(r, 1f, 1f), pal.Metal, pal.MetalDark, 45f))
                FillLampShape(g, RectangleF.Inflate(r, r.Width * 0.08f, r.Height * 0.08f), square, metal);
            // 玻璃罩
            var gr = RectangleF.Inflate(r, -r.Width * 0.04f, -r.Height * 0.04f);
            using (var gb = new LinearGradientBrush(gr, glow, dark, 90f))
                FillLampShape(g, gr, square, gb);
            // 顶部反光
            var hi = new RectangleF(gr.Left + gr.Width * 0.18f, gr.Top + gr.Height * 0.12f,
                gr.Width * 0.34f, gr.Height * 0.22f);
            using (var hb = new SolidBrush(Color.FromArgb(on ? 120 : 40, Color.White)))
                FillLampShape(g, hi, square, hb);
            // 内圈描边
            using (var ep = new Pen(dark, Math.Max(1f, r.Width * 0.03f)))
                DrawLampShape(g, gr, square, ep);
        }
        private static void FillLampShape(Graphics g, RectangleF r, bool square, Brush b)
        {
            if (square) g.FillRectangle(b, r.X, r.Y, r.Width, r.Height);
            else g.FillEllipse(b, r);
        }
        private static void DrawLampShape(Graphics g, RectangleF r, bool square, Pen p)
        {
            if (square) g.DrawRectangle(p, r.X, r.Y, r.Width, r.Height);
            else g.DrawEllipse(p, r);
        }
        // ----------------------------------------------------------------
        // 多层塔灯：竖直叠层 + 底座，可选蜂鸣帽/顶盖
        // ----------------------------------------------------------------
        private void DrawTower(Graphics g, RectangleF a, HToolPalette pal, Color[] layers, bool cap, bool buzzer)
        {
            int n = layers.Length;
            float postW = Math.Min(a.Width * 0.12f, 10f);
            float baseH = Math.Min(a.Height * 0.10f, 12f);
            float capH = buzzer ? Math.Min(a.Height * 0.10f, 12f) : Math.Min(a.Height * 0.06f, 8f);
            float stackTop = a.Top + capH + a.Height * 0.04f;
            float stackH = a.Height - capH - baseH - a.Height * 0.10f;
            if (stackH < n * 3f) stackH = n * 3f;
            float layerH = stackH / n;
            float cx = a.Left + a.Width / 2f;
            float lampW = Math.Min(a.Width * 0.62f, layerH * 1.7f);
            if (lampW < 5f) lampW = Math.Min(5f, a.Width);
            // 顶盖/蜂鸣帽
            using (var metal = new SolidBrush(pal.MetalDark))
            {
                if (buzzer)
                {
                    var br = new RectangleF(cx - lampW * 0.34f, a.Top, lampW * 0.68f, capH);
                    g.FillRectangle(metal, br);
                    using (var slot = new Pen(Color.Black, 1f))
                        for (float sx = br.Left + 2f; sx < br.Right - 1f; sx += 2.6f)
                            g.DrawLine(slot, sx, br.Top + 2f, sx, br.Bottom - 2f);
                }
                else
                {
                    g.FillRectangle(metal, cx - lampW * 0.42f, a.Top, lampW * 0.84f, capH);
                }
            }
            // 灯层
            for (int i = 0; i < n; i++)
            {
                var lr = new RectangleF(cx - lampW / 2f, stackTop + i * layerH, lampW, layerH - 1f);
                DrawLamp(g, lr, layers[i], Running, pal, false);
            }
            // 中心杆 + 底座
            using (var rod = new SolidBrush(pal.MetalDark))
            {
                float baseTop = a.Bottom - baseH;
                g.FillRectangle(rod, cx - postW / 2f, stackTop - 2f, postW, baseTop - stackTop + 2f);
                g.FillRectangle(rod, cx - lampW * 0.6f, baseTop, lampW * 1.2f, baseH * 0.45f);
                g.FillRectangle(rod, cx - lampW * 0.4f, baseTop + baseH * 0.45f, lampW * 0.8f, baseH * 0.55f);
            }
        }
        // ----------------------------------------------------------------
        // 交通灯盒
        // ----------------------------------------------------------------
        private void DrawTraffic(Graphics g, RectangleF a, HToolPalette pal, bool horizontal)
        {
            var colors = new[] { CRed, CAmber, CGreen };
            float pad = Math.Min(a.Width, a.Height) * 0.08f;
            var box = RectangleF.Inflate(a, -pad, -pad);
            if (box.Width <= 3f || box.Height <= 3f) return;
            using (var body = new LinearGradientBrush(box, Color.FromArgb(64, 68, 74), Color.FromArgb(30, 33, 38), horizontal ? 0f : 90f))
            using (var path = HBarBase.RoundPath(box, Math.Min(8f, Math.Min(box.Width, box.Height) * 0.18f)))
                g.FillPath(body, path);
            // 后挂杆
            using (var rod = new SolidBrush(pal.MetalDark))
            {
                if (horizontal)
                    g.FillRectangle(rod, box.Left, box.Top + box.Height * 0.2f, Math.Max(2f, box.Width * 0.08f), box.Height * 0.6f);
                else
                    g.FillRectangle(rod, box.Left + box.Width * 0.46f, box.Top, box.Width * 0.08f, Math.Max(2f, box.Height * 0.08f));
            }
            for (int i = 0; i < 3; i++)
            {
                RectangleF lr;
                float gap;
                if (horizontal)
                {
                    gap = box.Width * 0.06f;
                    float d = (box.Width - gap * 4) / 3f;
                    lr = new RectangleF(box.Left + gap + i * (d + gap), box.Top + (box.Height - d) / 2f, d, d);
                }
                else
                {
                    gap = box.Height * 0.06f;
                    float d = (box.Height - gap * 4) / 3f;
                    lr = new RectangleF(box.Left + (box.Width - d) / 2f, box.Top + gap + i * (d + gap), d, d);
                }
                // 交通灯点亮规则：停止红、运行绿、切换间隙琥珀（用相位粗略表达）
                bool lit = Running ? (i == 2 || (i == 1 && Phase % Math.PI > Math.PI * 0.8f)) : i == 0;
                DrawLamp(g, lr, colors[i], lit, pal, false);
            }
        }
        // ----------------------------------------------------------------
        // 黑色面板灯组（rows×cols）
        // ----------------------------------------------------------------
        private void DrawPanel(Graphics g, RectangleF a, HToolPalette pal, int rows, int cols)
        {
            var box = RectangleF.Inflate(a, -Math.Min(a.Width, a.Height) * 0.05f, -Math.Min(a.Width, a.Height) * 0.05f);
            using (var body = new LinearGradientBrush(box, Color.FromArgb(58, 62, 68), CPanel, 90f))
            using (var path = HBarBase.RoundPath(box, Math.Min(8f, Math.Min(box.Width, box.Height) * 0.12f)))
                g.FillPath(body, path);
            var colors = new[] { CRed, CAmber, CGreen, CBlue, CWhite, CRed, CAmber, CGreen };
            float mx = box.Width * 0.10f, my = box.Height * 0.14f;
            float cellW = (box.Width - mx * (cols + 1)) / cols;
            float cellH = (box.Height - my * (rows + 1)) / rows;
            int idx = 0;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    float d = Math.Min(cellW, cellH);
                    var lr = new RectangleF(box.Left + mx + c * (cellW + mx) + (cellW - d) / 2f,
                        box.Top + my + r * (cellH + my) + (cellH - d) / 2f, d, d);
                    bool lit = Running ? (idx % 2 == 0) : (idx == 0);
                    DrawLamp(g, lr, colors[idx % colors.Length], lit, pal, false);
                    idx++;
                }
        }
        // ----------------------------------------------------------------
        // 箭头方向灯板：dir 0=直行 1=左 2=右
        // ----------------------------------------------------------------
        private void DrawArrowBoard(Graphics g, RectangleF a, HToolPalette pal, int dir)
        {
            var box = RectangleF.Inflate(a, -Math.Min(a.Width, a.Height) * 0.06f, -Math.Min(a.Width, a.Height) * 0.06f);
            using (var body = new SolidBrush(Color.FromArgb(28, 31, 36)))
            using (var path = HBarBase.RoundPath(box, Math.Min(8f, Math.Min(box.Width, box.Height) * 0.14f)))
                g.FillPath(body, path);
            Color arrow = Running ? CGreen : HToolPalettes.Mix(CGreen, Color.Gray, 0.6f);
            float cx = box.Left + box.Width / 2f, cy = box.Top + box.Height / 2f;
            float s = Math.Min(box.Width, box.Height) * 0.32f;
            if (s < 2f) return;
            using (var ab = new SolidBrush(Color.FromArgb(Running ? 235 : 110, arrow)))
            using (var ap = new Pen(ab, Math.Max(2f, s * 0.16f)) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
            {
                if (dir == 0)
                {
                    g.DrawLine(ap, cx, cy + s, cx, cy - s * 0.7f);
                    g.DrawLine(ap, cx - s * 0.55f, cy - s * 0.1f, cx, cy - s * 0.7f);
                    g.DrawLine(ap, cx + s * 0.55f, cy - s * 0.1f, cx, cy - s * 0.7f);
                }
                else
                {
                    float dirx = dir == 1 ? -1f : 1f;
                    g.DrawLine(ap, cx - dirx * s, cy, cx + dirx * s * 0.7f, cy);
                    g.DrawLine(ap, cx + dirx * s * 0.1f, cy - s * 0.55f, cx + dirx * s * 0.7f, cy);
                    g.DrawLine(ap, cx + dirx * s * 0.1f, cy + s * 0.55f, cx + dirx * s * 0.7f, cy);
                }
            }
        }
        // ----------------------------------------------------------------
        // LED 电平条/状态柱
        // ----------------------------------------------------------------
        private void DrawLedBar(Graphics g, RectangleF a, HToolPalette pal, bool vertical)
        {
            int segs = 6;
            var colors = new[] { CGreen, CGreen, CGreen, CAmber, CAmber, CRed };
            float pad = Math.Min(a.Width, a.Height) * 0.08f;
            var box = RectangleF.Inflate(a, -pad, -pad);
            using (var body = new SolidBrush(Color.FromArgb(32, 35, 40)))
            using (var path = HBarBase.RoundPath(box, 6f))
                g.FillPath(body, path);
            float gap = vertical ? box.Height * 0.02f : box.Width * 0.02f;
            int lit = Running ? Math.Max(1, (int)(Math.Abs(Math.Sin(Phase)) * segs + 1.4f)) : 1;
            for (int i = 0; i < segs; i++)
            {
                RectangleF sr;
                if (vertical)
                {
                    float sh = (box.Height - gap * (segs + 1)) / segs;
                    sr = new RectangleF(box.Left + box.Width * 0.22f, box.Bottom - gap - (i + 1) * sh - i * gap,
                        box.Width * 0.56f, sh);
                }
                else
                {
                    float sw = (box.Width - gap * (segs + 1)) / segs;
                    sr = new RectangleF(box.Left + gap + i * (sw + gap), box.Top + box.Height * 0.30f,
                        sw, box.Height * 0.4f);
                }
                DrawLamp(g, sr, colors[i], i < lit, pal, true);
            }
        }
        // ----------------------------------------------------------------
        // 小型警示灯（圆罩 + 底座）
        // ----------------------------------------------------------------
        private void DrawBeacon(Graphics g, RectangleF a, HToolPalette pal, Color color)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = a.Height * 0.18f;
            var dome = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.08f,
                a.Width * 0.72f, a.Height * 0.66f);
            DrawLamp(g, dome, color, Running, pal, false);
            using (var metal = new LinearGradientBrush(
                new RectangleF(dome.Left - a.Width * 0.06f, dome.Bottom - 2f, dome.Width + a.Width * 0.12f, baseH),
                pal.Metal, pal.MetalDark, 90f))
                g.FillRectangle(metal, dome.Left - a.Width * 0.06f, dome.Bottom - 2f,
                    dome.Width + a.Width * 0.12f, baseH * 0.7f);
            // 旋转光束
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(cx, dome.Top + dome.Height * 0.55f);
                g.RotateTransform(SpinAngle);
                using (var beam = new SolidBrush(Color.FromArgb(40, color)))
                    g.FillPie(beam, -dome.Width * 0.9f, -dome.Height * 0.9f,
                        dome.Width * 1.8f, dome.Height * 1.8f, -28f, 56f);
                g.Restore(st);
            }
        }
        // ----------------------------------------------------------------
        // 环形扫描指示灯
        // ----------------------------------------------------------------
        private void DrawRing(Graphics g, RectangleF a, HToolPalette pal)
        {
            float d = Math.Min(a.Width, a.Height) * 0.8f;
            var r = new RectangleF(a.Left + (a.Width - d) / 2f, a.Top + (a.Height - d) / 2f, d, d);
            Color c = pal.Accent;
            using (var track = new Pen(HToolPalettes.Mix(c, Color.Gray, 0.7f), Math.Max(2f, d * 0.12f)))
                g.DrawEllipse(track, r);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(r.Left + d / 2f, r.Top + d / 2f);
                g.RotateTransform(SpinAngle);
                using (var lit = new Pen(c, Math.Max(2f, d * 0.12f)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(lit, -d / 2f, -d / 2f, d, d, -20f, 110f);
                g.Restore(st);
            }
            float cd = d * 0.22f;
            DrawLamp(g, new RectangleF(r.Left + (d - cd) / 2f, r.Top + (d - cd) / 2f, cd, cd), c, Running, pal, false);
        }
        // ----------------------------------------------------------------
        // 双联圆灯
        // ----------------------------------------------------------------
        private void DrawDual(Graphics g, RectangleF a, HToolPalette pal)
        {
            float d = Math.Min(a.Width * 0.42f, a.Height * 0.6f);
            float y = a.Top + (a.Height - d) / 2f;
            DrawLamp(g, new RectangleF(a.Left + a.Width * 0.12f, y, d, d), CRed, !Running, pal, false);
            DrawLamp(g, new RectangleF(a.Right - a.Width * 0.12f - d, y, d, d), CGreen, Running, pal, false);
        }
        // ----------------------------------------------------------------
        // 微型灯珠排
        // ----------------------------------------------------------------
        private void DrawBeads(Graphics g, RectangleF a, HToolPalette pal)
        {
            int n = 5;
            float d = Math.Min(a.Width / (n * 1.6f), a.Height * 0.55f);
            if (d < 2f) d = 2f;
            float y = a.Top + (a.Height - d) / 2f;
            float step = (a.Width - d) / (n - 1);
            var colors = new[] { CRed, CAmber, CGreen, CBlue, CWhite };
            for (int i = 0; i < n; i++)
                DrawLamp(g, new RectangleF(a.Left + i * step, y, d, d), colors[i], Running, pal, false);
        }
        // ----------------------------------------------------------------
        // 信号灯柱（灯头 + 高立柱 + 底盘）
        // ----------------------------------------------------------------
        private void DrawPost(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float baseH = a.Height * 0.10f;
            using (var metal = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(metal, cx - Math.Max(1.5f, a.Width * 0.04f), a.Top + a.Height * 0.42f,
                    Math.Max(3f, a.Width * 0.08f), a.Height * 0.48f);
                g.FillEllipse(metal, cx - a.Width * 0.22f, a.Bottom - baseH, a.Width * 0.44f, baseH);
            }
            float d = Math.Min(a.Width * 0.62f, a.Height * 0.4f);
            DrawLamp(g, new RectangleF(cx - d / 2f, a.Top, d, d), pal.Main, Running, pal, false);
        }
        // ----------------------------------------------------------------
        // L 型支架灯
        // ----------------------------------------------------------------
        private void DrawBracket(Graphics g, RectangleF a, HToolPalette pal)
        {
            using (var metal = new SolidBrush(pal.MetalDark))
            {
                float w = Math.Max(2f, a.Width * 0.08f);
                g.FillRectangle(metal, a.Left + a.Width * 0.18f, a.Top + a.Height * 0.3f,
                    a.Width * 0.5f, w);
                g.FillRectangle(metal, a.Left + a.Width * 0.18f, a.Top + a.Height * 0.3f,
                    w, a.Height * 0.55f);
            }
            float d = Math.Min(a.Width * 0.44f, a.Height * 0.42f);
            DrawLamp(g, new RectangleF(a.Right - a.Width * 0.18f - d, a.Top + a.Height * 0.12f, d, d),
                pal.Main, Running, pal, false);
        }
        // ----------------------------------------------------------------
        // 蜂窝灯簇（7 灯：中心 1 + 周围 6）
        // ----------------------------------------------------------------
        private void DrawCluster(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float ringR = Math.Min(a.Width, a.Height) * 0.30f;
            float d = Math.Min(a.Width, a.Height) * 0.26f;
            var colors = new[] { CWhite, CRed, CAmber, CGreen, CBlue, CRed, CAmber };
            DrawLamp(g, new RectangleF(cx - d / 2f, cy - d / 2f, d, d), colors[0], Running, pal, false);
            for (int i = 0; i < 6; i++)
            {
                double ang = Math.PI / 6 + i * Math.PI / 3;
                float x = cx + (float)Math.Cos(ang) * ringR - d / 2f;
                float y = cy + (float)Math.Sin(ang) * ringR - d / 2f;
                DrawLamp(g, new RectangleF(x, y, d, d), colors[i + 1], Running && i % 2 == 0, pal, false);
            }
        }
    }
}