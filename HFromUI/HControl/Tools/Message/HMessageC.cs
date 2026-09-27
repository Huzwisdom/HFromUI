using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Message
{
    public partial class HMessageC : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public HMessageC()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>hMessageC 字段。</summary>
        private static HMessageC hMessageC = new HMessageC();
        /// <summary>hMessageCA 字段。</summary>
        private static HMessageC hMessageCA = new HMessageC();
        /// <summary>hMessageCB 字段。</summary>
        private static HMessageC hMessageCB = new HMessageC();
        /// <summary>hMessageCC 字段。</summary>
        private static HMessageC hMessageCC = new HMessageC();
        /// <summary>hMessageCD 字段。</summary>
        private static HMessageC hMessageCD = new HMessageC();
        /// <summary>hMessageCE 字段。</summary>
        private static HMessageC hMessageCE = new HMessageC();
        /// <summary>hMessageCF 字段。</summary>
        private static HMessageC hMessageCF = new HMessageC();
        /// <summary>hMessageCG 字段。</summary>
        private static HMessageC hMessageCG = new HMessageC();
        /// <summary>hMessageCH 字段。</summary>
        private static HMessageC hMessageCH = new HMessageC();
        public static bool GetIsShow
        {
            get { return hMessageC.IsShow; }
        }
        public static bool GetIsShowA
        {
            get { return hMessageCA.IsShow; }
        }
        public static bool GetIsShowB
        {
            get { return hMessageCB.IsShow; }
        }
        public static bool GetIsShowC
        {
            get { return hMessageCC.IsShow; }
        }
        public static bool GetIsShowD
        {
            get { return hMessageCD.IsShow; }
        }
        public static bool GetIsShowE
        {
            get { return hMessageCE.IsShow; }
        }
        public static bool GetIsShowF
        {
            get { return hMessageCF.IsShow; }
        }
        public static bool GetIsShowG
        {
            get { return hMessageCG.IsShow; }
        }
        public static bool GetIsShowH
        {
            get { return hMessageCH.IsShow; }
        }

        public static bool ShowIsCanClose
        {
            set {
                hMessageC.IsCanClose = value;
            }
            get {
                return hMessageC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseA
        {
            set
            {
                hMessageCA.IsCanClose = value;
            }
            get
            {
                return hMessageCA.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseB
        {
            set
            {
                hMessageCB.IsCanClose = value;
            }
            get
            {
                return hMessageCB.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseC
        {
            set
            {
                hMessageCC.IsCanClose = value;
            }
            get
            {
                return hMessageCC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseD
        {
            set
            {
                hMessageCD.IsCanClose = value;
            }
            get
            {
                return hMessageCD.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseE
        {
            set
            {
                hMessageCE.IsCanClose = value;
            }
            get
            {
                return hMessageCE.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseF
        {
            set
            {
                hMessageCF.IsCanClose = value;
            }
            get
            {
                return hMessageCF.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseG
        {
            set
            {
                hMessageCG.IsCanClose = value;
            }
            get
            {
                return hMessageCG.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseH
        {
            set
            {
                hMessageCH.IsCanClose = value;
            }
            get
            {
                return hMessageCH.IsCanClose;
            }
        }


        /// <summary>ShowClose 方法。</summary>
        public static void ShowClose()
        {
            if (hMessageC.IsShow)
            {
                hMessageC.btn_Close_Click(null, null);
            }
         
        }
        /// <summary>ShowCloseA 方法。</summary>
        public static void ShowCloseA()
        {
            if (hMessageCA.IsShow)
            {
                hMessageCA.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseB 方法。</summary>
        public static void ShowCloseB()
        {
            if (hMessageCB.IsShow)
            {
                hMessageCB.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseC 方法。</summary>
        public static void ShowCloseC()
        {
            if (hMessageCC.IsShow)
            {
                hMessageCC.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseD 方法。</summary>
        public static void ShowCloseD()
        {
            if (hMessageCD.IsShow)
            {
                hMessageCD.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseE 方法。</summary>
        public static void ShowCloseE()
        {
            if (hMessageCE.IsShow)
            {
                hMessageCE.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseF 方法。</summary>
        public static void ShowCloseF()
        {
            if (hMessageCF.IsShow)
            {
                hMessageCF.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseG 方法。</summary>
        public static void ShowCloseG()
        {
            if (hMessageCG.IsShow)
            {
                hMessageCG.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseH 方法。</summary>
        public static void ShowCloseH()
        {
            if (hMessageCH.IsShow)
            {
                hMessageCH.btn_Close_Click(null, null);
            }
        }

        /// <summary>显示。</summary>
        public static void Show(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageC.ZzShow(content, title, mode,okContent,closeContent);
        }
        /// <summary>ShowA 方法。</summary>
        public static void ShowA(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCA.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowB 方法。</summary>
        public static void ShowB(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCB.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowC 方法。</summary>
        public static void ShowC(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCC.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowD 方法。</summary>
        public static void ShowD(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCD.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowE 方法。</summary>
        public static void ShowE(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCE.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowF 方法。</summary>
        public static void ShowF(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCF.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowG 方法。</summary>
        public static void ShowG(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCG.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowH 方法。</summary>
        public static void ShowH(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            hMessageCH.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>Show2 方法。</summary>
        public static void Show2(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageC hMessageCFF = new HMessageC();
            hMessageCFF.ZzShow(content,title,mode, okContent, closeContent);
        }
   
        /// <summary>ShowDialog 方法。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageC hMessageCDD = new HMessageC();
            DialogResult DialogResulthMessageCDD= hMessageCDD.ZzShowDialog(content, title, mode, okContent, closeContent);
            hMessageCDD.Dispose();
            hMessageCDD = null;
            return DialogResulthMessageCDD;
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
        /// <summary>HMessageC_Load 方法。</summary>
        private void HMessageC_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
