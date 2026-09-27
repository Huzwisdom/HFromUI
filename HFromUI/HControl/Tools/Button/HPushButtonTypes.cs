using System;

namespace HFromUI.HControl.Tools.Button
{
    /// <summary>外框形状（按钮族通用）。</summary>
    public enum HPushButtonShape
    {
        /// <summary>直角矩形。</summary>
        Rect = 0,
        /// <summary>圆角矩形（半径取 Radius 属性）。</summary>
        Round = 1,
        /// <summary>胶囊（半高圆角）。</summary>
        Capsule = 2,
        /// <summary>正圆/椭圆（由外接矩形决定）。</summary>
        Circle = 3
    }

    /// <summary>
    /// 预设风格（共 122 种）：30 个语义色 × 实心/描边/立体/渐变 4 种着色方式，
    /// 另加 Custom（自定义三态色）与 Link（链接）。Custom 时使用自定义三态色属性。
    /// 分段取值：实心 100~129，描边 200~229，立体 300~329，渐变 400~429。
    /// </summary>
    public enum HPushButtonStyle
    {
        /// <summary>自定义（使用 Normal/Hover/Pressed 系列颜色属性）。</summary>
        Custom = 0,
        /// <summary>链接（无框，悬停下划线）。</summary>
        Link = 1,

        #region 实心风格 100~129

        /// <summary>实心·主色蓝。</summary>
        FillBlue = 100,
        /// <summary>实心·中性灰。</summary>
        FillGray = 101,
        /// <summary>实心·成功绿。</summary>
        FillGreen = 102,
        /// <summary>实心·危险红。</summary>
        FillRed = 103,
        /// <summary>实心·警告琥珀。</summary>
        FillAmber = 104,
        /// <summary>实心·信息青。</summary>
        FillCyan = 105,
        /// <summary>实心·浅灰白。</summary>
        FillLight = 106,
        /// <summary>实心·深灰黑。</summary>
        FillDark = 107,
        /// <summary>实心·靛蓝。</summary>
        FillIndigo = 108,
        /// <summary>实心·紫色。</summary>
        FillPurple = 109,
        /// <summary>实心·粉玫。</summary>
        FillPink = 110,
        /// <summary>实心·玫瑰红。</summary>
        FillRose = 111,
        /// <summary>实心·橙色。</summary>
        FillOrange = 112,
        /// <summary>实心·蓝绿。</summary>
        FillTeal = 113,
        /// <summary>实心·翡翠绿。</summary>
        FillEmerald = 114,
        /// <summary>实心·青柠。</summary>
        FillLime = 115,
        /// <summary>实心·明黄。</summary>
        FillYellow = 116,
        /// <summary>实心·天青。</summary>
        FillSky = 117,
        /// <summary>实心·紫罗兰。</summary>
        FillViolet = 118,
        /// <summary>实心·品红。</summary>
        FillFuchsia = 119,
        /// <summary>实心·棕色。</summary>
        FillBrown = 120,
        /// <summary>实心·板岩。</summary>
        FillSlate = 121,
        /// <summary>实心·金黄。</summary>
        FillGold = 122,
        /// <summary>实心·深红。</summary>
        FillCrimson = 123,
        /// <summary>实心·薄荷。</summary>
        FillMint = 124,
        /// <summary>实心·海蓝。</summary>
        FillOcean = 125,
        /// <summary>实心·洋红。</summary>
        FillMagenta = 126,
        /// <summary>实心·橄榄。</summary>
        FillOlive = 127,
        /// <summary>实心·栗色。</summary>
        FillMaroon = 128,
        /// <summary>实心·炭灰。</summary>
        FillCharcoal = 129,

        #endregion

        #region 描边风格 200~229

