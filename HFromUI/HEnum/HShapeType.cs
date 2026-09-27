using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HEnum
{
    [Serializable]
    public enum HShapeType
    {
        Select = -1,
        None = 0,
        Line = 1,
        Lines = 2,
        Circle = 3,
        Arc = 4,
        Arc3P = 5,
        Polygon = 6,
        Point = 7,
    }
}
