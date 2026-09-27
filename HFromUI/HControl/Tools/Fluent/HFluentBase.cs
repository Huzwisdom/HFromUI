using System.Drawing;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Fluent
{
    /// <summary>
    /// Fluent 风格控件公共基类：直接继承 Control 全自绘，统一开启透明背景、
    /// 双缓冲与重绘样式位，默认微软雅黑 9pt。具体控件也可改继承 HControl\Base 下的基类。
    /// </summary>
    public abstract class HFluentBase : Control
    {
        /// <summary>初始化公共样式位。</summary>
        protected HFluentBase()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            Font = new Font("微软雅黑", 9f);
        }

        /// <summary>悬停态提亮。</summary>
        protected static Color HoverColor(Color c) => ControlPaint.Light(c, 0.12f);

        /// <summary>按下态压暗。</summary>
        protected static Color PressColor(Color c) => ControlPaint.Dark(c, 0.05f);
    }
}
