using System.Drawing;

namespace HFromUI.HControl.Chart.Tips
{
    /// <summary>悬停气泡一行文字（带颜色）：由 HChart 构造，HTipPopup 逐行绘制。
    /// 与标尺 HUD 使用的 HudLine 结构相同但相互独立，气泡提取到 Tips 目录后不再依赖 HChart 私有类型。</summary>
    public struct HTipLine
    {
        /// <summary>本行文字内容。</summary>
        public string Text;
        /// <summary>本行文字颜色（正文色/强调色/系列图例色）。</summary>
        public Color Color;

        /// <summary>构造：文字 + 颜色。</summary>
        public HTipLine(string text, Color color) { Text = text; Color = color; }
    }
}
