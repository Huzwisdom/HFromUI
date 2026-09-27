using System;
using System.Collections.Generic;
using System.Linq;

namespace HFromUI.HMath
{
    /// <summary>
    /// 二维点集数学工具。
    /// 提供基于 <see cref="HPoint"/> 列表的统计、拟合、几何变换与轮廓分析，
    /// 所有公开的数值结果统一包装为 <see cref="HDouble"/>，以安全处理 NaN/±∞。
    /// </summary>
    /// <remarks>
    /// 点集通过 <see cref="Points"/> 属性暴露，可随意增删改。
    /// 所有计算均基于当前点集的实时状态，无需手动更新。
    /// </remarks>
    public class HPoints
    {
        private readonly List<HPoint> _points;

        #region 构造函数与基础属性

        /// <summary>
        /// 初始化一个空的点集。
        /// </summary>
        public HPoints()
        {
            _points = new List<HPoint>();
        }

        /// <summary>
        /// 使用指定的点序列初始化点集。
        /// </summary>
        /// <param name="points">初始点集合，可以为 null（此时视为空集）。</param>
        public HPoints(IEnumerable<HPoint> points)
        {
            _points = points?.ToList() ?? new List<HPoint>();
        }

        /// <summary>
        /// 获取内部点列表的引用。可对此列表直接进行添加、删除或修改操作，
        /// 所有几何计算都将自动反映当前数据。
        /// </summary>
        public List<HPoint> Points => _points;

        /// <summary>
        /// 获取点集当前包含的点数量。
        /// </summary>
        public int Count => _points.Count;

        /// <summary>
        /// 获取点集的质心（所有点的算术平均位置）。
        /// 若点集为空，返回 (0, 0)。
        /// </summary>
        public HPoint Centroid
        {
            get
            {
                if (_points.Count == 0) return new HPoint(0, 0);
                double avgX = _points.Average(p => p.X.Value);
                double avgY = _points.Average(p => p.Y.Value);
                return new HPoint(avgX, avgY);
            }
        }

        /// <summary>
        /// 获取点集的轴对齐边界框。
        /// </summary>
        /// <returns>返回 (minX, maxX, minY, maxY)。点集为空时返回 (0,0,0,0)。</returns>
        public (double minX, double maxX, double minY, double maxY) Bounds
        {
            get
            {
                if (_points.Count == 0) return (0, 0, 0, 0);
                double minX = double.MaxValue, maxX = double.MinValue;
                double minY = double.MaxValue, maxY = double.MinValue;
                foreach (var p in _points)
                {
                    if (p.X.Value < minX) minX = p.X.Value;
                    if (p.X.Value > maxX) maxX = p.X.Value;
                    if (p.Y.Value < minY) minY = p.Y.Value;
                    if (p.Y.Value > maxY) maxY = p.Y.Value;
                }
                return (minX, maxX, minY, maxY);
            }
        }

        #endregion

        #region 最小二乘直线拟合

        /// <summary>
        /// 快速判断当前点集是否满足最小二乘直线拟合的基本条件：
        /// 至少 2 个点，且 X 坐标不全相等。
        /// </summary>
        public bool IsValidFit
        {
            get
            {
                if (_points.Count < 2) return false;
                double sumX = 0, sumX2 = 0;
                foreach (var p in _points)
                {
                    sumX += p.X.Value;
                    sumX2 += p.X.Value * p.X.Value;
                }
                // 方差分母不为零
                return Math.Abs(_points.Count * sumX2 - sumX * sumX) > 1e-15;
            }
        }

        /// <summary>
        /// 计算最小二乘法拟合直线 y = K·x + B 的斜率 K 和截距 B。
        /// 内部仅需遍历点集一次，高效获取两个参数。
        /// </summary>
        /// <returns>
        /// 返回 (<see cref="HDouble"/> K, <see cref="HDouble"/> B)。
        /// 如果点数不足 2 或 X 坐标几乎相同（垂直拟合），
        /// 两个分量的 Value 均为 <see cref="double.NaN"/>，且 <see cref="HDouble.IsValid"/> 为 false。
        /// </returns>
        public (HDouble K, HDouble B) GetEquation()
        {
            int n = _points.Count;
            if (n < 2)
                return (new HDouble { Value = double.NaN }, new HDouble { Value = double.NaN });

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            foreach (var p in _points)
            {
                sumX += p.X.Value;
                sumY += p.Y.Value;
                sumXY += p.X.Value * p.Y.Value;
                sumX2 += p.X.Value * p.X.Value;
            }

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < 1e-15)
                return (new HDouble { Value = double.NaN }, new HDouble { Value = double.NaN });

