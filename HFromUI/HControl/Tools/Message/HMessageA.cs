using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Message
{
    public partial class HMessageA : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public HMessageA()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>HMessageA0 字段。</summary>
        private static HMessageA HMessageA0 = new HMessageA();
        /// <summary>HMessageAA 字段。</summary>
        private static HMessageA HMessageAA = new HMessageA();
        /// <summary>HMessageAB 字段。</summary>
        private static HMessageA HMessageAB = new HMessageA();
        /// <summary>HMessageAC 字段。</summary>
        private static HMessageA HMessageAC = new HMessageA();
        /// <summary>HMessageAD 字段。</summary>
        private static HMessageA HMessageAD = new HMessageA();
        /// <summary>HMessageAE 字段。</summary>
        private static HMessageA HMessageAE = new HMessageA();
        /// <summary>HMessageAF 字段。</summary>
        private static HMessageA HMessageAF = new HMessageA();
        /// <summary>HMessageAG 字段。</summary>
        private static HMessageA HMessageAG = new HMessageA();
        /// <summary>HMessageAH 字段。</summary>
        private static HMessageA HMessageAH = new HMessageA();
        public static bool GetIsShow
        {
            get { return HMessageA0.IsShow; }
        }
        public static bool GetIsShowA
        {
            get { return HMessageAA.IsShow; }
        }
        public static bool GetIsShowB
        {
            get { return HMessageAB.IsShow; }
        }
        public static bool GetIsShowC
        {
            get { return HMessageAC.IsShow; }
        }
        public static bool GetIsShowD
        {
            get { return HMessageAD.IsShow; }
        }
        public static bool GetIsShowE
        {
            get { return HMessageAE.IsShow; }
        }
        public static bool GetIsShowF
        {
            get { return HMessageAF.IsShow; }
        }
        public static bool GetIsShowG
        {
            get { return HMessageAG.IsShow; }
        }
        public static bool GetIsShowH
        {
            get { return HMessageAH.IsShow; }
        }

        public static bool ShowIsCanClose
        {
            set {
                HMessageA0.IsCanClose = value;
            }
            get {
                return HMessageA0.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseA
        {
            set
            {
                HMessageAA.IsCanClose = value;
            }
            get
            {
                return HMessageAA.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseB
        {
            set
            {
                HMessageAB.IsCanClose = value;
            }
            get
            {
                return HMessageAB.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseC
        {
            set
            {
                HMessageAC.IsCanClose = value;
            }
            get
            {
                return HMessageAC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseD
        {
            set
            {
                HMessageAD.IsCanClose = value;
            }
            get
            {
                return HMessageAD.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseE
        {
            set
            {
                HMessageAE.IsCanClose = value;
            }
            get
            {
                return HMessageAE.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseF
        {
            set
            {
                HMessageAF.IsCanClose = value;
            }
            get
            {
                return HMessageAF.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseG
        {
            set
            {
                HMessageAG.IsCanClose = value;
            }
            get
            {
                return HMessageAG.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseH
        {
            set
            {
                HMessageAH.IsCanClose = value;
            }
            get
            {
                return HMessageAH.IsCanClose;
            }
        }


        /// <summary>ShowClose 方法。</summary>
        public static void ShowClose()
        {
            if (HMessageA0.IsShow)
            {
                HMessageA0.btn_Close_Click(null, null);
            }
         
        }
        /// <summary>ShowCloseA 方法。</summary>
        public static void ShowCloseA()
        {
            if (HMessageAA.IsShow)
            {
                HMessageAA.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseB 方法。</summary>
        public static void ShowCloseB()
        {
            if (HMessageAB.IsShow)
            {
                HMessageAB.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseC 方法。</summary>
        public static void ShowCloseC()
        {
            if (HMessageAC.IsShow)
            {
                HMessageAC.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseD 方法。</summary>
        public static void ShowCloseD()
        {
            if (HMessageAD.IsShow)
            {
                HMessageAD.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseE 方法。</summary>
        public static void ShowCloseE()
        {
            if (HMessageAE.IsShow)
            {
                HMessageAE.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseF 方法。</summary>
        public static void ShowCloseF()
        {
            if (HMessageAF.IsShow)
            {
                HMessageAF.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseG 方法。</summary>
        public static void ShowCloseG()
        {
            if (HMessageAG.IsShow)
            {
                HMessageAG.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseH 方法。</summary>
        public static void ShowCloseH()
        {
            if (HMessageAH.IsShow)
            {
                HMessageAH.btn_Close_Click(null, null);
            }
        }

        /// <summary>显示。</summary>
        public static void Show(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageA0.ZzShow(content, title, mode,closeContent);
        }
        /// <summary>ShowA 方法。</summary>
        public static void ShowA(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAA.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowB 方法。</summary>
        public static void ShowB(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAB.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowC 方法。</summary>
        public static void ShowC(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAC.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowD 方法。</summary>
        public static void ShowD(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAD.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowE 方法。</summary>
        public static void ShowE(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAE.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>ShowF 方法。</summary>
        public static void ShowF(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAF.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>ShowG 方法。</summary>
        public static void ShowG(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAG.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>ShowH 方法。</summary>
        public static void ShowH(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageAH.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>Show2 方法。</summary>
        public static void Show2(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageA HMessageAFF = new HMessageA();
            HMessageAFF.ZzShow(content,title,mode,  closeContent);
        }
   
        /// <summary>ShowDialog 方法。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            HMessageA HMessageADD = new HMessageA();
            DialogResult DialogResultHMessageADD= HMessageADD.ZzShowDialog(content, title, mode,  closeContent);
            HMessageADD.Dispose();
            HMessageADD = null;
            return DialogResultHMessageADD;
        }
        /// <summary>响应 FormClosing 事件。</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!IsCanClose)
            {
                e.Cancel = true;
                return;
            }
            if (modeTitle>0)
            {
                e.Cancel = true;
                this.Hide();
                IsShow = false;
                time_show.Enabled = IsShow;
            }
            else
            {
                base.OnFormClosing(e);
                IsShow = false;
                time_show.Enabled = IsShow;
            }
         
        }
        /// <summary>modeTitle 字段。</summary>
        private int modeTitle = 0;
        /// <summary>ZzShow 方法。</summary>
        public  void ZzShow(string content, string title="",int mode=0,string closeContent="")
        {
            _uiContext.Post(_ =>
            {
                if (!this.InvokeRequired)
                {
                    this.DialogResult = DialogResult.Cancel;
                    if (mode == 0)
                    {
                        mode = 1;
                    }
                    if (mode > 0)
                    {
                        modeTitle = mode;
                    }
                    else
                    {
                        modeTitle = mode * -1;
                    }
                    if (modeTitle == 1 || modeTitle == -1)
                    {
                        lbl_title.BackColor = Color.DarkRed;
                        lbl_show.BackColor = Color.OrangeRed;
                    }
                    else
                    {
                        lbl_title.BackColor = Color.Orange;
                        lbl_show.BackColor = Color.Green;
                    }
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        lbl_title.Text = title; this.Text = title;
                    }
                    if (!string.IsNullOrWhiteSpace(closeContent))
                    {
                        btn_Close.Text = closeContent;
                    }
                    else
                    {
                        btn_Close.Text = "Close";
                    }
                    lbl_show.Text = content;
                    if (!IsShow)
                    {
                        IsShow = true;
                        this.Show();
                    }
                    time_show.Enabled = IsShow;
                }
                else
                {
                    Invoke((EventHandler)delegate
                    {
                        this.DialogResult = DialogResult.Cancel;
                        if (mode == 0)
                        {
                            mode = 1;
                        }
                        if (mode > 0)
                        {
                            modeTitle = mode;
                        }
                        else
                        {
                            modeTitle = mode * -1;
                        }
                        if (modeTitle == 1 || modeTitle == -1)
                        {
                            lbl_title.BackColor = Color.DarkRed;
                            lbl_show.BackColor = Color.OrangeRed;
                        }
                        else
                        {
                            lbl_title.BackColor = Color.Orange;
                            lbl_show.BackColor = Color.Green;
                        }
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            lbl_title.Text = title; this.Text = title;
                        }
                        if (!string.IsNullOrWhiteSpace(closeContent))
                        {
                            btn_Close.Text = closeContent;
                        }
                        else
                        {
                            btn_Close.Text = "Close";
                        }
                        lbl_show.Text = content;
                        if (!IsShow)
                        {
                            IsShow = true;
                            this.Show();
                        }
                        time_show.Enabled = IsShow;
                    });
                }
            }, null);

        }
        /// <summary>ZzShowDialog 方法。</summary>
        public DialogResult ZzShowDialog(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            if (!this.InvokeRequired)
            {
                this.DialogResult = DialogResult.Cancel;
                IsShow = true;
                if (mode == 0)
                {
                    mode = 1;
                }
                if (mode < 0)
                {
                    modeTitle = mode;
                }
                else
                {
                    modeTitle = mode * -1;
                }
                if (modeTitle == 1 || modeTitle == -1)
                {
                    lbl_title.BackColor = Color.DarkRed;
                    lbl_show.BackColor = Color.OrangeRed;
                }
                else
                {
                    lbl_title.BackColor = Color.Orange;
                    lbl_show.BackColor = Color.Green;
                }
                if (!string.IsNullOrWhiteSpace(title))
                {
                    lbl_title.Text = title; this.Text = title;
                }
                if (!string.IsNullOrWhiteSpace(closeContent))
                {
                    btn_Close.Text = closeContent;
                }
                else
                {
                    btn_Close.Text = "Close";
                }
                lbl_show.Text = content;
                time_show.Enabled = IsShow;
                this.ShowDialog();
            }
            else
            {
                Invoke((EventHandler)delegate
                {
                    this.DialogResult = DialogResult.Cancel;
                    IsShow = true;
                    if (mode == 0)
                    {
                        mode = 1;
                    }
                    if (mode < 0)
                    {
                        modeTitle = mode;
                    }
                    else
                    {
                        modeTitle = mode * -1;
                    }
                    if (modeTitle == 1 || modeTitle == -1)
                    {
                        lbl_title.BackColor = Color.DarkRed;
                        lbl_show.BackColor = Color.OrangeRed;
                    }
                    else
                    {
                        lbl_title.BackColor = Color.Orange;
                        lbl_show.BackColor = Color.Green;
                    }
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        lbl_title.Text = title; this.Text = title;
                    }
                    if (!string.IsNullOrWhiteSpace(closeContent))
                    {
                        btn_Close.Text = closeContent;
                    }
                    else
                    {
                        btn_Close.Text = "Close";
                    }
                    lbl_show.Text = content;
                    time_show.Enabled = IsShow;
                    this.ShowDialog();
                });
            }
            return this.DialogResult;
        }
        /// <summary>time_show_Tick 方法。</summary>
        private void time_show_Tick(object sender, EventArgs e)
        {
            if (IsShow)
            {
                if (modeTitle == 1 || modeTitle == -1)
                {
                    if (lbl_title.BackColor == Color.DarkRed)
                    {
                        lbl_title.BackColor = Color.IndianRed;
                    }
                    else
                    {
                        lbl_title.BackColor = Color.DarkRed;
                    }
                }
                else if (modeTitle == 2 || modeTitle == -2)
                {
                    if (lbl_title.BackColor == Color.Orange)
                    {
                        lbl_title.BackColor = Color.DarkOrange;
                    }
                    else
                    {
                        lbl_title.BackColor = Color.Orange;
                    }
                }
            }
            if (IsCanClose)
            {
                btn_Close.BackColor = Color.White;
            }
            else
            {
                
                if (btn_Close.BackColor == Color.White)
                {
                    btn_Close.BackColor = Color.Red;
                }
                else
                {
                    btn_Close.BackColor = Color.White;
                }
            }
        }
        /// <summary>btn_OK_Click 方法。</summary>
        private void btn_OK_Click(object sender, EventArgs e)
        {
            if (IsCanClose)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

        }
        /// <summary>btn_Close_Click 方法。</summary>
        private void btn_Close_Click(object sender, EventArgs e)
        {
            if (IsCanClose)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        
        }
        /// <summary>HMessageA_Load 方法。</summary>
        private void HMessageA_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
