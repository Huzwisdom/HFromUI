using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Speech.Synthesis; // 需要引用 System.Speech.dll
using Microsoft.Win32;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 输入法与区域管理工具类（已修复 LoadKeyboardLayout 名称冲突，集成多语言翻译）。
    /// 提供输入法列表、切换默认输入法、添加/删除输入语言、键盘布局加载/卸载、语音合成（TTS）等功能。
    /// 输入法部分基于 Win32 API（无需 System.Windows.Forms）；语音合成需要引用 System.Speech.dll。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HInputRegion
    {

        #region 信息类定义

        /// <summary>输入语言（键盘布局）信息。</summary>
        public class InputLanguageInfo
        {
            /// <summary>键盘布局句柄（HKL）</summary>
            public IntPtr Handle { get; set; }
            /// <summary>语言名称（如“中文(简体，中国)”）</summary>
            public string LayoutName { get; set; }
            /// <summary>布局 ID（如“00000804”）</summary>
            public string LayoutId { get; set; }
            /// <summary>区域名称（如“zh-CN”）</summary>
            public string CultureName { get; set; }
        }

        /// <summary>语音合成引擎信息。</summary>
        public class VoiceInfo
        {
            /// <summary>语音名称</summary>
            public string Name { get; set; }
            /// <summary>语音描述</summary>
            public string Description { get; set; }
            /// <summary>性别（男/女/中性）</summary>
            public string Gender { get; set; }
        }

        #endregion

        #region 输入法列表与切换

        /// <summary>获取系统已安装的所有输入语言（键盘布局）列表。</summary>
        /// <returns>输入语言信息列表。</returns>
        public static List<InputLanguageInfo> GetInstalledInputLanguages()
        {
            var list = new List<InputLanguageInfo>();
            try
            {
                int count = GetKeyboardLayoutList(0, null);
                if (count == 0) return list;
                IntPtr[] hkls = new IntPtr[count];
                GetKeyboardLayoutList(count, hkls);

                foreach (IntPtr hkl in hkls)
                {
                    var info = new InputLanguageInfo
                    {
                        Handle = hkl,
                        LayoutId = hkl.ToString("X8"),
                        CultureName = GetCultureNameFromHKL(hkl),
                        LayoutName = GetLayoutNameFromHKL(hkl)
                    };
                    list.Add(info);
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取当前线程的活动输入语言。</summary>
        /// <returns>当前输入语言信息，失败返回 null。</returns>
        public static InputLanguageInfo GetCurrentInputLanguage()
        {
            try
            {
                IntPtr hkl = GetKeyboardLayout(0);
                if (hkl == IntPtr.Zero) return null;
                return new InputLanguageInfo
                {
                    Handle = hkl,
                    LayoutId = hkl.ToString("X8"),
                    CultureName = GetCultureNameFromHKL(hkl),
                    LayoutName = GetLayoutNameFromHKL(hkl)
                };
            }
            catch { return null; }
        }

        /// <summary>切换到指定的输入语言。</summary>
        /// <param name="cultureName">区域名称（如 "zh-CN"）或布局 ID（如 "00000804"）。</param>
        /// <returns>是否成功。</returns>
        public static bool SetCurrentInputLanguage(string cultureName)
        {
            try
            {
                // 首先根据 cultureName 或 layoutId 查找对应的 HKL
                IntPtr targetHkl = IntPtr.Zero;
                var languages = GetInstalledInputLanguages();
                foreach (var lang in languages)
                {
                    if (lang.CultureName.Equals(cultureName, StringComparison.OrdinalIgnoreCase) ||
                        lang.LayoutId.Equals(cultureName, StringComparison.OrdinalIgnoreCase))
                    {
                        targetHkl = lang.Handle;
                        break;
                    }
                }
                if (targetHkl == IntPtr.Zero)
                {
                    // 尝试加载该键盘布局（调用正确的 Win32 API 函数）
                    targetHkl = LoadKeyboardLayoutAPI(cultureName, KLF_ACTIVATE);
                    if (targetHkl == IntPtr.Zero) return false;
                }
                // 激活布局
                IntPtr prev = ActivateKeyboardLayout(targetHkl, KLF_SETFORPROCESS);
                return prev != IntPtr.Zero;
            }
            catch { return false; }
        }

        #endregion

        #region 添加/删除输入语言（通过注册表）

        /// <summary>添加输入语言（通过修改注册表，需要注销重新登录生效）。</summary>
        /// <param name="layoutId">键盘布局 ID（如 "00000804" 表示简体中文美式键盘）。</param>
        /// <returns>是否成功写入注册表。</returns>
        public static bool AddInputLanguage(string layoutId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(layoutId)) return false;
                layoutId = layoutId.Trim();
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Keyboard Layout\Preload", true))
                {
                    if (key == null) return false;
                    // 已存在则不重复添加（原来会重复写入，导致同一输入法出现多份）
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (string.Equals(key.GetValue(valueName)?.ToString(), layoutId, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    // 获取当前已有的最大编号
                    int maxNum = 0;
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (int.TryParse(valueName, out int num) && num > maxNum)
                            maxNum = num;
                    }
                    key.SetValue((maxNum + 1).ToString(), layoutId);
                }
                // 注意：原实现在 Substitutes 下无条件写入 "00000409"（美式英文）替代项，
                // 这会把任意添加的键盘布局强制替换为 en-US，属于错误副作用，已移除。
                // Substitutes 仅在需要把某 KLID 重定向到 IME/TIP 时才应写入，不应无条件添加。
                return true;
            }
            catch { return false; }
        }

        /// <summary>移除输入语言（通过注册表，需要注销重新登录生效）。</summary>
        /// <param name="layoutId">要移除的键盘布局 ID。</param>
        /// <returns>是否成功。</returns>
        public static bool RemoveInputLanguage(string layoutId)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Keyboard Layout\Preload", true))
                {
                    if (key == null) return false;
                    // 查找并删除对应值（GetValue 对异常注册表值可能返回 null，需空安全）
                    string valueToDelete = null;
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (string.Equals(key.GetValue(valueName)?.ToString(), layoutId, StringComparison.OrdinalIgnoreCase))
                        {
                            valueToDelete = valueName;
                            break;
                        }
                    }
                    if (valueToDelete != null)
                    {
                        key.DeleteValue(valueToDelete);
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }

        #endregion

        #region 键盘布局加载/卸载

        /// <summary>加载键盘布局（DLL 文件）</summary>
        /// <param name="dllPath">键盘布局 DLL 文件名（如 "kbdus.dll"）或布局 ID。</param>
        /// <returns>成功返回布局句柄，失败返回 IntPtr.Zero。</returns>
        public static IntPtr LoadKeyboardLayout(string dllPath)
        {
            try
            {
                return LoadKeyboardLayoutAPI(dllPath, KLF_ACTIVATE);
            }
            catch { return IntPtr.Zero; }
        }

        /// <summary>卸载键盘布局</summary>
        /// <param name="hkl">布局句柄</param>
        /// <returns>是否成功。</returns>
        public static bool UnloadKeyboardLayout(IntPtr hkl)
        {
            try
            {
                return UnloadKeyboardLayoutAPI(hkl);
            }
            catch { return false; }
        }

        #endregion

        #region 语音合成（TTS）

        // 共享的朗读器：原来每次 Speak 都 new 一个且不保存引用，
        // ① StopSpeaking 对一个全新对象取消，根本停不掉正在朗读的旧对象；
        // ② 朗读器对象随用随丢、无法释放。改为进程内共享单例（静态辅助类生命周期=进程生命周期）。
        private static SpeechSynthesizer _synth;
        /// <summary>_synthLock 字段。</summary>
        private static readonly object _synthLock = new object();

        /// <summary>获取 synth。</summary>
        private static SpeechSynthesizer GetSynth()
        {
            if (_synth == null)
            {
                _synth = new SpeechSynthesizer();
            }
            return _synth;
        }

        /// <summary>获取系统已安装的语音合成引擎列表。</summary>
        /// <returns>语音信息列表。</returns>
        public static List<VoiceInfo> GetInstalledVoices()
        {
            var voices = new List<VoiceInfo>();
            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    foreach (InstalledVoice voice in synth.GetInstalledVoices())
                    {
                        var info = new VoiceInfo
                        {
                            Name = voice.VoiceInfo.Name,
                            Description = voice.VoiceInfo.Description,
                            Gender = voice.VoiceInfo.Gender.ToString()
                        };
                        // 翻译性别
                        info.Gender = info.Gender == "Male" ? HTranslation.GetContent("男") : (info.Gender == "Female" ? HTranslation.GetContent("女") : HTranslation.GetContent("中性"));
                        voices.Add(info);
                    }
                }
            }
            catch { }
            return voices;
        }

        /// <summary>使用系统默认语音朗读文本（异步）。</summary>
        /// <param name="text">要朗读的文本。</param>
        /// <returns>是否成功开始朗读。</returns>
        public static bool SpeakText(string text)
        {
            try
            {
                lock (_synthLock)
                {
                    SpeechSynthesizer synth = GetSynth();
                    synth.SpeakAsyncCancelAll();
                    synth.SpeakAsync(text);
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>停止当前朗读。</summary>
        /// <returns>是否成功。</returns>
        public static bool StopSpeaking()
        {
            try
            {
                lock (_synthLock)
                {
                    // 必须对正在朗读的同一个对象取消，原来 new 新对象取消完全无效
                    GetSynth().SpeakAsyncCancelAll();
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>使用指定语音朗读文本。</summary>
        /// <param name="text">文本。</param>
        /// <param name="voiceName">语音名称（如 "Microsoft Huihui Desktop"）。</param>
        /// <returns>是否成功。</returns>
        public static bool SpeakTextWithVoice(string text, string voiceName)
        {
            try
            {
                lock (_synthLock)
                {
                    SpeechSynthesizer synth = GetSynth();
                    if (!string.IsNullOrEmpty(voiceName))
                    {
                        // 无效语音名会抛异常；先校验，找不到就用默认语音继续朗读
                        bool found = false;
                        foreach (InstalledVoice v in synth.GetInstalledVoices())
                        {
                            if (v.Enabled && v.VoiceInfo.Name.Equals(voiceName, StringComparison.OrdinalIgnoreCase))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found) synth.SelectVoice(voiceName);
                    }
                    synth.SpeakAsyncCancelAll();
                    synth.SpeakAsync(text);
                }
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Win32 API 与辅助方法

        private const uint KLF_ACTIVATE = 0x00000001;
        private const uint KLF_SETFORPROCESS = 0x00000100;

        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint Flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadKeyboardLayoutAPI(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern bool UnloadKeyboardLayoutAPI(IntPtr hkl);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetLocaleInfo(int Locale, int LCType, StringBuilder lpLCData, int cchData);

        [DllImport("kernel32.dll")]
        private static extern int LCIDToLocaleName(int Locale, StringBuilder lpName, int cchName, int dwFlags);

        private const int LOCALE_SLANGUAGE = 0x00000004;
        private const int LOCALE_SNAME = 0x0000005C;

        /// <summary>从 HKL 中提取区域名称（如 "zh-CN"）</summary>
        private static string GetCultureNameFromHKL(IntPtr hkl)
        {
            try
            {
                int lcid = (int)((uint)hkl & 0xFFFF);
                StringBuilder sb = new StringBuilder(256);
                if (LCIDToLocaleName(lcid, sb, sb.Capacity, 0) > 0)
                    return sb.ToString();
            }
            catch { }
            return "";
        }

        /// <summary>从 HKL 中提取语言显示名称（如“中文(简体，中国)”）</summary>
        private static string GetLayoutNameFromHKL(IntPtr hkl)
        {
            try
            {
                int lcid = (int)((uint)hkl & 0xFFFF);
                StringBuilder sb = new StringBuilder(256);
                if (GetLocaleInfo(lcid, LOCALE_SLANGUAGE, sb, sb.Capacity) > 0)
                    return sb.ToString();
            }
            catch { }
            return hkl.ToString();
        }

        #endregion
    }
}
