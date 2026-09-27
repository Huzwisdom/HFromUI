using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Panel
{
    using Panel = System.Windows.Forms.Panel;
    public partial class HUnlockerPanel : UserControl
    {
        /// <summary>ButtonImage 字段。</summary>
        private Bitmap ButtonImage = new Bitmap(50, 50);
        /// <summary>ButtonSelectImage 字段。</summary>
        private Bitmap ButtonSelectImage = new Bitmap(50, 50);
        /// <summary>ButtonErrorImage 字段。</summary>
        private Bitmap ButtonErrorImage = new Bitmap(50, 50);
        /// <summary>ButtonPassImage 字段。</summary>
        private Bitmap ButtonPassImage = new Bitmap(50, 50);
        /// <summary>ButtonSize 成员。</summary>
        public Size ButtonSize { get; set; } = new Size(30, 30);

        /// <summary>NormalColor 成员。</summary>
        public Color NormalColor { get; set; } = Color.DimGray;
        /// <summary>线颜色。</summary>
        public Color LineColor { get; set; } = Color.DodgerBlue;

        /// <summary>LineErrorColor 成员。</summary>
        public Color LineErrorColor { get; set; } = Color.Red;

        /// <summary>LinePassColor 成员。</summary>
        public Color LinePassColor { get; set; } = Color.Green;

        /// <summary>线宽。</summary>
        public int LineWidth { get; set; } = 20;

        /// <summary>password 字段。</summary>
        private string password = "1234";

        /// <summary>
        /// 只能试4到20位非连续非0的数字
        /// </summary>
        public string Password
        {
            get { return password; }
            set
            {
                try
                {
                    if (value.Length >= 4 && value.Length < 20)
                    {
                        foreach (char c in value)
                        {
                            if (!Char.IsNumber(c) || c == '0')
                                return;
                        }
                        for (int i = 0; i < value.Length; i++)
                        {
                            if (i < value.Length - 1)
                            {
                                if (value[i] == value[i + 1])
                                    return;
                            }
                        }

                        password = value;
                    }
                }
                catch
                {
                    return;
                }
            }
        }

        /// <summary>PasswordStatus 成员。</summary>
        public PswStatus PasswordStatus = PswStatus.Normal;

        /// <summary>CodeList 字段。</summary>
        private List<int> CodeList = new List<int>();

        /// <summary>ButtonRectList 字段。</summary>
        private List<Rectangle> ButtonRectList = new List<Rectangle>() { };

        /// <summary>StartUnlock 字段。</summary>
        private bool StartUnlock = false;

        public HUnlockerPanel()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.UpdateStyles();

            GetRectList();
            ButtonImage = GetButtonBmp(NormalColor);
            ButtonSelectImage = GetButtonBmp(LineColor);
            ButtonErrorImage = GetButtonBmp(LineErrorColor);
            ButtonPassImage = GetButtonBmp(LinePassColor);
        }

        /// <summary>获取 rectList。</summary>
        private void GetRectList()
        {
            ButtonRectList.Clear();

            float perWidth = (float)this.Width / 3;
            float perHeight = (float)this.Height / 3;

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    RectangleF rectangle = new RectangleF(column * perWidth, row * perHeight, perWidth, perHeight);
                    Rectangle rect = Rectangle.Ceiling(HDrawPaint.GetCenterRectInRect(rectangle, ButtonSize));
                    ButtonRectList.Add(rect);
                }
            }
        }

        /// <summary>获取 buttonBmp。</summary>
        private Bitmap GetButtonBmp(Color color)
        {
            Bitmap bmp = new Bitmap(ButtonSize.Width, ButtonSize.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Pen pen = new Pen(color, 0.12f * bmp.Width);
            Brush brush = new SolidBrush(color);
            g.DrawEllipse(pen, 0.06f * bmp.Width, 0.06f * bmp.Height, 0.88f * bmp.Width - 1, 0.88f * bmp.Height - 1);
            g.FillEllipse(brush, 0.3f * bmp.Width, 0.3f * bmp.Height, 0.4f * bmp.Width, 0.4f * bmp.Height);
            pen.Dispose();
            brush.Dispose();
            g.Dispose();
            return bmp;
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            this.Width = this.Height;
            GetRectList();
            base.OnSizeChanged(e);
        }

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            StartUnlock = true;
            CodeList.Clear();
            PasswordStatus = PswStatus.Normal;
            this.Invalidate();
        }

        /// <summary>mousePoint 字段。</summary>
        private Point mousePoint = new Point(0, 0);

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (StartUnlock)
            {
                if (ButtonRectList.Count >= 9)
                {
                    int num = 0;
                    if (ButtonRectList[0].Contains(e.Location))
                    {
                        num = 1;
                    }
                    else if (ButtonRectList[1].Contains(e.Location))
                    {
                        num = 2;
                    }
                    else if (ButtonRectList[2].Contains(e.Location))
                    {
                        num = 3;
                    }
                    else if (ButtonRectList[3].Contains(e.Location))
                    {
                        num = 4;
                    }
                    else if (ButtonRectList[4].Contains(e.Location))
                    {
                        num = 5;
                    }
                    else if (ButtonRectList[5].Contains(e.Location))
                    {
                        num = 6;
                    }
                    else if (ButtonRectList[6].Contains(e.Location))
                    {
                        num = 7;
                    }
                    else if (ButtonRectList[7].Contains(e.Location))
                    {
                        num = 8;
                    }
                    else if (ButtonRectList[8].Contains(e.Location))
                    {
                        num = 9;
                    }

                    if (num > 0)
                    {
                        if (CodeList.Count > 0)
                        {
                            if (CodeList[CodeList.Count - 1] != num)
                            {
                                CodeList.Add(num);
                            }
                        }
                        else
                        {
                            CodeList.Add(num);
                        }
                    }
                }
            }
            mousePoint = e.Location;
            this.Invalidate();
        }

        public event EventHandler<UnlockFinishEventArgs> OnUnlockFinished;

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            StartUnlock = false;

            if (CodeList.Count == 0)
                return;
            StringBuilder sb = new StringBuilder();
            foreach (int num in CodeList)
            {
                sb.Append(num.ToString());
            }

            if (sb.ToString() == password)
            {
                PasswordStatus = PswStatus.Success;
            }
            else
            {
                PasswordStatus = PswStatus.Fail;
            }

            this.Invalidate();

            if (OnUnlockFinished != null)
            {
                OnUnlockFinished(this, new UnlockFinishEventArgs(sb.ToString(), PasswordStatus));
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            ButtonImage = GetButtonBmp(NormalColor);
            ButtonSelectImage = GetButtonBmp(LineColor);
            ButtonErrorImage = GetButtonBmp(LineErrorColor);
            ButtonPassImage = GetButtonBmp(LinePassColor);
            if (ButtonRectList.Count >= 9)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (CodeList.Contains(i + 1))
                    {
                        switch (PasswordStatus)
                        {
                            case PswStatus.Normal: g.DrawImage(ButtonSelectImage, ButtonRectList[i]); break;
                            case PswStatus.Success: g.DrawImage(ButtonPassImage, ButtonRectList[i]); break;
                            case PswStatus.Fail: g.DrawImage(ButtonErrorImage, ButtonRectList[i]); break;
                        }
                    }
                    else
                    {
                        g.DrawImage(ButtonImage, ButtonRectList[i]);
                    }
                }
                Color pencolor = LineColor;
                switch (PasswordStatus)
                {
                    case PswStatus.Normal: pencolor = LineColor; break;
                    case PswStatus.Success: pencolor = LinePassColor; break;
                    case PswStatus.Fail: pencolor = LineErrorColor; break;
                }

                Pen pen = new Pen(Color.FromArgb(200, pencolor), LineWidth);
                if (CodeList.Count >= 2)
                {
                    for (int i = 0; i < CodeList.Count; i++)
                    {
                        if (i != CodeList.Count - 1)
                            g.DrawLine(pen, HDrawPaint.GetCenterPointInRect(ButtonRectList[CodeList[i] - 1]), HDrawPaint.GetCenterPointInRect(ButtonRectList[CodeList[i + 1] - 1]));
                    }
                }

                if (StartUnlock && CodeList.Count >= 1)
                {
                    g.DrawLine(pen, HDrawPaint.GetCenterPointInRect(ButtonRectList[CodeList[CodeList.Count - 1] - 1]), mousePoint);
                }
            }
        }
    }

    public enum PswStatus
    {
        Normal,
        Success,
        Fail
    }

    public class UnlockFinishEventArgs : EventArgs
    {
        public UnlockFinishEventArgs(string password, PswStatus state)
        {
            Password = password;
            State = state;
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public PswStatus State { get; set; }
    }
}
