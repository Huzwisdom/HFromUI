using System;

namespace HFromUI.HFrom.Tables
{
    /// <summary>
    /// 用于DataGridViewButtonColumnEx的按钮点击事件
    /// </summary>
    public class DataGridViewButtonClickEventArgs : EventArgs
    {
        /// <summary>ColumnIndex 成员。</summary>
        public int ColumnIndex { get; set; }

        /// <summary>RowIndex 成员。</summary>
        public int RowIndex { get; set; }

        /// <summary>值。</summary>
        public object Value { get; set; }

        /// <summary>Buttons 成员。</summary>
        public System.Windows.Forms.MouseButtons Buttons { get; set; }

        /// <summary>Clicks 成员。</summary>
        public int Clicks { get; set; }

        public DataGridViewButtonClickEventArgs(int columnIndex, int rowIndex, object value, System.Windows.Forms.MouseButtons buttons, int clicks)
        {
            this.ColumnIndex = columnIndex;
            this.RowIndex = rowIndex;
            this.Value = value;
            this.Buttons = buttons;
            this.Clicks = clicks;
        }
    }
}
