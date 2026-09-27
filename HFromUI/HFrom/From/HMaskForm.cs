using HFromUI.HData;
using HFromUI.HControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HControl.Tools.DateTime;
    using HFromUI.HData.Win;
    using HFromUI.HFrom.Panel;
    using HFromUI.HControl.Tools.Button;
    public partial class HMaskForm : Form
    {
        private HForm MainPPForm;
        private Form MainForm;
        private Control MainControl;

        public HMaskForm(Form form, Image image = null, int imgWidth = 100, int imgHeight = 100)
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            MainForm = form;
            InitForm();

            if (image != null)
            {
                pictureBox_loading.Image = image;
            }
            if (imgWidth > 0 && imgHeight > 0)
            {
                pictureBox_loading.Size = new Size(imgWidth, imgHeight);
            }

            pictureBox_loading.Location = new Point((this.Width - pictureBox_loading.Width) / 2, (this.Height - pictureBox_loading.Height) / 2);
        }

        public HMaskForm(HForm hForm, Image image = null, int imgWidth = 100, int imgHeight = 100)
        {
            InitializeComponent();
            MainPPForm = hForm;
            InitPPForm();

            if (image != null)
            {
                pictureBox_loading.Image = image;
            }
            if (imgWidth > 0 && imgHeight > 0)
            {
                pictureBox_loading.Size = new Size(imgWidth, imgHeight);
            }
            pictureBox_loading.Location = new Point((this.Width - pictureBox_loading.Width) / 2, (this.Height - pictureBox_loading.Height) / 2);
        }

        public HMaskForm(Control c, Image image = null, int imgWidth = 100, int imgHeight = 100)
        {
            InitializeComponent();
            MainControl = c;
            InitControl();

            if (image != null)
            {
                pictureBox_loading.Image = image;
            }
            if (imgWidth > 0 && imgHeight > 0)
            {
                pictureBox_loading.Size = new Size(imgWidth, imgHeight);
            }
            pictureBox_loading.Location = new Point((this.Width - pictureBox_loading.Width) / 2, (this.Height - pictureBox_loading.Height) / 2);
        }

        /// <summary>InitForm 方法。</summary>
        private void InitForm()
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            if (MainForm.FormBorderStyle == FormBorderStyle.None)
            {
                this.Location = new Point(MainForm.Location.X, MainForm.Location.Y);
                this.Width = MainForm.Width;
                this.Height = MainForm.Height;
            }
            else
            {
                this.Location = new Point(MainForm.Location.X + 8, MainForm.Location.Y);
                this.Width = MainForm.Width - 16;
                this.Height = MainForm.Height - 8;
            }

            //MainForm.Invoke(new MethodInvoker(() => { MainForm.Enabled = false; }));
        }

        /// <summary>InitPPForm 方法。</summary>
        private void InitPPForm()
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Location = new Point(MainPPForm.Location.X, MainPPForm.Location.Y);
            this.Width = MainPPForm.Width;
            this.Height = MainPPForm.Height;
            HWin32.SetFormRoundRectRgn(this, MainPPForm.Radius);
            // MainPPForm.Invoke(new MethodInvoker(() => { MainPPForm.Enabled = false; }));
        }

        /// <summary>InitControl 方法。</summary>
        private void InitControl()
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopLevel = false;
            MainControl.Parent.Controls.Add(this);

            this.Location = MainControl.Location;
            this.Width = MainControl.Width;
            this.Height = MainControl.Height;
            this.Anchor = MainControl.Anchor;
            this.Dock = MainControl.Dock;
            MainControl.BeginInvoke(new MethodInvoker(() => { MainControl.Enabled = false; }));
            this.BringToFront();

            Point pt = this.PointToScreen(new Point(0, 0));
            Bitmap b = new Bitmap(this.Width, this.Height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(pt, new Point(), new Size(this.Width, this.Height));
            }

            this.BackgroundImage = b;

            int radius = 0;
            if (MainControl is HPanel)
            {
                radius = (MainControl as HPanel).Radius;
            }
            else if (MainControl is HPushButton)
            {
                radius = (MainControl as HPushButton).Radius;
            }
            else if (MainControl is HDateTimePicker)
            {
                radius = (MainControl as HDateTimePicker).Radius;
            }

            if (radius != 0)
            {
                HWin32.SetFormRoundRectRgn(this, radius);
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (MainControl != null)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb((int)(this.Opacity * 255), this.BackColor)), new RectangleF(0, 0, this.Width, this.Height));
            }
        }

        /// <summary>响应 Closed 事件。</summary>
        protected override void OnClosed(EventArgs e)
        {
            if (MainForm != null)
                MainForm.BeginInvoke(new MethodInvoker(() => { MainForm.Enabled = true; }));
            if (MainPPForm != null)
                MainPPForm.BeginInvoke(new MethodInvoker(() => { MainPPForm.Enabled = true; }));
            if (MainControl != null)
            {
                MainControl.BeginInvoke(new MethodInvoker(() => { MainControl.Enabled = true; }));
                MainControl.Parent.Controls.Remove(this);
            }

            base.OnClosed(e);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            pictureBox_loading.Location = new Point((this.Width - pictureBox_loading.Width) / 2, (this.Height - pictureBox_loading.Height) / 2);
        }
    }
}
