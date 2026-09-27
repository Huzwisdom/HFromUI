namespace HFromUI.HFrom.Combbox
{
    partial class HTreeComboBox
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
        private new void InitializeComponent()
        {
            this.txtDisplay = new System.Windows.Forms.TextBox();
            this.btnArrow = new System.Windows.Forms.Button();
            this.innerTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            //
            // txtDisplay
            //
            this.txtDisplay.BackColor = System.Drawing.Color.White;
            this.txtDisplay.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDisplay.Location = new System.Drawing.Point(6, 9);
            this.txtDisplay.Name = "txtDisplay";
            this.txtDisplay.ReadOnly = true;
            this.txtDisplay.Size = new System.Drawing.Size(170, 14);
            this.txtDisplay.TabIndex = 0;
            this.txtDisplay.Click += new System.EventHandler(this.DisplayArea_Click);
            //
            // btnArrow
            //
            this.btnArrow.BackColor = System.Drawing.Color.Transparent;
            this.btnArrow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnArrow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArrow.FlatAppearance.BorderSize = 0;
            this.btnArrow.Location = new System.Drawing.Point(182, 6);
            this.btnArrow.Name = "btnArrow";
            this.btnArrow.Size = new System.Drawing.Size(14, 20);
            this.btnArrow.TabIndex = 1;
            this.btnArrow.Text = "▾";
            this.btnArrow.Click += new System.EventHandler(this.DisplayArea_Click);
            //
            // innerTreeView
            //
            this.innerTreeView.Name = "innerTreeView";
            this.innerTreeView.Size = new System.Drawing.Size(200, 100);
            this.innerTreeView.TabIndex = 2;
            this.innerTreeView.Visible = false;
            //
            // HTreeComboBox
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BaseColor = System.Drawing.Color.White;
            this.BorderColor = System.Drawing.Color.LightGray;
            this.BorderWidth = 1F;
            this.Controls.Add(this.btnArrow);
            this.Controls.Add(this.txtDisplay);
            this.Controls.Add(this.innerTreeView);
            this.Name = "HTreeComboBox";
            this.Radius = 4;
            this.ShowBorder = true;
            this.Size = new System.Drawing.Size(200, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDisplay;
        private System.Windows.Forms.Button btnArrow;
        private System.Windows.Forms.TreeView innerTreeView;
    }
}
