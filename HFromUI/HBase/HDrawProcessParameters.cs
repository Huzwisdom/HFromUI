using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using HFromUI.HMath;

namespace HFromUI.HBase
{
    using HFromUI.HLangage;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [FlattenProperty]
    public abstract class HDrawProcessParameters
    {
        public object this[string propertyName]
        {
            get
            {
                if (_data.TryGetValue(propertyName, out object val))
                    return val;
                // 属性未赋值，尝试从默认特性获取
                var prop = this.GetType().GetProperty(propertyName);
                var attr = prop?.GetCustomAttribute<HDictMemberAttribute>();
                if (attr?.DefaultValue != null)
                    return attr.DefaultValue;
                return null;
            }
            set
            {
                // 获取属性元数据
                var prop = this.GetType().GetProperty(propertyName);
                if (prop == null)
                    throw new ArgumentException(HTranslation.GetContent($"类型 {this.GetType().Name} 中不存在属性 {propertyName}"));

                Type targetType = prop.PropertyType;
                object convertedValue;

                // 根据属性的声明类型进行转换
                if (targetType == typeof(HDouble) || targetType == typeof(double))
                    convertedValue = Convert.ToDouble(value);
                else if (targetType == typeof(HInt) || targetType == typeof(int))
                    convertedValue = Convert.ToInt32(value);
                else if (targetType == typeof(HUInt) || targetType == typeof(uint))
                    convertedValue = Convert.ToUInt32(value);
                else if (targetType == typeof(HShort) || targetType == typeof(short))
                    convertedValue = Convert.ToInt16(value);
                else if (targetType == typeof(HUShort) || targetType == typeof(ushort))
                    convertedValue = Convert.ToUInt16(value);
                else if (targetType == typeof(HLong) || targetType == typeof(long))
                    convertedValue = Convert.ToInt64(value);
                else if (targetType == typeof(HULong) || targetType == typeof(ulong))
                    convertedValue = Convert.ToUInt64(value);
                else if (targetType == typeof(HBool) || targetType == typeof(bool))
                    convertedValue = Convert.ToBoolean(value);
                else
                    convertedValue = value;   // 其他类型（如 List<string>）直接赋值

                _data[propertyName] = convertedValue;
            }
        }
        public override string ToString()
        {
            return HTranslation.GetContent("工艺参数设置");
        }
        [HCategoryLanguage("图形G代码")]
        [HDisplayNameLanguage("1.GCode前缀")]
        [HDescriptionLanguage("G代码执行到这段后的前面的G代码补充")]
        [Browsable(true)]

