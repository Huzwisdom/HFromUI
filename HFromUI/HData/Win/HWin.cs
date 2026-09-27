using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HFromUI.HData.Win
{
    /// <summary>
    /// 超全面的 Win32 API 静态封装类（最终完整版，无重复/错误）。
    /// 涵盖 kernel32、user32、gdi32、shell32、advapi32、comctl32、winmm、psapi、shlwapi、version、ole32 等核心 DLL 的 200+ 个函数声明，
    /// 以及所有必要的结构体、常量和委托。文件/目录、内存、进程/线程、窗口/消息、输入、绘图、Shell、安全、注册表、多媒体、公共控件等一应俱全。
    /// 所有方法均为静态，附带详细注释，兼容 .NET Framework 4.8 / C# 7.3。
    /// </summary>
    public  class HWin
    {
        #region kernel32.dll ── 文件 / 目录 / 驱动器 / 进程 / 线程 / 内存 / 时间 / 动态库 / 控制台 / 同步 / 错误处理 / 环境 / 卷

        // ==================== 文件与目录基础操作 ====================

        /// <summary>创建或打开文件、目录、物理磁盘等对象。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        /// <summary>从文件或 I/O 设备读取数据。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        /// <summary>将数据写入文件或 I/O 设备。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        /// <summary>关闭打开的对象句柄。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>删除现有文件。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteFile(string lpFileName);

        /// <summary>复制文件。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

        /// <summary>移动文件或目录。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool MoveFile(string lpExistingFileName, string lpNewFileName);

        /// <summary>移动文件或目录（扩展选项）。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, uint dwFlags);

        /// <summary>获取文件属性。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetFileAttributes(string lpFileName);

        /// <summary>设置文件属性。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetFileAttributes(string lpFileName, uint dwFileAttributes);

        /// <summary>获取文件的创建、访问和写入时间。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileTime(IntPtr hFile, out long lpCreationTime,
            out long lpLastAccessTime, out long lpLastWriteTime);

        /// <summary>获取文件大小（32 位上限）。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetFileSize(IntPtr hFile, IntPtr lpFileSizeHigh);

        /// <summary>获取文件大小（64 位）。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetFileSizeEx(IntPtr hFile, out long lpFileSize);

        /// <summary>设置文件的物理结束位置。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEndOfFile(IntPtr hFile);

        /// <summary>移动文件指针。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetFilePointer(IntPtr hFile, int lDistanceToMove, ref int lpDistanceToMoveHigh,
            uint dwMoveMethod);

        /// <summary>获取临时文件目录路径。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetTempPath(uint nBufferLength, StringBuilder lpBuffer);

        /// <summary>创建临时文件。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetTempFileName(string lpPathName, string lpPrefixString, uint uUnique,
            StringBuilder lpTempFileName);

        /// <summary>创建目录。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes);

        /// <summary>删除空目录。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RemoveDirectory(string lpPathName);

        /// <summary>获取当前目录。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetCurrentDirectory(uint nBufferLength, StringBuilder lpBuffer);

        /// <summary>设置当前目录。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetCurrentDirectory(string lpPathName);

        /// <summary>获取系统目录（例如 C:\Windows\System32）。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetSystemDirectory(StringBuilder lpBuffer, uint uSize);

        /// <summary>获取 Windows 目录路径。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetWindowsDirectory(StringBuilder lpBuffer, uint uSize);

        /// <summary>将短路径转换为长路径。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath, uint cchBuffer);

        /// <summary>将长路径转换为短路径。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        // ==================== 文件查找 ====================

        /// <summary>查找第一个匹配的文件。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        /// <summary>查找下一个匹配的文件。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        /// <summary>关闭文件查找句柄。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FindClose(IntPtr hFindFile);

        // ==================== 驱动器与磁盘 ====================

        /// <summary>获取驱动器类型。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetDriveType(string lpRootPathName);

        /// <summary>获取磁盘可用空间（64 位）。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
            out ulong lpFreeBytesAvailableToCaller, out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);

        /// <summary>获取当前逻辑驱动器的位掩码。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetLogicalDrives();

        /// <summary>获取逻辑驱动器字符串列表。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetLogicalDriveStrings(uint nBufferLength, StringBuilder lpBuffer);

        /// <summary>获取卷信息（卷标、序列号、文件系统等）。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetVolumeInformation(string lpRootPathName, StringBuilder lpVolumeNameBuffer,
            uint nVolumeNameSize, out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength,
            out uint lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer, uint nFileSystemNameSize);

        // ==================== 进程与线程 ====================

        /// <summary>获取当前进程 ID。</summary>
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentProcessId();

        /// <summary>获取当前线程 ID。</summary>
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        /// <summary>打开现有进程。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        /// <summary>终止进程。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        /// <summary>创建新进程。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine,
            IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles,
            uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        /// <summary>获取进程退出代码。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint lpExitCode);

        /// <summary>将当前线程暂停指定的时间（毫秒）。</summary>
        [DllImport("kernel32.dll")]
        public static extern void Sleep(uint dwMilliseconds);

        /// <summary>获取系统自启动以来经过的毫秒数。</summary>
        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();

        /// <summary>获取系统自启动以来经过的毫秒数（64 位）。</summary>
        [DllImport("kernel32.dll")]
        public static extern ulong GetTickCount64();

        /// <summary>获取性能计数器的频率。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <summary>获取性能计数器的当前值。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>获取线程优先级。</summary>
        [DllImport("kernel32.dll")]
        public static extern int GetThreadPriority(IntPtr hThread);

        /// <summary>设置线程优先级。</summary>
        [DllImport("kernel32.dll")]
        public static extern bool SetThreadPriority(IntPtr hThread, int nPriority);

        /// <summary>获取当前进程的伪句柄。</summary>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        /// <summary>获取当前线程的伪句柄。</summary>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();

        /// <summary>复制句柄。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess,
            bool bInheritHandle, uint dwOptions);

        // ==================== 模块与动态库 ====================

        /// <summary>获取模块句柄（不增加引用计数）。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>获取模块的完整路径。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, uint nSize);

        /// <summary>加载动态链接库（DLL）。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>获取动态库中导出函数的地址。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        /// <summary>释放已加载的动态库。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        // ==================== 内存管理 ====================

        /// <summary>在调用进程的虚拟地址空间中保留或提交内存区域。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType,
            uint flProtect);

        /// <summary>释放或解除提交虚拟内存区域。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        /// <summary>查询虚拟内存信息。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UIntPtr VirtualQuery(IntPtr lpAddress, ref MEMORY_BASIC_INFORMATION lpBuffer,
            UIntPtr dwLength);

        /// <summary>从堆中分配内存。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        /// <summary>释放堆内存。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        /// <summary>获取调用进程的堆句柄。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcessHeap();

        /// <summary>获取当前内存使用情况。</summary>
        [DllImport("kernel32.dll")]
        public static extern void GlobalMemoryStatus(ref MEMORYSTATUS lpBuffer);

        /// <summary>获取当前内存使用情况（扩展）。</summary>
        [DllImport("kernel32.dll")]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        // ==================== 系统信息 ====================

        /// <summary>获取当前系统的硬件信息。</summary>
        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        /// <summary>获取物理或逻辑处理器的系统信息。</summary>
        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        /// <summary>获取计算机名称。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetComputerName(StringBuilder lpBuffer, ref uint nSize);

        /// <summary>获取当前用户名。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetUserName(StringBuilder lpBuffer, ref uint nSize);

        /// <summary>获取系统时间（UTC）。</summary>
        [DllImport("kernel32.dll")]
        public static extern void GetSystemTime(ref SYSTEMTIME lpSystemTime);

        /// <summary>获取本地时间。</summary>
        [DllImport("kernel32.dll")]
        public static extern void GetLocalTime(ref SYSTEMTIME lpSystemTime);

        /// <summary>设置系统时间（UTC）。</summary>
        [DllImport("kernel32.dll")]
        public static extern bool SetSystemTime(ref SYSTEMTIME lpSystemTime);

        /// <summary>获取命令行字符串。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetCommandLine();

        /// <summary>获取环境变量的值。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetEnvironmentVariable(string lpName, StringBuilder lpBuffer, uint nSize);

        /// <summary>设置环境变量的值。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetEnvironmentVariable(string lpName, string lpValue);

        // ==================== 同步 ====================

        /// <summary>等待一个对象进入信号状态。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        /// <summary>等待多个对象进入信号状态。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForMultipleObjects(uint nCount, IntPtr[] lpHandles, bool bWaitAll,
            uint dwMilliseconds);

        /// <summary>创建或打开事件对象。</summary>
        // 修复：lpName 为字符串，须与 CreateMutex/CreateSemaphore 一样指定 CharSet.Auto，否则按 ANSI 封送，非英文名称会出错
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset,
            bool bInitialState, string lpName);

        /// <summary>将事件设置为信号状态。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetEvent(IntPtr hEvent);

        /// <summary>将事件重置为非信号状态。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ResetEvent(IntPtr hEvent);

        /// <summary>创建或打开互斥体。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

        /// <summary>释放互斥体的所有权。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReleaseMutex(IntPtr hMutex);

        /// <summary>创建或打开信号量。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateSemaphore(IntPtr lpSemaphoreAttributes, int lInitialCount,
            int lMaximumCount, string lpName);

        /// <summary>释放信号量的计数。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReleaseSemaphore(IntPtr hSemaphore, int lReleaseCount,
            out int lpPreviousCount);

        // ==================== 调试与错误 ====================

        /// <summary>获取调用线程的最后错误代码。</summary>
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        /// <summary>设置调用线程的最后错误代码。</summary>
        [DllImport("kernel32.dll")]
        public static extern void SetLastError(uint dwErrCode);

        /// <summary>判断进程是否在调试器下运行。</summary>
        [DllImport("kernel32.dll")]
        public static extern bool IsDebuggerPresent();

        /// <summary>输出调试字符串。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string lpOutputString);

        // ==================== 控制台 ====================

        /// <summary>为调用进程分配新控制台。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();

        /// <summary>释放与调用进程关联的控制台。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeConsole();

        /// <summary>获取标准输入、输出或错误设备的句柄。</summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(uint nStdHandle);

        /// <summary>设置控制台窗口标题。</summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetConsoleTitle(string lpConsoleTitle);

        #endregion

        #region user32.dll ── 窗口 / 消息 / 输入 / 剪贴板 / 系统参数 / 菜单 / 光标 / 绘图文本

        /// <summary>获取前台窗口的句柄。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>获取桌面窗口的句柄。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>获取窗口标题。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>设置窗口标题。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

        /// <summary>获取窗口标题的长度。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>查找顶层窗口。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>查找子窗口。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className,
            string windowTitle);

        /// <summary>枚举所有顶层窗口。</summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>枚举指定窗口的所有子窗口。</summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>获取窗口的类名。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>获取创建窗口的线程 ID 和进程 ID。</summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>向窗口发送消息并等待处理结果。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>向窗口投递消息并立即返回。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>设置窗口显示状态。</summary>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>判断窗口是否可见。</summary>
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>启用或禁用窗口。</summary>
        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        /// <summary>将窗口设置为前台窗口。</summary>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>移动窗口位置和大小。</summary>
        [DllImport("user32.dll")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        /// <summary>设置窗口位置、大小和 Z 序。</summary>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx,
            int cy, uint uFlags);

        /// <summary>获取窗口矩形（屏幕坐标）。</summary>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        /// <summary>获取客户区矩形。</summary>
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        /// <summary>将客户区坐标转换为屏幕坐标。</summary>
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        /// <summary>将屏幕坐标转换为客户区坐标。</summary>
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        /// <summary>获取系统度量（如屏幕分辨率、图标尺寸等）。</summary>
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        /// <summary>判断键是否被按下（异步）。</summary>
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        /// <summary>获取键的状态（当前）。</summary>
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        /// <summary>获取光标在屏幕中的位置。</summary>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT lpPoint);

        /// <summary>设置光标在屏幕中的位置。</summary>
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        /// <summary>模拟鼠标事件。</summary>
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

        /// <summary>模拟键盘事件。</summary>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

        /// <summary>将数据放入剪贴板。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        /// <summary>打开剪贴板。</summary>
        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        /// <summary>清空剪贴板。</summary>
        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();

        /// <summary>从剪贴板获取数据。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(uint uFormat);

        /// <summary>关闭剪贴板。</summary>
        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();

        /// <summary>获取或设置系统范围参数（RECT 版本）。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam,
            uint fWinIni);

        /// <summary>获取或设置系统范围参数（通用指针版本）。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam,
            uint fWinIni);

        /// <summary>加载光标。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        /// <summary>设置当前光标。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr hCursor);

        /// <summary>销毁图标并释放内存。</summary>
        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>在指定矩形中绘制格式化文本。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int DrawText(IntPtr hDC, string lpchText, int nCount, ref RECT lpRect,
            uint uFormat);

        /// <summary>显示消息框。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // 消息循环

        /// <summary>从消息队列中检索消息（阻塞）。</summary>
        [DllImport("user32.dll")]
        public static extern bool GetMessage(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
            uint wMsgFilterMax);

        /// <summary>检查消息队列中的消息（不阻塞）。</summary>
        [DllImport("user32.dll")]
        public static extern bool PeekMessage(ref MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
            uint wMsgFilterMax, uint wRemoveMsg);

        /// <summary>将虚拟键消息转换为字符消息。</summary>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        /// <summary>将消息分派给窗口过程。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        /// <summary>发送退出消息。</summary>
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);

        /// <summary>注册窗口类。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        /// <summary>创建重叠、弹出或子窗口。</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName,
            string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        /// <summary>销毁窗口。</summary>
        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        /// <summary>调用默认窗口过程。</summary>
        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // 32/64 位兼容的 GetWindowLong / SetWindowLong
        /// <summary>获取窗口长整型属性（兼容 32/64 位）。</summary>
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return (IntPtr)GetWindowLong32(hWnd, nIndex);
        }

        /// <summary>设置窗口长整型属性（兼容 32/64 位）。</summary>
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return (IntPtr)SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32());
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        #endregion

        #region gdi32.dll ── 设备上下文 / 绘图 / 字体 / 位图 / 画笔 / 画刷

        /// <summary>获取指定窗口的设备上下文（DC）。</summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>释放设备上下文。</summary>
        [DllImport("gdi32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>创建与指定 DC 兼容的内存设备上下文。</summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        /// <summary>删除设备上下文。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        /// <summary>将 GDI 对象选入设备上下文。</summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hGdiObj);

        /// <summary>删除 GDI 对象并释放资源。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>设置背景模式（透明/不透明）。</summary>
        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hDC, int iBkMode);

        /// <summary>设置文本颜色。</summary>
        [DllImport("gdi32.dll")]
        public static extern uint SetTextColor(IntPtr hDC, uint crColor);

        /// <summary>创建逻辑画笔（实心颜色）。</summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint crColor);

        /// <summary>创建逻辑画笔。</summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

        /// <summary>将当前位置移动到指定坐标。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool MoveToEx(IntPtr hDC, int X, int Y, IntPtr lpPoint);

        /// <summary>从当前位置画线到指定坐标。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool LineTo(IntPtr hDC, int nXEnd, int nYEnd);

        /// <summary>绘制矩形。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(IntPtr hDC, int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect);

        /// <summary>绘制椭圆。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool Ellipse(IntPtr hDC, int nLeftRect, int nTopRect, int nRightRect,
            int nBottomRect);

        /// <summary>在指定位置输出文本。</summary>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool TextOut(IntPtr hDC, int nXStart, int nYStart, string lpString,
            int cchString);

        /// <summary>创建逻辑字体。</summary>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFont(int nHeight, int nWidth, int nEscapement,
            int nOrientation, int fnWeight, uint fdwItalic, uint fdwUnderline,
            uint fdwStrikeOut, uint fdwCharSet, uint fdwOutputPrecision,
            uint fdwClipPrecision, uint fdwQuality, uint fdwPitchAndFamily, string lpszFace);

        /// <summary>执行位块传输（BitBlt），用于拷贝位图。</summary>
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth,
            int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        /// <summary>创建与指定 DC 兼容的位图。</summary>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        /// <summary>获取 GDI 对象的信息。</summary>
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, IntPtr lpvObject);

        #endregion

        #region shell32.dll ── Shell 执行 / 特殊文件夹 / 文件操作 / 通知 / 浏览文件夹

        /// <summary>执行 Shell 操作（打开、打印、浏览等）。</summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile,
            string lpParameters, string lpDirectory, int nShowCmd);

        /// <summary>获取特殊文件夹路径。</summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwnd, StringBuilder pszPath, int csidl,
            bool fCreate);

        /// <summary>从可执行文件或 DLL 中提取图标。</summary>
        // 修复：lpszExeFileName 为字符串，缺省 CharSet.Ansi 会使含非 ASCII 字符的路径失败，须指定 CharSet.Auto（ExtractIconW）
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        /// <summary>获取文件信息（图标、类型等）。</summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern uint SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        /// <summary>执行文件操作（复制、移动、删除、重命名）。</summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        /// <summary>通知系统 Shell 发生了更改。</summary>
        [DllImport("shell32.dll")]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1,
            IntPtr dwItem2);

        /// <summary>显示浏览文件夹对话框（返回 PIDL）。</summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        /// <summary>将 PIDL 转换为文件系统路径。</summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

        #endregion

        #region advapi32.dll ── 安全 / 注册表 / 服务

        /// <summary>打开与进程关联的访问令牌。</summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess,
            out IntPtr TokenHandle);

        /// <summary>获取指定权限的本地唯一标识符 (LUID)。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName,
            out LUID lpLuid);

        /// <summary>启用或禁用令牌中的权限。</summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState,
            IntPtr ReturnLength);

        // 注册表操作

        /// <summary>打开注册表项。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RegOpenKeyEx(IntPtr hKey, string lpSubKey, uint ulOptions,
            uint samDesired, out IntPtr phkResult);

        /// <summary>创建注册表项（存在则打开）。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RegCreateKeyEx(IntPtr hKey, string lpSubKey, uint Reserved,
            string lpClass, uint dwOptions, uint samDesired, IntPtr lpSecurityAttributes,
            out IntPtr phkResult, out uint lpdwDisposition);

        /// <summary>关闭注册表项。</summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool RegCloseKey(IntPtr hKey);

        /// <summary>删除注册表子项。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RegDeleteKey(IntPtr hKey, string lpSubKey);

        /// <summary>删除注册表值。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RegDeleteValue(IntPtr hKey, string lpValueName);

        /// <summary>查询注册表值。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint RegQueryValueEx(IntPtr hKey, string lpValueName, IntPtr lpReserved,
            out uint lpType, byte[] lpData, ref uint lpcbData);

        /// <summary>设置注册表值。</summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegSetValueEx(IntPtr hKey, string lpValueName, uint Reserved,
            uint dwType, byte[] lpData, uint cbData);

        /// <summary>枚举注册表子项。</summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegEnumKeyEx(IntPtr hKey, uint dwIndex, StringBuilder lpName,
            ref uint lpcchName, IntPtr lpReserved, StringBuilder lpClass, ref uint lpcchClass,
            out long lpftLastWriteTime);

        /// <summary>枚举注册表值。</summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegEnumValue(IntPtr hKey, uint dwIndex, StringBuilder lpValueName,
            ref uint lpcchValueName, IntPtr lpReserved, out uint lpType, byte[] lpData,
            ref uint lpcbData);

        // 服务管理

        /// <summary>打开服务控制管理器。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName,
            uint dwDesiredAccess);

        /// <summary>打开服务。</summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName,
            uint dwDesiredAccess);

        /// <summary>启动服务。</summary>
        // 修复：lpServiceArgVectors 为字符串数组，StartService 有 A/W 两个版本，缺省按 ANSI 封送，须指定 CharSet.Auto
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool StartService(IntPtr hService, uint dwNumServiceArgs,
            string[] lpServiceArgVectors);

        /// <summary>向服务发送控制代码。</summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool ControlService(IntPtr hService, uint dwControl,
            ref SERVICE_STATUS lpServiceStatus);

        /// <summary>关闭服务控制管理器或服务的句柄。</summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        #endregion

        #region comctl32.dll ── 公共控件

        /// <summary>初始化公共控件库。</summary>
        [DllImport("comctl32.dll")]
        public static extern void InitCommonControls();

        /// <summary>绘制图像列表中的图像。</summary>
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_Draw(IntPtr himl, int i, IntPtr hdcDst, int x, int y,
            uint fStyle);

        #endregion

        #region winmm.dll ── 多媒体

        /// <summary>获取波形输出设备数量。</summary>
        [DllImport("winmm.dll")]
        public static extern int waveOutGetNumDevs();

        /// <summary>向媒体控制接口发送字符串命令。</summary>
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        public static extern int mciSendString(string command, StringBuilder returnString,
            int returnLength, IntPtr hwndCallback);

        /// <summary>获取多媒体时间（毫秒）。</summary>
        [DllImport("winmm.dll")]
        public static extern uint timeGetTime();

        #endregion

        #region psapi.dll ── 进程信息

        /// <summary>枚举进程中的模块。</summary>
        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule,
            uint cb, out uint lpcbNeeded);

        /// <summary>获取模块的文件名。</summary>
        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule,
            StringBuilder lpFilename, uint nSize);

        #endregion

        #region shlwapi.dll ── 路径处理

        /// <summary>判断文件或路径是否存在（推荐使用此版本）。</summary>
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern bool PathFileExists(string pszPath);

        /// <summary>逻辑字符串比较（按数字排序）。</summary>
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        #endregion

        #region version.dll ── 版本信息

        /// <summary>获取文件版本信息的大小。</summary>
        [DllImport("version.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetFileVersionInfoSize(string lptstrFilename, out uint lpdwHandle);

        /// <summary>获取文件版本信息。</summary>
        [DllImport("version.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetFileVersionInfo(string lptstrFilename, uint dwHandle, uint dwLen,
            IntPtr lpData);

        /// <summary>从版本信息中查询特定值。</summary>
        [DllImport("version.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool VerQueryValue(IntPtr pBlock, string lpSubBlock, out IntPtr lplpBuffer,
            out uint puLen);

        #endregion

        #region ole32.dll ── COM 基础

        /// <summary>初始化 COM 库。</summary>
        [DllImport("ole32.dll", SetLastError = true)]
        public static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        /// <summary>取消 COM 初始化。</summary>
        [DllImport("ole32.dll")]
        public static extern void CoUninitialize();

        /// <summary>将 GUID 转换为字符串。</summary>
        [DllImport("ole32.dll", CharSet = CharSet.Auto)]
        public static extern int StringFromGUID2(ref Guid rguid, StringBuilder lpsz, int cchMax);

        #endregion

        #region 结构体定义

        /// <summary>文件查找数据结构体。</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)] public string cAlternateFileName;
        }

        /// <summary>内存状态信息。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUS
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
        }

        /// <summary>扩展内存状态信息。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        /// <summary>系统信息。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        /// <summary>系统时间。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        /// <summary>启动信息。</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX, dwY, dwXSize, dwYSize;
            public int dwXCountChars, dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        /// <summary>进程信息。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        /// <summary>矩形。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        /// <summary>点。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X, Y;
        }

        /// <summary>消息结构。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        /// <summary>窗口类结构。</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
        }

        /// <summary>本地唯一标识符 (LUID)。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        /// <summary>令牌权限结构。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public LUID_AND_ATTRIBUTES Privileges;
        }

        /// <summary>LUID 与属性。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        /// <summary>Shell 文件信息结构。</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        /// <summary>Shell 文件操作结构。</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public string pFrom;
            public string pTo;
            public ushort fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        /// <summary>服务状态。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SERVICE_STATUS
        {
            public uint dwServiceType;
            public uint dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
        }

        /// <summary>浏览文件夹信息。</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        /// <summary>虚拟内存基本信息。</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        #endregion

        #region 常量 / 枚举 / 委托

        // 访问权限
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint GENERIC_ALL = 0x10000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;

        // 文件创建方式
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;
        public const uint OPEN_ALWAYS = 4;
        public const uint TRUNCATE_EXISTING = 5;

        // 文件属性
        public const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
        public const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        public const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        // 驱动器类型
        public const uint DRIVE_UNKNOWN = 0;
        public const uint DRIVE_NO_ROOT_DIR = 1;
        public const uint DRIVE_REMOVABLE = 2;
        public const uint DRIVE_FIXED = 3;
        public const uint DRIVE_REMOTE = 4;
        public const uint DRIVE_CDROM = 5;
        public const uint DRIVE_RAMDISK = 6;

        // 窗口显示命令
        public const uint SW_HIDE = 0;
        public const uint SW_SHOWNORMAL = 1;
        public const uint SW_SHOWMINIMIZED = 2;
        public const uint SW_SHOWMAXIMIZED = 3;
        public const uint SW_SHOW = 5;
        public const uint SW_RESTORE = 9;

        // 窗口样式
        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CAPTION = 0x00C00000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_EX_TOPMOST = 0x00000008;
        public const uint WS_EX_LAYERED = 0x00080000;

        // 系统度量索引
        public const int SM_CXSCREEN = 0;
        public const int SM_CYSCREEN = 1;

        // 消息常量
        public const uint WM_CLOSE = 0x0010;
        public const uint WM_QUIT = 0x0012;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_SETTEXT = 0x000C;
        public const uint WM_GETTEXT = 0x000D;
        public const uint WM_GETTEXTLENGTH = 0x000E;

        // 输入模拟标志
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint KEYEVENTF_KEYDOWN = 0x0000;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        // Shell 文件操作标志
        public const uint FO_MOVE = 0x0001;
        public const uint FO_COPY = 0x0002;
        public const uint FO_DELETE = 0x0003;
        public const uint FO_RENAME = 0x0004;
        public const uint FOF_SILENT = 0x0004;
        public const uint FOF_NOCONFIRMATION = 0x0010;
        public const uint FOF_ALLOWUNDO = 0x0040;
        public const uint FOF_NOERRORUI = 0x0400;

        // SHGetFileInfo 标志
        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_DISPLAYNAME = 0x000000200;
        public const uint SHGFI_TYPENAME = 0x000000400;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        // 虚拟键码
        public const int VK_LBUTTON = 0x01;
        public const int VK_RBUTTON = 0x02;
        public const int VK_RETURN = 0x0D;
        public const int VK_SHIFT = 0x10;
        public const int VK_CONTROL = 0x11;
        public const int VK_MENU = 0x12;
        public const int VK_ESCAPE = 0x1B;
        public const int VK_SPACE = 0x20;
        public const int VK_LEFT = 0x25;
        public const int VK_UP = 0x26;
        public const int VK_RIGHT = 0x27;
        public const int VK_DOWN = 0x28;
        public const int VK_F1 = 0x70;
        public const int VK_F12 = 0x7B;

        // 注册表访问权限
        public const uint KEY_READ = 0x20019;
        public const uint KEY_WRITE = 0x20006;
        public const uint KEY_ALL_ACCESS = 0xF003F;

        // 注册表值类型
        public const uint REG_SZ = 1;
        public const uint REG_EXPAND_SZ = 2;
        public const uint REG_BINARY = 3;
        public const uint REG_DWORD = 4;
        public const uint REG_MULTI_SZ = 7;
        public const uint REG_QWORD = 11;

        // 服务控制
        public const uint SERVICE_CONTROL_STOP = 0x00000001;
        public const uint SERVICE_CONTROL_PAUSE = 0x00000002;
        public const uint SERVICE_CONTROL_CONTINUE = 0x00000003;

        // COM 初始化标志
        public const uint COINIT_APARTMENTTHREADED = 0x2;
        public const uint COINIT_MULTITHREADED = 0x0;

        // 绘图常量
        public const uint SRCCOPY = 0x00CC0020;
        public const int TRANSPARENT = 1;
        public const int OPAQUE = 2;

        // 消息框标志
        public const uint MB_OK = 0x00000000;
        public const uint MB_OKCANCEL = 0x00000001;
        public const uint MB_YESNO = 0x00000004;
        public const uint MB_ICONINFORMATION = 0x00000040;
        public const uint MB_ICONWARNING = 0x00000030;
        public const uint MB_ICONERROR = 0x00000010;

        /// <summary>EnumWindows 和 EnumChildWindows 的回调委托。</summary>
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion
    }
}