using HFromUI.HAttribute;
using HFromUI.HMath;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HEnum;
    using HFromUI.HLangage;
    /// <summary>
    /// 视觉窗口控件：参考 <see cref="HVisualWindow"/>，在图像显示/缩放/平移基础上，
    /// 增加 ROI 画图工具交互（点、直线、矩形、旋转矩形、圆、椭圆、三点圆弧、折线、多边形）。
    /// 所有图形（HTableShape 系列）均以 <b>图像像素坐标</b> 存储，位于 HDraw/Table 目录下，
    /// 可直接用于后期找特征处理（裁剪 ROI、模板匹配、圆/直线拟合、Blob 分析等）。
    /// 交互：
    ///   None  工具=左键拖拽平移、滚轮缩放、双击自适应；
    ///   Select工具=点选图形、拖动整体移动、拖动锚点修改、Delete 删除；
    ///   其余工具=在图像上绘制图形（拖拽或逐点单击，折线/多边形双击完成，右键/Esc 取消）。
    /// </summary>
    public class HVisualWindowA : HVisualWindow
    {
        // ====== 继承 HVisualWindow，不再 new 基类成员！（否则 HCameraChannel.Window.Image 写基类、HVisualWindowA 读自己的 → 黑屏）======
        // 用基类的：ScreenImage / Image / EnableKeys / CoordinateKeyDown/Up/Press / OnPaint 事件订阅

        /// <summary>窗口上全部 ROI 图形（图像像素坐标）</summary>
        public HTableShapeList Shapes = new HTableShapeList();

        // ------------------------------------------------------------
        // 键盘消息（由 HVisualWindow 基类 WndProc 统一处理）
        // ------------------------------------------------------------

        /// <summary>新图形绘制完成事件</summary>
        public event EventHandler<HTableShape> ShapeCreated;

        // 交互状态
        private HTableShape _building;          // 正在绘制的图形
        private bool _panning;                  // 平移中（None 左键 / 任意工具中键）
        private HTableShape _dragShape;         // Select 工具下正在拖动的图形
        /// <summary>_dragAnchor 字段。</summary>
        private int _dragAnchor = -1;           // 正在拖动的锚点序号（-1=整体移动）
        private Point _downScreenPos;           // 按下时屏幕位置（区分点击/拖拽）
        private bool _mouseMoved;               // 按下后是否移动超过阈值
        private const int DragThresholdPx = 3;  // 拖拽判定阈值（屏幕像素）
        private HPoint _lastDragImg;            // 拖动图形时上一帧图像坐标（按增量平移）
        private bool _coordInited;              // 无图默认坐标系是否已初始化（只初始化一次，避免拖拽时重置视图）
        private HTableShape _editTarget;        // 本次拖动编辑的图形（用于撤销）
        private HTableShape _editBackup;        // 拖动开始前的几何备份

        public HVisualWindowA()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (!DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                // 基类 HVisualWindow 构造函数已经订阅了 Paint/Mouse*/DoubleClick/Resize/ScreenImage.ImageChanged
                // HVisualWindowA 只订阅 ROI 相关的：Paint（叠加层）、SizeChanged、ScreenImage.ImageChanged
                // Paint 叠加层：基类先画图像+标尺，HVisualWindowA 再画 ROI —— OK，事件叠加

                Paint += HVisualWindowA_Paint;
                MouseDown += HVisualWindowA_MouseDown;
                MouseMove += HVisualWindowA_MouseMove;
                MouseUp += HVisualWindowA_MouseUp;
                MouseWheel += HVisualWindowA_MouseWheel;
                MouseDoubleClick += HVisualWindowA_DoubleClick;
                SizeChanged += HVisualWindowA_SizeChanged;
                base.ScreenImage.ImageChanged += ScreenImage_ImageChanged;

                // 键盘：直接用基类 HVisualWindow 的事件（C# 子类不能 base.Event += 语法）
                CoordinateKeyDown += OnCoordinateKeyDown;

                Shapes.ShapesChanged += (s, e) => Invalidate();
                Shapes.SelectionChanged += (s, e) => Invalidate();
                Shapes.ShapeEdited += (s, e) => Invalidate();

                UpdateToolCursor();
            }
        }

        // ------------------------------------------------------------
        // 属性
        // ------------------------------------------------------------

        /// <summary>
        /// 当前交互工具。切换工具会取消正在绘制的图形。
        /// </summary>
        [Browsable(false)]
        public HTableTool CurrentTool
        {
            get { return _currentTool; }
            set
            {
                if (_currentTool == value) return;
                _currentTool = value;
                // 同步基类 HVisualWindow 的工具状态：
                //   None  → HShapeType.None：基类左键拖拽平移视图（浏览模式）；
                //   其余  → HShapeType.Select：基类不再武装平移，避免"拖图形/画 ROI 时视图跟着跑"。
                ScreenImage.SelectedShapeType = (value == HTableTool.None) ? HShapeType.None : HShapeType.Select;
                // 清掉基类平移残留的锚点，防止切工具后第一次拖拽仍触发基类平移
                ScreenImage.LastMousePosition = null;
                ScreenImage.LastViewOffsetPosition = null;
                CancelDrawing();
                UpdateToolCursor();
                Invalidate();
            }
        }
        /// <summary>_currentTool 字段。</summary>
        private HTableTool _currentTool = HTableTool.None;

        /// <summary>图形绘制完成后是否自动切回选择工具（默认 false，可连续绘制同类 ROI）</summary>
        /// <summary>ResetToolAfterDrawn 成员。</summary>
        /// <summary>ResetToolAfterDrawn 字段。</summary>
        [Browsable(false)]
        public bool ResetToolAfterDrawn { get; set; } = false;

        /// <summary>是否显示图形名称标签</summary>
        /// <summary>ShowShapeLabels 成员。</summary>
        /// <summary>ShowShapeLabels 字段。</summary>
        [Browsable(false)]
        public bool ShowShapeLabels { get; set; } = true;

        // ------------------------------------------------------------
        // 坐标变换（统一入口：绘制与拾取共用，避免显示/命中错位）
        // 图像像素坐标：原点在图像左上角，X 向右、Y 向下。
        // ------------------------------------------------------------

        /// <summary>屏幕坐标（控件客户区）→ 图像像素坐标</summary>
        public HPoint ScreenToImage(HPoint screenPoint)
        {
            HPoint world = ScreenImage.ScreenToWorld(screenPoint);
            return new HPoint(
                world.X.Value - ScreenImage.ImageTopLeftWorld.X.Value,
                ScreenImage.ImageTopLeftWorld.Y.Value - world.Y.Value);
        }

        /// <summary>图像像素坐标 → 屏幕坐标（控件客户区）</summary>
        public PointF ImageToScreenF(HPoint imagePoint)
        {
            HPoint world = new HPoint(
                ScreenImage.ImageTopLeftWorld.X.Value + imagePoint.X.Value,
                ScreenImage.ImageTopLeftWorld.Y.Value - imagePoint.Y.Value);
            HPoint s = ScreenImage.WorldToScreen(world);
            return new PointF(s.X.ToSingle(), s.Y.ToSingle());
        }

        /// <summary>当前缩放（屏幕像素/图像像素）</summary>
        public float ViewScale => ScreenImage.ViewScale.ToSingle();

        /// <summary>图像像素坐标下的命中公差（由屏幕像素公差换算）</summary>
        private double ImageHitTolerance
        {
            get
            {
                float s = ViewScale;
                if (s < 1e-6f) s = 1f;
                return HTableShapeList.HitToleranceScreenPx / s;
            }
        }

        // ------------------------------------------------------------
        // 图像/视图
        // ------------------------------------------------------------
        /// <summary>记录上一次图像尺寸——仅尺寸变化时 FitImage，避免每帧重置缩放/平移</summary>
        private Size _lastImageSize = Size.Empty;

        /// <summary>ScreenImage_ImageChanged 方法。</summary>
        private void ScreenImage_ImageChanged(object sender, EventArgs e)
        {
            // 仅当图像尺寸变化时 FitImage（摄像头 25fps 同尺寸帧不能重置缩放）
            if (ScreenImage.Image != null)
            {
                var cur = new Size(ScreenImage.Image.Width, ScreenImage.Image.Height);
                if (cur != _lastImageSize)
                {
                    _lastImageSize = cur;
                    if (ScreenImage.ScreenRectangle != null) FitImage();
                }
            }
            else
            {
                _lastImageSize = Size.Empty;
                _coordInited = false;   // 图片关闭后，下次无图交互重新建立默认坐标系
            }
            if (InvokeRequired)
                BeginInvoke(new Action(Invalidate));
            else
                Invalidate();
        }

        /// <summary>自适应：整张图像完整显示在窗口中</summary>
        public void FitImage()
        {
            if (ScreenImage.Image == null) return;
            ScreenImage.UpdateViewBoundsFromImage(1.2);
            ScreenImage.FitWorldRectangle(ScreenImage.GetRectWorld().ToHRectangle(), true);
            Invalidate();
        }

        /// <summary>无图时初始化默认坐标系：1:1 屏幕映射，原点在控件左上角，Y 向下。只初始化一次，平移/拖拽后不重置</summary>
        private void EnsureDefaultCoordinateSystem()
        {
            if (_coordInited) return;
            _coordInited = true;
            if (ScreenImage.ScreenRectangle == null)
                ScreenImage.ScreenRectangle = new HRect(0, 0, Width, Height);
            else
            {
                ScreenImage.ScreenRectangle.X = 0;
                ScreenImage.ScreenRectangle.Y = 0;
                ScreenImage.ScreenRectangle.Width = Width;
                ScreenImage.ScreenRectangle.Height = Height;
            }
            // 1:1 缩放：图像像素坐标 = 屏幕坐标（Y 轴需要翻转）
            ScreenImage.ViewScale = 1;
            ScreenImage.ViewOffset = new HPoint(0, 0);
            // 图像左上角在世界坐标 (0, Height) 处，使图像 Y 向下 = 世界 Y 向上
            ScreenImage.ImageTopLeftWorld = new HPoint(0, Height);
        }

        /// <summary>HVisualWindowA_SizeChanged 方法。</summary>
        private void HVisualWindowA_SizeChanged(object sender, EventArgs e)
        {
            if (ScreenImage.ScreenRectangle == null)
                ScreenImage.ScreenRectangle = new HRect(0, 0, Width, Height);
            else
            {
                ScreenImage.ScreenRectangle.X = 0;
                ScreenImage.ScreenRectangle.Y = 0;
                ScreenImage.ScreenRectangle.Width = Width;
                ScreenImage.ScreenRectangle.Height = Height;
            }
        }

        // ------------------------------------------------------------
        // 键盘（WndProc 钩子，与 HVisualWindow 相同）
        // ------------------------------------------------------------
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            base.OnMouseDown(e);
        }

        /// <summary>WndProc 方法。</summary>
        protected override void WndProc(ref Message m)
        {
            // HVisualWindowA 不再自己触发 CoordinateKey* 事件——由基类 HVisualWindow.WndProc 统一触发
            // 只 override IsInputKey 让方向键等能被控件捕获
            base.WndProc(ref m);
        }

        /// <summary>判断是否 InputKey。</summary>
        protected override bool IsInputKey(Keys keyData) => true;

        /// <summary>基类键盘事件处理：Delete 删选中图形、Esc 取消绘制、Ctrl+Z 撤销、F2 打开图片</summary>
        private void OnCoordinateKeyDown(object sender, KeyEventArgs e)
        {
            if (!EnableKeys) return;
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    PushUndoForRemove();
                    Shapes.RemoveSelected();
                    break;
                case Keys.Escape:
                    if (_building != null) CancelDrawing();
                    else Shapes.Select(null);
                    break;
                case Keys.F2:
                    ScreenImage.OpenImageDialog();
                    break;
                case Keys.Z:
                    // 基类 WndProc 构造 KeyEventArgs 时只传虚拟键码、不含修饰键，
                    // e.Control 恒为 false，这里用 Control.ModifierKeys 判断 Ctrl 状态
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control) Undo();
                    break;
            }
        }

        // ------------------------------------------------------------
        // CTRL+Z 撤销栈（最多 50 步）
        // ------------------------------------------------------------
        private enum UndoType { Add, Remove, Edit }
        private class UndoAction { public UndoType Type; public HTableShape Shape; public HTableShape Backup; }
        /// <summary>_undoStack 字段。</summary>
        private readonly Stack<UndoAction> _undoStack = new Stack<UndoAction>();

        private const int MaxUndo = 50;
        /// <summary>PushUndoForAdd 方法。</summary>
        private void PushUndoForAdd(HTableShape added) { Push(new UndoAction { Type = UndoType.Add, Shape = added }); }
        /// <summary>PushUndoForRemove 方法。</summary>
        private void PushUndoForRemove() { if (Shapes.SelectedShape != null) Push(new UndoAction { Type = UndoType.Remove, Shape = Shapes.SelectedShape }); }
        /// <summary>拖动（整体移动/锚点修改）结束后压入编辑撤销：Backup 为拖动前的几何快照</summary>
        private void PushUndoForEdit(HTableShape target, HTableShape backup)
        {
            if (target != null && backup != null)
                Push(new UndoAction { Type = UndoType.Edit, Shape = target, Backup = backup });
        }
        /// <summary>Push 方法。</summary>
        private void Push(UndoAction a)
        {
            _undoStack.Push(a);
            // 限制栈大小，超过 50 则裁剪
            if (_undoStack.Count > MaxUndo)
            {
                var list = new List<UndoAction>(_undoStack);
                list.RemoveRange(MaxUndo, list.Count - MaxUndo);
                _undoStack.Clear();
                for (int i = list.Count - 1; i >= 0; i--) _undoStack.Push(list[i]);
            }
        }
        /// <summary>撤销最近一次添加/删除/移动编辑</summary>
        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            var a = _undoStack.Pop();
            switch (a.Type)
            {
                case UndoType.Add: Shapes.Remove(a.Shape); break;
                case UndoType.Remove: if (a.Shape != null) { Shapes.Add(a.Shape); } break;
                case UndoType.Edit:
                    // 恢复拖动前的锚点几何，图形自动联动重绘
                    if (a.Shape != null && a.Backup != null && Shapes.Contains(a.Shape))
                    {
                        a.Shape.CopyGeometryFrom(a.Backup);
                        Shapes.Select(a.Shape);
                        Shapes.NotifyEdited(a.Shape);
                    }
                    break;
            }
            Invalidate();
        }

        // ------------------------------------------------------------
        // 绘制
        // ------------------------------------------------------------
        private void HVisualWindowA_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(BackColor);
                HVisualWindowA_SizeChanged(null, null);
                // 无图时保持 1:1 坐标系，确保 ROI 图形可见
                if (ScreenImage.Image == null)
                    EnsureDefaultCoordinateSystem();
                g.SetClip(ScreenImage.ScreenRectangle.ToRectangleF());

                ScreenImage.DrawImage(g);
                ScreenImage.DrawImageRect(g);

                // 已完成图形
                float scale = ViewScale;
                Shapes.DrawAll(g, ImageToScreenF, scale, ShowShapeLabels ? Font : null);

                // 绘制中的图形（橡皮筋）
                if (_building != null)
                    _building.Draw(g, ImageToScreenF, true, scale);

                g.ResetClip();
                ScreenImage.DrawPromptShowNameA(g);
            }
            catch { }
        }

        // ------------------------------------------------------------
        // 鼠标交互
        // ------------------------------------------------------------
        private void HVisualWindowA_MouseWheel(object sender, MouseEventArgs e)
        {
            // 缩放逻辑基类 HVisualWindow.HFromUICoordinate_MouseWheel 已完成（以鼠标为中心 1.2/0.8），
            // 这里不再重复计算（否则一次滚轮缩放两次），仅刷新提示与画面。
            ScreenImage.ScreenPoint = new HPoint(e.X, e.Y);
            UpdatePrompt(e.Location);
            Invalidate();
        }

        /// <summary>HVisualWindowA_MouseDown 方法。</summary>
        private void HVisualWindowA_MouseDown(object sender, MouseEventArgs e)
        {
            ScreenImage.ScreenPoint = new HPoint(e.X, e.Y);
            _downScreenPos = e.Location;
            _mouseMoved = false;

            // 右键：取消正在绘制的图形
            if (e.Button == MouseButtons.Right)
            {
                if (_building != null) CancelDrawing();
                return;
            }
            // 中键：任何工具下都可平移视图（事件入口短路，优先于绘制/选择）
            if (e.Button == MouseButtons.Middle)
            {
                ScreenImage.LastMousePosition = new HPoint(e.X, e.Y);
                ScreenImage.LastViewOffsetPosition = ScreenImage.ViewOffset.Clone();
                _panning = true;
                Cursor = Cursors.SizeAll;
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            // 无图时也允许绘图：初始化默认坐标系（1:1 屏幕映射），让用户在空白窗口上画 ROI
            if (ScreenImage.Image == null)
                EnsureDefaultCoordinateSystem();

            HPoint img = ScreenToImage(new HPoint(e.X, e.Y));

            switch (CurrentTool)
            {
                case HTableTool.None:
                    // 平移（与 HVisualWindow 一致）
                    ScreenImage.LastMousePosition = new HPoint(e.X, e.Y);
                    ScreenImage.LastViewOffsetPosition = ScreenImage.ViewOffset.Clone();
                    _panning = true;
                    break;

                case HTableTool.Select:
                    BeginSelectDrag(img);
                    break;

                case HTableTool.Point:
                    // 单击即完成
                    HTableShape ps = HTableShapeList.Create(HTableTool.Point);
                    ps.AddAnchor(img);
                    FinishBuilding(ps);
                    break;

                default:
                    BeginOrAddAnchor(img);
                    break;
            }
            UpdatePrompt(e.Location);
            Invalidate();
        }

        /// <summary>
        /// Select 工具按下：
        /// 1) 已选中图形的锚点优先（拖点改图形）；
        /// 2) 命中任意图形时先测其锚点——命中锚点则选中并直接拖该点（图形联动变化）；
        /// 3) 否则选中图形并整体拖动；空白处取消选中。
        /// </summary>
        private void BeginSelectDrag(HPoint img)
        {
            double tol = ImageHitTolerance;
            HTableShape sel = Shapes.SelectedShape;
            if (sel != null)
            {
                int anchor0 = sel.HitTestAnchor(img, tol);
                if (anchor0 >= 0)
                {
                    StartShapeDrag(sel, anchor0, img);
                    return;
                }
            }
            HTableShape hit = Shapes.HitTest(img, tol);
            if (hit == null)
            {
                Shapes.Select(null);
                return;
            }
            Shapes.Select(hit);
            int anchor = hit.HitTestAnchor(img, tol);
            StartShapeDrag(hit, anchor, img);
        }

        /// <summary>开始一次图形拖动（anchorIndex=-1 表示整体移动），并记录拖动前几何备份供撤销</summary>
        private void StartShapeDrag(HTableShape shape, int anchorIndex, HPoint img)
        {
            _dragShape = shape;
            _dragAnchor = anchorIndex;
            _lastDragImg = img;
            _editTarget = shape;
            _editBackup = shape.CloneShape();
        }

        /// <summary>绘制工具：按下时创建图形或提交锚点</summary>
        private void BeginOrAddAnchor(HPoint img)
        {
            if (_building == null)
            {
                _building = HTableShapeList.Create(CurrentTool);
                _building.AddAnchor(img);
                _building.SetPreview(img);
            }
            else
            {
                // 提交当前锚点（折线/多边形逐点累积；固定点数图形累计到 RequiredPoints 即完成）
                _building.AddAnchor(img);
                if (_building.RequiredPoints != int.MaxValue &&
                    _building.Points.Count >= _building.RequiredPoints)
                {
                    FinishBuilding(_building);
                    return;
                }
                _building.SetPreview(img);
            }
        }

        /// <summary>HVisualWindowA_MouseMove 方法。</summary>
        private void HVisualWindowA_MouseMove(object sender, MouseEventArgs e)
        {
            ScreenImage.ScreenPoint = new HPoint(e.X, e.Y);

            // 点击/拖拽判定
            if (!_mouseMoved && Math.Abs(e.X - _downScreenPos.X) + Math.Abs(e.Y - _downScreenPos.Y) > DragThresholdPx)
                _mouseMoved = true;

            // 无图时默认坐标系由 Paint/MouseDown 保证已建立，这里不再重复初始化（否则会重置平移/缩放）
            HPoint img = ScreenToImage(new HPoint(e.X, e.Y));

            // 绘制中：鼠标移动（无论是否按住）都更新橡皮筋预览
            if (_building != null)
            {
                _building.SetPreview(img);

                // Freehand/Lasso/Mask 自由画刷：鼠标按住持续追加点（间距 >= 3px 才加，避免过密）
                bool isBrush = CurrentTool == HTableTool.Freehand
                            || CurrentTool == HTableTool.Lasso
                            || CurrentTool == HTableTool.Mask;
                if (isBrush && e.Button == MouseButtons.Left)
                {
                    HPoint last = _building.Points.Count > 0 ? _building.Points[_building.Points.Count - 1] : null;
                    if (last == null || HTableGeom.Distance(last, img) >= 3)
                        _building.AddAnchor(img);
                }
            }
            else if (_dragShape != null && e.Button == MouseButtons.Left)
            {
                // Select 工具拖动：锚点拖动改单点（图形联动），否则整体平移
                if (_dragAnchor >= 0)
                    _dragShape.SetAnchor(_dragAnchor, img);
                else
                {
                    HPoint delta = new HPoint(img.X.Value - _lastDragImg.X.Value, img.Y.Value - _lastDragImg.Y.Value);
                    _dragShape.MoveBy(delta);
                }
                _lastDragImg = img;
                Shapes.NotifyEdited(_dragShape);
            }
            else if (CurrentTool == HTableTool.Select && e.Button == MouseButtons.None)
            {
                // 悬停反馈：锚点上=四向移动光标，图形上=手型，空白=默认
                UpdateHoverCursor(img);
            }

            // None 工具左键平移（中键平移任意工具可用，由基类 MouseMove 处理）
            if (_panning && e.Button == MouseButtons.Left && CurrentTool == HTableTool.None)
            {
                if (ScreenImage.LastViewOffsetPosition != null && ScreenImage.LastMousePosition != null)
                {
                    HDouble ox = ScreenImage.LastViewOffsetPosition.X.Value + 1d * (e.X - ScreenImage.LastMousePosition.X.Value);
                    HDouble oy = ScreenImage.LastViewOffsetPosition.Y.Value + -1d * (e.Y - ScreenImage.LastMousePosition.Y.Value);
                    ScreenImage.ViewOffset = new HPoint(ox, oy);
                    ScreenImage.ClampViewOffsetToBounds();
                    ScreenImage.LastViewOffsetPosition = ScreenImage.ViewOffset.Clone();
                    ScreenImage.LastMousePosition = new HPoint(e.X, e.Y);
                }
            }

            UpdatePrompt(e.Location);
            Invalidate();
        }

        /// <summary>Select 工具悬停光标：锚点→SizeAll，图形→Hand，空白→Default</summary>
        private void UpdateHoverCursor(HPoint img)
        {
            double tol = ImageHitTolerance;
            HTableShape sel = Shapes.SelectedShape;
            int anchor = sel != null ? sel.HitTestAnchor(img, tol) : -1;
            HTableShape hover = null;
            if (anchor < 0)
            {
                hover = Shapes.HitTest(img, tol);
                if (hover != null) anchor = hover.HitTestAnchor(img, tol);
            }
            if (anchor >= 0) Cursor = Cursors.SizeAll;
            else if (hover != null) Cursor = Cursors.Hand;
            else Cursor = Cursors.Default;
        }

        /// <summary>HVisualWindowA_MouseUp 方法。</summary>
        private void HVisualWindowA_MouseUp(object sender, MouseEventArgs e)
        {
            ScreenImage.ScreenPoint = new HPoint(e.X, e.Y);

            // 中键平移结束：清理状态并恢复工具光标（清掉基类平移锚点，避免后续左键拖拽误平移）
            if (e.Button == MouseButtons.Middle)
            {
                _panning = false;
                ScreenImage.LastMousePosition = null;
                ScreenImage.LastViewOffsetPosition = null;
                UpdateToolCursor();
                return;
            }

            // Lasso / Mask 闭合工具：松开时自动把起点再追加一次，闭合成多边形
            if (_building != null && e.Button == MouseButtons.Left
                && (CurrentTool == HTableTool.Lasso || CurrentTool == HTableTool.Mask)
                && _building.Points.Count >= 3)
            {
                // 如果起点和终点不重合，追加起点实现闭环
                HPoint first = _building.Points[0];
                HPoint last = _building.Points[_building.Points.Count - 1];
                if (first.DistanceTo(last) > 2) _building.AddAnchor(first.Clone());
                FinishBuilding(_building);
            }
            // Freehand：松开直接完成开放曲线（不必闭合）
            else if (_building != null && e.Button == MouseButtons.Left && CurrentTool == HTableTool.Freehand
                && _building.Points.Count >= HTableFreehand.MinPoints)
            {
                FinishBuilding(_building);
            }
            // 拖拽手势：固定点数图形在松开时提交预览点（支持按下-拖拽绘制）
            else if (_building != null && _mouseMoved && e.Button == MouseButtons.Left
                && _building.RequiredPoints != int.MaxValue
                && _building.Points.Count < _building.RequiredPoints)
            {
                HPoint img = ScreenToImage(new HPoint(e.X, e.Y));
                _building.AddAnchor(img);
                if (_building.Points.Count >= _building.RequiredPoints)
                    FinishBuilding(_building);
                else
                    _building.SetPreview(img);
            }

            // Select 拖动结束：发生过实际移动才记录撤销（纯点击不压栈）
            if (_dragShape != null && _mouseMoved)
                PushUndoForEdit(_editTarget, _editBackup);

            _panning = false;
            _dragShape = null;
            _dragAnchor = -1;
            _editTarget = null;
            _editBackup = null;
            Invalidate();
        }

        /// <summary>HVisualWindowA_DoubleClick 方法。</summary>
        private void HVisualWindowA_DoubleClick(object sender, EventArgs e)
        {
            // 折线/多边形双击完成（双击的第二次按下会多产生一个重复点，去除尾部重合点）
            if (_building != null && _building.RequiredPoints == int.MaxValue)
            {
                while (_building.Points.Count >= 2 &&
                       _building.Points[_building.Points.Count - 1]
                           .DistanceTo(_building.Points[_building.Points.Count - 2]) < 1.0)
                {
                    _building.Points.RemoveAt(_building.Points.Count - 1);
                }
                if (_building.Points.Count >= HTablePolyline.MinPoints && _building.IsValid())
                    FinishBuilding(_building);
                else
                    CancelDrawing();
                return;
            }
            // 其余情况：双击自适应
            if (ScreenImage.Image != null)
                ScreenImage.FitWorldRectangle(ScreenImage.GetRectWorld().ToHRectangle(), true);
            Invalidate();
        }

        /// <summary>
        /// 抑制基类 HVisualWindow 的 DoubleClick 自适应（基类订阅了 DoubleClick 事件，
        /// 会与折线/多边形双击完成冲突——双击收尾时视图被意外缩放）。
        /// 不调用 base.OnDoubleClick，基类的 DoubleClick 事件就不会触发；
        /// MouseDoubleClick 事件由 OnMouseDoubleClick 独立触发，HVisualWindowA_DoubleClick 不受影响。
        /// </summary>
        protected override void OnDoubleClick(EventArgs e)
        {
            // 不调用 base.OnDoubleClick(e)，屏蔽基类双击 FitWorldRectangle
        }

        // ------------------------------------------------------------
        // 绘制流程控制
        // ------------------------------------------------------------
        /// <summary>完成一个图形：校验、加入集合、选中、通知</summary>
        private void FinishBuilding(HTableShape shape)
        {
            _building = null;
            if (shape == null) return;
            if (!shape.IsValid())
            {
                // 退化图形（点重合/零面积）直接丢弃
                Invalidate();
                return;
            }
            shape.IsBuilding = false;
            shape.ClearPreview();
            Shapes.Add(shape);
            PushUndoForAdd(shape);
            Shapes.Select(shape);
            ShapeCreated?.Invoke(this, shape);
            if (ResetToolAfterDrawn)
                CurrentTool = HTableTool.Select;
            Invalidate();
        }

        /// <summary>取消正在绘制的图形</summary>
        public void CancelDrawing()
        {
            if (_building == null) return;
            _building = null;
            Invalidate();
        }

        /// <summary>删除选中图形</summary>
        public void DeleteSelected() => Shapes.RemoveSelected();

        /// <summary>清空全部 ROI 图形</summary>
        public void ClearShapes() => Shapes.Clear();

        /// <summary>UpdatePrompt 方法。</summary>
        private void UpdatePrompt(Point mouse)
        {
            string tool;
            switch (CurrentTool)
            {
                case HTableTool.Select: tool = HTranslation.GetContent("选择"); break;
                case HTableTool.Point: tool = HTranslation.GetContent("画点"); break;
                case HTableTool.Line: tool = HTranslation.GetContent("画直线"); break;
                case HTableTool.Rectangle: tool = HTranslation.GetContent("画矩形"); break;
                case HTableTool.RotatedRectangle: tool = HTranslation.GetContent("画旋转矩形(拖边后点宽度)"); break;
                case HTableTool.Circle: tool = HTranslation.GetContent("画圆"); break;
                case HTableTool.Ellipse: tool = HTranslation.GetContent("画椭圆"); break;
                case HTableTool.Arc3P: tool = HTranslation.GetContent("画三点圆弧"); break;
                case HTableTool.Polyline: tool = HTranslation.GetContent("画折线(双击完成)"); break;
                case HTableTool.Polygon: tool = HTranslation.GetContent("画多边形(双击闭合)"); break;
                case HTableTool.Annulus: tool = HTranslation.GetContent("画圆环(圆心→外圈→内圈)"); break;
                case HTableTool.Sector: tool = HTranslation.GetContent("画扇形"); break;
                case HTableTool.AnnulusSector: tool = HTranslation.GetContent("画圆环扇形"); break;
                case HTableTool.Bullseye: tool = HTranslation.GetContent("画双圆靶心"); break;
                case HTableTool.Bezier: tool = HTranslation.GetContent("画三次贝塞尔(4 控制点)"); break;
                case HTableTool.Arrow: tool = HTranslation.GetContent("画箭头标注"); break;
                case HTableTool.Cross: tool = HTranslation.GetContent("画十字准星"); break;
                case HTableTool.PointRect: tool = HTranslation.GetContent("画点方框标记"); break;
                case HTableTool.Text: tool = HTranslation.GetContent("画文字标注"); break;
                case HTableTool.Caliper: tool = HTranslation.GetContent("卡尺测量"); break;
                case HTableTool.Angle: tool = HTranslation.GetContent("角度测量"); break;
                case HTableTool.Freehand: tool = HTranslation.GetContent("自由曲线(按住拖拽)"); break;
                case HTableTool.Lasso: tool = HTranslation.GetContent("套索(按住拖拽,松开闭合)"); break;
                case HTableTool.Mask: tool = HTranslation.GetContent("画笔遮罩"); break;
                default: tool = HTranslation.GetContent("平移浏览"); break;
            }
            if (ScreenImage.Image != null)
            {
                HPoint img = ScreenToImage(new HPoint(mouse.X, mouse.Y));
                ScreenImage.PromptShowName = $"{tool}  {HTranslation.GetContent("图形数")}:{Shapes.Count}  X:{img.X.Value:0.#}  Y:{img.Y.Value:0.#}";
            }
            else
            {
                ScreenImage.PromptShowName = $"{tool}  ({HTranslation.GetContent("F2 打开图片")})";
            }
        }

        /// <summary>UpdateToolCursor 方法。</summary>
        private void UpdateToolCursor()
        {
            switch (CurrentTool)
            {
                case HTableTool.None:
                case HTableTool.Select:
                    Cursor = Cursors.Default;
                    break;
                default:
                    Cursor = Cursors.Cross;
                    break;
            }
        }

        /// <summary>刷新。</summary>
        public override void Refresh()
        {
            UpdatePrompt(PointToClient(Cursor.Position));
            base.Refresh();
        }

        // ============================================================
        // 【全网最全】OpenCV / Halcon 导出 —— 让画出的 ROI 直接可用于找特征
        // ============================================================

        /// <summary>
        /// ROI 导出数据结构（同时兼容 OpenCV C++ / Halcon C# 调用方）。
        /// 所有坐标都是图像像素坐标，原点在图像左上角、X 向右、Y 向下。
        /// </summary>
        [Serializable]
        public class CVExport
        {
            /// <summary>图像尺寸（导出时的参考）</summary>
            public int ImageWidth, ImageHeight;
            /// <summary>轴对齐矩形（对应 OpenCV cv::Rect / Halcon gen_rectangle1）</summary>
            public List<CVRect> Rectangles = new List<CVRect>();
            /// <summary>旋转矩形（对应 OpenCV cv::RotatedRect / Halcon gen_rectangle2）</summary>
            public List<CVRotatedRect> RotatedRects = new List<CVRotatedRect>();
            /// <summary>圆（对应 OpenCV fitCircle / Halcon gen_circle）</summary>
            public List<CVCircle> Circles = new List<CVCircle>();
            /// <summary>椭圆</summary>
            public List<CVEllipse> Ellipses = new List<CVEllipse>();
            /// <summary>多边形轮廓（对应 OpenCV vector of Point / Halcon gen_polygon_xld_region）</summary>
            public List<List<CVPoint>> Contours = new List<List<CVPoint>>();
            /// <summary>自由曲线/折线（开放）</summary>
            public List<List<CVPoint>> Curves = new List<List<CVPoint>>();
            /// <summary>特征点</summary>
            public List<CVPoint> Points = new List<CVPoint>();
        }
        [Serializable] public struct CVPoint { public double X, Y; public CVPoint(double x, double y) { X = x; Y = y; } }
        [Serializable] public struct CVRect { public double X, Y, Width, Height; }
        [Serializable] public struct CVRotatedRect { public CVPoint Center; public double Width, Height, AngleDeg; }
        [Serializable] public struct CVCircle { public CVPoint Center; public double Radius; }
        [Serializable] public struct CVEllipse { public CVPoint Center; public double SemiMajor, SemiMinor, AngleDeg; }

        /// <summary>
        /// 把当前窗口所有 ROI 图形转换为 OpenCV / Halcon 通用的导出结构。
        /// 拿到 CVExport 后 C++ 端可直接用 cv::Rect / cv::RotatedRect / std::vector<cv::Point2f> 喂给 OpenCV；
        /// C# (HalconDotNet) 端可用 HOperatorSet.GenRectangle1/GenCircle/GenPolygonXldRegion 构造 HObject。
        /// </summary>
        public CVExport ToOpenCV()
        {
            var exp = new CVExport();
            if (ScreenImage.Image != null)
            { exp.ImageWidth = ScreenImage.Image.Width; exp.ImageHeight = ScreenImage.Image.Height; }
            foreach (var s in Shapes) AppendToExport(s, exp);
            return exp;
        }

        /// <summary>AppendToExport 方法。</summary>
        private static void AppendToExport(HTableShape s, CVExport exp)
        {
            switch (s.ShapeType)
            {
                case HTableTool.Rectangle:
                    var rRect = s.GetBoundingRect();
                    exp.Rectangles.Add(new CVRect { X = rRect.X, Y = rRect.Y, Width = rRect.Width, Height = rRect.Height });
                    break;
                case HTableTool.RotatedRectangle:
                    if (s is HTableRotatedRectangle rr && rr.IsValid())
                    {
                        double h = rr.EdgeStart != null && rr.EdgeEnd != null ? HTableGeom.Distance(rr.EdgeStart, rr.EdgeEnd) : 0;
                        double w = rr.WidthPoint != null ? 2.0 * HTableGeom.DistancePointToSegment(rr.WidthPoint, rr.EdgeStart, rr.EdgeEnd) : 0;
                        exp.RotatedRects.Add(new CVRotatedRect { Center = new CVPoint(rr.Center.X.Value, rr.Center.Y.Value), Width = w, Height = h, AngleDeg = rr.AngleDeg });
                    }
                    break;
                case HTableTool.Circle:
                    if (s is HTableCircle cc) exp.Circles.Add(new CVCircle { Center = new CVPoint(cc.Center.X.Value, cc.Center.Y.Value), Radius = cc.Radius });
                    break;
                case HTableTool.Ellipse:
                    if (s is HTableEllipse ee) exp.Ellipses.Add(new CVEllipse
                    { Center = new CVPoint(ee.Center.X.Value, ee.Center.Y.Value), SemiMajor = ee.RadiusX, SemiMinor = ee.RadiusY, AngleDeg = 0 });
                    break;
                case HTableTool.Point:
                case HTableTool.Cross:
                case HTableTool.PointRect:
                case HTableTool.Bullseye:
                case HTableTool.Angle:
                    if (s.Points.Count > 0) exp.Points.Add(new CVPoint(s.Points[0].X.Value, s.Points[0].Y.Value));
                    break;
                case HTableTool.Polygon:
                case HTableTool.Lasso:
                case HTableTool.Mask:
                case HTableTool.Sector:
                case HTableTool.Annulus:
                case HTableTool.AnnulusSector:
                    var pl = s.Points.ConvertAll(p => new CVPoint(p.X.Value, p.Y.Value));
                    if (pl.Count >= 3) exp.Contours.Add(pl);
                    break;
                case HTableTool.Polyline:
                case HTableTool.Freehand:
                    var cr = s.Points.ConvertAll(p => new CVPoint(p.X.Value, p.Y.Value));
                    if (cr.Count >= 2) exp.Curves.Add(cr);
                    break;
                case HTableTool.Arc3P:
                case HTableTool.Bezier:
                case HTableTool.Arrow:
                case HTableTool.Line:
                case HTableTool.Caliper:
                    var cl = s.Points.ConvertAll(p => new CVPoint(p.X.Value, p.Y.Value));
                    if (cl.Count >= 2) exp.Curves.Add(cl);
                    break;
            }
        }

        /// <summary>
        /// 把指定形状集合 rasterize 成二值 mask（尺寸等于图像）。
        /// 适用于 OpenCV cv::minAreaRect、cv::findContours 或 Halcon reduce_domain 等场景。
        /// 填充值=255（白色前景），背景=0。
        /// </summary>
        public Bitmap ToMaskBitmap(IEnumerable<HTableShape> shapes = null)
        {
            if (ScreenImage.Image == null) return null;
            int w = ScreenImage.Image.Width, h = ScreenImage.Image.Height;
            var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            // 8bpp 需要完整 256 条目 palette：索引 0 背景黑，索引 255 前景白，其余也填白（SolidBrush(Color.White) 会用白索引）
            var pal = bmp.Palette;
            pal.Entries[0] = Color.FromArgb(0, 0, 0);
            for (int i = 1; i < 256; i++) pal.Entries[i] = Color.FromArgb(255, 255, 255);
            bmp.Palette = pal;
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                var fillShapes = shapes ?? Shapes;
                using (Brush b = new SolidBrush(Color.White))
                using (Pen p = new Pen(Color.White))
                {
                    foreach (var s in fillShapes)
                        DrawShapeToMask(g, s, b, p);
                }
            }
            return bmp;
        }

        /// <summary>DrawShapeToMask 方法。</summary>
        private static void DrawShapeToMask(Graphics g, HTableShape s, Brush fill, Pen pen)
        {
            switch (s.ShapeType)
            {
                case HTableTool.Rectangle:
                    var rRect = s.GetBoundingRect();
                    g.FillRectangle(fill, rRect); break;
                case HTableTool.Circle:
                    if (s is HTableCircle cc && cc.Center != null)
                        g.FillEllipse(fill, (float)(cc.Center.X.Value - cc.Radius), (float)(cc.Center.Y.Value - cc.Radius),
                            (float)(cc.Radius * 2), (float)(cc.Radius * 2));
                    break;
                case HTableTool.Ellipse:
                    if (s is HTableEllipse ee && ee.Center != null)
                        g.FillEllipse(fill, (float)(ee.Center.X.Value - ee.RadiusX), (float)(ee.Center.Y.Value - ee.RadiusY),
                            (float)(ee.RadiusX * 2), (float)(ee.RadiusY * 2));
                    break;
                case HTableTool.Polygon:
                case HTableTool.Lasso:
                case HTableTool.Mask:
                    if (s.Points.Count >= 3)
                    {
                        var pts = s.Points.ConvertAll(p => new PointF((float)p.X.Value, (float)p.Y.Value)).ToArray();
                        g.FillPolygon(fill, pts, FillMode.Alternate);
                    }
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 生成 Halcon HDevelop 代码片段（字符串），可直接 Ctrl+V 到 HDevelop 中运行。
        /// 每个图形都生成对应的 gen_rectangle1 / gen_circle / gen_polygon_xld_region 等算子调用。
        /// </summary>
        public string ToHalconHDevelop()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("// Auto-generated from HVisualWindowA ROI shapes");
            int idx = 1;
            foreach (var s in Shapes)
            {
                sb.AppendLine($"// Shape {idx}: {s.TypeName} ({s.ShapeType}) Name={s.Name}");
                switch (s.ShapeType)
                {
                    case HTableTool.Rectangle:
                        var rr = s.GetBoundingRect();
                        sb.AppendLine($"gen_rectangle1 (ROI_{idx}, {rr.Y:F3}, {rr.X:F3}, {rr.Bottom:F3}, {rr.Right:F3})");
                        break;
                    case HTableTool.RotatedRectangle:
                        if (s is HTableRotatedRectangle rot && rot.IsValid())
                        {
                            double hh = rot.EdgeStart != null && rot.EdgeEnd != null ? HTableGeom.Distance(rot.EdgeStart, rot.EdgeEnd) : 0;
                            double ww = rot.WidthPoint != null ? 2.0 * HTableGeom.DistancePointToSegment(rot.WidthPoint, rot.EdgeStart, rot.EdgeEnd) : 0;
                            sb.AppendLine($"gen_rectangle2 (ROI_{idx}, {(double)rot.Center.Y.Value:F3}, {(double)rot.Center.X.Value:F3}, {rot.AngleDeg * Math.PI / 180.0:F6}, {hh / 2:F3}, {ww / 2:F3})");
                        }
                        break;
                    case HTableTool.Circle:
                        if (s is HTableCircle cc && cc.Center != null)
                            sb.AppendLine($"gen_circle (ROI_{idx}, {(double)cc.Center.Y.Value:F3}, {(double)cc.Center.X.Value:F3}, {cc.Radius:F3})");
                        break;
                    case HTableTool.Ellipse:
                        if (s is HTableEllipse ee && ee.Center != null)
                            sb.AppendLine($"gen_ellipse (ROI_{idx}, {(double)ee.Center.Y.Value:F3}, {(double)ee.Center.X.Value:F3}, 0.0, {ee.RadiusY:F3}, {ee.RadiusX:F3})");
                        break;
                    case HTableTool.Line:
                        if (s is HTableLine ln && ln.Points.Count >= 2)
                            sb.AppendLine($"gen_region_line (ROI_{idx}, {(double)ln.Points[0].Y.Value:F3}, {(double)ln.Points[0].X.Value:F3}, {(double)ln.Points[1].Y.Value:F3}, {(double)ln.Points[1].X.Value:F3})");
                        break;
                    case HTableTool.Polygon:
                    case HTableTool.Lasso:
                    case HTableTool.Mask:
                        if (s.Points.Count >= 3)
                        {
                            string rows = string.Join(", ", s.Points.Select(p => ((double)p.Y.Value).ToString("F3")));
                            string cols = string.Join(", ", s.Points.Select(p => ((double)p.X.Value).ToString("F3")));
                            sb.AppendLine($"gen_polygon_xld_region (XLD_{idx}, [{rows}], [{cols}], ROI_{idx})");
                        }
                        break;
                    default: break;
                }
                idx++;
            }
            return sb.ToString();
        }
    }
}
