using HFromUI.HEnum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HFile;

namespace HFromUI.HLangage
{
    public class HLanguage
    {
        /// <summary>DirectoryName 成员。</summary>
        public string DirectoryName { set; get; }
        public HLanguage(HLanguageMode originalLanguageMode, string directoryName = "")
        {
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                DirectoryName = "HLanguage" + Application.CompanyName.Replace(".", "").Replace(":", "").Replace("\\", "");
            }
            else
            {
                DirectoryName = directoryName;
            }
            TranslationLanguageMode = originalLanguageMode;
            string path = HFromUI.HData.HAppData.AppDataPath + $"{DirectoryName}\\Language{TranslationLanguageMode.ToString().ToUpper()}.language";
            LanguageIniFile = new HDictionaryFile(path);
        }
        public HLanguage(string directoryName = "")
        {
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                DirectoryName = "Language" + Application.CompanyName.Replace(".", "").Replace(":", "").Replace("\\", "");
            }
            else
            {
                DirectoryName = directoryName;
            }
            if (LanguageIniFileMode == null)
            {
                string modepath = HFromUI.HData.HAppData.AppDataPath + $"{DirectoryName}\\LanguageMode.language";
                LanguageIniFileMode = new HDictionaryFile(modepath);
            }
            HLanguageMode languageMode = HLanguageMode.not;
            IsOnline = LanguageIniFileMode.Read("IsOnlineLanguageMode", false);
            IsConversion = LanguageIniFileMode.Read("IsConversion", true);
            baiduAppId = LanguageIniFileMode.Read("BaiduAppId", "20240522002058264");
            baiduSecretKey = LanguageIniFileMode.Read("BaiduSecretKey", "8WlWH2V0OvgQQNHMbMOU");
            string languageStr = LanguageIniFileMode.Read("TranslationLanguageMode", "en");
            Encoding LanguageIniFileFileEncoding = LanguageIniFileMode.Read("LanguageEncoding", Encoding.UTF8);
            if (!Enum.TryParse(languageStr, out languageMode))
            {
                try
                {
                    languageMode = (HLanguageMode)Convert.ToInt32(languageStr);
                }
                catch
                {
                    languageMode = HLanguageMode.not;
                }
            }
            TranslationLanguageMode = languageMode;

