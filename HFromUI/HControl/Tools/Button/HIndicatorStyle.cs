namespace HFromUI.HControl.Tools.Button
{
    /// <summary>
    /// 勾选指示符外观样式（43 种）：外框外形、底填充、描边、内部符号与光效的组合。
    /// HCheckBox 与 HRadioButton 共用：复选默认 Classic，单选默认 RingDot。
    /// 选中色仍取控件的 CheckedColor；标注自定义配色的样式除外。
    /// </summary>
    public enum HIndicatorStyle
    {
        /// <summary>经典：圆角方框填充 + 白对勾。</summary>
        Classic = 0,
        /// <summary>圆点单选：圆圈描边 + 中心圆点（单选默认）。</summary>
        RingDot = 1,
        /// <summary>方角：直角方框 + 对勾。</summary>
        Square = 2,
        /// <summary>圆形勾选：圆底 + 对勾。</summary>
        CircleCheck = 3,
        /// <summary>描边风：透明底，选中时主色描边 + 主色对勾。</summary>
        Outline = 4,
        /// <summary>粗边：加粗描边方框 + 对勾。</summary>
        Bold = 5,
        /// <summary>方块圆点：圆角底 + 白色圆点。</summary>
        Dot = 6,
        /// <summary>实心圆圆点：圆底 + 白圆点。</summary>
        FilledDot = 7,
        /// <summary>叉号：选中时方框内显示叉。</summary>
        Cross = 8,
        /// <summary>加号：选中时方框内显示加号。</summary>
        Plus = 9,
        /// <summary>菱形：菱形底 + 圆点。</summary>
        Diamond = 10,
        /// <summary>六边形：六边形底 + 对勾。</summary>
        Hexagon = 11,
        /// <summary>虚线风：虚线描边方框 + 对勾。</summary>
        Dashed = 12,
        /// <summary>双线风：同心双圆描边 + 圆点。</summary>
        DoubleLine = 13,
        /// <summary>纵向渐变底 + 白对勾。</summary>
        Gradient = 14,
        /// <summary>渐变描边圆环 + 圆点。</summary>
        GradientRing = 15,
        /// <summary>外发光：选中时指示符外缘有主色光晕。</summary>
        Glow = 16,
        /// <summary>霓虹：深色底 + 主色辉光描边与符号，配色自定义。</summary>
        Neon = 17,
        /// <summary>赛博：切角六边形 + 主色辉光边。</summary>
        Cyber = 18,
        /// <summary>磨砂玻璃：半透明白底 + 白描边 + 主色符号。</summary>
        Glass = 19,
        /// <summary>碳纤维：深色底 + 青色描边与符号，配色自定义。</summary>
        Carbon = 20,
        /// <summary>Fluent：中性灰细边，选中主色饱满填充。</summary>
        Fluent = 21,
        /// <summary>Material3：透明底 + 主色粗环与对勾。</summary>
        Material3 = 22,
        /// <summary>脉冲：圆点 + 两圈信号涟漪。</summary>
        Pulse = 23,
        /// <summary>单色线性：墨黑细线方框与对勾，无彩色。</summary>
        MonoLine = 24,
        /// <summary>翡翠绿：祖母绿填充，配色自定义。</summary>
        Emerald = 25,
        /// <summary>皇家靛：靛蓝填充，配色自定义。</summary>
        Royal = 26,
        /// <summary>Bootstrap：绿色填充，配色自定义。</summary>
        Bootstrap = 27,
        /// <summary>落日：粉橙渐变底 + 白对勾，配色自定义。</summary>
        Sunset = 28,
        /// <summary>极光：青紫渐变底 + 白对勾，配色自定义。</summary>
        Aurora = 29,
        /// <summary>新拟态：同底色凹凸浮雕，选中填主色。</summary>
        Neumorph = 30,
        /// <summary>星标：星星随选中点亮（固定琥珀色）。</summary>
        Star = 31,
        /// <summary>爱心：心形随选中变红（固定红色）。</summary>
        Heart = 32,
        /// <summary>闪电：闪电符号随选中点亮。</summary>
        Bolt = 33,
        /// <summary>旗帜：小旗随选中点亮。</summary>
        Flag = 34,
        /// <summary>书签：缎带书签随选中点亮。</summary>
        Bookmark = 35,
        /// <summary>守卫：盾牌勾随选中变绿（固定翠绿）。</summary>
        Shield = 36,
        /// <summary>锁扣：挂锁随选中合上变色。</summary>
        Lock = 37,
        /// <summary>眼睛：眼睛随选中睁开着色。</summary>
        Eye = 38,
        /// <summary>太阳：太阳随选中点亮。</summary>
        Sun = 39,
        /// <summary>月亮：新月随选中点亮。</summary>
        Moon = 40,
        /// <summary>云朵：云随选中转蓝（固定天蓝）。</summary>
        Cloud = 41,
        /// <summary>音符：八分音符随选中变紫（固定紫色）。</summary>
        Music = 42
    }
}
