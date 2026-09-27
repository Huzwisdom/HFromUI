using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HFromUI.HEnum;

namespace HFromUI.HInformation.Log
{
    internal class HLogRecording
    {
        /// <summary>Record 成员。</summary>
        internal string Record { set; get; }

        /// <summary>Time 成员。</summary>
        internal DateTime Time { set; get; }

        /// <summary>LOGGrade 成员。</summary>
        internal HLogGrade LOGGrade { set; get; }

        /// <summary>TypeName 成员。</summary>
        internal string TypeName { set; get; }

    }
}
