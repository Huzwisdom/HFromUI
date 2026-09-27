using System;
using System.Collections.Generic;
using System.Linq;

namespace HFromUI.HMath
{
    /// <summary>
    /// 三维球体（可变引用类型），以中心和半径定义。
    /// 提供丰富的三维几何运算：包含判断、距离、最近点、
    /// 与直线、平面、其他球体相交，表面积、体积，
    /// 以及球坐标/柱坐标与直角坐标的相互转换（角度可选用度数或弧度）。
    /// 所有标量结果均使用 <see cref="HDouble"/> 包装。
    /// </summary>
    [Serializable]
    public class HSphere
    {
        #region 字段

        private HPoint3D _center;
        private double _radius;

        #endregion

        #region 属性

        /// <summary>球心坐标。</summary>
        public HPoint3D Center
        {
            get => _center;
            set => _center = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>半径（自动取绝对值）。</summary>
        public double Radius
        {
            get => _radius;
            set => _radius = Math.Abs(value);
        }

        /// <summary>直径。</summary>
        public double Diameter => 2.0 * _radius;

        /// <summary>表面积（4πr²）。</summary>
        public double SurfaceArea => 4.0 * Math.PI * _radius * _radius;

        /// <summary>体积（4/3 π r³）。</summary>
        public double Volume => (4.0 / 3.0) * Math.PI * _radius * _radius * _radius;

        /// <summary>球体是否退化（半径接近于零）。</summary>
        public bool IsDegenerate => _radius < 1e-15;

        #endregion

        #region 构造函数

        /// <summary>默认构造：球心 (0,0,0)，半径 0。</summary>
        public HSphere()
        {
            _center = new HPoint3D(0, 0, 0);
            _radius = 0;
        }

        /// <summary>使用球心和半径构造。</summary>
        /// <param name="center">球心，不可为 null。</param>
        /// <param name="radius">半径，负值自动取绝对值。</param>
        public HSphere(HPoint3D center, double radius)
        {
            _center = center ?? throw new ArgumentNullException(nameof(center));
            _radius = Math.Abs(radius);
        }

        /// <summary>使用四个点构造外接球（四点不共面）。若共面则返回退化球（半径 0）。</summary>
        /// <param name="p1">第一个点。</param>
        /// <param name="p2">第二个点。</param>
        /// <param name="p3">第三个点。</param>
        /// <param name="p4">第四个点。</param>
        public HSphere(HPoint3D p1, HPoint3D p2, HPoint3D p3, HPoint3D p4)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            if (p3 == null) throw new ArgumentNullException(nameof(p3));
            if (p4 == null) throw new ArgumentNullException(nameof(p4));

            // 解线性方程组求外接球心与半径
            double[][] m = new double[4][];
            m[0] = new double[] { p1.X.Value, p1.Y.Value, p1.Z.Value, 1 };
            m[1] = new double[] { p2.X.Value, p2.Y.Value, p2.Z.Value, 1 };
            m[2] = new double[] { p3.X.Value, p3.Y.Value, p3.Z.Value, 1 };
            m[3] = new double[] { p4.X.Value, p4.Y.Value, p4.Z.Value, 1 };

            double det = Determinant4x4(m);
            if (Math.Abs(det) < 1e-15)
            {
                _center = new HPoint3D(0, 0, 0);
                _radius = 0;
                return;
            }

            double[] sq = new double[4]
            {
                p1.X.Value * p1.X.Value + p1.Y.Value * p1.Y.Value + p1.Z.Value * p1.Z.Value,
                p2.X.Value * p2.X.Value + p2.Y.Value * p2.Y.Value + p2.Z.Value * p2.Z.Value,
                p3.X.Value * p3.X.Value + p3.Y.Value * p3.Y.Value + p3.Z.Value * p3.Z.Value,
                p4.X.Value * p4.X.Value + p4.Y.Value * p4.Y.Value + p4.Z.Value * p4.Z.Value
            };

            double detX = -DeterminantWithColumn(m, 0, sq);
            double detY = DeterminantWithColumn(m, 1, sq);
            double detZ = -DeterminantWithColumn(m, 2, sq);
            double detW = DeterminantWithColumn(m, 3, sq);

            double cx = detX / (2 * det);
            double cy = detY / (2 * det);
            double cz = detZ / (2 * det);
            double r = Math.Sqrt(cx * cx + cy * cy + cz * cz - detW / det);

            _center = new HPoint3D(cx, cy, cz);
            _radius = double.IsNaN(r) ? 0 : r;
        }

