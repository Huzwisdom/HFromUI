using System;

namespace HFromUI.HAttribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HDictMemberAttribute : Attribute
    {
        /// <summary>
        /// 可选的字典键名（不设置则使用属性名）
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 属性的默认值（可选）。如果属性尚未赋值，Get 将返回该值并写入内部字典。
        /// 使用命名参数方式指定，如 [DictMember(DefaultValue = 100.0)]
        /// </summary>
        public object DefaultValue { get; set; }

        // 无参构造函数
        public HDictMemberAttribute() { }

        // 带键名的构造函数
        public HDictMemberAttribute(string key)
        {
            Key = key;
        }
    }
}