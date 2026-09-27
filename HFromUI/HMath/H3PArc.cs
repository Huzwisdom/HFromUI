using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HFromUI.HBase;

namespace HFromUI.HMath
{
    /// <summary>
    /// 三点定义的一个圆弧（或整圆，当三点不共线时）。
    /// 若三点共线或重合，圆弧将退化为直线段（<see cref="IsDegenerate"/> 为 true）。
    /// 所有浮点结果均以 <see cref="HDouble"/> 包装。
    /// </summary>
    public class H3PArc : HLineBase
    {
        private HPoint _start, _middle, _end;

        #region 构造函数与属性

        /// <summary>
        /// 使用起点、中间点和终点构造圆弧。
        /// 三点顺序决定了圆弧从起点经中间点到终点的走向（弧度从 <see cref="StartAngle"/> 到 <see cref="EndAngle"/>）。
        /// </summary>
        /// <param name="start">起点，不可为 null。</param>
        /// <param name="middle">中间点，不可为 null。</param>
        /// <param name="end">终点，不可为 null。</param>
        /// <exception cref="ArgumentNullException">任意点为 null。</exception>
        public H3PArc(HPoint start, HPoint middle, HPoint end)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _middle = middle ?? throw new ArgumentNullException(nameof(middle));
            _end = end ?? throw new ArgumentNullException(nameof(end));
        }

