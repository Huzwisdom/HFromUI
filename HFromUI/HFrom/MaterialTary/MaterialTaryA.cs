using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HInformation;

namespace HFromUI.HFrom.MaterialTary
{
    using HFromUI.HColor;
    public partial class MaterialTaryA : UserControl
    {
        /// <summary>upInterval 字段。</summary>
        private double upInterval = 2;
        /// <summary>leftInterval 字段。</summary>
        private double leftInterval = 2;
        /// <summary>rowMaterialInterval 字段。</summary>
        private double rowMaterialInterval = 2;
        /// <summary>columnMaterialInterval 字段。</summary>
        private double columnMaterialInterval = 2;
        /// <summary>timer 字段。</summary>
        private Timer timer = new Timer() { Interval=500,};
        /// <summary>mode 字段。</summary>
        private int mode = 0;
        /// <summary>oldw 字段。</summary>
        private double oldw = 0;
        /// <summary>oldh 字段。</summary>
        private double oldh = 0;
        public int TimeInterval
        {
            get
            {
                return timer.Interval;
            }
            set
            {
                timer.Interval = value;
            }
        }
        /// <summary>Tary 成员。</summary>
        public HTary Tary = new HTary();

        /// <summary>TimerAction 成员。</summary>
        public Action TimerAction { set; get; }

        public MaterialTaryA()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SizeChanged += MaterialTary_SizeChanged;
        }
        /// <summary>MaterialTary_SizeChanged 方法。</summary>
        private void MaterialTary_SizeChanged(object sender, EventArgs e)
        {
            if (Tary == null || Tary.rowList.Count == 0 || this.TaryPanel.Height == 0 || this.TaryPanel.Width == 0)
            {
                return;
            }
            double h = Convert.ToDouble(this.TaryPanel.Height) - upInterval * 2;
            double w = Convert.ToDouble(this.TaryPanel.Width) - 2 * leftInterval;

            int maxNum = Tary.rowList.Max();
            if (maxNum < 0)
            {
                return;
            }
            double mw = w / (maxNum + 1);
            double mh = h / Tary.rowList.Count;
            int num = 0;
            for (int i = 0; i < Tary.rowList.Count; i++)
            {
                double aw = w / (Tary.rowList[i] + 1);
                for (int j = 0; j <= Tary.rowList[i]; j++)
                {

                    TaryPanel.Controls[num].Location = new Point(Convert.ToInt32((aw - mw) / 2 + leftInterval + j * aw + columnMaterialInterval / 2), Convert.ToInt32(upInterval + i * mh + rowMaterialInterval / 2));
                    TaryPanel.Controls[num].Size = new Size(Convert.ToInt32(mw - columnMaterialInterval), Convert.ToInt32(mh - rowMaterialInterval));
                    num++;
                }
            }
        }
        public HMaterial this[int index]
        {
            get
            {
                return Tary[index];
            }
        }


        public HMaterial this[int row, int column]
        {
            get
            {
                return Tary[row, column];
            }
            set
            {
                Tary[row, column] = value;
            }
        }
        public int Count
        {
            get
            {
                return Tary.Count;
            }
        }

