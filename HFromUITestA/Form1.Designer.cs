namespace HFromUITestA
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.hLabelBase1 = new HFromUI.HControl.Base.HLabelBase();
            this.buttonTaskTest = new System.Windows.Forms.Button();
            this.buttonHoldTest = new System.Windows.Forms.Button();
            this.textBoxTaskLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // hLabelBase1
            // 
            this.hLabelBase1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hLabelBase1.BorderWidth = 4;
            this.hLabelBase1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hLabelBase1.Location = new System.Drawing.Point(620, 380);
            this.hLabelBase1.Name = "hLabelBase1";
            this.hLabelBase1.Radius = 12;
            this.hLabelBase1.Size = new System.Drawing.Size(100, 30);
            this.hLabelBase1.TabIndex = 0;
            this.hLabelBase1.Text = "hLabelBase1";
            this.hLabelBase1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.hLabelBase1.UseMnemonic = false;
            this.hLabelBase1.Click += new System.EventHandler(this.hLabelBase1_Click);
            // 
            // buttonTaskTest
            // 
            this.buttonTaskTest.Location = new System.Drawing.Point(12, 12);
            this.buttonTaskTest.Name = "buttonTaskTest";
            this.buttonTaskTest.Size = new System.Drawing.Size(560, 30);
            this.buttonTaskTest.TabIndex = 1;
            this.buttonTaskTest.Text = "运行 HTaskStart 调度测试：A→B→{C,D}→{E,F,G}→{H,I,J,K}，H→L，{I,J,K,L}→M";
            this.buttonTaskTest.UseVisualStyleBackColor = true;
            this.buttonTaskTest.Click += new System.EventHandler(this.buttonTaskTest_Click);
            //
            // buttonHoldTest
            //
            this.buttonHoldTest.Location = new System.Drawing.Point(578, 12);
            this.buttonHoldTest.Name = "buttonHoldTest";
            this.buttonHoldTest.Size = new System.Drawing.Size(142, 30);
            this.buttonHoldTest.TabIndex = 3;
            this.buttonHoldTest.Text = "开关/分组测试";
            this.buttonHoldTest.UseVisualStyleBackColor = true;
            this.buttonHoldTest.Click += new System.EventHandler(this.buttonHoldTest_Click);
            //
            // textBoxTaskLog
            //
            this.textBoxTaskLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxTaskLog.Location = new System.Drawing.Point(12, 48);
            this.textBoxTaskLog.Multiline = true;
            this.textBoxTaskLog.Name = "textBoxTaskLog";
            this.textBoxTaskLog.ReadOnly = true;
            this.textBoxTaskLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTaskLog.Size = new System.Drawing.Size(708, 360);
            this.textBoxTaskLog.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBoxTaskLog);
            this.Controls.Add(this.buttonHoldTest);
            this.Controls.Add(this.buttonTaskTest);
            this.Controls.Add(this.hLabelBase1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private HFromUI.HControl.Base.HLabelBase hLabelBase1;
        private System.Windows.Forms.Button buttonTaskTest;
        private System.Windows.Forms.Button buttonHoldTest;
        private System.Windows.Forms.TextBox textBoxTaskLog;
    }
}

