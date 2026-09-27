using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFromUI.HInformation;

namespace HFromUI.HFrom.MaterialTary
{
    using HFromUI.HColor;
    public class MaterialUserControlA : Button
    {
        public MaterialUserControlA()
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.MouseMove += MaterialUserControl_MouseMove;
            this.MouseLeave += MaterialUserControl_MouseLeave;
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
        /// <summary>showNumber 字段。</summary>
        private int showNumber = 0;
        /// <summary>ShowUI 方法。</summary>
        public virtual void ShowUI()
        {
            showNumber++;
            if (showNumber>10000)
            {
                showNumber = 0;
            }
            if (Material != null)
            {
                if (toolTip != null)
                {
                    if (isMouseMove)
                    {
                        toolTip.SetToolTip(this, Material.ShowContent);
                    }
                    else
                    {
                        toolTip.RemoveAll();
                    }

                }

                this.Text = Material.TextContent;
                if (Material.IsEnabelSelect)
                {
                    if (Material.IsSelect)
                    {
                        this.BackColor = HColors.Greens.Chive;
                    }
                    else
                    {
                        this.BackColor = HColors.Whites.AliceBlueWhite;
                    }
                }
                else
                {
                    switch (Material.Status)
                    {
                        case 0:
                            
                            this.BackColor = HColors.Greens.Basil;
                            break;
                        case 1:
                            this.BackColor = HColors.Reds.BrickRed;
                            break;
                        case 2:
                            this.BackColor = HColors.Oranges.Citrus;
                            break;
                        case 3:
                            this.BackColor = HColors.Blues.ArcticBlue;
                            break;
                        case 4:
                            this.BackColor = HColors.Yellows.GoldenYellow;
                            break;
                        case 5:
                            this.BackColor = HColors.Reds.ChiliRed;
                            break;
                        case 6:
                            this.BackColor = HColors.Oranges.DarkOrange;
                            break;
                        case 7:
                            if (showNumber%2==0)
                            {
                                this.BackColor = HColors.Oranges.DarkOrange;
                            }
                            else
                            {
                                this.BackColor = HColors.Greens.AppleGreen;
                            }
                          
                            break;
                        case 8:
                            if (showNumber % 3 == 0)
                            {
                                this.BackColor = HColors.Oranges.DarkOrange;
                            }
                            else
                            {
                                this.BackColor = HColors.Greens.AppleGreen;
                            }

                            break;
                        case 9:
                            if (showNumber % 4 == 0)
                            {
                                this.BackColor = HColors.Oranges.DarkOrange;
                            }
                            else
                            {
                                this.BackColor = HColors.Greens.AppleGreen;
                            }

                            break;
                        default:
                            this.BackColor = HColors.Whites.BlanchedAlmond;
                            break;
                    }
                }

            }
        }
    }
}
