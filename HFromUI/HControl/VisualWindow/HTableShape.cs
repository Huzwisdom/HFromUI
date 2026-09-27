using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 视觉 ROI 图形基类（画图工具基类）。
    /// 风格参考 HBase/HDrawBase 与 HCoordinate 下的 HDrawPoint/HDrawLine：
    /// Draw(Graphics, 坐标变换委托, 是否选中, 缩放比例)、HitTest、Move、Clone。
    /// 区别：本系列图形全部工作在 <b>图像像素坐标系</b>（原点在图像左上角，X 向右、Y 向下），
    /// 便于后期找特征处理（裁剪 ROI、模板匹配、斑点检测等直接使用像素坐标）。
    /// 绘制时由 HVisualWindowA 统一传入“图像像素坐标 → 屏幕坐标”变换，
    /// 命中测试与绘制使用同一套坐标变换，避免拾取/显示错位。
    /// </summary>
    [Serializable]
    public abstract class HTableShape
    {
        /// <summary>全局自增序号（仅用于默认命名）</summary>
        private static int _globalId = 0;

        protected HTableShape()
        {
            Id = System.Threading.Interlocked.Increment(ref _globalId);
            Color = HTableShapeList.DefaultColor;
            LineWidth = HTableShapeList.DefaultLineWidth;
            Name = TypeName + Id;
        }

        /// <summary>图形唯一编号（从 1 开始）</summary>
        public int Id { get; internal set; }

        /// <summary>图形名称（默认“类型名+编号”，可修改）</summary>
        public string Name { get; set; }

        /// <summary>线条颜色</summary>
        public Color Color { get; set; }

        /// <summary>线宽（屏幕像素，固定屏幕宽度，任意缩放下观感一致）</summary>
        public float LineWidth { get; set; }

        /// <summary>图形类型</summary>
        public abstract HTableTool ShapeType { get; }

        /// <summary>类型中文名（用于命名与提示）</summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// 完成绘制所需的锚点数（鼠标单击次数）。
        /// int.MaxValue 表示不固定（折线/多边形，双击完成）。
        /// </summary>
        public abstract int RequiredPoints { get; }

        /// <summary>锚点列表（图像像素坐标）。绘制过程中已确定的点。</summary>
        public List<HPoint> Points { get; } = new List<HPoint>();

        /// <summary>是否正在绘制中（false=已完成的图形）</summary>
        public bool IsBuilding { get; set; } = false;

        /// <summary>绘制中跟随鼠标的预览点（图像像素坐标，不参与完成后的几何）</summary>
        [NonSerialized]
        private HPoint _previewPoint;

        /// <summary>设置/更新预览点（绘制过程中鼠标移动时调用）</summary>
        public void SetPreview(HPoint imagePoint)
        {
            _previewPoint = imagePoint?.Clone();
        }

        /// <summary>清除预览点</summary>
        public void ClearPreview()
        {
            _previewPoint = null;
        }

        /// <summary>
        /// 实际参与绘制的点序列：已确定锚点 + 末尾预览点（绘制中）。
        /// </summary>
        protected IEnumerable<HPoint> EnumerateDrawPoints()
        {
            for (int i = 0; i < Points.Count; i++)
                yield return Points[i];
            if (IsBuilding && _previewPoint != null)
                yield return _previewPoint;
        }

        /// <summary>
        /// 提交一个锚点（鼠标单击/释放时调用）。返回提交后锚点总数。
        /// </summary>
        public int AddAnchor(HPoint imagePoint)
        {
            Points.Add(imagePoint.Clone());
            // 通知派生几何更新缓存（如扇形角度、圆弧圆心），保证绘制中预览与完成状态一致
            OnGeometryChanged();
            return Points.Count;
        }

        /// <summary>修改指定锚点（选择工具拖拽锚点时调用）。子类可 override 以更新派生几何参数（如扇形的角度）</summary>
        public virtual void SetAnchor(int index, HPoint imagePoint)
        {
            if (index >= 0 && index < Points.Count && imagePoint != null)
                Points[index] = imagePoint.Clone();
            OnGeometryChanged();
        }

        /// <summary>整体平移（图像像素增量）</summary>
        public virtual void MoveBy(HPoint deltaImage)
        {
            if (deltaImage == null) return;
            for (int i = 0; i < Points.Count; i++)
                Points[i] = Points[i] + deltaImage;
            OnGeometryChanged();
        }

        /// <summary>
        /// 从备份图形恢复几何（撤销整体移动/锚点拖拽用）：
        /// 把锚点、颜色、线宽复制回本实例，并触发派生几何缓存重建（如圆弧的圆心/半径）。
        /// </summary>
        public void CopyGeometryFrom(HTableShape src)
        {
            if (src == null) return;
            Points.Clear();
            foreach (HPoint p in src.Points)
                Points.Add(p.Clone());
            Color = src.Color;
            LineWidth = src.LineWidth;
            OnGeometryChanged();
        }

        /// <summary>图形是否有效（面积/长度不为 0、点数足够），完成绘制时校验</summary>
        public virtual bool IsValid()
        {
            return Points.Count > 0;
        }

        /// <summary>几何变化回调（子类可重写以清理缓存）</summary>
        protected virtual void OnGeometryChanged() { }

        // ------------------------------------------------------------------
        // 绘制
        // ------------------------------------------------------------------

        /// <summary>
        /// 在控件上绘制图形。
        /// </summary>
        /// <param name="g">画布</param>
        /// <param name="imageToScreen">图像像素坐标 → 屏幕坐标 变换（统一入口，绘制与拾取共用）</param>
        /// <param name="selected">是否选中</param>
        /// <param name="scale">屏幕像素/图像像素（当前视图缩放，用于把图像公差换算为屏幕像素）</param>
        public abstract void Draw(Graphics g, Func<HPoint, PointF> imageToScreen, bool selected, float scale);

        /// <summary>命中测试：图像像素点是否落在图形上（toleranceImagePx 为图像像素公差）</summary>
        public abstract bool HitTest(HPoint imagePoint, double toleranceImagePx);

        /// <summary>锚点命中测试：返回被点中的锚点序号，未命中返回 -1</summary>
        public virtual int HitTestAnchor(HPoint imagePoint, double toleranceImagePx)
        {
            for (int i = Points.Count - 1; i >= 0; i--)
            {
                if (Points[i].DistanceTo(imagePoint) <= toleranceImagePx)
                    return i;
            }
            return -1;
        }

        /// <summary>外接矩形（图像像素坐标），默认取所有锚点的包围盒</summary>
        public virtual RectangleF GetBoundingRect()
        {
            if (Points.Count == 0) return RectangleF.Empty;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (HPoint p in Points)
            {
                minX = Math.Min(minX, (float)p.X.Value); minY = Math.Min(minY, (float)p.Y.Value);
                maxX = Math.Max(maxX, (float)p.X.Value); maxY = Math.Max(maxY, (float)p.Y.Value);
            }
            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }

        /// <summary>深拷贝</summary>
        public abstract HTableShape CloneShape();

        /// <summary>拷贝基类属性</summary>
        protected void CopyBaseTo(HTableShape target)
        {
            target.Name = Name;
            target.Color = Color;
            target.LineWidth = LineWidth;
            target.IsBuilding = false;
            target.Points.Clear();
            foreach (HPoint p in Points)
                target.Points.Add(p.Clone());
        }

        // ------------------------------------------------------------------
        // 绘制辅助（屏幕坐标系，固定像素尺寸，任意缩放观感一致）
        // ------------------------------------------------------------------

        /// <summary>锚点方块半边长（屏幕像素）</summary>
        public const float HandleHalfSize = 4.5f;


        /// <summary>画一个锚点小方块</summary>
        protected static void DrawHandle(Graphics g, PointF screenPoint, bool filled, Color color)
        {
            RectangleF r = new RectangleF(screenPoint.X - HandleHalfSize, screenPoint.Y - HandleHalfSize,
                HandleHalfSize * 2, HandleHalfSize * 2);
            using (Pen pen = new Pen(color, 1.5f))
            {
                if (filled)
                {
                    using (Brush b = new SolidBrush(color))
                        g.FillRectangle(b, r);
                }
                else
                {
                    g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                }
            }
        }

        /// <summary>画十字标记（点工具）</summary>
        protected static void DrawCross(Graphics g, PointF c, float size, Pen pen)
        {
            g.DrawLine(pen, c.X - size, c.Y, c.X + size, c.Y);
            g.DrawLine(pen, c.X, c.Y - size, c.X, c.Y + size);
        }

        /// <summary>画图形名称标签</summary>
        protected void DrawLabel(Graphics g, PointF screenPoint, string text, Color color, Font font)
        {
            if (string.IsNullOrEmpty(text) || font == null) return;
            SizeF sz = g.MeasureString(text, font);
            float x = screenPoint.X + 6f;
            float y = screenPoint.Y - sz.Height - 4f;
            if (y < 0) y = screenPoint.Y + 6f;
            using (Brush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(bg, x - 2, y - 1, sz.Width + 4, sz.Height + 2);
            using (Brush b = new SolidBrush(color))
                g.DrawString(text, font, b, x, y);
        }

        /// <summary>创建图形所用画笔（选中时加粗、用选中色）</summary>
        protected Pen CreatePen(bool selected, float scale)
        {
            Color c = selected ? HTableShapeList.SelectedColor : (IsBuilding ? HTableShapeList.BuildingColor : Color);
            float w = selected ? LineWidth + 1.5f : LineWidth;
            Pen pen = new Pen(c, w);
            if (IsBuilding) pen.DashStyle = DashStyle.Dash;
            return pen;
        }
    }

    /// <summary>
    /// ROI 图形几何计算工具（图像像素坐标系）。
    /// </summary>
    public static class HTableGeom
    {
        /// <summary>两点距离</summary>
        public static double Distance(HPoint a, HPoint b)
        {
            double dx = (double)a.X.Value - (double)b.X.Value;
            double dy = (double)a.Y.Value - (double)b.Y.Value;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>点到线段的最短距离</summary>
        public static double DistancePointToSegment(HPoint p, HPoint a, HPoint b)
        {
            double ax = (double)a.X.Value, ay = (double)a.Y.Value;
            double bx = (double)b.X.Value, by = (double)b.Y.Value;
            double px = (double)p.X.Value, py = (double)p.Y.Value;
            double dx = bx - ax, dy = by - ay;
            double lenSq = dx * dx + dy * dy;
            if (lenSq < 1e-12)
                return Math.Sqrt((px - ax) * (px - ax) + (py - ay) * (py - ay));
            double t = ((px - ax) * dx + (py - ay) * dy) / lenSq;
            t = Math.Max(0, Math.Min(1, t));
            double cx = ax + t * dx, cy = ay + t * dy;
            return Math.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
        }

        /// <summary>
        /// 三点求圆：返回圆心 center 与半径 radius；三点共线时返回 false。
        /// </summary>
        public static bool CircleFrom3Points(HPoint p0, HPoint p1, HPoint p2, out HPoint center, out double radius)
        {
            center = null;
            radius = 0;
            double x1 = (double)p0.X.Value, y1 = (double)p0.Y.Value;
            double x2 = (double)p1.X.Value, y2 = (double)p1.Y.Value;
            double x3 = (double)p2.X.Value, y3 = (double)p2.Y.Value;

            double d = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
            if (Math.Abs(d) < 1e-9)
                return false;

            double ux = ((x1 * x1 + y1 * y1) * (y2 - y3)
                       + (x2 * x2 + y2 * y2) * (y3 - y1)
                       + (x3 * x3 + y3 * y3) * (y1 - y2)) / d;
            double uy = ((x1 * x1 + y1 * y1) * (x3 - x2)
                       + (x2 * x2 + y2 * y2) * (x1 - x3)
                       + (x3 * x3 + y3 * y3) * (x2 - x1)) / d;

            center = new HPoint(ux, uy);
            radius = Math.Sqrt((ux - x1) * (ux - x1) + (uy - y1) * (uy - y1));
            return radius > 1e-6;
        }

        /// <summary>射线法判断点是否在多边形内（图像像素坐标）</summary>
        public static bool PointInPolygon(HPoint p, IList<HPoint> polygon)
        {
            if (polygon == null || polygon.Count < 3) return false;
            double x = (double)p.X.Value, y = (double)p.Y.Value;
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                double xi = (double)polygon[i].X.Value, yi = (double)polygon[i].Y.Value;
                double xj = (double)polygon[j].X.Value, yj = (double)polygon[j].Y.Value;
                bool intersect = ((yi > y) != (yj > y))
                    && (x < (xj - xi) * (y - yi) / ((yj - yi) == 0 ? 1e-12 : (yj - yi)) + xi);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        /// <summary>点到折线/多边形边界的最短距离（closed=true 时闭合）</summary>
        public static double DistancePointToPolyline(HPoint p, IList<HPoint> pts, bool closed)
        {
            if (pts == null || pts.Count == 0) return double.MaxValue;
            if (pts.Count == 1) return Distance(p, pts[0]);
            double min = double.MaxValue;
            int n = closed ? pts.Count : pts.Count - 1;
            for (int i = 0; i < n; i++)
            {
                HPoint a = pts[i];
                HPoint b = pts[(i + 1) % pts.Count];
                min = Math.Min(min, DistancePointToSegment(p, a, b));
            }
            return min;
        }

        /// <summary>角度归一化到 [0, 360) 度</summary>
        public static double NormalizeAngle360(double deg)
        {
            deg = deg % 360.0;
            if (deg < 0) deg += 360.0;
            return deg;
        }

        /// <summary>角度归一化到 (-180, 180] 度</summary>
        public static double NormalizeAngle180(double deg)
        {
            deg = NormalizeAngle360(deg);
            if (deg > 180) deg -= 360.0;
            return deg;
        }

        /// <summary>度→弧度</summary>
        public static double Deg2Rad(double deg) => deg * Math.PI / 180.0;

        /// <summary>弧度→度</summary>
        public static double Rad2Deg(double rad) => rad * 180.0 / Math.PI;

        /// <summary>
        /// 多边形面积（鞋带公式，正数=逆时针/负数=顺时针/0=共线或自交）。
        /// 输入是闭合多边形，图像像素坐标系。
        /// </summary>
        public static double PolygonArea(IList<HPoint> polygon)
        {
            if (polygon == null || polygon.Count < 3) return 0;
            double s = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                HPoint a = polygon[i];
                HPoint b = polygon[(i + 1) % polygon.Count];
                s += (double)a.X.Value * (double)b.Y.Value - (double)b.X.Value * (double)a.Y.Value;
            }
            return Math.Abs(s) / 2.0;
        }

        /// <summary>多边形质心（图像像素坐标）</summary>
        public static HPoint PolygonCentroid(IList<HPoint> polygon)
        {
            if (polygon == null || polygon.Count < 3) return null;
            double A2 = 0, cx = 0, cy = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                HPoint a = polygon[i];
                HPoint b = polygon[(i + 1) % polygon.Count];
                double cross = (double)a.X.Value * (double)b.Y.Value - (double)b.X.Value * (double)a.Y.Value;
                A2 += cross;
                cx += ((double)a.X.Value + (double)b.X.Value) * cross;
                cy += ((double)a.Y.Value + (double)b.Y.Value) * cross;
            }
            if (Math.Abs(A2) < 1e-12) return null;
            return new HPoint(cx / (3.0 * A2), cy / (3.0 * A2));
        }

        /// <summary>多边形包围盒（轴对齐）</summary>
        public static RectangleF PolygonBoundingRect(IList<HPoint> polygon)
        {
            if (polygon == null || polygon.Count == 0) return RectangleF.Empty;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (HPoint p in polygon)
            {
                minX = Math.Min(minX, (float)p.X.Value); minY = Math.Min(minY, (float)p.Y.Value);
                maxX = Math.Max(maxX, (float)p.X.Value); maxY = Math.Max(maxY, (float)p.Y.Value);
            }
            return RectangleF.FromLTRB(minX, minY, maxX, maxY);
        }

        /// <summary>扇形面积（圆心在原点、扫掠角 sweepDeg，半径 r）</summary>
        public static double SectorArea(double r, double sweepDeg)
        {
            return 0.5 * r * r * Deg2Rad(Math.Abs(sweepDeg));
        }

        /// <summary>圆环面积（外半径 - 内半径）</summary>
        public static double AnnulusArea(double rOuter, double rInner)
        {
            if (rOuter <= rInner) return 0;
            return Math.PI * (rOuter * rOuter - rInner * rInner);
        }

        /// <summary>圆环扇形面积</summary>
        public static double AnnulusSectorArea(double rOuter, double rInner, double sweepDeg)
        {
            if (rOuter <= rInner) return 0;
            double a = 0.5 * Deg2Rad(Math.Abs(sweepDeg));
            return a * (rOuter * rOuter - rInner * rInner);
        }

        /// <summary>点是否在圆环内（含边界）</summary>
        public static bool PointInAnnulus(HPoint p, HPoint center, double rOuter, double rInner)
        {
            double d = Distance(p, center);
            return d >= rInner && d <= rOuter;
        }

        /// <summary>
        /// 点是否在扇形内（图像像素坐标系，起始角 startDeg、扫掠角 sweepDeg）。
        /// startDeg 和 sweepDeg 在图像坐标系中：0° 指向 +X，顺时针为正。
        /// </summary>
        public static bool PointInSector(HPoint p, HPoint center, double r, double startDeg, double sweepDeg)
        {
            double d = Distance(p, center);
            if (d > r) return false;
            double ang = Rad2Deg(Math.Atan2((double)(p.Y.Value - center.Y.Value), (double)(p.X.Value - center.X.Value)));
            ang = NormalizeAngle360(ang);
            double s = NormalizeAngle360(startDeg);
            double e = NormalizeAngle360(s + sweepDeg);
            if (sweepDeg >= 0)
                return s <= e ? (ang >= s && ang <= e) : (ang >= s || ang <= e);
            else
                return s >= e ? (ang >= e && ang <= s) : (ang >= e || ang <= s);
        }

        /// <summary>点是否在椭圆内（轴对齐，圆心 c、半长轴 a、半短轴 b）</summary>
        public static bool PointInEllipse(HPoint p, HPoint c, double a, double b)
        {
            if (a <= 0 || b <= 0) return false;
            double dx = (double)(p.X.Value - c.X.Value) / a;
            double dy = (double)(p.Y.Value - c.Y.Value) / b;
            return dx * dx + dy * dy <= 1.0;
        }

        /// <summary>点到圆的边界距离（返回 0 表示在圆上，负数在圆内，正数在圆外）</summary>
        public static double SignedDistanceToCircle(HPoint p, HPoint center, double radius)
        {
            return Distance(p, center) - radius;
        }

        /// <summary>根据圆心、半径和角度计算圆上一点（图像像素坐标系，0°=+X、顺时针为正）</summary>
        public static HPoint PointOnCircle(HPoint center, double radius, double angleDeg)
        {
            double rad = Deg2Rad(angleDeg);
            return new HPoint(
                center.X.Value + radius * Math.Cos(rad),
                center.Y.Value + radius * Math.Sin(rad));
        }

        /// <summary>旋转矩形四个角点（图像像素坐标系）。center=中心、width=长、height=宽、angleDeg=长轴相对 +X 轴的顺时针角度</summary>
        public static HPoint[] RotatedRectCorners(HPoint center, double width, double height, double angleDeg)
        {
            double hw = width / 2.0, hh = height / 2.0;
            HPoint[] local = new[]
            {
                new HPoint(-hw, -hh), new HPoint( hw, -hh),
                new HPoint( hw,  hh), new HPoint(-hw,  hh)
            };
            double rad = Deg2Rad(angleDeg);
            double cos = Math.Cos(rad), sin = Math.Sin(rad);
            HPoint[] result = new HPoint[4];
            for (int i = 0; i < 4; i++)
            {
                result[i] = new HPoint(
                    center.X.Value + local[i].X.Value * cos - local[i].Y.Value * sin,
                    center.Y.Value + local[i].X.Value * sin + local[i].Y.Value * cos);
            }
            return result;
        }

        /// <summary>归一化旋转矩形角度到 (-90, 90] 度，保持 width >= height 的语义</summary>
        public static double NormalizeRotatedRectAngle(double angleDeg)
        {
            double a = angleDeg % 180.0;
            if (a > 90) a -= 180.0;
            if (a <= -90) a += 180.0;
            return a;
        }

        /// <summary>向量点积</summary>
        public static double Dot(HPoint a, HPoint b)
        {
            return (double)a.X.Value * (double)b.X.Value + (double)a.Y.Value * (double)b.Y.Value;
        }

        /// <summary>二维叉积（返回值符号=旋转方向）</summary>
        public static double Cross(HPoint a, HPoint b)
        {
            return (double)a.X.Value * (double)b.Y.Value - (double)a.Y.Value * (double)b.X.Value;
        }

        /// <summary>将折线重采样为等间距（dx 像素）</summary>
        public static List<HPoint> Resample(IList<HPoint> pts, double dx)
        {
            var outPts = new List<HPoint>();
            if (pts == null || pts.Count < 2 || dx <= 0) { if (pts != null) outPts.AddRange(pts); return outPts; }
            HPoint prev = pts[0];
            outPts.Add(prev.Clone());
            double segAccum = 0;
            for (int i = 1; i < pts.Count; i++)
            {
                HPoint cur = pts[i];
                double segLen = Distance(prev, cur);
                if (segLen < 1e-12) continue;
                while (segAccum + segLen >= dx)
                {
                    double remain = dx - segAccum;
                    double t = remain / segLen;
                    HPoint np = new HPoint(
                        prev.X.Value + (cur.X.Value - prev.X.Value) * t,
                        prev.Y.Value + (cur.Y.Value - prev.Y.Value) * t);
                    outPts.Add(np);
                    prev = np;
                    segLen = Distance(prev, cur);
                    segAccum = 0;
                }
                segAccum += segLen;
                prev = cur;
            }
            outPts.Add(pts[pts.Count - 1].Clone());
            return outPts;
        }

        /// <summary>简化折线（Ramer-Douglas-Peucker 距离阈值法）</summary>
        public static List<HPoint> Simplify(IList<HPoint> pts, double epsilon)
        {
            var result = new List<HPoint>();
            if (pts == null || pts.Count < 3) { if (pts != null) result.AddRange(pts); return result; }
            double maxDist = -1;
            int idx = 0;
            for (int i = 1; i < pts.Count - 1; i++)
            {
                double d = DistancePointToSegment(pts[i], pts[0], pts[pts.Count - 1]);
                if (d > maxDist) { maxDist = d; idx = i; }
            }
            if (maxDist > epsilon)
            {
                var left = Simplify(SubRange(pts, 0, idx + 1), epsilon);
                var right = Simplify(SubRange(pts, idx, pts.Count), epsilon);
                result.AddRange(left);
                for (int i = 1; i < right.Count; i++) result.Add(right[i]);
            }
            else
            {
                result.Add(pts[0]);
                result.Add(pts[pts.Count - 1]);
            }
            return result;
        }

        /// <summary>SubRange 方法。</summary>
        private static List<HPoint> SubRange(IList<HPoint> src, int start, int end)
        {
            var r = new List<HPoint>(end - start);
            for (int i = start; i < end; i++) r.Add(src[i]);
            return r;
        }
    }
}
