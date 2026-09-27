using System;
using System.Collections.Generic;
using System.Linq;

namespace HFromUI.HMath
{
    /// <summary>
    /// 轴对齐三维长方体（AABB），由两个对角顶点定义（边与坐标轴平行）。
    /// 提供丰富的几何运算：顶点操作、包含检测、距离、相交、与射线/平面/球体求交、网格转换等。
    /// 所有标量结果使用 <see cref="HDouble"/> 包装。
    /// </summary>
    [Serializable]
    public class HCuboid
    {
        #region 顶点标识枚举

        /// <summary>长方体的 8 个顶点标识，采用面向 X 轴正方向时的命名。</summary>
        public enum Corner
        {
            /// <summary>前-左下 (minX, minY, minZ)</summary>
            FrontBottomLeft,
            /// <summary>前-右下 (maxX, minY, minZ)</summary>
            FrontBottomRight,
            /// <summary>前-左上 (minX, maxY, minZ)</summary>
            FrontTopLeft,
            /// <summary>前-右上 (maxX, maxY, minZ)</summary>
            FrontTopRight,
            /// <summary>后-左下 (minX, minY, maxZ)</summary>
            BackBottomLeft,
            /// <summary>后-右下 (maxX, minY, maxZ)</summary>
            BackBottomRight,
            /// <summary>后-左上 (minX, maxY, maxZ)</summary>
            BackTopLeft,
            /// <summary>后-右上 (maxX, maxY, maxZ)</summary>
            BackTopRight
        }

        #endregion

        #region 字段

        private double _minX, _maxX;
        private double _minY, _maxY;
        private double _minZ, _maxZ;

        #endregion

        #region 属性

        /// <summary>X 方向最小边界。</summary>
        public double MinX => _minX;
        /// <summary>X 方向最大边界。</summary>
        public double MaxX => _maxX;
        /// <summary>Y 方向最小边界。</summary>
        public double MinY => _minY;
        /// <summary>Y 方向最大边界。</summary>
        public double MaxY => _maxY;
        /// <summary>Z 方向最小边界。</summary>
        public double MinZ => _minZ;
        /// <summary>Z 方向最大边界。</summary>
        public double MaxZ => _maxZ;

        /// <summary>X 方向尺寸。</summary>
        public double Width => Math.Max(0, _maxX - _minX);
        /// <summary>Y 方向尺寸。</summary>
        public double Height => Math.Max(0, _maxY - _minY);
        /// <summary>Z 方向尺寸。</summary>
        public double Depth => Math.Max(0, _maxZ - _minZ);

        /// <summary>中心点坐标。</summary>
        public HPoint3D Center => new HPoint3D(
            (_minX + _maxX) * 0.5,
            (_minY + _maxY) * 0.5,
            (_minZ + _maxZ) * 0.5);

        /// <summary>体积（长×宽×高）。</summary>
        public double Volume => Width * Height * Depth;
        /// <summary>表面积（2*(长宽+长高+宽高)）。</summary>
        public double SurfaceArea => 2.0 * (Width * Height + Width * Depth + Height * Depth);
        /// <summary>是否为退化长方体（任一维度尺寸接近于零）。</summary>
        public bool IsDegenerate => Width < 1e-15 || Height < 1e-15 || Depth < 1e-15;
        /// <summary>三个维度的尺寸元组。</summary>
        public (double Width, double Height, double Depth) Size => (Width, Height, Depth);

        #endregion

        #region 构造函数

        /// <summary>默认构造：原点边长为0的退化长方体。</summary>
        public HCuboid()
        {
            _minX = _maxX = 0;
            _minY = _maxY = 0;
            _minZ = _maxZ = 0;
        }

        /// <summary>通过两个对角点创建（自动规范化为 min/max）。</summary>
        public HCuboid(HPoint3D p1, HPoint3D p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            _minX = Math.Min(p1.X.Value, p2.X.Value); _maxX = Math.Max(p1.X.Value, p2.X.Value);
            _minY = Math.Min(p1.Y.Value, p2.Y.Value); _maxY = Math.Max(p1.Y.Value, p2.Y.Value);
            _minZ = Math.Min(p1.Z.Value, p2.Z.Value); _maxZ = Math.Max(p1.Z.Value, p2.Z.Value);
        }

