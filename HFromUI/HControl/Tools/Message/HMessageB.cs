using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Message
{
    public partial class HMessageB : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public HMessageB()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>HMessageB0 字段。</summary>
        private static HMessageB HMessageB0 = new HMessageB();
        /// <summary>HMessageBA 字段。</summary>
        private static HMessageB HMessageBA = new HMessageB();
        /// <summary>HMessageBB 字段。</summary>
        private static HMessageB HMessageBB = new HMessageB();
        /// <summary>HMessageBC 字段。</summary>
        private static HMessageB HMessageBC = new HMessageB();
        /// <summary>HMessageBD 字段。</summary>
        private static HMessageB HMessageBD = new HMessageB();
        /// <summary>HMessageBE 字段。</summary>
        private static HMessageB HMessageBE = new HMessageB();
        /// <summary>HMessageBF 字段。</summary>
        private static HMessageB HMessageBF = new HMessageB();
        /// <summary>HMessageBG 字段。</summary>
        private static HMessageB HMessageBG = new HMessageB();
        /// <summary>HMessageBH 字段。</summary>
        private static HMessageB HMessageBH = new HMessageB();
        public static bool GetIsShow
        {
            get { return HMessageB0.IsShow; }
        }
        public static bool GetIsShowA
        {
            get { return HMessageBA.IsShow; }
        }
        public static bool GetIsShowB
        {
            get { return HMessageBB.IsShow; }
        }
        public static bool GetIsShowC
        {
            get { return HMessageBC.IsShow; }
        }
        public static bool GetIsShowD
        {
            get { return HMessageBD.IsShow; }
        }
        public static bool GetIsShowE
        {
            get { return HMessageBE.IsShow; }
        }
        public static bool GetIsShowF
        {
            get { return HMessageBF.IsShow; }
        }
        public static bool GetIsShowG
        {
            get { return HMessageBG.IsShow; }
        }
        public static bool GetIsShowH
        {
            get { return HMessageBH.IsShow; }
        }

        public static bool ShowIsCanClose
        {
            set {
                HMessageB0.IsCanClose = value;
            }
            get {
                return HMessageB0.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseA
        {
            set
            {
                HMessageBA.IsCanClose = value;
            }
            get
            {
                return HMessageBA.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseB
        {
            set
            {
                HMessageBB.IsCanClose = value;
            }
            get
            {
                return HMessageBB.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseC
        {
            set
            {
                HMessageBC.IsCanClose = value;
            }
            get
            {
                return HMessageBC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseD
        {
            set
            {
                HMessageBD.IsCanClose = value;
            }
            get
            {
                return HMessageBD.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseE
        {
            set
            {
                HMessageBE.IsCanClose = value;
            }
            get
            {
                return HMessageBE.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseF
        {
            set
            {
                HMessageBF.IsCanClose = value;
            }
            get
            {
                return HMessageBF.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseG
        {
            set
            {
                HMessageBG.IsCanClose = value;
            }
            get
            {
                return HMessageBG.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseH
        {
            set
            {
                HMessageBH.IsCanClose = value;
            }
            get
            {
                return HMessageBH.IsCanClose;
            }
        }


        /// <summary>ShowClose 方法。</summary>
        public static void ShowClose()
        {
            if (HMessageB0.IsShow)
            {
                HMessageB0.btn_Close_Click(null, null);
            }
         
        }
        /// <summary>ShowCloseA 方法。</summary>
        public static void ShowCloseA()
        {
            if (HMessageBA.IsShow)
            {
                HMessageBA.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseB 方法。</summary>
        public static void ShowCloseB()
        {
            if (HMessageBB.IsShow)
            {
                HMessageBB.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseC 方法。</summary>
        public static void ShowCloseC()
        {
            if (HMessageBC.IsShow)
            {
                HMessageBC.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseD 方法。</summary>
        public static void ShowCloseD()
        {
            if (HMessageBD.IsShow)
            {
                HMessageBD.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseE 方法。</summary>
        public static void ShowCloseE()
        {
            if (HMessageBE.IsShow)
            {
                HMessageBE.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseF 方法。</summary>
        public static void ShowCloseF()
        {
            if (HMessageBF.IsShow)
            {
                HMessageBF.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseG 方法。</summary>
        public static void ShowCloseG()
        {
            if (HMessageBG.IsShow)
            {
                HMessageBG.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseH 方法。</summary>
        public static void ShowCloseH()
        {
            if (HMessageBH.IsShow)
            {
                HMessageBH.btn_Close_Click(null, null);
            }
        }

        /// <summary>显示。</summary>
        public static void Show(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageB0.ZzShow(content, title, mode,okContent,closeContent);
        }
        /// <summary>ShowA 方法。</summary>
        public static void ShowA(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBA.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowB 方法。</summary>
        public static void ShowB(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBB.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowC 方法。</summary>
        public static void ShowC(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBC.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowD 方法。</summary>
        public static void ShowD(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBD.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowE 方法。</summary>
        public static void ShowE(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBE.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowF 方法。</summary>
        public static void ShowF(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBF.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowG 方法。</summary>
        public static void ShowG(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBG.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowH 方法。</summary>
        public static void ShowH(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageBH.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>Show2 方法。</summary>
        public static void Show2(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageB HMessageBFF = new HMessageB();
            HMessageBFF.ZzShow(content,title,mode, okContent, closeContent);
        }
   
        /// <summary>ShowDialog 方法。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            HMessageB HMessageBDD = new HMessageB();
            DialogResult DialogResultHMessageBDD= HMessageBDD.ZzShowDialog(content, title, mode, okContent, closeContent);
            HMessageBDD.Dispose();
            HMessageBDD = null;
            return DialogResultHMessageBDD;
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
        /// <summary>HMessageB_Load 方法。</summary>
        private void HMessageB_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
