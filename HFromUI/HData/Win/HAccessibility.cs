using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using HFromUI; // HTranslation

namespace HFromUI.HData.Win
{
    using HFromUI.HLangage;
    /// <summary>
    /// Windows 辅助功能管理工具类（终极全功能版，集成多语言翻译）。
    /// 提供屏幕键盘、放大镜、讲述人、高对比度、截屏、粘滞键、筛选键、切换键、鼠标键、
    /// 文本光标指示器等辅助功能的启用/禁用与状态查询。
    /// 底层基于 Win32 API 和系统进程调用，异常安全。
    /// 所有面向用户的描述性文本均通过 <see cref="HTranslation.GetContent"/> 翻译。
    /// 兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public static class HAccessibility
    {

        #region 屏幕键盘 (On‑Screen Keyboard)

        /// <summary>启动屏幕键盘（osk.exe）。</summary>
        public static void OpenOnScreenKeyboard()
        {
            try { Process.Start("osk.exe"); } catch { }
        }

        /// <summary>关闭屏幕键盘。</summary>
        public static void CloseOnScreenKeyboard()
        {
            KillProcessByName("osk");
        }

        #endregion

        #region 放大镜 (Magnifier)

        /// <summary>启动放大镜（magnify.exe）。</summary>
        public static void OpenMagnifier()
        {
            try { Process.Start("magnify.exe"); } catch { }
        }

        /// <summary>关闭放大镜。</summary>
        public static void CloseMagnifier()
        {
            KillProcessByName("magnify");
        }

        #endregion

        #region 讲述人 (Narrator)

        /// <summary>启动讲述人（Narrator.exe）。</summary>
        public static void OpenNarrator()
        {
            try { Process.Start("Narrator.exe"); } catch { }
        }

        /// <summary>关闭讲述人。</summary>
        public static void CloseNarrator()
        {
            KillProcessByName("Narrator");
        }

        #endregion

        #region 截屏工具 (Snipping Tool)

        /// <summary>打开 Windows 截图工具（Snipping Tool 或 Snip & Sketch）。</summary>
        public static void OpenSnippingTool()
        {
            try
            {
                // 尝试新版截图工具
                Process.Start("ms-screenclip:");
            }
            catch
            {
                // 回退到经典版
                try { Process.Start("SnippingTool.exe"); } catch { }
            }
        }

        #endregion

        #region 高对比度

        /// <summary>获取高对比度主题是否已启用。</summary>
        public static bool IsHighContrastEnabled()
        {
            HIGHCONTRAST hc = new HIGHCONTRAST();
            hc.cbSize = Marshal.SizeOf(typeof(HIGHCONTRAST));
            if (SystemParametersInfo(SPI_GETHIGHCONTRAST, (uint)hc.cbSize, ref hc, 0))
                return (hc.dwFlags & HCF_HIGHCONTRASTON) != 0;
            return false;
        }

        /// <summary>启用或禁用高对比度主题。</summary>
        /// <param name="enable">true 启用高对比度，false 禁用。</param>
        public static void EnableHighContrast(bool enable)
        {
            HIGHCONTRAST hc = new HIGHCONTRAST();
            hc.cbSize = Marshal.SizeOf(typeof(HIGHCONTRAST));
            // 修复：先 GET 现有设置（保留配色方案等参数），仅切换开关位，避免零值结构体覆盖；
            // 并补 SPIF_UPDATEINIFILE 使设置持久化（原仅 SENDCHANGE，注销后丢失）
            SystemParametersInfo(SPI_GETHIGHCONTRAST, (uint)hc.cbSize, ref hc, 0);
            if (enable)
                hc.dwFlags |= HCF_HIGHCONTRASTON;
            else
                hc.dwFlags &= ~HCF_HIGHCONTRASTON;
            SystemParametersInfo(SPI_SETHIGHCONTRAST, (uint)hc.cbSize, ref hc, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        #endregion

        #region 粘滞键 (Sticky Keys)

        /// <summary>获取粘滞键是否已启用。</summary>
        public static bool IsStickyKeysEnabled()
        {
            STICKYKEYS sk = new STICKYKEYS();
            sk.cbSize = Marshal.SizeOf(typeof(STICKYKEYS));
            if (SystemParametersInfo(SPI_GETSTICKYKEYS, (uint)sk.cbSize, ref sk, 0))
                return (sk.dwFlags & SKF_STICKYKEYSON) != 0;
            return false;
        }

        /// <summary>启用或禁用粘滞键。</summary>
        /// <param name="enable">true 启用。</param>
        public static void EnableStickyKeys(bool enable)
        {
            STICKYKEYS sk = new STICKYKEYS();
            sk.cbSize = Marshal.SizeOf(typeof(STICKYKEYS));
            // 修复：先 GET 现有设置再切换开关位，避免零值结构体把“可用”等标志一并清掉；GET 失败时给默认可用标志
            if (!SystemParametersInfo(SPI_GETSTICKYKEYS, (uint)sk.cbSize, ref sk, 0))
                sk.dwFlags = SKF_AVAILABLE;
            if (enable)
                sk.dwFlags |= SKF_STICKYKEYSON;
            else
                sk.dwFlags &= ~SKF_STICKYKEYSON;
            SystemParametersInfo(SPI_SETSTICKYKEYS, (uint)sk.cbSize, ref sk, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        #endregion

        #region 筛选键 (Filter Keys)

        /// <summary>获取筛选键是否已启用。</summary>
        public static bool IsFilterKeysEnabled()
        {
            FILTERKEYS fk = new FILTERKEYS();
            fk.cbSize = Marshal.SizeOf(typeof(FILTERKEYS));
            if (SystemParametersInfo(SPI_GETFILTERKEYS, (uint)fk.cbSize, ref fk, 0))
                return (fk.dwFlags & FKF_FILTERKEYSON) != 0;
            return false;
        }

        /// <summary>启用或禁用筛选键。</summary>
        /// <param name="enable">true 启用。</param>
        public static void EnableFilterKeys(bool enable)
        {
            FILTERKEYS fk = new FILTERKEYS();
            fk.cbSize = Marshal.SizeOf(typeof(FILTERKEYS));
            // 修复：先 GET 现有设置（保留等待/延迟/重复等时间参数），再切换开关位；
            // 原实现用全零结构体 SET 会把各项时间参数清零并导致功能异常
            if (!SystemParametersInfo(SPI_GETFILTERKEYS, (uint)fk.cbSize, ref fk, 0))
                fk.dwFlags = FKF_AVAILABLE;
            if (enable)
                fk.dwFlags |= FKF_FILTERKEYSON;
            else
                fk.dwFlags &= ~FKF_FILTERKEYSON;
            SystemParametersInfo(SPI_SETFILTERKEYS, (uint)fk.cbSize, ref fk, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        #endregion

        #region 切换键 (Toggle Keys)

        /// <summary>获取切换键是否已启用。</summary>
        public static bool IsToggleKeysEnabled()
        {
            TOGGLEKEYS tk = new TOGGLEKEYS();
            tk.cbSize = Marshal.SizeOf(typeof(TOGGLEKEYS));
            if (SystemParametersInfo(SPI_GETTOGGLEKEYS, (uint)tk.cbSize, ref tk, 0))
                return (tk.dwFlags & TKF_TOGGLEKEYSON) != 0;
            return false;
        }

        /// <summary>启用或禁用切换键。</summary>
        /// <param name="enable">true 启用。</param>
        public static void EnableToggleKeys(bool enable)
        {
            TOGGLEKEYS tk = new TOGGLEKEYS();
            tk.cbSize = Marshal.SizeOf(typeof(TOGGLEKEYS));
            // 修复：先 GET 现有设置再切换开关位，避免零值结构体把“可用”标志清掉
            if (!SystemParametersInfo(SPI_GETTOGGLEKEYS, (uint)tk.cbSize, ref tk, 0))
                tk.dwFlags = TKF_AVAILABLE;
            if (enable)
                tk.dwFlags |= TKF_TOGGLEKEYSON;
            else
                tk.dwFlags &= ~TKF_TOGGLEKEYSON;
            SystemParametersInfo(SPI_SETTOGGLEKEYS, (uint)tk.cbSize, ref tk, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        #endregion

        #region 鼠标键 (Mouse Keys)

        /// <summary>获取鼠标键是否已启用。</summary>
        public static bool IsMouseKeysEnabled()
        {
            MOUSEKEYS mk = new MOUSEKEYS();
            mk.cbSize = Marshal.SizeOf(typeof(MOUSEKEYS));
            if (SystemParametersInfo(SPI_GETMOUSEKEYS, (uint)mk.cbSize, ref mk, 0))
                return (mk.dwFlags & MKF_MOUSEKEYSON) != 0;
            return false;
        }

        /// <summary>启用或禁用鼠标键。</summary>
        /// <param name="enable">true 启用。</param>
        public static void EnableMouseKeys(bool enable)
        {
            MOUSEKEYS mk = new MOUSEKEYS();
            mk.cbSize = Marshal.SizeOf(typeof(MOUSEKEYS));
            // 修复：先 GET 现有设置（保留最大速度、加速时间等参数），再切换开关位；
            // 原实现用全零结构体 SET 会把鼠标键速度参数清零并导致功能异常
            if (!SystemParametersInfo(SPI_GETMOUSEKEYS, (uint)mk.cbSize, ref mk, 0))
                mk.dwFlags = MKF_AVAILABLE;
            if (enable)
                mk.dwFlags |= MKF_MOUSEKEYSON;
            else
                mk.dwFlags &= ~MKF_MOUSEKEYSON;
            SystemParametersInfo(SPI_SETMOUSEKEYS, (uint)mk.cbSize, ref mk, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        #endregion

        #region 文本光标指示器 (Text Cursor Indicator)

        /// <summary>获取文本光标指示器是否已启用。</summary>
        public static bool IsTextCursorIndicatorEnabled()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Accessibility"))
                {
                    if (key != null)
                    {
                        int val = (int)key.GetValue("Text Cursor Indicator", 0);
                        return val == 1;
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>启用或禁用文本光标指示器。</summary>
        /// <param name="enable">true 启用。</param>
        public static void EnableTextCursorIndicator(bool enable)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Accessibility", true))
                {
                    if (key != null)
                    {
                        key.SetValue("Text Cursor Indicator", enable ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region 辅助方法

        /// <summary>通过进程名结束进程。</summary>
        private static void KillProcessByName(string processName)
        {
            try
            {
                foreach (Process p in Process.GetProcessesByName(processName))
                {
                    // 修复：Process 对象持有进程句柄，用完必须释放，否则反复调用泄漏句柄
                    using (p)
                    {
                        try { p.Kill(); } catch { }
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Win32 API 与常量

        // 修复：补 SPIF_UPDATEINIFILE，设置时写入用户配置文件，否则注销后设置丢失
        private const uint SPIF_UPDATEINIFILE = 0x0001;
        private const uint SPIF_SENDCHANGE = 0x0002;

        private const uint SPI_GETHIGHCONTRAST = 0x0042;
        private const uint SPI_SETHIGHCONTRAST = 0x0043;
        private const uint HCF_HIGHCONTRASTON = 0x00000001;

        private const uint SPI_GETSTICKYKEYS = 0x003A;
        private const uint SPI_SETSTICKYKEYS = 0x003B;
        private const uint SKF_STICKYKEYSON = 0x00000001;
        private const uint SKF_AVAILABLE = 0x00000002;

        private const uint SPI_GETFILTERKEYS = 0x0032;
        private const uint SPI_SETFILTERKEYS = 0x0033;
        private const uint FKF_FILTERKEYSON = 0x00000001;
        private const uint FKF_AVAILABLE = 0x00000002;

        private const uint SPI_GETTOGGLEKEYS = 0x0034;
        private const uint SPI_SETTOGGLEKEYS = 0x0035;
        private const uint TKF_TOGGLEKEYSON = 0x00000001;
        private const uint TKF_AVAILABLE = 0x00000002;

        private const uint SPI_GETMOUSEKEYS = 0x0036;
        private const uint SPI_SETMOUSEKEYS = 0x0037;
        private const uint MKF_MOUSEKEYSON = 0x00000001;
        private const uint MKF_AVAILABLE = 0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct HIGHCONTRAST
        {
            public int cbSize;
            public uint dwFlags;
            public IntPtr lpszDefaultScheme;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STICKYKEYS
        {
            public int cbSize;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FILTERKEYS
        {
            // 修复：补齐结构体字段。原定义只有 cbSize+dwFlags（8 字节），
            // 与系统 FILTERKEYS（24 字节）大小不符，SPI_GET/SETFILTERKEYS 会因 cbSize 校验失败而无效
            public int cbSize;
            public uint dwFlags;
            public uint iWaitMSec;
            public uint iDelayMSec;
            public uint iRepeatMSec;
            public uint iBounceMSec;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOGGLEKEYS
        {
            public int cbSize;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEKEYS
        {
            // 修复：补齐结构体字段。原定义只有 cbSize+dwFlags（8 字节），
            // 与系统 MOUSEKEYS（24 字节）大小不符，SPI_GET/SETMOUSEKEYS 会因 cbSize 校验失败而无效
            public int cbSize;
            public uint dwFlags;
            public uint iMaxSpeed;
            public uint iTimeToMaxSpeed;
            public uint iCtrlSpeed;
            public uint dwReserved1;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref HIGHCONTRAST pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref STICKYKEYS pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref FILTERKEYS pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref TOGGLEKEYS pvParam, uint fWinIni);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref MOUSEKEYS pvParam, uint fWinIni);

        #endregion
    }
}