        /// <summary>通过六个边界值直接创建。</summary>
        public HCuboid(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            _minX = Math.Min(minX, maxX); _maxX = Math.Max(minX, maxX);
            _minY = Math.Min(minY, maxY); _maxY = Math.Max(minY, maxY);
            _minZ = Math.Min(minZ, maxZ); _maxZ = Math.Max(minZ, maxZ);
        }

        /// <summary>通过中心点和尺寸创建。</summary>
        public static HCuboid FromCenter(HPoint3D center, double width, double height, double depth)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            double hw = Math.Abs(width) * 0.5;
            double hh = Math.Abs(height) * 0.5;
            double hd = Math.Abs(depth) * 0.5;
            return new HCuboid(center.X.Value - hw, center.X.Value + hw,
                               center.Y.Value - hh, center.Y.Value + hh,
                               center.Z.Value - hd, center.Z.Value + hd);
        }

        /// <summary>从点集构建最小轴对齐包围盒。</summary>
        public static HCuboid FromPoints(IEnumerable<HPoint3D> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var list = points.ToList();
            if (list.Count == 0) return new HCuboid();
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            double minZ = double.MaxValue, maxZ = double.MinValue;
            foreach (var p in list)
            {
                if (p.X.Value < minX) minX = p.X.Value; if (p.X.Value > maxX) maxX = p.X.Value;
                if (p.Y.Value < minY) minY = p.Y.Value; if (p.Y.Value > maxY) maxY = p.Y.Value;
                if (p.Z.Value < minZ) minZ = p.Z.Value; if (p.Z.Value > maxZ) maxZ = p.Z.Value;
            }
            return new HCuboid(minX, maxX, minY, maxY, minZ, maxZ);
        }

        #endregion

        #region 顶点操作

