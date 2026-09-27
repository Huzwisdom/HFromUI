using HFromUI.HAttribute;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Panel
{
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 横向标签
    /// </summary>
    public class HTabControlL : TabControl
    {
        private Color _tabBackColor;
        private Color _selectedTabColor;

        /// <summary>
        /// 标签背景色
        /// </summary>
        [HCategoryLanguage("自定义属性"), HDisplayNameLanguage("选项卡背景色"), HDescriptionLanguage("获取或设置背景颜色"), Browsable(true)]
        public Color TabBackColor
        {
            get { return _tabBackColor; }
            set
            {
                if (value != _tabBackColor)
                {
                    _tabBackColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 选中颜色
        /// </summary>
        [HCategoryLanguage("自定义属性"), HDisplayNameLanguage("选中选项卡颜色"), HDescriptionLanguage("获取或设置选中的颜色"), Browsable(true)]
        public Color SelectedTabColor
        {
            get { return _selectedTabColor; }
            set
            {
                if (value != _selectedTabColor)
                {
                    _selectedTabColor = value;
                    Invalidate();
                }
            }
        }

        /// <inheritdoc />
        public HTabControlL()
        {
            SetStyle(
                ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor |
                ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            Alignment = TabAlignment.Left;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(40, 80);
            _tabBackColor = SystemColors.Control;
            _selectedTabColor = Color.FromArgb(52, 152, 219);
            SelectedIndexChanged += TabControl_SelectedIndexChanged;
            SelectedIndex = 0;
        }

        /// <summary>PaintTabControlBackground 方法。</summary>
        private void PaintTabControlBackground(Graphics g, Color backColor)
        {
            var solidBrush = new SolidBrush(backColor);
            g.FillRectangle(solidBrush, ClientRectangle);
            solidBrush.Dispose();
        }

        /// <summary>PaintSelectTabBackground 方法。</summary>
        private void PaintSelectTabBackground(Graphics g, Color backColor)
        {
            if (SelectedIndex >= 0)
            {
                var num = 5;
                var solidBrush = new SolidBrush(backColor);
                var tabRect = GetTabRect(SelectedIndex);
                tabRect.Y++;
                tabRect.Height -= 2;
                var rect = default(Rectangle);
                rect.X = tabRect.X;
                rect.Width = tabRect.Width;
                rect.Y = tabRect.Y + num;
                rect.Height = tabRect.Height - 2 * num;
                g.FillRectangle(solidBrush, rect);
                rect.X = tabRect.X + num;
                rect.Width = tabRect.Width - 2 * num;
                rect.Y = tabRect.Y;
                rect.Height = tabRect.Height;
                g.FillRectangle(solidBrush, rect);
                rect.X = tabRect.X;
                rect.Y = tabRect.Y;
                rect.Width = 2 * num + 1;
                rect.Height = 2 * num + 1;
                g.FillPie(solidBrush, rect, 180f, 90f);
                rect.X = tabRect.Right - 2 * num - 1;
                rect.Y = tabRect.Y;
                rect.Width = 2 * num + 1;
                rect.Height = 2 * num + 1;
                g.FillPie(solidBrush, rect, 270f, 90f);
                rect.X = tabRect.Right - 2 * num - 1;
                rect.Y = tabRect.Bottom - 2 * num - 1;
                rect.Width = 2 * num + 1;
                rect.Height = 2 * num + 1;
                g.FillPie(solidBrush, rect, 0f, 90f);
                rect.X = tabRect.X;
                rect.Y = tabRect.Bottom - 2 * num - 1;
                rect.Width = 2 * num + 1;
                rect.Height = 2 * num + 1;
                g.FillPie(solidBrush, rect, 90f, 90f);
                solidBrush.Dispose();
            }
        }

        /// <summary>PaintSelectTabFeature 方法。</summary>
        private void PaintSelectTabFeature(Graphics g)
        {
            if (SelectedIndex >= 0)
            {
                var brush = new SolidBrush(SelectedTab.BackColor);
                var tabRect = GetTabRect(SelectedIndex);
                tabRect.Y++;
                tabRect.Width += 2;
                tabRect.Height -= 2;
                var pointF = new PointF(tabRect.Right, tabRect.Y + tabRect.Height / 2 - 10);
                var pointF2 = new PointF(tabRect.Right, tabRect.Y + tabRect.Height / 2 + 10);
                var pointF3 = new PointF(tabRect.Right - 10, tabRect.Y + tabRect.Height / 2);
                var points = new[] { pointF, pointF2, pointF3 };
                g.FillPolygon(brush, points);
                brush.Dispose();
            }
        }

        /// <summary>PaintTabText 方法。</summary>
        private void PaintTabText(Graphics g)
        {
            var solidBrush = new SolidBrush(_selectedTabColor);
            for (var i = 0; i < TabCount; i++)
            {
                var tabRect = GetTabRect(i);
                var textSize = TextRenderer.MeasureText(TabPages[i].Text, Font);
                var textPoint = new Point
                {
                    X = tabRect.X + (tabRect.Width-textSize.Width)/2, Y = tabRect.Y + (tabRect.Height - textSize.Height) / 2 + 1
                };
                solidBrush.Color = i == SelectedIndex ? _tabBackColor : _selectedTabColor;
                g.DrawString(TabPages[i].Text, Font, solidBrush, textPoint.X, textPoint.Y);
            }

            solidBrush.Dispose();
        }

        /// <summary>TabControl_SelectedIndexChanged 方法。</summary>
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            var graphics = CreateGraphics();
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            PaintTabControlBackground(graphics, _tabBackColor);
            PaintSelectTabBackground(graphics, _selectedTabColor);
            PaintSelectTabFeature(graphics);
            PaintTabText(graphics);
            graphics.Dispose();
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            PaintTabControlBackground(e.Graphics, _tabBackColor);
            PaintSelectTabBackground(e.Graphics, _selectedTabColor);
            PaintSelectTabFeature(e.Graphics);
            PaintTabText(e.Graphics);
        }

        private IContainer components;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>InitializeComponent 方法。</summary>
        private void InitializeComponent()
        {
            components = new Container();
        }
    }
}