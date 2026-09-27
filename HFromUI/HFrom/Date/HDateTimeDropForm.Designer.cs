
using HFromUI.HEnum;
using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;


namespace HFromUI.HFrom.Date
{
    partial class HDateTimeDropForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.hDateYearPanel1 = new HDateYearPanel();
            this.hDateMonthPanel1 = new HDateMonthPanel();
            this.hDateDayPanel1 = new HDateDayPanel();
            this.Button_today = new HPushButton();
            this.Button_forward = new HPushButton();
            this.Button_backward = new HPushButton();
            this.Button_Top = new HPushButton();
            this.hTimePicker1 = new HTimePicker();
            this.btn_confirm = new HPushButton();
            this.SuspendLayout();
            // 
            // hDateYearPanel1
            // 
            this.hDateYearPanel1.BackColor = System.Drawing.Color.White;
            this.hDateYearPanel1.DateTime = new System.DateTime(2021, 3, 30, 15, 48, 54, 710);
            this.hDateYearPanel1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hDateYearPanel1.Location = new System.Drawing.Point(5, 40);
            this.hDateYearPanel1.Name = "hDateYearPanel1";
            this.hDateYearPanel1.Size = new System.Drawing.Size(350, 220);
            this.hDateYearPanel1.TabIndex = 6;
            this.hDateYearPanel1.YearClick += new System.EventHandler<YearClickEventArgs>(this.hDateYearPanel1_YearClick);
            // 
            // hDateMonthPanel1
            // 
            this.hDateMonthPanel1.BackColor = System.Drawing.Color.White;
            this.hDateMonthPanel1.DateTime = new System.DateTime(2021, 3, 30, 15, 48, 54, 716);
            this.hDateMonthPanel1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hDateMonthPanel1.Location = new System.Drawing.Point(5, 40);
            this.hDateMonthPanel1.Name = "hDateMonthPanel1";
            this.hDateMonthPanel1.Size = new System.Drawing.Size(350, 220);
            this.hDateMonthPanel1.TabIndex = 5;
            this.hDateMonthPanel1.Visible = false;
            this.hDateMonthPanel1.MonthClick += new System.EventHandler<MonthClickEventArgs>(this.hDateMonthPanel1_MonthClick);
            // 
            // hDateDayPanel1
            // 
            this.hDateDayPanel1.BackColor = System.Drawing.Color.White;
            this.hDateDayPanel1.DateTime = new System.DateTime(((long)(0)));
            this.hDateDayPanel1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hDateDayPanel1.Location = new System.Drawing.Point(5, 40);
            this.hDateDayPanel1.Name = "hDateDayPanel1";
            this.hDateDayPanel1.Size = new System.Drawing.Size(350, 219);
            this.hDateDayPanel1.TabIndex = 4;
            this.hDateDayPanel1.DayClick += new System.EventHandler<DayClickEventArgs>(this.hDateDayPanel1_DayClick);
            // Button_today
            this.Button_today.ButtonStyle = HPushButtonStyle.Custom;
            this.Button_today.Shape = HPushButtonShape.Round;
            this.Button_today.Radius = 3;
            this.Button_today.DialogResult = System.Windows.Forms.DialogResult.None;
            this.Button_today.Location = new System.Drawing.Point(15, 266);
            this.Button_today.Name = "Button_today";
            this.Button_today.NormalBackColor = System.Drawing.Color.White;
            this.Button_today.NormalBorderColor = System.Drawing.Color.LightGray;
            this.Button_today.HoverBackColor = System.Drawing.Color.LightGray;
            this.Button_today.HoverBorderColor = System.Drawing.Color.Silver;
            this.Button_today.PressedBackColor = System.Drawing.Color.LightGray;
            this.Button_today.PressedBorderColor = System.Drawing.Color.Silver;
            this.Button_today.NormalForeColor = System.Drawing.Color.Black;
            this.Button_today.Size = new System.Drawing.Size(114, 30);
            this.Button_today.TabIndex = 3;
            this.Button_today.Text = "现在:2021-03-30";
            this.Button_today.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Button_today.Click += new System.EventHandler(this.Button_today_Click);
            // Button_forward
            this.Button_forward.ButtonStyle = HPushButtonStyle.Custom;
            this.Button_forward.Shape = HPushButtonShape.Round;
            this.Button_forward.Radius = 3;
            this.Button_forward.DialogResult = System.Windows.Forms.DialogResult.None;
            this.Button_forward.Glyph = HPushButtonGlyph.ArrowRight;
            this.Button_forward.ImageSize = 12;
            this.Button_forward.HoverForeColor = System.Drawing.Color.DodgerBlue;
            this.Button_forward.PressedForeColor = System.Drawing.Color.DodgerBlue;
            this.Button_forward.Location = new System.Drawing.Point(261, 4);
            this.Button_forward.Name = "Button_forward";
            this.Button_forward.NormalBackColor = System.Drawing.Color.White;
            this.Button_forward.NormalBorderColor = System.Drawing.Color.LightGray;
            this.Button_forward.HoverBackColor = System.Drawing.Color.LightGray;
            this.Button_forward.HoverBorderColor = System.Drawing.Color.Silver;
            this.Button_forward.PressedBackColor = System.Drawing.Color.LightGray;
            this.Button_forward.PressedBorderColor = System.Drawing.Color.Silver;
            this.Button_forward.NormalForeColor = System.Drawing.Color.Black;
            this.Button_forward.Size = new System.Drawing.Size(30, 30);
            this.Button_forward.TabIndex = 2;
            this.Button_forward.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Button_forward.Click += new System.EventHandler(this.Button_forward_Click);
            // Button_backward
            this.Button_backward.ButtonStyle = HPushButtonStyle.Custom;
            this.Button_backward.Shape = HPushButtonShape.Round;
            this.Button_backward.Radius = 3;
            this.Button_backward.DialogResult = System.Windows.Forms.DialogResult.None;
            this.Button_backward.Glyph = HPushButtonGlyph.ArrowLeft;
            this.Button_backward.ImageSize = 12;
            this.Button_backward.HoverForeColor = System.Drawing.Color.DodgerBlue;
            this.Button_backward.PressedForeColor = System.Drawing.Color.DodgerBlue;
            this.Button_backward.Location = new System.Drawing.Point(69, 4);
            this.Button_backward.Name = "Button_backward";
            this.Button_backward.NormalBackColor = System.Drawing.Color.White;
            this.Button_backward.NormalBorderColor = System.Drawing.Color.LightGray;
            this.Button_backward.HoverBackColor = System.Drawing.Color.LightGray;
            this.Button_backward.HoverBorderColor = System.Drawing.Color.Silver;
            this.Button_backward.PressedBackColor = System.Drawing.Color.LightGray;
            this.Button_backward.PressedBorderColor = System.Drawing.Color.Silver;
            this.Button_backward.NormalForeColor = System.Drawing.Color.Black;
            this.Button_backward.Size = new System.Drawing.Size(30, 30);
            this.Button_backward.TabIndex = 1;
            this.Button_backward.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Button_backward.Click += new System.EventHandler(this.Button_backward_Click);
            // 
            // Button_Top
            // 
            this.Button_Top.ButtonStyle = HPushButtonStyle.Custom;
            this.Button_Top.Shape = HPushButtonShape.Round;
            this.Button_Top.Radius = 3;
            this.Button_Top.DialogResult = System.Windows.Forms.DialogResult.None;
            this.Button_Top.Location = new System.Drawing.Point(105, 4);
            this.Button_Top.Name = "Button_Top";
            this.Button_Top.NormalBackColor = System.Drawing.Color.White;
            this.Button_Top.NormalBorderColor = System.Drawing.Color.LightGray;
            this.Button_Top.HoverBackColor = System.Drawing.Color.LightGray;
            this.Button_Top.HoverBorderColor = System.Drawing.Color.Silver;
            this.Button_Top.PressedBackColor = System.Drawing.Color.LightGray;
            this.Button_Top.PressedBorderColor = System.Drawing.Color.Silver;
            this.Button_Top.NormalForeColor = System.Drawing.Color.Black;
            this.Button_Top.Size = new System.Drawing.Size(150, 30);
            this.Button_Top.TabIndex = 0;
            this.Button_Top.Text = "2021-03";
            this.Button_Top.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Button_Top.Click += new System.EventHandler(this.Button_Top_Click);
            // 
            // hTimePicker1
            // 
            this.hTimePicker1.BackColor = System.Drawing.Color.White;
            this.hTimePicker1.Location = new System.Drawing.Point(137, 267);
            this.hTimePicker1.Name = "hTimePicker1";
            this.hTimePicker1.Size = new System.Drawing.Size(118, 29);
            this.hTimePicker1.TabIndex = 7;
            this.hTimePicker1.TextDrawMode = HEnum.HDrawMode.Clear;
            this.hTimePicker1.Time = new System.DateTime(((long)(0)));
            this.hTimePicker1.UseAnime = true;
            // 
            // btn_confirm
            // 
            this.btn_confirm.ButtonStyle = HPushButtonStyle.Custom;
            this.btn_confirm.Shape = HPushButtonShape.Round;
            this.btn_confirm.Radius = 3;
            this.btn_confirm.DialogResult = System.Windows.Forms.DialogResult.None;
            this.btn_confirm.Location = new System.Drawing.Point(263, 265);
            this.btn_confirm.Name = "btn_confirm";
            this.btn_confirm.NormalBackColor = System.Drawing.Color.White;
            this.btn_confirm.NormalBorderColor = System.Drawing.Color.LightGray;
            this.btn_confirm.HoverBackColor = System.Drawing.Color.LightGray;
            this.btn_confirm.HoverBorderColor = System.Drawing.Color.Silver;
            this.btn_confirm.PressedBackColor = System.Drawing.Color.LightGray;
            this.btn_confirm.PressedBorderColor = System.Drawing.Color.Silver;
            this.btn_confirm.NormalForeColor = System.Drawing.Color.Black;
            this.btn_confirm.Size = new System.Drawing.Size(81, 30);
            this.btn_confirm.TabIndex = 8;
            this.btn_confirm.Text = "确定";
            this.btn_confirm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btn_confirm.Click += new System.EventHandler(this.btn_confirm_Click);
            // 
            // PPDateTimeDropForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(360, 301);
            this.Controls.Add(this.btn_confirm);
            this.Controls.Add(this.hTimePicker1);
            this.Controls.Add(this.hDateYearPanel1);
            this.Controls.Add(this.hDateMonthPanel1);
            this.Controls.Add(this.hDateDayPanel1);
            this.Controls.Add(this.Button_today);
            this.Controls.Add(this.Button_forward);
            this.Controls.Add(this.Button_backward);
            this.Controls.Add(this.Button_Top);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PPDateTimeDropForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Radius = 5;
            this.ShowBorder = true;
            this.Text = "PPComboboxDropForm";
            this.ResumeLayout(false);

        }

        #endregion

        private HPushButton Button_Top;
        private HPushButton Button_backward;
        private HPushButton Button_forward;
        private HPushButton Button_today;
        private HDateDayPanel hDateDayPanel1;
        private HDateMonthPanel hDateMonthPanel1;
        private HDateYearPanel hDateYearPanel1;
        private HTimePicker hTimePicker1;
        private HPushButton btn_confirm;
    }
}
