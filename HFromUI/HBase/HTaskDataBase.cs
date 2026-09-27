using HFromUI.HAttribute;
using HFromUI.HData;
using HFromUI.HInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public abstract class HTaskDataBase : HDataBase
    {
        public abstract OK Initialization();
        public abstract OK GetData();
        public abstract OK Run();
        public abstract OK Stop();
        public abstract OK EStop();
        public abstract OK Pause();

    }
}
