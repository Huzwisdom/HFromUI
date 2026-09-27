using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using HFromUI.HEnum;
using HFromUI.HMath;

namespace HFromUI.HControl.Tools.Button
{
    /// <summary>
    /// HCheckBox/HRadioButton 勾选指示符的统一矢量渲染：每种 HIndicatorStyle 解析为一组配方
    /// （外形 + 填充 + 描边 + 内部符号 + 光效 + 固定配色），由同一条绘制管线输出。
    /// </summary>
    internal static class HIndicatorRenderer
    {
        // 外形
        private const int ShRound = 0, ShSquare = 1, ShCircle = 2, ShChamfer = 3, ShDiamond = 4, ShHexagon = 5, ShGlyph = 6;
        // 填充
        private const int FlNone = 0, FlSolid = 1, FlVert = 2, FlDuo = 3, FlDark = 4, FlGlass = 5, FlNeu = 6;
        // 描边
        private const int EdNone = 0, EdRing = 1, EdBold = 2, EdDash = 3, EdDouble = 4, EdGradient = 5;
        // 内部符号
        private const int MkNone = 0, MkCheck = 1, MkDot = 2, MkCross = 3, MkPlus = 4;
        // 独立符号（无外框）
        private const int GlStar = 1, GlHeart = 2, GlBolt = 3, GlFlag = 4, GlBookmark = 5,
                           GlShield = 6, GlLock = 7, GlEye = 8, GlSun = 9, GlMoon = 10, GlCloud = 11, GlNote = 12;

        private struct Recipe
        {
            public int Shape, Fill, Edge, Mark, Glyph;
            public bool Halo, Pulse, Persist, AccentMark;
            public Color FillA, FillB, FixedEdge, FixedMark, OffEdge;
        }

