using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    /// <summary>
    /// 二维椭圆（可变引用类型），以中心、半长轴、半短轴和旋转角定义。
    /// 提供丰富的几何运算：包含判断、距离、最近点、切线、交点、周长近似等。
    /// 所有标量结果均使用 <see cref="HDouble"/> 包装。
    /// </summary>
    [Serializable]
    public class HEllipse
    {
        #region 字段

        private HPoint _center;
        private double _semiMajor;  // 半长轴 a
        private double _semiMinor;  // 半短轴 b
        private double _angleRad;   // 旋转角（弧度），长轴与 X 轴夹角

        #endregion

        #region 属性

        /// <summary>椭圆中心。</summary>
        public HPoint Center
        {
            get => _center;
            set => _center = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>半长轴（a），自动保持不小于半短轴。</summary>
        public double SemiMajorAxis
        {
            get => _semiMajor;
            set
            {
                _semiMajor = Math.Abs(value);
                if (_semiMajor < _semiMinor)
                {
                    double tmp = _semiMajor;
                    _semiMajor = _semiMinor;
                    _semiMinor = tmp;
                    _angleRad += Math.PI * 0.5; // 交换轴时旋转角补偿 90°
                }
            }
        }

        /// <summary>半短轴（b），自动保持不大于半长轴。</summary>
        public double SemiMinorAxis
        {
            get => _semiMinor;
            set
            {
                _semiMinor = Math.Abs(value);
                if (_semiMinor > _semiMajor)
                {
                    double tmp = _semiMajor;
                    _semiMajor = _semiMinor;
                    _semiMinor = tmp;
                    _angleRad += Math.PI * 0.5;
                }
            }
        }

        /// <summary>旋转角（弧度），长轴与 X 轴正方向的夹角，范围通常 (-π/2, π/2]。</summary>
        public double AngleRad
        {
            get => _angleRad;
            set => _angleRad = value % Math.PI; // 规范到 (-π/2, π/2] 附近
        }

        /// <summary>椭圆面积（π·a·b）。</summary>
        public double Area => Math.PI * _semiMajor * _semiMinor;

        /// <summary>焦距（中心到焦点的距离）。</summary>
        public double FocalLength => Math.Sqrt(Math.Max(0, _semiMajor * _semiMajor - _semiMinor * _semiMinor));

        /// <summary>离心率 e = c / a（圆时为 0）。</summary>
        public double Eccentricity => _semiMajor < 1e-15 ? 0 : FocalLength / _semiMajor;

        /// <summary>是否为圆（a ≈ b）。</summary>
        public bool IsCircle => Math.Abs(_semiMajor - _semiMinor) < 1e-10;

        /// <summary>是否退化（a 或 b 接近 0）。</summary>
        public bool IsDegenerate => _semiMajor < 1e-15 || _semiMinor < 1e-15;

        /// <summary>两个焦点坐标。</summary>
        public (HPoint Focus1, HPoint Focus2) Foci
        {
            get
            {
                double c = FocalLength;
                double cos = Math.Cos(_angleRad);
                double sin = Math.Sin(_angleRad);
                return (
                    new HPoint(_center.X.Value + c * cos, _center.Y.Value + c * sin),
                    new HPoint(_center.X.Value - c * cos, _center.Y.Value - c * sin)
                );
            }
        }

        #endregion

        #region 构造函数

        /// <summary>默认构造：中心 (0,0)，半轴均为 0，角度 0。</summary>
        public HEllipse()
        {
            _center = new HPoint(0, 0);
            _semiMajor = 0;
            _semiMinor = 0;
            _angleRad = 0;
        }

        /// <summary>用中心、半长轴、半短轴和旋转角构造。</summary>
        public HEllipse(HPoint center, double semiMajor, double semiMinor, double angleRad = 0)
        {
            _center = center ?? throw new ArgumentNullException(nameof(center));
            // 使用属性确保 a >= b
            _semiMajor = Math.Abs(semiMajor);
            _semiMinor = Math.Abs(semiMinor);
            if (_semiMajor < _semiMinor)
            {
                double tmp = _semiMajor; _semiMajor = _semiMinor; _semiMinor = tmp;
                _angleRad = angleRad + Math.PI * 0.5;
            }
            else
                _angleRad = angleRad;
        }

        /// <summary>通过两个焦点和一个点构造（点必须在椭圆上）。</summary>
        public static HEllipse FromFociAndPoint(HPoint focus1, HPoint focus2, HPoint pointOnEllipse)
        {
            if (focus1 == null) throw new ArgumentNullException(nameof(focus1));
            if (focus2 == null) throw new ArgumentNullException(nameof(focus2));
            if (pointOnEllipse == null) throw new ArgumentNullException(nameof(pointOnEllipse));

            double distSum = pointOnEllipse.DistanceTo(focus1).Value + pointOnEllipse.DistanceTo(focus2).Value;
            double a = distSum * 0.5;
            double c = focus1.DistanceTo(focus2).Value * 0.5;
            double b = Math.Sqrt(Math.Max(0, a * a - c * c));
            var center = new HPoint((focus1.X.Value + focus2.X.Value) * 0.5, (focus1.Y.Value + focus2.Y.Value) * 0.5);
            double angle = Math.Atan2(focus2.Y.Value - focus1.Y.Value, focus2.X.Value - focus1.X.Value) + Math.PI * 0.5;
            return new HEllipse(center, a, b, angle);
        }

        /// <summary>用一般式系数 Ax² + Bxy + Cy² + Dx + Ey + F = 0 构造（需满足椭圆条件）。</summary>
        public static HEllipse FromGeneralForm(double A, double B, double C, double D, double E, double F)
        {
            // 化简实现：通过二次型分解求出中心、轴和旋转角
            double det = B * B - 4 * A * C;
            if (det >= 0) throw new ArgumentException(HTranslation.GetContent("系数不构成椭圆。"));

            double xc = (2 * C * D - B * E) / det;
            double yc = (2 * A * E - B * D) / det;
            double centerX = -xc;
            double centerY = -yc;

            // 旋转角
            double theta = 0.5 * Math.Atan2(B, A - C);
            double cos = Math.Cos(theta), sin = Math.Sin(theta);

            // 在旋转坐标系中的系数
            double Aprime = A * cos * cos + B * sin * cos + C * sin * sin;
            double Cprime = A * sin * sin - B * sin * cos + C * cos * cos;
            double Fprime = F - (A * xc * xc + B * xc * yc + C * yc * yc);

            double a = Math.Sqrt(-Fprime / Aprime);
            double b = Math.Sqrt(-Fprime / Cprime);
            return new HEllipse(new HPoint(centerX, centerY), a, b, theta);
        }

        #endregion

        #region 参数方程与周长

        /// <summary>根据离心角 t（0～2π）返回椭圆上的点。</summary>
        public HPoint PointAtAngle(double eccentricAngle)
        {
            double cosT = Math.Cos(eccentricAngle);
            double sinT = Math.Sin(eccentricAngle);
            double cosA = Math.Cos(_angleRad);
            double sinA = Math.Sin(_angleRad);
            double x = _center.X.Value + _semiMajor * cosT * cosA - _semiMinor * sinT * sinA;
            double y = _center.Y.Value + _semiMajor * cosT * sinA + _semiMinor * sinT * cosA;
            return new HPoint(x, y);
        }

        /// <summary>椭圆周长的近似值（Ramanujan 第二近似，精度高）。</summary>
        public HDouble Circumference
        {
            get
            {
                double a = _semiMajor, b = _semiMinor;
                if (Math.Abs(a - b) < 1e-15) return new HDouble { Value = 2 * Math.PI * a };
                double h = (a - b) * (a - b) / ((a + b) * (a + b));
                double sum = 1 + (3 * h) / (10 + Math.Sqrt(4 - 3 * h));
                return new HDouble { Value = Math.PI * (a + b) * sum };
            }
        }

        /// <summary>周长。</summary>
        public HDouble Length => Circumference;

        #endregion

        #region 点关系：包含、距离、最近点

        /// <summary>判断点是否在椭圆内（含边界）。</summary>
        public bool ContainsPoint(HPoint point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            // 变换到椭圆局部坐标系
            double dx = point.X.Value - _center.X.Value;
            double dy = point.Y.Value - _center.Y.Value;
            double cos = Math.Cos(-_angleRad);
            double sin = Math.Sin(-_angleRad);
            double localX = dx * cos - dy * sin;
            double localY = dx * sin + dy * cos;
            double test = (localX * localX) / (_semiMajor * _semiMajor) + (localY * localY) / (_semiMinor * _semiMinor);
            return test <= 1 + tolerance;
        }

        /// <summary>点到椭圆的最短距离（HDouble）。使用迭代法。</summary>
        public HDouble DistanceToPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            HPoint closest = ClosestPoint(point);
            return new HDouble { Value = point.DistanceTo(closest).Value };
        }

        /// <summary>椭圆上距离给定点最近的点（迭代优化）。</summary>
        public HPoint ClosestPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));

            // 退化椭圆（直线或点）直接用几何方法
            if (_semiMinor < 1e-15) // 退化线段
            {
                var line = new HLine(
                    new HPoint(_center.X.Value - _semiMajor * Math.Cos(_angleRad), _center.Y.Value - _semiMajor * Math.Sin(_angleRad)),
                    new HPoint(_center.X.Value + _semiMajor * Math.Cos(_angleRad), _center.Y.Value + _semiMajor * Math.Sin(_angleRad)));
                return line.ClosestPointOnSegment(point);
            }
            if (_semiMajor < 1e-15) return _center.Clone();

            // 使用 Newton-Raphson 迭代求解离心角 t
            double dx = point.X.Value - _center.X.Value;
            double dy = point.Y.Value - _center.Y.Value;
            double cosA = Math.Cos(_angleRad);
            double sinA = Math.Sin(_angleRad);
            double a = _semiMajor, b = _semiMinor;

            // 将点转到椭圆局部坐标
            double localX = dx * cosA + dy * sinA;
            double localY = -dx * sinA + dy * cosA;

            // 初始猜测（根据局部坐标角度）
            double t = Math.Atan2(localY / b, localX / a);
            if (Math.Abs(localX) < 1e-12 && Math.Abs(localY) < 1e-12) t = 0;

            for (int i = 0; i < 15; i++)
            {
                double cosT = Math.Cos(t);
                double sinT = Math.Sin(t);
                double f1 = a * cosT * (a * sinT) - localX * a * sinT + localY * b * cosT; // 一阶条件
                double f2 = a * a * cosT * cosT + b * b * sinT * sinT + localX * a * cosT + localY * b * sinT; // 近似导数
                if (Math.Abs(f2) < 1e-15) break;
                double dt = f1 / f2;
                t -= dt;
                if (Math.Abs(dt) < 1e-10) break;
            }

            // 从局部坐标反算全局点
            double ex = _center.X.Value + a * Math.Cos(t) * cosA - b * Math.Sin(t) * sinA;
            double ey = _center.Y.Value + a * Math.Cos(t) * sinA + b * Math.Sin(t) * cosA;
            return new HPoint(ex, ey);
        }

        /// <summary>返回从点向椭圆引出的切线切点（最多两个）。若点在椭圆内则返回空。</summary>
        public List<HPoint> TangentPointsFromPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            var result = new List<HPoint>();
            if (ContainsPoint(point, 0)) return result; // 内部或边界无切线

            // 使用极线法求切点
            double dx = point.X.Value - _center.X.Value;
            double dy = point.Y.Value - _center.Y.Value;
            double cos = Math.Cos(-_angleRad);
            double sin = Math.Sin(-_angleRad);
            double u = dx * cos - dy * sin;
            double v = dx * sin + dy * cos;
            double a2 = _semiMajor * _semiMajor;
            double b2 = _semiMinor * _semiMinor;

            // 极线方程：u*x/a² + v*y/b² = 1  与椭圆 x²/a² + y²/b² = 1 联立
            double A = (u * u) / (a2 * a2) + (v * v) / (b2 * b2);
            double B = 2 * u / a2;
            double C = 2 * v / b2;
            // 解关于 t 的参数
            double disc = 4 * ((u * u) / a2 + (v * v) / b2 - 1);
            if (disc < 0) return result;

            double sqrtD = Math.Sqrt(disc);
            double denom = (u * u) / a2 + (v * v) / b2;
            // 交点参数 t = -B ± sqrtD? 这里直接用几何法：切点满足 (x - xc)·(px - xc)/a² + ...
            // 更稳健：在局部坐标系中解二次
            double term1 = u * u / a2 + v * v / b2;
            double x1 = (a2 * b2 * u + a2 * b2 * v * sqrtD) / (a2 * v * v + b2 * u * u);
            // 简化：直接使用通用公式
            double a = _semiMajor, b = _semiMinor;
            double px = u, py = v;
            double d = Math.Sqrt(a * a * py * py + b * b * px * px);
            double k = a * b / d;
            double t1 = Math.Atan2(py / b, px / a);
            double theta = Math.Atan2(a * py * k, b * px * k);
            double xT1 = _center.X.Value + a * Math.Cos(theta) * cos - b * Math.Sin(theta) * sin;
            double yT1 = _center.Y.Value + a * Math.Cos(theta) * sin + b * Math.Sin(theta) * cos;
            result.Add(new HPoint(xT1, yT1));
            // 第二个切点
            double xT2 = _center.X.Value + a * Math.Cos(theta + Math.PI) * cos - b * Math.Sin(theta + Math.PI) * sin;
            double yT2 = _center.Y.Value + a * Math.Cos(theta + Math.PI) * sin + b * Math.Sin(theta + Math.PI) * cos;
            result.Add(new HPoint(xT2, yT2));
            return result;
        }

        #endregion

        #region 与直线相交

        /// <summary>椭圆与无限直线的交点（0～2个）。</summary>
        public List<HPoint> IntersectionWithLine(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var result = new List<HPoint>();
            double dx = line.HPointEnd.X.Value - line.HPointStart.X.Value;
            double dy = line.HPointEnd.Y.Value - line.HPointStart.Y.Value;
            if (Math.Abs(dx) < 1e-12 && Math.Abs(dy) < 1e-12) return result;

            // 将直线转换到椭圆局部坐标系
            double cos = Math.Cos(-_angleRad);
            double sin = Math.Sin(-_angleRad);
            double x1 = line.HPointStart.X.Value - _center.X.Value;
            double y1 = line.HPointStart.Y.Value - _center.Y.Value;
            double x2 = line.HPointEnd.X.Value - _center.X.Value;
            double y2 = line.HPointEnd.Y.Value - _center.Y.Value;
            double lx1 = x1 * cos - y1 * sin;
            double ly1 = x1 * sin + y1 * cos;
            double lx2 = x2 * cos - y2 * sin;
            double ly2 = x2 * sin + y2 * cos;
            double ldx = lx2 - lx1;
            double ldy = ly2 - ly1;

            double a2 = _semiMajor * _semiMajor;
            double b2 = _semiMinor * _semiMinor;
            double A = ldx * ldx / a2 + ldy * ldy / b2;
            double B = 2 * (lx1 * ldx / a2 + ly1 * ldy / b2);
            double C = lx1 * lx1 / a2 + ly1 * ly1 / b2 - 1;

            double disc = B * B - 4 * A * C;
            if (disc < 0) return result;
            double sqrtD = Math.Sqrt(Math.Max(0, disc));
            double t1 = (-B - sqrtD) / (2 * A);
            double t2 = (-B + sqrtD) / (2 * A);

            double cosA = Math.Cos(_angleRad);
            double sinA = Math.Sin(_angleRad);
            double gx1 = line.HPointStart.X.Value + t1 * dx;
            double gy1 = line.HPointStart.Y.Value + t1 * dy;
            double gx2 = line.HPointStart.X.Value + t2 * dx;
            double gy2 = line.HPointStart.Y.Value + t2 * dy;
            result.Add(new HPoint(gx1, gy1));
            if (disc > 1e-12)
                result.Add(new HPoint(gx2, gy2));
            return result;
        }

        /// <summary>椭圆与线段的交点。</summary>
        public List<HPoint> IntersectionWithSegment(HLine segment)
        {
            var lineIntersections = IntersectionWithLine(segment);
            lineIntersections.RemoveAll(p => !segment.IsPointOnSegment(p));
            return lineIntersections;
        }

        #endregion

        #region 与圆、椭圆相交

        /// <summary>椭圆与圆的交点（迭代法）。</summary>
        public List<HPoint> IntersectionWithCircle(HCircle circle)
        {
            if (circle == null) throw new ArgumentNullException(nameof(circle));
            // 使用一般方程联立，通过数值迭代得到（略），直接返回空或不精确结果
            // 为实用，采用从椭圆局部坐标系的数值求解
            var result = new List<HPoint>();
            // 将圆变换到椭圆局部坐标
            double cos = Math.Cos(-_angleRad);
            double sin = Math.Sin(-_angleRad);
            double cx = circle.Center.X.Value - _center.X.Value;
            double cy = circle.Center.Y.Value - _center.Y.Value;
            double lcx = cx * cos - cy * sin;
            double lcy = cx * sin + cy * cos;
            double r = circle.Radius;

            // 在局部坐标系中，椭圆为 x²/a² + y²/b² = 1，圆为 (x-lcx)² + (y-lcy)² = r²
            // 这是一个四次方程，采用采样+Newton法
            for (int i = 0; i <= 72; i++)
            {
                double angle = i * 2 * Math.PI / 72;
                double guessX = lcx + r * Math.Cos(angle);
                double guessY = lcy + r * Math.Sin(angle);
                // 简单 Newton 迭代
                for (int iter = 0; iter < 5; iter++)
                {
                    double f1 = guessX * guessX / (SemiMajorAxis * SemiMajorAxis) + guessY * guessY / (SemiMinorAxis * SemiMinorAxis) - 1;
                    double f2 = (guessX - lcx) * (guessX - lcx) + (guessY - lcy) * (guessY - lcy) - r * r;
                    double j11 = 2 * guessX / (SemiMajorAxis * SemiMajorAxis);
                    double j12 = 2 * guessY / (SemiMinorAxis * SemiMinorAxis);
                    double j21 = 2 * (guessX - lcx);
                    double j22 = 2 * (guessY - lcy);
                    double det = j11 * j22 - j12 * j21;
                    if (Math.Abs(det) < 1e-15) break;
                    double dx = (f2 * j12 - f1 * j22) / det;
                    double dy = (f1 * j21 - f2 * j11) / det;
                    guessX -= dx;
                    guessY -= dy;
                    if (Math.Abs(dx) < 1e-8 && Math.Abs(dy) < 1e-8) break;
                }
                // 投影到椭圆上
                double t = Math.Atan2(guessY / SemiMinorAxis, guessX / SemiMajorAxis);
                double ex = SemiMajorAxis * Math.Cos(t);
                double ey = SemiMinorAxis * Math.Sin(t);
                double dist = Math.Sqrt((ex - lcx) * (ex - lcx) + (ey - lcy) * (ey - lcy));
                if (Math.Abs(dist - r) < 1e-6)
                {
                    double gx = _center.X.Value + ex * cos - ey * sin;
                    double gy = _center.Y.Value + ex * sin + ey * cos;
                    var pt = new HPoint(gx, gy);
                    if (!result.Any(p => p == pt)) result.Add(pt);
                }
            }
            return result;
        }

        /// <summary>与另一椭圆的交点（数值迭代）。</summary>
        public List<HPoint> IntersectionWithEllipse(HEllipse other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            var result = new List<HPoint>();
            // 使用类似圆相交的迭代，采样当前椭圆上的点，投影到另一个椭圆
            for (double t = 0; t < 2 * Math.PI; t += 2 * Math.PI / 100)
            {
                HPoint p = this.PointAtAngle(t);
                HPoint closest = other.ClosestPoint(p);
                if (p.DistanceTo(closest) < 1e-6)
                {
                    if (!result.Any(pt => pt == p)) result.Add(p);
                }
            }
            return result;
        }

        #endregion

        #region 几何变换

        /// <summary>平移椭圆，返回新椭圆。</summary>
        public HEllipse Translate(double dx, double dy)
        {
            return new HEllipse(new HPoint(_center.X.Value + dx, _center.Y.Value + dy), _semiMajor, _semiMinor, _angleRad);
        }

        /// <summary>旋转椭圆（改变角度），返回新椭圆。</summary>
        public HEllipse Rotate(double deltaAngleRad)
        {
            return new HEllipse(_center, _semiMajor, _semiMinor, _angleRad + deltaAngleRad);
        }

        /// <summary>以指定中心缩放轴长度，返回新椭圆。</summary>
        public HEllipse ScaleAxes(double scaleX, double scaleY)
        {
            double newA = _semiMajor * Math.Abs(scaleX);
            double newB = _semiMinor * Math.Abs(scaleY);
            return new HEllipse(_center, newA, newB, _angleRad);
        }

        /// <summary>获取旋转角为 0 时的轴对齐包围盒。</summary>
        public HRectangle GetAxisAlignedBoundingBox()
        {
            // 旋转角为0时的外接矩形
            return new HRectangle(_center.X.Value - _semiMajor, _center.Y.Value + _semiMinor,
                                  _center.X.Value + _semiMajor, _center.Y.Value - _semiMinor);
        }

        /// <summary>获取任意角度时的轴对齐包围盒（通过极值点计算）。</summary>
        public HRectangle BoundingBox()
        {
            double cos = Math.Cos(_angleRad);
            double sin = Math.Sin(_angleRad);
            double width = Math.Sqrt(_semiMajor * _semiMajor * cos * cos + _semiMinor * _semiMinor * sin * sin);
            double height = Math.Sqrt(_semiMajor * _semiMajor * sin * sin + _semiMinor * _semiMinor * cos * cos);
            return new HRectangle(_center.X.Value - width, _center.Y.Value + height,
                                  _center.X.Value + width, _center.Y.Value - height);
        }

        #endregion

        #region 多边形逼近

        /// <summary>将椭圆离散为多边形（默认64段）。</summary>
        public List<HPoint> ToPolyline(int segments = 64)
        {
            var poly = new List<HPoint>(segments);
            double step = 2 * Math.PI / segments;
            for (int i = 0; i < segments; i++)
            {
                double angle = i * step;
                poly.Add(PointAtAngle(angle));
            }
            return poly;
        }

        #endregion

        #region 工具与运算符

        /// <summary>深拷贝椭圆。</summary>
        public HEllipse Clone() => new HEllipse(_center.Clone(), _semiMajor, _semiMinor, _angleRad);

        public override string ToString() =>
            $"Ellipse(Center={_center}, a={_semiMajor:F4}, b={_semiMinor:F4}, angle={_angleRad:F4} rad)";

        public override bool Equals(object obj)
        {
            if (obj is HEllipse other)
                return _center == other._center &&
                       Math.Abs(_semiMajor - other._semiMajor) < 1e-10 &&
                       Math.Abs(_semiMinor - other._semiMinor) < 1e-10 &&
                       Math.Abs(_angleRad - other._angleRad) < 1e-10;
            return false;
        }

        public override int GetHashCode()
        {
            return _center.GetHashCode() ^ _semiMajor.GetHashCode() ^ _semiMinor.GetHashCode() ^ _angleRad.GetHashCode();
        }

        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HEllipse a, HEllipse b) => a?.Equals(b) ?? b is null;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HEllipse a, HEllipse b) => !(a == b);

        #endregion
    }
}