        /// <summary>描边·主色蓝。</summary>
        OutlineBlue = 200,
        /// <summary>描边·中性灰。</summary>
        OutlineGray = 201,
        /// <summary>描边·成功绿。</summary>
        OutlineGreen = 202,
        /// <summary>描边·危险红。</summary>
        OutlineRed = 203,
        /// <summary>描边·警告琥珀。</summary>
        OutlineAmber = 204,
        /// <summary>描边·信息青。</summary>
        OutlineCyan = 205,
        /// <summary>描边·浅灰白。</summary>
        OutlineLight = 206,
        /// <summary>描边·深灰黑。</summary>
        OutlineDark = 207,
        /// <summary>描边·靛蓝。</summary>
        OutlineIndigo = 208,
        /// <summary>描边·紫色。</summary>
        OutlinePurple = 209,
        /// <summary>描边·粉玫。</summary>
        OutlinePink = 210,
        /// <summary>描边·玫瑰红。</summary>
        OutlineRose = 211,
        /// <summary>描边·橙色。</summary>
        OutlineOrange = 212,
        /// <summary>描边·蓝绿。</summary>
        OutlineTeal = 213,
        /// <summary>描边·翡翠绿。</summary>
        OutlineEmerald = 214,
        /// <summary>描边·青柠。</summary>
        OutlineLime = 215,
        /// <summary>描边·明黄。</summary>
        OutlineYellow = 216,
        /// <summary>描边·天青。</summary>
        OutlineSky = 217,
        /// <summary>描边·紫罗兰。</summary>
        OutlineViolet = 218,
        /// <summary>描边·品红。</summary>
        OutlineFuchsia = 219,
        /// <summary>描边·棕色。</summary>
        OutlineBrown = 220,
        /// <summary>描边·板岩。</summary>
        OutlineSlate = 221,
        /// <summary>描边·金黄。</summary>
        OutlineGold = 222,
        /// <summary>描边·深红。</summary>
        OutlineCrimson = 223,
        /// <summary>描边·薄荷。</summary>
        OutlineMint = 224,
        /// <summary>描边·海蓝。</summary>
        OutlineOcean = 225,
        /// <summary>描边·洋红。</summary>
        OutlineMagenta = 226,
        /// <summary>描边·橄榄。</summary>
        OutlineOlive = 227,
        /// <summary>描边·栗色。</summary>
        OutlineMaroon = 228,
        /// <summary>描边·炭灰。</summary>
        OutlineCharcoal = 229,

        #endregion

        #region 立体风格 300~329

        /// <summary>立体·主色蓝。</summary>
        ThreeDBlue = 300,
        /// <summary>立体·中性灰。</summary>
        ThreeDGray = 301,
        /// <summary>立体·成功绿。</summary>
        ThreeDGreen = 302,
        /// <summary>立体·危险红。</summary>
        ThreeDRed = 303,
        /// <summary>立体·警告琥珀。</summary>
        ThreeDAmber = 304,
        /// <summary>立体·信息青。</summary>
        ThreeDCyan = 305,
        /// <summary>立体·浅灰白。</summary>
        ThreeDLight = 306,
        /// <summary>立体·深灰黑。</summary>
        ThreeDDark = 307,
        /// <summary>立体·靛蓝。</summary>
        ThreeDIndigo = 308,
        /// <summary>立体·紫色。</summary>
        ThreeDPurple = 309,
        /// <summary>立体·粉玫。</summary>
        ThreeDPink = 310,
        /// <summary>立体·玫瑰红。</summary>
        ThreeDRose = 311,
        /// <summary>立体·橙色。</summary>
        ThreeDOrange = 312,
        /// <summary>立体·蓝绿。</summary>
        ThreeDTeal = 313,
        /// <summary>立体·翡翠绿。</summary>
        ThreeDEmerald = 314,
        /// <summary>立体·青柠。</summary>
        ThreeDLime = 315,
        /// <summary>立体·明黄。</summary>
        ThreeDYellow = 316,
        /// <summary>立体·天青。</summary>
        ThreeDSky = 317,
        /// <summary>立体·紫罗兰。</summary>
        ThreeDViolet = 318,
        /// <summary>立体·品红。</summary>
        ThreeDFuchsia = 319,
        /// <summary>立体·棕色。</summary>
        ThreeDBrown = 320,
        /// <summary>立体·板岩。</summary>
        ThreeDSlate = 321,
        /// <summary>立体·金黄。</summary>
        ThreeDGold = 322,
        /// <summary>立体·深红。</summary>
        ThreeDCrimson = 323,
        /// <summary>立体·薄荷。</summary>
        ThreeDMint = 324,
        /// <summary>立体·海蓝。</summary>
        ThreeDOcean = 325,
        /// <summary>立体·洋红。</summary>
        ThreeDMagenta = 326,
        /// <summary>立体·橄榄。</summary>
        ThreeDOlive = 327,
        /// <summary>立体·栗色。</summary>
        ThreeDMaroon = 328,
        /// <summary>立体·炭灰。</summary>
        ThreeDCharcoal = 329,

        #endregion

        #region 渐变风格 400~429