            languageStr = LanguageIniFileMode.Read("OriginalLanguageMode", "auto");
            if (!Enum.TryParse(languageStr, out languageMode))
            {
                try
                {
                    languageMode = (HLanguageMode)Convert.ToInt32(languageStr);
                }
                catch
                {
                    languageMode = HLanguageMode.not;
                }
            }
            OriginalLanguageMode = languageMode;
            string path = HFromUI.HData.HAppData.AppDataPath + $"{DirectoryName}\\Language{TranslationLanguageMode.ToString().ToUpper()}.language";
            LanguageIniFile = new HDictionaryFile(path);
            LanguageIniFile.FileEncoding = LanguageIniFileFileEncoding;
            string LanguageModeIllustrate = "Instructions>>";
            foreach (HLanguageMode languageModeStr in Enum.GetValues(typeof(HLanguageMode)))
            {
                LanguageModeIllustrate += $"Name*{languageModeStr.ToString()}  Value*{(int)languageModeStr}||";
            }
            LanguageIniFileMode.Write("LanguageModeIllustrate", LanguageModeIllustrate.Trim('|').Trim(),true);
            LanguageIniFileMode.Save();
        }
        /// <summary>
        /// 是否在线翻译
        /// </summary>
        public bool IsOnline { set; get; } = false;

        public delegate bool TranslationDelegate(string s, ref string result);

        public TranslationDelegate OnlineTranslation;
        /// <summary>
        /// 是否解析
        /// </summary>
        public bool IsConversion { set; get; } = true;
        /// <summary>
        /// 所有翻译后的INI文件
        /// </summary>
        private HDictionaryFile LanguageIniFile;
        /// <summary>保存。</summary>
        public void Save()
        {
            if (LanguageIniFile!=null)
            {
                LanguageIniFile.Save();
            }
        }
        /// <summary>
        /// 设置的翻译文档INI文件
        /// </summary>
        private HDictionaryFile LanguageIniFileMode;
        /// <summary>mLock 字段。</summary>
        private object mLock = new object();
        /// <summary>
        /// 本机语言
        /// </summary>
        public HLanguageMode OriginalLanguageMode = HLanguageMode.zh;
        /// <summary>
        /// 翻译语言
        /// </summary>
        public HLanguageMode TranslationLanguageMode = HLanguageMode.en;
        /// <summary>baiduAppId 字段。</summary>
        private string baiduAppId = "";
        /// <summary>baiduSecretKey 字段。</summary>
        private string baiduSecretKey = "";

        public bool IsNotTranslation
        {
            get
            {
                if (TranslationLanguageMode == HLanguageMode.not)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>获取 content。</summary>
        public string GetContent(string original)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                return "";
            }
            string translationString = "";
            if (IsOnline && OriginalLanguageMode != TranslationLanguageMode && TranslationLanguageMode != HLanguageMode.not && TranslationLanguageMode != HLanguageMode.auto)
            {
                string buffer = LanguageIniFile.Read(original, IsOnline ? translationString : original);
                if (string.IsNullOrWhiteSpace(buffer) || buffer.Trim() == original.Trim())
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (OnlineTranslation==null)
                        {
                            OnlineTranslation = OnlineBaiduTranslation;
                        }
                        if (OnlineTranslation(original, ref translationString))
                        {
                            SetContent(original, translationString);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (IsOnline && OriginalLanguageMode == TranslationLanguageMode)
                {
                    SetContent(original, original);
                }
            }
            if (TranslationLanguageMode == HLanguageMode.auto || TranslationLanguageMode == HLanguageMode.not || OriginalLanguageMode == TranslationLanguageMode)
            {
                return original;
            }
            else
            {
                translationString = LanguageIniFile.Read(original, IsOnline ? translationString : original);
            }
            return UnicodeToLanguage(translationString);
        }
        /// <summary>获取 content。</summary>
        public string GetContent(string original, params string[] param)
        {
            if (string.IsNullOrWhiteSpace(original))
            {
                return "";
            }
            string translationString = "";
            if (IsOnline && OriginalLanguageMode != TranslationLanguageMode && TranslationLanguageMode != HLanguageMode.not && TranslationLanguageMode != HLanguageMode.auto)
            {
                string buffer = LanguageIniFile.Read(original, IsOnline ? translationString : original);
                if (string.IsNullOrWhiteSpace(buffer) || buffer.Trim() == original.Trim())
                {
                    for (int i = 0; i < 20; i++)
                    {
                        if (OnlineTranslation == null)
                        {
                            OnlineTranslation = OnlineBaiduTranslation;
                        }
                        if (OnlineTranslation(original, ref translationString))
                        {
                            SetContent(original, translationString);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (IsOnline && OriginalLanguageMode == TranslationLanguageMode)
                {
                    SetContent(original, original);
                }
            }
            if (TranslationLanguageMode == HLanguageMode.auto || TranslationLanguageMode == HLanguageMode.not || OriginalLanguageMode == TranslationLanguageMode)
            {
                return original;
            }
            else
            {
                try
                {
                    translationString = string.Format(LanguageIniFile.Read(original, IsOnline ? translationString : original), param);
                }
                catch
                {
                    translationString = LanguageIniFile.Read(original, IsOnline ? translationString : original);
                }

            }
            return UnicodeToLanguage(translationString);
        }
        /// <summary>设置 content。</summary>
        public void SetContent(string original, string translation, bool isConversion = false)
        {
            if (isConversion)
            {
                LanguageIniFile.Write(original, LanguageToUnicode(translation));
            }
            else
            {
                LanguageIniFile.Write(original, translation);
            }
        }
        /// <summary>设置 translationLanguageMode。</summary>
        public void SetTranslationLanguageMode(HLanguageMode languageMode)
        {
            LanguageIniFileMode.Write("TranslationLanguageMode", languageMode.ToString());
        }
        /// <summary>设置 originalLanguageMode。</summary>
        public void SetOriginalLanguageMode(HLanguageMode languageMode)
        {
            LanguageIniFileMode.Write("OriginalLanguageMode", languageMode.ToString());
        }
        /// <summary>LanguageModeIllustrate 方法。</summary>
        public string LanguageModeIllustrate()
        {
            if (LanguageIniFileMode == null)
            {
                string modepath = HFromUI.HData.HAppData.AppDataPath + $"{DirectoryName}\\LanguageMode.language";
                LanguageIniFileMode = new HDictionaryFile(modepath);
            }
            string LanguageModeIllustrate = "Instructions>>";
            foreach (HLanguageMode languageModeStr in Enum.GetValues(typeof(HLanguageMode)))
            {
                LanguageModeIllustrate += $"<Name*{languageModeStr.GetRemark()}><{languageModeStr.ToString()}><Value*{(int)languageModeStr}>||";
            }
            LanguageModeIllustrate = LanguageModeIllustrate.Trim('|').Trim();
            LanguageIniFileMode.Write("LanguageModeIllustrate", LanguageModeIllustrate);
            return LanguageModeIllustrate;
        }
        /// <summary>EncryptString 方法。</summary>
        public string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
        /// <summary>LanguageToUnicode 方法。</summary>
        public virtual string LanguageToUnicode(string str)
        {
            if (!IsConversion)
            {
                return str;
            }
            string outStr = "";
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if (Regex.IsMatch(str[i].ToString(), @"[\u4e00-\u9fa5]")) { outStr += "\\u" + ((int)str[i]).ToString("x"); }
                    else { outStr += str[i]; }
                }
            }
            return outStr;

        }
        /// <summary>UnicodeToLanguage 方法。</summary>
        public virtual string UnicodeToLanguage(string str)
        {
            if (!IsConversion)
            {
                return str;
            }
            string outStr = "";
            Regex reg = new Regex(@"(?i)\\u([0-9a-f]{4})");
            outStr = reg.Replace(str, delegate (Match m1)
            {
                return ((char)Convert.ToInt32(m1.Groups[1].Value, 16)).ToString();
            });
            return outStr;
        }
        /// <summary>获取 languageKeys。</summary>
        public string[] GetLanguageKeys()
        {
            try
            {
                List<string> keys = new List<string>();
                foreach (var item in LanguageIniFile.DictionaryList.Keys)
                {
                    keys.Add(item);
                }
                return keys.ToArray();
            }
            catch { return null; }

        }
        /// <summary>响应 OnlineBaiduTranslation 事件。</summary>
        public virtual  bool OnlineBaiduTranslation(string original, ref string Translation)
        {

            try
            {
                lock (mLock)
                {
                    // 源语言
                    string from = OriginalLanguageMode.ToString();
                    if (OriginalLanguageMode == HLanguageMode.not)
                    {
                        from = "auto";
                    }
                    // 目标语言
                    string to = TranslationLanguageMode.ToString();
                    Random rd = new Random();
                    string salt = rd.Next(100000).ToString();
                    string sign = EncryptString(baiduAppId + original + salt + baiduSecretKey);
                    string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                    url += "q=" + original;
                    url += "&from=" + from;
                    url += "&to=" + to;
                    url += "&appid=" + baiduAppId;
                    url += "&salt=" + salt;
                    url += "&sign=" + sign;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "text/html;charset=UTF-8";
                    request.UserAgent = null;
                    request.Timeout = 6000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    string retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                    if (retString.Contains("dst"))
                    {
                        retString = retString.Substring(retString.IndexOf("dst") + 6, retString.Length - 10 - retString.IndexOf("dst"));
                        Translation = retString;
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }
    }
}
