using HFromUI.HData;
using HFromUI.HEnum;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Tips
{
    using HFromUI.HLangage;
    using HFromUI.HData.Win;
    using Message = System.Windows.Forms.Message;
    /// <summary>
    /// Class FrmAnchorTips. Implements the <see cref="System.Windows.Forms.Form"/>
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class HAnchorTips : Form
    {
        /// <summary>
        /// The m string MSG
        /// </summary>
        private string m_strMsg = string.Empty;

        /// <summary>
        /// Gets or sets the string MSG.
        /// </summary>
        /// <value>The string MSG.</value>
        public string StrMsg
        {
            get { return m_strMsg; }
            set
            {
                m_strMsg = value;
                if (string.IsNullOrEmpty(value))
                    return;
                ResetForm(value);
            }
        }

        /// <summary>
        /// The have handle
        /// </summary>
        private bool haveHandle = false;

        /// <summary>
        /// The m rect control
        /// </summary>
        private Rectangle m_rectControl;

        /// <summary>
        /// Gets or sets the rect control.
        /// </summary>
        /// <value>The rect control.</value>
        public Rectangle RectControl
        {
            get { return m_rectControl; }
            set
            {
                m_rectControl = value;
            }
        }

        /// <summary>
        /// The m location
        /// </summary>
        private AnchorTipsLocation m_location;

        /// <summary>
        /// The m background
        /// </summary>
        private Color? m_background = null;

        /// <summary>
        /// The m fore color
        /// </summary>
        private Color? m_foreColor = null;

        /// <summary>
        /// The m font name
        /// </summary>
        private string m_fontName = HTranslation.GetContent("微软雅黑");

        /// <summary>
        /// The m font size
        /// </summary>
        private int m_fontSize = 10;

        /// <summary>TriangleSize 字段。</summary>
        private int TriangleSize = 10;

        #region 构造函数    English:Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HAnchorTips"/> class.
        /// </summary>
        /// <param name="rectControl">The rect control.</param>
        /// <param name="strMsg">The string MSG.</param>
        /// <param name="location">The location.</param>
        /// <param name="background">The background.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="autoCloseTime">The automatic close time.</param>
        private HAnchorTips(Rectangle rectControl,string strMsg,string fontName,int fontSize,AnchorTipsLocation location = AnchorTipsLocation.RIGHT,Color? background = null,Color? foreColor = null,int autoCloseTime = 5000)
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            m_rectControl = rectControl;
            m_location = location;
            m_background = background;
            m_foreColor = foreColor;
            m_fontName = fontName;
            m_fontSize = fontSize;
            StrMsg = strMsg;
            if (autoCloseTime > 0)
            {
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = autoCloseTime;
                t.Tick += (a, b) =>
                {
                    this.Close();
                };
                t.Enabled = true;
            }
        }

        /// <summary>
        /// Resets the form.
        /// </summary>
        /// <param name="strMsg">The string MSG.</param>
        private void ResetForm(string strMsg)
        {
            Graphics g = this.CreateGraphics();
            Font _font = new Font(m_fontName, m_fontSize);
            Color _background = m_background == null ? Color.FromArgb(255, 77, 58) : m_background.Value;
            Color _foreColor = m_foreColor == null ? Color.White : m_foreColor.Value;
            System.Drawing.SizeF sizeText = g.MeasureString(strMsg, _font);
            g.Dispose();
            var formSize = new Size((int)sizeText.Width + 15, (int)sizeText.Height + 10);
            if (formSize.Width < 15)
                formSize.Width = 15;
            if (formSize.Height < 10)
                formSize.Height = 10;
            if (m_location == AnchorTipsLocation.LEFT || m_location == AnchorTipsLocation.RIGHT)
            {
                formSize.Width += TriangleSize;
            }
            else
            {
                formSize.Height += TriangleSize;
            }

            #region 获取窗体path    English:Get the form path

            GraphicsPath path = new GraphicsPath();
            Rectangle rect;
            switch (m_location)
            {
                case AnchorTipsLocation.TOP:
                    rect = new Rectangle(1, 1, formSize.Width - 2, formSize.Height - TriangleSize - 1);
                    this.Location = new Point(m_rectControl.X + (m_rectControl.Width - rect.Width) / 2, m_rectControl.Y - rect.Height - TriangleSize);
                    break;

                case AnchorTipsLocation.RIGHT:
                    rect = new Rectangle(TriangleSize, 1, formSize.Width - TriangleSize - 1, formSize.Height - 2);
                    this.Location = new Point(m_rectControl.Right, m_rectControl.Y + (m_rectControl.Height - rect.Height) / 2);
                    break;

                case AnchorTipsLocation.BOTTOM:
                    rect = new Rectangle(1, TriangleSize, formSize.Width - 2, formSize.Height - TriangleSize - 1);
                    this.Location = new Point(m_rectControl.X + (m_rectControl.Width - rect.Width) / 2, m_rectControl.Bottom);
                    break;

                default:
                    rect = new Rectangle(1, 1, formSize.Width - TriangleSize - 1, formSize.Height - 2);
                    this.Location = new Point(m_rectControl.X - rect.Width - TriangleSize, m_rectControl.Y + (m_rectControl.Height - rect.Height) / 2);
                    break;
            }
            int cornerRadius = 2;

            path.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);//左上角

            #region 上边

            if (m_location == AnchorTipsLocation.BOTTOM)
            {
                path.AddLine(rect.X + cornerRadius, rect.Y, rect.Left + rect.Width / 2 - TriangleSize/2, rect.Y);//上
                path.AddLine(rect.Left + rect.Width / 2 - TriangleSize/2, rect.Y, rect.Left + rect.Width / 2, rect.Y - TriangleSize-1);//上
                path.AddLine(rect.Left + rect.Width / 2, rect.Y - TriangleSize-1, rect.Left + rect.Width / 2 + TriangleSize-1, rect.Y);//上
                path.AddLine(rect.Left + rect.Width / 2 + TriangleSize/2, rect.Y, rect.Right - cornerRadius * 2, rect.Y);//上
            }
            else
            {
                path.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);//上
            }

            #endregion 上边

            path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);//右上角

            #region 右边

            if (m_location == AnchorTipsLocation.LEFT)
            {
                path.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height / 2 - TriangleSize/2);//右
                path.AddLine(rect.Right, rect.Y + rect.Height / 2 - TriangleSize/2, rect.Right + TriangleSize-1, rect.Y + rect.Height / 2);//右
                path.AddLine(rect.Right + TriangleSize-1, rect.Y + rect.Height / 2, rect.Right, rect.Y + rect.Height / 2 + TriangleSize/2);//右
                path.AddLine(rect.Right, rect.Y + rect.Height / 2 + TriangleSize/2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);//右
            }
            else
            {
                path.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);//右
            }

            #endregion 右边

            path.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);//右下角

            #region 下边

            if (m_location == AnchorTipsLocation.TOP)
            {
                path.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.Left + rect.Width / 2 + TriangleSize/2, rect.Bottom);
                path.AddLine(rect.Left + rect.Width / 2 + TriangleSize/2, rect.Bottom, rect.Left + rect.Width / 2, rect.Bottom + TriangleSize-1);
                path.AddLine(rect.Left + rect.Width / 2, rect.Bottom + TriangleSize-1, rect.Left + rect.Width / 2 - TriangleSize/2, rect.Bottom);
                path.AddLine(rect.Left + rect.Width / 2 - TriangleSize/2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            }
            else
            {
                path.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            }

            #endregion 下边

            path.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);//左下角

            #region 左边

            if (m_location == AnchorTipsLocation.RIGHT)
            {
                path.AddLine(rect.Left, rect.Y + cornerRadius * 2, rect.Left, rect.Y + rect.Height / 2 - TriangleSize/2);//左
                path.AddLine(rect.Left, rect.Y + rect.Height / 2 - TriangleSize/2, rect.Left - TriangleSize-1, rect.Y + rect.Height / 2);//左
                path.AddLine(rect.Left - TriangleSize-1, rect.Y + rect.Height / 2, rect.Left, rect.Y + rect.Height / 2 + TriangleSize/2);//左
                path.AddLine(rect.Left, rect.Y + rect.Height / 2 + TriangleSize/2, rect.Left, rect.Y + rect.Height - cornerRadius * 2);//左
            }
            else
            {
                path.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);//左
            }

            #endregion 左边

            path.CloseFigure();

            #endregion 获取窗体path    English:Get the form path

            Bitmap bit = new Bitmap(formSize.Width, formSize.Height);
            this.Size = formSize;

            #region 画图    English:Drawing

            Graphics gBit = Graphics.FromImage(bit);
            gBit.SmoothingMode = SmoothingMode.AntiAlias;  //使绘图质量最高，即消除锯齿
            gBit.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            gBit.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gBit.CompositingQuality = CompositingQuality.HighQuality;
            gBit.FillPath(new SolidBrush(_background), path);
            gBit.DrawString(strMsg, _font, new SolidBrush(_foreColor), rect, new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            gBit.Dispose();

            #endregion 画图    English:Drawing

            SetBits(bit);
        }

        #endregion 构造函数    English:Constructor

        #region 显示一个提示    English:Show a hint

        /// <summary>
        /// Shows the tips.
        /// </summary>
        /// <param name="anchorControl">The parent control.</param>
        /// <param name="strMsg">The string MSG.</param>
        /// <param name="location">The location.</param>
        /// <param name="background">The background.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <param name="deviation">The deviation.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="autoCloseTime">The automatic close time.</param>
        /// <param name="blnTopMost">是否置顶</param>
        /// <returns>FrmAnchorTips.</returns>
        public static HAnchorTips ShowTips(
            Control anchorControl,
            string strMsg,
            string fontName,
            int fontSize = 10,
            int autoCloseTime = 5000,
            bool blnTopMost = true,
            AnchorTipsLocation location = AnchorTipsLocation.RIGHT,
            Color? background = null,
            Color? foreColor = null,
            Size? deviation = null
            )
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return null;
            }
            Point p;
            if (anchorControl is Form)
            {
                p = anchorControl.Location;
            }
            else
            {
                p = anchorControl.Parent.PointToScreen(anchorControl.Location);
            }
            if (deviation != null)
            {
                p = p + deviation.Value;
            }
            return ShowTips(new Rectangle(p, anchorControl.Size), strMsg, fontName, fontSize, location, background, foreColor, autoCloseTime, anchorControl.Parent, blnTopMost);
        }

        #endregion 显示一个提示    English:Show a hint

        #region 显示一个提示    English:Show a hint

        /// <summary>
        /// Shows the tips.
        /// </summary>
        /// <param name="rectControl">The rect control.</param>
        /// <param name="strMsg">The string MSG.</param>
        /// <param name="location">The location.</param>
        /// <param name="background">The background.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="autoCloseTime">The automatic close time.</param>
        /// <param name="parentForm">父窗体</param>
        /// <param name="blnTopMost">是否置顶</param>
        /// <returns>FrmAnchorTips.</returns>
        public static HAnchorTips ShowTips(
            Rectangle rectControl,
            string strMsg,
            string fontName,
            int fontSize,
            AnchorTipsLocation location = AnchorTipsLocation.RIGHT,
            Color? background = null,
            Color? foreColor = null,
            int autoCloseTime = 5000,
            Control parentControl = null,
            bool blnTopMost = true)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return null;
            }
            HAnchorTips frm = new HAnchorTips(rectControl, strMsg, fontName, fontSize, location, background, foreColor, autoCloseTime);
            frm.TopMost = blnTopMost;
            frm.Show(parentControl);
            //if (parentControl != null)
            //{
            //    parentControl.VisibleChanged += (a, b) =>
            //    {
            //        try
            //        {
            //            Control c = a as Control;
            //            if (CheckControlClose(c))
            //            {
            //                frm.Close();
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //        }
            //    };
            //}
            return frm;
        }

        /// <summary>CheckControlClose 方法。</summary>
        private static bool CheckControlClose(Control c)
        {
            if (c.IsDisposed || !c.Visible)
                return true;
            else if (c.Parent != null)
                return CheckControlClose(c.Parent);
            else
            {
                if (c is Form)
                    return false;
                else
                    return true;
            }
        }

        #endregion 显示一个提示    English:Show a hint

        #region Override

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Forms.Form.Closing"/> 事件。
        /// </summary>
        /// <param name="e">一个包含事件数据的 <see cref="T:System.ComponentModel.CancelEventArgs"/>。</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            haveHandle = false;
            this.Dispose();
        }

        /// <summary>
        /// Handles the <see cref="E:HandleCreated"/> event.
        /// </summary>
        /// <param name="e">包含事件数据的 <see cref="T:System.EventArgs"/>。</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            InitializeStyles();
            base.OnHandleCreated(e);
            haveHandle = true;
        }

        /// <summary>
        /// Gets the create parameters.
        /// </summary>
        /// <value>The create parameters.</value>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cParms = base.CreateParams;
                cParms.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cParms;
            }
        }

        #endregion Override

        /// <summary>
        /// Initializes the styles.
        /// </summary>
        private void InitializeStyles()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        #region 根据图片显示窗体    English:Display Forms Based on Pictures

        /// <summary>
        /// 功能描述:根据图片显示窗体 English:Display Forms Based on Pictures
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <exception cref="System.ApplicationException">
        /// The picture must be 32bit picture with alpha channel.
        /// </exception>
        /// <exception cref="ApplicationException">
        /// The picture must be 32bit picture with alpha channel.
        /// </exception>
        private void SetBits(Bitmap bitmap)
        {
            if (!haveHandle) return;

            if (!Bitmap.IsCanonicalPixelFormat(bitmap.PixelFormat) || !Bitmap.IsAlphaPixelFormat(bitmap.PixelFormat))
                throw new ApplicationException(HTranslation.GetContent("图片必须是带 Alpha 通道的 32 位图片。"));

            IntPtr oldBits = IntPtr.Zero;
            IntPtr screenDC = HWin32.GetDC(IntPtr.Zero);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr memDc = HWin32.CreateCompatibleDC(screenDC);

            try
            {
                HWin32.Point topLoc = new HWin32.Point(Left, Top);
                HWin32.Size bitMapSize = new HWin32.Size(bitmap.Width, bitmap.Height);
                HWin32.BLENDFUNCTION blendFunc = new HWin32.BLENDFUNCTION();
                HWin32.Point srcLoc = new HWin32.Point(0, 0);

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = HWin32.SelectObject(memDc, hBitmap);

                blendFunc.BlendOp = HWin32.AC_SRC_OVER;
                blendFunc.SourceConstantAlpha = 255;
                blendFunc.AlphaFormat = HWin32.AC_SRC_ALPHA;
                blendFunc.BlendFlags = 0;

                HWin32.UpdateLayeredWindow(Handle, screenDC, ref topLoc, ref bitMapSize, memDc, ref srcLoc, 0, ref blendFunc, HWin32.ULW_ALPHA);
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

        #endregion 根据图片显示窗体    English:Display Forms Based on Pictures

        #region 无焦点窗体处理

        /// <summary>
        /// Sets the active window.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>IntPtr.</returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr handle);

        /// <summary>
        /// The wm activate
        /// </summary>
        private const int WM_ACTIVATE = 0x006;

        /// <summary>
        /// The wm activateapp
        /// </summary>
        private const int WM_ACTIVATEAPP = 0x01C;

        /// <summary>
        /// The wm ncactivate
        /// </summary>
        private const int WM_NCACTIVATE = 0x086;

        /// <summary>
        /// The wa inactive
        /// </summary>
        private const int WA_INACTIVE = 0;

        /// <summary>
        /// The wm mouseactivate
        /// </summary>
        private const int WM_MOUSEACTIVATE = 0x21;

        /// <summary>
        /// The ma noactivate
        /// </summary>
        private const int MA_NOACTIVATE = 3;

        /// <summary>
        /// WNDs the proc.
        /// </summary>
        /// <param name="m">要处理的 Windows <see cref="T:System.Windows.Forms.Message"/>。</param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = new IntPtr(MA_NOACTIVATE);
                return;
            }
            else if (m.Msg == WM_NCACTIVATE)
            {
                if (((int)m.WParam & 0xFFFF) != WA_INACTIVE)
                {
                    if (m.LParam != IntPtr.Zero)
                    {
                        SetActiveWindow(m.LParam);
                    }
                    else
                    {
                        SetActiveWindow(IntPtr.Zero);
                    }
                }
            }
            base.WndProc(ref m);
        }

        #endregion 无焦点窗体处理
    }

 
}
