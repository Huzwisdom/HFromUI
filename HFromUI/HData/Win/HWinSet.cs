using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace HFromUI.HData.Win
{
    /// <summary>
    /// 终极系统控制、监控与安全工具类。
    /// 提供窗口管理、进程启动、系统监控、线程/CPU优化、电源、输入、桌面、音量、剪贴板、
    /// 系统时间、安全策略、网络适配器管理、启动项、文件关联、环境变量、Shell操作、
    /// USB设备、自动播放、文件保护、鼠标/键盘、电源计划、时区、默认应用、性能选项、
    /// 虚拟内存、共享文件夹、时间同步、输入法、资源管理器选项、网络状态与网卡详细信息等。
    /// 依赖 <see cref="HWin"/> 和 System.Management。
    /// </summary>
    public  class HWinSet
    {
        #region 窗口管理

        /// <summary>最大化指定窗口。</summary>
        public static void MaximizeWindow(IntPtr hWnd)
        {
            HWin.ShowWindow(hWnd, (int)HWin.SW_SHOWMAXIMIZED);
        }

        /// <summary>最小化指定窗口。</summary>
        public static void MinimizeWindow(IntPtr hWnd)
        {
            HWin.ShowWindow(hWnd, (int)HWin.SW_SHOWMINIMIZED);
        }

        /// <summary>恢复指定窗口到正常大小。</summary>
        public static void RestoreWindow(IntPtr hWnd)
        {
            HWin.ShowWindow(hWnd, (int)HWin.SW_RESTORE);
        }

        /// <summary>隐藏指定窗口。</summary>
        public static void HideWindow(IntPtr hWnd)
        {
            HWin.ShowWindow(hWnd, (int)HWin.SW_HIDE);
        }

        /// <summary>正常显示并激活指定窗口。</summary>
        public static void ShowWindowNormal(IntPtr hWnd)
        {
            HWin.ShowWindow(hWnd, (int)HWin.SW_SHOW);
        }

        /// <summary>将窗口设置为置顶或取消置顶。</summary>
        public static void SetTopMost(IntPtr hWnd, bool topMost)
        {
            IntPtr HWND_TOPMOST = new IntPtr(-1);
            IntPtr HWND_NOTOPMOST = new IntPtr(-2);
            HWin.SetWindowPos(hWnd, topMost ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }

        /// <summary>使窗口进入全屏模式（无边框），并可选隐藏任务栏。</summary>
        public static void FullScreenWindow(IntPtr hWnd, bool hideTaskbar)
        {
            int style = (int)HWin.GetWindowLongPtr(hWnd, GWL_STYLE);
            int exStyle = (int)HWin.GetWindowLongPtr(hWnd, GWL_EXSTYLE);
            style &= ~((int)HWin.WS_CAPTION | (int)HWin.WS_THICKFRAME | (int)HWin.WS_SYSMENU);
            exStyle |= (int)HWin.WS_EX_TOPMOST;
            HWin.SetWindowLongPtr(hWnd, GWL_STYLE, new IntPtr(style));
            HWin.SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));
            int w = HWin.GetSystemMetrics(HWin.SM_CXSCREEN);
            int h = HWin.GetSystemMetrics(HWin.SM_CYSCREEN);
            HWin.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, w, h, SWP_FRAMECHANGED);
            if (hideTaskbar) ShowTaskbar(false);
        }

        /// <summary>退出全屏模式，恢复窗口样式。</summary>
        public static void RestoreFromFullScreen(IntPtr hWnd, bool showTaskbar)
        {
            int style = (int)HWin.GetWindowLongPtr(hWnd, GWL_STYLE);
            style |= (int)HWin.WS_CAPTION | (int)HWin.WS_THICKFRAME | (int)HWin.WS_SYSMENU;
            HWin.SetWindowLongPtr(hWnd, GWL_STYLE, new IntPtr(style));
            HWin.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
            if (showTaskbar) ShowTaskbar(true);
        }

        /// <summary>显示或隐藏 Windows 任务栏。</summary>
        public static void ShowTaskbar(bool show)
        {
            IntPtr taskbar = HWin.FindWindow("Shell_TrayWnd", null);
            if (taskbar != IntPtr.Zero)
            {
                HWin.ShowWindow(taskbar, show ? (int)HWin.SW_SHOW : (int)HWin.SW_HIDE);
            }
        }

        /// <summary>设置窗口透明度。</summary>
        public static void SetWindowTransparency(IntPtr hWnd, byte alpha)
        {
            int exStyle = (int)HWin.GetWindowLongPtr(hWnd, GWL_EXSTYLE);
            if ((exStyle & (int)HWin.WS_EX_LAYERED) == 0)
            {
                HWin.SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle | (int)HWin.WS_EX_LAYERED));
            }
            SetLayeredWindowAttributes(hWnd, 0, alpha, LWA_ALPHA);
        }

        /// <summary>移动窗口并调整大小。</summary>
        public static void MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint)
        {
            HWin.MoveWindow(hWnd, x, y, width, height, repaint);
        }

        /// <summary>获取窗口的屏幕坐标矩形。</summary>
        public static Rectangle GetWindowRect(IntPtr hWnd)
        {
            HWin.RECT r = new HWin.RECT();
            HWin.GetWindowRect(hWnd, ref r);
            return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
        }

        /// <summary>枚举所有顶层窗口句柄。</summary>
        public static List<IntPtr> EnumTopWindows()
        {
            List<IntPtr> list = new List<IntPtr>();
            HWin.EnumWindows(delegate (IntPtr hWnd, IntPtr lParam) { list.Add(hWnd); return true; }, IntPtr.Zero);
            return list;
        }

        /// <summary>通过窗口标题（部分匹配）查找窗口句柄。</summary>
        public static IntPtr FindWindowByTitle(string titlePart)
        {
            foreach (IntPtr hWnd in EnumTopWindows())
            {
                StringBuilder sb = new StringBuilder(256);
                HWin.GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().Contains(titlePart))
                    return hWnd;
            }
            return IntPtr.Zero;
        }

        #endregion

        #region 进程启动与管理

        public enum ShowWindowCommands
        {
            SW_HIDE = 0, SW_SHOWNORMAL = 1, SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3, SW_SHOWNOACTIVATE = 4, SW_SHOW = 5,
            SW_MINIMIZE = 6, SW_SHOWMINNOACTIVE = 7, SW_SHOWNA = 8,
            SW_RESTORE = 9, SW_SHOWDEFAULT = 10
        }

        /// <summary>通过 ShellExecute 执行程序或打开文件。</summary>
        public static IntPtr ShellExecute(string file, string parameters, string operation,
            string directory, ShowWindowCommands showCmd)
        {
            return HWin.ShellExecute(IntPtr.Zero, operation, file, parameters, directory, (int)showCmd);
        }

        /// <summary>用指定程序打开一个文件。</summary>
        public static IntPtr OpenFileWithProgram(string programPath, string filePath)
        {
            return ShellExecute(programPath, "\"" + filePath + "\"", "open", "", ShowWindowCommands.SW_SHOWNORMAL);
        }

        /// <summary>使用系统默认关联程序打开文件。</summary>
        public static IntPtr OpenFileWithDefaultProgram(string filePath)
        {
            return ShellExecute(filePath, "", "open", "", ShowWindowCommands.SW_SHOWNORMAL);
        }

        /// <summary>以管理员权限运行指定程序。</summary>
        public static IntPtr RunAsAdmin(string filePath, string parameters)
        {
            return HWin.ShellExecute(IntPtr.Zero, "runas", filePath, parameters, "", (int)ShowWindowCommands.SW_SHOWNORMAL);
        }

        /// <summary>启动进程并等待其退出，返回退出代码。</summary>
        public static int StartProcessAndWait(string fileName, string arguments, bool hidden)
        {
            ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments);
            if (hidden) { psi.WindowStyle = ProcessWindowStyle.Hidden; psi.CreateNoWindow = true; }
            using (Process p = Process.Start(psi))
            {
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        /// <summary>启动进程并返回 Process 对象，便于后续控制。</summary>
        public static Process StartProcess(string fileName, string arguments, bool hidden)
        {
            ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments);
            if (hidden) { psi.WindowStyle = ProcessWindowStyle.Hidden; psi.CreateNoWindow = true; }
            return Process.Start(psi);
        }

        /// <summary>使用资源管理器打开文件夹。</summary>
        public static void OpenFolder(string folderPath)
        {
            Process.Start("explorer.exe", folderPath);
        }

        #endregion

        #region 系统监控 (CPU / 内存 / 磁盘 / 网络)

        /// <summary>获取系统总体 CPU 使用率（百分比）。</summary>
        public static float GetSystemCpuUsagePercent()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 包装非托管 WMI 对象，用完必须释放
                            return Convert.ToSingle(obj["PercentProcessorTime"]);
            }
            catch { }
            return -1f;
        }

        /// <summary>获取每个逻辑核心的 CPU 使用率。</summary>
        public static List<float> GetCpuUsagePerCore()
        {
            List<float> list = new List<float>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name <> '_Total'"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 包装非托管 WMI 对象，用完必须释放
                            list.Add(Convert.ToSingle(obj["PercentProcessorTime"]));
            }
            catch { }
            return list;
        }

        /// <summary>获取当前进程的 CPU 占用率（采样500ms）。</summary>
        public static double GetCurrentProcessCpuUsage()
        {
            try
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    TimeSpan startCpu = p.TotalProcessorTime;
                    DateTime startTime = DateTime.UtcNow;
                    Thread.Sleep(500);
                    TimeSpan endCpu = p.TotalProcessorTime;
                    DateTime endTime = DateTime.UtcNow;
                    double cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
                    double totalMsPassed = (endTime - startTime).TotalMilliseconds;
                    return Math.Min((cpuUsedMs / (Environment.ProcessorCount * totalMsPassed)) * 100, 100);
                }
            }
            catch { }
            return -1;
        }

        /// <summary>获取系统物理内存总量（字节）。</summary>
        public static long GetTotalPhysicalMemoryBytes()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 包装非托管 WMI 对象，用完必须释放
                            return Convert.ToInt64(obj["TotalVisibleMemorySize"]) * 1024;
            }
            catch { }
            return -1;
        }

        /// <summary>获取可用物理内存（字节）。</summary>
        public static long GetAvailablePhysicalMemoryBytes()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 包装非托管 WMI 对象，用完必须释放
                            return Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024;
            }
            catch { }
            return -1;
        }

        /// <summary>获取内存使用率（0-100）。</summary>
        public static float GetMemoryUsagePercent()
        {
            long total = GetTotalPhysicalMemoryBytes();
            long avail = GetAvailablePhysicalMemoryBytes();
            if (total <= 0 || avail < 0) return -1f;
            return (1f - (float)avail / total) * 100f;
        }

        /// <summary>获取所有逻辑驱动器的盘符、总大小和可用空间。</summary>
        public static List<Tuple<string, long, long>> GetDriveInfo()
        {
            List<Tuple<string, long, long>> list = new List<Tuple<string, long, long>>();
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                    if (drive.IsReady)
                        list.Add(Tuple.Create(drive.Name, drive.TotalSize, drive.AvailableFreeSpace));
            }
            catch { }
            return list;
        }

        /// <summary>获取指定网络接口的速度（bps）。</summary>
        public static long GetNetworkInterfaceSpeed(string interfaceName)
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    if (ni.Name == interfaceName) return ni.Speed;
            }
            catch { }
            return -1;
        }

        /// <summary>获取当前 TCP 连接数。</summary>
        public static int GetTcpConnectionsCount()
        {
            try
            {
                using (PerformanceCounter perf = new PerformanceCounter("TCPv4", "Connections Established"))
                    return (int)perf.NextValue();
            }
            catch { return -1; }
        }

        /// <summary>获取系统句柄总数。</summary>
        public static int GetSystemHandleCount()
        {
            try
            {
                using (PerformanceCounter perf = new PerformanceCounter("Process", "Handle Count", "_Total"))
                    return (int)perf.NextValue();
            }
            catch { return -1; }
        }

        /// <summary>获取系统线程总数。</summary>
        public static int GetSystemThreadCount()
        {
            try
            {
                using (PerformanceCounter perf = new PerformanceCounter("System", "Threads"))
                    return (int)perf.NextValue();
            }
            catch { return -1; }
        }

        #endregion

        #region 进程/线程优先级与CPU亲和性

        /// <summary>将当前进程设置为高优先级。</summary>
        public static void SetCurrentProcessPriorityHigh()
        {
            using (Process p = Process.GetCurrentProcess())
                p.PriorityClass = ProcessPriorityClass.High;
        }

        /// <summary>通过Win32设置进程优先级类。</summary>
        public static void SetProcessPriorityClass(uint priorityClass)
        {
            SetPriorityClass(HWin.GetCurrentProcess(), priorityClass);
        }

        /// <summary>将指定线程的优先级设置为最高。</summary>
        public static void SetThreadPriorityHighest(Thread thread)
        {
            thread.Priority = ThreadPriority.Highest;
        }

        /// <summary>通过Win32线程句柄设置优先级。</summary>
        public static void SetThreadPriorityWin32(IntPtr hThread, int priority)
        {
            HWin.SetThreadPriority(hThread, priority);
        }

        /// <summary>设置当前进程的 CPU 亲和性掩码。</summary>
        public static void SetProcessAffinityMask(UIntPtr affinityMask)
        {
            SetProcessAffinityMask(HWin.GetCurrentProcess(), affinityMask);
        }

        /// <summary>使当前进程使用所有可用处理器。</summary>
        public static void UseAllProcessors()
        {
            SetProcessAffinityMask((UIntPtr)0xFFFFFFFF);
        }

        /// <summary>将当前线程提升至实时优先级（谨慎使用）。</summary>
        public static void BoostThreadToRealtime()
        {
            HWin.SetThreadPriority(HWin.GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);
        }

        /// <summary>设置当前进程的工作集大小。</summary>
        public static void SetProcessWorkingSetSize(IntPtr min, IntPtr max)
        {
            SetProcessWorkingSetSize(HWin.GetCurrentProcess(), min, max);
        }

        #endregion

        #region 电源与显示器

        /// <summary>强制关闭计算机。</summary>
        public static void Shutdown()
        {
            AdjustShutdownPrivilege();
            ExitWindowsEx(EWX_SHUTDOWN | EWX_FORCE, 0);
        }

        /// <summary>强制重新启动计算机。</summary>
        public static void Reboot()
        {
            AdjustShutdownPrivilege();
            ExitWindowsEx(EWX_REBOOT | EWX_FORCE, 0);
        }

        /// <summary>注销当前用户。</summary>
        public static void LogOff()
        {
            ExitWindowsEx(EWX_LOGOFF, 0);
        }

        /// <summary>系统休眠（挂起到磁盘）。</summary>
        public static void Hibernate()
        {
            AdjustShutdownPrivilege();
            SetSuspendState(true, true, false);
        }

        /// <summary>系统睡眠（挂起到内存）。</summary>
        public static void SleepSystem()
        {
            SetSuspendState(false, true, false);
        }

        /// <summary>锁定工作站。</summary>
        public static void LockWorkStation()
        {
            LockWorkStationAPI();
        }

        /// <summary>关闭显示器。</summary>
        public static void TurnOffMonitor()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, MONITOR_OFF);
        }

        /// <summary>打开显示器。</summary>
        public static void TurnOnMonitor()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, MONITOR_ON);
        }

        #endregion

        #region 输入设备与多媒体

        /// <summary>模拟键盘按键（按下并释放）。</summary>
        public static void SimulateKeyPress(byte virtualKey)
        {
            HWin.keybd_event(virtualKey, 0, HWin.KEYEVENTF_KEYDOWN, IntPtr.Zero);
            HWin.keybd_event(virtualKey, 0, HWin.KEYEVENTF_KEYUP, IntPtr.Zero);
        }

        /// <summary>模拟鼠标左键单击。</summary>
        public static void SimulateLeftClick()
        {
            HWin.mouse_event(HWin.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            HWin.mouse_event(HWin.MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        /// <summary>移动鼠标光标到指定屏幕坐标。</summary>
        public static void MoveCursorTo(int x, int y)
        {
            HWin.SetCursorPos(x, y);
        }

        /// <summary>获取当前光标在屏幕中的位置。</summary>
        public static Point GetCursorPosition()
        {
            HWin.POINT pt = new HWin.POINT();
            HWin.GetCursorPos(ref pt);
            return new Point(pt.X, pt.Y);
        }

        public enum MediaKey
        {
            MediaNextTrack = 0xB0, MediaPrevTrack = 0xB1, MediaStop = 0xB2,
            MediaPlayPause = 0xB3, VolumeMute = 0xAD, VolumeDown = 0xAE, VolumeUp = 0xAF
        }

        /// <summary>发送多媒体控制键（如播放/暂停）。</summary>
        public static void SendMediaKey(MediaKey key)
        {
            SimulateKeyPress((byte)key);
        }

        #endregion

        #region 桌面与显示设置

        public enum WallpaperStyle { Centered = 0, Tiled = 1, Stretched = 2, Fit = 6, Fill = 10, Span = 22 }

        /// <summary>设置桌面壁纸。</summary>
        public static void SetWallpaper(string imagePath, WallpaperStyle style)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
            {
                if (key == null) return;
                key.SetValue("WallpaperStyle", ((int)style).ToString());
                key.SetValue("TileWallpaper", style == WallpaperStyle.Tiled ? "1" : "0");
                key.Close();
            }
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        /// <summary>启动屏幕保护程序。</summary>
        public static void StartScreenSaver()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, (IntPtr)SC_SCREENSAVE, IntPtr.Zero);
        }

        /// <summary>获取主显示器分辨率。</summary>
        public static Size GetPrimaryScreenResolution()
        {
            return new Size(HWin.GetSystemMetrics(HWin.SM_CXSCREEN), HWin.GetSystemMetrics(HWin.SM_CYSCREEN));
        }

        /// <summary>获取桌面图标是否可见。</summary>
        public static bool GetDesktopIconsVisible()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                    return key != null && (int)key.GetValue("HideIcons", 0) == 0;
            }
            catch { return true; }
        }

        /// <summary>设置桌面图标是否可见。</summary>
        public static void SetDesktopIconsVisible(bool visible)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                {
                    if (key != null) key.SetValue("HideIcons", visible ? 0 : 1, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        /// <summary>获取任务栏是否自动隐藏。</summary>
        public static bool GetTaskbarAutoHide()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3"))
                {
                    byte[] data = key?.GetValue("Settings") as byte[];
                    return data != null && data.Length > 8 && (data[8] & 1) != 0;
                }
            }
            catch { }
            return false;
        }

        /// <summary>设置任务栏自动隐藏。</summary>
        public static void SetTaskbarAutoHide(bool autoHide)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3", true))
                {
                    byte[] data = key?.GetValue("Settings") as byte[];
                    if (data != null && data.Length > 8)
                    {
                        if (autoHide) data[8] |= 1; else data[8] &= 0xFE;
                        key.SetValue("Settings", data, RegistryValueKind.Binary);
                    }
                }
            }
            catch { }
        }

        /// <summary>获取屏幕保护程序超时时间（秒）。</summary>
        public static int GetScreenSaverTimeout()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                    return Convert.ToInt32(key?.GetValue("ScreenSaveTimeOut", 600));
            }
            catch { return 600; }
        }

        /// <summary>设置屏幕保护程序超时时间（秒）。</summary>
        public static void SetScreenSaverTimeout(int seconds)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                    key?.SetValue("ScreenSaveTimeOut", seconds.ToString());
                SystemParametersInfo(SPI_SETSCREENSAVETIMEOUT, (uint)seconds, IntPtr.Zero, 0);
            }
            catch { }
        }

        /// <summary>获取屏幕保护程序密码保护是否启用。</summary>
        public static bool GetScreenSaverPasswordProtected()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                    return key?.GetValue("ScreenSaverIsSecure", "0").ToString() == "1";
            }
            catch { return false; }
        }

        /// <summary>设置屏幕保护程序密码保护。</summary>
        public static void SetScreenSaverPasswordProtected(bool protect)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                    key?.SetValue("ScreenSaverIsSecure", protect ? "1" : "0");
            }
            catch { }
        }

        /// <summary>获取当前应用是否使用浅色主题。</summary>
        public static bool GetAppsUseLightTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    return key != null && (int)key.GetValue("AppsUseLightTheme", 1) == 1;
            }
            catch { return true; }
        }

        /// <summary>设置应用使用浅色/深色主题。</summary>
        public static void SetAppsUseLightTheme(bool light)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", true))
                    key?.SetValue("AppsUseLightTheme", light ? 1 : 0, RegistryValueKind.DWord);
            }
            catch { }
        }

        #endregion

        #region 音量控制

        /// <summary>获取主音量（0-100）。优先使用 Vista+ Core Audio API（默认渲染端点），
        /// 失败时回退到遗留 mixer API；均不可用返回 -1。</summary>
        public static int GetMasterVolume()
        {
            int coreAudio = GetMasterVolumeCoreAudio();
            if (coreAudio >= 0) return coreAudio;
            return GetMasterVolumeLegacy();
        }

        /// <summary>设置主音量（0-100）。优先使用 Vista+ Core Audio API，失败时回退到遗留 mixer API。</summary>
        public static void SetMasterVolume(int percent)
        {
            int p = percent < 0 ? 0 : (percent > 100 ? 100 : percent);
            if (SetMasterVolumeCoreAudio(p)) return;
            SetMasterVolumeLegacy(p);
        }

        /// <summary>遗留 mixer API 路径（XP 及 Core Audio 不可用时的回退）：获取主音量（0-100）。</summary>
        private static int GetMasterVolumeLegacy()
        {
            IntPtr hMixer;
            if (mixerOpen(out hMixer, 0, IntPtr.Zero, IntPtr.Zero, 0) != MMSYSERR_NOERROR) return -1;
            try
            {
                // 修复：目标线路无音量控件时回退遍历源线路（Vista+ 常见）
                uint volumeLineId;
                if (!FindVolumeLineId(hMixer, out volumeLineId)) return -1;

                MIXERLINECONTROLS mlc = new MIXERLINECONTROLS
                {
                    cbStruct = (uint)Marshal.SizeOf(typeof(MIXERLINECONTROLS)),
                    dwLineID = volumeLineId,
                    dwControlType = MIXERCONTROL_CONTROLTYPE_VOLUME,
                    cControls = 1,
                    cbmxctrl = (uint)Marshal.SizeOf(typeof(MIXERCONTROL))
                };
                mlc.pamxctrl = Marshal.AllocHGlobal((int)mlc.cbmxctrl);
                try
                {
                    // 修复：MIXERCONTROL 数组元素的 cbStruct 必须先初始化，否则调用可能失败
                    MIXERCONTROL mcInit = new MIXERCONTROL { cbStruct = (uint)Marshal.SizeOf(typeof(MIXERCONTROL)) };
                    Marshal.StructureToPtr(mcInit, mlc.pamxctrl, false);
                    if (mixerGetLineControls(hMixer, ref mlc, MIXER_GETLINECONTROLSF_ONEBYTYPE) != MMSYSERR_NOERROR) return -1;
                    MIXERCONTROL mc = (MIXERCONTROL)Marshal.PtrToStructure(mlc.pamxctrl, typeof(MIXERCONTROL));

                    MIXERCONTROLDETAILS mcd = new MIXERCONTROLDETAILS
                    {
                        cbStruct = (uint)Marshal.SizeOf(typeof(MIXERCONTROLDETAILS)),
                        dwControlID = mc.dwControlID,
                        cChannels = 1,
                        cbDetails = (uint)Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED))
                    };
                    mcd.paDetails = Marshal.AllocHGlobal((int)mcd.cbDetails);
                    try
                    {
                        if (mixerGetControlDetails(hMixer, ref mcd, MIXER_GETCONTROLDETAILSF_VALUE) != MMSYSERR_NOERROR) return -1;
                        MIXERCONTROLDETAILS_UNSIGNED mcdu = (MIXERCONTROLDETAILS_UNSIGNED)Marshal.PtrToStructure(mcd.paDetails, typeof(MIXERCONTROLDETAILS_UNSIGNED));
                        // 修复：lMax 来自修正后的结构体布局（Bounds 联合）；为 0 时直接返回失败，避免除零
                        if (mc.lMax <= 0) return -1;
                        return (int)(mcdu.dwValue * 100 / (uint)mc.lMax);
                    }
                    finally { Marshal.FreeHGlobal(mcd.paDetails); }
                }
                finally { Marshal.FreeHGlobal(mlc.pamxctrl); }
            }
            finally { mixerClose(hMixer); }
        }

        /// <summary>遗留 mixer API 路径（XP 及 Core Audio 不可用时的回退）：设置主音量（0-100）。</summary>
        private static void SetMasterVolumeLegacy(int percent)
        {
            IntPtr hMixer;
            if (mixerOpen(out hMixer, 0, IntPtr.Zero, IntPtr.Zero, 0) != MMSYSERR_NOERROR) return;
            try
            {
                // 修复：目标线路无音量控件时回退遍历源线路（Vista+ 常见）
                uint volumeLineId;
                if (!FindVolumeLineId(hMixer, out volumeLineId)) return;

                MIXERLINECONTROLS mlc = new MIXERLINECONTROLS
                {
                    cbStruct = (uint)Marshal.SizeOf(typeof(MIXERLINECONTROLS)),
                    dwLineID = volumeLineId,
                    dwControlType = MIXERCONTROL_CONTROLTYPE_VOLUME,
                    cControls = 1,
                    cbmxctrl = (uint)Marshal.SizeOf(typeof(MIXERCONTROL))
                };
                mlc.pamxctrl = Marshal.AllocHGlobal((int)mlc.cbmxctrl);
                try
                {
                    // 修复：MIXERCONTROL 数组元素的 cbStruct 必须先初始化，否则调用可能失败
                    MIXERCONTROL mcInit = new MIXERCONTROL { cbStruct = (uint)Marshal.SizeOf(typeof(MIXERCONTROL)) };
                    Marshal.StructureToPtr(mcInit, mlc.pamxctrl, false);
                    if (mixerGetLineControls(hMixer, ref mlc, MIXER_GETLINECONTROLSF_ONEBYTYPE) != MMSYSERR_NOERROR) return;
                    MIXERCONTROL mc = (MIXERCONTROL)Marshal.PtrToStructure(mlc.pamxctrl, typeof(MIXERCONTROL));

                    // 修复：lMax 来自修正后的结构体布局（Bounds 联合）；为 0 时直接返回，避免算出错误音量
                    uint max = (uint)mc.lMax;
                    if (max == 0) return;
                    uint newVolume = (uint)(percent * max / 100);

                    MIXERCONTROLDETAILS mcd = new MIXERCONTROLDETAILS
                    {
                        cbStruct = (uint)Marshal.SizeOf(typeof(MIXERCONTROLDETAILS)),
                        dwControlID = mc.dwControlID,
                        cChannels = 1,
                        cbDetails = (uint)Marshal.SizeOf(typeof(MIXERCONTROLDETAILS_UNSIGNED))
                    };
                    MIXERCONTROLDETAILS_UNSIGNED mcdu = new MIXERCONTROLDETAILS_UNSIGNED { dwValue = newVolume };
                    mcd.paDetails = Marshal.AllocHGlobal(Marshal.SizeOf(mcdu));
                    try
                    {
                        Marshal.StructureToPtr(mcdu, mcd.paDetails, false);
                        mixerSetControlDetails(hMixer, ref mcd, MIXER_SETCONTROLDETAILSF_VALUE);
                    }
                    finally { Marshal.FreeHGlobal(mcd.paDetails); }
                }
                finally { Marshal.FreeHGlobal(mlc.pamxctrl); }
            }
            finally { mixerClose(hMixer); }
        }

        #endregion

        #region Core Audio（Vista+ 主音量，现代默认路径；遗留 mixer 仅作回退）

        private const int CLSCTX_ALL = 0x17;
        private const int EDataFlow_eRender = 0;   // 渲染（输出）设备
        private const int ERole_eConsole = 0;      // 控制台角色（系统默认音量）

        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumeratorComObject { }

        [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            [PreserveSig] int EnumAudioEndpoints(int dataFlow, int dwStateMask, out IntPtr ppDevices);
            [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
        }

        [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
            [PreserveSig] int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
            [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
            [PreserveSig] int GetState(out int pdwState);
        }

        [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
            [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
            [PreserveSig] int GetChannelCount(out uint pnChannelCount);
            [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
            [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
            [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
            [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
            [PreserveSig] int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
            [PreserveSig] int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
            [PreserveSig] int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            [PreserveSig] int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            [PreserveSig] int SetMute(bool bMute, ref Guid pguidEventContext);
            [PreserveSig] int GetMute(out bool pbMute);
            [PreserveSig] int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
            [PreserveSig] int VolumeStepUp(ref Guid pguidEventContext);
            [PreserveSig] int VolumeStepDown(ref Guid pguidEventContext);
            [PreserveSig] int QueryHardwareSupport(out uint pdwHardwareSupportMask);
            [PreserveSig] int GetVolumeRange(out float pfLevelMinDB, out float pfLevelMaxDB, out float pfVolumeIncrementDB);
        }

        /// <summary>打开默认渲染（扬声器）端点的音量控制接口；失败返回 null（由调用方释放已拿到的 COM 对象）。</summary>
        private static IAudioEndpointVolume OpenDefaultEndpointVolume(out IMMDevice device, out IMMDeviceEnumerator enumerator)
        {
            device = null;
            enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
            if (enumerator.GetDefaultAudioEndpoint(EDataFlow_eRender, ERole_eConsole, out device) != 0 || device == null) return null;
            Guid iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A"); // IID_IAudioEndpointVolume
            object volObj;
            if (device.Activate(ref iid, CLSCTX_ALL, IntPtr.Zero, out volObj) != 0 || volObj == null) return null;
            return (IAudioEndpointVolume)volObj;
        }

        /// <summary>获取 masterVolumeCoreAudio。</summary>
        private static int GetMasterVolumeCoreAudio()
        {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;
            IAudioEndpointVolume vol = null;
            try
            {
                vol = OpenDefaultEndpointVolume(out device, out enumerator);
                if (vol == null) return -1;
                float scalar;
                if (vol.GetMasterVolumeLevelScalar(out scalar) != 0) return -1;
                int v = (int)Math.Round(scalar * 100f);
                return v < 0 ? 0 : (v > 100 ? 100 : v);
            }
            catch
            {
                return -1; // 无音频端点/服务不可用等环境下静默回退
            }
            finally
            {
                if (vol != null) Marshal.ReleaseComObject(vol);
                if (device != null) Marshal.ReleaseComObject(device);
                if (enumerator != null) Marshal.ReleaseComObject(enumerator);
            }
        }

        /// <summary>设置 masterVolumeCoreAudio。</summary>
        private static bool SetMasterVolumeCoreAudio(int percent0to100)
        {
            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;
            IAudioEndpointVolume vol = null;
            try
            {
                vol = OpenDefaultEndpointVolume(out device, out enumerator);
                if (vol == null) return false;
                Guid eventContext = Guid.Empty;
                return vol.SetMasterVolumeLevelScalar(percent0to100 / 100f, ref eventContext) == 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (vol != null) Marshal.ReleaseComObject(vol);
                if (device != null) Marshal.ReleaseComObject(device);
                if (enumerator != null) Marshal.ReleaseComObject(enumerator);
            }
        }

        #endregion

        #region 剪贴板

        /// <summary>获取剪贴板中的文本。</summary>
        public static string GetClipboardText()
        {
            if (!HWin.OpenClipboard(IntPtr.Zero)) return null;
            IntPtr hData = HWin.GetClipboardData(CF_UNICODETEXT);
            if (hData == IntPtr.Zero) { HWin.CloseClipboard(); return null; }
            IntPtr pText = GlobalLock(hData);
            string text = Marshal.PtrToStringUni(pText);
            GlobalUnlock(hData);
            HWin.CloseClipboard();
            return text;
        }

        /// <summary>将文本写入剪贴板。</summary>
        public static void SetClipboardText(string text)
        {
            if (!HWin.OpenClipboard(IntPtr.Zero)) return;
            HWin.EmptyClipboard();
            IntPtr hGlobal = Marshal.StringToHGlobalUni(text);
            // 修复：SetClipboardData 成功后内存所有权转移给剪贴板，绝不能再 FreeHGlobal，否则破坏剪贴板/堆；仅失败时自行释放
            if (HWin.SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                Marshal.FreeHGlobal(hGlobal);
            HWin.CloseClipboard();
        }

        #endregion

        #region 系统时间与计算机名

        /// <summary>设置系统时间（本地时间）。</summary>
        public static bool SetSystemTime(DateTime time)
        {
            // 修复：Win32 SetSystemTime 接收的是 UTC 时间，直接填本地时间会导致时差错误，须先转换
            DateTime utc = time.ToUniversalTime();
            HWin.SYSTEMTIME st = new HWin.SYSTEMTIME
            {
                wYear = (ushort)utc.Year,
                wMonth = (ushort)utc.Month,
                wDay = (ushort)utc.Day,
                wHour = (ushort)utc.Hour,
                wMinute = (ushort)utc.Minute,
                wSecond = (ushort)utc.Second,
                wMilliseconds = (ushort)utc.Millisecond
            };
            return HWin.SetSystemTime(ref st);
        }

        /// <summary>设置计算机名（需要重启生效）。</summary>
        public static bool SetComputerName(string name)
        {
            return SetComputerNameEx(ComputerNameFormat.ComputerNamePhysicalDnsHostname, name);
        }

        #endregion

        #region 安全策略

        /// <summary>禁用或启用任务管理器。</summary>
        public static void DisableTaskManager(bool disable)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true)
                ?? Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (disable) key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
            else key.DeleteValue("DisableTaskMgr", false);
            key.Close();
        }

        /// <summary>禁用或启用注册表编辑器。</summary>
        public static void DisableRegistryEditor(bool disable)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true)
                ?? Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (disable) key.SetValue("DisableRegistryTools", 1, RegistryValueKind.DWord);
            else key.DeleteValue("DisableRegistryTools", false);
            key.Close();
        }

        /// <summary>获取 UAC 级别。</summary>
        public static int GetUACLevel()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
                    return (int)(key?.GetValue("ConsentPromptBehaviorAdmin", 5) ?? 5);
            }
            catch { return 5; }
        }

        /// <summary>设置 UAC 级别。</summary>
        public static void SetUACLevel(int level)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true))
                {
                    if (key != null)
                    {
                        key.SetValue("ConsentPromptBehaviorAdmin", level, RegistryValueKind.DWord);
                        key.SetValue("EnableLUA", level > 0 ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        /// <summary>获取远程桌面是否启用。</summary>
        public static bool GetRemoteDesktopEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server"))
                    return key != null && (int)key.GetValue("fDenyTSConnections", 1) == 0;
            }
            catch { return false; }
        }

        /// <summary>启用或禁用远程桌面。</summary>
        public static void SetRemoteDesktopEnabled(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", true))
                    key?.SetValue("fDenyTSConnections", enable ? 0 : 1, RegistryValueKind.DWord);
            }
            catch { }
        }

        /// <summary>获取 Windows 防火墙是否启用（标准配置文件）。</summary>
        public static bool GetFirewallEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"))
                    return key != null && (int)key.GetValue("EnableFirewall", 1) == 1;
            }
            catch { return true; }
        }

        /// <summary>启用或禁用 Windows 防火墙。</summary>
        public static void SetFirewallEnabled(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", true))
                    key?.SetValue("EnableFirewall", enable ? 1 : 0, RegistryValueKind.DWord);
            }
            catch { }
        }

        /// <summary>获取 Windows 更新设置。</summary>
        public static int GetWindowsUpdateSetting()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
                    return key != null ? (int)key.GetValue("AUOptions", 3) : 3;
            }
            catch { return 3; }
        }

        /// <summary>设置 Windows 更新选项。</summary>
        public static void SetWindowsUpdateSetting(int auOptions)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", true)
                    ?? Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
                    key.SetValue("AUOptions", auOptions, RegistryValueKind.DWord);
            }
            catch { }
        }

        /// <summary>获取系统还原是否启用。</summary>
        public static bool GetSystemRestoreEnabled()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT DisableSR FROM Win32_SystemRestore"))
                    foreach (ManagementObject obj in searcher.Get())
                        return (uint)obj["DisableSR"] == 0;
            }
            catch { }
            return false;
        }

        /// <summary>启用或禁用系统还原（针对系统盘）。</summary>
        public static void SetSystemRestoreEnabled(bool enable)
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_SystemRestore"))
                {
                    ManagementBaseObject inParams = mc.GetMethodParameters("Enable");
                    inParams["Drive"] = Environment.SystemDirectory.Substring(0, 1) + ":\\";
                    if (enable) mc.InvokeMethod("Enable", inParams, null);
                    else mc.InvokeMethod("Disable", new object[] { Environment.SystemDirectory.Substring(0, 1) + ":\\" });
                }
            }
            catch { }
        }

        #endregion

        #region 网络适配器管理

        /// <summary>网卡详细信息（包含IP、流量、状态等）。</summary>
        public class NetworkAdapterInfo
        {
            /// <summary>名称。</summary>
            public string Name { get; set; }
            /// <summary>Description 成员。</summary>
            public string Description { get; set; }
            /// <summary>MACAddress 成员。</summary>
            public string MACAddress { get; set; }
            /// <summary>IPAddresses 成员。</summary>
            public string[] IPAddresses { get; set; }
            /// <summary>SubnetMasks 成员。</summary>
            public string[] SubnetMasks { get; set; }
            /// <summary>Gateways 成员。</summary>
            public string[] Gateways { get; set; }
            /// <summary>DnsServers 成员。</summary>
            public string[] DnsServers { get; set; }
            /// <summary>DhcpEnabled 成员。</summary>
            public bool DhcpEnabled { get; set; }
            /// <summary>DhcpServer 成员。</summary>
            public string DhcpServer { get; set; }
            /// <summary>AdapterType 成员。</summary>
            public string AdapterType { get; set; }
            /// <summary>Speed 成员。</summary>
            public long Speed { get; set; }
            /// <summary>Status 成员。</summary>
            public OperationalStatus Status { get; set; }
            /// <summary>BytesSent 成员。</summary>
            public long BytesSent { get; set; }
            /// <summary>BytesReceived 成员。</summary>
            public long BytesReceived { get; set; }
        }

        /// <summary>获取所有物理网卡的完整信息（包括IP、流量等）。</summary>
        public static List<NetworkAdapterInfo> GetNetworkAdapters()
        {
            var adapters = new List<NetworkAdapterInfo>();
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                        continue;

                    var info = new NetworkAdapterInfo
                    {
                        Name = ni.Name,
                        Description = ni.Description,
                        MACAddress = ni.GetPhysicalAddress().ToString(),
                        AdapterType = ni.NetworkInterfaceType.ToString(),
                        Speed = ni.Speed,
                        Status = ni.OperationalStatus,
                        BytesSent = ni.GetIPv4Statistics().BytesSent,
                        BytesReceived = ni.GetIPv4Statistics().BytesReceived
                    };

                    IPInterfaceProperties ipProps = ni.GetIPProperties();
                    if (ipProps != null)
                    {
                        info.IPAddresses = ipProps.UnicastAddresses
                            .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(a => a.Address.ToString()).ToArray();
                        info.SubnetMasks = ipProps.UnicastAddresses
                            .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(a => a.IPv4Mask.ToString()).ToArray();
                        info.Gateways = ipProps.GatewayAddresses.Select(g => g.Address.ToString()).ToArray();
                        info.DnsServers = ipProps.DnsAddresses.Select(d => d.ToString()).ToArray();
                        info.DhcpEnabled = ipProps.GetIPv4Properties().IsDhcpEnabled;
                        info.DhcpServer = ipProps.DhcpServerAddresses.FirstOrDefault()?.ToString() ?? "";
                    }
                    adapters.Add(info);
                }
            }
            catch { }
            return adapters;
        }

        /// <summary>启用或禁用指定网卡（基于WMI）。</summary>
        public static bool SetNetworkAdapterEnabled(string adapterName, bool enable)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapter WHERE Name='{adapterName}'"))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        using (obj)
                        {
                            // 修复：Enable/Disable 的返回对象中 ReturnValue 非 0 表示失败（如权限不足），必须检查，不能直接返回 true
                            // 显式指定 (object[])null 以绑定 InvokeMethod(string, object[]) 重载
                            ManagementBaseObject result = (ManagementBaseObject)obj.InvokeMethod(enable ? "Enable" : "Disable", (object[])null);
                            using (result)
                            {
                                return result != null && Convert.ToUInt32(result["ReturnValue"]) == 0;
                            }
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>检测当前是否连接到互联网。</summary>
        public static bool IsInternetAvailable()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0");
                    using (var stream = client.OpenRead("http://www.microsoft.com"))
                        return true;
                }
            }
            catch { return false; }
        }

        /// <summary>获取当前连接的WiFi信号强度（0-100）。</summary>
        public static int GetWiFiSignalStrength()
        {
            try
            {
                // 修复：MSNdis_80211_* 类位于 root\wmi 命名空间，默认 root\cimv2 下查询必失败
                ManagementScope scope = new ManagementScope(@"\\.\root\wmi");
                using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSNdis_80211_ReceivedSignalStrength WHERE Active=true")))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj)
                            return Convert.ToInt32(obj["Ndis80211ReceivedSignalStrength"]);
            }
            catch { }
            return -1;
        }

        /// <summary>获取指定网卡的网络利用率（百分比）。</summary>
        public static double GetNetworkUtilization(string adapterName)
        {
            try
            {
                var adapters = GetNetworkAdapters();
                var adapter = adapters.FirstOrDefault(a => a.Name == adapterName);
                if (adapter == null || adapter.Speed <= 0) return -1;

                long bytesSent1 = adapter.BytesSent;
                long bytesRecv1 = adapter.BytesReceived;
                Thread.Sleep(1000);
                adapters = GetNetworkAdapters();
                adapter = adapters.FirstOrDefault(a => a.Name == adapterName);
                if (adapter == null) return -1;
                long bytesSent2 = adapter.BytesSent;
                long bytesRecv2 = adapter.BytesReceived;

                long totalBytes = (bytesSent2 - bytesSent1) + (bytesRecv2 - bytesRecv1);
                double bitsPerSec = totalBytes * 8;
                double utilization = bitsPerSec / adapter.Speed * 100;
                return Math.Min(utilization, 100);
            }
            catch { return -1; }
        }

        /// <summary>启用或禁用指定网络适配器（使用 netsh 命令）。</summary>
        public static void SetNetworkAdapterState(string adapterName, bool enable)
        {
            Process.Start("netsh", $"interface set interface \"{adapterName}\" {(enable ? "enable" : "disable")}");
        }

        /// <summary>设置静态IP地址。</summary>
        public static bool SetNetworkAdapterIP(string adapterName, string ipAddress, string subnetMask, string gateway)
        {
            try
            {
                string cmd = $"interface ip set address name=\"{adapterName}\" static {ipAddress} {subnetMask}";
                if (!string.IsNullOrEmpty(gateway)) cmd += $" {gateway} 1";
                Process.Start("netsh", cmd);
                return true;
            }
            catch { return false; }
        }

        /// <summary>设置DNS服务器。</summary>
        public static bool SetNetworkAdapterDNS(string adapterName, string primaryDns, string secondaryDns)
        {
            try
            {
                Process.Start("netsh", $"interface ip set dns name=\"{adapterName}\" static {primaryDns}");
                if (!string.IsNullOrEmpty(secondaryDns))
                    Process.Start("netsh", $"interface ip add dns name=\"{adapterName}\" {secondaryDns} index=2");
                return true;
            }
            catch { return false; }
        }

        /// <summary>获取系统代理服务器地址。</summary>
        public static string GetProxyServer()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings"))
                    return key?.GetValue("ProxyServer")?.ToString();
            }
            catch { return null; }
        }

        /// <summary>设置系统代理服务器。</summary>
        public static void SetProxyServer(string proxy, bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true))
                {
                    if (key != null)
                    {
                        key.SetValue("ProxyServer", proxy);
                        key.SetValue("ProxyEnable", enable ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        #endregion

        #region 开机启动

        /// <summary>添加开机启动项（当前用户）。</summary>
        public static void AddStartup(string name, string exePath)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)
                ?? Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            key.SetValue(name, exePath);
            key.Close();
        }

        /// <summary>移除开机启动项（当前用户）。</summary>
        public static void RemoveStartup(string name)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key != null)
            {
                key.DeleteValue(name, false);
                key.Close();
            }
        }

        #endregion

        #region 文件关联

        /// <summary>设置文件类型关联。</summary>
        public static void SetFileAssociation(string extension, string progId, string description, string iconPath, string openCommand)
        {
            // 修复：CreateSubKey 返回的注册表项必须释放，原代码泄漏多个 RegistryKey 句柄
            using (RegistryKey progKey = Registry.ClassesRoot.CreateSubKey(progId))
            {
                progKey.SetValue("", description);
                using (RegistryKey iconKey = progKey.CreateSubKey("DefaultIcon"))
                    iconKey.SetValue("", iconPath);
                using (RegistryKey cmdKey = progKey.CreateSubKey(@"shell\open\command"))
                    cmdKey.SetValue("", openCommand);
            }
            using (RegistryKey extKey = Registry.ClassesRoot.CreateSubKey(extension))
                extKey.SetValue("", progId);
        }

        #endregion

        #region 环境变量

        /// <summary>获取环境变量。</summary>
        public static string GetEnvironmentVariable(string name, EnvironmentVariableTarget target)
        {
            return Environment.GetEnvironmentVariable(name, target);
        }

        /// <summary>设置环境变量。</summary>
        public static void SetEnvironmentVariable(string name, string value, EnvironmentVariableTarget target)
        {
            Environment.SetEnvironmentVariable(name, value, target);
        }

        #endregion

        #region Shell 文件操作

        /// <summary>使用Shell复制文件或文件夹（支持撤销）。</summary>
        public static void ShellCopy(string source, string dest)
        {
            SHFileOperation(FO_COPY, source, dest);
        }

        /// <summary>使用Shell移动文件或文件夹。</summary>
        public static void ShellMove(string source, string dest)
        {
            SHFileOperation(FO_MOVE, source, dest);
        }

        /// <summary>使用Shell删除文件或文件夹。</summary>
        public static void ShellDelete(string path)
        {
            SHFileOperation(FO_DELETE, path, null);
        }

        /// <summary>SHFileOperation 方法。</summary>
        private static void SHFileOperation(uint func, string from, string to)
        {
            HWin.SHFILEOPSTRUCT op = new HWin.SHFILEOPSTRUCT
            {
                hwnd = IntPtr.Zero,
                wFunc = func,
                pFrom = from + '\0',
                pTo = to != null ? to + '\0' : null,
                fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT
            };
            HWin.SHFileOperation(ref op);
        }

        #endregion

        #region USB 设备信息

        /// <summary>USB设备信息。</summary>
        public class UsbDeviceInfo
        {
            public string DeviceID;
            public string Description;
            public string Manufacturer;
            public string Service;
            public string PNPDeviceID;
            public string Status;
            public string Protocol;
            public string DriveLetter;
            public string VolumeName;
            public string FileSystem;
            public long TotalSize;
            public long FreeSpace;

            public override string ToString()
            {
                string info = $"{Description} ({Manufacturer})";
                if (!string.IsNullOrEmpty(DriveLetter)) info += $" [{DriveLetter}]";
                if (TotalSize > 0) info += $" {TotalSize / (1024 * 1024 * 1024)}GB";
                return info;
            }
        }

        /// <summary>获取所有USB集线器设备。</summary>
        public static List<UsbDeviceInfo> GetUsbDevices()
        {
            List<UsbDeviceInfo> list = new List<UsbDeviceInfo>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBHub"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 包装非托管 WMI 对象，用完必须释放
                            list.Add(new UsbDeviceInfo
                            {
                                DeviceID = obj["DeviceID"]?.ToString(),
                                Description = obj["Description"]?.ToString(),
                                Manufacturer = obj["Manufacturer"]?.ToString(),
                                Service = obj["Service"]?.ToString(),
                                PNPDeviceID = obj["PNPDeviceID"]?.ToString(),
                                Status = obj["Status"]?.ToString(),
                                Protocol = obj["Protocol"]?.ToString()
                            });
            }
            catch { }
            return list;
        }

        /// <summary>获取所有USB存储设备（包含盘符、容量等）。</summary>
        public static List<UsbDeviceInfo> GetUsbStorageDevices()
        {
            List<UsbDeviceInfo> list = new List<UsbDeviceInfo>();
            try
            {
                using (ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'"))
                using (ManagementObjectCollection diskResults = diskSearcher.Get())
                {
                    foreach (ManagementObject disk in diskResults)
                    {
                        // 修复：disk/part/logical 均为 ManagementObject，用完必须释放
                        using (disk)
                        {
                            UsbDeviceInfo info = new UsbDeviceInfo
                            {
                                Description = disk["Caption"]?.ToString() ?? disk["Model"]?.ToString(),
                                Manufacturer = disk["Manufacturer"]?.ToString(),
                                PNPDeviceID = disk["PNPDeviceID"]?.ToString(),
                                Status = disk["Status"]?.ToString()
                            };

                            using (ManagementObjectSearcher partSearcher = new ManagementObjectSearcher(
                                $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{disk["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition"))
                            using (ManagementObjectCollection partResults = partSearcher.Get())
                            {
                                foreach (ManagementObject part in partResults)
                                {
                                    using (part)
                                    using (ManagementObjectSearcher logicalSearcher = new ManagementObjectSearcher(
                                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} WHERE AssocClass = Win32_LogicalDiskToPartition"))
                                    using (ManagementObjectCollection logicalResults = logicalSearcher.Get())
                                    {
                                        foreach (ManagementObject logical in logicalResults)
                                        {
                                            using (logical)
                                            {
                                                info.DriveLetter = logical["DeviceID"]?.ToString();
                                                info.VolumeName = logical["VolumeName"]?.ToString();
                                                info.FileSystem = logical["FileSystem"]?.ToString();
                                                info.TotalSize = Convert.ToInt64(logical["Size"] ?? 0);
                                                info.FreeSpace = Convert.ToInt64(logical["FreeSpace"] ?? 0);
                                            }
                                        }
                                    }
                                }
                            }
                            list.Add(info);
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        /// <summary>从PNPDeviceID中解析出VID和PID。</summary>
        public static void ParseUsbVidPid(string pnpDeviceId, out string vid, out string pid)
        {
            vid = null; pid = null;
            if (string.IsNullOrEmpty(pnpDeviceId)) return;
            var match = System.Text.RegularExpressions.Regex.Match(pnpDeviceId,
                @"VID_(\w{4})&PID_(\w{4})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                vid = match.Groups[1].Value;
                pid = match.Groups[2].Value;
            }
        }

        #endregion

        #region 自动播放管理

        public enum AutoPlayDeviceType { Unknown = 0, RemovableDrive, CDRom, Camera, Phone, Other, All }
        public enum AutoPlayAction { PromptEachTime, TakeNoAction, OpenFolderToViewFiles, UseDefaultProgram, Custom }

        /// <summary>获取指定设备类型的自动播放操作。</summary>
        public static AutoPlayAction GetAutoPlayAction(AutoPlayDeviceType deviceType)
        {
            string keyName = GetAutoPlayDeviceTypeString(deviceType);
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\EventHandlersDefaultSelection"))
                {
                    string action = key?.GetValue(keyName)?.ToString();
                    if (action == "MSPromptEachTime") return AutoPlayAction.PromptEachTime;
                    if (action == "MSTakeNoAction") return AutoPlayAction.TakeNoAction;
                    return string.IsNullOrEmpty(action) ? AutoPlayAction.UseDefaultProgram : AutoPlayAction.Custom;
                }
            }
            catch { return AutoPlayAction.UseDefaultProgram; }
        }

        /// <summary>设置指定设备类型的自动播放操作。</summary>
        public static void SetAutoPlayAction(AutoPlayDeviceType deviceType, AutoPlayAction action)
        {
            string keyName = GetAutoPlayDeviceTypeString(deviceType);
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\AutoplayHandlers\EventHandlersDefaultSelection"))
                {
                    switch (action)
                    {
                        case AutoPlayAction.PromptEachTime: key.SetValue(keyName, "MSPromptEachTime"); break;
                        case AutoPlayAction.TakeNoAction: key.SetValue(keyName, "MSTakeNoAction"); break;
                        case AutoPlayAction.OpenFolderToViewFiles: key.SetValue(keyName, "MSOpenFolder"); break;
                        default: key.DeleteValue(keyName, false); break;
                    }
                }
            }
            catch { }
        }

        /// <summary>获取自动播放是否启用。</summary>
        public static bool GetAutoPlayEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"))
                {
                    object val = key?.GetValue("NoDriveTypeAutoRun");
                    return !(val is int intVal && intVal == 0x91);
                }
            }
            catch { return true; }
        }

        /// <summary>启用或禁用自动播放。</summary>
        public static void SetAutoPlayEnabled(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"))
                {
                    if (enabled) key.DeleteValue("NoDriveTypeAutoRun", false);
                    else key.SetValue("NoDriveTypeAutoRun", 0x91, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        /// <summary>获取 autoPlayDeviceTypeString。</summary>
        private static string GetAutoPlayDeviceTypeString(AutoPlayDeviceType type)
        {
            switch (type)
            {
                case AutoPlayDeviceType.RemovableDrive: return "StorageOnArrival";
                case AutoPlayDeviceType.CDRom: return "PlayCDAudioOnArrival";
                case AutoPlayDeviceType.Camera: return "CameraOnArrival";
                case AutoPlayDeviceType.Phone: return "PhoneOnArrival";
                default: return "UnknownContentOnArrival";
            }
        }

        #endregion

        #region 文件保护与安全

        /// <summary>使用EFS加密文件或文件夹（仅NTFS）。</summary>
        public static bool EncryptFileEFS(string path) { try { File.Encrypt(path); return true; } catch { return false; } }

        /// <summary>解密EFS加密的文件或文件夹。</summary>
        public static bool DecryptFileEFS(string path) { try { File.Decrypt(path); return true; } catch { return false; } }

        /// <summary>设置文件为只读+隐藏+系统属性。</summary>
        public static bool ProtectFileAttributes(string path)
        {
            try { File.SetAttributes(path, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System); return true; } catch { return false; }
        }

        /// <summary>恢复文件为普通属性。</summary>
        public static bool UnprotectFileAttributes(string path)
        {
            try { File.SetAttributes(path, FileAttributes.Normal); return true; } catch { return false; }
        }

        /// <summary>设置文件权限仅允许当前用户完全控制。</summary>
        public static bool SetFileProtectionForCurrentUser(string path)
        {
            try
            {
                FileSecurity security = File.GetAccessControl(path);
                security.SetAccessRuleProtection(true, false);
                foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(NTAccount)))
                    security.RemoveAccessRule(rule);
                security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name,
                    FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(path, security);
                return true;
            }
            catch { return false; }
        }

        /// <summary>恢复文件权限为继承父目录的默认权限。</summary>
        public static bool ResetFileProtectionToDefault(string path)
        {
            try
            {
                FileSecurity security = File.GetAccessControl(path);
                security.SetAccessRuleProtection(false, false);
                File.SetAccessControl(path, security);
                return true;
            }
            catch { return false; }
        }

        /// <summary>以独占方式打开文件流（锁定）。</summary>
        public static FileStream LockFile(string path)
        {
            try { return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); } catch { return null; }
        }

        /// <summary>安全擦除文件（覆盖3次后删除）。</summary>
        public static bool ShredFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    long size = fs.Length;
                    byte[] zeros = new byte[4096];
                    for (int pass = 0; pass < 3; pass++)
                    {
                        fs.Position = 0;
                        while (fs.Position < size)
                            fs.Write(zeros, 0, (int)Math.Min(size - fs.Position, zeros.Length));
                        fs.Flush();
                    }
                    fs.SetLength(0);
                }
                File.Delete(path);
                return true;
            }
            catch { return false; }
        }

        /// <summary>计算文件的SHA256哈希值。</summary>
        public static string ComputeFileHash(string path)
        {
            try
            {
                using (SHA256 sha = SHA256.Create())
                using (FileStream fs = File.OpenRead(path))
                {
                    byte[] hash = sha.ComputeHash(fs);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch { return null; }
        }

        /// <summary>验证文件哈希是否与给定值一致。</summary>
        public static bool VerifyFileHash(string path, string originalHash)
        {
            string current = ComputeFileHash(path);
            return current != null && current.Equals(originalHash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>批量保护USB设备上指定扩展名的文件。</summary>
        public static void ProtectUsbFiles(string driveLetter, string[] extensions, bool encrypt, bool setAttributes, bool setPermissions)
        {
            try
            {
                foreach (string ext in extensions)
                    foreach (string file in Directory.GetFiles(driveLetter, "*" + ext, SearchOption.AllDirectories))
                    {
                        if (encrypt) EncryptFileEFS(file);
                        if (setAttributes) ProtectFileAttributes(file);
                        if (setPermissions) SetFileProtectionForCurrentUser(file);
                    }
            }
            catch { }
        }

        /// <summary>在USB设备上创建不可删除的占位文件。</summary>
        public static bool CreateUndeletablePlaceholder(string usbDriveLetter, string fileName)
        {
            try
            {
                string path = Path.Combine(usbDriveLetter, fileName);
                File.WriteAllText(path, "PROTECTED");
                ProtectFileAttributes(path);
                SetFileProtectionForCurrentUser(path);
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region 鼠标/键盘设置

        /// <summary>获取 mouseSpeed。</summary>
        public static int GetMouseSpeed() { int speed = 10; SystemParametersInfo(SPI_GETMOUSESPEED, 0, out speed, 0); return speed; }
        // 修复：SPI_SETMOUSESPEED 的新速度值通过 pvParam 传递，原代码错放到 fWinIni 且 pvParam 传了 Zero
        public static void SetMouseSpeed(int speed) { SystemParametersInfo(SPI_SETMOUSESPEED, 0, (IntPtr)speed, 0); }

        /// <summary>获取 mouseWheelScrollLines。</summary>
        public static int GetMouseWheelScrollLines() { int lines = 3; SystemParametersInfo(SPI_GETWHEELSCROLLLINES, 0, out lines, 0); return lines; }
        // 修复：SPI_SETWHEELSCROLLLINES 的行数通过 pvParam 传递，原代码错放到 uiParam 且 pvParam 传了 Zero
        public static void SetMouseWheelScrollLines(int lines) { SystemParametersInfo(SPI_SETWHEELSCROLLLINES, 0, (IntPtr)lines, 0); }

        /// <summary>获取 keyboardSpeed。</summary>
        public static int GetKeyboardSpeed() { int speed = 31; SystemParametersInfo(SPI_GETKEYBOARDSPEED, 0, out speed, 0); return speed; }
        /// <summary>设置 keyboardSpeed。</summary>
        public static void SetKeyboardSpeed(int speed) { SystemParametersInfo(SPI_SETKEYBOARDSPEED, (uint)speed, IntPtr.Zero, 0); }

        /// <summary>获取 keyboardRepeatDelay。</summary>
        public static int GetKeyboardRepeatDelay() { int delay = 1; SystemParametersInfo(SPI_GETKEYBOARDDELAY, 0, out delay, 0); return delay; }
        /// <summary>设置 keyboardRepeatDelay。</summary>
        public static void SetKeyboardRepeatDelay(int delay) { SystemParametersInfo(SPI_SETKEYBOARDDELAY, (uint)delay, IntPtr.Zero, 0); }

        #endregion

        #region 电源计划、时区、默认应用等

        /// <summary>获取 activePowerPlan。</summary>
        public static string GetActivePowerPlan()
        {
            try
            {
                // 修复：Win32_PowerPlan 类位于 root\cimv2\power 命名空间，默认 root\cimv2 下查询必失败
                ManagementScope scope = new ManagementScope(@"\\.\root\cimv2\power");
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM Win32_PowerPlan WHERE IsActive = True")))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 用完必须释放
                            return obj["ElementName"]?.ToString();
            }
            catch { }
            return null;
        }

        /// <summary>设置 activePowerPlan。</summary>
        public static void SetActivePowerPlan(string planName) { try { Process.Start("powercfg", $"/setactive \"{planName}\""); } catch { } }

        /// <summary>获取 timeZone。</summary>
        public static string GetTimeZone() => TimeZoneInfo.Local.DisplayName;
        /// <summary>设置 timeZone。</summary>
        public static void SetTimeZone(string timeZoneId) { try { Process.Start("tzutil", $"/s \"{timeZoneId}\""); } catch { } }

        /// <summary>获取 defaultBrowserProgId。</summary>
        public static string GetDefaultBrowserProgId()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
                    return key?.GetValue("ProgId")?.ToString();
            }
            catch { return null; }
        }

        /// <summary>设置 defaultBrowser。</summary>
        public static void SetDefaultBrowser(string progId)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice", true))
                    key?.SetValue("ProgId", progId);
            }
            catch { }
        }

        /// <summary>获取 processorScheduling。</summary>
        public static int GetProcessorScheduling()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl"))
                    return (int)(key?.GetValue("Win32PrioritySeparation", 2) ?? 2);
            }
            catch { return 2; }
        }

        /// <summary>设置 processorScheduling。</summary>
        public static void SetProcessorScheduling(int value)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl", true))
                    key?.SetValue("Win32PrioritySeparation", value, RegistryValueKind.DWord);
            }
            catch { }
        }

        /// <summary>获取 pagingFileSize。</summary>
        public static long[] GetPagingFileSize()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileSetting"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 用完必须释放
                            return new long[] { Convert.ToInt64(obj["InitialSize"]), Convert.ToInt64(obj["MaximumSize"]) };
            }
            catch { }
            return new long[] { 0, 0 };
        }

        /// <summary>设置 pagingFileSize。</summary>
        public static void SetPagingFileSize(long initialSizeMB, long maximumSizeMB)
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_PageFileSetting"))
                using (ManagementObjectCollection instances = mc.GetInstances())
                    foreach (ManagementObject obj in instances)
                    {
                        // 修复：ManagementObject 用完必须释放
                        using (obj)
                        {
                            obj["InitialSize"] = initialSizeMB;
                            obj["MaximumSize"] = maximumSizeMB;
                            obj.Put();
                            break;
                        }
                    }
            }
            catch { }
        }

        /// <summary>获取 sharedFolders。</summary>
        public static List<string> GetSharedFolders()
        {
            List<string> shares = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, Path FROM Win32_Share"))
                using (ManagementObjectCollection results = searcher.Get())
                    foreach (ManagementObject obj in results)
                        using (obj) // 修复：ManagementObject 用完必须释放
                            shares.Add($"{obj["Name"]} ({obj["Path"]})");
            }
            catch { }
            return shares;
        }

        /// <summary>CreateShare 方法。</summary>
        public static bool CreateShare(string folderPath, string shareName, string description)
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_Share"))
                using (ManagementBaseObject inParams = mc.GetMethodParameters("Create"))
                {
                    inParams["Path"] = folderPath;
                    inParams["Name"] = shareName;
                    inParams["Description"] = description;
                    inParams["Type"] = 0;
                    // 修复：Create 的返回对象中 ReturnValue 非 0 表示失败（如共享名已存在），必须检查
                    using (ManagementBaseObject outParams = mc.InvokeMethod("Create", inParams, null))
                        return outParams != null && Convert.ToUInt32(outParams["ReturnValue"]) == 0;
                }
            }
            catch { return false; }
        }

        /// <summary>DeleteShare 方法。</summary>
        public static bool DeleteShare(string shareName)
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_Share"))
                using (ManagementObjectCollection instances = mc.GetInstances())
                    foreach (ManagementObject obj in instances)
                        // 修复：ManagementObject 用完必须释放
                        using (obj)
                            if (obj["Name"].ToString() == shareName)
                            {
                                obj.Delete();
                                return true;
                            }
            }
            catch { }
            return false;
        }

        /// <summary>获取 timeSyncServer。</summary>
        public static string GetTimeSyncServer()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\W32Time\Parameters"))
                    return key?.GetValue("NtpServer")?.ToString()?.Split(',')[0];
            }
            catch { return null; }
        }

        /// <summary>设置 timeSyncServer。</summary>
        public static void SetTimeSyncServer(string server)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\W32Time\Parameters", true))
                    key?.SetValue("NtpServer", $"{server},0x1");
            }
            catch { }
        }

        /// <summary>获取 installedInputMethods。</summary>
        public static List<string> GetInstalledInputMethods()
        {
            List<string> list = new List<string>();
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\CTF\Assemblies\0x00000804"))
                {
                    if (key != null)
                        foreach (string sub in key.GetSubKeyNames())
                            list.Add(sub);
                }
            }
            catch { }
            return list;
        }

        /// <summary>获取 showHiddenFiles。</summary>
        public static bool GetShowHiddenFiles()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                    return (int)(key?.GetValue("Hidden", 2) ?? 2) == 1;
            }
            catch { return false; }
        }

        /// <summary>设置 showHiddenFiles。</summary>
        public static void SetShowHiddenFiles(bool show)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                    key?.SetValue("Hidden", show ? 1 : 2, RegistryValueKind.DWord);
            }
            catch { }
        }

        /// <summary>获取 showFileExtensions。</summary>
        public static bool GetShowFileExtensions()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced"))
                    return (int)(key?.GetValue("HideFileExt", 0) ?? 0) == 0;
            }
            catch { return true; }
        }

        /// <summary>设置 showFileExtensions。</summary>
        public static void SetShowFileExtensions(bool show)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true))
                    key?.SetValue("HideFileExt", show ? 0 : 1, RegistryValueKind.DWord);
            }
            catch { }
        }

        #endregion

        #region 常量与 P/Invoke

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const uint LWA_ALPHA = 0x00000002;
        private const uint EWX_LOGOFF = 0;
        private const uint EWX_SHUTDOWN = 1;
        private const uint EWX_REBOOT = 2;
        private const uint EWX_FORCE = 4;
        private const int SC_MONITORPOWER = 0xF170;
        private  const int SC_SCREENSAVE = 0xF140;

        /// <summary>MONITOR_OFF 字段。</summary>
        private static readonly IntPtr MONITOR_OFF = (IntPtr)2;
        /// <summary>HWND_BROADCAST 字段。</summary>
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
        /// <summary>MONITOR_ON 字段。</summary>
        private static readonly IntPtr MONITOR_ON = new IntPtr(-1);
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint CF_UNICODETEXT = 13;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDWININICHANGE = 0x02;
        private const uint SPI_SETDESKWALLPAPER = 0x0014;
        private const uint SPI_SETSCREENSAVETIMEOUT = 0x000F;
        private const int THREAD_PRIORITY_TIME_CRITICAL = 15;
        private const uint FO_MOVE = 0x0001;
        private const uint FO_COPY = 0x0002;
        private const uint FO_DELETE = 0x0003;
        private const ushort FOF_ALLOWUNDO = 0x0040;
        private const ushort FOF_NOCONFIRMATION = 0x0010;
        private const ushort FOF_SILENT = 0x0004;
        private const uint MMSYSERR_NOERROR = 0;
        private const uint MIXERLINE_COMPONENTTYPE_DST_SPEAKERS = 0x00000004;
        private const uint MIXERCONTROL_CONTROLTYPE_VOLUME = 0x50030001;
        private const uint MIXER_GETLINEINFOF_COMPONENTTYPE = 0x00000003;
        private const uint MIXER_GETLINEINFOF_SRCLINE = 0x00000001;
        private const uint MIXER_GETLINECONTROLSF_ONEBYTYPE = 0x00000002;
        private const uint MIXER_GETCONTROLDETAILSF_VALUE = 0x00000000;
        private const uint MIXER_SETCONTROLDETAILSF_VALUE = 0x00000001;

        /// <summary>
        /// 查找主音量控件所在的线路 ID。
        /// 修复：Vista 以后 DST_SPEAKERS 目标线路上可能没有 VOLUME 控件（mixerGetLineControls 返回 INVALCONTROL），
        /// 此时需要遍历该目标线路的各源线路（波形/软件合成器等），第一条带 VOLUME 控件的即作为主音量。
        /// </summary>
        private static bool FindVolumeLineId(IntPtr hMixer, out uint lineId)
        {
            lineId = 0;
            MIXERLINE dest = new MIXERLINE
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(MIXERLINE)),
                dwComponentType = MIXERLINE_COMPONENTTYPE_DST_SPEAKERS
            };
            if (mixerGetLineInfo(hMixer, ref dest, MIXER_GETLINEINFOF_COMPONENTTYPE) != MMSYSERR_NOERROR) return false;

            // 先在扬声器目标线路上直接找
            if (LineHasVolumeControl(hMixer, dest.dwLineID))
            {
                lineId = dest.dwLineID;
                return true;
            }

            // 回退：遍历源线路
            for (uint i = 0; i < dest.cConnections; i++)
            {
                MIXERLINE src = new MIXERLINE
                {
                    cbStruct = (uint)Marshal.SizeOf(typeof(MIXERLINE)),
                    dwDestination = dest.dwDestination,
                    dwSource = i
                };
                if (mixerGetLineInfo(hMixer, ref src, MIXER_GETLINEINFOF_SRCLINE) != MMSYSERR_NOERROR) continue;
                if (LineHasVolumeControl(hMixer, src.dwLineID))
                {
                    lineId = src.dwLineID;
                    return true;
                }
            }
            return false;
        }

        /// <summary>探测指定线路上是否存在音量控件。</summary>
        private static bool LineHasVolumeControl(IntPtr hMixer, uint targetLineId)
        {
            MIXERLINECONTROLS mlc = new MIXERLINECONTROLS
            {
                cbStruct = (uint)Marshal.SizeOf(typeof(MIXERLINECONTROLS)),
                dwLineID = targetLineId,
                dwControlType = MIXERCONTROL_CONTROLTYPE_VOLUME,
                cControls = 1,
                cbmxctrl = (uint)Marshal.SizeOf(typeof(MIXERCONTROL))
            };
            mlc.pamxctrl = Marshal.AllocHGlobal((int)mlc.cbmxctrl);
            try
            {
                MIXERCONTROL mcInit = new MIXERCONTROL { cbStruct = (uint)Marshal.SizeOf(typeof(MIXERCONTROL)) };
                Marshal.StructureToPtr(mcInit, mlc.pamxctrl, false);
                return mixerGetLineControls(hMixer, ref mlc, MIXER_GETLINECONTROLSF_ONEBYTYPE) == MMSYSERR_NOERROR;
            }
            finally { Marshal.FreeHGlobal(mlc.pamxctrl); }
        }
        private const uint SPI_GETMOUSESPEED = 0x0070;
        private const uint SPI_SETMOUSESPEED = 0x0071;
        private const uint SPI_GETWHEELSCROLLLINES = 0x0068;
        private const uint SPI_SETWHEELSCROLLLINES = 0x0069;
        private const uint SPI_GETKEYBOARDSPEED = 0x000A;
        private const uint SPI_SETKEYBOARDSPEED = 0x000B;
        private const uint SPI_GETKEYBOARDDELAY = 0x0016;
        private const uint SPI_SETKEYBOARDDELAY = 0x0017;
        private enum ComputerNameFormat { ComputerNamePhysicalDnsHostname = 5 }

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("kernel32.dll")]
        private static extern bool SetPriorityClass(IntPtr hProcess, uint dwPriorityClass);

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessAffinityMask(IntPtr hProcess, UIntPtr mask);

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr hProcess, IntPtr min, IntPtr max);

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("powrprof.dll")]
        private static extern bool SetSuspendState(bool hibernate, bool force, bool disableWakeEvent);

        // 修复：user32 中导出的函数名是 LockWorkStation，未指定 EntryPoint 会按 LockWorkStationAPI 查找导致 EntryPointNotFoundException
        [DllImport("user32.dll", EntryPoint = "LockWorkStation")]
        private static extern bool LockWorkStationAPI();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, out int pvParam, uint fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetComputerNameEx(ComputerNameFormat nameType, string lpBuffer);

        [DllImport("winmm.dll")]
        private static extern int mixerOpen(out IntPtr phmx, int uMxId, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

        [DllImport("winmm.dll")]
        private static extern int mixerClose(IntPtr hMixer);

        // 修复：原生 mixerGetLineInfo 只有 3 个参数 (HMIXER, LPMIXERLINE, DWORD)，原签名多出 ref int，调用会栈破坏
        // 修复：结构体含 TCHAR 字符串，必须与 MIXERLINE/MIXERCONTROL 的 CharSet.Auto(Unicode) 一致，
        // 否则 cbStruct 按 ANSI/Unicode 混用计算，winmm 返回 MMSYSERR_INVALPARAM(11)
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mixerGetLineInfo(IntPtr hMixer, ref MIXERLINE pmxl, uint fdwInfo);

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mixerGetLineControls(IntPtr hMixer, ref MIXERLINECONTROLS pmxlc, uint fdwControls);

        [DllImport("winmm.dll")]
        private static extern int mixerGetControlDetails(IntPtr hMixer, ref MIXERCONTROLDETAILS pmxcd, uint fdwDetails);

        [DllImport("winmm.dll")]
        private static extern int mixerSetControlDetails(IntPtr hMixer, ref MIXERCONTROLDETAILS pmxcd, uint fdwDetails);

        /// <summary>AdjustShutdownPrivilege 方法。</summary>
        private static void AdjustShutdownPrivilege()
        {
            if (!HWin.OpenProcessToken(HWin.GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr hToken)) return;
            if (!HWin.LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, out HWin.LUID luid)) { HWin.CloseHandle(hToken); return; }
            HWin.TOKEN_PRIVILEGES tp = new HWin.TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Privileges = new HWin.LUID_AND_ATTRIBUTES { Luid = luid, Attributes = SE_PRIVILEGE_ENABLED }
            };
            HWin.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            HWin.CloseHandle(hToken);
        }

        private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MIXERLINE
        {
            // 字段顺序与原生 MIXERLINE(W)（mmsystem.h）完全一致：
            // 10 个 DWORD + szShortName[16] + szName[48] + Target 嵌套结构体
            public uint cbStruct, dwDestination, dwSource, dwLineID, fdwLine, dwUser, dwComponentType, cChannels, cConnections, cControls;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string szShortName;  // MIXER_SHORT_NAME_CHARS = 16
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)] public string szName;       // MIXER_LONG_NAME_CHARS = 48
            // 末尾 Target 是顺序嵌套结构体（不是联合）：描述线路关联的目标设备，各字段同时有效
            public uint dwType;
            public uint dwDeviceID;
            public ushort wMid;   // WORD
            public ushort wPid;   // WORD
            public uint vDriverVersion;  // MMVERSION = DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szPname;      // MAXPNAMELEN = 32
            // 封送大小 = 40 + 32 + 96 + 16 + 64 = 248 字节，与原生 MIXERLINEW 一致
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MIXERCONTROL
        {
            // 原生 MIXERCONTROL(W)：5 个 DWORD + szShortName[16] + szName[48] + Bounds 联合(6 DWORD) + Metrics 联合(6 DWORD)
            // CharSet 必须与 DllImport 的 CharSet.Auto 一致（Unicode），否则名称字段按 ANSI 封送，cbStruct 大小不符
            public uint cbStruct, dwControlID, dwControlType, fdwControl, cMultipleItems;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string szShortName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)] public string szName;
            // Bounds/Metrics 两个联合平铺为 12 个 int 占位；音量控件只使用 lMin/lMax，
            // 其余字段保留以保证封送大小 = 20 + 32 + 96 + 48 = 196 字节，与原生 MIXERCONTROLW 一致
            public int lMin;
            public int lMax;
            public int dwReserved0, dwReserved1, dwReserved2, dwReserved3, dwReserved4, dwReserved5, dwReserved6, dwReserved7, dwReserved8, dwReserved9;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIXERLINECONTROLS
        {
            public uint cbStruct, dwLineID, dwControlType, cControls, cbmxctrl;
            public IntPtr pamxctrl;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIXERCONTROLDETAILS
        {
            // 修复：原生结构在 cChannels 与 cbDetails 之间还有一个联合字段（hwndOwner/cMultipleItems，1 个 DWORD），
            // 缺失会导致 paDetails 指针偏移错误，64 位下结构体大小也不符
            public uint cbStruct, dwControlID, cChannels, cMultipleItems, cbDetails;
            public IntPtr paDetails;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIXERCONTROLDETAILS_UNSIGNED
        {
            public uint dwValue;
        }

        #endregion
    }
}
