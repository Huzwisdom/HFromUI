using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Power
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 提升/起重设备样式：Classic 为封闭式胶带斗提，其余 21 种覆盖环链斗提、Z 型转斗、
    /// 垂直振动/螺旋/气力提升、矿用提升机、施工升降机、电动/手拉/气动葫芦、井架、
    /// 爬斗、卷扬机、堆垛机、剪叉/桅柱升降台、移动式斗提及真空气力吸料等。
    /// </summary>
    public enum HElevatorStyle
    {
        Classic = 0,          // 封闭胶带斗提
        BeltBucket = 1,       // 开式胶带斗提
        ChainBucket = 2,      // 环链斗提
        ZType = 3,            // Z 型转斗提升机
        Pendulum = 4,         // 摇臂悬挂斗式输送机
        VibroVertical = 5,    // 垂直振动螺旋提升机
        Pneumatic = 6,        // 气力提升泵
        ScrewVertical = 7,    // 立式螺旋提升机
        Mobile = 8,           // 移动式斗提
        MineHoist = 9,        // 矿用提升机
        ConstructionHoist = 10,// 施工升降机
        ChainHoist = 11,      // 电动葫芦
        Headframe = 12,       // 竖井井架提升
        SkipHoist = 13,       // 爬斗/翻斗提升机
        Winch = 14,           // 电动卷扬机
        LeverHoist = 15,      // 手拉葫芦
        StackerCrane = 16,    // 巷道堆垛机
        ScissorLift = 17,     // 剪叉式升降台
        MastLift = 18,        // 桅柱式高空作业平台
        AirHoist = 19,        // 气动葫芦
        InclineScrew = 20,    // 倾斜螺旋提升机
        VacuumLift = 21       // 真空气力吸料提升
    }
    /// <summary>
    /// 提升/起重设备控件（继承 HToolAnimBase）：22 种提升起重外形、13 种色调，
    /// Running 时料斗循环、卷筒转动、吊钩或平台升降。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("提升起重设备控件：22 种斗提/葫芦/升降机外形、13 种色调，运行时料斗循环或吊钩升降")]
    public class HElevator : HToolAnimBase
    {
        private HElevatorStyle _style = HElevatorStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HElevator() { Size = new Size(120, 200); }
        /// <summary>提升/起重设备外形样式。</summary>
        [HCategoryLanguage("升降梯"), HDisplayNameLanguage("提升起重设备外形样式"), HDescriptionLanguage("提升起重设备外形样式"), Browsable(true)]
        [DefaultValue(HElevatorStyle.Classic)]
        public HElevatorStyle ElevatorStyle
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
            switch (_style)
            {
                case HElevatorStyle.BeltBucket: DrawLift(g, a, pal, false); break;
                case HElevatorStyle.ChainBucket: DrawChain(g, a, pal); break;
                case HElevatorStyle.ZType: DrawZ(g, a, pal); break;
                case HElevatorStyle.Pendulum: DrawPendulum(g, a, pal); break;
                case HElevatorStyle.VibroVertical: DrawVibro(g, a, pal); break;
                case HElevatorStyle.Pneumatic: DrawPneumatic(g, a, pal); break;
                case HElevatorStyle.ScrewVertical: DrawScrewV(g, a, pal); break;
                case HElevatorStyle.Mobile: DrawLift(g, a, pal, true); break;
                case HElevatorStyle.MineHoist: DrawFrameHoist(g, a, pal, false); break;
                case HElevatorStyle.Headframe: DrawFrameHoist(g, a, pal, true); break;
                case HElevatorStyle.ConstructionHoist: DrawConstruction(g, a, pal); break;
                case HElevatorStyle.ChainHoist: DrawHookHoist(g, a, pal, false); break;
                case HElevatorStyle.AirHoist: DrawHookHoist(g, a, pal, true); break;
                case HElevatorStyle.LeverHoist: DrawLever(g, a, pal); break;
                case HElevatorStyle.SkipHoist: DrawSkip(g, a, pal); break;
                case HElevatorStyle.Winch: DrawWinch(g, a, pal); break;
                case HElevatorStyle.StackerCrane: DrawStacker(g, a, pal); break;
                case HElevatorStyle.ScissorLift: DrawScissor(g, a, pal); break;
                case HElevatorStyle.MastLift: DrawMast(g, a, pal); break;
                case HElevatorStyle.InclineScrew: DrawInclineScrew(g, a, pal); break;
                case HElevatorStyle.VacuumLift: DrawVacuum(g, a, pal); break;
                default: DrawLift(g, a, pal, false); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>循环位移参数 0..1（随 Phase 推进，停止时停在 0.2）。</summary>
        private float Move => Running ? Phase / (float)Math.PI * 0.5f : 0.2f;
        /// <summary>升降往复参数 0..1（运行时正弦往复，停止时停在低位）。</summary>
        private float Lift => Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) : 0.25f;
        /// <summary>链轮/滑轮：外圆 + 辐条 + 轴帽。</summary>
        private void DrawWheel(Graphics g, float cx, float cy, float r, HToolPalette pal)
        {
            if (r < 1f) return;
            g.Ell(cx, cy, r, r, pal.Metal);
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), cx - r, cy - r, r * 2f, r * 2f);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(SpinAngle);
            using (var sp = new Pen(pal.MetalDark, 1.3f))
                for (int i = 0; i < 4; i++)
                {
                    g.RotateTransform(90f);
                    g.DrawLine(sp, 0, 0, r * 0.9f, 0);
                }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.2f, r * 0.2f, pal.Accent);
        }
        /// <summary>悬挂料斗：梯形斗体，loaded 时画出骨料。</summary>
        private void DrawBucket(Graphics g, float cx, float cy, float bw, float bh,
            HToolPalette pal, bool loaded)
        {
            var bp = new[]
            {
                new PointF(cx - bw / 2f, cy - bh / 2f),
                new PointF(cx + bw / 2f, cy - bh / 2f),
                new PointF(cx + bw * 0.34f, cy + bh / 2f),
                new PointF(cx - bw * 0.34f, cy + bh / 2f)
            };
            g.Poly(bp, HToolPalettes.Tint(pal.Main, 0.1f), pal.Edge, 1f);
            g.Bolt(cx, cy - bh / 2f, 1.6f, pal.Edge);
            if (loaded)
                using (var stone = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    g.FillEllipse(stone, cx - bw * 0.22f, cy - bh * 0.62f, bw * 0.44f, bh * 0.42f);
        }
        /// <summary>吊钩（含吊颈与钩弧）。</summary>
        private void DrawHook(Graphics g, float x, float y, float s, Color c)
        {
            using (var p = new Pen(c, 2.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(p, x, y - s, x, y + s * 0.2f);
                g.DrawArc(p, x - s * 0.5f, y + s * 0.05f, s, s * 0.95f, 25f, 275f);
            }
        }
        // ----------------------------------------------------------------
        // Classic / BeltBucket / Mobile：胶带斗提机
        // ----------------------------------------------------------------
        private void DrawLift(Graphics g, RectangleF a, HToolPalette pal, bool mobile)
        {
            float cx = a.Left + a.Width * 0.46f;
            float cw = a.Width * 0.4f;
            float xL = cx - cw * 0.3f, xR = cx + cw * 0.3f;
            float r = cw * 0.3f;
            float yHead = a.Top + a.Height * 0.14f, yBoot = a.Top + a.Height * 0.82f;
            if (!mobile)
            {
                // 封闭机壳（头尾箱 + 中间直筒）
                var casing = new RectangleF(xL - r, yHead - r, cw * 0.6f + r * 2f, yBoot - yHead + r * 2f);
                using (var gp = HBarBase.RoundPath(casing, 6f))
                using (var b = new LinearGradientBrush(casing, HToolPalettes.Tint(pal.Main, 0.3f),
                    HToolPalettes.Shade(pal.Main, 0.22f), 0f))
                {
                    g.FillPath(b, gp);
                    g.DrawPath(new Pen(pal.Edge, 1.6f), gp);
                }
                // 观察窗
                var win = new RectangleF(cx - cw * 0.14f, a.Top + a.Height * 0.34f, cw * 0.28f, a.Height * 0.3f);
                using (var wb = new SolidBrush(Color.FromArgb(60, 70, 80)))
                    g.FillRectangle(wb, win);
                g.DrawRectangle(new Pen(pal.Edge, 1.1f), win.X, win.Y, win.Width, win.Height);
            }
            else
            {
                // 开式：四根框架立柱
                using (var fr = new Pen(pal.MetalDark, 2.6f))
                    foreach (float fx in new[] { xL - r, xR + r })
                    {
                        g.DrawLine(fr, fx, yHead - r, fx, yBoot + r);
                        g.DrawLine(fr, fx - 5f, yHead - r, fx + 5f, yHead - r);
                        g.DrawLine(fr, fx - 5f, yBoot + r, fx + 5f, yBoot + r);
                    }
                // 移动底盘 + 行走轮 + 拖杆
                g.Box(new RectangleF(a.Left + 4f, yBoot + r + 6f, a.Width - 8f, 7f), 2f, pal.MetalDark, pal.Edge, 1.2f);
                g.Ell(xL - r + 8f, yBoot + r + 19f, 6f, 6f, Color.FromArgb(48, 50, 56));
                g.Ell(xR + r - 8f, yBoot + r + 19f, 6f, 6f, Color.FromArgb(48, 50, 56));
                g.Line(pal.MetalDark, 2.6f, a.Left + 4f, yBoot + r + 9f, a.Left - 2f, yBoot + r + 16f);
            }
            // 链条回路
            using (var chain = new Pen(Color.FromArgb(56, 58, 64), 3f))
            {
                g.DrawLine(chain, xL, yHead, xL, yBoot);
                g.DrawLine(chain, xR, yHead, xR, yBoot);
            }
            DrawWheel(g, (xL + xR) / 2f, yHead, r * 0.98f, pal);
            DrawWheel(g, (xL + xR) / 2f, yBoot, r * 0.98f, pal);
            // 料斗沿环链提升
            int n = 5;
            float bw = cw * 0.42f, bh = a.Height * 0.045f;
            for (int i = 0; i < n; i++)
            {
                float u = (i / (float)n + Move) % 1f;
                bool rightUp = u < 0.4f;
                bool leftDown = u > 0.55f && u < 0.95f;
                if (!rightUp && !leftDown) continue;
                float x = rightUp ? xR : xL;
                float t = rightUp ? u / 0.4f : (u - 0.55f) / 0.4f;
                float y = rightUp ? yBoot - t * (yBoot - yHead) : yHead + t * (yBoot - yHead);
                DrawBucket(g, x, y, bw, bh, pal, rightUp);
            }
            // 头部驱动电机 + 卸料溜槽
            float casingR = xR + r;
            g.Motor(casingR + 2f, a.Right - 6f, yHead - r * 0.1f, r * 0.5f, pal);
            g.Poly(new[]
            {
                new PointF(casingR - 2f, yHead + r * 0.2f),
                new PointF(a.Right - 4f, yHead + r * 0.9f),
                new PointF(a.Right - 4f, yHead + r * 1.35f),
                new PointF(casingR - 6f, yHead + r * 0.6f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 尾部进料斗
            g.Poly(new[]
            {
                new PointF(a.Left + 4f, yBoot - r * 1.4f),
                new PointF(xL - r + 4f, yBoot - r * 0.2f),
                new PointF(xL - r + 4f, yBoot + r * 0.3f),
                new PointF(a.Left + 4f, yBoot - r * 0.8f)
            }, pal.Metal, pal.Edge, 1.1f);
        }
        // ----------------------------------------------------------------
        // ChainBucket：环链斗提（双排链条 + 宽料斗）
        // ----------------------------------------------------------------
        private void DrawChain(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            float xL = cx - a.Width * 0.22f, xR = cx + a.Width * 0.22f;
            float yHead = a.Top + a.Height * 0.14f, yBoot = a.Top + a.Height * 0.82f, r = 11f;
            // 框架
            using (var fr = new Pen(pal.MetalDark, 3f))
                foreach (float fx in new[] { xL - 12f, xR + 12f })
                {
                    g.DrawLine(fr, fx, yHead - r, fx, yBoot + r);
                    g.DrawLine(fr, fx - 6f, yHead - r, fx + 6f, yHead - r);
                    g.DrawLine(fr, fx - 6f, yBoot + r, fx + 6f, yBoot + r);
                }
            // 双排环链（点划线）
            using (var chain = new Pen(Color.FromArgb(56, 58, 64), 2.2f) { DashStyle = DashStyle.Dash })
                foreach (float x in new[] { xL, xR })
                {
                    g.DrawLine(chain, x - 4f, yHead, x - 4f, yBoot);
                    g.DrawLine(chain, x + 4f, yHead, x + 4f, yBoot);
                }
            DrawWheel(g, cx, yHead, r, pal);
            DrawWheel(g, cx, yBoot, r, pal);
            // 跨挂两链的宽料斗
            int n = 4;
            for (int i = 0; i < n; i++)
            {
                float u = (i / (float)n + Move) % 1f;
                bool up = u < 0.42f, down = u > 0.58f && u < 1f;
                if (!up && !down) continue;
                float t = up ? u / 0.42f : (u - 0.58f) / 0.42f;
                float y = up ? yBoot - t * (yBoot - yHead) : yHead + t * (yBoot - yHead);
                DrawBucket(g, cx, y, a.Width * 0.5f, a.Height * 0.05f, pal, up);
            }
            // 头部电机 + 卸料溜槽
            g.Motor(xR + 14f, a.Right - 4f, yHead - 2f, 8f, pal);
            g.Poly(new[]
            {
                new PointF(xR + 10f, yHead + 2f), new PointF(a.Right - 4f, yHead + 10f),
                new PointF(a.Right - 4f, yHead + 18f), new PointF(xR + 8f, yHead + 9f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 尾部进料口
            g.Poly(new[]
            {
                new PointF(a.Left + 4f, yBoot - 16f), new PointF(xL - 10f, yBoot - 4f),
                new PointF(xL - 10f, yBoot + 4f), new PointF(a.Left + 4f, yBoot - 8f)
            }, pal.Metal, pal.Edge, 1.1f);
        }
        // ----------------------------------------------------------------
        // ZType：Z 型转斗提升（下水平 → 垂直 → 上水平）
        // ----------------------------------------------------------------
        private void DrawZ(Graphics g, RectangleF a, HToolPalette pal)
        {
            float xL = a.Left + a.Width * 0.14f, xR = a.Left + a.Width * 0.66f;
            float yTop = a.Top + a.Height * 0.2f, yBot = a.Top + a.Height * 0.74f;
            float endR = a.Right - 6f;
            // 三段封闭机槽（半透明金属）
            Color shell = Color.FromArgb(220, HToolPalettes.Tint(pal.Main, 0.3f));
            g.Box(new RectangleF(xL - 12f, yBot - 8f, xR - xL + 20f, 16f), 3f, shell, pal.Edge, 1.3f);
            g.Box(new RectangleF(xR - 8f, yTop - 8f, 16f, yBot - yTop + 16f), 3f, shell, pal.Edge, 1.3f);
            g.Box(new RectangleF(xR - 8f, yTop - 8f, endR - xR + 14f, 16f), 3f, shell, pal.Edge, 1.3f);
            // 转角链轮
            DrawWheel(g, xR, yTop, 9f, pal);
            DrawWheel(g, xR, yBot, 9f, pal);
            DrawWheel(g, xL - 4f, yBot, 7f, pal);
            // 沿 Z 路径行走的悬吊转斗
            int n = 7;
            for (int i = 0; i < n; i++)
            {
                float u = (i / (float)n + Move) % 1f;
                float px, py;
                if (u < 0.3f)
                {
                    float t = u / 0.3f;
                    px = xL + (xR - xL) * t; py = yBot;
                }
                else if (u < 0.7f)
                {
                    float t = (u - 0.3f) / 0.4f;
                    px = xR; py = yBot - (yBot - yTop) * t;
                }
                else
                {
                    float t = (u - 0.7f) / 0.3f;
                    px = xR + (endR - xR) * t; py = yTop;
                }
                DrawBucket(g, px, py + 2f, 13f, 9f, pal, u < 0.66f);
            }
            // 下水平进料斗 + 上水平卸料溜槽
            g.Poly(new[]
            {
                new PointF(xL - 16f, yBot - 22f), new PointF(xL + 2f, yBot - 10f),
                new PointF(xL + 2f, yBot - 4f), new PointF(xL - 16f, yBot - 14f)
            }, pal.Metal, pal.Edge, 1.1f);
            g.Poly(new[]
            {
                new PointF(endR - 2f, yTop + 2f), new PointF(endR + 6f, yTop + 10f),
                new PointF(endR + 6f, yTop + 16f), new PointF(endR - 2f, yTop + 9f)
            }, pal.MetalDark, pal.Edge, 1.1f);
        }
        // ----------------------------------------------------------------
        // Pendulum：摇臂悬挂斗（两链间铰接，斗体始终朝上）
        // ----------------------------------------------------------------
        private void DrawPendulum(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            float xL = cx - a.Width * 0.22f, xR = cx + a.Width * 0.22f;
            float yHead = a.Top + a.Height * 0.12f, yBoot = a.Top + a.Height * 0.84f, r = 12f;
            // 全封闭机壳
            var casing = new RectangleF(xL - r, yHead - r, (xR - xL) + r * 2f, yBoot - yHead + r * 2f);
            g.Box(casing, 5f, HToolPalettes.Tint(pal.Main, 0.28f), pal.Edge, 1.5f);
            // 两排观察窗
            foreach (float x in new[] { xL - 2f, xR - 2f })
            {
                var win = new RectangleF(x - 7f, yHead + 26f, 18f, yBoot - yHead - 52f);
                using (var wb = new SolidBrush(Color.FromArgb(60, 70, 80)))
                    g.FillRectangle(wb, win);
                g.DrawRectangle(new Pen(pal.Edge, 1f), win.X, win.Y, win.Width, win.Height);
            }
            using (var chain = new Pen(Color.FromArgb(56, 58, 64), 2.4f))
            {
                g.DrawLine(chain, xL, yHead, xL, yBoot);
                g.DrawLine(chain, xR, yHead, xR, yBoot);
            }
            DrawWheel(g, cx, yHead, r, pal);
            DrawWheel(g, cx, yBoot, r, pal);
            // 两侧各 3 个铰接斗（始终朝上）
            for (int i = 0; i < 6; i++)
            {
                float u = (i / 6f + Move) % 1f;
                bool up = u < 0.42f, down = u > 0.58f && u < 1f;
                if (!up && !down) continue;
                float t = up ? u / 0.42f : (u - 0.58f) / 0.42f;
                float y = up ? yBoot - t * (yBoot - yHead) : yHead + t * (yBoot - yHead);
                float x = up ? xR : xL;
                DrawBucket(g, x, y, a.Width * 0.3f, 12f, pal, up);
            }
            // 头部卸料口 + 驱动
            g.Motor(casing.Right + 1f, a.Right - 4f, yHead - 4f, 8f, pal);
            g.Box(new RectangleF(casing.Left - 4f, yBoot + r - 2f, 12f, 8f), 2f, pal.MetalDark, pal.Edge, 1f);
        }
        // ----------------------------------------------------------------
        // VibroVertical：垂直振动螺旋提升机
        // ----------------------------------------------------------------
        private void DrawVibro(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            float tubeW = a.Width * 0.34f;
            float yTop = a.Top + a.Height * 0.16f, yBot = a.Top + a.Height * 0.78f;
            // 底座 + 隔振弹簧
            g.Box(new RectangleF(cx - a.Width * 0.26f, yBot + 16f, a.Width * 0.52f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            foreach (float lx in new[] { cx - a.Width * 0.2f, cx + a.Width * 0.2f })
            {
                using (var sp = new Pen(pal.Edge, 1.5f))
                    for (int k = 0; k < 4; k++)
                        g.DrawArc(sp, lx - 5f, yBot + 14f - k * 5f, 10f, 7f, 0f, 180f);
            }
            // 振动电机（底座上，带偏心块）
            float my = yBot + 10f;
            g.Motor(cx - 22f, cx + 22f, my, 9f, pal);
            foreach (float wx in new[] { cx - 24f, cx + 24f })
            {
                var st = g.Save();
                g.TranslateTransform(wx, my);
                g.RotateTransform(SpinAngle);
                using (var wb = new SolidBrush(Color.FromArgb(96, 100, 106)))
                    g.FillPie(wb, -7f, -5f, 14f, 10f, 0f, 180f);
                g.Restore(st);
            }
            // 垂直管体
            var tube = new RectangleF(cx - tubeW / 2f, yTop, tubeW, yBot - yTop);
            using (var b = new LinearGradientBrush(tube, HToolPalettes.Tint(pal.Main, 0.35f),
                HToolPalettes.Shade(pal.Main, 0.15f), 0f))
                g.FillRectangle(b, tube);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), tube.X, tube.Y, tube.Width, tube.Height);
            // 螺旋槽道（两条相位交错的正弦线绕管上升）
            using (var sp = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 2f))
                for (int k = 0; k < 2; k++)
                {
                    var pts = new PointF[48];
                    for (int i = 0; i < pts.Length; i++)
                    {
                        float t = i / 47f;
                        double ang = t * Math.PI * 5.0 + k * Math.PI;
                        pts[i] = new PointF(cx + (float)Math.Sin(ang) * tubeW * 0.46f,
                            yBot - 4f - t * (yBot - yTop - 8f));
                    }
                    g.DrawLines(sp, pts);
                }
            // 沿螺旋上升的物料点
            using (var mat = new SolidBrush(Color.FromArgb(149, 112, 72)))
                for (int i = 0; i < 6; i++)
                {
                    float t = (i / 6f + Move) % 1f;
                    double ang = t * Math.PI * 5.0;
                    float px = cx + (float)Math.Sin(ang) * tubeW * 0.4f;
                    float py = yBot - 4f - t * (yBot - yTop - 8f);
                    g.FillEllipse(mat, px - 2.5f, py - 2.5f, 5f, 5f);
                }
            // 顶部卸料槽
            g.Poly(new[]
            {
                new PointF(cx + tubeW / 2f - 2f, yTop + 2f), new PointF(a.Right - 4f, yTop - 4f),
                new PointF(a.Right - 4f, yTop + 3f), new PointF(cx + tubeW / 2f - 2f, yTop + 9f)
            }, pal.MetalDark, pal.Edge, 1.1f);
        }
        // ----------------------------------------------------------------
        // Pneumatic：气力提升泵（仓式泵 + 垂直输送管）
        // ----------------------------------------------------------------
        private void DrawPneumatic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.46f;
            float yTop = a.Top + a.Height * 0.1f, yBot = a.Top + a.Height * 0.7f;
            // 立式压力容器（椭圆封头）
            float vx = cx - a.Width * 0.24f, vw = a.Width * 0.48f, r = vw / 2f;
            var body = new RectangleF(vx, yBot - a.Height * 0.34f, vw, a.Height * 0.3f);
            using (var path = new GraphicsPath())
            {
                path.AddArc(vx, body.Top - r * 0.6f, vw, r * 1.2f, 180f, 180f);
                path.AddRectangle(body);
                path.AddArc(vx, body.Bottom - r * 0.6f, vw, r * 1.2f, 0f, 180f);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(body, HToolPalettes.Tint(pal.Main, 0.32f),
                    HToolPalettes.Shade(pal.Main, 0.2f), 0f))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(pal.Edge, 1.5f), path);
            }
            // 支腿
            foreach (float lx in new[] { vx + 5f, vx + vw - 5f })
                g.Line(pal.MetalDark, 3.4f, lx, body.Bottom, lx, a.Bottom - 8f);
            // 压力表
            g.Ell(vx + 8f, body.Top + 8f, 5f, 5f, Color.White);
            g.DrawEllipse(new Pen(pal.Edge, 1f), vx + 3f, body.Top + 3f, 10f, 10f);
            g.Line(pal.Edge, 1.1f, vx + 8f, body.Top + 8f, vx + 11f, body.Top + 5f);
            // 进料阀 + 气管
            g.Box(new RectangleF(vx + vw * 0.3f, body.Top - 14f, vw * 0.4f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            using (var hose = new Pen(Color.FromArgb(58, 60, 66), 3.4f))
                g.DrawBezier(hose, vx - 2f, body.Top + 14f, vx - 18f, body.Top - 6f,
                    a.Left + 8f, yBot - 2f, a.Left + 8f, a.Bottom - 8f);
            // 垂直输送管 + 顶部弯管卸料
            float px = cx + vw * 0.28f, pw = 12f;
            g.Box(new RectangleF(px - pw / 2f, yTop, pw, body.Top - yTop + 6f), 2f,
                HToolPalettes.Tint(pal.Metal, 0.1f), pal.Edge, 1.2f);
            g.Box(new RectangleF(px - pw / 2f, yTop - 4f, a.Right - 4f - (px - pw / 2f), 10f), 2f,
                HToolPalettes.Tint(pal.Metal, 0.1f), pal.Edge, 1.2f);
            // 管内上升料气
            if (Running)
                using (var dot = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (i / 5f + Move) % 1f;
                        g.FillEllipse(dot, px - 2f + (t > 0.85f ? t * 30f : 0f),
                            body.Top - t * (body.Top - yTop), 4f, 4f);
                    }
        }
        // ----------------------------------------------------------------
        // ScrewVertical：立式螺旋提升机
        // ----------------------------------------------------------------
        private void DrawScrewV(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            float tw = a.Width * 0.36f;
            float yTop = a.Top + a.Height * 0.16f, yBot = a.Top + a.Height * 0.84f;
            // 顶驱电机
            g.Motor(cx - tw, cx + tw, yTop - 10f, 9f, pal);
            // 圆筒槽体
            var tube = new RectangleF(cx - tw / 2f, yTop, tw, yBot - yTop);
            using (var b = new LinearGradientBrush(tube, HToolPalettes.Tint(pal.Main, 0.35f),
                HToolPalettes.Shade(pal.Main, 0.18f), 0f))
                g.FillRectangle(b, tube);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), tube.X, tube.Y, tube.Width, tube.Height);
            // 心轴 + 螺旋叶片（交错半圆）
            g.Line(Color.FromArgb(70, 74, 80), 3f, cx, yTop, cx, yBot);
            int flights = 8;
            using (var fp = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 2.2f))
                for (int i = 0; i < flights; i++)
                {
                    float y = yBot - (i + 0.5f) * (yBot - yTop) / flights;
                    g.DrawArc(fp, cx - tw * 0.42f, y - 8f, tw * 0.84f, 16f, i % 2 == 0 ? 0f : 180f, 180f);
                }
            // 底部进料溜槽 + 顶部卸料口
            g.Poly(new[]
            {
                new PointF(a.Left + 4f, yBot - 18f), new PointF(tube.Left + 2f, yBot - 6f),
                new PointF(tube.Left + 2f, yBot + 2f), new PointF(a.Left + 4f, yBot - 10f)
            }, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(tube.Right - 2f, yTop + 10f, 12f, 9f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 支腿
            foreach (float lx in new[] { tube.Left + 3f, tube.Right - 3f })
                g.Line(pal.MetalDark, 3f, lx, yBot, lx, a.Bottom - 6f);
        }
        // ----------------------------------------------------------------
        // MineHoist / Headframe：井架提升（false 矿用 A 架，true 格栅井塔）
        // ----------------------------------------------------------------
        private void DrawFrameHoist(Graphics g, RectangleF a, HToolPalette pal, bool lattice)
        {
            float ground = a.Bottom - 6f;
            g.Line(pal.Edge, 2.4f, a.Left + 4f, ground, a.Right - 4f, ground);
            float cx = a.Left + a.Width * (lattice ? 0.5f : 0.44f);
            float topY = a.Top + 10f;
            float legTop = lattice ? a.Width * 0.12f : a.Width * 0.08f;
            float legBot = lattice ? a.Width * 0.34f : a.Width * 0.3f;
            Color steel = pal.MetalDark;
            // 井架双腿（lattice 为梯形桁架并加 X 横撑）
            g.Line(steel, 3.4f, cx - legTop, topY, cx - legBot, ground);
            g.Line(steel, 3.4f, cx + legTop, topY, cx + legBot, ground);
            int levels = lattice ? 4 : 2;
            for (int k = 1; k <= levels; k++)
            {
                float t = k / (levels + 1f);
                float ly = topY + (ground - topY) * t;
                float lx = legTop + (legBot - legTop) * t;
                g.Line(pal.Edge, 1.8f, cx - lx, ly, cx + lx, ly);
                float ly2 = topY + (ground - topY) * (k - 1) / (levels + 1f);
                float lx2 = legTop + (legBot - legTop) * ((k - 1) / (levels + 1f));
                g.Line(pal.Edge, 1.4f, cx - lx, ly, cx + lx2, ly2);
                g.Line(pal.Edge, 1.4f, cx + lx, ly, cx - lx2, ly2);
            }
            // 天轮
            DrawWheel(g, cx, topY, 9f, pal);
            // 罐笼/箕斗沿架内升降
            float cageY = topY + 18f + Lift * (ground - topY - 46f);
            float cw = lattice ? a.Width * 0.24f : a.Width * 0.2f;
            g.Box(new RectangleF(cx - cw / 2f, cageY, cw, 26f), 2f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.3f);
            using (var bp = new Pen(pal.Edge, 1f))
            {
                g.DrawLine(bp, cx, cageY, cx, cageY + 26f);
                g.DrawLine(bp, cx - cw / 2f, cageY + 13f, cx + cw / 2f, cageY + 13f);
            }
            // 提升钢丝绳
            g.Line(Color.FromArgb(56, 58, 64), 1.8f, cx, cageY, cx, topY);
            // 地面卷扬机房
            float hx = a.Right - a.Width * 0.3f, hw = a.Width * 0.28f, hh = 26f;
            g.Box(new RectangleF(hx, ground - hh, hw, hh), 2f,
                HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.2f);
            g.GableRoof(hx, ground - hh - 9f, hw, ground - hh, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge);
            // 机房内卷筒
            DrawWheel(g, hx + hw / 2f, ground - hh / 2f, 7f, pal);
            g.Line(Color.FromArgb(56, 58, 64), 1.6f, hx + hw / 2f, ground - hh / 2f - 7f, cx, topY);
        }
        // ----------------------------------------------------------------
        // ConstructionHoist：施工升降机（齿条导轨 + 吊笼）
        // ----------------------------------------------------------------
        private void DrawConstruction(Graphics g, RectangleF a, HToolPalette pal)
        {
            float mx = a.Left + a.Width * 0.32f;
            float top = a.Top + 6f, ground = a.Bottom - 8f;
            float mw = a.Width * 0.2f;
            // 导轨架（三弦杆桁架）
            using (var tr = new Pen(Color.FromArgb(120, 124, 130), 1.6f))
            {
                foreach (float ox in new[] { -mw / 2f, 0f, mw / 2f })
                    g.DrawLine(tr, mx + ox, top, mx + ox, ground);
                for (float y = top; y < ground - 8f; y += 14f)
                {
                    g.DrawLine(tr, mx - mw / 2f, y, mx + mw / 2f, y + 14f);
                    g.DrawLine(tr, mx + mw / 2f, y, mx - mw / 2f, y + 14f);
                }
            }
            // 齿条
            g.Line(Color.FromArgb(56, 58, 64), 2.4f, mx + mw / 2f + 3f, top, mx + mw / 2f + 3f, ground);
            // 吊笼（沿导轨升降）
            float cw = a.Width * 0.42f, ch = a.Height * 0.22f;
            float cy = ground - ch - 6f - Lift * (ground - top - ch - 14f);
            g.Box(new RectangleF(mx + mw / 2f + 5f, cy, cw, ch), 3f,
                HToolPalettes.Tint(pal.Main, 0.25f), pal.Edge, 1.4f);
            // 笼门网格
            using (var mp = new Pen(pal.Edge, 1f))
                for (int i = 1; i < 4; i++)
                    g.DrawLine(mp, mx + mw / 2f + 5f + cw * i / 4f, cy + 3f,
                        mx + mw / 2f + 5f + cw * i / 4f, cy + ch - 3f);
            // 笼顶驱动
            g.Motor(mx + mw / 2f + 8f, mx + mw / 2f + 8f + cw - 8f, cy - 7f, 6f, pal);
            // 地面围栏 + 底座
            g.Box(new RectangleF(a.Left + 4f, ground - 4f, a.Width - 8f, 6f), 2f, pal.MetalDark, pal.Edge, 1.2f);
            using (var fp = new Pen(pal.Edge, 1.3f))
                g.DrawLine(fp, mx + mw / 2f + 5f, ground - 22f, a.Right - 4f, ground - 22f);
        }
        // ----------------------------------------------------------------
        // ChainHoist / AirHoist：轨道葫芦 + 吊钩（false 电动，true 气动）
        // ----------------------------------------------------------------
        private void DrawHookHoist(Graphics g, RectangleF a, HToolPalette pal, bool air)
        {
            float beamY = a.Top + 12f;
            // 工字钢梁
            g.Box(new RectangleF(a.Left + 4f, beamY - 4f, a.Width - 8f, 8f), 1f,
                pal.MetalDark, pal.Edge, 1.2f);
            g.Line(pal.Edge, 1.4f, a.Left + 8f, beamY - 9f, a.Right - 8f, beamY - 9f);
            float hx = a.Left + a.Width * 0.56f;
            // 行走小车
            g.Box(new RectangleF(hx - 12f, beamY + 4f, 24f, 8f), 2f, pal.Metal, pal.Edge, 1.1f);
            // 电动葫芦筒体 / 气缸体
            float bodyTop = beamY + 14f, bodyH = 26f;
            if (air)
            {
                g.Box(new RectangleF(hx - 9f, bodyTop, 18f, bodyH), 6f,
                    HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
                g.Box(new RectangleF(hx - 7f, bodyTop - 5f, 14f, 7f), 2f, pal.MetalDark, pal.Edge, 1.1f);
                // 气管
                using (var hose = new Pen(Color.FromArgb(58, 60, 66), 3f))
                    g.DrawBezier(hose, hx + 9f, bodyTop + 4f, hx + 26f, bodyTop - 6f,
                        hx + 24f, bodyTop + 24f, a.Right - 4f, bodyTop + 18f);
            }
            else
            {
                g.Motor(hx - 18f, hx + 14f, bodyTop + bodyH / 2f, bodyH / 2f, pal);
                // 按钮手柄线
                g.Line(Color.FromArgb(58, 60, 66), 1.6f, hx + 14f, bodyTop + bodyH / 2f,
                    a.Right - 6f, bodyTop + bodyH + 14f);
                g.Box(new RectangleF(a.Right - 10f, bodyTop + bodyH + 12f, 7f, 12f), 2f,
                    pal.MetalDark, pal.Edge, 1f);
            }
            // 钢丝绳/活塞杆 + 吊钩 + 吊重（随 Lift 升降）
            float hookY = bodyTop + bodyH + 10f + (1f - Lift) * a.Height * 0.4f;
            g.Line(Color.FromArgb(56, 58, 64), air ? 3f : 1.8f, hx, bodyTop + bodyH, hx, hookY);
            DrawHook(g, hx, hookY, 9f, Color.FromArgb(70, 74, 80));
            // 吊重货箱
            float lw = a.Width * 0.34f;
            g.Box(new RectangleF(hx - lw / 2f, hookY + 12f, lw, a.Height * 0.16f), 2f,
                HToolPalettes.Tint(pal.Main, 0.18f), pal.Edge, 1.3f);
        }
        // ----------------------------------------------------------------
        // LeverHoist：手拉葫芦
        // ----------------------------------------------------------------
        private void DrawLever(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            float beamY = a.Top + 8f;
            g.Box(new RectangleF(a.Left + 6f, beamY - 3f, a.Width - 12f, 6f), 1f,
                pal.MetalDark, pal.Edge, 1.1f);
            // 上吊钩
            DrawHook(g, cx, beamY + 12f, 8f, Color.FromArgb(70, 74, 80));
            // 葫芦机体
            float by = beamY + 26f;
            g.Box(new RectangleF(cx - 16f, by, 32f, 24f), 6f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.4f);
            g.Ell(cx, by + 12f, 5f, 5f, pal.Accent);
            // 手拉链条环（闭合椭圆环）
            using (var ch = new Pen(Color.FromArgb(56, 58, 64), 1.8f))
            {
                g.DrawLine(ch, cx + 11f, by + 4f, cx + 11f, by + 46f);
                g.DrawLine(ch, cx + 17f, by + 4f, cx + 17f, by + 46f);
                g.DrawArc(ch, cx + 11f, by + 40f, 6f, 12f, 0f, 180f);
            }
            // 起重链 + 下吊钩（随 Lift 上升）
            float hookY = by + 56f + (1f - Lift) * a.Height * 0.3f;
            g.Line(Color.FromArgb(56, 58, 64), 2f, cx, by + 22f, cx, hookY);
            DrawHook(g, cx, hookY, 8f, Color.FromArgb(70, 74, 80));
            // 吊重
            g.Box(new RectangleF(cx - 18f, hookY + 10f, 36f, a.Height * 0.13f), 2f,
                HToolPalettes.Tint(pal.Main, 0.18f), pal.Edge, 1.2f);
        }
        // ----------------------------------------------------------------
        // SkipHoist：爬斗提升机（倾斜导轨 + 翻斗）
        // ----------------------------------------------------------------
        private void DrawSkip(Graphics g, RectangleF a, HToolPalette pal)
        {
            float x1 = a.Left + a.Width * 0.12f, y1 = a.Bottom - a.Height * 0.14f;
            float x2 = a.Right - a.Width * 0.12f, y2 = a.Top + a.Height * 0.14f;
            // 双排导轨 + 支架
            using (var rail = new Pen(pal.MetalDark, 3f))
                foreach (float n in new[] { -4f, 4f })
                {
                    double ang = Math.Atan2(y2 - y1, x2 - x1) + Math.PI / 2.0;
                    float ox = (float)Math.Cos(ang) * n, oy = (float)Math.Sin(ang) * n;
                    g.DrawLine(rail, x1 + ox, y1 + oy, x2 + ox, y2 + oy);
                }
            foreach (float ft in new[] { 0.25f, 0.6f })
            {
                float px = x1 + (x2 - x1) * ft, py = y1 + (y2 - y1) * ft;
                g.Line(pal.Edge, 2.4f, px, py, px, a.Bottom - 6f);
            }
            // 顶滑轮 + 卷扬绳
            DrawWheel(g, x2, y2 - 10f, 8f, pal);
            // 底部进料斗
            g.Poly(new[]
            {
                new PointF(a.Left + 2f, y1 - 22f), new PointF(x1 + 8f, y1 - 8f),
                new PointF(x1 + 8f, y1), new PointF(a.Left + 2f, y1 - 12f)
            }, pal.Metal, pal.Edge, 1.1f);
            // 顶部卸料溜槽
            g.Poly(new[]
            {
                new PointF(x2 - 6f, y2 - 16f), new PointF(a.Right - 2f, y2 - 26f),
                new PointF(a.Right - 2f, y2 - 18f), new PointF(x2 - 6f, y2 - 8f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 爬斗（沿轨运行，近顶时前倾）
            float t = Lift;
            float sx = x1 + (x2 - x1) * t, sy = y1 + (y2 - y1) * t;
            var st = g.Save();
            g.TranslateTransform(sx, sy);
            g.RotateTransform((float)(Math.Atan2(y2 - y1, x2 - x1) * 180.0 / Math.PI) + (t > 0.8f ? (t - 0.8f) * 120f : 0f));
            g.Poly(new[]
            {
                new PointF(-16f, -8f), new PointF(16f, -8f),
                new PointF(12f, 10f), new PointF(-12f, 10f)
            }, HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            using (var stone = new SolidBrush(Color.FromArgb(149, 112, 72)))
                g.FillEllipse(stone, -8f, -11f, 16f, 7f);
            g.Ell(-12f, 11f, 3f, 3f, Color.FromArgb(48, 50, 56));
            g.Ell(12f, 11f, 3f, 3f, Color.FromArgb(48, 50, 56));
            g.Restore(st);
            // 卷扬绳
            g.Line(Color.FromArgb(56, 58, 64), 1.8f, sx, sy, x2, y2 - 10f);
        }
        // ----------------------------------------------------------------
        // Winch：电动卷扬机
        // ----------------------------------------------------------------
        private void DrawWinch(Graphics g, RectangleF a, HToolPalette pal)
        {
            float ground = a.Bottom - 8f;
            // 滑橇底座
            g.Box(new RectangleF(a.Left + 6f, ground - 8f, a.Width - 12f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            float cy = a.Top + a.Height * 0.52f;
            // 电机 + 减速箱 + 大卷筒
            g.Motor(a.Left + 8f, a.Left + a.Width * 0.34f, cy, a.Height * 0.09f, pal);
            g.Box(new RectangleF(a.Left + a.Width * 0.34f, cy - 10f, a.Width * 0.12f, 20f), 2f,
                pal.Metal, pal.Edge, 1.2f);
            float dx1 = a.Left + a.Width * 0.48f, dx2 = a.Right - 8f;
            var drum = new RectangleF(dx1, cy - 16f, dx2 - dx1, 32f);
            g.FillCylH(drum, HToolPalettes.Shade(pal.Main, 0.3f), HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), drum.X, drum.Y, drum.Width, drum.Height);
            // 绳槽
            using (var gp = new Pen(pal.Edge, 1f))
                for (float x = drum.X + 4f; x < drum.Right; x += 6f)
                    g.DrawLine(gp, x, drum.Y + 2f, x, drum.Bottom - 2f);
            // 卷筒辐条（旋转）
            float dcx = (dx1 + dx2) / 2f;
            var st = g.Save();
            g.TranslateTransform(dcx, cy);
            g.RotateTransform(SpinAngle);
            using (var sp = new Pen(pal.MetalDark, 1.6f))
                for (int i = 0; i < 3; i++)
                {
                    g.RotateTransform(120f);
                    g.DrawLine(sp, 0, 0, 13f, 0);
                }
            g.Restore(st);
            // 出绳（向上）
            using (var rope = new Pen(Color.FromArgb(56, 58, 64), 2f) { DashStyle = DashStyle.Dash })
                g.DrawLine(rope, dcx, cy - 16f, dcx, a.Top + 4f);
            // 控制箱
            g.Box(new RectangleF(a.Left + a.Width * 0.12f, cy - 38f, a.Width * 0.2f, 20f), 2f,
                HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.2f);
        }
        // ----------------------------------------------------------------
        // StackerCrane：巷道堆垛机
        // ----------------------------------------------------------------
        private void DrawStacker(Graphics g, RectangleF a, HToolPalette pal)
        {
            float top = a.Top + 8f, bot = a.Bottom - 8f;
            // 上下地轨/天轨
            foreach (float y in new[] { top, bot })
            {
                g.Box(new RectangleF(a.Left + 2f, y - 3f, a.Width - 4f, 6f), 1f,
                    pal.MetalDark, pal.Edge, 1.1f);
            }
            // 双立柱
            float m1 = a.Left + a.Width * 0.34f, m2 = a.Left + a.Width * 0.56f;
            foreach (float mx in new[] { m1, m2 })
                g.Line(pal.MetalDark, 3.4f, mx, top, mx, bot);
            g.Line(pal.Edge, 2f, m1, top + 8f, m2, top + 8f);
            // 载货台（沿立柱升降）
            float ch = a.Height * 0.16f;
            float cy = bot - ch - 6f - Lift * (bot - top - ch - 20f);
            g.Box(new RectangleF(m1 - 8f, cy, m2 - m1 + 16f, ch), 2f,
                HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.3f);
            // 货叉 + 货物
            g.Line(pal.MetalDark, 3f, m2, cy + ch * 0.5f, a.Right - 6f, cy + ch * 0.5f);
            g.Box(new RectangleF(a.Right - a.Width * 0.26f, cy + ch * 0.18f, a.Width * 0.22f, ch * 0.64f),
                2f, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.2f);
            // 顶部提升电机
            g.Motor(m1 - 6f, m2 + 6f, top + 16f, 7f, pal);
        }
        // ----------------------------------------------------------------
        // ScissorLift：剪叉式升降台
        // ----------------------------------------------------------------
        private void DrawScissor(Graphics g, RectangleF a, HToolPalette pal)
        {
            float ground = a.Bottom - 8f;
            float baseY = ground - 6f;
            g.Box(new RectangleF(a.Left + 6f, baseY, a.Width - 12f, 6f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            // 平台高度随 Lift
            float low = a.Height * 0.24f, high = a.Height * 0.72f;
            float travel = low + (high - low) * Lift;
            float platY = baseY - travel;
            float x1 = a.Left + a.Width * 0.12f, x2 = a.Right - a.Width * 0.12f;
            // 两级剪臂（各级端点由几何关系算出）
            float midY1 = baseY - travel * 0.5f;
            using (var arm = new Pen(pal.MetalDark, 3.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                // 下 X：左下固定 → 右上（平台滚轮），左上（底滚轮）→ 右下固定
                g.DrawLine(arm, x1, baseY, x2, midY1);
                g.DrawLine(arm, x2, baseY, x1, midY1);
                // 上 X
                g.DrawLine(arm, x1, midY1, x2, platY);
                g.DrawLine(arm, x2, midY1, x1, platY);
            }
            g.Ell(x1, midY1, 2.6f, 2.6f, pal.Edge);
            g.Ell(x2, midY1, 2.6f, 2.6f, pal.Edge);
            // 液压缸
            g.Line(Color.FromArgb(70, 74, 80), 4f, x1 + 6f, baseY - 2f,
                x1 + (x2 - x1) * 0.42f, midY1 - 6f);
            // 平台 + 护栏
            g.Box(new RectangleF(x1 - 8f, platY - 6f, x2 - x1 + 16f, 7f), 2f,
                HToolPalettes.Tint(pal.Metal, 0.1f), pal.Edge, 1.3f);
            using (var rail = new Pen(pal.Edge, 1.3f))
            {
                foreach (float rx in new[] { x1 - 4f, (x1 + x2) / 2f, x2 + 4f })
                    g.DrawLine(rail, rx, platY - 6f, rx, platY - 18f);
                g.DrawLine(rail, x1 - 4f, platY - 18f, x2 + 4f, platY - 18f);
            }
        }
        // ----------------------------------------------------------------
        // MastLift：桅柱式高空作业平台
        // ----------------------------------------------------------------
        private void DrawMast(Graphics g, RectangleF a, HToolPalette pal)
        {
            float ground = a.Bottom - 8f;
            float mx = a.Left + a.Width * 0.42f, top = a.Top + 8f;
            // 底盘 + 四轮
            g.Box(new RectangleF(a.Left + 6f, ground - 8f, a.Width - 12f, 8f), 3f,
                pal.MetalDark, pal.Edge, 1.2f);
            foreach (float wx in new[] { a.Left + 14f, a.Right - 14f })
                g.Ell(wx, ground, 5f, 5f, Color.FromArgb(48, 50, 56));
            // 桅柱（套筒，两级）
            float w2 = a.Width * 0.16f, w1 = a.Width * 0.1f;
            g.Box(new RectangleF(mx - w2 / 2f, ground - 8f - a.Height * 0.4f, w2, a.Height * 0.4f), 2f,
                HToolPalettes.Shade(pal.Metal, 0.15f), pal.Edge, 1.2f);
            float ext = a.Height * (0.26f + 0.34f * Lift);
            g.Box(new RectangleF(mx - w1 / 2f, ground - 8f - a.Height * 0.4f - ext, w1, ext), 2f,
                HToolPalettes.Tint(pal.Metal, 0.12f), pal.Edge, 1.2f);
            // 作业平台（带护栏的 U 斗）
            float by = ground - 8f - a.Height * 0.4f - ext;
            float bx1 = mx + w1 / 2f + 2f, bw = a.Width * 0.4f;
            g.Box(new RectangleF(bx1, by - 6f, bw, 8f), 2f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.3f);
            using (var rail = new Pen(pal.Edge, 1.3f))
            {
                foreach (float rx in new[] { bx1 + 2f, bx1 + bw / 2f, bx1 + bw - 2f })
                    g.DrawLine(rail, rx, by - 6f, rx, by - 20f);
                g.DrawLine(rail, bx1 + 2f, by - 20f, bx1 + bw - 2f, by - 20f);
            }
        }
        // ----------------------------------------------------------------
        // InclineScrew：倾斜螺旋提升机
        // ----------------------------------------------------------------
        private void DrawInclineScrew(Graphics g, RectangleF a, HToolPalette pal)
        {
            float x1 = a.Left + a.Width * 0.14f, y1 = a.Bottom - a.Height * 0.16f;
            float x2 = a.Right - a.Width * 0.12f, y2 = a.Top + a.Height * 0.16f;
            double ang = Math.Atan2(y2 - y1, x2 - x1);
            float nx = -(float)Math.Sin(ang) * 9f, ny = (float)Math.Cos(ang) * 9f;
            // 槽管（平行四边形管体）
            g.Poly(new[]
            {
                new PointF(x1, y1 + ny), new PointF(x2, y2 + ny),
                new PointF(x2, y2 - ny), new PointF(x1, y1 - ny)
            }, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.4f);
            // 心轴 + 螺旋叶片节
            g.Line(Color.FromArgb(70, 74, 80), 2.6f, x1, y1, x2, y2);
            using (var fp = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.8f))
                for (int i = 1; i < 10; i++)
                {
                    float t = i / 10f;
                    float px = x1 + (x2 - x1) * t, py = y1 + (y2 - y1) * t;
                    float sw = i % 2 == 0 ? 1f : -1f;
                    g.DrawLine(fp, px - nx * 0.8f * sw, py - ny * 0.8f * sw,
                        px + nx * 0.8f * sw, py + ny * 0.8f * sw);
                }
            // 上行物料点
            using (var mat = new SolidBrush(Color.FromArgb(149, 112, 72)))
                for (int i = 0; i < 6; i++)
                {
                    float t = (i / 6f + Move) % 1f;
                    g.FillEllipse(mat, x1 + (x2 - x1) * t - 2.5f, y1 + (y2 - y1) * t - 2.5f, 5f, 5f);
                }
            // 底进料斗 + 顶驱电机 + 顶卸料口
            g.Poly(new[]
            {
                new PointF(a.Left + 2f, y1 - 18f), new PointF(x1 - 2f, y1 - 6f),
                new PointF(x1 - 2f, y1 + 2f), new PointF(a.Left + 2f, y1 - 8f)
            }, pal.Metal, pal.Edge, 1.1f);
            var st = g.Save();
            g.TranslateTransform(x2, y2);
            g.RotateTransform((float)(ang * 180.0 / Math.PI));
            g.Motor(-26f, 4f, 0f, 8f, pal);
            g.Restore(st);
            g.Poly(new[]
            {
                new PointF(x2 - 4f, y2 + ny - 2f), new PointF(a.Right - 2f, y2 + 12f),
                new PointF(a.Right - 2f, y2 + 19f), new PointF(x2 - 4f, y2 + ny + 6f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 支腿
            g.Line(pal.MetalDark, 3f, x1 + (x2 - x1) * 0.25f, y1 + (y2 - y1) * 0.25f,
                x1 + (x2 - x1) * 0.25f - 6f, a.Bottom - 6f);
            g.Line(pal.MetalDark, 3f, x1 + (x2 - x1) * 0.7f, y1 + (y2 - y1) * 0.7f,
                x1 + (x2 - x1) * 0.7f + 6f, a.Bottom - 6f);
        }
        // ----------------------------------------------------------------
        // VacuumLift：真空气力吸料提升
        // ----------------------------------------------------------------
        private void DrawVacuum(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.52f;
            float ground = a.Bottom - 6f;
            // 受料罐（立式 + 椭圆封盖）
            float vw = a.Width * 0.46f, vh = a.Height * 0.26f;
            var v = new RectangleF(cx - vw / 2f, a.Top + a.Height * 0.2f, vw, vh);
            using (var path = new GraphicsPath())
            {
                path.AddArc(v.X, v.Y - 8f, vw, 16f, 180f, 180f);
                path.AddRectangle(v);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(v, HToolPalettes.Tint(pal.Main, 0.32f),
                    HToolPalettes.Shade(pal.Main, 0.18f), 0f))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(pal.Edge, 1.4f), path);
            }
            // 罐内料位
            using (var mat = new SolidBrush(Color.FromArgb(149, 112, 72)))
                g.FillRectangle(mat, v.X + 2f, v.Bottom - vh * 0.35f, vw - 4f, vh * 0.35f);
            // 顶置真空泵 + 滤芯 + 排气
            g.Motor(cx - vw * 0.42f, cx + vw * 0.1f, v.Top - 14f, 8f, pal);
            g.Box(new RectangleF(cx + vw * 0.14f, v.Top - 10f, 9f, 14f), 2f,
                pal.Metal, pal.Edge, 1.1f);
            // 罐侧卸料阀 + 支腿
            g.Box(new RectangleF(v.Right - 2f, v.Bottom - 14f, 10f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            foreach (float lx in new[] { v.X + 4f, v.Right - 4f })
                g.Line(pal.MetalDark, 3f, lx, v.Bottom, lx, ground);
            // 地面料堆
            g.Poly(new[]
            {
                new PointF(a.Left + 2f, ground), new PointF(a.Left + a.Width * 0.3f, ground - 20f),
                new PointF(a.Left + a.Width * 0.46f, ground)
            }, Color.FromArgb(149, 112, 72), pal.Edge, 1.1f);
            // 柔性吸料管（贝塞尔曲线插入料堆）
            float sx = a.Left + a.Width * 0.28f, sy = ground - 14f;
            using (var hose = new Pen(Color.FromArgb(58, 60, 66), 5f))
                g.DrawBezier(hose, v.X + 4f, v.Top + 6f, v.X - 18f, v.Top + 30f,
                    sx + 14f, sy - 26f, sx, sy);
            // 吸嘴
            g.Poly(new[]
            {
                new PointF(sx - 5f, sy), new PointF(sx + 5f, sy),
                new PointF(sx + 2.5f, sy + 10f), new PointF(sx - 2.5f, sy + 10f)
            }, pal.MetalDark, pal.Edge, 1f);
            // 管内上升料粒
            if (Running)
                using (var dot = new SolidBrush(Color.FromArgb(149, 112, 72)))
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (i / 5f + Move) % 1f;
                        float px = sx + (v.X + 4f - sx) * t + (float)Math.Sin(t * 9f) * 3f;
                        float py = sy + (v.Top + 6f - sy) * t;
                        g.FillEllipse(dot, px - 2.5f, py - 2.5f, 5f, 5f);
                    }
        }
    }
}