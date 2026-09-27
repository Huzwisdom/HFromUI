using System;

namespace HFromUI.HMath
{
    /// <summary>
    /// 二维与三维角度计算全能工具（静态类）。
    /// 支持弧度/度互换、规范化、方向角（与 X/Y/Z 轴）、两点连线夹角、三点夹角、
    /// 两线夹角（带符号/无符号/锐角）、直线与平面夹角、平面与平面夹角、方向余弦等。
    /// 所有几何结果均返回 <see cref="HDouble"/>，便于统一处理。
    /// </summary>
    public static class HAngle
    {
        // ---------- 常量 ----------
        /// <summary>每个度对应的弧度值（π/180）。</summary>
        public const double RadiansPerDegree = Math.PI / 180.0;
        /// <summary>每个弧度对应的度值（180/π）。</summary>
        public const double DegreesPerRadian = 180.0 / Math.PI;

        // ---------- 基础转换 ----------
        /// <summary>将度转换为弧度。</summary>
        /// <param name="degrees">角度值（度）。</param>
        public static double ToRadians(double degrees) => degrees * RadiansPerDegree;

        /// <summary>将弧度转换为度。</summary>
        /// <param name="radians">角度值（弧度）。</param>
        public static double ToDegrees(double radians) => radians * DegreesPerRadian;

        // ---------- 规范化 ----------
        /// <summary>
        /// 将弧度规范到 [min, min + 2π) 区间。默认 min = -π，即范围 [-π, π)。
        /// </summary>
        /// <param name="radians">要规范化的弧度值。</param>
        /// <param name="min">区间下限（包含）。</param>
        public static double NormalizeRadians(double radians, double min = -Math.PI)
        {
            double twoPi = 2 * Math.PI;
            double offset = (radians - min) % twoPi;
            if (offset < 0) offset += twoPi;
            return min + offset;
        }

        /// <summary>将弧度规范到 [0, 2π)。</summary>
        /// <param name="radians">要规范化的弧度值。</param>
        public static double NormalizeRadians0To2Pi(double radians) => NormalizeRadians(radians, 0.0);

        /// <summary>将度规范到 [0, 360)。</summary>
        /// <param name="degrees">要规范化的角度值（度）。</param>
        public static double NormalizeDegrees(double degrees)
        {
            degrees %= 360.0;
            if (degrees < 0) degrees += 360.0;
            return degrees;
        }

        // ===================== 二维 =====================
        // ======== 方向角：直线/方向向量/两点连线 与 坐标轴的夹角 ========

        /// <summary>
        /// 二维直线与 X 轴正方向的 有符号夹角（弧度，范围 (-π, π]）。
        /// 等同于从 X 轴正向旋转到直线方向所需的角度，逆时针为正。
        /// </summary>
        /// <param name="line">二维直线（HLine），不可为 null。</param>
        public static HDouble SignedAngleWithXAxis(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return new HDouble { Value = Math.Atan2(line.DeltaY, line.DeltaX) };
        }

        /// <summary>
        /// 二维直线与 Y 轴正方向的 有符号夹角（弧度，范围 (-π, π]）。
        /// 结果为从 Y 轴正向旋转到直线方向所需的角度。
        /// </summary>
        /// <param name="line">二维直线（HLine），不可为 null。</param>
        public static HDouble SignedAngleWithYAxis(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            double angleX = Math.Atan2(line.DeltaY, line.DeltaX);
            double angleY = Math.PI / 2.0 - angleX;
            return new HDouble { Value = NormalizeRadians(angleY) };
        }

        /// <summary>
        /// 二维方向向量与 X 轴正方向的 有符号夹角（弧度，范围 (-π, π]）。
        /// </summary>
        /// <param name="direction">方向向量（HPoint），不可为 null。</param>
        public static HDouble SignedAngleWithXAxis(HPoint direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            return new HDouble { Value = Math.Atan2(direction.Y.Value, direction.X.Value) };
        }

