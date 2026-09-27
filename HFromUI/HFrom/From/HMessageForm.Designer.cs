using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;

namespace HFromUI.HFrom.From
{
    partial class HMessageForm
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
            this.hButton_OK = new HPushButton();
            this.hButton_YES = new HPushButton();
            this.hButton_NO = new HPushButton();
            this.hButton_RETRY = new HPushButton();
            this.hButton_CANCLE = new HPushButton();
            this.hButton_IGNORE = new HPushButton();
            this.hButton_ABORT = new HPushButton();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            //
            // 通用白底灰边样式；NO/CANCLE/ABORT 悬停为 Tomato 红
            //
            ConfigButton(this.hButton_OK, 21, 41, 3, "OK", false, this.hButton_OK_Click);
            ConfigButton(this.hButton_YES, 128, 41, 4, "YES", false, this.hButton_YES_Click);
            ConfigButton(this.hButton_NO, 230, 41, 5, "NO", true, this.hButton_NO_Click);
            ConfigButton(this.hButton_RETRY, 21, 81, 6, "Retry", false, this.hButton_RETRY_Click);
            ConfigButton(this.hButton_CANCLE, 128, 81, 7, "Cancle", true, this.hButton_CANCLE_Click);
            ConfigButton(this.hButton_IGNORE, 230, 81, 8, "Ignore", false, this.hButton_IGNORE_Click);
            ConfigButton(this.hButton_ABORT, 21, 121, 9, "Abort", true, this.hButton_ABORT_Click);
            //
            // timer
            //
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            //
            // MessageForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(320, 180);
            this.Controls.Add(this.hButton_ABORT);
            this.Controls.Add(this.hButton_IGNORE);
            this.Controls.Add(this.hButton_CANCLE);
            this.Controls.Add(this.hButton_RETRY);
            this.Controls.Add(this.hButton_NO);
            this.Controls.Add(this.hButton_YES);
            this.Controls.Add(this.hButton_OK);
            this.EnableDoubleCilckSizeChange = false;
            this.EnableSizeChange = false;
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Location = new System.Drawing.Point(0, 0);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MouseMovePosition = HForm.MovePosition.TITLE;
            this.Name = "MessageForm";
            this.Radius = 0;
            this.ShadowColor = System.Drawing.Color.DimGray;
            this.ShowIcon = false;
            this.ResumeLayout(false);

        }

        /// <summary>统一配置消息框按钮：白底浅灰边圆角按钮，danger=true 时悬停 Tomato。</summary>
        private static void ConfigButton(HPushButton btn, int x, int y, int tab, string text,
            bool danger, System.EventHandler onClick)
        {
            btn.ButtonStyle = HPushButtonStyle.Custom;
            btn.Shape = HPushButtonShape.Round;
            btn.Radius = 3;
            btn.DialogResult = System.Windows.Forms.DialogResult.None;
            btn.Location = new System.Drawing.Point(x, y);
            btn.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            btn.Name = "hButton_" + text.ToUpperInvariant();
            btn.NormalBackColor = System.Drawing.Color.White;
            btn.NormalBorderColor = System.Drawing.Color.LightGray;
            btn.HoverBackColor = danger ? System.Drawing.Color.Tomato : System.Drawing.Color.LightGray;
            btn.HoverBorderColor = System.Drawing.Color.Silver;
            btn.PressedBackColor = System.Drawing.Color.LightGray;
            btn.PressedBorderColor = System.Drawing.Color.Silver;
            btn.NormalForeColor = System.Drawing.Color.Black;
            btn.HoverForeColor = danger ? System.Drawing.Color.White : System.Drawing.Color.Black;
            btn.Size = new System.Drawing.Size(70, 30);
            btn.TabIndex = tab;
            btn.Text = text;
            btn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            btn.Enabled = false;
            btn.Click += onClick;
        }

        #endregion

        private HPushButton hButton_OK;
        private HPushButton hButton_YES;
        private HPushButton hButton_NO;
        private HPushButton hButton_RETRY;
        private HPushButton hButton_CANCLE;
        private HPushButton hButton_IGNORE;
        private HPushButton hButton_ABORT;
        private System.Windows.Forms.Timer timer;
    }
}
