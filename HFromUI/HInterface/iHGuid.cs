using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HInterface
{
    public interface iHGuid: iHFrom
    {
        string GuidCode { get; }
        string Name { get; }
    }
}
