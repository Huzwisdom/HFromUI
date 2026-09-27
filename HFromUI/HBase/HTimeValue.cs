using HFromUI;
using System;
using System.Collections.Generic;
using System.Linq;
using HFromUI.HMath;

namespace HFromUI.HBase
{

/// <summary>
/// 轻量级时间-数据点结构，用于图表绑定。
/// 仅包含 Y 轴数据值和实际时间点，时间间隔通过方法按需计算，返回值统一为 HDouble 类型。
/// </summary>
public struct HTimeValue : IEquatable<HTimeValue>, IComparable<HTimeValue>
{
    #region 属性

    /// <summary>
    /// Y 轴数据值（例如测量值、价格等）。
    /// </summary>
    public HDouble Value { get; set; }

    /// <summary>
    /// 实际时间点，用于生成 X 轴标签。
    /// </summary>
    public DateTime DateTime { get; set; }

    #endregion

    #region 构造函数

    /// <summary>
    /// 同时指定 Y 轴数据值和实际时间点。
    /// </summary>
    /// <param name="value">Y 轴数值</param>
    /// <param name="dateTime">实际时间点</param>
    public HTimeValue(HDouble value, DateTime dateTime)
    {
        Value = value;
        DateTime = dateTime;
    }
    public HTimeValue(HDouble value)
    {
        Value = value;
        DateTime = DateTime.Now;
    }
    /// <summary>
    /// 快捷构造：仅指定时间点，Y 值默认为 0。
    /// </summary>
    /// <param name="dateTime">实际时间点</param>
    public HTimeValue(DateTime dateTime) : this(HDouble.Zero, dateTime)
    {
    }

    #endregion

    #region 时间间隔计算方法（返回值类型均为 HDouble）


    /// <summary>获取 hTotalMilliseconds。</summary>
    public HDouble GetHTotalMilliseconds()
    {
        return (DateTime.Now- DateTime ).TotalMilliseconds;
    }
    /// <summary>
    /// 获取与指定参考时间的时间间隔（毫秒），返回 HDouble。
    /// </summary>
    /// <param name="reference">参考时间</param>
    /// <returns>当前时间 - 参考时间的总毫秒数，HDouble 类型</returns>
    public HDouble GetTotalMilliseconds(DateTime reference) =>
        (DateTime - reference).TotalMilliseconds;



    /// <summary>获取 hTotalSeconds。</summary>
    public HDouble GetHTotalSeconds()
    {
        return (DateTime.Now - DateTime).TotalSeconds;
    }
    /// <summary>
    /// 获取与指定参考时间的时间间隔（秒），返回 HDouble。
    /// </summary>
    public HDouble GetTotalSeconds(DateTime reference) =>
        (DateTime - reference).TotalSeconds;
    /// <summary>获取 hTotalMinutes。</summary>
    public HDouble GetHTotalMinutes()
    {
        return (DateTime.Now - DateTime).TotalMinutes;
    }
    /// <summary>
    /// 获取与指定参考时间的时间间隔（分钟），返回 HDouble。
    /// </summary>
    public HDouble GetTotalMinutes(DateTime reference) =>
        (DateTime - reference).TotalMinutes;

    /// <summary>
    /// 获取与指定参考时间的时间间隔（小时），返回 HDouble。
    /// </summary>
    public HDouble GetTotalHours(DateTime reference) =>
        (DateTime - reference).TotalHours;

    /// <summary>
    /// 获取与指定参考时间的时间间隔（天），返回 HDouble。
    /// </summary>
    public HDouble GetTotalDays(DateTime reference) =>
        (DateTime - reference).TotalDays;

    /// <summary>
    /// 获取与指定参考时间的 TimeSpan 间隔。
    /// </summary>
    public TimeSpan GetTimeSpan(DateTime reference) =>
        DateTime - reference;

    /// <summary>
    /// 计算与另一个 HTimeValue 之间的时间间隔（毫秒），返回 HDouble。
    /// </summary>
    public HDouble GetMillisecondsTo(HTimeValue other) =>
        (other.DateTime - DateTime).TotalMilliseconds;

    /// <summary>
    /// 计算与另一个 HTimeValue 之间的 TimeSpan 间隔。
    /// </summary>
    public TimeSpan GetTimeSpanTo(HTimeValue other) =>
        other.DateTime - DateTime;

    #endregion

    #region 静态工具方法（返回值类型统一为 HDouble）

    /// <summary>
    /// 将 HTimeValue 序列转换为 X 轴数值数组（毫秒偏移），以第一个元素的时间为参考点。
    /// 所有偏移值均为 HDouble 类型。
    /// </summary>
    public static HDouble[] ToXOffsetsMilliseconds(IList<HTimeValue> points)
    {
        if (points == null || points.Count == 0)
            return Array.Empty<HDouble>();
        var reference = points[0].DateTime;
        return points.Select(p => (HDouble)(p.DateTime - reference).TotalMilliseconds).ToArray();
    }

