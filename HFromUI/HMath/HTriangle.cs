using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    /// <summary>
    /// 二维三角形（可变引用类型），提供丰富的几何计算：边长、角度、面积、各类中心、包含、距离、相交等。
    /// 所有标量结果均使用 <see cref="HDouble"/> 包装。
    /// </summary>
    [Serializable]
    public class HTriangle
    {
        #region 顶点标识枚举

        /// <summary>三角形的三个顶点标识。</summary>
        public enum Vertex
        {
            A,
            B,
            C
        }

        #endregion

        #region 字段与属性

        private HPoint _a, _b, _c;

        /// <summary>顶点 A。</summary>
        public HPoint A { get => _a; set => _a = value ?? throw new ArgumentNullException(nameof(value)); }
        /// <summary>顶点 B。</summary>
        public HPoint B { get => _b; set => _b = value ?? throw new ArgumentNullException(nameof(value)); }
        /// <summary>顶点 C。</summary>
        public HPoint C { get => _c; set => _c = value ?? throw new ArgumentNullException(nameof(value)); }

        #endregion

        #region 构造函数

        /// <summary>默认构造：(0,0), (1,0), (0,1)。</summary>
        public HTriangle()
        {
            _a = new HPoint(0, 0);
            _b = new HPoint(1, 0);
            _c = new HPoint(0, 1);
        }

        /// <summary>使用三个顶点构造。</summary>
        public HTriangle(HPoint a, HPoint b, HPoint c)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            _c = c ?? throw new ArgumentNullException(nameof(c));
        }

        /// <summary>使用三边长度构造（SSS），a 为边 BC，b 为 CA，c 为 AB。</summary>
        public static HTriangle FromSSS(double sideA, double sideB, double sideC)
        {
            if (sideA <= 0 || sideB <= 0 || sideC <= 0)
                throw new ArgumentException(HTranslation.GetContent("边长必须为正。"));
            if (sideA + sideB <= sideC || sideA + sideC <= sideB || sideB + sideC <= sideA)
                throw new ArgumentException(HTranslation.GetContent("不满足三角形不等式。"));
            // 将 A 放在原点，B 放在 (c, 0)，通过余弦定律计算 C
            double c = sideC; // AB 长度
            double x = (sideB * sideB + c * c - sideA * sideA) / (2 * c);
            double y = Math.Sqrt(Math.Max(0, sideB * sideB - x * x));
            return new HTriangle(new HPoint(0, 0), new HPoint(c, 0), new HPoint(x, y));
        }

        /// <summary>使用两边及其夹角构造（SAS），a = AB，b = AC，angleA = 夹角（弧度）。</summary>
        public static HTriangle FromSAS(double sideAB, double sideAC, double angleA)
        {
            if (sideAB <= 0 || sideAC <= 0) throw new ArgumentException(HTranslation.GetContent("边长必须为正。"));
            var a = new HPoint(0, 0);
            var b = new HPoint(sideAB, 0);
            var c = new HPoint(sideAC * Math.Cos(angleA), sideAC * Math.Sin(angleA));
            return new HTriangle(a, b, c);
        }

        /// <summary>使用两角及其公共边构造（ASA），sideAB 为边 AB，angleA 和 angleB 为相邻两角（弧度）。</summary>
        public static HTriangle FromASA(double sideAB, double angleA, double angleB)
        {
            if (sideAB <= 0) throw new ArgumentException(HTranslation.GetContent("边长必须为正。"));
            if (angleA + angleB >= Math.PI) throw new ArgumentException(HTranslation.GetContent("两角和必须小于 π。"));
            double angleC = Math.PI - angleA - angleB;
            double b = sideAB * Math.Sin(angleB) / Math.Sin(angleC);
            double c = sideAB * Math.Sin(angleA) / Math.Sin(angleC);
            return FromSSS(sideAB, b, c);
        }

        #endregion

        #region 基本几何量

        /// <summary>边 BC 的长度。</summary>
        public HDouble LengthA => new HDouble { Value = _b.DistanceTo(_c).Value };
        /// <summary>边 CA 的长度。</summary>
        public HDouble LengthB => new HDouble { Value = _c.DistanceTo(_a).Value };
        /// <summary>边 AB 的长度。</summary>
        public HDouble LengthC => new HDouble { Value = _a.DistanceTo(_b).Value };

        /// <summary>周长。</summary>
        public HDouble Perimeter => new HDouble { Value = LengthA.Value + LengthB.Value + LengthC.Value };

        /// <summary>半周长。</summary>
        public double SemiPerimeter => (LengthA.Value + LengthB.Value + LengthC.Value) * 0.5;

        /// <summary>面积（海伦公式）。</summary>
        public HDouble Area
        {
            get
            {
                double s = SemiPerimeter;
                double a = LengthA.Value, b = LengthB.Value, c = LengthC.Value;
                double area = Math.Sqrt(Math.Max(0, s * (s - a) * (s - b) * (s - c)));
                return new HDouble { Value = area };
            }
        }

        /// <summary>角度 A（顶点 A 处的内角，弧度）。</summary>
        public HDouble AngleA => CalculateAngle(_b, _a, _c);
        /// <summary>角度 B。</summary>
        public HDouble AngleB => CalculateAngle(_a, _b, _c);
        /// <summary>角度 C。</summary>
        public HDouble AngleC => CalculateAngle(_b, _c, _a);

        /// <summary>CalculateAngle 方法。</summary>
        private static HDouble CalculateAngle(HPoint p1, HPoint vertex, HPoint p2)
        {
            double dx1 = p1.X.Value - vertex.X.Value, dy1 = p1.Y.Value - vertex.Y.Value;
            double dx2 = p2.X.Value - vertex.X.Value, dy2 = p2.Y.Value - vertex.Y.Value;
            double dot = dx1 * dx2 + dy1 * dy2;
            double len1 = Math.Sqrt(dx1 * dx1 + dy1 * dy1);
            double len2 = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
            if (len1 < 1e-15 || len2 < 1e-15) return new HDouble { Value = double.NaN };
            double cos = Math.Max(-1.0, Math.Min(1.0, dot / (len1 * len2)));
            return new HDouble { Value = Math.Acos(cos) };
        }

        #endregion

        #region 三角形中心

        /// <summary>重心（三条中线交点）。</summary>
        public HPoint Centroid => new HPoint(
            (_a.X.Value + _b.X.Value + _c.X.Value) / 3.0,
            (_a.Y.Value + _b.Y.Value + _c.Y.Value) / 3.0);

        /// <summary>外心（外接圆圆心）。</summary>
        public HPoint Circumcenter
        {
            get
            {
                double d = 2 * (_a.X.Value * (_b.Y.Value - _c.Y.Value) + _b.X.Value * (_c.Y.Value - _a.Y.Value) + _c.X.Value * (_a.Y.Value - _b.Y.Value));
                if (Math.Abs(d) < 1e-15) return new HPoint(double.NaN, double.NaN);
                double ux = ((_a.X.Value * _a.X.Value + _a.Y.Value * _a.Y.Value) * (_b.Y.Value - _c.Y.Value) +
                             (_b.X.Value * _b.X.Value + _b.Y.Value * _b.Y.Value) * (_c.Y.Value - _a.Y.Value) +
                             (_c.X.Value * _c.X.Value + _c.Y.Value * _c.Y.Value) * (_a.Y.Value - _b.Y.Value)) / d;
                double uy = ((_a.X.Value * _a.X.Value + _a.Y.Value * _a.Y.Value) * (_c.X.Value - _b.X.Value) +
                             (_b.X.Value * _b.X.Value + _b.Y.Value * _b.Y.Value) * (_a.X.Value - _c.X.Value) +
                             (_c.X.Value * _c.X.Value + _c.Y.Value * _c.Y.Value) * (_b.X.Value - _a.X.Value)) / d;
                return new HPoint(ux, uy);
            }
        }

        /// <summary>外接圆半径。</summary>
        public HDouble Circumradius
        {
            get
            {
                double a = LengthA.Value, b = LengthB.Value, c = LengthC.Value;
                double area = Area.Value;
                if (area < 1e-15) return new HDouble { Value = double.PositiveInfinity };
                return new HDouble { Value = (a * b * c) / (4 * area) };
            }
        }

        /// <summary>内心（内切圆圆心）。</summary>
        public HPoint Incenter
        {
            get
            {
                double a = LengthA.Value, b = LengthB.Value, c = LengthC.Value;
                double p = a + b + c;
                if (p < 1e-15) return new HPoint(_a.X.Value, _a.Y.Value);
                return new HPoint(
                    (a * _a.X.Value + b * _b.X.Value + c * _c.X.Value) / p,
                    (a * _a.Y.Value + b * _b.Y.Value + c * _c.Y.Value) / p);
            }
        }

        /// <summary>内切圆半径。</summary>
        public HDouble Inradius
        {
            get
            {
                double area = Area.Value;
                double s = SemiPerimeter;
                if (s < 1e-15) return new HDouble { Value = 0 };
                return new HDouble { Value = area / s };
            }
        }

        /// <summary>垂心（三条高线交点）。</summary>
        public HPoint Orthocenter
        {
            get
            {
                // 垂心是重心和外心的三倍关系：O = 3*G - 2*H? 实际 H = 3*G - 2*O
                var g = Centroid;
                var o = Circumcenter;
                return new HPoint(3 * g.X.Value - 2 * o.X.Value, 3 * g.Y.Value - 2 * o.Y.Value);
            }
        }

        #endregion

        #region 内切圆、外接圆

        /// <summary>外接圆。</summary>
        public HCircle Circumcircle
        {
            get
            {
                var center = Circumcenter;
                double r = Circumradius.Value;
                if (double.IsNaN(center.X.Value) || double.IsInfinity(r))
                    return new HCircle(new HPoint(0, 0), 0);
                return new HCircle(center, r);
            }
        }

        /// <summary>内切圆。</summary>
        public HCircle Incircle
        {
            get
            {
                var center = Incenter;
                double r = Inradius.Value;
                return new HCircle(center, r);
            }
        }

        #endregion

        #region 顶点与边操作

        /// <summary>获取指定顶点的坐标。</summary>
        public HPoint GetVertex(Vertex v)
        {
            switch (v)
            {
                case Vertex.A: return _a;
                case Vertex.B: return _b;
                case Vertex.C: return _c;
                default: throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        /// <summary>设置指定顶点的坐标。</summary>
        public void SetVertex(Vertex v, HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            switch (v)
            {
                case Vertex.A: _a = point; break;
                case Vertex.B: _b = point; break;
                case Vertex.C: _c = point; break;
                default: throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        /// <summary>获取指定顶点所对的边（两个点）。</summary>
        public (HPoint Start, HPoint End) OppositeEdge(Vertex v)
        {
            switch (v)
            {
                case Vertex.A: return (_b, _c);
                case Vertex.B: return (_a, _c);
                case Vertex.C: return (_a, _b);
                default: throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        /// <summary>获取指定顶点的对边长度。</summary>
        public HDouble OppositeLength(Vertex v)
        {
            switch (v)
            {
                case Vertex.A: return LengthA;
                case Vertex.B: return LengthB;
                case Vertex.C: return LengthC;
                default: throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        #endregion

        #region 点与三角形的关系

        /// <summary>判断点是否在三角形内部（包括边界）。</summary>
        public bool ContainsPoint(HPoint point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            // 重心坐标法
            double d1 = Sign(point, _a, _b);
            double d2 = Sign(point, _b, _c);
            double d3 = Sign(point, _c, _a);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        /// <summary>点到三角形的最短距离（HDouble）。点在内部时返回0。</summary>
        public HDouble DistanceToPoint(HPoint point)
        {
            if (ContainsPoint(point)) return new HDouble { Value = 0 };
            // 取到三条边距离的最小值
            double d1 = new HLine(_a, _b).DistanceToSegment(point).Value;
            double d2 = new HLine(_b, _c).DistanceToSegment(point).Value;
            double d3 = new HLine(_c, _a).DistanceToSegment(point).Value;
            return new HDouble { Value = Math.Min(d1, Math.Min(d2, d3)) };
        }

        /// <summary>三角形边界上距离指定点最近的点。</summary>
        public HPoint ClosestPoint(HPoint point)
        {
            if (ContainsPoint(point)) return point.Clone();
            HPoint p1 = new HLine(_a, _b).ClosestPointOnSegment(point);
            HPoint p2 = new HLine(_b, _c).ClosestPointOnSegment(point);
            HPoint p3 = new HLine(_c, _a).ClosestPointOnSegment(point);
            double d1 = point.DistanceSquaredTo(p1).Value;
            double d2 = point.DistanceSquaredTo(p2).Value;
            double d3 = point.DistanceSquaredTo(p3).Value;
            double min = Math.Min(d1, Math.Min(d2, d3));
            if (min == d1) return p1;
            if (min == d2) return p2;
            return p3;
        }

        /// <summary>Sign 方法。</summary>
        private static double Sign(HPoint p1, HPoint p2, HPoint p3)
        {
            return (p1.X.Value - p3.X.Value) * (p2.Y.Value - p3.Y.Value) - (p2.X.Value - p3.X.Value) * (p1.Y.Value - p3.Y.Value);
        }

        #endregion

        #region 与直线、线段、圆、矩形的相交

        /// <summary>三角形的三条边。</summary>
        public List<HLine> Edges()
        {
            return new List<HLine>
            {
                new HLine(_a, _b),
                new HLine(_b, _c),
                new HLine(_c, _a)
            };
        }

        /// <summary>三角形与无限直线的交点（最多6个点）。</summary>
        public List<HPoint> IntersectionWithLine(HLine line)
        {
            var result = new List<HPoint>();
            foreach (var edge in Edges())
            {
                HPoint inter = line.IntersectionWith(edge);
                if (inter != null && edge.IsPointOnSegment(inter))
                {
                    if (!result.Any(p => p == inter))
                        result.Add(inter);
                }
            }
            return result;
        }

        /// <summary>三角形与线段的交点。</summary>
        public List<HPoint> IntersectionWithSegment(HLine segment)
        {
            var lineIntersections = IntersectionWithLine(segment);
            lineIntersections.RemoveAll(p => !segment.IsPointOnSegment(p));
            return lineIntersections;
        }

        /// <summary>判断三角形是否与圆相交。</summary>
        public bool IntersectsCircle(HCircle circle)
        {
            if (circle == null) throw new ArgumentNullException(nameof(circle));
            // 如果圆包含任意顶点，或圆心到三角形距离 <= 半径
            if (ContainsPoint(circle.Center)) return true;
            foreach (var edge in Edges())
            {
                if (circle.IntersectionWithSegment(edge).Count > 0) return true;
            }
            return false;
        }

        /// <summary>判断三角形是否与矩形相交。</summary>
        public bool IntersectsRectangle(HRectangle rect)
        {
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            // 检查任一顶点在矩形内，或矩形任一顶点在三角形内，或边相交
            if (rect.ContainsPoint(_a) || rect.ContainsPoint(_b) || rect.ContainsPoint(_c)) return true;
            if (ContainsPoint(rect.TopLeft) || ContainsPoint(rect.TopRight) ||
                ContainsPoint(rect.BottomLeft) || ContainsPoint(rect.BottomRight)) return true;
            foreach (var edge in Edges())
            {
                if (rect.IntersectionWithSegment(edge).Count > 0) return true;
            }
            return false;
        }

        #endregion

        #region 几何变换

        /// <summary>平移三角形，返回新三角形。</summary>
        public HTriangle Translate(double dx, double dy)
        {
            return new HTriangle(_a.Translate(dx, dy), _b.Translate(dx, dy), _c.Translate(dx, dy));
        }

        /// <summary>绕指定中心旋转，返回新三角形。</summary>
        public HTriangle Rotate(double angleRad, HPoint center = null)
        {
            center = center ?? Centroid;
            return new HTriangle(
                _a.RotateAround(center, angleRad),
                _b.RotateAround(center, angleRad),
                _c.RotateAround(center, angleRad));
        }

        /// <summary>以指定中心缩放，返回新三角形。</summary>
        public HTriangle Scale(double sx, double sy, HPoint center = null)
        {
            center = center ?? Centroid;
            return new HTriangle(
                _a.ScaleAt(center, sx, sy),
                _b.ScaleAt(center, sx, sy),
                _c.ScaleAt(center, sx, sy));
        }

        #endregion

        #region 多边形转换

        /// <summary>返回三个顶点组成的列表（顺序 A, B, C）。</summary>
        public List<HPoint> ToPolygon() => new List<HPoint> { _a, _b, _c };

        /// <summary>三角形内切圆逼近多边形（用于绘制，默认 32 段）。</summary>
        public List<HPoint> ToIncirclePolyline(int segments = 32) => Incircle.ToPolyline(segments);

        /// <summary>三角形外接圆逼近多边形。</summary>
        public List<HPoint> ToCircumcirclePolyline(int segments = 64) => Circumcircle.ToPolyline(segments);

        #endregion

        #region 工具

        /// <summary>深拷贝三角形。</summary>
        public HTriangle Clone() => new HTriangle(_a.Clone(), _b.Clone(), _c.Clone());

        public override string ToString() =>
            $"Triangle(A={_a}, B={_b}, C={_c})";

        public override bool Equals(object obj)
        {
            if (obj is HTriangle other)
                return _a == other._a && _b == other._b && _c == other._c;
            return false;
        }

        public override int GetHashCode()
        {
            return _a.GetHashCode() ^ _b.GetHashCode() ^ _c.GetHashCode();
        }

        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HTriangle a, HTriangle b) => a?.Equals(b) ?? b is null;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HTriangle a, HTriangle b) => !(a == b);

        #endregion
    }
}
