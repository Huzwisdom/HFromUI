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

namespace HFromUI.HFrom.Date
{
    public partial class HTimePicker : UserControl
    {
        /// <summary>time 字段。</summary>
        private DateTime time=new DateTime();
        /// <summary>Time 成员。</summary>
        public DateTime Time { 
            get { 
                return time; 
            }
            set { 
                time = value;
                panel_secSelect.Top = -30 * time.Second;
                panel_minSelect.Top = -30 * time.Minute;
                panel_hourSelect.Top = -30 * time.Hour;
            } 
        }

        /// <summary>UseAnime 成员。</summary>
        public bool UseAnime { get; set; }=true;

        /// <summary>TextDrawMode 成员。</summary>
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Clear;

        private HAnimation animH;
        private HAnimation animM;
        private HAnimation animS;
        public HTimePicker()
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
            animH = new HAnimation(panel_hourSelect);
            animM = new HAnimation(panel_minSelect);
            animS = new HAnimation(panel_secSelect);
            animH.AnimationRunType = AnimationRunType.Interrupt;
            animM.AnimationRunType = AnimationRunType.Interrupt;
            animS.AnimationRunType = AnimationRunType.Interrupt;
            panel_hourSelect.MouseWheel += Panel_hourSelect_MouseWheel;
            panel_minSelect.MouseWheel += Panel_minSelect_MouseWheel;
            panel_secSelect.MouseWheel += Panel_secSelect_MouseWheel;
        }

        /// <summary>Panel_secSelect_MouseWheel 方法。</summary>
        private void Panel_secSelect_MouseWheel(object sender, MouseEventArgs e)
        {
            var y = panel_secSelect.Location.Y;
            var h = time.Second;
            if (e.Delta < 0)//向上滚，数值增加
            {
                if (h<59)
                {
                    h += 1;
                    if(UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Top", -30 * h);
                        animS.AnimationControl(dic, 200);
                    }
                    else
                    {
                        panel_secSelect.Top = -30 * h;
                    }
                    
                }
            }
            else
            {
                if (h>0)
                {
                    h-=1;
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Top", -30 * h);
                        animS.AnimationControl(dic, 200);
                    }
                    else
                    {
                        panel_secSelect.Top = -30 * h;
                    }
                }
            }
            
            time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, h);
         }

        /// <summary>Panel_minSelect_MouseWheel 方法。</summary>
        private void Panel_minSelect_MouseWheel(object sender, MouseEventArgs e)
        {
            var y = panel_minSelect.Location.Y;
            var h = time.Minute;
            if (e.Delta < 0)//向上滚，数值增加
            {
                if (h<59)
                {
                    h += 1;
                    
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Top", -30 * h);
                        animM.AnimationControl(dic, 200);
                    }
                    else
                    {
                        panel_minSelect.Top = -30 * h;
                    }
                }
            }
            else
            {
                if (h>0)
                {
                    h -= 1;
                    
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Top", -30 * h);
                        animM.AnimationControl(dic, 200);
                    }
                    else
                    {
                        panel_minSelect.Top = -30 * h;
                    }
                }
            }

            time = new DateTime(time.Year, time.Month, time.Day, time.Hour, h, time.Second);
        }

        /// <summary>Panel_hourSelect_MouseWheel 方法。</summary>
        private void Panel_hourSelect_MouseWheel(object sender, MouseEventArgs e)
        {
            var y = panel_hourSelect.Location.Y;
            var h = time.Hour;
            if (e.Delta < 0)//向上滚，数值增加
            {
                if (h<23)
                {
                    h += 1;
                    
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Top", -30 * h);
                        animH.AnimationControl(dic, 200);
                    }
                    else
                    {
                        panel_hourSelect.Top = -30 * h;
                    }
                }
            }
            else
            {
                if (h>0)
                {
                    h -= 1;
                    
                    if (UseAnime)
                    {
                        var dic = new Dictionary<string, float>();
                        dic.Add("Top", -30 * h);
                        animH.AnimationControl(dic, 200);
                    }
                    else
                    {
                        panel_hourSelect.Top = -30 * h;
                    }
                }
            }

            time = new DateTime(time.Year, time.Month, time.Day, h, time.Minute, time.Second);
        }

        /// <summary>panel_hourSelect_Paint 方法。</summary>
        private void panel_hourSelect_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            SolidBrush solidBrush = new SolidBrush(this.ForeColor);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            for (int i=0;i<24;i++)
            {
                var rect = new Rectangle(0, i * 30, 25, 30);
                g.DrawString(i.ToString().PadLeft(2, '0'), this.Font, solidBrush, rect,stringFormat);
            }
            solidBrush.Dispose();
            stringFormat.Dispose();

            
        }

        /// <summary>panel_minSelect_Paint 方法。</summary>
        private void panel_minSelect_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            SolidBrush solidBrush = new SolidBrush(this.ForeColor);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            for (int i = 0; i < 60; i++)
            {
                var rect = new Rectangle(0, i * 30, 25, 30);
                g.DrawString(i.ToString().PadLeft(2, '0'), this.Font, solidBrush, rect, stringFormat);
            }
            solidBrush.Dispose();
            stringFormat.Dispose();
        }

        /// <summary>panel_secSelect_Paint 方法。</summary>
        private void panel_secSelect_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextDrawMode == HEnum.HDrawMode.Anti ? System.Drawing.Text.TextRenderingHint.AntiAlias : System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            SolidBrush solidBrush = new SolidBrush(this.ForeColor);
            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            for (int i = 0; i < 60; i++)
            {
                var rect = new Rectangle(0, i * 30, 25, 30);
                g.DrawString(i.ToString().PadLeft(2, '0'), this.Font, solidBrush, rect, stringFormat);
            }
            solidBrush.Dispose();
            stringFormat.Dispose();
        }
    }
}
