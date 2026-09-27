using System;
using System.Windows.Forms;
using HFromUI.HColor;
using HFromUI.HInformation;

namespace HFromUI.HControl.Tools.Material
{
    /// <summary>
    /// 物料单元格包装器（泛型）：在任意 <typeparamref name="T"/>（Control 派生类，需有无参构造）
    /// 控件上承载 <see cref="HMaterial"/> 的显示行为——文字、状态底色、悬停气泡提示、选中色。
    /// 既可由无参构造内部创建控件（托盘自动布控），也可传入已有控件进行包装。
    /// </summary>
    /// <typeparam name="T">被包装的实际控件类型，必须是 <see cref="Control"/> 派生类。</typeparam>
    public class HMaterialCell<T> where T : Control, new()
    {
        /// <summary>无参构造：内部创建被包装控件并挂接鼠标事件。</summary>
        public HMaterialCell() : this(new T())
        {
        }

        /// <summary>包装构造：使用外部已创建的控件并挂接鼠标事件。</summary>
        /// <param name="control">被包装的实际控件。</param>
        public HMaterialCell(T control)
        {
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            Control = control;
            Control.MouseMove += MaterialCell_MouseMove;
            Control.MouseLeave += MaterialCell_MouseLeave;
        }

        /// <summary>被包装的实际控件（托盘面板中添加的就是它）。</summary>
        public T Control { get; }

        /// <summary>isMouseMove 字段。</summary>
        private bool isMouseMove = false;
        /// <summary>MaterialCell_MouseLeave 方法。</summary>
        private void MaterialCell_MouseLeave(object sender, EventArgs e)
        {
            isMouseMove = false;
        }

        /// <summary>MaterialCell_MouseMove 方法。</summary>
        private void MaterialCell_MouseMove(object sender, MouseEventArgs e)
        {
            isMouseMove = true;
        }

        /// <summary>Material 成员。</summary>
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
                        toolTip.SetToolTip(Control, Material.ShowContent);
                    }
                    else
                    {
                        toolTip.RemoveAll();
                    }

                }

                Control.Text = Material.TextContent;
                if (Material.IsEnabelSelect)
                {
                    if (Material.IsSelect)
                    {
                        Control.BackColor = HColors.Greens.Chive;
                    }
                    else
                    {
                        Control.BackColor = HColors.Whites.AliceBlueWhite;
                    }
                }
                else
                {
                    switch (Material.Status)
                    {
                        case 0:

                            Control.BackColor = HColors.Greens.Basil;
                            break;
                        case 1:
                            Control.BackColor = HColors.Reds.BrickRed;
                            break;
                        case 2:
                            Control.BackColor = HColors.Oranges.Citrus;
                            break;
                        case 3:
                            Control.BackColor = HColors.Blues.ArcticBlue;
                            break;
                        case 4:
                            Control.BackColor = HColors.Yellows.GoldenYellow;
                            break;
                        case 5:
                            Control.BackColor = HColors.Reds.ChiliRed;
                            break;
                        case 6:
                            Control.BackColor = HColors.Oranges.DarkOrange;
                            break;
                        case 7:
                            if (showNumber%2==0)
                            {
                                Control.BackColor = HColors.Oranges.DarkOrange;
                            }
                            else
                            {
                                Control.BackColor = HColors.Greens.AppleGreen;
                            }

                            break;
                        case 8:
                            if (showNumber % 3 == 0)
                            {
                                Control.BackColor = HColors.Oranges.DarkOrange;
                            }
                            else
                            {
                                Control.BackColor = HColors.Greens.AppleGreen;
                            }

                            break;
                        case 9:
                            if (showNumber % 4 == 0)
                            {
                                Control.BackColor = HColors.Oranges.DarkOrange;
                            }
                            else
                            {
                                Control.BackColor = HColors.Greens.AppleGreen;
                            }

                            break;
                        default:
                            Control.BackColor = HColors.Whites.BlanchedAlmond;
                            break;
                    }
                }

            }
        }
    }
}
