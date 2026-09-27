using HFromUI.HControl;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HFromUI.HBase;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    using HFromUI.HData;
    /// <summary>
    /// 由两个 <see cref="HPoint"/> 定义的直线（无限延伸）或线段，
    /// 提供丰富的几何计算与变换方法。
    /// 斜率 K 与截距 B 通过 <see cref="GetEquation"/> 方法获取。
    /// </summary>
    public class HLine : HLineBase
    {
        private HPoint _start, _end;



        #region 构造与基本属性

        /// <summary>
        /// 使用指定的起点和终点构造直线段。
        /// </summary>
        /// <param name="start">起点，不可为 null。</param>
        /// <param name="end">终点，不可为 null。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="start"/> 或 <paramref name="end"/> 为 null。
        /// </exception>
        public HLine(HPoint start, HPoint end)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _end = end ?? throw new ArgumentNullException(nameof(end));
        }

        /// <summary>
        /// 获取或设置起点。赋值后所有计算结果自动同步。
        /// </summary>
        /// <exception cref="ArgumentNullException">设置的值为 null。</exception>
        public override HPoint HPointStart
        {
            get => _start;
            set => _start = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 获取或设置终点。
        /// </summary>
        /// <exception cref="ArgumentNullException">设置的值为 null。</exception>
        public override HPoint HPointEnd
        {
            get => _end;
            set => _end = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>X 方向的差值。</summary>
        public double DeltaX => _end.X.Value - _start.X.Value;

        /// <summary>Y 方向的差值。</summary>
        public double DeltaY => _end.Y.Value - _start.Y.Value;

        /// <summary>线段的长度（<see cref="HDouble"/> 类型）。</summary>
        public HDouble Length =>
            new HDouble { Value = Math.Sqrt(DeltaX * DeltaX + DeltaY * DeltaY) };

        /// <summary>线段的中点。</summary>
        public HPoint MidPoint =>
            new HPoint((_start.X.Value + _end.X.Value) * 0.5, (_start.Y.Value + _end.Y.Value) * 0.5);

        /// <summary>直线与 X 轴正方向的夹角（弧度），范围 (-π, π]。</summary>
        public HDouble Angle =>
            new HDouble { Value = Math.Atan2(DeltaY, DeltaX) };

        /// <summary>线段方向向量（单位向量）。</summary>
        public (double X, double Y) Direction
        {
            get
            {
                double len = Length.Value;
                if (len < HAppData.Epsilon) return (0, 0);
                return (DeltaX / len, DeltaY / len);
            }
        }

        #endregion
        /// <summary>
        /// 从线段起点出发，沿线段方向前进指定的距离，返回对应点的坐标。
        /// 如果指定距离大于等于线段长度，则返回终点；
        /// 如果指定距离小于等于 0，则返回起点；
        /// 否则返回线段上按比例内插的点。
        /// </summary>
        /// <param name="distance">要前进的距离。负值视为 0，超出长度视为长度值。</param>
        /// <returns>线段上的对应点。</returns>
        public HPoint PointFromStartAtDistance(double distance)
        {
            double len = Length.Value;
            if (len < HAppData.Epsilon)
            {
                // 退化点：线段长度为 0，始终返回起点
                return new HPoint(_start.X.Value, _start.Y.Value);
            }

            if (distance <= 0)
                return new HPoint(_start.X.Value, _start.Y.Value);

            if (distance >= len)
                return new HPoint(_end.X.Value, _end.Y.Value);

            double t = distance / len;
            return PointAtRatio(t);
        }
        #region 方程与状态

        /// <summary>
        /// 获取直线的斜率 K 和截距 B（基于 y = Kx + B）。
        /// 垂直时 K 为 ±∞（IsValid 为 false）；两点重合时 K 为 NaN。
        /// </summary>
        /// <returns>
        /// 返回 (<see cref="HDouble"/> K, <see cref="HDouble"/> B) 元组。
        /// </returns>
        public (HDouble K, HDouble B) GetEquation()
        {
            double dx = DeltaX;
            double dy = DeltaY;

            // 斜率
            HDouble k;
            if (Math.Abs(dx) < HAppData.Epsilon)
            {
                // 垂直线
                if (Math.Abs(dy) < HAppData.Epsilon)
                    k = new HDouble { Value = double.NaN }; // 退化为点
                else
                    k = new HDouble { Value = dy > 0 ? double.PositiveInfinity : double.NegativeInfinity };
            }
            else
            {
                k = new HDouble { Value = dy / dx };
            }

            // 截距（仅在 K 有限时有效）
            HDouble b;
            if (!k.IsValid || double.IsInfinity(k.Value))
                b = new HDouble { Value = double.NaN };
            else
                b = new HDouble { Value = _start.Y.Value - k.Value * _start.X.Value };

            return (k, b);
        }

        /// <summary>
        /// 直线是否垂直（起点与终点的 X 坐标之差接近于零）。
        /// </summary>
        public bool IsVertical => Math.Abs(DeltaX) < HAppData.Epsilon;

        /// <summary>
        /// 直线是否水平（起点与终点的 Y 坐标之差接近于零）。
        /// </summary>
        public bool IsHorizontal => Math.Abs(DeltaY) < HAppData.Epsilon;

        /// <summary>
        /// 直线是否退化成一个点（起点与终点重合）。
        /// </summary>
        public bool IsPoint => IsVertical && Math.Abs(DeltaY) < HAppData.Epsilon;

        /// <summary>
        /// 返回直线的一般式系数：Ax + By + C = 0。
        /// </summary>
        /// <returns>(A, B, C)</returns>
        public (double A, double B, double C) GetGeneralForm()
        {
            double A = DeltaY;
            double B = -DeltaX;
            double C = DeltaX * _start.Y.Value - DeltaY * _start.X.Value;
            return (A, B, C);
        }

        /// <summary>
        /// 从一般式系数构造直线（至少需两个不同的点，此处取与两坐标轴的交点）。
        /// </summary>
        public static HLine FromGeneralForm(double A, double B, double C)
        {
            if (Math.Abs(A) < HAppData.Epsilon && Math.Abs(B) < HAppData.Epsilon)
                throw new ArgumentException(HTranslation.GetContent("A 和 B 不能同时为零。"));

            HPoint p1, p2;
            if (Math.Abs(A) > Math.Abs(B))
            {
                // 取 y = 0 和 y = 1
                p1 = new HPoint((-C - B * 0) / A, 0);
                p2 = new HPoint((-C - B * 1) / A, 1);
            }
            else
            {
                // 取 x = 0 和 x = 1
                p1 = new HPoint(0, (-C - A * 0) / B);
                p2 = new HPoint(1, (-C - A * 1) / B);
            }
            return new HLine(p1, p2);
        }

        #endregion

        #region 点到直线/线段的距离与投影

        /// <summary>
        /// 计算点到当前直线所在无限直线的垂直距离（HDouble）。
        /// </summary>
        /// <param name="point">不可为 null。</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <summary>DistanceToPoint 方法。</summary>
        /// <summary>DistanceToPoint 方法。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HDouble DistanceToPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = DeltaX, dy = DeltaY;

            if (IsPoint)
            {
                double px = point.X.Value - _start.X.Value, py = point.Y.Value - _start.Y.Value;
                return new HDouble { Value = Math.Sqrt(px * px + py * py) };
            }

            double cross = Math.Abs(dx * (_start.Y.Value - point.Y.Value) - dy * (_start.X.Value - point.X.Value));
            double length = Math.Sqrt(dx * dx + dy * dy);
            return new HDouble { Value = cross / length };
        }

        /// <summary>
        /// 计算点到线段的最近距离（若投影点落在线段外，则取端点距离）。
        /// </summary>
        public HDouble DistanceToSegment(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = DeltaX, dy = DeltaY;

            if (IsPoint)
                return new HDouble { Value = Math.Sqrt((point.X.Value - _start.X.Value) * (point.X.Value - _start.X.Value) + (point.Y.Value - _start.Y.Value) * (point.Y.Value - _start.Y.Value)) };

            double t = ((point.X.Value - _start.X.Value) * dx + (point.Y.Value - _start.Y.Value) * dy) / (dx * dx + dy * dy);
            if (t < 0) t = 0;
            if (t > 1) t = 1;

            double projX = _start.X.Value + t * dx;
            double projY = _start.Y.Value + t * dy;
            double dist = Math.Sqrt((point.X.Value - projX) * (point.X.Value - projX) + (point.Y.Value - projY) * (point.Y.Value - projY));
            return new HDouble { Value = dist };
        }
        /// <summary>判断是否 OnLine。</summary>
        public static bool IsOnLine(HPoint start, HPoint end, HPoint p1,HDouble scaleHeight)
        {
            double dx = end.X.Value - start.X.Value;
            double dy = end.Y.Value - start.Y.Value;
            double lengthSq = dx * dx + dy * dy;

            // 线段长度为 0，退化为一个点
            if (lengthSq < HAppData.Epsilon)
                return Math.Sqrt((p1.X.Value - start.X.Value) * (p1.X.Value - start.X.Value) + (p1.Y.Value - start.Y.Value) * (p1.Y.Value - start.Y.Value)) <  scaleHeight;

            // 1. 计算点到直线的垂直距离（叉积 / 线段长度）
            double cross = (p1.X.Value - start.X.Value) * dy - (p1.Y.Value - start.Y.Value) * dx;
            double distance = Math.Abs(cross) / Math.Sqrt(lengthSq);
            if (distance >  scaleHeight)
                return false;

            // 2. 检查投影是否在线段范围内（点积）
            double dot = (p1.X.Value - start.X.Value) * dx + (p1.Y.Value - start.Y.Value) * dy;
            if (dot < 0 || dot > lengthSq)
                return false;

            return true;
        }
        /// <summary>
        /// 计算点到直线（无限）的垂足（投影点）。
        /// 退化点时返回起点。
        /// </summary>
        /// <summary>获取 projection。</summary>
        /// <summary>获取 projection。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HPoint GetProjection(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = DeltaX, dy = DeltaY;

            if (IsPoint) return new HPoint(_start.X.Value, _start.Y.Value);

            double t = ((point.X.Value - _start.X.Value) * dx + (point.Y.Value - _start.Y.Value) * dy) / (dx * dx + dy * dy);
            return new HPoint(_start.X.Value + t * dx, _start.Y.Value + t * dy);
        }

        /// <summary>
        /// 计算点到线段上最近的点（可能为端点）。
        /// </summary>
        public HPoint ClosestPointOnSegment(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = DeltaX, dy = DeltaY;
            if (IsPoint) return new HPoint(_start.X.Value, _start.Y.Value);

            double t = ((point.X.Value - _start.X.Value) * dx + (point.Y.Value - _start.Y.Value) * dy) / (dx * dx + dy * dy);
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return new HPoint(_start.X.Value + t * dx, _start.Y.Value + t * dy);
        }

        /// <summary>
        /// 判断指定点是否在直线（无限）上（距离小于容差）。
        /// </summary>
        public bool IsPointOnLine(HPoint point, double tolerance = 1e-10)
        {
            return DistanceToPoint(point).Value < tolerance;
        }
        /// <summary>
        /// 计算两条直线的交点（无限直线）。
        /// 平行时返回 null，否则返回交点。
        /// </summary>
        /// <param name="A1">第一条直线的点1</param>
        /// <param name="A2">第一条直线的点2</param>
        /// <param name="B1">第二条直线的点1</param>
        /// <param name="B2">第二条直线的点2</param>
        /// <returns>交点；平行时返回 null</returns>
        public static HPoint GetLineIntersection(HPoint A1, HPoint A2, HPoint B1, HPoint B2)
        {
            double x1 = (double)A1.X.Value, y1 = (double)A1.Y.Value;
            double x2 = (double)A2.X.Value, y2 = (double)A2.Y.Value;
            double x3 = (double)B1.X.Value, y3 = (double)B1.Y.Value;
            double x4 = (double)B2.X.Value, y4 = (double)B2.Y.Value;

            double dx1 = x2 - x1, dy1 = y2 - y1;
            double dx2 = x4 - x3, dy2 = y4 - y3;

            double cross = dx1 * dy2 - dy1 * dx2;
            // 平行或共线，视为无唯一交点
            if (Math.Abs(cross) < 1e-12)
                return null;

            double t = ((x3 - x1) * dy2 - (y3 - y1) * dx2) / cross;
            return new HPoint(x1 + t * dx1, y1 + t * dy1);
        }

        /// <summary>
        /// 在两条线段构成的角内生成半径为 <paramref name="radius"/> 的圆角过渡点。
        /// 要求两条线段的长度均 ≥ 半径，且不平行，否则返回 null。
        /// </summary>
        /// <param name="A1">第一条线段的端点1</param>
        /// <param name="A2">第一条线段的端点2</param>
        /// <param name="B1">第二条线段的端点1</param>
        /// <param name="B2">第二条线段的端点2</param>
        /// <param name="radius">圆角半径</param>
        /// <returns>
        /// 包含5个点的数组：
        /// [0] 第一条线段上离交点较远的端点，
        /// [1] 第一条线段上的切点（从交点向远端点方向移动半径距离），
        /// [2] 圆弧中点，
        /// [3] 第二条线段上的切点，
        /// [4] 第二条线段上离交点较远的端点。
        /// 若任一线段长度小于半径或直线平行则返回 null。
        /// </returns>
        public static HPoint[] GetFilletPoints(HPoint A1, HPoint A2, HPoint B1, HPoint B2, double radius)
        {
            // 1. 线段长度检查
            double len1 = A1.DistanceTo(A2).Value;
            double len2 = B1.DistanceTo(B2).Value;
            if (len1 < radius || len2 < radius)
                return null;

            // 2. 求交点
            HPoint I = GetLineIntersection(A1, A2, B1, B2);
            if (I == null)   // 平行
                return null;

            // 3. 找出两条线段上距离交点较远的端点
            HPoint far1 = I.DistanceTo(A1) >= I.DistanceTo(A2) ? A1 : A2;
            HPoint far2 = I.DistanceTo(B1) >= I.DistanceTo(B2) ? B1 : B2;

            // 4. 从交点指向远端点的单位方向向量
            HPoint dir1 = (far1 - I).Normalized;
            HPoint dir2 = (far2 - I).Normalized;

            // 5. 切点 = 交点 + 方向 * 半径
            HPoint T1 = I + dir1 * radius;
            HPoint T2 = I + dir2 * radius;

            // 6. 圆弧中点（以交点为圆心，半径作圆，取两方向夹角的平分线方向）
            HPoint midDir = (dir1 + dir2).Normalized;
            HPoint M = I + midDir * radius;

            return new HPoint[] { far1, T1, M, T2, far2 };
        }
        /// <summary>
        /// 判断指定点是否在线段上（包含端点，距离小于容差）。
        /// </summary>
        public bool IsPointOnSegment(HPoint point, double tolerance = 1e-10)
        {
            return DistanceToSegment(point).Value < tolerance;
        }

        /// <summary>
        /// 判断两点在当前直线的同侧还是异侧。
        /// </summary>
        /// <returns>1 表示同侧，-1 表示异侧，0 表示至少一点在线上。</returns>
        public int SameSide(HPoint p1, HPoint p2)
        {
            double dx = DeltaX, dy = DeltaY;
            double cross1 = dx * (p1.Y.Value - _start.Y.Value) - dy * (p1.X.Value - _start.X.Value);
            double cross2 = dx * (p2.Y.Value - _start.Y.Value) - dy * (p2.X.Value - _start.X.Value);
            if (Math.Abs(cross1) < HAppData.Epsilon || Math.Abs(cross2) < HAppData.Epsilon) return 0;
            return (cross1 * cross2 > 0) ? 1 : -1;
        }

        #endregion

        #region 直线与直线的相互关系

        /// <summary>
        /// 判断与另一条直线是否平行（方向向量叉积接近于零）。
        /// </summary>
        public bool IsParallelTo(HLine other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double cross = DeltaX * other.DeltaY - DeltaY * other.DeltaX;
            return Math.Abs(cross) < HAppData.Epsilon;
        }

        /// <summary>
        /// 判断与另一条直线是否垂直（方向向量点积接近于零）。
        /// </summary>
        public bool IsPerpendicularTo(HLine other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double dot = DeltaX * other.DeltaX + DeltaY * other.DeltaY;
            return Math.Abs(dot) < HAppData.Epsilon;
        }

        /// <summary>
        /// 计算当前无限直线与另一无限直线的交点。平行或共线时返回 null。
        /// </summary>
        public HPoint IntersectionWith(HLine other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double a1 = DeltaY, b1 = -DeltaX, c1 = DeltaX * _start.Y.Value - DeltaY * _start.X.Value;
            double a2 = other.DeltaY, b2 = -other.DeltaX, c2 = other.DeltaX * other._start.Y.Value - other.DeltaY * other._start.X.Value;

            double det = a1 * b2 - a2 * b1;
            if (Math.Abs(det) < HAppData.Epsilon) return null; // 平行或共线

            double x = (b1 * c2 - b2 * c1) / det;
            double y = (a2 * c1 - a1 * c2) / det;
            return new HPoint(x, y);
        }

        /// <summary>
        /// 计算当前线段与另一线段的交点。若不存在唯一交点则返回 null。
        /// </summary>
        public HPoint IntersectionWithSegment(HLine other)
        {
            HPoint inter = IntersectionWith(other);
            if (inter == null) return null;
            // 检查交点是否在两个线段上
            if (IsPointOnSegment(inter) && other.IsPointOnSegment(inter))
                return inter;
            return null;
        }

        #endregion

        #region 构造相关直线

        /// <summary>
        /// 过指定点构造一条平行线。
        /// </summary>
        public HLine ParallelLineThroughPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            return new HLine(point, new HPoint(point.X.Value + DeltaX, point.Y.Value + DeltaY));
        }

        /// <summary>
        /// 过指定点构造一条垂线（以该点为起点，方向旋转 90°）。
        /// </summary>
        public HLine PerpendicularLineThroughPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            return new HLine(point, new HPoint(point.X.Value - DeltaY, point.Y.Value + DeltaX));
        }

        /// <summary>
        /// 返回当前直线以线段形式表示的垂线（过中点）。
        /// </summary>
        public HLine PerpendicularBisector()
        {
            var mid = MidPoint;
            return new HLine(mid, new HPoint(mid.X.Value - DeltaY, mid.Y.Value + DeltaX));
        }

        #endregion

        #region 线段操作

        /// <summary>
        /// 反转线段方向。
        /// </summary>
        public void Reverse()
        {
            HPoint temp = _start;
            _start = _end;
            _end = temp;
        }

        /// <summary>
        /// 平移线段（直线本身无变化，端点移动）。
        /// </summary>
        public void Translate(double dx, double dy)
        {
            _start.X += dx; _start.Y += dy;
            _end.X += dx; _end.Y += dy;
        }

        /// <summary>
        /// 绕指定中心旋转线段（逆时针）。
        /// </summary>
        public void Rotate(double angleRad, HPoint center = null)
        {
            center = center ?? new HPoint(0, 0);
            double cos = Math.Cos(angleRad), sin = Math.Sin(angleRad);

            double RotateX(double x, double y) => center.X.Value + (x - center.X.Value) * cos - (y - center.Y.Value) * sin;
            double RotateY(double x, double y) => center.Y.Value + (x - center.X.Value) * sin + (y - center.Y.Value) * cos;

            double newStartX = RotateX(_start.X.Value, _start.Y.Value);
            double newStartY = RotateY(_start.X.Value, _start.Y.Value);
            double newEndX = RotateX(_end.X.Value, _end.Y.Value);
            double newEndY = RotateY(_end.X.Value, _end.Y.Value);

            _start.X = newStartX; _start.Y = newStartY;
            _end.X = newEndX; _end.Y = newEndY;
        }

        /// <summary>
        /// 以指定中心缩放线段（只改变端点位置）。
        /// </summary>
        public void Scale(double sx, double sy, HPoint center = null)
        {
            center = center ?? MidPoint;
            _start.X = center.X.Value + (_start.X.Value - center.X.Value) * sx;
            _start.Y = center.Y.Value + (_start.Y.Value - center.Y.Value) * sy;
            _end.X = center.X.Value + (_end.X.Value - center.X.Value) * sx;
            _end.Y = center.Y.Value + (_end.Y.Value - center.Y.Value) * sy;
        }

        /// <summary>
        /// 获取线段上按比例 t (0~1) 分割的点。t 在 [0,1] 外时延伸至直线。
        /// </summary>
        public HPoint PointAtRatio(double t)
        {
            return new HPoint(_start.X.Value + DeltaX * t, _start.Y.Value + DeltaY * t);
        }

        /// <summary>
        /// 与 PointAtRatio 同义，线性插值。
        /// </summary>
        public HPoint Lerp(double t) => PointAtRatio(t);

        /// <summary>
        /// 获取线段按给定长度从起点延伸后的点（可负向）。
        /// </summary>
        public HPoint PointAtDistance(double distance)
        {
            double len = Length.Value;
            if (len < HAppData.Epsilon) return new HPoint(_start.X.Value, _start.Y.Value);
            double t = distance / len;
            return PointAtRatio(t);
        }

        #endregion

        #region 与其他几何图形相交

        /// <summary>
        /// 计算直线（无限）与圆的交点，最多两个。
        /// </summary>
        /// <param name="center">圆心</param>
        /// <param name="radius">半径</param>
        /// <returns>交点列表（0、1 或 2 个点）</returns>
        public List<HPoint> IntersectionWithCircle(HPoint center, double radius)
        {
            var result = new List<HPoint>();
            if (center == null) throw new ArgumentNullException(nameof(center));

            double dx = DeltaX, dy = DeltaY;
            double fx = _start.X.Value - center.X.Value;
            double fy = _start.Y.Value - center.Y.Value;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - radius * radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < -HAppData.Epsilon) return result; // 无交点
            if (discriminant < HAppData.Epsilon) discriminant = 0;

            double sqrtD = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtD) / (2 * a);
            double t2 = (-b + sqrtD) / (2 * a);

            result.Add(PointAtRatio(t1));
            if (Math.Abs(t1 - t2) > HAppData.Epsilon)
                result.Add(PointAtRatio(t2));
            return result;
        }

        /// <summary>
        /// 判断线段是否与轴对齐矩形相交。
        /// </summary>
        public bool IntersectsRect(double minX, double minY, double maxX, double maxY)
        {
            // 快速排斥 + 跨立实验简化版
            if (_start.X.Value < minX && _end.X.Value < minX) return false;
            if (_start.X.Value > maxX && _end.X.Value > maxX) return false;
            if (_start.Y.Value < minY && _end.Y.Value < minY) return false;
            if (_start.Y.Value > maxY && _end.Y.Value > maxY) return false;

            // 裁剪至矩形边界
            double p1 = (minX - _start.X.Value) / DeltaX;
            double p2 = (maxX - _start.X.Value) / DeltaX;
            double p3 = (minY - _start.Y.Value) / DeltaY;
            double p4 = (maxY - _start.Y.Value) / DeltaY;

            double tmin = Math.Max(Math.Min(p1, p2), Math.Min(p3, p4));
            double tmax = Math.Min(Math.Max(p1, p2), Math.Max(p3, p4));

            return tmax >= Math.Max(0, tmin) && Math.Min(1, tmax) >= tmin;
        }

        #endregion

        #region 工具

        /// <summary>
        /// 创建当前直线的深拷贝。
        /// </summary>
        public HLine Clone() => new HLine(_start.Clone(), _end.Clone());

        /// <summary>
        /// 返回直线的字符串表示。
        /// </summary>
        public override string ToString()
        {
            var (k, b) = GetEquation();
            if (IsPoint) return $"Point({_start.X.Value}, {_start.Y.Value})";
            if (IsVertical) return $"Vertical x ≈ {_start.X.Value}";
            return $"y = {k.Value:F4}x + {b.Value:F4}";
        }

        #endregion
    }
}
