using HFromUI;
using HFromUI.HMath;

namespace HFromUI.HInterface
{

public interface iHDrawContinuousLine
{
    HPoint Start { get;  }
    HPoint End { get;  }
    HPoint GetPointAtDistance2D(HDouble distance);
    HDouble Length2D { get; }
    HDouble Speed { get; }
}
}
