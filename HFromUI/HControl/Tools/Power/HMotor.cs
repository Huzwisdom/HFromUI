using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Power
{
    using HFromUI.HControl.Base;
    /// <summary>电机样式：Classic 与旧版 HFrom.HMotor 完全一致，其余 21 种为新机型。</summary>
    public enum HMotorStyle
    {
        Classic = 0,          // 经典侧视电机（旧版原样）
        Servo = 1,            // 伺服电机
        Stepper = 2,          // 步进电机
        Gearmotor = 3,        // 减速一体电机
        VerticalMotor = 4,    // 立式法兰电机
        FlangeMount = 5,      // B5 法兰盘电机
        ExplosionProof = 6,   // 防爆电机
        FanCooled = 7,        // 风冷电机（后风罩）
        PulleyDrive = 8,      // 皮带轮电机
        Compact = 9,          // 紧凑型电机
        DrumMotor = 10,       // 滚筒电机
        LinearMotor = 11,     // 直线电机
        EncoderMotor = 12,    // 带编码器电机
        BrakeMotor = 13,      // 带刹车电机
        Waterproof = 14,      // 防水电机
        DripProof = 15,       // 自冷散热筋电机
        DCMotor = 16,         // 直流有刷电机
        HollowShaft = 17,     // 空心轴电机
        RightAngle = 18,      // 直角减速电机
        PumpUnit = 19,        // 电机泵组
        FanUnit = 20,         // 电机风机组
        NEMA = 21             // NEMA 标准脚座电机
    }
    /// <summary>色调：Classic 为旧版深灰机身配色，1..12 取现代色调表。</summary>
    public enum HMotorTheme
    {
        Classic = 0, 天蓝 = 1, 海蓝 = 2, 翠绿 = 3, 墨青 = 4, 琥珀 = 5, 橙 = 6,
        玫瑰 = 7, 红 = 8, 紫 = 9, 品红 = 10, 咖啡 = 11, 石墨 = 12
    }
    /// <summary>
    /// 电机控件（继承 HLabelBase）：22 种机型、13 种色调、四个安装/轴方向，
    /// Classic 样式 + Classic 色调与旧版 HFrom.HMotor 的形状和颜色完全一致。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("电机控件：22 种机型、13 种色调，支持四方向与颜色自定义")]
    public class HMotor : HLabelBase
    {
        private HMotorStyle _style = HMotorStyle.Classic;
        private HMotorTheme _theme = HMotorTheme.Classic;
        private Color _edgeColor = Color.FromArgb(89, 91, 96);
        private Color _centerColor = Color.FromArgb(200, 200, 200);
        private Color _borderColor = Color.Gray;
        private ArrowDirection _direction = ArrowDirection.Right;
        private StringFormat _sf;
        private bool _running;                       // 是否处于运行状态
        private readonly Timer _runTimer;            // 运行动画定时器（非阻塞）
        private float _spinAngle;                    // 旋转部件当前角度（度）
        private float _linearPhase;                  // 直线电机动子往返相位（弧度）
        public HMotor()
        {
            AutoSize = false;
            BackColor = Color.Transparent;
            Size = new Size(275, 134);
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor
                | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            _sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            _runTimer = new Timer { Interval = 50 };
            _runTimer.Tick += RunTimer_Tick;
        }
        private void RunTimer_Tick(object sender, EventArgs e)
        {
            _spinAngle = (_spinAngle + 12f) % 360f;
            if (_style == HMotorStyle.LinearMotor)
            {
                _linearPhase += 0.18f;
                if (_linearPhase > (float)Math.PI * 2f) _linearPhase -= (float)Math.PI * 2f;
            }
            Invalidate();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _runTimer.Stop();
                _runTimer.Dispose();
                _sf?.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>电机样式。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机外形样式"), HDescriptionLanguage("电机外形样式，Classic 与旧版完全一致"), Browsable(true)]
        [DefaultValue(HMotorStyle.Classic)]
        public HMotorStyle MotorStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>色调。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机色调"), HDescriptionLanguage("电机色调，Classic 为旧版深灰机身"), Browsable(true)]
        [DefaultValue(HMotorTheme.Classic)]
        public HMotorTheme MotorTheme
        {
            get => _theme;
            set { _theme = value; Invalidate(); }
        }
        /// <summary>电机主题色（Classic 端盖/机身渐变边缘色）。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("边缘颜色"), HDescriptionLanguage("电机主题色（Classic 渐变边缘色）"), Browsable(true)]
        [DefaultValue(typeof(Color), "[89, 91, 96]")]
        public Color EdgeColor
        {
            get => _edgeColor;
            set { _edgeColor = value; Invalidate(); }
        }
        /// <summary>中间淡色（Classic 渐变色）。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机渐变中间淡色"), HDescriptionLanguage("电机渐变中间淡色"), Browsable(true)]
        [DefaultValue(typeof(Color), "[200, 200, 200]")]
        public Color CenterColor
        {
            get => _centerColor;
            set { _centerColor = value; Invalidate(); }
        }
        /// <summary>边框颜色。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机边框颜色"), HDescriptionLanguage("电机边框颜色"), Browsable(true)]
        [DefaultValue(typeof(Color), "Gray")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; Invalidate(); }
        }
        /// <summary>轴方向：默认向右，支持左/上/下变换（与旧版一致）。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机轴方向"), HDescriptionLanguage("电机轴方向"), Browsable(true)]
        [DefaultValue(ArrowDirection.Right)]
        public ArrowDirection Direction
        {
            get => _direction;
            set { _direction = value; Invalidate(); }
        }
        /// <summary>运行状态：true 时轴/叶轮/风罩等旋转部件转动（直线电机动子往返），false 时静止。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机运行状态"), HDescriptionLanguage("电机运行状态，运行时旋转部件产生动画"), Browsable(true)]
        [DefaultValue(false)]
        public bool Running
        {
            get => _running;
            set
            {
                if (_running == value) return;
                _running = value;
                if (value) _runTimer.Start();
                else _runTimer.Stop();
                Invalidate();
            }
        }
        /// <summary>电机文本（绘制在接线盒区域）。</summary>
        [HCategoryLanguage("电机"), HDisplayNameLanguage("电机文本"), HDescriptionLanguage("绘制在电机接线盒区域的文本"), Browsable(true)]
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
            if (Width < 8 || Height < 8) return;
            RectangleF textRect;
            if (_direction == ArrowDirection.Right)
            {
                textRect = new RectangleF(Width * 0.21f, 0f, Width * 0.24f, Height * 0.22f);
                PaintBody(g, Width, Height);
            }
            else if (_direction == ArrowDirection.Left)
            {
                textRect = new RectangleF(Width * 0.55f, 0f, Width * 0.24f, Height * 0.22f);
                using (var m = new Matrix())
                {
                    m.Scale(-1f, 1f);
                    g.MultiplyTransform(m);
                    g.TranslateTransform(-Width, 0f);
                    PaintBody(g, Width, Height);
                }
                g.ResetTransform();
            }
            else if (_direction == ArrowDirection.Up)
            {
                textRect = new RectangleF(0f, Height * 0.55f, Width * 0.22f, Height * 0.24f);
                g.RotateTransform(-90f);
                g.TranslateTransform(-Height, 0f);
                PaintBody(g, Height, Width);
                g.ResetTransform();
            }
            else
            {
                textRect = new RectangleF(Width * 0.78f, Height * 0.21f, Width * 0.22f, Height * 0.24f);
                g.TranslateTransform(Width, 0f);
                g.RotateTransform(90f);
                PaintBody(g, Height, Width);
                g.ResetTransform();
            }
            using (var b = new SolidBrush(ForeColor))
                g.DrawString(Text, Font, b, textRect, _sf);
        }
        private void PaintBody(Graphics g, float width, float height)
        {
            if (_style == HMotorStyle.Classic) PaintClassic(g, width, height);
            else PaintModern(g, new RectangleF(0f, 0f, width, height));
        }
        // ------------------------------------------------------------------
        // Classic：旧版 HFrom.HMotor 原样移植（机身/端盖/散热筋/接线盒/法兰/轴）
        // ------------------------------------------------------------------
        private void PaintClassic(Graphics g, float width, float height)
        {
            // Classic 色调沿用旧版深灰机身（EdgeColor/CenterColor/BorderColor 自定义生效）；
            // 其他色调把同一几何映射到现代调色板
            bool themed = _theme != HMotorTheme.Classic;
            HToolPalette pal = themed ? HToolPalettes.Get((int)_theme) : default(HToolPalette);
            Color edge = themed ? pal.Dark : _edgeColor;
            Color center = themed ? HToolPalettes.Tint(pal.Main, 0.78f) : _centerColor;
            Color border = themed ? pal.Edge : _borderColor;
            Color accent = themed ? pal.Accent : Color.DodgerBlue;
            var cb = new ColorBlend
            {
                Positions = new[] { 0f, 0.25f, 1f },
                Colors = new[] { edge, center, edge }
            };
            using (var b1 = new LinearGradientBrush(new PointF(0f, height * 0.28f), new PointF(0f, height * 0.92f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
            {
                b1.InterpolationColors = cb;
                g.FillRectangle(b1, new RectangleF(width * 0.2f, height * 0.28f, width * 0.41f, height * 0.64f));
            }
            using (var b2 = new LinearGradientBrush(new PointF(0f, height * 0.25f), new PointF(0f, height * 0.95f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
            {
                b2.InterpolationColors = cb;
                g.FillEllipse(b2, new RectangleF(0f, height * 0.25f, width * 0.2f, height * 0.7f));
                g.FillRectangle(b2, new RectangleF(width * 0.1f, height * 0.25f, width * 0.11f, height * 0.7f));
                g.FillRectangle(b2, new RectangleF(width * 0.6f, height * 0.25f, width * 0.25f, height * 0.7f));
                using (var pen = new Pen(border, 1f))
                {
                    g.DrawLine(pen, width * 0.21f, height * 0.25f, width * 0.21f, height * 0.95f);
                    g.DrawLine(pen, width * 0.6f, height * 0.25f, width * 0.6f, height * 0.95f);
                }
            }
            using (var b3 = new LinearGradientBrush(new PointF(0f, height * 0.52f), new PointF(0f, height * 0.68f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
            {
                b3.InterpolationColors = cb;
                g.FillRectangle(b3, new RectangleF(width * 0.9f, height * 0.52f, width * 0.1f + 1f, height * 0.16f));
            }
            using (var b4 = new LinearGradientBrush(new PointF(0f, height * 0.46f), new PointF(0f, height * 0.74f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
            {
                b4.InterpolationColors = cb;
                PointF[] pts =
                {
                    new PointF(width * 0.84f, height * 0.46f),
                    new PointF(width * 0.9f, height * 0.52f),
                    new PointF(width * 0.9f, height * 0.68f),
                    new PointF(width * 0.84f, height * 0.74f),
                    new PointF(width * 0.84f, height * 0.46f)
                };
                g.FillPolygon(b4, pts);
            }
            using (var b5 = new LinearGradientBrush(new PointF(0f, height * 0.2f), new PointF(0f, height * 1f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
            {
                b5.InterpolationColors = cb;
                g.FillRectangle(b5, new RectangleF(width * 0.82f, height * 0.2f, width * 0.04f, height * 0.8f));
                using (var pen = new Pen(border, 1f))
                    g.DrawRectangle(pen, width * 0.82f, height * 0.2f, width * 0.04f, height * 0.8f);
            }
            // 12 条散热筋
            for (int i = 0; i < 12; i++)
            {
                float num = height * 0.64f / 12f;
                float num2 = height * 0.02f;
                if (num2 < 3f) num2 = 3f;
                using (var b6 = new LinearGradientBrush(
                    new PointF(0f, height * 0.3f + num * i), new PointF(0f, height * 0.3f + num * i + num2),
                    Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
                {
                    b6.InterpolationColors = cb;
                    g.FillRectangle(b6, new RectangleF(width * 0.21f, height * 0.3f + num * i, width * 0.39f, num2));
                }
            }
            // 接线盒（顶面梯形盖 + 盒体）
            Color light = HToolPalettes.Tint(edge, 0.4f);
            using (var br = new SolidBrush(light))
            {
                PointF[] pts2 =
                {
                    new PointF(width * 0.21f, height * 0.22f),
                    new PointF(width * 0.23f, height * 0.28f),
                    new PointF(width * 0.43f, height * 0.28f),
                    new PointF(width * 0.45f, height * 0.22f),
                    new PointF(width * 0.21f, height * 0.22f)
                };
                g.FillPolygon(br, pts2);
                var cb2 = new ColorBlend
                {
                    Positions = new[] { 0f, 0.1f, 0.9f, 0.95f, 1f },
                    Colors = new[] { edge, light, light, center, edge }
                };
                using (var b7 = new LinearGradientBrush(new PointF(width * 0.21f, 0f), new PointF(width * 0.45f, 0f),
                    Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
                {
                    b7.InterpolationColors = cb2;
                    g.FillRectangle(b7, new RectangleF(width * 0.21f, 0f, width * 0.24f, height * 0.22f));
                }
                using (var pen = new Pen(border, 1f))
                {
                    g.DrawRectangle(pen, width * 0.21f, 0f, width * 0.24f, height * 0.22f);
                    g.DrawPolygon(pen, pts2);
                }
            }
            // 法兰盘
            using (var b8 = new LinearGradientBrush(new PointF(width * 0.65f, 0f), new PointF(width * 0.75f, 0f),
                Color.FromArgb(247, 252, 253), Color.WhiteSmoke))
            {
                b8.InterpolationColors = cb;
                g.FillEllipse(b8, new RectangleF(width * 0.65f, height * 0.6f, width * 0.1f, width * 0.1f));
            }
            // 轴端旋转标记（运行：强调色弧转动；停止：静止键槽线）
            DrawSpinMark(g, width * 0.95f, height * 0.60f, height * 0.07f, accent);
        }
        // ------------------------------------------------------------------
        // 现代样式（横向画布 c：轴朝右）
        // ------------------------------------------------------------------
        private HToolPalette Palette => _theme == HMotorTheme.Classic
            ? new HToolPalette
            {
                Main = Color.FromArgb(89, 91, 96), Light = Color.FromArgb(150, 152, 156),
                Lighter = Color.FromArgb(210, 212, 215), Dark = Color.FromArgb(56, 58, 62),
                Edge = Color.Gray, Glass = Color.FromArgb(247, 252, 253), GlassEdge = Color.FromArgb(180, 184, 190),
                Metal = Color.FromArgb(228, 230, 233), MetalDark = Color.FromArgb(148, 152, 158), Accent = Color.DodgerBlue
            }
            : HToolPalettes.Get((int)_theme);
        /// <summary>实心矩形。</summary>
        private static void Rect(Graphics g, float x, float y, float w, float h, Color color)
        {
            using (var b = new SolidBrush(color))
                g.FillRectangle(b, x, y, w, h);
        }
        /// <summary>描边矩形。</summary>
        private static void RectE(Graphics g, float x, float y, float w, float h, Color color, float lw = 1.2f)
        {
            using (var p = new Pen(color, lw))
                g.DrawRectangle(p, x, y, w, h);
        }
        /// <summary>圆筒机身（竖向高光渐变 + 左端椭圆盖），右端返回坐标。</summary>
        private static void CylBody(Graphics g, RectangleF r, HToolPalette pal)
        {
            using (var b = HBarBase.CylinderV(r, pal.Dark, HToolPalettes.Tint(pal.Main, 0.55f), pal.Dark))
                g.FillRectangle(b, r);
            using (var eb = HBarBase.CylinderH(new RectangleF(r.X - r.Height * 0.10f, r.Y, r.Height * 0.20f, r.Height),
                pal.Dark, HToolPalettes.Tint(pal.Main, 0.45f)))
                g.FillEllipse(eb, r.X - r.Height * 0.10f, r.Y, r.Height * 0.20f, r.Height);
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.35f), 1.2f))
                g.DrawEllipse(p, r.X - r.Height * 0.10f, r.Y, r.Height * 0.20f, r.Height);
        }
        /// <summary>两只安装脚座。</summary>
        private static void Feet(Graphics g, RectangleF r, HToolPalette pal, float gy)
        {
            float fw = r.Width * 0.22f, fh = gy - (r.Bottom - r.Height * 0.02f);
            if (fh <= 0f) return;
            using (var b = new SolidBrush(pal.MetalDark))
            using (var p = new Pen(pal.Edge, 1f))
                foreach (float fx in new[] { r.X + r.Width * 0.12f, r.Right - r.Width * 0.12f - fw })
                {
                    var fr = new RectangleF(fx, r.Bottom - r.Height * 0.02f, fw, fh);
                    g.FillRectangle(b, fr);
                    g.DrawRectangle(p, fr.X, fr.Y, fr.Width, fr.Height);
                    g.FillEllipse(Brushes.DimGray, fx + fw / 2f - fh * 0.16f, fr.Bottom - fh * 0.42f, fh * 0.32f, fh * 0.32f);
                }
        }
        /// <summary>右法兰 + 轴（默认在轴端绘制旋转标记）。</summary>
        private void Shaft(Graphics g, float flangeX, RectangleF r, HToolPalette pal, float shaftEnd = 0.97f, float canvasW = 0f, bool spinMark = true)
        {
            float w = canvasW <= 0f ? r.Right + r.Height * 0.4f : canvasW;
            // 法兰盘
            float fd = r.Height * 1.12f;
            using (var b = HBarBase.CylinderH(new RectangleF(flangeX - fd * 0.06f, r.Y - r.Height * 0.06f, fd * 0.22f, fd),
                pal.MetalDark, Color.WhiteSmoke))
                g.FillEllipse(b, flangeX - fd * 0.02f, r.Y - r.Height * 0.06f, fd * 0.14f, fd);
            // 轴
            float sh = r.Height * 0.20f;
            var sr = new RectangleF(flangeX + fd * 0.08f, r.Y + r.Height * 0.40f, w - (flangeX + fd * 0.08f), sh);
            using (var b = HBarBase.CylinderV(sr, pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(b, sr);
            using (var p = new Pen(pal.Edge, 1f))
                g.DrawRectangle(p, sr.X, sr.Y, sr.Width, sr.Height);
            // 轴端旋转标记
            if (spinMark)
                DrawSpinMark(g, w - sh * 0.85f, sr.Y + sh / 2f, sh * 0.85f, pal.Accent);
        }
        /// <summary>
        /// 旋转部件标记：运行时画转动的强调色圆弧；停止时只留静止的深色键槽线。
        /// 绘制发生在各方向矩阵变换之内，朝向随 Direction 自动正确。
        /// </summary>
        private void DrawSpinMark(Graphics g, float cx, float cy, float rr, Color accent)
        {
            double a = _spinAngle * Math.PI / 180d;
            if (_running)
                using (var p = new Pen(accent, Math.Max(1.8f, rr * 0.24f))
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                    g.DrawArc(p, cx - rr, cy - rr, rr * 2f, rr * 2f, _spinAngle, 110f);
            using (var p = new Pen(Color.FromArgb(86, 88, 94), Math.Max(1f, rr * 0.13f)))
                g.DrawLine(p, cx, cy,
                    cx + (float)Math.Cos(a) * rr * 0.78f, cy + (float)Math.Sin(a) * rr * 0.78f);
        }
        /// <summary>在带轮/圆盘上绘制辐条（运行时按 speed 系数转动，停止时静止）。</summary>
        private void DrawSpokes(Graphics g, float cx, float cy, float rr, int n, Color color, float speed = 1f)
        {
            double baseA = _spinAngle * Math.PI / 180d * speed;
            using (var p = new Pen(color, Math.Max(1.4f, rr * 0.10f)))
                for (int i = 0; i < n; i++)
                {
                    double a = baseA + i * Math.PI * 2d / n;
                    g.DrawLine(p, cx, cy,
                        cx + (float)Math.Cos(a) * rr * 0.82f, cy + (float)Math.Sin(a) * rr * 0.82f);
                }
        }
        private void PaintModern(Graphics g, RectangleF c)
        {
            var pal = Palette;
            switch (_style)
            {
                case HMotorStyle.Servo: DrawServo(g, c, pal); break;
                case HMotorStyle.Stepper: DrawStepper(g, c, pal); break;
                case HMotorStyle.Gearmotor: DrawGearmotor(g, c, pal); break;
                case HMotorStyle.VerticalMotor: DrawVertical(g, c, pal); break;
                case HMotorStyle.FlangeMount: DrawFlangeB5(g, c, pal); break;
                case HMotorStyle.ExplosionProof: DrawExplosion(g, c, pal); break;
                case HMotorStyle.FanCooled: DrawFanCooled(g, c, pal); break;
                case HMotorStyle.PulleyDrive: DrawPulley(g, c, pal); break;
                case HMotorStyle.Compact: DrawCompact(g, c, pal); break;
                case HMotorStyle.DrumMotor: DrawDrum(g, c, pal); break;
                case HMotorStyle.LinearMotor: DrawLinear(g, c, pal); break;
                case HMotorStyle.EncoderMotor: DrawEncoder(g, c, pal); break;
                case HMotorStyle.BrakeMotor: DrawBrake(g, c, pal); break;
                case HMotorStyle.Waterproof: DrawWaterproof(g, c, pal); break;
                case HMotorStyle.DripProof: DrawDripProof(g, c, pal); break;
                case HMotorStyle.DCMotor: DrawDC(g, c, pal); break;
                case HMotorStyle.HollowShaft: DrawHollow(g, c, pal); break;
                case HMotorStyle.RightAngle: DrawRightAngle(g, c, pal); break;
                case HMotorStyle.PumpUnit: DrawPump(g, c, pal); break;
                case HMotorStyle.FanUnit: DrawFanUnit(g, c, pal); break;
                case HMotorStyle.NEMA: DrawNEMA(g, c, pal); break;
            }
        }
        /// <summary>伺服：光滑短机身 + 后编码器 + 顶斜插电缆。</summary>
        private void DrawServo(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.22f, c.Height * 0.36f, c.Width * 0.38f, c.Height * 0.44f);
            // 后编码器
            var enc = new RectangleF(c.Width * 0.11f, c.Height * 0.43f, c.Width * 0.10f, c.Height * 0.30f);
            using (var b = HBarBase.CylinderH(enc, pal.MetalDark, Color.WhiteSmoke))
                g.FillRectangle(b, enc);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.97f, c.Width * 0.97f);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 编码器插接头 + 电缆弧
            using (var p = new Pen(pal.Edge, Math.Max(1.6f, c.Height * 0.02f)))
                g.DrawArc(p, c.Width * 0.10f, c.Height * 0.20f, c.Width * 0.16f, c.Height * 0.26f, 210f, 110f);
            Rect(g, c.Width * 0.15f, c.Height * 0.39f, c.Width * 0.035f, c.Height * 0.05f, pal.Dark);
        }
        /// <summary>步进：方机身 + 环形散热片。</summary>
        private void DrawStepper(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.18f, c.Height * 0.30f, c.Width * 0.46f, c.Height * 0.56f);
            using (var b = new SolidBrush(pal.Main))
                g.FillRectangle(b, r);
            RectE(g, r.X, r.Y, r.Width, r.Height, pal.Dark);
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), c.Width * 0.012f))
                for (int i = 1; i <= 5; i++)
                    g.DrawLine(p, r.X + r.Width * i / 6f, r.Y, r.X + r.Width * i / 6f, r.Bottom);
            // 后端平盖
            Rect(g, r.X - c.Width * 0.04f, r.Y + c.Height * 0.02f, c.Width * 0.04f, r.Height * 0.96f - c.Height * 0.04f, pal.Dark);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.95f);
        }
        /// <summary>减速电机：电机 + 前置齿轮箱。</summary>
        private void DrawGearmotor(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.10f, c.Height * 0.38f, c.Width * 0.34f, c.Height * 0.42f);
            CylBody(g, r, pal);
            var box = new RectangleF(c.Width * 0.44f, c.Height * 0.26f, c.Width * 0.28f, c.Height * 0.64f);
            using (var b = HBarBase.CylinderH(box, HToolPalettes.Shade(pal.Main, 0.12f), HToolPalettes.Tint(pal.Main, 0.5f)))
                g.FillRectangle(b, box);
            RectE(g, box.X, box.Y, box.Width, box.Height, pal.Dark, 1.3f);
            // 分模线 + 输出轴
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.35f), 1.1f))
                g.DrawLine(p, box.X, box.Y + box.Height * 0.5f, box.Right, box.Y + box.Height * 0.5f);
            var sr = new RectangleF(box.Right, box.Y + box.Height * 0.42f, c.Width * 0.16f, box.Height * 0.16f);
            using (var b = HBarBase.CylinderV(sr, pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(b, sr);
            DrawSpinMark(g, box.Right + c.Width * 0.16f - box.Height * 0.08f, box.Y + box.Height * 0.5f,
                box.Height * 0.08f, pal.Accent);
            Feet(g, box, pal, c.Bottom - c.Height * 0.05f);
        }
        /// <summary>立式电机：底座在下，顶法兰 + 上轴。</summary>
        private void DrawVertical(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.36f, c.Height * 0.26f, c.Width * 0.28f, c.Height * 0.58f);
            using (var b = HBarBase.CylinderH(r, pal.Dark, HToolPalettes.Tint(pal.Main, 0.55f)))
                g.FillRectangle(b, r);
            RectE(g, r.X, r.Y, r.Width, r.Height, pal.Dark);
            // 散热筋（竖向）
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.3f), 1.2f))
                for (int i = 1; i <= 5; i++)
                    g.DrawLine(p, r.X + r.Width * i / 6f, r.Y, r.X + r.Width * i / 6f, r.Bottom);
            // 顶法兰
            float fy = r.Y - c.Height * 0.08f, fh = c.Height * 0.09f;
            using (var b = HBarBase.CylinderV(new RectangleF(r.X - r.Width * 0.14f, fy, r.Width * 1.28f, fh), pal.MetalDark, Color.WhiteSmoke, pal.MetalDark))
                g.FillRectangle(b, r.X - r.Width * 0.14f, fy, r.Width * 1.28f, fh);
            DrawBoltCircle(g, r.X + r.Width / 2f, fy + fh / 2f, r.Width * 0.5f, 6, pal);
            // 上轴
            using (var b = new SolidBrush(pal.Metal))
                g.FillRectangle(b, r.X + r.Width * 0.42f, c.Height * 0.04f, r.Width * 0.16f, c.Height * 0.14f);
            DrawSpinMark(g, r.X + r.Width * 0.5f, c.Height * 0.055f, c.Height * 0.045f, pal.Accent);
            // 底座
            Rect(g, r.X - r.Width * 0.22f, r.Bottom, r.Width * 1.44f, c.Height * 0.07f, pal.MetalDark);
            RectE(g, r.X - r.Width * 0.22f, r.Bottom, r.Width * 1.44f, c.Height * 0.07f, pal.Edge);
        }
        /// <summary>在圆周上画螺栓点。</summary>
        private static void DrawBoltCircle(Graphics g, float cx, float cy, float rr, int n, HToolPalette pal)
        {
            float dot = Math.Max(1.4f, rr * 0.10f);
            using (var b = new SolidBrush(pal.Edge))
                for (int i = 0; i < n; i++)
                {
                    double a = i * Math.PI * 2d / n;
                    g.FillEllipse(b, cx + (float)Math.Cos(a) * rr - dot, cy + (float)Math.Sin(a) * rr - dot, dot * 2f, dot * 2f);
                }
        }
        /// <summary>B5 法兰盘电机。</summary>
        private void DrawFlangeB5(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.20f, c.Height * 0.34f, c.Width * 0.36f, c.Height * 0.48f);
            CylBody(g, r, pal);
            float fcx = c.Width * 0.58f, fcy = c.Height * 0.58f, fr = c.Height * 0.38f;
            using (var b = HBarBase.CylinderH(new RectangleF(fcx - fr * 0.26f, fcy - fr, fr * 0.52f, fr * 2f),
                pal.MetalDark, Color.WhiteSmoke))
                g.FillEllipse(b, fcx - fr * 0.26f, fcy - fr, fr * 0.52f, fr * 2f);
            DrawBoltCircle(g, fcx, fcy, fr * 0.78f, 8, pal);
            using (var b = new SolidBrush(pal.Metal))
                g.FillRectangle(b, fcx - fr * 0.08f, fcy - fr * 0.10f, c.Width * 0.30f, fr * 0.20f);
            DrawSpinMark(g, fcx - fr * 0.08f + c.Width * 0.30f - fr * 0.12f, fcy, fr * 0.12f, pal.Accent);
        }
        /// <summary>防爆电机：粗壳 + 防爆接线盒 + EX 标识。</summary>
        private void DrawExplosion(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.20f, c.Height * 0.34f, c.Width * 0.42f, c.Height * 0.50f);
            CylBody(g, r, pal);
            // 粗散热筋
            using (var b = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.2f)))
                for (int i = 1; i <= 6; i++)
                    g.FillRectangle(b, r.X + r.Width * i / 7f - c.Width * 0.005f, r.Y + c.Height * 0.02f,
                        c.Width * 0.01f, r.Height - c.Height * 0.04f);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.94f);
            // 高顶防爆接线盒
            var box = new RectangleF(c.Width * 0.30f, c.Height * 0.06f, c.Width * 0.20f, c.Height * 0.22f);
            using (GraphicsPath bp = HBarBase.RoundPath(box, c.Height * 0.02f))
            using (var b = new SolidBrush(pal.Dark))
                g.FillPath(b, bp);
            Rect(g, box.Right, box.Y + box.Height * 0.35f, c.Width * 0.05f, box.Height * 0.22f, pal.Edge);
            using (var f = new Font(Font.FontFamily, c.Height * 0.085f, FontStyle.Bold, GraphicsUnit.Pixel))
                TextRenderer.DrawText(g, "EX", f, new Rectangle((int)r.X, (int)(r.Y + r.Height * 0.35f),
                    (int)r.Width, (int)(r.Height * 0.3f)), Color.Gold,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        /// <summary>风冷电机：后风罩 + 格栅。</summary>
        private void DrawFanCooled(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.28f, c.Height * 0.32f, c.Width * 0.32f, c.Height * 0.52f);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.94f, spinMark: false);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 风罩
            float hcx = c.Width * 0.16f, hcy = c.Height * 0.58f, hr = c.Height * 0.30f;
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.3f)))
                g.FillEllipse(b, hcx - hr, hcy - hr, hr * 2f, hr * 2f);
            using (var p = new Pen(pal.Dark, 1.3f))
            {
                g.DrawEllipse(p, hcx - hr, hcy - hr, hr * 2f, hr * 2f);
                g.DrawEllipse(p, hcx - hr * 0.55f, hcy - hr * 0.55f, hr * 1.1f, hr * 1.1f);
            }
            // 径向格栅（运行时随机身旋转）
            double fanA = _spinAngle * Math.PI / 180d;
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.4f), Math.Max(1.4f, hr * 0.10f)))
                for (int i = 0; i < 6; i++)
                {
                    double a = i * Math.PI / 3d + fanA;
                    g.DrawLine(p, hcx + (float)Math.Cos(a) * hr * 0.35f, hcy + (float)Math.Sin(a) * hr * 0.35f,
                        hcx + (float)Math.Cos(a) * hr * 0.85f, hcy + (float)Math.Sin(a) * hr * 0.85f);
                }
        }
        /// <summary>皮带轮电机。</summary>
        private void DrawPulley(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.16f, c.Height * 0.40f, c.Width * 0.30f, c.Height * 0.42f);
            CylBody(g, r, pal);
            Feet(g, r, pal, c.Bottom - c.Height * 0.05f);
            // 电机轴（贯通至从动轮中心）
            float pcy = c.Height * 0.56f;
            float pcx = c.Width * 0.78f, pr = c.Height * 0.24f;
            float scx = r.Right, sr = pr * 0.55f;
            using (var b = new SolidBrush(pal.Metal))
                g.FillRectangle(b, scx, pcy - c.Height * 0.025f, pcx - scx + pr * 0.2f, c.Height * 0.05f);
            // 电机轴端小带轮
            using (var b = HBarBase.CylinderH(new RectangleF(scx - sr * 0.22f, pcy - sr, sr * 0.44f, sr * 2f),
                pal.MetalDark, Color.WhiteSmoke))
                g.FillEllipse(b, scx - sr * 0.22f, pcy - sr, sr * 0.44f, sr * 2f);
            DrawSpokes(g, scx, pcy, sr * 0.8f, 3, pal.Dark);
            // 三角皮带（两轮上下公切线）
            using (var p = new Pen(pal.Dark, Math.Max(2f, c.Height * 0.03f)))
            {
                g.DrawLine(p, scx, pcy - sr, pcx, pcy - pr);
                g.DrawLine(p, scx, pcy + sr, pcx, pcy + pr);
            }
            // 双槽从动皮带轮
            using (var b = HBarBase.CylinderH(new RectangleF(pcx - pr * 0.20f, pcy - pr, pr * 0.40f, pr * 2f),
                pal.MetalDark, Color.WhiteSmoke))
                g.FillEllipse(b, pcx - pr * 0.20f, pcy - pr, pr * 0.40f, pr * 2f);
            using (var p = new Pen(pal.Dark, 2f))
            {
                g.DrawEllipse(p, pcx - pr * 0.20f, pcy - pr, pr * 0.40f, pr * 2f);
                g.DrawLine(p, pcx - pr * 0.18f, pcy - pr * 0.35f, pcx + pr * 0.18f, pcy - pr * 0.35f);
                g.DrawLine(p, pcx - pr * 0.18f, pcy + pr * 0.35f, pcx + pr * 0.18f, pcy + pr * 0.35f);
            }
            // 从动轮辐条（减速比：角速度约为小轮的 0.55，同向）
            DrawSpokes(g, pcx, pcy, pr * 0.8f, 3, pal.Dark, 0.55f);
        }
        /// <summary>紧凑型短粗电机（双头轴、无脚座）。</summary>
        private void DrawCompact(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.30f, c.Height * 0.28f, c.Width * 0.34f, c.Height * 0.60f);
            CylBody(g, r, pal);
            // 右轴
            using (var b = new SolidBrush(pal.Metal))
            {
                g.FillRectangle(b, r.Right, r.Y + r.Height * 0.40f, c.Width * 0.18f, r.Height * 0.20f);
                g.FillRectangle(b, c.Width * 0.12f, r.Y + r.Height * 0.40f, r.X - c.Width * 0.12f, r.Height * 0.20f);
            }
            DrawSpinMark(g, r.Right + c.Width * 0.18f - r.Height * 0.09f, r.Y + r.Height * 0.5f,
                r.Height * 0.09f, pal.Accent);
            Rect(g, r.X + r.Width * 0.35f, r.Y + r.Height * 0.18f, r.Width * 0.30f, r.Height * 0.14f, pal.Dark);
        }
        /// <summary>滚筒电机：大直径滚筒 + 两端轴颈。</summary>
        private void DrawDrum(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.16f, c.Height * 0.18f, c.Width * 0.64f, c.Height * 0.62f);
            using (var b = HBarBase.CylinderV(r, pal.Dark, HToolPalettes.Tint(pal.Main, 0.6f), pal.Dark))
                g.FillRectangle(b, r);
            // 两端封盖
            using (var eb = HBarBase.CylinderH(new RectangleF(r.X - r.Height * 0.06f, r.Y, r.Height * 0.12f, r.Height), pal.Dark, HToolPalettes.Tint(pal.Main, 0.4f)))
            {
                g.FillEllipse(eb, r.X - r.Height * 0.06f, r.Y, r.Height * 0.12f, r.Height);
                g.FillEllipse(eb, r.Right - r.Height * 0.06f, r.Y, r.Height * 0.12f, r.Height);
            }
            using (var b = new SolidBrush(pal.Metal))
            {
                g.FillRectangle(b, c.Width * 0.04f, r.Y + r.Height * 0.42f, r.X - c.Width * 0.04f, r.Height * 0.16f);
                g.FillRectangle(b, r.Right, r.Y + r.Height * 0.42f, c.Width * 0.12f, r.Height * 0.16f);
            }
            DrawSpinMark(g, r.Right + c.Width * 0.12f - r.Height * 0.07f, r.Y + r.Height * 0.5f,
                r.Height * 0.07f, pal.Accent);
            // 脚座托辊
            Feet(g, new RectangleF(r.X, r.Bottom - c.Height * 0.06f, r.Width, c.Height * 0.06f), pal, c.Bottom - c.Height * 0.04f);
        }
        /// <summary>直线电机：长导轨 + 动子滑块。</summary>
        private void DrawLinear(Graphics g, RectangleF c, HToolPalette pal)
        {
            float gy = c.Bottom - c.Height * 0.16f;
            // 双导轨
            using (var b = new SolidBrush(pal.MetalDark))
                for (int i = 0; i < 2; i++)
                {
                    float ry = gy + i * c.Height * 0.08f;
                    g.FillRectangle(b, c.Width * 0.05f, ry, c.Width * 0.90f, c.Height * 0.035f);
                }
            // 磁轨条纹
            using (var b = new SolidBrush(pal.Dark))
                for (int i = 0; i < 20; i++)
                    g.FillRectangle(b, c.Width * 0.06f + i * c.Width * 0.044f, gy - c.Height * 0.02f,
                        c.Width * 0.02f, c.Height * 0.02f);
            // 动子滑块（运行时沿导轨往返；停止时冻结在当前位置）
            float k = (float)((Math.Sin(_linearPhase) + 1d) / 2d);
            var sl = new RectangleF(c.Width * 0.14f + k * c.Width * 0.44f, gy - c.Height * 0.34f, c.Width * 0.22f, c.Height * 0.32f);
            using (var sb = HBarBase.CylinderH(sl, pal.Main, HToolPalettes.Tint(pal.Main, 0.55f)))
                g.FillRectangle(sb, sl);
            RectE(g, sl.X, sl.Y, sl.Width, sl.Height, pal.Dark);
            // 拖链
            using (var p = new Pen(pal.Edge, 1.6f))
                g.DrawLine(p, c.Width * 0.06f, gy - c.Height * 0.06f, sl.X, sl.Bottom - c.Height * 0.02f);
        }
        /// <summary>带编码器电机。</summary>
        private void DrawEncoder(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.24f, c.Height * 0.32f, c.Width * 0.36f, c.Height * 0.50f);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.94f);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 后编码器圆盘
            float ecx = c.Width * 0.14f, ecy = c.Height * 0.57f, er = c.Height * 0.18f;
            using (var b = new SolidBrush(pal.Lighter))
                g.FillEllipse(b, ecx - er, ecy - er, er * 2f, er * 2f);
            using (var p = new Pen(pal.Dark, 1.2f))
            {
                g.DrawEllipse(p, ecx - er, ecy - er, er * 2f, er * 2f);
                g.DrawEllipse(p, ecx - er * 0.5f, ecy - er * 0.5f, er, er);
            }
            // 码盘刻线（运行时随轴转动）
            DrawSpinMark(g, ecx, ecy, er * 0.52f, pal.Accent);
            // 电缆
            using (var p = new Pen(pal.Edge, Math.Max(1.5f, c.Height * 0.018f)))
                g.DrawArc(p, ecx - er * 0.6f, ecy - er * 1.5f, er * 1.4f, er, 200f, 120f);
        }
        /// <summary>刹车电机：后端刹车鼓 + 松刹杆。</summary>
        private void DrawBrake(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.26f, c.Height * 0.34f, c.Width * 0.34f, c.Height * 0.48f);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.94f);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 刹车鼓
            var br = new RectangleF(c.Width * 0.12f, c.Height * 0.38f, c.Width * 0.13f, c.Height * 0.40f);
            using (var b = HBarBase.CylinderH(br, Color.FromArgb(172, 77, 80), Color.FromArgb(224, 150, 152)))
                g.FillRectangle(b, br);
            RectE(g, br.X, br.Y, br.Width, br.Height, Color.FromArgb(120, 50, 52));
            // 松刹杆
            using (var p = new Pen(pal.Edge, Math.Max(1.6f, c.Height * 0.02f)))
                g.DrawLine(p, br.X + br.Width * 0.5f, br.Y, br.X + br.Width * 0.5f, br.Y - c.Height * 0.12f);
            g.FillEllipse(Brushes.DimGray, br.X + br.Width * 0.36f, br.Y - c.Height * 0.15f,
                br.Width * 0.28f, c.Height * 0.045f);
        }
        /// <summary>防水电机：光面机身 + 密封环纹 + 防水电缆接头。</summary>
        private void DrawWaterproof(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.22f, c.Height * 0.34f, c.Width * 0.40f, c.Height * 0.48f);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.94f);
            // 密封环纹
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.35f), 1.5f))
            {
                g.DrawEllipse(p, r.X - r.Height * 0.08f, r.Y + 1f, r.Height * 0.16f, r.Height - 2f);
                g.DrawEllipse(p, r.Right - r.Height * 0.08f, r.Y + 1f, r.Height * 0.16f, r.Height - 2f);
            }
            // 顶防水接头（螺旋帽 + 电缆弧）
            var gl = new RectangleF(c.Width * 0.36f, c.Height * 0.20f, c.Width * 0.07f, c.Height * 0.10f);
            using (var b = new SolidBrush(pal.MetalDark))
                g.FillRectangle(b, gl);
            using (var p = new Pen(pal.Edge, 1.1f))
            {
                g.DrawRectangle(p, gl.X, gl.Y, gl.Width, gl.Height);
                for (int i = 1; i <= 2; i++)
                    g.DrawLine(p, gl.X, gl.Y + gl.Height * i / 3f, gl.Right, gl.Y + gl.Height * i / 3f);
                g.DrawArc(p, gl.X - c.Width * 0.06f, c.Height * 0.06f, c.Width * 0.14f, c.Height * 0.18f, 200f, 120f);
            }
        }
        /// <summary>自冷电机：全长纵向散热筋。</summary>
        private void DrawDripProof(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.20f, c.Height * 0.30f, c.Width * 0.42f, c.Height * 0.54f);
            using (var b = new SolidBrush(pal.Main))
                g.FillRectangle(b, r);
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.32f), c.Width * 0.008f))
                for (int i = 1; i <= 9; i++)
                    g.DrawLine(p, r.X, r.Y + r.Height * i / 10f, r.Right, r.Y + r.Height * i / 10f);
            using (var eb = HBarBase.CylinderH(new RectangleF(r.X - r.Height * 0.08f, r.Y, r.Height * 0.16f, r.Height),
                pal.Dark, HToolPalettes.Tint(pal.Main, 0.45f)))
                g.FillEllipse(eb, r.X - r.Height * 0.08f, r.Y, r.Height * 0.16f, r.Height);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.95f);
            Feet(g, r, pal, c.Bottom - c.Height * 0.05f);
        }
        /// <summary>直流有刷电机：圆机身 + 碳刷盖 + 端子。</summary>
        private void DrawDC(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.22f, c.Height * 0.32f, c.Width * 0.40f, c.Height * 0.50f);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.94f);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 两个碳刷检修盖（顶部斜黑圆）
            using (var b = new SolidBrush(pal.Dark))
                foreach (float bx in new[] { r.X + r.Width * 0.28f, r.X + r.Width * 0.66f })
                    g.FillEllipse(b, bx - c.Width * 0.025f, r.Y - c.Height * 0.05f, c.Width * 0.05f, c.Height * 0.07f);
            // 端子板
            Rect(g, c.Width * 0.34f, c.Height * 0.10f, c.Width * 0.16f, c.Height * 0.08f, pal.MetalDark);
            g.FillEllipse(Brushes.Gold, c.Width * 0.365f, c.Height * 0.115f, c.Width * 0.025f, c.Height * 0.05f);
            g.FillEllipse(Brushes.Gold, c.Width * 0.45f, c.Height * 0.115f, c.Width * 0.025f, c.Height * 0.05f);
        }
        /// <summary>空心轴电机：前法兰 + 中心大孔。</summary>
        private void DrawHollow(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.20f, c.Height * 0.34f, c.Width * 0.36f, c.Height * 0.48f);
            CylBody(g, r, pal);
            float fcx = c.Width * 0.64f, fcy = c.Height * 0.58f, fr = c.Height * 0.34f;
            using (var b = HBarBase.CylinderH(new RectangleF(fcx - fr * 0.15f, fcy - fr, fr * 0.30f, fr * 2f),
                pal.MetalDark, Color.WhiteSmoke))
                g.FillEllipse(b, fcx - fr * 0.15f, fcy - fr, fr * 0.30f, fr * 2f);
            DrawBoltCircle(g, fcx, fcy, fr * 0.75f, 8, pal);
            // 中心孔
            using (var b = new SolidBrush(Color.FromArgb(48, 52, 58)))
                g.FillEllipse(b, fcx - fr * 0.34f, fcy - fr * 0.34f, fr * 0.68f, fr * 0.68f);
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawEllipse(p, fcx - fr * 0.34f, fcy - fr * 0.34f, fr * 0.68f, fr * 0.68f);
            // 空心轴孔内旋转标记
            DrawSpinMark(g, fcx, fcy, fr * 0.27f, pal.Accent);
        }
        /// <summary>直角减速电机：电机 + 蜗轮箱 + 下输出轴。</summary>
        private void DrawRightAngle(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.06f, c.Height * 0.30f, c.Width * 0.34f, c.Height * 0.34f);
            CylBody(g, r, pal);
            var box = new RectangleF(c.Width * 0.42f, c.Height * 0.22f, c.Width * 0.28f, c.Height * 0.42f);
            using (var b = HBarBase.CylinderH(box, HToolPalettes.Shade(pal.Main, 0.12f), HToolPalettes.Tint(pal.Main, 0.5f)))
                g.FillRectangle(b, box);
            RectE(g, box.X, box.Y, box.Width, box.Height, pal.Dark);
            // 下伸箱体 + 输出轴（收在控件底边内，避免被裁切）
            Rect(g, box.X + box.Width * 0.18f, box.Bottom, box.Width * 0.64f, c.Height * 0.24f,
                HToolPalettes.Shade(pal.Main, 0.08f));
            RectE(g, box.X + box.Width * 0.18f, box.Bottom, box.Width * 0.64f, c.Height * 0.24f, pal.Dark);
            using (var b = new SolidBrush(pal.Metal))
                g.FillRectangle(b, box.X + box.Width * 0.42f, box.Bottom + c.Height * 0.24f,
                    box.Width * 0.16f, c.Height * 0.07f);
            DrawSpinMark(g, box.X + box.Width * 0.5f, box.Bottom + c.Height * 0.275f,
                c.Height * 0.03f, pal.Accent);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
        }
        /// <summary>电机泵组：联轴器 + 离心泵蜗壳。</summary>
        private void DrawPump(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.08f, c.Height * 0.34f, c.Width * 0.30f, c.Height * 0.42f);
            CylBody(g, r, pal);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 联轴器
            float ccx = c.Width * 0.47f, ccy = c.Height * 0.55f, cr = c.Height * 0.12f;
            using (var b = new SolidBrush(pal.MetalDark))
            {
                g.FillEllipse(b, ccx - cr, ccy - cr, cr * 0.9f, cr * 2f);
                g.FillEllipse(b, ccx + cr * 0.1f, ccy - cr, cr * 0.9f, cr * 2f);
            }
            for (int i = -1; i <= 1; i++)
                Rect(g, ccx - cr * 0.18f, ccy - cr * 0.7f + i * cr * 0.7f, cr * 0.36f, cr * 0.28f, pal.Accent);
            // 联轴器旋转标记
            DrawSpinMark(g, ccx + cr * 0.05f, ccy, cr * 0.42f, pal.Accent);
            // 蜗壳
            float vcx = c.Width * 0.66f, vcy = c.Height * 0.58f, vr = c.Height * 0.26f;
            using (var b = HBarBase.CylinderH(new RectangleF(vcx - vr, vcy - vr, vr * 2f, vr * 2f),
                pal.Main, HToolPalettes.Tint(pal.Main, 0.55f)))
                g.FillEllipse(b, vcx - vr, vcy - vr, vr * 2f, vr * 2f);
            using (var p = new Pen(pal.Dark, 1.4f))
            {
                g.DrawEllipse(p, vcx - vr, vcy - vr, vr * 2f, vr * 2f);
                g.DrawArc(p, vcx - vr * 0.6f, vcy - vr * 0.6f, vr * 1.2f, vr * 1.2f, 30f, 300f);
            }
            // 出口管（上弯）+ 入口管（右出）
            using (var b = new SolidBrush(pal.MetalDark))
            {
                g.FillRectangle(b, vcx - vr * 0.16f, vcy - vr - c.Height * 0.10f, vr * 0.32f, c.Height * 0.12f);
                g.FillRectangle(b, vcx + vr * 0.7f, vcy - vr * 0.16f, c.Width * 0.14f, vr * 0.32f);
            }
            Feet(g, new RectangleF(vcx - vr, vcy + vr * 0.6f, vr * 2f, c.Height * 0.06f), pal, c.Bottom - c.Height * 0.05f);
        }
        /// <summary>电机风机组：前蜗壳方箱 + 视窗叶轮。</summary>
        private void DrawFanUnit(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.08f, c.Height * 0.38f, c.Width * 0.30f, c.Height * 0.38f);
            CylBody(g, r, pal);
            Feet(g, r, pal, c.Bottom - c.Height * 0.06f);
            // 联轴器短轴
            using (var b = new SolidBrush(pal.Metal))
                g.FillRectangle(b, r.Right, r.Y + r.Height * 0.42f, c.Width * 0.06f, r.Height * 0.16f);
            // 蜗壳方箱
            var box = new RectangleF(c.Width * 0.46f, c.Height * 0.22f, c.Width * 0.36f, c.Height * 0.60f);
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.62f)))
                g.FillRectangle(b, box);
            RectE(g, box.X, box.Y, box.Width, box.Height, pal.Dark, 1.4f);
            // 视窗 + 三叶片
            float wcx = box.X + box.Width * 0.5f, wcy = box.Y + box.Height * 0.5f, wr = box.Height * 0.34f;
            using (var b = new SolidBrush(Color.FromArgb(225, 232, 240)))
                g.FillEllipse(b, wcx - wr, wcy - wr, wr * 2f, wr * 2f);
            using (var bp = new SolidBrush(pal.Accent))
                for (int i = 0; i < 3; i++)
                {
                    double a = i * Math.PI * 2d / 3d + 0.35d + _spinAngle * Math.PI / 180d;
                    var pts = new[]
                    {
                        new PointF(wcx, wcy),
                        new PointF(wcx + (float)Math.Cos(a - 0.22d) * wr * 0.85f, wcy + (float)Math.Sin(a - 0.22d) * wr * 0.85f),
                        new PointF(wcx + (float)Math.Cos(a + 0.22d) * wr * 0.85f, wcy + (float)Math.Sin(a + 0.22d) * wr * 0.85f)
                    };
                    g.FillPolygon(bp, pts);
                }
            using (var p = new Pen(pal.Dark, 1.2f))
                g.DrawEllipse(p, wcx - wr, wcy - wr, wr * 2f, wr * 2f);
            // 出风口
            Rect(g, box.Right, box.Y + box.Height * 0.12f, c.Width * 0.06f, box.Height * 0.24f, pal.MetalDark);
        }
        /// <summary>NEMA 标准电机：大脚座 + 顶吊环。</summary>
        private void DrawNEMA(Graphics g, RectangleF c, HToolPalette pal)
        {
            var r = new RectangleF(c.Width * 0.20f, c.Height * 0.30f, c.Width * 0.42f, c.Height * 0.52f);
            CylBody(g, r, pal);
            Shaft(g, r.Right, r, pal, 0.96f, c.Width * 0.95f);
            // 大脚座（通长）
            float gy = c.Bottom - c.Height * 0.04f;
            Rect(g, r.X - c.Width * 0.04f, gy - c.Height * 0.08f, r.Width + c.Width * 0.10f, c.Height * 0.07f, pal.MetalDark);
            RectE(g, r.X - c.Width * 0.04f, gy - c.Height * 0.08f, r.Width + c.Width * 0.10f, c.Height * 0.07f, pal.Edge);
            g.FillEllipse(Brushes.DimGray, r.X, gy - c.Height * 0.055f, c.Height * 0.035f, c.Height * 0.035f);
            g.FillEllipse(Brushes.DimGray, r.Right - c.Height * 0.035f, gy - c.Height * 0.055f, c.Height * 0.035f, c.Height * 0.035f);
            // 顶吊环螺钉
            float ex = r.X + r.Width * 0.30f, er = c.Height * 0.05f;
            using (var p = new Pen(pal.Metal, Math.Max(1.6f, c.Height * 0.018f)))
                g.DrawEllipse(p, ex - er, c.Height * 0.02f, er * 2f, er * 2f);
            Rect(g, ex - c.Width * 0.008f, c.Height * 0.10f, c.Width * 0.016f, c.Height * 0.05f, pal.MetalDark);
        }
    }
}