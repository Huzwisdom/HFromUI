namespace HFromUI.HFrom.Text
{
    using HFromUI.HControl.Tools.Bars;
    partial class HRichTextbox
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.hvScrollBarExt1 = new HVScrollBar();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Location = new System.Drawing.Point(14, 18);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(322, 286);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // hvScrollBarExt1
            // 
            this.hvScrollBarExt1.BackColor = System.Drawing.Color.Transparent;
            this.hvScrollBarExt1.BaseColor = System.Drawing.Color.Gainsboro;
            this.hvScrollBarExt1.BaseRadius = 0;
            this.hvScrollBarExt1.BaseSize = 2;
            this.hvScrollBarExt1.ButtonColor = System.Drawing.Color.DodgerBlue;
            this.hvScrollBarExt1.ButtonHoverColor = System.Drawing.Color.DeepSkyBlue;
            this.hvScrollBarExt1.ButtonRadius = 10;
            this.hvScrollBarExt1.ButtonSize = 10;
            this.hvScrollBarExt1.LargeChange = 10;
            this.hvScrollBarExt1.Location = new System.Drawing.Point(371, 18);
            this.hvScrollBarExt1.Maximum = 100;
            this.hvScrollBarExt1.Minimum = 0;
            this.hvScrollBarExt1.Name = "hvScrollBarExt1";
            this.hvScrollBarExt1.Size = new System.Drawing.Size(17, 367);
            this.hvScrollBarExt1.SmallChange = 1;
            this.hvScrollBarExt1.TabIndex = 1;
            this.hvScrollBarExt1.Value = 0;
            this.hvScrollBarExt1.VisibleValue = 10;
            // 
            // PPRichTextbox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BaseColor = System.Drawing.Color.White;
            this.BorderColor = System.Drawing.Color.Black;
            this.Controls.Add(this.hvScrollBarExt1);
            this.Controls.Add(this.richTextBox1);
            this.Name = "PPRichTextbox";
            this.Radius = 10;
            this.Size = new System.Drawing.Size(400, 400);
            this.BaseColorChanged += new System.EventHandler(this.PPRichTextbox_BaseColorChanged);
            this.ResumeLayout(false);
        }
        #endregion
        private System.Windows.Forms.RichTextBox richTextBox1;
        private HVScrollBar hvScrollBarExt1;
    }
}
