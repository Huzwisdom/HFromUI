using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HInterface;
using HFromUI.HInformation;

namespace HFromUI.HFrom.MaterialTary
{
    public partial class MaterialTaryC : UserControl
    {
        public MaterialTaryC()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SizeChanged += MaterialTaryC_SizeChanged;
        }
        /// <summary>Work 成员。</summary>
        public iHFrom Work { set; get; }
        /// <summary>MaterialTaryC_SizeChanged 方法。</summary>
        private void MaterialTaryC_SizeChanged(object sender, EventArgs e)
        {
            widthScore = 1d * (this.TaryPanel.Width - leftInterval - rightInterval) / 100d;
            heightScore = 1d * (this.TaryPanel.Height - upInterval - downInterval) / 100d;
            int num = 0;
            foreach (var item in concurrentDictionary.Keys)
            {
                concurrentDictionary[item].XScore = widthScore;
                concurrentDictionary[item].YScore = heightScore;
                if (concurrentDictionary[item].UserControl is Control)
                {
                    if (concurrentDictionary[item].IsScore)
                    {
                        TaryPanel.Controls[num].Location = new Point(Convert.ToInt32(concurrentDictionary[item].XScore * concurrentDictionary[item].X + leftInterval), Convert.ToInt32(concurrentDictionary[item].YScore * concurrentDictionary[item].Y + upInterval));
                        TaryPanel.Controls[num].Size = new Size(Convert.ToInt32(concurrentDictionary[item].XScore * concurrentDictionary[item].Width), Convert.ToInt32(concurrentDictionary[item].YScore * concurrentDictionary[item].Height));
                    }
                    else
                    {
                        TaryPanel.Controls[num].Location = new Point(Convert.ToInt32(concurrentDictionary[item].X + leftInterval), Convert.ToInt32(concurrentDictionary[item].Y + upInterval));
                        TaryPanel.Controls[num].Size = new Size(Convert.ToInt32(concurrentDictionary[item].Width), Convert.ToInt32(concurrentDictionary[item].Height));
                    }
                    num++;
                }

            }
        }

        /// <summary>upInterval 字段。</summary>
        private double upInterval = 3;
        /// <summary>downInterval 字段。</summary>
        private double downInterval = 3;
        /// <summary>leftInterval 字段。</summary>
        private double leftInterval = 3;
        /// <summary>rightInterval 字段。</summary>
        private double rightInterval = 3;
        /// <summary>widthScore 字段。</summary>
        private double widthScore = 0;
        /// <summary>heightScore 字段。</summary>
        private double heightScore = 0;

        private ConcurrentDictionary<int, PMaterial> concurrentDictionary = new ConcurrentDictionary<int, PMaterial>();

        /// <summary>清空。</summary>
        public void Clear()
        {
            concurrentDictionary.Clear();

            TaryPanel.Controls.Clear();
        }
        /// <summary>添加。</summary>
        public void Add(int index, double x, double y, double w, double h, object control, bool isScore = true)
        {
            widthScore = 1d * (this.TaryPanel.Width - leftInterval - rightInterval) / 100d;
            heightScore = 1d * (this.TaryPanel.Height - upInterval - downInterval) / 100d;

            PMaterial userControlMaterialC = new PMaterial();
            userControlMaterialC.Index = index;
            userControlMaterialC.X = x;
            userControlMaterialC.Y = y;
            userControlMaterialC.Width = w;
            userControlMaterialC.Height = h;
            userControlMaterialC.UserControl = control;
            userControlMaterialC.IsScore = isScore;
            userControlMaterialC.XScore = widthScore;
            userControlMaterialC.YScore = heightScore;
            concurrentDictionary.TryAdd(index, userControlMaterialC);
        }

        /// <summary>添加。</summary>
        public void Add(string name, double x, double y, double w, double h, object control, bool isScore = true)
        {
            widthScore = 1d * (this.TaryPanel.Width - leftInterval - rightInterval) / 100d;
            heightScore = 1d * (this.TaryPanel.Height - upInterval - downInterval) / 100d;
            List<int> indexs = new List<int>();
            int index = 0;
            foreach (var item in concurrentDictionary.Keys)
            {
                indexs.Add(item);
            }
            if (indexs.Count > 0)
            {
                index = indexs.Max() + 1;
            }
            PMaterial userControlMaterialC = new PMaterial();
            userControlMaterialC.Index = index;
            userControlMaterialC.X = x;
            userControlMaterialC.Y = y;
            userControlMaterialC.Width = w;
            userControlMaterialC.Height = h;
            userControlMaterialC.UserControl = control;
            userControlMaterialC.IsScore = isScore;
            userControlMaterialC.XScore = widthScore;
            userControlMaterialC.YScore = heightScore;
            concurrentDictionary.TryAdd(index, userControlMaterialC);
        }
        /// <summary>绘制。</summary>
        public void Draw()
        {
            foreach (var item in concurrentDictionary.Keys)
            {
                if (concurrentDictionary[item].UserControl is Control)
                {
                    Control userControl = concurrentDictionary[item].UserControl as Control;
                    if (concurrentDictionary[item].IsScore)
                    {
                        userControl.Location = new Point(Convert.ToInt32(concurrentDictionary[item].XScore * concurrentDictionary[item].X + leftInterval), Convert.ToInt32(concurrentDictionary[item].YScore * concurrentDictionary[item].Y + upInterval));
                        userControl.Size = new Size(Convert.ToInt32(concurrentDictionary[item].XScore * concurrentDictionary[item].Width), Convert.ToInt32(concurrentDictionary[item].YScore * concurrentDictionary[item].Height));
                    }
                    else
                    {
                        userControl.Location = new Point(Convert.ToInt32(concurrentDictionary[item].X + leftInterval), Convert.ToInt32(concurrentDictionary[item].Y + upInterval));
                        userControl.Size = new Size(Convert.ToInt32(concurrentDictionary[item].Width), Convert.ToInt32(concurrentDictionary[item].Height));
                    }
                    TaryPanel.Controls.Add(userControl);
                }
            }
        }
    }
}
