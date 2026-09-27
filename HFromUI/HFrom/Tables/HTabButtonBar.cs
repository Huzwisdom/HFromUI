using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tables
{
    public partial class HTabButtonBar : UserControl
    {
        #region 动画属性
        /// <summary>selectRectX 字段。</summary>
        private int selectRectX = 0;
        /// <summary>selectRectY 字段。</summary>
        private int selectRectY = 0;
        /// <summary>selectRectW 字段。</summary>
        private int selectRectW = 0;
        /// <summary>selectRectH 字段。</summary>
        private int selectRectH = 0;

        private HAnimation animation;

        [Browsable(false), HDescriptionLanguage("动画属性，不可手动修改")]
        public int SelectRectX
        { get { return selectRectX; } set { selectRectX = value; this.Invalidate(); } }

        [Browsable(false), HDescriptionLanguage("动画属性，不可手动修改")]
        public int SelectRectY
        { get { return selectRectY; } set { selectRectY = value; this.Invalidate(); } }

        [Browsable(false), HDescriptionLanguage("动画属性，不可手动修改")]
        public int SelectRectW
        { get { return selectRectW; } set { selectRectW = value; this.Invalidate(); } }

        [Browsable(false), HDescriptionLanguage("动画属性，不可手动修改")]
        public int SelectRectH
        { get { return selectRectH; } set { selectRectH = value; this.Invalidate(); } }
        #endregion

        #region 公共属性
        /// <summary>SelectIndex 成员。</summary>
        /// <summary>SelectIndex 字段。</summary>
        [HDescriptionLanguage("当前选中下标")]
        public int SelectIndex = 0;

        /// <summary>EnableAnime 成员。</summary>
        /// <summary>EnableAnime 字段。</summary>
        [HDescriptionLanguage("是否启用动画")]
        public bool EnableAnime { get; set; } = true;

        /// <summary>AnimationType 成员。</summary>
        /// <summary>AnimationType 字段。</summary>
        [HDescriptionLanguage("动画类型")]
        public AnimationType AnimationType { get; set; } = AnimationType.BounceOut;

        [HDescriptionLanguage("动画时间")]
        /// <summary>AnimationTime 成员。</summary>
        /// <summary>AnimationTime 字段。</summary>
        [Browsable(true)]
        public int AnimationTime { get; set; } = 500;

        /// <summary>Texts 成员。</summary>
        /// <summary>Texts 字段。</summary>
        [HDescriptionLanguage("选项按钮文本")]
        public List<string> Texts { get; set; } = new List<string> { "tabpage1", "tabpage2" };

        /// <summary>TextOffset 成员。</summary>
        /// <summary>TextOffset 字段。</summary>
        [HDescriptionLanguage("文本位移")]
        public Point TextOffset { get; set; } = new Point(0,0);

        /// <summary>TextAlignment 成员。</summary>
        /// <summary>TextAlignment 字段。</summary>
        [HDescriptionLanguage("文本对齐方式")]
        public StringAlignment TextAlignment { get; set; } = StringAlignment.Center;

        /// <summary>ShowImages 成员。</summary>
        /// <summary>ShowImages 字段。</summary>
        [HDescriptionLanguage("是否显示图片")]
        public bool ShowImages { get; set; } = false;

        /// <summary>Images 成员。</summary>
        /// <summary>Images 字段。</summary>
        [HDescriptionLanguage("图片列表")]
        public List<Bitmap> Images { get; set; } = new List<Bitmap>();

        /// <summary>ImageSize 成员。</summary>
        /// <summary>ImageSize 字段。</summary>
        [HDescriptionLanguage("图片大小")]
        public Size ImageSize { get; set; } = new Size(25, 25);

        /// <summary>ImageOffset 成员。</summary>
        /// <summary>ImageOffset 字段。</summary>
        [HDescriptionLanguage("图片位置")]
        public Point ImageOffset { get; set; } = new Point(0, 0);

        /// <summary>ButtonWidth 成员。</summary>
        /// <summary>ButtonWidth 字段。</summary>
        [HDescriptionLanguage("按钮的宽度，若为垂直排列时则为高度，ItemAutoSize为false时生效")]
        public int ButtonWidth { get; set; } = 100;

        /// <summary>ItemAutoSize 成员。</summary>
        /// <summary>ItemAutoSize 字段。</summary>
        [HDescriptionLanguage("启用自动宽度（高度）")]
        public bool ItemAutoSize { get; set; } = false;

        /// <summary>NormalForeColor 成员。</summary>
        /// <summary>NormalForeColor 字段。</summary>
        [HDescriptionLanguage("正常文字颜色")]
        public Color NormalForeColor { get; set; } = Color.Black;

        /// <summary>SelectForeColor 成员。</summary>
        /// <summary>SelectForeColor 字段。</summary>
        [HDescriptionLanguage("选中时文字颜色")]
        public Color SelectForeColor { get; set; } = Color.White;

        /// <summary>NormalBaseColor 成员。</summary>
        /// <summary>NormalBaseColor 字段。</summary>
        [HDescriptionLanguage("正常背景色")]
        public Color NormalBaseColor { get; set; } = Color.White;

        /// <summary>SelectBaseColor 成员。</summary>
        /// <summary>SelectBaseColor 字段。</summary>
        [HDescriptionLanguage("选中背景色")]
        public Color SelectBaseColor { get; set; } = Color.DimGray;

        /// <summary>SelectBarColor 成员。</summary>
        /// <summary>SelectBarColor 字段。</summary>
        [HDescriptionLanguage("选中条颜色")]
        public Color SelectBarColor { get; set; } = Color.DodgerBlue;

        /// <summary>MouseSelectColor 成员。</summary>
        /// <summary>MouseSelectColor 字段。</summary>
        [HDescriptionLanguage("鼠标选中背景色")]
        public Color MouseSelectColor { get; set; } = Color.Silver;

        [HDescriptionLanguage("选中条的高度，垂直排列时为宽度")]
        public int BarSize
        {
            get { return barSize; }
            set
            {
                if (value <= 0 || ((SelectBarPosition == selectBarPosition.Top || SelectBarPosition == selectBarPosition.Bottom) && value > this.Height) || ((SelectBarPosition == selectBarPosition.Left || SelectBarPosition == selectBarPosition.Right) && value > this.Width))
                    return;
                barSize = value;
            }
        }

        /// <summary>Direction 成员。</summary>
        /// <summary>Direction 字段。</summary>
        [HDescriptionLanguage("排列方向")]
        public direction Direction { get; set; } = direction.Horizontal;

        /// <summary>SelectBarPosition 成员。</summary>
        /// <summary>SelectBarPosition 字段。</summary>
        [HDescriptionLanguage("选中背景位置")]
        public selectBarPosition SelectBarPosition { get; set; } = selectBarPosition.Bottom;

        /// <summary>TextDrawMode 成员。</summary>
        /// <summary>TextDrawMode 字段。</summary>
        [HDescriptionLanguage("字符绘制方式")]
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Anti;

        [HDescriptionLanguage("绑定的TabControl控件")]
        public TabControl BindTabControl
        {
            get { return bindTabControl; }
            set
            {
                if (bindTabControl != null && !bindTabControl.IsDisposed)
                {
                    bindTabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
                    bindTabControl.ControlAdded -= TabControl_ControlChanged;
                    bindTabControl.ControlRemoved -= TabControl_ControlChanged;
                    bindTabControl.Disposed -= TabControl_Disposed;
                }

                bindTabControl = value;

                if (bindTabControl == null)
                {
                    Texts.Clear();
                    SelectItem(-1, false);
                    this.Invalidate();
                    return;
                }

                bindTabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
                bindTabControl.ControlAdded += TabControl_ControlChanged;
                bindTabControl.ControlRemoved += TabControl_ControlChanged;
                bindTabControl.Disposed += TabControl_Disposed;

                List<string> texts = new List<string>();

                foreach (TabPage page in bindTabControl.TabPages)
                {
                    texts.Add(page.Text);
                }
                Texts = texts;
                if (Texts.Count > 0)
                {
                    SelectItem(0, false);
                }
                this.Invalidate();
            }
        }
        #endregion

        #region 私有属性
        /// <summary>
        /// 绑定的TabControl控件
        /// </summary>
        private TabControl bindTabControl;

        /// <summary>
        /// 选中条的高度，垂直排列时为宽度
        /// </summary>
        private int barSize = 10;

        /// <summary>
        /// 鼠标选中的下标
        /// </summary>
        private int MouseSelectIndex = -1;
        #endregion

        #region 枚举
        public enum direction
        {
            Vertical,
            Horizontal
        }

        public enum selectBarPosition
        {
            Left,
            Top,
            Right,
            Bottom
        }
        #endregion

        public HTabButtonBar()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            base.SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor, true);
            base.UpdateStyles();

            animation = new HAnimation(this) { AnimationType = AnimationType,AnimationRunType=AnimationRunType.Interrupt };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;//用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        /// <summary>
        /// 绑定一个TabControl
        /// </summary>
        /// <param name="tabControl"></param>
        public void BindingTabControl(TabControl tabControl)
        {
            BindTabControl=tabControl;
        }

        /// <summary>TabControl_Disposed 方法。</summary>
        private void TabControl_Disposed(object sender, EventArgs e)
        {
            if (bindTabControl != null)
            {
                bindTabControl.SelectedIndexChanged -= TabControl_SelectedIndexChanged;
                bindTabControl.ControlAdded -= TabControl_ControlChanged;
                bindTabControl.ControlRemoved -= TabControl_ControlChanged;
                bindTabControl.Disposed -= TabControl_Disposed;
                bindTabControl = null;
            }
        }

        /// <summary>TabControl_ControlChanged 方法。</summary>
        private void TabControl_ControlChanged(object sender, ControlEventArgs e)
        {
            List<string> texts = new List<string>();
            foreach (TabPage page in (sender as TabControl).TabPages)
            {
                texts.Add(page.Text);
            }
            Texts = texts;
            this.Invalidate();
            SelectItem((sender as TabControl).SelectedIndex);
        }

        /// <summary>TabControl_SelectedIndexChanged 方法。</summary>
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectItem((sender as TabControl).SelectedIndex);
        }

        /// <summary>响应 Load 事件。</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SelectItem(0, false);
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Texts.Count() == 0)
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            g.TextRenderingHint = TextDrawMode ==HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.Clear(NormalBaseColor);//先画背景色

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = TextAlignment;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.FormatFlags = StringFormatFlags.NoClip;

            float itemWidth = ButtonWidth;

            if (ItemAutoSize)//自动宽度
            {
                if (Texts.Count() > 0)
                {
                    if (Direction == direction.Vertical)
                    {
                        itemWidth = (float)this.Height / Texts.Count();
                    }
                    else
                    {
                        itemWidth = (float)this.Width / Texts.Count();
                    }
                }
            }

            SolidBrush BrushText = new SolidBrush(NormalForeColor);
            SolidBrush BrushTextSel = new SolidBrush(SelectForeColor);
            SolidBrush BrushSelectBase = new SolidBrush(SelectBaseColor);
            SolidBrush BrushMouseSelectBase = new SolidBrush(MouseSelectColor);
            for (int i = 0; i < Texts.Count(); i++)
            {
                Rectangle rect = new Rectangle((int)(i * itemWidth), 0, (int)itemWidth, this.Height);
                if (Direction == direction.Vertical)
                {
                    rect = new Rectangle(0, (int)(i * itemWidth), this.Width, (int)itemWidth);
                }
                var textRect=new Rectangle(rect.X+TextOffset.X,rect.Y+TextOffset.Y, rect.Width, rect.Height);

                if (i == SelectIndex)
                {
                    g.FillRectangle(BrushSelectBase, rect);//画选中背景色
                    if (MouseSelectIndex == i)
                    {
                        g.FillRectangle(BrushMouseSelectBase, rect);//画选中背景色
                    }

                    g.DrawString(Texts[i], this.Font, BrushTextSel, textRect, stringFormat);
                }
                else
                {
                    if (MouseSelectIndex == i)
                    {
                        g.FillRectangle(BrushMouseSelectBase, rect);//画选中背景色
                    }
                    g.DrawString(Texts[i], this.Font, BrushText, textRect, stringFormat);
                }

                if (ShowImages)
                {
                    if (Images.Count - 1 >= i)
                    {
                        Image img = Images[i];
                        if (img != null)
                        {
                            g.DrawImage(img, new Rectangle(rect.Left + ImageOffset.X, rect.Top + ImageOffset.Y, ImageSize.Width, ImageSize.Height));
                        }
                    }
                }
            }

            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            SolidBrush selectBarBrush = new SolidBrush(SelectBarColor);
            g.FillRectangle(selectBarBrush, new Rectangle(SelectRectX, SelectRectY, SelectRectW, SelectRectH));//画选中条背景色

            BrushText.Dispose();
            BrushTextSel.Dispose();
            BrushSelectBase.Dispose();
            BrushMouseSelectBase.Dispose();
            selectBarBrush.Dispose();
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int count = Texts.Count();
            if (count == 0)
                return;
            int x = e.X;
            int y = e.Y;
            int mouseIndex = -1;
            if (Direction == direction.Horizontal)
            {
                float itemWidth = ButtonWidth;
                if (ItemAutoSize)//自动宽度
                {
                    if (Texts.Count() > 0)
                    {
                        itemWidth = (float)this.Width / Texts.Count();
                    }
                }
                mouseIndex = (e.X / (int)itemWidth);
            }
            else
            {
                float itemWidth = ButtonWidth;
                if (ItemAutoSize)//自动宽度
                {
                    if (Texts.Count() > 0)
                    {
                        itemWidth = (float)this.Height / Texts.Count();
                    }
                }
                mouseIndex = (e.Y / (int)itemWidth);
            }

            bool changed = MouseSelectIndex == mouseIndex;

            if (mouseIndex >= 0 && mouseIndex < Texts.Count())
            {
                MouseSelectIndex = mouseIndex;
                this.Cursor = Cursors.Hand;
            }
            else
            {
                MouseSelectIndex = -1;
                this.Cursor = Cursors.Default;
            }

            if (changed)
            {
                this.Invalidate();
            }
        }

        /// <summary>响应 MouseLeave 事件。</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            MouseSelectIndex = -1;
            this.Cursor = Cursors.Default;
            this.Invalidate();
        }

        /// <summary>响应 MouseClick 事件。</summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int count = Texts.Count();
            if (count == 0)
                return;
            int x = e.X;
            int y = e.Y;
            if (Direction == direction.Horizontal)
            {
                float itemWidth = ButtonWidth;
                if (ItemAutoSize)//自动宽度
                {
                    if (Texts.Count() > 0)
                    {
                        itemWidth = (float)this.Width / Texts.Count();
                    }
                }
                int mouseIndex = (e.X / (int)itemWidth);
                if (mouseIndex > count - 1)
                    return;
                SelectItem(mouseIndex);
            }
            else
            {
                float itemWidth = ButtonWidth;
                if (ItemAutoSize)//自动宽度
                {
                    if (Texts.Count() > 0)
                    {
                        itemWidth = (float)this.Height / Texts.Count();
                    }
                }
                int mouseIndex = (e.Y / (int)itemWidth);
                if (mouseIndex > count - 1)
                    return;
                SelectItem(mouseIndex);
            }
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SelectItem(this.SelectIndex, false);
        }

        /// <summary>SelectItem 方法。</summary>
        public void SelectItem(int index, bool userAnim = true)
        {
            float itemWidth = ButtonWidth;
            if (ItemAutoSize)//自动宽度
            {
                if (Texts.Count() > 0)
                {
                    if (Direction == direction.Vertical)
                    {
                        itemWidth = (float)this.Height / Texts.Count();
                    }
                    else
                    {
                        itemWidth = (float)this.Width / Texts.Count();
                    }
                }
            }

            Rectangle rect = new Rectangle((int)(index * itemWidth), 0, (int)itemWidth, this.Height);
            if (Direction == direction.Vertical)
            {
                rect = new Rectangle(0, (int)(index * itemWidth), this.Width, (int)itemWidth);
            }

            switch (SelectBarPosition)
            {
                case selectBarPosition.Top: rect = new Rectangle(rect.Left, rect.Top, rect.Width, barSize); break;
                case selectBarPosition.Bottom: rect = new Rectangle(rect.Left, rect.Top + (rect.Height - barSize), rect.Width, barSize); break;
                case selectBarPosition.Left: rect = new Rectangle(rect.Left, rect.Top, barSize, (int)itemWidth); break;
                case selectBarPosition.Right: rect = new Rectangle(rect.Left + (rect.Width - barSize), rect.Top, barSize, (int)itemWidth); break;
            }

            if (userAnim && EnableAnime && !DesignMode)
            {
                Dictionary<string, float> dic = new Dictionary<string, float>();
                dic.Add("SelectRectX", rect.X);
                dic.Add("SelectRectY", rect.Y);
                dic.Add("SelectRectW", rect.Width);
                dic.Add("SelectRectH", rect.Height);

                animation.AnimationControl(dic, AnimationTime);
            }
            else
            {
                SelectRectX = rect.X;
                SelectRectY = rect.Y;
                SelectRectW = rect.Width;
                SelectRectH = rect.Height;
            }

            SelectIndex = index;
            if (SelectChanged != null)
            {
                SelectChanged(this, new SelectChangedArgs(index));
            }

            if (BindTabControl != null && !BindTabControl.IsDisposed)
            {
                BindTabControl.SelectedIndex = SelectIndex;
            }
        }

        [HDescriptionLanguage("选项改变事件")]
        public event EventHandler<SelectChangedArgs> SelectChanged;
    }

    public class SelectChangedArgs : EventArgs
    {
        public SelectChangedArgs(int index)
        {
            SelectIndex = index;
        }

        public int SelectIndex;
    }
}
