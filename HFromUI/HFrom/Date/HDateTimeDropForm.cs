using System;
using System.ComponentModel;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Date
{
    public partial class HDateTimeDropForm : HAnchorBase
    {
       
        public HDateTimePicker MainDateTimePicker;

        /// <summary>dateTime 字段。</summary>
        private DateTime dateTime = DateTime.Now;

        private DateTime DateTime
        {
            get { return dateTime; }
            set
            {
                dateTime = value;
            }
        }

        /// <summary>topStatus 字段。</summary>
        private TopStatus topStatus = TopStatus.Day;

        private enum TopStatus
        {
            Day,
            Month,
            Year
        }



        

        //protected override bool ShowWithoutActivation => true;//确保此窗体不会被激活

        public HDateTimeDropForm(HDateTimePicker pPDateTimePicker)
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            if (!DesignMode)
            {
                base.BaseControl = pPDateTimePicker;
                MainDateTimePicker = base.BaseControl as HDateTimePicker;
                if (!MainDateTimePicker.PickTime)
                {
                    Button_today.Location = new System.Drawing.Point(122, 266);
                    hTimePicker1.Visible = false;
                    btn_confirm.Visible = false;
                }

                this.DateTime = MainDateTimePicker.Value;
                LoadTopStatus(TopStatus.Day);
            }

        }

        /// <summary>Button_backward_Click 方法。</summary>
        private void Button_backward_Click(object sender, EventArgs e)
        {
            if (topStatus == TopStatus.Day)
            {
                hDateDayPanel1.LoadMonth(-1);
                Button_Top.Text = hDateDayPanel1.DateTime.ToString("yyyy-MM");
            }
            else if (topStatus == TopStatus.Year)
            {
                Button_Top.Text = hDateYearPanel1.LoadYear(-1);
            }
        }

        /// <summary>Button_forward_Click 方法。</summary>
        private void Button_forward_Click(object sender, EventArgs e)
        {
            if (topStatus == TopStatus.Day)
            {
                hDateDayPanel1.LoadMonth(1);
                Button_Top.Text = hDateDayPanel1.DateTime.ToString("yyyy-MM");
            }
            else if (topStatus == TopStatus.Year)
            {
                Button_Top.Text = hDateYearPanel1.LoadYear(1);
            }
        }

        /// <summary>hDateDayPanel1_DayClick 方法。</summary>
        private void hDateDayPanel1_DayClick(object sender, DayClickEventArgs e)
        {
            if(!MainDateTimePicker.PickTime)
            {
                MainDateTimePicker.Value = e.DateTime;
                this.Close();
            }
            else
            {
                this.DateTime = new DateTime(dateTime.Year, dateTime.Month, e.DateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            }
            
        }

        /// <summary>hDateMonthPanel1_MonthClick 方法。</summary>
        private void hDateMonthPanel1_MonthClick(object sender, MonthClickEventArgs e)
        {
            this.DateTime = new DateTime(e.DateTime.Year, e.DateTime.Month, 1, dateTime.Hour, dateTime.Minute, dateTime.Second);
            LoadTopStatus(TopStatus.Day);
            hDateDayPanel1.LoadMonth(0);
        }

        /// <summary>hDateYearPanel1_YearClick 方法。</summary>
        private void hDateYearPanel1_YearClick(object sender, YearClickEventArgs e)
        {
            this.DateTime = new DateTime(e.DateTime.Year, dateTime.Month, 1, dateTime.Hour, dateTime.Minute, dateTime.Second);
            LoadTopStatus(TopStatus.Month);
            hDateMonthPanel1.LoadMonth();
        }

        /// <summary>LoadTopStatus 方法。</summary>
        private void LoadTopStatus(TopStatus status)
        {
            switch (status)
            {
                case TopStatus.Day:
                    hDateDayPanel1.DateTime = this.dateTime;
                    hDateDayPanel1.Visible = true;
                    hDateMonthPanel1.Visible = false;
                    hDateYearPanel1.Visible = false;
                    this.topStatus = status;
                    Button_Top.Text = this.DateTime.ToString("yyyy-MM");
                    break;

                case TopStatus.Month:
                    hDateMonthPanel1.DateTime = this.dateTime;
                    hDateDayPanel1.Visible = false;
                    hDateMonthPanel1.Visible = true;
                    hDateYearPanel1.Visible = false;
                    this.topStatus = status;
                    Button_Top.Text = this.DateTime.ToString("yyyy");
                    break;

                case TopStatus.Year:
                    hDateYearPanel1.DateTime = this.dateTime;
                    hDateDayPanel1.Visible = false;
                    hDateMonthPanel1.Visible = false;
                    hDateYearPanel1.Visible = true;
                    hDateYearPanel1.LoadYear(0);
                    this.topStatus = status;
                    Button_Top.Text = this.DateTime.ToString("yyyy");
                    break;
            }

            hTimePicker1.Time = this.DateTime;
        }

        /// <summary>Button_Top_Click 方法。</summary>
        private void Button_Top_Click(object sender, EventArgs e)
        {
            switch (this.topStatus)
            {
                case TopStatus.Day:
                    LoadTopStatus(TopStatus.Month); break;
                case TopStatus.Month:
                    LoadTopStatus(TopStatus.Year); break;
            }
        }

        /// <summary>Button_today_Click 方法。</summary>
        private void Button_today_Click(object sender, EventArgs e)
        {
            MainDateTimePicker.Value = DateTime.Now;
            this.Close();
        }

        /// <summary>btn_confirm_Click 方法。</summary>
        private void btn_confirm_Click(object sender, EventArgs e)
        {
            var time = hTimePicker1.Time;
            MainDateTimePicker.Value = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, time.Hour, time.Minute, time.Second);
            this.Close();
        }
    }
}
