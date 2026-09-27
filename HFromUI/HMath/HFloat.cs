using HFromUI.HFrom;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using HFromUI.HFrom.From;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    using HFromUI.HData;
    [TypeConverter(typeof(HFloatConverter))]
    /// <summary>
    /// 一个高性能、线程安全、功能完备的单精度浮点数包装器。
    /// 提供算术运算符、比较运算符、常用常量以及无锁的线程安全随机数生成。
    /// Value 属性为可读可写，便于灵活使用。
    /// </summary>
    public struct HFloat : IEquatable<HFloat>, IComparable<HFloat>, IFormattable
    {
        #region 静态常量
        /// <summary>表示数字 0 的 HFloat。</summary>
        public static readonly HFloat Zero = new HFloat(0f);
        /// <summary>表示数字 1 的 HFloat。</summary>
        public static readonly HFloat One = new HFloat(1f);
        /// <summary>表示负一的 HFloat。</summary>
        public static readonly HFloat MinusOne = new HFloat(-1f);
        /// <summary>HFloat 的最小可能正值。</summary>
        public static readonly HFloat Epsilon = new HFloat(float.Epsilon);
        /// <summary>HFloat 的最小可能值。</summary>
        public static readonly HFloat MinValue = new HFloat(float.MinValue);
        /// <summary>HFloat 的最大可能值。</summary>
        public static readonly HFloat MaxValue = new HFloat(float.MaxValue);
        /// <summary>表示非数字 (NaN) 的 HFloat。</summary>
        public static readonly HFloat NaN = new HFloat(float.NaN);
        /// <summary>表示正无穷大的 HFloat。</summary>
        public static readonly HFloat PositiveInfinity = new HFloat(float.PositiveInfinity);
        /// <summary>表示负无穷大的 HFloat。</summary>
        public static readonly HFloat NegativeInfinity = new HFloat(float.NegativeInfinity);
        #endregion

        #region 构造与属性
        /// <summary>获取或设置此实例封装的 <see cref="float"/> 值。</summary>
        public float Value { get; set; }

        /// <summary>获取一个值，指示当前值是否为有效数字（既非 NaN 也非无穷）。</summary>
        public bool IsValid => !float.IsNaN(Value) && !float.IsInfinity(Value);

        /// <summary>
        /// 使用指定的 <see cref="float"/> 值初始化 HFloat 的新实例。
        /// </summary>
        /// <param name="value">单精度浮点数。</param>
        public HFloat(float value)
        {
            Value = value;
        }
        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将原生 <see cref="float"/> 装箱为 HFloat。
        /// 其余方向的转换一律使用 <see cref="Value"/> 属性或 ToXxx()/FromXxx() 方法显式完成。
        /// </summary>
        /// <param name="d">要封装的单精度浮点值。</param>
        public static implicit operator HFloat(float d) => new HFloat(d);

        // 以下原隐式转换已删除，改用对应方法：
        //   float(HFloat)  → 直接读取 Value 属性；
        //   double/int/uint/short/ushort/long/ulong(HFloat) → 下面的 ToXxx() 方法；
        //   HFloat(double) → FromDouble()；
        //   HFloat(int/uint/short/ushort/long/ulong) 可直接 new HFloat(x)（C# 内置到 float 的隐式转换）。

        /// <summary>以 <see cref="double"/> 形式获取封装值。</summary>
        public double ToDouble() => Convert.ToDouble(Value);

        /// <summary>转换为 32 位有符号整数（遵循 <see cref="Convert.ToInt32(float)"/> 取整规则）。</summary>
        public int ToInt() => Convert.ToInt32(Value);

        /// <summary>转换为 32 位无符号整数。</summary>
        public uint ToUInt() => Convert.ToUInt32(Value);

        /// <summary>转换为 16 位有符号整数。</summary>
        public short ToShort() => Convert.ToInt16(Value);

        /// <summary>转换为 16 位无符号整数。</summary>
        public ushort ToUShort() => Convert.ToUInt16(Value);

        /// <summary>转换为 64 位有符号整数。</summary>
        public long ToLong() => Convert.ToInt64(Value);

        /// <summary>转换为 64 位无符号整数。</summary>
        public ulong ToULong() => Convert.ToUInt64(Value);

        /// <summary>从 <see cref="double"/> 创建 HFloat（double→float 为 C# 显式转换，故提供方法）。</summary>
        /// <param name="d">双精度浮点值。</param>
        public static HFloat FromDouble(double d) => new HFloat(Convert.ToSingle(d));

        #endregion

        #region 算术运算符重载
        /// <summary>运算符 +：用于 加法。</summary>
        public static HFloat operator +(HFloat a, HFloat b) => new HFloat(a.Value + b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HFloat operator -(HFloat a, HFloat b) => new HFloat(a.Value - b.Value);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HFloat operator *(HFloat a, HFloat b) => new HFloat(a.Value * b.Value);
        /// <summary>运算符 /：用于 除法。</summary>
        public static HFloat operator /(HFloat a, HFloat b) => new HFloat(a.Value / b.Value);
        /// <summary>取模运算符：返回 a 除以 b 的余数。</summary>
        public static HFloat operator %(HFloat a, HFloat b) => new HFloat(a.Value % b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HFloat operator -(HFloat a) => new HFloat(-a.Value);

        /// <summary>条件判断支持：值非零时为真，零时为假。允许 if(hFloat) ...</summary>
        public static bool operator true(HFloat a) => a.Value != 0f;

        /// <summary>条件判断支持：值为零时为假，非零时为真。</summary>
        public static bool operator false(HFloat a) => a.Value == 0f;

        /// <summary>自增运算符：值加 1。对属性使用时请通过临时变量，避免 CS1612。</summary>
        public static HFloat operator ++(HFloat a) => new HFloat(a.Value + 1f);

        /// <summary>自减运算符：值减 1。</summary>
        public static HFloat operator --(HFloat a) => new HFloat(a.Value - 1f);

        /// <summary>按位与运算符：将两个 float 的 IEEE 754 位模式相与。</summary>
        /// <remarks>浮点数原生 C# 不支持位运算，通过 BitConverter.GetBytes(float)/ToInt32 转 int 再运算。</remarks>
        public static HFloat operator &(HFloat a, HFloat b) =>
            new HFloat(BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(a.Value), 0) & BitConverter.ToInt32(BitConverter.GetBytes(b.Value), 0)), 0));

        /// <summary>按位或运算符：将两个 float 的 IEEE 754 位模式相或。</summary>
        public static HFloat operator |(HFloat a, HFloat b) =>
            new HFloat(BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(a.Value), 0) | BitConverter.ToInt32(BitConverter.GetBytes(b.Value), 0)), 0));

        /// <summary>按位异或运算符：将两个 float 的 IEEE 754 位模式相异或。</summary>
        public static HFloat operator ^(HFloat a, HFloat b) =>
            new HFloat(BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(a.Value), 0) ^ BitConverter.ToInt32(BitConverter.GetBytes(b.Value), 0)), 0));

        /// <summary>按位取反运算符：将 float 的 IEEE 754 位模式每一位取反。</summary>
        public static HFloat operator ~(HFloat a) =>
            new HFloat(BitConverter.ToSingle(BitConverter.GetBytes(~BitConverter.ToInt32(BitConverter.GetBytes(a.Value), 0)), 0));

        #endregion

        #region 比较运算符重载
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HFloat a, HFloat b) => a.Value.Equals(b.Value);
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HFloat a, HFloat b) => !a.Value.Equals(b.Value);
        /// <summary>运算符 >：用于 大于。</summary>
        public static bool operator >(HFloat a, HFloat b) => a.Value > b.Value;
        /// <summary>运算符 <：用于 小于。</summary>
        public static bool operator <(HFloat a, HFloat b) => a.Value < b.Value;
        public static bool operator >=(HFloat a, HFloat b) => a.Value >= b.Value;
        public static bool operator <=(HFloat a, HFloat b) => a.Value <= b.Value;
        #endregion

        #region 接口实现
        public bool Equals(HFloat other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is HFloat h && Equals(h);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(HFloat other) => Value.CompareTo(other.Value);
        public override string ToString() => Value.ToString();
        public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
        #endregion

        #region 静态工厂方法 / 解析
        /// <summary>
        /// 尝试将数字的字符串表示形式转换为等效的 HFloat。返回指示转换是否成功的值。
        /// </summary>
        public static bool TryParse(string s, out HFloat result)
        {
            if (float.TryParse(s, out float f))
            {
                result = new HFloat(f);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 将数字的字符串表示形式转换为等效的 HFloat。
        /// </summary>
        public static HFloat Parse(string s) => new HFloat(float.Parse(s));
        #endregion

        #region 随机数生成（线程安全，无锁）
        /// <summary>ThreadRandom 字段。</summary>
        private static readonly ThreadLocal<Random> ThreadRandom =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// 返回一个在 [<paramref name="min"/>, <paramref name="max"/>) 区间内的随机 <see cref="HFloat"/>。
        /// 若 <paramref name="min"/> 大于 <paramref name="max"/>，会自动交换二者。
        /// 当区间跨度导致溢出（无穷大）时，将回退到安全范围内生成随机数。
        /// </summary>
        /// <param name="min">随机数的下限（包含）。不能为 NaN 或 Infinity。</param>
        /// <param name="max">随机数的上限（不包含）。不能为 NaN 或 Infinity。</param>
        /// <returns>包含随机值的 HFloat 实例。</returns>
        /// <exception cref="ArgumentException">任一参数为 NaN 或 Infinity。</exception>
        public static HFloat Random(float min, float max)
        {
            if (float.IsNaN(min) || float.IsNaN(max) ||
                float.IsInfinity(min) || float.IsInfinity(max))
                throw new ArgumentException(HTranslation.GetContent("min 和 max 不能为 NaN 或 Infinity"));

            if (min > max)
                (min, max) = (max, min);

            double range = (double)max - (double)min; // 使用 double 计算范围，防止 float 溢出

            float value;
            if (double.IsInfinity(range) || double.IsNaN(range))
            {
                // 回退安全范围：[-MaxValue/2, MaxValue/2)
                double halfRange = (double)float.MaxValue / 2.0;
                double offset = ThreadRandom.Value.NextDouble() * halfRange;
                value = (float)(-halfRange + offset * 2.0);
            }
            else
            {
                double sample = ThreadRandom.Value.NextDouble();
                value = (float)((double)min + sample * range);
            }

            return new HFloat(value);
        }

        /// <summary>
        /// 使用指定的 <see cref="Random"/> 实例生成 [<paramref name="min"/>, <paramref name="max"/>) 内的随机数。
        /// 适合需要可重复随机序列或自定义随机源的场景。
        /// </summary>
        /// <param name="min">下限（包含），不能为 NaN/Infinity。</param>
        /// <param name="max">上限（不包含），不能为 NaN/Infinity。</param>
        /// <param name="random">非 null 的 Random 实例。</param>
        /// <returns>随机 HFloat。</returns>
        /// <exception cref="ArgumentNullException">random 为 null。</exception>
        /// <exception cref="ArgumentException">min 或 max 无效。</exception>
        public static HFloat Random(float min, float max, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (float.IsNaN(min) || float.IsNaN(max) ||
                float.IsInfinity(min) || float.IsInfinity(max))
                throw new ArgumentException(HTranslation.GetContent("min 和 max 不能为 NaN 或 Infinity"));

            if (min > max)
                (min, max) = (max, min);

            double range = (double)max - (double)min;

            float value;
            if (double.IsInfinity(range) || double.IsNaN(range))
            {
                double halfRange = (double)float.MaxValue / 2.0;
                double offset = random.NextDouble() * halfRange;
                value = (float)(-halfRange + offset * 2.0);
            }
            else
            {
                double sample = random.NextDouble();
                value = (float)((double)min + sample * range);
            }
            return new HFloat(value);
        }
        #endregion
    }

    /// <summary>
    /// 为 HFloat 提供与字符串之间的类型转换，供 PropertyGrid 等设计时环境使用。
    /// </summary>
    public class HFloatConverter : TypeConverter
    {
        /// <summary>CanConvertFrom 方法。</summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <summary>CanConvertTo 方法。</summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        /// <summary>zxMessageShow 字段。</summary>
        private ZvMessageShow zxMessageShow = new ZvMessageShow();

        /// <summary>ConvertFrom 方法。</summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                object old = GetOldValue(context);
                if (string.IsNullOrWhiteSpace(str))
                {
                    if (!zxMessageShow.IsShow)
                    {
                        zxMessageShow.ZzShowDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 Float。输入为null,请正确写数字！", str),
                            HTranslation.GetContent("输入值错误"),
                            0,
                            HTranslation.GetContent("确认"));
                    }
                    return old;
                }
                else
                {
                    if (float.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands,
                            culture.NumberFormat, out float result))
                    {
                        if (!string.IsNullOrWhiteSpace(HAppData.Name))
                        {
                            object instance = context?.Instance;
                            string instanceTypeName = instance?.GetType().FullName;
                            PropertyDescriptor prop = context?.PropertyDescriptor;
                            string propertyName = prop?.Name;
                            string fullPath = $"{instanceTypeName}.{propertyName}";
                            HAppData.Log(fullPath + HTranslation.GetContent(">>用户[{0}]输入：", HAppData.Name) + str);
                        }
                        return new HFloat(result);
                    }
                    if (!zxMessageShow.IsShow)
                    {
                        zxMessageShow.ZzShowDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 Float。请正确写数字！", str),
                            HTranslation.GetContent("输入值错误"),
                            0,
                            HTranslation.GetContent("确认"));
                    }
                }
                return old;
            }
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>获取 oldValue。</summary>
        private HFloat GetOldValue(ITypeDescriptorContext context)
        {
            if (context?.PropertyDescriptor != null && context.Instance != null)
            {
                object val = context.PropertyDescriptor.GetValue(context.Instance);
                if (val is HFloat hf) return hf;
            }
            return HFloat.Zero;
        }

        /// <summary>ConvertTo 方法。</summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is HFloat hf)
            {
                return hf.Value.ToString(culture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