        public static void Draw(Graphics g, Rectangle rect, HIndicatorStyle st, bool on, Color accent, bool enabled)
        {
            Recipe r = Resolve(st);
            Color gray = Color.FromArgb(156, 163, 175);
            if (!enabled)
            {
                accent = Mix(accent, SystemColors.Control, 0.55f);
                gray = Color.FromArgb(203, 213, 225);
                r.FillA = r.FillA == Color.Empty ? Color.Empty : Mix(r.FillA, SystemColors.Control, 0.5f);
                r.FillB = r.FillB == Color.Empty ? Color.Empty : Mix(r.FillB, SystemColors.Control, 0.5f);
                r.FixedEdge = r.FixedEdge == Color.Empty ? Color.Empty : Mix(r.FixedEdge, SystemColors.Control, 0.5f);
                r.FixedMark = r.FixedMark == Color.Empty ? Color.Empty : Mix(r.FixedMark, SystemColors.Control, 0.5f);
                r.Halo = r.Pulse = false;
            }

            if (r.Shape == ShGlyph)
            {
                DrawGlyph(g, rect, r.Glyph, on ? (r.FillA == Color.Empty ? accent : r.FillA) : gray);
                return;
            }

            float s = rect.Width;
            var bf = new RectangleF(rect.X + 0.5f, rect.Y + 0.5f, s - 1f, s - 1f);
            using (GraphicsPath path = ShapePath(bf, s, r.Shape))
            {
                Color edgeOn = r.FixedEdge == Color.Empty ? accent : r.FixedEdge;
                Color markC = r.FixedMark == Color.Empty ? (r.AccentMark ? accent : Color.White) : r.FixedMark;

                if (on && r.Halo) Halo(g, path, edgeOn, s, 1f);

                #region 填充
                bool persistFill = r.Persist || r.Fill == FlDark || r.Fill == FlGlass || r.Fill == FlNeu;
                if (on || persistFill)
                {
                    if (r.Fill == FlVert)
                        using (var br = new LinearGradientBrush(bf, Light(accent, 0.18f), Dark(accent, 0.08f), 90f))
                            g.FillPath(br, path);
                    else if (r.Fill == FlDuo && r.FillA != Color.Empty && r.FillB != Color.Empty)
                        using (var br = new LinearGradientBrush(bf, r.FillA, r.FillB, 45f))
                            g.FillPath(br, path);
                    else if (r.Fill == FlDark)
                        Fill(g, path, on ? Color.FromArgb(15, 23, 42) : Color.FromArgb(24, 28, 38));
                    else if (r.Fill == FlGlass)
                        Fill(g, path, Color.FromArgb(110, 255, 255, 255));
                    else if (r.Fill == FlNeu)
                        Fill(g, path, on ? accent : SystemColors.Control);
                    else if (r.Fill != FlNone && on)
                        Fill(g, path, r.FillA == Color.Empty ? accent : r.FillA);
                }
                #endregion

                if (r.Fill == FlNeu && !on) NeuEdge(g, bf, s, r.Shape);

                #region 描边
                if (r.Edge != EdNone)
                {
                    if (r.Edge == EdDouble)
                    {
                        DrawEdgeRing(g, bf, s, r.Shape, on ? edgeOn : gray, Math.Max(1.1f, s * 0.07f));
                        var b2 = Inset(bf, Math.Max(3f, s * 0.2f));
                        using (GraphicsPath p2 = ShapePath(b2, b2.Width, r.Shape))
                            using (var pen = new Pen(on ? edgeOn : gray, Math.Max(1f, s * 0.06f)))
                                g.DrawPath(pen, p2);
                    }
                    else if (r.Edge == EdGradient)
                    {
                        Color e2 = on ? Color.FromArgb(249, 115, 22) : gray;
                        float ew = Math.Max(1.5f, s * 0.1f);
                        using (GraphicsPath inner = ShapePath(Inset(bf, ew), s - 2f * ew, r.Shape))
                        using (var ring = new GraphicsPath(FillMode.Alternate))
                        using (var br = new LinearGradientBrush(bf, on ? edgeOn : gray, e2, 45f))
                        {
                            ring.AddPath(path, false);
                            ring.AddPath(inner, false);
                            g.FillPath(br, ring);
                        }
                    }
                    else if (r.Edge == EdDash)
                    {
                        var b2 = Inset(bf, s * 0.06f);
                        // 显式连线闭合的连续路径：自定义虚线相位沿整周连续，不会在隐式连接段重置
                        using (GraphicsPath p2 = DashRoundPath(b2, b2.Width * 0.2f))
                        using (var pen = new Pen(on ? edgeOn : gray, Math.Max(1.2f, s * 0.07f))
                        {
                            DashStyle = DashStyle.Custom,
                            DashPattern = new[] { s * 0.16f, s * 0.13f }
                        })
                            g.DrawPath(pen, p2);
                    }
                    else
                    {
                        float ew = r.Edge == EdBold ? Math.Max(2f, s * 0.16f) : Math.Max(1.3f, s * 0.09f);
                        Color ec = on ? edgeOn : (r.OffEdge == Color.Empty ? gray : r.OffEdge);
                        using (var pen = new Pen(ec, ew) { Alignment = PenAlignment.Center })
                            g.DrawPath(pen, path);
                    }
                }
                #endregion

                if (on && r.Pulse)
                    foreach (var e in new[] { 0.14f, 0.28f })
                        using (var pen = new Pen(Color.FromArgb(e < 0.2f ? 60 : 34, edgeOn), Math.Max(1f, s * 0.05f)))
                            g.DrawEllipse(pen, RectangleF.Inflate(bf, s * e, s * e));

                #region 内部符号
                if (on && r.Mark != MkNone)
                {
                    var mr = new RectangleF(rect.X, rect.Y, s, s);
                    if (r.Mark == MkDot)
                        using (var br = new SolidBrush(markC))
                        {
                            var d = Rectangle.Inflate(rect, -(int)Math.Round(s * 0.27f), -(int)Math.Round(s * 0.27f));
                            g.FillEllipse(br, d);
                        }
                    else if (r.Mark == MkCross || r.Mark == MkPlus)
                        using (var pen = new Pen(markC, Math.Max(1.6f, s * 0.12f))
                        { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        {
                            float a = s * 0.3f, b = s * 0.7f;
                            if (r.Mark == MkCross)
                            {
                                g.DrawLine(pen, rect.X + a, rect.Y + a, rect.X + b, rect.Y + b);
                                g.DrawLine(pen, rect.X + b, rect.Y + a, rect.X + a, rect.Y + b);
                            }
                            else
                            {
                                g.DrawLine(pen, rect.X + a, rect.Y + s / 2f, rect.X + b, rect.Y + s / 2f);
                                g.DrawLine(pen, rect.X + s / 2f, rect.Y + a, rect.X + s / 2f, rect.Y + b);
                            }
                        }
                    else
                        using (var pen = new Pen(markC, Math.Max(1.6f, s * 0.12f))
                        { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                            g.DrawLines(pen, new[]
                            {
                                new PointF(mr.X + s * 0.22f, mr.Y + s * 0.52f),
                                new PointF(mr.X + s * 0.42f, mr.Y + s * 0.72f),
                                new PointF(mr.X + s * 0.78f, mr.Y + s * 0.30f)
                            });
                }
                #endregion
            }
        }

        private static Recipe Resolve(HIndicatorStyle st)
        {
            var r = new Recipe { Shape = ShRound, Fill = FlSolid, Edge = EdRing, Mark = MkCheck };
            switch (st)
            {
                case HIndicatorStyle.Classic: break;
                case HIndicatorStyle.RingDot:
                    r.Shape = ShCircle; r.Fill = FlNone; r.Mark = MkDot; r.AccentMark = true; break;
                case HIndicatorStyle.Square:
                    r.Shape = ShSquare; break;
                case HIndicatorStyle.CircleCheck:
                    r.Shape = ShCircle; break;
                case HIndicatorStyle.Outline:
                    r.Fill = FlNone; r.AccentMark = true; break;
                case HIndicatorStyle.Bold:
                    r.Fill = FlNone; r.Edge = EdBold; r.AccentMark = true; break;
                case HIndicatorStyle.Dot:
                    r.Mark = MkDot; break;
                case HIndicatorStyle.FilledDot:
                    r.Shape = ShCircle; r.Mark = MkDot; break;
                case HIndicatorStyle.Cross:
                    r.Mark = MkCross; break;
                case HIndicatorStyle.Plus:
                    r.Mark = MkPlus; break;
                case HIndicatorStyle.Diamond:
                    r.Shape = ShDiamond; r.Mark = MkDot; break;
                case HIndicatorStyle.Hexagon:
                    r.Shape = ShHexagon; break;
                case HIndicatorStyle.Dashed:
                    r.Fill = FlNone; r.Edge = EdDash; r.AccentMark = true; break;
                case HIndicatorStyle.DoubleLine:
                    r.Shape = ShCircle; r.Fill = FlNone; r.Edge = EdDouble; r.Mark = MkDot; r.AccentMark = true; break;
                case HIndicatorStyle.Gradient:
                    r.Fill = FlVert; break;
                case HIndicatorStyle.GradientRing:
                    r.Shape = ShCircle; r.Fill = FlNone; r.Edge = EdGradient; r.Mark = MkDot; r.AccentMark = true; break;
                case HIndicatorStyle.Glow:
                    r.Halo = true; break;
                case HIndicatorStyle.Neon:
                    r.Fill = FlDark; r.Persist = true; r.Halo = true; r.AccentMark = true;
                    r.OffEdge = Color.FromArgb(71, 85, 105); break;
                case HIndicatorStyle.Cyber:
                    r.Shape = ShChamfer; r.Halo = true; break;
                case HIndicatorStyle.Glass:
                    r.Fill = FlGlass; r.Persist = true;
                    // 白描边在白底上会消失，混入主色保证可辨
                    r.FixedEdge = Color.FromArgb(150, 180, 200, 230); r.AccentMark = true; break;
                case HIndicatorStyle.Carbon:
                    r.Fill = FlDark; r.Persist = true;
                    r.FixedEdge = Color.FromArgb(34, 211, 238); r.FixedMark = Color.FromArgb(34, 211, 238);
                    r.OffEdge = Color.FromArgb(55, 70, 82); break;
                case HIndicatorStyle.Fluent:
                    r.OffEdge = Color.FromArgb(131, 133, 142); break;
                case HIndicatorStyle.Material3:
                    r.Fill = FlNone; r.Edge = EdBold; r.AccentMark = true; break;
                case HIndicatorStyle.Pulse:
                    r.Shape = ShCircle; r.Fill = FlNone; r.Mark = MkDot; r.AccentMark = true; r.Pulse = true; break;
                case HIndicatorStyle.MonoLine:
                    r.Fill = FlNone; r.AccentMark = true;
                    r.FixedEdge = Color.FromArgb(63, 63, 70); r.FixedMark = Color.FromArgb(63, 63, 70);
                    r.OffEdge = Color.FromArgb(156, 163, 175); break;
                case HIndicatorStyle.Emerald:
                    r.FillA = Color.FromArgb(5, 150, 105); break;
                case HIndicatorStyle.Royal:
                    r.FillA = Color.FromArgb(79, 70, 229); break;
                case HIndicatorStyle.Bootstrap:
                    r.FillA = Color.FromArgb(32, 164, 100); break;
                case HIndicatorStyle.Sunset:
                    r.Fill = FlDuo; r.FillA = Color.FromArgb(255, 107, 129); r.FillB = Color.FromArgb(255, 176, 92); break;
                case HIndicatorStyle.Aurora:
                    r.Fill = FlDuo; r.FillA = Color.FromArgb(34, 211, 238); r.FillB = Color.FromArgb(139, 92, 246); break;
                case HIndicatorStyle.Neumorph:
                    r.Fill = FlNeu; r.Persist = true; break;
                case HIndicatorStyle.Star:
                    r.Shape = ShGlyph; r.Glyph = GlStar; r.FillA = Color.FromArgb(245, 158, 11); break;
                case HIndicatorStyle.Heart:
                    r.Shape = ShGlyph; r.Glyph = GlHeart; r.FillA = Color.FromArgb(239, 68, 68); break;
                case HIndicatorStyle.Bolt:
                    r.Shape = ShGlyph; r.Glyph = GlBolt; break;
                case HIndicatorStyle.Flag:
                    r.Shape = ShGlyph; r.Glyph = GlFlag; break;
                case HIndicatorStyle.Bookmark:
                    r.Shape = ShGlyph; r.Glyph = GlBookmark; break;
                case HIndicatorStyle.Shield:
                    r.Shape = ShGlyph; r.Glyph = GlShield; r.FillA = Color.FromArgb(15, 143, 122); break;
                case HIndicatorStyle.Lock:
                    r.Shape = ShGlyph; r.Glyph = GlLock; break;
                case HIndicatorStyle.Eye:
                    r.Shape = ShGlyph; r.Glyph = GlEye; break;
                case HIndicatorStyle.Sun:
                    r.Shape = ShGlyph; r.Glyph = GlSun; break;
                case HIndicatorStyle.Moon:
                    r.Shape = ShGlyph; r.Glyph = GlMoon; break;
                case HIndicatorStyle.Cloud:
                    r.Shape = ShGlyph; r.Glyph = GlCloud; r.FillA = Color.FromArgb(14, 165, 233); break;
                case HIndicatorStyle.Music:
                    r.Shape = ShGlyph; r.Glyph = GlNote; r.FillA = Color.FromArgb(139, 92, 246); break;
            }
            return r;
        }

        #region 几何与外框
        private static RectangleF Inset(RectangleF r, float d)
            => new RectangleF(r.X + d, r.Y + d, r.Width - 2f * d, r.Height - 2f * d);

        private static GraphicsPath ShapePath(RectangleF r, float s, int shape)
        {
            var p = new GraphicsPath();
            switch (shape)
            {
                case ShCircle:
                    p.AddEllipse(r);
                    break;
                case ShSquare:
                    p.AddRectangle(r);
                    break;
                case ShChamfer:
                    p.AddPolygon(Poly(r, new[] { 0.22f, 0f, 0.78f, 0f, 1f, 0.5f, 0.78f, 1f, 0.22f, 1f, 0f, 0.5f }));
                    break;
                case ShDiamond:
                    p.AddPolygon(Poly(r, new[] { 0.5f, 0f, 1f, 0.5f, 0.5f, 1f, 0f, 0.5f }));
                    break;
                case ShHexagon:
                    p.AddPolygon(Poly(r, new[] { 0.5f, 0f, 1f, 0.25f, 1f, 0.75f, 0.5f, 1f, 0f, 0.75f, 0f, 0.25f }));
                    break;
                default:
                    p.Dispose();
                    return HDrawPaint.CreatePath(r, s * 0.2f, HRoundStyle.All, false);
            }
            return p;
        }

        /// <summary>四段圆弧 + 四条直线显式连成的单一闭合图元，供虚线描边沿整周连续排布。</summary>
        private static GraphicsPath DashRoundPath(RectangleF r, float d)
        {
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180f, 90f);
            p.AddLine(r.X + d, r.Y, r.Right - d, r.Y);
            p.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
            p.AddLine(r.Right, r.Y + d, r.Right, r.Bottom - d);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
            p.AddLine(r.Right - d, r.Bottom, r.X + d, r.Bottom);
            p.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
            p.CloseFigure();
            return p;
        }

        private static PointF[] Poly(RectangleF r, float[] fx)
        {
            var pts = new PointF[fx.Length / 2];
            for (int i = 0; i < pts.Length; i++)
                pts[i] = new PointF(r.X + fx[i * 2] * r.Width - 0.5f, r.Y + fx[i * 2 + 1] * r.Height - 0.5f);
            return pts;
        }

        private static void DrawEdgeRing(Graphics g, RectangleF outer, float s, int shape, Color c, float w)
        {
            using (GraphicsPath inner = ShapePath(Inset(outer, w), s - 2f * w, shape))
            using (GraphicsPath outerPath = ShapePath(outer, s, shape))
            using (var ring = new GraphicsPath(FillMode.Alternate))
            using (var br = new SolidBrush(c))
            {
                ring.AddPath(outerPath, false);
                ring.AddPath(inner, false);
                g.FillPath(br, ring);
            }
        }

        private static void Halo(Graphics g, GraphicsPath path, Color c, float s, float strength)
        {
            float[] gw = { s * 0.28f, s * 0.17f, s * 0.09f };
            int[] ga = { 22, 40, 66 };
            for (int i = 0; i < 3; i++)
                using (var pen = new Pen(Color.FromArgb((int)(ga[i] * strength), c), gw[i]))
                    g.DrawPath(pen, path);
        }

        /// <summary>新拟态凹凸边：内缩 0.8 画暗线、内缩 2.2 画亮线。</summary>
        private static void NeuEdge(Graphics g, RectangleF r, float s, int shape)
        {
            using (GraphicsPath p1 = ShapePath(Inset(r, 0.8f), Math.Max(1f, s - 1.6f), shape))
            using (GraphicsPath p2 = ShapePath(Inset(r, 2.2f), Math.Max(1f, s - 4.4f), shape))
            {
                using (var dp = new Pen(Color.FromArgb(46, 0, 0, 0), 1.4f)) g.DrawPath(dp, p1);
                using (var lp = new Pen(Color.FromArgb(180, 255, 255, 255), 1.4f)) g.DrawPath(lp, p2);
            }
        }

        private static void Fill(Graphics g, GraphicsPath p, Color c)
        {
            using (var b = new SolidBrush(c)) g.FillPath(b, p);
        }

        private static Color Mix(Color a, Color b, float t) => HPushButtonScheme.Mix(a, b, t);
        private static Color Light(Color c, float t) => Mix(c, Color.White, t);
        private static Color Dark(Color c, float t) => HPushButtonScheme.Shade(c, t);
        #endregion

        #region 独立符号
        private static void DrawGlyph(Graphics g, Rectangle r, int glyph, Color c)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height, cx = x + w / 2f;
            using (var br = new SolidBrush(c))
            {
                switch (glyph)
                {
                    case GlStar:
                        var pts = new PointF[10];
                        for (int i = 0; i < 10; i++)
                        {
                            double a = -Math.PI / 2 + i * Math.PI / 5;
                            float rr = i % 2 == 0 ? w * 0.5f : w * 0.22f;
                            pts[i] = new PointF(cx + (float)Math.Cos(a) * rr, y + h / 2f + (float)Math.Sin(a) * rr);
                        }
                        g.FillPolygon(br, pts);
                        break;
                    case GlHeart:
                        using (var p = new GraphicsPath())
                        {
                            var tip = new PointF(cx, y + h * 0.94f);
                            p.AddBezier(tip,
                                new PointF(x + w * 0.02f, y + h * 0.55f),
                                new PointF(x + w * 0.04f, y + h * 0.02f),
                                new PointF(cx, y + h * 0.3f));
                            p.AddBezier(new PointF(cx, y + h * 0.3f),
                                new PointF(x + w * 0.96f, y + h * 0.02f),
                                new PointF(x + w * 0.98f, y + h * 0.55f),
                                tip);
                            p.CloseFigure();
                            g.FillPath(br, p);
                        }
                        break;
                    case GlBolt:
                        g.FillPolygon(br, new[]
                        {
                            new PointF(x + w * 0.56f, y + h * 0.04f),
                            new PointF(x + w * 0.22f, y + h * 0.54f),
                            new PointF(x + w * 0.45f, y + h * 0.54f),
                            new PointF(x + w * 0.36f, y + h * 0.96f),
                            new PointF(x + w * 0.82f, y + h * 0.40f),
                            new PointF(x + w * 0.56f, y + h * 0.40f)
                        });
                        break;
                    case GlFlag:
                        g.FillRectangle(br, x + w * 0.2f, y + h * 0.06f, w * 0.1f, h * 0.88f);
                        g.FillPolygon(br, new[]
                        {
                            new PointF(x + w * 0.3f, y + h * 0.1f),
                            new PointF(x + w * 0.82f, y + h * 0.1f),
                            new PointF(x + w * 0.66f, y + h * 0.3f),
                            new PointF(x + w * 0.82f, y + h * 0.5f),
                            new PointF(x + w * 0.3f, y + h * 0.5f)
                        });
                        break;
                    case GlBookmark:
                        g.FillPolygon(br, new[]
                        {
                            new PointF(x + w * 0.22f, y + h * 0.04f),
                            new PointF(x + w * 0.78f, y + h * 0.04f),
                            new PointF(x + w * 0.78f, y + h * 0.96f),
                            new PointF(cx, y + h * 0.72f),
                            new PointF(x + w * 0.22f, y + h * 0.96f)
                        });
                        break;
                    case GlShield:
                        g.FillPolygon(br, new[]
                        {
                            new PointF(x + w * 0.1f, y + h * 0.18f),
                            new PointF(x + w * 0.9f, y + h * 0.18f),
                            new PointF(x + w * 0.9f, y + h * 0.52f),
                            new PointF(cx, y + h * 0.94f),
                            new PointF(x + w * 0.1f, y + h * 0.52f)
                        });
                        using (var sp = new Pen(Color.White, Math.Max(1.4f, w * 0.11f))
                        { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                            g.DrawLines(sp, new[]
                            {
                                new PointF(x + w * 0.32f, y + h * 0.5f),
                                new PointF(x + w * 0.45f, y + h * 0.63f),
                                new PointF(x + w * 0.7f, y + h * 0.36f)
                            });
                        break;
                    case GlLock:
                        g.FillRectangle(br, x + w * 0.22f, y + h * 0.46f, w * 0.56f, h * 0.42f);
                        using (var pen = new Pen(c, Math.Max(1.5f, w * 0.11f)))
                            g.DrawArc(pen, x + w * 0.3f, y + h * 0.12f, w * 0.4f, h * 0.46f, 180f, 180f);
                        g.FillEllipse(br, cx - w * 0.06f, y + h * 0.6f, w * 0.12f, h * 0.12f);
                        break;
                    case GlEye:
                        using (var p = new GraphicsPath())
                        {
                            p.AddBezier(new PointF(x + w * 0.04f, y + h * 0.5f),
                                new PointF(x + w * 0.24f, y + h * 0.16f),
                                new PointF(x + w * 0.76f, y + h * 0.16f),
                                new PointF(x + w * 0.96f, y + h * 0.5f));
                            p.AddBezier(new PointF(x + w * 0.96f, y + h * 0.5f),
                                new PointF(x + w * 0.76f, y + h * 0.84f),
                                new PointF(x + w * 0.24f, y + h * 0.84f),
                                new PointF(x + w * 0.04f, y + h * 0.5f));
                            p.CloseFigure();
                            using (var pen = new Pen(c, Math.Max(1.4f, w * 0.1f))) g.DrawPath(pen, p);
                        }
                        g.FillEllipse(br, cx - w * 0.13f, y + h * 0.37f, w * 0.26f, h * 0.26f);
                        break;
                    case GlSun:
                        g.FillEllipse(br, x + w * 0.34f, y + h * 0.34f, w * 0.32f, h * 0.32f);
                        using (var pen = new Pen(c, Math.Max(1.4f, w * 0.09f))
                        { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            for (int i = 0; i < 8; i++)
                            {
                                double a = i * Math.PI / 4;
                                float c1 = (float)Math.Cos(a), s1 = (float)Math.Sin(a);
                                g.DrawLine(pen,
                                    cx + c1 * w * 0.42f, y + h / 2f + s1 * h * 0.42f,
                                    cx + c1 * w * 0.5f, y + h / 2f + s1 * h * 0.5f);
                            }
                        break;
                    case GlMoon:
                        using (var p = new GraphicsPath(FillMode.Alternate))
                        {
                            p.AddEllipse(x + w * 0.12f, y + h * 0.06f, w * 0.8f, h * 0.88f);
                            p.AddEllipse(x + w * 0.34f, y + h * 0.0f, w * 0.72f, h * 0.88f);
                            g.FillPath(br, p);
                        }
                        break;
                    case GlCloud:
                        // Winding：重叠圆与底托按并集实心填充，Alternate 会在重叠处留洞
                        using (var p = new GraphicsPath(FillMode.Winding))
                        {
                            p.AddEllipse(x + w * 0.08f, y + h * 0.42f, w * 0.42f, h * 0.42f);
                            p.AddEllipse(x + w * 0.28f, y + h * 0.18f, w * 0.48f, h * 0.48f);
                            p.AddEllipse(x + w * 0.54f, y + h * 0.4f, w * 0.38f, h * 0.38f);
                            p.AddRectangle(new RectangleF(x + w * 0.14f, y + h * 0.54f, w * 0.7f, h * 0.26f));
                            g.FillPath(br, p);
                        }
                        break;
                    case GlNote:
                        g.FillEllipse(br, x + w * 0.16f, y + h * 0.6f, w * 0.38f, h * 0.3f);
                        g.FillRectangle(br, x + w * 0.5f, y + h * 0.08f, w * 0.1f, h * 0.6f);
                        g.FillPolygon(br, new[]
                        {
                            new PointF(x + w * 0.6f, y + h * 0.08f),
                            new PointF(x + w * 0.86f, y + h * 0.17f),
                            new PointF(x + w * 0.86f, y + h * 0.33f),
                            new PointF(x + w * 0.6f, y + h * 0.25f)
                        });
                        break;
                }
            }
        }
        #endregion
    }
}