        /// <summary>
        /// 二维方向向量与 Y 轴正方向的 有符号夹角（弧度，范围 (-π, π]）。
        /// </summary>
        /// <param name="direction">方向向量（HPoint），不可为 null。</param>
        public static HDouble SignedAngleWithYAxis(HPoint direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double angleX = Math.Atan2(direction.Y.Value, direction.X.Value);
            double angleY = Math.PI / 2.0 - angleX;
            return new HDouble { Value = NormalizeRadians(angleY) };
        }

        /// <summary>
        /// 二维直线与 X 轴正方向的 无符号夹角（弧度，范围 [0, π]）。
        /// 等于方向向量与 X 轴正向夹角的绝对值（不考虑方向）。
        /// </summary>
        /// <param name="line">二维直线（HLine），不可为 null。</param>
        public static HDouble AngleWithXAxis(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleWithXAxis(new HPoint(line.DeltaX, line.DeltaY));
        }

        /// <summary>
        /// 二维方向向量与 X 轴正方向的 无符号夹角（弧度，范围 [0, π]）。
        /// </summary>
        /// <param name="direction">方向向量（HPoint），不可为 null。</param>
        public static HDouble AngleWithXAxis(HPoint direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double len = direction.Length.Value;
            if (len < 1e-15) return new HDouble { Value = double.NaN };
            double cosAngle = direction.X.Value / len;
            return new HDouble { Value = Math.Acos(Clamp1(cosAngle)) };
        }

        /// <summary>
        /// 二维直线与 Y 轴正方向的 无符号夹角（弧度，范围 [0, π]）。
        /// </summary>
        /// <param name="line">二维直线（HLine），不可为 null。</param>
        public static HDouble AngleWithYAxis(HLine line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleWithYAxis(new HPoint(line.DeltaX, line.DeltaY));
        }

        /// <summary>
        /// 二维方向向量与 Y 轴正方向的 无符号夹角（弧度，范围 [0, π]）。
        /// </summary>
        /// <param name="direction">方向向量（HPoint），不可为 null。</param>
        public static HDouble AngleWithYAxis(HPoint direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double len = direction.Length.Value;
            if (len < 1e-15) return new HDouble { Value = double.NaN };
            double cosAngle = direction.Y.Value / len;
            return new HDouble { Value = Math.Acos(Clamp1(cosAngle)) };
        }

        // --- 度版本（基于 HLine） ---
        /// <summary>二维直线与 X 轴的有符号夹角（度）。</summary>
        public static HDouble SignedAngleWithXAxisDeg(HLine line) =>
            new HDouble { Value = ToDegrees(SignedAngleWithXAxis(line).Value) };
        /// <summary>二维直线与 Y 轴的有符号夹角（度）。</summary>
        public static HDouble SignedAngleWithYAxisDeg(HLine line) =>
            new HDouble { Value = ToDegrees(SignedAngleWithYAxis(line).Value) };
        /// <summary>二维直线与 X 轴的无符号夹角（度）。</summary>
        public static HDouble AngleWithXAxisDeg(HLine line) =>
            new HDouble { Value = ToDegrees(AngleWithXAxis(line).Value) };
        /// <summary>二维直线与 Y 轴的无符号夹角（度）。</summary>
        public static HDouble AngleWithYAxisDeg(HLine line) =>
            new HDouble { Value = ToDegrees(AngleWithYAxis(line).Value) };

