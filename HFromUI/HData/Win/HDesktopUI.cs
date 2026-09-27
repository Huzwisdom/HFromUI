using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// 桌面环境与用户界面管理工具类（已修复 SendMessage 参数，集成多语言翻译）。
    /// 提供主题切换、声音方案、任务栏设置、开始菜单设置、字体管理、系统级热键注册等功能。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HDesktopUI
    {

        #region 主题切换

        /// <summary>应用指定主题文件（.theme）</summary>
        /// <param name="themeFilePath">主题文件完整路径。</param>
        /// <returns>是否成功启动应用主题进程。</returns>
        public static bool ApplyTheme(string themeFilePath)
        {
            try
            {
                Process.Start(themeFilePath);
                return true;
            }
            catch { return false; }
        }

        /// <summary>获取当前使用的主题名称（显示名）</summary>
        /// <returns>主题显示名称，失败返回“未知”。</returns>
        public static string GetCurrentThemeName()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes"))
                {
                    if (key != null)
                    {
                        string name = key.GetValue("CurrentTheme")?.ToString();
                        if (!string.IsNullOrEmpty(name))
                            return name;
                    }
                }
            }
            catch { }
            return HTranslation.GetContent("未知");
        }

        #endregion

        #region 声音方案

        /// <summary>获取当前声音方案名称</summary>
        /// <returns>方案名称，失败返回“默认”。</returns>
        public static string GetCurrentSoundScheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes"))
                {
                    if (key != null)
                    {
                        string scheme = key.GetValue("")?.ToString();
                        if (!string.IsNullOrEmpty(scheme))
                            return scheme;
                    }
                }
            }
            catch { }
            return HTranslation.GetContent("默认");
        }

        /// <summary>设置声音方案</summary>
        /// <param name="schemeName">方案名称，如 ".Default" 或自定义名称。</param>
        /// <returns>是否成功。</returns>
        public static bool SetSoundScheme(string schemeName)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"AppEvents\Schemes", true))
                {
                    if (key != null)
                    {
                        key.SetValue("", schemeName);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion

        #region 任务栏设置

        /// <summary>获取任务栏是否自动隐藏</summary>
        public static bool GetTaskbarAutoHide()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3"))
                {
                    if (key != null)
                    {
                        byte[] data = key.GetValue("Settings") as byte[];
                        if (data != null && data.Length > 8)
                            return (data[8] & 0x01) != 0;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>设置任务栏自动隐藏（可能需要重启资源管理器生效）</summary>
        public static bool SetTaskbarAutoHide(bool autoHide)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3", true))
                {
                    if (key != null)
                    {
                        byte[] data = key.GetValue("Settings") as byte[];
                        if (data != null && data.Length > 8)
                        {
                            if (autoHide)
                                data[8] |= 0x01;
                            else
                                data[8] &= 0xFE;
                            key.SetValue("Settings", data, RegistryValueKind.Binary);
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>获取任务栏图标是否使用小图标</summary>
        public static bool GetTaskbarSmallIcons()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("TaskbarSmallIcons");
                        if (val != null && Convert.ToInt32(val) == 1)
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>设置任务栏图标大小（true=小图标，false=大图标）</summary>
        public static bool SetTaskbarSmallIcons(bool smallIcons)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                {
                    if (key != null)
                    {
                        key.SetValue("TaskbarSmallIcons", smallIcons ? 1 : 0, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion

        #region 开始菜单设置

        /// <summary>获取是否使用全屏开始菜单（Win10/Win11）</summary>
        public static bool GetFullScreenStartMenu()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("StartMenuFullScreen");
                        if (val != null && Convert.ToInt32(val) == 1)
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>设置全屏开始菜单</summary>
        /// <param name="enable">true 启用全屏，false 关闭。</param>
        /// <returns>是否成功。</returns>
        public static bool SetFullScreenStartMenu(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage", true))
                {
                    if (key != null)
                    {
                        key.SetValue("StartMenuFullScreen", enable ? 1 : 0, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>获取开始菜单是否显示最近打开的项目</summary>
        public static bool GetShowRecentItems()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("Start_TrackDocs");
                        return val != null && Convert.ToInt32(val) == 1;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>设置开始菜单是否显示最近打开的项目</summary>
        public static bool SetShowRecentItems(bool show)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                {
                    if (key != null)
                    {
                        key.SetValue("Start_TrackDocs", show ? 1 : 0, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        #endregion

        #region 字体管理

        /// <summary>获取系统所有已安装字体的名称列表</summary>
        public static List<string> GetInstalledFonts()
        {
            var fonts = new List<string>();
            try
            {
                // 修复：InstalledFontCollection 实现了 IDisposable，使用 using 释放底层 GDI 资源
                using (InstalledFontCollection ifc = new InstalledFontCollection())
                {
                    foreach (System.Drawing.FontFamily family in ifc.Families)
                    {
                        fonts.Add(family.Name);
                    }
                }
            }
            catch { }
            return fonts;
        }

        /// <summary>安装字体（将字体文件添加到系统字体资源）</summary>
        /// <param name="fontFilePath">字体文件路径（.ttf, .otf 等）</param>
        /// <returns>是否成功。</returns>
        public static bool InstallFont(string fontFilePath)
        {
            try
            {
                int result = AddFontResource(fontFilePath);
                if (result > 0)
                {
                    // 通知所有窗口字体已更改
                    SendMessage((IntPtr)HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>卸载字体（从系统中移除字体资源）</summary>
        /// <param name="fontFilePath">字体文件路径。</param>
        /// <returns>是否成功。</returns>
        public static bool UninstallFont(string fontFilePath)
        {
            try
            {
                int result = RemoveFontResource(fontFilePath);
                if (result > 0)
                {
                    SendMessage((IntPtr)HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
                    return true;
                }
            }
            catch { }
            return false;
        }

        #endregion

        #region 系统级热键注册与注销

        /// <summary>注册系统级热键</summary>
        /// <param name="hWnd">接收热键消息的窗口句柄。</param>
        /// <param name="id">热键唯一ID（范围0x0000~0xBFFF）。</param>
        /// <param name="modifiers">组合键修饰符（如 MOD_ALT = 0x0001, MOD_CONTROL = 0x0002）。</param>
        /// <param name="vk">虚拟键码。</param>
        /// <returns>是否注册成功。</returns>
        public static bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint vk)
        {
            try
            {
                return RegisterHotKeyAPI(hWnd, id, modifiers, vk);
            }
            catch { return false; }
        }

        /// <summary>注销系统级热键</summary>
        /// <param name="hWnd">窗口句柄。</param>
        /// <param name="id">热键ID。</param>
        /// <returns>是否成功。</returns>
        public static bool UnregisterHotKey(IntPtr hWnd, int id)
        {
            try
            {
                return UnregisterHotKeyAPI(hWnd, id);
            }
            catch { return false; }
        }

        #endregion

        #region Win32 API 与常量

        private const int HWND_BROADCAST = 0xffff;
        private const uint WM_FONTCHANGE = 0x001D;

        // 热键修饰符常量
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKeyAPI(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKeyAPI(IntPtr hWnd, int id);

        [DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);

        [DllImport("gdi32.dll")]
        private static extern int RemoveFontResource(string lpFileName);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}
