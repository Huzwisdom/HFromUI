using System.Drawing;
using System.Windows.Forms;
using HFromUI.HEnum;
using HFromUI.HControl;
using HFromUI.HControl.Tools.Button;
using HFromUI.HControl.Tools.Editors;
using HFromUI.HFrom.Panel;
using HFromUI.HFrom.Tables;


namespace HFromUI.HFrom.HUiKit
{
    using HFromUI.HColor;
    /// <summary>
    /// 深色监控主题（相机监控/工控软件风格）：统一配色 + HFrom 控件一键换肤。
    /// 用法：Form 背景设 WindowBg，卡片用 StyleCard，输入框用 StyleInput，按钮按用途 StyleXxx。
    /// </summary>
    public static class HUiTheme
    {
        // ============ 色板 ============
        /// <summary>窗体最深背景</summary>
        public static readonly Color WindowBg = Color.FromArgb(23, 24, 29);
        /// <summary>面板/标题栏/状态栏背景</summary>
        public static readonly Color PanelBg = Color.FromArgb(30, 32, 39);
        /// <summary>卡片背景</summary>
        public static readonly Color CardBg = Color.FromArgb(38, 41, 50);
        /// <summary>输入框背景</summary>
        public static readonly Color InputBg = Color.FromArgb(22, 24, 30);
        /// <summary>通用边框</summary>
        public static readonly Color Border = Color.FromArgb(58, 63, 74);
        /// <summary>输入框聚焦/悬停边框</summary>
        public static readonly Color BorderHover = Color.FromArgb(45, 127, 249);

        /// <summary>主色（蓝）：连接/主操作</summary>
        public static readonly Color Accent = Color.FromArgb(45, 127, 249);
        /// <summary>AccentHover 成员。</summary>
        public static readonly Color AccentHover = Color.FromArgb(78, 152, 255);
        /// <summary>AccentDown 成员。</summary>
        public static readonly Color AccentDown = Color.FromArgb(28, 99, 200);

        /// <summary>危险色（红）：录像/停止</summary>
        public static readonly Color Danger = Color.FromArgb(229, 72, 77);
        /// <summary>DangerHover 成员。</summary>
        public static readonly Color DangerHover = Color.FromArgb(240, 102, 107);
        /// <summary>DangerDown 成员。</summary>
        public static readonly Color DangerDown = Color.FromArgb(190, 55, 60);

        /// <summary>成功色（绿）</summary>
        public static readonly Color Green = Color.FromArgb(48, 209, 88);
        /// <summary>警告色（黄）</summary>
        public static readonly Color Yellow = Color.FromArgb(255, 179, 0);

        /// <summary>次按钮底色</summary>
        public static readonly Color GhostBg = Color.FromArgb(48, 52, 62);
        /// <summary>GhostHover 成员。</summary>
        public static readonly Color GhostHover = Color.FromArgb(62, 67, 80);
        /// <summary>GhostDown 成员。</summary>
        public static readonly Color GhostDown = Color.FromArgb(38, 41, 50);

        /// <summary>主文字</summary>
        public static readonly Color TextMain = Color.FromArgb(237, 240, 245);
        /// <summary>次要文字（标签/提示）</summary>
        public static readonly Color TextSub = Color.FromArgb(154, 162, 175);
        /// <summary>标题胶囊背景（卡片标题）</summary>
        public static readonly Color TitleCapBg = Color.FromArgb(45, 127, 249);

        /// <summary>FontTitle 成员。</summary>
        public static Font FontTitle = new Font("微软雅黑", 14F, FontStyle.Bold);
        /// <summary>FontUi 成员。</summary>
        public static Font FontUi = new Font("微软雅黑", 9F);
        /// <summary>FontUiBold 成员。</summary>
        public static Font FontUiBold = new Font("微软雅黑", 9F, FontStyle.Bold);
        /// <summary>FontSmall 成员。</summary>
        public static Font FontSmall = new Font("微软雅黑", 8F);

        // ============ 按钮 ============

        /// <summary>统一应用 HPushButton 三态色板（HPushButton 全部矢量自绘，不依赖资源图片）。</summary>
        private static void ApplyButton(HPushButton b, Color back, Color fore, Color hover, Color down)
        {
            b.ButtonStyle = HPushButtonStyle.Custom;
            b.Shape = HPushButtonShape.Round;
            b.Radius = 4;
            b.BorderWidth = 1;
            b.NormalBackColor = back;
            b.NormalBorderColor = back;
            b.NormalForeColor = fore;
            b.HoverBackColor = hover;
            b.HoverBorderColor = hover;
            b.HoverForeColor = fore;
            b.PressedBackColor = down;
            b.PressedBorderColor = down;
            b.PressedForeColor = fore;
        }

        /// <summary>主色按钮（连接/确认等主操作）</summary>
        public static HPushButton StyleAccent(HPushButton b)
        {
            ApplyButton(b, Accent, Color.White, AccentHover, AccentDown);
            b.Font = FontUiBold;
            return b;
        }