        // 四阶行列式
        private static double Determinant4x4(double[][] m)
        {
            double det = 0;
            for (int i = 0; i < 4; i++)
            {
                double[][] sub = new double[3][];
                for (int r = 1; r < 4; r++)
                {
                    sub[r - 1] = new double[3];
                    int colIdx = 0;
                    for (int c = 0; c < 4; c++)
                    {
                        if (c == i) continue;
                        sub[r - 1][colIdx] = m[r][c];
                        colIdx++;
                    }
                }
                double sign = (i % 2 == 0) ? 1 : -1;
                det += sign * m[0][i] * Determinant3x3(sub);
            }
            return det;
        }

        /// <summary>Determinant3x3 方法。</summary>
        private static double Determinant3x3(double[][] m)
        {
            return m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
                 - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
                 + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);
        }

        /// <summary>DeterminantWithColumn 方法。</summary>
        private static double DeterminantWithColumn(double[][] m, int colIndex, double[] newCol)
        {
            double[][] temp = new double[4][];
            for (int r = 0; r < 4; r++)
            {
                temp[r] = new double[4];
                for (int c = 0; c < 4; c++)
                {
                    temp[r][c] = (c == colIndex) ? newCol[r] : m[r][c];
                }
            }
            return Determinant4x4(temp);
        }

        #endregion

        #region 点关系

        /// <summary>判断点是否在球体内（含表面）。</summary>
        public bool ContainsPoint(HPoint3D point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double distSq = point.DistanceSquaredTo(_center);
            return distSq <= (_radius + tolerance) * (_radius + tolerance);
        }

        /// <summary>点到球面的最短距离（有符号，负值表示在内部）。返回 HDouble。</summary>
        public HDouble DistanceToPoint(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dist = point.DistanceTo(_center);
            return new HDouble { Value = dist - _radius };
        }

        /// <summary>球面上距离给定点最近的点（径向投影）。</summary>
        public HPoint3D ClosestPoint(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = point.X.Value - _center.X.Value;
            double dy = point.Y.Value - _center.Y.Value;
            double dz = point.Z.Value - _center.Z.Value;
            double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (dist < 1e-15)
                return new HPoint3D(_center.X.Value + _radius, _center.Y.Value, _center.Z.Value); // 默认返回 X 轴正向的点
            double scale = _radius / dist;
            return new HPoint3D(_center.X.Value + dx * scale, _center.Y.Value + dy * scale, _center.Z.Value + dz * scale);
        }

        #endregion

        #region 与直线相交

