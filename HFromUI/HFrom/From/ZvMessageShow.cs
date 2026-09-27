using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    public partial class ZvMessageShow : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public ZvMessageShow()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>ZvMessageShow0 字段。</summary>
        private static ZvMessageShow ZvMessageShow0 = new ZvMessageShow();
        /// <summary>ZvMessageShowA 字段。</summary>
        private static ZvMessageShow ZvMessageShowA = new ZvMessageShow();
        /// <summary>ZvMessageShowB 字段。</summary>
        private static ZvMessageShow ZvMessageShowB = new ZvMessageShow();
        /// <summary>ZvMessageShowC 字段。</summary>
        private static ZvMessageShow ZvMessageShowC = new ZvMessageShow();
        /// <summary>ZvMessageShowD 字段。</summary>
        private static ZvMessageShow ZvMessageShowD = new ZvMessageShow();
        /// <summary>ZvMessageShowE 字段。</summary>
        private static ZvMessageShow ZvMessageShowE = new ZvMessageShow();
        /// <summary>ZvMessageShowF 字段。</summary>
        private static ZvMessageShow ZvMessageShowF = new ZvMessageShow();
        /// <summary>ZvMessageShowG 字段。</summary>
        private static ZvMessageShow ZvMessageShowG = new ZvMessageShow();
        /// <summary>ZvMessageShowH 字段。</summary>
        private static ZvMessageShow ZvMessageShowH = new ZvMessageShow();
        public static bool GetIsShow
        {
            get { return ZvMessageShow0.IsShow; }
        }
        public static bool GetIsShowA
        {
            get { return ZvMessageShowA.IsShow; }
        }
        public static bool GetIsShowB
        {
            get { return ZvMessageShowB.IsShow; }
        }
        public static bool GetIsShowC
        {
            get { return ZvMessageShowC.IsShow; }
        }
        public static bool GetIsShowD
        {
            get { return ZvMessageShowD.IsShow; }
        }
        public static bool GetIsShowE
        {
            get { return ZvMessageShowE.IsShow; }
        }
        public static bool GetIsShowF
        {
            get { return ZvMessageShowF.IsShow; }
        }
        public static bool GetIsShowG
        {
            get { return ZvMessageShowG.IsShow; }
        }
        public static bool GetIsShowH
        {
            get { return ZvMessageShowH.IsShow; }
        }

        public static bool ShowIsCanClose
        {
            set {
                ZvMessageShow0.IsCanClose = value;
            }
            get {
                return ZvMessageShow0.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseA
        {
            set
            {
                ZvMessageShowA.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowA.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseB
        {
            set
            {
                ZvMessageShowB.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowB.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseC
        {
            set
            {
                ZvMessageShowC.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseD
        {
            set
            {
                ZvMessageShowD.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowD.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseE
        {
            set
            {
                ZvMessageShowE.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowE.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseF
        {
            set
            {
                ZvMessageShowF.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowF.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseG
        {
            set
            {
                ZvMessageShowG.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowG.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseH
        {
            set
            {
                ZvMessageShowH.IsCanClose = value;
            }
            get
            {
                return ZvMessageShowH.IsCanClose;
            }
        }


        /// <summary>ShowClose 方法。</summary>
        public static void ShowClose()
        {
            if (ZvMessageShow0.IsShow)
            {
                ZvMessageShow0.btn_Close_Click(null, null);
            }
         
        }
        /// <summary>ShowCloseA 方法。</summary>
        public static void ShowCloseA()
        {
            if (ZvMessageShowA.IsShow)
            {
                ZvMessageShowA.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseB 方法。</summary>
        public static void ShowCloseB()
        {
            if (ZvMessageShowB.IsShow)
            {
                ZvMessageShowB.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseC 方法。</summary>
        public static void ShowCloseC()
        {
            if (ZvMessageShowC.IsShow)
            {
                ZvMessageShowC.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseD 方法。</summary>
        public static void ShowCloseD()
        {
            if (ZvMessageShowD.IsShow)
            {
                ZvMessageShowD.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseE 方法。</summary>
        public static void ShowCloseE()
        {
            if (ZvMessageShowE.IsShow)
            {
                ZvMessageShowE.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseF 方法。</summary>
        public static void ShowCloseF()
        {
            if (ZvMessageShowF.IsShow)
            {
                ZvMessageShowF.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseG 方法。</summary>
        public static void ShowCloseG()
        {
            if (ZvMessageShowG.IsShow)
            {
                ZvMessageShowG.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseH 方法。</summary>
        public static void ShowCloseH()
        {
            if (ZvMessageShowH.IsShow)
            {
                ZvMessageShowH.btn_Close_Click(null, null);
            }
        }

        /// <summary>显示。</summary>
        public static void Show(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShow0.ZzShow(content, title, mode,closeContent);
        }
        /// <summary>ShowA 方法。</summary>
        public static void ShowA(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowA.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowB 方法。</summary>
        public static void ShowB(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowB.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowC 方法。</summary>
        public static void ShowC(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowC.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowD 方法。</summary>
        public static void ShowD(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowD.ZzShow(content, title, mode,  closeContent);

        }
        /// <summary>ShowE 方法。</summary>
        public static void ShowE(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowE.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>ShowF 方法。</summary>
        public static void ShowF(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowF.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>ShowG 方法。</summary>
        public static void ShowG(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowG.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>ShowH 方法。</summary>
        public static void ShowH(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShowH.ZzShow(content, title, mode,  closeContent);
        }
        /// <summary>Show2 方法。</summary>
        public static void Show2(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShow ZvMessageShowFF = new ZvMessageShow();
            ZvMessageShowFF.ZzShow(content,title,mode,  closeContent);
        }
   
        /// <summary>ShowDialog 方法。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0,  string closeContent = "")
        {
            ZvMessageShow ZvMessageShowDD = new ZvMessageShow();
            DialogResult DialogResultZvMessageShowDD= ZvMessageShowDD.ZzShowDialog(content, title, mode,  closeContent);
            ZvMessageShowDD.Dispose();
            ZvMessageShowDD = null;
            return DialogResultZvMessageShowDD;
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
        /// <summary>ZvMessageShow_Load 方法。</summary>
        private void ZvMessageShow_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
