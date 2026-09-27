namespace HFromUI.HFrom.Date
{
    using HFromUI.HFrom.Text;
    partial class HTimePicker
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
        #region 组件设计器生成的代码
        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_hourbase = new System.Windows.Forms.Panel();
            this.panel_hourSelect = new System.Windows.Forms.Panel();
            this.panel_minBase = new System.Windows.Forms.Panel();
            this.panel_minSelect = new System.Windows.Forms.Panel();
            this.panel_secBase = new System.Windows.Forms.Panel();
            this.panel_secSelect = new System.Windows.Forms.Panel();
            this.hLabel1 = new HLabel();
            this.hLabel2 = new HLabel();
            this.panel_hourbase.SuspendLayout();
            this.panel_minBase.SuspendLayout();
            this.panel_secBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_hourbase
            // 
            this.panel_hourbase.Controls.Add(this.panel_hourSelect);
            this.panel_hourbase.Location = new System.Drawing.Point(0, 0);
            this.panel_hourbase.Name = "panel_hourbase";
            this.panel_hourbase.Size = new System.Drawing.Size(25, 30);
            this.panel_hourbase.TabIndex = 0;
            // 
            // panel_hourSelect
            // 
            this.panel_hourSelect.Location = new System.Drawing.Point(0, 0);
            this.panel_hourSelect.Name = "panel_hourSelect";
            this.panel_hourSelect.Size = new System.Drawing.Size(25, 720);
            this.panel_hourSelect.TabIndex = 0;
            this.panel_hourSelect.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_hourSelect_Paint);
            // 
            // panel_minBase
            // 
            this.panel_minBase.Controls.Add(this.panel_minSelect);
            this.panel_minBase.Location = new System.Drawing.Point(43, 0);
            this.panel_minBase.Name = "panel_minBase";
            this.panel_minBase.Size = new System.Drawing.Size(25, 30);
            this.panel_minBase.TabIndex = 1;
            // 
            // panel_minSelect
            // 
            this.panel_minSelect.Location = new System.Drawing.Point(0, 0);
            this.panel_minSelect.Name = "panel_minSelect";
            this.panel_minSelect.Size = new System.Drawing.Size(25, 1800);
            this.panel_minSelect.TabIndex = 0;
            this.panel_minSelect.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_minSelect_Paint);
            // 
            // panel_secBase
            // 
            this.panel_secBase.Controls.Add(this.panel_secSelect);
            this.panel_secBase.Location = new System.Drawing.Point(85, 0);
            this.panel_secBase.Name = "panel_secBase";
            this.panel_secBase.Size = new System.Drawing.Size(25, 30);
            this.panel_secBase.TabIndex = 2;
            // 
            // panel_secSelect
            // 
            this.panel_secSelect.Location = new System.Drawing.Point(0, 0);
            this.panel_secSelect.Name = "panel_secSelect";
            this.panel_secSelect.Size = new System.Drawing.Size(25, 1800);
            this.panel_secSelect.TabIndex = 0;
            this.panel_secSelect.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_secSelect_Paint);
            // 
            // hLabel1
            // 
            this.hLabel1.AutoSize = true;
            this.hLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hLabel1.Location = new System.Drawing.Point(29, 5);
            this.hLabel1.Name = "hLabel1";
            this.hLabel1.Size = new System.Drawing.Size(11, 17);
            this.hLabel1.TabIndex = 3;
            this.hLabel1.Text = ":";
            this.hLabel1.TextDrawMode = HEnum.HDrawMode.Clear;
            // 
            // hLabel2
            // 
            this.hLabel2.AutoSize = true;
            this.hLabel2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hLabel2.Location = new System.Drawing.Point(71, 5);
            this.hLabel2.Name = "hLabel2";
            this.hLabel2.Size = new System.Drawing.Size(11, 17);
            this.hLabel2.TabIndex = 4;
            this.hLabel2.Text = ":";
            this.hLabel2.TextDrawMode =HEnum.HDrawMode.Clear;
            // 
            // PPTimePicker
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.hLabel2);
            this.Controls.Add(this.hLabel1);
            this.Controls.Add(this.panel_secBase);
            this.Controls.Add(this.panel_minBase);
            this.Controls.Add(this.panel_hourbase);
            this.Name = "PPTimePicker";
            this.Size = new System.Drawing.Size(110, 30);
            this.panel_hourbase.ResumeLayout(false);
            this.panel_minBase.ResumeLayout(false);
            this.panel_secBase.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        private System.Windows.Forms.Panel panel_hourbase;
        private System.Windows.Forms.Panel panel_hourSelect;
        private System.Windows.Forms.Panel panel_minBase;
        private System.Windows.Forms.Panel panel_minSelect;
        private System.Windows.Forms.Panel panel_secBase;
        private System.Windows.Forms.Panel panel_secSelect;
        private HLabel hLabel1;
        private HLabel hLabel2;
    }
}
