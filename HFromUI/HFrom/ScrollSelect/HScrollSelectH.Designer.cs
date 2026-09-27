namespace HFromUI.HFrom.ScrollSelect
{
    using HFromUI.HFrom.Base;
    partial class HScrollSelectH
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
            this.panel_Select = new HPanelBase();
            this.SuspendLayout();
            // 
            // panel_Select
            // 
            this.panel_Select.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.panel_Select.Location = new System.Drawing.Point(0, 0);
            this.panel_Select.Name = "panel_Select";
            this.panel_Select.Size = new System.Drawing.Size(120, 60);
            this.panel_Select.TabIndex = 0;
            this.panel_Select.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Select_Paint);
            // 
            // PPScrollSelectH
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel_Select);
            this.Name = "PPScrollSelectH";
            this.Size = new System.Drawing.Size(120, 60);
            this.ResumeLayout(false);
        }
        #endregion
        private HPanelBase panel_Select;
    }
}
