using HFromUI.HFrom;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using HFromUI.HFrom.From;
using HFromUI.HControl.Tools.Message;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    using HFromUI.HData;
    [TypeConverter(typeof(HShortConverter))]
    /// <summary>
    /// 一个高性能、线程安全、功能完备的16位有符号整数包装器。
    /// 提供算术运算符、比较运算符、常用常量以及无锁的线程安全随机数生成。
    /// Value 属性为可读可写，便于灵活使用。
    /// </summary>
    public struct HShort : IEquatable<HShort>, IComparable<HShort>, IFormattable
    {
        #region 静态常量
        /// <summary>表示数字 0 的 HShort。</summary>
        public static readonly HShort Zero = new HShort(0);
        /// <summary>表示数字 1 的 HShort。</summary>
        public static readonly HShort One = new HShort(1);
        /// <summary>表示负一的 HShort。</summary>
        public static readonly HShort MinusOne = new HShort(-1);
        /// <summary>HShort 的最小可能值（-32768）。</summary>
        public static readonly HShort MinValue = new HShort(short.MinValue);
        /// <summary>HShort 的最大可能值（32767）。</summary>
        public static readonly HShort MaxValue = new HShort(short.MaxValue);
        #endregion

        #region 构造与属性
        /// <summary>获取或设置此实例封装的 <see cref="short"/> 值。</summary>
        public short Value { get; set; }

        /// <summary>获取一个值，指示当前值是否有效。对于 HShort，所有 short 值均有效。</summary>
        public bool IsValid => true; // 保持接口统一

        /// <summary>
        /// 使用指定的 <see cref="short"/> 值初始化 HShort 的新实例。
        /// </summary>
        /// <param name="value">有符号16位整数。</param>
        public HShort(short value)
        {
            Value = value;
        }
        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将原生 <see cref="short"/> 装箱为 HShort。
        /// 其余方向的转换一律使用 <see cref="Value"/> 属性或 ToXxx()/FromXxx() 方法显式完成。
        /// </summary>
        /// <param name="d">要封装的 16 位有符号整数。</param>
        public static implicit operator HShort(short d) => new HShort(d);

        // 以下原隐式转换已删除，改用对应方法：
        //   short(HShort) → 直接读取 Value 属性；
        //   int/long/float/double/decimal/ushort/uint/ulong/byte/sbyte(HShort) → 下面的 ToXxx() 方法；
        //   HShort(sbyte/byte) 可直接 new HShort(x)（C# 内置到 short 的隐式转换）；
        //   HShort(int/uint/long/ulong/float/double) → 下面的 FromXxx() 静态方法。

        /// <summary>转换为 32 位有符号整数。</summary>
        public int ToInt() => Value;

        /// <summary>转换为 64 位有符号整数。</summary>
        public long ToLong() => Value;

        /// <summary>转换为单精度浮点数。</summary>
        public float ToSingle() => Value;

        /// <summary>转换为双精度浮点数。</summary>
        public double ToDouble() => Value;

        /// <summary>转换为十进制数 <see cref="decimal"/>。</summary>
        public decimal ToDecimal() => Value;

        /// <summary>转换为 16 位无符号整数（负数按 <see cref="Convert.ToUInt16(short)"/> 处理）。</summary>
        public ushort ToUShort() => Convert.ToUInt16(Value);

        /// <summary>转换为 32 位无符号整数（负数按 <see cref="Convert.ToUInt32(short)"/> 处理）。</summary>
        public uint ToUInt() => Convert.ToUInt32(Value);

        /// <summary>转换为 64 位无符号整数（负数按 <see cref="Convert.ToUInt64(short)"/> 处理）。</summary>
        public ulong ToULong() => Convert.ToUInt64(Value);

        /// <summary>转换为 8 位无符号整数（可能截断/溢出）。</summary>
        public byte ToByte() => Convert.ToByte(Value);

        /// <summary>转换为 8 位有符号整数（可能截断/溢出）。</summary>
        public sbyte ToSByte() => Convert.ToSByte(Value);

        /// <summary>从 32 位有符号整数创建 HShort（可能截断为 16 位）。</summary>
        public static HShort FromInt(int d) => new HShort(Convert.ToInt16(d));

        /// <summary>从 32 位无符号整数创建 HShort（可能截断/溢出）。</summary>
        public static HShort FromUInt(uint d) => new HShort(Convert.ToInt16(d));

        /// <summary>从 64 位有符号整数创建 HShort（可能截断为 16 位）。</summary>
        public static HShort FromLong(long d) => new HShort(Convert.ToInt16(d));

        /// <summary>从 64 位无符号整数创建 HShort（可能截断/溢出）。</summary>
        public static HShort FromULong(ulong d) => new HShort(Convert.ToInt16(d));

        /// <summary>从单精度浮点数创建 HShort（遵循 <see cref="Convert.ToInt16(float)"/> 取整规则）。</summary>
        public static HShort FromSingle(float d) => new HShort(Convert.ToInt16(d));

        /// <summary>从双精度浮点数创建 HShort（遵循 <see cref="Convert.ToInt16(double)"/> 取整规则）。</summary>
        public static HShort FromDouble(double d) => new HShort(Convert.ToInt16(d));

        #endregion

        #region 算术运算符重载
        /// <summary>运算符 +：用于 加法。</summary>
        public static HShort operator +(HShort a, HShort b) => new HShort((short)(a.Value + b.Value));
        /// <summary>运算符 -：用于 减法。</summary>
        public static HShort operator -(HShort a, HShort b) => new HShort((short)(a.Value - b.Value));
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HShort operator *(HShort a, HShort b) => new HShort((short)(a.Value * b.Value));
        /// <summary>运算符 /：用于 除法。</summary>
        public static HShort operator /(HShort a, HShort b) => new HShort((short)(a.Value / b.Value));
        /// <summary>运算符 %：用于 取模。</summary>
        public static HShort operator %(HShort a, HShort b) => new HShort((short)(a.Value % b.Value));
        /// <summary>运算符 -：用于 减法。</summary>
        public static HShort operator -(HShort a) => new HShort((short)(-a.Value));
        public static HShort operator ++(HShort a) => new HShort((short)(a.Value + 1));
        public static HShort operator --(HShort a) => new HShort((short)(a.Value - 1));

        /// <summary>按位与运算符：a 的每一位与 b 对应位相与。</summary>
        public static HShort operator &(HShort a, HShort b) => new HShort((short)(a.Value & b.Value));

        /// <summary>按位或运算符：a 的每一位与 b 对应位相或。</summary>
        public static HShort operator |(HShort a, HShort b) => new HShort((short)(a.Value | b.Value));

        /// <summary>按位异或运算符：a 的每一位与 b 对应位相异或（不同为 1）。</summary>
        public static HShort operator ^(HShort a, HShort b) => new HShort((short)(a.Value ^ b.Value));

        /// <summary>按位取反运算符：将 a 的每一位取反。</summary>
        public static HShort operator ~(HShort a) => new HShort((short)(~a.Value));

        #endregion

        #region 比较运算符重载
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HShort a, HShort b) => a.Value == b.Value;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HShort a, HShort b) => a.Value != b.Value;
        /// <summary>运算符 >：用于 大于。</summary>
        public static bool operator >(HShort a, HShort b) => a.Value > b.Value;
        /// <summary>运算符 <：用于 小于。</summary>
        public static bool operator <(HShort a, HShort b) => a.Value < b.Value;
        public static bool operator >=(HShort a, HShort b) => a.Value >= b.Value;
        public static bool operator <=(HShort a, HShort b) => a.Value <= b.Value;

        /// <summary>条件判断支持：值非零时为真，零时为假。允许 if(HShort) ...</summary>
        public static bool operator true(HShort a) => a.Value != 0;

        /// <summary>条件判断支持：值为零时为假，非零时为真。</summary>
        public static bool operator false(HShort a) => a.Value == 0;
        #endregion

        #region 接口实现
        public bool Equals(HShort other) => Value == other.Value;
        public override bool Equals(object obj) => obj is HShort h && Equals(h);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(HShort other) => Value.CompareTo(other.Value);

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
        /// 尝试将数字的字符串表示形式转换为等效的 HShort。返回指示转换是否成功的值。
        /// </summary>
        public static bool TryParse(string s, out HShort result)
        {
            if (short.TryParse(s, out short sh))
            {
                result = new HShort(sh);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 将数字的字符串表示形式转换为等效的 HShort。
        /// </summary>
        public static HShort Parse(string s) => new HShort(short.Parse(s));
        #endregion

        #region 随机数生成（线程安全，无锁）
        /// <summary>
        /// 每个线程拥有独立的 <see cref="Random"/> 实例，使用 <see cref="Guid"/> 哈希值作为种子，
        /// 避免多线程下的锁争用与序列重复。
        /// </summary>
        private static readonly ThreadLocal<Random> ThreadRandom =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// 返回一个在 [<paramref name="min"/>, <paramref name="max"/>) 区间内的随机 <see cref="HShort"/>。
        /// 若 <paramref name="min"/> 大于 <paramref name="max"/>，会自动交换二者。
        /// 由于 short 范围较小（-32768~32767），始终在 int 安全范围内，不会溢出。
        /// </summary>
        /// <param name="min">随机数的下限（包含）。</param>
        /// <param name="max">随机数的上限（不包含）。</param>
        /// <returns>包含随机值的 HShort 实例。</returns>
        public static HShort Random(short min, short max)
        {
            if (min > max)
                (min, max) = (max, min);

            if (min == max)
                return new HShort(min);

            // 直接使用 int 范围调用 Random.Next，然后转换为 short
            return new HShort((short)ThreadRandom.Value.Next(min, max));
        }

        /// <summary>
        /// 使用指定的 <see cref="Random"/> 实例生成 [<paramref name="min"/>, <paramref name="max"/>) 内的随机数。
        /// 适合需要可重复随机序列或自定义随机源的场景。
        /// </summary>
        /// <param name="min">下限（包含）。</param>
        /// <param name="max">上限（不包含）。</param>
        /// <param name="random">非 null 的 Random 实例。</param>
        /// <returns>随机 HShort。</returns>
        /// <exception cref="ArgumentNullException">random 为 null。</exception>
        public static HShort Random(short min, short max, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            if (min > max)
                (min, max) = (max, min);

            if (min == max)
                return new HShort(min);

            return new HShort((short)random.Next(min, max));
        }
        #endregion
    }

    /// <summary>
    /// 为 HShort 提供与字符串之间的类型转换，供 PropertyGrid 等设计时环境使用。
    /// </summary>
    public class HShortConverter : TypeConverter
    {
        /// <summary>CanConvertFrom 方法。</summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <summary>CanConvertTo 方法。</summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        /// <summary>zxMessageShow 字段。</summary>
        private HMessageA zxMessageShow = new HMessageA();

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
                            HTranslation.GetContent("无法将 {0} 转换为 Short。输入为null,请正确写数字！", str),
                            HTranslation.GetContent("输入值错误"),
                            0,
                            HTranslation.GetContent("确认"));
                    }
                    return old;
                }
                else
                {
                    if (short.TryParse(str, NumberStyles.Integer | NumberStyles.AllowThousands,
                            culture.NumberFormat, out short result))
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
                        return new HShort(result);
                    }
                    if (!zxMessageShow.IsShow)
                    {
                        zxMessageShow.ZzShowDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 Short。请正确写数字！", str),
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
        private HShort GetOldValue(ITypeDescriptorContext context)
        {
            if (context?.PropertyDescriptor != null && context.Instance != null)
            {
                object val = context.PropertyDescriptor.GetValue(context.Instance);
                if (val is HShort hs) return hs;
            }
            return HShort.Zero;
        }

        /// <summary>ConvertTo 方法。</summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is HShort hs)
            {
                return hs.Value.ToString(culture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
