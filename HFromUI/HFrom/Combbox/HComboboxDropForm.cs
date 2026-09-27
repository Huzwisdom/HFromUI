using HFromUI.HFrom;
using HFromUI.HControl;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HFrom.Base;

namespace HFromUI.HFrom.Combbox
{
    using HFromUI.HControl.Tools.Button;
    internal partial class HComboboxDropForm : HAnchorBase
    {
       

        private HComboboxEx MainCombobox;

        //protected override bool ShowWithoutActivation => true;//确保此窗体不会被激活

        public HComboboxDropForm(HComboboxEx pPComboboxEx)
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.BaseControl = pPComboboxEx;

            hvScrollBarExt1.BindingControl(flowLayoutPanel);
            MainCombobox = base.BaseControl as HComboboxEx;

            #region 禁用水平滚动条

            flowLayoutPanel.AutoScroll = false;
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.WrapContents = false;
            flowLayoutPanel.HorizontalScroll.Maximum = 0; // 把水平滚动范围设成0就看不到水平滚动条了
            flowLayoutPanel.AutoScroll = true;

            #endregion 禁用水平滚动条

            this.Width = MainCombobox.Width;
            this.Radius = MainCombobox.DropformRadius;
            this.BackColor = MainCombobox.DropbaseColor;
            this.BorderColor = MainCombobox.DropborderColor;

            flowLayoutPanel.BackColor = this.BackColor;
            this.Invalidate();

        }

        /// <summary>响应 VisibleChanged 事件。</summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            SetDropSize();
            base.OnVisibleChanged(e);
           
            if (this.Visible)
            {
                this.Width = MainCombobox.Width;

                int fontheight = 4 + (int)HDrawPaint.GetTextSize("AA", MainCombobox.Font).Height + 4;

                #region 加载选项

                if (MainCombobox.Items != null && MainCombobox.Items.Count > 0)
                {
                    int count = 0;
                    foreach (object item in MainCombobox.Items)
                    {
                        HPushButton button = new HPushButton();
                        button.Text = (string)item;
                        button.Font = MainCombobox.Font;
                        button.ButtonStyle = HPushButtonStyle.Custom;
                        button.Shape = HPushButtonShape.Round;
                        Color backColor = MainCombobox.SelectedIndex == count
                            ? MainCombobox.DropbuttonSelectColor
                            : MainCombobox.DropbuttonColor;
                        // 三态色跟随主下拉框配色
                        button.NormalBackColor = backColor;
                        button.NormalBorderColor = backColor;
                        button.HoverBackColor = MainCombobox.DropbuttonMouseInColor;
                        button.HoverBorderColor = MainCombobox.DropbuttonMouseInColor;
                        button.PressedBackColor = MainCombobox.DropbuttonMouseDownColor;
                        button.PressedBorderColor = MainCombobox.DropbuttonMouseDownColor;
                        button.NormalForeColor = MainCombobox.ForeColor;
                        button.HoverForeColor = MainCombobox.ForeColor;
                        button.PressedForeColor = MainCombobox.ForeColor;

                        button.Radius = MainCombobox.DropbuttonRadius;
                        button.Margin = new Padding(0);
                        button.TextAlign = MainCombobox.DropTextAlign == HComboboxEx.Align.LEFT
                            ? ContentAlignment.MiddleLeft
                            : ContentAlignment.MiddleCenter;
                        button.Click += Button_Click;
                        button.Width = flowLayoutPanel.Width;
                        button.Height = fontheight;
                        flowLayoutPanel.Controls.Add(button);
                        count++;
                    }
                }
                else if (MainCombobox.Items.Count == 0)
                {
                    this.Height = fontheight;
                }

                if (flowLayoutPanel.VerticalScroll.Visible)
                {
                    Padding pd = new Padding(0, 0, 20, 0);
                    foreach (Control c in flowLayoutPanel.Controls)
                    {
                        c.Width = flowLayoutPanel.Width - 20;
                        c.Margin = pd;
                    }
                }

                //if (this.Height < 90)
                //    this.ShowShadow = false;

                #endregion 加载选项
            }
            else
            {
                flowLayoutPanel.Controls.Clear();
            }
            hvScrollBarExt1.SetScrollBarNum();
        }

        /// <summary>Button_Click 方法。</summary>
        private void Button_Click(object sender, EventArgs e)
        {
            //this.Hide();
            MainCombobox.SelectedIndex = (sender as HPushButton).TabIndex;
            MainCombobox.SelectionChangeCommitted_Event();

            //MainCombobox.Invalidate();

            this.Close();
        }

        /// <summary>设置 dropSize。</summary>
        private void SetDropSize()
        {
            Point screenPoint = MainCombobox.PointToScreen(new Point(0, 0));//获取主控件左上角对应的屏幕位置

            Point DropPoint = new Point(screenPoint.X + 1, screenPoint.Y + MainCombobox.Height);//获取主控件下侧的位置

            int fontheight = (int)HDrawPaint.GetTextSize("AA", MainCombobox.Font).Height + 4;

            int dropformHeight = 6 + MainCombobox.Items.Count * (fontheight + 4);//获取将要显示的高度

            int maxdropformheight = 6 + MainCombobox.MaxDropDownItems * (fontheight + 4);//计算能显示的最大个数的高度

            Rectangle screenRect = Screen.FromControl(MainCombobox).WorkingArea;

            if (dropformHeight > maxdropformheight)//若dropform显示的窗口高度大于设置的最大高度则修改为最大高度
            {
                dropformHeight = maxdropformheight;
            }

            if (screenRect.Bottom - DropPoint.Y < dropformHeight)//判断若当前位置下拉框会显示不完整
            {
                if (screenRect.Bottom - DropPoint.Y < 200)//若能显示下拉框高度小于200则切换到combobox上侧显示
                {
                    //dropformHeight = screenPoint.Y - screenRect.Top;
                    if (dropformHeight > maxdropformheight)//若dropform显示的窗口高度大于设置的最大高度则修改为最大高度
                    {
                        dropformHeight = maxdropformheight;
                    }

                    DropPoint = new Point(screenPoint.X + 1, screenPoint.Y - dropformHeight);
                }
                else//否则按combobox下侧到屏幕最下侧的距离来显示dropform
                {
                    dropformHeight = screenRect.Bottom - DropPoint.Y;
                }
            }
            else
            {
                if (dropformHeight > maxdropformheight)//若dropform显示的窗口高度大于设置的最大高度则修改为最大高度
                {
                    dropformHeight = maxdropformheight;
                }
            }

            this.Height = dropformHeight;
            this.Location = DropPoint;
        }

      

    }
}