            double k = (n * sumXY - sumX * sumY) / denominator;
            double b = (sumY - k * sumX) / n;
            return (new HDouble { Value = k }, new HDouble { Value = b });
        }

        /// <summary>
        /// 基于当前点集生成一条覆盖数据 X 范围的拟合直线段。
        /// 如果拟合无效，返回退化直线（起点终点均为 (0,0)）。
        /// </summary>
        /// <returns>返回一个 <see cref="HLine"/> 对象。</returns>
        public HLine FitLine()
        {
            var (k, b) = GetEquation();
            if (!k.IsValid || !b.IsValid)
                return new HLine(new HPoint(0, 0), new HPoint(0, 0));

            double minX = _points.Min(p => p.X.Value);
            double maxX = _points.Max(p => p.X.Value);
            return new HLine(
                new HPoint(minX, k.Value * minX + b.Value),
                new HPoint(maxX, k.Value * maxX + b.Value));
        }

        /// <summary>
        /// 获取所有点到拟合直线在 Y 方向上的残差列表。
        /// 残差 = Y_i - (K·X_i + B)。
        /// 每个残差均为 <see cref="HDouble"/> 类型。
        /// </summary>
        public List<HDouble> Residuals
        {
            get
            {
                var (k, b) = GetEquation();
                return _points.Select(p => new HDouble { Value = p.Y.Value - (k.Value * p.X.Value + b.Value) }).ToList();
            }
        }

        /// <summary>
        /// 获取拟合的决定系数 R²（<see cref="HDouble"/> 类型）。
        /// R² 越接近 1 表示拟合越优。若拟合无效，Value 为 NaN。
        /// </summary>
        public HDouble RSquared
        {
            get
            {
                if (_points.Count < 2) return new HDouble { Value = double.NaN };
                var (k, b) = GetEquation();
                if (!k.IsValid) return new HDouble { Value = double.NaN };

                double meanY = _points.Average(p => p.Y.Value);
                double ssTot = _points.Sum(p => (p.Y.Value - meanY) * (p.Y.Value - meanY));
                double ssRes = _points.Sum(p =>
                {
                    double predicted = k.Value * p.X.Value + b.Value;
                    return (p.Y.Value - predicted) * (p.Y.Value - predicted);
                });
                return new HDouble { Value = 1.0 - ssRes / ssTot };
            }
        }

        #endregion

        #region 距离与离散度

        /// <summary>
        /// 计算所有点到指定目标点的平均欧氏距离。
        /// </summary>
        /// <param name="target">目标点，不可为 null。</param>
        /// <returns>平均距离（<see cref="HDouble"/>）。点集为空时返回 0。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null。</exception>
        public HDouble AverageDistanceTo(HPoint target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (_points.Count == 0) return new HDouble { Value = 0 };

            double avg = _points.Average(p =>
                Math.Sqrt((p.X.Value - target.X.Value) * (p.X.Value - target.X.Value) + (p.Y.Value - target.Y.Value) * (p.Y.Value - target.Y.Value)));
            return new HDouble { Value = avg };
        }

        /// <summary>
        /// 获取点集 X 坐标和 Y 坐标的样本方差（除以 n-1）。
        /// </summary>
        /// <returns>
        /// 返回 (varianceX, varianceY)，均为 <see cref="HDouble"/>。
        /// 点数不足 2 时返回 (0, 0)。
        /// </returns>
        public (HDouble varianceX, HDouble varianceY) Variance
        {
            get
            {
                if (_points.Count < 2) return (new HDouble { Value = 0 }, new HDouble { Value = 0 });
                double meanX = _points.Average(p => p.X.Value);
                double meanY = _points.Average(p => p.Y.Value);
                double sumX2 = _points.Sum(p => (p.X.Value - meanX) * (p.X.Value - meanX));
                double sumY2 = _points.Sum(p => (p.Y.Value - meanY) * (p.Y.Value - meanY));
                int n = _points.Count;
                return (new HDouble { Value = sumX2 / (n - 1) },
                        new HDouble { Value = sumY2 / (n - 1) });
            }
        }

        #endregion

        #region 凸包 (Monotone Chain 算法)

