namespace HFromUITestB
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.hPushButton2 = new HFromUI.HControl.Tools.Button.HPushButton();
            this.hPushButton1 = new HFromUI.HControl.Tools.Button.HPushButton();
            this.hAffixTextBox2 = new HFromUI.HControl.Tools.Editors.HAffixTextBox();
            this.hAffixTextBox1 = new HFromUI.HControl.Tools.Editors.HAffixTextBox();
            this.hChip1 = new HFromUI.HControl.Tools.Vessel.HChip();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(524, 45);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(264, 364);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // hPushButton2
            // 
            this.hPushButton2.BackColor = System.Drawing.Color.Transparent;
            this.hPushButton2.ButtonStyle = HFromUI.HControl.Tools.Button.HPushButtonStyle.Secondary;
            this.hPushButton2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.hPushButton2.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hPushButton2.Glyph = HFromUI.HControl.Tools.Button.HPushButtonGlyph.Stop;
            this.hPushButton2.HoverBackColor = System.Drawing.Color.Empty;
            this.hPushButton2.HoverBorderColor = System.Drawing.Color.Empty;
            this.hPushButton2.HoverForeColor = System.Drawing.Color.Empty;
            this.hPushButton2.HoverImage = null;
            this.hPushButton2.Image = ((System.Drawing.Image)(resources.GetObject("hPushButton2.Image")));
            this.hPushButton2.ImageSize = 90;
            this.hPushButton2.Location = new System.Drawing.Point(280, 178);
            this.hPushButton2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.hPushButton2.Name = "hPushButton2";
            this.hPushButton2.NormalBackColor = System.Drawing.Color.Empty;
            this.hPushButton2.NormalBorderColor = System.Drawing.Color.Empty;
            this.hPushButton2.NormalForeColor = System.Drawing.Color.Empty;
            this.hPushButton2.PressedBackColor = System.Drawing.Color.Empty;
            this.hPushButton2.PressedBorderColor = System.Drawing.Color.Empty;
            this.hPushButton2.PressedForeColor = System.Drawing.Color.Empty;
            this.hPushButton2.Radius = 90;
            this.hPushButton2.Size = new System.Drawing.Size(208, 97);
            this.hPushButton2.TabIndex = 4;
            this.hPushButton2.Text = "停止";
            this.hPushButton2.TipScheme = HFromUI.HControl.Chart.Tips.HTipSchemeKind.Forest;
            this.hPushButton2.Click += new System.EventHandler(this.hPushButton2_Click);
            // 
            // hPushButton1
            // 
            this.hPushButton1.BackColor = System.Drawing.Color.Transparent;
            this.hPushButton1.ButtonStyle = HFromUI.HControl.Tools.Button.HPushButtonStyle.GradientEmerald;
            this.hPushButton1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.hPushButton1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hPushButton1.Glyph = HFromUI.HControl.Tools.Button.HPushButtonGlyph.Play;
            this.hPushButton1.HoverBackColor = System.Drawing.Color.Empty;
            this.hPushButton1.HoverBorderColor = System.Drawing.Color.Empty;
            this.hPushButton1.HoverForeColor = System.Drawing.Color.Empty;
            this.hPushButton1.HoverImage = null;
            this.hPushButton1.Image = ((System.Drawing.Image)(resources.GetObject("hPushButton1.Image")));
            this.hPushButton1.ImageSize = 90;
            this.hPushButton1.Location = new System.Drawing.Point(54, 178);
            this.hPushButton1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.hPushButton1.Name = "hPushButton1";
            this.hPushButton1.NormalBackColor = System.Drawing.Color.Empty;
            this.hPushButton1.NormalBorderColor = System.Drawing.Color.Empty;
            this.hPushButton1.NormalForeColor = System.Drawing.Color.Empty;
            this.hPushButton1.PressedBackColor = System.Drawing.Color.Empty;
            this.hPushButton1.PressedBorderColor = System.Drawing.Color.Empty;
            this.hPushButton1.PressedForeColor = System.Drawing.Color.Empty;
            this.hPushButton1.Radius = 90;
            this.hPushButton1.Shape = HFromUI.HControl.Tools.Button.HPushButtonShape.Capsule;
            this.hPushButton1.Size = new System.Drawing.Size(208, 97);
            this.hPushButton1.TabIndex = 3;
            this.hPushButton1.Text = "运行";
            this.hPushButton1.TipScheme = HFromUI.HControl.Chart.Tips.HTipSchemeKind.Forest;
            this.hPushButton1.Click += new System.EventHandler(this.hPushButton1_Click_1);
            // 
            // hAffixTextBox2
            // 
            this.hAffixTextBox2.BackColor = System.Drawing.SystemColors.Window;
            this.hAffixTextBox2.BorderWidth = 0;
            this.hAffixTextBox2.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hAffixTextBox2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.hAffixTextBox2.Location = new System.Drawing.Point(192, 100);
            this.hAffixTextBox2.Name = "hAffixTextBox2";
            this.hAffixTextBox2.PrefixAlign = HFromUI.HControl.Tools.Editors.HAffixVerticalAlign.Bottom;
            this.hAffixTextBox2.PrefixText = "Port:";
            this.hAffixTextBox2.Radius = 1;
            this.hAffixTextBox2.SelectionLength = 0;
            this.hAffixTextBox2.SelectionStart = 0;
            this.hAffixTextBox2.Size = new System.Drawing.Size(296, 42);
            this.hAffixTextBox2.TabIndex = 2;
            this.hAffixTextBox2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.hAffixTextBox2.UnderlineActiveColor = System.Drawing.Color.DodgerBlue;
            this.hAffixTextBox2.UnderlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.hAffixTextBox2.UnderlineHoverColor = System.Drawing.Color.DodgerBlue;
            this.hAffixTextBox2.WatermarkColor = System.Drawing.Color.Gray;
            this.hAffixTextBox2.TextChanged += new System.EventHandler(this.hAffixTextBox2_TextChanged);
            // 
            // hAffixTextBox1
            // 
            this.hAffixTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.hAffixTextBox1.BorderWidth = 0;
            this.hAffixTextBox1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hAffixTextBox1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.hAffixTextBox1.Location = new System.Drawing.Point(192, 45);
            this.hAffixTextBox1.Name = "hAffixTextBox1";
            this.hAffixTextBox1.PrefixAlign = HFromUI.HControl.Tools.Editors.HAffixVerticalAlign.Bottom;
            this.hAffixTextBox1.PrefixText = "IP:";
            this.hAffixTextBox1.Radius = 1;
            this.hAffixTextBox1.SelectionLength = 0;
            this.hAffixTextBox1.SelectionStart = 0;
            this.hAffixTextBox1.Size = new System.Drawing.Size(296, 42);
            this.hAffixTextBox1.TabIndex = 1;
            this.hAffixTextBox1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.hAffixTextBox1.UnderlineActiveColor = System.Drawing.Color.DodgerBlue;
            this.hAffixTextBox1.UnderlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.hAffixTextBox1.UnderlineHoverColor = System.Drawing.Color.DodgerBlue;
            this.hAffixTextBox1.WatermarkColor = System.Drawing.Color.Gray;
            this.hAffixTextBox1.TextChanged += new System.EventHandler(this.hAffixTextBox1_TextChanged);
            // 
            // hChip1
            // 
            this.hChip1.BackColor = System.Drawing.Color.Transparent;
            this.hChip1.ChipStyle = HFromUI.HControl.Tools.Vessel.HChipStyle.PLCC;
            this.hChip1.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.hChip1.Location = new System.Drawing.Point(25, 12);
            this.hChip1.Name = "hChip1";
            this.hChip1.Size = new System.Drawing.Size(171, 161);
            this.hChip1.TabIndex = 0;
            this.hChip1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.hChip1.Theme = HFromUI.HControl.Base.HToolTheme.Coffee;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.hPushButton2);
            this.Controls.Add(this.hPushButton1);
            this.Controls.Add(this.hAffixTextBox2);
            this.Controls.Add(this.hAffixTextBox1);
            this.Controls.Add(this.hChip1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);

        }

        #endregion

        private HFromUI.HControl.Tools.Vessel.HChip hChip1;
        private HFromUI.HControl.Tools.Editors.HAffixTextBox hAffixTextBox1;
        private HFromUI.HControl.Tools.Editors.HAffixTextBox hAffixTextBox2;
        private HFromUI.HControl.Tools.Button.HPushButton hPushButton1;
        private HFromUI.HControl.Tools.Button.HPushButton hPushButton2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Timer timer1;
    }
}