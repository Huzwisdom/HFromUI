using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HAttribute
{
    public class HRemarkLanguageAttribute : Attribute
    {
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        public HRemarkLanguageAttribute(string remark)
        {
            this.Remark = remark;
        }
    }
}
