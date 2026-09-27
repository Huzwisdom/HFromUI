using HFromUI.HAttribute;
using HFromUI.HEnum;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HFromUI.HControl.Tools.Tips;
using HFromUI.HControl.Base;

namespace HFromUI.HControl.Tools.Bars
{
    [DefaultEvent("ValueChanged")]
    public class HTrackBar : HBarBase
    {
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("手动操作值改变事件"), HDescriptionLanguage("手动操作值改变事件"), Browsable(true)]
        public event EventHandler ManualValueChanged;

        /// <summary>dcimalDigits 字段。</summary>
        private int dcimalDigits = 0;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("值小数精确位数"), HDescriptionLanguage("值小数精确位数"), Browsable(true)]
        public int DcimalDigits
        {
            get { return dcimalDigits; }
            set { dcimalDigits = value; }
        }

        /// <summary>线宽。</summary>
        private int lineWidth = 10;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("线宽度"), HDescriptionLanguage("线宽度"), Browsable(true)]
        public int LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        /// <summary>minValue 字段。</summary>
        private long minValue = 0;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("最小值"), HDescriptionLanguage("最小值"), Browsable(true)]
        public new long MinValue
        {
            get { return minValue; }
            set
            {
                if (minValue > m_value)
                    return;
                minValue = value;
                SyncBaseState(minValue, maxValue, m_value);
                this.Refresh();
            }
        }

        /// <summary>maxValue 字段。</summary>
        private long maxValue = 100;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("最大值"), HDescriptionLanguage("最大值"), Browsable(true)]
        public new long MaxValue
        {
            get { return maxValue; }
            set
            {
                if (value < m_value)
                    return;
                maxValue = value;
                SyncBaseState(minValue, maxValue, m_value);
                this.Refresh();
            }
        }

        /// <summary>m_value 字段。</summary>
        private long m_value = 0;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("值"), HDescriptionLanguage("值"), Browsable(true)]
        public new long Value
        {
            get { return this.m_value; }
            set
            {
                if (value > maxValue || value < minValue)
                    return;
                //var v = (int)Math.Round((double)value, dcimalDigits);
                //if (m_value == value)
                //return;
                this.m_value = value;
                SyncBaseState(minValue, maxValue, m_value);
                this.Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        /// <summary>m_lineColor 字段。</summary>
        private Color m_lineColor = Color.FromArgb(228, 231, 237);

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("线颜色"), HDescriptionLanguage("线颜色"), Browsable(true)]
        public Color LineColor
        {
            get { return m_lineColor; }
            set
            {
                m_lineColor = value;
                this.Refresh();
            }
        }

        /// <summary>m_valueColor 字段。</summary>
        private Color m_valueColor = Color.DodgerBlue;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("值颜色"), HDescriptionLanguage("值颜色"), Browsable(true)]
        public Color ValueColor
        {
            get { return m_valueColor; }
            set
            {
                m_valueColor = value;
                this.Refresh();
            }
        }

        /// <summary>ellipseColor 字段。</summary>
        private Color ellipseColor = Color.White;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆按钮颜色"), HDescriptionLanguage("圆按钮颜色"), Browsable(true)]
        public Color EllipseColor
        {
            get { return ellipseColor; }
            set
            {
                ellipseColor = value;
                this.Refresh();
            }
        }

        /// <summary>EllipseBorderColor 成员。</summary>
        /// <summary>EllipseBorderColor 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("圆按钮边框颜色"), HDescriptionLanguage("圆按钮边框颜色"), Browsable(true)]
        public Color EllipseBorderColor { get;set; }=Color.White;

        /// <summary>isShowTips 字段。</summary>
        private bool isShowTips = true;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示提示"), HDescriptionLanguage("点击滑动时是否显示数值提示"), Browsable(true)]
        public bool IsShowTips
        {
            get { return isShowTips; }
            set { isShowTips = value; }
        }

        /// <summary>TipsFormat 成员。</summary>
        /// <summary>TipsFormat 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("提示格式"), HDescriptionLanguage("显示数值提示的格式化形式"), Browsable(true)]
        public string TipsFormat { get; set; }

        /// <summary>ValueIsSecond 成员。</summary>
        /// <summary>ValueIsSecond 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("数值是秒"), HDescriptionLanguage("数值是秒"), Browsable(true)]
        public bool ValueIsSecond { get; set; }

        /// <summary>radioButtonSize 字段。</summary>
        private int radioButtonSize = 10;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("按钮大小"), HDescriptionLanguage("按钮大小"), Browsable(true)]
        public int RadioButtonSize
        {
            get { return radioButtonSize; }
            set
            {
                if (value < lineWidth)
                    return;
                radioButtonSize = value;
                this.Invalidate();
            }
        }

        /// <summary>ShowButton 成员。</summary>
        /// <summary>ShowButton 字段。</summary>
        [HCategoryLanguage("自定义"), HDisplayNameLanguage("显示按钮"), HDescriptionLanguage("显示按钮"), Browsable(true)]
        public bool ShowButton { get; set; } = true;

        /// <summary>corners 字段。</summary>
        private bool corners = true;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("是否启用圆角"), HDescriptionLanguage("是否启用圆角"), Browsable(true)]
        public bool Corners
        {
            get { return corners; }
            set
            {
                corners = value;
                this.Invalidate();
            }
        }

        /// <summary>tipBackColor 字段。</summary>
        private Color tipBackColor = Color.DodgerBlue;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("提示气泡背景色"), HDescriptionLanguage("Tip气泡的背景色"), Browsable(true)]
        public Color TipBackColor
        {
            get { return tipBackColor; }
            set
            {
                tipBackColor = value;
            }
        }

        /// <summary>tipForeColor 字段。</summary>
        private Color tipForeColor = Color.White;

        [HCategoryLanguage("自定义"), HDisplayNameLanguage("提示气泡文本色"), HDescriptionLanguage("Tip气泡的文本色"), Browsable(true)]
        public Color TipForeColor
        {
            get { return tipForeColor; }
            set
            {
                tipForeColor = value;
            }
        }

        private RectangleF m_lineRectangle;

        private RectangleF m_trackRectangle;

        public HTrackBar()
        {
            SyncBaseState(minValue, maxValue, m_value);
            this.Size = new Size(250, 30);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        /// <summary>blnDown 字段。</summary>
        private bool blnDown = false;

        /// <summary>响应 MouseDown 事件。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (m_lineRectangle.Contains(e.Location) || m_trackRectangle.Contains(e.Location))
            {
                blnDown = true;
                Value = minValue + ((e.Location.X - radioButtonSize / 2) * (maxValue - minValue)) / (this.Width - radioButtonSize);
                if (ManualValueChanged != null)
                {
                    ManualValueChanged(this, null);
                }
                ShowTips();
            }
        }

        /// <summary>响应 MouseMove 事件。</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (blnDown)
            {
                Value = minValue + ((e.Location.X - radioButtonSize / 2) * (maxValue - minValue)) / (this.Width - radioButtonSize);
                if (ManualValueChanged != null)
                {
                    ManualValueChanged(this, null);
                }
                ShowTips();
            }
            if (m_lineRectangle.Contains(e.Location))
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>响应 MouseUp 事件。</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            blnDown = false;

            if (frmTips != null && !frmTips.IsDisposed)
            {
                frmTips.Close();
                frmTips = null;
            }
        }

        /// <summary>frmTips 字段。</summary>
        private HAnchorTips frmTips = null;

        /// <summary>ShowTips 方法。</summary>
        private void ShowTips()
        {
            if (isShowTips)
            {
                string strValue = Value.ToString();
                if (!ValueIsSecond)//值不是秒
                {
                    if (!string.IsNullOrEmpty(TipsFormat))
                    {
                        try
                        {
                            strValue = Value.ToString(TipsFormat);
                        }
                        catch { }
                    }
                }
                else
                {
                    System.DateTime dt = new System.DateTime().AddSeconds(Value);
                    TimeSpan ts = dt - new System.DateTime();
                    strValue = ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                }

                var p = this.PointToScreen(new Point((int)m_trackRectangle.X - 1, (int)m_trackRectangle.Y));

                if (frmTips == null || frmTips.IsDisposed || !frmTips.Visible)
                {
                    frmTips = HAnchorTips.ShowTips(new Rectangle(p.X, p.Y, (int)m_trackRectangle.Width, (int)m_trackRectangle.Height), strValue, this.Font.Name, (int)this.Font.Size, AnchorTipsLocation.TOP, tipBackColor, tipForeColor, autoCloseTime: -1);
                }
                else
                {
                    frmTips.RectControl = new Rectangle(p.X, p.Y, (int)m_trackRectangle.Width, (int)m_trackRectangle.Height);
                    frmTips.StrMsg = strValue;
                }
            }
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            Brush baseBrush = new SolidBrush(m_lineColor);
            Brush valueBrush = new SolidBrush(m_valueColor);
            Brush backBrush = new SolidBrush(this.BackColor);

            m_lineRectangle = new RectangleF(radioButtonSize / 2, (float)(this.Size.Height - lineWidth) / 2, this.Size.Width - radioButtonSize, lineWidth);
            GraphicsPath pathLine = HDrawPaint.CreatePath(m_lineRectangle, corners ? lineWidth-2 : 0);// new GraphicsPath();
            GraphicsPath valueLine = HDrawPaint.CreatePath(new RectangleF(radioButtonSize / 2, (float)(this.Size.Height - lineWidth) / 2, (int)((m_value - minValue) * m_lineRectangle.Width / (maxValue - minValue)), lineWidth), corners ? lineWidth-2 : 0);// new GraphicsPath();

            g.FillPath(baseBrush, pathLine);
            g.FillPath(valueBrush, valueLine);

            pathLine.AddRectangle(new Rectangle(-1, -1, this.Width + 1, this.Height + 1));
            g.FillPath(backBrush, pathLine);

            m_trackRectangle = new RectangleF(((float)(m_value - minValue) / (float)(maxValue - minValue)) * (this.Size.Width - radioButtonSize - 1), (float)(this.Size.Height - radioButtonSize) / 2, radioButtonSize, radioButtonSize);
            if (ShowButton)
            {
                g.FillEllipse(new SolidBrush(ellipseColor), m_trackRectangle);

                Pen pen = new Pen(EllipseBorderColor, 1f);
                g.DrawEllipse(pen, m_trackRectangle);
                pen.Dispose();
            }


            
            baseBrush.Dispose();
            valueBrush.Dispose();
            baseBrush.Dispose();
            pathLine.Dispose();
            valueLine.Dispose();
        }

        /// <summary>InitializeComponent 方法。</summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // PPTrackBar
            this.Name = "PPTrackBar";
            this.Size = new System.Drawing.Size(276, 29);
            this.ResumeLayout(false);
        }
    }
}