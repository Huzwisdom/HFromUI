namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HFrom.Text;
    using HFromUI.HFrom.Tables;
    partial class HTreeComboDropForm
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
        #region Windows Form Designer generated code
        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.searchBox = new HSearchBox();
            this.tree = new HTreeViewEx();
            this.SuspendLayout();
            //
            // searchBox
            //
            this.searchBox.BackColor = System.Drawing.Color.White;
            this.searchBox.BaseColor = System.Drawing.Color.White;
            this.searchBox.BorderColor = System.Drawing.Color.LightGray;
            this.searchBox.BorderWidth = 1F;
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchBox.Location = new System.Drawing.Point(0, 0);
            this.searchBox.Name = "searchBox";
            this.searchBox.Radius = 0;
            this.searchBox.ShowBorder = false;
            this.searchBox.Size = new System.Drawing.Size(200, 28);
            this.searchBox.TabIndex = 0;
            this.searchBox.Visible = false;
            //
            // tree
            //
            this.tree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.Location = new System.Drawing.Point(0, 28);
            this.tree.Name = "tree";
            this.tree.ShowPlusMinus = true;
            this.tree.Size = new System.Drawing.Size(200, 272);
            this.tree.TabIndex = 1;
            //
            // HTreeComboDropForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BorderColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(200, 300);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.searchBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HTreeComboDropForm";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Radius = 4;
            this.ShowBorder = true;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        private HSearchBox searchBox;
        private HTreeViewEx tree;
    }
}
