using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    using HFromUI.HColor;
    public class DDataGridView : DataGridView
    {
        public DDataGridView()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            BackgroundColor = Color.White;
            GridColor = Color.Blue;
            base.DoubleBuffered = true;

            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            //支持自定义标题行风格
            EnableHeadersVisualStyles = false;

            //标题行风格
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ColumnHeadersDefaultCellStyle.BackColor = HColors.Blues.AirForceBlue; ;
            ColumnHeadersDefaultCellStyle.ForeColor = HColors.Magentas.DeepMagenta;
            ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;

            //行头部颜色
            RowHeadersDefaultCellStyle.BackColor = Color.LightBlue;
            RowHeadersDefaultCellStyle.ForeColor = Color.White;
            RowHeadersDefaultCellStyle.SelectionBackColor = Color.Blue;
            RowHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            //标题行行高，与OnColumnAdded事件配合


            ColumnHeadersHeight = 32;

            //设置奇偶数行颜色
            StripeEvenColor = HColors.Whites.AntiqueWhite;
            StripeOddColor = HColors.Blues.ArcticBlue;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

        }
        /// <summary>RectColorDataH 字段。</summary>
        protected Color RectColorDataH = HColors.Blacks.Onyx;
        /// <summary>RectWidthDataH 字段。</summary>
        protected float RectWidthDataH = 1;
        /// <summary>IsAutoRowHeightDataH 字段。</summary>
        protected bool IsAutoRowHeightDataH = false;
        [HCategoryLanguage("边框"), HDisplayNameLanguage("是否显示边框"), HDescriptionLanguage("是否显示边框"), Browsable(true)]
        [DefaultValue(true)]
        public bool ShowRect
        {
            get { return BorderStyle == BorderStyle.FixedSingle; }
            set
            {
                BorderStyle = value ? BorderStyle.FixedSingle : BorderStyle.None;
                Invalidate();
            }
        }
        [HCategoryLanguage("行显示"), HDisplayNameLanguage("是否自动调整行高"), HDescriptionLanguage("是否自动调整行高"), Browsable(true)]
        [DefaultValue(false)]
        public bool IsAutoRowHeight
        {
            get { return IsAutoRowHeightDataH; }
            set
            {
                IsAutoRowHeightDataH = value;
                Invalidate();
            }
        }
        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ShowRect)
            {
                System.Drawing.Pen Pen = new System.Drawing.Pen(RectColorDataH, RectWidthDataH);
                e.Graphics.DrawRectangle(Pen, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }
        /// <summary>响应 CellEnter 事件。</summary>
        protected override void OnCellEnter(DataGridViewCellEventArgs e)
        {
            base.OnCellEnter(e);
            if (IsAutoRowHeightDataH)
            {
                this.Rows[e.RowIndex].Height = this.Rows[e.RowIndex].Height + 20;
            }
        }
        /// <summary>响应 CellLeave 事件。</summary>
        protected override void OnCellLeave(DataGridViewCellEventArgs e)
        {
            base.OnCellEnter(e);
            if (IsAutoRowHeightDataH)
            {
                this.Rows[e.RowIndex].Height = this.Rows[e.RowIndex].Height - 20;
            }
        }
        [HCategoryLanguage("行显示"), HDisplayNameLanguage("偶数行显示颜色"), HDescriptionLanguage("偶数行显示颜色"), Browsable(true)]
        public Color StripeEvenColor
        {
            get { return RowsDefaultCellStyle.BackColor; }
            set
            {
                RowsDefaultCellStyle.BackColor = value;
                Invalidate();
            }
        }

        [HCategoryLanguage("行显示"), HDisplayNameLanguage("奇数行显示颜色"), HDescriptionLanguage("奇数行显示颜色"), Browsable(true)]
        public Color StripeOddColor
        {
            get { return AlternatingRowsDefaultCellStyle.BackColor; }
            set
            {
                AlternatingRowsDefaultCellStyle.BackColor = value;
                Invalidate();
            }
        }

        [HCategoryLanguage("边框"), HDisplayNameLanguage("边框颜色"), HDescriptionLanguage("边框颜色"), Browsable(true)]
        public Color RectColor
        {
            get
            {
                return RectColorDataH;
            }
            set
            {
                RectColorDataH = value;
                Invalidate();
            }
        }

        [HCategoryLanguage("边框"), HDisplayNameLanguage("边框宽度"), HDescriptionLanguage("边框宽度"), Browsable(true)]
        public float RectWidth
        {
            get
            {
                return RectWidthDataH;
            }
            set
            {
                RectWidthDataH = value;
                Invalidate();
            }
        }
    }
}