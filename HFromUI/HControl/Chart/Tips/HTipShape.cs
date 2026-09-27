using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Chart.Tips
{
    /// <summary>
    /// 气泡外形绘制引擎：测量正文→外框尺寸、计算正文区/文字原点、按外形描述画填充/边框/尾巴。
    /// 标准形（圆角/直角/胶囊/椭圆）+ 32 种特殊外形（云/爆炸/星牌/横幅/卷轴/游戏牌/墨迹/双气泡等）。
    /// 所有几何均为纯函数式 GDI+ 路径，无随机数（重绘不抖动）。
    /// </summary>
    internal static class HTipShape
    {
        /// <summary>测量：正文尺寸（cw 最长行宽、ch 行高总和）→ 气泡外框总尺寸。</summary>
        public static SizeF Measure(HTSkinDef d, float cw, float ch, bool hasArt)
        {
            GetPads(d, cw, ch, out float padL, out float padR, out float padT, out float padB,
                    out float exL, out float exR, out float exT, out float exB);
            if (hasArt) padR += 15f; // 右上角饰留位
            float bodyW = cw + padL + padR;
            float bodyH = ch + padT + padB;
            // 标准形尾巴占位（特殊形已在各自附加边距里处理）
            if (d.Core != HTCore.Custom)
            {
                if (d.Tail == HTTail.Think) exB = 13f;
                else if (d.Tail == HTTail.BL || d.Tail == HTTail.BC || d.Tail == HTTail.BR) exB = HTipSkins.TailH;
                else if (d.Tail == HTTail.TL || d.Tail == HTTail.TC || d.Tail == HTTail.TR) exT = HTipSkins.TailH;
            }
            return new SizeF(bodyW + exL + exR, bodyH + exT + exB);
        }

        /// <summary>正文区矩形（外框去掉尾巴占位与特殊装饰边距）。</summary>
        public static RectangleF BodyRect(HTSkinDef d, RectangleF box, HTTail tail)
        {
            if (d.Core == HTCore.Custom) return CustomBody(d.Special, box);
            float y = box.Y, h = box.Height;
            if (tail == HTTail.Think || tail == HTTail.BL || tail == HTTail.BC || tail == HTTail.BR) h -= HTipSkins.TailH + (tail == HTTail.Think ? 3f : 0f);
            else if (tail == HTTail.TL || tail == HTTail.TC || tail == HTTail.TR) { y += HTipSkins.TailH; h -= HTipSkins.TailH; }
            return new RectangleF(box.X, y, box.Width, h);
        }

        /// <summary>文字原点（首行左上角，TextRenderer 用）。</summary>
        public static PointF TextOrigin(HTSkinDef d, float cw, float ch, RectangleF body, bool hasArt)
        {
            GetPads(d, cw, ch, out float padL, out _, out float padT, out _, out _, out _, out _, out _);
            return new PointF(body.X + padL, body.Y + padT);
        }

        /// <summary>绘制气泡外形（不含文字）：box 为外框总矩形，tail 为经落点翻转后的实际朝向。</summary>
        public static void Paint(Graphics g, HTSkinDef d, RectangleF box, HTTail tail, HTipColors pc)
        {
            if (d.Core == HTCore.Custom) { PaintCustom(g, d.Special, box, pc); return; }
            var body = BodyRect(d, box, tail);

            // 1) 尾巴实底（先画，正文形填充随后盖住接缝）
            if (tail != HTTail.None && tail != HTTail.Think) FillTail(g, body, box, tail, pc.Bg);
            // 2) 正文形填充
            using (var path = BodyPath(d, body))
            using (var br = new SolidBrush(pc.Bg))
                g.FillPath(br, path);
            // 3) 尾巴两条外边（不描接缝，避免气泡内部横穿一条线）
            if (tail != HTTail.None && tail != HTTail.Think)
                using (var p = new Pen(pc.Border, d.BorderW)) DrawTailEdges(g, p, body, box, tail);
            // 4) 正文形描边
            using (var path = BodyPath(d, body))
            using (var p = new Pen(pc.Border, d.BorderW))
                g.DrawPath(p, path);
            // 5) 双线内框
            if (d.DoubleLine)
                using (var path = BodyPath(d, InflateBody(body, 3f)))
                using (var p = new Pen(Color.FromArgb(110, pc.Border.R, pc.Border.G, pc.Border.B), 1f))
                    g.DrawPath(p, path);
            // 6) 思考点链
            if (tail == HTTail.Think) DrawThinkDots(g, body, box, pc, false);
        }

        // ============ 标准形几何 ============

        /// <summary>取各外形的内边距（pad*=正文到正文形边）与附加边距（ex*=正文形到外框，含尾巴/卷杆等装饰）。</summary>
        private static void GetPads(HTSkinDef d, float cw, float ch,
            out float padL, out float padR, out float padT, out float padB,
            out float exL, out float exR, out float exT, out float exB)
        {
            padL = padR = 6f; padT = padB = 5f;
            exL = exR = exT = exB = 0f;
            if (d.Core == HTCore.Custom) { CustomPads(d.Special, cw, ch, ref padL, ref padR, ref padT, ref padB, ref exL, ref exR, ref exT, ref exB); return; }
            if (d.Core == HTCore.Capsule)
            {
                // 胶囊两端为半圆：正文需落在中段直筒内（bodyH 约 ch+10）
                float r = 8f + padT;
                padL = padR = r;
            }
            else if (d.Core == HTCore.Oval)
            {
                padL = padR = 12f; padT = padB = 8f;
            }
            if (d.DoubleLine) { padL += 2f; padR += 2f; padT += 2f; padB += 2f; }
        }

        /// <summary>标准正文形路径（圆角/直角/胶囊/椭圆）。</summary>
        private static GraphicsPath BodyPath(HTSkinDef d, RectangleF body)
        {
            var path = new GraphicsPath();
            float r = d.Radius;
            if (d.Core == HTCore.Rect) path.AddRectangle(body);
            else if (d.Core == HTCore.Oval) path.AddEllipse(body);
            else if (d.Core == HTCore.Capsule) r = body.Height / 2f;
            if (d.Core == HTCore.Rounded || d.Core == HTCore.Capsule)
            {
                r = Math.Min(r, body.Height / 2f);
                AddRoundRect(path, body.X, body.Y, body.Width, body.Height, r);
            }
            return path;
        }

        /// <summary>圆角矩形路径（与 HChart.RoundRectPath 同构）。</summary>
        internal static GraphicsPath RoundRect(float x, float y, float w, float h, float r)
        {
            var path = new GraphicsPath();
            AddRoundRect(path, x, y, w, h, r);
            return path;
        }

        /// <summary>向路径追加圆角矩形。</summary>
        private static void AddRoundRect(GraphicsPath path, float x, float y, float w, float h, float r)
        {
            float d = r * 2f;
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(x + w - d, y, d, d, 270, 90);
            path.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            path.AddArc(x, y + h - d, d, d, 90, 90);
            path.CloseFigure();
        }

        /// <summary>矩形四边内缩。</summary>
        private static RectangleF InflateBody(RectangleF body, float m)
            => new RectangleF(body.X + m, body.Y + m, body.Width - m * 2f, body.Height - m * 2f);

        /// <summary>尾巴三角形实底。</summary>
        private static void FillTail(Graphics g, RectangleF b, RectangleF box, HTTail tail, Color bg)
        {
            using (var br = new SolidBrush(bg))
                g.FillPolygon(br, TailPoints(b, box, tail));
        }

        /// <summary>只描尾巴两条外边（尖→两腰点）。</summary>
        private static void DrawTailEdges(Graphics g, Pen p, RectangleF b, RectangleF box, HTTail tail)
        {
            var pts = TailPoints(b, box, tail);
            g.DrawLine(p, pts[0], pts[1]);
            g.DrawLine(p, pts[0], pts[2]);
        }

        /// <summary>尾巴三点：[0]=尖，[1]/[2]=腰部两接点。</summary>
        private static PointF[] TailPoints(RectangleF b, RectangleF box, HTTail tail)
        {
            float w18 = b.Width * 0.16f, w28 = b.Width * 0.30f, w50 = b.Width * 0.50f;
            switch (tail)
            {
                case HTTail.BL:
                    return new[] { new PointF(b.X + w18 - 4f, box.Bottom - 1f), new PointF(b.X + w18 - 1f, b.Bottom), new PointF(b.X + w28 + 5f, b.Bottom) };
                case HTTail.BC:
                    return new[] { new PointF(b.X + w50, box.Bottom - 1f), new PointF(b.X + w50 - 9f, b.Bottom), new PointF(b.X + w50 + 9f, b.Bottom) };
                case HTTail.BR:
                    return new[] { new PointF(b.Right - w18 + 4f, box.Bottom - 1f), new PointF(b.Right - w28 - 5f, b.Bottom), new PointF(b.Right - w18 + 1f, b.Bottom) };
                case HTTail.TL:
                    return new[] { new PointF(b.X + w18 - 4f, box.Y + 1f), new PointF(b.X + w28 + 5f, b.Top), new PointF(b.X + w18 - 1f, b.Top) };
                case HTTail.TC:
                    return new[] { new PointF(b.X + w50, box.Y + 1f), new PointF(b.X + w50 + 9f, b.Top), new PointF(b.X + w50 - 9f, b.Top) };
                case HTTail.TR:
                    return new[] { new PointF(b.Right - w18 + 4f, box.Y + 1f), new PointF(b.Right - w18 + 1f, b.Top), new PointF(b.Right - w28 - 5f, b.Top) };
            }
            return new[] { PointF.Empty, PointF.Empty, PointF.Empty };
        }

        /// <summary>思考点链：三颗递小圆点（边圈+实底），向左下收向锚点。</summary>
        private static void DrawThinkDots(Graphics g, RectangleF b, RectangleF box, HTipColors pc, bool solid)
        {
            var dots = new[]
            {
                new { X = b.X + 10f, Y = b.Bottom + 4.5f, R = 2.8f },
                new { X = b.X + 4f,  Y = b.Bottom + 9f,   R = 2f },
                new { X = b.X + 0.5f, Y = b.Bottom + 12f, R = 1.2f },
            };
            using (var br = new SolidBrush(pc.Bg))
            using (var pen = new Pen(pc.Border, 1f))
                foreach (var d in dots)
                {
                    if (solid) { using (var b2 = new SolidBrush(pc.Border)) g.FillEllipse(b2, d.X - d.R, d.Y - d.R, d.R * 2f, d.R * 2f); }
                    else
                    {
                        g.FillEllipse(br, d.X - d.R, d.Y - d.R, d.R * 2f, d.R * 2f);
                        g.DrawEllipse(pen, d.X - d.R, d.Y - d.R, d.R * 2f, d.R * 2f);
                    }
                }
        }

        // ============ 特殊外形（32 种，PaintCustom 分段实现于文件后部） ============

        /// <summary>特殊形内边距（pad*=正文到正文形边）/附加边距（ex*=正文形到外框），与 CustomBody 严格对齐。</summary>
        private static void CustomPads(int sp, float cw, float ch,
            ref float padL, ref float padR, ref float padT, ref float padB,
            ref float exL, ref float exR, ref float exT, ref float exB)
        {
            // 默认值即经典内边距；下面逐形改写
            switch (sp)
            {
                case 1: (padL, padR, padT, padB, exL, exR, exT, exB) = (2, 2, 1, 1, 9, 9, 11, 7); break;                    // 云朵
                case 2: (padL, padR, padT, padB, exL, exR, exT, exB) = (2, 2, 1, 1, 9, 9, 11, 17); break;                   // 云朵·下尾
                case 3: case 5: (padL, padR, padT, padB, exL, exR, exT, exB) = (8, 8, 6, 6, 13, 13, 13, 11); break;         // 爆炸芒/柔爆
                case 4: (padL, padR, padT, padB, exL, exR, exT, exB) = (8, 8, 6, 6, 13, 13, 13, 21); break;                 // 爆炸芒·下尾
                case 6: (padL, padR, padT, padB, exL, exR, exT, exB) = (8, 8, 6, 6, 15, 15, 15, 13); break;                 // 尖锐爆炸
                case 7: // 五角星牌：外扩按正文宽高比例（与 CustomBody 百分比同源）
                    (padL, padR, padT, padB, exL, exR, exT, exB) = (0, 0, 0, 0, cw * 0.45f, cw * 0.45f, ch * 0.75f, ch * 0.85f); break;
                case 8: // 六角星牌
                    (padL, padR, padT, padB, exL, exR, exT, exB) = (0, 0, 0, 0, cw * 0.45f, cw * 0.45f, ch * 0.58f, ch * 0.62f); break;
                case 9: (padL, padR, padT, padB, exL, exR, exT, exB) = (3, 3, 0, 0, 4, 4, 3, 3); break;                     // 燕尾横幅
                case 10: (padL, padR, padT, padB, exL, exR, exT, exB) = (3, 3, 0, 0, 4, 4, 3, 13); break;                   // 横幅·下尾
                case 11: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 5, 5, 11, 11, 0, 0); break;                  // 卷轴卷杆
                case 12: (padL, padR, padT, padB, exL, exR, exT, exB) = (12, 12, 7, 7, 16, 16, 7, 7); break;                  // 游戏标题牌
                case 13: (padL, padR, padT, padB, exL, exR, exT, exB) = (12, 12, 7, 7, 16, 16, 7, 17); break;                 // 标题牌·下尾
                case 14: (padL, padR, padT, padB, exL, exR, exT, exB) = (2, 2, 1, 1, 4, 4, 4, 4); break;                    // 墨迹方框
                case 15: (padL, padR, padT, padB, exL, exR, exT, exB) = (2, 2, 1, 1, 4, 4, 4, 14); break;                   // 墨迹·下尾
                case 16: (padL, padR, padT, padB, exL, exR, exT, exB) = (2, 2, 1, 1, 9, 9, 11, 7); break;                   // 墨迹云
                case 17: (padL, padR, padT, padB, exL, exR, exT, exB) = (3, 3, 2, 2, 11, 11, 9, 9); break;                  // 墨迹椭圆
                case 18: case 19: (padL, padR, padT, padB, exL, exR, exT, exB) = (8, 8, 7, 7, 0, 16, 0, 14); break;         // 双气泡鬼影占位
                case 20: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 7, 5, 5, 1, 1, 1, 13); break;                   // 圆点拖尾
                case 21: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 5, 5, 12, 0, 0, 0); break;                   // 左箭头牌
                case 22: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 5, 5, 0, 12, 0, 0); break;                  // 右箭头牌
                case 23: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 4, 4, 13, 6, 2, 2); break;                  // 标签牌
                case 24: (padL, padR, padT, padB, exL, exR, exT, exB) = (3, 3, 1, 1, 7, 7, 5, 5); break;                   // 票券
                case 25: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 2, 2, 8, 8, 5, 21); break;                  // 盾牌
                case 26: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 4, 4, 7, 7, 9, 5); break;                   // 文件夹标签
                case 27: (padL, padR, padT, padB, exL, exR, exT, exB) = (3, 3, 2, 2, 5, 5, 5, 5); break;                   // 折角便签
                case 28: (padL, padR, padT, padB, exL, exR, exT, exB) = (3, 3, 2, 2, 5, 5, 5, 15); break;                  // 便签·下尾
                case 29: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 5, 5, 0, 5, 0, 5); break;                   // 投影卡片（前层右下偏移 5px）
                case 30: (padL, padR, padT, padB, exL, exR, exT, exB) = (6, 6, 5, 5, 2, 5, 2, 5); break;                   // 霓虹描边
                case 31: (padL, padR, padT, padB, exL, exR, exT, exB) = (12, 12, 7, 7, 16, 16, 7, 7); break;                 // 叶饰牌
                case 32: (padL, padR, padT, padB, exL, exR, exT, exB) = (2, 2, 2, 2, 14, 14, 4, 4); break; // 尖角丝带
            }
        }

        /// <summary>特殊形正文区（与 CustomPads 对齐；默认外框内缩 1px）。</summary>
        private static RectangleF CustomBody(int sp, RectangleF box)
        {
            float x = box.X, y = box.Y, w = box.Width, h = box.Height;
            switch (sp)
            {
                case 1: return new RectangleF(x + 9, y + 11, w - 18, h - 18);
                case 2: return new RectangleF(x + 9, y + 11, w - 18, h - 28);
                case 3: case 5: return new RectangleF(x + 13, y + 13, w - 26, h - 24);
                case 4: return new RectangleF(x + 13, y + 13, w - 26, h - 34);
                case 6: return new RectangleF(x + 15, y + 15, w - 30, h - 28);
                case 7: return new RectangleF(x + w * 0.237f, y + h * 0.288f, w * 0.526f, h * 0.385f);
                case 8: return new RectangleF(x + w * 0.237f, y + h * 0.264f, w * 0.526f, h * 0.455f);
                case 9: return new RectangleF(x + 4, y + 3, w - 8, h - 6);
                case 10: return new RectangleF(x + 4, y + 3, w - 8, h - 16);
                case 11: return new RectangleF(x + 11, y, w - 22, h);
                case 12: return new RectangleF(x + 16, y + 7, w - 32, h - 14);
                case 13: return new RectangleF(x + 16, y + 7, w - 32, h - 24);
                case 14: return new RectangleF(x + 4, y + 4, w - 8, h - 8);
                case 15: return new RectangleF(x + 4, y + 4, w - 8, h - 18);
                case 16: return new RectangleF(x + 9, y + 11, w - 18, h - 18);
                case 17: return new RectangleF(x + 11, y + 9, w - 22, h - 18);
                case 18: case 19: return new RectangleF(x, y, w - 16, h - 14);
                case 20: return new RectangleF(x + 1, y + 1, w - 2, h - 14);
                case 21: return new RectangleF(x + 12, y, w - 12, h);
                case 22: return new RectangleF(x, y, w - 12, h);
                case 23: return new RectangleF(x + 13, y + 2, w - 19, h - 4);
                case 24: return new RectangleF(x + 7, y + 5, w - 14, h - 10);
                case 25: return new RectangleF(x + 8, y + 5, w - 16, h - 26);
                case 26: return new RectangleF(x + 7, y + 9, w - 14, h - 14);
                case 27: return new RectangleF(x + 5, y + 5, w - 10, h - 10);
                case 28: return new RectangleF(x + 5, y + 5, w - 10, h - 20);
                case 29: return new RectangleF(x, y, w - 5, h - 5);
                case 31: return new RectangleF(x + 16, y + 7, w - 32, h - 14);
                case 32: return new RectangleF(x + 14, y + 4, w - 28, h - 8);
            }
            return new RectangleF(x + 1, y + 1, w - 2, h - 2);
        }

        /// <summary>特殊形分派：32 种参考图外形（云朵/爆炸芒/星牌/横幅/卷轴/游戏牌/墨迹/双气泡/箭头/票券等）。</summary>
        private static void PaintCustom(Graphics g, int sp, RectangleF box, HTipColors pc)
        {
            var body = CustomBody(sp, box);
            switch (sp)
            {
                case 1: CloudPath(g, box, 10f, 0f, pc, false); break;
                case 2: CloudPath(g, box, 10f, 0f, pc, true); break;
                case 16: CloudPath(g, box, 10f, 1.6f, pc, false); break;
                case 3: Burst(g, box, body, 7, 0.86f, false, pc, 0f); break;
                case 4: Burst(g, box, body, 7, 0.86f, true, pc, 0f); break;
                case 5: Burst(g, box, body, 14, 0.94f, false, pc, 0.45f); break;
                case 6: Burst(g, box, body, 12, 0.79f, false, pc, 0f); break;
                case 7: StarPlaque(g, box, 5, 0.55f, pc); break;
                case 8: StarPlaque(g, box, 6, 0.575f, pc); break;
                case 9: Banner(g, box, false, pc); break;
                case 10: Banner(g, box, true, pc); break;
                case 11: Scroll(g, box, pc); break;
                case 12: Plaque(g, box, body, false, pc); break;
                case 13: Plaque(g, box, body, true, pc); break;
                case 14: InkRect(g, box, false, pc); break;
                case 15: InkRect(g, box, true, pc); break;
                case 17: InkOval(g, box, pc); break;
                case 18: DoubleBubble(g, box, false, pc); break;
                case 19: DoubleBubble(g, box, true, pc); break;
                case 20: ChatDot(g, box, body, pc); break;
                case 21: ArrowTag(g, box, true, false, pc); break;
                case 22: ArrowTag(g, box, false, false, pc); break;
                case 23: ArrowTag(g, box, true, true, pc); break;
                case 24: Ticket(g, box, pc); break;
                case 25: Shield(g, box, pc); break;
                case 26: FolderTab(g, box, pc); break;
                case 27: Note(g, box, body, false, pc); break;
                case 28: Note(g, box, body, true, pc); break;
                case 29: ShadowCard(g, box, pc); break;
                case 30: Neon(g, box, pc); break;
                case 31: LeafOnly(g, box, body, pc); break;
                case 32: RibbonEnds(g, box, pc); break;
            }
        }

        // ============ 特殊形小工具 ============

        /// <summary>带 alpha 的颜色。</summary>
        private static Color Ac(int a, Color c) => Color.FromArgb(a, c.R, c.G, c.B);

        /// <summary>多边形填充 + 描边。</summary>
        private static void Poly(Graphics g, PointF[] pts, Color bg, Color border, float bw = 1f)
        {
            using (var br = new SolidBrush(bg)) g.FillPolygon(br, pts);
            using (var p = new Pen(border, bw)) g.DrawPolygon(p, pts);
        }

        /// <summary>路径填充 + 描边。</summary>
        private static void FillStroke(Graphics g, GraphicsPath path, Color bg, Color border, float bw = 1f)
        {
            using (var br = new SolidBrush(bg)) g.FillPath(br, path);
            using (var p = new Pen(border, bw)) g.DrawPath(p, path);
        }

        /// <summary>下中尾巴（特殊形通用：先实底后两腰边线）。</summary>
        private static void CTail(Graphics g, RectangleF body, RectangleF box, HTipColors pc, float bw = 1f)
        {
            float cx = box.X + box.Width / 2f;
            var pts = new[] { new PointF(cx, box.Bottom - 1f), new PointF(cx - 9f, body.Bottom), new PointF(cx + 9f, body.Bottom) };
            using (var br = new SolidBrush(pc.Bg)) g.FillPolygon(br, pts);
            using (var p = new Pen(pc.Border, bw)) { g.DrawLine(p, pts[0], pts[1]); g.DrawLine(p, pts[0], pts[2]); }
        }

        /// <summary>确定性微抖动（重绘不爬）：振幅 ±amp。</summary>
        private static float Jit(int i, float amp)
        {
            float v = (float)(Math.Sin(i * 12.9898 + 78.233) * 43758.5453);
            return ((v - (float)Math.Floor(v)) - 0.5f) * 2f * amp;
        }

        // ============ 云朵 / 墨迹云 ============

        /// <summary>云朵/墨迹云：沿外周走点（顶部四个圆弧凸瓣 + 两侧微鼓 + 平底），闭合曲线柔化；可叠加确定性抖动。</summary>
        private static void CloudPath(Graphics g, RectangleF box, float lobeH, float jitter, HTipColors pc, bool tail)
        {
            float x = box.X + 7f, w = box.Width - 14f;
            float top = box.Y + 9f;
            float bot = box.Bottom - (tail ? 17f : 7f);
            var pts = new System.Collections.Generic.List<PointF>();
            int n = 56;
            for (int i = 0; i <= n; i++)
            {
                double t = (double)i / n; // 顺时针：底→右侧→顶瓣→左侧
                if (i == 0 || i == n) pts.Add(new PointF(x, bot + Jit(i, jitter)));
                else if (t < 0.22)
                {
                    double k = t / 0.22;
                    pts.Add(new PointF(x + w + (float)(2.2 * Math.Sin(k * Math.PI / 2)) + Jit(i, jitter),
                                       bot - (float)(k * (bot - top - lobeH * 0.2)) + Jit(i + 30, jitter)));
                }
                else if (t < 0.80)
                {
                    double k = (t - 0.22) / 0.58;           // 顶沿：右→左
                    double kk = 1.0 - k;
                    double phase = kk * 4.0;                // 四个凸瓣
                    double frac = phase - Math.Floor(phase);
                    double bump = Math.Cos(frac * Math.PI * 2.0 - Math.PI) * 0.5 + 0.5;
                    pts.Add(new PointF(x + (float)(w * kk) + Jit(i + 60, jitter),
                                       top - (float)(lobeH * bump) + Jit(i + 90, jitter)));
                }
                else
                {
                    double k = (t - 0.80) / 0.20;
                    pts.Add(new PointF(x - (float)(2.2 * Math.Sin(k * Math.PI / 2)) + Jit(i + 120, jitter),
                                       top + lobeH * 0.2f + (float)((1.0 - k) * (bot - top - lobeH * 0.2)) + Jit(i + 150, jitter)));
                }
            }
            using (var path = new GraphicsPath())
            {
                path.AddClosedCurve(pts.ToArray(), jitter > 0f ? 0.35f : 0.55f);
                FillStroke(g, path, pc.Bg, pc.Border);
            }
            if (tail) CTail(g, new RectangleF(x, top, w, bot - top), box, pc);
        }

        // ============ 爆炸芒 ============

        /// <summary>星芒/柔爆：绕中心交替长短半径；tension&gt;0 时柔化为圆瓣。</summary>
        private static void Burst(Graphics g, RectangleF box, RectangleF body, int spikes, float ratio, bool tail, HTipColors pc, float tension)
        {
            float cx = box.X + box.Width / 2f;
            float cy = body.Y + body.Height / 2f;
            // 椭圆芒星：水平半径撑满外框宽，垂直半径沿正文区上下外扩（让宽正文也在芒星实底内）
            float rrx = box.Width / 2f - 5f;
            float rry = body.Height / 2f + 6f;
            var pts = new PointF[spikes * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                double ang = -Math.PI / 2.0 + i * Math.PI / spikes;
                bool tip = i % 2 == 0;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * (tip ? rrx : rrx * ratio),
                                    cy + (float)Math.Sin(ang) * (tip ? rry : rry * ratio));
            }
            using (var path = new GraphicsPath())
            {
                if (tension > 0f) path.AddClosedCurve(pts, tension);
                else path.AddPolygon(pts);
                FillStroke(g, path, pc.Bg, pc.Border);
            }
            if (tail) CTail(g, body, box, pc);
        }

        // ============ 星牌 ============

        /// <summary>五角星/六角星牌：外尖角 + 内凹点交替。</summary>
        private static void StarPlaque(Graphics g, RectangleF box, int n, float rInRatio, HTipColors pc)
        {
            float cx = box.X + box.Width / 2f, cy = box.Y + box.Height / 2f + 1f;
            // 横纵独立半径：宽正文配宽星，避免文字超出星牌实底
            float rx = box.Width / 2f - 1.5f;
            float ry = box.Height / 2f - 2.5f;
            var pts = new PointF[n * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                double ang = -Math.PI / 2.0 + i * Math.PI / n;
                bool tip = i % 2 == 0;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * (tip ? rx : rx * rInRatio),
                                    cy + (float)Math.Sin(ang) * (tip ? ry : ry * rInRatio));
            }
            Poly(g, pts, pc.Bg, pc.Border);
        }

        // ============ 横幅 / 丝带 ============

        /// <summary>燕尾横幅：左直右 V 切口；可加下中尾。</summary>
        private static void Banner(Graphics g, RectangleF box, bool tail, HTipColors pc)
        {
            float x = box.X, y = box.Y + 1f, w = box.Width, h = box.Height - (tail ? 10f : 2f), mid = y + h / 2f;
            var pts = new[]
            {
                new PointF(x, y), new PointF(x + w - 12f, y), new PointF(x + w, mid),
                new PointF(x + w - 12f, y + h), new PointF(x, y + h)
            };
            Poly(g, pts, pc.Bg, pc.Border);
            if (tail) CTail(g, new RectangleF(x, y, w, h), box, pc);
        }

        /// <summary>两端 V 切的尖角丝带。</summary>
        private static void RibbonEnds(Graphics g, RectangleF box, HTipColors pc)
        {
            float x = box.X, y = box.Y + 2f, w = box.Width, h = box.Height - 4f, mid = y + h / 2f;
            var pts = new[]
            {
                new PointF(x + 12f, y), new PointF(x + w - 12f, y), new PointF(x + w, mid),
                new PointF(x + w - 12f, y + h), new PointF(x + 12f, y + h), new PointF(x, mid)
            };
            Poly(g, pts, pc.Bg, pc.Border);
        }

        // ============ 卷轴 ============

        /// <summary>卷轴牌：左右圆卷杆 + 中间圆角纸面。</summary>
        private static void Scroll(Graphics g, RectangleF box, HTipColors pc)
        {
            float cy = box.Y + box.Height / 2f;
            var lRoll = new RectangleF(box.X + 2f, cy - 8f, 18f, 16f);
            var rRoll = new RectangleF(box.Right - 20f, cy - 8f, 18f, 16f);
            using (var br = new SolidBrush(pc.Bg))
            {
                g.FillEllipse(br, lRoll);
                g.FillEllipse(br, rRoll);
            }
            using (var path = RoundRect(box.X + 11f, box.Y, box.Width - 22f, box.Height, 3f))
            using (var br = new SolidBrush(pc.Bg))
                g.FillPath(br, path);
            using (var p = new Pen(pc.Border, 1.2f))
            {
                g.DrawEllipse(p, lRoll);
                g.DrawEllipse(p, rRoll);
                g.DrawLine(p, lRoll.X + 9f, lRoll.Y + 2.5f, lRoll.X + 9f, lRoll.Bottom - 2.5f);
                g.DrawLine(p, rRoll.X + 9f, rRoll.Y + 2.5f, rRoll.X + 9f, rRoll.Bottom - 2.5f);
            }
            using (var path = RoundRect(box.X + 11f, box.Y, box.Width - 22f, box.Height, 3f))
            using (var p = new Pen(pc.Border, 1f))
                g.DrawPath(p, path);
        }

        // ============ 游戏标题牌 / 叶饰牌 ============

        /// <summary>游戏标题牌（参考绿色描边牌）：投影 + 顶高光带 + 双亮边 + 四角三叶饰 + 可选下中尾。</summary>
        private static void Plaque(Graphics g, RectangleF box, RectangleF body, bool tail, HTipColors pc)
        {
            float fx = body.X, fy = body.Y, fw = body.Width, fh = body.Height;
            // 投影
            using (var sh = RoundRect(fx + 2.5f, fy + 3f, fw, fh, 7f))
            using (var br = new SolidBrush(Ac(60, Color.Black)))
                g.FillPath(br, sh);
            // 主体
            using (var path = RoundRect(fx, fy, fw, fh, 7f))
            {
                using (var br = new SolidBrush(pc.Bg)) g.FillPath(br, path);
                // 顶高光带（裁在牌形内）
                var old = g.Clip;
                using (var rg = new Region(path))
                {
                    g.Clip = rg;
                    using (var band = new SolidBrush(Ac(26, Color.White)))
                        g.FillRectangle(band, fx, fy, fw, fh * 0.45f);
                }
                g.Clip = old;
                using (var p = new Pen(pc.Border, 2.2f)) g.DrawPath(p, path);
            }
            using (var inner = RoundRect(fx + 3.5f, fy + 3.5f, fw - 7f, fh - 7f, 4f))
            using (var p = new Pen(Ac(135, pc.Border), 1f))
                g.DrawPath(p, inner);
            CornerLeaves(g, fx, fy, fw, fh, pc.Border);
            if (tail) CTail(g, new RectangleF(fx, fy, fw, fh), box, pc, 1.6f);
        }

        /// <summary>叶饰牌：经典圆角身 + 四角叶饰（单亮边）。</summary>
        private static void LeafOnly(Graphics g, RectangleF box, RectangleF body, HTipColors pc)
        {
            using (var path = RoundRect(body.X, body.Y, body.Width, body.Height, 7f))
                FillStroke(g, path, pc.Bg, pc.Border);
            CornerLeaves(g, body.X, body.Y, body.Width, body.Height, pc.Border);
        }

        /// <summary>四角各三片叶饰（沿对角线向内展开的双线小叶，含叶脉）。</summary>
        private static void CornerLeaves(Graphics g, float x, float y, float w, float h, Color c)
        {
            using (var p = new Pen(Ac(200, c), 1.2f))
            {
                Leaf(g, p, new PointF(x + 3f, y + 11f), new PointF(x + 12f, y + 3f), 11f, 4f);
                Leaf(g, p, new PointF(x + 3f, y + 18f), new PointF(x + 18f, y + 3f), 10f, 3.6f);
                Leaf(g, p, new PointF(x + 10f, y + 3f), new PointF(x + 3f, y + 18f), 9.5f, 3.3f);
                Leaf(g, p, new PointF(x + w - 3f, y + 11f), new PointF(x + w - 12f, y + 3f), 11f, 4f);
                Leaf(g, p, new PointF(x + w - 3f, y + 18f), new PointF(x + w - 18f, y + 3f), 10f, 3.6f);
                Leaf(g, p, new PointF(x + w - 10f, y + 3f), new PointF(x + w - 3f, y + 18f), 9.5f, 3.3f);
                Leaf(g, p, new PointF(x + 3f, y + h - 11f), new PointF(x + 12f, y + h - 3f), 11f, 4f);
                Leaf(g, p, new PointF(x + 3f, y + h - 18f), new PointF(x + 18f, y + h - 3f), 10f, 3.6f);
                Leaf(g, p, new PointF(x + 10f, y + h - 3f), new PointF(x + 3f, y + h - 18f), 9.5f, 3.3f);
                Leaf(g, p, new PointF(x + w - 3f, y + h - 11f), new PointF(x + w - 12f, y + h - 3f), 11f, 4f);
                Leaf(g, p, new PointF(x + w - 3f, y + h - 18f), new PointF(x + w - 18f, y + h - 3f), 10f, 3.6f);
            }
        }

        /// <summary>一片双线小叶：根部 o 沿 d 方向画两条对称贝塞尔 + 中央叶脉。</summary>
        private static void Leaf(Graphics g, Pen p, PointF o, PointF d, float len, float wid)
        {
            float dx = d.X - o.X, dy = d.Y - o.Y;
            float nm = (float)Math.Sqrt(dx * dx + dy * dy);
            dx /= nm; dy /= nm;
            float nx = -dy, ny = dx;
            var tip = new PointF(o.X + dx * len, o.Y + dy * len);
            g.DrawBezier(p, o,
                new PointF(o.X + dx * len * 0.35f + nx * wid, o.Y + dy * len * 0.35f + ny * wid),
                new PointF(o.X + dx * len * 0.85f + nx * wid * 0.4f, o.Y + dy * len * 0.85f + ny * wid * 0.4f), tip);
            g.DrawBezier(p, o,
                new PointF(o.X + dx * len * 0.35f - nx * wid, o.Y + dy * len * 0.35f - ny * wid),
                new PointF(o.X + dx * len * 0.85f - nx * wid * 0.4f, o.Y + dy * len * 0.85f - ny * wid * 0.4f), tip);
            g.DrawLine(p, o.X + dx * 1.2f, o.Y + dy * 1.2f,
                          o.X + dx * len * 0.92f, o.Y + dy * len * 0.92f);
        }

        // ============ 墨迹 ============

        /// <summary>墨迹方框：四边按固定步长采样叠加确定性抖动 + 微抖闭合曲线；可加下中尾。</summary>
        private static void InkRect(Graphics g, RectangleF box, bool tail, HTipColors pc)
        {
            float x = box.X + 3f, y = box.Y + 3f, w = box.Width - 6f, h = box.Height - (tail ? 16f : 6f);
            var pts = new System.Collections.Generic.List<PointF>();
            int idx = 0;
            void Edge(float x1, float y1, float x2, float y2, bool horiz)
            {
                float len = horiz ? x2 - x1 : y2 - y1;
                int steps = Math.Max(2, (int)(Math.Abs(len) / 9f));
                for (int i = 0; i <= steps; i++)
                {
                    if (idx > 0 && i == steps) continue; // 角点不重复
                    float t = (float)i / steps;
                    float px = x1 + (x2 - x1) * t, py = y1 + (y2 - y1) * t;
                    pts.Add(new PointF(px + (horiz ? 0f : Jit(idx, 1.1f)), py + (horiz ? Jit(idx, 1.1f) : 0f)));
                    idx++;
                }
            }
            Edge(x, y, x + w, y, true);
            Edge(x + w, y, x + w, y + h, false);
            Edge(x + w, y + h, x, y + h, true);
            Edge(x, y + h, x, y, false);
            using (var path = new GraphicsPath())
            {
                path.AddClosedCurve(pts.ToArray(), 0.28f);
                FillStroke(g, path, pc.Bg, pc.Border, 1.2f);
            }
            if (tail) CTail(g, new RectangleF(x, y, w, h), box, pc, 1.2f);
        }

        /// <summary>墨迹椭圆：椭圆周采样 + 抖动。</summary>
        private static void InkOval(Graphics g, RectangleF box, HTipColors pc)
        {
            float cx = box.X + box.Width / 2f, cy = box.Y + box.Height / 2f;
            float rx = box.Width / 2f - 3f, ry = box.Height / 2f - 3f;
            var pts = new PointF[44];
            for (int i = 0; i < pts.Length; i++)
            {
                double ang = i * Math.PI * 2.0 / pts.Length;
                pts[i] = new PointF(cx + (float)Math.Cos(ang) * rx + Jit(i, 1.3f),
                                    cy + (float)Math.Sin(ang) * ry + Jit(i + 44, 1.3f));
            }
            using (var path = new GraphicsPath())
            {
                path.AddClosedCurve(pts, 0.32f);
                FillStroke(g, path, pc.Bg, pc.Border, 1.2f);
            }
        }

        // ============ 双气泡 ============

        /// <summary>双气泡组合（参考扁平双气泡图）：后层胶囊鬼影 + 前层圆角大气泡（带左下小尖尾）。</summary>
        private static void DoubleBubble(Graphics g, RectangleF box, bool ghostFill, HTipColors pc)
        {
            var front = new RectangleF(box.X, box.Y, box.Width - 16f, box.Height - 14f);
            var ghost = new RectangleF(box.X + box.Width * 0.30f, box.Y + box.Height * 0.34f,
                                       box.Width * 0.60f, box.Height * 0.52f);
            // 鬼影
            using (var gp = RoundRect(ghost.X, ghost.Y, ghost.Width, ghost.Height, ghost.Height / 2f))
            {
                using (var br = new SolidBrush(ghostFill ? Ac(85, pc.Accent) : pc.Bg)) g.FillPath(br, gp);
                using (var p = new Pen(pc.Border, 1.4f)) g.DrawPath(p, gp);
            }
            // 前层尾巴 + 本体
            var tailPts = new[]
            {
                new PointF(front.X + 16f, box.Bottom - 1f),
                new PointF(front.X + 11f, front.Bottom),
                new PointF(front.X + 27f, front.Bottom)
            };
            using (var br = new SolidBrush(pc.Bg)) g.FillPolygon(br, tailPts);
            using (var path = RoundRect(front.X, front.Y, front.Width, front.Height, 10f))
            using (var br = new SolidBrush(pc.Bg)) g.FillPath(br, path);
            using (var p = new Pen(pc.Border, 1.4f))
            {
                g.DrawLine(p, tailPts[0], tailPts[1]);
                g.DrawLine(p, tailPts[0], tailPts[2]);
            }
            using (var path = RoundRect(front.X, front.Y, front.Width, front.Height, 10f))
            using (var p = new Pen(pc.Border, 1.4f)) g.DrawPath(p, path);
        }

        // ============ 圆点拖尾 ============

        /// <summary>圆点拖尾：圆角本体 + 三颗递小实心点（无描边圈）。</summary>
        private static void ChatDot(Graphics g, RectangleF box, RectangleF body, HTipColors pc)
        {
            using (var path = RoundRect(body.X, body.Y, body.Width, body.Height, 7f))
                FillStroke(g, path, pc.Bg, pc.Border);
            var dots = new[]
            {
                new { X = body.X + 10f, Y = body.Bottom + 4.5f, R = 2.8f },
                new { X = body.X + 4f,  Y = body.Bottom + 9f,   R = 2f },
                new { X = body.X + 0.5f, Y = body.Bottom + 12f, R = 1.2f },
            };
            using (var br = new SolidBrush(pc.Border))
                foreach (var d in dots)
                    g.FillEllipse(br, d.X - d.R, d.Y - d.R, d.R * 2f, d.R * 2f);
        }

        // ============ 箭头牌 / 标签牌 ============

        /// <summary>左/右向箭头牌（left=true 尖在左）；tag=true 时右头改圆头并打挂孔。</summary>
        private static void ArrowTag(Graphics g, RectangleF box, bool left, bool tag, HTipColors pc)
        {
            float x = box.X, y = box.Y + 1f, w = box.Width, h = box.Height - 2f, mid = y + h / 2f;
            if (left)
            {
                var pts = new[]
                {
                    new PointF(x, mid), new PointF(x + 13f, y), new PointF(x + w - (tag ? 9f : 0f), y),
                    new PointF(x + w - (tag ? 9f : 0f), y + h), new PointF(x + 13f, y + h)
                };
                using (var path = new GraphicsPath())
                {
                    path.AddPolygon(pts);
                    if (tag)
                    {
                        path.AddArc(x + w - 18f, y, 18f, h, 270, 180);
                        path.CloseFigure();
                    }
                    FillStroke(g, path, pc.Bg, pc.Border);
                }
                if (tag)
                {
                    using (var br = new SolidBrush(pc.Bg)) g.FillEllipse(br, x + 11f, mid - 2.6f, 5.2f, 5.2f);
                    using (var p = new Pen(pc.Border, 1.1f)) g.DrawEllipse(p, x + 11f, mid - 2.6f, 5.2f, 5.2f);
                }
            }
            else
            {
                var pts = new[]
                {
                    new PointF(x, y), new PointF(x + w - 13f, y), new PointF(x + w, mid),
                    new PointF(x + w - 13f, y + h), new PointF(x, y + h)
                };
                Poly(g, pts, pc.Bg, pc.Border);
            }
        }

        // ============ 票券 ============

        /// <summary>票券：圆角纸面 + 左右中腰半圆撕口 + 水平虚线。</summary>
        private static void Ticket(Graphics g, RectangleF box, HTipColors pc)
        {
            float x = box.X + 1f, y = box.Y + 2f, w = box.Width - 2f, h = box.Height - 4f, r = 6f, mid = y + h / 2f;
            using (var path = new GraphicsPath())
            {
                path.AddArc(x, y, r * 2f, r * 2f, 180, 90);
                path.AddArc(x + w - r * 2f, y, r * 2f, r * 2f, 270, 90);
                path.AddLine(x + w, y + r, x + w, mid - 4f);
                path.AddArc(x + w - 8f, mid - 8f, 8f, 8f, 270f, 180f);   // 右侧内咬半圆
                path.AddLine(x + w, mid + 4f, x + w, y + h - r);
                path.AddArc(x + w - r * 2f, y + h - r * 2f, r * 2f, r * 2f, 0, 90);
                path.AddLine(x + w - r, y + h, x + r, y + h);
                path.AddArc(x, y + h - r * 2f, r * 2f, r * 2f, 90, 90);
                path.AddLine(x, mid + 4f, x, mid - 4f);
                path.AddArc(x, mid - 8f, 8f, 8f, 90f, 180f);               // 左侧内咬半圆
                path.CloseFigure();
                FillStroke(g, path, pc.Bg, pc.Border);
            }
            using (var p = new Pen(Ac(150, pc.Border), 1f) { DashStyle = DashStyle.Dot })
                g.DrawLine(p, x + 12f, mid, x + w - 12f, mid);
        }

        // ============ 盾牌 ============

        /// <summary>盾牌：平肩圆肩 + 弧收底尖。</summary>
        private static void Shield(Graphics g, RectangleF box, HTipColors pc)
        {
            float x = box.X + 2f, y = box.Y + 2f, w = box.Width - 4f;
            var pts = new[]
            {
                new PointF(x + 7f, y + 5f), new PointF(x + w - 7f, y + 5f),
                new PointF(x + w - 5f, y + box.Height * 0.52f),
                new PointF(x + w / 2f, box.Bottom - 2f),
                new PointF(x + 5f, y + box.Height * 0.52f)
            };
            using (var path = new GraphicsPath())
            {
                path.AddClosedCurve(pts, 0.28f);
                FillStroke(g, path, pc.Bg, pc.Border, 1.2f);
            }
        }

        // ============ 文件夹标签 ============

        /// <summary>文件夹标签：顶部凸舌 + 圆角纸身。</summary>
        private static void FolderTab(Graphics g, RectangleF box, HTipColors pc)
        {
            float x = box.X, y = box.Y, w = box.Width, h = box.Height, r = 6f;
            using (var path = new GraphicsPath())
            {
                path.StartFigure();
                path.AddLine(x + 12f, y + 10f, x + 18f, y + 3f);
                path.AddLine(x + 18f, y + 3f, x + 52f, y + 3f);
                path.AddLine(x + 52f, y + 3f, x + 58f, y + 10f);
                path.AddLine(x + 58f, y + 10f, x + w - r, y + 10f);
                path.AddArc(x + w - r * 2f, y + 10f, r * 2f, r * 2f, 270, 90);
                path.AddLine(x + w, y + 10f + r, x + w, y + h - r);
                path.AddArc(x + w - r * 2f, y + h - r * 2f, r * 2f, r * 2f, 0, 90);
                path.AddLine(x + w - r, y + h, x + r, y + h);
                path.AddArc(x, y + h - r * 2f, r * 2f, r * 2f, 90, 90);
                path.AddLine(x, y + 10f + r, x, y + 10f);
                path.CloseFigure();
                FillStroke(g, path, pc.Bg, pc.Border);
            }
        }

        // ============ 折角便签 ============

        /// <summary>折角便签：圆角纸面 + 右上折角（阴影三角）；可加下中尾。</summary>
        private static void Note(Graphics g, RectangleF box, RectangleF body, bool tail, HTipColors pc)
        {
            float x = box.X + 2f, y = box.Y + 2f, w = box.Width - 4f, h = box.Height - (tail ? 14f : 4f);
            using (var path = RoundRect(x, y, w, h, 4f))
                FillStroke(g, path, pc.Bg, pc.Border);
            // 折角
            var fold = new[] { new PointF(x + w - 15f, y + 1f), new PointF(x + w - 1f, y + 15f), new PointF(x + w - 15f, y + 15f) };
            using (var br = new SolidBrush(Ac(55, pc.Border))) g.FillPolygon(br, fold);
            using (var p = new Pen(pc.Border, 1f))
            {
                g.DrawLine(p, fold[0], fold[1]);
                g.DrawLine(p, fold[0], fold[2]);
            }
            if (tail) CTail(g, new RectangleF(x, y, w, h), box, pc);
        }

        // ============ 投影卡片 / 霓虹 ============

        /// <summary>投影卡片：经典圆角 + 右下柔影。</summary>
        private static void ShadowCard(Graphics g, RectangleF box, HTipColors pc)
        {
            using (var sh = RoundRect(box.X + 2.5f, box.Y + 3f, box.Width - 4f, box.Height - 4f, 7f))
            using (var br = new SolidBrush(Ac(70, Color.Black)))
                g.FillPath(br, sh);
            using (var path = RoundRect(box.X, box.Y, box.Width - 5f, box.Height - 5f, 7f))
                FillStroke(g, path, pc.Bg, pc.Border);
        }

        /// <summary>霓虹描边：三层辉光边 + 强调色亮线。</summary>
        private static void Neon(Graphics g, RectangleF box, HTipColors pc)
        {
            using (var path = RoundRect(box.X + 2f, box.Y + 2f, box.Width - 7f, box.Height - 7f, 7f))
            {
                using (var br = new SolidBrush(pc.Bg)) g.FillPath(br, path);
                using (var p = new Pen(Ac(50, pc.Accent), 4.5f)) g.DrawPath(p, path);
                using (var p = new Pen(Ac(105, pc.Accent), 2.4f)) g.DrawPath(p, path);
                using (var p = new Pen(pc.Accent, 1.2f)) g.DrawPath(p, path);
            }
        }
    }
}
