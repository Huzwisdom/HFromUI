using HFromUI.HFrom;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using HFromUI.HFrom.From;
using HFromUI.HControl.Tools.Message;

namespace HFromUI.HMath
{
    using HFromUI.HLangage;
    using HFromUI.HData;
    [TypeConverter(typeof(HBoolConverter))]
    /// <summary>
    /// 一个高性能、线程安全、功能完备的布尔值包装器。
    /// 提供逻辑运算符、比较运算符、常用常量以及隐式转换。
    /// Value 属性为可读可写，便于灵活使用。
    /// </summary>
    public struct HBool : IEquatable<HBool>, IComparable<HBool>, IFormattable
    {
        #region 静态常量
        /// <summary>表示布尔值 true 的 HBool。</summary>
        public static readonly HBool True = new HBool(true);
        /// <summary>表示布尔值 false 的 HBool。</summary>
        public static readonly HBool False = new HBool(false);
        #endregion

        #region 构造与属性
        /// <summary>获取或设置此实例封装的 <see cref="bool"/> 值。</summary>
        public bool Value { get; set; }

        /// <summary>获取一个值，指示当前值是否有效。对于 HBool，所有 bool 值均有效。</summary>
        public bool IsValid => true; // 保持接口统一

        /// <summary>
        /// 使用指定的 <see cref="bool"/> 值初始化 HBool 的新实例。
        /// </summary>
        /// <param name="value">布尔值。</param>
        public HBool(bool value)
        {
            Value = value;
        }
        #endregion

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将原生 <see cref="bool"/> 装箱为 HBool。
        /// 取出布尔值请使用 <see cref="Value"/> 属性；
        /// 直接写在 if/while 等条件表达式中由 true/false 运算符支持，无需隐式转出。
        /// </summary>
        /// <param name="b">要封装的布尔值。</param>
        public static implicit operator HBool(bool b) => new HBool(b);

        // 原隐式转换 bool(HBool) 已删除：需要 bool 时请读取 .Value。
        #endregion

        #region 逻辑与比较运算符重载
        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HBool a, HBool b) => a.Value == b.Value;
        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HBool a, HBool b) => a.Value != b.Value;

        /// <summary>比较运算符：a 大于 b（仅当 a=true 且 b=false）。</summary>
        public static bool operator >(HBool a, HBool b) => a.Value && !b.Value;

        /// <summary>比较运算符：a 小于 b（仅当 a=false 且 b=true）。</summary>
        public static bool operator <(HBool a, HBool b) => !a.Value && b.Value;

        /// <summary>比较运算符：a 大于等于 b（a=true 或 b=false）。</summary>
        public static bool operator >=(HBool a, HBool b) => a.Value || !b.Value;

        /// <summary>比较运算符：a 小于等于 b（a=false 或 b=true）。</summary>
        public static bool operator <=(HBool a, HBool b) => !a.Value || b.Value;
        /// <summary>逻辑与运算符。</summary>
        public static HBool operator &(HBool a, HBool b) => new HBool(a.Value & b.Value);
        /// <summary>逻辑或运算符。</summary>
        public static HBool operator |(HBool a, HBool b) => new HBool(a.Value | b.Value);
        /// <summary>逻辑异或运算符。</summary>
        public static HBool operator ^(HBool a, HBool b) => new HBool(a.Value ^ b.Value);
        /// <summary>逻辑非运算符。</summary>
        public static HBool operator !(HBool a) => new HBool(!a.Value);

        /// <summary>将 HBool 视为条件逻辑值（true/false 运算符）。</summary>
        public static bool operator true(HBool a) => a.Value;
        /// <summary>运算符 false：用于 假判定。</summary>
        public static bool operator false(HBool a) => !a.Value;
        #endregion

        #region 接口实现
        public bool Equals(HBool other) => Value == other.Value;
        public override bool Equals(object obj) => obj is HBool h && Equals(h);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(HBool other) => Value.CompareTo(other.Value);

        /// <summary>
        /// 返回此实例的字符串表示形式（"True" 或 "False"）。
        /// </summary>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// 使用指定的格式和区域性特定格式信息，将此实例的值转换为其等效的字符串表示形式。
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider) => Value.ToString(formatProvider);
        #endregion

        #region 静态工厂方法 / 解析
        /// <summary>
        /// 尝试将字符串表示形式转换为等效的 HBool。返回指示转换是否成功的值。
        /// </summary>
        public static bool TryParse(string s, out HBool result)
        {
            if (bool.TryParse(s, out bool b))
            {
                result = new HBool(b);
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 将字符串表示形式转换为等效的 HBool。
        /// </summary>
        public static HBool Parse(string s) => new HBool(bool.Parse(s));
        #endregion
    }


    /// <summary>
    /// 为 HBool 提供与字符串之间的类型转换，支持 PropertyGrid 下拉框选择。
    /// </summary>
    public class HBoolConverter : TypeConverter
    {
        // 原有字段与方法
        private HMessageA zxMessageShow = new HMessageA();

        /// <summary>CanConvertFrom 方法。</summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <summary>CanConvertTo 方法。</summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        // 输入验证与日志（保留原有逻辑）
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
                            HTranslation.GetContent("无法将 {0} 转换为 Bool。输入为null,请正确写 True 或 False！", str),
                            HTranslation.GetContent("输入值错误"),
                            0,
                            HTranslation.GetContent("确认"));
                    }
                    return old;
                }
                else
                {
                    if (bool.TryParse(str, out bool result))
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
                        return new HBool(result);
                    }
                    if (!zxMessageShow.IsShow)
                    {
                        zxMessageShow.ZzShowDialog(
                            HTranslation.GetContent("无法将 {0} 转换为 Bool。请正确写 True 或 False！", str),
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
        private HBool GetOldValue(ITypeDescriptorContext context)
        {
            if (context?.PropertyDescriptor != null && context.Instance != null)
            {
                object val = context.PropertyDescriptor.GetValue(context.Instance);
                if (val is HBool hb) return hb;
            }
            return HBool.False;
        }

        /// <summary>ConvertTo 方法。</summary>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is HBool hb)
            {
                return hb.Value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        // ========== 新增：支持下拉框选择 ==========
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        // 设置为 true 表示用户只能从下拉列表中选择，不允许手动输入（如需允许手动输入可改为 false）
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        /// <summary>获取 standardValues。</summary>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // 返回 True 和 False 两个标准值
            return new StandardValuesCollection(new HBool[] { HBool.True, HBool.False });
        }
    }
}