        // --- 度版本（基于 HPoint） ---
        /// <summary>二维方向向量与 X 轴的有符号夹角（度）。</summary>
        public static HDouble SignedAngleWithXAxisDeg(HPoint direction) =>
            new HDouble { Value = ToDegrees(SignedAngleWithXAxis(direction).Value) };
        /// <summary>二维方向向量与 Y 轴的有符号夹角（度）。</summary>
        public static HDouble SignedAngleWithYAxisDeg(HPoint direction) =>
            new HDouble { Value = ToDegrees(SignedAngleWithYAxis(direction).Value) };
        /// <summary>二维方向向量与 X 轴的无符号夹角（度）。</summary>
        public static HDouble AngleWithXAxisDeg(HPoint direction) =>
            new HDouble { Value = ToDegrees(AngleWithXAxis(direction).Value) };
        /// <summary>二维方向向量与 Y 轴的无符号夹角（度）。</summary>
        public static HDouble AngleWithYAxisDeg(HPoint direction) =>
            new HDouble { Value = ToDegrees(AngleWithYAxis(direction).Value) };

        // ======== 两点连线夹角 ========

        /// <summary>
        /// 二维中从起点指向终点的向量与 X 轴正方向的有符号夹角（弧度）。
        /// </summary>
        /// <param name="from">起点（HPoint），不可为 null。</param>
        /// <param name="to">终点（HPoint），不可为 null。</param>
        public static HDouble SignedAngleWithXAxis(HPoint from, HPoint to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            double dx = to.X.Value - from.X.Value;
            double dy = to.Y.Value - from.Y.Value;
            return new HDouble { Value = Math.Atan2(dy, dx) };
        }

        /// <summary>二维两点连线与 Y 轴正方向的有符号夹角（弧度）。</summary>
        public static HDouble SignedAngleWithYAxis(HPoint from, HPoint to)
        {
            double angleX = SignedAngleWithXAxis(from, to).Value;
            return new HDouble { Value = NormalizeRadians(Math.PI / 2.0 - angleX) };
        }

        /// <summary>二维两点连线与 X 轴正方向的无符号夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleWithXAxis(HPoint from, HPoint to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return AngleWithXAxis(new HPoint(to.X.Value - from.X.Value, to.Y.Value - from.Y.Value));
        }

