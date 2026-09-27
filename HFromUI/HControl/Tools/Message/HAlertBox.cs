using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace HFromUI.HControl.Tools.Message
{
    public partial class HAlertBox : Form
    {
        private SynchronizationContext _uiContext;
        /// <summary>IsCanClose 成员。</summary>
        public bool IsCanClose { set; get; } = true;
        /// <summary>IsShow 成员。</summary>
        public bool IsShow {private set; get; }
        public HAlertBox()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            {
                return;
            }
            time_show.Enabled = IsShow;
            _uiContext = SynchronizationContext.Current;
        }
        /// <summary>默认通道键名。</summary>
        public const string DefaultKey = "default";
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<string, HAlertBox> _boxes = new Dictionary<string, HAlertBox>();

        /// <summary>按键获取共享实例：键不存在则新建并加入字典，键已存在则直接返回。</summary>
        private static HAlertBox GetOrCreate(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = DefaultKey;
            }
            lock (_syncRoot)
            {
                HAlertBox box;
                if (!_boxes.TryGetValue(key, out box))
                {
                    box = new HAlertBox();
                    _boxes[key] = box;
                }
                return box;
            }
        }

        /// <summary>默认通道是否正在显示。</summary>
        public static bool IsShowing
        {
            get { return GetOrCreate(DefaultKey).IsShow; }
        }

        /// <summary>指定键通道是否正在显示（键不存在时先创建再查询）。</summary>
        public static bool GetIsShowing(string key)
        {
            return GetOrCreate(key).IsShow;
        }

        /// <summary>默认通道是否允许关闭。</summary>
        public static bool CanClose
        {
            get { return GetOrCreate(DefaultKey).IsCanClose; }
            set { GetOrCreate(DefaultKey).IsCanClose = value; }
        }

        /// <summary>设置指定键通道是否允许关闭（键不存在时先创建再设置）。</summary>
        public static void SetCanClose(string key, bool value)
        {
            GetOrCreate(key).IsCanClose = value;
        }

        /// <summary>获取指定键通道是否允许关闭（键不存在时先创建再查询）。</summary>
        public static bool GetCanClose(string key)
        {
            return GetOrCreate(key).IsCanClose;
        }

        /// <summary>关闭默认通道的提示框。</summary>
        public static void CloseBox()
        {
            CloseBox(DefaultKey);
        }

        /// <summary>关闭指定键通道的提示框（键不存在时先创建）。</summary>
        public static void CloseBox(string key)
        {
            HAlertBox box = GetOrCreate(key);
            if (box.IsShow)
            {
                box.btn_Close_Click(null, null);
            }
        }

        /// <summary>在默认通道显示提示框（非模态）。</summary>
        public static void Show(string content, string title = "", int mode = 0, string closeContent = "")
        {
            Show(DefaultKey, content, title, mode, closeContent);
        }

        /// <summary>在指定键通道显示提示框：该键已有实例则直接显示，没有则新建后显示（非模态）。</summary>
        public static void Show(string key, string content, string title = "", int mode = 0, string closeContent = "")
        {
            GetOrCreate(key).ShowMessage(content, title, mode, closeContent);
        }

        /// <summary>新建临时实例显示提示框（非模态，不占用共享通道）。</summary>
        public static void ShowNew(string content, string title = "", int mode = 0, string closeContent = "")
        {
            new HAlertBox().ShowMessage(content, title, mode, closeContent);
        }

        /// <summary>新建临时实例模态显示，返回点击结果。</summary>
        public static DialogResult ShowDialog(string content, string title = "", int mode = 0, string closeContent = "")
        {
            HAlertBox box = new HAlertBox();
            DialogResult result = box.ShowMessageDialog(content, title, mode, closeContent);
            box.Dispose();
            return result;
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
        /// <summary>显示提示框（非模态）。</summary>
        public void ShowMessage(string content, string title="",int mode=0,string closeContent="")
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
        /// <summary>模态显示提示框并返回点击结果。</summary>
        public DialogResult ShowMessageDialog(string content, string title = "", int mode = 0,  string closeContent = "")
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
        /// <summary>HAlertBox_Load 方法。</summary>
        private void HAlertBox_Load(object sender, EventArgs e)
        {
            IsShow = true;
        }
    }
}
