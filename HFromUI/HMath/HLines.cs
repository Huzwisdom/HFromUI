using System;
using System.Collections.Generic;
using System.Linq;
using HFromUI;               // LineBase, HPoint
using HFromUI.HMath;         // HDouble
using HFromUI.HBase;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    /// <summary>
    /// 连续复合路径，由多个 <see cref="HLineBase"/> 子类线段首尾相连组成。
    /// 线段类型支持：<see cref="HLine"/>、<see cref="H3PArc"/>、<see cref="HBezier"/>、<see cref="HCatmullRom"/>。
    /// 提供总长度计算、沿路径按距离取点等核心操作，并自动保证线段间的连续性。
    /// </summary>
    public class HLines
    {
        /// <summary>_segments 字段。</summary>
        private readonly List<HLineBase> _segments = new List<HLineBase>();

        /// <summary>内部线段列表（可枚举，不建议直接修改）。</summary>
        public List<HLineBase> LineBases => _segments;

        /// <summary>路径是否为空。</summary>
        public bool IsEmpty => _segments.Count == 0;

        /// <summary>路径的起点（第一条线段的起点）。</summary>
        public HPoint StartPoint => IsEmpty ? null : _segments[0].HPointStart;

        /// <summary>路径的终点（最后一条线段的终点）。</summary>
        public HPoint EndPoint => IsEmpty ? null : _segments[_segments.Count - 1].HPointEnd;

        /// <summary>路径是否连续（每条线段的终点等于下一条线段的起点）。</summary>
        public bool IsContinuous
        {
            get
            {
                if (_segments.Count < 2) return true;
                for (int i = 0; i < _segments.Count - 1; i++)
                {
                    var end = _segments[i].HPointEnd;
                    var nextStart = _segments[i + 1].HPointStart;
                    if (Math.Abs(end.X.Value - nextStart.X.Value) > 1e-10 ||
                        Math.Abs(end.Y.Value - nextStart.Y.Value) > 1e-10)
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 路径总长度（HDouble 类型）。遍历所有线段并累加。
        /// 空路径返回 0。
        /// </summary>
        public HDouble TotalLength
        {
            get
            {
                double sum = 0;
                foreach (var seg in _segments)
                    sum += GetSegmentLength(seg);
                return new HDouble { Value = sum };
            }
        }

        #region 构造函数

        /// <summary>创建空路径。</summary>
        public HLines() { }

        /// <summary>使用初始线段列表创建路径（不会自动调整连续性，请确保输入已首尾相连）。</summary>
        public HLines(IEnumerable<HLineBase> segments)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            _segments.AddRange(segments);
        }

        #endregion

        #region 添加与编辑（自动保持连续性）

        /// <summary>
        /// 在路径末尾添加一条线段。如果路径非空，会自动将该线段的起点修正为当前路径的终点，
        /// 以保证连续性（即使传入线段的起点不同也会被覆盖）。
        /// </summary>
        /// <param name="segment">要添加的线段，不可为 null。</param>
        public void Add(HLineBase segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            if (_segments.Count > 0)
                segment.HPointStart = _segments[_segments.Count - 1].HPointEnd;
            _segments.Add(segment);
        }

        /// <summary>
        /// 在指定索引处插入一条线段。会自动连接前后线段：
        /// 如果前面有线段，将插入线段的起点设为其终点；
        /// 如果后面有线段，将插入线段的终点设为后一线段的起点（如果线段类型允许）。
        /// </summary>
        /// <param name="index">插入位置索引。</param>
        /// <param name="segment">要插入的线段。</param>
        public void Insert(int index, HLineBase segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            if (index < 0 || index > _segments.Count) throw new ArgumentOutOfRangeException(nameof(index));

            // 连接前一段
            if (index > 0)
                segment.HPointStart = _segments[index - 1].HPointEnd;

            // 连接后一段
            if (index < _segments.Count)
            {
                var next = _segments[index];
                segment.HPointEnd = next.HPointStart;
            }

            _segments.Insert(index, segment);
        }

        /// <summary>移除指定索引处的线段，并连接前后两段（前一段的终点设置为后一段的起点）。</summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _segments.Count) throw new ArgumentOutOfRangeException(nameof(index));
            _segments.RemoveAt(index);
            if (index > 0 && index < _segments.Count)
                _segments[index - 1].HPointEnd = _segments[index].HPointStart;
        }

        /// <summary>清空所有线段。</summary>
        public void Clear() => _segments.Clear();

        #endregion

        #region 核心功能：按距离取点

        /// <summary>
        /// 从路径起点出发，沿路径前进指定距离，返回到达点的坐标。
        /// 若距离 ≤ 0 则返回路径起点；若距离 ≥ 路径总长则返回路径终点；
        /// 否则精确计算所在线段及其上的位置。
        /// </summary>
        /// <param name="distance">要前进的距离。</param>
        /// <returns>路径上对应的点。</returns>
        public HPoint PointAtDistance(double distance)
        {
            if (_segments.Count == 0)
                throw new InvalidOperationException(HTranslation.GetContent("路径中没有线段。"));

            if (distance <= 0)
                return _segments[0].HPointStart.Clone();

            double total = TotalLength.Value;
            if (distance >= total)
                return _segments[_segments.Count - 1].HPointEnd.Clone();

            double remaining = distance;
            foreach (var seg in _segments)
            {
                double segLen = GetSegmentLength(seg);
                if (remaining <= segLen)
                    return GetPointOnSegment(seg, remaining);

                remaining -= segLen;
            }

            // 浮点误差保护：返回终点
            return _segments[_segments.Count - 1].HPointEnd.Clone();
        }

        /// <summary>
        /// 按路径长度比例 t (0~1) 取点。t=0 为起点，t=1 为终点。
        /// </summary>
        /// <param name="t">比例参数，将被限制在 [0,1]。</param>
        public HPoint PointAt(double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return PointAtDistance(t * TotalLength.Value);
        }

        #endregion

        #region 内部辅助：线段长度与点定位

        /// <summary>获取指定线段的长度（double 值）。</summary>
        private static double GetSegmentLength(HLineBase seg)
        {
            if (seg is HLine line) return line.Length.Value;
            if (seg is H3PArc arc) return arc.ArcLength.Value;    // 退化时 ArcLength 返回弦长
            if (seg is HBezier bezier) return bezier.ApproximateLength().Value;
            if (seg is HCatmullRom catmull) return catmull.Length.Value;
            // 未知类型回退为起终点直线距离
            return seg.HPointStart.DistanceTo(seg.HPointEnd).Value;
        }

        /// <summary>
        /// 从指定线段的起点出发，沿线段走 distanceFromStart 距离，返回对应点。
        /// 自动根据线段类型调用合适的弧长参数化方法。
        /// </summary>
        private static HPoint GetPointOnSegment(HLineBase seg, double distanceFromStart)
        {
            // 直线：使用 PointFromStartAtDistance（它处理了长度为零的情况）
            if (seg is HLine line)
                return line.PointFromStartAtDistance(distanceFromStart);

            // 圆弧：基于角度线性插值
            if (seg is H3PArc arc)
            {
                double len = arc.ArcLength.Value;
                if (len < 1e-15) return arc.HPointStart.Clone();

                // 退化圆弧（共线）按直线处理
                if (arc.IsDegenerate)
                {
                    double t = distanceFromStart / len;
                    return new HPoint(
                        arc.HPointStart.X.Value + t * (arc.HPointEnd.X.Value - arc.HPointStart.X.Value),
                        arc.HPointStart.Y.Value + t * (arc.HPointEnd.Y.Value - arc.HPointStart.Y.Value));
                }

                // 正常圆弧：起始角 + 按比例扫角
                double sweep = arc.SweepAngle.Value;
                double startAngle = arc.StartAngle.Value;
                double angle = startAngle + (distanceFromStart / len) * sweep;
                double r = arc.Radius.Value;
                var c = arc.Center;
                return new HPoint(c.X.Value + r * Math.Cos(angle), c.Y.Value + r * Math.Sin(angle));
            }

            // 贝塞尔曲线：调用专用弧长参数化方法
            if (seg is HBezier bezier)
                return bezier.PointFromStartAtDistance(distanceFromStart);

            // Catmull‑Rom 样条：调用专用弧长参数化方法
            if (seg is HCatmullRom catmull)
                return catmull.PointFromStartAtDistance(distanceFromStart);

            // 最终回退：线性插值
            double straightLen = seg.HPointStart.DistanceTo(seg.HPointEnd).Value;
            if (straightLen < 1e-15) return seg.HPointStart.Clone();
            double tFallback = distanceFromStart / straightLen;
            return new HPoint(
                seg.HPointStart.X.Value + tFallback * (seg.HPointEnd.X.Value - seg.HPointStart.X.Value),
                seg.HPointStart.Y.Value + tFallback * (seg.HPointEnd.Y.Value - seg.HPointStart.Y.Value));
        }

        #endregion

        #region 额外功能：反转、连接路径

        /// <summary>反转整个路径的方向。所有线段的起点和终点交换，内部顺序也颠倒。</summary>
        public void Reverse()
        {
            foreach (var seg in _segments)
            {
                var temp = seg.HPointStart;
                seg.HPointStart = seg.HPointEnd;
                seg.HPointEnd = temp;
            }
            _segments.Reverse();
        }

        /// <summary>
        /// 将另一个路径的所有线段追加到当前路径末尾，并自动连接（调整另一路径首段的起点）。
        /// </summary>
        public void AppendPath(HLines other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other._segments.Count == 0) return;

            var segmentsToAdd = other._segments.Select(s => CloneSegment(s)).ToList();
            // 调整第一条新线段的起点，使其与当前路径终点相连
            if (_segments.Count > 0)
                segmentsToAdd[0].HPointStart = _segments[_segments.Count - 1].HPointEnd;

            _segments.AddRange(segmentsToAdd);
        }

        #endregion

        #region 工具方法

        /// <summary>深拷贝当前路径（所有线段也被克隆）。</summary>
        public HLines Clone()
        {
            var copy = new HLines();
            foreach (var seg in _segments)
                copy.Add(CloneSegment(seg));
            return copy;
        }

        /// <summary>对单个线段进行克隆，支持已知的所有具体类型。</summary>
        private static HLineBase CloneSegment(HLineBase seg)
        {
            if (seg is HLine line) return line.Clone();
            if (seg is H3PArc arc) return arc.Clone();
            if (seg is HBezier bezier) return bezier.Clone();
            if (seg is HCatmullRom catmull) return catmull.Clone();
            // 回退：用起终点创建直线
            return new HLine(seg.HPointStart.Clone(), seg.HPointEnd.Clone());
        }

        /// <summary>格式化输出路径信息。</summary>
        public override string ToString()
        {
            return $"HLines ({_segments.Count} segments, TotalLength={TotalLength.Value:F4})";
        }

        #endregion
    }
}
