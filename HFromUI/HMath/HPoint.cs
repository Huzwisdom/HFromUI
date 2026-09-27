using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace HFromUI.HMath
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    /// <summary>
    /// 二维点（可变引用类型），提供丰富的几何运算方法。
    /// 支持向量运算、旋转、投影、极坐标等。
    /// </summary>
    [Serializable]
    public class HPoint
    {
        #region 坐标字段
        [HCategoryLanguage("坐标"), HDisplayNameLanguage("X坐标"), HDescriptionLanguage("X 坐标。")]
        [Browsable(true)]
        /// <summary>X 坐标。</summary>
        public HDouble X { get; set; }

        [HCategoryLanguage("坐标"), HDisplayNameLanguage("Y坐标"), HDescriptionLanguage("Y 坐标。")]
        [Browsable(true)]
        /// <summary>Y 坐标。</summary>
        public HDouble Y { get; set; }

        #endregion

        #region 构造函数

        /// <summary>无参构造，默认为 (0,0)。</summary>
        public HPoint() { }

        /// <summary>使用整数坐标构造。</summary>
        public HPoint(int x, int y) { X = x; Y = y; }

        /// <summary>使用单精度浮点坐标构造。</summary>
        public HPoint(float x, float y) { X = x; Y = y; }

        /// <summary>使用双精度浮点坐标构造。</summary>
        public HPoint(HDouble x, HDouble y) { X = x; Y = y; }


        /// <summary>从 <see cref="System.Drawing.Point"/> 构造。</summary>
        public HPoint(System.Drawing.Point point) { X = point.X; Y = point.Y; }

        /// <summary>从 <see cref="System.Drawing.PointF"/> 构造。</summary>
        public HPoint(System.Drawing.PointF point) { X = point.X; Y = point.Y; }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 从极坐标构造点（角度为弧度）。
        /// </summary>
        /// <param name="length">极径（≥0）。</param>
        /// <param name="angleRad">极角（弧度）。</param>
        /// <returns>对应的直角坐标点。</returns>
        public static HPoint FromPolar(HDouble length, HDouble angleRad)
        {
            return new HPoint(length * Math.Cos(angleRad.Value), length * Math.Sin(angleRad.Value));
        }

        #endregion

        #region 实例属性（向量特征）
        [Browsable(false)]
        /// <summary>获取从原点到该点的向量长度。</summary>
        public virtual HDouble Length => Math.Sqrt(X.Value * X.Value + Y.Value * Y.Value);
        [Browsable(false)]
        /// <summary>获取长度平方，避免开方，性能更高。</summary>
        public virtual HDouble LengthSquared => X.Value * X.Value + Y.Value * Y.Value;

        [Browsable(false)]
        /// <summary>
        /// 获取该点对应的单位向量（方向不变，长度为 1）。
        /// 若为零向量则返回 (0,0)。
        /// </summary>
        public  HPoint Normalized
        {
            get
            {
                HDouble len = Length;
                if (len < 1e-15) return new HPoint(0, 0);
                return new HPoint(X.Value / len, Y.Value / len);
            }
        }

        #endregion

  
        #region 实例方法：基本运算

        /// <summary>深拷贝当前点。</summary>
        public virtual HPoint Clone() => new HPoint(X.Value, Y.Value);

        /// <summary>克隆。</summary>
        public virtual HPoint Clone(int decimals)
        {
            return new HPoint(X.SetValue(decimals), Y.SetValue(decimals));
        }

        /// <summary>计算当前点到目标点的距离。</summary>
        /// <summary>计算两点距离。</summary>
        /// <summary>计算两点距离。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HDouble DistanceTo(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            HDouble dx = X.Value - other.X.Value;
            HDouble dy = Y.Value - other.Y.Value;
            return Math.Sqrt((dx * dx + dy * dy).Value);
        }

        /// <summary>计算当前点到目标点的距离平方。</summary>
        /// <summary>计算距离平方。</summary>
        /// <summary>计算距离平方。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HDouble DistanceSquaredTo(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            HDouble dx = X.Value - other.X.Value;
            HDouble dy = Y.Value - other.Y.Value;
            return dx * dx + dy * dy;
        }

        /// <summary>返回从当前点指向目标点的方向角（弧度，范围 (-π, π]）。</summary>
        public HDouble AngleTo(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return Math.Atan2(other.Y.Value - Y.Value, other.X.Value - X.Value);
        }

        #endregion

        #region 实例方法：向量运算

        /// <summary>向量加法（当前点 + 参数点），返回新点。</summary>
        public HPoint Add(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint(X.Value + other.X.Value, Y.Value + other.Y.Value);
        }

        /// <summary>向量减法（当前点 - 参数点），返回新点。</summary>
        public HPoint Subtract(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint(X.Value - other.X.Value, Y.Value - other.Y.Value);
        }

        /// <summary>标量乘法，返回新点。</summary>
        public  HPoint Scale(HDouble factor) => new HPoint(X.Value * factor, Y.Value * factor);

        /// <summary>与另一点的点积（内积）。</summary>
        public HDouble Dot(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return X.Value * other.X.Value + Y.Value * other.Y.Value;
        }

        /// <summary>与另一点的叉积（二维叉积的标量值，X1*Y2 - Y1*X2）。</summary>
        public HDouble Cross(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return X.Value * other.Y.Value - Y.Value * other.X.Value;
        }

        #endregion

        #region 实例方法：几何变换

        /// <summary>
        /// 绕原点 (0,0) 旋转指定弧度（逆时针为正）。
        /// 返回新点，不修改当前对象。
        /// </summary>
        public HPoint Rotate(HDouble angleRad)
        {
            HDouble cos = Math.Cos(angleRad.Value);
            HDouble sin = Math.Sin(angleRad.Value);
            return new HPoint(X.Value * cos - Y.Value * sin, X.Value * sin + Y.Value * cos);
        }

        /// <summary>
        /// 绕指定中心旋转指定弧度（逆时针为正）。
        /// 返回新点，不修改当前对象。
        /// </summary>
        /// <param name="center">旋转中心，不可为 null。</param>
        /// <param name="angleRad">旋转弧度。</param>
        public HPoint RotateAround(HPoint center, HDouble angleRad)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            HDouble dx = X.Value - center.X.Value;
            HDouble dy = Y.Value - center.Y.Value;
            HDouble cos = Math.Cos(angleRad.Value);
            HDouble sin = Math.Sin(angleRad.Value);
            return new HPoint(center.X.Value + dx * cos - dy * sin,
                              center.Y.Value + dx * sin + dy * cos);
        }

        /// <summary>平移 (dx, dy)，返回新点。</summary>
        public HPoint Translate(HDouble dx, HDouble dy) => new HPoint(X.Value + dx, Y.Value + dy);

        /// <summary>以指定中心缩放，返回新点。</summary>
        public HPoint ScaleAt(HPoint center, HDouble sx, HDouble sy)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            return new HPoint(center.X.Value + (X.Value - center.X.Value) * sx,
                              center.Y.Value + (Y.Value - center.Y.Value) * sy);
        }

        /// <summary>
        /// 返回当前点关于给定直线的镜像对称点（直线由两点定义）。
        /// </summary>
        public HPoint MirrorAcrossLine(HPoint linePoint1, HPoint linePoint2)
        {
            if (linePoint1 == null) throw new ArgumentNullException(nameof(linePoint1));
            if (linePoint2 == null) throw new ArgumentNullException(nameof(linePoint2));

            HDouble dx = linePoint2.X.Value - linePoint1.X.Value;
            HDouble dy = linePoint2.Y.Value - linePoint1.Y.Value;
            if (Math.Abs(dx.Value) < 1e-15 && Math.Abs(dy.Value) < 1e-15)
                return Clone(); // 直线退化为点，镜像即自身

            // 点到直线的垂足
            HDouble t = ((X.Value - linePoint1.X.Value) * dx + (Y.Value - linePoint1.Y.Value) * dy) / (dx * dx + dy * dy);
            HDouble footX = linePoint1.X.Value + t * dx;
            HDouble footY = linePoint1.Y.Value + t * dy;

            // 对称点 = 2*垂足 - 原坐标
            return new HPoint(2 * footX - X.Value, 2 * footY - Y.Value);
        }

        #endregion

        #region 实例方法：坐标与插值

        /// <summary>
        /// 按比例 t 在当前点与目标点之间线性插值。
        /// t=0 返回当前点，t=1 返回目标点。
        /// </summary>
        public HPoint Lerp(HPoint other, HDouble t)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint(X.Value + (other.X.Value - X.Value) * t, Y.Value + (other.Y.Value - Y.Value) * t);
        }

        /// <summary>返回当前点与另一点的中点。</summary>
        public HPoint MidPoint(HPoint other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HPoint((X.Value + other.X.Value) * 0.5, (Y.Value + other.Y.Value) * 0.5);
        }

        #endregion

        #region 静态方法：基本运算

        /// <summary>两点间距离。</summary>
        /// <summary>Distance 方法。</summary>
        /// <summary>Distance 方法。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HDouble Distance(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.DistanceTo(b);
        }

        /// <summary>两点间距离平方。</summary>
        /// <summary>DistanceSquared 方法。</summary>
        /// <summary>DistanceSquared 方法。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HDouble DistanceSquared(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.DistanceSquaredTo(b);
        }

        /// <summary>中点。</summary>
        public static HPoint MidPoint(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.MidPoint(b);
        }

        /// <summary>线性插值。</summary>
        public static HPoint Lerp(HPoint a, HPoint b, HDouble t)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.Lerp(b, t);
        }

        #endregion

        #region 静态方法：向量运算

        /// <summary>向量加法。</summary>
        public static HPoint Add(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return new HPoint(a.X.Value + b.X.Value, a.Y.Value + b.Y.Value);
        }

        /// <summary>向量减法。</summary>
        public static HPoint Subtract(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return new HPoint(a.X.Value - b.X.Value, a.Y.Value - b.Y.Value);
        }

        /// <summary>标量乘法。</summary>
        public static HPoint Scale(HPoint p, HDouble factor)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            return new HPoint(p.X.Value * factor, p.Y.Value * factor);
        }

        /// <summary>点积。</summary>
        public static HDouble Dot(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.X.Value * b.X.Value + a.Y.Value * b.Y.Value;
        }

        /// <summary>叉积（标量）。</summary>
        public static HDouble Cross(HPoint a, HPoint b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            return a.X.Value * b.Y.Value - a.Y.Value * b.X.Value;
        }

        /// <summary>返回从 a 指向 b 的向量的角度（弧度）。</summary>
        public static HDouble Angle(HPoint from, HPoint to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return Math.Atan2(to.Y.Value - from.Y.Value, to.X.Value - from.X.Value);
        }

        #endregion

        #region 运算符重载

        /// <summary>向量加法。</summary>
        public static HPoint operator +(HPoint a, HPoint b) => Add(a, b);

        /// <summary>向量减法。</summary>
        public static HPoint operator -(HPoint a, HPoint b) => Subtract(a, b);

        /// <summary>取反。</summary>
        public static HPoint operator -(HPoint a) => new HPoint(-a.X.Value, -a.Y.Value);

        /// <summary>标量乘法。</summary>
        public static HPoint operator *(HPoint a, HDouble factor) => Scale(a, factor);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HPoint operator *(HDouble factor, HPoint a) => Scale(a, factor);

        /// <summary>标量除法。</summary>
        public static HPoint operator /(HPoint a, HDouble divisor)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            return new HPoint(a.X.Value / divisor, a.Y.Value / divisor);
        }

        /// <summary>相等比较（基于坐标，带浮点容差）。</summary>
        public static bool operator ==(HPoint a, HPoint b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return Math.Abs(a.X.Value - b.X.Value) < 1e-10 && Math.Abs(a.Y.Value - b.Y.Value) < 1e-10;
        }

        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HPoint a, HPoint b) => !(a == b);

        /// <summary>基于坐标比较的相等性。</summary>
        public override bool Equals(object obj)
        {
            if (obj is HPoint other)
                return this == other;
            return false;
        }

        /// <summary>获取哈希码。</summary>
        public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 16);

        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将 HPoint 转为 GDI+ 的 <see cref="System.Drawing.PointF"/>。
        /// 这是绘图场景（Graphics.DrawLine/DrawString 等）中最常用的目标类型。
        /// 其余方向的转换一律使用下面的 ToXxx()/FromXxx()/Parse() 方法显式完成。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator System.Drawing.PointF(HPoint p)
            => new System.Drawing.PointF(p.X.ToSingle(), p.Y.ToSingle());

        // 以下原转换已删除，改用对应方法：
        //   explicit Size(HPoint)              → ToSize()
        //   implicit Point(HPoint)              → ToPoint()
        //   implicit System.Windows.Point(HPoint) → ToWindowsPoint()
        //   implicit HPoint(Point/PointF)       → 直接用构造函数 new HPoint(point/pointF)
        //   implicit HPoint(System.Windows.Point) → FromWindowsPoint()
        //   implicit HPoint(Size)               → FromSize()
        //   implicit HPoint(string)             → Parse()/TryParse()

        /// <summary>转换为 GDI 整数点 <see cref="System.Drawing.Point"/>（坐标四舍五入取整）。</summary>
        /// <summary>转换为 GDI Point。</summary>
        /// <summary>转换为 GDI Point。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public System.Drawing.Point ToPoint()
            => new System.Drawing.Point((int)Math.Round(X.Value), (int)Math.Round(Y.Value));

        /// <summary>转换为 GDI 尺寸 <see cref="System.Drawing.Size"/>（坐标四舍五入取整）。</summary>
        /// <summary>转换为 Size。</summary>
        /// <summary>转换为 Size。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public System.Drawing.Size ToSize()
            => new System.Drawing.Size((int)Math.Round(X.Value), (int)Math.Round(Y.Value));

        /// <summary>转换为 WPF 点 <see cref="System.Windows.Point"/>。</summary>
        /// <summary>转换为 WindowsPoint。</summary>
        /// <summary>转换为 WindowsPoint。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public System.Windows.Point ToWindowsPoint()
            => new System.Windows.Point(X.Value, Y.Value);

        /// <summary>从 WPF 点 <see cref="System.Windows.Point"/> 创建 HPoint（Z 坐标不存在，按二维点处理）。</summary>
        /// <summary>从 Windows Point 创建。</summary>
        /// <summary>从 Windows Point 创建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HPoint FromWindowsPoint(System.Windows.Point p)
            => new HPoint(p.X, p.Y);

        /// <summary>从 GDI 尺寸 <see cref="System.Drawing.Size"/> 创建 HPoint（Width→X，Height→Y）。</summary>
        /// <summary>从 Size 创建。</summary>
        /// <summary>从 Size 创建。</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HPoint FromSize(System.Drawing.Size s)
            => new HPoint(s.Width, s.Height);

        /// <summary>
        /// 将 "x,y" 格式的字符串解析为 HPoint；格式错误时返回 null。
        /// </summary>
        /// <param name="text">形如 "1.5,2.3" 的坐标文本。</param>
        public static HPoint Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            string[] vs = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (vs.Length == 2)
            {
                return new HPoint(Convert.ToDouble(vs[0]), Convert.ToDouble(vs[1]));
            }
            return null;
        }

        /// <summary>
        /// 尝试将 "x,y" 格式的字符串解析为 HPoint。解析成功返回 true 并输出结果；失败返回 false。
        /// </summary>
        /// <param name="text">形如 "1.5,2.3" 的坐标文本。</param>
        /// <param name="result">解析成功时为对应的 HPoint；失败时为 null。</param>
        public static bool TryParse(string text, out HPoint result)
        {
            result = Parse(text);
            return result != null;
        }

        #endregion

        #region 重写 ToString

        /// <summary>返回格式为 "(X, Y)" 的字符串。</summary>
        public override string ToString() => $"({X.Value:F5}, {Y.Value:F5})";

        #endregion
    }



}