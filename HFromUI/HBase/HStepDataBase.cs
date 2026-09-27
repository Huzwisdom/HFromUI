using HFromUI.HAttribute;
using HFromUI.HInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public abstract class HStepDataBase : HDataBase
    {
        public static ConcurrentDictionary<string, HStepDataBase> StepDatas = new ConcurrentDictionary<string, HStepDataBase>(); 
   
        public HStepDataBase this[string name]
        {
            get
            {
                return StepDatas[name];
            }
            set
            {
                StepDatas[name] = value;
            }
        }


    }
    public abstract class HStepDataBase<T> : HStepDataBase
    {
        public abstract T Value { set; get; }

    }
}
