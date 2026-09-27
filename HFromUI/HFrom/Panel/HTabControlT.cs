using HFromUI.HAttribute;

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.Panel
{
    using HFromUI.HColor;
    using Panel = System.Windows.Forms.Panel;
    /// <summary>
    /// 纵向标签
    /// </summary>
    public class HTabControlT : TabControl
    {
        private Image mBackImage;
        private int _imageDistanceToTop;
        private int _textDistanceToBottom;
        private Color _tabColor;

        /// <summary>
        /// 图片顶端距离
        /// </summary>
        [HCategoryLanguage("自定义属性"), HDisplayNameLanguage("图片距顶部距离"), HDescriptionLanguage("获取或设置图像到顶部的距离"), Browsable(true)]
        public int ImageDistanceToTop
        {
            get { return _imageDistanceToTop; }
            set
            {
                _imageDistanceToTop = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 文本底部距离
        /// </summary>
        [HCategoryLanguage("自定义属性"), HDisplayNameLanguage("文本距底部距离"), HDescriptionLanguage("获取或设置文本到底部的距离"), Browsable(true)]
        public int TextDistanceToBottom
        {
            get { return _textDistanceToBottom; }
            set
            {
                _textDistanceToBottom = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 标签颜色
        /// </summary>
        [HCategoryLanguage("自定义属性"), HDisplayNameLanguage("选项卡颜色"), HDescriptionLanguage("获取或设置背景颜色"), Browsable(true)]
        public Color TabColor
        {
            get { return _tabColor; }
            set
            {
                _tabColor = value;
                Invalidate();
            }
        }

        /// <inheritdoc />
        public HTabControlT()
        {
            SetStyle(ControlStyles.UserPaint
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.SupportsTransparentBackColor
                     | ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(100, 100);
            mBackImage = HPhoto.Get("zoom_actual");
            _imageDistanceToTop = 5;
            _textDistanceToBottom = 5;
            _tabColor = SystemColors.Control;
            InitializeComponent();
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            using (var brush = new SolidBrush(_tabColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            for (var i = 0; i < TabCount; i++)
            {
                var tabRect = GetTabRect(i);
                if (SelectedIndex == i)
                {
                    e.Graphics.DrawImage(mBackImage, tabRect);
                }

                var textSize = TextRenderer.MeasureText(TabPages[i].Text, Font);
                var textLocation = new PointF
                {
                    X = tabRect.X + (tabRect.Width - textSize.Width) / 2f + 2f,
                    Y = tabRect.Bottom - textSize.Height - _textDistanceToBottom
                };
                e.Graphics.DrawString(TabPages[i].Text, Font, SystemBrushes.ControlText, textLocation.X, textLocation.Y);

                if (ImageList != null)
                {
                    if (TabPages[i].ImageIndex >= 0 && TabPages[i].ImageIndex < ImageList.Images.Count)
                    {
                        using (var image = ImageList.Images[TabPages[i].ImageIndex])
                        {
                            e.Graphics.DrawImage(image, new Point(tabRect.X + (tabRect.Width - image.Width) / 2, tabRect.Top + _imageDistanceToTop));
                        }
                    }
                }
            }

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