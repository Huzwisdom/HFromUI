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
    /// 机房样式：Electrical / Central 为已验收经典外形（绘制原样保留），
    /// 其余 20 种为新式工业厂房与机房外形。
    /// </summary>
    public enum HRoomStyle
    {
        Electrical = 0,       // 电气室（经典）
        Central = 1,          // 中控室（经典）
        Substation = 2,       // 户外变电站
        Switchgear = 3,       // 高压开关柜室
        Transformer = 4,      // 箱式变电站
        GeneratorHouse = 5,   // 发电机房
        BatteryRoom = 6,      // 蓄电池室
        UpsRoom = 7,          // UPS 电源室
        CabinetHall = 8,      // 控制柜阵列间
        ServerRoom = 9,       // 数据机房
        PumpHouse = 10,       // 水泵房
        CompressorHouse = 11, // 空压机房
        BoilerHouse = 12,     // 锅炉房
        ChillerPlant = 13,    // 冷冻机房
        HvacPlant = 14,       // 空调机房
        CleanRoom = 15,       // 洁净车间
        Laboratory = 16,      // 化验室
        Warehouse = 17,       // 库房
        LoadingDock = 18,     // 装卸月台
        GuardHouse = 19,      // 门卫室
        WatchTower = 20,      // 岗塔楼
        FirePumpHouse = 21    // 消防泵房
    }
    /// <summary>
    /// 机房控件（继承 HToolAnimBase）：22 种厂房/机房外形、13 种色调；
    /// Electrical 为带闪电符号与配电柜的电气室，Central 为带天线与显示屏的中控室。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("机房控件：22 种厂房机房外形、13 种色调")]
    public class HRoom : HToolAnimBase
    {
        private HRoomStyle _style = HRoomStyle.Electrical;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HRoom()
        {
            Size = new Size(150, 140);
        }
        /// <summary>机房外形样式。</summary>
        [HCategoryLanguage("房间"), HDisplayNameLanguage("机房外形样式"), HDescriptionLanguage("机房外形样式"), Browsable(true)]
        [DefaultValue(HRoomStyle.Electrical)]
        public HRoomStyle RoomStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
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
                case HRoomStyle.Substation: DrawSubstation(g, a, pal); break;
                case HRoomStyle.Switchgear: DrawSwitchgear(g, a, pal); break;
                case HRoomStyle.Transformer: DrawTransformer(g, a, pal); break;
                case HRoomStyle.GeneratorHouse: DrawGenerator(g, a, pal); break;
                case HRoomStyle.BatteryRoom: DrawBattery(g, a, pal); break;
                case HRoomStyle.UpsRoom: DrawUps(g, a, pal); break;
                case HRoomStyle.CabinetHall: DrawCabinetHall(g, a, pal); break;
                case HRoomStyle.ServerRoom: DrawServer(g, a, pal); break;
                case HRoomStyle.PumpHouse: DrawPump(g, a, pal); break;
                case HRoomStyle.CompressorHouse: DrawCompressor(g, a, pal); break;
                case HRoomStyle.BoilerHouse: DrawBoiler(g, a, pal); break;
                case HRoomStyle.ChillerPlant: DrawChiller(g, a, pal); break;
                case HRoomStyle.HvacPlant: DrawHvac(g, a, pal); break;
                case HRoomStyle.CleanRoom: DrawClean(g, a, pal); break;
                case HRoomStyle.Laboratory: DrawLaboratory(g, a, pal); break;
                case HRoomStyle.Warehouse: DrawWarehouse(g, a, pal); break;
                case HRoomStyle.LoadingDock: DrawDock(g, a, pal); break;
                case HRoomStyle.GuardHouse: DrawGuard(g, a, pal); break;
                case HRoomStyle.WatchTower: DrawTower(g, a, pal); break;
                case HRoomStyle.FirePumpHouse: DrawFirePump(g, a, pal); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：电气室 / 中控室（已验收绘法，原样保留）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width, glyphH = a.Height;
            float w = Width * 0.8f, x0 = (Width - w) / 2f;
            float wallTop = glyphH * 0.32f, wallBot = glyphH * 0.9f;
            // 墙身
            using (var wall = new LinearGradientBrush(new RectangleF(x0, wallTop, w, wallBot - wallTop),
                HToolPalettes.Tint(pal.Main, 0.35f), HToolPalettes.Shade(pal.Main, 0.15f), 90f))
                g.FillRectangle(wall, x0, wallTop, w, wallBot - wallTop);
            g.DrawRectangle(new Pen(pal.Edge, 1.5f), x0, wallTop, w, wallBot - wallTop);
            // 双坡屋顶
            var roof = new[]
            {
                new PointF(x0 - 8f, wallTop + 2f),
                new PointF(x0 + w / 2f, glyphH * 0.08f),
                new PointF(x0 + w + 8f, wallTop + 2f)
            };
            using (var rb = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.35f)))
                g.FillPolygon(rb, roof);
            g.DrawPolygon(new Pen(pal.Edge, 1.5f), roof);
            if (_style == HRoomStyle.Electrical)
            {
                // 门
                var door = new RectangleF(x0 + w * 0.12f, wallTop + w * 0.3f, w * 0.22f,
                    wallBot - (wallTop + w * 0.3f));
                using (var db = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.25f)))
                    g.FillRectangle(db, door);
                g.DrawRectangle(new Pen(pal.Edge, 1.2f), door.X, door.Y, door.Width, door.Height);
                // 配电柜 + 闪电
                var cab = new RectangleF(x0 + w * 0.5f, wallTop + w * 0.16f, w * 0.34f, w * 0.34f);
                using (var cb = new LinearGradientBrush(cab, Color.FromArgb(232, 234, 238),
                    Color.FromArgb(168, 172, 180), 90f))
                    g.FillRectangle(cb, cab);
                g.DrawRectangle(new Pen(pal.Edge, 1.2f), cab.X, cab.Y, cab.Width, cab.Height);
                var bolt = new[]
                {
                    new PointF(cab.X + cab.Width * 0.58f, cab.Y + 4f),
                    new PointF(cab.X + cab.Width * 0.36f, cab.Y + cab.Height * 0.52f),
                    new PointF(cab.X + cab.Width * 0.52f, cab.Y + cab.Height * 0.52f),
                    new PointF(cab.X + cab.Width * 0.4f, cab.Bottom - 4f),
                    new PointF(cab.X + cab.Width * 0.68f, cab.Y + cab.Height * 0.44f),
                    new PointF(cab.X + cab.Width * 0.52f, cab.Y + cab.Height * 0.44f)
                };
                g.FillPolygon(Brushes.Gold, bolt);
            }
            else
            {
                // 中控室：宽窗 + 三块监视屏
                var win = new RectangleF(x0 + w * 0.14f, wallTop + w * 0.14f, w * 0.72f, w * 0.34f);
                using (var wb = new SolidBrush(Color.FromArgb(40, 52, 72)))
                    g.FillRectangle(wb, win);
                g.DrawRectangle(new Pen(pal.Edge, 1.2f), win.X, win.Y, win.Width, win.Height);
                using (var scr = new SolidBrush(Color.FromArgb(70, 130, 200)))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var sw = win.Width / 3f - 5f;
                        var sr = new RectangleF(win.X + 3f + i * (sw + 5f), win.Y + 4f, sw, win.Height - 8f);
                        g.FillRectangle(scr, sr);
                        g.DrawRectangle(new Pen(Color.FromArgb(160, 200, 240), 1f), sr.X, sr.Y, sr.Width, sr.Height);
                    }
                }
                // 门
                var door = new RectangleF(x0 + w * 0.4f, wallTop + w * 0.55f, w * 0.2f,
                    wallBot - (wallTop + w * 0.55f));
                using (var db = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.25f)))
                    g.FillRectangle(db, door);
                g.DrawRectangle(new Pen(pal.Edge, 1.2f), door.X, door.Y, door.Width, door.Height);
                // 屋顶天线
                using (var ant = new Pen(Color.FromArgb(60, 64, 70), 2f))
                {
                    float ax = x0 + w * 0.78f;
                    g.DrawLine(ant, ax, wallTop, ax, glyphH * 0.12f);
                    g.DrawLine(ant, ax - 6f, glyphH * 0.18f, ax + 6f, glyphH * 0.18f);
                }
            }
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // 新式厂房
        // ----------------------------------------------------------------
        // 户外变电站：门构 + 绝缘子 + 变压器
        private void DrawSubstation(Graphics g, RectangleF a, HToolPalette pal)
        {
            float gy = a.Bottom - a.Height * 0.06f;
            g.Line(pal.Edge, 2f, a.Left + 4f, gy, a.Right - 4f, gy);
            // 门形构架
            float mx1 = a.Left + a.Width * 0.14f, mx2 = a.Right - a.Width * 0.14f, beamY = a.Top + a.Height * 0.12f;
            g.Line(pal.MetalDark, 4f, mx1, beamY, mx1, gy);
            g.Line(pal.MetalDark, 4f, mx2, beamY, mx2, gy);
            g.Line(pal.MetalDark, 5f, mx1, beamY, mx2, beamY);
            // 三串悬挂绝缘子
            for (int i = 0; i < 3; i++)
            {
                float ix = mx1 + (mx2 - mx1) * (0.22f + i * 0.28f);
                g.Line(pal.Edge, 1.6f, ix, beamY, ix, beamY + 12f);
                for (int k = 0; k < 3; k++)
                    g.Ell(ix, beamY + 6f + k * 4f, 2.4f, 2.4f, Color.FromArgb(150, 130, 120));
            }
            // 变压器（油箱 + 散热器 + 套管）
            float cx = a.Left + a.Width * 0.5f;
            var tank = new RectangleF(cx - a.Width * 0.2f, gy - a.Height * 0.3f, a.Width * 0.4f, a.Height * 0.26f);
            for (int s = -1; s <= 1; s += 2)
                using (var fin = new Pen(HToolPalettes.Shade(pal.Main, 0.18f), 2f))
                    for (int k = 0; k < 4; k++)
                        g.DrawLine(fin, tank.Left + s * 2f + (s < 0 ? -k * 3f - 3f : tank.Width + k * 3f + 1f),
                            tank.Top + 3f, tank.Left + (s < 0 ? -k * 3f - 3f : tank.Width + k * 3f + 1f), tank.Bottom - 3f);
            g.Box(tank, 3f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.5f);
            for (int i = 0; i < 3; i++)
            {
                float bx = tank.Left + tank.Width * (0.28f + i * 0.22f);
                g.Line(pal.Edge, 2.2f, bx, tank.Top - 8f, bx, tank.Top);
                g.Ell(bx, tank.Top - 10f, 4f, 4f, Color.FromArgb(150, 130, 120));
            }
        }
        // 高压开关柜室：室内成列柜体
        private void DrawSwitchgear(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.9f, false);
            int n = 5;
            float gap = 3f, cw = (r.Width - gap * (n + 1)) / n;
            for (int i = 0; i < n; i++)
            {
                var cr = new RectangleF(r.Left + gap + i * (cw + gap), r.Top + r.Height * 0.16f,
                    cw, r.Height * 0.72f);
                g.Box(cr, 2f, pal.Metal, pal.Edge, 1.2f);
                g.Ell(cr.Left + cw * 0.5f, cr.Top + 9f, 2.6f, 2.6f, Color.FromArgb(80, 200, 120));
                using (var vent = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.2f))
                    for (int k = 0; k < 3; k++)
                        g.DrawLine(vent, cr.Left + 4f, cr.Bottom - 14f + k * 4f, cr.Right - 4f, cr.Bottom - 14f + k * 4f);
                g.Line(pal.Edge, 1.4f, cr.Right - 4f, cr.Top + cr.Height * 0.4f, cr.Right - 4f, cr.Top + cr.Height * 0.62f);
            }
            // 屋顶避雷短针
            g.Line(pal.Edge, 2f, r.Left + r.Width * 0.5f, r.Top - 7f, r.Left + r.Width * 0.5f, r.Top - 1f);
        }
        // 箱式变电站：户外箱体 + 百叶 + 高压套管
        private void DrawTransformer(Graphics g, RectangleF a, HToolPalette pal)
        {
            float gy = a.Bottom - a.Height * 0.08f;
            g.Box(new RectangleF(a.Left + a.Width * 0.14f, gy - 5f, a.Width * 0.72f, 7f),
                2f, pal.MetalDark, pal.Edge, 1.2f);
            var r = new RectangleF(a.Left + a.Width * 0.18f, a.Top + a.Height * 0.3f,
                a.Width * 0.64f, gy - (a.Top + a.Height * 0.3f) - 5f);
            g.Box(r, 4f, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.6f);
            g.Box(new RectangleF(r.X - 5f, r.Top - 6f, r.Width + 10f, 8f), 2f,
                HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.3f);
            // 双室门
            g.Line(pal.Edge, 1.2f, r.Left + r.Width / 2f, r.Top + 6f, r.Left + r.Width / 2f, r.Bottom - 6f);
            Door(g, new RectangleF(r.Left + 6f, r.Top + r.Height * 0.42f, r.Width / 2f - 10f, r.Height * 0.5f), pal);
            Door(g, new RectangleF(r.Left + r.Width / 2f + 4f, r.Top + r.Height * 0.42f, r.Width / 2f - 10f, r.Height * 0.5f), pal);
            // 百叶窗
            using (var lou = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.4f))
                for (int i = 0; i < 4; i++)
                    g.DrawLine(lou, r.Right - r.Width * 0.3f, r.Top + 10f + i * 6f, r.Right - 8f, r.Top + 10f + i * 6f);
            // 顶部两只高压套管
            for (int i = 0; i < 2; i++)
            {
                float bx = r.Left + r.Width * (0.32f + i * 0.36f);
                g.Line(pal.Edge, 2.4f, bx, r.Top - 18f, bx, r.Top - 6f);
                g.Ell(bx, r.Top - 20f, 4.5f, 4.5f, Color.FromArgb(150, 130, 120));
            }
        }
        // 发电机房：烟囱 + 热风 + 风机口
        private void DrawGenerator(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.74f, true);
            // 侧面烟囱
            float sx = r.Right - 5f;
            g.Box(new RectangleF(sx, r.Top - a.Height * 0.22f, 9f, a.Height * 0.24f), 2f,
                HToolPalettes.Shade(pal.Main, 0.25f), pal.Edge, 1.3f);
            g.Box(new RectangleF(sx - 2f, r.Top - a.Height * 0.24f, 13f, 5f), 1f, pal.MetalDark, pal.Edge, 1.1f);
            HeatWaves(g, sx + 4.5f, r.Top - a.Height * 0.25f, pal);
            // 圆形通风口
            float fx = r.Left + r.Width * 0.3f, fy = r.Top + r.Height * 0.4f, fr = r.Width * 0.16f;
            g.Ell(fx, fy, fr, fr, Color.FromArgb(52, 56, 62));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), fx - fr, fy - fr, fr * 2f, fr * 2f);
            for (int i = 0; i < 5; i++)
            {
                double an = i * Math.PI * 2.0 / 5.0;
                g.Line(Color.FromArgb(150, 154, 160), 1.8f, fx, fy,
                    fx + (float)Math.Cos(an) * fr * 0.85f, fy + (float)Math.Sin(an) * fr * 0.85f);
            }
            Door(g, new RectangleF(r.Left + r.Width * 0.52f, r.Bottom - r.Height * 0.46f, r.Width * 0.24f, r.Height * 0.46f), pal);
        }
        // 蓄电池室：墙面电池组标识
        private void DrawBattery(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.82f, false);
            // 大电池组
            var bat = new RectangleF(r.Left + r.Width * 0.2f, r.Top + r.Height * 0.28f,
                r.Width * 0.6f, r.Height * 0.34f);
            g.Box(bat, 3f, HToolPalettes.Shade(pal.Main, 0.28f), pal.Edge, 1.4f);
            g.Box(new RectangleF(bat.Right - 1f, bat.Top + bat.Height * 0.25f, 4f, bat.Height * 0.5f),
                1f, pal.MetalDark, pal.Edge, 1f);
            using (var cell = new Pen(pal.Edge, 1.2f))
                for (int i = 1; i < 4; i++)
                    g.DrawLine(cell, bat.Left + bat.Width * i / 4f, bat.Top + 4f,
                        bat.Left + bat.Width * i / 4f, bat.Bottom - 4f);
            using (var bf = new Font("微软雅黑", 8f, FontStyle.Bold))
            {
                HBarBase.DrawText(g, "+", bf, Color.FromArgb(70, 170, 90),
                    new RectangleF(bat.Left + 2f, bat.Top, 12f, bat.Height), ContentAlignment.MiddleLeft);
                HBarBase.DrawText(g, "−", bf, Color.FromArgb(210, 70, 60),
                    new RectangleF(bat.Right - 16f, bat.Top, 14f, bat.Height), ContentAlignment.MiddleCenter);
            }
            Door(g, new RectangleF(r.Left + r.Width * 0.38f, r.Bottom - r.Height * 0.34f, r.Width * 0.24f, r.Height * 0.34f), pal);
        }
        // UPS 电源室：UPS 柜 + 正弦波屏
        private void DrawUps(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.8f, false);
            var cab = new RectangleF(r.Left + r.Width * 0.28f, r.Top + r.Height * 0.16f,
                r.Width * 0.44f, r.Height * 0.56f);
            g.Box(cab, 3f, pal.Metal, pal.Edge, 1.4f);
            var scr = new RectangleF(cab.Left + 6f, cab.Top + 8f, cab.Width - 12f, cab.Height * 0.3f);
            g.Box(scr, 2f, Color.FromArgb(36, 58, 70), pal.Edge, 1.1f);
            using (var sine = new Pen(Color.FromArgb(90, 220, 150), 1.8f))
                g.DrawBezier(sine, scr.Left + 3f, scr.Top + scr.Height / 2f,
                    scr.Left + scr.Width * 0.25f, scr.Top - 2f,
                    scr.Left + scr.Width * 0.25f, scr.Bottom + 2f,
                    scr.Left + scr.Width * 0.5f, scr.Top + scr.Height / 2f);
            g.Lamp(cab.Left + cab.Width * 0.3f, cab.Bottom - 12f, true);
            g.Lamp(cab.Left + cab.Width * 0.5f, cab.Bottom - 12f, false);
            g.Lamp(cab.Left + cab.Width * 0.7f, cab.Bottom - 12f, true);
            Door(g, new RectangleF(r.Left + r.Width * 0.4f, r.Bottom - r.Height * 0.22f, r.Width * 0.2f, r.Height * 0.22f), pal);
        }
        // 控制柜阵列间：成排控制柜（无外墙）
        private void DrawCabinetHall(Graphics g, RectangleF a, HToolPalette pal)
        {
            float gy = a.Bottom - a.Height * 0.08f;
            // 电缆桥架
            g.Box(new RectangleF(a.Left + a.Width * 0.06f, a.Top + a.Height * 0.1f,
                a.Width * 0.88f, 8f), 2f, pal.MetalDark, pal.Edge, 1.2f);
            int n = 4;
            float gap = 4f, cw = (a.Width * 0.88f - gap * (n - 1)) / n;
            for (int i = 0; i < n; i++)
            {
                float cx0 = a.Left + a.Width * 0.06f + i * (cw + gap);
                var cr = new RectangleF(cx0, a.Top + a.Height * 0.18f, cw, gy - (a.Top + a.Height * 0.18f));
                g.Box(cr, 3f, HToolPalettes.Tint(pal.Main, 0.22f), pal.Edge, 1.4f);
                // HMI 屏
                g.Box(new RectangleF(cr.Left + 5f, cr.Top + 8f, cw - 10f, cr.Height * 0.26f),
                    2f, Color.FromArgb(40, 66, 84), pal.Edge, 1.1f);
                g.Line(Color.FromArgb(110, 200, 240), 1.4f, cr.Left + 8f, cr.Top + cr.Height * 0.18f,
                    cr.Right - 8f, cr.Top + cr.Height * 0.18f);
                // 指示灯 + 按钮
                g.Lamp(cr.Left + cw * 0.3f, cr.Top + cr.Height * 0.36f, true);
                g.Lamp(cr.Left + cw * 0.55f, cr.Top + cr.Height * 0.36f, i % 2 == 0);
                g.Ell(cr.Left + cw * 0.72f, cr.Top + cr.Height * 0.36f, 3f, 3f, pal.Accent);
                using (var vent = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.2f))
                    for (int k = 0; k < 4; k++)
                        g.DrawLine(vent, cr.Left + 6f, cr.Bottom - 18f + k * 4f, cr.Right - 6f, cr.Bottom - 18f + k * 4f);
            }
        }
        // 数据机房：玻璃幕墙 + 服务器机柜
        private void DrawServer(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.86f, false);
            // 深色玻璃幕墙
            g.Box(new RectangleF(r.Left + 4f, r.Top + 4f, r.Width - 8f, r.Height * 0.72f),
                2f, Color.FromArgb(32, 46, 60), pal.Edge, 1.2f);
            int n = 5;
            float rackW = (r.Width - 16f) / n;
            for (int i = 0; i < n; i++)
            {
                var rr = new RectangleF(r.Left + 8f + i * rackW, r.Top + 8f, rackW - 3f, r.Height * 0.68f);
                g.Box(rr, 1f, Color.FromArgb(48, 54, 62), pal.Edge, 1f);
                for (int k = 0; k < 6; k++)
                {
                    float ly = rr.Top + 5f + k * (rr.Height - 10f) / 6f;
                    g.Line(Color.FromArgb(70, 76, 84), 1f, rr.Left + 3f, ly, rr.Right - 3f, ly);
                    g.Ell(rr.Right - 5f, ly - 1.6f, 1.4f, 1.4f, (i + k) % 3 == 0
                        ? Color.FromArgb(80, 210, 120) : Color.FromArgb(120, 130, 140));
                }
            }
            // 屋顶空调外机
            for (int i = 0; i < 2; i++)
            {
                var ac = new RectangleF(r.Left + r.Width * (0.2f + i * 0.42f), r.Top - 13f, r.Width * 0.26f, 10f);
                g.Box(ac, 2f, pal.Metal, pal.Edge, 1.1f);
                g.Ell(ac.Left + ac.Width / 2f, ac.Top + 5f, 3.4f, 3.4f, Color.FromArgb(60, 64, 70));
            }
        }
        // 水泵房：穿墙管道 + 泵组
        private void DrawPump(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.72f, true);
            float py = r.Top + r.Height * 0.42f;
            // 穿墙管
            var pipe = new RectangleF(a.Left + 3f, py - 5f, r.Right - a.Left - 4f, 10f);
            g.FillCylH(pipe, pal.MetalDark, pal.Metal);
            g.DrawR(pipe, 3f, pal.Edge, 1.2f);
            // 蜗壳泵 + 电机
            float cx = r.Left + r.Width * 0.62f, cy = py + a.Height * 0.16f, rr = a.Height * 0.11f;
            g.Ell(cx, cy, rr, rr, HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - rr, cy - rr, rr * 2f, rr * 2f);
            g.Ell(cx, cy, rr * 0.28f, rr * 0.28f, pal.Accent);
            var mot = new RectangleF(cx + rr + 3f, cy - rr * 0.42f, a.Right - cx - rr - 8f, rr * 0.84f);
            g.Motor(mot.Left, mot.Right, cy, mot.Height / 2f, pal);
            g.BasePlate(r.Left + r.Width * 0.56f, r.Width * 0.62f, r.Bottom - 8f, 6f, pal.MetalDark);
            Door(g, new RectangleF(r.Left + 7f, r.Bottom - r.Height * 0.34f, r.Width * 0.2f, r.Height * 0.34f), pal);
        }
        // 空压机房：大圆风门口 + 储气罐
        private void DrawCompressor(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.72f, true);
            float fr = r.Width * 0.17f, fx = r.Left + r.Width * 0.3f, fy = r.Top + r.Height * 0.38f;
            g.Ell(fx, fy, fr, fr, Color.FromArgb(52, 56, 62));
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), fx - fr, fy - fr, fr * 2f, fr * 2f);
            for (int i = 0; i < 6; i++)
            {
                double an = i * Math.PI / 3.0;
                g.Line(Color.FromArgb(150, 154, 160), 1.8f, fx, fy,
                    fx + (float)Math.Cos(an) * fr * 0.82f, fy + (float)Math.Sin(an) * fr * 0.82f);
            }
            // 室外储气罐
            float tx = a.Right - a.Width * 0.14f, tw = a.Width * 0.16f;
            var tank = new RectangleF(tx - tw / 2f, r.Bottom - a.Height * 0.34f, tw, a.Height * 0.26f);
            g.Box(tank, tw / 2f, HToolPalettes.Tint(pal.Main, 0.15f), pal.Edge, 1.3f);
            g.Leg(tx - tw / 2f, tank.Bottom, r.Bottom - 4f, pal.MetalDark, 3f);
            g.Leg(tx + tw / 2f, tank.Bottom, r.Bottom - 4f, pal.MetalDark, 3f);
            g.Ell(tx, tank.Top - 3f, 2.6f, 2.6f, pal.Accent);
        }
        // 锅炉房：高烟囱 + 火光窗
        private void DrawBoiler(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.66f, true);
            float sx = a.Left + a.Width * 0.84f;
            float sTop = a.Top + a.Height * 0.04f;
            g.Poly(new[]
            {
                new PointF(sx - 6f, r.Bottom), new PointF(sx + 6f, r.Bottom),
                new PointF(sx + 4f, sTop), new PointF(sx - 4f, sTop)
            }, HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.4f);
            g.Box(new RectangleF(sx - 7f, sTop - 4f, 14f, 5f), 1f, pal.MetalDark, pal.Edge, 1.1f);
            HeatWaves(g, sx, sTop - 5f, pal);
            // 炉火观察窗
            var win = new RectangleF(r.Left + r.Width * 0.24f, r.Top + r.Height * 0.36f, r.Width * 0.34f, r.Height * 0.3f);
            using (var glow = new LinearGradientBrush(win, Color.FromArgb(255, 170, 40), Color.FromArgb(210, 70, 20), 90f))
                g.FillRectangle(glow, win);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), win.X, win.Y, win.Width, win.Height);
            Door(g, new RectangleF(r.Left + r.Width * 0.62f, r.Bottom - r.Height * 0.3f, r.Width * 0.2f, r.Height * 0.3f), pal);
        }
        // 冷冻机房：室外冷水机组
        private void DrawChiller(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.82f, false);
            float cy = r.Bottom - a.Height * 0.14f;
            var shell = new RectangleF(a.Left + a.Width * 0.12f, cy - a.Height * 0.12f,
                a.Width * 0.76f, a.Height * 0.18f);
            g.FillCylH(shell, HToolPalettes.Shade(pal.Main, 0.18f), HToolPalettes.Tint(pal.Main, 0.3f));
            g.DrawR(shell, 4f, pal.Edge, 1.4f);
            // 两端封头
            g.Ell(shell.Left + 6f, cy, 7f, shell.Height / 2f, HToolPalettes.Shade(pal.Main, 0.1f));
            g.Ell(shell.Right - 6f, cy, 7f, shell.Height / 2f, HToolPalettes.Shade(pal.Main, 0.1f));
            // 顶部接管 + 小泵
            g.Box(new RectangleF(shell.Left + shell.Width * 0.3f, shell.Top - 9f, 8f, 9f), 2f, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(shell.Left + shell.Width * 0.62f, shell.Top - 9f, 8f, 9f), 2f, pal.Metal, pal.Edge, 1.1f);
            g.Ell(shell.Left + shell.Width * 0.82f, shell.Top - 11f, 8f, 8f, HToolPalettes.Tint(pal.Main, 0.2f));
            g.Line(pal.Edge, 2f, r.Left + r.Width * 0.2f, r.Top, r.Left + r.Width * 0.2f, shell.Top);
        }
        // 空调机房：屋顶两台空调机组 + 百叶墙
        private void DrawHvac(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.84f, false);
            for (int i = 0; i < 2; i++)
            {
                var ac = new RectangleF(r.Left + r.Width * (0.14f + i * 0.44f), r.Top - a.Height * 0.16f,
                    r.Width * 0.3f, a.Height * 0.13f);
                g.Box(ac, 3f, pal.Metal, pal.Edge, 1.3f);
                g.Ell(ac.Left + ac.Width / 2f, ac.Top + ac.Height * 0.45f, ac.Height * 0.3f, ac.Height * 0.3f,
                    Color.FromArgb(52, 56, 62));
            }
            // 百叶进风墙
            using (var lou = new Pen(HToolPalettes.Shade(pal.Main, 0.28f), 2f))
                for (int i = 0; i < 5; i++)
                    g.DrawLine(lou, r.Left + r.Width * 0.16f, r.Top + r.Height * 0.24f + i * 9f,
                        r.Left + r.Width * 0.46f, r.Top + r.Height * 0.24f + i * 9f);
            Door(g, new RectangleF(r.Left + r.Width * 0.56f, r.Bottom - r.Height * 0.46f, r.Width * 0.26f, r.Height * 0.46f), pal);
        }
        // 洁净车间：FFU 风机过滤单元顶列 + 气密门
        private void DrawClean(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.86f, false);
            g.Box(new RectangleF(r.Left + 3f, r.Top + 3f, r.Width - 6f, r.Height - 6f),
                3f, Color.FromArgb(244, 247, 249), pal.Edge, 1.2f);
            // 顶部 4 台 FFU
            for (int i = 0; i < 4; i++)
            {
                float fw = (r.Width - 20f) / 4f;
                var fr = new RectangleF(r.Left + 7f + i * (fw + 2f), r.Top + 7f, fw - 2f, fw - 2f);
                g.Box(fr, 2f, Color.FromArgb(214, 222, 228), pal.Edge, 1.1f);
                g.Ell(fr.Left + fr.Width / 2f, fr.Top + fr.Height / 2f, fr.Width * 0.26f, fr.Width * 0.26f,
                    Color.FromArgb(120, 130, 140));
            }
            // 气密门 + 压差表
            var door = new RectangleF(r.Left + r.Width * 0.36f, r.Bottom - r.Height * 0.52f,
                r.Width * 0.28f, r.Height * 0.52f);
            g.Box(door, 3f, HToolPalettes.Tint(pal.Main, 0.4f), pal.Edge, 1.3f);
            g.Ell(r.Right - 14f, door.Top + 10f, 6f, 6f, Color.White);
            g.Line(pal.Accent, 1.6f, r.Right - 14f, door.Top + 10f, r.Right - 11f, door.Top + 7f);
            g.Lamp(r.Right - 14f, door.Top + 28f, true);
        }
        // 化验室：窗上烧瓶标识
        private void DrawLaboratory(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.78f, true);
            var win = new RectangleF(r.Left + r.Width * 0.18f, r.Top + r.Height * 0.18f,
                r.Width * 0.64f, r.Height * 0.36f);
            g.Box(win, 2f, Color.FromArgb(226, 238, 244), pal.Edge, 1.2f);
            // 锥形烧瓶剪影
            float fx = win.Left + win.Width / 2f, fTop = win.Top + 5f;
            using (var fp = new GraphicsPath())
            {
                fp.AddLine(fx - 4f, fTop, fx - 4f, fTop + win.Height * 0.34f);
                fp.AddLine(fx - 4f, fTop + win.Height * 0.34f, fx - 13f, win.Bottom - 5f);
                fp.AddLine(fx - 13f, win.Bottom - 5f, fx + 13f, win.Bottom - 5f);
                fp.AddLine(fx + 13f, win.Bottom - 5f, fx + 4f, fTop + win.Height * 0.34f);
                fp.AddLine(fx + 4f, fTop + win.Height * 0.34f, fx + 4f, fTop);
                using (var fb = new SolidBrush(Color.FromArgb(170, 110, 180, 220)))
                    g.FillPath(fb, fp);
                using (var pe = new Pen(Color.FromArgb(90, 120, 150), 1.4f))
                    g.DrawPath(pe, fp);
            }
            Door(g, new RectangleF(r.Left + r.Width * 0.38f, r.Bottom - r.Height * 0.36f, r.Width * 0.24f, r.Height * 0.36f), pal);
        }
        // 库房：大卷帘门
        private void DrawWarehouse(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.92f, false);
            var door = new RectangleF(r.Left + r.Width * 0.12f, r.Top + r.Height * 0.12f,
                r.Width * 0.6f, r.Height * 0.84f);
            g.Box(door, 2f, HToolPalettes.Shade(pal.Main, 0.22f), pal.Edge, 1.5f);
            using (var slat = new Pen(HToolPalettes.Shade(pal.Main, 0.4f), 1.4f))
                for (float y = door.Top + 8f; y < door.Bottom; y += 9f)
                    g.DrawLine(slat, door.Left + 3f, y, door.Right - 3f, y);
            // 人员小门 + 防撞柱
            var pd = new RectangleF(r.Right - r.Width * 0.18f, r.Bottom - r.Height * 0.42f,
                r.Width * 0.12f, r.Height * 0.42f);
            g.Box(pd, 2f, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.2f);
            g.Ell(door.Left - 6f, r.Bottom - 7f, 3f, 7f, Color.FromArgb(220, 170, 40));
            g.Ell(door.Right + 6f, r.Bottom - 7f, 3f, 7f, Color.FromArgb(220, 170, 40));
        }
        // 装卸月台：两个装卸门洞 + 雨棚
        private void DrawDock(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.92f, false);
            // 雨棚
            g.Poly(new[]
            {
                new PointF(r.Left - 8f, r.Top + 2f), new PointF(r.Right + 8f, r.Top + 2f),
                new PointF(r.Right + 4f, r.Top - 9f), new PointF(r.Left - 4f, r.Top - 9f)
            }, HToolPalettes.Shade(pal.Main, 0.35f), pal.Edge, 1.3f);
            for (int i = 0; i < 2; i++)
            {
                var open = new RectangleF(r.Left + r.Width * (0.1f + i * 0.44f), r.Top + r.Height * 0.22f,
                    r.Width * 0.36f, r.Height * 0.6f);
                g.Box(open, 3f, Color.FromArgb(48, 52, 58), pal.Edge, 1.4f);
                // 登车桥
                g.Poly(new[]
                {
                    new PointF(open.Left, open.Bottom), new PointF(open.Right, open.Bottom),
                    new PointF(open.Right + 5f, open.Bottom + 6f), new PointF(open.Left - 5f, open.Bottom + 6f)
                }, pal.MetalDark, pal.Edge, 1.1f);
            }
        }
        // 门卫室：岗亭 + 道闸杆
        private void DrawGuard(Graphics g, RectangleF a, HToolPalette pal)
        {
            float gy = a.Bottom - a.Height * 0.08f;
            var r = new RectangleF(a.Left + a.Width * 0.08f, a.Top + a.Height * 0.34f,
                a.Width * 0.36f, gy - (a.Top + a.Height * 0.34f));
            g.Box(r, 3f, HToolPalettes.Tint(pal.Main, 0.35f), pal.Edge, 1.4f);
            // 环绕窗带
            g.Box(new RectangleF(r.Left + 4f, r.Top + 7f, r.Width - 8f, r.Height * 0.36f),
                2f, Color.FromArgb(190, 220, 236), pal.Edge, 1.1f);
            g.Box(new RectangleF(r.X - 4f, r.Top - 6f, r.Width + 8f, 7f), 2f,
                HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.2f);
            Door(g, new RectangleF(r.Left + r.Width * 0.32f, r.Bottom - r.Height * 0.42f,
                r.Width * 0.36f, r.Height * 0.42f), pal);
            // 道闸机 + 横杆
            float px = a.Left + a.Width * 0.52f;
            g.Box(new RectangleF(px - 5f, gy - a.Height * 0.26f, 10f, a.Height * 0.26f),
                2f, pal.MetalDark, pal.Edge, 1.2f);
            float by = gy - a.Height * 0.24f;
            using (var arm = new Pen(Color.FromArgb(224, 176, 44), 4f) { StartCap = LineCap.Round })
                g.DrawLine(arm, px, by, a.Right - 6f, by);
            using (var red = new Pen(Color.FromArgb(200, 60, 50), 4f))
                for (float x = px + 10f; x < a.Right - 8f; x += 16f)
                    g.DrawLine(red, x, by, x + 8f, by);
        }
        // 岗塔楼：四脚桁架塔 + 顶部岗亭
        private void DrawTower(Graphics g, RectangleF a, HToolPalette pal)
        {
            float gy = a.Bottom - a.Height * 0.05f;
            float top = a.Top + a.Height * 0.3f, cx = a.Left + a.Width * 0.5f;
            float bw = a.Width * 0.26f, tw = a.Width * 0.09f;
            // 四条斜腿
            g.Line(pal.MetalDark, 3f, cx - bw, gy, cx - tw, top);
            g.Line(pal.MetalDark, 3f, cx + bw, gy, cx + tw, top);
            g.Line(pal.MetalDark, 2f, cx - bw * 0.62f, gy, cx - tw * 0.4f, top);
            g.Line(pal.MetalDark, 2f, cx + bw * 0.62f, gy, cx + tw * 0.4f, top);
            // 横撑 + X 撑
            for (int i = 1; i <= 3; i++)
            {
                float t = i / 4f, y = gy - (gy - top) * t;
                float w0 = bw + (tw - bw) * t;
                g.Line(pal.Edge, 1.6f, cx - w0, y, cx + w0, y);
                g.Line(pal.Edge, 1.2f, cx - w0, y, cx + w0, y - (gy - top) / 4f);
                g.Line(pal.Edge, 1.2f, cx + w0, y, cx - w0, y - (gy - top) / 4f);
            }
            // 顶部岗亭 + 顶盖 + 天线
            var cab = new RectangleF(cx - a.Width * 0.16f, top - a.Height * 0.2f, a.Width * 0.32f, a.Height * 0.2f);
            g.Box(cab, 3f, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.4f);
            g.Box(new RectangleF(cab.Left + 4f, cab.Top + 4f, cab.Width - 8f, cab.Height * 0.45f),
                1f, Color.FromArgb(190, 220, 236), pal.Edge, 1f);
            g.GableRoof(cab.Left - 2f, cab.Top - a.Height * 0.1f, cab.Width + 4f, cab.Top + 1f,
                HToolPalettes.Shade(pal.Main, 0.35f), pal.Edge);
            g.Line(pal.Edge, 1.8f, cx, cab.Top - a.Height * 0.1f, cx, a.Top + 2f);
        }
        // 消防泵房：红外墙 + 室外消火栓
        private void DrawFirePump(Graphics g, RectangleF a, HToolPalette pal)
        {
            var r = House(g, a, pal, 0.66f, true);
            // 消防红外圈
            g.DrawR(new RectangleF(r.Left - 2f, r.Top - 2f, r.Width + 4f, r.Height + 4f),
                4f, Color.FromArgb(214, 64, 52), 2.2f);
            Door(g, new RectangleF(r.Left + r.Width * 0.36f, r.Bottom - r.Height * 0.44f,
                r.Width * 0.28f, r.Height * 0.44f), pal);
            // 室外消火栓
            float hx = a.Right - a.Width * 0.16f, hy = r.Bottom;
            g.Line(Color.FromArgb(202, 60, 48), 6f, hx, hy - a.Height * 0.18f, hx, hy);
            g.Line(Color.FromArgb(202, 60, 48), 6f, hx - a.Width * 0.08f, hy - a.Height * 0.16f,
                hx + a.Width * 0.08f, hy - a.Height * 0.16f);
            g.Ell(hx, hy - a.Height * 0.2f, 5f, 5f, Color.FromArgb(202, 60, 48));
            g.Ell(hx - a.Width * 0.08f, hy - a.Height * 0.16f, 3f, 3f, Color.FromArgb(202, 60, 48));
            g.Ell(hx + a.Width * 0.08f, hy - a.Height * 0.16f, 3f, 3f, Color.FromArgb(202, 60, 48));
        }
        // ----------------------------------------------------------------
        // 共享小原语
        // ----------------------------------------------------------------
        /// <summary>墙体：gable=true 双坡屋顶，false 平顶挑檐；返回墙身矩形。</summary>
        private RectangleF House(Graphics g, RectangleF a, HToolPalette pal, float wFrac, bool gable)
        {
            float w = a.Width * wFrac, x = a.Left + (a.Width - w) / 2f;
            float top = a.Top + a.Height * (gable ? 0.34f : 0.3f);
            float bot = a.Bottom - a.Height * 0.07f;
            var r = new RectangleF(x, top, w, bot - top);
            using (var wb = new LinearGradientBrush(r, HToolPalettes.Tint(pal.Main, 0.35f),
                HToolPalettes.Shade(pal.Main, 0.15f), 90f))
                g.FillRectangle(wb, r);
            g.DrawRectangle(new Pen(pal.Edge, 1.5f), r.X, r.Y, r.Width, r.Height);
            if (gable)
                g.GableRoof(x, a.Top + a.Height * 0.1f, w, top + 2f,
                    HToolPalettes.Shade(pal.Main, 0.38f), pal.Edge);
            else
                g.Box(new RectangleF(x - 5f, top - 7f, w + 10f, 8f), 2f,
                    HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.2f);
            return r;
        }
        /// <summary>平板门。</summary>
        private static void Door(Graphics g, RectangleF r, HToolPalette pal)
        {
            g.Box(r, 1f, HToolPalettes.Shade(pal.Main, 0.25f), pal.Edge, 1.2f);
            g.Ell(r.Right - 5f, r.Top + r.Height / 2f, 1.8f, 1.8f, Color.Gold);
        }
        /// <summary>静态热气流（两弯弧）。</summary>
        private static void HeatWaves(Graphics g, float cx, float yTop, HToolPalette pal)
        {
            using (var wave = new Pen(HToolPalettes.Tint(pal.Edge, 0.4f), 1.6f)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 2; i++)
                {
                    float x = cx - 5f + i * 10f;
                    g.DrawBezier(wave, x, yTop, x - 4f, yTop - 6f, x + 4f, yTop - 10f, x, yTop - 16f);
                }
        }
    }
}