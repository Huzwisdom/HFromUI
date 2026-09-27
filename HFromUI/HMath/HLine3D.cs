using System;
using System.Collections.Generic;
using HFromUI;               // Line3DBase, HPoint3D
using HFromUI.HMath;         // HDouble, HCuboid, HSphere 等
using HFromUI.HBase;

namespace HFromUI.HMath
{
    /// <summary>
    /// 三维空间线段，继承 <see cref="HLine3DBase"/>。
    /// 提供长度、中点、方向、点到直线/线段距离、投影、最近点、
    /// 与另一条直线的关系、平面/球体/长方体相交、几何变换等全面三维几何运算。
    /// 所有距离/长度结果均使用 <see cref="HDouble"/> 包装。
    /// </summary>
    public class HLine3D : HLine3DBase
    {
        private HPoint3D _start, _end;

        #region 构造与属性

        public HLine3D(HPoint3D start, HPoint3D end)
        {
            _start = start ?? throw new ArgumentNullException(nameof(start));
            _end = end ?? throw new ArgumentNullException(nameof(end));
        }

        public override HPoint3D HPointStart
        {
            get => _start;
            set => _start = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override HPoint3D HPointEnd
        {
            get => _end;
            set => _end = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>从起点指向终点的向量。</summary>
        public HPoint3D Direction => _end.Subtract(_start);

        /// <summary>线段长度（HDouble）。</summary>
        public HDouble Length => new HDouble { Value = Direction.Length.Value };

        /// <summary>单位方向向量。长度为零时返回零向量。</summary>
        public HPoint3D UnitDirection => Direction.Length < 1e-15
            ? new HPoint3D(0, 0, 0)
            : new HPoint3D(Direction.X.Value / Direction.Length.Value, Direction.Y.Value / Direction.Length.Value, Direction.Z.Value / Direction.Length.Value);

        /// <summary>中点。</summary>
        public HPoint3D MidPoint =>
            new HPoint3D((_start.X.Value + _end.X.Value) * 0.5, (_start.Y.Value + _end.Y.Value) * 0.5, (_start.Z.Value + _end.Z.Value) * 0.5);

        /// <summary>是否为退化点（长度 ≈ 0）。</summary>
        public bool IsPoint => Length.Value < 1e-15;

        #endregion

        #region 参数化与弧长

        /// <summary>参数 t (0~1) 对应的点。</summary>
        public HPoint3D PointAt(double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            var d = Direction;
            return new HPoint3D(_start.X.Value + d.X.Value * t, _start.Y.Value + d.Y.Value * t, _start.Z.Value + d.Z.Value * t);
        }

        /// <summary>
        /// 从起点出发，沿线段前进指定距离，返回对应点。
        /// 距离 ≤ 0 返回起点，≥ 全长返回终点。
        /// </summary>
        public HPoint3D PointFromStartAtDistance(double distance)
        {
            double len = Length.Value;
            if (len < 1e-15) return _start.Clone3D();
            if (distance <= 0) return _start.Clone3D();
            if (distance >= len) return _end.Clone3D();
            return PointAt(distance / len);
        }

        #endregion

        #region 点到线

        /// <summary>点到无限直线的距离（HDouble）。</summary>
        public HDouble DistanceToPoint(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsPoint) return new HDouble { Value = _start.DistanceTo(point) };
            var cross = point.Subtract(_start).Cross(Direction);
            return new HDouble { Value = cross.Length.Value / Length.Value };
        }

        /// <summary>点到线段的最短距离（HDouble）。</summary>
        public HDouble DistanceToSegment(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsPoint) return new HDouble { Value = _start.DistanceTo(point) };
            return new HDouble { Value = point.DistanceTo(ClosestPointOnSegment(point)) };
        }

        /// <summary>点到直线投影（垂足）。</summary>
        public HPoint3D ProjectPoint(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsPoint) return _start.Clone3D();
            var dir = Direction;
            double t = point.Subtract(_start).Dot(dir) / dir.LengthSquared.Value;
            return PointAt(t); // 无限制（无限直线）
        }

        /// <summary>点在线段上的最近点（限制在端点内）。</summary>
        public HPoint3D ClosestPointOnSegment(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            if (IsPoint) return _start.Clone3D();
            var dir = Direction;
            double t = point.Subtract(_start).Dot(dir) / dir.LengthSquared.Value;
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return PointAt(t);
        }

        /// <summary>点是否在直线上（距离小于容差）。</summary>
        public bool IsPointOnLine(HPoint3D point, double tolerance = 1e-10)
        {
            return DistanceToPoint(point).Value < tolerance;
        }

        /// <summary>点是否在线段上。</summary>
        public bool IsPointOnSegment(HPoint3D point, double tolerance = 1e-10)
        {
            return DistanceToSegment(point).Value < tolerance;
        }

        #endregion

        #region 与另一条直线的关系

