using HFromUI.HAttribute;
using HFromUI.HEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HFrom.PropertyGrid
{
    using System.ComponentModel;
    using HFromUI.HColor;
    using PropertyGrid = System.Windows.Forms.PropertyGrid;
    public class APropertyGrid : PropertyGrid
    {
        /// <summary>themeMode 字段。</summary>
        private HThemeMode themeMode = HThemeMode.Warm;
        /// <summary>rowHeight 字段。</summary>
        private int rowHeight = -1;             // 记录用户设置的行高（-1 表示未设置）
        /// <summary>setRowHeight 字段。</summary>
        private int setRowHeight = -1;
        
        public APropertyGrid()
        {

        }
        /// <summary>
        /// 重写基类方法：属性值改变时触发自定义事件。
        /// </summary>
        protected override void OnPropertyValueChanged(PropertyValueChangedEventArgs e)
        {
            base.OnPropertyValueChanged(e);
        }

        // 重写基类方法，在选中对象改变后触发自定义事件
        protected override void OnSelectedObjectsChanged(EventArgs e)
        {
            base.OnSelectedObjectsChanged(e);
        }
        public void SetValue(object value)
        { 
          this.SelectedObject = value;
          this.Refresh();
        }
        /// <summary>
        /// 设置属性网格的行高（单位：像素）。
        /// 内部通过反射修改 PropertyGridView 的 RowHeight 属性，并调整字体以双保险生效。
        /// </summary>
        /// <param name="height">期望的行高值</param>
        public void SetRowHeight(int height)
        {
            if (height <= 0) return;
            rowHeight = height;

            // 立即尝试应用（如果内部控件已创建）
            ApplyRowHeight(height);

            // 同时调整字体，确保行高大于字体
            if (this.Font != null)
            {
                float fontSize = height / 1.23f;
                if (fontSize < 6) fontSize = 6;
                if (fontSize > 30) fontSize = 30;
                if (Math.Abs(this.Font.Size - fontSize) > 0.5f)
                {
                    this.Font = new Font(this.Font.FontFamily, fontSize);
                }
            }
            this.Refresh();
        }

        /// <summary>
        /// 当控件句柄创建时，应用之前设置的行高（如果有）。
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (rowHeight > 0)
                ApplyRowHeight(rowHeight);
        }
        /// <summary>RowHeight 成员。</summary>
        /// <summary>RowHeight 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("行高"), HDescriptionLanguage("行高"), Browsable(true)]
        public int RowHeight { get { return setRowHeight; } set { setRowHeight = value; SetRowHeight(setRowHeight); } }
        /// <summary>
        /// 实际执行行高设置（通过反射）。
        /// </summary>
        private void ApplyRowHeight(int height)
        {
            try
            {
                // 查找内部 PropertyGridView 控件
                Control gridView = null;
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl.GetType().Name == "PropertyGridView")
                    {
                        gridView = ctrl;
                        break;
                    }
                }

                if (gridView != null)
                {
                    // 尝试设置 RowHeight 公共属性（某些版本存在）
                    PropertyInfo rowHeightProp = gridView.GetType().GetProperty(
                        "RowHeight",
                        BindingFlags.Instance | BindingFlags.Public);

                    if (rowHeightProp != null && rowHeightProp.CanWrite)
                    {
                        rowHeightProp.SetValue(gridView, height, null);
                    }
                    else
                    {
                        // 备选：通过字段 _rowHeight 或直接修改内部对象
                        FieldInfo rowHeightField = gridView.GetType().GetField(
                            "_rowHeight",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        if (rowHeightField != null)
                        {
                            rowHeightField.SetValue(gridView, height);
                        }
                    }

                    // 使布局刷新
                    gridView.PerformLayout();
                }
            }
            catch
            {
                // 静默失败，不影响使用
            }
        }

        // ============ 主题部分（保持不变） ============
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("主题模式"), HDescriptionLanguage("主题模式"), Browsable(true)]
        public HThemeMode ThemeMode
        {
            get {return themeMode; }
            set
            {
                themeMode = value;
                switch (themeMode)
                {
                    case HThemeMode.Default:
                        this.HelpBackColor = HColors.Whites.White;
                        this.HelpBorderColor = HColors.Blacks.Void;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Whites.White;
                        this.ViewBorderColor = HColors.Blacks.Void;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Grays.Silver;
                        this.BackColor = HColors.Whites.White;
                        this.CommandsActiveLinkColor = HColors.Blacks.Black;
                        this.CommandsBackColor = HColors.Grays.Gainsboro;
                        this.CommandsBorderColor = HColors.Whites.White;
                        this.CommandsDisabledLinkColor = HColors.Grays.Silver;
                        this.CommandsForeColor = HColors.Blacks.Black;
                        this.CommandsLinkColor = HColors.Blacks.Black;
                        break;
                    case HThemeMode.Black:
                        this.HelpBackColor = HColors.Blacks.Jet;
                        this.HelpBorderColor = HColors.Blacks.Onyx;
                        this.HelpForeColor = HColors.Grays.Silver;
                        this.ViewBackColor = HColors.Blacks.DarkNight;
                        this.ViewBorderColor = HColors.Blacks.Onyx;
                        this.ViewForeColor = HColors.Grays.WhiteSmoke;
                        this.LineColor = HColors.Grays.DimGray;
                        this.BackColor = HColors.Blacks.CharcoalBlack;
                        this.CommandsActiveLinkColor = HColors.Grays.WhiteSmoke;
                        this.CommandsBackColor = HColors.Blacks.Jet;
                        this.CommandsBorderColor = HColors.Blacks.Onyx;
                        this.CommandsDisabledLinkColor = HColors.Grays.DimGray;
                        this.CommandsForeColor = HColors.Grays.WhiteSmoke;
                        this.CommandsLinkColor = HColors.Grays.Silver;
                        break;
                    case HThemeMode.Warm:
                        this.HelpBackColor = HColors.Browns.Latte;
                        this.HelpBorderColor = HColors.Browns.Coffee;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Browns.Tan;
                        this.ViewBorderColor = HColors.Browns.Coffee;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Browns.Sienna;
                        this.BackColor = HColors.Browns.Beige;
                        this.CommandsActiveLinkColor = HColors.Reds.Crimson;
                        this.CommandsBackColor = HColors.Browns.Camel;
                        this.CommandsBorderColor = HColors.Browns.Coffee;
                        this.CommandsDisabledLinkColor = HColors.Grays.Silver;
                        this.CommandsForeColor = HColors.Blacks.Black;
                        this.CommandsLinkColor = HColors.Reds.BrickRed;
                        break;
                    case HThemeMode.Blue:
                        this.HelpBackColor = HColors.Blues.PowderBlue;
                        this.HelpBorderColor = HColors.Blues.SteelBlue;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Blues.AliceBlue;
                        this.ViewBorderColor = HColors.Blues.CornflowerBlue;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Blues.DodgerBlue;
                        this.BackColor = HColors.Blues.LightBlue;
                        this.CommandsActiveLinkColor = HColors.Blues.Navy;
                        this.CommandsBackColor = HColors.Blues.RoyalBlue;
                        this.CommandsBorderColor = HColors.Blues.SteelBlue;
                        this.CommandsDisabledLinkColor = HColors.Grays.Silver;
                        this.CommandsForeColor = HColors.Whites.White;
                        this.CommandsLinkColor = HColors.Whites.White;
                        break;
                    case HThemeMode.Green:
                        this.HelpBackColor = HColors.Greens.MintCream;
                        this.HelpBorderColor = HColors.Greens.SeaGreen;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Greens.PaleGreen;
                        this.ViewBorderColor = HColors.Greens.ForestGreen;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Greens.MediumSeaGreen;
                        this.BackColor = HColors.Greens.AppleGreen;
                        this.CommandsActiveLinkColor = HColors.Greens.DarkGreen;
                        this.CommandsBackColor = HColors.Greens.SeaGreen;
                        this.CommandsBorderColor = HColors.Greens.ForestGreen;
                        this.CommandsDisabledLinkColor = HColors.Grays.LightGray;
                        this.CommandsForeColor = HColors.Whites.White;
                        this.CommandsLinkColor = HColors.Whites.White;
                        break;
                    case HThemeMode.Yellow:
                        this.HelpBackColor = HColors.Yellows.LightYellow;
                        this.HelpBorderColor = HColors.Yellows.Gold;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Yellows.Canary;
                        this.ViewBorderColor = HColors.Yellows.GoldenYellow;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Yellows.Mustard;
                        this.BackColor = HColors.Yellows.LemonChiffon;
                        this.CommandsActiveLinkColor = HColors.Browns.Chestnut;
                        this.CommandsBackColor = HColors.Yellows.Gold;
                        this.CommandsBorderColor = HColors.Yellows.Ochre;
                        this.CommandsDisabledLinkColor = HColors.Grays.Silver;
                        this.CommandsForeColor = HColors.Blacks.Black;
                        this.CommandsLinkColor = HColors.Blacks.Black;
                        break;
                    case HThemeMode.Red:
                        this.HelpBackColor = HColors.Pinks.LavenderBlush;
                        this.HelpBorderColor = HColors.Reds.Crimson;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Whites.Snow;
                        this.ViewBorderColor = HColors.Reds.FireBrick;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Reds.IndianRed;
                        this.BackColor = HColors.Whites.White;
                        this.CommandsActiveLinkColor = HColors.Reds.Maroon;
                        this.CommandsBackColor = HColors.Reds.Crimson;
                        this.CommandsBorderColor = HColors.Reds.DarkRed;
                        this.CommandsDisabledLinkColor = HColors.Grays.Silver;
                        this.CommandsForeColor = HColors.Whites.White;
                        this.CommandsLinkColor = HColors.Whites.White;
                        break;
                    case HThemeMode.Orange:
                        this.HelpBackColor = HColors.Oranges.Papaya;
                        this.HelpBorderColor = HColors.Oranges.DarkOrange;
                        this.HelpForeColor = HColors.Blacks.Black;
                        this.ViewBackColor = HColors.Oranges.Peach;
                        this.ViewBorderColor = HColors.Oranges.Orange;
                        this.ViewForeColor = HColors.Blacks.Black;
                        this.LineColor = HColors.Oranges.Tangerine;
                        this.BackColor = HColors.Oranges.Apricot;
                        this.CommandsActiveLinkColor = HColors.Oranges.Flame;
                        this.CommandsBackColor = HColors.Oranges.DarkOrange;
                        this.CommandsBorderColor = HColors.Oranges.Pumpkin;
                        this.CommandsDisabledLinkColor = HColors.Grays.Silver;
                        this.CommandsForeColor = HColors.Whites.White;
                        this.CommandsLinkColor = HColors.Whites.White;
                        break;
                }
                this.Refresh();
            }
        }
    }
}