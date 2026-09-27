using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HFromUI; // 翻译类所在命名空间
using HFromUI.HEnum;

namespace HFromUI.HConvert
{
    using HFromUI.HLangage;


    /// <summary>
    /// 全能字符串验证工具类，涵盖邮箱、电话、身份证、IP、URL、银行卡、护照、ISBN、QQ、微信、经纬度、字符类型位置判断等50+种验证与解析功能。
    /// 所有返回给调用者的文本均通过 <see cref="HTranslation.GetContent"/> 进行多语言翻译。
    /// </summary>
    public static class HStringValidator
    {

        #region 预编译正则表达式

        /// <summary>EmailRegex 字段。</summary>
        private static readonly Regex EmailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
        /// <summary>ChineseMobileRegex 字段。</summary>
        private static readonly Regex ChineseMobileRegex = new Regex(@"^1[3-9]\d{9}$", RegexOptions.Compiled);
        /// <summary>ChineseFixedPhoneRegex 字段。</summary>
        private static readonly Regex ChineseFixedPhoneRegex = new Regex(@"^(0\d{2,3})[- ]?\d{7,8}$", RegexOptions.Compiled);
        /// <summary>InternationalPhoneRegex 字段。</summary>
        private static readonly Regex InternationalPhoneRegex = new Regex(@"^\+\d{1,3}[\s.-]?\d{1,14}$", RegexOptions.Compiled);
        /// <summary>IdCard18Regex 字段。</summary>
        private static readonly Regex IdCard18Regex = new Regex(@"^\d{17}[\dXx]$", RegexOptions.Compiled);
        /// <summary>IdCard15Regex 字段。</summary>
        private static readonly Regex IdCard15Regex = new Regex(@"^\d{15}$", RegexOptions.Compiled);
        /// <summary>IntegerRegex 字段。</summary>
        private static readonly Regex IntegerRegex = new Regex(@"^-?\d+$", RegexOptions.Compiled);
        /// <summary>DecimalRegex 字段。</summary>
        private static readonly Regex DecimalRegex = new Regex(@"^-?\d+(\.\d+)?$", RegexOptions.Compiled);
        /// <summary>LettersRegex 字段。</summary>
        private static readonly Regex LettersRegex = new Regex(@"^[a-zA-Z]+$", RegexOptions.Compiled);
        /// <summary>AlphaNumericRegex 字段。</summary>
        private static readonly Regex AlphaNumericRegex = new Regex(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled);
        /// <summary>ChineseRegex 字段。</summary>
        private static readonly Regex ChineseRegex = new Regex(@"^[\u4e00-\u9fa5]+$", RegexOptions.Compiled);
        /// <summary>IPv4Regex 字段。</summary>
        private static readonly Regex IPv4Regex = new Regex(@"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$", RegexOptions.Compiled);
        /// <summary>IPv6Regex 字段。</summary>
        private static readonly Regex IPv6Regex = new Regex(
            @"^(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]+|::(ffff(:0{1,4})?:)?((25[0-5]|(2[0-4]|1?\d)?\d)\.){3}(25[0-5]|(2[0-4]|1?\d)?\d)|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1?\d)?\d)\.){3}(25[0-5]|(2[0-4]|1?\d)?\d))$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>UrlRegex 字段。</summary>
        private static readonly Regex UrlRegex = new Regex(@"^(https?|ftp)://[^\s/$.?#].[^\s]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>ChinesePostalCodeRegex 字段。</summary>
        private static readonly Regex ChinesePostalCodeRegex = new Regex(@"^\d{6}$", RegexOptions.Compiled);
        /// <summary>CurrencyRegex 字段。</summary>
        private static readonly Regex CurrencyRegex = new Regex(@"^-?\d{1,3}(,\d{3})*(\.\d{1,2})?$", RegexOptions.Compiled);
        /// <summary>GuidRegex 字段。</summary>
        private static readonly Regex GuidRegex = new Regex(@"^(\{)?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}(\})?$", RegexOptions.Compiled);
        /// <summary>Base64Regex 字段。</summary>
        private static readonly Regex Base64Regex = new Regex(@"^[a-zA-Z0-9\+/]*={0,2}$", RegexOptions.Compiled);
        /// <summary>ChineseCharRegex 字段。</summary>
        private static readonly Regex ChineseCharRegex = new Regex(@"[\u4e00-\u9fa5]");
        /// <summary>EnglishLetterRegex 字段。</summary>
        private static readonly Regex EnglishLetterRegex = new Regex(@"[a-zA-Z]");
        /// <summary>MacAddressRegex 字段。</summary>
        private static readonly Regex MacAddressRegex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}$", RegexOptions.Compiled);
        /// <summary>DomainRegex 字段。</summary>
        private static readonly Regex DomainRegex = new Regex(@"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*$", RegexOptions.Compiled);
        /// <summary>HexColorRegex 字段。</summary>
        private static readonly Regex HexColorRegex = new Regex(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled);
        /// <summary>ChineseLicensePlateRegex 字段。</summary>
        private static readonly Regex ChineseLicensePlateRegex = new Regex(
            HTranslation.GetContent(@"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤川青藏琼宁][A-HJ-NP-Z][A-HJ-NP-Z0-9]{4,5}[A-HJ-NP-Z0-9挂学警港澳]$"),
            RegexOptions.Compiled);
        /// <summary>ChinesePassportRegex 字段。</summary>
        private static readonly Regex ChinesePassportRegex = new Regex(@"^E\d{8}$", RegexOptions.Compiled);
        /// <summary>QQRegex 字段。</summary>
        private static readonly Regex QQRegex = new Regex(@"^[1-9]\d{4,10}$", RegexOptions.Compiled);
        /// <summary>WeChatIdRegex 字段。</summary>
        private static readonly Regex WeChatIdRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9_-]{5,19}$", RegexOptions.Compiled);
        /// <summary>ChineseNameRegex 字段。</summary>
        private static readonly Regex ChineseNameRegex = new Regex(@"^[\u4e00-\u9fa5]{2,4}$", RegexOptions.Compiled);
        /// <summary>ImageExtensionRegex 字段。</summary>
        private static readonly Regex ImageExtensionRegex = new Regex(@"\.(jpg|jpeg|png|gif|bmp|webp|svg|ico|tiff?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        /// <summary>HtmlTagRegex 字段。</summary>
        private static readonly Regex HtmlTagRegex = new Regex(@"<[^>]+>", RegexOptions.Compiled);
        /// <summary>SocialSecurityCardRegex 字段。</summary>
        private static readonly Regex SocialSecurityCardRegex = new Regex(@"^[0-9A-Z]{18}$", RegexOptions.Compiled);
        /// <summary>MainlandTravelPermitRegex 字段。</summary>
        private static readonly Regex MainlandTravelPermitRegex = new Regex(@"^[HMhm]\d{10}$|^\d{9}$", RegexOptions.Compiled);
        /// <summary>TaiwanEntryPermitRegex 字段。</summary>
        private static readonly Regex TaiwanEntryPermitRegex = new Regex(@"^\d{8}$|^[0-9A-Z]{8,10}$", RegexOptions.Compiled);
        /// <summary>VatInvoiceCodeRegex 字段。</summary>
        private static readonly Regex VatInvoiceCodeRegex = new Regex(@"^\d{10}$|^\d{12}$", RegexOptions.Compiled);
        /// <summary>VatInvoiceNumberRegex 字段。</summary>
        private static readonly Regex VatInvoiceNumberRegex = new Regex(@"^\d{8}$", RegexOptions.Compiled);
        /// <summary>FilePathRegex 字段。</summary>
        private static readonly Regex FilePathRegex = new Regex(
            @"^([a-zA-Z]:\\[^*|""<>?\r\n\\]*|\\\\[^\\*?|""<>]+\\[^*?|""<>\\]*|/[^/][^*?|""<>\0]*)$",
            RegexOptions.Compiled);

        #endregion

        #region 完整数据字典

        private static readonly Dictionary<string, (string Province, string City)> ChinaAreaCodeMap =
            new Dictionary<string, (string, string)>
            {
                { "010", (HTranslation.GetContent("北京市"), HTranslation.GetContent("北京")) }, { "020", (HTranslation.GetContent("广东省"), HTranslation.GetContent("广州")) }, { "021", (HTranslation.GetContent("上海市"), HTranslation.GetContent("上海")) },
                { "022", (HTranslation.GetContent("天津市"), HTranslation.GetContent("天津")) }, { "023", (HTranslation.GetContent("重庆市"), HTranslation.GetContent("重庆")) }, { "024", (HTranslation.GetContent("辽宁省"), HTranslation.GetContent("沈阳")) },
                { "025", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("南京")) }, { "027", (HTranslation.GetContent("湖北省"), HTranslation.GetContent("武汉")) }, { "028", (HTranslation.GetContent("四川省"), HTranslation.GetContent("成都")) },
                { "029", (HTranslation.GetContent("陕西省"), HTranslation.GetContent("西安")) },
                { "0311", (HTranslation.GetContent("河北省"), HTranslation.GetContent("石家庄")) }, { "0312", (HTranslation.GetContent("河北省"), HTranslation.GetContent("保定")) }, { "0313", (HTranslation.GetContent("河北省"), HTranslation.GetContent("张家口")) },
                { "0314", (HTranslation.GetContent("河北省"), HTranslation.GetContent("承德")) }, { "0315", (HTranslation.GetContent("河北省"), HTranslation.GetContent("唐山")) }, { "0316", (HTranslation.GetContent("河北省"), HTranslation.GetContent("廊坊")) },
                { "0317", (HTranslation.GetContent("河北省"), HTranslation.GetContent("沧州")) }, { "0318", (HTranslation.GetContent("河北省"), HTranslation.GetContent("衡水")) }, { "0319", (HTranslation.GetContent("河北省"), HTranslation.GetContent("邢台")) },
                { "0335", (HTranslation.GetContent("河北省"), HTranslation.GetContent("秦皇岛")) },
                { "0351", (HTranslation.GetContent("山西省"), HTranslation.GetContent("太原")) }, { "0352", (HTranslation.GetContent("山西省"), HTranslation.GetContent("大同")) }, { "0353", (HTranslation.GetContent("山西省"), HTranslation.GetContent("阳泉")) },
                { "0354", (HTranslation.GetContent("山西省"), HTranslation.GetContent("晋中")) }, { "0355", (HTranslation.GetContent("山西省"), HTranslation.GetContent("长治")) }, { "0356", (HTranslation.GetContent("山西省"), HTranslation.GetContent("晋城")) },
                { "0357", (HTranslation.GetContent("山西省"), HTranslation.GetContent("临汾")) }, { "0358", (HTranslation.GetContent("山西省"), HTranslation.GetContent("吕梁")) }, { "0359", (HTranslation.GetContent("山西省"), HTranslation.GetContent("运城")) },
                { "0371", (HTranslation.GetContent("河南省"), HTranslation.GetContent("郑州")) }, { "0372", (HTranslation.GetContent("河南省"), HTranslation.GetContent("安阳")) }, { "0373", (HTranslation.GetContent("河南省"), HTranslation.GetContent("新乡")) },
                { "0374", (HTranslation.GetContent("河南省"), HTranslation.GetContent("许昌")) }, { "0375", (HTranslation.GetContent("河南省"), HTranslation.GetContent("平顶山")) }, { "0376", (HTranslation.GetContent("河南省"), HTranslation.GetContent("信阳")) },
                { "0377", (HTranslation.GetContent("河南省"), HTranslation.GetContent("南阳")) }, { "0378", (HTranslation.GetContent("河南省"), HTranslation.GetContent("开封")) }, { "0379", (HTranslation.GetContent("河南省"), HTranslation.GetContent("洛阳")) },
                { "0391", (HTranslation.GetContent("河南省"), HTranslation.GetContent("焦作")) }, { "0392", (HTranslation.GetContent("河南省"), HTranslation.GetContent("鹤壁")) }, { "0393", (HTranslation.GetContent("河南省"), HTranslation.GetContent("濮阳")) },
                { "0394", (HTranslation.GetContent("河南省"), HTranslation.GetContent("周口")) }, { "0395", (HTranslation.GetContent("河南省"), HTranslation.GetContent("漯河")) }, { "0396", (HTranslation.GetContent("河南省"), HTranslation.GetContent("驻马店")) },
                { "0431", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("长春")) }, { "0432", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("吉林")) }, { "0433", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("延边")) },
                { "0434", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("四平")) }, { "0435", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("通化")) }, { "0436", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("白城")) },
                { "0437", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("辽源")) }, { "0438", (HTranslation.GetContent("吉林省"), HTranslation.GetContent("松原")) },
                { "0451", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("哈尔滨")) }, { "0452", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("齐齐哈尔")) }, { "0453", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("牡丹江")) },
                { "0454", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("佳木斯")) }, { "0455", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("绥化")) }, { "0456", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("黑河")) },
                { "0457", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("大兴安岭")) }, { "0458", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("伊春")) }, { "0459", (HTranslation.GetContent("黑龙江省"), HTranslation.GetContent("大庆")) },
                { "0471", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("呼和浩特")) }, { "0472", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("包头")) }, { "0473", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("乌海")) },
                { "0474", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("乌兰察布")) }, { "0475", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("通辽")) }, { "0476", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("赤峰")) },
                { "0477", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("鄂尔多斯")) }, { "0478", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("巴彦淖尔")) }, { "0479", (HTranslation.GetContent("内蒙古自治区"), HTranslation.GetContent("锡林郭勒")) },
                { "0531", (HTranslation.GetContent("山东省"), HTranslation.GetContent("济南")) }, { "0532", (HTranslation.GetContent("山东省"), HTranslation.GetContent("青岛")) }, { "0533", (HTranslation.GetContent("山东省"), HTranslation.GetContent("淄博")) },
                { "0534", (HTranslation.GetContent("山东省"), HTranslation.GetContent("德州")) }, { "0535", (HTranslation.GetContent("山东省"), HTranslation.GetContent("烟台")) }, { "0536", (HTranslation.GetContent("山东省"), HTranslation.GetContent("潍坊")) },
                { "0537", (HTranslation.GetContent("山东省"), HTranslation.GetContent("济宁")) }, { "0538", (HTranslation.GetContent("山东省"), HTranslation.GetContent("泰安")) }, { "0539", (HTranslation.GetContent("山东省"), HTranslation.GetContent("临沂")) },
                { "0551", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("合肥")) }, { "0552", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("蚌埠")) }, { "0553", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("芜湖")) },
                { "0554", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("淮南")) }, { "0555", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("马鞍山")) }, { "0556", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("安庆")) },
                { "0557", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("宿州")) }, { "0558", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("阜阳")) }, { "0559", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("黄山")) },
                { "0561", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("淮北")) }, { "0562", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("铜陵")) }, { "0563", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("宣城")) },
                { "0564", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("六安")) }, { "0566", (HTranslation.GetContent("安徽省"), HTranslation.GetContent("池州")) },
                { "0571", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("杭州")) }, { "0572", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("湖州")) }, { "0573", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("嘉兴")) },
                { "0574", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("宁波")) }, { "0575", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("绍兴")) }, { "0576", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("台州")) },
                { "0577", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("温州")) }, { "0578", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("丽水")) }, { "0579", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("金华")) },
                { "0580", (HTranslation.GetContent("浙江省"), HTranslation.GetContent("舟山")) },
                { "0591", (HTranslation.GetContent("福建省"), HTranslation.GetContent("福州")) }, { "0592", (HTranslation.GetContent("福建省"), HTranslation.GetContent("厦门")) }, { "0593", (HTranslation.GetContent("福建省"), HTranslation.GetContent("宁德")) },
                { "0594", (HTranslation.GetContent("福建省"), HTranslation.GetContent("莆田")) }, { "0595", (HTranslation.GetContent("福建省"), HTranslation.GetContent("泉州")) }, { "0596", (HTranslation.GetContent("福建省"), HTranslation.GetContent("漳州")) },
                { "0597", (HTranslation.GetContent("福建省"), HTranslation.GetContent("龙岩")) }, { "0598", (HTranslation.GetContent("福建省"), HTranslation.GetContent("三明")) }, { "0599", (HTranslation.GetContent("福建省"), HTranslation.GetContent("南平")) },
                { "0731", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("长沙")) }, { "0732", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("湘潭")) }, { "0733", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("株洲")) },
                { "0734", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("衡阳")) }, { "0735", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("郴州")) }, { "0736", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("常德")) },
                { "0737", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("益阳")) }, { "0738", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("娄底")) }, { "0739", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("邵阳")) },
                { "0743", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("湘西")) }, { "0744", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("张家界")) }, { "0745", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("怀化")) },
                { "0746", (HTranslation.GetContent("湖南省"), HTranslation.GetContent("永州")) },
                { "0750", (HTranslation.GetContent("广东省"), HTranslation.GetContent("江门")) }, { "0751", (HTranslation.GetContent("广东省"), HTranslation.GetContent("韶关")) }, { "0752", (HTranslation.GetContent("广东省"), HTranslation.GetContent("惠州")) },
                { "0753", (HTranslation.GetContent("广东省"), HTranslation.GetContent("梅州")) }, { "0754", (HTranslation.GetContent("广东省"), HTranslation.GetContent("汕头")) }, { "0755", (HTranslation.GetContent("广东省"), HTranslation.GetContent("深圳")) },
                { "0756", (HTranslation.GetContent("广东省"), HTranslation.GetContent("珠海")) }, { "0757", (HTranslation.GetContent("广东省"), HTranslation.GetContent("佛山")) }, { "0758", (HTranslation.GetContent("广东省"), HTranslation.GetContent("肇庆")) },
                { "0759", (HTranslation.GetContent("广东省"), HTranslation.GetContent("湛江")) }, { "0760", (HTranslation.GetContent("广东省"), HTranslation.GetContent("中山")) }, { "0762", (HTranslation.GetContent("广东省"), HTranslation.GetContent("河源")) },
                { "0763", (HTranslation.GetContent("广东省"), HTranslation.GetContent("清远")) }, { "0766", (HTranslation.GetContent("广东省"), HTranslation.GetContent("云浮")) }, { "0768", (HTranslation.GetContent("广东省"), HTranslation.GetContent("潮州")) },
                { "0769", (HTranslation.GetContent("广东省"), HTranslation.GetContent("东莞")) }, { "0660", (HTranslation.GetContent("广东省"), HTranslation.GetContent("汕尾")) }, { "0662", (HTranslation.GetContent("广东省"), HTranslation.GetContent("阳江")) },
                { "0663", (HTranslation.GetContent("广东省"), HTranslation.GetContent("揭阳")) }, { "0668", (HTranslation.GetContent("广东省"), HTranslation.GetContent("茂名")) },
                { "0771", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("南宁")) }, { "0772", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("柳州")) }, { "0773", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("桂林")) },
                { "0774", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("梧州")) }, { "0775", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("贵港")) }, { "0776", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("百色")) },
                { "0777", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("钦州")) }, { "0778", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("河池")) }, { "0779", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("北海")) },
                { "0791", (HTranslation.GetContent("江西省"), HTranslation.GetContent("南昌")) }, { "0792", (HTranslation.GetContent("江西省"), HTranslation.GetContent("九江")) }, { "0793", (HTranslation.GetContent("江西省"), HTranslation.GetContent("上饶")) },
                { "0794", (HTranslation.GetContent("江西省"), HTranslation.GetContent("抚州")) }, { "0795", (HTranslation.GetContent("江西省"), HTranslation.GetContent("宜春")) }, { "0796", (HTranslation.GetContent("江西省"), HTranslation.GetContent("吉安")) },
                { "0797", (HTranslation.GetContent("江西省"), HTranslation.GetContent("赣州")) }, { "0798", (HTranslation.GetContent("江西省"), HTranslation.GetContent("景德镇")) }, { "0799", (HTranslation.GetContent("江西省"), HTranslation.GetContent("萍乡")) },
                { "0851", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("贵阳")) }, { "0852", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("遵义")) }, { "0853", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("安顺")) },
                { "0854", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("黔南")) }, { "0855", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("黔东南")) }, { "0856", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("铜仁")) },
                { "0857", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("毕节")) }, { "0858", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("六盘水")) }, { "0859", (HTranslation.GetContent("贵州省"), HTranslation.GetContent("黔西南")) },
                { "0871", (HTranslation.GetContent("云南省"), HTranslation.GetContent("昆明")) }, { "0872", (HTranslation.GetContent("云南省"), HTranslation.GetContent("大理")) }, { "0873", (HTranslation.GetContent("云南省"), HTranslation.GetContent("红河")) },
                { "0874", (HTranslation.GetContent("云南省"), HTranslation.GetContent("曲靖")) }, { "0875", (HTranslation.GetContent("云南省"), HTranslation.GetContent("保山")) }, { "0876", (HTranslation.GetContent("云南省"), HTranslation.GetContent("文山")) },
                { "0877", (HTranslation.GetContent("云南省"), HTranslation.GetContent("玉溪")) }, { "0878", (HTranslation.GetContent("云南省"), HTranslation.GetContent("楚雄")) }, { "0879", (HTranslation.GetContent("云南省"), HTranslation.GetContent("普洱")) },
                { "0891", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("拉萨")) }, { "0892", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("日喀则")) }, { "0893", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("山南")) },
                { "0894", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("林芝")) }, { "0895", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("昌都")) }, { "0896", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("那曲")) },
                { "0897", (HTranslation.GetContent("西藏自治区"), HTranslation.GetContent("阿里")) },
                { "0931", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("兰州")) }, { "0932", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("定西")) }, { "0933", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("平凉")) },
                { "0934", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("庆阳")) }, { "0935", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("武威")) }, { "0936", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("张掖")) },
                { "0937", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("酒泉")) }, { "0938", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("天水")) }, { "0939", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("陇南")) },
                { "0941", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("甘南")) }, { "0943", (HTranslation.GetContent("甘肃省"), HTranslation.GetContent("白银")) },
                { "0951", (HTranslation.GetContent("宁夏回族自治区"), HTranslation.GetContent("银川")) }, { "0952", (HTranslation.GetContent("宁夏回族自治区"), HTranslation.GetContent("石嘴山")) }, { "0953", (HTranslation.GetContent("宁夏回族自治区"), HTranslation.GetContent("吴忠")) },
                { "0954", (HTranslation.GetContent("宁夏回族自治区"), HTranslation.GetContent("固原")) }, { "0955", (HTranslation.GetContent("宁夏回族自治区"), HTranslation.GetContent("中卫")) },
                { "0971", (HTranslation.GetContent("青海省"), HTranslation.GetContent("西宁")) }, { "0972", (HTranslation.GetContent("青海省"), HTranslation.GetContent("海东")) }, { "0973", (HTranslation.GetContent("青海省"), HTranslation.GetContent("黄南")) },
                { "0974", (HTranslation.GetContent("青海省"), HTranslation.GetContent("海南")) }, { "0975", (HTranslation.GetContent("青海省"), HTranslation.GetContent("果洛")) }, { "0976", (HTranslation.GetContent("青海省"), HTranslation.GetContent("玉树")) },
                { "0977", (HTranslation.GetContent("青海省"), HTranslation.GetContent("海西")) }, { "0979", (HTranslation.GetContent("青海省"), HTranslation.GetContent("格尔木")) },
                { "0991", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("乌鲁木齐")) }, { "0992", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("奎屯")) }, { "0993", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("石河子")) },
                { "0994", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("昌吉")) }, { "0995", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("吐鲁番")) }, { "0996", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("巴音郭楞")) },
                { "0997", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("阿克苏")) }, { "0998", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("喀什")) }, { "0999", (HTranslation.GetContent("新疆维吾尔自治区"), HTranslation.GetContent("伊犁")) },
                { "0510", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("无锡")) }, { "0511", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("镇江")) }, { "0512", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("苏州")) },
                { "0513", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("南通")) }, { "0514", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("扬州")) }, { "0515", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("盐城")) },
                { "0516", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("徐州")) }, { "0517", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("淮安")) }, { "0518", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("连云港")) },
                { "0519", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("常州")) }, { "0523", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("泰州")) }, { "0527", (HTranslation.GetContent("江苏省"), HTranslation.GetContent("宿迁")) },
                { "0770", (HTranslation.GetContent("广西壮族自治区"), HTranslation.GetContent("防城港")) },
                { "0870", (HTranslation.GetContent("云南省"), HTranslation.GetContent("昭通")) }, { "0883", (HTranslation.GetContent("云南省"), HTranslation.GetContent("临沧")) }, { "0886", (HTranslation.GetContent("云南省"), HTranslation.GetContent("怒江")) },
                { "0887", (HTranslation.GetContent("云南省"), HTranslation.GetContent("迪庆")) }, { "0888", (HTranslation.GetContent("云南省"), HTranslation.GetContent("丽江")) },
                { "0691", (HTranslation.GetContent("云南省"), HTranslation.GetContent("西双版纳")) }, { "0692", (HTranslation.GetContent("云南省"), HTranslation.GetContent("德宏")) },
                { "0830", (HTranslation.GetContent("四川省"), HTranslation.GetContent("泸州")) }, { "0831", (HTranslation.GetContent("四川省"), HTranslation.GetContent("宜宾")) }, { "0832", (HTranslation.GetContent("四川省"), HTranslation.GetContent("资阳")) },
                { "0833", (HTranslation.GetContent("四川省"), HTranslation.GetContent("乐山")) }, { "0834", (HTranslation.GetContent("四川省"), HTranslation.GetContent("凉山")) }, { "0835", (HTranslation.GetContent("四川省"), HTranslation.GetContent("雅安")) },
                { "0836", (HTranslation.GetContent("四川省"), HTranslation.GetContent("甘孜")) }, { "0837", (HTranslation.GetContent("四川省"), HTranslation.GetContent("阿坝")) }, { "0838", (HTranslation.GetContent("四川省"), HTranslation.GetContent("德阳")) },
                { "0839", (HTranslation.GetContent("四川省"), HTranslation.GetContent("广元")) },
                { "0812", (HTranslation.GetContent("四川省"), HTranslation.GetContent("攀枝花")) }, { "0813", (HTranslation.GetContent("四川省"), HTranslation.GetContent("自贡")) }, { "0816", (HTranslation.GetContent("四川省"), HTranslation.GetContent("绵阳")) },
                { "0817", (HTranslation.GetContent("四川省"), HTranslation.GetContent("南充")) }, { "0818", (HTranslation.GetContent("四川省"), HTranslation.GetContent("达州")) }, { "0825", (HTranslation.GetContent("四川省"), HTranslation.GetContent("遂宁")) },
                { "0826", (HTranslation.GetContent("四川省"), HTranslation.GetContent("广安")) }, { "0827", (HTranslation.GetContent("四川省"), HTranslation.GetContent("巴中")) }
            };

        private static readonly Dictionary<string, string> IdCardProvinceMap = new Dictionary<string, string>
        {
            { "11", HTranslation.GetContent("北京市") }, { "12", HTranslation.GetContent("天津市") }, { "13", HTranslation.GetContent("河北省") }, { "14", HTranslation.GetContent("山西省") },
            { "15", HTranslation.GetContent("内蒙古自治区") }, { "21", HTranslation.GetContent("辽宁省") }, { "22", HTranslation.GetContent("吉林省") }, { "23", HTranslation.GetContent("黑龙江省") },
            { "31", HTranslation.GetContent("上海市") }, { "32", HTranslation.GetContent("江苏省") }, { "33", HTranslation.GetContent("浙江省") }, { "34", HTranslation.GetContent("安徽省") },
            { "35", HTranslation.GetContent("福建省") }, { "36", HTranslation.GetContent("江西省") }, { "37", HTranslation.GetContent("山东省") }, { "41", HTranslation.GetContent("河南省") },
            { "42", HTranslation.GetContent("湖北省") }, { "43", HTranslation.GetContent("湖南省") }, { "44", HTranslation.GetContent("广东省") }, { "45", HTranslation.GetContent("广西壮族自治区") },
            { "46", HTranslation.GetContent("海南省") }, { "50", HTranslation.GetContent("重庆市") }, { "51", HTranslation.GetContent("四川省") }, { "52", HTranslation.GetContent("贵州省") },
            { "53", HTranslation.GetContent("云南省") }, { "54", HTranslation.GetContent("西藏自治区") }, { "61", HTranslation.GetContent("陕西省") }, { "62", HTranslation.GetContent("甘肃省") },
            { "63", HTranslation.GetContent("青海省") }, { "64", HTranslation.GetContent("宁夏回族自治区") }, { "65", HTranslation.GetContent("新疆维吾尔自治区") },
            { "71", HTranslation.GetContent("台湾省") }, { "81", HTranslation.GetContent("香港特别行政区") }, { "82", HTranslation.GetContent("澳门特别行政区") },
            { "91", HTranslation.GetContent("国外") }
        };

        private static readonly Dictionary<string, string> InternationalPhoneCodeMap = new Dictionary<string, string>
        {
            { "1", HTranslation.GetContent("美国/加拿大") }, { "7", HTranslation.GetContent("俄罗斯/哈萨克斯坦") }, { "20", HTranslation.GetContent("埃及") }, { "27", HTranslation.GetContent("南非") },
            { "30", HTranslation.GetContent("希腊") }, { "31", HTranslation.GetContent("荷兰") }, { "32", HTranslation.GetContent("比利时") }, { "33", HTranslation.GetContent("法国") }, { "34", HTranslation.GetContent("西班牙") },
            { "36", HTranslation.GetContent("匈牙利") }, { "39", HTranslation.GetContent("意大利") }, { "40", HTranslation.GetContent("罗马尼亚") }, { "41", HTranslation.GetContent("瑞士") }, { "43", HTranslation.GetContent("奥地利") },
            { "44", HTranslation.GetContent("英国") }, { "45", HTranslation.GetContent("丹麦") }, { "46", HTranslation.GetContent("瑞典") }, { "47", HTranslation.GetContent("挪威") }, { "48", HTranslation.GetContent("波兰") },
            { "49", HTranslation.GetContent("德国") }, { "51", HTranslation.GetContent("秘鲁") }, { "52", HTranslation.GetContent("墨西哥") }, { "53", HTranslation.GetContent("古巴") }, { "54", HTranslation.GetContent("阿根廷") },
            { "55", HTranslation.GetContent("巴西") }, { "56", HTranslation.GetContent("智利") }, { "57", HTranslation.GetContent("哥伦比亚") }, { "58", HTranslation.GetContent("委内瑞拉") }, { "60", HTranslation.GetContent("马来西亚") },
            { "61", HTranslation.GetContent("澳大利亚") }, { "62", HTranslation.GetContent("印度尼西亚") }, { "63", HTranslation.GetContent("菲律宾") }, { "64", HTranslation.GetContent("新西兰") }, { "65", HTranslation.GetContent("新加坡") },
            { "66", HTranslation.GetContent("泰国") }, { "81", HTranslation.GetContent("日本") }, { "82", HTranslation.GetContent("韩国") }, { "84", HTranslation.GetContent("越南") }, { "86", HTranslation.GetContent("中国") },
            { "90", HTranslation.GetContent("土耳其") }, { "91", HTranslation.GetContent("印度") }, { "92", HTranslation.GetContent("巴基斯坦") }, { "93", HTranslation.GetContent("阿富汗") }, { "94", HTranslation.GetContent("斯里兰卡") },
            { "95", HTranslation.GetContent("缅甸") }, { "98", HTranslation.GetContent("伊朗") }, { "212", HTranslation.GetContent("摩洛哥") }, { "213", HTranslation.GetContent("阿尔及利亚") }, { "216", HTranslation.GetContent("突尼斯") },
            { "218", HTranslation.GetContent("利比亚") }, { "220", HTranslation.GetContent("冈比亚") }, { "221", HTranslation.GetContent("塞内加尔") }, { "222", HTranslation.GetContent("毛里塔尼亚") }, { "223", HTranslation.GetContent("马里") },
            { "224", HTranslation.GetContent("几内亚") }, { "225", HTranslation.GetContent("科特迪瓦") }, { "226", HTranslation.GetContent("布基纳法索") }, { "227", HTranslation.GetContent("尼日尔") }, { "228", HTranslation.GetContent("多哥") },
            { "229", HTranslation.GetContent("贝宁") }, { "230", HTranslation.GetContent("毛里求斯") }, { "231", HTranslation.GetContent("利比里亚") }, { "232", HTranslation.GetContent("塞拉利昂") }, { "233", HTranslation.GetContent("加纳") },
            { "234", HTranslation.GetContent("尼日利亚") }, { "235", HTranslation.GetContent("乍得") }, { "236", HTranslation.GetContent("中非") }, { "237", HTranslation.GetContent("喀麦隆") }, { "238", HTranslation.GetContent("佛得角") },
            { "239", HTranslation.GetContent("圣多美和普林西比") }, { "240", HTranslation.GetContent("赤道几内亚") }, { "241", HTranslation.GetContent("加蓬") }, { "242", HTranslation.GetContent("刚果（布）") },
            { "243", HTranslation.GetContent("刚果（金）") }, { "244", HTranslation.GetContent("安哥拉") }, { "245", HTranslation.GetContent("几内亚比绍") }, { "246", HTranslation.GetContent("迪戈加西亚") },
            { "247", HTranslation.GetContent("阿森松岛") }, { "248", HTranslation.GetContent("塞舌尔") }, { "249", HTranslation.GetContent("苏丹") }, { "250", HTranslation.GetContent("卢旺达") }, { "251", HTranslation.GetContent("埃塞俄比亚") },
            { "252", HTranslation.GetContent("索马里") }, { "253", HTranslation.GetContent("吉布提") }, { "254", HTranslation.GetContent("肯尼亚") }, { "255", HTranslation.GetContent("坦桑尼亚") }, { "256", HTranslation.GetContent("乌干达") },
            { "257", HTranslation.GetContent("布隆迪") }, { "258", HTranslation.GetContent("莫桑比克") }, { "260", HTranslation.GetContent("赞比亚") }, { "261", HTranslation.GetContent("马达加斯加") }, { "262", HTranslation.GetContent("留尼汪") },
            { "263", HTranslation.GetContent("津巴布韦") }, { "264", HTranslation.GetContent("纳米比亚") }, { "265", HTranslation.GetContent("马拉维") }, { "266", HTranslation.GetContent("莱索托") }, { "267", HTranslation.GetContent("博茨瓦纳") },
            { "268", HTranslation.GetContent("斯威士兰") }, { "269", HTranslation.GetContent("科摩罗") }, { "290", HTranslation.GetContent("圣赫勒拿") }, { "291", HTranslation.GetContent("厄立特里亚") }, { "297", HTranslation.GetContent("阿鲁巴") },
            { "298", HTranslation.GetContent("法罗群岛") }, { "299", HTranslation.GetContent("格陵兰") }, { "350", HTranslation.GetContent("直布罗陀") }, { "351", HTranslation.GetContent("葡萄牙") }, { "352", HTranslation.GetContent("卢森堡") },
            { "353", HTranslation.GetContent("爱尔兰") }, { "354", HTranslation.GetContent("冰岛") }, { "355", HTranslation.GetContent("阿尔巴尼亚") }, { "356", HTranslation.GetContent("马耳他") }, { "357", HTranslation.GetContent("塞浦路斯") },
            { "358", HTranslation.GetContent("芬兰") }, { "359", HTranslation.GetContent("保加利亚") }, { "370", HTranslation.GetContent("立陶宛") }, { "371", HTranslation.GetContent("拉脱维亚") }, { "372", HTranslation.GetContent("爱沙尼亚") },
            { "373", HTranslation.GetContent("摩尔多瓦") }, { "374", HTranslation.GetContent("亚美尼亚") }, { "375", HTranslation.GetContent("白俄罗斯") }, { "376", HTranslation.GetContent("安道尔") }, { "377", HTranslation.GetContent("摩纳哥") },
            { "378", HTranslation.GetContent("圣马力诺") }, { "379", HTranslation.GetContent("梵蒂冈") }, { "380", HTranslation.GetContent("乌克兰") }, { "381", HTranslation.GetContent("塞尔维亚") }, { "382", HTranslation.GetContent("黑山") },
            { "383", HTranslation.GetContent("科索沃") }, { "385", HTranslation.GetContent("克罗地亚") }, { "386", HTranslation.GetContent("斯洛文尼亚") }, { "387", HTranslation.GetContent("波黑") }, { "389", HTranslation.GetContent("北马其顿") },
            { "420", HTranslation.GetContent("捷克") }, { "421", HTranslation.GetContent("斯洛伐克") }, { "423", HTranslation.GetContent("列支敦士登") }, { "500", HTranslation.GetContent("福克兰群岛") }, { "501", HTranslation.GetContent("伯利兹") },
            { "502", HTranslation.GetContent("危地马拉") }, { "503", HTranslation.GetContent("萨尔瓦多") }, { "504", HTranslation.GetContent("洪都拉斯") }, { "505", HTranslation.GetContent("尼加拉瓜") }, { "506", HTranslation.GetContent("哥斯达黎加") },
            { "507", HTranslation.GetContent("巴拿马") }, { "508", HTranslation.GetContent("圣皮埃尔和密克隆") }, { "509", HTranslation.GetContent("海地") }, { "590", HTranslation.GetContent("瓜德罗普") }, { "591", HTranslation.GetContent("玻利维亚") },
            { "592", HTranslation.GetContent("圭亚那") }, { "593", HTranslation.GetContent("厄瓜多尔") }, { "594", HTranslation.GetContent("法属圭亚那") }, { "595", HTranslation.GetContent("巴拉圭") }, { "596", HTranslation.GetContent("马提尼克") },
            { "597", HTranslation.GetContent("苏里南") }, { "598", HTranslation.GetContent("乌拉圭") }, { "599", HTranslation.GetContent("荷属安的列斯") }, { "670", HTranslation.GetContent("东帝汶") }, { "672", HTranslation.GetContent("诺福克岛") },
            { "673", HTranslation.GetContent("文莱") }, { "674", HTranslation.GetContent("瑙鲁") }, { "675", HTranslation.GetContent("巴布亚新几内亚") }, { "676", HTranslation.GetContent("汤加") }, { "677", HTranslation.GetContent("所罗门群岛") },
            { "678", HTranslation.GetContent("瓦努阿图") }, { "679", HTranslation.GetContent("斐济") }, { "680", HTranslation.GetContent("帕劳") }, { "681", HTranslation.GetContent("瓦利斯和富图纳") }, { "682", HTranslation.GetContent("库克群岛") },
            { "683", HTranslation.GetContent("纽埃") }, { "685", HTranslation.GetContent("萨摩亚") }, { "686", HTranslation.GetContent("基里巴斯") }, { "687", HTranslation.GetContent("新喀里多尼亚") }, { "688", HTranslation.GetContent("图瓦卢") },
            { "689", HTranslation.GetContent("法属波利尼西亚") }, { "690", HTranslation.GetContent("托克劳") }, { "691", HTranslation.GetContent("密克罗尼西亚") }, { "692", HTranslation.GetContent("马绍尔群岛") },
            { "850", HTranslation.GetContent("朝鲜") }, { "852", HTranslation.GetContent("香港") }, { "853", HTranslation.GetContent("澳门") }, { "855", HTranslation.GetContent("柬埔寨") }, { "856", HTranslation.GetContent("老挝") },
            { "870", HTranslation.GetContent("国际海事卫星") }, { "878", HTranslation.GetContent("环球个人通信") }, { "880", HTranslation.GetContent("孟加拉国") }, { "886", HTranslation.GetContent("台湾") },
            { "960", HTranslation.GetContent("马尔代夫") }, { "961", HTranslation.GetContent("黎巴嫩") }, { "962", HTranslation.GetContent("约旦") }, { "963", HTranslation.GetContent("叙利亚") }, { "964", HTranslation.GetContent("伊拉克") },
            { "965", HTranslation.GetContent("科威特") }, { "966", HTranslation.GetContent("沙特阿拉伯") }, { "967", HTranslation.GetContent("也门") }, { "968", HTranslation.GetContent("阿曼") }, { "970", HTranslation.GetContent("巴勒斯坦") },
            { "971", HTranslation.GetContent("阿联酋") }, { "972", HTranslation.GetContent("以色列") }, { "973", HTranslation.GetContent("巴林") }, { "974", HTranslation.GetContent("卡塔尔") }, { "975", HTranslation.GetContent("不丹") },
            { "976", HTranslation.GetContent("蒙古") }, { "977", HTranslation.GetContent("尼泊尔") }, { "992", HTranslation.GetContent("塔吉克斯坦") }, { "993", HTranslation.GetContent("土库曼斯坦") }, { "994", HTranslation.GetContent("阿塞拜疆") },
            { "995", HTranslation.GetContent("格鲁吉亚") }, { "996", HTranslation.GetContent("吉尔吉斯斯坦") }, { "998", HTranslation.GetContent("乌兹别克斯坦") }
        };

        private static readonly Dictionary<string, string> LicensePlateProvinceMap = new Dictionary<string, string>
        {
            {HTranslation.GetContent("京"),HTranslation.GetContent("北京市")},{HTranslation.GetContent("津"),HTranslation.GetContent("天津市")},{HTranslation.GetContent("沪"),HTranslation.GetContent("上海市")},{HTranslation.GetContent("渝"),HTranslation.GetContent("重庆市")},
            {HTranslation.GetContent("冀"),HTranslation.GetContent("河北省")},{HTranslation.GetContent("豫"),HTranslation.GetContent("河南省")},{HTranslation.GetContent("云"),HTranslation.GetContent("云南省")},{HTranslation.GetContent("辽"),HTranslation.GetContent("辽宁省")},
            {HTranslation.GetContent("黑"),HTranslation.GetContent("黑龙江省")},{HTranslation.GetContent("湘"),HTranslation.GetContent("湖南省")},{HTranslation.GetContent("皖"),HTranslation.GetContent("安徽省")},{HTranslation.GetContent("鲁"),HTranslation.GetContent("山东省")},
            {HTranslation.GetContent("新"),HTranslation.GetContent("新疆维吾尔自治区")},{HTranslation.GetContent("苏"),HTranslation.GetContent("江苏省")},{HTranslation.GetContent("浙"),HTranslation.GetContent("浙江省")},{HTranslation.GetContent("赣"),HTranslation.GetContent("江西省")},
            {HTranslation.GetContent("鄂"),HTranslation.GetContent("湖北省")},{HTranslation.GetContent("桂"),HTranslation.GetContent("广西壮族自治区")},{HTranslation.GetContent("甘"),HTranslation.GetContent("甘肃省")},{HTranslation.GetContent("晋"),HTranslation.GetContent("山西省")},
            {HTranslation.GetContent("蒙"),HTranslation.GetContent("内蒙古自治区")},{HTranslation.GetContent("陕"),HTranslation.GetContent("陕西省")},{HTranslation.GetContent("吉"),HTranslation.GetContent("吉林省")},{HTranslation.GetContent("闽"),HTranslation.GetContent("福建省")},
            {HTranslation.GetContent("贵"),HTranslation.GetContent("贵州省")},{HTranslation.GetContent("粤"),HTranslation.GetContent("广东省")},{HTranslation.GetContent("川"),HTranslation.GetContent("四川省")},{HTranslation.GetContent("青"),HTranslation.GetContent("青海省")},
            {HTranslation.GetContent("藏"),HTranslation.GetContent("西藏自治区")},{HTranslation.GetContent("琼"),HTranslation.GetContent("海南省")},{HTranslation.GetContent("宁"),HTranslation.GetContent("宁夏回族自治区")}
        };

        #endregion

        #region 邮箱验证

        /// <summary>验证邮箱格式</summary>
        public static bool ValidateEmail(string email) => !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);

        #endregion

        #region 中国手机号与座机号

        /// <summary>验证中国大陆手机号</summary>
        public static bool ValidateChineseMobilePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            phone = phone.Replace(" ", "").Replace("-", "");
            return ChineseMobileRegex.IsMatch(phone);
        }

        /// <summary>验证中国座机号（带区号）</summary>
        public static bool ValidateChineseFixedPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var match = ChineseFixedPhoneRegex.Match(phone);
            if (!match.Success) return false;
            return ChinaAreaCodeMap.ContainsKey(match.Groups[1].Value);
        }

        /// <summary>根据区号获取翻译后的省份/城市</summary>
        public static (string Province, string City) GetChinaAreaCodeInfo(string areaCode)
        {
            if (ChinaAreaCodeMap.TryGetValue(areaCode, out var info))
                return (HTranslation.GetContent(info.Province), HTranslation.GetContent(info.City));
            return (null, null);
        }

        /// <summary>从座机号提取翻译后的省份/城市</summary>
        public static (string Province, string City) ParseFixedPhoneArea(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return (null, null);
            var match = ChineseFixedPhoneRegex.Match(phone);
            if (!match.Success) return (null, null);
            return GetChinaAreaCodeInfo(match.Groups[1].Value);
        }

        #endregion

        #region 身份证

        public class IdCardInfo
        {
            /// <summary>IsValid 成员。</summary>
            public bool IsValid { get; set; }
            /// <summary>ErrorMessage 成员。</summary>
            public string ErrorMessage { get; set; }
            /// <summary>Province 成员。</summary>
            public string Province { get; set; }
            /// <summary>BirthDate 成员。</summary>
            public DateTime? BirthDate { get; set; }
            /// <summary>Gender 成员。</summary>
            public string Gender { get; set; }
        }

        /// <summary>综合验证身份证（15/18位）并提取翻译信息</summary>
        public static IdCardInfo ValidateAndParseIdCard(string id)
        {
            var result = new IdCardInfo();
            if (string.IsNullOrWhiteSpace(id))
            {
                result.ErrorMessage = HTranslation.GetContent("身份证号为空");
                return result;
            }

            id = id.Trim().ToUpper();

            if (id.Length == 15)
            {
                if (!IdCard15Regex.IsMatch(id))
                {
                    result.ErrorMessage = HTranslation.GetContent("15位身份证格式错误");
                    return result;
                }
                id = id.Substring(0, 6) + "19" + id.Substring(6) + " ";
                id = id.Substring(0, 17);
                id += CalculateIdCardCheckDigit(id);
            }
            else if (id.Length == 18)
            {
                if (!IdCard18Regex.IsMatch(id))
                {
                    result.ErrorMessage = HTranslation.GetContent("18位身份证格式错误");
                    return result;
                }
                string expected = CalculateIdCardCheckDigit(id.Substring(0, 17));
                if (id[17] != expected[0])
                {
                    result.ErrorMessage = HTranslation.GetContent("身份证校验位错误");
                    return result;
                }
            }
            else
            {
                result.ErrorMessage = HTranslation.GetContent("身份证长度必须为15位或18位");
                return result;
            }

            string provinceCode = id.Substring(0, 2);
            if (IdCardProvinceMap.TryGetValue(provinceCode, out string provinceChinese))
                result.Province = HTranslation.GetContent(provinceChinese);
            else
            {
                result.ErrorMessage = HTranslation.GetContent("身份证省份代码无效");
                return result;
            }

            string birthStr = id.Substring(6, 8);
            if (DateTime.TryParseExact(birthStr, "yyyyMMdd", null, DateTimeStyles.None, out DateTime birth))
            {
                if (birth > DateTime.Today || birth < new DateTime(1800, 1, 1))
                {
                    result.ErrorMessage = HTranslation.GetContent("出生日期不合理");
                    return result;
                }
                result.BirthDate = birth;
            }
            else
            {
                result.ErrorMessage = HTranslation.GetContent("出生日期格式无效");
                return result;
            }

            int genderDigit = int.Parse(id[16].ToString());
            result.Gender = HTranslation.GetContent(genderDigit % 2 == 1 ? HTranslation.GetContent("男") : HTranslation.GetContent("女"));
            result.IsValid = true;
            return result;
        }

        /// <summary>简单判断身份证是否有效</summary>
        public static bool ValidateIdCard(string id) => ValidateAndParseIdCard(id).IsValid;

        /// <summary>通过身份证获取性别</summary>
        public static string GetGenderFromIdCard(string id)
        {
            var info = ValidateAndParseIdCard(id);
            return info.IsValid ? info.Gender : null;
        }

        /// <summary>CalculateIdCardCheckDigit 方法。</summary>
        private static string CalculateIdCardCheckDigit(string id17)
        {
            int[] weights = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            char[] map = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };
            int sum = 0;
            for (int i = 0; i < 17; i++) sum += (id17[i] - '0') * weights[i];
            return map[sum % 11].ToString();
        }

        /// <summary>获取身份证位数类型（已翻译）</summary>
        public static string GetIdCardType(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return HTranslation.GetContent("未知");
            id = id.Trim();
            if (IdCard15Regex.IsMatch(id)) return HTranslation.GetContent("15位");
            if (IdCard18Regex.IsMatch(id)) return HTranslation.GetContent("18位");
            return HTranslation.GetContent("未知");
        }

        #endregion

        #region 国际电话号码

        /// <summary>验证国际电话号码格式</summary>
        public static bool ValidateInternationalPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            string cleaned = phone.Replace(" ", "").Replace("-", "").Replace(".", "");
            return InternationalPhoneRegex.IsMatch(cleaned);
        }

        /// <summary>根据区号获取国家（已翻译）</summary>
        public static string GetCountryByPhoneCode(string phoneCode)
        {
            if (InternationalPhoneCodeMap.TryGetValue(phoneCode, out string countryChinese))
                return HTranslation.GetContent(countryChinese);
            return null;
        }

        /// <summary>从国际号码提取国家</summary>
        public static string ParseCountryFromInternationalPhone(string phone)
        {
            if (!ValidateInternationalPhone(phone)) return null;
            string cleaned = phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace(".", "");
            for (int len = 3; len >= 1; len--)
            {
                if (cleaned.Length >= len)
                {
                    string code = cleaned.Substring(0, len);
                    if (InternationalPhoneCodeMap.TryGetValue(code, out string countryChinese))
                        return HTranslation.GetContent(countryChinese);
                }
            }
            return null;
        }

        #endregion

        #region 数字/整数/小数

        /// <summary>是否为数字（含小数、负数）</summary>
        public static bool IsNumeric(string input) => !string.IsNullOrWhiteSpace(input) && DecimalRegex.IsMatch(input);
        /// <summary>是否为整数</summary>
        public static bool IsInteger(string input) => !string.IsNullOrWhiteSpace(input) && IntegerRegex.IsMatch(input);
        /// <summary>是否为正整数</summary>
        public static bool IsPositiveInteger(string input) => !string.IsNullOrWhiteSpace(input) && Regex.IsMatch(input, @"^\d+$");
        /// <summary>是否为小数</summary>
        public static bool IsDecimal(string input) => IsNumeric(input);

        #endregion

        #region 字母/字母数字/中文

        /// <summary>是否纯字母</summary>
        public static bool IsLetters(string input) => !string.IsNullOrWhiteSpace(input) && LettersRegex.IsMatch(input);
        /// <summary>是否字母数字</summary>
        public static bool IsAlphaNumeric(string input) => !string.IsNullOrWhiteSpace(input) && AlphaNumericRegex.IsMatch(input);
        /// <summary>是否纯中文</summary>
        public static bool IsChinese(string input) => !string.IsNullOrWhiteSpace(input) && ChineseRegex.IsMatch(input);

        #endregion

        #region IP地址

        /// <summary>验证IPv4</summary>
        public static bool ValidateIPv4(string ip) => !string.IsNullOrWhiteSpace(ip) && IPv4Regex.IsMatch(ip);
        /// <summary>验证IPv6</summary>
        public static bool ValidateIPv6(string ip) => !string.IsNullOrWhiteSpace(ip) && IPv6Regex.IsMatch(ip);
        /// <summary>验证IP（自动）</summary>
        public static bool ValidateIPAddress(string ip) => ValidateIPv4(ip) || ValidateIPv6(ip);

        #endregion

        #region MAC地址

        /// <summary>验证MAC地址</summary>
        public static bool ValidateMacAddress(string mac) => !string.IsNullOrWhiteSpace(mac) && MacAddressRegex.IsMatch(mac);

        #endregion

        #region URL/域名

        /// <summary>验证URL</summary>
        public static bool ValidateUrl(string url) => !string.IsNullOrWhiteSpace(url) && UrlRegex.IsMatch(url);
        /// <summary>验证域名</summary>
        public static bool ValidateDomain(string domain) => !string.IsNullOrWhiteSpace(domain) && domain.Length <= 253 && DomainRegex.IsMatch(domain);

        #endregion

        #region 日期/时间

        /// <summary>验证日期（yyyy-MM-dd等）</summary>
        public static bool IsDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string[] formats = { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd", "yyyy-M-d", "yyyy/M/d" };
            return DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        /// <summary>验证时间格式（HH:mm:ss 或 HH:mm）</summary>
        public static bool ValidateTimeFormat(string time)
        {
            if (string.IsNullOrWhiteSpace(time)) return false;
            return DateTime.TryParseExact(time, new[] { "HH:mm:ss", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        #endregion

        #region 邮政编码/金额

        /// <summary>验证中国邮编</summary>
        public static bool ValidateChinesePostalCode(string code) => !string.IsNullOrWhiteSpace(code) && ChinesePostalCodeRegex.IsMatch(code);
        /// <summary>验证金额格式</summary>
        public static bool ValidateCurrency(string amount) => !string.IsNullOrWhiteSpace(amount) && CurrencyRegex.IsMatch(amount);

        #endregion

        #region GUID/Base64

        /// <summary>验证GUID</summary>
        public static bool IsGuid(string input) => !string.IsNullOrWhiteSpace(input) && GuidRegex.IsMatch(input);
        /// <summary>验证Base64</summary>
        public static bool IsBase64(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim();
            if (input.Length % 4 != 0) return false;
            return Base64Regex.IsMatch(input);
        }

        #endregion

        #region 密码强度

        /// <summary>验证密码强度</summary>
        public static bool ValidatePasswordStrength(string password, int minLength = 8, bool requireUppercase = true,
            bool requireLowercase = true, bool requireDigit = true, bool requireSpecial = true)
        {
            if (string.IsNullOrEmpty(password) || password.Length < minLength) return false;
            if (requireUppercase && !Regex.IsMatch(password, @"[A-Z]")) return false;
            if (requireLowercase && !Regex.IsMatch(password, @"[a-z]")) return false;
            if (requireDigit && !Regex.IsMatch(password, @"[0-9]")) return false;
            if (requireSpecial && !Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]")) return false;
            return true;
        }

        #endregion

        #region 银行卡（Luhn）

        /// <summary>验证银行卡号</summary>
        public static bool ValidateBankCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return false;
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");
            if (!Regex.IsMatch(cardNumber, @"^\d{13,19}$")) return false;
            int sum = 0;
            bool alternate = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = cardNumber[i] - '0';
                if (alternate) { digit *= 2; if (digit > 9) digit -= 9; }
                sum += digit;
                alternate = !alternate;
            }
            return sum % 10 == 0;
        }

        #endregion

        #region 统一社会信用代码

        /// <summary>验证统一社会信用代码</summary>
        public static bool ValidateUnifiedSocialCreditCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 18) return false;
            if (!Regex.IsMatch(code, @"^[0-9A-HJ-NPQRTUWXY]{2}\d{6}[0-9A-HJ-NPQRTUWXY]{10}$")) return false;
            int[] weights = { 1, 3, 9, 27, 19, 26, 16, 17, 20, 29, 25, 13, 8, 24, 10, 30, 28 };
            string checkChars = "0123456789ABCDEFGHJKLMNPQRTUWXY";
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                int index = checkChars.IndexOf(code[i]);
                if (index == -1) return false;
                sum += index * weights[i];
            }
            int mod = sum % 31;
            if (mod == 0) mod = 31;
            return code[17] == checkChars[31 - mod];
        }

        #endregion

        #region 十六进制颜色

        /// <summary>验证十六进制颜色</summary>
        public static bool ValidateHexColor(string color) => !string.IsNullOrWhiteSpace(color) && HexColorRegex.IsMatch(color);

        #endregion

        #region 车牌号

        /// <summary>验证中国车牌</summary>
        public static bool ValidateChineseLicensePlate(string plate) => !string.IsNullOrWhiteSpace(plate) && ChineseLicensePlateRegex.IsMatch(plate);
        /// <summary>从车牌获取省份（已翻译）</summary>
        public static string GetProvinceFromLicensePlate(string plate)
        {
            if (!ValidateChineseLicensePlate(plate)) return null;
            string prefix = plate.Substring(0, 1);
            if (LicensePlateProvinceMap.TryGetValue(prefix, out string provinceChinese))
                return HTranslation.GetContent(provinceChinese);
            return null;
        }

        #endregion

        #region 护照/证件

        /// <summary>验证中国护照</summary>
        public static bool ValidateChinesePassport(string passport) => !string.IsNullOrWhiteSpace(passport) && ChinesePassportRegex.IsMatch(passport);
        /// <summary>验证港澳通行证</summary>
        public static bool ValidateMainlandTravelPermit(string permit) => !string.IsNullOrWhiteSpace(permit) && MainlandTravelPermitRegex.IsMatch(permit);
        /// <summary>验证台胞证</summary>
        public static bool ValidateTaiwanEntryPermit(string permit) => !string.IsNullOrWhiteSpace(permit) && TaiwanEntryPermitRegex.IsMatch(permit);
        /// <summary>验证社保卡号</summary>
        public static bool ValidateSocialSecurityCard(string number) => !string.IsNullOrWhiteSpace(number) && SocialSecurityCardRegex.IsMatch(number);

        #endregion

        #region 发票代码/号码

        /// <summary>验证增值税发票代码</summary>
        public static bool ValidateVatInvoiceCode(string code) => !string.IsNullOrWhiteSpace(code) && VatInvoiceCodeRegex.IsMatch(code);
        /// <summary>验证增值税发票号码</summary>
        public static bool ValidateVatInvoiceNumber(string number) => !string.IsNullOrWhiteSpace(number) && VatInvoiceNumberRegex.IsMatch(number);

        #endregion

        #region ISBN

        /// <summary>验证ISBN-10</summary>
        public static bool ValidateIsbn10(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;
            isbn = isbn.Replace("-", "").Replace(" ", "").ToUpper();
            if (!Regex.IsMatch(isbn, @"^\d{9}[\dX]$")) return false;
            int sum = 0;
            for (int i = 0; i < 9; i++) sum += (isbn[i] - '0') * (10 - i);
            char check = (isbn[9] == 'X') ? 'X' : (char)('0' + ((11 - (sum % 11)) % 11));
            return isbn[9] == check;
        }

        /// <summary>验证ISBN-13</summary>
        public static bool ValidateIsbn13(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;
            isbn = isbn.Replace("-", "").Replace(" ", "");
            if (!Regex.IsMatch(isbn, @"^\d{13}$")) return false;
            int sum = 0;
            for (int i = 0; i < 12; i++) sum += (isbn[i] - '0') * (i % 2 == 0 ? 1 : 3);
            int check = (10 - (sum % 10)) % 10;
            return (isbn[12] - '0') == check;
        }

        /// <summary>验证ISBN（自动识别10/13）</summary>
        public static bool ValidateIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return false;
            string cleaned = isbn.Replace("-", "").Replace(" ", "");
            if (cleaned.Length == 10) return ValidateIsbn10(isbn);
            if (cleaned.Length == 13) return ValidateIsbn13(isbn);
            return false;
        }

        #endregion

        #region QQ/微信

        /// <summary>验证QQ号</summary>
        public static bool ValidateQQ(string qq) => !string.IsNullOrWhiteSpace(qq) && QQRegex.IsMatch(qq);
        /// <summary>验证微信号</summary>
        public static bool ValidateWeChatId(string wechat) => !string.IsNullOrWhiteSpace(wechat) && WeChatIdRegex.IsMatch(wechat);

        #endregion

        #region 经纬度

        /// <summary>验证纬度</summary>
        public static bool ValidateLatitude(string lat)
        {
            if (string.IsNullOrWhiteSpace(lat)) return false;
            if (double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                return val >= -90 && val <= 90;
            return false;
        }
        /// <summary>验证经度</summary>
        public static bool ValidateLongitude(string lon)
        {
            if (string.IsNullOrWhiteSpace(lon)) return false;
            if (double.TryParse(lon, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                return val >= -180 && val <= 180;
            return false;
        }

        #endregion

        #region 中文姓名/图片扩展名/HTML标签/文件路径

        /// <summary>验证中文姓名</summary>
        public static bool ValidateChineseName(string name) => !string.IsNullOrWhiteSpace(name) && ChineseNameRegex.IsMatch(name);
        /// <summary>验证图片扩展名</summary>
        public static bool ValidateImageExtension(string filename) => !string.IsNullOrWhiteSpace(filename) && ImageExtensionRegex.IsMatch(filename);
        /// <summary>是否包含HTML标签</summary>
        public static bool ValidateHtmlTagExists(string text) => !string.IsNullOrWhiteSpace(text) && HtmlTagRegex.IsMatch(text);
        /// <summary>验证文件路径</summary>
        public static bool ValidateFilePath(string path) => !string.IsNullOrWhiteSpace(path) && FilePathRegex.IsMatch(path);

        #endregion

        #region 字符统计

        /// <summary>中文字符个数</summary>
        public static int CountChineseCharacters(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            int count = 0;
            foreach (char c in input) if (ChineseCharRegex.IsMatch(c.ToString())) count++;
            return count;
        }
        /// <summary>英文字母个数</summary>
        public static int CountEnglishLetters(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            int count = 0;
            foreach (char c in input) if (EnglishLetterRegex.IsMatch(c.ToString())) count++;
            return count;
        }
        /// <summary>特殊符号个数</summary>
        public static int CountSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            int count = 0;
            foreach (char c in input)
            {
                if (ChineseCharRegex.IsMatch(c.ToString()) || EnglishLetterRegex.IsMatch(c.ToString()) ||
                    char.IsDigit(c) || char.IsWhiteSpace(c)) continue;
                count++;
            }
            return count;
        }
        /// <summary>是否含中文</summary>
        public static bool HasChinese(string input) => CountChineseCharacters(input) > 0;
        /// <summary>是否含英文</summary>
        public static bool HasEnglish(string input) => CountEnglishLetters(input) > 0;
        /// <summary>是否含特殊符号</summary>
        public static bool HasSpecial(string input) => CountSpecialCharacters(input) > 0;
        /// <summary>字符统计描述（已翻译）</summary>
        public static string GetCharacterStatsDescription(string input)
        {
            int chinese = CountChineseCharacters(input);
            int english = CountEnglishLetters(input);
            int special = CountSpecialCharacters(input);
            return string.Format(HTranslation.GetContent("中文字符：{0}，英文字母：{1}，特殊符号：{2}"), chinese, english, special);
        }

        #endregion

        #region 按位置判断字符类型

        /// <summary>
        /// 获取字符串中指定位置（从0开始）的字符类型描述（已翻译）。
        /// </summary>
        /// <param name="input">待检查的字符串</param>
        /// <param name="index">字符索引（0-based）</param>
        /// <returns>翻译后的类型描述，如“数字”、“字母”、“中文”、“其他”；索引无效则返回 null</returns>
        public static string GetCharTypeAt(string input, int index)
        {
            if (string.IsNullOrEmpty(input) || index < 0 || index >= input.Length)
                return null;

            char c = input[index];
            if (char.IsDigit(c)) return HTranslation.GetContent("数字");
            if (EnglishLetterRegex.IsMatch(c.ToString())) return HTranslation.GetContent("字母");
            if (ChineseCharRegex.IsMatch(c.ToString())) return HTranslation.GetContent("中文");
            return HTranslation.GetContent("其他");
        }

        /// <summary>
        /// 判断指定位置的字符是否为预期类型。
        /// </summary>
        /// <param name="input">字符串</param>
        /// <param name="index">索引（0-based）</param>
        /// <param name="type">预期的字符类型</param>
        /// <returns>是预期类型返回 true，索引无效或类型不匹配返回 false</returns>
        public static bool IsCharTypeAt(string input, int index, HCharType type)
        {
            if (string.IsNullOrEmpty(input) || index < 0 || index >= input.Length)
                return false;

            char c = input[index];
            switch (type)
            {
                case HCharType.Digit: return char.IsDigit(c);
                case HCharType.Letter: return EnglishLetterRegex.IsMatch(c.ToString());
                case HCharType.Chinese: return ChineseCharRegex.IsMatch(c.ToString());
                case HCharType.Other:
                    return !char.IsDigit(c) && !EnglishLetterRegex.IsMatch(c.ToString()) && !ChineseCharRegex.IsMatch(c.ToString());
                default: return false;
            }
        }

        #endregion

        #region 自动识别

        /// <summary>自动判断类型并返回翻译描述</summary>
        public static string AutoIdentify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return HTranslation.GetContent("空字符串");

            if (ValidateEmail(input)) return HTranslation.GetContent("邮箱");
            if (ValidateChineseMobilePhone(input)) return HTranslation.GetContent("中国手机号");
            if (ValidateChineseFixedPhone(input))
            {
                var info = ParseFixedPhoneArea(input);
                return string.Format(HTranslation.GetContent("中国座机号（{0} {1}）"), info.Province, info.City);
            }
            if (ValidateIdCard(input))
            {
                var info = ValidateAndParseIdCard(input);
                return string.Format(HTranslation.GetContent("身份证（{0}，{1:yyyy-MM-dd}，{2}）"),
                                     info.Province, info.BirthDate, info.Gender);
            }
            if (ValidateInternationalPhone(input))
                return string.Format(HTranslation.GetContent("国际电话号码（{0}）"), ParseCountryFromInternationalPhone(input) ?? HTranslation.GetContent("未知国家"));
            if (ValidateChinesePassport(input)) return HTranslation.GetContent("护照号");
            if (ValidateMainlandTravelPermit(input)) return HTranslation.GetContent("港澳通行证");
            if (ValidateTaiwanEntryPermit(input)) return HTranslation.GetContent("台胞证");
            if (ValidateSocialSecurityCard(input)) return HTranslation.GetContent("社保卡号");
            if (ValidateIPv4(input)) return HTranslation.GetContent("IPv4地址");
            if (ValidateIPv6(input)) return HTranslation.GetContent("IPv6地址");
            if (ValidateMacAddress(input)) return HTranslation.GetContent("MAC地址");
            if (ValidateUrl(input)) return HTranslation.GetContent("URL");
            if (ValidateDomain(input)) return HTranslation.GetContent("域名");
            if (ValidateChineseLicensePlate(input)) return HTranslation.GetContent("车牌号");
            if (ValidateUnifiedSocialCreditCode(input)) return HTranslation.GetContent("统一社会信用代码");
            if (ValidateBankCard(input)) return HTranslation.GetContent("银行卡号");
            if (ValidateIsbn(input)) return HTranslation.GetContent("ISBN");
            if (ValidateVatInvoiceCode(input)) return HTranslation.GetContent("增值税发票代码");
            if (ValidateVatInvoiceNumber(input)) return HTranslation.GetContent("增值税发票号码");
            if (ValidateQQ(input)) return HTranslation.GetContent("QQ号");
            if (ValidateWeChatId(input)) return HTranslation.GetContent("微信号");
            if (ValidateLatitude(input)) return HTranslation.GetContent("纬度");
            if (ValidateLongitude(input)) return HTranslation.GetContent("经度");
            if (ValidateChineseName(input)) return HTranslation.GetContent("中文姓名");
            if (IsInteger(input)) return HTranslation.GetContent("整数");
            if (IsNumeric(input)) return HTranslation.GetContent("数字（小数）");
            if (IsLetters(input)) return HTranslation.GetContent("纯字母");
            if (IsAlphaNumeric(input)) return HTranslation.GetContent("字母数字组合");
            if (IsChinese(input)) return HTranslation.GetContent("中文");
            if (IsDate(input)) return HTranslation.GetContent("日期");
            if (ValidateTimeFormat(input)) return HTranslation.GetContent("时间");
            if (ValidateChinesePostalCode(input)) return HTranslation.GetContent("邮政编码");
            if (ValidateCurrency(input)) return HTranslation.GetContent("货币金额");
            if (IsGuid(input)) return HTranslation.GetContent("GUID");
            if (IsBase64(input)) return HTranslation.GetContent("Base64");
            if (ValidateHexColor(input)) return HTranslation.GetContent("十六进制颜色");
            if (ValidateImageExtension(input)) return HTranslation.GetContent("图片扩展名");
            if (ValidateHtmlTagExists(input)) return HTranslation.GetContent("包含HTML标签");
            if (ValidateFilePath(input)) return HTranslation.GetContent("文件路径");

            return HTranslation.GetContent("未识别的格式");
        }

        #endregion

        #region 验证类型列表

        /// <summary>获取所有支持的验证类型名称（已翻译）</summary>
        public static List<string> GetSupportedValidationTypes() => new List<string>
        {
            HTranslation.GetContent("邮箱"), HTranslation.GetContent("中国手机号"), HTranslation.GetContent("中国座机号"), HTranslation.GetContent("身份证"),
            HTranslation.GetContent("国际电话号码"), HTranslation.GetContent("护照号"), HTranslation.GetContent("港澳通行证"), HTranslation.GetContent("台胞证"), HTranslation.GetContent("社保卡号"),
            HTranslation.GetContent("IPv4地址"), HTranslation.GetContent("IPv6地址"), HTranslation.GetContent("MAC地址"), HTranslation.GetContent("URL"), HTranslation.GetContent("域名"),
            HTranslation.GetContent("整数"), HTranslation.GetContent("数字（小数）"), HTranslation.GetContent("纯字母"), HTranslation.GetContent("字母数字组合"), HTranslation.GetContent("中文"),
            HTranslation.GetContent("日期"), HTranslation.GetContent("时间"), HTranslation.GetContent("邮政编码"), HTranslation.GetContent("货币金额"), HTranslation.GetContent("GUID"), HTranslation.GetContent("Base64"),
            HTranslation.GetContent("强密码"), HTranslation.GetContent("银行卡号"), HTranslation.GetContent("统一社会信用代码"), HTranslation.GetContent("十六进制颜色"),
            HTranslation.GetContent("车牌号"), HTranslation.GetContent("ISBN"), HTranslation.GetContent("QQ号"), HTranslation.GetContent("微信号"), HTranslation.GetContent("纬度"), HTranslation.GetContent("经度"),
            HTranslation.GetContent("中文姓名"), HTranslation.GetContent("图片扩展名"), HTranslation.GetContent("包含HTML标签"), HTranslation.GetContent("文件路径"),
            HTranslation.GetContent("增值税发票代码"), HTranslation.GetContent("增值税发票号码")
        };

        #endregion
    }
}
