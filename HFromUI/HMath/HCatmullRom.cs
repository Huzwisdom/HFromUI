using System;
using System.Collections.Generic;
using System.Linq;
using HFromUI;               // 仅需 LineBase 和 HPoint
using HFromUI.HMath;         // 使用 HDouble
using HFromUI.HBase;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    /// <summary>
    /// Catmull‑Rom 样条曲线，通过所有控制点，保持 C1 连续。
    /// 支持 Uniform、Centripetal、Chordal 三种参数化，可调张力系数 α。
    /// 仅继承 <see cref="HLineBase"/>，内部提供完整的样条运算：求值、导数、曲率、
    /// 弧长、弧长参数化、控制点编辑、最近点、直线相交、包围盒、多边形生成等。
    /// 所有标量结果均使用 <see cref="HDouble"/> 包装。
    /// </summary>
    public class HCatmullRom : HLineBase
    {
        #region 参数化类型枚举

        /// <summary>Catmull‑Rom 样条参数化方式。</summary>
        public enum ParameterizationType
        {
            /// <summary>均匀参数化 (α = 0)，节点间隔相等。</summary>
            Uniform,
            /// <summary>向心参数化 (α = 0.5)，通常可避免尖点和自交。</summary>
            Centripetal,
            /// <summary>弦长参数化 (α = 1.0)，接近弧长参数化。</summary>
            Chordal
        }

        #endregion

        #region 字段

        private readonly List<HPoint> _controlPoints;   // 用户可见控制点（不含虚拟点）
        private List<HPoint> _fullPoints;               // 包含虚拟头尾的完整点列（长度 = n+2）
        private List<double> _knots;                    // 累积弦长参数，用于映射全局 t → (段, 局部 t)
        private double _totalKnotLength;                // 总节点长度
        private readonly double _alpha;                 // 张力系数（通常 0.5）

        #endregion

        #region 属性

        /// <summary>用户原始控制点列表（不含自动添加的虚拟边界点）。</summary>
        public List<HPoint> ControlPoints => _controlPoints;

        /// <summary>曲线包含的段数 = 控制点数 - 1。</summary>
        public int SegmentCount => _controlPoints.Count - 1;

        /// <summary>起点（第一个控制点）。</summary>
        public override HPoint HPointStart
        {
            get => _controlPoints[0];
            set => UpdateControlPoint(0, value);
        }

        /// <summary>终点（最后一个控制点）。</summary>
        public override HPoint HPointEnd
        {
            get => _controlPoints[_controlPoints.Count - 1];
            set => UpdateControlPoint(_controlPoints.Count - 1, value);
        }

        /// <summary>当前使用的参数化类型。</summary>
        public ParameterizationType ParamType { get; }

        /// <summary>张力系数 α（标准 Catmull‑Rom 为 0.5）。</summary>
        public double Alpha => _alpha;

        #endregion

        #region 构造函数

        /// <summary>
        /// 使用控制点序列创建样条。默认采用 Centripetal 参数化，α = 0.5。
        /// </summary>
        /// <param name="controlPoints">至少 2 个控制点。</param>
        /// <exception cref="ArgumentNullException"><paramref name="controlPoints"/> 为 null。</exception>
        /// <exception cref="ArgumentException">控制点数量不足 2。</exception>
        public HCatmullRom(IEnumerable<HPoint> controlPoints)
            : this(controlPoints, ParameterizationType.Centripetal, 0.5) { }

        /// <summary>
        /// 使用控制点、指定参数化类型和张力系数创建样条。
        /// </summary>
        /// <param name="controlPoints">至少 2 个控制点。</param>
        /// <param name="paramType">参数化类型。</param>
        /// <param name="alpha">张力系数，通常 0.5。</param>
        public HCatmullRom(IEnumerable<HPoint> controlPoints, ParameterizationType paramType, double alpha = 0.5)
        {
            if (controlPoints == null) throw new ArgumentNullException(nameof(controlPoints));
            var pts = controlPoints.ToList();
            if (pts.Count < 2) throw new ArgumentException(HTranslation.GetContent("至少需要 2 个控制点。"));

            ParamType = paramType;
            _alpha = alpha;
            _controlPoints = pts;

            BuildInternalData();
        }

        #endregion

        #region 内部数据构建与重建

        /// <summary>根据当前控制点和参数化设置重建内部数据结构（fullPoints、knots）。</summary>
        private void BuildInternalData()
        {
            // 自然边界：虚拟起点 = 2*P0 - P1；虚拟终点 = 2*P_{n-1} - P_{n-2}
            var first = _controlPoints[0];
            var second = _controlPoints[1];
            var last = _controlPoints[_controlPoints.Count - 1];
            var secondLast = _controlPoints[_controlPoints.Count - 2];

            HPoint virtualStart = new HPoint(2 * first.X.Value - second.X.Value, 2 * first.Y.Value - second.Y.Value);
            HPoint virtualEnd = new HPoint(2 * last.X.Value - secondLast.X.Value, 2 * last.Y.Value - secondLast.Y.Value);

            _fullPoints = new List<HPoint>(_controlPoints.Count + 2)
            {
                virtualStart
            };
            _fullPoints.AddRange(_controlPoints);
            _fullPoints.Add(virtualEnd);

            // 节点向量构建
            _knots = new List<double> { 0.0 };
            double alphaExp = 0.0;
            switch (ParamType)
            {
                case ParameterizationType.Uniform: alphaExp = 0.0; break;
                case ParameterizationType.Centripetal: alphaExp = 0.5; break;
                case ParameterizationType.Chordal: alphaExp = 1.0; break;
            }
            for (int i = 1; i < _fullPoints.Count - 2; i++) // 从第1段到第 n-1 段（总共 n-1 段，knots 需要 n 个值）
            {
                double dx = _fullPoints[i + 1].X.Value - _fullPoints[i].X.Value;
                double dy = _fullPoints[i + 1].Y.Value - _fullPoints[i].Y.Value;
                double dist = Math.Sqrt(dx * dx + dy * dy);
                _knots.Add(_knots[i - 1] + Math.Pow(dist, alphaExp));
            }
            _totalKnotLength = _knots[_knots.Count - 1];
        }

        /// <summary>更新单个控制点并重建内部数据。</summary>
        private void UpdateControlPoint(int index, HPoint value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _controlPoints[index] = value;
            BuildInternalData();
        }

        #endregion

        #region 参数映射

        /// <summary>将全局参数 t∈[0,1] 映射为 (段索引 seg, 段内局部参数 localT∈[0,1])。</summary>
        private (int seg, double localT) MapToSegment(double t)
        {
            if (t <= 0) return (0, 0.0);
            if (t >= 1) return (SegmentCount - 1, 1.0);

            double target = t * _totalKnotLength;
            int seg = 0;
            // knots 共有 SegmentCount+1 个值（0..n），对应 fullPoints 中段 0..n-1 的起点
            for (int i = 1; i < _knots.Count; i++)
            {
                if (target <= _knots[i])
                {
                    seg = i - 1;
                    break;
                }
                if (i == _knots.Count - 1)
                    seg = i - 1;
            }
            double segStart = _knots[seg];
            double segEnd = _knots[seg + 1];
            double localT = (target - segStart) / (segEnd - segStart);
            if (localT < 0) localT = 0;
            if (localT > 1) localT = 1;
            return (seg, localT);
        }

        /// <summary>将 (段索引, 段内局部参数) 还原为全局参数 t。</summary>
        private double SegmentToGlobal(int seg, double localT)
        {
            double segStart = _knots[seg];
            double segEnd = _knots[seg + 1];
            double globalKnot = segStart + localT * (segEnd - segStart);
            return globalKnot / _totalKnotLength;
        }

        #endregion

        #region 求值与导数

        /// <summary>计算全局参数 t (0~1) 对应的曲线点。</summary>
        public HPoint PointAt(double t)
        {
            var (seg, localT) = MapToSegment(t);
            return EvaluateSegment(seg, localT);
        }

        /// <summary>计算全局参数 t 处的一阶导数（切线向量）。</summary>
        public HPoint FirstDerivative(double t)
        {
            var (seg, localT) = MapToSegment(t);
            return EvaluateDerivative(seg, localT);
        }

        /// <summary>计算全局参数 t 处的二阶导数。</summary>
        public HPoint SecondDerivative(double t)
        {
            var (seg, localT) = MapToSegment(t);
            return EvaluateSecondDerivative(seg, localT);
        }

        /// <summary>获取 t 处的单位切线向量。零导数时返回 (0,0)。</summary>
        public HPoint TangentAt(double t)
        {
            var d = FirstDerivative(t);
            double len = d.Length.Value;
            if (len < 1e-15) return new HPoint(0, 0);
            return new HPoint(d.X.Value / len, d.Y.Value / len);
        }

        /// <summary>获取 t 处的单位法向量（切线逆时针旋转90°）。</summary>
        public HPoint NormalAt(double t)
        {
            var d = FirstDerivative(t);
            double len = d.Length.Value;
            if (len < 1e-15) return new HPoint(0, 0);
            return new HPoint(-d.Y.Value / len, d.X.Value / len);
        }

        /// <summary>计算参数 t 处的曲率绝对值 (HDouble)。</summary>
        public HDouble CurvatureAt(double t)
        {
            var d1 = FirstDerivative(t);
            var d2 = SecondDerivative(t);
            double cross = d1.X.Value * d2.Y.Value - d1.Y.Value * d2.X.Value;
            double len = d1.Length.Value;
            if (len < 1e-15) return new HDouble { Value = 0 };
            return new HDouble { Value = Math.Abs(cross) / (len * len * len) };
        }

        #region 分段求值核心（基于 Catmull‑Rom 矩阵）

        /// <summary>EvaluateSegment 方法。</summary>
        private HPoint EvaluateSegment(int seg, double t)
        {
            var p0 = _fullPoints[seg];
            var p1 = _fullPoints[seg + 1];
            var p2 = _fullPoints[seg + 2];
            var p3 = _fullPoints[seg + 3];

            double t2 = t * t, t3 = t2 * t;
            double a0 = -0.5 * t3 + t2 - 0.5 * t;
            double a1 = 1.5 * t3 - 2.5 * t2 + 1.0;
            double a2 = -1.5 * t3 + 2.0 * t2 + 0.5 * t;
            double a3 = 0.5 * t3 - 0.5 * t2;

            return new HPoint(
                a0 * p0.X.Value + a1 * p1.X.Value + a2 * p2.X.Value + a3 * p3.X.Value,
                a0 * p0.Y.Value + a1 * p1.Y.Value + a2 * p2.Y.Value + a3 * p3.Y.Value);
        }

        /// <summary>EvaluateDerivative 方法。</summary>
        private HPoint EvaluateDerivative(int seg, double t)
        {
            var p0 = _fullPoints[seg];
            var p1 = _fullPoints[seg + 1];
            var p2 = _fullPoints[seg + 2];
            var p3 = _fullPoints[seg + 3];

            double t2 = t * t;
            double a0 = -1.5 * t2 + 2.0 * t - 0.5;
            double a1 = 4.5 * t2 - 5.0 * t;
            double a2 = -4.5 * t2 + 4.0 * t + 0.5;
            double a3 = 1.5 * t2 - 1.0 * t;

            return new HPoint(
                a0 * p0.X.Value + a1 * p1.X.Value + a2 * p2.X.Value + a3 * p3.X.Value,
                a0 * p0.Y.Value + a1 * p1.Y.Value + a2 * p2.Y.Value + a3 * p3.Y.Value);
        }

        /// <summary>EvaluateSecondDerivative 方法。</summary>
        private HPoint EvaluateSecondDerivative(int seg, double t)
        {
            var p0 = _fullPoints[seg];
            var p1 = _fullPoints[seg + 1];
            var p2 = _fullPoints[seg + 2];
            var p3 = _fullPoints[seg + 3];

            double a0 = -3.0 * t + 2.0;
            double a1 = 9.0 * t - 5.0;
            double a2 = -9.0 * t + 4.0;
            double a3 = 3.0 * t - 1.0;

            return new HPoint(
                a0 * p0.X.Value + a1 * p1.X.Value + a2 * p2.X.Value + a3 * p3.X.Value,
                a0 * p0.Y.Value + a1 * p1.Y.Value + a2 * p2.Y.Value + a3 * p3.Y.Value);
        }

        #endregion

        #endregion

        #region 弧长与弧长参数化

        /// <summary>曲线总长度的近似值（HDouble）。使用分段 Simpson 积分，精度 1e-6。</summary>
        public HDouble Length => ComputeLength();

        /// <summary>ComputeLength 方法。</summary>
        private HDouble ComputeLength(double tolerance = 1e-6)
        {
            double total = 0;
            for (int seg = 0; seg < SegmentCount; seg++)
                total += SimpsonSegment(seg, 0, 1, tolerance);
            return new HDouble { Value = total };
        }

        /// <summary>计算从起点到参数 t 的弧长（全局 t ∈ [0,1]）。</summary>
        public double PartialLength(double t, double tolerance = 1e-6)
        {
            if (t <= 0) return 0;
            if (t >= 1) return Length.Value;

            var (seg, localT) = MapToSegment(t);
            double len = 0;
            for (int i = 0; i < seg; i++)
                len += SimpsonSegment(i, 0, 1, tolerance);
            len += SimpsonSegment(seg, 0, localT, tolerance);
            return len;
        }

        /// <summary>
        /// 从曲线起点出发，沿曲线方向前进指定距离，返回对应点的坐标。
        /// 距离 ≤ 0 返回起点，≥ 总长返回终点，否则通过弧长二分搜索精确定位。
        /// </summary>
        public HPoint PointFromStartAtDistance(double distance)
        {
            double total = Length.Value;
            if (distance <= 0) return PointAt(0);
            if (distance >= total) return PointAt(1);

            double lo = 0, hi = 1;
            for (int i = 0; i < 50; i++)
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
            return PointAt((lo + hi) * 0.5);
        }

        #region Simpson 积分辅助

        /// <summary>SimpsonSegment 方法。</summary>
        private double SimpsonSegment(int seg, double a, double b, double tolerance)
        {
            int n = 16;
            double step = (b - a) / n;
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                double t0 = a + i * step;
                double t1 = t0 + step;
                double mid = (t0 + t1) * 0.5;
                sum += (t1 - t0) / 6.0 * (SegmentSpeed(seg, t0) + 4 * SegmentSpeed(seg, mid) + SegmentSpeed(seg, t1));
            }
            return sum;
        }

        /// <summary>SegmentSpeed 方法。</summary>
        private double SegmentSpeed(int seg, double t)
        {
            var d = EvaluateDerivative(seg, t);
            return Math.Sqrt(d.X.Value * d.X.Value + d.Y.Value * d.Y.Value);
        }

        #endregion

        #endregion

        #region 控制点编辑

        /// <summary>在指定索引处插入一个控制点（会改变曲线形状，但曲线仍通过所有新旧控制点）。</summary>
        public void InsertControlPoint(int index, HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (index < 0 || index > _controlPoints.Count) throw new ArgumentOutOfRangeException(nameof(index));
            _controlPoints.Insert(index, point);
            BuildInternalData();
        }

        /// <summary>移除指定索引的控制点。</summary>
        public void RemoveControlPoint(int index)
        {
            if (_controlPoints.Count <= 2) throw new InvalidOperationException(HTranslation.GetContent("至少需要 2 个控制点。"));
            if (index < 0 || index >= _controlPoints.Count) throw new ArgumentOutOfRangeException(nameof(index));
            _controlPoints.RemoveAt(index);
            BuildInternalData();
        }

        /// <summary>
        /// 在全局参数 t 处将曲线分割为两条独立的样条（实现方式：细分该段并构造新控制点序列）。
        /// </summary>
        public (HCatmullRom Left, HCatmullRom Right) SplitAt(double t)
        {
            var (seg, localT) = MapToSegment(t);
            var splitPt = PointAt(t);

            // 左侧控制点：原控制点中索引 ≤ seg 的部分，再加分割点
            var leftPts = new List<HPoint>(_controlPoints);
            leftPts.RemoveRange(seg + 1, _controlPoints.Count - seg - 1);
            leftPts.Add(splitPt);

            // 右侧控制点：分割点 + 原控制点中索引 ≥ seg+1 的部分
            var rightPts = new List<HPoint>();
            rightPts.Add(splitPt);
            for (int i = seg + 1; i < _controlPoints.Count; i++)
                rightPts.Add(_controlPoints[i]);

            return (new HCatmullRom(leftPts, ParamType, _alpha),
                    new HCatmullRom(rightPts, ParamType, _alpha));
        }

        #endregion

        #region 最近点

        /// <summary>计算点到样条的最短距离（HDouble）。使用采样+牛顿迭代。</summary>
        public HDouble DistanceToPoint(HPoint point, int sampleCount = 20)
        {
            // 多点采样寻找最佳初值
            double bestT = 0, bestDist = double.MaxValue;
            for (int i = 0; i <= sampleCount; i++)
            {
                double t = (double)i / sampleCount;
                double d = point.DistanceTo(PointAt(t)).Value;
                if (d < bestDist) { bestDist = d; bestT = t; }
            }
            // 牛顿迭代求精
            return RefineClosest(point, bestT, 10).Distance;
        }

        /// <summary>获取点到样条最近点的参数 t 及距离（HDouble）。</summary>
        public (double T, HDouble Distance) ClosestPoint(HPoint point)
        {
            return RefineClosest(point, 0.5, 10);
        }

        private (double T, HDouble Distance) RefineClosest(HPoint point, double guess, int maxIter)
        {
            double t = Math.Max(0, Math.Min(1, guess));
            for (int i = 0; i < maxIter; i++)
            {
                var p = PointAt(t);
                var d1 = FirstDerivative(t);
                var d2 = SecondDerivative(t);
                double f = (p.X.Value - point.X.Value) * d1.X.Value + (p.Y.Value - point.Y.Value) * d1.Y.Value;
                double df = d1.X.Value * d1.X.Value + d1.Y.Value * d1.Y.Value + (p.X.Value - point.X.Value) * d2.X.Value + (p.Y.Value - point.Y.Value) * d2.Y.Value;
                if (Math.Abs(df) < 1e-15) break;
                double tNew = t - f / df;
                tNew = Math.Max(0, Math.Min(1, tNew));
                if (Math.Abs(tNew - t) < 1e-8) break;
                t = tNew;
            }
            return (t, new HDouble { Value = point.DistanceTo(PointAt(t)).Value });
        }

        #endregion

        #region 与直线相交

        /// <summary>返回样条与无限直线的所有交点全局参数 t（递归细分法）。</summary>
        public List<double> IntersectionsWithLine(HLine line, double tolerance = 1e-6)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var result = new List<double>();
            for (int seg = 0; seg < SegmentCount; seg++)
                FindLineIntersectionsInSegment(seg, 0, 1, line, tolerance, result);
            return result.OrderBy(t => t).ToList();
        }

        /// <summary>FindLineIntersectionsInSegment 方法。</summary>
        private void FindLineIntersectionsInSegment(int seg, double t0, double t1, HLine line, double tol, List<double> result)
        {
            var p0 = EvaluateSegment(seg, t0);
            var p1 = EvaluateSegment(seg, t1);
            double d0 = SignedDistanceToLine(p0, line);
            double d1 = SignedDistanceToLine(p1, line);

            if (Math.Abs(d0) < tol) { result.Add(SegmentToGlobal(seg, t0)); return; }
            if (Math.Abs(d1) < tol) { result.Add(SegmentToGlobal(seg, t1)); return; }
            if (d0 * d1 > 0) return;

            double tMid = (t0 + t1) * 0.5;
            if (Math.Abs(t1 - t0) < tol)
            {
                result.Add(SegmentToGlobal(seg, tMid));
                return;
            }
            var pMid = EvaluateSegment(seg, tMid);
            double dMid = SignedDistanceToLine(pMid, line);

            if (Math.Abs(dMid) < tol) { result.Add(SegmentToGlobal(seg, tMid)); return; }
            if (d0 * dMid <= 0) FindLineIntersectionsInSegment(seg, t0, tMid, line, tol, result);
            if (dMid * d1 <= 0) FindLineIntersectionsInSegment(seg, tMid, t1, line, tol, result);
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

        #region 包围盒

        /// <summary>计算样条的轴对齐包围盒。</summary>
        public (double MinX, double MaxX, double MinY, double MaxY) BoundingBox()
        {
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            // 采样法，每段取 21 个点（足够近似）
            for (int seg = 0; seg < SegmentCount; seg++)
            {
                for (int j = 0; j <= 20; j++)
                {
                    double t = j / 20.0;
                    var p = EvaluateSegment(seg, t);
                    if (p.X.Value < minX) minX = p.X.Value;
                    if (p.X.Value > maxX) maxX = p.X.Value;
                    if (p.Y.Value < minY) minY = p.Y.Value;
                    if (p.Y.Value > maxY) maxY = p.Y.Value;
                }
            }
            return (minX, maxX, minY, maxY);
        }

        #endregion

        #region 多边形逼近

        /// <summary>将样条转换为指定分段数的折线列表（默认 64 段）。</summary>
        public List<HPoint> ToPolyline(int segments = 64)
        {
            if (segments < 2) segments = 2;
            var poly = new List<HPoint>(segments + 1);
            for (int i = 0; i <= segments; i++)
                poly.Add(PointAt((double)i / segments));
            return poly;
        }

        #endregion

        #region 工具

        /// <summary>深拷贝当前样条。</summary>
        public HCatmullRom Clone() => new HCatmullRom(_controlPoints, ParamType, _alpha);

        /// <summary>格式：CatmullRom (Centripetal, α=0.5, 5 segments)</summary>
        public override string ToString() =>
            $"CatmullRom ({ParamType}, α={_alpha}, {SegmentCount} segments)";

        #endregion
    }
}