        /// <summary>获取指定标识的顶点坐标。</summary>
        public HPoint3D GetCorner(Corner corner)
        {
            switch (corner)
            {
                case Corner.FrontBottomLeft: return new HPoint3D(_minX, _minY, _minZ);
                case Corner.FrontBottomRight: return new HPoint3D(_maxX, _minY, _minZ);
                case Corner.FrontTopLeft: return new HPoint3D(_minX, _maxY, _minZ);
                case Corner.FrontTopRight: return new HPoint3D(_maxX, _maxY, _minZ);
                case Corner.BackBottomLeft: return new HPoint3D(_minX, _minY, _maxZ);
                case Corner.BackBottomRight: return new HPoint3D(_maxX, _minY, _maxZ);
                case Corner.BackTopLeft: return new HPoint3D(_minX, _maxY, _maxZ);
                case Corner.BackTopRight: return new HPoint3D(_maxX, _maxY, _maxZ);
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
        }

        /// <summary>获取指定顶点的对角顶点坐标。</summary>
        public HPoint3D GetOppositeCorner(Corner corner)
        {
            switch (corner)
            {
                case Corner.FrontBottomLeft: return GetCorner(Corner.BackTopRight);
                case Corner.FrontBottomRight: return GetCorner(Corner.BackTopLeft);
                case Corner.FrontTopLeft: return GetCorner(Corner.BackBottomRight);
                case Corner.FrontTopRight: return GetCorner(Corner.BackBottomLeft);
                case Corner.BackBottomLeft: return GetCorner(Corner.FrontTopRight);
                case Corner.BackBottomRight: return GetCorner(Corner.FrontTopLeft);
                case Corner.BackTopLeft: return GetCorner(Corner.FrontBottomRight);
                case Corner.BackTopRight: return GetCorner(Corner.FrontBottomLeft);
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
        }

        /// <summary>将指定顶点移动到新位置，对角的顶点保持不变（自动纠正反向）。</summary>
        public void SetCorner(Corner corner, HPoint3D newPoint)
        {
            if (newPoint == null) throw new ArgumentNullException(nameof(newPoint));
            switch (corner)
            {
                case Corner.FrontBottomLeft: _minX = newPoint.X.Value; _minY = newPoint.Y.Value; _minZ = newPoint.Z.Value; break;
                case Corner.FrontBottomRight: _maxX = newPoint.X.Value; _minY = newPoint.Y.Value; _minZ = newPoint.Z.Value; break;
                case Corner.FrontTopLeft: _minX = newPoint.X.Value; _maxY = newPoint.Y.Value; _minZ = newPoint.Z.Value; break;
                case Corner.FrontTopRight: _maxX = newPoint.X.Value; _maxY = newPoint.Y.Value; _minZ = newPoint.Z.Value; break;
                case Corner.BackBottomLeft: _minX = newPoint.X.Value; _minY = newPoint.Y.Value; _maxZ = newPoint.Z.Value; break;
                case Corner.BackBottomRight: _maxX = newPoint.X.Value; _minY = newPoint.Y.Value; _maxZ = newPoint.Z.Value; break;
                case Corner.BackTopLeft: _minX = newPoint.X.Value; _maxY = newPoint.Y.Value; _maxZ = newPoint.Z.Value; break;
                case Corner.BackTopRight: _maxX = newPoint.X.Value; _maxY = newPoint.Y.Value; _maxZ = newPoint.Z.Value; break;
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
            // 自动规范化为 min/max
            if (_minX > _maxX) { double t = _minX; _minX = _maxX; _maxX = t; }
            if (_minY > _maxY) { double t = _minY; _minY = _maxY; _maxY = t; }
            if (_minZ > _maxZ) { double t = _minZ; _minZ = _maxZ; _maxZ = t; }
        }

        /// <summary>用两个对角点重新定义长方体。</summary>
        public void SetDiagonalPoints(HPoint3D p1, HPoint3D p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            _minX = Math.Min(p1.X.Value, p2.X.Value); _maxX = Math.Max(p1.X.Value, p2.X.Value);
            _minY = Math.Min(p1.Y.Value, p2.Y.Value); _maxY = Math.Max(p1.Y.Value, p2.Y.Value);
            _minZ = Math.Min(p1.Z.Value, p2.Z.Value); _maxZ = Math.Max(p1.Z.Value, p2.Z.Value);
        }

        #endregion

        #region 平移、缩放与中点移动

        /// <summary>平移长方体，返回新长方体。</summary>
        public HCuboid Translate(double dx, double dy, double dz)
        {
            return new HCuboid(_minX + dx, _maxX + dx,
                               _minY + dy, _maxY + dy,
                               _minZ + dz, _maxZ + dz);
        }

        /// <summary>将中心移动到指定点。</summary>
        public HCuboid MoveCenterTo(HPoint3D newCenter)
        {
            if (newCenter == null) throw new ArgumentNullException(nameof(newCenter));
            var curCenter = Center;
            return Translate(newCenter.X.Value - curCenter.X.Value,
                             newCenter.Y.Value - curCenter.Y.Value,
                             newCenter.Z.Value - curCenter.Z.Value);
        }

        /// <summary>原地平移。</summary>
        public void Offset(double dx, double dy, double dz)
        {
            _minX += dx; _maxX += dx;
            _minY += dy; _maxY += dy;
            _minZ += dz; _maxZ += dz;
        }

        /// <summary>原地膨胀（负值收缩）。</summary>
        public void Inflate(double dx, double dy, double dz)
        {
            _minX -= dx; _maxX += dx;
            _minY -= dy; _maxY += dy;
            _minZ -= dz; _maxZ += dz;
        }

        /// <summary>返回膨胀后的新长方体。</summary>
        public HCuboid Inflated(double dx, double dy, double dz)
        {
            return new HCuboid(_minX - dx, _maxX + dx,
                               _minY - dy, _maxY + dy,
                               _minZ - dz, _maxZ + dz);
        }

        /// <summary>以中心为锚点缩放，返回新长方体。</summary>
        public HCuboid ScaleFromCenter(double sx, double sy, double sz)
        {
            var c = Center;
            double hw = Width * Math.Abs(sx) * 0.5;
            double hh = Height * Math.Abs(sy) * 0.5;
            double hd = Depth * Math.Abs(sz) * 0.5;
            return new HCuboid(c.X.Value - hw, c.X.Value + hw,
                               c.Y.Value - hh, c.Y.Value + hh,
                               c.Z.Value - hd, c.Z.Value + hd);
        }

        #endregion

        #region 包含与关系

        /// <summary>判断点是否在长方体内（含表面）。</summary>
        public bool ContainsPoint(HPoint3D point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            return point.X.Value >= _minX - tolerance && point.X.Value <= _maxX + tolerance &&
                   point.Y.Value >= _minY - tolerance && point.Y.Value <= _maxY + tolerance &&
                   point.Z.Value >= _minZ - tolerance && point.Z.Value <= _maxZ + tolerance;
        }

        /// <summary>判断是否完全包含另一个长方体。</summary>
        public bool ContainsCuboid(HCuboid other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return _minX <= other._minX && _maxX >= other._maxX &&
                   _minY <= other._minY && _maxY >= other._maxY &&
                   _minZ <= other._minZ && _maxZ >= other._maxZ;
        }

        /// <summary>判断是否与另一长方体相交。</summary>
        public bool Intersects(HCuboid other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return _minX < other._maxX && _maxX > other._minX &&
                   _minY < other._maxY && _maxY > other._minY &&
                   _minZ < other._maxZ && _maxZ > other._minZ;
        }

        /// <summary>返回与另一长方体的交集（不相交则返回退化长方体）。</summary>
        public HCuboid Intersection(HCuboid other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double minX = Math.Max(_minX, other._minX);
            double maxX = Math.Min(_maxX, other._maxX);
            double minY = Math.Max(_minY, other._minY);
            double maxY = Math.Min(_maxY, other._maxY);
            double minZ = Math.Max(_minZ, other._minZ);
            double maxZ = Math.Min(_maxZ, other._maxZ);
            if (minX >= maxX || minY >= maxY || minZ >= maxZ)
                return new HCuboid();
            return new HCuboid(minX, maxX, minY, maxY, minZ, maxZ);
        }

        /// <summary>返回包围两个长方体的最小长方体。</summary>
        public HCuboid Union(HCuboid other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HCuboid(Math.Min(_minX, other._minX), Math.Max(_maxX, other._maxX),
                               Math.Min(_minY, other._minY), Math.Max(_maxY, other._maxY),
                               Math.Min(_minZ, other._minZ), Math.Max(_maxZ, other._maxZ));
        }

        #endregion

        #region 点到长方体的距离与最近点

        /// <summary>点到长方体表面的最短距离（HDouble），内部点距离为0。</summary>
        public HDouble DistanceToPoint(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = Math.Max(_minX - point.X.Value, Math.Max(0d, point.X.Value - _maxX));
            double dy = Math.Max(_minY - point.Y.Value, Math.Max(0d, point.Y.Value - _maxY));
            double dz = Math.Max(_minZ - point.Z.Value, Math.Max(0d, point.Z.Value - _maxZ));
            return new HDouble { Value = Math.Sqrt(dx * dx + dy * dy + dz * dz) };
        }

        /// <summary>长方体表面上距离给定点最近的点。</summary>
        public HPoint3D ClosestPoint(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double x = Clamp(point.X.Value, _minX, _maxX);
            double y = Clamp(point.Y.Value, _minY, _maxY);
            double z = Clamp(point.Z.Value, _minZ, _maxZ);
            // 若点已经在内部，选择最近表面投影（按最大差值方向）
            if (x == point.X.Value && y == point.Y.Value && z == point.Z.Value)
            {
                double dx = Math.Min(point.X.Value - _minX, _maxX - point.X.Value);
                double dy = Math.Min(point.Y.Value - _minY, _maxY - point.Y.Value);
                double dz = Math.Min(point.Z.Value - _minZ, _maxZ - point.Z.Value);
                if (dx <= dy && dx <= dz) return new HPoint3D(
                    point.X.Value - _minX <= _maxX - point.X.Value ? _minX : _maxX, point.Y.Value, point.Z.Value);
                if (dy <= dx && dy <= dz) return new HPoint3D(
                    point.X.Value, point.Y.Value - _minY <= _maxY - point.Y.Value ? _minY : _maxY, point.Z.Value);
                return new HPoint3D(point.X.Value, point.Y.Value,
                    point.Z.Value - _minZ <= _maxZ - point.Z.Value ? _minZ : _maxZ);
            }
            return new HPoint3D(x, y, z);
        }

        /// <summary>Clamp 方法。</summary>
        private static double Clamp(double val, double min, double max) =>
            Math.Max(min, Math.Min(max, val));

        #endregion

        #region 与球体相交

        /// <summary>判断球体是否与长方体相交（含接触）。</summary>
        public bool IntersectsSphere(HSphere sphere)
        {
            if (sphere == null) throw new ArgumentNullException(nameof(sphere));
            return DistanceToPoint(sphere.Center).Value <= sphere.Radius;
        }

        #endregion

        #region 与射线相交

        /// <summary>
        /// 计算从原点出发的射线与长方体的交点（参数 t 的范围，射线方程：P = origin + t * direction）。
        /// 返回进入和离开时的 t 值，若无交点则返回 null。
        /// </summary>
        /// <param name="origin">射线起点。</param>
        /// <param name="direction">射线方向（不必归一化）。</param>
        /// <returns>(tMin, tMax)，若不存在则返回 null。</returns>
        public (double tMin, double tMax)? IntersectionWithRay(HPoint3D origin, HPoint3D direction)
        {
            if (origin == null) throw new ArgumentNullException(nameof(origin));
            if (direction == null) throw new ArgumentNullException(nameof(direction));

            double tMin = double.MinValue, tMax = double.MaxValue;
            if (Math.Abs(direction.X.Value) > 1e-15)
            {
                double tx1 = (_minX - origin.X.Value) / direction.X.Value;
                double tx2 = (_maxX - origin.X.Value) / direction.X.Value;
                tMin = Math.Max(tMin, Math.Min(tx1, tx2));
                tMax = Math.Min(tMax, Math.Max(tx1, tx2));
            }
            else if (origin.X.Value < _minX || origin.X.Value > _maxX) return null;

            if (Math.Abs(direction.Y.Value) > 1e-15)
            {
                double ty1 = (_minY - origin.Y.Value) / direction.Y.Value;
                double ty2 = (_maxY - origin.Y.Value) / direction.Y.Value;
                tMin = Math.Max(tMin, Math.Min(ty1, ty2));
                tMax = Math.Min(tMax, Math.Max(ty1, ty2));
            }
            else if (origin.Y.Value < _minY || origin.Y.Value > _maxY) return null;

            if (Math.Abs(direction.Z.Value) > 1e-15)
            {
                double tz1 = (_minZ - origin.Z.Value) / direction.Z.Value;
                double tz2 = (_maxZ - origin.Z.Value) / direction.Z.Value;
                tMin = Math.Max(tMin, Math.Min(tz1, tz2));
                tMax = Math.Min(tMax, Math.Max(tz1, tz2));
            }
            else if (origin.Z.Value < _minZ || origin.Z.Value > _maxZ) return null;

            if (tMin > tMax) return null;
            return (tMin, tMax);
        }

        #endregion

        #region 转换为面与网格

        /// <summary>长方体的 6 个面（每个面由 4 个顶点按逆时针顺序排列）。</summary>
        public List<List<HPoint3D>> Faces()
        {
            var ftl = GetCorner(Corner.FrontTopLeft);
            var ftr = GetCorner(Corner.FrontTopRight);
            var fbl = GetCorner(Corner.FrontBottomLeft);
            var fbr = GetCorner(Corner.FrontBottomRight);
            var btl = GetCorner(Corner.BackTopLeft);
            var btr = GetCorner(Corner.BackTopRight);
            var bbl = GetCorner(Corner.BackBottomLeft);
            var bbr = GetCorner(Corner.BackBottomRight);

            return new List<List<HPoint3D>>
            {
                new List<HPoint3D> { fbl, fbr, ftr, ftl }, // 前面
                new List<HPoint3D> { bbr, bbl, btl, btr }, // 后面
                new List<HPoint3D> { ftl, ftr, btr, btl }, // 上面
                new List<HPoint3D> { fbl, bbl, bbr, fbr }, // 下面
                new List<HPoint3D> { fbl, ftl, btl, bbl }, // 左面
                new List<HPoint3D> { fbr, bbr, btr, ftr }  // 右面
            };
        }

        /// <summary>获取 8 个顶点列表。</summary>
        public List<HPoint3D> ToPoints()
        {
            var list = new List<HPoint3D>();
            foreach (Corner corner in Enum.GetValues(typeof(Corner)))
                list.Add(GetCorner(corner));
            return list;
        }

        /// <summary>
        /// 生成长方体的三角形网格（12 个三角形，36 个顶点索引）。
        /// 返回顶点列表和索引列表（每三个索引构成一个三角形）。
        /// </summary>
        public (List<HPoint3D> Vertices, List<int> Indices) ToMesh()
        {
            var verts = ToPoints();
            var indices = new List<int>
            {
                // 前面 (0,1,2) & (2,3,0) 假设顺序: fbl=0, fbr=1, ftr=2, ftl=3
                0,1,2, 2,3,0,
                // 后面 (4,5,6) & (6,7,4) bbl=4, bbr=5, btr=6, btl=7
                4,5,6, 6,7,4,
                // 上面 (3,2,6) & (6,7,3) ftl=3, ftr=2, btr=6, btl=7
                3,2,6, 6,7,3,
                // 下面 (0,4,5) & (5,1,0) fbl=0, bbl=4, bbr=5, fbr=1
                0,4,5, 5,1,0,
                // 左面 (0,3,7) & (7,4,0) fbl=0, ftl=3, btl=7, bbl=4
                0,3,7, 7,4,0,
                // 右面 (1,5,6) & (6,2,1) fbr=1, bbr=5, btr=6, ftr=2
                1,5,6, 6,2,1
            };
            return (verts, indices);
        }

        #endregion

        #region 变换：旋转后包围盒

        /// <summary>
        /// 绕指定中心旋转后，返回新的轴对齐包围盒（AABB）。
        /// </summary>
        public HCuboid RotatedBounds(double angleX, double angleY, double angleZ, HPoint3D rotationCenter = null)
        {
            rotationCenter = rotationCenter ?? Center;
            var points = ToPoints();
            var rotated = points.Select(p =>
            {
                var temp = p.Subtract(rotationCenter);
                // 依次绕 X, Y, Z 旋转
                temp = temp.RotateX(angleX);
                temp = temp.RotateY(angleY);
                temp = temp.RotateZ(angleZ);
                return temp.Add(rotationCenter);
            });
            return FromPoints(rotated);
        }

        #endregion

        #region 工具方法

        /// <summary>克隆。</summary>
        public HCuboid Clone() => new HCuboid(_minX, _maxX, _minY, _maxY, _minZ, _maxZ);

        public override string ToString() =>
            $"Cuboid([{_minX:F2},{_maxX:F2}] x [{_minY:F2},{_maxY:F2}] x [{_minZ:F2},{_maxZ:F2}])";

        public override bool Equals(object obj)
        {
            if (obj is HCuboid other)
                return Math.Abs(_minX - other._minX) < 1e-10 &&
                       Math.Abs(_maxX - other._maxX) < 1e-10 &&
                       Math.Abs(_minY - other._minY) < 1e-10 &&
                       Math.Abs(_maxY - other._maxY) < 1e-10 &&
                       Math.Abs(_minZ - other._minZ) < 1e-10 &&
                       Math.Abs(_maxZ - other._maxZ) < 1e-10;
            return false;
        }

        public override int GetHashCode()
        {
            return _minX.GetHashCode() ^ _maxX.GetHashCode() ^
                   _minY.GetHashCode() ^ _maxY.GetHashCode() ^
                   _minZ.GetHashCode() ^ _maxZ.GetHashCode();
        }

        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HCuboid a, HCuboid b) => a?.Equals(b) ?? b is null;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HCuboid a, HCuboid b) => !(a == b);

        #endregion
    }
}
