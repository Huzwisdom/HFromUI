using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    using MenuStrip = System.Windows.Forms.MenuStrip;
    public class HMenuRender : ToolStripProfessionalRenderer
    {
        /// <summary>_fontcolor 字段。</summary>
        private Color _fontcolor = Color.Black;

        /// <summary>
        /// 菜单字体颜色
        /// </summary>
        public Color FontColor
        {
            get { return this._fontcolor; }
            set { this._fontcolor = value; }
        }

        /// <summary>_marginstartcolor 字段。</summary>
        private Color _marginstartcolor = Color.White;

        /// <summary>
        /// 下拉菜单坐标图标区域开始颜色
        /// </summary>
        public Color MarginStartColor
        {
            get { return this._marginstartcolor; }
            set { this._marginstartcolor = value; }
        }

        /// <summary>_marginendcolor 字段。</summary>
        private Color _marginendcolor = Color.DodgerBlue;

        /// <summary>
        /// 下拉菜单坐标图标区域结束颜色
        /// </summary>
        public Color MarginEndColor
        {
            get { return this._marginendcolor; }
            set { this._marginendcolor = value; }
        }

        /// <summary>_dropdownitembackcolor 字段。</summary>
        private Color _dropdownitembackcolor = Color.DodgerBlue;

        /// <summary>
        /// 下拉项背景颜色
        /// </summary>
        public Color DropDownItemBackColor
        {
            get { return this._dropdownitembackcolor; }
            set { this._dropdownitembackcolor = value; }
        }

        /// <summary>_dropdownitemstartcolor 字段。</summary>
        private Color _dropdownitemstartcolor = Color.DeepSkyBlue;

        /// <summary>
        /// 下拉项选中时开始颜色
        /// </summary>
        public Color DropDownItemStartColor
        {
            get { return this._dropdownitemstartcolor; }
            set { this._dropdownitemstartcolor = value; }
        }

        /// <summary>_dorpdownitemendcolor 字段。</summary>
        private Color _dorpdownitemendcolor = Color.LightSkyBlue;

        /// <summary>
        /// 下拉项选中时结束颜色
        /// </summary>
        public Color DropDownItemEndColor
        {
            get { return this._dorpdownitemendcolor; }
            set { this._dorpdownitemendcolor = value; }
        }

        /// <summary>_menuitemstartcolor 字段。</summary>
        private Color _menuitemstartcolor = Color.DodgerBlue;

        /// <summary>
        /// 主菜单项选中时的开始颜色
        /// </summary>
        public Color MenuItemStartColor
        {
            get { return this._menuitemstartcolor; }
            set { this._menuitemstartcolor = value; }
        }

        /// <summary>_menuitemendcolor 字段。</summary>
        private Color _menuitemendcolor = Color.DodgerBlue;

        /// <summary>
        /// 主菜单项选中时的结束颜色
        /// </summary>
        public Color MenuItemEndColor
        {
            get { return this._menuitemendcolor; }
            set { this._menuitemendcolor = value; }
        }

        /// <summary>_separatorcolor 字段。</summary>
        private Color _separatorcolor = Color.Gray;

        /// <summary>
        /// 分割线颜色
        /// </summary>
        public Color SeparatorColor
        {
            get { return this._separatorcolor; }
            set { this._separatorcolor = value; }
        }

        /// <summary>_mainmenubackcolor 字段。</summary>
        private Color _mainmenubackcolor = Color.White;

        /// <summary>
        /// 主菜单背景色
        /// </summary>
        public Color MainMenuBackColor
        {
            get { return this._mainmenubackcolor; }
            set { this._mainmenubackcolor = value; }
        }

        /// <summary>_mainmenustartcolor 字段。</summary>
        private Color _mainmenustartcolor = Color.White;

        /// <summary>
        /// 主菜单背景开始颜色
        /// </summary>
        public Color MainMenuStartColor
        {
            get { return this._mainmenustartcolor; }
            set { this._mainmenustartcolor = value; }
        }

        /// <summary>_mainmenuendcolor 字段。</summary>
        private Color _mainmenuendcolor = Color.White;

        /// <summary>
        /// 主菜单背景结束颜色
        /// </summary>
        public Color MainMenuEndColor
        {
            get { return this._mainmenuendcolor; }
            set { this._mainmenuendcolor = value; }
        }

        /// <summary>_dropdownborder 字段。</summary>
        private Color _dropdownborder = Color.DodgerBlue;

        /// <summary>
        /// 下拉区域边框颜色
        /// </summary>
        public Color DropDownBorder
        {
            get { return this._dropdownborder; }
            set { this._dropdownborder = value; }
        }

        /// <summary>dropDownBorderRadius 字段。</summary>
        private int dropDownBorderRadius = 0;

        /// <summary>
        /// 下拉区域边框圆角半径
        /// </summary>
        public int DropDownBorderRadius
        {
            get { return this.dropDownBorderRadius; }
            set { this.dropDownBorderRadius = value; }
        }

        /// <summary>_arrowcolor 字段。</summary>
        private Color _arrowcolor = Color.DodgerBlue;

        /// <summary>
        /// 箭头颜色
        /// </summary>
        public Color ArrowColor
        {
            get { return _arrowcolor; }
            set { _arrowcolor = value; }
        }

        /// <summary>dropdownItemRadius 字段。</summary>
        private int dropdownItemRadius = 15;

        /// <summary>
        /// 下拉项选中时的圆角半径
        /// </summary>
        public int DropDownItemRadius
        {
            get { return dropdownItemRadius; }
            set { dropdownItemRadius = value; }
        }

        /// <summary>
        /// 渲染整个背景
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            e.ToolStrip.ForeColor = FontColor;
            //如果是下拉
            if (e.ToolStrip is ToolStripDropDown)
            {
                e.Graphics.FillRectangle(new SolidBrush(DropDownItemBackColor), e.AffectedBounds);
            }
            //如果是菜单项
            else if (e.ToolStrip is MenuStrip)
            {
                Blend blend = new Blend();
                float[] fs = new float[5] { 0f, 0.3f, 0.5f, 0.8f, 1f };
                float[] f = new float[5] { 0f, 0.5f, 0.9f, 0.5f, 0f };
                blend.Positions = fs;
                blend.Factors = f;
                FillLineGradient(e.Graphics, e.AffectedBounds, MainMenuStartColor, MainMenuEndColor, 90f, blend, 0);
            }
            else
            {
                base.OnRenderToolStripBackground(e);
            }
        }

        /// <summary>
        /// 渲染下拉左侧图标区域
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            FillLineGradient(e.Graphics, e.AffectedBounds, MarginStartColor, MarginEndColor, 0f, null, 0);
        }

        /// <summary>
        /// 渲染菜单项的背景
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.ToolStrip is MenuStrip)
            {
                //如果被选中或被按下
                if (e.Item.Selected || e.Item.Pressed)
                {
                    Blend blend = new Blend();
                    float[] fs = new float[5] { 0f, 0.3f, 0.5f, 0.8f, 1f };
                    float[] f = new float[5] { 0f, 0.5f, 1f, 0.5f, 0f };
                    blend.Positions = fs;
                    blend.Factors = f;
                    FillLineGradient(e.Graphics, new Rectangle(0, 0, e.Item.Size.Width, e.Item.Size.Height), MenuItemStartColor, MenuItemEndColor, 90f, blend, dropdownItemRadius);
                }
                else
                    base.OnRenderMenuItemBackground(e);
            }
            else if (e.ToolStrip is ToolStripDropDown)
            {
                if (e.Item.Selected)
                {
                    FillLineGradient(e.Graphics, new Rectangle(0, 0, e.Item.Size.Width, e.Item.Size.Height), DropDownItemStartColor, DropDownItemEndColor, 90f, null, dropdownItemRadius);
                }
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }

        /// <summary>
        /// 渲染菜单项的分隔线
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(SeparatorColor), 0, 2, e.Item.Width, 2);
        }

        /// <summary>
        /// 渲染边框
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
            {
                e.Graphics.DrawPath(new Pen(DropDownBorder), CreateRound(new Rectangle(0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1), dropDownBorderRadius));
                //e.Graphics.DrawRectangle(new Pen(DropDownBorder), new Rectangle(0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
            }
            else
            {
                base.OnRenderToolStripBorder(e);
            }
        }

        /// <summary>
        /// 渲染箭头
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = ArrowColor;//设置为红色，当然还可以 画出各种形状
            base.OnRenderArrow(e);
        }

        /// <summary>
        /// 填充线性渐变
        /// </summary>
        /// <param name="g">画布</param>
        /// <param name="rect">填充区域</param>
        /// <param name="startcolor">开始颜色</param>
        /// <param name="endcolor">结束颜色</param>
        /// <param name="angle">角度</param>
        /// <param name="blend">对象的混合图案</param>
        private void FillLineGradient(Graphics g, Rectangle rect, Color startcolor, Color endcolor, float angle, Blend blend, int radius)
        {
            LinearGradientBrush linebrush = new LinearGradientBrush(rect, startcolor, endcolor, angle);
            if (blend != null)
            {
                linebrush.Blend = blend;
            }
            GraphicsPath path = CreateRound(rect, radius);
            //path.AddRectangle(rect);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(linebrush, path);
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
