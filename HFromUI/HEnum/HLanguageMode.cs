using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HEnum
{
    public enum HLanguageMode
    {
        [HRemarkLanguage("自动获取")]
        auto = -1,
        [HRemarkLanguage("不翻译")]
        not = -2,
        [HRemarkLanguage("中文")]
        zh = 0,
        [HRemarkLanguage("英语")]
        en = 1,
        [HRemarkLanguage("越南语")]
        vie = 2,
        [HRemarkLanguage("法语")]
        fra = 3,
        [HRemarkLanguage("德语")]
        de = 4,
        [HRemarkLanguage("粤语")]
        yue = 5,
        [HRemarkLanguage("韩语")]
        kor = 6,
        [HRemarkLanguage("泰语")]
        th = 7,
        [HRemarkLanguage("葡萄牙语")]
        pt = 8,
        [HRemarkLanguage("希腊语")]
        el = 9,
        [HRemarkLanguage("保加利亚语")]
        bul = 10,
        [HRemarkLanguage("芬兰语")]
        fin = 11,
        [HRemarkLanguage("斯洛文尼亚语")]
        slo = 12,
        [HRemarkLanguage("繁体中文")]
        cht = 13,
        [HRemarkLanguage("文言文")]
        wyw = 14,
        [HRemarkLanguage("阿拉伯语")]
        ara = 15,
        [HRemarkLanguage("荷兰语")]
        nl = 16,
        [HRemarkLanguage("爱沙尼亚语")]
        est = 17,
        [HRemarkLanguage("捷克语")]
        cs = 18,
        [HRemarkLanguage("瑞典语")]
        swe = 19,
        [HRemarkLanguage("日语")]
        jp = 20,
        [HRemarkLanguage("西班牙语")]
        spa = 21,
        [HRemarkLanguage("俄语")]
        ru = 22,
        [HRemarkLanguage("意大利语")]
        it = 23,
        [HRemarkLanguage("波兰语")]
        pl = 24,
        [HRemarkLanguage("丹麦语")]
        dan = 25,
        [HRemarkLanguage("罗马尼亚语")]
        rom = 26,
        [HRemarkLanguage("匈牙利语")]
        hu = 27,
    }
}
