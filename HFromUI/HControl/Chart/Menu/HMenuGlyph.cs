using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.Chart.Menu
{
    /// <summary>菜单矢量图标枚举：共 130 个 GDI+ 自绘 glyph（28 个图表专用 + 102 个通用）。</summary>
    public enum HMenuGlyph
    {
        // ===== 图表专用 28 =====
        Rubber, Fit, Play, Pause, RulerV, RulerH, Clear, Eye, Save, Legend,
        Wheel, AxisX, AxisY, Palette, Restore, ChartType,
        TypeLine, TypeSpline, TypeStep, TypeArea, TypeColumn, TypeBar,
        TypePoint, TypePie, TypeStackedArea, TypeStackedColumn, TypeRadar, TypeCandlestick,

        // ===== 文件/编辑 11 =====
        New, Open, SaveAs, Cut, Copy, Paste, Clipboard, Undo, Redo, Delete, Edit,

        // ===== 常规操作 10 =====
        Plus, Minus, Check, Close, Search, Refresh, Settings, Sliders, Filter, Sort,

        // ===== 数据/传输 10 =====
        Print, Export, Import, Download, Upload, Cloud, Link, Attachment, Folder, File,

        // ===== 媒体 8 =====
        Stop, Record, FastForward, Rewind, Eject, Volume, Shuffle, Repeat,

        // ===== 视图/窗口 20 =====
        ZoomIn, ZoomOut, Maximize, Minimize, Move,
        ExpandDown, CollapseUp, ChevronRight, ChevronLeft,
        ArrowUp, ArrowDown, ArrowLeft, ArrowRight,
        EyeOff, Sun, Moon, Grid, List, Table, Layers,

        // ===== 标记/导航 13 =====
        Home, Target, Crosshair, Compass, MapPin, Globe,
        Flag, Bookmark, Star, Heart, Pin, Bell, Tag,

        // ===== 用户/信息 8 =====
        User, Users, Mail, Clock, Calendar, Info, Warning, Question,

        // ===== 设备 8 =====
        Monitor, Smartphone, Cpu, Database, HardDrive, Wifi, Camera, Image,

        // ===== 绘图/开发 12 =====
        Brush, PenTool, Eraser, EyeDropper, Crop, Scissors,
        Code, Terminal, GitBranch, Dashboard, Activity, TrendingUp,

        // ===== 安全 2 =====
        Lock, Unlock,

        // ======================================================================
        // 扩展矢量图标（数据驱动 HVec 引擎绘制，共 410 个，编号 130..539）
        // 分组：箭头64 / 雪佛龙32 / 折角16 / 容器符号72 / 数字20 / 三角12 /
        //       多边形32 / 月相8 / 天气24 / 数学30 / 图表36 / 排版24 / 进出16 / 杂项24
        // ======================================================================

        // ----- 箭头族 64：8 方向（上/右上/右/右下/下/左下/左/左上）× 8 样式 -----
        _VArrowStart = 130,
        VArU = 130, VArUR, VArR, VArDR, VArD, VArDL, VArL, VArUL,
        VArUDash, VArURDash, VArRDash, VArDRDash, VArDDash, VArDLDash, VArLDash, VArULDash,
        VArUThin, VArURThin, VArRThin, VArDRThin, VArDThin, VArDLThin, VArLThin, VArULThin,
        VArUBold, VArURBold, VArRBold, VArDRBold, VArDBold, VArDLBold, VArLBold, VArULBold,
        VArUDouble, VArURDouble, VArRDouble, VArDRDouble, VArDDouble, VArDLDouble, VArLDouble, VArULDouble,
        VArUCurve, VArURCurve, VArRCurve, VArDRCurve, VArDCurve, VArDLCurve, VArLCurve, VArULCurve,
        VArUCirc, VArURCirc, VArRCirc, VArDRCirc, VArDCirc, VArDLCirc, VArLCirc, VArULCirc,
        VArUSq, VArURSq, VArRSq, VArDRSq, VArDSq, VArDLSq, VArLSq, VArULSq,

        // ----- 雪佛龙/三角指针族 32：8 方向 × 4 样式（单/双/三线/实底） -----
        _VChevronStart = 194,
        VChU = 194, VChUR, VChR, VChDR, VChD, VChDL, VChL, VChUL,
        VChU2, VChUR2, VChR2, VChDR2, VChD2, VChDL2, VChL2, VChUL2,
        VChU3, VChUR3, VChR3, VChDR3, VChD3, VChDL3, VChL3, VChUL3,
        VChUFill, VChURFill, VChRFill, VChDRFill, VChDFill, VChDLFill, VChLFill, VChULFill,

        // ----- 折角回转箭头族 16：4 角 × 4 样式 -----
        _VCornerStart = 226,
        VCnUL = 226, VCnUR, VCnLR, VCnLL,
        VCnULBold, VCnURBold, VCnLRBold, VCnLLBold,
        VCnULDash, VCnURDash, VCnLRDash, VCnLLDash,
        VCnULDouble, VCnURDouble, VCnLRDouble, VCnLLDouble,

        // ----- 容器符号族 72：12 符号（加/减/叉/勾/问/叹/信息/点/刷新/房子/星/心）× 6 容器（圆/实圆/方/实方/圆角方/实圆角方） -----
        _VContainStart = 242,
        VPlusCir = 242, VPlusCirFill, VPlusSq, VPlusSqFill, VPlusRd, VPlusRdFill,
        VMinusCir, VMinusCirFill, VMinusSq, VMinusSqFill, VMinusRd, VMinusRdFill,
        VCrossCir, VCrossCirFill, VCrossSq, VCrossSqFill, VCrossRd, VCrossRdFill,
        VCheckCir, VCheckCirFill, VCheckSq, VCheckSqFill, VCheckRd, VCheckRdFill,
        VQuestCir, VQuestCirFill, VQuestSq, VQuestSqFill, VQuestRd, VQuestRdFill,
        VBangCir, VBangCirFill, VBangSq, VBangSqFill, VBangRd, VBangRdFill,
        VInfoCir, VInfoCirFill, VInfoSq, VInfoSqFill, VInfoRd, VInfoRdFill,
        VDotCir, VDotCirFill, VDotSq, VDotSqFill, VDotRd, VDotRdFill,
        VSyncCir, VSyncCirFill, VSyncSq, VSyncSqFill, VSyncRd, VSyncRdFill,
        VHome2Cir, VHome2CirFill, VHome2Sq, VHome2SqFill, VHome2Rd, VHome2RdFill,
        VStar2Cir, VStar2CirFill, VStar2Sq, VStar2SqFill, VStar2Rd, VStar2RdFill,
        VHeart2Cir, VHeart2CirFill, VHeart2Sq, VHeart2SqFill, VHeart2Rd, VHeart2RdFill,

        // ----- 数字圆牌族 20：0..9 描边圆牌 + 实底圆牌 -----
        _VNumStart = 314,
        VNum0 = 314, VNum1, VNum2, VNum3, VNum4, VNum5, VNum6, VNum7, VNum8, VNum9,
        VNum0F, VNum1F, VNum2F, VNum3F, VNum4F, VNum5F, VNum6F, VNum7F, VNum8F, VNum9F,

        // ----- 三角播放族 12：4 方向 × 3 样式（描边/实底/带基线） -----
        _VTriStart = 334,
        VTriROut = 334, VTriRFill, VTriRBar, VTriDOut, VTriDFill, VTriDBar,
        VTriLOut, VTriLFill, VTriLBar, VTriUOut, VTriUFill, VTriUBar,

        // ----- 多边形族 32：8 形状（圆/方/菱/三角/五边/六边/五角星/六角星）× 4 样式（描边/实底/粗描边/双线） -----
        _VShapeStart = 346,
        VShpCircle = 346, VShpCircleFill, VShpCircleThick, VShpCircleDouble,
        VShpSquare, VShpSquareFill, VShpSquareThick, VShpSquareDouble,
        VShpDiamond, VShpDiamondFill, VShpDiamondThick, VShpDiamondDouble,
        VShpTri, VShpTriFill, VShpTriThick, VShpTriDouble,
        VShpPenta, VShpPentaFill, VShpPentaThick, VShpPentaDouble,
        VShpHexa, VShpHexaFill, VShpHexaThick, VShpHexaDouble,
        VShpStar5, VShpStar5Fill, VShpStar5Thick, VShpStar5Double,
        VShpStar6, VShpStar6Fill, VShpStar6Thick, VShpStar6Double,

        // ----- 月相族 8：新月→上弦→满月→下弦 -----
        _VMoonStart = 378,
        VMoon0 = 378, VMoon1, VMoon2, VMoon3, VMoon4, VMoon5, VMoon6, VMoon7,

        // ----- 天气/氛围族 24 -----
        _VWeatherStart = 386,
        VSun2 = 386, VSunCloud, VCloud2, VCloudRain, VHeavyRain, VDrizzle,
        VSnow, VLightning, VFog, VWind, VUmbrella, VMoonStars,
        VPartlyDay, VPartlyNight, VRainbow, VTornado, VThermoHot, VThermoCold,
        VDroplet, VDroplets, VCloudUp, VCloudDown, VBolt2, VSnow2,

        // ----- 数学符号族 30 -----
        _VMathStart = 410,
        VEq = 410, VNeq, VApprox, VLt, VGt, VLe, VGe, VDivide,
        VPercent, VPermille, VInfinity, VSigma, VIntegral, VSqrt, VPi2, VDelta2,
        VAngle, VParallel, VPerp, VSine, VFunction, VHash, VAmp, VAsterisk,
        VAt, VBull, VTilde, VDegree, VPrime, VEtc,

        // ----- 扩展图表族 36 -----
        _VChartStart = 440,
        VGauge = 440, VDonut, VFunnel, VPyramid, VScatter2, VBubble2,
        VHeat, VTreemap, VSankey, VKpi, VHistogram, VStepArea,
        VLinePoint, VMultiLine, VBarLine, VRangeBar, VErrorBar, VTrendDown,
        VTrendFlat, VMinMaxLine, VAvgLine, VThreshold, VMarker2, VCallout2,
        VZoomArea, VSplit2, VGridDots, VGridDash, VAxes3D, VPieHalf,
        VRadar2, VPolyArea, VWaterfall, VDotPlot, VRose, VZoomXY,

        // ----- 排版/对齐族 24 -----
        _VUiStart = 476,
        VBold = 476, VItalic, VUnderline2, VStrike, VText, VHeading,
        VListNum, VListCheck, VQuote, VAlignL, VAlignC, VAlignR,
        VAlignJ, VAlignTop, VAlignMid, VAlignBottom, VDistH, VDistV,
        VIndent, VOutdent, VFontSize, VCase, VHighlight, VPilcrow,

        // ----- 进出/登录族 16：4 方向 × 登录/退出/进入/离开 -----
        _VLoginStart = 500,
        VLoginR = 500, VLoginL, VLoginT, VLoginB,
        VLogoutR, VLogoutL, VLogoutT, VLogoutB,
        VEnterR, VEnterL, VEnterT, VEnterB,
        VExitR, VExitL, VExitT, VExitB,

        // ----- 杂项语义族 24（含图表右键菜单专用：夕阳/沙丘/矩阵/卷轴/叶/仙人掌/色板/气泡/开关/闪光 + 电池5 + 信号4 + 设备4） -----
        _VMiscStart = 516,
        VSunset = 516, VDune, VMatrix2, VScroll, VLeaf, VCactus,
        VSwatches, VTip, VToggleOn, VSparkle,
        VBatt0, VBatt25, VBatt50, VBatt100, VBattCharge,
        VSignal0, VSignal1, VSignal2, VSignal3,
        VMic, VHeadset, VKeyboard, VProjector
    }

    /// <summary>
    /// 菜单图标工厂：16×16 透明底 GDI+ 矢量绘图。
    /// 墨色 ink 决定线条主色、强调色 accent 决定高亮元素，随菜单配色风格自动变色；
    /// 同参数位图缓存复用，菜单换配色时按新色取图即可。
    /// </summary>
    public static partial class HMenuIcon
    {
        /// <summary>语义色：播放绿（深/浅底均清晰）。</summary>
        private static readonly Color Green = Color.FromArgb(60, 170, 90);
        /// <summary>语义色：暂停橙。</summary>
        private static readonly Color Orange = Color.FromArgb(220, 150, 50);
        /// <summary>语义色：危险红。</summary>
        private static readonly Color Red = Color.FromArgb(205, 80, 80);
        /// <summary>图表多色系列 1（蓝）。</summary>
        private static readonly Color C1 = Color.FromArgb(70, 130, 210);
        /// <summary>图表多色系列 2（红橙）。</summary>
        private static readonly Color C2 = Color.FromArgb(220, 95, 75);
        /// <summary>图表多色系列 3（绿）。</summary>
        private static readonly Color C3 = Color.FromArgb(46, 184, 135);
        /// <summary>图表多色系列 4（紫）。</summary>
        private static readonly Color C4 = Color.FromArgb(149, 108, 220);
        /// <summary>图表多色系列 5（金黄）。</summary>
        private static readonly Color C5 = Color.FromArgb(235, 155, 52);

        /// <summary>位图缓存：按 glyph + 墨色 + 强调色 复用。</summary>
        private static readonly Dictionary<Tuple<HMenuGlyph, int, int>, Bitmap> _cache =
            new Dictionary<Tuple<HMenuGlyph, int, int>, Bitmap>();

        /// <summary>获取指定图标的 16×16 位图（透明底，结果缓存）。</summary>
        public static Bitmap Get(HMenuGlyph glyph, Color ink, Color accent)
        {
            var key = Tuple.Create(glyph, ink.ToArgb(), accent.ToArgb());
            if (!_cache.TryGetValue(key, out var bmp))
            {
                bmp = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    Draw(glyph, g, ink, accent);
                }
                _cache[key] = bmp;
            }
            return bmp;
        }

        // ============ 通用绘制小工具 ============

        /// <summary>圆角矩形路径。</summary>
        private static GraphicsPath Round(RectangleF r, float d)
        {
            var p = new GraphicsPath();
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        /// <summary>填充+描边圆角矩形。</summary>
        private static void FillStroke(Graphics g, RectangleF r, float d, Brush fill, Pen pen)
        {
            using (var path = Round(r, d))
            {
                if (fill != null) g.FillPath(fill, path);
                if (pen != null) g.DrawPath(pen, path);
            }
        }

        /// <summary>迷你坐标系：X/Y 轴线。</summary>
        private static void Axes(Graphics g, Color k)
        {
            using (var p = new Pen(k, 1f)) { g.DrawLine(p, 1, 14, 14, 14); g.DrawLine(p, 1, 2, 1, 14); }
        }

        /// <summary>白色纸张（带折角）+ 墨色描边。</summary>
        private static void Paper(Graphics g, Color k, float x, float y, float w, float h)
        {
            using (var p = new Pen(k, 1.1f))
            {
                g.FillRectangle(Brushes.White, x, y, w, h);
                g.DrawRectangle(p, x, y, w, h);
            }
        }

        // ============ 总调度 ============
        /// <summary>按枚举绘制对应 glyph（画布已开抗锯齿、透明底）。</summary>
        private static void Draw(HMenuGlyph id, Graphics g, Color k, Color a)
        {
            switch (id)
            {
                // ---------- 图表专用 ----------
                case HMenuGlyph.Rubber: DrawRubber(g, k, a); break;
                case HMenuGlyph.Fit: DrawFit(g, a); break;
                case HMenuGlyph.Play: DrawTriangle(g, Green); break;
                case HMenuGlyph.Pause: DrawPause(g); break;
                case HMenuGlyph.RulerV: DrawRulerV(g, k, a); break;
                case HMenuGlyph.RulerH: DrawRulerH(g, k, a); break;
                case HMenuGlyph.Clear: DrawCross(g, Red); break;
                case HMenuGlyph.Eye: DrawEye(g, k, a); break;
                case HMenuGlyph.Save: DrawSave(g, k); break;
                case HMenuGlyph.Legend: DrawLegend(g, k); break;
                case HMenuGlyph.Wheel: DrawWheel(g, k, a); break;
                case HMenuGlyph.AxisX: DrawDoubleArrow(g, k, a, true); break;
                case HMenuGlyph.AxisY: DrawDoubleArrow(g, k, a, false); break;
                case HMenuGlyph.Palette: DrawPalette(g, k); break;
                case HMenuGlyph.Restore: DrawRestore(g, a); break;
                case HMenuGlyph.ChartType: DrawLineChart(g, k, a); break;
                case HMenuGlyph.TypeLine: DrawLineChart(g, k, C1); break;
                case HMenuGlyph.TypeSpline: DrawSpline(g); break;
                case HMenuGlyph.TypeStep: DrawStep(g); break;
                case HMenuGlyph.TypeArea: DrawArea(g); break;
                case HMenuGlyph.TypeColumn: DrawColumns(g); break;
                case HMenuGlyph.TypeBar: DrawBars(g); break;
                case HMenuGlyph.TypePoint: DrawPoints(g); break;
                case HMenuGlyph.TypePie: DrawPie(g, k); break;
                case HMenuGlyph.TypeStackedArea: DrawStackedArea(g); break;
                case HMenuGlyph.TypeStackedColumn: DrawStackedColumns(g); break;
                case HMenuGlyph.TypeRadar: DrawRadar(g, k, a); break;
                case HMenuGlyph.TypeCandlestick: DrawCandle(g, k); break;

                // ---------- 文件/编辑 ----------
                case HMenuGlyph.New: DrawNew(g, k, a); break;
                case HMenuGlyph.Open: DrawFolder(g, k, a, true); break;
                case HMenuGlyph.SaveAs: DrawSaveAs(g, k, a); break;
                case HMenuGlyph.Cut: DrawScissors(g, k, a); break;
                case HMenuGlyph.Copy: DrawCopy(g, k); break;
                case HMenuGlyph.Paste: DrawClipboardBody(g, k, a, false); break;
                case HMenuGlyph.Clipboard: DrawClipboardBody(g, k, a, true); break;
                case HMenuGlyph.Undo: DrawUndoRedo(g, a, true); break;
                case HMenuGlyph.Redo: DrawUndoRedo(g, a, false); break;
                case HMenuGlyph.Delete: DrawTrash(g); break;
                case HMenuGlyph.Edit: DrawPencil(g, k, a); break;

                // ---------- 常规操作 ----------
                case HMenuGlyph.Plus: DrawPlusMinus(g, a, true); break;
                case HMenuGlyph.Minus: DrawPlusMinus(g, a, false); break;
                case HMenuGlyph.Check:
                    using (var p = new Pen(a, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawLines(p, new[] { new PointF(3, 8.5f), new PointF(6.5f, 12), new PointF(13, 3.5f) });
                    break;
                case HMenuGlyph.Close: DrawCross(g, k); break;
                case HMenuGlyph.Search: DrawSearch(g, k, a, false); break;
                case HMenuGlyph.Refresh: DrawRefresh(g, a); break;
                case HMenuGlyph.Settings: DrawGear(g, k); break;
                case HMenuGlyph.Sliders: DrawSliders(g, k, a); break;
                case HMenuGlyph.Filter: DrawFilter(g, k, a); break;
                case HMenuGlyph.Sort: DrawSort(g, k, a); break;

                // ---------- 数据/传输 ----------
                case HMenuGlyph.Print: DrawPrinter(g, k, a); break;
                case HMenuGlyph.Export: DrawTransfer(g, k, a, false); break;
                case HMenuGlyph.Import: DrawTransfer(g, k, a, true); break;
                case HMenuGlyph.Download: DrawUpDown(g, k, a, true); break;
                case HMenuGlyph.Upload: DrawUpDown(g, k, a, false); break;
                case HMenuGlyph.Cloud: DrawCloud(g, k); break;
                case HMenuGlyph.Link: DrawLink(g, a); break;
                case HMenuGlyph.Attachment: DrawAttachment(g, a); break;
                case HMenuGlyph.Folder: DrawFolder(g, k, a, false); break;
                case HMenuGlyph.File: DrawFile(g, k); break;

                // ---------- 媒体 ----------
                case HMenuGlyph.Stop:
                    FillStroke(g, new RectangleF(4, 4, 8, 8), 1.5f, new SolidBrush(Red), null); break;
                case HMenuGlyph.Record:
                    using (var br = new SolidBrush(Red)) g.FillEllipse(br, 3, 3, 10, 10); break;
                case HMenuGlyph.FastForward: DrawFFRew(g, a, true); break;
                case HMenuGlyph.Rewind: DrawFFRew(g, a, false); break;
                case HMenuGlyph.Eject:
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(3, 11), new PointF(13, 11), new PointF(8, 3) });
                    using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round }) g.DrawLine(p, 3, 13, 13, 13);
                    break;
                case HMenuGlyph.Volume: DrawVolume(g, k, a); break;
                case HMenuGlyph.Shuffle: DrawShuffle(g, a); break;
                case HMenuGlyph.Repeat: DrawRepeat(g, a); break;

                // ---------- 视图/窗口 ----------
                case HMenuGlyph.ZoomIn: DrawSearch(g, k, a, true); break;
                case HMenuGlyph.ZoomOut:
                    using (var p = new Pen(k, 1.5f)) g.DrawEllipse(p, 2, 2, 9, 9);
                    using (var p = new Pen(k, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    { g.DrawLine(p, 10.5f, 10.5f, 14, 14); g.DrawLine(p, 4.5f, 6.5f, 8.5f, 6.5f); }
                    break;
                case HMenuGlyph.Maximize: DrawMaximize(g, k, a); break;
                case HMenuGlyph.Minimize:
                    using (var p = new Pen(k, 1.4f)) g.DrawRectangle(p, 2.5f, 4.5f, 11f, 7f);
                    using (var p = new Pen(a, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawLine(p, 3, 13.5f, 13, 13.5f);
                    break;
                case HMenuGlyph.Move: DrawMove(g, a); break;
                case HMenuGlyph.ExpandDown: DrawChevron(g, k, 0); break;
                case HMenuGlyph.CollapseUp: DrawChevron(g, k, 180); break;
                case HMenuGlyph.ChevronRight: DrawChevron(g, k, 90); break;
                case HMenuGlyph.ChevronLeft: DrawChevron(g, k, 270); break;
                case HMenuGlyph.ArrowUp: DrawArrow(g, a, 270); break;
                case HMenuGlyph.ArrowDown: DrawArrow(g, a, 90); break;
                case HMenuGlyph.ArrowLeft: DrawArrow(g, a, 180); break;
                case HMenuGlyph.ArrowRight: DrawArrow(g, a, 0); break;
                case HMenuGlyph.EyeOff: DrawEyeOff(g, k); break;
                case HMenuGlyph.Sun: DrawSun(g, k, a); break;
                case HMenuGlyph.Moon: DrawMoon(g, a); break;
                case HMenuGlyph.Grid:
                    using (var p = new Pen(k, 1.3f))
                        for (int i = 0; i <= 3; i++)
                        { g.DrawLine(p, 2 + i * 4, 2, 2 + i * 4, 14); g.DrawLine(p, 2, 2 + i * 4, 14, 2 + i * 4); }
                    break;
                case HMenuGlyph.List:
                    using (var p = new Pen(k, 1.3f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        for (int i = 0; i < 3; i++) g.DrawLine(p, 5, 4.5f + i * 3.5f, 14, 4.5f + i * 3.5f);
                    using (var br = new SolidBrush(a))
                        for (int i = 0; i < 3; i++) g.FillEllipse(br, 1.2f, 3.4f + i * 3.5f, 2.2f, 2.2f);
                    break;
                case HMenuGlyph.Table: DrawTable(g, k, a); break;
                case HMenuGlyph.Layers: DrawLayers(g, k, a); break;

                // ---------- 标记/导航 ----------
                case HMenuGlyph.Home: DrawHome(g, k, a); break;
                case HMenuGlyph.Target: DrawTarget(g, k, a); break;
                case HMenuGlyph.Crosshair: DrawCrosshair(g, k, a); break;
                case HMenuGlyph.Compass: DrawCompass(g, k, a); break;
                case HMenuGlyph.MapPin: DrawMapPin(g, a); break;
                case HMenuGlyph.Globe: DrawGlobe(g, k, a); break;
                case HMenuGlyph.Flag: DrawFlag(g, k, a); break;
                case HMenuGlyph.Bookmark:
                    using (var br = new SolidBrush(a))
                        g.FillPolygon(br, new[] { new PointF(4, 2), new PointF(12, 2), new PointF(12, 14), new PointF(8, 11), new PointF(4, 14) });
                    break;
                case HMenuGlyph.Star: DrawStar(g, a); break;
                case HMenuGlyph.Heart: DrawHeart(g, Red); break;
                case HMenuGlyph.Pin: DrawPin(g, k, a); break;
                case HMenuGlyph.Bell: DrawBell(g, k, a); break;
                case HMenuGlyph.Tag: DrawTag(g, k, a); break;

                // ---------- 用户/信息 ----------
                case HMenuGlyph.User: DrawUser(g, k, a, false); break;
                case HMenuGlyph.Users:
                    DrawUser(g, Color.FromArgb(a.A, (int)(a.R * 0.7f), (int)(a.G * 0.8f), (int)(a.B * 0.9f)), a, true);
                    DrawUser(g, k, a, false); break;
                case HMenuGlyph.Mail: DrawMail(g, k, a); break;
                case HMenuGlyph.Clock: DrawClock(g, k, a); break;
                case HMenuGlyph.Calendar: DrawCalendar(g, k, a); break;
                case HMenuGlyph.Info: DrawInfo(g, a); break;
                case HMenuGlyph.Warning: DrawWarning(g); break;
                case HMenuGlyph.Question: DrawQuestion(g, a); break;

                // ---------- 设备 ----------
                case HMenuGlyph.Monitor: DrawMonitor(g, k, a); break;
                case HMenuGlyph.Smartphone: DrawPhone(g, k, a); break;
                case HMenuGlyph.Cpu: DrawCpu(g, k, a); break;
                case HMenuGlyph.Database: DrawDatabase(g, k, a); break;
                case HMenuGlyph.HardDrive: DrawHardDrive(g, k, a); break;
                case HMenuGlyph.Wifi: DrawWifi(g, k, a); break;
                case HMenuGlyph.Camera: DrawCamera(g, k, a); break;
                case HMenuGlyph.Image: DrawImage(g, k); break;

                // ---------- 绘图/开发 ----------
                case HMenuGlyph.Brush: DrawBrush(g, k, a); break;
                case HMenuGlyph.PenTool: DrawPenTool(g, k, a); break;
                case HMenuGlyph.Eraser: DrawEraser(g, k); break;
                case HMenuGlyph.EyeDropper: DrawDropper(g, k, a); break;
                case HMenuGlyph.Crop: DrawCrop(g, k, a); break;
                case HMenuGlyph.Scissors: DrawScissors(g, a, k); break;
                case HMenuGlyph.Code: DrawCode(g, a); break;
                case HMenuGlyph.Terminal: DrawTerminal(g, k, a); break;
                case HMenuGlyph.GitBranch: DrawGitBranch(g, a); break;
                case HMenuGlyph.Dashboard: DrawDashboard(g, k, a); break;
                case HMenuGlyph.Activity:
                    using (var p = new Pen(a, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        g.DrawLines(p, new[] { new PointF(1, 9), new PointF(4, 9), new PointF(6, 4), new PointF(9, 12), new PointF(11, 9), new PointF(15, 9) });
                    break;
                case HMenuGlyph.TrendingUp:
                    using (var p = new Pen(k, 1f)) g.DrawLine(p, 2, 13, 14, 13);
                    using (var p = new Pen(a, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        g.DrawLines(p, new[] { new PointF(2, 11), new PointF(6, 8), new PointF(9, 10.5f), new PointF(13, 4.5f) });
                        g.DrawLine(p, 10, 4.5f, 13, 4.5f); g.DrawLine(p, 13, 4.5f, 13, 7.5f);
                    }
                    break;

                // ---------- 安全 ----------
                case HMenuGlyph.Lock: DrawLock(g, k, a, false); break;
                case HMenuGlyph.Unlock: DrawLock(g, k, a, true); break;

                // ---------- 扩展矢量图标（410 个，数据驱动） ----------
                default: HVec.Paint(id, g, k, a); break;
            }
        }

        /// <summary>内部直绘入口：供 Tips 插图库等复用同一套矢量绘制（不经过位图缓存）。</summary>
        internal static void DrawIcon(HMenuGlyph id, Graphics g, Color ink, Color accent) => Draw(id, g, ink, accent);

        // ============ 图表专用 glyph ============

        private static void DrawRubber(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f) { DashStyle = DashStyle.Dash }) g.DrawRectangle(p, 1.5f, 1.5f, 9f, 8f);
            using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            { g.DrawLine(p, 9, 12, 15, 12); g.DrawLine(p, 12, 9, 12, 15); }
        }

       private static void DrawFit(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.6f) { EndCap = LineCap.ArrowAnchor })
            {
                g.DrawLine(p, 6.5f, 6.5f, 3, 3);
                g.DrawLine(p, 9.5f, 6.5f, 13, 3);
                g.DrawLine(p, 6.5f, 9.5f, 3, 13);
                g.DrawLine(p, 9.5f, 9.5f, 13, 13);
            }
        }

        private static void DrawTriangle(Graphics g, Color c)
        {
            using (var br = new SolidBrush(c))
                g.FillPolygon(br, new[] { new PointF(4, 2.5f), new PointF(4, 13.5f), new PointF(13, 8f) });
        }

        private static void DrawPause(Graphics g)
        {
            using (var br = new SolidBrush(Orange))
            {
                g.FillRectangle(br, 3.5f, 2.5f, 3.2f, 11f);
                g.FillRectangle(br, 9.3f, 2.5f, 3.2f, 11f);
            }
        }

        private static void DrawRulerV(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(a, 1.8f)) g.DrawLine(p, 8, 1.5f, 8, 14.5f);
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawLine(p, 8, 3, 11, 3); g.DrawLine(p, 8, 7, 10.5f, 7);
                g.DrawLine(p, 8, 11, 11, 11); g.DrawLine(p, 8, 14, 10.5f, 14);
            }
        }

        private static void DrawRulerH(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(a, 1.8f)) g.DrawLine(p, 1.5f, 8, 14.5f, 8);
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawLine(p, 3, 8, 3, 5); g.DrawLine(p, 7, 8, 7, 5.5f);
                g.DrawLine(p, 11, 8, 11, 5); g.DrawLine(p, 14, 8, 14, 5.5f);
            }
        }

        /// <summary>叉号（Clear/Close 共用，颜色语义不同）。</summary>
        private static void DrawCross(Graphics g, Color c)
        {
            using (var p = new Pen(c, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            { g.DrawLine(p, 3, 3, 13, 13); g.DrawLine(p, 13, 3, 3, 13); }
        }

        private static void DrawEye(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.4f))
            {
                g.DrawArc(p, 1.5f, 5f, 13f, 8f, 10, 160);
                g.DrawArc(p, 1.5f, 3f, 13f, 8f, 190, 160);
            }
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 6.2f, 6.2f, 3.6f, 3.6f);
        }

        private static void DrawSave(Graphics g, Color k)
        {
            using (var br = new SolidBrush(Color.FromArgb(235, 238, 242)))
            using (var p = new Pen(k, 1.2f))
            {
                g.FillRectangle(br, 2.5f, 1.5f, 11f, 13f);
                g.DrawRectangle(p, 2.5f, 1.5f, 11f, 13f);
                g.FillRectangle(Brushes.White, 5f, 2.5f, 6f, 3.5f);
                g.DrawRectangle(p, 5f, 2.5f, 6f, 3.5f);
                g.FillRectangle(Brushes.White, 4.5f, 9f, 7f, 5f);
                g.DrawRectangle(p, 4.5f, 9f, 7f, 5f);
            }
        }

        private static void DrawLegend(Graphics g, Color k)
        {
            Color[] cs = { C1, C2, C3 };
            using (var p = new Pen(k, 1.2f))
                for (int i = 0; i < 3; i++)
                {
                    float y = 3f + i * 4.5f;
                    using (var br = new SolidBrush(cs[i])) g.FillRectangle(br, 2f, y, 4f, 2.6f);
                    g.DrawLine(p, 8f, y + 1.3f, 14f, y + 1.3f);
                }
        }

        private static void DrawWheel(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.4f)) g.DrawEllipse(p, 3.5f, 2f, 9f, 12f);
            using (var p = new Pen(a, 1.5f))
            {
                g.DrawLine(p, 8, 4.5f, 8, 7.5f);
                g.DrawLine(p, 8, 8.5f, 8, 11.5f);
            }
        }

        /// <summary>双向箭头（AxisX/AxisY）。</summary>
        private static void DrawDoubleArrow(Graphics g, Color k, Color a, bool horizontal)
        {
            using (var p = new Pen(a, 1.7f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor })
                g.DrawLine(p, horizontal ? 2f : 8f, horizontal ? 8f : 2f, horizontal ? 14f : 8f, horizontal ? 8f : 14f);
        }

        private static void DrawPalette(Graphics g, Color k)
        {
            using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, 0, 0, 16, 16);
            using (var p = new Pen(k, 1f)) g.DrawEllipse(p, 0.5f, 0.5f, 15f, 15f);
            var cs = new[] { Color.FromArgb(230, 70, 60), Color.FromArgb(60, 160, 230), Color.FromArgb(80, 190, 110) };
            float[] xs = { 3f, 8f, 12f }, ys = { 8f, 3f, 9.5f };
            for (int i = 0; i < 3; i++)
                using (var br = new SolidBrush(cs[i])) g.FillEllipse(br, xs[i], ys[i], 5f, 5f);
        }

        private static void DrawRestore(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.6f) { EndCap = LineCap.ArrowAnchor })
            {
                g.DrawArc(p, 3, 3, 10, 10, 40, 280);
                g.DrawLine(p, 10, 10, 13, 11);
                g.DrawLine(p, 10, 10, 10, 13);
            }
        }

        private static void DrawLineChart(Graphics g, Color k, Color line)
        {
            Axes(g, k);
            using (var p = new Pen(line, 1.5f))
                g.DrawLines(p, new[] { new PointF(2, 11), new PointF(5, 7), new PointF(8, 9), new PointF(11, 4), new PointF(14, 6) });
        }

        private static void DrawSpline(Graphics g)
        {
            Axes(g, Color.FromArgb(72, 80, 92));
            using (var p = new Pen(C1, 1.5f)) g.DrawBezier(p, 2, 12, 4, 4, 10, 12, 14, 5);
        }

        private static void DrawStep(Graphics g)
        {
            Axes(g, Color.FromArgb(72, 80, 92));
            using (var p = new Pen(C1, 1.5f))
            {
                g.DrawLine(p, 2, 11, 2, 8); g.DrawLine(p, 2, 8, 5, 8);
                g.DrawLine(p, 5, 8, 5, 5); g.DrawLine(p, 5, 5, 9, 5);
                g.DrawLine(p, 9, 5, 9, 7); g.DrawLine(p, 9, 7, 13, 7);
            }
        }

        private static void DrawArea(Graphics g)
        {
            var k = Color.FromArgb(72, 80, 92);
            Axes(g, k);
            var pts = new[] { new PointF(2, 11), new PointF(5, 7), new PointF(8, 9), new PointF(11, 4), new PointF(14, 6) };
            using (var path = new GraphicsPath())
            {
                path.AddLine(pts[0].X, 14, pts[0].X, pts[0].Y);
                for (int i = 1; i < pts.Length; i++) path.AddLine(pts[i - 1], pts[i]);
                path.AddLine(pts[pts.Length - 1].X, pts[pts.Length - 1].Y, pts[pts.Length - 1].X, 14);
                path.CloseFigure();
                using (var br = new SolidBrush(Color.FromArgb(120, 70, 130, 210))) g.FillPath(br, path);
            }
            using (var p = new Pen(C1, 1.2f)) g.DrawLines(p, pts);
        }

        private static void DrawColumns(Graphics g)
        {
            Axes(g, Color.FromArgb(72, 80, 92));
            var cs = new[] { C1, C2, C3 };
            for (int i = 0; i < 3; i++)
            {
                float x = 3f + i * 4f, h = 3f + i * 2f;
                using (var br = new SolidBrush(cs[i])) g.FillRectangle(br, x, 14 - h, 3f, h);
            }
        }

        private static void DrawBars(Graphics g)
        {
            var k = Color.FromArgb(72, 80, 92);
            using (var p = new Pen(k, 1f)) { g.DrawLine(p, 1, 2, 14, 2); g.DrawLine(p, 1, 2, 1, 14); }
            var cs = new[] { C1, C2, C3 };
            for (int i = 0; i < 3; i++)
            {
                float y = 4f + i * 4f, w = 3f + i * 2f;
                using (var br = new SolidBrush(cs[i])) g.FillRectangle(br, 2, y, w, 2.5f);
            }
        }

        private static void DrawPoints(Graphics g)
        {
            Axes(g, Color.FromArgb(72, 80, 92));
            var cs = new[] { C1, C2, C3, C4, C5 };
            for (int i = 0; i < cs.Length; i++)
            {
                float x = 2f + i * 2.4f, y = 12f - i * 1.8f;
                using (var br = new SolidBrush(cs[i])) g.FillEllipse(br, x - 1.5f, y - 1.5f, 3f, 3f);
            }
        }

        private static void DrawPie(Graphics g, Color k)
        {
            using (var br = new SolidBrush(Color.FromArgb(230, 230, 230))) g.FillEllipse(br, 1, 1, 14, 14);
            using (var br = new SolidBrush(C1)) g.FillPie(br, 1, 1, 14, 14, 0, 120);
            using (var br = new SolidBrush(C2)) g.FillPie(br, 1, 1, 14, 14, 120, 90);
            using (var p = new Pen(k, 1f)) g.DrawEllipse(p, 1, 1, 14, 14);
        }

        private static void DrawStackedArea(Graphics g)
        {
            Axes(g, Color.FromArgb(72, 80, 92));
            var bottom = new[] { new PointF(2, 11), new PointF(5, 9), new PointF(8, 10), new PointF(11, 7), new PointF(14, 8) };
            var top = new[] { new PointF(2, 7), new PointF(5, 5), new PointF(8, 6), new PointF(11, 3), new PointF(14, 4) };
            using (var path = new GraphicsPath())
            {
                path.AddLine(bottom[0].X, 14, bottom[0].X, bottom[0].Y);
                for (int i = 1; i < bottom.Length; i++) path.AddLine(bottom[i - 1], bottom[i]);
                for (int i = top.Length - 1; i >= 0; i--) path.AddLine(top[i], i == 0 ? top[top.Length - 1] : top[i - 1]);
                path.CloseFigure();
                using (var br = new SolidBrush(Color.FromArgb(100, 220, 95, 75))) g.FillPath(br, path);
            }
        }

        private static void DrawStackedColumns(Graphics g)
        {
            Axes(g, Color.FromArgb(72, 80, 92));
            var cs = new[] { C1, C2 };
            for (int i = 0; i < 3; i++)
            {
                float x = 3f + i * 4f;
                using (var br = new SolidBrush(cs[0])) g.FillRectangle(br, x, 8f, 3f, 6f);
                using (var br = new SolidBrush(cs[1])) g.FillRectangle(br, x, 4f, 3f, 4f);
            }
        }

        private static void DrawRadar(Graphics g, Color k, Color a)
        {
            var pts = new[] { new PointF(8, 2), new PointF(14, 6), new PointF(12, 13), new PointF(4, 13), new PointF(2, 6) };
            using (var p = new Pen(k, 0.8f) { DashStyle = DashStyle.Dot })
            {
                g.DrawLine(p, pts[0], pts[2]); g.DrawLine(p, pts[1], pts[3]);
                g.DrawLine(p, pts[2], pts[4]); g.DrawLine(p, pts[3], pts[0]); g.DrawLine(p, pts[4], pts[1]);
            }
            using (var p = new Pen(a, 1.2f)) g.DrawPolygon(p, pts);
        }

        private static void DrawCandle(Graphics g, Color k)
        {
            using (var p = new Pen(k, 1f)) g.DrawLine(p, 1, 14, 14, 14);
            using (var up = new Pen(C2, 1f))
            using (var down = new Pen(Green, 1f))
            {
                g.DrawLine(up, 4, 2, 4, 12); g.FillRectangle(Brushes.White, 3f, 5f, 2f, 4f);
                g.DrawLine(down, 8, 4, 8, 14); g.FillRectangle(new SolidBrush(Green), 7f, 7f, 2f, 4f);
                g.DrawLine(up, 12, 3, 12, 13); g.FillRectangle(Brushes.White, 11f, 6f, 2f, 4f);
            }
        }

        // ============ 文件/编辑类 glyph ============

        private static void DrawNew(Graphics g, Color k, Color a)
        {
            Paper(g, k, 3, 1.5f, 9f, 13f);
            using (var p = new Pen(a, 1.7f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            { g.DrawLine(p, 11, 10.5f, 14.5f, 10.5f); g.DrawLine(p, 12.75f, 8.75f, 12.75f, 12.25f); }
        }

        /// <summary>文件夹（open=true 为开口文件夹，用于打开）。</summary>
        private static void DrawFolder(Graphics g, Color k, Color a, bool open)
        {
            using (var br = new SolidBrush(Color.FromArgb(70, a.R, a.G, a.B)))
            {
                if (open)
                {
                    g.FillPolygon(br, new[] { new PointF(2, 5.5f), new PointF(6.5f, 5.5f), new PointF(8, 7.5f), new PointF(14, 7.5f), new PointF(12.6f, 13), new PointF(2, 13) });
                    using (var p = new Pen(k, 1.2f))
                        g.DrawPolygon(p, new[] { new PointF(2, 5.5f), new PointF(6.5f, 5.5f), new PointF(8, 7.5f), new PointF(14, 7.5f), new PointF(12.6f, 13), new PointF(2, 13) });
                }
                else
                {
                    g.FillRectangle(br, 2, 5.5f, 12f, 8f);
                    g.FillRectangle(br, 2, 4f, 5.5f, 2f);
                    using (var p = new Pen(k, 1.2f))
                    {
                        g.DrawRectangle(p, 2, 5.5f, 12f, 8f);
                        g.DrawRectangle(p, 2, 4f, 5.5f, 2f);
                    }
                }
            }
        }

        private static void DrawSaveAs(Graphics g, Color k, Color a)
        {
            DrawSave(g, k);
            using (var p = new Pen(a, 1.6f) { EndCap = LineCap.ArrowAnchor })
                g.DrawLine(p, 10, 10, 14.5f, 5.5f);
        }

        /// <summary>剪刀（Cut/Scissors，两环颜色对调以示区别）。</summary>
        private static void DrawScissors(Graphics g, Color ring, Color blade)
        {
            using (var p = new Pen(ring, 1.3f))
            {
                g.DrawEllipse(p, 1.5f, 3.5f, 4f, 4f);
                g.DrawEllipse(p, 1.5f, 9f, 4f, 4f);
            }
            using (var p = new Pen(blade, 1.4f))
            {
                g.DrawLine(p, 5, 6, 14, 4.5f);
                g.DrawLine(p, 5, 10.5f, 14, 12);
                g.DrawLine(p, 5, 6, 5, 10.5f);
            }
        }

        private static void DrawCopy(Graphics g, Color k)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawRectangle(p, 5, 1.5f, 8.5f, 10f);
                g.FillRectangle(Brushes.White, 2, 4f, 8.5f, 10f);
                g.DrawRectangle(p, 2, 4f, 8.5f, 10f);
            }
        }

        /// <summary>写字板本体（Paste 无文字线，Clipboard 带三行线）。</summary>
        private static void DrawClipboardBody(Graphics g, Color k, Color a, bool lines)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.FillRectangle(Brushes.White, 3, 3.5f, 10f, 11f);
                g.DrawRectangle(p, 3, 3.5f, 10f, 11f);
            }
            using (var br = new SolidBrush(a))
            {
                FillStroke(g, new RectangleF(5.8f, 1.8f, 4.4f, 2.6f), 0.8f, br, null);
            }
            if (lines)
                using (var p = new Pen(k, 1.1f))
                    for (int i = 0; i < 3; i++) g.DrawLine(p, 5, 7f + i * 2.3f, 11, 7f + i * 2.3f);
        }

        private static void DrawUndoRedo(Graphics g, Color a, bool undo)
        {
            var ps = new[] { new PointF(13, 4f), new PointF(3, 8f), new PointF(13, 12f) };
            if (!undo) Array.Reverse(ps);
            using (var p = new Pen(a, 1.6f) { EndCap = LineCap.ArrowAnchor, StartCap = LineCap.Round })
                g.DrawCurve(p, ps, 0.6f);
        }

        private static void DrawTrash(Graphics g)
        {
            using (var p = new Pen(Red, 1.3f))
            {
                g.DrawLine(p, 2.8f, 4.5f, 13.2f, 4.5f);
                g.DrawLine(p, 5, 4.5f, 5.5f, 2.8f);
                g.DrawLine(p, 10.5f, 2.8f, 11f, 4.5f);
                g.DrawRectangle(p, 3.5f, 5.3f, 9f, 8.7f);
                g.DrawLine(p, 6.2f, 7f, 6.2f, 12f);
                g.DrawLine(p, 9.8f, 7f, 9.8f, 12f);
            }
        }

        private static void DrawPencil(Graphics g, Color k, Color a)
        {
            using (var br = new SolidBrush(Color.FromArgb(235, 180, 70)))
                g.FillPolygon(br, new[] { new PointF(2.5f, 13.5f), new PointF(3.5f, 10.5f), new PointF(11.5f, 2.5f), new PointF(13.5f, 4.5f), new PointF(5.5f, 12.5f) });
            using (var p = new Pen(k, 1.1f))
                g.DrawPolygon(p, new[] { new PointF(2.5f, 13.5f), new PointF(3.5f, 10.5f), new PointF(11.5f, 2.5f), new PointF(13.5f, 4.5f), new PointF(5.5f, 12.5f) });
        }

        // ============ 常规操作 glyph ============

        private static void DrawPlusMinus(Graphics g, Color a, bool plus)
        {
            using (var p = new Pen(a, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(p, 4, 8, 12, 8);
                if (plus) g.DrawLine(p, 8, 4, 8, 12);
            }
        }

        /// <summary>放大镜：zoom=true 时镜片内画加号。</summary>
        private static void DrawSearch(Graphics g, Color k, Color a, bool zoom)
        {
            using (var p = new Pen(k, 1.5f)) g.DrawEllipse(p, 1.5f, 1.5f, 9.5f, 9.5f);
            using (var p = new Pen(k, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 10.2f, 10.2f, 14, 14);
            if (zoom)
                using (var p = new Pen(a, 1.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                { g.DrawLine(p, 4.2f, 6.25f, 8.3f, 6.25f); g.DrawLine(p, 6.25f, 4.2f, 6.25f, 8.3f); }
        }

        private static void DrawRefresh(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.6f) { EndCap = LineCap.ArrowAnchor })
                g.DrawArc(p, 2.5f, 2.5f, 11f, 11f, 40, 285);
        }

        private static void DrawGear(Graphics g, Color k)
        {
            using (var p = new Pen(k, 1.5f))
            {
                for (int i = 0; i < 8; i++)
                {
                    double ang = i * Math.PI / 4;
                    float x1 = 8 + (float)Math.Cos(ang) * 5.6f, y1 = 8 + (float)Math.Sin(ang) * 5.6f;
                    float x2 = 8 + (float)Math.Cos(ang) * 7f, y2 = 8 + (float)Math.Sin(ang) * 7f;
                    g.DrawLine(p, x1, y1, x2, y2);
                }
                g.DrawEllipse(p, 3.2f, 3.2f, 9.6f, 9.6f);
            }
            using (var p = new Pen(k, 1.3f)) g.DrawEllipse(p, 5.6f, 5.6f, 4.8f, 4.8f);
        }

        private static void DrawSliders(Graphics g, Color k, Color a)
        {
            float[] ys = { 4f, 8f, 12f }, xs = { 10f, 5.5f, 11f };
            using (var line = new Pen(k, 1.2f))
            using (var br = new SolidBrush(a))
                for (int i = 0; i < 3; i++)
                {
                    g.DrawLine(line, 2, ys[i], 14, ys[i]);
                    FillStroke(g, new RectangleF(xs[i] - 1.6f, ys[i] - 1.6f, 3.2f, 3.2f), 1.6f, br, null);
                }
        }

        private static void DrawFilter(Graphics g, Color k, Color a)
        {
            var pts = new[] { new PointF(2, 2.5f), new PointF(14, 2.5f), new PointF(9.8f, 9), new PointF(9.8f, 13.6f), new PointF(6.2f, 12f), new PointF(6.2f, 9) };
            using (var br = new SolidBrush(Color.FromArgb(50, a.R, a.G, a.B))) g.FillPolygon(br, pts);
            using (var p = new Pen(k, 1.3f)) g.DrawPolygon(p, pts);
        }

        private static void DrawSort(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawLine(p, 2, 5f, 11f, 5f);
                g.DrawLine(p, 2, 8f, 8.5f, 8f);
                g.DrawLine(p, 2, 11f, 6f, 11f);
            }
            using (var p = new Pen(a, 1.5f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor })
                g.DrawLine(p, 13.2f, 3.5f, 13.2f, 12.5f);
        }

        // ============ 数据/传输 glyph ============

        private static void DrawPrinter(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.FillRectangle(Brushes.White, 4, 2f, 8f, 4f);
                g.DrawRectangle(p, 4, 2f, 8f, 4f);
                g.FillRectangle(new SolidBrush(Color.FromArgb(225, 230, 238)), 2, 6f, 12f, 5.5f);
                g.DrawRectangle(p, 2, 6f, 12f, 5.5f);
                g.FillRectangle(Brushes.White, 4, 9.5f, 8f, 4.5f);
                g.DrawRectangle(p, 4, 9.5f, 8f, 4.5f);
            }
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 11.3f, 7.7f, 1.6f, 1.6f);
        }

        /// <summary>导入/导出：托盘 + 方向箭头（in=true 向下导入）。</summary>
        private static void DrawTransfer(Graphics g, Color k, Color a, bool inBox)
        {
            using (var p = new Pen(k, 1.4f))
            {
                g.DrawLine(p, 3, 9f, 13f, 9f);
                g.DrawLine(p, 3, 9f, 3f, 13f);
                g.DrawLine(p, 3, 13f, 13f, 13f);
                g.DrawLine(p, 13, 9f, 13f, 13f);
            }
            DrawArrow(g, a, inBox ? 90 : 270, 8, 7.5f, 4.5f);
        }

        /// <summary>下载/上传：底部托盘线 + 箭头。</summary>
        private static void DrawUpDown(Graphics g, Color k, Color a, bool down)
        {
            DrawArrow(g, a, down ? 90 : 270, 8, 8, 6.5f);
            using (var p = new Pen(k, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(p, 2.5f, 13f, 13.5f, 13f);
                g.DrawLine(p, 3, 13f, 3f, 10.5f);
                g.DrawLine(p, 13, 13f, 13f, 10.5f);
            }
        }

        private static void DrawCloud(Graphics g, Color k)
        {
            var rects = new[] { new RectangleF(3.5f, 6.5f, 5.5f, 5.5f), new RectangleF(6.5f, 4f, 6f, 6f), new RectangleF(8.5f, 6.5f, 5f, 5.5f) };
            using (var path = new GraphicsPath())
            {
                foreach (var r in rects) path.AddEllipse(r);
                path.AddRectangle(new RectangleF(3.5f, 8f, 10f, 5f));
                using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
                using (var p = new Pen(k, 1.2f)) g.DrawPath(p, path);
            }
        }

        private static void DrawLink(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.5f))
            {
                g.DrawArc(p, 2, 4.5f, 6f, 6f, 90, 270);
                g.DrawArc(p, 8, 5.5f, 6f, 6f, -90, 270);
                g.DrawLine(p, 7.5f, 6f, 9f, 6f);
                g.DrawLine(p, 7f, 10f, 8.5f, 10f);
            }
        }

        private static void DrawAttachment(Graphics g, Color a)
        {
            // 整体绕中心旋转 -25°：回形针为长圆角杆 + 下端 U 形回钩
            var state = g.Save();
            g.TranslateTransform(8, 8);
            g.RotateTransform(-25f);
            g.TranslateTransform(-8, -8);
            using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var path = new GraphicsPath())
            {
                path.AddArc(4.5f, 1.5f, 4.5f, 4.5f, 180, 90);
                path.AddArc(7f, 1.5f, 4.5f, 4.5f, 270, 90);
                path.AddLine(11.5f, 6f, 11.5f, 11f);
                path.AddArc(6.5f, 8f, 5f, 5f, 0, 270);
                path.AddLine(6.5f, 10.5f, 6.5f, 4f);
                g.DrawPath(p, path);
            }
            g.Restore(state);
        }

        private static void DrawFile(Graphics g, Color k)
        {
            Paper(g, k, 3, 1.5f, 9f, 13f);
            using (var p = new Pen(k, 1.1f))
                for (int i = 0; i < 3; i++) g.DrawLine(p, 5, 6.5f + i * 2.5f, 10.5f, 6.5f + i * 2.5f);
        }

        // ============ 媒体 glyph ============

        private static void DrawFFRew(Graphics g, Color a, bool ff)
        {
            using (var br = new SolidBrush(a))
            {
                if (ff)
                {
                    g.FillPolygon(br, new[] { new PointF(2, 3), new PointF(8, 8), new PointF(2, 13) });
                    g.FillPolygon(br, new[] { new PointF(7.5f, 3), new PointF(13.5f, 8), new PointF(7.5f, 13) });
                }
                else
                {
                    g.FillPolygon(br, new[] { new PointF(14, 3), new PointF(8, 8), new PointF(14, 13) });
                    g.FillPolygon(br, new[] { new PointF(8.5f, 3), new PointF(2.5f, 8), new PointF(8.5f, 13) });
                }
            }
        }

        private static void DrawVolume(Graphics g, Color k, Color a)
        {
            using (var br = new SolidBrush(k))
                g.FillPolygon(br, new[] { new PointF(2, 6), new PointF(5, 6), new PointF(9, 2.8f), new PointF(9, 13.2f), new PointF(5, 10), new PointF(2, 10) });
            using (var p = new Pen(a, 1.3f))
            {
                g.DrawArc(p, 10, 5.5f, 3f, 5f, -30, 60);
                g.DrawArc(p, 11, 4f, 4.5f, 8f, -35, 70);
            }
        }

        private static void DrawShuffle(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.4f) { EndCap = LineCap.ArrowAnchor, StartCap = LineCap.Round })
            {
                g.DrawLine(p, 1.5f, 3.5f, 5f, 3.5f);
                g.DrawLine(p, 5f, 3.5f, 13.5f, 11.5f);
                g.DrawLine(p, 1.5f, 12.5f, 5f, 12.5f);
                g.DrawLine(p, 5f, 12.5f, 8.5f, 9f);
                g.DrawLine(p, 7.5f, 7f, 11f, 3.5f);
                g.DrawLine(p, 11f, 3.5f, 14.5f, 3.5f);
            }
        }

        private static void DrawRepeat(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.4f) { EndCap = LineCap.ArrowAnchor })
            {
                g.DrawLine(p, 4, 3.5f, 12.5f, 3.5f);
                g.DrawArc(p, 6.5f, 3.5f, 7f, 8f, -90, 180);
                g.DrawLine(p, 10, 11.5f, 4, 11.5f);
                g.DrawArc(p, 2.5f, 4.5f, 7f, 8f, 90, 180);
            }
        }

        // ============ 视图/窗口 glyph ============

        private static void DrawMaximize(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 2.5f, 2.5f, 9.5f, 9.5f);
            using (var p = new Pen(a, 1.4f) { EndCap = LineCap.ArrowAnchor })
            {
                g.DrawLine(p, 9, 7f, 13.5f, 2.5f);
                g.DrawLine(p, 7f, 9f, 2.5f, 13.5f);
            }
        }

        private static void DrawMove(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.6f) { StartCap = LineCap.ArrowAnchor, EndCap = LineCap.ArrowAnchor })
            {
                g.DrawLine(p, 8, 2.5f, 8, 13.5f);
                g.DrawLine(p, 2.5f, 8, 13.5f, 8);
            }
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 6.6f, 6.6f, 2.8f, 2.8f);
        }

        /// <summary>V 形雪佛龙箭头，rot=0 朝下、90 朝右、180 朝上、270 朝左。</summary>
        private static void DrawChevron(Graphics g, Color k, float rot)
        {
            var state = g.Save();
            g.TranslateTransform(8, 8);
            g.RotateTransform(rot);
            using (var p = new Pen(k, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLines(p, new[] { new PointF(-4.5f, -2), new PointF(0, 2.5f), new PointF(4.5f, -2) });
            g.Restore(state);
        }

        /// <summary>单箭头（支持自定义位置与长度），rot=0 朝右。</summary>
        private static void DrawArrow(Graphics g, Color c, float rot, float cx, float cy, float len)
        {
            var state = g.Save();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(rot);
            using (var p = new Pen(c, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.ArrowAnchor })
                g.DrawLine(p, -len * 0.5f, 0, len * 0.5f, 0);
            g.Restore(state);
        }

        /// <summary>居中型单箭头。</summary>
        private static void DrawArrow(Graphics g, Color c, float rot) => DrawArrow(g, c, rot, 8, 8, 11);

        private static void DrawEyeOff(Graphics g, Color k)
        {
            using (var p = new Pen(k, 1.3f))
            {
                g.DrawArc(p, 1.5f, 5f, 13f, 8f, 10, 160);
                g.DrawArc(p, 1.5f, 3f, 13f, 8f, 190, 160);
            }
            using (var p = new Pen(Red, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 2.5f, 2.5f, 13.5f, 13.5f);
        }

        private static void DrawSun(Graphics g, Color k, Color a)
        {
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 5.6f, 5.6f, 4.8f, 4.8f);
            using (var p = new Pen(k, 1.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                for (int i = 0; i < 8; i++)
                {
                    double ang = i * Math.PI / 4;
                    float x1 = 8 + (float)Math.Cos(ang) * 3.8f, y1 = 8 + (float)Math.Sin(ang) * 3.8f;
                    float x2 = 8 + (float)Math.Cos(ang) * 6.6f, y2 = 8 + (float)Math.Sin(ang) * 6.6f;
                    g.DrawLine(p, x1, y1, x2, y2);
                }
        }

        private static void DrawMoon(Graphics g, Color a)
        {
            using (var path = new GraphicsPath(FillMode.Alternate))
            {
                path.AddEllipse(2.5f, 1f, 11f, 14f);
                path.AddEllipse(5.5f, -0.5f, 11f, 14f);
                using (var br = new SolidBrush(a)) g.FillPath(br, path);
            }
        }

        private static void DrawTable(Graphics g, Color k, Color a)
        {
            using (var br = new SolidBrush(Color.FromArgb(60, a.R, a.G, a.B)))
                g.FillRectangle(br, 2, 2f, 12f, 3f);
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawRectangle(p, 2, 2f, 12f, 12f);
                g.DrawLine(p, 2, 5f, 14, 5f);
                g.DrawLine(p, 6.5f, 5f, 6.5f, 14f);
                g.DrawLine(p, 10.5f, 5f, 10.5f, 14f);
                g.DrawLine(p, 2, 9f, 14f, 9f);
            }
        }

        private static void DrawLayers(Graphics g, Color k, Color a)
        {
            var top = new[] { new PointF(8, 2), new PointF(14, 5.5f), new PointF(8, 9f), new PointF(2, 5.5f) };
            using (var br = new SolidBrush(Color.FromArgb(70, a.R, a.G, a.B))) g.FillPolygon(br, top);
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawPolygon(p, top);
                g.DrawLine(p, 2, 8.2f, 8, 11.7f);
                g.DrawLine(p, 8, 11.7f, 14f, 8.2f);
                g.DrawLine(p, 2, 10.8f, 8f, 14.3f);
                g.DrawLine(p, 8, 14.3f, 14f, 10.8f);
            }
        }

        // ============ 标记/导航 glyph ============

        private static void DrawHome(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.4f))
            {
                g.DrawLine(p, 2.5f, 8f, 8f, 2.8f);
                g.DrawLine(p, 8f, 2.8f, 13.5f, 8f);
                g.DrawRectangle(p, 4f, 7.8f, 8f, 5.7f);
            }
            using (var br = new SolidBrush(a)) g.FillRectangle(br, 7f, 10f, 2f, 3.5f);
        }

        private static void DrawTarget(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(Red, 1.4f)) g.DrawEllipse(p, 1.5f, 1.5f, 13f, 13f);
            using (var p = new Pen(k, 1.2f)) g.DrawEllipse(p, 4.2f, 4.2f, 7.6f, 7.6f);
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 6.9f, 6.9f, 2.2f, 2.2f);
        }

        private static void DrawCrosshair(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f)) g.DrawEllipse(p, 2.5f, 2.5f, 11f, 11f);
            using (var p = new Pen(a, 1.3f))
            {
                g.DrawLine(p, 8, 1f, 8f, 4.5f);
                g.DrawLine(p, 8, 11.5f, 8f, 15f);
                g.DrawLine(p, 1f, 8f, 4.5f, 8f);
                g.DrawLine(p, 11.5f, 8f, 15f, 8f);
            }
        }

        private static void DrawCompass(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f)) g.DrawEllipse(p, 1.8f, 1.8f, 12.4f, 12.4f);
            using (var br = new SolidBrush(a))
                g.FillPolygon(br, new[] { new PointF(8, 3.5f), new PointF(9.8f, 8), new PointF(8, 12.5f), new PointF(6.2f, 8) });
        }

        private static void DrawMapPin(Graphics g, Color a)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(3f, 1.5f, 10f, 10f);
                path.AddPolygon(new[] { new PointF(5.5f, 10f), new PointF(10.5f, 10f), new PointF(8f, 14.5f) });
                using (var br = new SolidBrush(a)) g.FillPath(br, path);
            }
            using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, 6.4f, 4.8f, 3.2f, 3.2f);
        }

        private static void DrawGlobe(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawEllipse(p, 1.8f, 1.8f, 12.4f, 12.4f);
                g.DrawLine(p, 8, 2f, 8f, 14f);
                g.DrawEllipse(p, 4.6f, 1.8f, 6.8f, 12.4f);
                g.DrawLine(p, 2, 8f, 14f, 8f);
            }
        }

        private static void DrawFlag(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.5f) { StartCap = LineCap.Round }) g.DrawLine(p, 3.5f, 2f, 3.5f, 14f);
            using (var br = new SolidBrush(a))
                g.FillPolygon(br, new[] { new PointF(3.5f, 2.5f), new PointF(12.5f, 2.5f), new PointF(10.5f, 5.5f), new PointF(12.5f, 8.5f), new PointF(3.5f, 8.5f) });
        }

        private static void DrawStar(Graphics g, Color a)
        {
            var pts = new PointF[10];
            for (int i = 0; i < 10; i++)
            {
                double rad = Math.PI / 5 * i - Math.PI / 2;
                float r = i % 2 == 0 ? 6.2f : 2.6f;
                pts[i] = new PointF(8 + (float)Math.Cos(rad) * r, 8 + (float)Math.Sin(rad) * r);
            }
            using (var br = new SolidBrush(a)) g.FillPolygon(br, pts);
        }

        private static void DrawHeart(Graphics g, Color c)
        {
            using (var path = new GraphicsPath())
            {
                path.AddBezier(2.5f, 5.5f, 2.5f, 2.5f, 6.5f, 2f, 8f, 5f);
                path.AddBezier(9.5f, 2f, 13.5f, 2.5f, 13.5f, 5.5f, 8f, 13.5f);
                path.CloseFigure();
                using (var br = new SolidBrush(c)) g.FillPath(br, path);
            }
        }

        private static void DrawPin(Graphics g, Color k, Color a)
        {
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 4.5f, 2f, 7f, 7f);
            using (var p = new Pen(k, 1.2f)) g.DrawEllipse(p, 4.5f, 2f, 7f, 7f);
            using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round })
            {
                g.DrawLine(p, 6.2f, 8.5f, 4f, 14f);
                g.DrawLine(p, 9.8f, 8.5f, 12f, 14f);
            }
        }

        private static void DrawBell(Graphics g, Color k, Color a)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(3.5f, 3f, 9f, 9f, 180, 180);
                path.AddLine(3.5f, 8f, 2.2f, 12.5f);
                path.AddLine(2.2f, 12.5f, 13.8f, 12.5f);
                path.AddLine(12.5f, 8f, 12.5f, 8f);
                path.CloseFigure();
                using (var br = new SolidBrush(Color.FromArgb(70, a.R, a.G, a.B))) g.FillPath(br, path);
                using (var p = new Pen(k, 1.3f)) g.DrawPath(p, path);
            }
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 6.8f, 12.8f, 2.4f, 2.4f);
        }

        private static void DrawTag(Graphics g, Color k, Color a)
        {
            var pts = new[] { new PointF(2, 9f), new PointF(9f, 2f), new PointF(14f, 2f), new PointF(14f, 7f), new PointF(7f, 14f) };
            using (var br = new SolidBrush(Color.FromArgb(60, a.R, a.G, a.B))) g.FillPolygon(br, pts);
            using (var p = new Pen(k, 1.3f)) g.DrawPolygon(p, pts);
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 11.3f, 4.3f, 1.8f, 1.8f);
        }

        // ============ 用户/信息 glyph ============

        /// <summary>单人/双人（back=true 画后景偏位小人）。</summary>
        private static void DrawUser(Graphics g, Color c, Color accent, bool back)
        {
            float dx = back ? -2.2f : 0;
            using (var br = new SolidBrush(c))
            {
                g.FillEllipse(br, 5.5f + dx, 2.8f, 5f, 5f);
                using (var path = new GraphicsPath())
                {
                    path.AddArc(3f + dx, 8.5f, 10f, 8f, 180, 180);
                    path.AddLine(13f + dx, 14.5f, 3f + dx, 14.5f);
                    path.CloseFigure();
                    g.FillPath(br, path);
                }
            }
        }

        private static void DrawMail(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 1.5f, 4f, 13f, 8.5f);
            using (var p = new Pen(a, 1.3f))
            {
                g.DrawLine(p, 1.5f, 4.5f, 8f, 9.5f);
                g.DrawLine(p, 14.5f, 4.5f, 8f, 9.5f);
            }
        }

        private static void DrawClock(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.4f)) g.DrawEllipse(p, 1.8f, 1.8f, 12.4f, 12.4f);
            using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(p, 8, 8f, 8f, 4f);
                g.DrawLine(p, 8, 8f, 11f, 9.5f);
            }
        }

        private static void DrawCalendar(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.FillRectangle(Brushes.White, 2.2f, 3.5f, 11.6f, 10.5f);
                g.DrawRectangle(p, 2.2f, 3.5f, 11.6f, 10.5f);
            }
            using (var p = new Pen(a, 1.5f) { StartCap = LineCap.Round })
            {
                g.DrawLine(p, 4.8f, 2f, 4.8f, 5.5f);
                g.DrawLine(p, 11.2f, 2f, 11.2f, 5.5f);
            }
            using (var br = new SolidBrush(a))
            {
                g.FillRectangle(br, 4.5f, 7f, 2f, 1.8f);
                g.FillRectangle(br, 8f, 7f, 2f, 1.8f);
                g.FillRectangle(br, 11.5f, 7f, 1.5f, 1.8f);
                g.FillRectangle(br, 4.5f, 10f, 2f, 1.8f);
                g.FillRectangle(br, 8f, 10f, 2f, 1.8f);
            }
        }

        private static void DrawInfo(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.5f)) g.DrawEllipse(p, 2, 2, 12, 12);
            using (var br = new SolidBrush(a))
            {
                g.FillEllipse(br, 7.2f, 3.8f, 1.6f, 1.6f);
                g.FillRectangle(br, 7.2f, 6.5f, 1.6f, 6f);
            }
        }

        private static void DrawWarning(Graphics g)
        {
            using (var br = new SolidBrush(Color.FromArgb(80, 235, 170, 40)))
                g.FillPolygon(br, new[] { new PointF(8, 2f), new PointF(14.5f, 13.5f), new PointF(1.5f, 13.5f) });
            using (var p = new Pen(Orange, 1.4f))
                g.DrawPolygon(p, new[] { new PointF(8, 2f), new PointF(14.5f, 13.5f), new PointF(1.5f, 13.5f) });
            using (var br = new SolidBrush(Color.FromArgb(120, 60, 30)))
            {
                g.FillRectangle(br, 7.3f, 6f, 1.4f, 4f);
                g.FillEllipse(br, 7.3f, 10.8f, 1.4f, 1.4f);
            }
        }

        private static void DrawQuestion(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.5f))
            {
                g.DrawEllipse(p, 2, 2, 12, 12);
                g.DrawArc(p, 5.6f, 5f, 4.8f, 4f, 200, 210);
            }
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 7.2f, 10.8f, 1.6f, 1.6f);
        }

        // ============ 设备 glyph ============

        private static void DrawMonitor(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f))
            {
                g.DrawRectangle(p, 1.5f, 2.5f, 13f, 9f);
                g.DrawLine(p, 6.5f, 11.5f, 6.5f, 13.5f);
                g.DrawLine(p, 9.5f, 11.5f, 9.5f, 13.5f);
                g.DrawLine(p, 4.5f, 13.5f, 11.5f, 13.5f);
            }
            using (var p = new Pen(a, 1.3f)) g.DrawLine(p, 4, 9.5f, 8f, 5.5f);
        }

        private static void DrawPhone(Graphics g, Color k, Color a)
        {
            FillStroke(g, new RectangleF(4.5f, 1.2f, 7f, 13.6f), 1.6f, Brushes.White, new Pen(k, 1.3f));
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 7.3f, 12.6f, 1.4f, 1.4f);
            using (var p = new Pen(k, 1.2f)) g.DrawLine(p, 6.5f, 2.6f, 9.5f, 2.6f);
        }

        private static void DrawCpu(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f))
            {
                for (int i = 0; i < 4; i++)
                {
                    float o = 5.2f + i * 1.9f;
                    g.DrawLine(p, o, 2f, o, 4f);
                    g.DrawLine(p, o, 12f, o, 14f);
                    g.DrawLine(p, 2f, o, 4f, o);
                    g.DrawLine(p, 12f, o, 14f, o);
                }
                g.DrawRectangle(p, 4f, 4f, 8f, 8f);
            }
            using (var br = new SolidBrush(Color.FromArgb(80, a.R, a.G, a.B))) g.FillRectangle(br, 6f, 6f, 4f, 4f);
        }

        private static void DrawDatabase(Graphics g, Color k, Color a)
        {
            using (var br = new SolidBrush(Color.FromArgb(70, a.R, a.G, a.B)))
                g.FillEllipse(br, 3f, 3f, 10f, 3.2f);
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawLine(p, 3f, 4.6f, 3f, 12f);
                g.DrawLine(p, 13f, 4.6f, 13f, 12f);
                g.DrawArc(p, 3f, 6.4f, 10f, 3.2f, 0, 180);
                g.DrawArc(p, 3f, 8.8f, 10f, 3.2f, 0, 180);
                g.DrawEllipse(p, 3f, 10.4f, 10f, 3.2f);
                g.DrawEllipse(p, 3f, 3f, 10f, 3.2f);
            }
        }

        private static void DrawHardDrive(Graphics g, Color k, Color a)
        {
            FillStroke(g, new RectangleF(1.5f, 4f, 13f, 8f), 1.2f, Brushes.White, new Pen(k, 1.3f));
            using (var p = new Pen(k, 1.1f)) g.DrawEllipse(p, 3f, 6f, 4f, 4f);
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 11.5f, 9f, 1.5f, 1.5f);
        }

        private static void DrawWifi(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(a, 1.4f))
            {
                g.DrawArc(p, 1.5f, 4f, 13f, 11f, 200, 140);
                g.DrawArc(p, 3.8f, 6.5f, 8.4f, 8.5f, 205, 130);
                g.DrawArc(p, 6f, 8.8f, 4f, 5f, 210, 120);
            }
            using (var br = new SolidBrush(k)) g.FillEllipse(br, 7.2f, 12f, 1.6f, 1.6f);
        }

        private static void DrawCamera(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.FillRectangle(Brushes.White, 1.5f, 5.5f, 13f, 8.5f);
                g.DrawRectangle(p, 1.5f, 5.5f, 13f, 8.5f);
                g.FillRectangle(Brushes.White, 4.5f, 3f, 5f, 2.8f);
                g.DrawRectangle(p, 4.5f, 3f, 5f, 2.8f);
            }
            using (var p = new Pen(a, 1.3f)) g.DrawEllipse(p, 5.5f, 7.3f, 5f, 5f);
        }

        private static void DrawImage(Graphics g, Color k)
        {
            using (var p = new Pen(k, 1.2f))
            {
                g.FillRectangle(Brushes.White, 1.5f, 3f, 13f, 10.5f);
                g.DrawRectangle(p, 1.5f, 3f, 13f, 10.5f);
            }
            using (var br = new SolidBrush(C5)) g.FillEllipse(br, 9.5f, 5f, 2.2f, 2.2f);
            using (var br = new SolidBrush(C3))
                g.FillPolygon(br, new[] { new PointF(2, 13.5f), new PointF(6.5f, 8.5f), new PointF(9f, 11f), new PointF(11.5f, 8.8f), new PointF(14.5f, 12.5f), new PointF(14.5f, 13.5f) });
        }

        // ============ 绘图/开发 glyph ============

        private static void DrawBrush(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(a, 2.2f) { StartCap = LineCap.Round })
                g.DrawLine(p, 11.8f, 4.2f, 4.5f, 11.5f);
            using (var br = new SolidBrush(k))
                g.FillPolygon(br, new[] { new PointF(3, 13), new PointF(4.2f, 10.6f), new PointF(6f, 12.4f), new PointF(4.5f, 13.5f) });
        }

        private static void DrawPenTool(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1f) { DashStyle = DashStyle.Dot })
            {
                g.DrawLine(p, 2f, 14f, 6f, 10f);
                g.DrawLine(p, 14f, 2f, 10f, 6f);
            }
            using (var p = new Pen(a, 1.5f)) g.DrawBezier(p, 2, 14, 6, 4, 10, 12, 14, 2);
            using (var br = new SolidBrush(a))
            {
                g.FillEllipse(br, 5.2f, 9.2f, 1.6f, 1.6f);
                g.FillEllipse(br, 9.2f, 5.2f, 1.6f, 1.6f);
            }
        }

        private static void DrawEraser(Graphics g, Color k)
        {
            var state = g.Save();
            g.TranslateTransform(8, 8);
            g.RotateTransform(-35f);
            using (var br = new SolidBrush(Color.FromArgb(235, 160, 175)))
                g.FillRectangle(br, -3.5f, -5.5f, 7f, 11f);
            using (var p = new Pen(k, 1.2f))
            {
                g.DrawRectangle(p, -3.5f, -5.5f, 7f, 11f);
                g.DrawLine(p, -3.5f, 1.5f, 3.5f, 1.5f);
            }
            g.Restore(state);
        }

        private static void DrawDropper(Graphics g, Color k, Color a)
        {
            var state = g.Save();
            g.TranslateTransform(8, 8);
            g.RotateTransform(45f);
            using (var p = new Pen(k, 1.4f))
            {
                g.DrawLine(p, 0, -6f, 0, 3f);
                g.DrawLine(p, -2f, -6f, 2f, -6f);
            }
            using (var br = new SolidBrush(a)) g.FillRectangle(br, -1.4f, 3f, 2.8f, 3.2f);
            g.Restore(state);
        }

        private static void DrawCrop(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.3f))
            {
                g.DrawLine(p, 3f, 1.5f, 3f, 13f);
                g.DrawLine(p, 3f, 13f, 14.5f, 13f);
            }
            using (var p = new Pen(a, 1.3f))
            {
                g.DrawLine(p, 1.5f, 3f, 13f, 3f);
                g.DrawLine(p, 13f, 3f, 13f, 14.5f);
            }
        }

        private static void DrawCode(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLines(p, new[] { new PointF(5, 3.5f), new PointF(1.5f, 8), new PointF(5, 12.5f) });
                g.DrawLines(p, new[] { new PointF(11, 3.5f), new PointF(14.5f, 8), new PointF(11, 12.5f) });
                g.DrawLine(p, 9f, 2.5f, 7f, 13.5f);
            }
        }

        private static void DrawTerminal(Graphics g, Color k, Color a)
        {
            FillStroke(g, new RectangleF(1.5f, 2.5f, 13f, 11f), 1f, new SolidBrush(Color.FromArgb(40, 44, 52)), new Pen(k, 1.2f));
            using (var p = new Pen(a, 1.4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLines(p, new[] { new PointF(3.5f, 5.5f), new PointF(5.5f, 7.3f), new PointF(3.5f, 9.2f) });
            using (var p = new Pen(Color.FromArgb(210, 215, 225), 1.2f) { StartCap = LineCap.Round })
                g.DrawLine(p, 7f, 10.2f, 11.5f, 10.2f);
        }

        private static void DrawGitBranch(Graphics g, Color a)
        {
            using (var p = new Pen(a, 1.4f))
            {
                g.DrawLine(p, 5f, 3.5f, 5f, 12.5f);
                g.DrawLine(p, 5f, 5.5f, 11f, 5.5f);
                g.DrawLine(p, 11f, 5.5f, 11f, 9f);
                g.DrawLine(p, 5f, 9.5f, 11f, 9f);
            }
            using (var br = new SolidBrush(Color.White))
            using (var p = new Pen(a, 1.3f))
            {
                g.DrawEllipse(p, 3.2f, 2.2f, 3.6f, 3.6f); g.FillEllipse(br, 3.2f, 2.2f, 3.6f, 3.6f);
                g.DrawEllipse(p, 3.2f, 10.7f, 3.6f, 3.6f); g.FillEllipse(br, 3.2f, 10.7f, 3.6f, 3.6f);
                g.DrawEllipse(p, 9.2f, 3.7f, 3.6f, 3.6f); g.FillEllipse(br, 9.2f, 3.7f, 3.6f, 3.6f);
            }
        }

        private static void DrawDashboard(Graphics g, Color k, Color a)
        {
            using (var p = new Pen(k, 1.4f)) g.DrawArc(p, 2f, 3f, 12f, 11f, 180, 180);
            using (var p = new Pen(k, 1.1f))
            {
                g.DrawLine(p, 8f, 8.5f, 8f, 4.5f);
                g.DrawLine(p, 11.5f, 9.2f, 14f, 7f);
                g.DrawLine(p, 4.5f, 9.2f, 2f, 7f);
            }
            using (var p = new Pen(a, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, 8f, 8.5f, 11f, 6f);
            using (var br = new SolidBrush(a)) g.FillEllipse(br, 7.2f, 7.7f, 1.6f, 1.6f);
        }

        private static void DrawLock(Graphics g, Color k, Color a, bool opened)
        {
            using (var p = new Pen(a, 1.6f))
            {
                if (opened) g.DrawArc(p, 5.2f, 2f, 5.6f, 6f, 90, 200);
                else g.DrawArc(p, 4.5f, 2f, 7f, 7f, 180, 180);
            }
            using (var br = new SolidBrush(Color.FromArgb(60, a.R, a.G, a.B)))
                g.FillRectangle(br, 3f, 7.5f, 10f, 6.5f);
            using (var p = new Pen(k, 1.3f)) g.DrawRectangle(p, 3f, 7.5f, 10f, 6.5f);
            using (var brk = new SolidBrush(k)) g.FillEllipse(brk, 7.2f, 9.8f, 1.6f, 1.6f);
            using (var pk = new Pen(k, 1.2f)) g.DrawLine(pk, 8f, 10.6f, 8f, 12.3f);
        }
    }
}