        /// <summary>二维两点连线与 Y 轴正方向的无符号夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleWithYAxis(HPoint from, HPoint to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return AngleWithYAxis(new HPoint(to.X.Value - from.X.Value, to.Y.Value - from.Y.Value));
        }

        // 度版本
        /// <summary>二维两点连线与 X 轴的有符号夹角（度）。</summary>
        public static HDouble SignedAngleWithXAxisDeg(HPoint from, HPoint to) =>
            new HDouble { Value = ToDegrees(SignedAngleWithXAxis(from, to).Value) };
        /// <summary>二维两点连线与 Y 轴的有符号夹角（度）。</summary>
        public static HDouble SignedAngleWithYAxisDeg(HPoint from, HPoint to) =>
            new HDouble { Value = ToDegrees(SignedAngleWithYAxis(from, to).Value) };
        /// <summary>二维两点连线与 X 轴的无符号夹角（度）。</summary>
        public static HDouble AngleWithXAxisDeg(HPoint from, HPoint to) =>
            new HDouble { Value = ToDegrees(AngleWithXAxis(from, to).Value) };
        /// <summary>二维两点连线与 Y 轴的无符号夹角（度）。</summary>
        public static HDouble AngleWithYAxisDeg(HPoint from, HPoint to) =>
            new HDouble { Value = ToDegrees(AngleWithYAxis(from, to).Value) };

        // ======== 三点夹角（以中间点为顶点） ========
        /// <summary>
        /// 二维三点构成的夹角（点1-顶点-点2），即向量 (点1→顶点) 与 (点2→顶点) 之间的无符号夹角。
        /// 返回弧度，范围 [0, π]。
        /// </summary>
        /// <param name="vertex">角的顶点（HPoint），不可为 null。</param>
        /// <param name="point1">第一条边上的点（HPoint），不可为 null。</param>
        /// <param name="point2">第二条边上的点（HPoint），不可为 null。</param>
        public static HDouble AngleBetweenThreePoints(HPoint vertex, HPoint point1, HPoint point2)
        {
            if (vertex == null) throw new ArgumentNullException(nameof(vertex));
            if (point1 == null) throw new ArgumentNullException(nameof(point1));
            if (point2 == null) throw new ArgumentNullException(nameof(point2));
            var v1 = new HPoint(point1.X.Value - vertex.X.Value, point1.Y.Value - vertex.Y.Value);
            var v2 = new HPoint(point2.X.Value - vertex.X.Value, point2.Y.Value - vertex.Y.Value);
            return AngleBetweenDirections(v1, v2);
        }

        /// <summary>二维三点夹角的度数（度，范围 [0, 180]）。</summary>
        public static HDouble AngleBetweenThreePointsDeg(HPoint vertex, HPoint point1, HPoint point2) =>
            new HDouble { Value = ToDegrees(AngleBetweenThreePoints(vertex, point1, point2).Value) };

        // ======== 两线 / 两方向向量夹角 ========

        /// <summary>
        /// 二维中从直线1方向到直线2方向的 有符号夹角（弧度，范围 (-π, π]）。
        /// 正值为逆时针旋转角度。
        /// </summary>
        /// <param name="line1">起始直线（HLine），不可为 null。</param>
        /// <param name="line2">目标直线（HLine），不可为 null。</param>
        public static HDouble SignedAngleBetweenLines(HLine line1, HLine line2)
        {
            if (line1 == null) throw new ArgumentNullException(nameof(line1));
            if (line2 == null) throw new ArgumentNullException(nameof(line2));
            var v1 = new HPoint(line1.DeltaX, line1.DeltaY);
            var v2 = new HPoint(line2.DeltaX, line2.DeltaY);
            return SignedAngleBetweenDirections(v1, v2);
        }

        /// <summary>
        /// 两个二维方向向量之间的 有符号夹角（弧度，范围 (-π, π]）。
        /// </summary>
        /// <param name="dir1">第一个方向向量（HPoint），不可为 null。</param>
        /// <param name="dir2">第二个方向向量（HPoint），不可为 null。</param>
        public static HDouble SignedAngleBetweenDirections(HPoint dir1, HPoint dir2)
        {
            if (dir1 == null) throw new ArgumentNullException(nameof(dir1));
            if (dir2 == null) throw new ArgumentNullException(nameof(dir2));
            double cross = dir1.X.Value * dir2.Y.Value - dir1.Y.Value * dir2.X.Value;
            double dot = dir1.X.Value * dir2.X.Value + dir1.Y.Value * dir2.Y.Value;
            return new HDouble { Value = Math.Atan2(cross, dot) };
        }

        /// <summary>
        /// 二维两条直线方向之间的 无符号夹角（弧度，范围 [0, π]）。
        /// </summary>
        /// <param name="line1">第一条直线（HLine），不可为 null。</param>
        /// <param name="line2">第二条直线（HLine），不可为 null。</param>
        public static HDouble AngleBetweenLines(HLine line1, HLine line2)
        {
            if (line1 == null) throw new ArgumentNullException(nameof(line1));
            if (line2 == null) throw new ArgumentNullException(nameof(line2));
            var v1 = new HPoint(line1.DeltaX, line1.DeltaY);
            var v2 = new HPoint(line2.DeltaX, line2.DeltaY);
            return AngleBetweenDirections(v1, v2);
        }

        /// <summary>
        /// 两个二维方向向量之间的 无符号夹角（弧度，范围 [0, π]）。
        /// </summary>
        /// <param name="dir1">第一个方向向量（HPoint），不可为 null。</param>
        /// <param name="dir2">第二个方向向量（HPoint），不可为 null。</param>
        public static HDouble AngleBetweenDirections(HPoint dir1, HPoint dir2)
        {
            if (dir1 == null) throw new ArgumentNullException(nameof(dir1));
            if (dir2 == null) throw new ArgumentNullException(nameof(dir2));
            double len1 = dir1.Length.Value, len2 = dir2.Length.Value;
            if (len1 < 1e-15 || len2 < 1e-15) return new HDouble { Value = double.NaN };
            double dot = dir1.Dot(dir2).Value / (len1 * len2);
            return new HDouble { Value = Math.Acos(Clamp1(dot)) };
        }

        /// <summary>
        /// 二维两条直线方向之间的 锐角（弧度，范围 [0, π/2]）。
        /// 如果夹角超过 90 度，则返回其补角。
        /// </summary>
        public static HDouble AcuteAngleBetweenLines(HLine line1, HLine line2)
        {
            if (line1 == null) throw new ArgumentNullException(nameof(line1));
            if (line2 == null) throw new ArgumentNullException(nameof(line2));
            var v1 = new HPoint(line1.DeltaX, line1.DeltaY);
            var v2 = new HPoint(line2.DeltaX, line2.DeltaY);
            return AcuteAngleBetweenDirections(v1, v2);
        }

        /// <summary>
        /// 两个二维方向向量之间的 锐角（弧度，范围 [0, π/2]）。
        /// </summary>
        public static HDouble AcuteAngleBetweenDirections(HPoint dir1, HPoint dir2)
        {
            var angle = AngleBetweenDirections(dir1, dir2);
            double val = angle.Value;
            if (double.IsNaN(val)) return angle;
            if (val > Math.PI * 0.5) val = Math.PI - val;
            return new HDouble { Value = val };
        }

        // ---- 度版本（二维两线/向量） ----
        /// <summary>二维两直线有符号夹角（度）。</summary>
        public static HDouble SignedAngleBetweenLinesDeg(HLine line1, HLine line2) =>
            new HDouble { Value = ToDegrees(SignedAngleBetweenLines(line1, line2).Value) };
        /// <summary>二维两直线无符号夹角（度）。</summary>
        public static HDouble AngleBetweenLinesDeg(HLine line1, HLine line2) =>
            new HDouble { Value = ToDegrees(AngleBetweenLines(line1, line2).Value) };
        /// <summary>二维两直线锐角（度）。</summary>
        public static HDouble AcuteAngleBetweenLinesDeg(HLine line1, HLine line2) =>
            new HDouble { Value = ToDegrees(AcuteAngleBetweenLines(line1, line2).Value) };

        // ===================== 三维 =====================
        // ======== 方向角：直线/方向向量/两点连线 与 X/Y/Z 轴 ========

        /// <summary>三维直线与 X 轴正方向的夹角（弧度，范围 [0, π]）。</summary>
        public static HDouble AngleWithXAxis(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleWithXAxis(line.UnitDirection);
        }

        /// <summary>三维直线与 Y 轴正方向的夹角（弧度，范围 [0, π]）。</summary>
        public static HDouble AngleWithYAxis(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleWithYAxis(line.UnitDirection);
        }

        /// <summary>三维直线与 Z 轴正方向的夹角（弧度，范围 [0, π]）。</summary>
        public static HDouble AngleWithZAxis(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleWithZAxis(line.UnitDirection);
        }

        /// <summary>三维方向向量与 X 轴正方向的夹角（弧度，范围 [0, π]）。</summary>
        public static HDouble AngleWithXAxis(HPoint3D direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double len = direction.Length.Value;
            if (len < 1e-15) return new HDouble { Value = double.NaN };
            return new HDouble { Value = Math.Acos(Clamp1(direction.X.Value / len)) };
        }

        /// <summary>三维方向向量与 Y 轴正方向的夹角（弧度，范围 [0, π]）。</summary>
        public static HDouble AngleWithYAxis(HPoint3D direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double len = direction.Length.Value;
            if (len < 1e-15) return new HDouble { Value = double.NaN };
            return new HDouble { Value = Math.Acos(Clamp1(direction.Y.Value / len)) };
        }

        /// <summary>三维方向向量与 Z 轴正方向的夹角（弧度，范围 [0, π]）。</summary>
        public static HDouble AngleWithZAxis(HPoint3D direction)
        {
            if (direction == null) throw new ArgumentNullException(nameof(direction));
            double len = direction.Length.Value;
            if (len < 1e-15) return new HDouble { Value = double.NaN };
            return new HDouble { Value = Math.Acos(Clamp1(direction.Z.Value / len)) };
        }

        // 度版本
        /// <summary>三维直线与 X 轴的夹角（度）。</summary>
        public static HDouble AngleWithXAxisDeg(HLine3D line) =>
            new HDouble { Value = ToDegrees(AngleWithXAxis(line).Value) };
        /// <summary>三维直线与 Y 轴的夹角（度）。</summary>
        public static HDouble AngleWithYAxisDeg(HLine3D line) =>
            new HDouble { Value = ToDegrees(AngleWithYAxis(line).Value) };
        /// <summary>三维直线与 Z 轴的夹角（度）。</summary>
        public static HDouble AngleWithZAxisDeg(HLine3D line) =>
            new HDouble { Value = ToDegrees(AngleWithZAxis(line).Value) };

        // ======== 三维两点连线与坐标轴夹角 ========
        /// <summary>三维两点连线与 X 轴正方向的夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleWithXAxis(HPoint3D from, HPoint3D to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return AngleWithXAxis(to.Subtract(from));
        }

        /// <summary>三维两点连线与 Y 轴正方向的夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleWithYAxis(HPoint3D from, HPoint3D to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return AngleWithYAxis(to.Subtract(from));
        }

        /// <summary>三维两点连线与 Z 轴正方向的夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleWithZAxis(HPoint3D from, HPoint3D to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return AngleWithZAxis(to.Subtract(from));
        }

        /// <summary>三维两点连线与 X 轴的夹角（度）。</summary>
        public static HDouble AngleWithXAxisDeg(HPoint3D from, HPoint3D to) =>
            new HDouble { Value = ToDegrees(AngleWithXAxis(from, to).Value) };
        /// <summary>三维两点连线与 Y 轴的夹角（度）。</summary>
        public static HDouble AngleWithYAxisDeg(HPoint3D from, HPoint3D to) =>
            new HDouble { Value = ToDegrees(AngleWithYAxis(from, to).Value) };
        /// <summary>三维两点连线与 Z 轴的夹角（度）。</summary>
        public static HDouble AngleWithZAxisDeg(HPoint3D from, HPoint3D to) =>
            new HDouble { Value = ToDegrees(AngleWithZAxis(from, to).Value) };

        // ======== 三维三点夹角 ========
        /// <summary>
        /// 三维三点夹角（点1-顶点-点2），即向量 (点1→顶点) 与 (点2→顶点) 之间的无符号夹角。
        /// 返回弧度，范围 [0, π]。
        /// </summary>
        /// <param name="vertex">角的顶点（HPoint3D），不可为 null。</param>
        /// <param name="point1">第一条边上的点（HPoint3D），不可为 null。</param>
        /// <param name="point2">第二条边上的点（HPoint3D），不可为 null。</param>
        public static HDouble AngleBetweenThreePoints(HPoint3D vertex, HPoint3D point1, HPoint3D point2)
        {
            if (vertex == null) throw new ArgumentNullException(nameof(vertex));
            if (point1 == null) throw new ArgumentNullException(nameof(point1));
            if (point2 == null) throw new ArgumentNullException(nameof(point2));
            var v1 = point1.Subtract(vertex);
            var v2 = point2.Subtract(vertex);
            return AngleBetweenDirections(v1, v2);
        }

        /// <summary>三维三点夹角的度数（度，范围 [0, 180]）。</summary>
        public static HDouble AngleBetweenThreePointsDeg(HPoint3D vertex, HPoint3D point1, HPoint3D point2) =>
            new HDouble { Value = ToDegrees(AngleBetweenThreePoints(vertex, point1, point2).Value) };

        // ======== 三维两线夹角 ========
        /// <summary>三维两条直线方向之间的无符号夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleBetweenLines(HLine3D line1, HLine3D line2)
        {
            if (line1 == null) throw new ArgumentNullException(nameof(line1));
            if (line2 == null) throw new ArgumentNullException(nameof(line2));
            return AngleBetweenDirections(line1.UnitDirection, line2.UnitDirection);
        }

        /// <summary>两个三维方向向量之间的无符号夹角（弧度，[0, π]）。</summary>
        public static HDouble AngleBetweenDirections(HPoint3D dir1, HPoint3D dir2)
        {
            if (dir1 == null) throw new ArgumentNullException(nameof(dir1));
            if (dir2 == null) throw new ArgumentNullException(nameof(dir2));
            double len1 = dir1.Length.Value, len2 = dir2.Length.Value;
            if (len1 < 1e-15 || len2 < 1e-15) return new HDouble { Value = double.NaN };
            // HPoint3D 的 Dot 直接返回 double，无需 .Value
            double dot = dir1.Dot(dir2) / (len1 * len2);
            return new HDouble { Value = Math.Acos(Clamp1(dot)) };
        }

        /// <summary>三维两条直线的锐角（弧度，[0, π/2]）。</summary>
        public static HDouble AcuteAngleBetweenLines(HLine3D line1, HLine3D line2)
        {
            var angle = AngleBetweenLines(line1, line2);
            double val = angle.Value;
            if (double.IsNaN(val)) return angle;
            if (val > Math.PI * 0.5) val = Math.PI - val;
            return new HDouble { Value = val };
        }

        /// <summary>三维两个方向向量的锐角（弧度，[0, π/2]）。</summary>
        public static HDouble AcuteAngleBetweenDirections(HPoint3D dir1, HPoint3D dir2)
        {
            var angle = AngleBetweenDirections(dir1, dir2);
            double val = angle.Value;
            if (double.IsNaN(val)) return angle;
            if (val > Math.PI * 0.5) val = Math.PI - val;
            return new HDouble { Value = val };
        }

        // 度版本
        /// <summary>三维两直线夹角（度）。</summary>
        public static HDouble AngleBetweenLinesDeg(HLine3D line1, HLine3D line2) =>
            new HDouble { Value = ToDegrees(AngleBetweenLines(line1, line2).Value) };
        /// <summary>三维两直线锐角（度）。</summary>
        public static HDouble AcuteAngleBetweenLinesDeg(HLine3D line1, HLine3D line2) =>
            new HDouble { Value = ToDegrees(AcuteAngleBetweenLines(line1, line2).Value) };

        // ======== 三维：直线与平面的夹角 ========
        /// <summary>
        /// 三维直线与平面之间的夹角（弧度，范围 [0, π/2]）。
        /// 平面由一点和法向量定义。夹角定义为直线方向与平面法向量夹角的余角。
        /// </summary>
        /// <param name="line">三维直线（HLine3D），不可为 null。</param>
        /// <param name="planeNormal">平面法向量（HPoint3D），不可为 null，无需归一化。</param>
        public static HDouble AngleBetweenLineAndPlane(HLine3D line, HPoint3D planeNormal)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (planeNormal == null) throw new ArgumentNullException(nameof(planeNormal));
            var lineDir = line.UnitDirection;
            double lenN = planeNormal.Length.Value;
            if (lenN < 1e-15 || lineDir.Length < 1e-15) return new HDouble { Value = double.NaN };

            double cosTheta = Math.Abs(lineDir.Dot(planeNormal)) / lenN; // 法向量与线夹角的余弦
            double acute = Math.PI / 2.0 - Math.Acos(Clamp1(cosTheta)); // 余角
            return new HDouble { Value = acute };
        }

        /// <summary>三维直线与平面夹角的度数（度，范围 [0, 90]）。</summary>
        public static HDouble AngleBetweenLineAndPlaneDeg(HLine3D line, HPoint3D planeNormal) =>
            new HDouble { Value = ToDegrees(AngleBetweenLineAndPlane(line, planeNormal).Value) };

        /// <summary>
        /// 三维直线与 XY 平面的夹角（弧度，范围 [0, π/2]）。XY 平面法向量为 (0,0,1)。
        /// </summary>
        public static HDouble AngleWithXYPlane(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleBetweenLineAndPlane(line, new HPoint3D(0, 0, 1));
        }

        /// <summary>
        /// 三维直线与 XZ 平面的夹角（弧度，范围 [0, π/2]）。XZ 平面法向量为 (0,1,0)。
        /// </summary>
        public static HDouble AngleWithXZPlane(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleBetweenLineAndPlane(line, new HPoint3D(0, 1, 0));
        }

        /// <summary>
        /// 三维直线与 YZ 平面的夹角（弧度，范围 [0, π/2]）。YZ 平面法向量为 (1,0,0)。
        /// </summary>
        public static HDouble AngleWithYZPlane(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            return AngleBetweenLineAndPlane(line, new HPoint3D(1, 0, 0));
        }

        /// <summary>三维直线与 XY 平面的夹角（度）。</summary>
        public static HDouble AngleWithXYPlaneDeg(HLine3D line) =>
            new HDouble { Value = ToDegrees(AngleWithXYPlane(line).Value) };
        /// <summary>三维直线与 XZ 平面的夹角（度）。</summary>
        public static HDouble AngleWithXZPlaneDeg(HLine3D line) =>
            new HDouble { Value = ToDegrees(AngleWithXZPlane(line).Value) };
        /// <summary>三维直线与 YZ 平面的夹角（度）。</summary>
        public static HDouble AngleWithYZPlaneDeg(HLine3D line) =>
            new HDouble { Value = ToDegrees(AngleWithYZPlane(line).Value) };

        // ======== 三维：平面与平面的夹角（二面角） ========
        /// <summary>
        /// 两个平面之间的夹角（弧度，范围 [0, π/2]）。
        /// 平面由各自的法向量定义。夹角为两法向量夹角的锐角（或补角中的锐角）。
        /// </summary>
        /// <param name="normal1">第一个平面的法向量（HPoint3D），不可为 null。</param>
        /// <param name="normal2">第二个平面的法向量（HPoint3D），不可为 null。</param>
        public static HDouble AngleBetweenPlanes(HPoint3D normal1, HPoint3D normal2)
        {
            if (normal1 == null) throw new ArgumentNullException(nameof(normal1));
            if (normal2 == null) throw new ArgumentNullException(nameof(normal2));
            double len1 = normal1.Length.Value, len2 = normal2.Length.Value;
            if (len1 < 1e-15 || len2 < 1e-15) return new HDouble { Value = double.NaN };
            double dot = Math.Abs(normal1.Dot(normal2)) / (len1 * len2);
            double angle = Math.Acos(Clamp1(dot));
            if (angle > Math.PI * 0.5) angle = Math.PI - angle; // 取锐角
            return new HDouble { Value = angle };
        }

        /// <summary>两平面夹角的度数（度，范围 [0, 90]）。</summary>
        public static HDouble AngleBetweenPlanesDeg(HPoint3D normal1, HPoint3D normal2) =>
            new HDouble { Value = ToDegrees(AngleBetweenPlanes(normal1, normal2).Value) };

        // ======== 三维方向余弦 ========
        /// <summary>
        /// 三维直线方向向量的方向余弦 (cosα, cosβ, cosγ)，即与 X/Y/Z 轴夹角的余弦值。
        /// 若直线退化为点则返回 (0,0,0)。
        /// </summary>
        public static (double cosX, double cosY, double cosZ) DirectionCosines(HLine3D line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var dir = line.UnitDirection;
            return (dir.X.Value, dir.Y.Value, dir.Z.Value);
        }

        // ---------- 辅助 ----------
        /// <summary>将值限制在 [-1, 1] 区间，防止浮点误差导致反三角函数出错。</summary>
        private static double Clamp1(double x) => Math.Max(-1.0, Math.Min(1.0, x));
    }
}