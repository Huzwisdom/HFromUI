using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HControl.Coordinate
{
    partial class HSetLayer
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
            this.lbl_enable = new System.Windows.Forms.Label();
            this.lbl_set = new System.Windows.Forms.Label();
            this.TB_set = new System.Windows.Forms.TextBox();
            this.cb_enable = new HSlideSwitch();
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
            this.lbl_title.Size = new System.Drawing.Size(425, 38);
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
            this.lbl_show.Size = new System.Drawing.Size(425, 99);
            this.lbl_show.TabIndex = 1;
            this.lbl_show.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("微软雅黑", 7F);
            this.btn_OK.Location = new System.Drawing.Point(45, 149);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(131, 33);
            this.btn_OK.TabIndex = 2;
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Font = new System.Drawing.Font("微软雅黑", 7F);
            this.btn_Close.Location = new System.Drawing.Point(240, 149);
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
            // lbl_enable
            // 
            this.lbl_enable.BackColor = System.Drawing.Color.Green;
            this.lbl_enable.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_enable.Location = new System.Drawing.Point(81, 51);
            this.lbl_enable.Name = "lbl_enable";
            this.lbl_enable.Size = new System.Drawing.Size(125, 30);
            this.lbl_enable.TabIndex = 4;
            this.lbl_enable.Text = "显示所有：";
            this.lbl_enable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_set
            // 
            this.lbl_set.BackColor = System.Drawing.Color.Green;
            this.lbl_set.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_set.Location = new System.Drawing.Point(81, 92);
            this.lbl_set.Name = "lbl_set";
            this.lbl_set.Size = new System.Drawing.Size(125, 30);
            this.lbl_set.TabIndex = 5;
            this.lbl_set.Text = "设置图层：";
            this.lbl_set.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TB_set
            // 
            this.TB_set.BackColor = System.Drawing.Color.Green;
            this.TB_set.Location = new System.Drawing.Point(204, 88);
            this.TB_set.MaxLength = 32767;
            this.TB_set.Name = "TB_set";
            this.TB_set.Size = new System.Drawing.Size(146, 35);
            this.TB_set.TabIndex = 7;
            this.TB_set.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.TB_set.TextChanged += new System.EventHandler(this.TB_set_TextChanged);
            //
            // cb_enable：滑动开关，矢量自绘
            //
            this.cb_enable.BackColor = System.Drawing.Color.Green;
            this.cb_enable.Checked = false;
            this.cb_enable.Location = new System.Drawing.Point(204, 52);
            this.cb_enable.Name = "cb_enable";
            this.cb_enable.Size = new System.Drawing.Size(59, 30);
            this.cb_enable.TabIndex = 6;
            this.cb_enable.CheckedChanged += new System.EventHandler(this.cb_enable_CheckedChanged);
            // 
            // HSetLayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(421, 195);
            this.Controls.Add(this.TB_set);
            this.Controls.Add(this.cb_enable);
            this.Controls.Add(this.lbl_set);
            this.Controls.Add(this.lbl_enable);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.lbl_show);
            this.Controls.Add(this.lbl_title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HSetLayer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.ZxMessageShow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_show;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.Timer time_show;
        private System.Windows.Forms.Label lbl_enable;
        private System.Windows.Forms.Label lbl_set;
        private HSlideSwitch cb_enable;
        private System.Windows.Forms.TextBox TB_set;
    }
}
