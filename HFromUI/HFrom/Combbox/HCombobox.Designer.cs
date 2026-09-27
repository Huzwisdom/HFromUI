

namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HColor;
    partial class HCombobox
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
            this.hNoBoraderCombobox1 = new HNoBoraderCombobox();
            this.SuspendLayout();
            //
            // hNoBoraderCombobox1
            //
            this.hNoBoraderCombobox1.ArrowImage = HPhoto.Get("down");
            this.hNoBoraderCombobox1.BackColor = System.Drawing.Color.White;
            this.hNoBoraderCombobox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hNoBoraderCombobox1.FormattingEnabled = true;
            this.hNoBoraderCombobox1.ImageOffset = new System.Drawing.Point(0, 0);
            this.hNoBoraderCombobox1.ImageSize = new System.Drawing.Size(13, 13);
            this.hNoBoraderCombobox1.Location = new System.Drawing.Point(8, 9);
            this.hNoBoraderCombobox1.Name = "hNoBoraderCombobox1";
            this.hNoBoraderCombobox1.Size = new System.Drawing.Size(118, 20);
            this.hNoBoraderCombobox1.TabIndex = 0;
            this.hNoBoraderCombobox1.SelectedIndexChanged += new System.EventHandler(this.hNoBoraderCombobox1_SelectedIndexChanged);
            this.hNoBoraderCombobox1.SelectedValueChanged += new System.EventHandler(this.hNoBoraderCombobox1_SelectedValueChanged);
            this.hNoBoraderCombobox1.SizeChanged += new System.EventHandler(this.hNoBoraderCombobox1_SizeChanged);
            this.hNoBoraderCombobox1.SelectionChangeCommitted += new System.EventHandler(this.hNoBoraderCombobox1_SelectionChangeCommitted);
            // 
            // PPCombobox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.hNoBoraderCombobox1);
            this.Name = "PPCombobox";
            this.Size = new System.Drawing.Size(136, 34);
            this.ResumeLayout(false);

        }

        #endregion

        private HNoBoraderCombobox hNoBoraderCombobox1;
    }
}