        /// <summary>危险按钮（录像/删除）</summary>
        public static HPushButton StyleDanger(HPushButton b)
        {
            ApplyButton(b, Danger, Color.White, DangerHover, DangerDown);
            b.Font = FontUiBold;
            return b;
        }

        /// <summary>次要按钮（灰色幽灵按钮：刷新/浏览/断开）</summary>
        public static HPushButton StyleGhost(HPushButton b)
        {
            ApplyButton(b, GhostBg, TextMain, GhostHover, GhostDown);
            b.NormalBorderColor = Border;
            b.HoverBorderColor = BorderHover;
            b.HoverForeColor = Color.White;
            b.PressedBorderColor = BorderHover;
            b.PressedForeColor = TextMain;
            b.Font = FontUi;
            return b;
        }

        /// <summary>选中态按钮（如分屏切换选中）：主色描边浅底</summary>
        public static HPushButton StyleActive(HPushButton b)
        {
            StyleGhost(b);
            Color activeBack = Color.FromArgb(30, 45, 72);
            Color activeFore = Color.FromArgb(140, 190, 255);
            b.NormalBackColor = activeBack;
            b.NormalBorderColor = Accent;
            b.NormalForeColor = activeFore;
            b.HoverBackColor = activeBack;
            b.HoverBorderColor = Accent;
            b.HoverForeColor = activeFore;
            b.PressedBackColor = activeBack;
            b.PressedBorderColor = Accent;
            b.PressedForeColor = activeFore;
            return b;
        }

        // ============ 容器 ============

        /// <summary>卡片（HGroupBox）：深色卡片底 + 蓝色标题胶囊</summary>
        public static HGroupBox StyleCard(HGroupBox gb)
        {
            gb.Radius = 8;
            gb.TitleRadius = 5;
            gb.BorderWidth = 1;
            gb.BackColor = WindowBg;
            gb.BaseColor = CardBg;
            gb.BorderColor = Border;
            gb.TitleBaseColor = TitleCapBg;
            gb.TitleBorderColor = TitleCapBg;
            gb.ForeColor = Color.White;
            gb.Font = FontUiBold;
            return gb;
        }

        /// <summary>面板（HPanel）</summary>
        public static HPanel StylePanel(HPanel p, Color baseColor, int radius = 4)
        {
            p.Radius = radius;
            p.BorderWidth = 1;
            p.ShowBorder = true;
            p.BackColor = WindowBg;
            p.BaseColor = baseColor;
            p.BorderColor = Border;
            return p;
        }

        // ============ 标签 ============

        /// <summary>标签（Label）</summary>
        public static Label StyleLabel(Label l, bool secondary = false, bool bold = false)
        {
            l.BackColor = Color.Transparent;
            l.ForeColor = secondary ? TextSub : TextMain;
            l.Font = bold ? FontUiBold : FontUi;
            return l;
        }

        // ============ 新增控件主题 ============

        /// <summary>搜索框（HSearchBox 深色风格）</summary>
        public static HSearchBox StyleSearchBox(HSearchBox s)
        {
            s.SearchStyle = HEditStyle.Dark;
            s.IconColor = TextSub;
            return s;
        }

        /// <summary>分页条（HPagination）</summary>
        public static HPagination StylePagination(HPagination p)
        {
            p.BackColor = PanelBg;
            p.BaseColor = PanelBg;
            p.BorderColor = Border;
            p.ShowBorder = false;
            return p;
        }

        /// <summary>增强树（HTreeViewEx）</summary>
        public static HTreeViewEx StyleTreeViewEx(HTreeViewEx t)
        {
            t.BackColor = PanelBg;
            t.ForeColor = TextMain;
            t.LineColor = Border;
            t.Font = FontUi;
            return t;
        }

        /// <summary>多列树（HTreeList）</summary>
        public static HTreeList StyleTreeList(HTreeList t)
        {
            t.BackColor = PanelBg;
            t.HeaderColor = CardBg;
            t.RowColor = PanelBg;
            t.AltRowColor = CardBg;
            t.SelectColor = Accent;
            t.TextColor = TextMain;
            t.LineColor = Border;
            return t;
        }

        /// <summary>分页数据网格（HPagedDataGridView）</summary>
        public static HPagedDataGridView StylePagedGrid(HPagedDataGridView g)
        {
            g.BackColor = WindowBg;
            g.BaseColor = WindowBg;
            g.BorderColor = Border;
            g.ShowBorder = false;
            if (g.SearchBox != null) StyleSearchBox(g.SearchBox);
            if (g.Pagination != null) StylePagination(g.Pagination);
            if (g.Grid != null)
            {
                g.Grid.BackgroundColor = PanelBg;
                g.Grid.DefaultCellStyle.BackColor = PanelBg;
                g.Grid.DefaultCellStyle.ForeColor = TextMain;
                g.Grid.DefaultCellStyle.SelectionBackColor = Accent;
                g.Grid.DefaultCellStyle.SelectionForeColor = Color.White;
                g.Grid.ColumnHeadersDefaultCellStyle.BackColor = CardBg;
                g.Grid.ColumnHeadersDefaultCellStyle.ForeColor = TextMain;
            }
            return g;
        }
    }
}
