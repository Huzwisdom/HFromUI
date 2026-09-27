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
    [Serializable]
    [TypeConverter(typeof(HDoubleConverter))]
    /// <summary>
    /// 一个高性能、线程安全、功能完备的双精度浮点数包装器。
    /// 提供算术运算符、比较运算符、常用常量以及无锁的线程安全随机数生成。
    /// Value 属性为可读可写，便于灵活使用。
    /// </summary>
    public struct HDouble : IEquatable<HDouble>, IComparable<HDouble>, IFormattable
    {
        #region 静态常量
        /// <summary>表示数字 0 的 HDouble。</summary>
        public static readonly HDouble Zero = new HDouble(0.0);
        /// <summary>表示数字 1 的 HDouble。</summary>
        public static readonly HDouble One = new HDouble(1.0);
        /// <summary>表示负一的 HDouble。</summary>
        public static readonly HDouble MinusOne = new HDouble(-1.0);
        /// <summary>HDouble 的最小可能正值。</summary>
        public static readonly HDouble Epsilon = new HDouble(double.Epsilon);
        /// <summary>HDouble 的最小可能值。</summary>
        public static readonly HDouble MinValue = new HDouble(double.MinValue);
        /// <summary>HDouble 的最大可能值。</summary>
        public static readonly HDouble MaxValue = new HDouble(double.MaxValue);
        /// <summary>表示非数字 (NaN) 的 HDouble。</summary>
        public static readonly HDouble NaN = new HDouble(double.NaN);
        /// <summary>表示正无穷大的 HDouble。</summary>
        public static readonly HDouble PositiveInfinity = new HDouble(double.PositiveInfinity);
        /// <summary>表示负无穷大的 HDouble。</summary>
        public static readonly HDouble NegativeInfinity = new HDouble(double.NegativeInfinity);


        #endregion

        #region 构造与属性
        /// <summary>获取或设置此实例封装的 <see cref="double"/> 值。</summary>
        public double Value { get; set; }

        /// <summary>获取一个值，指示当前值是否为有效数字（既非 NaN 也非无穷）。</summary>
        public bool IsValid => !double.IsNaN(Value) && !double.IsInfinity(Value);

        /// <summary>
        /// 使用指定的 <see cref="double"/> 值初始化 HDouble 的新实例。
        /// </summary>
        /// <param name="value">双精度浮点数。</param>
        public HDouble(double value)
        {
            Value = value;
        }
        public HDouble(double value, int decimals)
        {
            Value = Math.Round(value, decimals);
        }
        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将原生 <see cref="double"/> 装箱为 HDouble。
        /// 其余方向的转换一律使用 <see cref="Value"/> 属性或 ToXxx() 方法显式完成。
        /// </summary>
        /// <param name="d">要封装的双精度浮点值。</param>
        public static implicit operator HDouble(double d) => new HDouble(d);

        // 以下原隐式转换已删除（float/int/uint/ushort/short/ulong/long → HDouble）：
        // 这些数值到 double 本身就是 C# 内置隐式转换，直接写 new HDouble(x) 即可。

        /// <summary>以 <see cref="double"/> 形式获取封装值（等价于 <see cref="Value"/>）。</summary>
        public double ToDouble() => Value;

        /// <summary>转换为 32 位有符号整数（四舍六入五取整，遵循 <see cref="Convert.ToInt32(double)"/> 规则）。</summary>
        public int ToInt()
        {
            return Convert.ToInt32(Value);
        }

        /// <summary>转换为 32 位无符号整数。</summary>
        public uint ToUInt()
        {
            return Convert.ToUInt32(Value);
        }

        /// <summary>转换为单精度浮点数。</summary>
        public float ToSingle()
        {
            return Convert.ToSingle(Value);
        }

        /// <summary>转换为 16 位有符号整数。</summary>
        public short ToShort()
        {
            return Convert.ToInt16(Value);
        }

        /// <summary>转换为 16 位无符号整数。</summary>
        public ushort ToUShort()
        {
            return Convert.ToUInt16(Value);
        }

        /// <summary>转换为 64 位有符号整数。</summary>
        public long ToLong()
        {
            return Convert.ToInt64(Value);
        }

        /// <summary>转换为 64 位无符号整数。</summary>
        public ulong ToULong()
        {
            return Convert.ToUInt64(Value);
        }

        /// <summary>转换为十进制数 <see cref="decimal"/>。</summary>
        public decimal ToDecimal()
        {
            return Convert.ToDecimal(Value);
        }

        /// <summary>转换为 8 位无符号整数。</summary>
        public byte ToByte()
        {
            return Convert.ToByte(Value);
        }

        /// <summary>转换为 8 位有符号整数。</summary>
        public sbyte ToSByte()
        {
            return Convert.ToSByte(Value);
        }

        /// <summary>从单精度浮点数创建 HDouble。</summary>
        /// <param name="d">单精度浮点值。</param>
        public static HDouble FromSingle(float d) => new HDouble(Convert.ToDouble(d));

        #endregion
        public HDouble SetValue(double x,int decimals)
        {
            return Value = Math.Round(x, decimals);
        }
        public HDouble SetValue( int decimals)
        {
             return  Value = Math.Round(Value, decimals);
        }
        #region 算术运算符重载
        /// <summary>运算符 +：用于 加法。</summary>
        public static HDouble operator +(HDouble a, HDouble b) => new HDouble(a.Value + b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HDouble operator -(HDouble a, HDouble b) => new HDouble(a.Value - b.Value);
        /// <summary>运算符 *：用于 乘法。</summary>
        public static HDouble operator *(HDouble a, HDouble b) => new HDouble(a.Value * b.Value);
        /// <summary>运算符 /：用于 除法。</summary>
        public static HDouble operator /(HDouble a, HDouble b) => new HDouble(a.Value / b.Value);
        /// <summary>取模运算符：返回 a 除以 b 的余数。</summary>
        public static HDouble operator %(HDouble a, HDouble b) => new HDouble(a.Value % b.Value);
        /// <summary>运算符 -：用于 减法。</summary>
        public static HDouble operator -(HDouble a) => new HDouble(-a.Value);
        #endregion

        #region 比较运算符重载
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HDouble a, HDouble b) => a.Value.Equals(b.Value);
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HDouble a, HDouble b) => !a.Value.Equals(b.Value);
        /// <summary>运算符 >：用于 大于。</summary>
        public static bool operator >(HDouble a, HDouble b) => a.Value > b.Value;
        /// <summary>运算符 <：用于 小于。</summary>
        public static bool operator <(HDouble a, HDouble b) => a.Value < b.Value;
        public static bool operator >=(HDouble a, HDouble b) => a.Value >= b.Value;
        public static bool operator <=(HDouble a, HDouble b) => a.Value <= b.Value;

        /// <summary>条件判断支持：值非零时为真，零时为假。允许直接写 if(hDouble) ...</summary>
        /// <remarks>注意：不要用它替代显式的 if(Math.Abs(x) &lt; 1e-9) 来判断是否接近零（浮点精度问题）。</remarks>
        public static bool operator true(HDouble a) => a.Value != 0;

        /// <summary>条件判断支持：值为零时为假，非零时为真。</summary>
        public static bool operator false(HDouble a) => a.Value == 0;

        /// <summary>自增运算符：值加 1。对属性使用时请通过临时变量，避免 CS1612。</summary>
        public static HDouble operator ++(HDouble a) => new HDouble(a.Value + 1.0);

        /// <summary>自减运算符：值减 1。</summary>
        public static HDouble operator --(HDouble a) => new HDouble(a.Value - 1.0);

        /// <summary>按位与运算符：将两个 double 的 IEEE 754 位模式相与。</summary>
        /// <remarks>
        /// 浮点数原生 C# 不支持位运算，这里通过 BitConverter 将位模式转成 long 进行运算，
        /// 再转回 double。常用于位掩码操作或特殊标记位处理（不要用于普通数值计算）。
        /// </remarks>
        public static HDouble operator &(HDouble a, HDouble b) =>
            new HDouble(BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(a.Value) & BitConverter.DoubleToInt64Bits(b.Value)));

        /// <summary>按位或运算符：将两个 double 的 IEEE 754 位模式相或。</summary>
        public static HDouble operator |(HDouble a, HDouble b) =>
            new HDouble(BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(a.Value) | BitConverter.DoubleToInt64Bits(b.Value)));

        /// <summary>按位异或运算符：将两个 double 的 IEEE 754 位模式相异或。</summary>
        public static HDouble operator ^(HDouble a, HDouble b) =>
            new HDouble(BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(a.Value) ^ BitConverter.DoubleToInt64Bits(b.Value)));

        /// <summary>按位取反运算符：将 double 的 IEEE 754 位模式每一位取反。</summary>
        public static HDouble operator ~(HDouble a) =>
            new HDouble(BitConverter.Int64BitsToDouble(~BitConverter.DoubleToInt64Bits(a.Value)));

        #endregion

        #region 接口实现
        public bool Equals(HDouble other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is HDouble h && Equals(h);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(HDouble other) => Value.CompareTo(other.Value);

        /// <summary>
        /// 返回此实例的字符串表示形式。
        /// </summary>
        public override string ToString() => Value.ToString("F5");

        /// <summary>
        /// 使用指定的格式和区域性特定格式信息，将此实例的数值转换为其等效的字符串表示形式。
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(format, formatProvider);
        #endregion

        #region 静态工厂方法 / 解析
        /// <summary>
        /// 尝试将数字的字符串表示形式转换为等效的 HDouble。返回指示转换是否成功的值。
        /// </summary>
        public static bool TryParse(string s, out HDouble result)
        {
            if (double.TryParse(s, out double d))
            {
                result = new HDouble(d);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 将数字的字符串表示形式转换为等效的 HDouble。
        /// </summary>
        public static HDouble Parse(string s) => new HDouble(double.Parse(s));
        #endregion

        #region 随机数生成（线程安全，无锁）
        /// <summary>
        /// 每个线程拥有独立的 <see cref="Random"/> 实例，使用 <see cref="Guid"/> 哈希值作为种子，
        /// 避免多线程下的锁争用与序列重复。
        /// </summary>
        private static readonly ThreadLocal<Random> ThreadRandom =
            new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        /// <summary>
        /// 返回一个在 [<paramref name="min"/>, <paramref name="max"/>) 区间内的随机 <see cref="HDouble"/>。
        /// 若 <paramref name="min"/> 大于 <paramref name="max"/>，会自动交换二者。
        /// 当区间跨度导致溢出（无穷大）时，将回退到安全范围内生成随机数。
        /// </summary>
        /// <param name="min">随机数的下限（包含）。不能为 NaN 或 Infinity。</param>
        /// <param name="max">随机数的上限（不包含）。不能为 NaN 或 Infinity。</param>
        /// <returns>包含随机值的 HDouble 实例。</returns>
        /// <exception cref="ArgumentException">任一参数为 NaN 或 Infinity。</exception>
        public static HDouble Random(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max) ||
                double.IsInfinity(min) || double.IsInfinity(max))
                throw new ArgumentException(HTranslation.GetContent("min 和 max 不能为 NaN 或 Infinity"));

            // 自动交换，确保 min <= max
            if (min > max)
                (min, max) = (max, min);

            double range = max - min;

            // 防止范围溢出为无穷大
            double value;
            if (double.IsInfinity(range) || double.IsNaN(range))
            {
                // 安全范围内的随机数生成（覆盖绝大多数场景）
                double halfRange = double.MaxValue / 2.0;
                double offset = ThreadRandom.Value.NextDouble() * halfRange;
                value = -halfRange + offset * 2.0;
            }
            else
            {
                value = min + ThreadRandom.Value.NextDouble() * range;
            }

            return new HDouble(value);
        }

        /// <summary>
        /// 使用指定的 <see cref="Random"/> 实例生成 [<paramref name="min"/>, <paramref name="max"/>) 内的随机数。
        /// 适合需要可重复随机序列或自定义随机源的场景。
        /// </summary>
        /// <param name="min">下限（包含），不能为 NaN/Infinity。</param>
        /// <param name="max">上限（不包含），不能为 NaN/Infinity。</param>
        /// <param name="random">非 null 的 Random 实例。</param>
        /// <returns>随机 HDouble。</returns>
        /// <exception cref="ArgumentNullException">random 为 null。</exception>
        /// <exception cref="ArgumentException">min 或 max 无效。</exception>
        public static HDouble Random(double min, double max, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (double.IsNaN(min) || double.IsNaN(max) ||
                double.IsInfinity(min) || double.IsInfinity(max))
                throw new ArgumentException(HTranslation.GetContent("min 和 max 不能为 NaN 或 Infinity"));

            if (min > max)
                (min, max) = (max, min);

            double range = max - min;
            double value;
            if (double.IsInfinity(range) || double.IsNaN(range))
            {
                double halfRange = double.MaxValue / 2.0;
                double offset = random.NextDouble() * halfRange;
                value = -halfRange + offset * 2.0;
            }
            else
            {
                value = min + random.NextDouble() * range;
            }
            return new HDouble(value);
        }
        #endregion

        #region 小数位数获取
        /// <summary>
        /// 获取当前数值实际包含的有效小数位数（去除末尾无意义的零）。
        /// 例如：1.23450 → 返回 4；1.23456 → 返回 5；1.0 → 返回 0。
        /// 对于 NaN 或 Infinity，返回 0。
        /// </summary>
        /// <returns>有效小数位数，若没有小数部分则返回 0。</returns>
        public int GetDecimalPlaces()
        {
            if (double.IsNaN(Value) || double.IsInfinity(Value))
                return 0;

            // 优先尝试转换为 decimal，因为 decimal 内部保留了小数位数（scale）
            try
            {
                decimal d = (decimal)Value;
                int[] bits = decimal.GetBits(d);
                // 第 4 个 int 的第 16~23 位存储了 scale（0~28）
                int scale = (bits[3] >> 16) & 0x7F;
                return scale;
            }
            catch (OverflowException)
            {
                // 若数值超出 decimal 范围（极大值），回退到字符串解析
                return GetDecimalPlacesFromString();
            }
        }
        /// <summary>获取 decimalPlaces。</summary>
        public int GetDecimalPlaces(int max)
        {
            if (double.IsNaN(Value) || double.IsInfinity(Value))
                return 0;

            // 优先尝试转换为 decimal，因为 decimal 内部保留了小数位数（scale）
            try
            {
                decimal d = (decimal)Value;
                int[] bits = decimal.GetBits(d);
                // 第 4 个 int 的第 16~23 位存储了 scale（0~28）
                int scale = (bits[3] >> 16) & 0x7F;
                if (scale> max)
                {
                    return max;
                }
                return scale;
            }
            catch
            {
                return max;
            }
        }
        /// <summary>
        /// 通过高精度字符串解析获取小数位数（备用方案）。
        /// </summary>
        private int GetDecimalPlacesFromString()
        {
            // 使用 "G17" 保证完整精度，并使用固定区域性避免逗号干扰
            string str = Value.ToString("G17", CultureInfo.InvariantCulture);
            int index = str.IndexOf('.');
            if (index == -1)
                return 0;
            // 去除末尾的 '0'，得到真正有效的小数位数
            string fractional = str.Substring(index + 1).TrimEnd('0');
            return fractional.Length;
        }
        #endregion

        /// <summary>
        /// 将当前数值向上取整到下一个10的整数次幂（数量级）。
        /// 例如：5 → 10，50 → 100，500 → 1000，0.5 → 1，-5 → -10（负数的处理见备注）。
        /// 对于 NaN 或 Infinity，返回原值。
        /// </summary>
        /// <returns>映射后的 HDouble 值</returns>
        public HDouble ToNextPowerOfTen()
        {
            if (double.IsNaN(Value) || double.IsInfinity(Value))
                return this;

            // 对负数取绝对值处理，然后再恢复符号（可选，根据您的业务决定）
            double absValue = Math.Abs(Value);
            if (absValue == 0)
                return HDouble.Zero;

            // 计算向上取整的指数：floor(log10(absValue)) + 1
            double exponent = Math.Floor(Math.Log10(absValue)) + 1;
            double result = Math.Pow(10, exponent);

            // 恢复符号（如果原始值为负，结果也为负）
            if (Value < 0)
                result = -result;

            return new HDouble(result);
        }



        /// <summary>
        /// 将当前值按指定的单位向下截断（不四舍五入），返回新的 HDouble。
        /// 例如：999.99999 以 10 为单位截断 → 990；以 0.001 为单位截断 → 999.999。
        /// </summary>
        /// <param name="unit">截断单位，如 100、10、0.001 等，必须大于 0</param>
        /// <returns>截断后的 HDouble</returns>
        public HDouble TruncateByUnit(double unit)
        {
            if (unit <= 0)
                throw new ArgumentException(HTranslation.GetContent("单位必须大于零"), nameof(unit));

            // 使用 decimal 保证精度，避免浮点除法/乘法误差
            try
            {
                decimal decValue = (decimal)Value;
                decimal decUnit = (decimal)unit;
                decimal truncated = Math.Truncate(decValue / decUnit) * decUnit;
                return new HDouble((double)truncated);
            }
            catch (OverflowException)
            {
                // 极值回退到 double 计算（精度可能略有损失，但足够覆盖日常场景）
                double truncated = Math.Truncate(Value / unit) * unit;
                return new HDouble(truncated);
            }
        }
    }



    public class HDoubleConverter : TypeConverter
    {
        // 允许从 string 转换为 HDouble
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        // 允许将 HDouble 转换为 string
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);


        ZvMessageShow zxMessageShow = new ZvMessageShow();
        // 从 string 解析为 HDouble
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                object old = GetOldValue(context);
                // 尝试解析数字字符串，支持空字符串转为 0
                if (string.IsNullOrWhiteSpace(str))
                {
                    if (!zxMessageShow.IsShow)
                    {
                        DialogResult dialogResult = zxMessageShow.ZzShowDialog(HTranslation.GetContent("无法将 {0} 转换为 Double。输入为null,请正确写数字！",str), HTranslation.GetContent("输入值错误"), 0, HTranslation.GetContent("确认"));
                    }
                }
                else
                {
                    if (double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands,
                            culture.NumberFormat, out double result))
                    {
                        if (!string.IsNullOrWhiteSpace(HAppData.Name))
                        {
                            // 1. 被编辑的对象（宿主对象）
                            object instance = context?.Instance;
                            string instanceTypeName = instance?.GetType().FullName;

                            // 2. 正在编辑的属性描述符
                            PropertyDescriptor prop = context?.PropertyDescriptor;
                            string propertyName = prop?.Name;
                            Type propertyType = prop?.PropertyType;

                            // 3. 拼接完整的“对象.属性”标识
                            string fullPath = $"{instanceTypeName}.{propertyName}";
                            HAppData.Log( fullPath + HTranslation.GetContent(">>用户[{0}]输入：",HAppData.Name) + str);
                        }

                        return new HDouble(result);
                    }
                    if (!zxMessageShow.IsShow)
                    {
                        DialogResult dialogResult = zxMessageShow.ZzShowDialog(HTranslation.GetContent("无法将 {0} 转换为 Double。请正确写数字！",str), HTranslation.GetContent("输入值错误"), 0, HTranslation.GetContent("确认"));

                    }
                }

                return old;


            }
            return base.ConvertFrom(context, culture, value);
        }
        /// <summary>获取 oldValue。</summary>
        private HDouble GetOldValue(ITypeDescriptorContext context)
        {
            if (context?.PropertyDescriptor != null && context.Instance != null)
            {
                object val = context.PropertyDescriptor.GetValue(context.Instance);
                if (val is HDouble hd) return hd;
            }
            return HDouble.Zero;
        }
        // 将 HDouble 转为显示用的字符串
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is HDouble hd)
            {
                // 使用当前的 culture 格式化，也可固定格式，如 "G"
               return hd.Value.ToString(culture);
                // return hd.Value.ToString("F5");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
