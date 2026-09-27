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
    /// 磨床样式：Classic 为往复工作台平面磨床，其余 21 种覆盖外圆/无心/内圆/齿轮/
    /// 坐标/曲轴/成形磨，台式/立式砂带/圆盘/布轮抛光、角磨/直磨/切割、珩磨、
    /// 钢轨与地面研磨等设备。
    /// </summary>
    public enum HGrinderStyle
    {
        Classic = 0,        // 平面磨床
        Cylindrical = 1,    // 外圆磨床
        Centerless = 2,     // 无心磨床
        Bench = 3,          // 台式砂轮机
        Belt = 4,           // 立式砂带机
        Disc = 5,           // 圆盘磨床
        Polisher = 6,       // 布轮抛光机
        Orbital = 7,        // 偏心砂光机
        ValveSeat = 8,      // 气门研磨机
        GearGrinder = 9,    // 齿轮磨床
        JigGrinder = 10,    // 坐标磨床
        Internal = 11,      // 内圆磨床
        Crankshaft = 12,    // 曲轴磨床
        ToolCutter = 13,    // 万能工具磨
        Profile = 14,       // 成形磨床
        Rail = 15,          // 钢轨打磨机
        Floor = 16,         // 地面研磨机
        Angle = 17,         // 角磨机
        Die = 18,           // 直磨机
        CutOff = 19,        // 型材切割机
        Honing = 20,        // 立式珩磨机
        RotaryTable = 21    // 圆台平面磨
    }
    /// <summary>
    /// 研磨机控件（继承 HToolAnimBase）：22 种磨削设备外形、13 种色调，
    /// Running 时砂轮旋转、工作台往复/进给、接触点喷射火花。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("研磨机控件：22 种磨削设备外形、13 种色调，运行时砂轮旋转并喷射火花")]
    public class HGrinder : HToolAnimBase
    {
        private HGrinderStyle _style = HGrinderStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HGrinder()
        {
            Size = new Size(150, 132);
        }
        /// <summary>磨床外形样式。</summary>
        [HCategoryLanguage("研磨机"), HDisplayNameLanguage("磨床外形样式"), HDescriptionLanguage("磨床外形样式"), Browsable(true)]
        [DefaultValue(HGrinderStyle.Classic)]
        public HGrinderStyle GrinderStyle
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
                case HGrinderStyle.Cylindrical: DrawCylindrical(g, a, pal); break;
                case HGrinderStyle.Centerless: DrawCenterless(g, a, pal); break;
                case HGrinderStyle.Bench: DrawBench(g, a, pal); break;
                case HGrinderStyle.Belt: DrawBelt(g, a, pal); break;
                case HGrinderStyle.Disc: DrawDisc(g, a, pal); break;
                case HGrinderStyle.Polisher: DrawPolisher(g, a, pal); break;
                case HGrinderStyle.Orbital: DrawOrbital(g, a, pal); break;
                case HGrinderStyle.ValveSeat: DrawValveSeat(g, a, pal); break;
                case HGrinderStyle.GearGrinder: DrawGearGrinder(g, a, pal); break;
                case HGrinderStyle.JigGrinder: DrawJigGrinder(g, a, pal); break;
                case HGrinderStyle.Internal: DrawInternal(g, a, pal); break;
                case HGrinderStyle.Crankshaft: DrawCrankshaft(g, a, pal); break;
                case HGrinderStyle.ToolCutter: DrawToolCutter(g, a, pal); break;
                case HGrinderStyle.Profile: DrawProfile(g, a, pal); break;
                case HGrinderStyle.Rail: DrawRail(g, a, pal); break;
                case HGrinderStyle.Floor: DrawFloor(g, a, pal); break;
                case HGrinderStyle.Angle: DrawAngle(g, a, pal); break;
                case HGrinderStyle.Die: DrawDie(g, a, pal); break;
                case HGrinderStyle.CutOff: DrawCutOff(g, a, pal); break;
                case HGrinderStyle.Honing: DrawHoning(g, a, pal); break;
                case HGrinderStyle.RotaryTable: DrawRotaryTable(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>砂轮：轮体 + 旋转磨粒线 + 轮毂，可选防护罩扇形。</summary>
        private void Wheel(Graphics g, float cx, float cy, float r, bool spinning,
            HToolPalette pal, bool guard = false, float gStart = 180f, float gSweep = 180f)
        {
            using (var wc = new SolidBrush(Color.FromArgb(182, 150, 92)))
                g.FillEllipse(wc, cx - r, cy - r, r * 2f, r * 2f);
            if (spinning)
            {
                var st = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(SpinAngle * 2f);
                using (var grain = new Pen(Color.FromArgb(120, 84, 44), 1.3f))
                    for (int i = 0; i < 6; i++)
                    {
                        g.RotateTransform(60f);
                        g.DrawLine(grain, r * 0.3f, 0, r * 0.95f, 0);
                    }
                g.Restore(st);
            }
            g.DrawEllipse(new Pen(Color.FromArgb(100, 72, 38), 1.2f), cx - r, cy - r, r * 2f, r * 2f);
            g.Ell(cx, cy, r * 0.22f, r * 0.22f, pal.MetalDark);
            if (guard)
                using (var gd = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.12f)))
                    g.FillPie(gd, cx - r * 1.14f, cy - r * 1.14f, r * 2.28f, r * 2.28f, gStart, gSweep);
        }
        /// <summary>接触点火花：沿 dir 角度飞溅并下落。</summary>
        private void Sparks(Graphics g, float sx, float sy, float dir, float len, int n)
        {
            if (!Running) return;
            using (var spark = new Pen(Color.Orange, 1.3f) { StartCap = LineCap.Round })
                for (int i = 0; i < n; i++)
                {
                    float t = (Phase * 1.7f + i * 0.41f) % 1f;
                    float jit = (i % 3 - 1) * 2.2f;
                    float dx = (float)Math.Cos(dir) * len * t + jit;
                    float dy = (float)Math.Sin(dir) * len * t + t * t * 14f;
                    spark.Color = Color.FromArgb((int)(230 * (1f - t)),
                        t < 0.5f ? Color.Orange : Color.Tomato);
                    g.DrawLine(spark, sx + dx, sy + dy, sx + dx - 4f, sy + dy + 3f);
                }
        }
        // ----------------------------------------------------------------
        // Classic：往复工作台平面磨床（原验收外形）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width, glyphH = a.Height;
            // 床身
            var bed = new RectangleF(Width * 0.08f, glyphH * 0.66f, Width * 0.84f, glyphH * 0.2f);
            using (var gp = HBarBase.RoundPath(bed, 4f))
            using (var b = new LinearGradientBrush(bed, HToolPalettes.Tint(pal.Main, 0.15f),
                HToolPalettes.Shade(pal.Main, 0.3f), 90f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.4f), gp);
            }
            // 立柱
            var col = new RectangleF(Width * 0.72f, glyphH * 0.14f, Width * 0.14f, glyphH * 0.54f);
            using (var b = new LinearGradientBrush(col, HToolPalettes.Tint(pal.Main, 0.2f),
                HToolPalettes.Shade(pal.Main, 0.25f), 0f))
                g.FillRectangle(b, col);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), col.X, col.Y, col.Width, col.Height);
            // 横向磨头滑枕
            var head = new RectangleF(Width * 0.3f, glyphH * 0.26f, Width * 0.5f, glyphH * 0.12f);
            using (var gp = HBarBase.RoundPath(head, 3f))
            using (var b = new LinearGradientBrush(head, pal.Metal, pal.MetalDark, 0f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.2f), gp);
            }
            // 工作台（运行时小幅往复）
            float slide = Running ? (float)Math.Sin(Phase * 1.6f) * Width * 0.02f : 0f;
            var table = new RectangleF(Width * 0.14f + slide, glyphH * 0.56f, Width * 0.5f, glyphH * 0.1f);
            using (var tb = HBarBase.CylinderH(table, pal.MetalDark, Color.White))
                g.FillRectangle(tb, table);
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), table.X, table.Y, table.Width, table.Height);
            // 工件
            var work = new RectangleF(table.X + table.Width * 0.34f, table.Y - glyphH * 0.08f,
                table.Width * 0.26f, glyphH * 0.08f);
            using (var wb = new SolidBrush(Color.FromArgb(120, 126, 134)))
                g.FillRectangle(wb, work);
            g.DrawRectangle(new Pen(pal.Edge, 1f), work.X, work.Y, work.Width, work.Height);
            // 砂轮
            float wx = Width * 0.42f, wy = glyphH * 0.48f, wr = glyphH * 0.15f;
            Wheel(g, wx, wy, wr, true, pal);
            // 防护罩（遮上半圆）
            using (var guard = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.15f)))
                g.FillPie(guard, wx - wr * 1.12f, wy - wr * 1.12f, wr * 2.24f, wr * 2.24f, 180f, 180f);
            // 火花
            if (Running)
            {
                var rnd = new Random(3);
                using (var spark = new Pen(Color.Orange, 1.4f) { StartCap = LineCap.Round })
                {
                    for (int i = 0; i < 9; i++)
                    {
                        float t = (Phase * 1.7f + i * 0.7f) % 1f;
                        float dir = i % 2 == 0 ? 1f : -1f;
                        float sx = wx + wr * 0.86f + t * Width * 0.12f * dir + rnd.Next(-2, 3);
                        float sy = wy + wr * 0.5f - t * glyphH * 0.14f + rnd.Next(-2, 3);
                        spark.Color = Color.FromArgb((int)(230 * (1f - t)),
                            t < 0.5f ? Color.Orange : Color.Tomato);
                        g.DrawLine(spark, sx, sy, sx - 6f * dir, sy + 3f);
                    }
                }
            }
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // Cylindrical：外圆磨床（顶针长工件 + 大砂轮）
        // ----------------------------------------------------------------
        private void DrawCylindrical(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 床身
            g.Box(new RectangleF(a.X + a.Width * 0.06f, a.Bottom - a.Height * 0.26f,
                a.Width * 0.88f, a.Height * 0.18f), 4f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            // 头尾架
            foreach (float fx in new[] { 0.14f, 0.86f })
                g.Box(new RectangleF(a.X + a.Width * fx - 7f, a.Bottom - a.Height * 0.44f,
                    14f, a.Height * 0.2f), 2f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.2f);
            // 工件（旋转圆柱）
            float cy = a.Bottom - a.Height * 0.38f;
            var work = new RectangleF(a.X + a.Width * 0.16f, cy - 7f, a.Width * 0.68f, 14f);
            g.FillCylH(work, Color.FromArgb(120, 126, 134), Color.FromArgb(190, 196, 204));
            g.DrawRectangle(new Pen(pal.Edge, 1.1f), work.X, work.Y, work.Width, work.Height);
            if (Running)
                using (var ln = new Pen(Color.FromArgb(120, Color.Black), 1f))
                    g.DrawLine(ln, work.X + 4f, cy, work.Right - 4f, cy);
            // 砂轮架（前侧大轮）
            float wx = a.X + a.Width * 0.34f, wy = a.Bottom - a.Height * 0.5f, wr = a.Height * 0.18f;
            g.Box(new RectangleF(wx - 8f, wy - wr - 8f, 16f, 10f), 2f,
                HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.1f);
            Wheel(g, wx, wy, wr, Running, pal, true, 160f, 200f);
            Sparks(g, wx + wr * 0.8f, wy + wr * 0.4f, 0.15f, a.Width * 0.16f, 8);
        }
        // ----------------------------------------------------------------
        // Centerless：无心磨（砂轮 + 导轮 + 工件 + 托刀）
        // ----------------------------------------------------------------
        private void DrawCenterless(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.08f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.84f, a.Height * 0.14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            float cy = a.Top + a.Height * 0.46f;
            float r = a.Height * 0.22f;
            float x1 = a.X + a.Width * 0.32f, x2 = a.X + a.Width * 0.68f;
            Wheel(g, x1, cy, r, Running, pal);
            // 导轮（橡胶深色）
            g.Ell(x2, cy, r * 0.7f, r * 0.7f, Color.FromArgb(70, 72, 78));
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), x2 - r * 0.7f, cy - r * 0.7f, r * 1.4f, r * 1.4f);
            // 工件小圆（两 轮之间上方）
            float px = a.X + a.Width * 0.5f, py = cy - r * 0.66f;
            g.Ell(px, py, r * 0.26f, r * 0.26f, Color.FromArgb(180, 186, 194));
            g.DrawEllipse(new Pen(pal.Edge, 1.1f), px - r * 0.26f, py - r * 0.26f, r * 0.52f, r * 0.52f);
            // 托刀
            using (var blade = new Pen(Color.FromArgb(150, 154, 162), 3f))
                g.DrawLine(blade, px, py + r * 0.26f, px, a.Bottom - a.Height * 0.2f);
            Sparks(g, px - r * 0.12f, py, (float)Math.PI * 1.15f, a.Width * 0.1f, 7);
        }
        // ----------------------------------------------------------------
        // Bench：台式砂轮机（底座 + 电机 + 左右双轮护罩 + 刀架）
        // ----------------------------------------------------------------
        private void DrawBench(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f;
            // 底座 + 立柱
            g.Box(new RectangleF(a.X + a.Width * 0.32f, a.Bottom - 10f, a.Width * 0.36f, 10f),
                3f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            g.Line(pal.MetalDark, 4f, a.X + a.Width * 0.5f, cy + 10f,
                a.X + a.Width * 0.5f, a.Bottom - 10f);
            // 电机筒
            var motor = new RectangleF(a.X + a.Width * 0.34f, cy - 13f, a.Width * 0.32f, 26f);
            g.FillCylH(motor, HToolPalettes.Shade(pal.Main, 0.25f), HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), motor.X, motor.Y, motor.Width, motor.Height);
            // 左右砂轮 + 护罩 + 刀架
            foreach (int s in new[] { -1, 1 })
            {
                float wx = a.X + a.Width * 0.5f + s * a.Width * 0.26f;
                Wheel(g, wx, cy, a.Height * 0.16f, Running, pal, true,
                    s < 0 ? 200f : -20f, 160f);
                g.Line(pal.MetalDark, 2.4f, wx - s * 12f, cy + a.Height * 0.15f,
                    wx + s * 6f, cy + a.Height * 0.15f);
            }
            Sparks(g, a.X + a.Width * 0.5f - a.Width * 0.26f, cy + a.Height * 0.1f,
                (float)Math.PI * 1.1f, a.Width * 0.1f, 6);
        }
        // ----------------------------------------------------------------
        // Belt：立式砂带机（上下滚轮 + 砂带环 + 工作台）
        // ----------------------------------------------------------------
        private void DrawBelt(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width * 0.42f;
            float ry1 = a.Top + a.Height * 0.18f, ry2 = a.Bottom - a.Height * 0.2f;
            float rr = a.Width * 0.13f;
            // 砂带环
            using (var belt = new SolidBrush(Color.FromArgb(74, 60, 46)))
            {
                g.FillRectangle(belt, cx - rr, ry1, rr * 2f, ry2 - ry1);
                g.FillEllipse(belt, cx - rr, ry1 - rr, rr * 2f, rr * 2f);
                g.FillEllipse(belt, cx - rr, ry2 - rr, rr * 2f, rr * 2f);
            }
            // 砂带运动纹理
            if (Running)
                using (var ln = new Pen(Color.FromArgb(120, 110, 88), 1.3f))
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.2f) % 1f;
                        float y = ry1 + t * (ry2 - ry1);
                        g.DrawLine(ln, cx - rr * 0.8f, y, cx + rr * 0.8f, y);
                    }
            // 滚轮轮毂
            foreach (float yy in new[] { ry1, ry2 })
            {
                g.DrawEllipse(new Pen(pal.Edge, 1.2f), cx - rr, yy - rr, rr * 2f, rr * 2f);
                g.Ell(cx, yy, rr * 0.26f, rr * 0.26f, pal.MetalDark);
            }
            // 机架 + 工作台
            g.Line(pal.MetalDark, 3.4f, cx + rr, ry1, cx + rr, a.Bottom - 8f);
            g.Box(new RectangleF(cx + rr - 2f, a.Top + a.Height * 0.52f,
                a.Width * 0.34f, 7f), 1f, pal.Metal, pal.Edge, 1.1f);
            Sparks(g, cx + rr, a.Top + a.Height * 0.52f, 0f, a.Width * 0.12f, 7);
        }
        // ----------------------------------------------------------------
        // Disc：圆盘磨床（大立式圆盘 + 工件条）
        // ----------------------------------------------------------------
        private void DrawDisc(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width * 0.44f, cy = a.Y + a.Height * 0.56f;
            float r = Math.Min(a.Width, a.Height) * 0.38f;
            // 圆盘（旋转）
            g.Ell(cx, cy, r, r, Color.FromArgb(150, 154, 160));
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - r, cy - r, r * 2f, r * 2f);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform(SpinAngle);
                using (var ln = new Pen(Color.FromArgb(120, 90, 60), 1.2f))
                    for (int i = 0; i < 8; i++)
                    {
                        g.RotateTransform(45f);
                        g.DrawLine(ln, r * 0.2f, 0, r * 0.9f, 0);
                    }
                g.Restore(st);
            }
            g.Ell(cx, cy, r * 0.1f, r * 0.1f, pal.MetalDark);
            // 工作台 + 工件条（靠在盘侧）
            g.Box(new RectangleF(cx + r * 0.3f, cy - r * 0.7f, a.Width * 0.3f, 8f), 2f,
                pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(cx + r * 0.5f, cy - r * 0.7f - 6f, a.Width * 0.16f, 6f),
                1f, Color.FromArgb(120, 126, 134), pal.Edge, 1f);
            Sparks(g, cx + r * 0.72f, cy - r * 0.4f, -0.3f, a.Width * 0.1f, 6);
        }
        // ----------------------------------------------------------------
        // Polisher：布轮抛光机（细轴 + 软布轮 + 抛光蜡）
        // ----------------------------------------------------------------
        private void DrawPolisher(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f;
            // 电机
            var motor = new RectangleF(a.X + a.Width * 0.12f, cy - 12f, a.Width * 0.3f, 24f);
            g.FillCylH(motor, HToolPalettes.Shade(pal.Main, 0.25f), HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), motor.X, motor.Y, motor.Width, motor.Height);
            g.Line(pal.MetalDark, 3f, motor.Right, cy, motor.Right + 16f, cy);
            // 布轮（多层波纹）
            float wx = motor.Right + 26f, wr = a.Height * 0.28f;
            using (var cloth = new SolidBrush(Color.FromArgb(224, 214, 186)))
                g.FillEllipse(cloth, wx - wr, cy - wr, wr * 2f, wr * 2f);
            for (int i = 1; i <= 3; i++)
                g.DrawEllipse(new Pen(Color.FromArgb(170, 160, 130), 1f),
                    wx - wr * (1f - i * 0.18f), cy - wr * (1f - i * 0.18f),
                    wr * 2f * (1f - i * 0.18f), wr * 2f * (1f - i * 0.18f));
            g.Ell(wx, cy, wr * 0.16f, wr * 0.16f, pal.MetalDark);
            // 抛光蜡条
            g.Box(new RectangleF(wx - 5f, cy + wr + 4f, 26f, 8f), 2f,
                Color.FromArgb(214, 180, 96), pal.Edge, 1f);
            // 抛光粉光
            Sparks(g, wx, cy - wr * 0.4f, -1.2f, a.Width * 0.08f, 6);
        }
        // ----------------------------------------------------------------
        // Orbital：手持偏心砂光机
        // ----------------------------------------------------------------
        private void DrawOrbital(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width * 0.5f, cy = a.Y + a.Height * 0.62f;
            float pr = a.Width * 0.28f;
            // 砂光盘（偏心旋转）
            float ex = Running ? (float)Math.Cos(SpinAngle) * 3f : 0f;
            using (var pad = new SolidBrush(Color.FromArgb(196, 170, 110)))
                g.FillEllipse(pad, cx - pr + ex, cy - pr, pr * 2f, pr * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), cx - pr + ex, cy - pr, pr * 2f, pr * 2f);
            // 机身穹顶 + 握柄
            g.Box(new RectangleF(cx - pr * 0.62f, cy - pr - 18f, pr * 1.24f, 22f), 8f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            g.Box(new RectangleF(cx - 9f, cy - pr - 34f, 18f, 18f), 5f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            g.Lamp(cx + pr * 0.5f, cy - pr - 6f, Running);
            // 尘屑
            if (Running)
                using (var dust = new SolidBrush(Color.FromArgb(150, 200, 180, 140)))
                    for (int i = 0; i < 6; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.17f) % 1f;
                        g.FillEllipse(dust, cx - pr - 4f - t * 18f, cy + pr - t * 10f, 2.6f, 2.6f);
                    }
        }
        // ----------------------------------------------------------------
        // ValveSeat：气门研磨机（机架 + 旋转吸盘杆 + 气门盘）
        // ----------------------------------------------------------------
        private void DrawValveSeat(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width / 2f;
            // 底座 + 立柱 + 悬臂
            g.Box(new RectangleF(cx - a.Width * 0.32f, a.Bottom - 10f, a.Width * 0.64f, 10f),
                3f, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.2f);
            g.Line(pal.MetalDark, 3.4f, cx - a.Width * 0.24f, a.Top + 14f,
                cx - a.Width * 0.24f, a.Bottom - 10f);
            g.Line(pal.MetalDark, 4f, cx - a.Width * 0.24f, a.Top + 14f,
                cx, a.Top + 14f);
            // 旋转吸盘杆（上下小幅往复）
            float lift = Running ? (float)Math.Sin(Phase * 2.2f) * 3f : 0f;
            g.Line(pal.Metal, 3f, cx, a.Top + 16f, cx, a.Top + a.Height * 0.52f + lift);
            // 气门（杆 + 盘）
            float vy = a.Top + a.Height * 0.52f + lift;
            g.Line(Color.FromArgb(120, 126, 134), 3f, cx, vy, cx, vy + a.Height * 0.22f);
            g.Poly(new[]
            {
                new PointF(cx - 16f, vy + a.Height * 0.22f),
                new PointF(cx + 16f, vy + a.Height * 0.22f),
                new PointF(cx + 10f, vy + a.Height * 0.28f),
                new PointF(cx - 10f, vy + a.Height * 0.28f)
            }, Color.FromArgb(160, 164, 172), pal.Edge, 1.1f);
            // 座圈
            g.Line(pal.MetalDark, 3.6f, cx - 20f, vy + a.Height * 0.28f,
                cx + 20f, vy + a.Height * 0.28f);
            // 旋转扭纹
            if (Running)
                using (var tw = new Pen(Color.FromArgb(160, pal.Accent), 1.2f)
                { DashStyle = DashStyle.Dot })
                    g.DrawLine(tw, cx - 3f, a.Top + 22f, cx + 3f, vy - 4f);
        }
        // ----------------------------------------------------------------
        // GearGrinder：齿轮磨床（成形砂轮 + 齿轮坯）
        // ----------------------------------------------------------------
        private void DrawGearGrinder(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.8f, a.Height * 0.14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            // 立柱 + 磨头
            g.Box(new RectangleF(a.X + a.Width * 0.42f, a.Top + 8f, a.Width * 0.16f, a.Height * 0.34f),
                3f, HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.2f);
            float wx = a.X + a.Width * 0.5f, wy = a.Top + a.Height * 0.4f, wr = a.Width * 0.12f;
            Wheel(g, wx, wy, wr, Running, pal, false);
            // 齿轮坯（慢速旋转 + 齿）
            float gx = a.X + a.Width * 0.5f, gy = a.Bottom - a.Height * 0.34f, gr = a.Height * 0.2f;
            var st = g.Save();
            g.TranslateTransform(gx, gy);
            g.RotateTransform(Running ? SpinAngle * 0.5f : 0f);
            using (var gb = new SolidBrush(Color.FromArgb(150, 154, 162)))
            using (var gp = new Pen(pal.Edge, 1.2f))
            {
                g.FillEllipse(gb, -gr, -gr, gr * 2f, gr * 2f);
                g.DrawEllipse(gp, -gr, -gr, gr * 2f, gr * 2f);
                for (int i = 0; i < 12; i++)
                {
                    double ang = i * Math.PI / 6.0;
                    float tx = (float)Math.Cos(ang) * gr, ty = (float)Math.Sin(ang) * gr;
                    g.FillRectangle(gb, tx - 2.5f, ty - 2.5f, 5f, 5f);
                }
            }
            g.Restore(st);
            g.Ell(gx, gy, gr * 0.22f, gr * 0.22f, pal.MetalDark);
            Sparks(g, wx, wy + wr, (float)Math.PI / 2f, a.Width * 0.06f, 6);
        }
        // ----------------------------------------------------------------
        // JigGrinder：坐标磨床（立柱 + 行星偏心磨头伸入工件孔）
        // ----------------------------------------------------------------
        private void DrawJigGrinder(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 床身 + 十字工作台
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.22f,
                a.Width * 0.8f, a.Height * 0.16f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            g.Box(new RectangleF(a.X + a.Width * 0.2f, a.Bottom - a.Height * 0.32f,
                a.Width * 0.6f, a.Height * 0.1f), 2f, pal.Metal, pal.Edge, 1.2f);
            // 龙门立柱 + 横梁
            g.Line(pal.MetalDark, 5f, a.X + a.Width * 0.18f, a.Top + 10f,
                a.X + a.Width * 0.18f, a.Bottom - a.Height * 0.32f);
            g.Line(pal.MetalDark, 5f, a.Right - a.Width * 0.18f, a.Top + 10f,
                a.Right - a.Width * 0.18f, a.Bottom - a.Height * 0.32f);
            g.Line(pal.MetalDark, 5f, a.X + a.Width * 0.18f, a.Top + 10f,
                a.Right - a.Width * 0.18f, a.Top + 10f);
            // 滑块 + 磨杆
            float hx = a.X + a.Width * 0.5f;
            float orb = Running ? (float)Math.Cos(SpinAngle) * 2.4f : 0f;
            g.Box(new RectangleF(hx - 10f, a.Top + 10f, 20f, 16f), 2f,
                HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.1f);
            g.Line(pal.Metal, 2.6f, hx + orb, a.Top + 26f, hx + orb, a.Bottom - a.Height * 0.42f);
            // 工件（带孔方块）+ 孔内磨头
            var work = new RectangleF(hx - 22f, a.Bottom - a.Height * 0.42f, 44f, a.Height * 0.14f);
            g.Box(work, 2f, Color.FromArgb(130, 134, 142), pal.Edge, 1.2f);
            g.Ell(hx + orb, a.Bottom - a.Height * 0.35f, 7f, 7f, Color.FromArgb(60, 64, 72));
            Sparks(g, hx + orb + 6f, a.Bottom - a.Height * 0.35f, 0f, 8f, 5);
        }
        // ----------------------------------------------------------------
        // Internal：内圆磨床（工件盘 + 伸入孔内的砂轮杆）
        // ----------------------------------------------------------------
        private void DrawInternal(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + 6f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.86f, a.Height * 0.14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            float cy = a.Bottom - a.Height * 0.4f;
            // 工件（带孔环）
            float wx = a.X + a.Width * 0.66f, wr = a.Height * 0.2f;
            g.Ell(wx, cy, wr, wr, Color.FromArgb(140, 144, 152));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), wx - wr, cy - wr, wr * 2f, wr * 2f);
            g.Ell(wx, cy, wr * 0.55f, wr * 0.55f, Color.FromArgb(58, 62, 70));
            // 卡盘（旋转纹）
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(wx, cy);
                g.RotateTransform(SpinAngle);
                using (var ln = new Pen(Color.FromArgb(200, pal.Metal), 1.4f))
                    for (int i = 0; i < 3; i++)
                    {
                        g.RotateTransform(120f);
                        g.DrawLine(ln, wr * 0.6f, 0, wr * 0.9f, 0);
                    }
                g.Restore(st);
            }
            // 砂轮杆（从左伸入孔）
            float sx = wx - wr * 0.5f;
            g.Line(pal.MetalDark, 4f, a.X + a.Width * 0.14f, cy, sx, cy);
            g.Ell(sx, cy, wr * 0.34f, wr * 0.34f, Color.FromArgb(182, 150, 92));
            Sparks(g, sx, cy - wr * 0.2f, -1.4f, a.Width * 0.06f, 5);
        }
        // ----------------------------------------------------------------
        // Crankshaft：曲轴磨床（长床身 + 大圆砂轮 + 曲轴偏心颈）
        // ----------------------------------------------------------------
        private void DrawCrankshaft(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + 4f, a.Bottom - a.Height * 0.22f,
                a.Width - 8f, a.Height * 0.16f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            // 头尾架
            foreach (float fx in new[] { 0.1f, 0.9f })
                g.Box(new RectangleF(a.X + a.Width * fx - 6f, a.Bottom - a.Height * 0.4f,
                    12f, a.Height * 0.2f), 2f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.1f);
            float cy = a.Bottom - a.Height * 0.34f;
            // 曲轴：主轴 + 偏心曲柄
            g.Line(Color.FromArgb(120, 126, 134), 5f, a.X + a.Width * 0.1f, cy,
                a.X + a.Width * 0.9f, cy);
            float orb = Running ? SpinAngle * (float)Math.PI / 180f : 0.5f;
            for (int i = 0; i < 2; i++)
            {
                float px = a.X + a.Width * (0.34f + 0.24f * i);
                float py = cy + (float)Math.Sin(orb + i * Math.PI) * 8f;
                g.Ell(px, py, 7f, 7f, Color.FromArgb(160, 164, 172));
                g.DrawEllipse(new Pen(pal.Edge, 1f), px - 7f, py - 7f, 14f, 14f);
                g.Line(Color.FromArgb(110, 114, 122), 2.4f, px, cy, px, py);
            }
            // 砂轮架 + 大轮
            float wx = a.X + a.Width * 0.3f, wy = a.Bottom - a.Height * 0.52f, wr = a.Height * 0.17f;
            Wheel(g, wx, wy, wr, Running, pal, true, 170f, 180f);
            Sparks(g, wx + wr * 0.6f, wy + wr * 0.6f, 0.5f, a.Width * 0.08f, 6);
        }
        // ----------------------------------------------------------------
        // ToolCutter：万能工具磨（可调角度磨头 + 工作台 + 刀具）
        // ----------------------------------------------------------------
        private void DrawToolCutter(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.08f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.84f, a.Height * 0.14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            // 可调角度工作台
            float tilt = -0.15f;
            var st = g.Save();
            g.TranslateTransform(a.X + a.Width * 0.42f, a.Bottom - a.Height * 0.24f);
            g.RotateTransform(tilt * 180f / (float)Math.PI);
            g.Box(new RectangleF(-a.Width * 0.3f, -5f, a.Width * 0.6f, 10f), 2f,
                pal.Metal, pal.Edge, 1.1f);
            // 刀具（铣刀条）
            using (var tool = new Pen(Color.FromArgb(150, 154, 162), 3.4f))
                g.DrawLine(tool, -a.Width * 0.22f, -6f, a.Width * 0.2f, -6f);
            g.Restore(st);
            // 磨头（斜置）
            float hx = a.X + a.Width * 0.66f, hy = a.Top + a.Height * 0.36f;
            var st2 = g.Save();
            g.TranslateTransform(hx, hy);
            g.RotateTransform(-25f);
            g.Box(new RectangleF(-12f, -8f, 24f, 16f), 3f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.2f);
            Wheel(g, -16f, 0f, a.Height * 0.13f, Running, pal, false);
            g.Restore(st2);
            g.Line(pal.MetalDark, 4f, hx, hy + 8f, hx, a.Bottom - a.Height * 0.2f);
            Sparks(g, hx - 14f, hy + 6f, -2.6f, a.Width * 0.08f, 6);
        }
        // ----------------------------------------------------------------
        // Profile：成形磨床（样板 + 成形截面砂轮）
        // ----------------------------------------------------------------
        private void DrawProfile(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.22f,
                a.Width * 0.8f, a.Height * 0.14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            float cx = a.X + a.Width * 0.5f, wy = a.Top + a.Height * 0.36f;
            float wr = a.Width * 0.16f;
            // 成形砂轮（外廓带 V 形截面）
            using (var wc = new SolidBrush(Color.FromArgb(182, 150, 92)))
            {
                g.FillEllipse(wc, cx - wr, wy - wr, wr * 2f, wr * 2f);
                g.FillPolygon(wc, new[]
                {
                    new PointF(cx - 8f, wy), new PointF(cx, wy + 9f),
                    new PointF(cx + 8f, wy)
                });
            }
            g.DrawEllipse(new Pen(Color.FromArgb(100, 72, 38), 1.2f),
                cx - wr, wy - wr, wr * 2f, wr * 2f);
            g.Ell(cx, wy, wr * 0.2f, wr * 0.2f, pal.MetalDark);
            // 立柱
            g.Box(new RectangleF(cx - 9f, a.Top + 6f, 18f, a.Height * 0.18f), 2f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.1f);
            // 样板工件（齿形条）
            float ty = a.Bottom - a.Height * 0.3f;
            using (var wp = new SolidBrush(Color.FromArgb(130, 134, 142)))
            {
                g.FillRectangle(wp, cx - wr - 6f, ty, wr * 2f + 12f, 10f);
                for (int i = 0; i < 5; i++)
                    g.FillPolygon(wp, new[]
                    {
                        new PointF(cx - wr - 4f + i * 8f, ty),
                        new PointF(cx - wr + i * 8f, ty),
                        new PointF(cx - wr - 2f + i * 8f, ty - 7f)
                    });
            }
            Sparks(g, cx, wy + wr * 0.7f, (float)Math.PI / 2f, a.Width * 0.05f, 6);
        }
        // ----------------------------------------------------------------
        // Rail：钢轨打磨机（杯形砂轮压在钢轨头上）
        // ----------------------------------------------------------------
        private void DrawRail(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 钢轨剖面（轨底 + 轨腰 + 轨头）
            float ry = a.Bottom - a.Height * 0.26f;
            g.Poly(new[]
            {
                new PointF(a.Left + a.Width * 0.12f, ry + 16f),
                new PointF(a.Right - a.Width * 0.12f, ry + 16f),
                new PointF(a.Right - a.Width * 0.16f, ry + 10f),
                new PointF(a.X + a.Width * 0.5f + 5f, ry + 10f),
                new PointF(a.X + a.Width * 0.5f + 3f, ry + 3f),
                new PointF(a.X + a.Width * 0.5f + 8f, ry),
                new PointF(a.X + a.Width * 0.5f - 8f, ry),
                new PointF(a.X + a.Width * 0.5f - 3f, ry + 3f),
                new PointF(a.X + a.Width * 0.5f - 5f, ry + 10f),
                new PointF(a.Left + a.Width * 0.16f, ry + 10f)
            }, Color.FromArgb(120, 126, 134), pal.Edge, 1.2f);
            // 跨轨车架 + 推手
            float cx = a.X + a.Width / 2f;
            g.Box(new RectangleF(cx - a.Width * 0.2f, a.Top + a.Height * 0.36f,
                a.Width * 0.4f, a.Height * 0.12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.2f);
            using (var frame = new Pen(pal.MetalDark, 3f))
            {
                g.DrawLine(frame, cx - a.Width * 0.16f, a.Top + a.Height * 0.36f,
                    cx - a.Width * 0.26f, ry + 2f);
                g.DrawLine(frame, cx + a.Width * 0.16f, a.Top + a.Height * 0.36f,
                    cx + a.Width * 0.26f, ry + 2f);
                g.DrawLine(frame, cx, a.Top + a.Height * 0.36f, cx, a.Top + 12f);
                g.DrawLine(frame, cx, a.Top + 12f, cx + a.Width * 0.2f, a.Top + 4f);
            }
            // 杯形砂轮（压在轨头，旋转）
            float wx = cx, wy = a.Top + a.Height * 0.56f, wr = a.Width * 0.12f;
            using (var cup = new SolidBrush(Color.FromArgb(182, 150, 92)))
                g.FillEllipse(cup, wx - wr, wy - wr * 0.45f, wr * 2f, wr * 0.9f);
            g.DrawEllipse(new Pen(Color.FromArgb(100, 72, 38), 1.2f),
                wx - wr, wy - wr * 0.45f, wr * 2f, wr * 0.9f);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(wx, wy);
                g.RotateTransform(SpinAngle * 2f);
                using (var ln = new Pen(Color.FromArgb(120, 84, 44), 1.2f))
                    for (int i = 0; i < 4; i++)
                    {
                        g.RotateTransform(90f);
                        g.DrawLine(ln, -wr * 0.7f, 0, wr * 0.7f, 0);
                    }
                g.Restore(st);
            }
            Sparks(g, wx, wy + wr * 0.4f, (float)Math.PI / 2f, a.Width * 0.08f, 7);
        }
        // ----------------------------------------------------------------
        // Floor：地面研磨机（三磨盘 + 推手 + 行走轮）
        // ----------------------------------------------------------------
        private void DrawFloor(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width * 0.42f, cy = a.Y + a.Height * 0.62f;
            float rr = a.Width * 0.3f;
            // 机罩（圆盘）
            g.Ell(cx, cy, rr, rr * 0.34f, HToolPalettes.Shade(pal.Main, 0.14f));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - rr, cy - rr * 0.34f, rr * 2f, rr * 0.68f);
            // 三个行星磨盘
            for (int i = 0; i < 3; i++)
            {
                double ang = i * 2.094 + (Running ? SpinAngle * 0.05f : 0f);
                float px = cx + (float)Math.Cos(ang) * rr * 0.55f;
                float py = cy + (float)Math.Sin(ang) * rr * 0.16f;
                g.Ell(px, py, rr * 0.24f, rr * 0.24f, Color.FromArgb(110, 100, 92));
                g.DrawEllipse(new Pen(pal.Edge, 1f),
                    px - rr * 0.24f, py - rr * 0.24f, rr * 0.48f, rr * 0.48f);
            }
            g.Ell(cx, cy - rr * 0.34f - 8f, 6f, 6f, pal.MetalDark);
            // 推手 + 行走轮
            using (var handle = new Pen(pal.MetalDark, 3f))
            {
                g.DrawLine(handle, cx + rr * 0.5f, cy - rr * 0.3f,
                    a.Right - 8f, a.Top + a.Height * 0.18f);
                g.DrawLine(handle, a.Right - 14f, a.Top + a.Height * 0.18f,
                    a.Right - 4f, a.Top + a.Height * 0.18f);
            }
            g.Ell(cx - rr * 0.6f, cy + rr * 0.3f, 5f, 5f, Color.FromArgb(48, 50, 56));
        }
        // ----------------------------------------------------------------
        // Angle：手持角磨机
        // ----------------------------------------------------------------
        private void DrawAngle(Graphics g, RectangleF a, HToolPalette pal)
        {
            float hx = a.X + a.Width * 0.4f, hy = a.Y + a.Height * 0.42f;
            // 机身 + 尾柄
            g.Box(new RectangleF(hx - 8f, hy - 11f, a.Width * 0.42f, 22f), 8f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            g.Poly(new[]
            {
                new PointF(hx + a.Width * 0.32f, hy - 8f),
                new PointF(a.Right - 6f, hy - 4f),
                new PointF(a.Right - 6f, hy + 6f),
                new PointF(hx + a.Width * 0.32f, hy + 8f)
            }, HToolPalettes.Shade(pal.Main, 0.22f), pal.Edge, 1.2f);
            // 辅助手柄
            g.Line(pal.MetalDark, 3f, hx + 6f, hy - 10f, hx + 20f, hy - 26f);
            // 齿轮头 + 薄片砂轮 + 护罩
            g.Box(new RectangleF(hx - 16f, hy - 8f, 14f, 16f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            float wx = hx - 18f, wy = hy + 14f, wr = a.Height * 0.2f;
            using (var disc = new SolidBrush(Color.FromArgb(120, 124, 132)))
                g.FillEllipse(disc, wx - wr, wy - wr, wr * 2f, wr * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), wx - wr, wy - wr, wr * 2f, wr * 2f);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(wx, wy);
                g.RotateTransform(SpinAngle * 2f);
                using (var ln = new Pen(Color.FromArgb(180, 184, 192), 1.2f))
                    g.DrawLine(ln, -wr * 0.8f, 0, wr * 0.8f, 0);
                g.Restore(st);
            }
            using (var gd = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.12f)))
                g.FillPie(gd, wx - wr * 1.1f, wy - wr * 1.1f, wr * 2.2f, wr * 2.2f, 220f, 120f);
            // 工件
            g.Line(pal.MetalDark, 4f, a.X + 8f, wy + wr + 4f, wx + wr, wy + wr + 4f);
            Sparks(g, wx - wr * 0.7f, wy + wr * 0.6f, (float)Math.PI * 1.15f,
                a.Width * 0.14f, 8);
        }
        // ----------------------------------------------------------------
        // Die：直磨机（笔状机身 + 磨头）
        // ----------------------------------------------------------------
        private void DrawDie(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width * 0.52f, cy = a.Y + a.Height * 0.46f;
            // 笔状机身（斜置）
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(-28f);
            g.Box(new RectangleF(-a.Width * 0.3f, -9f, a.Width * 0.52f, 18f), 8f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            // 夹头 + 轴
            g.Box(new RectangleF(a.Width * 0.22f, -5f, 12f, 10f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            g.Line(pal.Metal, 3f, a.Width * 0.34f, 0, a.Width * 0.44f, 0);
            // 旋转磨头
            float bx = a.Width * 0.48f;
            using (var bb = new SolidBrush(Color.FromArgb(150, 120, 90)))
                g.FillEllipse(bb, bx - 7f, -7f, 14f, 14f);
            if (Running)
            {
                g.RotateTransform(SpinAngle * 3f);
                using (var ln = new Pen(Color.FromArgb(110, 80, 50), 1.1f))
                    g.DrawLine(ln, bx - 6f, 0, bx + 6f, 0);
            }
            g.Restore(st);
            // 工件
            g.Box(new RectangleF(a.Right - a.Width * 0.26f, a.Bottom - a.Height * 0.26f,
                a.Width * 0.22f, a.Height * 0.14f), 2f,
                Color.FromArgb(130, 134, 142), pal.Edge, 1.2f);
            Sparks(g, a.Right - a.Width * 0.26f, a.Bottom - a.Height * 0.28f,
                (float)Math.PI * 1.1f, a.Width * 0.08f, 6);
        }
        // ----------------------------------------------------------------
        // CutOff：型材切割机（下压臂 + 薄片 + 夹料台）
        // ----------------------------------------------------------------
        private void DrawCutOff(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 底座夹料台
            g.Box(new RectangleF(a.X + a.Width * 0.08f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.84f, a.Height * 0.12f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            // 型材（角钢）
            using (var steel = new SolidBrush(Color.FromArgb(120, 126, 134)))
            {
                g.FillRectangle(steel, a.X + a.Width * 0.2f, a.Bottom - a.Height * 0.24f,
                    a.Width * 0.5f, 6f);
                g.FillRectangle(steel, a.X + a.Width * 0.2f, a.Bottom - a.Height * 0.32f,
                    6f, a.Height * 0.1f);
            }
            // 铰链 + 压臂（下压）
            float px = a.X + a.Width * 0.66f, py = a.Bottom - a.Height * 0.2f;
            g.Ell(px, py, 4f, 4f, pal.MetalDark);
            float press = Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) : 0.35f;
            float ang = -1.9f + press * 0.5f;
            float armLen = a.Width * 0.42f;
            float ex = px + (float)Math.Cos(ang) * armLen;
            float ey = py + (float)Math.Sin(ang) * armLen;
            g.Line(HToolPalettes.Shade(pal.Main, 0.1f), 6f, px, py, ex, ey);
            // 电机 + 薄片
            g.Box(new RectangleF(ex - 12f, ey - 9f, 20f, 18f), 4f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            float wr = a.Height * 0.2f;
            using (var disc = new SolidBrush(Color.FromArgb(110, 114, 122)))
                g.FillEllipse(disc, ex - wr, ey - wr, wr * 2f, wr * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), ex - wr, ey - wr, wr * 2f, wr * 2f);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(ex, ey);
                g.RotateTransform(SpinAngle * 2f);
                using (var ln = new Pen(Color.FromArgb(180, 184, 192), 1.2f))
                    g.DrawLine(ln, -wr * 0.85f, 0, wr * 0.85f, 0);
                g.Restore(st);
            }
            g.Ell(ex, ey, wr * 0.16f, wr * 0.16f, pal.MetalDark);
            Sparks(g, ex - wr * 0.5f, ey + wr * 0.7f, (float)Math.PI * 1.05f,
                a.Width * 0.12f, 8);
        }
        // ----------------------------------------------------------------
        // Honing：立式珩磨机（珩头在缸体内旋转 + 往复）
        // ----------------------------------------------------------------
        private void DrawHoning(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width / 2f;
            // 顶梁 + 立柱
            g.Line(pal.MetalDark, 5f, a.X + a.Width * 0.2f, a.Top + 10f,
                a.X + a.Width * 0.2f, a.Bottom - 10f);
            g.Line(pal.MetalDark, 5f, a.X + a.Width * 0.2f, a.Top + 10f,
                a.Right - a.Width * 0.2f, a.Top + 10f);
            // 缸体（工件）
            var block = new RectangleF(cx - a.Width * 0.2f, a.Bottom - a.Height * 0.42f,
                a.Width * 0.4f, a.Height * 0.34f);
            g.Box(block, 3f, Color.FromArgb(120, 126, 134), pal.Edge, 1.3f);
            float boreR = a.Width * 0.11f;
            g.Ell(cx, block.Y + 6f, boreR, boreR * 0.4f, Color.FromArgb(54, 58, 66));
            // 珩磨杆（旋转 + 往复）
            float rec = Running ? (float)(Math.Sin(Phase * 1.3f) * 0.5 + 0.5) : 0.5f;
            float hy = a.Top + 16f + rec * a.Height * 0.16f;
            g.Line(pal.Metal, 3.4f, cx, a.Top + 10f, cx, hy + a.Height * 0.26f);
            // 珩头油石条（旋转）
            var st = g.Save();
            g.TranslateTransform(cx, hy + a.Height * 0.18f);
            g.RotateTransform(Running ? SpinAngle : 0f);
            using (var stone = new SolidBrush(Color.FromArgb(182, 150, 92)))
                for (int i = 0; i < 4; i++)
                {
                    g.RotateTransform(90f);
                    g.FillRectangle(stone, boreR * 0.55f, -2.4f, boreR * 0.5f, 4.8f);
                }
            g.Restore(st);
            g.Ell(cx, hy + a.Height * 0.18f, 4f, 4f, pal.MetalDark);
            // 油 石 磨粒火花（少量）
            Sparks(g, cx + boreR * 0.7f, block.Y + 8f, 0.2f, 8f, 4);
        }
        // ----------------------------------------------------------------
        // RotaryTable：圆台平面磨（圆形回转工作台 + 立轴碗形砂轮）
        // ----------------------------------------------------------------
        private void DrawRotaryTable(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 床身
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.8f, a.Height * 0.14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            // 圆工作台（回转）
            float tx = a.X + a.Width * 0.44f, ty = a.Bottom - a.Height * 0.3f;
            float tr = a.Width * 0.28f;
            g.Ell(tx, ty, tr, tr * 0.32f, pal.Metal);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), tx - tr, ty - tr * 0.32f, tr * 2f, tr * 0.64f);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(tx, ty);
                g.RotateTransform(SpinAngle * 0.6f);
                using (var ln = new Pen(Color.FromArgb(160, 120, 80), 1.2f))
                    for (int i = 0; i < 4; i++)
                    {
                        g.RotateTransform(90f);
                        g.DrawLine(ln, tr * 0.3f, 0, tr * 0.9f, 0);
                    }
                g.Restore(st);
            }
            // 立柱 + 立轴 + 碗形砂轮
            g.Box(new RectangleF(a.Right - a.Width * 0.24f, a.Top + a.Height * 0.14f,
                a.Width * 0.12f, a.Height * 0.42f), 2f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.2f);
            g.Line(pal.MetalDark, 4f, a.Right - a.Width * 0.18f, a.Top + a.Height * 0.14f,
                a.Right - a.Width * 0.18f, a.Bottom - a.Height * 0.42f);
            float wx = a.Right - a.Width * 0.18f, wy = a.Bottom - a.Height * 0.4f;
            using (var cup = new SolidBrush(Color.FromArgb(182, 150, 92)))
                g.FillEllipse(cup, wx - 12f, wy - 6f, 24f, 12f);
            g.DrawEllipse(new Pen(Color.FromArgb(100, 72, 38), 1.2f), wx - 12f, wy - 6f, 24f, 12f);
            Sparks(g, wx - 10f, wy + 4f, (float)Math.PI * 1.1f, a.Width * 0.08f, 6);
        }
    }
}