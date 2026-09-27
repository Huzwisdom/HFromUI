
using HFromUI.HFrom.Text;
namespace HFromUI.HFrom.Date
{
    partial class HDateYearPanel
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
            this.hLabel1 = new HLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // hLabel1
            // 
            this.hLabel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.hLabel1.Location = new System.Drawing.Point(0, 0);
            this.hLabel1.Margin = new System.Windows.Forms.Padding(0);
            this.hLabel1.Name = "hLabel1";
            this.hLabel1.Size = new System.Drawing.Size(350, 15);
            this.hLabel1.TabIndex = 0;
            this.hLabel1.Text = "年份";
            this.hLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.hLabel1.TextDrawMode = HEnum.HDrawMode.Clear;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 15);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(350, 205);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // PPDateYearPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.hLabel1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "PPDateYearPanel";
            this.Size = new System.Drawing.Size(350, 220);
            this.ResumeLayout(false);

        }

        #endregion

        private HLabel hLabel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
