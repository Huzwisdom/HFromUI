
using HFromUI.HEnum;
using HFromUI.HControl;
using HFromUI.HFrom.Panel;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HFrom.ScreenShot
{
    partial class HScreenShotForm
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
            this.Panel_control = new HPanel();
            this.Button_COPY = new HPushButton();
            this.Button_SAVE = new HPushButton();
            this.Panel_control.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_control
            // 
            this.Panel_control.BackColor = System.Drawing.Color.White;
            this.Panel_control.BaseColor = System.Drawing.Color.White;
            this.Panel_control.BorderColor = System.Drawing.Color.LightGray;
            this.Panel_control.BorderWidth = 1;
            this.Panel_control.Controls.Add(this.Button_COPY);
            this.Panel_control.Controls.Add(this.Button_SAVE);
            this.Panel_control.Location = new System.Drawing.Point(513, 420);
            this.Panel_control.Name = "Panel_control";
            this.Panel_control.Radius = 0;
            this.Panel_control.RoundStyle = HEnum.HRoundStyle.All;
            this.Panel_control.ShowBorder = false;
            this.Panel_control.Size = new System.Drawing.Size(48, 24);
            this.Panel_control.TabIndex = 0;
            this.Panel_control.Visible = false;
            //
            // Button_COPY
            //
            this.Button_COPY.ButtonStyle = HPushButtonStyle.Custom;
            this.Button_COPY.Shape = HPushButtonShape.Rect;
            this.Button_COPY.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_COPY.DialogResult = System.Windows.Forms.DialogResult.None;
            this.Button_COPY.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Button_COPY.Glyph = HPushButtonGlyph.Check;
            this.Button_COPY.ImageSize = 16;
            this.Button_COPY.Location = new System.Drawing.Point(-1, -1);
            this.Button_COPY.Name = "Button_COPY";
            this.Button_COPY.NormalBackColor = System.Drawing.Color.White;
            this.Button_COPY.NormalBorderColor = System.Drawing.Color.White;
            this.Button_COPY.HoverBackColor = System.Drawing.Color.White;
            this.Button_COPY.HoverBorderColor = System.Drawing.Color.White;
            this.Button_COPY.PressedBackColor = System.Drawing.Color.White;
            this.Button_COPY.PressedBorderColor = System.Drawing.Color.White;
            this.Button_COPY.NormalForeColor = System.Drawing.Color.Black;
            this.Button_COPY.Size = new System.Drawing.Size(25, 25);
            this.Button_COPY.TabIndex = 1;
            this.Button_COPY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Button_COPY.Click += new System.EventHandler(this.Button_COPY_Click);
            //
            // Button_SAVE
            //
            this.Button_SAVE.ButtonStyle = HPushButtonStyle.Custom;
            this.Button_SAVE.Shape = HPushButtonShape.Rect;
            this.Button_SAVE.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_SAVE.DialogResult = System.Windows.Forms.DialogResult.None;
            this.Button_SAVE.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Button_SAVE.Glyph = HPushButtonGlyph.Save;
            this.Button_SAVE.ImageSize = 17;
            this.Button_SAVE.Location = new System.Drawing.Point(24, -1);
            this.Button_SAVE.Name = "Button_SAVE";
            this.Button_SAVE.NormalBackColor = System.Drawing.Color.White;
            this.Button_SAVE.NormalBorderColor = System.Drawing.Color.White;
            this.Button_SAVE.HoverBackColor = System.Drawing.Color.White;
            this.Button_SAVE.HoverBorderColor = System.Drawing.Color.White;
            this.Button_SAVE.PressedBackColor = System.Drawing.Color.White;
            this.Button_SAVE.PressedBorderColor = System.Drawing.Color.White;
            this.Button_SAVE.NormalForeColor = System.Drawing.Color.Black;
            this.Button_SAVE.Size = new System.Drawing.Size(25, 25);
            this.Button_SAVE.TabIndex = 0;
            this.Button_SAVE.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Button_SAVE.Click += new System.EventHandler(this.Button_SAVE_Click);
            // 
            // PPScreenShotForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.Panel_control);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PPScreenShotForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PPScreenShotForm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.PPScreenShotForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PPScreenShotForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PPScreenShotForm_MouseClick);
            this.Panel_control.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HPanel Panel_control;
        private HPushButton Button_SAVE;
        private HPushButton Button_COPY;
    }
}