        /// <summary>起点。修改后所有计算结果自动同步。</summary>
        public override HPoint HPointStart
        {
            get => _start;
            set => _start = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>中间点（用于确定圆弧弯曲方向）。</summary>
        public HPoint HPointMiddle
        {
            get => _middle;
            set => _middle = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>终点。</summary>
        public override HPoint HPointEnd
        {
            get => _end;
            set => _end = value ?? throw new ArgumentNullException(nameof(value));
        }

        #endregion

        #region 圆心与半径

        /// <summary>圆心坐标。三点共线或重合时返回 null。</summary>
        public HPoint Center
        {
            get
            {
                if (IsDegenerate) return null;

                // 解两条垂直平分线的交点
                double x1 = _start.X.Value, y1 = _start.Y.Value;
                double x2 = _middle.X.Value, y2 = _middle.Y.Value;
                double x3 = _end.X.Value, y3 = _end.Y.Value;

                double d = 2 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
                if (Math.Abs(d) < 1e-15) return null;

                double ux = ((x1 * x1 + y1 * y1) * (y2 - y3) +
                             (x2 * x2 + y2 * y2) * (y3 - y1) +
                             (x3 * x3 + y3 * y3) * (y1 - y2)) / d;
                double uy = ((x1 * x1 + y1 * y1) * (x3 - x2) +
                             (x2 * x2 + y2 * y2) * (x1 - x3) +
                             (x3 * x3 + y3 * y3) * (x2 - x1)) / d;

                return new HPoint(ux, uy);
            }
        }

        /// <summary>圆弧半径（HDouble）。退化时 Value 为 PositiveInfinity。</summary>
        public HDouble Radius
        {
            get
            {
                var center = Center;
                if (center == null) return new HDouble { Value = double.PositiveInfinity };
                double r = Math.Sqrt((_start.X.Value - center.X.Value) * (_start.X.Value - center.X.Value) +
                                     (_start.Y.Value - center.Y.Value) * (_start.Y.Value - center.Y.Value));
                return new HDouble { Value = r };
            }
        }

        /// <summary>三点是否共线或重合（无法确定唯一圆）。</summary>
        public bool IsDegenerate
        {
            get
            {
                double area = Math.Abs((_start.X.Value - _end.X.Value) * (_middle.Y.Value - _end.Y.Value) -
                                       (_start.Y.Value - _end.Y.Value) * (_middle.X.Value - _end.X.Value));
                return area < 1e-15;
            }
        }

        #endregion

        #region 角度方向

        /// <summary>从圆心到起点的角度（弧度，范围 (-π, π]）。退化时返回 NaN。</summary>
        public HDouble StartAngle
        {
            get
            {
                var c = Center;
                if (c == null) return new HDouble { Value = double.NaN };
                return new HDouble { Value = Math.Atan2(_start.Y.Value - c.Y.Value, _start.X.Value - c.X.Value) };
            }
        }

        /// <summary>从圆心到终点的角度（弧度）。</summary>
        public HDouble EndAngle
        {
            get
            {
                var c = Center;
                if (c == null) return new HDouble { Value = double.NaN };
                return new HDouble { Value = Math.Atan2(_end.Y.Value - c.Y.Value, _end.X.Value - c.X.Value) };
            }
        }

        /// <summary>从圆心到中间点的角度（弧度）。</summary>
        public HDouble MiddleAngle
        {
            get
            {
                var c = Center;
                if (c == null) return new HDouble { Value = double.NaN };
                return new HDouble { Value = Math.Atan2(_middle.Y.Value - c.Y.Value, _middle.X.Value - c.X.Value) };
            }
        }

        /// <summary>圆弧的扫角（弧度），范围在 [0, 2π)。退化时返回 0。</summary>
        public HDouble SweepAngle
        {
            get
            {
                var c = Center;
                if (c == null) return new HDouble { Value = 0 };

                double start = StartAngle.Value;
                double end = EndAngle.Value;
                double mid = MiddleAngle.Value;

                // 根据中间点的角度判断是劣弧还是优弧
                double sweep = end - start;
                if (sweep < 0) sweep += 2 * Math.PI;

                // 若中间点不在扫角范围，则取另一侧
                double midNorm = mid - start;
                if (midNorm < 0) midNorm += 2 * Math.PI;

                if (midNorm > sweep)
                    sweep = 2 * Math.PI - sweep;

                return new HDouble { Value = sweep };
            }
        }

        /// <summary>圆弧走向是否为顺时针。退化时返回 false。</summary>
        public bool IsClockwise
        {
            get
            {
                if (IsDegenerate) return false;
                double cross = (_middle.X.Value - _start.X.Value) * (_end.Y.Value - _start.Y.Value) -
                               (_middle.Y.Value - _start.Y.Value) * (_end.X.Value - _start.X.Value);
                return cross < 0; // 右手系，正值逆时针，负值顺时针
            }
        }

        /// <summary>返回扫角的绝对值（忽略方向，范围 [0, 2π]）。</summary>
        public HDouble AbsoluteSweepAngle => new HDouble { Value = Math.Abs(SweepAngle.Value) };

        #endregion

        #region 几何量

        /// <summary>弧长。</summary>
        public HDouble ArcLength
        {
            get
            {
                if (IsDegenerate) return new HDouble { Value = DistanceBetween(_start, _end) };
                return new HDouble { Value = Radius.Value * SweepAngle.Value };
            }
        }

        /// <summary>弦长（起点到终点的直线距离）。</summary>
        public HDouble ChordLength => new HDouble { Value = DistanceBetween(_start, _end) };

        /// <summary>弦高（圆弧中点到弦的最大距离）。</summary>
        public HDouble Sagitta
        {
            get
            {
                if (IsDegenerate) return new HDouble { Value = 0 };
                double r = Radius.Value;
                double halfChord = ChordLength.Value * 0.5;
                double sag = r - Math.Sqrt(Math.Max(0, r * r - halfChord * halfChord));
                return new HDouble { Value = sag };
            }
        }

        /// <summary>扇形面积。</summary>
        public HDouble AreaOfSector
        {
            get
            {
                if (IsDegenerate) return new HDouble { Value = 0 };
                double r = Radius.Value;
                return new HDouble { Value = 0.5 * r * r * SweepAngle.Value };
            }
        }

        /// <summary>弓形面积（扇形减去三角形）。</summary>
        public HDouble AreaOfSegment
        {
            get
            {
                if (IsDegenerate) return new HDouble { Value = 0 };
                double r = Radius.Value;
                double sweep = SweepAngle.Value;
                double triangle = 0.5 * r * r * Math.Sin(sweep);
                return new HDouble { Value = AreaOfSector.Value - triangle };
            }
        }

        #endregion

        #region 点与圆弧的关系

        /// <summary>
        /// 判断点是否在圆弧上（距离圆弧路径小于 tolerance）。
        /// </summary>
        public bool ContainsPoint(HPoint point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsDegenerate)
            {
                // 退化为线段
                var line = new HLine(_start, _end);
                return line.IsPointOnSegment(point, tolerance);
            }

            var c = Center;
            double r = Radius.Value;
            double distToCenter = DistanceBetween(point, c);
            if (Math.Abs(distToCenter - r) > tolerance) return false;

            // 检查角度是否在扫角范围内
            double angle = Math.Atan2(point.Y.Value - c.Y.Value, point.X.Value - c.X.Value);
            return IsAngleInSweep(angle);
        }

        /// <summary>点到圆弧的最短距离。</summary>
        public HDouble DistanceToPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsDegenerate)
            {
                var line = new HLine(_start, _end);
                return line.DistanceToPoint(point); // 注意：原HLine已有HDouble版本
            }

