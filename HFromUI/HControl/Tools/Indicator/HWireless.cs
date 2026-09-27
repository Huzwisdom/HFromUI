using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HAttribute;
namespace HFromUI.HControl.Tools.Indicator
{
    using HFromUI.HControl.Tools.Shared;
    using HFromUI.HControl.Base;
    /// <summary>
    /// 无线信号样式：Wifi=0、Signal=1 为已验收经典外形，其余 20 种覆盖蜂窝塔、
    /// 雷达、卫星、八木天线、路由器、AP、对讲机、相控阵、激光雷达、红外对射等通信外形。
    /// </summary>
    public enum HWirelessStyle
    {
        Wifi = 0,              // 经典 Wifi 热点
        Signal = 1,            // 经典信号发射塔
        CellTower = 2,         // 蜂窝通信塔
        RadarDish = 3,         // 抛物面雷达
        Satellite = 4,         // 人造卫星
        Yagi = 5,              // 八木天线
        Router = 6,            // 工业路由器
        AccessPoint = 7,       // 吸顶无线 AP
        WalkieTalkie = 8,      // 手持对讲机
        BroadcastTower = 9,    // 广播电视塔
        MicrowaveLink = 10,    // 微波接力站
        PhasedArray = 11,      // 相控阵雷达
        Beacon = 12,           // 蓝牙信标
        GpsAntenna = 13,       // GPS 接收天线
        Repeater = 14,         // 直放中继站
        SmallCell = 15,        // 小微基站
        WhipRadio = 16,        // 鞭天线电台
        SonarBuoy = 17,        // 水声浮标
        Lidar = 18,            // 激光雷达
        IrBeam = 19,           // 红外对射
        MeshNode = 20,         // Mesh 节点
        Radome = 21            // 球形雷达罩
    }
    /// <summary>
    /// 无线信号控件（继承 HToolAnimBase）：22 种通信外形、13 种色调，
    /// Transmitting 时信号弧呼吸闪烁、波环扩散、雷达波束旋转或数据包飞行。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("无线信号控件：22 种通信外形、13 种色调，传输时信号弧闪烁波环扩散")]
    public class HWireless : HToolAnimBase
    {
        private HWirelessStyle _style = HWirelessStyle.Wifi;
        private readonly HToolPalette _classic = HToolPalettes.Get((int)HToolTheme.Graphite);
        protected override HToolPalette ClassicPalette => _classic;
        public HWireless()
        {
            Size = new Size(120, 110);
        }
        /// <summary>信号外形样式。</summary>
        [HCategoryLanguage("无线模块"), HDisplayNameLanguage("信号外形样式"), HDescriptionLanguage("信号外形样式"), Browsable(true)]
        [DefaultValue(HWirelessStyle.Wifi)]
        public HWirelessStyle WirelessStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>是否正在传输，传输时信号弧闪烁、波环扩散。</summary>
        [HCategoryLanguage("无线模块"), HDisplayNameLanguage("是否正在传输信号"), HDescriptionLanguage("是否正在传输信号"), Browsable(true)]
        [DefaultValue(false)]
        public bool Transmitting
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
                case HWirelessStyle.CellTower: DrawCellTower(g, a, pal); break;
                case HWirelessStyle.RadarDish: DrawRadarDish(g, a, pal); break;
                case HWirelessStyle.Satellite: DrawSatellite(g, a, pal); break;
                case HWirelessStyle.Yagi: DrawYagi(g, a, pal); break;
                case HWirelessStyle.Router: DrawRouter(g, a, pal); break;
                case HWirelessStyle.AccessPoint: DrawAccessPoint(g, a, pal); break;
                case HWirelessStyle.WalkieTalkie: DrawWalkie(g, a, pal); break;
                case HWirelessStyle.BroadcastTower: DrawBroadcast(g, a, pal); break;
                case HWirelessStyle.MicrowaveLink: DrawMicrowave(g, a, pal); break;
                case HWirelessStyle.PhasedArray: DrawPhased(g, a, pal); break;
                case HWirelessStyle.Beacon: DrawBeacon(g, a, pal); break;
                case HWirelessStyle.GpsAntenna: DrawGps(g, a, pal); break;
                case HWirelessStyle.Repeater: DrawRepeater(g, a, pal); break;
                case HWirelessStyle.SmallCell: DrawSmallCell(g, a, pal); break;
                case HWirelessStyle.WhipRadio: DrawWhip(g, a, pal); break;
                case HWirelessStyle.SonarBuoy: DrawSonar(g, a, pal); break;
                case HWirelessStyle.Lidar: DrawLidar(g, a, pal); break;
                case HWirelessStyle.IrBeam: DrawIrBeam(g, a, pal); break;
                case HWirelessStyle.MeshNode: DrawMesh(g, a, pal); break;
                case HWirelessStyle.Radome: DrawRadome(g, a, pal); break;
                default: DrawClassic(g, a); break;
            }
            PaintPlacedText(g);
        }
        // ----------------------------------------------------------------
        // Classic：Wifi 热点 / 信号发射塔（已验收绘法，原样保留）
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF a)
        {
            var st0 = g.Save();
            g.TranslateTransform(a.Left, a.Top);
            float Width = a.Width;
            float glyphH = a.Height;
            Color wave = Color.FromArgb(36, 120, 220);
            if (_style == HWirelessStyle.Wifi)
            {
                float cx = Width / 2f, baseY = glyphH * 0.86f;
                // 三层信号弧
                for (int i = 0; i < 3; i++)
                {
                    float rr = glyphH * (0.16f + i * 0.16f);
                    int alpha = Running
                        ? (int)(110 + 120 * Math.Abs(Math.Sin(Phase + i * 0.9)))
                        : 150;
                    using (var pen = new Pen(Color.FromArgb(alpha, wave), 4.5f)
                    { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawArc(pen, cx - rr, baseY - rr * 1.05f, rr * 2f, rr * 2f, 210f, 120f);
                }
                using (var dot = new SolidBrush(wave))
                    g.FillEllipse(dot, cx - 6.5f, baseY - 6.5f, 13f, 13f);
                if (Running)
                {
                    float t = Phase / (float)Math.PI * 0.5f;
                    float rr = glyphH * (0.14f + t * 0.42f);
                    using (var ring = new Pen(Color.FromArgb((int)(160 * (1f - t)), wave), 1.6f))
                        g.DrawArc(ring, cx - rr, baseY - rr * 1.05f, rr * 2f, rr * 2f, 210f, 120f);
                }
            }
            else
            {
                float cx = Width / 2f, top = glyphH * 0.12f, bot = glyphH * 0.86f;
                float half = Width * 0.22f;
                // 塔桅
                using (var mast = new Pen(Color.FromArgb(64, 68, 74), 3f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    g.DrawLine(mast, cx - half, bot, cx, top);
                    g.DrawLine(mast, cx + half, bot, cx, top);
                    g.DrawLine(mast, cx - half * 0.62f, bot - (bot - top) * 0.36f,
                        cx + half * 0.62f, bot - (bot - top) * 0.36f);
                    g.DrawLine(mast, cx - half * 0.34f, bot - (bot - top) * 0.68f,
                        cx + half * 0.34f, bot - (bot - top) * 0.68f);
                }
                using (var baseB = new SolidBrush(Color.FromArgb(64, 68, 74)))
                    g.FillRectangle(baseB, cx - half - 4f, bot - 3f, half * 2f + 8f, 6f);
                // 双侧信号弧
                for (int i = 0; i < 2; i++)
                {
                    float rr = glyphH * (0.14f + i * 0.12f);
                    int alpha = Running
                        ? (int)(110 + 120 * Math.Abs(Math.Sin(Phase * 1.4 + i)))
                        : 150;
                    using (var pen = new Pen(Color.FromArgb(alpha, wave), 3.6f)
                    { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawArc(pen, cx - rr, top - rr * 0.1f, rr * 2f, rr * 2f, 215f, 55f);
                        g.DrawArc(pen, cx - rr, top - rr * 0.1f, rr * 2f, rr * 2f, -90f, 55f);
                    }
                }
                // 传输中的数据包
                if (Running)
                {
                    float t = Phase / (float)Math.PI * 0.5f;
                    using (var pkt = new SolidBrush(Color.FromArgb((int)(240 * (1f - t * 0.5f)), wave)))
                    {
                        double ang = (215 - t * 40) * Math.PI / 180.0;
                        float rr = glyphH * 0.28f;
                        g.FillEllipse(pkt, cx + (float)Math.Cos(ang) * rr - 2.5f,
                            top - rr * 0.1f + (float)Math.Sin(ang) * rr - 2.5f, 5f, 5f);
                    }
                }
            }
            g.Restore(st0);
        }
        // ----------------------------------------------------------------
        // 2 蜂窝通信塔：格构塔桅 + 三扇区板状天线 + 航标灯
        // ----------------------------------------------------------------
        private void DrawCellTower(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.12f, bot = a.Bottom - 8f;
            Lattice(g, cx, top + 12f, bot, a.Width * 0.44f, 4f, 3, pal.MetalDark);
            // 顶部抱杆 + 三扇区天线
            g.Line(pal.Edge, 2.4f, cx, top, cx, top + 14f);
            for (int i = -1; i <= 1; i++)
                g.Box(new RectangleF(cx + i * 7f - 2.2f, top + 2f, 4.4f, 15f), 2f,
                    HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.1f);
            Blinker(g, cx, top - 3f, Running, Phase);
            Waves(g, cx, top + 9f, a.Height * 0.17f, a.Height * 0.085f, 2,
                215f, 90f, 3.4f, pal.Accent);
            if (Running) Ring(g, cx, top + 9f, a.Height * 0.12f, a.Height * 0.42f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 3 抛物面雷达：锅面 + 馈源三脚支架，传输时锅口探测弧扩散
        // ----------------------------------------------------------------
        private void DrawRadarDish(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float dy = a.Top + a.Height * 0.64f, dr = a.Width * 0.3f;
            // 立柱 + 底座
            g.Line(pal.MetalDark, 4f, cx, dy, cx, a.Bottom - 8f);
            g.BasePlate(cx, a.Width * 0.42f, a.Bottom - 9f, 7f, pal.MetalDark);
            // 锅面（∪ 形下半盘 + 口径线）
            var dishR = new RectangleF(cx - dr, dy - dr * 0.6f, dr * 2f, dr * 1.2f);
            using (var b = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.16f)))
                g.FillPie(b, dishR.X, dishR.Y, dishR.Width, dishR.Height, 0f, 180f);
            using (var p = new Pen(pal.Edge, 1.5f))
            {
                g.DrawArc(p, dishR.X, dishR.Y, dishR.Width, dishR.Height, 0f, 180f);
                g.DrawLine(p, cx - dr, dy, cx + dr, dy);
            }
            // 馈源 + 支架
            float fy = dy - dr * 0.72f;
            g.Line(pal.Edge, 1.6f, cx - dr * 0.3f, dy, cx, fy);
            g.Line(pal.Edge, 1.6f, cx + dr * 0.3f, dy, cx, fy);
            g.Ell(cx, fy, 4.5f, 3.2f, pal.Metal);
            Waves(g, cx, fy, a.Height * 0.1f, a.Height * 0.075f, 3,
                210f, 120f, 3f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 4 人造卫星：本体 + 双太阳能帆板 + 小碟天线
        // ----------------------------------------------------------------
        private void DrawSatellite(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.55f;
            float bw = a.Width * 0.2f, bh = a.Height * 0.2f;
            float pw = a.Width * 0.23f, ph = bh * 0.82f, py = cy - ph / 2f;
            // 帆板连接臂
            g.Line(pal.MetalDark, 2.2f, cx - bw / 2f, cy, a.Left + a.Width * 0.06f + pw, cy);
            g.Line(pal.MetalDark, 2.2f, cx + bw / 2f, cy, a.Right - a.Width * 0.06f - pw, cy);
            // 双太阳能帆板（分格）
            foreach (float px in new[] { a.Left + a.Width * 0.06f, a.Right - a.Width * 0.06f - pw })
            {
                g.Box(new RectangleF(px, py, pw, ph), 2f,
                    HToolPalettes.Shade(pal.Main, 0.05f), pal.Edge, 1.2f);
                g.Line(pal.Edge, 1f, px + pw / 3f, py + 2f, px + pw / 3f, py + ph - 2f);
                g.Line(pal.Edge, 1f, px + pw * 2f / 3f, py + 2f, px + pw * 2f / 3f, py + ph - 2f);
            }
            // 本体 + 金色蒙皮条
            g.Box(new RectangleF(cx - bw / 2f, cy - bh / 2f, bw, bh), 3f, pal.Light, pal.Edge, 1.4f);
            g.Line(pal.Accent, 2.4f, cx - bw / 2f + 2f, cy, cx + bw / 2f - 2f, cy);
            // 顶部小碟
            g.Line(pal.Edge, 1.6f, cx, cy - bh / 2f, cx, cy - bh / 2f - 8f);
            using (var p = new Pen(pal.Edge, 1.5f))
                g.DrawArc(p, cx - 6f, cy - bh / 2f - 14f, 12f, 9f, 0f, 180f);
            g.Ell(cx, cy - bh / 2f - 15f, 2f, 2f, pal.MetalDark);
            Blinker(g, cx - bw / 2f + 4f, cy + bh / 2f - 4f, Running, Phase);
        }
        // ----------------------------------------------------------------
        // 5 八木天线：主梁 + 渐变引向器 + 桅杆，信号向前端发射
        // ----------------------------------------------------------------
        private void DrawYagi(Graphics g, RectangleF a, HToolPalette pal)
        {
            float x1 = a.Left + a.Width * 0.14f, x2 = a.Right - a.Width * 0.14f;
            float my = a.Top + a.Height * 0.34f, cx = a.Left + a.Width / 2f;
            // 桅杆
            g.Line(pal.MetalDark, 3.4f, cx, my, cx, a.Bottom - 8f);
            g.BasePlate(cx, a.Width * 0.3f, a.Bottom - 9f, 6f, pal.MetalDark);
            // 主梁
            g.Line(pal.Edge, 3f, x1, my, x2 + 6f, my);
            // 引向器/反射器（后端最长，向前渐短）
            float[] lens = { 26f, 22f, 18f, 14f, 10f };
            for (int i = 0; i < lens.Length; i++)
            {
                float ex = x2 - i * (x2 - x1) * 0.2f;
                g.Line(HToolPalettes.Shade(pal.Main, 0.2f), 2.2f, ex, my - lens[i] / 2f, ex, my + lens[i] / 2f);
            }
            // 折叠振子环
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 1.8f))
                g.DrawEllipse(p, x1 - 3f, my - 7f, 6f, 14f);
            Waves(g, x2 + 4f, my, a.Height * 0.1f, a.Height * 0.07f, 2,
                -55f, 110f, 3f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 6 工业路由器：圆角盒体 + 三天线 + 状态灯
        // ----------------------------------------------------------------
        private void DrawRouter(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            var body = new RectangleF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.52f,
                a.Width * 0.68f, a.Height * 0.26f);
            // 三根天线
            for (int i = -1; i <= 1; i++)
            {
                float ax = cx + i * body.Width * 0.28f;
                float tipX = ax + i * 5f, tipY = body.Top - a.Height * 0.22f;
                g.Line(pal.MetalDark, 2.8f, ax, body.Top, tipX, tipY);
                g.Ell(tipX, tipY, 2.4f, 2.4f, pal.MetalDark);
            }
            g.Box(body, body.Height * 0.22f, pal.Light, pal.Edge, 1.4f);
            // 状态灯组
            for (int i = 0; i < 3; i++)
                g.Lamp(body.Left + 9f + i * 9f, body.Bottom - 7f, Running);
            Waves(g, cx, body.Top - a.Height * 0.2f, a.Height * 0.09f, a.Height * 0.06f, 2,
                215f, 110f, 2.8f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 7 吸顶无线 AP：顶管 + 扁圆盘体，信号向下覆盖
        // ----------------------------------------------------------------
        private void DrawAccessPoint(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            // 吊顶杆 + 安装盘
            g.Line(pal.MetalDark, 3f, cx, a.Top + 2f, cx, a.Top + a.Height * 0.18f);
            g.Ell(cx, a.Top + 3f, 7f, 3f, pal.MetalDark);
            float dy = a.Top + a.Height * 0.3f;
            // 扁圆盘体
            g.Ell(cx, dy, a.Width * 0.28f, a.Height * 0.08f, HToolPalettes.Shade(pal.Main, 0.08f));
            g.Box(new RectangleF(cx - a.Width * 0.28f, dy - a.Height * 0.05f,
                a.Width * 0.56f, a.Height * 0.1f), a.Height * 0.05f,
                HToolPalettes.Shade(pal.Main, 0.08f), pal.Edge, 1.3f);
            g.Ell(cx, dy + a.Height * 0.05f, 3.5f, 3.5f, Running ? pal.Accent : pal.MetalDark);
            // 向下覆盖弧
            Waves(g, cx, dy + a.Height * 0.08f, a.Height * 0.13f, a.Height * 0.09f, 2,
                35f, 110f, 3f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 8 手持对讲机：机身 + 长天线 + 喇叭孔 + PTT 键
        // ----------------------------------------------------------------
        private void DrawWalkie(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width * 0.52f;
            var body = new RectangleF(cx - a.Width * 0.17f, a.Top + a.Height * 0.26f,
                a.Width * 0.34f, a.Height * 0.56f);
            // 天线
            g.Line(pal.Edge, 2.8f, body.Right - 5f, body.Top,
                body.Right + 3f, a.Top + a.Height * 0.04f);
            g.Ell(body.Right + 3f, a.Top + a.Height * 0.04f, 2.4f, 2.4f, pal.Edge);
            // 机身 + 顶旋钮
            g.Box(body, 4f, HToolPalettes.Shade(pal.Main, 0.08f), pal.Edge, 1.4f);
            g.Box(new RectangleF(body.Left + 5f, body.Top - 5f, 9f, 6f), 2f, pal.MetalDark, pal.Edge, 1.1f);
            // 喇叭孔
            using (var hb = new SolidBrush(pal.Edge))
                for (int r = 0; r < 3; r++)
                    for (int c2 = 0; c2 < 2; c2++)
                        g.FillEllipse(hb, body.Left + 9f + c2 * 10f, body.Top + 12f + r * 8f, 4f, 4f);
            // PTT 侧键 + 状态灯
            g.Box(new RectangleF(body.Right - 2f, body.Top + body.Height * 0.34f, 5f, 12f),
                2f, HToolPalettes.Shade(pal.Main, 0.25f), pal.Edge, 1.1f);
            g.Lamp(body.Left + 8f, body.Bottom - 9f, Running);
            Waves(g, body.Right + 3f, a.Top + a.Height * 0.06f, a.Height * 0.08f,
                a.Height * 0.055f, 2, -50f, 100f, 2.6f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 9 广播电视塔：细高格构桅 + 双层横担天线 + 航空闪灯
        // ----------------------------------------------------------------
        private void DrawBroadcast(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.06f, bot = a.Bottom - 8f;
            Lattice(g, cx, top + 14f, bot, a.Width * 0.36f, 2.5f, 5, pal.MetalDark);
            // 顶针
            g.Line(pal.Edge, 2.2f, cx, top, cx, top + 14f);
            // 双层横担 + 端头吊灯
            foreach (var fy in new[] { 0.34f, 0.52f })
            {
                float y = a.Top + a.Height * fy, hw = a.Width * (fy > 0.4f ? 0.3f : 0.22f);
                g.Line(pal.MetalDark, 2.4f, cx - hw, y, cx + hw, y);
                g.Line(pal.MetalDark, 1.4f, cx - hw, y, cx - hw + 5f, y - 7f);
                g.Line(pal.MetalDark, 1.4f, cx + hw, y, cx + hw - 5f, y - 7f);
                Blinker(g, cx - hw, y - 7f, Running, Phase);
                Blinker(g, cx + hw, y - 7f, Running, Phase);
            }
            Blinker(g, cx, top - 3f, Running, Phase);
            if (Running) Ring(g, cx, top + 10f, a.Height * 0.1f, a.Height * 0.36f, Color.FromArgb(225, 60, 50));
        }
        // ----------------------------------------------------------------
        // 10 微波接力站：短塔 + 顶部双向小口径微波锅
        // ----------------------------------------------------------------
        private void DrawMicrowave(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float top = a.Top + a.Height * 0.22f, bot = a.Bottom - 8f;
            Lattice(g, cx, top, bot, a.Width * 0.3f, 5f, 2, pal.MetalDark);
            // 横担 + 双向锅（椭圆锅盖 + 背馈）
            float hw = a.Width * 0.3f;
            g.Line(pal.Edge, 2.6f, cx - hw, top, cx + hw, top);
            foreach (int dir in new[] { -1, 1 })
            {
                float dx = cx + dir * hw;
                g.Ell(dx, top, 8f, 10f, HToolPalettes.Shade(pal.Main, 0.12f));
                using (var p = new Pen(pal.Edge, 1.3f))
                    g.DrawEllipse(p, dx - 8f, top - 10f, 16f, 20f);
                g.Ell(dx + dir * 8f, top, 2.4f, 2.4f, pal.MetalDark);
                Waves(g, dx + dir * 4f, top, a.Height * 0.09f, a.Height * 0.06f, 2,
                    dir < 0 ? 125f : -55f, 110f, 2.8f, pal.Accent);
            }
        }
        // ----------------------------------------------------------------
        // 11 相控阵雷达：倾斜阵面 + 阵元网格 + 旋转扫描波束
        // ----------------------------------------------------------------
        private void DrawPhased(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.42f;
            // 底座 + 立柱
            g.Line(pal.MetalDark, 4f, cx, cy + a.Height * 0.12f, cx, a.Bottom - 8f);
            g.BasePlate(cx, a.Width * 0.4f, a.Bottom - 9f, 7f, pal.MetalDark);
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(-10f);
            // 阵面
            var panel = new RectangleF(-a.Width * 0.28f, -a.Height * 0.2f,
                a.Width * 0.56f, a.Height * 0.36f);
            g.Box(panel, 3f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.4f);
            using (var b = new SolidBrush(HToolPalettes.Tint(pal.Main, 0.3f)))
                for (int r = 0; r < 3; r++)
                    for (int c2 = 0; c2 < 6; c2++)
                        g.FillRectangle(b, -panel.Width * 0.4f + c2 * panel.Width * 0.16f,
                            -panel.Height * 0.28f + r * panel.Height * 0.28f,
                            panel.Width * 0.08f, panel.Height * 0.12f);
            g.Restore(st);
            // 扫描扇面绕阵面中心旋转
            if (Running) Sweep(g, cx, cy, a.Width * 0.42f, SpinAngle, pal.Accent);
            g.Lamp(cx + a.Width * 0.2f, a.Bottom - 14f, Running);
        }
        // ----------------------------------------------------------------
        // 12 蓝牙信标：小圆盒 + 短促呼吸波
        // ----------------------------------------------------------------
        private void DrawBeacon(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.62f;
            float rw = a.Width * 0.2f, rh = a.Height * 0.12f;
            g.Box(new RectangleF(cx - rw, cy - rh, rw * 2f, rh * 2f), rh * 0.8f,
                pal.Light, pal.Edge, 1.4f);
            g.Ell(cx, cy - rh * 0.2f, rw * 0.82f, rh * 0.5f, HToolPalettes.Tint(pal.Main, 0.15f));
            g.Lamp(cx, cy + rh * 0.35f, Running);
            // 顶部短弧 + 中号弧
            Waves(g, cx, cy - rh, a.Height * 0.1f, a.Height * 0.07f, 2,
                220f, 100f, 2.8f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 13 GPS 接收天线：蘑菇头 + 短杆
        // ----------------------------------------------------------------
        private void DrawGps(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float dy = a.Top + a.Height * 0.46f, rr = a.Width * 0.26f;
            // 短杆 + 底座
            g.Line(pal.MetalDark, 3.4f, cx, dy + rr * 0.55f, cx, a.Bottom - 8f);
            g.BasePlate(cx, a.Width * 0.32f, a.Bottom - 9f, 6f, pal.MetalDark);
            // 蘑菇头：∩ 形罩 + 底盘沿
            using (var b = new SolidBrush(HToolPalettes.Shade(pal.Main, 0.06f)))
                g.FillPie(b, cx - rr, dy - rr, rr * 2f, rr * 2f, 180f, 180f);
            using (var p = new Pen(pal.Edge, 1.4f))
            {
                g.DrawArc(p, cx - rr, dy - rr, rr * 2f, rr * 2f, 180f, 180f);
                g.DrawEllipse(p, cx - rr, dy - 3f, rr * 2f, 6f);
            }
            Waves(g, cx, dy - rr, a.Height * 0.09f, a.Height * 0.06f, 2,
                215f, 110f, 2.8f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 14 直放中继站：机柜 + 双天线
        // ----------------------------------------------------------------
        private void DrawRepeater(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            var cab = new RectangleF(a.Left + a.Width * 0.24f, a.Top + a.Height * 0.34f,
                a.Width * 0.52f, a.Height * 0.5f);
            // 双天线
            for (int i = 0; i < 2; i++)
            {
                float ax = cab.Left + cab.Width * (0.25f + i * 0.5f);
                g.Line(pal.MetalDark, 2.6f, ax, cab.Top, ax + (i == 0 ? -4f : 4f), a.Top + a.Height * 0.08f);
            }
            g.Box(cab, 3f, HToolPalettes.Shade(pal.Main, 0.08f), pal.Edge, 1.4f);
            // 柜门缝 + 散热孔
            g.Line(pal.Edge, 1.1f, cab.Left + cab.Width / 2f, cab.Top + 4f,
                cab.Left + cab.Width / 2f, cab.Bottom - 4f);
            for (int r = 0; r < 4; r++)
                g.Line(pal.Edge, 1f, cab.Right - 12f, cab.Top + 10f + r * 6f,
                    cab.Right - 5f, cab.Top + 10f + r * 6f);
            g.Lamp(cab.Left + 8f, cab.Bottom - 9f, Running);
            Waves(g, cx, a.Top + a.Height * 0.1f, a.Height * 0.1f, a.Height * 0.06f, 2,
                215f, 110f, 2.8f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 15 小微基站：短杆 + 圆柱全向天线头
        // ----------------------------------------------------------------
        private void DrawSmallCell(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float hy = a.Top + a.Height * 0.3f, hh = a.Height * 0.26f, hw = a.Width * 0.1f;
            // 立杆
            g.Line(pal.MetalDark, 4f, cx, hy + hh, cx, a.Bottom - 8f);
            g.BasePlate(cx, a.Width * 0.32f, a.Bottom - 9f, 6f, pal.MetalDark);
            // 圆柱头
            g.Box(new RectangleF(cx - hw, hy, hw * 2f, hh), 4f,
                HToolPalettes.Shade(pal.Main, 0.08f), pal.Edge, 1.3f);
            g.Ell(cx, hy, hw, 4f, HToolPalettes.Tint(pal.Main, 0.2f));
            g.Line(pal.Accent, 2f, cx, hy + 5f, cx, hy + hh - 5f);
            // 全向波环
            for (int i = 0; i < 2; i++)
            {
                float rr = a.Height * (0.16f + i * 0.1f);
                int alpha = Running ? (int)(90 + 110 * Math.Abs(Math.Sin(Phase * 1.5f + i))) : 80;
                using (var p = new Pen(Color.FromArgb(alpha, pal.Accent), 2.4f))
                    g.DrawEllipse(p, cx - rr, hy + hh * 0.5f - rr, rr * 2f, rr * 2f);
            }
        }
        // ----------------------------------------------------------------
        // 16 鞭天线电台：底座 + 长鞭（发射时轻摆）
        // ----------------------------------------------------------------
        private void DrawWhip(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, by = a.Bottom - a.Height * 0.2f;
            var box = new RectangleF(cx - a.Width * 0.18f, by, a.Width * 0.36f, a.Height * 0.16f);
            g.Box(box, 3f, HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.3f);
            g.Ell(box.Right - 7f, box.Top + box.Height / 2f, 3f, 3f, Running ? pal.Accent : pal.MetalDark);
            // 长鞭（摆动用贝塞尔曲线）
            float tipX = cx + (Running ? (float)Math.Sin(Phase * 2f) * 6f : 0f);
            float tipY = a.Top + a.Height * 0.06f;
            using (var p = new Pen(pal.MetalDark, 2.4f) { StartCap = LineCap.Round })
                g.DrawBezier(p, cx, box.Top, cx - 6f, box.Top - a.Height * 0.12f,
                    tipX + 4f, tipY + a.Height * 0.1f, tipX, tipY);
            g.Ell(tipX, tipY, 2.2f, 2.2f, pal.MetalDark);
            Waves(g, tipX, tipY, a.Height * 0.07f, a.Height * 0.05f, 2,
                215f, 110f, 2.6f, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 17 水声浮标：水面浮标 + 水下同心波环
        // ----------------------------------------------------------------
        private void DrawSonar(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, wy = a.Top + a.Height * 0.52f;
            // 水面线
            using (var p = new Pen(HToolPalettes.Tint(pal.Main, 0.4f), 1.8f)
            { DashStyle = DashStyle.Dash })
                g.DrawLine(p, a.Left + 4f, wy, a.Right - 4f, wy);
            // 浮标球体 + 顶旗杆 + 标灯
            g.Ell(cx, wy - 2f, a.Width * 0.12f, a.Width * 0.12f,
                HToolPalettes.Shade(pal.Main, 0.08f));
            using (var p = new Pen(pal.Edge, 1.3f))
                g.DrawEllipse(p, cx - a.Width * 0.12f, wy - 2f - a.Width * 0.12f,
                    a.Width * 0.24f, a.Width * 0.24f);
            g.Line(pal.Edge, 1.8f, cx, wy - a.Width * 0.12f - 2f, cx, wy - a.Width * 0.24f);
            Blinker(g, cx, wy - a.Width * 0.24f - 3f, Running, Phase);
            // 水下波环（椭圆扩散）
            for (int i = 0; i < 2; i++)
            {
                float t = Running
                    ? (Phase / (float)(Math.PI * 2.0) + i * 0.5f) % 1f
                    : i * 0.3f + 0.2f;
                float rx = a.Width * (0.1f + t * 0.32f), ry = rx * 0.5f;
                int alpha = (int)((Running ? 170 : 90) * (1f - t));
                using (var p = new Pen(Color.FromArgb(alpha, pal.Accent), 1.6f))
                    g.DrawEllipse(p, cx - rx, wy - ry, rx * 2f, ry * 2f);
            }
        }
        // ----------------------------------------------------------------
        // 18 激光雷达：矮圆柱 + 旋转顶盖 + 扫描扇面
        // ----------------------------------------------------------------
        private void DrawLidar(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.56f;
            float rw = a.Width * 0.22f, bh = a.Height * 0.2f;
            g.BasePlate(cx, a.Width * 0.4f, a.Bottom - 9f, 7f, pal.MetalDark);
            g.Box(new RectangleF(cx - rw, cy - bh / 2f, rw * 2f, bh), 4f,
                HToolPalettes.Shade(pal.Main, 0.1f), pal.Edge, 1.4f);
            // 旋转顶盖
            g.Ell(cx, cy - bh / 2f, rw, 5f, HToolPalettes.Tint(pal.Main, 0.15f));
            using (var p = new Pen(pal.Edge, 1.2f))
                g.DrawEllipse(p, cx - rw, cy - bh / 2f - 5f, rw * 2f, 10f);
            float ra = SpinAngle * (float)Math.PI / 180f;
            g.Line(pal.Accent, 2f, cx, cy - bh / 2f,
                cx + (float)Math.Cos(ra) * rw * 0.8f, cy - bh / 2f + (float)Math.Sin(ra) * 4f);
            if (Running) Sweep(g, cx, cy, a.Width * 0.4f, SpinAngle, pal.Accent);
        }
        // ----------------------------------------------------------------
        // 19 红外对射：两端收发器 + 虚线光束
        // ----------------------------------------------------------------
        private void DrawIrBeam(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.55f;
            float lw = a.Width * 0.12f, lh = a.Height * 0.34f;
            float lx = a.Left + a.Width * 0.08f, rx = a.Right - a.Width * 0.08f - lw;
            foreach (float px in new[] { lx, rx })
            {
                // 立杆 + 盒体
                g.Line(pal.MetalDark, 3f, px + lw / 2f, cy + lh / 2f, px + lw / 2f, a.Bottom - 8f);
                g.Box(new RectangleF(px, cy - lh / 2f, lw, lh), 3f,
                    HToolPalettes.Shade(pal.Main, 0.08f), pal.Edge, 1.3f);
                // 内向透镜
                float lensX = px == lx ? px + lw - 3f : px + 3f;
                g.Ell(lensX, cy, 4f, 7f, Running ? pal.Accent : pal.MetalDark);
                g.Lamp(px + lw / 2f, cy + lh / 2f - 6f, Running);
            }
            // 虚线光束
            using (var beam = new Pen(Color.FromArgb(Running ? 200 : 80, pal.Accent), 2f)
            { DashStyle = DashStyle.Dash })
            {
                if (Running) beam.DashOffset = Phase * 2f % 8f;
                g.DrawLine(beam, lx + lw, cy, rx, cy);
            }
        }
        // ----------------------------------------------------------------
        // 20 Mesh 节点：中心六边形 + 三个邻节点链路，传输时数据脉冲沿线飞行
        // ----------------------------------------------------------------
        private void DrawMesh(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f, cy = a.Top + a.Height * 0.52f;
            var peers = new[]
            {
                new PointF(a.Left + a.Width * 0.16f, a.Top + a.Height * 0.22f),
                new PointF(a.Right - a.Width * 0.16f, a.Top + a.Height * 0.22f),
                new PointF(cx, a.Bottom - a.Height * 0.16f)
            };
            // 链路
            foreach (var p in peers)
            {
                g.Line(pal.MetalDark, 1.8f, cx, cy, p.X, p.Y);
                if (Running) Pulse(g, cx, cy, p.X, p.Y, pal.Accent);
                HexNode(g, p.X, p.Y, 7f, HToolPalettes.Shade(pal.Main, 0.05f), pal.Edge);
            }
            // 中心节点
            HexNode(g, cx, cy, 12f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge);
            g.Ell(cx, cy, 3.5f, 3.5f, Running ? pal.Accent : pal.MetalDark);
        }
        // ----------------------------------------------------------------
        // 21 球形雷达罩：机房底座 + 白球罩 + 内部隐约扫描线
        // ----------------------------------------------------------------
        private void DrawRadome(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cx = a.Left + a.Width / 2f;
            float sr = a.Width * 0.3f, sy = a.Top + a.Height * 0.42f;
            // 机房底座 + 支腿
            var room = new RectangleF(cx - a.Width * 0.24f, sy + sr * 0.7f,
                a.Width * 0.48f, a.Height * 0.2f);
            g.Box(room, 2f, HToolPalettes.Shade(pal.Main, 0.12f), pal.Edge, 1.3f);
            g.Line(pal.Edge, 1.2f, room.Left + 8f, room.Bottom - 6f, room.Right - 8f, room.Bottom - 6f);
            // 球罩
            using (var b = new SolidBrush(Color.FromArgb(225, pal.Lighter)))
                g.FillEllipse(b, cx - sr, sy - sr, sr * 2f, sr * 2f);
            using (var p = new Pen(pal.GlassEdge, 1.5f))
            {
                g.DrawEllipse(p, cx - sr, sy - sr, sr * 2f, sr * 2f);
                g.DrawArc(p, cx - sr, sy - sr * 0.4f, sr * 2f, sr * 0.8f, 0f, 180f);
            }
            // 内部扫描线
            if (Running)
            {
                float ang = SpinAngle * (float)Math.PI / 180f;
                using (var p = new Pen(Color.FromArgb(120, pal.Accent), 1.6f))
                    g.DrawLine(p, cx, sy,
                        cx + (float)Math.Cos(ang) * sr * 0.8f,
                        sy + (float)Math.Sin(ang) * sr * 0.8f);
            }
            Blinker(g, cx + sr * 0.6f, sy - sr * 0.55f, Running, Phase);
        }
        // ----------------------------------------------------------------
        // 共享原语
        // ----------------------------------------------------------------
        /// <summary>扇形信号弧：n 层同心弧，传输时按相位呼吸闪烁。</summary>
        private void Waves(Graphics g, float cx, float cy, float r0, float dr, int n,
            float start, float sweep, float w, Color c)
        {
            for (int i = 0; i < n; i++)
            {
                float rr = r0 + dr * i;
                int alpha = Running
                    ? (int)(110 + 120 * Math.Abs(Math.Sin(Phase + i * 0.9)))
                    : 150;
                using (var pen = new Pen(Color.FromArgb(alpha, c), w)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawArc(pen, cx - rr, cy - rr, rr * 2f, rr * 2f, start, sweep);
            }
        }
        /// <summary>扩散波环：单个圆环半径与透明度随相位变化。</summary>
        private void Ring(Graphics g, float cx, float cy, float r0, float r1, Color c)
        {
            float t = Phase / (2f * (float)Math.PI);
            float rr = r0 + t * (r1 - r0);
            using (var pen = new Pen(Color.FromArgb((int)(170 * (1f - t)), c), 1.6f))
                g.DrawEllipse(pen, cx - rr, cy - rr, rr * 2f, rr * 2f);
        }
        /// <summary>航空/状态闪灯：传输时红色呼吸，停止时灰色。</summary>
        private static void Blinker(Graphics g, float cx, float cy, bool run, float phase)
        {
            Color c = run
                ? Color.FromArgb((int)(120 + 120 * Math.Abs(Math.Sin(phase * 2f))), 225, 60, 50)
                : Color.FromArgb(120, 120, 120);
            g.Ell(cx, cy, 3.4f, 3.4f, c);
        }
        /// <summary>雷达扫描扇面：绕中心旋转的半透明楔形 + 亮边线。</summary>
        private static void Sweep(Graphics g, float cx, float cy, float r, float angleDeg, Color c)
        {
            var st = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angleDeg);
            using (var b = new SolidBrush(Color.FromArgb(46, c)))
                g.FillPie(b, -r, -r, r * 2f, r * 2f, -28f, 56f);
            using (var p = new Pen(Color.FromArgb(150, c), 1.6f))
                g.DrawLine(p, 0, 0, r, 0);
            g.Restore(st);
        }
        /// <summary>链路上飞行的数据脉冲。</summary>
        private void Pulse(Graphics g, float x1, float y1, float x2, float y2, Color c)
        {
            float t = Phase / (2f * (float)Math.PI);
            g.Ell(x1 + (x2 - x1) * t, y1 + (y2 - y1) * t, 3f, 3f, Color.FromArgb(220, c));
        }
        /// <summary>格构塔桅：两腿收敛 + 多层横撑与 X 形斜撑。</summary>
        private static void Lattice(Graphics g, float cx, float top, float bot,
            float wBot, float wTop, int levels, Color c)
        {
            using (var p = new Pen(c, 2.2f))
            {
                g.DrawLine(p, cx - wBot / 2f, bot, cx - wTop / 2f, top);
                g.DrawLine(p, cx + wBot / 2f, bot, cx + wTop / 2f, top);
                for (int i = 0; i < levels; i++)
                {
                    float t0 = (float)i / levels, t1 = (float)(i + 1) / levels;
                    float y0 = bot + (top - bot) * t0, y1 = bot + (top - bot) * t1;
                    float w0 = wBot + (wTop - wBot) * t0;
                    float w1 = wBot + (wTop - wBot) * t1;
                    g.DrawLine(p, cx - w0 / 2f, y0, cx + w0 / 2f, y0);
                    g.DrawLine(p, cx - w0 / 2f, y0, cx + w1 / 2f, y1);
                    g.DrawLine(p, cx + w0 / 2f, y0, cx - w1 / 2f, y1);
                }
                g.DrawLine(p, cx - wTop / 2f, top, cx + wTop / 2f, top);
            }
        }
        /// <summary>小型六边形节点。</summary>
        private static void HexNode(Graphics g, float cx, float cy, float r, Color fill, Color edge)
        {
            var pts = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double an = Math.PI / 6.0 + i * Math.PI / 3.0;
                pts[i] = new PointF(cx + (float)Math.Cos(an) * r, cy + (float)Math.Sin(an) * r);
            }
            g.Poly(pts, fill, edge, 1.3f);
        }
    }
}