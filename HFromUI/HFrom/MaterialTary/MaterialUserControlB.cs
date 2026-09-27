using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HInformation;

namespace HFromUI.HFrom.MaterialTary
{
    using HFromUI.HColor;
    public class MaterialUserControlB<T> where T : UserControl
    {
        /// <summary>UserControl 成员。</summary>
        public T UserControl { set; get; }

        public MaterialUserControlB(UserControl userControl)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            UserControl =(T)userControl;
            UserControl.MouseMove += MaterialUserControl_MouseMove;
            UserControl.MouseLeave += MaterialUserControl_MouseLeave;
        }
        /// <summary>isMouseMove 字段。</summary>
        private bool isMouseMove = false;
        /// <summary>MaterialUserControl_MouseLeave 方法。</summary>
        private void MaterialUserControl_MouseLeave(object sender, EventArgs e)
        {
            isMouseMove = false;
        }

        /// <summary>MaterialUserControl_MouseMove 方法。</summary>
        private void MaterialUserControl_MouseMove(object sender, MouseEventArgs e)
        {
            isMouseMove = true;
        }


        public HMaterial Material;
        /// <summary>toolTip 字段。</summary>
        private ToolTip toolTip = new ToolTip()
        {
            IsBalloon = true,
            InitialDelay = 1000,
            ReshowDelay = 1000,
            OwnerDraw = true,
            AutoPopDelay = 2700,

        };
        /// <summary>ShowUI 方法。</summary>
        public virtual void ShowUI()
        {
            if (Material != null)
            {
                if (toolTip != null)
                {
                    if (isMouseMove)
                    {
                        toolTip.SetToolTip(UserControl, Material.ShowContent);
                    }
                    else
                    {
                        toolTip.RemoveAll();
                    }

                }

                UserControl.Text = Material.TextContent;
                if (Material.IsEnabelSelect)
                {
                    if (Material.IsSelect)
                    {
                        UserControl.BackColor = HColors.Greens.Chive;
                    }
                    else
                    {
                        UserControl.BackColor = HColors.Whites.AliceBlueWhite;
                    }
                }
                else
                {
                    switch (Material.Status)
                    {
                        case 0:

                            UserControl.BackColor = HColors.Greens.Basil;
                            break;
                        case 1:
                            UserControl.BackColor = HColors.Reds.BrickRed;
                            break;
                        case 2:
                            UserControl.BackColor = HColors.Oranges.Citrus;
                            break;
                        case 3:
                            UserControl.BackColor = HColors.Blues.ArcticBlue;
                            break;
                        case 4:
                            UserControl.BackColor = HColors.Yellows.GoldenYellow;
                            break;
                        case 5:
                            UserControl.BackColor = HColors.Reds.ChiliRed;
                            break;
                        case 6:
                            UserControl.BackColor = HColors.Oranges.DarkOrange;
                            break;
                        default:
                            UserControl.BackColor = HColors.Whites.BlanchedAlmond;
                            break;
                    }
                }

            }
        }

    }
}
