using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 系列内置矢量图标：全部用 GraphicsPath 按给定矩形等比绘制，
    /// 描边图标使用圆角笔帽，星形/播放为填充路径。
    /// </summary>
    public enum HFluentGlyph
    {
        /// <summary>无图标。</summary>
        None,
        /// <summary>对勾。</summary>
        Check,
        /// <summary>关闭。</summary>
        Close,
        /// <summary>信息。</summary>
        Info,
        /// <summary>疑问。</summary>
        Question,
        /// <summary>五角星。</summary>
        Star,
        /// <summary>警告。</summary>
        Warning,
        /// <summary>锁。</summary>
        Lock,
        /// <summary>纸飞机发送。</summary>
        Send,
        /// <summary>搜索放大镜。</summary>
        Search,
        /// <summary>地球网络。</summary>
        Globe,
        /// <summary>下载。</summary>
        Download,
        /// <summary>点赞。</summary>
        ThumbUp,
        /// <summary>下箭头。</summary>
        ChevronDown,
        /// <summary>上箭头。</summary>
        ChevronUp,
        /// <summary>右箭头。</summary>
        ChevronRight,
        /// <summary>加号。</summary>
        Plus,
        /// <summary>减号。</summary>
        Minus,
        /// <summary>播放三角。</summary>
        Play,
        /// <summary>全屏四角。</summary>
        FullScreen
    }

    /// <summary>矢量图标绘制器。</summary>
    public static class HFluentGlyphDraw
    {
        /// <summary>在 box 区域内绘制图标；filled 仅对星形等填充图标生效。</summary>
        public static void Draw(Graphics g, HFluentGlyph kind, RectangleF box, Color color, float penWidth, bool filled)
        {
            if (kind == HFluentGlyph.None) return;
            var inset = penWidth / 2f + 0.5f;
            box.Inflate(-inset, -inset);
            using (var pen = new Pen(color, penWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (kind)
                {
                    case HFluentGlyph.Check:
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.Left + box.Width * 0.08f, box.Top + box.Height * 0.55f),
                            new PointF(box.Left + box.Width * 0.40f, box.Bottom - box.Height * 0.15f),
                            new PointF(box.Right - box.Width * 0.06f, box.Top + box.Height * 0.18f)
                        });
                        break;
                    case HFluentGlyph.Close:
                        g.DrawLine(pen, box.Left + box.Width * 0.22f, box.Top + box.Height * 0.22f,
                            box.Right - box.Width * 0.22f, box.Bottom - box.Height * 0.22f);
                        g.DrawLine(pen, box.Right - box.Width * 0.22f, box.Top + box.Height * 0.22f,
                            box.Left + box.Width * 0.22f, box.Bottom - box.Height * 0.22f);
                        break;
                    case HFluentGlyph.Plus:
                        g.DrawLine(pen, box.Left + box.Width * 0.5f, box.Top + box.Height * 0.2f,
                            box.Left + box.Width * 0.5f, box.Bottom - box.Height * 0.2f);
                        g.DrawLine(pen, box.Left + box.Width * 0.2f, box.Top + box.Height * 0.5f,
                            box.Right - box.Width * 0.2f, box.Top + box.Height * 0.5f);
                        break;
                    case HFluentGlyph.Minus:
                        g.DrawLine(pen, box.Left + box.Width * 0.2f, box.Top + box.Height * 0.5f,
                            box.Right - box.Width * 0.2f, box.Top + box.Height * 0.5f);
                        break;
                    case HFluentGlyph.ChevronDown:
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.Left + box.Width * 0.2f, box.Top + box.Height * 0.32f),
                            new PointF(box.Left + box.Width * 0.5f, box.Bottom - box.Height * 0.28f),
                            new PointF(box.Right - box.Width * 0.2f, box.Top + box.Height * 0.32f)
                        });
                        break;
                    case HFluentGlyph.ChevronUp:
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.Left + box.Width * 0.2f, box.Bottom - box.Height * 0.32f),
                            new PointF(box.Left + box.Width * 0.5f, box.Top + box.Height * 0.28f),
                            new PointF(box.Right - box.Width * 0.2f, box.Bottom - box.Height * 0.32f)
                        });
                        break;
                    case HFluentGlyph.ChevronRight:
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.Left + box.Width * 0.32f, box.Top + box.Height * 0.2f),
                            new PointF(box.Right - box.Width * 0.28f, box.Top + box.Height * 0.5f),
                            new PointF(box.Left + box.Width * 0.32f, box.Bottom - box.Height * 0.2f)
                        });
                        break;
                    case HFluentGlyph.Play:
                        using (var play = PlayPath(box)) g.FillPath(brush, play);
                        break;
                    case HFluentGlyph.Star:
                        using (var star = StarPath(box))
                        {
                            if (filled) g.FillPath(brush, star);
                            else g.DrawPath(pen, star);
                        }
                        break;
                    case HFluentGlyph.Info:
                        // 纯 i 字形（圆点 + 竖条），外圆盘由 HIconCircle/卡片背景提供
                        float dotS = penWidth * 2.6f;
                        g.FillEllipse(brush, box.Left + box.Width * 0.5f - dotS / 2f, box.Top + box.Height * 0.2f,
                            dotS, dotS);
                        g.DrawLine(pen, box.Left + box.Width * 0.5f, box.Top + box.Height * 0.44f,
                            box.Left + box.Width * 0.5f, box.Bottom - box.Height * 0.2f);
                        break;
                    case HFluentGlyph.Question:
                        g.DrawEllipse(pen, box.Left + box.Width * 0.16f, box.Top + box.Height * 0.06f,
                            box.Width * 0.68f, box.Height * 0.68f);
                        using (var arc = new GraphicsPath())
                        {
                            arc.AddArc(box.Left + box.Width * 0.3f, box.Top + box.Height * 0.18f,
                                box.Width * 0.4f, box.Height * 0.4f, 200f, 140f);
                            arc.AddLine(box.Left + box.Width * 0.5f, box.Top + box.Height * 0.56f,
                                box.Left + box.Width * 0.5f, box.Bottom - box.Height * 0.28f);
                            g.DrawPath(pen, arc);
                        }
                        g.FillEllipse(brush, box.Left + box.Width * 0.5f - penWidth, box.Bottom - box.Height * 0.2f,
                            penWidth * 2f, penWidth * 2f);
                        break;
                    case HFluentGlyph.Warning:
                        using (var tri = new GraphicsPath())
                        {
                            float cx0 = box.Left + box.Width / 2f;
                            tri.AddPolygon(new[]
                            {
                                new PointF(cx0, box.Top + box.Height * 0.08f),
                                new PointF(box.Right - box.Width * 0.06f, box.Bottom - box.Height * 0.14f),
                                new PointF(box.Left + box.Width * 0.06f, box.Bottom - box.Height * 0.14f)
                            });
                            if (filled) g.FillPath(brush, tri);
                            else g.DrawPath(pen, tri);
                            g.DrawLine(pen, cx0, box.Top + box.Height * 0.38f, cx0, box.Bottom - box.Height * 0.38f);
                            g.FillEllipse(brush, cx0 - penWidth, box.Bottom - box.Height * 0.24f,
                                penWidth * 2f, penWidth * 2f);
                        }
                        break;
                    case HFluentGlyph.Lock:
                        g.DrawArc(pen, box.Left + box.Width * 0.28f, box.Top + box.Height * 0.05f,
                            box.Width * 0.44f, box.Height * 0.45f, 180f, 180f);
                        g.DrawRectangle(pen, box.Left + box.Width * 0.18f, box.Top + box.Height * 0.42f,
                            box.Width * 0.64f, box.Height * 0.5f);
                        g.FillEllipse(brush, box.Left + box.Width * 0.5f - penWidth, box.Top + box.Height * 0.62f,
                            penWidth * 2f, penWidth * 2f);
                        break;
                    case HFluentGlyph.Send:
                        using (var send = new GraphicsPath())
                        {
                            send.AddPolygon(new[]
                            {
                                new PointF(box.Left + box.Width * 0.08f, box.Top + box.Height * 0.5f),
                                new PointF(box.Right - box.Width * 0.1f, box.Top + box.Height * 0.12f),
                                new PointF(box.Right - box.Width * 0.28f, box.Bottom - box.Height * 0.18f)
                            });
                            send.CloseAllFigures();
                            send.AddLine(box.Right - box.Width * 0.28f, box.Bottom - box.Height * 0.18f,
                                box.Right - box.Width * 0.1f, box.Bottom - box.Height * 0.5f);
                            g.DrawPath(pen, send);
                        }
                        break;
                    case HFluentGlyph.Search:
                        float r0 = box.Width * 0.32f;
                        g.DrawEllipse(pen, box.Left + box.Width * 0.1f, box.Top + box.Height * 0.1f, r0 * 2f, r0 * 2f);
                        g.DrawLine(pen, box.Left + box.Width * 0.56f, box.Top + box.Height * 0.56f,
                            box.Right - box.Width * 0.12f, box.Bottom - box.Height * 0.12f);
                        break;
                    case HFluentGlyph.Globe:
                        g.DrawEllipse(pen, box.Left + box.Width * 0.1f, box.Top + box.Height * 0.1f,
                            box.Width * 0.8f, box.Height * 0.8f);
                        g.DrawEllipse(pen, box.Left + box.Width * 0.34f, box.Top + box.Height * 0.12f,
                            box.Width * 0.32f, box.Height * 0.76f);
                        g.DrawLine(pen, box.Left + box.Width * 0.12f, box.Top + box.Height * 0.5f,
                            box.Right - box.Width * 0.12f, box.Top + box.Height * 0.5f);
                        break;
                    case HFluentGlyph.Download:
                        g.DrawLine(pen, box.Left + box.Width * 0.5f, box.Top + box.Height * 0.12f,
                            box.Left + box.Width * 0.5f, box.Bottom - box.Height * 0.42f);
                        g.DrawLines(pen, new[]
                        {
                            new PointF(box.Left + box.Width * 0.28f, box.Top + box.Height * 0.6f),
                            new PointF(box.Left + box.Width * 0.5f, box.Bottom - box.Height * 0.16f),
                            new PointF(box.Right - box.Width * 0.28f, box.Top + box.Height * 0.6f)
                        });
                        g.DrawLine(pen, box.Left + box.Width * 0.2f, box.Bottom - box.Height * 0.12f,
                            box.Right - box.Width * 0.2f, box.Bottom - box.Height * 0.12f);
                        break;
                    case HFluentGlyph.ThumbUp:
                        using (var thumb = new GraphicsPath())
                        {
                            thumb.AddLines(new[]
                            {
                                new PointF(box.Left + box.Width * 0.42f, box.Bottom - box.Height * 0.14f),
                                new PointF(box.Left + box.Width * 0.42f, box.Top + box.Height * 0.42f),
                                new PointF(box.Left + box.Width * 0.58f, box.Top + box.Height * 0.16f),
                                new PointF(box.Left + box.Width * 0.68f, box.Top + box.Height * 0.16f),
                                new PointF(box.Right - box.Width * 0.2f, box.Top + box.Height * 0.4f),
                                new PointF(box.Right - box.Width * 0.1f, box.Bottom - box.Height * 0.56f),
                                new PointF(box.Left + box.Width * 0.42f, box.Bottom - box.Height * 0.14f)
                            });
                            thumb.CloseAllFigures();
                            thumb.AddRectangle(new RectangleF(
                                box.Left + box.Width * 0.14f, box.Top + box.Height * 0.42f,
                                box.Width * 0.28f, box.Height * 0.44f));
                            g.DrawPath(pen, thumb);
                        }
                        break;
                    case HFluentGlyph.FullScreen:
                        float a0 = box.Width * 0.22f;
                        g.DrawLine(pen, box.Left + box.Width * 0.14f, box.Top + box.Height * 0.14f + a0,
                            box.Left + box.Width * 0.14f, box.Top + box.Height * 0.14f);
                        g.DrawLine(pen, box.Left + box.Width * 0.14f, box.Top + box.Height * 0.14f,
                            box.Left + box.Width * 0.14f + a0, box.Top + box.Height * 0.14f);
                        g.DrawLine(pen, box.Right - box.Width * 0.14f - a0, box.Top + box.Height * 0.14f,
                            box.Right - box.Width * 0.14f, box.Top + box.Height * 0.14f);
                        g.DrawLine(pen, box.Right - box.Width * 0.14f, box.Top + box.Height * 0.14f,
                            box.Right - box.Width * 0.14f, box.Top + box.Height * 0.14f + a0);
                        g.DrawLine(pen, box.Left + box.Width * 0.14f, box.Bottom - box.Height * 0.14f - a0,
                            box.Left + box.Width * 0.14f, box.Bottom - box.Height * 0.14f);
                        g.DrawLine(pen, box.Left + box.Width * 0.14f, box.Bottom - box.Height * 0.14f,
                            box.Left + box.Width * 0.14f + a0, box.Bottom - box.Height * 0.14f);
                        g.DrawLine(pen, box.Right - box.Width * 0.14f - a0, box.Bottom - box.Height * 0.14f,
                            box.Right - box.Width * 0.14f, box.Bottom - box.Height * 0.14f);
                        g.DrawLine(pen, box.Right - box.Width * 0.14f, box.Bottom - box.Height * 0.14f - a0,
                            box.Right - box.Width * 0.14f, box.Bottom - box.Height * 0.14f);
                        break;
                }
            }
        }

        private static GraphicsPath StarPath(RectangleF b)
        {
            var p = new GraphicsPath();
            float cx = b.Left + b.Width / 2f, cy = b.Top + b.Height / 2f;
            float outer = b.Width * 0.42f, inner = outer * 0.45f;
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double ang = -Math.PI / 2 + i * Math.PI / 5;
                float rr = (i % 2 == 0) ? outer : inner;
                pts[i] = new PointF(cx + (float)(Math.Cos(ang) * rr), cy + (float)(Math.Sin(ang) * rr));
            }
            p.AddPolygon(pts);
            return p;
        }

        private static GraphicsPath PlayPath(RectangleF b)
        {
            var p = new GraphicsPath();
            p.AddPolygon(new[]
            {
                new PointF(b.Left + b.Width * 0.25f, b.Top + b.Height * 0.15f),
                new PointF(b.Right - b.Width * 0.2f, b.Top + b.Height * 0.5f),
                new PointF(b.Left + b.Width * 0.25f, b.Bottom - b.Height * 0.15f)
            });
            return p;
        }
    }
}