        /// <summary>
        /// 使用 Monotone Chain 算法计算点集的凸包。
        /// </summary>
        /// <returns>
        /// 返回凸包上的点列表（顺时针顺序）。点数不足 3 时直接返回原始点集的副本。
        /// </returns>
        public List<HPoint> ConvexHull()
        {
            if (_points.Count < 3) return new List<HPoint>(_points);

            // 叉积辅助函数（C# 7.3 需定义为普通局部函数）
            double Cross(HPoint o, HPoint a, HPoint b)
            {
                return (a.X.Value - o.X.Value) * (b.Y.Value - o.Y.Value) - (a.Y.Value - o.Y.Value) * (b.X.Value - o.X.Value);
            }

            var sorted = _points.OrderBy(p => p.X.Value).ThenBy(p => p.Y.Value).ToList();
            var hull = new List<HPoint>();

            // 下凸壳
            foreach (var p in sorted)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }

            // 上凸壳
            int lowerCount = hull.Count;
            for (int i = sorted.Count - 2; i >= 0; i--)
            {
                var p = sorted[i];
                while (hull.Count > lowerCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }

            hull.RemoveAt(hull.Count - 1); // 移除重复的起点
            return hull;
        }

        #endregion

        #region 几何变换

        /// <summary>
        /// 将所有点平移 (dx, dy)。
        /// </summary>
        /// <param name="dx">X 方向位移。</param>
        /// <param name="dy">Y 方向位移。</param>
        public void Translate(double dx, double dy)
        {
            foreach (var p in _points) { p.X += dx; p.Y += dy; }
        }

        /// <summary>
        /// 以指定中心为基准缩放所有点。
        /// </summary>
        /// <param name="sx">X 方向缩放因子。</param>
        /// <param name="sy">Y 方向缩放因子。</param>
        /// <param name="center">缩放中心，若为 null 则使用质心。</param>
        public void Scale(double sx, double sy, HPoint center = null)
        {
            center = center ?? Centroid;
            foreach (var p in _points)
            {
                p.X = center.X.Value + (p.X.Value - center.X.Value) * sx;
                p.Y = center.Y.Value + (p.Y.Value - center.Y.Value) * sy;
            }
        }

        /// <summary>
        /// 围绕指定中心旋转所有点。
        /// </summary>
        /// <param name="angleRad">旋转弧度（逆时针为正）。</param>
        /// <param name="center">旋转中心，若为 null 则使用质心。</param>
        public void Rotate(double angleRad, HPoint center = null)
        {
            center = center ?? Centroid;
            double cos = Math.Cos(angleRad), sin = Math.Sin(angleRad);
            foreach (var p in _points)
            {
                double dx = p.X.Value - center.X.Value, dy = p.Y.Value - center.Y.Value;
                p.X = center.X.Value + dx * cos - dy * sin;
                p.Y = center.Y.Value + dx * sin + dy * cos;
            }
        }

        #endregion

        #region 最小外接圆

        /// <summary>
        /// 迭代计算近似最小外接圆。
        /// </summary>
        /// <param name="iterations">迭代次数，默认 20。</param>
        /// <returns>返回圆心 (<see cref="HPoint"/>) 和半径 (<see cref="HDouble"/>)。点集为空时返回 (0,0) 和半径 0。</returns>
        public (HPoint center, HDouble radius) MinEnclosingCircle(int iterations = 20)
        {
            if (_points.Count == 0) return (new HPoint(0, 0), new HDouble { Value = 0 });
            if (_points.Count == 1) return (_points[0].Clone(), new HDouble { Value = 0 });

            HPoint center = Centroid;
            for (int iter = 0; iter < iterations; iter++)
            {
                HPoint farthest = _points[0];
                double maxDistSq = 0;
                foreach (var p in _points)
                {
                    double dx = p.X.Value - center.X.Value, dy = p.Y.Value - center.Y.Value;
                    double distSq = dx * dx + dy * dy;
                    if (distSq > maxDistSq) { maxDistSq = distSq; farthest = p; }
                }
                // 圆心向最远点移动 10%
                center.X += (farthest.X.Value - center.X.Value) * 0.1;
                center.Y += (farthest.Y.Value - center.Y.Value) * 0.1;
            }

            double finalRadius = Math.Sqrt(
                _points.Max(p => (p.X.Value - center.X.Value) * (p.X.Value - center.X.Value) + (p.Y.Value - center.Y.Value) * (p.Y.Value - center.Y.Value)));
            return (center, new HDouble { Value = finalRadius });
        }

