using HalconDotNet;
using System;

namespace HFromUI.HCamera.HHalcon
{
    /// <summary>
    /// HALCON 二维计量模型（2D Metrology）完整封装：在一个模型里挂接直线/圆/椭圆/旋转矩形
    /// 计量对象，apply_metrology_model 后一次性输出各对象的拟合几何参数与结果轮廓，
    /// 并支持模板对齐（align_metrology_model）、单个对象参数调整（measure_sigma/threshold、
    /// min_score 等）。句柄由实例持有，Dispose 时 clear_metrology_model。
    /// 典型用途：工件多尺寸综合测量（孔径、边距、角度、中心距）。
    /// </summary>
    public class HHalconMetrology : HHalconBase
    {
        #region ==================== 字段与属性 ====================

        /// <summary>计量模型句柄。</summary>
        public HTuple Handle { get; private set; } = new HTuple();

        /// <summary>当前模型中计量对象个数（add 成功次数）。</summary>
        public int ObjectCount { get; private set; }

        /// <summary>模型是否已创建。</summary>
        public bool IsCreated
        {
            get
            {
                try { return Handle != null && Handle.Length > 0; }
                catch { return false; }
            }
        }

        #endregion

        #region ==================== 创建/销毁 ====================

        /// <summary>创建空的 2D 计量模型（create_metrology_model）。</summary>
        public bool Create(int imageWidth = 0, int imageHeight = 0)
        {
            Clear();
            return SafeRun(() =>
            {
                HTuple h;
                HOperatorSet.CreateMetrologyModel(out h);
                Handle = h;
                ObjectCount = 0;
                if (imageWidth > 0 && imageHeight > 0)
                    HOperatorSet.SetMetrologyModelImageSize(Handle, imageWidth, imageHeight);
            });
        }

