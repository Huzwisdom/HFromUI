using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HFrom.Date
{
    [DefaultEvent("DayClick")]
    internal partial class HDateYearPanel : UserControl
    {
        /// <summary>Buttons 字段：24 个年份格。</summary>
        private readonly List<HPushButton> Buttons = new List<HPushButton>();

        /// <summary>dateTime 字段。</summary>
        private DateTime dateTime = DateTime.Now;

        /// <summary>selectIndex 字段。</summary>
        private int selectIndex = 0;

        public DateTime DateTime
        {
            get { return dateTime; }
            set
            {
                dateTime = value;
                selectIndex = 0;
            }
        }

        public HDateYearPanel()
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
            for (int i = 1; i <= 24; i++)
            {
                var button = new HPushButton
                {
                    Size = new Size(40, 40),
                    Margin = new Padding(9, 6, 9, 5),
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

        public event EventHandler<YearClickEventArgs> YearClick;

        /// <summary>Button_Click 方法。</summary>
        private void Button_Click(object sender, EventArgs e)
        {
            int year = Convert.ToInt32((sender as HPushButton).Text);
            if (YearClick != null)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    YearClickEventArgs ee = new YearClickEventArgs(new DateTime(year, 1, 1, 0, 0, 0));
                    YearClick(this, ee);
                }));
            }
        }

        /// <summary>LoadYear 方法：返回当前 24 年区间标题。</summary>
        public string LoadYear(int index)
        {
            selectIndex += index;

            HDateTimePicker mainDateTimePicker = (this.Parent as HDateTimeDropForm).MainDateTimePicker;
            int year = DateTime.Year;

            int a = (year + selectIndex * 24) / 24;

            int startYear = a * 24;
            int endYear = startYear + 24 - 1;

            for (int i = startYear; i <= endYear; i++)
            {
                var btn = Buttons[i - startYear];
                btn.Text = i.ToString();
                if (i < mainDateTimePicker.MinDate.Year || i > mainDateTimePicker.MaxDate.Year)
                {
                    btn.Enabled = false;
                    btn.NormalBackColor = Color.LightGray;
                }
                else
                {
                    btn.Enabled = true;
                    btn.NormalBackColor = Color.White;
                }
            }

            return startYear.ToString() + "-" + endYear.ToString();
        }


    }

    public class YearClickEventArgs : EventArgs
    {
        public YearClickEventArgs(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public DateTime DateTime;
    }
}
