using HFromUI.HData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HInterface
{
    public interface iHGode: iHGuid
    {
        GCodeText GCode { set; get; }
        GCodeText Prefix { get; set; }
        GCodeText Suffix { get; set; }
    }
}
