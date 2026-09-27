using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.LED
{
    public partial class HLedNum : UserControl
    {
        /// <summary>numColor 字段。</summary>
        private Color numColor = Color.DodgerBlue;
        /// <summary>numChar 字段。</summary>
        private char numChar = '-';
        /// <summary>spaceWidth 字段。</summary>
        private int spaceWidth = 4;

        [HCategoryLanguage("通用"), HDisplayNameLanguage("数量颜色"), HDescriptionLanguage("led文字的颜色"), Browsable(true)]
        public Color NumColor
        {
            get { return numColor; }
            set
            {
                numColor = value;
                this.Invalidate();
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("数量字符"), HDescriptionLanguage("led文字的字符"), Browsable(true)]
        public char NumChar
        {
            get { return numChar; }
            set
            {
                if (UseableCharList.Contains(value))
                {
                    numChar = value;
                    this.Invalidate();
                }
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("间距宽度"), HDescriptionLanguage("led文字的笔划间隔"), Browsable(true)]
        public int SpaceWidth
        {
            get { return spaceWidth; }
            set
            {
                spaceWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>UseableCharList 字段。</summary>
        private List<char> UseableCharList = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-', ':', '_', ' ' };

        private Dictionary<char, int[]> dic = new Dictionary<char, int[]>();

        public HLedNum()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            int[] list = new int[] { 1, 2, 3, 4, 5, 6 };
            dic.Add('0', list);
            list = new int[] { 2, 3 };
            dic.Add('1', list);
            list = new int[] { 1, 2, 4, 5, 7 };
            dic.Add('2', list);
            list = new int[] { 1, 2, 3, 4, 7 };
            dic.Add('3', list);
            list = new int[] { 2, 3, 6, 7 };
            dic.Add('4', list);
            list = new int[] { 1, 3, 4, 6, 7 };
            dic.Add('5', list);
            list = new int[] { 1, 3, 4, 5, 6, 7 };
            dic.Add('6', list);
            list = new int[] { 1, 2, 3 };
            dic.Add('7', list);
            list = new int[] { 1, 2, 3, 4, 5, 6, 7 };
            dic.Add('8', list);
            list = new int[] { 1, 2, 3, 4, 6, 7 };
            dic.Add('9', list);
            list = new int[] { 9 };
            dic.Add('.', list);
            list = new int[] { 7 };
            dic.Add('-', list);
            list = new int[] { 8, 9 };
            dic.Add(':', list);
            list = new int[] { 4 };
            dic.Add('_', list);
            list = new int[] { };
            dic.Add(' ', list);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Width = this.Height / 2;
        }

        /// <summary>响应 Paint 事件。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            int[] val;
            if (dic.TryGetValue(this.NumChar, out val))
            {
                if (val == null)
                    return;
                foreach (int num in val)
                {
                    PaintLed(g, num);
                }
            }
        }

        /// <summary>PaintLed 方法。</summary>
        private void PaintLed(Graphics g, int index)
        {
            Brush brush = new SolidBrush(NumColor);
            int spaceWidth = SpaceWidth;
            int ledWith = this.Width / 5;
            switch (index)
            {
                case 1:
                    Point[] points = new Point[6];
                    points[0] = new Point(spaceWidth + ledWith / 3, ledWith / 3);
                    points[1] = new Point(spaceWidth + 2 * ledWith / 3, 0);
                    points[2] = new Point(this.Width - spaceWidth - 2 * ledWith / 3, 0);
                    points[3] = new Point(this.Width - spaceWidth - ledWith / 3, ledWith / 3);
                    points[4] = new Point(this.Width - spaceWidth - ledWith, ledWith);
                    points[5] = new Point(spaceWidth + ledWith, ledWith);
                    g.FillPolygon(brush, points);
                    break;

                case 2:
                    points = new Point[6];
                    points[0] = new Point(this.Width - ledWith / 3, spaceWidth + ledWith / 3);
                    points[1] = new Point(this.Width, spaceWidth + 2 * ledWith / 3);
                    points[2] = new Point(this.Width, this.Height / 2 - spaceWidth - ledWith / 3);
                    points[3] = new Point(this.Width - ledWith / 3, this.Height / 2 - spaceWidth);
                    points[4] = new Point(this.Width - ledWith, this.Height / 2 - 2 * ledWith / 3 - spaceWidth);
                    points[5] = new Point(this.Width - ledWith, spaceWidth + ledWith);
                    g.FillPolygon(brush, points);
                    break;

                case 3:
                    points = new Point[6];
                    points[0] = new Point(this.Width - ledWith / 3, this.Height / 2 + spaceWidth);
                    points[1] = new Point(this.Width, this.Height / 2 + spaceWidth + ledWith / 3);
                    points[2] = new Point(this.Width, this.Height - spaceWidth - 2 * ledWith / 3);
                    points[3] = new Point(this.Width - ledWith / 3, this.Height - spaceWidth - ledWith / 3);
                    points[4] = new Point(this.Width - ledWith, this.Height - ledWith - spaceWidth);
                    points[5] = new Point(this.Width - ledWith, this.Height / 2 + spaceWidth + 2 * ledWith / 3);
                    g.FillPolygon(brush, points);
                    break;

                case 4:
                    points = new Point[6];
                    points[0] = new Point(spaceWidth + ledWith / 3, this.Height - ledWith / 3);
                    points[1] = new Point(spaceWidth + 2 * ledWith / 3, this.Height);
                    points[2] = new Point(this.Width - spaceWidth - 2 * ledWith / 3, this.Height);
                    points[3] = new Point(this.Width - spaceWidth - ledWith / 3, this.Height - ledWith / 3);
                    points[4] = new Point(this.Width - ledWith - spaceWidth, this.Height - ledWith);
                    points[5] = new Point(spaceWidth + ledWith, this.Height - ledWith);
                    g.FillPolygon(brush, points);
                    break;

                case 5:
                    points = new Point[6];
                    points[0] = new Point(ledWith / 3, this.Height / 2 + spaceWidth);
                    points[1] = new Point(0, this.Height / 2 + spaceWidth + ledWith / 3);
                    points[2] = new Point(0, this.Height - spaceWidth - 2 * ledWith / 3);
                    points[3] = new Point(ledWith / 3, this.Height - spaceWidth - ledWith / 3);
                    points[4] = new Point(ledWith, this.Height - ledWith - spaceWidth);
                    points[5] = new Point(ledWith, this.Height / 2 + spaceWidth + 2 * ledWith / 3);
                    g.FillPolygon(brush, points);
                    break;

                case 6:
                    points = new Point[6];
                    points[0] = new Point(ledWith / 3, spaceWidth + ledWith / 3);
                    points[1] = new Point(0, spaceWidth + 2 * ledWith / 3);
                    points[2] = new Point(0, this.Height / 2 - spaceWidth - ledWith / 3);
                    points[3] = new Point(ledWith / 3, this.Height / 2 - spaceWidth);
                    points[4] = new Point(ledWith, this.Height / 2 - 2 * ledWith / 3 - spaceWidth);
                    points[5] = new Point(ledWith, spaceWidth + ledWith);
                    g.FillPolygon(brush, points);
                    break;

                case 7:
                    points = new Point[6];
                    points[0] = new Point(spaceWidth + ledWith / 3, this.Height / 2);
                    points[1] = new Point(spaceWidth + ledWith, this.Height / 2 - 2 * ledWith / 3);
                    points[2] = new Point(this.Width - spaceWidth - ledWith, this.Height / 2 - 2 * ledWith / 3);
                    points[3] = new Point(this.Width - spaceWidth - ledWith / 3, this.Height / 2);
                    points[4] = new Point(this.Width - spaceWidth - ledWith, this.Height / 2 + 2 * ledWith / 3);
                    points[5] = new Point(spaceWidth + ledWith, this.Height / 2 + 2 * ledWith / 3);
                    g.FillPolygon(brush, points);
                    break;

                case 8:
                    g.FillEllipse(brush, new RectangleF((this.Width - ledWith) / 2, (this.Height / 2 - ledWith) / 2, ledWith, ledWith));
                    break;

                case 9:
                    g.FillEllipse(brush, new RectangleF((this.Width - ledWith) / 2, this.Height / 2 + (this.Height / 2 - ledWith) / 2, ledWith, ledWith));
                    break;
            }
            brush.Dispose();
        }
    }
}