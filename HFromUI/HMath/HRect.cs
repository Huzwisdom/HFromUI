using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HFromUI.HMath
{
    /// <summary>
    /// 矩形区域（可变引用类型），以左上角坐标 (X, Y) 加宽高 (Width, Height) 表示。
    /// 坐标与尺寸均使用 <see cref="HDouble"/> 包装，便于与 HFromUI 数值体系混用。
    /// </summary>
    public class HRect
    {
        /// <summary>无参构造，默认所有分量为 0。</summary>
        public HRect()
        { }

        /// <summary>
        /// 使用左上角坐标与宽高构造矩形。
        /// </summary>
        /// <param name="x">左上角 X 坐标。</param>
        /// <param name="y">左上角 Y 坐标。</param>
        /// <param name="w">矩形宽度。</param>
        /// <param name="h">矩形高度。</param>
        public HRect(HDouble x, HDouble y, HDouble w, HDouble h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        /// <summary>左上角 X 坐标。</summary>
        public HDouble X { get; set; }

        /// <summary>左上角 Y 坐标。</summary>
        public HDouble Y { get; set; }

        /// <summary>矩形宽度。</summary>
        public HDouble Width { get; set; }

        /// <summary>矩形高度。</summary>
        public HDouble Height { get; set; }

        #region 类型转换（约定：仅保留唯一一个隐式转换）

        /// <summary>
        /// 唯一保留的隐式转换：将 GDI+ 的 <see cref="System.Drawing.RectangleF"/> 装箱为 HRect。
        /// 其余方向的转换一律使用下面的 ToXxx()/FromXxx() 方法显式完成。
        /// </summary>
        /// <param name="p">要封装的浮点矩形。</param>
        public static implicit operator HRect(System.Drawing.RectangleF p)
            => new HRect(p.X, p.Y, p.Width, p.Height);

        // 以下原隐式转换已删除，改用对应方法：
        //   implicit RectangleF(HRect)  → ToRectangleF()
        //   implicit Rectangle(HRect)   → ToRectangle()
        //   implicit HRectangle(HRect)  → ToHRectangle()
        //   implicit HRect(Rectangle)   → FromRectangle()
        //   implicit HRect(HRectangle)  → FromHRectangle()

        /// <summary>转换为 GDI+ 浮点矩形 <see cref="System.Drawing.RectangleF"/>。</summary>
        public System.Drawing.RectangleF ToRectangleF()
            => new System.Drawing.RectangleF(X.ToSingle(), Y.ToSingle(), Width.ToSingle(), Height.ToSingle());

        /// <summary>转换为 GDI 整数矩形 <see cref="System.Drawing.Rectangle"/>（各分量取整）。</summary>
        public System.Drawing.Rectangle ToRectangle()
            => new System.Drawing.Rectangle(X.ToInt(), Y.ToInt(), Width.ToInt(), Height.ToInt());

        /// <summary>
        /// 转换为两点式矩形 <see cref="HRectangle"/>：
        /// 左上角 (X, Y)，右下角 (X + Width, Y + Height)。
        /// </summary>
        public HRectangle ToHRectangle()
            => new HRectangle(X.Value, Y.Value, (X + Width).Value, (Y + Height).Value);

        /// <summary>从 GDI 整数矩形 <see cref="System.Drawing.Rectangle"/> 创建 HRect。</summary>
        /// <param name="p">整数矩形。</param>
        public static HRect FromRectangle(System.Drawing.Rectangle p)
            => new HRect(p.X, p.Y, p.Width, p.Height);

        /// <summary>
        /// 从两点式矩形 <see cref="HRectangle"/> 创建 HRect。
        /// 注意：HRectangle 以两点表示，转换后 Width/Height 为两点坐标之差。
        /// </summary>
        /// <param name="p">两点式矩形。</param>
        public static HRect FromHRectangle(HRectangle p)
            => new HRect(p.X.Value, p.Y.Value, p.Width, p.Height);

        #endregion
    }
}
