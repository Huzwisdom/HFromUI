using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HMath
{
    public class HChangePoint
    {


        /// <summary>全局唯一标识符。</summary>
        public string GUID { set; get; }

        /// <summary>名称。</summary>
        public string Name { set; get; }

        /// <summary>获取 name。</summary>
        public string GetName()
        {



            if (!string.IsNullOrWhiteSpace(Name))
            {
                if (Name.Contains("."))
                {
                    string[] buffers = Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (buffers != null && buffers.Length > 0)
                    {
                        return buffers[buffers.Length - 1];
                    }
                }
            }
            return Name;
        }
    }
}
