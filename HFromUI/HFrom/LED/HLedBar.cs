using HFromUI.HAttribute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HFromUI.HFrom.LED
{
    public partial class HLedBar : FlowLayoutPanel
    {
        /// <summary>charNum 字段。</summary>
        private int charNum = 4;
        /// <summary>文本。</summary>
        private string text = "";
        /// <summary>spaceWidth 字段。</summary>
        private int spaceWidth = 4;
        /// <summary>numColor 字段。</summary>
        private Color numColor = Color.DodgerBlue;
        /// <summary>charInterval 字段。</summary>
        private int charInterval = 5;

        [HCategoryLanguage("通用"), HDisplayNameLanguage("字符数量"), HDescriptionLanguage("led文字的个数"), Browsable(true)]
        public int CharNum
        {
            get { return charNum; }
            set
            {
                if (value > 0)
                {
                    charNum = value;
                    loadLedNum(charNum);
                    SetText(text);
                }
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("数量颜色"), HDescriptionLanguage("led文字的颜色"), Browsable(true)]
        public Color NumColor
        {
            get { return numColor; }
            set
            {
                numColor = value;
                foreach (HLedNum ledNum in this.Controls)
                {
                    ledNum.NumColor = numColor; ;
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
                foreach (HLedNum ledNum in this.Controls)
                {
                    ledNum.SpaceWidth = spaceWidth;
                }
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("字符时间间隔"), HDescriptionLanguage("led文字的间隔"), Browsable(true)]
        public int CharInterval
        {
            get { return charInterval; }
            set
            {
                charInterval = value;
                foreach (HLedNum ledNum in this.Controls)
                {
                    ledNum.Margin = new Padding(charInterval, 0, charInterval, 0);
                }
            }
        }

        [HCategoryLanguage("通用"), HDisplayNameLanguage("文本"), HDescriptionLanguage("led文字"), Browsable(true)]
        public new string Text
        {
            get { return text; }
            set
            {
                if (value != "")
                {
                    foreach (char c in value)
                    {
                        if (!UseableCharList.Contains(c))
                        {
                            return;
                        }
                    }
                }
                text = value;
                SetText(text);
            }
        }

        /// <summary>UseableCharList 字段。</summary>
        private List<char> UseableCharList = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-', ':', '_', ' ' };

        public HLedBar()
        {
            InitializeComponent();
        }

        /// <summary>loadLedNum 方法。</summary>
        private void loadLedNum(int num)
        {
            this.Controls.Clear();
            for (int i = 0; i < num; i++)
            {
                HLedNum ledNum = new HLedNum();
                this.Controls.Add(ledNum);
                ledNum.Margin = new Padding(charInterval, 0, charInterval, 0);
                ledNum.Height = this.Height;
                ledNum.SpaceWidth = spaceWidth;
                ledNum.NumColor = numColor;
                //ledNum.Dock = DockStyle.Left;
            }
        }

        /// <summary>设置 text。</summary>
        private void SetText(string text)
        {
            if (text.Length >= this.Controls.Count)
            {
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    (this.Controls[i] as HLedNum).NumChar = text[i];
                }
            }
            else
            {
                for (int i = 0; i < text.Length; i++)
                {
                    (this.Controls[i] as HLedNum).NumChar = text[i];
                }
            }
        }

        /// <summary>响应 HandleCreated 事件。</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            loadLedNum(charNum);
            SetText(text);
        }

        /// <summary>响应 SizeChanged 事件。</summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            foreach (Control c in this.Controls)
            {
                c.Height = this.Height;
                //this.BeginInvoke(new MethodInvoker(delegate () { c.Height = this.Height; }));
            }
        }
    }
}