using HFromUI.HBase;
using HFromUI.HData;
using HFromUI.HEnum;
using HFromUI.HMath;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace HFromUI.HControl.Coordinate
{
    using HFromUI.HColor;
    using HFromUI.HLangage;
    using HFromUI.HInterface;
    using HFromUI.HControl.Tools.Message;

    public class HDrawList
    {
        /// <summary>名称。</summary>
        public string Name { set; get; }
        /// <summary>获取鼠标当前位置，以 HPoint 返回（屏幕像素整数坐标）。</summary>
        public static HPoint GetMousePoint()
        {
            return new HPoint(System.Windows.Forms.Cursor.Position);
        }
        /// <summary>将鼠标移动到指定的 HPoint 位置（屏幕像素整数坐标）。</summary>
        public static void SetMousePoint(HPoint hPoint)
        {
            System.Windows.Forms.Cursor.Position = hPoint.ToPoint();
        }
        /// <summary>
        /// 缩放图片到指定宽度和高度（可保持比例）
        /// </summary>
        /// <param name="source">原始图片</param>
        /// <param name="width">目标宽度</param>
        /// <param name="height">目标高度</param>
        /// <param name="keepAspectRatio">是否保持纵横比（true 时会对齐较小边，剩余用背景色填充）</param>
        /// <param name="backgroundColor">背景色（仅在保持比例时填充空白区域）</param>
        public static Bitmap ResizeImage(Image source, int width, int height,
                                         bool keepAspectRatio = false,
                                         Color? backgroundColor = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            // 计算实际绘制区域
            int destWidth = width, destHeight = height;
            int srcX = 0, srcY = 0;
            if (keepAspectRatio)
            {
                float srcRatio = (float)source.Width / source.Height;
                float dstRatio = (float)width / height;
                if (srcRatio > dstRatio) // 原图更宽，以宽度为准
                {
                    destHeight = (int)(width / srcRatio);
                    srcY = (height - destHeight) / 2;
                }
                else // 原图更高，以高度为准
                {
                    destWidth = (int)(height * srcRatio);
                    srcX = (width - destWidth) / 2;
                }
            }
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                // 高质量设置
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                // 背景填充
                if (keepAspectRatio && backgroundColor.HasValue)
                    g.Clear(backgroundColor.Value);
                // 绘制缩放后的图片
                g.DrawImage(source, srcX, srcY, destWidth, destHeight);
            }
            return result;
        }
        /// <summary>获取 image。</summary>
        public static Image GetImage(HLinePhoto hLinePhoto)
        {
            Image image = null;
            switch (hLinePhoto)
            {
                case HLinePhoto.None:
                    image = null;
                    break;
                case HLinePhoto.Default:
                    image = HPhoto.Get("cursor_arrow");
                    break;
                case HLinePhoto.Line:
                    image = HPhoto.Get("draw_line_straight");
                    break;
                case HLinePhoto.Arc:
                    image = HPhoto.Get("draw_circle_arc");
                    break;
                case HLinePhoto.Arc3P:
                    image = HPhoto.Get("arc_3pt");
                    break;
                case HLinePhoto.Select:
                    image = HPhoto.Get("cursor_select");
                    break;
                case HLinePhoto.Start:
                    image = HPhoto.Get("tool_start");
                    break;
                case HLinePhoto.Stop:
                    image = HPhoto.Get("tool_stop");
                    break;
                case HLinePhoto.Pause:
                    image = HPhoto.Get("tool_pause_circle");
                    break;
                case HLinePhoto.UP:
                    image = HPhoto.Get("arrow_up_triple");
                    break;
                case HLinePhoto.Down:
                    image = HPhoto.Get("arrow_down_5");
                    break;
                case HLinePhoto.Left:
                    image = HPhoto.Get("arrow_left_double");
                    break;
                case HLinePhoto.Night:
                    image = HPhoto.Get("arrow_right_double");
                    break;
                case HLinePhoto.Mouse:
                    image = HPhoto.Get("cursor_arrow_active");
                    break;
                default:
                    image = HPhoto.Get("bug_fill");
                    break;
            }
            return image;
        }
        /// <summary>
        /// 重写基类的 OnPropertyValueChanged，在属性值修改后触发自定义的 ValueChanged 事件。
        /// </summary>
        public event EventHandler SelectValueChanged;
        /// <summary>响应 OnSelectValueChanged 事件。</summary>
        public virtual void OnSelectValueChanged(object o, EventArgs e)
        {
            SelectValueChanged?.Invoke(o, e);   // sender = 当前对象
        }
        /// <summary>HDrawRunPoint 成员。</summary>
        public HDrawRunPoint HDrawRunPoint = new HDrawRunPoint();
        private HList<string> selectGUID;
        /// <summary>是否启用鼠标捕获。</summary>
        public HBool IsCapture { get; set; }
        /// <summary>键盘方向键步长。</summary>
        public HDouble KeysMoveLength { set; get; } = 0.1;
        /// <summary>绘图变更记录集合。</summary>
        public HList<HDrawChange> DrawChanges { get; set; } = new HList<HDrawChange>();
        /// <summary>DrawChangesIndex 成员。</summary>
        public int DrawChangesIndex { get; set; } = -100;
        /// <summary>drawChangesIndexLock 字段。</summary>
        private object drawChangesIndexLock=new object();
        /// <summary>DrawChangeAdd 方法。</summary>
        public void DrawChangeAdd(HDrawChange HDrawChange)
        {
            lock (drawChangesIndexLock)
            {
                // -100 为“无历史”哨兵，不能参与截断循环（否则 RemoveAt(-100) 空转）
                if (DrawChangesIndex != -100 && DrawChangesIndex < DrawChanges.Count)
                {
                    int buffer = DrawChanges.Count - DrawChangesIndex;
                    for (int i = 0; i < buffer; i++)
                    {
                        DrawChanges.RemoveAt(DrawChangesIndex);
                    }
                }
                if (DrawChangesIndex == -100)
                {
                    DrawChangesIndex = DrawChanges.Count + 1;
                }
                else
                {
                    DrawChangesIndex++;
                }
                DrawChanges.Add(HDrawChange);
            }
            
        }
        /// <summary>DrawChangeClear 方法。</summary>
        public void DrawChangeClear()
        {
            lock (drawChangesIndexLock)
            {
                DrawChanges.Clear();
                DrawChangesIndex = -100;
            }
        }
        /// <summary>DrawChangeForward 方法。</summary>
        public bool DrawChangeForward(bool mode = false)
        {
            lock (drawChangesIndexLock)
            {
                if (mode)
                {
                    if (DrawChangesIndex - 1 < 0 || DrawChangesIndex - 1 >= DrawChanges.Count)
                        return false;
                    HDrawChange HDrawChange = DrawChanges[DrawChangesIndex - 1];
                    switch (HDrawChange.OperationMode)
                    {
                        case HOperationMode.None:
                            break;
                        // 撤销“添加” → 按记录索引删除该元素，并记录重做数据（否则重做时 DrawChange 为空）
                        case HOperationMode.Add:
                            {
                                if (HDrawChange.OldIndex >= 0 && HDrawChange.OldIndex < Shapes.Count)
                                {
                                    HDrawBase removed = Shapes[HDrawChange.OldIndex];
                                    // Clone 必须在 RemoveAt 之前——RemoveAt 之后 Shapes 不再持有它，但 Clone 不依赖 Shapes
                                    // 不调 Clear()：removed 可能被 HDrawGroup.Group 等其他集合引用，Clear 会把几何字段置 null
                                    HDrawChange.DrawChange = HDrawChange.Remove(new[] { HDrawChange.OldIndex }, new[] { removed.Clone() });
                                    Shapes.RemoveAt(HDrawChange.OldIndex);
                                }
                            }
                            break;
                        // 撤销“删除” → 还原被删元素
                        case HOperationMode.Remove:
                            {
                                List<int> keysAdd = new List<int>();
                                List<HDrawBase> AddShapes = new List<HDrawBase>();
                                List<int> keys = new List<int>(HDrawChange.Data.Keys);
                                keys.Sort(); // 从小到大还原
                                foreach (int idx in keys)
                                {
                                    AddShapes.Add(HDrawChange.Data[idx].Clone());
                                    if (idx > Shapes.Count)
                                    {
                                        Shapes.Add(HDrawChange.Data[idx].Clone());
                                        keysAdd.Add(Shapes.Count - 1);
                                    }
                                    else
                                    {
                                        Shapes.Insert(idx, HDrawChange.Data[idx].Clone());
                                        keysAdd.Add(idx);
                                    }
                                }
                                HDrawChange.DrawChange = HDrawChange.Add(keysAdd.ToArray(), AddShapes.ToArray());
                            }
                            break;
                        // 撤销“更新（移动/修改属性）” → 还原旧状态
                        case HOperationMode.Update:
                            {
                                List<int> keysAdd = new List<int>();
                                List<HDrawBase> AddShapes = new List<HDrawBase>();
                                foreach (var kv in HDrawChange.Data)
                                {
                                    int idx = kv.Key;
                                    if (idx < 0 || idx >= Shapes.Count) continue;
                                    keysAdd.Add(idx);
                                    AddShapes.Add(Shapes[idx].Clone());
                                    // 不调 Clear()：Shapes[idx] 可能被其他集合（如 HDrawGroup.Group）引用
                                    Shapes[idx] = kv.Value.Clone();
                                }
                                if (keysAdd.Count > 0)
                                    HDrawChange.DrawChange = HDrawChange.Update(keysAdd.ToArray(), AddShapes.ToArray());
                            }
                            break;
                        // 撤销“顺序变更（置顶/置底/前后移动）”——纯索引法
                        case HOperationMode.Change:
                            {
                                int oldIdx = HDrawChange.OldIndex;
                                int newIdx = HDrawChange.NewIndex;
                                // 生成重做用的正向操作（存入 DrawChange）
                                if (newIdx == -1)        // 原始是置顶
                                {
                                    HDrawChange.DrawChange = HDrawChange.Change(oldIdx, true);   // 正向置顶
                                }
                                else if (newIdx == -2)   // 原始是置底
                                {
                                    HDrawChange.DrawChange = HDrawChange.Change(oldIdx, false);  // 正向置底
                                }
                                else                     // 普通前后移动
                                {
                                    HDrawChange.DrawChange = HDrawChange.Change(newIdx, oldIdx); // 反向移动（即 Swap 回去）
                                }
                                // 找到当前位于 newIdx 的元素，将其移回 oldIdx
                                // 注意：若 newIdx 是 -1（置顶）表示元素已到末尾，-2（置底）表示元素已在开头
                                if (newIdx == -1)        // 撤销置顶：元素在末尾，移回 oldIdx
                                {
                                    if (Shapes.Count > 0 && oldIdx >= 0 && oldIdx < Shapes.Count)
                                        Shapes.Move(Shapes.Count - 1, oldIdx);
                                }
                                else if (newIdx == -2)   // 撤销置底：元素在开头，移回 oldIdx
                                {
                                    if (Shapes.Count > 0 && oldIdx >= 0 && oldIdx < Shapes.Count)
                                        Shapes.Move(0, oldIdx );
                                }
                                else if (newIdx >= 0 && newIdx < Shapes.Count) // 普通移动：元素在 newIdx，移回 oldIdx
                                {
                                    // 注意：若列表中间有增删，newIdx 可能指向错误元素，这是设计限制
                                    Shapes.Swap(newIdx, oldIdx);
                                }
                            }
                            break;
                        // === 撤销 Group（纯索引）：组位于记录的 Index（buffer），LIFO 下位置不变 ===
                        case HOperationMode.Group:
                            {
                                int groupIdx = HDrawChange.Index;
                                if (groupIdx < 0 || groupIdx >= Shapes.Count || !(Shapes[groupIdx] is HDrawGroup))
                                    break;
                                // 先克隆组作为重做数据（Clone 在 Clear 之前做，否则几何字段被置 null）
                                HDrawChange.DrawChange = HDrawChange.UnGroup(groupIdx, Shapes[groupIdx].Clone());
                                // 不调 Clear()：Shapes[groupIdx] 可能被其他地方（如临时集合）引用
                                Shapes.RemoveAt(groupIdx);
                                // 把子图形按记录的原始索引升序恢复（像 Remove 撤销一样）
                                var keys = new List<int>(HDrawChange.Data.Keys);
                                keys.Sort();
                                for (int i = 0; i < keys.Count; i++)
                                {
                                    int origIdx = keys[i];
                                    if (origIdx > Shapes.Count)
                                        Shapes.Add(HDrawChange.Data[origIdx].Clone());
                                    else
                                        Shapes.Insert(origIdx, HDrawChange.Data[origIdx].Clone());
                                }
                            }
                            break;
                        // UnGroup 撤销（重新组合）
                        // 解组时子图形连续插在 idx..idx+count-1，这里按同一连续区间收集。
                        // GetListAndRemove 按传入索引顺序返回（与索引数组一一对应），
                        // 故 HDrawBases[i] 与 indexsUnGroup[i] 配对正确。
                        case HOperationMode.UnGroup:
                            {
                                List<int> indexsUnGroup = new List<int>(HDrawChange.Data.Keys);
                                if (indexsUnGroup.Count == 1)
                                {
                                    int buffer = indexsUnGroup[0];
                                    if (HDrawChange.Data[buffer] is HDrawGroup)
                                    {
                                        HDrawGroup HDrawGroup = HDrawChange.Data[buffer] as HDrawGroup;
                                        indexsUnGroup.Clear();
                                        for (int i = buffer; i < buffer + HDrawGroup.Group.Count; i++)
                                        {
                                            indexsUnGroup.Add(i);
                                        }
                                        HList<HDrawBase> HDrawBases = Shapes.GetListAndRemove(indexsUnGroup.ToArray());
                                        List<HDrawBase> UnGroupShapes = new List<HDrawBase>();
                                        for (int i = 0; i < HDrawBases.Count; i++)
                                        {
                                            UnGroupShapes.Add(HDrawBases[i].Clone());
                                        }
                                        // 组回插到原位置 buffer：buffer 等于移除后元素数时应追加到末尾
                                        if (buffer >= Shapes.Count)
                                        {
                                            Shapes.Add(HDrawGroup.Clone());
                                        }
                                        else
                                        {
                                            Shapes.Insert(buffer, HDrawGroup.Clone());
                                        }
                                        HDrawChange.DrawChange = HDrawChange.Group(indexsUnGroup.ToArray(), UnGroupShapes.ToArray(), buffer);
                                    }
                                }
                            }
                            break;
                        case HOperationMode.Select:
                        default:
                            break;
                    }
                    if (DrawChangesIndex > 0)
                        DrawChangesIndex--;
                }
                if (DrawChangesIndex < 0)
                    return false;
                return DrawChangesIndex > 0; // 还有可撤销的步骤
            }
        }
        /// <summary>DrawChangeBackward 方法。</summary>
        public bool DrawChangeBackward(bool mode = false)
        {
            lock (drawChangesIndexLock)
            {
                if (mode)
                {
                    if (DrawChangesIndex < 0 || DrawChangesIndex >= DrawChanges.Count)
                        return false;
                    HDrawChange HDrawChange = DrawChanges[DrawChangesIndex].DrawChange;
                    HDrawChange HDrawSource = DrawChanges[DrawChangesIndex];   // 原始操作记录（重做数据的母体，可提供原始索引）
                    switch (HDrawChange.OperationMode)
                    {
                        case HOperationMode.None:
                            break;
                        // 重做“删除还原”的逆操作 → 按 Data 记录的索引降序移除
                        // （不能用 OldIndex：Add(int[], HDrawBase[]) 工厂不填 OldIndex，默认 0 会误删第一个图形）
                        case HOperationMode.Add:
                            {
                                List<int> keysRemoveAgain = new List<int>(HDrawChange.Data.Keys);
                                keysRemoveAgain.Sort();
                                for (int i = keysRemoveAgain.Count - 1; i >= 0; i--)
                                {
                                    int idx = keysRemoveAgain[i];
                                    if (idx >= 0 && idx < Shapes.Count)
                                    {
                                        // 不调 Clear()：可能被其他集合（如 HDrawGroup.Group）引用
                                        Shapes.RemoveAt(idx);
                                    }
                                }
                            }
                            break;
                        // 撤销“删除” → 还原被删元素
                        case HOperationMode.Remove:
                            {
                                List<int> keys = new List<int>(HDrawChange.Data.Keys);
                                keys.Sort(); // 从小到大还原
                                foreach (int idx in keys)
                                {
                                    if (idx > Shapes.Count)
                                    { Shapes.Add(HDrawChange.Data[idx].Clone()); }
                                    else
                                    { Shapes.Insert(idx, HDrawChange.Data[idx].Clone()); }
                                }
                            }
                            break;
                        // 撤销“更新（移动/修改属性）” → 还原旧状态
                        case HOperationMode.Update:
                            {
                                foreach (var kv in HDrawChange.Data)
                                {
                                    int idx = kv.Key;
                                    if (idx >= 0 && idx < Shapes.Count)
                                    {
                                        // 不调 Clear()：可能被其他集合引用
                                        Shapes[idx] = kv.Value.Clone();
                                    }
                                }
                            }
                            break;
                        // 撤销“顺序变更（置顶/置底/前后移动）”——纯索引法
                        case HOperationMode.Change:
                            {
                                int oldIdx = HDrawChange.OldIndex;
                                int newIdx = HDrawChange.NewIndex;
                                // 找到当前位于 newIdx 的元素，将其移回 oldIdx
                                // 注意：若 newIdx 是 -1（置顶）表示元素已到末尾，-2（置底）表示元素已在开头
                                if (newIdx == -1)        // 撤销置顶：元素在末尾，移回 oldIdx
                                {
                                    if (Shapes.Count > 0 && oldIdx >= 0 && oldIdx < Shapes.Count)
                                        Shapes.MoveToLast(oldIdx);
                                }
                                else if (newIdx == -2)   // 撤销置底：元素在开头，移回 oldIdx
                                {
                                    if (Shapes.Count > 0 && oldIdx >= 0 && oldIdx < Shapes.Count)
                                        Shapes.MoveToFirst(oldIdx);
                                }
                                else if (newIdx >= 0 && newIdx < Shapes.Count) // 普通移动：元素在 newIdx，移回 oldIdx
                                {
                                    // 注意：若列表中间有增删，newIdx 可能指向错误元素，这是设计限制
                                    Shapes.Swap(newIdx, oldIdx);
                                }
                            }
                            break;
                        // === 重做 Group（纯索引）：组位于重做数据记录的 Index（buffer） ===
                        case HOperationMode.Group:
                            {
                                int groupIdx = HDrawChange.Index;
                                if (groupIdx < 0 || groupIdx >= Shapes.Count || !(Shapes[groupIdx] is HDrawGroup))
                                    break;
                                // 不调 Clear()：Shapes[groupIdx] 可能被其他地方引用
                                Shapes.RemoveAt(groupIdx);
                                // 把子图形按记录的原始索引升序恢复（像 Remove 撤销一样）
                                var keys = new List<int>(HDrawChange.Data.Keys);
                                keys.Sort();
                                for (int i = 0; i < keys.Count; i++)
                                {
                                    int origIdx = keys[i];
                                    if (origIdx > Shapes.Count)
                                        Shapes.Add(HDrawChange.Data[origIdx].Clone());
                                    else
                                        Shapes.Insert(origIdx, HDrawChange.Data[origIdx].Clone());
                                }
                            }
                            break;
                        // UnGroup 重做（重新拆散 = 重做群组）
                        // ★ 关键修复：撤销 Group 后子图形回到各自原始索引（不一定连续），
                        // 旧代码用 buffer..buffer+n-1 连续窗口定位子图形——非连续群组时删错图形，
                        // 真子图形仍留在列表中且组克隆也含它们 → GUID 重复 + 图形丢失。
                        // 子图形真实索引取自原始 Group 记录（HDrawSource）的 Data 键。
                        case HOperationMode.UnGroup:
                            {
                                int buffer = HDrawChange.Index;
                                if (buffer < 0 || HDrawChange.Data.Count == 0) break;
                                HDrawGroup HDrawGroup = null;
                                foreach (var kv in HDrawChange.Data)
                                {
                                    HDrawGroup = kv.Value as HDrawGroup;
                                    break;
                                }
                                if (HDrawGroup == null) break;
                                // 子图形位置：优先取原始 Group 记录的 Data 键；无母体时退化为连续窗口
                                List<int> indexsUnGroup;
                                if (HDrawSource != null && HDrawSource.OperationMode == HOperationMode.Group && HDrawSource.Data.Count > 0)
                                {
                                    indexsUnGroup = new List<int>(HDrawSource.Data.Keys);
                                    indexsUnGroup.Sort();
                                }
                                else
                                {
                                    indexsUnGroup = new List<int>();
                                    for (int i = buffer; i < buffer + HDrawGroup.Group.Count; i++)
                                    {
                                        indexsUnGroup.Add(i);
                                    }
                                }
                                // 索引全部有效才执行，防止半途删错
                                bool valid = indexsUnGroup.Count > 0;
                                foreach (int i in indexsUnGroup)
                                {
                                    if (i < 0 || i >= Shapes.Count) { valid = false; break; }
                                }
                                if (!valid) break;
                                // 降序移除子图形
                                for (int i = indexsUnGroup.Count - 1; i >= 0; i--)
                                {
                                    int idx = indexsUnGroup[i];
                                    // 不调 Clear()：子图形可能被其他集合引用
                                    Shapes.RemoveAt(idx);
                                }
                                if (buffer >= Shapes.Count)
                                {
                                    Shapes.Add(HDrawGroup.Clone());
                                }
                                else
                                {
                                    Shapes.Insert(buffer, HDrawGroup.Clone());
                                }
                            }
                            break;
                        case HOperationMode.Select:
                        default:
                            break;
                    }
                    if (DrawChangesIndex < DrawChanges.Count)
                        DrawChangesIndex++;
                }
                if (DrawChangesIndex < 0)
                    return false;
                // 修正：允许重做最后一个操作
                return DrawChangesIndex < DrawChanges.Count;
            }
        }
        /// <summary>ColorLine 成员。</summary>
        public Color ColorLine { get; set; } = HColors.Oranges.Amber;
        /// <summary>ColorPoint 成员。</summary>
        public Color ColorPoint { get; set; } = HColors.Oranges.Amber;
        /// <summary>Color3PArc 成员。</summary>
        public Color Color3PArc { get; set; } = HColors.Oranges.Amber;
        /// <summary>ColorPointRect 成员。</summary>
        public Color ColorPointRect { get; set; } = HColors.Reds.Burgundy;
        /// <summary>线宽。</summary>
        public HDouble LineWidth { get; set; } = 2;
        /// <summary>DefaultColor 成员。</summary>
        public static Color DefaultColor { get; set; } = HColors.Oranges.Amber;
        /// <summary>DefaultLineWidth 成员。</summary>
        public static HDouble DefaultLineWidth { get; set; } = 2;
        /// <summary>StartEndPoints 成员。</summary>
        public List<HPoint3D> StartEndPoints { get; set; }
        /// <summary>showShapes 字段。</summary>
        private HList<HDrawBase> showShapes  = new HList<HDrawBase>();
        /// <summary>IsEnableLayer 成员。</summary>
        public bool IsEnableLayer { set; get; }
        /// <summary>Layer 成员。</summary>
        public int Layer { set; get; }
        /// <summary>DrawFormat 方法。</summary>
        public void DrawFormat(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            if (IsEnableFormat)
            {
                if (FormatX1== FormatX2|| FormatY2== FormatY1)
                {
                    return;
                }
                HRectangle hRectangle = new HRectangle(worldToScreen(new HPoint(FormatX1, FormatY1)), worldToScreen(new HPoint(FormatX2, FormatY2))); ;
                using (var pen = new Pen(FormatColor, FormatLineHeight))
                {
                    g.DrawRectangle(pen, hRectangle.ToDrawingRectangle());
                }
            }
        }
        /// <summary>DrawRunShapes 方法。</summary>
        public void DrawRunShapes(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            if (HDrawRunPoint != null&&IsRunShapes)
            {
                HDrawRunPoint.Color = HColors.Oranges.SunsetOrange;
                HDrawRunPoint.LineWidth = 2;
                HDrawRunPoint.Draw(g,worldToScreen,isSelected,scale);
            }
        }
        /// <summary>IsEnableFormat 成员。</summary>
        public bool IsEnableFormat { set; get; }
        /// <summary>FormatX1 成员。</summary>
        public double FormatX1 { set; get; }
        /// <summary>FormatY1 成员。</summary>
        public double FormatY1 { set; get; }
        /// <summary>FormatX2 成员。</summary>
        public double FormatX2 { set; get; }
        /// <summary>FormatY2 成员。</summary>
        public double FormatY2 { set; get; }
        /// <summary>FormatColor 成员。</summary>
        public Color FormatColor { set; get; } = HColors.Whites.Vanilla;
        /// <summary>FormatLineHeight 成员。</summary>
        public float FormatLineHeight { set; get; } = 3;
        /// <summary>SelectLineHeight 成员。</summary>
        public HDouble SelectLineHeight { get; set; } = 3;
        /// <summary>SelectPointHeight 成员。</summary>
        public HDouble SelectPointHeight { get; set; } = 8;
        /// <summary>IsRunShapes 成员。</summary>
        public bool IsRunShapes { set; get; }
        /// <summary>RunShapes 成员。</summary>
        public HList<HDrawBase> RunShapes { get; set; } = new HList<HDrawBase>();
        /// <summary>RunPoint2Ds 成员。</summary>
        public List<HPoint> RunPoint2Ds { get; set; } = new List<HPoint>();
        public HList<HDrawBase> ShowShapes
        { 
            get {
                if (IsRunShapes&& RunShapes!=null&& RunShapes.Count>0)
                {
                    if (IsEnableLayer)
                    {
                        if (RunShapes.Count < ShowShapesInt)
                        {
                            return RunShapes;
                        }
                    }
                }
                if (IsEnableLayer)
                {
                    if (Shapes.Count < ShowShapesInt)
                    {
                        return Shapes;
                    }
                }
                return showShapes; }
            set { showShapes = value; }
        }
        /// <summary>获取 shapesIndex。</summary>
        public int GetShapesIndex(string guid)
        {
            return  Shapes.FindIndex(t => t.GuidCode == guid);
        }
        /// <summary>获取 shapesGUID。</summary>
        public string GetShapesGUID(int index)
        {
            return Shapes[index].GuidCode;
        }
        /// <summary>获取 shapes。</summary>
        public HDrawBase GetShapes(int index)
        {
            return Shapes[index];
        }
        public ConcurrentDictionary<string, HPoint3D> ShowPoint { set; get; }
        private HDrawBase selectDrawBase;
        public HDrawBase SelectDrawBase
        {
            get
            { 
               return selectDrawBase;
            }
            set
            {
                selectDrawBase = value;
                if (selectDrawBase!=null)
                {
                    SelectValueChanged?.Invoke(selectDrawBase, EventArgs.Empty);
                    ConcurrentDictionary<string, HPoint3D> showPoint = new ConcurrentDictionary<string, HPoint3D>();
                    showPoint.Clear();
                    if (SelectDrawBase is HDrawLine)
                    {
                        HDrawLine HDrawLine = SelectDrawBase as HDrawLine;
                        showPoint.GetOrAdd("Start", HDrawLine.Start);
                        showPoint.GetOrAdd("End", HDrawLine.End);
                    }
                    else if (SelectDrawBase is HDraw3PArc)
                    {
                        HDraw3PArc HDrawLine = SelectDrawBase as HDraw3PArc;
                        showPoint.GetOrAdd("Start", HDrawLine.Start);
                        showPoint.GetOrAdd("End", HDrawLine.End);
                        showPoint.GetOrAdd("Middle", HDrawLine.Middle);
                    }
                    else if (SelectDrawBase is HDrawPoint)
                    {
                        HDrawPoint HDrawLine = SelectDrawBase as HDrawPoint;
                        showPoint.GetOrAdd("Center", HDrawLine.Center);
                    }
                    ShowPoint = showPoint;
                }
                else
                {
                    if (ShowPoint != null)
                    {
                        ShowPoint.Clear();
                    }
                    ShowPoint = null;
                }
            }
        }
        /// <summary>DrawShowPoint 方法。</summary>
        public void DrawShowPoint(Graphics g, Func<HPoint, HPoint> worldToScreen, HBool isSelected, HDouble scale)
        {
            if (ShowPoint==null|| ShowPoint.Count==0)
            {
                return;
            }
            List<HDrawBase> drawBases   =new List<HDrawBase>();
            drawBases.Clear();
            foreach (var item in ShowPoint.Keys)
            {
                if (item=="Start")
                {
                    HDrawBase HDrawBase = new HDrawPointCross(ShowPoint[item], ColorPointRect, LineWidth, "");
                    drawBases.Add(HDrawBase);
                }
                else
                {
                    HDrawBase HDrawBase = new HDrawPointRect(ShowPoint[item], ColorPointRect, LineWidth, "");
                    drawBases.Add(HDrawBase);
                }
            }
            foreach (var item in drawBases)
            {
                item.Draw(g,worldToScreen,isSelected,scale);
            }
        }
        /// <summary>
        /// 做CPU显示优化处理的阈值
        /// </summary>
        public HInt ShowShapesInt { set; get; } = 100;
        /// <summary>SelectedShapeType 成员。</summary>
        public HShapeType SelectedShapeType { set; get; } = HShapeType.None ;
        /// <summary>SelectedColor 成员。</summary>
        public static Color SelectedColor { set; get; } = HColors.Yellows.Corn;
        /// <summary>DrawPointSize 成员。</summary>
        public static HDouble DrawPointSize { set; get; } = 0.1d;
        /// <summary>DrawCrossLength 成员。</summary>
        public static HDouble DrawCrossLength { set; get; } = 0.1d;
        /// <summary>Shapes 成员。</summary>
        public HList<HDrawBase> Shapes { set; get; } = new HList<HDrawBase>();
        /// <summary>LastMousePosition 成员。</summary>
        public HPoint LastMousePosition { set; get; }
        /// <summary>LastMousePositionWorldPoint 成员。</summary>
        public HPoint LastMousePositionWorldPoint { set; get; } = new HPoint();
        /// <summary>LastViewOffsetPosition 成员。</summary>
        public HPoint LastViewOffsetPosition { set; get; }
        /// <summary>LastZ 成员。</summary>
        public HDouble LastZ { set; get; }
        /// <summary>SharedDraw3PArc 成员。</summary>
        public HDraw3PArc SharedDraw3PArc { set; get; }
        /// <summary>SharedDrawLine 成员。</summary>
        public HDrawLine SharedDrawLine { set; get; }
        /// <summary>SharedDrawPoint 成员。</summary>
        public HDrawPoint SharedDrawPoint { set; get; } 
        /// <summary>Next 方法。</summary>
        public OK Next(Action<object> action)
        {
            if (SelectGUID.Count>1)
            {
                return OK.FromError(-1);
            }
            if (SelectDrawBase!=null)
            {
                SelectDrawBase = Shapes.Next(GetShapesIndex(SelectDrawBase.GuidCode));
                if (SelectDrawBase==null)
                {
                    if (!_confirmBox.IsShow)
                    {
                        DialogResult dialogResult = _confirmBox.ShowMessageDialog(
                            HTranslation.GetContent("已经是最后一个图案了，是否保留在这个图案？"),
                            HTranslation.GetContent("提示"),
                            2,
                             HTranslation.GetContent("取消"), HTranslation.GetContent("保留"));
                        if (dialogResult != DialogResult.OK)
                        {
                            SelectDrawBase = Shapes[Shapes.Count-1];
                            SelectGUID.Clear();
                            SelectGUID.Add(SelectDrawBase.GuidCode);
                        }
                        else
                        {
                            action(SelectDrawBase);
                            SelectGUID.Clear();
                        }
                    }
                    return OK.FromError(-2);
                }
                SelectGUID.Clear();
                SelectGUID.Add(SelectDrawBase.GuidCode);
                return true;
            }
            return OK.FromError(2);
        }
        /// <summary>_confirmBox 字段。</summary>
        private HConfirmBox _confirmBox = new HConfirmBox();
        /// <summary>Last 方法。</summary>
        public OK Last(Action<object> action)
        {
            if (SelectGUID.Count > 1)
            {
                return OK.FromError(-1);
            }
            if (SelectDrawBase != null)
            {
                
                SelectDrawBase = Shapes.Last(GetShapesIndex(SelectDrawBase.GuidCode));
                if (SelectDrawBase == null)
                {
                    if (!_confirmBox.IsShow)
                    {
                        DialogResult dialogResult= _confirmBox.ShowMessageDialog(
                            HTranslation.GetContent("已经是第一个图案了，是否保留在这个图案？"),
                            HTranslation.GetContent("提示"),
                            2,
                            HTranslation.GetContent("取消"), HTranslation.GetContent("保留"));
                        if (dialogResult != DialogResult.OK)
                        {
                            SelectDrawBase = Shapes[0];
                            SelectGUID.Clear();
                            SelectGUID.Add(SelectDrawBase.GuidCode);
                        }
                        else
                        {
                            action(SelectDrawBase);
                            SelectGUID.Clear();
                        }
                    }
                    return OK.FromError(-2);
                }
                SelectGUID.Clear();
                SelectGUID.Add(SelectDrawBase.GuidCode);
                return true;
            }
            return OK.FromError(2);
        }
        /// <summary>AddSelectGUID 方法。</summary>
        public void AddSelectGUID(string GUID)
        {
            SelectGUID.Add(GUID);
            if (selectGUID != null)
            {
                if (selectGUID.Count == 1)
                {
                    SelectDrawBase = GetShapes(GetShapesIndex(selectGUID[0]));
                }
                else
                {
                    SelectDrawBase = null;
                }
            }
        }
        public HList<string> SelectGUID
        {
            get
            {
                if (selectGUID==null)
                {
                    selectGUID = new HList<string>();
                }
                return selectGUID;
            }
            set
            {
                if (selectGUID == null)
                {
                    selectGUID = new HList<string>();
                }
                selectGUID = value;
                if (selectGUID!=null)
                {
                    if (selectGUID.Count==1)
                    {
                        SelectDrawBase= GetShapes( GetShapesIndex(selectGUID[0]));
                    }
                }
            }
        }
        /// <summary>TempShape 成员。</summary>
        public HDrawBase TempShape { set; get; }
        /// <summary>IsDragging 成员。</summary>
        public HBool IsDragging { set; get; }
        /// <summary>
        /// 方向
        /// </summary>
        public HBool Direction { set; get; }
        /// <summary>
        /// 辅助线
        /// </summary>
        public HList<HDrawBase> VirtualShapes { set; get; } = new HList<HDrawBase>();
        /// <summary>IsContinuous 成员。</summary>
        public HBool IsContinuous { set; get; }
        /// <summary>
        /// 辅助连接线
        /// </summary>
        public HList<HDrawBase> VirtualContinuousShapes { set; get; } = new HList<HDrawBase>();
        /// <summary>EndPoint 成员。</summary>
        public HPoint EndPoint { set; get; }
        #region 辅助函数：递归展开群组
        /// <summary>
        /// 递归展开所有 HDrawGroup，返回只包含普通连续图元（线、弧、点）的扁平列表。
        /// 保持原始层次顺序。
        /// </summary>
        private List<iHDrawContinuousLine> FlattenToContinuousLines(HList<HDrawBase> shapes)
        {
            var result = new List<iHDrawContinuousLine>();
            if (shapes == null) return result;
            foreach (var shape in shapes)
            {
                if (shape is HDrawGroup group)
                {
                    // 递归处理嵌套群组
                    result.AddRange(FlattenToContinuousLines(group.Group));
                }
                else if (shape is iHDrawContinuousLine line)
                {
                    result.Add(line);
                }
            }
            return result;
        }
        #endregion
        #region 方法一：插入连接虚线
        /// <summary>
        /// 遍历展开后的连续图元，若前一个 End 与后一个 Start 距离 > 容差，
        /// 则在两者之间插入一条半透明紫色虚线直线。返回完全展开后的列表（不再包含 HDrawGroup）。
        /// </summary>
        public HList<HDrawBase> InsertConnectingDashedLines(HList<HDrawBase> shapes)
        {
            var flatLines = FlattenToContinuousLines(shapes);
            var result = new HList<HDrawBase>();
            if (flatLines.Count == 0)
                return result;
            for (int i = 0; i < flatLines.Count - 1; i++)
            {
                var cur = flatLines[i];
                var next = flatLines[i + 1];
                HDrawBase curDraw = cur as HDrawBase;
                result.Add(curDraw);
                HPoint endPt = new HPoint(cur.End.X.Value, cur.End.Y.Value);
                HPoint startPt = new HPoint(next.Start.X.Value, next.Start.Y.Value);
                double distance = HDrawList.Distance(endPt, startPt).Value;
                if (distance > HAppData.EpsilonSmall)
                {  // 创建半透明紫色：Alpha = 179 ≈ 255 * 0.7（30%透明）
                    Color lineColor = Color.FromArgb(120, HColors.Cyans.Aqua);
                    if (!IsEnableLayer)
                    {
                        lineColor = Color.FromArgb(120, HColors.Blues.ArcticBlue);
                    }
                  
                    // 线宽沿用前一个图元的线宽，若为空则使用默认值
                    HDouble lineWidth = curDraw?.LineWidth ?? HDrawList.DefaultLineWidth;
                
                    HDrawLine dashed = new HDrawLine(
                        new HPoint3D(endPt.X.Value, endPt.Y.Value, 0),
                        new HPoint3D(startPt.X.Value, startPt.Y.Value, 0),
                        lineColor,
                        lineWidth,
                        "AutoConnect"
                    );
                    dashed.Layer = curDraw.Layer;
                    dashed.PenMode = DashStyle.Dash;   // 虚线样式
                    result.Add(dashed);
                }
            }
            result.Add(flatLines.Last() as HDrawBase);
            return result;
        }
        #endregion
        #region 方法二：采样路径点（支持群组，步长可配置）
        /// <summary>SamplePathWithPoints2D 方法。</summary>
        public List<HPoint> SamplePathWithPoints2D(HList<HDrawBase> connectedShapes, double denominator = 1000.0)
        {
            List<iHDrawContinuousLine> iHDrawContinuousLines = FlattenToContinuousLines(connectedShapes);
            List < HPoint > result = new List<HPoint >();
            if (iHDrawContinuousLines==null|| iHDrawContinuousLines.Count==0)
            {
                return result;
            }
            result.Add(iHDrawContinuousLines[0].Start);
            double d = 0;
            for (int i = 0; i < iHDrawContinuousLines.Count; i++)
            {
                double s = iHDrawContinuousLines[i].Speed > 0 ? iHDrawContinuousLines[i].Speed.Value / denominator : 1200 / denominator;
                if (iHDrawContinuousLines[i].Length2D< s)
                {
                    result.Add(iHDrawContinuousLines[i].End);
                }
                else
                {
                    int j = 0;
                    while (true)
                    {
                        j++;
                        if (j*s> iHDrawContinuousLines[i].Length2D)
                        {
                            d = j * s - iHDrawContinuousLines[i].Length2D.Value;
                            result.Add(iHDrawContinuousLines[i].End);
                            break;
                        }
                        else
                        {
                            result.Add(iHDrawContinuousLines[i].GetPointAtDistance2D(j * s+d));
                        }
                    }
                }
            
            }
            return result;
        }
        #endregion
        /// <summary>CircleAngle 方法。</summary>
        public static HDouble CircleAngle(HPoint p1, HPoint center)
        {
            HDouble dx = p1.X.Value - center.X.Value;
            HDouble dy = p1.Y.Value - center.Y.Value;
            HDouble angle = (Math.Atan2(dy.Value, dx.Value) * (180 / Math.PI));
            return -1d * (angle + 180);
        }
        /// <summary>Distance 方法。</summary>
        public static HDouble Distance(HPoint p1, HPoint p2)
        {
            try
            {
                HDouble dx = p1.X.Value - p2.X.Value;
                HDouble dy = p1.Y.Value - p2.Y.Value;
                HDouble distance = Math.Sqrt((dx * dx + dy * dy).Value);
                return distance;
            }
            catch
            {
                return 0;
            }
          
        }
        /// <summary>获取 average。</summary>
        public static HPoint3D GetAverage(HPoint3D[] hPoint3Ds)
        {
            if (hPoint3Ds==null|| hPoint3Ds.Length==0)
            {
                return null;
            }
            HDouble x=0, y=0, z=0;
            for (int i = 0; i < hPoint3Ds.Length; i++)
            {
                x += hPoint3Ds[i].X.Value;
                y += hPoint3Ds[i].Y.Value;
                z += hPoint3Ds[i].Z.Value;
            }
            return new HPoint3D((x / hPoint3Ds.Length).Value, (y / hPoint3Ds.Length).Value, (z / hPoint3Ds.Length).Value);
        }
        /// <summary>
        /// 根据起点、参考终点（确定方向）和指定的长度，计算新的终点。
        /// 保持从起点到参考终点的方向不变，仅缩放长度。
        /// 若起点与参考终点重合（长度为零），无法确定方向，则直接返回参考终点。
        /// </summary>
        /// <param name="start">线段起点</param>
        /// <param name="referenceEnd">参考终点，用于确定线段方向</param>
        /// <param name="length">新的线段长度（世界单位）</param>
        /// <returns>计算得到的新终点</returns>
        public static HPoint3D CalculateEndPoint(HPoint3D start, HPoint3D referenceEnd, HDouble length)
        {
            double dx = referenceEnd.X.Value - start.X.Value;
            double dy = referenceEnd.Y.Value - start.Y.Value;
            double dz = referenceEnd.Z.Value - start.Z.Value;
            double currentLen = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            // 零长度向量时无法确定方向，返回原终点
            if (currentLen < 1e-10)
                return referenceEnd;
            double scale = length.Value / currentLen;
            return new HPoint3D(
                start.X.Value + dx * scale,
                start.Y.Value + dy * scale,
                start.Z.Value + dz * scale
            );
        }
        /// <summary>
        /// 根据2D起点、参考终点（确定方向）和指定长度，计算新的2D终点。
        /// 保持从起点到参考终点的方向不变，仅缩放长度。
        /// </summary>
        /// <param name="start">线段起点</param>
        /// <param name="referenceEnd">参考终点，用于确定线段方向</param>
        /// <param name="length">新的线段长度</param>
        /// <returns>计算得到的新终点</returns>
        public static HPoint CalculateEndPoint2D(HPoint start, HPoint referenceEnd, HDouble length)
        {
            double dx = referenceEnd.X.Value - start.X.Value;
            double dy = referenceEnd.Y.Value - start.Y.Value;
            double currentLen = Math.Sqrt(dx * dx + dy * dy);
            if (currentLen < 1e-10)
                return referenceEnd;
            double scale = length.Value / currentLen;
            return new HPoint(
                start.X.Value + dx * scale,
                start.Y.Value + dy * scale
            );
        }
    }
}
