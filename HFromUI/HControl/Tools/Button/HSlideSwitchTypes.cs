namespace HFromUI.HControl.Tools.Button
{
    /// <summary>
    /// 滑动开关外观样式（64 种）：轨道外形、圆钮造型、轨道/钮面符号与光效的组合。
    /// 开/关两态的基准色仍取 CheckedColor/UncheckedColor，标注自定义配色的样式除外。
    /// </summary>
    public enum HSlideSwitchStyle
    {
        /// <summary>经典 iOS：胶囊轨道 + 白色圆钮。</summary>
        IOS = 0,
        /// <summary>质感设计：胶囊轨道内显示勾/叉，白钮带轻投影。</summary>
        Material = 1,
        /// <summary>Windows 11：圆钮贴近轨道边缘，关态钮带灰边。</summary>
        Windows = 2,
        /// <summary>方角轨道 + 圆角方钮。</summary>
        Square = 3,
        /// <summary>大圆角矩形轨道 + 圆钮。</summary>
        Rounded = 4,
        /// <summary>描边风：白底细边，开启时圆钮填主色。</summary>
        Outline = 5,
        /// <summary>纵向渐变轨道。</summary>
        Gradient = 6,
        /// <summary>外发光：开启时轨道外缘有主色光晕。</summary>
        Glow = 7,
        /// <summary>霓虹：深色槽 + 霓虹圆钮/描边辉光。</summary>
        Neon = 8,
        /// <summary>拟物立体：槽体明暗渐变 + 凸起圆钮。</summary>
        Skeuo = 9,
        /// <summary>极简扁平：纯色轨道与圆钮，无描边无投影。</summary>
        Flat = 10,
        /// <summary>粗大：圆钮几乎填满轨道高度。</summary>
        Bold = 11,
        /// <summary>纤细：细槽 + 外悬圆钮。</summary>
        Thin = 12,
        /// <summary>胶囊钮：圆钮随滑行动作拉长为胶囊。</summary>
        Pill = 13,
        /// <summary>钮上勾叉：圆钮内开勾关叉。</summary>
        Check = 14,
        /// <summary>电源符号：圆钮内电源标志随状态着色。</summary>
        Power = 15,
        /// <summary>日夜主题：关太阳（琥珀槽）/ 开月亮（藏青槽），配色自定义。</summary>
        DayNight = 16,
        /// <summary>轨道文字：槽内显示 ON/OFF（可用 OnText/OffText 自定义）。</summary>
        Text = 17,
        /// <summary>分段式：轨道中央有分隔线。</summary>
        Segmented = 18,
        /// <summary>液态：圆钮拖带主色液面填充轨道。</summary>
        Liquid = 19,
        /// <summary>圆环钮：环形圆钮透出轨道色。</summary>
        Ring = 20,
        /// <summary>卡片投影：圆钮带明显下垂阴影。</summary>
        Shadow = 21,
        /// <summary>深色科技：炭灰槽 + 白钮（开态槽体加深）。</summary>
        Dark = 22,
        /// <summary>糖果渐变：粉橙双色渐变轨道。</summary>
        Candy = 23,
        /// <summary>海洋风：蓝色渐变槽 + 圆钮内点。</summary>
        Ocean = 24,
        /// <summary>凹陷浮雕：轨道内陷、圆钮凸起。</summary>
        Emboss = 25,
        /// <summary>单色线性：白底细线，圆钮仅细环勾边。</summary>
        MonoLine = 26,
        /// <summary>交通灯：红关绿开，配色自定义。</summary>
        Traffic = 27,
        /// <summary>小圆点：宽槽上的小圆形滑钮。</summary>
        Dot = 28,
        /// <summary>大钮外悬：细槽配明显大于槽高的圆钮。</summary>
        Rubber = 29,
        /// <summary>赛博：切角轨道与方钮 + 青色辉光描边，配色自定义。</summary>
        Cyber = 30,
        /// <summary>复古黄铜：黄铜渐变槽 + 象牙圆钮，配色自定义。</summary>
        Antique = 31,
        /// <summary>Fluent：Win11 中性灰槽 + 小尺寸白钮。</summary>
        Fluent = 32,
        /// <summary>Metro：Win8 扁平方角，主色饱满。</summary>
        Metro = 33,
        /// <summary>Bootstrap：灰关绿开，白钮带柔影，配色自定义。</summary>
        Bootstrap = 34,
        /// <summary>Material3：小钮配主色光环。</summary>
        Material3 = 35,
        /// <summary>触感钮：3D 光泽渐变圆钮 + 明显投影。</summary>
        Tactile = 36,
        /// <summary>粉霓虹：深槽 + 品红辉光描边。</summary>
        NeonPink = 37,
        /// <summary>青柠霓虹：深槽 + 黄绿辉光描边。</summary>
        NeonLime = 38,
        /// <summary>极光：青紫双色渐变槽 + 内辉光。</summary>
        Aurora = 39,
        /// <summary>落日：粉橙双色渐变槽。</summary>
        Sunset = 40,
        /// <summary>翡翠：祖母绿实色槽 + 绿色辉光。</summary>
        Emerald = 41,
        /// <summary>皇家：靛蓝槽 + 鎏金圆钮。</summary>
        Royal = 42,
        /// <summary>碳纤维：深色斜纹槽 + 青色细边。</summary>
        Carbon = 43,
        /// <summary>磨砂玻璃：半透明白槽 + 同色描边。</summary>
        Glass = 44,
        /// <summary>新拟态：同底色凹凸浮雕槽与钮。</summary>
        Neumorph = 45,
        /// <summary>虚线风：白槽 + 主色虚线描边。</summary>
        Dashed = 46,
        /// <summary>双线风：白槽 + 同心双线描边。</summary>
        DoubleLine = 47,
        /// <summary>渐变描边：白槽 + 主色到橙的渐变环。</summary>
        GradientStroke = 48,
        /// <summary>合成波：深紫槽 + 品红青渐变钮 + 辉光。</summary>
        Synthwave = 49,
        /// <summary>终端机：黑绿槽 + 扫描线 + 荧光描边。</summary>
        Terminal = 50,
        /// <summary>指示灯：深色槽上的红绿发光小钮。</summary>
        Led = 51,
        /// <summary>锁扣：圆钮内挂锁随状态变色。</summary>
        Lock = 52,
        /// <summary>爱心：圆钮内心形随状态变红。</summary>
        Heart = 53,
        /// <summary>星标：圆钮内星星随状态点亮。</summary>
        Star = 54,
        /// <summary>云朵：圆钮内云朵随状态转蓝。</summary>
        Cloud = 55,
        /// <summary>音符：圆钮内八分音符。</summary>
        Music = 56,
        /// <summary>像素风：厚黑边方槽 + 米色方钮。</summary>
        Retro = 57,
        /// <summary>纸片：白槽 + 小尺寸墨黑圆钮。</summary>
        Paper = 58,
        /// <summary>石板：灰蓝槽 + 天蓝开态。</summary>
        Slate = 59,
        /// <summary>纯单色：灰黑槽 + 白钮无彩色。</summary>
        Mono = 60,
        /// <summary>脉冲：圆钮带两圈信号涟漪。</summary>
        Pulse = 61,
        /// <summary>守卫：圆钮内盾牌勾随状态着色。</summary>
        Guard = 62,
        /// <summary>新星：深蓝槽 + 主色渐变光环钮。</summary>
        Nova = 63
    }
}
