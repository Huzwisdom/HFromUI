using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HFromUI.HColor
{
    /// <summary>
    /// 控件库内置图片的矢量绘制器：全部图标用 GDI+ 在 100x100 单位盒中绘制，
    /// 按每个图标的原始像素尺寸输出位图，任意 DPI 不模糊。文件名与原 Properties.Resources 一致。
    /// </summary>
    internal static class HVectorPhoto
    {
        // 常用配色（与原位图一致：黑形、蓝强调、灰态、窗体悬停青、危险红）
        private static readonly Color Black = Color.FromArgb(18, 18, 18);
        private static readonly Color Blue = Color.FromArgb(30, 144, 255);
        private static readonly Color Cyan = Color.FromArgb(0, 176, 240);
        private static readonly Color Gray = Color.FromArgb(96, 96, 96);
        /// <summary>小箭头（down/up/left/right）原图为偏冷的浅灰。</summary>
        private static readonly Color CoolGray = Color.FromArgb(117, 121, 125);
        private static readonly Color LineGray = Color.FromArgb(210, 210, 210);
        private static readonly Color White = Color.White;
        private static readonly Color Red = Color.FromArgb(237, 40, 48);
        private static readonly Color Yellow = Color.FromArgb(246, 201, 21);
        private static readonly Color Orange = Color.FromArgb(245, 166, 35);
        private static readonly Color Green = Color.FromArgb(43, 212, 61);

        // ===== HCoordinate 工具栏语义色板（煤炭色为主 + 橙色小面积点缀 + 媒体语义色） =====
        /// <summary>煤炭色：图标主体 / 描边的主色，沉稳近黑。</summary>
        private static readonly Color Ink = Color.FromArgb(44, 47, 52);
        /// <summary>点缀橙：锚点 / 激活态 / 动作箭头等小面积强调，绝不大面积铺底。</summary>
        private static readonly Color Pri = Color.FromArgb(240, 122, 26);
        /// <summary>播放绿（同 HChart 菜单）。</summary>
        private static readonly Color PlayGreen = Color.FromArgb(60, 170, 90);
        /// <summary>危险红：删除 / 停止（同 HChart 菜单）。</summary>
        private static readonly Color Danger = Color.FromArgb(205, 80, 80);
        /// <summary>危险红的浅底填充。</summary>
        private static readonly Color DangerSoft = Color.FromArgb(30, 205, 80, 80);
        /// <summary>紫色备用常量（当前无引用）。</summary>
        private static readonly Color Purple = Color.FromArgb(149, 108, 220);
        /// <summary>煤炭色的浅底填充（后置窗口、次级内容线）。</summary>
        private static readonly Color InkSoft = Color.FromArgb(95, 44, 47, 52);

        /// <summary>按名称渲染位图（先取原始尺寸，再在 100 单位坐标系中矢量绘制）。</summary>
        public static Bitmap Render(string name)
        {
            Size s = NativeSize(name);
            Bitmap bmp = new Bitmap(s.Width, s.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.ScaleTransform(s.Width / 100f, s.Height / 100f);
                Paint(new IconG(g), name);
            }
            return bmp;
        }

        /// <summary>每个图标的原始输出尺寸（与历史 PNG 一致，保证界面布局不位移）。</summary>
        private static Size NativeSize(string name)
        {
            switch (name)
            {
                case "down": case "downBlue": case "up": case "upBlue":
                    return new Size(13, 10);
                case "left": case "leftBlue": case "right": case "rightBlue":
                    return new Size(10, 13);
                case "CheckNormal": case "CheckSelect":
                case "RadioChecked": case "RadioUnCheck":
                    return new Size(15, 15);
                case "FormClose": case "FormCloseIn": case "FormMax": case "FormMaxIn":
                case "FormMin": case "FormMinIn": case "FormRestore": case "FormRestoreIn":
                    return new Size(25, 25);
                case "Date":
                    return new Size(26, 26);
                case "backward": case "forward": case "Max": case "Open":
                case "pause": case "play": case "Restore": case "stop":
                    return new Size(30, 30);
                case "ok": case "save":
                    return new Size(32, 32);
                case "newBlue": case "newGray":
                    return new Size(24, 24);
                case "music": case "random":
                    return new Size(100, 100);
                case "appIcon":
                    return new Size(64, 64);
                default:
                    return new Size(64, 64);
            }
        }

        /// <summary>按名称分发到具体图标的绘制方法。</summary>
        private static void Paint(IconG g, string name)
        {
            switch (name)
            {
                // ===== 小箭头：灰态 / 蓝悬停态（down/up 13x10，left/right 10x13，笔画贴边） =====
                case "up": SmallChevron(g, Dir.Up, CoolGray); break;
                case "upBlue": SmallChevron(g, Dir.Up, Blue); break;
                case "down": SmallChevron(g, Dir.Down, CoolGray); break;
                case "downBlue": SmallChevron(g, Dir.Down, Blue); break;
                case "left": SmallChevron(g, Dir.Left, CoolGray); break;
                case "leftBlue": SmallChevron(g, Dir.Left, Blue); break;
                case "right": SmallChevron(g, Dir.Right, CoolGray); break;
                case "rightBlue": SmallChevron(g, Dir.Right, Blue); break;

                // ===== 64 箭头族 =====
                case "arrow_up": Chevron(g, Dir.Up, Ink, 7); break;
                case "arrow_down": Chevron(g, Dir.Down, Ink, 7); break;
                case "arrow_left": Chevron(g, Dir.Left, Black, 7); break;
                case "arrow_right": Chevron(g, Dir.Right, Black, 7); break;
                case "arrow_double_left": DoubleChevron(g, Dir.Left, Ink); break;
                case "arrow_double_right": DoubleChevron(g, Dir.Right, Ink); break;
                case "collapse_up": DoubleChevron(g, Dir.Up, Black); break;
                case "expand_down": DoubleChevron(g, Dir.Down, Black); break;
                case "arrow_up_triple": CircleArrow(g, Dir.Up, Ink, Pri); break;
                case "arrow_down_5": CircleArrow(g, Dir.Down, Ink, Pri); break;
                case "arrow_left_double": CircleArrow(g, Dir.Left, Ink, Pri); break;
                case "arrow_right_double": CircleArrow(g, Dir.Right, Ink, Pri); break;

                // ===== 窗体标题栏按钮 25 =====
                case "FormClose": FormClose(g, White); break;
                case "FormCloseIn": FormClose(g, Red); break;
                case "FormMax": g.RRect(24, 24, 52, 52, 7, White, 8); break;
                case "FormMaxIn": g.RRect(24, 24, 52, 52, 7, Cyan, 8); break;
                case "FormMin": g.Line(26, 50, 74, 50, White, 9); break;
                case "FormMinIn": g.Line(26, 50, 74, 50, Cyan, 9); break;
                case "FormRestore": FormRestore(g, White); break;
                case "FormRestoreIn": FormRestore(g, Cyan); break;

                // ===== 30 媒体按钮（白色，深色底上使用） =====
                case "play": g.FillPoly(White, 30, 20, 82, 50, 30, 80); break;
                case "pause": g.FillRRect(28, 22, 17, 56, 6, White); g.FillRRect(55, 22, 17, 56, 6, White); break;
                case "stop": g.FillRRect(24, 24, 52, 52, 6, White); break;
                case "backward":
                    g.FillRect(12, 24, 10, 52, White);
                    g.FillPoly(White, 30, 50, 80, 16, 80, 84);
                    break;
                case "forward":
                    g.FillPoly(White, 70, 50, 20, 16, 20, 84);
                    g.FillRect(78, 24, 10, 52, White);
                    break;
                case "Max": Corners(g, White, 9, 14, 20); break;
                case "Restore":
                    g.RRect(38, 20, 40, 40, 5, White, 6);
                    g.RRect(22, 38, 40, 40, 5, White, 6);
                    break;
                case "Open":
                    // 白色打开的文件夹
                    g.FillPoly(White, 6, 34, 26, 34, 34, 44, 90, 44, 84, 70, 10, 70);
                    g.FillPoly(White, 34, 44, 90, 44, 84, 58, 30, 58);
                    break;

                // ===== 勾选 / 单选 15 =====
                case "CheckNormal": g.RRect(9, 9, 82, 82, 16, LineGray, 10); break;
                case "CheckSelect":
                    g.FillRRect(6, 6, 88, 88, 18, Blue);
                    g.Lines(White, 12, 26, 52, 44, 68, 74, 30);
                    break;
                case "RadioUnCheck": g.Circle(50, 50, 84, LineGray, 10); break;
                case "RadioChecked":
                    g.FillCircle(50, 50, 80, Blue);
                    g.FillCircle(50, 50, 56, Color.FromArgb(150, 255, 255, 255));
                    g.FillCircle(50, 50, 40, Blue);
                    break;

                // ===== 日期 / 保存 =====
                case "Date": DateIcon(g); break;
                case "save": SaveBlue(g); break;
                case "save_as": SaveAs(g); break;
                case "save_chart": SaveOutline(g, false); break;

                // ===== 文件操作 =====
                case "file_new": Plus(g, Black, 11); break;
                case "file_new_alt": g.Circle(50, 50, 82, Black, 7); Plus(g, Black, 9); break;
                case "newBlue":
                    g.Line(50, 22, 50, 78, Blue, 13);
                    g.Line(22, 50, 78, 50, Blue, 13);
                    break;
                case "newGray":
                    g.Line(50, 22, 50, 78, Gray, 13);
                    g.Line(22, 50, 78, 50, Gray, 13);
                    break;
                case "file_open": FolderOpen(g); break;
                case "tool_file": ToolFile(g); break;
                case "tool_upload": Upload(g); break;
                case "tool_delete": Trash(g); break;
                case "op_reset": Reset(g); break;
                case "fit_window": Corners(g, Ink, 7, 10, 22); break;

                // ===== 对齐：细对齐线 + 实心短条 + 空心长条 =====
                case "align_left":
                    g.Line(20, 12, 20, 88, Black, 6);
                    g.FillRRect(30, 20, 44, 22, 4, Black);
                    g.RRect(30, 54, 52, 20, 4, Black, 6);
                    break;
                case "align_right":
                    g.Line(80, 12, 80, 88, Black, 6);
                    g.FillRRect(26, 20, 44, 22, 4, Black);
                    g.RRect(18, 54, 52, 20, 4, Black, 6);
                    break;
                case "align_top":
                    g.Line(12, 20, 88, 20, Black, 6);
                    g.RRect(20, 30, 26, 46, 4, Black, 6);
                    g.FillRRect(52, 30, 26, 26, 4, Black);
                    break;
                case "align_bottom":
                    g.Line(12, 80, 88, 80, Black, 6);
                    g.RRect(16, 20, 26, 52, 4, Black, 6);
                    g.FillRRect(48, 46, 26, 26, 4, Black);
                    break;

                // ===== CAD 绘图工具（煤炭色笔画 + 橙色锚点） =====
                case "draw_point":
                    // 炭环 + 中心橙点
                    g.Circle(50, 50, 34, Ink, 8);
                    g.FillCircle(50, 50, 10, Pri);
                    break;
                case "draw_line_straight":
                    g.Line(16, 84, 84, 16, Ink, 6);
                    g.FillCircle(16, 84, 11, Pri);
                    g.FillCircle(84, 16, 11, Pri);
                    break;
                case "draw_line":
                    DrawLinePen(g);
                    break;
                case "draw_circle_arc":
                    // 圆心在右下的 90° 短弧 + 三个橙点（起点、终点、圆心点）
                    g.Arc(14, 12, 144, 144, 180, 90, Ink, 7);
                    g.FillCircle(14, 84, 11, Pri);
                    g.FillCircle(86, 14, 11, Pri);
                    g.FillCircle(86, 84, 11, Pri);
                    break;
                case "arc_3pt":
                    g.Bezier(Ink, 7, 16, 82, 13, 42, 32, 22, 84, 18);
                    g.FillCircle(16, 82, 10, Pri);
                    g.FillCircle(84, 18, 10, Pri);
                    g.FillCircle(28, 40, 10, Pri);
                    break;
                case "dim_chain":
                    g.Lines(Ink, 6, 14, 82, 35, 32, 58, 62, 86, 16);
                    g.FillCircle(14, 82, 10, Pri);
                    g.FillCircle(35, 32, 10, Pri);
                    g.FillCircle(58, 62, 10, Pri);
                    g.FillCircle(86, 16, 10, Pri);
                    break;
                case "grid_lines":
                    // 煤炭色方框 + 炭色十字中线伸出框外
                    g.RRect(23, 23, 54, 54, 5, Ink, 7);
                    g.Line(6, 50, 94, 50, Ink, 7);
                    g.Line(50, 6, 50, 94, Ink, 7);
                    break;
                case "decimal_place_review":
                    // 小数位：煤炭色圆角牌 + 白色 0.0 数字
                    g.FillRRect(8, 20, 84, 60, 14, Ink);
                    g.Text("0.0", 50, 62, 30, White);
                    break;
                case "text_input":
                    // 编辑框 + 右上角斜放钢笔
                    g.RRect(8, 26, 58, 58, 10, Black, 9);
                    g.Line(36, 14, 78, 56, Black, 12);
                    g.FillPoly(Black, 24, 62, 38, 48, 50, 60);
                    break;
                case "cad":
                    g.Text("A", 38, 42, 52, Black);
                    g.Line(12, 80, 80, 80, Black, 9);
                    g.Line(80, 12, 80, 88, Black, 9);
                    break;
                case "DXF":
                    // 单张折角图纸（白底炭边）+ 炭色 CAD 折线橙端点 + 炭色 DXF 字
                    using (GraphicsPath p = new GraphicsPath())
                    {
                        p.AddArc(20, 8, 12, 12, 180, 90);
                        p.AddLine(26, 8, 56, 8);
                        p.AddLine(56, 8, 80, 32);
                        p.AddLine(80, 32, 80, 82);
                        p.AddArc(68, 76, 12, 12, 0, 90);
                        p.AddLine(74, 88, 26, 88);
                        p.AddArc(20, 76, 12, 12, 90, 90);
                        p.AddLine(20, 82, 20, 14);
                        p.CloseFigure();
                        g.FillPath(White, p);
                        g.DrawPath(Ink, 6, p);
                    }
                    g.Line(56, 8, 56, 32, Ink, 6);
                    g.Line(56, 32, 80, 32, Ink, 6);
                    g.Lines(Ink, 5, 30, 48, 42, 40, 56, 50, 70, 42);
                    g.FillCircle(30, 48, 4, Pri);
                    g.FillCircle(70, 42, 4, Pri);
                    g.Text("DXF", 50, 75, 17, Ink);
                    break;
                case "Gcode":
                    GCodeFile(g);
                    break;
                case "table_format":
                    // 煤炭色表格板 + 白色表头分隔、列线与行线
                    g.FillRRect(8, 22, 84, 56, 8, Ink);
                    g.Line(8, 40, 92, 40, White, 6);
                    g.Line(36, 22, 36, 40, White, 5);
                    g.Line(64, 22, 64, 40, White, 5);
                    g.Line(8, 54, 92, 54, Color.FromArgb(150, 255, 255, 255), 5);
                    g.Line(8, 67, 92, 67, Color.FromArgb(150, 255, 255, 255), 5);
                    break;
                case "tool_decimal": g.Text(".0", 50, 54, 52, Black); break;
                case "tool_database":
                    g.Ellipse(12, 12, 76, 22, Black, 6);
                    g.Ellipse(12, 38, 76, 22, Black, 6);
                    g.Ellipse(12, 64, 76, 22, Black, 6);
                    g.Line(12, 23, 12, 75, Black, 6);
                    g.Line(88, 23, 88, 75, Black, 6);
                    break;
                case "tool_settings": Gear(g); break;

                // ===== 圆媒体（语义色实心圆 + 白色符号） =====
                case "tool_start":
                    g.FillCircle(50, 50, 82, PlayGreen);
                    g.FillPoly(White, 37, 31, 70, 50, 37, 69);
                    break;
                case "tool_stop":
                    g.FillCircle(50, 50, 82, Danger);
                    g.FillRRect(37, 37, 26, 26, 4, White);
                    break;
                case "tool_pause":
                    // 左下开口圆环 + 两根暂停竖条
                    g.Arc(10, 10, 80, 80, 250, 280, Black, 9);
                    g.Line(41, 32, 41, 68, Black, 9);
                    g.Line(59, 32, 59, 68, Black, 9);
                    break;
                case "tool_pause_circle":
                    // 橙色实心圆 + 两根白色暂停竖条
                    g.FillCircle(50, 50, 82, Orange);
                    g.FillRRect(40, 34, 7, 32, 3, White);
                    g.FillRRect(53, 34, 7, 32, 3, White);
                    break;
                case "tool_zero_return":
                    g.Circle(50, 50, 80, Black, 8);
                    g.Text("0", 50, 54, 54, Black);
                    break;

                // ===== 图层 / 群组 / 视图 =====
                case "view_layers":
                    // 后层炭色窗口 + 前层炭标题白窗（错位重叠，前窗白底挡住后框穿入部分）
                    g.RRect(30, 8, 54, 44, 8, Ink, 7);
                    g.FillRRect(12, 30, 54, 48, 8, White);
                    g.FillRRect(12, 30, 54, 13, 8, Ink);
                    g.FillRect(12, 37, 54, 6, Ink);
                    g.RRect(12, 30, 54, 48, 8, Ink, 7);
                    g.Line(22, 58, 56, 58, InkSoft, 5);
                    g.Line(22, 68, 46, 68, InkSoft, 5);
                    break;
                case "group_ungroup":
                    // 两个错位重叠的圆角方框：后框、前框均为煤炭色
                    g.RRect(36, 8, 48, 42, 8, Ink, 7);
                    g.FillRRect(16, 34, 40, 40, 6, White);
                    g.RRect(12, 30, 48, 48, 8, Ink, 7);
                    break;
                case "group_create":
                    PeopleGroup(g);
                    break;
                case "layer_bring_forward":
                    // 实心块在上、线框在下右
                    g.FillRRect(10, 10, 42, 42, 8, Black);
                    g.RRect(38, 32, 50, 50, 8, Black, 8);
                    break;
                case "layer_bring_front":
                    // 后线框 + 前实心块 + 右下小拉手
                    g.RRect(8, 10, 40, 40, 8, Black, 8);
                    g.FillRRect(24, 28, 40, 40, 8, Black);
                    g.RRect(56, 58, 24, 20, 6, Black, 8);
                    break;
                case "layer_bring_to_front":
                    // 后窗口炭色线框 + 前煤炭色实心窗口（带白色内容线）
                    g.RRect(34, 8, 54, 42, 7, Ink, 7);
                    g.FillRRect(8, 30, 50, 56, 7, Ink);
                    g.Line(18, 46, 50, 46, Color.FromArgb(150, 255, 255, 255), 5);
                    g.Line(18, 58, 42, 58, Color.FromArgb(150, 255, 255, 255), 5);
                    g.Line(18, 70, 50, 70, Color.FromArgb(150, 255, 255, 255), 5);
                    break;
                case "layer_send_backward":
                    // 浅炭实心窗口在后、白窗炭框在前（前窗白底遮挡交叉区）
                    g.FillRRect(8, 32, 48, 54, 7, InkSoft);
                    g.FillRRect(34, 8, 54, 42, 7, White);
                    g.FillRRect(34, 8, 54, 13, 7, Ink);
                    g.FillRect(34, 15, 54, 6, Ink);
                    g.RRect(34, 8, 54, 42, 7, Ink, 7);
                    break;
                case "view_dimension":
                    // 外窗口框 + 右下实心小窗 + 指向左上的箭头
                    g.RRect(8, 8, 68, 68, 6, Black, 8);
                    g.FillRRect(52, 54, 26, 24, 4, Black);
                    g.Line(42, 42, 20, 20, Black, 8);
                    g.Lines(Black, 8, 20, 20, 20, 34, 34, 20);
                    break;
                case "view_preview":
                    // 四角取景框 + 居中的眼睛（墨灰眼廓 + 品牌橙瞳孔 + 白色高光）
                    Corners(g, Ink, 7, 10, 22);
                    g.Bezier(Ink, 7, 18, 50, 32, 28, 68, 28, 82, 50);
                    g.Bezier(Ink, 7, 18, 50, 32, 72, 68, 72, 82, 50);
                    g.FillCircle(50, 50, 15, Pri);
                    g.FillCircle(55, 45, 5, White);
                    break;
                case "zoom_actual":
                    // 倾斜的小叶片：外轮廓 + 中脉
                    g.Bezier(Black, 6, 34, 28, 46, 22, 66, 44, 74, 72);
                    g.Bezier(Black, 6, 34, 28, 30, 50, 54, 74, 74, 72);
                    g.Bezier(Black, 5, 38, 32, 46, 42, 58, 58, 70, 66);
                    break;
                case "pos_center":
                    // 煤炭色定位针：圆头 + 尖尾 + 白色针孔
                    g.FillCircle(50, 39, 48, Ink);
                    g.FillPoly(Ink, 36, 56, 64, 56, 50, 86);
                    g.FillCircle(50, 37, 16, White);
                    break;

                // ===== 鼠标光标 =====
                case "cursor_arrow":
                    // 粗胖经典箭头：尖在上、左沿竖直、右下斜尾、右翘（墨灰色）
                    g.FillPoly(Ink, 24, 10, 22, 60, 38, 52, 55, 88, 64, 83, 50, 47, 74, 55);
                    break;
                case "cursor_arrow_active":
                    // 同形放大加粗，激活态用品牌橙
                    g.FillPoly(Pri, 18, 4, 16, 66, 36, 57, 56, 94, 67, 88, 48, 46, 78, 57);
                    break;
                case "cursor_select":
                    // 炭色十字准星（四条粗短臂留中心孔）+ 橙色中心点 + 炭箭头尖对中心
                    g.Line(8, 30, 26, 30, Ink, 8);
                    g.Line(34, 30, 56, 30, Ink, 8);
                    g.Line(30, 8, 30, 26, Ink, 8);
                    g.Line(30, 34, 30, 56, Ink, 8);
                    g.FillCircle(30, 30, 5, Pri);
                    g.FillPoly(Ink, 30, 30, 29, 68, 41, 62, 53, 89, 60, 85, 50, 58, 68, 64);
                    break;

                // ===== 信息 / 提示 =====
                case "info_tip_1": TipCircle(g, Yellow, false, "!"); break;
                case "notify_outline": TipCircle(g, Yellow, false, "?"); break;
                case "info_tip_2":
                    // 橙色圆 + 白色警告三角 + 橙色叹号
                    g.FillCircle(50, 50, 88, Orange);
                    g.FillPoly(White, 50, 20, 81, 74, 19, 74);
                    g.Line(50, 38, 50, 58, Orange, 8);
                    g.FillCircle(50, 67, 8, Orange);
                    break;
                case "notify_solid": TipCircle(g, Red, true, "!"); break;
                case "info_detail_1":
                    // 信封：深 V 封口
                    g.RRect(8, 16, 84, 68, 10, Green, 8);
                    g.Lines(Green, 8, 12, 24, 50, 52, 88, 24);
                    break;
                case "info_detail_2":
                    // 信封：浅 V 封口
                    g.RRect(8, 20, 84, 60, 10, Green, 8);
                    g.Lines(Green, 8, 12, 28, 50, 44, 88, 28);
                    break;
                case "bug_fill": Bug(g); break;
                case "ok":
                    g.Circle(50, 50, 84, Blue, 7);
                    g.Lines(Blue, 9, 28, 52, 44, 68, 74, 33);
                    break;

                // ===== 杂项 =====
                case "music": Music(g); break;
                case "random": Random(g); break;
                case "loading":
                    g.Circle(50, 50, 80, LineGray, 9);
                    g.Arc(10, 10, 80, 80, -60, 260, Blue, 9);
                    break;
                case "appIcon": Panda(g); break;
                default:
                    // 未登记名称：画占位方框，保证调用方永远拿得到图
                    g.RRect(8, 8, 84, 84, 8, Color.Silver, 5);
                    g.Lines(Color.Silver, 6, 28, 28, 72, 72, 28, 72);
                    break;
            }
        }

        // ===== 复合图标 =====

        private enum Dir { Left, Right, Up, Down }

        /// <summary>V 形箭头。</summary>
        private static void Chevron(IconG g, Dir d, Color c, float w)
        {
            switch (d)
            {
                case Dir.Down: g.Lines(c, w, 16, 34, 50, 68, 84, 34); break;
                case Dir.Up: g.Lines(c, w, 16, 66, 50, 32, 84, 66); break;
                case Dir.Left: g.Lines(c, w, 66, 16, 32, 50, 66, 84); break;
                case Dir.Right: g.Lines(c, w, 34, 16, 68, 50, 34, 84); break;
            }
        }

        /// <summary>小箭头（13x10 / 10x13 非方盒）：细笔画、撑满画布，与原 down/up/left/right 位图一致。</summary>
        private static void SmallChevron(IconG g, Dir d, Color c)
        {
            const float w = 13;
            switch (d)
            {
                case Dir.Down: g.Lines(c, w, 8, 30, 50, 85, 92, 30); break;
                case Dir.Up: g.Lines(c, w, 8, 70, 50, 15, 92, 70); break;
                case Dir.Left: g.Lines(c, w, 85, 8, 15, 50, 85, 92); break;
                case Dir.Right: g.Lines(c, w, 15, 8, 85, 50, 15, 92); break;
            }
        }

        /// <summary>双 V 形箭头。</summary>
        private static void DoubleChevron(IconG g, Dir d, Color c)
        {
            switch (d)
            {
                case Dir.Left:
                    g.Lines(c, 6, 62, 18, 36, 50, 62, 82);
                    g.Lines(c, 6, 42, 18, 16, 50, 42, 82);
                    break;
                case Dir.Right:
                    g.Lines(c, 6, 38, 18, 64, 50, 38, 82);
                    g.Lines(c, 6, 58, 18, 84, 50, 58, 82);
                    break;
                case Dir.Up:
                    g.Lines(c, 6, 24, 62, 50, 36, 76, 62);
                    g.Lines(c, 6, 24, 44, 50, 18, 76, 44);
                    break;
                case Dir.Down:
                    g.Lines(c, 6, 24, 38, 50, 64, 76, 38);
                    g.Lines(c, 6, 24, 20, 50, 46, 76, 20);
                    break;
            }
        }

        /// <summary>圆圈内方向箭头（墨灰细环 + 品牌橙箭头）。</summary>
        private static void CircleArrow(IconG g, Dir d, Color ring, Color ar)
        {
            g.Circle(50, 50, 76, ring, 6);
            switch (d)
            {
                case Dir.Left:
                    g.Line(66, 50, 38, 50, ar, 6);
                    g.Lines(ar, 6, 50, 36, 38, 50, 50, 64);
                    break;
                case Dir.Right:
                    g.Line(34, 50, 62, 50, ar, 6);
                    g.Lines(ar, 6, 50, 36, 62, 50, 50, 64);
                    break;
                case Dir.Up:
                    g.Line(50, 66, 50, 40, ar, 6);
                    g.Lines(ar, 6, 34, 48, 50, 32, 66, 48);
                    break;
                case Dir.Down:
                    g.Line(50, 34, 50, 60, ar, 6);
                    g.Lines(ar, 6, 34, 52, 50, 68, 66, 52);
                    break;
            }
        }

        /// <summary>四角括号（最大化/适配窗口）：i 为距边留白，len 为每臂长度。</summary>
        private static void Corners(IconG g, Color c, float w, float i, float len)
        {
            float e = 100 - i;
            g.Lines(c, w, i, i + len, i, i, i + len, i);
            g.Lines(c, w, e - len, i, e, i, e, i + len);
            g.Lines(c, w, e, e - len, e, e, e - len, e);
            g.Lines(c, w, i + len, e, i, e, i, e - len);
        }

        /// <summary>加号。</summary>
        private static void Plus(IconG g, Color c, float w)
        {
            g.Line(50, 18, 50, 82, c, w);
            g.Line(18, 50, 82, 50, c, w);
        }

        /// <summary>窗体关闭 X。</summary>
        private static void FormClose(IconG g, Color c)
        {
            g.Line(25, 25, 75, 75, c, 10);
            g.Line(75, 25, 25, 75, c, 10);
        }

        /// <summary>窗体还原：两个错位的圆角方框。</summary>
        private static void FormRestore(IconG g, Color c)
        {
            g.RRect(40, 18, 38, 38, 4, c, 7);
            g.RRect(22, 38, 38, 38, 4, c, 7);
        }

        /// <summary>蓝色日历（Date 26px）：白色本体 + 蓝色装订耳 + 3 行蓝色圆点。</summary>
        private static void DateIcon(IconG g)
        {
            g.FillRRect(12, 18, 76, 68, 8, White);
            g.RRect(12, 18, 76, 68, 8, Blue, 7);
            g.Line(12, 38, 88, 38, Blue, 7);
            g.Line(28, 8, 28, 28, Blue, 8);
            g.Line(72, 8, 72, 28, Blue, 8);
            // 3 行 × 3 列蓝色短点
            float[] xs = { 28f, 50f, 72f };
            float[] ys = { 50f, 60f, 70f };
            foreach (float y in ys)
                foreach (float x in xs)
                    g.FillRRect(x - 5, y - 2, 10, 4, 2, Blue);
        }

        /// <summary>蓝色实心软盘（save 32px）。</summary>
        private static void SaveBlue(IconG g)
        {
            g.FillRRect(13, 7, 74, 86, 9, Blue);
            g.FillRect(30, 7, 40, 32, White);
            g.FillRect(38, 27, 24, 12, Blue);
            g.FillRRect(26, 57, 48, 25, 4, White);
        }

        /// <summary>保存：煤炭色实心软盘（白色快门槽 + 白色标签），参考 HChart 菜单软盘造型。</summary>
        private static void SaveAs(IconG g)
        {
            g.FillRRect(13, 8, 74, 84, 9, Ink);
            g.FillRect(30, 8, 40, 30, White);          // 顶部快门凹槽
            g.FillRect(39, 26, 22, 12, Ink);           // 快门孔
            g.FillRRect(25, 54, 50, 28, 5, White);     // 下方标签
            g.Line(34, 67, 66, 67, Color.FromArgb(130, 30, 33, 38), 5);
            g.Line(34, 75, 56, 75, Color.FromArgb(130, 30, 33, 38), 5);
        }

        /// <summary>线性软盘（64px）：save_as 只有外框 + 顶部标签块与快门槽，save_chart 标签下方加横线。</summary>
        private static void SaveOutline(IconG g, bool labelBox)
        {
            g.RRect(14, 9, 72, 83, 9, Black, 7);
            g.Rect(31, 9, 38, 26, Black, 7);
            if (labelBox)
                g.FillRRect(46, 15, 8, 12, 3, Black);
            else
                g.Line(24, 62, 76, 62, Black, 7);
        }

        /// <summary>白色文件夹（煤炭色描边）+ 右上角橙色回环箭头（打开动作点缀）。</summary>
        private static void FolderOpen(IconG g)
        {
            // 经典文件夹剪影：后片带标签台阶、前片袋身，先填白再描炭边
            g.FillPoly(White, 6, 30, 26, 30, 34, 42, 94, 42, 88, 88, 8, 88);
            g.FillRRect(6, 42, 82, 42, 4, White);
            g.Poly(Ink, 6, 6, 30, 26, 30, 34, 42, 94, 42, 88, 88, 8, 88);
            g.RRect(6, 42, 82, 42, 4, Ink, 6);
            // 从文件夹右沿绕顶部大半圆、箭头在右上下指（细线弱化为辅标记）
            g.Arc(48, 4, 38, 38, 220, 130, Pri, 6);
            g.FillPoly(Pri, 72, 24, 88, 23, 83, 36);
        }

        /// <summary>两层错位的文件夹线稿。</summary>
        private static void ToolFile(IconG g)
        {
            g.Poly(Black, 7, 36, 16, 52, 16, 60, 26, 86, 26, 86, 64, 44, 64, 36, 56);
            g.Poly(Black, 7, 10, 30, 28, 30, 36, 40, 78, 40, 78, 82, 14, 82, 14, 40);
        }

        /// <summary>云上传：煤炭色饱满云轮廓 + 云中央橙色向上箭头（动作点缀）。</summary>
        private static void Upload(IconG g)
        {
            using (GraphicsPath cloud = new GraphicsPath())
            {
                // 顶弧（70,38 起止）→ 右弧 → 底部双瓣 → 左弧闭合，各弧端点相接
                cloud.AddArc(30, 18, 40, 40, 180, 180);
                cloud.AddArc(56, 38, 28, 28, 270, 120);
                cloud.AddBezier(82, 59, 78, 74, 62, 74, 52, 66);
                cloud.AddBezier(48, 66, 38, 74, 22, 74, 18, 59);
                cloud.AddArc(16, 38, 28, 28, 210, 60);
                cloud.CloseFigure();
                g.DrawPath(Ink, 7, cloud);
            }
            g.Line(50, 70, 50, 40, Pri, 8);
            g.Lines(Pri, 8, 39, 51, 50, 39, 61, 51);
        }

        /// <summary>垃圾桶（危险红）：盖把 + 横贯盖沿 + 浅红底圆角桶身 + 两条竖棱。</summary>
        private static void Trash(IconG g)
        {
            g.FillRRect(40, 13, 20, 7, 3, Danger);
            g.FillRRect(10, 23, 80, 8, 3, Danger);
            g.FillRRect(21, 33, 58, 53, 5, DangerSoft);
            g.RRect(21, 33, 58, 53, 5, Danger, 7);
            g.Line(38, 42, 38, 76, Danger, 6);
            g.Line(62, 42, 62, 76, Danger, 6);
        }

        /// <summary>复位：近整圆的开口圆弧 + 末端单向箭头（左下留口）。</summary>
        private static void Reset(IconG g)
        {
            g.Arc(16, 16, 68, 68, 120, 282, Black, 11);
            // 弧线终点在右上约 42°，箭头沿顺时针切线
            g.FillPoly(Black, 82, 22, 64, 14, 70, 34);
        }

        /// <summary>齿轮（墨灰 8 齿 + 细圆环身 + 品牌橙轴孔）。</summary>
        private static void Gear(IconG g)
        {
            for (int i = 0; i < 8; i++)
            {
                double a = i * Math.PI / 4.0;
                GraphicsState st = g.Save();
                g.Translate((float)(50 + Math.Cos(a) * 32), (float)(50 + Math.Sin(a) * 32));
                g.Rotate((float)(a * 180.0 / Math.PI + 90));
                g.FillRRect(-6f, -7f, 12, 14, 2, Ink);
                g.Restore(st);
            }
            g.Circle(50, 50, 60, Ink, 7);
            g.FillCircle(50, 50, 22, Pri);
            g.FillCircle(50, 50, 9, White);
        }

        /// <summary>提示圆圈：fill=true 实心白字，false 描边深色字。</summary>
        private static void TipCircle(IconG g, Color c, bool fill, string mark)
        {
            if (fill)
            {
                g.FillCircle(50, 50, 86, c);
                g.Line(50, 26, 50, 56, White, 12);
                g.FillCircle(50, 71, 11, White);
            }
            else
            {
                g.Circle(50, 50, 82, c, 7);
                if (mark == "?")
                {
                    g.Arc(33, 24, 34, 26, 110, 260, c, 9);
                    g.FillCircle(50, 72, 10, c);
                }
                else
                {
                    g.Line(50, 26, 50, 57, c, 9);
                    g.FillCircle(50, 71, 10, c);
                }
            }
        }

        /// <summary>红色瓢虫（调试）：半圆头 + 盾牌形壳 + 中央白缝 + 钩端触角与三条直折腿。</summary>
        private static void Bug(IconG g)
        {
            // 触角（直立粗茎 + 顶端向内小钩）
            g.Bezier(Red, 8, 42, 21, 43, 12, 36, 8, 31, 12);
            g.Bezier(Red, 8, 58, 21, 57, 12, 64, 8, 69, 12);
            // 盾牌形壳（直边、圆底）+ 半圆头叠在壳顶
            g.FillRRect(24, 32, 52, 58, 16, Red);
            g.FillEllipse(33, 18, 34, 24, Red);
            // 左侧三条直折腿（上钩、中平、下钩）
            g.Line(27, 40, 10, 36, Red, 9);
            g.Line(10, 36, 8, 27, Red, 9);
            g.Line(24, 55, 5, 55, Red, 9);
            g.Line(28, 70, 13, 80, Red, 9);
            g.Line(13, 80, 12, 89, Red, 9);
            // 右侧镜像
            g.Line(73, 40, 90, 36, Red, 9);
            g.Line(90, 36, 92, 27, Red, 9);
            g.Line(76, 55, 95, 55, Red, 9);
            g.Line(72, 70, 87, 80, Red, 9);
            g.Line(87, 80, 88, 89, Red, 9);
            // 甲壳中缝（从头到底一条白）
            g.Line(50, 42, 50, 88, White, 7);
        }

        /// <summary>画线工具：单支斜放工程铅笔（参考 HChart 菜单 DrawPencil）——煤炭笔杆、橙色金属箍、白木段炭芯。</summary>
        private static void DrawLinePen(IconG g)
        {
            // 45° 铅笔，笔尖朝左下 (16,88)，笔杆在右上
            g.FillPoly(Ink, 27, 65, 63, 28, 78, 43, 42, 80);    // 煤炭色笔杆（实心）
            g.Line(36, 72, 70, 38, Color.FromArgb(120, 255, 255, 255), 4); // 笔杆高光线
            g.Line(63, 28, 78, 43, Pri, 9);                     // 橙色金属箍
            g.FillPoly(White, 22, 77, 27, 82, 42, 80, 27, 65);  // 白色木质段
            g.Poly(Ink, 6, 22, 77, 27, 82, 42, 80, 27, 65);
            g.FillPoly(Ink, 16, 88, 22, 77, 27, 82);            // 铅芯
        }

        /// <summary>群组：后一人 + 前一人，均为煤炭色（前圆内部白底擦掉后弧穿入段）。</summary>
        private static void PeopleGroup(IconG g)
        {
            g.Arc(31, 21, 20, 20, 16, 313, Ink, 7);
            g.Arc(16, 58, 36, 44, 180, 140, Ink, 7);
            g.FillCircle(61, 27, 12, White);
            g.Circle(61, 27, 24, Ink, 7);
            g.Arc(36, 50, 50, 64, 180, 180, Ink, 7);
        }

        /// <summary>G 代码文件：白纸折角文件（炭色描边、折痕）+ 居中煤炭色 G。</summary>
        private static void GCodeFile(IconG g)
        {
            using (GraphicsPath p = new GraphicsPath())
            {
                p.AddArc(20, 8, 12, 12, 180, 90);
                p.AddLine(26, 8, 56, 8);
                p.AddLine(56, 8, 80, 32);
                p.AddLine(80, 32, 80, 82);
                p.AddArc(68, 76, 12, 12, 0, 90);
                p.AddLine(74, 88, 26, 88);
                p.AddArc(20, 76, 12, 12, 90, 90);
                p.AddLine(20, 82, 20, 14);
                p.CloseFigure();
                g.FillPath(White, p);
                g.DrawPath(Ink, 7, p);
            }
            g.Line(56, 8, 56, 32, Ink, 7);
            g.Line(56, 32, 80, 32, Ink, 7);
            g.Text("G", 50, 62, 38, Ink);
        }

        /// <summary>灰色音符（圆环内八分音符）。</summary>
        private static void Music(IconG g)
        {
            Color m = Color.FromArgb(110, 110, 110);
            g.Circle(50, 50, 84, m, 7);
            g.FillEllipse(24, 60, 22, 16, m);
            g.Line(44, 66, 44, 24, m, 8);
            g.FillPoly(m, 44, 24, 70, 32, 70, 52, 44, 42);
        }

        /// <summary>白色随机播放（交叉双箭头）。</summary>
        private static void Random(IconG g)
        {
            // 上箭头：右半段直杆 + 箭头
            g.Line(50, 26, 68, 26, White, 9);
            g.FillPoly(White, 82, 26, 64, 12, 64, 40);
            // 下箭头
            g.Line(50, 74, 88, 74, White, 9);
            g.FillPoly(White, 88, 74, 72, 60, 72, 88);
            // 左侧两股交叉到右半段
            g.Bezier(White, 8, 10, 74, 40, 74, 52, 26, 64, 26);
            g.Bezier(White, 8, 10, 26, 36, 26, 56, 74, 68, 74);
        }

        /// <summary>程序图标：熊猫头像（替代原 HAnchorTips.resx 内嵌 ico）。</summary>
        private static void Panda(IconG g)
        {
            Color outline = Color.FromArgb(64, 58, 54);
            Color earFill = Color.FromArgb(112, 112, 112);
            Color earInner = Color.FromArgb(208, 208, 208);
            Color patch = Color.FromArgb(88, 88, 88);
            Color eye = Color.FromArgb(45, 40, 36);
            Color blush = Color.FromArgb(255, 143, 125);
            Color nose = Color.FromArgb(82, 76, 72);
            Color shadow = Color.FromArgb(226, 226, 226);

            // 底部投影
            g.FillEllipse(24, 90, 52, 6, shadow);
            // 耳朵（外层灰 + 内耳 + 描边）
            g.FillCircle(27, 25, 40, earFill);
            g.FillCircle(73, 25, 40, earFill);
            g.FillEllipse(18, 12, 18, 21, earInner);
            g.FillEllipse(64, 12, 18, 21, earInner);
            g.Circle(27, 25, 40, outline, 4.5f);
            g.Circle(73, 25, 40, outline, 4.5f);
            // 脸
            g.FillEllipse(7, 16, 86, 78, White);
            g.Ellipse(7, 16, 86, 78, outline, 4.5f);
            // 倾斜黑眼圈
            g.FillEllipseRot(33, 51, 26, 32, 17, patch);
            g.FillEllipseRot(67, 51, 26, 32, -17, patch);
            // 眼珠 + 高光
            g.FillCircle(33, 53, 11, eye);
            g.FillCircle(67, 53, 11, eye);
            g.FillCircle(30.5f, 50.5f, 3.4f, White);
            g.FillCircle(64.5f, 50.5f, 3.4f, White);
            // 腮红
            g.FillCircle(19, 67, 13, blush);
            g.FillCircle(81, 67, 13, blush);
            // 鼻子 + 嘴
            g.FillEllipse(41, 65.5f, 18, 12.5f, nose);
            g.Line(50, 72.5f, 50, 81.5f, outline, 4);
        }

        // ===== 100 单位盒绘图助手（笔/画刷按本次渲染缓存复用） =====

        /// <summary>一次渲染内复用的 Graphics + Pen/Brush 池，全部坐标为 100 单位世界坐标。</summary>
        private sealed class IconG
        {
            public readonly Graphics G;
            private readonly Dictionary<int, Pen> pens = new Dictionary<int, Pen>();
            private readonly Dictionary<int, Brush> brushes = new Dictionary<int, Brush>();

            public IconG(Graphics g) { G = g; }

            // 坐标变换（旋转的笔/尺等复合图元用）
            public GraphicsState Save() { return G.Save(); }
            public void Translate(float x, float y) { G.TranslateTransform(x, y); }
            public void Rotate(float deg) { G.RotateTransform(deg); }
            public void Restore(GraphicsState st) { G.Restore(st); }

            public Pen Pen(Color c, float w)
            {
                int key = (c.ToArgb() << 8) | (int)(w * 4);
                Pen p;
                if (!pens.TryGetValue(key, out p))
                {
                    p = new Pen(c, w) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                    pens[key] = p;
                }
                return p;
            }

            public Brush Brush(Color c)
            {
                Brush b;
                if (!brushes.TryGetValue(c.ToArgb(), out b))
                {
                    b = new SolidBrush(c);
                    brushes[c.ToArgb()] = b;
                }
                return b;
            }

            // 基础图元
            public void Line(float x1, float y1, float x2, float y2, Color c, float w)
            {
                G.DrawLine(Pen(c, w), x1, y1, x2, y2);
            }

            public void Lines(Color c, float w, params float[] xy)
            {
                PointF[] pts = Points(xy);
                G.DrawLines(Pen(c, w), pts);
            }

            public void Poly(Color c, float w, params float[] xy)
            {
                PointF[] pts = Points(xy);
                G.DrawPolygon(Pen(c, w), pts);
            }

            public void FillPoly(Color c, params float[] xy)
            {
                G.FillPolygon(Brush(c), Points(xy));
            }

            public void Rect(float x, float y, float w, float h, Color c, float pw)
            {
                G.DrawRectangle(Pen(c, pw), x, y, w, h);
            }

            public GraphicsPath RRectPath(float x, float y, float w, float h, float r)
            {
                GraphicsPath p = new GraphicsPath();
                float d = r * 2;
                p.AddArc(x, y, d, d, 180, 90);
                p.AddArc(x + w - d, y, d, d, 270, 90);
                p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
                p.AddArc(x, y + h - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }

            public void RRect(float x, float y, float w, float h, float r, Color c, float pw)
            {
                using (GraphicsPath p = RRectPath(x, y, w, h, r))
                    G.DrawPath(Pen(c, pw), p);
            }

            public void FillRRect(float x, float y, float w, float h, float r, Color c)
            {
                using (GraphicsPath p = RRectPath(x, y, w, h, r))
                    G.FillPath(Brush(c), p);
            }

            public void Circle(float cx, float cy, float d, Color c, float pw)
            {
                G.DrawEllipse(Pen(c, pw), cx - d / 2, cy - d / 2, d, d);
            }

            public void Ellipse(float x, float y, float w, float h, Color c, float pw)
            {
                G.DrawEllipse(Pen(c, pw), x, y, w, h);
            }

            public void FillCircle(float cx, float cy, float d, Color c)
            {
                G.FillEllipse(Brush(c), cx - d / 2, cy - d / 2, d, d);
            }

            public void FillEllipse(float x, float y, float w, float h, Color c)
            {
                G.FillEllipse(Brush(c), x, y, w, h);
            }

            /// <summary>以 (cx,cy) 为中心、绕中心旋转 angleDeg 度填充椭圆。</summary>
            public void FillEllipseRot(float cx, float cy, float w, float h, float angleDeg, Color c)
            {
                GraphicsState st = G.Save();
                G.TranslateTransform(cx, cy);
                G.RotateTransform(angleDeg);
                G.FillEllipse(Brush(c), -w / 2, -h / 2, w, h);
                G.Restore(st);
            }

            public void FillRect(float x, float y, float w, float h, Color c)
            {
                G.FillRectangle(Brush(c), x, y, w, h);
            }

            public void Arc(float x, float y, float w, float h, float start, float sweep, Color c, float pw)
            {
                G.DrawArc(Pen(c, pw), x, y, w, h, start, sweep);
            }

            public void Bezier(Color c, float w, params float[] v)
            {
                G.DrawBezier(Pen(c, w), v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]);
            }

            public void DrawPath(Color c, float w, GraphicsPath path)
            {
                G.DrawPath(Pen(c, w), path);
            }

            /// <summary>填充任意路径（纸张等复合外形）。</summary>
            public void FillPath(Color c, GraphicsPath path)
            {
                G.FillPath(Brush(c), path);
            }

            /// <summary>居中文本（字号为世界单位）。</summary>
            public void Text(string s, float cx, float cy, float size, Color c)
            {
                using (Font f = new Font("Arial", size, FontStyle.Bold, GraphicsUnit.World))
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                    G.DrawString(s, f, Brush(c), new PointF(cx, cy), sf);
            }

            private static PointF[] Points(float[] xy)
            {
                PointF[] pts = new PointF[xy.Length / 2];
                for (int i = 0; i < pts.Length; i++)
                    pts[i] = new PointF(xy[i * 2], xy[i * 2 + 1]);
                return pts;
            }
        }
    }
}