        #endregion

        #region 最小面积外接矩形（旋转卡壳）

        // 向量点积和叉积辅助方法
        private static double Dot(double x1, double y1, double x2, double y2) => x1 * x2 + y1 * y2;
        /// <summary>叉积。</summary>
        private static double Cross(double x1, double y1, double x2, double y2) => x1 * y2 - y1 * x2;

        /// <summary>
        /// 计算凸包的最小面积外接矩形面积（旋转卡壳法）。
        /// </summary>
        /// <returns>最小面积（<see cref="HDouble"/>）。凸包点数不足 3 时返回 0。</returns>
        public HDouble MinAreaRect()
        {
            var hull = ConvexHull();
            if (hull.Count < 3) return new HDouble { Value = 0 };

            hull.Reverse(); // 转为逆时针
            int n = hull.Count;
            double minArea = double.MaxValue;
            int right = 1, top = 1;

            for (int i = 0; i < n; i++)
            {
                int next = (i + 1) % n;
                double edgeX = hull[next].X.Value - hull[i].X.Value;
                double edgeY = hull[next].Y.Value - hull[i].Y.Value;

                // 推进最右支持点
                while (Dot(edgeX, edgeY, hull[(right + 1) % n].X.Value - hull[i].X.Value, hull[(right + 1) % n].Y.Value - hull[i].Y.Value) >
                       Dot(edgeX, edgeY, hull[right].X.Value - hull[i].X.Value, hull[right].Y.Value - hull[i].Y.Value))
                    right = (right + 1) % n;

                // 推进最上支持点（垂直方向投影）
                while (Cross(edgeX, edgeY, hull[(top + 1) % n].X.Value - hull[i].X.Value, hull[(top + 1) % n].Y.Value - hull[i].Y.Value) >
                       Cross(edgeX, edgeY, hull[top].X.Value - hull[i].X.Value, hull[top].Y.Value - hull[i].Y.Value))
                    top = (top + 1) % n;

                double width = Dot(edgeX, edgeY, hull[right].X.Value - hull[i].X.Value, hull[right].Y.Value - hull[i].Y.Value)
                               / Math.Sqrt(edgeX * edgeX + edgeY * edgeY);
                double height = Cross(edgeX, edgeY, hull[top].X.Value - hull[i].X.Value, hull[top].Y.Value - hull[i].Y.Value)
                                / Math.Sqrt(edgeX * edgeX + edgeY * edgeY);
                double area = width * height;
                if (area < minArea) minArea = area;
            }
            return new HDouble { Value = minArea };
        }

        #endregion

        #region 最小二乘圆拟合

        /// <summary>
        /// 使用代数最小二乘法拟合圆（要求至少 3 个点）。
        /// </summary>
        /// <returns>
        /// 返回圆心 (<see cref="HPoint"/>) 和半径 (<see cref="HDouble"/>)。
        /// 点数不足 3 或拟合失败时返回圆心 (0,0)、半径 0。
        /// </returns>
        public (HPoint center, HDouble radius) FitCircle()
        {
            if (_points.Count < 3) return (new HPoint(0, 0), new HDouble { Value = 0 });

            double n = _points.Count;
            double sumX = 0, sumY = 0, sumX2 = 0, sumY2 = 0, sumX3 = 0, sumY3 = 0,
                   sumXY = 0, sumX2Y = 0, sumXY2 = 0;

            foreach (var p in _points)
            {
                double x = p.X.Value, y = p.Y.Value;
                double x2 = x * x, y2 = y * y;
                sumX += x; sumY += y;
                sumX2 += x2; sumY2 += y2;
                sumX3 += x2 * x; sumY3 += y2 * y;
                sumXY += x * y;
                sumX2Y += x2 * y;
                sumXY2 += x * y2;
            }

            double a = n * sumX2 - sumX * sumX;
            double b = n * sumXY - sumX * sumY;
            double c = n * sumY2 - sumY * sumY;
            double d = 0.5 * (n * sumX2Y - sumX2 * sumY + n * sumX3 - sumX * sumX2);
            double e = 0.5 * (n * sumXY2 - sumY2 * sumX + n * sumY3 - sumY * sumY2);

            double denominator = a * c - b * b;
            if (Math.Abs(denominator) < 1e-15)
                return (new HPoint(0, 0), new HDouble { Value = 0 });

            double cx = (d * c - e * b) / denominator;
            double cy = (e * a - d * b) / denominator;
            double r = Math.Sqrt((sumX2 - 2 * cx * sumX + n * cx * cx +
                                  sumY2 - 2 * cy * sumY + n * cy * cy) / n);

            return (new HPoint(cx, cy), new HDouble { Value = r });
        }

