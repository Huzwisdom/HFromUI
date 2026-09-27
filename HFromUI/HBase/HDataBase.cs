using HFromUI.HAttribute;
using HFromUI.HInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public abstract class HDataBase : iHGuid
    {
        private string name;
        private string guidCode;

        [Browsable(false)]
        public string GuidCode
        {
            get
            {
                if (string.IsNullOrWhiteSpace(guidCode))
                {
                    guidCode = Guid.NewGuid().ToString("N");
                }
                return guidCode;
            }
            set { guidCode = value; }
        }

        [HCategoryLanguage("名称"), HDescriptionLanguage("任务名称")]
        [HDisplayNameLanguage("任务名称")]
        [Browsable(true)]
        public string Name
        {
            get { return name; }
            set
            {
                name = value; 
            }
        }

    }
}