using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HFromUI.HMath;

namespace HFromUI.HAttribute
{
    /// <summary>
    /// 标记的属性/字段不参与 JSON / XML 序列化与反序列化。
    /// 用法：[HIgnore] public string Password { get; set; }
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// 序列化时使用指定名称（JSON 的键名 / XML 的元素名或特性名），反序列化时按该名称匹配。
    /// 可标注在属性、字段、类（类名仅影响 XML 根节点名）上。
    /// 用法：[HName("user_name")] public string UserName { get; set; }
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct,
        AllowMultiple = false, Inherited = true)]
    public sealed class HNameAttribute : Attribute
    {
        /// <summary>序列化使用的名称</summary>
        public string Name { get; private set; }

        public HNameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// 仅 XML 有效：属性/字段以 XML 特性(attribute)形式写出，而不是子节点(element)。
    /// 用法：[HXmlAttribute] public int Id { get; set; }  →  &lt;Person Id="1"&gt;
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HXmlAttributeAttribute : Attribute
    {
    }

    /// <summary>
    /// DateTime / DateTimeOffset 序列化时使用的格式，如 "yyyy-MM-dd HH:mm:ss"。
    /// JSON 与 XML 通用；反序列化时优先按该格式解析。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class HDateTimeFormatAttribute : Attribute
    {
        /// <summary>日期时间格式字符串</summary>
        public string Format { get; private set; }

        public HDateTimeFormatAttribute(string format)
        {
            Format = format;
        }
    }

    /// <summary>
    /// HJsonFile / HXmlFile 共享的反射成员模型（内部使用）：
    /// 收集类型上参与序列化的公开属性/字段，解析 HIgnore / HName / HXmlAttribute / HDateTimeFormat 特性。
    /// </summary>
    internal static class HSerializedType
    {
        internal const int MaxDepth = 100;

        internal sealed class HMember
        {
            internal string Name;          // 序列化使用的名称（HName 或 原名）
            internal string OriginalName;  // C# 成员原名
            internal Type MemberType;
            internal string DateFormat;    // HDateTimeFormat
            internal bool IsXmlAttribute;  // HXmlAttribute
            private readonly PropertyInfo _prop;
            private readonly FieldInfo _field;

            internal HMember(PropertyInfo p) { _prop = p; MemberType = p.PropertyType; }
            internal HMember(FieldInfo f) { _field = f; MemberType = f.FieldType; }

            /// <summary>Get 方法。</summary>
            internal object Get(object obj)
            {
                return _prop != null ? _prop.GetValue(obj, null) : _field.GetValue(obj);
            }
            /// <summary>Set 方法。</summary>
            internal void Set(object obj, object value)
            {
                try
                {
                    if (_prop != null) { if (_prop.CanWrite) _prop.SetValue(obj, value, null); }
                    else if (!_field.IsInitOnly && !_field.IsLiteral) _field.SetValue(obj, value);
                }
                catch { }
            }
        }

        private static readonly ConcurrentDictionary<Type, List<HMember>> _cache =
            new ConcurrentDictionary<Type, List<HMember>>();

        private const BindingFlags BindFlags = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>获取类型参与序列化的成员（结果缓存）</summary>
        internal static List<HMember> GetMembers(Type type)
        {
            return _cache.GetOrAdd(type, t =>
            {
                List<HMember> list = new List<HMember>();
                foreach (PropertyInfo p in t.GetProperties(BindFlags))
                {
                    if (p.GetIndexParameters().Length > 0) continue;
                    if (p.GetGetMethod(false) == null) continue;
                    if (p.IsDefined(typeof(HIgnoreAttribute), true)) continue;
                    HMember m = new HMember(p);
                    ApplyAttributes(m, p.Name, p);
                    list.Add(m);
                }
                foreach (FieldInfo f in t.GetFields(BindFlags))
                {
                    if (f.IsInitOnly || f.IsLiteral) continue;
                    if (f.IsDefined(typeof(HIgnoreAttribute), true)) continue;
                    HMember m = new HMember(f);
                    ApplyAttributes(m, f.Name, f);
                    list.Add(m);
                }
                return list;
            });
        }

        /// <summary>ApplyAttributes 方法。</summary>
        private static void ApplyAttributes(HMember m, string original, ICustomAttributeProvider cap)
        {
            m.OriginalName = original;
            m.Name = original;
            object[] names = cap.GetCustomAttributes(typeof(HNameAttribute), true);
            if (names.Length > 0)
            {
                string nn = ((HNameAttribute)names[0]).Name;
                if (!string.IsNullOrWhiteSpace(nn)) m.Name = nn;
            }
            object[] fmts = cap.GetCustomAttributes(typeof(HDateTimeFormatAttribute), true);
            if (fmts.Length > 0) m.DateFormat = ((HDateTimeFormatAttribute)fmts[0]).Format;
            m.IsXmlAttribute = cap.IsDefined(typeof(HXmlAttributeAttribute), true);
        }

        /// <summary>类型（类或其任意成员）是否使用了 H 系列序列化特性</summary>
        internal static bool HasHAttributes(Type type)
        {
            if (type == null) return false;
            if (type.IsDefined(typeof(HNameAttribute), true)) return true;
            foreach (PropertyInfo p in type.GetProperties(BindFlags))
                if (p.IsDefined(typeof(HIgnoreAttribute), true) || p.IsDefined(typeof(HNameAttribute), true)
                    || p.IsDefined(typeof(HDateTimeFormatAttribute), true) || p.IsDefined(typeof(HXmlAttributeAttribute), true))
                    return true;
            foreach (FieldInfo f in type.GetFields(BindFlags))
                if (f.IsDefined(typeof(HIgnoreAttribute), true) || f.IsDefined(typeof(HNameAttribute), true)
                    || f.IsDefined(typeof(HDateTimeFormatAttribute), true) || f.IsDefined(typeof(HXmlAttributeAttribute), true))
                    return true;
            return false;
        }

        /// <summary>类型的序列化名称：类上 HName 优先，否则类型名（用于 XML 根节点）</summary>
        internal static string TypeName(Type t)
        {
            object[] hn = t.GetCustomAttributes(typeof(HNameAttribute), true);
            if (hn.Length > 0)
            {
                string n = ((HNameAttribute)hn[0]).Name;
                if (!string.IsNullOrWhiteSpace(n)) return n;
            }
            return t.Name;
        }
    }

    /// <summary>
    /// HFromUI 的 H 系列标量包装类型（HFromUI.HMath 命名空间下的
    /// HBool / HDouble / HFloat / HInt / HLong / HShort / HUInt / HULong / HUShort）在 JSON / XML 中
    /// 统一按底层原生值（bool / double / float / int / long / short / uint / ulong / ushort）直接读写：
    /// 序列化时取出 Value 平铺为原生标量，避免输出 { "Value": ... } 包装对象；
    /// 反序列化时把原生值经单参构造函数重新装箱为 H 包装实例。
    /// </summary>
    internal static class HScalarValue
    {
        /// <summary>H 包装类型 → 底层原生类型映射表</summary>
        private static readonly Dictionary<Type, Type> HToNative = new Dictionary<Type, Type>
        {
            { typeof(HBool),   typeof(bool) },
            { typeof(HDouble), typeof(double) },
            { typeof(HFloat),  typeof(float) },
            { typeof(HInt),    typeof(int) },
            { typeof(HLong),   typeof(long) },
            { typeof(HShort),  typeof(short) },
            { typeof(HUInt),   typeof(uint) },
            { typeof(HULong),  typeof(ulong) },
            { typeof(HUShort), typeof(ushort) },
        };

        /// <summary>各 H 类型 Value 属性的缓存</summary>
        private static readonly ConcurrentDictionary<Type, PropertyInfo> ValueProps =
            new ConcurrentDictionary<Type, PropertyInfo>();

        /// <summary>各 H 类型单参原生值构造函数的缓存</summary>
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> Ctors =
            new ConcurrentDictionary<Type, ConstructorInfo>();

        /// <summary>
        /// 判断类型是否为 H 标量包装（自动剥除 Nullable&lt;&gt; 壳）；
        /// 命中时 native 输出其底层原生类型，未命中 native 为 null。
        /// </summary>
        internal static bool IsHScalar(Type t, out Type native)
        {
            native = null;
            if (t == null) return false;
            Type u = Nullable.GetUnderlyingType(t) ?? t;
            return HToNative.TryGetValue(u, out native);
        }

        /// <summary>
        /// 取出 H 标量包装的底层原生值（反射 Value 属性，结果缓存）；
        /// 入参为 null 返回 null；不是 H 标量则原样返回。
        /// </summary>
        internal static object Unwrap(object obj)
        {
            if (obj == null) return null;
            Type t = obj.GetType();
            if (!HToNative.ContainsKey(t)) return obj;
            PropertyInfo pi = ValueProps.GetOrAdd(t, x => x.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance));
            return pi != null ? pi.GetValue(obj, null) : obj;
        }

        /// <summary>
        /// 用原生值构造指定 H 标量包装类型（自动剥除 Nullable&lt;&gt; 壳）：
        /// value 先 ChangeType 到底层原生类型，再调用 H 类型的单参构造函数。
        /// value 为 null 时，值类型返回默认实例，引用/可空类型返回 null。
        /// </summary>
        internal static object Wrap(Type hType, object value)
        {
            Type u = Nullable.GetUnderlyingType(hType) ?? hType;
            Type native;
            if (!HToNative.TryGetValue(u, out native)) return value;
            if (value == null) return hType.IsValueType ? Activator.CreateInstance(hType) : null;
            object cv = value.GetType() == native ? value : Convert.ChangeType(value, native, CultureInfo.InvariantCulture);
            ConstructorInfo ci = Ctors.GetOrAdd(u, x => x.GetConstructor(new[] { native }));
            return ci != null ? ci.Invoke(new[] { cv }) : Activator.CreateInstance(u);
        }
    }
}
