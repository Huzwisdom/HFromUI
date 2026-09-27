using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HLangage;
    public partial class HMessageForm : HForm
    {
        /// <summary>MessageInfo 字段。</summary>
        private string MessageInfo = string.Empty;
        /// <summary>BtnType 字段。</summary>
        private MessageBoxButtons BtnType = MessageBoxButtons.OK;
        /// <summary>IconType 字段。</summary>
        private MessageBoxIcon IconType = MessageBoxIcon.None;
        /// <summary>Lang 字段。</summary>
        private PPMessageBox.Language Lang = PPMessageBox.Language.ENGLISH;
        /// <summary>timerInterval 字段。</summary>
        private int timerInterval = 0;
        /// <summary>maxWidth 字段。</summary>
        private int maxWidth = 500;//

        public HMessageForm(string Text)
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            MessageInfo = Text;
        }

        public HMessageForm(string Text, MessageBoxButtons button)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
        }

        public HMessageForm(string Text, MessageBoxButtons button, MessageBoxIcon icon)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            IconType = icon;
        }

        public HMessageForm(string Text, string title)
        {
            InitializeComponent();
            MessageInfo = Text;
            this.Text = title;
        }

        public HMessageForm(string Text, string title, MessageBoxButtons button)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            this.Text = title;
        }

        public HMessageForm(string Text, string title, MessageBoxButtons button, MessageBoxIcon icon)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            IconType = icon;
            this.Text = title;
        }

        public HMessageForm(string Text, int millisecond)
        {
            InitializeComponent();
            MessageInfo = Text;
            timerInterval = millisecond;
        }

        public HMessageForm(string Text, string title, int millisecond)
        {
            InitializeComponent();
            MessageInfo = Text;
            this.Text = title;
            timerInterval = millisecond;
        }

        public HMessageForm(string Text, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            Lang = lang;
        }

        public HMessageForm(string Text, MessageBoxButtons button, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            Lang = lang;
        }

        public HMessageForm(string Text, MessageBoxButtons button, MessageBoxIcon icon, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            IconType = icon;
            Lang = lang;
        }

        public HMessageForm(string Text, string title, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            this.Text = title;
            Lang = lang;
        }

        public HMessageForm(string Text, string title, MessageBoxButtons button, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            this.Text = title;
            Lang = lang;
        }

        public HMessageForm(string Text, string title, MessageBoxButtons button, MessageBoxIcon icon, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            BtnType = button;
            IconType = icon;
            this.Text = title;
            Lang = lang;
        }

        public HMessageForm(string Text, int millisecond, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            timerInterval = millisecond;
            Lang = lang;
        }

        public HMessageForm(string Text, string title, int millisecond, PPMessageBox.Language lang)
        {
            InitializeComponent();
            MessageInfo = Text;
            this.Text = title;
            timerInterval = millisecond;
            Lang = lang;
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.EnableDoubleCilckSizeChange = false;

            switch (IconType)
            {
                case MessageBoxIcon.Information: this.Icon = SystemIcons.Information; this.ShowTitleIcon = true; break;
                case MessageBoxIcon.Exclamation: this.Icon = SystemIcons.Exclamation; this.ShowTitleIcon = true; break;
                case MessageBoxIcon.Error: this.Icon = SystemIcons.Error; this.ShowTitleIcon = true; break;
                case MessageBoxIcon.Question: this.Icon = SystemIcons.Question; this.ShowTitleIcon = true; break;
                case MessageBoxIcon.None: this.ShowTitleIcon = false; break;
            }
            Graphics g = this.CreateGraphics();
            SizeF fontsize = g.MeasureString(MessageInfo, this.Font);
            int width = this.Width;
            if (fontsize.Width > this.Width - 30)
            {
                width = (int)fontsize.Width + 30;
                if (width > maxWidth)
                {
                    this.Width = maxWidth;
                }
                else
                {
                    this.Width = width;
                }
            }
            //Graphics g = this.CreateGraphics();
            //if (icon == null)
            //{
            //    SizeF fontsize = g.MeasureString(MessageInfo, this.Font);
            //    string str = MessageInfo;
            //    if (fontsize.Width > this.Width - 30)
            //    {
            //        str = "";
            //        foreach (char c in MessageInfo)
            //        {
            //            str += c;
            //            fontsize = g.MeasureString(str, this.Font);
            //            if (fontsize.Width > this.Width - 30)
            //            {
            //                str = str.Substring(0, str.Length - 1);
            //                str += '\n';
            //                str += c;
            //            }
            //        }
            //    }
            //    MessageInfo = str;
            //}
            //else
            //{
            //    SizeF fontsize = g.MeasureString(MessageInfo, this.Font);
            //    string str = MessageInfo;
            //    if (fontsize.Width > this.Width - 70)
            //    {
            //        str = "";
            //        foreach (char c in MessageInfo)
            //        {
            //            str += c;
            //            fontsize = g.MeasureString(str, this.Font);
            //            if (fontsize.Width > this.Width - 70)
            //            {
            //                str = str.Substring(0, str.Length - 1);
            //                str += '\n';
            //                str += c;
            //            }
            //        }
            //    }
            //    MessageInfo = str;
            //}

            if (Lang == PPMessageBox.Language.CHINESE)
            {
                hButton_OK.Text = HTranslation.GetContent("确定");
                hButton_YES.Text = HTranslation.GetContent("是");
                hButton_NO.Text = HTranslation.GetContent("否");
                hButton_CANCLE.Text = HTranslation.GetContent("取消");
                hButton_IGNORE.Text = HTranslation.GetContent("忽略");
                hButton_ABORT.Text = HTranslation.GetContent("中止");
                hButton_RETRY.Text = HTranslation.GetContent("重试");
            }

            switch (BtnType)
            {
                case MessageBoxButtons.OK:
                    ShowOK();
                    break;

                case MessageBoxButtons.YesNo:
                    ShowYESNO();
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    ShowABORTRETRYIGNORE();
                    break;

                case MessageBoxButtons.OKCancel:
                    ShowOKCANCLE();
                    break;

                case MessageBoxButtons.RetryCancel:
                    ShowRETRYCANCLE();
                    break;

                case MessageBoxButtons.YesNoCancel:
                    ShowYESNOCANCLE();
                    break;
            }
            hButton_OK.Parent = this;
            hButton_YES.Parent = this;
            hButton_NO.Parent = this;
            hButton_CANCLE.Parent = this;
            hButton_ABORT.Parent = this;
            hButton_RETRY.Parent = this;
            hButton_IGNORE.Parent = this;

            hButton_OK.BringToFront();
            hButton_YES.BringToFront();
            hButton_NO.BringToFront();
            hButton_CANCLE.BringToFront();
            hButton_ABORT.BringToFront();
            hButton_RETRY.BringToFront();
            hButton_IGNORE.BringToFront();

            if (timerInterval != 0)
            {
                timer.Interval = timerInterval;
                timer.Enabled = true;
            }

            this.TopMost = true;
            this.CenterToParent();
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            //g.Clear(this.BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            SizeF fontsize = g.MeasureString(MessageInfo, this.Font);

            Rectangle rect = new Rectangle(15, this.TitleHeight, this.Width - 30, this.Height - this.TitleHeight - 40);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Trimming = StringTrimming.EllipsisPath;
            SolidBrush solidBrush = new SolidBrush(this.ForeColor);
            g.DrawString(MessageInfo, this.Font, solidBrush, rect, stringFormat);

            //if (icon == null)
            //{
            //    SizeF fontsize = g.MeasureString(MessageInfo, this.Font);
            //    PointF p = new PointF((this.Width - fontsize.Width) / 2, (this.Height - fontsize.Height) / 2);
            //    g.DrawString(MessageInfo, this.Font, new SolidBrush(this.ForeColor), p);
            //}
            //else
            //{
            //    SizeF fontsize = g.MeasureString(MessageInfo, this.Font);
            //    Point icoPoint = new Point((this.Width - 35 - (int)fontsize.Width) / 2, (this.Height - 30) / 2);
            //    g.DrawIcon(icon, new Rectangle(icoPoint, new Size(30, 30)));

            //    PointF p = new PointF(icoPoint.X + 35, (this.Height - fontsize.Height) / 2);
            //    g.DrawString(MessageInfo, this.Font, new SolidBrush(this.ForeColor), p);
            //}
        }

        /// <summary>ShowOK 方法。</summary>
        private void ShowOK()
        {
            Font font = this.Font;
            Size fontsize = TextRenderer.MeasureText(MessageInfo, font);

            hButton_OK.Visible = true;
            hButton_YES.Visible = false;
            hButton_NO.Visible = false;
            hButton_ABORT.Visible = false;
            hButton_CANCLE.Visible = false;
            hButton_IGNORE.Visible = false;
            hButton_RETRY.Visible = false;

            hButton_OK.Location = new Point(this.Width - 100, this.Height - 40);

            //panel_base.Invalidate();
        }

        /// <summary>ShowYESNO 方法。</summary>
        private void ShowYESNO()
        {
            Font font = this.Font;
            Size fontsize = TextRenderer.MeasureText(MessageInfo, font);

            hButton_OK.Visible = false;
            hButton_YES.Visible = true;
            hButton_NO.Visible = true;
            hButton_ABORT.Visible = false;
            hButton_CANCLE.Visible = false;
            hButton_IGNORE.Visible = false;
            hButton_RETRY.Visible = false;

            hButton_YES.Location = new Point(this.Width / 4 - 40, this.Height - 50);
            hButton_NO.Location = new Point(3 * this.Width / 4 - 40, this.Height - 50);

            //panel_base.Invalidate();
        }

        /// <summary>ShowOKCANCLE 方法。</summary>
        private void ShowOKCANCLE()
        {
            Font font = this.Font;
            Size fontsize = TextRenderer.MeasureText(MessageInfo, font);

            hButton_OK.Visible = true;
            hButton_YES.Visible = false;
            hButton_NO.Visible = false;
            hButton_ABORT.Visible = false;
            hButton_CANCLE.Visible = true;
            hButton_IGNORE.Visible = false;
            hButton_RETRY.Visible = false;

            hButton_OK.Location = new Point(this.Width / 4 - 40, this.Height - 50);
            hButton_CANCLE.Location = new Point(3 * this.Width / 4 - 40, this.Height - 50);

            // panel_base.Invalidate();
        }

        /// <summary>ShowRETRYCANCLE 方法。</summary>
        private void ShowRETRYCANCLE()
        {
            Font font = this.Font;
            Size fontsize = TextRenderer.MeasureText(MessageInfo, font);

            hButton_OK.Visible = false;
            hButton_YES.Visible = false;
            hButton_NO.Visible = false;
            hButton_ABORT.Visible = false;
            hButton_CANCLE.Visible = true;
            hButton_IGNORE.Visible = false;
            hButton_RETRY.Visible = true;

            hButton_RETRY.Location = new Point(this.Width / 4 - 40, this.Height - 50);
            hButton_CANCLE.Location = new Point(3 * this.Width / 4 - 40, this.Height - 50);

            //panel_base.Invalidate();
        }

        /// <summary>ShowYESNOCANCLE 方法。</summary>
        private void ShowYESNOCANCLE()
        {
            Font font = this.Font;
            Size fontsize = TextRenderer.MeasureText(MessageInfo, font);

            hButton_OK.Visible = false;
            hButton_YES.Visible = true;
            hButton_NO.Visible = true;
            hButton_ABORT.Visible = false;
            hButton_CANCLE.Visible = true;
            hButton_IGNORE.Visible = false;
            hButton_RETRY.Visible = false;

            hButton_YES.Location = new Point(this.Width / 6 - 40, this.Height - 50);
            hButton_NO.Location = new Point(3 * this.Width / 6 - 40, this.Height - 50);
            hButton_CANCLE.Location = new Point(5 * this.Width / 6 - 40, this.Height - 50);

            //panel_base.Invalidate();
        }

        /// <summary>ShowABORTRETRYIGNORE 方法。</summary>
        private void ShowABORTRETRYIGNORE()
        {
            Font font = this.Font;
            Size fontsize = TextRenderer.MeasureText(MessageInfo, font);

            hButton_OK.Visible = false;
            hButton_YES.Visible = false;
            hButton_NO.Visible = false;
            hButton_ABORT.Visible = true;
            hButton_CANCLE.Visible = false;
            hButton_IGNORE.Visible = true;
            hButton_RETRY.Visible = true;

            hButton_ABORT.Location = new Point(this.Width / 6 - 40, this.Height - 50);
            hButton_RETRY.Location = new Point(3 * this.Width / 6 - 40, this.Height - 50);
            hButton_IGNORE.Location = new Point(5 * this.Width / 6 - 40, this.Height - 50);

            //panel_base.Invalidate();
        }

        /// <summary>hButton_OK_Click 方法。</summary>
        private void hButton_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>hButton_YES_Click 方法。</summary>
        private void hButton_YES_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        /// <summary>hButton_NO_Click 方法。</summary>
        private void hButton_NO_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        /// <summary>hButton_RETRY_Click 方法。</summary>
        private void hButton_RETRY_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        /// <summary>hButton_CANCLE_Click 方法。</summary>
        private void hButton_CANCLE_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>hButton_IGNORE_Click 方法。</summary>
        private void hButton_IGNORE_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
            this.Close();
        }

        /// <summary>hButton_ABORT_Click 方法。</summary>
        private void hButton_ABORT_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        /// <summary>timer_Tick 方法。</summary>
        private void timer_Tick(object sender, EventArgs e)
        {
            this.timer.Enabled = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public partial class PPMessageBox
    {
        public enum Language
        {
            ENGLISH,
            CHINESE,
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, int millisecond, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, millisecond);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, MessageBoxButtons button, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, button);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, MessageBoxButtons button, MessageBoxIcon icon, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, button, icon);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, int millisecond, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, millisecond);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, MessageBoxButtons button, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, button);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, MessageBoxButtons button, MessageBoxIcon icon, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, button, icon);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, int millisecond, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, millisecond, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, MessageBoxButtons button, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, button, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, MessageBoxButtons button, MessageBoxIcon icon, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, button, icon, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, int millisecond, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, millisecond, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, MessageBoxButtons button, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, button, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }

        /// <summary>显示。</summary>
        public static DialogResult Show(string text, string title, MessageBoxButtons button, MessageBoxIcon icon, Language lang, IWin32Window win32Window = null)
        {
            HMessageForm mf = new HMessageForm(text, title, button, icon, lang);
            if (win32Window == null || (Form.FromHandle(win32Window.Handle) as Form).WindowState == FormWindowState.Minimized)
            {
                mf.StartPosition = FormStartPosition.CenterScreen;
                return mf.ShowDialog();
            }
            else
            {
                return mf.ShowDialog(win32Window);
            }
        }
    }
}
