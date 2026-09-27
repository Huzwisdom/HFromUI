using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HFrom.Date
{
    using HFromUI.HLangage;
    [DefaultEvent("DayClick")]
    internal partial class HDateMonthPanel : UserControl
    {
        /// <summary>Buttons 字段：12 个月格。</summary>
        private readonly List<HPushButton> Buttons = new List<HPushButton>();

        /// <summary>dateTime 字段。</summary>
        private DateTime dateTime = DateTime.Now;

        public DateTime DateTime
        {
            get { return dateTime; }
            set
            {
                dateTime = value;
            }
        }

        public HDateMonthPanel()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            for (int i = 1; i <= 12; i++)
            {
                var padding = new Padding(18, 9, 19, 9);
                if (i == 1 || i == 5 || i == 9 || i == 4 || i == 8 || i == 12)
                {
                    padding = new Padding(19, 9, 19, 9);
                }
                var button = new HPushButton
                {
                    Size = new Size(50, 50),
                    Margin = padding,
                    ButtonStyle = HPushButtonStyle.Custom,
                    Shape = HPushButtonShape.Round,
                    Radius = 4,
                    Cursor = Cursors.Hand,
                    Text = i.ToString(),
                    NormalBackColor = Color.White,
                    NormalBorderColor = Color.LightGray,
                    HoverBackColor = Color.White,
                    HoverBorderColor = Color.DodgerBlue,
                    PressedBackColor = Color.DodgerBlue,
                    PressedBorderColor = Color.DodgerBlue,
                    PressedForeColor = Color.White
                };
                Buttons.Add(button);
                flowLayoutPanel1.Controls.Add(button);
                button.Click += Button_Click;
            }
        }

        public event EventHandler<MonthClickEventArgs> MonthClick;

        /// <summary>Button_Click 方法。</summary>
        private void Button_Click(object sender, EventArgs e)
        {
            int month = Convert.ToInt32((sender as HPushButton).Text);
            if (MonthClick != null)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    MonthClickEventArgs ee = new MonthClickEventArgs(new DateTime(DateTime.Year, month, 1, DateTime.Hour, DateTime.Minute, DateTime.Second));
                    MonthClick(this, ee);
                }));
            }
        }

        /// <summary>LoadMonth 方法：按主控件 MinDate/MaxDate 禁用越界月份。</summary>
        public void LoadMonth()
        {
            HDateTimePicker pPDateTimePicker = (this.Parent as HDateTimeDropForm).MainDateTimePicker;
            foreach (var pPButton in Buttons)
            {
                pPButton.Enabled = true;
                pPButton.NormalBackColor = Color.White;
            }

            if (this.dateTime.Year == pPDateTimePicker.MinDate.Year)
            {
                foreach (var pPButton in Buttons)
                {
                    if (Convert.ToInt32(pPButton.Text) < pPDateTimePicker.MinDate.Month)
                    {
                        pPButton.Enabled = false;
                        pPButton.NormalBackColor = Color.LightGray;
                    }
                }
            }

            if (this.dateTime.Year == pPDateTimePicker.MaxDate.Year)
            {
                foreach (var pPButton in Buttons)
                {
                    if (Convert.ToInt32(pPButton.Text) > pPDateTimePicker.MinDate.Month)
                    {
                        pPButton.Enabled = false;
                        pPButton.NormalBackColor = Color.LightGray;
                    }
                }
            }
        }

        /// <summary>设置 lang。</summary>
        public void SetLang()
        {
            hLabel1.Text = HTranslation.GetContent("月份");
        }
    }

    public class MonthClickEventArgs : EventArgs
    {
        public MonthClickEventArgs(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public DateTime DateTime;
    }
}
