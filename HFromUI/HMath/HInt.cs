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
    [TypeConverter(typeof(HIntConverter))]
    /// <summary>
    /// 一个高性能、线程安全、功能完备的32位整数包装器。
    /// 提供算术运算符、比较运算符、常用常量以及无锁的线程安全随机数生成。
    /// Value 属性为可读可写，便于灵活使用。
    /// </summary>
    public struct HInt : IEquatable<HInt>, IComparable<HInt>, IFormattable
    {
        #region 静态常量
        /// <summary>表示数字 0 的 HInt。</summary>
        public static readonly HInt Zero = new HInt(0);
        /// <summary>表示数字 1 的 HInt。</summary>
        public static readonly HInt One = new HInt(1);
        /// <summary>表示负一的 HInt。</summary>
        public static readonly HInt MinusOne = new HInt(-1);
        /// <summary>HInt 的最小可能值。</summary>
        public static readonly HInt MinValue = new HInt(int.MinValue);
        /// <summary>HInt 的最大可能值。</summary>
        public static readonly HInt MaxValue = new HInt(int.MaxValue);
        #endregion

        #region 构造与属性
        /// <summary>获取或设置此实例封装的 <see cref="int"/> 值。</summary>
        public int Value { get; set; }

        /// <summary>获取一个值，指示当前值是否有效。对于 HInt，所有 int 值均有效。</summary>
        public bool IsValid => true; // 保持与 HDouble 接口统一

        /// <summary>
        /// 使用指定的 <see cref="int"/> 值初始化 HInt 的新实例。
        /// </summary>
        /// <param name="value">32位整数。</param>
        public HInt(int value)
        {
            Value = value;
        }
        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将原生 <see cref="int"/> 装箱为 HInt。
        /// 其余方向的转换一律使用 <see cref="Value"/> 属性或 ToXxx()/FromXxx() 方法显式完成。
        /// </summary>
        /// <param name="d">要封装的 32 位有符号整数。</param>
        public static implicit operator HInt(int d) => new HInt(d);

        // 以下原隐式转换已删除，改用对应方法：
        //   int(HInt)     → 直接读取 Value 属性；
        //   uint/long/ulong/short/ushort/float/double/decimal(HInt) → 下面的 ToXxx() 方法；
        //   HInt(short/ushort/byte/sbyte) 可直接 new HInt(x)（C# 内置到 int 的隐式转换）；
        //   HInt(uint/long/ulong/float/double) → 下面的 FromXxx() 静态方法。

        /// <summary>转换为 32 位无符号整数。</summary>
        public uint ToUInt() => Convert.ToUInt32(Value);

        /// <summary>转换为 64 位有符号整数。</summary>
        public long ToLong() => Value;

        /// <summary>转换为 64 位无符号整数。</summary>
        public ulong ToULong() => Convert.ToUInt64(Value);

        /// <summary>转换为 16 位有符号整数（可能截断）。</summary>
        public short ToShort() => Convert.ToInt16(Value);

        /// <summary>转换为 16 位无符号整数（可能截断）。</summary>
        public ushort ToUShort() => Convert.ToUInt16(Value);

        /// <summary>转换为单精度浮点数。</summary>
        public float ToSingle() => Value;

        /// <summary>转换为双精度浮点数。</summary>
        public double ToDouble() => Value;

        /// <summary>转换为十进制数 <see cref="decimal"/>。</summary>
        public decimal ToDecimal() => Value;

        /// <summary>从 32 位无符号整数创建 HInt（超出范围时按 <see cref="Convert.ToInt32(uint)"/> 处理）。</summary>
        public static HInt FromUInt(uint d) => new HInt(Convert.ToInt32(d));

        /// <summary>从 64 位有符号整数创建 HInt（可能截断为 32 位）。</summary>
        public static HInt FromLong(long d) => new HInt(Convert.ToInt32(d));

        /// <summary>从 64 位无符号整数创建 HInt（可能截断为 32 位）。</summary>
        public static HInt FromULong(ulong d) => new HInt(Convert.ToInt32(d));

        /// <summary>从单精度浮点数创建 HInt（遵循 <see cref="Convert.ToInt32(float)"/> 取整规则）。</summary>
        public static HInt FromSingle(float d) => new HInt(Convert.ToInt32(d));

        /// <summary>从双精度浮点数创建 HInt（遵循 <see cref="Convert.ToInt32(double)"/> 取整规则）。</summary>
        public static HInt FromDouble(double d) => new HInt(Convert.ToInt32(d));

        #endregion

        #region 算术运算符重载
        /// <summary>运算符 +：用于 加法。</summary>
        public static HInt operator +(HInt a, HInt b) => new HInt(a.Value + b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HInt operator -(HInt a, HInt b) => new HInt(a.Value - b.Value);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HInt operator *(HInt a, HInt b) => new HInt(a.Value * b.Value);
        /// <summary>运算符 /：用于 除法。</summary>
        public static HInt operator /(HInt a, HInt b) => new HInt(a.Value / b.Value);
        /// <summary>运算符 %：用于 取模。</summary>
        public static HInt operator %(HInt a, HInt b) => new HInt(a.Value % b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HInt operator -(HInt a) => new HInt(-a.Value);
        public static HInt operator ++(HInt a) => new HInt(a.Value + 1);
        public static HInt operator --(HInt a) => new HInt(a.Value - 1);

        /// <summary>按位与运算符：a 的每一位与 b 对应位相与。</summary>
        public static HInt operator &(HInt a, HInt b) => new HInt(a.Value & b.Value);

        /// <summary>按位或运算符：a 的每一位与 b 对应位相或。</summary>
        public static HInt operator |(HInt a, HInt b) => new HInt(a.Value | b.Value);

        /// <summary>按位异或运算符：a 的每一位与 b 对应位相异或（不同为 1）。</summary>
        public static HInt operator ^(HInt a, HInt b) => new HInt(a.Value ^ b.Value);

        /// <summary>按位取反运算符：将 a 的每一位取反。</summary>
        public static HInt operator ~(HInt a) => new HInt(~a.Value);

        #endregion

        #region 比较运算符重载
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HInt a, HInt b) => a.Value == b.Value;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HInt a, HInt b) => a.Value != b.Value;
        /// <summary>运算符 >：用于 大于。</summary>
        public static bool operator >(HInt a, HInt b) => a.Value > b.Value;
        /// <summary>运算符 <：用于 小于。</summary>
        public static bool operator <(HInt a, HInt b) => a.Value < b.Value;
        public static bool operator >=(HInt a, HInt b) => a.Value >= b.Value;
        public static bool operator <=(HInt a, HInt b) => a.Value <= b.Value;

        /// <summary>条件判断支持：值非零时为真，零时为假。允许 if(HInt) ...</summary>
        public static bool operator true(HInt a) => a.Value != 0;

        /// <summary>条件判断支持：值为零时为假，非零时为真。</summary>
        public static bool operator false(HInt a) => a.Value == 0;
        #endregion

        #region 接口实现
        public bool Equals(HInt other) => Value == other.Value;
        public override bool Equals(object obj) => obj is HInt h && Equals(h);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(HInt other) => Value.CompareTo(other.Value);

        /// <summary>
        /// 返回此实例的字符串表示形式。
        /// </summary>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// 使用指定的格式和区域性特定格式信息，将此实例的数值转换为其等效的字符串表示形式。
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
        #endregion

        #region 静态工厂方法 / 解析
        /// <summary>
        /// 尝试将数字的字符串表示形式转换为等效的 HInt。返回指示转换是否成功的值。
        /// </summary>
        public static bool TryParse(string s, out HInt result)
        {
            if (int.TryParse(s, out int i))
            {
                result = new HInt(i);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 将数字的字符串表示形式转换为等效的 HInt。
        /// </summary>
        public static HInt Parse(string s) => new HInt(int.Parse(s));
        #endregion

        #region 随机数生成（线程安全，无锁）
        /// <summary>
        /// 每个线程拥有独立的 <see cref="Random"/> 实例，使用 <see cref="Guid"/> 哈希值作为种子，
        /// 避免多线程下的锁争用与序列重复。
        /// </summary>
        private static readonly ThreadLocal<Random> ThreadRandom =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// 返回一个在 [<paramref name="min"/>, <paramref name="max"/>) 区间内的随机 <see cref="HInt"/>。
        /// 若 <paramref name="min"/> 大于 <paramref name="max"/>，会自动交换二者。
        /// 支持跨越整个 <see cref="int"/> 范围的随机数生成。
        /// </summary>
        /// <param name="min">随机数的下限（包含）。</param>
        /// <param name="max">随机数的上限（不包含）。</param>
        /// <returns>包含随机值的 HInt 实例。</returns>
        public static HInt Random(int min, int max)
        {
            if (min > max)
                (min, max) = (max, min);

            long range = (long)max - min;
            if (range <= 0)
                return new HInt(min);

            // 当范围超过 int.MaxValue 时，Random.Next(int,int) 无法直接使用，
            // 改用 double 采样以保证均匀分布，且兼容 .NET Framework 4.8
            if (range > int.MaxValue)
            {
                double sample = ThreadRandom.Value.NextDouble();
                long result = min + (long)(sample * range);
                // 强制转换回 int 时可能溢出，使用 unchecked 确保不抛异常（C# 默认为 unchecked）
                return new HInt(unchecked((int)result));
            }
            else
            {
                return new HInt(ThreadRandom.Value.Next(min, max));
            }
        }

        /// <summary>
        /// 使用指定的 <see cref="Random"/> 实例生成 [<paramref name="min"/>, <paramref name="max"/>) 内的随机数。
        /// 适合需要可重复随机序列或自定义随机源的场景。
        /// </summary>
        /// <param name="min">下限（包含）。</param>
        /// <param name="max">上限（不包含）。</param>
        /// <param name="random">非 null 的 Random 实例。</param>
        /// <returns>随机 HInt。</returns>
        /// <exception cref="ArgumentNullException">random 为 null。</exception>
        public static HInt Random(int min, int max, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            if (min > max)
                (min, max) = (max, min);

            long range = (long)max - min;
            if (range <= 0)
                return new HInt(min);

            if (range > int.MaxValue)
            {
                double sample = random.NextDouble();
                long result = min + (long)(sample * range);
                return new HInt(unchecked((int)result));
            }
            else
            {
                return new HInt(random.Next(min, max));
            }
        }
        #endregion
    }

    /// <summary>
    /// 为 HInt 提供与字符串之间的类型转换，供 PropertyGrid 等设计时环境使用。
    /// </summary>
    public class HIntConverter : TypeConverter
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
                            HTranslation.GetContent("无法将 {0} 转换为 Int。输入为null,请正确写数字！", str),
                            HTranslation.GetContent("输入值错误"),
                            0,
                            HTranslation.GetContent("确认"));
                    }
                    return old;
                }
                else
                {
                    if (int.TryParse(str, NumberStyles.Integer | NumberStyles.AllowThousands,
                            culture.NumberFormat, out int result))
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
                        return new HInt(result);
                    }
                    if (!zxMessageShow.IsShow)
                    {
                        zxMessageShow.ZzShowDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 Int。请正确写数字！", str),
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
        private HInt GetOldValue(ITypeDescriptorContext context)
        {
            if (context?.PropertyDescriptor != null && context.Instance != null)
            {
                object val = context.PropertyDescriptor.GetValue(context.Instance);
                if (val is HInt hi) return hi;
            }
            return HInt.Zero;
        }

        /// <summary>ConvertTo 方法。</summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is HInt hi)
            {
                return hi.Value.ToString(culture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
