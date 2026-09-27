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
    using HFromUI.HLangage;
    /// <summary>管内介质：水 / 气体。</summary>
    public enum HPipeKind
    {
        Water = 0,
        Gas = 1
    }
    /// <summary>
    /// 管路样式：Classic 为两端法兰直管，其余 21 种覆盖弯头、三通、四通、异径、
    /// U 型、盘管、软管、保温管、卡箍、补偿器、过滤器等常用管件外形。
    /// </summary>
    public enum HPipeStyle
    {
        Classic = 0,        // 法兰直管
        Elbow90 = 1,        // 90° 弯头
        Elbow45 = 2,        // 45° 弯头
        Tee = 3,            // T 型三通
        Cross = 4,          // 四通
        Reducer = 5,        // 异径管
        UBend = 6,          // U 型管
        Coil = 7,           // S 形盘管
        Hose = 8,           // 波纹软管
        Nipple = 9,         // 螺纹短管
        ValvePipe = 10,     // 带阀管段
        YBranch = 11,       // Y 型三通
        Blind = 12,         // 盲板管
        Insulated = 13,     // 保温管
        PVC = 14,           // PVC 粘接
        CastIron = 15,      // 承插铸铁管
        Clamp = 16,         // 快装卡箍
        FlexJoint = 17,     // 柔性接头
        Parallel = 18,      // 平行双管
        Spool = 19,         // 支架预制管段
        Expansion = 20,     // 波纹补偿器
        Filter = 21         // Y 型过滤器
    }
    /// <summary>
    /// 水管控件（继承 HToolAnimBase）：22 种管路外形、13 种色调，
    /// Flowing 时管内介质流动（水流纹/气泡）；支持横/纵方向。
    /// 气体管见 <see cref="HGasPipe"/>。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("水管管控件：22 种管路外形、13 种色调，流动时管内有水流/气泡动画")]
    public class HPipe : HToolAnimBase
    {
        /// <summary>介质类型（HGasPipe 重写为 Gas）。</summary>
        protected HPipeKind _kind = HPipeKind.Water;
        private HBarOrientation _orientation = HBarOrientation.Horizontal;
        private HPipeStyle _style = HPipeStyle.Classic;
        private readonly HToolPalette _classicWater = HToolPalettes.Get((int)HToolTheme.SkyBlue);
        protected override HToolPalette ClassicPalette =>
            _kind == HPipeKind.Gas ? HToolPalettes.Get((int)HToolTheme.Amber) : _classicWater;
        public HPipe() { Size = new Size(160, 66); }
        /// <summary>管内介质：水或气体。</summary>
        [HCategoryLanguage("管道"), HDisplayNameLanguage("管内介质"), HDescriptionLanguage("管内介质：水或气体"), Browsable(true)]
        [DefaultValue(HPipeKind.Water)]
        public HPipeKind Kind
        {
            get => _kind;
            set { _kind = value; Invalidate(); }
        }
        /// <summary>管路外形样式。</summary>
        [HCategoryLanguage("管道"), HDisplayNameLanguage("管路外形样式"), HDescriptionLanguage("管路外形样式"), Browsable(true)]
        [DefaultValue(HPipeStyle.Classic)]
        public HPipeStyle PipeStyle
        {
            get => _style;
            set { _style = value; Invalidate(); }
        }
        /// <summary>管道方向：横向或纵向。</summary>
        [HCategoryLanguage("管道"), HDisplayNameLanguage("管道方向"), HDescriptionLanguage("管道方向：横向或纵向"), Browsable(true)]
        [DefaultValue(HBarOrientation.Horizontal)]
        public HBarOrientation PipeLineStyle
        {
            get => _orientation;
            set { _orientation = value; Invalidate(); }
        }
        /// <summary>是否有介质流动，流动时管内有流向动画。</summary>
        [HCategoryLanguage("管道"), HDisplayNameLanguage("是否有介质流动"), HDescriptionLanguage("是否有介质流动，流动时管内有流向动画"), Browsable(true)]
        [DefaultValue(false)]
        public bool Flowing
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
            // 纵向显示时逻辑画布宽高互换；文字带在物理方位与逻辑方位间的映射：
            // 物理上↔逻辑左、物理下↔逻辑右、物理左↔逻辑下、物理右↔逻辑上
            bool vert = _orientation == HBarOrientation.Vertical;
            HTextPlacement logicalPlace = vert ? MapVerticalPlacement(TextPlacement) : TextPlacement;
            RectangleF area;
            if (vert)
            {
                // 物理图元区（文字带之外）与逻辑（旋转后）绘图区精确对应
                RectangleF phys = HBarBase.PlacedGlyphArea(Width, Height, Text, Font, TextPlacement);
                area = HBarBase.PlacedGlyphArea(Height, Width, Text, Font, logicalPlace);
                g.RotateTransform(90f);
                // (lx,ly) 旋转 90° 后平移到物理图元区
                g.TranslateTransform(phys.X + area.Height + area.Y, phys.Y - area.X);
            }
            else
                area = PlacedGlyphArea();
            var pal = Palette;
            switch (_style)
            {
                case HPipeStyle.Elbow90: DrawElbow(g, area, pal, 90); break;
                case HPipeStyle.Elbow45: DrawElbow(g, area, pal, 45); break;
                case HPipeStyle.Tee: DrawBranch(g, area, pal, 3); break;
                case HPipeStyle.Cross: DrawBranch(g, area, pal, 4); break;
                case HPipeStyle.Reducer: DrawReducer(g, area, pal); break;
                case HPipeStyle.UBend: DrawUBend(g, area, pal); break;
                case HPipeStyle.Coil: DrawCoil(g, area, pal); break;
                case HPipeStyle.Hose: DrawHose(g, area, pal, false); break;
                case HPipeStyle.FlexJoint: DrawHose(g, area, pal, true); break;
                case HPipeStyle.Nipple: DrawNipple(g, area, pal); break;
                case HPipeStyle.ValvePipe: DrawValvePipe(g, area, pal); break;
                case HPipeStyle.YBranch: DrawY(g, area, pal); break;
                case HPipeStyle.Blind: DrawBlind(g, area, pal); break;
                case HPipeStyle.Insulated: DrawInsulated(g, area, pal); break;
                case HPipeStyle.PVC: DrawPVC(g, area, pal, false); break;
                case HPipeStyle.CastIron: DrawPVC(g, area, pal, true); break;
                case HPipeStyle.Clamp: DrawClamp(g, area, pal); break;
                case HPipeStyle.Parallel: DrawParallel(g, area, pal); break;
                case HPipeStyle.Spool: DrawSpool(g, area, pal); break;
                case HPipeStyle.Expansion: DrawExpansion(g, area, pal); break;
                case HPipeStyle.Filter: DrawFilter(g, area, pal); break;
                default: DrawClassic(g, area, pal); break;
            }
            if (vert) g.ResetTransform();
            // 文字始终在物理坐标系绘制，保证横排/竖排方向不随管路旋转
            HBarBase.PaintPlacedText(g, Width, Height, Text, Font, ForeColor, TextPlacement);
        }
        /// <summary>纵向显示时物理文字方位到逻辑（旋转后）方位的映射。</summary>
        private static HTextPlacement MapVerticalPlacement(HTextPlacement p)
        {
            switch (p)
            {
                case HTextPlacement.Top: return HTextPlacement.Left;
                case HTextPlacement.Bottom: return HTextPlacement.Right;
                case HTextPlacement.Left: return HTextPlacement.Bottom;
                case HTextPlacement.Right: return HTextPlacement.Top;
                default: return HTextPlacement.Middle;
            }
        }
        // ----------------------------------------------------------------
        // 共享：介质色 / 直管段（含介质腔与流动动画）/ 法兰 / 流向箭头
        // ----------------------------------------------------------------
        private Color Media => _kind == HPipeKind.Gas ? Color.FromArgb(255, 224, 130) : Palette.Main;
        private Color MediaDark => _kind == HPipeKind.Gas ? Color.FromArgb(224, 176, 60) : Palette.Dark;
        private void Lumen(Graphics g, RectangleF body, HToolPalette pal, float insetX = 5f)
        {
            var lumen = RectangleF.Inflate(body, -insetX, -body.Height * 0.22f);
            // 窄管/小尺寸内缩后内腔可能归零，此时省略介质绘制（渐变画刷不允许零尺寸矩形）
            if (lumen.Width <= 0f || lumen.Height <= 0f) return;
            using (var b = new LinearGradientBrush(lumen, MediaDark, Media, 90f))
                g.FillRectangle(b, lumen);
            if (!Running) return;
            float span = lumen.Width + 60f, off = (Phase / (float)Math.PI * 0.5f * span) % 60f;
            if (_kind == HPipeKind.Gas)
            {
                using (var bub = new SolidBrush(Color.FromArgb(150, Color.White)))
                    for (int i = 0; i < 7; i++)
                    {
                        float bx = lumen.Left - 30f + ((i * 60f + off) % span);
                        if (bx < lumen.Left || bx > lumen.Right) continue;
                        float d = 3.2f + (i % 3) * 1.4f;
                        g.FillEllipse(bub, bx, lumen.Top + lumen.Height * (0.3f + (i % 2) * 0.35f), d, d);
                    }
            }
            else
            {
                using (var s = new Pen(Color.FromArgb(170, Color.White), 2.4f)
                { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    for (int i = 0; i < 5; i++)
                    {
                        float bx = lumen.Left - 40f + ((i * 60f + off) % span);
                        if (bx + 26 < lumen.Left || bx > lumen.Right) continue;
                        g.DrawLine(s, Math.Max(bx, lumen.Left), lumen.GetCenterY(),
                            Math.Min(bx + 26f, lumen.Right), lumen.GetCenterY());
                    }
            }
        }
        private void MetalBody(Graphics g, RectangleF body, HToolPalette pal)
        {
            using (var b = HBarBase.CylinderH(body, pal.MetalDark, Color.White))
                g.FillRectangle(b, body);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), body.X, body.Y, body.Width, body.Height);
            Lumen(g, body, pal);
        }
        private void FlangeV(Graphics g, float x, float cy, float pipeH, HToolPalette pal, float w = 14f)
        {
            var fr = new RectangleF(x, cy - pipeH * 0.52f, w, pipeH * 1.04f);
            using (var gp = HBarBase.RoundPath(fr, 2f))
            using (var b = HBarBase.CylinderH(fr, pal.MetalDark, pal.Metal))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.3f), gp);
            }
            g.Ell(fr.GetCenterX(), fr.Y + 4f, 2f, 2f, pal.Edge);
            g.Ell(fr.GetCenterX(), fr.Bottom - 4f, 2f, 2f, pal.Edge);
        }
        /// <summary>流向人字形箭头（dir：1 向右，-1 向左，0 向上）。</summary>
        private void Chevron(Graphics g, float x, float y, int dir, int alpha)
        {
            Color c = Color.FromArgb(alpha, Palette.Accent);
            using (var p = new Pen(c, 2.2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (dir == 0)
                {
                    g.DrawLine(p, x, y + 5f, x, y - 5f);
                    g.DrawLine(p, x - 4f, y, x, y - 5f);
                    g.DrawLine(p, x + 4f, y, x, y - 5f);
                }
                else
                {
                    float tip = x + dir * 6f;
                    g.DrawLine(p, x - dir * 4f, y - 5f, tip, y);
                    g.DrawLine(p, x - dir * 4f, y + 5f, tip, y);
                }
            }
        }
        private void FlowH(Graphics g, float x1, float x2, float y)
        {
            if (!Running) return;
            float span = x2 - x1, off = (Phase / (float)Math.PI * 0.5f * span) % 34f;
            for (float x = x1 - 34f + off; x < x2; x += 34f)
                Chevron(g, x, y, 1, 210);
        }
        // ----------------------------------------------------------------
        // Classic：法兰直管
        // ----------------------------------------------------------------
        private void DrawClassic(Graphics g, RectangleF area, HToolPalette pal)
        {
            float pipeH = Math.Max(8f, Math.Min(area.Height - 14f, 46f));
            float cy = area.Top + area.Height / 2f;
            float x1 = 6f, x2 = area.Width - 6f, fw = 14f;
            var body = new RectangleF(x1 + fw * 0.6f, cy - pipeH * 0.36f, x2 - x1 - fw * 1.2f, pipeH * 0.72f);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, pipeH, pal);
            FlangeV(g, x2 - fw, cy, pipeH, pal);
        }
        // ----------------------------------------------------------------
        // 新式管件
        // ----------------------------------------------------------------
        private void DrawElbow(Graphics g, RectangleF a, HToolPalette pal, int deg)
        {
            float t = Math.Min(a.Height, a.Width) * 0.22f;
            float cx = a.Left + a.Width * 0.62f, cy = a.Bottom - a.Height * 0.26f;
            float rOut = Math.Min(a.Width * 0.42f, a.Height * 0.5f), rIn = rOut - t;
            // 弯头（弧带）：外弧 + 内弧
            float start = 180f, sweep = deg;
            using (var path = new GraphicsPath())
            {
                path.AddArc(cx - rOut, cy - rOut, rOut * 2f, rOut * 2f, start, sweep);
                path.AddArc(cx - rIn, cy - rIn, rIn * 2f, rIn * 2f, start + sweep, -sweep);
                path.CloseFigure();
                using (var b = new LinearGradientBrush(new RectangleF(cx - rOut, cy - rOut, rOut * 2f, rOut * 2f),
                    Color.White, pal.MetalDark, 45f))
                    g.FillPath(b, path);
                g.DrawPath(new Pen(pal.Edge, 1.4f), path);
            }
            // 水平短段 + 法兰
            g.FillRectangle(Brushes.White, cx - rOut, cy - t / 2f, rOut, t);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), cx - rOut, cy - t / 2f, rOut, t);
            Lumen(g, new RectangleF(cx - rOut, cy - t / 2f, rOut, t), pal);
            FlangeV(g, cx - rOut - 12f, cy, t * 1.3f, pal);
            // 竖直段 + 法兰
            var vb = new RectangleF(cx - t / 2f, a.Top + 8f, t, cy - rOut - (a.Top + 8f));
            g.FillRectangle(new SolidBrush(Color.White), vb);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), vb.X, vb.Y, vb.Width, vb.Height);
            Lumen(g, vb, pal);
            FlangeH(g, cx, a.Top + 2f, t * 1.3f, pal);
            if (Running) Chevron(g, cx - rOut * 0.45f, cy - t * 0.9f, 0, 220);
        }
        private void FlangeH(Graphics g, float cx, float y, float pipeW, HToolPalette pal, float h = 12f)
        {
            var fr = new RectangleF(cx - pipeW * 0.52f, y, pipeW * 1.04f, h);
            using (var gp = HBarBase.RoundPath(fr, 2f))
            using (var b = new LinearGradientBrush(fr, pal.Metal, pal.MetalDark, 90f))
            {
                g.FillPath(b, gp);
                g.DrawPath(new Pen(pal.Edge, 1.3f), gp);
            }
            g.Ell(fr.Left + 4f, fr.GetCenterY(), 2f, 2f, pal.Edge);
            g.Ell(fr.Right - 4f, fr.GetCenterY(), 2f, 2f, pal.Edge);
        }
        private void DrawBranch(Graphics g, RectangleF a, HToolPalette pal, int n)
        {
            float cy = a.Top + a.Height * (n == 4 ? 0.5f : 0.64f), t = Math.Min(a.Height, a.Width) * 0.2f;
            float x1 = 8f, x2 = a.Width - 8f;
            var body = new RectangleF(x1, cy - t / 2f, x2 - x1, t);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, t * 1.3f, pal);
            FlangeV(g, x2 - 14f, cy, t * 1.3f, pal);
            // 中心节点
            float jx = a.Width * (n == 4 ? 0.5f : 0.56f);
            g.Ell(jx, cy, t * 0.72f, t * 0.72f, HToolPalettes.Tint(pal.Metal, 0.15f));
            // 上支管
            var up = new RectangleF(jx - t / 2f, a.Top + 8f, t, cy - t / 2f - (a.Top + 8f));
            g.FillRectangle(Brushes.White, up);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), up.X, up.Y, up.Width, up.Height);
            Lumen(g, up, pal);
            FlangeH(g, jx, a.Top + 2f, t * 1.3f, pal);
            if (n == 4)
            {
                var dn = new RectangleF(jx - t / 2f, cy + t / 2f, t, a.Bottom - 16f - (cy + t / 2f));
                g.FillRectangle(Brushes.White, dn);
                g.DrawRectangle(new Pen(pal.Edge, 1.3f), dn.X, dn.Y, dn.Width, dn.Height);
                Lumen(g, dn, pal);
                FlangeH(g, jx, a.Bottom - 14f, t * 1.3f, pal);
            }
            if (Running) { Chevron(g, jx, cy - t * 1.1f, 0, 210); if (n == 4) Chevron(g, jx, cy + t * 1.3f, 0, 180); }
        }
        private void DrawReducer(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.46f, h1 = a.Height * 0.44f, h2 = a.Height * 0.24f;
            float x1 = 10f, x2 = a.Width - 10f, sx = a.Width * 0.36f, ex = a.Width * 0.66f;
            var big = new RectangleF(x1, cy - h1 / 2f, sx - x1, h1);
            var sm = new RectangleF(ex, cy - h2 / 2f, x2 - ex, h2);
            MetalBody(g, big, pal);
            MetalBody(g, sm, pal);
            // 锥形段
            g.Poly(new[]
            {
                new PointF(sx, cy - h1 / 2f), new PointF(ex, cy - h2 / 2f),
                new PointF(ex, cy + h2 / 2f), new PointF(sx, cy + h1 / 2f)
            }, Color.White, pal.Edge, 1.3f);
            using (var b = new LinearGradientBrush(new RectangleF(sx, 0, ex - sx, 1), MediaDark, Media, 90f))
                g.FillPolygon(b, new[]
                {
                    new PointF(sx + 1f, cy - h1 * 0.28f), new PointF(ex - 1f, cy - h2 * 0.28f),
                    new PointF(ex - 1f, cy + h2 * 0.28f), new PointF(sx + 1f, cy + h1 * 0.28f)
                });
            FlangeV(g, x1 - 6f, cy, h1 * 1.15f, pal);
            FlangeV(g, x2 - 8f, cy, h2 * 1.4f, pal, 10f);
            FlowH(g, sx, ex, cy);
        }
        private void DrawUBend(Graphics g, RectangleF a, HToolPalette pal)
        {
            float t = a.Height * 0.2f, xL = a.Left + a.Width * 0.2f, xR = a.Right - a.Width * 0.2f;
            float yTop = a.Top + a.Height * 0.16f, yBot = a.Bottom - a.Height * 0.24f;
            float r = (xR - xL) / 2f, cyc = yTop + r;
            // U 形路径（描边带 + 内腔）
            using (var path = new GraphicsPath())
            {
                path.AddLine(xL, yBot, xL, cyc);
                path.AddArc(xL, yTop, r * 2f, r * 2f, 180f, 180f);
                path.AddLine(xR, cyc, xR, yBot);
                using (var p = new Pen(Color.White, t))
                    g.DrawPath(p, path);
                using (var p = new Pen(pal.Edge, 1.4f))
                {
                    g.DrawPath(p, path);
                    using (var mp = new Pen(Media, t * 0.52f))
                        g.DrawPath(mp, path);
                }
            }
            FlangeH(g, xL, yBot - 4f, t * 1.3f, pal);
            FlangeH(g, xR, yBot - 4f, t * 1.3f, pal);
            if (Running)
            {
                Chevron(g, xL, (yTop + yBot) / 2f + 8f, 0, 150);
                Chevron(g, xR, (yTop + yBot) / 2f + 8f, 0, 150);
            }
        }
        private void DrawCoil(Graphics g, RectangleF a, HToolPalette pal)
        {
            float t = a.Height * 0.16f, x1 = a.Left + 10f, x2 = a.Right - 10f;
            float y1 = a.Top + a.Height * 0.26f, y2 = a.Bottom - a.Height * 0.26f;
            using (var path = new GraphicsPath())
            {
                path.AddLine(x1, y1, x2 - 20f, y1);
                path.AddArc(x2 - 40f, y1 - t, 40f, (y2 - y1) + t * 2f, 90f, -180f);
                path.AddLine(x2 - 40f, y2, x1 + 20f, y2);
                path.AddArc(x1, y1 - t, 40f, (y2 - y1) + t * 2f, -90f, -180f);
                using (var p = new Pen(Color.White, t)) g.DrawPath(p, path);
                using (var p = new Pen(pal.Edge, 1.3f)) g.DrawPath(p, path);
                if (Running) using (var mp = new Pen(Media, t * 0.5f)) g.DrawPath(mp, path);
            }
            FlangeV(g, x1 - 8f, y1, t * 1.5f, pal, 10f);
            FlangeV(g, x2 - 2f, y2, t * 1.5f, pal, 10f);
        }
        private void DrawHose(Graphics g, RectangleF a, HToolPalette pal, bool flex)
        {
            float cy = a.Top + a.Height * (flex ? 0.5f : 0.54f), h = a.Height * (flex ? 0.26f : 0.3f);
            float x1 = a.Left + 14f, x2 = a.Right - 14f;
            var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
            g.FillR(body, h / 2f, Color.FromArgb(70, 74, 80));
            using (var rib = new Pen(Color.FromArgb(44, 46, 52), 2.2f))
                for (float x = x1 + 8f; x < x2 - 6f; x += (flex ? 7f : 9f))
                    g.DrawLine(rib, x, cy - h / 2f + 2, x, cy + h / 2f - 2);
            // 内腔（半透明介质）
            var lumen = RectangleF.Inflate(body, -10f, -h * 0.24f);
            using (var b = new SolidBrush(Color.FromArgb(150, Media))) g.FillR(lumen, h * 0.2f, b.Color);
            if (Running)
                using (var s = new Pen(Color.FromArgb(180, Color.White), 2f))
                {
                    float span = lumen.Width + 50f, off = (Phase / (float)Math.PI * 0.5f * span) % 50f;
                    for (float x = lumen.Left - 50f + off; x < lumen.Right; x += 50f)
                        g.DrawLine(s, x, lumen.GetCenterY(), x + 18f, lumen.GetCenterY());
                }
            // 两端快接
            g.Box(new RectangleF(x1 - 12f, cy - h * 0.42f, 12f, h * 0.84f), 2f, pal.Metal, pal.Edge, 1.1f);
            g.Box(new RectangleF(x2, cy - h * 0.42f, 12f, h * 0.84f), 2f, pal.Metal, pal.Edge, 1.1f);
            if (flex)
                HBarBase.DrawText(g, HTranslation.GetContent("挠性"), new Font("微软雅黑", 6.5f), Color.White,
                    new RectangleF(a.Left, a.Top + 2f, a.Width, 12f), ContentAlignment.TopCenter);
        }
        private void DrawNipple(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.5f, h = a.Height * 0.26f;
            float x1 = a.Left + 18f, x2 = a.Right - 18f;
            var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
            MetalBody(g, body, pal);
            // 两端外螺纹（密集竖线）
            using (var th = new Pen(pal.MetalDark, 1.1f))
            {
                for (float x = x1; x < x1 + 16f; x += 2.6f)
                    g.DrawLine(th, x, cy - h / 2f, x, cy + h / 2f);
                for (float x = x2 - 16f; x < x2; x += 2.6f)
                    g.DrawLine(th, x, cy - h / 2f, x, cy + h / 2f);
            }
            // 六角螺母
            g.Poly(new[]
            {
                new PointF(x1 + 18f, cy - h * 0.7f), new PointF(x1 + 30f, cy - h * 0.7f),
                new PointF(x1 + 34f, cy), new PointF(x1 + 30f, cy + h * 0.7f),
                new PointF(x1 + 18f, cy + h * 0.7f), new PointF(x1 + 14f, cy)
            }, pal.Metal, pal.Edge, 1.2f);
            g.Poly(new[]
            {
                new PointF(x2 - 30f, cy - h * 0.7f), new PointF(x2 - 18f, cy - h * 0.7f),
                new PointF(x2 - 14f, cy), new PointF(x2 - 18f, cy + h * 0.7f),
                new PointF(x2 - 30f, cy + h * 0.7f), new PointF(x2 - 34f, cy)
            }, pal.Metal, pal.Edge, 1.2f);
            FlowH(g, x1 + 34f, x2 - 34f, cy);
        }
        private void DrawValvePipe(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.58f, h = a.Height * 0.22f, x1 = 8f, x2 = a.Width - 8f;
            var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, h * 1.4f, pal, 10f);
            FlangeV(g, x2 - 10f, cy, h * 1.4f, pal, 10f);
            float vx = a.Width * 0.5f;
            // 阀体（菱形）
            g.Poly(new[]
            {
                new PointF(vx - 16f, cy), new PointF(vx, cy - h * 0.85f),
                new PointF(vx + 16f, cy), new PointF(vx, cy + h * 0.85f)
            }, HToolPalettes.Tint(pal.Main, 0.2f), pal.Edge, 1.4f);
            // 阀杆 + 手轮
            g.Line(pal.Edge, 2.6f, vx, cy - h * 0.85f, vx, cy - h * 1.7f);
            g.DrawEllipse(new Pen(pal.Edge, 2f), vx - 8f, cy - h * 2.2f, 16f, 10f);
            FlowH(g, x1 + 14f, vx - 18f, cy);
            FlowH(g, vx + 18f, x2 - 14f, cy);
        }
        private void DrawY(Graphics g, RectangleF a, HToolPalette pal)
        {
            float t = a.Height * 0.2f;
            float jx = a.Left + a.Width * 0.5f, jy = a.Bottom - a.Height * 0.28f;
            // 单入口（底）
            var st = new RectangleF(jx - t / 2f, jy, t, a.Bottom - 14f - jy);
            g.FillRectangle(Brushes.White, st);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), st.X, st.Y, st.Width, st.Height);
            Lumen(g, st, pal);
            FlangeH(g, jx, a.Bottom - 14f, t * 1.3f, pal);
            // 两支斜管
            double ang = 32 * Math.PI / 180.0;
            foreach (int s in new[] { -1, 1 })
            {
                float ex = jx + s * a.Width * 0.34f, ey = a.Top + a.Height * 0.2f;
                using (var p = new Pen(Color.White, t))
                    g.DrawLine(p, jx, jy, ex, ey);
                using (var p = new Pen(pal.Edge, 1.3f))
                    g.DrawLine(p, jx, jy, ex, ey);
                if (Running) using (var p = new Pen(Media, t * 0.52f))
                        g.DrawLine(p, jx, jy, ex, ey);
                FlangeV(g, ex - 7f, ey, t * 1.3f, pal, 10f);
                if (Running)
                {
                    float mx = (jx + ex) / 2f, my = (jy + ey) / 2f;
                    Chevron(g, mx, my, s, 200);
                }
            }
            g.Ell(jx, jy, t * 0.5f, t * 0.5f, HToolPalettes.Tint(pal.Metal, 0.15f));
        }
        private void DrawBlind(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f, h = a.Height * 0.3f, x1 = 8f, x2 = a.Width * 0.6f;
            var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, h * 1.2f, pal);
            // 盲板（厚法兰 + 实心板 + 红色吊牌）
            FlangeV(g, x2 - 4f, cy, h * 1.25f, pal, 12f);
            g.Box(new RectangleF(x2 + 8f, cy - h * 0.62f, 8f, h * 1.24f), 2f,
                Color.FromArgb(190, 60, 55), pal.Edge, 1.3f);
            g.Line(Color.FromArgb(190, 60, 55), 1.6f, x2 + 12f, cy - h * 0.3f, x2 + 12f, cy + h * 0.3f);
            FlowH(g, x1 + 16f, x2 - 6f, cy);
        }
        private void DrawInsulated(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f, x1 = 8f, x2 = a.Width - 8f;
            float hi = a.Height * 0.44f;
            var ins = new RectangleF(x1 + 10f, cy - hi / 2f, x2 - x1 - 20f, hi);
            g.FillR(ins, 6f, Color.FromArgb(200, 190, 170));
            g.DrawR(ins, 6f, Color.FromArgb(150, 130, 100), 1.6f);
            // 保温抱箍
            using (var band = new Pen(Color.FromArgb(120, 100, 70), 3f))
                foreach (float bx in new[] { ins.Left + 18f, ins.Right - 18f })
                    g.DrawLine(band, bx, ins.Top - 2f, bx, ins.Bottom + 2f);
            // 内管露头 + 介质
            var body = new RectangleF(x1, cy - hi * 0.36f, x2 - x1, hi * 0.72f);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, hi * 0.86f, pal, 12f);
            FlangeV(g, x2 - 12f, cy, hi * 0.86f, pal, 12f);
        }
        private void DrawPVC(Graphics g, RectangleF a, HToolPalette pal, bool cast)
        {
            float cy = a.Top + a.Height * 0.52f, h = a.Height * 0.28f, x1 = 10f, x2 = a.Width - 10f;
            Color pvc = cast ? Color.FromArgb(96, 100, 106) : Color.FromArgb(225, 230, 236);
            var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
            g.FillRectangle(new SolidBrush(pvc), body);
            g.DrawRectangle(new Pen(pal.Edge, 1.3f), body.X, body.Y, body.Width, body.Height);
            Lumen(g, body, pal);
            if (cast)
            {
                // 承插扩口（两端喇叭）
                g.Poly(new[] { new PointF(x1, cy - h / 2f), new PointF(x1 - 10f, cy - h * 0.72f),
                    new PointF(x1 - 10f, cy + h * 0.72f), new PointF(x1, cy + h / 2f) },
                    Color.FromArgb(70, 74, 80), pal.Edge, 1.2f);
                g.Poly(new[] { new PointF(x2, cy - h / 2f), new PointF(x2 + 10f, cy - h * 0.72f),
                    new PointF(x2 + 10f, cy + h * 0.72f), new PointF(x2, cy + h / 2f) },
                    Color.FromArgb(70, 74, 80), pal.Edge, 1.2f);
            }
            else
            {
                // PVC 粘接套环
                g.Box(new RectangleF(x1, cy - h * 0.62f, 10f, h * 1.24f), 2f, Color.FromArgb(200, 206, 214), pal.Edge, 1.1f);
                g.Box(new RectangleF(x2 - 10f, cy - h * 0.62f, 10f, h * 1.24f), 2f, Color.FromArgb(200, 206, 214), pal.Edge, 1.1f);
            }
        }
        private void DrawClamp(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f, h = a.Height * 0.26f, x1 = 10f, x2 = a.Width - 10f, mx = a.Width * 0.5f;
            var b1 = new RectangleF(x1, cy - h / 2f, mx - x1 - 8f, h);
            var b2 = new RectangleF(mx + 8f, cy - h / 2f, x2 - mx - 8f, h);
            MetalBody(g, b1, pal);
            MetalBody(g, b2, pal);
            // 快装卡箍（半圆环 + 卡耳）
            g.DrawArc(new Pen(pal.MetalDark, 4f), mx - 10f, cy - h * 0.78f, 20f, h * 1.56f, 200f, 140f);
            g.Ell(mx - 11f, cy - h * 0.86f, 3f, 3f, pal.Edge);
            g.Ell(mx + 11f, cy - h * 0.86f, 3f, 3f, pal.Edge);
            g.Line(pal.Edge, 1.4f, mx - 10f, cy - h * 0.55f, mx - 14f, cy - h * 0.95f);
            g.Line(pal.Edge, 1.4f, mx + 10f, cy - h * 0.55f, mx + 14f, cy - h * 0.95f);
        }
        private void DrawParallel(Graphics g, RectangleF a, HToolPalette pal)
        {
            float x1 = 10f, x2 = a.Width - 10f, h = a.Height * 0.16f;
            foreach (float k in new[] { 0.34f, 0.66f })
            {
                float cy = a.Top + a.Height * k;
                var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
                MetalBody(g, body, pal);
                FlangeV(g, x1, cy, h * 1.5f, pal, 10f);
                FlangeV(g, x2 - 10f, cy, h * 1.5f, pal, 10f);
            }
            // 共架
            g.Line(pal.MetalDark, 3f, a.Width * 0.28f, a.Bottom - 8f, a.Width * 0.28f, a.Bottom - 3f);
            g.Line(pal.MetalDark, 3f, a.Width * 0.72f, a.Bottom - 8f, a.Width * 0.72f, a.Bottom - 3f);
        }
        private void DrawSpool(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f, h = a.Height * 0.24f, x1 = 8f, x2 = a.Width - 8f;
            var body = new RectangleF(x1 + 8f, cy - h / 2f, x2 - x1 - 16f, h);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, h * 1.3f, pal);
            FlangeV(g, x2 - 14f, cy, h * 1.3f, pal);
            // 支吊架（三角）
            float sx = a.Width * 0.5f;
            g.Line(pal.MetalDark, 3.4f, sx, cy - h / 2f, sx, a.Top + 4f);
            g.Line(pal.MetalDark, 2.6f, sx - 14f, a.Top + 10f, sx + 14f, a.Top + 10f);
            // U 型管卡
            g.DrawArc(new Pen(pal.MetalDark, 2f), sx - 9f, cy - h / 2f - 2f, 18f, 14f, 0f, 180f);
        }
        private void DrawExpansion(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.52f, h = a.Height * 0.3f, x1 = 8f, x2 = a.Width - 8f;
            float b1 = a.Width * 0.32f, b2 = a.Width * 0.68f;
            var left = new RectangleF(x1, cy - h / 2f, b1 - x1, h);
            var right = new RectangleF(b2, cy - h / 2f, x2 - b2, h);
            MetalBody(g, left, pal);
            MetalBody(g, right, pal);
            FlangeV(g, x1, cy, h * 1.15f, pal);
            FlangeV(g, x2 - 14f, cy, h * 1.15f, pal);
            // 波纹节
            using (var path = new GraphicsPath())
            {
                float x = b1;
                bool up = true;
                while (x < b2)
                {
                    float nx = Math.Min(x + 7f, b2);
                    path.AddLine(x, up ? cy - h * 0.62f : cy + h * 0.62f,
                        nx, up ? cy + h * 0.62f : cy - h * 0.62f);
                    x = nx;
                    up = !up;
                }
                using (var p = new Pen(HToolPalettes.Shade(pal.Metal, 0.25f), 2.4f))
                    g.DrawPath(p, path);
            }
            g.Line(pal.Edge, 1.3f, b1, cy - h / 2f, b1, cy + h / 2f);
            g.Line(pal.Edge, 1.3f, b2, cy - h / 2f, b2, cy + h / 2f);
        }
        private void DrawFilter(Graphics g, RectangleF a, HToolPalette pal)
        {
            float cy = a.Top + a.Height * 0.42f, h = a.Height * 0.22f, x1 = 8f, x2 = a.Width - 8f;
            var body = new RectangleF(x1, cy - h / 2f, x2 - x1, h);
            MetalBody(g, body, pal);
            FlangeV(g, x1, cy, h * 1.4f, pal, 10f);
            FlangeV(g, x2 - 10f, cy, h * 1.4f, pal, 10f);
            float jx = a.Width * 0.5f, fy = a.Bottom - 6f;
            // Y 型滤筒（斜下 45°）
            double ang = 55 * Math.PI / 180.0;
            float ex = jx + (float)Math.Cos(ang) * a.Height * 0.42f;
            float ey = cy + (float)Math.Sin(ang) * a.Height * 0.42f;
            using (var p = new Pen(HToolPalettes.Tint(pal.Metal, 0.2f), h)) g.DrawLine(p, jx, cy, ex, ey);
            using (var p = new Pen(pal.Edge, 1.3f)) g.DrawLine(p, jx, cy, ex, ey);
            // 滤网斜线
            using (var p = new Pen(HToolPalettes.Shade(pal.Main, 0.2f), 1.2f))
                for (int i = 1; i <= 4; i++)
                {
                    float t2 = i / 5.2f, px = jx + (ex - jx) * t2, py = cy + (ey - cy) * t2;
                    g.DrawLine(p, px - 5f, py + 2f, px + 5f, py - 2f);
                }
            // 底部排污堵头
            g.Ell(ex, ey, 5f, 5f, pal.MetalDark);
            FlowH(g, x1 + 14f, jx - 10f, cy);
            FlowH(g, jx + 10f, x2 - 14f, cy);
        }
    }
    /// <summary>
    /// 气体管控件（继承 HPipe）：22 种管路外形与水管一致，管内介质为气体，
    /// Classic 色调为燃气管路常用的琥珀黄，Flowing 时管内气泡流动。
    /// </summary>
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [HDescriptionLanguage("气体管控件：22 种管路外形，输送时管内有气泡流动动画")]
    public class HGasPipe : HPipe
    {
        public HGasPipe()
        {
            _kind = HPipeKind.Gas;
        }
    }
    internal static class HRectExt
    {
        public static float GetCenterX(this RectangleF r) => r.X + r.Width / 2f;
        public static float GetCenterY(this RectangleF r) => r.Y + r.Height / 2f;
    }
}