using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HBase
{
    public abstract class HLine3DBase
    {
        /// <summary>HPointStart 成员。</summary>
        public abstract HPoint3D HPointStart { set; get; }

        /// <summary>HPointEnd 成员。</summary>
        public abstract HPoint3D HPointEnd { set; get; }


    }
}
