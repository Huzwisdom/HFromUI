using HFromUI.HAttribute;
using HFromUI.HEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static HFromUI.HLangage.HLanguage;

namespace HFromUI.HLangage
{
    public static class HTranslation
    {
        private static Dictionary<string, string> dictionaryTranslation = new Dictionary<string, string>();
        /// <summary>language 字段。</summary>
        private static HLanguage language = new HLanguage();
        /// <summary>LockGet 字段。</summary>
        private static readonly object LockGet = new object();
        /// <summary>IsNotTranslation 字段。</summary>
        private static bool IsNotTranslation = true;
        /// <summary>保存。</summary>
        public static void Save()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            language.Save();
        }
        /// <summary>设置 onlineTranslation。</summary>
        public static void SetOnlineTranslation(TranslationDelegate translation)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            language.OnlineTranslation= translation;
        }
        /// <summary>IsDictionaryFind 成员。</summary>
        public static bool IsDictionaryFind { set; get; } = true;
        /// <summary>
        /// 获取目标语言（翻译）
        /// </summary>
        /// <param name="original">输入语言</param>
        /// <returns>返回目标语言</returns>
        public static string GetContent(string original)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            { 
            return original;
            }
                if (IsNotTranslation && IsDictionaryFind)
            {
                IsDictionaryFind = !language.IsNotTranslation;
                IsNotTranslation = false;
            }
            string GetContentStringBuffer = string.Empty;
            if (IsDictionaryFind)
            {
                lock (LockGet)
                {
                    if (dictionaryTranslation.ContainsKey(original))
                    {
                        GetContentStringBuffer = dictionaryTranslation[original];
                    }
                    else
                    {
                        GetContentStringBuffer = language.GetContent(original);
                        dictionaryTranslation.Add(original, GetContentStringBuffer);
                    }
                }
            }
            else
            {
                if (language.IsNotTranslation)
                {
                    GetContentStringBuffer = original;
                }
                else
                {
                    GetContentStringBuffer = language.GetContent(original);
                }
            }
            return GetContentStringBuffer;
        }
        /// <summary>
        ///  获取目标语言（翻译）
        /// </summary>
        /// <param name="original">输入语言</param>
        /// <param name="param">正式表达式里内容</param>
        /// <returns>返回目标语言</returns>
        public static string GetContent(string original, params string[] param)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return original;
            }
            if (IsNotTranslation && IsDictionaryFind)
            {
                IsDictionaryFind = !language.IsNotTranslation;
                IsNotTranslation = false;
            }
            string GetContentStringBuffer = string.Empty;
            if (IsDictionaryFind)
            {
                lock (LockGet)
                {
                    if (dictionaryTranslation.ContainsKey(original))
                    {
                        GetContentStringBuffer = dictionaryTranslation[original];
                    }
                    else
                    {
                        GetContentStringBuffer = language.GetContent(original);
                        dictionaryTranslation.Add(original, GetContentStringBuffer);
                    }
                }
            }
            else
            {
                if (language.IsNotTranslation)
                {
                    GetContentStringBuffer = original;
                }
                else
                {
                    GetContentStringBuffer = language.GetContent(original);
                }
            }
            try
            {
                return string.Format(GetContentStringBuffer, param);
            }
            catch { return language.GetContent(original, param); }
        }

        public static event EventHandler TranslationLanguageChanged;

        /// <summary>响应 OnTranslationLanguageChanged 事件。</summary>
        public static void OnTranslationLanguageChanged()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            TranslationLanguageChanged?.Invoke(null, EventArgs.Empty);
        }
        /// <summary>
        /// 设置翻译的语言类型
        /// </summary>
        /// <param name="languageMode">设置的语言种类</param>
        public static void SetTranslationLanguageMode(HLanguageMode languageMode)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            language.SetTranslationLanguageMode(languageMode);
            OnTranslationLanguageChanged();
        }
        /// <summary>
        /// 设置原本的语言类型
        /// </summary>
        /// <param name="languageMode">设置的语言种类</param>
        public static void SetOriginalLanguageMode(HLanguageMode languageMode)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            language.SetOriginalLanguageMode(languageMode);
            OnTranslationLanguageChanged();
        }
        /// <summary>
        /// 设置语言说明书
        /// </summary>
        /// <returns>语言说明书</returns>
        public static string LanguageModeIllustrate()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return string.Empty;
            }
            return language.LanguageModeIllustrate();
        }
        /// <summary>
        /// 设置翻译的语言
        /// </summary>
        /// <param name="original">原本内容</param>
        /// <param name="translation">翻译内容</param>
        /// <param name="isConversion">是否解析</param>
        public static void SetContent(string original, string translation, bool isConversion = true)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return ;
            }
            language.SetContent(original, translation, isConversion);
        }
        /// <summary>
        /// 获取枚举的备注信息
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public static string GetRemark(this Enum value)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return string.Empty;
            }
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi == null)
            {
                return value.ToString();
            }
            object[] attributes = fi.GetCustomAttributes(typeof(HRemarkLanguageAttribute), false);
            if (attributes.Length > 0)
            {
                return language.GetContent(((HRemarkLanguageAttribute)attributes[0]).Remark);
            }
            else
            {
                return value.ToString();
            }
        }
        /// <summary>
        /// 获取所有的原版语言内容
        /// </summary>
        /// <returns></returns>
        public static string[] GetLanguageKeys()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return null;
            }
            if (IsDictionaryFind)
            {
                List<string> GetLanguageKeysBuffer = new List<string>();
                foreach (var item in dictionaryTranslation.Keys)
                {
                    GetLanguageKeysBuffer.Add(item);
                }
                return GetLanguageKeysBuffer.ToArray();
            }
            return language.GetLanguageKeys();
        }
    }
}
