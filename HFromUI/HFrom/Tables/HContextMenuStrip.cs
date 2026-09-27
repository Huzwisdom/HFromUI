using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    public partial class HContextMenuStrip : ContextMenuStrip
    {
        /// <summary>myRender 字段。</summary>
        private HMenuRender myRender = new HMenuRender();

        /// <summary>
        /// 菜单字体颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("菜单字体颜色"), HDescriptionLanguage("菜单字体颜色"), Browsable(true)]
        public Color FontColor
        {
            get { return myRender.FontColor; }
            set { myRender.FontColor = value; this.Renderer = myRender; this.Invalidate(); }//设置渲染
        }

        /// <summary>
        /// 下拉菜单坐标图标区域开始颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("外边距起始颜色"), HDescriptionLanguage("下拉菜单坐标图标区域开始颜色"), Browsable(true)]
        public Color MarginStartColor
        {
            get { return myRender.MarginStartColor; }
            set { myRender.MarginStartColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 下拉菜单坐标图标区域结束颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("外边距结束颜色"), HDescriptionLanguage("下拉菜单坐标图标区域结束颜色"), Browsable(true)]
        public Color MarginEndColor
        {
            get { return myRender.MarginEndColor; }
            set { myRender.MarginEndColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 下拉项背景颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("下拉项背景颜色"), HDescriptionLanguage("下拉项背景颜色"), Browsable(true)]
        public Color DropDownItemBackColor
        {
            get { return myRender.DropDownItemBackColor; }
            set { myRender.DropDownItemBackColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 下拉项选中时开始颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("下拉项选中时开始颜色"), HDescriptionLanguage("下拉项选中时开始颜色"), Browsable(true)]
        public Color DropDownItemStartColor
        {
            get { return myRender.DropDownItemStartColor; }
            set { myRender.DropDownItemStartColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 下拉项选中时结束颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("下拉项选中时结束颜色"), HDescriptionLanguage("下拉项选中时结束颜色"), Browsable(true)]
        public Color DropDownItemEndColor
        {
            get { return myRender.DropDownItemEndColor; }
            set { myRender.DropDownItemEndColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 主菜单项选中时的开始颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("菜单项起始色"), HDescriptionLanguage("主菜单项选中时的开始颜色"), Browsable(true)]
        public Color MenuItemStartColor
        {
            get { return myRender.MenuItemStartColor; }
            set { myRender.MenuItemStartColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 主菜单项选中时的结束颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("菜单项结束色"), HDescriptionLanguage("主菜单项选中时的结束颜色"), Browsable(true)]
        public Color MenuItemEndColor
        {
            get { return myRender.MenuItemEndColor; }
            set { myRender.MenuItemEndColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 分割线颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("分割线颜色"), HDescriptionLanguage("分割线颜色"), Browsable(true)]
        public Color SeparatorColor
        {
            get { return myRender.SeparatorColor; }
            set { myRender.SeparatorColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 主菜单背景色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("主菜单背景色"), HDescriptionLanguage("主菜单背景色"), Browsable(true)]
        public Color MainMenuBackColor
        {
            get { return myRender.MainMenuBackColor; }
            set { myRender.MainMenuBackColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 主菜单背景开始颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("主菜单背景开始颜色"), HDescriptionLanguage("主菜单背景开始颜色"), Browsable(true)]
        public Color MainMenuStartColor
        {
            get { return myRender.MainMenuStartColor; }
            set { myRender.MainMenuStartColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 主菜单背景结束颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("主菜单背景结束颜色"), HDescriptionLanguage("主菜单背景结束颜色"), Browsable(true)]
        public Color MainMenuEndColor
        {
            get { return myRender.MainMenuEndColor; }
            set { myRender.MainMenuEndColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 下拉区域边框颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("下拉区域边框颜色"), HDescriptionLanguage("下拉区域边框颜色"), Browsable(true)]
        public Color DropDownBorder
        {
            get { return myRender.DropDownBorder; }
            set { myRender.DropDownBorder = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 箭头颜色
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("箭头颜色"), HDescriptionLanguage("箭头颜色"), Browsable(true)]
        public Color ArrowColor
        {
            get { return myRender.ArrowColor; }
            set { myRender.ArrowColor = value; this.Renderer = myRender; this.Invalidate(); }
        }

        /// <summary>
        /// 下拉项选中时的圆角半径
        /// </summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("下拉项圆角"), HDescriptionLanguage("下拉项选中时的圆角半径"), Browsable(true)]
        public int DropDownItemRadius
        {
            get { return myRender.DropDownItemRadius; }
            set { myRender.DropDownItemRadius = value; this.Renderer = myRender; this.Invalidate(); }
        }

        public HContextMenuStrip()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            myRender.RoundedEdges = true;
            this.Renderer = myRender;
        }

        public HContextMenuStrip(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            myRender.RoundedEdges = true;
            this.Renderer = myRender;//设置渲染
        }

        /// <summary>CreateRound 方法。</summary>
        public static GraphicsPath CreateRound(Rectangle rect, int radius)
        {
            GraphicsPath roundRect = new GraphicsPath();
            int halfradius = radius % 2 == 0 ? radius / 2 : radius / 2 + 1;
            if (radius != 0)
            {
                //顶端
                //roundRect.AddLine(rect.Left + halfradius-1, rect.Top, rect.Right - halfradius, rect.Top);
                //右上角
                roundRect.AddArc(rect.Right - radius, rect.Top, radius, radius, 270f, 90f);
                //右边
                //roundRect.AddLine(rect.Right, rect.Top + halfradius, rect.Right, rect.Bottom - halfradius);
                //右下角

                roundRect.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0f, 90f);
                //底边
                //roundRect.AddLine(rect.Right - halfradius, rect.Bottom, rect.Left + halfradius, rect.Bottom);
                //左下角
                roundRect.AddArc(rect.Left, rect.Bottom - radius, radius, radius, 90f, 90f);
                //左边
                //roundRect.AddLine(rect.Left, rect.Top + halfradius, rect.Left, rect.Bottom - halfradius);
                //左上角
                roundRect.AddArc(rect.Left, rect.Top, radius, radius, 180f, 90f);

                roundRect.CloseFigure();
            }
            else
            {
                //顶端
                roundRect.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
                //右边
                roundRect.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
                //底边
                roundRect.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
                //左边
                roundRect.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom);
            }

            return roundRect;
        }

        /// <summary>CreateRound 方法。</summary>
        public static GraphicsPath CreateRound(RectangleF rect, float radius)
        {
            GraphicsPath roundRect = new GraphicsPath();
            float halfradius = radius % 2 == 0 ? radius / 2 : radius / 2 + 1;
            if (radius != 0)
            {
                //顶端
                //roundRect.AddLine(rect.Left + halfradius-1, rect.Top, rect.Right - halfradius, rect.Top);
                //右上角
                roundRect.AddArc(rect.Right - radius, rect.Top, radius, radius, 270f, 90f);
                //右边
                //roundRect.AddLine(rect.Right, rect.Top + halfradius, rect.Right, rect.Bottom - halfradius);
                //右下角

                roundRect.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0f, 90f);
                //底边
                //roundRect.AddLine(rect.Right - halfradius, rect.Bottom, rect.Left + halfradius, rect.Bottom);
                //左下角
                roundRect.AddArc(rect.Left, rect.Bottom - radius, radius, radius, 90f, 90f);
                //左边
                //roundRect.AddLine(rect.Left, rect.Top + halfradius, rect.Left, rect.Bottom - halfradius);
                //左上角
                roundRect.AddArc(rect.Left, rect.Top, radius, radius, 180f, 90f);

                roundRect.CloseFigure();
            }
            else
            {
                //顶端
                roundRect.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
                //右边
                roundRect.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom);
                //底边
                roundRect.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
                //左边
                roundRect.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom);
            }

            return roundRect;
        }
    }
}