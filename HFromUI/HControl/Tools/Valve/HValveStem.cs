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
    /// 阀杆驱动样式：Classic 为 T 型手轮明杆阀（已验收外形），其余 21 种覆盖齿轮箱、
    /// 手柄、链轮、气动薄膜/活塞、电动/液动、电磁阀、智能定位器等工业阀门驱动外形。
    /// </summary>
    public enum HValveStemStyle
    {
        Classic = 0,            // T 型手轮明杆阀
        GearHandwheel = 1,      // 涡轮箱手轮
        LeverHandle = 2,        // 杠杆手柄
        ChainWheel = 3,         // 链轮操作
        Diaphragm = 4,          // 气动薄膜执行器
        Piston = 5,             // 气动活塞执行器
        RackPinion = 6,         // 齿轮齿条执行器
        ElectricMultiTurn = 7,  // 多回转电动头
        ElectricQuarterTurn = 8,// 角行程电动头
        Hydraulic = 9,          // 液压执行器
        ElectroHydraulic = 10,  // 电液联动执行器
        BevelGear = 11,         // 伞齿轮手轮
        TBar = 12,              // T 型扳手暗杆阀
        SquareNut = 13,         // 方帽扳手位
        Knob = 14,              // 旋钮手柄
        LockedLever = 15,       // 带锁手柄
        FootPedal = 16,         // 脚踏阀
        Solenoid = 17,          // 电磁线圈
        Thermal = 18,           // 热静力执行器
        MotorizedBall = 19,     // 电动球阀
        FailSafe = 20,          // 故障安全弹簧缸
        Positioner = 21         // 智能阀门定位器
    }
    /// <summary>
    /// 阀杆控件（继承 HToolAnimBase）：22 种阀门驱动外形、13 种色调，
    /// Open 为开度（0..100），阀杆/阀瓣位置与状态灯随开度变化。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("阀杆控件：22 种阀门驱动外形、13 种色调，Open 开度控制阀杆升降")]
    public class HValveStem : HToolAnimBase
    {
        private double _open = 100;
        private HValveStemStyle _style = HValveStemStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HValveStem()
        {
            Size = new Size(76, 172);
        }
        /// <summary>阀杆驱动外形样式。</summary>
        [HCategoryLanguage("阀杆"), HDisplayNameLanguage("阀杆驱动外形样式"), HDescriptionLanguage("阀杆驱动外形样式"), Browsable(true)]
        [DefaultValue(HValveStemStyle.Classic)]
        public HValveStemStyle ValveStemStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>阀门开度（0..100），增大时阀杆向上提升。</summary>
        [HCategoryLanguage("阀杆"), HDisplayNameLanguage("阀门开度"), HDescriptionLanguage("阀门开度，0..100，阀杆随开度上移"), Browsable(true)]
        [DefaultValue(100.0)]
        public double Open
        {
            get => _open;
            set { _open = Math.Max(0, Math.Min(100, value)); Invalidate(); }
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
                case HValveStemStyle.GearHandwheel: DrawGear(g, a, pal, false); break;
                case HValveStemStyle.BevelGear: DrawGear(g, a, pal, true); break;
                case HValveStemStyle.LeverHandle: DrawLever(g, a, pal, false); break;
                case HValveStemStyle.LockedLever: DrawLever(g, a, pal, true); break;
                case HValveStemStyle.ChainWheel: DrawChain(g, a, pal); break;
                case HValveStemStyle.Diaphragm: DrawDiaphragm(g, a, pal, false); break;
                case HValveStemStyle.FailSafe: DrawDiaphragm(g, a, pal, true); break;
                case HValveStemStyle.Piston: DrawPiston(g, a, pal); break;
                case HValveStemStyle.RackPinion: DrawRack(g, a, pal); break;
                case HValveStemStyle.ElectricMultiTurn: DrawElectric(g, a, pal, true); break;
                case HValveStemStyle.ElectricQuarterTurn: DrawElectric(g, a, pal, false); break;
                case HValveStemStyle.Hydraulic: DrawHydraulic(g, a, pal, false); break;
                case HValveStemStyle.ElectroHydraulic: DrawHydraulic(g, a, pal, true); break;
                case HValveStemStyle.TBar: DrawTBar(g, a, pal); break;
                case HValveStemStyle.SquareNut: DrawSquareNut(g, a, pal); break;
                case HValveStemStyle.Knob: DrawKnob(g, a, pal); break;
                case HValveStemStyle.FootPedal: DrawFoot(g, a, pal); break;
                case HValveStemStyle.Solenoid: DrawSolenoid(g, a, pal); break;
                case HValveStemStyle.Thermal: DrawThermal(g, a, pal); break;
                case HValveStemStyle.MotorizedBall: DrawMotorBall(g, a, pal); break;
                case HValveStemStyle.Positioner: DrawPositioner(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：T 型手轮 + 螺纹杆 + 压紧螺母 + 阀轭（已验收绘法，原样保留）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width, glyphH = a.Height;
            float cx = Width * 0.46f;
            float lift = (float)(_open / 100.0) * glyphH * 0.12f;
            float handleY = glyphH * 0.12f - lift;
            float nutY = glyphH * 0.44f;
            float plugY = glyphH * 0.82f;
            // 阀轭（桥形支架）
            using (var yoke = new Pen(pal.MetalDark, 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(yoke, cx - Width * 0.26f, plugY, cx - Width * 0.26f, nutY + glyphH * 0.06f);
                g.DrawLine(yoke, cx + Width * 0.26f, plugY, cx + Width * 0.26f, nutY + glyphH * 0.06f);
                g.DrawLine(yoke, cx - Width * 0.26f, nutY + glyphH * 0.06f,
                    cx + Width * 0.26f, nutY + glyphH * 0.06f);
            }
            // 底部法兰
            var flange = new RectangleF(cx - Width * 0.28f, plugY - 3f, Width * 0.56f, 9f);
            using (var fb = new SolidBrush(pal.MetalDark))
                g.FillRectangle(fb, flange);
            // 阀杆（随开度上移）
            float stemW = 4.5f;
            using (var stem = HBarBase.CylinderH(new RectangleF(cx - stemW / 2f, 0, stemW, 1f),
                pal.MetalDark, Color.White))
            {
                g.FillRectangle(stem, cx - stemW / 2f, handleY, stemW, plugY - handleY);
            }
            // 螺纹杆段（手轮到螺母之间的牙线）
            using (var thread = new Pen(HToolPalettes.Shade(pal.Metal, 0.35f), 1.2f))
            {
                for (float y = handleY + 5f; y < nutY - 2f; y += 4.5f)
                {
                    g.DrawLine(thread, cx - stemW / 2f - 1.5f, y, cx + stemW / 2f + 1.5f, y + 2.4f);
                    g.DrawLine(thread, cx + stemW / 2f + 1.5f, y, cx - stemW / 2f - 1.5f, y + 2.4f);
                }
            }
            // 阀瓣（杆下端）
            using (var plug = new SolidBrush(pal.Metal))
                g.FillEllipse(plug, cx - 7f, plugY - 3f, 14f, 9f);
            // 压紧螺母（六角）
            var nut = new RectangleF(cx - Width * 0.18f, nutY, Width * 0.36f, glyphH * 0.07f);
            using (var nb = HBarBase.CylinderH(nut, pal.MetalDark, pal.Metal))
                g.FillRectangle(nb, nut);
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), nut.X, nut.Y, nut.Width, nut.Height);
            // T 型手轮
            float barY = handleY - 2f;
            using (var bar = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 6f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(bar, cx - Width * 0.34f, barY, cx + Width * 0.34f, barY);
            using (var hub = new SolidBrush(pal.Accent))
                g.FillEllipse(hub, cx - 7f, barY - 7f, 14f, 14f);
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), cx - 7f, barY - 7f, 14f, 14f);
            // 开度刻度
            float sx = Width * 0.86f, sy0 = glyphH * 0.2f, sy1 = glyphH * 0.6f;
            using (var scale = new Pen(Color.Gray, 1.2f))
            {
                g.DrawLine(scale, sx, sy0, sx, sy1);
                for (int i = 0; i <= 4; i++)
                    g.DrawLine(scale, sx - 4f, sy1 - (sy1 - sy0) * i / 4f,
                        sx, sy1 - (sy1 - sy0) * i / 4f);
            }
            float markY = sy1 - (float)(_open / 100.0) * (sy1 - sy0);
            using (var mp = new SolidBrush(Color.FromArgb(210, 60, 55)))
                g.FillPolygon(mp, new[]
                {
                    new PointF(sx - 3f, markY), new PointF(sx - 9f, markY - 4f),
                    new PointF(sx - 9f, markY + 4f)
                });
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // 新式阀门驱动
        // ----------------------------------------------------------------
        // 涡轮箱手轮（bevel=true 为伞齿轮侧置手轮）
        private void DrawGear(Graphics g, RectangleF a, HToolPalette pal, bool bevel)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            // 阀轭
            Yoke(g, cx, body.Y - 2f, a.Top + a.Height * 0.34f, pal);
            // 齿轮箱
            var box = new RectangleF(cx - a.Width * 0.18f, a.Top + a.Height * 0.26f,
                a.Width * 0.36f, a.Height * 0.1f);
            g.Box(box, 4f, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.4f);
            // 手轮
            float wx = bevel ? box.Right + 9f : box.Right + 12f;
            float wy = box.Top + box.Height / 2f;
            float wr = bevel ? 10f : 12f;
            if (bevel)
            {
                // 伞齿轮：倾斜椭圆手轮
                var st = g.Save();
                g.TranslateTransform(wx, wy);
                g.RotateTransform(-20f);
                g.DrawEllipse(new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 3f), -wr, -wr * 0.55f, wr * 2f, wr * 1.1f);
                using (var sp = new Pen(pal.Edge, 1.3f))
                    for (int i = 0; i < 5; i++)
                    {
                        double an = i * Math.PI * 2.0 / 5.0;
                        g.DrawLine(sp, 0f, 0f, (float)Math.Cos(an) * wr, (float)Math.Sin(an) * wr * 0.55f);
                    }
                g.Restore(st);
                g.Line(pal.Edge, 2f, box.Right, wy, wx - 4f, wy);
            }
            else
            {
                Wheel(g, wx, wy, wr, pal);
                g.Line(pal.Edge, 2.4f, box.Right, wy, wx - wr, wy);
            }
            g.Ell(cx, box.Top - 2f, 3.4f, 3.4f, pal.Accent);
            // 阀杆
            g.Line(pal.Metal, 4f, cx, box.Bottom, cx, body.Y);
        }
        // 杠杆手柄（locked=true 带挂锁）
        private void DrawLever(Graphics g, RectangleF a, HToolPalette pal, bool locked)
        {
            float k = (float)(_open / 100.0);
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.8f;
            // 球阀本体
            g.Box(new RectangleF(a.Left + 3f, cy - 9f, a.Width - 6f, 18f), 3f,
                HToolPalettes.Tint(pal.Main, 0.18f), pal.Edge, 1.4f);
            // 阀孔（开度示意）
            g.Ell(cx, cy, 6f, 6f, k > 0.5f ? Color.FromArgb(90, 130, 150) : Color.FromArgb(50, 54, 60));
            // 填料压盖
            g.Box(new RectangleF(cx - 6f, cy - 16f, 12f, 7f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 手柄：关闭竖直、开启水平
            double ang = -(Math.PI / 2.0) * (1.0 - k);
            float len = a.Height * 0.26f;
            float ex = cx + (float)Math.Cos(ang) * len, ey = cy - 12f + (float)Math.Sin(ang) * len;
            g.Line(HToolPalettes.Shade(pal.Main, 0.25f), 4.5f, cx, cy - 12f, ex, ey);
            g.Line(pal.Accent, 6f, ex - (float)Math.Cos(ang) * 12f, ey - (float)Math.Sin(ang) * 12f, ex, ey);
            if (locked)
            {
                // 挂锁 + 锁链
                g.Box(new RectangleF(cx - 5f, cy - 27f, 10f, 8f), 2f, Color.FromArgb(210, 170, 50), pal.Edge, 1.1f);
                g.DrawArc(new Pen(pal.Edge, 1.6f), cx - 3.5f, cy - 33f, 7f, 8f, 180f, 180f);
            }
            g.Lamp(a.Right - 8f, cy - 20f, k > 0.5f);
        }
        // 链轮：顶置链轮 + 下垂锚链
        private void DrawChain(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            float wy = a.Top + a.Height * 0.16f, wr = 11f;
            // 阀杆 + 轭
            g.Line(pal.Metal, 4f, cx, wy + wr, cx, body.Y);
            Yoke(g, cx, body.Y - 2f, wy + wr + 4f, pal);
            Wheel(g, cx, wy, wr, pal);
            // 锚链（两侧交替链环）
            float chainLen = 26f + k * 16f;
            using (var link = new Pen(pal.Edge, 1.4f))
                for (int i = 0; i < 6; i++)
                {
                    float y = wy + wr + 3f + i * (chainLen - 18f) / 5f;
                    if (y > body.Y - 6f) break;
                    g.DrawEllipse(link, cx - wr + 3f - 2.5f, y, 5f, 7f);
                    g.DrawEllipse(link, cx + wr - 3f - 2.5f, y + 3.5f, 5f, 7f);
                }
        }
        // 气动薄膜执行器（spring=true 为故障安全弹簧缸，弹簧外露）
        private void DrawDiaphragm(Graphics g, RectangleF a, HToolPalette pal, bool spring)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            float headTop = a.Top + a.Height * 0.08f, headW = a.Width * 0.56f;
            float headH = a.Height * 0.14f;
            // 阀轭
            float yokeTop = headTop + headH;
            Yoke(g, cx, body.Y - 2f, yokeTop, pal);
            // 薄膜头（上下盖夹膜片）
            g.Poly(new[]
            {
                new PointF(cx - headW / 2f, headTop + headH * 0.55f),
                new PointF(cx + headW / 2f, headTop + headH * 0.55f),
                new PointF(cx + headW / 2f, headTop + headH),
                new PointF(cx - headW / 2f, headTop + headH)
            }, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            using (var dome = new GraphicsPath())
            {
                dome.AddArc(new RectangleF(cx - headW / 2f, headTop - headH * 0.1f, headW, headH * 0.9f),
                    180f, 180f);
                dome.CloseFigure();
                using (var db = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.3f)))
                    g.FillPath(db, dome);
                g.DrawPath(new Pen(pal.Edge, 1.4f), dome);
            }
            // 膜片（随开度起伏）
            using (var dia = new Pen(Color.FromArgb(120, 70, 76, 84), 2f))
                g.DrawBezier(dia, cx - headW / 2f + 2f, headTop + headH * 0.55f,
                    cx - headW * 0.2f, headTop + headH * (0.42f - k * 0.12f),
                    cx + headW * 0.2f, headTop + headH * (0.42f - k * 0.12f),
                    cx + headW / 2f - 2f, headTop + headH * 0.55f);
            // 阀杆
            float stemTop = spring ? yokeTop - 2f : headTop + headH * 0.5f;
            g.Line(pal.Metal, 3.4f, cx, stemTop, cx, body.Y);
            if (spring)
                Zigzag(g, cx, yokeTop + 2f, body.Y - 6f, a.Width * 0.14f, pal.Edge, 8);
            // 气源接口 + 调节螺母
            g.Box(new RectangleF(cx + headW / 2f - 2f, headTop + 2f, 7f, 6f), 1f, pal.MetalDark, pal.Edge, 1f);
            g.Ell(cx, headTop - 3f, 4f, 4f, pal.MetalDark);
        }
        // 气动活塞执行器
        private void DrawPiston(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            float cy0 = a.Top + a.Height * 0.08f, cw = a.Width * 0.34f, ch = a.Height * 0.22f;
            // 气缸
            g.Box(new RectangleF(cx - cw / 2f, cy0, cw, ch), 4f,
                HToolPalettes.Tint(pal.Main, 0.25f), pal.Edge, 1.4f);
            // 活塞（随开度上下）
            float py = cy0 + ch * (0.72f - k * 0.44f);
            g.Line(pal.MetalDark, 5f, cx - cw / 2f + 3f, py, cx + cw / 2f - 3f, py);
            // 活塞杆
            g.Line(pal.Metal, 3.6f, cx, py, cx, body.Y);
            // 上下气口
            g.Box(new RectangleF(cx - cw / 2f - 6f, cy0 + 5f, 6f, 6f), 1f, pal.MetalDark, pal.Edge, 1f);
            g.Box(new RectangleF(cx - cw / 2f - 6f, cy0 + ch - 11f, 6f, 6f), 1f, pal.MetalDark, pal.Edge, 1f);
            Yoke(g, cx, body.Y - 2f, cy0 + ch, pal);
        }
        // 齿轮齿条摆动执行器
        private void DrawRack(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            float cy = a.Top + a.Height * 0.26f;
            // 横置气缸
            var cyl = new RectangleF(a.Left + a.Width * 0.12f, cy - a.Height * 0.06f,
                a.Width * 0.56f, a.Height * 0.12f);
            g.Box(cyl, cyl.Height / 2f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.3f);
            // 齿条
            float rx = cyl.Left + cyl.Width * (0.2f + k * 0.55f);
            g.Line(pal.MetalDark, 3f, cyl.Left + 4f, cy, rx, cy);
            // 齿轮 + 阀杆
            float gy = cy + cyl.Height / 2f + 8f;
            g.Ell(cx, gy, 8f, 8f, HToolPalettes.Shade(pal.Main, 0.1f));
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), cx - 8f, gy - 8f, 16f, 16f);
            for (int i = 0; i < 8; i++)
            {
                double an = i * Math.PI / 4.0;
                g.Line(pal.Edge, 1.1f, cx + (float)Math.Cos(an) * 5f, gy + (float)Math.Sin(an) * 5f,
                    cx + (float)Math.Cos(an) * 9f, gy + (float)Math.Sin(an) * 9f);
            }
            g.Line(pal.Metal, 3.4f, cx, gy + 8f, cx, body.Y);
            Yoke(g, cx, body.Y - 2f, gy + 12f, pal);
        }
        // 电动执行头（multi=true 多回转高头，false 角行程矮头）
        private void DrawElectric(Graphics g, RectangleF a, HToolPalette pal, bool multi)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            float top = a.Top + (multi ? a.Height * 0.06f : a.Height * 0.2f);
            float h = multi ? a.Height * 0.24f : a.Height * 0.14f;
            var head = new RectangleF(cx - a.Width * 0.24f, top, a.Width * 0.48f, h);
            g.Box(head, 4f, HToolPalettes.Shade(pal.Main, 0.08f), pal.Edge, 1.5f);
            // 顶盖 + 电缆接头
            g.Box(new RectangleF(head.Left + 4f, head.Top - 6f, head.Width - 8f, 7f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            g.Line(pal.Edge, 2f, cx, head.Top - 12f, cx, head.Top - 6f);
            // 显示屏 + 按钮
            var scr = new RectangleF(head.Left + 6f, head.Top + 6f, head.Width - 12f, h * 0.32f);
            g.Box(scr, 2f, Color.FromArgb(36, 58, 70), pal.Edge, 1.1f);
            using (var bf = new Font("微软雅黑", 6.5f, FontStyle.Bold))
                HBarBase.DrawText(g, ((int)(k * 100)).ToString(), bf, Color.FromArgb(120, 230, 170),
                    scr, ContentAlignment.MiddleCenter);
            g.Lamp(head.Left + head.Width * 0.3f, head.Bottom - 8f, k > 0.5f);
            g.Lamp(head.Left + head.Width * 0.55f, head.Bottom - 8f, false);
            g.Ell(head.Left + head.Width * 0.78f, head.Bottom - 8f, 3f, 3f, pal.Accent);
            // 侧手轮
            Wheel(g, head.Right + 8f, head.Top + h * 0.55f, 7f, pal);
            g.Line(pal.Edge, 2f, head.Right, head.Top + h * 0.55f, head.Right + 2f, head.Top + h * 0.55f);
            // 阀杆
            g.Line(pal.Metal, 3.6f, cx, head.Bottom, cx, body.Y);
            Yoke(g, cx, body.Y - 2f, head.Bottom + 2f, pal);
        }
        // 液压执行器（withMotor=true 顶置电机=电液联动）
        private void DrawHydraulic(Graphics g, RectangleF a, HToolPalette pal, bool withMotor)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            float top = withMotor ? a.Top + a.Height * 0.24f : a.Top + a.Height * 0.1f;
            float ch = a.Height * 0.2f, cw = a.Width * 0.36f;
            g.Box(new RectangleF(cx - cw / 2f, top, cw, ch), 4f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.4f);
            // 活塞杆
            float py = top + ch * (0.75f - k * 0.5f);
            g.Line(pal.MetalDark, 4f, cx - cw / 2f + 4f, py, cx + cw / 2f - 4f, py);
            g.Line(pal.Metal, 3.4f, cx, py, cx, body.Y);
            Yoke(g, cx, body.Y - 2f, top + ch, pal);
            if (withMotor)
            {
                // 顶置电机 + 联轴器
                var mot = new RectangleF(cx - a.Width * 0.26f, a.Top + a.Height * 0.08f,
                    a.Width * 0.52f, a.Height * 0.1f);
                g.Motor(mot.Left, mot.Right, mot.Top + mot.Height / 2f, mot.Height / 2f, pal);
                g.Line(pal.Edge, 3f, cx, mot.Bottom, cx, top);
            }
            // 高压油管
            using (var hose = new Pen(pal.Edge, 2f))
            {
                g.DrawBezier(hose, cx - cw / 2f, top + 6f, cx - a.Width * 0.34f, top + 6f,
                    cx - a.Width * 0.34f, body.Y - 10f, cx - 8f, body.Y - 4f);
                g.DrawBezier(hose, cx - cw / 2f, top + ch - 6f, cx - a.Width * 0.3f, top + ch,
                    cx - a.Width * 0.3f, body.Y, cx - 8f, body.Y + 4f);
            }
        }
        // T 型扳手暗杆阀
        private void DrawTBar(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f, by = a.Top + a.Height * 0.14f;
            // 暗杆不升降：T 把固定，阀瓣在体内转动
            using (var bar = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 5.5f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(bar, cx - a.Width * 0.26f, by, cx + a.Width * 0.26f, by);
            g.Line(pal.Metal, 4f, cx, by, cx, body.Y);
            g.Ell(cx, by, 5f, 5f, pal.Accent);
            Yoke(g, cx, body.Y - 2f, a.Top + a.Height * 0.34f, pal);
            // 填料螺母
            g.Box(new RectangleF(cx - 8f, body.Y - 12f, 16f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
        }
        // 方帽扳手位
        private void DrawSquareNut(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f, ny = a.Top + a.Height * 0.2f;
            Yoke(g, cx, body.Y - 2f, ny + 14f, pal);
            g.Line(pal.Metal, 3.6f, cx, ny, cx, body.Y);
            // 方帽
            g.Box(new RectangleF(cx - 9f, ny, 18f, 14f), 2f, pal.Metal, pal.Edge, 1.4f);
            g.Line(pal.Edge, 1.2f, cx - 9f, ny + 7f, cx + 9f, ny + 7f);
            g.Ell(cx, ny - 3f, 3f, 3f, pal.Accent);
        }
        // 旋钮手柄（小口径阀）
        private void DrawKnob(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.8f;
            // 小型阀体
            g.Box(new RectangleF(a.Left + 6f, cy - 7f, a.Width - 12f, 14f), 3f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.3f);
            g.Box(new RectangleF(cx - 6f, cy - 14f, 12f, 7f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 滚花旋钮（开度旋转）
            float ky = cy - a.Height * 0.16f;
            float rr = a.Width * 0.2f;
            g.Ell(cx, ky, rr, rr, HToolPalettes.Shade(pal.Main, 0.12f));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - rr, ky - rr, rr * 2f, rr * 2f);
            using (var kn = new Pen(HToolPalettes.Shade(pal.Main, 0.35f), 1.2f))
                for (int i = 0; i < 12; i++)
                {
                    double an = i * Math.PI / 6.0;
                    g.DrawLine(kn, cx + (float)Math.Cos(an) * (rr - 2f), ky + (float)Math.Sin(an) * (rr - 2f),
                        cx + (float)Math.Cos(an) * rr, ky + (float)Math.Sin(an) * rr);
                }
            // 指示刻线随开度旋转
            double ia = -Math.PI / 2.0 + k * Math.PI * 1.5;
            g.Line(pal.Accent, 2.2f, cx, ky,
                cx + (float)Math.Cos(ia) * (rr - 3f), ky + (float)Math.Sin(ia) * (rr - 3f));
        }
        // 脚踏阀
        private void DrawFoot(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            float cx = a.Left + a.Width * 0.55f, cy = a.Top + a.Height * 0.8f;
            // 阀体
            g.Box(new RectangleF(cx - 12f, cy - 9f, 24f, 18f), 3f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.3f);
            g.Ell(cx, cy, 5f, 5f, k > 0.5f ? Color.FromArgb(90, 130, 150) : Color.FromArgb(50, 54, 60));
            // 踏板（左端铰支，踩下角度随开度）
            float hx = a.Left + a.Width * 0.14f, hy = a.Bottom - a.Height * 0.2f;
            float ang = -(0.18f + k * 0.22f);
            float px = hx + (float)Math.Cos(ang) * a.Width * 0.36f;
            float py = hy + (float)Math.Sin(ang) * a.Width * 0.36f;
            g.Line(pal.MetalDark, 5f, hx, hy, px, py);
            g.Line(HToolPalettes.Shade(pal.Main, 0.2f), 7f, px - 4f, py - 3f, px + 12f, py + 2f);
            g.Ell(hx, hy, 3.5f, 3.5f, pal.MetalDark);
            // 复位弹簧
            Zigzag(g, px, py + 5f, hy + 12f, 5f, pal.Edge, 5);
            // 连杆
            g.Line(pal.Edge, 2f, px, py, cx - 8f, cy - 8f);
        }
        // 电磁线圈阀
        private void DrawSolenoid(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.8f;
            // 阀体 + 动铁芯
            g.Box(new RectangleF(a.Left + 5f, cy - 8f, a.Width - 10f, 16f), 3f,
                HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.3f);
            g.Ell(cx, cy, 5f, 5f, k > 0.5f ? Color.FromArgb(90, 130, 150) : Color.FromArgb(50, 54, 60));
            float lift = k * 5f;
            g.Line(pal.Metal, 2.6f, cx, cy - 8f, cx, cy - 20f - lift);
            // 线圈（梯形骨架 + 绕线）
            var coil = new RectangleF(cx - 10f, cy - a.Height * 0.28f, 20f, a.Height * 0.16f);
            g.Poly(new[]
            {
                new PointF(coil.Left - 3f, coil.Bottom), new PointF(coil.Right + 3f, coil.Bottom),
                new PointF(coil.Right, coil.Top), new PointF(coil.Left, coil.Top)
            }, HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.3f);
            using (var wind = new Pen(HToolPalettes.Tint(pal.Main, 0.1f), 1.3f))
                for (float y = coil.Top + 3f; y < coil.Bottom - 2f; y += 4f)
                    g.DrawLine(wind, coil.Left - 1f, y, coil.Right + 1f, y - 2f);
            // DIN 插头 + 电缆
            g.Box(new RectangleF(coil.Right - 2f, coil.Top - 6f, 11f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Line(pal.Edge, 1.8f, coil.Right + 9f, coil.Top - 2f, a.Right - 3f, coil.Top - 8f);
            g.Lamp(coil.Left + 4f, coil.Top + 5f, k > 0.5f);
        }
        // 热静力执行器（温包 + 毛细管）
        private void DrawThermal(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.5f;
            // 执行头
            var head = new RectangleF(cx - 9f, a.Top + a.Height * 0.3f, 18f, a.Height * 0.1f);
            g.Box(head, 3f, HToolPalettes.Tint(pal.Main, 0.25f), pal.Edge, 1.3f);
            g.Line(pal.Metal, 3f, cx, head.Bottom, cx, body.Y);
            Yoke(g, cx, body.Y - 2f, head.Bottom + 2f, pal);
            // 毛细管（盘旋几圈后上连温包）
            using (var cap = new Pen(Color.FromArgb(160, 110, 70), 1.8f))
            {
                float sx = head.Left + 3f, sy = head.Top + head.Height / 2f;
                g.DrawBezier(cap, sx, sy, sx - 10f, sy - 6f, sx - 2f, sy - 14f, sx + 1f, a.Top + a.Height * 0.22f);
                g.DrawArc(cap, cx - 7f, a.Top + a.Height * 0.12f, 14f, 10f, 0f, 360f);
                g.DrawLine(cap, cx, a.Top + a.Height * 0.12f, cx, a.Top + a.Height * 0.08f);
            }
            // 温包
            g.Box(new RectangleF(cx - 4f, a.Top + 2f, 8f, a.Height * 0.08f), 3f,
                Color.FromArgb(190, 150, 110), pal.Edge, 1.2f);
            g.Lamp(head.Right - 4f, head.Top + 4f, k > 0.5f);
        }
        // 电动三件式球阀
        private void DrawMotorBall(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.74f;
            // 三件式阀体
            g.Box(new RectangleF(a.Left + 2f, cy - 8f, a.Width * 0.24f, 16f), 2f, pal.Metal, pal.Edge, 1.2f);
            g.Box(new RectangleF(a.Right - a.Width * 0.24f - 2f, cy - 8f, a.Width * 0.24f, 16f), 2f, pal.Metal, pal.Edge, 1.2f);
            g.Poly(new[]
            {
                new PointF(cx - 13f, cy - 11f), new PointF(cx + 13f, cy - 11f),
                new PointF(cx + 10f, cy + 11f), new PointF(cx - 10f, cy + 11f)
            }, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.4f);
            g.Ell(cx, cy, 6.5f, 6.5f, k > 0.5f ? Color.FromArgb(90, 130, 150) : Color.FromArgb(50, 54, 60));
            // 矮电动头
            var head = new RectangleF(cx - 11f, cy - a.Height * 0.22f, 22f, a.Height * 0.12f);
            g.Box(head, 3f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.3f);
            g.Line(pal.Metal, 3f, cx, head.Bottom, cx, cy - 11f);
            g.Lamp(head.Right - 5f, head.Top + 5f, k > 0.5f);
        }
        // 智能阀门定位器（薄膜头 + 侧装配电盒 + 压力表）
        private void DrawPositioner(Graphics g, RectangleF a, HToolPalette pal)
        {
            float k = (float)(_open / 100.0);
            var body = ValveBody(g, a, pal, k);
            float cx = a.Left + a.Width * 0.46f;
            float headTop = a.Top + a.Height * 0.1f, headW = a.Width * 0.46f, headH = a.Height * 0.12f;
            Yoke(g, cx, body.Y - 2f, headTop + headH, pal);
            using (var dome = new GraphicsPath())
            {
                dome.AddArc(new RectangleF(cx - headW / 2f, headTop - headH * 0.1f, headW, headH * 0.9f),
                    180f, 180f);
                dome.CloseFigure();
                using (var db = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.3f)))
                    g.FillPath(db, dome);
                g.DrawPath(new Pen(pal.Edge, 1.4f), dome);
            }
            g.Line(pal.Metal, 3f, cx, headTop + headH * 0.5f, cx, body.Y);
            // 定位器盒
            var pos = new RectangleF(cx + headW / 2f - 2f, headTop + headH * 0.2f,
                a.Width * 0.22f, a.Height * 0.12f);
            g.Box(pos, 3f, pal.MetalDark, pal.Edge, 1.3f);
            var scr = new RectangleF(pos.Left + 3f, pos.Top + 3f, pos.Width - 6f, pos.Height * 0.4f);
            g.Box(scr, 1f, Color.FromArgb(36, 58, 70), pal.Edge, 1f);
            using (var bf = new Font("微软雅黑", 6f, FontStyle.Bold))
                HBarBase.DrawText(g, ((int)(k * 100)).ToString(), bf, Color.FromArgb(120, 230, 170),
                    scr, ContentAlignment.MiddleCenter);
            // 压力表
            float gx = pos.Left + pos.Width / 2f, gy = pos.Bottom + 8f, gr = 6f;
            g.Ell(gx, gy, gr, gr, Color.White);
            g.DrawEllipse(new Pen(pal.Edge, 1.1f), gx - gr, gy - gr, gr * 2f, gr * 2f);
            double ga = -Math.PI * 0.75 + k * Math.PI * 1.5;
            g.Line(pal.Accent, 1.4f, gx, gy, gx + (float)Math.Cos(ga) * (gr - 2f),
                gy + (float)Math.Sin(ga) * (gr - 2f));
        }
        // ----------------------------------------------------------------
        // 共享小原语
        // ----------------------------------------------------------------
        /// <summary>底部直通阀体（管段 + 法兰 + 中腔），返回中腔顶点。</summary>
        private static PointF ValveBody(Graphics g, RectangleF a, HToolPalette pal, float k)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.82f;
            var pipe = new RectangleF(a.Left + 1f, cy - 4f, a.Width - 2f, 8f);
            g.FillCylH(pipe, pal.MetalDark, pal.Metal);
            g.Box(new RectangleF(pipe.Left - 2f, cy - 7f, 6f, 14f), 1.5f, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(pipe.Right - 4f, cy - 7f, 6f, 14f), 1.5f, pal.Metal, pal.Edge, 1.1f);
            g.Ell(cx, cy, 10f, 10f, HToolPalettes.Tint(pal.Main, 0.15f));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - 10f, cy - 10f, 20f, 20f);
            // 阀瓣（随开度提升）
            using (var wedge = new SolidBrush(HToolPalettes.Shade(pal.Metal, 0.1f)))
                g.FillPolygon(wedge, new[]
                {
                    new PointF(cx - 7f, cy + 7f - k * 6f), new PointF(cx + 7f, cy + 7f - k * 6f),
                    new PointF(cx + 4f, cy - 2f - k * 6f), new PointF(cx - 4f, cy - 2f - k * 6f)
                });
            return new PointF(cx, cy - 10f);
        }
        /// <summary>阀轭（两根支腿）。</summary>
        private static void Yoke(Graphics g, float cx, float y1, float y2, HToolPalette pal)
        {
            if (y2 >= y1 - 4f) return;
            g.Line(pal.MetalDark, 4f, cx - 11f, y1, cx - 11f, y2);
            g.Line(pal.MetalDark, 4f, cx + 11f, y1, cx + 11f, y2);
            g.Line(pal.MetalDark, 4f, cx - 11f, y2, cx + 11f, y2);
        }
        /// <summary>圆形手轮（轮圈 + 辐条 + 轴帽）。</summary>
        private static void Wheel(Graphics g, float cx, float cy, float r, HToolPalette pal)
        {
            g.DrawEllipse(new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 3f), cx - r, cy - r, r * 2f, r * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.1f), cx - r, cy - r, r * 2f, r * 2f);
            using (var sp = new Pen(pal.Edge, 1.3f))
                for (int i = 0; i < 5; i++)
                {
                    double an = i * Math.PI * 2.0 / 5.0 + 0.3;
                    g.DrawLine(sp, cx, cy, cx + (float)Math.Cos(an) * (r - 2f), cy + (float)Math.Sin(an) * (r - 2f));
                }
            g.Ell(cx, cy, 3.4f, 3.4f, pal.Accent);
        }
        /// <summary>压力弹簧（Z 形折线）。</summary>
        private static void Zigzag(Graphics g, float cx, float y1, float y2, float half, Color c, int n)
        {
            using (var p = new Pen(c, 1.6f))
            {
                float dy = (y2 - y1) / n;
                for (int i = 0; i < n; i++)
                {
                    float ya = y1 + dy * i, yb = ya + dy;
                    g.DrawLine(p, cx, ya, cx + (i % 2 == 0 ? half : -half), ya + dy / 2f);
                    g.DrawLine(p, cx + (i % 2 == 0 ? half : -half), ya + dy / 2f, cx, yb);
                }
            }
        }
    }
}