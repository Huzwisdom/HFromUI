using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using HFromUI.HControl.Tools.Message;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    using HFromUI.HData;
    [TypeConverter(typeof(HULongConverter))]
    /// <summary>
    /// 一个高性能、线程安全、功能完备的64位无符号整数包装器。
    /// 提供算术运算符、比较运算符、常用常量以及无锁的线程安全随机数生成。
    /// Value 属性为可读可写，便于灵活使用。
    /// </summary>
    public struct HULong : IEquatable<HULong>, IComparable<HULong>, IFormattable
    {
        #region 静态常量
        /// <summary>表示数字 0 的 HULong。</summary>
        public static readonly HULong Zero = new HULong(0uL);
        /// <summary>表示数字 1 的 HULong。</summary>
        public static readonly HULong One = new HULong(1uL);
        /// <summary>HULong 的最小可能值（0）。</summary>
        public static readonly HULong MinValue = new HULong(ulong.MinValue);
        /// <summary>HULong 的最大可能值。</summary>
        public static readonly HULong MaxValue = new HULong(ulong.MaxValue);
        #endregion

        #region 构造与属性
        /// <summary>获取或设置此实例封装的 <see cref="ulong"/> 值。</summary>
        public ulong Value { get; set; }

        /// <summary>获取一个值，指示当前值是否有效。对于 HULong，所有 ulong 值均有效。</summary>
        public bool IsValid => true; // 保持接口统一

        /// <summary>
        /// 使用指定的 <see cref="ulong"/> 值初始化 HULong 的新实例。
        /// </summary>
        /// <param name="value">无符号64位整数。</param>
        public HULong(ulong value)
        {
            Value = value;
        }
        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将原生 <see cref="ulong"/> 装箱为 HULong。
        /// 其余方向的转换一律使用 <see cref="Value"/> 属性或 ToXxx()/FromXxx() 方法显式完成。
        /// </summary>
        /// <param name="d">要封装的 64 位无符号整数。</param>
        public static implicit operator HULong(ulong d) => new HULong(d);

        // 以下原隐式转换已删除，改用对应方法：
        //   ulong(HULong) → 直接读取 Value 属性；
        //   float/double/decimal/long/uint/int/ushort/short/byte/sbyte(HULong) → 下面的 ToXxx() 方法；
        //   HULong(uint/ushort/byte) 可直接 new HULong(x)（C# 内置到 ulong 的隐式转换）；
        //   HULong(long/int/short/sbyte/float/double/decimal) → 下面的 FromXxx() 静态方法。

        /// <summary>转换为单精度浮点数（极大整数可能丢失精度）。</summary>
        public float ToSingle() => Value;

        /// <summary>转换为双精度浮点数（极大整数可能丢失精度）。</summary>
        public double ToDouble() => Value;

        /// <summary>转换为十进制数 <see cref="decimal"/>。</summary>
        public decimal ToDecimal() => Value;

        /// <summary>转换为 64 位有符号整数（超出 long 范围时按 <see cref="Convert.ToInt64(ulong)"/> 处理）。</summary>
        public long ToLong() => Convert.ToInt64(Value);

        /// <summary>转换为 32 位无符号整数（可能截断）。</summary>
        public uint ToUInt() => Convert.ToUInt32(Value);

        /// <summary>转换为 32 位有符号整数（可能截断/溢出）。</summary>
        public int ToInt() => Convert.ToInt32(Value);

        /// <summary>转换为 16 位无符号整数（可能截断）。</summary>
        public ushort ToUShort() => Convert.ToUInt16(Value);

        /// <summary>转换为 16 位有符号整数（可能截断/溢出）。</summary>
        public short ToShort() => Convert.ToInt16(Value);

        /// <summary>转换为 8 位无符号整数（可能截断/溢出）。</summary>
        public byte ToByte() => Convert.ToByte(Value);

        /// <summary>转换为 8 位有符号整数（可能截断/溢出）。</summary>
        public sbyte ToSByte() => Convert.ToSByte(Value);

        /// <summary>从 64 位有符号整数创建 HULong（负数按 <see cref="Convert.ToUInt64(long)"/> 处理）。</summary>
        public static HULong FromLong(long d) => new HULong(Convert.ToUInt64(d));

        /// <summary>从 32 位有符号整数创建 HULong（负数按 <see cref="Convert.ToUInt64(long)"/> 处理）。</summary>
        public static HULong FromInt(int d) => new HULong(Convert.ToUInt64(d));

        /// <summary>从 16 位有符号整数创建 HULong（负数按 <see cref="Convert.ToUInt64(long)"/> 处理）。</summary>
        public static HULong FromShort(short d) => new HULong(Convert.ToUInt64(d));

        /// <summary>从 8 位有符号整数创建 HULong（负数按 <see cref="Convert.ToUInt64(long)"/> 处理）。</summary>
        public static HULong FromSByte(sbyte d) => new HULong(Convert.ToUInt64(d));

        /// <summary>从单精度浮点数创建 HULong（遵循 <see cref="Convert.ToUInt64(float)"/> 取整规则）。</summary>
        public static HULong FromSingle(float d) => new HULong(Convert.ToUInt64(d));

        /// <summary>从双精度浮点数创建 HULong（遵循 <see cref="Convert.ToUInt64(double)"/> 取整规则）。</summary>
        public static HULong FromDouble(double d) => new HULong(Convert.ToUInt64(d));

        /// <summary>从十进制数创建 HULong（遵循 <see cref="Convert.ToUInt64(decimal)"/> 取整规则）。</summary>
        public static HULong FromDecimal(decimal d) => new HULong(Convert.ToUInt64(d));

        #endregion

        #region 算术运算符重载
        /// <summary>运算符 +：用于 加法。</summary>
        public static HULong operator +(HULong a, HULong b) => new HULong(a.Value + b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HULong operator -(HULong a, HULong b) => new HULong(a.Value - b.Value);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HULong operator *(HULong a, HULong b) => new HULong(a.Value * b.Value);
        /// <summary>运算符 /：用于 除法。</summary>
        public static HULong operator /(HULong a, HULong b) => new HULong(a.Value / b.Value);
        /// <summary>运算符 %：用于 取模。</summary>
        public static HULong operator %(HULong a, HULong b) => new HULong(a.Value % b.Value);
        // 无符号类型不提供一元负号运算符
        public static HULong operator ++(HULong a) => new HULong(a.Value + 1);
        public static HULong operator --(HULong a) => new HULong(a.Value - 1);

        /// <summary>按位与运算符：a 的每一位与 b 对应位相与。</summary>
        public static HULong operator &(HULong a, HULong b) => new HULong(a.Value & b.Value);

        /// <summary>按位或运算符：a 的每一位与 b 对应位相或。</summary>
        public static HULong operator |(HULong a, HULong b) => new HULong(a.Value | b.Value);

        /// <summary>按位异或运算符：a 的每一位与 b 对应位相异或（不同为 1）。</summary>
        public static HULong operator ^(HULong a, HULong b) => new HULong(a.Value ^ b.Value);

        /// <summary>按位取反运算符：将 a 的每一位取反。</summary>
        public static HULong operator ~(HULong a) => new HULong(~a.Value);

        #endregion

        #region 比较运算符重载
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HULong a, HULong b) => a.Value == b.Value;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HULong a, HULong b) => a.Value != b.Value;
        /// <summary>运算符 >：用于 大于。</summary>
        public static bool operator >(HULong a, HULong b) => a.Value > b.Value;
        /// <summary>运算符 <：用于 小于。</summary>
        public static bool operator <(HULong a, HULong b) => a.Value < b.Value;
        public static bool operator >=(HULong a, HULong b) => a.Value >= b.Value;
        public static bool operator <=(HULong a, HULong b) => a.Value <= b.Value;

        /// <summary>条件判断支持：值非零时为真，零时为假。允许 if(HULong) ...</summary>
        public static bool operator true(HULong a) => a.Value != 0;

        /// <summary>条件判断支持：值为零时为假，非零时为真。</summary>
        public static bool operator false(HULong a) => a.Value == 0;
        #endregion

        #region 接口实现
        public bool Equals(HULong other) => Value == other.Value;
        public override bool Equals(object obj) => obj is HULong h && Equals(h);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(HULong other) => Value.CompareTo(other.Value);

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
        /// 尝试将数字的字符串表示形式转换为等效的 HULong。返回指示转换是否成功的值。
        /// </summary>
        public static bool TryParse(string s, out HULong result)
        {
            if (ulong.TryParse(s, out ulong ul))
            {
                result = new HULong(ul);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 将数字的字符串表示形式转换为等效的 HULong。
        /// </summary>
        public static HULong Parse(string s) => new HULong(ulong.Parse(s));
        #endregion

        #region 随机数生成（线程安全，无锁）
        /// <summary>
        /// 每个线程拥有独立的 <see cref="Random"/> 实例，使用 <see cref="Guid"/> 哈希值作为种子，
        /// 避免多线程下的锁争用与序列重复。
        /// </summary>
        private static readonly ThreadLocal<Random> ThreadRandom =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// 返回一个在 [<paramref name="min"/>, <paramref name="max"/>) 区间内的随机 <see cref="HULong"/>。
        /// 若 <paramref name="min"/> 大于 <paramref name="max"/>，会自动交换二者。
        /// 使用 <see cref="Random"/> 的 NextDouble 进行缩放，可能对大范围值产生微小的精度损失。
        /// </summary>
        /// <param name="min">随机数的下限（包含）。</param>
        /// <param name="max">随机数的上限（不包含）。</param>
        /// <returns>包含随机值的 HULong 实例。</returns>
        public static HULong Random(ulong min, ulong max)
        {
            if (min > max)
                (min, max) = (max, min);

            if (min == max)
                return new HULong(min);

            // 使用 double 计算范围，注意 ulong 范围可能使 double 无法精确表示，但业务可接受
            double range = (double)max - (double)min;
            if (range <= 0.0)
                return new HULong(min);

            double sample = ThreadRandom.Value.NextDouble();
            ulong result = min + (ulong)(sample * range);
            return new HULong(result);
        }

        /// <summary>
        /// 使用指定的 <see cref="Random"/> 实例生成 [<paramref name="min"/>, <paramref name="max"/>) 内的随机数。
        /// 适合需要可重复随机序列或自定义随机源的场景。
        /// </summary>
        /// <param name="min">下限（包含）。</param>
        /// <param name="max">上限（不包含）。</param>
        /// <param name="random">非 null 的 Random 实例。</param>
        /// <returns>随机 HULong。</returns>
        /// <exception cref="ArgumentNullException">random 为 null。</exception>
        public static HULong Random(ulong min, ulong max, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            if (min > max)
                (min, max) = (max, min);

            if (min == max)
                return new HULong(min);

            double range = (double)max - (double)min;
            if (range <= 0.0)
                return new HULong(min);

            double sample = random.NextDouble();
            ulong result = min + (ulong)(sample * range);
            return new HULong(result);
        }
        #endregion
    }

    /// <summary>
    /// 为 HULong 提供与字符串之间的类型转换，供 PropertyGrid 等设计时环境使用。
    /// </summary>
    public class HULongConverter : TypeConverter
    {
        /// <summary>CanConvertFrom 方法。</summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <summary>CanConvertTo 方法。</summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        /// <summary>_alertBox 字段。</summary>
        private HAlertBox _alertBox = new HAlertBox();

        /// <summary>ConvertFrom 方法。</summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                object old = GetOldValue(context);
                if (string.IsNullOrWhiteSpace(str))
                {
                    if (!_alertBox.IsShow)
                    {
                        _alertBox.ShowMessageDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 ULong。输入为null,请正确写数字！", str),
                            HTranslation.GetContent("输入值错误"),
                            0,
                            HTranslation.GetContent("确认"));
                    }
                    return old;
                }
                else
                {
                    if (ulong.TryParse(str, NumberStyles.Integer | NumberStyles.AllowThousands,
                            culture.NumberFormat, out ulong result))
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
                        return new HULong(result);
                    }
                    if (!_alertBox.IsShow)
                    {
                        _alertBox.ShowMessageDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 ULong。请正确写数字！", str),
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
        private HULong GetOldValue(ITypeDescriptorContext context)
        {
            if (context?.PropertyDescriptor != null && context.Instance != null)
            {
                object val = context.PropertyDescriptor.GetValue(context.Instance);
                if (val is HULong hul) return hul;
            }
            return HULong.Zero;
        }

        /// <summary>ConvertTo 方法。</summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is HULong hul)
            {
                return hul.Value.ToString(culture);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