        /// <summary>渐变·主色蓝。</summary>
        GradientBlue = 400,
        /// <summary>渐变·中性灰。</summary>
        GradientGray = 401,
        /// <summary>渐变·成功绿。</summary>
        GradientGreen = 402,
        /// <summary>渐变·危险红。</summary>
        GradientRed = 403,
        /// <summary>渐变·警告琥珀。</summary>
        GradientAmber = 404,
        /// <summary>渐变·信息青。</summary>
        GradientCyan = 405,
        /// <summary>渐变·浅灰白。</summary>
        GradientLight = 406,
        /// <summary>渐变·深灰黑。</summary>
        GradientDark = 407,
        /// <summary>渐变·靛蓝。</summary>
        GradientIndigo = 408,
        /// <summary>渐变·紫色。</summary>
        GradientPurple = 409,
        /// <summary>渐变·粉玫。</summary>
        GradientPink = 410,
        /// <summary>渐变·玫瑰红。</summary>
        GradientRose = 411,
        /// <summary>渐变·日落橙红。</summary>
        GradientOrange = 412,
        /// <summary>渐变·蓝绿。</summary>
        GradientTeal = 413,
        /// <summary>渐变·翡翠绿。</summary>
        GradientEmerald = 414,
        /// <summary>渐变·青柠。</summary>
        GradientLime = 415,
        /// <summary>渐变·明黄。</summary>
        GradientYellow = 416,
        /// <summary>渐变·天青。</summary>
        GradientSky = 417,
        /// <summary>渐变·紫罗兰。</summary>
        GradientViolet = 418,
        /// <summary>渐变·品红。</summary>
        GradientFuchsia = 419,
        /// <summary>渐变·棕色。</summary>
        GradientBrown = 420,
        /// <summary>渐变·板岩。</summary>
        GradientSlate = 421,
        /// <summary>渐变·金黄。</summary>
        GradientGold = 422,
        /// <summary>渐变·深红。</summary>
        GradientCrimson = 423,
        /// <summary>渐变·薄荷。</summary>
        GradientMint = 424,
        /// <summary>渐变·海蓝。</summary>
        GradientOcean = 425,
        /// <summary>渐变·洋红。</summary>
        GradientMagenta = 426,
        /// <summary>渐变·橄榄。</summary>
        GradientOlive = 427,
        /// <summary>渐变·栗色。</summary>
        GradientMaroon = 428,
        /// <summary>渐变·炭灰。</summary>
        GradientCharcoal = 429,

        #endregion

        #region 旧命名别名（与新分段等价，保留兼容既有代码）

        /// <summary>旧名：等价 FillBlue。</summary>
        Primary = FillBlue,
        /// <summary>旧名：等价 FillGray。</summary>
        Secondary = FillGray,
        /// <summary>旧名：等价 FillGreen。</summary>
        Success = FillGreen,
        /// <summary>旧名：等价 FillRed。</summary>
        Danger = FillRed,
        /// <summary>旧名：等价 FillAmber。</summary>
        Warning = FillAmber,
        /// <summary>旧名：等价 FillCyan。</summary>
        Info = FillCyan,
        /// <summary>旧名：等价 OutlineBlue。</summary>
        OutlinePrimary = OutlineBlue,
        /// <summary>旧名：等价 OutlineGray。</summary>
        OutlineSecondary = OutlineGray,
        /// <summary>旧名：等价 OutlineGreen。</summary>
        OutlineSuccess = OutlineGreen,
        /// <summary>旧名：等价 OutlineRed。</summary>
        OutlineDanger = OutlineRed,
        /// <summary>旧名：等价 OutlineAmber。</summary>
        OutlineWarning = OutlineAmber,
        /// <summary>旧名：等价 OutlineCyan。</summary>
        OutlineInfo = OutlineCyan,
        /// <summary>旧名：等价 ThreeDBlue。</summary>
        ThreeDPrimary = ThreeDBlue,
        /// <summary>旧名：等价 ThreeDGray。</summary>
        ThreeDSecondary = ThreeDGray,
        /// <summary>旧名：等价 ThreeDGreen。</summary>
        ThreeDSuccess = ThreeDGreen,
        /// <summary>旧名：等价 ThreeDRed。</summary>
        ThreeDDanger = ThreeDRed,
        /// <summary>旧名：等价 ThreeDAmber。</summary>
        ThreeDWarning = ThreeDAmber,
        /// <summary>旧名：等价 ThreeDCyan。</summary>
        ThreeDInfo = ThreeDCyan

        #endregion
    }

