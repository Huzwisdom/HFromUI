using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Valve
{
    using HFromUI.HControl.Base;
    /// <summary>阀门样式：Classic 与旧版 HFrom.HValves 完全一致，其余 21 种为新式阀门。</summary>
    public enum HValvesStyle
    {
        Classic = 0,          // 经典手动阀（旧版原样）
        BallValve = 1,        // 球阀
        GateValve = 2,        // 闸阀
        ButterflyValve = 3,   // 蝶阀
        CheckValve = 4,       // 止回阀
        NeedleValve = 5,      // 针型阀
        SafetyValve = 6,      // 安全阀
        ThreeWayValve = 7,    // 三通阀
        AngleValve = 8,       // 角阀
        DiaphragmValve = 9,   // 隔膜阀
        SolenoidValve = 10,   // 电磁阀
        PneumaticValve = 11,  // 气动薄膜阀
        HandwheelValve = 12,  // 大手轮阀
        LeverValve = 13,      // 重锤杠杆阀
        PlugValve = 14,       // 旋塞阀
        GlobeValve = 15,      // 截止阀
        SteamTrap = 16,       // 疏水阀
        Regulator = 17,       // 减压阀
        PinchValve = 18,      // 夹管阀
        MultiPortValve = 19,  // 四通阀
        BlindFlange = 20,     // 8 字盲板
        MeteringValve = 21    // 计量阀
    }
    /// <summary>色调：Classic 为旧版灰边红棕执行机构配色，1..12 取现代色调表。</summary>
    public enum HValvesTheme
    {
        Classic = 0, 天蓝 = 1, 海蓝 = 2, 翠绿 = 3, 墨青 = 4, 琥珀 = 5, 橙 = 6,
        玫瑰 = 7, 红 = 8, 紫 = 9, 品红 = 10, 咖啡 = 11, 石墨 = 12
    }
    /// <summary>
    /// 阀门控件（继承 HLabelBase）：22 种阀型、13 种色调、横/纵管道方向、开/关两态，
    /// Classic 样式 + Classic 色调与旧版 HFrom.HValves 的形状和颜色完全一致。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("阀门控件：22 种阀型、13 种色调，支持横/纵方向与开关状态")]
    public class HValves : HLabelBase
    {
        private HValvesStyle _style = HValvesStyle.Classic;
        private HValvesTheme _theme = HValvesTheme.Classic;
        private Color _edgeColor = Color.Gray;
        private bool _open = true;
        private HBarOrientation _orientation = HBarOrientation.Horizontal;
        private StringFormat _sf;
        // 开/关两态固定状态色（与主题对撞，醒目）
        private static readonly Color OpenGreen = Color.FromArgb(60, 174, 103);
        private static readonly Color ShutRed = Color.FromArgb(210, 45, 45);
        public HValves()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(177, 90);
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            _sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        }
        /// <summary>阀门样式。</summary>
        [HCategoryLanguage("阀门"), HDisplayNameLanguage("阀门外形样式"), HDescriptionLanguage("阀门外形样式，Classic 与旧版完全一致"), Browsable(true)]
        [DefaultValue(HValvesStyle.Classic)]
        public HValvesStyle ValvesStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("阀门"), HDisplayNameLanguage("阀门色调"), HDescriptionLanguage("阀门色调，Classic 为旧版灰边红棕配色"), Browsable(true)]
        [DefaultValue(HValvesTheme.Classic)]
        public HValvesTheme ValvesTheme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>边缘颜色（Classic 样式生效，等同旧版 EdgeColor）。</summary>
        [HCategoryLanguage("阀门"), HDisplayNameLanguage("边缘颜色"), HDescriptionLanguage("阀门边缘颜色，Classic 样式下与旧版一致"), Browsable(true)]
        [DefaultValue(typeof(Color), "Gray")]
        public Color EdgeColor
        {
            get => _edgeColor;
            set { _edgeColor = value; Invalidate(); }
        }
        /// <summary>是否处于开启状态（现代样式中手柄/挡板/指示灯随状态变化）。</summary>
        [HCategoryLanguage("阀门"), HDisplayNameLanguage("阀门是否开启"), HDescriptionLanguage("阀门是否开启，开启为绿色、关闭为红色"), Browsable(true)]
        [DefaultValue(true)]
        public bool Open
        {
            get => _open;
            set { _open = value; Invalidate(); }
        }
        /// <summary>管道方向：横向/纵向。</summary>
        [HCategoryLanguage("阀门"), HDisplayNameLanguage("管道方向"), HDescriptionLanguage("管道方向：横向或纵向（纵向旋转 90 度绘制）"), Browsable(true)]
        [DefaultValue(HBarOrientation.Horizontal)]
        public HBarOrientation PipeLineStyle
        {
            get => _orientation;
            set { _orientation = value; Invalidate(); }
        }
        /// <summary>阀门文本。</summary>
        [HCategoryLanguage("阀门"), HDisplayNameLanguage("绘制在阀体上的文本"), HDescriptionLanguage("绘制在阀体上的文本"), Browsable(true)]
        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            if (Width < 6 || Height < 6) return;
            if (_orientation == HBarOrientation.Horizontal)
            {
                PaintAll(g, Width, Height);
            }
            else
            {
                g.TranslateTransform(Width, 0f);
                g.RotateTransform(90f);
                PaintAll(g, Height, Width);
                g.ResetTransform();
            }
        }
        private void PaintAll(Graphics g, float width, float height)
        {
            if (_style == HValvesStyle.Classic) PaintClassic(g, width, height);
            else PaintModern(g, new RectangleF(0f, 0f, width, height));
        }
        // ------------------------------------------------------------------
        // Classic：旧版 HFrom.HValves 原样移植（横管/手轮/阀杆/阀体/红棕执行机构）
        // ------------------------------------------------------------------
        private void PaintClassic(Graphics g, float width, float height)
        {
            g.TranslateTransform(width / 2f, 0f);
            // Classic 色调沿用旧版灰管红轮；其他色调：管路仍为金属灰，阀体随主题、手轮随 Accent
            bool themed = _theme != HValvesTheme.Classic;
            HToolPalette pal = themed ? HToolPalettes.Get((int)_theme) : default(HToolPalette);
            Color pipeColor = themed ? pal.MetalDark : _edgeColor;
            Color stemColor = themed ? pal.MetalDark : Color.FromArgb(144, 162, 167);
            Color bodyA = themed ? pal.Dark : Color.FromArgb(190, 190, 190);
            Color bodyB = themed ? pal.Main : Color.FromArgb(170, 175, 175);
            Color wheelA = themed ? HToolPalettes.Shade(pal.Accent, 0.20f) : Color.FromArgb(172, 77, 80);
            Color wheelLight = themed ? HToolPalettes.Tint(pal.Accent, 0.55f) : Color.FromArgb(244, 188, 189);
            Color wheelC = themed ? HToolPalettes.Shade(pal.Accent, 0.28f) : Color.FromArgb(165, 74, 77);
            Color wheelD = themed ? HToolPalettes.Shade(pal.Accent, 0.22f) : Color.FromArgb(184, 87, 90);
            var cb = new ColorBlend
            {
                Positions = new[] { 0f, 0.25f, 1f },
                Colors = new[] { pipeColor, Color.WhiteSmoke, pipeColor }
            };
            LinearGradientBrush lgb = new LinearGradientBrush(
                new PointF(0f, height * 0.34f), new PointF(0f, height * 0.93f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            g.FillRectangle(lgb, -width * 0.5f - 1f, height * 0.38f, width + 1f, height * 0.55f);
            lgb.Dispose();
            lgb = new LinearGradientBrush(new PointF(0f, height * 0.25f), new PointF(0f, height * 1.1f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            g.FillRectangle(lgb, -width * 0.45f, height * 0.31f, width * 0.18f, height * 0.69f);
            g.FillRectangle(lgb, width * 0.27f, height * 0.31f, width * 0.18f, height * 0.69f);
            lgb.Dispose();
            cb.Positions = new[] { 0f, 0.31f, 1f };
            cb.Colors = new[] { stemColor, Color.WhiteSmoke, stemColor };
            lgb = new LinearGradientBrush(new PointF(-width * 0.07f, 0f), new PointF(width * 0.07f, 0f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            g.FillRectangle(lgb, -width * 0.05f, height * 0.1f, width * 0.1f, height * 0.13f);
            lgb.Dispose();
            cb.Positions = new[] { 0f, 0.21f, 1f };
            cb.Colors = new[] { bodyA, Color.WhiteSmoke, bodyB };
            lgb = new LinearGradientBrush(new PointF(-width * 0.1f, 0f), new PointF(width * 0.1f, 0f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            g.FillEllipse(lgb, -width * 0.1f, height * 0.34f, width * 0.2f, height * 0.08f);
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddLines(new[]
                {
                    new PointF(-width * 0.1f, height * 0.38f),
                    new PointF(-width * 0.07f, height * 0.22f),
                    new PointF(width * 0.07f, height * 0.22f),
                    new PointF(width * 0.1f, height * 0.38f)
                });
                gp.CloseFigure();
                g.FillPath(lgb, gp);
            }
            lgb.Dispose();
            cb.Positions = new[] { 0f, 0.78f, 1f };
            cb.Colors = new[] { wheelA, wheelLight, wheelA };
            lgb = new LinearGradientBrush(new PointF(-width * 0.23f, 0f), new PointF(width * 0.23f, 0f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            g.FillRectangle(lgb, -width * 0.23f, 0f, width * 0.46f, height * 0.15f);
            lgb.Dispose();
            cb.Positions = new[] { 0f, 0.7f, 1f };
            cb.Colors = new[] { wheelC, wheelLight, wheelD };
            lgb = new LinearGradientBrush(new PointF(0f, 0f), new PointF(width * 0.06f, 0f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke);
            lgb.InterpolationColors = cb;
            g.TranslateTransform(-width * 0.03f, 0f);
            g.FillRectangle(lgb, 0f, 0f, width * 0.06f, height * 0.15f);
            g.TranslateTransform(-width * 0.2f, 0f);
            g.FillRectangle(lgb, 0f, 0f, width * 0.06f, height * 0.15f);
            g.TranslateTransform(width * 0.4f, 0f);
            g.FillRectangle(lgb, 0f, 0f, width * 0.06f, height * 0.15f);
            g.TranslateTransform(-width * 0.17f, 0f);
            lgb.Dispose();
            using (var b = new SolidBrush(ForeColor))
                g.DrawString(Text, Font, b, new RectangleF(-width / 2f, height * 0.38f, width, height * 0.55f), _sf);
            g.TranslateTransform(-width / 2f, 0f);
        }
        // ------------------------------------------------------------------
        // 现代样式（横向画布 c：左进右出，管心在 c 高 50% 处）
        // ------------------------------------------------------------------
        private HToolPalette Palette => _theme == HValvesTheme.Classic
            ? new HToolPalette
            {
                Main = Color.FromArgb(96, 132, 150), Light = Color.FromArgb(150, 180, 194),
                Lighter = Color.FromArgb(224, 234, 239), Dark = Color.FromArgb(60, 84, 98),
                Edge = Color.Gray, Glass = Color.FromArgb(247, 252, 253), GlassEdge = Color.FromArgb(142, 196, 216),
                Metal = Color.FromArgb(228, 230, 233), MetalDark = Color.FromArgb(148, 152, 158), Accent = Color.FromArgb(172, 77, 80)
            }
            : HToolPalettes.Get((int)_theme);
        private Color StateColor => _open ? OpenGreen : ShutRed;
        private void PaintModern(Graphics g, RectangleF c)
        {
            var pal = Palette;
            float cy = c.Y + c.Height * 0.50f;
            float ph = c.Height * 0.16f;                 // 管外径
            float bodyL = c.Width * 0.30f;
            float bodyR = c.Width * 0.70f;
            var body = new RectangleF(bodyL, c.Y + c.Height * 0.30f, bodyR - bodyL, c.Height * 0.44f);
            switch (_style)
            {
                case HValvesStyle.BallValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawBall(g, body, pal, cy, ph); break;
                case HValvesStyle.GateValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawBowBody(g, body, pal, cy, ph); DrawStemWheel(g, c, pal, cy); break;
                case HValvesStyle.ButterflyValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawButterfly(g, body, pal, cy, ph); break;
                case HValvesStyle.CheckValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawCheck(g, body, pal, cy, ph, c); break;
                case HValvesStyle.NeedleValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawNeedle(g, c, body, pal, cy, ph); break;
                case HValvesStyle.SafetyValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawSafety(g, c, body, pal, cy, ph); break;
                case HValvesStyle.ThreeWayValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawThreeWay(g, c, body, pal, cy, ph); break;
                case HValvesStyle.AngleValve: DrawAngle(g, c, pal, cy, ph); break;
                case HValvesStyle.DiaphragmValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawDiaphragm(g, c, body, pal, cy, ph); break;
                case HValvesStyle.SolenoidValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawSolenoid(g, c, body, pal, cy, ph); break;
                case HValvesStyle.PneumaticValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawPneumatic(g, c, body, pal, cy, ph); break;
                case HValvesStyle.HandwheelValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawBowBody(g, body, pal, cy, ph); DrawWheel(g, c.Width * 0.5f, c.Height * 0.075f, c.Height * 0.15f, pal); break;
                case HValvesStyle.LeverValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawBowBody(g, body, pal, cy, ph); DrawLever(g, c, pal, cy); break;
                case HValvesStyle.PlugValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawPlug(g, c, body, pal, cy, ph); break;
                case HValvesStyle.GlobeValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawGlobe(g, body, pal, cy, ph); DrawStemWheel(g, c, pal, cy); break;
                case HValvesStyle.SteamTrap: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawSteamTrap(g, c, body, pal, cy, ph); break;
                case HValvesStyle.Regulator: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawRegulator(g, c, body, pal, cy, ph); break;
                case HValvesStyle.PinchValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawPinch(g, c, body, pal, cy, ph); break;
                case HValvesStyle.MultiPortValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawMultiPort(g, c, body, pal, cy, ph); break;
                case HValvesStyle.BlindFlange: DrawBlind(g, c, pal, cy, ph); break;
                case HValvesStyle.MeteringValve: DrawPipes(g, c, pal, cy, ph, bodyL, bodyR); DrawMetering(g, c, body, pal, cy, ph); break;
            }
            HBarBase.DrawText(g, Text, Font, ForeColor,
                new RectangleF(c.X, c.Bottom - c.Height * 0.20f, c.Width, c.Height * 0.18f));
        }
        /// <summary>左右管段 + 法兰盘（公共管架）。</summary>
        private void DrawPipes(Graphics g, RectangleF c, HToolPalette pal, float cy, float ph, float bodyL, float bodyR)
        {
            float top = cy - ph / 2f;
            using (var pb = HBarBase.CylinderV(new RectangleF(0f, top, c.Width, ph),
                pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
            {
                g.FillRectangle(pb, c.X, top, bodyL - c.X + 1f, ph);
                g.FillRectangle(pb, bodyR, top, c.Right - bodyR + 1f, ph);
            }
            // 管内壁暗线
            using (var lp = new Pen(HToolPalettes.Tint(pal.MetalDark, 0.25f), 1f))
            {
                g.DrawLine(lp, c.X, top + 1.2f, bodyL, top + 1.2f);
                g.DrawLine(lp, bodyR, top + 1.2f, c.Right, top + 1.2f);
            }
            float fw = Math.Max(2.5f, c.Width * 0.028f);
            using (var fb = new SolidBrush(pal.Metal))
            using (var ep = new Pen(pal.Edge, 1.1f))
                foreach (float fx in new[] { bodyL - fw, bodyR })
                {
                    var fr = new RectangleF(fx, top - ph * 0.28f, fw, ph * 1.56f);
                    g.FillRectangle(fb, fr);
                    g.DrawRectangle(ep, fr.X, fr.Y, fr.Width, fr.Height);
                    // 螺栓点
                    float br = Math.Max(1f, ph * 0.07f);
                    g.FillEllipse(Brushes.DimGray, fx + fw / 2f - br, fr.Y + fr.Height * 0.16f - br, br * 2f, br * 2f);
                    g.FillEllipse(Brushes.DimGray, fx + fw / 2f - br, fr.Bottom - fr.Height * 0.16f - br, br * 2f, br * 2f);
                }
        }
        /// <summary>阀杆 + 顶部手轮（闸阀/截止阀通用）。</summary>
        private void DrawStemWheel(Graphics g, RectangleF c, HToolPalette pal, float cy)
        {
            float cx = c.Width * 0.5f;
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.5f, c.Height * 0.03f)))
                g.DrawLine(sp, cx, cy - c.Height * 0.06f, cx, c.Height * 0.155f);
            DrawWheel(g, cx, c.Height * 0.075f, c.Height * 0.105f, pal);
        }
        /// <summary>手轮（圆环 + 三辐条 + 轮毂）。</summary>
        private void DrawWheel(Graphics g, float cx, float cy, float r, HToolPalette pal)
        {
            float lw = Math.Max(1.4f, r * 0.22f);
            using (var wp = new Pen(_open ? pal.MetalDark : StateColor, lw))
                g.DrawEllipse(wp, cx - r, cy - r, r * 2f, r * 2f);
            using (var sp = new Pen(pal.MetalDark, Math.Max(1f, lw * 0.55f)))
                for (int i = 0; i < 3; i++)
                {
                    double a = Math.PI / 2d + i * Math.PI * 2d / 3d;
                    g.DrawLine(sp, cx, cy, cx + (float)Math.Cos(a) * r * 0.85f, cy + (float)Math.Sin(a) * r * 0.85f);
                }
            using (var hb = new SolidBrush(pal.Edge))
                g.FillEllipse(hb, cx - r * 0.18f, cy - r * 0.18f, r * 0.36f, r * 0.36f);
        }
        /// <summary>蝴蝶结形阀体（闸阀/手轮阀通用）。</summary>
        private void DrawBowBody(Graphics g, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = (b.Left + b.Right) / 2f;
            var pts = new[]
            {
                new PointF(b.Left, cy), new PointF(cx - ph * 0.55f, b.Top),
                new PointF(cx + ph * 0.55f, b.Top), new PointF(b.Right, cy),
                new PointF(cx + ph * 0.55f, b.Bottom), new PointF(cx - ph * 0.55f, b.Bottom)
            };
            using (var bb = new SolidBrush(pal.Main))
                g.FillPolygon(bb, pts);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawPolygon(ep, pts);
            // 中腔高光
            using (var hb = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.45f)))
                g.FillEllipse(hb, cx - ph * 0.42f, cy - ph * 0.42f, ph * 0.84f, ph * 0.84f);
        }
        /// <summary>球阀：球形阀体 + 顶装杠杆手柄（开=与管平行，关=竖直）。</summary>
        private void DrawBall(Graphics g, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = (b.Left + b.Right) / 2f;
            float r = ph * 1.15f;
            using (var bb = HBarBase.CylinderH(new RectangleF(cx - r, cy - r, r * 2f, r * 2f), pal.Main, HToolPalettes.Tint(pal.Main, 0.6f)))
                g.FillEllipse(bb, cx - r, cy - r, r * 2f, r * 2f);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawEllipse(ep, cx - r, cy - r, r * 2f, r * 2f);
            // 球体通道：开=横缝，关=竖缝
            using (var cb = new SolidBrush(HToolPalettes.Tint(pal.Dark, 0.15f)))
                if (_open) g.FillRectangle(cb, cx - r * 0.85f, cy - ph * 0.22f, r * 1.7f, ph * 0.44f);
                else g.FillRectangle(cb, cx - ph * 0.22f, cy - r * 0.85f, ph * 0.44f, r * 1.7f);
            // 阀杆 + 手柄
            float hy = b.Top + r * 0.25f;
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.5f, ph * 0.12f)))
                g.DrawLine(sp, cx, cy - r * 0.85f, cx, hy);
            using (var hb = new SolidBrush(StateColor))
            {
                if (_open) g.FillRectangle(hb, cx - r * 1.15f, hy - ph * 0.13f, r * 2.3f, ph * 0.26f);
                else g.FillRectangle(hb, cx - ph * 0.13f, hy - r * 1.15f, ph * 0.26f, r * 2.3f);
            }
        }
        /// <summary>蝶阀：对夹圆阀体 + 碟板（开=侧视细线，关=横板）。</summary>
        private void DrawButterfly(Graphics g, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = (b.Left + b.Right) / 2f;
            float r = ph * 1.1f;
            using (var bb = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.5f)))
                g.FillEllipse(bb, cx - r, cy - r, r * 2f, r * 2f);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawEllipse(ep, cx - r, cy - r, r * 2f, r * 2f);
            using (var dp = new Pen(StateColor, Math.Max(2f, ph * (_open ? 0.12f : 0.42f))))
                g.DrawLine(dp, cx, cy - r * 0.85f, cx, cy + r * 0.85f);
            float hy = b.Top + ph * 0.2f;
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.4f, ph * 0.1f)))
                g.DrawLine(sp, cx, cy - r, cx, hy);
            using (var hb = new SolidBrush(StateColor))
                g.FillRectangle(hb, cx - r * 0.75f, hy - ph * 0.12f, r * 1.5f, ph * 0.24f);
        }
        /// <summary>止回阀：绕上铰链的拍门 + 流向箭头。</summary>
        private void DrawCheck(Graphics g, RectangleF b, HToolPalette pal, float cy, float ph, RectangleF c)
        {
            float cx = (b.Left + b.Right) / 2f;
            float r = ph * 1.05f;
            using (var bb = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.55f)))
                g.FillEllipse(bb, cx - r, cy - r, r * 2f, r * 2f);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawEllipse(ep, cx - r, cy - r, r * 2f, r * 2f);
            // 拍门：开=绕上铰链斜开 35°，关=垂直封死
            double ang = _open ? Math.PI * 0.22d : 0d;
            float dx = (float)Math.Sin(ang) * r * 1.55f, dy = (float)Math.Cos(ang) * r * 1.55f;
            using (var dp = new Pen(StateColor, Math.Max(2.5f, ph * 0.16f)))
                g.DrawLine(dp, cx, cy - r * 0.8f, cx + dx, cy - r * 0.8f + dy);
            g.FillEllipse(Brushes.DimGray, cx - ph * 0.1f, cy - r * 0.92f, ph * 0.2f, ph * 0.2f);
            // 流向箭头（左管内）
            float ax = c.Width * 0.16f, ar = ph * 0.32f;
            using (var ap = new Pen(StateColor, Math.Max(1.4f, ph * 0.1f)))
            {
                g.DrawLine(ap, ax - ar, cy, ax + ar, cy);
                g.DrawLine(ap, ax + ar, cy, ax, cy - ar * 0.7f);
                g.DrawLine(ap, ax + ar, cy, ax, cy + ar * 0.7f);
            }
        }
        /// <summary>针型阀：斜 45° 阀盖 + 细针杆 + 小手轮。</summary>
        private void DrawNeedle(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            // 斜阀体（右上 45° 鼓包）
            using (var bp = new Pen(pal.Dark, Math.Max(2f, ph * 0.28f)))
            {
                g.DrawLine(bp, cx, cy, cx + b.Width * 0.26f, b.Top + ph * 0.1f);
                g.DrawLine(bp, cx, cy, cx - b.Width * 0.10f, b.Top + ph * 0.42f);
            }
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - ph * 0.6f, cy - ph * 0.6f, ph * 1.2f, ph * 1.2f);
            using (var ep = new Pen(pal.Dark, 1.2f))
                g.DrawEllipse(ep, cx - ph * 0.6f, cy - ph * 0.6f, ph * 1.2f, ph * 1.2f);
            // 针杆沿斜线延伸到手轮
            float wx = cx + b.Width * 0.42f, wy = b.Top - ph * 0.28f;
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.2f, ph * 0.08f)))
                g.DrawLine(sp, cx + b.Width * 0.26f, b.Top + ph * 0.1f, wx, wy);
            DrawWheel(g, wx, wy, ph * 0.55f, pal);
        }
        /// <summary>安全阀：弹簧腔 + 顶部弯管排气口，开启时冒汽。</summary>
        private void DrawSafety(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - ph * 0.7f, cy - ph * 0.7f, ph * 1.4f, ph * 1.4f);
            using (var ep = new Pen(pal.Dark, 1.2f))
                g.DrawEllipse(ep, cx - ph * 0.7f, cy - ph * 0.7f, ph * 1.4f, ph * 1.4f);
            // 弹簧腔
            float capW = ph * 1.05f, capTop = c.Height * 0.10f, capH = cy - ph * 0.7f - capTop;
            var cap = new RectangleF(cx - capW / 2f, capTop, capW, capH);
            using (var cb = HBarBase.CylinderV(cap, pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(cb, cap);
            using (var ep = new Pen(pal.Edge, 1.1f))
                g.DrawRectangle(ep, cap.X, cap.Y, cap.Width, cap.Height);
            // 弹簧（之字线）
            using (var sp = new Pen(StateColor, 1.3f))
                for (int i = 0; i < 5; i++)
                {
                    float y1 = cap.Y + capH * (0.15f + i * 0.17f), y2 = y1 + capH * 0.085f;
                    g.DrawLine(sp, cx - capW * 0.3f, y1, cx + capW * 0.3f, y2);
                }
            // 顶部弯管排气口
            using (var pp = new Pen(pal.MetalDark, Math.Max(2f, ph * 0.18f)))
            {
                g.DrawLine(pp, cx, capTop, cx, capTop - ph * 0.25f);
                g.DrawLine(pp, cx, capTop - ph * 0.25f, cx + ph * 0.8f, capTop - ph * 0.25f);
            }
            if (_open)
                using (var steam = new Pen(HToolPalettes.Tint(pal.MetalDark, 0.65f), 1.4f))
                    for (int i = 0; i < 3; i++)
                    {
                        float sx = cx + ph * (0.85f + i * 0.22f), sy = capTop - ph * 0.3f;
                        g.DrawArc(steam, sx - ph * 0.14f, sy - ph * (0.5f + i * 0.18f), ph * 0.28f, ph * 0.28f, 200f, 140f);
                    }
        }
        /// <summary>三通阀：T 形三路管 + 中央球 + 小手轮。</summary>
        private void DrawThreeWay(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            using (var pb = HBarBase.CylinderV(new RectangleF(0f, 0f, ph, cy), pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(pb, cx - ph / 2f, c.Y, ph, cy - c.Y);
            // 顶部法兰
            float fw = Math.Max(2.5f, c.Width * 0.028f);
            var frTop = new RectangleF(cx - ph * 0.78f, c.Y, ph * 1.56f, fw);
            using (var fb = new SolidBrush(pal.Metal))
            using (var ep = new Pen(pal.Edge, 1.1f))
            {
                g.FillRectangle(fb, frTop);
                g.DrawRectangle(ep, frTop.X, frTop.Y, frTop.Width, frTop.Height);
            }
            float r = ph * 0.95f;
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - r, cy - r, r * 2f, r * 2f);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawEllipse(ep, cx - r, cy - r, r * 2f, r * 2f);
            using (var hb = new SolidBrush(StateColor))
                g.FillEllipse(hb, cx - r * 0.28f, cy - r * 0.28f, r * 0.56f, r * 0.56f);
            // 小手轮装在球侧（斜 45°）
            DrawWheel(g, cx + r * 1.25f, cy - r * 1.25f, r * 0.55f, pal);
            using (var sp = new Pen(pal.MetalDark, 1.3f))
                g.DrawLine(sp, cx + r * 0.5f, cy - r * 0.5f, cx + r * 0.95f, cy - r * 0.95f);
        }
        /// <summary>角阀：水平进、拐 90° 上出，转角手轮。</summary>
        private void DrawAngle(Graphics g, RectangleF c, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            float top = cy - ph / 2f;
            // 水平进管 + 垂直上出管
            using (var pbH = HBarBase.CylinderV(new RectangleF(0f, top, c.Width, ph), pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(pbH, c.X, top, cx - c.X + ph / 2f, ph);
            using (var pbV = HBarBase.CylinderV(new RectangleF(cx - ph / 2f, c.Y, ph, cy - c.Y), pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(pbV, cx - ph / 2f, c.Y, ph, cy - c.Y + ph / 2f);
            // 转角阀体
            float r = ph * 0.95f;
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - r, cy - r, r * 2f, r * 2f);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawEllipse(ep, cx - r, cy - r, r * 2f, r * 2f);
            // 两处法兰
            float fw = Math.Max(2.5f, c.Width * 0.028f);
            using (var fb = new SolidBrush(pal.Metal))
            using (var ep = new Pen(pal.Edge, 1.1f))
            {
                g.FillRectangle(fb, c.X, top - ph * 0.28f, fw, ph * 1.56f);
                g.DrawRectangle(ep, c.X, top - ph * 0.28f, fw, ph * 1.56f);
                var fr = new RectangleF(cx - ph * 0.78f, c.Y, ph * 1.56f, fw);
                g.FillRectangle(fb, fr);
                g.DrawRectangle(ep, fr.X, fr.Y, fr.Width, fr.Height);
            }
            DrawWheel(g, cx + r * 1.2f, cy - r * 0.25f, r * 0.6f, pal);
        }
        /// <summary>隔膜阀：驼峰阀体 + 隔膜（开=上拱/关=下压）+ 手轮。</summary>
        private void DrawDiaphragm(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            using (var bp = new GraphicsPath())
            {
                bp.AddLines(new[]
                {
                    new PointF(b.Left, cy + ph * 0.3f),
                    new PointF(b.Left, cy - ph * 0.15f),
                    new PointF(cx - ph * 0.9f, cy - ph * 0.35f),
                    new PointF(cx, b.Top + ph * 0.15f),
                    new PointF(cx + ph * 0.9f, cy - ph * 0.35f),
                    new PointF(b.Right, cy - ph * 0.15f),
                    new PointF(b.Right, cy + ph * 0.3f)
                });
                bp.CloseFigure();
                using (var bb = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.45f)))
                    g.FillPath(bb, bp);
                using (var ep = new Pen(pal.Dark, 1.3f))
                    g.DrawPath(ep, bp);
            }
            // 隔膜弧线
            using (var dp = new Pen(StateColor, Math.Max(2f, ph * 0.16f)))
            {
                float y = _open ? cy - ph * 0.45f : cy + ph * 0.15f;
                g.DrawArc(dp, cx - ph * 0.95f, y - ph * 0.5f, ph * 1.9f, ph * 1.0f, _open ? 200f : 20f, _open ? 140f : 140f);
            }
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.4f, ph * 0.1f)))
                g.DrawLine(sp, cx, b.Top + ph * 0.15f, cx, c.Height * 0.15f);
            DrawWheel(g, cx, c.Height * 0.075f, c.Height * 0.095f, pal);
        }
        /// <summary>电磁阀：线圈方块 + 电缆接头 + 状态指示灯。</summary>
        private void DrawSolenoid(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - ph * 0.7f, cy - ph * 0.7f, ph * 1.4f, ph * 1.4f);
            using (var ep = new Pen(pal.Dark, 1.2f))
                g.DrawEllipse(ep, cx - ph * 0.7f, cy - ph * 0.7f, ph * 1.4f, ph * 1.4f);
            // 线圈立筒
            float coilW = ph * 1.25f, coilBottom = cy - ph * 0.7f, coilTop = c.Height * 0.10f;
            var coil = new RectangleF(cx - coilW / 2f, coilTop, coilW, coilBottom - coilTop);
            using (var cb = HBarBase.CylinderH(coil, pal.Dark, HToolPalettes.Tint(pal.Dark, 0.35f)))
                g.FillRectangle(cb, coil);
            using (var ep = new Pen(pal.Edge, 1.1f))
                g.DrawRectangle(ep, coil.X, coil.Y, coil.Width, coil.Height);
            // 绕线纹
            using (var wp = new Pen(HToolPalettes.Tint(pal.Dark, 0.55f), 1.1f))
                for (int i = 1; i <= 4; i++)
                    g.DrawLine(wp, coil.X + 1.5f, coil.Y + coil.Height * i / 5f, coil.Right - 1.5f, coil.Y + coil.Height * i / 5f);
            // 电缆接头（顶部斜出）
            using (var cp = new Pen(pal.Edge, Math.Max(1.6f, ph * 0.14f)))
                g.DrawLine(cp, cx + coilW * 0.2f, coilTop, cx + coilW * 0.75f, coilTop - ph * 0.3f);
            // 指示灯
            using (var lb = new SolidBrush(StateColor))
                g.FillEllipse(lb, coil.Right - ph * 0.06f, coil.Y + ph * 0.12f, ph * 0.26f, ph * 0.26f);
        }
        /// <summary>气动薄膜阀：薄膜圆罩头 + 气源管。</summary>
        private void DrawPneumatic(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            DrawBowBody(g, b, pal, cy, ph);
            float cx = c.Width * 0.5f;
            float headW = b.Width * 0.78f, headH = c.Height * 0.24f, headY = c.Y + c.Height * 0.02f;
            var head = new RectangleF(cx - headW / 2f, headY, headW, headH);
            using (GraphicsPath hp = HBarBase.RoundPath(head, headH * 0.45f))
            using (var hb = HBarBase.CylinderV(head, pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
            using (var ep = new Pen(pal.Edge, 1.2f))
            {
                g.FillPath(hb, hp);
                g.DrawPath(ep, hp);
            }
            // 隔膜 + 连杆
            using (var dp = new Pen(StateColor, 2f))
                g.DrawLine(dp, head.Left + headW * 0.12f, head.Bottom - headH * 0.28f,
                    head.Right - headW * 0.12f, head.Bottom - headH * 0.28f);
            using (var sp = new Pen(pal.Edge, Math.Max(1.4f, ph * 0.1f)))
                g.DrawLine(sp, cx, head.Bottom - headH * 0.28f, cx, cy - ph * 0.4f);
            // 气源小管弯入
            using (var ap = new Pen(pal.Dark, Math.Max(1.4f, ph * 0.1f)))
            {
                g.DrawLine(ap, head.Left, head.Y + headH * 0.3f, head.Left - ph * 0.5f, head.Y + headH * 0.3f);
                g.DrawLine(ap, head.Left - ph * 0.5f, head.Y + headH * 0.3f, head.Left - ph * 0.5f, head.Y - ph * 0.18f);
            }
        }
        /// <summary>重锤杠杆阀：长杠杆 + 端部重锤。</summary>
        private void DrawLever(Graphics g, RectangleF c, HToolPalette pal, float cy)
        {
            float cx = c.Width * 0.5f;
            float pivotY = c.Height * 0.17f;
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.5f, c.Height * 0.028f)))
                g.DrawLine(sp, cx, cy - c.Height * 0.05f, cx, pivotY);
            double ang = _open ? 0d : -Math.PI / 5d;
            float len = c.Width * 0.26f;
            float ex = cx + (float)Math.Cos(ang) * len, ey = pivotY + (float)Math.Sin(ang) * len;
            using (var lp = new Pen(StateColor, Math.Max(2.2f, c.Height * 0.035f)))
                g.DrawLine(lp, cx, pivotY, ex, ey);
            // 重锤方块
            float w = c.Width * 0.045f, h = c.Height * 0.10f;
            using (var wb = new SolidBrush(pal.Dark))
                g.FillRectangle(wb, ex - w / 2f, ey - h / 2f, w, h);
            using (var ep = new Pen(pal.Edge, 1.1f))
                g.DrawRectangle(ep, ex - w / 2f, ey - h / 2f, w, h);
            g.FillEllipse(Brushes.DimGray, cx - c.Height * 0.022f, pivotY - c.Height * 0.022f,
                c.Height * 0.044f, c.Height * 0.044f);
        }
        /// <summary>旋塞阀：倒梯形锥阀体 + 顶方头 + 短手柄。</summary>
        private void DrawPlug(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            var pts = new[]
            {
                new PointF(cx - ph * 1.15f, cy - ph * 0.9f),
                new PointF(cx + ph * 1.15f, cy - ph * 0.9f),
                new PointF(cx + ph * 0.62f, cy + ph * 0.8f),
                new PointF(cx - ph * 0.62f, cy + ph * 0.8f)
            };
            using (var bb = new SolidBrush(pal.Main))
                g.FillPolygon(bb, pts);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawPolygon(ep, pts);
            // 方头（螺母）
            float sq = ph * 0.55f, sqy = cy - ph * 1.25f;
            using (var sb = HBarBase.CylinderH(new RectangleF(cx - sq / 2f, sqy, sq, sq), pal.MetalDark, Color.WhiteSmoke))
                g.FillRectangle(sb, cx - sq / 2f, sqy, sq, sq);
            using (var ep = new Pen(pal.Edge, 1.1f))
                g.DrawRectangle(ep, cx - sq / 2f, sqy, sq, sq);
            // 手柄（开=横，关=斜）
            double ang = _open ? 0d : Math.PI / 4d;
            float len = ph * 1.25f, hx = cx + (float)Math.Cos(ang) * len, hy = sqy - (float)Math.Sin(ang) * len;
            using (var hp = new Pen(StateColor, Math.Max(2f, ph * 0.15f)))
                g.DrawLine(hp, cx, sqy, hx, hy);
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.3f, ph * 0.09f)))
                g.DrawLine(sp, cx, sqy, cx, cy - ph * 0.9f);
        }
        /// <summary>截止阀：上下鼓包阀体。</summary>
        private void DrawGlobe(Graphics g, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = (b.Left + b.Right) / 2f;
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(cx - b.Width * 0.34f, b.Top, b.Width * 0.68f, b.Height * 0.62f);
                gp.AddEllipse(cx - b.Width * 0.34f, cy - b.Height * 0.02f, b.Width * 0.68f, b.Height * 0.62f);
                using (var bb = new SolidBrush(pal.Main))
                    g.FillPath(bb, gp);
                using (var ep = new Pen(pal.Dark, 1.3f))
                    g.DrawPath(ep, gp);
            }
            using (var hb = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.5f)))
                g.FillEllipse(hb, cx - ph * 0.4f, cy - ph * 0.4f, ph * 0.8f, ph * 0.8f);
        }
        /// <summary>疏水阀：竖立桶体 + 顶盖 + 底部排放小阀。</summary>
        private void DrawSteamTrap(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            float bw = b.Width * 0.62f;
            var body = new RectangleF(cx - bw / 2f, b.Top - ph * 0.15f, bw, b.Height * 1.05f);
            using (GraphicsPath bp = HBarBase.RoundPath(body, bw * 0.22f))
            using (var bb = HBarBase.CylinderH(body, pal.Main, HToolPalettes.Tint(pal.Main, 0.6f)))
            using (var ep = new Pen(pal.Dark, 1.3f))
            {
                g.FillPath(bb, bp);
                g.DrawPath(ep, bp);
            }
            // 顶盖 + 螺栓
            var cap = new RectangleF(cx - bw * 0.32f, body.Y - ph * 0.22f, bw * 0.64f, ph * 0.3f);
            using (var cb = new SolidBrush(pal.MetalDark))
                g.FillRectangle(cb, cap);
            float br = ph * 0.08f;
            using (var bb2 = new SolidBrush(pal.Edge))
                foreach (float bx in new[] { cap.Left + bw * 0.12f, cap.Right - bw * 0.12f })
                    g.FillEllipse(bb2, bx - br, cap.Top - br, br * 2f, br * 2f);
            // 底部排放小阀
            using (var dp = new Pen(pal.MetalDark, Math.Max(1.3f, ph * 0.1f)))
                g.DrawLine(dp, cx, body.Bottom, cx, body.Bottom + ph * 0.28f);
            g.FillEllipse(Brushes.DimGray, cx - ph * 0.14f, body.Bottom + ph * 0.24f, ph * 0.28f, ph * 0.28f);
        }
        /// <summary>减压阀：膜片圆罩 + 调节螺钉 + 侧装压力表。</summary>
        private void DrawRegulator(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - ph * 0.7f, cy - ph * 0.7f, ph * 1.4f, ph * 1.4f);
            using (var ep = new Pen(pal.Dark, 1.2f))
                g.DrawEllipse(ep, cx - ph * 0.7f, cy - ph * 0.7f, ph * 1.4f, ph * 1.4f);
            // 膜罩
            float dw = b.Width * 0.72f, dh = c.Height * 0.20f, dy = c.Y + c.Height * 0.06f;
            var dome = new RectangleF(cx - dw / 2f, dy, dw, dh);
            using (GraphicsPath dp = HBarBase.RoundPath(dome, dh * 0.5f))
            using (var db = HBarBase.CylinderV(dome, pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
            using (var ep2 = new Pen(pal.Edge, 1.1f))
            {
                g.FillPath(db, dp);
                g.DrawPath(ep2, dp);
            }
            // 调节螺钉
            using (var sp = new Pen(pal.Edge, Math.Max(1.3f, ph * 0.1f)))
            {
                g.DrawLine(sp, cx, dy, cx, dy - ph * 0.22f);
                g.DrawLine(sp, cx - ph * 0.18f, dy - ph * 0.22f, cx + ph * 0.18f, dy - ph * 0.22f);
            }
            // 压力表
            float gr = ph * 0.52f, gx = cx + dw * 0.36f, gy = dy + dh * 0.5f;
            using (var gb = new SolidBrush(Color.WhiteSmoke))
                g.FillEllipse(gb, gx - gr, gy - gr, gr * 2f, gr * 2f);
            using (var gp = new Pen(pal.Edge, 1.1f))
                g.DrawEllipse(gp, gx - gr, gy - gr, gr * 2f, gr * 2f);
            double pa = _open ? -Math.PI / 3d : -Math.PI * 2d / 3d;
            using (var np = new Pen(StateColor, 1.3f))
                g.DrawLine(np, gx, gy, gx + (float)Math.Cos(pa) * gr * 0.7f, gy + (float)Math.Sin(pa) * gr * 0.7f);
        }
        /// <summary>夹管阀：阀体内夹胶管 + 压杆手轮。</summary>
        private void DrawPinch(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            using (var frame = new GraphicsPath())
            {
                frame.AddRectangle(new RectangleF(b.Left, b.Top, b.Width, b.Height));
                using (var fb = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.65f)))
                    g.FillPath(fb, frame);
                using (var ep = new Pen(pal.Dark, 1.3f))
                    g.DrawPath(ep, frame);
            }
            // 胶管（上下弧合成扁管）
            float pinch = _open ? 0f : ph * 0.32f;
            using (var hp = new Pen(pal.Accent, Math.Max(2f, ph * 0.16f)))
            {
                g.DrawArc(hp, b.Left, cy - ph * 0.9f + pinch, b.Width, ph * 1.0f, 20f, 140f);
                g.DrawArc(hp, b.Left, cy - ph * 0.1f - pinch, b.Width, ph * 1.0f, 200f, 140f);
            }
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.4f, ph * 0.1f)))
                g.DrawLine(sp, cx, b.Top, cx, c.Height * 0.15f);
            DrawWheel(g, cx, c.Height * 0.075f, c.Height * 0.095f, pal);
        }
        /// <summary>四通阀：十字四路 + 中央旋塞 + 手柄。</summary>
        private void DrawMultiPort(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            float cx = c.Width * 0.5f;
            // 竖直上下管
            using (var pb = HBarBase.CylinderV(new RectangleF(cx - ph / 2f, c.Y, ph, c.Height), pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(pb, cx - ph / 2f, c.Y, ph, c.Height);
            float fw = Math.Max(2.5f, c.Width * 0.028f);
            using (var fb = new SolidBrush(pal.Metal))
            using (var ep = new Pen(pal.Edge, 1.1f))
                foreach (var fr in new[]
                {
                    new RectangleF(cx - ph * 0.78f, c.Y, ph * 1.56f, fw),
                    new RectangleF(cx - ph * 0.78f, c.Bottom - fw, ph * 1.56f, fw)
                })
                {
                    g.FillRectangle(fb, fr);
                    g.DrawRectangle(ep, fr.X, fr.Y, fr.Width, fr.Height);
                }
            float r = ph * 1.05f;
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, cx - r, cy - r, r * 2f, r * 2f);
            using (var ep = new Pen(pal.Dark, 1.3f))
                g.DrawEllipse(ep, cx - r, cy - r, r * 2f, r * 2f);
            using (var hb = new SolidBrush(StateColor))
            {
                var pts = _open
                    ? new[] { new PointF(cx, cy - r * 0.55f), new PointF(cx + r * 0.55f, cy), new PointF(cx, cy + r * 0.55f), new PointF(cx - r * 0.55f, cy) }
                    : new[] { new PointF(cx, cy - r * 0.55f), new PointF(cx + r * 0.38f, cy - r * 0.38f), new PointF(cx + r * 0.55f, cy), new PointF(cx + r * 0.38f, cy + r * 0.38f),
                             new PointF(cx, cy + r * 0.55f), new PointF(cx - r * 0.38f, cy + r * 0.38f), new PointF(cx - r * 0.55f, cy), new PointF(cx - r * 0.38f, cy - r * 0.38f) };
                g.FillPolygon(hb, pts);
            }
        }
        /// <summary>8 字盲板：两片法兰中间，开=环板/关=实心盲板。</summary>
        private void DrawBlind(Graphics g, RectangleF c, HToolPalette pal, float cy, float ph)
        {
            float top = cy - ph / 2f;
            float midL = c.Width * 0.40f, midR = c.Width * 0.60f;
            using (var pb = HBarBase.CylinderV(new RectangleF(0f, top, c.Width, ph), pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
            {
                g.FillRectangle(pb, c.X, top, midL - c.X + 1f, ph);
                g.FillRectangle(pb, midR, top, c.Right - midR + 1f, ph);
            }
            float fw = Math.Max(3f, c.Width * 0.04f);
            float gapCY = cy;
            float rr = ph * 0.92f;
            // 两法兰盘（竖向厚盘）
            using (var fb = new SolidBrush(pal.Metal))
            using (var ep = new Pen(pal.Edge, 1.2f))
                foreach (float fx in new[] { midL, midR })
                {
                    g.FillRectangle(fb, fx - fw / 2f, top - ph * 0.4f, fw, ph * 1.8f);
                    g.DrawRectangle(ep, fx - fw / 2f, top - ph * 0.4f, fw, ph * 1.8f);
                }
            float bcx = (midL + midR) / 2f;
            if (_open)
            {
                // 环板（中空）
                using (var rp = new Pen(pal.Metal, rr * 0.32f))
                    g.DrawEllipse(rp, bcx - rr, gapCY - rr, rr * 2f, rr * 2f);
                using (var ep = new Pen(pal.Edge, 1.1f))
                    g.DrawEllipse(ep, bcx - rr, gapCY - rr, rr * 2f, rr * 2f);
            }
            else
            {
                using (var bb = new SolidBrush(StateColor))
                    g.FillEllipse(bb, bcx - rr, gapCY - rr, rr * 2f, rr * 2f);
                using (var ep = new Pen(pal.Dark, 1.2f))
                    g.DrawEllipse(ep, bcx - rr, gapCY - rr, rr * 2f, rr * 2f);
                // 实心板螺栓点
                float dot = rr * 0.10f;
                using (var db = new SolidBrush(pal.Edge))
                    for (int i = 0; i < 6; i++)
                    {
                        double a = i * Math.PI / 3d;
                        g.FillEllipse(db, bcx + (float)Math.Cos(a) * rr * 0.62f - dot,
                            gapCY + (float)Math.Sin(a) * rr * 0.62f - dot, dot * 2f, dot * 2f);
                    }
            }
        }
        /// <summary>计量阀：小阀体 + 刻度手轮 + 流量指示条。</summary>
        private void DrawMetering(Graphics g, RectangleF c, RectangleF b, HToolPalette pal, float cy, float ph)
        {
            using (var bb = new SolidBrush(pal.Main))
                g.FillEllipse(bb, b.Left + b.Width * 0.18f, cy - ph * 0.65f, ph * 1.3f, ph * 1.3f);
            using (var ep = new Pen(pal.Dark, 1.2f))
                g.DrawEllipse(ep, b.Left + b.Width * 0.18f, cy - ph * 0.65f, ph * 1.3f, ph * 1.3f);
            float wx = b.Right - b.Width * 0.12f, wy = c.Height * 0.10f, wr = c.Height * 0.085f;
            using (var sp = new Pen(pal.MetalDark, Math.Max(1.3f, ph * 0.09f)))
                g.DrawLine(sp, b.Left + b.Width * 0.34f, cy - ph * 0.5f, wx, wy + wr);
            using (var wp = new Pen(pal.MetalDark, Math.Max(1.4f, wr * 0.2f)))
                g.DrawEllipse(wp, wx - wr, wy - wr, wr * 2f, wr * 2f);
            using (var tp = new Pen(pal.Edge, 1.1f))
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4d;
                    g.DrawLine(tp, wx + (float)Math.Cos(a) * wr * 0.75f, wy + (float)Math.Sin(a) * wr * 0.75f,
                        wx + (float)Math.Cos(a) * wr * 1.02f, wy + (float)Math.Sin(a) * wr * 1.02f);
                }
            // 流量指示条（右管上方）
            float ix = c.Width * 0.78f, iw = c.Width * 0.16f, ih = ph * 0.28f;
            using (var eb = new SolidBrush(HToolPalettes.Tint(pal.MetalDark, 0.55f)))
                g.FillRectangle(eb, ix, cy - ph * 1.05f, iw, ih);
            using (var fb = new SolidBrush(StateColor))
                g.FillRectangle(fb, ix, cy - ph * 1.05f, iw * (_open ? 0.8f : 0.25f), ih);
        }
    }
}