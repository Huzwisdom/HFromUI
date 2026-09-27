using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HFrom.From
{
    public partial class ZzMessageShow : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public ZzMessageShow()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>zzMessageShow 字段。</summary>
        private static ZzMessageShow zzMessageShow = new ZzMessageShow();
        /// <summary>zzMessageShowA 字段。</summary>
        private static ZzMessageShow zzMessageShowA = new ZzMessageShow();
        /// <summary>zzMessageShowB 字段。</summary>
        private static ZzMessageShow zzMessageShowB = new ZzMessageShow();
        /// <summary>zzMessageShowC 字段。</summary>
        private static ZzMessageShow zzMessageShowC = new ZzMessageShow();
        /// <summary>zzMessageShowD 字段。</summary>
        private static ZzMessageShow zzMessageShowD = new ZzMessageShow();
        /// <summary>zzMessageShowE 字段。</summary>
        private static ZzMessageShow zzMessageShowE = new ZzMessageShow();
        /// <summary>zzMessageShowF 字段。</summary>
        private static ZzMessageShow zzMessageShowF = new ZzMessageShow();
        /// <summary>zzMessageShowG 字段。</summary>
        private static ZzMessageShow zzMessageShowG = new ZzMessageShow();
        /// <summary>zzMessageShowH 字段。</summary>
        private static ZzMessageShow zzMessageShowH = new ZzMessageShow();
        public static bool GetIsShow
        {
            get { return zzMessageShow.IsShow; }
        }
        public static bool GetIsShowA
        {
            get { return zzMessageShowA.IsShow; }
        }
        public static bool GetIsShowB
        {
            get { return zzMessageShowB.IsShow; }
        }
        public static bool GetIsShowC
        {
            get { return zzMessageShowC.IsShow; }
        }
        public static bool GetIsShowD
        {
            get { return zzMessageShowD.IsShow; }
        }
        public static bool GetIsShowE
        {
            get { return zzMessageShowE.IsShow; }
        }
        public static bool GetIsShowF
        {
            get { return zzMessageShowF.IsShow; }
        }
        public static bool GetIsShowG
        {
            get { return zzMessageShowG.IsShow; }
        }
        public static bool GetIsShowH
        {
            get { return zzMessageShowH.IsShow; }
        }

        public static bool ShowIsCanClose
        {
            set {
                zzMessageShow.IsCanClose = value;
            }
            get {
                return zzMessageShow.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseA
        {
            set
            {
                zzMessageShowA.IsCanClose = value;
            }
            get
            {
                return zzMessageShowA.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseB
        {
            set
            {
                zzMessageShowB.IsCanClose = value;
            }
            get
            {
                return zzMessageShowB.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseC
        {
            set
            {
                zzMessageShowC.IsCanClose = value;
            }
            get
            {
                return zzMessageShowC.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseD
        {
            set
            {
                zzMessageShowD.IsCanClose = value;
            }
            get
            {
                return zzMessageShowD.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseE
        {
            set
            {
                zzMessageShowE.IsCanClose = value;
            }
            get
            {
                return zzMessageShowE.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseF
        {
            set
            {
                zzMessageShowF.IsCanClose = value;
            }
            get
            {
                return zzMessageShowF.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseG
        {
            set
            {
                zzMessageShowG.IsCanClose = value;
            }
            get
            {
                return zzMessageShowG.IsCanClose;
            }
        }
        public static bool ShowIsCanCloseH
        {
            set
            {
                zzMessageShowH.IsCanClose = value;
            }
            get
            {
                return zzMessageShowH.IsCanClose;
            }
        }


        /// <summary>ShowClose 方法。</summary>
        public static void ShowClose()
        {
            if (zzMessageShow.IsShow)
            {
                zzMessageShow.btn_Close_Click(null, null);
            }
         
        }
        /// <summary>ShowCloseA 方法。</summary>
        public static void ShowCloseA()
        {
            if (zzMessageShowA.IsShow)
            {
                zzMessageShowA.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseB 方法。</summary>
        public static void ShowCloseB()
        {
            if (zzMessageShowB.IsShow)
            {
                zzMessageShowB.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseC 方法。</summary>
        public static void ShowCloseC()
        {
            if (zzMessageShowC.IsShow)
            {
                zzMessageShowC.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseD 方法。</summary>
        public static void ShowCloseD()
        {
            if (zzMessageShowD.IsShow)
            {
                zzMessageShowD.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseE 方法。</summary>
        public static void ShowCloseE()
        {
            if (zzMessageShowE.IsShow)
            {
                zzMessageShowE.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseF 方法。</summary>
        public static void ShowCloseF()
        {
            if (zzMessageShowF.IsShow)
            {
                zzMessageShowF.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseG 方法。</summary>
        public static void ShowCloseG()
        {
            if (zzMessageShowG.IsShow)
            {
                zzMessageShowG.btn_Close_Click(null, null);
            }
        }
        /// <summary>ShowCloseH 方法。</summary>
        public static void ShowCloseH()
        {
            if (zzMessageShowH.IsShow)
            {
                zzMessageShowH.btn_Close_Click(null, null);
            }
        }

        /// <summary>显示。</summary>
        public static void Show(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShow.ZzShow(content, title, mode,okContent,closeContent);
        }
        /// <summary>ShowA 方法。</summary>
        public static void ShowA(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowA.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowB 方法。</summary>
        public static void ShowB(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowB.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowC 方法。</summary>
        public static void ShowC(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowC.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowD 方法。</summary>
        public static void ShowD(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowD.ZzShow(content, title, mode, okContent, closeContent);

        }
        /// <summary>ShowE 方法。</summary>
        public static void ShowE(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowE.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowF 方法。</summary>
        public static void ShowF(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowF.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowG 方法。</summary>
        public static void ShowG(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowG.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>ShowH 方法。</summary>
        public static void ShowH(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            zzMessageShowH.ZzShow(content, title, mode, okContent, closeContent);
        }
        /// <summary>Show2 方法。</summary>
        public static void Show2(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZzMessageShow zzMessageShowFF = new ZzMessageShow();
            zzMessageShowFF.ZzShow(content,title,mode, okContent, closeContent);
        }
   
        /// <summary>ShowDialog 方法。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0, string okContent = "", string closeContent = "")
        {
            ZzMessageShow zzMessageShowDD = new ZzMessageShow();
            DialogResult DialogResultzzMessageShowDD= zzMessageShowDD.ZzShowDialog(content, title, mode, okContent, closeContent);
            zzMessageShowDD.Dispose();
            zzMessageShowDD = null;
            return DialogResultzzMessageShowDD;
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
        /// <summary>ZzMessageShow_Load 方法。</summary>
        private void ZzMessageShow_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
