using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.Button
{
    /// <summary>
    /// 滑动开关 32 种样式的统一渲染器：控件只负责轨道外框（无文字时为 Padding 内整块区域）、
    /// 滑行进度 0..1 与基准色，轨道外形、填充、光效、圆钮造型/符号全部在此按样式分发。
    /// 圆角口径与 HRadioButton 一致：外轮廓像素盒 (-0.5 偏移)、弧直径 +0.5，
    /// 描边为向内的奇偶填充环，四边等宽、四角弧线均匀，所有内容均收在圆角框内。
    /// </summary>
    internal static class HSlideSwitchRenderer
    {
        public static void Draw(Graphics g, RectangleF t, HSlideSwitchStyle st, float k,
            Color onCol, Color offCol, bool enabled, string onWord, string offWord, Font wordFont)
        {
            float h = t.Height, w = t.Width;
            if (h <= 1 || w <= 1) return;

            // 扩展样式（32 种）走独立配方管线，不占用原 32 样式的分支
            if ((int)st >= 32) { DrawExtra(g, t, st, k, onCol, offCol, enabled); return; }

            #region 配色（部分样式使用固定语义色）
            Color off = offCol, on = onCol;
            switch (st)
            {
                case HSlideSwitchStyle.DayNight:
                    off = Color.FromArgb(245, 180, 52);
                    on = Color.FromArgb(51, 65, 85);
                    break;
                case HSlideSwitchStyle.Traffic:
                    off = Color.FromArgb(239, 68, 68);
                    on = Color.FromArgb(34, 197, 94);
                    break;
                case HSlideSwitchStyle.Neon:
                    off = Color.FromArgb(16, 24, 39);
                    on = Color.FromArgb(8, 15, 28);
                    break;
                case HSlideSwitchStyle.Cyber:
                    off = Color.FromArgb(21, 27, 38);
                    on = Color.FromArgb(10, 20, 36);
                    break;
                case HSlideSwitchStyle.Dark:
                    off = Color.FromArgb(58, 66, 80);
                    on = Color.FromArgb(17, 24, 39);
                    break;
                case HSlideSwitchStyle.Antique:
                    off = Color.FromArgb(176, 141, 87);
                    on = Color.FromArgb(131, 95, 46);
                    break;
            }
            Color gray = SystemColors.Control;
            if (!enabled) { off = Mix(off, gray, 0.55f); on = Mix(on, gray, 0.55f); }
            Color track = Mix(off, on, k);
            Color accent = enabled ? onCol : Mix(onCol, gray, 0.5f);
            Color ink = Color.FromArgb(enabled ? 255 : 150, 255, 255, 255);
            #endregion

            #region 轨道外形（所有样式共用同一外框尺寸；Thin/Rubber 仅纵向内收为细槽）
            RectangleF ti = t;
            if (st == HSlideSwitchStyle.Thin) ti = new RectangleF(t.X, t.Y + h * 0.27f, t.Width, h * 0.46f);
            else if (st == HSlideSwitchStyle.Rubber) ti = new RectangleF(t.X, t.Y + h * 0.2f, t.Width, h * 0.6f);

            bool chamfer = st == HSlideSwitchStyle.Cyber;
            float trackVisualR =
                st == HSlideSwitchStyle.Square ? Math.Max(1.5f, h * 0.08f) :
                st == HSlideSwitchStyle.Rounded ? h * 0.26f :
                ti.Height / 2f;
            // Cyber 切角六边形：外框四边整数对齐控件边界，上下左右像素相位一致（六边等宽）；
            // 顶点留作描边环等距内缩使用
            PointF[] chamferPts = null;
            GraphicsPath trackPath;
            if (chamfer)
            {
                chamferPts = ChamferPoints(new RectangleF(ti.X, ti.Y, ti.Width, ti.Height));
                trackPath = PolygonPath(chamferPts);
            }
            else trackPath = TrackBox(ti, trackVisualR);
            #endregion

            #region 槽体描边环宽（0 表示无描边；描边全部向内填充，不占框外像素）
            float pw = 0f;
            Color edgeColor = Color.Empty;
            bool whiteTrack = st == HSlideSwitchStyle.Outline || st == HSlideSwitchStyle.MonoLine;
            bool glowStyle = st == HSlideSwitchStyle.Glow || st == HSlideSwitchStyle.Neon ||
                st == HSlideSwitchStyle.Cyber || st == HSlideSwitchStyle.Traffic;
            if (whiteTrack)
            {
                pw = st == HSlideSwitchStyle.MonoLine ? Math.Max(1f, h * 0.05f) : Math.Max(1.2f, h * 0.07f);
                edgeColor = track;
            }
            else if (st == HSlideSwitchStyle.Neon)
            {
                pw = Math.Max(1.2f, h * 0.06f);
                edgeColor = Color.FromArgb((int)(60 + 150 * k), accent);
            }
            else if (st == HSlideSwitchStyle.Cyber)
            {
                pw = Math.Max(1.2f, h * 0.06f);
                edgeColor = Color.FromArgb((int)(80 + 140 * k), 34, 211, 238);
            }
            #endregion

            #region 轨道填充
            if (whiteTrack)
                Fill(g, trackPath, enabled ? Color.White : Mix(Color.White, gray, 0.4f));
            else if (st == HSlideSwitchStyle.Skeuo)
            {
                using (var br = new LinearGradientBrush(ti, Dark(track, 0.22f), Light(track, 0.18f), 90f))
                    g.FillPath(br, trackPath);
            }
            else if (st == HSlideSwitchStyle.Gradient || st == HSlideSwitchStyle.Ocean || st == HSlideSwitchStyle.Antique)
            {
                using (var br = new LinearGradientBrush(ti, Light(track, 0.22f), Dark(track, 0.08f), 90f))
                    g.FillPath(br, trackPath);
            }
            else if (st == HSlideSwitchStyle.Candy)
            {
                Color c1 = Mix(Color.FromArgb(255, 107, 129), off, 1f - k);
                Color c2 = Mix(Color.FromArgb(255, 159, 67), off, 1f - k);
                using (var br = new LinearGradientBrush(ti, c1, c2, 0f))
                    g.FillPath(br, trackPath);
            }
            else if (st == HSlideSwitchStyle.Emboss)
                Fill(g, trackPath, Dark(track, 0.06f));
            else if (st == HSlideSwitchStyle.Liquid)
            {
                // 关态槽打底，开态液面随进度从左端线性铺满：k=0 不露开态色，k=1 铺满整条轨道
                Fill(g, trackPath, off);
                if (k > 0f)
                {
                    g.SetClip(trackPath);
                    using (var ob = new SolidBrush(on))
                        g.FillRectangle(ob, new RectangleF(t.X, ti.Y, t.Width * k, ti.Height));
                    g.ResetClip();
                }
            }
            else
                Fill(g, trackPath, track);
            #endregion

            #region 内发光（Glow/Neon/Cyber/Traffic）：辉光裁剪在圆角框内，外框尺寸不缩
            if (glowStyle)
            {
                Color gc = (st == HSlideSwitchStyle.Neon || st == HSlideSwitchStyle.Cyber) ? accent : track;
                float strength = 0.2f + 0.8f * k;
                float[] gw = { h * 0.30f, h * 0.20f, h * 0.11f };
                int[] ga = { 22, 42, 66 };
                var oldClip = g.Clip;
                g.SetClip(trackPath);
                for (int i = 0; i < 3; i++)
                {
                    int alpha = (int)(ga[i] * strength);
                    using (var pen = new Pen(Color.FromArgb(Math.Max(0, Math.Min(255, alpha)), gc), gw[i]))
                        g.DrawPath(pen, trackPath);
                }
                g.Clip = oldClip;
            }
            #endregion

            #region 槽内装饰线
            if (st == HSlideSwitchStyle.Skeuo)
            {
                Line(g, ti.X + h * 0.34f, ti.Y + 1.6f, ti.Right - h * 0.34f, ti.Y + 1.6f, Color.FromArgb(80, 255, 255, 255));
                Line(g, ti.X + h * 0.34f, ti.Bottom - 1.6f, ti.Right - h * 0.34f, ti.Bottom - 1.6f, Color.FromArgb(40, 0, 0, 0));
            }
            else if (st == HSlideSwitchStyle.Emboss)
            {
                Line(g, ti.X + h * 0.3f, ti.Y + 1.5f, ti.Right - h * 0.3f, ti.Y + 1.5f, Color.FromArgb(56, 0, 0, 0));
                Line(g, ti.X + h * 0.3f, ti.Bottom - 1.5f, ti.Right - h * 0.3f, ti.Bottom - 1.5f, Color.FromArgb(140, 255, 255, 255));
            }
            else if (st == HSlideSwitchStyle.Segmented)
            {
                float cx = ti.X + ti.Width / 2;
                Line(g, cx, ti.Y + h * 0.24f, cx, ti.Bottom - h * 0.24f, Color.FromArgb(150, 255, 255, 255), Math.Max(1f, h * 0.045f));
            }
            #endregion

            #region 槽体描边环（厚度全部向内，四边等宽）
            if (pw > 0f)
            {
                using (GraphicsPath inner = chamfer
                    ? PolygonPath(InsetPolygon(chamferPts, pw))
                    : HDrawPaint.CreatePath(InnerBox(ti, pw), trackVisualR + 0.5f - pw, HRoundStyle.All, false))
                using (var ring = new GraphicsPath(FillMode.Alternate))
                {
                    ring.AddPath(trackPath, false);
                    ring.AddPath(inner, false);
                    using (var b = new SolidBrush(edgeColor)) g.FillPath(b, ring);
                }
            }
            #endregion

            #region 圆钮几何（槽内文字区域需扣除圆钮占位，先行计算）
            float d = KnobD(st, t, ti);
            float kwExtra = st == HSlideSwitchStyle.Pill ? h * 0.42f : 0f;
            float margin = st == HSlideSwitchStyle.Dot ? h * 0.12f : KnobMargin(st, h);
            float travel = w - 2 * margin - d - kwExtra;
            float kx = t.X + margin + Math.Max(0, travel) * k;
            float ky = ti.Y + (ti.Height - d) / 2;
            float kw = d + kwExtra * k;
            var knob = new RectangleF(kx, ky, kw, d);
            bool onRight = k >= 0.5f;
            #endregion

            #region 槽内符号：Material 勾叉 / Text 或自定义 ON·OFF 文字
            if (st == HSlideSwitchStyle.Material)
            {
                var z = SideZone(t, onRight, h * 0.62f);
                if (onRight) DrawCheck(g, z, ink, Math.Max(1.4f, h * 0.08f));
                else DrawCross(g, z, ink, Math.Max(1.4f, h * 0.075f));
            }
            else if (st == HSlideSwitchStyle.Text || onWord.Length > 0 || offWord.Length > 0)
            {
                string word;
                if (st == HSlideSwitchStyle.Text)
                    word = onRight ? (onWord.Length > 0 ? onWord : "ON")
                                   : (offWord.Length > 0 ? offWord : "OFF");
                else
                    word = onRight ? onWord : offWord;
                if (!string.IsNullOrEmpty(word))
                {
                    // 文本矩形 = 轨道（控件尺寸减 Padding）扣除圆钮与端弧/描边后的剩余矩形，文字在其中居中
                    float ins = h * 0.1f + pw;
                    RectangleF wordArea = onRight
                        ? new RectangleF(t.X + ins, t.Y, kx - t.X - ins, t.Height)
                        : new RectangleF(kx + kw, t.Y, t.Right - ins - kx - kw, t.Height);
                    var z = Rectangle.Round(wordArea);
                    Color wc = whiteTrack ? track : Color.FromArgb(235, 255, 255, 255);
                    TextRenderer.DrawText(g, word, wordFont, z, wc,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                }
            }
            #endregion

            #region 圆钮（阴影裁剪在槽内；填充与描边均用半像素盒，边缘等宽）
            Color kc = Color.White;
            switch (st)
            {
                case HSlideSwitchStyle.Neon: kc = Mix(Color.FromArgb(99, 110, 128), accent, k); break;
                case HSlideSwitchStyle.Cyber: kc = Mix(Color.FromArgb(86, 96, 112), Color.FromArgb(34, 211, 238), k); break;
                case HSlideSwitchStyle.Outline: kc = onRight ? track : Color.White; break;
                case HSlideSwitchStyle.Antique: kc = Color.FromArgb(244, 236, 216); break;
            }
            if (!enabled) kc = Mix(kc, gray, 0.35f);

            bool shadow = st != HSlideSwitchStyle.Flat && st != HSlideSwitchStyle.Outline &&
                st != HSlideSwitchStyle.MonoLine && st != HSlideSwitchStyle.Ring && st != HSlideSwitchStyle.Thin &&
                st != HSlideSwitchStyle.Dot && st != HSlideSwitchStyle.Neon && st != HSlideSwitchStyle.Cyber &&
                st != HSlideSwitchStyle.Dark && st != HSlideSwitchStyle.Glow && st != HSlideSwitchStyle.Emboss &&
                st != HSlideSwitchStyle.Skeuo;
            if (shadow)
            {
                int sa = st == HSlideSwitchStyle.Shadow ? 90 : 38;
                float dy = Math.Max(1f, h * 0.05f);
                var oldClip = g.Clip;
                g.SetClip(trackPath);
                using (var sb = new SolidBrush(Color.FromArgb(sa, 0, 0, 0)))
                    g.FillEllipse(sb, knob.X, knob.Y + dy, knob.Width, d);
                g.Clip = oldClip;
            }

            // 圆钮填充/描边盒：外沿落在半像素边界（与 HRadioButton 圆环同口径）
            var knobBox = new RectangleF(kx + 0.5f, ky + 0.5f, kw - 1f, d - 1f);
            GraphicsPath kp =
                st == HSlideSwitchStyle.Square ? KnobRound(knob, d * 0.22f) :
                st == HSlideSwitchStyle.Cyber ? PolygonPath(ChamferPoints(knob)) :
                st == HSlideSwitchStyle.Pill ? KnobPill(knob) : null;
            using (kp)
            {
                if (st == HSlideSwitchStyle.Ring)
                {
                    using (var kb = new SolidBrush(kc)) g.FillEllipse(kb, knobBox);
                    float inner = d * 0.6f;
                    var ib = new RectangleF(kx + (kw - inner) / 2 + 0.5f, ky + (d - inner) / 2 + 0.5f, inner - 1f, inner - 1f);
                    using (var kb = new SolidBrush(track)) g.FillEllipse(kb, ib);
                }
                else if (kp == null)
                {
                    using (var kb = new SolidBrush(kc)) g.FillEllipse(kb, knobBox);
                }
                else
                    Fill(g, kp, kc);

                bool noEdge = st == HSlideSwitchStyle.Flat || st == HSlideSwitchStyle.Glow ||
                    st == HSlideSwitchStyle.Neon || st == HSlideSwitchStyle.Cyber ||
                    st == HSlideSwitchStyle.Dark || st == HSlideSwitchStyle.Ring;
                if (!noEdge)
                {
                    Color ec = st == HSlideSwitchStyle.Windows && !onRight
                        ? Color.FromArgb(170, 170, 170)
                        : Color.FromArgb(enabled ? 34 : 24, 0, 0, 0);
                    using (var pen = new Pen(ec, 1f))
                    {
                        if (kp == null) g.DrawEllipse(pen, knobBox);
                        else g.DrawPath(pen, kp);
                    }
                }
            }

            // 圆钮符号
            var glyph = new RectangleF(knob.X + d * 0.22f, knob.Y + d * 0.22f, d * 0.56f, d * 0.56f);
            switch (st)
            {
                case HSlideSwitchStyle.Check:
                    if (onRight) DrawCheck(g, glyph, Color.FromArgb(22, 163, 74), d * 0.1f);
                    else DrawCross(g, glyph, Color.FromArgb(150, 156, 168), d * 0.09f);
                    break;
                case HSlideSwitchStyle.Power:
                    DrawPower(g, glyph, Mix(Color.FromArgb(120, 126, 138), accent, k), d * 0.1f);
                    break;
                case HSlideSwitchStyle.DayNight:
                    if (onRight) DrawMoon(g, glyph, Color.FromArgb(71, 85, 105));
                    else DrawSun(g, glyph, d);
                    break;
                case HSlideSwitchStyle.Ocean:
                    using (var db = new SolidBrush(enabled ? onCol : Color.Gray))
                        g.FillEllipse(db, glyph);
                    break;
                case HSlideSwitchStyle.Cyber:
                    using (var db = new SolidBrush(Color.FromArgb((int)(120 + 135 * k), 34, 211, 238)))
                        g.FillEllipse(db, glyph);
                    break;
            }
            #endregion

            trackPath.Dispose();
        }

        #region 扩展样式（32-63）配方渲染

        // 钮面符号
        private const int GlLock = 0, GlHeart = 1, GlStar = 2, GlCloud = 3, GlNote = 4, GlShield = 5, GlDot = 6;
        // 槽体填充
        private const int FmSolid = 0, FmVert = 1, FmDuo = 2;
        // 描边
        private const int BdNone = 0, BdRing = 1, BdDash = 2, BdDouble = 3, BdGradient = 4;
        // 钮填充
        private const int KbSolid = 0, KbGloss = 1, KbWave = 2, KbNova = 3;

        private static void DrawExtra(Graphics g, RectangleF t, HSlideSwitchStyle st, float k,
            Color onCol, Color offCol, bool enabled)
        {
            float h = t.Height, w = t.Width;
            Color gray = SystemColors.Control;
            Color acc = enabled ? onCol : Mix(onCol, gray, 0.5f);

            Color offC = Color.FromArgb(180, 186, 194), onC = acc;
            float rfac = 0.5f;
            int fillMode = FmSolid;
            Color duoA = Color.Empty, duoB = Color.Empty, duoDark = Color.Empty;
            int texture = 0;           // 1 碳纤斜纹 2 扫描线
            Color glowC = Color.Empty;
            int border = BdNone; float bw = 1f; Color bc = Color.White, bc2 = Color.White;
            bool alphaFill = false; int fillAlpha = 255;
            bool neuEdge = false;
            float knobF = 0.74f, knobMF = 0.13f;
            bool knobSquare = false, knobShadow = false, knobEdge = false;
            Color knobOff = Color.White, knobOn = Color.White;
            int knobFill = KbSolid;
            int halo = 0; Color haloC = acc;
            int glyph = -1; Color glyphOn = Color.Empty, glyphOff = Color.Empty;

            switch (st)
            {
                case HSlideSwitchStyle.Fluent:
                    offC = Color.FromArgb(131, 133, 142); knobF = 0.56f; knobMF = 0.22f; knobShadow = true; break;
                case HSlideSwitchStyle.Metro:
                    offC = Color.FromArgb(209, 213, 219); rfac = 0.14f; knobF = 0.7f; break;
                case HSlideSwitchStyle.Bootstrap:
                    offC = Color.FromArgb(200, 206, 214); onC = Color.FromArgb(32, 164, 100);
                    knobShadow = true; break;
                case HSlideSwitchStyle.Material3:
                    offC = Color.FromArgb(192, 196, 204); onC = Light(acc, 0.22f);
                    knobF = 0.6f; knobMF = 0.2f; halo = 1; break;
                case HSlideSwitchStyle.Tactile:
                    offC = Color.FromArgb(154, 161, 172); fillMode = FmVert;
                    knobF = 0.8f; knobFill = KbGloss; knobShadow = true; break;
                case HSlideSwitchStyle.NeonPink:
                    offC = Color.FromArgb(34, 18, 36); onC = Color.FromArgb(42, 15, 38);
                    bc = Color.FromArgb(255, 61, 154); glowC = Color.FromArgb(255, 61, 154);
                    knobOff = Color.FromArgb(107, 114, 128); knobOn = Color.FromArgb(255, 119, 184);
                    border = BdRing; bw = Math.Max(1.2f, h * 0.06f); break;
                case HSlideSwitchStyle.NeonLime:
                    offC = Color.FromArgb(26, 32, 18); onC = Color.FromArgb(28, 38, 14);
                    bc = Color.FromArgb(182, 255, 46); glowC = Color.FromArgb(182, 255, 46);
                    knobOff = Color.FromArgb(107, 114, 128); knobOn = Color.FromArgb(217, 255, 138);
                    border = BdRing; bw = Math.Max(1.2f, h * 0.06f); break;
                case HSlideSwitchStyle.Aurora:
                    fillMode = FmDuo; duoA = Color.FromArgb(34, 211, 238); duoB = Color.FromArgb(139, 92, 246);
                    duoDark = Color.FromArgb(30, 41, 59); glowC = Color.FromArgb(34, 211, 238);
                    knobShadow = true; break;
                case HSlideSwitchStyle.Sunset:
                    fillMode = FmDuo; duoA = Color.FromArgb(255, 107, 129); duoB = Color.FromArgb(255, 176, 92);
                    duoDark = Color.FromArgb(42, 30, 42); knobShadow = true; break;
                case HSlideSwitchStyle.Emerald:
                    offC = Color.FromArgb(71, 85, 105); onC = Color.FromArgb(5, 150, 105);
                    glowC = Color.FromArgb(5, 150, 105); break;
                case HSlideSwitchStyle.Royal:
                    offC = Color.FromArgb(75, 68, 112); onC = Color.FromArgb(79, 70, 229);
                    knobOff = knobOn = Color.FromArgb(232, 197, 96); knobShadow = true; break;
                case HSlideSwitchStyle.Carbon:
                    offC = Color.FromArgb(22, 27, 34); onC = Color.FromArgb(13, 17, 23);
                    texture = 1; bc = Color.FromArgb(34, 211, 238);
                    border = BdRing; bw = Math.Max(1f, h * 0.045f);
                    knobOff = knobOn = Color.FromArgb(215, 222, 232); break;
                case HSlideSwitchStyle.Glass:
                    alphaFill = true; fillAlpha = 110;
                    border = BdRing; bw = Math.Max(1.1f, h * 0.05f); bc = Color.FromArgb(170, 255, 255, 255);
                    knobShadow = true; break;
                case HSlideSwitchStyle.Neumorph:
                    offC = onC = gray; neuEdge = true; knobF = 0.7f;
                    knobEdge = true; knobShadow = true; break;
                case HSlideSwitchStyle.Dashed:
                    border = BdDash; bw = Math.Max(1.4f, h * 0.06f); bc = acc;
                    knobOff = Color.FromArgb(154, 163, 178); knobOn = acc; break;
                case HSlideSwitchStyle.DoubleLine:
                    border = BdDouble; bw = Math.Max(1.1f, h * 0.05f); bc = acc; break;
                case HSlideSwitchStyle.GradientStroke:
                    border = BdGradient; bw = Math.Max(1.5f, h * 0.07f); bc = acc; bc2 = Color.FromArgb(249, 115, 22); break;
                case HSlideSwitchStyle.Synthwave:
                    offC = Color.FromArgb(27, 20, 48); onC = Color.FromArgb(18, 12, 38);
                    glowC = Color.FromArgb(255, 61, 154); knobFill = KbWave; knobF = 0.76f; break;
                case HSlideSwitchStyle.Terminal:
                    offC = Color.FromArgb(12, 21, 16); onC = Color.FromArgb(7, 18, 11);
                    texture = 2; border = BdRing; bw = Math.Max(1.1f, h * 0.05f); bc = Color.FromArgb(150, 34, 197, 94);
                    knobOff = knobOn = Color.FromArgb(6, 20, 13);
                    glyph = GlDot; glyphOff = glyphOn = Color.FromArgb(34, 197, 94); break;
                case HSlideSwitchStyle.Led:
                    offC = Color.FromArgb(29, 36, 48); onC = Color.FromArgb(17, 24, 39);
                    knobF = 0.46f; knobMF = 0.27f;
                    knobOff = Color.FromArgb(239, 68, 68); knobOn = Color.FromArgb(34, 197, 94);
                    halo = 3; haloC = Color.Empty; break;
                case HSlideSwitchStyle.Lock:
                    offC = Color.FromArgb(226, 230, 236); onC = Light(acc, 0.18f);
                    border = BdRing; bw = 1f; bc = Color.FromArgb(194, 200, 208);
                    knobShadow = true; glyph = GlLock;
                    glyphOff = Color.FromArgb(154, 163, 178); glyphOn = acc; break;
                case HSlideSwitchStyle.Heart:
                    offC = Color.FromArgb(229, 231, 235); onC = Color.FromArgb(249, 168, 196);
                    knobShadow = true; glyph = GlHeart;
                    glyphOff = Color.FromArgb(195, 202, 212); glyphOn = Color.FromArgb(239, 68, 68); break;
                case HSlideSwitchStyle.Star:
                    offC = Color.FromArgb(229, 231, 235); onC = Color.FromArgb(246, 196, 83);
                    knobShadow = true; glyph = GlStar;
                    glyphOff = Color.FromArgb(195, 202, 212); glyphOn = Color.FromArgb(245, 158, 11); break;
                case HSlideSwitchStyle.Cloud:
                    offC = Color.FromArgb(214, 222, 232); onC = Color.FromArgb(90, 185, 234);
                    knobShadow = true; glyph = GlCloud;
                    glyphOff = Color.FromArgb(154, 168, 184); glyphOn = Color.FromArgb(14, 165, 233); break;
                case HSlideSwitchStyle.Music:
                    offC = Color.FromArgb(214, 222, 232); onC = Color.FromArgb(139, 92, 246);
                    knobShadow = true; glyph = GlNote;
                    glyphOff = Color.FromArgb(154, 163, 178); glyphOn = Color.FromArgb(109, 40, 217); break;
                case HSlideSwitchStyle.Retro:
                    offC = Color.FromArgb(58, 58, 58); onC = Color.FromArgb(30, 158, 58);
                    rfac = 0.14f; border = BdRing; bw = Math.Max(2f, h * 0.09f); bc = Color.FromArgb(20, 20, 20);
                    knobSquare = true; knobEdge = true; knobF = 0.62f; knobMF = 0.19f;
                    knobOff = knobOn = Color.FromArgb(242, 235, 207); break;
                case HSlideSwitchStyle.Paper:
                    offC = onC = Color.White; knobF = 0.5f; knobMF = 0.25f;
                    knobOff = knobOn = Color.FromArgb(32, 38, 46); knobShadow = true; break;
                case HSlideSwitchStyle.Slate:
                    offC = Color.FromArgb(100, 116, 139); onC = Color.FromArgb(14, 165, 233); break;
                case HSlideSwitchStyle.Mono:
                    offC = Color.FromArgb(189, 189, 189); onC = Color.FromArgb(43, 43, 43); break;
                case HSlideSwitchStyle.Pulse:
                    offC = Color.FromArgb(216, 222, 232); onC = acc;
                    halo = 2; haloC = acc; knobF = 0.62f; knobMF = 0.19f; break;
                case HSlideSwitchStyle.Guard:
                    offC = Color.FromArgb(203, 213, 221); onC = Color.FromArgb(15, 143, 122);
                    knobShadow = true; glyph = GlShield;
                    glyphOff = Color.FromArgb(154, 163, 178); glyphOn = Color.FromArgb(13, 148, 136); break;
                case HSlideSwitchStyle.Nova:
                    offC = Color.FromArgb(17, 24, 39); onC = Color.FromArgb(11, 18, 32);
                    glowC = acc; knobFill = KbNova; knobEdge = true; knobF = 0.76f; break;
            }
            if (!enabled)
            {
                offC = Mix(offC, gray, 0.5f); onC = Mix(onC, gray, 0.5f);
                knobOff = Mix(knobOff, gray, 0.4f); knobOn = Mix(knobOn, gray, 0.4f);
                bc = Mix(bc, gray, 0.4f); glowC = Color.Empty;
                glyphOff = Mix(glyphOff, gray, 0.4f); glyphOn = Mix(glyphOn, gray, 0.4f);
            }

            // 轨道外框：整数边对齐控件边界，上下左右抗锯齿相位一致
            float vr = Math.Min(h / 2f, h * rfac);
            using (GraphicsPath track = RoundRect(t, vr))
            {
                #region 槽体填充
                Color trackC = Mix(offC, onC, k);
                if (alphaFill)
                    Fill(g, track, Color.FromArgb(fillAlpha, Color.White.R, Color.White.G, Color.White.B));
                else if (fillMode == FmDuo)
                {
                    Color a = Mix(duoA, duoDark, 1f - k), b = Mix(duoB, duoDark, 1f - k);
                    using (var br = new LinearGradientBrush(t, a, b, 0f)) g.FillPath(br, track);
                }
                else if (fillMode == FmVert)
                    using (var br = new LinearGradientBrush(t, Light(trackC, 0.18f), Dark(trackC, 0.08f), 90f)) g.FillPath(br, track);
                else
                    Fill(g, track, trackC);
                #endregion

                #region 槽内纹理
                if (texture == 1)
                {
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    using (var pen = new Pen(Color.FromArgb(26, 0, 0, 0), Math.Max(1f, h * 0.05f)))
                        for (float x = t.X - h; x < t.Right + h; x += h * 0.16f)
                            g.DrawLine(pen, x, t.Bottom, x + h, t.Top);
                    g.Clip = oldClip;
                }
                else if (texture == 2)
                {
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    using (var pen = new Pen(Color.FromArgb(28, 34, 197, 94), 1f))
                        for (float yy = t.Y + 2f; yy < t.Bottom; yy += 2f)
                            g.DrawLine(pen, t.X + h * 0.12f, yy, t.Right - h * 0.12f, yy);
                    g.Clip = oldClip;
                }
                #endregion

                #region 内发光
                if (glowC != Color.Empty)
                {
                    float str = 0.2f + 0.8f * k;
                    float[] gw = { h * 0.28f, h * 0.18f, h * 0.1f };
                    int[] ga = { 26, 46, 70 };
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    for (int i = 0; i < 3; i++)
                        using (var pen = new Pen(Color.FromArgb((int)(ga[i] * str), glowC), gw[i]))
                            g.DrawPath(pen, track);
                    g.Clip = oldClip;
                }
                #endregion

                #region 新拟态凹凸边
                if (neuEdge)
                {
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    using (var p1 = RoundRect(InsetRect(t, 0.8f), Math.Max(0f, vr - 0.8f)))
                    using (var p2 = RoundRect(InsetRect(t, 2.2f), Math.Max(0f, vr - 2.2f)))
                    {
                        using (var dpen = new Pen(Color.FromArgb(46, 0, 0, 0), 1.4f)) g.DrawPath(dpen, p1);
                        using (var lpen = new Pen(Color.FromArgb(180, 255, 255, 255), 1.4f)) g.DrawPath(lpen, p2);
                    }
                    g.Clip = oldClip;
                }
                #endregion

                #region 槽体描边
                if (border == BdRing || border == BdGradient)
                {
                    using (GraphicsPath inner = RoundRect(InsetRect(t, bw), Math.Max(0f, vr - bw)))
                    using (var ring = new GraphicsPath(FillMode.Alternate))
                    {
                        ring.AddPath(track, false);
                        ring.AddPath(inner, false);
                        Brush br = border == BdGradient
                            ? new LinearGradientBrush(t, bc, bc2, 0f)
                            : (Brush)new SolidBrush(bc);
                        using (br) g.FillPath(br, ring);
                    }
                }
                else if (border == BdDash)
                {
                    float di = bw * 0.9f;
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    using (GraphicsPath pl = RoundRect(InsetRect(t, di), Math.Max(0f, vr - di)))
                    using (var pen = new Pen(Mix(bc, Color.White, 0.6f * k), bw * 0.75f)
                    {
                        DashStyle = DashStyle.Custom,
                        DashPattern = new[] { 4f, 3f },
                        StartCap = LineCap.Round, EndCap = LineCap.Round
                    })
                        g.DrawPath(pen, pl);
                    g.Clip = oldClip;
                }
                else if (border == BdDouble)
                {
                    float gap = Math.Max(1.5f, h * 0.07f);
                    using (GraphicsPath inner = RoundRect(InsetRect(t, bw), Math.Max(0f, vr - bw)))
                    using (GraphicsPath o2 = RoundRect(InsetRect(t, bw + gap), Math.Max(0f, vr - bw - gap)))
                    using (GraphicsPath i2 = RoundRect(InsetRect(t, bw + gap + 1f), Math.Max(0f, vr - bw - gap - 1f)))
                    using (var ring = new GraphicsPath(FillMode.Alternate))
                    using (var br = new SolidBrush(Mix(bc, Color.White, 0.6f * k)))
                    {
                        ring.AddPath(track, false); ring.AddPath(inner, false);
                        g.FillPath(br, ring);
                        ring.Reset();
                        ring.AddPath(o2, false); ring.AddPath(i2, false);
                        g.FillPath(br, ring);
                    }
                }
                #endregion

                #region 圆钮
                float margin = Math.Max(2f, h * knobMF);
                float d = Math.Min(h - 2 * margin, h * knobF);
                float travel = w - 2 * margin - d;
                float kx = t.X + margin + Math.Max(0f, travel) * k;
                float ky = t.Y + (h - d) / 2f;
                var kr = new RectangleF(kx, ky, d, d);

                if (knobShadow)
                {
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    using (var sb = new SolidBrush(Color.FromArgb(34, 0, 0, 0)))
                        g.FillEllipse(sb, kr.X, kr.Y + Math.Max(1f, h * 0.05f), d, d);
                    g.Clip = oldClip;
                }

                Color kc = Mix(knobOff, knobOn, k);

                // 光环/涟漪/LED 辉光：一律裁剪在圆角轨道内，钮停靠两端也不溢出框外
                if (halo == 1 || halo == 2)
                {
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    if (halo == 1)
                    {
                        float a = 0.15f + 0.45f * k;
                        using (var pen = new Pen(Color.FromArgb((int)(150 * a), haloC), Math.Max(1.5f, h * 0.09f)))
                            g.DrawEllipse(pen, RectangleF.Inflate(kr, d * 0.16f, d * 0.16f));
                        using (var pen = new Pen(Color.FromArgb((int)(90 * a), haloC), h * 0.16f))
                            g.DrawEllipse(pen, RectangleF.Inflate(kr, d * 0.18f, d * 0.18f));
                    }
                    else
                    {
                        float a = 0.25f + 0.5f * k;
                        foreach (float e in new[] { 0.14f, 0.28f })
                            using (var pen = new Pen(Color.FromArgb((int)(70 * a), haloC), Math.Max(1f, h * 0.045f)))
                                g.DrawEllipse(pen, RectangleF.Inflate(kr, d * e, d * e));
                    }
                    g.Clip = oldClip;
                }
                else if (halo == 3)
                {
                    Color hc = kc;
                    var oldClip = g.Clip;
                    g.SetClip(track);
                    foreach (float gw in new[] { h * 0.22f, h * 0.13f, h * 0.07f })
                        using (var pen = new Pen(Color.FromArgb(gw > h * 0.15f ? 34 : gw > h * 0.09f ? 60 : 110, hc), gw))
                            g.DrawEllipse(pen, kr);
                    g.Clip = oldClip;
                }

                var kb = new RectangleF(kx + 0.5f, ky + 0.5f, d - 1f, d - 1f);
                if (knobSquare)
                {
                    using (GraphicsPath kp = KnobRound(kr, d * 0.18f))
                    {
                        Fill(g, kp, kc);
                        if (knobEdge)
                            using (var pen = new Pen(Color.FromArgb(20, 20, 20), Math.Max(1.2f, h * 0.05f))) g.DrawPath(pen, kp);
                    }
                }
                else if (knobFill == KbGloss)
                {
                    using (var br = new LinearGradientBrush(kr, Light(kc, 0.42f), Dark(kc, 0.06f), 90f))
                        g.FillEllipse(br, kb);
                    using (var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f)) g.DrawEllipse(pen, kb);
                }
                else if (knobFill == KbWave)
                {
                    // 关态钮面收敛为中性灰，开态亮起品红-青色波
                    Color wvA = Mix(Color.FromArgb(96, 102, 116), Color.FromArgb(255, 61, 154), k);
                    Color wvB = Mix(Color.FromArgb(96, 102, 116), Color.FromArgb(34, 211, 238), k);
                    using (var br = new LinearGradientBrush(kr, wvA, wvB, 90f))
                        g.FillEllipse(br, kb);
                }
                else if (knobFill == KbNova)
                {
                    // 关态钮面为暗石板色，开态点亮主色高光
                    Color nvA = Mix(Color.FromArgb(58, 70, 92), Light(acc, 0.45f), k);
                    Color nvB = Mix(Color.FromArgb(22, 30, 46), Dark(acc, 0.15f), k);
                    using (var br = new LinearGradientBrush(kr, nvA, nvB, 90f))
                        g.FillEllipse(br, kb);
                    using (var pen = new Pen(Color.FromArgb((int)(60 + 90 * k), 255, 255, 255), 1f)) g.DrawEllipse(pen, kb);
                }
                else
                    using (var brsh = new SolidBrush(kc)) g.FillEllipse(brsh, kb);

                // 同色槽面上的新拟态圆钮：柔灰描边分离钮与槽
                if (knobEdge && !knobSquare && knobFill == KbSolid)
                    using (var pen = new Pen(Color.FromArgb(150, 150, 150, 150), 1f)) g.DrawEllipse(pen, kb);

                if (glyph >= 0 && glyph != GlDot)
                {
                    var gr = new RectangleF(kx + d * 0.24f, ky + d * 0.24f, d * 0.52f, d * 0.52f);
                    Color gc = Mix(glyphOff, glyphOn, k);
                    DrawExtraGlyph(g, gr, glyph, gc, d);
                }
                else if (glyph == GlDot)
                {
                    using (var brsh = new SolidBrush(glyphOn))
                        g.FillEllipse(brsh, kx + d * 0.34f, ky + d * 0.34f, d * 0.32f, d * 0.32f);
                }
                #endregion
            }
        }

        private static RectangleF InsetRect(RectangleF r, float d)
            => new RectangleF(r.X + d, r.Y + d, r.Width - 2f * d, r.Height - 2f * d);

        private static GraphicsPath RoundRect(RectangleF r, float visualR)
            => HDrawPaint.CreatePath(r, visualR * 2f, HRoundStyle.All, false);

        private static void DrawExtraGlyph(Graphics g, RectangleF r, int glyph, Color c, float d)
        {
            float x = r.X, y = r.Y, ww = r.Width, hh = r.Height;
            float cx = x + ww / 2f;
            using (var br = new SolidBrush(c))
            {
                if (glyph == GlHeart)
                {
                    using (var p = new GraphicsPath())
                    {
                        var tip = new PointF(cx, y + hh * 0.94f);
                        p.AddBezier(tip,
                            new PointF(x + ww * 0.02f, y + hh * 0.55f),
                            new PointF(x + ww * 0.04f, y + hh * 0.02f),
                            new PointF(cx, y + hh * 0.3f));
                        p.AddBezier(new PointF(cx, y + hh * 0.3f),
                            new PointF(x + ww * 0.96f, y + hh * 0.02f),
                            new PointF(x + ww * 0.98f, y + hh * 0.55f),
                            tip);
                        p.CloseFigure();
                        g.FillPath(br, p);
                    }
                }
                else if (glyph == GlStar)
                {
                    var pts = new PointF[10];
                    for (int i = 0; i < 10; i++)
                    {
                        double a = -Math.PI / 2 + i * Math.PI / 5;
                        float rr = i % 2 == 0 ? ww * 0.5f : ww * 0.22f;
                        pts[i] = new PointF(cx + (float)Math.Cos(a) * rr, y + hh / 2f + (float)Math.Sin(a) * rr);
                    }
                    g.FillPolygon(br, pts);
                }
                else if (glyph == GlCloud)
                {
                    using (var p = new GraphicsPath())
                    {
                        p.AddEllipse(x + ww * 0.14f, y + hh * 0.38f, ww * 0.38f, hh * 0.38f);
                        p.AddEllipse(x + ww * 0.34f, y + hh * 0.2f, ww * 0.42f, hh * 0.42f);
                        p.AddEllipse(x + ww * 0.54f, y + hh * 0.38f, ww * 0.32f, hh * 0.32f);
                        p.AddRectangle(new RectangleF(x + ww * 0.18f, y + hh * 0.52f, ww * 0.62f, hh * 0.24f));
                        g.FillPath(br, p);
                    }
                }
                else if (glyph == GlNote)
                {
                    g.FillEllipse(br, new RectangleF(x + ww * 0.18f, y + hh * 0.6f, ww * 0.36f, hh * 0.28f));
                    g.FillRectangle(br, new RectangleF(x + ww * 0.5f, y + hh * 0.1f, ww * 0.1f, hh * 0.58f));
                    g.FillPolygon(br, new[]
                    {
                        new PointF(x + ww * 0.6f, y + hh * 0.1f),
                        new PointF(x + ww * 0.84f, y + hh * 0.18f),
                        new PointF(x + ww * 0.84f, y + hh * 0.34f),
                        new PointF(x + ww * 0.6f, y + hh * 0.26f)
                    });
                }
                else if (glyph == GlShield)
                {
                    g.FillPolygon(br, new[]
                    {
                        new PointF(x + ww * 0.12f, y + hh * 0.2f),
                        new PointF(x + ww * 0.88f, y + hh * 0.2f),
                        new PointF(x + ww * 0.88f, y + hh * 0.52f),
                        new PointF(cx, y + hh * 0.92f),
                        new PointF(x + ww * 0.12f, y + hh * 0.52f)
                    });
                    DrawCheck(g, new RectangleF(x + ww * 0.28f, y + hh * 0.32f, ww * 0.44f, hh * 0.4f),
                        Color.White, Math.Max(1.2f, d * 0.09f));
                }
                else if (glyph == GlLock)
                {
                    g.FillRectangle(br, new RectangleF(x + ww * 0.24f, y + hh * 0.48f, ww * 0.52f, hh * 0.4f));
                    using (var pen = new Pen(c, Math.Max(1.4f, d * 0.1f)))
                        g.DrawArc(pen, new RectangleF(x + ww * 0.3f, y + hh * 0.14f, ww * 0.4f, hh * 0.44f), 180f, 180f);
                    g.FillEllipse(br, new RectangleF(cx - ww * 0.05f, y + hh * 0.6f, ww * 0.1f, hh * 0.1f));
                }
            }
        }
        #endregion

        #region 几何

        private static float KnobMargin(HSlideSwitchStyle st, float h)
        {
            switch (st)
            {
                case HSlideSwitchStyle.Bold: return Math.Max(1f, h * 0.06f);
                case HSlideSwitchStyle.Windows: return Math.Max(1.5f, h * 0.08f);
                case HSlideSwitchStyle.Thin:
                case HSlideSwitchStyle.Rubber: return h * 0.07f;
                case HSlideSwitchStyle.Ring: return h * 0.13f;
                case HSlideSwitchStyle.Dot: return h * 0.12f;
                default: return Math.Max(2f, h * 0.12f);
            }
        }

        private static float KnobD(HSlideSwitchStyle st, RectangleF t, RectangleF ti)
        {
            float h = t.Height;
            switch (st)
            {
                case HSlideSwitchStyle.Thin: return h * 0.82f;
                case HSlideSwitchStyle.Rubber: return h * 0.94f;
                case HSlideSwitchStyle.Dot: return h * 0.44f;
                default: return ti.Height - 2 * KnobMargin(st, h);
            }
        }

        /// <summary>圆钮停靠侧的槽内符号区（onRight=true 符号在左、圆钮在右），内收避开端弧。</summary>
        private static RectangleF SideZone(RectangleF t, bool onRight, float zw)
        {
            float h = t.Height;
            float x = onRight ? t.X + h * 0.16f : t.Right - h * 0.16f - zw;
            return new RectangleF(x, t.Y, zw, h);
        }
        #endregion

        #region 路径与颜色原语

        /// <summary>轨道外轮廓像素盒：(-0.5 偏移)，与 CreateOuterBoxPath 同口径。</summary>
        private static RectangleF OuterBox(RectangleF r)
            => new RectangleF(r.X - 0.5f, r.Y - 0.5f, r.Width, r.Height);

        /// <summary>描边环的内轮廓像素盒（环厚全部向内）。</summary>
        private static RectangleF InnerBox(RectangleF r, float pw)
            => new RectangleF(r.X - 0.5f + pw, r.Y - 0.5f + pw, r.Width - 2f * pw, r.Height - 2f * pw);

        /// <summary>轨道圆角/胶囊路径：visualR 为视觉圆角半径（胶囊为盒高一半）。</summary>
        private static GraphicsPath TrackBox(RectangleF r, float visualR)
            => HDrawPaint.CreatePath(OuterBox(r), visualR + 0.5f, HRoundStyle.All, false);

        private static GraphicsPath KnobPill(RectangleF r)
            => HDrawPaint.CreatePath(OuterBox(r), r.Height + 0.5f, HRoundStyle.All, false);

        private static GraphicsPath KnobRound(RectangleF r, float visualR)
            => HDrawPaint.CreatePath(OuterBox(r), visualR + 0.5f, HRoundStyle.All, false);

        /// <summary>切角六边形顶点（顺时针，y 向下）：左右两端为尖点，上下边各切去高度的 0.22。</summary>
        private static PointF[] ChamferPoints(RectangleF r)
        {
            float c = r.Height * 0.22f;
            return new[]
            {
                new PointF(r.X + c, r.Y), new PointF(r.Right - c, r.Y),
                new PointF(r.Right, r.Y + r.Height / 2),
                new PointF(r.Right - c, r.Bottom), new PointF(r.X + c, r.Bottom),
                new PointF(r.X, r.Y + r.Height / 2)
            };
        }

        private static GraphicsPath PolygonPath(PointF[] pts)
        {
            var p = new GraphicsPath();
            p.AddPolygon(pts);
            return p;
        }

        /// <summary>
        /// 凸多边形等距内缩：每条边沿内向法线平移 d，相邻平移线相交得到新顶点，
        /// 内外对应边严格平行、法向距离处处为 d，切角斜边与尖点处环宽一致。
        /// </summary>
        private static PointF[] InsetPolygon(PointF[] pts, float d)
        {
            int n = pts.Length;
            var res = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                PointF n1 = InwardNormal(pts[(i - 1 + n) % n], pts[i]);
                PointF n2 = InwardNormal(pts[i], pts[(i + 1) % n]);
                float c1 = n1.X * pts[i].X + n1.Y * pts[i].Y + d;
                float c2 = n2.X * pts[i].X + n2.Y * pts[i].Y + d;
                float det = n1.X * n2.Y - n1.Y * n2.X;
                res[i] = new PointF((c1 * n2.Y - n1.Y * c2) / det, (n1.X * c2 - c1 * n2.X) / det);
            }
            return res;
        }

        /// <summary>顺时针多边形（y 向下）的边内向单位法线：边方向 (dx,dy) 的右法线。</summary>
        private static PointF InwardNormal(PointF a, PointF b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            return new PointF(-dy / len, dx / len);
        }

        private static void Fill(Graphics g, GraphicsPath p, Color c)
        {
            using (var b = new SolidBrush(c)) g.FillPath(b, p);
        }

        private static void Line(Graphics g, float x1, float y1, float x2, float y2, Color c, float w = 1f)
        {
            using (var pen = new Pen(c, w) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(pen, x1, y1, x2, y2);
        }

        private static Color Mix(Color a, Color b, float t) => HPushButtonScheme.Mix(a, b, t);
        private static Color Light(Color c, float t) => Mix(c, Color.White, t);
        private static Color Dark(Color c, float t) => HPushButtonScheme.Shade(c, t);
        #endregion

        #region 符号原语

        private static void DrawCheck(Graphics g, RectangleF r, Color c, float pw)
        {
            using (var pen = new Pen(c, pw) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                var p1 = new PointF(r.X + r.Width * 0.18f, r.Y + r.Height * 0.52f);
                var p2 = new PointF(r.X + r.Width * 0.42f, r.Y + r.Height * 0.76f);
                var p3 = new PointF(r.X + r.Width * 0.84f, r.Y + r.Height * 0.24f);
                g.DrawLines(pen, new[] { p1, p2, p3 });
            }
        }

        private static void DrawCross(Graphics g, RectangleF r, Color c, float pw)
        {
            using (var pen = new Pen(c, pw) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(pen, r.X + r.Width * 0.24f, r.Y + r.Height * 0.24f,
                    r.Right - r.Width * 0.24f, r.Bottom - r.Height * 0.24f);
                g.DrawLine(pen, r.Right - r.Width * 0.24f, r.Y + r.Height * 0.24f,
                    r.X + r.Width * 0.24f, r.Bottom - r.Height * 0.24f);
            }
        }

        private static void DrawPower(Graphics g, RectangleF r, Color c, float pw)
        {
            using (var pen = new Pen(c, pw) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var path = new GraphicsPath())
            {
                var arc = RectangleF.Inflate(r, -r.Width * 0.06f, -r.Height * 0.06f);
                path.AddArc(arc, -58f, 296f);
                g.DrawPath(pen, path);
                g.DrawLine(pen, r.X + r.Width / 2, r.Y + r.Height * 0.08f,
                    r.X + r.Width / 2, r.Y + r.Height * 0.5f);
            }
        }

        private static void DrawMoon(Graphics g, RectangleF r, Color c)
        {
            using (var path = new GraphicsPath(FillMode.Alternate))
            {
                path.AddEllipse(r);
                path.AddEllipse(r.X + r.Width * 0.24f, r.Y - r.Height * 0.05f, r.Width, r.Height);
                using (var b = new SolidBrush(c)) g.FillPath(b, path);
            }
        }

        private static void DrawSun(Graphics g, RectangleF r, float h)
        {
            float cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
            float rr = r.Width * 0.3f;
            using (var b = new SolidBrush(Color.FromArgb(245, 158, 11)))
                g.FillEllipse(b, cx - rr, cy - rr, rr * 2, rr * 2);
            float ray = rr * 0.55f, gap = rr * 0.28f;
            using (var pen = new Pen(Color.FromArgb(245, 158, 11), Math.Max(1.2f, h * 0.05f))
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4;
                    float x1 = cx + (float)Math.Cos(a) * (rr + gap);
                    float y1 = cy + (float)Math.Sin(a) * (rr + gap);
                    float x2 = cx + (float)Math.Cos(a) * (rr + gap + ray);
                    float y2 = cy + (float)Math.Sin(a) * (rr + gap + ray);
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }
        #endregion
    }
}
