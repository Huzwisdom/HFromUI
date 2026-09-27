using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    public partial class ZxMessageShow : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public ZxMessageShow()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>ZxMessageShow0 字段。</summary>
        private static ZxMessageShow ZxMessageShow0 = new ZxMessageShow();
        /// <summary>ZxMessageShowA 字段。</summary>
        private static ZxMessageShow ZxMessageShowA = new ZxMessageShow();
        /// <summary>ZxMessageShowB 字段。</summary>
        private static ZxMessageShow ZxMessageShowB = new ZxMessageShow();
        /// <summary>ZxMessageShowC 字段。</summary>
        private static ZxMessageShow ZxMessageShowC = new ZxMessageShow();
        /// <summary>ZxMessageShowD 字段。</summary>
        private static ZxMessageShow ZxMessageShowD = new ZxMessageShow();
        /// <summary>ZxMessageShowE 字段。</summary>
        private static ZxMessageShow ZxMessageShowE = new ZxMessageShow();
        /// <summary>ZxMessageShowF 字段。</summary>
        private static ZxMessageShow ZxMessageShowF = new ZxMessageShow();
        /// <summary>ZxMessageShowG 字段。</summary>
        private static ZxMessageShow ZxMessageShowG = new ZxMessageShow();
        /// <summary>ZxMessageShowH 字段。</summary>
        private static ZxMessageShow ZxMessageShowH = new ZxMessageShow();
        public static bool GetIsShow
        {
            get { return ZxMessageShow0.IsShow; }
        }
        public static bool GetIsShowA
        {
            get { return ZxMessageShowA.IsShow; }
        }
        public static bool GetIsShowB
        {
            get { return ZxMessageShowB.IsShow; }
        }
        public static bool GetIsShowC
        {
            get { return ZxMessageShowC.IsShow; }
        }
        public static bool GetIsShowD
        {
            get { return ZxMessageShowD.IsShow; }
        }
        public static bool GetIsShowE
        {
            get { return ZxMessageShowE.IsShow; }
        }
        public static bool GetIsShowF
        {
            get { return ZxMessageShowF.IsShow; }
        }
        public static bool GetIsShowG
        {
            get { return ZxMessageShowG.IsShow; }
        }
        public static bool GetIsShowH
        {
            get { return ZxMessageShowH.IsShow; }
        }

        public static bool ShowIsCanClose
        {
            set {
                ZxMessageShow0.IsCanClose = value;
            }
            get {
                return ZxMessageShow0.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseA
        {
            set
            {
                ZxMessageShowA.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowA.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseB
        {
            set
            {
                ZxMessageShowB.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowB.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseC
        {
            set
            {
                ZxMessageShowC.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseD
        {
            set
            {
                ZxMessageShowD.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowD.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseE
        {
            set
            {
                ZxMessageShowE.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowE.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseF
        {
            set
            {
                ZxMessageShowF.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowF.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseG
        {
            set
            {
                ZxMessageShowG.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowG.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseH
        {
            set
            {
                ZxMessageShowH.IsCanClose = value;
            }
            get
            {
                return ZxMessageShowH.IsCanClose;
            }
        }


        /// <summary>ShowClose 方法。</summary>
        public static void ShowClose()
        {
            if (ZxMessageShow0.IsShow)
            {
                ZxMessageShow0.btn_Close_Click(null, null);
            }
         
        }
        /// <summary>ShowCloseA 方法。</summary>
        public static void ShowCloseA()
        {
            if (ZxMessageShowA.IsShow)
            {
                ZxMessageShowA.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseB 方法。</summary>
        public static void ShowCloseB()
        {
            if (ZxMessageShowB.IsShow)
            {
                ZxMessageShowB.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseC 方法。</summary>
        public static void ShowCloseC()
        {
            if (ZxMessageShowC.IsShow)
            {
                ZxMessageShowC.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseD 方法。</summary>
        public static void ShowCloseD()
        {
            if (ZxMessageShowD.IsShow)
            {
                ZxMessageShowD.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseE 方法。</summary>
        public static void ShowCloseE()
        {
            if (ZxMessageShowE.IsShow)
            {
                ZxMessageShowE.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseF 方法。</summary>
        public static void ShowCloseF()
        {
            if (ZxMessageShowF.IsShow)
            {
                ZxMessageShowF.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseG 方法。</summary>
        public static void ShowCloseG()
        {
            if (ZxMessageShowG.IsShow)
            {
                ZxMessageShowG.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseH 方法。</summary>
        public static void ShowCloseH()
        {
            if (ZxMessageShowH.IsShow)
            {
                ZxMessageShowH.btn_Close_Click(null, null);
            }
        }

        /// <summary>显示。</summary>
        public static void Show(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShow0.ZzShow(content, title, mode,okContent,closeContent);
        }
        /// <summary>ShowA 方法。</summary>
        public static void ShowA(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowA.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowB 方法。</summary>
        public static void ShowB(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowB.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowC 方法。</summary>
        public static void ShowC(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowC.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowD 方法。</summary>
        public static void ShowD(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowD.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowE 方法。</summary>
        public static void ShowE(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowE.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowF 方法。</summary>
        public static void ShowF(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowF.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowG 方法。</summary>
        public static void ShowG(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowG.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowH 方法。</summary>
        public static void ShowH(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShowH.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>Show2 方法。</summary>
        public static void Show2(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShow ZxMessageShowFF = new ZxMessageShow();
            ZxMessageShowFF.ZzShow(content,title,mode, okContent, closeContent);
        }
   
        /// <summary>ShowDialog 方法。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZxMessageShow ZxMessageShowDD = new ZxMessageShow();
            DialogResult DialogResultZxMessageShowDD= ZxMessageShowDD.ZzShowDialog(content, title, mode, okContent, closeContent);
            ZxMessageShowDD.Dispose();
            ZxMessageShowDD = null;
            return DialogResultZxMessageShowDD;
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
        public  void ZzShow(string content, string title="",int mode=0,string okContent = "",string closeContent="")
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
                    if (!string.IsNullOrWhiteSpace(okContent))
                    {
                        btn_OK.Text = okContent;
                    }
                    else
                    {
                        btn_OK.Text = "Confirm";
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
                        if (!string.IsNullOrWhiteSpace(okContent))
                        {
                            btn_OK.Text = okContent;
                        }
                        else
                        {
                            btn_OK.Text = "Confirm";
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
        public DialogResult ZzShowDialog(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
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
                if (!string.IsNullOrWhiteSpace(okContent))
                {
                    btn_OK.Text = okContent;
                }
                else
                {
                    btn_OK.Text = "Confirm";
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
                    if (!string.IsNullOrWhiteSpace(okContent))
                    {
                        btn_OK.Text = okContent;
                    }
                    else
                    {
                        btn_OK.Text = "Confirm";
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
                btn_OK.BackColor = Color.White;
                btn_Close.BackColor = Color.White;
            }
            else
            {
                if (btn_OK.BackColor == Color.White)
                {
                    btn_OK.BackColor = Color.Red;
                }
                else
                {
                    btn_OK.BackColor = Color.White;
                }
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
        /// <summary>ZxMessageShow_Load 方法。</summary>
        private void ZxMessageShow_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
