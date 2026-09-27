using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HFromUI.HMath
{
    using HFromUI.HData;
    /// <summary>
    /// 轴对齐矩形（AABB），边与坐标轴平行。
    /// 支持两点构造、顶点操作、距离、相交、膨胀、变换等全功能。
    /// 所有标量距离结果使用 <see cref="HDouble"/> 包装。
    /// </summary>
    [Serializable]
    public class HRectangle
    {
        #region 枚举

        /// <summary>矩形四个顶点的标识。</summary>
        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        #endregion

        #region 字段

        private double _left;   // min X
        private double _right;  // max X
        private double _top;    // max Y
        private double _bottom; // min Y

        #endregion

        #region 属性
        public HDouble X
        { 
        get { return _left; }
            set { _left = value.Value; }
        }
        public HDouble Y
        {
            get { return _bottom; }
            set { _bottom = value.Value; }
        }


        /// <summary>左边界 X。</summary>
        public double Left => _left;
        /// <summary>右边界 X。</summary>
        public double Right => _right;
        /// <summary>上边界 Y。</summary>
        public double Top => _top;
        /// <summary>下边界 Y。</summary>
        public double Bottom => _bottom;

        /// <summary>矩形宽度（非负）。</summary>
        public double Width => Math.Max(0, _right - _left);
        /// <summary>矩形高度（非负）。</summary>
        public double Height => Math.Max(0, _top - _bottom);
        /// <summary>面积（宽×高）。</summary>
        public double Area => Width * Height;
        /// <summary>周长。</summary>
        public double Perimeter => 2.0 * (Width + Height);
        /// <summary>宽高比（宽度/高度），高度为 0 时返回 0。</summary>
        public double AspectRatio => Height > 1e-15 ? Width / Height : 0;

        /// <summary>中心点坐标。</summary>
        public HPoint Center => new HPoint((_left + _right) * 0.5, (_bottom + _top) * 0.5);

        /// <summary>左上顶点。</summary>
        public HPoint TopLeft => new HPoint(_left, _top);
        /// <summary>右上顶点。</summary>
        public HPoint TopRight => new HPoint(_right, _top);
        /// <summary>左下顶点。</summary>
        public HPoint BottomLeft => new HPoint(_left, _bottom);
        /// <summary>右下顶点。</summary>
        public HPoint BottomRight => new HPoint(_right, _bottom);

        /// <summary>是否为退化矩形（宽度或高度接近零）。</summary>
        public bool IsDegenerate => Width < 1e-15 || Height < 1e-15;
        /// <summary>是否为空矩形。</summary>
        public bool IsEmpty => IsDegenerate;

        /// <summary>以点对表示的对角点（左下, 右上）。</summary>
        public (HPoint MinPoint, HPoint MaxPoint) DiagonalPoints => (BottomLeft, TopRight);

        /// <summary>尺寸（宽度, 高度）。</summary>
        public (double Width, double Height) Size => (Width, Height);

        #endregion

        #region 构造函数

        /// <summary>默认构造，位于 (0,0) 且宽高为 0。</summary>
        public HRectangle()
        {
            _left = _right = 0;
            _top = _bottom = 0;
        }
        public HRectangle(HPoint location,HDouble width,HDouble height)
        {
            _left = location.X.Value;
            _top=location.Y.Value;
            _right = location.X.Value + width.Value;
            _bottom = location.Y.Value + height.Value;
        }
        /// <summary>通过两个对角点构造（自动规范化为 min/max）。</summary>
        /// <param name="p1">第一个对角点。</param>
        /// <param name="p2">第二个对角点。</param>
        public HRectangle(HPoint p1, HPoint p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            _left = Math.Min((double)p1.X.Value, (double)p2.X.Value);
            _right = Math.Max((double)p1.X.Value, (double)p2.X.Value);
            _bottom = Math.Min((double)p1.Y.Value, (double)p2.Y.Value);
            _top = Math.Max((double)p1.Y.Value, (double)p2.Y.Value);
        }

        /// <summary>通过边界值直接构造。</summary>
        public HRectangle(double left, double top, double right, double bottom)
        {
            _left = Math.Min(left, right);
            _right = Math.Max(left, right);
            _bottom = Math.Min(bottom, top);
            _top = Math.Max(bottom, top);
        }

        /// <summary>从中心点和宽高创建矩形。</summary>
        public static HRectangle FromCenter(HPoint center, double width, double height)
        {
            if (center == null) throw new ArgumentNullException(nameof(center));
            double halfW = Math.Abs(width) * 0.5;
            double halfH = Math.Abs(height) * 0.5;
            return new HRectangle(center.X.Value - halfW, center.Y.Value + halfH,
                                  center.X.Value + halfW, center.Y.Value - halfH);
        }

        /// <summary>从点集创建最小轴对齐包围盒。</summary>
        public static HRectangle FromPoints(IEnumerable<HPoint> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var list = points.ToList();
            if (list.Count == 0) return new HRectangle(0, 0, 0, 0);
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            foreach (var p in list)
            {
                if (p.X.Value < minX) minX = p.X.Value;
                if (p.X.Value > maxX) maxX = p.X.Value;
                if (p.Y.Value < minY) minY = p.Y.Value;
                if (p.Y.Value > maxY) maxY = p.Y.Value;
            }
            return new HRectangle(minX, maxY, maxX, minY);
        }

        #endregion


        #region 顶点访问与对角点操作

        /// <summary>获取指定标识的顶点坐标。</summary>
        public HPoint GetCorner(Corner corner)
        {
            switch (corner)
            {
                case Corner.TopLeft: return TopLeft;
                case Corner.TopRight: return TopRight;
                case Corner.BottomLeft: return BottomLeft;
                case Corner.BottomRight: return BottomRight;
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
        }

        /// <summary>获取指定顶点的对角顶点坐标。</summary>
        public HPoint GetOppositeCorner(Corner corner)
        {
            switch (corner)
            {
                case Corner.TopLeft: return BottomRight;
                case Corner.TopRight: return BottomLeft;
                case Corner.BottomLeft: return TopRight;
                case Corner.BottomRight: return TopLeft;
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
        }

        /// <summary>用两个对角点重新定义矩形。</summary>
        public void SetDiagonalPoints(HPoint p1, HPoint p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            _left = Math.Min((double)p1.X.Value, (double)p2.X.Value);
            _right = Math.Max((double)p1.X.Value, (double)p2.X.Value);
            _bottom = Math.Min((double)p1.Y.Value, (double)p2.Y.Value);
            _top = Math.Max((double)p1.Y.Value, (double)p2.Y.Value);
        }

        /// <summary>将指定顶点移动到新位置，对角的顶点保持不变。</summary>
        public void SetCorner(Corner corner, HPoint newPoint)
        {
            if (newPoint == null) throw new ArgumentNullException(nameof(newPoint));
            switch (corner)
            {
                case Corner.TopLeft:
                    _left = newPoint.X.Value; _top = newPoint.Y.Value;
                    break;
                case Corner.TopRight:
                    _right = newPoint.X.Value; _top = newPoint.Y.Value;
                    break;
                case Corner.BottomLeft:
                    _left = newPoint.X.Value; _bottom = newPoint.Y.Value;
                    break;
                case Corner.BottomRight:
                    _right = newPoint.X.Value; _bottom = newPoint.Y.Value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(corner));
            }
            // 自动纠正可能的反向
            if (_left > _right) { double t = _left; _left = _right; _right = t; }
            if (_bottom > _top) { double t = _bottom; _bottom = _top; _top = t; }
        }

        #endregion

        #region 平移、缩放与中点移动

        /// <summary>平移矩形，返回新矩形。</summary>
        public HRectangle Translate(double dx, double dy)
        {
            return new HRectangle(_left + dx, _top + dy, _right + dx, _bottom + dy);
        }

        /// <summary>将中心移动到指定点。</summary>
        public HRectangle MoveCenterTo(HPoint newCenter)
        {
            if (newCenter == null) throw new ArgumentNullException(nameof(newCenter));
            return Translate(newCenter.X.Value - Center.X.Value, newCenter.Y.Value - Center.Y.Value);
        }

        /// <summary>原地平移（修改当前实例）。</summary>
        public void Offset(double dx, double dy)
        {
            _left += dx; _right += dx;
            _top += dy; _bottom += dy;
        }

        /// <summary>原地膨胀（负值收缩）。</summary>
        public void Inflate(double horizontal, double vertical)
        {
            _left -= horizontal; _right += horizontal;
            _bottom -= vertical; _top += vertical;
        }

        /// <summary>返回膨胀后的新矩形。</summary>
        public HRectangle Inflated(double horizontal, double vertical)
        {
            return new HRectangle(_left - horizontal, _top + vertical,
                                  _right + horizontal, _bottom - vertical);
        }

        /// <summary>以左上角为锚点缩放。</summary>
        public HRectangle ScaleFromTopLeft(double sx, double sy)
        {
            return new HRectangle(_left, _top,
                                  _left + Width * sx, _top - Height * sy);
        }

        /// <summary>以中心为锚点等比缩放。</summary>
        public HRectangle ScaleFromCenter(double sx, double sy)
        {
            double cx = Center.X.Value, cy = Center.Y.Value;
            double newHalfW = Width * Math.Abs(sx) * 0.5;
            double newHalfH = Height * Math.Abs(sy) * 0.5;
            return new HRectangle(cx - newHalfW, cy + newHalfH, cx + newHalfW, cy - newHalfH);
        }

        /// <summary>矩形绕中心旋转指定弧度后，返回新的轴对齐包围盒。</summary>
        public HRectangle RotatedBounds(double angleRad)
        {
            if (IsEmpty) return Clone();
            var corners = ToPolygon();
            var rotated = corners.Select(p => p.RotateAround(Center, angleRad));
            return FromPoints(rotated);
        }

        #endregion

        #region 包含与关系判断

        /// <summary>判断点是否在矩形内部（含边界）。</summary>
        public bool ContainsPoint(HPoint point, double tolerance = 1e-10)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            return point.X.Value >= _left - tolerance && point.X.Value <= _right + tolerance &&
                   point.Y.Value >= _bottom - tolerance && point.Y.Value <= _top + tolerance;
        }
        /// <summary>ContainsSegment 方法。</summary>
        public bool ContainsSegment(HPoint p1, HPoint p2, double tolerance = 1e-10)
        {
            return ContainsPoint(p1, tolerance) && ContainsPoint(p2, tolerance);
        }
        /// <summary>判断是否完全包含另一个矩形。</summary>
        public bool ContainsRectangle(HRectangle other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return _left <= other._left && _right >= other._right &&
                   _bottom <= other._bottom && _top >= other._top;
        }

        /// <summary>ContainPointLineRect 方法。</summary>
        public bool ContainPointLineRect(HPoint[] hPoints,int mode=0)
        {
            bool isc = false;
            try
            {
                if (hPoints==null)
                {
                    return false;
                }
                if (hPoints.Length == 4)
                {
                    HPoints hPointss = new HPoints(hPoints);
                    HRectangle hRectangle = new HRectangle(new HPoint(hPointss.Bounds.minX, hPointss.Bounds.minY), new HPoint(hPointss.Bounds.maxX, hPointss.Bounds.maxX));
                    if (ContainsRectangle(hRectangle) || Intersects(hRectangle))
                    {
                        return true;
                    }
                }
                else
                {
                    foreach (var item in hPoints)
                    {
                        if (ContainsPoint(item, HAppData.Epsilon))
                        {
                            return true;
                        }
                    }
                }
                if (hPoints.Length == 2 && mode == 1)
                {
                    isc = IntersectionWithSegment(new HLine(hPoints[0], hPoints[1])).Count > 0;
                }
            }
            catch 
            {
                return false;
            }

            return isc;
        }

        /// <summary>判断是否与另一矩形相交。</summary>
        public bool Intersects(HRectangle other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return _left < other._right && _right > other._left &&
                   _bottom < other._top && _top > other._bottom;
        }

        /// <summary>返回与另一矩形的交集矩形（不相交则返回 0 面积矩形）。</summary>
        public HRectangle Intersection(HRectangle other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            double l = Math.Max((double)_left, (double)other._left);
            double r = Math.Min((double)_right, (double)other._right);
            double b = Math.Max((double)_bottom, (double)other._bottom);
            double t = Math.Min((double)_top, (double)other._top);
            return (l < r && b < t) ? new HRectangle(l, t, r, b) : new HRectangle(0, 0, 0, 0);
        }

        /// <summary>返回包围两个矩形的最小矩形。</summary>
        public HRectangle Union(HRectangle other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new HRectangle(
                Math.Min((double)_left, (double)other._left),
                Math.Max((double)_top, (double)other._top),
                Math.Max((double)_right, (double)other._right),
                Math.Min((double)_bottom, (double)other._bottom));
        }

        /// <summary>带容差的相等比较。</summary>
        public bool Equals(HRectangle other, double tolerance = 1e-10)
        {
            if (other == null) return false;
            return Math.Abs(_left - other._left) < tolerance &&
                   Math.Abs(_right - other._right) < tolerance &&
                   Math.Abs(_top - other._top) < tolerance &&
                   Math.Abs(_bottom - other._bottom) < tolerance;
        }

        #endregion

        #region 点到矩形的距离与最近点

        /// <summary>点到矩形的最短距离（HDouble）。点在内部时返回 0。</summary>
        public HDouble DistanceToPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            HDouble dx = Math.Max(_left - point.X.Value, Math.Max(0d, point.X.Value - _right));
            HDouble dy = Math.Max(_bottom - point.Y.Value, Math.Max(0d, point.Y.Value - _top));
            return Math.Sqrt((dx * dx + dy * dy).Value);
        }

        /// <summary>点到矩形的最短距离平方。</summary>
        public double DistanceSquaredToPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double dx = Math.Max(_left - point.X.Value, Math.Max(0d, point.X.Value - _right));
            double dy = Math.Max(_bottom - point.Y.Value, Math.Max(0d, point.Y.Value - _top));
            return dx * dx + dy * dy;
        }

        /// <summary>矩形边界上距离指定点最近的点。</summary>
        public HPoint ClosestPoint(HPoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            double x = Clamp(point.X.Value, _left, _right);
            double y = Clamp(point.Y.Value, _bottom, _top);
            // 若点已在矩形内部，取最近边上的投影
            if (x == point.X.Value && y == point.Y.Value)
            {
                double toLeft = point.X.Value - _left;
                double toRight = _right - point.X.Value;
                double toBottom = point.Y.Value - _bottom;
                double toTop = _top - point.Y.Value;
                double min = Math.Min(Math.Min(toLeft, toRight), Math.Min(toBottom, toTop));
                if (Math.Abs(min - toLeft) < 1e-15) return new HPoint(_left, point.Y.Value);
                if (Math.Abs(min - toRight) < 1e-15) return new HPoint(_right, point.Y.Value);
                if (Math.Abs(min - toBottom) < 1e-15) return new HPoint(point.X.Value, _bottom);
                return new HPoint(point.X.Value, _top);
            }
            return new HPoint(x, y);
        }

        /// <summary>Clamp 方法。</summary>
        private static double Clamp(double val, double min, double max)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        #endregion

        #region 与直线、线段、圆的相交

        /// <summary>矩形的四条边（顺序：上、右、下、左）。</summary>
        public List<HLine> Edges()
        {
            return new List<HLine>
            {
                new HLine(TopLeft, TopRight),
                new HLine(TopRight, BottomRight),
                new HLine(BottomRight, BottomLeft),
                new HLine(BottomLeft, TopLeft)
            };
        }

        /// <summary>矩形与无限直线的交点（最多 2 个点）。</summary>
        public List<HPoint> IntersectionWithLine(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var result = new List<HPoint>();
            foreach (var edge in Edges())
            {
                HPoint inter = line.IntersectionWith(edge);
                if (inter != null && edge.IsPointOnSegment(inter))
                {
                    // 去重（避免顶点的重复）
                    if (!result.Any(p => p == inter))
                        result.Add(inter);
                }
            }
            return result;
        }

        /// <summary>矩形与线段的交点（最多 2 个点）。</summary>
        public List<HPoint> IntersectionWithSegment(HLine segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            var lineIntersections = IntersectionWithLine(segment);
            lineIntersections.RemoveAll(p => !segment.IsPointOnSegment(p));
            return lineIntersections;
        }

        /// <summary>判断圆是否与矩形相交（含边缘）。</summary>
        public bool IntersectsCircle(HCircle circle)
        {
            if (circle == null) throw new ArgumentNullException(nameof(circle));
            return DistanceSquaredToPoint(circle.Center) <= circle.Radius * circle.Radius;
        }

        /// <summary>判断线段是否与矩形相交。</summary>
        public bool IntersectsSegment(HLine segment)
        {
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            if (ContainsPoint(segment.HPointStart) || ContainsPoint(segment.HPointEnd)) return true;
            foreach (var edge in Edges())
            {
                if (segment.IntersectionWithSegment(edge) != null)
                    return true;
            }
            return false;
        }

        #endregion

        #region 多边形与转换

        /// <summary>返回顺时针排列的四个顶点列表。</summary>
        public List<HPoint> ToPolygon()
        {
            return new List<HPoint> { TopLeft, TopRight, BottomRight, BottomLeft };
        }

        /// <summary>
        /// 转换为 GDI+ Rectangle（屏幕坐标系）。
        /// HRectangle 内部用数学坐标系（_top = Max Y, _bottom = Min Y），
        /// GDI Rectangle 的 Y 表示左上角 Y（Min Y），所以必须用 _bottom。
        /// </summary>
        public System.Drawing.Rectangle ToDrawingRectangle()
        {
            int x = (int)Math.Round(_left);
            int y = (int)Math.Round(_bottom);
            int w = (int)Math.Round(Width);
            int h = (int)Math.Round(Height);
            return new System.Drawing.Rectangle(x, y, w, h);
        }

        /// <summary>转换为 GDI+ RectangleF。</summary>
        public System.Drawing.RectangleF ToDrawingRectangleF()
        {
            return new System.Drawing.RectangleF((float)_left, (float)_bottom, (float)Width, (float)Height);
        }

        #endregion

        #region 运算符与重写

        public override bool Equals(object obj)
        {
            return obj is HRectangle r && Equals(r);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _left.GetHashCode();
                hash = hash * 23 + _right.GetHashCode();
                hash = hash * 23 + _top.GetHashCode();
                hash = hash * 23 + _bottom.GetHashCode();
                return hash;
            }
        }

        #region 类型转换（约定：本类型不保留任何隐式转换）

        // 原两个隐式转换已删除（RectangleF/Rectangle），绘图请直接使用
        // ToDrawingRectangleF() / ToDrawingRectangle() 方法；转 HRect 请使用 ToHRect()。

        /// <summary>转换为左上角加宽高形式的 <see cref="HRect"/>。</summary>
        public HRect ToHRect()
        {
            return new HRect(X, Y, Width, Height);
        }

        #endregion

        /// <summary>运算符 ==：用于 相等判定。</summary>
        public static bool operator ==(HRectangle a, HRectangle b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        /// <summary>运算符 !=：用于 不等判定。</summary>
        public static bool operator !=(HRectangle a, HRectangle b) => !(a == b);

        /// <summary>克隆。</summary>
        public HRectangle Clone() => new HRectangle(_left, _top, _right, _bottom);

        public override string ToString() =>
            $"Rect(L={_left:F2}, T={_top:F2}, R={_right:F2}, B={_bottom:F2})";

        #endregion
    }
}
