using System;
using System.Collections.Generic;
using System.Linq;
using HFromUI;
using HFromUI.HBase;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    /// <summary>
    /// 通用贝塞尔曲线（阶数 = 控制点数 - 1），继承 <see cref="HLineBase"/>，以首尾控制点作为起点/终点。
    /// 提供完整的求值、求导、曲率、长度、分割、升/降阶、最近点、直线求交、距离点、多边形逼近等。
    /// 所有数值结果均使用 <see cref="HDouble"/> 包装，以安全处理无穷/NaN。
    /// </summary>
    public class HBezier : HLineBase
    {
        private List<HPoint> _controlPoints;

        /// <summary>贝塞尔曲线的控制点列表（至少包含起点和终点）。</summary>
        public List<HPoint> ControlPoints
        {
            get => _controlPoints;
            set => _controlPoints = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>曲线的阶数（控制点数 - 1）。</summary>
        public int Degree => ControlPoints.Count - 1;

        /// <summary>获取或设置起点（第一个控制点）。</summary>
        public override HPoint HPointStart
        {
            get => ControlPoints[0];
            set => ControlPoints[0] = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>获取或设置终点（最后一个控制点）。</summary>
        public override HPoint HPointEnd
        {
            get => ControlPoints[ControlPoints.Count - 1];
            set => ControlPoints[ControlPoints.Count - 1] = value ?? throw new ArgumentNullException(nameof(value));
        }

        #region 构造函数

        /// <summary>使用控制点序列创建贝塞尔曲线（点数至少为 2）。</summary>
        public HBezier(IEnumerable<HPoint> controlPoints)
        {
            if (controlPoints == null) throw new ArgumentNullException(nameof(controlPoints));
            var pts = controlPoints.ToList();
            if (pts.Count < 2)
                throw new ArgumentException(HTranslation.GetContent("控制点不能少于 2 个。"), nameof(controlPoints));
            ControlPoints = pts;
        }

        /// <summary>二次贝塞尔曲线。</summary>
        public HBezier(HPoint p0, HPoint p1, HPoint p2)
            : this(new[] { p0, p1, p2 }) { }

        /// <summary>三次贝塞尔曲线。</summary>
        public HBezier(HPoint p0, HPoint p1, HPoint p2, HPoint p3)
            : this(new[] { p0, p1, p2, p3 }) { }

        #endregion

        #region 核心求值（de Casteljau）

        /// <summary>计算参数 t（0～1）对应的曲线点。</summary>
        public HPoint PointAt(double t) => DeCasteljau(ControlPoints, t);

        /// <summary>获取参数 t 处的一阶导数（切线向量）。</summary>
        public HPoint FirstDerivative(double t)
        {
            if (ControlPoints.Count == 2)
                return ControlPoints[1].Subtract(ControlPoints[0]);
            var reduced = GetDerivativeControlPoints(ControlPoints);
            return DeCasteljau(reduced, t);
        }

        /// <summary>获取参数 t 处的二阶导数。</summary>
        public HPoint SecondDerivative(double t)
        {
            if (ControlPoints.Count < 3) return new HPoint(0, 0);
            var firstDerivPts = GetDerivativeControlPoints(ControlPoints);
            if (firstDerivPts.Count < 2) return new HPoint(0, 0);
            var secondDerivPts = GetDerivativeControlPoints(firstDerivPts);
            return DeCasteljau(secondDerivPts, t);
        }

        /// <summary>获取参数 t 处的单位切线向量。零导数时返回 (0,0)。</summary>
        public HPoint TangentAt(double t)
        {
            var d1 = FirstDerivative(t);
            double len = d1.Length.Value;
            if (len < 1e-15) return new HPoint(0, 0);
            return new HPoint(d1.X.Value / len, d1.Y.Value / len);
        }

        /// <summary>获取参数 t 处的单位法向量（切线逆时针旋转90°）。</summary>
        public HPoint NormalAt(double t)
        {
            var d1 = FirstDerivative(t);
            double len = d1.Length.Value;
            if (len < 1e-15) return new HPoint(0, 0);
            return new HPoint(-d1.Y.Value / len, d1.X.Value / len);
        }

        /// <summary>计算参数 t 处的曲率（绝对值）。</summary>
        public HDouble CurvatureAt(double t)
        {
            var d1 = FirstDerivative(t);
            var d2 = SecondDerivative(t);
            double cross = d1.X.Value * d2.Y.Value - d1.Y.Value * d2.X.Value;
            double len = d1.Length.Value;
            if (len < 1e-15) return new HDouble { Value = 0 };
            return new HDouble { Value = Math.Abs(cross) / (len * len * len) };
        }

        #endregion

        #region 长度（自适应 Simpson 积分）

        /// <summary>使用自适应 Simpson 法则估算曲线总长度，精度默认为 1e-6。</summary>
        public HDouble ApproximateLength(double tolerance = 1e-6)
        {
            double length = 0;
            int n = 8;
            while (true)
            {
                double step = 1.0 / n;
                double sum = 0;
                for (int i = 0; i < n; i++)
                    sum += Simpson(i * step, (i + 1) * step);
                if (n > 1000 || Math.Abs(sum - length) < tolerance)
                {
                    length = sum;
                    break;
                }
                length = sum;
                n *= 2;
            }
            return new HDouble { Value = length };
        }

        /// <summary>计算曲线从 0 到 t 的弧长（自适应 Simpson，精度 1e-6）。</summary>
        private double PartialLength(double t, double tolerance = 1e-6)
        {
            if (t <= 0) return 0;
            if (t >= 1) return ApproximateLength().Value;

            double length = 0;
            int n = 8;
            while (true)
            {
                double step = t / n;
                double sum = 0;
                for (int i = 0; i < n; i++)
                    sum += Simpson(i * step, (i + 1) * step);
                if (n > 1000 || (n > 8 && Math.Abs(sum - length) < tolerance))
                {
                    length = sum;
                    break;
                }
                length = sum;
                n *= 2;
            }
            return length;
        }

        /// <summary>Simpson 方法。</summary>
        private double Simpson(double a, double b)
        {
            double c = (a + b) * 0.5;
            double fa = SpeedAt(a);
            double fb = SpeedAt(b);
            double fc = SpeedAt(c);
            return (b - a) / 6.0 * (fa + 4 * fc + fb);
        }

        /// <summary>SpeedAt 方法。</summary>
        private double SpeedAt(double t)
        {
            var d = FirstDerivative(t);
            return Math.Sqrt(d.X.Value * d.X.Value + d.Y.Value * d.Y.Value);
        }

        #endregion

        #region 从起点沿曲线前进指定距离

        /// <summary>
        /// 从曲线起点出发，沿曲线方向前进指定距离，返回对应点的坐标。
        /// 若距离小于等于 0 则返回起点；若距离大于等于曲线全长则返回终点；
        /// 否则通过弧长二分搜索找到对应点。
        /// </summary>
        /// <param name="distance">要前进的距离。负值视为 0，超出全长视为全长。</param>
        /// <returns>曲线上的对应点。</returns>
        public HPoint PointFromStartAtDistance(double distance)
        {
            double totalLength = ApproximateLength().Value;

            if (distance <= 0)
                return HPointStart.Clone();
            if (distance >= totalLength)
                return HPointEnd.Clone();

            // 二分搜索查找满足弧长 S(t) = distance 的参数 t
            double lo = 0, hi = 1;
            for (int i = 0; i < 50; i++) // 足够迭代次数
            {
                double mid = (lo + hi) * 0.5;
                double lenMid = PartialLength(mid);
                if (Math.Abs(lenMid - distance) < 1e-8)
                    return PointAt(mid);
                if (lenMid < distance)
                    lo = mid;
                else
                    hi = mid;
            }
            double tFinal = (lo + hi) * 0.5;
            return PointAt(tFinal);
        }

        #endregion

        #region de Casteljau 细分与导数控制点

        /// <summary>DeCasteljau 方法。</summary>
        private static HPoint DeCasteljau(List<HPoint> pts, double t)
        {
            int n = pts.Count;
            if (n == 1) return pts[0];
            var temp = new List<HPoint>(n);
            for (int i = 0; i < n; i++) temp.Add(pts[i].Clone());
            for (int k = 1; k < n; k++)
            {
                for (int i = 0; i < n - k; i++)
                {
                    double x = (1 - t) * temp[i].X.Value + t * temp[i + 1].X.Value;
                    double y = (1 - t) * temp[i].Y.Value + t * temp[i + 1].Y.Value;
                    temp[i] = new HPoint(x, y);
                }
            }
            return temp[0];
        }

        /// <summary>获取 derivativeControlPoints。</summary>
        private static List<HPoint> GetDerivativeControlPoints(List<HPoint> pts)
        {
            if (pts.Count < 2) return new List<HPoint>();
            int n = pts.Count - 1;
            var deriv = new List<HPoint>(n);
            for (int i = 0; i < n; i++)
            {
                deriv.Add(new HPoint(
                    n * (pts[i + 1].X.Value - pts[i].X.Value),
                    n * (pts[i + 1].Y.Value - pts[i].Y.Value)));
            }
            return deriv;
        }

        /// <summary>在参数 t 处将曲线分割为两条子曲线。</summary>
        public (HBezier Left, HBezier Right) SplitAt(double t)
        {
            var leftPts = new List<HPoint>();
            var rightPts = new List<HPoint>();
            var temp = new List<HPoint>(ControlPoints.Count);
            foreach (var p in ControlPoints) temp.Add(p.Clone());

            leftPts.Add(temp[0].Clone());
            rightPts.Insert(0, temp[temp.Count - 1].Clone());

            for (int k = 1; k < ControlPoints.Count; k++)
            {
                for (int i = 0; i < ControlPoints.Count - k; i++)
                {
                    temp[i].X = (1 - t) * temp[i].X.Value + t * temp[i + 1].X.Value;
                    temp[i].Y = (1 - t) * temp[i].Y.Value + t * temp[i + 1].Y.Value;
                }
                leftPts.Add(temp[0].Clone());
                rightPts.Insert(0, temp[ControlPoints.Count - k - 1].Clone());
            }

            return (new HBezier(leftPts), new HBezier(rightPts));
        }

        #endregion

        #region 升阶 / 降阶

        /// <summary>升阶，新增一个控制点（形状不变）。</summary>
        public HBezier ElevateDegree()
        {
            var pts = ControlPoints;
            int n = pts.Count;
            var newPts = new List<HPoint>(n + 1) { pts[0].Clone() };
            for (int i = 1; i < n; i++)
            {
                double alpha = (double)i / n;
                double x = (1 - alpha) * pts[i].X.Value + alpha * pts[i - 1].X.Value;
                double y = (1 - alpha) * pts[i].Y.Value + alpha * pts[i - 1].Y.Value;
                newPts.Add(new HPoint(x, y));
            }
            newPts.Add(pts[n - 1].Clone());
            return new HBezier(newPts);
        }

        /// <summary>降阶（近似），控制点数减一。</summary>
        public HBezier ReduceDegree()
        {
            if (ControlPoints.Count <= 2)
                throw new InvalidOperationException(HTranslation.GetContent("不能降阶到少于2个控制点。"));
            var pts = ControlPoints;
            int n = pts.Count - 1;
            var newPts = new List<HPoint>(n) { pts[0].Clone() };
            for (int i = 1; i < n; i++)
            {
                double alpha = (double)i / n;
                double x = (pts[i].X.Value - alpha * pts[i - 1].X.Value) / (1 - alpha);
                double y = (pts[i].Y.Value - alpha * pts[i - 1].Y.Value) / (1 - alpha);
                newPts.Add(new HPoint(x, y));
            }
            newPts.Add(pts[pts.Count - 1].Clone());
            return new HBezier(newPts);
        }

        #endregion

        #region 包围盒

        /// <summary>轴对齐包围盒（基于极值点近似）。</summary>
        public (double MinX, double MaxX, double MinY, double MaxY) BoundingBox()
        {
            var extrema = GetExtremaParameters();
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            void Evaluate(double t)
            {
                var pt = PointAt(t);
                if (pt.X.Value < minX) minX = pt.X.Value;
                if (pt.X.Value > maxX) maxX = pt.X.Value;
                if (pt.Y.Value < minY) minY = pt.Y.Value;
                if (pt.Y.Value > maxY) maxY = pt.Y.Value;
            }

            Evaluate(0);
            Evaluate(1);
            foreach (double t in extrema) Evaluate(t);

            return (minX, maxX, minY, maxY);
        }

        /// <summary>获取 extremaParameters。</summary>
        private List<double> GetExtremaParameters()
        {
            var result = new List<double>();
            var derivPts = GetDerivativeControlPoints(ControlPoints);
            if (Degree == 3)
            {
                double a = derivPts[0].X.Value, b = derivPts[1].X.Value, c = derivPts[2].X.Value;
                double A = a - 2 * b + c;
                double B = 2 * (b - a);
                double C = a;
                AddValidRoots(SolveQuadratic(A, B, C), result);

                a = derivPts[0].Y.Value; b = derivPts[1].Y.Value; c = derivPts[2].Y.Value;
                A = a - 2 * b + c;
                B = 2 * (b - a);
                C = a;
                AddValidRoots(SolveQuadratic(A, B, C), result);
            }
            else
            {
                for (int i = 1; i <= 20; i++)
                    result.Add(i / 20.0);
            }
            return result.Distinct().OrderBy(t => t).ToList();
        }

        /// <summary>AddValidRoots 方法。</summary>
        private static void AddValidRoots(double[] roots, List<double> result)
        {
            foreach (double r in roots)
                if (r > 0.01 && r < 0.99)
                    result.Add(r);
        }

        /// <summary>SolveQuadratic 方法。</summary>
        private static double[] SolveQuadratic(double a, double b, double c)
        {
            if (Math.Abs(a) < 1e-12)
            {
                if (Math.Abs(b) < 1e-12) return new double[0];
                return new double[] { -c / b };
            }
            double disc = b * b - 4 * a * c;
            if (disc < 0) return new double[0];
            double sqrt = Math.Sqrt(disc);
            return new double[] { (-b - sqrt) / (2 * a), (-b + sqrt) / (2 * a) };
        }

        #endregion

        #region 最近点（牛顿迭代）

        /// <summary>使用牛顿法查找点到曲线的最近参数 t 及距离（HDouble）。</summary>
        /// <param name="point">目标点。</param>
        /// <param name="initialGuess">初始参数猜测值（0~1）。</param>
        /// <param name="maxIter">最大迭代次数。</param>
        /// <returns>(最近参数, 距离)</returns>
        public (double T, HDouble Distance) ClosestPoint(HPoint point, double initialGuess = 0.5, int maxIter = 10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (ControlPoints.Count == 2)
            {
                var line = new HLine(HPointStart, HPointEnd);
                var closest = line.ClosestPointOnSegment(point);
                double dist = point.DistanceTo(closest).Value;
                double t = HPointStart.DistanceTo(closest).Value / HPointStart.DistanceTo(HPointEnd).Value;
                return (Math.Max(0, Math.Min(1, t)), new HDouble { Value = dist });
            }

            double tGuess = Math.Max(0, Math.Min(1, initialGuess));
            for (int i = 0; i < maxIter; i++)
            {
                var p = PointAt(tGuess);
                var d1 = FirstDerivative(tGuess);
                var d2 = SecondDerivative(tGuess);
                double f = (p.X.Value - point.X.Value) * d1.X.Value + (p.Y.Value - point.Y.Value) * d1.Y.Value;
                double df = d1.X.Value * d1.X.Value + d1.Y.Value * d1.Y.Value + (p.X.Value - point.X.Value) * d2.X.Value + (p.Y.Value - point.Y.Value) * d2.Y.Value;
                if (Math.Abs(df) < 1e-15) break;
                double tNew = tGuess - f / df;
                tNew = Math.Max(0, Math.Min(1, tNew));
                if (Math.Abs(tNew - tGuess) < 1e-8) break;
                tGuess = tNew;
            }
            var closestPt = PointAt(tGuess);
            return (tGuess, new HDouble { Value = point.DistanceTo(closestPt).Value });
        }

        #endregion

        #region 与直线相交

        /// <summary>返回曲线与无限直线的所有交点参数 t（递归细分法）。</summary>
        public List<double> IntersectionsWithLine(HLine line, double tolerance = 1e-6)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var result = new List<double>();
            if (ControlPoints.Count == 2)
            {
                var seg = new HLine(HPointStart, HPointEnd);
                var inter = line.IntersectionWith(seg);
                if (inter != null)
                {
                    double t = seg.HPointStart.DistanceTo(inter).Value / seg.Length.Value;
                    if (t >= -1e-9 && t <= 1 + 1e-9)
                        result.Add(Math.Max(0, Math.Min(1, t)));
                }
                return result;
            }
            FindLineIntersections(0, 1, line, tolerance, result);
            return result.OrderBy(t => t).ToList();
        }

        /// <summary>FindLineIntersections 方法。</summary>
        private void FindLineIntersections(double t0, double t1, HLine line, double tol, List<double> result)
        {
            var p0 = PointAt(t0);
            var p1 = PointAt(t1);
            double d0 = SignedDistanceToLine(p0, line);
            double d1 = SignedDistanceToLine(p1, line);

            if (Math.Abs(d0) < tol) { result.Add(t0); return; }
            if (Math.Abs(d1) < tol) { result.Add(t1); return; }
            if (d0 * d1 > 0) return;

            double tMid = (t0 + t1) * 0.5;
            if (Math.Abs(t1 - t0) < tol)
            {
                result.Add(tMid);
                return;
            }
            var pMid = PointAt(tMid);
            double dMid = SignedDistanceToLine(pMid, line);

            if (Math.Abs(dMid) < tol) { result.Add(tMid); return; }
            if (d0 * dMid <= 0) FindLineIntersections(t0, tMid, line, tol, result);
            if (dMid * d1 <= 0) FindLineIntersections(tMid, t1, line, tol, result);
        }

        /// <summary>SignedDistanceToLine 方法。</summary>
        private static double SignedDistanceToLine(HPoint p, HLine line)
        {
            double dx = line.HPointEnd.X.Value - line.HPointStart.X.Value;
            double dy = line.HPointEnd.Y.Value - line.HPointStart.Y.Value;
            return (dx * (p.Y.Value - line.HPointStart.Y.Value) - dy * (p.X.Value - line.HPointStart.X.Value)) /
                   Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion

        #region 多边形逼近与工具

        /// <summary>将曲线转换为指定段数的折线。</summary>
        public List<HPoint> ToPolyline(int segments = 64)
        {
            if (segments < 2) segments = 2;
            var poly = new List<HPoint>(segments + 1);
            double step = 1.0 / segments;
            for (int i = 0; i <= segments; i++)
                poly.Add(PointAt(i * step));
            return poly;
        }

        /// <summary>深拷贝。</summary>
        public HBezier Clone() => new HBezier(ControlPoints.Select(p => p.Clone()));

        public override string ToString() =>
            $"Bezier (Degree {Degree}, {ControlPoints.Count} pts)";

        #endregion
    }
}
