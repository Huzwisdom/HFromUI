using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HFrom.Date
{
    using HFromUI.HLangage;
    [DefaultEvent("DayClick")]
    internal partial class HDateDayPanel : UserControl
    {
        /// <summary>Buttons 字段：43 个日期格（上月尾 + 当月 + 下月头）。</summary>
        private readonly List<HPushButton> Buttons = new List<HPushButton>();

        /// <summary>datetime 字段。</summary>
        private DateTime datetime = DateTime.Now;

        /// <summary>selectIndex 字段。</summary>
        private int selectIndex = 0;

        public DateTime DateTime
        {
            get { return datetime; }
            set
            {
                datetime = value;
                selectIndex = 0;
                ClearSelection();
                // 选中当月当天格
                var btn = Buttons.FirstOrDefault(b => b.Enabled && b.Text == datetime.Day.ToString());
                if (btn != null) MarkSelected(btn);
            }
        }

        public HDateDayPanel()
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
            for (int i = 0; i <= 42; i++)
            {
                var button = new HPushButton
                {
                    Size = new Size(30, 30),
                    Margin = new Padding(10, 2, 10, 2),
                    ButtonStyle = HPushButtonStyle.Custom,
                    Shape = HPushButtonShape.Round,
                    Radius = 4,
                    Cursor = Cursors.Hand,
                    Text = "0"
                };
                // 悬停蓝边、按下蓝底白字
                button.HoverBackColor = Color.White;
                button.HoverBorderColor = Color.DodgerBlue;
                button.PressedBackColor = Color.DodgerBlue;
                button.PressedBorderColor = Color.DodgerBlue;
                button.PressedForeColor = Color.White;
                Buttons.Add(button);
                flowLayoutPanel1.Controls.Add(button);
                button.Click += Button_Click;
            }
            LoadMonth(0);
        }

        public event EventHandler<DayClickEventArgs> DayClick;

        /// <summary>Button_Click 方法。</summary>
        private void Button_Click(object sender, EventArgs e)
        {
            var btn = sender as HPushButton;
            ClearSelection();
            MarkSelected(btn);
            int day = Convert.ToInt32(btn.Text);
            if (DayClick != null)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    DayClickEventArgs ee = new DayClickEventArgs(new DateTime(DateTime.Year, DateTime.Month, day));
                    DayClick(this, ee);
                }));
            }
        }

        /// <summary>清除全部格子的选中态（蓝字蓝边 → 黑字灰边）。</summary>
        private void ClearSelection()
        {
            Buttons.ForEach(b =>
            {
                b.NormalBorderColor = Color.LightGray;
                if (b.Enabled) b.NormalForeColor = Color.Black;
            });
        }

        /// <summary>标记选中格：蓝字蓝边。</summary>
        private static void MarkSelected(HPushButton b)
        {
            b.NormalBorderColor = Color.DodgerBlue;
            b.NormalForeColor = Color.DodgerBlue;
        }

        /// <summary>LoadMonth 方法。</summary>
        public void LoadMonth(int index)
        {
            selectIndex += index;

            DateTime dateTime = this.DateTime.AddMonths(selectIndex);
            DateTime lastMonth = dateTime.Year==1&&dateTime.Month==1?dateTime:dateTime.AddMonths(-1);
            DateTime nextMonth = dateTime.AddMonths(1);

            int lastDayCount = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);
            int nextDayCount = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            int DayCount = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

            DateTime firstDay = new DateTime(dateTime.Year, dateTime.Month, 1);
            int lastCount = 0;
            switch (firstDay.DayOfWeek)
            {
                case DayOfWeek.Sunday: lastCount = 7; break;
                case DayOfWeek.Monday: lastCount = 1; break;
                case DayOfWeek.Tuesday: lastCount = 2; break;
                case DayOfWeek.Wednesday: lastCount = 3; break;
                case DayOfWeek.Thursday: lastCount = 4; break;
                case DayOfWeek.Friday: lastCount = 5; break;
                case DayOfWeek.Saturday: lastCount = 6; break;
            }

            for (int i = 0; i < lastCount; i++)
            {
                Buttons[i].Enabled = false;
                Buttons[i].NormalBackColor = Color.LightGray;
                Buttons[i].Text = dateTime.Year == 1 && dateTime.Month == 1 ? "":(lastDayCount - lastCount + 1 + i).ToString();
            }

            for (int i = lastCount; i < DayCount + lastCount; i++)
            {
                Buttons[i].Enabled = true;
                Buttons[i].NormalBackColor = Color.White;
                Buttons[i].NormalBorderColor = Color.LightGray;
                Buttons[i].Text = (i - lastCount + 1).ToString();
                // 当天蓝字
                Buttons[i].NormalForeColor = i - lastCount + 1 == DateTime.Day ? Color.DodgerBlue : Color.Black;
                Buttons[i].HoverForeColor = Buttons[i].NormalForeColor;
            }

            for (int i = lastCount + DayCount; i < Buttons.Count; i++)
            {
                Buttons[i].Enabled = false;
                Buttons[i].NormalBackColor = Color.LightGray;
                Buttons[i].Text = (i - lastCount - DayCount + 1).ToString();
            }
            DateTime = dateTime;
        }

        /// <summary>设置 lang。</summary>
        public void SetLang()
        {
            hLabel1.Text = HTranslation.GetContent("星期日");
            hLabel2.Text = HTranslation.GetContent("星期一");
            hLabel3.Text = HTranslation.GetContent("星期二");
            hLabel4.Text = HTranslation.GetContent("星期三");
            hLabel5.Text = HTranslation.GetContent("星期四");
            hLabel6.Text = HTranslation.GetContent("星期五");
            hLabel7.Text = HTranslation.GetContent("星期六");
        }
    }

    public class DayClickEventArgs : EventArgs
    {
        public DayClickEventArgs(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public DateTime DateTime;
    }
}
