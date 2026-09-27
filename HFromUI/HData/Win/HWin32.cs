using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HData.Win
{
    public class HWin32
    {
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_LAYERED = 0x00080000;

        /// <summary>
        /// Struct Size
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Size
        {
            /// <summary>
            /// The cx
            /// </summary>
            public Int32 cx;

            /// <summary>
            /// The cy
            /// </summary>
            public Int32 cy;

            /// <summary>
            /// Initializes a new instance of the <see cref="Size"/> struct.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            public Size(Int32 x, Int32 y)
            {
                cx = x;
                cy = y;
            }
        }

        /// <summary>
        /// Struct BLENDFUNCTION
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            /// <summary>
            /// The blend op
            /// </summary>
            public byte BlendOp;

            /// <summary>
            /// The blend flags
            /// </summary>
            public byte BlendFlags;

            /// <summary>
            /// The source constant alpha
            /// </summary>
            public byte SourceConstantAlpha;

            /// <summary>
            /// The alpha format
            /// </summary>
            public byte AlphaFormat;
        }

        /// <summary>
        /// Struct Point
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            /// <summary>
            /// The x
            /// </summary>
            public Int32 x;

            /// <summary>
            /// The y
            /// </summary>
            public Int32 y;

            /// <summary>
            /// Initializes a new instance of the <see cref="Point"/> struct.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            public Point(Int32 x, Int32 y)
            {
                this.x = x;
                this.y = y;
            }
        }

        /// <summary>
        /// The ac source over
        /// </summary>
        public const byte AC_SRC_OVER = 0;

        /// <summary>
        /// The ulw alpha
        /// </summary>
        public const Int32 ULW_ALPHA = 2;

        /// <summary>
        /// The ac source alpha
        /// </summary>
        public const byte AC_SRC_ALPHA = 1;

        /// <summary>
        /// Creates the compatible dc.
        /// </summary>
        /// <param name="hDC">The h dc.</param>
        /// <returns>IntPtr.</returns>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        /// <summary>
        /// Gets the dc.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <returns>IntPtr.</returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        /// <summary>
        /// Selects the object.
        /// </summary>
        /// <param name="hDC">The h dc.</param>
        /// <param name="hObj">The h object.</param>
        /// <returns>IntPtr.</returns>
        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

        /// <summary>
        /// Releases the dc.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="hDC">The h dc.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Deletes the dc.
        /// </summary>
        /// <param name="hDC">The h dc.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int DeleteDC(IntPtr hDC);

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="hObj">The h object.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int DeleteObject(IntPtr hObj);

        /// <summary>
        /// Updates the layered window.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="hdcDst">The HDC DST.</param>
        /// <param name="pptDst">The PPT DST.</param>
        /// <param name="psize">The psize.</param>
        /// <param name="hdcSrc">The HDC source.</param>
        /// <param name="pptSrc">The PPT source.</param>
        /// <param name="crKey">The cr key.</param>
        /// <param name="pblend">The pblend.</param>
        /// <param name="dwFlags">The dw flags.</param>
        /// <returns>System.Int32.</returns>
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

        /// <summary>
        /// Exts the create region.
        /// </summary>
        /// <param name="lpXform">The lp xform.</param>
        /// <param name="nCount">The n count.</param>
        /// <param name="rgnData">The RGN data.</param>
        /// <returns>IntPtr.</returns>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr ExtCreateRegion(IntPtr lpXform, uint nCount, IntPtr rgnData);

        #region API调用

        //常量
        public const int WM_SYSCOMMAND = 0x0112;

        //窗体移动
        public const int SC_MOVE = 0xF010;

        public const int HTCAPTION = 0x0002;

        //改变窗体大小
        public const int WMSZ_LEFT = 0xF001;

        public const int WMSZ_RIGHT = 0xF002;
        public const int WMSZ_TOP = 0xF003;
        public const int WMSZ_TOPLEFT = 0xF004;
        public const int WMSZ_TOPRIGHT = 0xF005;
        public const int WMSZ_BOTTOM = 0xF006;
        public const int WMSZ_BOTTOMLEFT = 0xF007;
        public const int WMSZ_BOTTOMRIGHT = 0xF008;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_RESTORE = 0xF120;

        public const int WS_MINIMIZEBOX = 0x00020000;//允许窗体最小化
        public const int WS_SYSMENU = 0x00080000;//系统菜单

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();                               //用来释放被当前线程中某个窗口捕获的光标

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwdn, int wMsg, int mParam, int lParam); //向指定的窗体发送Windows消息

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public const Int32 AW_HOR_POSITIVE = 0x00000001;//自左向右显示窗口，可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略
        public const Int32 AW_HOR_NEGATIVE = 0x00000002;//自右向左显示窗口，当使用了 AW_CENTER标志时被忽略
        public const Int32 AW_VER_POSITIVE = 0x00000004;//自顶向下显示窗口，可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略
        public const Int32 AW_VER_NEGATIVE = 0x00000008;//自下向上显示窗口。该标志可以在滚动动画和滑动动画中使用。当使用AW_CENTER标志时，该标志将被忽略
        public const Int32 AW_CENTER = 0x00000010;//若使用了AW_HIDE标志，则使窗口向内重叠；若未使用AW_HIDE标志，则使窗口向外扩展
        public const Int32 AW_HIDE = 0x00010000;//隐藏窗口，默认显示窗口
        public const Int32 AW_ACTIVATE = 0x00020000;//激活窗口，在使用了AW_HIDE标志后不要使用这个标志
        public const Int32 AW_SLIDE = 0x00040000;//使用滑动类型，默认为滚动动画类型。当使用AW_CENTER标志时，这个标志就被忽略
        public const Int32 AW_BLEND = 0x00080000;//使用淡入效果，只有当hWnd为顶层窗口的时候才可以使用此标志

        [DllImport("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);//重写系统api，窗口动画效果

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern int GetWindowLong(HandleRef hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern int SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool SetParent(IntPtr hwndChild, IntPtr hwndNewParent);

        /// <summary>
        /// 是获取当前系统中被激活的窗口.
        /// </summary>
        /// <returns>IntPtr.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        public const int WM_FONTCHANGE = 0x001D;
        public const int HWND_BROADCAST = 0xffff;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int WriteProfileString(string lpszSection, string lpszKeyName, string lpszString);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd,// handle to destination window
        uint Msg,   // message
        int wParam, // first message parameter
        int lParam  // second message parameter
        );

        [DllImport("gdi32")]
        public static extern int AddFontResource(string lpFileName);

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }

        // 用于设置系统时间
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        // 用于获得系统时间
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);

        // 用于设置系统时间
        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime sysTime);

        // 用于获得系统时间
        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTime(ref SystemTime sysTime);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        public const int CS_DROPSHADOW = 0x20000;

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        #endregion API调用

        #region 圆角设置api

        [DllImport("gdi32.dll")]
        public static extern int CreateRoundRectRgn(int x1, int y1, int x2, int y2, int x3, int y3);

        [DllImport("user32.dll")]
        public static extern int SetWindowRgn(IntPtr hwnd, int hRgn, Boolean bRedraw);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject", CharSet = CharSet.Ansi)]
        public static extern int DeleteObject(int hObject);

        /// <summary>设置 formRoundRectRgn。</summary>
        public static void SetFormRoundRectRgn(Form form, int rgnRadius)
        {
            // 创建圆角区域，失败返回 0
            int hRgn = CreateRoundRectRgn(0, 0, form.Width + 1, form.Height + 1, rgnRadius, rgnRadius);
            if (hRgn == 0) return;
            // 修复：SetWindowRgn 成功后区域句柄归系统所有（系统不会复制区域），
            // 此时绝不能 DeleteObject，否则窗口区域失效；仅在调用失败时才自行删除，避免 GDI 句柄泄漏
            if (SetWindowRgn(form.Handle, hRgn, true) == 0)
                DeleteObject(hRgn);
        }

        #endregion 圆角设置api

        #region 鼠标操作

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        //移动鼠标
        public static int MOUSEEVENTF_MOVE = 0x0001;

        //模拟鼠标左键按下
        public static int MOUSEEVENTF_LEFTDOWN = 0x0002;

        //模拟鼠标左键抬起
        public static int MOUSEEVENTF_LEFTUP = 0x0004;

        //模拟鼠标右键按下
        public static int MOUSEEVENTF_RIGHTDOWN = 0x0008;

        //模拟鼠标右键抬起
        public static int MOUSEEVENTF_RIGHTUP = 0x0010;

        //模拟鼠标中键按下
        public static int MOUSEEVENTF_MIDDLEDOWN = 0x0020;

        //模拟鼠标中键抬起
        public static int MOUSEEVENTF_MIDDLEUP = 0x0040;

        //标示是否采用绝对坐标
        public static int MOUSEEVENTF_ABSOLUTE = 0x8000;

        #endregion 鼠标操作

        #region 窗口操作

        /// <summary>
        /// 鼠标移动窗体，在mousedown事件中使用
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowMove(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，左侧
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeLeft(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_LEFT, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，右侧
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeRight(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_RIGHT, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，上侧
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeTop(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_TOP, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，下侧
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeBottom(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_BOTTOM, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，左上
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeTopLeft(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_TOPLEFT, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，右上
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeTopRight(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_TOPRIGHT, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，左下
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeBottomLeft(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_BOTTOMLEFT, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 鼠标拖动修改窗体大小，右下
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowSizeBottomRight(IntPtr hwnd)
        {
            ReleaseCapture();//用来释放被当前线程中某个窗口捕获的光标
            SendMessage(hwnd, WM_SYSCOMMAND, WMSZ_BOTTOMRIGHT, 0);//向Windows发送拖动窗体的消息
        }

        /// <summary>
        /// 设置无边框窗体可以点击任务栏图标最小化和还原
        /// </summary>
        /// <param name="form"></param>
        public static void WindowTaskBarMinimsizeAndRestore(Form form)
        {
            int windowLong = (GetWindowLong(new HandleRef(form, form.Handle), -16));
            SetWindowLong(new HandleRef(form, form.Handle), -16, windowLong | WS_SYSMENU | WS_MINIMIZEBOX);
        }

        #endregion 窗口操作

        #region 滚动条

        [StructLayout(LayoutKind.Sequential)]
        public struct TagScrollinfo
        {
            /// <summary>
            /// 标识结构体大小
            /// </summary>
            public uint cbSize;

            /// <summary>
            /// 说明待找回的滚动条参数（通俗点说：我们需要获取滚动条什么参数就在这里按要求设置哪个值），以下为它可设置的值：
            ///SIF_PAGE：获取SCROLLINFO结构中的nPage成员
            ///SIF_POS：获取SCROLLINFO结构中的nPos成员
            ///SIF_RANGE：获取SCROLLINFO结构中的nMin和nMax成员
            ///SIF_TRACKPOS：获取SCROLLINFO 结构中的nPage成员
            /// </summary>
            public uint fMask;

            /// <summary>
            /// 滚动条最小值
            /// </summary>
            public int nMin;

            /// <summary>
            /// 滚动条最大值
            /// </summary>
            public int nMax;

            /// <summary>
            /// 页尺寸，确定比例滚动条滑块的大小（编辑框中一页可显示的行数（不足一页时为当前页中的行数））
            /// </summary>
            public uint nPage;

            /// <summary>
            /// 滚动条滑块的位置
            /// </summary>
            public int nPos;

            /// <summary>
            /// 拖动时滚动条滑块的位置（已经滚动的行数）
            /// </summary>
            public int nTrackPos;
        }

        public enum fnBar
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2
        }

        public enum fMask
        {
            SIF_ALL,
            SIF_DISABLENOSCROLL = 0X0010,
            SIF_PAGE = 0X0002,
            SIF_POS = 0X0004,
            SIF_RANGE = 0X0001,
            SIF_TRACKPOS = 0X0008
        }

        /// <summary>MakeLong 方法。</summary>
        public static int MakeLong(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }

        public const int SB_THUMBTRACK = 5;
        public const int WM_HSCROLL = 0x114;
        public const int WM_VSCROLL = 0x115;

        [DllImport("user32.dll", EntryPoint = "GetScrollInfo")]
        public static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref Scrollinfo lpsi);

        [DllImport("user32.dll", EntryPoint = "SetScrollInfo")]
        public static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref Scrollinfo lpsi, bool fRedraw);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll", SetLastError = true)]
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, long wParam, int lParam);

        public struct Scrollinfo
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        public enum ScrollInfoMask
        {
            SIF_RANGE = 0x1,
            SIF_PAGE = 0x2,
            SIF_POS = 0x4,
            SIF_DISABLENOSCROLL = 0x8,
            SIF_TRACKPOS = 0x10,
            SIF_ALL = SIF_RANGE + SIF_PAGE + SIF_POS + SIF_TRACKPOS
        }

        public enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        #endregion 滚动条

        #region 注册全局快捷键

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(
                        IntPtr hWnd,                //要定义热键的窗口的句柄
            int id,                     //定义热键ID（不能与其它ID重复）
            KeyModifiers fsModifiers,   //标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效
            Keys vk                     //定义热键的内容
            );

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,                //要取消热键的窗口的句柄
            int id                      //要取消热键的ID
            );

        //定义了辅助键的名称（将数字转变为字符以便于记忆，也可去除此枚举而直接使用数值）
        [Flags()]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }

        private const int WM_HOTKEY = 0x0312;

        //protected override void OnActivated(EventArgs e)
        //{
        //    RegisterHotKey(this.Handle, 100, KeyModifiers.None, Keys.Right);
        //    base.OnActivated(e);

        //}

        //protected override void OnDeactivate(EventArgs e)
        //{
        //    base.OnDeactivate(e);
        //    UnregisterHotKey(this.Handle, 100);
        //}

        //protected override void WndProc(ref Message m)
        //{
        //    switch (m.Msg)
        //    {
        //        case WM_HOTKEY:
        //            switch (m.WParam.ToInt32())
        //            {
        //                case 100:
        //                    //do something
        //                    break;

        // } break; }

        //    base.WndProc(ref m);
        //}

        #endregion 注册全局快捷键
    }
}