        /// <summary>
        /// 计算球体与由两点定义的无限直线的交点（最多两个）。
        /// </summary>
        /// <param name="p1">直线上一点。</param>
        /// <param name="p2">直线上另一点。</param>
        /// <returns>交点列表（0、1 或 2 个点）。</returns>
        public List<HPoint3D> IntersectionWithLine(HPoint3D p1, HPoint3D p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));

            var result = new List<HPoint3D>();
            double dx = p2.X.Value - p1.X.Value;
            double dy = p2.Y.Value - p1.Y.Value;
            double dz = p2.Z.Value - p1.Z.Value;

            double fx = p1.X.Value - _center.X.Value;
            double fy = p1.Y.Value - _center.Y.Value;
            double fz = p1.Z.Value - _center.Z.Value;

            double a = dx * dx + dy * dy + dz * dz;
            if (a < 1e-15) return result; // 两点重合

            double b = 2 * (fx * dx + fy * dy + fz * dz);
            double c = (fx * fx + fy * fy + fz * fz) - _radius * _radius;

            double discriminant = b * b - 4 * a * c;
            if (discriminant < -1e-12) return result;
            if (discriminant < 0) discriminant = 0;

            double sqrtD = Math.Sqrt(discriminant);
            double t1 = (-b - sqrtD) / (2 * a);
            double t2 = (-b + sqrtD) / (2 * a);

            result.Add(new HPoint3D(p1.X.Value + t1 * dx, p1.Y.Value + t1 * dy, p1.Z.Value + t1 * dz));
            if (Math.Abs(t1 - t2) > 1e-12)
                result.Add(new HPoint3D(p1.X.Value + t2 * dx, p1.Y.Value + t2 * dy, p1.Z.Value + t2 * dz));
            return result;
        }

        /// <summary>计算球体与线段的交点（仅保留参数 t 在 [0,1] 内的点）。</summary>
        public List<HPoint3D> IntersectionWithSegment(HPoint3D start, HPoint3D end)
        {
            var lineInt = IntersectionWithLine(start, end);
            // 过滤不在线段上的点（通过参数 t 反向验证）
            double dx = end.X.Value - start.X.Value;
            double dy = end.Y.Value - start.Y.Value;
            double dz = end.Z.Value - start.Z.Value;
            double lenSq = dx * dx + dy * dy + dz * dz;
            if (lenSq < 1e-15) return lineInt; // 线段退化为点

            lineInt.RemoveAll(pt =>
            {
                double t = ((pt.X.Value - start.X.Value) * dx + (pt.Y.Value - start.Y.Value) * dy + (pt.Z.Value - start.Z.Value) * dz) / lenSq;
                return t < 0 || t > 1;
            });
            return lineInt;
        }

        #endregion

        #region 与平面相交

        /// <summary>
        /// 计算球体与平面的相交圆（圆心和半径）。
        /// 若不相交（相离）则返回 null；若相切则半径为零。
        /// </summary>
        /// <param name="pointOnPlane">平面上一点。</param>
        /// <param name="normal">平面法向量（可不必归一化）。</param>
        /// <returns>相交圆的圆心 (<see cref="HPoint3D"/>) 和半径 (double)，不存在时返回 null。</returns>
        public (HPoint3D center, double radius)? IntersectionWithPlane(HPoint3D pointOnPlane, HPoint3D normal)
        {
            if (pointOnPlane == null) throw new ArgumentNullException(nameof(pointOnPlane));
            if (normal == null) throw new ArgumentNullException(nameof(normal));

            // 归一化法向量
            double len = normal.Length.Value;
            if (len < 1e-15) return null;
            double nx = normal.X.Value / len;
            double ny = normal.Y.Value / len;
            double nz = normal.Z.Value / len;

            // 球心到平面的有向距离
            double dist = (nx * (_center.X.Value - pointOnPlane.X.Value) +
                           ny * (_center.Y.Value - pointOnPlane.Y.Value) +
                           nz * (_center.Z.Value - pointOnPlane.Z.Value));
            double absDist = Math.Abs(dist);
            if (absDist > _radius + 1e-10) return null; // 相离

            // 相交圆圆心 = 球心 - dist * n
            var circleCenter = new HPoint3D(_center.X.Value - dist * nx,
                                            _center.Y.Value - dist * ny,
                                            _center.Z.Value - dist * nz);
            double circleRadius = Math.Sqrt(Math.Max(0, _radius * _radius - absDist * absDist));
            return (circleCenter, circleRadius);
        }

        #endregion

        #region 与球体相交

        /// <summary>
        /// 计算两个球体的相交圆（圆心和半径）。若不相交或包含则返回 null。
        /// </summary>
        /// <param name="other">另一个球体。</param>
        /// <returns>相交圆的圆心、半径和法向量，若不存在则返回 null。</returns>
        public (HPoint3D center, double radius, HPoint3D normal)? IntersectionWithSphere(HSphere other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double d = _center.DistanceTo(other._center);
            if (d < 1e-12) return null; // 同心球
            if (d > _radius + other._radius + 1e-10 || d < Math.Abs(_radius - other._radius) - 1e-10)
                return null; // 相离或内含

            // 余弦定律求距离
            double a = (_radius * _radius - other._radius * other._radius + d * d) / (2 * d);
            double h = Math.Sqrt(Math.Max(0, _radius * _radius - a * a));

            // 圆心
            double ratio = a / d;
            double cx = _center.X.Value + ratio * (other._center.X.Value - _center.X.Value);
            double cy = _center.Y.Value + ratio * (other._center.Y.Value - _center.Y.Value);
            double cz = _center.Z.Value + ratio * (other._center.Z.Value - _center.Z.Value);

            // 法向量方向（从当前球心指向另一球心）
            var normal = new HPoint3D(
                (other._center.X.Value - _center.X.Value) / d,
                (other._center.Y.Value - _center.Y.Value) / d,
                (other._center.Z.Value - _center.Z.Value) / d);

            return (new HPoint3D(cx, cy, cz), h, normal);
        }

        #endregion

        #region 坐标转换：球坐标与柱坐标

        /// <summary>
        /// 将直角坐标 (x,y,z) 转换为球坐标 (半径r, 方位角θ, 极角φ)。
        /// θ 为 XY 平面内的角度（弧度），φ 为与 Z 轴正方向的夹角（弧度）。
        /// </summary>
        public static (double Radius, double ThetaRad, double PhiRad) ToSpherical(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double r = point.Length.Value;
            double theta = Math.Atan2(point.Y.Value, point.X.Value);
            double phi = (r < 1e-15) ? 0 : Math.Acos(point.Z.Value / r);
            return (r, theta, phi);
        }

        /// <summary>将球坐标转换为直角坐标点。</summary>
        public static HPoint3D FromSpherical(double radius, double thetaRad, double phiRad)
        {
            double sinPhi = Math.Sin(phiRad);
            return new HPoint3D(
                radius * sinPhi * Math.Cos(thetaRad),
                radius * sinPhi * Math.Sin(thetaRad),
                radius * Math.Cos(phiRad));
        }

        /// <summary>
        /// 将直角坐标转换为柱坐标 (r, θ, z)，θ 为弧度。
        /// </summary>
        public static (double Radius, double ThetaRad, double Z) ToCylindrical(HPoint3D point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double r = Math.Sqrt(point.X.Value * point.X.Value + point.Y.Value * point.Y.Value);
            double theta = Math.Atan2(point.Y.Value, point.X.Value);
            return (r, theta, point.Z.Value);
        }

        /// <summary>将柱坐标转换为直角坐标点。</summary>
        public static HPoint3D FromCylindrical(double radius, double thetaRad, double z)
        {
            return new HPoint3D(radius * Math.Cos(thetaRad), radius * Math.Sin(thetaRad), z);
        }

        // ---- 角度制版本 ----
        /// <summary>球坐标转换，角度单位为度。</summary>
        public static (double Radius, double ThetaDeg, double PhiDeg) ToSphericalDeg(HPoint3D point)
        {
            var (r, theta, phi) = ToSpherical(point);
            return (r, theta * 180.0 / Math.PI, phi * 180.0 / Math.PI);
        }

        /// <summary>从球坐标（角度制）创建点。</summary>
        public static HPoint3D FromSphericalDeg(double radius, double thetaDeg, double phiDeg)
        {
            return FromSpherical(radius, thetaDeg * Math.PI / 180.0, phiDeg * Math.PI / 180.0);
        }

        /// <summary>柱坐标转换，角度单位为度。</summary>
        public static (double Radius, double ThetaDeg, double Z) ToCylindricalDeg(HPoint3D point)
        {
            var (r, theta, z) = ToCylindrical(point);
            return (r, theta * 180.0 / Math.PI, z);
        }

        /// <summary>从柱坐标（角度制）创建点。</summary>
        public static HPoint3D FromCylindricalDeg(double radius, double thetaDeg, double z)
        {
            return FromCylindrical(radius, thetaDeg * Math.PI / 180.0, z);
        }

        #endregion

        #region 球面经纬度方法

        /// <summary>
        /// 根据球面上的经纬度（弧度）获取点。
        /// 经度（longitude）为 XY 平面内的角度，纬度（latitude）为与 XY 平面的夹角（-π/2 到 π/2），
        /// 这里采用地理常用定义：纬度 φ，经度 λ。
        /// </summary>
        /// <param name="longitudeRad">经度（弧度）。</param>
        /// <param name="latitudeRad">纬度（弧度），正北为正。</param>
        public HPoint3D PointFromLatLong(double longitudeRad, double latitudeRad)
        {
            double cosLat = Math.Cos(latitudeRad);
            double x = _center.X.Value + _radius * cosLat * Math.Cos(longitudeRad);
            double y = _center.Y.Value + _radius * cosLat * Math.Sin(longitudeRad);
            double z = _center.Z.Value + _radius * Math.Sin(latitudeRad);
            return new HPoint3D(x, y, z);
        }

        /// <summary>
        /// 根据球面上的经纬度（度）获取点。
        /// </summary>
        public HPoint3D PointFromLatLongDeg(double longitudeDeg, double latitudeDeg)
        {
            return PointFromLatLong(longitudeDeg * Math.PI / 180.0, latitudeDeg * Math.PI / 180.0);
        }

        #endregion

        #region 几何变换

        /// <summary>平移球体，返回新球体。</summary>
        public HSphere Translate(double dx, double dy, double dz)
        {
            return new HSphere(new HPoint3D(_center.X.Value + dx, _center.Y.Value + dy, _center.Z.Value + dz), _radius);
        }

        /// <summary>等比例缩放（改变半径），返回新球体。</summary>
        public HSphere Scale(double factor)
        {
            return new HSphere(_center.Clone3D(), _radius * Math.Abs(factor));
        }

        #endregion

        #region 多边形网格逼近

        /// <summary>
        /// 使用经纬度细分生成球面顶点网格（三角形列表），用于绘制。
        /// </summary>
        /// <param name="longitudeSegments">经度段数（≥3）。</param>
        /// <param name="latitudeSegments">纬度段数（≥2）。</param>
        /// <returns>三角形顶点列表（每三个点构成一个三角形）。</returns>
        public List<HPoint3D> ToMesh(int longitudeSegments = 24, int latitudeSegments = 16)
        {
            var vertices = new List<HPoint3D>();
            if (longitudeSegments < 3) longitudeSegments = 3;
            if (latitudeSegments < 2) latitudeSegments = 2;

            double lonStep = 2 * Math.PI / longitudeSegments;
            double latStep = Math.PI / latitudeSegments;

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                double latRad = -Math.PI / 2 + lat * latStep; // 从 -90° 到 +90°
                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    double lonRad = lon * lonStep;
                    var p = PointFromLatLong(lonRad, latRad);
                    vertices.Add(p);
                }
            }

            var triangles = new List<HPoint3D>();
            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int current = lat * (longitudeSegments + 1) + lon;
                    int next = current + longitudeSegments + 1;

                    // 两个三角形构成一个四边形网格
                    triangles.Add(vertices[current]);
                    triangles.Add(vertices[next]);
                    triangles.Add(vertices[current + 1]);

                    triangles.Add(vertices[next]);
                    triangles.Add(vertices[next + 1]);
                    triangles.Add(vertices[current + 1]);
                }
            }
            return triangles;
        }

        #endregion

        #region 运算符与工具

        /// <summary>克隆。</summary>
        public HSphere Clone() => new HSphere(_center.Clone3D(), _radius);

        public override string ToString() =>
            $"Sphere(Center=({_center.X.Value:F4},{_center.Y.Value:F4},{_center.Z.Value:F4}), R={_radius:F4})";

        public override bool Equals(object obj)
        {
            if (obj is HSphere other)
                return _center == other._center && Math.Abs(_radius - other._radius) < 1e-10;
            return false;
        }

        public override int GetHashCode() => _center.GetHashCode() ^ _radius.GetHashCode();

        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HSphere a, HSphere b) => a?.Equals(b) ?? b is null;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HSphere a, HSphere b) => !(a == b);

        #endregion
    }
}
