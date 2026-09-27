
using HFromUI.HFrom.Bars;
namespace HFromUI.HFrom.Combbox
{
    partial class HComboboxDropForm
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
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.hvScrollBarExt1 = new HVScrollBarExt();
            this.SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(155, 210);
            this.flowLayoutPanel.TabIndex = 0;
            // 
            // hvScrollBarExt1
            // 
            this.hvScrollBarExt1.BackColor = System.Drawing.Color.Transparent;
            this.hvScrollBarExt1.BaseColor = System.Drawing.Color.Gray;
            this.hvScrollBarExt1.BaseRadius = 0;
            this.hvScrollBarExt1.BaseSize = 1;
            this.hvScrollBarExt1.ButtonColor = System.Drawing.Color.DodgerBlue;
            this.hvScrollBarExt1.ButtonHoverColor = System.Drawing.Color.DeepSkyBlue;
            this.hvScrollBarExt1.ButtonRadius = 5;
            this.hvScrollBarExt1.ButtonSize = 5;
            this.hvScrollBarExt1.Dock = System.Windows.Forms.DockStyle.Right;
            this.hvScrollBarExt1.LargeChange = 10;
            this.hvScrollBarExt1.Location = new System.Drawing.Point(141, 3);
            this.hvScrollBarExt1.Maximum = 100;
            this.hvScrollBarExt1.Minimum = 0;
            this.hvScrollBarExt1.Name = "hvScrollBarExt1";
            this.hvScrollBarExt1.Size = new System.Drawing.Size(17, 210);
            this.hvScrollBarExt1.SmallChange = 1;
            this.hvScrollBarExt1.TabIndex = 1;
            this.hvScrollBarExt1.Value = 0;
            this.hvScrollBarExt1.VisibleValue = 10;
            // 
            // PPComboboxDropForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BorderColor = System.Drawing.Color.Red;
            this.ClientSize = new System.Drawing.Size(161, 216);
            this.Controls.Add(this.hvScrollBarExt1);
            this.Controls.Add(this.flowLayoutPanel);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PPComboboxDropForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Radius = 5;
            this.ShowBorder = true;
            this.Text = "PPComboboxDropForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private HVScrollBarExt hvScrollBarExt1;
    }
}
