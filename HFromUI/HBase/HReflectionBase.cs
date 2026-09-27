using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    using HFromUI.HLangage;
    /// <summary>
    /// 反射操作基类，提供最全面的类型反射、动态创建实例、成员访问等方法。
    /// 可实例化调用，也可作为基类供其他类继承使用。
    /// </summary>
    public class HReflectionBase: HDataBase
    {
        #region 类型查找与子类获取

        /// <summary>
        /// 从指定程序集中获取所有继承自 <typeparamref name="T"/> 的非抽象子类类型。
        /// </summary>
        /// <typeparam name="T">基类类型</typeparam>
        /// <param name="assembly">要搜索的程序集</param>
        /// <returns>所有符合条件的子类类型集合</returns>
        public IEnumerable<Type> GetDerivedTypes<T>(Assembly assembly)
        {
            Type baseType = typeof(T);
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));
        }

        /// <summary>
        /// 从单个 DLL 文件加载程序集，并获取所有继承自 <typeparamref name="T"/> 的子类类型。
        /// </summary>
        /// <typeparam name="T">基类类型</typeparam>
        /// <param name="dllPath">DLL 文件完整路径</param>
        /// <returns>所有符合条件的子类类型集合</returns>
        public IEnumerable<Type> GetDerivedTypesFromDll<T>(string dllPath)
        {
            Assembly asm = Assembly.LoadFrom(dllPath);
            return GetDerivedTypes<T>(asm);
        }

        /// <summary>
        /// 从指定文件夹中加载所有 *.dll 程序集，并获取所有继承自 <typeparamref name="T"/> 的子类类型。
        /// </summary>
        /// <typeparam name="T">基类类型</typeparam>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>所有符合条件的子类类型集合</returns>
        public IEnumerable<Type> GetDerivedTypesFromFolder<T>(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                yield break;

            foreach (string dll in Directory.GetFiles(folderPath, "*.dll"))
            {
                Assembly asm;
                try { asm = Assembly.LoadFrom(dll); }
                catch { continue; }

                foreach (var type in GetDerivedTypes<T>(asm))
                    yield return type;
            }
        }

        /// <summary>
        /// 从程序集中获取所有实现了指定接口 <typeparamref name="TInterface"/> 的非抽象类型。
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <param name="assembly">程序集</param>
        public IEnumerable<Type> GetInterfaceImplementations<TInterface>(Assembly assembly)
        {
            Type interfaceType = typeof(TInterface);
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
        }

        /// <summary>
        /// 根据完整类名（"命名空间.类名"）从程序集中获取 Type。
        /// </summary>
        public Type GetTypeByName(Assembly assembly, string fullClassName)
        {
            return assembly.GetType(fullClassName);
        }

        /// <summary>
        /// 根据类名（不含命名空间）在程序集中查找第一个匹配的非抽象公共类。
        /// </summary>
        public Type GetTypeBySimpleName(Assembly assembly, string className)
        {
            return assembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(className, StringComparison.Ordinal) && t.IsClass && !t.IsAbstract);
        }

        #endregion

        #region 动态创建实例

        /// <summary>
        /// 使用无参构造函数创建类型实例（返回 object）。
        /// </summary>
        public object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 使用无参构造函数创建类型实例，并转换为指定类型 <typeparamref name="T"/>。
        /// </summary>
        public T CreateInstance<T>(Type type) where T : class
        {
            if (!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException(HTranslation.GetContent($"{type.FullName} 不是 {typeof(T).FullName} 的子类或实现"));
            return (T)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 通过完整类名从程序集创建实例（无参构造函数）。
        /// </summary>
        public object CreateInstance(Assembly assembly, string fullClassName)
        {
            Type type = GetTypeByName(assembly, fullClassName);
            if (type == null)
                throw new ArgumentException(HTranslation.GetContent($"类型 {fullClassName} 在程序集中未找到"));
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// 从程序集中查找指定名称的子类（需继承自 <typeparamref name="T"/>），并创建实例。
        /// </summary>
        public T CreateDerivedInstance<T>(Assembly assembly, string className) where T : class
        {
            Type type = assembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(className, StringComparison.Ordinal)
                                     && typeof(T).IsAssignableFrom(t)
                                     && !t.IsAbstract);
            if (type == null)
                throw new ArgumentException(HTranslation.GetContent($"在程序集中未找到名为 {className} 的非抽象子类"));
            return (T)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 使用带参数的构造函数创建实例。
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="args">构造函数参数数组</param>
        public object CreateInstance(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        /// <summary>
        /// 使用带参数的构造函数创建实例并转换为指定类型。
        /// </summary>
        public T CreateInstance<T>(Type type, params object[] args) where T : class
        {
            return (T)CreateInstance(type, args);
        }

        /// <summary>
        /// 根据程序集限定名（"完整类名, 程序集显示名"）创建实例。
        /// </summary>
        public object CreateInstanceFromQualifiedName(string typeQualifiedName)
        {
            Type type = Type.GetType(typeQualifiedName);
            if (type == null)
                throw new ArgumentException(HTranslation.GetContent($"无法解析类型：{typeQualifiedName}"));
            return Activator.CreateInstance(type);
        }

        #endregion

        #region 字段与属性操作

        /// <summary>
        /// 获取实例的公共字段值。
        /// </summary>
        public object GetFieldValue(object obj, string fieldName)
        {
            Type type = obj.GetType();
            FieldInfo fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            return fi?.GetValue(obj);
        }

        /// <summary>
        /// 设置实例的公共字段值。
        /// </summary>
        public void SetFieldValue(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            FieldInfo fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            fi?.SetValue(obj, value);
        }

        /// <summary>
        /// 获取实例的公共属性值。
        /// </summary>
        public object GetPropertyValue(object obj, string propertyName)
        {
            Type type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return pi?.GetValue(obj);
        }

        /// <summary>
        /// 设置实例的公共属性值。
        /// </summary>
        public void SetPropertyValue(object obj, string propertyName, object value)
        {
            Type type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            pi?.SetValue(obj, value);
        }

        /// <summary>
        /// 获取静态字段值。
        /// </summary>
        public object GetStaticFieldValue(Type type, string fieldName)
        {
            FieldInfo fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            return fi?.GetValue(null);
        }

        /// <summary>
        /// 设置静态字段值。
        /// </summary>
        public void SetStaticFieldValue(Type type, string fieldName, object value)
        {
            FieldInfo fi = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            fi?.SetValue(null, value);
        }

        /// <summary>
        /// 获取静态属性值。
        /// </summary>
        public object GetStaticPropertyValue(Type type, string propertyName)
        {
            PropertyInfo pi = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            return pi?.GetValue(null);
        }

        /// <summary>
        /// 设置静态属性值。
        /// </summary>
        public void SetStaticPropertyValue(Type type, string propertyName, object value)
        {
            PropertyInfo pi = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
            pi?.SetValue(null, value);
        }

        /// <summary>
        /// 获取类型的所有公共属性。
        /// </summary>
        public PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 获取类型的所有公共字段。
        /// </summary>
        public FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 判断类型是否具有指定名称的属性。
        /// </summary>
        public bool HasProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance) != null;
        }

        #endregion

        #region 方法调用

        /// <summary>
        /// 调用实例的公共方法（无参数）。
        /// </summary>
        /// <returns>方法返回值</returns>
        public object InvokeMethod(object obj, string methodName)
        {
            Type type = obj.GetType();
            MethodInfo mi = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return mi?.Invoke(obj, null);
        }

        /// <summary>
        /// 调用实例的公共方法（带参数）。
        /// </summary>
        public object InvokeMethod(object obj, string methodName, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo mi = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            return mi?.Invoke(obj, parameters);
        }

        /// <summary>
        /// 调用实例的公共泛型方法。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="methodName">方法名</param>
        /// <param name="genericTypes">泛型参数类型数组</param>
        /// <param name="parameters">方法参数</param>
        /// <returns>方法返回值</returns>
        public object InvokeGenericMethod(object obj, string methodName, Type[] genericTypes, params object[] parameters)
        {
            Type type = obj.GetType();
            MethodInfo mi = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (mi == null) return null;
            MethodInfo genericMi = mi.MakeGenericMethod(genericTypes);
            return genericMi.Invoke(obj, parameters);
        }

        /// <summary>
        /// 调用静态方法（无参数）。
        /// </summary>
        public object InvokeStaticMethod(Type type, string methodName)
        {
            MethodInfo mi = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            return mi?.Invoke(null, null);
        }

        /// <summary>
        /// 调用静态方法（带参数）。
        /// </summary>
        public object InvokeStaticMethod(Type type, string methodName, params object[] parameters)
        {
            MethodInfo mi = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            return mi?.Invoke(null, parameters);
        }

        /// <summary>
        /// 获取类型的所有公共方法（不含继承自 System.Object 的方法）。
        /// </summary>
        public MethodInfo[] GetMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        }

        #endregion

        #region 特性(Attribute)操作

        /// <summary>
        /// 获取类型上指定的自定义特性（第一个匹配）。
        /// </summary>
        public T GetCustomAttribute<T>(Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 获取类型上所有指定的自定义特性。
        /// </summary>
        public IEnumerable<T> GetCustomAttributes<T>(Type type) where T : Attribute
        {
            return type.GetCustomAttributes<T>();
        }

        /// <summary>
        /// 获取成员上指定的自定义特性。
        /// </summary>
        public T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 判断类型是否定义了某个特性。
        /// </summary>
        public bool IsDefined<T>(Type type) where T : Attribute
        {
            return type.IsDefined(typeof(T), inherit: false);
        }

        /// <summary>
        /// 获取程序集上的自定义特性。
        /// </summary>
        public T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttribute<T>();
        }

        #endregion

        #region 类型关系与判断

        /// <summary>
        /// 判断一个类型是否继承自另一个类型。
        /// </summary>
        public bool IsSubclassOf(Type derivedType, Type baseType)
        {
            return derivedType.IsSubclassOf(baseType);
        }

        /// <summary>
        /// 判断类型是否实现了指定接口。
        /// </summary>
        public bool ImplementsInterface(Type type, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(type) && type.IsClass;
        }

        /// <summary>
        /// 判断类型是否为可实例化的非抽象类。
        /// </summary>
        public bool IsConcreteClass(Type type)
        {
            return type.IsClass && !type.IsAbstract;
        }

        /// <summary>
        /// 获取类型实现的所有接口。
        /// </summary>
        public Type[] GetInterfaces(Type type)
        {
            return type.GetInterfaces();
        }

        #endregion

        #region 其他常用操作

        /// <summary>
        /// 获取当前应用程序域中所有已加载的程序集。
        /// </summary>
        public Assembly[] GetLoadedAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// 从当前已加载的程序集中查找所有继承自 <typeparamref name="T"/> 的子类类型。
        /// </summary>
        public IEnumerable<Type> GetDerivedTypesFromLoadedAssemblies<T>()
        {
            Type baseType = typeof(T);
            foreach (Assembly asm in GetLoadedAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
                        yield return t;
                }
            }
        }

        /// <summary>
        /// 获取类型的默认值（值类型返回零值，引用类型返回 null）。
        /// </summary>
        public object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// 获取枚举类型的所有值。
        /// </summary>
        public Array GetEnumValues(Type enumType)
        {
            return Enum.GetValues(enumType);
        }

        /// <summary>
        /// 获取枚举值的描述特性（若存在）。
        /// </summary>
        public string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attr = fi?.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }

        #endregion
    }
}
