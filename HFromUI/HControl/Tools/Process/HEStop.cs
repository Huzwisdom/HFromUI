using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Process
{
    using HFromUI.HControl.Base;
    /// <summary>
    /// 急停按钮外形：22 种工业紧急停止按钮外形——标准蘑菇头、防护圈、挂锁式、
    /// 照光式、旋转释放、脚踏、拉绳、击碎玻璃、钥匙复位等。
    /// </summary>
    public enum HEStopStyle
    {
        Classic = 0,         // 黄底板红蘑菇头
        Panel = 1,           // 面板安装式
        Guard = 2,           // 黄色防护圈
        Lockable = 3,        // 挂锁式
        Illuminated = 4,     // 照光式
        Dual = 5,            // 双联急停
        TwistRelease = 6,    // 旋转释放纹
        MushroomLarge = 7,   // 超大蘑菇头
        Compact = 8,         // 紧凑型
        ExplosionProof = 9,  // 防爆厚壳
        Foot = 10,           // 脚踏急停
        RopePull = 11,       // 拉绳开关
        Palm = 12,           // 掌按圆盘
        Standard40 = 13,     // 标准 40mm 型
        MetalRing = 14,      // 金属锁紧环
        FullGuard = 15,      // 全包围护罩
        DoubleHead = 16,     // 双蘑菇头
        SurfaceBox = 17,     // 明装盒式
        BreakGlass = 18,     // 击碎玻璃箱
        PushLock = 19,       // 自锁标签式
        Wireless = 20,       // 无线急停
        KeyReset = 21        // 钥匙复位式
    }
    /// <summary>
    /// 急停按钮控件（继承 HToolAnimBase）：22 种急停/停止外形，Pressed 表示按钮被按下（自锁状态），
    /// 按下时蘑菇头下陷、变暗；点击控件可切换按下状态。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("急停按钮控件：22 种急停外形，点击切换按下状态")]
    public class HEStop : HToolAnimBase
    {
        private HEStopStyle _style = HEStopStyle.Classic;
        private bool _pressed;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        private static readonly Color CYellow = Color.FromArgb(247, 201, 32);
        private static readonly Color CYellowDk = Color.FromArgb(190, 142, 20);
        private static readonly Color CRed = Color.FromArgb(214, 38, 38);
        private static readonly Color CRedLt = Color.FromArgb(246, 96, 86);
        private static readonly Color CRedDk = Color.FromArgb(132, 22, 22);
        private static readonly Color CMetal = Color.FromArgb(196, 200, 206);
        private static readonly Color CMetalDk = Color.FromArgb(112, 116, 124);
        /// <summary>急停按钮外形。</summary>
        [HCategoryLanguage("急停按钮"), HDisplayNameLanguage("急停按钮外形样式"), HDescriptionLanguage("急停按钮外形样式"), Browsable(true)]
        [DefaultValue(HEStopStyle.Classic)]
        public HEStopStyle EStopStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>按钮是否处于按下（自锁）状态。</summary>
        [HCategoryLanguage("急停按钮"), HDisplayNameLanguage("按钮是否被按下"), HDescriptionLanguage("按钮是否被按下（自锁），点击可切换"), Browsable(true)]
        [DefaultValue(false)]
        public bool Pressed
        {
            get => _pressed;
            set { _pressed = value; Invalidate(); }
        }
        protected override void OnClick(EventArgs e)
        {
            _pressed = !_pressed;
            Invalidate();
            base.OnClick(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pal = Palette;
            var a = PlacedGlyphArea();
            if (a.Width <= 2f || a.Height <= 2f) { PaintPlacedText(g); return; }
            switch (_style)
            {
                case HEStopStyle.Panel: DrawPanelMount(g, a); break;
                case HEStopStyle.Guard: DrawGuarded(g, a, false); break;
                case HEStopStyle.Lockable: DrawLockable(g, a); break;
                case HEStopStyle.Illuminated: DrawIlluminated(g, a); break;
                case HEStopStyle.Dual: DrawDual(g, a); break;
                case HEStopStyle.TwistRelease: DrawTwist(g, a); break;
                case HEStopStyle.MushroomLarge: DrawClassic(g, a, 0.96f, 0.82f); break;
                case HEStopStyle.Compact: DrawClassic(g, a, 0.58f, 0.78f); break;
                case HEStopStyle.ExplosionProof: DrawExplosion(g, a); break;
                case HEStopStyle.Foot: DrawFoot(g, a); break;
                case HEStopStyle.RopePull: DrawRopePull(g, a); break;
                case HEStopStyle.Palm: DrawClassic(g, a, 0.9f, 0.66f); break;
                case HEStopStyle.Standard40: DrawClassic(g, a, 0.86f, 0.6f); break;
                case HEStopStyle.MetalRing: DrawMetalRing(g, a); break;
                case HEStopStyle.FullGuard: DrawGuarded(g, a, true); break;
                case HEStopStyle.DoubleHead: DrawDoubleHead(g, a); break;
                case HEStopStyle.SurfaceBox: DrawSurfaceBox(g, a); break;
                case HEStopStyle.BreakGlass: DrawBreakGlass(g, a); break;
                case HEStopStyle.PushLock: DrawPushLock(g, a); break;
                case HEStopStyle.Wireless: DrawWireless(g, a); break;
                case HEStopStyle.KeyReset: DrawKeyReset(g, a); break;
                default: DrawClassic(g, a, 0.74f, 0.58f); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // 红色蘑菇头：裙座 + 凸圆盖；pressed 时下陷变暗
        // ----------------------------------------------------------------
        private void DrawMushroom(Graphics g, RectangleF r)
        {
            if (r.Width <= 3f || r.Height <= 3f) return;
            float sink = _pressed ? r.Height * 0.12f : 0f;
            // 裙座（圆柱）
            var skirt = new RectangleF(r.Left + r.Width * 0.2f, r.Top + r.Height * 0.42f + sink,
                r.Width * 0.6f, r.Height * 0.34f);
            using (var sb = new LinearGradientBrush(skirt, CRedDk, CRed, 0f))
                g.FillRectangle(sb, skirt);
            // 圆盖（蘑菇伞）
            var cap = new RectangleF(r.Left, r.Top + sink, r.Width, r.Height * 0.62f);
            Color hi = _pressed ? CRed : CRedLt;
            Color lo = _pressed ? CRedDk : CRedDk;
            using (var cb = new LinearGradientBrush(cap, hi, lo, 90f))
                g.FillEllipse(cb, cap);
            // 伞沿
            using (var rim = new Pen(CRedDk, Math.Max(1f, r.Width * 0.02f)))
                g.DrawEllipse(rim, cap);
            // 高光
            var hl = new RectangleF(cap.Left + cap.Width * 0.2f, cap.Top + cap.Height * 0.14f + sink,
                cap.Width * 0.28f, cap.Height * 0.22f);
            using (var hb = new SolidBrush(Color.FromArgb(_pressed ? 50 : 130, Color.White)))
                g.FillEllipse(hb, hl);
        }
        // 黄色圆底板
        private void DrawYellowPlate(Graphics g, RectangleF r, float dRatio)
        {
            float d = Math.Min(r.Width, r.Height) * dRatio;
            var pr = new RectangleF(r.Left + (r.Width - d) / 2f, r.Top + (r.Height - d) / 2f, d, d);
            using (var pb = new LinearGradientBrush(pr, CYellow, CYellowDk, 90f))
                g.FillEllipse(pb, pr);
            using (var pp = new Pen(CYellowDk, Math.Max(1f, d * 0.02f)))
                g.DrawEllipse(pp, pr);
            return;
        }
        // ----------------------------------------------------------------
        // Classic：黄底板 + 蘑菇头（蘑菇占比/底板占比可调）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a, float mushRatio, float plateRatio)
        {
            DrawYellowPlate(g, a, plateRatio + 0.32f);
            float d = Math.Min(a.Width, a.Height) * mushRatio;
            DrawMushroom(g, new RectangleF(a.Left + (a.Width - d) / 2f,
                a.Top + (a.Height - d) / 2f - a.Height * 0.02f, d, d));
        }
        // ----------------------------------------------------------------
        // 面板安装：灰色面板 + 锁紧螺母 + 蘑菇头
        // ----------------------------------------------------------------
        private void DrawPanelMount(Graphics g, RectangleF a)
        {
            var panel = RectangleF.Inflate(a, -a.Width * 0.04f, -a.Height * 0.18f);
            using (var pb = new LinearGradientBrush(panel, Color.FromArgb(222, 224, 228),
                Color.FromArgb(182, 185, 191), 90f))
            using (var path = HBarBase.RoundPath(panel, 4f))
                g.FillPath(pb, path);
            // 安装孔
            using (var hole = new SolidBrush(CMetalDk))
            {
                float hr = Math.Max(1.2f, panel.Width * 0.04f);
                g.FillEllipse(hole, panel.Left + panel.Width * 0.12f - hr, panel.Bottom - panel.Height * 0.18f - hr, hr * 2, hr * 2);
                g.FillEllipse(hole, panel.Right - panel.Width * 0.12f - hr, panel.Bottom - panel.Height * 0.18f - hr, hr * 2, hr * 2);
            }
            // 锁紧螺母（六边形）
            float cx = a.Left + a.Width / 2f;
            float nutR = Math.Min(a.Width, a.Height) * 0.3f;
            var nutPts = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double ang = Math.PI / 6 + i * Math.PI / 3;
                nutPts[i] = new PointF(cx + (float)Math.Cos(ang) * nutR,
                    a.Top + a.Height * 0.58f + (float)Math.Sin(ang) * nutR * 0.42f);
            }
            using (var nb = new SolidBrush(CMetalDk))
                g.FillPolygon(nb, nutPts);
            float d = Math.Min(a.Width, a.Height) * 0.62f;
            DrawMushroom(g, new RectangleF(cx - d / 2f, a.Top + a.Height * 0.08f, d, d));
        }
        // ----------------------------------------------------------------
        // 防护圈：黄色高围圈；full=全包围开口式
        // ----------------------------------------------------------------
        private void DrawGuarded(Graphics g, RectangleF a, bool full)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float d = Math.Min(a.Width, a.Height) * (full ? 0.96f : 0.92f);
            float ringW = d * 0.16f;
            var outer = new RectangleF(cx - d / 2f, cy - d / 2f, d, d);
            float sweep = full ? 320f : 360f;
            float start = full ? 200f : 0f;
            // 护圈（厚环）
            using (var guard = new Pen(CYellow, ringW) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawArc(guard, outer, start, sweep);
            using (var edge = new Pen(CYellowDk, Math.Max(1f, ringW * 0.18f)))
                g.DrawArc(edge, outer, start, sweep);
            // 按钮
            float bd = d * 0.52f;
            DrawMushroom(g, new RectangleF(cx - bd / 2f, cy - bd / 2f - a.Height * 0.02f, bd, bd));
        }
        // ----------------------------------------------------------------
        // 挂锁式：黄底板 + 锁孔耳
        // ----------------------------------------------------------------
        private void DrawLockable(Graphics g, RectangleF a)
        {
            DrawYellowPlate(g, a, 0.86f);
            float bd = Math.Min(a.Width, a.Height) * 0.52f;
            DrawMushroom(g, new RectangleF(a.Left + (a.Width - bd) / 2f,
                a.Top + (a.Height - bd) / 2f - a.Height * 0.04f, bd, bd));
            // 底板下方挂锁耳（两个小孔）
            using (var hole = new SolidBrush(CRedDk))
            {
                float hr = Math.Max(1.5f, a.Width * 0.05f);
                float y = a.Bottom - a.Height * 0.16f;
                g.FillEllipse(hole, a.Left + a.Width * 0.34f - hr, y - hr, hr * 2, hr * 2);
                g.FillEllipse(hole, a.Left + a.Width * 0.66f - hr, y - hr, hr * 2, hr * 2);
            }
        }
        // ----------------------------------------------------------------
        // 照光式：按钮发光环
        // ----------------------------------------------------------------
        private void DrawIlluminated(Graphics g, RectangleF a)
        {
            DrawYellowPlate(g, a, 0.86f);
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float d = Math.Min(a.Width, a.Height) * 0.62f;
            // 发光环
            int glowA = _pressed ? 200 : 60;
            using (var ring = new Pen(Color.FromArgb(glowA, 255, 230, 120), Math.Max(2f, d * 0.08f)))
                g.DrawEllipse(ring, cx - d * 0.62f, cy - d * 0.62f, d * 1.24f, d * 1.24f);
            DrawMushroom(g, new RectangleF(cx - d / 2f, cy - d / 2f - a.Height * 0.02f, d, d));
            // STOP 白字箭头
            using (var wb = new SolidBrush(Color.White))
                g.FillPolygon(wb, new[]
                {
                    new PointF(cx - d * 0.1f, cy - d * 0.2f), new PointF(cx + d * 0.1f, cy - d * 0.2f),
                    new PointF(cx + d * 0.1f, cy), new PointF(cx + d * 0.22f, cy),
                    new PointF(cx, cy + d * 0.22f), new PointF(cx - d * 0.22f, cy),
                    new PointF(cx - d * 0.1f, cy)
                });
        }
        // ----------------------------------------------------------------
        // 双联
        // ----------------------------------------------------------------
        private void DrawDual(Graphics g, RectangleF a)
        {
            float d = Math.Min(a.Width * 0.46f, a.Height * 0.7f);
            float y = a.Top + (a.Height - d) / 2f;
            DrawYellowPlate(g, new RectangleF(a.Left, y, a.Width / 2f, d), 0.9f);
            DrawYellowPlate(g, new RectangleF(a.Left + a.Width / 2f, y, a.Width / 2f, d), 0.9f);
            DrawMushroom(g, new RectangleF(a.Left + a.Width / 4f - d * 0.36f, y + d * 0.12f, d * 0.72f, d * 0.72f));
            bool saved = _pressed; _pressed = false;
            DrawMushroom(g, new RectangleF(a.Left + a.Width * 3f / 4f - d * 0.36f, y + d * 0.12f, d * 0.72f, d * 0.72f));
            _pressed = saved;
        }
        // ----------------------------------------------------------------
        // 旋转释放箭头纹（黄底板 + 双箭头）
        // ----------------------------------------------------------------
        private void DrawTwist(Graphics g, RectangleF a)
        {
            DrawYellowPlate(g, a, 0.9f);
            float d = Math.Min(a.Width, a.Height) * 0.5f;
            DrawMushroom(g, new RectangleF(a.Left + (a.Width - d) / 2f,
                a.Top + a.Height * 0.06f, d, d));
            float cx = a.Left + a.Width / 2f, cy = a.Bottom - a.Height * 0.18f;
            using (var ap = new Pen(CRedDk, Math.Max(1.5f, a.Width * 0.03f))
            { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
            {
                float r = Math.Min(a.Width, a.Height) * 0.14f;
                g.DrawArc(ap, cx - r, cy - r * 0.6f, r * 2f, r * 1.2f, 30f, 200f);
                g.DrawArc(ap, cx - r, cy - r * 0.6f, r * 2f, r * 1.2f, 210f, 200f);
            }
        }
        // ----------------------------------------------------------------
        // 防爆厚壳
        // ----------------------------------------------------------------
        private void DrawExplosion(Graphics g, RectangleF a)
        {
            var shell = RectangleF.Inflate(a, -a.Width * 0.08f, -a.Height * 0.1f);
            using (var sb = new LinearGradientBrush(shell, Color.FromArgb(120, 124, 130),
                Color.FromArgb(70, 74, 80), 90f))
            using (var path = HBarBase.RoundPath(shell, shell.Width * 0.12f))
                g.FillPath(sb, path);
            // 螺栓
            using (var bolt = new SolidBrush(CMetalDk))
            {
                float br = Math.Max(1.5f, shell.Width * 0.04f);
                g.FillEllipse(bolt, shell.Left + shell.Width * 0.1f - br, shell.Top + shell.Height * 0.1f - br, br * 2, br * 2);
                g.FillEllipse(bolt, shell.Right - shell.Width * 0.1f - br, shell.Top + shell.Height * 0.1f - br, br * 2, br * 2);
                g.FillEllipse(bolt, shell.Left + shell.Width * 0.1f - br, shell.Bottom - shell.Height * 0.1f - br, br * 2, br * 2);
                g.FillEllipse(bolt, shell.Right - shell.Width * 0.1f - br, shell.Bottom - shell.Height * 0.1f - br, br * 2, br * 2);
            }
            float d = Math.Min(shell.Width, shell.Height) * 0.62f;
            DrawMushroom(g, new RectangleF(shell.Left + (shell.Width - d) / 2f,
                shell.Top + shell.Height * 0.06f, d, d));
        }
        // ----------------------------------------------------------------
        // 脚踏急停（黄踏板 + 红压块）
        // ----------------------------------------------------------------
        private void DrawFoot(Graphics g, RectangleF a)
        {
            // 底座
            var baseR = new RectangleF(a.Left + a.Width * 0.06f, a.Top + a.Height * 0.52f,
                a.Width * 0.88f, a.Height * 0.34f);
            using (var bb = new LinearGradientBrush(baseR, CMetal, CMetalDk, 90f))
                g.FillRectangle(bb, baseR);
            // 黄色踏板
            var pedal = new[]
            {
                new PointF(a.Left + a.Width * 0.12f, a.Top + a.Height * 0.6f),
                new PointF(a.Right - a.Width * 0.12f, a.Top + a.Height * 0.6f),
                new PointF(a.Right - a.Width * 0.22f, a.Top + a.Height * 0.4f),
                new PointF(a.Left + a.Width * 0.28f, a.Top + a.Height * 0.4f)
            };
            using (var yb = new LinearGradientBrush(a, CYellow, CYellowDk, 90f))
                g.FillPolygon(yb, pedal);
            // 红色压块
            var red = new RectangleF(a.Left + a.Width * 0.3f,
                a.Top + (_pressed ? a.Height * 0.48f : a.Height * 0.28f),
                a.Width * 0.4f, a.Height * 0.2f);
            using (var rb = new LinearGradientBrush(red, CRedLt, CRedDk, 90f))
                g.FillRectangle(rb, red);
        }
        // ----------------------------------------------------------------
        // 拉绳开关：方盒 + 拉环 + 绳
        // ----------------------------------------------------------------
        private void DrawRopePull(Graphics g, RectangleF a)
        {
            var box = new RectangleF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.12f,
                a.Width * 0.68f, a.Height * 0.42f);
            using (var bb = new LinearGradientBrush(box, Color.FromArgb(214, 64, 54), CRedDk, 90f))
            using (var path = HBarBase.RoundPath(box, 4f))
                g.FillPath(bb, path);
            // 蘑菇点
            float d = box.Width * 0.3f;
            DrawMushroom(g, new RectangleF(box.Left + (box.Width - d) / 2f, box.Top - box.Height * 0.2f, d, d));
            // 拉绳 + 环
            float cx = a.Left + a.Width / 2f;
            float ringY = a.Bottom - a.Height * 0.16f;
            using (var rope = new Pen(CMetalDk, Math.Max(1f, a.Width * 0.025f)))
            {
                g.DrawLine(rope, cx, box.Bottom - 2f, cx, ringY - a.Width * 0.1f);
                g.DrawEllipse(rope, cx - a.Width * 0.12f, ringY - a.Width * 0.12f,
                    a.Width * 0.24f, a.Width * 0.24f);
                // 左右拉绳
                g.DrawLine(rope, a.Left + a.Width * 0.12f, a.Bottom - a.Height * 0.04f,
                    cx - a.Width * 0.12f, ringY);
                g.DrawLine(rope, a.Right - a.Width * 0.12f, a.Bottom - a.Height * 0.04f,
                    cx + a.Width * 0.12f, ringY);
            }
        }
        // ----------------------------------------------------------------
        // 金属锁紧环特写
        // ----------------------------------------------------------------
        private void DrawMetalRing(Graphics g, RectangleF a)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height / 2f;
            float d = Math.Min(a.Width, a.Height) * 0.9f;
            using (var ring = new LinearGradientBrush(
                new RectangleF(cx - d / 2f, cy - d / 2f, d, d), CMetal, CMetalDk, 45f))
                g.FillEllipse(ring, cx - d / 2f, cy - d / 2f, d, d);
            using (var hole = new SolidBrush(Color.FromArgb(60, 62, 66)))
                g.FillEllipse(hole, cx - d * 0.32f, cy - d * 0.32f, d * 0.64f, d * 0.64f);
            // 棱面
            using (var facet = new Pen(Color.FromArgb(120, Color.White), 1f))
                g.DrawArc(facet, cx - d * 0.42f, cy - d * 0.42f, d * 0.84f, d * 0.84f, 200f, 130f);
            float bd = d * 0.56f;
            DrawMushroom(g, new RectangleF(cx - bd / 2f, cy - bd / 2f - a.Height * 0.03f, bd, bd));
        }
        // ----------------------------------------------------------------
        // 双蘑菇头
        // ----------------------------------------------------------------
        private void DrawDoubleHead(Graphics g, RectangleF a)
        {
            var plate = RectangleF.Inflate(a, -a.Width * 0.05f, -a.Height * 0.22f);
            using (var pb = new SolidBrush(CYellow))
            using (var path = HBarBase.RoundPath(plate, plate.Height * 0.3f))
                g.FillPath(pb, path);
            float d = Math.Min(a.Width * 0.4f, a.Height * 0.62f);
            float y = a.Top + (a.Height - d) / 2f - a.Height * 0.04f;
            DrawMushroom(g, new RectangleF(a.Left + a.Width * 0.16f, y, d, d));
            DrawMushroom(g, new RectangleF(a.Right - a.Width * 0.16f - d, y, d, d));
        }
        // ----------------------------------------------------------------
        // 明装盒
        // ----------------------------------------------------------------
        private void DrawSurfaceBox(Graphics g, RectangleF a)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.1f, -a.Height * 0.1f);
            using (var bb = new LinearGradientBrush(box, Color.FromArgb(214, 216, 220),
                Color.FromArgb(168, 171, 178), 90f))
            using (var path = HBarBase.RoundPath(box, 6f))
                g.FillPath(bb, path);
            DrawYellowPlate(g, box, 0.78f);
            float d = Math.Min(box.Width, box.Height) * 0.56f;
            DrawMushroom(g, new RectangleF(box.Left + (box.Width - d) / 2f,
                box.Top + box.Height * 0.08f, d, d));
        }
        // ----------------------------------------------------------------
        // 击碎玻璃箱
        // ----------------------------------------------------------------
        private void DrawBreakGlass(Graphics g, RectangleF a)
        {
            var box = RectangleF.Inflate(a, -a.Width * 0.08f, -a.Height * 0.08f);
            using (var red = new SolidBrush(CRed))
            using (var path = HBarBase.RoundPath(box, 4f))
                g.FillPath(red, path);
            var glass = RectangleF.Inflate(box, -box.Width * 0.1f, -box.Height * 0.12f);
            using (var gb = new SolidBrush(Color.FromArgb(_pressed ? 120 : 210, 230, 238, 245)))
                g.FillRectangle(gb, glass);
            // 裂纹
            using (var crack = new Pen(Color.FromArgb(120, 140, 150), 1f))
            {
                g.DrawLine(crack, glass.Left + glass.Width * 0.3f, glass.Top,
                    glass.Left + glass.Width * 0.42f, glass.Top + glass.Height * 0.5f);
                g.DrawLine(crack, glass.Left + glass.Width * 0.42f, glass.Top + glass.Height * 0.5f,
                    glass.Left + glass.Width * 0.28f, glass.Bottom);
                g.DrawLine(crack, glass.Left + glass.Width * 0.42f, glass.Top + glass.Height * 0.5f,
                    glass.Right - glass.Width * 0.1f, glass.Top + glass.Height * 0.7f);
            }
            // 内中小蘑菇
            float d = Math.Min(glass.Width, glass.Height) * 0.42f;
            DrawMushroom(g, new RectangleF(glass.Left + (glass.Width - d) / 2f,
                glass.Bottom - d - glass.Height * 0.06f, d, d));
        }
        // ----------------------------------------------------------------
        // 自锁标签式（STOP 底条）
        // ----------------------------------------------------------------
        private void DrawPushLock(Graphics g, RectangleF a)
        {
            DrawYellowPlate(g, a, 0.72f);
            float d = Math.Min(a.Width, a.Height) * 0.52f;
            DrawMushroom(g, new RectangleF(a.Left + (a.Width - d) / 2f, a.Top + a.Height * 0.06f, d, d));
            var tag = new RectangleF(a.Left + a.Width * 0.2f, a.Bottom - a.Height * 0.2f,
                a.Width * 0.6f, a.Height * 0.12f);
            using (var tb = new SolidBrush(_pressed ? CRedDk : Color.FromArgb(60, 60, 60)))
                g.FillRectangle(tb, tag);
        }
        // ----------------------------------------------------------------
        // 无线急停（蘑菇头 + 天线波纹）
        // ----------------------------------------------------------------
        private void DrawWireless(Graphics g, RectangleF a)
        {
            DrawYellowPlate(g, a, 0.74f);
            float d = Math.Min(a.Width, a.Height) * 0.5f;
            DrawMushroom(g, new RectangleF(a.Left + (a.Width - d) / 2f,
                a.Top + a.Height * 0.16f, d, d));
            using (var ant = new Pen(CMetalDk, Math.Max(1f, a.Width * 0.02f)))
            {
                float x1 = a.Right - a.Width * 0.2f, y1 = a.Top + a.Height * 0.3f;
                g.DrawLine(ant, x1, y1, x1 + a.Width * 0.12f, a.Top + a.Height * 0.08f);
                using (var wave = new Pen(Color.FromArgb(120, CRedDk), 1.4f))
                    for (int k = 0; k < 2; k++)
                        g.DrawArc(wave, x1 - 2f - k * 3f, a.Top + a.Height * 0.04f - k * 3f,
                            10f + k * 6f, 10f + k * 6f, -60f, 120f);
            }
        }
        // ----------------------------------------------------------------
        // 钥匙复位（蘑菇头 + 钥匙孔）
        // ----------------------------------------------------------------
        private void DrawKeyReset(Graphics g, RectangleF a)
        {
            DrawYellowPlate(g, a, 0.82f);
            float d = Math.Min(a.Width, a.Height) * 0.5f;
            DrawMushroom(g, new RectangleF(a.Left + (a.Width - d) / 2f, a.Top + a.Height * 0.04f, d, d));
            // 钥匙孔
            float cx = a.Left + a.Width / 2f, ky = a.Bottom - a.Height * 0.16f;
            using (var kb = new SolidBrush(CRedDk))
            {
                g.FillEllipse(kb, cx - a.Width * 0.07f, ky - a.Width * 0.07f, a.Width * 0.14f, a.Width * 0.14f);
                g.FillRectangle(kb, cx - a.Width * 0.022f, ky, a.Width * 0.044f, a.Width * 0.1f);
            }
        }
    }
}