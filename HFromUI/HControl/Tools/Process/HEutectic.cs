using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Process
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 共晶/键合设备样式：Classic 为龙门共晶贴片机，其余 21 种覆盖倒装/引线键合、
    /// 固晶、热压/银烧结/超声/激光/阳极/晶圆/混合键合，回流炉、波峰焊、
    /// 焊锡机器人、脉冲热压与泛用贴片机等。
    /// </summary>
    public enum HEutecticStyle
    {
        Classic = 0,        // 共晶贴片机
        FlipChip = 1,       // 倒装键合机
        WireBond = 2,       // 引线键合机
        DieAttach = 3,      // 固晶机
        ReflowOven = 4,     // 回流焊炉
        VacuumChamber = 5,  // 真空共晶炉
        FormicAcid = 6,     // 甲酸还原炉
        SilverSinter = 7,   // 银烧结压机
        TCB = 8,            // 热压键合机
        UltrasonicWelder = 9,// 超声金属焊机
        LaserSolder = 10,   // 激光焊接机
        PasteAttach = 11,   // 锡膏点胶固晶
        WaferBond = 12,     // 晶圆键合机
        HybridBond = 13,    // 混合键合机
        AnodicBond = 14,    // 阳极键合机
        HotBar = 15,        // 脉冲热压焊
        SolderRobot = 16,   // 焊锡机器人
        SelectiveWave = 17, // 选择性波峰焊
        WaveSolder = 18,    // 波峰焊机
        PickPlace = 19,     // 泛用贴片机
        BatchOven = 20,     // 批次热风炉
        PreformDie = 21     // 焊片贴装机
    }
    /// <summary>
    /// 共晶贴片机控件（继承 HToolAnimBase）：22 种芯片贴装/键合/焊接设备外形、13 种色调，
    /// Running 时贴装头上下、炉台发热、锡波涌动、送线等按工艺产生动画。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("共晶贴片机控件：22 种键合焊接外形、13 种色调，运行时贴装头运动并加热发光")]
    public class HEutectic : HToolAnimBase
    {
        private HEutecticStyle _style = HEutecticStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HEutectic()
        {
            Size = new Size(150, 150);
        }
        /// <summary>键合设备外形样式。</summary>
        [HCategoryLanguage("共晶台"), HDisplayNameLanguage("键合设备外形样式"), HDescriptionLanguage("键合设备外形样式"), Browsable(true)]
        [DefaultValue(HEutecticStyle.Classic)]
        public HEutecticStyle EutecticStyle
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
                case HEutecticStyle.FlipChip: DrawFlipChip(g, a, pal); break;
                case HEutecticStyle.WireBond: DrawWireBond(g, a, pal); break;
                case HEutecticStyle.DieAttach: DrawDieAttach(g, a, pal); break;
                case HEutecticStyle.ReflowOven: DrawReflow(g, a, pal, false); break;
                case HEutecticStyle.BatchOven: DrawReflow(g, a, pal, true); break;
                case HEutecticStyle.VacuumChamber: DrawVacuumChamber(g, a, pal, false); break;
                case HEutecticStyle.FormicAcid: DrawVacuumChamber(g, a, pal, true); break;
                case HEutecticStyle.SilverSinter: DrawPress(g, a, pal, true); break;
                case HEutecticStyle.TCB: DrawPress(g, a, pal, false); break;
                case HEutecticStyle.UltrasonicWelder: DrawUltrasonic(g, a, pal); break;
                case HEutecticStyle.LaserSolder: DrawLaserSolder(g, a, pal); break;
                case HEutecticStyle.PasteAttach: DrawPasteAttach(g, a, pal); break;
                case HEutecticStyle.WaferBond: DrawWaferBond(g, a, pal, false); break;
                case HEutecticStyle.HybridBond: DrawWaferBond(g, a, pal, true); break;
                case HEutecticStyle.AnodicBond: DrawAnodic(g, a, pal); break;
                case HEutecticStyle.HotBar: DrawHotBar(g, a, pal); break;
                case HEutecticStyle.SolderRobot: DrawSolderRobot(g, a, pal); break;
                case HEutecticStyle.SelectiveWave: DrawWave(g, a, pal, true); break;
                case HEutecticStyle.WaveSolder: DrawWave(g, a, pal, false); break;
                case HEutecticStyle.PickPlace: DrawPickPlace(g, a, pal); break;
                case HEutecticStyle.PreformDie: DrawPreform(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>加热辉光。</summary>
        private void HeatGlow(Graphics g, RectangleF r, int baseA)
        {
            if (!Running) return;
            int glow = (int)(baseA + 40 * Math.Abs(Math.Sin(Phase * 2f)));
            using (var halo = new SolidBrush(Color.FromArgb(glow, Color.OrangeRed)))
                g.FillRectangle(halo, RectangleF.Inflate(r, 8f, 6f));
        }
        /// <summary>芯片小方块。</summary>
        private void Die(Graphics g, RectangleF r)
        {
            using (var db = new SolidBrush(Color.FromArgb(58, 64, 86)))
                g.FillRectangle(db, r);
            g.DrawRectangle(new Pen(Color.FromArgb(120, 130, 160), 1f), r.X, r.Y, r.Width, r.Height);
        }
        /// <summary>龙门双柱 + 横梁。</summary>
        private void Gantry(Graphics g, RectangleF a, HToolPalette pal, float legFrac)
        {
            using (var frame = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.12f)))
            {
                g.FillRectangle(frame, a.X + a.Width * 0.1f, a.Y + a.Height * 0.12f,
                    a.Width * 0.07f, a.Height * legFrac);
                g.FillRectangle(frame, a.Right - a.Width * 0.17f, a.Y + a.Height * 0.12f,
                    a.Width * 0.07f, a.Height * legFrac);
                g.FillRectangle(frame, a.X + a.Width * 0.1f, a.Y + a.Height * 0.1f,
                    a.Width * 0.8f, a.Height * 0.07f);
            }
        }
        // ----------------------------------------------------------------
        // Classic：龙门共晶贴片机（原验收外形）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width, glyphH = a.Height;
            // 底座
            var bed = new RectangleF(Width * 0.06f, glyphH * 0.78f, Width * 0.88f, glyphH * 0.14f);
            using (var gp = HBarBase.RoundPath(bed, 4f))
            using (var b = new LinearGradientBrush(bed, HToolPalettes.Tint(pal.Main, 0.2f),
                HToolPalettes.Shade(pal.Main, 0.3f), 90f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.5f), gp);
            }
            // 龙门双柱 + 横梁
            using (var frame = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.12f)))
            {
                g.FillRectangle(frame, Width * 0.1f, glyphH * 0.12f, Width * 0.07f, glyphH * 0.66f);
                g.FillRectangle(frame, Width * 0.83f, glyphH * 0.12f, Width * 0.07f, glyphH * 0.66f);
                g.FillRectangle(frame, Width * 0.1f, glyphH * 0.1f, Width * 0.8f, glyphH * 0.07f);
            }
            // 加热工作台
            var stage = new RectangleF(Width * 0.3f, glyphH * 0.64f, Width * 0.4f, glyphH * 0.14f);
            if (Running)
            {
                int glow = (int)(60 + 50 * Math.Abs(Math.Sin(Phase * 2f)));
                using (var halo = new SolidBrush(Color.FromArgb(glow, Color.OrangeRed)))
                    g.FillRectangle(halo, RectangleF.Inflate(stage, 8f, 6f));
            }
            using (var sb = new LinearGradientBrush(stage, Color.FromArgb(150, 106, 64),
                Color.FromArgb(96, 64, 40), 90f))
                g.FillRectangle(sb, stage);
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), stage.X, stage.Y, stage.Width, stage.Height);
            // 引线框架 + 芯片
            var lead = new RectangleF(stage.X + stage.Width * 0.2f, stage.Y - 3f, stage.Width * 0.6f, 3f);
            using (var lb = new SolidBrush(pal.Metal))
                g.FillRectangle(lb, lead);
            var die = new RectangleF(stage.X + stage.Width * 0.38f, stage.Y - glyphH * 0.06f,
                stage.Width * 0.24f, glyphH * 0.05f);
            using (var db = new SolidBrush(Color.FromArgb(58, 64, 86)))
                g.FillRectangle(db, die);
            g.DrawRectangle(new Pen(Color.FromArgb(120, 130, 160), 1f), die.X, die.Y, die.Width, die.Height);
            // 贴装头（上下运动）
            float bob = Running ? (float)(Math.Sin(Phase * 2.4f) * 0.5 + 0.5) * glyphH * 0.08f : 0f;
            float hx = Width * 0.5f;
            float headTop = glyphH * 0.17f + bob;
            var slide = new RectangleF(hx - Width * 0.1f, glyphH * 0.17f, Width * 0.2f, glyphH * 0.08f);
            using (var slb = new SolidBrush(pal.MetalDark))
                g.FillRectangle(slb, slide);
            var head = new RectangleF(hx - Width * 0.07f, headTop + glyphH * 0.08f,
                Width * 0.14f, glyphH * 0.12f);
            using (var gp = HBarBase.RoundPath(head, 3f))
            using (var b = new LinearGradientBrush(head, HToolPalettes.Tint(pal.Main, 0.25f),
                HToolPalettes.Shade(pal.Main, 0.2f), 0f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.2f), gp);
            }
            // 吸嘴 + 金球
            float nozzleY = head.Bottom;
            g.DrawLine(new Pen(Color.FromArgb(80, 84, 90), 2.4f), hx, nozzleY, hx, nozzleY + glyphH * 0.1f + bob);
            using (var gold = new SolidBrush(Color.FromArgb(240, 196, 72)))
                g.FillEllipse(gold, hx - 3.2f, nozzleY + glyphH * 0.1f + bob - 2f, 6.4f, 6.4f);
            // 操控面板
            var panel = new RectangleF(Width * 0.13f, glyphH * 0.3f, Width * 0.1f, glyphH * 0.16f);
            using (var pb = new SolidBrush(Color.FromArgb(40, 46, 56)))
                g.FillRectangle(pb, panel);
            using (var lamp = new SolidBrush(Running ? Color.FromArgb(80, 210, 120) : Color.FromArgb(210, 60, 55)))
                g.FillEllipse(lamp, panel.X + panel.Width / 2f - 3f, panel.Y + 5f, 6f, 6f);
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // FlipChip：倒装键合（吸嘴持面朝下芯片 + 凸点 + 基板）
        // ----------------------------------------------------------------
        private void DrawFlipChip(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.08f, a.Bottom - a.Height * 0.16f,
                a.Width * 0.84f, a.Height * 0.1f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            Gantry(g, a, pal, 0.62f);
            float bob = Running ? (float)(Math.Sin(Phase * 2f) * 0.5 + 0.5) * a.Height * 0.1f : a.Height * 0.03f;
            float hx = a.X + a.Width / 2f;
            // 吸嘴杆
            g.Line(pal.MetalDark, 3f, hx, a.Y + a.Height * 0.17f, hx, a.Y + a.Height * 0.36f + bob);
            g.Box(new RectangleF(hx - 9f, a.Y + a.Height * 0.16f, 18f, 10f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            // 面朝下芯片（凸点向下）
            var chip = new RectangleF(hx - a.Width * 0.13f, a.Y + a.Height * 0.36f + bob,
                a.Width * 0.26f, a.Height * 0.12f);
            Die(g, chip);
            using (var bump = new SolidBrush(Color.FromArgb(240, 196, 72)))
                for (int i = 0; i < 5; i++)
                    g.FillEllipse(bump, chip.X + 5f + i * (chip.Width - 10f) / 4f - 1.6f,
                        chip.Bottom - 1f, 3.2f, 3.2f);
            // 基板（对应焊盘）
            var sub = new RectangleF(a.X + a.Width * 0.24f, a.Bottom - a.Height * 0.24f,
                a.Width * 0.52f, a.Height * 0.07f);
            g.Box(sub, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            using (var pad = new SolidBrush(pal.Metal))
                for (int i = 0; i < 5; i++)
                    g.FillRectangle(pad, sub.X + 5f + i * (sub.Width - 10f) / 4f - 2f,
                        sub.Y - 2f, 4f, 3f);
            HeatGlow(g, sub, 30);
        }
        // ----------------------------------------------------------------
        // WireBond：引线键合（毛细管 + 金线弧）
        // ----------------------------------------------------------------
        private void DrawWireBond(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.18f,
                a.Width * 0.8f, a.Height * 0.12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // 基板 + 芯片 + 焊盘
            var sub = new RectangleF(a.X + a.Width * 0.18f, a.Bottom - a.Height * 0.26f,
                a.Width * 0.64f, a.Height * 0.08f);
            g.Box(sub, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            var die = new RectangleF(a.X + a.Width * 0.4f, a.Bottom - a.Height * 0.4f,
                a.Width * 0.2f, a.Height * 0.14f);
            Die(g, die);
            // 键合头（斜置毛细管）
            float hx = a.X + a.Width * 0.62f, hy = a.Y + a.Height * 0.28f;
            var st = g.Save();
            g.TranslateTransform(hx, hy);
            g.RotateTransform(-18f);
            g.Box(new RectangleF(-10f, -8f, 20f, 16f), 3f,
                HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.2f);
            using (var cap = new Pen(Color.FromArgb(220, 224, 230), 3f))
                g.DrawLine(cap, 0f, 8f, 0f, 34f);
            g.Restore(st);
            g.Line(pal.MetalDark, 4f, hx, hy, a.X + a.Width * 0.8f, a.Y + a.Height * 0.16f);
            // 金线弧（从芯片焊盘画到毛细管尖，弧高周期）
            float arch = Running ? a.Height * 0.05f + (float)(Math.Sin(Phase * 2f) * 0.5 + 0.5) * a.Height * 0.03f
                : a.Height * 0.06f;
            float tipX = hx + 10f, tipY = hy + 32f;
            using (var wire = new Pen(Color.FromArgb(230, 200, 90), 1.4f))
                g.DrawBezier(wire, die.Right - 4f, die.Top,
                    die.Right + 6f, die.Top - arch, tipX - 8f, tipY - arch * 0.4f, tipX, tipY);
            // 焊球
            using (var ball = new SolidBrush(Color.FromArgb(240, 196, 72)))
                g.FillEllipse(ball, die.Right - 6f, die.Top - 2.5f, 5f, 5f);
            g.Lamp(a.X + a.Width * 0.84f, a.Y + a.Height * 0.2f, Running);
        }
        // ----------------------------------------------------------------
        // DieAttach：固晶机（晶圆环 + 顶针 + 点胶臂）
        // ----------------------------------------------------------------
        private void DrawDieAttach(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.08f, a.Bottom - a.Height * 0.16f,
                a.Width * 0.84f, a.Height * 0.1f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // 基板引线框
            var lead = new RectangleF(a.X + a.Width * 0.5f, a.Bottom - a.Height * 0.3f,
                a.Width * 0.34f, a.Height * 0.08f);
            g.Box(lead, 1f, pal.Metal, pal.Edge, 1f);
            // 晶圆（蓝膜环，左下）
            float wx = a.X + a.Width * 0.26f, wy = a.Bottom - a.Height * 0.34f, wr = a.Width * 0.16f;
            using (var film = new SolidBrush(Color.FromArgb(140, 60, 110, 180)))
                g.FillEllipse(film, wx - wr, wy - wr, wr * 2f, wr * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), wx - wr, wy - wr, wr * 2f, wr * 2f);
            using (var dieB = new SolidBrush(Color.FromArgb(58, 64, 86)))
                for (int i = 0; i < 6; i++)
                {
                    double ang = i * 1.047;
                    g.FillRectangle(dieB, wx + (float)Math.Cos(ang) * wr * 0.5f - 3f,
                        wy + (float)Math.Sin(ang) * wr * 0.5f - 3f, 6f, 6f);
                }
            // 摆臂（在晶圆与引线框之间往复）
            float px = a.X + a.Width * 0.5f, py = a.Y + a.Height * 0.2f;
            g.Ell(px, py, 4f, 4f, pal.MetalDark);
            float swing = Running ? -0.9f + (float)(Math.Sin(Phase) * 0.5 + 0.5) * 0.8f : -0.5f;
            float ex = px + (float)Math.Cos(swing) * a.Width * 0.32f;
            float ey = py - (float)Math.Sin(swing) * a.Width * 0.32f;
            g.Line(pal.MetalDark, 4f, px, py, ex, ey);
            // 吸嘴持芯片
            Die(g, new RectangleF(ex - 5f, ey - 2f, 10f, 8f));
            g.Line(pal.Metal, 1.8f, ex, ey + 6f, ex, ey + 12f);
        }
        // ----------------------------------------------------------------
        // Reflow / BatchOven：回流隧道炉（温区 + 过炉 PCB）
        // ----------------------------------------------------------------
        private void DrawReflow(Graphics g, RectangleF a, HToolPalette pal, bool batch)
        {
            float y = a.Y + a.Height * 0.3f, h = a.Height * 0.4f;
            if (batch)
            {
                // 批次箱式炉：单门 + 观察窗
                var box = new RectangleF(a.X + a.Width * 0.2f, y, a.Width * 0.6f, h);
                g.Box(box, 4f, HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.4f);
                g.Box(new RectangleF(box.X + 8f, box.Y + 8f, box.Width - 16f, box.Height * 0.5f),
                    2f, Color.FromArgb(60, 70, 60), pal.Edge, 1.1f);
                // 内辉光
                if (Running)
                    using (var glow = new SolidBrush(Color.FromArgb(90, 255, 130, 40)))
                        g.FillRectangle(glow, box.X + 10f, box.Y + 10f, box.Width - 20f, box.Height * 0.46f);
                g.Box(new RectangleF(box.Right - 14f, box.Bottom - 18f, 8f, 12f), 1.5f,
                    pal.MetalDark, pal.Edge, 1f);
                // 温控面板
                g.Box(new RectangleF(box.X - 20f, box.Y, 16f, 30f), 2f,
                    Color.FromArgb(40, 46, 56), pal.Edge, 1.1f);
                g.Lamp(box.X - 12f, box.Y + 6f, Running);
                // 支脚
                g.Line(pal.MetalDark, 3f, box.X + 10f, box.Bottom, box.X + 10f, a.Bottom - 6f);
                g.Line(pal.MetalDark, 3f, box.Right - 10f, box.Bottom, box.Right - 10f, a.Bottom - 6f);
                return;
            }
            // 隧道炉体（五温区渐变）
            var oven = new RectangleF(a.X + 6f, y, a.Width - 12f, h);
            g.Box(oven, 5f, HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.4f);
            Color[] zones =
            {
                Color.FromArgb(120, 255, 200, 120), Color.FromArgb(140, 255, 160, 80),
                Color.FromArgb(170, 255, 110, 40), Color.FromArgb(150, 255, 140, 60),
                Color.FromArgb(110, 255, 210, 150)
            };
            for (int i = 0; i < 5; i++)
            {
                var zr = new RectangleF(oven.X + 6f + i * (oven.Width - 12f) / 5f,
                    oven.Y + 8f, (oven.Width - 12f) / 5f - 2f, oven.Height - 16f);
                using (var zb = new SolidBrush(zones[i]))
                    g.FillRectangle(zb, zr);
            }
            // 进出口暗口
            g.Box(new RectangleF(a.X + 2f, y + h * 0.3f, 8f, h * 0.4f), 1f,
                Color.FromArgb(40, 44, 50), pal.Edge, 1f);
            // 传送带 + PCB（移动）
            float beltY = y + h * 0.62f;
            g.Line(pal.MetalDark, 3f, a.X + 2f, beltY, a.Right - 2f, beltY);
            float move = Running ? Phase / (float)Math.PI * 0.5f % 1f : 0.2f;
            for (int i = -1; i < 2; i++)
            {
                float px = a.X + 10f + ((move + i * 0.4f + 1f) % 1f) * (a.Width - 20f);
                g.Box(new RectangleF(px - 14f, beltY - 6f, 28f, 5f), 1f,
                    Color.FromArgb(70, 130, 90), pal.Edge, 1f);
            }
            // 排气管
            g.Line(pal.MetalDark, 3f, a.X + a.Width * 0.7f, oven.Top - 8f,
                a.X + a.Width * 0.7f, oven.Top);
            g.Lamp(a.X + 12f, y + 6f, Running);
        }
        // ----------------------------------------------------------------
        // VacuumChamber / FormicAcid：真空共晶炉（腔体 + 加热台 + 真空表）
        // ----------------------------------------------------------------
        private void DrawVacuumChamber(Graphics g, RectangleF a, HToolPalette pal, bool formic)
        {
            float cx = a.X + a.Width / 2f;
            var body = new RectangleF(a.X + a.Width * 0.14f, a.Y + a.Height * 0.2f,
                a.Width * 0.72f, a.Height * 0.56f);
            g.Box(body, 8f, HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.5f);
            // 观察窗
            var win = new RectangleF(body.X + 10f, body.Y + 10f, body.Width - 20f, body.Height * 0.56f);
            g.Box(win, 3f, Color.FromArgb(46, 50, 60), pal.Edge, 1.2f);
            // 加热台 + 工件
            var stage = new RectangleF(win.X + 8f, win.Bottom - 12f, win.Width - 16f, 10f);
            HeatGlow(g, stage, 50);
            g.Box(stage, 1f, Color.FromArgb(150, 106, 64), pal.Edge, 1f);
            Die(g, new RectangleF(cx - 8f, stage.Y - 8f, 16f, 8f));
            // 甲酸氛围（淡绿雾）/ 真空
            if (formic && Running)
                using (var gas = new SolidBrush(Color.FromArgb(40, 150, 220, 130)))
                    g.FillRectangle(gas, win.X + 2f, win.Y + 2f, win.Width - 4f, win.Height - 4f);
            // 表 + 气口
            g.Ell(body.X + 8f, body.Bottom - 18f, 6f, 6f, Color.White);
            g.DrawEllipse(new Pen(pal.Edge, 1f), body.X + 2f, body.Bottom - 24f, 12f, 12f);
            g.Line(pal.MetalDark, 2.6f, body.Right, body.Y + 12f, a.Right - 6f, body.Y + 4f);
            if (formic)
                g.Line(Color.FromArgb(120, 160, 120), 2.2f, body.Left - 8f, body.Y + 16f,
                    body.Left, body.Y + 16f);
            g.Lamp(body.Right - 12f, body.Y + 6f, Running);
        }
        // ----------------------------------------------------------------
        // Press：热压 / 银烧结压机（上下热板压头 + 力指示）
        // ----------------------------------------------------------------
        private void DrawPress(Graphics g, RectangleF a, HToolPalette pal, bool sinter)
        {
            float cx = a.X + a.Width / 2f;
            // 框架（C 形/四柱）
            using (var fr = new Pen(pal.MetalDark, 5f))
            {
                g.DrawLine(fr, a.X + a.Width * 0.16f, a.Y + a.Height * 0.12f,
                    a.X + a.Width * 0.16f, a.Bottom - a.Height * 0.12f);
                g.DrawLine(fr, a.Right - a.Width * 0.16f, a.Y + a.Height * 0.12f,
                    a.Right - a.Width * 0.16f, a.Bottom - a.Height * 0.12f);
                g.DrawLine(fr, a.X + a.Width * 0.16f, a.Y + a.Height * 0.12f,
                    a.Right - a.Width * 0.16f, a.Y + a.Height * 0.12f);
            }
            // 下加热台
            var lower = new RectangleF(cx - a.Width * 0.26f, a.Bottom - a.Height * 0.26f,
                a.Width * 0.52f, a.Height * 0.08f);
            g.Box(lower, 2f, Color.FromArgb(150, 106, 64), pal.Edge, 1.2f);
            HeatGlow(g, lower, 40);
            // 工件（芯片/银烧结片）
            Die(g, new RectangleF(cx - 12f, lower.Y - 10f, 24f, 10f));
            if (sinter)
                using (var sl = new SolidBrush(Color.FromArgb(180, 200, 200, 196)))
                    g.FillRectangle(sl, cx - 14f, lower.Y - 13f, 28f, 3f);
            // 上压头（下压）
            float press = Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) : 0.3f;
            float upY = a.Y + a.Height * 0.22f + press * a.Height * 0.26f;
            var upper = new RectangleF(cx - a.Width * 0.26f, upY, a.Width * 0.52f, a.Height * 0.08f);
            HeatGlow(g, upper, sinter ? 60 : 40);
            g.Box(upper, 2f, Color.FromArgb(150, 106, 64), pal.Edge, 1.2f);
            // 油缸
            g.Box(new RectangleF(cx - 12f, a.Y + a.Height * 0.12f, 24f, upY - a.Y - a.Height * 0.12f),
                3f, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.2f);
            g.Lamp(a.X + a.Width * 0.2f, a.Y + a.Height * 0.16f, Running);
        }
        // ----------------------------------------------------------------
        // Ultrasonic：超声金属焊机（换能器 + 楔形焊头 + 铝带）
        // ----------------------------------------------------------------
        private void DrawUltrasonic(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.18f,
                a.Width * 0.8f, a.Height * 0.12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // 换能器横筒（水平）
            float cy = a.Y + a.Height * 0.36f;
            var horn = new RectangleF(a.X + a.Width * 0.14f, cy - 11f, a.Width * 0.5f, 22f);
            g.FillCylH(horn, HToolPalettes.Shade(pal.Main, 0.26f), HToolPalettes.Tint(pal.Main, 0.22f));
            g.DrawRectangle(new Pen(pal.Edge, 1.2f), horn.X, horn.Y, horn.Width, horn.Height);
            // 收窄变幅杆 + 楔形头
            g.Poly(new[]
            {
                new PointF(horn.Right, cy - 8f), new PointF(horn.Right + a.Width * 0.16f, cy - 3f),
                new PointF(horn.Right + a.Width * 0.16f, cy + 3f), new PointF(horn.Right, cy + 8f)
            }, pal.MetalDark, pal.Edge, 1.1f);
            float wx = horn.Right + a.Width * 0.16f;
            // 下压
            float bob = Running ? (float)(Math.Sin(Phase * 2.4f) * 0.5 + 0.5) * 6f : 0f;
            // 工件 + 铝带
            var work = new RectangleF(a.X + a.Width * 0.3f, a.Bottom - a.Height * 0.28f,
                a.Width * 0.5f, a.Height * 0.1f);
            g.Box(work, 2f, pal.Metal, pal.Edge, 1.1f);
            using (var ribbon = new SolidBrush(Color.FromArgb(210, 214, 220)))
                g.FillRectangle(ribbon, wx - 12f, work.Y - 4f + bob, 30f, 4f);
            // 超声振动纹
            if (Running)
                using (var wave = new Pen(Color.FromArgb(170, pal.Accent), 1.3f))
                    for (int i = 0; i < 3; i++)
                        g.DrawArc(wave, horn.X - 6f + i * 8f, cy - 16f, 8f, 8f, 0f, 180f);
            g.Lamp(horn.X + 8f, horn.Y + 5f, Running);
        }
        // ----------------------------------------------------------------
        // LaserSolder：激光焊接头（光束 + 焊盘 + 送丝）
        // ----------------------------------------------------------------
        private void DrawLaserSolder(Graphics g, RectangleF a, HToolPalette pal)
        {
            Gantry(g, a, pal, 0.55f);
            float hx = a.X + a.Width / 2f;
            g.Box(new RectangleF(hx - 11f, a.Y + a.Height * 0.18f, 22f, 18f), 3f,
                HToolPalettes.Shade(pal.Main, 0.16f), pal.Edge, 1.2f);
            g.Line(pal.MetalDark, 3f, hx, a.Y + a.Height * 0.17f, hx, a.Y + a.Height * 0.18f);
            // 工件板
            var board = new RectangleF(a.X + a.Width * 0.2f, a.Bottom - a.Height * 0.24f,
                a.Width * 0.6f, a.Height * 0.1f);
            g.Box(board, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            float spotY = board.Y;
            // 激光束（扫描抖动）
            float jitter = Running ? (float)Math.Sin(Phase * 5f) * 3f : 0f;
            using (var beam = new Pen(Color.FromArgb(200, 255, 60, 40), 2.4f))
                g.DrawLine(beam, hx, a.Y + a.Height * 0.36f, hx + jitter, spotY);
            if (Running)
                using (var gl = new SolidBrush(Color.FromArgb(90, 255, 80, 40)))
                    g.FillEllipse(gl, hx - 9f + jitter, spotY - 3f, 18f, 8f);
            // 焊锡丝（斜送）
            g.Line(Color.FromArgb(200, 204, 208), 1.8f, hx + 16f, a.Y + a.Height * 0.24f,
                hx + jitter + 3f, spotY - 2f);
            // 焊点亮斑
            using (var solder = new SolidBrush(Color.FromArgb(220, 224, 230)))
                g.FillEllipse(solder, hx - 3f + jitter, spotY - 2f, 6f, 4f);
        }
        // ----------------------------------------------------------------
        // PasteAttach：锡膏点胶固晶（点胶头 + 胶点阵列 + 芯片待贴）
        // ----------------------------------------------------------------
        private void DrawPasteAttach(Graphics g, RectangleF a, HToolPalette pal)
        {
            Gantry(g, a, pal, 0.6f);
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.16f,
                a.Width * 0.8f, a.Height * 0.1f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            var sub = new RectangleF(a.X + a.Width * 0.24f, a.Bottom - a.Height * 0.28f,
                a.Width * 0.52f, a.Height * 0.1f);
            g.Box(sub, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            // 横移点胶头
            float move = Running ? Phase / (float)Math.PI * 0.5f % 1f : 0.3f;
            float hx = sub.X + 6f + move * (sub.Width - 12f);
            float hy = a.Y + a.Height * 0.24f;
            g.Box(new RectangleF(hx - 7f, hy, 14f, 12f), 2f,
                HToolPalettes.Shade(pal.Main, 0.14f), pal.Edge, 1.1f);
            g.Line(Color.FromArgb(90, 94, 100), 2f, hx, hy + 12f, hx, sub.Y - 2f);
            // 已点胶点
            using (var paste = new SolidBrush(Color.FromArgb(180, 150, 150, 150)))
                for (int i = 0; i < 6; i++)
                {
                    float px = sub.X + 8f + i * (sub.Width - 16f) / 5f;
                    float frac = (px - sub.X - 6f) / (sub.Width - 12f);
                    if (frac < move)
                        g.FillEllipse(paste, px - 2.4f, sub.Y - 3f, 4.8f, 3.6f);
                }
        }
        // ----------------------------------------------------------------
        // WaferBond / HybridBond：晶圆键合机（上下大圆盘对准压合）
        // ----------------------------------------------------------------
        private void DrawWaferBond(Graphics g, RectangleF a, HToolPalette pal, bool hybrid)
        {
            float cx = a.X + a.Width / 2f;
            // 框架
            g.Line(pal.MetalDark, 5f, a.X + a.Width * 0.14f, a.Y + a.Height * 0.1f,
                a.X + a.Width * 0.14f, a.Bottom - a.Height * 0.1f);
            g.Line(pal.MetalDark, 5f, a.Right - a.Width * 0.14f, a.Y + a.Height * 0.1f,
                a.Right - a.Width * 0.14f, a.Bottom - a.Height * 0.1f);
            g.Line(pal.MetalDark, 5f, a.X + a.Width * 0.14f, a.Y + a.Height * 0.1f,
                a.Right - a.Width * 0.14f, a.Y + a.Height * 0.1f);
            float r = a.Width * 0.26f;
            // 下吸盘 + 晶圆
            float ly = a.Bottom - a.Height * 0.26f;
            g.Ell(cx, ly, r, r * 0.22f, pal.MetalDark);
            g.Ell(cx, ly, r * 0.92f, r * 0.18f, Color.FromArgb(160, 178, 224));
            // 上吸盘（下降压合）
            float close = Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) : 0.3f;
            float uy = a.Y + a.Height * 0.22f + close * (ly - a.Height * 0.12f - r * 0.5f);
            g.Ell(cx, uy, r, r * 0.22f, pal.MetalDark);
            g.Ell(cx, uy, r * 0.92f, r * 0.18f, Color.FromArgb(160, 178, 224));
            g.Box(new RectangleF(cx - 14f, a.Y + a.Height * 0.1f, 28f, uy - a.Y - a.Height * 0.1f),
                2f, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.1f);
            // 对位标记 + 混合键合等离子标识
            using (var mark = new Pen(Color.FromArgb(200, 90, 90), 1.2f))
            {
                g.DrawLine(mark, cx - r * 0.6f, ly - 1f, cx - r * 0.6f + 6f, ly - 1f);
                g.DrawLine(mark, cx - r * 0.6f + 3f, ly - 4f, cx - r * 0.6f + 3f, ly + 2f);
            }
            if (hybrid && Running)
                using (var plasma = new SolidBrush(Color.FromArgb(50, 150, 110, 230)))
                    g.FillEllipse(plasma, cx - r * 0.8f, (uy + ly) / 2f - 8f, r * 1.6f, 16f);
            g.Lamp(a.X + a.Width * 0.18f, a.Y + a.Height * 0.14f, Running);
        }
        // ----------------------------------------------------------------
        // Anodic：阳极键合（高压电极 + 玻璃/硅叠片）
        // ----------------------------------------------------------------
        private void DrawAnodic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width / 2f;
            // 绝缘立柱 + 上下电极板
            g.Line(Color.FromArgb(120, 110, 90), 4f, a.X + a.Width * 0.2f, a.Y + a.Height * 0.14f,
                a.X + a.Width * 0.2f, a.Bottom - a.Height * 0.14f);
            g.Line(Color.FromArgb(120, 110, 90), 4f, a.Right - a.Width * 0.2f, a.Y + a.Height * 0.14f,
                a.Right - a.Width * 0.2f, a.Bottom - a.Height * 0.14f);
            var lower = new RectangleF(cx - a.Width * 0.28f, a.Bottom - a.Height * 0.3f,
                a.Width * 0.56f, a.Height * 0.07f);
            var upper = new RectangleF(cx - a.Width * 0.28f, a.Y + a.Height * 0.34f,
                a.Width * 0.56f, a.Height * 0.07f);
            g.Box(lower, 2f, Color.FromArgb(150, 110, 70), pal.Edge, 1.2f);
            g.Box(upper, 2f, Color.FromArgb(150, 110, 70), pal.Edge, 1.2f);
            // 玻璃片（半透明）+ 硅片
            g.Box(new RectangleF(cx - a.Width * 0.2f, lower.Y - 8f, a.Width * 0.4f, 6f), 1f,
                Color.FromArgb(120, 170, 210, 200), Color.FromArgb(120, 140, 160), 1f);
            Die(g, new RectangleF(cx - a.Width * 0.2f, upper.Bottom + 2f, a.Width * 0.4f, 6f));
            // 高压标 + 电场线
            using (var hv = new Font("Microsoft YaHei", 7f, FontStyle.Bold))
                g.DrawString("HV", hv, Brushes.Crimson, upper.X + 4f, upper.Y - 12f);
            if (Running)
                using (var field = new Pen(Color.FromArgb(120, 120, 180, 255), 1.1f)
                { DashStyle = DashStyle.Dash })
                    for (int i = 0; i < 4; i++)
                    {
                        float x = cx - a.Width * 0.15f + i * a.Width * 0.1f;
                        g.DrawLine(field, x, upper.Bottom + 2f, x, lower.Y - 10f);
                    }
            // 高压电源
            g.Box(new RectangleF(a.Right - a.Width * 0.16f, a.Bottom - a.Height * 0.14f,
                a.Width * 0.12f, a.Height * 0.1f), 2f, Color.FromArgb(40, 46, 56), pal.Edge, 1.1f);
        }
        // ----------------------------------------------------------------
        // HotBar：脉冲热压焊（热压头 + FPC + 焊锡）
        // ----------------------------------------------------------------
        private void DrawHotBar(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.X + a.Width / 2f;
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.18f,
                a.Width * 0.8f, a.Height * 0.12f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // PCB + FPC
            var pcb = new RectangleF(a.X + a.Width * 0.16f, a.Bottom - a.Height * 0.3f,
                a.Width * 0.68f, a.Height * 0.1f);
            g.Box(pcb, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            g.Poly(new[]
            {
                new PointF(pcb.Right, pcb.Y - 2f), new PointF(a.Right - 2f, pcb.Y + 2f),
                new PointF(a.Right - 2f, pcb.Bottom - 2f), new PointF(pcb.Right, pcb.Bottom + 2f)
            }, Color.FromArgb(190, 160, 80), pal.Edge, 1f);
            // 压头（长条热刀，下压）
            float bob = Running ? (float)(Math.Sin(Phase * 2f) * 0.5 + 0.5) * a.Height * 0.08f : 0f;
            float hy = a.Y + a.Height * 0.34f + bob;
            var bar = new RectangleF(pcb.X + 4f, hy, pcb.Width - 8f, 8f);
            if (Running)
                using (var glow = new SolidBrush(Color.FromArgb(70, 255, 120, 40)))
                    g.FillRectangle(glow, bar.X - 4f, bar.Y - 3f, bar.Width + 8f, bar.Height + 6f);
            g.Box(bar, 1f, Color.FromArgb(180, 130, 70), pal.Edge, 1.1f);
            g.Line(pal.MetalDark, 4f, cx, a.Y + a.Height * 0.18f, cx, hy);
            g.Box(new RectangleF(cx - 10f, a.Y + a.Height * 0.12f, 20f, 10f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            // 压头排线
            using (var lead = new Pen(Color.FromArgb(200, pal.Metal), 1f))
                for (int i = 0; i < 8; i++)
                {
                    float x = bar.X + 4f + i * (bar.Width - 8f) / 7f;
                    g.DrawLine(lead, x, bar.Bottom, x, bar.Bottom + 3f);
                }
        }
        // ----------------------------------------------------------------
        // SolderRobot：焊锡机器人（烙铁头 + 锡丝 + 青烟）
        // ----------------------------------------------------------------
        private void DrawSolderRobot(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 工作台 + PCB
            g.Box(new RectangleF(a.X + 8f, a.Bottom - a.Height * 0.18f, a.Width - 16f, a.Height * 0.12f),
                3f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            var pcb = new RectangleF(a.X + a.Width * 0.22f, a.Bottom - a.Height * 0.28f,
                a.Width * 0.56f, a.Height * 0.1f);
            g.Box(pcb, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            // 四轴关节臂
            float j0x = a.X + a.Width * 0.24f, j0y = a.Y + a.Height * 0.24f;
            g.Ell(j0x, j0y, 5f, 5f, pal.MetalDark);
            float a1 = Running ? -0.6f + (float)Math.Sin(Phase * 0.8f) * 0.15f : -0.6f;
            float j1x = j0x + (float)Math.Cos(a1) * a.Width * 0.28f;
            float j1y = j0y - (float)Math.Sin(a1) * a.Width * 0.28f;
            g.Ell(j1x, j1y, 4f, 4f, pal.MetalDark);
            float a2 = a1 + (Running ? 1.1f + (float)Math.Sin(Phase) * 0.2f : 1.1f);
            float tx = j1x + (float)Math.Cos(a2) * a.Width * 0.22f;
            float ty = j1y - (float)Math.Sin(a2) * a.Width * 0.22f;
            using (var arm = new Pen(HToolPalettes.Shade(pal.Main, 0.1f), 5f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(arm, j0x, j0y, j1x, j1y);
                g.DrawLine(arm, j1x, j1y, tx, ty);
            }
            // 烙铁头（发热尖）
            g.Line(Color.FromArgb(90, 84, 70), 3f, tx, ty, tx + 8f, ty + 14f);
            if (Running)
                using (var heat = new SolidBrush(Color.FromArgb(150, 255, 110, 40)))
                    g.FillEllipse(heat, tx + 4f, ty + 10f, 9f, 7f);
            // 锡丝斜送
            g.Line(Color.FromArgb(200, 204, 208), 1.8f, j1x + 4f, j1y, tx + 10f, ty + 12f);
            // 青烟
            if (Running)
                using (var smoke = new Pen(Color.FromArgb(70, 200, 200, 205), 1.2f))
                    for (int i = 0; i < 3; i++)
                    {
                        float t = (Phase / (float)Math.PI * 0.5f + i * 0.3f) % 1f;
                        float sx = tx + 10f + (float)Math.Sin(t * 6f) * 4f;
                        g.DrawLine(smoke, sx, ty + 12f - t * 22f,
                            sx + 3f, ty + 8f - t * 22f);
                    }
        }
        // ----------------------------------------------------------------
        // Wave / Selective：波峰焊（锡锅 + 涌动锡波 + 传送 PCB）
        // ----------------------------------------------------------------
        private void DrawWave(Graphics g, RectangleF a, HToolPalette pal, bool selective)
        {
            // 锡锅
            float potX = selective ? a.X + a.Width * 0.3f : a.X + a.Width * 0.1f;
            float potW = selective ? a.Width * 0.4f : a.Width * 0.8f;
            var pot = new RectangleF(potX, a.Bottom - a.Height * 0.34f, potW, a.Height * 0.22f);
            g.Box(pot, 3f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // 熔锡液面
            using (var solder = new SolidBrush(Color.FromArgb(225, 218, 200)))
                g.FillRectangle(solder, pot.X + 3f, pot.Bottom - pot.Height * 0.4f,
                    pot.Width - 6f, pot.Height * 0.4f - 3f);
            // 锡波（涌动）
            float waveH = a.Height * 0.14f;
            int waves = selective ? 1 : 3;
            using (var wave = new SolidBrush(Color.FromArgb(235, 228, 208)))
                for (int i = 0; i < waves; i++)
                {
                    float wx0 = pot.X + pot.Width * (selective ? 0.32f : (0.16f + 0.34f * i));
                    float ww = pot.Width * (selective ? 0.36f : 0.24f);
                    float wob = Running ? (float)Math.Sin(Phase * 3f + i) * 3f : 0f;
                    g.FillPolygon(wave, new[]
                    {
                        new PointF(wx0, pot.Bottom - pot.Height * 0.4f),
                        new PointF(wx0 + ww, pot.Bottom - pot.Height * 0.4f),
                        new PointF(wx0 + ww * 0.5f, pot.Bottom - pot.Height * 0.4f - waveH + wob)
                    });
                }
            // 传送导轨（斜向）+ PCB
            float railY1 = a.Y + a.Height * 0.24f, railY2 = a.Bottom - a.Height * 0.46f;
            g.Line(pal.MetalDark, 2.6f, a.X + 8f, railY1, a.Right - 8f, railY2);
            g.Line(pal.MetalDark, 2.6f, a.X + 8f, railY1 + 8f, a.Right - 8f, railY2 + 8f);
            float px = Running ? a.X + 14f + (Phase / (float)Math.PI * 0.5f % 1f) * (a.Width - 40f)
                : a.X + a.Width * 0.5f;
            float py = railY1 + (px - a.X - 8f) / (a.Width - 16f) * (railY2 - railY1) + 2f;
            g.Box(new RectangleF(px - 16f, py, 32f, 7f), 1f,
                Color.FromArgb(70, 130, 90), pal.Edge, 1f);
            // 烟
            if (Running)
                using (var fume = new Pen(Color.FromArgb(60, 200, 200, 205), 1.1f))
                    g.DrawLine(fume, pot.X + pot.Width / 2f, pot.Y + 4f,
                        pot.X + pot.Width / 2f + 4f, pot.Y - 8f);
        }
        // ----------------------------------------------------------------
        // PickPlace：泛用贴片机（飞达 + 移动贴装头 + 料站）
        // ----------------------------------------------------------------
        private void DrawPickPlace(Graphics g, RectangleF a, HToolPalette pal)
        {
            g.Box(new RectangleF(a.X + 8f, a.Bottom - a.Height * 0.16f, a.Width - 16f, a.Height * 0.1f),
                3f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // PCB
            var pcb = new RectangleF(a.X + a.Width * 0.3f, a.Bottom - a.Height * 0.26f,
                a.Width * 0.5f, a.Height * 0.09f);
            g.Box(pcb, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            // 前置飞达排
            for (int i = 0; i < 4; i++)
                g.Box(new RectangleF(a.X + 10f + i * a.Width * 0.12f, a.Bottom - a.Height * 0.34f,
                    a.Width * 0.1f, 6f), 1f, pal.Metal, pal.Edge, 0.9f);
            // 拱架 + 横移头
            g.Line(pal.MetalDark, 4f, a.X + 10f, a.Y + a.Height * 0.16f,
                a.Right - 10f, a.Y + a.Height * 0.16f);
            float move = Running ? Phase / (float)Math.PI * 0.5f % 1f : 0.4f;
            float hx = a.X + 16f + move * (a.Width - 32f);
            float bob = Running ? (float)(Math.Sin(Phase * 3f) * 0.5 + 0.5) * a.Height * 0.06f : 0f;
            g.Box(new RectangleF(hx - 9f, a.Y + a.Height * 0.13f, 18f, 10f), 2f,
                HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.1f);
            g.Line(pal.Metal, 2.4f, hx, a.Y + a.Height * 0.23f, hx, a.Y + a.Height * 0.34f + bob);
            // 吸嘴上的芯片
            Die(g, new RectangleF(hx - 6f, a.Y + a.Height * 0.34f + bob, 12f, 8f));
            // 视觉相机
            g.Ell(hx + 14f, a.Y + a.Height * 0.26f, 4f, 4f, Color.FromArgb(40, 60, 90));
        }
        // ----------------------------------------------------------------
        // Preform：焊片贴装机（料带冲切 + 真空吸头放置预成型焊片）
        // ----------------------------------------------------------------
        private void DrawPreform(Graphics g, RectangleF a, HToolPalette pal)
        {
            Gantry(g, a, pal, 0.6f);
            g.Box(new RectangleF(a.X + a.Width * 0.1f, a.Bottom - a.Height * 0.16f,
                a.Width * 0.8f, a.Height * 0.1f), 3f,
                HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            // 基板
            var sub = new RectangleF(a.X + a.Width * 0.44f, a.Bottom - a.Height * 0.3f,
                a.Width * 0.42f, a.Height * 0.1f);
            g.Box(sub, 2f, Color.FromArgb(70, 130, 90), pal.Edge, 1.1f);
            // 左侧料带 + 冲切座
            g.Box(new RectangleF(a.X + 8f, a.Y + a.Height * 0.44f, a.Width * 0.26f, a.Height * 0.14f),
                2f, pal.MetalDark, pal.Edge, 1.1f);
            using (var tape = new Pen(Color.FromArgb(140, 120, 70), 3f))
                g.DrawLine(tape, a.X + 4f, a.Y + a.Height * 0.6f,
                    a.X + 8f + a.Width * 0.26f, a.Y + a.Height * 0.6f);
            // 焊片（料带上小方块）
            using (var pre = new SolidBrush(Color.FromArgb(225, 210, 150)))
                for (int i = 0; i < 3; i++)
                    g.FillRectangle(pre, a.X + 12f + i * 11f, a.Y + a.Height * 0.585f, 7f, 5f);
            // 吸头（在料带与基板间横移 + 上下）
            float move = Running ? (float)(Math.Sin(Phase) * 0.5 + 0.5) : 0.5f;
            float hx = a.X + a.Width * (0.22f + 0.34f * move);
            float bob = (float)(Math.Abs(Math.Cos(Phase)) ) * a.Height * 0.05f;
            if (!Running) bob = a.Height * 0.03f;
            g.Line(pal.MetalDark, 3f, hx, a.Y + a.Height * 0.17f, hx, a.Y + a.Height * 0.34f + bob);
            g.Box(new RectangleF(hx - 8f, a.Y + a.Height * 0.15f, 16f, 9f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            using (var pre = new SolidBrush(Color.FromArgb(225, 210, 150)))
                g.FillRectangle(pre, hx - 5f, a.Y + a.Height * 0.34f + bob, 10f, 6f);
        }
    }
}