        #endregion

        #region 滤波与重采样

        /// <summary>
        /// 移除离群点：距离质心超过 (thresholdFactor × 标准差) 的点被删除。
        /// </summary>
        /// <param name="thresholdFactor">标准差倍数，默认 3.0。</param>
        public void RemoveOutliers(double thresholdFactor = 3.0)
        {
            if (_points.Count < 2) return;

            var center = Centroid;
            var (varX, varY) = Variance;
            double std = Math.Sqrt(varX.Value + varY.Value);
            double threshold = std * thresholdFactor;

            _points.RemoveAll(p =>
            {
                double dx = p.X.Value - center.X.Value, dy = p.Y.Value - center.Y.Value;
                return Math.Sqrt(dx * dx + dy * dy) > threshold;
            });
        }

        /// <summary>
        /// 使用 Douglas-Peucker 算法简化点集（通常用于折线简化）。
        /// 注意：点集应已按路径顺序排列；若顺序不明确，可先按 X 排序。
        /// </summary>
        /// <param name="epsilon">简化容差，值越大保留的点越少。</param>
        /// <returns>简化后的点列表。</returns>
        public List<HPoint> Simplify(double epsilon)
        {
            if (_points.Count < 3) return new List<HPoint>(_points);
            return DouglasPeucker(_points, epsilon);
        }

        /// <summary>DouglasPeucker 方法。</summary>
        private static List<HPoint> DouglasPeucker(List<HPoint> points, double epsilon)
        {
            if (points.Count < 2) return new List<HPoint>(points);

            double dmax = 0;
            int index = 0, end = points.Count - 1;
            var line = new HLine(points[0], points[end]);

            for (int i = 1; i < end; i++)
            {
                double d = line.DistanceToPoint(points[i]).Value;
                if (d > dmax) { dmax = d; index = i; }
            }

            if (dmax > epsilon)
            {
                var left = DouglasPeucker(points.Take(index + 1).ToList(), epsilon);
                var right = DouglasPeucker(points.Skip(index).ToList(), epsilon);
                left.RemoveAt(left.Count - 1); // 移除重复的连接点
                left.AddRange(right);
                return left;
            }
            return new List<HPoint> { points[0], points[end] };
        }

        #endregion

        #region 线性插值

        /// <summary>
        /// 沿 X 方向对点集进行线性插值，生成指定数量的等间距点。
        /// 点集内部会自动按 X 升序排序。
        /// </summary>
        /// <param name="totalPoints">生成的总点数，至少为 2。</param>
        /// <returns>插值后的点列表。点数不足 2 或 totalPoints 小于 2 时返回原始点集副本。</returns>
        public List<HPoint> InterpolateLinear(int totalPoints)
        {
            if (_points.Count < 2 || totalPoints < 2) return new List<HPoint>(_points);

            var sorted = _points.OrderBy(p => p.X.Value).ToList();
            var result = new List<HPoint>(totalPoints);
            double step = (sorted[sorted.Count - 1].X.Value - sorted[0].X.Value) / (totalPoints - 1);
            int seg = 0;

            for (int i = 0; i < totalPoints; i++)
            {
                double x = sorted[0].X.Value + step * i;
                while (seg < sorted.Count - 2 && x > sorted[seg + 1].X.Value) seg++;
                double t = (x - sorted[seg].X.Value) / (sorted[seg + 1].X.Value - sorted[seg].X.Value);
                double y = sorted[seg].Y.Value + t * (sorted[seg + 1].Y.Value - sorted[seg].Y.Value);
                result.Add(new HPoint(x, y));
            }
            return result;
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 创建当前点集的深拷贝（每个点都执行 <see cref="HPoint.Clone"/>）。
        /// </summary>
        /// <returns>新的 <see cref="HMath.HPoints"/> 实例，包含相同坐标的点。</returns>
        public HPoints Clone()
        {
            return new HPoints(_points.Select(p => p.Clone()));
        }

        #endregion
    }
}
