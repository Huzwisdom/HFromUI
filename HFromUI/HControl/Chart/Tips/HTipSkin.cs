using System.Collections.Generic;
using System.Drawing;

namespace HFromUI.HControl.Chart.Tips
{
    using HFromUI.HLangage;
    /// <summary>
    /// 悬停气泡外形（64 种）：基础形（圆角/直角/胶囊/椭圆 × 六向尾巴）+ 思考泡泡 +
    /// 云朵/爆炸芒/星形/横幅/卷轴/游戏牌/墨迹/双气泡/箭头牌/票券/盾牌/便签/霓虹等参考图外形。
    /// 默认 Classic 与 HChart 提取前像素一致（7px 圆角、无尾、1px 边）。
    /// </summary>
    public enum HTipSkinKind
    {
        /// <summary>经典圆角（默认，提取前原样：r7 无尾 1px 边）。</summary>
        Classic = 0,
        // ----- 圆角气泡 7（六向尾巴） -----
        RoundedTailBL, RoundedTailBC, RoundedTailBR,
        RoundedTailTL, RoundedTailTC, RoundedTailTR,
        /// <summary>粗边圆角（2px 描边）。</summary>
        RoundedThick,
        /// <summary>双线圆角（外框 + 内框）。</summary>
        RoundedDouble,
        // ----- 直角气泡 7 -----
        Rect, RectTailBL, RectTailBC, RectTailBR, RectTailTL, RectTailTC, RectTailTR,
        // ----- 胶囊气泡 7 -----
        Capsule, CapTailBL, CapTailBC, CapTailBR, CapTailTL, CapTailTC, CapTailTR,
        // ----- 椭圆气泡 7 -----
        Oval, OvalTailBL, OvalTailBC, OvalTailBR, OvalTailTL, OvalTailTC, OvalTailTR,
        // ----- 思考泡泡（点链）2 -----
        ThinkRounded, ThinkOval,
        // ----- 云/爆炸/星形 6 -----
        Cloud, CloudTail, Burst, BurstTail, BurstSoft, BurstSpiky,
        // ----- 星牌 2 -----
        StarPlaque5, StarPlaque6,
        // ----- 横幅/卷轴 3 -----
        Banner, BannerTail, Scroll,
        // ----- 游戏标题牌（参考绿色描边叶饰牌）2 -----
        Plaque, PlaqueTail,
        // ----- 手绘墨迹 4 -----
        Ink, InkTail, InkCloud, InkOval,
        // ----- 双气泡组合（参考扁平双气泡图）2 -----
        DoubleBubble, DoubleBubbleFill,
        // ----- 点拖尾/箭头牌 3 -----
        ChatDot, ArrowL, ArrowR,
        // ----- 标签牌/票券/盾牌/文件夹 4 -----
        TagPill, Ticket, Shield, FolderTab,
        // ----- 便签 2 -----
        Note, NoteTail,
        // ----- 卡片/霓虹/叶饰/丝带 4 -----
        ShadowCard, NeonRounded, LeafPlaque, Ribbon
    }

    /// <summary>尾巴朝向（绘制时按气泡实际落点左右/上下翻转）。</summary>
    internal enum HTTail { None, BL, BC, BR, TL, TC, TR, Think }

    /// <summary>基础几何形。</summary>
    internal enum HTCore { Rounded, Rect, Capsule, Oval, Custom }

    /// <summary>单个气泡外形的几何描述。</summary>
    internal sealed class HTSkinDef
    {
        /// <summary>显示名（中文）。</summary>
        public string Name;
        /// <summary>基础几何（Custom 走 Special 编号自绘）。</summary>
        public HTCore Core;
        /// <summary>圆角半径（Rounded 用）。</summary>
        public float Radius;
        /// <summary>尾巴朝向。</summary>
        public HTTail Tail;
        /// <summary>边框宽度（像素）。</summary>
        public float BorderW = 1f;
        /// <summary>双线描边（外框内再套一圈细线）。</summary>
        public bool DoubleLine;
        /// <summary>右下柔投影。</summary>
        public bool Shadow;
        /// <summary>特殊外形编号（Core=Custom 时分派）。</summary>
        public int Special;
    }

