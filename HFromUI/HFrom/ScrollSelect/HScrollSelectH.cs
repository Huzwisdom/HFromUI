using HFromUI.HAttribute;
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

namespace HFromUI.HFrom.ScrollSelect
{
    public partial class HScrollSelectH : UserControl
    {
        /// <summary>selection 字段。</summary>
        private string[] selection=new string[] { };

        /// <summary>selectIndex 字段。</summary>
        private int selectIndex = -1;

        private HAnimation anime;



        [HCategoryLanguage("通用"), HDisplayNameLanguage("选项"), HDescriptionLanguage("选项"), Browsable(true)]
        public string[] Selections 
        {
            get { return selection; }
            set
            {
                selection = value;
                panel_Select.Height = this.Height;
                panel_Select.Left = 0;
                if (selection==null||selection.Length==0)
                {
                    panel_Select.Width = this.Width;
                    SelectIndex = -1;
                }
                else
                {
                    panel_Select.Width =selection.Length*this.Width;
                    SelectIndex = 0;
                }
                
            }
        }
        /// <summary>SelectText 成员。</summary>
        /// <summary>SelectText 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中文本"), HDescriptionLanguage("选中文本"), Browsable(true)]
        public string SelectText { 
            get
            {
                if(selection.Length >SelectIndex&&SelectIndex>=0)
                {
                    return selection[SelectIndex];
                }
                else
                {
                    return string.Empty;
                }
            } 
        }
        [HCategoryLanguage("通用"), HDisplayNameLanguage("选中序号"), HDescriptionLanguage("选中序号"), Browsable(true)]
        public int SelectIndex
        {
            get { return selectIndex; }
            set { 
                if(selection.Length>0&&value<selection.Length)
                {
                    selectIndex = value;
                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Left", -1 * this.Width * selectIndex);
                        anime.AnimationControl(dic, AnimationTime);
                    }
                    else
                    {
                        panel_Select.Top = -1 * this.Width * selectIndex;
                    }
                }
                

                
            }
        }
        /// <summary>TextDrawMode 成员。</summary>
        /// <summary>TextDrawMode 字段。</summary>
        [HCategoryLanguage("通用"), HDisplayNameLanguage("文本绘制方式"), HDescriptionLanguage("文本绘制方式"), Browsable(true)]
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Clear;

        /// <summary>UseAnime 成员。</summary>
        /// <summary>UseAnime 字段。</summary>
        [HCategoryLanguage("动画"), HDisplayNameLanguage("使用动画"), HDescriptionLanguage("使用动画"), Browsable(true)]
        public bool UseAnime { get; set; } = true;

        [HCategoryLanguage("动画"), HDisplayNameLanguage("动画类型"), HDescriptionLanguage("动画类型"), Browsable(true)]
        public AnimationType AnimationType
        {
            get { return anime.AnimationType; }
            set
            {
                anime.AnimationType = value;
            }
        }

        /// <summary>AnimationTime 成员。</summary>
        /// <summary>AnimationTime 字段。</summary>
        [HCategoryLanguage("动画"), HDisplayNameLanguage("动画运行时间"), HDescriptionLanguage("动画运行时间"), Browsable(true)]
        public int AnimationTime { get;set; } = 100;

        [HDescriptionLanguage("选中项改变事件")]
        public event EventHandler SelectionChanged;

        public HScrollSelectH()
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
            this.UpdateStyles();
            anime = new HAnimation(panel_Select);
            anime.AnimationRunType = AnimationRunType.Interrupt;
            panel_Select.MouseWheel += Panel_Select_MouseWheel;
        }


        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            panel_Select.Height=this.Height;
            if(selection.Length>0)
            {
                panel_Select.Width=this.Width*selection.Length;
                if (SelectIndex < selection.Length)
                {
                    panel_Select.Left = SelectIndex * -1 * this.Width;
                }
            }
            else
            {
                panel_Select.Width = this.Width;
                panel_Select.Left = 0;
            }
            panel_Select.Invalidate();

        }

        /// <summary>Panel_Select_MouseWheel 方法。</summary>
        private void Panel_Select_MouseWheel(object sender, MouseEventArgs e)
        {
            var x = panel_Select.Left;
            var s = SelectIndex;
            if (e.Delta < 0)//向上滚，数值增加
            {
                if (s < selection.Length-1)
                {
                    s += 1;
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Left", -1*this.Width * s);
                        anime.AnimationControl(dic, AnimationTime);
                    }
                    else
                    {
                        panel_Select.Top = -1 * this.Width * s;
                    }

                }
            }
            else
            {
                if (s > 0)
                {
                    s -= 1;
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Left", -1 * this.Width * s);
                        anime.AnimationControl(dic, AnimationTime);
                    }
                    else
                    {
                        panel_Select.Left = -1 * this.Width * s;
                    }
                }
            }
            SelectIndex = s;

        }

        /// <summary>panel_Select_Paint 方法。</summary>
        private void panel_Select_Paint(object sender, PaintEventArgs e)
        {
            if(Selections.Length >0)
            {
                Graphics g = e.Graphics;
                g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                SolidBrush solidBrush = new SolidBrush(this.ForeColor);
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                for (int i = 0; i < Selections.Length; i++)
                {
                    var rect = new Rectangle(i * this.Width, 0,this.Width, this.Height);
                    g.DrawString(Selections[i], this.Font, solidBrush, rect, stringFormat);
                }
                solidBrush.Dispose();
                stringFormat.Dispose();
            }

        }
    }
}