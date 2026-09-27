using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HData.Win
{
    public class HIme
    {
        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern bool ImmSetCandidateWindow(IntPtr hImc, ref CANDIDATEFORM form);

        [DllImport("imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hImc);

        [StructLayout(LayoutKind.Sequential)]
        private struct CANDIDATEFORM
        {
            public int dwIndex;
            public int dwStyle;
            public Point ptCurrentPos;
            public Rectangle rcArea;
        }

        private const int CFS_CANDIDATEPOS = 0x0040;
        private const int CFS_EXCLUDE = 0x0080; // 也可以排除某区域

        /// <summary>
        /// 将候选窗口设置到控件客户区的指定位置
        /// </summary>
        /// <param name="ctrl">接收输入的控件</param>
        /// <param name="clientPoint">相对于控件的客户区坐标</param>
        public static void SetCandidatePosition(Control ctrl, Point clientPoint)
        {
            if (ctrl.IsDisposed) return;
            IntPtr hWnd = ctrl.Handle;
            IntPtr hImc = ImmGetContext(hWnd);
            if (hImc == IntPtr.Zero) return;

            var form = new CANDIDATEFORM
            {
                dwIndex = 0,
                dwStyle = CFS_CANDIDATEPOS,
                ptCurrentPos = clientPoint
            };

            ImmSetCandidateWindow(hImc, ref form);
            ImmReleaseContext(hWnd, hImc);
        }
    }
}