        public string Prefix
        {
            get
            {
                string buffer= Get<string>();
               

                return buffer;
            }
            set
            {
                Set(value);
            }
        }
        [HCategoryLanguage("图形G代码")]
        [HDisplayNameLanguage("2.GCode图形")]
        [HDescriptionLanguage("G代码执行到这段后的后面的G代码补充")]
        [Browsable(true)]
        public string GcodeShow
        {
            get
            {
                return GCode;
            }
        }
        [HCategoryLanguage("图形G代码")]
        [HDisplayNameLanguage("GCode")]
        [HDescriptionLanguage("G代码执行到这段后的后面的G代码补充")]
        [Browsable(false)]
        public string GCode
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set(value);
            }
        }
        [HCategoryLanguage("图形G代码")]
        [HDisplayNameLanguage("3.GCode后缀")]
        [HDescriptionLanguage("G代码执行到这段后的后面的G代码补充")]
        [Browsable(true)]

        public string Suffix
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set(value);
            }
        }

        [HCategoryLanguage("速度")]
        [HDisplayNameLanguage("速度")]
        [HDescriptionLanguage("设置设备的运行速度")]
        [Browsable(true)]
        [HDictMember(DefaultValue = 1200d)]
        public HDouble Speed
        {
            get => Get<HDouble>();
            set
            {
                Set(value);
            }
        }
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        protected T Get<T>([CallerMemberName] string propertyName = "")
        {
            try
            {
                if (_data.TryGetValue(propertyName, out object val))
                    return (T)val;
            }
            catch { }

            var prop = this.GetType().GetProperty(propertyName);
            var attr = prop?.GetCustomAttribute<HDictMemberAttribute>();
            if (attr?.DefaultValue != null)
            {
                object defaultVal = attr.DefaultValue;
                if (defaultVal is T tVal)
                {
                    _data[propertyName] = tVal;
                    return tVal;
                }
                if (typeof(T) == typeof(HDouble) && defaultVal is double d)
                {
                    HDouble hd = d;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HInt) && defaultVal is int i)
                {
                    HInt hd = i;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HUInt) && defaultVal is uint iu)
                {
                    HUInt hd = iu;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HUShort) && defaultVal is ushort us)
                {
                    HUShort hd = us;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HShort) && defaultVal is short s)
                {
                    HShort hd = s;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HLong) && defaultVal is long l)
                {
                    HLong hd = l;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HULong) && defaultVal is long ul)
                {
                    HLong hd = ul;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                if (typeof(T) == typeof(HBool) && defaultVal is bool b)
                {
                    HBool hd = b;
                    _data[propertyName] = hd;
                    return (T)(object)hd;
                }
                try
                {
                    var converted = defaultVal;
                    _data[propertyName] = converted;
                    return (T)converted;
                }
                catch { }
                try
                {
                    var converted = Convert.ChangeType(defaultVal, typeof(T));
                    _data[propertyName] = converted;
                    return (T)converted;
                }
                catch { }
            }
            return default(T);
        }
        /// <summary>清空。</summary>
        public void Clear()
        {
            _data.Clear();
        }
        protected void Set<T>(T value, [CallerMemberName] string propertyName = "")
        {
            _data[propertyName] = value;
        }

        public Dictionary<string, object> ExportMarked()
        {
            return GetType()
                   .GetProperties()
                   .Where(p => p.GetCustomAttribute<HDictMemberAttribute>() != null)
                   .ToDictionary(p => p.Name, p => p.GetValue(this));
        }


        /// <summary>克隆。</summary>
        public HDrawProcessParameters Clone()
        {
            // 创建当前类型的未初始化对象（不调用构造函数）
            var clone = (HDrawProcessParameters)FormatterServices.GetUninitializedObject(this.GetType());

            // 通过反射获取基类的私有字段 _data
            var dataField = typeof(HDrawProcessParameters).GetField("_data",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            // 创建新字典，浅拷贝所有值（内部值类型和包装类安全，若对象需要深拷贝请在派生类重写）
            var newData = new Dictionary<string, object>(_data);
            dataField.SetValue(clone, newData);

            return clone;
        }
        public Dictionary<string, object> ToRawDictionary() => new Dictionary<string, object>(_data);



    }


    [AttributeUsage(AttributeTargets.Class)]
    public class FlattenPropertyAttribute : Attribute { }

    /// <summary>
    /// 继承此类的对象将自动把带有 [FlattenProperty] 的属性平铺。
    /// </summary>
    public class FlattenableObject : ICustomTypeDescriptor
    {
        private PropertyDescriptorCollection _flattenedProperties;
        private readonly object _instance;

        public FlattenableObject()
        {
            _instance = this;
        }

        // 统一获取扁平化后的属性集合
        private PropertyDescriptorCollection GetFlattenedProperties(Attribute[] attributes)
        {
            if (_flattenedProperties == null)
            {
                var baseProps = TypeDescriptor.GetProperties(_instance, attributes, true);
                var list = new List<PropertyDescriptor>();

                foreach (PropertyDescriptor prop in baseProps)
                {
                    if (prop.PropertyType.GetCustomAttribute<FlattenPropertyAttribute>() != null)
                    {
                        // 平铺：用子属性替换此属性
                        var childProps = TypeDescriptor.GetProperties(prop.PropertyType, attributes);
                        foreach (PropertyDescriptor child in childProps)
                        {
                            list.Add(new FlattenPropertyDescriptor(prop, child));
                        }
                    }
                    else
                    {
                        list.Add(prop);
                    }
                }
                _flattenedProperties = new PropertyDescriptorCollection(list.ToArray());
            }
            // 注意：attributes 参数可能影响过滤，但这里简化处理，始终返回全量
            return _flattenedProperties;
        }

        #region ICustomTypeDescriptor 实现
        public String GetClassName() => TypeDescriptor.GetClassName(this, true);
        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);
        public String GetComponentName() => TypeDescriptor.GetComponentName(this, true);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);
        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);
        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetFlattenedProperties(attributes);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }
        #endregion

        /// <summary>
        /// 平铺属性描述器，将子属性包装在父级
        /// </summary>
        private class FlattenPropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyDescriptor _parentProp;
            private readonly PropertyDescriptor _childProp;

            public FlattenPropertyDescriptor(PropertyDescriptor parent, PropertyDescriptor child)
                : base($"{parent.Name}_{child.Name}", child.Attributes.Cast<Attribute>().ToArray())
            {
                _parentProp = parent;
                _childProp = child;
            }

            public override Type PropertyType => _childProp.PropertyType;
            public override Type ComponentType => _parentProp.ComponentType;
            public override bool IsReadOnly => _childProp.IsReadOnly;

            public override object GetValue(object component)
            {
                var parentValue = _parentProp.GetValue(component);
                return parentValue == null ? null : _childProp.GetValue(parentValue);
            }

            public override void SetValue(object component, object value)
            {
                var parentValue = _parentProp.GetValue(component);
                if (parentValue == null) return;
                _childProp.SetValue(parentValue, value);
                OnValueChanged(component, EventArgs.Empty);
            }

            public override bool CanResetValue(object component) => false;
            public override void ResetValue(object component) { }
            public override bool ShouldSerializeValue(object component) => false;

            public override string DisplayName => _childProp.DisplayName;
            /// <summary>Category 成员。</summary>
            public override string Category => _childProp.Category;
            /// <summary>Description 成员。</summary>
            public override string Description => _childProp.Description;
        }

    }
}