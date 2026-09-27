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
    /// 燃烧火焰样式：Classic 为燃气喷嘴多层火焰（已验收外形），其余 21 种覆盖燃油、
    /// 火炬、加热炉、本生灯、焊枪、等离子、焚烧炉、回转窑等工业燃烧外形。
    /// </summary>
    public enum HFireStyle
    {
        Classic = 0,          // 燃气喷嘴火焰
        OilBurner = 1,        // 燃油燃烧器
        Pilot = 2,            // 长明火
        FlareStack = 3,       // 放空火炬
        ProcessHeater = 4,    // 工艺加热炉
        Bunsen = 5,           // 本生灯
        WeldingTorch = 6,     // 气焊枪
        PlasmaArc = 7,        // 等离子弧
        Campfire = 8,         // 篝火
        CoalGrate = 9,        // 链条炉排
        Forge = 10,           // 锻造炉
        Incinerator = 11,     // 焚烧炉
        BoilerRegister = 12,  // 锅炉旋流燃烧器
        GasStove = 13,        // 燃气灶头
        BoxFurnace = 14,      // 箱式马弗炉
        RotaryKiln = 15,      // 回转窑
        RocketTest = 16,      // 发动机试车
        Blowtorch = 17,       // 喷灯
        Fireplace = 18,       // 壁炉
        CuttingSpark = 19,    // 切割火花
        Smolder = 20,         // 阴燃冒烟
        RingBurner = 21       // 环形长明灯
    }
    /// <summary>
    /// 燃烧火焰控件（继承 HToolAnimBase）：22 种燃烧外形、13 种色调，
    /// Burning 为 true 时火焰摇曳、火花飞溅或烟气上升，false 时仅显示未点燃设备。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("燃烧火焰控件：22 种燃烧外形、13 种色调，燃烧时火焰摇曳跳动")]
    public class HFire : HToolAnimBase
    {
        private HFireStyle _style = HFireStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        // 火焰语义固定色（设备本体走调色板，火焰保持真实火色）
        private static readonly Color FireRed = Color.FromArgb(225, 60, 28);
        private static readonly Color FireOrange = Color.FromArgb(255, 120, 24);
        private static readonly Color FireGold = Color.FromArgb(255, 206, 60);
        private static readonly Color FireCream = Color.FromArgb(255, 246, 200);
        public HFire()
        {
            Size = new Size(110, 150);
        }
        /// <summary>燃烧外形样式。</summary>
        [HCategoryLanguage("火焰"), HDisplayNameLanguage("燃烧外形样式"), HDescriptionLanguage("燃烧外形样式"), Browsable(true)]
        [DefaultValue(HFireStyle.Classic)]
        public HFireStyle FireStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>是否燃烧，燃烧时火焰产生摇曳动画。</summary>
        [HCategoryLanguage("火焰"), HDisplayNameLanguage("是否燃烧"), HDescriptionLanguage("是否燃烧，燃烧时火焰摇曳跳动"), Browsable(true)]
        [DefaultValue(false)]
        public bool Burning
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
                case HFireStyle.OilBurner: DrawOilBurner(g, a, pal); break;
                case HFireStyle.Pilot: DrawPilot(g, a, pal); break;
                case HFireStyle.FlareStack: DrawFlare(g, a, pal); break;
                case HFireStyle.ProcessHeater: DrawHeater(g, a, pal); break;
                case HFireStyle.Bunsen: DrawBunsen(g, a, pal); break;
                case HFireStyle.WeldingTorch: DrawWelding(g, a, pal); break;
                case HFireStyle.PlasmaArc: DrawPlasma(g, a, pal); break;
                case HFireStyle.Campfire: DrawCampfire(g, a, pal); break;
                case HFireStyle.CoalGrate: DrawCoal(g, a, pal); break;
                case HFireStyle.Forge: DrawForge(g, a, pal); break;
                case HFireStyle.Incinerator: DrawIncinerator(g, a, pal); break;
                case HFireStyle.BoilerRegister: DrawRegister(g, a, pal); break;
                case HFireStyle.GasStove: DrawStove(g, a, pal); break;
                case HFireStyle.BoxFurnace: DrawBoxFurnace(g, a, pal); break;
                case HFireStyle.RotaryKiln: DrawKiln(g, a, pal); break;
                case HFireStyle.RocketTest: DrawRocket(g, a, pal); break;
                case HFireStyle.Blowtorch: DrawBlowtorch(g, a, pal); break;
                case HFireStyle.Fireplace: DrawFireplace(g, a, pal); break;
                case HFireStyle.CuttingSpark: DrawCutting(g, a, pal); break;
                case HFireStyle.Smolder: DrawSmolder(g, a, pal); break;
                case HFireStyle.RingBurner: DrawRing(g, a, pal); break;
                default: DrawClassic(g, a); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：燃气喷嘴 + 三层火焰（已验收绘法，原样保留）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width, glyphH = a.Height;
            float cx = Width / 2f;
            float pipeW = Width * 0.62f, pipeH = glyphH * 0.1f;
            float pipeY = glyphH * 0.82f;
            // 燃气管 + 喷嘴
            var pipe = new RectangleF(cx - pipeW / 2f, pipeY, pipeW, pipeH);
            using (var pb = HBarBase.CylinderH(pipe, Color.FromArgb(90, 94, 100), Color.FromArgb(220, 224, 230)))
                g.FillRectangle(pb, pipe);
            g.DrawRectangle(new Pen(Color.FromArgb(60, 64, 70), 1.3f), pipe.X, pipe.Y, pipe.Width, pipe.Height);
            var noz = new[]
            {
                new PointF(cx - pipeW * 0.12f, pipeY),
                new PointF(cx + pipeW * 0.12f, pipeY),
                new PointF(cx + pipeW * 0.05f, pipeY - pipeH * 0.9f),
                new PointF(cx - pipeW * 0.05f, pipeY - pipeH * 0.9f)
            };
            using (var nb = new SolidBrush(Color.FromArgb(70, 74, 80)))
                g.FillPolygon(nb, noz);
            float baseY = pipeY - pipeH * 0.9f;
            if (Running)
            {
                // 外焰/中焰/内焰三层，各自随节拍摇曳
                float f1 = (float)Math.Sin(Phase * 3.1) * glyphH * 0.03f;
                float f2 = (float)Math.Sin(Phase * 4.7 + 1f) * glyphH * 0.025f;
                float f3 = (float)Math.Sin(Phase * 6.3 + 2f) * glyphH * 0.018f;
                Flame(g, cx, baseY, glyphH * 0.52f + f1, pipeW * 0.26f, FireRed, FireOrange);
                Flame(g, cx, baseY - 2f, glyphH * 0.4f + f2, pipeW * 0.19f, FireOrange, FireGold);
                Flame(g, cx, baseY - 3f, glyphH * 0.26f + f3, pipeW * 0.11f, FireGold, FireCream);
                // 根部蓝光
                using (var core = new SolidBrush(Color.FromArgb(160, 120, 200, 255)))
                    g.FillEllipse(core, cx - pipeW * 0.08f, baseY - 5f, pipeW * 0.16f, 9f);
            }
            else
            {
                // 未点燃：喷嘴小火星盖
                using (var cap = new SolidBrush(Color.FromArgb(50, 54, 60)))
                    g.FillEllipse(cap, cx - pipeW * 0.06f, baseY - 3f, pipeW * 0.12f, 6f);
            }
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // 新式燃烧器
        // ----------------------------------------------------------------
        // 燃油燃烧器：喷枪横置，雾化焰锥向右
        private void DrawOilBurner(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.62f;
            var gun = new RectangleF(a.Left + a.Width * 0.08f, cy - a.Height * 0.07f,
                a.Width * 0.36f, a.Height * 0.14f);
            g.FillCylH(gun, pal.MetalDark, pal.Metal);
            g.DrawR(gun, 4f, pal.Edge, 1.3f);
            // 风箱 + 喷嘴
            g.Box(new RectangleF(gun.Left - 2f, cy - a.Height * 0.1f, a.Width * 0.12f, a.Height * 0.2f),
                3f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.3f);
            g.Poly(new[]
            {
                new PointF(gun.Right - 2f, cy - 6f), new PointF(gun.Right + 6f, cy - 3f),
                new PointF(gun.Right + 6f, cy + 3f), new PointF(gun.Right - 2f, cy + 6f)
            }, pal.MetalDark, pal.Edge, 1.2f);
            if (Running)
            {
                float wob = (float)Math.Sin(Phase * 3f) * 5f;
                FlameCone(g, gun.Right + 6f, cy, 8f, a.Width * 0.42f + wob, FireRed, FireGold);
                Sparks(g, gun.Right + 8f, cy, 0.0, 0.5, a.Width * 0.4f, 7);
            }
            else
                g.Ell(gun.Right + 6f, cy, 3f, 3f, Color.FromArgb(50, 54, 60));
        }
        // 长明火：细立管 + 小稳焰火
        private void DrawPilot(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, by = a.Bottom - a.Height * 0.1f;
            // 底座
            g.Box(new RectangleF(cx - a.Width * 0.16f, by - 6f, a.Width * 0.32f, 7f),
                2f, pal.MetalDark, pal.Edge, 1.2f);
            g.Line(pal.Metal, 5f, cx, by - 6f, cx, by - a.Height * 0.26f);
            g.Ell(cx, by - a.Height * 0.27f, 4.5f, 4.5f, pal.MetalDark);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 5f) * 2f;
                Flame(g, cx, by - a.Height * 0.3f, a.Height * 0.2f + f, 7f, FireOrange, FireCream);
            }
            else
                g.Ell(cx, by - a.Height * 0.3f, 2.6f, 2.6f, Color.FromArgb(50, 54, 60));
        }
        // 放空火炬：高塔 + 缆风绳 + 大火
        private void DrawFlare(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, top = a.Top + a.Height * 0.3f;
            float by = a.Bottom - a.Height * 0.08f;
            // 塔架（三柱桁架示意）
            g.Line(pal.MetalDark, 4f, cx - 9f, by, cx - 3f, top);
            g.Line(pal.MetalDark, 4f, cx + 9f, by, cx + 3f, top);
            using (var cross = new Pen(pal.Edge, 1.3f))
                for (int i = 1; i < 5; i++)
                {
                    float t = i / 5f, y = by - (by - top) * t, w = 9f - 6f * t;
                    g.DrawLine(cross, cx - w, y, cx + w, y);
                    g.DrawLine(cross, cx - w, y, cx + w, y - (by - top) / 5f);
                }
            // 缆风绳
            g.Line(pal.Edge, 1.2f, cx, top + 6f, a.Left + 3f, by);
            g.Line(pal.Edge, 1.2f, cx, top + 6f, a.Right - 3f, by);
            // 烧嘴筒
            g.Box(new RectangleF(cx - 5f, top - 6f, 10f, 10f), 2f, pal.MetalDark, pal.Edge, 1.2f);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 2.4) * 5f;
                Flame(g, cx, top - 8f, a.Height * 0.3f + f, a.Width * 0.14f, FireRed, FireGold);
                Flame(g, cx, top - 10f, a.Height * 0.2f + f * 0.6f, a.Width * 0.08f, FireGold, FireCream);
                Smoke(g, cx, top - a.Height * 0.3f - 2f, a.Height * 0.12f);
            }
        }
        // 工艺加热炉：圆筒炉 + 底部火口 + 烟囱
        private void DrawHeater(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.46f;
            var shell = new RectangleF(cx - a.Width * 0.22f, a.Top + a.Height * 0.2f,
                a.Width * 0.44f, a.Height * 0.62f);
            g.FillCylV(shell, HToolPalettes.Tint(pal.Main, 0.3f), pal.Main, HToolPalettes.Shade(pal.Main, 0.2f));
            g.DrawR(shell, 6f, pal.Edge, 1.5f);
            // 顶烟囱
            g.Box(new RectangleF(cx - 5f, a.Top + a.Height * 0.06f, 10f, a.Height * 0.16f),
                2f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.2f);
            // 平台栏杆
            g.Line(pal.Edge, 1.3f, shell.Left - 5f, shell.Top + 8f, shell.Right + 5f, shell.Top + 8f);
            // 底部看火口
            var port = new RectangleF(cx - a.Width * 0.1f, shell.Bottom - a.Height * 0.18f,
                a.Width * 0.2f, a.Height * 0.14f);
            if (Running)
            {
                using (var glow = new LinearGradientBrush(port, FireGold, FireRed, 90f))
                    g.FillRectangle(glow, port);
                Flame(g, cx, port.Bottom - 2f, a.Height * 0.12f, a.Width * 0.07f, FireRed, FireGold);
            }
            else
                g.Box(port, 2f, Color.FromArgb(48, 52, 58), pal.Edge, 1.2f);
        }
        // 本生灯：底座 + 立管 + 蓝焰
        private void DrawBunsen(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, by = a.Bottom - a.Height * 0.1f;
            g.Ell(cx, by - 3f, a.Width * 0.22f, 6f, pal.MetalDark);
            g.Box(new RectangleF(cx - a.Width * 0.14f, by - 9f, a.Width * 0.28f, 6f),
                2f, pal.Metal, pal.Edge, 1.2f);
            // 调风环 + 管
            g.Box(new RectangleF(cx - 6f, by - a.Height * 0.3f, 12f, 7f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Line(pal.Metal, 5f, cx, by - a.Height * 0.3f, cx, by - a.Height * 0.52f);
            g.Line(pal.Edge, 1.4f, cx - 11f, by - a.Height * 0.16f, cx - 4f, by - a.Height * 0.2f);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 4f) * 2f;
                // 外焰淡蓝
                Flame(g, cx, by - a.Height * 0.54f, a.Height * 0.26f + f, 8f,
                    Color.FromArgb(120, 90, 170, 255), Color.FromArgb(200, 150, 210, 255));
                // 内锥
                Flame(g, cx, by - a.Height * 0.55f, a.Height * 0.13f, 4f,
                    Color.FromArgb(180, 60, 130, 255), Color.FromArgb(230, 190, 230, 255));
            }
        }
        // 气焊枪：斜握枪身 + 尖焰
        private void DrawWelding(Graphics g, RectangleF a, HToolPalette pal)
        {
            float hx = a.Left + a.Width * 0.24f, hy = a.Bottom - a.Height * 0.16f;
            float nx = a.Left + a.Width * 0.56f, ny = a.Top + a.Height * 0.42f;
            // 握把 + 枪管
            g.Line(HToolPalettes.Shade(pal.Main, 0.2f), 7f, hx, hy + 10f, hx, hy - 8f);
            g.Line(pal.MetalDark, 4f, hx, hy - 6f, nx, ny);
            g.Line(pal.Metal, 2.4f, nx - 6f, ny + 6f, nx, ny);
            // 调节阀
            g.Ell(hx + 2f, hy - 10f, 3.4f, 3.4f, pal.Accent);
            if (Running)
            {
                var st = g.Save();
                g.TranslateTransform(nx, ny);
                g.RotateTransform(-48f + (float)Math.Sin(Phase * 4f) * 1.5f);
                Flame(g, 0f, 0f, a.Height * 0.2f, 5f, FireRed, FireCream);
                g.Restore(st);
                Sparks(g, nx, ny, -Math.PI / 2.4, 0.6, a.Width * 0.22f, 6);
            }
        }
        // 等离子弧：压缩细弧 + 火花
        private void DrawPlasma(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, by = a.Bottom - a.Height * 0.12f;
            // 枪体
            g.Box(new RectangleF(cx - a.Width * 0.14f, by - a.Height * 0.16f, a.Width * 0.28f, a.Height * 0.16f),
                4f, HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.3f);
            g.Poly(new[]
            {
                new PointF(cx - 7f, by - a.Height * 0.16f), new PointF(cx + 7f, by - a.Height * 0.16f),
                new PointF(cx + 3f, by - a.Height * 0.22f), new PointF(cx - 3f, by - a.Height * 0.22f)
            }, pal.MetalDark, pal.Edge, 1.2f);
            g.Line(pal.Edge, 2f, cx + a.Width * 0.14f, by - a.Height * 0.08f,
                a.Right - 4f, by - a.Height * 0.02f);
            if (Running)
            {
                float wob = (float)Math.Sin(Phase * 9f) * 2.5f;
                Flame(g, cx + wob, by - a.Height * 0.24f, a.Height * 0.24f, 3.4f,
                    Color.FromArgb(200, 60, 160, 255), Color.FromArgb(255, 220, 250, 255));
                Sparks(g, cx, by - a.Height * 0.24f, -Math.PI / 2.0, 0.8, a.Width * 0.26f, 9);
            }
        }
        // 篝火：交叉木柴 + 石圈
        private void DrawCampfire(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, by = a.Bottom - a.Height * 0.14f;
            g.Line(pal.Edge, 2f, a.Left + a.Width * 0.1f, by + 6f, a.Right - a.Width * 0.1f, by + 6f);
            for (int i = 0; i < 7; i++)
            {
                double an = i * Math.PI * 2.0 / 7.0;
                g.Ell(cx + (float)Math.Cos(an) * a.Width * 0.3f, by + 4f, 3f, 3f, pal.MetalDark);
            }
            // 交叉木柴
            g.Line(Color.FromArgb(110, 70, 36), 5f, cx - a.Width * 0.24f, by + 3f, cx + a.Width * 0.2f, by - 4f);
            g.Line(Color.FromArgb(126, 82, 42), 5f, cx + a.Width * 0.24f, by + 3f, cx - a.Width * 0.2f, by - 4f);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 3.4) * 4f;
                Flame(g, cx, by - 4f, a.Height * 0.34f + f, a.Width * 0.17f, FireRed, FireOrange);
                Flame(g, cx, by - 6f, a.Height * 0.24f + f, a.Width * 0.1f, FireOrange, FireGold);
                Sparks(g, cx, by - a.Height * 0.3f, -Math.PI / 2.0, 0.7, a.Height * 0.16f, 5);
            }
        }
        // 链条炉排：炉箱 + 炉排 + 煤层透火
        private void DrawCoal(Graphics g, RectangleF a, HToolPalette pal)
        {
            var box = new RectangleF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.24f,
                a.Width * 0.76f, a.Height * 0.6f);
            g.Box(box, 4f, HToolPalettes.Shade(pal.Main, 0.2f), pal.Edge, 1.5f);
            // 看火口
            var open = RectangleF.Inflate(box, -a.Width * 0.08f, -a.Height * 0.08f);
            g.Box(open, 2f, Color.FromArgb(40, 36, 34), pal.Edge, 1.2f);
            // 炉排
            using (var bar = new Pen(pal.MetalDark, 2.4f))
                for (float x = open.Left + 5f; x < open.Right - 3f; x += 7f)
                    g.DrawLine(bar, x, open.Bottom - 4f, x, open.Bottom - 12f);
            // 煤块
            var rnd = new Random(7);
            using (var coal = new SolidBrush(Color.FromArgb(38, 34, 32)))
                for (int i = 0; i < 9; i++)
                    g.FillEllipse(coal, open.Left + 4f + i * (open.Width - 12f) / 9f,
                        open.Bottom - 10f - rnd.Next(0, 6), 8f, 7f);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 4f) * 3f;
                Flame(g, open.Left + open.Width * 0.32f, open.Bottom - 8f, a.Height * 0.2f + f, 9f, FireRed, FireOrange);
                Flame(g, open.Left + open.Width * 0.66f, open.Bottom - 8f, a.Height * 0.17f + f, 8f, FireRed, FireGold);
            }
        }
        // 锻造炉：炉台 + 拱形炉膛 + 烟囱
        private void DrawForge(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            // 烟囱
            g.Box(new RectangleF(cx - 5f, a.Top + a.Height * 0.06f, 10f, a.Height * 0.24f),
                2f, HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.2f);
            // 炉身
            var body = new RectangleF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.28f,
                a.Width * 0.68f, a.Height * 0.54f);
            g.Box(body, 4f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.5f);
            // 拱形炉膛
            var open = new RectangleF(body.Left + 8f, body.Top + 10f, body.Width - 16f, body.Height * 0.56f);
            using (var arch = new GraphicsPath())
            {
                arch.AddArc(open, 180f, 180f);
                arch.AddLine(open.Right, open.Top + open.Height / 2f, open.Right, open.Bottom);
                arch.AddLine(open.Right, open.Bottom, open.Left, open.Bottom);
                arch.AddLine(open.Left, open.Bottom, open.Left, open.Top + open.Height / 2f);
                arch.CloseFigure();
                using (var dark = new SolidBrush(Color.FromArgb(40, 34, 30)))
                    g.FillPath(dark, arch);
                g.DrawPath(new Pen(pal.Edge, 1.3f), arch);
                if (Running)
                {
                    var old = g.Clip; g.SetClip(arch);
                    float f = (float)Math.Sin(Phase * 3.6f) * 3f;
                    Flame(g, cx, open.Bottom - 2f, open.Height * 0.92f + f, open.Width * 0.32f, FireRed, FireGold);
                    g.Clip = old;
                }
            }
            // 炉台
            g.Box(new RectangleF(body.Left - 6f, body.Bottom, body.Width + 12f, 8f),
                2f, pal.MetalDark, pal.Edge, 1.2f);
        }
        // 焚烧炉：立式炉膛 + 侧看火口 + 排烟
        private void DrawIncinerator(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.44f;
            var body = new RectangleF(cx - a.Width * 0.2f, a.Top + a.Height * 0.18f,
                a.Width * 0.4f, a.Height * 0.66f);
            g.Box(body, 5f, HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.5f);
            g.Box(new RectangleF(cx - 6f, a.Top + a.Height * 0.06f, 12f, a.Height * 0.13f),
                2f, HToolPalettes.Shade(pal.Main, 0.3f), pal.Edge, 1.2f);
            // 加料口 + 看火口
            g.Box(new RectangleF(body.Left + 6f, body.Top + 8f, body.Width - 12f, a.Height * 0.08f),
                2f, pal.MetalDark, pal.Edge, 1.1f);
            var port = new RectangleF(body.Left + 7f, body.Bottom - a.Height * 0.22f,
                body.Width - 14f, a.Height * 0.16f);
            if (Running)
            {
                using (var glow = new SolidBrush(Color.FromArgb(255, 150, 40)))
                    g.FillRectangle(glow, port);
                Flame(g, cx, port.Bottom, a.Height * 0.14f, a.Width * 0.08f, FireRed, FireGold);
                Smoke(g, cx, a.Top + a.Height * 0.06f, a.Height * 0.14f);
            }
            else
                g.Box(port, 2f, Color.FromArgb(48, 52, 58), pal.Edge, 1.2f);
        }
        // 锅炉旋流燃烧器：圆形调风器 + 入炉焰锥
        private void DrawRegister(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.36f, cy = a.Top + a.Height * 0.5f, r = a.Height * 0.2f;
            // 炉墙管口
            g.Box(new RectangleF(cx + r * 0.4f, cy - r * 1.3f, r * 2.2f, r * 2.6f),
                4f, HToolPalettes.Shade(pal.Main, 0.25f), pal.Edge, 1.4f);
            // 旋流调风器圆盘
            g.Ell(cx, cy, r, r, pal.Metal);
            g.DrawEllipse(new Pen(pal.Edge, 1.4f), cx - r, cy - r, r * 2f, r * 2f);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(Running ? SpinAngle : 20f);
            using (var vane = new SolidBrush(Color.FromArgb(90, 96, 104)))
                for (int i = 0; i < 10; i++)
                {
                    double an = i * Math.PI * 2.0 / 10.0;
                    g.FillPolygon(vane, new[]
                    {
                        new PointF((float)Math.Cos(an) * r * 0.35f, (float)Math.Sin(an) * r * 0.35f),
                        new PointF((float)Math.Cos(an + 0.4f) * r * 0.88f, (float)Math.Sin(an + 0.4f) * r * 0.88f),
                        new PointF((float)Math.Cos(an + 0.62f) * r * 0.88f, (float)Math.Sin(an + 0.62f) * r * 0.88f),
                        new PointF((float)Math.Cos(an + 0.22f) * r * 0.35f, (float)Math.Sin(an) * r * 0.35f)
                    });
                }
            g.Restore(st);
            g.Ell(cx, cy, r * 0.18f, r * 0.18f, pal.MetalDark);
            if (Running)
                FlameCone(g, cx + r * 0.9f, cy, r * 0.5f, r * 2.4f + (float)Math.Sin(Phase * 3f) * 4f,
                    FireRed, FireGold);
        }
        // 燃气灶头：炉头环 + 一周小火
        private void DrawStove(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.56f, r = a.Width * 0.26f;
            // 灶面
            g.Box(new RectangleF(a.Left + a.Width * 0.1f, cy - r - 8f, a.Width * 0.8f, r * 2f + 26f),
                6f, HToolPalettes.Tint(pal.Main, 0.3f), pal.Edge, 1.4f);
            // 锅架
            using (var triv = new Pen(pal.MetalDark, 3f))
                for (int i = 0; i < 4; i++)
                {
                    double an = i * Math.PI / 2.0;
                    g.DrawLine(triv, cx + (float)Math.Cos(an) * r * 0.6f, cy + (float)Math.Sin(an) * r * 0.6f,
                        cx + (float)Math.Cos(an) * r * 1.15f, cy + (float)Math.Sin(an) * r * 1.15f);
                }
            // 炉头环 + 中心盖
            g.Ell(cx, cy, r * 0.72f, r * 0.72f, pal.MetalDark);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r * 0.72f, cy - r * 0.72f, r * 1.44f, r * 1.44f);
            g.Ell(cx, cy, r * 0.26f, r * 0.26f, pal.Metal);
            if (Running)
                for (int i = 0; i < 12; i++)
                {
                    double an = i * Math.PI / 6.0;
                    float f = (float)Math.Sin(Phase * 6f + i) * 1.5f;
                    float bx = cx + (float)Math.Cos(an) * r * 0.72f;
                    float by = cy + (float)Math.Sin(an) * r * 0.72f;
                    Flame(g, bx, by, r * 0.42f + f, r * 0.1f, FireOrange, FireCream);
                }
            // 旋钮
            g.Ell(cx - r * 0.8f, cy + r + 14f, 4f, 4f, pal.Accent);
            g.Ell(cx + r * 0.8f, cy + r + 14f, 4f, 4f, pal.MetalDark);
        }
        // 箱式马弗炉：开门炉膛 + 内焰
        private void DrawBoxFurnace(Graphics g, RectangleF a, HToolPalette pal)
        {
            var body = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.2f,
                a.Width * 0.66f, a.Height * 0.56f);
            g.Box(body, 4f, HToolPalettes.Shade(pal.Main, 0.18f), pal.Edge, 1.5f);
            // 侧开炉门
            g.Poly(new[]
            {
                new PointF(body.Left, body.Top), new PointF(body.Left - a.Width * 0.14f, body.Top + 6f),
                new PointF(body.Left - a.Width * 0.14f, body.Bottom - 6f), new PointF(body.Left, body.Bottom)
            }, pal.Metal, pal.Edge, 1.3f);
            // 炉膛
            var open = RectangleF.Inflate(body, -10f, -10f);
            if (Running)
            {
                using (var glow = new LinearGradientBrush(open, Color.FromArgb(255, 200, 80),
                    Color.FromArgb(210, 70, 20), 90f))
                    g.FillRectangle(glow, open);
                Flame(g, open.Left + open.Width * 0.5f, open.Bottom, open.Height * 0.8f,
                    open.Width * 0.26f, FireRed, FireGold);
            }
            else
                g.Box(open, 2f, Color.FromArgb(44, 42, 42), pal.Edge, 1.2f);
            // 控制盒
            g.Box(new RectangleF(body.Right - 2f, body.Top - 4f, a.Width * 0.16f, a.Height * 0.14f),
                3f, pal.Metal, pal.Edge, 1.2f);
            g.Lamp(body.Right + a.Width * 0.08f, body.Top + 8f, Running);
        }
        // 回转窑：斜置转筒 + 托轮 + 窑头焰
        private void DrawKiln(Graphics g, RectangleF a, HToolPalette pal)
        {
            var st = g.Save();
            g.TranslateTransform(a.Left + a.Width * 0.5f, a.Top + a.Height * 0.55f);
            g.RotateTransform(-12f);
            float w = a.Width * 0.76f, h = a.Height * 0.22f;
            var shell = new RectangleF(-w / 2f, -h / 2f, w, h);
            g.FillCylH(shell, HToolPalettes.Shade(pal.Main, 0.22f), HToolPalettes.Tint(pal.Main, 0.25f));
            g.DrawR(shell, h / 2f, pal.Edge, 1.5f);
            // 两轮带 + 托轮
            foreach (float fx in new[] { -w * 0.26f, w * 0.26f })
            {
                g.Line(pal.MetalDark, 4f, fx, -h / 2f, fx, h * 0.9f);
                g.Ell(fx - 9f, h * 0.95f, 7f, 7f, pal.MetalDark);
                g.Ell(fx + 9f, h * 0.95f, 7f, 7f, pal.MetalDark);
            }
            // 窑口焰
            if (Running)
                FlameCone(g, w / 2f + 2f, 0f, h * 0.36f, w * 0.26f, FireRed, FireGold);
            else
                g.Ell(w / 2f, 0f, h * 0.34f, h * 0.34f, Color.FromArgb(48, 52, 58));
            g.Restore(st);
        }
        // 发动机试车：钟形喷管 + 马赫环焰
        private void DrawRocket(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, ny = a.Top + a.Height * 0.2f;
            // 试车台架
            g.Line(pal.MetalDark, 4f, cx - a.Width * 0.26f, ny, cx - a.Width * 0.3f, a.Bottom - 6f);
            g.Line(pal.MetalDark, 4f, cx + a.Width * 0.26f, ny, cx + a.Width * 0.3f, a.Bottom - 6f);
            // 发动机身 + 钟形喷管
            g.Box(new RectangleF(cx - a.Width * 0.12f, a.Top + 4f, a.Width * 0.24f, ny - a.Top),
                3f, HToolPalettes.Shade(pal.Main, 0.15f), pal.Edge, 1.3f);
            g.Poly(new[]
            {
                new PointF(cx - a.Width * 0.08f, ny), new PointF(cx + a.Width * 0.08f, ny),
                new PointF(cx + a.Width * 0.2f, ny + a.Height * 0.12f),
                new PointF(cx - a.Width * 0.2f, ny + a.Height * 0.12f)
            }, pal.MetalDark, pal.Edge, 1.4f);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 5f) * 4f;
                // 主焰向下
                float by = ny + a.Height * 0.12f, len = a.Height * 0.52f + f;
                using (var fp = new GraphicsPath())
                {
                    fp.AddBezier(cx - a.Width * 0.16f, by, cx - a.Width * 0.22f, by + len * 0.5f,
                        cx - a.Width * 0.06f, by + len * 0.85f, cx, by + len);
                    fp.AddBezier(cx, by + len, cx + a.Width * 0.06f, by + len * 0.85f,
                        cx + a.Width * 0.22f, by + len * 0.5f, cx + a.Width * 0.16f, by);
                    fp.CloseFigure();
                    using (var fb = new LinearGradientBrush(new RectangleF(cx - 20, by, 40, len),
                        FireCream, FireRed, 90f))
                        g.FillPath(fb, fp);
                }
                // 马赫环
                for (int i = 0; i < 3; i++)
                    g.Ell(cx, by + len * (0.25f + i * 0.22f), 5f - i, 3f,
                        Color.FromArgb(180 - i * 40, 170, 210, 255));
                Smoke(g, cx - a.Width * 0.2f, a.Bottom - 6f, a.Height * 0.1f);
                Smoke(g, cx + a.Width * 0.2f, a.Bottom - 6f, a.Height * 0.1f);
            }
        }
        // 喷灯：油壶 + 弯颈喷管
        private void DrawBlowtorch(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.72f;
            // 油壶
            var can = new RectangleF(a.Left + a.Width * 0.14f, cy - a.Height * 0.1f,
                a.Width * 0.44f, a.Height * 0.18f);
            g.Box(can, a.Height * 0.09f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.4f);
            g.Ell(can.Left + 8f, can.Top - 4f, 4f, 4f, pal.MetalDark);
            // 弯颈喷管
            g.Line(pal.MetalDark, 4f, can.Right - 4f, can.Top, can.Right + a.Width * 0.14f, cy - a.Height * 0.26f);
            float nx = can.Right + a.Width * 0.14f, ny = cy - a.Height * 0.26f;
            g.Line(pal.Metal, 3f, nx, ny, nx + a.Width * 0.16f, ny - a.Height * 0.06f);
            if (Running)
            {
                float f = (float)Math.Sin(Phase * 4f) * 2f;
                var st = g.Save();
                g.TranslateTransform(nx + a.Width * 0.16f, ny - a.Height * 0.06f);
                g.RotateTransform(-20f);
                Flame(g, 0f, 0f, a.Height * 0.2f + f, 6f, FireRed, FireGold);
                g.Restore(st);
            }
            // 调节阀
            g.Ell(can.Left + can.Width * 0.3f, can.Bottom + 2f, 3.4f, 3.4f, pal.Accent);
        }
        // 壁炉：炉台 + 炉膛 + 木柴
        private void DrawFireplace(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f;
            // 壁炉外框
            var outer = new RectangleF(a.Left + a.Width * 0.14f, a.Top + a.Height * 0.16f,
                a.Width * 0.72f, a.Height * 0.72f);
            g.Box(outer, 5f, HToolPalettes.Shade(pal.Main, 0.25f), pal.Edge, 1.5f);
            g.Box(new RectangleF(outer.Left - 5f, outer.Bottom - 2f, outer.Width + 10f, 9f),
                2f, HToolPalettes.Shade(pal.Main, 0.4f), pal.Edge, 1.2f);
            // 拱形炉膛（小尺寸下内缩量按外框收缩，避免宽高为负导致 AddArc 异常）
            float ix = Math.Min(12f, outer.Width / 2f - 1f), iy = Math.Min(12f, outer.Height / 2f - 1f);
            var open = RectangleF.Inflate(outer, -ix, -iy);
            using (var arch = new GraphicsPath())
            {
                arch.AddArc(open, 180f, 180f);
                arch.AddLine(open.Right, open.Top + open.Height / 2f, open.Right, open.Bottom);
                arch.AddLine(open.Right, open.Bottom, open.Left, open.Bottom);
                arch.AddLine(open.Left, open.Bottom, open.Left, open.Top + open.Height / 2f);
                arch.CloseFigure();
                using (var dark = new SolidBrush(Color.FromArgb(36, 32, 30)))
                    g.FillPath(dark, arch);
                g.DrawPath(new Pen(pal.Edge, 1.3f), arch);
                // 木柴
                g.Line(Color.FromArgb(110, 70, 36), 4f, cx - 12f, open.Bottom - 4f, cx + 9f, open.Bottom - 10f);
                g.Line(Color.FromArgb(126, 82, 42), 4f, cx + 12f, open.Bottom - 4f, cx - 9f, open.Bottom - 10f);
                if (Running)
                {
                    var old = g.Clip; g.SetClip(arch);
                    float f = (float)Math.Sin(Phase * 3.2f) * 3f;
                    Flame(g, cx, open.Bottom - 6f, open.Height * 0.78f + f, 13f, FireRed, FireOrange);
                    Flame(g, cx, open.Bottom - 8f, open.Height * 0.5f, 7f, FireGold, FireCream);
                    g.Clip = old;
                }
            }
        }
        // 切割火花：割炬 + 扇形火花雨
        private void DrawCutting(Graphics g, RectangleF a, HToolPalette pal)
        {
            float tx = a.Left + a.Width * 0.42f, ty = a.Top + a.Height * 0.2f;
            g.Line(HToolPalettes.Shade(pal.Main, 0.2f), 6f, tx - 8f, ty - 10f, tx + 4f, ty + 4f);
            g.Line(pal.MetalDark, 3.4f, tx, ty, a.Left + a.Width * 0.56f, a.Top + a.Height * 0.38f);
            float sx = a.Left + a.Width * 0.56f, sy = a.Top + a.Height * 0.38f;
            if (Running)
            {
                // 氧割细焰
                g.Line(Color.FromArgb(220, 120, 200, 255), 2.4f, sx, sy, sx + 6f, sy + 12f);
                Sparks(g, sx + 6f, sy + 12f, Math.PI / 2.0, 1.1, a.Height * 0.42f, 16);
            }
            // 工件
            g.Line(pal.MetalDark, 6f, a.Left + a.Width * 0.2f, sy + 14f, a.Right - a.Width * 0.1f, sy + 14f);
        }
        // 阴燃冒烟：料堆 + 余烬 + 烟气
        private void DrawSmolder(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, by = a.Bottom - a.Height * 0.14f;
            g.Line(pal.Edge, 2f, a.Left + a.Width * 0.08f, by + 6f, a.Right - a.Width * 0.08f, by + 6f);
            // 料堆
            g.Poly(new[]
            {
                new PointF(cx - a.Width * 0.3f, by + 4f), new PointF(cx - a.Width * 0.12f, by - a.Height * 0.12f),
                new PointF(cx + a.Width * 0.06f, by - a.Height * 0.08f),
                new PointF(cx + a.Width * 0.24f, by + 4f)
            }, HToolPalettes.Shade(pal.Main, 0.35f), pal.Edge, 1.3f);
            // 余烬
            int ember = Running ? (int)(140 + 100 * Math.Abs(Math.Sin(Phase * 2f))) : 90;
            using (var glow = new SolidBrush(Color.FromArgb(ember, 255, 90, 20)))
                g.FillEllipse(glow, cx - a.Width * 0.12f, by - 8f, a.Width * 0.24f, 8f);
            if (Running)
                Smoke(g, cx, by - a.Height * 0.12f, a.Height * 0.3f);
        }
        // 环形长明灯：环管一周小火
        private void DrawRing(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.5f, cy = a.Top + a.Height * 0.56f, r = a.Width * 0.28f;
            // 环形气管
            g.DrawEllipse(new Pen(pal.MetalDark, 6f), cx - r, cy - r, r * 2f, r * 2f);
            g.DrawEllipse(new Pen(pal.Edge, 1.3f), cx - r, cy - r, r * 2f, r * 2f);
            // 进气管
            g.Line(pal.MetalDark, 5f, cx + r, cy + r, a.Right - 5f, cy + r + a.Height * 0.14f);
            if (Running)
                for (int i = 0; i < 14; i++)
                {
                    double an = i * Math.PI * 2.0 / 14.0;
                    float f = (float)Math.Sin(Phase * 5.5f + i) * 1.4f;
                    Flame(g, cx + (float)Math.Cos(an) * r, cy + (float)Math.Sin(an) * r,
                        a.Height * 0.13f + f, 3.6f, FireOrange, FireCream);
                }
            else
                for (int i = 0; i < 14; i++)
                {
                    double an = i * Math.PI * 2.0 / 14.0;
                    g.Ell(cx + (float)Math.Cos(an) * r, cy + (float)Math.Sin(an) * r, 1.8f, 1.8f,
                        Color.FromArgb(50, 54, 60));
                }
        }
        // ----------------------------------------------------------------
        // 共享小原语
        // ----------------------------------------------------------------
        /// <summary>竖直火焰（贝塞尔火舌 + 上下渐变色）。</summary>
        private static void Flame(Graphics g, float cx, float baseY, float height, float halfWidth,
            Color outer, Color inner)
        {
            using (var path = new GraphicsPath())
            {
                float tipX = cx + height * 0.06f;
                path.StartFigure();
                path.AddBezier(cx - halfWidth, baseY,
                    cx - halfWidth * 1.5f, baseY - height * 0.45f,
                    cx - halfWidth * 0.5f, baseY - height * 0.75f,
                    tipX, baseY - height);
                path.AddBezier(tipX, baseY - height,
                    cx + halfWidth * 0.6f, baseY - height * 0.7f,
                    cx + halfWidth * 1.5f, baseY - height * 0.4f,
                    cx + halfWidth, baseY);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(cx - halfWidth, baseY - height,
                    halfWidth * 2f, height), inner, outer, 90f))
                    g.FillPath(b, path);
            }
        }
        /// <summary>水平焰锥（喷嘴向右喷射）。</summary>
        private static void FlameCone(Graphics g, float bx, float by, float halfWidth, float len,
            Color outer, Color inner)
        {
            using (var path = new GraphicsPath())
            {
                path.StartFigure();
                path.AddBezier(bx, by - halfWidth,
                    bx + len * 0.35f, by - halfWidth * 1.25f,
                    bx + len * 0.72f, by - halfWidth * 0.35f,
                    bx + len, by);
                path.AddBezier(bx + len, by,
                    bx + len * 0.72f, by + halfWidth * 0.35f,
                    bx + len * 0.35f, by + halfWidth * 1.25f,
                    bx, by + halfWidth);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(bx, by - halfWidth, len, halfWidth * 2f),
                    outer, inner, 0f))
                    g.FillPath(b, path);
            }
        }
        /// <summary>上升烟气（三团渐隐灰烟）。</summary>
        private void Smoke(Graphics g, float cx, float yTop, float span)
        {
            for (int i = 0; i < 3; i++)
            {
                float t = (float)(Phase / (Math.PI * 2.0) + i / 3.0) % 1f;
                float r = 4f + t * 9f;
                int alpha = (int)(110 * (1f - t));
                using (var sb = new SolidBrush(Color.FromArgb(alpha, 110, 112, 116)))
                    g.FillEllipse(sb, cx + (float)Math.Sin(t * 6f + i) * 7f - r,
                        yTop - t * span - r, r * 2f, r * 2f);
            }
        }
        /// <summary>飞溅火花（以 baseAngle 为中心的扇形短线）。</summary>
        private void Sparks(Graphics g, float sx, float sy, double baseAngle, double spread,
            float len, int n)
        {
            using (var spark = new Pen(FireGold, 1.6f) { StartCap = LineCap.Round })
                for (int i = 0; i < n; i++)
                {
                    double h = (i * 1.7315) % 1.0;
                    float t = (float)(Phase / (Math.PI * 2.0) * 1.4 + h) % 1f;
                    double an = baseAngle + (h - 0.5) * spread + Math.Sin(Phase * 3f + i) * 0.05;
                    float d = t * len;
                    spark.Color = Color.FromArgb((int)(240 * (1f - t)), FireOrange);
                    g.DrawLine(spark, sx + (float)Math.Cos(an) * d, sy + (float)Math.Sin(an) * d,
                        sx + (float)Math.Cos(an) * (d + 4f), sy + (float)Math.Sin(an) * (d + 4f));
                }
        }
    }
}