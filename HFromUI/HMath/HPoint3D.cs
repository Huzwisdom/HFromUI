using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HFromUI.HMath
{
    using HFromUI.HData;
    /// <summary>
    /// 三维点（可变引用类型），继承自 <see cref="HPoint"/>，提供丰富的三维几何运算方法。
    /// 包含距离、向量运算、旋转（绕轴）、球坐标/柱坐标、插值等。
    /// </summary>
    [Serializable]
    public class HPoint3D : HPoint
    {
        #region 坐标属性
        [HCategoryLanguage("坐标"), HDisplayNameLanguage("Z坐标"), HDescriptionLanguage("Z 坐标。")]
        [Browsable(true)]
        /// <summary>Z 坐标。</summary>
        public HDouble Z { get; set; }

        #endregion

        #region 构造函数

        /// <summary>无参构造，初始化为 (0,0,0)。</summary>
        public HPoint3D() : base() { Z = 0; }

        /// <summary>使用三个双精度坐标构造。</summary>
        public HPoint3D(double x, double y, double z) : base(x, y) { Z = z; }

        /// <summary>使用三个整数坐标构造。</summary>
        public HPoint3D(int x, int y, int z) : base(x, y) { Z = z; }

        /// <summary>使用三个单精度浮点坐标构造。</summary>
        public HPoint3D(float x, float y, float z) : base(x, y) { Z = z; }

        /// <summary>从二维点构造，Z 坐标默认为 0。</summary>
        public HPoint3D(HPoint point) : base(point.X.Value, point.Y.Value) { Z = 0; }

        /// <summary>从二维点加 Z 坐标构造。</summary>
        public HPoint3D(HPoint point, double z) : base(point.X.Value, point.Y.Value) { Z = z; }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 从柱坐标 (radius, angle, height) 创建点，角度为弧度。
        /// X = radius * cos(angle), Y = radius * sin(angle), Z = height。
        /// </summary>
        public static HPoint3D FromCylindrical(double radius, double angleRad, double height)
        {
            return new HPoint3D(radius * Math.Cos(angleRad), radius * Math.Sin(angleRad), height);
        }

        /// <summary>
        /// 从球坐标 (radius, theta, phi) 创建点，其中 theta 为方位角（弧度），phi 为极角（与 Z 轴正方向夹角，弧度）。
        /// X = r * sin(phi) * cos(theta), Y = r * sin(phi) * sin(theta), Z = r * cos(phi)。
        /// </summary>
        public static HPoint3D FromSpherical(double radius, double thetaRad, double phiRad)
        {
            double sinPhi = Math.Sin(phiRad);
            return new HPoint3D(
                radius * sinPhi * Math.Cos(thetaRad),
                radius * sinPhi * Math.Sin(thetaRad),
                radius * Math.Cos(phiRad));
        }

        #endregion

        #region 实例属性（向量特征）
        [Browsable(false)]
        /// <summary>向量的长度（模）。</summary>
        public override HDouble Length => Math.Sqrt(X.Value * X.Value + Y.Value * Y.Value + Z.Value * Z.Value);
        [Browsable(false)]
        /// <summary>长度平方，避免开方。</summary>
        public override HDouble LengthSquared => X.Value * X.Value + Y.Value * Y.Value + Z.Value * Z.Value;
        [Browsable(false)]
        /// <summary>返回当前向量的单位向量，若为零向量则返回 (0,0,0)。</summary>
        public  HPoint3D Normalized3D
        {
            get
            {
                double len = Length.Value;
                if (len < 1e-15) return new HPoint3D(0, 0, 0);
                return new HPoint3D(X.Value / len, Y.Value / len, Z.Value / len);
            }
        }

        #endregion

        #region 实例方法：基本几何运算

        /// <summary>计算当前点到目标点的三维欧氏距离。</summary>
        /// <summary>计算两点距离。</summary>
        /// <summary>计算两点距离。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceTo(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double dx = X.Value - other.X.Value;
            double dy = Y.Value - other.Y.Value;
            double dz = Z.Value - other.Z.Value;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>计算当前点到目标点的距离平方。</summary>
        /// <summary>计算距离平方。</summary>
        /// <summary>计算距离平方。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistanceSquaredTo(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double dx = X.Value - other.X.Value;
            double dy = Y.Value - other.Y.Value;
            double dz = Z.Value - other.Z.Value;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// 计算从当前点指向另一点的方向向量（单位向量）。
        /// 如果两点重合，返回零向量。
        /// </summary>
        public HPoint3D DirectionTo(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double dx = other.X.Value - X.Value;
            double dy = other.Y.Value - Y.Value;
            double dz = other.Z.Value - Z.Value;
            double len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (len < 1e-15) return new HPoint3D(0, 0, 0);
            return new HPoint3D(dx / len, dy / len, dz / len);
        }

        #endregion

        #region 实例方法：向量运算

        /// <summary>向量加法。</summary>
        public HPoint3D Add(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint3D(X.Value + other.X.Value, Y.Value + other.Y.Value, Z.Value + other.Z.Value);
        }

        /// <summary>向量减法。</summary>
        public HPoint3D Subtract(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint3D(X.Value - other.X.Value, Y.Value - other.Y.Value, Z.Value - other.Z.Value);
        }

        /// <summary>标量乘法。</summary>
        public HPoint3D Scale3D(double factor) => new HPoint3D(X.Value * factor, Y.Value * factor, Z.Value * factor);

        /// <summary>与另一点的点积（内积）。</summary>
        public double Dot(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return X.Value * other.X.Value + Y.Value * other.Y.Value + Z.Value * other.Z.Value;
        }

        /// <summary>与另一点的叉积（返回垂直向量）。</summary>
        public HPoint3D Cross(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint3D(
                Y.Value * other.Z.Value - Z.Value * other.Y.Value,
                Z.Value * other.X.Value - X.Value * other.Z.Value,
                X.Value * other.Y.Value - Y.Value * other.X.Value);
        }

        /// <summary>三个点的混合积 (this · (v1 × v2))，即行列式。</summary>
        public double TripleProduct(HPoint3D v1, HPoint3D v2)
        {
            if (v1 == null) throw new ArgumentNullException(nameof(v1));
            if (v2 == null) throw new ArgumentNullException(nameof(v2));
            return X.Value * (v1.Y.Value * v2.Z.Value - v1.Z.Value * v2.Y.Value)
                 + Y.Value * (v1.Z.Value * v2.X.Value - v1.X.Value * v2.Z.Value)
                 + Z.Value * (v1.X.Value * v2.Y.Value - v1.Y.Value * v2.X.Value);
        }

        #endregion

        #region 实例方法：几何变换

        /// <summary>平移 (dx, dy, dz)，返回新点。</summary>
        public HPoint3D Translate(double dx, double dy, double dz) =>
            new HPoint3D(X.Value + dx, Y.Value + dy, Z.Value + dz);

        /// <summary>以原点为中心缩放，返回新点。</summary>
        public HPoint3D ScaleAtOrigin(double sx, double sy, double sz) =>
            new HPoint3D(X.Value * sx, Y.Value * sy, Z.Value * sz);

        /// <summary>以指定点为中心缩放，返回新点。</summary>
        public HPoint3D ScaleAt(HPoint3D center, double sx, double sy, double sz)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            return new HPoint3D(
                center.X.Value + (X.Value - center.X.Value) * sx,
                center.Y.Value + (Y.Value - center.Y.Value) * sy,
                center.Z.Value + (Z.Value - center.Z.Value) * sz);
        }

        /// <summary>
        /// 绕 X 轴旋转指定弧度（右手法则，逆时针看向原点→正方向）。
        /// 返回新点，不修改当前对象。
        /// </summary>
        public HPoint3D RotateX(double angleRad)
        {
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
            return new HPoint3D(
                X.Value,
                Y.Value * cos - Z.Value * sin,
                Y.Value * sin + Z.Value * cos);
        }

        /// <summary>
        /// 绕 Y 轴旋转指定弧度。
        /// </summary>
        public HPoint3D RotateY(double angleRad)
        {
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
            return new HPoint3D(
                X.Value * cos + Z.Value * sin,
                Y.Value,
                -X.Value * sin + Z.Value * cos);
        }

        /// <summary>
        /// 绕 Z 轴旋转指定弧度（相当于二维平面旋转）。
        /// </summary>
        public HPoint3D RotateZ(double angleRad)
        {
            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
            return new HPoint3D(
                X.Value * cos - Y.Value * sin,
                X.Value * sin + Y.Value * cos,
                Z.Value);
        }

        /// <summary>
        /// 绕通过原点的指定单位方向向量旋转指定弧度（罗德里格旋转公式）。
        /// 如果轴向量长度接近 0，则返回当前点的副本。
        /// </summary>
        /// <param name="axis">旋转轴的单位方向向量（不可为 null）。</param>
        /// <param name="angleRad">旋转弧度。</param>
        public HPoint3D RotateAroundAxis(HPoint3D axis, double angleRad)
        {
            if (axis == null) throw new ArgumentNullException(nameof(axis));
            double axisLen = axis.Length.Value;
            if (axisLen < 1e-15) return Clone3D();

            // 归一化轴向量
            double ux = axis.X.Value / axisLen;
            double uy = axis.Y.Value / axisLen;
            double uz = axis.Z.Value / axisLen;

            double cos = Math.Cos(angleRad);
            double sin = Math.Sin(angleRad);
            double oneMinusCos = 1 - cos;

            // 罗德里格旋转公式
            double dot = X.Value * ux + Y.Value * uy + Z.Value * uz;
            double newX = X.Value * cos + (uy * Z.Value - uz * Y.Value) * sin + ux * dot * oneMinusCos;
            double newY = Y.Value * cos + (uz * X.Value - ux * Z.Value) * sin + uy * dot * oneMinusCos;
            double newZ = Z.Value * cos + (ux * Y.Value - uy * X.Value) * sin + uz * dot * oneMinusCos;

            return new HPoint3D(newX, newY, newZ);
        }

        /// <summary>
        /// 绕通过任意中心的指定轴旋转指定弧度（组合平移与罗德里格旋转）。
        /// </summary>
        /// <param name="center">旋转中心，不可为 null。</param>
        /// <param name="axisDirection">旋转轴方向（无需归一化，不可为 null）。</param>
        /// <param name="angleRad">旋转弧度。</param>
        public HPoint3D RotateAroundPointAxis(HPoint3D center, HPoint3D axisDirection, double angleRad)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            if (axisDirection == null) throw new ArgumentNullException(nameof(axisDirection));

            // 平移到中心为原点
            HPoint3D translated = this.Subtract(center);
            // 绕原点轴旋转
            HPoint3D rotated = translated.RotateAroundAxis(axisDirection, angleRad);
            // 平移回去
            return rotated.Add(center);
        }

        #endregion

        #region 投影

        /// <summary>
        /// 当前向量在给定方向向量上的投影（标量长度）。
        /// 如果方向向量长度接近 0，则返回 0。
        /// </summary>
        public double ProjectScalar(HPoint3D direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double lenSq = direction.LengthSquared.Value;
            if (lenSq < 1e-15) return 0;
            return this.Dot(direction) / Math.Sqrt(lenSq);
        }

        /// <summary>
        /// 当前向量在给定方向上的向量投影。
        /// </summary>
        public HPoint3D ProjectVector(HPoint3D direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double lenSq = direction.LengthSquared.Value;
            if (lenSq < 1e-15) return new HPoint3D(0, 0, 0);
            double scalar = this.Dot(direction) / lenSq;
            return direction.Scale3D(scalar);
        }

        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将 HPoint3D 转为 GDI+ 的 <see cref="System.Drawing.PointF"/>（取 X、Y，忽略 Z）。
        /// 其余方向的转换一律使用下面的 ToXxx()/FromXxx()/Parse() 方法显式完成。
        /// 说明：ToPoint()/ToSize()/ToWindowsPoint() 直接继承自基类 <see cref="HPoint"/>。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator System.Drawing.PointF(HPoint3D p)
            => new System.Drawing.PointF(p.X.ToSingle(), p.Y.ToSingle());

        // 以下原转换已删除，改用对应方法：
        //   explicit Size(HPoint3D)               → 基类 ToSize()
        //   implicit Point(HPoint3D)               → 基类 ToPoint()
        //   implicit System.Windows.Point(HPoint3D) → 基类 ToWindowsPoint()
        //   implicit HPoint3D(Point/PointF/Size/System.Windows.Point) → 下面的 FromXxx()
        //   implicit HPoint3D(string)              → Parse()/TryParse()

        /// <summary>从 GDI 整数点 <see cref="System.Drawing.Point"/> 创建三维点（Z 取 0）。</summary>
        /// <summary>从 HPoint 创建。</summary>
        /// <summary>从 HPoint 创建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HPoint3D FromPoint(System.Drawing.Point p)
            => new HPoint3D(p.X, p.Y, 0);

        /// <summary>从 GDI 浮点点 <see cref="System.Drawing.PointF"/> 创建三维点（Z 取 0）。</summary>
        /// <summary>从 PointF 创建。</summary>
        /// <summary>从 PointF 创建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HPoint3D FromPointF(System.Drawing.PointF p)
            => new HPoint3D(p.X, p.Y, 0);

        /// <summary>从 WPF 点 <see cref="System.Windows.Point"/> 创建三维点（Z 取 0）。</summary>
        /// <summary>从 Windows Point 创建。</summary>
        /// <summary>从 Windows Point 创建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HPoint3D FromWindowsPoint(System.Windows.Point p)
            => new HPoint3D(p.X, p.Y, 0);

        /// <summary>从 GDI 尺寸 <see cref="System.Drawing.Size"/> 创建三维点（Width→X，Height→Y，Z 取 0）。</summary>
        /// <summary>从 Size 创建。</summary>
        /// <summary>从 Size 创建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HPoint3D FromSize(System.Drawing.Size s)
            => new HPoint3D(s.Width, s.Height, 0);

        /// <summary>
        /// 将 "x,y" 或 "x,y,z" 格式的字符串解析为 HPoint3D；格式错误时返回 null。
        /// </summary>
        /// <param name="text">形如 "1.5,2.3" 或 "1,2,3" 的坐标文本。</param>
        public static HPoint3D Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string[] vs = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (vs.Length == 2)
            {
                return new HPoint3D(Convert.ToDouble(vs[0]), Convert.ToDouble(vs[1]), 0);
            }
            else if (vs.Length == 3)
            {
                return new HPoint3D(Convert.ToDouble(vs[0]), Convert.ToDouble(vs[1]), Convert.ToDouble(vs[2]));
            }
            return null;
        }

        /// <summary>
        /// 尝试将 "x,y" 或 "x,y,z" 格式的字符串解析为 HPoint3D。解析成功返回 true 并输出结果；失败返回 false。
        /// </summary>
        /// <param name="text">形如 "1.5,2.3" 或 "1,2,3" 的坐标文本。</param>
        /// <param name="result">解析成功时为对应的 HPoint3D；失败时为 null。</param>
        public static bool TryParse(string text, out HPoint3D result)
        {
            result = Parse(text);
            return result != null;
        }

        #endregion

        #region 插值与中点

        /// <summary>
        /// 线性插值：t=0 返回当前点，t=1 返回目标点。
        /// </summary>
        public HPoint3D Lerp(HPoint3D other, double t)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint3D(
                X.Value + (other.X.Value - X.Value) * t,
                Y.Value + (other.Y.Value - Y.Value) * t,
                Z.Value + (other.Z.Value - Z.Value) * t);
        }

        /// <summary>返回当前点与另一点的中点。</summary>
        public HPoint3D MidPoint(HPoint3D other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint3D(
                (X.Value + other.X.Value) * 0.5,
                (Y.Value + other.Y.Value) * 0.5,
                (Z.Value + other.Z.Value) * 0.5);
        }

        #endregion

        #region 克隆与转换

        /// <summary>
        /// 深度克隆为 <see cref="HPoint3D"/> 类型。
        /// 注意：覆盖基类的 Clone，返回类型仍为 HPoint，但实际是 HPoint3D。
        /// </summary>
        public override HPoint Clone()
        {
            return new HPoint3D(X.Value, Y.Value, Z.Value);
        }

        /// <summary>返回一个显式的 HPoint3D 深拷贝。</summary>
        public HPoint3D Clone3D() => new HPoint3D(X.Value, Y.Value, Z.Value);

        /// <summary>转换为二维点（丢弃 Z 坐标）。</summary>
        public HPoint ToPoint2D() => new HPoint(X.Value, Y.Value);

        #endregion

        #region 静态方法

        /// <summary>两点间三维欧氏距离。</summary>
        /// <summary>Distance 方法。</summary>
        /// <summary>Distance 方法。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(HPoint3D a, HPoint3D b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.DistanceTo(b);
        }

        /// <summary>两点间距离平方。</summary>
        /// <summary>DistanceSquared 方法。</summary>
        /// <summary>DistanceSquared 方法。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DistanceSquared(HPoint3D a, HPoint3D b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.DistanceSquaredTo(b);
        }

        /// <summary>向量加法。</summary>
        public static HPoint3D Add(HPoint3D a, HPoint3D b) => a.Add(b);

        /// <summary>向量减法。</summary>
        public static HPoint3D Subtract(HPoint3D a, HPoint3D b) => a.Subtract(b);

        /// <summary>标量乘法。</summary>
        public static HPoint3D Scale(HPoint3D p, double factor) => p.Scale3D(factor);

        /// <summary>点积。</summary>
        public static double Dot(HPoint3D a, HPoint3D b) => a.Dot(b);

        /// <summary>叉积。</summary>
        public static HPoint3D Cross(HPoint3D a, HPoint3D b) => a.Cross(b);

        /// <summary>混合积 (a × b) · c。</summary>
        public static double TripleProduct(HPoint3D a, HPoint3D b, HPoint3D c)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            return a.TripleProduct(b, c);
        }

        /// <summary>线性插值。</summary>
        public static HPoint3D Lerp(HPoint3D a, HPoint3D b, double t) => a.Lerp(b, t);

        /// <summary>中点。</summary>
        public static HPoint3D MidPoint(HPoint3D a, HPoint3D b) => a.MidPoint(b);

        /// <summary>
        /// 计算两个向量之间的夹角（弧度，范围 [0, π]）。
        /// </summary>
        public static double AngleBetween(HPoint3D a, HPoint3D b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            double dot = a.Dot(b);
            double lenProduct = a.Length.Value * b.Length.Value;
            if (lenProduct < 1e-15) return 0;
            double cosAngle = dot / lenProduct;
            // 防止浮点误差导致 acos 参数超出 [-1,1]
            if (cosAngle > 1) cosAngle = 1;
            if (cosAngle < -1) cosAngle = -1;
            return Math.Acos(cosAngle);
        }

        #endregion

        #region 运算符重载

        /// <summary>运算符 +：用于 加法。</summary>
        public static HPoint3D operator +(HPoint3D a, HPoint3D b) => a.Add(b);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HPoint3D operator -(HPoint3D a, HPoint3D b) => a.Subtract(b);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HPoint3D operator -(HPoint3D a) => new HPoint3D(-a.X.Value, -a.Y.Value, -a.Z.Value);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HPoint3D operator *(HPoint3D a, double factor) => a.Scale3D(factor);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HPoint3D operator *(double factor, HPoint3D a) => a.Scale3D(factor);
        /// <summary>运算符 /：用于 除法。</summary>
        public static HPoint3D operator /(HPoint3D a, double divisor)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            return new HPoint3D(a.X.Value / divisor, a.Y.Value / divisor, a.Z.Value / divisor);
        }

        /// <summary>基于坐标容差相等比较。</summary>
        public static bool operator ==(HPoint3D a, HPoint3D b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return Math.Abs(a.X.Value - b.X.Value) < HAppData.Epsilon &&
                   Math.Abs(a.Y.Value - b.Y.Value) < HAppData.Epsilon &&
                   Math.Abs(a.Z.Value - b.Z.Value) < HAppData.Epsilon;
        }

        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HPoint3D a, HPoint3D b) => !(a == b);

        /// <summary>基于坐标的相等性判断。</summary>
        public override bool Equals(object obj)
        {
            if (obj is HPoint3D other)
                return this == other;
            return false;
        }

        /// <summary>重写哈希码。</summary>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 8) ^ (Z.GetHashCode() << 16);
        }

        #endregion

        #region ToString

        /// <summary>返回格式为 "(X, Y, Z)" 的字符串。</summary>
        public override string ToString() => $"({X.Value:F5}, {Y.Value:F5}, {Z.Value:F5})";

        #endregion
    }
}