    /// <summary>
    /// 将 HTimeValue 序列转换为 X 轴数值数组，使用指定参考时间计算毫秒偏移，返回 HDouble 数组。
    /// </summary>
    public static HDouble[] ToXOffsetsMilliseconds(IList<HTimeValue> points, DateTime reference)
    {
        if (points == null)
            return Array.Empty<HDouble>();
        return points.Select(p => (HDouble)(p.DateTime - reference).TotalMilliseconds).ToArray();
    }

    /// <summary>
    /// 将 HTimeValue 序列的 DateTime 格式化为字符串数组，用于 X 轴标签。
    /// </summary>
    /// <param name="points">数据点集合</param>
    /// <param name="format">时间格式，默认为 "yyyy-MM-dd HH:mm:ss.fff"（显示毫秒）</param>
    public static string[] ToXLabels(IList<HTimeValue> points, string format = "yyyy-MM-dd HH:mm:ss.fff")
    {
        if (points == null)
            return Array.Empty<string>();
        return points.Select(p => p.DateTime.ToString(format)).ToArray();
    }

    #endregion

    #region 格式化与显示

    /// <summary>
    /// 返回实际时间的默认字符串表示（显示毫秒）。
    /// </summary>
    public string ToDateTimeString() => DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

    /// <summary>
    /// 使用自定义格式返回实际时间字符串。
    /// </summary>
    public string ToDateTimeString(string format) => DateTime.ToString(format);
    /// <summary>转换为 HString。</summary>
    public string ToHString()
    {
        return DateTime.ToString("yyyy-MM-dd")+"\r\n"+ DateTime.ToString("HH:mm:ss");
    }
    /// <summary>
    /// 调试用字符串，同时显示时间和 Y 值。
    /// </summary>
    public override string ToString() =>
        $"[{DateTime:yyyy-MM-dd HH:mm:ss.fff}] Y={Value}";

    #endregion

    #region 类型转换（约定：仅保留唯一一个隐式转换）

    /// <summary>
    /// 唯一保留的隐式转换：将 HTimeValue 转为其时间点 <see cref="DateTime"/>。
    /// 取 Y 轴数值请使用 <see cref="Value"/> 属性；
    /// 需要其他数值类型时请使用下面的 ToXxx() 方法显式转换。
    /// </summary>
    public static implicit operator DateTime(HTimeValue htv) => htv.DateTime;

    // 以下原隐式转换已删除，改用对应成员：
    //   HDouble/double(HTimeValue) → 直接读取 Value 属性；
    //   float/int/uint/short/ushort/long/ulong(HTimeValue) → 下面的 ToXxx() 方法。

    /// <summary>以 <see cref="double"/> 形式获取 Y 轴数值。</summary>
    public double ToDouble() => Value.Value;

    /// <summary>转换为单精度浮点数。</summary>
    public float ToSingle() => Value.ToSingle();

    /// <summary>转换为 32 位有符号整数（遵循 <see cref="Convert.ToInt32(double)"/> 取整规则）。</summary>
    public int ToInt() => Value.ToInt();

    /// <summary>转换为 32 位无符号整数。</summary>
    public uint ToUInt() => Value.ToUInt();

    /// <summary>转换为 16 位有符号整数。</summary>
    public short ToShort() => Value.ToShort();

    /// <summary>转换为 16 位无符号整数。</summary>
    public ushort ToUShort() => Value.ToUShort();

    /// <summary>转换为 64 位有符号整数。</summary>
    public long ToLong() => Value.ToLong();

    /// <summary>转换为 64 位无符号整数。</summary>
    public ulong ToULong() => Value.ToULong();

    #endregion

    #region 比较与相等

    public bool Equals(HTimeValue other) =>
        Value.Equals(other.Value) && DateTime.Equals(other.DateTime);

    public override bool Equals(object obj) => obj is HTimeValue other && Equals(other);

    /// <summary>
    /// 计算哈希值，兼容 .NET Framework 等无 HashCode.Combine 的环境。
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Value.GetHashCode();
            hash = hash * 23 + DateTime.GetHashCode();
            return hash;
        }
    }

    public int CompareTo(HTimeValue other)
    {
        int cmp = DateTime.CompareTo(other.DateTime);
        if (cmp != 0) return cmp;
        return Value.CompareTo(other.Value);
    }

    /// <summary>运算符 ==：用于 相等判定。</summary>
    public static bool operator ==(HTimeValue left, HTimeValue right) => left.Equals(right);
    /// <summary>运算符 !=：用于 不等判定。</summary>
    public static bool operator !=(HTimeValue left, HTimeValue right) => !left.Equals(right);
    /// <summary>运算符 <：用于 小于。</summary>
    public static bool operator <(HTimeValue left, HTimeValue right) => left.CompareTo(right) < 0;
    /// <summary>运算符 >：用于 大于。</summary>
    public static bool operator >(HTimeValue left, HTimeValue right) => left.CompareTo(right) > 0;
    public static bool operator <=(HTimeValue left, HTimeValue right) => left.CompareTo(right) <= 0;
    public static bool operator >=(HTimeValue left, HTimeValue right) => left.CompareTo(right) >= 0;

    #endregion
}
}
