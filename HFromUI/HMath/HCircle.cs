using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HFromUI.HMath
{
    /// <summary>
    /// 二维圆（可变引用类型），提供丰富的几何运算方法。
    /// 支持包含检测、点到圆周距离、切线点、交点计算等。
    /// </summary>
    [Serializable]
    public class HCircle
    {
        #region 字段

        private HPoint _center;
        private double _radius;

        #endregion

        #region 属性

        /// <summary>圆心坐标。</summary>
        public HPoint Center
        {
            get => _center;
            set => _center = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>半径。</summary>
        public double Radius
        {
            get => _radius;
            set => _radius = Math.Max(0, value);       // 半径不能为负
        }

        /// <summary>直径。</summary>
        public double Diameter => _radius * 2.0;

        /// <summary>圆的面积（π·r²）。</summary>
        public double Area => Math.PI * _radius * _radius;

        /// <summary>周长（2π·r）。</summary>
        public double Circumference => 2.0 * Math.PI * _radius;

        /// <summary>周长（与 Circumference 相同）。</summary>
        public double Length => Circumference;

        /// <summary>
        /// 圆是否退化（半径接近于零）。
        /// </summary>
        public bool IsDegenerate => _radius < 1e-15;

        #endregion

        #region 构造函数

        /// <summary>默认构造，圆心为 (0,0)，半径为 0。</summary>
        public HCircle()
        {
            _center = new HPoint(0, 0);
            _radius = 0;
        }

        /// <summary>使用圆心和半径构造。</summary>
        /// <param name="center">圆心，不可为 null。</param>
        /// <param name="radius">半径，负值将取绝对值。</param>
        public HCircle(HPoint center, double radius)
        {
            _center = center ?? throw new ArgumentNullException(nameof(center));
            _radius = Math.Abs(radius);
        }

        /// <summary>使用圆心坐标 (x, y) 和半径构造。</summary>
        public HCircle(double x, double y, double radius)
        {
            _center = new HPoint(x, y);
            _radius = Math.Abs(radius);
        }

        /// <summary>使用三个点构造外接圆（三点不共线）。若共线则返回退化圆（半径为 0）。</summary>
        /// <param name="p1">第一个点。</param>
        /// <param name="p2">第二个点。</param>
        /// <param name="p3">第三个点。</param>
        public HCircle(HPoint p1, HPoint p2, HPoint p3)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            if (p3 == null) throw new ArgumentNullException(nameof(p3));

            // 计算三角形的外接圆圆心
            double d = 2 * (p1.X.Value * (p2.Y.Value - p3.Y.Value) + p2.X.Value * (p3.Y.Value - p1.Y.Value) + p3.X.Value * (p1.Y.Value - p2.Y.Value));
            if (Math.Abs(d) < 1e-15)
            {
                _center = new HPoint(0, 0);
                _radius = 0;
            }
            else
            {
                double ux = ((p1.X.Value * p1.X.Value + p1.Y.Value * p1.Y.Value) * (p2.Y.Value - p3.Y.Value) +
                             (p2.X.Value * p2.X.Value + p2.Y.Value * p2.Y.Value) * (p3.Y.Value - p1.Y.Value) +
                             (p3.X.Value * p3.X.Value + p3.Y.Value * p3.Y.Value) * (p1.Y.Value - p2.Y.Value)) / d;
                double uy = ((p1.X.Value * p1.X.Value + p1.Y.Value * p1.Y.Value) * (p3.X.Value - p2.X.Value) +
                             (p2.X.Value * p2.X.Value + p2.Y.Value * p2.Y.Value) * (p1.X.Value - p3.X.Value) +
                             (p3.X.Value * p3.X.Value + p3.Y.Value * p3.Y.Value) * (p2.X.Value - p1.X.Value)) / d;
                _center = new HPoint(ux, uy);
                _radius = _center.DistanceTo(p1).Value;
            }
        }

        #endregion

        #region 静态工厂方法

        /// <summary>通过直径的两个端点构造圆。</summary>
        public static HCircle FromDiameter(HPoint p1, HPoint p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            var center = new HPoint((p1.X.Value + p2.X.Value) * 0.5, (p1.Y.Value + p2.Y.Value) * 0.5);
            double radius = p1.DistanceTo(p2).Value * 0.5;
            return new HCircle(center, radius);
        }

        /// <summary>通过圆上的三个点构造外接圆（静态版本）。</summary>
        public static HCircle FromThreePoints(HPoint p1, HPoint p2, HPoint p3)
        {
            return new HCircle(p1, p2, p3);
        }

        #endregion

        #region 实例方法：与点的关系

        /// <summary>
        /// 判断指定点是否在圆内（包括圆周）。
        /// </summary>
        public bool ContainsPoint(HPoint point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            return point.DistanceSquaredTo(_center) <= (_radius + tolerance) * (_radius + tolerance);
        }

        /// <summary>
        /// 计算点到圆周的最短距离（HDouble）。若点在圆内，距离 = 半径 - 到圆心距离；若点在圆外，距离 = 到圆心距离 - 半径。
        /// </summary>
        public HDouble DistanceToPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double distToCenter = point.DistanceTo(_center).Value;
            return new HDouble { Value = Math.Abs(distToCenter - _radius) };
        }

        /// <summary>
        /// 返回圆上距离指定点最近的点（径向投影）。
        /// 若圆心与给定点重合，则返回圆上任意一点（例如 (center.X + radius, center.Y)）。
        /// </summary>
        public HPoint ClosestPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = point.X.Value - _center.X.Value;
            double dy = point.Y.Value - _center.Y.Value;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1e-15)
                return new HPoint(_center.X.Value + _radius, _center.Y.Value);   // 返回正右方点
            double scale = _radius / dist;
            return new HPoint(_center.X.Value + dx * scale, _center.Y.Value + dy * scale);
        }

        /// <summary>
        /// 返回从给定点向圆引出的两条切线的切点（最多两个）。若点在圆内则返回空列表。
        /// </summary>
        public List<HPoint> TangentPointsFromPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            var result = new List<HPoint>();
            double d = point.DistanceTo(_center).Value;
            if (d <= _radius - 1e-10) return result;   // 点在圆内或圆周上

            double a = Math.Asin(_radius / d);
            double baseAngle = Math.Atan2(point.Y.Value - _center.Y.Value, point.X.Value - _center.X.Value);
            double r = Math.Sqrt(d * d - _radius * _radius);   // 切线长

            double angle1 = baseAngle + a;
            double angle2 = baseAngle - a;
            result.Add(new HPoint(point.X.Value + r * Math.Cos(angle1 + Math.PI), point.Y.Value + r * Math.Sin(angle1 + Math.PI))); // 从切点反推？
            // 更直接：切点 = 圆心 + 半径 * 旋转方向
            result.Add(new HPoint(_center.X.Value + _radius * Math.Cos(angle1), _center.Y.Value + _radius * Math.Sin(angle1)));
            result.Add(new HPoint(_center.X.Value + _radius * Math.Cos(angle2), _center.Y.Value + _radius * Math.Sin(angle2)));
            return result;
        }

        #endregion

        #region 实例方法：与直线相交

        /// <summary>计算圆与无限直线的交点（最多两个）。返回列表可能为空。</summary>
        public List<HPoint> IntersectionWithLine(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var result = new List<HPoint>();
            double dx = line.HPointEnd.X.Value - line.HPointStart.X.Value;
            double dy = line.HPointEnd.Y.Value - line.HPointStart.Y.Value;
            double fx = line.HPointStart.X.Value - _center.X.Value;
            double fy = line.HPointStart.Y.Value - _center.Y.Value;

            double a = dx * dx + dy * dy;
            double b = 2 * (fx * dx + fy * dy);
            double c = (fx * fx + fy * fy) - _radius * _radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < -1e-12) return result;
            if (discriminant < 1e-12) discriminant = 0;

            double sqrtD = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtD) / (2 * a);
            double t2 = (-b + sqrtD) / (2 * a);

            result.Add(new HPoint(line.HPointStart.X.Value + t1 * dx, line.HPointStart.Y.Value + t1 * dy));
            if (Math.Abs(t1 - t2) > 1e-12)
                result.Add(new HPoint(line.HPointStart.X.Value + t2 * dx, line.HPointStart.Y.Value + t2 * dy));
            return result;
        }

        /// <summary>计算圆与线段的交点（仅保留落在段内的点）。</summary>
        public List<HPoint> IntersectionWithSegment(HLine segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            var lineIntersections = IntersectionWithLine(segment);
            lineIntersections.RemoveAll(p => !segment.IsPointOnSegment(p));
            return lineIntersections;
        }

        #endregion

        #region 实例方法：与圆相交

        /// <summary>计算两个圆的交点（最多两个）。同心圆或不相交返回空列表。</summary>
        public List<HPoint> IntersectionWithCircle(HCircle other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            var result = new List<HPoint>();
            double d = _center.DistanceTo(other._center).Value;
            if (d < 1e-12) return result; // 同心圆

            double r1 = _radius;
            double r2 = other._radius;

            // 检查是否相交
            if (d > r1 + r2 + 1e-12 || d < Math.Abs(r1 - r2) - 1e-12) return result;

            double a = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
            double hSq = r1 * r1 - a * a;
            double h = hSq > 0 ? Math.Sqrt(hSq) : 0;

            double midX = _center.X.Value + a * (other._center.X.Value - _center.X.Value) / d;
            double midY = _center.Y.Value + a * (other._center.Y.Value - _center.Y.Value) / d;

            double rx = -(other._center.Y.Value - _center.Y.Value) * (h / d);
            double ry = (other._center.X.Value - _center.X.Value) * (h / d);

            result.Add(new HPoint(midX + rx, midY + ry));
            if (hSq > 1e-12)
                result.Add(new HPoint(midX - rx, midY - ry));
            return result;
        }

        #endregion

        #region 实例方法：几何变换

        /// <summary>平移圆，返回新圆（不改变原圆）。</summary>
        public HCircle Translate(double dx, double dy)
        {
            return new HCircle(new HPoint(_center.X.Value + dx, _center.Y.Value + dy), _radius);
        }

        /// <summary>以指定中心缩放圆（仅改变半径），返回新圆。</summary>
        public HCircle ScaleAt(HPoint center, double sx, double sy)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            double newRadius = _radius * Math.Max(Math.Abs(sx), Math.Abs(sy));
            return new HCircle(new HPoint(center.X.Value + (_center.X.Value - center.X.Value) * sx,
                                          center.Y.Value + (_center.Y.Value - center.Y.Value) * sy), newRadius);
        }

        /// <summary>将圆缩放为另一个圆（直接修改当前实例）。</summary>
        public void Transform(double newRadius)
        {
            _radius = Math.Abs(newRadius);
        }

        #endregion

        #region 实例方法：边界与转换

        /// <summary>圆的轴对齐包围盒（minX, maxX, minY, maxY）。</summary>
        public (double MinX, double MaxX, double MinY, double MaxY) BoundingBox
        {
            get
            {
                double minX = _center.X.Value - _radius;
                double maxX = _center.X.Value + _radius;
                double minY = _center.Y.Value - _radius;
                double maxY = _center.Y.Value + _radius;
                return (minX, maxX, minY, maxY);
            }
        }

        /// <summary>将圆转换为逼近的多边形（默认 64 段）。</summary>
        public List<HPoint> ToPolyline(int pointsCount = 64)
        {
            if (pointsCount < 3) pointsCount = 3;
            var poly = new List<HPoint>(pointsCount);
            double angleStep = 2.0 * Math.PI / pointsCount;
            for (int i = 0; i < pointsCount; i++)
            {
                double a = i * angleStep;
                poly.Add(new HPoint(_center.X.Value + _radius * Math.Cos(a),
                                    _center.Y.Value + _radius * Math.Sin(a)));
            }
            return poly;
        }

        #endregion

        #region 工具方法

        /// <summary>深拷贝当前圆。</summary>
        public HCircle Clone() => new HCircle(_center.Clone(), _radius);

        /// <summary>基于坐标和半径的相等比较（带容差）。</summary>
        public static bool operator ==(HCircle a, HCircle b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return Math.Abs(a._radius - b._radius) < 1e-10 &&
                   a._center == b._center;
        }

        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HCircle a, HCircle b) => !(a == b);

        /// <summary>相等性判断。</summary>
        public override bool Equals(object obj)
        {
            if (obj is HCircle other) return this == other;
            return false;
        }

        public override int GetHashCode() => _center.GetHashCode() ^ _radius.GetHashCode();

        /// <summary>返回圆的字符串表示。</summary>
        public override string ToString() => $"Circle(Center={_center}, R={_radius:F4})";

        #endregion
    }
}
