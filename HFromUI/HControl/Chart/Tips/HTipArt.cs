using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace HFromUI.HControl.Chart.Tips
{
    using HFromUI.HLangage;
    using HFromUI.HControl.Chart.Menu;
    /// <summary>
    /// 气泡角饰/插图绘图（108 种）：24 种 Tips 专属手绘装饰（星芒/靶心/灯泡/花瓣/折角等）
    /// + 84 种复用 540 矢量图标库的语义插图；None=不显示装饰（默认，与提取前一致）。
    /// 所有绘图以 16×16 坐标系描述，实际绘制时映射到气泡右上角 13px 小框。
    /// </summary>
    public enum HTipArt
    {
        /// <summary>无装饰（默认）。</summary>
        None = 0,
        // ----- 专属手绘装饰 24 -----
        Sparkle, StarFill, Dots3, DotRing, Ring, DiamondFill, Heart2, Bulb, Bolt, Leaf2,
        Flower, Quote, Bullets, CornerFold, Wave, Target2, Crosshair2, Sun3, Crown, Pin2,
        Eye2, Clock2, Key, Thumb,
        // ----- 图标库插图 84 -----
        IInfo, IWarning, IQuestion, IBell, IStar, IHeart, IFlag, IBookmark, IPin, ITarget,
        ICrosshair, IMapPin, IGlobe, ITag, IClock, ICalendar, IMail, ISun, IMoon, ICloud,
        ITrendingUp, IActivity, IDashboard, IDatabase, IEye, ISearch, IZoomIn, IZoomOut, ILayers, IGrid,
        IList, ITable, IImage, ICamera, IFilter, ISliders, ISettings, IBrush, IHome, ILock,
        IUnlock, IPrint, ILink, IFolder, IFile, ISaveAs, IDownload, IUpload, IWifi, ICompass,
        ISun2, ICloud2, ICloudRain, ISnow, ILightning, IWind, IUmbrella, IRainbow, IDroplet, ITornado,
        IGauge, IDonut, IFunnel, IPyramid, IBubble2, IHeat, ITreemap, ISankey, IKpi, IHistogram,
        IMoonStars, IPartlyDay, IThermoHot, ISigma, IPercent, IInfinity, IStar5Fill, IDiamondFill2, IHexaFill, IPentaFill,
        IToggle, ISparkle, ILeaf, ICactus
    }
    /// <summary>装饰绘图提供者：绘制 108 种角饰 + 菜单缩略位图。</summary>
    public static class HTipArts
    {
        /// <summary>专属装饰数（枚举前 24 个非 None 项走自绘）。</summary>
        private const int CustomCount = 24;
        /// <summary>全部可选项（含 None 之外的 108 种）。</summary>
        public static readonly HTipArt[] All = BuildAll();
        /// <summary>84 个图标库插图对应的 glyph（与枚举顺序严格对应）。</summary>
        private static readonly HMenuGlyph[] Glyphs =
        {
            HMenuGlyph.Info, HMenuGlyph.Warning, HMenuGlyph.Question, HMenuGlyph.Bell,
            HMenuGlyph.Star, HMenuGlyph.Heart, HMenuGlyph.Flag, HMenuGlyph.Bookmark,
            HMenuGlyph.Pin, HMenuGlyph.Target, HMenuGlyph.Crosshair, HMenuGlyph.MapPin,
            HMenuGlyph.Globe, HMenuGlyph.Tag, HMenuGlyph.Clock, HMenuGlyph.Calendar,
            HMenuGlyph.Mail, HMenuGlyph.Sun, HMenuGlyph.Moon, HMenuGlyph.Cloud,
            HMenuGlyph.TrendingUp, HMenuGlyph.Activity, HMenuGlyph.Dashboard, HMenuGlyph.Database,
            HMenuGlyph.Eye, HMenuGlyph.Search, HMenuGlyph.ZoomIn, HMenuGlyph.ZoomOut,
            HMenuGlyph.Layers, HMenuGlyph.Grid, HMenuGlyph.List, HMenuGlyph.Table,
            HMenuGlyph.Image, HMenuGlyph.Camera, HMenuGlyph.Filter, HMenuGlyph.Sliders,
            HMenuGlyph.Settings, HMenuGlyph.Brush, HMenuGlyph.Home, HMenuGlyph.Lock,
            HMenuGlyph.Unlock, HMenuGlyph.Print, HMenuGlyph.Link, HMenuGlyph.Folder,
            HMenuGlyph.File, HMenuGlyph.SaveAs, HMenuGlyph.Download, HMenuGlyph.Upload,
            HMenuGlyph.Wifi, HMenuGlyph.Compass,
            HMenuGlyph.VSun2, HMenuGlyph.VCloud2, HMenuGlyph.VCloudRain, HMenuGlyph.VSnow,
            HMenuGlyph.VLightning, HMenuGlyph.VWind, HMenuGlyph.VUmbrella, HMenuGlyph.VRainbow,
            HMenuGlyph.VDroplet, HMenuGlyph.VTornado,
            HMenuGlyph.VGauge, HMenuGlyph.VDonut, HMenuGlyph.VFunnel, HMenuGlyph.VPyramid,
            HMenuGlyph.VBubble2, HMenuGlyph.VHeat, HMenuGlyph.VTreemap, HMenuGlyph.VSankey,
            HMenuGlyph.VKpi, HMenuGlyph.VHistogram, HMenuGlyph.VMoonStars, HMenuGlyph.VPartlyDay,
            HMenuGlyph.VThermoHot, HMenuGlyph.VSigma, HMenuGlyph.VPercent, HMenuGlyph.VInfinity,
            HMenuGlyph.VShpStar5Fill, HMenuGlyph.VShpDiamondFill, HMenuGlyph.VShpHexaFill, HMenuGlyph.VShpPentaFill,
            HMenuGlyph.VToggleOn, HMenuGlyph.VSparkle, HMenuGlyph.VLeaf, HMenuGlyph.VCactus
        };
        /// <summary>构造 108 项顺序表。</summary>
        private static HTipArt[] BuildAll()
        {
            var arr = (HTipArt[])Enum.GetValues(typeof(HTipArt));
            var list = new List<HTipArt>();
            foreach (var a in arr) if (a != HTipArt.None) list.Add(a);
            return list.ToArray();
        }
        /// <summary>装饰显示名（中文）。</summary>
        public static string Name(HTipArt a)
        {
            switch (a)
            {
                case HTipArt.None: return HTranslation.GetContent("无装饰");
                case HTipArt.Sparkle: return HTranslation.GetContent("四角星光");
                case HTipArt.StarFill: return HTranslation.GetContent("实心小星");
                case HTipArt.Dots3: return HTranslation.GetContent("三点横排");
                case HTipArt.DotRing: return HTranslation.GetContent("圆点环");
                case HTipArt.Ring: return HTranslation.GetContent("空心圆环");
                case HTipArt.DiamondFill: return HTranslation.GetContent("实心菱形");
                case HTipArt.Heart2: return HTranslation.GetContent("小爱心");
                case HTipArt.Bulb: return HTranslation.GetContent("灯泡");
                case HTipArt.Bolt: return HTranslation.GetContent("闪电");
                case HTipArt.Leaf2: return HTranslation.GetContent("嫩叶");
                case HTipArt.Flower: return HTranslation.GetContent("四瓣花");
                case HTipArt.Quote: return HTranslation.GetContent("引号");
                case HTipArt.Bullets: return HTranslation.GetContent("项目点列");
                case HTipArt.CornerFold: return HTranslation.GetContent("折角");
                case HTipArt.Wave: return HTranslation.GetContent("波浪");
                case HTipArt.Target2: return HTranslation.GetContent("靶心");
                case HTipArt.Crosshair2: return HTranslation.GetContent("十字瞄准");
                case HTipArt.Sun3: return HTranslation.GetContent("小太阳");
                case HTipArt.Crown: return HTranslation.GetContent("小皇冠");
                case HTipArt.Pin2: return HTranslation.GetContent("定位针");
                case HTipArt.Eye2: return HTranslation.GetContent("眼睛");
                case HTipArt.Clock2: return HTranslation.GetContent("小时钟");
                case HTipArt.Key: return HTranslation.GetContent("钥匙");
                case HTipArt.Thumb: return HTranslation.GetContent("点赞");
            }
            int gi = (int)a - (CustomCount + 1);
            if (gi >= 0 && gi < GlyphNames.Length) return GlyphNames[gi];
            return a.ToString();
        }
        /// <summary>84 个插图中文名（与 Glyphs 数组对应）。</summary>
        private static readonly string[] GlyphNames =
        {
            HTranslation.GetContent("信息"), HTranslation.GetContent("警告"), HTranslation.GetContent("问号"), HTranslation.GetContent("铃铛"), HTranslation.GetContent("五角星"), HTranslation.GetContent("爱心"), HTranslation.GetContent("旗帜"), HTranslation.GetContent("书签"), HTranslation.GetContent("图钉"), HTranslation.GetContent("靶标"),
            HTranslation.GetContent("十字准星"), HTranslation.GetContent("地图针"), HTranslation.GetContent("地球"), HTranslation.GetContent("标签"), HTranslation.GetContent("时钟"), HTranslation.GetContent("日历"), HTranslation.GetContent("邮件"), HTranslation.GetContent("太阳"), HTranslation.GetContent("月亮"), HTranslation.GetContent("云"),
            HTranslation.GetContent("趋势向上"), HTranslation.GetContent("波形"), HTranslation.GetContent("仪表盘"), HTranslation.GetContent("数据库"), HTranslation.GetContent("眼睛"), HTranslation.GetContent("搜索"), HTranslation.GetContent("放大"), HTranslation.GetContent("缩小"), HTranslation.GetContent("图层"), HTranslation.GetContent("网格"),
            HTranslation.GetContent("列表"), HTranslation.GetContent("表格"), HTranslation.GetContent("图片"), HTranslation.GetContent("相机"), HTranslation.GetContent("筛选"), HTranslation.GetContent("滑块"), HTranslation.GetContent("设置"), HTranslation.GetContent("画刷"), HTranslation.GetContent("主页"), HTranslation.GetContent("锁"),
            HTranslation.GetContent("解锁"), HTranslation.GetContent("打印"), HTranslation.GetContent("链接"), HTranslation.GetContent("文件夹"), HTranslation.GetContent("文件"), HTranslation.GetContent("另存"), HTranslation.GetContent("下载"), HTranslation.GetContent("上传"), HTranslation.GetContent("无线"), HTranslation.GetContent("罗盘"),
            HTranslation.GetContent("晴天"), HTranslation.GetContent("多云"), HTranslation.GetContent("下雨"), HTranslation.GetContent("下雪"), HTranslation.GetContent("雷电"), HTranslation.GetContent("风"), HTranslation.GetContent("雨伞"), HTranslation.GetContent("彩虹"), HTranslation.GetContent("水滴"), HTranslation.GetContent("龙卷风"),
            HTranslation.GetContent("仪表"), HTranslation.GetContent("圆环图"), HTranslation.GetContent("漏斗"), HTranslation.GetContent("金字塔"), HTranslation.GetContent("气泡"), HTranslation.GetContent("热力"), HTranslation.GetContent("矩形树图"), HTranslation.GetContent("桑基图"), HTranslation.GetContent("KPI卡"), HTranslation.GetContent("直方图"),
            HTranslation.GetContent("星月夜"), HTranslation.GetContent("晴间多云"), HTranslation.GetContent("高温"), HTranslation.GetContent("求和"), HTranslation.GetContent("百分号"), HTranslation.GetContent("无穷"), HTranslation.GetContent("五角星块"), HTranslation.GetContent("菱形块"), HTranslation.GetContent("六边形块"), HTranslation.GetContent("五边形块"),
            HTranslation.GetContent("开关"), HTranslation.GetContent("闪烁星"), HTranslation.GetContent("叶子"), HTranslation.GetContent("仙人掌")
        };
        /// <summary>在指定矩形内绘制装饰（rect 约 13×13）。</summary>
        public static void Draw(Graphics g, RectangleF rect, HTipArt art, Color ink, Color accent)
        {
            if (art == HTipArt.None) return;
            var state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(rect.X, rect.Y);
            g.ScaleTransform(rect.Width / 16f, rect.Height / 16f);
            int idx = (int)art - 1;
            if (idx < CustomCount) DrawCustom(g, idx, ink, accent);
            else HMenuIcon.DrawIcon(Glyphs[idx - CustomCount], g, ink, accent);
            g.Restore(state);
        }
        /// <summary>菜单缩略位图（16×16，随菜单配色着色）。</summary>
        public static Bitmap Tile(HTipArt art, Color ink, Color accent)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Draw(g, new RectangleF(0.5f, 0.5f, 15f, 15f), art, ink, accent);
            }
            return bmp;
        }
        // ============ 24 种专属手绘装饰（16×16 坐标系） ============
        /// <summary>按编号绘制专属装饰。</summary>
        private static void DrawCustom(Graphics g, int i, Color k, Color a)
        {
            switch (i)
            {
                case 0: Sparkle(g, k); break;          // 四角星光
                case 1: StarSmall(g, k); break;        // 实心小星
                case 2: Dots(g, k); break;             // 三点横排
                case 3: DotRing(g, k); break;          // 圆点环
                case 4: RingOnly(g, k); break;         // 圆环
                case 5: Diamond(g, k, true); break;    // 实菱形
                case 6: HeartSmall(g, k); break;       // 爱心
                case 7: Bulb(g, k, a); break;          // 灯泡
                case 8: BoltSmall(g, a); break;        // 闪电
                case 9: LeafSmall(g, k); break;        // 嫩叶
                case 10: Flower(g, k, a); break;       // 四瓣花
                case 11: Quote(g, k); break;           // 引号
                case 12: Bullets(g, k); break;         // 点列
                case 13: Fold(g, k); break;            // 折角
                case 14: Wave(g, k); break;            // 波浪
                case 15: Target(g, k, a); break;       // 靶心
                case 16: Crosshair(g, k); break;       // 十字瞄准
                case 17: SunSmall(g, k); break;        // 太阳
                case 18: Crown(g, k); break;           // 皇冠
                case 19: PinSmall(g, k, a); break;     // 定位针
                case 20: EyeSmall(g, k); break;        // 眼睛
                case 21: ClockSmall(g, k); break;      // 时钟
                case 22: KeySmall(g, k); break;        // 钥匙
                case 23: ThumbSmall(g, k); break;      // 点赞
            }
        }
        /// <summary>四角星光（描边十字星）。</summary>
        private static void Sparkle(Graphics c, Color k)
        {
            var pts = new[] { new PointF(8, 1.5f), new PointF(9.6f, 6.4f), new PointF(14.5f, 8f), new PointF(9.6f, 9.6f),
                              new PointF(8, 14.5f), new PointF(6.4f, 9.6f), new PointF(1.5f, 8f), new PointF(6.4f, 6.4f) };
            using (var br = new SolidBrush(k)) c.FillPolygon(br, pts);
        }
        /// <summary>实心小五角星。</summary>
        private static void StarSmall(Graphics c, Color k)
        {
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double ang = -Math.PI / 2 + i * Math.PI / 5;
                float r = (i % 2 == 0) ? 6.5f : 2.7f;
                pts[i] = new PointF(8 + (float)Math.Cos(ang) * r, 8 + (float)Math.Sin(ang) * r);
            }
            using (var br = new SolidBrush(k)) c.FillPolygon(br, pts);
        }
        /// <summary>三点横排。</summary>
        private static void Dots(Graphics c, Color k)
        {
            using (var br = new SolidBrush(k))
                foreach (var x in new[] { 4f, 8f, 12f }) c.FillEllipse(br, x - 1.6f, 6.4f, 3.2f, 3.2f);
        }
        /// <summary>圆点 + 外环。</summary>
        private static void DotRing(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.3f)) c.DrawEllipse(p, 2.5f, 2.5f, 11f, 11f);
            using (var br = new SolidBrush(k)) c.FillEllipse(br, 6.2f, 6.2f, 3.6f, 3.6f);
        }
        /// <summary>空心圆环。</summary>
        private static void RingOnly(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.6f)) c.DrawEllipse(p, 3f, 3f, 10f, 10f);
        }
        /// <summary>菱形（fill=true 实底）。</summary>
        private static void Diamond(Graphics c, Color k, bool fill)
        {
            var pts = new[] { new PointF(8, 2f), new PointF(14, 8f), new PointF(8, 14f), new PointF(2, 8f) };
            if (fill) using (var br = new SolidBrush(k)) c.FillPolygon(br, pts);
            else using (var p = new Pen(k, 1.4f)) c.DrawPolygon(p, pts);
        }
        /// <summary>小爱心。</summary>
        private static void HeartSmall(Graphics c, Color k)
        {
            using (var path = new GraphicsPath())
            using (var br = new SolidBrush(k))
            {
                path.AddBezier(4.6f, 5.4f, 4.2f, 4.2f, 5.6f, 4f, 6.6f, 5f);
                path.AddBezier(6.6f, 5f, 7.6f, 4f, 9.2f, 4.2f, 11.2f, 5.6f);
                path.AddBezier(11.2f, 5.6f, 12.6f, 7.2f, 10.4f, 9.6f, 8f, 11.6f);
                path.AddBezier(8f, 11.6f, 5.6f, 9.6f, 3.4f, 7.4f, 4.6f, 5.4f);
                path.CloseFigure();
                c.FillPath(br, path);
            }
        }
        /// <summary>灯泡（玻璃泡 + 灯座 + 光线）。</summary>
        private static void Bulb(Graphics c, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f))
            {
                c.DrawEllipse(p, 4.6f, 2f, 6.8f, 6.8f);
                c.DrawLine(p, 6.2f, 9.6f, 9.8f, 9.6f);
                c.DrawLine(p, 6.5f, 11.4f, 9.5f, 11.4f);
                c.DrawLine(p, 2.6f, 3.6f, 1.2f, 2.6f);
                c.DrawLine(p, 13.4f, 3.6f, 14.8f, 2.6f);
            }
            using (var br = new SolidBrush(a)) c.FillRectangle(br, 6.6f, 12.6f, 2.8f, 1.6f);
        }
        /// <summary>小闪电（强调色）。</summary>
        private static void BoltSmall(Graphics c, Color a)
        {
            using (var br = new SolidBrush(a))
                c.FillPolygon(br, new[] { new PointF(9f, 1f), new PointF(4.4f, 8.4f), new PointF(7.6f, 8.4f),
                                          new PointF(6f, 15f), new PointF(12f, 6.2f), new PointF(8.6f, 6.2f) });
        }
        /// <summary>两片嫩叶。</summary>
        private static void LeafSmall(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.3f))
            {
                c.DrawBezier(p, 3f, 12f, 3f, 6f, 7f, 3f, 12f, 3f);
                c.DrawLine(p, 3f, 12f, 10f, 5f);
                c.DrawBezier(p, 6f, 14f, 8f, 10f, 11f, 8.5f, 14f, 8.5f);
            }
        }
        /// <summary>四瓣花（深色描边瓣 + 强调色花心）。</summary>
        private static void Flower(Graphics c, Color k, Color a)
        {
            var o = new PointF(8, 8);
            foreach (var d in new[] { new PointF(0, -4.6f), new PointF(4.6f, 0), new PointF(0, 4.6f), new PointF(-4.6f, 0) })
            {
                var ce = new PointF(o.X + d.X, o.Y + d.Y);
                using (var p = new Pen(k, 1.2f)) c.DrawEllipse(p, ce.X - 2.4f, ce.Y - 2.4f, 4.8f, 4.8f);
            }
            using (var br = new SolidBrush(a)) c.FillEllipse(br, 6.6f, 6.6f, 2.8f, 2.8f);
        }
        /// <summary>引号（两个方块逗号）。</summary>
        private static void Quote(Graphics c, Color k)
        {
            using (var br = new SolidBrush(k))
            {
                c.FillRectangle(br, 3.4f, 3.6f, 3.4f, 4.4f);
                c.FillEllipse(br, 3f, 7f, 3.2f, 3.2f);
                c.FillRectangle(br, 9.2f, 3.6f, 3.4f, 4.4f);
                c.FillEllipse(br, 8.8f, 7f, 3.2f, 3.2f);
            }
        }
        /// <summary>项目点列（三点 + 三短线）。</summary>
        private static void Bullets(Graphics c, Color k)
        {
            using (var br = new SolidBrush(k))
            using (var p = new Pen(k, 1.3f))
                for (int i = 0; i < 3; i++)
                {
                    float y = 4.2f + i * 3.8f;
                    c.FillEllipse(br, 2.2f, y - 1.2f, 2.4f, 2.4f);
                    c.DrawLine(p, 6f, y, 13.6f, y);
                }
        }
        /// <summary>折角（右上）。</summary>
        private static void Fold(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.4f))
            {
                c.DrawLine(p, 2.5f, 3f, 10.5f, 3f);
                c.DrawLine(p, 10.5f, 3f, 13.5f, 6f);
                c.DrawLine(p, 10.5f, 3f, 10.5f, 6f);
                c.DrawLine(p, 13.5f, 6f, 10.5f, 6f);
                c.DrawLine(p, 2.5f, 3f, 2.5f, 13f);
                c.DrawLine(p, 2.5f, 13f, 13.5f, 13f);
                c.DrawLine(p, 13.5f, 6f, 13.5f, 13f);
            }
        }
        /// <summary>波浪线。</summary>
        private static void Wave(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                c.DrawBeziers(p, new[]
                {
                    new PointF(1.5f, 10f), new PointF(4f, 4f), new PointF(6.5f, 4f), new PointF(8f, 8f),
                    new PointF(9.5f, 12f), new PointF(12f, 12f), new PointF(14.5f, 6f)
                });
        }
        /// <summary>靶心（三环 + 强调色心）。</summary>
        private static void Target(Graphics c, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f)) { c.DrawEllipse(p, 1.6f, 1.6f, 12.8f, 12.8f); c.DrawEllipse(p, 5f, 5f, 6f, 6f); }
            using (var br = new SolidBrush(a)) c.FillEllipse(br, 6.8f, 6.8f, 2.4f, 2.4f);
        }
        /// <summary>十字瞄准。</summary>
        private static void Crosshair(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.3f))
            {
                c.DrawEllipse(p, 3.4f, 3.4f, 9.2f, 9.2f);
                c.DrawLine(p, 8, 0.8f, 8, 3.6f); c.DrawLine(p, 8, 12.4f, 8, 15.2f);
                c.DrawLine(p, 0.8f, 8, 3.6f, 8); c.DrawLine(p, 12.4f, 8, 15.2f, 8);
            }
        }
        /// <summary>小太阳。</summary>
        private static void SunSmall(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.3f))
            {
                c.DrawEllipse(p, 5.4f, 5.4f, 5.2f, 5.2f);
                for (int i = 0; i < 8; i++)
                {
                    double ang = i * Math.PI / 4;
                    c.DrawLine(p, 8 + (float)Math.Cos(ang) * 3.6f, 8 + (float)Math.Sin(ang) * 3.6f,
                                  8 + (float)Math.Cos(ang) * 5.6f, 8 + (float)Math.Sin(ang) * 5.6f);
                }
            }
        }
        /// <summary>小皇冠。</summary>
        private static void Crown(Graphics c, Color k)
        {
            using (var br = new SolidBrush(k))
                c.FillPolygon(br, new[]
                {
                    new PointF(2.5f, 11.5f), new PointF(2.5f, 5f), new PointF(5.6f, 8f),
                    new PointF(8f, 3.4f), new PointF(10.4f, 8f), new PointF(13.5f, 5f),
                    new PointF(13.5f, 11.5f)
                });
            using (var p = new Pen(k, 1.2f)) c.DrawLine(p, 2.5f, 12.8f, 13.5f, 12.8f);
        }
        /// <summary>定位针。</summary>
        private static void PinSmall(Graphics c, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f))
            {
                c.DrawEllipse(p, 4.2f, 1.6f, 7.6f, 7.6f);
                c.DrawLine(p, 6.4f, 8.8f, 8f, 14.6f);
                c.DrawLine(p, 9.6f, 8.8f, 8f, 14.6f);
            }
            using (var br = new SolidBrush(a)) c.FillEllipse(br, 6.6f, 4f, 2.8f, 2.8f);
        }
        /// <summary>眼睛。</summary>
        private static void EyeSmall(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.3f))
                c.DrawBezier(p, 1.5f, 8f, 4f, 3.6f, 12f, 3.6f, 14.5f, 8f);
            using (var p = new Pen(k, 1.3f))
                c.DrawBezier(p, 1.5f, 8f, 4f, 12.4f, 12f, 12.4f, 14.5f, 8f);
            using (var br = new SolidBrush(k)) c.FillEllipse(br, 6.6f, 6.4f, 2.8f, 3.2f);
        }
        /// <summary>小时钟。</summary>
        private static void ClockSmall(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.3f))
            {
                c.DrawEllipse(p, 2.4f, 2.4f, 11.2f, 11.2f);
                c.DrawLine(p, 8, 8, 8, 4.4f);
                c.DrawLine(p, 8, 8, 10.8f, 9.4f);
            }
        }
        /// <summary>钥匙。</summary>
        private static void KeySmall(Graphics c, Color k)
        {
            using (var p = new Pen(k, 1.4f))
            {
                c.DrawEllipse(p, 1.8f, 6.2f, 4.6f, 4.6f);
                c.DrawLine(p, 6f, 10.4f, 13.6f, 2.8f);
                c.DrawLine(p, 11f, 5.4f, 12.6f, 7f);
                c.DrawLine(p, 9.2f, 7.2f, 10.6f, 8.6f);
            }
        }
        /// <summary>点赞（简化拇指）。</summary>
        private static void ThumbSmall(Graphics c, Color k)
        {
            using (var br = new SolidBrush(k))
                c.FillPolygon(br, new[]
                {
                    new PointF(3f, 7.4f), new PointF(6.4f, 7.4f), new PointF(7.2f, 2.8f),
                    new PointF(9.4f, 2.6f), new PointF(9.8f, 7.4f), new PointF(12.6f, 7.4f),
                    new PointF(13.2f, 8.4f), new PointF(12.4f, 12.6f), new PointF(3f, 12.6f)
                });
        }
    }
}
