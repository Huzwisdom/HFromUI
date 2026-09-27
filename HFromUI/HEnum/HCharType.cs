using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HEnum
{
    /// <summary>
    /// 字符类型枚举
    /// </summary>
    public enum HCharType
    {
        /// <summary>数字 0-9</summary>
        Digit,
        /// <summary>英文字母 A-Z/a-z</summary>
        Letter,
        /// <summary>中文汉字</summary>
        Chinese,
        /// <summary>其他字符（特殊符号、空格等）</summary>
        Other
    }
}