    /// <summary>气泡外形表：64 种描述 + 测量（正文区→外框尺寸/文字原点/尾巴占位）。</summary>
    internal static class HTipSkins
    {
        /// <summary>尾巴占位高度（三角形/点链在框外占的空间）。</summary>
        public const float TailH = 10f;

        /// <summary>描述表缓存。</summary>
        private static readonly Dictionary<HTipSkinKind, HTSkinDef> _defs = new Dictionary<HTipSkinKind, HTSkinDef>();

        /// <summary>取外形描述。</summary>
        public static HTSkinDef Get(HTipSkinKind kind)
        {
            if (_defs.TryGetValue(kind, out var d)) return d;
            d = Build(kind);
            _defs[kind] = d;
            return d;
        }

        /// <summary>外形显示名。</summary>
        public static string Name(HTipSkinKind kind) => Get(kind).Name;

        /// <summary>构建 64 种外形描述。</summary>
        private static HTSkinDef Build(HTipSkinKind k)
        {
            switch (k)
            {
                // 经典 + 圆角六向尾 + 粗边/双线
                case HTipSkinKind.Classic: return Std(HTranslation.GetContent("经典圆角（默认）"), HTCore.Rounded, 7f, HTTail.None);
                case HTipSkinKind.RoundedTailBL: return Std(HTranslation.GetContent("圆角气泡·左下尾"), HTCore.Rounded, 7f, HTTail.BL);
                case HTipSkinKind.RoundedTailBC: return Std(HTranslation.GetContent("圆角气泡·下中尾"), HTCore.Rounded, 7f, HTTail.BC);
                case HTipSkinKind.RoundedTailBR: return Std(HTranslation.GetContent("圆角气泡·右下尾"), HTCore.Rounded, 7f, HTTail.BR);
                case HTipSkinKind.RoundedTailTL: return Std(HTranslation.GetContent("圆角气泡·左上尾"), HTCore.Rounded, 7f, HTTail.TL);
                case HTipSkinKind.RoundedTailTC: return Std(HTranslation.GetContent("圆角气泡·上中尾"), HTCore.Rounded, 7f, HTTail.TC);
                case HTipSkinKind.RoundedTailTR: return Std(HTranslation.GetContent("圆角气泡·右上尾"), HTCore.Rounded, 7f, HTTail.TR);
                case HTipSkinKind.RoundedThick: return Std(HTranslation.GetContent("粗边圆角"), HTCore.Rounded, 7f, HTTail.None, border: 2f);
                case HTipSkinKind.RoundedDouble: return Std(HTranslation.GetContent("双线圆角"), HTCore.Rounded, 7f, HTTail.None, dbl: true);
                // 直角七式
                case HTipSkinKind.Rect: return Std(HTranslation.GetContent("直角矩形"), HTCore.Rect, 0f, HTTail.None);
                case HTipSkinKind.RectTailBL: return Std(HTranslation.GetContent("直角气泡·左下尾"), HTCore.Rect, 0f, HTTail.BL);
                case HTipSkinKind.RectTailBC: return Std(HTranslation.GetContent("直角气泡·下中尾"), HTCore.Rect, 0f, HTTail.BC);
                case HTipSkinKind.RectTailBR: return Std(HTranslation.GetContent("直角气泡·右下尾"), HTCore.Rect, 0f, HTTail.BR);
                case HTipSkinKind.RectTailTL: return Std(HTranslation.GetContent("直角气泡·左上尾"), HTCore.Rect, 0f, HTTail.TL);
                case HTipSkinKind.RectTailTC: return Std(HTranslation.GetContent("直角气泡·上中尾"), HTCore.Rect, 0f, HTTail.TC);
                case HTipSkinKind.RectTailTR: return Std(HTranslation.GetContent("直角气泡·右上尾"), HTCore.Rect, 0f, HTTail.TR);
                // 胶囊七式
                case HTipSkinKind.Capsule: return Std(HTranslation.GetContent("胶囊条"), HTCore.Capsule, 0f, HTTail.None);
                case HTipSkinKind.CapTailBL: return Std(HTranslation.GetContent("胶囊气泡·左下尾"), HTCore.Capsule, 0f, HTTail.BL);
                case HTipSkinKind.CapTailBC: return Std(HTranslation.GetContent("胶囊气泡·下中尾"), HTCore.Capsule, 0f, HTTail.BC);
                case HTipSkinKind.CapTailBR: return Std(HTranslation.GetContent("胶囊气泡·右下尾"), HTCore.Capsule, 0f, HTTail.BR);
                case HTipSkinKind.CapTailTL: return Std(HTranslation.GetContent("胶囊气泡·左上尾"), HTCore.Capsule, 0f, HTTail.TL);
                case HTipSkinKind.CapTailTC: return Std(HTranslation.GetContent("胶囊气泡·上中尾"), HTCore.Capsule, 0f, HTTail.TC);
                case HTipSkinKind.CapTailTR: return Std(HTranslation.GetContent("胶囊气泡·右上尾"), HTCore.Capsule, 0f, HTTail.TR);
                // 椭圆七式
                case HTipSkinKind.Oval: return Std(HTranslation.GetContent("椭圆气泡"), HTCore.Oval, 0f, HTTail.None);
                case HTipSkinKind.OvalTailBL: return Std(HTranslation.GetContent("椭圆气泡·左下尾"), HTCore.Oval, 0f, HTTail.BL);
                case HTipSkinKind.OvalTailBC: return Std(HTranslation.GetContent("椭圆气泡·下中尾"), HTCore.Oval, 0f, HTTail.BC);
                case HTipSkinKind.OvalTailBR: return Std(HTranslation.GetContent("椭圆气泡·右下尾"), HTCore.Oval, 0f, HTTail.BR);
                case HTipSkinKind.OvalTailTL: return Std(HTranslation.GetContent("椭圆气泡·左上尾"), HTCore.Oval, 0f, HTTail.TL);
                case HTipSkinKind.OvalTailTC: return Std(HTranslation.GetContent("椭圆气泡·上中尾"), HTCore.Oval, 0f, HTTail.TC);
                case HTipSkinKind.OvalTailTR: return Std(HTranslation.GetContent("椭圆气泡·右上尾"), HTCore.Oval, 0f, HTTail.TR);
                // 思考泡泡
                case HTipSkinKind.ThinkRounded: return Std(HTranslation.GetContent("思考泡泡·圆角"), HTCore.Rounded, 7f, HTTail.Think);
                case HTipSkinKind.ThinkOval: return Std(HTranslation.GetContent("思考泡泡·椭圆"), HTCore.Oval, 0f, HTTail.Think);
                // 云/爆炸
                case HTipSkinKind.Cloud: return Custom(HTranslation.GetContent("云朵气泡"), 1);
                case HTipSkinKind.CloudTail: return Custom(HTranslation.GetContent("云朵气泡·下尾"), 2);
                case HTipSkinKind.Burst: return Custom(HTranslation.GetContent("爆炸芒气泡"), 3);
                case HTipSkinKind.BurstTail: return Custom(HTranslation.GetContent("爆炸芒气泡·下尾"), 4);
                case HTipSkinKind.BurstSoft: return Custom(HTranslation.GetContent("圆瓣柔爆"), 5);
                case HTipSkinKind.BurstSpiky: return Custom(HTranslation.GetContent("尖锐爆炸星"), 6);
                // 星牌
                case HTipSkinKind.StarPlaque5: return Custom(HTranslation.GetContent("五角星牌"), 7);
                case HTipSkinKind.StarPlaque6: return Custom(HTranslation.GetContent("六角星牌"), 8);
                // 横幅/卷轴
                case HTipSkinKind.Banner: return Custom(HTranslation.GetContent("丝带横幅（燕尾）"), 9);
                case HTipSkinKind.BannerTail: return Custom(HTranslation.GetContent("丝带横幅·下尾"), 10);
                case HTipSkinKind.Scroll: return Custom(HTranslation.GetContent("卷轴牌"), 11);
                // 游戏牌
                case HTipSkinKind.Plaque: return Custom(HTranslation.GetContent("游戏标题牌"), 12);
                case HTipSkinKind.PlaqueTail: return Custom(HTranslation.GetContent("游戏标题牌·下尾"), 13);
                // 墨迹
                case HTipSkinKind.Ink: return Custom(HTranslation.GetContent("墨迹方框"), 14);
                case HTipSkinKind.InkTail: return Custom(HTranslation.GetContent("墨迹方框·下尾"), 15);
                case HTipSkinKind.InkCloud: return Custom(HTranslation.GetContent("墨迹云团"), 16);
                case HTipSkinKind.InkOval: return Custom(HTranslation.GetContent("墨迹椭圆"), 17);
                // 双气泡
                case HTipSkinKind.DoubleBubble: return Custom(HTranslation.GetContent("双气泡·描边"), 18);
                case HTipSkinKind.DoubleBubbleFill: return Custom(HTranslation.GetContent("双气泡·实底"), 19);
                // 点拖尾/箭头
                case HTipSkinKind.ChatDot: return Custom(HTranslation.GetContent("圆点拖尾"), 20);
                case HTipSkinKind.ArrowL: return Custom(HTranslation.GetContent("左向箭头牌"), 21);
                case HTipSkinKind.ArrowR: return Custom(HTranslation.GetContent("右向箭头牌"), 22);
                // 标签/票券/盾/文件夹
                case HTipSkinKind.TagPill: return Custom(HTranslation.GetContent("标签牌"), 23);
                case HTipSkinKind.Ticket: return Custom(HTranslation.GetContent("票券"), 24);
                case HTipSkinKind.Shield: return Custom(HTranslation.GetContent("盾牌"), 25);
                case HTipSkinKind.FolderTab: return Custom(HTranslation.GetContent("文件夹标签"), 26);
                // 便签
                case HTipSkinKind.Note: return Custom(HTranslation.GetContent("折角便签"), 27);
                case HTipSkinKind.NoteTail: return Custom(HTranslation.GetContent("折角便签·下尾"), 28);
                // 卡片/霓虹/叶饰/丝带
                case HTipSkinKind.ShadowCard: return Custom(HTranslation.GetContent("投影卡片"), 29);
                case HTipSkinKind.NeonRounded: return Custom(HTranslation.GetContent("霓虹描边"), 30);
                case HTipSkinKind.LeafPlaque: return Custom(HTranslation.GetContent("叶饰描边牌"), 31);
                case HTipSkinKind.Ribbon: return Custom(HTranslation.GetContent("尖角丝带"), 32);
            }
            return Std(HTranslation.GetContent("经典圆角（默认）"), HTCore.Rounded, 7f, HTTail.None);
        }

