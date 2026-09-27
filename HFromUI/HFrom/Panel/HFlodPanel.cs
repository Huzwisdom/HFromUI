using HFromUI.HAttribute;
using HFromUI.HControl;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HFromUI.HFrom.Panel
{
    using Panel = System.Windows.Forms.Panel;
    using HFromUI.HControl.Tools.Button;
    public partial class HFlodPanel : Panel
    {
        private HAnimation animation;
        /// <summary>_button 字段。</summary>
        private HPushButton _button = new HPushButton();
        /// <summary>titleBaseColor 字段。</summary>
        private Color titleBaseColor = Color.LightGray;
        /// <summary>arrowSize 字段。</summary>
        private Size arrowSize = new Size(13, 13);
        /// <summary>_text 字段。</summary>
        private string _text = "PPFoldPanel";
        /// <summary>BorderWidth 字段。</summary>
        private int BorderWidth { get; set; } = 1;
        /// <summary>TitleHeight 成员。</summary>
        /// <summary>TitleHeight 字段。</summary>
        [HDescriptionLanguage("标题高度")]
        public int TitleHeight { get; set; } = 30;
        /// <summary>Radius 成员。</summary>
        /// <summary>Radius 字段。</summary>
        [HDescriptionLanguage("圆角半径")]
        public int Radius { get; set; } = 5;
        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [HDescriptionLanguage("边框颜色")]
        public Color BorderColor { get; set; } = Color.DodgerBlue;
        /// <summary>TitleBorderColor 成员。</summary>
        /// <summary>TitleBorderColor 字段。</summary>
        [HDescriptionLanguage("标题边框颜色")]
        public Color TitleBorderColor { get; set; } =Color.DodgerBlue;
        [HDescriptionLanguage("边框背景色")]
        public Color TitleBaseColor 
        {
            get { return titleBaseColor; }
            set
            {
                titleBaseColor = value;
                // 箭头按钮三态底色与标题栏一致，视觉上无独立按钮底
                _button.NormalBackColor = TitleBaseColor;
                _button.HoverBackColor = TitleBaseColor;
                _button.PressedBackColor = TitleBaseColor;
                _button.NormalBorderColor = TitleBaseColor;
                _button.HoverBorderColor = TitleBaseColor;
                _button.PressedBorderColor = TitleBaseColor;
            }
        }
        /// <summary>箭头常规色（矢量绘制，不用资源图片）。</summary>
        [HDescriptionLanguage("箭头颜色")]
        public Color ArrowColor
        {
            get { return _button.NormalForeColor; }
            set { _button.NormalForeColor = value; _button.PressedForeColor = value; }
        }
        /// <summary>箭头悬停色（矢量绘制，不用资源图片）。</summary>
        [HDescriptionLanguage("箭头Hover颜色")]
        public Color ArrowHoverColor
        {
            get { return _button.HoverForeColor; }
            set { _button.HoverForeColor = value; }
        }
        /// <summary>FlodHeight 成员。</summary>
        /// <summary>FlodHeight 字段。</summary>
        [HDescriptionLanguage("控件折叠高度")]
        public int FlodHeight { get; set; } = 100;
        /// <summary>IsFold 成员。</summary>
        /// <summary>IsFold 字段。</summary>
        [HDescriptionLanguage("是否折叠")]
        public bool IsFold {
            get { return this.Height<=TitleHeight; }
            set
            {
                if (!value)
                {
                    if(UseAnime)
                    {
                        Dictionary<string, float> dic = new Dictionary<string, float>();
                        dic.Add("Height", FlodHeight);
                        animation.AnimationControl(dic, AnimeTime);
                    }
                    else
                    {
                        this.Height = FlodHeight;
                    }
                    
                }
                else
                {
                    if(UseAnime)
                    {
                        Dictionary<string, float> dic = new Dictionary<string, float>();
                        dic.Add("Height", TitleHeight);
                        animation.AnimationControl(dic, AnimeTime);
                    }
                    else
                    {
                        this.Height = TitleHeight;
                    }
                    
                }
                    
            }
        }
        public new int Height
        {
            get { return base.Height; }
            set { 
                if(value<TitleHeight)
                    base.Height = TitleHeight;
                else
                    base.Height = value; 
            }
        }
        [Browsable(true)]
        public new string Text 
        {
            get { return _text; }
            set
            {
                _text = value;
            }
        }
        [HDescriptionLanguage("是否显示箭头图标")]
        public bool ShowArrow
        {
            get { return _button.Visible; }
            set
            {
                this._button.Visible = value;
            }
        }
        [HDescriptionLanguage("箭头图标大小")]
        public Size ArrowSize
        {
            get { return arrowSize; }
            set
            {
                arrowSize = value;
                _button.ImageSize = Math.Min(value.Width, value.Height);
                _button.Size = value;
                _button.Location = new Point(this.Width - BorderWidth - Radius - _button.Width + ArrowOffset.X, (TitleHeight - _button.Height) / 2 + ArrowOffset.Y);
            }
        }
        /// <summary>ArrowOffset 成员。</summary>
        /// <summary>ArrowOffset 字段。</summary>
        [HDescriptionLanguage("箭头图标位置偏移")]
        public Point ArrowOffset { get; set; }=new Point(0,0);
        /// <summary>UseAnime 成员。</summary>
        /// <summary>UseAnime 字段。</summary>
        [HDescriptionLanguage("使用动画")]
        public bool UseAnime { get; set; }=true;
        /// <summary>AnimeTime 成员。</summary>
        /// <summary>AnimeTime 字段。</summary>
        [HDescriptionLanguage("动画时间")]
        public int AnimeTime { get; set; } = 100;
        public HFlodPanel()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            _button.Text = "";
            _button.ButtonStyle = HPushButtonStyle.Custom;
            _button.Shape = HPushButtonShape.Rect;
            _button.Radius = 0;
            _button.BorderWidth = 0;
            _button.Size = arrowSize;
            _button.ImageSize = Math.Min(arrowSize.Width, arrowSize.Height);
            _button.Cursor = Cursors.Hand;
            // 矢量箭头：默认深灰，悬停蓝色
            _button.NormalForeColor = Color.FromArgb(64, 64, 64);
            _button.PressedForeColor = Color.FromArgb(64, 64, 64);
            _button.HoverForeColor = Color.DodgerBlue;
            _button.Glyph = HPushButtonGlyph.ArrowDown;
            TitleBaseColor = titleBaseColor;
            _button.Click += _button_Click;
            this.Controls.Add(_button);
            _button.Location = new Point(this.Width - BorderWidth - Radius - _button.Width + ArrowOffset.X, (TitleHeight - _button.Height) / 2 + ArrowOffset.Y);
            animation = new HAnimation(this);
            animation.AnimationType = AnimationType.EaseOut;
            this.ControlAdded += PPFlodPanel_ControlAdded;
            this.ControlRemoved += PPFlodPanel_ControlRemoved;
        }
        /// <summary>_button_Click 方法。</summary>
        private void _button_Click(object sender, EventArgs e)
        {
            this.IsFold=!this.IsFold;
        }
        /// <summary>PPFlodPanel_ControlRemoved 方法。</summary>
        private void PPFlodPanel_ControlRemoved(object sender, ControlEventArgs e)
        {
            e.Control.LocationChanged -= Control_LocationChanged;
        }
        /// <summary>PPFlodPanel_ControlAdded 方法。</summary>
        private void PPFlodPanel_ControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.LocationChanged += Control_LocationChanged;
        }
        /// <summary>Control_LocationChanged 方法。</summary>
        private void Control_LocationChanged(object sender, EventArgs e)
        {
            var c = sender as Control;
            if (c.Top < TitleHeight)
                c.Top = TitleHeight;
            //if(c.Left<=BorderWidth)
            //    c.Left = BorderWidth+1;
            //if(c.Right>this.Width-BorderWidth)
            //    c.Left=this.Width-BorderWidth-c.Width-1;
            //if(c.Bottom>this.Height-BorderWidth)
            //    c.Top=this.Height-BorderWidth-c.Height-1;
        }
        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if(this.Height<this.TitleHeight)
                this.Height = this.TitleHeight;
            _button.Location=new Point(this.Width- BorderWidth - Radius-_button.Width+ArrowOffset.X,(TitleHeight-_button.Height)/2+ArrowOffset.Y);
            // 折叠态显示下箭头（点击展开），展开态显示上箭头，全部矢量绘制
            _button.Glyph = IsFold ? HPushButtonGlyph.ArrowDown : HPushButtonGlyph.ArrowUp;
        }
        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g=e.Graphics;
            g.Clear(BackColor);
            HDrawPaint.SetGraphicsHighQuality(g);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (!IsFold)
            {
                var Borderpath = HDrawPaint.CreatePath(new Rectangle(0, TitleHeight - 1, base.Width, base.Height - TitleHeight+1), Radius, HEnum.HRoundStyle.Bottom);
                using (var pen = new Pen(BorderColor, 1f))
                {
                    g.DrawPath(pen, Borderpath);
                }
            }
            var TitlePath= HDrawPaint.CreatePath(new Rectangle(0,0,base.Width,TitleHeight), Radius, IsFold?HEnum.HRoundStyle.All:HEnum.HRoundStyle.Top);
            using (var brush=new SolidBrush(TitleBaseColor))
            {
                g.FillPath(brush, TitlePath);
            }
            using (var pen=new Pen(TitleBorderColor,1f))
            {
                g.DrawPath(pen, TitlePath);
            }
            using (var fontBrush=new SolidBrush(ForeColor))
            {
                g.DrawString(this.Text, this.Font, fontBrush, new Rectangle(0, 0, this.Width, TitleHeight),new StringFormat() { LineAlignment=StringAlignment.Center,Alignment=StringAlignment.Center});
            }
                
        }
        /// <summary>响应 Click 事件。</summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.IsFold = !this.IsFold;
        }
    }
}