        /// <summary>获取 materialControl。</summary>
        internal MaterialUserControlA GetMaterialControl(int index)
        {
            return (MaterialUserControlA)TaryPanel.Controls[index];
        }
        /// <summary>设置 interval。</summary>
        public void SetInterval(double up, double left, double rowI, double colI)
        {
            upInterval = up;
            leftInterval = left;
            rowMaterialInterval = rowI;
            columnMaterialInterval = colI;
        }
        /// <summary>设置 columnInterval。</summary>
        public void SetColumnInterval(double colI)
        {
            columnMaterialInterval = colI;
        }
        /// <summary>添加。</summary>
        public void Add(int columnNum, bool isReversal = false)
        {
            mode = 0;
            int start = 0; int index = 0;
            if (Count == 0)
            {
                start = 0; index = 0;
                TaryPanel.BackColor = HColors.Whites.AntiqueWhite;
                TaryPanel.Controls.Clear();
            }
            else
            {
                start = Tary[Count - 1].Row + 1;
                index = Tary[Count - 1].Index + 1;
            }
            if (isReversal)
            {
                for (int i = columnNum - 1; i >= 0; i--)
                {
                    Tary.Add(start, i, index + (columnNum - 1) - i);
                }
            }
            else
            {
                for (int i = 0; i < columnNum; i++)
                {
                    Tary.Add(start, i, index + i);
                }
            }

        }
        /// <summary>添加。</summary>
        public void Add(int rowNum, int columnNum)
        {
            mode = 1;
            TaryPanel.BackColor = HColors.Whites.AntiqueWhite;
            TaryPanel.Controls.Clear();
            Tary.Clear();
            int num = 0;
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < columnNum; j++)
                {
                    Tary.Add(i, j, num);
                    num++;
                }
            }
        }

        /// <summary>添加。</summary>
        public void Add(int rowNum, int columnNum, int index)
        {
            mode = 0;
            bool isok = Tary.Add(rowNum, columnNum, index);
            if (!isok)
            {
                throw new Exception($@"add Error rowNum:{rowNum} columnNum:{columnNum} index:{index}");
            }
        }

        /// <summary>清空。</summary>
        public void Clear()
        {
            TaryPanel.BackColor = HColors.Whites.AntiqueWhite;
            TaryPanel.Controls.Clear();
            Tary.Clear();
        }
        /// <summary>EnableSelect 方法。</summary>
        public void EnableSelect(bool enable)
        {
            foreach (var item in Tary.Materials.Keys)
            {
                Tary.Materials[item].IsEnabelSelect = enable;
            }
            if (enable)
            {
                TaryPanel.BackColor = HColors.Blacks.Midnight;
            }
            else
            {
                TaryPanel.BackColor = HColors.Whites.AntiqueWhite;
            }
        }
        /// <summary>SelectAll 方法。</summary>
        public void SelectAll(bool select)
        {
            foreach (var item in Tary.Materials.Keys)
            {
                if (Tary.Materials[item].IsEnabelSelect)
                {
                    Tary.Materials[item].IsSelect = select;
                }
            }
        }
        /// <summary>绘制。</summary>
        public void Draw()
        {
            Tary.Calculate();
            double h = Convert.ToDouble(oldh= this.TaryPanel.Height) - 2 * upInterval;
            double w = Convert.ToDouble(oldw= this.TaryPanel.Width) - 2 * leftInterval;

            int maxNum = Tary.rowList.Max();

            double mw = w / (maxNum + 1);
            double mh = h / Tary.rowList.Count;
            int num = 0;
            for (int i = 0; i < Tary.rowList.Count; i++)
            {
                double aw = w / (Tary.rowList[i] + 1);
                for (int j = 0; j <= Tary.rowList[i]; j++)
                {
                    MaterialUserControlA materialUserControla = new MaterialUserControlA();
                    materialUserControla.Location = new Point(Convert.ToInt32((aw - mw) / 2 + leftInterval + j * aw + columnMaterialInterval / 2), Convert.ToInt32(upInterval + i * mh + rowMaterialInterval / 2));
                    materialUserControla.Size = new Size(Convert.ToInt32(mw - columnMaterialInterval), Convert.ToInt32(mh - rowMaterialInterval));
                    materialUserControla.Click += MaterialUserControl_Click;
                    if (mode == 1)
                    {
                        materialUserControla.Material = Tary[num];
                    }
                    else
                    {
                        materialUserControla.Material = Tary[i, j];
                    }

                    TaryPanel.Controls.Add(materialUserControla);
                    num++;
                }






            }

            Run();

        }

        /// <summary>MaterialUserControl_Click 方法。</summary>
        private void MaterialUserControl_Click(object sender, EventArgs e)
        {
            Tary.SelectIndex = ((MaterialUserControlA)sender).Material.Index;
            if (((MaterialUserControlA)sender).Material.IsEnabelSelect)
            {
                ((MaterialUserControlA)sender).Material.IsSelect = !((MaterialUserControlA)sender).Material.IsSelect;
            }
        }

        /// <summary>执行。</summary>
        public void Run()
        {
            if (!timer.Enabled)
            {
                timer.Tick += TimeTick;
                timer.Start();

            }


        }

        /// <summary>TimeTick 方法。</summary>
        protected virtual void TimeTick(object sender, EventArgs e)
        {
            if (TimerAction!=null)
            {
                TimerAction();
            }
            if (TaryPanel.Controls != null)
            {
                foreach (var item in TaryPanel.Controls)
                {
                    ((MaterialUserControlA)item).ShowUI();
                }
            }
            if ( Convert.ToInt32(oldh) != this.TaryPanel.Height || Convert.ToInt32(oldw) != this.TaryPanel.Width)
            {
                MaterialTary_SizeChanged(null, null);
                oldh = this.TaryPanel.Height;
                oldw = this.TaryPanel.Width;
            }
        }
    }
}