    /// <summary>
    /// 内置矢量图标（共 129 种，全部 GDI+ 现画，不使用 Properties.Resources 图片）。
    /// 图标颜色自动跟随文字三态色（常规/悬停/按下），按 24 单位盒矢量缩放。
    /// </summary>
    public enum HPushButtonGlyph
    {
        /// <summary>无图标。</summary>
        None = 0,
        /// <summary>左箭头 ‹。</summary>
        ArrowLeft = 1,
        /// <summary>右箭头 ›。</summary>
        ArrowRight = 2,
        /// <summary>上箭头 ˄。</summary>
        ArrowUp = 3,
        /// <summary>下箭头 ˅。</summary>
        ArrowDown = 4,
        /// <summary>播放 ▶（实心三角）。</summary>
        Play = 5,
        /// <summary>暂停 ‖（双竖条）。</summary>
        Pause = 6,
        /// <summary>停止 ■（实心圆角方块）。</summary>
        Stop = 7,
        /// <summary>对勾 ✓。</summary>
        Check = 8,
        /// <summary>保存（软盘）。</summary>
        Save = 9,

        /// <summary>左尖括号 « 单个。</summary>
        ChevronLeft = 10,
        /// <summary>右尖括号 » 单个。</summary>
        ChevronRight = 11,
        /// <summary>上尖括号。</summary>
        ChevronUp = 12,
        /// <summary>下尖括号。</summary>
        ChevronDown = 13,
        /// <summary>双左尖括号。</summary>
        ChevronsLeft = 14,
        /// <summary>双右尖括号。</summary>
        ChevronsRight = 15,
        /// <summary>双上尖括号。</summary>
        ChevronsUp = 16,
        /// <summary>双下尖括号。</summary>
        ChevronsDown = 17,
        /// <summary>右上箭头（外链）。</summary>
        ArrowUpRight = 18,
        /// <summary>左下箭头。</summary>
        ArrowDownLeft = 19,
        /// <summary>登录（箭头入盒）。</summary>
        Login = 20,
        /// <summary>登出（箭头离盒）。</summary>
        Logout = 21,
        /// <summary>撤销（左转角箭头）。</summary>
        Undo = 22,
        /// <summary>重做（右转角箭头）。</summary>
        Redo = 23,
        /// <summary>刷新（环形双箭头）。</summary>
        Refresh = 24,
        /// <summary>顺时针旋转。</summary>
        RotateCw = 25,
        /// <summary>逆时针旋转。</summary>
        RotateCcw = 26,
        /// <summary>四向移动。</summary>
        Move = 27,
        /// <summary>展开（最大化）。</summary>
        Expand = 28,
        /// <summary>收起（最小化）。</summary>
        Shrink = 29,
        /// <summary>外部链接（盒+右上箭头）。</summary>
        ExternalLink = 30,
        /// <summary>升序排列。</summary>
        SortAsc = 31,
        /// <summary>降序排列。</summary>
        SortDesc = 32,
        /// <summary>筛选漏斗。</summary>
        Filter = 33,
        /// <summary>乱序（交叉箭头）。</summary>
        Shuffle = 34,
        /// <summary>循环（单曲/列表重复）。</summary>
        Repeat = 35,
        /// <summary>菜单（汉堡三横线）。</summary>
        Menu = 36,

        /// <summary>上一曲（三角+竖条）。</summary>
        SkipBack = 37,
        /// <summary>下一曲。</summary>
        SkipForward = 38,
        /// <summary>快进（双三角）。</summary>
        FastForward = 39,
        /// <summary>快退（双三角）。</summary>
        Rewind = 40,
        /// <summary>弹出（上三角+底线）。</summary>
        Eject = 41,
        /// <summary>录制（实心圆点）。</summary>
        Record = 42,
        /// <summary>音量（喇叭+三波）。</summary>
        Volume = 43,
        /// <summary>小音量（喇叭+一波）。</summary>
        VolumeLow = 44,
        /// <summary>静音（喇叭+叉）。</summary>
        VolumeMute = 45,
        /// <summary>麦克风。</summary>
        Microphone = 46,
        /// <summary>照相机。</summary>
        Camera = 47,
        /// <summary>摄像机。</summary>
        Video = 48,
        /// <summary>胶片。</summary>
        Film = 49,
        /// <summary>图片（画框+山日）。</summary>
        Picture = 50,
        /// <summary>音乐（音符）。</summary>
        Music = 51,
        /// <summary>通知铃。</summary>
        Bell = 52,
        /// <summary>通知铃关闭。</summary>
        BellOff = 53,

