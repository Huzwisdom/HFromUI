using HFromUI.HColor;
using HFromUI.HMath;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 视觉 ROI 图形集合：管理 HVisualWindowA 上所有已完成的画图工具图形。
    /// 提供添加/删除/清空/选择/命中测试/统一绘制，图形坐标均为图像像素坐标。
    /// </summary>
    [Serializable]
    public class HTableShapeList : IEnumerable<HTableShape>
    {
        // ------------------------------------------------------------
        // 全局默认外观（参考 HDrawList.DefaultColor/SelectedColor）
        // ------------------------------------------------------------
        /// <summary>默认图形颜色（亮绿，视觉软件 ROI 经典色）</summary>
        public static Color DefaultColor { get; set; } = HColors.Greens.Lime;

        /// <summary>选中图形颜色（金黄）</summary>
        public static Color SelectedColor { get; set; } = HColors.Yellows.Corn;

        /// <summary>绘制中（橡皮筋）颜色（血红）</summary>
        public static Color BuildingColor { get; set; } = HColors.Reds.BloodRed;

        /// <summary>默认线宽（屏幕像素）</summary>
        public static float DefaultLineWidth { get; set; } = 2f;

        /// <summary>锚点/图形命中公差（屏幕像素），内部换算为图像像素</summary>
        public const double HitToleranceScreenPx = 7.0;

        /// <summary>_shapes 字段。</summary>
        private readonly List<HTableShape> _shapes = new List<HTableShape>();

        /// <summary>图形数量</summary>
        public int Count => _shapes.Count;

        /// <summary>按序号取图形</summary>
        public HTableShape this[int index] => (index >= 0 && index < _shapes.Count) ? _shapes[index] : null;

        /// <summary>当前选中图形（null=未选中）</summary>
        public HTableShape SelectedShape { get; private set; }

        /// <summary>当前选中图形序号（-1=未选中）</summary>
        public int SelectedIndex => SelectedShape == null ? -1 : _shapes.IndexOf(SelectedShape);

        // ------------------------------------------------------------
        // 事件
        // ------------------------------------------------------------
        /// <summary>图形集合变化（新增/删除/清空）</summary>
        [field: NonSerialized]
        public event EventHandler ShapesChanged;

        /// <summary>选中图形变化</summary>
        [field: NonSerialized]
        public event EventHandler SelectionChanged;

        /// <summary>图形几何被编辑（移动/拖拽锚点）</summary>
        [field: NonSerialized]
        public event EventHandler<HTableShape> ShapeEdited;

        /// <summary>响应 ShapesChanged 事件。</summary>
        private void OnShapesChanged() => ShapesChanged?.Invoke(this, EventArgs.Empty);
        /// <summary>响应 SelectionChanged 事件。</summary>
        private void OnSelectionChanged() => SelectionChanged?.Invoke(this, EventArgs.Empty);

        // ------------------------------------------------------------
        // 工厂方法：按工具类型创建图形
        // ------------------------------------------------------------
        /// <summary>
        /// 根据工具类型创建空白图形（锚点由后续鼠标交互补充）。
        /// </summary>
        public static HTableShape Create(HTableTool tool)
        {
            HTableShape shape;
            switch (tool)
            {
                case HTableTool.Point: shape = new HTablePoint(); break;
                case HTableTool.Line: shape = new HTableLine(); break;
                case HTableTool.Rectangle: shape = new HTableRectangle(); break;
                case HTableTool.RotatedRectangle: shape = new HTableRotatedRectangle(); break;
                case HTableTool.Circle: shape = new HTableCircle(); break;
                case HTableTool.Ellipse: shape = new HTableEllipse(); break;
                case HTableTool.Arc3P: shape = new HTableArc3P(); break;
                case HTableTool.Polyline: shape = new HTablePolyline(); break;
                case HTableTool.Polygon: shape = new HTablePolygon(); break;

                // ======== 扩展几何 ========
                case HTableTool.Annulus: shape = new HTableAnnulus(); break;
                case HTableTool.Sector: shape = new HTableSector(); break;
                case HTableTool.AnnulusSector: shape = new HTableAnnulusSector(); break;
                case HTableTool.Bullseye: shape = new HTableBullseye(); break;
                case HTableTool.Bezier: shape = new HTableBezier(); break;

                // ======== 标注 ========
                case HTableTool.Arrow: shape = new HTableArrow(); break;
                case HTableTool.Cross: shape = new HTableCross(); break;
                case HTableTool.PointRect: shape = new HTablePointRect(); break;
                case HTableTool.Text: shape = new HTableText(); break;
                case HTableTool.Caliper: shape = new HTableCaliper(); break;
                case HTableTool.Angle: shape = new HTableAngle(); break;

                // ======== 自由画刷 ========
                case HTableTool.Freehand: shape = new HTableFreehand(); break;
                case HTableTool.Lasso: shape = new HTableLasso(); break;
                case HTableTool.Mask: shape = new HTableMask(); break;

                default:
                    throw new ArgumentException(HTranslation.GetContent("不支持的画图工具类型：") + tool, nameof(tool));
            }
            shape.IsBuilding = true;
            return shape;
        }

        // ------------------------------------------------------------
        // 集合操作
        // ------------------------------------------------------------
        /// <summary>添加一个已完成的图形（自动重命名编号）</summary>
        public void Add(HTableShape shape)
        {
            if (shape == null) return;
            shape.IsBuilding = false;
            shape.ClearPreview();
            if (string.IsNullOrWhiteSpace(shape.Name) || _shapes.Any(s => s.Name == shape.Name))
                shape.Name = shape.TypeName + shape.Id;
            _shapes.Add(shape);
            OnShapesChanged();
        }

        /// <summary>删除指定图形</summary>
        public bool Remove(HTableShape shape)
        {
            if (shape == null) return false;
            bool ok = _shapes.Remove(shape);
            if (ok)
            {
                if (ReferenceEquals(SelectedShape, shape))
                    Select(null);
                OnShapesChanged();
            }
            return ok;
        }

        /// <summary>删除选中图形</summary>
        public bool RemoveSelected()
        {
            if (SelectedShape == null) return false;
            HTableShape s = SelectedShape;
            Select(null);
            bool ok = _shapes.Remove(s);
            if (ok) OnShapesChanged();
            return ok;
        }

        /// <summary>清空全部图形</summary>
        public void Clear()
        {
            if (_shapes.Count == 0 && SelectedShape == null) return;
            _shapes.Clear();
            Select(null);
            OnShapesChanged();
        }

        /// <summary>选中指定图形（null 取消选中）</summary>
        public void Select(HTableShape shape)
        {
            if (ReferenceEquals(SelectedShape, shape)) return;
            SelectedShape = shape;
            OnSelectionChanged();
        }

        /// <summary>通知图形被编辑（移动/锚点拖拽后调用）</summary>
        public void NotifyEdited(HTableShape shape)
        {
            ShapeEdited?.Invoke(this, shape);
        }

        /// <summary>
        /// 命中测试：返回最上层（最后绘制）被点中的图形；无命中返回 null。
        /// </summary>
        public HTableShape HitTest(HPoint imagePoint, double toleranceImagePx)
        {
            for (int i = _shapes.Count - 1; i >= 0; i--)
            {
                if (_shapes[i].HitTest(imagePoint, toleranceImagePx))
                    return _shapes[i];
            }
            return null;
        }

        /// <summary>集合中是否包含指定图形实例（撤销编辑时校验用）</summary>
        public bool Contains(HTableShape shape)
        {
            return shape != null && _shapes.Contains(shape);
        }

        // ------------------------------------------------------------
        // 绘制
        // ------------------------------------------------------------
        /// <summary>
        /// 绘制全部图形。imageToScreen 为图像像素→屏幕坐标统一变换，
        /// scale 为屏幕像素/图像像素。
        /// </summary>
        public void DrawAll(Graphics g, Func<HPoint, PointF> imageToScreen, float scale, Font labelFont)
        {
            // 先画非选中图形，再画选中图形，保证选中图形置顶可见
            for (int i = 0; i < _shapes.Count; i++)
            {
                HTableShape s = _shapes[i];
                if (!ReferenceEquals(s, SelectedShape))
                    s.Draw(g, imageToScreen, false, scale);
            }
            if (SelectedShape != null)
            {
                SelectedShape.Draw(g, imageToScreen, true, scale);
                DrawAnchors(g, SelectedShape, imageToScreen);
            }
            if (labelFont != null)
            {
                for (int i = 0; i < _shapes.Count; i++)
                {
                    HTableShape s = _shapes[i];
                    if (s.Points.Count == 0) continue;
                    PointF sp = imageToScreen(s.Points[0]);
                    Color c = ReferenceEquals(s, SelectedShape) ? SelectedColor : s.Color;
                    DrawLabelText(g, sp, s.Name, c, labelFont);
                }
            }
        }

        /// <summary>绘制选中图形的锚点方块（可拖拽）</summary>
        private static void DrawAnchors(Graphics g, HTableShape shape, Func<HPoint, PointF> imageToScreen)
        {
            foreach (HPoint p in shape.Points)
            {
                PointF sp = imageToScreen(p);
                RectangleF r = new RectangleF(sp.X - HTableShape.HandleHalfSize, sp.Y - HTableShape.HandleHalfSize,
                    HTableShape.HandleHalfSize * 2, HTableShape.HandleHalfSize * 2);
                using (Pen pen = new Pen(SelectedColor, 1.5f))
                using (Brush b = new SolidBrush(Color.White))
                {
                    g.FillRectangle(b, r);
                    g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                }
            }
        }

        /// <summary>DrawLabelText 方法。</summary>
        private static void DrawLabelText(Graphics g, PointF sp, string text, Color color, Font font)
        {
            if (string.IsNullOrEmpty(text)) return;
            SizeF sz = g.MeasureString(text, font);
            float x = sp.X + 6f;
            float y = sp.Y - sz.Height - 4f;
            if (y < 0) y = sp.Y + 6f;
            using (Brush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                g.FillRectangle(bg, x - 2, y - 1, sz.Width + 4, sz.Height + 2);
            using (Brush b = new SolidBrush(color))
                g.DrawString(text, font, b, x, y);
        }

        // ------------------------------------------------------------
        // 查询便捷方法
        // ------------------------------------------------------------
        /// <summary>获取指定类型的全部图形</summary>
        public List<HTableShape> GetByType(HTableTool tool)
        {
            return _shapes.Where(s => s.ShapeType == tool).ToList();
        }

        /// <summary>获取 enumerator。</summary>
        public IEnumerator<HTableShape> GetEnumerator() => _shapes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _shapes.GetEnumerator();
    }
}