        /// <summary>标准形描述简写。</summary>
        private static HTSkinDef Std(string name, HTCore core, float r, HTTail tail, float border = 1f, bool dbl = false)
            => new HTSkinDef { Name = name, Core = core, Radius = r, Tail = tail, BorderW = border, DoubleLine = dbl };

        /// <summary>特殊形描述简写。</summary>
        private static HTSkinDef Custom(string name, int special)
            => new HTSkinDef { Name = name, Core = HTCore.Custom, Special = special };

        /// <summary>尾巴经落点翻转后的实际朝向（水平不够翻左、垂直不够翻上）：水平翻转 L↔R，垂直翻转 B↔T。</summary>
        public static HTTail FlipTail(HTTail t, bool flipH, bool flipV)
        {
            if (t == HTTail.None || t == HTTail.Think) return t;
            if (flipH)
            {
                switch (t)
                {
                    case HTTail.BL: t = HTTail.BR; break;
                    case HTTail.BR: t = HTTail.BL; break;
                    case HTTail.TL: t = HTTail.TR; break;
                    case HTTail.TR: t = HTTail.TL; break;
                }
            }
            if (flipV)
            {
                switch (t)
                {
                    case HTTail.BL: t = HTTail.TL; break;
                    case HTTail.BC: t = HTTail.TC; break;
                    case HTTail.BR: t = HTTail.TR; break;
                    case HTTail.TL: t = HTTail.BL; break;
                    case HTTail.TC: t = HTTail.BC; break;
                    case HTTail.TR: t = HTTail.BR; break;
                }
            }
            return t;
        }
    }
}