        /// <summary>关闭叉。</summary>
        X = 54,
        /// <summary>加号。</summary>
        Plus = 55,
        /// <summary>减号。</summary>
        Minus = 56,
        /// <summary>搜索放大镜。</summary>
        Search = 57,
        /// <summary>编辑铅笔。</summary>
        Pencil = 58,
        /// <summary>复制（双纸）。</summary>
        Copy = 59,
        /// <summary>剪切（剪刀）。</summary>
        Scissors = 60,
        /// <summary>粘贴板。</summary>
        Clipboard = 61,
        /// <summary>文件。</summary>
        File = 62,
        /// <summary>新建文件。</summary>
        FilePlus = 63,
        /// <summary>文件夹。</summary>
        Folder = 64,
        /// <summary>文件夹已完成（含对勾）。</summary>
        FolderCheck = 65,
        /// <summary>回收站。</summary>
        Trash = 66,
        /// <summary>打印机。</summary>
        Printer = 67,
        /// <summary>下载。</summary>
        Download = 68,
        /// <summary>上传。</summary>
        Upload = 69,
        /// <summary>云。</summary>
        Cloud = 70,
        /// <summary>链接（链环）。</summary>
        Link = 71,
        /// <summary>解除链接。</summary>
        Unlink = 72,
        /// <summary>粗体 B。</summary>
        Bold = 73,
        /// <summary>斜体 I。</summary>
        Italic = 74,
        /// <summary>下划线 U。</summary>
        Underline = 75,
        /// <summary>左对齐。</summary>
        AlignLeft = 76,
        /// <summary>居中对齐。</summary>
        AlignCenter = 77,
        /// <summary>右对齐。</summary>
        AlignRight = 78,
        /// <summary>项目符号列表。</summary>
        List = 79,
        /// <summary>编号列表。</summary>
        ListOrdered = 80,
        /// <summary>增加缩进。</summary>
        IndentRight = 81,
        /// <summary>减少缩进。</summary>
        IndentLeft = 82,
        /// <summary>表格。</summary>
        Table = 83,
        /// <summary>九宫格。</summary>
        Grid = 84,
        /// <summary>橡皮擦。</summary>
        Eraser = 85,
        /// <summary>画刷。</summary>
        Brush = 86,

        /// <summary>主页（房子）。</summary>
        Home = 87,
        /// <summary>设置（滑杆）。</summary>
        Settings = 88,
        /// <summary>单人。</summary>
        User = 89,
        /// <summary>双人。</summary>
        Users = 90,
        /// <summary>星标。</summary>
        Star = 91,
        /// <summary>爱心。</summary>
        Heart = 92,
        /// <summary>书签。</summary>
        Bookmark = 93,
        /// <summary>标签。</summary>
        Tag = 94,
        /// <summary>旗帜。</summary>
        Flag = 95,
        /// <summary>邮件信封。</summary>
        Mail = 96,
        /// <summary>对话气泡。</summary>
        Message = 97,
        /// <summary>电话听筒。</summary>
        Phone = 98,
        /// <summary>日历。</summary>
        Calendar = 99,
        /// <summary>时钟。</summary>
        Clock = 100,
        /// <summary>闭锁。</summary>
        Lock = 101,
        /// <summary>开锁。</summary>
        Unlock = 102,
        /// <summary>钥匙。</summary>
        Key = 103,
        /// <summary>眼睛（可见）。</summary>
        Eye = 104,
        /// <summary>眼睛关闭（不可见）。</summary>
        EyeOff = 105,
        /// <summary>信息圆圈。</summary>
        Info = 106,
        /// <summary>警告三角。</summary>
        Alert = 107,
        /// <summary>对勾圆圈。</summary>
        CheckCircle = 108,
        /// <summary>叉圆圈。</summary>
        XCircle = 109,
        /// <summary>加号圆圈。</summary>
        PlusCircle = 110,
        /// <summary>减号圆圈。</summary>
        MinusCircle = 111,
        /// <summary>帮助问号圆圈。</summary>
        HelpCircle = 112,
        /// <summary>盾牌（安全）。</summary>
        Shield = 113,
        /// <summary>靶心。</summary>
        Target = 114,
        /// <summary>十字准星。</summary>
        Crosshair = 115,
        /// <summary>闪电。</summary>
        Zap = 116,
        /// <summary>太阳。</summary>
        Sun = 117,
        /// <summary>月亮。</summary>
        Moon = 118,
        /// <summary>地图定位针。</summary>
        MapPin = 119,
        /// <summary>礼物盒。</summary>
        Gift = 120,
        /// <summary>奖章。</summary>
        Award = 121,
        /// <summary>赞（大拇指向上）。</summary>
        ThumbUp = 122,
        /// <summary>踩（大拇指向下）。</summary>
        ThumbDown = 123,
        /// <summary>柱状图。</summary>
        BarChart = 124,
        /// <summary>饼图。</summary>
        PieChart = 125,
        /// <summary>已核对清单。</summary>
        ClipboardCheck = 126,
        /// <summary>咖啡杯。</summary>
        Coffee = 127,
        /// <summary>扳手。</summary>
        Wrench = 128
    }
}
