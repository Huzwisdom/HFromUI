using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HControl.Coordinate
{
    partial class SetDecimalPlaces
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
            this.components = new System.ComponentModel.Container();
            this.lbl_title = new System.Windows.Forms.Label();
            this.lbl_show = new System.Windows.Forms.Label();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.time_show = new System.Windows.Forms.Timer(this.components);
            this.lbl_save2 = new System.Windows.Forms.Label();
            this.lbl_save1 = new System.Windows.Forms.Label();
            this.lbl_save3 = new System.Windows.Forms.Label();
            this.lbl_save4 = new System.Windows.Forms.Label();
            this.lbl_save5 = new System.Windows.Forms.Label();
            this.cb_save2 = new HRadioButton();
            this.cb_save5 = new HRadioButton();
            this.cb_save4 = new HRadioButton();
            this.cb_save3 = new HRadioButton();
            this.cb_save1 = new HRadioButton();
            this.SuspendLayout();
            //
            // lbl_title
            //
            this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_title.BackColor = System.Drawing.Color.SandyBrown;
            this.lbl_title.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lbl_title.Location = new System.Drawing.Point(0, 0);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(419, 38);
            this.lbl_title.TabIndex = 0;
            this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lbl_show
            //
            this.lbl_show.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_show.BackColor = System.Drawing.Color.Green;
            this.lbl_show.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.lbl_show.Location = new System.Drawing.Point(0, 38);
            this.lbl_show.Name = "lbl_show";
            this.lbl_show.Size = new System.Drawing.Size(419, 233);
            this.lbl_show.TabIndex = 1;
            this.lbl_show.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btn_OK
            //
            this.btn_OK.Font = new System.Drawing.Font("微软雅黑", 7F);
            this.btn_OK.Location = new System.Drawing.Point(41, 276);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(131, 33);
            this.btn_OK.TabIndex = 2;
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            //
            // btn_Close
            //
            this.btn_Close.Font = new System.Drawing.Font("微软雅黑", 7F);
            this.btn_Close.Location = new System.Drawing.Point(236, 276);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(131, 33);
            this.btn_Close.TabIndex = 3;
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            //
            // time_show
            //
            this.time_show.Interval = 500;
            this.time_show.Tick += new System.EventHandler(this.time_show_Tick);
            //
            // lbl_save2
            //
            this.lbl_save2.BackColor = System.Drawing.Color.Green;
            this.lbl_save2.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_save2.Location = new System.Drawing.Point(78, 96);
            this.lbl_save2.Name = "lbl_save2";
            this.lbl_save2.Size = new System.Drawing.Size(182, 30);
            this.lbl_save2.TabIndex = 9;
            this.lbl_save2.Text = "保留2位小数：";
            this.lbl_save2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lbl_save1
            //
            this.lbl_save1.BackColor = System.Drawing.Color.Green;
            this.lbl_save1.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_save1.Location = new System.Drawing.Point(78, 54);
            this.lbl_save1.Name = "lbl_save1";
            this.lbl_save1.Size = new System.Drawing.Size(182, 30);
            this.lbl_save1.TabIndex = 8;
            this.lbl_save1.Text = "保留1位小数：";
            this.lbl_save1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lbl_save3
            //
            this.lbl_save3.BackColor = System.Drawing.Color.Green;
            this.lbl_save3.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_save3.Location = new System.Drawing.Point(78, 137);
            this.lbl_save3.Name = "lbl_save3";
            this.lbl_save3.Size = new System.Drawing.Size(182, 30);
            this.lbl_save3.TabIndex = 12;
            this.lbl_save3.Text = "保留3位小数：";
            this.lbl_save3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lbl_save4
            //
            this.lbl_save4.BackColor = System.Drawing.Color.Green;
            this.lbl_save4.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_save4.Location = new System.Drawing.Point(78, 179);
            this.lbl_save4.Name = "lbl_save4";
            this.lbl_save4.Size = new System.Drawing.Size(182, 30);
            this.lbl_save4.TabIndex = 14;
            this.lbl_save4.Text = "保留4位小数：";
            this.lbl_save4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lbl_save5
            //
            this.lbl_save5.BackColor = System.Drawing.Color.Green;
            this.lbl_save5.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_save5.Location = new System.Drawing.Point(78, 220);
            this.lbl_save5.Name = "lbl_save5";
            this.lbl_save5.Size = new System.Drawing.Size(182, 30);
            this.lbl_save5.TabIndex = 16;
            this.lbl_save5.Text = "保留5位小数：";
            this.lbl_save5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // 五个小数位数选项：同组单选，矢量自绘，不再使用旧 HCheckBox
            //
            ConfigRadio(this.cb_save1, 267, 54, 10, this.cb_save1_CheckedChanged);
            ConfigRadio(this.cb_save2, 267, 96, 20, this.cb_save2_CheckedChanged);
            ConfigRadio(this.cb_save3, 267, 137, 17, this.cb_save3_CheckedChanged);
            ConfigRadio(this.cb_save4, 267, 179, 18, this.cb_save4_CheckedChanged);
            ConfigRadio(this.cb_save5, 267, 220, 19, this.cb_save5_CheckedChanged);
            //
            // SetDecimalPlaces
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(415, 319);
            this.Controls.Add(this.cb_save2);
            this.Controls.Add(this.cb_save5);
            this.Controls.Add(this.cb_save4);
            this.Controls.Add(this.cb_save3);
            this.Controls.Add(this.lbl_save5);
            this.Controls.Add(this.lbl_save4);
            this.Controls.Add(this.lbl_save3);
            this.Controls.Add(this.cb_save1);
            this.Controls.Add(this.lbl_save2);
            this.Controls.Add(this.lbl_save1);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.lbl_show);
            this.Controls.Add(this.lbl_title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetDecimalPlaces";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.ZxMessageShow_Load);
            this.ResumeLayout(false);

        }

        /// <summary>统一配置小数位单选按钮：绿底白字、同组互斥。</summary>
        private static void ConfigRadio(HRadioButton rb, int x, int y, int tab, System.EventHandler onCheckedChanged)
        {
            rb.GroupName = "DecimalPlaces";
            rb.BackColor = System.Drawing.Color.Green;
            rb.ForeColor = System.Drawing.Color.White;
            rb.Checked = false;
            rb.Location = new System.Drawing.Point(x, y);
            rb.Name = "cb_save" + (tab == 10 ? 1 : tab == 20 ? 2 : tab - 15);
            rb.Size = new System.Drawing.Size(59, 30);
            rb.TabIndex = tab;
            rb.CheckedChanged += onCheckedChanged;
        }

        #endregion

        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_show;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Timer time_show;
        private HRadioButton cb_save1;
        private System.Windows.Forms.Label lbl_save2;
        private System.Windows.Forms.Label lbl_save1;
        private System.Windows.Forms.Label lbl_save3;
        private System.Windows.Forms.Label lbl_save4;
        private System.Windows.Forms.Label lbl_save5;
        private HRadioButton cb_save3;
        private HRadioButton cb_save4;
        private HRadioButton cb_save5;
        private HRadioButton cb_save2;
    }
}
