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
    /// 点胶阀样式：Classic 为气动提升式点胶阀，其余 21 种覆盖时间压力/螺杆/喷射/
    /// 压电、喷雾/隔膜/蠕动/齿轮计量、热熔/双液/UV、手动针筒与三轴平台等。
    /// </summary>
    public enum HDispenseValveStyle
    {
        Classic = 0,       // 气动点胶阀
        TimePressure = 1,  // 时间压力针筒
        Auger = 2,         // 螺杆阀
        Jet = 3,           // 喷射阀
        Piezo = 4,         // 压电喷射阀
        Pneumatic = 5,     // 大气缸提升阀
        Needle = 6,        // 针座式点胶阀
        Spray = 7,         // 喷雾阀
        Diaphragm = 8,     // 隔膜计量阀
        Peristaltic = 9,   // 蠕动泵阀
        GearPump = 10,     // 齿轮计量泵
        ScrewPump = 11,    // 单螺杆泵
        Rotary = 12,       // 旋转分配阀
        Pinch = 13,        // 夹管阀
        HotMelt = 14,      // 热熔胶加热阀
        UV = 15,           // UV 遮光针筒
        TwoComponent = 16, // 双液混胶阀
        Cartridge = 17,    // 手动胶枪
        SyringeManual = 18,// 手动针筒
        MicroJet = 19,     // 微型压电阀
        Conformal = 20,    // 扇形喷涂阀
        RobotStage = 21    // 三轴点胶平台
    }
    /// <summary>
    /// 点胶阀控件（继承 HToolAnimBase）：22 种流体分配外形、13 种色调，
    /// Running 时按工艺显示积胶、滴落、喷射或雾化动画；胶液颜色可自定义。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("点胶阀控件：22 种点胶/喷涂外形、13 种色调，运行时显示积胶滴落与喷射动画")]
    public class HDispenseValve : HToolAnimBase
    {
        private HDispenseValveStyle _style = HDispenseValveStyle.Classic;
        private Color _glueColor = Color.FromArgb(240, 184, 44);
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HDispenseValve()
        {
            Size = new Size(94, 172);
        }
        /// <summary>点胶阀外形样式。</summary>
        [HCategoryLanguage("点胶阀"), HDisplayNameLanguage("点胶阀外形样式"), HDescriptionLanguage("点胶阀外形样式"), Browsable(true)]
        [DefaultValue(HDispenseValveStyle.Classic)]
        public HDispenseValveStyle DispenseValveStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>胶液颜色。</summary>
        [HCategoryLanguage("点胶阀"), HDisplayNameLanguage("胶液颜色"), HDescriptionLanguage("胶液颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "240, 184, 44")]
        public Color GlueColor
        {
            get => _glueColor;
            set { _glueColor = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HDispenseValveStyle.TimePressure: DrawSyringe(g, a, pal, false, false); break;
                case HDispenseValveStyle.Auger: DrawAuger(g, a, pal); break;
                case HDispenseValveStyle.Jet: DrawJet(g, a, pal, false); break;
                case HDispenseValveStyle.Piezo: DrawPiezo(g, a, pal); break;
                case HDispenseValveStyle.Pneumatic: DrawPneumatic(g, a, pal); break;
                case HDispenseValveStyle.Needle: DrawNeedleSeat(g, a, pal); break;
                case HDispenseValveStyle.Spray: DrawSpray(g, a, pal, 0); break;
                case HDispenseValveStyle.Conformal: DrawSpray(g, a, pal, 1); break;
                case HDispenseValveStyle.Diaphragm: DrawDiaphragm(g, a, pal); break;
                case HDispenseValveStyle.Peristaltic: DrawPeristaltic(g, a, pal); break;
                case HDispenseValveStyle.GearPump: DrawGearPump(g, a, pal); break;
                case HDispenseValveStyle.ScrewPump: DrawScrewPump(g, a, pal); break;
                case HDispenseValveStyle.Rotary: DrawRotary(g, a, pal); break;
                case HDispenseValveStyle.Pinch: DrawPinch(g, a, pal); break;
                case HDispenseValveStyle.HotMelt: DrawHotMelt(g, a, pal); break;
                case HDispenseValveStyle.UV: DrawSyringe(g, a, pal, true, false); break;
                case HDispenseValveStyle.TwoComponent: DrawTwoComponent(g, a, pal); break;
                case HDispenseValveStyle.Cartridge: DrawCartridge(g, a, pal); break;
                case HDispenseValveStyle.SyringeManual: DrawSyringe(g, a, pal, false, true); break;
                case HDispenseValveStyle.MicroJet: DrawJet(g, a, pal, true); break;
                case HDispenseValveStyle.RobotStage: DrawRobotStage(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>针口积胶 + 周期性滴落。</summary>
        private void GlueDrop(Graphics g, float cx, float tipY, float range)
        {
            using (var glue = new SolidBrush(_glueColor))
            {
                if (Running)
                {
                    float grow = Phase / (float)Math.PI * 0.5f % 1f;
                    float d = 3f + grow * 5f;
                    g.FillEllipse(glue, cx - d / 2f, tipY - 1f, d, d);
                    if (grow > 0.55f)
                    {
                        float fall = (grow - 0.55f) / 0.45f;
                        float dd = 5.5f - fall * 1.6f;
                        float dy = tipY + 3f + fall * range;
                        g.FillEllipse(glue, cx - dd / 2f, dy, dd, dd * 1.25f);
                    }
                }
                else
                    g.FillEllipse(glue, cx - 2.2f, tipY - 1f, 4.4f, 5f);
            }
        }
        // ----------------------------------------------------------------
        // Classic：气动提升式点胶阀（原验收外形）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width, glyphH = a.Height;
            float cx = Width / 2f;
            float w = Width * 0.42f;
            // 气缸
            var cyl = new RectangleF(cx - w / 2f, glyphH * 0.06f, w, glyphH * 0.34f);
            using (var gp = HBarBase.RoundPath(cyl, 5f))
            using (var b = HBarBase.CylinderH(cyl, HToolPalettes.Shade(pal.Main, 0.28f),
                HToolPalettes.Tint(pal.Main, 0.3f)))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.4f), gp);
            }
            // 进气口
            using (var port = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(port, cx - 3f, cyl.Y - 7f, 6f, 8f);
                g.FillRectangle(port, cyl.Right - 2f, cyl.Y + cyl.Height * 0.2f, 7f, 6f);
            }
            // 活塞杆
            g.DrawLine(new Pen(Color.FromArgb(80, 84, 90), 3f), cx, cyl.Bottom, cx, cyl.Bottom + glyphH * 0.06f);
            // 阀身（六角螺母观感）
            var nut = new RectangleF(cx - w * 0.62f, cyl.Bottom + glyphH * 0.05f, w * 1.24f, glyphH * 0.12f);
            using (var nb = HBarBase.CylinderH(nut, pal.MetalDark, Color.White))
                g.FillRectangle(nb, nut);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), nut.X, nut.Y, nut.Width, nut.Height);
            // 鲁尔接头
            var luer1 = new RectangleF(cx - w * 0.3f, nut.Bottom, w * 0.6f, glyphH * 0.07f);
            var luer2 = new RectangleF(cx - w * 0.18f, luer1.Bottom, w * 0.36f, glyphH * 0.05f);
            using (var lb = new SolidBrush(pal.Metal))
            {
                g.FillRectangle(lb, luer1);
                g.FillRectangle(lb, luer2);
            }
            g.DrawRectangle(new Pen(pal.Edge, 1.1f), luer1.X, luer1.Y, luer1.Width, luer1.Height);
            // 针头
            float tipY = luer2.Bottom + glyphH * 0.2f;
            using (var needle = new Pen(Color.FromArgb(90, 94, 100), 2.2f))
                g.DrawLine(needle, cx, luer2.Bottom, cx, tipY);
            GlueDrop(g, cx, tipY, glyphH * 0.1f);
            // 运行状态灯
            using (var lamp = new SolidBrush(Running ? Color.FromArgb(80, 210, 120) : Color.FromArgb(210, 60, 55)))
                g.FillEllipse(lamp, cyl.X + 5f, cyl.Y + 5f, 6f, 6f);
            g.Restore(st0);
        }
        /// <summary>通用：针筒（barrel 半透明料筒 + 活塞 + 接头 + 针）。</summary>
        private float SyringeBody(Graphics g, float cx, float top, float w, float h,
            HToolPalette pal, Color barrelTint, bool manual)
        {
            // 料筒（圆柱收锥）
            var body = new RectangleF(cx - w / 2f, top, w, h * 0.76f);
            using (var barrel = new SolidBrush(Color.FromArgb(60, barrelTint)))
                g.FillRectangle(barrel, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), body.X, body.Y, body.Width, body.Height);
            // 胶液
            float fill = Running ? 0.62f : 0.72f;
            using (var glue = new SolidBrush(Color.FromArgb(150, _glueColor)))
                g.FillRectangle(glue, body.X + 1.5f, body.Bottom - body.Height * fill,
                    body.Width - 3f, body.Height * fill - 1.5f);
            // 刻度
            using (var tick = new Pen(Color.FromArgb(150, pal.Edge), 0.8f))
                for (int i = 1; i < 5; i++)
                    g.DrawLine(tick, body.Right - 4f, body.Y + body.Height * i / 5f,
                        body.Right, body.Y + body.Height * i / 5f);
            // 活塞
            if (manual)
            {
                g.FillRectangle(new SolidBrush(pal.MetalDark), cx - 1.5f, top - h * 0.18f, 3f, h * 0.18f);
                g.FillRectangle(new SolidBrush(HToolPalettes.Shade(pal.Main, 0.1f)),
                    cx - w * 0.5f, top - h * 0.2f, w, 5f);
                // 两翼
                g.Line(pal.MetalDark, 2.6f, cx - w * 0.42f, body.Y, cx - w * 0.66f, body.Y + 6f);
                g.Line(pal.MetalDark, 2.6f, cx + w * 0.42f, body.Y, cx + w * 0.66f, body.Y + 6f);
            }
            // 锥头 + 接头 + 针
            float cy = body.Bottom;
            g.Poly(new[]
            {
                new PointF(cx - w * 0.3f, cy), new PointF(cx + w * 0.3f, cy),
                new PointF(cx + w * 0.1f, cy + h * 0.1f), new PointF(cx - w * 0.1f, cy + h * 0.1f)
            }, Color.FromArgb(70, barrelTint), pal.Edge, 1.1f);
            float tipY = cy + h * 0.1f + h * 0.16f;
            using (var nd = new Pen(Color.FromArgb(90, 94, 100), 1.8f))
                g.DrawLine(nd, cx, cy + h * 0.1f, cx, tipY);
            return tipY;
        }
        // ----------------------------------------------------------------
        // TimePressure / SyringeManual / UV：针筒式
        // ----------------------------------------------------------------
        private void DrawSyringe(Graphics g, RectangleF a, HToolPalette pal, bool uv, bool manual)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + (manual ? a.Height * 0.16f : a.Height * 0.12f);
            float w = a.Width * 0.34f, h = a.Height * (manual ? 0.7f : 0.76f);
            float tipY = SyringeBody(g, cx, top, w, h, pal,
                uv ? Color.FromArgb(120, 80, 30) : Color.FromArgb(200, 208, 218), manual);
            if (uv)
            {
                // 遮光筒（琥珀色罩）
                using (var cov = new SolidBrush(Color.FromArgb(150, 92, 52, 18)))
                    g.FillRectangle(cov, cx - w / 2f - 1f, top, w + 2f, h * 0.76f);
                // 气压接头
                g.Box(new RectangleF(cx - 4f, top - 7f, 8f, 7f), 1.5f,
                    pal.MetalDark, pal.Edge, 1f);
            }
            else if (!manual)
            {
                // 气压接头
                g.Box(new RectangleF(cx - 4f, top - 7f, 8f, 7f), 1.5f,
                    pal.MetalDark, pal.Edge, 1f);
            }
            GlueDrop(g, cx, tipY, a.Height * 0.08f);
            g.Lamp(cx + w / 2f - 6f, top + 6f, Running);
        }
        // ----------------------------------------------------------------
        // Auger：螺杆阀（顶置电机 + 螺杆腔 + 针）
        // ----------------------------------------------------------------
        private void DrawAuger(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.44f;
            // 电机
            var motor = new RectangleF(cx - w * 0.44f, a.Top + a.Height * 0.05f, w * 0.88f, a.Height * 0.16f);
            g.FillCylH(motor, HToolPalettes.Shade(pal.Main, 0.26f), HToolPalettes.Tint(pal.Main, 0.24f));
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), motor.X, motor.Y, motor.Width, motor.Height);
            // 进料斗
            g.Poly(new[]
            {
                new PointF(cx - w * 0.7f, motor.Bottom + 2f),
                new PointF(cx - w * 0.16f, motor.Bottom + 2f),
                new PointF(cx - w * 0.24f, motor.Bottom + 14f),
                new PointF(cx - w * 0.62f, motor.Bottom + 14f)
            }, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.1f);
            // 螺杆腔
            var ch = new RectangleF(cx - w * 0.24f, motor.Bottom + 12f, w * 0.48f, a.Height * 0.42f);
            g.Box(ch, 3f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            // 螺杆（旋转螺旋）
            var st = g.Save();
            g.TranslateTransform(cx, ch.Y);
            if (Running) g.TranslateTransform(0f, (Phase / (float)Math.PI * 0.5f % 1f) * 12f);
            using (var flight = new Pen(pal.Metal, 2.2f))
                for (float yy = -12f; yy < ch.Height + 12f; yy += 9f)
                {
                    g.DrawLine(flight, -w * 0.16f, yy, w * 0.16f, yy + 4.5f);
                    g.DrawLine(flight, w * 0.16f, yy + 4.5f, -w * 0.16f, yy + 9f);
                }
            g.Restore(st);
            // 接头 + 针
            float tipY = ch.Bottom + a.Height * 0.18f;
            g.Box(new RectangleF(cx - 5f, ch.Bottom, 10f, 7f), 1.5f, pal.Metal, pal.Edge, 1f);
            g.Line(Color.FromArgb(90, 94, 100), 2f, cx, ch.Bottom + 7f, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.08f);
        }
        // ----------------------------------------------------------------
        // Jet / MicroJet：喷射阀（方块阀体 + 喷嘴 + 水平高速胶线）
        // ----------------------------------------------------------------
        private void DrawJet(Graphics g, RectangleF a, HToolPalette pal, bool micro)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * (micro ? 0.5f : 0.56f);
            float top = a.Top + a.Height * 0.12f;
            var body = new RectangleF(cx - w / 2f, top, w, a.Height * 0.5f);
            g.Box(body, 4f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.4f);
            // 撞针电磁阀（顶柱 + 线圈）
            g.Box(new RectangleF(cx - 8f, top - 12f, 16f, 12f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            g.Line(pal.Metal, 2.4f, cx, top - 12f, cx, top - 2f);
            // 流体接口
            g.Line(pal.MetalDark, 2.6f, body.Left - 7f, body.Y + body.Height * 0.3f,
                body.Left, body.Y + body.Height * 0.3f);
            // 喷嘴（锥形朝下）
            float ny = body.Bottom;
            g.Poly(new[]
            {
                new PointF(cx - 8f, ny), new PointF(cx + 8f, ny),
                new PointF(cx + 2.4f, ny + 14f), new PointF(cx - 2.4f, ny + 14f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 基板
            float boardY = a.Bottom - a.Height * 0.14f;
            g.Box(new RectangleF(a.Left + a.Width * 0.14f, boardY, a.Width * 0.72f, 7f), 1.5f,
                Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            // 高速喷射胶滴流
            using (var glue = new SolidBrush(_glueColor))
            {
                if (Running)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.2f) % 1f;
                        float yy = ny + 16f + t * (boardY - ny - 18f);
                        float d = micro ? 1.8f : 2.6f;
                        g.FillEllipse(glue, cx - d / 2f, yy, d, d * 1.3f);
                    }
                }
                else
                    g.FillEllipse(glue, cx - 2f, ny + 14f, 4f, 4f);
                // 板上胶点
                g.FillEllipse(glue, cx - 3.2f, boardY - 2.4f, 6.4f, 3.6f);
            }
            g.Lamp(body.Right - 10f, body.Y + 6f, Running);
        }
        // ----------------------------------------------------------------
        // Piezo：压电叠堆喷射阀
        // ----------------------------------------------------------------
        private void DrawPiezo(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.4f;
            float top = a.Top + a.Height * 0.1f;
            // 压电叠堆（薄片堆叠）
            var stack = new RectangleF(cx - w / 2f, top, w, a.Height * 0.42f);
            g.Box(stack, 3f, Color.FromArgb(200, 170, 90), pal.Edge, 1.3f);
            using (var layer = new Pen(Color.FromArgb(140, 100, 50), 1f))
                for (int i = 1; i < 10; i++)
                    g.DrawLine(layer, stack.X, stack.Y + stack.Height * i / 10f,
                        stack.Right, stack.Y + stack.Height * i / 10f);
            // 电极线
            g.Line(Color.FromArgb(180, 60, 50), 1.6f, stack.Left - 5f, stack.Y + 4f,
                stack.Left, stack.Y + 4f);
            g.Line(Color.FromArgb(60, 90, 180), 1.6f, stack.Right + 5f, stack.Bottom - 4f,
                stack.Right, stack.Bottom - 4f);
            // 撞针 + 喷嘴
            float vib = Running ? (float)Math.Sin(Phase * 6f) * 1.6f : 0f;
            g.Line(pal.Metal, 2.2f, cx + vib, stack.Bottom, cx + vib, stack.Bottom + 16f);
            g.Poly(new[]
            {
                new PointF(cx - 7f, stack.Bottom + 14f),
                new PointF(cx + 7f, stack.Bottom + 14f),
                new PointF(cx + 2f, stack.Bottom + 26f),
                new PointF(cx - 2f, stack.Bottom + 26f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            float boardY = a.Bottom - a.Height * 0.12f;
            g.Box(new RectangleF(a.Left + a.Width * 0.16f, boardY, a.Width * 0.68f, 6f), 1.5f,
                Color.FromArgb(70, 130, 90), pal.Edge, 1f);
            using (var glue = new SolidBrush(_glueColor))
            {
                if (Running)
                    for (int i = 0; i < 4; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.25f) % 1f;
                        g.FillEllipse(glue, cx - 1.4f, stack.Bottom + 28f + t * (boardY - stack.Bottom - 30f),
                            2.8f, 3.4f);
                    }
                g.FillEllipse(glue, cx - 2.6f, boardY - 2f, 5.2f, 3f);
            }
        }
        // ----------------------------------------------------------------
        // Pneumatic：大气缸提升阀（柱塞式）
        // ----------------------------------------------------------------
        private void DrawPneumatic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.52f;
            var cyl = new RectangleF(cx - w / 2f, a.Top + a.Height * 0.06f, w, a.Height * 0.4f);
            g.Box(cyl, 5f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.4f);
            using (var port = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(port, cx - 3f, cyl.Y - 7f, 6f, 8f);
                g.FillRectangle(port, cyl.Right - 2f, cyl.Y + 8f, 7f, 6f);
            }
            // 阀针（运行时下探）
            float probe = Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) * a.Height * 0.08f : 0f;
            // 阀体 + 腔室
            var chamber = new RectangleF(cx - w * 0.42f, cyl.Bottom + 8f, w * 0.84f, a.Height * 0.22f);
            g.Box(chamber, 3f, HToolPalettes.Tint(pal.Main, 0.16f), pal.Edge, 1.3f);
            g.Line(Color.FromArgb(80, 84, 90), 3.4f, cx, cyl.Bottom, cx, chamber.Bottom - 4f + probe);
            // 胶液腔
            using (var glue = new SolidBrush(Color.FromArgb(140, _glueColor)))
                g.FillRectangle(glue, chamber.X + 3f, chamber.Bottom - 12f, chamber.Width - 6f, 9f);
            float tipY = chamber.Bottom + a.Height * 0.14f;
            g.Poly(new[]
            {
                new PointF(cx - 6f, chamber.Bottom), new PointF(cx + 6f, chamber.Bottom),
                new PointF(cx + 2f, chamber.Bottom + 8f), new PointF(cx - 2f, chamber.Bottom + 8f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            g.Line(Color.FromArgb(90, 94, 100), 2f, cx, chamber.Bottom + 8f, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.08f);
        }
        // ----------------------------------------------------------------
        // NeedleSeat：细长针座式点胶阀
        // ----------------------------------------------------------------
        private void DrawNeedleSeat(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.3f;
            // 细长阀杆
            var body = new RectangleF(cx - w / 2f, a.Top + a.Height * 0.08f, w, a.Height * 0.56f);
            g.Box(body, 3f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.3f);
            // 调节旋钮
            g.Box(new RectangleF(cx - w * 0.7f, a.Top + a.Height * 0.04f, w * 1.4f, a.Height * 0.06f),
                2f, pal.MetalDark, pal.Edge, 1.1f);
            // 锁紧螺母
            g.Box(new RectangleF(cx - w * 0.66f, body.Bottom - a.Height * 0.06f, w * 1.32f, a.Height * 0.06f),
                2f, pal.Metal, pal.Edge, 1.1f);
            // 弯形针嘴（90°）
            float by = body.Bottom + a.Height * 0.14f;
            using (var nd = new Pen(Color.FromArgb(90, 94, 100), 2.2f))
            {
                g.DrawLine(nd, cx, body.Bottom, cx, by);
                g.DrawLine(nd, cx, by, cx + a.Width * 0.18f, by);
            }
            float tipX = cx + a.Width * 0.18f;
            using (var glue = new SolidBrush(_glueColor))
            {
                if (Running)
                {
                    float grow = Phase / (float)Math.PI * 0.5f % 1f;
                    g.FillEllipse(glue, tipX - 1.5f - grow * 1.5f, by - 2f - grow, 3f + grow * 3f,
                        4f + grow * 2f);
                }
                else g.FillEllipse(glue, tipX - 2f, by - 2f, 4f, 4f);
            }
            // 进料管
            g.Line(pal.MetalDark, 2.4f, body.Left - 6f, body.Y + 10f, body.Left, body.Y + 10f);
            g.Lamp(body.Right - 8f, body.Y + 6f, Running);
        }
        // ----------------------------------------------------------------
        // Spray / Conformal：喷雾阀（锥形雾化）
        // ----------------------------------------------------------------
        private void DrawSpray(Graphics g, RectangleF a, HToolPalette pal, int fan)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.46f;
            // 阀体
            var body = new RectangleF(cx - w / 2f, a.Top + a.Height * 0.08f, w, a.Height * 0.34f);
            g.Box(body, 4f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.3f);
            g.Box(new RectangleF(cx - 6f, a.Top + a.Height * 0.03f, 12f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1f);
            // 气液接口
            g.Line(pal.MetalDark, 2.4f, body.Left - 6f, body.Y + 10f, body.Left, body.Y + 10f);
            g.Line(pal.MetalDark, 2.4f, body.Right - 6f, body.Bottom - 10f, body.Right, body.Bottom - 10f);
            // 雾化喷嘴
            float ny = body.Bottom;
            g.Poly(new[]
            {
                new PointF(cx - 7f, ny), new PointF(cx + 7f, ny),
                new PointF(cx + 3f, ny + 12f), new PointF(cx - 3f, ny + 12f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            // 工件
            float wy = a.Bottom - a.Height * 0.14f;
            if (fan == 1)
                // PCB
                g.Box(new RectangleF(a.Left + 6f, wy, a.Width - 12f, a.Height * 0.08f), 2f,
                    Color.FromArgb(60, 120, 80), pal.Edge, 1.1f);
            else
                g.Box(new RectangleF(a.Left + a.Width * 0.16f, wy, a.Width * 0.68f, 7f), 2f,
                    HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.1f);
            // 锥形/扇形雾化
            if (Running)
            {
                float half = fan == 1 ? a.Width * 0.36f : a.Width * 0.2f;
                using (var mist = new SolidBrush(Color.FromArgb(40, _glueColor)))
                {
                    var cone = new GraphicsPath();
                    cone.AddLine(cx, ny + 12f, cx - half, wy);
                    cone.AddLine(cx + half, wy, cx, ny + 12f);
                    cone.CloseFigure();
                    g.FillPath(mist, cone);
                }
                using (var dot = new SolidBrush(Color.FromArgb(170, _glueColor)))
                    for (int i = 0; i < 8; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.13f) % 1f;
                        float spread = (i % 3 - 1) * half * t;
                        float px = cx + spread + (float)Math.Sin(Phase * 3f + i) * 2f;
                        float py = ny + 14f + t * (wy - ny - 16f);
                        g.FillEllipse(dot, px - 1.4f, py - 1.4f, 2.8f, 2.8f);
                    }
            }
        }
        // ----------------------------------------------------------------
        // Diaphragm：隔膜计量泵阀
        // ----------------------------------------------------------------
        private void DrawDiaphragm(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            // 泵体（上下两瓣夹膜片）
            var top = new RectangleF(cx - a.Width * 0.3f, a.Top + a.Height * 0.16f,
                a.Width * 0.6f, a.Height * 0.2f);
            var bot = new RectangleF(cx - a.Width * 0.3f, a.Top + a.Height * 0.42f,
                a.Width * 0.6f, a.Height * 0.2f);
            g.Box(top, 6f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            g.Box(bot, 6f, HToolPalettes.Tint(pal.Main, 0.14f), pal.Edge, 1.3f);
            // 膜片（鼓动）
            float bulge = Running ? (float)Math.Sin(Phase * 2f) * 4f : 0f;
            using (var dia = new SolidBrush(Color.FromArgb(120, 60, 64, 70)))
                g.FillEllipse(dia, top.X, top.Bottom - 6f - bulge, top.Width, 12f + bulge * 2f);
            // 连杆 + 偏心驱动
            g.Line(pal.Metal, 2.6f, cx, top.Y - 8f, cx, top.Bottom - 4f);
            g.Ell(cx, top.Y - 12f, 5f, 5f, pal.MetalDark);
            // 胶腔 + 单向阀 + 管路 + 针
            using (var glue = new SolidBrush(Color.FromArgb(150, _glueColor)))
                g.FillRectangle(glue, bot.X + 4f, bot.Bottom - 12f, bot.Width - 8f, 8f);
            g.Line(pal.MetalDark, 2.6f, bot.Left - 8f, bot.Y + 10f, bot.Left, bot.Y + 10f);
            float tipY = bot.Bottom + a.Height * 0.16f;
            g.Line(Color.FromArgb(90, 94, 100), 2f, cx, bot.Bottom, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.07f);
        }
        // ----------------------------------------------------------------
        // Peristaltic：蠕动泵（滚轮挤压软管）
        // ----------------------------------------------------------------
        private void DrawPeristaltic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.42f;
            float r = a.Width * 0.34f;
            // 泵壳
            g.Ell(cx, cy, r, r, HToolPalettes.Shade(pal.Main, 0.12f));
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - r, cy - r, r * 2f, r * 2f);
            // U 形软管（内贴壳壁）
            using (var tube = new Pen(Color.FromArgb(180, 200, 210), 4.4f))
                g.DrawArc(tube, cx - r * 0.78f, cy - r * 0.78f, r * 1.56f, r * 1.56f, 200f, 140f);
            using (var glueTube = new Pen(Color.FromArgb(150, _glueColor), 2f))
                g.DrawArc(glueTube, cx - r * 0.78f, cy - r * 0.78f, r * 1.56f, r * 1.56f, 200f, 140f);
            // 旋转三滚轮
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 0f);
            using (var roller = new SolidBrush(pal.MetalDark))
                for (int i = 0; i < 3; i++)
                {
                    g.RotateTransform(120f);
                    g.FillEllipse(roller, r * 0.6f - 5f, -5f, 10f, 10f);
                }
            g.Restore(st);
            g.Ell(cx, cy, 5f, 5f, pal.Metal);
            // 软管出口 + 针
            float tipY = a.Bottom - a.Height * 0.08f;
            g.Line(Color.FromArgb(180, 200, 210), 3f, cx + r * 0.6f, cy + r * 0.5f, cx, tipY - 10f);
            g.Line(Color.FromArgb(90, 94, 100), 2f, cx, tipY - 10f, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.06f);
        }
        // ----------------------------------------------------------------
        // GearPump：齿轮计量泵 + 喷嘴
        // ----------------------------------------------------------------
        private void DrawGearPump(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.34f;
            float gr = a.Width * 0.2f;
            // 泵体
            var body = new RectangleF(cx - gr * 1.7f, cy - gr * 1.2f, gr * 3.4f, gr * 2.4f);
            g.Box(body, 4f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.3f);
            // 啮合双齿轮
            foreach (int s in new[] { -1, 1 })
            {
                var st = g.Save();
                g.TranslateTransform(cx + s * gr * 0.72f, cy);
                g.RotateTransform(Running ? SpinAngle * s : 0f);
                using (var gb = new SolidBrush(pal.Metal))
                using (var gp = new Pen(pal.Edge, 1f))
                {
                    g.FillEllipse(gb, -gr * 0.6f, -gr * 0.6f, gr * 1.2f, gr * 1.2f);
                    for (int i = 0; i < 8; i++)
                    {
                        g.RotateTransform(45f);
                        g.FillRectangle(gb, -2f, -gr * 0.72f, 4f, 5f);
                    }
                    g.DrawEllipse(gp, -gr * 0.6f, -gr * 0.6f, gr * 1.2f, gr * 1.2f);
                }
                g.Restore(st);
            }
            // 驱动轴 + 进出管 + 喷嘴
            g.Line(pal.MetalDark, 2.6f, cx, body.Top - 8f, cx, body.Top);
            g.Line(pal.MetalDark, 2.6f, body.Left - 7f, cy, body.Left, cy);
            float tipY = a.Bottom - a.Height * 0.12f;
            g.Poly(new[]
            {
                new PointF(cx - 6f, body.Bottom), new PointF(cx + 6f, body.Bottom),
                new PointF(cx + 2f, body.Bottom + 10f), new PointF(cx - 2f, body.Bottom + 10f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            g.Line(Color.FromArgb(90, 94, 100), 2f, cx, body.Bottom + 10f, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.07f);
        }
        // ----------------------------------------------------------------
        // ScrewPump：单螺杆（莫诺）泵
        // ----------------------------------------------------------------
        private void DrawScrewPump(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.36f;
            var body = new RectangleF(cx - w / 2f, a.Top + a.Height * 0.14f, w, a.Height * 0.5f);
            g.Box(body, 8f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.3f);
            // 定子内胶液
            using (var glue = new SolidBrush(Color.FromArgb(120, _glueColor)))
                g.FillRectangle(glue, body.X + 3f, body.Y + 6f, body.Width - 6f, body.Height - 12f);
            // 转子螺旋（旋转推进）
            var st = g.Save();
            g.TranslateTransform(cx, body.Y);
            if (Running) g.TranslateTransform(0f, (Phase / (float)Math.PI * 0.5f % 1f) * 14f);
            using (var rotor = new Pen(pal.Metal, 2.6f))
                for (float yy = -14f; yy < body.Height + 14f; yy += 11f)
                {
                    g.DrawLine(rotor, -w * 0.28f, yy, 0f, yy + 5.5f);
                    g.DrawLine(rotor, 0f, yy + 5.5f, w * 0.28f, yy + 11f);
                }
            g.Restore(st);
            // 驱动 + 料口 + 出口
            g.Box(new RectangleF(cx - 10f, body.Top - 12f, 20f, 12f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            g.Line(pal.MetalDark, 2.6f, body.Left - 7f, body.Y + 10f, body.Left, body.Y + 10f);
            float tipY = body.Bottom + a.Height * 0.16f;
            g.Line(Color.FromArgb(90, 94, 100), 2.2f, cx, body.Bottom, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.08f);
        }
        // ----------------------------------------------------------------
        // Rotary：旋转分配阀（多工位转盘 + 阀针）
        // ----------------------------------------------------------------
        private void DrawRotary(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.46f;
            float r = a.Width * 0.34f;
            // 定子盘 + 转子（步进）
            g.Ell(cx, cy, r, r, HToolPalettes.Shade(pal.Main, 0.12f));
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - r, cy - r, r * 2f, r * 2f);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? (float)(Math.Round(SpinAngle / 60f) * 60f) : 0f);
            using (var port = new SolidBrush(Color.FromArgb(70, _glueColor)))
                for (int i = 0; i < 6; i++)
                {
                    g.RotateTransform(60f);
                    g.FillEllipse(port, r * 0.62f - 3f, -3f, 6f, 6f);
                }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.16f, r * 0.16f, pal.MetalDark);
            // 固定进料 + 出料针
            g.Line(pal.MetalDark, 2.6f, cx, a.Top + 8f, cx, cy - r);
            float tipY = a.Bottom - a.Height * 0.1f;
            g.Line(Color.FromArgb(90, 94, 100), 2f, cx, cy + r, cx, tipY);
            GlueDrop(g, cx, tipY, a.Height * 0.06f);
        }
        // ----------------------------------------------------------------
        // Pinch：夹管阀（夹爪挤压软管）
        // ----------------------------------------------------------------
        private void DrawPinch(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            // 阀体
            var body = new RectangleF(cx - a.Width * 0.3f, a.Top + a.Height * 0.16f,
                a.Width * 0.6f, a.Height * 0.4f);
            g.Box(body, 6f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.3f);
            // 软管（穿体而过，中部被夹）
            float pinch = Running ? 0.7f + (float)(Math.Sin(Phase) * 0.5 + 0.5) * 0.28f : 0.96f;
            using (var tube = new Pen(Color.FromArgb(190, 206, 216), 5f))
            {
                g.DrawLine(tube, cx, body.Top - 12f, cx, body.Y + body.Height * pinch);
                g.DrawLine(tube, cx, body.Bottom - body.Height * pinch + 4f, cx, body.Bottom + 14f);
            }
            using (var glueTube = new Pen(Color.FromArgb(150, _glueColor), 2.2f))
            {
                g.DrawLine(glueTube, cx, body.Top - 12f, cx, body.Y + body.Height * pinch - 2f);
                g.DrawLine(glueTube, cx, body.Bottom - body.Height * pinch + 6f, cx, body.Bottom + 14f);
            }
            // 夹爪（气动下压）
            float jy = body.Y + body.Height * pinch - 4f;
            g.Box(new RectangleF(cx - 10f, jy - 6f, 20f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Line(pal.Metal, 3f, cx, body.Top - 8f, cx, jy - 6f);
            // 气缸顶
            g.Box(new RectangleF(cx - 9f, body.Top - 20f, 18f, 12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.1f);
            float tipY = body.Bottom + a.Height * 0.16f;
            GlueDrop(g, cx, tipY, a.Height * 0.07f);
        }
        // ----------------------------------------------------------------
        // HotMelt：热熔胶加热阀
        // ----------------------------------------------------------------
        private void DrawHotMelt(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, w = a.Width * 0.42f;
            // 胶粒罐
            var hopper = new RectangleF(cx - w * 0.4f, a.Top + a.Height * 0.05f, w * 0.8f, a.Height * 0.14f);
            g.Box(hopper, 3f, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.2f);
            // 加热块（带温感辉光）
            var heat = new RectangleF(cx - w / 2f, hopper.Bottom + 6f, w, a.Height * 0.34f);
            int glow = Running ? (int)(40 + 30 * Math.Abs(Math.Sin(Phase * 2f))) : 0;
            using (var hb = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.16f)))
                g.FillRectangle(hb, heat);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), heat.X, heat.Y, heat.Width, heat.Height);
            if (glow > 0)
                using (var gl = new SolidBrush(Color.FromArgb(glow, 255, 130, 40)))
                    g.FillRectangle(gl, heat.X + 2f, heat.Y + 2f, heat.Width - 4f, heat.Height - 4f);
            // 散热鳍片
            for (int i = 0; i < 4; i++)
                g.Line(pal.MetalDark, 2f, heat.Right + 2f, heat.Y + 6f + i * 8f,
                    heat.Right + 9f, heat.Y + 6f + i * 8f);
            // 温度传感器
            g.Line(Color.FromArgb(200, 80, 60), 1.8f, heat.Left - 6f, heat.Y + 10f,
                heat.Left, heat.Y + 10f);
            // 加热喷嘴（短粗）
            float ny = heat.Bottom;
            g.Poly(new[]
            {
                new PointF(cx - 7f, ny), new PointF(cx + 7f, ny),
                new PointF(cx + 3f, ny + 12f), new PointF(cx - 3f, ny + 12f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            float tipY = ny + a.Height * 0.16f;
            g.Line(Color.FromArgb(90, 94, 100), 2.4f, cx, ny + 12f, cx, tipY);
            // 热熔胶滴（带发光感）
            using (var glue = new SolidBrush(_glueColor))
            {
                if (Running)
                {
                    float grow = Phase / (float)Math.PI * 0.5f % 1f;
                    float d = 3f + grow * 5f;
                    g.FillEllipse(glue, cx - d / 2f, tipY - 1f, d, d);
                    if (grow > 0.6f)
                    {
                        float fall = (grow - 0.6f) / 0.4f;
                        g.FillEllipse(glue, cx - 2.4f, tipY + 3f + fall * a.Height * 0.08f,
                            4.8f, 6f);
                    }
                }
                else g.FillEllipse(glue, cx - 2.2f, tipY - 1f, 4.4f, 5f);
            }
        }
        // ----------------------------------------------------------------
        // TwoComponent：双液混胶阀（双针筒 + 静态混合管）
        // ----------------------------------------------------------------
        private void DrawTwoComponent(Graphics g, RectangleF a, HToolPalette pal)
        {
            float w = a.Width * 0.26f;
            float x1 = a.Left + a.Width * 0.32f, x2 = a.Left + a.Width * 0.68f;
            float top = a.Top + a.Height * 0.08f, h = a.Height * 0.44f;
            // 双料筒
            foreach (float bx in new[] { x1, x2 })
            {
                var body = new RectangleF(bx - w / 2f, top, w, h * 0.78f);
                g.Box(body, 3f, Color.FromArgb(60, 200, 208, 218), pal.Edge, 1.1f);
            }
            // A/B 胶液
            using (var ga = new SolidBrush(Color.FromArgb(160, _glueColor)))
                g.FillRectangle(ga, x1 - w / 2f + 2f, top + h * 0.2f, w - 4f, h * 0.56f);
            using (var gb = new SolidBrush(Color.FromArgb(160, 220, 220, 224)))
                g.FillRectangle(gb, x2 - w / 2f + 2f, top + h * 0.14f, w - 4f, h * 0.62f);
            // 活塞气缸
            foreach (float bx in new[] { x1, x2 })
                g.Box(new RectangleF(bx - w * 0.42f, top - 10f, w * 0.84f, 10f), 2f,
                    HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1f);
            // 汇流块
            float my = top + h * 0.82f;
            g.Box(new RectangleF(x1 - 5f, my, x2 - x1 + 10f, 10f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            // 静态混合管（螺旋芯）
            var mix = new RectangleF(a.Left + a.Width * 0.36f, my + 10f,
                a.Width * 0.28f, a.Height * 0.22f);
            g.Box(mix, 3f, Color.FromArgb(150, 214, 220, 226), pal.Edge, 1.1f);
            var st = g.Save();
            g.TranslateTransform(mix.X, mix.Y);
            if (Running) g.TranslateTransform(0f, (Phase / (float)Math.PI * 0.5f % 1f) * 8f);
            for (float yy = -8f; yy < mix.Height + 8f; yy += 6f)
                g.DrawLine(new Pen(Color.FromArgb(150, _glueColor), 1.4f),
                    3f, yy, mix.Width - 3f, yy + 3f);
            g.Restore(st);
            // 针头
            float cxm = a.Left + a.Width / 2f, tipY = mix.Bottom + a.Height * 0.12f;
            g.Line(Color.FromArgb(90, 94, 100), 2f, cxm, mix.Bottom, cxm, tipY);
            GlueDrop(g, cxm, tipY, a.Height * 0.06f);
        }
        // ----------------------------------------------------------------
        // Cartridge：手动压胶枪
        // ----------------------------------------------------------------
        private void DrawCartridge(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            // 胶筒（斜置一点）
            var st = g.Save();
            g.TranslateTransform(cx, a.Top + a.Height * 0.36f);
            g.RotateTransform(-8f);
            float w = a.Width * 0.32f, h = a.Height * 0.42f;
            g.Box(new RectangleF(-w / 2f, -h / 2f, w, h * 0.82f), 3f,
                Color.FromArgb(150, _glueColor), pal.Edge, 1.1f);
            // 锥嘴
            g.Poly(new[]
            {
                new PointF(-w * 0.28f, h * 0.32f), new PointF(w * 0.28f, h * 0.32f),
                new PointF(w * 0.08f, h * 0.44f), new PointF(-w * 0.08f, h * 0.44f)
            }, Color.FromArgb(150, _glueColor), pal.Edge, 1f);
            // 推杆
            g.Line(pal.MetalDark, 2.4f, 0f, -h / 2f - 14f, 0f, h * 0.32f);
            g.Restore(st);
            // 枪架 + 扳机（运行时扣动）
            float trig = Running ? 3f : 0f;
            using (var frame = new Pen(HToolPalettes.Shade(pal.Main, 0.12f), 4f))
            {
                // 上部托梁
                g.DrawLine(frame, a.Left + a.Width * 0.18f, a.Top + a.Height * 0.2f,
                    a.Right - a.Width * 0.14f, a.Top + a.Height * 0.26f);
                // 握把
                g.DrawLine(frame, a.Left + a.Width * 0.3f, a.Top + a.Height * 0.3f,
                    a.Left + a.Width * 0.24f, a.Bottom - a.Height * 0.12f);
                // 扳机
                g.DrawLine(frame, a.Left + a.Width * 0.36f, a.Top + a.Height * 0.4f + trig,
                    a.Left + a.Width * 0.3f, a.Bottom - a.Height * 0.22f + trig);
            }
            // 胶滴
            float tipX = cx - a.Width * 0.07f, tipY = a.Top + a.Height * 0.62f;
            using (var glue = new SolidBrush(_glueColor))
                if (Running)
                {
                    float grow = Phase / (float)Math.PI * 0.5f % 1f;
                    g.FillEllipse(glue, tipX - 2f - grow, tipY - 1f + grow * 2f,
                        4f + grow * 2f, 5f + grow);
                }
                else g.FillEllipse(glue, tipX - 2f, tipY, 4f, 5f);
        }
        // ----------------------------------------------------------------
        // RobotStage：三轴点胶平台（龙门 + 横移点胶头）
        // ----------------------------------------------------------------
        private void DrawRobotStage(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 工作台
            float baseY = a.Bottom - a.Height * 0.16f;
            g.Box(new RectangleF(a.Left + 6f, baseY, a.Width - 12f, a.Height * 0.1f), 3f,
                HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.2f);
            // 龙门
            g.Line(pal.MetalDark, 3.6f, a.Left + 10f, baseY, a.Left + 10f, a.Top + a.Height * 0.12f);
            g.Line(pal.MetalDark, 3.6f, a.Right - 10f, baseY, a.Right - 10f, a.Top + a.Height * 0.12f);
            g.Line(pal.MetalDark, 4f, a.Left + 10f, a.Top + a.Height * 0.12f,
                a.Right - 10f, a.Top + a.Height * 0.12f);
            // 横移滑块（X 往复）
            float move = Running ? Phase / (float)Math.PI * 0.5f % 1f : 0.25f;
            float hx = a.Left + 16f + move * (a.Width - 32f);
            float hy = a.Top + a.Height * 0.12f;
            g.Box(new RectangleF(hx - 8f, hy - 4f, 16f, 10f), 2f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.1f);
            // Z 轴点胶阀（迷你针筒）
            float zDrop = Running ? a.Height * 0.04f + (float)Math.Abs(Math.Sin(Phase * 2f)) * a.Height * 0.04f
                : a.Height * 0.06f;
            g.Line(pal.Metal, 2.4f, hx, hy + 6f, hx, hy + 6f + zDrop);
            g.Box(new RectangleF(hx - 4f, hy + 6f + zDrop, 8f, a.Height * 0.12f), 1.5f,
                Color.FromArgb(150, 200, 208, 218), pal.Edge, 1f);
            float tipY = hy + 6f + zDrop + a.Height * 0.16f;
            g.Line(Color.FromArgb(90, 94, 100), 1.6f, hx, hy + 6f + zDrop + a.Height * 0.12f, hx, tipY);
            // 工件板 + 胶线路径（虚线）
            g.Box(new RectangleF(a.Left + a.Width * 0.2f, baseY - 5f, a.Width * 0.6f, 5f), 1f,
                Color.FromArgb(70, 130, 90), pal.Edge, 1f);
            using (var trace = new Pen(Color.FromArgb(170, _glueColor), 1.6f)
            { DashStyle = DashStyle.Dash })
                g.DrawLine(trace, a.Left + a.Width * 0.24f, baseY - 7f,
                    a.Right - a.Width * 0.24f, baseY - 7f);
            using (var glue = new SolidBrush(_glueColor))
                g.FillEllipse(glue, hx - 2f, tipY - 1f, 4f, 4f);
        }
    }
}