        /// <summary>销毁模型（clear_metrology_model）。</summary>
        public void Clear()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (IsCreated)
                    {
                        try { HOperatorSet.ClearMetrologyModel(Handle); } catch { }
                        Handle = new HTuple();
                    }
                    ObjectCount = 0;
                }
                catch { }
            }
        }

        #endregion

        #region ==================== 添加计量对象 ====================

        /// <summary>默认测量参数：每个对象边缘搜索弧半长/半宽、平滑 sigma、幅度阈值。</summary>
        private const double DefaultMeasureLength1 = 20;
        private const double DefaultMeasureLength2 = 5;
        private const double DefaultSigma = 1;
        private const double DefaultThreshold = 30;

        /// <summary>添加直线计量对象（add_metrology_object_line_measure），返回从 0 起的对象索引；失败 -1。</summary>
        /// <param name="rowBegin">起点行（近似位置）。</param>
        /// <param name="columnBegin">起点列。</param>
        /// <param name="rowEnd">终点行。</param>
        /// <param name="columnEnd">终点列。</param>
        /// <param name="measureLength1">边缘测量弧半长（沿线方向间距）。</param>
        /// <param name="measureLength2">边缘测量弧半宽（搜索带宽度一半）。</param>
        /// <param name="measureSigma">边缘提取平滑 sigma。</param>
        /// <param name="measureThreshold">边缘幅度阈值。</param>
        public int AddLine(double rowBegin, double columnBegin, double rowEnd, double columnEnd,
            double measureLength1 = DefaultMeasureLength1, double measureLength2 = DefaultMeasureLength2,
            double measureSigma = DefaultSigma, double measureThreshold = DefaultThreshold)
        {
            if (!IsCreated) return -1;
            int idx = -1;
            return SafeRun(() =>
            {
                HTuple outIdx;
                HOperatorSet.AddMetrologyObjectLineMeasure(Handle,
                    rowBegin, columnBegin, rowEnd, columnEnd,
                    measureLength1, measureLength2, measureSigma, measureThreshold,
                    new HTuple(), new HTuple(), out outIdx);
                idx = ObjectCount;
                ObjectCount++;
            }) ? idx : -1;
        }

        /// <summary>添加圆计量对象（add_metrology_object_circle_measure），返回对象索引；失败 -1。</summary>
        public int AddCircle(double row, double column, double radius,
            double measureLength1 = DefaultMeasureLength1, double measureLength2 = DefaultMeasureLength2,
            double measureSigma = DefaultSigma, double measureThreshold = DefaultThreshold)
        {
            if (!IsCreated) return -1;
            int idx = -1;
            return SafeRun(() =>
            {
                HTuple outIdx;
                HOperatorSet.AddMetrologyObjectCircleMeasure(Handle,
                    row, column, radius, measureLength1, measureLength2,
                    measureSigma, measureThreshold, new HTuple(), new HTuple(), out outIdx);
                idx = ObjectCount;
                ObjectCount++;
            }) ? idx : -1;
        }

        /// <summary>添加椭圆计量对象（add_metrology_object_ellipse_measure），返回对象索引；失败 -1。</summary>
        /// <param name="phi">长轴方向角（弧度）。</param>
        /// <param name="radius1">长半轴。</param>
        /// <param name="radius2">短半轴。</param>
        public int AddEllipse(double row, double column, double phi, double radius1, double radius2,
            double measureLength1 = DefaultMeasureLength1, double measureLength2 = DefaultMeasureLength2,
            double measureSigma = DefaultSigma, double measureThreshold = DefaultThreshold)
        {
            if (!IsCreated) return -1;
            int idx = -1;
            return SafeRun(() =>
            {
                HTuple outIdx;
                HOperatorSet.AddMetrologyObjectEllipseMeasure(Handle,
                    row, column, phi, radius1, radius2, measureLength1, measureLength2,
                    measureSigma, measureThreshold, new HTuple(), new HTuple(), out outIdx);
                idx = ObjectCount;
                ObjectCount++;
            }) ? idx : -1;
        }

        /// <summary>添加旋转矩形计量对象（add_metrology_object_rectangle2_measure），返回对象索引；失败 -1。</summary>
        public int AddRectangle2(double row, double column, double phi, double length1, double length2,
            double measureLength1 = DefaultMeasureLength1, double measureLength2 = DefaultMeasureLength2,
            double measureSigma = DefaultSigma, double measureThreshold = DefaultThreshold)
        {
            if (!IsCreated) return -1;
            int idx = -1;
            return SafeRun(() =>
            {
                HTuple outIdx;
                HOperatorSet.AddMetrologyObjectRectangle2Measure(Handle,
                    row, column, phi, length1, length2, measureLength1, measureLength2,
                    measureSigma, measureThreshold, new HTuple(), new HTuple(), out outIdx);
                idx = ObjectCount;
                ObjectCount++;
            }) ? idx : -1;
        }

        /// <summary>
        /// 设置计量对象参数（set_metrology_object_param），如 "min_score"、"measure_transition"、
        /// "measure_distance"、"num_instances"。index 传 -1 表示全部对象（HALCON 的 "all"）。
        /// </summary>
        public bool SetObjectParam(int index, string paramName, object value)
        {
            if (!IsCreated) return false;
            HTuple objIdx = index < 0 ? new HTuple("all") : new HTuple(index);
            return SafeRun(() =>
                HOperatorSet.SetMetrologyObjectParam(Handle,
                    objIdx,
                    paramName ?? string.Empty, HHalconEnv.ToTuple(value)));
        }

        /// <summary>整体位姿对齐（align_metrology_model）：测量前把模型平移 (row,column) 并旋转 angle（弧度）。</summary>
        public bool Align(double row, double column, double angle)
        {
            if (!IsCreated) return false;
            return SafeRun(() => HOperatorSet.AlignMetrologyModel(Handle, row, column, angle));
        }

        #endregion

        #region ==================== 执行与取结果 ====================

        /// <summary>对图像执行全部计量对象的边缘提取与拟合（apply_metrology_model）。</summary>
        /// <param name="image">待测量图像。</param>
        public bool Apply(HObject image)
        {
            if (!IsCreated || image == null) return false;
            return SafeRun(() => HOperatorSet.ApplyMetrologyModel(image, Handle));
        }

        /// <summary>
        /// 取指定计量对象的几何参数（get_metrology_object_result，result_type）。
        /// 返回顺序：line=[起点行,起点列,终点行,终点列]；circle=[行,列,半径]；
        /// ellipse=[行,列,角度,长半轴,短半轴]；rectangle2=[行,列,角度,半长,半宽]。
        /// </summary>
        /// <param name="index">对象索引（<see cref="AddLine"/> 等返回值），-1 取全部对象的拼接结果。</param>
        /// <param name="instance">实例序号，"all" 取全部实例，这里默认第 1 个。</param>
        public double[] GetResult(int index, int instance = 1)
        {
            if (!IsCreated) return new double[0];
            double[] res = new double[0];
            HTuple objIdx = index < 0 ? new HTuple("all") : new HTuple(index + 1);
            SafeRun(() =>
            {
                HTuple p;
                HOperatorSet.GetMetrologyObjectResult(Handle,
                    objIdx,
                    (HTuple)instance, "result_type", "all_param", out p);
                res = p.DArr;
            });
            return res;
        }

        /// <summary>取圆计量结果（行,列,半径）；index 为对象索引。</summary>
        public bool GetCircle(int index, out double row, out double column, out double radius)
        {
            double[] r = GetResult(index);
            row = column = radius = 0;
            if (r.Length < 3) return false;
            row = r[0];
            column = r[1];
            radius = r[2];
            return true;
        }

        /// <summary>取直线计量结果（起点行,列,终点行,列）。</summary>
        public bool GetLine(int index, out double rowBegin, out double colBegin,
            out double rowEnd, out double colEnd)
        {
            double[] r = GetResult(index);
            rowBegin = colBegin = rowEnd = colEnd = 0;
            if (r.Length < 4) return false;
            rowBegin = r[0];
            colBegin = r[1];
            rowEnd = r[2];
            colEnd = r[3];
            return true;
        }

        /// <summary>取旋转矩形计量结果（中心行,列,角度,半长,半宽）。</summary>
        public bool GetRectangle2(int index, out double row, out double column,
            out double phi, out double length1, out double length2)
        {
            double[] r = GetResult(index);
            row = column = phi = length1 = length2 = 0;
            if (r.Length < 5) return false;
            row = r[0];
            column = r[1];
            phi = r[2];
            length1 = r[3];
            length2 = r[4];
            return true;
        }

        /// <summary>
        /// 生成全部计量结果的可视化 XLD 轮廓（get_metrology_object_result_contour），
        /// 可直接 disp_obj 叠加到原图；返回 HObject 由调用方 Dispose。
        /// </summary>
        public HObject GetResultContours(double resolution = 1.5)
        {
            if (!IsCreated) return null;
            return SafeRun(delegate
            {
                HObject contour;
                HOperatorSet.GetMetrologyObjectResultContour(out contour, Handle, "all", "all", resolution);
                return contour;
            }, (HObject)null);
        }

        #endregion

        #region ==================== 释放 ====================

        /// <summary>销毁模型句柄。</summary>
        protected override void DisposeUnmanaged()
        {
            Clear();
        }

        #endregion
    }
}
