using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HFromUI.HColor;
using HFromUI.HInformation;

namespace HFromUI.HControl.Tools.Material
{
    /// <summary>
    /// 物料托盘（泛型）：托盘中的每个料位由 <see cref="HMaterialCell{T}"/> 包装的 T 控件按行列网格呈现，
    /// T 为 <see cref="Button"/> 时为标准按钮料位，也可换用任意 Control 派生控件。
    /// 内容区边距用 <see cref="Display"/>（Padding 四向）设置，行列间距用
    /// <see cref="RowSpacing"/> / <see cref="ColumnSpacing"/> 设置，均可在属性窗口中直接调整。
    /// </summary>
    /// <typeparam name="T">料位控件类型，必须是 <see cref="Control"/> 派生类且有无参构造。</typeparam>
    public partial class HMaterialTray<T> : UserControl where T : Control, new()
    {
        /// <summary>timer 字段。</summary>
        private Timer timer = new Timer() { Interval=500,};
        /// <summary>mode 字段。</summary>
        private int mode = 0;
        /// <summary>oldw 字段。</summary>
        private double oldw = 0;
        /// <summary>oldh 字段。</summary>
        private double oldh = 0;
        /// <summary>当前托盘中的料位包装器列表，顺序与 Body.Controls 一一对应。</summary>
        private List<HMaterialCell<T>> materialCells = new List<HMaterialCell<T>>();

        /// <summary>定时器刷新间隔（毫秒）。</summary>
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

        /// <summary>内容区距托盘四边的边距（上/下/左/右，像素），可在属性窗口展开设置。</summary>
        [Category("布局"), Description("内容区距托盘四边的边距（上/下/左/右，像素）")]
        public Padding Display { get; set; } = new Padding(2);

        /// <summary>相邻两行料位之间的垂直间距（像素）。</summary>
        [Category("布局"), DefaultValue(2), Description("相邻两行料位之间的垂直间距（像素）")]
        public int RowSpacing { get; set; } = 2;

        /// <summary>相邻两列料位之间的水平间距（像素）。</summary>
        [Category("布局"), DefaultValue(2), Description("相邻两列料位之间的水平间距（像素）")]
        public int ColumnSpacing { get; set; } = 2;

        /// <summary>Tary 成员。</summary>
        public HTary Tary = new HTary();

        /// <summary>TimerAction 成员。</summary>
        public Action TimerAction { set; get; }

        public HMaterialTray()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SizeChanged += UpdateLayout;
        }

        /// <summary>按当前边距与行列间距重新排布所有料位控件。</summary>
        private void UpdateLayout(object sender, EventArgs e)
        {
            if (Tary == null || Tary.rowList.Count == 0 || Body.Height == 0 || Body.Width == 0)
            {
                return;
            }
            double h = Convert.ToDouble(Body.Height) - Display.Vertical;
            double w = Convert.ToDouble(Body.Width) - Display.Horizontal;

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

                    Body.Controls[num].Location = new Point(Convert.ToInt32((aw - mw) / 2 + Display.Left + j * aw + ColumnSpacing / 2), Convert.ToInt32(Display.Top + i * mh + RowSpacing / 2));
                    Body.Controls[num].Size = new Size(Convert.ToInt32(mw - ColumnSpacing), Convert.ToInt32(mh - RowSpacing));
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

        /// <summary>获取 materialCell。</summary>
        internal HMaterialCell<T> GetMaterialCell(int index)
        {
            return materialCells[index];
        }

        /// <summary>添加。</summary>
        public void Add(int columnNum, bool isReversal = false)
        {
            mode = 0;
            int start = 0; int index = 0;
            if (Count == 0)
            {
                start = 0; index = 0;
                Body.BackColor = HColors.Whites.AntiqueWhite;
                Body.Controls.Clear();
                materialCells.Clear();
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
            Body.BackColor = HColors.Whites.AntiqueWhite;
            Body.Controls.Clear();
            materialCells.Clear();
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
            Body.BackColor = HColors.Whites.AntiqueWhite;
            Body.Controls.Clear();
            materialCells.Clear();
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
                Body.BackColor = HColors.Blacks.Midnight;
            }
            else
            {
                Body.BackColor = HColors.Whites.AntiqueWhite;
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
            double h = Convert.ToDouble(oldh= Body.Height) - Display.Vertical;
            double w = Convert.ToDouble(oldw= Body.Width) - Display.Horizontal;

            int maxNum = Tary.rowList.Max();

            double mw = w / (maxNum + 1);
            double mh = h / Tary.rowList.Count;
            int num = 0;
            for (int i = 0; i < Tary.rowList.Count; i++)
            {
                double aw = w / (Tary.rowList[i] + 1);
                for (int j = 0; j <= Tary.rowList[i]; j++)
                {
                    HMaterialCell<T> materialCell = new HMaterialCell<T>();
                    materialCell.Control.Location = new Point(Convert.ToInt32((aw - mw) / 2 + Display.Left + j * aw + ColumnSpacing / 2), Convert.ToInt32(Display.Top + i * mh + RowSpacing / 2));
                    materialCell.Control.Size = new Size(Convert.ToInt32(mw - ColumnSpacing), Convert.ToInt32(mh - RowSpacing));
                    materialCell.Control.Click += (sender, e) => MaterialCell_Click(materialCell);
                    if (mode == 1)
                    {
                        materialCell.Material = Tary[num];
                    }
                    else
                    {
                        materialCell.Material = Tary[i, j];
                    }

                    Body.Controls.Add(materialCell.Control);
                    materialCells.Add(materialCell);
                    num++;
                }
            }

            Run();

        }

        /// <summary>MaterialCell_Click 方法。</summary>
        private void MaterialCell_Click(HMaterialCell<T> materialCell)
        {
            Tary.SelectIndex = materialCell.Material.Index;
            if (materialCell.Material.IsEnabelSelect)
            {
                materialCell.Material.IsSelect = !materialCell.Material.IsSelect;
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
            if (Body.Controls != null)
            {
                foreach (HMaterialCell<T> item in materialCells)
                {
                    item.ShowUI();
                }
            }
            if ( Convert.ToInt32(oldh) != Body.Height || Convert.ToInt32(oldw) != Body.Width)
            {
                UpdateLayout(null, null);
                oldh = Body.Height;
                oldw = Body.Width;
            }
        }
    }
}
