using HalconDotNet;
using System;
using System.Windows.Forms;

namespace HFromUI.HCamera.HHalcon
{
    using HFromUI.HLangage;

    /// <summary>
    /// HALCON 图形窗口封装：可创建离屏缓冲窗口（buffer）、绑定到 WinForms 控件句柄的可见窗口，
    /// 提供图像/区域/轮廓显示（disp_obj）、文本叠加（disp_text）、颜色与绘制模式设置、
    /// ROI 基本图元绘制（线/矩形/圆/十字/箭头）、自适应图像尺寸（set_part）以及窗口截图（dump_window_image）。
    /// 多线程显示时请在 UI 线程调用。
    /// </summary>
    public class HHalconWindow : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>HALCON 窗口句柄（HTuple 包装）；未开窗时为空元组。</summary>
        public HTuple Handle { get; private set; } = new HTuple();

        /// <summary>窗口是否已打开。</summary>
        public bool IsOpen
        {
            get
            {
                try { return Handle != null && Handle.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 构造与开窗 ====================

        /// <summary>创建封装对象但暂不开窗，随后调用 <see cref="OpenBuffer"/> 或 <see cref="AttachControl"/>。</summary>
        public HHalconWindow()
        {
        }

        /// <summary>
        /// 创建并打开一个窗口：fatherHandle 为零/负数时打开离屏缓冲窗口（buffer，可截图但不可见），
        /// 为有效窗口句柄时作为子窗口嵌入显示（visible）。
        /// </summary>
        /// <param name="width">窗口宽（像素）。</param>
        /// <param name="height">窗口高（像素）。</param>
        /// <param name="fatherHandle">父窗口句柄（控件 Handle）；IntPtr.Zero 表示离屏缓冲。</param>
        /// <param name="row">窗口在父窗口中的起始行。</param>
        /// <param name="column">窗口在父窗口中的起始列。</param>
        public HHalconWindow(int width, int height, IntPtr fatherHandle, int row = 0, int column = 0)
        {
            if (fatherHandle == IntPtr.Zero) OpenBuffer(width, height);
            else AttachControl(fatherHandle, width, height, row, column);
        }

        /// <summary>
        /// 打开离屏缓冲窗口（open_window 的 father='buffer' 模式），
        /// 适合无界面服务端做渲染截图；返回是否成功。
        /// </summary>
        /// <param name="width">缓冲宽。</param>
        /// <param name="height">缓冲高。</param>
        public bool OpenBuffer(int width, int height)
        {
            Close();
            return SafeRun(() =>
            {
                HTuple win;
                HOperatorSet.OpenWindow(0, 0, width, height, "buffer", "visible", string.Empty, out win);
                Handle = win;
            });
        }

        /// <summary>
        /// 把 HALCON 窗口作为子窗口嵌入到指定 WinForms 控件/窗口（open_window father=控件句柄，visible 模式）。
        /// 调用前控件应已完成句柄创建（IsHandleCreated=true）。
        /// </summary>
        /// <param name="controlHandle">宿主控件句柄（Control.Handle）。</param>
        /// <param name="width">窗口宽（通常传控件 ClientSize.Width）。</param>
        /// <param name="height">窗口高（通常传控件 ClientSize.Height）。</param>
        /// <param name="row">起始行。</param>
        /// <param name="column">起始列。</param>
        public bool AttachControl(IntPtr controlHandle, int width, int height, int row = 0, int column = 0)
        {
            Close();
            return SafeRun(() =>
            {
                HTuple win;
                HOperatorSet.OpenWindow(row, column, width, height, controlHandle.ToInt64(), "visible", string.Empty, out win);
                Handle = win;
            });
        }

        /// <summary>便捷重载：直接绑定到 WinForms 控件客户区。</summary>
        public bool AttachControl(Control control)
        {
            if (control == null)
            {
                Error = HTranslation.GetContent("宿主控件为 null");
                return false;
            }
            if (!control.IsHandleCreated)
            {
                // 强制创建句柄，保证 open_window 能挂载
                IntPtr dummy = control.Handle;
            }
            return AttachControl(control.Handle, control.ClientSize.Width, control.ClientSize.Height);
        }

        /// <summary>关闭并销毁 HALCON 窗口（已关闭时安全跳过）。</summary>
        public void Close()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsOpen)
                    {
                        try { new HWindow(Handle.H).Dispose(); } catch { }
                        Handle = new HTuple();
                    }
                }
                catch { }
            }
        }

        #endregion

        #region ==================== 显示控制 ====================

        /// <summary>清空窗口（clear_window），用当前背景色填充。</summary>
        public bool Clear()
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.ClearWindow(Handle));
        }

        /// <summary>
        /// 设置显示坐标系（set_part），使后续图像按给定图像范围等比适配窗口；
        /// 传入图像宽高即可实现整图自适应。
        /// </summary>
        /// <param name="imageWidth">图像宽。</param>
        /// <param name="imageHeight">图像高。</param>
        public bool SetPart(int imageWidth, int imageHeight)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.SetPart(Handle, 0, 0, imageHeight - 1, imageWidth - 1));
        }

        /// <summary>显示一个 HALCON 对象（HImage/HRegion/HXLD/HShapeModel 轮廓等）。</summary>
        /// <param name="obj">待显示对象。</param>
        public bool DispObject(HObject obj)
        {
            if (!IsOpen || obj == null) return false;
            return SafeRun(() => HOperatorSet.DispObj(obj, Handle));
        }

        /// <summary>
        /// 先按图像尺寸 set_part 再显示图像，等效于“自适应铺满”。
        /// </summary>
        /// <param name="image">待显示图像。</param>
        public bool ShowImage(HObject image)
        {
            if (!IsOpen || image == null) return false;
            return SafeRun(() =>
            {
                HTuple w, h;
                HOperatorSet.GetImageSize(image, out w, out h);
                HOperatorSet.SetPart(Handle, 0, 0, h - 1, w - 1);
                HOperatorSet.DispObj(image, Handle);
            });
        }

        /// <summary>在窗口上输出一行文本（disp_text）。</summary>
        /// <param name="text">文本内容。</param>
        /// <param name="row">起始行（像素坐标或窗口坐标，由 coordSystem 决定）。</param>
        /// <param name="column">起始列。</param>
        /// <param name="color">颜色名（如 "red"），null 用当前色。</param>
        /// <param name="coordSystem">"window"=窗口坐标（默认），"image"=图像坐标。</param>
        public bool DispText(string text, double row = 12, double column = 12, string color = "yellow", string coordSystem = "window")
        {
            if (!IsOpen) return false;
            return SafeRun(() =>
                HOperatorSet.DispText(Handle, text ?? string.Empty, coordSystem, row, column,
                    color ?? string.Empty, new HTuple(), new HTuple()));
        }

        /// <summary>设置后续绘图颜色（set_color），如 "red"、"lime green"。</summary>
        public bool SetColor(string color)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.SetColor(Handle, color ?? "white"));
        }

        /// <summary>设置多对象循环配色数量（set_colored，如 6/12）。</summary>
        public bool SetColored(int colorCount)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.SetColored(Handle, colorCount));
        }

        /// <summary>设置区域绘制模式（set_draw）："margin" 只画轮廓，"fill" 填充。</summary>
        public bool SetDraw(string mode)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.SetDraw(Handle, mode ?? "margin"));
        }

        /// <summary>设置线宽（set_line_width）。</summary>
        public bool SetLineWidth(double width)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.SetLineWidth(Handle, width));
        }

        /// <summary>设置字体（set_font），font 形如 "mono-Bold-18"；可用 query_font 查询。</summary>
        public bool SetFont(string font)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.SetFont(Handle, font ?? string.Empty));
        }

        #endregion

        #region ==================== 图元绘制 ====================

        /// <summary>画直线（disp_line），两端点像素坐标。</summary>
        public bool DispLine(double row1, double col1, double row2, double col2)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.DispLine(Handle, row1, col1, row2, col2));
        }

        /// <summary>画轴对齐矩形框（disp_rectangle1）。</summary>
        public bool DispRectangle1(double row1, double col1, double row2, double col2)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.DispRectangle1(Handle, row1, col1, row2, col2));
        }

        /// <summary>画圆（disp_circle）。</summary>
        public bool DispCircle(double row, double col, double radius)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.DispCircle(Handle, row, col, radius));
        }

        /// <summary>画十字标记（disp_cross），常用于标注定位点。</summary>
        /// <param name="row">中心行。</param>
        /// <param name="col">中心列。</param>
        /// <param name="size">十字臂长（像素）。</param>
        /// <param name="angle">十字旋转角（弧度）。</param>
        public bool DispCross(double row, double col, double size, double angle = 0)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.DispCross(Handle, row, col, size, angle));
        }

        /// <summary>画箭头（disp_arrow），从 (row1,col1) 指向 (row2,col2)。</summary>
        /// <param name="width">箭头内部夹角对应的显示宽度参数（像素，一般 0~80）。</param>
        public bool DispArrow(double row1, double col1, double row2, double col2, double width = 2)
        {
            if (!IsOpen) return false;
            return SafeRun(() => HOperatorSet.DispArrow(Handle, row1, col1, row2, col2, width));
        }

        #endregion

        #region ==================== 截图 ====================

        /// <summary>
        /// 把当前窗口内容截取为 HALCON 图像（dump_window_image），失败返回 null。
        /// 离屏缓冲窗口同样可截图。
        /// </summary>
        public HObject DumpToImage()
        {
            if (!IsOpen) return null;
            return SafeRun(delegate
            {
                HObject img;
                HOperatorSet.DumpWindowImage(out img, Handle);
                return img;
            }, (HObject)null);
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>释放窗口资源。</summary>
        protected override void DisposeUnmanaged()
        {
            Close();
        }

        #endregion
    }
}
