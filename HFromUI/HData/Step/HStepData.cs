using HFromUI.HBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HData.Step
{
    public class HStepData<T>: HStepDataBase<T>
    {

        internal  T valueData;
        public override T Value
        {
            get { 
            return valueData;
            }
            set {
                valueData = value;
                HStepDataBase.StepDatas.AddOrUpdate(GuidCode, this, (k, old) =>
                {
                    if (old is HStepData<T> )
                    {
                        HStepData<T> oldTyped=old as HStepData<T>;
                        if (!ReferenceEquals(old, this))
                        {
                            oldTyped.valueData = value;  // 直接改字段，不触发 setter
                        }
                    }
                    else
                    {
                        old = this;
                    }
                    return old;
                }); 
        }
        }
    }
}
