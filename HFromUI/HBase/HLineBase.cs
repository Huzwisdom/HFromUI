using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public abstract class HLineBase
    {
        /// <summary>HPointStart 成员。</summary>
        public abstract HPoint HPointStart { set; get; }

        /// <summary>HPointEnd 成员。</summary>
        public abstract HPoint HPointEnd { set; get; }

    }
}
