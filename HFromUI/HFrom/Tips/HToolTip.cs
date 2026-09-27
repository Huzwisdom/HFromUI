using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFromUI.HFrom.Tips
{
    public partial class HToolTip : ToolTip
    {

        /// <summary>iconSize 字段。</summary>
        private Size iconSize = new Size(16, 16);

        /// <summary>_icon 字段。</summary>
        private Image _icon=null;


        /// <summary>TextDrawMode 成员。</summary>
        /// <summary>TextDrawMode 字段。</summary>
        [HDescriptionLanguage("文本绘制模式")]
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Clear;

        /// <summary>BaseColorStart 成员。</summary>
        /// <summary>BaseColorStart 字段。</summary>
        [HDescriptionLanguage("渐变背景色1")]
        public Color BaseColorStart { get; set; } = Color.DeepSkyBlue;

        /// <summary>BaseColorEnd 成员。</summary>
        /// <summary>BaseColorEnd 字段。</summary>
        [HDescriptionLanguage("渐变背景色2")]
        public Color BaseColorEnd { get; set; } = Color.DodgerBlue;

        /// <summary>BorderColor 成员。</summary>
        /// <summary>BorderColor 字段。</summary>
        [HDescriptionLanguage("边框色")]
        public Color BorderColor { get; set; } = Color.DodgerBlue;

        /// <summary>ShowBorder 成员。</summary>
        /// <summary>ShowBorder 字段。</summary>
        [HDescriptionLanguage("是否显示边框")]
        public bool ShowBorder { get; set; } = true;

        [HDescriptionLanguage("图标大小")]
        public Size IconSize
        {
            get { return iconSize; }
            set { 
                if(iconSize!=value)
                {
                    iconSize = value;
                }
                if(iconSize.Width>32)
                    iconSize.Width = 32;
                if(iconSize.Height>32)
                    iconSize.Height = 32;
            }
        }

        [HDescriptionLanguage("图标")]
        public Image Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                if (_icon != null)
                    base.ToolTipIcon = ToolTipIcon.None;
            }
        }

        /// <summary>字体。</summary>
        /// <summary>字体。</summary>
        [HDescriptionLanguage("字体")]
        public  Font Font { get; set; }=new Font("微软雅黑", 9f);

        /// <summary>TitleFont 成员。</summary>
        /// <summary>TitleFont 字段。</summary>
        [HDescriptionLanguage("标题字体")]
        public Font TitleFont { get; set; } = new Font("微软雅黑", 9f, FontStyle.Bold);

        /// <summary>TitleForeColor 成员。</summary>
        /// <summary>TitleForeColor 字段。</summary>
        [HDescriptionLanguage("标题字体颜色")]
        public Color TitleForeColor { get; set; } = Color.Black;

        public HToolTip()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            Init();
        }

        public HToolTip(IContainer container):base(container)
        {
            InitializeComponent();
            Init();
        }

        /// <summary>初始化。</summary>
        private void Init()
        {
            this.OwnerDraw = true;
            this.ReshowDelay = 800;
            this.InitialDelay = 500;
            base.Draw += PPToolTip_Draw;
            base.Popup += PPToolTip_Popup;
        }

        /// <summary>PPToolTip_Popup 方法。</summary>
        private void PPToolTip_Popup(object sender, PopupEventArgs e)
        {
            
        }

        /// <summary>PPToolTip_Draw 方法。</summary>
        private void PPToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            //g.Clear(this.BackColor);
            Rectangle bounds=e.Bounds;

            //画背景及边框
            using (var path = HDrawPaint.CreatePath(bounds, 2))
            {
                using (var basebrush = new LinearGradientBrush(bounds, BaseColorStart, BaseColorEnd, LinearGradientMode.Vertical))
                {
                    g.FillPath(basebrush, path);
                }

                if (ShowBorder)
                {
                    using (Pen borderPen = new Pen(BorderColor))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }

            //画图标
            Image showicon = null;
            if(this.ToolTipIcon!=ToolTipIcon.None)
            {
                var icon = GetSysIcon(); 
                if(icon!=null)
                {
                    showicon = icon.ToBitmap();
                }
            }
            else
            {
                if(_icon!=null)
                    showicon=_icon.Clone() as Image;
            }

            Rectangle imageRect=Rectangle.Empty;
            if (showicon != null)
            {
                imageRect = new Rectangle(2, 2, iconSize.Width, iconSize.Height);
                g.DrawImage(showicon, imageRect);
            }

            //画标题
            Rectangle titleRect=Rectangle.Empty;
            if(!string.IsNullOrEmpty(this.ToolTipTitle))
            {
                var titleSize = HDrawPaint.GetTextSize(this.ToolTipTitle, TitleFont);
                if(showicon!=null)
                {
                    titleRect = new Rectangle(iconSize.Width + 2 + 2, 2, bounds.Width - iconSize.Width - 2 - 2 - 2, (int)titleSize.Height);
                }
                else
                {
                    titleRect = new Rectangle(2,2,bounds.Width-4, (int)titleSize.Height);
                }
                using (var titlebrush=new SolidBrush(this.TitleForeColor))
                {
                    g.DrawString(this.ToolTipTitle, this.TitleFont, titlebrush, titleRect);
                }
            }


            //画文本
            Rectangle textRect=new Rectangle(2,2,bounds.Width-4,bounds.Height-4);
            if(showicon!=null)
            {
                textRect.X += iconSize.Width + 2;
                textRect.Width -= iconSize.Width+2;
            }
            if (!string.IsNullOrEmpty(this.ToolTipTitle))
            {
                textRect.Y+= titleRect.Height + 2;
                textRect.Height -= titleRect.Height+2;
            }


            using (SolidBrush textBrush = new SolidBrush(ForeColor))
            {
                g.DrawString(e.ToolTipText, e.Font, textBrush, textRect);
            }



        }

        /// <summary>
        /// 获取图标
        /// </summary>
        /// <returns></returns>
        private Icon GetSysIcon()
        {
            switch(ToolTipIcon)
            {
                case ToolTipIcon.None:return null;
                case ToolTipIcon.Error:return SystemIcons.Error;
                case ToolTipIcon.Warning:return SystemIcons.Warning;
                case ToolTipIcon.Info:return SystemIcons.Information;
            }

            return null;
        }
    }
}
