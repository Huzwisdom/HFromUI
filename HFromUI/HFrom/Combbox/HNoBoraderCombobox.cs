using HFromUI.HAttribute;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HColor;
    internal partial class HNoBoraderCombobox : ComboBox
    {
        /// <summary>arrowImage 字段。</summary>
        private Image arrowImage = HPhoto.Get("down");
        /// <summary>imageSize 字段。</summary>
        private Size imageSize = new Size(13, 10);
        /// <summary>imageOffset 字段。</summary>
        private Point imageOffset = new Point(0, 0);

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("箭头图片"), HDescriptionLanguage("箭头图片"), Browsable(true)]
        public Image ArrowImage
        {
            get { return arrowImage; }
            set
            {
                arrowImage = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图片大小"), HDescriptionLanguage("图片大小"), Browsable(true)]
        public Size ImageSize
        {
            get { return imageSize; }
            set
            {
                imageSize = value;
            }
        }

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("图片位置"), HDescriptionLanguage("图片位置"), Browsable(true)]
        public Point ImageOffset
        {
            get { return imageOffset; }
            set
            {
                imageOffset = value;
            }
        }

        public HNoBoraderCombobox()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.SetStyle(ControlStyles.DoubleBuffer, true);
            //this.SetStyle(ControlStyles.ResizeRedraw, true);
            //this.SetStyle(ControlStyles.Selectable, true);
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            //this.UpdateStyles();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);//返回hWnd参数所指定的窗口的设备环境。

        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); //函数释放设备上下文环境（DC）

        protected override CreateParams CreateParams
        {
            get
            {
                if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                {
                    return null;
                }
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.WndProc(ref m);
            //WM_PAINT = 0xf; 要求一个窗口重画自己,即Paint事件时
            //WM_CTLCOLOREDIT = 0x133;当一个编辑型控件将要被绘制时发送此消息给它的父窗口；
            //通过响应这条消息，所有者窗口可以通过使用给定的相关显示设备的句柄来设置编辑框的文本和背景颜色
            //windows消息值表,可参考:
            if (m.Msg == 0xf || m.Msg == 0x133)
            {
                IntPtr hDC = GetWindowDC(m.HWnd);
                if (hDC.ToInt32() == 0) //如果取设备上下文失败则返回
                {
                    ReleaseDC(m.HWnd, hDC);

                    return;
                }

                //建立Graphics对像
                Graphics g = Graphics.FromHdc(hDC);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(BackColor);
                //画边框的
                ControlPaint.DrawBorder(g, new Rectangle(0, 0, Width, Height), BackColor, ButtonBorderStyle.Solid);

                Bitmap bmp = new Bitmap(this.Width, this.Height);
                Graphics bg = Graphics.FromImage(bmp);
                bg.SmoothingMode = SmoothingMode.AntiAlias;
                bg.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                try
                {
                    if (base.SelectedIndex != -1)
                    {
                        bg.DrawString((string)this.SelectedItem, this.Font, new SolidBrush(base.ForeColor), new Point(0, (Height - this.Font.Height) / 2));
                    }
                }
                catch
                {
                }

                //画坚线
                //ControlPaint.DrawBorder(g, new Rectangle(Width - Height, 0, Height, Height), BackColor, ButtonBorderStyle.Solid);
                //Point[] pp = new Point[] { new Point(Width - 9, Height / 2 - 3), new Point(Width-5, Height / 2 + 3), new Point(Width-1, Height / 2 - 3) };

                //g.DrawPolygon(new Pen(Color.Black, 1), pp);
                try
                {
                    if (arrowImage != null)
                    {
                        bg.DrawImage(arrowImage, new Rectangle(new Point(Width - imageSize.Width + imageOffset.X, (Height - imageSize.Height) / 2 + imageOffset.Y), imageSize));
                    }
                }
                catch
                {
                }

                g.DrawImage(bmp, 0, 0);
                bg.Dispose();
                bmp.Dispose();

                //ControlPaint.DrawComboButton(g, new Rectangle(Width - Height, 0, Height, Height), ButtonState.Flat);
                //g.DrawLine(new Pen(Brushes.Blue, 2), new PointF(this.Width - this.Height, 0), new PointF(this.Width - this.Height, this.Height));
                //释放DC
                ReleaseDC(m.HWnd, hDC);
            }
        }
    }
}