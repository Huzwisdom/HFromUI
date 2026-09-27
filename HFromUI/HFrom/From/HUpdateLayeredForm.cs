using HFromUI.HData;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HLangage;
    using HFromUI.HData.Win;
    public partial class HUpdateLayeredForm : Form
    {
        /// <summary>IsBackCenter 成员。</summary>
        public bool IsBackCenter { get; set; } = true;
        /// <summary>BackImageOffset 成员。</summary>
        public Point BackImageOffset { get; set; } = new Point(0, 0);
        /// <summary>BackImage 成员。</summary>
        public Bitmap BackImage { get; set; } = null;

        /// <summary>Radius 成员。</summary>
        public int Radius { get; set; } = 10;

        public HUpdateLayeredForm()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000;  //  WS_EX_LAYERED 扩展样式
                return cp;
            }
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Init();
        }

        /// <summary>初始化。</summary>
        public void Init()
        {
            Bitmap bitmap = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            if (BackImage != null)
            {
                Rectangle rectangle = new Rectangle(0, 0, BackImage.Width, BackImage.Height);
                if (IsBackCenter)
                {
                    rectangle = Rectangle.Round(HDrawPaint.GetCenterRectInRect(new RectangleF(0, 0, this.Width, this.Height), BackImage.Size));
                }
                rectangle.Offset(BackImageOffset);

                g.DrawImage(BackImage, rectangle);
            }
            else
            {
                GraphicsPath path = HDrawPaint.CreatePath(new Rectangle(0, 0, this.Width, this.Height), Radius);
                Brush brush = new SolidBrush(this.BackColor);
                g.FillPath(brush, path);
                path.Dispose();
                brush.Dispose();
            }
            g.Dispose();
            SetBits(bitmap);
            bitmap.Dispose();
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            HWin32.WindowMove(this.Handle);
        }

        /// <summary>设置 bits。</summary>
        public void SetBits(Bitmap bitmap)
        {
            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException(HTranslation.GetContent("图片必须是32位带Alhpa通道的图片。"));
            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = HWin32.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = HWin32.CreateCompatibleDC(screenDC);

            try
            {
                HWin32.Point topLoc = new HWin32.Point(Left, Top);
                HWin32.Size bitMapSize = new HWin32.Size(Width, Height);
                HWin32.BLENDFUNCTION blendFunc = new HWin32.BLENDFUNCTION();
                HWin32.Point srcLoc = new HWin32.Point(0, 0);

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = HWin32.SelectObject(memDc, hBitmap);

                blendFunc.BlendOp = HWin32.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = Byte.Parse("255");
                blendFunc.AlphaFormat = HWin32.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;

                HWin32.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc, HWin32.ULW_ALPHA);

                bitmap.Dispose();
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    HWin32.SelectObject(memDc, oldBits);
                    HWin32.DeleteObject(hBitmap);
                }
                HWin32.ReleaseDC(IntPtr.Zero, screenDC);
                HWin32.DeleteDC(memDc);
            }
        }
    }
}
