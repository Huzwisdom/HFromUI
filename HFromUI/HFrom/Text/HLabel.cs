using HFromUI.HAttribute;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace HFromUI.HFrom.Text
{
    public partial class HLabel : Label
    {
        /// <summary>TextDrawMode 成员。</summary>
        /// <summary>TextDrawMode 字段。</summary>
        [HDescriptionLanguage("字符绘制方式")]
        public HEnum.HDrawMode TextDrawMode { get; set; } = HEnum.HDrawMode.Anti;

        public HLabel()
        {
            InitializeComponent();
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(this.BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            switch(TextDrawMode)
            {
                case HEnum.HDrawMode.Anti: g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;break;
                case HEnum.HDrawMode.Clear:g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;break;
                case HEnum.HDrawMode.Default:g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;break;
            }

            using (var brush=new SolidBrush(this.ForeColor))
            {
                if (this.AutoSize)
                {
                    g.DrawString(this.Text, this.Font, brush, new Point(0, 0));
                }
                else
                {
                    System.Drawing.StringFormat format = new System.Drawing.StringFormat();
                    format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.EllipsisCharacter;

                    switch (this.TextAlign)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.BottomLeft:
                            format.Alignment = StringAlignment.Near;
                            break;

                        case ContentAlignment.TopRight:
                        case ContentAlignment.MiddleRight:
                        case ContentAlignment.BottomRight:
                            format.Alignment = StringAlignment.Far;
                            break;

                        case ContentAlignment.TopCenter:
                        case ContentAlignment.MiddleCenter:
                        case ContentAlignment.BottomCenter:
                            format.Alignment = StringAlignment.Center;
                            break;
                    }

                    switch (this.TextAlign)
                    {
                        case ContentAlignment.TopLeft:
                        case ContentAlignment.TopCenter:
                        case ContentAlignment.TopRight:
                            format.LineAlignment = StringAlignment.Near;
                            break;

                        case ContentAlignment.MiddleLeft:
                        case ContentAlignment.MiddleRight:
                        case ContentAlignment.MiddleCenter:
                            format.LineAlignment = StringAlignment.Center;
                            break;

                        case ContentAlignment.BottomLeft:
                        case ContentAlignment.BottomRight:
                        case ContentAlignment.BottomCenter:
                            format.LineAlignment = StringAlignment.Far;
                            break;
                    }
                    g.DrawString(this.Text, this.Font, brush, new Rectangle(0, 0, this.Width, this.Height), format);
                }
            }
                
        }
    }
}