        /// <summary>是否平行（方向向量叉积长度为零）。</summary>
        public bool IsParallelTo(HLine3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (IsPoint || other.IsPoint) return false;
            return Direction.Cross(other.Direction).Length < 1e-12;
        }

        /// <summary>是否垂直（方向向量点积为零）。</summary>
        public bool IsPerpendicularTo(HLine3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (IsPoint || other.IsPoint) return false;
            return Math.Abs(Direction.Dot(other.Direction)) < 1e-12;
        }

        /// <summary>两条直线之间的最短距离（HDouble）。</summary>
        public HDouble DistanceToLine(HLine3D other)
        {
            var (p1, p2) = ClosestPointsBetweenLines(other);
            return new HDouble { Value = p1.DistanceTo(p2) };
        }

        /// <summary>
        /// 求两条直线上最近的点对（本直线上的点, 另一直线上的点）。
        /// </summary>
        public (HPoint3D OnThis, HPoint3D OnOther) ClosestPointsBetweenLines(HLine3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            var p1 = _start;
            var d1 = Direction;
            var p2 = other._start;
            var d2 = other.Direction;

            if (IsPoint) return (p1.Clone3D(), other.ClosestPointOnSegment(p1));
            if (other.IsPoint) return (ClosestPointOnSegment(p2), p2.Clone3D());

            double a = d1.Dot(d1);
            double b = d1.Dot(d2);
            double c = d2.Dot(d2);
            double d = d1.Dot(p1) - d1.Dot(p2);
            double e = d2.Dot(p1) - d2.Dot(p2);
            double denom = a * c - b * b;

            double t1, t2;
            if (Math.Abs(denom) < 1e-12) // 平行或几乎平行
            {
                t1 = 0;
                t2 = (b > c ? d / b : e / c);
            }
            else
            {
                t1 = (b * e - c * d) / denom;
                t2 = (a * e - b * d) / denom;
            }

            // 使用 Scale3D 而不是 Scale
            HPoint3D onThis = p1.Add(d1.Scale3D(t1));
            HPoint3D onOther = p2.Add(d2.Scale3D(t2));
            return (onThis, onOther);
        }

        /// <summary>计算两条无限直线的交点（必须共面且不平行）。否则返回 null。</summary>
        public HPoint3D IntersectionWithLine(HLine3D other)
        {
            var (p1, p2) = ClosestPointsBetweenLines(other);
            if (p1.DistanceTo(p2) > 1e-10) return null;
            return p1; // 重合或交于一点
        }

        #endregion

        #region 与平面相交

        /// <summary>无限直线与平面的交点。平行时返回 null。</summary>
        public HPoint3D IntersectionWithPlane(HPoint3D pointOnPlane, HPoint3D normal)
        {
            if (pointOnPlane == null) throw new ArgumentNullException(nameof(pointOnPlane));
            if (normal == null) throw new ArgumentNullException(nameof(normal));

            double denom = Direction.Dot(normal);
            if (Math.Abs(denom) < 1e-15) return null;
            double t = pointOnPlane.Subtract(_start).Dot(normal) / denom;
            return PointAt(t);
        }

        /// <summary>线段与平面的交点（交点必须在端点之间）。</summary>
        public HPoint3D IntersectionWithPlaneBounded(HPoint3D pointOnPlane, HPoint3D normal)
        {
            var inter = IntersectionWithPlane(pointOnPlane, normal);
            if (inter == null) return null;
            // 利用投影计算参数 t
            double t = inter.Subtract(_start).Dot(Direction) / Direction.LengthSquared.Value;
            if (t < -1e-10 || t > 1 + 1e-10) return null;
            return inter;
        }

        #endregion

        #region 与球体相交

        /// <summary>无限直线与球体的交点（0~2个）。</summary>
        public List<HPoint3D> IntersectionWithSphere(HPoint3D center, double radius)
        {
            var result = new List<HPoint3D>();
            if (center == null) throw new ArgumentNullException(nameof(center));

            var dir = Direction;
            double a = dir.LengthSquared.Value;
            if (a < 1e-15) return result;

            double b = 2 * (_start.Subtract(center).Dot(dir));
            double c = _start.Subtract(center).LengthSquared.Value - radius * radius;
            double disc = b * b - 4 * a * c;
            if (disc < -1e-12) return result;
            if (disc < 0) disc = 0;
            double sqrtD = Math.Sqrt(disc);
            double t1 = (-b - sqrtD) / (2 * a);
            double t2 = (-b + sqrtD) / (2 * a);
            result.Add(PointAt(t1));
            if (Math.Abs(t1 - t2) > 1e-12) result.Add(PointAt(t2));
            return result;
        }

        /// <summary>线段与球体的交点（仅保留位于线段上的点）。</summary>
        public List<HPoint3D> IntersectionWithSphereBounded(HPoint3D center, double radius)
        {
            var hits = IntersectionWithSphere(center, radius);
            hits.RemoveAll(pt =>
            {
                double t = pt.Subtract(_start).Dot(Direction) / Direction.LengthSquared.Value;
                return t < -1e-10 || t > 1 + 1e-10;
            });
            return hits;
        }

