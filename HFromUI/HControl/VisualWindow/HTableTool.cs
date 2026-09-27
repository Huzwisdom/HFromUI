using System;

namespace HFromUI.HControl.VisualWindow
{
    using HFromUI.HLangage;
    /// <summary>
    /// 视觉窗口（HVisualWindowA）交互工具类型。
    /// 参考 Halcon Vision / OpenCV / NI Vision / Cognex VisionPro 等业界主流视觉软件的画图工具，
    /// 覆盖点/线/面/标注/测量/自由画刷/橡皮擦 全品类。
    /// None=平移浏览、Select=选择编辑、其余为各类 ROI 画图工具。
    /// </summary>
    [Serializable]
    public enum HTableTool
    {
        /// <summary>无工具：左键拖拽平移视图、滚轮缩放（浏览模式）</summary>
        None = 0,

        /// <summary>选择工具：点选图形、拖拽移动、拖拽锚点修改、Delete 删除、Shift 多选</summary>
        Select = -1,

        // ======== 基础几何 ========
        /// <summary>点：单击放置一个特征点</summary>
        Point = 1,

        /// <summary>直线：两点单击 或 按下拖拽</summary>
        Line = 2,

        /// <summary>矩形（轴对齐）：按下拖拽对角点，框选 ROI</summary>
        Rectangle = 3,

        /// <summary>圆：按下定圆心、拖拽定半径</summary>
        Circle = 4,

        /// <summary>椭圆（轴对齐）：按下拖拽外接矩形</summary>
        Ellipse = 5,

        /// <summary>三点圆弧：依次单击 起点、弧上一点、终点</summary>
        Arc3P = 6,

        /// <summary>折线：逐点单击，双击完成</summary>
        Polyline = 7,

        /// <summary>多边形（闭合）：逐点单击，双击闭合</summary>
        Polygon = 8,

        /// <summary>旋转矩形：先拖拽一条边（定中心/角度/长度），再单击定宽度</summary>
        RotatedRectangle = 9,

        // ======== 扩展几何（全网最全） ========
        /// <summary>圆环（Annulus）：圆心 + 内半径 + 外半径。点击圆心后拖拽外圈，再点击内圈半径。</summary>
        Annulus = 10,

        /// <summary>扇形（Sector/Pie）：圆心 + 半径 + 起始角 + 扫掠角。点击圆心→拖拽定半径→点击起始角→点击扫掠角。</summary>
        Sector = 11,

        /// <summary>圆环扇形（AnnulusSector）：圆环 + 扇形的交集区域</summary>
        AnnulusSector = 12,

        /// <summary>三次贝塞尔曲线：4 个控制点</summary>
        Bezier = 13,

        // ======== 标注工具 ========
        /// <summary>箭头：起点 + 终点（带箭头三角），可叠加文字标注</summary>
        Arrow = 20,

        /// <summary>文字标注：单击位置放置文字（可带引线锚点）</summary>
        Text = 21,

        /// <summary>十字标记（大十字准星）：特征点定位，比 Point 显示更醒目</summary>
        Cross = 22,

        /// <summary>点+方框（PointRect）：中心点 + 外圈方框标记</summary>
        PointRect = 23,

        /// <summary>双圆靶心（Bullseye）：同心双圆，用于对准中心</summary>
        Bullseye = 24,

        // ======== 自由画刷/圈图 ========
        /// <summary>自由曲线（Freehand）：按下拖拽自动追加点，松开完成。可做 S 曲线、签名等。</summary>
        Freehand = 30,

        /// <summary>套索（Lasso）：按下拖拽任意形状，松开自动闭合为多边形 ROI</summary>
        Lasso = 31,

        /// <summary>画笔遮罩（Mask）：像刷子一样拖拽涂满一片区域，生成填充多边形（默认闭合+填充模式）</summary>
        Mask = 32,

        /// <summary>橡皮擦（Eraser）：按下拖拽像橡皮一样圈住要删除的区域，松开后对选中图形执行减法运算挖掉一块</summary>
        Eraser = 33,

        // ======== 测量 ========
        /// <summary>卡尺（Caliper）：两条平行直线 + 一个方向箭头，实时显示间距和角度</summary>
        Caliper = 40,

        /// <summary>角度测量：三条点构成 V 型，显示夹角（内角/外角）</summary>
        Angle = 41,
    }
}
