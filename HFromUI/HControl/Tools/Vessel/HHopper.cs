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
    /// 料仓料斗样式：Classic 为圆筒锥底料仓，其余 21 种覆盖方仓、整体流/膨胀流仓、
    /// 称量斗、灰斗、旋风料斗、投料站、波纹钢板仓、柔性仓、活底料仓、移动翻斗等。
    /// </summary>
    public enum HHopperStyle
    {
        Classic = 0,        // 圆筒锥底料仓
        RectSilo = 1,       // 方形料仓
        MassFlow = 2,       // 整体流陡锥仓
        ExpandedFlow = 3,   // 膨胀流不对称仓
        DayBin = 4,         // 日用方锥斗
        SurgeBin = 5,       // 缓冲仓
        WeighHopper = 6,    // 悬挂称量斗
        LossInWeight = 7,   // 减重式料仓
        DustHopper = 8,     // 除尘器灰斗
        Cyclone = 9,        // 旋风分离器料斗
        BagDump = 10,       // 拆包投料站
        Octagonal = 11,     // 八角称重斗
        ConicalTank = 12,   // 支耳式锥底罐
        RibbedBin = 13,     // 波纹钢板仓
        WeldedSteel = 14,   // 带爬梯焊接仓
        FlexSilo = 15,      // 钢架柔性仓
        VacuumReceiver = 16,// 真空上料接收器
        LiveBottom = 17,    // 活底多螺杆仓
        VibratingBin = 18,  // 带振动器料斗
        AirCannon = 19,     // 空气炮清堵仓
        ForkliftBin = 20,   // 叉车移动料斗
        SelfDumping = 21    // 自翻卸料斗
    }
    /// <summary>
    /// 漏斗仓控件（继承 HBarBase）：22 种料仓料斗外形、13 种色调，
    /// Value 表示仓内料位百分比（0..100）。
    /// </summary>
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [HDescriptionLanguage("料仓料斗控件：22 种仓斗外形、13 种色调，Value 为料位百分比")]
    public class HHopper : HBarBase
    {
        private HToolTheme _theme = HToolTheme.Classic;
        private HHopperStyle _style = HHopperStyle.Classic;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        public HHopper()
        {
            Size = new Size(120, 170);
            Value = 60;
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("料斗"), HDisplayNameLanguage("仓体色调"), HDescriptionLanguage("仓体色调"), Browsable(true)]
        [DefaultValue(HToolTheme.Classic)]
        public HToolTheme Theme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>料仓料斗外形样式。</summary>
        [HCategoryLanguage("料斗"), HDisplayNameLanguage("料仓料斗外形样式"), HDescriptionLanguage("料仓料斗外形样式"), Browsable(true)]
        [DefaultValue(HHopperStyle.Classic)]
        public HHopperStyle HopperStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        private HToolPalette Pal => _theme == HToolTheme.Classic ? _classic : HToolPalettes.Get((int)_theme);
        // 骨料渐变
        private static readonly Color GrainTop = Color.FromArgb(196, 152, 96);
        private static readonly Color GrainBot = Color.FromArgb(140, 98, 56);
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Pal;
            var a = PlacedGlyphArea();
            switch (_style)
            {
                case HHopperStyle.RectSilo: DrawRect(g, a, pal, false); break;
                case HHopperStyle.DayBin: DrawRect(g, a, pal, true); break;
                case HHopperStyle.MassFlow: DrawRound(g, a, pal, 0.32f); break;
                case HHopperStyle.ExpandedFlow: DrawAsym(g, a, pal); break;
                case HHopperStyle.SurgeBin: DrawRound(g, a, pal, 0.5f); break;
                case HHopperStyle.WeighHopper: DrawWeigh(g, a, pal, true); break;
                case HHopperStyle.LossInWeight: DrawWeigh(g, a, pal, false); break;
                case HHopperStyle.DustHopper: DrawDust(g, a, pal, false); break;
                case HHopperStyle.Cyclone: DrawDust(g, a, pal, true); break;
                case HHopperStyle.BagDump: DrawBagDump(g, a, pal); break;
                case HHopperStyle.Octagonal: DrawOct(g, a, pal); break;
                case HHopperStyle.ConicalTank: DrawTank(g, a, pal); break;
                case HHopperStyle.RibbedBin: DrawRibbed(g, a, pal, false); break;
                case HHopperStyle.WeldedSteel: DrawRibbed(g, a, pal, true); break;
                case HHopperStyle.FlexSilo: DrawFlex(g, a, pal); break;
                case HHopperStyle.VacuumReceiver: DrawReceiver(g, a, pal); break;
                case HHopperStyle.LiveBottom: DrawLiveBottom(g, a, pal); break;
                case HHopperStyle.VibratingBin: DrawVibrating(g, a, pal); break;
                case HHopperStyle.AirCannon: DrawAirCannon(g, a, pal); break;
                case HHopperStyle.ForkliftBin: DrawMobile(g, a, pal, false); break;
                case HHopperStyle.SelfDumping: DrawMobile(g, a, pal, true); break;
                default: DrawClassic(g, a, pal); break;
            }
            PaintPlacedText(g);
        }
        /// <summary>在 cavity 内裁剪填充料位（top/bottom 为腔上下界）。</summary>
        private void PaintGrain(Graphics g, PointF[] cavity, float top, float bottom,
            float lx, float rx, float neckHalf)
        {
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(cavity);
                path.CloseFigure();
                var old = g.Clip;
                g.SetClip(path);
                float fillTop = bottom - Percent * (bottom - top);
                var fr = new RectangleF(Math.Min(lx, rx) - 2f, fillTop, Math.Abs(rx - lx) + 4f,
                    bottom - fillTop + 2f);
                using (var mb = HBarBase.Gradient(fr, GrainTop, GrainBot, 90f))
                    g.FillRectangle(mb, fr);
                g.DrawLine(new Pen(Color.FromArgb(104, 70, 38), 1.4f), lx - 2f, fillTop, rx + 2f, fillTop);
                g.Clip = old;
            }
        }
        /// <summary>半透明金属仓壁。</summary>
        private static void PaintShell(Graphics g, PointF[] shell, RectangleF gradR,
            HToolPalette pal, bool light)
        {
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(shell);
                path.CloseFigure();
                Color c1 = light ? Color.FromArgb(90, pal.Metal) : Color.FromArgb(70, pal.Metal);
                using (var b = HBarBase.Gradient(gradR, c1, Color.FromArgb(120, pal.MetalDark), 0f))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(pal.Edge, 1.6f), path);
            }
        }
        private void Legs4(Graphics g, float lx, float rx, float yTop, float yBot, HToolPalette pal)
        {
            using (var leg = new Pen(pal.MetalDark, 4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(leg, lx + 2f, yTop, lx - 4f, yBot);
                g.DrawLine(leg, rx - 2f, yTop, rx + 4f, yBot);
                g.DrawLine(leg, (lx + rx) / 2f - 3f, yTop + 2f, (lx + rx) / 2f - 3f, yBot);
                g.DrawLine(leg, (lx + rx) / 2f + 3f, yTop + 2f, (lx + rx) / 2f + 3f, yBot);
            }
        }
        // ----------------------------------------------------------------
        // Classic：圆筒锥底仓
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.62f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 10f, cylBot = top + a.Height * 0.4f;
            float neckW = w * 0.22f, neckY = top + a.Height * 0.72f;
            float legBot = a.Bottom - 4f;
            Legs4(g, lx, rx, cylBot - 2f, legBot, pal);
            var cavity = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckW / 2f, neckY),
                new PointF(cx - neckW / 2f, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, cavity, top, neckY, lx, rx, neckW / 2f);
            PaintShell(g, cavity, new RectangleF(lx, a.Top, w, neckY - a.Top), pal, false);
            using (var hl = new Pen(Color.FromArgb(90, Color.White), 2f))
                g.DrawLine(hl, lx + w * 0.18f, top + 2f, lx + w * 0.18f, cylBot - 2f);
            using (var rimB = new SolidBrush(pal.MetalDark))
                g.FillEllipse(rimB, lx - 3f, top - 5f, w + 6f, 10f);
            g.DrawEllipse(new Pen(pal.Edge, 1.2f), lx - 3f, top - 5f, w + 6f, 10f);
            using (var spout = new SolidBrush(pal.Metal))
                g.FillRectangle(spout, cx - neckW / 2f, neckY, neckW, a.Height * 0.1f);
            using (var flange = new SolidBrush(pal.MetalDark))
                g.FillRectangle(flange, cx - neckW * 0.9f, neckY + a.Height * 0.1f - 2f, neckW * 1.8f, 5f);
        }
        // ----------------------------------------------------------------
        // 新式仓斗
        // ----------------------------------------------------------------
        private void DrawRect(Graphics g, RectangleF a, HToolPalette pal, bool small)
        {
            // 方仓 / 日用小方斗
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * (small ? 0.66f : 0.7f);
            float lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + (small ? 14f : 10f);
            float bodyH = a.Height * (small ? 0.42f : 0.46f);
            float cylBot = top + bodyH;
            float neckY = a.Bottom - (small ? 26f : 20f);
            float neckHalf = w * 0.12f;
            Legs4(g, lx, rx, cylBot, a.Bottom - 4f, pal);
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, false);
            // 仓板竖筋
            using (var rib = new Pen(HToolPalettes.Shade(pal.Metal, 0.3f), 1.2f))
                for (int i = 1; i < 4; i++)
                    g.DrawLine(rib, lx + w * i / 4f, top + 2f, lx + w * i / 4f, cylBot - 2f);
            // 口沿
            g.Line(pal.MetalDark, 3f, lx - 3f, top, rx + 3f, top);
            // 出料管 + 闸板
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 10f), 1.5f, pal.Metal, pal.Edge, 1.1f);
            g.Line(pal.Edge, 2f, cx + neckHalf + 3f, neckY + 2f, cx + neckHalf + 3f, neckY + 12f);
        }
        private void DrawRound(Graphics g, RectangleF a, HToolPalette pal, float coneRatio)
        {
            // 圆筒 + 不同锥高（整体流陡锥 / 缓冲仓缓锥）
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.6f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 10f;
            float cylBot = top + a.Height * (0.56f - coneRatio * 0.3f);
            float neckY = cylBot + a.Height * coneRatio * 0.42f;
            float neckHalf = w * 0.14f;
            Legs4(g, lx, rx, cylBot, a.Bottom - 4f, pal);
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, false);
            using (var rim = new SolidBrush(pal.MetalDark))
                g.FillEllipse(rim, lx - 3f, top - 5f, w + 6f, 10f);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 10f), 1.5f, pal.Metal, pal.Edge, 1.1f);
        }
        private void DrawAsym(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 膨胀流：一侧直壁、一侧缓坡
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.66f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 10f, cylBot = top + a.Height * 0.34f;
            float neckY = a.Bottom - 18f, neckHalf = w * 0.12f;
            Legs4(g, lx, rx, cylBot, a.Bottom - 4f, pal);
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot + a.Height * 0.12f),
                new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY),
                new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, false);
            using (var rim = new SolidBrush(pal.MetalDark))
                g.FillEllipse(rim, lx - 3f, top - 5f, w + 6f, 10f);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 10f), 1.5f, pal.Metal, pal.Edge, 1.1f);
        }
        private void DrawWeigh(Graphics g, RectangleF a, HToolPalette pal, bool hung)
        {
            // 悬挂称量斗（三个称重传感器吊耳）/ 减重式（坐地传感器）
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.7f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 26f, neckY = a.Bottom - 22f, neckHalf = w * 0.1f;
            if (hung)
            {
                // 吊点 + 传感器（小方块）
                foreach (float hx in new[] { lx + 4f, rx - 4f })
                {
                    g.Line(pal.Edge, 2f, hx, a.Top + 4f, hx, top - 8f);
                    g.Box(new RectangleF(hx - 4f, top - 8f, 8f, 8f), 1.5f,
                        HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1f);
                }
            }
            else
            {
                // 座式传感器
                foreach (float hx in new[] { lx + 8f, rx - 8f })
                    g.Box(new RectangleF(hx - 5f, a.Bottom - 12f, 10f, 8f), 1.5f,
                        HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1f);
            }
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx - w * 0.12f, top + 10f),
                new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY),
                new PointF(lx + w * 0.12f, top + 10f)
            };
            PaintGrain(g, pts, top + 6f, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, true);
            // 上沿口
            g.Line(pal.MetalDark, 3f, lx, top, rx, top);
            // 卸料蝶阀
            g.Box(new RectangleF(cx - neckHalf - 2f, neckY, neckHalf * 2f + 4f, 9f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
            g.Ell(cx, neckY + 4.5f, 2.6f, 2.6f, pal.Accent);
        }
        private void DrawDust(Graphics g, RectangleF a, HToolPalette pal, bool cyclone)
        {
            // 除尘器灰斗：上方箱口 + 陡锥；cyclone 为旋风筒 + 锥 + 中心管
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * (cyclone ? 0.56f : 0.72f);
            float lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + (cyclone ? 22f : 10f);
            float cylBot = cyclone ? top + a.Height * 0.34f : top + a.Height * 0.24f;
            float neckY = a.Bottom - 18f, neckHalf = w * 0.1f;
            if (cyclone)
            {
                // 进风口（侧切矩形）+ 顶部中心排风管
                g.Box(new RectangleF(rx - 4f, top + 6f, 12f, 16f), 2f, pal.MetalDark, pal.Edge, 1.2f);
                g.Box(new RectangleF(cx - w * 0.1f, a.Top + 4f, w * 0.2f, top - a.Top + 4f), 2f,
                    pal.Metal, pal.Edge, 1.1f);
            }
            else
            {
                // 花板（顶部多孔板）
                g.Box(new RectangleF(lx - 2f, top - 6f, w + 4f, 8f), 1.5f, pal.MetalDark, pal.Edge, 1.1f);
            }
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top + 2f, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, false);
            // 卸灰阀（星形阀小壳体）
            g.Box(new RectangleF(cx - neckHalf - 4f, neckY, neckHalf * 2f + 8f, 12f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            g.Ell(cx, neckY + 6f, 3f, 3f, pal.Accent);
        }
        private void DrawBagDump(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 拆包投料站：工作台格栅 + 受料斗 + 除尘风口
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.78f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 30f, neckY = a.Bottom - 24f, neckHalf = w * 0.12f;
            // 支腿
            using (var leg = new Pen(pal.MetalDark, 4f))
            {
                g.DrawLine(leg, lx, top, lx - 2f, a.Bottom - 4f);
                g.DrawLine(leg, rx, top, rx + 2f, a.Bottom - 4f);
            }
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(cx + neckHalf, neckY), new PointF(cx - neckHalf, neckY)
            };
            PaintGrain(g, pts, top + 4f, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, true);
            // 格栅台面
            using (var grate = new Pen(pal.MetalDark, 1.6f))
                for (float x = lx + 4f; x < rx - 2f; x += 7f)
                    g.DrawLine(grate, x, top - 4f, x, top + 4f);
            g.Line(pal.MetalDark, 3f, lx - 4f, top - 4f, rx + 4f, top - 4f);
            // 防尘罩（半开）+ 除尘口
            g.Poly(new[]
            {
                new PointF(lx - 2f, top - 4f), new PointF(lx - 2f, a.Top + 8f),
                new PointF(cx, a.Top + 2f), new PointF(cx, top - 8f)
            }, Color.FromArgb(150, HToolPalettes.Tint(pal.Main, 0.3f)), pal.Edge, 1.2f);
            g.Box(new RectangleF(cx + 4f, a.Top + 4f, 12f, 10f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 10f), 1.5f, pal.Metal, pal.Edge, 1.1f);
        }
        private void DrawOct(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 八角称重斗：截顶八边形
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.72f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 24f, neckY = a.Bottom - 22f, neckHalf = w * 0.1f;
            foreach (float hx in new[] { lx + 4f, rx - 4f })
            {
                g.Line(pal.Edge, 2f, hx, a.Top + 4f, hx, top - 6f);
                g.Box(new RectangleF(hx - 4f, top - 6f, 8f, 7f), 1.5f,
                    HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1f);
            }
            float cut = w * 0.14f, vcut = w * 0.22f;
            var pts = new[]
            {
                new PointF(lx + cut, top), new PointF(rx - cut, top),
                new PointF(rx, top + cut), new PointF(rx - vcut, neckY),
                new PointF(cx + neckHalf, neckY), new PointF(cx - neckHalf, neckY),
                new PointF(lx + vcut, neckY), new PointF(lx, top + cut)
            };
            PaintGrain(g, pts, top + 4f, neckY, lx, rx, neckHalf);
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(pts);
                path.CloseFigure();
                using (var b = HBarBase.Gradient(new RectangleF(lx, top, w, neckY - top),
                    Color.FromArgb(90, pal.Metal), Color.FromArgb(130, pal.MetalDark), 0f))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(pal.Edge, 1.6f), path);
            }
            g.Box(new RectangleF(cx - neckHalf - 3f, neckY, neckHalf * 2f + 6f, 10f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
        }
        private void DrawTank(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 支耳式锥底罐：椭圆封头顶 + 支耳
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.64f, r = w / 2f;
            float lx = cx - r, rx = cx + r;
            float top = a.Top + 26f, cylBot = top + a.Height * 0.36f;
            float neckY = a.Bottom - 16f, neckHalf = w * 0.1f;
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, false);
            // 椭圆封头
            g.DrawArc(new Pen(pal.Edge, 1.6f), lx, top - r * 0.7f, w, r * 0.7f, 180f, 180f);
            using (var hd = new SolidBrush(Color.FromArgb(110, pal.Metal)))
                g.FillPie(hd, lx, top - r * 0.7f, w, r * 0.7f, 180f, 180f);
            // 支耳（四个小三角座）
            foreach (float bx in new[] { lx, rx })
                g.Poly(new[]
                {
                    new PointF(bx - 4f, cylBot), new PointF(bx + 8f, cylBot),
                    new PointF(bx + 6f, cylBot + 9f), new PointF(bx - 6f, cylBot + 9f)
                }, pal.MetalDark, pal.Edge, 1.1f);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 10f), 1.5f, pal.Metal, pal.Edge, 1.1f);
        }
        private void DrawRibbed(Graphics g, RectangleF a, HToolPalette pal, bool ladder)
        {
            // 波纹/焊接钢板高仓
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.56f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 8f, cylBot = top + a.Height * 0.56f;
            float neckY = a.Bottom - 14f, neckHalf = w * 0.12f;
            Legs4(g, lx, rx, cylBot, a.Bottom - 4f, pal);
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, false);
            // 波纹环筋
            using (var rib = new Pen(HToolPalettes.Shade(pal.Metal, 0.35f), 1.2f))
                for (float y = top + 8f; y < cylBot; y += 9f)
                    g.DrawLine(rib, lx + 1f, y, rx - 1f, y);
            if (ladder)
            {
                // 爬梯 + 顶部护栏
                float ldx = rx + 3f;
                g.Line(pal.Edge, 1.6f, ldx, top, ldx, cylBot);
                g.Line(pal.Edge, 1.6f, ldx + 6f, top, ldx + 6f, cylBot);
                for (float y = top + 4f; y < cylBot; y += 9f)
                    g.Line(pal.Edge, 1.2f, ldx, y, ldx + 6f, y);
            }
            using (var rim = new SolidBrush(pal.MetalDark))
                g.FillEllipse(rim, lx - 3f, top - 5f, w + 6f, 10f);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 8f), 1.5f, pal.Metal, pal.Edge, 1.1f);
        }
        private void DrawFlex(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 钢架柔性仓：四角立柱 + 软布仓体（褶皱）
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.62f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 12f, cylBot = top + a.Height * 0.42f;
            float neckY = a.Bottom - 16f, neckHalf = w * 0.14f;
            // 钢架
            using (var steel = new Pen(pal.MetalDark, 4f))
            {
                g.DrawLine(steel, lx - 5f, a.Top + 4f, lx - 5f, a.Bottom - 4f);
                g.DrawLine(steel, rx + 5f, a.Top + 4f, rx + 5f, a.Bottom - 4f);
                g.DrawLine(steel, lx - 5f, a.Top + 4f, rx + 5f, a.Top + 4f);
            }
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            // 软布仓（半透明）
            using (var path = new GraphicsPath())
            {
                path.AddPolygon(pts);
                using (var b = new SolidBrush(Color.FromArgb(120, HToolPalettes.Tint(pal.Main, 0.35f))))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(pal.Edge, 1.5f), path);
            }
            // 褶皱线
            using (var wr = new Pen(HToolPalettes.Shade(pal.Main, 0.1f), 1.1f))
                for (float y = top + 8f; y < neckY; y += 10f)
                    g.DrawLine(wr, lx + 3f, y, rx - 3f, y);
            g.Box(new RectangleF(cx - neckHalf, neckY, neckHalf * 2f, 8f), 2f, pal.MetalDark, pal.Edge, 1.1f);
        }
        private void DrawReceiver(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 真空吸料接收器：圆顶筒 + 真空口 + 视镜 + 翻转卸料
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.58f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 22f, cylBot = top + a.Height * 0.36f;
            float neckY = a.Bottom - 22f, neckHalf = w * 0.12f;
            Legs4(g, lx, rx, cylBot, a.Bottom - 4f, pal);
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(cx + neckHalf, neckY),
                new PointF(cx - neckHalf, neckY), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, neckY, lx, rx, neckHalf);
            PaintShell(g, pts, new RectangleF(lx, top, w, neckY - top), pal, true);
            // 圆顶 + 真空管口
            using (var dome = new SolidBrush(Color.FromArgb(120, pal.Metal)))
                g.FillPie(dome, lx, top - w * 0.28f, w, w * 0.28f, 180f, 180f);
            g.DrawArc(new Pen(pal.Edge, 1.5f), lx, top - w * 0.28f, w, w * 0.28f, 180f, 180f);
            g.Box(new RectangleF(cx - 5f, a.Top + 2f, 10f, 12f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 视镜
            g.Ell(lx + w * 0.28f, top + a.Height * 0.12f, 7f, 7f, Color.FromArgb(120, 160, 180));
            g.DrawEllipse(new Pen(pal.Edge, 1.1f), lx + w * 0.28f - 7f,
                top + a.Height * 0.12f - 7f, 14f, 14f);
            g.Box(new RectangleF(cx - neckHalf - 3f, neckY, neckHalf * 2f + 6f, 10f), 2f,
                pal.MetalDark, pal.Edge, 1.1f);
        }
        private void DrawLiveBottom(Graphics g, RectangleF a, HToolPalette pal)
        {
            // 活底料仓：方仓 + 底部并排多螺杆
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.72f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 12f, cylBot = top + a.Height * 0.46f;
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx, cylBot), new PointF(lx, cylBot)
            };
            PaintGrain(g, pts, top, cylBot, lx, rx, 0f);
            PaintShell(g, pts, new RectangleF(lx, top, w, cylBot - top), pal, false);
            // 螺杆槽（三根）
            float troughH = a.Height * 0.14f, ty = cylBot + 2f;
            for (int k = -1; k <= 1; k++)
            {
                float tx = cx + k * w * 0.26f, tw = w * 0.22f;
                g.Box(new RectangleF(tx - tw / 2f, ty, tw, troughH), 3f, pal.MetalDark, pal.Edge, 1.1f);
                using (var fl = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 2f))
                    for (float x = tx - tw / 2f + 4f; x < tx + tw / 2f; x += 8f)
                    {
                        g.DrawLine(fl, x, ty + 3f, x + 7f, ty + troughH - 3f);
                        g.DrawLine(fl, x, ty + troughH - 3f, x + 7f, ty + 3f);
                    }
            }
            g.Line(pal.Edge, 3f, lx - 4f, ty + troughH / 2f, rx + 4f, ty + troughH / 2f);
        }
        private void DrawVibrating(Graphics g, RectangleF a, HToolPalette pal)
        {
            // Classic 仓型 + 锥部仓壁振动器
            DrawClassic(g, a, pal);
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.62f, rx = cx + w / 2f;
            float vy = a.Top + 10f + a.Height * 0.6f;
            // 振动电机（小圆柱 + 偏心块）
            g.Motor(rx + 2f, rx + 20f, vy, 7f, pal);
            var st = g.Save();
            g.TranslateTransform(rx + 22f, vy);
            g.RotateTransform(45f);
            using (var wb = new SolidBrush(Color.FromArgb(96, 100, 106)))
                g.FillPie(wb, -6f, -4f, 12f, 8f, 0f, 180f);
            g.Restore(st);
            // 振动波纹
            using (var arc = new Pen(Color.FromArgb(150, pal.Accent), 1.3f))
                for (int k = 0; k < 2; k++)
                {
                    float rr = 8f + k * 6f;
                    g.DrawArc(arc, rx + 18f - rr, vy - rr, rr * 2f, rr * 2f, 120f, 40f);
                }
        }
        private void DrawAirCannon(Graphics g, RectangleF a, HToolPalette pal)
        {
            // Classic 仓型 + 锥部空气炮（储气筒 + 喷嘴）
            DrawClassic(g, a, pal);
            float cx = a.Left + a.Width / 2f;
            float w = a.Width * 0.62f, lx = cx - w / 2f;
            float ay = a.Top + a.Height * 0.62f;
            // 储气罐（右侧横筒 + 支架）
            var tank = new RectangleF(a.Right - 34f, ay - 8f, 26f, 16f);
            g.FillCylH(tank, HToolPalettes.Shade(pal.Main, 0.2f), HToolPalettes.Tint(pal.Main, 0.2f));
            g.DrawR(tank, 6f, pal.Edge, 1.2f);
            // 喷嘴斜插入仓
            g.Line(pal.MetalDark, 3.4f, a.Right - 34f, ay, lx + w * 0.28f, ay - 12f);
            g.Bolt(a.Right - 34f, ay, 2.4f, pal.Edge);
        }
        private void DrawMobile(Graphics g, RectangleF a, HToolPalette pal, bool dump)
        {
            // 叉车移动料斗 / 自翻斗：锥斗 + 底盘叉孔 + 行走轮/翻转铰链
            float cx = a.Left + a.Width / 2f;
            var st = g.Save();
            float w = a.Width * 0.72f, lx = cx - w / 2f, rx = cx + w / 2f;
            float top = a.Top + 20f, bodyBot = top + a.Height * 0.34f;
            float mouthY = a.Bottom - 26f, mh = w * 0.16f;
            var pts = new[]
            {
                new PointF(lx, top), new PointF(rx, top),
                new PointF(rx - w * 0.1f, bodyBot + 10f),
                new PointF(cx + mh, mouthY + (dump ? 10f : 0f)),
                new PointF(cx - mh, mouthY + (dump ? 10f : 0f)),
                new PointF(lx + w * 0.1f, bodyBot + 10f)
            };
            if (dump)
            {
                // 翻转坐标下重算（以枢轴为基准）
                g.Restore(st);
                st = g.Save();
                g.TranslateTransform(cx, a.Bottom - 30f);
                g.RotateTransform(-24f);
                pts = new[]
                {
                    new PointF(lx - cx, -a.Height * 0.52f), new PointF(rx - cx, -a.Height * 0.52f),
                    new PointF(rx - cx - w * 0.1f, -a.Height * 0.22f),
                    new PointF(-mh, 0f), new PointF(mh, 0f),
                    new PointF(lx - cx + w * 0.1f, -a.Height * 0.22f)
                };
                lx -= cx; rx -= cx; top = a.Top + 20f - (a.Bottom - 30f);
                mouthY = 0f;
            }
            PaintGrain(g, pts, dump ? -a.Height * 0.5f : top, mouthY,
                dump ? -w / 2f : lx, dump ? w / 2f : rx, mh);
            PaintShell(g, pts, new RectangleF(dump ? -w / 2f : lx, dump ? -a.Height * 0.52f : top,
                w, a.Height * 0.55f), pal, false);
            g.Restore(st);
            // 底盘架 + 叉孔
            float fy = a.Bottom - 18f;
            g.Box(new RectangleF(a.Left + a.Width * 0.12f, fy, a.Width * 0.76f, 8f), 2f,
                pal.MetalDark, pal.Edge, 1.2f);
            using (var hole = new SolidBrush(Color.FromArgb(40, 42, 48)))
            {
                g.FillRectangle(hole, a.Left + a.Width * 0.2f, fy + 1.5f, a.Width * 0.16f, 5f);
                g.FillRectangle(hole, a.Right - a.Width * 0.36f, fy + 1.5f, a.Width * 0.16f, 5f);
            }
            // 轮 / 翻转铰链
            if (dump)
                g.Ell(cx, a.Bottom - 14f, 5f, 5f, pal.Accent);
            else
            {
                g.Ell(a.Left + a.Width * 0.24f, a.Bottom - 8f, 7f, 7f, Color.FromArgb(60, 64, 70));
                g.Ell(a.Right - a.Width * 0.24f, a.Bottom - 8f, 7f, 7f, Color.FromArgb(60, 64, 70));
            }
        }
    }
}