        #endregion

        #region 与长方体相交

        /// <summary>线段是否与轴对齐长方体相交（Slab 方法）。</summary>
        public bool IntersectsCuboid(HCuboid box)
        {
            if (box == null) throw new ArgumentNullException(nameof(box));
            double tMin = 0.0, tMax = 1.0;
            double[] starts = { _start.X.Value, _start.Y.Value, _start.Z.Value };
            double[] dirs = { Direction.X.Value, Direction.Y.Value, Direction.Z.Value };
            double[] mins = { box.MinX, box.MinY, box.MinZ };
            double[] maxs = { box.MaxX, box.MaxY, box.MaxZ };

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(dirs[i]) < 1e-15)
                {
                    if (starts[i] < mins[i] || starts[i] > maxs[i]) return false;
                }
                else
                {
                    double inv = 1.0 / dirs[i];
                    double t1 = (mins[i] - starts[i]) * inv;
                    double t2 = (maxs[i] - starts[i]) * inv;
                    if (t1 > t2) { double tmp = t1; t1 = t2; t2 = tmp; }
                    tMin = Math.Max(tMin, t1);
                    tMax = Math.Min(tMax, t2);
                    if (tMin > tMax) return false;
                }
            }
            return true;
        }

        /// <summary>计算线段与长方体的进入点和离开点（若不相交返回空）。</summary>
        public List<HPoint3D> IntersectionWithCuboid(HCuboid box)
        {
            var result = new List<HPoint3D>();
            if (!IntersectsCuboid(box)) return result;

            double tMin = 0.0, tMax = 1.0;
            double[] starts = { _start.X.Value, _start.Y.Value, _start.Z.Value };
            double[] dirs = { Direction.X.Value, Direction.Y.Value, Direction.Z.Value };
            double[] mins = { box.MinX, box.MinY, box.MinZ };
            double[] maxs = { box.MaxX, box.MaxY, box.MaxZ };

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(dirs[i]) > 1e-15)
                {
                    double inv = 1.0 / dirs[i];
                    double t1 = (mins[i] - starts[i]) * inv;
                    double t2 = (maxs[i] - starts[i]) * inv;
                    tMin = Math.Max(tMin, Math.Min(t1, t2));
                    tMax = Math.Min(tMax, Math.Max(t1, t2));
                }
            }

            if (tMin <= tMax)
            {
                result.Add(PointAt(tMin));
                if (tMax - tMin > 1e-12) result.Add(PointAt(tMax));
            }
            return result;
        }

        #endregion

        #region 几何变换

        /// <summary>平移。</summary>
        public HLine3D Translate(double dx, double dy, double dz) =>
            new HLine3D(_start.Translate(dx, dy, dz), _end.Translate(dx, dy, dz));

        /// <summary>缩放。</summary>
        public HLine3D Scale(double sx, double sy, double sz, HPoint3D center = null)
        {
            center = center ?? new HPoint3D(0, 0, 0);
            return new HLine3D(_start.ScaleAt(center, sx, sy, sz), _end.ScaleAt(center, sx, sy, sz));
        }

        /// <summary>RotateX 方法。</summary>
        public HLine3D RotateX(double angleRad, HPoint3D center = null)
        {
            center = center ?? new HPoint3D(0, 0, 0);
            return new HLine3D(_start.RotateAroundPointAxis(center, new HPoint3D(1, 0, 0), angleRad),
                               _end.RotateAroundPointAxis(center, new HPoint3D(1, 0, 0), angleRad));
        }

        /// <summary>RotateY 方法。</summary>
        public HLine3D RotateY(double angleRad, HPoint3D center = null)
        {
            center = center ?? new HPoint3D(0, 0, 0);
            return new HLine3D(_start.RotateAroundPointAxis(center, new HPoint3D(0, 1, 0), angleRad),
                               _end.RotateAroundPointAxis(center, new HPoint3D(0, 1, 0), angleRad));
        }

        /// <summary>RotateZ 方法。</summary>
        public HLine3D RotateZ(double angleRad, HPoint3D center = null)
        {
            center = center ?? new HPoint3D(0, 0, 0);
            return new HLine3D(_start.RotateAroundPointAxis(center, new HPoint3D(0, 0, 1), angleRad),
                               _end.RotateAroundPointAxis(center, new HPoint3D(0, 0, 1), angleRad));
        }

        /// <summary>反转方向。</summary>
        public void Reverse()
        {
            HPoint3D tmp = _start;
            _start = _end;
            _end = tmp;
        }

        #endregion

        #region 工具

        /// <summary>克隆。</summary>
        public HLine3D Clone() => new HLine3D(_start.Clone3D(), _end.Clone3D());

        public override string ToString() => $"HLine3D({_start} → {_end})";

        #endregion
    }
}