            var c = Center;
            double r = Radius.Value;
            double angle = Math.Atan2(point.Y.Value - c.Y.Value, point.X.Value - c.X.Value);

            if (IsAngleInSweep(angle))
            {
                // 点在扇形区域内，距离为到圆弧的径向距离
                double distToCenter = DistanceBetween(point, c);
                return new HDouble { Value = Math.Abs(distToCenter - r) };
            }
            else
            {
                // 投影在圆弧之外，取到两个端点距离的最小值
                double d1 = DistanceBetween(point, _start);
                double d2 = DistanceBetween(point, _end);
                return new HDouble { Value = Math.Min((double)d1, d2) };
            }
        }

        /// <summary>
        /// 返回点在圆弧上的投影（最近点）。如果退化，调用线段的最近点。
        /// </summary>
        public HPoint ClosestPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsDegenerate)
            {
                var line = new HLine(_start, _end);
                return line.ClosestPointOnSegment(point);
            }

            var c = Center;
            double r = Radius.Value;
            double angle = Math.Atan2(point.Y.Value - c.Y.Value, point.X.Value - c.X.Value);

            if (IsAngleInSweep(angle))
            {
                // 径向投影到圆弧上
                double x = c.X.Value + r * Math.Cos(angle);
                double y = c.Y.Value + r * Math.Sin(angle);
                return new HPoint(x, y);
            }
            else
            {
                // 返回距离更近的端点
                double d1 = DistanceBetween(point, _start);
                double d2 = DistanceBetween(point, _end);
                return d1 <= d2 ? _start.Clone() : _end.Clone();
            }
        }

        #endregion

        #region 角度工具

        /// <summary>判断某个角度（弧度）是否在圆弧的扫角范围内。</summary>
        private bool IsAngleInSweep(double angle)
        {
            double start = StartAngle.Value;
            double end = EndAngle.Value;
            double sweep = SweepAngle.Value;
            double a = NormalizeAngle(angle - start);
            return a >= 0 && a <= sweep + 1e-12;
        }

        /// <summary>将角度规范化到 [0, 2π)。</summary>
        private static double NormalizeAngle(double rad)
        {
            rad = rad % (2 * Math.PI);
            if (rad < 0) rad += 2 * Math.PI;
            return rad;
        }

        /// <summary>计算两点间欧氏距离（内部使用）。</summary>
        private static double DistanceBetween(HPoint a, HPoint b)
        {
            double dx = a.X.Value - b.X.Value;
            double dy = a.Y.Value - b.Y.Value;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion

        #region 相交与转换

        /// <summary>
        /// 圆弧与直线的交点（无限直线）。返回列表（0~2个点）。
        /// </summary>
        public List<HPoint> IntersectionWithLine(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (IsDegenerate)
            {
                // 线段与直线的交点
                HPoint p = line.IntersectionWith(new HLine(_start, _end));
                if (p != null && new HLine(_start, _end).IsPointOnSegment(p))
                    return new List<HPoint> { p };
                return new List<HPoint>();
            }

            var c = Center;
            double r = Radius.Value;
            var lineStart = line.HPointStart;
            var lineEnd = line.HPointEnd;

            double dx = lineEnd.X.Value - lineStart.X.Value;
            double dy = lineEnd.Y.Value - lineStart.Y.Value;
            double fx = lineStart.X.Value - c.X.Value;
            double fy = lineStart.Y.Value - c.Y.Value;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double cVal = fx * fx + fy * fy - r * r;

            double discriminant = b * b - 4 * a * cVal;
            var result = new List<HPoint>();

            if (discriminant < -1e-12) return result;
            if (discriminant < 1e-12) discriminant = 0;

            double sqrtD = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtD) / (2 * a);
            double t2 = (-b + sqrtD) / (2 * a);

            HPoint p1 = new HPoint(lineStart.X.Value + t1 * dx, lineStart.Y.Value + t1 * dy);
            HPoint p2 = new HPoint(lineStart.X.Value + t2 * dx, lineStart.Y.Value + t2 * dy);

            if (ContainsPoint(p1)) result.Add(p1);
            if (Math.Abs(t1 - t2) > 1e-12 && ContainsPoint(p2)) result.Add(p2);

            return result;
        }

        /// <summary>
        /// 圆弧与另一圆弧的交点（同一平面且圆心不同）。需要圆心、半径和角度区间。返回交点列表。
        /// </summary>
        public List<HPoint> IntersectionWithArc(H3PArc other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            var result = new List<HPoint>();
            if (IsDegenerate || other.IsDegenerate) return result;

            var c1 = this.Center;
            var c2 = other.Center;
            double r1 = this.Radius.Value;
            double r2 = other.Radius.Value;

            double d = DistanceBetween(c1, c2);
            if (d < 1e-12) return result; // 同心圆

            double a = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            double hSq = r1 * r1 - a * a;
            if (hSq < -1e-12) return result;
            double h = Math.Sqrt(Math.Max(0, hSq));

            double midX = c1.X.Value + a * (c2.X.Value - c1.X.Value) / d;
            double midY = c1.Y.Value + a * (c2.Y.Value - c1.Y.Value) / d;

            double rx = -(c2.Y.Value - c1.Y.Value) * (h / d);
            double ry = (c2.X.Value - c1.X.Value) * (h / d);

            var p1 = new HPoint(midX + rx, midY + ry);
            var p2 = new HPoint(midX - rx, midY - ry);

            if (this.ContainsPoint(p1) && other.ContainsPoint(p1)) result.Add(p1);
            if (hSq > 1e-12 && this.ContainsPoint(p2) && other.ContainsPoint(p2)) result.Add(p2);

            return result;
        }

        /// <summary>
        /// 将圆弧分解为多边形顶点（用于绘制）。采样点数默认为 64。
        /// </summary>
        public List<HPoint> ToPolyline(int pointsCount = 64)
        {
            if (pointsCount < 2) pointsCount = 2;
            var result = new List<HPoint>(pointsCount);
            if (IsDegenerate)
            {
                result.Add(_start.Clone());
                result.Add(_end.Clone());
                return result;
            }

            double start = StartAngle.Value;
            double sweep = SweepAngle.Value;
            bool clockwise = IsClockwise;
            double angleStep = clockwise ? -sweep / (pointsCount - 1) : sweep / (pointsCount - 1);
            double r = Radius.Value;
            var c = Center;

            for (int i = 0; i < pointsCount; i++)
            {
                double a = start + i * angleStep;
                result.Add(new HPoint(c.X.Value + r * Math.Cos(a), c.Y.Value + r * Math.Sin(a)));
            }
            return result;
        }

        #endregion

        #region 工具

        /// <summary>创建当前圆弧的深拷贝。</summary>
        public H3PArc Clone() => new H3PArc(_start.Clone(), _middle.Clone(), _end.Clone());

        /// <summary>返回圆弧的字符串表示。</summary>
        public override string ToString()
        {
            if (IsDegenerate)
                return $"Degenerate (line from {_start} to {_end})";
            return $"Arc Center={Center}, R={Radius.Value:F4}, Start={StartAngle.Value:F4}, Sweep={SweepAngle.Value:F4}";
        }

        #endregion
    }
}
